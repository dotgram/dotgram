using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every place a streamed reading hands out is a place in the whole input, however much of it the
/// window has let go of: where a located value was read, where the marks over a construction were
/// placed, and where a rejected element stopped.
/// </summary>
/// <remarks>
/// A streamed reading hands the machine what its window holds, and the window drops what was read
/// before it. A place counted in what the machine was handed is then a place in the window — the
/// third entry of a configuration said 0 where the string says 15.
/// </remarks>
public sealed class StreamedPositionTests
{
	static string Source(string carrier)
	{
		return $$""""
			using DotGram;

			namespace StreamedLocationProbe
			{
				public interface ILoc
				{
					void Locate(int at, int length);

					void Locate(int at, int length, int gapStart);
				}

				public sealed class Key : ILoc
				{
					public Key(string name)
					{
						Name = name;
					}

					public string Name { get; }

					public int At = -1;

					public int Length = -1;

					public int Gap = -1;

					public void Locate(int at, int length)
					{
						At     = at;
						Length = length;
					}

					public void Locate(int at, int length, int gapStart)
					{
						At     = at;
						Length = length;
						Gap    = gapStart;
					}

					public override string ToString()
					{
						return Name + "@" + At + "+" + Length + "/" + Gap;
					}
				}

				public sealed class Entry : ILoc
				{
					public Entry(Key key, string value)
					{
						Key   = key;
						Value = value;
					}

					public Key Key { get; }

					public string Value { get; }

					public int At = -1;

					public int Length = -1;

					public int Gap = -1;

					public void Locate(int at, int length)
					{
						At     = at;
						Length = length;
					}

					public void Locate(int at, int length, int gapStart)
					{
						At     = at;
						Length = length;
						Gap    = gapStart;
					}

					public override string ToString()
					{
						return Key + "=" + Value + "@" + At + "+" + Length + "/" + Gap;
					}
				}

				[Gram("""
					trivia = { ' '* }

					Config : @Entry[] = Entry*

					Entry : @Entry = k: Name & '=' & v: Word & eol => @(new Entry(k, v))

					Name : @Key = w: Word => @(new Key(w))

					Word : @string = t: ['a'..'z']+ => @(t)

					parse Config
					find Name as Names
					""", Carrier = GramCarrier.{{carrier}}, LocationType = typeof(ILoc))]
				public static partial class P
				{
				}
			}
			"""";
	}

	/// <summary>
	/// A located value read through a window that has let go of what came before it is told where
	/// it stands in the input, over a reader and over a sequence of lines, by <c>parse</c> and by
	/// <c>find</c>. A window is read on the tape whatever carrier is asked for (GRAM5007).
	/// </summary>
	[Theory]
	[InlineData("Auto")]
	[InlineData("Tape")]
	[InlineData("Immediate")]
	public void A_located_value_read_through_a_window_is_told_its_place_in_the_input(string carrier)
	{
		var host = GeneratorDriverTests.Build(Source(carrier), permittedWarning: "GRAM5007").GetType("StreamedLocationProbe.P")!;

		// Past the window a reader starts with, so that what was read first has been let go of.
		var text  = string.Concat(Enumerable.Range(0, 1_500).Select(static i => new string(' ', i % 3) + "k" + (char)('a' + i % 26) + " = v\n"));
		var lines = text.Split('\n')[..^1];

		var whole = Said(host, "ParseConfig", typeof(string), text);

		Assert.Equal(1_500, whole.Length);
		Assert.Equal("kr@11993+2/-1=v@11993+7/-1", whole[^1]);
		Assert.Equal(whole, Said(host, "ParseConfig", typeof(TextReader), new StringReader(text)));
		Assert.Equal(whole, Said(host, "ParseConfig", typeof(IEnumerable<string>), lines.Select(static one => one)));

		var found = Said(host, "Names", typeof(string), text);

		Assert.Equal(3_000, found.Length);
		Assert.Equal(found, Said(host, "Names", typeof(TextReader), new StringReader(text)));
	}

