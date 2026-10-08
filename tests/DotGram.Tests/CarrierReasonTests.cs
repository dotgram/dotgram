using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Microsoft.CodeAnalysis;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What a grammar left to choose is told about the carrier it gets: the immediate one wherever
/// that carrier does not refuse a machine, with a warning (GRAM5016) where the tape would be
/// safer and information (GRAM5012) where the tape would only hold constructions back.
/// </summary>
/// <remarks>
/// <para>
/// The warning has two reasons. Constructions and the hooks that run during recognition — a
/// <c>when</c>, a <c>switch</c> selector, an external recognizer handed the context — that share
/// <c>context</c>, where a reading may not stand: carried immediately a construction runs as its
/// alternative is read, interleaved with those hooks, and on the tape after the parse has
/// accepted, so either can see what the other wrote at a different time, and what is accepted or
/// the value built can differ. Each fixture asserts the difference as well as the warning, so that
/// the warning's claim is held to the parsers rather than merely printed. The other is a loop that
/// rebuilds what it read each time a turn is given back, which the reader does not defer in a
/// grammar with a context or a recovery, or for a rule whose own guard asks for its value so far.
/// </para>
/// <para>
/// A carrier the author named is told none of it, and a machine the immediate carrier refuses is
/// on the tape in silence under the default; asked for by name, the refusal is a warning (GRAM5007).
/// </para>
/// </remarks>
public sealed class CarrierReasonTests
{
	// ── Constructions and hooks sharing the context ──────────────────────────

	/// <summary>
	/// A construction that reads the context before a guard that writes it: carried immediately the
	/// construction runs as <c>Value</c> is read, before the guard, and sees nothing written yet; on
	/// the tape it runs after the parse has accepted, after the guard.
	/// </summary>
	[Fact]
	public void A_construction_reading_what_a_guard_writes_later_warns_and_builds_another_value()
	{
		var host = Hosted(
			"""
			context : @Ctx
			Start : @int = v: Value & when @((context.N = 1) == 1) & '!' => @(v)
			             | v: Value => @(v)
			Value : @int = 'a'+ => @(context.N)
			parse Start
			""");

		Warned(host.Told, "Value");

		Assert.Equal("1", Answer(host.Tape, "aa!"));
		Assert.Equal("0", Answer(host.Auto, "aa!"));
	}

	/// <summary>
	/// A construction that writes the context before an external recognizer that reads it: carried
	/// immediately the recognizer sees what the construction wrote, and reads what the tape refuses.
	/// </summary>
	[Fact]
	public void A_construction_writing_what_a_recognizer_reads_warns_and_accepts_another_text()
	{
		var host = Hosted(
			"""
			context : @Ctx
			Start : @string = v: Value & t: Tail => @(t)
			Tail  : @string = @Bang & eof => @("bang") | eof => @("plain")
			Value : @int = 'a'+ => @(context.N = 1)
			parse Start
			""",
			"static bool Bang(ParserInput<char> input, ref int pos, Ctx context) " +
			"{ if (context.N != 1 || !input.Peek(pos, out var c) || c != '!') return false; pos++; return true; }");

		Warned(host.Told, "Value");

		Assert.Equal("<refused>", Answer(host.Tape, "a!"));
		Assert.Equal("bang",      Answer(host.Auto, "a!"));
		Assert.Equal("plain",     Answer(host.Tape, "a"));
		Assert.Equal("plain",     Answer(host.Auto, "a"));
	}

	/// <summary>The same with a <c>switch</c> whose selector reads the context: each carrier reads a text the other refuses.</summary>
	[Fact]
	public void A_construction_writing_what_a_selector_reads_warns_and_accepts_another_text()
	{
		var host = Hosted(
			"""
			context : @Ctx
			Start : @string = v: Value & switch @(context.N) { case 1: '!' default: '?' } & eof => @("x")
			                | v: Value & '#' => @("y")
			Value : @int = 'a'+ => @(context.N = 1)
			parse Start
			""");

		Warned(host.Told, "Value");

		Assert.Equal("<refused>", Answer(host.Tape, "a!"));
		Assert.Equal("x",         Answer(host.Auto, "a!"));
		Assert.Equal("x",         Answer(host.Tape, "a?"));
		Assert.Equal("<refused>", Answer(host.Auto, "a?"));
	}

