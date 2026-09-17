using System;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

public sealed class DenseValuesTests
{
	const string Grammar = """
		trivia = { ' '* }
		Start : @string = rows: Row+ => @(string.Join("|", rows))
		Guarded : @string = rows: Row+ & when @(rows.Length > 0) => @(string.Join("|", rows))
		Row : @string = Dead? & a: A & b: B & c: C & d: D & e: E & f: F & g: G & h: H
			=> @(a.ToString() + b.ToString() + c.ToString() + d.ToString() + e.ToString() + f.ToString() + g.ToString() + h)
		Dead : @int = 'x' => @(Unused())
		A : @int = 'a' => @(First())
		B : @long = 'b' => @(2L)
		C : @short = 'c' => @((short)3)
		D : @byte = 'd' => @((byte)4)
		E : @float = 'e' => @(5f)
		F : @double = 'f' => @(6d)
		G : @decimal = 'g' => @(7m)
		H : @char = 'h' => @('h')
		parse Start
		parse Guarded
		""";

	const string Members = """
		static int Unused() => throw new System.InvalidOperationException("Unused factory");
		public static bool Fail;
		public static bool Reenter;
		static int First()
		{
			if (Fail) throw new System.InvalidOperationException("Factory failure");
			if (Reenter)
			{
				Reenter = false;
				if (!TryParseStart("abcdefgh").IsSuccess) throw new System.InvalidOperationException("Nested parse failed");
			}
			return 1;
		}
		""";

	[Theory]
	[InlineData(false, ValueStorageKind.Auto)]
	[InlineData(true, ValueStorageKind.Auto)]
	[InlineData(false, ValueStorageKind.Flat)]
	[InlineData(true, ValueStorageKind.Flat)]
	[InlineData(false, ValueStorageKind.Adaptive)]
	[InlineData(true, ValueStorageKind.Adaptive)]
	[InlineData(false, ValueStorageKind.Paged)]
	[InlineData(true, ValueStorageKind.Paged)]
	public void Dense_values_survive_growth_strays_and_alternating_guarded_publications(bool lexical, ValueStorageKind storage)
	{
		var (assembly, source) = Compile(lexical, storage: storage);
		Assert.Equal(storage == ValueStorageKind.Auto, source.Contains("dense: true"));
		Assert.Equal(storage == ValueStorageKind.Adaptive, source.Contains("struct ValueTable<T>"));
		Assert.Contains("var built = values.Built;", source);
		foreach (var count in new[] { 1, 33, 257, 2, 65, 1 })
		{
			var input = string.Concat(Enumerable.Repeat("xabcdefgh", count));
			var expected = string.Join("|", Enumerable.Repeat("1234567h", count));
			foreach (var entry in new[] { "TryParseStart", "TryParseGuarded", "TryParseStart" })
			{
				var match = EmittedCode.Match(assembly, "Grammar", entry, input);
				Assert.True(match.IsSuccess, match.Error);
				Assert.Equal(expected, match.Value);
				Assert.False(EmittedCode.Match(assembly, "Grammar", entry, input[..^1]).IsSuccess);
			}
		}
	}

	[Fact]
	public void Dense_tables_grow_for_their_own_values_and_clear_after_failure_and_reentry()
	{
		var (assembly, _) = Compile(false);
		var owner = assembly.GetType("Grammar")!;
		var input = string.Concat(Enumerable.Repeat("abcdefgh", 257));
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).IsSuccess);
		const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
		var store = owner.GetNestedType("DirectValues", Flags)!;
		var spare = store.GetField("_spare", Flags)!.GetValue(null)!;
		var tables = store.GetFields(Flags).Where(field => field.Name.StartsWith("V", StringComparison.Ordinal) && field.FieldType.IsArray).ToArray();
		Assert.True(tables.Length >= 8);
		foreach (var table in tables)
		{
			var array = (Array)table.GetValue(spare)!;
			Assert.InRange(array.Length, 16, 512);
			foreach (var held in array)
				Assert.Equal(Activator.CreateInstance(held!.GetType()), held);
		}

		owner.GetField("Fail")!.SetValue(null, true);
		Assert.ThrowsAny<Exception>(() => EmittedCode.Match(assembly, "Grammar", "TryParseStart", input));
		owner.GetField("Fail")!.SetValue(null, false);
		owner.GetField("Reenter")!.SetValue(null, true);
		Assert.Equal("1234567h", EmittedCode.Match(assembly, "Grammar", "TryParseStart", "abcdefgh").Value);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Guard_rollback_reuses_records_for_different_types(bool lexical)
	{
		var grammar = Grammar + """

			Retry : @string = a: A & when @(a < 0) & '!' => @(a.ToString())
				| b: Other & '!' => @(b.ToString())
			Other : @long = 'a' => @(2L)
			Retries : @string = rows: Retry+ => @(string.Join("|", rows))
			parse Retries
			""";
		var (assembly, _) = Compile(lexical, grammar);
		foreach (var count in new[] { 1, 257, 1025, 2 })
		{
			var match = EmittedCode.Match(assembly, "Grammar", "TryParseRetries", string.Concat(Enumerable.Repeat("a!", count)));
			Assert.True(match.IsSuccess, match.Error);
			Assert.Equal(string.Join("|", Enumerable.Repeat("2", count)), match.Value);
		}
	}

	static (Assembly Assembly, string Source) Compile(bool lexical, string grammar = Grammar, ValueStorageKind storage = ValueStorageKind.Auto)
	{
		var compiled = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = CarrierKind.Tape, ValueStorage = storage, Lexical = lexical,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
		Assert.DoesNotContain(compiled.Diagnostics, one => one.Severity != GramSeverity.Info);
		var source = Assert.Single(compiled.Sources).Text;
		return (EmittedCode.Compile(source, declarationMembers: Members), source);
	}
}
