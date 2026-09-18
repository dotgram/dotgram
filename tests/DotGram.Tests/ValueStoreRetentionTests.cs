using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A finished parse lets go of everything it built: nothing a factory made is reachable from
/// the value store the next parse will rent.
/// </summary>
/// <remarks>
/// <para>
/// The store is kept per thread between parses, so whatever it still points at outlives the
/// parse that built it — a document's worth of tree held by a parser nobody is using. The
/// tables are therefore cleared on the way back, and each is cleared as far as it was
/// written, which is what its mark records.
/// </para>
/// <para>
/// <b>The mixed store is the one to test</b> (D2). A machine that indexes its tables densely
/// raises its own mark as it adds; a machine that indexes by record raises it beside the
/// write; and where a grammar has both kinds and they share one store, a mark missed by
/// either leaves a table uncleared. This grammar has both: <c>Plain</c> is read without
/// guards, and <c>Guarded</c> has guards that name a built value, which is what makes a
/// machine keep its records instead.
/// </para>
/// <para>
/// The emitted code is checked for the two shapes first. Without them the grammar would be
/// compiled some other way and the test would pass by not testing anything.
/// </para>
/// <para>
/// <b>Run alone.</b> A forced collection is the whole process's, and a neighbour holding a
/// reference through a parse of its own would be collected with it.
/// </para>
/// </remarks>
[Collection(typeof(GeneratorCostTests.Alone))]
public sealed class ValueStoreRetentionTests
{
	/// <summary>
	/// Nine value types, so that the store indexes densely where it can, and two publications
	/// — one guarded, one not — so that both kinds of machine write into it.
	/// </summary>
	const string Grammar = """
		trivia = { ' '* }
		Guarded : @int = rows: CheckedRow+ & when @(rows.Length > 0) => @(rows.Length)
		CheckedRow : @string = value: Row & when @(value.Length > 0) => @(value)
		Plain : @int = rows: Row+ => @(rows.Length)
		Row : @string = a: A & b: B & c: C & d: D & e: E & f: F & g: G & h: H
			=> @(Keep(a.ToString() + b.ToString() + c.ToString() + d.ToString() + e.ToString() + f.ToString() + g.ToString() + h))
		A : @int = 'a' => @(1)
		B : @long = 'b' => @(2L)
		C : @short = 'c' => @((short)3)
		D : @byte = 'd' => @((byte)4)
		E : @float = 'e' => @(5f)
		F : @double = 'f' => @(6d)
		G : @decimal = 'g' => @(7m)
		H : @char = 'h' => @('h')
		parse Guarded
		parse Plain
		""";

	/// <summary>
	/// Every row the parse builds, held weakly. A strong reference here would be the thing the
	/// test is looking for, so the factory keeps none.
	/// </summary>
	const string Members = """
		public static readonly System.Collections.Generic.List<System.WeakReference> Rows =
			new System.Collections.Generic.List<System.WeakReference>();
		static string Keep(string row)
		{
			Rows.Add(new System.WeakReference(row));
			return row;
		}
		""";

	[Theory]
	[InlineData("TryParseGuarded")]
	[InlineData("TryParsePlain")]
	public void A_finished_parse_holds_none_of_what_it_built(string entry)
	{
		var (host, source) = Compile();

		Assert.True(source.Contains("internal int N0;"),
			"The store is not the dense-marked one this test is about; the grammar compiled some other way.");
		Assert.True(source.Contains(">= values.N"),
			"No machine raises a mark where it writes, so the store here is not the mixed one D2 is about.");

		var rows = (IList)host.GetField("Rows", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;

		rows.Clear();

		Assert.True(Read(host, entry, "abcdefgh abcdefgh abcdefgh"), $"{entry} did not read its input.");
		Assert.Equal(3, rows.Count);

		Collect();

		var held = new List<int>();

		for (var i = 0; i < rows.Count; i++)
			if (((WeakReference)rows[i]!).IsAlive)
				held.Add(i);

		Assert.True(held.Count == 0,
			$"{entry}: {held.Count} of {rows.Count} rows are still reachable after the parse " +
			$"(rows {string.Join(", ", held)}). A value table was left uncleared, which is a mark " +
			$"the walk did not raise where it wrote.");
	}

	/// <summary>
	/// The parse, in a frame of its own: a local holding the result would keep the rows alive
	/// past the collection, and the jit is free to keep one as long as the method runs.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	static bool Read(Type host, string entry, string input)
	{
		var match = host.GetMethod(entry, [typeof(string)])!.Invoke(null, [input])!;

		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static void Collect()
	{
		for (var round = 0; round < 3; round++)
		{
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
			GC.WaitForPendingFinalizers();
		}
	}

	static (Type Host, string Source) Compile()
	{
		var compiled = GramCompiler.Compile(Grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = CarrierKind.Tape, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.DoesNotContain(compiled.Diagnostics, one => one.Severity != GramSeverity.Info);

		var source = Assert.Single(compiled.Sources).Text;

		return (EmittedCode.Compile(source, declarationMembers: Members).GetType("Grammar")!, source);
	}
}
