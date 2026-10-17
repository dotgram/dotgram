using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The rows read through a buffer — a reader, a stream, bytes held whole — as the string form
/// reads them: the same answer and the same value, whatever the size of the pieces the input
/// arrives in; and a refusal at the same place in the same words, but for the rows a buffer
/// places and words apart for reasons that are not the bindings' (<c>WordedApartOverABuffer</c>).
/// </summary>
/// <remarks>
/// A buffer of one item makes every boundary between two items a boundary between two reads,
/// so a binding, the reference to it and a count by its length are each cut through somewhere
/// on every input of every row — and, read from a source that hands over one item per read
/// whatever room it is given, inside a window that has grown as well. Bytes are read for the
/// inputs that are ASCII, where an item of the one is an item of the other.
/// </remarks>
public sealed partial class BackReferenceTests
{
	/// <summary>The buffer sizes every input is read through: an item at a time, a few, and all at once.</summary>
	static readonly int[] BufferSizes = [1, 2, 3, 5, 4096];

	/// <summary>
	/// The rows whose refusals a buffered reading places or words otherwise than the string's, in the ways
	/// it words them apart over grammars with no binding as well: a literal where the binding is,
	/// or a number where the length is, shows the same kinds of difference. Where the input
	/// ends inside a look the reading over a buffer may leave out the class it would have read
	/// there, or say it as <c>[^ ]</c> where the string's says <c>any</c>
	/// (<c>"[==[" &amp; (?!"]==]" &amp; any)* &amp; "]==]"</c> on <c>[==[a]=]</c>, on the engine); a
	/// repetition's next turn may go unsaid (<c>'a'+ &amp; ':' &amp; "bc"{1,2} &amp; "bc" &amp; 'x'</c>
	/// on <c>aa:bcx</c> is <c>"bc" or "bcx"</c> to the string and <c>"bcx"</c> to the reader); and the
	/// engine names the class a rule is where the string names the rule (<c>Title</c>, <c>N</c>).
	/// After a lookahead that failed near the end the place can move too:
	/// <c>'b' &amp; ('b' &amp; ':')* &amp; any &amp; ?=("b") &amp; eof</c> on <c>ba</c>. Their answers
	/// and values are held to the string's; where and in what words they refuse, to every other
	/// buffered reading of the row.
	/// </summary>
	static readonly HashSet<string> WordedApartOverABuffer =
	[
		"BindingInTheBaseOfAFold", "CountByAnEmptyLengthInALoop", "CountOfASequenceGivesBack", "CppRaw",
		"CSharpRawString", "Heredoc", "HeredocWithQuotedTag", "InlineCodeSpan", "LuaLongComment", "LuaLongString",
		"PostgreSqlDollarQuoted", "RawStringWithoutAtomicGroups", "RestructuredTextHeading", "RustRaw",
		"SwiftExtendedDelimiter", "UntilTheSameCharacter",
	];

	[Theory]
	[MemberData(nameof(RowNames))]
	public void A_buffered_reading_reads_every_row_as_the_string_does(string name)
	{
		BackReferenceEmission.RequireBoth();

		var row = Named(name);

		foreach (var direct in new[] { true, false })
		{
			var (assembly, bytes) = Buffered(row.Published(), CarrierKind.Auto, direct);

			foreach (var input in row.Accepted.Concat(row.Refused))
				AgreesOverABuffer(row.Name, assembly, row.Rule, input, direct ? "reader" : "engine", bytes);
		}
	}

	[Theory]
	[MemberData(nameof(FindRowNames))]
	public void A_buffered_find_finds_every_occurrence_the_string_does(string name)
	{
		BackReferenceEmission.RequireEngine();

		var row = Named(name);

		foreach (var direct in new[] { true, false })
		{
			var (assembly, bytes) = Buffered(row.Published(), CarrierKind.Auto, direct);

			foreach (var size in BufferSizes)
			{
				Assert.Equal(row.Found!.Select(static at => (long)at), FoundOver(assembly, "Find" + row.Rule, row.Text!, typeof(TextReader), size));

				if (bytes && IsAscii(row.Text!))
					Assert.Equal(row.Found!.Select(static at => (long)at), FoundOver(assembly, "Find" + row.Rule, row.Text!, typeof(Stream), size));
			}

			Assert.Equal(row.Found!.Select(static at => (long)at), FoundOver(assembly, "Find" + row.Rule, row.Text!, typeof(TextReader), 4096, trickle: true));
		}
	}

