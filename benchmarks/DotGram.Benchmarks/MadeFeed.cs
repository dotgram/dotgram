using System;
using System.IO;
using System.Text;

namespace DotGram.Benchmarks;

/// <summary>A feed of the asked-for length, made as it is read and never held.</summary>
/// <remarks>
/// <para>
/// What `--feed` measures is what a streamed parse holds, and a file on disk would put a
/// second thing in the way of that: reading twenty gigabytes off a disk says as much about
/// the disk as about the parser, and needing twenty gigabytes of disk to run a benchmark
/// is how a benchmark stops being run.
/// </para>
/// <para>
/// So the input exists one line at a time. This reader keeps one line's worth of
/// characters and nothing else, which means the process's own memory is the parse's and
/// not the corpus's.
/// </para>
/// </remarks>
public sealed class MadeFeed(long rows) : TextReader
{
	readonly StringBuilder _line = new();

	long _row;
	int  _at;

	/// <summary>How much text has been handed over: the size of the input.</summary>
	public long Characters { get; private set; }

	public override int Read(char[] buffer, int index, int count)
	{
		if (_at >= _line.Length)
		{
			_line.Clear();
			_at = 0;

			if (_row == 0)
				_line.Append("H|made\n");
			else if (_row <= rows)
				// About forty characters, which is what a feed line looks like — enough
				// that the size of the input is records rather than line endings.
				_line
					.Append("R|SYM").Append((_row % 1000).ToString("000"))
					.Append("-PAYLOAD-PAYLOAD-PAYLOAD|")
					.Append(_row % 10000)
					.Append('\n');
			else if (_row == rows + 1)
				_line.Append("T|0\n");
			else
				return 0;

			_row++;
		}

		var taken = Math.Min(count, _line.Length - _at);

		_line.CopyTo(_at, buffer, index, taken);
		_at += taken;
		Characters += taken;

		return taken;
	}
}
