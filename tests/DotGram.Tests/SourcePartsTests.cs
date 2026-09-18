using System;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.Tests;

public sealed class SourcePartsTests
{
	const string Grammar = """
		Start : @int = n: Number & when @(n == 1) & '!' => @(n)
		    | n: Number & '?' => @(n + 10)
		Number : @int = 'a' => @(1)
		parse Start
		""";

	[Theory]
	[InlineData(ValueStorageKind.Flat, false)]
	[InlineData(ValueStorageKind.Flat, true)]
	[InlineData(ValueStorageKind.Adaptive, false)]
	[InlineData(ValueStorageKind.Adaptive, true)]
	public void Separated_engines_keep_bodies_and_parse_all_input_forms(ValueStorageKind storage, bool direct)
	{
		GramCompilation Compile(int size) => GramCompiler.Compile(Grammar, new GramCompilerOptions
		{
			Direct = direct, BufferedInput = true, BufferedBytes = true,
			ValueStorage = storage, Portable = true, SourceFileSize = size, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		var whole = Compile(0);
		var split = Compile(1);
		EmittedCode.Quiet(split.Diagnostics);
		Assert.Single(whole.Sources);
		Assert.True(split.Sources.Count > 1);
		Assert.Equal(split.Sources.Count, split.Sources.Select(s => s.HintName).Distinct().Count());
		Assert.Equal(Bodies(whole), Bodies(split));
		Assert.Single(split.Sources, s => s.Text.Contains("[global::DotGram.GramSourceAttribute("));
		var assembly = EmittedCode.Compile(split.Sources[0].Text, sourceParts: split.Sources.Skip(1).Select(s => s.Text));
		Assert.Equal(1, EmittedCode.Match(assembly, "Grammar", "TryParseStart", "a!").Value);
		Assert.Equal(11, EmittedCode.Match(assembly, "Grammar", "TryParseStart", "a?").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "ax").IsSuccess);
		var host = assembly.GetType("Grammar")!;
		using var reader = new StringReader("a?");
		using var bytes = new MemoryStream(Encoding.ASCII.GetBytes("a?"));
		Assert.Equal(11, host.GetMethod("ParseStart", [typeof(TextReader), typeof(int), typeof(int)])!.Invoke(null, [reader, 1, 100]));
		Assert.Equal(11, host.GetMethod("ParseStart", [typeof(Stream), typeof(int), typeof(int)])!.Invoke(null, [bytes, 1, 100]));
	}

	[Fact]
	public void Partial_headers_preserve_nested_generic_hosts_imports_and_suffix()
	{
		var result = GramCompiler.Compile("@using System;\nStart : @int = 'x' => @(Math.Abs(-7))\nparse Start", new GramCompilerOptions
		{
			ClassName = "Outer<TContext>.Inner", Namespace = "Example", Suffix = "Reading",
			Direct = false, BufferedInput = true, SourceFileSize = 1, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		Assert.True(result.Sources.Count > 1);
		Assert.DoesNotContain(result.Sources, s => s.Text.Contains("[global::DotGram.GramSourceAttribute("));
		EmittedCode.Compile(result.Sources[0].Text, "Outer<TContext>", "Example", "public partial class Inner { }", result.Sources.Skip(1).Select(s => s.Text));
	}

	[Fact]
	public void Small_output_is_unchanged_and_repeat_compilations_use_stable_names()
	{
		var baseline = GramCompiler.Compile(Grammar, new GramCompilerOptions { Direct = false, SourceFileSize = 0, CSharpScanner = RoslynCSharpScanner.Instance });
		var normal = GramCompiler.Compile(Grammar, new GramCompilerOptions { Direct = false, CSharpScanner = RoslynCSharpScanner.Instance });
		Assert.Equal(Assert.Single(baseline.Sources).Text, Assert.Single(normal.Sources).Text);
		var split = GramCompiler.Compile(Grammar, new GramCompilerOptions { Direct = false, SourceFileSize = 1, CSharpScanner = RoslynCSharpScanner.Instance });
		var repeated = GramCompiler.Compile(Grammar, new GramCompilerOptions { Direct = false, SourceFileSize = 1, CSharpScanner = RoslynCSharpScanner.Instance });
		Assert.Equal(split.Sources.ToArray(), repeated.Sources.ToArray());
	}

	static string[] Bodies(GramCompilation result) => result.Sources
		.SelectMany(source => CSharpSyntaxTree.ParseText(source.Text).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
		.Select(method => method.Identifier.ValueText + method.Body?.ToFullString() + method.ExpressionBody?.ToFullString()).Order().ToArray();
}
