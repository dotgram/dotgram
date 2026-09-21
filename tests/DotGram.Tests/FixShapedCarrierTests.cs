using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The gate's claim, held by counting on the shape it was won on: nothing a machine builds
/// is read for a derivation that is then given up, so the carrier that builds where it reads
/// runs the same factories as the one that waits for a walk at the end.
/// </summary>
/// <remarks>
/// <para>
/// The shape is FIX's, small enough to read whole: a stream of fields recovering at a
/// separator, a value read until that separator (<c>(?!Sep &amp; any)+</c>, the form
/// <c>Determinism.NeverGivesBack</c> answers with a continuation's lead), a field whose arm
/// the host chooses from the tag, and a length that decides whether the data after it is
/// accepted — a guard carrying state from one field into the next, which is what a
/// length-and-data pair is once the host's own read of the payload is left out. That read
/// happens while matching, under either carrier, so it decides nothing here.
/// </para>
/// <para>
/// It is a stream over a buffer because that is what makes a recovering machine reach the
/// methods and so have a carrier to choose at all (<c>Machine.CanDirect</c>): compiled as a
/// plain parse, the engine reads it and there is nothing to ask.
/// </para>
/// <para>
/// This is the standing form of what the FIX package's move to the immediate carrier was held
/// to. It lives here rather than beside FIX because the Finance tests carry no reference to
/// the generator, deliberately, and compiling a grammar in memory there would cost them the
/// seconds D12 bought. If it ever disagrees with what the measuring stand reads off FIX
/// itself — allocation per field, where a construction shows as bytes — the answer is to
/// count inside FIX and not to trust this.
/// </para>
/// </remarks>
public sealed class FixShapedCarrierTests
{
	const string Grammar = """
		Sep = ';'

		Number : @string = t: ['0'..'9']+ => @(t.ToString())
		Text   : @string = t: (?!Sep & any)+ => @(t.ToString())

		Field : @string =
			tag: Number & '=' & switch @(Kind(tag)) {
				case 0: v: Text
				case 1: size: Number & Sep & data: Number & '=' & body: Text & when @(Fits(size, body))
			} & (Sep | eof)
			=> @(Log("field", tag + "=" + (v ?? body ?? "")))

		Fields : @string[] = Field* recover Sep => @(Log("bad", parserText))

		parse Fields as ParseFields stream
		""";

	const string Members = """
		public static readonly System.Collections.Generic.List<string> Built =
			new System.Collections.Generic.List<string>();
		static string Log(string name, string value)
		{
			Built.Add(name);
			return value;
		}
		// Tag 9 carries a length, as FIX's data tags do; everything else is text.
		static int Kind(string tag) => tag == "9" ? 1 : 0;
		static bool Fits(string size, string body) => int.TryParse(size, out var n) && body.Length == n;
		""";

	[Theory]
	// Fields that stand.
	[InlineData("1=abc;2=de;")]
	[InlineData("1=abc;2=de")]
	// One that cannot be read, recovered in the middle and at the front.
	[InlineData("1=abc;=;2=de;")]
	[InlineData("=;1=abc;")]
	// A length and the data it measures, accepted and refused.
	[InlineData("9=3;7=abc;1=de;")]
	[InlineData("9=4;7=abc;1=de;")]
	// Nothing, and nothing but separators.
	[InlineData("")]
	[InlineData(";;;")]
	public void The_two_carriers_build_the_same(string wire)
	{
		var tape      = Read(CarrierKind.Tape, wire);
		var immediate = Read(CarrierKind.Immediate, wire);

		Assert.Equal(tape.Fields, immediate.Fields);
		Assert.Equal(Counted(tape.Built), Counted(immediate.Built));
	}

	static (string Fields, string[] Built) Read(CarrierKind carrier, string wire)
	{
		var compiled = GramCompiler.Compile(Grammar, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			Carrier       = carrier,
			CSharpScanner = RoslynCSharpScanner.Instance,
			BufferedInput = true,
		});

		Assert.DoesNotContain(compiled.Diagnostics, one => one.Severity == GramSeverity.Error);

		var source = Assert.Single(compiled.Sources).Text;

		// The carrier asked for has to be the one that was given, or this compares the tape
		// with itself and holds nothing.
		Assert.Equal(
			carrier == CarrierKind.Tape,
			source.Contains("Materialize_DotGram", StringComparison.Ordinal));

		var host  = EmittedCode.Compile(source, declarationMembers: Members).GetType("Grammar")!;
		var built = (IList)host.GetField("Built", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;

		built.Clear();

		var read   = host.GetMethods().Single(one =>
			one.Name == "ParseFields" && one.GetParameters()[0].ParameterType == typeof(string));
		var fields = ((IEnumerable)read.Invoke(null, [wire])!).Cast<object?>().Select(one => one?.ToString() ?? "<null>");

		return (string.Join(" | ", fields), built.Cast<string>().ToArray());
	}

	static string Counted(string[] built)
	{
		return string.Join(", ", built.GroupBy(static one => one).OrderBy(static one => one.Key, StringComparer.Ordinal)
			.Select(static one => one.Key + "×" + one.Count()));
	}
}