	/// <summary>
	/// Every carrier, through a reader an item at a time: what is kept of a binding is two
	/// positions however the carrier keeps the rest, and both stay in the window.
	/// </summary>
	[Theory]
	[InlineData("CSharpRawString")]
	[InlineData("XmlElement")]
	[InlineData("CountBetweenTwoLengths")]
	[InlineData("GiveBackAcrossAlternatives")]
	[InlineData("StarOverABindingOfAValuedRule")]
	[InlineData("BindingOverARuleThatBinds")]
	[InlineData("TriviaInsideABinding")]
	public void A_buffered_reading_agrees_on_every_carrier(string name)
	{
		BackReferenceEmission.RequireBoth();

		var row = Named(name);

		foreach (var carrier in Carriers)
			foreach (var direct in new[] { true, false })
			{
				var (assembly, bytes) = Buffered(row.Grammar + $"\nparse {row.Rule}\n", carrier, direct);

				foreach (var input in row.Accepted.Concat(row.Refused))
					AgreesOverABuffer(row.Name, assembly, row.Rule, input, $"{(direct ? "reader" : "engine")}, {carrier}", bytes);
			}
	}

	/// <summary>
	/// A feed that lets the window go at every turn of its recovering repetition, and binds text
	/// where no turn is open: in what follows the repetition, and in the trivia woven there.
	/// Such a binding begins after the turn that lets go and is read before the next, so the feed
	/// still streams: a long input is read through a window far smaller than itself, which a
	/// window kept whole could not do (it would need all of it and say so). Short inputs, a bad
	/// line recovered from included, read alike in pieces of every size.
	/// </summary>
	[Theory]
	[InlineData("Tail = $q: W & ':' & $q\nparse Feed\n", "")]
	[InlineData("Tail = T.Pair\nnamespace T\n{\n\ttrivia = { $s: (' '*) & $s }\n\tPair = W & ':' & W\n}\nparse Feed\n", "  ")]
	public void A_feed_that_binds_where_no_turn_is_open_still_lets_the_window_go(string tail, string spaces)
	{
		BackReferenceEmission.RequireBoth();

		const string feed =
			"""
			Feed : @string = lines: Line* recover eol => @("!") & Tail & eof => @(string.Join("|", lines))
			Line : @string = n: ['0'..'9']+ & eol => @(Text(n))
			W              = ['a'..'z']+
			""";

		var (assembly, source) = Recovering(feed + tail);

		string[] inputs = ["1\n22\n333\nab:ab", "1\nx\n22\nab:ab", "1\n22\nab:ac", "ab:ab", "1\n22\nab:a"];

		foreach (var input in inputs.Select(one => one.Replace(":", spaces + ":")))
		{
			var whole = Spelled(EmittedCode.Match(assembly, "Grammar", "TryParseFeed", input));

			foreach (var size in BufferSizes)
			{
				Assert.Equal(whole, Spelled(Over(assembly, "TryParseFeed", input, typeof(TextReader), size)));
				Assert.Equal(whole, Spelled(Over(assembly, "TryParseFeed", input, typeof(Stream), size)));
			}
		}

		Assert.Equal((true, "1|!|22"), Answer(Over(assembly, "TryParseFeed", inputs[1].Replace(":", spaces + ":"), typeof(TextReader), 1)));

		// 20,000 lines, some 120,000 characters, through a buffer of 16 that may hold 256 at most.
		const int lines = 20000;

		var text = string.Concat(Enumerable.Repeat("12345\n", lines)) + "ab" + spaces + ":ab";
		var type = assembly.GetType("Grammar")!;

		foreach (var over in new[] { typeof(TextReader), typeof(Stream) })
		{
			using var input = over == typeof(TextReader)
				? (IDisposable)new StringReader(text)
				: new MemoryStream(Encoding.ASCII.GetBytes(text));

			var match = type.GetMethod("TryParseFeed", [over, typeof(int?), typeof(int?)])!.Invoke(null, [input, 16, 256])!;
			var value = (string?)match.GetType().GetProperty("Value")!.GetValue(match);

			Assert.True((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!);
			Assert.Equal(lines * 6 - 1, value!.Length);
		}

		// And that is the turns letting go, not a window that happened to fit.
		Assert.Contains("text.LetGo(", source, StringComparison.Ordinal);
	}

	static (bool, object?) Answer((bool IsSuccess, object? Value, string? Error, long Position) match)
	{
		return (match.IsSuccess, match.Value);
	}

	/// <summary>
	/// The grammars a reader or stream form was refused over before it read bindings: a binding in
	/// a rule the publication reads, in the trivia woven between its operands and read at the two
	/// ends of a parse, and in the trivia of a namespace a call reaches — which an ordinary call
	/// never runs, but which the reader form is compiled over all the same.
	/// </summary>
	[Theory]
	[InlineData("R = $q: 'a'+ & 'b' & $q\nparse R stream", "aba|aabaa|aaba|abaa|b|")]
	[InlineData("trivia = { $q: (' '*) & $q }\nR = 'a'\nparse R", "a|  a|  a  | a|a   ")]
	[InlineData("trivia = { $q: (' '*) & $q }\nR = 'a' & 'b'\nparse R", "ab|a  b|  a    b  |a b|a   b")]
	[InlineData("trivia = { $q: (' '*) & $q }\nR = 'a' & 'b' & 'c'\nparse R", "abc|a  b    c|a  b c")]
	[InlineData("trivia = { $q: (' '*) & $q }\nR = S\nS = 'a'\nparse R", "a|  a  |   a")]
	[InlineData("trivia = { T }\nT = $q: (' '*) & $q\nR = 'a'\nparse R", "a|  a  |   a")]
	[InlineData("namespace N\n{\n\ttrivia = { $q: (' '*) & $q }\n\tT = 'a'\n}\nR = 'x' & N.T\nparse R", "xa|x  a|x a")]
	public void A_reader_form_over_a_binding_reads_as_the_string_form_does(string grammar, string inputs)
	{
		BackReferenceEmission.RequireBoth();

		foreach (var direct in new[] { true, false })
		{
			var (assembly, _) = Buffered(grammar, CarrierKind.Auto, direct);

			foreach (var input in inputs.Split('|'))
				foreach (var size in BufferSizes)
				{
					var whole = Spelled(EmittedCode.Match(assembly, "Grammar", "TryParseR", input));

					Assert.Equal(whole, Spelled(Over(assembly, "TryParseR", input, typeof(TextReader), size)));
					Assert.Equal(whole, Spelled(Over(assembly, "TryParseR", input, typeof(Stream), size)));
				}
		}
	}

	/// <summary>
	/// A binding that must be held longer than the buffer may grow: the reading stops with the
	/// buffer's own <see cref="IOException"/>, as it does for the same input without a binding,
	/// and never takes the text let go of for a mismatch. An input that fits reads.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_binding_longer_than_the_buffer_may_hold_stops_with_the_buffer_s_exception(bool direct)
	{
		BackReferenceEmission.RequireBoth();

		foreach (var grammar in new[] { "R = $q: 'a'+ & ':' & $q\nparse R\n", "R = 'a'+ & ':' & 'a'+\nparse R\n" })
		{
			var (assembly, bytes) = Buffered(grammar, CarrierKind.Auto, direct);

			Assert.True(bytes);

			foreach (var over in new[] { typeof(TextReader), typeof(Stream) })
			{
				foreach (var input in new[] { "aaaaaaaaa:aaaaaaaaa", "aaaa:aaaa" })
				{
					var thrown = Assert.Throws<TargetInvocationException>(() => Over(assembly, "TryParseR", input, over, 1, retained: 8));

					Assert.IsType<IOException>(thrown.InnerException);
					Assert.Contains("maxRetained", thrown.InnerException!.Message, StringComparison.Ordinal);
				}

				Assert.Equal((true, "aaa:aaa"), Answer(Over(assembly, "TryParseR", "aaa:aaa", over, 1, retained: 8)));
			}
		}
	}

	/// <summary>
	/// A terminal of a grammar cut in two is still refused where it holds a binding: the lexer
	/// measures what follows the regular beginning of a terminal from where that beginning ended,
	/// and a reference there reads back into it. Lifting the refusal over a buffer leaves this one.
	/// </summary>
	[Fact]
	public void A_binding_in_a_terminal_of_a_grammar_cut_in_two_is_refused_over_a_buffer_too()
	{
		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Quoted = $q: '"'+ & ['a'..'z']* & $q
				Word   = ['a'..'z']+
			}
			R = Lexical.Quoted & Lexical.Word
			parse R stream
			""";

		var told = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Grammar", Lexical = true, BufferedInput = true, BufferedBytes = true }).Diagnostics;

		var refused = told.Where(static one => one.Id == GrammarNormalizer.BindingNotSupported).ToArray();

		// Each of the binding and the reference, said for the lexer and not for the buffer.
		Assert.Equal(
			[grammar.IndexOf("$q", StringComparison.Ordinal), grammar.LastIndexOf("$q", StringComparison.Ordinal)],
			refused.Select(static one => one.Position));
		Assert.All(refused, static one => Assert.Contains("leave Lexical off", one.Message, StringComparison.Ordinal));
	}

	// ── Helpers ─────────────────────────────────────────────────────────────────

	/// <summary>
	/// The grammar compiled with its reader and stream forms beside the string's; and whether the
	/// stream's is there, which it is not for a grammar whose classes are wider than a byte.
	/// </summary>
	static (Assembly Assembly, bool Bytes) Buffered(string grammar, CarrierKind carrier, bool direct)
	{
		foreach (var bytes in new[] { true, false })
		{
			var result = GramCompiler.Compile(grammar, new GramCompilerOptions
			{
				Carrier       = carrier,
				Direct        = direct,
				BufferedInput = true,
				BufferedBytes = bytes,
				CSharpScanner = RoslynCSharpScanner.Instance,
			});

			if (bytes && result.Diagnostics.Any(static one => one.Id == GramCompiler.BufferedUnsupported))
				continue;

			EmittedCode.Quiet(result.Diagnostics);

			var assembly = EmittedCode.Compile(
				result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static source => source.Text));

			return (assembly, bytes);
		}

		throw new InvalidOperationException("Unreachable: the form without bytes is always tried.");
	}

	/// <summary>
	/// One input read through a reader and a stream in pieces of every size, and from bytes held
	/// whole, held to the string's reading of it: the answer, the value and the place always, and
	/// the words unless the row is worded apart over a buffer (<see cref="WordedApartOverABuffer"/>)
	/// — where they are held to the buffered reading of the whole input instead.
	/// </summary>
	static void AgreesOverABuffer(string row, Assembly assembly, string rule, string input, string how, bool bytes)
	{
		var method   = "TryParse" + rule;
		var whole    = EmittedCode.Match(assembly, "Grammar", method, input);
		var buffered = Over(assembly, method, input, typeof(TextReader), BufferSizes[^1]);
		var worded   = !WordedApartOverABuffer.Contains(row);

		void Same((bool IsSuccess, object? Value, string? Error, long Position) read, string over)
		{
			var expected = worded ? Spelled(whole) : Spelled(buffered);

			Assert.True(
				(whole.IsSuccess, Spelled(whole.Value)) == (read.IsSuccess, Spelled(read.Value)) &&
				expected == Spelled(read),
				$"{row}, {how}, {over}: \"{Escaped(input)}\"\n  string: {Spelled(whole)}\n  " +
				(worded ? "" : $"buffer: {Spelled(buffered)}\n  ") + $"{over}: {Spelled(read)}");
		}

		foreach (var size in BufferSizes)
		{
			Same(Over(assembly, method, input, typeof(TextReader), size), $"a reader in pieces of {size}");

			if (bytes && IsAscii(input))
				Same(Over(assembly, method, input, typeof(Stream), size), $"a stream in pieces of {size}");
		}

		// A buffer that has grown still meets a refill boundary between every two items.
		foreach (var size in new[] { 1, 4096 })
		{
			Same(Over(assembly, method, input, typeof(TextReader), size, trickle: true), $"a reader handing one item a read, buffer {size}");

			if (bytes && IsAscii(input))
				Same(Over(assembly, method, input, typeof(Stream), size, trickle: true), $"a stream handing one item a read, buffer {size}");
		}

		if (bytes && IsAscii(input))
			Same(Over(assembly, method, input, typeof(byte[]), 0), "bytes held whole");
	}

	/// <summary>
	/// A feed on the immediate carrier, with its reader and stream forms: the assembly, and the
	/// source, which says whether the reader lets the window go.
	/// </summary>
	static (Assembly Assembly, string Source) Recovering(string grammar)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Carrier       = CarrierKind.Immediate,
			BufferedInput = true,
			BufferedBytes = true,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		var source = string.Join("\n", result.Sources.Select(static one => one.Text));

		var assembly = EmittedCode.Compile(
			result.Sources[0].Text,
			declarationMembers:
				"static string Text(string text) { return text; } " +
				"static string Text(global::System.ReadOnlySpan<char> text) { return text.ToString(); } " +
				"static string Text(global::System.ReadOnlySpan<byte> text) { return global::System.Text.Encoding.ASCII.GetString(text.ToArray()); }",
			sourceParts: result.Sources.Skip(1).Select(static one => one.Text));

		return (assembly, source);
	}

	/// <summary>
	/// A <c>Try…</c> form over a reader, a stream or bytes held whole, handed the input in pieces
	/// of <paramref name="size"/>; the value read back as text, as the string form hands it.
	/// </summary>
	/// <param name="retained">The most the buffer may hold, or null for the default.</param>
	/// <param name="trickle">Whether the source hands over one item per read, whatever room it is given.</param>
	static (bool IsSuccess, object? Value, string? Error, long Position) Over(
		Assembly assembly, string method, string input, Type over, int size, int? retained = null, bool trickle = false)
	{
		var type = assembly.GetType("Grammar")!;
		object match;

		if (over == typeof(byte[]))
			match = type.GetMethod(method, [typeof(byte[])])!.Invoke(null, [Encoding.ASCII.GetBytes(input)])!;
		else
		{
			using var source = Source(input, over, trickle);

			match = type.GetMethod(method, [over, typeof(int?), typeof(int?)])!.Invoke(null, [source, size, retained])!;
		}

		object? Read(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		var value = Read("Value") is byte[] read ? Encoding.ASCII.GetString(read) : Read("Value");

		return ((bool)Read("IsSuccess")!, value, (string?)Read("Error"), Convert.ToInt64(Read("Position")));
	}

	/// <summary>Where a <c>find</c> over a reader or a stream in pieces of <paramref name="size"/> finds each occurrence.</summary>
	static long[] FoundOver(Assembly assembly, string method, string text, Type over, int size, bool trickle = false)
	{
		using var source = Source(text, over, trickle);

		var found = (System.Collections.IEnumerable)assembly.GetType("Grammar")!
			.GetMethod(method, [over, typeof(int?), typeof(int?)])!
			.Invoke(null, [source, size, null])!;

		var positions = new List<long>();

		foreach (var match in found)
			positions.Add(Convert.ToInt64(match!.GetType().GetProperty("Position")!.GetValue(match)));

		return [.. positions];
	}

	/// <summary>A reader or a stream over the input, handing over all it is asked for or one item a read.</summary>
	static IDisposable Source(string input, Type over, bool trickle)
	{
		if (over == typeof(TextReader))
			return trickle ? new OneAtATimeReader(input) : new StringReader(input);

		var bytes = Encoding.ASCII.GetBytes(input);

		return trickle ? new OneAtATimeStream(bytes) : new MemoryStream(bytes);
	}

	/// <summary>A reader that hands over at most one character per read, as a slow pipe may.</summary>
	sealed class OneAtATimeReader(string text) : TextReader
	{
		int _at;

		public override int Peek()
		{
			return _at < text.Length ? text[_at] : -1;
		}

		public override int Read()
		{
			return _at < text.Length ? text[_at++] : -1;
		}

		public override int Read(char[] buffer, int index, int count)
		{
			if (count == 0 || _at == text.Length)
				return 0;

			buffer[index] = text[_at++];

			return 1;
		}
	}

	/// <summary>A stream that hands over at most one byte per read.</summary>
	sealed class OneAtATimeStream(byte[] bytes) : Stream
	{
		int _at;

		public override bool CanRead => true;

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length => bytes.Length;

		public override long Position
		{
			get => _at;
			set => throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (count == 0 || _at == bytes.Length)
				return 0;

			buffer[offset] = bytes[_at++];

			return 1;
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
	}

	static string Spelled((bool IsSuccess, object? Value, string? Error, long Position) match)
	{
		return (match.IsSuccess, Spelled(match.Value), match.Error, match.Position).ToString();
	}

	static bool IsAscii(string text)
	{
		return text.All(static one => one < 128);
	}
}
