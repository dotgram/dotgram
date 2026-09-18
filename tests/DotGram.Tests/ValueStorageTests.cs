using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

public sealed class ValueStorageTests
{
	const string Grammar = """
		trivia = { ' '* }
		Start : @string = items: Item+ => @(string.Join("", items))
		Item : @string = text: 'a' => @(text)
		parse Start
		""";

	[Theory]
	[InlineData(false, ValueStorageKind.Auto)]
	[InlineData(true, ValueStorageKind.Auto)]
	[InlineData(false, ValueStorageKind.Flat)]
	[InlineData(true, ValueStorageKind.Flat)]
	[InlineData(false, ValueStorageKind.Adaptive)]
	[InlineData(true, ValueStorageKind.Adaptive)]
	public void Small_grammar_honors_explicit_storage(bool lexical, ValueStorageKind storage)
	{
		var compiled = GramCompiler.Compile(Grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = CarrierKind.Tape, ValueStorage = storage,
			Lexical = lexical, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(compiled.Diagnostics);
		var source = Assert.Single(compiled.Sources).Text;
		Assert.Equal(storage == ValueStorageKind.Adaptive, source.Contains("struct ValueTable<T>"));
		var assembly = EmittedCode.Compile(source);
		foreach (var count in new[] { 1, 255, 256, 257, 1025, 2 })
		{
			var text = new string('a', count);
			var result = EmittedCode.Match(assembly, "Grammar", "TryParseStart", text);
			Assert.True(result.IsSuccess, result.Error);
			Assert.Equal(text, result.Value);
			Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", text + "!").IsSuccess);
		}
	}

	[Theory]
	[InlineData(CarrierKind.Immediate, true)]
	[InlineData(CarrierKind.Mixed, true)]
	[InlineData(CarrierKind.Tape, false)]
	public void Readers_without_direct_value_tables_ignore_storage(CarrierKind carrier, bool direct)
	{
		string? expected = null;
		foreach (var storage in new[] { ValueStorageKind.Auto, ValueStorageKind.Flat, ValueStorageKind.Adaptive })
		{
			var compiled = GramCompiler.Compile(Grammar, new GramCompilerOptions
			{
				ClassName = "Grammar", Carrier = carrier, Direct = direct, ValueStorage = storage,
				CSharpScanner = RoslynCSharpScanner.Instance,
			});
			EmittedCode.Quiet(compiled.Diagnostics);
			var source = Assert.Single(compiled.Sources).Text;
			Assert.DoesNotContain("struct ValueTable<T>", source);
			Assert.Equal(expected ??= source, source);
			Assert.Equal("aaa", EmittedCode.Match(EmittedCode.Compile(source), "Grammar", "TryParseStart", "aaa").Value);
		}
	}

	[Theory]
	[InlineData(ValueStorageKind.Adaptive)]
	public void Mixed_fallback_keeps_explicit_adaptive_storage_and_spans(ValueStorageKind storage)
	{
		var compiled = GramCompiler.Compile("""
			Where : @SourceSpan = 'a'
			Item : @string = w: Where => @(w.Start + ":" + w.Length)
			Start : @string = items: Item+ => @(string.Join("|", items))
			parse Start
			""", new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = CarrierKind.Mixed,
			ValueStorage = storage, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(compiled.Diagnostics);
		Assert.Contains(compiled.Diagnostics, diagnostic => diagnostic.Id == GramCompiler.CarrierRefused);
		var source = Assert.Single(compiled.Sources).Text;
		Assert.Equal(storage == ValueStorageKind.Adaptive, source.Contains("struct ValueTable<T>"));
		var assembly = EmittedCode.Compile(source);
		Assert.Equal(string.Join("|", Enumerable.Range(0, 1025).Select(i => i + ":1")),
			EmittedCode.Match(assembly, "Grammar", "TryParseStart", new string('a', 1025)).Value);
	}
}