	// ── Lists rebuilt on every turn given back ───────────────────────────────

	/// <summary>
	/// A list whose last turn is given back, so that a refused input rebuilds it once a turn: the
	/// reader defers that rebuilding in a plain grammar, and does not where the grammar has a context
	/// or a recovery, or where a rule's own guard asks for its value so far.
	/// </summary>
	const string Listed =
		"""
		trivia = none
		Ows  = [' ']*
		Item : @string = t: ['a'..'z']+ => @(t)
		Rest : @string = Ows & ',' & Ows & i: Item => @(i)
		List : @string = first: Item & rest: Rest* & Ows => @(first + string.Concat(rest))
		Pair : @string = l: List & ',' & Ows & 'z' & '!' => @(l)
		parse Pair

		""";

	const string SelfAsking =
		"""
		trivia = none
		Ows = [' ']*
		Start : @string = f: Fold & Ows & ';' => @(f)
		Fold : @string = l: Fold & Ows & ',' & Ows & when @(l.Length > 0) & d: Digit => @(l + d) | d: Digit => @(d)
		Digit : @string = t: ['0'..'9'] => @(t)
		parse Start
		""";

	[Theory]
	[InlineData("a context",  "context : @object\n" + Listed,                                                              "the grammar has a context")]
	[InlineData("a recovery", Listed + "Row : @string = 'R' & t: ['a'..'z']+ & ';' => @(t)\nRows : @string = rows: Row* recover ';' => @(\"!\") & eof => @(string.Join(\",\", rows))\nparse Rows\n", "the grammar has a recovery")]
	[InlineData("its guard",  SelfAsking,                                                                                   "a rule's own guard asks for its value so far")]
	public void A_list_rebuilt_on_every_turn_given_back_warns(string where, string grammar, string because)
	{
		var said = Assert.Single(Told(grammar), static one => one.Id == GramCompiler.CarrierCaution);

		Assert.True(said.Severity == GramSeverity.Warning, where);
		Assert.Contains("quadratic in its length", said.Message, StringComparison.Ordinal);
		Assert.Contains(because,                   said.Message, StringComparison.Ordinal);

		// A trace build decides the same of every loop, though it defers none.
		Assert.Equal(
			Told(grammar).Select(static one => one.Id + one.Message),
			Told(grammar, trace: true).Select(static one => one.Id + one.Message));
	}

	/// <summary>The plain list defers, buffered input included, and is told only what the tape would hold back.</summary>
	[Fact]
	public void A_plain_list_defers_and_is_not_warned_of()
	{
		foreach (var buffered in new[] { false, true })
		{
			var told = Told(Listed, buffered: buffered);

			Assert.DoesNotContain(told, static one => one.Id == GramCompiler.CarrierCaution);
			Assert.Contains(told, static one => one.Id == GramCompiler.CarrierChosen && one.Severity == GramSeverity.Info);
		}
	}

	// ── What a named carrier and a refused machine are told ──────────────────

	/// <summary>
	/// A grammar the generator lets interleave on purpose — a construction recording into the context
	/// what the guard after it checks, read only where the reading stands — is told nothing: the
	/// warning is for what the tape would hold back, and here it holds nothing back.
	/// </summary>
	[Fact]
	public void A_context_shared_where_every_reading_stands_is_silent()
	{
		var told = Told(
			"""
			context : @object
			Start : @string = v: Value & when @(context != null) & '!' => @(v)
			Value : @string = t: 'a' => @(context.ToString() + t)
			parse Start
			""");

		Assert.DoesNotContain(told, static one => one.Id is GramCompiler.CarrierCaution or GramCompiler.CarrierChosen);
	}

