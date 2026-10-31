using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using DotGram.Examples.Formats;
using DotGram.Examples.Languages;
using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A refusal in an example grammar costs a power of its input's length, not a factor for every unit
/// of it: doubling the input does not do much more than multiply the time by a few.
/// </summary>
/// <remarks>
/// <para>
/// Each input is a run of a unit that the grammar used to be able to read in two ways, ending in
/// something it cannot read, so that the whole run is tried every way it can be cut before the
/// refusal. The units are a line ending that is one <c>"\r\n"</c> or a <c>'\r'</c> and a <c>'\n'</c>,
/// blanks that a value or the separator before it could take, a heading, a bullet or a fence that
/// is also a paragraph, a word, a number or a string that could end sooner, a <c>--</c> that is a
/// comment or two symbols, and a value that runs to the end of its line or stops where the next
/// entry could begin. A reading with two ways per unit doubles for every unit, and no bound on a
/// time can be set under that: twenty-two lines of the YAML took two seconds and thirty-two would
/// have taken minutes.
/// </para>
/// <para>
/// <b>What is held is a ratio, never a time.</b> The input doubles from <see cref="First"/> units,
/// each size is timed over enough repeated refusals to be read off the clock, and the time of a
/// size may not be more than <see cref="Step"/> times that of the size before it: a linear reading
/// doubles it, a quadratic one quadruples it, and a shape that reads every way multiplies it by a
/// hundred at the first step. The ladder stops at the first step over the bound, so that a
/// regression fails in a second and not after minutes. Over the whole ladder the time may not grow
/// faster than twice a quadratic reading's. Some rows do grow about quadratically (a failed repetition
/// gives its turns back one at a time), which this does not mind.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class ExampleRefusalGrowthTests
{
	/// <summary>The size the ladder starts at, in units.</summary>
	const int First = 4;

	/// <summary>How many times the input doubles after <see cref="First"/>.</summary>
	const int Doublings = 6;

	/// <summary>What one doubling of the input may cost, as a multiple: a linear reading is 2, a quadratic one 4.</summary>
	const double Step = 8;

	/// <summary>What the whole ladder may cost, as a multiple of a quadratic reading's growth (64 times the size, 4096 times the time).</summary>
	const double Whole = 2;

	/// <summary>How long one refusal may take before the ladder is given up.</summary>
	static readonly TimeSpan Deadline = TimeSpan.FromSeconds(3);

	[Theory]
	[InlineData("a: b\r\n")]
	[InlineData("a:  b  c  \r\n")]
	[InlineData("  a:\r\n")]
	[InlineData("\r\n")]
	[InlineData("  # c\r\n")]
	[InlineData("a: b\n\r")]
	public void YAML_lines_are_refused_in_step_with_their_length(string piece)
	{
		Ladder("yaml " + Shown(piece), n => Repeated(piece, n) + "!", static text => YamlLite.TryParseDoc(text).IsSuccess);
	}

	[Fact]
	public void YAML_still_reads_what_it_read()
	{
		var root = YamlLite.Read("a: b\r\n\r\n  # x\r\n  c:  c  \r\nd: e");

		Assert.Equal("b", root["a"].Value);
		Assert.Equal("c", root["a"]["c"].Value);
		Assert.Equal("e", root["d"].Value);
		Assert.Equal("c", YamlLite.Read("a:\r\n  b:  c  \r\n\r\n").Children["a"]["b"].Value);
		Assert.False(YamlLite.TryParseDoc("a: b\r\n!").IsSuccess);
	}

	[Theory]
	[InlineData("k=v\r\n")]
	[InlineData("  k  =  v  \r\n")]
	[InlineData(" k k =\r\n")]
	[InlineData("; c\r\n\r\n")]
	[InlineData("[s]\r\n")]
	[InlineData(" [ s ] \r\nk=v\r\n")]
	[InlineData(";[s]\n")]
	[InlineData("k=v ;[s]\n#[t]\n")]
	public void INI_lines_are_refused_in_step_with_their_length(string piece)
	{
		Ladder("ini " + Shown(piece), n => Repeated(piece, n) + "]", static text => IniParser.TryParseIni(text).IsSuccess);
	}

	[Fact]
	public void INI_still_reads_what_it_read()
	{
		var file = IniParser.Read("g = 1\r\n\r\n  ; c\r\n[s]\r\n  k  k = v  \r\n =x\r\nz=1");

		Assert.Equal("1", file[""]["g"]);
		Assert.Equal("v", file["s"]["k  k"]);
		Assert.Equal("x", file["s"][""]);
		Assert.Equal("1", file["s"]["z"]);
		Assert.False(IniParser.TryParseIni("k=v\r\n]").IsSuccess);
	}

	[Theory]
	[InlineData("A: v\r\n \t x\r\n")]
	[InlineData("A:  v  \r\n")]
	[InlineData("A:\r\n \t \r\n  \r\n")]
	[InlineData("A: v\n\tw\n")]
	public void HTTP_fields_are_refused_in_step_with_their_length(string piece)
	{
		Ladder("http " + Shown(piece), n => Repeated(piece, n) + ": x", static text => HttpParser.TryParseHeaders(text).IsSuccess);
	}

	[Fact]
	public void HTTP_still_reads_what_it_read()
	{
		var headers = HttpParser.Read("A:  v \r\n \t x\r\nB:\r\n  \r\nC: c");

		Assert.Equal("v x", headers["A"]);
		Assert.Equal(" ", headers["B"]);
		Assert.Equal("c", headers["C"]);
	}

	[Theory]
	[InlineData("a\r\n")]
	[InlineData("a\n")]
	[InlineData("\r\n")]
	[InlineData("a\r\n\r\n")]
	[InlineData("# h\r\n")]
	[InlineData("- b\r\n")]
	[InlineData("- b\r\n- c\r\n\r\n")]
	[InlineData("```\r\nx\r\n```\r\n")]
	[InlineData("``` x\r\n\r\ny\r\n```\r\n\r\n")]
	public void Markdown_blocks_are_refused_in_step_with_their_length(string piece)
	{
		Ladder("markdown " + Shown(piece), n => Repeated(piece, n) + "z", static text => MarkdownParser.TryParseDoc(text).IsSuccess);
	}

	[Fact]
	public void Markdown_code_lines_ending_in_CRLF_are_refused_in_step_with_their_length()
	{
		Ladder("markdown code", static n => "```\r\n" + Repeated("x\r\n", n) + "y", static text => MarkdownParser.TryParseDoc(text).IsSuccess);
	}

	[Fact]
	public void Markdown_still_reads_what_it_read()
	{
		var blocks = MarkdownParser.Read("# h\r\n\r\n- a\r\n- b\r\ntext\n```\r\nx\r\n```\r\n");

		Assert.Equal(
			[typeof(MarkdownHeading), typeof(MarkdownBlank), typeof(MarkdownList), typeof(MarkdownParagraph), typeof(MarkdownCode)],
			blocks.Select(static one => one.GetType()).ToArray());

		// A fence that never closes is read as the lines it is.
		Assert.Equal(2, MarkdownParser.Read("```\r\nx\r\n").Count);
		Assert.False(MarkdownParser.TryParseDoc("a").IsSuccess);
	}

	[Theory]
	[InlineData(" abc")]
	[InlineData(" 12e-3")]
	[InlineData(" 'it''s'")]
	[InlineData(" \"q\" [b]")]
	[InlineData(" ( ) a.b,\t- /")]
	[InlineData(" -- c\n")]
	[InlineData(" a -- c\n")]
	[InlineData(" 1--c\n")]
	[InlineData(" /* c */")]
	[InlineData(" a/* c */b")]
	public void A_read_only_query_of_repeated_tokens_is_refused_in_step_with_its_length(string piece)
	{
		Ladder("sql " + Shown(piece), n => "select" + Repeated(piece, n) + " insert", static text => SqlReadOnly.IsReadOnly(text));
	}

	[Fact]
	public void A_read_only_query_still_reads_what_it_read()
	{
		Assert.True(SqlReadOnly.IsReadOnly("select a -- c\n from t"));
		Assert.True(SqlReadOnly.IsReadOnly("select a /* c */ from t where 1e-3 < 'it''s' and b.c = \"q\" or [b] = 2 - 1 / 3"));
		Assert.True(SqlReadOnly.IsReadOnly("select 1end"));
		Assert.True(SqlReadOnly.IsReadOnly("select 1 /* unterminated"));
		Assert.False(SqlReadOnly.IsReadOnly("select 1 /* drop table t"));
		Assert.False(SqlReadOnly.IsReadOnly("select a -- c\n insert into t values (1)"));
	}

	[Fact]
	public void A_minus_or_slash_that_starts_a_comment_is_not_a_symbol()
	{
		// Read as two symbols and a string, this would hide the insert from the check, and the server
		// reads the first line as a comment.
		Assert.False(SqlReadOnly.IsReadOnly("select 1 -- '\n insert into t values (1) --'"));
		Assert.False(SqlReadOnly.IsReadOnly("select 1 /* '\n */ insert into t values (1) /* ' */"));
	}

	[Theory]
	[InlineData("a=b=c\n")]
	[InlineData("a=b=\n")]
	[InlineData("aa=bb=cc=\n")]
	[InlineData("a = b = \n")]
	[InlineData("a=b #c\n")]
	[InlineData("a=b =\n")]
	public void A_located_config_whose_lines_hold_more_than_one_equals_sign_is_refused_in_step_with_its_length(string piece)
	{
		Ladder("located config " + Shown(piece), n => Repeated(piece, n) + "z\n!", static text => Config.Located.TryParseFile(text).IsSuccess);
	}

	[Fact]
	public void A_located_config_still_reads_what_it_read()
	{
		Assert.True(Config.Located.TryParseFile("a=b=c\nd = e\n").IsSuccess);

		// The first value gives back `b=`, so that the key before the line's end is the next entry's.
		var entries = Config.Located.ParseFile("a=b=\nc");

		Assert.Equal(["a", "b"], entries.Select(static one => one.Key).ToArray());
		Assert.Equal(["", "c"], entries.Select(static one => one.Value).ToArray());
		Assert.Equal([new At(0, 2), new At(2, 4)], entries.Select(static one => one.Where).ToArray());

		entries = Config.Located.ParseFile("a=b=c=\nz");

		Assert.Equal(["a", "c"], entries.Select(static one => one.Key).ToArray());
		Assert.Equal(["b=", "z"], entries.Select(static one => one.Value).ToArray());

		// A key and its equals sign on different lines.
		entries = Config.Located.ParseFile("a=x y\n=z");

		Assert.Equal(["a", "y"], entries.Select(static one => one.Key).ToArray());
		Assert.Equal(["x ", "z"], entries.Select(static one => one.Value).ToArray());

		Assert.False(Config.Located.TryParseFile("a=b=\nz\n!").IsSuccess);
	}

	/// <remarks>
	/// The grammar of <c>LocatedNullTests</c>, read over characters: <c>xASy</c> was one key, or a key
	/// and the alias word <c>AS</c> before another.
	/// </remarks>
	[Fact]
	public void A_list_of_keys_and_aliases_read_over_characters_is_refused_in_step_with_its_length()
	{
		var result = GramCompiler.Compile(
			"""
			using Std;

			trivia = { Spacing* }

			List  : @string[] = Entry* & eof
			Entry : @string   = { n: Key & a: Alias & ';' } => @(n + a)
			Key   : @string   = w: Identifier => @(w)
			Alias : @string   = "AS" & w: Identifier => @(w)
			                  | none => @("")

			parse List
			""",
			new GramCompilerOptions { ClassName = "Aliases", CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var parser = EmittedCode.Compile(result.Sources[0].Text, "Aliases", sourceParts: result.Sources.Skip(1).Select(static one => one.Text)).GetType("Aliases")!;
		var parse  = parser.GetMethod("TryParseList", [typeof(string)])!;

		bool Accepts(string text)
		{
			var match = parse.Invoke(null, [text])!;

			return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
		}

		Ladder("aliases", static n => Repeated("xASy;", n) + "!", Accepts);
		Assert.True(Accepts("t;u AS v;xASy;"));

		// `xASy` is a key, and `xAS y` a key and an alias: whichever reads the entry.
		Assert.True(Accepts("xAS y;"));
	}

	/// <summary>
	/// Doubles the input from <see cref="First"/> units and holds the time of each size against the
	/// one before it, and of the last against the first.
	/// </summary>
	static void Ladder(string what, Func<int, string> input, Func<string, bool> accepts)
	{
		var sizes = Enumerable.Range(0, Doublings + 1).Select(static one => First << one).ToArray();
		var texts = new string[sizes.Length];
		var seen  = new List<string>();

		for (var at = 0; at < sizes.Length; at++)
		{
			texts[at] = input(sizes[at]);

			// Once, with a deadline: a shape that reads every way would otherwise be in its first
			// refusal for minutes before any ratio could be asked of it.
			Assert.False(
				Refused(accepts, texts[at], what, sizes[at]),
				$"The {what} input of {sizes[at]} units is accepted, so it measures a refusal of nothing.");

			if (at == 0)
				continue;

			var small   = texts[at - 1];
			var large   = texts[at];
			var reading = ScalingClock.Settle(() => accepts(small), () => accepts(large), Step);

			seen.Add($"{sizes[at]}: {reading.Longer / 1000:F3} ms (x{reading.Ratio:F1})");
			Say(what, seen);

			Assert.True(
				reading.Within,
				$"The {what} refusal went from {reading.Shorter / 1000:F3} ms at {sizes[at - 1]} units to {reading.Longer / 1000:F3} ms at {sizes[at]}: " +
				$"{reading.Ratio:F1} times for twice the input, against a bound of {Step}, on every one of {ScalingClock.Attempts} measurements ({reading.Readings}).");
		}

		var first = texts[0];
		var last  = texts[^1];
		var bound = Whole * Math.Pow(sizes[^1] / (double)sizes[0], 2);
		var whole = ScalingClock.Settle(() => accepts(first), () => accepts(last), bound);

		Assert.True(
			whole.Within,
			$"The {what} refusal grew {whole.Ratio:F1} times over {sizes[0]} to {sizes[^1]} units, against a bound of {bound:F0}, " +
			$"on every one of {ScalingClock.Attempts} measurements ({whole.Readings}).");
	}

	/// <summary>One call of <paramref name="accepts"/>, given up on after <see cref="Deadline"/>.</summary>
	/// <remarks>
	/// The call runs on a thread of its own, which cannot be stopped from outside: on a miss it is left to
	/// run on, in the background, until the process ends, and the test fails at once.
	/// </remarks>
	static bool Refused(Func<string, bool> accepts, string text, string what, int units)
	{
		var result = false;
		var thread = new Thread(() => result = accepts(text)) { IsBackground = true };

		thread.Start();

		Assert.True(
			thread.Join(Deadline),
			$"One refusal of the {what} input took more than {Deadline.TotalSeconds:F0} s at {units} units, which is a time no ladder can be climbed in.");

		return result;
	}

	static void Say(string what, List<string> seen)
	{
		TestContext.Current.SendDiagnosticMessage($"{what}: {string.Join(", ", seen)}");
	}

	static string Shown(string piece)
	{
		return piece.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
	}

	static string Repeated(string piece, int times)
	{
		var built = new StringBuilder(piece.Length * times);

		for (var at = 0; at < times; at++)
			built.Append(piece);

		return built.ToString();
	}
}
