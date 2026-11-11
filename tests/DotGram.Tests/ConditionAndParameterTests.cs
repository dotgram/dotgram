using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Conditions (`when … is …`) and parameters, where what the grammar meant reached C# in a file the
/// author did not write: unreachable code after a rule a condition ruled out, a rule's name taken for
/// a guard, a quantified side of `is` told as a declaration, a recognizer parameter named in C#.
/// </summary>
public sealed class ConditionAndParameterTests
{
	const string Versions = "Version = \"2000\" | \"2022\"\n";

	static GramCompilation Compile(string grammar, Func<GramCompilerOptions>? options = null)
	{
		var made = options?.Invoke() ?? new GramCompilerOptions();

		made.CSharpScanner = RoslynCSharpScanner.Instance;

		return GramCompiler.Compile(grammar, made);
	}

	static string[] Errors(string grammar)
	{
		return [.. Compile(grammar).Diagnostics.Where(static one => one.Severity == GramSeverity.Error).Select(static one => one.Id)];
	}

	public static TheoryData<string> Renderings()
	{
		return new() { "default", "lexical", "tape", "buffered", "nodirect", "trace" };
	}

	static GramCompilerOptions Rendering(string name)
	{
		return name switch
		{
			"lexical"  => new GramCompilerOptions { Lexical = true },
			"tape"     => new GramCompilerOptions { Carrier = CarrierKind.Tape },
			"buffered" => new GramCompilerOptions { BufferedInput = true, BufferedBytes = true },
			"nodirect" => new GramCompilerOptions { Direct = false },
			"trace"    => new GramCompilerOptions { Trace = true },
			_          => new GramCompilerOptions(),
		};
	}

	/// <summary>
	/// A rule every alternative of which a condition rules out reads nothing (GRAM4022, information), and
	/// its scan is written as the refusal it is: the alternatives it kept for their shape used to stand
	/// after a refusal nothing passes, which a consumer's build reports as CS0162.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_rule_a_condition_rules_out_is_written_without_unreachable_code(string rendering)
	{
		foreach (var rules in new[]
		{
			"Old  = 'a' & when Version is \"2000\"\nTest = Old | 'b'\n",
			"Old  = ('a'+ & 'c') & when Version is \"2000\" | 'x' & when Version is \"2000\"\nTest = Old* & 'b'\n",
		})
		{
			var result = Compile(Versions + rules + "parse Test with (Version = \"2022\") as ParseNew", () => Rendering(rendering));

			Assert.Contains(result.Diagnostics, static one => one.Id == GrammarNormalizer.EmptyAfterConditions);
			Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

			var source = string.Join("\n", result.Sources.Select(static one => one.Text));

			Assert.Empty(EmittedCode.Complaints(source, "Grammar", null, null, TestContext.Current.CancellationToken));

			var assembly = EmittedCode.Compile(source);

			Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseNew", "b").IsSuccess);
			Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseNew", "ab").IsSuccess);
		}
	}

