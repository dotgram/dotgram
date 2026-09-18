using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

public sealed class SwitchTests
{
	[Theory]
	[InlineData("1/22/333", "CBA", 6)]
	[InlineData("1//333", "CA", 4)]
	public void Scalar_guard_preserves_reverse_construction_order_and_reuses_values(string input, string order, int value)
	{
		var result = GramCompiler.Compile("""
			A : @int = digits: { ['1'..'9']+ } => @(Made("A", digits.Length))
			B : @int = digits: { ['1'..'9']+ } => @(Made("B", digits.Length))
			C : @int = digits: { ['1'..'9']+ } => @(Made("C", digits.Length))
			Start : @int = a: A & '/' & b: B? & '/' & c: C
				& when @(a + b.GetValueOrDefault() + c > 0)
				& when @(c > 0 && a > 0)
				=> @(a + b.GetValueOrDefault() + c)
			parse Start
			""", new GramCompilerOptions
		{
			BufferedInput = true, BufferedBytes = true, Direct = false, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;
		Assert.Contains("var guardPending", source);
		var assembly = EmittedCode.Compile(source, declarationMembers:
			"public static string Order = string.Empty; static int Made(string name, int value) { Order += name; return value; }");
		var host = assembly.GetType("Grammar")!;
		var trace = host.GetField("Order")!;
		Assert.Equal(value, EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).Value);
		Assert.Equal(order, trace.GetValue(null));
		foreach (var bytes in new[] { false, true })
		{
			trace.SetValue(null, "");
			using var reader = new StringReader(input);
			using var stream = new MemoryStream(Encoding.ASCII.GetBytes(input));
			Assert.Equal(value, host.GetMethod("ParseStart", [bytes ? typeof(Stream) : typeof(TextReader), typeof(int?), typeof(int?)])!
				.Invoke(null, [bytes ? stream : reader, 1, 32]));
			Assert.Equal(order, trace.GetValue(null));
		}
	}

	[Theory]
	[InlineData("11", 12)]
	[InlineData("22", 22)]
	public void Scalar_guard_with_different_rules_in_one_member_uses_the_matched_factory(string input, int expected)
	{
		var assembly = Compile("""
			A : @int = digits: { ['1']+ } => @(digits.Length + 10)
			B : @int = digits: { ['2']+ } => @(digits.Length + 20)
			Start : @int = (n: A | n: B) & when @(n > 0) => @(n)
			parse Start
			""", direct: false);
		Assert.Equal(expected, EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).Value);
	}

