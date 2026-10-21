using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A <c>parse</c> read through a reader hands back what the same parse over a string does, for
/// every shape a streamed parse accepts, and a shape it would read differently gets no reader
/// (docs/syntax.md §6.3).
/// </summary>
/// <remarks>
/// <para>
/// The string is the oracle: what a grammar means is what it reads over one, and a stream is
/// the same grammar read a part at a time. Each shape is read on every carrier, with the methods
/// and without, through a window of four characters so that every part of it crosses a refill,
/// over the inputs written beside it and over input nobody chose, made of their characters.
/// Two readings agree when they hand back the same elements, or when both refuse; how a refusal
/// is worded is the stream's own (it names the part that failed).
/// </para>
/// <para>
/// The shapes are what <c>Retention.StreamedParse</c> lets through: parts read in order, each a
/// call, a repetition of one, or something that builds no value; at most one repetition marked
/// <c>recover</c>, anywhere among them; a repetition with a least and a most; the seam of a
/// spaced collection. Each was found reading differently, or stands next to one that was: an
/// ordinary repetition before the marked one was read as if it were marked, so the rest of the
/// rule took its elements for bad ones of its own; the least and the most of a repetition were
/// not held; nothing checked that the input ended after the last part.
/// </para>
/// </remarks>
public sealed class StreamedParseTests
{
	/// <summary>The rules a shape is read with: the ones it calls, and what they call.</summary>
	static string Rules(string start)
	{
		return "Word : @string = t: ['a'..'z']+ => @(t)\n" +
			(start.Contains("Line", StringComparison.Ordinal) ? "Line : @string = w: Word & eol => @(w)\n" : "") +
			(start.Contains("Row", StringComparison.Ordinal) ? "Row : @string = '[' & w: Word & ']' & eol => @(w)\n" : "") +
			start;
	}

	/// <summary>The same for a spaced collection: elements with the trivia between them.</summary>
	static string Spaced(string start)
	{
		return "trivia = { ' '* }\n" +
			"Word : @string = t: ['a'..'z']+ => @(t)\n" +
			"Item : @string = w: Word & ';' => @(w)\n" +
			(start.Contains("Row", StringComparison.Ordinal) ? "Row : @string = '[' & w: Word & ']' => @(w)\n" : "") +
			start;
	}

	public static TheoryData<string, string, string[]> Shapes => new()
	{
		// The rest of the rule recovers, so it can always be read: asked first, it took every
		// line for a bad row, and the lines were lost.
		{ "an ordinary repetition before the marked one", Rules("Start : @string[] = Line* & Row* recover eol"), ["a\nb\n[c]\n[9]\n[d]\n", "a\nb\n", "[c]\n9\n[d]\n", ""] },
		{ "the same with a factory on the marked one", Rules("Start : @string[] = Line* & Row* recover eol => @(\"!\" + parserText)"), ["a\nb\n[c]\n[9]\n[d]\n", "a\n[c]\n9\n"] },
		{ "the same through the seams of a spaced collection", Spaced("Start : @string[] = Item* & Row* recover ';'"), ["a ; b; [c] [9]; [d]", "[c] x ;"] },
		{ "the marked one first", Rules("Start : @string[] = Row* recover eol & Line*"), ["[a]\n[9]\nb\nc\n", "b\n"] },
		{ "a separator between", Rules("Start : @string[] = Line* & '-' & eol & Row* recover eol"), ["a\nb\n-\n[c]\n[9]\n[d]\n", "-\n[c]\n", "a\n-\n"] },
		{ "a trailer after the marked one", Rules("Start : @string[] = Line* & Row* recover eol & \"END\" & eol"), ["a\n[b]\n[9]\nEND\n", "END\n", "a\nEND\n"] },
		{ "a header and a trailer around an ordinary one", Rules("Start : @string[] = \"H\" & eol & Line* & \"T\" & eol"), ["H\na\nb\nT\n", "H\nT\n", "H\na\n9\nT\n"] },
		{ "a call before an ordinary one", Rules("Start : @string[] = Word & eol & Line* & eof"), ["h\na\nb\n", "h\na\n9"] },
		{ "nothing marked, input left over", Rules("Start : @string[] = Line* & Row*"), ["a\nb\n[c]\n[d]\n", "a\nb\n[c]\n9\n"] },
		{ "a least and a most on an ordinary one", Rules("Start : @string[] = Line{2,3} & Row*"), ["a\nb\n[c]\n", "a\nb\nc\nd\n[e]\n", "a\n[c]\n"] },
		{ "a least on an ordinary one before the marked one", Rules("Start : @string[] = Line+ & Row* recover eol"), ["a\nb\n[c]\n[9]\n[d]\n", "[c]\n"] },
		{ "a least on the marked one", Rules("Start : @string[] = Row+ recover eol"), ["", "[a]\n9\n[b]\n", "9\n"] },
		{ "a most on the marked one", Rules("Start : @string[] = Row{1,2} recover eol & Line*"), ["[a]\n[b]\nc\n", "[a]\n9\nc\n", "[a]\n[b]\n[c]\n"] },
		{ "a least on the marked one after an ordinary one", Rules("Start : @string[] = Line* & Row{2,} recover eol"), ["a\n[a]\n[b]\n", "a\n[a]\n", "a\n[a]\n9\n"] },
		{ "a least and a most through the seams", Spaced("Start : @string[] = Item{1,2} & '.'"), ["a; b; .", "a; b; c; .", "."] },
		{ "a group that builds nothing", Rules("Start : @string[] = Line* & ('#' & eol)*"), ["a\nb\n#\n#\n", "#\na\n"] },
		{ "a => that hands back the repetition it captured", Rules("Start : @string[] = \"H\" & eol & l: Line* & \"T\" & eol => @( l )"), ["H\na\nb\nT\n", "H\nT\n", "H\na\n9\nT\n"] },
		{ "an optional call", Rules("Hash : @string = '#' & w: Word & eol => @(w)\nStart : @string[] = Line* & Hash?"), ["a\nb\n#c\n", "a\n", "#c\n#d\n"] },
	};

