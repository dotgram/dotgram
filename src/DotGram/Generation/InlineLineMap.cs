using System;
using System.Collections.Generic;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.Generation;

/// <summary>
/// Where a position in an inline grammar is, as a place in the C# file holding it (§7.6).
/// </summary>
/// <remarks>
/// <para>
/// A grammar written into a <c>[Gram("…")]</c> attribute has no file of its own, so a
/// <c>#line</c> over the C# it hands across has to point at the C# file it is written in
/// — at the line of the string literal that the grammar's own line sits on.
/// </para>
/// <para>
/// Which line that is cannot be computed, because what the compiler was given is the
/// decoded value of the literal and what the author reads is its spelling: escapes,
/// quoting and a raw literal's indentation all sit between the two. So it is searched
/// for instead, exactly as a diagnostic's position is (<see cref="Report"/>).
/// </para>
/// <para>
/// <b>In order, and that is what makes a repeated line answerable.</b> The lines of the
/// decoded value stand in the spelling in the order they were written, so each is looked
/// for from where the one before it was found. A line written twice — and a grammar of
/// any size has them, <c>=&gt; @(ExpressionParser.Listed(first, rest))</c> five times
/// over in the expression language — is then found at its own occurrence rather than
/// refused for having a twin. Searching the whole spelling instead cost eleven of that
/// grammar's constructions their directive, <c>While</c> and <c>For</c> among them.
/// </para>
/// <para>
/// Where a line is <em>not</em> found where it should be — one whose escapes were
/// written differently from what they decode to — the place after it is no longer
/// known, and a repeated line beyond it could bind to the wrong occurrence. So the
/// order is given up at that point and what is left of the file is asked the older
/// question: a line that occurs exactly once has an answer, and one that does not has
/// none, and the C# error goes on landing in the generated file as it did before.
/// </para>
/// <para>
/// Never a guess. A directive pointing at the wrong line is worse than none at all: the
/// author reads a place that has nothing wrong with it and concludes the message is
/// nonsense.
/// </para>
/// </remarks>
sealed class InlineLineMap(string grammar, string spelling, int spellingAt, SyntaxTree tree) : ILineMap
{
	public bool TryMap(int position, out string file, out int line, out int column)
	{
		file   = tree.FilePath;
		line   = 0;
		column = 0;

		if (position < 0 || position >= grammar.Length)
			return false;

		var from = grammar.LastIndexOf('\n', position) + 1;

		if (!Placed().TryGetValue(from, out var at) || at < 0)
			return false;

		var placed = tree.GetText().Lines.GetLinePosition(spellingAt + at + (position - from));

		line   = placed.Line + 1;
		column = placed.Character + 1;

		return true;
	}

	/// <summary>Where every line of the grammar begins in the spelling, or -1 for none.</summary>
	Dictionary<int, int> Placed()
	{
		if (_placed is not null)
			return _placed;

		_placed = [];

		var cursor  = 0;
		var ordered = true;

		for (var from = 0; from < grammar.Length;)
		{
			var to   = grammar.IndexOf('\n', from);
			var end  = to < 0 ? grammar.Length : to;
			var text = grammar.Substring(from, end - from).TrimEnd('\r');

			if (text.Length > 0)
			{
				var at = ordered ? spelling.IndexOf(text, cursor, StringComparison.Ordinal) : -1;

				if (at < 0)
				{
					// Not where it should have been, so where anything after it is is no longer
					// known. What is left of the file is asked the older question instead.
					ordered = false;
					at      = spelling.IndexOf(text, StringComparison.Ordinal);

					if (at >= 0 && spelling.IndexOf(text, at + 1, StringComparison.Ordinal) >= 0)
						at = -1;
				}
				else
				{
					cursor = at + text.Length;
				}

				_placed[from] = at;
			}

			from = end + 1;
		}

		return _placed;
	}

	Dictionary<int, int>? _placed;
}
