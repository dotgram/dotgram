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
/// hand-written readings in <c>benchmarks/DotGram.HandDeferred</c> ran on themselves,
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
	static readonly (string Name, string Grammar, string[] Inputs)[] Shapes =
	[
		("a fold",
			"trivia = ' '*\n" +
			"Start : @string = l: Start & '+' & r: Pair => @(l + \"+\" + r)\n" +
			"                | one: Pair => @(one)\n" +
			"Pair : @string = name: Name & '=' & value: Digits => @(name + \":\" + value)\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["a = 1 + bb = 22", "a=1", "a = 1 + bb", "a = ", ""]),

		("a cycle",
			"trivia = ' '*\n" +
			"Start : @string = l: Start & '+' & r: Pair => @(l + \"+\" + r)\n" +
			"                | one: Pair => @(one)\n" +
			"Pair : @string = name: Name & '=' & value: Digits => @(name + \":\" + value)\n" +
			"               | '(' & inner: Start & ')' => @(\"(\" + inner + \")\")\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["a = 1 + (b = 2 + c = 3)", "((a = 1))", "(a = 1", "a = 1 + (b = 2"]),

		("records gathered",
			"Start : @string = first: Name & (',' & rest: Name)* => @(first + \"|\" + string.Join(\"|\", rest))\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"parse Start\n",
			["a,b,c", "a", "a,", ",a"]),

		("text gathered",
			"Start : @string = (parts: ['a'..'z']+ & ','?)+ => @(parts)\n" +
			"parse Start\n",
			["ab,cd,e", "ab", "ab,,cd", ""]),

		// An extent has no record to be the span of and needs none: the two positions are
		// the reader's own locals. Read here beside a rule that builds and a rule that
		// collects, because what a carrier does to one it must do to all three.
		("an extent",
			"trivia = ' '*\n" +
			"Where : @SourceSpan = ['a'..'z']+\n" +
			"Name  : @string     = t: ['a'..'z']+ => @(t)\n" +
			"Pair  : @string     = n: Name & '=' & w: Where => @(n + \"@\" + w.Start + \":\" + w.Length)\n" +
			"Start : @string     = first: Pair & (',' & rest: Pair)* => @(first + string.Join(\"|\", rest))\n" +
			"parse Start\n",
			["ab=cd", "ab = cd , ef = gh", "ab=", "", "ab=cd,"]),

		("a guard over a record",
			"Start : @string = d: Digits & when @(d.Length < 3) => @(\"<\" + d + \">\")\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["12", "1234", "", "x"]),

		("marks laid over an operand",
			"state : @int\n" +
			"Start : @string = '(' & inner: Start with state @(9) & ';' => @(\"<\" + inner + \">\")\n" +
			"                | '(' & inner: Start with state @(1) & ')' => @(\"(\" + inner + \")\")\n" +
			"                | t: ['a'..'z']+ => @(string.Join(\",\", parserState.ToArray()) + \":\" + t)\n" +
			"parse Start\n",
			["a", "(a)", "((a))", "(a;", "(a"]),

		("two captures of one folding rule",
			"trivia = ' '*\n" +
			"Start : @string = l: Sum & '=' & r: Sum => @(l + \"/\" + r)\n" +
			"Sum : @string = a: Sum & '+' & b: Name => @(a + \"+\" + b)\n" +
			"              | one: Name => @(one)\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"parse Start\n",
			["a = b", "a + b = c", "a = b + c", "a + b = c + d", "a ="]),

		("a rule that reaches itself",
			"trivia = ' '*\n" +
			"Start : @string = '(' & inner: Start & ')' => @(\"(\" + inner + \")\")\n" +
			"                | t: Name => @(t)\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"parse Start\n",
			["a", "(a)", "( ( a ) )", "(a", ""]),

		("a rule that builds two ways",
			"Start : @string = a: Name & b: Digits? => @(\"n\" + a + (b ?? \"-\"))\n" +
			"                | d: Digits & c: Name? => @(\"d\" + d + (c ?? \"-\"))\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["ab12", "12ab", "ab", "12", "", "1a2"]),

		("a member that may be missing",
			"Start : @string = a: Name & b: Digits? => @(a + (b ?? \"-\"))\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["ab12", "ab", "12", ""]),
	];

	public static IEnumerable<object[]> Every() => Shapes.Select(one => new object[] { one.Name });

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
	/// What the mixed carrier carries today, so that the theory above cannot pass by
	/// carrying nothing.
	/// </summary>
	/// <remarks>
	/// This is the list that grows as the shapes are written, and the one line to change
	/// when one is. A rule that builds one way, out of runs of text and of other rules'
	/// values, is the first of them.
	/// </remarks>
	[Fact]
	public void The_mixed_carrier_carries_a_rule_that_builds_one_way()
	{
		var (source, _) = Compiled(Shapes.Single(one => one.Name == "a member that may be missing").Grammar, CarrierKind.Mixed);

		Assert.Contains("private readonly struct Shape_Start", source, StringComparison.Ordinal);
		Assert.Contains("value = reader.shape_Start.Build(text);", source, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram", source, StringComparison.Ordinal);
		Assert.DoesNotContain("DirectValues", source, StringComparison.Ordinal);
	}

	/// <summary>
	/// The mixed carrier agrees with the tape on every shape it carries, and leaves the
	/// rest to the tape.
	/// </summary>
	/// <remarks>
	/// One test for both, because which of the two a shape is changes as the carrier is
	/// written and the test should not have to be edited when it does. A shape it carries
	/// is held to the tape's answers; a shape it refuses is held only to being refused,
	/// which is what the fallback below is about.
	/// </remarks>
	[Theory]
	[MemberData(nameof(Every))]
	public void Mixed_agrees_with_the_tape_on_what_it_carries(string name)
	{
		var (_, grammar, inputs) = Shapes.Single(one => one.Name == name);

		var tape  = Compiled(grammar, CarrierKind.Tape);
		var mixed = Compiled(grammar, CarrierKind.Mixed);

		if (!mixed.Source.Contains("Shape_", StringComparison.Ordinal))
		{
			Assert.Contains("Materialize_DotGram", mixed.Source, StringComparison.Ordinal);

			return;
		}

		Assert.DoesNotContain("Materialize_DotGram", mixed.Source, StringComparison.Ordinal);

		foreach (var input in inputs)
		{
			var expected = EmittedCode.Match(tape.Assembly,  "Carried.Probe", "TryParseStart", input);
			var actual   = EmittedCode.Match(mixed.Assembly, "Carried.Probe", "TryParseStart", input);

			Assert.True(
				expected.IsSuccess == actual.IsSuccess,
				$"{name} on \"{input}\": the tape says {expected.IsSuccess}, mixed says {actual.IsSuccess}.");

			if (expected.IsSuccess)
				Assert.Equal(ValueOf(expected), ValueOf(actual));
		}
	}

	/// <summary>
	/// A rule that builds several ways is one shape and a byte saying which, rather than a
	/// shape for each and a virtual call to build it.
	/// </summary>
	[Fact]
	public void A_shape_of_several_constructions_says_which_it_holds()
	{
		var (source, _) = Compiled(Shapes.Single(one => one.Name == "a rule that builds two ways").Grammar, CarrierKind.Mixed);

		Assert.Contains("private readonly byte which;", source, StringComparison.Ordinal);
		Assert.Contains("switch (this.which)",          source, StringComparison.Ordinal);
		Assert.Contains("Shape_Start.Of0(",             source, StringComparison.Ordinal);
		Assert.Contains("Shape_Start.Of1(",             source, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram",    source, StringComparison.Ordinal);
	}

	/// <summary>
	/// A rule that can be reached from itself is a class, because a value cannot contain
	/// itself; everything off every cycle stays a value held inside whatever captured it.
	/// </summary>
	[Fact]
	public void A_shape_that_can_reach_itself_is_a_class()
	{
		var (source, _) = Compiled(Shapes.Single(one => one.Name == "a rule that reaches itself").Grammar, CarrierKind.Mixed);

		Assert.Contains("private sealed class Shape_Start",   source, StringComparison.Ordinal);
		Assert.Contains("private readonly struct Shape_Name", source, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram",          source, StringComparison.Ordinal);
	}

	/// <summary>
	/// A fold is a base and a run of turns threaded through the turns themselves, and what
	/// it is worth is one loop over that run rather than a frame per turn.
	/// </summary>
	/// <remarks>
	/// A chain built the other way round would read the same language and put a depth limit
	/// where the tape has none: a thousand terms would be a thousand frames.
	/// </remarks>
	[Fact]
	public void A_fold_is_a_run_threaded_through_its_own_turns()
	{
		var (source, _) = Compiled(Shapes.Single(one => one.Name == "a fold").Grammar, CarrierKind.Mixed);

		Assert.Contains("private sealed class Step_Start",  source, StringComparison.Ordinal);
		Assert.Contains("internal Step_Start? Next;",       source, StringComparison.Ordinal);
		Assert.Contains("private readonly struct Base_Start", source, StringComparison.Ordinal);
		Assert.Contains("for (var turn = 0; turn < this._count; turn++)", source, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram",       source, StringComparison.Ordinal);
	}

	/// <summary>
	/// What a repetition gathers is pushed on a stack as it is read and taken as one array
	/// where the record is written; a run of text keeps where its pieces stand, not what
	/// they say.
	/// </summary>
	[Fact]
	public void What_a_repetition_gathers_is_taken_as_one_array()
	{
		var (records, _) = Compiled(Shapes.Single(one => one.Name == "records gathered").Grammar, CarrierKind.Mixed);
		var (pieces,  _) = Compiled(Shapes.Single(one => one.Name == "text gathered").Grammar,   CarrierKind.Mixed);

		Assert.Contains("sealed class MixedValues",       records, StringComparison.Ordinal);
		Assert.Contains("values.PushShape_Name(",         records, StringComparison.Ordinal);
		Assert.Contains("private readonly Shape_Name[] _g", records, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram",      records, StringComparison.Ordinal);

		Assert.Contains("values.PushSpans(",              pieces,  StringComparison.Ordinal);
		Assert.Contains("private readonly long[] _g",       pieces,  StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram",      pieces,  StringComparison.Ordinal);
	}

	/// <summary>
	/// A guard that reads a captured value costs a shape nothing extra: the shape is there,
	/// and what it is worth is one call away.
	/// </summary>
	[Fact]
	public void A_guard_reads_a_shape_by_building_it()
	{
		var (source, _) = Compiled(Shapes.Single(one => one.Name == "a guard over a record").Grammar, CarrierKind.Mixed);

		Assert.Contains(".Build(text)",              source, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram", source, StringComparison.Ordinal);
	}

	/// <summary>
	/// One member captured in two places that read two rules is two fields, because a shape
	/// is per rule and the two are two types.
	/// </summary>
	/// <remarks>
	/// The tape and the immediate carrier never meet this: they hand values about by value
	/// type, and both places build what the member is declared as.
	/// </remarks>
	[Fact]
	public void A_member_captured_in_two_places_is_two_fields()
	{
		const string Twice =
			"trivia = ' '*\n" +
			"Start : @string = t: Word & x: Digits? => @(\"w:\" + t + (x ?? \"-\"))\n" +
			"                | t: Digits & y: Word? => @(\"d:\" + t + (y ?? \"-\"))\n" +
			"Word : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n";

		var tape  = Compiled(Twice, CarrierKind.Tape);
		var mixed = Compiled(Twice, CarrierKind.Mixed);

		Assert.Contains("Shape_Word",   mixed.Source, StringComparison.Ordinal);
		Assert.Contains("Shape_Digits", mixed.Source, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram", mixed.Source, StringComparison.Ordinal);

		foreach (var input in new[] { "abc", "abc12", "123", "123abc", "", "a1b" })
		{
			var expected = EmittedCode.Match(tape.Assembly,  "Carried.Probe", "TryParseStart", input);
			var actual   = EmittedCode.Match(mixed.Assembly, "Carried.Probe", "TryParseStart", input);

			Assert.Equal(expected.IsSuccess, actual.IsSuccess);

			if (expected.IsSuccess)
				Assert.Equal(ValueOf(expected), ValueOf(actual));
		}
	}

	/// <summary>
	/// A carrier that cannot carry a grammar is not an error: the tape carries it instead,
	/// and the machine keeps the reason for whoever asks.
	/// </summary>
	/// <remarks>
	/// The mixed carrier carries nothing yet — its shapes are written one at a time — so it
	/// is the one to ask this of. What the test is really about is the fallback, which every
	/// carrier after this one will need on the way to being finished.
	/// </remarks>
	[Fact]
	public void A_carrier_that_refuses_a_grammar_leaves_it_to_the_tape()
	{
		// A rule whose value is the extent it matched, which no carrier but the tape
		// carries and none is going to: there is no record for the span to be the span of.
		const string Extent =
			"Start : @SourceSpan = ['a'..'z']+\n" +
			"parse Start\n";

		var (source, _) = Compiled(Extent, CarrierKind.Mixed);

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

	/// <summary>A rule read for a reading that is thrown away keeps the grammar on the tape, and is named.</summary>
	[Fact]
	public void Left_to_the_generator_a_rule_read_for_nothing_keeps_the_tape()
	{
		const string Ahead =
			"""
			Start : @string = ?=Name & n: Name => @(n)
			Name  : @string = t: ['a'..'z']+ => @(t)
			parse Start
			""";

		var (source, _) = Compiled(Ahead, CarrierKind.Auto);

		Assert.Contains("Materialize_DotGram", source, StringComparison.Ordinal);

		var told = Assert.Single(Diagnostics(Ahead, CarrierKind.Auto), static one => one.Id == GramCompiler.CarrierChosen);

		Assert.Contains("on the tape", told.Message, StringComparison.Ordinal);
		Assert.Contains("Name",        told.Message, StringComparison.Ordinal);
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

	static IReadOnlyList<GramDiagnostic> Diagnostics(string grammar, CarrierKind carrier) =>
		GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Probe",
			Namespace     = "Carried",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier       = carrier,
		}).Diagnostics;

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

	static string? ValueOf((bool IsSuccess, object? Value, string? Error, long Position) match) =>
		match.Value?.ToString();

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