	static readonly CarrierKind[] Carriers = [CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate];

	[Theory]
	[MemberData(nameof(Shapes))]
	public void Streams_what_a_string_reads(string name, string grammar, string[] rows)
	{
		var alphabet = rows.SelectMany(static row => row).Distinct().OrderBy(static c => c).ToArray();
		var random   = new Random(name.Length * 7919);
		var inputs   = rows
			.Concat(Enumerable.Range(0, 200)
				.Select(_ => new string([.. Enumerable.Range(0, random.Next(0, 16)).Select(_ => alphabet[random.Next(alphabet.Length)])])))
			.ToArray();

		foreach (var carrier in Carriers)
			foreach (var direct in new[] { true, false })
			{
				var host = Host(grammar, carrier, direct);

				foreach (var input in inputs)
				{
					var whole    = Whole(host, input);
					var streamed = Streamed(host, input);

					Assert.True(
						whole == streamed || whole.StartsWith('!') && streamed.StartsWith('!'),
						$"{name}, {carrier}, direct {direct}, [{input}]: string {whole}, reader {streamed}");
				}
			}
	}

	/// <summary>The two that were found, with the values they are meant to have.</summary>
	[Theory]
	[InlineData("Start : @string[] = Line* & Row* recover eol", "a\nb\n[c]\n[9]\n[d]\n", "a,b,c,d")]
	[InlineData("Start : @string[] = Line* & Row* recover eol", "a\nb\n", "a,b")]
	[InlineData("Start : @string[] = Line* & Row*", "a\nb\n[c]\n9\n", "!")]
	[InlineData("Start : @string[] = Line+ & Row* recover eol", "[c]\n", "!")]
	public void Reads_the_ordinary_repetition_as_one(string rule, string input, string expected)
	{
		foreach (var carrier in Carriers)
			foreach (var direct in new[] { true, false })
			{
				var streamed = Streamed(Host(Rules(rule), carrier, direct), input);

				Assert.True(
					expected == "!" ? streamed.StartsWith('!') : streamed == expected,
					$"{carrier}, direct {direct}: {streamed}");
			}
	}

	/// <summary>
	/// A rule a stream would read differently gets no reader, and says why; its twin, with the
	/// part made a rule of its own, gets one and reads what the string reads.
	/// </summary>
	/// <remarks>
	/// A part that is not a call is read whole by a recognizer of its own and hands nothing over,
	/// so a value collected inside it was missing from the stream: <c>[a,b]</c> where the string
	/// reads <c>[a,b,c,d]</c>. A rule that builds its own value with <c>=&gt;</c> had the stream
	/// hand over the elements as if there were none.
	/// </remarks>
	[Theory]
	[InlineData(
		"Start : @string[] = Line* & ('[' & Word & ']' & eol)*",
		"Start : @string[] = Line* & Row*",
		"collects values without being a call of a rule",
		"a\nb\n[c]\n[d]\n")]
	[InlineData(
		"Start : @string[] = Line* & ('[' & Word & ']' & eol)* recover eol",
		"Start : @string[] = Line* & Row* recover eol",
		"collects values without being a call of a rule",
		"a\nb\n[c]\n[9]\n[d]\n")]
	[InlineData(
		"Start : @string[] = Line* & ('#' & Word & eol)?",
		"Hash : @string = '#' & w: Word & eol => @(w)\nStart : @string[] = Line* & Hash?",
		"collects values without being a call of a rule",
		"a\nb\n#c\n")]
	[InlineData(
		"Start : @string[] = l: Line* => @(new[] { \"x\" })",
		null,
		"builds its value with '=>'",
		"a\nb\n")]
	public void Is_refused_a_reader_where_it_would_lose_values(string inline, string? twin, string why, string input)
	{
		var refused = Compiled(Rules(inline), CarrierKind.Auto, direct: true);

		Assert.Contains(
			refused.Diagnostics,
			one => one.Id == "GRAM5001" && one.Message.Contains(why, StringComparison.Ordinal));

		if (twin is null)
			return;

		var expected = Whole(Host(Rules(inline), CarrierKind.Auto, true, streams: false), input);

		foreach (var carrier in Carriers)
			foreach (var direct in new[] { true, false })
				Assert.Equal(expected, Streamed(Host(Rules(twin), carrier, direct), input));
	}

