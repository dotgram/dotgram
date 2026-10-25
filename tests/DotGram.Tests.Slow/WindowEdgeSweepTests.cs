using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The edge oracle (<see cref="WindowEdge"/>) over grammars of records: every streamed form, read with the
/// window's end at every offset of its first records, answers as the whole input does — the list of
/// segments under <c>eos</c>, the string under <c>eol</c>.
/// </summary>
/// <remarks>
/// The grammars are in <c>WindowEdgeShapes.txt</c>, each written over segments and read again over lines
/// (<see cref="Shape.AsLines"/>). Here rather than in DotGram.Tests because each is compiled twice and read
/// some thousand times: half a minute that every run of the ordinary tests would pay.
/// </remarks>
public sealed class WindowEdgeSweepTests
{
	public static TheoryData<string, string> Shapes()
	{
		var data = new TheoryData<string, string>();

		foreach (var shape in Shape.All())
		{
			data.Add(shape.Name, "eos");

			if (shape.Grammar.Contains("eos", StringComparison.Ordinal))
				data.Add(shape.Name, "eol");
		}

		return data;
	}

	[Theory]
	[MemberData(nameof(Shapes))]
	public void Every_window_end_reads_a_shape_as_the_whole_input(string name, string end)
	{
		var shape  = Shape.All().Single(one => one.Name == name);
		var eol    = end == "eol";
		var parser = WindowEdge.Compiled(eol ? shape.AsLines() : shape.Grammar, shape.Members);
		var random = new Random(name.Aggregate(17, static (h, c) => h * 31 + c) + (eol ? 1 : 0));
		var found  = new WindowEdge.Mismatches();

		// A shape may be one the compiler refuses, or one that is not streamed: neither has an edge.
		if (parser is null || !parser.Streams)
			return;

		for (var i = 0; i < 24; i++)
		{
			var segments = shape.Generate(random, eol);
			var text     = Join(segments, i % 3);
			var shown    = WindowEdge.Show(segments);

			// Over lines, the sequence ends every line, the last as well; the reader reads the text as it is.
			var whole = eol
				? WindowEdge.Said(() => parser.Call(shape.Method, typeof(string), text))
				: WindowEdge.Said(() => parser.Call(shape.Method, typeof(IReadOnlyList<string>), segments));
			var ended = eol
				? WindowEdge.Said(() => parser.Call(shape.Method, typeof(string), string.Concat(segments.Select(static one => one + "\n"))))
				: whole;

			if (segments.All(static one => !one.Contains('\n') && !one.Contains('\r')) && parser.Has(shape.Method, typeof(TextReader)))
				WindowEdge.Edges(parser, found, "reader", shown, text.Length, whole, () => parser.Call(shape.Method, typeof(TextReader), new StringReader(text)));

			if (parser.Has(shape.Method, typeof(IEnumerable<string>)))
				WindowEdge.Edges(parser, found, "sequence", shown, text.Length, ended, () => parser.Call(shape.Method, typeof(IEnumerable<string>), segments.Select(static one => one)));
		}

		found.Assert();
	}

	/// <summary>
	/// What every shape writes: wherever a scanner is told of the window, its refusal says whether it reached
	/// the end, the same as its match does.
	/// </summary>
	[Fact]
	public void Every_scanner_that_is_told_of_the_window_says_so_when_it_refuses()
	{
		var scanners = 0;
		var untold   = new List<string>();

		foreach (var shape in Shape.All())
		{
			foreach (var (name, grammar) in new[] { (shape.Name, shape.Grammar), (shape.Name + " over lines", shape.AsLines()) })
			{
				if (WindowEdge.Compiled(grammar, shape.Members) is not { Streams: true } parser)
					continue;

				var (some, these) = WindowEdge.Untold(parser.Source);

				scanners += some;
				untold.AddRange(these.Select(one => name + ": " + one));
			}
		}

		Assert.True(scanners > 0, "no scanner was told of the window");
		Assert.Empty(untold);
	}

