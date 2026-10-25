using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The adapter that reads a <see cref="TextReader"/> as the segments of a grammar that names <c>eos</c>
/// (each line a segment, U+0000 refused) rewrites the line endings in the buffer it is handed. It is held
/// to the adapter that copied the text a character at a time, and to how <c>File.ReadLines</c> splits a
/// file, over every text of a few characters and a spread of longer ones, at every size the source hands
/// its text over in and every size the reader is asked for.
/// </summary>
public sealed class SegmentReaderTests
{
	const string Feed = """
		Feed   : @string[] = Row* recover eos => @("bad " + parserLine)
		Row    : @string   = n: Number & '|' & t: Text & eos => @(n + "=" + t)
		Number = ['0'..'9']+
		Text   = [^ '|']+
		parse Feed
		""";

	/// <summary>How many texts and settings the differential has held, for the run to show it was not empty.</summary>
	static long _held;

	static readonly Type Adapter = FindAdapter();

	static Type FindAdapter()
	{
		var result = GramCompiler.Compile(Feed, new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(Assert.Single(result.Sources).Text);

		return assembly.GetType("Grammar")!.GetNestedType("SegmentReader_DotGram", BindingFlags.NonPublic)!;
	}

	/// <summary>The same answers as the adapter it replaces, for every text of up to six characters.</summary>
	[Fact]
	public void The_adapter_answers_as_the_one_that_copied_a_character_at_a_time_over_every_short_text()
	{
		var alphabet = new[] { 'a', '\r', '\n', '\0' };
		var texts    = new List<string> { "" };

		for (var length = 1; length <= 6; length++)
		{
			var count = (int)Math.Pow(alphabet.Length, length);

			for (var i = 0; i < count; i++)
			{
				var chars = new char[length];
				var rest  = i;

				for (var at = 0; at < length; at++, rest /= alphabet.Length)
					chars[at] = alphabet[rest % alphabet.Length];

				texts.Add(new string(chars));
			}
		}

		foreach (var text in texts)
			foreach (var block in new[] { 1, 2, 3, 4096 })
				foreach (var asked in new[] { 1, 2, 5, 4096 })
					Hold(text, block, asked);

		Assert.True(_held >= 80_000);
	}

	/// <summary>
	/// The same for longer texts, a U+0000 in the first read, the second and a later one, line endings
	/// of every kind mixed, and reads of every size, so that a count kept across reads is held to account.
	/// </summary>
	[Fact]
	public void The_adapter_answers_as_the_old_one_over_longer_texts_and_a_U0000_in_a_later_read()
	{
		var random = new Random(20261025);
		var pieces = new[] { "a", "bc", "def", "\n", "\r\n", "\r", "\n\n", "\r\r\n", "gh ij" };

		for (var round = 0; round < 3_000; round++)
		{
			var text = new StringBuilder();

			for (var length = random.Next(0, 80); length > 0; length--)
				text.Append(pieces[random.Next(pieces.Length)]);

			// Without U+0000, with one at a random place, and with one after enough text to be in a later read.
			Held(text.ToString());

			if (text.Length > 0)
				Held(text.Insert(random.Next(text.Length + 1), '\0').ToString());
		}

		// A U+0000 at every offset of a text of every kind of ending, and at the start of every read.
		foreach (var unit in new[] { "ab\n", "ab\r\n", "ab\r", "a\n\n", "\r\n" })
		{
			var body = string.Concat(Enumerable.Repeat(unit, 12));

			for (var at = 0; at <= body.Length; at++)
				Held(body.Insert(at, "\0"));
		}

		// The texts are long enough that U+0000 is in the second and the third read of 4096 characters.
		var line = new string('x', 77) + "\r\n";
		var big  = string.Concat(Enumerable.Repeat(line, 160));

		foreach (var at in new[] { 4095, 4096, 4097, 8191, 8192, 8193, 12000, big.Length - 1 })
			foreach (var block in new[] { 4096, 100, 4095, 4097 })
				foreach (var asked in new[] { 4096, 1000, 4095 })
					Hold(big.Insert(at, "\0"), block, asked);

		Assert.True(_held >= 90_000);

		void Held(string text)
		{
			foreach (var block in new[] { 1, 3, 7, 4096 })
				foreach (var asked in new[] { 1, 4, 64, 4096 })
					Hold(text, block, asked);
		}
	}

	/// <summary>
	/// A text without U+0000 is the lines <c>File.ReadLines</c> splits it into, each followed by the
	/// joiner, whichever size the source hands it over in.
	/// </summary>
	[Theory]
	[InlineData("")]
	[InlineData("\n")]
	[InlineData("\r")]
	[InlineData("\r\n")]
	[InlineData("a")]
	[InlineData("a\n")]
	[InlineData("a\r\n")]
	[InlineData("a\rb")]
	[InlineData("a\r\n\r\nb\n\nc\r\rd")]
	[InlineData("one\ntwo\r\nthree\rfour")]
	[InlineData("\n\n\n")]
	[InlineData("a\r\r\nb")]
	public void A_text_without_U0000_is_its_lines(string text)
	{
		var lines = new List<string>();

		using (var split = new StringReader(text))
			for (var line = split.ReadLine(); line is not null; line = split.ReadLine())
				lines.Add(line);

		var expected = string.Concat(lines.Select(static line => line + "\0"));

		foreach (var block in new[] { 1, 2, 3, 4096 })
			foreach (var asked in new[] { 1, 2, 3, 4096 })
				Assert.Equal(expected, Drain(Make(text, block, true), asked).Text);
	}

	/// <summary>
	/// Where a U+0000 is told to be: in the text read (a line ending counting for one position, a
	/// <c>\r\n</c> counted once), the line and the column. The positions are worked out from the
	/// text itself, not from the adapter it replaces.
	/// </summary>
	[Theory]
	[InlineData("a\0", 1L, 1, 2)]
	[InlineData("\0", 0L, 1, 1)]
	[InlineData("a\nb\0", 3L, 2, 2)]
	[InlineData("a\r\nb\0", 3L, 2, 2)]
	[InlineData("a\rb\0", 3L, 2, 2)]
	[InlineData("a\r\n\r\n\0", 3L, 3, 1)]
	[InlineData("a\n\nbc\0d", 5L, 3, 3)]
	public void A_U0000_is_told_where_it_stands(string text, long refused, int line, int column)
	{
		foreach (var block in new[] { 1, 2, 3, 4096 })
			foreach (var asked in new[] { 1, 2, 4096 })
			{
				var read = Drain(Make(text, block, false), asked);

				Assert.Equal((refused, line, column), (read.Refused, read.Line, read.Column));
			}
	}

	/// <summary>One text, one size the source hands it over in, one size the reader is asked for: both adapters, in both modes.</summary>
	static void Hold(string text, int block, int asked)
	{
		// Answering: the text read up to the refusal, and where it was.
		var now = Drain(Make(text, block, false), asked);
		var was = Drain(new Reference(new BlockReader(text, block), false), asked);

		Assert.Equal(was, now);

		// Throwing: the refusal, and what it says of the line and the column.
		var thrownNow = Drain(Make(text, block, true), asked);
		var thrownWas = Drain(new Reference(new BlockReader(text, block), true), asked);

		Assert.Equal(thrownWas.Thrown, thrownNow.Thrown);
		Assert.Equal((thrownWas.Refused, thrownWas.Line, thrownWas.Column), (thrownNow.Refused, thrownNow.Line, thrownNow.Column));

		if (thrownNow.Thrown is null)
			Assert.Equal(thrownWas.Text, thrownNow.Text);

		_held++;
	}

	static TextReader Make(string text, int block, bool throws)
	{
		return (TextReader)Activator.CreateInstance(Adapter, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [new BlockReader(text, block), throws], null)!;
	}

	/// <summary>Reads a reader to its end <paramref name="asked"/> characters at a time, and keeps what it says of a refusal.</summary>
	static Outcome Drain(TextReader reader, int asked)
	{
		var text   = new StringBuilder();
		var buffer = new char[asked + 3];
		string? thrown = null;

		try
		{
			for (var read = reader.Read(buffer, 1, asked); read > 0; read = reader.Read(buffer, 1, asked))
			{
				Assert.True(read <= asked);
				text.Append(buffer, 1, read);
			}
		}
		catch (FormatException caught)
		{
			thrown = caught.Message;
		}

		var type = reader.GetType();

		return new Outcome(
			text.ToString(),
			thrown,
			(long)type.GetProperty("Refused")!.GetValue(reader)!,
			(int)type.GetProperty("RefusedLine")!.GetValue(reader)!,
			(int)type.GetProperty("RefusedColumn")!.GetValue(reader)!);
	}

	sealed record Outcome(string Text, string? Thrown, long Refused, int Line, int Column);

	/// <summary>A reader that hands its text over a few characters at a time.</summary>
	sealed class BlockReader(string text, int block) : TextReader
	{
		int _at;

		public override int Read(char[] buffer, int index, int count)
		{
			var length = Math.Min(Math.Min(block, count), text.Length - _at);

			text.CopyTo(_at, buffer, index, length);
			_at += length;

			return length;
		}
	}

	/// <summary>
	/// The adapter as it was before it read in place, which copied the text through a block of its own one
	/// character at a time: what the new one is held to.
	/// </summary>
	sealed class Reference(TextReader input, bool throws) : TextReader
	{
		readonly char[] _block = new char[4096];
		int  _at;
		int  _filled;
		bool _cr;
		bool _open;
		bool _ended;
		long _position;
		int  _line = 1;
		int  _column;

		public long Refused { get; private set; } = -1;

		public int RefusedLine { get; private set; }

		public int RefusedColumn { get; private set; }

		public override int Read(char[] buffer, int index, int count)
		{
			var written = 0;

			while (written < count && !_ended)
			{
				if (_at == _filled)
				{
					_filled = input.Read(_block, 0, _block.Length);
					_at     = 0;

					if (_filled <= 0)
					{
						_filled = 0;
						_ended  = true;

						if (_open)
						{
							buffer[index + written++] = '\0';
							_open = false;
						}

						break;
					}
				}

				var c = _block[_at++];

				if (_cr && c == '\n')
				{
					_cr = false;
					continue;
				}

				_cr = c == '\r';

				if (c == '\r' || c == '\n')
				{
					buffer[index + written++] = '\0';
					_open   = false;
					_column = 0;
					_line++;
				}
				else if (c == '\0')
				{
					Refused       = _position;
					RefusedLine   = _line;
					RefusedColumn = _column + 1;
					_ended        = true;

					if (throws)
						throw new FormatException(
							"A line holds U+0000, which this grammar reads as the end of a segment and never as a character of one at line " +
							_line + ", column " + (_column + 1) + ".");

					break;
				}
				else
				{
					buffer[index + written++] = c;
					_open = true;
					_column++;
				}

				_position++;
			}

			return written;
		}
	}
}
