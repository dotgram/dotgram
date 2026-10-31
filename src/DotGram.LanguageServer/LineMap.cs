using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.Text;

namespace DotGram.LanguageServer;

/// <summary>Offsets of a text to the protocol's positions and back: zero-based lines, UTF-16 code units.</summary>
/// <remarks>
/// The lines are the protocol's: they end at <c>\n</c>, <c>\r\n</c> or <c>\r</c> and nowhere else.
/// Roslyn's <see cref="SourceText.Lines"/> also breaks at U+0085, U+2028 and U+2029, which an LSP client
/// does not, so a grammar holding one of those would have every later position off by a line.
/// </remarks>
sealed class LineMap
{
	readonly int[]      _starts;
	readonly SourceText _text;

	public LineMap(SourceText text)
	{
		var starts = new List<int> { 0 };
		var length = text.Length;

		for (var at = 0; at < length; at++)
		{
			var character = text[at];

			if (character == '\r')
			{
				if (at + 1 < length && text[at + 1] == '\n')
					at++;

				starts.Add(at + 1);
			}
			else if (character == '\n')
				starts.Add(at + 1);
		}

		_starts = [.. starts];
		_text   = text;
		Length  = length;
	}

	public LineMap(string text)
		: this(SourceText.From(text))
	{
	}

	LineMap(int[] starts, SourceText text)
	{
		_starts = starts;
		_text   = text;
		Length  = text.Length;
	}

	/// <summary>
	/// The map of the text after one change, worked out from this one's: the lines before the change
	/// are kept, those after it moved, and only the changed text read — an edit of a large grammar
	/// is not a read of all of it.
	/// </summary>
	/// <param name="text">The text after the change.</param>
	/// <param name="start">Where the change starts.</param>
	/// <param name="end">Where the replaced text ended, in the text before.</param>
	/// <param name="inserted">How long the inserted text is.</param>
	public LineMap Apply(SourceText text, int start, int end, int inserted)
	{
		// A line starts at p where the character before p breaks the line: a '\n', or a '\r' not
		// followed by '\n'. That depends on the characters at p - 1 and p, so a start the change
		// cannot have moved is one before it (p < start) or one after it (p > end, in the old text).
		var shift  = inserted - (end - start);
		var result = new List<int>(_starts.Length + 4);

		foreach (var at in _starts)
		{
			if (at >= start)
				break;

			result.Add(at);
		}

		if (result.Count == 0)
			result.Add(0);

		for (var at = Math.Max(1, start); at <= start + inserted && at <= text.Length; at++)
		{
			var before = text[at - 1];

			if (before == '\n' || before == '\r' && (at == text.Length || text[at] != '\n'))
				result.Add(at);
		}

		var first = Array.BinarySearch(_starts, end + 1);

		for (var index = first < 0 ? ~first : first; index < _starts.Length; index++)
			result.Add(_starts[index] + shift);

		return new LineMap([.. result], text);
	}

	public int Length { get; }

	public int LineCount => _starts.Length;

	public Position Position(int offset)
	{
		var bounded = Math.Max(0, Math.Min(offset, Length));
		var line    = Array.BinarySearch(_starts, bounded);

		if (line < 0)
			line = ~line - 1;

		return new Position(line, bounded - _starts[line]);
	}

	public Range Range(int offset, int length)
	{
		return new Range(Position(offset), Position(offset + length));
	}

	/// <summary>The offset of a position; a character past the end of its line is the line's end, as the protocol has it.</summary>
	public int Offset(Position position)
	{
		if (position.Line < 0)
			return 0;

		if (position.Line >= _starts.Length)
			return Length;

		var start = _starts[position.Line];
		var end   = LineEnd(position.Line);

		return Math.Min(start + Math.Max(0, position.Character), end);
	}

	/// <summary>Where a line's text ends, before its line break.</summary>
	public int LineEnd(int line)
	{
		var start = _starts[line];
		var end   = line + 1 < _starts.Length ? _starts[line + 1] : Length;

		if (end > start && _text[end - 1] == '\n')
			end--;

		if (end > start && _text[end - 1] == '\r')
			end--;

		return end;
	}

	public int LineStart(int line)
	{
		return _starts[line];
	}
}