	[Theory]
	[InlineData("12=x", 1)]
	[InlineData("12=a", 1)]
	[InlineData("12=b", 2)]
	public void Scalar_run_methods_preserve_factory_demand_across_backtracking(string input, int calls)
	{
		var result = GramCompiler.Compile("""
			Number : @int = digits: { ['1'..'9'] & ['0'..'9']* } => @(Make(digits))
			First : @int = n: Number & "=x" => @(n)
			Selected : @int = n: Number & '=' & switch @(n) { case 12: 'a' => @(n) }
			Last : @int = n: Number & "=b" => @(n)
			Start : @int = v: First => @(v) | v: Selected => @(v) | v: Last => @(v)
			parse Start
			""", new GramCompilerOptions
		{
			BufferedInput = true, BufferedBytes = true, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;
		Assert.Contains("static int Read_Number", source);
		var assembly = EmittedCode.Compile(source, declarationMembers: """
			public static int Calls;
			static int Make(string text) { Calls++; return 12; }
			static int Make(global::System.ReadOnlySpan<byte> text) { Calls++; return 12; }
			""");
		var host = assembly.GetType("Grammar")!;
		var counter = host.GetField("Calls")!;
		Assert.Equal(12, EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).Value);
		Assert.Equal(calls, counter.GetValue(null));
		foreach (var bytes in new[] { false, true })
		{
			counter.SetValue(null, 0);
			using var reader = new StringReader(input);
			using var stream = new MemoryStream(Encoding.ASCII.GetBytes(input));
			Assert.Equal(12, host.GetMethod("ParseStart", [bytes ? typeof(Stream) : typeof(TextReader), typeof(int?), typeof(int?)])!
				.Invoke(null, [bytes ? stream : reader, 1, 32]));
			Assert.Equal(calls, counter.GetValue(null));
		}
	}

	[Fact]
	public void Scalar_run_with_overlapping_follow_keeps_backtracking()
	{
		var result = GramCompiler.Compile("""
			Number : @int = digits: ['0'..'9']+ => @(digits.Length)
			Start : @int = n: Number & '2' & when @(n == 2) => @(n)
			parse Start
			""", new GramCompilerOptions
		{
			BufferedInput = true, BufferedBytes = true, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;
		Assert.DoesNotContain("static int Read_Number", source);
		var assembly = EmittedCode.Compile(source);
		Assert.Equal(2, EmittedCode.Match(assembly, "Grammar", "TryParseStart", "122").Value);
	}

	static Assembly Compile(string grammar, string members = "", bool lexical = false, bool direct = true)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			BufferedInput = !lexical, BufferedBytes = !lexical, Lexical = lexical, Direct = direct, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		return EmittedCode.Compile(Assert.Single(result.Sources).Text, declarationMembers: members);
	}

	[Theory]
	[InlineData("1", "case 1", 10)]
	[InlineData("-1", "case -1", 10)]
	[InlineData("\"binary\"", "case \"binary\"", 10)]
	[InlineData("2", "case 1", 20)]
	public void Computed_selection_runs_once_on_all_input_forms(string value, string label, int expected)
	{
		var type = value.StartsWith('"') ? "string" : "int";
		var assembly = Compile("Start : @int = tag: 'x' & switch @(Pick(tag)) { " + label +
			": 'a' => @(10) default: 'a' => @(20) }\nparse Start",
			$"public static int Calls; static {type} Pick(string tag) {{ Calls++; return {value}; }} static {type} Pick(global::System.ReadOnlySpan<byte> tag) {{ Calls++; return {value}; }}");
		var host = assembly.GetType("Grammar")!;
		Assert.Equal(expected, EmittedCode.Match(assembly, "Grammar", "TryParseStart", "xa").Value);
		foreach (var bytes in new[] { false, true })
		{
			using var reader = new StringReader("xa");
			using var stream = new MemoryStream(Encoding.ASCII.GetBytes("xa"));
			Assert.Equal(expected, host.GetMethod("ParseStart", [bytes ? typeof(Stream) : typeof(TextReader), typeof(int?), typeof(int?)])!
				.Invoke(null, [bytes ? stream : reader, 1, 10]));
		}
		Assert.Equal(3, host.GetField("Calls")!.GetValue(null));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Trailing_switch_keeps_outer_captures_when_another_group_is_lowered(bool lexical)
	{
		var assembly = Compile((lexical ? "trivia = { ' '* }\n" : "") + """
			Start : @int = n: ('x' => @(10) | 'y' => @(20)) & switch @(n) {
				case 10: 'a' => @(n + 1)
				default: 'b' => @(n + 2)
			}
			parse Start
			""", lexical: lexical);

		Assert.Equal(11, EmittedCode.Match(assembly, "Grammar", "TryParseStart", "xa").Value);
		Assert.Equal(22, EmittedCode.Match(assembly, "Grammar", "TryParseStart", "yb").Value);
	}

	[Fact]
	public void Failed_selected_case_does_not_try_default_but_outer_choice_can_backtrack()
	{
		var assembly = Compile("""
			Selected : @int = switch @(Pick()) { case 1: 'a' & 'b' => @(1) default: 'a' => @(2) }
			Outer : @int = v: Selected => @(v) | 'a' => @(3)
			parse Selected
			parse Outer
			""", "public static int Calls; static int Pick() { Calls++; return 1; }");
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseSelected", "a").IsSuccess);
		Assert.Equal(3, EmittedCode.Match(assembly, "Grammar", "TryParseOuter", "a").Value);
		Assert.Equal(2, assembly.GetType("Grammar")!.GetField("Calls")!.GetValue(null));
	}

	[Fact]
	public void Unmatched_switch_without_default_fails()
	{
		var assembly = Compile("Start = switch @(2) { case 1: 'a' }\nparse Start");
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "a").IsSuccess);
	}
	[Theory]
	[InlineData("switch @(Pick()) { case 1: 'a' default: 'b' }", "a")]
	[InlineData("{ switch @(Pick()) { case 1: 'a' default: 'b' } }", "a")]
	[InlineData("switch @(Pick()) { case 1: switch @(1) { case 1: 'a' } }", "a")]
	[InlineData("switch @(Pick()) { case 0: case 1: 'a' }", "a")]
	[InlineData("switch @(Pick()) { default: 'a' }", "a")]
	public void Atomic_nested_and_default_only_switches_preserve_selection(string expression, string input)
	{
		var assembly = Compile("Start = " + expression + "\nparse Start", "public static int Calls; static int Pick() { Calls++; return 1; }");
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).IsSuccess);
		Assert.Equal(1, assembly.GetType("Grammar")!.GetField("Calls")!.GetValue(null));
	}

	[Fact]
	public void Typed_captures_with_rebinding_and_yield_reach_the_selector()
	{
		var assembly = Compile("""
			Number : @int = '1' => @(1)
			End = ';'
			Item : @int = n: Number & switch @(n) { case 1: End => @(n) default: 'x' => @(2) }
			Feed : @int[] = Item*
			parse Feed with (End = '!') as Items stream bytes yield : @int
			""");
		using var input = new MemoryStream(Encoding.ASCII.GetBytes("1!1!"));
		var items = (System.Collections.Generic.IEnumerable<int>)assembly.GetType("Grammar")!.GetMethod("Items", [typeof(Stream), typeof(int?), typeof(int?)])!
			.Invoke(null, [input, 1, 16])!;
		Assert.Equal(new[] { 1, 1 }, items.ToArray());
	}

	[Theory]
	[InlineData("case 1: 'a' case 1: 'b'")]
	[InlineData("default: 'a' default: 'b'")]
	[InlineData("case 1: 'a' case \"1\": 'b'")]
	[InlineData("")]
	public void Invalid_cases_report_grammar_diagnostics(string cases)
	{
		var result = GramCompiler.Compile("Start = switch @(1) { " + cases + " }\nparse Start");
		Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Severity == GramSeverity.Error);
	}

	[Fact]
	public void Lexical_syntax_keeps_computed_selection()
	{
		var assembly = Compile("trivia = { ' '* }\nStart = switch @(2) { case 1: 'a' default: 'b' }\nparse Start", lexical: true);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "a").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", " b").IsSuccess);
	}

	[Fact]
	public void Branch_results_keep_implicit_construction()
	{
		var assembly = Compile("""
			A : @int = 'a' => @(1)
			Start : A = switch @(2) { case 1: A default: A }
			parse Start
			""");
		Assert.Equal(1, EmittedCode.Match(assembly, "Grammar", "TryParseStart", "a").Value);
	}

	[Theory]
	[InlineData("-9223372036854775808L", "-9223372036854775808")]
	[InlineData("18446744073709551615UL", "18446744073709551615")]
	public void Integral_selectors_keep_their_full_range(string selector, string label)
	{
		var assembly = Compile("Start = switch @(" + selector + ") { case " + label + ": 'a' }\nparse Start");
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "a").IsSuccess);
	}


	static Assembly CompileDirect(string grammar, string members = "", bool lexical = false, bool direct = true)
	{
		var result = GramCompiler.Compile((lexical ? "trivia = { ' '* }\n" : "") + grammar,
			new GramCompilerOptions { Direct = direct, Lexical = lexical, CSharpScanner = RoslynCSharpScanner.Instance });
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;
		if (direct)
			Assert.Contains("Read_", source);
		return EmittedCode.Compile(source, declarationMembers: members);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Direct_selector_uses_typed_and_text_captures_once(bool lexical)
	{
		var assembly = CompileDirect("""
			Number : @int = '1' => @(Build())
			Start : @int = tag: 'x' & n: Number & switch @(Pick(tag, n)) {
				case 1: 'a' => @(n)
				default: 'b' => @(0)
			}
			parse Start
			""", """public static int Builds, Calls; static int Build() { Builds++; return 7; } static int Pick(string tag, int n) { Calls++; return tag == "x" && n == 7 ? 1 : 0; }""", lexical);
		var match = EmittedCode.Match(assembly, "Grammar", "TryParseStart", "x1a");
		Assert.True(match.IsSuccess, match.Error);
		Assert.Equal(7, match.Value);
		Assert.Equal(1, assembly.GetType("Grammar")!.GetField("Builds")!.GetValue(null));
		Assert.Equal(1, assembly.GetType("Grammar")!.GetField("Calls")!.GetValue(null));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Direct_selected_failure_preserves_outer_choice(bool lexical)
	{
		var assembly = CompileDirect("""
			Selected : @int = switch @(Pick()) { case 1: 'a' & 'b' => @(1) default: 'a' => @(2) }
			Outer : @int = v: Selected => @(v) | 'a' => @(3)
			parse Selected
			parse Outer
			""", "public static int Calls; static int Pick() { Calls++; return 1; }", lexical);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseSelected", "a").IsSuccess);
		Assert.Equal(3, EmittedCode.Match(assembly, "Grammar", "TryParseOuter", "a").Value);
		Assert.Equal(2, assembly.GetType("Grammar")!.GetField("Calls")!.GetValue(null));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Repeated_switch_cannot_be_replaced_by_a_character_set(bool direct)
	{
		var assembly = CompileDirect("Start = (switch @(Pick()) { case 1: 'a' default: 'b' })+ & eof\nparse Start",
			"public static int Calls; static int Pick() { Calls++; return 1; }", direct: direct);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "aaa").IsSuccess);
		Assert.True((int)assembly.GetType("Grammar")!.GetField("Calls")!.GetValue(null)! >= 3);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "b").IsSuccess);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Selected_branch_keeps_its_own_backtracking(bool direct)
	{
		var assembly = CompileDirect("Start = switch @(1) { case 1: ('a' | ('a' & 'b')) default: 'z' } & 'c' & eof\nparse Start", direct: direct);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "abc").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "zc").IsSuccess);
	}


	[Theory]
	[InlineData("-9223372036854775808L", "-9223372036854775808")]
	[InlineData("18446744073709551615UL", "18446744073709551615")]
	[InlineData("\"kind\"", "\"kind\"")]
	public void Direct_selector_preserves_key_types(string selector, string label)
	{
		var assembly = CompileDirect("Start = switch @(" + selector + ") { case " + label + ": 'a' }\nparse Start", lexical: true);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "a").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "b").IsSuccess);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Direct_null_string_uses_default_and_missing_label_fails(bool lexical)
	{
		var assembly = CompileDirect("""
			Start = switch @((string?)null) { case "key": 'b' default: 'a' }
			Missing = switch @(2) { case 1: 'a' }
			parse Start
			parse Missing
			""", lexical: lexical);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "a").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseMissing", "a").IsSuccess);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Direct_observations_are_not_pruned_by_the_selected_body(bool lexical)
	{
		var assembly = CompileDirect("Start = switch @(Pick()) { case 1: 'a' } | 'b'\nparse Start",
			"public static int Calls; static int Pick() { Calls++; return 1; }", lexical);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "b").IsSuccess);
		Assert.Equal(1, assembly.GetType("Grammar")!.GetField("Calls")!.GetValue(null));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Direct_switch_materialization_is_invalidated_after_outer_rollback(bool lexical)
	{
		var assembly = CompileDirect("""
			A : @int = 'a' => @(++Calls)
			B : @int = 'a' => @(10 + ++Calls)
			Start : @int = n: A & switch @(Key(n)) { case 1: '?' => @(n) }
				| n: B & switch @(Key(n)) { case 12: '!' => @(n) }
			parse Start
			""", "public static int Calls; static int Key(int? value) => value ?? -1;", lexical);
		Assert.Equal(12, EmittedCode.Match(assembly, "Grammar", "TryParseStart", "a!").Value);
		Assert.Equal(2, assembly.GetType("Grammar")!.GetField("Calls")!.GetValue(null));
	}


	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Retrying_inside_a_case_does_not_reselect_the_case(bool direct)
	{
		var assembly = CompileDirect("Start = switch @(++Calls) { case 1: ('a' | ('a' & 'b')) default: 'z' } & 'c' & eof\nparse Start",
			"public static int Calls;", direct: direct);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "abc").IsSuccess);
		Assert.Equal(1, assembly.GetType("Grammar")!.GetField("Calls")!.GetValue(null));
	}


	[Fact]
	public void Lexical_computed_selection_keeps_committed_rule_calls()
	{
		var assembly = CompileDirect("""
			Part = 'a' | ('a' & 'b')
			Start = switch @(1) { case 1: Part } & 'c' & eof
			parse Start
			""", lexical: true);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "ac").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "abc").IsSuccess);
	}

}
