using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.VisualStudio;

/// <summary>
/// Maps positions in a decoded C# string value back to the literal spelling in its
/// syntax tree. Unsupported or malformed tokens produce no map rather than an
/// approximate one.
/// </summary>
public sealed class CSharpStringMap
{
	readonly int[] _starts;
	readonly int[] _ends;
	readonly int   _end;

	CSharpStringMap(int[] starts, int[] ends, int end)
	{
		_starts = starts;
		_ends   = ends;
		_end    = end;
	}

	public int Length => _starts.Length;

	/// <summary>Creates a map for a non-interpolated regular, verbatim or raw string.</summary>
	public static bool TryCreate(SyntaxToken token, out CSharpStringMap? map)
	{
		map = null;

		if (!token.IsKind(SyntaxKind.StringLiteralToken) &&
			!token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken) &&
			!token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken))
			return false;

		var text   = token.Text;
		var starts = new List<int>(token.ValueText.Length);
		var ends   = new List<int>(token.ValueText.Length);
		var end    = 0;

		var made = text.StartsWith("@\"", StringComparison.Ordinal)
			? Verbatim(text, token.SpanStart, starts, ends, out end)
			: StartsRaw(text)
				? Raw(text, token.SpanStart, starts, ends, out end)
				: Regular(text, token.SpanStart, starts, ends, out end);

		if (!made || starts.Count != token.ValueText.Length)
			return false;

		map = new CSharpStringMap(starts.ToArray(), ends.ToArray(), end);

		return true;
	}

	/// <summary>Maps a decoded span to a correct, possibly broader, host source span.</summary>
	public bool TryMap(int decodedStart, int decodedLength, out TextSpan sourceSpan)
	{
		sourceSpan = default;

		if (decodedStart < 0 || decodedLength < 0 || decodedStart > Length - decodedLength)
			return false;

		if (decodedLength == 0)
		{
			var position = decodedStart == Length ? _end : _starts[decodedStart];

			sourceSpan = new TextSpan(position, 0);

			return true;
		}

		var from = _starts[decodedStart];
		var to   = _ends[decodedStart + decodedLength - 1];

		sourceSpan = TextSpan.FromBounds(from, to);

		return true;
	}

	static bool Regular(string text, int offset, List<int> starts, List<int> ends, out int end)
	{
		end = 0;

		if (text.Length < 2 || text[0] != '"' || text[text.Length - 1] != '"')
			return false;

		var at = 1;

		while (at < text.Length - 1)
		{
			var from = at;
			var units = 1;

			if (text[at] == '\\')
			{
				if (!Escape(text, ref at, text.Length - 1, out units))
					return false;
			}
			else
				at++;

			for (var unit = 0; unit < units; unit++)
			{
				starts.Add(offset + from);
				ends.Add(offset + at);
			}
		}

		end = offset + text.Length - 1;

		return true;
	}

	static bool Escape(string text, ref int at, int limit, out int units)
	{
		units = 1;
		at++;

		if (at >= limit)
			return false;

		var kind = text[at++];

		if (kind == 'u')
			return Hex(text, ref at, limit, 4, 4);

		if (kind == 'U')
		{
			if (!EightHex(text, ref at, limit, out var scalar) || scalar > 0x10FFFF)
				return false;

			units = scalar > 0xFFFF ? 2 : 1;

			return true;
		}

		if (kind == 'x')
			return Hex(text, ref at, limit, 1, 4);

		return kind is '\'' or '"' or '\\' or '0' or 'a' or 'b' or 'f' or 'n' or 'r' or 't' or 'v';
	}

	static bool Hex(string text, ref int at, int limit, int minimum, int maximum)
	{
		var count = 0;

		while (at < limit && count < maximum && IsHex(text[at]))
		{
			at++;
			count++;
		}

		return count >= minimum;
	}

	static bool IsHex(char value) =>
		value is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';

	static bool EightHex(string text, ref int at, int limit, out uint value)
	{
		value = 0;

		if (limit - at < 8)
			return false;

		for (var count = 0; count < 8; count++)
		{
			var digit = text[at++];

			if (!IsHex(digit))
				return false;

			value = value * 16 + (uint)(digit <= '9'
				? digit - '0'
				: char.ToUpperInvariant(digit) - 'A' + 10);
		}

		return true;
	}

	static bool Verbatim(string text, int offset, List<int> starts, List<int> ends, out int end)
	{
		end = 0;

		if (text.Length < 3 || !text.EndsWith("\"", StringComparison.Ordinal))
			return false;

		var at = 2;

		while (at < text.Length - 1)
		{
			var from = at;

			if (text[at] == '"')
			{
				if (at + 1 >= text.Length - 1 || text[at + 1] != '"')
					return false;

				at += 2;
			}
			else
				at++;

			starts.Add(offset + from);
			ends.Add(offset + at);
		}

		end = offset + text.Length - 1;

		return true;
	}

	static bool StartsRaw(string text)
	{
		var quotes = 0;

		while (quotes < text.Length && text[quotes] == '"')
			quotes++;

		return quotes >= 3;
	}

	static bool Raw(string text, int offset, List<int> starts, List<int> ends, out int end)
	{
		end = 0;

		var quotes = 0;

		while (quotes < text.Length && text[quotes] == '"')
			quotes++;

		var close = text.Length - quotes;

		if (quotes < 3 || close < quotes || !Quotes(text, close, quotes))
			return false;

		if (!NewLine(text, quotes, out var content))
		{
			for (var at = quotes; at < close; at++)
			{
				starts.Add(offset + at);
				ends.Add(offset + at + 1);
			}

			end = offset + close;

			return true;
		}

		var closingLine = LineStart(text, close);

		for (var at = closingLine; at < close; at++)
			if (text[at] is not (' ' or '\t'))
				return false;

		var indentation = text.Substring(closingLine, close - closingLine);
		var line        = content;

		while (line < closingLine)
		{
			var lineEnd    = NextLine(text, line, closingLine, out var afterLine);
			var whitespace = line;

			while (whitespace < lineEnd && text[whitespace] is ' ' or '\t')
				whitespace++;

			var blank = whitespace == lineEnd;
			var start = line;

			if (text.AsSpan(line, lineEnd - line).StartsWith(indentation.AsSpan(), StringComparison.Ordinal))
				start += indentation.Length;
			else if (blank && indentation.AsSpan().StartsWith(text.AsSpan(line, lineEnd - line), StringComparison.Ordinal))
				start = lineEnd;
			else
				return false;

			for (var at = start; at < lineEnd; at++)
			{
				starts.Add(offset + at);
				ends.Add(offset + at + 1);
			}

			if (afterLine < closingLine)
				for (var at = lineEnd; at < afterLine; at++)
				{
					starts.Add(offset + at);
					ends.Add(offset + at + 1);
				}

			line = afterLine;
		}

		end = offset + closingLine;

		return true;
	}

	static bool Quotes(string text, int start, int count)
	{
		for (var at = start; at < start + count; at++)
			if (text[at] != '"')
				return false;

		return true;
	}

	static bool NewLine(string text, int at, out int after)
	{
		after = at;

		if (at >= text.Length)
			return false;

		if (text[at] == '\n')
		{
			after = at + 1;

			return true;
		}

		if (text[at] == '\r' && at + 1 < text.Length && text[at + 1] == '\n')
		{
			after = at + 2;

			return true;
		}

		return false;
	}

	static int LineStart(string text, int at)
	{
		while (at > 0 && text[at - 1] is not ('\r' or '\n'))
			at--;

		return at;
	}

	static int NextLine(string text, int at, int limit, out int after)
	{
		while (at < limit && text[at] is not ('\r' or '\n'))
			at++;

		after = at;

		if (after < limit && text[after] == '\r')
			after++;

		if (after < limit && text[after] == '\n')
			after++;

		return at;
	}
}
