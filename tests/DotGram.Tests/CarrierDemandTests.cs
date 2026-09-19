using System;
using System.Collections;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The immediate carrier builds what the tape builds and nothing else: a value nobody asks
/// for is read and never built (<c>Grammar/Model/Demand.cs</c>).
/// </summary>
/// <remarks>
/// <para>
/// The tape is the reference. It builds nothing while it reads, and once the parse has
/// accepted it builds what the answer is made of and nothing more; what a guard names it
/// builds while reading, since the guard needs it then. The immediate carrier builds as it
/// reads, so without being told it built everything it read, and a construction nobody
/// asked for ran — which is harmless for a pure one, visible to one with an effect, and
/// fatal to one that throws before something it needs exists (the expression language's
/// untyped lambdas, <c>ExpressionCarrierTests</c>).
/// </para>
/// <para>
/// Every factory here logs its name, so what the two carriers built is compared, and not
/// only what they answered. The two build in different orders — one as it reads, one after
/// — so the logs are compared as counts per factory.
/// </para>
/// </remarks>
public sealed class CarrierDemandTests
{
	const string Members = """
		public static readonly System.Collections.Generic.List<string> Built =
			new System.Collections.Generic.List<string>();
		static string Log(string name, string value)
		{
			Built.Add(name);
			return value;
		}
		""";

	/// <summary>
	/// <c>Held</c> reads <c>Inner</c> and keeps its span, not its value, so neither
	/// <c>Inner</c> nor anything under it is built. <c>Inner</c>'s guard names <c>x</c>, so
	/// <c>x</c> is built while reading even there; <c>Kept</c> is captured and built as usual.
	/// </summary>
	const string Grammar = """
		Start : @string = h: Held & ';' & k: Kept => @(Log("Start", h + k))
		Held : @string = Inner => @(Log("Held", "held"))
		Inner : @string = x: Leaf & ',' & y: Other & when @(x.Length > 0) => @(Log("Inner", x + y))
		Kept : @string = l: Leaf => @(Log("Kept", l))
		Leaf : @string = t: ['a'..'z']+ => @(Log("Leaf", t.ToString()))
		Other : @string = t: ['0'..'9']+ => @(Log("Other", t.ToString()))
		parse Start
		""";

	[Theory]
	[InlineData("ab,12;cd")]
	[InlineData("a,1;z")]
	[InlineData("ab,12;")]
	[InlineData("ab;cd")]
	public void The_immediate_carrier_builds_what_the_tape_builds(string input)
	{
		var (tape, _)          = Run(CarrierKind.Tape, input);
		var (immediate, value) = Run(CarrierKind.Immediate, input);

		Assert.Equal(tape.Answer, immediate.Answer);

		// A refused parse may already have run what it read: that is what the immediate
		// carrier gives up (§3.7), and not what demand is about.
		if (value is null)
			return;

		Assert.Equal(Counted(tape.Built), Counted(immediate.Built));

		// What the answer is made of, and the one value a guard asked for inside what
		// nobody built.
		Assert.DoesNotContain("Inner", immediate.Built);
		Assert.DoesNotContain("Other", immediate.Built);
	}

	[Fact]
	public void A_value_nobody_asks_for_is_not_built()
	{
		var (immediate, value) = Run(CarrierKind.Immediate, "ab,12;cd");

		Assert.Equal("heldcd", value);
		Assert.Equal(["Held", "Kept", "Leaf", "Leaf", "Start"], immediate.Built.OrderBy(static one => one, StringComparer.Ordinal));
	}