	/// <summary>
	/// Where each mark over a construction was placed (<c>parserMarks</c>), read through a window that
	/// ends at every offset of the input.
	/// </summary>
	[Fact]
	public void The_marks_over_a_construction_read_through_a_window_are_placed_in_the_input()
	{
		var parser = WindowEdge.Compile("""
			state : @int
			Rows : @string[] = Row* recover eol => @("bad") & eof
			Row  : @string   = '(' & t: Item with state @(1) & ')' & eol => @(t)
			Item : @string   = t: Word => @(string.Join(",", parserMarks.ToArray()) + ":" + t)
			Word : @string   = t: ['a'..'z']+ => @(t)
			parse Rows
			""");

		var text  = "(ab)\n(cd)\n(x1)\n(ef)\n";
		var whole = WindowEdge.Said(() => parser.Call("ParseRows", typeof(string), text));
		var found = new WindowEdge.Mismatches();

		Assert.Equal("\"1:ab\" | \"6:cd\" | \"bad\" | \"16:ef\" | ", whole);

		WindowEdge.Edges(parser, found, "reader", text, text.Length, whole,
			() => parser.Call("ParseRows", typeof(TextReader), new StringReader(text)));

		found.Assert();
	}

	/// <summary>
	/// Where a rejected element stopped, where it stopped at its very start inside a look: the reading
	/// over a reader said column -6 of line 8, and at a window holding the whole input line 1, where the
	/// string says the start of line 8. The element's reach began at nought rather than where it began.
	/// </summary>
	[Fact]
	public void A_rejected_element_refused_inside_a_look_stops_where_it_began()
	{
		var parser = WindowEdge.Compile("""
			Rows : @string[] = Row* recover eol => @("bad " + parserFailureLine + ":" + parserFailureColumn + ":" + parserFailurePosition) & eof
			Row  : @string   = t: Word & eol => @(t)
			Word : @string   = ?=(['a'..'d']+ & eol) & t: ['a'..'d']+ => @(t)
			parse Rows
			""");

		var text  = "bcd\nbcd\nbcd\nbcd\nab\nabc\nab\n1x\nab\n";
		var whole = WindowEdge.Said(() => parser.Call("ParseRows", typeof(string), text));
		var found = new WindowEdge.Mismatches();

		Assert.Contains("\"bad 8:1:26\"", whole, StringComparison.Ordinal);

		WindowEdge.Edges(parser, found, "reader", text, text.Length, whole,
			() => parser.Call("ParseRows", typeof(TextReader), new StringReader(text)));
		WindowEdge.Edges(parser, found, "sequence", text, text.Length, whole,
			() => parser.Call("ParseRows", typeof(IEnumerable<string>), text.Split('\n')[..^1].Select(static one => one)));

		found.Assert();
	}

	/// <summary>A located value of the host's, which says where it was told it stands.</summary>
	const string Host = """
		public interface ILoc
		{
			void Locate(int at, int length);
		}

		public sealed class N : ILoc
		{
			public N(string name) { Name = name; }

			public string Name { get; }
			public int At { get; private set; } = -1;
			public int Length { get; private set; } = -1;

			public void Locate(int at, int length) { At = at; Length = length; }
		}
		""";

	/// <summary>
	/// Rows past the first window, then a bad row the synchronization of which holds a bad part of its
	/// own: the guard accepts it only where its places are the input's, and the row after it is read.
	/// </summary>
	static readonly string Nested = string.Concat(Enumerable.Repeat("a\n", 3_000)) + "z#y;\na\n";

