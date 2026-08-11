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

	public static readonly DiagnosticDescriptor AmbiguousSupportTypes = new(
		id:                 "GRAM0001",
		title:              "More than one referenced assembly publishes the .Gram support types",
		messageFormat:      "Referenced assemblies {0} all publish the .Gram support types; remove [assembly: GramRuntime] from all but one",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description:        "Which assembly's types a grammar binds to must be unambiguous, not decided silently by reference order.");

	/// <summary>
	/// Wraps a grammar diagnostic for Roslyn. The grammar half reports positionally and
	/// knows nothing about <see cref="Diagnostic"/>; this is where its messages become
	/// something an IDE can show at the right place in the .gram file.
	/// </summary>
	public static Diagnostic ToRoslyn(GramDiagnostic diagnostic, string filePath)
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

		var location = Location.Create(
			filePath,
			new TextSpan(diagnostic.Position, diagnostic.Length),
			new LinePositionSpan());

		return Diagnostic.Create(descriptor, location, diagnostic.Message);
	}

	static readonly Dictionary<string, DiagnosticDescriptor> _descriptors = [];
}
