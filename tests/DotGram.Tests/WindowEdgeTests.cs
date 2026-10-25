using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A reading fed by a window never takes the window's end for the end of the input: on a match, on a
/// refusal, and on a refusal a lookahead swallows (<c>?=</c>) or turns into a match (<c>?!</c>).
/// </summary>
/// <remarks>
/// <para>
/// The streamed forms read through a window of the input and read an element again, over a longer
/// window, wherever its reading met the window's end. What says it met the end is the failure's
/// <c>Starved</c>, which every read of the machine that runs into the end sets, inside a lookahead
/// too — but a scanned rule said so only on a match. Its refusal handed back how far it reached, and a
/// lookahead's caller throws that away: the lookahead failed quietly at its own start, nothing said the
/// window had ended, and the element was answered as though the input had. A record whose lookahead
/// reads across the window's end was then refused by the reader and read by the string, or, under
/// <c>?!</c>, read by the reader and refused by the string.
/// </para>
/// <para>
/// The oracle (<see cref="WindowEdge"/>) runs here over the snapshots that stream; over many small
/// grammars of records, under <c>eos</c> and <c>eol</c>, it runs in DotGram.Tests.Slow.
/// </para>
/// </remarks>
public sealed class WindowEdgeTests
{
	/// <summary>Records, each a word and <c>;</c>, and a look past all of them to the <c>STOP</c> that ends them.</summary>
	[Fact]
	public void A_lookahead_over_a_scanned_rule_looks_past_the_window_end()
	{
		var parser = WindowEdge.Compile("""
			Rows  : @string[] = Row* recover ';' => @("bad " + parserFailureColumn) & ("STOP" & ';')? & eof
			Row   : @string   = t: Word & ';' & ?=Ahead => @(t)
			Ahead = (Word & ';')* & "STOP" & ';'
			Word  = ['a'..'z']+
			parse Rows
			""");

		var text = string.Concat(Enumerable.Range(0, 5_000).Select(static i => "w" + (char)('a' + i % 26) + ";")) + "STOP;";

		Assert.Contains("Scan_Ahead", parser.Source, StringComparison.Ordinal);

		var whole    = Strings(parser.Call("ParseRows", typeof(string), text));
		var streamed = Strings(parser.Call("ParseRows", typeof(TextReader), new StringReader(text)));

		Assert.DoesNotContain(whole, static one => one.StartsWith("bad", StringComparison.Ordinal));
		Assert.Equal(whole, streamed);
	}

	/// <summary>The same records under a look that must not see an empty record ahead: the reader may not accept.</summary>
	[Fact]
	public void A_negative_lookahead_over_a_scanned_rule_does_not_accept_at_the_window_end()
	{
		var parser = WindowEdge.Compile("""
			Rows : @string[] = Row* recover ';' => @("bad " + parserFailureColumn) & ';'? & eof
			Row  : @string   = t: Word & ';' & ?!Bad => @(t)
			Bad  = (Word & ';')* & ';'
			Word = ['a'..'z']+
			parse Rows
			""");

		var text = string.Concat(Enumerable.Range(0, 5_000).Select(static i => "w" + (char)('a' + i % 26) + ";")) + ";";

		Assert.Contains("Scan_Bad", parser.Source, StringComparison.Ordinal);

		var whole    = Strings(parser.Call("ParseRows", typeof(string), text));
		var streamed = Strings(parser.Call("ParseRows", typeof(TextReader), new StringReader(text)));

		Assert.All(whole, static one => Assert.StartsWith("bad", one, StringComparison.Ordinal));
		Assert.Equal(whole, streamed);
	}

	/// <summary>A lookahead that reads many segments ahead answers as the list does.</summary>
	[Fact]
	public void A_lookahead_past_many_segments_reads_as_the_list_does()
	{
		var parser = WindowEdge.Compile("""
			Rows   : @string[] = Row* recover eos => @("bad " + parserLine) & ("STOP" & eos)? & eof
			Row    : @string   = t: Word & eos & ?=Ahead => @(t)
			Ahead  = (Word & eos)* & "STOP" & eos
			Word   = ['a'..'z']+
			parse Rows
			""");

		var segments = Enumerable.Range(0, 5_000).Select(static i => "w" + (char)('a' + i % 26)).Append("STOP").ToArray();
		var listed   = Strings(parser.Call("ParseRows", typeof(IReadOnlyList<string>), segments));
		var streamed = Strings(parser.Call("ParseRows", typeof(IEnumerable<string>), segments.Select(static one => one)));
		var read     = Strings(parser.Call("ParseRows", typeof(TextReader), new StringReader(string.Join("\n", segments))));

		Assert.DoesNotContain(listed, static one => one.StartsWith("bad", StringComparison.Ordinal));
		Assert.Equal(listed, streamed);
		Assert.Equal(listed, read);
	}

