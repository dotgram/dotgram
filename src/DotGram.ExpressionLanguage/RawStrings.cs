using System;
using System.Collections.Generic;
using System.Text;

namespace DotGram.ExpressionLanguage;

// Raw string literals — a run of three or more quotes, text, and the same run again — with
// their interpolated forms, read as C# 11 reads them.
//
// The grammar reads the forms of three to five quotes and one or two dollars itself: each a
// beginning the lexer reads and a rule, handed the closing quotes, that ends it. Everything
// longer is measured and cut here, by hand, because C# allows any number of either and the
// notation writes a count where it knows one. What both share is what the value of a raw
// string is: the text between its quotes, and — where that text runs over several lines — the
// lines between the first and the last with the last line's indentation taken off each.

public static partial class ExpressionParser
{
	/// <summary>A raw string with no holes, from its token and the number of quotes it opened with.</summary>
	internal static string Raw(string token, int quotes)
	{
		if (token is null)
			throw new ArgumentNullException(nameof(token));

		var text = new StringBuilder();

		foreach (var part in Unindented([Segment.Of(token.Substring(quotes, token.Length - 2 * quotes))]))
			text.Append(part.Text);

		return text.ToString();
	}

	/// <summary>A raw interpolated string the grammar measured, its lines unindented.</summary>
	internal static InterpolatedText Raw(Segment[] parts, string input) =>
		new(Unindented(parts ?? throw new ArgumentNullException(nameof(parts))),
			input ?? throw new ArgumentNullException(nameof(input)));

	/// <summary>A raw string of more quotes or dollars than the grammar writes out, cut by hand.</summary>
	/// <param name="token">The whole token, dollars and quotes included.</param>
	/// <param name="input">The text the token stands in, which the holes are read over.</param>
	/// <param name="at">Where the token stands in that text, so that a hole's window is where it is.</param>
	internal static InterpolatedText RawByHand(string token, string input, SourceSpan at)
	{
		if (token is null)
			throw new ArgumentNullException(nameof(token));

		var dollars = Run(token.AsSpan(), 0, '$');
		var quotes  = Run(token.AsSpan(), dollars, '"');
		var parts   = new List<Segment>();

		if (RawEnd(token.AsSpan(), dollars + quotes, dollars, quotes, parts, at.Start) != token.Length)
			throw new FormatException("The raw string literal is not one C# reads.");

		return new InterpolatedText(Unindented([.. parts]), input ?? throw new ArgumentNullException(nameof(input)));
	}

	/// <summary>Where a raw string the lexer began by hand ends (§7.1's external recognizer).</summary>
	/// <remarks>
	/// The beginning is every dollar and every quote, which the lexer read before this is asked:
	/// how many of each is counted back from <paramref name="pos"/>, since that is all the
	/// beginning is, and the rest is measured the way C# measures it.
	/// </remarks>
	static bool MeasureRaw(ReadOnlySpan<char> text, ref int pos)
	{
		var quotes = 0;

		while (pos - quotes - 1 >= 0 && text[pos - quotes - 1] == '"')
			quotes++;

		var dollars = 0;

		while (pos - quotes - dollars - 1 >= 0 && text[pos - quotes - dollars - 1] == '$')
			dollars++;

		var end = RawEnd(text, pos, dollars, quotes, null, 0);

		if (end < 0)
			return false;

		pos = end;

		return true;
	}

	/// <summary>How many of a character stand in a row from a position.</summary>
	static int Run(ReadOnlySpan<char> text, int from, char what)
	{
		var at = from;

		while (at < text.Length && text[at] == what)
			at++;

		return at - from;
	}

