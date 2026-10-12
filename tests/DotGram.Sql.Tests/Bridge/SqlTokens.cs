using System;
using System.Collections.Generic;

namespace DotGram.Sql.Tests;

/// <summary>
/// T-SQL text cut into tokens, as far as the bridge and its oracle need it: a name in brackets or quotes and
/// a string are one token each and keep their text, a word is one token, a number is one, and every other
/// character that is not blank is one. Comments and blanks are not tokens.
/// </summary>
static class SqlTokens
{
	public enum Kind { Word, Delimited, String, Number, Punctuation }

	public readonly record struct Token(Kind Kind, string Text);

	public static List<Token> Of(string text)
	{
		var tokens = new List<Token>();
		var at     = 0;

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
				var end = text.IndexOf("*/", at + 2, StringComparison.Ordinal);

				at = end < 0 ? text.Length : end + 2;

				continue;
			}

			var start = at;

			if (c is '[' or '"')
			{
				at = Closed(text, at, c == '[' ? ']' : '"');
				tokens.Add(new Token(Kind.Delimited, text[start..at]));

				continue;
			}

			// A string, and a national one: the N is the string's.
			if (c == '\'' || c is 'N' or 'n' && At(text, at + 1) == '\'')
			{
				at = Closed(text, c == '\'' ? at : at + 1, '\'');
				tokens.Add(new Token(Kind.String, text[start..at]));

				continue;
			}

			if (char.IsDigit(c) || c == '.' && char.IsDigit(At(text, at + 1)))
			{
				while (at < text.Length && (char.IsLetterOrDigit(text[at]) || text[at] == '.'))
					at++;

				tokens.Add(new Token(Kind.Number, text[start..at]));

				continue;
			}

			if (char.IsLetter(c) || c is '_' or '@' or '#' or '$')
			{
				while (at < text.Length && (char.IsLetterOrDigit(text[at]) || text[at] is '_' or '@' or '#' or '$'))
					at++;

				tokens.Add(new Token(Kind.Word, text[start..at]));

				continue;
			}

			tokens.Add(new Token(Kind.Punctuation, c.ToString()));
			at++;
		}

		return tokens;
	}

	static char At(string text, int at)
	{
		return at < text.Length ? text[at] : '\0';
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