	/// <summary>
	/// A sequence of <c>T</c> does not collect a rule that builds a <c>T[]</c> (§4.1 case 2): the
	/// grammar is refused with GRAM4008, as a compilation refuses it. Without a compilation to ask,
	/// the array used to be taken for an element and the parser did not compile (CS0029).
	/// </summary>
	[Theory]
	[InlineData(
		"Block(Open, Close) : @string[] = (Open & Word & Close & eol)* recover eol\n" +
		"Start : @string[] = Block('[', ']') & '-' & eol & Block('(', ')')")]
	[InlineData(
		"Block : @string[] = ('[' & Word & ']' & eol)*\n" +
		"Start : @string[] = Block & '-' & eol")]
	public void An_array_is_not_collected_as_its_own_element(string rules)
	{
		var grammar = "Word : @string = t: ['a'..'z']+ => @(t)\n" + rules;

		foreach (var carrier in Carriers)
			Assert.Contains(Compiled(grammar, carrier, direct: true).Diagnostics, static one => one.Id == "GRAM4008");

		var host = "[DotGram.Gram(\"\"\"\"\n" + grammar + "\nparse Start\n\"\"\"\")]\npublic partial class Host { }\n";

		Assert.Contains(GeneratorDriverTests.RunGenerator(host).Diagnostics, static one => one.Id == "GRAM4008");
	}

	static GramCompilation Compiled(string grammar, CarrierKind carrier, bool direct)
	{
		return GramCompiler.Compile(grammar + "\nparse Start", new GramCompilerOptions
		{
			ClassName     = "Grammar",
			Carrier       = carrier,
			Direct        = direct,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
	}

	/// <summary>Compiled once per text: a carrier that yields to another writes the same source.</summary>
	static readonly ConcurrentDictionary<string, Type> Hosts = new();

	/// <summary>The parser, its window cut to four characters so that every part crosses a refill.</summary>
	static Type Host(string grammar, CarrierKind carrier, bool direct, bool streams = true)
	{
		var result = Compiled(grammar, carrier, direct);

		EmittedCode.Quiet(result.Diagnostics);

		Assert.Equal(!streams, result.Diagnostics.Any(static one => one.Id == "GRAM5001"));

		var source = result.Sources[0].Text;

		if (streams)
		{
			Assert.Contains("new Window(input, 4096)", source, StringComparison.Ordinal);

			source = source.Replace("new Window(input, 4096)", "new Window(input, 4)", StringComparison.Ordinal);
		}

		return Hosts.GetOrAdd(
			source + string.Concat(result.Sources.Skip(1).Select(static one => one.Text)),
			_ => EmittedCode.Compile(source, sourceParts: result.Sources.Skip(1).Select(static one => one.Text)).GetType("Grammar")!);
	}

	static string Whole(Type host, string input)
	{
		return Called(() => host.GetMethod("ParseStart", [typeof(string)])!.Invoke(null, [input]));
	}

	static string Streamed(Type host, string input)
	{
		var reader = host.GetMethod("ParseStart", [typeof(TextReader)]);

		Assert.NotNull(reader);

		return Called(() => reader.Invoke(null, [new StringReader(input)]));
	}

	/// <summary>The elements, joined by commas, or <c>!</c> and why it refused.</summary>
	static string Called(Func<object?> read)
	{
		try
		{
			return Show(read());
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException is FormatException refused)
		{
			return "!" + refused.Message;
		}
		catch (FormatException refused)
		{
			// A stream refuses as it is enumerated, past the reflected call.
			return "!" + refused.Message;
		}
	}

	static string Show(object? value)
	{
		return value switch
		{
			null              => "null",
			string text       => text,
			IEnumerable items => string.Join(",", items.Cast<object?>().Select(Show)),
			_                 => value.ToString() ?? "",
		};
	}
}
