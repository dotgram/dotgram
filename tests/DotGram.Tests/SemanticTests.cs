using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Small grammars run against real input, and grammars that must be refused.
/// </summary>
/// <remarks>
/// A corpus rather than a test per feature. What breaks a grammar engine is almost
/// never <c>X*</c> on its own — it is <c>X*</c> next to something else, or a construct
/// that parses and then quietly means nothing. Several of these were written from a
/// review that found exactly that: syntax accepted at the front of the pipeline and
/// lost somewhere before the back of it.
/// </remarks>
public sealed class SemanticTests
{
	/// <summary>Compiles, compiles the result, and runs it. Fails if the grammar does not compile.</summary>
	static bool Matches(string grammar, string input) => Parsed(grammar, input).IsSuccess;

	static (bool IsSuccess, object? Value, string? Error, long Position) Parsed(string grammar, string input)
	{
		var result = Compile(grammar + "\nparse Start");

		Assert.Empty(result.Diagnostics);

		return EmittedCode.Match(
			EmittedCode.Compile(result.Sources[0].Text), "Grammar", "TryParseStart", input);
	}

	static GramCompilation Compile(string grammar) => GramCompiler.Compile(
		grammar,
		new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

	/// <summary>The one diagnostic a grammar must be refused with.</summary>
	// ── One mistake, one message about it ────────────────────────────────────────

	/// <summary>A feed with a single stray character in the middle of one rule.</summary>
	const string OneStrayCharacter =
		"Row     = \"R\" & '|' & name: Text & eol\n" +
		"Text    = [^ '|']+\n" +
		"Feed    = header: Header & rows: Row* & eof\n" +
		"Header  = \"H\" ~ '|' & date: Digit{4} & eol\n" +
		"Digit   = ['0'..'9']\n" +
		"parse Feed";

	[Fact]
	public void A_stray_character_is_not_reported_seven_times()
	{
		// It used to be. The lexer said what was actually wrong, and then the parser said
		// it again about the same character, twice more about where it ended up, and the
		// binder and normalizer described the tree the parser had guessed at — including
		// `No rule, parameter or capture named ''`, which is about nothing at all.
		var reported = Compile(OneStrayCharacter).Diagnostics;

		// The first one is the one that says what happened; what follows is the parser
		// finding its feet again, which is worth seeing and is not the same message.
		Assert.Equal(GramLexer.UnexpectedCharacter, reported[0].Id);
		Assert.Equal(3, reported.Count);
		Assert.DoesNotContain(reported, diagnostic => diagnostic.Id.StartsWith("GRAM3", StringComparison.Ordinal));
		Assert.DoesNotContain(reported, diagnostic => diagnostic.Id.StartsWith("GRAM4", StringComparison.Ordinal));
	}

	[Fact]
	public void But_a_rule_the_parser_could_read_is_still_checked()
	{
		// The silence is scoped to the declaration that broke, not to the file. One rule
		// being unreadable must not hide what is wrong with the one below it
		// (implementation.md §0) — here `Other` is perfectly readable and names something
		// that does not exist.
		var reported = Compile(OneStrayCharacter + "\nOther = Missing").Diagnostics;

		Assert.Contains(reported, diagnostic => diagnostic.Id == DotGram.Grammar.Binding.GrammarBinder.UndefinedName);
	}

	// ── Lookahead (§3.4, §3.6) ───────────────────────────────────────────────────

	[Fact]
	public void A_lookahead_produces_what_it_saw()
	{
		// §3.4: "?=X is a recognizer in its own right and produces X's value (which can be
		// captured) without moving the input." It parsed only the other way round —
		// `?=n: X` — and the capture came back empty, because the extent was measured from
		// a position the lookahead had deliberately not moved.
		Assert.Equal(
			"ab",
			Parsed("Start : @string = seen: ?=Word & Word => @(seen)\nWord = ['a'..'z']+", "ab")
				.Value);
	}

	[Fact]
	public void And_a_negative_one_saw_nothing() =>
		// §3.4: "?!X produces nothing" — it succeeded because what it looked for was not
		// there, so there is nothing to have seen.
		Assert.Equal(
			"",
			Parsed("Start : @string = seen: ?!'z' & Word => @(seen)\nWord = ['a'..'z']+", "ab")
				.Value);

	[Fact]
	public void And_the_specification_example_works() =>
		// §3.6, written out: look ahead, name what was seen, ask a question of it, then
		// read it for real.
		Assert.True(Matches(
			"Start = n: ?=Word & where @(n.Length < 3) & Word\nWord = ['a'..'z']+",
			"ab"));

	[Fact]
	public void And_the_question_is_asked_of_what_was_seen() =>
		// The guard is what makes the capture worth having, so it has to be able to say no.
		Assert.False(Matches(
			"Start = n: ?=Word & where @(n.Length < 3) & Word\nWord = ['a'..'z']+",
			"abcd"));

	// ── The extent a rule matched (§4.1 case 4) ──────────────────────────────────

	[Fact]
	public void A_rule_may_say_out_loud_that_its_result_is_the_text()
	{
		// §4.1 case 4: with no `=>` and no captures, "the result is the matched extent:
		// string gives the text". Declaring that was refused, with a message about
		// matching captures to a constructor — of which there were none.
		Assert.Equal("ab", Parsed("Start : @string = ['a'..'z']+", "ab").Value);

		// Which is what the same rule without a type has always done.
		Assert.Equal("ab", Parsed("Start = ['a'..'z']+", "ab").Value);
	}

	[Fact]
	public void And_any_other_type_still_has_to_be_built()
	{
		// `SourceSpan` is the other half of case 4 and is not built. The message says so
		// now, and says what to do instead — it used to talk about constructors.
		var reported = Compile("Start : @DotGram.SourceSpan = ['a'..'z']+\nparse Start").Diagnostics;

		Assert.Equal(GrammarNormalizer.UnbuiltConstruction, Assert.Single(reported).Id);
		Assert.Contains("§4.1 case 4", reported[0].Message, StringComparison.Ordinal);
	}

	// ── Publication (§6) ─────────────────────────────────────────────────────────

	/// <summary>Compiles a grammar and calls one of its published methods.</summary>
	static object? Published(string grammar, string method, string input)
	{
		var result = Compile(grammar);

		Assert.Empty(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text)
			.GetType("Grammar")!
			.GetMethod(method, [typeof(string)])!
			.Invoke(null, [input]);
	}

	[Fact]
	public void Either_directive_may_be_renamed() =>
		// §6: `as` is on both, and only `parse as` had a test.
		Assert.Single(
			(System.Collections.IEnumerable)Published(
				"Word = ['a'..'z']+\nfind Word as AllWords", "AllWords", "ab")!);

	[Fact]
	public void A_rule_in_a_scope_can_be_published() =>
		// §5 and §6 together: the directive reaches into a scope by the qualified name,
		// and the method is named after the rule rather than after the path to it.
		Assert.Equal(
			"ab",
			Published("scope Inner { Word = ['a'..'z']+ }\nparse Inner.Word", "ParseWord", "ab"));

	[Fact]
	public void A_find_of_a_rule_that_matches_nothing_ends()
	{
		// `['a'..'z']*` matches the empty string everywhere, so a `find` that took the
		// match and did not move would answer for ever. It moves.
		var found = (System.Collections.IEnumerable)Published(
			"Maybe = ['a'..'z']*\nfind Maybe", "FindMaybe", "..")!;

		Assert.Equal(3, found.Cast<object>().Count());
	}

	static void Refused(string id, string grammar)
	{
		var diagnostics = Compile(grammar).Diagnostics;

		Assert.True(
			diagnostics.Any(diagnostic => diagnostic.Id == id),
			$"Expected {id}; got " + (diagnostics.Count == 0
				? "no diagnostics at all"
				: string.Join(", ", diagnostics.Select(diagnostic => diagnostic.ToString()))));
	}

	// ── Parameterized rules (§4.2) ───────────────────────────────────────────────

	const string Listing =
		"List(item, sep) = item & (sep & item)*\n" +
		"Word  = ['a'..'z']+\n" +
		"Comma = ','\n" +
		"Semi  = ';'\n";

	[Fact]
	public void A_rule_may_take_another_rule_as_a_parameter() =>
		// §4.2. A parameter is a compile-time thing entirely: the call becomes a rule of
		// its own with `item` and `sep` replaced by what was passed, so nothing downstream
		// ever meets a parameter and nothing is dispatched at run time.
		Assert.True(Matches(Listing + "Start = List(Word, Comma)", "ab,cd,ef"));

	[Fact]
	public void And_the_same_rule_twice_with_different_arguments() =>
		// Two specializations of one rule, side by side in one grammar, which is the whole
		// point of writing `List` once.
		Assert.True(Matches(
			Listing + "Start = List(Word, Comma) & ' ' & List(Word, Semi)",
			"ab,cd ef;gh"));

	[Fact]
	public void And_what_it_was_given_still_has_to_match() =>
		Assert.False(Matches(Listing + "Start = List(Word, Comma)", "ab;cd"));

	[Fact]
	public void The_same_arguments_twice_are_one_rule()
	{
		// Keyed by what the arguments lower to, so a grammar naming the same specialization
		// in two places gets one recognizer rather than two identical ones.
		var source = Compile(
			Listing + "Start = List(Word, Comma) & ' ' & List(Word, Comma)\nparse Start")
			.Sources[0].Text;

		Assert.Equal(
			1,
			source.Split("static int Recognize_List_Word_Comma(").Length - 1);
	}

	[Fact]
	public void An_argument_may_be_anything_that_recognizes()
	{
		// Not only a rule: what a parameter stands for is a piece of grammar, so a
		// character class or a literal passed in place of one works the same way.
		Assert.True(Matches(
			"Padded(item, pad) = pad* & item & pad*\nWord = ['a'..'z']+\nStart = Padded(Word, ' ')",
			"  ab  "));
	}

	[Fact]
	public void A_sequence_result_may_name_a_parameter()
	{
		// §4.2: `: item[]` is a sequence of whatever the argument produces. There are no
		// type parameters in the language and none are needed — a specialization has one
		// concrete argument, so its element type is a concrete answer.
		var built = Built(
			"Many(item) : item[] = item*\n" +
			"Word : @string = text: ['a'..'z']+ & ',' => @(text)\n" +
			"Start = words: Many(Word)",
			"ab,cd,");

		Assert.Equal(["ab", "cd"], (string[])Read(built, "Words")!);
	}

	[Fact]
	public void The_scalar_form_of_it_is_refused_and_says_which_form_is_built()
	{
		// `: item` alone needs the argument's own value handed out as the rule's, which is
		// a second mechanism and is not built. Said where the rule is declared, and naming
		// the form that does work.
		var reported = Compile(
			"Lex(item) : item = ' '* & item\n" +
			"Word : @string = ['a'..'z']+ => @(parserText)\n" +
			"Start = Lex(Word)\n" +
			"parse Start").Diagnostics;

		Assert.Equal(GrammarNormalizer.UnbuiltRuleType, Assert.Single(reported).Id);
		Assert.Contains("item[]", Assert.Single(reported).Message, StringComparison.Ordinal);
	}

	[Fact]
	public void An_argument_may_also_be_a_number()
	{
		// §4.2's other kind of argument. A count may name a parameter, and the number the
		// call passed is substituted into the quantifier — so `Digits(4)` is a rule that
		// takes exactly four.
		const string counted = "Digits(n) = ['0'..'9']{n}\nStart = Digits(4)";

		Assert.True (Matches(counted, "2026"));
		Assert.False(Matches(counted, "202"));
		Assert.False(Matches(counted, "20268"));
	}

	[Fact]
	public void And_two_counts_are_two_rules() =>
		// One rule written once, two lengths asked of it, side by side.
		Assert.True(Matches(
			"Digits(n) = ['0'..'9']{n}\nStart = Digits(4) & '-' & Digits(2)",
			"2026-08"));

	[Fact]
	public void A_count_passed_on_is_still_a_count() =>
		// The argument names the caller's own parameter rather than a number, so what is
		// passed through is what the outer call was given.
		Assert.True(Matches(
			"Digits(n) = ['0'..'9']{n}\nPair(n) = Digits(n) & '-' & Digits(n)\nStart = Pair(2)",
			"20-26"));

	[Fact]
	public void A_count_naming_a_parameter_that_was_not_given_one_is_refused() =>
		// The rule takes a piece of grammar and uses it as a number, which is a rule that
		// would otherwise repeat zero times and match nothing.
		Refused(
			GrammarNormalizer.UnbuiltCall,
			"Digits(n) = ['0'..'9']{n}\nWord = ['a'..'z']\nStart = Digits(Word)");

	[Fact]
	public void A_call_that_would_specialize_for_ever_is_refused()
	{
		// §4.2 asks for this in as many words, and the reason is worse than an unhelpful
		// message: each call wraps its own argument, so there is no repeat to find and no
		// end to the specializing. It used to overflow the stack — which is not an
		// exception and takes the process with it, so an author would watch their IDE lose
		// the compiler rather than read anything about their grammar.
		Refused(
			GrammarNormalizer.UnbuiltCall,
			"Grow(item) = 'x' & Grow(Pair(item))\n" +
			"Pair(item) = item & item\n" +
			"Word = ['a'..'z']\n" +
			"Start = Grow(Word)");
	}

	[Fact]
	public void A_call_with_the_wrong_number_of_arguments_is_refused() =>
		Refused(GrammarNormalizer.UnbuiltCall, Listing + "Start = List(Word)");

	[Fact]
	public void A_parameter_declared_as_a_C_sharp_type_is_a_value_and_says_so() =>
		// §4.2: a C# type makes the parameter a value, anything else makes it a recognizer.
		// Only one value is built — a number — so a `pad: char` handed a literal used to be
		// quietly taken as a recognizer instead, which is the declaration meaning one thing
		// to the author and another to the compiler.
		Refused(
			GrammarNormalizer.UnbuiltCall,
			"Padded(item, pad: char) = item & pad\nWord = ['a'..'z']+\nStart = Padded(Word, ' ')");

	[Fact]
	public void A_number_still_reaches_a_parameter_that_declared_its_type() =>
		// The half that is built: `n: int` is a value, and a number is a value.
		Assert.True(Matches("Digits(n: int) = ['0'..'9']{n}\nStart = Digits(4)", "2026"));

	// ── Backtracking (§11) ───────────────────────────────────────────────────────

	// A greedy operand that took too much has to give it back when what follows fails.
	// Every one of these matched nothing before the recognizer became a machine with a
	// stack of the points it could have gone another way.

	[Fact] public void Optional_gives_back()  => Assert.True(Matches("Start = 'a'? & 'a'",     "a"));
	[Fact] public void Star_gives_back()      => Assert.True(Matches("Start = 'a'* & 'a'",     "a"));
	[Fact] public void Plus_gives_back()      => Assert.True(Matches("Start = 'a'+ & 'a'",     "aa"));
	[Fact] public void Counted_gives_back()   => Assert.True(Matches("Start = 'a'{1,2} & 'a'", "aa"));

	[Fact]
	public void A_choice_gives_back_across_the_rest_of_the_sequence() =>
		// The specification's own counterexample: §11 rests the rule that alternatives
		// may never be reordered on this working.
		Assert.True(Matches("""Start = ("x" | "xy") & 'y'""", "xy"));

	[Fact]
	public void And_keeps_giving_back_until_something_fits() =>
		Assert.True(Matches("Start = ['a'..'z']* & 'c' & ['a'..'z']*", "abcde"));

	[Theory]
	[InlineData("aaab", true)]
	[InlineData("aaa",  false)]
	public void Nested_repetition_backtracks(string input, bool expected) =>
		Assert.Equal(expected, Matches("Start = ('a'+)+ & 'b'", input));

	[Fact]
	public void Backtracking_does_not_make_a_failing_match_succeed() =>
		Assert.False(Matches("Start = 'a'* & 'b'", "aaa"));

	[Fact]
	public void A_lookahead_leaves_nothing_behind_to_backtrack_into() =>
		Assert.True(Matches("Start = ?=('a' | 'b') & 'a' & 'b'", "ab"));

	[Theory]
	[InlineData("ab", true)]
	[InlineData("ax", false)]
	public void A_positive_lookahead_asks_without_consuming(string input, bool expected) =>
		// It consumes nothing, so the 'b' after it matches the same character it looked at.
		Assert.Equal(expected, Matches("Start = 'a' & ?='b' & 'b'", input));

	[Theory]
	[InlineData("ac", true)]
	[InlineData("ab", false)]
	public void A_negative_lookahead_refuses_what_it_finds(string input, bool expected) =>
		// The other half of §3.6, and the only one nothing tested for its own sake: `eof`
		// lowers to `?![^ ]`, so it was exercised only through that.
		Assert.Equal(expected, Matches("Start = 'a' & ?!'b' & ['a'..'z']", input));

	[Theory]
	[InlineData("Name = \"x\" | \"xy\"", "xy", true)]   // shorter first: "x", then 'y' takes the y
	[InlineData("Name = \"xy\" | \"x\"", "xy", false)]  // longer first: "xy", and 'y' has nothing left
	public void A_call_answers_once_and_is_not_asked_again(string name, string input, bool expected) =>
		// The boundary §4 freezes. Which way round the alternatives are written decides
		// whether it shows: ordered choice inside `Name` picks the first that matches, and
		// `Start` cannot send it back for the other one. Written in one rule it could.
		Assert.Equal(expected, Matches($"Start = Name & 'y'\n{name}", input));

	[Fact]
	public void And_the_same_expressions_in_one_rule_do_backtrack() =>
		Assert.True(Matches("Start = (\"xy\" | \"x\") & 'y'", "xy"));

	[Fact]
	public void Nesting_a_rule_deep_inside_itself() =>
		// Backtracking is a machine inside a rule and an ordinary call between rules, so
		// nesting costs the process stack — about 2700 levels on the default one, which
		// docs/status.md states and explains. This is well under it, and is here so that a
		// change making frames heavier shows up as a failure rather than in production.
		Assert.True(Matches(
			"Expr = '(' & Expr & ')' | 'x'\nStart = Expr",
			new string('(', 1000) + "x" + new string(')', 1000)));

	[Fact]
	public void Repetition_longer_than_the_first_stack_page() =>
		// The stack starts at 48 ints and grows; this needs far more frames than that.
		Assert.True(Matches("Start = ['a'..'z']* & 'z'", new string('a', 500) + "z"));

	// ── Unicode categories (§3.1) ────────────────────────────────────────────────

	[Theory]
	[InlineData("AB",  true)]
	[InlineData("Ab",  false)]
	public void Category_by_abbreviation(string input, bool expected) =>
		Assert.Equal(expected, Matches(@"Start = [\p{Lu}]+", input));

	[Theory]
	[InlineData("abc", true)]
	[InlineData("ab1", false)]
	public void A_category_group_is_every_category_in_it(string input, bool expected) =>
		Assert.Equal(expected, Matches(@"Start = [\p{L}]+", input));

	[Fact]
	public void Digits_by_category() => Assert.True(Matches(@"Start = [\p{Nd}]+", "2026"));

	[Fact]
	public void An_unknown_category_is_refused() =>
		Refused(GramLexer.UnknownCategory, @"Start = [\p{Zz}]");

	// ── References inside an element set (§3.1) ──────────────────────────────────

	[Theory]
	[InlineData("ab",  true)]
	[InlineData("a1",  false)]
	public void A_set_may_name_an_elementary_rule(string input, bool expected) =>
		Assert.Equal(expected, Matches("Letter = ['a'..'z']\nStart = [Letter]+", input));

	[Theory]
	[InlineData("a1",  true)]
	[InlineData("a-",  false)]
	public void And_is_merged_with_the_rest_of_the_set(string input, bool expected) =>
		Assert.Equal(expected, Matches("Letter = ['a'..'z']\nStart = [Letter | '0'..'9']+", input));

	[Fact]
	public void A_rule_declared_after_the_set_that_names_it_still_merges() =>
		Assert.True(Matches("Start = [Letter]+\nLetter = ['a'..'z']", "ab"));

	[Fact]
	public void Complementing_a_set_complements_what_was_merged_into_it() =>
		Assert.False(Matches("Letter = ['a'..'z']\nStart = [^ Letter]+", "ab"));

	[Fact]
	public void A_rule_that_is_not_one_element_cannot_be_in_a_set() =>
		Refused(GrammarNormalizer.UnsupportedElement, "Pair = 'a' & 'b'\nStart = [Pair]");

	// ── Literals ────────────────────────────────────────────────────────────────

	[Fact]
	public void A_character_literal_holds_one_character() =>
		Refused(GramLexer.MalformedCharacter, "Start = 'ab'");

	[Fact]
	public void An_empty_character_literal_is_refused() =>
		Refused(GramLexer.MalformedCharacter, "Start = ''");

	[Theory]
	[InlineData(@"'\0'",     "\0")]
	[InlineData(@"'\a'",     "\a")]
	[InlineData(@"'\v'",     "\v")]
	[InlineData(@"'é'", "é")]
	[InlineData("'\u2028'", "\u2028")]     // a line separator: legal in a grammar, not in C# source
	public void Control_and_non_ascii_characters_survive_emission(string literal, string input) =>
		Assert.True(Matches($"Start = {literal}", input));

	// ── Where a failure is reported ─────────────────────────────────────────────

	/// <summary>The message and the position a grammar refuses an input with.</summary>
	static (string Error, long Position) Refusal(string grammar, string input)
	{
		var (isSuccess, _, error, position) = Parsed(grammar, input);

		Assert.False(isSuccess, input);

		return (error!, position);
	}

	[Fact]
	public void A_refusal_names_the_position_the_input_stopped_making_sense_at() =>
		Assert.Equal(2, Refusal("""Start = "ab" & ['c'] & ['d']""", "abXY").Position);

	[Fact]
	public void The_position_is_where_the_failing_operand_began_not_where_it_gave_up() =>
		// `"abcd"` is one operand and it starts at 0, so that is what is named, though the
		// character that did not fit is at 2. Sharpening this means recording the offset
		// at each failing test rather than one position at the point of giving up — a
		// refinement of what is here, not a different shape.
		Assert.Equal(0, Refusal("""Start = "abcd" """, "abXY").Position);

	[Fact]
	public void It_is_the_furthest_reached_and_not_the_last_tried() =>
		// The first alternative fails at 0 and the second gets to 2 before failing. What
		// is worth reporting is how far the input could be followed, so the position only
		// ever rises — a later, shallower failure does not overwrite a deeper one.
		Assert.Equal(2, Refusal("""Start = ("abc" | "ab") & 'z' """, "abq").Position);

	[Fact]
	public void A_failure_inside_a_rule_is_the_caller_s_failure_too() =>
		// The state is threaded through the call rather than returned, so a rule boundary
		// does not flatten the position back to where the call was made.
		Assert.Equal(
			2,
			Refusal("Inner = ['a'] & ['b'] & ['c']\nStart = Inner", "abq").Position);

	[Fact]
	public void A_lookahead_does_not_report_how_far_it_looked() =>
		// It reached position 2 inside itself and consumed nothing. Naming 2 would point
		// at input the match never needed; what failed is the lookahead, at 0.
		Assert.Equal(
			0,
			Refusal("Start = ?=(['a'] & ['b'] & ['z']) & ['a']", "abq").Position);

	// ── Captures, and the value they build (§7.3) ───────────────────────────────

	/// <summary>Compiles and runs as <see cref="Matches"/> does, and hands back the value.</summary>
	static object? Built(string grammar, string input)
	{
		var (isSuccess, value, _, _) = Parsed(grammar, input);

		Assert.True(isSuccess, input);

		return value;
	}

	/// <summary>A member of a built value, by name — the type did not exist to compile against.</summary>
	static object? Read(object? value, params string[] path)
	{
		foreach (var name in path)
			value = value?.GetType().GetProperty(name)?.GetValue(value);

		return value;
	}

	[Fact]
	public void Each_capture_is_a_member_named_after_it()
	{
		var value = Built("Start = scheme: \"ab\" & rest: 'c'", "abc");

		Assert.Equal("ab", Read(value, "Scheme"));
		Assert.Equal("c",  Read(value, "Rest"));
	}

	[Fact]
	public void A_repeated_capture_is_the_text_of_the_whole_run() =>
		// §10 binds a capture tighter than a quantifier, so this is one capture repeated.
		// §7.3 gives it the text joined, which is the run rather than its last iteration.
		Assert.Equal("8080", Read(Built("Start = digits: ['0'..'9']+", "8080"), "Digits"));

	[Fact]
	public void A_run_that_matched_nothing_is_empty_rather_than_absent() =>
		Assert.Equal("", Read(Built("Start = digits: ['0'..'9']* & 'x'", "x"), "Digits"));

	[Fact]
	public void An_option_that_was_not_taken_is_absent_rather_than_empty() =>
		Assert.Null(Read(Built("Start = (sign: '-')? & 'x'", "x"), "Sign"));

	[Fact]
	public void A_capture_of_a_rule_that_builds_holds_its_value() =>
		Assert.Equal(
			"x",
			Read(Built("Inner = letter: 'x'\nStart = inner: Inner", "x"), "Inner", "Letter"));

	[Fact]
	public void A_capture_the_match_gave_back_is_not_in_the_value()
	{
		// The first alternative matches 'a' and then fails on 'c', so the match resumes in
		// the second — where nothing was captured, and `a` must be as unwritten as if the
		// first alternative had never been tried.
		var value = Built("Start = (a: 'x' & 'y' | 'x' & b: 'z')", "xz");

		Assert.Null(Read(value, "A"));
		Assert.Equal("z", Read(value, "B"));
	}

	[Fact]
	public void The_same_name_in_two_alternatives_is_one_member() =>
		Assert.Equal("y", Read(Built("Start = (v: \"xy\" | v: 'y')", "y"), "V"));

	[Fact]
	public void A_repeated_capture_of_a_rule_is_a_sequence_of_its_values()
	{
		var value = Built("Item = letter: ['a'..'z']\nStart = items: Item+", "abc");
		var items = (Array)Read(value, "Items")!;

		Assert.Equal(3, items.Length);
		Assert.Equal(["a", "b", "c"], items.Cast<object>().Select(item => Read(item, "Letter")));
	}

	[Fact]
	public void An_empty_run_is_an_empty_sequence_rather_than_null() =>
		Assert.Empty((Array)Read(
			Built("Item = letter: 'x'\nStart = items: Item* & 'y'", "y"), "Items")!);

	[Fact]
	public void A_sequence_gives_back_what_an_abandoned_attempt_appended()
	{
		// `Item+` takes three, `'z'` fails, and the repetition hands one back at a time
		// until `'c' & 'z'` fits. What it collected has to shrink with it — the length at
		// the moment of the push is on the backtracking frame, and the resume truncates
		// to it.
		var items = (Array)Read(
			Built("Item = letter: ['a'..'z']\nStart = items: Item+ & 'c' & 'z'", "abcz"), "Items")!;

		Assert.Equal(["a", "b"], items.Cast<object>().Select(item => Read(item, "Letter")));
	}

	[Fact]
	public void And_a_repetition_inside_a_repetition_gives_back_only_its_own_iteration()
	{
		var items = (Array)Read(
			Built("Item = letter: ['a'..'z']\nStart = (items: Item+ & '.')* & 'x'", "ab.cd.x"),
			"Items")!;

		Assert.Equal(["a", "b", "c", "d"], items.Cast<object>().Select(item => Read(item, "Letter")));
	}

	[Fact]
	public void A_capture_of_text_inside_a_repetition_is_still_the_run_it_matched() =>
		// §7.3 keeps the two apart: a quantifier over text joins it, a quantifier over a
		// rule collects. Only the second is a sequence.
		Assert.Equal("8080", Read(Built("Start = digits: ['0'..'9']+", "8080"), "Digits"));

	[Fact]
	public void But_text_captured_around_something_else_is_still_refused() =>
		Refused(GrammarNormalizer.UnbuiltCapture, "Start = (a: 'x' & 'y')+");

	[Fact]
	public void Nor_is_one_inside_a_lookahead() =>
		Refused(GrammarNormalizer.UnbuiltCapture, "Start = ?=(a: 'x') & 'x'");

	[Fact]
	public void One_name_cannot_hold_two_different_things() =>
		Refused(
			GrammarNormalizer.CaptureTypeMismatch,
			"Item = a: 'x'\nStart = (v: Item | v: 'y')");

	// ── A rule that declares its own type and builds it (§7.3) ──────────────────

	[Fact]
	public void A_rule_may_name_a_C_sharp_type_and_say_how_to_build_it() =>
		Assert.Equal(
			42,
			Built(
				"""
				@using System.Globalization;

				Start : @int = ['0'..'9']+ => @int.Parse(parserText, @CultureInfo.InvariantCulture)
				""",
				"42"));

	[Fact]
	public void The_matched_text_is_supplied_as_parserText() =>
		Assert.Equal(
			3,
			Built("Start : @int = ['a'..'z']+ => @(parserText.Length)", "abc"));

	[Fact]
	public void Captures_reach_the_expression_by_their_own_names() =>
		Assert.Equal(
			"b-a",
			Built("""Start : @string = a: 'a' & b: 'b' => @(b + "-" + a)""", "ab"));

	[Theory]
	[InlineData("12",  24)]
	[InlineData("abc", 3)]
	public void Each_alternative_may_build_the_value_its_own_way(string input, int expected) =>
		Assert.Equal(
			expected,
			Built(
				"""
				Start : @int = digits: ['0'..'9']+   => @(int.Parse(digits) * 2)
				             | letters: ['a'..'z']+  => @(letters.Length)
				""",
				input));

	[Fact]
	public void An_alternative_that_was_tried_and_given_back_does_not_build_the_value() =>
		// The first alternative matches "ab" and then `eof` fails, so the match returns
		// and the second builds instead. Which `=>` fired is undone with everything else
		// the abandoned attempt did.
		Assert.Equal(
			2,
			Built(
				"""
				Start : @int = a: "ab"        => @(1)
				             | b: ['a'..'z']+ => @(2)
				""",
				"abc"));

	[Fact]
	public void A_construction_needs_a_type_to_build() =>
		Refused(GrammarNormalizer.UnbuiltConstruction, "Start = ['0'..'9']+ => @(1)");

	[Fact]
	public void And_a_type_needs_every_alternative_to_build_it() =>
		Refused(
			GrammarNormalizer.UnbuiltConstruction,
			"""Start : @int = "a" => @(1) | "b" """);

	[Fact]
	public void A_construction_belongs_on_an_alternative_of_the_rule() =>
		Refused(
			GrammarNormalizer.UnbuiltConstruction,
			"""Start : @int = ("a" => @(1) | "b" => @(2)) & 'c' => @(3)""");

	// ── Left recursion, and what it says about associativity (§4.3) ─────────────

	const string Calculator = """
		Sum     : @int = left: Sum     & op: ['+' | '-'] & right: Product => @(op == "+" ? left + right : left - right)
		               | value: Product                                   => @(value)

		Product : @int = left: Product & op: ['*' | '/'] & right: Primary => @(op == "*" ? left * right : left / right)
		               | value: Primary                                   => @(value)

		Primary : @int = '(' & inner: Sum & ')'                           => @(inner)
		               | digits: ['0'..'9']+                              => @int.Parse(digits)

		""";

	[Theory]
	[InlineData("1+2+3",       6)]
	[InlineData("1-2-3",      -4)]     // (1-2)-3, not 1-(2-3)
	[InlineData("2*3+4",      10)]
	[InlineData("2+3*4",      14)]
	[InlineData("(2+3)*4",    20)]
	[InlineData("100/5/2",    10)]     // (100/5)/2, not 100/(5/2)
	[InlineData("7",           7)]
	public void A_calculator(string input, int expected) =>
		Assert.Equal(expected, Built(Calculator + "Start : @int = value: Sum => @(value)", input));

	[Fact]
	public void An_alternative_recursive_on_both_sides_is_refused() =>
		// `-1-2` would answer 1 rather than -3: the trailing call takes everything to the
		// right, so what is written left-associative parses right-associative. Ordered
		// choice cannot settle it, and a wrong answer is worse than a refusal.
		Refused(
			GrammarNormalizer.LeftRecursion,
			"""
			Start : @int = left: Start & '-' & right: Start => @(left - right)
			             | '-' & operand: Start             => @(-operand)
			             | digits: ['0'..'9']+              => @int.Parse(digits)
			""");

	[Theory]
	[InlineData("-1-2", -3)]
	[InlineData("1--2",  3)]
	[InlineData("-1",   -1)]
	public void A_unary_operator_written_at_its_own_level(string input, int expected) =>
		// The same operators, said properly: unary binds tighter, so it is a level of its
		// own and the binary one takes operands from it.
		Assert.Equal(
			expected,
			Built(
				"""
				Start : @int = left: Start & '-' & right: Unary => @(left - right)
				             | value: Unary                     => @(value)

				Unary : @int = '-' & operand: Unary             => @(-operand)
				             | digits: ['0'..'9']+              => @int.Parse(digits)
				""",
				input));

	[Theory]
	[InlineData("a.b.c",      "((a.b).c)")]
	[InlineData("a(b)[c].d",  "(((a(b))[c]).d)")]
	[InlineData("a",          "a")]
	public void A_rule_may_have_as_many_recursive_alternatives_as_it_likes(string input, string expected) =>
		// A postfix chain wants three, each with captures of its own. Nothing is built
		// while matching, so nothing has to hold them all in one type.
		Assert.Equal(
			expected,
			Built(
				"""
				Start : @string = target: Start & '.' & name: Name        => @("(" + target + "." + name + ")")
				                | target: Start & '(' & arg: Name & ')'   => @("(" + target + "(" + arg + "))")
				                | target: Start & '[' & index: Name & ']' => @("(" + target + "[" + index + "])")
				                | atom: Name                              => @(atom)

				Name : @string  = letters: ['a'..'z']+                    => @(letters)
				""",
				input));

	/// <summary>
	/// §4.3.1 is specified and not built. It parses, so a grammar that uses it is told
	/// what is wrong rather than handed a syntax error — and the day the engine lands,
	/// these are the tests that stop passing for the right reason.
	/// </summary>
	// ── Binding powers (§4.3.1) ─────────────────────────────────────────────────

	/// <summary>§4.3.1 as written there: one rule, a whole expression language.</summary>
	const string Powers = """
		Start : @int = left: Start & '+' & right: Start << 1 => @(left + right)
		             | left: Start & '-' & right: Start << 1 => @(left - right)
		             | left: Start & '*' & right: Start << 2 => @(left * right)
		             | '-' & operand: Start              >> 4 => @(-operand)
		             | '(' & inner: Start & ')'               => @(inner)
		             | digits: ['0'..'9']+                    => @int.Parse(digits)
		""";

	[Theory]
	[InlineData("1+2",       3)]
	[InlineData("1-2-3",    -4)]    // << is left-associative: (1-2)-3
	[InlineData("2+3*4",    14)]    // 3*4 is stronger, so it is taken first
	[InlineData("2*3+4",    10)]
	[InlineData("(2+3)*4",  20)]
	[InlineData("-1-2",     -3)]    // §4.3.1's own example: unary is stronger than binary
	[InlineData("-(1-2)",    1)]
	public void One_rule_of_strengths_parses_a_whole_expression_language(string input, int expected) =>
		Assert.Equal(expected, Built(Powers, input));

	/// <summary>The same operator both ways round — the whole of what the markers say.</summary>
	static string Associates(string marker) => $"""
		Start : @int = left: Start & '-' & right: Start {marker} => @(left - right)
		             | digits: ['0'..'9']+                       => @int.Parse(digits)
		""";

	[Fact]
	public void Left_is_one_strength_tighter_and_right_is_the_same_one()
	{
		// `<<` parses the right operand at n + 1, so the operator cannot appear in it and
		// 1-2-3 groups as (1-2)-3 = -4. `>>` parses it at n, so it can, and the same input
		// groups as 1-(2-3) = 2. One character of difference, one number of difference.
		Assert.Equal(-4, Built(Associates("<< 1"), "1-2-3"));
		Assert.Equal( 2, Built(Associates(">> 1"), "1-2-3"));
	}

	[Fact]
	public void An_alternative_recursive_on_both_sides_is_what_a_strength_settles() =>
		// Refused without a strength (ordered choice cannot say which way it groups), and
		// the ordinary case with one. The refusal and the feature are the same shape.
		Assert.Equal(2, Built(Associates(">> 1"), "1-2-3"));

	[Fact]
	public void A_prefix_needs_no_loop_and_gets_none() =>
		// Nothing but a prefix and an atom: every alternative is a base, so the rule climbs
		// without ever looping. `--1` works because the operand is parsed at 4, where the
		// prefix itself still lives.
		Assert.Equal(1, Built(
			"""
			Start : @int = '-' & operand: Start >> 4 => @(-operand)
			             | digits: ['0'..'9']+       => @int.Parse(digits)
			""",
			"--1"));

	[Fact]
	public void A_recursive_alternative_without_a_strength_among_ones_that_have_it_is_refused() =>
		// §4.3.1: a rule uses one convention or the other. Half of each would be two
		// answers to the same question.
		Refused(
			GrammarNormalizer.UnbuiltBinding,
			"""
			Start : @int = left: Start & '+' & right: Start << 1 => @(left + right)
			             | left: Start & '-' & right: Start      => @(left - right)
			             | digits: ['0'..'9']+                   => @int.Parse(digits)
			""");

	[Fact]
	public void A_strength_on_something_with_no_operand_is_refused() =>
		// A strength says how tightly the operand to the right is read, and there is none.
		Refused(
			GrammarNormalizer.UnbuiltBinding,
			"""
			Start : @int = left: Start & '+' & right: Start << 1 => @(left + right)
			             | digits: ['0'..'9']+              >> 9 => @int.Parse(digits)
			""");

	// ── `recover` (§8.2) ────────────────────────────────────────────────────────

	const string Records = """
		Row   = name: ['a'..'z']+ & eol
		Start = rows: Row* recover eol => @(new Row("!" + parserText))
		""";

	[Fact]
	public void A_broken_element_is_stepped_over_and_the_rest_are_read() =>
		// The second line begins a Row and breaks in the middle of one, which is an error
		// rather than the end of the sequence. What follows is read.
		Assert.Equal(
			["aa", "!b1b", "cc"],
			((Array)Read(Built(Records, "aa\nb1b\ncc\n"), "Rows")!)
				.Cast<object>()
				.Select(row => Read(row, "Name")));

	[Fact]
	public void What_never_began_still_ends_the_sequence() =>
		// `Trailer` is not a Row and does not start like one, so the repetition ends
		// rather than recovering — the difference §8.2 rests on.
		Assert.Equal(
			["aa"],
			((Array)Read(
				Built("""
					Row   = name: ['a'..'z']+ & eol
					Start = rows: Row* recover eol => @(new Row("!" + parserText)) & '.' & eol
					""",
					"aa\n.\n"),
				"Rows")!)
				.Cast<object>()
				.Select(row => Read(row, "Name")));

	[Fact]
	public void A_broken_element_at_the_end_takes_what_is_left() =>
		Assert.Equal(
			["aa", "!b1b"],
			((Array)Read(Built(Records, "aa\nb1b"), "Rows")!)
				.Cast<object>()
				.Select(row => Read(row, "Name")));

	[Fact]
	public void A_recovered_element_is_told_where_it_was_and_which_one_it_is() =>
		// `parserText`, `parserPosition` and `parserOrdinal` are supplied rather than
		// captured (§8.2), and the ordinal counts the rejected element too — it holds its
		// place.
		Assert.Equal(
			["aa", "!3:1", "cc"],
			((Array)Read(
				Built("""
					Row   = name: ['a'..'z']+ & eol
					Start = rows: Row* recover eol => @(new Row($"!{parserPosition}:{parserOrdinal}"))
					""",
					"aa\nb1b\ncc\n"),
				"Rows")!)
				.Cast<object>()
				.Select(row => Read(row, "Name")));

	[Fact]
	public void A_recovered_element_knows_where_a_person_would_look_for_it() =>
		// `line` and `column` are where the element starts, both from 1 — the header shifts
		// the first record off line one, which is the whole reason they are not the ordinal.
		Assert.Equal(
			["aa", "!3:1", "cc"],
			((Array)Read(
				Built("""
					Row   = name: ['a'..'z']+ & eol
					Start = "H" & eol & rows: Row* recover eol => @(new Row($"!{parserLine}:{parserColumn}"))
					""",
					"H\naa\nb1b\ncc\n"),
				"Rows")!)
				.Cast<object>()
				.Select(row => Read(row, "Name")));

	[Fact]
	public void A_recovered_element_knows_its_extent() =>
		// `span` is the support type, which a generated parser has beside it — the factory
		// is private to the host class, so nothing internal reaches a public signature. The
		// extent stops where the synchronization point begins: `eol` separates the elements
		// and is not part of one, which is why "b1b" is three characters and not four.
		Assert.Equal(
			["aa", "!3+3", "cc"],
			((Array)Read(
				Built("""
					Row   = name: ['a'..'z']+ & eol
					Start = rows: Row* recover eol => @(new Row($"!{parserSpan.Start}+{parserSpan.Length}"))
					""",
					"aa\nb1b\ncc\n"),
				"Rows")!)
				.Cast<object>()
				.Select(row => Read(row, "Name")));

	[Fact]
	public void A_recovered_element_can_say_why_it_was_rejected() =>
		// The rule it should have been, and where the input stopped being one.
		Assert.Equal(
			["aa", "Input does not match 'Row' at 4.", "cc"],
			((Array)Read(
				Built("""
					Row   = name: ['a'..'z']+ & eol
					Start = rows: Row* recover eol => @(new Row(parserMessage))
					""",
					"aa\nb1b\ncc\n"),
				"Rows")!)
				.Cast<object>()
				.Select(row => Read(row, "Name")));

	[Fact]
	public void An_ordinary_repetition_hands_an_element_back() =>
		// One row, and `tail` gets the other — the repetition took both and gave one up.
		Assert.True(
			Matches("""
				Row   = name: ['a'..'z']+ & eol
				Start = rows: Row* & tail: ['a'..'z']+ & eol
				""",
				"aa\nbb\n"));

	[Fact]
	public void A_recovering_repetition_gives_nothing_back() =>
		// The same grammar, marked. §8.2 calls the mark a commit point: an element it took
		// was either good or explicitly rejected, so there is no shorter reading to come
		// back for, and `tail` is left with nothing.
		Assert.False(
			Matches("""
				Row   = name: ['a'..'z']+ & eol
				Start = rows: Row* recover eol => @(new Row("!" + parserText)) & tail: ['a'..'z']+ & eol
				""",
				"aa\nbb\n"));

	[Fact]
	public void Recover_belongs_on_a_repetition() =>
		Refused(GrammarNormalizer.UnbuiltRecovery, "Start = 'a' recover eol");

	[Fact]
	public void Recover_without_a_factory_drops_the_element_and_reports_it()
	{
		// §8.3: no `=>`, so the broken element does not join the sequence — the good ones
		// are all that come back — and what it was goes to a `partial void` the class
		// declares for the consumer to implement.
		var source = Emitted("""
			Row   = name: ['a'..'z']+ & eol
			Start = rows: Row* recover eol
			""");

		Assert.Contains("static partial void OnRecovered(", source);

		// Everything the hook is told is an argument. Nothing is computed into a local
		// first, because a statement would survive the erasure and the scan would happen
		// whether or not anybody is listening.
		Assert.Contains("OnRecovered(\"Row\", text.Slice(from, to - from).ToString(), from, LineAt(text, from), ColumnAt(text, from), c", source);

		// The elements that did match still collect — it is only the broken one that is
		// dropped — so what must be absent is a factory, not the collecting.
		Assert.Contains("l0.Add(v0);",  source);
		Assert.DoesNotContain("_Recover(", source);
	}

	/// <summary>The C# a grammar compiles to, when what is under test is the code itself.</summary>
	static string Emitted(string grammar)
	{
		var result = Compile(grammar + "\nparse Start");

		Assert.Empty(result.Diagnostics);

		return result.Sources[0].Text;
	}

	[Fact]
	public void And_with_nobody_listening_the_hook_is_not_there_at_all()
	{
		// The claim the whole design rests on, checked against the compiler rather than
		// assumed: with no implementing half, C# removes the declaration itself — so it
		// cannot be found on the compiled type, and the calls and their arguments went
		// with it. Nothing is materialized, nothing is scanned, nothing is paid.
		var assembly = EmittedCode.Compile(Emitted("""
			Row   = name: ['a'..'z']+ & eol
			Start = rows: Row* recover eol
			"""));

		Assert.Null(assembly.GetType("Grammar")!.GetMethod(
			"OnRecovered",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static));
	}

	[Fact]
	public void And_the_parse_still_steps_over_the_broken_one() =>
		// Dropped from the sequence, and the rest read — which is what §8.3 promises a
		// grammar that has no type to spare.
		Assert.Equal(
			["aa", "cc"],
			((Array)Read(
				Built("""
					Row   = name: ['a'..'z']+ & eol
					Start = rows: Row* recover eol
					""",
					"aa\nb1b\ncc\n"),
				"Rows")!)
				.Cast<object>()
				.Select(row => Read(row, "Name")));

	[Fact]
	public void And_a_grammar_that_recovers_with_a_factory_declares_no_hook() =>
		// The channel is emitted for the grammars that report on it and no others.
		Assert.DoesNotContain("OnRecovered", Emitted(Records));

	[Fact]
	public void A_synchronization_point_may_be_a_choice_when_it_is_bracketed()
	{
		// Recovering to the next separator *or* the end of the line, whichever comes
		// first — which is what a field-level recovery wants, rather than throwing the
		// rest of the record away.
		var rows = (Array)Read(
			Built("""
				Field = name: ['a'..'z']+ & '|'
				Start = fields: Field* recover ('|' | eol) => @(new Field("!" + parserText))
				""",
				"aa|b1b|cc|"),
			"Fields")!;

		Assert.Equal(["aa", "!b1b", "cc"], rows.Cast<object>().Select(field => Read(field, "Name")));
	}

	[Fact]
	public void Without_the_brackets_it_binds_tighter_than_the_choice() =>
		// `recover` takes one operand, so the `|` belongs to the enclosing expression:
		// this is `(fields: Field* recover '|') | eol`, not a choice of two sync points.
		// Precedence, the same as `a & b | c` — and the reason the brackets are not
		// optional.
		Assert.Contains(
			"Recovering",
			GramParser.Parse(GramLexer.Tokenize(
				"Start = fields: Field* recover '|' | eol\nField = ['a'..'z']+",
				RoslynCSharpScanner.Instance)).File.ToString());

	[Fact]
	public void Only_one_repetition_of_a_rule_recovers() =>
		// The second would be ignored, and a `recover` that is quietly not there is the
		// failure recovery exists to prevent.
		Refused(
			GrammarNormalizer.UnbuiltRecovery,
			"""
			Row   = name: ['a'..'z']+ & eol
			Start = rows: Row* recover eol => @(new Row("!" + parserText))
			      & more: Row* recover eol => @(new Row("?" + text))
			""");

	[Fact]
	public void A_recovery_that_builds_needs_a_sequence_to_build_into() =>
		// `Row` captures nothing, so `rows: Row*` is one string — the run joined (§7.3) —
		// and a rejection has nowhere to arrive. Found by writing a test about something
		// else: it emitted a factory call against a list that does not exist, which the
		// consumer's compiler reports as an undefined name in a file they never wrote.
		Refused(
			GrammarNormalizer.UnbuiltRecovery,
			"""
			Row   = ['a'..'z']+ & eol
			Start = rows: Row* recover eol => @(parserText)
			""");

	[Fact]
	public void And_the_same_repetition_without_one_is_fine() =>
		// §8.3: no `=>`, so nothing is collected and the rejection goes to the hook. The
		// sequence that is not there is not needed.
		Assert.Empty(Compile("""
			Row   = ['a'..'z']+ & eol
			Start = rows: Row* recover eol
			parse Start
			""").Diagnostics);

	[Fact]
	public void Every_alternative_of_a_rule_being_left_recursive_is_refused() =>
		Refused(GrammarNormalizer.LeftRecursion, "Start : @int = left: Start & 'x' => @(left)");

	[Fact]
	public void Indirect_left_recursion_is_still_refused() =>
		Refused(
			GrammarNormalizer.LeftRecursion,
			"""
			Start = Other & 'x'
			Other = Start | 'y'
			""");

	// ── `where` guards (§8.1) ───────────────────────────────────────────────────

	[Theory]
	[InlineData("12",  true)]
	[InlineData("123", false)]
	public void A_guard_asks_a_question_of_the_text_so_far(string input, bool expected) =>
		Assert.Equal(expected, Matches("Start = ['0'..'9']+ & where @(parserText.Length < 3)", input));

	[Theory]
	[InlineData("ab", true)]
	[InlineData("ax", false)]
	public void And_of_the_captures_written_before_it(string input, bool expected) =>
		Assert.Equal(
			expected,
			Matches("""Start = a: 'a' & b: ['a'..'z'] & where @(b == "b")""", input));

	[Fact]
	public void A_failing_guard_is_a_non_match_and_a_sibling_is_tried() =>
		// Recognition, not a value failure: saying no sends the match back into the choice
		// rather than ending it. §8.1 is where the two are told apart.
		Assert.True(Matches(
			"""Start = (a: "ab" & where @(a == "xy") | a: "ab") & 'c'""",
			"abc"));

	[Fact]
	public void A_guard_may_stand_where_nothing_has_been_captured() =>
		Assert.True(Matches("Start = ['0'..'9']+ & where @(true)", "7"));

	// §7.1's `@Name` as an operand is decided by the shape of the C# method it names, so
	// it is tested where there is a compilation to ask: GeneratorDriverTests.

	[Fact]
	public void A_capture_may_not_take_a_name_that_is_supplied() =>
		// The supplied names become parameters of the method a `=>` turns into, so a
		// capture of the same name wants one that is already taken. The prefix makes that
		// unlikely rather than impossible, and this is the backstop: before it, the
		// generated code simply did not compile, with an error pointing at a file the
		// author never wrote and saying nothing about the grammar.
		Refused(
			GrammarNormalizer.ReservedCaptureName,
			"Start : @string = parserText: ['a'..'z']+ => @(parserText)");

	[Fact]
	public void A_rule_typed_as_another_rule_says_it_is_not_built() =>
		// §4.1 case 3. Before it was refused the declaration was dropped in silence and the
		// rule got a type generated from its own captures — so `A : B` compiled, ran, and
		// handed back an `A` that had nothing to do with `B`.
		Refused(
			GrammarNormalizer.UnbuiltRuleType,
			"""
			A : B = digits: ['0'..'9']+
			B     = 'x'
			""");

	// ── Scopes (§5) ─────────────────────────────────────────────────────────────

	[Fact]
	public void Two_scopes_may_each_have_a_rule_of_the_same_name() =>
		// Which is the whole point of a scope, and which used to emit two C# methods of
		// the same name into the consumer's build. The scopes a rule is declared in are
		// prefixed to the identifier it becomes.
		Assert.True(Matches(
			"""
			using Inner;

			scope Inner
			{
				Digit = ['0'..'9']
				Pair  = Digit & Digit
			}

			Digit = ['a'..'f']
			Start = Digit & Pair & Digit
			""",
			"a12b"));

	[Theory]
	[InlineData("1.5",   true)]
	[InlineData("1 . 5", false)]
	public void A_scope_shadows_Trivia_the_other_way_round(string input, bool expected) =>
		// Trivia goes between the operands of every sequence, `Number`'s included. A scope
		// that shadows it with `none` is how a rule says a space means something here.
		Assert.Equal(
			expected,
			Matches(
				"""
				using Lexical;

				scope Lexical
				{
					Trivia = none

					Number = ['0'..'9']+ & ('.' & ['0'..'9']+)?
				}

				Trivia = [' ']*
				Start  = Number
				""",
				input));

	// ── Repetition counts ───────────────────────────────────────────────────────

	[Fact]
	public void A_count_too_large_for_an_int_is_a_diagnostic_and_not_a_crash() =>
		Refused(GramParser.InvalidCount, "Start = 'a'{999999999999999999999999999}");

	[Fact]
	public void A_range_that_can_never_match_is_a_diagnostic() =>
		Refused(GramParser.InvalidCount, "Start = 'a'{5,2}");

	[Theory]
	[InlineData("aa",   true)]
	[InlineData("aaa",  true)]
	[InlineData("a",    false)]
	[InlineData("aaaa", false)]
	public void Bounded_repetition(string input, bool expected) =>
		Assert.Equal(expected, Matches("Start = 'a'{2,3}", input));
}
