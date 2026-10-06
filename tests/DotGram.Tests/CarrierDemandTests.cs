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
	/// A refused input is read twice by the match form — quietly, then again for what the
	/// refusal says — and the second reading asks nothing of the value, so it builds only what
	/// a guard asks for (<c>Leaf</c>, as <c>x</c>). The first reading built the prefix the input
	/// was accepted up to, once; until the second reading was told so, it built it again.
	/// </summary>
	[Theory]
	[InlineData("ab,12;", "Held×1, Leaf×2")]
	[InlineData("ab,12;cd!", "Held×1, Kept×1, Leaf×3, Start×1")]
	[InlineData("ab;", "Leaf×2")]
	public void A_refused_input_is_read_again_without_building(string input, string built)
	{
		var (tape, _)      = Run(CarrierKind.Tape, input);
		var (immediate, _) = Run(CarrierKind.Immediate, input);

		Assert.StartsWith("<refused", immediate.Answer, StringComparison.Ordinal);
		Assert.Equal(tape.Answer, immediate.Answer);
		Assert.Equal(built, Counted(immediate.Built));
	}

	/// <summary>
	/// A left-recursive rule whose step guard reads the value so far asks for its own value while
	/// it is read, so the second reading of a refused input builds it as the first did: a reading
	/// that did not would hand the guard nothing. The two carriers refuse alike, and the value so
	/// far is built once a step on each reading.
	/// </summary>
	const string SelfAsking = """
		Start : @string = f: Fold => @(Log("Start", f))
		Fold : @string = l: Fold & ',' & when @(l.Length > 0) & d: Digit => @(Log("Fold", l + d)) | d: Digit => @(Log("Fold", d))
		Digit : @string = t: ['0'..'9'] => @(Log("Digit", t.ToString()))
		parse Start
		""";

	[Theory]
	[InlineData("1,2,", "Digit×4, Fold×4, Start×1")]
	[InlineData("1,2,3;", "Digit×6, Fold×6, Start×1")]
	public void A_rule_whose_guard_reads_its_value_so_far_builds_on_every_reading(string input, string built)
	{
		var (tape, _)      = Run(CarrierKind.Tape, input, SelfAsking);
		var (immediate, _) = Run(CarrierKind.Immediate, input, SelfAsking);

		Assert.StartsWith("<refused", immediate.Answer, StringComparison.Ordinal);
		Assert.Equal(tape.Answer, immediate.Answer);
		Assert.Equal(built, Counted(immediate.Built));
	}

	/// <summary>
	/// The form that answers only yes or no reads once and builds as it reads, so a refusal
	/// through it has built the accepted prefix once: the two readings of the match form built
	/// it once more only where a guard asked.
	/// </summary>
	[Fact]
	public void The_bool_form_reads_a_refused_input_once()
	{
		var (immediate, _) = Run(CarrierKind.Immediate, "ab,12;cd!", form: "bool");

		Assert.Equal("<refused>", immediate.Answer);
		Assert.Equal("Held×1, Kept×1, Leaf×2, Start×1", Counted(immediate.Built));
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

	/// <param name="form">
	/// Which published form reads the input: <c>match</c>, the <c>Match</c>-returning form, which
	/// reads a refused input twice; or <c>bool</c>, the form that answers yes or no and reads once.
	/// </param>
	static (Outcome Outcome, string? Value) Run(CarrierKind carrier, string input, string grammar = Grammar, string form = "match")
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

		if (form == "bool")
		{
			var arguments = new object?[] { input, null };
			var answered  = (bool)host.GetMethod("TryParseStart", [typeof(string), typeof(string).MakeByRefType()])!.Invoke(null, arguments)!;
			var answer    = answered ? (string?)arguments[1] : null;

			return (new Outcome(answered ? answer : "<refused>", built.Cast<string>().ToArray()), answer);
		}

		var match = host.GetMethod("TryParseStart", [typeof(string)])!.Invoke(null, [input])!;
		var ok    = (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
		var value = ok ? (string?)match.GetType().GetProperty("Value")!.GetValue(match) : null;

		// A refusal is where it refused and what it says, so that the two carriers are held to
		// the same message and not only to the same no.
		var refused = ok
			? null
			: $"<refused at {match.GetType().GetProperty("Position")!.GetValue(match)}: {match.GetType().GetProperty("Error")!.GetValue(match)}>";

		return (new Outcome(ok ? value : refused, built.Cast<string>().ToArray()), value);
	}

	static string Counted(string[] built)
	{
		return string.Join(", ", built.GroupBy(static one => one).OrderBy(static one => one.Key, StringComparer.Ordinal)
			.Select(static one => one.Key + "×" + one.Count()));
	}

	sealed record Outcome(string? Answer, string[] Built);
}
