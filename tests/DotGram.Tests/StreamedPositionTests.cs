using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

	/// <summary>Each value handed over, and for a match where it was found and what it holds.</summary>
	static string[] Said(Type host, string method, Type over, object input)
	{
		var values = host.GetMethod(method, [over])!.Invoke(null, [input])!;

		return [.. ((IEnumerable)values).Cast<object>().Select(static one => one.GetType().GetProperty("Position") is { } position
			? position.GetValue(one) + ":" + one.GetType().GetProperty("Value")!.GetValue(one)
			: one.ToString()!)];
	}
}