	/// <summary>An explicit carrier, either of them, is told no reason at all.</summary>
	[Fact]
	public void A_carrier_named_is_told_no_reason()
	{
		foreach (var carrier in new[] { CarrierKind.Immediate, CarrierKind.Tape })
		{
			Assert.DoesNotContain(
				Told("context : @object\n" + Listed, carrier),
				static one => one.Id is GramCompiler.CarrierCaution or GramCompiler.CarrierChosen);
			Assert.DoesNotContain(
				Told(SelfAsking, carrier),
				static one => one.Id is GramCompiler.CarrierCaution or GramCompiler.CarrierChosen);
		}
	}

	/// <summary>
	/// A machine the immediate carrier refuses, whose constructions and guard share the context and
	/// whose reading may not stand: under the default it is on the tape in silence — the reasons are
	/// for what the immediate carrier carries — and named, it is a warning naming what it refused.
	/// </summary>
	[Fact]
	public void A_refused_machine_sharing_the_context_falls_back_in_silence()
	{
		const string grammar =
			"""
			context : @object
			X : @string = t: ['a'..'z'] => @(t + context.ToString())
			S : @string = a: X* & ';' & b: X* & when @(context != null) & eof => @(string.Concat(a) + "|" + string.Concat(b))
			            | c: X* & '?' => @(string.Concat(c))
			parse S
			""";

		Assert.DoesNotContain(
			Told(grammar),
			static one => one.Id is GramCompiler.CarrierCaution or GramCompiler.CarrierChosen or GramCompiler.CarrierRefused);

		var refused = Assert.Single(Told(grammar, CarrierKind.Immediate), static one => one.Id == GramCompiler.CarrierRefused);

		Assert.Equal(GramSeverity.Warning, refused.Severity);
		Assert.Contains("ParseS [whole] is carried on the tape rather than as Immediate", refused.Message, StringComparison.Ordinal);
		Assert.Contains("'S' gathers two members onto one stack", refused.Message, StringComparison.Ordinal);
		Assert.Equal(Source(grammar, CarrierKind.Tape), Source(grammar, CarrierKind.Auto));
	}

	/// <summary>
	/// Every kind of refusal the immediate carrier has: the default falls back to the tape in
	/// silence and writes the tape's parser byte for byte; the carrier named warns, naming why.
	/// </summary>
	[Theory]
	[InlineData("an extent gathered",   "Where : @SourceSpan = ['a'..'z']\nStart : @SourceSpan[] = items: Where+ => @(items)\nparse Start\n",                                       "is an extent collected across turns")]
	[InlineData("a shared stack",       "X : @string = t: ['a'..'z'] => @(t)\nS : @string = a: X* & ';' & b: X* & eof => @(string.Concat(a) + string.Concat(b))\nparse S\n", "gathers two members onto one stack")]
	[InlineData("an unsettled recovery", "Mark : @string = t: ';' => @(\";\")\nRow : @string = 'R' & t: ['a'..'z']+ & ';' => @(t)\nStart : @string = rows: Row* recover Mark => @(\"!\") & eof => @(string.Join(\",\", rows))\nparse Start\n", "recovers")]
	public void Every_refusal_falls_back_in_silence_and_warns_where_named(string kind, string grammar, string why)
	{
		Assert.DoesNotContain(Told(grammar), static one => one.Severity != GramSeverity.Info);
		Assert.Equal(Source(grammar, CarrierKind.Tape), Source(grammar, CarrierKind.Auto));

		var refused = Assert.Single(Told(grammar, CarrierKind.Immediate), static one => one.Id == GramCompiler.CarrierRefused);

		Assert.True(refused.Severity == GramSeverity.Warning, kind);
		Assert.Contains(why, refused.Message, StringComparison.Ordinal);
	}

