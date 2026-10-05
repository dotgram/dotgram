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
		var (assembly, source) = Compile(false);
		var owner = assembly.GetType("Grammar")!;
		var input = string.Concat(Enumerable.Repeat("abcdefgh", 257));
		const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
		var store = owner.GetNestedType("DirectValues", Flags)!;
		var tables = store.GetFields(Flags).Where(field => field.Name.StartsWith("V", StringComparison.Ordinal) && field.FieldType.IsArray).ToArray();
		Assert.True(tables.Length >= 8);

		// Few tables: Return tests every table's count, and keeps no list of the ones written.
		Assert.DoesNotContain("Written", source);
		Assert.Contains("if (slot >= values.N", source);

		// After either entry — the appending machine (Start) and the one that writes by record
		// under a guard (Guarded) — the parked store holds no value.
		foreach (var entry in new[] { "TryParseStart", "TryParseGuarded" })
		{
			Assert.True(EmittedCode.Match(assembly, "Grammar", entry, input).IsSuccess);
			var spare = store.GetField("_spare", Flags)!.GetValue(null)!;
			foreach (var table in tables)
			{
				var array = (Array)table.GetValue(spare)!;
				// The appending machine's tables hold one value a row; the guarded one writes by
				// record, and its tables reach as far as the record that wrote them.
				Assert.InRange(array.Length, 16, entry == "TryParseStart" ? 512 : 4096);
				foreach (var held in array)
					Assert.Equal(Activator.CreateInstance(held!.GetType()), held);
			}
		}

		owner.GetField("Fail")!.SetValue(null, true);
		Assert.ThrowsAny<Exception>(() => EmittedCode.Match(assembly, "Grammar", "TryParseStart", input));
		owner.GetField("Fail")!.SetValue(null, false);
		owner.GetField("Reenter")!.SetValue(null, true);
		Assert.Equal("1234567h", EmittedCode.Match(assembly, "Grammar", "TryParseStart", "abcdefgh").Value);
	}

	[Fact]
	public void Many_dense_tables_are_emptied_by_the_list_of_those_written()
	{
		// Thirty-three value types: past the count at which the store keeps a list of the
		// tables a parse wrote, for Return to empty those and read nothing of the rest. The
		// padding's factories are never reached, so their tables are never written.
		var grammar = Grammar.Replace("Row : @string = Dead?", "Row : @string = Padding & Dead?") +
			"\nPadding = " + string.Join(" & ", Enumerable.Range(0, 24).Select(i => "Spare" + i + "?")) + "\n" +
			string.Join("\n", Enumerable.Range(0, 24).Select(i =>
				"Spare" + i + " : @SpareValue" + i + " = '" + (char)(0xE000 + i) + "' => @(UnusedSpare<SpareValue" + i + ">())"));
		var spares = "static T UnusedSpare<T>() => throw new System.InvalidOperationException(\"Unused spare factory\");\n" +
			string.Join("\n", Enumerable.Range(0, 24).Select(i => "public sealed class SpareValue" + i + " {}"));
		var (assembly, source) = Compile(false, grammar, extraMembers: spares);
		var owner = assembly.GetType("Grammar")!;
		const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
		var store = owner.GetNestedType("DirectValues", Flags)!;
		var tables = store.GetFields(Flags).Where(field => field.Name.StartsWith("V", StringComparison.Ordinal) && field.FieldType.IsArray).ToArray();
		Assert.True(tables.Length >= 32);

		// The appending machine (Start) lists a table as it adds the first value; the one that
		// writes by record under a guard (Guarded) lists it as it raises the table's count.
		Assert.Contains("Written[WrittenCount++]", source);
		Assert.Contains("values.Wrote(", source);
		Assert.Contains("switch (values.Written[written])", source);

		var input = string.Concat(Enumerable.Repeat("abcdefgh", 257));

		foreach (var entry in new[] { "TryParseStart", "TryParseGuarded", "TryParseStart" })
		{
			var match = EmittedCode.Match(assembly, "Grammar", entry, input);
			Assert.True(match.IsSuccess, match.Error);
			Assert.Equal(string.Join("|", Enumerable.Repeat("1234567h", 257)), match.Value);

			// Returned: no value in any table, the counts at zero, and the list empty.
			var spare = store.GetField("_spare", Flags)!.GetValue(null)!;
			Assert.Equal(0, store.GetField("WrittenCount", Flags)!.GetValue(spare));
			foreach (var table in tables)
			{
				Assert.Equal(0, store.GetField("N" + table.Name.Substring(1), Flags)!.GetValue(spare));
				foreach (var held in (Array)table.GetValue(spare)!)
					Assert.Equal(Activator.CreateInstance(held!.GetType()), held);
			}
		}
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

	static (Assembly Assembly, string Source) Compile(bool lexical, string grammar = Grammar, ValueStorageKind storage = ValueStorageKind.Auto, string extraMembers = "")
	{
		var compiled = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = CarrierKind.Tape, ValueStorage = storage, Lexical = lexical,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
		Assert.DoesNotContain(compiled.Diagnostics, one => one.Severity != GramSeverity.Info);
		var source = Assert.Single(compiled.Sources).Text;
		return (EmittedCode.Compile(source, declarationMembers: Members + "\n" + extraMembers), source);
	}
}
