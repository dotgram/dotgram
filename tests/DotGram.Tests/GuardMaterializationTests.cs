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
		// Twice per reading, and a refusal is read twice: quietly, then recording what it
		// says (Q7.2). Each reading still builds each value once.
		Assert.Equal(4, calls.GetValue(null));
	}

	[Theory]
	[InlineData(ValueStorageKind.Flat)]
	[InlineData(ValueStorageKind.Adaptive)]
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

	/// <summary>
	/// A guarded rule whose group is read by a method of its own hands that method the rule's
	/// log mark — only where the carrier keeps one: the immediate carrier without a context
	/// has none, and once handed a mark it never declared, the build failed with CS0103.
	/// </summary>
	[Theory]
	[InlineData(CarrierKind.Tape, "engine")]
	[InlineData(CarrierKind.Tape, "flat")]
	[InlineData(CarrierKind.Tape, "reader")]
	[InlineData(CarrierKind.Immediate, "engine")]
	[InlineData(CarrierKind.Immediate, "flat")]
	[InlineData(CarrierKind.Immediate, "reader")]
	public void A_guard_beside_a_group_read_in_parts_builds(CarrierKind carrier, string rendering)
	{
		// Recursion keeps the rule out of the flat rendering, so that it is read by methods.
		var grammar =
			"T : @string = h: ['0'..'9'] & ('.' & f: ['0'..'9'])? & when @(Seen(h!)) => @(h! + f)\n" +
			(rendering == "reader"
				? "S : @string = t: T => @(t) | '(' & s: S & ')' => @(s)\nparse S\n"
				: "parse T\n");

		var compilation = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = carrier, Direct = rendering != "engine",
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(compilation.Diagnostics);

		var assembly = EmittedCode.Compile(Assert.Single(compilation.Sources).Text,
			declarationMembers: "public static int Guards; static bool Seen(string h) { Guards++; return h != \"0\"; }");

		var entry = rendering == "reader" ? "TryParseS" : "TryParseT";
		var input = rendering == "reader" ? "(1.2)" : "1.2";

		var match = EmittedCode.Match(assembly, "Grammar", entry, input);
		Assert.True(match.IsSuccess, match.Error);
		Assert.Equal("12", match.Value);

		Assert.False(EmittedCode.Match(assembly, "Grammar", entry, input.Replace('1', '0')).IsSuccess);
	}
}
