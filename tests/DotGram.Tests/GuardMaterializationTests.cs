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
}
