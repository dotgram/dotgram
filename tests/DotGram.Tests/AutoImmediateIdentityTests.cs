using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A grammar left to choose is compiled exactly as one that asks for the immediate carrier: the
/// same sources, name for name and character for character, with the carriers report off.
/// </summary>
/// <remarks>
/// <para>
/// The emitter takes what was asked for and, where it is <see cref="CarrierKind.Auto"/>, writes as
/// for <see cref="CarrierKind.Immediate"/> before anything reads it; what was asked is read by the
/// diagnostics and the carriers report alone. This holds that to the output over every carrier
/// shape, every snapshot grammar, the SQL grammars as they ship, and two grammars where the
/// generator once wrote something else for a grammar left to choose: two large publications that
/// share their rules, one of which the tape would hold back, which are one machine; and a machine
/// the carrier refuses beside one it carries, where how the tape stores its values once depended on
/// the carried machine being counted as a tape.
/// </para>
/// <para>
/// Diagnostics are not compared: a grammar left to choose is told what the tape would hold back,
/// and one that named its carrier is not.
/// </para>
/// </remarks>
public sealed class AutoImmediateIdentityTests
{
	public static TheoryData<string> Shapes()
	{
		var data = new TheoryData<string>();

		foreach (var (name, _, _) in CarrierShapes.All)
			data.Add(name);

		return data;
	}

	[Theory]
	[MemberData(nameof(Shapes))]
	public void A_carrier_shape_is_compiled_as_under_Immediate(string name)
	{
		var grammar = CarrierShapes.All.Single(one => one.Name == name).Grammar;

		Same(grammar, Options);
	}

	public static TheoryData<string> Snapshots => SnapshotTests.Snapshots;

	[Theory]
	[MemberData(nameof(Snapshots))]
	public void A_snapshot_grammar_is_compiled_as_under_Immediate(string name)
	{
		var text = File.ReadAllText(Path.Combine(SnapshotTests.Directory, name + ".gram"));
		var auto = SnapshotTests.Renderings(name, text).ToList();
		var told = SnapshotTests.Renderings(name, text).ToList();

		for (var at = 0; at < auto.Count; at++)
		{
			// A rendering that names its carrier is the author's and has nothing to compare.
			if (auto[at].Options.Carrier != CarrierKind.Auto)
				continue;

			told[at].Options.Carrier = CarrierKind.Immediate;

			Equal(GramCompiler.Compile(text, auto[at].Options), GramCompiler.Compile(text, told[at].Options));
		}
	}

	[Theory]
	[InlineData("Standard", "SqlStandard.gram")]
	[InlineData("Standard", "SqlStandard92.gram")]
	public void A_shipped_grammar_is_compiled_as_under_Immediate(string directory, string file)
	{
		var text = File.ReadAllText(Path.Combine(Here(), "..", "..", "src", "DotGram.Sql", directory, file));

		Same(text, () => new GramCompilerOptions
		{
			ClassName     = "Parser",
			Namespace     = "Shipped",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Lexical       = true,
		});
	}

	/// <summary>
	/// Two publications of over a hundred and twenty-eight rules sharing nearly all of them, one of
	/// which builds a value read where the reading may not stand: one machine, carried immediately,
	/// as under the immediate carrier named. Left to choose, they were two, the second on the tape.
	/// </summary>
	[Fact]
	public void Two_large_publications_are_one_machine_as_under_Immediate()
	{
		var source = Same(SiblingPublicationTests.TapedBeside, Options);

		Assert.Single(SiblingPublicationTests.Readers(source));
		Assert.Empty(SiblingPublicationTests.Materializers(source));
	}

	/// <summary>
	/// A machine the immediate carrier refuses (one construction takes two members gathered onto
	/// one stack) beside one it carries that builds values of ten types — recursive, so that it is
	/// read by methods and not compiled flat — both read by methods: the
	/// tape's store is planned as under the immediate carrier named, where the carried machine is
	/// not a tape.
	/// </summary>
	[Fact]
	public void A_refused_machine_beside_a_carried_one_stores_as_under_Immediate()
	{
		const string grammar =
			"""
			X : @string = t: ['a'..'z'] => @(t)
			S : @string = a: X* & ';' & b: X* & eof => @(string.Concat(a) + "|" + string.Concat(b))
			A : @int = 'a' => @(1)
			B : @long = 'b' => @(2L)
			C : @short = 'c' => @((short)3)
			D : @byte = 'd' => @((byte)4)
			E : @double = 'e' => @(5.0)
			F : @float = 'f' => @(6f)
			G : @char = 'g' => @('g')
			H : @bool = 'h' => @(true)
			I : @decimal = 'i' => @(9m)
			Many : @string = a: A & b: B & c: C & d: D & e: E & f: F & g: G & h: H & i: I => @(string.Concat(a, b, c, d, e, f, g, h, i))
			               | '(' & m: Many & ')' => @(m)
			parse S
			parse Many
			""";

		var source = Same(grammar, Options);

		Assert.Contains("ImmediateValues", source, StringComparison.Ordinal);
		Assert.Equal(2, SiblingPublicationTests.Readers(source).Count);
		Assert.NotEmpty(SiblingPublicationTests.Materializers(source));
	}

	static GramCompilerOptions Options()
	{
		return new()
		{
			ClassName     = "Probe",
			Namespace     = "Carried",
			CSharpScanner = RoslynCSharpScanner.Instance,
		};
	}

	/// <summary>The grammar under both requests, held equal; the first source, for what else a test asks of it.</summary>
	static string Same(string grammar, Func<GramCompilerOptions> options)
	{
		var auto = options();
		var told = options();

		auto.Carrier = CarrierKind.Auto;
		told.Carrier = CarrierKind.Immediate;

		var left = GramCompiler.Compile(grammar, auto);

		Equal(left, GramCompiler.Compile(grammar, told));

		return left.Sources[0].Text;
	}

	static void Equal(GramCompilation auto, GramCompilation told)
	{
		Assert.DoesNotContain(auto.Diagnostics, static one => one.Severity == GramSeverity.Error);
		Assert.NotEmpty(auto.Sources);
		Assert.Equal(told.Sources.Select(static one => one.HintName), auto.Sources.Select(static one => one.HintName));

		for (var at = 0; at < auto.Sources.Count; at++)
			Assert.True(
				auto.Sources[at].Text == told.Sources[at].Text,
				$"{auto.Sources[at].HintName} differs between Auto and Immediate.");
	}

	static string Here([CallerFilePath] string path = "")
	{
		return Path.GetDirectoryName(path)!;
	}
}