	/// <summary>
	/// The same where the scan writes a called rule's body in place of the call — inside braces, on the
	/// tape, in the trivia of a grammar read over tokens.
	/// </summary>
	[Theory]
	[InlineData("Old = 'a' & when Version is \"2000\"\nA = { 'q' & Old }\nTest = A | 'b'\n", "default", "b", "qa")]
	[InlineData("Old = 'a' & when Version is \"2000\"\nA = { 'q' & Old }\nTest = A | 'b'\n", "tape", "b", "qa")]
	[InlineData("Old = '#' & when Version is \"2000\"\ntrivia = { (Old | ' ')* }\nTest = 'a' & 'b'\n", "lexical", "a b", "a#b")]
	public void A_called_rule_a_condition_rules_out_is_left_out_where_it_is_written_in_place(
		string rules, string rendering, string accepted, string refused)
	{
		var result = Compile(Versions + rules + "parse Test with (Version = \"2022\") as ParseNew", () => Rendering(rendering));

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var source = string.Join("\n", result.Sources.Select(static one => one.Text));

		Assert.Empty(EmittedCode.Complaints(source, "Grammar", null, null, TestContext.Current.CancellationToken));

		var assembly = EmittedCode.Compile(source);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseNew", accepted).IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseNew", refused).IsSuccess);
	}

	[Theory]
	[InlineData("Test = 'a' & when Version\nparse Test")]
	[InlineData("Test = 'a' & when Version & 'b'\nparse Test")]
	public void A_rule_s_name_after_when_is_asked_for_its_is(string rules)
	{
		var grammar = Versions + rules;
		var told    = Assert.Single(Compile(grammar).Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GramParser.ExpectedIs, told.Id);
		Assert.Equal("Version", grammar.Substring(told.Position, told.Length));
	}

	[Theory]
	[InlineData("B : @bool = 't' => @(true)\nA = ok: B & when ok\nparse A")]
	[InlineData("A = 'a' & when @(true)\nparse A")]
	[InlineData(Versions + "A = 'a' & when Version is \"2000\"\nparse A with (Version = \"2000\") as P")]
	public void While_a_capture_named_bare_is_still_the_value_it_holds(string grammar)
	{
		Assert.Empty(Errors(grammar));
	}

	[Theory]
	[InlineData("Test = 'a' & when Version is ['0'..'9']+", "+")]
	[InlineData("Test = 'a' & when Version is ['0'..'9']{4}", "{4}")]
	[InlineData("Test = 'a' & when Version is \"20\"* & 'b'", "*")]
	[InlineData("Test = 'a' & when Version+ is \"2000\"", "+")]
	[InlineData("Test = 'a' & when Version{2} is \"2000\" and Version is \"2000\"", "{2}")]
	public void A_quantified_side_of_is_is_told_as_one(string rule, string at)
	{
		var grammar = Versions + rule + "\nparse Test with (Version = \"2000\") as P";
		var told    = Assert.Single(Compile(grammar).Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GramParser.QuantifiedSideOfIs, told.Id);
		Assert.Equal(at, grammar.Substring(told.Position, told.Length));
	}

	[Fact]
	public void And_a_side_named_as_a_rule_is_read()
	{
		var errors = Errors(Versions + "Digits = ['0'..'9']+\nTest = 'a' & when Version is Digits\nparse Test with (Version = \"2000\") as P");

		Assert.DoesNotContain(GramParser.QuantifiedSideOfIs, errors);
		Assert.DoesNotContain("GRAM2002", errors);
	}

	/// <summary>
	/// A recognizer parameter stands for a piece of grammar, and C# that names it used to fail as CS0103
	/// in the generated file: in a switch's selector, a guard, a construction.
	/// </summary>
	[Theory]
	[InlineData("Value(tag) = switch @(Kind(tag)) { case 0: Digits default: none }\nField = tag: Digits & '=' & Value(tag)")]
	[InlineData("Value(x) = x & when @(x != null)\nField = '=' & Value(Digits)")]
	[InlineData("Value(x) : @int = x & 'z' => @(Count(x))\nField : @int = '=' & v: Value(Digits) => @(v)")]
	public void C_sharp_that_names_a_recognizer_parameter_is_said_on_the_grammar(string rules)
	{
		var grammar = "Digits = ['0'..'9']+\n" + rules + "\nparse Field";
		var told    = Assert.Single(Compile(grammar).Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GrammarNormalizer.RecognizerInCSharp, told.Id);
		Assert.Contains("'Value'", told.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// What an argument brings in is the caller's C#, which may name a host member called like the
	/// parameter; and a name written before a dot may be a namespace or a type. Neither is the parameter.
	/// </summary>
	[Theory]
	[InlineData("Use(x) = x & 'z'\nTest = Use('a' & when @(x()))\nparse Test", "TryParseTest")]
	[InlineData("Use(x) = x & 'z'\nW = 'a'\nV = 'b'\nTest = Use(W & when @(x()))\nparse Test with (W = V) as P", "TryP")]
	[InlineData("Use(System) = System & 'z' & when @(System.Math.Abs(-1) > 0)\nTest = Use('a')\nparse Test", "TryParseTest")]
	public void The_csharp_a_rule_s_own_declaration_writes_is_what_is_asked(string grammar, string method)
	{
		var result = Compile(grammar);

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(
			string.Join("\n", result.Sources.Select(static one => one.Text)),
			declarationMembers: "static bool x() { return true; }");

		Assert.True(EmittedCode.Match(assembly, "Grammar", method, method == "TryP" ? "bz" : "az").IsSuccess);
	}

	[Theory]
	[InlineData("Value(x) : @int = x: Digits & 'z' => @(x.Length)\nField : @int = '=' & v: Value(Digits) => @(v)")]
	[InlineData("Value(n: @int) = 'a' & when @(n > 0)\nField = '=' & Value(1)")]
	[InlineData("Value(x) = x & 'z'\nField = tag: Digits & '=' & Value(tag)")]
	public void While_a_capture_or_a_value_parameter_is_read_as_before(string rules)
	{
		Assert.Empty(Errors("Digits = ['0'..'9']+\n" + rules + "\nparse Field"));
	}
}