	/// <summary>
	/// Where a raw string's text ends, past its closing quotes — or -1 where C# refuses it — with
	/// its pieces added to <paramref name="parts"/> where there is a list to add them to.
	/// </summary>
	/// <remarks>
	/// A run of quotes as long as the opening one closes it, and a longer run is refused, as C#
	/// refuses it. With dollars, a run of braces shorter than them is text; one as long opens a
	/// hole, the braces past that count being text before it; and one twice as long is refused.
	/// </remarks>
	static int RawEnd(ReadOnlySpan<char> text, int from, int dollars, int quotes, List<Segment>? parts, int origin)
	{
		var literal = new StringBuilder();
		var p       = from;

		while (p < text.Length)
		{
			var c = text[p];

			if (c == '"')
			{
				var run = Run(text, p, '"');

				if (run >= quotes)
				{
					if (run > quotes)
						return -1;

					Flush(literal, parts);

					return p + run;
				}

				literal.Append('"', run);
				p += run;

				continue;
			}

			if (dollars > 0 && c == '{')
			{
				var run = Run(text, p, '{');

				if (run < dollars)
				{
					literal.Append('{', run);
					p += run;

					continue;
				}

				if (run >= 2 * dollars)
					return -1;

				literal.Append('{', run - dollars);
				Flush(literal, parts);

				var start = p + run;
				var (length, format, next) = HoleEnd(text, start, dollars);

				if (next < 0)
					return -1;

				parts?.Add(Segment.Hole(origin + start, length).Formatted(format));
				p = next;

				continue;
			}

			if (dollars > 0 && c == '}')
			{
				var run = Run(text, p, '}');

				if (run >= dollars)
					return -1;

				literal.Append('}', run);
				p += run;

				continue;
			}

			literal.Append(c);
			p++;
		}

		return -1;

		static void Flush(StringBuilder literal, List<Segment>? parts)
		{
			if (literal.Length == 0)
				return;

			parts?.Add(Segment.Of(literal.ToString()));
			literal.Clear();
		}
	}

	/// <summary>
	/// A hole of a raw string: the length of the expression's window, the format after its colon
	/// where there is one, and where the hole's closing braces end — or -1 there where it has none.
	/// </summary>
	/// <remarks>
	/// Brackets are balanced and strings skipped, so a brace or a quote inside either is not the
	/// hole's; a colon standing at the top begins the format, as it does for C#'s lexer.
	/// </remarks>
	static (int Length, string? Format, int Next) HoleEnd(ReadOnlySpan<char> text, int start, int dollars)
	{
		var depth = 0;
		var colon = -1;
		var p     = start;

		while (p < text.Length)
		{
			var c = text[p];

			if (colon >= 0 && c != '}')
			{
				p++;

				continue;
			}

			if (colon < 0)
			{
				if (c is '(' or '[' or '{')
				{
					depth++;
					p++;

					continue;
				}

				if (c is ')' or ']' || c == '}' && depth > 0)
				{
					depth--;
					p++;

					continue;
				}

				if (c == ':' && depth == 0)
				{
					colon = p;
					p++;

					continue;
				}

				if (c is '"' or '\'' or '@' or '$')
				{
					var skipped = Skipped(text, p);

					if (skipped < 0)
						return (0, null, -1);

					p = skipped > p ? skipped : p + 1;

					continue;
				}

				if (c != '}')
				{
					p++;

					continue;
				}
			}

			// A closing brace at the top: the hole ends here, with as many as it opened with.
			if (Run(text, p, '}') < dollars)
				return (0, null, -1);

			var end = colon < 0 ? p : colon;

			return (end - start, colon < 0 ? null : text.Slice(colon + 1, p - colon - 1).ToString(), p + dollars);
		}

		return (0, null, -1);
	}

	/// <summary>
	/// Past a string or a character written inside a hole, the position itself where none begins
	/// there, or -1 where one begins and does not end.
	/// </summary>
	static int Skipped(ReadOnlySpan<char> text, int p)
	{
		var q        = p;
		var dollars  = 0;
		var verbatim = false;

		while (q < text.Length && (text[q] == '$' || text[q] == '@'))
		{
			if (text[q] == '$')
				dollars++;
			else
				verbatim = true;

			q++;
		}

		if (q >= text.Length)
			return p;

		if (q == p && text[q] == '\'')
		{
			q++;

			if (q < text.Length && text[q] == '\\')
				q++;

			q++;

			while (q < text.Length && text[q] != '\'')
				q++;

			return q < text.Length ? q + 1 : -1;
		}

		if (text[q] != '"')
			return p;

		var run = Run(text, q, '"');

		if (run >= 3 && !verbatim)
			return RawEnd(text, q + run, dollars, run, null, 0);

		for (q++; q < text.Length; q++)
		{
			var c = text[q];

			if (c == '"')
			{
				if (verbatim && q + 1 < text.Length && text[q + 1] == '"')
				{
					q++;

					continue;
				}

				return q + 1;
			}

			if (c == '\\' && !verbatim)
			{
				q++;

				continue;
			}

			if (dollars > 0 && (c == '{' || c == '}') && q + 1 < text.Length && text[q + 1] == c)
			{
				q++;

				continue;
			}

			if (dollars > 0 && c == '{')
			{
				var (_, _, next) = HoleEnd(text, q + 1, 1);

				if (next < 0)
					return -1;

				q = next - 1;
			}
		}

		return -1;
	}

