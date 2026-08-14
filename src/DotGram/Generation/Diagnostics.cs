using System;
using System.Collections.Generic;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.Generation;

/// <summary>Diagnostics the shell reports, and conversion of the grammar half's own.</summary>
static class Diagnostics
{
	const string Category = "DotGram";

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
	/// Wraps a grammar diagnostic for Roslyn. The grammar half reports positionally and
	/// knows nothing about <see cref="Diagnostic"/>; this is where its messages become
	/// something an IDE can show at the right place in the .gram file.
	/// </summary>
	/// <param name="filePath">The .gram file, or null when the grammar was written inline.</param>
	/// <param name="grammarText">The grammar, to turn an offset into a line and column.</param>
	/// <param name="fallback">Where to point when there is no file — the <c>[Gram]</c> attribute.</param>
	public static Diagnostic ToRoslyn(GramDiagnostic diagnostic, string? filePath, string grammarText, Location? fallback)
	{
		if (!_descriptors.TryGetValue(diagnostic.Id, out var descriptor))
		{
			descriptor = new DiagnosticDescriptor(
				id:                 diagnostic.Id,
				title:              diagnostic.Id,
				messageFormat:      "{0}",
				category:           Category,
				defaultSeverity:    diagnostic.Severity == GramSeverity.Error
					? DiagnosticSeverity.Error
					: DiagnosticSeverity.Warning,
				isEnabledByDefault: true);

			_descriptors[diagnostic.Id] = descriptor;
		}

		var span = new TextSpan(diagnostic.Position, diagnostic.Length);

		// An inline grammar has no file to point into, so the message lands on the
		// attribute that carries it — still the right place to look, if not the right
		// character.
		var location = filePath is null
			? fallback ?? Location.None
			: Location.Create(filePath, span, LinesOf(grammarText, span));

		return Diagnostic.Create(descriptor, location, diagnostic.Message);
	}

	/// <summary>
	/// Turns an offset span into lines and columns.
	/// </summary>
	/// <remarks>
	/// Roslyn does not read the additional file to work this out — it believes whatever
	/// it is told. Told nothing, it points every grammar message at line 1, which is the
	/// difference between a diagnostic and a diagnostic that helps.
	/// </remarks>
	static LinePositionSpan LinesOf(string text, TextSpan span)
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

	static readonly Dictionary<string, DiagnosticDescriptor> _descriptors = [];
}
