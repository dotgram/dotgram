using System;
using System.IO;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The emitter checked by compiling what it wrote and running it.
/// </summary>
/// <remarks>
/// Asserting on the text would only say the generator is consistent with itself.
/// Compiling it says it is valid C#, and running it says the grammar recognizes what
/// it claims to — which is the only claim worth making at this stage.
/// </remarks>
public sealed class CSharpEmitterTests
{
	/// <summary>Compiles a grammar the way the generator does, and returns the C#.</summary>
	static string Emit(string grammar)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

		// Anything but information. A grammar is allowed to be told what it did not get and
		// still be a grammar the emitter should be asked about — that is what `Info` is
		// for, and §6.3's "no reader overload" is one.
		Assert.DoesNotContain(
			result.Diagnostics,
			static diagnostic => diagnostic.Severity != GramSeverity.Info);

		return Assert.Single(result.Sources).Text;
	}

	/// <summary>Compiles the emitted source and calls the asking half of a publication.</summary>
	static (bool Matched, object? Value) Invoke(string grammar, string method, string input)
	{
		var (isSuccess, value, _, _) =
			EmittedCode.Match(EmittedCode.Compile(Emit(grammar)), "Grammar", "Try" + method, input);

		return (isSuccess, value);
	}

	/// <summary>The common case: one rule, published with <c>parse</c>.</summary>
	static (bool Matched, string Value) Run(string grammar, string input)
	{
		var (matched, value) = Invoke(grammar + "\nparse Start", "ParseStart", input);

		return (matched, (string)(value ?? ""));
	}

	[Theory]
	[InlineData("abc",  true)]
	[InlineData("abd",  false)]
	[InlineData("ab",   false)]
	[InlineData("abcd", false)]     // parse requires the whole input
	public void Literals(string input, bool expected) =>
		Assert.Equal(expected, Run("""Start = 'a' & 'b' & 'c'""", input).Matched);

	[Theory]
	[InlineData("42",    true)]
	[InlineData("0",     true)]
	[InlineData("",      false)]
	[InlineData("4x",    false)]
	public void Element_sets_and_repetition(string input, bool expected) =>
		Assert.Equal(expected, Run("Start = ['0'..'9']+", input).Matched);

	[Fact]
	public void Simple_recursive_rule_uses_the_shared_parser_arena()
	{
		const int depth = 100_000;
		const string grammar = "Start = '(' & Start & ')' | 'x'\nparse Start";

		var source = Emit(grammar);
		var parser = EmittedCode.Compile(source);
		var input = new string('(', depth) + "x" + new string(')', depth);

		Assert.Contains("ParserArena Entries", source);
		Assert.DoesNotContain("List<ParserEntry>", source);
		Assert.DoesNotContain("Span<int> calls", source);
		Assert.True(EmittedCode.Match(parser, "Grammar", "TryParseStart", input).IsSuccess);
		Assert.False(EmittedCode.Match(parser, "Grammar", "TryParseStart", input + ")").IsSuccess);
	}

	[Fact]
	public void Recursive_invocations_keep_their_repetition_state_apart()
	{
		const int depth = 20_000;
		const string grammar = "Start = 'a'* & ('(' & Start & ')' | 'x')\nparse Start";

		var source = Emit(grammar);
		var parser = EmittedCode.Compile(source);
		var input = string.Concat(Enumerable.Repeat("a(", depth)) + "ax" + new string(')', depth);

		Assert.Contains("ParserEntry.Repeat", source);
		Assert.DoesNotContain("Span<int> calls", source);
		Assert.True(EmittedCode.Match(parser, "Grammar", "TryParseStart", input).IsSuccess);
	}

	[Fact]
	public void Mutually_recursive_rules_share_one_automaton()
	{
		const int depth = 50_000;
		const string grammar =
			"A = 'a' & B | 'x'\n" +
			"B = 'b' & A | 'y'\n" +
			"parse A";

		var source = Emit(grammar);
		var parser = EmittedCode.Compile(source);
		var input = string.Concat(Enumerable.Repeat("ab", depth)) + "x";

		Assert.Contains("ParserArena Entries", source);
		Assert.DoesNotContain("List<ParserEntry>", source);
		Assert.DoesNotContain("Recognize_A(", source);
		Assert.DoesNotContain("Recognize_B(", source);
		Assert.True(EmittedCode.Match(parser, "Grammar", "TryParseA", input).IsSuccess);
		Assert.False(EmittedCode.Match(parser, "Grammar", "TryParseA", input + "b").IsSuccess);
	}

	[Fact]
	public void A_cycle_of_three_rules_is_a_cycle()
	{
		// Recursion is reachability from a rule to itself, which is not the same as a rule
		// naming itself or two rules naming each other. If a cycle of three were missed, each
		// of them would be compiled into the next — none keeps a value, so nothing else stops
		// it — and the expansion would not terminate.
		const int depth = 20_000;
		const string grammar =
			"A = 'a' & B\n" +
			"B = 'b' & C\n" +
			"C = 'c' & A | 'x'\n" +
			"parse A";

		var source = Emit(grammar);
		var input  = string.Concat(Enumerable.Repeat("abc", depth)) + "abx";

		Assert.Contains("call B", source);
		Assert.True(EmittedCode.Match(EmittedCode.Compile(source), "Grammar", "TryParseA", input).IsSuccess);
	}

	[Fact]
	public void A_rule_whose_value_is_kept_has_one_shared_block()
	{
		// Three calls, one block. The value is what needs the block: it is materialized at
		// the rule's own boundary, so the boundary has to be there to materialize it at.
		var source = Emit(
			"""
			Start : @string = a: Name & ':' & b: Name & ':' & c: Name => @(a + b + c)
			Name  : @string = t: ('a' | 'b') => @(t)
			parse Start
			""");

		// Three call sites, and a rule that is called at all is a rule with a block: one
		// that keeps nothing is compiled where it is called and has no call site to count,
		// which is the test below.
		Assert.Equal(3, source.Split(["call Name"], StringSplitOptions.None).Length - 1);
		Assert.Contains("Conditional(\"DOTGRAM_TRACE\")", source);
		Assert.Contains("Debug.Assert", source);
	}

	[Fact]
	public void A_rule_that_keeps_nothing_is_compiled_where_it_is_called()
	{
		// And one that needs no boundary gets none: it is its caller's control flow, three
		// times over, and the block it would have been is never written. What that costs is
		// text, which this project spends; what it buys is the call, the frame and the jump
		// back — and the sight of the body in place, where the analyses that decide how to
		// compile a repetition can see it.
		var source = Emit(
			"""
			Start = Name & ':' & Name & ':' & Name
			Name = 'a' | 'b'
			parse Start
			""");

		Assert.DoesNotContain("call Name", source);
	}

	[Fact]
	public void What_the_rule_matched_is_built_only_where_it_is_asked_for()
	{
		// It is the whole run the rule covered, made into a string on every construction, so
		// a rule that never looks at it would allocate its own text beside the captures it
		// actually keeps — twice the string, for a value that is one of them.
		Assert.DoesNotContain(
			"parserText",
			Emit("Start : @string = t: ['a'..'z']+ => @(t)\nparse Start"));

		Assert.Contains(
			"Construct_Start(string parserText",
			Emit("Start : @string = ['a'..'z']+ => @(parserText)\nparse Start"));
	}

	[Fact]
	public void Text_alternatives_that_cannot_both_match_need_nothing_written_down()
	{
		// At most one of them matches anywhere, so there is no second reading to come back
		// for. What they share is read once, and the position is not moved until one of them
		// has matched whole, which is what makes reading it once possible.
		const string grammar = """Start = "abc_x" | "abc_y" """;

		Assert.DoesNotContain(
			"entries.Add(new ParserEntry(ParserEntry.Choice", Emit(grammar + "\nparse Start"));
		Assert.True(Run(grammar, "abc_y").Matched);
		Assert.False(Run(grammar, "abc_z").Matched);
	}

	[Fact]
	public void But_one_that_begins_another_still_needs_it()
	{
		// Both match at the same place, the shorter is taken, and if what follows the choice
		// then fails the longer has to be tried. That is a way back, and a way back is what
		// the entry is.
		const string grammar = """Start = ("ab" | "abc") & 'c' """;

		Assert.Contains(
			"entries.Add(new ParserEntry(ParserEntry.Choice", Emit(grammar + "\nparse Start"));
		Assert.True(Run(grammar, "abc").Matched);
	}

	[Fact]
	public void Unless_the_longer_comes_first_and_nothing_can_follow_where_the_shorter_would_stand()
	{
		// Taking "https" and failing later would leave "http" standing at the 's' the longer
		// went on with — and "://" does not begin with one, so the shorter reading fails
		// wherever it is tried. An entry that leads only to a failure is one nothing needs.
		const string grammar = """Start = ("https" | "http" | "ftp") & "://" """;

		Assert.DoesNotContain(
			"entries.Add(new ParserEntry(ParserEntry.Choice", Emit(grammar + "\nparse Start"));
		Assert.True(Run(grammar, "https://").Matched);
		Assert.True(Run(grammar, "http://").Matched);
		Assert.True(Run(grammar, "ftp://").Matched);
		Assert.False(Run(grammar, "htt://").Matched);
	}

	[Fact]
	public void The_shorter_written_first_is_still_a_reading_to_come_back_for()
	{
		// The same pair the other way round. "http" is taken first, and coming back for the
		// second alternative is the only thing that can ever match the extra character —
		// docs/syntax.md §11 promises alternatives are never reordered, so this is a fact
		// about the grammar as written and not one to optimize away.
		const string grammar = """Start = ("http" | "https") & "://" """;

		Assert.Contains(
			"entries.Add(new ParserEntry(ParserEntry.Choice", Emit(grammar + "\nparse Start"));
		Assert.True(Run(grammar, "https://").Matched);
		Assert.True(Run(grammar, "http://").Matched);
	}

	[Fact]
	public void Nor_where_what_follows_can_begin_where_the_shorter_would_stand()
	{
		// Longer first, but 'b' is exactly the character "ab" went on with, so taking it and
		// failing leaves "a" standing somewhere 'b' can begin: a real second reading, and
		// the entry is what reaches it.
		const string grammar = """Start = ("ab" | "a") & 'b' """;

		Assert.Contains(
			"entries.Add(new ParserEntry(ParserEntry.Choice", Emit(grammar + "\nparse Start"));
		Assert.True(Run(grammar, "ab").Matched);
	}

	[Fact]
	public void A_guard_is_handed_what_it_names_and_not_what_it_could_have()
	{
		// Every value a guard is given is built to give it — a run cut into a string, a rule's
		// value materialized — and it runs at every position the rule reaches it. A condition
		// asking about one capture used to be handed all of them.
		var source = Emit(
			"""
			Start = a: "xy" & b: "z" & when @(b == "z")
			parse Start
			""");

		Assert.Contains("Recognize_DotGram_Guard0(string b)", source);
		Assert.DoesNotContain("string? a", source);
	}

	[Fact]
	public void Text_captures_are_records_in_the_shared_parser_arena()
	{
		var source = Emit("Start = digits: ['0'..'9']+\nparse Start");

		Assert.Contains("ParserEntry.Capture", source);
		Assert.DoesNotContain("Recognize_Start(", source);

		// No longer also `Assert.DoesNotContain("List<string>", source)`: every
		// TryParseX wrapper flattens `Failure.Expected`/`ExpectedMore` into a local
		// `List<string>` once, on an actual overall failure (§11's first-tier
		// diagnostics), unrelated to how a capture is stored — the claim this test
		// makes is about captures, not about the file being free of `List<string>`
		// altogether.
	}

	[Fact]
	public void Construction_is_recorded_and_runs_only_after_acceptance()
	{
		// The choice needs a way back — "a" continues into "ab" — so the rule stays on
		// the engine, where construction is an arena record resolved at Accept.
		var source = Emit(
			"Start : @int = digits: ['0'..'9']+ & (\"a\" | \"ab\") => @int.Parse(digits)\n" +
			"parse Start");

		Assert.Contains("ParserEntry.Construct", source);
		Assert.Contains("entries[call] = new ParserEntry(ParserEntry.Completed", source);
		Assert.DoesNotContain("Recognize_Start(", source);
		Assert.True(
			source.IndexOf("Accept:", StringComparison.Ordinal) <
			 source.LastIndexOf("[completedAt] = Construct_Start(", StringComparison.Ordinal));
	}

	/// <summary>
	/// §10's difference between the capture that did not happen and the run of no turns,
	/// kept by the flat form's sentinel where the engine kept it by a missing entry.
	/// </summary>
	[Fact]
	public void A_lowered_optional_capture_still_tells_null_from_empty()
	{
		const string grammar = "Sign : @string = (s: '-')? & 'x' => @(s ?? \"none\")\nparse Sign";

		Assert.DoesNotContain("RentParser", Emit(grammar));
		Assert.Equal("none", Invoke(grammar, "ParseSign", "x").Value);
		Assert.Equal("-", Invoke(grammar, "ParseSign", "-x").Value);
	}

	/// <summary>
	/// The flat form keeps the same promise without the record: the factory call sits
	/// after the whole-input check, so nothing the author wrote runs on a parse that
	/// then fails.
	/// </summary>
	[Fact]
	public void A_lowered_construction_still_runs_only_after_acceptance()
	{
		var source = Emit("Start : @int = digits: ['0'..'9']+ => @int.Parse(digits)\nparse Start");

		Assert.DoesNotContain("ParserEntry.Construct", source);
		Assert.DoesNotContain("RentParser", source);
		Assert.True(
			source.IndexOf("if (p != text.Length)", StringComparison.Ordinal) <
			 source.IndexOf("value = Construct_Start(", StringComparison.Ordinal));
	}

	[Fact]
	public void Construction_runs_only_for_values_reachable_from_the_accepted_derivation()
	{
		const string grammar =
			"Start : @int = value: Value & 'x' => @(value)\n" +
			"             | 'a'                => @(0)\n" +
			"Value : @int = 'a' => @int.Parse(\"not a number\")\n" +
			"parse Start";

		var result = Invoke(grammar, "ParseStart", "a");

		Assert.True(result.Matched);
		Assert.Equal(0, result.Value);
	}

	[Fact]
	public void Captured_rule_values_are_materialized_from_completed_invocations()
	{
		var source = Emit("Inner = letter: 'x'\nStart = inner: Inner\nparse Start");

		Assert.Contains("ParserEntry.RuleCapture", source);
		Assert.Contains("parser.Materialization(entries.Count)", source);
		Assert.DoesNotContain("Recognize_Inner(", source);
		Assert.DoesNotContain("List<Inner>", source);
	}

	[Fact]
	public void Captured_rule_sequences_use_exact_arrays_not_typed_lists()
	{
		var source = Emit("Item = letter: 'x'\nStart = items: Item*\nparse Start");

		Assert.Contains("captured0Count", source);
		Assert.Contains("new global::Grammar.Item[captured0Count]", source);
		Assert.DoesNotContain("List<global::Grammar.Item>", source);
	}

	[Fact]
	public void Declared_sequence_results_use_the_shared_arena_and_exact_arrays()
	{
		const string grammar =
			"Start : Items = Items & eof\n" +
			"Items : @string[] = Item*\n" +
			"Item : @string = value: 'a' => @(value)\n" +
			"parse Start";
		var source = Emit(grammar);
		var result = Invoke(grammar, "ParseStart", "aaa");

		Assert.Contains("ParserArena Entries", source);
		Assert.DoesNotContain("List<ParserEntry>", source);
		Assert.Contains("var items = new string[count];", source);
		Assert.DoesNotContain("Recognize_Start(", source);

		// No longer also `Assert.DoesNotContain("List<string>", source)`: every
		// TryParseX wrapper flattens `Failure.Expected`/`ExpectedMore` into a local
		// `List<string>` once, on an actual overall failure (§11's first-tier
		// diagnostics), unrelated to how a sequence is materialized — the array
		// assertion just above is what actually proves the sequence itself uses no
		// `List<T>`.
		Assert.True(result.Matched);
		Assert.Equal(new[] { "a", "a", "a" }, Assert.IsType<string[]>(result.Value));
	}

	[Fact]
	public void A_deep_fold_is_materialized_iteratively_from_the_shared_arena()
	{
		const int terms = 20_000;
		const string grammar =
			"Start : @int = left: Start & '+' & right: Number => @(left + right)\n" +
			"             | value: Number                    => @(value)\n" +
			"Number : @int = '1' => @(1)\n" +
			"parse Start";
		var source = Emit(grammar);
		var parser = EmittedCode.Compile(source);
		var input = string.Join("+", Enumerable.Repeat("1", terms));
		var match = EmittedCode.Match(parser, "Grammar", "TryParseStart", input);

		Assert.Contains("var hasAccumulated = false;", source);
		Assert.Contains("for (var constructAt = completedAt + 1;", source);
		Assert.DoesNotContain("Recognize_Start(", source);
		Assert.True(match.IsSuccess);
		Assert.Equal(terms, match.Value);
	}

	[Fact]
	public void A_text_capture_guard_reads_the_current_path_from_the_shared_arena()
	{
		const string grammar =
			"Start = (value: \"ab\" & when @(value == \"xy\") | value: \"ab\") & 'c'\n" +
			"parse Start";
		var source = Emit(grammar);
		var parser = EmittedCode.Compile(source);
		var match = EmittedCode.Match(parser, "Grammar", "TryParseStart", "abc");

		// No `parserText`: the condition asks about the capture, so the run around it is
		// never built. What the guard reads out of the arena is the capture and nothing else.
		Assert.Contains("Recognize_DotGram_Guard0(string? value)", source);
		Assert.Contains("candidate.Kind == ParserEntry.Capture", source);
		Assert.DoesNotContain("bool[] _built", source);
		Assert.DoesNotContain("Recognize_Start(", source);
		Assert.True(match.IsSuccess);
	}

	[Fact]
	public void A_typed_capture_guard_materializes_and_reuses_the_captured_value()
	{
		const string grammar =
			"Start : @int = value: Number & when @(value < 3) => @(value)\n" +
			"Number : @int = digit: ['0'..'9'] => @int.Parse(digit)\n" +
			"parse Start";
		var source = Emit(grammar);
		var parser = EmittedCode.Compile(source);

		Assert.Contains("Materialize_DotGram(text, parser, entries);", source);
		Assert.Contains("bool[] _built", source);
		Assert.Contains("parser.Materialized();", source);
		Assert.DoesNotContain("parser.Materialized(entries.Count);", source);
		Assert.DoesNotContain("Recognize_Start(", source);
		Assert.True(EmittedCode.Match(parser, "Grammar", "TryParseStart", "2").IsSuccess);
		Assert.False(EmittedCode.Match(parser, "Grammar", "TryParseStart", "4").IsSuccess);
	}

	[Fact]
	public void A_typed_sequence_guard_receives_every_value_in_grammar_order()
	{
		const string grammar =
			"Start : @int = values: Number+ & when @(values.Length == 2) => @(values[0] * 10 + values[1])\n" +
			"Number : @int = digit: ['0'..'9'] => @int.Parse(digit)\n" +
			"parse Start";
		var parser = EmittedCode.Compile(Emit(grammar));
		var accepted = EmittedCode.Match(parser, "Grammar", "TryParseStart", "12");

		Assert.True(accepted.IsSuccess);
		Assert.Equal(12, accepted.Value);
		Assert.False(EmittedCode.Match(parser, "Grammar", "TryParseStart", "1").IsSuccess);
	}

	[Fact]
	public void A_typed_sequence_guard_includes_recovered_values()
	{
		const string grammar =
			"Start : @int = rows: Row* recover eol => @(0) & when @(rows.Length == 3) & eof => @(rows.Length)\n" +
			"Row : @int = digit: ['0'..'9'] & eol => @int.Parse(digit)\n" +
			"parse Start";
		var parser = EmittedCode.Compile(Emit(grammar));
		var accepted = EmittedCode.Match(parser, "Grammar", "TryParseStart", "1\n3x\n2\n");

		Assert.True(accepted.IsSuccess);
		Assert.Equal(3, accepted.Value);
	}

	[Fact]
	public void A_positive_lookahead_capture_records_the_extent_it_saw()
	{
		var source = Emit("Word = ['a'..'z']+\nStart = seen: ?=Word & Word\nparse Start");

		Assert.Contains("capture lookahead", source);
		Assert.Contains("var seenTo = p;", source);
		Assert.DoesNotContain("Recognize_Start(", source);
	}

	[Fact]
	public void A_negative_lookahead_capture_is_recorded_only_on_its_success_path()
	{
		var source = Emit("Start = seen: ?!'z' & 'a'\nparse Start");

		Assert.Contains("capture negative lookahead", source);
		Assert.DoesNotContain("Recognize_Start(", source);
	}

	[Fact]
	public void Typed_recursive_results_return_through_explicit_frames()
	{
		const int depth = 20_000;
		const string grammar =
			"Start : @int = '(' & inner: Start & ')' => @(inner + 1)\n" +
			"             | 'x'                     => @(1)\n" +
			"parse Start";

		var source = Emit(grammar);
		var parser = EmittedCode.Compile(source);
		var input = new string('(', depth) + "x" + new string(')', depth);
		var match = EmittedCode.Match(parser, "Grammar", "TryParseStart", input);

		Assert.True(match.IsSuccess);
		Assert.Equal(depth + 1, match.Value);
	}

	[Fact]
	public void Recursive_frames_restore_text_and_typed_captures_together()
	{
		const int depth = 10_000;
		const string grammar =
			"Start : @string = '(' & inner: Start & ')' => @(inner)\n" +
			"                | value: 'x'              => @(value)\n" +
			"parse Start";

		var parser = EmittedCode.Compile(Emit(grammar));
		var input = new string('(', depth) + "x" + new string(')', depth);
		var match = EmittedCode.Match(parser, "Grammar", "TryParseStart", input);

		Assert.True(match.IsSuccess);
		Assert.Equal("x", match.Value);
	}

	[Fact]
	public void An_uncaptured_recursive_value_needs_no_return_slot()
	{
		const string grammar =
			"Start : @int = '(' & Start & ')' => @(1)\n" +
			"             | 'x'                 => @(1)\n" +
			"parse Start";

		var parser = EmittedCode.Compile(Emit(grammar));
		var match = EmittedCode.Match(parser, "Grammar", "TryParseStart", "(((x)))");

		Assert.True(match.IsSuccess);
		Assert.Equal(1, match.Value);
	}

	[Fact]
	public void Sequence_captures_use_one_list_with_an_invocation_segment()
	{
		const int depth = 5_000;
		const string grammar =
			"Start : @int = '(' & (item: Item)* & child: Start? & ')' => @(item.Length + (child ?? 0))\n" +
			"             | 'x'                                       => @(0)\n" +
			"Item = 'a'\n" +
			"parse Start";

		var parser = EmittedCode.Compile(Emit(grammar));
		var input = string.Concat(Enumerable.Repeat("(a", depth)) + "x" + new string(')', depth);
		var match = EmittedCode.Match(parser, "Grammar", "TryParseStart", input);

		Assert.True(match.IsSuccess);
		Assert.Equal(depth, match.Value);
	}

	[Theory]
	[InlineData("cat", true)]
	[InlineData("dog", true)]
	[InlineData("cow", false)]
	public void Ordered_choice(string input, bool expected) =>
		Assert.Equal(expected, Run("""Start = "cat" | "dog" """, input).Matched);

	[Fact]
	public void Ordered_choice_backtracks_across_a_shared_prefix()
	{
		// The case a commit point would have broken: both alternatives begin with the
		// same rule and diverge only after it (docs/syntax.md §11).
		var grammar = """
			Start = Call | Index
			Call  = Name & '(' & ')'
			Index = Name & '[' & ']'
			Name  = ['a'..'z']+
			""";

		Assert.True(Run(grammar, "foo()").Matched);
		Assert.True(Run(grammar, "foo[]").Matched);
		Assert.False(Run(grammar, "foo{}").Matched);
	}

	[Theory]
	[InlineData("ab",   true)]
	[InlineData("b",    true)]
	[InlineData("aab",  false)]
	public void Optional(string input, bool expected) =>
		Assert.Equal(expected, Run("Start = 'a'? & 'b'", input).Matched);

	[Theory]
	[InlineData("aaa", true)]
	[InlineData("aa",  false)]
	[InlineData("aaaa", false)]
	public void Counted_repetition(string input, bool expected) =>
		Assert.Equal(expected, Run("Start = 'a'{3}", input).Matched);

	[Theory]
	[InlineData("ab", true)]
	[InlineData("ax", false)]
	public void Lookahead_consumes_nothing(string input, bool expected) =>
		Assert.Equal(expected, Run("Start = 'a' & ?='b' & 'b'", input).Matched);

	[Fact]
	public void A_lookahead_runs_recursive_rules_on_the_same_arena()
	{
		const int depth = 10_000;
		const string grammar =
			"Start = ?=Value & Value\n" +
			"Value = '(' & Value & ')' | 'x'\n" +
			"parse Start";
		var source = Emit(grammar);
		var parser = EmittedCode.Compile(source);
		var input = new string('(', depth) + "x" + new string(')', depth);

		Assert.Contains("ParserEntry.Lookahead", source);
		Assert.True(EmittedCode.Match(parser, "Grammar", "TryParseStart", input).IsSuccess);
	}

	[Fact]
	public void The_value_is_the_matched_text() =>
		Assert.Equal("hello", Run("Start = ['a'..'z']+", "hello").Value);

	[Fact]
	public void Rules_call_each_other()
	{
		var grammar = """
			Start  = Digits & '-' & Digits
			Digits = ['0'..'9']+
			""";

		Assert.True (Run(grammar, "12-34").Matched);
		Assert.False(Run(grammar, "12-").Matched);
	}

	// ── The standard library (§3.1) ──────────────────────────────────────────────

	[Theory]
	[InlineData("a\n",   true)]
	[InlineData("a\r\n", true)]
	[InlineData("a\r",   true)]
	[InlineData("a",     false)]
	public void Eol(string input, bool expected) =>
		Assert.Equal(expected, Run("Start = 'a' & eol", input).Matched);

	[Theory]
	[InlineData("ab",  true)]
	[InlineData("a\n", true)]
	[InlineData("a",   false)]
	public void Any_is_one_of_whatever(string input, bool expected) =>
		Assert.Equal(expected, Run("Start = 'a' & any", input).Matched);

	[Fact]
	public void Eof_consumes_nothing_and_is_only_true_at_the_end() =>
		Assert.True(Run("Start = 'a' & eof", "a").Matched);

	[Fact]
	public void None_matches_the_empty_sequence() =>
		Assert.True(Run("Start = 'a' & none", "a").Matched);

	// ── Publication (§6) ─────────────────────────────────────────────────────────

	const string Digits = """
		Start = ['0'..'9']+

		""";

	/// <summary>Compiles the emitted source and walks a <c>find</c>.</summary>
	static object?[] Occurrences(string grammar, string method, string input) =>
		EmittedCode.Found(EmittedCode.Compile(Emit(grammar)), "Grammar", method, input);

	[Fact]
	public void Parse_requires_the_input_to_end() =>
		Assert.False(Run("Start = ['0'..'9']+", "12ab").Matched);

	[Fact]
	public void Find_takes_every_occurrence() =>
		Assert.Equal(
			new object?[] { "12", "34" },
			Occurrences(Digits + "find Start", "FindStart", "ab12cd34"));

	[Fact]
	public void And_the_first_one_is_LINQ_s_business_rather_than_a_directive_s() =>
		Assert.Equal("12", Occurrences(Digits + "find Start", "FindStart", "ab12cd34")[0]);

	[Fact]
	public void Find_yields_nothing_rather_than_matching_nothing() =>
		Assert.Empty(Occurrences(Digits + "find Start", "FindStart", "abc"));

	[Fact]
	public void As_renames_the_pair() =>
		Assert.True(Invoke(Digits + "parse Start as ReadDigits", "ReadDigits", "12").Matched);

	[Fact]
	public void One_grammar_can_publish_the_same_rule_both_ways()
	{
		var grammar = Digits + """
			parse Start
			find Start
			""";

		Assert.True(Invoke(grammar, "ParseStart", "12").Matched);
		Assert.Single(Occurrences(grammar, "FindStart", "ab12"));
	}

	[Fact]
	public void Parse_and_find_share_one_automaton()
	{
		var source = Emit(Digits + """
			parse Start
			find Start
			""");

		Assert.Equal(1, source.Split("static int Recognize_DotGram(", StringSplitOptions.None).Length - 1);
		Assert.Contains("Recognize_DotGram(text, pos", source, StringComparison.Ordinal);
	}

	[Fact]
	public void Two_directives_wanting_one_name_is_a_diagnostic()
	{
		var result = GramCompiler.Compile("""
			Start = 'a'
			parse Start
			parse Start
			""");

		Assert.Equal(GrammarBinder.DuplicatePublication, Assert.Single(result.Diagnostics).Id);
		Assert.Empty(result.Sources);
	}

	/// <summary>
	/// A rule that named its own type gets a method to build it with, and the captures
	/// are its parameters — which is what lets a <c>=&gt;</c> use their names without
	/// dodging every local the recognizer has.
	/// </summary>
	[Fact]
	public void A_declared_type_is_built_by_a_method_of_its_own()
	{
		var source = Emit("""
			@using System.Globalization;

			Start : @int = digits: ['0'..'9']+ => @int.Parse(digits, @CultureInfo.InvariantCulture)
			parse Start
			""");

		Assert.Contains("using System.Globalization;", source);
		Assert.Contains(
			"static int Construct_Start(string digits) =>",
			source);
		Assert.Contains("int.Parse(digits, CultureInfo.InvariantCulture);", source);

		// And the publication hands back that type rather than the matched text.
		Assert.Contains("public static int ParseStart(string input)", source);
	}

	[Fact]
	public void A_broken_grammar_emits_nothing()
	{
		var result = GramCompiler.Compile("Start = Missing");

		Assert.NotEmpty(result.Diagnostics);
		Assert.Empty(result.Sources);
	}

	[Fact]
	public void Blocks_written_in_one_piece_land_at_the_depth_they_are_written_at()
	{
		// `Match<T>`, `Failure` and the parser runtime are raw string literals in the emitter, which
		// carry whatever line endings the emitter's own file was saved with. Read as one
		// line — which is what happens when those endings are not the ones the writer
		// splits on — they arrive indented once and flat after that. The code still
		// compiles, so nothing but this notices.
		// In a namespace, so the depth every one of them sits at is two and not "whatever
		// the class happened to be nested at". Deliberately not silent — the shorter literal
		// is written first, so it takes the position wherever the longer would have and the
		// choice needs a way back, which is what asks for the arena and the Parser below.
		// The other order lowers, and the whole engine, Parser included, is what a silent
		// grammar no longer pays for.
		var source = Assert.Single(GramCompiler.Compile(
			"Start = (\"a\" | \"ab\") & 'c'\nparse Start",
			new GramCompilerOptions { ClassName = "Grammar", Namespace = "My.App" }).Sources).Text;

		Assert.Contains("\t\tpublic readonly struct Match<T>\r\n\t\t{\r\n\t\t\t/// <summary>", source);
		Assert.Contains("\t\tstruct Failure\r\n\t\t{\r\n\t\t\t/// <summary>",                    source);
		Assert.Contains("\t\tprivate sealed class Parser\r\n\t\t{\r\n", source);
	}

	[Fact]
	public void Only_a_rule_of_strengths_takes_one()
	{
		var source = Emit("""
			E : @int = left: E & '+' & right: E << 1 => @(left + right)
			         | digits: D+                    => @int.Parse(digits)
			D        = ['0'..'9']
			parse E
			""");

		// The published climbing entry takes the strength. The other rule is a shared label
		// in the same automaton, so it needs neither a method nor a strength parameter.
		Assert.Contains("static int Recognize_E_Whole(global::System.ReadOnlySpan<char> text, int pos, int power, ref Failure failure, out int value)", source);
		Assert.DoesNotContain("static int Recognize_D(", source);
		Assert.Contains("int rootRule, int initialPower, bool whole", source);
		Assert.Contains("if (1 < power) { expected = null; goto Fail; }", source);

		// `<< 1` reads its right operand at 2 — one tighter, so a `+` cannot appear in it.
		Assert.Contains("power = 2;", source);

		// And publication asks at 0, which admits everything.
		Assert.Contains("Recognize_E_Whole(text, 0, 0, ref failure, out var recognized);", source);
	}

	// ── Streaming (§6.3) ─────────────────────────────────────────────────────────

	/// <summary>What the emitted window holds before it has to grow.</summary>
	/// <remarks>
	/// The generator's own constant, and the tests below are about what happens at its
	/// edge — so they read it rather than restating it, and a change to it moves them.
	/// </remarks>
	const int Buffer = 4096;

	/// <summary>An input whose only occurrence straddles the end of the first window.</summary>
	static string AcrossTheBoundary(string occurrence, int before = 6) =>
		new string('x', Buffer - before) + occurrence + new string('x', 16);

	[Fact]
	public void A_find_over_a_reader_takes_the_same_occurrences()
	{
		var assembly = EmittedCode.Compile(Emit(Digits + "find Start"));

		Assert.Equal(
			EmittedCode.Found(assembly, "Grammar", "FindStart", "ab12cd34"),
			EmittedCode.Found(assembly, "Grammar", "FindStart", "ab12cd34", typeof(TextReader)));
	}

	[Fact]
	public void An_occurrence_that_straddles_the_buffer_is_one_occurrence()
	{
		// The defect a windowed parse has if it believes what a full buffer tells it: the
		// digits stop at the edge of what is held, which looks exactly like the input
		// stopping. Read naively this is "123456" and "7890" — two occurrences where the
		// input has one.
		var input = AcrossTheBoundary("1234567890");

		Assert.Equal(
			new object?[] { "1234567890" },
			EmittedCode.Found(
				EmittedCode.Compile(Emit(Digits + "find Start")),
				"Grammar",
				"FindStart",
				input,
				typeof(TextReader)));
	}

	[Fact]
	public void And_is_reported_at_its_place_in_the_whole_input()
	{
		// The offset is into the input and not into the buffer, which is the whole reason
		// §6.3 makes it a `long`. This one is past the first window, so a position measured
		// from the buffer would be a small number that means nothing.
		Assert.Equal(
			[Buffer - 6],
			EmittedCode.FoundAt(
				EmittedCode.Compile(Emit(Digits + "find Start")),
				"Grammar",
				"FindStart",
				AcrossTheBoundary("1234567890"),
				typeof(TextReader)));
	}

	[Fact]
	public void An_occurrence_longer_than_the_window_grows_it()
	{
		// The analysis bounds retention by the grammar, not by a constant: an element that
		// does not fit is a long record rather than a runaway, and the window grows for it.
		var digits = new string('7', Buffer * 3);

		Assert.Equal(
			new object?[] { digits },
			EmittedCode.Found(
				EmittedCode.Compile(Emit(Digits + "find Start")),
				"Grammar",
				"FindStart",
				"ab" + digits + "cd",
				typeof(TextReader)));
	}

	[Fact]
	public void A_reader_that_gives_nothing_yields_nothing() =>
		Assert.Empty(
			EmittedCode.Found(
				EmittedCode.Compile(Emit(Digits + "find Start")),
				"Grammar",
				"FindStart",
				"",
				typeof(TextReader)));

	[Fact]
	public void Only_find_gets_a_reader_so_far()
	{
		// `parse` needs the decomposition of Retention.PlanFor — what lets its window move
		// is a committed repetition inside it — and that is not built yet. Pinned so that
		// the day it is, this test is what says so.
		var source = Emit(Digits + "parse Start\nfind Start");

		Assert.Contains(
			"IEnumerable<Match<string>> FindStart(global::System.IO.TextReader input)",
			source,
			StringComparison.Ordinal);

		Assert.DoesNotContain("TryParseStart(global::System.IO.TextReader", source, StringComparison.Ordinal);
	}

	[Fact]
	public void A_rule_that_would_hold_the_input_gets_no_reader_overload()
	{
		// `any*` reads to the end of the file and can give any of it back, so the window
		// would be the input and streaming would be a word rather than a property (§6.3).
		var source = Emit("Start = any* & 'z'\nfind Start");

		Assert.DoesNotContain("TextReader", source, StringComparison.Ordinal);
		Assert.DoesNotContain("class Window", source, StringComparison.Ordinal);
	}

	[Fact]
	public void A_construction_asking_for_the_whole_input_gets_no_reader_overload()
	{
		// `parserInput` is the input, and a stream is what having no input to hand over is
		// called: a window holds the part being read, and the part it holds is not what a
		// construction asking for this means (§6.3, §8.2). Refused rather than handed the
		// window's own text, which would be one name meaning two different things and the
		// wrong one silently.
		var source = Emit(
			"Start : @Held = 'a'+ => @(new Held(parserInput))\nfind Start");

		Assert.DoesNotContain("TextReader", source, StringComparison.Ordinal);
	}

	[Fact]
	public void And_says_which_rule_asked_for_it()
	{
		var result = GramCompiler.Compile(
			"Start : @Held = 'a'+ => @(new Held(parserInput))\nfind Start",
			new GramCompilerOptions
			{
				ClassName     = "Grammar",
				CSharpScanner = RoslynCSharpScanner.Instance,
			});

		var told = Assert.Single(result.Diagnostics);

		Assert.Equal(Retention.NotStreamable, told.Id);
		Assert.Equal(GramSeverity.Info,       told.Severity);
		Assert.Contains("parserInput", told.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void And_is_told_so_where_it_asked()
	{
		// The alternative is what the author actually meets: a call that does not bind,
		// naming neither the rule responsible nor anything they could do about it.
		var result = GramCompiler.Compile(
			"Start = any* & 'z'\nfind Start",
			new GramCompilerOptions { ClassName = "Grammar" });

		var told = Assert.Single(result.Diagnostics);

		Assert.Equal(Retention.NotStreamable, told.Id);
		Assert.Equal(GramSeverity.Info,       told.Severity);

		// The innermost part that does not fit, rather than the whole body it sits in.
		Assert.Contains("any*",   told.Message, StringComparison.Ordinal);
		Assert.Contains("§6.3",   told.Message, StringComparison.Ordinal);
		Assert.Contains("FindStart", told.Message, StringComparison.Ordinal);

		// Information, so the parser is still generated: what it did not get is an
		// overload, not a method.
		Assert.NotEmpty(result.Sources);
	}

	[Fact]
	public void A_rule_that_streams_is_told_nothing() =>
		Assert.Empty(GramCompiler.Compile(
			"Start = ['0'..'9']+\nfind Start",
			new GramCompilerOptions { ClassName = "Grammar" }).Diagnostics);

	[Fact]
	public void And_neither_is_a_parse_that_has_no_reader_overload_yet() =>
		// The reason there is a fact about this compiler rather than about the grammar in
		// front of it. Saying it on every build of every grammar would be noise, and
		// docs/status.md is where it belongs.
		Assert.Empty(GramCompiler.Compile(
			"Start = any* & 'z'\nparse Start",
			new GramCompilerOptions { ClassName = "Grammar" }).Diagnostics);

	[Fact]
	public void The_class_goes_where_it_was_asked_to()
	{
		var source = GramCompiler.Compile(
			"Start = 'a'",
			new GramCompilerOptions { ClassName = "Feed", Namespace = "My.App" });

		Assert.Contains("namespace My.App",  Assert.Single(source.Sources).Text);
		Assert.Contains("partial class Feed", source.Sources[0].Text);
	}

	// ── What was expected there (§11's first tier) ───────────────────────────────

	[Fact]
	public void A_terminal_failure_site_records_its_own_display_before_jumping()
	{
		// Not pinned to a specific Recognize_DotGram_ExpectedN index: which number a
		// given occurrence gets depends on how many terminals were visited compiling the
		// rest of the grammar first, which is an implementation detail this test has no
		// business caring about.
		var source = Emit("Start = 'x'\nparse Start");

		Assert.Matches(
			@"static readonly string\[\] Recognize_DotGram_Expected\d+ = \{ ""'x'"" \};", source);
		Assert.Matches(@"expected = Recognize_DotGram_Expected\d+;", source);
	}

	[Fact]
	public void A_non_terminal_failure_site_clears_it_instead()
	{
		var source = Emit("Start = 'x' & when @(false)\nparse Start");

		Assert.Contains("if (!Recognize_DotGram_Guard0()) { expected = null; goto Fail; }", source);
	}

	[Fact]
	public void The_Fail_label_replaces_on_a_new_furthest_position_and_appends_on_a_tie()
	{
		// A capture keeps this off the flat/no-arena path (Silent has no case for
		// Node.Capture), so the shared Fail: label — the one with a max/tie to make,
		// unlike the flat path's single, unconditional attempt — is the one compiled.
		var source = Emit("Start = a: 'x'\nparse Start");

		// A new furthest position costs a reference assignment, not an allocation —
		// the whole point of the split (Support.cs's own ExpectedField remarks).
		Assert.Contains("failure.Expected = expected;", source);
		Assert.Contains("failure.ExpectedMore = null;", source);

		// A tie allocates, but only the list of arrays, and only on the tie itself.
		Assert.Contains(
			"(failure.ExpectedMore ??= new global::System.Collections.Generic.List<string[]>())" +
			".Add(expected);",
			source);
	}

	// ── Case-insensitive literals ─────────────────────────────────────────────────

	[Fact]
	public void A_case_insensitive_literal_compares_folded()
	{
		// ToUpperInvariant on both sides, so one comparison shape covers every character
		// — the constant side is folded once, at generation time.
		var source = Emit("""Start = "http"i""");

		Assert.Matches(
			@"static readonly string\[\] Recognize_DotGram_Expected\d+ = \{ ""\\""http\\""i"" \};", source);
		Assert.Contains("global::System.Char.ToUpperInvariant(text[p]) != 'H'", source);
		Assert.Contains("global::System.Char.ToUpperInvariant(text[p + 3]) != 'P'", source);
	}

	[Fact]
	public void A_case_insensitive_run_does_not_join_the_shared_prefix_optimization()
	{
		// "https" and "httpx" share a prefix and merge into one CompileLiterals block;
		// "http"i sits outside that run entirely, on its own case-folded site.
		var source = Emit("""Start = "http"i | "https" | "httpx" """);

		Assert.Matches(
			@"static readonly string\[\] Recognize_DotGram_Expected\d+ = " +
			@"\{ ""\\""https\\"""", ""\\""httpx\\"""" \};",
			source);
		Assert.Contains("AsSpan(\"http\")", source);
		Assert.Contains("global::System.Char.ToUpperInvariant(text[p]) != 'H'", source);
	}

	// ── A rule that scans ──────────────────────────────────────────────────

	/// <summary>
	/// An atomic, record-free rule compiles as a plain method, and its calls as calls.
	/// </summary>
	[Fact]
	public void An_atomic_recordless_rule_compiles_as_a_scanner()
	{
		var source = Emit(
			"trivia = { (' ' | \"//\" & [^ '\\n']*)* }" + '\n' +
			"Start = 'a' & 'b'" + '\n' + "parse Start");

		Assert.Contains("static int Scan_trivia(", source, StringComparison.Ordinal);
		Assert.Contains("p = Scan_trivia(text, p);", source, StringComparison.Ordinal);

		// The seam pays a call, not an arena cycle: no atomic entry anywhere.
		Assert.DoesNotContain("ParserEntry.Atomic, 0", source, StringComparison.Ordinal);
	}

	// ── A capture that can open before it closes ─────────────────────────

	/// <summary>
	/// A text capture in a rule that can reach itself keeps its start in the arena.
	/// </summary>
	/// <remarks>
	/// A variable is right for as long as nothing opens the same capture between the
	/// opening and the close. A rule that can reach itself does exactly that, and the half
	/// no variable can get right is the failed inner attempt: backtracking puts the arena
	/// back and nothing else, so the start left behind is a position the parse has already
	/// given up. The outer close then reads it and the span comes back inside out — which
	/// is an exception out of the materializer, not a parse that answers wrongly.
	/// </remarks>
	[Fact]
	public void A_capture_that_can_reopen_keeps_its_start_where_backtracking_can_reach_it()
	{
		var source = Emit("Start = text: ('a' & Start?)");

		Assert.Contains("ParserEntry.Capture, 0, p, call, atomic, repeat, lookahead, -1", source);
		Assert.DoesNotContain("capture0 = p;", source, StringComparison.Ordinal);
		Assert.DoesNotContain("var capture0 = 0;", source, StringComparison.Ordinal);
	}

	/// <summary>The same rule, not recursive: nothing can reopen it, and the variable stays.</summary>
	[Fact]
	public void A_capture_that_cannot_reopen_still_keeps_its_start_in_a_variable()
	{
		var source = Emit("Start = text: ('a' & 'b')");

		Assert.Contains("capture0 = p;", source);
		Assert.DoesNotContain("ParserEntry.Capture, 0, p, call, atomic, repeat, lookahead, -1", source);
	}

	// ── A literal a later alternative continues ─────────────────────────────────

	[Fact]
	public void The_way_back_to_a_longer_alternative_is_written_past_what_already_matched()
	{
		var source = Emit("Start = QhttpQ | QhttpsQNparse Start".Replace("Q", "\"").Replace("N", "\n"));

		// The order of these two lines is the whole optimization. An arena entry records
		// the position as it stands, so pushing after the advance means what resumes there
		// resumes past the four characters `"http"` matched — and the state it names
		// compares the fifth alone. Pushed before, it would resume at the start and compare
		// `"https"` from its first character.
		Assert.Matches(
			@"p \+= 4;\s*entries\.Add\(new ParserEntry\(ParserEntry\.Choice,", source);

		Assert.Matches(@"text\[p\] == 's'\)\s*\{\s*p \+= 1;", source);

		// And the longer text is never compared as a text at all: it is not in the
		// falling-through chain, because it begins with one tested above it, and where
		// the way back names it only the one character it adds is read.
		Assert.DoesNotContain("AsSpan(QhttpsQ)".Replace("Q", "\""), source, StringComparison.Ordinal);
	}

	// ── Position sharpening: the character that failed, not the operand's start ──

	[Fact]
	public void A_terminal_failure_advances_p_to_the_character_that_did_not_fit()
	{
		var source = Emit("""Start = "abcd" """);

		// The literal is one comparison, so where it went wrong is worked out afterwards,
		// inside the branch that comparison already failed. The first character needs no
		// adjustment — p is already there — and the last needs no test: if every earlier
		// one matched and the whole did not, it is the one.
		Assert.Contains(
			"if (!global::System.MemoryExtensions.SequenceEqual(" +
			"text.Slice(p, 4), global::System.MemoryExtensions.AsSpan(\"abcd\")))",
			source, StringComparison.Ordinal);

		Assert.Matches(@"if \(text\[p\] == 'a'\)", source);
		Assert.Matches(@"if \(text\[p \+ 1\] != 'b'\)\s*p \+= 1;", source);
		Assert.Matches(@"else if \(text\[p \+ 2\] != 'c'\)\s*p \+= 2;", source);
		Assert.Matches(@"else\s*p \+= 3;", source);
	}
}
