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

	/// <summary>
	/// The kind of a text's one token, or null where it has none or more than one.
	/// </summary>
	public static Kind? Lone(string text)
	{
		var at = 0;

		if (!Next(text, ref at, out var kind, out _))
			return null;

		return Next(text, ref at, out _, out _) ? null : kind;
	}

	/// <summary>
	/// Whether the last token of a text is the upper-case word given, in any case (<see cref="Raises"/>).
	/// </summary>
	public static bool LastIs(string text, string word)
	{
		var last = false;

		for (var at = 0; Next(text, ref at, out var kind, out var start);)
			last = kind == Kind.Word && Raises(text.AsSpan(start, at - start), word);

		return last;
	}

	static bool Spelled(Kind kind, ReadOnlySpan<char> token, string spelled)
	{
		return kind == Kind.Word
			? token.Equals(spelled, StringComparison.OrdinalIgnoreCase)
			: token.SequenceEqual(spelled);
	}

	/// <summary>
	/// Whether a word AS stands in a text outside round brackets and is not followed by the word OF (the AS of
	/// <c>FOR SYSTEM_TIME AS OF</c> is not an alias's), a word in any case and never a name in brackets or quotes: what
	/// <c>Words</c> would show of the text, found without making a list of its tokens or cutting the text out of the one it
	/// stands in.
	/// </summary>
	public static bool AliasAs(ReadOnlySpan<char> text)
	{
		var depth   = 0;
		var pending = false;

		for (var at = 0; Next(text, ref at, out var kind, out var start);)
		{
			var token = text.Slice(start, at - start);

			// An AS outside the brackets is an alias's unless OF is the next word.
			if (pending)
			{
				if (kind != Kind.Word || !Upper(token, 'O', 'F'))
					return true;

				pending = false;
			}

			if (kind == Kind.Punctuation)
				depth += token[0] switch { '(' => 1, ')' => -1, _ => 0 };
			else if (kind == Kind.Word && depth == 0 && Upper(token, 'A', 'S'))
				pending = true;
		}

		return pending;
	}

	/// <summary>Whether a token of two characters is the two given upper-case ones when it is upper-cased, as the words of a text are.</summary>
	static bool Upper(ReadOnlySpan<char> token, char first, char second)
	{
		return token.Length == 2 && char.ToUpperInvariant(token[0]) == first && char.ToUpperInvariant(token[1]) == second;
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
	/// Whether the first token of a text from a position on is the upper-case word given, in any case (<see cref="Raises"/>):
	/// what <see cref="First"/> would say of it, without cutting the token out.
	/// </summary>
	public static bool FirstIs(string text, int from, string word)
	{
		var at = from;

		return Next(text, ref at, out var kind, out var start) && kind == Kind.Word && Raises(text.AsSpan(start, at - start), word);
	}

	/// <summary>
	/// Whether a word upper-cased is the upper-case word given, as <see cref="Words"/> would compare it: character by
	/// character, as the invariant culture raises them — which is not quite ignoring case, since it raises the long s to S.
	/// </summary>
	internal static bool Raises(ReadOnlySpan<char> token, string word)
	{
		if (token.Length != word.Length)
			return false;

		for (var at = 0; at < token.Length; at++)
		{
			if (char.ToUpperInvariant(token[at]) != word[at])
				return false;
		}

		return true;
	}

	/// <summary>
	/// The tokens of a text as words, at most <paramref name="most"/> of them from its start: a word upper-cased, and a
	/// name in brackets or quotes, a string or a number as written — so that `"as"` is never the word AS. Each is made
	/// once, from the text, with no list of tokens in between.
	/// </summary>
	public static List<string> Words(string text, int most = int.MaxValue)
	{
		var words = new List<string>();

		for (var at = 0; words.Count < most && Next(text, ref at, out var kind, out var start);)
			words.Add(kind == Kind.Word ? Raised(text, start, at - start) : text.Substring(start, at - start));

		return words;
	}

	/// <summary>
	/// A word of a text upper-cased, as <see cref="string.ToUpperInvariant"/> raises it: cut once where it is ASCII, and
	/// the text itself where it is all of it and already upper-case.
	/// </summary>
	static string Raised(string text, int start, int length)
	{
		var lower = false;

		for (var at = start; at < start + length; at++)
		{
			var c = text[at];

			if (c >= '\u0080')
				return text.Substring(start, length).ToUpperInvariant();

			if (c is >= 'a' and <= 'z')
				lower = true;
		}

		if (!lower)
			return length == text.Length ? text : text.Substring(start, length);

		Span<char> raised = length <= 128 ? stackalloc char[length] : new char[length];

		for (var at = 0; at < length; at++)
		{
			var c = text[start + at];

			raised[at] = c is >= 'a' and <= 'z' ? (char)(c - ('a' - 'A')) : c;
		}

		return raised.ToString();
	}

	/// <summary>
	/// The next token from a position on: its kind and where it starts, with the position moved past its end; false where
	/// only blanks and comments are left. Nothing is allocated: a caller that wants the token's text cuts it.
	/// </summary>
	internal static bool Next(string text, ref int at, out Kind kind, out int start)
	{
		return Next(text.AsSpan(), ref at, out kind, out start);
	}

	/// <summary>
	/// The same, over a stretch of text that need not be a string of its own.
	/// </summary>
	internal static bool Next(ReadOnlySpan<char> text, ref int at, out Kind kind, out int start)
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

			if (Digit(c) || c == '.' && Digit(At(text, at + 1)))
			{
				while (at < text.Length && (LetterOrDigit(text[at]) || text[at] == '.'))
					at++;

				kind = Kind.Number;

				return true;
			}

			if (Letter(c) || c is '_' or '@' or '#' or '$')
			{
				while (at < text.Length && (LetterOrDigit(text[at]) || text[at] is '_' or '@' or '#' or '$'))
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

	static char At(ReadOnlySpan<char> text, int at)
	{
		return at < text.Length ? text[at] : '\0';
	}

	// What char.IsDigit, IsLetter and IsLetterOrDigit say, answered for ASCII without asking them: nearly every character
	// a builder reads again is ASCII.

	static bool Digit(char c)
	{
		return c < '\u0080' ? c is >= '0' and <= '9' : char.IsDigit(c);
	}

	static bool Letter(char c)
	{
		return c < '\u0080' ? c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' : char.IsLetter(c);
	}

	static bool LetterOrDigit(char c)
	{
		return c < '\u0080' ? c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' : char.IsLetterOrDigit(c);
	}

	/// <summary>The end of a block comment opened at <paramref name="at"/>, the comments inside it counted.</summary>
	static int Commented(ReadOnlySpan<char> text, int at)
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
	static int Closed(ReadOnlySpan<char> text, int at, char close)
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
