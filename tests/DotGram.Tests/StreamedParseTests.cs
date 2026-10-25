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
		var words = new[] { "Word", "Line", "Row", "Hash" }.Any(name => start.Contains(name, StringComparison.Ordinal));

		return (words ? "Word : @string = t: ['a'..'z']+ => @(t)\n" : "") +
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

	/// <summary>Two one-letter rules and the trivia between them, for what a whole parse allows around a rule.</summary>
	static string Letters(string start)
	{
		return "trivia = { ' '* }\n" +
			"A : @string = 'a' => @(\"a\")\n" +
			(start.Contains('B', StringComparison.Ordinal) ? "B : @string = 'b' => @(\"b\")\n" : "") +
			start;
	}

	public static TheoryData<string, string, string[]> Shapes => new()
	{
		// A whole parse reads the rule's trivia before it and after it, and so does the rest of
		// the rule the marked one asks: without it, the last element and the space after it were
		// stepped over as a bad one.
		{ "trivia around the rule", Letters("Start : @string[] = A? & B?"), ["a b ", " a b", " a", " ", "", "b a"] },
		{ "trivia after the marked one's continuation", Letters("Start : @string[] = A* recover ';' & B?"), ["a b ", "a x; b", "a a; b ", " a a b"] },
		{ "a => that hands back a spaced collection", Letters("Start : @string[] = xs: A* => @(xs)"), ["a a", " a a ", "", "a b"] },
		{ "a least on an ordinary one refused inside an element", Rules("Start : @string[] = Row{2,3}"), ["[a]\n[a", "[a]\n[b]\n", "[a]\n[b]\n[c]\n[d]\n", "[a]\n"] },
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
						whole == streamed || whole.StartsWith('!') && streamed.StartsWith('!') && Place(whole) == Place(streamed),
						$"{name}, {carrier}, direct {direct}, [{input}]: string {whole}, reader {streamed}");
				}
			}
	}

	/// <summary>
	/// A part that can end in two places — a choice whose earlier alternative is a prefix of a
	/// later one — is given back over a string when what follows fails, and a stream cannot go
	/// back into it. Such a rule either gets no reader, and is told why, or reads what the string
	/// reads, wherever the choice stands: a part of its own, the last part, a call, the element
	/// of a repetition, with trivia, beside a recover.
	/// </summary>
	[Theory]
	[InlineData("Start : @string[] = (\"a\" | \"ab\") & \"b\" & eol & Line*", true, new[] { "abb\nx\n", "ab\nx\n", "abb\n" })]
	[InlineData("Start : @string[] = Line* & '-' & (\"a\" | \"ab\")", true, new[] { "x\n-ab", "x\n-a", "-ab" })]
	[InlineData("X : @string = (\"a\" | \"ab\") => @(\"x\")\nStart : @string[] = X & \"b\" & eol & Line*", true, new[] { "abb\nx\n", "ab\n" })]
	[InlineData("X : @string = (\"a\" | \"ab\") => @(\"x\")\nStart : @string[] = X* & \"b\" & eol", true, new[] { "aabb\n", "ab\n", "abab\n" })]
	[InlineData("Item : @string = (\"a\" | \"ab\") & \"b\" & eol => @(\"i\")\nStart : @string[] = Item*", false, new[] { "abb\nab\n", "ab\nabb\n" })]
	[InlineData("Item : @string = (\"a\" | \"ab\") & \"b\" & eol => @(\"i\")\nStart : @string[] = Item* recover eol", false, new[] { "abb\nx\nab\n" })]
	[InlineData("Start : @string[] = (\"a\" | \"ab\") & \"b\" & eol & Row* recover eol", true, new[] { "abb\n[c]\n[9]\n", "ab\n[c]\n" })]
	[InlineData("Start : @string[] = Row* & ((\"a\" | \"ab\") & \"b\" & ';')*", true, new[] { "[x]\nabb;ab;", "ab;abb;" })]
	[InlineData("trivia = { ' '* }\nC : @string = 'c' => @(\"c\")\nStart : @string[] = (\"a\" | \"ab\") & \"b\" & C*", true, new[] { "a b c", "ab b c c", "abb c", " abb" })]
	[InlineData("X : @string = (\"a\" | \"Ab\"i) => @(\"x\")\nStart : @string[] = X* & 'b' & eol", true, new[] { "abb\n", "ab\n" })]
	[InlineData("X : @string = \"a\" => @(\"short\") | \"Ab\"i => @(\"long\")\nStart : @string[] = X+ & 'b'", true, new[] { "abb", "ab" })]
	[InlineData("X : @string = (\"a\" | \"Cb\"i) => @(\"x\")\nStart : @string[] = X* & 'b' & eol", false, new[] { "ab\n", "cbab\n", "Cbb\n" })]
	public void A_part_that_can_end_two_ways_is_not_read_one_way(string rule, bool refused, string[] inputs)
	{
		var grammar = rule.StartsWith("trivia", StringComparison.Ordinal) ? rule : Rules(rule);
		var told    = Compiled(grammar, CarrierKind.Auto, direct: true).Diagnostics;

		Assert.Equal(
			refused,
			told.Any(static one => one.Id == "GRAM5001" && one.Message.Contains("can end in more than one place", StringComparison.Ordinal)));

		if (refused)
			return;

		foreach (var carrier in Carriers)
			foreach (var direct in new[] { true, false })
			{
				var host = Host(grammar, carrier, direct);

				foreach (var input in inputs)
				{
					var whole    = Whole(host, input);
					var streamed = Streamed(host, input);

					Assert.True(
						whole == streamed || whole.StartsWith('!') && streamed.StartsWith('!') && Place(whole) == Place(streamed),
						$"{carrier}, direct {direct}, [{input}]: string {whole}, reader {streamed}");
				}
			}
	}

	/// <summary>
	/// The trivia a whole parse reads around the rule and between its parts takes all it can, and
	/// over a string gives back what the part after it needed: where it can read what a part begins
	/// with, or what the last part reads, the rule gets no reader.
	/// </summary>
	[Theory]
	[InlineData("trivia = 'a'*\nA : @string = 'a' => @(\"a\")\nStart : @string[] = A+", true, new[] { "a", "aa" })]
	[InlineData("trivia = 'a'*\nA : @string = 'b' => @(\"b\")\nB : @string = 'a' => @(\"a\")\nStart : @string[] = A* & B", true, new[] { "ba", "a" })]
	[InlineData("trivia = { ' '* }\nA : @string = 'a' => @(\"a\")\nStart : @string[] = A+", false, new[] { "a", " a a ", "a  a" })]
	[InlineData("trivia = { ' '* }\nA : @string = 'a' => @(\"a\")\nStart : @string[] = A* ~ \" b\"", true, new[] { "a b" })]
	public void Trivia_that_reads_into_a_part_is_not_read_once(string grammar, bool refused, string[] inputs)
	{
		var told = Compiled(grammar, CarrierKind.Auto, direct: true).Diagnostics;

		Assert.Equal(
			refused,
			told.Any(static one => one.Id == "GRAM5001" && one.Message.Contains("the trivia can read", StringComparison.Ordinal)));

		foreach (var carrier in Carriers)
			foreach (var direct in new[] { true, false })
			{
				var host = Host(grammar, carrier, direct, streams: !refused);

				foreach (var input in inputs)
				{
					var whole = Whole(host, input);

					if (refused)
					{
						Assert.False(whole.StartsWith('!'), $"{carrier}, direct {direct}, [{input}]: string {whole}");

						continue;
					}

					var streamed = Streamed(host, input);

					Assert.True(
						whole == streamed || whole.StartsWith('!') && streamed.StartsWith('!') && Place(whole) == Place(streamed),
						$"{carrier}, direct {direct}, [{input}]: string {whole}, reader {streamed}");
				}
			}
	}

	/// <summary>
	/// What the stream reads the same as the string and is still refused, because the checks that
	/// refuse it cannot tell it from a shape that reads differently. Kept as the cases to read
	/// right when the analysis can tell them apart.
	/// </summary>
	[Theory(Skip = "Known false refusal: space trivia reached through the seam after an optional prefix overlaps the trivia before the part, and only splits the spaces between them.")]
	[InlineData("trivia = { ' '* }\nA : @string = 'a'? & 'b' => @(\"b\")\nStart : @string[] = A+", new[] { " b b ", "ab b", "b", " a b" })]
	public void Space_trivia_before_an_optional_prefix_keeps_the_reader(string grammar, string[] inputs)
	{
		Agrees(grammar, inputs);
	}

	[Theory(Skip = "Known false refusal: alternatives that call fixed-width rules have no literal text to compare, and overlapping first sets are taken as a prefix.")]
	[InlineData("AB = \"ab\"\nAC = \"ac\"\nX : @string = AB => @(\"1\") | AC => @(\"2\")\nStart : @string[] = X* & eol", new[] { "abac\n", "ac\n" })]
	public void Named_fixed_width_alternatives_keep_the_reader(string grammar, string[] inputs)
	{
		Agrees(grammar, inputs);
	}

	[Theory]
	[InlineData("A : @string = 'a' => @(\"a\")\nStart : @string[] = A{2} & A+", new[] { "aa", "aaa", "aaaa", "a" })]
	public void An_exact_count_before_an_overlapping_stage_keeps_the_reader(string grammar, string[] inputs)
	{
		Agrees(grammar, inputs);
	}

	/// <summary>The grammar gets a reader, and it reads what the string reads.</summary>
	static void Agrees(string grammar, string[] inputs)
	{
		foreach (var carrier in Carriers)
			foreach (var direct in new[] { true, false })
			{
				var host = Host(grammar, carrier, direct);

				foreach (var input in inputs)
				{
					var whole    = Whole(host, input);
					var streamed = Streamed(host, input);

					Assert.True(
						whole == streamed || whole.StartsWith('!') && streamed.StartsWith('!') && Place(whole) == Place(streamed),
						$"{carrier}, direct {direct}, [{input}]: string {whole}, reader {streamed}");
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
		"not a repetition of a rule",
		"a\nb\n[c]\n[9]\n[d]\n")]
	[InlineData(
		"Start : @string[] = Line* & ('#' & Word & eol)?",
		"Hash : @string = '#' & w: Word & eol => @(w)\nStart : @string[] = Line* & Hash?",
		"collects values without being a call of a rule",
		"a\nb\n#c\n")]
	[InlineData(
		"A : @string = 'a' => @(\"a\")\nStart : @string[] = A? & A+",
		null,
		"can begin with the same input as what follows it",
		"a")]
	[InlineData(
		"Start : @string[] = Line{1,2} & Line",
		null,
		"can begin with the same input as what follows it",
		"a\nb\n")]
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

	/// <summary>
	/// Without a compilation to ask, an array is still taken for the types every array is, and for
	/// no other element of its own.
	/// </summary>
	[Theory]
	[InlineData("string[]", "string", false)]
	[InlineData("Row[]", "Row", false)]
	[InlineData("global::Feed.Row[]", "global::Feed.Row", false)]
	[InlineData("object[]", "object", true)]
	[InlineData("System.Collections.IEnumerable[]", "System.Collections.IEnumerable", true)]
	[InlineData("global::System.Collections.IList[]", "global::System.Collections.IList", true)]
	[InlineData("System.Array[]", "System.Array", true)]
	[InlineData("System.Collections.Generic.IEnumerable<object>[]", "System.Collections.Generic.IEnumerable<object>", true)]
	[InlineData("Row", "Item", true)]
	public void An_array_is_its_own_element_only_where_every_array_is(string from, string to, bool assignable)
	{
		Assert.Equal(assignable, PermissiveSymbolResolver.Instance.IsAssignable(from, to));
	}

	/// <summary>Where a refusal says it stopped: the number in its last <c>at N.</c>.</summary>
	static string Place(string refused)
	{
		var at = refused.LastIndexOf(" at ", StringComparison.Ordinal);

		return at < 0 ? refused : refused.Substring(at);
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
			Assert.Contains("new Window_DotGram(input, 4096)", source, StringComparison.Ordinal);

			source = source.Replace("new Window_DotGram(input, 4096)", "new Window_DotGram(input, 4)", StringComparison.Ordinal);
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
