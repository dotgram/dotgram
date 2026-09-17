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
	static Assembly Compile(string grammar, string members = "", bool lexical = false)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			BufferedInput = !lexical, BufferedBytes = !lexical, Lexical = lexical, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics.Where(one => !lexical || one.Id != "GRAM5005"));
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
			Assert.Equal(expected, host.GetMethod("ParseStart", [bytes ? typeof(Stream) : typeof(TextReader), typeof(int), typeof(int)])!
				.Invoke(null, [bytes ? stream : reader, 1, 10]));
		}
		Assert.Equal(3, host.GetField("Calls")!.GetValue(null));
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
		var items = (System.Collections.Generic.IEnumerable<int>)assembly.GetType("Grammar")!.GetMethod("Items", [typeof(Stream), typeof(int), typeof(int)])!
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

}
