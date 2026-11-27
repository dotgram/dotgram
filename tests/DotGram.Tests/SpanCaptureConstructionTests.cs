using System;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Where captures are spans, a type the host declares is handed the text as the parameter it fills takes it: a string, or the
/// characters as a span.
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