	/// <summary>A sequence gathered from several types, which needs the host's types to be compiled at all.</summary>
	[Fact]
	public void A_sequence_of_several_types_falls_back_in_silence_and_warns_where_named()
	{
		static string Host(string carrier)
		{
			return
				"using DotGram;\n" +
				"public abstract class Shape { }\n" +
				"public sealed class Circle : Shape { }\n" +
				"public sealed class Square : Shape { }\n" +
				"[Gram(@\"Shapes : @Shape[] = (C | S)+ & eof\nC : @Circle = 'c' => @(new Circle())\nS : @Square = 's' => @(new Square())\nparse Shapes\"" +
				carrier + ")]\n" +
				"public static partial class P { }\n";
		}

		var auto  = GeneratorDriverTests.RunGenerator(Host(""));
		var named = GeneratorDriverTests.RunGenerator(Host(", Carrier = GramCarrier.Immediate"));
		var tape  = GeneratorDriverTests.RunGenerator(Host(", Carrier = GramCarrier.Tape"));

		Assert.DoesNotContain(auto.Diagnostics, static one => one.Severity != DiagnosticSeverity.Info);
		Assert.Equal(GeneratorDriverTests.GetGeneratedSource(tape, "P.g.cs"), GeneratorDriverTests.GetGeneratedSource(auto, "P.g.cs"));

		var refused = Assert.Single(named.Diagnostics, static one => one.Id == GramCompiler.CarrierRefused);

		Assert.Equal(DiagnosticSeverity.Warning, refused.Severity);
		Assert.Contains("gathers values of several types into one sequence", refused.GetMessage(), StringComparison.Ordinal);
	}

	// ── Helpers ──────────────────────────────────────────────────────────────

	static void Warned(IReadOnlyList<Diagnostic> told, string rule)
	{
		var said = Assert.Single(told, static one => one.Id == GramCompiler.CarrierCaution);

		Assert.Equal(DiagnosticSeverity.Warning, said.Severity);
		Assert.Contains($"constructions of {rule} and its `when`/`switch`/recognizers", said.GetMessage(), StringComparison.Ordinal);
		Assert.Contains("Carrier = GramCarrier.Tape", said.GetMessage(), StringComparison.Ordinal);
		Assert.DoesNotContain(told, static one => one.Id == GramCompiler.CarrierChosen);
	}

	/// <summary>
	/// The grammar compiled twice into one host: left to choose (<c>Probe</c>), and on the tape
	/// (<c>Probe.Tape</c>), which is told nothing because it named its carrier.
	/// </summary>
	static (Type Auto, Type Tape, IReadOnlyList<Diagnostic> Told) Hosted(string grammar, string members = "")
	{
		var code =
			"using System;\n" +
			"using DotGram;\n" +
			"[Gram(@\"" + grammar.Replace("\"", "\"\"", StringComparison.Ordinal) + "\")]\n" +
			"[GramOptions(Carrier = GramCarrier.Tape, Suffix = \"Tape\")]\n" +
			"public static partial class Probe\n" +
			"{\n" +
			"\tpublic sealed class Ctx { public int N; }\n" +
			"\t" + members + "\n" +
			"}\n";

		var told  = GeneratorDriverTests.RunGenerator(code).Diagnostics;
		var probe = GeneratorDriverTests.Build(code, permittedWarning: GramCompiler.CarrierCaution).GetType("Probe")!;

		return (probe, probe.GetNestedType("Tape")!, told);
	}

	/// <summary>The value read from a fresh context, or <c>&lt;refused&gt;</c>.</summary>
	static string Answer(Type host, string input)
	{
		var context = Activator.CreateInstance((host.DeclaringType ?? host).GetNestedType("Ctx")!)!;
		var parse   = host.GetMethods().Single(static one => one.Name == "TryParseStart" &&
			one.GetParameters() is [{ ParameterType: var text }, { ParameterType.Name: "Ctx" }] && text == typeof(string));
		var match   = parse.Invoke(null, [input, context])!;

		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!
			? match.GetType().GetProperty("Value")!.GetValue(match)!.ToString()!
			: "<refused>";
	}

	static IReadOnlyList<GramDiagnostic> Told(
		string grammar, CarrierKind carrier = CarrierKind.Auto, bool trace = false, bool buffered = false)
	{
		return Compiled(grammar, carrier, trace, buffered).Diagnostics;
	}

	static string Source(string grammar, CarrierKind carrier)
	{
		return Assert.Single(Compiled(grammar, carrier).Sources).Text;
	}

	static GramCompilation Compiled(string grammar, CarrierKind carrier, bool trace = false, bool buffered = false)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier       = carrier,
			Trace         = trace,
			BufferedInput = buffered,
		});

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		return result;
	}
}
