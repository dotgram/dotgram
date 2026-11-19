using System;
using System.Collections.Generic;

namespace DotGram.Grammar.Parsing;

/// <summary>The lexer as a caller that holds a C# scanner, or none, asks for it.</summary>
static partial class GramLexer
{
	/// <param name="scanner">
	/// Finds where an inline <c>@(...)</c> ends. Null means the grammar may not use
	/// one — reported as a diagnostic rather than a crash, since a caller that only
	/// exercises the grammar side legitimately has no C# lexer to offer.
	/// </param>
	public static TokenList Tokenize(string text, ICSharpScanner? scanner = null)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		return Lex(text, CSharpLanguage.Reading(scanner));
	}

	/// <summary>
	/// Reads a text whose tail from <paramref name="splicedAt"/> on was spliced in by the
	/// compiler — the standard library — and is read with the scanner that comes with it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The author's text keeps the author's scanner, and keeps being refused a <c>@(...)</c>
	/// where none was supplied (GRAM1007): that contract is about their text. The library's
	/// C# was written to be read by <see cref="StandardLibrary.Scanner"/>, and is, whatever
	/// the caller brought.
	/// </para>
	/// <para>
	/// The two parts are read as two texts and joined, so that nothing in the author's part — a
	/// comment, a literal, an escape, an inline expression — can run on into the library and be
	/// closed by what it finds there. An unterminated construct ends, and is reported, at the
	/// end of the author's own text. The tokens are one list with an end-of-file token between
	/// the parts, which the parser stops at in the same way.
	/// </para>
	/// </remarks>
	public static TokenList Tokenize(string text, ICSharpScanner? scanner, int splicedAt, ICSharpScanner? spliced)
	{
		return Tokenize(text, scanner, null, splicedAt, spliced);
	}

	/// <summary>
	/// As above, for a text that also has grammars included by the host after the author's own:
	/// <paramref name="cuts"/> are the offsets at which each text of its own begins (the end of the
	/// author's, and of each included body with its wrapper), and every one is read apart from the
	/// rest.
	/// </summary>
	public static TokenList Tokenize(
		string text, ICSharpScanner? scanner, IReadOnlyList<int>? cuts, int splicedAt, ICSharpScanner? spliced)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		return TokenizeIn(CSharpLanguage.Reading(scanner), text, cuts, splicedAt, CSharpLanguage.Reading(spliced));
	}
}
