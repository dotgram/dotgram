using System;
using System.Collections.Generic;
using System.Text;

namespace DotGram.Grammar.Model;

/// <summary>
/// What a value parameter stands for, put where the grammar wrote its name.
/// </summary>
/// <remarks>
/// <para>
/// §4.2 gives a parameter's kind by its declaration: a C# type makes it a value, and a
/// value is allowed anywhere a value is expected — a quantifier count, the arguments of
/// <c>@Method</c>, inside <c>@(...)</c>. Only the count ever worked. A name written in
/// C# was emitted as itself, so <c>Digits(n: int) : @int = ['0'..'9']{n} =&gt; @(n * 100)</c>
/// produced a factory reading an <c>n</c> that does not exist — a compile error in
/// somebody else's build, about a file they never wrote.
/// </para>
/// <para>
/// A specialization has one concrete argument, so what a value parameter stands for is
/// known where the specialization is made, and it is a piece of C# text. Substituting it
/// is the whole of the feature: the factory reads the literal the call passed.
/// </para>
/// </remarks>
public sealed partial class GrammarNormalizer
{
	/// <summary>What each value parameter stands for, as C#, while a specialization is lowered.</summary>
	Dictionary<string, string> _values = new(StringComparer.Ordinal);

	/// <summary>
	/// The same C#, with every value parameter's name replaced by what the call passed.
	/// </summary>
	/// <remarks>
	/// Identifier-aware rather than a string replace: a parameter called <c>n</c> must not
	/// rewrite the <c>n</c> of <c>name</c>, of <c>x.n</c>, or of anything inside a string,
	/// a character literal or a comment. An interpolated string is read as what it is —
	/// text with code in its holes — so a name in a hole is substituted and the same
	/// letters in the text around it are not.
	/// </remarks>
	string Substituted(string csharp)
	{
		if (_values.Count == 0 || csharp.Length == 0)
			return csharp;

		var written = new StringBuilder(csharp.Length);
		var at      = 0;

		while (at < csharp.Length)
		{
			var c = csharp[at];

			if (c == '/' && at + 1 < csharp.Length && (csharp[at + 1] == '/' || csharp[at + 1] == '*'))
			{
				at = Copy(csharp, written, at, EndOfComment(csharp, at));

				continue;
			}

			if (c == '\'')
			{
				at = Copy(csharp, written, at, EndOfQuoted(csharp, at, '\'', verbatim: false));

				continue;
			}

			if (OpensString(csharp, at, out var quote, out var interpolated, out var verbatim))
			{
				if (!interpolated)
				{
					at = Copy(csharp, written, at, EndOfQuoted(csharp, quote, '"', verbatim));

					continue;
				}

				written.Append(csharp, at, quote - at + 1);
				at = Interpolated(csharp, quote + 1, verbatim, written);

				continue;
			}

			if (IsIdentifierStart(c))
			{
				var end = at + 1;

				while (end < csharp.Length && IsIdentifierPart(csharp[end]))
					end++;

				var name   = csharp.Substring(at, end - at);
				var member = at > 0 && csharp[at - 1] == '.';

				written.Append(!member && _values.TryGetValue(name, out var value) ? value : name);
				at = end;

				continue;
			}

			written.Append(c);
			at++;
		}

		return written.ToString();
	}

	/// <summary>
	/// The body of an interpolated string, from just past its opening quote: its text
	/// copied as it stands, each hole substituted as the C# it is.
	/// </summary>
	int Interpolated(string csharp, int at, bool verbatim, StringBuilder written)
	{
		while (at < csharp.Length)
		{
			var c = csharp[at];

			// An escape, a doubled quote in a verbatim string, and a doubled brace are
			// each two characters of text.
			if (!verbatim && c == '\\' && at + 1 < csharp.Length ||
				(c == '"' || c == '{' || c == '}') && at + 1 < csharp.Length && csharp[at + 1] == c)
			{
				written.Append(csharp, at, 2);
				at += 2;

				continue;
			}

			if (c == '"')
			{
				written.Append('"');

				return at + 1;
			}

			if (c == '{')
			{
				var end = EndOfHole(csharp, at + 1);

				written.Append('{');
				written.Append(Substituted(csharp.Substring(at + 1, end - at - 1)));
				at = end;

				continue;
			}

			written.Append(c);
			at++;
		}

		return csharp.Length;
	}

	/// <summary>
	/// Where an interpolation hole's code ends — its own braces matched, and its strings
	/// and comments read as text rather than counted.
	/// </summary>
	static int EndOfHole(string csharp, int at)
	{
		var depth = 0;

		while (at < csharp.Length)
		{
			var c = csharp[at];

			if (c == '/' && at + 1 < csharp.Length && (csharp[at + 1] == '/' || csharp[at + 1] == '*'))
			{
				at = EndOfComment(csharp, at);

				continue;
			}

			if (c == '\'')
			{
				at = EndOfQuoted(csharp, at, '\'', verbatim: false);

				continue;
			}

			if (OpensString(csharp, at, out var quote, out _, out var verbatim))
			{
				at = EndOfQuoted(csharp, quote, '"', verbatim);

				continue;
			}

			if (c == '{')
				depth++;
			else if (c == '}' && depth-- == 0)
				return at;

			at++;
		}

		return csharp.Length;
	}

	/// <summary>
	/// Whether a string literal opens here, and how it is spelled: where its quote is, and
	/// which of the two prefixes it carries.
	/// </summary>
	static bool OpensString(string csharp, int at, out int quote, out bool interpolated, out bool verbatim)
	{
		quote        = at;
		interpolated = false;
		verbatim     = false;

		while (quote < csharp.Length && (csharp[quote] == '$' || csharp[quote] == '@'))
		{
			interpolated |= csharp[quote] == '$';
			verbatim     |= csharp[quote] == '@';
			quote++;
		}

		return quote < csharp.Length && csharp[quote] == '"';
	}

	static int Copy(string csharp, StringBuilder written, int from, int to)
	{
		written.Append(csharp, from, to - from);

		return to;
	}

	static int EndOfComment(string csharp, int at)
	{
		if (csharp[at + 1] == '/')
		{
			var line = csharp.IndexOf('\n', at);

			return line < 0 ? csharp.Length : line;
		}

		var block = csharp.IndexOf("*/", at + 2, StringComparison.Ordinal);

		return block < 0 ? csharp.Length : block + 2;
	}

	/// <summary>Past a whole quoted run, its opening quote included.</summary>
	static int EndOfQuoted(string csharp, int at, char quote, bool verbatim)
	{
		var end = at + 1;

		while (end < csharp.Length)
		{
			if (!verbatim && csharp[end] == '\\')
			{
				end += 2;

				continue;
			}

			if (csharp[end] == quote)
			{
				// `""` inside a verbatim string is one quote of text, not the end of it.
				if (verbatim && end + 1 < csharp.Length && csharp[end + 1] == quote)
				{
					end += 2;

					continue;
				}

				return end + 1;
			}

			end++;
		}

		return csharp.Length;
	}

	static bool IsIdentifierStart(char c) => char.IsLetter(c) || c == '_';

	static bool IsIdentifierPart(char c) => char.IsLetterOrDigit(c) || c == '_';
}