	/// <summary>
	/// What a reading that builds nothing gathered is let go of all the same: <c>Inner</c>
	/// gathers its letters on the stack its type shares with <c>Start</c>'s items, while
	/// <c>Start</c> is gathering, and a stack nobody collected would hand them to
	/// <c>Start</c> as items of its own.
	/// </summary>
	const string Gathering = """
		Start : @string = ks: Item+ => @(Log("Start", string.Join(",", ks)))
		Item : @string = '[' & Inner & ']' => @(Log("Item", "held")) | t: Leaf => @(Log("Item", t))
		Inner : @string = ls: Leaf+ => @(Log("Inner", string.Join("", ls)))
		Leaf : @string = t: ['a'..'z'] => @(Log("Leaf", t.ToString()))
		parse Start
		""";

	[Theory]
	[InlineData("a[bc]d", "a,held,d")]
	[InlineData("[xyz][q]", "held,held")]
	public void What_is_gathered_where_nothing_is_built_is_not_handed_on(string input, string expected)
	{
		var (tape, _)          = Run(CarrierKind.Tape, input, Gathering);
		var (immediate, value) = Run(CarrierKind.Immediate, input, Gathering);

		Assert.Equal(expected, value);
		Assert.Equal(tape.Answer, immediate.Answer);
		Assert.Equal(Counted(tape.Built), Counted(immediate.Built));
	}

	/// <summary>
	/// What a recovery synchronizes on is read to find where to go on, and thrown away: its
	/// text is the bad element's, and nothing it builds is anyone's. <c>Mark</c> builds, and is
	/// read only there; whichever carrier Auto chooses must never run its factory, as the tape
	/// never does. What holds it today is the immediate carrier's gate for a recovering machine
	/// (Machine.RecoveryRefusal): Commit counts what a synchronization reads as thrown away, so
	/// <c>Mark</c>'s construction has no point and the carrier refuses (GRAM5007).
	/// </summary>
	const string Synchronized = """
		Mark : @string = t: ';' => @(Log("Mark", ";"))
		Row : @string = 'R' & t: ['a'..'z']+ & ';' => @(Log("Row", t.ToString()))
		Start : @string = rows: Row* recover Mark => @(Log("Bad", "!")) & eof => @(Log("Start", string.Join(",", rows)))
		parse Start
		""";

	[Theory]
	[InlineData("Ra;X;Rb;")]
	[InlineData("Ra;Rb;")]
	[InlineData("X;Y;")]
	public void What_a_recovery_synchronizes_on_is_not_built(string input)
	{
		var (tape, _) = Run(CarrierKind.Tape, input, Synchronized);
		var (auto, _) = Run(CarrierKind.Auto, input, Synchronized);

		Assert.Equal(tape.Answer, auto.Answer);
		Assert.DoesNotContain("Mark", auto.Built);
		Assert.Equal(Counted(tape.Built), Counted(auto.Built));
	}

	static (Outcome Outcome, string? Value) Run(CarrierKind carrier, string input, string grammar = Grammar)
	{
		var compiled = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = carrier, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.DoesNotContain(compiled.Diagnostics, one => one.Severity == GramSeverity.Error);

		var source = Assert.Single(compiled.Sources).Text;

		// The immediate reading is the one under test, and a grammar it could not carry would
		// be read on the tape and pass by testing nothing.
		if (carrier == CarrierKind.Immediate)
			Assert.Contains("unbuilt", source, StringComparison.Ordinal);

		var host  = EmittedCode.Compile(source, declarationMembers: Members).GetType("Grammar")!;
		var built = (IList)host.GetField("Built", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;

		built.Clear();

		var match = host.GetMethod("TryParseStart", [typeof(string)])!.Invoke(null, [input])!;
		var ok    = (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
		var value = ok ? (string?)match.GetType().GetProperty("Value")!.GetValue(match) : null;

		return (new Outcome(ok ? value : "<refused>", built.Cast<string>().ToArray()), value);
	}

	static string Counted(string[] built) =>
		string.Join(", ", built.GroupBy(static one => one).OrderBy(static one => one.Key, StringComparer.Ordinal)
			.Select(static one => one.Key + "×" + one.Count()));

	sealed record Outcome(string? Answer, string[] Built);
}
