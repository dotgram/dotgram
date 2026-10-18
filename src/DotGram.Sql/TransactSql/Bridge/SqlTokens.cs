using System;
using System.Collections.Generic;

namespace DotGram.Sql.TransactSql;

/// <summary>
/// T-SQL text cut into tokens, as far as the bridge and its oracle need it: a name in brackets or quotes and
/// a string are one token each and keep their text, a word is one token, a number is one, and every other
/// character that is not blank is one. Comments and blanks are not tokens; a block comment nests, as T-SQL's
/// does.
/// </summary>
static class SqlTokens
{
	public enum Kind { Word, Delimited, String, Number, Punctuation }

	public readonly record struct Token(Kind Kind, string Text);

	public static List<Token> Of(string text)
	{
		return [.. Scan(text, 0)];
	}

	/// <summary>
	/// The first token of a text from a position on, or null where there is none: what is written after a node,
	/// comments and blanks skipped.
	/// </summary>
	public static Token? First(string text, int from)
	{
		foreach (var token in Scan(text, from))
			return token;

		return null;
	}

	static IEnumerable<Token> Scan(string text, int at)
	{
		while (at < text.Length)
		{
			var c = text[at];

			if (char.IsWhiteSpace(c))
			{
				at++;

				continue;
			}

			if (c == '-' && At(text, at + 1) == '-')
			{
				while (at < text.Length && text[at] != '\n')
					at++;

				continue;
			}

			if (c == '/' && At(text, at + 1) == '*')
			{
				at = Commented(text, at);

				continue;
			}

			var start = at;

			if (c is '[' or '"')
			{
				at = Closed(text, at, c == '[' ? ']' : '"');
				yield return new Token(Kind.Delimited, text[start..at]);

				continue;
			}

			// A string, and a national one: the N is the string's.
			if (c == '\'' || c is 'N' or 'n' && At(text, at + 1) == '\'')
			{
				at = Closed(text, c == '\'' ? at : at + 1, '\'');
				yield return new Token(Kind.String, text[start..at]);

				continue;
			}

			if (char.IsDigit(c) || c == '.' && char.IsDigit(At(text, at + 1)))
			{
				while (at < text.Length && (char.IsLetterOrDigit(text[at]) || text[at] == '.'))
					at++;

				yield return new Token(Kind.Number, text[start..at]);

				continue;
			}

			if (char.IsLetter(c) || c is '_' or '@' or '#' or '$')
			{
				while (at < text.Length && (char.IsLetterOrDigit(text[at]) || text[at] is '_' or '@' or '#' or '$'))
					at++;

				yield return new Token(Kind.Word, text[start..at]);

				continue;
			}

			yield return new Token(Kind.Punctuation, c.ToString());
			at++;
		}

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
