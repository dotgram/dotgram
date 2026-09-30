using System;
using System.Collections.Generic;

using DotGram.Grammar;

namespace DotGram.VisualStudio;

/// <summary>What a squiggle over a grammar says, and where it is drawn.</summary>
/// <remarks>
/// The squiggle and Quick Info both read it. Quick Info over a grammar shows DotGram's own
/// content and drops everything else (<see cref="GramToolTipPresenter"/>), and the text Visual
/// Studio shows for an error tag is part of that everything else: a squiggle under a word that
/// has Quick Info of its own would otherwise explain nothing. So the message is said again where
/// DotGram's content is built, over exactly the span the tagger draws.
/// </remarks>
static class GramDiagnosticText
{
	public static string Format(GramDiagnostic diagnostic)
	{
		return $"{diagnostic.Id}: {diagnostic.Message}";
	}

	/// <summary>The span a squiggle is drawn over: inside the text, and never empty unless the text is.</summary>
	public static (int Position, int Length) Span(int position, int length, int textLength)
	{
		position = Math.Max(0, Math.Min(position, textLength));
		length   = Math.Max(0, Math.Min(length, textLength - position));

		if (length == 0 && textLength > 0)
		{
			if (position == textLength)
				position--;

			length = 1;
		}

		return (position, length);
	}

	/// <summary>The messages of the squiggles drawn under <paramref name="point"/>, in the order given.</summary>
	public static IReadOnlyList<string> At(
		IEnumerable<(int Position, int Length, GramDiagnostic Diagnostic)> diagnostics,
		int point,
		int textLength)
	{
		var result = new List<string>();

		foreach (var (position, length, diagnostic) in diagnostics)
		{
			var span = Span(position, length, textLength);

			if (span.Position <= point && point < span.Position + span.Length)
				result.Add(Format(diagnostic));
		}

		return result;
	}
}
