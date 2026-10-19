using System;
using System.Collections.Generic;

namespace DotGram.Sql.TransactSql;

/// <summary>
/// T-SQL text cut into tokens, as far as the builders need it: a name in brackets or quotes and
/// a string are one token each and keep their text, a word is one token, a number is one, and every other
/// character that is not blank is one. Comments and blanks are not tokens; a block comment nests, as T-SQL's
/// does, and a blank is what the grammar's spacing is (<see cref="TransactSqlParser.Spacing"/>).
/// </summary>
static class SqlTokens
{
	public enum Kind { Word, Delimited, String, Number, Punctuation }

	public readonly record struct Token(Kind Kind, string Text);

	public static List<Token> Of(string text)
	{
		var tokens = new List<Token>();

		for (var at = 0; Next(text, ref at, out var kind, out var start);)
			tokens.Add(new Token(kind, text[start..at]));

		return tokens;
	}

	/// <summary>
	/// The tokens of a text, each with where it starts in the text: where the scan found it, so that a token is never
	/// placed at the same text inside a comment or a string before it.
	/// </summary>
	public static List<(Token Token, int At)> Located(string text)
	{
		var tokens = new List<(Token, int)>();

		for (var at = 0; Next(text, ref at, out var kind, out var start);)
			tokens.Add((new Token(kind, text[start..at]), start));

		return tokens;
	}

	/// <summary>
	/// Where the first token spelled as given stands in a text, from a position on — a word in any case, anything else
	/// as written — or -1: a bracket or a word found where the scan finds it, never inside a comment, a string or a
	/// delimited name.
	/// </summary>
	public static int Find(string text, string spelled, int from = 0)
	{
		for (var at = 0; Next(text, ref at, out var kind, out var start);)
			if (start >= from && Spelled(kind, text.AsSpan(start, at - start), spelled))
				return start;

		return -1;
	}

	/// <summary>Where the last token spelled as given stands in a text, or -1.</summary>
	public static int FindLast(string text, string spelled)
	{
		var last = -1;

		for (var at = 0; Next(text, ref at, out var kind, out var start);)
			if (Spelled(kind, text.AsSpan(start, at - start), spelled))
				last = start;

		return last;
	}

	static bool Spelled(Kind kind, ReadOnlySpan<char> token, string spelled)
	{
		return kind == Kind.Word
			? token.Equals(spelled, StringComparison.OrdinalIgnoreCase)
			: token.SequenceEqual(spelled);
	}

	/// <summary>
	/// The first token of a text from a position on, or null where there is none: what is written after a node,
	/// comments and blanks skipped.
	/// </summary>
	public static Token? First(string text, int from)
	{
		var at = from;

		return Next(text, ref at, out var kind, out var start) ? new Token(kind, text[start..at]) : null;
	}

	/// <summary>
	/// The next token from a position on: its kind and where it starts, with the position moved past its end; false where
	/// only blanks and comments are left. Nothing is allocated: a caller that wants the token's text cuts it.
	/// </summary>
	static bool Next(string text, ref int at, out Kind kind, out int start)
	{
		while (at < text.Length)
		{
			var c = text[at];

			if (TransactSqlParser.Spacing(c))
			{
				at++;

				continue;
			}

			// A line comment runs to a line feed or a carriage return, as the grammar's does (`Std.LineComment`).
			if (c == '-' && At(text, at + 1) == '-')
			{
				while (at < text.Length && text[at] is not ('\n' or '\r'))
					at++;

				continue;
			}

			if (c == '/' && At(text, at + 1) == '*')
			{
				at = Commented(text, at);

				continue;
			}

			start = at;

			if (c is '[' or '"')
			{
				at   = Closed(text, at, c == '[' ? ']' : '"');
				kind = Kind.Delimited;

				return true;
			}

			// A string, and a national one: the N is the string's.
			if (c == '\'' || c is 'N' or 'n' && At(text, at + 1) == '\'')
			{
				at   = Closed(text, c == '\'' ? at : at + 1, '\'');
				kind = Kind.String;

				return true;
			}

			if (char.IsDigit(c) || c == '.' && char.IsDigit(At(text, at + 1)))
			{
				while (at < text.Length && (char.IsLetterOrDigit(text[at]) || text[at] == '.'))
					at++;

				kind = Kind.Number;

				return true;
			}

			if (char.IsLetter(c) || c is '_' or '@' or '#' or '$')
			{
				while (at < text.Length && (char.IsLetterOrDigit(text[at]) || text[at] is '_' or '@' or '#' or '$'))
					at++;

				kind = Kind.Word;

				return true;
			}

			at++;
			kind = Kind.Punctuation;

			return true;
		}

		kind  = default;
		start = at;

		return false;
	}

	static char At(string text, int at)
	{
		return at < text.Length ? text[at] : '\0';
	}

	/// <summary>The end of a block comment opened at <paramref name="at"/>, the comments inside it counted.</summary>
	static int Commented(string text, int at)
	{
		var depth = 0;

		while (at < text.Length)
		{
			if (text[at] == '/' && At(text, at + 1) == '*')
			{
				depth++;
				at += 2;
			}
			else if (text[at] == '*' && At(text, at + 1) == '/')
			{
				at += 2;

				if (--depth == 0)
					return at;
			}
			else
				at++;
		}

		return text.Length;
	}

	/// <summary>The end of a token opened at <paramref name="at"/> and closed by <paramref name="close"/>, a doubled close inside it.</summary>
	static int Closed(string text, int at, char close)
	{
		for (at++; at < text.Length; at++)
		{
			if (text[at] != close)
				continue;

			if (At(text, at + 1) == close)
			{
				at++;

				continue;
			}

			return at + 1;
		}

		return text.Length;
	}
}
