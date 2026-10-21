using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// §7.3: a run that matched nothing is <c>""</c>, and only an optional capture that never ran is
/// null. A repetition that may take no turn is a run like <c>*</c> whatever its bounds, and a rule
/// of several alternatives is no reason for it to be absent: the alternative that holds it always
/// writes it.
/// </summary>
/// <remarks>
/// Every carrier, the direct reader and the engine, and each input form: a string, a reader, a
/// stream and bytes held whole. Over bytes a text capture is an empty span, never null, so the
/// string form reading null for the same grammar was an answer the two forms did not share.
/// </remarks>
public sealed class EmptyRunCaptureTests
{
	public static IEnumerable<object[]> Readings()
	{
		foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate })
			foreach (var direct in new[] { false, true })
				foreach (var buffered in new[] { false, true })
					yield return [carrier, direct, buffered];
	}

	public static IEnumerable<object[]> StringReadings()
	{
		foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate })
			foreach (var direct in new[] { false, true })
				yield return [carrier, direct];
	}

	[Theory]
	[MemberData(nameof(Readings))]
	public void A_bounded_run_that_took_no_turn_is_empty_text_in_a_rule_of_several_alternatives(
		CarrierKind carrier, bool direct, bool buffered)
	{
		var assembly = Compile(
			"""
			One : @string = c: 'a'{0,2} => @(Shown(c))
			Two : @string = c: 'a'{0,2} => @(Shown(c)) | 'c' => @("c")
			parse One
			parse Two
			""", carrier, direct, buffered);

		foreach (var (method, input, expected) in new[]
		{
			("TryParseOne", "",   "[]"),
			("TryParseOne", "aa", "[aa]"),
			("TryParseTwo", "",   "[]"),
			("TryParseTwo", "a",  "[a]"),
			("TryParseTwo", "aa", "[aa]"),
			("TryParseTwo", "c",  "c"),
		})
			Same(assembly, method, input, expected, buffered);
	}

	/// <summary>
	/// A run of a sequence, which no carrier reads as one span: each turn is a piece of its own, and
	/// no turn is no piece at all. Starred and bounded alike.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_run_of_pieces_that_took_no_turn_is_empty_text_in_a_rule_of_several_alternatives(
		CarrierKind carrier, bool direct, bool buffered)
	{
		var assembly = Compile(
			"""
			Starred : @string = c: ('a' & 'b')* & 'x' => @(Shown(c)) | 'c' => @("c")
			Bounded : @string = c: ('a' & 'b'){0,3} & 'x' => @(Shown(c)) | 'c' => @("c")
			parse Starred
			parse Bounded
			""", carrier, direct, buffered);

		foreach (var method in new[] { "TryParseStarred", "TryParseBounded" })
			foreach (var (input, expected) in new[] { ("x", "[]"), ("abx", "[ab]"), ("ababx", "[abab]"), ("c", "c") })
				Same(assembly, method, input, expected, buffered);
	}

	/// <summary>
	/// A capture repeated with something else in each turn, whose text is the turns' pieces joined
	/// and not the extent of the run: no turn is no piece, and still <c>""</c>.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_run_of_separated_pieces_that_took_no_turn_is_empty_text(CarrierKind carrier, bool direct, bool buffered)
	{
		var assembly = Compile(
			"""
			Choice : @string = (c: 'a' & ',')* & 'x' => @(Shown(c)) | 'c' => @("c")
			Opt : @string = ((c: 'a' & ','){0,3} & 'x')? & 'y' => @(Shown(c))
			parse Choice
			parse Opt
			""", carrier, direct, buffered);

		foreach (var (method, input, expected) in new[]
		{
			("TryParseChoice", "x",      "[]"),
			("TryParseChoice", "a,a,x",  "[aa]"),
			("TryParseChoice", "c",      "c"),
			("TryParseOpt",    "y",      "null"),
			("TryParseOpt",    "xy",     "[]"),
			("TryParseOpt",    "a,xy",   "[a]"),
		})
			Same(assembly, method, input, expected, buffered);
	}

	/// <summary>
	/// The same inside the element of a repetition marked <c>recover</c>, a run of one piece per
	/// turn and a run of separated pieces.
	/// </summary>
	/// <remarks>
	/// The string form only: a recovery tracks lines and columns, which a buffer of bytes does not
	/// carry (GRAM4026).
	/// </remarks>
	[Theory]
	[MemberData(nameof(StringReadings))]
	public void A_run_that_took_no_turn_is_empty_text_in_a_recovered_element(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			Recovered : @string = (c: 'a'{0,2} & 'x')* recover ';' & 'z' => @(Shown(c)) | 'q' => @("q")
			Spaced : @string = ((c: 'a' & ','){0,2} & 'x')* recover ';' & 'z' => @(Shown(c)) | 'q' => @("q")
			parse Recovered
			parse Spaced
			""", carrier, direct, buffered: false);

		foreach (var (method, input, expected) in new[]
		{
			("TryParseRecovered", "xz",        "[]"),
			("TryParseRecovered", "axaxz",     "[aa]"),
			("TryParseRecovered", "a;xz",      "[]"),
			("TryParseRecovered", "q",         "q"),
			("TryParseSpaced",    "xz",        "[]"),
			("TryParseSpaced",    "a,xa,a,xz", "[aaa]"),
			("TryParseSpaced",    "q",         "q"),
		})
			Same(assembly, method, input, expected, buffered: false);
	}

	/// <summary>
	/// A recovering repetition handed to a rule as an argument stands wherever the parameter does,
	/// one node in two places: rebuilt in each, it recovers in each.
	/// </summary>
	[Theory]
	[MemberData(nameof(StringReadings))]
	public void A_recovered_element_passed_as_an_argument_recovers_wherever_it_stands(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			Twice(item) = item & '|' & item & 'z'
			Start = Twice(((c: 'a'{0,2} & 'x')* recover ';'))
			parse Start
			""", carrier, direct, buffered: false);

		foreach (var input in new[] { "x|xz", "x|a;xz", "a;x|xz", "|z" })
		{
			var match = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);

			Assert.True(match.IsSuccess, $"\"{input}\": {match.Error}");
		}
	}

	/// <summary>
	/// And in a rule whose left recursion is read as a loop: in its base, under an option there,
	/// and in a step of the loop.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_run_that_took_no_turn_is_empty_text_in_a_fold(CarrierKind carrier, bool direct, bool buffered)
	{
		var assembly = Compile(
			"""
			Folded : @string = prior: Folded & '+' => @(prior + "+") | (c: 'a'{0,2} & 'x')? & 'b' => @(Shown(c))
			Stepped : @string = prior: Stepped & '+' & c: 'a'{0,2} & 'x' => @(prior + Shown(c)) | 'b' => @("b")
			parse Folded
			parse Stepped
			""", carrier, direct, buffered);

		foreach (var (method, input, expected) in new[]
		{
			("TryParseFolded",  "xb",       "[]"),
			("TryParseFolded",  "b",        "null"),
			("TryParseFolded",  "axb+",     "[a]+"),
			("TryParseStepped", "b+x",      "b[]"),
			("TryParseStepped", "b+aax+x",  "b[aa][]"),
		})
			Same(assembly, method, input, expected, buffered);
	}

	/// <summary>A step of a fold whose run is of separated pieces, on the tape.</summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_run_of_separated_pieces_that_took_no_turn_is_empty_text_in_a_fold_step(
		CarrierKind carrier, bool direct, bool buffered)
	{
		// Read by methods, a text capture a repetition inside a fold's step repeats does not compile
		// (CS0103), with or without this run's seed: those rows are the skipped test below.
		if (direct && carrier != CarrierKind.Tape)
			return;

		var assembly = Compile(
			"""
			Stepped : @string = prior: Stepped & '+' & (c: 'a' & ','){0,2} & 'x' => @(prior + Shown(c)) | 'b' => @("b")
			parse Stepped
			""", carrier, direct, buffered);

		Same(assembly, "TryParseStepped", "b+x", "b[]", buffered);
		Same(assembly, "TryParseStepped", "b+a,x+x", "b[a][]", buffered);
	}

	/// <summary>
	/// Read by methods, a text capture that a repetition inside a fold's step repeats: the
	/// generated code names its spans' count outside the scope that declares it.
	/// </summary>
	[Theory(Skip = "Open: CS0103 'refs_Spans' where a repetition inside a fold's step repeats a text capture, read by methods.")]
	[InlineData(CarrierKind.Auto)]
	[InlineData(CarrierKind.Immediate)]
	public void A_repeated_text_capture_in_a_fold_step_compiles_when_read_by_methods(CarrierKind carrier)
	{
		var assembly = Compile(
			"""
			Stepped : @string = prior: Stepped & '+' & (c: 'a' & ','){0,2} & 'x' => @(prior + Shown(c)) | 'b' => @("b")
			parse Stepped
			""", carrier, direct: true, buffered: false);

		Same(assembly, "TryParseStepped", "b+a,x+x", "b[a][]", buffered: false);
	}

	/// <summary>
	/// What is genuinely optional stays null, and a run that took no turn inside it is still a
	/// run: <c>(c: 'a'{0,2} &amp; 'x')?</c> that did not match is null, and matched over just the
	/// <c>'x'</c> is <c>""</c>. A guard is handed the same as the construction after it.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_run_inside_an_option_is_empty_where_the_option_matched_and_null_where_it_did_not(
		CarrierKind carrier, bool direct, bool buffered)
	{
		var assembly = Compile(
			"""
			Opt : @string = (c: 'a'{0,2} & 'x')? & 'y' => @(Shown(c))
			Guarded : @string = c: 'a'{0,2} & when @(c.Length < 2) & 'x' => @(Shown(c)) | 'c' => @("c")
			parse Opt
			parse Guarded
			""", carrier, direct, buffered);

		foreach (var (method, input, expected) in new[]
		{
			("TryParseOpt",     "y",   "null"),
			("TryParseOpt",     "xy",  "[]"),
			("TryParseOpt",     "axy", "[a]"),
			("TryParseGuarded", "x",   "[]"),
			("TryParseGuarded", "ax",  "[a]"),
			("TryParseGuarded", "c",   "c"),
		})
			Same(assembly, method, input, expected, buffered);

		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseGuarded", "aax").IsSuccess);
	}

	/// <summary>
	/// <c>?</c> is the one repetition that is an option and not a run (§7.3, <c>(sign: '-')?</c>):
	/// one that did not match is null, in a rule of one alternative or of several, and <c>{0,1}</c>
	/// is the same bound spelled out.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void An_option_that_did_not_match_stays_null(CarrierKind carrier, bool direct, bool buffered)
	{
		var assembly = Compile(
			"""
			One : @string = c: 'a'? & 'x' => @(Shown(c))
			Two : @string = c: 'a'{0,1} & 'x' => @(Shown(c)) | 'c' => @("c")
			parse One
			parse Two
			""", carrier, direct, buffered);

		foreach (var (method, input, expected) in new[]
		{
			("TryParseOne", "x",  "null"),
			("TryParseOne", "ax", "[a]"),
			("TryParseTwo", "x",  "null"),
			("TryParseTwo", "ax", "[a]"),
		})
			Same(assembly, method, input, expected, buffered);
	}

	/// <summary>
	/// Where the rule builds no value of its own, the member of the class generated for it is the
	/// same answer: <c>""</c> for the alternative that read a run of no turns.
	/// </summary>
	/// <remarks>
	/// The string form only: a class generated for a rule holds its text as strings, which a buffer
	/// of bytes cannot hand it (GRAM4026).
	/// </remarks>
	[Theory]
	[MemberData(nameof(StringReadings))]
	public void A_generated_class_holds_empty_text_for_a_run_that_took_no_turn(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			Pair = c: 'a'{0,2} & 'x' | d: 'c'
			Spaced = (c: 'a' & ',')* & 'x' | d: 'c'
			parse Pair
			parse Spaced
			""", carrier, direct, buffered: false);

		foreach (var method in new[] { "TryParsePair", "TryParseSpaced" })
		{
			foreach (var (input, expected) in new[] { ("x", "[]"), (method == "TryParsePair" ? "aax" : "a,a,x", "[aa]") })
			{
				var match = EmittedCode.Match(assembly, "Grammar", method, input);

				Assert.True(match.IsSuccess, $"{method}(\"{input}\"): {match.Error}");
				Assert.Equal(expected, Shown(match.Value!.GetType().GetProperty("C")!.GetValue(match.Value)));
			}

			var other = EmittedCode.Match(assembly, "Grammar", method, "c");

			Assert.True(other.IsSuccess, other.Error);
			Assert.Null(other.Value!.GetType().GetProperty("C")!.GetValue(other.Value));
		}
	}

	static string Shown(object? value)
	{
		return value is null ? "null" : "[" + value + "]";
	}

	/// <summary>The string form, and over a buffer the reader, stream and byte forms beside it.</summary>
	/// <remarks>
	/// A text capture over bytes is a span, which cannot hold an absence: an option that did not
	/// match is an empty span there, by design (§6.3), where the forms over characters keep null.
	/// </remarks>
	static void Same(System.Reflection.Assembly assembly, string method, string input, string expected, bool buffered)
	{
		var match = EmittedCode.Match(assembly, "Grammar", method, input);

		Assert.True(match.IsSuccess, $"{method}(\"{input}\"): {match.Error}");
		Assert.Equal(expected, match.Value);

		if (!buffered)
			return;

		var type = assembly.GetType("Grammar")!;

		foreach (var (over, argument) in new (System.Type, object)[]
		{
			(typeof(TextReader), new StringReader(input)),
			(typeof(Stream), new MemoryStream(Encoding.ASCII.GetBytes(input))),
			(typeof(byte[]), Encoding.ASCII.GetBytes(input)),
		})
		{
			var found = over == typeof(byte[])
				? type.GetMethod(method, [over])!.Invoke(null, [argument])!
				: type.GetMethod(method, [over, typeof(int?), typeof(int?)])!.Invoke(null, [argument, null, null])!;
			var ok    = (bool)found.GetType().GetProperty("IsSuccess")!.GetValue(found)!;
			var value = found.GetType().GetProperty("Value")!.GetValue(found);

			Assert.True(ok, $"{method} over {over.Name}(\"{input}\"): {found.GetType().GetProperty("Error")!.GetValue(found)}");

			var wanted = over != typeof(TextReader) && expected == "null" ? "[]" : expected;

			Assert.True(wanted == (string?)value, $"{method} over {over.Name}(\"{input}\"): expected {wanted}, read {value}");
		}
	}

	internal const string Host =
		"static string Shown(string text) { return text is null ? \"null\" : \"[\" + text + \"]\"; } " +
		"static string Shown(global::System.ReadOnlySpan<char> text) { return \"[\" + text.ToString() + \"]\"; } " +
		"static string Shown(global::System.ReadOnlySpan<byte> text) { return \"[\" + global::System.Text.Encoding.ASCII.GetString(text.ToArray()) + \"]\"; }";

	internal static System.Reflection.Assembly Compile(string grammar, CarrierKind carrier, bool direct, bool buffered, string host = Host)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Carrier       = carrier,
			Direct        = direct,
			BufferedInput = buffered,
			BufferedBytes = buffered,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(
			result.Sources[0].Text,
			declarationMembers: host,
			sourceParts: result.Sources.Skip(1).Select(static source => source.Text));
	}
}
