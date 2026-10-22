using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Grammar.Emit;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The reader a sequence of lines is read through hands back each line followed by <c>\n</c>,
/// whatever size of buffer it is read into.
/// </summary>
/// <remarks>
/// The class is compiled from the text the generator emits and read by reflection, so what is
/// tested is what a generated parser carries. It writes the terminator into the caller's buffer
/// rather than into a copy of the line, so the cases that matter are the ones where a buffer
/// ends on a line's last character, on its terminator, or holds nothing but terminators.
/// </remarks>
public sealed class LinesReaderTests
{
	static readonly Type Lines = Compiled();

	/// <summary>The emitted class, taken from the generator by reflection: it is internal there.</summary>
	static Type Compiled()
	{
		var emitter = typeof(CSharpEmitter);
		var source  = (string)emitter.GetField("LinesClass", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
		var name    = (string)emitter.GetField("LinesType", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

		var assembly = EmittedCode.Compile("partial class Grammar\n{\n" + source + "\n}\n");

		return assembly.GetType("Grammar")!.GetNestedType(name, BindingFlags.NonPublic)!;
	}

	static TextReader Reader(IEnumerable<string> lines)
	{
		return (TextReader)Activator.CreateInstance(Lines, lines)!;
	}

	/// <summary>Everything the reader hands back, read <paramref name="size"/> characters at a time.</summary>
	static string ReadAll(IEnumerable<string> lines, int size, int offset = 0)
	{
		using var reader = Reader(lines);

		var buffer = new char[size + offset];
		var text   = new StringBuilder();

		while (true)
		{
			var read = reader.Read(buffer, offset, size);

			Assert.InRange(read, 0, size);

			if (read == 0)
				break;

			text.Append(buffer, offset, read);
		}

		// Ended stays ended.
		Assert.Equal(0, reader.Read(buffer, offset, size));

		return text.ToString();
	}

	static string Expected(IEnumerable<string> lines)
	{
		return string.Concat(lines.Select(static line => line + "\n"));
	}

	public static TheoryData<string, string[]> Sequences => new()
	{
		{ "nothing", [] },
		{ "one empty line", [""] },
		{ "only empty lines", ["", "", ""] },
		{ "empty lines between", ["a", "", "bc", "", ""] },
		{ "one character lines", ["a", "b", "c", "d"] },
		{ "lines of every width", [.. Enumerable.Range(0, 40).Select(static width => new string((char)('a' + width % 26), width))] },
		{ "a long line", [new string('x', 10_000), "y"] },
	};

	[Theory]
	[MemberData(nameof(Sequences))]
	public void Every_buffer_size_reads_the_same_text(string name, string[] lines)
	{
		_ = name;

		var expected = Expected(lines);

		foreach (var size in new[] { 1, 2, 3, 4, 5, 7, 8, 16, 41, 4096 })
		{
			Assert.Equal(expected, ReadAll(lines, size));
			Assert.Equal(expected, ReadAll(lines, size, offset: 3));
		}
	}

	[Fact]
	public void A_buffer_that_ends_on_a_line_leaves_its_terminator_for_the_next_read()
	{
		using var reader = Reader(["abc", "de"]);

		var buffer = new char[3];

		Assert.Equal(3, reader.Read(buffer, 0, 3));
		Assert.Equal("abc", new string(buffer, 0, 3));

		Assert.Equal(3, reader.Read(buffer, 0, 3));
		Assert.Equal("\nde", new string(buffer, 0, 3));

		Assert.Equal(1, reader.Read(buffer, 0, 3));
		Assert.Equal('\n', buffer[0]);

		Assert.Equal(0, reader.Read(buffer, 0, 3));
	}

	[Fact]
	public void A_null_line_reads_as_an_empty_one()
	{
		Assert.Equal("a\n\nb\n", ReadAll(["a", null!, "b"], 2));
	}

	[Fact]
	public void Many_lines_read_as_their_text()
	{
		var lines = Enumerable.Range(0, 100_000).Select(static i => "R|AAPL|" + i % 1000).ToArray();

		Assert.Equal(Expected(lines), ReadAll(lines, 4096));
		Assert.Equal(Expected(lines), ReadAll(lines, 13));
	}

	[Fact]
	public void The_sequence_is_disposed_with_the_reader()
	{
		var lines = new Disposing();

		var reader = Reader(lines);

		// Begun, so that the enumerator stands inside its try and has a finally to run.
		Assert.Equal(1, reader.Read(new char[1], 0, 1));

		reader.Dispose();

		Assert.True(lines.Disposed);
	}

	sealed class Disposing : IEnumerable<string>
	{
		public bool Disposed { get; private set; }

		public IEnumerator<string> GetEnumerator()
		{
			try
			{
				yield return "a";
			}
			finally
			{
				Disposed = true;
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