	/// <summary>
	/// Grammars whose host code is handed a place by something other than the walk that builds the
	/// values: a guard's own span beside a located capture it compares with, a guard run by the probe
	/// of what follows a recovering repetition, a recovery factory's span, and a guard's span in a
	/// <c>find</c>. Each with a short input
	/// the oracle puts the window's end at every offset of, and a long one past the first window.
	/// </summary>
	static readonly (string Name, string Method, string Grammar, string Short, string Long)[] Handed =
	[
		("guard", "ParseRows", """
			Name : @N = 'a' => @(new N("a"))
			Row  : @N = n: Name & when @(n.At == parserSpan.Start) & eol => @(n)
			Rows : @N[] = Row*
			parse Rows
			""", "a\na\na\na\n", string.Concat(Enumerable.Repeat("a\n", 3_000))),
		("continuation", "ParseRows", """
			Row  : @N = 'a' & eol => @(new N("a"))
			Name : @N = 'z' => @(new N("z"))
			Tail : @N = n: Name & when @(n.At > 0) & eol => @(n)
			Rows : @N[] = Row* recover eol => @(new N("bad")) & Tail
			parse Rows
			""", "a\na\na\na\nz\n", string.Concat(Enumerable.Repeat("a\n", 3_000)) + "z\n"),
		("recovery", "ParseRows", """
			Row  : @N = 'a' & eol => @(new N("a"))
			Rows : @N[] = Row* recover eol => @(new N(parserSpan.Start + "+" + parserSpan.Length))
			parse Rows
			""", "a\na\na\na\nz\n", string.Concat(Enumerable.Repeat("a\n", 3_000)) + "z\na\n"),
		("find", "FindHit", """
			Name : @N = 'a' => @(new N("a"))
			Hit  : @N = n: Name & when @(n.At == parserSpan.Start) & 'b' => @(n)
			find Hit
			""", "xab\nab\n", string.Concat(Enumerable.Repeat("xab\n", 1_500))),
		// A recovery inside the synchronization of another, which the engine reads: its span and its
		// position, and a guard that holds them to the places in the input, which the string reads.
		("nested", "ParseRows", """
			Part : @string = 'x' & ';' => @("x")
			Sync : @string[] = '#' & items: Part* recover ';' => @("bad " + parserSpan.Start) & eol & when @(items.Length == 1 && items[0] == "bad 6002") => @(items)
			Row  : @string = 'a' & eol => @("a")
			Rows : @string[] = Row* recover Sync => @(parserSpan.Start + "+" + parserSpan.Length)
			parse Rows
			""", Nested, Nested),
		("nestedplaces", "ParseRows", """
			Part : @string = 'x' & ';' => @("x")
			Sync : @string[] = '#' & items: Part* recover ';' => @("bad " + parserPosition + "/" + parserFailurePosition + "/" + parserLine + ":" + parserColumn + "/" + parserFailureLine + ":" + parserFailureColumn) & eol & when @(items.Length == 1 && items[0] == "bad 6002/6002/3001:3/3001:3") => @(items)
			Row  : @string = 'a' & eol => @("a")
			Rows : @string[] = Row* recover Sync => @(parserSpan.Start + "+" + parserSpan.Length)
			parse Rows
			""", Nested, Nested),
		// A root with trivia, whose whole reading over a string leaves the trivia out of what it is
		// told (Failure.Began, Ended), read through a window instead: its rows, their guards and their
		// located values are read as parts, past the trivia and past the dropped prefix.
		("wholeroot", "ParseRows", """
			trivia = { ' '* }
			Name : @N = 'a' => @(new N("a"))
			Row  : @N = n: Name & when @(n.At == parserSpan.Start) & eol => @(n)
			Rows : @N[] = Row*
			parse Rows
			""", " a\n  a\na\n", string.Concat(Enumerable.Repeat("  a\n", 1_500))),
		// The same synchronization published as well, whose own reading steps over its bad part: the
		// engine still reads it inside the other's, out of the window.
		("nestedtwice", "ParseRows", """
			Part : @string = 'x' & ';' => @("x")
			Sync : @string[] = '#' & items: Part* recover ';' => @(parserPosition.ToString()) & eol & when @(items.Length == 1 && items[0] == "6002") => @(items)
			Row  : @string = 'a' & eol => @("a")
			Rows : @string[] = Row* recover Sync => @("bad")
			parse Rows
			parse Sync
			""", Nested, Nested),
	];


