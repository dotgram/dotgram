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

	/// <summary>
	/// Compiled as a build compiles it: with a resolver that sees the host, which declares
	/// <paramref name="members"/> and nothing else.
	/// </summary>
	static GramCompilation Compile(string grammar, Func<GramCompilerOptions>? options = null, string members = "")
	{
		var made = options?.Invoke() ?? new GramCompilerOptions();

		made.CSharpScanner  = RoslynCSharpScanner.Instance;
		made.SymbolResolver = Resolver(members);

		return GramCompiler.Compile(grammar, made);
	}

	static RoslynSymbolResolver Resolver(string members)
	{
		var host = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
			"Host",
			[Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText($"public partial class Grammar {{ {members} }}")],
			EmittedCode.References);

		return new RoslynSymbolResolver(host, "Grammar");
	}

	static string[] Errors(string grammar)
	{
		return [.. Compile(grammar).Diagnostics.Where(static one => one.Severity == GramSeverity.Error).Select(static one => one.Id)];
	}

	public static TheoryData<string> Renderings()
	{
		return new() { "default", "lexical", "deferred", "buffered", "nodirect", "trace" };
	}

	static GramCompilerOptions Rendering(string name)
	{
		return name switch
		{
			"lexical"  => new GramCompilerOptions { Lexical = true },
			"deferred"     => new GramCompilerOptions { Carrier = CarrierKind.Deferred },
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
	/// deferred carrier, in the trivia of a grammar read over tokens.
	/// </summary>
	[Theory]
	[InlineData("Old = 'a' & when Version is \"2000\"\nA = { 'q' & Old }\nTest = A | 'b'\n", "default", "b", "qa")]
	[InlineData("Old = 'a' & when Version is \"2000\"\nA = { 'q' & Old }\nTest = A | 'b'\n", "deferred", "b", "qa")]
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
	/// in the generated file: in a switch's selector, a guard, a construction, a value handed to a call,
	/// and before a dot, as in <c>item.Length</c>.
	/// </summary>
	[Theory]
	[InlineData("Value(tag) = switch @(Kind(tag)) { case 0: Digits default: none }\nField = tag: Digits & '=' & Value(tag)")]
	[InlineData("Value(x) = x & when @(x != null)\nField = '=' & Value(Digits)")]
	[InlineData("Value(x) : @int = x & 'z' => @(Count(x))\nField : @int = '=' & v: Value(Digits) => @(v)")]
	[InlineData("Value(x) = x & when (@(x != null))\nField = '=' & Value(Digits)")]
	[InlineData("Version = \"2000\" | \"2022\"\nValue(x) = x & when Version is \"2000\" and @(x != null)\nField = '=' & Value(Digits)")]
	[InlineData("Value(x) = x & when @(@x != null)\nField = '=' & Value(Digits)")]
	[InlineData("Value(item) = item & item & when @(item.Length > 0)\nField = '=' & Value(Digits)")]
	[InlineData("Value(item) = switch @(item.Length) { case 0: Digits default: none }\nField = '=' & Value(Digits)")]
	[InlineData("Value(x) : @int = x & 'z' => @(x.Length)\nField : @int = '=' & v: Value(Digits) => @(v)")]
	[InlineData("Value(var) = var & when @(var.Length > 0)\nField = '=' & Value(Digits)")]
	public void C_sharp_that_names_a_recognizer_parameter_is_said_on_the_grammar(string rules)
	{
		var grammar = "Digits = ['0'..'9']+\n" + rules + "\nparse Field";
		var told    = Assert.Single(Compile(grammar).Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GrammarNormalizer.RecognizerInCSharp, told.Id);
		Assert.Contains("'Value'", told.Message, StringComparison.Ordinal);

		// And the generator, which asks the host ahead of time, says the same.
		var run = GeneratorDriverTests.RunGenerator("[DotGram.Gram] public partial class Host;", ("/proj/Host.gram", grammar));

		Assert.Contains(run.Diagnostics, static one => one.Id == GrammarNormalizer.RecognizerInCSharp);
	}

	/// <summary>
	/// C# handed to a call as a value is refused for what it is, not a literal nor a value the rule was
	/// handed (GRAM4013), and a recognizer parameter it names is said as well.
	/// </summary>
	[Fact]
	public void C_sharp_handed_to_a_call_that_names_a_recognizer_parameter_is_said_too()
	{
		var errors = Errors("Digits = ['0'..'9']+\nInner(n: @int) = 'a'\nValue(x) = x & Inner(@(x.Length))\nField = '=' & Value(Digits)\nparse Field");

		Assert.Contains(GrammarNormalizer.RecognizerInCSharp, errors);
	}

	/// <summary>
	/// What an argument brings in is the caller's C#, which may name a host member called like the
	/// parameter; and a name before a dot may be a namespace the host sees. Neither is the parameter.
	/// </summary>
	[Theory]
	[InlineData("Use(x) = x & 'z'\nTest = Use('a' & when @(x()))\nparse Test", "TryParseTest")]
	[InlineData("Use(x) = x & 'z'\nW = 'a'\nV = 'b'\nTest = Use(W & when @(x()))\nparse Test with (W = V) as P", "TryP")]
	[InlineData("Use(System) = System & 'z' & when @(System.Math.Abs(-1) > 0)\nTest = Use('a')\nparse Test", "TryParseTest")]
	[InlineData("Use(System) = System & 'z' & when @(System /*System*/ .Math.Abs(-1) > 0)\nTest = Use('a')\nparse Test", "TryParseTest")]
	[InlineData("Use(System) = System & 'z' & when @(System.Math.Abs(-1) > 0 && \"System\".Length > 0)\nTest = Use('a')\nparse Test", "TryParseTest")]
	public void The_csharp_a_rule_s_own_declaration_writes_is_what_is_asked(string grammar, string method)
	{
		var result = Compile(grammar);

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(
			string.Join("\n", result.Sources.Select(static one => one.Text)),
			declarationMembers: "static bool x() { return true; }");

		Assert.True(EmittedCode.Match(assembly, "Grammar", method, method == "TryP" ? "bz" : "az").IsSuccess);
	}

	/// <summary>
	/// A name before a dot that the grammar's own <c>@using</c> brings in, a class, is
	/// that, as it is in the generated file, which writes the directive where the call stands; not the
	/// parameter of the same name. Without the directive the same rule is said on the grammar.
	/// </summary>
	[Theory]
	[InlineData("@using System;\n", "Use(Math) = Math & 'z' & when @(Math.Abs(-1) > 0)")]
	[InlineData("@using System.Text;\n", "Use(Encoding) = Encoding & 'z' & when @(Encoding.UTF8 != null)")]
	public void A_name_the_grammar_s_own_using_brings_in_is_not_the_parameter(string usings, string rule)
	{
		var grammar = usings + rule + "\nTest = Use('a')\nparse Test";
		var result  = Compile(grammar);

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(string.Join("\n", result.Sources.Select(static one => one.Text)));

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseTest", "az").IsSuccess);

		// The generator asks the same question ahead of time, beside the same directives.
		var run = GeneratorDriverTests.RunGenerator("[DotGram.Gram] public partial class Host;", ("/proj/Host.gram", grammar));

		Assert.DoesNotContain(run.Diagnostics, static one => one.Id == GrammarNormalizer.RecognizerInCSharp);

		// And without the directive, nothing answers to the name, and it is the parameter.
		var bare = rule + "\nTest = Use('a')\nparse Test";

		Assert.Contains(Compile(bare).Diagnostics, static one => one.Id == GrammarNormalizer.RecognizerInCSharp);
		Assert.Contains(
			GeneratorDriverTests.RunGenerator("[DotGram.Gram] public partial class Host;", ("/proj/Host.gram", bare)).Diagnostics,
			static one => one.Id == GrammarNormalizer.RecognizerInCSharp);
	}

	/// <summary>
	/// A lambda's parameter or a local of a lambda's block is its own only inside that lambda: the
	/// same name read after it is the rule's parameter, and said. Read only inside it, it is not.
	/// </summary>
	[Theory]
	[InlineData("Use(item) = item & 'z' & when @(System.Linq.Enumerable.Any(new[] { \"a\" }, item => item.Length > 0) && item.Length > 0)", true)]
	[InlineData("Use(item) = item & 'z' & when @(((System.Func<bool>)(() => { var item = \"a\"; return item.Length > 0; }))() && item.Length > 0)", true)]
	[InlineData("Use(item) = item & 'z' & when @(System.Linq.Enumerable.Any(new[] { \"a\" }, item => item.Length > 0))", false)]
	[InlineData("Use(item) = item & 'z' & when @(((System.Func<bool>)(() => { var item = \"a\"; return item.Length > 0; }))())", false)]
	public void A_name_a_lambda_declares_is_its_own_only_inside_it(string rule, bool said)
	{
		var result = Compile(rule + "\nTest = Use('a')\nparse Test");

		if (said)
		{
			Assert.Contains(result.Diagnostics, static one => one.Id == GrammarNormalizer.RecognizerInCSharp);
		}
		else
		{
			Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);
			Assert.Empty(EmittedCode.Errors(string.Join("\n", result.Sources.Select(static one => one.Text)), "Grammar", null, null, null));
		}
	}

	/// <summary>
	/// What a declaration in the C# reaches is the C# compiler's to say: an expression variable in a
	/// <c>lock</c>'s expression, or in a query's first source, belongs to the block around it, and is
	/// what a later name of that spelling reads — not the parameter.
	/// </summary>
	[Theory]
	[InlineData(
		"",
		"Use(x) = x & 'z' & when @(((System.Func<bool>)(() => { lock (Gate(out var x)) { } return x > 0; }))())",
		"static object Gate(out int v) { v = 1; return new object(); }")]
	[InlineData(
		"@using System.Linq;\n",
		"Use(x) = x & 'z' & when @(((System.Func<bool>)(() => { var q = from a in Items(out var x) select a + x; return x > 0 && q.Any(); }))())",
		"static int[] Items(out int v) { v = 1; return new[] { 1 }; }")]
	[InlineData("", "Use(nameof) = nameof & 'z' & when @(nameof(System.String).Length > 0)", "")]
	[InlineData("", "Use(var) = var & 'z' & when @(((System.Func<bool>)(() => { var n = 1; return n > 0; }))())", "")]
	public void A_name_a_declaration_in_the_csharp_reaches_is_not_the_parameter(string usings, string rule, string members)
	{
		var grammar = usings + rule + "\nTest = Use('a')\nparse Test";
		var result  = Compile(grammar, members: members);

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(string.Join("\n", result.Sources.Select(static one => one.Text)), declarationMembers: members);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseTest", "az").IsSuccess);

		// The generator asks the same of the host ahead of time.
		var run = GeneratorDriverTests.RunGenerator("[DotGram.Gram] public partial class Host { " + members + " }", ("/proj/Host.gram", grammar));

		Assert.DoesNotContain(run.Diagnostics, static one => one.Id == GrammarNormalizer.RecognizerInCSharp);
	}

	/// <summary>
	/// A parameter's name in C# binds to the host's member of that name where the host has one, and the
	/// grammar compiles and reads: a field, or a method handed on as a method group.
	/// </summary>
	[Theory]
	[InlineData("Use(x) = x & 'z' & when @(x > 0)", "static int x = 1;")]
	[InlineData("Use(x) = x & 'z' & when @(Check(x))", "static bool x() { return true; } static bool Check(System.Func<bool> f) { return f(); }")]
	public void A_parameter_s_name_the_host_declares_is_the_host_s_in_csharp(string rule, string members)
	{
		var grammar = rule + "\nTest = Use('a')\nparse Test";
		var result  = Compile(grammar, members: members);

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(string.Join("\n", result.Sources.Select(static one => one.Text)), declarationMembers: members);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseTest", "az").IsSuccess);

		// And where the host declares no such name, the same rule is said on the grammar.
		Assert.Contains(Compile(grammar).Diagnostics, static one => one.Id == GrammarNormalizer.RecognizerInCSharp);
	}

	/// <summary>
	/// With no host to ask — a grammar compiled on its own — the name may still be one the host will
	/// declare, so nothing is said, and a C# that does not compile is the C# compiler's to say.
	/// </summary>
	[Fact]
	public void With_no_host_to_ask_nothing_is_said()
	{
		var result = GramCompiler.Compile(
			"Digits = ['0'..'9']+\nValue(x) = x & when @(x != null)\nField = '=' & Value(Digits)\nparse Field",
			new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.DoesNotContain(result.Diagnostics, static one => one.Id == GrammarNormalizer.RecognizerInCSharp);
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
