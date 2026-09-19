using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using DotGram.Examples.Feeds;

namespace DotGram.Handwritten.Feeds;

/// <summary>
/// The stock count of <c>StockCountExample</c>, read by hand: a line at a time from a reader,
/// into the same <see cref="StockCount"/> the grammar builds.
/// </summary>
/// <remarks>
/// <para>
/// It reads exactly what the grammar reads (docs/design/architecture-decisions.md, D1): the
/// same counts, the same lines taken for unreadable, and a refusal at the same position. The
/// grammar's <c>recover</c> is line by line вЂ” at each line the closing line gets the first
/// word, then an item, and a line that is neither is kept as unreadable вЂ” so this reads a line
/// and asks those questions of it, the first with a look past its end, since the closing line
/// is the total only where nothing follows it.
/// </para>
/// <para>
/// A refusal is where the text stops being the beginning of any count, the furthest point any
/// of those questions got: past the last line, where a closing line was wanted and not found.
/// </para>
/// </remarks>
public static class HandStockCount
{
	/// <summary>A count held in memory, or false with where it stops being one.</summary>
	public static bool TryRead(string text, out StockCount? count, out long failure)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		return TryRead(new StringReader(text), Math.Max(text.Length, 1), out count, out failure);
	}

	/// <summary>A count read from a reader, or false with where it stops being one.</summary>
	public static bool TryRead(TextReader input, out StockCount? count, out long failure, int bufferSize = 4096)
	{
		if (input is null)
			throw new ArgumentNullException(nameof(input));

		return TryRead(input, bufferSize, out count, out failure);
	}

	static bool TryRead(TextReader input, int bufferSize, out StockCount? count, out long failure)
	{
		var reader   = new LineReader(input, bufferSize);
		var lines    = new List<StockLine>();
		var furthest = 0L;
		var number   = 1;

		count = null;

		while (true)
		{
			var start = reader.Position;

			if (!reader.ReadLine(out var line, out var ending))
			{
				// Past the last line: the closing line was wanted here and there is none.
				failure = Math.Max(furthest, start);

				return false;
			}

			var end = start + line.Length + ending;

			// The closing line first, and it is the total only where nothing follows it.
			if (Total(line, start, ref furthest) is int total)
			{
				if (reader.AtEnd)
				{
					count   = new StockCount(lines.ToArray(), total);
					failure = -1;

					return true;
				}

				furthest = Math.Max(furthest, ending > 0 ? end : start + line.Length);
			}

			// Then an item, which needs its line end; a line that is neither is unreadable.
			lines.Add(Item(line, start, ending > 0, ref furthest) ?? new Unreadable(number, line));

			number++;
		}
	}

	/// <summary>
	/// <c>name: count</c> and a line end, or null with the furthest position it got to noted.
	/// </summary>
	static StockLine? Item(string line, long start, bool ended, ref long furthest)
	{
		var at = 0;

		while (at < line.Length && IsLetter(line[at]))
			at++;

		if (at == 0)
			return Refused(start, ref furthest);

		// ": " is read as one, and refused where it would have begun.
		if (at + 1 >= line.Length || line[at] != ':' || line[at + 1] != ' ')
			return Refused(start + at, ref furthest);

		var digits = at + 2;
		var end    = digits;

		while (end < line.Length && IsDigit(line[end]))
			end++;

		if (end == digits || end < line.Length || !ended)
			return Refused(start + end, ref furthest);

		return new Stocked(line.Substring(0, at), Number(line.Substring(digits, end - digits)));
	}

	/// <summary><c>END n</c>, or null with the furthest position it got to noted.</summary>
	static int? Total(string line, long start, ref long furthest)
	{
		if (!line.StartsWith("END ", StringComparison.Ordinal))
		{
			Refused(start, ref furthest);

			return null;
		}

		var end = 4;

		while (end < line.Length && IsDigit(line[end]))
			end++;

		if (end == 4 || end < line.Length)
		{
			Refused(start + end, ref furthest);

			return null;
		}

		return Number(line.Substring(4));
	}

	static StockLine? Refused(long position, ref long furthest)
	{
		furthest = Math.Max(furthest, position);

		return null;
	}

	static bool IsLetter(char c)
	{
		return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
	}

	static bool IsDigit(char c)
	{
		return c is >= '0' and <= '9';
	}

	static int Number(string digits)
	{
		return int.Parse(digits, NumberStyles.None, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Lines from a reader through a buffer of its own, with how long each line's ending was:
	/// "\r\n", "\n" or "\r", as the grammar's <c>eol</c> reads them.
	/// </summary>
	sealed class LineReader(TextReader input, int bufferSize)
	{
		readonly char[] _buffer = new char[bufferSize];
		readonly StringBuilder _line = new();

		int _at;
		int _count;

		public long Position { get; private set; }

		/// <summary>Whether the reader has nothing more.</summary>
		public bool AtEnd => !Fill();

		public bool ReadLine(out string line, out int ending)
		{
			_line.Clear();
			ending = 0;

			if (!Fill())
			{
				line = "";

				return false;
			}

			while (Fill())
			{
				var c = _buffer[_at];

				if (c == '\n')
				{
					Take();
					ending = 1;

					break;
				}

				if (c == '\r')
				{
					Take();
					ending = 1;

					if (Fill() && _buffer[_at] == '\n')
					{
						Take();
						ending = 2;
					}

					break;
				}

				_line.Append(c);
				Take();
			}

			line = _line.ToString();

			return true;
		}

		void Take()
		{
			_at++;
			Position++;
		}

		bool Fill()
		{
			if (_at < _count)
				return true;

			_count = input.Read(_buffer, 0, _buffer.Length);
			_at    = 0;

			return _count > 0;
		}
	}
}
