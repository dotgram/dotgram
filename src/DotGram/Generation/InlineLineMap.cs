using System;

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
/// for instead, exactly as a diagnostic's position is (<see cref="Report"/>): take the
/// grammar's line, find it in the spelling, and where it occurs once the offset is
/// known exactly. Found twice or not at all — a line repeated, or one whose escapes were
/// written differently from what they decode to — there is no answer, and the C# error
/// goes on landing in the generated file as it did before.
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
		var to   = grammar.IndexOf('\n', from);
		var text = grammar.Substring(from, (to < 0 ? grammar.Length : to) - from).TrimEnd('\r');

		if (text.Length == 0)
			return false;

		var at = spelling.IndexOf(text, StringComparison.Ordinal);

		if (at < 0 || spelling.IndexOf(text, at + 1, StringComparison.Ordinal) >= 0)
			return false;

		var placed = tree.GetText().Lines.GetLinePosition(spellingAt + at + (position - from));

		line   = placed.Line + 1;
		column = placed.Character + 1;

		return true;
	}
}