	/// <summary>The segments as text, each ended as <paramref name="ending"/> picks: <c>\n</c>, <c>\r\n</c>, or the last left open.</summary>
	static string Join(string[] segments, int ending)
	{
		var builder = new StringBuilder();

		for (var i = 0; i < segments.Length; i++)
		{
			builder.Append(segments[i]);

			if (i == segments.Length - 1 && ending == 2 && segments[i].Length > 0)
				break;

			builder.Append(ending == 1 ? "\r\n" : "\n");
		}

		return builder.ToString();
	}

	/// <summary>
	/// A grammar of records, the method that reads them, and the records to make inputs of.
	/// </summary>
	/// <remarks>
	/// A record marked <c>^</c> comes first, one marked <c>$</c> last, and one marked <c>!</c> is one
	/// the grammar refuses. Written over segments (<c>eos</c>); <see cref="AsLines"/> is the same over
	/// lines, where a negated class does not read a line's end and <c>any</c> reads anything else.
	/// </remarks>
	sealed record Shape(string Name, string Grammar, string Method, string[] Samples, string? Members)
	{
		public static IReadOnlyList<Shape> All()
		{
			return _all ??= Read();
		}

		static IReadOnlyList<Shape>? _all;

		static IReadOnlyList<Shape> Read()
		{
			var text   = File.ReadAllText(Path.Combine(Path.GetDirectoryName(ThisFile)!, "WindowEdgeShapes.txt")).Replace("\r\n", "\n");
			var shapes = new List<Shape>();

			foreach (var block in text.Split("\n=== ", StringSplitOptions.RemoveEmptyEntries))
			{
				var named = block.StartsWith("=== ", StringComparison.Ordinal) ? block[4..] : block;
				var name  = named[..named.IndexOf('\n')].Trim();
				var parts = named[(named.IndexOf('\n') + 1)..].Split("\n---\n");

				shapes.Add(new Shape(
					name,
					parts[0].Trim() + "\n",
					parts[1].Trim(),
					[.. parts[2].Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(Unescape)],
					parts.Length > 3 ? parts[3].Trim() : null));
			}

			return shapes;
		}

		public string AsLines()
		{
			var lines = Regex.Replace(Grammar, @"\[\^ ([^\]]*)\]", "[^ $1 | '\\n' | '\\r']");

			lines = Regex.Replace(lines, @"\bany\b", "[^ '\\n' | '\\r']");

			return Regex.Replace(lines, @"\beos\b", "eol");
		}

		/// <summary>An input of records: a first, some of the rest and of the refused, a last.</summary>
		public string[] Generate(Random random, bool lines)
		{
			var usable = Samples.Where(one => !lines || !one.Contains('\n') && !one.Contains('\r')).ToArray();
			var heads  = usable.Where(static one => one.StartsWith('^')).Select(static one => one[1..]).ToArray();
			var tails  = usable.Where(static one => one.StartsWith('$')).Select(static one => one[1..]).ToArray();
			var bads   = usable.Where(static one => one.StartsWith('!')).Select(static one => one[1..]).ToArray();
			var bodies = usable.Where(static one => one.Length == 0 || !"^$!".Contains(one[0])).ToArray();
			var list   = new List<string>();

			if (heads.Length > 0 && random.Next(8) != 0)
				list.Add(heads[random.Next(heads.Length)]);

			for (var n = random.Next(0, 7); n > 0; n--)
			{
				if (bads.Length > 0 && random.Next(6) == 0)
					list.Add(bads[random.Next(bads.Length)]);
				else if (bodies.Length > 0)
					list.Add(bodies[random.Next(bodies.Length)]);
			}

			if (tails.Length > 0 && random.Next(8) != 0)
				list.Add(tails[random.Next(tails.Length)]);

			return [.. list];
		}

		static string Unescape(string text)
		{
			return text.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\0", "\0").Replace("\\u0001", "\u0001").Replace("\\e", "");
		}
	}

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "")
	{
		return path;
	}
}
