using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

public sealed class YieldPublicationTests
{
	[Fact]
	public void Input_and_output_modes_are_independent()
	{
		var assembly = Compile("""
			Item : @int = 'a' & ';' => @(1) | 'b' & ';' => @(2)
			Feed : @int[] = { Item* }
			parse Feed as All
			parse Feed as Buffered stream
			parse Feed as ByteArray stream bytes
			parse Feed as Lazy yield : @int
			parse Feed as Chars stream yield : @int
			parse Feed as Bytes stream bytes yield : @int
			""");
		var host = assembly.GetType("Grammar")!;
		Assert.Equal(new[] { 1, 2 }, (int[])host.GetMethod("All", [typeof(string)])!.Invoke(null, ["a;b;"])!);
		using var reader = new StringReader("a;b;");
		Assert.Equal(new[] { 1, 2 }, (int[])host.GetMethod("Buffered", [typeof(TextReader), typeof(int?), typeof(int?)])!.Invoke(null, [reader, 1, 32])!);
		using var eagerBytes = new MemoryStream("a;b;"u8.ToArray());
		Assert.Equal(new[] { 1, 2 }, (int[])host.GetMethod("ByteArray", [typeof(Stream), typeof(int?), typeof(int?)])!.Invoke(null, [eagerBytes, 1, 32])!);
		foreach (var method in new[] { "Lazy", "Chars", "Bytes" })
		{
			using var chars = new StringReader("a;b;");
			using var bytes = new MemoryStream("a;b;"u8.ToArray());
			var parameter = method == "Lazy" ? typeof(string) : method == "Chars" ? typeof(TextReader) : typeof(Stream);
			var target = host.GetMethod(method, method == "Lazy" ? [parameter] : [parameter, typeof(int?), typeof(int?)])!;
			Assert.Equal(typeof(System.Collections.Generic.IEnumerable<int>), target.ReturnType);
			var sequence = (IEnumerable)target.Invoke(null, method == "Lazy" ? ["a;b;"] : [method == "Chars" ? chars : bytes, 1, 32])!;
			Assert.Equal(new[] { 1, 2 }, sequence.Cast<int>());
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Yield_is_lazy_strict_and_releases_completed_elements(bool bytes)
	{
		var assembly = Compile("""
			Item : @int = 'a' & ';' => @(1)
			Feed : @int[] = Item*
			parse Feed as Items stream yield
			parse Feed as Bytes stream bytes yield
			""");
		using var reader = new StringReader("a;broken;a;");
		using var stream = new MemoryStream(Encoding.ASCII.GetBytes("a;broken;a;"));
		var method = assembly.GetType("Grammar")!.GetMethod(bytes ? "Bytes" : "Items", [bytes ? typeof(Stream) : typeof(TextReader), typeof(int?), typeof(int?)])!;
		var result = (System.Collections.Generic.IEnumerable<int>)method.Invoke(null, [bytes ? stream : reader, 1, 16])!;
		Assert.Equal(0, stream.Position);
		using var iterator = result.GetEnumerator();
		Assert.True(iterator.MoveNext());
		Assert.Equal(1, iterator.Current);
		Assert.Throws<FormatException>(() => iterator.MoveNext());
		Assert.True(stream.CanRead);
	}

	[Theory]
	[InlineData("*", false)]
	[InlineData("+", true)]
	public void Minimum_count_and_empty_input_are_preserved(string repeat, bool fails)
	{
		var assembly = Compile("Item : @int = 'a' => @(1)\nFeed : @int[] = Item" + repeat + "\nparse Feed yield : @int");
		var result = (System.Collections.Generic.IEnumerable<int>)assembly.GetType("Grammar")!.GetMethod("ParseFeed", [typeof(string)])!.Invoke(null, [""])!;
		if (fails) Assert.Throws<FormatException>(() => result.ToArray());
		else Assert.Empty(result);
	}

	[Theory]
	[InlineData("Feed : @int[] = Item* & 'z'")]
	[InlineData("Feed : @int[] = v: Item* => @(v)")]
	[InlineData("Feed : @int[] = Item* | Item+")]
	public void Unproven_collection_shapes_have_a_grammar_diagnostic(string feed)
	{
		var result = GramCompiler.Compile("Item : @int = 'a' => @(1)\n" + feed + "\nparse Feed yield : @int", Options());
		Assert.Contains(result.Diagnostics, d => d.Id == GrammarNormalizer.UnsafeYield && d.Severity == GramSeverity.Error);
		Assert.Empty(result.Sources);
	}

	[Fact]
	public void Parser_preserves_yield_type_and_modifiers()
	{
		var parsed = GramParser.Parse(GramLexer.Tokenize("parse Feed stream bytes yield : @BaseNode"));
		Assert.False(parsed.HasErrors);
		var publication = Assert.IsType<Decl.Publish>(Assert.Single(parsed.File.Decls));
		Assert.True(publication.BufferedBytes);
		Assert.True(publication.Yield);
		Assert.Equal("BaseNode", publication.ResultType!.Name);
		Assert.True(publication.ResultType.IsCSharp);
	}

	[Theory]
	[InlineData("BaseNode", true)]
	[InlineData("OtherNode", false)]
	public void Explicit_yield_type_is_checked_with_the_real_CSharp_type_system(string type, bool accepted)
	{
		const string declarations = "public class BaseNode {} public class Leaf : BaseNode {} public class OtherNode {}";
		var host = CSharpCompilation.Create("ResultTypes", [CSharpSyntaxTree.ParseText("public partial class Grammar { " + declarations + " }", cancellationToken: TestContext.Current.CancellationToken)],
			[MetadataReference.CreateFromFile(typeof(object).Assembly.Location)], new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		var options = Options();
		options.SymbolResolver = new RoslynSymbolResolver(host, "Grammar");
		var compiled = GramCompiler.Compile("Item : @Leaf = 'a' => @(new Leaf())\nFeed : @Leaf[] = Item*\nparse Feed yield : @" + type, options);
		if (!accepted)
		{
			Assert.Contains(compiled.Diagnostics, d => d.Id == GrammarNormalizer.UnsafeYield && d.Message.Contains("assignable", StringComparison.Ordinal));
			return;
		}
		EmittedCode.Quiet(compiled.Diagnostics);
		var assembly = EmittedCode.Compile(Assert.Single(compiled.Sources).Text, declarationMembers: declarations);
		var method = assembly.GetType("Grammar")!.GetMethod("ParseFeed", [typeof(string)])!;
		Assert.Equal("BaseNode", method.ReturnType.GetGenericArguments()[0].Name);
		Assert.All(((IEnumerable)method.Invoke(null, ["aaa"])!).Cast<object>(), node => Assert.Equal("Leaf", node.GetType().Name));
	}

	[Fact]
	public void Transparent_wrappers_preserve_specialization_and_inferred_type()
	{
		var assembly = Compile("""
			End = ';'
			Item : @int = 'a' & End => @(1)
			Feed : @int[] = { Item* }
			Wrapped : Feed = Feed
			parse Wrapped as Whole
			parse Wrapped with (End = '|') as Rows yield
			""");
		var result = (System.Collections.Generic.IEnumerable<int>)assembly.GetType("Grammar")!.GetMethod("Rows", [typeof(string)])!.Invoke(null, ["a|a|"])!;
		Assert.Equal(new[] { 1, 1 }, result);
		Assert.Equal(new[] { 1, 1 }, (int[])assembly.GetType("Grammar")!.GetMethod("Whole", [typeof(string)])!.Invoke(null, ["a;a;"])!);
	}

	[Fact]
	public void A_giving_back_element_is_rejected()
	{
		var compiled = GramCompiler.Compile("Item? : @int = 'a' => @(1)\nFeed : @int[] = Item*\nparse Feed yield : @int", Options());
		Assert.Contains(compiled.Diagnostics, d => d.Id == GrammarNormalizer.UnsafeYield);
	}

	[Fact]
	public void Yield_does_not_turn_find_into_strict_parsing()
	{
		var compiled = GramCompiler.Compile("Item : @int = 'a' => @(1)\nFeed : @int[] = Item*\nfind Feed yield : @int", Options());
		Assert.Contains(compiled.Diagnostics, d => d.Id == GrammarNormalizer.UnsafeYield);
	}

	[Fact]
	public void Yield_remains_a_rule_name()
	{
		var parsed = GramParser.Parse(GramLexer.Tokenize("A = 'a'\nparse A\nyield : @int = 'b' => @(1)\nparse yield", RoslynCSharpScanner.Instance));
		Assert.False(parsed.HasErrors);
		Assert.Equal(4, parsed.File.Decls.Count);
	}

	static GramCompilerOptions Options() => new() { CSharpScanner = RoslynCSharpScanner.Instance, Direct = false };
	static Assembly Compile(string grammar)
	{
		var compiled = GramCompiler.Compile(grammar, Options());
		EmittedCode.Quiet(compiled.Diagnostics);
		return EmittedCode.Compile(Assert.Single(compiled.Sources).Text);
	}
}
