using System;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

public sealed class AdaptiveValuesTests
{
	static readonly string Grammar = WithPadding("""
		trivia = { ' '* }
		Guarded : @string = rows: CheckedRow+ & when @(rows.Length > 0) => @(string.Join("|", rows))
		CheckedRow : @string = value: Row & when @(value.Length > 0) => @(value)
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
		parse Guarded
		""");

	const string Members = """
		static T UnusedSpare<T>() => throw new System.InvalidOperationException("Unused spare factory");
		static int Unused() => throw new System.InvalidOperationException("Unused factory");
		public static bool Fail;
		public static int Calls;
		public static int FailAfter;
		public static bool Reenter;
		static int First()
		{
			if (Fail || ++Calls == FailAfter) throw new System.InvalidOperationException("Factory failure");
			if (Reenter)
			{
				Reenter = false;
				if (!TryParseGuarded("abcdefgh").IsSuccess) throw new System.InvalidOperationException("Nested parse failed");
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
	public void Adaptive_values_preserve_guard_results_across_page_growth_and_reuse(bool lexical, ValueStorageKind storage)
	{
		var (assembly, source) = Compile(lexical, storage: storage);
		Assert.Equal(storage is ValueStorageKind.Auto or ValueStorageKind.Adaptive, source.Contains("struct ValueTable<T>"));
		Assert.Contains("var built = values.Built;", source);
		foreach (var count in new[] { 1, 16, 31, 32, 33, 257, 1025, 2, 65, 1 })
		{
			var input = string.Concat(Enumerable.Repeat("qxabcdefgh", count));
			var expected = string.Join("|", Enumerable.Repeat("1234567h", count));
			foreach (var entry in new[] { "TryParseGuarded" })
			{
				var calls = assembly.GetType("Grammar")!.GetField("Calls")!;
				calls.SetValue(null, 0);
				var match = EmittedCode.Match(assembly, "Grammar", entry, input);
				Assert.True(match.IsSuccess, match.Error);
				Assert.Equal(expected, match.Value);
				Assert.Equal(count, calls.GetValue(null));
				Assert.False(EmittedCode.Match(assembly, "Grammar", entry, input[..^1]).IsSuccess);
			}
		}
	}

	[Theory]
	[InlineData(ValueStorageKind.Adaptive)]
	[InlineData(ValueStorageKind.Paged)]
	public void Adaptive_tables_clear_references_after_failure_and_reentrant_parsing(ValueStorageKind storage)
	{
		var (assembly, _) = Compile(false, storage: storage);
		var owner = assembly.GetType("Grammar")!;
		var input = string.Concat(Enumerable.Repeat("abcdefgh", 257));
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseGuarded", input).IsSuccess);
		AssertCleared(owner);
		owner.GetField("Fail")!.SetValue(null, true);
		Assert.ThrowsAny<Exception>(() => EmittedCode.Match(assembly, "Grammar", "TryParseGuarded", input));
		owner.GetField("Fail")!.SetValue(null, false);
		AssertCleared(owner);
		owner.GetField("Calls")!.SetValue(null, 0);
		owner.GetField("FailAfter")!.SetValue(null, 64);
		Assert.ThrowsAny<Exception>(() => EmittedCode.Match(assembly, "Grammar", "TryParseGuarded", input));
		owner.GetField("FailAfter")!.SetValue(null, 0);
		AssertCleared(owner);
		owner.GetField("Reenter")!.SetValue(null, true);
		Assert.Equal("1234567h", EmittedCode.Match(assembly, "Grammar", "TryParseGuarded", "abcdefgh").Value);
		AssertCleared(owner);
	}

	static void AssertCleared(Type owner)
	{
		const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
		var store = owner.GetNestedType("DirectValues", Flags)!;
		var spare = store.GetField("_spare", Flags)!.GetValue(null)!;
		var tables = store.GetFields(Flags).Where(field => field.Name.StartsWith("V", StringComparison.Ordinal)).ToArray();
		Assert.True(tables.Length >= 8);
		foreach (var table in tables)
		{
			var value = table.GetValue(spare)!;
			foreach (var field in value.GetType().GetFields(Flags).Where(field => field.FieldType.IsArray))
				if (field.GetValue(value) is Array array) Clear(array);
		}

		static void Clear(Array array)
		{
			foreach (var item in array)
				if (item is Array page) Clear(page);
				else if (item is not null) Assert.Equal(Activator.CreateInstance(item.GetType()), item);
		}
	}

	[Theory]
	[InlineData(false, ValueStorageKind.Adaptive)]
	[InlineData(true, ValueStorageKind.Adaptive)]
	[InlineData(false, ValueStorageKind.Paged)]
	[InlineData(true, ValueStorageKind.Paged)]
	public void Guard_rollback_reuses_records_for_different_types(bool lexical, ValueStorageKind storage)
	{
		var grammar = Grammar + """

			Retry : @string = a: A & when @(a < 0) & '!' => @(a.ToString())
				| b: Other & '!' => @(b.ToString())
			Other : @long = 'a' => @(2L)
			Retries : @string = rows: Retry+ => @(string.Join("|", rows))
			parse Retries
			""";
		var (assembly, _) = Compile(lexical, grammar, storage);
		foreach (var count in new[] { 1, 257, 1025, 2 })
		{
			var match = EmittedCode.Match(assembly, "Grammar", "TryParseRetries", string.Concat(Enumerable.Repeat("a!", count)));
			Assert.True(match.IsSuccess, match.Error);
			Assert.Equal(string.Join("|", Enumerable.Repeat("2", count)), match.Value);
		}
	}

	static string WithPadding(string grammar)
	{
		// Extra reachable value types exercise the wide-table policy; their factories
		// are not needed by these inputs and must remain unbuilt.
		return grammar.Replace("Row : @string = Dead?", "Row : @string = Padding & Dead?") +
			"\nPadding = " + string.Join(" & ", Enumerable.Range(0, 24).Select(i => "Spare" + i + "?")) + "\n" +
			string.Join("\n", Enumerable.Range(0, 24).Select(i =>
				"Spare" + i + " : @SpareValue" + i + " = '" + (i == 0 ? 'q' : (char)(0xE000 + i)) + "' => @(UnusedSpare<SpareValue" + i + ">())"));
	}

	static (Assembly Assembly, string Source) Compile(bool lexical, string? grammar = null, ValueStorageKind storage = ValueStorageKind.Auto)
	{
		var compiled = GramCompiler.Compile(grammar ?? Grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = CarrierKind.Tape, Lexical = lexical, ValueStorage = storage,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
		Assert.DoesNotContain(compiled.Diagnostics, one => one.Severity != GramSeverity.Info);
		var source = Assert.Single(compiled.Sources).Text;
		return (EmittedCode.Compile(source, declarationMembers: Members + "\n" +
			string.Join("\n", Enumerable.Range(0, 24).Select(i => "public sealed class SpareValue" + i + " {}"))), source);
	}
}
