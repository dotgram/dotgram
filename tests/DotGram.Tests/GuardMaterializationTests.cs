using System;
using System.Linq;
using DotGram.Generation;
using DotGram.Grammar;
using Xunit;

namespace DotGram.Tests;

public sealed class GuardMaterializationTests
{
	[Theory]
	[InlineData(ValueStorageKind.Auto, false)]
	[InlineData(ValueStorageKind.Auto, true)]
	[InlineData(ValueStorageKind.Flat, false)]
	[InlineData(ValueStorageKind.Adaptive, false)]
	[InlineData(ValueStorageKind.Paged, false)]
	public void Repeated_guards_reuse_values_but_rollback_invalidates_reused_record_numbers(ValueStorageKind storage, bool large)
	{
		var grammar = """
			Start : @int = a: Value & when @(a > 0) & Gap? & when @(a > 0) & '?' => @(a)
				| b: Other & when @(b > 0) & '!' => @(b)
			Value : @int = 'a' => @(Next())
			Other : @int = 'a' => @(10 + Next())
			Gap : @int = 'b' => @(Unused())
			parse Start
			""";
		if (large)
			grammar = grammar.Replace("Gap : @int = 'b' => @(Unused())", "Gap : @int = 'b' => @(Unused()) | v: Filler => @(v)") +
				"\nFiller : @int = " + string.Join(" | ", Enumerable.Range(0, 11).Select(g => "v: Group" + g + " => @(v)")) +
				string.Join("", Enumerable.Range(0, 11).Select(g => "\nGroup" + g + " : @int = " +
					string.Join(" | ", Enumerable.Range(g * 100, 100).Select(i =>
						"'" + (char)(0xE000 + i) + "' => @(" + i + ")"))));
		var compilation = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = CarrierKind.Tape, ValueStorage = storage,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(compilation.Diagnostics);
		var source = Assert.Single(compilation.Sources).Text;
		Assert.Equal(large, source.Contains("MemoryExtensions.IndexOf"));
		var assembly = EmittedCode.Compile(source, declarationMembers: """
			public static int Calls;
			static int Next() => ++Calls;
			static int Unused() => throw new System.InvalidOperationException("Uncaptured factory");
			""");
		var calls = assembly.GetType("Grammar")!.GetField("Calls")!;
		foreach (var (input, value, count) in new[] { ("a?", 1, 1), ("ab?", 1, 1), ("a!", 12, 2), ("a?", 1, 1) })
		{
			calls.SetValue(null, 0);
			var match = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);
			Assert.True(match.IsSuccess, match.Error);
			Assert.Equal(value, match.Value);
			Assert.Equal(count, calls.GetValue(null));
		}
		calls.SetValue(null, 0);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "ax").IsSuccess);
		Assert.Equal(2, calls.GetValue(null));
	}

	[Theory]
	[InlineData(ValueStorageKind.Flat)]
	[InlineData(ValueStorageKind.Adaptive)]
	[InlineData(ValueStorageKind.Paged)]
	public void Engine_switch_builds_only_requested_values_in_repeated_records(ValueStorageKind storage)
	{
		var compilation = GramCompiler.Compile("""
			Number : @int = '1' => @(One())
			Pair : @int = a: Number & '/' & b: Number => @(a + b)
			Unused : @int = 'x' => @(Unexpected())
			Item : @int = Unused & n: Pair & switch @(n) { case 2: ';' => @(n) }
			Feed : @int[] = Item+
			parse Feed
			""", new GramCompilerOptions { Direct = false, ValueStorage = storage, CSharpScanner = RoslynCSharpScanner.Instance });
		EmittedCode.Quiet(compilation.Diagnostics);
		var source = Assert.Single(compilation.Sources).Text;
		var assembly = EmittedCode.Compile(source, declarationMembers: """
			public static int Calls;
			static int One() { Calls++; return 1; }
			static int Unexpected() => throw new System.InvalidOperationException("Unrequested value");
			""");
		var input = string.Concat(Enumerable.Repeat("x1/1;", 1000));
		var match = EmittedCode.Match(assembly, "Grammar", "TryParseFeed", input);
		Assert.True(match.IsSuccess, match.Error);
		Assert.Equal(Enumerable.Repeat(2, 1000), Assert.IsType<int[]>(match.Value));
		Assert.Equal(2000, assembly.GetType("Grammar")!.GetField("Calls")!.GetValue(null));
	}

	[Fact]
	public void Engine_guard_bound_includes_the_earliest_pending_capture()
	{
		var compilation = GramCompiler.Compile("""
			Number : @int = '1' => @(One())
			Item : @int = a: Number & b: Number & when @(b == 1) & when @(a + b == 2) & ';' => @(a + b)
			Feed : @int[] = Item+
			parse Feed
			""", new GramCompilerOptions { Direct = false, CSharpScanner = RoslynCSharpScanner.Instance });
		EmittedCode.Quiet(compilation.Diagnostics);
		var assembly = EmittedCode.Compile(Assert.Single(compilation.Sources).Text,
			declarationMembers: "public static int Calls; static int One() { Calls++; return 1; }");
		Assert.Equal(new[] { 2, 2 }, EmittedCode.Match(assembly, "Grammar", "TryParseFeed", "11;11;").Value);
		Assert.Equal(4, assembly.GetType("Grammar")!.GetField("Calls")!.GetValue(null));
	}

}
