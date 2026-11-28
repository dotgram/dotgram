using System;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Language;

using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Where captures are spans, a type the grammar generates holds the text of its captures as strings, and a type the host
/// declares is handed the text as the parameter it fills takes it: a string, or the characters as a span.
/// </summary>
public sealed class SpanCaptureConstructionTests
{
	const string Host = """
		static string Text(System.ReadOnlySpan<char> text) { return text.ToString(); }
		static string Text(string text) { return text; }
		""";

	const string Boxes = Host + """
		public sealed class Box
		{
			public readonly string V;
			public Box(string name) { V = "box:" + name; }
			public Box(Box inner) { V = "box:[" + inner.V + "]"; }
			public override string ToString() { return V; }
		}
		public sealed class Spanning
		{
			public readonly string V;
			public Spanning(System.ReadOnlySpan<char> text) { V = "span:" + text.ToString(); }
			public override string ToString() { return V; }
		}
		""";

	[Theory]
	[InlineData("Start : @Box = name: ('(' & Word & ')')\nWord : @string = c: ['a'..'z']{2} => @(\"W:\" + Text(c))", "(cd)", "box:(cd)")]
	[InlineData("Start : @Box = inner: Plain\nPlain = ['a'..'z']{2}", "cd", "box:cd")]
	[InlineData("Start : @Spanning = text: ['a'..'z']{2}", "cd", "span:cd")]
	public void A_type_the_host_declares_is_handed_the_text_it_takes_over_spans(string grammar, string input, string expected)
	{
		var parser = Build(grammar + "\nparse Start", Boxes, spans: true);
		var read   = EmittedCode.Match(parser, "Grammar", "TryParseStart", input);

		Assert.True(read.IsSuccess);
		Assert.Equal(expected, read.Value!.ToString());
	}

	[Fact]
	public void A_type_the_grammar_generates_holds_the_text_of_its_captures_as_strings_over_spans()
	{
		var parser = Build("Start = x: ['a'..'z']{2} & '<' & y: ('(' & ['a'..'z']{2} & ')')? & ws: (',' & ['a'..'z']{2})* & '>'\nparse Start", Host, spans: true);

		string Members(string input)
		{
			var read = EmittedCode.Match(parser, "Grammar", "TryParseStart", input);

			Assert.True(read.IsSuccess);

			return string.Join("|", read.Value!.GetType().GetProperties().Select(one => one.Name + "=" + (one.GetValue(read.Value) ?? "null")));
		}

		Assert.Equal("X=ab|Y=(cd)|Ws=,ef,gh", Members("ab<(cd),ef,gh>"));
		Assert.Equal("X=ab|Y=null|Ws=", Members("ab<>"));
	}

	/// <summary>
	/// A name of the constructor under a repetition fills it on every route, the one with no turns included: a run of no
	/// turns is no text, not no name. Over characters and over spans alike.
	/// </summary>
	[Theory]
	[InlineData("<>", "box:", false)]
	[InlineData("<,ef,gh>", "box:efgh", false)]
	[InlineData("<>", "box:", true)]
	[InlineData("<,ef,gh>", "box:efgh", true)]
	public void A_name_of_the_constructor_under_a_repetition_fills_it_with_no_turns_too(string input, string expected, bool spans)
	{
		var parser = Build("Start : @Box = '<' & (',' & name: Plain)* & '>'\nPlain = ['a'..'z']{2}\nparse Start", Boxes, spans);
		var read   = EmittedCode.Match(parser, "Grammar", "TryParseStart", input);

		Assert.True(read.IsSuccess);
		Assert.Equal(expected, read.Value!.ToString());
	}

	/// <summary>
	/// Names choose the construction for the whole rule, so a route on which none of them stands is refused: an option that took
	/// no turn is no name, where a repetition that took none still fills the constructor with an empty text.
	/// </summary>
	[Theory]
	[InlineData("Start : @Box = '<' & (name: Plain)? & '>'\nPlain = ['a'..'z']{2}\nparse Start")]
	[InlineData("Start : @Box = name: Plain & '<' & '>' | '!' & Plain\nPlain = ['a'..'z']{2}\nparse Start")]
	public void A_route_on_which_no_name_of_the_constructor_stands_is_refused(string grammar)
	{
		var host   = CSharpCompilation.Create(
			"Host",
			[CSharpSyntaxTree.ParseText($"public partial class Grammar {{ {Boxes} }}", cancellationToken: TestContext.Current.CancellationToken)],
			EmittedCode.References);
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = new RoslynSymbolResolver(host, "Grammar"),
		});

		Assert.Contains(result.Diagnostics, static one => one.Severity == GramSeverity.Error && one.Id == GrammarNormalizer.UnbuiltConstruction);
	}

	static Assembly Build(string grammar, string members, bool spans = false)
	{
		var host   = CSharpCompilation.Create(
			"Host",
			[CSharpSyntaxTree.ParseText($"public partial class Grammar {{ {members} }}", cancellationToken: TestContext.Current.CancellationToken)],
			EmittedCode.References);
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = new RoslynSymbolResolver(host, "Grammar"),
			SpanCaptures   = spans,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text, declarationMembers: members, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}
}
