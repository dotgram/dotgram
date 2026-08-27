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
	[Theory]
	[InlineData("x")]
	[InlineData("x<y>")]
	[InlineData("x<<")]
	[InlineData("x<")]
	[InlineData("x<<1")]
	[InlineData("x()")]
	[InlineData("x<y>()")]
	public void TEMPORARY_probe(string input)
	{
		var grammar =
			"trivia = none" + '\n' +
			"Start : @string = p: Primary => @(p)" + '\n' +
			"Primary : @string = c: Call => @(c) | r: Reference => @(r)" + '\n' +
			"Call : @string = t: Reference & '(' & ')' => @(t)" + '\n' +
			"Reference : @string = text: (N & Args?) => @(text)" + '\n' +
			"Args = '<' & Type & '>'" + '\n' +
			"Type : @string = text: (Reference & \"[]\"?) => @(text)" + '\n' +
			"N = ['a'..'z']+";

		var thrown = Record.Exception(() => Parsed(grammar, input));

		Assert.True(thrown is null, $"{input}: {thrown}");
	}

	/// <summary>
	/// A text capture reopened by recursion still spans what it matched.
	/// </summary>
	[Theory]
	[InlineData("a", "a")]
	[InlineData("aa", "aa")]
	[InlineData("aaa", "aaa")]
	public void A_capture_reopened_by_recursion_spans_what_it_matched(string input, string expected)
	{
		var (success, value, _, _) = Parsed(
			"Start : @string = text: ('a' & Start?) => @(text)", input);

		Assert.True(success);
		Assert.Equal(expected, value);
	}

	// ── §4.5: where trivia goes ─────────────────────────────────────────────

	/// <summary>
	/// A separated list is spaced on both sides of its separator, on every turn.
	/// </summary>
	/// <remarks>
	/// The turns of a repetition are a seam between the operands of a sequence wherever the
	/// thing repeated is one, so <c>item &amp; (sep &amp; item)*</c> needs nothing written
	/// out. It used to space only its first turn — from the sequence around the repetition
	/// — which refused a space to the left of the second and every later separator: silently,
	/// and only from the third item on.
	/// </remarks>
	[Theory]
	[InlineData("a", true)]
	[InlineData("a,a", true)]
	[InlineData("a, a", true)]
	[InlineData("a ,a", true)]
	[InlineData("a , a", true)]
	[InlineData("a,a,a", true)]
	[InlineData("a, a, a", true)]
	[InlineData("a ,a ,a", true)]
	[InlineData("a , a , a", true)]
	[InlineData("a , a , a , a", true)]
	[InlineData("a a", false)]
	public void A_separated_list_is_spaced_on_every_turn(string input, bool expected) =>
		Assert.Equal(expected, Matches(
			"trivia = [' ']*" + '\n' +
			"Start = A & (',' & A)*" + '\n' +
			"A = ['a'..'z']+", input));

	/// <summary>
	/// A repetition of one thing is a lexeme, and is not spaced.
	/// </summary>
	/// <remarks>
	/// This is what the insertion was held back for, and it still is: nothing can tell
	/// <c>Word*</c> from a list by looking, so a repetition with no seam inside a turn gets
	/// none between them (§4.5).
	/// </remarks>
	[Theory]
	[InlineData("abcd", true)]
	[InlineData("ab cd", false)]
	public void A_repetition_of_one_thing_is_a_lexeme(string input, bool expected) =>
		Assert.Equal(expected, Matches(
			"trivia = [' ']*" + '\n' +
			"Start = W*" + '\n' +
			"W = ['a'..'z']", input));

	/// <summary>Digits, the example §4.5 gives: <c>1 2</c> is two numbers, not one.</summary>
	[Theory]
	[InlineData("123", true)]
	[InlineData("1 2", false)]
	public void Digits_are_a_lexeme_too(string input, bool expected) =>
		Assert.Equal(expected, Matches(
			"trivia = [' ']*" + '\n' +
			"Start = ['0'..'9']+", input));

	/// <summary>An optional has no second turn, so it has no seam to space.</summary>
	[Theory]
	[InlineData("ab", true)]
	[InlineData("a b", true)]
	[InlineData("a", true)]
	public void An_optional_is_left_alone(string input, bool expected) =>
		Assert.Equal(expected, Matches(
			"trivia = [' ']*" + '\n' +
			"Start = 'a' & 'b'?", input));

	/// <summary>
	/// Spacing a repetition of a single thing is the case that cannot be inferred, and is
	/// still written out (§4.5).
	/// </summary>
	[Theory]
	[InlineData("a a a", true)]
	[InlineData("aaa", true)]
	public void A_spaced_repetition_of_one_thing_still_says_so(string input, bool expected) =>
		Assert.Equal(expected, Matches(
			"trivia = [' ']*" + '\n' +
			"Start = A & (trivia & A)*" + '\n' +
			"A = ['a'..'z']", input));

	// ── A settled repetition keeps one way back ─────────────────────────────

	/// <summary>
	/// Thinning removes the repetition's own exits and nothing else: the body's internal
	/// machinery still finds a reading where a turn has to re-match shorter.
	/// </summary>
	/// <remarks>
	/// `("ab" | "a")*` before `'b'` is settled — a turn starts with 'a', the continuation
	/// with 'b' — so it keeps a single standing exit. On "aab" the parse still has to give
	/// the last turn's longer alternative back and re-take it as "a" before the exit can
	/// stand where 'b' is. That path goes through the body's own choice entries, which is
	/// exactly what the thinning proof leaves in place.
	/// </remarks>
	[Theory]
	[InlineData("b", true)]
	[InlineData("ab", true)]
	[InlineData("aab", true)]
	[InlineData("abab", true)]
	[InlineData("aa", false)]
	public void A_settled_repetition_still_rematches_its_body(string input, bool expected) =>
		Assert.Equal(expected, Matches(
			"Start = (\"ab\" | \"a\")* & 'b'", input));

	/// <summary>
	/// §11 makes a comment's interior reachable by backtracking — and atomic trivia is how
	/// an author says it is not.
	/// </summary>
	/// <remarks>
	/// With `trivia = (' ' | Comment)*`, "x //y" parses as `'x' & 'y'`: the comment first
	/// swallows "//y", the parse fails wanting 'y', and ordered choice hands characters
	/// back until the 'y' inside the comment is syntax again. Legal, and exactly why the
	/// thinning proof declines comment-bearing trivia. With `trivia = { … }` the author
	/// commits what trivia swallowed, the reading disappears, and the proof applies.
	/// </remarks>
	[Theory]
	[InlineData(false, "x //y", true)]
	[InlineData(false, "x y", true)]
	[InlineData(true, "x //y", false)]
	[InlineData(true, "x y", true)]
	public void A_comment_interior_is_syntax_until_the_trivia_is_atomic(
		bool atomic, string input, bool expected) =>
		Assert.Equal(expected, Matches(
			(atomic
				? "trivia = { (' ' | \"//\" & [^ '\\n']*)* }"
				: "trivia = (' ' | \"//\" & [^ '\\n']*)*") + '\n' + "Start = 'x' & 'y'", input));

	/// <summary>
	/// A counted repetition does not count a turn twice when the turn re-matches.
	/// </summary>
	/// <remarks>
	/// The count lives in the Repeat entry and is rewritten in place, and an in-place
	/// rewrite survives backtracking that the turn it counted does not. Resuming the
	/// second alternative inside a completed turn re-completed the body, counted the same
	/// turn again, and `{2}` read two of a thing the input held one of. Found by the
	/// differential fuzzer against the reference interpreter on its first run, and as old
	/// as the engine: the commit this repository started this week at accepts it too.
	/// </remarks>
	[Theory]
	[InlineData("a", false)]
	[InlineData("aa", true)]
	[InlineData("ab", true)]
	[InlineData("aaa", false)]
	public void A_rematched_turn_is_counted_once(string input, bool expected) =>
		Assert.Equal(expected, Matches(
			"Start = ({ ['a'|'c'] } | 'a' | ('b' | 'a' | \"b\")){2}", input));

	// ── Maximal munch is per-symbol, and expressible ───────────────────────

	/// <summary>
	/// C's `a+++++b`, both ways. The notation imposes no maximal munch on symbols, so
	/// which language a grammar means is the grammar's to say.
	/// </summary>
	/// <remarks>
	/// C's lexer is greedy: `a+++++b` lexes as `a ++ ++ + b` and no parse exists — the
	/// standard's own famous corner. A grammar that means that writes the greed as a
	/// guard, `'+' & ?!'+'`, exactly as published PEG grammars of C do; `a+++b` still
	/// reads as `a++ + b`, and `a+++++b` dies the death the standard prescribes. A
	/// grammar that leaves the guard off keeps §11's give-back, and `a+++++b` reads as
	/// `a++ + ++b` — the reading C programmers wish they had. Both languages are three
	/// lines apart, and neither is imposed.
	/// </remarks>
	[Theory]
	[InlineData(true, "a+b", true)]
	[InlineData(true, "a+++b", true)]
	[InlineData(true, "a+++++b", false)]
	[InlineData(true, "a++", true)]
	[InlineData(false, "a+++++b", true)]
	[InlineData(false, "a+++b", true)]
	public void A_grammar_says_whether_plus_is_greedy(bool cLike, string input, bool expected)
	{
		var plus = cLike ? "Plus = '+' & ?!'+'" : "Plus = '+'";
		var grammar =
			"Start    = Operand & (Plus & Operand)*" + '\n' +
			"Operand  = PlusPlus? & ['a'..'z'] & PlusPlus?" + '\n' +
			"PlusPlus = \"++\"" + '\n' + plus;

		Assert.Equal(expected, Matches(grammar, input));
	}

	// ── A rule that only forwards costs nothing ───────────────────────────

	/// <summary>
	/// A transparent tower still delivers its value, through however many floors.
	/// </summary>
	/// <remarks>
	/// `Middle` and `Outer` only forward; the normalizer inlines the choice of their
	/// sources at every call site, distributing the capture over the branches, so the
	/// layers cost nothing at run time. What this test pins is that the collapse is
	/// invisible: values, ordered choice and refusals are exactly what the written tower
	/// means.
	/// </remarks>
	[Theory]
	[InlineData("a", 1)]
	[InlineData("b", 2)]
	[InlineData("c", 3)]
	public void A_transparent_tower_still_delivers(string input, int expected) =>
		Assert.Equal(expected, Parsed(
			"Start : @int = v: Outer => @(v)" + '\n' +
			"Outer : @int = o: Middle => @(o) | o: C => @(o)" + '\n' +
			"Middle : @int = m: A => @(m) | m: B => @(m)" + '\n' +
			"A : @int = 'a' => @(1)" + '\n' +
			"B : @int = 'b' => @(2)" + '\n' +
			"C : @int = 'c' => @(3)", input).Value);

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
		Assert.Equal(2, reported.Count);
		Assert.Equal(GramLexer.UnexpectedCharacter, reported[0].Id);
		Assert.Equal(GramParser.ExpectedDeclaration, reported[1].Id);
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
			"Start = n: ?=Word & when @(n.Length < 3) & Word\nWord = ['a'..'z']+",
			"ab"));

	[Fact]
	public void And_the_question_is_asked_of_what_was_seen() =>
		// The guard is what makes the capture worth having, so it has to be able to say no.
		Assert.False(Matches(
			"Start = n: ?=Word & when @(n.Length < 3) & Word\nWord = ['a'..'z']+",
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
	public void And_a_rule_may_ask_for_the_bounds_instead_of_the_text()
	{
		// §4.1 case 4's other half: `: @string` is the extent as text, `: @SourceSpan` is
		// the same extent as where it was. An ordinary construction whose expression is the
		// name §8.2 already supplies for it, so nothing in the machine is new.
		Assert.Equal(
			"2:2",
			Parsed(
				"Start : @string = ' '* & at: Word => @(at.Start + \":\" + at.Length)\n"
				+ "Word : @DotGram.SourceSpan = ['a'..'z']+",
				"  ab").Value);
	}

	[Fact]
	public void And_a_sequence_of_them_is_read_the_same_way()
	{
		// Extents are read out of the entries the rules left rather than out of the value
		// table, and a collected sequence is the path where that is least like the others:
		// the elements are walked in the arena and each one taken from its own entry.
		Assert.Equal(
			"0:2,3:1,5:3",
			Parsed(
				"@using System.Linq;\n" +
				"Start : @string = (words: Word & ' '?)+ " +
				"=> @(string.Join(\",\", words.Select(w => w.Start + \":\" + w.Length)))" + "\n" +
				"Word : @SourceSpan = ['a'..'z']+",
				"ab c def").Value);
	}

	[Fact]
	public void And_it_may_be_what_a_published_method_hands_back()
	{
		// It could not, once: everything emitted into a namespace has to be internal so that
		// two assemblies do not collide over it, and internal is what a public method may not
		// return. `SourceSpan` is emitted into the host class instead, where its name is the
		// host's — so it still cannot collide, and it can be handed over.
		var span = Published(
			"Start : @DotGram.SourceSpan = ' '* & ['a'..'z']+\nparse Start", "ParseStart", "  ab")!;

		Assert.Equal("SourceSpan", span.GetType().Name);
		Assert.Equal(0, span.GetType().GetProperty("Start")!.GetValue(span));
		Assert.Equal(4, span.GetType().GetProperty("Length")!.GetValue(span));
	}

	[Fact]
	public void And_not_by_asking_for_one_inside_something_else()
	{
		// A rule need not have a span for its value to hand one out: a construction can ask
		// for it and put it inside a type of its own, and what comes out is still an offset
		// into a window that will have moved.
		const string lines =
			"Word : @string = ['a'..'z']+\n" +
			"Start : @string[] = Line*\n" +
			"parse Start\n";

		Assert.Contains(
			"TextReader",
			Compile("Line : @string = w: Word & eol => @(w)\n" + lines).Sources[0].Text);
		Assert.DoesNotContain(
			"TextReader",
			Compile("Line : @string = w: Word & eol => @(parserSpan.Length.ToString())\n" + lines)
				.Sources[0].Text);
	}

	[Fact]
	public void And_only_where_the_publication_can_reach_it()
	{
		// What stops a stream is what the stream would run into. A grammar may hold a rule
		// that hands back a span and publish something that never calls it, and asking about
		// every rule in the file refused such a grammar for something it does not do.
		const string streamed =
			"Line : @string = w: Word & eol => @(w)\n" +
			"Word : @string = ['a'..'z']+\n" +
			"Start : @string[] = Line*\n" +
			"parse Start\n";

		Assert.Contains(
			"TextReader",
			Compile(streamed + "Elsewhere : @DotGram.SourceSpan = ['0'..'9']+").Sources[0].Text);
	}

	[Fact]
	public void A_span_cannot_be_handed_out_of_a_window_that_moves()
	{
		// It says where in the input it matched, and a streamed parse holds a window that
		// moves on — so the place it points at is gone before anyone could look. Refused
		// rather than materialized at the edge of the window: that would work and would stop
		// working silently, which is the failure this project is most careful about.
		const string lines =
			"Line : @string = w: Word & eol => @(w.Length.ToString())\n" +
			"Start : @string[] = Line*\n" +
			"parse Start\n";

		Assert.Contains("TextReader", Compile("Word : @string = ['a'..'z']+\n" + lines).Sources[0].Text);
		Assert.DoesNotContain(
			"TextReader",
			Compile("Word : @DotGram.SourceSpan = ['a'..'z']+\n" + lines).Sources[0].Text);
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
	public void A_rule_in_a_namespace_can_be_published() =>
		// §5 and §6 together: the directive reaches into a namespace by the qualified name,
		// and the method is named after the rule rather than after the path to it.
		Assert.Equal(
			"ab",
			Published("namespace Inner { Word = ['a'..'z']+ }\nparse Inner.Word", "ParseWord", "ab"));

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
		//
		// Asked of a specialization that keeps its value, because that is what makes it a
		// block in the emitted text at all — one that keeps nothing is compiled into each
		// of its callers, and then there is no block to count.
		var source = Compile(
			"""
			List(item, sep) : item[] = item & (sep & item)*
			Word : @string = t: ['a'..'z']+ => @(t)
			Comma = ','
			Start = List(Word, Comma) & ' ' & List(Word, Comma)
			parse Start
			""")
			.Sources[0].Text;

		// One rule, not two: a second specialization of the same arguments would be
		// `List_Word_Comma1`, and both call sites name the first.
		Assert.Contains("List_Word_Comma", source);
		Assert.DoesNotContain("List_Word_Comma1", source);
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
	public void The_list_of_4_2_collects_every_element_and_not_the_first()
	{
		// The line §4.2 prints as its example. The separated elements are inside a group,
		// and a sequence result used to take only the operands of the alternative itself —
		// so this collected one element and read as though it worked.
		var built = Built(
			"List(item, sep) : item[] = item & (sep & item)*\n" +
			"Word : @string = text: ['a'..'z']+ => @(text)\n" +
			"Start = words: List(Word, ',')",
			"ab,cd,ef");

		Assert.Equal(["ab", "cd", "ef"], (string[])Read(built, "Words")!);
	}

	[Fact]
	public void An_operand_captured_by_hand_is_not_collected_and_the_message_says_so()
	{
		// The likeliest way to write §4.1 case 2 wrong, because naming things is what a
		// grammar author does everywhere else. A sequence result collects the operands
		// nothing has spoken for, so a captured one leaves it with nothing — and the
		// message used to say no operand produces one, which is untrue of a rule that does.
		var reported = Compile(
			"""
			Feed : @string[] = rows: Row*
			Row : @string = text: ['a'..'z']+ & eol => @(text)
			parse Feed
			""").Diagnostics;

		Assert.Equal(GrammarNormalizer.UnbuiltConstruction, reported[0].Id);
		Assert.Contains("captured as 'rows'", reported[0].Message, StringComparison.Ordinal);
	}

	// ── What `recover` recovers from (§8.2) ─────────────────────────────────────

	[Fact]
	public void Recovery_steps_over_an_element_that_started_and_broke()
	{
		// `ab1` begins the way a row does and fails part way through, which is what a
		// malformed record looks like: the run knows an element was there and knows it was
		// refused.
		Assert.True(Matches(
			"Start = rows: Row* recover eol => @(\"!\") & eof\n"
			+ "Row : @string = t: ['a'..'z']+ & eol => @(t)",
			"aa\nab1\ncc\n"));
	}

	[Fact]
	public void And_not_from_a_line_that_never_began_one()
	{
		// `1bad` cannot start a row at all, so the repetition ends rather than breaks —
		// zero further iterations is a legitimate outcome for `*`, and what follows is
		// asked to match from there. Nothing tells this apart from a run that simply
		// finished, which is why recovery has nothing to step over.
		Assert.False(Matches(
			"Start = rows: Row* recover eol => @(\"!\") & eof\n"
			+ "Row : @string = t: ['a'..'z']+ & eol => @(t)",
			"aa\n1bad\ncc\n"));
	}

	// ── The rows the table calls refused (docs/status.md) ───────────────────────

	/// <summary>
	/// That what the status table calls refused really is, and by the diagnostic named.
	/// </summary>
	/// <remarks>
	/// The table is this project's answer to "what works", and three of its rows turned out
	/// stale in one week — two features built and never marked, one behaviour that was
	/// semantics rather than a gap. A row saying "refused" is a claim about the compiler and
	/// belongs under a test like any other.
	/// </remarks>
	[Theory]
	[InlineData(GrammarNormalizer.LeftRecursion,   "A = B & 'x' | 'a'\nB = A & 'y' | 'b'\nStart = A")]
	[InlineData(GrammarNormalizer.UnbuiltRecovery,
		"Start = a: Row* recover eol => @(1) & b: Row* recover eol => @(2)\n"
		+ "Row : @int = ['a'..'z']+ & eol => @(0)")]
	[InlineData(DotGram.Grammar.Binding.GrammarBinder.ParameterizedRebinding,
		"B(item) = item\nD = 'd'\nnamespace Ctx with (B = D) { }")]
	public void Still_refused(string expected, string grammar) => Refused(expected, grammar);

	// ── Atomic groups and what they carry out (§3.2) ────────────────────────────

	/// <summary>
	/// What a group recognised comes out of it; what could take the parse back into it does
	/// not.
	/// </summary>
	/// <remarks>
	/// The arena holds two unlike things, and commit is about one of them. It used to take
	/// the length off the end, which took both — so a capture written inside <c>{ … }</c>
	/// was thrown out with the choice beside it. The ways back are put out in place instead,
	/// because an entry's index is its name and closing the gaps would rename the records
	/// either side.
	/// </remarks>
	[Fact]
	public void An_atomic_group_carries_its_captures_out()
	{
		Assert.Equal("a",  Parsed("Start : @string = { x: \"a\" } => @(x)", "a").Value);
		Assert.Equal(
			"ab",
			Parsed(
				"Start : @string = { x: Child } => @(x)\n"
				+ "Child : @string = t: ['a'..'z']+ => @(t)",
				"ab").Value);
	}

	[Fact]
	public void And_out_of_every_turn_of_a_repetition()
	{
		// Committed once a turn, and each turn puts out its own ways back and leaves its own
		// records. What the turns before recorded is still there to be read at the end.
		Assert.Equal(
			"a|bb|ccc",
			Parsed(
				"@using System.Linq;\n" +
				"Start : @string = (xs: Word & ','?)+ => @(string.Join(\"|\", xs))\n" +
				"Word  : @string = { t: ['a'..'z']+ } => @(t)",
				"a,bb,ccc").Value);
	}

	[Fact]
	public void And_a_group_that_recognised_nothing_to_keep_leaves_nothing_behind()
	{
		// Nothing above the boundary is named by anything below it, so the length comes off
		// the arena as it always did. Under a repetition that is the difference between an
		// arena the grammar bounds and one the input does.
		Assert.True(Matches(
			"Start = ({ 'a'+ } & ','?)+",
			string.Join(",", Enumerable.Repeat("aaa", 20_000))));
	}

	// ── Keyword boundaries (§4.6) ────────────────────────────────────────────────

	[Fact]
	public void A_keyword_does_not_match_the_start_of_a_longer_word()
	{
		// §4.6: shadow the rule and every all-word literal picks up `& ?!wordboundary`.
		// Which literals qualify is decided when the grammar is built — `"if"` gets the
		// check and `"("` does not, since asking whether a letter follows a bracket would
		// refuse `(a)`.
		const string grammar =
			"wordboundary = ['a'..'z' | '0'..'9' | '_']\n"
			+ "Start = \"if\" & '(' & ['a'..'z']+ & ')'";

		Assert.True (Matches(grammar, "if(x)"));
		Assert.False(Matches(grammar, "iffy(x)"));
	}

	[Fact]
	public void And_without_the_rule_nothing_is_inserted() =>
		// Empty by default, so a grammar that never mentions it pays nothing — and keeps
		// the prefix match it always had.
		Assert.True(Matches("Start = \"if\" & ['a'..'z']*", "iffy"));

	[Fact]
	public void And_the_check_goes_before_the_trivia() =>
		// The other order would ask whether a letter follows the whitespace rather than
		// whether it follows the keyword, which is no question at all.
		Assert.False(Matches(
			"wordboundary = ['a'..'z']\ntrivia = ' '*\nStart = \"if\" & \"then\"",
			"iffy then"));

	// ── Trivia and repetition (§4.5) ─────────────────────────────────────────────

	[Fact]
	public void Trivia_goes_between_operands_and_not_between_iterations()
	{
		// §4.5 says between the operands of a sequence, and means it. The two cases look
		// alike and are not, which is worth a test of its own because the difference is
		// what keeps `['0'..'9']+` from reading "1 2" as one number in a grammar that
		// ignores spaces.
		Assert.True(Matches("trivia = ' '*\nStart = Word & Word\nWord = ['a'..'z']+", "ab cd"));
		Assert.False(Matches("trivia = ' '*\nStart = Word*\nWord = ['a'..'z']+", "ab cd"));
	}

	[Fact]
	public void And_a_spaced_list_says_so_with_the_rule_itself()
	{
		// `trivia` is an ordinary rule (§4.5), so a repetition that wants spacing names it.
		// Nothing new in the language and nothing special about the name.
		Assert.True(Matches(
			"trivia = ' '*\nStart = Word & (trivia & Word)*\nWord = ['a'..'z']+",
			"ab cd ef"));
	}

	/// <summary>
	/// A repetition of a valued rule is a collection, and a grammar that separates its
	/// operands separates its collections the same way (§4.5). Valuedness is the line:
	/// `Word*` above stays a lexeme-shaped run because `Word` builds nothing, while
	/// `Entry` here is the thing §4.1 case 2 gathers — and things are spaced.
	/// </summary>
	[Theory]
	[InlineData("a;b;",     true)]
	[InlineData("a; b;",    true)]
	[InlineData(" a; b; ",  true)]
	[InlineData("a ; b ;",  true)]
	public void A_collection_of_a_valued_rule_is_spaced(string input, bool expected)
	{
		const string collected =
			"trivia = ' '*\n" +
			"Entry : @string = t: ['a'..'z']+ & ';' => @(t)\n" +
			"Start : @string[] = (e: Entry)* & eof => @(e)";

		Assert.Equal(expected, Spaced(collected, input));

		// The bare form collects through §4.1 case 2's implicit capture and is the same
		// list; the hand-written seam is the same list once more, not seamed twice.
		const string bare =
			"trivia = ' '*\n" +
			"Entry : @string = t: ['a'..'z']+ & ';' => @(t)\n" +
			"Start : @string[] = Entry* & eof";

		Assert.Equal(expected, Spaced(bare, input));

		const string manual =
			"trivia = ' '*\n" +
			"Entry : @string = t: ['a'..'z']+ & ';' => @(t)\n" +
			"Start : @string[] = (trivia & e: Entry)* & eof => @(e)";

		Assert.Equal(expected, Spaced(manual, input));
	}

	/// <summary>
	/// Like <see cref="Matches"/>, but a spaced collection is told — accurately — that a
	/// streamed parse does not yet skip trivia between elements, and that information is
	/// not a defect of the grammar.
	/// </summary>
	static bool Spaced(string grammar, string input)
	{
		var result = Compile(grammar + "\nparse Start");

		Assert.DoesNotContain(
			result.Diagnostics,
			static diagnostic => diagnostic.Severity != GramSeverity.Info);

		return EmittedCode.Match(
			EmittedCode.Compile(Assert.Single(result.Sources).Text),
			"Grammar", "TryParseStart", input).IsSuccess;
	}

	[Theory]
	[InlineData("http",   true)]
	[InlineData("https",  true)]
	[InlineData("ftp",    true)]
	[InlineData("httpx",  false)]
	public void A_literal_a_later_one_continues_is_tried_first_and_come_back_for(string input, bool matches) =>
		// Ordered choice: `"http"` is preferred where both fit, and the longer one is
		// reachable because the parse comes back for it when the shorter leaves the input
		// unfinished. Both readings of `https` exist; which is answered is §11's business.
		Assert.Equal(
			matches,
			Matches("Start = QhttpQ | QhttpsQ | QftpQ".Replace("Q", "\""), input));

	[Theory]
	[InlineData("https",  true)]
	[InlineData("httpss", true)]
	[InlineData("http",   false)]
	public void And_comes_back_for_it_past_a_shorter_reading_that_did_not_work_out(string input, bool matches) =>
		// The case the way back exists for. On `httpss` the shorter alternative matches,
		// the `"s"` after the choice matches, and a character is left over — so the parse
		// returns to the choice and spends it on the longer alternative instead. The
		// entry it resumes at was written past the characters already matched, so what
		// runs there compares one character and not five.
		Assert.Equal(
			matches,
			Matches("Start = (QhttpQ | QhttpsQ) & QsQ".Replace("Q", "\""), input));

	[Theory]
	[InlineData("@int.Parse(d, CultureInfo.InvariantCulture)")]
	[InlineData("@int.Parse(d, @CultureInfo.InvariantCulture)")]
	[InlineData("@(int.Parse(d, CultureInfo.InvariantCulture))")]
	public void Everything_under_an_at_sign_is_the_consumer_s_own_C_sharp(string construction)
	{
		// One rule, three spellings of it. Whatever follows an `@` in a `=>` goes across as
		// text: the grammar does not look inside it, so a name there needs no `@` of its own
		// and is given none of the grammar's meanings either. `@Hold(x)` and `@(Hold(x))`
		// are the same construction written two ways, and this is what makes them so.
		//
		// This used to be the other way round — a bare name in an argument list was looked
		// up among rules and captures, so `CultureInfo.InvariantCulture` was refused there
		// and accepted one bracket away. What that bought was the grammar compiler catching
		// a mistyped capture in that one position. What it cost is the reason it is gone:
		// resolving C# means keeping up with C#, and every construct this compiler does not
		// know becomes a construct the language forbids for no reason of its own.
		Assert.Empty(Compile(
			"@using System.Globalization;\n"
			+ "Start : @int = d: ['0'..'9']+ => " + construction + "\n"
			+ "parse Start").Diagnostics);
	}

	[Fact]
	public void And_a_name_in_a_grammar_argument_list_still_is_one()
	{
		// The half that stays. Outside an `@` nothing changed: a call to a rule of the
		// grammar takes grammar names, and one that names nothing is found here rather
		// than in a file nobody wrote.
		var told = Assert.Single(Compile(
			"Pair(a, b) = a & ',' & b\nStart = Pair(Word, Missing)\nWord = ['a'..'z']+\n"
			+ "parse Start").Diagnostics);

		Assert.Contains("Missing", told.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void And_the_scalar_form_of_it_works_the_same_way()
	{
		// §4.2's `: item` is §4.1 case 3 said of a parameter, and is the same rewrite: the
		// operand the specialization was given becomes the value handed back.
		Assert.Equal(
			"ab",
			Parsed(
				"Lex(item) : item = ' '* & item\n"
				+ "Word : @string = text: ['a'..'z']+ => @(text)\n"
				+ "Start : Word = Lex(Word)",
				"  ab").Value);
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
	public void A_parameter_declared_as_a_C_sharp_type_takes_a_literal_of_that_type()
	{
		// §4.2: a C# type makes the parameter a value, and a value is a literal. The
		// specialization holds the one the call passed, so the C# the rule wrote reads
		// it — which is what "a value is allowed anywhere a value is expected" says, and
		// what used to emit a factory reading a name that does not exist.
		var result = Compile("""
			Padded(item, pad: char) : @string = t: item => @(t + pad)
			Word   : @string = w: ['a'..'z']+ => @(w)
			Marked : @string = m: Padded(Word, '!') => @(m)

			parse Marked
			""");

		Assert.Empty(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal("ab!", EmittedCode.Match(assembly, "Grammar", "TryParseMarked", "ab").Value);
	}

	[Fact]
	public void And_the_same_rule_specialized_twice_holds_two_values()
	{
		// The value is part of what a specialization is: two calls passing different
		// literals are two rules, not one shared one holding whichever came last.
		var result = Compile("""
			Mark(item, m: char) : @string = t: item => @(t + m)
			Word  : @string = w: ['a'..'z']+ => @(w)
			Bang  : @string = b: Mark(Word, '!') => @(b)
			Query : @string = q: Mark(Word, '?') => @(q)

			parse Bang
			parse Query
			""");

		Assert.Empty(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal("ab!", EmittedCode.Match(assembly, "Grammar", "TryParseBang",  "ab").Value);
		Assert.Equal("ab?", EmittedCode.Match(assembly, "Grammar", "TryParseQuery", "ab").Value);
	}

	[Fact]
	public void And_refuses_a_rule_where_a_value_was_declared() =>
		// The declaration says which kind the parameter is, and a rule is not a value —
		// taken as a recognizer, it would be the declaration meaning one thing to the
		// author and another to the compiler.
		Refused(
			GrammarNormalizer.UnbuiltCall,
			"Padded(item, pad: char) = item & @(pad)\nWord = ['a'..'z']+\nSpace = ' '\nStart = Padded(Word, Space)");

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
	[InlineData("Name = \"x\" | \"xy\"")]
	[InlineData("Name = \"xy\" | \"x\"")]
	public void Backtracking_crosses_a_rule_boundary(string name) =>
		Assert.True(Matches($"Start = Name & 'y'\n{name}", "xy"));

	[Fact]
	public void A_repetition_can_give_input_back_across_a_rule_boundary()
	{
		Assert.True(Matches("Start = Run & 'a' & 'b'\nRun = 'a'+", "aaab"));
		Assert.False(Matches("Start = { Run } & 'a' & 'b'\nRun = 'a'+", "aaab"));
	}

	[Fact]
	public void An_atomic_group_commits_a_called_rule_too() =>
		Assert.False(Matches("Start = { Name } & 'y'\nName = \"xy\" | \"x\"", "xy"));

	[Fact]
	public void And_the_same_expressions_in_one_rule_do_backtrack() =>
		Assert.True(Matches("Start = (\"xy\" | \"x\") & 'y'", "xy"));

	[Fact]
	public void An_atomic_group_discards_its_internal_choices_after_success()
	{
		Assert.False(Matches("Start = { \"xy\" | \"x\" } & 'y'", "xy"));
		Assert.True(Matches("Start = { \"xz\" | \"x\" } & 'y'", "xy"));
	}

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

	[Theory]
	[InlineData("http")]
	[InlineData("HTTP")]
	[InlineData("Http")]
	[InlineData("hTtP")]
	public void A_trailing_i_matches_any_case(string input) =>
		Assert.True(Matches("""Start = "http"i""", input));

	[Fact]
	public void A_trailing_i_still_refuses_a_different_word() =>
		Assert.False(Matches("""Start = "http"i""", "htttp"));

	[Theory]
	[InlineData("a")]
	[InlineData("A")]
	public void A_single_character_literal_takes_i_too(string input) =>
		Assert.True(Matches("Start = 'a'i", input));

	[Fact]
	public void A_digit_has_no_case_to_ignore_but_i_does_not_mind() =>
		Assert.True(Matches("Start = '5'i", "5"));

	[Fact]
	public void Without_i_the_literal_stays_case_sensitive() =>
		Assert.False(Matches("""Start = "http" """, "HTTP"));

	[Theory]
	[InlineData("htTP", true)]      // "ht" case-sensitive, "tp"i folded — this is what both allow
	[InlineData("HTtp", false)]     // "ht" wrong case — adjacent-literal merging must not have
	                                 // silently dropped the second literal's own `i`
	public void Adjacent_literals_of_different_case_sensitivity_do_not_merge(string input, bool expected) =>
		Assert.Equal(expected, Matches("""Start = "ht" & "tp"i""", input));

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
	public void The_position_is_the_character_that_did_not_fit_not_the_operand_s_start() =>
		// `"abcd"` is one operand and it starts at 0, but the character that did not fit
		// is at 2 — the offset is recorded at each failing test rather than one position
		// at the point of giving up.
		Assert.Equal(2, Refusal("""Start = "abcd" """, "abXY").Position);

	[Fact]
	public void The_same_sharpening_applies_inside_a_merged_literal_run() =>
		// "abcd"/"abef" share the prefix "ab" and compile through CompileLiterals's own
		// merged read rather than Node.Literal's loop — this input fails inside that
		// shared prefix itself (at 'X', index 1), which needs the identical fix rather
		// than a free ride off the other site.
		Assert.Equal(1, Refusal("""Start = "abcd" | "abef" """, "aXYZ").Position);

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

	// ── What was expected there (§11's first tier) ───────────────────────────────

	[Fact]
	public void A_refusal_names_the_one_thing_that_would_have_fit() =>
		// `a: 'a'` keeps the two literals from merging into one "a)"-shaped token during
		// normalization (adjacent bare literals in a sequence would otherwise become
		// one, which §11's own concerns start only after) — and unlike a repetition,
		// a plain capture tries its operand exactly once, so nothing else ties with ')'
		// at the position it fails.
		Assert.Equal("Expected ')'.", Refusal("Start = a: 'a' & ')'", "a").Error);

	[Fact]
	public void And_names_every_alternative_tried_at_the_same_furthest_position() =>
		// "ab"/"ac" share a prefix and are read once as one merged run (Machine.cs's
		// CompileLiterals) — both alternatives fail together, at the same position, and
		// both are named.
		Assert.Equal("Expected \"ab\" or \"ac\".", Refusal("""Start = "ab" | "ac" """, "ax").Error);

	[Fact]
	public void And_names_a_repeated_element_the_same_way_as_a_single_one() =>
		Assert.Equal("Expected ['0'..'9'].", Refusal("Start = ['0'..'9']+", "").Error);

	[Fact]
	public void With_nothing_left_to_try_it_says_the_input_ran_out() =>
		// The guard fails at the end of the input and names no terminal of its own; a
		// clear failure at a tied position does not erase what an earlier one recorded
		// there, so this only holds when nothing else was tried at that exact position.
		Assert.Equal(
			"Expected more input.",
			Refusal("Start = 'a' & when @(false)", "a").Error);

	[Fact]
	public void A_prefix_conflicted_run_reports_everything_it_covers()
	{
		// This used to under-report — an accepted first-cut gap, whose test said it
		// should change on purpose if ever fixed. The checkpoint class fixed it as a
		// side effect: the choice now records every alternative's failure the way the
		// engine's Fail: does, ties added rather than overwritten, so the "p"/"q" run
		// and "pr" are both named.
		Assert.Equal(
			"Expected ['p'..'q'] or \"pr\".",
			Refusal("""Start = "p" | "q" | "pr" """, "x").Error);
	}

	[Fact]
	public void A_case_insensitive_literal_names_itself_with_its_own_i() =>
		Assert.Equal("Expected \"http\"i.", Refusal("""Start = "http"i""", "xxxx").Error);

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

	[Theory]
	[InlineData("y",  null)]
	[InlineData("xy", "x")]
	public void An_optional_rule_capture_is_absent_or_its_value(string input, string? expected) =>
		Assert.Equal(
			expected,
			Read(Built("Item = letter: 'x'\nStart = (item: Item)? & 'y'", input), "Item", "Letter"));

	[Fact]
	public void An_optional_value_type_rule_capture_is_nullable()
	{
		const string grammar = "Item : @int = 'x' => @(1)\nStart = (item: Item)? & 'y'";

		Assert.Null(Read(Built(grammar, "y"), "Item"));
		Assert.Equal(1, Read(Built(grammar, "xy"), "Item"));
	}

	[Fact]
	public void A_rule_value_the_match_gave_back_is_not_materialized()
	{
		var value = Built(
			"Item = letter: 'x'\nStart = (item: Item & 'y' | item: Item & 'z')",
			"xz");

		Assert.Equal("x", Read(value, "Item", "Letter"));
	}

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
	public void An_atomic_discriminator_can_claim_an_element_without_consuming_input() =>
		Assert.Equal(
			["!X"],
			((Array)Read(
				Built("""
					Row   = { ?!"T|" } & "R|" & name: ['a'..'z']+ & eol
					Start = rows: Row* recover eol => @(new Row("!" + parserText)) & "T|" & eol
					""",
					"X\nT|\n"),
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
	public void A_synchronization_match_must_consume_input() =>
		Assert.Equal(
			["aa", "!b1b", "cc"],
			((Array)Read(
				Built("""
					Row   = name: ['a'..'z']+ & eol
					Start = rows: Row* recover (?='1' | eol) => @(new Row("!" + parserText))
					""",
					"aa\nb1b\ncc\n"),
				"Rows")!)
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
	public void A_recovering_repetition_asks_the_continuation_before_another_element() =>
		// At every element boundary the complete continuation gets first refusal. Only if
		// it fails does the repetition ask for another Row, so `tail` owns the second line.
		Assert.True(
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
		Assert.Contains("OnRecovered(\"Row\", text.Slice(recovered.Position, recovered.Value - recovered.Position).ToString(), recovered.Position, LineAt(text, recovered.Position), ColumnAt(text, recovered.Position), recovered.RuleIndex", source);

		// The elements that did match still collect — it is only the broken one that is
		// dropped — so what must be absent is a factory, not the collecting.
		Assert.Contains("captured0Count", source);
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
	public void Indirect_left_recursion_through_a_rule_that_does_something_is_still_refused() =>
		// `Other` is not only a name for what it forwards — one of its alternatives is a
		// literal of its own — so unfolding it would put that literal, and whatever else
		// an intermediary might carry, into the fold's tail. Refused, as §4.3 says.
		Refused(
			GrammarNormalizer.LeftRecursion,
			"""
			Start = Other & 'x'
			Other = Start | 'y'
			""");

	[Fact]
	public void And_so_is_a_mutual_recursion_where_both_sides_build() =>
		// The shape the general transform would need: `B`'s own operands and its `=>`
		// would join `A`'s tail, so the fold would have to apply two constructions in
		// order against an accumulator that is itself the result of one.
		Refused(
			GrammarNormalizer.LeftRecursion,
			"""
			N : @int = d: ['0'..'9']+ => @(int.Parse(d))
			A : @int = l: B & '-' & r: N => @(l - r) | v: N => @(v)
			B : @int = l: A & '+' & r: N => @(l + r) | v: N => @(v)
			""");

	[Fact]
	public void But_one_through_a_rule_that_only_forwards_is_made_direct()
	{
		// §4.3 over the layered shape every expression grammar is written in: `Call`
		// reaches itself through `Primary`, and `Primary` only forwards — so the leading
		// `Primary` is the choice of what it forwards, the alternative distributes over
		// it, and what is left is the direct recursion §4.3 already folds. Left-
		// associative, because that is where the recursion is: `7()()` is `(7())()`.
		var result = Compile("""
			Number  : @int = d: ['0'..'9']+ => @(int.Parse(d))
			Primary : @int = p: Call => @(p) | n: Number => @(n)
			Call    : @int = target: Primary & "()" => @(target * 10)

			parse Primary
			""");

		Assert.Empty(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal(7,   EmittedCode.Match(assembly, "Grammar", "TryParsePrimary", "7").Value);
		Assert.Equal(70,  EmittedCode.Match(assembly, "Grammar", "TryParsePrimary", "7()").Value);
		Assert.Equal(700, EmittedCode.Match(assembly, "Grammar", "TryParsePrimary", "7()()").Value);
	}

	[Fact]
	public void The_forwarder_itself_still_parses_on_its_own()
	{
		// Unfolding rewrites the recursive rule's own alternatives; the forwarder stays
		// in the grammar and means what it always did, publication included.
		var result = Compile("""
			Number  : @int = d: ['0'..'9']+ => @(int.Parse(d))
			Primary : @int = p: Call => @(p) | n: Number => @(n)
			Call    : @int = target: Primary & "()" => @(target * 10)

			parse Primary as Any
			parse Call as Applied
			""");

		Assert.Empty(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal(70, EmittedCode.Match(assembly, "Grammar", "TryApplied", "7()").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryApplied", "7").IsSuccess);
		Assert.Equal(7,  EmittedCode.Match(assembly, "Grammar", "TryAny", "7").Value);
	}

	[Fact]
	public void A_valueless_alias_makes_a_valueless_recursion_direct()
	{
		// The same unfolding where nothing builds: `Term` is a name for `List` and
		// `Word`, and `List` reaching itself through it is the loop it reads as.
		var result = Compile("""
			Word  = ['a'..'z']+
			Term  = List | Word
			List  = Term & ',' & Word

			parse List
			""");

		Assert.Empty(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseList", "a,b").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseList", "a,b,c").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseList", "a").IsSuccess);
	}

	// ── `when` guards (§8.1) ───────────────────────────────────────────────────

	[Theory]
	[InlineData("12",  true)]
	[InlineData("123", false)]
	public void A_guard_asks_a_question_of_the_text_so_far(string input, bool expected) =>
		Assert.Equal(expected, Matches("Start = ['0'..'9']+ & when @(parserText.Length < 3)", input));

	[Theory]
	[InlineData("ab", true)]
	[InlineData("ax", false)]
	public void And_of_the_captures_written_before_it(string input, bool expected) =>
		Assert.Equal(
			expected,
			Matches("""Start = a: 'a' & b: ['a'..'z'] & when @(b == "b")""", input));

	[Fact]
	public void A_failing_guard_is_a_non_match_and_a_sibling_is_tried() =>
		// Recognition failure: saying no sends the match back into the choice rather than
		// ending it.
		Assert.True(Matches(
			"""Start = (a: "ab" & when @(a == "xy") | a: "ab") & 'c'""",
			"abc"));

	[Fact]
	public void A_guard_may_stand_where_nothing_has_been_captured() =>
		Assert.True(Matches("Start = ['0'..'9']+ & when @(true)", "7"));

	// §7.1's C# recognizer contracts are fixed by syntactic position; their emitted calls
	// and ordinary C# diagnostics are tested with a host compilation in GeneratorDriverTests.

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
	public void A_rule_may_take_another_rules_value_as_its_own()
	{
		// §4.1 case 3: `A : B` says A's value is B's. The operand that produces it becomes
		// a capture and the rule hands it back — the same rewrite a sequence result uses,
		// one size down.
		Assert.Equal(
			42,
			Parsed(
				"Start : Number = ' '* & Number\n"
				+ "Number : @int = digits: ['0'..'9']+ => @int.Parse(digits)",
				"  42").Value);
	}

	[Fact]
	public void And_two_operands_that_could_be_it_are_refused() =>
		// Two answers and nothing to say which. A grammar to rewrite rather than a choice
		// for the compiler to make quietly.
		Refused(
			GrammarNormalizer.UnbuiltConstruction,
			"""
			Start : Number = Number & '+' & Number
			Number : @int = digits: ['0'..'9']+ => @int.Parse(digits)
			""");

	// ── Namespaces (§5) ──────────────────────────────────────────────────────────

	[Fact]
	public void Two_namespaces_may_each_have_a_rule_of_the_same_name() =>
		// Which is the whole point of a namespace, and which used to emit two C# methods
		// of the same name into the consumer's build. The namespaces a rule is declared in
		// are prefixed to the identifier it becomes.
		Assert.True(Matches(
			"""
			using Inner;

			namespace Inner
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
	public void A_namespace_shadows_Trivia_the_other_way_round(string input, bool expected) =>
		// trivia goes between the operands of every sequence, `Number`'s included. A
		// namespace that shadows it with `none` is how a rule says a space means something
		// here.
		Assert.Equal(
			expected,
			Matches(
				"""
				using Lexical;

				namespace Lexical
				{
					trivia = none

					Number = ['0'..'9']+ & ('.' & ['0'..'9']+)?
				}

				trivia = [' ']*
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

	// ── Namespace rebindings — §23 ───────────────────────────────────────────────

	[Fact]
	public void A_namespace_bound_publication_coexists_with_the_unbound_one()
	{
		// §23: the namespace specializes a use of `A`; it does not mutate `A` globally —
		// both publications have to exist side by side in the one generated parser.
		var result = Compile("""
			B = 'b'
			A = B

			parse A as DefaultA

			D = 'd'

			namespace Ns with (B = D)
			{
				parse A as NamespaceA
			}
			""");

		Assert.Empty(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryDefaultA", "b").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryDefaultA", "d").IsSuccess);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryNamespaceA", "d").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryNamespaceA", "b").IsSuccess);
	}

	[Fact]
	public void A_parameterized_rule_may_be_rebound_to_one_of_the_same_signature()
	{
		// §5.1 over §4.2: the binding replaces the rule a call named and keeps the
		// call's arguments — `A('a')` under `with (A = B)` is `B('a')`, and the plain
		// publication still reads the unrebound instantiation beside it.
		var result = Compile("""
			A(x) = '<' & x & '>'
			B(x) = '[' & x & ']'
			Start = A('a')

			parse Start with (A = B) as Swapped
			parse Start as Plain
			""");

		Assert.Empty(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TrySwapped", "[a]").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TrySwapped", "<a>").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryPlain", "<a>").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryPlain", "[a]").IsSuccess);
	}

	[Fact]
	public void A_rebound_parameterized_call_keeps_its_value_argument()
	{
		// A value parameter (§4.2) travels the same way a recognizer one does: the
		// replacement's specialization is built for the number the call already carried.
		var result = Compile("""
			D(n: int) = 'x'{n}
			E(n: int) = 'y'{n}
			Start = D(3)

			parse Start with (D = E) as Swapped
			""");

		Assert.Empty(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TrySwapped", "yyy").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TrySwapped", "xxx").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TrySwapped", "yy").IsSuccess);
	}

	[Fact]
	public void A_rebound_parameterized_argument_observes_its_sibling_bindings()
	{
		// The argument a call carried is spliced into the replacement's specialization,
		// and the same header's other bindings reach it there — the simultaneity §5.1
		// promises, through the instantiation.
		var result = Compile("""
			A(x)  = '<' & x & '>'
			B(x)  = '[' & x & ']'
			Inner = 'i'
			Other = 'o'
			Start = A(Inner)

			parse Start with (A = B, Inner = Other) as Swapped
			""");

		Assert.Empty(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TrySwapped", "[o]").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TrySwapped", "[i]").IsSuccess);
	}

	[Fact]
	public void A_namespace_header_reaches_a_parameterized_call_in_a_shared_rule()
	{
		// The same reach §5.1's own example shows for plain rules: `Mid` is declared
		// outside the namespace and never mentions `Spaced`, and the call graph reached
		// from inside still resolves its `List` through the binding.
		var result = Compile("""
			List(item, sep)   = item & (sep & item)*
			Spaced(item, sep) = item & ((sep | ' ') & item)*
			W   = ['a'..'z']+
			Mid = List(W, ',')

			namespace Ns with (List = Spaced)
			{
				parse Mid as Loose
			}

			parse Mid as Tight
			""");

		Assert.Empty(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryLoose", "a b,c").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryTight", "a b,c").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryTight", "a,b,c").IsSuccess);
	}

	[Fact]
	public void An_item_typed_parameterized_rule_carries_its_value_through_a_rebinding()
	{
		// `: item` (§4.2): both sides produce whatever the argument produces, and the
		// rebound instantiation hands the value back the same way the original did.
		var result = Compile("""
			Num : @int = d: ['0'..'9']+ => @(int.Parse(d))
			WrapA(item) : item = '<' & item & '>'
			WrapB(item) : item = '[' & item & ']'
			Start : @int = v: WrapA(Num) => @(v)

			parse Start with (WrapA = WrapB) as Bracketed
			""");

		Assert.Empty(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);
		var match    = EmittedCode.Match(assembly, "Grammar", "TryBracketed", "[7]");

		Assert.True(match.IsSuccess);
		Assert.Equal(7, match.Value);
	}

	[Fact]
	public void A_replacement_reached_through_a_binding_observes_its_sibling_bindings()
	{
		// §5.1: bindings in one header resolve simultaneously over the whole call graph
		// reached — and the replacement itself, reached only through the binding, is part
		// of that graph. `B` substituted for `A` must read `Sep` through the same header's
		// `Sep = Semi`, not as written. It did not, until the reachability walk learned to
		// follow the binding edge and a bound call learned to land on the replacement's
		// clone.
		var result = Compile("""
			Semi  = ';'
			Sep   = ','
			B     = 'b' & Sep
			A     = 'a'
			Start = A & Sep

			parse Start with (A = B, Sep = Semi) as Rebound
			parse Start as Plain
			""");

		Assert.Empty(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryRebound", "b;;").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryRebound", "b,;").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryPlain", "a,").IsSuccess);
	}

	[Fact]
	public void Lexical_Trivia_shadowing_does_not_reach_a_reused_outer_rule_but_a_namespace_binding_does()
	{
		// §22 test 12: `namespace { trivia = none }` is lexical and has no effect on `Pair`,
		// declared outside it and merely published from inside — `LexicalPair` behaves
		// exactly like `DefaultPair`. `namespace (trivia = none) { ... }` is a rebinding and
		// does reach `Pair` — `NamespacePair` rejects the space `DefaultPair` accepts.
		var result = Compile("""
			trivia = ' '*
			Pair   = A & B
			A      = 'a'
			B      = 'b'

			parse Pair as DefaultPair

			namespace Lex
			{
				trivia = none

				parse Pair as LexicalPair
			}

			namespace Ns with (trivia = none)
			{
				parse Pair as NamespacePair
			}
			""");

		Assert.Empty(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryDefaultPair", "a b").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryDefaultPair", "ab").IsSuccess);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryLexicalPair", "a b").IsSuccess);

		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryNamespacePair", "a b").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryNamespacePair", "ab").IsSuccess);
	}

	// ── `Expression with (A = B, ...)` — §5.1's substitution, expression-scoped ────

	const string RowGrammar =
		"Digit  = ['0'..'9']\n" +
		"Point  = '.'\n" +
		"Comma  = ','\n" +
		"Number = Digit+ & Point & Digit+\n" +
		"Start  = a: Number & ';' & b: Number & ';' & c: Number with (Point = Comma)";

	[Fact]
	public void With_rebinds_only_the_one_field_it_wraps() =>
		// A comma is accepted where the third field's decimal point goes — the first two
		// fields still take an ordinary '.' — because `with` scopes the rebinding to `c`
		// alone, not to `Number` everywhere it is called.
		Assert.True(Matches(RowGrammar, "1.2;3.4;5,6"));

	[Fact]
	public void And_the_first_two_fields_are_untouched_by_it() =>
		// The third field still requires a comma — a '.' there is refused, because it is
		// `c`'s own clone that was rebound, and the clone requires `Comma`.
		Assert.False(Matches(RowGrammar, "1.2;3.4;5.6"));

	[Fact]
	public void And_the_rebinding_does_not_leak_backward_into_the_earlier_fields() =>
		// The first field is still the plain, unrebound `Number` — a comma there is
		// refused exactly as it would be with no `with` in the grammar at all.
		Assert.False(Matches(RowGrammar, "1,2;3.4;5,6"));

	[Fact]
	public void A_publication_may_carry_its_own_with_header()
	{
		// The question that started this feature: `parse Sum with (trivia = none) as
		// Evaluate` alongside the ordinary, whitespace-tolerant publication of the same
		// rule — one directive, no block, no name for the substitution beyond the
		// publication's own. Mirrors what `namespace (trivia = none) { parse ... }`
		// already proved elsewhere, through the publication's own header instead.
		var result = Compile("""
			trivia = ' '*
			Pair   = A & B
			A      = 'a'
			B      = 'b'

			parse Pair as DefaultPair
			parse Pair with (trivia = none) as TightPair
			""");

		Assert.Empty(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryDefaultPair", "a b").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryDefaultPair", "ab").IsSuccess);

		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryTightPair", "a b").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryTightPair", "ab").IsSuccess);
	}
}