	public static TheoryData<string, bool, CarrierKind> HandedCases()
	{
		var cases = new TheoryData<string, bool, CarrierKind>();

		foreach (var (name, _, _, _, _) in Handed)
			foreach (var direct in new[] { false, true })
				foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate })
					cases.Add(name, direct, carrier);

		return cases;
	}

	/// <summary>
	/// Each of <see cref="Handed"/> read as the string reads it: through a window that ends at every
	/// offset of the short input, through windows of 1, 4 and the default over the long one, over a
	/// sequence of lines, and through a buffer of 1, 4 and the default over a reader and a stream.
	/// </summary>
	[Theory]
	[MemberData(nameof(HandedCases))]
	public void A_place_handed_to_the_hosts_code_is_a_place_in_the_input(string name, bool direct, CarrierKind carrier)
	{
		var (_, method, grammar, brief, length) = Handed.Single(one => one.Name == name);
		var parser = Located(grammar, direct, carrier, buffered: false);
		var found  = new WindowEdge.Mismatches();

		Assert.True(parser.Streams, name + " is not read through a window");

		var whole = WindowEdge.Said(() => parser.Call(method, typeof(string), brief));

		// The nested rows hold their guard to places of the input: over the string it holds.
		if (name.StartsWith("nested", StringComparison.Ordinal))
			Assert.EndsWith(name == "nestedtwice" ? "\"bad\" | \"a\" | " : "\"6000+1\" | \"a\" | ", whole, StringComparison.Ordinal);

		WindowEdge.Edges(parser, found, "reader", brief, brief.Length, whole,
			() => parser.Call(method, typeof(TextReader), new StringReader(brief)));
		WindowEdge.Edges(parser, found, "sequence", brief, brief.Length, whole,
			() => parser.Call(method, typeof(IEnumerable<string>), brief.Split('\n')[..^1].Select(static one => one)));

		var failures = new StringBuilder();

		foreach (var text in new[] { brief, length })
		{
			var said = WindowEdge.Said(() => parser.Call(method, typeof(string), text));

			foreach (var capacity in new[] { 1, 4, 4096 })
			{
				parser.Capacity = capacity;

				Differs(failures, $"reader, window {capacity}", text, said, WindowEdge.Said(() => parser.Call(method, typeof(TextReader), new StringReader(text))));
			}

			parser.Capacity = 4096;

			Differs(failures, "sequence", text, said, WindowEdge.Said(() => parser.Call(method, typeof(IEnumerable<string>), text.Split('\n')[..^1].Select(static one => one))));
		}

		Assert.True(failures.Length == 0, $"{name}, direct {direct}, {carrier}:\n{failures}");
		found.Assert();

		// The same grammar with its reader forms read through a buffer, which holds what it read.
		var buffered = Located(grammar, direct, carrier, buffered: true);

		foreach (var text in new[] { brief, length })
		{
			var said = WindowEdge.Said(() => parser.Call(method, typeof(string), text));

			Differs(failures, "buffered string", text, said, WindowEdge.Said(() => buffered.Call(method, typeof(string), text)));

			foreach (var size in new int?[] { 1, 4, null })
			{
				Differs(failures, $"reader, buffer {size}", text, said, Buffered(buffered, "Try" + method, typeof(TextReader), new StringReader(text), size));
				Differs(failures, $"stream, buffer {size}", text, said, Buffered(buffered, "Try" + method, typeof(Stream), new MemoryStream(Encoding.UTF8.GetBytes(text)), size));
			}
		}

		Assert.True(failures.Length == 0, $"{name}, direct {direct}, {carrier}, buffered:\n{failures}");
	}

	static void Differs(StringBuilder failures, string form, string text, string expected, string? actual)
	{
		if (actual is not null && actual != expected)
			failures.Append($"{form}, {text.Length} characters:\n  string {Shown(expected)}\n  form   {Shown(actual)}\n");
	}

	static string Shown(string said)
	{
		return said.Length <= 300 ? said : said[..150] + " … " + said[^150..];
	}

	/// <summary>What a buffered form reads, said as the other forms are, or null where there is none.</summary>
	static string? Buffered(WindowEdge.Parser parser, string name, Type over, object input, int? buffer)
	{
		if (parser.Type.GetMethod(name, [over, typeof(int?), typeof(int?)]) is not { } method)
			return null;

		return WindowEdge.Said(() =>
		{
			var match = method.Invoke(null, [input, buffer, null])!;
			var type  = match.GetType();

			return (bool)type.GetProperty("IsSuccess")!.GetValue(match)!
				? type.GetProperty("Value")!.GetValue(match)
				: throw new FormatException(type.GetProperty("Error")!.GetValue(match) + " at " + type.GetProperty("Position")!.GetValue(match) + ".");
		});
	}

	/// <summary>
	/// The parser of a grammar whose values implement <c>ILoc</c>, under a real symbol resolver; its
	/// reader forms read through a window, or with <paramref name="buffered"/> through a buffer, over
	/// characters and over bytes.
	/// </summary>
	static WindowEdge.Parser Located(string grammar, bool direct, CarrierKind carrier, bool buffered)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			Direct         = direct,
			Carrier        = carrier,
			BufferedInput  = buffered,
			BufferedBytes  = buffered,
			LocationType   = "ILoc",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = new RoslynSymbolResolver(
				CSharpCompilation.Create(
					"Located",
					[CSharpSyntaxTree.ParseText($"public partial class Grammar {{ {Host} }}")],
					EmittedCode.References,
					new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)),
				"Grammar"),
		});

		// A buffered form the grammar cannot have is told and left out, as the other forms are not.
		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error && one.Id != GramCompiler.BufferedUnsupported);

		return new WindowEdge.Parser(result.Sources[0].Text, "Grammar", null, Host, [.. result.Sources.Skip(1).Select(static one => one.Text)]);
	}

	/// <summary>
	/// A recovery no window reaches is told nothing of one: a grammar whose streamed publication reaches
	/// no recovery, beside a publication that recovers and is read over a string, a buffer or a stream
	/// of its own, carries no origin and writes none, as before it could.
	/// </summary>
	[Theory]
	[InlineData(false, CarrierKind.Auto)]
	[InlineData(true, CarrierKind.Auto)]
	[InlineData(false, CarrierKind.Tape)]
	[InlineData(true, CarrierKind.Immediate)]
	public void A_recovery_no_window_reaches_is_told_no_origin(bool direct, CarrierKind carrier)
	{
		var parser = Located("""
			Part : @string = 'x' & ';' => @("x")
			Inner : @string[] = '[' & items: Part* recover ';' => @(parserLine.ToString()) & ']' => @(items)
			Bad  : @string[] = '(' & i: Inner & ')' => @(i)
			Row  : @string = 'a' & eol => @("a")
			Rows : @string[] = Row*
			parse Rows
			parse Bad
			""", direct, carrier, buffered: false);

		Assert.True(parser.Streams);
		Assert.DoesNotContain("Origin", parser.Source, StringComparison.Ordinal);
	}

	/// <summary>
	/// A guard of the root that reads the root's extent is told, over a string, the root from where it
	/// began past the trivia; a streamed parse would read the guard on its own, from where it stands,
	/// and so the root is not streamed (GRAM5001), as the root builds no value either.
	/// </summary>
	[Fact]
	public void A_guard_of_the_root_reading_its_extent_is_not_streamed()
	{
		var result = GramCompiler.Compile("""
			trivia = { ' '* }
			Row  : @string = 'a' & eol => @("a")
			Rows : @string[] = Row* & when @(parserSpan.Start <= 2)
			parse Rows
			""", new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.DoesNotContain("TextReader input", result.Sources[0].Text, StringComparison.Ordinal);
		Assert.Contains(result.Diagnostics, static one => one.Id == "GRAM5001" && one.Message.Contains("reads the rule's extent", StringComparison.Ordinal));
	}

	/// <summary>Each value handed over, and for a match where it was found and what it holds.</summary>
	static string[] Said(Type host, string method, Type over, object input)
	{
		var values = host.GetMethod(method, [over])!.Invoke(null, [input])!;

		return [.. ((IEnumerable)values).Cast<object>().Select(static one => one.GetType().GetProperty("Position") is { } position
			? position.GetValue(one) + ":" + one.GetType().GetProperty("Value")!.GetValue(one)
			: one.ToString()!)];
	}
}
