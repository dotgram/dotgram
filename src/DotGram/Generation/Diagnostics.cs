using System;
using System.Collections.Concurrent;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.Generation;

/// <summary>Diagnostics the shell reports, and conversion of the grammar half's own.</summary>
static class Diagnostics
{
	const string Category = "DotGram";

	/// <summary>
	/// The final safety net for a defect reached through grammar input. Kept out of the
	/// public language specification: this is an internal failure, not language behavior.
	/// </summary>
	public static readonly DiagnosticDescriptor InternalFailure = new(
		id:                 "GRAM0001",
		title:              "The .Gram generator encountered an internal error",
		messageFormat:      "The generator failed during {0}: {1}: {2}",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public static readonly DiagnosticDescriptor HostNotPartial = new(
		id:                 "GRAM0002",
		title:              "A class hosting a grammar must be partial",
		messageFormat:      "'{0}' is marked [Gram] but is not partial; the generated parser has nowhere to go",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description:        "A nested host needs every enclosing class to be partial as well.");

	public static readonly DiagnosticDescriptor GrammarFileNotFound = new(
		id:                 "GRAM0003",
		title:              "No grammar file for a [Gram] class",
		messageFormat:      "No additional file '{0}' for '{1}'; add <AdditionalFiles Include=\"{0}\" /> to the project",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description:        "A .gram file is only visible to the generator when the project lists it as an additional file.");

	public static readonly DiagnosticDescriptor AmbiguousGrammarFile = new(
		id:                 "GRAM0004",
		title:              "More than one grammar file matches",
		messageFormat:      "'{0}' matches several additional files ({1}); give [Gram] a path that names one of them",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description:        "Which file a grammar comes from must be unambiguous, not decided silently by order.");

	/// <summary>
	/// The descriptor for an id, made once and kept.
	/// </summary>
	/// <remarks>
	/// The grammar half reports positionally and knows nothing about
	/// <see cref="Diagnostic"/>; its ids arrive here and become rules an IDE can show.
	/// Kept rather than remade because two descriptors for one id are two rules as far as
	/// Roslyn is concerned.
	/// </remarks>
	public static DiagnosticDescriptor DescriptorFor(
		string id, string title, string messageFormat, DiagnosticSeverity severity) =>
		_descriptors.GetOrAdd(
			id,
			_ => new DiagnosticDescriptor(
				id:                 id,
				title:              title,
				messageFormat:      messageFormat,
				category:           Category,
				defaultSeverity:    severity,
				isEnabledByDefault: true));

	/// <summary>
	/// Turns an offset span into lines and columns.
	/// </summary>
	/// <remarks>
	/// Roslyn does not read the additional file to work this out — it believes whatever
	/// it is told. Told nothing, it points every grammar message at line 1, which is the
	/// difference between a diagnostic and a diagnostic that helps.
	/// </remarks>
	public static LinePositionSpan LinesOf(string text, TextSpan span)
	{
		var line   = 0;
		var start  = 0;
		var result = default(LinePosition);

		for (var i = 0; i <= text.Length; i++)
		{
			if (i == span.Start)
				result = new LinePosition(line, i - start);

			if (i == span.End)
				return new LinePositionSpan(result, new LinePosition(line, i - start));

			if (i < text.Length && text[i] == '\n')
			{
				line++;
				start = i + 1;
			}
		}

		return new LinePositionSpan(result, result);
	}

	static readonly ConcurrentDictionary<string, DiagnosticDescriptor> _descriptors = [];
}