	/// <summary>A raw string's pieces with the closing line's indentation taken off every line.</summary>
	/// <remarks>
	/// <para>
	/// A raw string on one line is its text, and a run of quotes at its end is refused: C# reads
	/// the quotes that close one greedily, so such a run would have closed it sooner.
	/// </para>
	/// <para>
	/// One over several lines is not the first line and not the last: nothing but whitespace may
	/// stand after the opening quotes or before the closing ones, and what stands before the
	/// closing quotes is the indentation every line between must begin with — except one that is
	/// only whitespace, which is left empty. The line breaks between those lines are kept as they
	/// were written. A hole is a piece of a line like any text, and one standing where the
	/// indentation should is refused, since a hole is no whitespace.
	/// </para>
	/// </remarks>
	static Segment[] Unindented(Segment[] parts)
	{
		var lines    = new List<List<Segment>> { new() };
		var breaks   = new List<string>();

		foreach (var part in parts)
		{
			if (part.Text is not { } text)
			{
				lines[lines.Count - 1].Add(part);

				continue;
			}

			var from = 0;

			for (var i = 0; i < text.Length; i++)
			{
				if (text[i] != '\r' && text[i] != '\n')
					continue;

				var width = text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n' ? 2 : 1;

				if (i > from)
					lines[lines.Count - 1].Add(Segment.Of(text.Substring(from, i - from)));

				breaks.Add(text.Substring(i, width));
				lines.Add([]);

				i   += width - 1;
				from = i + 1;
			}

			if (from < text.Length)
				lines[lines.Count - 1].Add(Segment.Of(text.Substring(from)));
		}

		if (lines.Count == 1)
		{
			if (lines[0].Count > 0 && lines[0][lines[0].Count - 1].Text is { } last && last.EndsWith("\"", StringComparison.Ordinal))
				throw new FormatException(
					"The raw string literal does not start with enough quote characters to allow this many " +
					"consecutive quote characters as content.");

			return Merged(lines[0]);
		}

		if (Whitespace(lines[0]) is null)
			throw new FormatException("The opening quotes of a multi-line raw string literal must be the last thing on their line.");

		if (Whitespace(lines[lines.Count - 1]) is not { } indentation)
			throw new FormatException("The closing quotes of a multi-line raw string literal must stand on a line of their own.");

		if (lines.Count < 3)
			throw new FormatException("Multi-line raw string literals must contain at least one line of content.");

		var made = new List<Segment>();

		for (var at = 1; at < lines.Count - 1; at++)
		{
			var line = Merged(lines[at]);

			if (Whitespace(lines[at]) is { } blank)
			{
				if (!blank.StartsWith(indentation, StringComparison.Ordinal) && !indentation.StartsWith(blank, StringComparison.Ordinal))
					throw new FormatException(Mismatch(blank, indentation));

				var kept = blank.Length > indentation.Length ? blank.Substring(indentation.Length) : "";

				if (kept.Length > 0)
					made.Add(Segment.Of(kept));
			}
			else if (indentation.Length > 0)
			{
				if (line[0].Text is not { } first || !first.StartsWith(indentation, StringComparison.Ordinal))
					throw new FormatException(Mismatch(line[0].Text ?? "", indentation));

				if (first.Length > indentation.Length)
					made.Add(Segment.Of(first.Substring(indentation.Length)));

				for (var piece = 1; piece < line.Length; piece++)
					made.Add(line[piece]);
			}
			else
			{
				made.AddRange(line);
			}

			// The break that ends this line, unless the next is the closing one.
			if (at < lines.Count - 2)
				made.Add(Segment.Of(breaks[at]));
		}

		return Merged(made);

		static string Mismatch(string line, string indentation) =>
			"Line does not start with the same whitespace as the closing line of the raw string literal.";
	}

	/// <summary>The text of a line that holds nothing but whitespace, or null where it holds more.</summary>
	static string? Whitespace(List<Segment> line)
	{
		var text = new StringBuilder();

		foreach (var piece in line)
		{
			if (piece.Text is not { } part)
				return null;

			foreach (var c in part)
				if (!char.IsWhiteSpace(c))
					return null;

			text.Append(part);
		}

		return text.ToString();
	}

	/// <summary>Adjacent pieces of text joined into one.</summary>
	static Segment[] Merged(List<Segment> pieces)
	{
		var merged = new List<Segment>(pieces.Count);

		foreach (var piece in pieces)
			if (piece.Text is { } text && merged.Count > 0 && merged[merged.Count - 1].Text is { } before)
				merged[merged.Count - 1] = Segment.Of(before + text);
			else
				merged.Add(piece);

		return [.. merged];
	}
}
