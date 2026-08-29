using System;
using System.Collections.Generic;
using System.Text;

namespace DotGram.Grammar;

/// <summary>
/// Several grammars joined into the one text they are compiled as.
/// </summary>
/// <remarks>
/// <para>
/// The text and the map come out of one call, because they are one fact said two ways and
/// working them out separately is how they come to disagree.
/// </para>
/// <para>
/// **The author's own grammar goes first, and untouched.** Its positions are then the ones
/// they always were, so every diagnostic in the text somebody is actually editing lands
/// exactly where it landed before any of this existed, and only what follows is translated.
/// Putting the included grammars first would shift the one text an author reads to serve
/// the order of a file nobody reads.
/// </para>
/// <para>
/// **What is included is wrapped in a namespace and not indented.** The wrapper is what
/// hides its rules until a `using` asks for them (§5.1) and what keeps its own `trivia`
/// its own (§4.5). Indenting inside the wrapper would read better and is refused all the
/// same: it would shift every position on every line, and the translation this file exists
/// for is only exact while a segment's bytes are the segment's bytes.
/// </para>
/// <para>
/// The wrapper's own characters belong to no grammar. They are left out of the map rather
/// than attributed to one, so a position landing in them is answered "nowhere" — which is
/// what <see cref="ILineMap"/> asks for over a guess.
/// </para>
/// </remarks>
public static class GrammarSplice
{
	/// <summary>One grammar going into the joined text.</summary>
	/// <param name="Text">Its own text, which is copied in unchanged.</param>
	/// <param name="Name">
	/// The namespace to wrap it in, or null for the grammar doing the including — which is
	/// wrapped in nothing, because its rules are the ones being declared.
	/// </param>
	/// <param name="Map">Where a position inside it belongs, in its own offsets.</param>
	public readonly record struct Part(string Text, string? Name, ILineMap? Map);

	/// <summary>The joined text, and the map that takes a position in it back apart.</summary>
	public static (string Text, SplicedLineMap Map) Join(Part own, IReadOnlyList<Part> included)
	{
		if (own.Text is null)
			throw new ArgumentNullException(nameof(own));

		if (included is null)
			throw new ArgumentNullException(nameof(included));

		var text     = new StringBuilder(own.Text);
		var segments = new List<SplicedLineMap.Segment> { new(0, own.Text.Length, own.Map) };

		// Only where something follows. Appended, so nothing above it moves — a grammar
		// whose last line has no terminator would otherwise run into the wrapper. With
		// nothing to wrap there is nothing to run into, and joining one grammar has to be
		// that grammar to the character: one more makes the end of the text a different
		// place, and a rule that failed at the end of the input is reported there.
		if (included.Count > 0 && own.Text.Length > 0 && own.Text[own.Text.Length - 1] != '\n')
			text.Append('\n');

		foreach (var part in included)
		{
			if (part.Name is null)
				throw new ArgumentException(
					"An included grammar is wrapped in a namespace, so it has to have a name.",
					nameof(included));

			text.Append('\n').Append("namespace ").Append(part.Name).Append('\n').Append("{\n");

			segments.Add(new SplicedLineMap.Segment(text.Length, part.Text.Length, part.Map));

			text.Append(part.Text);

			if (part.Text.Length > 0 && part.Text[part.Text.Length - 1] != '\n')
				text.Append('\n');

			text.Append("}\n");
		}

		return (text.ToString(), new SplicedLineMap(segments));
	}
}
