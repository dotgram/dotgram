using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every carrier a grammar can be compiled with answers the same on every input — the same
/// verdict, and the same value where there is one.
/// </summary>
/// <remarks>
/// <para>
/// The carriers differ in when the author's constructions run and where the pieces of a
/// value wait; they may not differ in what comes out. This is the agreement test the
/// hand-written readings of deferred construction once ran on themselves,
/// asked of the generator: the tape is the reference, because it is the one that keeps
/// §7.3, and everything else has to agree with it or it does not ship.
/// </para>
/// <para>
/// The grammars are the shapes the second carrier had to learn one at a time: a fold, a
/// cycle, members gathered across turns as records and as text, a guard that reads a
/// captured value, and a member that may be missing.
/// </para>
/// </remarks>
public sealed class CarrierTests
{
	static readonly (string Name, string Grammar, string[] Inputs)[] Shapes = CarrierShapes.All;

	public static IEnumerable<object[]> Every()
	{
		return Shapes.Select(one => new object[] { one.Name });
	}

	/// <summary>The immediate carrier agrees with the tape on every shape and every input.</summary>
	[Theory]
	[MemberData(nameof(Every))]
	public void Immediate_agrees_with_the_tape(string name)
	{
		var (_, grammar, inputs) = Shapes.Single(one => one.Name == name);

		var tape  = Compiled(grammar, CarrierKind.Tape);
		var immediate = Compiled(grammar, CarrierKind.Immediate);

		Assert.Contains("ImmediateValues", immediate.Source, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram", immediate.Source, StringComparison.Ordinal);
		Assert.DoesNotContain("DirectValues", immediate.Source, StringComparison.Ordinal);

		foreach (var input in inputs)
		{
			var expected = EmittedCode.Match(tape.Assembly,  "Carried.Probe", "TryParseStart", input);
			var actual   = EmittedCode.Match(immediate.Assembly, "Carried.Probe", "TryParseStart", input);

			Assert.True(
				expected.IsSuccess == actual.IsSuccess,
				$"{name} on \"{input}\": the tape says {expected.IsSuccess}, immediate says {actual.IsSuccess}.");

			if (expected.IsSuccess)
				Assert.Equal(ValueOf(expected), ValueOf(actual));
		}
	}

	/// <summary>
	/// A carrier that cannot carry a grammar is not an error: the tape carries it instead,
	/// and the machine keeps the reason for whoever asks.
	/// </summary>
	/// <remarks>
	/// The reader has to accept the grammar for the answer to be the carrier's own, so the
	/// obstacle is one only the immediate carrier has: an extent collected across the turns of
	/// a repetition, for which it would need a stack of spans and has none.
	/// </remarks>
	[Fact]
	public void A_carrier_that_refuses_a_grammar_leaves_it_to_the_tape()
	{
		const string Extents =
			"Where : @SourceSpan = ['a'..'z']\n" +
			"Start : @SourceSpan[] = items: Where+ => @(items)\n" +
			"parse Start\n";

		var (source, _) = Compiled(Extents, CarrierKind.Immediate);

		Assert.Contains("DirectValues",         source, StringComparison.Ordinal);
		Assert.Contains("Materialize_DotGram",  source, StringComparison.Ordinal);
		Assert.DoesNotContain("ImmediateValues", source, StringComparison.Ordinal);
	}

	// ── What is said about a carrier, and what is offered ────────────────────────

	/// <summary>A carrier asked for and not used says so, refused or not.</summary>
	/// <remarks>
	/// It used to say so only where a carrier refused the grammar. A carrier is what a
	/// reader holds, and over a machine that has no reader it is not refused but never
	/// asked — the file that comes out is the same file byte for byte, and nothing said
	/// anything at all. An author changing the attribute and measuring would have measured
	/// the tape twice.
	/// </remarks>
	[Fact]
	public void A_carrier_that_carried_nothing_says_so()
	{
		// Read a window at a time, which the engine does and a reader does not.
		var told = Assert.Single(Diagnostics(
			"""
			Name : @string = t: ['a'..'z']+ => @(t)
			find Name as AllNames
			""",
			CarrierKind.Immediate));

		Assert.Equal(GramCompiler.CarrierRefused, told.Id);
		Assert.Equal(GramSeverity.Info,           told.Severity);
		Assert.Contains("window",                 told.Message, StringComparison.Ordinal);
	}

	/// <summary>And the tape is not offered away where the offer would be refused.</summary>
	/// <remarks>
	/// One compiler saying "take this carrier" and then "you cannot have it" about one
	/// grammar is advice contradicted by a refusal, which is worse than either alone.
	/// </remarks>
	[Fact]
	public void And_is_not_offered_where_it_would_be_refused()
	{
		const string Recovering =
			"""
			Row   : @string   = t: ['a'..'z']* & eol => @(t)
			Sheet : @string[] = rows: Row* recover eol => @(parserText)
			                  => @(rows)
			parse Sheet
			""";

		// Left to the generator, nothing offers the carrier.
		Assert.DoesNotContain(
			Diagnostics(Recovering, CarrierKind.Auto),
			one => one.Id == GramCompiler.CarrierChosen);

		// And asking for it anyway says so, naming what stands in the way. Recovery keeps
		// this grammar off the reader altogether, so that is the answer rather than the
		// carrier's own refusal for the same reason — one obstacle, said once.
		var told = Assert.Single(Diagnostics(Recovering, CarrierKind.Immediate));

		Assert.Equal(GramCompiler.CarrierRefused, told.Id);
		Assert.Contains("recover",                told.Message, StringComparison.Ordinal);
	}

	/// <summary>The carrier the generator chooses agrees with the tape on every shape and every input.</summary>
	/// <remarks>
	/// Whichever it chose: a machine choosing reads once on the tape to learn which rules open a
	/// way back, and is written again for what it chose, so this is also what holds that first
	/// reading to leaving nothing behind.
	/// </remarks>
	[Theory]
	[MemberData(nameof(Every))]
	public void Auto_agrees_with_the_tape(string name)
	{
		var (_, grammar, inputs) = Shapes.Single(one => one.Name == name);

		var tape = Compiled(grammar, CarrierKind.Tape);
		var auto = Compiled(grammar, CarrierKind.Auto);

		foreach (var input in inputs)
		{
			var expected = EmittedCode.Match(tape.Assembly, "Carried.Probe", "TryParseStart", input);
			var actual   = EmittedCode.Match(auto.Assembly, "Carried.Probe", "TryParseStart", input);

			Assert.True(
				expected.IsSuccess == actual.IsSuccess,
				$"{name} on \"{input}\": the tape says {expected.IsSuccess}, auto says {actual.IsSuccess}.");

			if (expected.IsSuccess)
				Assert.Equal(ValueOf(expected), ValueOf(actual));
		}
	}

	/// <summary>Left to the generator, a grammar that gives up no reading is carried immediately, and told so.</summary>
	/// <remarks>
	/// Told, because what it gives up is a parse that fails having run the constructions of what
	/// it read, and the author whose constructions mind needs the word that takes it back. An
	/// author who chose either carrier is told nothing.
	/// </remarks>
	[Fact]
	public void Left_to_the_generator_a_grammar_that_gives_up_nothing_is_carried_immediately()
	{
		const string Plain =
			"""
			Start : @string = first: Name & (',' & rest: Name)*
			                => @(first + "|" + string.Join("|", rest))
			Name  : @string = t: ['a'..'z']+ => @(t)
			parse Start
			""";

		var (source, _) = Compiled(Plain, CarrierKind.Auto);

		Assert.Contains("ImmediateValues",           source, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram", source, StringComparison.Ordinal);

		var told = Assert.Single(Diagnostics(Plain, CarrierKind.Auto));

		Assert.Equal(GramCompiler.CarrierChosen, told.Id);
		Assert.Equal(GramSeverity.Info,          told.Severity);
		Assert.Contains("as Immediate",          told.Message, StringComparison.Ordinal);
		Assert.Contains("GramCarrier.Tape",      told.Message, StringComparison.Ordinal);

		Assert.Empty(Diagnostics(Plain, CarrierKind.Tape));
		Assert.Empty(Diagnostics(Plain, CarrierKind.Immediate));
	}

	/// <summary>
	/// A look whose body the machine can read silently builds nothing for the reading it
	/// throws away, so it keeps nothing on the tape.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>?=Name &amp; n: Name</c> reads <c>Name</c> twice and keeps the second reading only.
	/// What decides the carrier is not that a reading is given up but that something was
	/// <em>built</em> for it: where the look's body is read silently — nothing captured,
	/// nothing constructed, the machine's <c>unbuilt</c> standing over it — §7.3 is kept
	/// without the tape, and the constructions run exactly where the tape would have run them.
	/// </para>
	/// <para>
	/// Held by counting, not by reading the code: the factories the two carriers run, for an
	/// input that stands and for one that is refused, have to be the same list.
	/// </para>
	/// </remarks>
	[Fact]
	public void A_look_the_machine_can_read_silently_builds_nothing()
	{
		const string Ahead =
			"""
			Start : @string = ?=Name & n: Name => @(Log("Start", n))
			Name  : @string = t: ['a'..'z']+ => @(Log("Name", t.ToString()))
			parse Start
			""";

		const string Members = """
			public static readonly System.Collections.Generic.List<string> Built =
				new System.Collections.Generic.List<string>();
			static string Log(string name, string value)
			{
				Built.Add(name);
				return value;
			}
			""";

		var source = Assert.Single(GramCompiler.Compile(Ahead, new GramCompilerOptions
		{
			ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Carrier = CarrierKind.Auto,
		}).Sources).Text;

		Assert.Contains("ImmediateValues",           source, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram", source, StringComparison.Ordinal);

		foreach (var input in new[] { "abc", "", "1" })
			Assert.Equal(Built(CarrierKind.Tape, input), Built(CarrierKind.Auto, input));

		// A look reads Name and gives it back; the rule is built once, for the reading that stands.
		Assert.Equal(["Name", "Start"], Built(CarrierKind.Auto, "abc"));
		Assert.Empty(Built(CarrierKind.Auto, "1"));

		string[] Built(CarrierKind carrier, string input)
		{
			var result = GramCompiler.Compile(Ahead, new GramCompilerOptions
			{
				ClassName     = "Grammar",
				CSharpScanner = RoslynCSharpScanner.Instance,
				Carrier       = carrier,
			});

			var host = EmittedCode.Compile(Assert.Single(result.Sources).Text, declarationMembers: Members)
				.GetType("Grammar")!;
			var told = (IList<string>)host.GetField("Built", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;

			told.Clear();

			host.GetMethod("TryParseStart", [typeof(string)])!.Invoke(null, [input]);

			return [.. told];
		}
	}

	/// <summary>And so does a rule read again after it answered, which the graph alone does not show.</summary>
	/// <remarks>
	/// The repetition takes every letter and gives the last one back for the <c>'a'</c> after
	/// it. Every reading of <c>Letter</c> is on the derivation that stands or on a parse that
	/// fails, as far as the graph can say; it is the reader that opens the way back.
	/// </remarks>
	[Fact]
	public void Left_to_the_generator_a_turn_given_back_keeps_the_tape()
	{
		const string GivesBack =
			"""
			Start  : @string = parts: Letter* & 'a' => @(string.Concat(parts))
			Letter : @string = t: ['a'..'z'] => @(t)
			parse Start
			""";

		var (source, assembly) = Compiled(GivesBack, CarrierKind.Auto);
		var tape = Compiled(GivesBack, CarrierKind.Tape);

		Assert.Contains("Materialize_DotGram", source, StringComparison.Ordinal);

		foreach (var input in new[] { "bca", "a", "ba", "b", "" })
		{
			var expected = EmittedCode.Match(tape.Assembly, "Carried.Probe", "TryParseStart", input);
			var actual   = EmittedCode.Match(assembly,      "Carried.Probe", "TryParseStart", input);

			Assert.Equal(expected.IsSuccess, actual.IsSuccess);
			Assert.Equal(ValueOf(expected),  ValueOf(actual));
		}

		var told = Assert.Single(Diagnostics(GivesBack, CarrierKind.Auto), static one => one.Id == GramCompiler.CarrierChosen);

		Assert.Contains("read again", told.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// A choice of texts where the shorter one could only be wanted by what cannot follow opens no
	/// way back, on the tape as in the engine: <c>eol</c> is <c>"\r\n" | '\n' | '\r'</c>, and giving
	/// <c>"\r\n"</c> back for <c>'\r'</c> is worth something only where a <c>'\n'</c> may come next.
	/// </summary>
	[Theory]
	[InlineData("(s: Line & eol)*", true)]
	[InlineData("(s: Line & eol)* & \"\\nz\"", false)]
	public void A_run_of_texts_nothing_can_come_back_for_opens_no_way(string file, bool immediate)
	{
		var grammar =
			$$"""
			using Std;
			File : @string[] = {{file}} => @(s)
			Line : @string = t: ['a'..'z']+ => @(t)
			parse File
			""";

		var told = Assert.Single(Diagnostics(grammar, CarrierKind.Auto), static one => one.Id == GramCompiler.CarrierChosen);

		Assert.Contains(immediate ? "as Immediate" : "read again", told.Message, StringComparison.Ordinal);

		if (!immediate)
			Assert.Contains("eol", told.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// A rule called only inside an atomic group is never asked for another answer, whatever ways
	/// it opens inside: the group seals them once it has answered. Beside it, the same rule called
	/// openly, which a failure after it comes back into.
	/// </summary>
	[Theory]
	[InlineData("t: {Word}", true)]
	[InlineData("t: Word", false)]
	public void A_rule_only_called_inside_an_atomic_group_is_not_read_again(string line, bool immediate)
	{
		var grammar =
			$$"""
			Start : @string[] = (s: Line & ';')* => @(s)
			Line  : @string = {{line}} => @(t)
			Word  = ['a'..'z']+ & (Two | One)
			Two   = ['0'..'9'] & ['0'..'9']
			One   = ['0'..'9']
			parse Start
			""";

		var told = Assert.Single(Diagnostics(grammar, CarrierKind.Auto), static one => one.Id == GramCompiler.CarrierChosen);

		Assert.Contains(immediate ? "as Immediate" : "read again", told.Message, StringComparison.Ordinal);

		if (!immediate)
			Assert.Contains("Word", told.Message, StringComparison.Ordinal);

		// And what it chose reads what the tape reads, the ways inside the group included.
		var tape = Compiled(grammar, CarrierKind.Tape);
		var auto = Compiled(grammar, CarrierKind.Auto);

		foreach (var input in new[] { "ab12;cd3;", "a1;", "ab123;", "x;", "" })
		{
			var expected = EmittedCode.Match(tape.Assembly, "Carried.Probe", "TryParseStart", input);
			var actual   = EmittedCode.Match(auto.Assembly, "Carried.Probe", "TryParseStart", input);

			Assert.Equal(expected.IsSuccess, actual.IsSuccess);

			if (expected.IsSuccess)
				Assert.Equal((string[])expected.Value!, (string[])actual.Value!);
		}
	}

	/// <summary>
	/// Alternatives the first character tells apart open no way, however wide a class that tells
	/// them is: <c>[^ ';' | '\\']</c> is too wide for a switch to name, and decides all the same.
	/// Beside it, two that begin alike, which a failure after them can come back into.
	/// </summary>
	[Theory]
	[InlineData("Plain | Escape", true)]
	[InlineData("Plain | Two", false)]
	public void Alternatives_that_begin_apart_open_no_way(string pair, bool immediate)
	{
		var grammar =
			$$"""
			Start  : @string[] = (s: Pair & ';')* => @(s)
			Pair   : @string = t: ({{pair}})* => @(t)
			Plain  = [^ ';' | '\\']
			Escape = '\\' & ['n' | 't']
			Two    = [^ ';' | '\\'] & ['a'..'z']
			parse Start
			""";

		var told = Assert.Single(Diagnostics(grammar, CarrierKind.Auto), static one => one.Id == GramCompiler.CarrierChosen);

		Assert.Contains(immediate ? "as Immediate" : "read again", told.Message, StringComparison.Ordinal);

		if (!immediate)
			Assert.Contains("Pair", told.Message, StringComparison.Ordinal);

		var tape = Compiled(grammar, CarrierKind.Tape);
		var auto = Compiled(grammar, CarrierKind.Auto);

		foreach (var input in new[] { "ab;c\\n;", "\\t;", "a\\x;", "\\;", ";;", "é\\n;", "" })
		{
			var expected = EmittedCode.Match(tape.Assembly, "Carried.Probe", "TryParseStart", input);
			var actual   = EmittedCode.Match(auto.Assembly, "Carried.Probe", "TryParseStart", input);

			Assert.Equal(expected.IsSuccess, actual.IsSuccess);

			if (expected.IsSuccess)
				Assert.Equal((string[])expected.Value!, (string[])actual.Value!);
			else
				Assert.Equal(expected.Position, actual.Position);
		}
	}

	/// <summary>
	/// An alternative that reads nothing opens no way where it cannot hold where the one before it
	/// read — <c>(eol | ?=eof)</c>, here before a semicolon. Beside it, a lookahead that holds
	/// where the text before it was read, which a failure after it comes back into.
	/// </summary>
	[Theory]
	[InlineData("('\\n' | ?=';')", "", true)]
	[InlineData("('\\n' | eof)", "", true)]
	[InlineData("('\\n' | ?='\\n')", " & '\\n'", false)]
	public void An_empty_alternative_that_cannot_hold_there_opens_no_way(string end, string after, bool immediate)
	{
		var grammar =
			$$"""
			using Std;
			Start : @string[] = (s: Line{{after}} & ';')* => @(s)
			Line  : @string = t: ['a'..'z']+ & {{end}} => @(t)
			parse Start
			""";

		var told = Assert.Single(Diagnostics(grammar, CarrierKind.Auto), static one => one.Id == GramCompiler.CarrierChosen);

		Assert.Contains(immediate ? "as Immediate" : "read again", told.Message, StringComparison.Ordinal);

		if (!immediate)
			Assert.Contains("Line", told.Message, StringComparison.Ordinal);

		var tape = Compiled(grammar, CarrierKind.Tape);
		var auto = Compiled(grammar, CarrierKind.Auto);

		foreach (var input in new[] { "ab\n;cd;", "ab;", "ab\n\n;", "ab\n", "a;b", "" })
		{
			var expected = EmittedCode.Match(tape.Assembly, "Carried.Probe", "TryParseStart", input);
			var actual   = EmittedCode.Match(auto.Assembly, "Carried.Probe", "TryParseStart", input);

			Assert.Equal(expected.IsSuccess, actual.IsSuccess);

			if (expected.IsSuccess)
				Assert.Equal((string[])expected.Value!, (string[])actual.Value!);
			else
				Assert.Equal(expected.Position, actual.Position);
		}
	}

	/// <summary>
	/// A run of texts opens no way where nothing can want the shorter of two, and that holds of
	/// texts built from, <c>"?1" => … | "?0" => …</c>, and of texts that ignore case, compared
	/// ignoring it. Beside each, a shorter text what follows can go on from — in either case.
	/// </summary>
	[Theory]
	[InlineData("@bool", "\"?1\" => @(true) | \"?0\" => @(false)", "';'", true)]
	[InlineData("@bool", "\"?1\" => @(true) | \"?\" => @(false)", "'1'", false)]
	[InlineData("@string", "t: (\"abc\"i | \"ab\"i) => @(t)", "';'", true)]
	[InlineData("@string", "t: (\"abc\"i | \"ab\"i) => @(t)", "'c'", false)]
	[InlineData("@string", "t: (\"abc\"i | \"ab\"i) => @(t)", "'C'", false)]
	public void A_run_of_texts_built_from_or_ignoring_case_opens_no_way(string type, string word, string after, bool immediate)
	{
		var grammar =
			$$"""
			Start : {{type}}[] = (s: Word & {{after}})* => @(s)
			Word  : {{type}} = {{word}}
			parse Start
			""";

		var told = Assert.Single(Diagnostics(grammar, CarrierKind.Auto), static one => one.Id == GramCompiler.CarrierChosen);

		Assert.Contains(immediate ? "as Immediate" : "read again", told.Message, StringComparison.Ordinal);

		if (!immediate)
			Assert.Contains("Word", told.Message, StringComparison.Ordinal);

		var tape = Compiled(grammar, CarrierKind.Tape);
		var auto = Compiled(grammar, CarrierKind.Auto);

		foreach (var input in new[] { "?1;?0;", "?1?11", "abc;AB;", "abcc", "ABCC", "aBC", "ab;", "" })
		{
			var expected = EmittedCode.Match(tape.Assembly, "Carried.Probe", "TryParseStart", input);
			var actual   = EmittedCode.Match(auto.Assembly, "Carried.Probe", "TryParseStart", input);

			Assert.Equal(expected.IsSuccess, actual.IsSuccess);

			if (expected.IsSuccess)
				Assert.Equal(ValueOf(expected), ValueOf(actual));
			else
				Assert.Equal(expected.Position, actual.Position);
		}
	}

	/// <summary>
	/// A fold's operators each begin with the seam, and the character after it tells them apart:
	/// the choice looks past the seam once and the turns are compared past it, so neither opens a
	/// way. Beside it, operators that begin alike past the seam, which a failure can come back into.
	/// </summary>
	[Theory]
	[InlineData("{ ' '* }", "'+'", "'-'", true)]
	[InlineData("' '*", "'+'", "'-'", true)]
	[InlineData("{ ' '* }", "'+' & '+'", "'+'", false)]
	public void A_folds_operators_told_apart_past_the_seam_open_no_way(string trivia, string plus, string minus, bool immediate)
	{
		var grammar =
			$$"""
			trivia = {{trivia}}
			Start : @int = e: Expr => @(e)
			Expr  : @int = l: Expr & {{plus}} & r: Num => @(l + r) | l: Expr & {{minus}} & r: Num => @(l - r) | n: Num => @(n)
			Num   : @int = t: ['0'..'9']+ => @(int.Parse(t))
			parse Start
			""";

		var told = Assert.Single(Diagnostics(grammar, CarrierKind.Auto), static one => one.Id == GramCompiler.CarrierChosen);

		Assert.Contains(immediate ? "as Immediate" : "read again", told.Message, StringComparison.Ordinal);

		var tape = Compiled(grammar, CarrierKind.Tape);
		var auto = Compiled(grammar, CarrierKind.Auto);

		foreach (var input in new[] { "1 + 2 - 3", "1+2", "1 ++ 2 + 3", "12 - 3 +", "1 +", " 1", "" })
		{
			var expected = EmittedCode.Match(tape.Assembly, "Carried.Probe", "TryParseStart", input);
			var actual   = EmittedCode.Match(auto.Assembly, "Carried.Probe", "TryParseStart", input);

			Assert.Equal(expected.IsSuccess, actual.IsSuccess);

			if (expected.IsSuccess)
				Assert.Equal(ValueOf(expected), ValueOf(actual));
			else
				Assert.Equal(expected.Position, actual.Position);
		}
	}

	static IReadOnlyList<GramDiagnostic> Diagnostics(string grammar, CarrierKind carrier)
	{
		return GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Probe",
			Namespace = "Carried",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier = carrier,
		}).Diagnostics;
	}

	[Theory]
	[InlineData("Letter = ['a'..'z']")]
	[InlineData("Letter = Inner\nInner = ['a'..'z']")]
	[InlineData("Letter = ['a'..'m'] | ['n'..'z']")]
	public void Repeated_text_calls_use_one_extent_without_pooled_storage(string leaf)
	{
		var grammar = "Start : @string = t: Letter+ => @(t)\n" + leaf + "\nparse Start";
		var (source, assembly) = Compiled(grammar, CarrierKind.Auto);

		Assert.DoesNotContain("Ways.Rent()", source, StringComparison.Ordinal);
		Assert.DoesNotContain("ImmediateValues", source, StringComparison.Ordinal);
		Assert.DoesNotContain("DirectValues", source, StringComparison.Ordinal);

		foreach (var input in new[] { "a", new string('x', 40), new string('z', 256) })
		{
			var match = EmittedCode.Match(assembly, "Carried.Probe", "TryParseStart", input);

			Assert.True(match.IsSuccess);
			Assert.Equal(input, ValueOf(match));
		}

		foreach (var input in new[] { "", "!", "abc!" })
			Assert.False(EmittedCode.Match(assembly, "Carried.Probe", "TryParseStart", input).IsSuccess);
	}

	[Theory]
	[InlineData("Start : @string = t: Letter+ & 'a' => @(t)\nLetter = ['a'..'z']", "bca", "bc")]
	[InlineData("Start : @string = t: Letter* => @(t)\nLetter = ['a'..'z']", "", "")]
	[InlineData("Start : @string = t: Letter? => @(t ?? \"missing\")\nLetter = ['a'..'z']", "", "missing")]
	[InlineData("trivia = ' '*\nStart : @string = (t: Letter & ',')+ => @(t)\nLetter = ['a'..'z']", "a , b , c ,", "abc")]
	[InlineData("Start : @string = t: Letter+ => @(t)\nLetter = 'a' | '(' & Letter & ')'", "a(a)", "a(a)")]
	[InlineData("Start : @string = t: Letter+ => @(string.Join(\"|\", t))\nLetter : @string = c: ['a'..'z'] => @(c)", "abc", "a|b|c")]
	[InlineData("Start : @string = t: Letter+ => @(t)\nLetter = ?=['a'..'z'] & ['a'..'z']", "abc", "abc")]
	[InlineData("Start : @string = t: Letter+ => @(t)\nLetter = ['a'..'z'] & when @(true)", "abc", "abc")]
	public void Text_call_capture_preserves_returns_gaps_and_value_shapes(string grammar, string input, string expected)
	{
		foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape })
		{
			var (_, assembly) = Compiled(grammar + "\nparse Start", carrier);
			var match = EmittedCode.Match(assembly, "Carried.Probe", "TryParseStart", input);

			Assert.True(match.IsSuccess);
			Assert.Equal(expected, ValueOf(match));
		}
	}

	[Fact]
	public void Immediate_storage_contains_only_the_gathered_value_type()
	{
		const string Grammar = """
			Start : @int = items: Item+ => @(items.Length)
			Item : @string = c: ['a'..'z'] => @(c)
			parse Start
			""";

		var (source, assembly) = Compiled(Grammar, CarrierKind.Immediate);

		Assert.DoesNotContain("StackText", source, StringComparison.Ordinal);
		Assert.DoesNotContain("StackSpans", source, StringComparison.Ordinal);
		Assert.DoesNotContain("internal int[] Stack", source, StringComparison.Ordinal);
		Assert.Contains("internal string[] Stack", source, StringComparison.Ordinal);

		foreach (var input in new[] { "a", new string('x', 40), "b", new string('y', 100), "c" })
		{
			var match = EmittedCode.Match(assembly, "Carried.Probe", "TryParseStart", input);

			Assert.True(match.IsSuccess);
			Assert.Equal(input.Length, Assert.IsType<int>(match.Value));
		}
	}

	[Fact]
	public void Immediate_text_pieces_do_not_allocate_value_stacks()
	{
		const string Grammar = """
			Start : @string = (piece: Letter & ',')+ => @(piece)
			Letter = ['a'..'z']
			parse Start
			""";

		var (source, assembly) = Compiled(Grammar, CarrierKind.Immediate);

		Assert.Contains("internal long[] StackSpans", source, StringComparison.Ordinal);
		Assert.DoesNotContain("StackText", source, StringComparison.Ordinal);
		Assert.DoesNotContain("internal string[] Stack", source, StringComparison.Ordinal);

		foreach (var input in new[] { "a,b,c,", "x,", "a,b,c," })
		{
			var match = EmittedCode.Match(assembly, "Carried.Probe", "TryParseStart", input);

			Assert.True(match.IsSuccess);
			Assert.Equal(input.Replace(",", "", StringComparison.Ordinal), match.Value);
		}
	}

	[Fact]
	public void Immediate_publications_share_the_union_of_their_required_stacks()
	{
		const string Grammar = """
			Words : @int = items: Word+ => @(items.Length)
			Word : @string = c: ['a'..'z'] => @(c)
			Numbers : @string = items: Number+ => @(string.Join(",", items))
			Number : @int = ['0'..'9'] => @(1)
			parse Words
			parse Numbers
			""";

		var (source, assembly) = Compiled(Grammar, CarrierKind.Immediate);

		Assert.Contains("internal int[] Stack", source, StringComparison.Ordinal);
		Assert.Contains("internal string[] Stack", source, StringComparison.Ordinal);
		Assert.DoesNotContain("StackText", source, StringComparison.Ordinal);
		Assert.DoesNotContain("StackSpans", source, StringComparison.Ordinal);

		for (var pass = 0; pass < 3; pass++)
		{
			Assert.Equal(3, EmittedCode.Match(assembly, "Carried.Probe", "TryParseWords", "abc").Value);
			Assert.Equal("1,1", EmittedCode.Match(assembly, "Carried.Probe", "TryParseNumbers", "12").Value);
		}
	}

	[Theory]
	[InlineData(CarrierKind.Auto)]
	[InlineData(CarrierKind.Immediate)]
	public void Immediate_scalar_recursion_does_not_rent_value_storage(CarrierKind carrier)
	{
		const string Grammar = """
			Depth : @int = 'a' => @(1) | '(' & n: Depth & ')' => @(n + 1)
			parse Depth
			""";

		var (source, assembly) = Compiled(Grammar, carrier);

		Assert.DoesNotContain("Ways.Rent()", source, StringComparison.Ordinal);
		Assert.DoesNotContain("readonly Ways ways", source, StringComparison.Ordinal);
		Assert.DoesNotContain("ImmediateValues.Rent()", source, StringComparison.Ordinal);
		Assert.DoesNotContain("ImmediateValues.Return(", source, StringComparison.Ordinal);
		Assert.DoesNotContain("ImmediateValues values", source, StringComparison.Ordinal);
		Assert.DoesNotContain("static ImmediateValues? _spare", source, StringComparison.Ordinal);

		foreach (var depth in new[] { 0, 40, 1, 100, 0 })
		{
			var input = new string('(', depth) + "a" + new string(')', depth);
			var match = EmittedCode.Match(assembly, "Carried.Probe", "TryParseDepth", input);

			Assert.True(match.IsSuccess);
			Assert.Equal(depth + 1, match.Value);
			Assert.False(EmittedCode.Match(assembly, "Carried.Probe", "TryParseDepth", input + ")").IsSuccess);
		}
	}

	[Fact]
	public void Immediate_scalar_reader_does_not_rent_another_publications_stacks()
	{
		const string Grammar = """
			Depth : @int = 'a' => @(1) | '(' & n: Depth & ')' => @(n + 1)
			Words : @int = items: Word+ => @(items.Length)
			Word : @string = c: ['a'..'z'] => @(c)
			parse Depth
			parse Words
			""";

		var (source, assembly) = Compiled(Grammar, CarrierKind.Immediate);

		// Only Words and its positional entry rent the shared store.
		Assert.Equal(2, source.Split("var values = ImmediateValues.Rent();", StringSplitOptions.None).Length - 1);
		Assert.Contains("internal string[] Stack", source, StringComparison.Ordinal);

		for (var pass = 0; pass < 3; pass++)
		{
			Assert.Equal(3, EmittedCode.Match(assembly, "Carried.Probe", "TryParseWords", "abc").Value);
			Assert.Equal(3, EmittedCode.Match(assembly, "Carried.Probe", "TryParseDepth", "((a))").Value);
			Assert.False(EmittedCode.Match(assembly, "Carried.Probe", "TryParseWords", "abc!").IsSuccess);
			Assert.False(EmittedCode.Match(assembly, "Carried.Probe", "TryParseDepth", "((a)").IsSuccess);
		}
	}

	[Theory]
	[InlineData("Start : @int = items: Item+ => @(items.Length)\nItem : @int = 'a' => @(1)")]
	[InlineData("Start : @int = 'a' => @(1) | '(' & n: Start & ')' => @(n + 1)")]
	[InlineData("Start : @int = items: Item* & 'a' => @(items.Length)\nItem : @int = 'a' => @(1)")]
	[InlineData("Start : @int = ?!'b' & n: Item => @(n)\nItem : @int = 'a' => @(1)")]
	[InlineData("Start : @int = { n: Item } & 'a' => @(n)\nItem : @int = 'a' => @(1) | 'a' & 'a' => @(2)")]
	[InlineData("trivia = ' '*\nStart : @int = n: Item => @(n)\nItem : @int = 'a' => @(1)")]
	public void Optional_ways_preserve_results_and_failures(string grammar)
	{
		var (_, immediate) = Compiled(grammar + "\nparse Start", CarrierKind.Immediate);
		var (_, tape) = Compiled(grammar + "\nparse Start", CarrierKind.Tape);

		foreach (var input in new[] { "", "a", "aa", "aaa", "b", "ab", "(a)", "((a))", "((a)", "(b)", " a ", " a b " })
		{
			var expected = EmittedCode.Match(tape, "Carried.Probe", "TryParseStart", input);
			var actual = EmittedCode.Match(immediate, "Carried.Probe", "TryParseStart", input);

			Assert.Equal(expected, actual);
		}
	}

	[Fact]
	public void Immediate_publications_rent_ways_only_for_the_reader_that_replays()
	{
		const string Grammar = """
			Depth : @int = 'x' => @(1) | '(' & n: Depth & ')' => @(n + 1)
			Replay : @int = items: Item* & 'a' => @(items.Length)
			Item : @int = 'a' => @(1)
			parse Depth
			parse Replay
			""";

		var (source, assembly) = Compiled(Grammar, CarrierKind.Immediate);

		Assert.Equal(2, source.Split("var ways = Ways.Rent();", StringSplitOptions.None).Length - 1);

		for (var pass = 0; pass < 3; pass++)
		{
			Assert.Equal(2, EmittedCode.Match(assembly, "Carried.Probe", "TryParseReplay", "aaa").Value);
			Assert.Equal(3, EmittedCode.Match(assembly, "Carried.Probe", "TryParseDepth", "((x))").Value);
			Assert.False(EmittedCode.Match(assembly, "Carried.Probe", "TryParseReplay", "aaab").IsSuccess);
			Assert.False(EmittedCode.Match(assembly, "Carried.Probe", "TryParseDepth", "((x)").IsSuccess);
		}
	}

	[Theory]
	[InlineData("' '*", false)]
	[InlineData("'a'*", true)]
	public void Entry_trivia_rents_ways_only_when_its_reading_needs_them(string trivia, bool needsWays)
	{
		var grammar = "trivia = " + trivia + "\n" + """
			Depth : @int = 'a' => @(1) | '(' & n: Depth & ')' => @(n + 1)
			parse Depth
			""";
		var (source, immediate) = Compiled(grammar, CarrierKind.Immediate);
		var (_, tape) = Compiled(grammar, CarrierKind.Tape);

		Assert.Equal(needsWays, source.Contains("Ways.Rent()", StringComparison.Ordinal));

		foreach (var input in new[] { "", "a", "aaa", "  a  ", "( a )", " (( a )) ", "(a)", "((a))", "(a", "a!", " b " })
		{
			Assert.Equal(
				EmittedCode.Match(tape, "Carried.Probe", "TryParseDepth", input),
				EmittedCode.Match(immediate, "Carried.Probe", "TryParseDepth", input));

			var window = "!" + input + "!";
			Assert.Equal(
				EmittedCode.Positioned(tape, "Carried.Probe", "TryParseDepth", window, 1),
				EmittedCode.Positioned(immediate, "Carried.Probe", "TryParseDepth", window, 1));
			Assert.Equal(
				EmittedCode.Positioned(tape, "Carried.Probe", "TryParseDepth", window, 1, input.Length),
				EmittedCode.Positioned(immediate, "Carried.Probe", "TryParseDepth", window, 1, input.Length));
		}
	}

	static string? ValueOf((bool IsSuccess, object? Value, string? Error, long Position) match)
	{
		return match.Value?.ToString();
	}

	static (string Source, Assembly Assembly) Compiled(string grammar, CarrierKind carrier)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Probe",
			Namespace     = "Carried",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier       = carrier,
		});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		var source = Assert.Single(result.Sources).Text;

		return (source, EmittedCode.Compile(source, "Probe", "Carried"));
	}
}