	/// <summary>
	/// A look inside a scanned rule, at a literal the window ends inside: <c>Head</c> refuses <c>abc</c>, and
	/// a window that ends after <c>ab</c> may not let it read <c>a</c> and refuse the record somewhere else.
	/// </summary>
	[Fact]
	public void A_look_inside_a_scanned_rule_reads_past_the_window_end()
	{
		var parser = WindowEdge.Compile("""
			Rows : @string[] = Row* recover eol => @("bad " + parserLine + ":" + parserFailureColumn) & eof
			Row  : @string   = t: Head & '1' & eol => @(t)
			Head = ?!"abc" & ['a'..'z']
			parse Rows
			""");

		var text  = "a1\nabc1\nb1\nab1\n";
		var found = new WindowEdge.Mismatches();

		Assert.Contains("Scan_Head", parser.Source, StringComparison.Ordinal);

		WindowEdge.Edges(parser, found, "reader", text, text.Length, WindowEdge.Said(() => parser.Call("ParseRows", typeof(string), text)),
			() => parser.Call("ParseRows", typeof(TextReader), new StringReader(text)));

		found.Assert();
	}

	/// <summary>The snapshots with a streamed form.</summary>
	static readonly string[] Streamed = ["Feed", "Located", "Minimal", "Notation", "Split", "Url"];

	public static TheoryData<string> Snapshots()
	{
		return [.. Streamed];
	}

	/// <summary>
	/// Every streamed form of the snapshots, read with the window's end at every offset of what they are
	/// handed, answers as the same form over the whole string does.
	/// </summary>
	[Theory]
	[MemberData(nameof(Snapshots))]
	public void Every_window_end_reads_a_snapshot_as_its_string(string name)
	{
		var text = File.ReadAllText(Path.Combine(SnapshotDirectory, name + ".gram"));
		var (options, declarations) = SnapshotTests.Renderings(name, text).First();
		var result = GramCompiler.Compile(text, options);
		var parser = new WindowEdge.Parser(result.Sources[0].Text, options.ClassName, options.Namespace, declarations,
			[.. result.Sources.Skip(1).Select(static source => source.Text)]);
		var found  = new WindowEdge.Mismatches();

		Assert.True(parser.Streams);

		foreach (var method in parser.Type.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Where(static one => one.GetParameters() is [{ } only] && only.ParameterType == typeof(TextReader))
			.Select(static one => one.Name)
			.Distinct()
			.OrderBy(static one => one, StringComparer.Ordinal))
		{
			foreach (var input in SnapshotInputs(name))
			{
				var whole = Unplaced(name, WindowEdge.Said(() => parser.Call(method, typeof(string), input)));

				WindowEdge.Edges(parser, found, method, WindowEdge.Show([input]), input.Length, whole,
					() => parser.Call(method, typeof(TextReader), new StringReader(input)), one => Unplaced(name, one));
			}
		}

		found.Assert();
	}

	/// <summary>
	/// What every streaming snapshot writes: wherever a scanner is told of the window, its refusal says
	/// whether it reached the end, the same as its match does.
	/// </summary>
	[Fact]
	public void Every_scanner_that_is_told_of_the_window_says_so_when_it_refuses()
	{
		var scanners = 0;
		var untold   = new List<string>();

		foreach (var name in Streamed.Append("Buffered"))
		{
			var (some, these) = WindowEdge.Untold(File.ReadAllText(Path.Combine(SnapshotDirectory, name + ".gram.g.cs")));

			scanners += some;
			untold.AddRange(these.Select(one => name + ": " + one));
		}

		Assert.True(scanners > 0, "no scanner was told of the window");
		Assert.Empty(untold);
	}

	/// <summary>
	/// What a reading of <c>Located</c> said, without the places its values were told: a streamed form
	/// tells them where they stand in the window, not in the input, which is a defect of its own and not
	/// the window's end this class holds.
	/// </summary>
	static string Unplaced(string name, string said)
	{
		return name == "Located" ? Regex.Replace(said, @"\bAt=\d+", "At=?") : said;
	}

	/// <summary>Inputs that read and inputs that are refused, of each snapshot's streamed forms.</summary>
	static IEnumerable<string> SnapshotInputs(string name)
	{
		return name switch
		{
			"Feed"     =>
			[
				"H|2026-10-25\nR|ab c|12.50\nR|x|-3\nT|2\n",
				"H|2026-10-25\r\nR|abc|1\r\nR|q|7.5\r\nR|r|12.25\r\nT|3",
				"R|a|1\nnoise R|b|2.00\nR||3\nR|c|\nR|d|4.\n",
			],
			"Located"  => ["a=1\nbb = two\n  ccc=\n", "a=1\n=2\nb=3", "key=value\nother=x y z\r\nlast=z"],
			"Minimal"  => ["ab;cd;e1;;fg;", "abc", "a;b;c", ";;", "abcd ab abcd a"],
			"Notation" => ["one two three", "Rows = Row* & eof\nRow = 'a' & eol\n", "a b\n// c\n"],
			"Split"    => ["let a = b; print c; call f(x); wait 12;", "while x { y } unless z { w } halt; skip;", "let a = ; print b;", "raise q; return r; wat;"],
			"Url"      => ["see http://a.example/x?y=1#z and https://b.example:8080/p/q", "ftp://f.example/ mailto:x@y.example http://", "http://[::1]:80/a http://a.example/%41%4"],
			_          => throw new ArgumentOutOfRangeException(nameof(name), name, null),
		};
	}

	static string[] Strings(object? value)
	{
		return [.. ((IEnumerable<string>)value!)];
	}

	static string SnapshotDirectory => Path.Combine(Path.GetDirectoryName(ThisFile)!, "..", "Snapshots");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "")
	{
		return path;
	}
}
