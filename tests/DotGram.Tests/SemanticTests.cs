using System;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
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
	/// <summary>
	/// A name whose type arguments and whose invocation are both optional reads every way it
	/// can begin, and the compiler survives compiling it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The shape a grammar of a programming language always has, and the one that has broken
	/// the most: `Call` begins with a `Reference` and stands beside a bare `Reference`, and a
	/// `Reference` ends with an optional `&lt;…&gt;` whose first character also begins other
	/// things. `Type` reaches back to `Reference`, so the whole of it is a cycle with two
	/// optionals in it — which is where the analyses that decide what may be folded, lowered
	/// or read without a way back all have to agree.
	/// </para>
	/// <para>
	/// The inputs are the readings that separate them. `x` takes neither optional; `x&lt;y&gt;`
	/// takes the type arguments; `x&lt;` and `x&lt;&lt;` and `x&lt;&lt;1` offer a `&lt;` that
	/// is not the start of any; `x()` takes the invocation and not the arguments; `x&lt;y&gt;()`
	/// takes both. It asserts only that nothing throws — the grammar compiles and the parse
	/// runs — because what it is guarding against is a compiler defect rather than a wrong
	/// answer, and every one it has caught has been one.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("x")]
	[InlineData("x<y>")]
	[InlineData("x<<")]
	[InlineData("x<")]
	[InlineData("x<<1")]
	[InlineData("x()")]
	[InlineData("x<y>()")]
	public void A_name_with_optional_arguments_reads_every_way_it_can(string input)
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
	public void A_separated_list_is_spaced_on_every_turn(string input, bool expected)
	{
		Assert.Equal(expected, Matches(
			"trivia = [' ']*" + '\n' +
			"Start = A & (',' & A)*" + '\n' +
			"A = ['a'..'z']+".AsDotGram(), input));
	}

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
	public void A_repetition_of_one_thing_is_a_lexeme(string input, bool expected)
	{
		Assert.Equal(expected, Matches(
			"trivia = [' ']*" + '\n' +
			"Start = W*" + '\n' +
			"W = ['a'..'z']", input));
	}

	/// <summary>Digits, the example §4.5 gives: <c>1 2</c> is two numbers, not one.</summary>
	[Theory]
	[InlineData("123", true)]
	[InlineData("1 2", false)]
	public void Digits_are_a_lexeme_too(string input, bool expected)
	{
		Assert.Equal(expected, Matches(
			"trivia = [' ']*" + '\n' +
			"Start = ['0'..'9']+", input));
	}

	/// <summary>An optional has no second turn, so it has no seam to space.</summary>
	[Theory]
	[InlineData("ab", true)]
	[InlineData("a b", true)]
	[InlineData("a", true)]
	public void An_optional_is_left_alone(string input, bool expected)
	{
		Assert.Equal(expected, Matches(
			"trivia = [' ']*" + '\n' +
			"Start = 'a' & 'b'?", input));
	}

	/// <summary>
	/// Spacing a repetition of a single thing is the case that cannot be inferred, and is
	/// still written out (§4.5).
	/// </summary>
	[Theory]
	[InlineData("a a a", true)]
	[InlineData("aaa", true)]
	public void A_spaced_repetition_of_one_thing_still_says_so(string input, bool expected)
	{
		Assert.Equal(expected, Matches(
			"trivia = [' ']*" + '\n' +
			"Start = A & (trivia & A)*" + '\n' +
			"A = ['a'..'z']", input));
	}

	// ── §4.5: `~`, where trivia may not go ──────────────────────────────────

	/// <summary>
	/// The angle brackets, which is what <c>~</c> was written for.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A shift is two <c>&gt;</c> with nothing between them and a nested type argument
	/// list closes with two that are each their own; a language with both cannot spell
	/// the shift <c>"&gt;&gt;"</c>, because a token cannot be half spent. Written as
	/// <c>'&gt;' ~ '&gt;'</c> there is no <c>&gt;&gt;</c> to make, and what tells the two
	/// apart is the gap: C++ needed a space here until C++11 and C# never did.
	/// </para>
	/// <para>
	/// Asked of both halves, because <c>~</c> means one thing and a grammar must read the
	/// same language whether or not it is lexically split. Over characters it is the seam
	/// the normalizer withheld; over kinds the trivia was skipped before the tokens were
	/// made, so the same statement is a question about where each token began.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData(true,  "a >> b",              true)]
	[InlineData(true,  "a>>b",                true)]
	[InlineData(true,  "a > > b",             false)]
	[InlineData(true,  "list<list<int>>",     true)]
	[InlineData(true,  "list<list<int> >",    true)]
	[InlineData(false, "a >> b",              true)]
	[InlineData(false, "a>>b",                true)]
	[InlineData(false, "a > > b",             false)]
	[InlineData(false, "list<list<int>>",     true)]
	[InlineData(false, "list<list<int> >",    true)]
	public void Glue_tells_a_shift_from_two_closing_brackets(bool lexical, string input, bool expected)
	{
		var angles =
			"trivia = { ' '* }" + '\n' +
			"namespace Lexical" + '\n' +
			"{" + '\n' +
			"\ttrivia = none" + '\n' +
			"\tName = ['a'..'z']+" + '\n' +
			"}" + '\n' +
			"Type = Lexical.Name & ('<' & Type & (',' & Type)* & '>')?" + '\n' +
			"Expr = Expr & '>' ~ '>' & Prim | Prim" + '\n' +
			"Prim = Lexical.Name" + '\n' +
			"Start = Type & eof | Expr & eof" + '\n' +
			"parse Start";

		var result = GramCompiler.Compile(
			angles,
			new GramCompilerOptions
			{
				ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Lexical = lexical,
			});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity != GramSeverity.Info);

		var match = EmittedCode.Match(
			EmittedCode.Compile(result.Sources[0].Text), "Grammar", "TryParseStart", input);

		Assert.Equal(expected, match.IsSuccess);
	}

	/// <summary>
	/// A left-recursive rule that builds nothing folds without a value on either carrier. The
	/// immediate carrier keeps a register for each type its machine builds, and once wrote the
	/// fold's value into one for the text a rule with no type would have, which no machine built:
	/// the generator stopped at a value table the file did not keep.
	/// </summary>
	[Theory]
	[InlineData(CarrierKind.Tape)]
	[InlineData(CarrierKind.Immediate)]
	public void A_fold_that_builds_nothing_reads_on_either_carrier(CarrierKind carrier)
	{
		const string grammar =
			"""
			Type  = Name & ('<' & Type & '>')?
			Name  = ['a'..'z']+
			Expr  = Expr & '>' & Prim | Prim
			Prim  = Name
			Start = Type & eof | Expr & eof
			parse Start
			""";

		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions
			{
				ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Carrier = carrier,
			});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "a>b>c").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "list<int>").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "a>").IsSuccess);
	}

	/// <summary>
	/// A left-recursive rule whose value is the record its captures make, with no <c>=&gt;</c>
	/// on any alternative, builds that record on the engine as the direct reader does. The
	/// engine once folded it like a rule with constructions: a fold over none, an empty
	/// <c>switch</c> (CS1522, a failed build under warnings as errors) and a value never set.
	/// </summary>
	[Theory]
	[InlineData(CarrierKind.Auto)]
	[InlineData(CarrierKind.Tape)]
	[InlineData(CarrierKind.Immediate)]
	public void A_fold_with_no_construction_builds_its_record_on_the_engine(CarrierKind carrier)
	{
		const string grammar =
			"""
			E = E & '+' & N | c: N & '=' & N
			N = ['0'..'9']+
			parse E
			""";

		string Value(bool direct, string input)
		{
			var result = GramCompiler.Compile(
				grammar,
				new GramCompilerOptions
				{
					ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Carrier = carrier, Direct = direct,
				});

			EmittedCode.Quiet(result.Diagnostics);

			var match = EmittedCode.Match(EmittedCode.Compile(result.Sources[0].Text), "Grammar", "TryParseE", input);

			Assert.True(match.IsSuccess, match.Error);

			var value = Assert.IsAssignableFrom<object>(match.Value);

			return (string)value.GetType().GetProperty("C")!.GetValue(value)!;
		}

		foreach (var (input, expected) in new[] { ("12=3", "12"), ("12=3+4", "12"), ("12=3+4+56", "12") })
		{
			Assert.Equal(expected, Value(direct: false, input));
			Assert.Equal(expected, Value(direct: true,  input));
		}
	}

	// ── A split half the methods refuse is told it will backtrack ────────────

	/// <summary>
	/// Over kinds a rule's answer stands (§4), and it is the methods that say it. A
	/// syntactic half the methods cannot write runs on the engine, which backtracks —
	/// so the grammar is told, and told which rule cost it that.
	/// </summary>
	/// <remarks>
	/// <c>recover</c> is the shape used here because it is the plainest of the several
	/// the direct rendering still hands back. Over characters the same grammar says
	/// nothing: backtracking is what reading characters means.
	/// </remarks>
	[Theory]
	[InlineData(false, false)]
	[InlineData(true,  true)]
	public void A_split_half_the_methods_refuse_is_told_it_backtracks(bool lexical, bool warned)
	{
		var feed = string.Join(
			"\n",
			"trivia = { ' '* }",
			"namespace Lexical",
			"{",
			"\ttrivia = none",
			"\tName = ['a'..'z']+",
			"}",
			"Row   = Lexical.Name & ';'",
			"Start = Row* recover ';' & eof",
			"parse Start");

		var result = GramCompiler.Compile(
			feed,
			new GramCompilerOptions
			{
				ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Lexical = lexical,
			});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		var backtracks = result.Diagnostics.Where(one => one.Id == "GRAM5005").ToArray();

		Assert.Equal(warned, backtracks.Length > 0);

		if (warned)
			Assert.Contains("recovers from a bad element", backtracks[0].Message, StringComparison.Ordinal);
	}

	// ── A split grammar keeps every call site's own strength ─────────────────

	/// <summary>
	/// Binding powers over kinds. The operand inside the parentheses is read at full
	/// strength, whatever the operator around it binds at — which is what a call site of
	/// its own means (§4.3.1).
	/// </summary>
	/// <remarks>
	/// It was not, and only over kinds: the lexical split rewrites every node, and the map
	/// from what a node was to what it became was keyed by structure, so two calls to the
	/// same rule were one key. Every call took the strength of whichever site was rewritten
	/// last, and <c>(a + b) * c</c> was read with the parenthesized operand at the strength
	/// of the <c>*</c> around it, which refuses the <c>+</c>. Over characters the same
	/// grammar was always right, which is why both halves are asked here.
	/// </remarks>
	[Theory]
	[InlineData(false, "(a + b) * c")]
	[InlineData(true,  "(a + b) * c")]
	[InlineData(false, "a * (b + c)")]
	[InlineData(true,  "a * (b + c)")]
	[InlineData(false, "a + b * c")]
	[InlineData(true,  "a + b * c")]
	public void A_call_site_of_a_climbing_rule_keeps_its_own_strength(bool lexical, string input)
	{
		var arithmetic = string.Join(
			"\n",
			"trivia = { ' '* }",
			"namespace Lexical",
			"{",
			"\ttrivia = none",
			"\tName = ['a'..'z']+",
			"}",
			"Start = Expr & eof",
			"Expr = Expr & '+' & Expr << 1",
			"     | Expr & '*' & Expr << 2",
			"     | Prim",
			"Prim = Lexical.Name | '(' & Expr & ')'",
			"parse Start");

		var result = GramCompiler.Compile(
			arithmetic,
			new GramCompilerOptions
			{
				ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Lexical = lexical,
			});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity != GramSeverity.Info);

		var match = EmittedCode.Match(
			EmittedCode.Compile(result.Sources[0].Text), "Grammar", "TryParseStart", input);

		Assert.True(match.IsSuccess, $"{input} was refused.");
	}

	// ── Over kinds a rule's answer stands, unless the rule says otherwise ─────

	/// <summary>
	/// Over characters a loop gives a turn back when what follows fails; over kinds it
	/// does not (§4), so the path's last part is the loop's and the call never comes —
	/// unless the rule is marked <c>?</c>, which lets it give back inside itself.
	/// </summary>
	[Theory]
	[InlineData(false, false, true)]
	[InlineData(false, true,  true)]
	[InlineData(true,  false, false)]
	[InlineData(true,  true,  true)]
	public void Over_kinds_a_rule_answer_stands_unless_it_is_marked(bool lexical, bool marked, bool expected)
	{
		var path =
			"trivia = { ' '* }" + '\n' +
			"namespace Lexical" + '\n' +
			"{" + '\n' +
			"\ttrivia = none" + '\n' +
			"\tName = ['a'..'z']+" + '\n' +
			"}" + '\n' +
			$"Path{(marked ? "?" : "")} = Lexical.Name & ('.' & Lexical.Name)* & '.' & Lexical.Name & '(' & ')'" + '\n' +
			"Start = Path & eof" + '\n' +
			"parse Start";

		var result = GramCompiler.Compile(
			path,
			new GramCompilerOptions
			{
				ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Lexical = lexical,
			});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity != GramSeverity.Info);

		var match = EmittedCode.Match(
			EmittedCode.Compile(result.Sources[0].Text), "Grammar", "TryParseStart", "a.b.c()");

		Assert.Equal(expected, match.IsSuccess);
	}

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
	public void A_settled_repetition_still_rematches_its_body(string input, bool expected)
	{
		Assert.Equal(expected, Matches(
			"Start = (\"ab\" | \"a\")* & 'b'", input));
	}

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
		bool atomic, string input, bool expected)
	{
		Assert.Equal(expected, Matches(
			(atomic
				? "trivia = { (' ' | \"//\" & [^ '\\n']*)* }"
				: "trivia = (' ' | \"//\" & [^ '\\n']*)*") + '\n' + "Start = 'x' & 'y'", input));
	}

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
	public void A_rematched_turn_is_counted_once(string input, bool expected)
	{
		Assert.Equal(expected, Matches(
			"Start = ({ ['a'|'c'] } | 'a' | ('b' | 'a' | \"b\")){2}", input));
	}

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
	public void A_transparent_tower_still_delivers(string input, int expected)
	{
		Assert.Equal(expected, Parsed(
			"Start : @int = v: Outer => @(v)" + '\n' +
			"Outer : @int = o: Middle => @(o) | o: C => @(o)" + '\n' +
			"Middle : @int = m: A => @(m) | m: B => @(m)" + '\n' +
			"A : @int = 'a' => @(1)" + '\n' +
			"B : @int = 'b' => @(2)" + '\n' +
			"C : @int = 'c' => @(3)", input).Value);
	}

	static bool Matches(string grammar, string input)
	{
		return Parsed(grammar, input).IsSuccess;
	}

	static (bool IsSuccess, object? Value, string? Error, long Position) Parsed(
		string grammar, string input, string? expected = null)
	{
		var result = Compile(grammar + "\nparse Start");

		EmittedCode.Quiet(
			expected is null
				? result.Diagnostics
				: result.Diagnostics.Where(one => one.Id != expected).ToArray());

		return EmittedCode.Match(
			EmittedCode.Compile(result.Sources[0].Text), "Grammar", "TryParseStart", input);
	}

	static GramCompilation Compile(string grammar)
	{
		return GramCompiler.Compile(
		grammar,
		new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });
	}

	/// <summary>
	/// The same, with C# types answered by Roslyn rather than taken on trust: what a type check
	/// between two rules' results needs to be asked for real.
	/// </summary>
	static GramCompilation CompileResolved(string grammar)
	{
		var host = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
			"Host",
			[Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText("public partial class Grammar { }")],
			EmittedCode.References);

		return GramCompiler.Compile(
		grammar,
		new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = new RoslynSymbolResolver(host, "Grammar"),
		});
	}

	/// <summary>The graph itself, where what a test asks about is not a diagnostic.</summary>
	static RecognitionGraph Graph(string grammar)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(grammar, RoslynCSharpScanner.Instance)).File));
	}

	/// <summary>The one diagnostic a grammar must be refused with.</summary>
	// ── One mistake, one message about it ────────────────────────────────────────

	/// <summary>A feed with a single stray character in the middle of one rule.</summary>
	const string OneStrayCharacter =
		"Row     = \"R\" & '|' & name: Text & eol\n" +
		"Text    = [^ '|']+\n" +
		"Feed    = header: Header & rows: Row* & eof\n" +
		"Header  = \"H\" % '|' & date: Digit{4} & eol\n" +
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
	public void And_a_negative_one_saw_nothing()
	{
		// §3.4: "?!X produces nothing" — it succeeded because what it looked for was not
		// there, so there is nothing to have seen.
		Assert.Equal(
			"",
			Parsed("Start : @string = seen: ?!'z' & Word => @(seen)\nWord = ['a'..'z']+", "ab")
				.Value);
	}

	[Fact]
	public void And_the_specification_example_works()
	{
		// §3.6, written out: look ahead, name what was seen, ask a question of it, then
		// read it for real.
		Assert.True(Matches(
			"Start = n: ?=Word & when @(n.Length < 3) & Word\nWord = ['a'..'z']+",
			"ab"));
	}

	[Fact]
	public void And_the_question_is_asked_of_what_was_seen()
	{
		// The guard is what makes the capture worth having, so it has to be able to say no.
		Assert.False(Matches(
			"Start = n: ?=Word & when @(n.Length < 3) & Word\nWord = ['a'..'z']+",
			"abcd"));
	}

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

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text)
			.GetType("Grammar")!
			.GetMethod(method, [typeof(string)])!
			.Invoke(null, [input]);
	}

	[Fact]
	public void Either_directive_may_be_renamed()
	{
		// §6: `as` is on both, and only `parse as` had a test.
		Assert.Single(
			(System.Collections.IEnumerable)Published(
				"Word = ['a'..'z']+\nfind Word as AllWords", "AllWords", "ab")!);
	}

	[Fact]
	public void A_rule_in_a_namespace_can_be_published()
	{
		// §5 and §6 together: the directive reaches into a namespace by the qualified name,
		// and the method is named after the rule rather than after the path to it.
		Assert.Equal(
			"ab",
			Published("namespace Inner { Word = ['a'..'z']+ }\nparse Inner.Word", "ParseWord", "ab"));
	}

	[Fact]
	public void A_find_of_a_rule_that_matches_nothing_ends()
	{
		// `['a'..'z']*` matches the empty string everywhere, so a `find` that took the
		// match and did not move would answer for ever. It moves.
		var found = (System.Collections.IEnumerable)Published(
			"Maybe = ['a'..'z']*\nfind Maybe", "FindMaybe", "..")!;

		Assert.Equal(3, found.Cast<object>().Count());
	}

	/// <summary>Two alternatives beginning alike, where beginning twice compounds.</summary>
	/// <remarks>
	/// `Primary` leads back to `Power` through its parentheses, so reading it once for each
	/// alternative doubles at every level of nesting. The shipped calculator example was
	/// written this way: sixteen parentheses deep took 30 ms, and reading the operand once
	/// takes 0.05 ms.
	/// </remarks>
	[Fact]
	public void A_shared_beginning_that_leads_back_to_its_rule_is_reported()
	{
		Refused(
			GrammarNormalizer.SharedPrefix,
			"""
			Power   : @int = left: Primary & '^' & right: Power => @(left + right)
			               | value: Primary                    => @(value)
			Primary : @int = '(' & inner: Power & ')'           => @(inner)
			               | d: ['0'..'9']+                    => @(int.Parse(d))
			""");
	}

	[Fact]
	public void And_the_same_written_with_the_rest_optional_is_not()
	{
		Accepted(
			"""
			Power   : @int = left: Primary & ('^' & right: Power)? => @(left + (right ?? 0))
			Primary : @int = '(' & inner: Power & ')'              => @(inner)
			               | d: ['0'..'9']+                        => @(int.Parse(d))
			""");
	}

	[Fact]
	public void And_a_shared_beginning_that_leads_nowhere_back_is_said_too()
	{
		// Splitting the last segment off a path: two alternatives are the only way to say
		// it, and the operand gives back so the tail fits. This used to go unreported on
		// the grounds that nothing nests here and so nothing doubles per level — until a
		// profile of `ExpressionParser` found a flat eleven readings of one operand
		// costing most of a parse. Flat is still worth saying.
		Reported(
			"""
			Name     = ['a'..'z']+
			Segments = Name & ('/' & Name)*
			Start    = d: Segments & '/' & f: Name | d: Segments
			""");
	}

	[Fact]
	public void And_one_whose_beginning_cannot_give_back_is_not()
	{
		// A literal has one reading, so the two orders hold the same one thing and there is
		// nothing to weigh — which is also why the emitter has always factored these.
		Accepted("Start = \"ab\" & 'c' | \"ab\" & 'd'");
	}

	static void Accepted(string grammar)
	{
		var diagnostics = Compile(grammar).Diagnostics;

		Assert.DoesNotContain(
			diagnostics, diagnostic => diagnostic.Id == GrammarNormalizer.SharedPrefix);
	}

	static void Reported(string grammar)
	{
		var diagnostics = Compile(grammar).Diagnostics;

		Assert.Contains(
			diagnostics, diagnostic => diagnostic.Id == GrammarNormalizer.SharedPrefix);
	}

	// ── A `with` cycle, which has no order (GRAM4017) ───────────────────────────

	/// <summary>
	/// Two rules whose `with` sites reach each other are refused rather than ordered.
	/// </summary>
	/// <remarks>
	/// A `with` rebinds what a rule actually resolves to, so `R2 = R1 with (C = D)` has to
	/// be specialized after whatever `R1`'s own `with` did to it. Where each wants the other
	/// first there is no such order, and the pass used to settle it by whichever rule the
	/// loop reached first — document order. Moving a rule in the file changed what the
	/// parser did.
	/// </remarks>
	[Fact]
	public void Two_rules_whose_with_sites_reach_each_other_are_refused()
	{
		Refused(
			GrammarNormalizer.CircularWith,
			"""
			A = B with (X = Y)
			B = A with (X = Z)
			X = 'x'
			Y = 'y'
			Z = 'z'
			Start = A
			""");
	}

	/// <summary>And the answer does not depend on which of them is written first.</summary>
	/// <remarks>
	/// The test the defect deserved: not "is it refused" but "is the same thing said either
	/// way round". A pass that settles a cycle by document order passes the first assertion
	/// and fails this one.
	/// </remarks>
	[Fact]
	public void And_it_is_refused_whichever_of_them_is_written_first()
	{
		const string Tail =
			"""
			X = 'x'
			Y = 'y'
			Z = 'z'
			Start = A
			""";

		Refused(GrammarNormalizer.CircularWith, "A = B with (X = Y)\nB = A with (X = Z)\n" + Tail);
		Refused(GrammarNormalizer.CircularWith, "B = A with (X = Z)\nA = B with (X = Y)\n" + Tail);
	}

	// ── A `with` expression and a namespace header, in one order (§5.1) ────────────

	/// <summary>
	/// A <c>with</c> expression around a call into a namespace with a header composes on top of
	/// the header, as a publication's <c>with</c> does; a <c>with</c> a header reaches is
	/// specialized before the header, as it always was.
	/// </summary>
	[Theory]
	[MemberData(nameof(WithsAndHeaders))]
	public void A_with_expression_composes_with_a_namespace_header(
		string rule, string accepted, string refused, CarrierKind carrier, bool direct)
	{
		var result = GramCompiler.Compile(
			WithAndHeader(rule),
			new GramCompilerOptions
			{
				ClassName     = "Grammar",
				Carrier       = carrier,
				Direct        = direct,
				CSharpScanner = RoslynCSharpScanner.Instance,
			});

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(
			result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static source => source.Text));

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", accepted).IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", refused).IsSuccess);
	}

	public static TheoryData<string, string, string, CarrierKind, bool> WithsAndHeaders()
	{
		var data = new TheoryData<string, string, string, CarrierKind, bool>();

		const string EG  = "namespace Ns with (B = D) { E = B  G = E & Z }\n";
		const string ZQ  = "Z = 'z'\nQ = 'q'\n";

		(string Rule, string Accepted, string Refused)[] shapes =
		[
			// The header is kept under a `with` that names none of its rules.
			(ZQ + EG + "R = Ns.G with (Z = Q)", "dq", "bq"),
			// The `with` replaces what the header put in place.
			("Y = 'y'\nnamespace Ns with (B = D) { E = B }\nR = Ns.E with (D = Y)", "y", "d"),
			// The `with` replaces what the header already replaced: nothing is left to replace.
			("Y = 'y'\nnamespace Ns with (B = D) { E = B }\nR = Ns.E with (B = Y)", "d", "y"),
			// At any depth.
			(ZQ + EG + "X = Ns.G\nR = X with (Z = Q)", "dq", "bq"),
			// A `with` inside the namespace whose rules it reaches: as before.
			(ZQ + "namespace Ns with (B = D) { E = B  G = E & Z  H = G with (Z = Q) }\nR = Ns.H", "dq", "bq"),
			// A `with` reached from one namespace's header, calling into another's.
			(ZQ + "Y = 'y'\n" + EG + "W = Ns.G with (Z = Q)\nnamespace N1 with (Q = Y) { K = W }\nR = N1.K", "dy", "dq"),
			// An instance a `with` rebuilds passes what the original passed: the specialization.
			("Y = 'y'\nL(x) = x\nF(x) = x & '!'\nnamespace Ns with (B = D) { E = B }\nR = L(Ns.E) with (L = F, D = Y)", "y!", "d!"),
			// A `with` on a bracketed sequence inside a larger one, on either side of it.
			("A = 'a'\nC = 'c'\nR = (A & B) with (A = C) & D", "cbd", "abd"),
			("A = 'a'\nC = 'c'\nR = ((A & B) with (A = C)) & D", "cbd", "abd"),
			("A = 'a'\nC = 'c'\nR = D & (A & B) with (A = C)", "dcb", "dab"),
			("Y = 'y'\nZ = 'z'\nP = 'p'\nnamespace Ns with (B = D) { E = B  G = Z }\nR = (Ns.E & Ns.G) with (D = Y, Z = P) & D", "ypd", "dzd"),
			// A parameterized replacement taken from a namespace is read as that namespace makes it.
			("Y = 'y'\nZ = 'z'\nL(x) = x\nnamespace Ns with (B = D) { F(x) = x & B }\nU = B\nparse U\nR = L(Z) with (L = Ns.F, D = Y)", "zy", "zb"),
			// Everything stacks: an outer `with` over an inner one rebinds what the inner introduced,
			// and a binding of a name the inner already replaced does nothing.
			("Y = 'y'\nP = 'p'\nX = D\nU = B\nparse U\nR = (X with (D = Y)) with (Y = P)", "p", "y"),
			("Y = 'y'\nZ = 'z'\nX = D\nU = B\nparse U\nR = (X with (D = Y)) with (D = Z)", "y", "z"),
			// A caller's header over a call into another namespace, as a `with` there would be.
			("V = 'v'\nnamespace Ns with (B = D) { E = B }\nW = Ns.E\nnamespace M with (D = V) { K = W }\nR = M.K", "v", "d"),
			("V = 'v'\nnamespace Ns with (B = D) { E = B }\nR = Ns.E with (D = V)", "v", "d"),
			// A nested header resolves with what it inherits, and replaces an inherited binding of the same name.
			("C = 'c'\nnamespace O with (B = D) { namespace M with (D = C) { E = B } }\nR = O.M.E", "c", "d"),
			("C = 'c'\nnamespace O with (B = D) { namespace M with (B = C) { E = B } }\nR = O.M.E", "c", "d"),
			("C = 'c'\nnamespace O with (B = D) { E = B  namespace M with (B = C) { F = E } }\nR = O.M.F", "c", "d"),
			("C = 'c'\nY = 'y'\nnamespace O with (B = D) { namespace I with (B = C) { E = B }  R2 = I.E with (C = Y) }\nR = O.R2", "y", "d"),
			// The target of a deeper header rebound where the graph also calls it directly.
			("Y = 'y'\nnamespace Ns with (B = D) { G = B & D }\nR = Ns.G with (D = Y)", "yy", "dd"),
			// Parameterized: the outer `with` rebinds the rule the inner put in place and the arguments it carried.
			("Y = 'y'\nA2(x) = 'a' & x\nB2(x) = 'b' & x\nC2(x) = 'c' & x\nU = B\nparse U\nR = (A2(D) with (A2 = B2)) with (B2 = C2, D = Y)", "cy", "bd"),
			// A header's replacement that is another namespace's rule is read as that namespace makes it.
			("C = 'c'\nY = 'y'\nnamespace Ns1 with (B = Ns2.F) { E = B }\nnamespace Ns2 with (C = D) { F = C }\nR = Ns1.E with (D = Y)", "y", "d"),
			// An argument from outside that the header does not rebind, and one inside that it does.
			("Y = 'y'\nnamespace Ns with (B = D) { L(x) = x & B }\nR = Ns.L(Y)", "yd", "yb"),
			("namespace Ns with (B = D) { L(x) = x & B }\nR = Ns.L(D)", "dd", "bd"),
			("namespace Ns with (B = D) { L(x) = x & B  G = L(B) }\nR = Ns.G", "dd", "bd"),
			("Y = 'y'\nnamespace Ns with (B = D) { L(x) = x & B }\nM(x) = Ns.L(x)\nR = M(Y)", "yd", "yb"),
			// A `with` on a rule of its own, passed as the argument, in place of one inside the argument.
			("Y = 'y'\nL(x) = x & B\nX = D with (D = Y)\nR = L(X)", "yb", "db"),
			// An outer `with` over an inner one on an operand of its own: the outer rebinds what the inner's
			// clone put in place, through a quantifier or a sequence around the inner.
			("A = 'a'\nC = 'c'\nX = A\nU = D\nparse U\nR = ((X with (A = B)) & '!') with (B = C)", "c!", "b!"),
			("A = 'a'\nC = 'c'\nX = A\nU = D\nparse U\nR = (X with (A = B))+ with (B = C)", "cc", "bb"),
			("A = 'a'\nC = 'c'\nX = A\nR = ((((X with (A = B)) & '!') with (B = C)) & '?') with (C = D)", "d!?", "c!?"),
			("C = 'c'\nL(x) = x\nF(x) = B & x\nU = B & D\nparse U\nR = ((L('!') with (L = F)) & '?') with (B = C)", "c!?", "b!?"),
			// Two sibling `with`s under one: neither is over the other's call, so nothing loops.
			("Y = 'y'\nZ = 'z'\nU = Y\nparse U\nR = ((B with (B = D)) & (D with (D = B))) with (Y = Z)", "db", "bd"),
			// An inner `with` sent `D` on already: the outer's binding of `D` binds nothing, and does not loop.
			("Y = 'y'\nnamespace N with (B = D) { X = B }\nR = ((N.X with (D = Y)) & '!') with (D = B)", "y!", "b!"),
			// An argument passed on by a rule outside, and the caller's header binding it as the callee's does.
			("Y = 'y'\nnamespace Ns with (B = Y) { L(x) = x }\nM(x) = Ns.L(x)\nnamespace O with (B = Y) { R2 = M(B) }\nR = O.R2 & D", "yd", "bd"),
			("Y = 'y'\nnamespace Ns with (B = Y) { L(x) = x }\nM(x) = Ns.L(x & '!')\nnamespace O with (B = Y) { R2 = M((B & 'a')) }\nR = O.R2 & D", "ya!d", "ba!d"),
			// The caller's header rebinds the argument to a rule the callee's does not touch.
			("Y = 'y'\nnamespace Ns with (D = Y) { L(x) = x }\nnamespace O with (B = D) { R2 = Ns.L(B) }\nR = O.R2", "d", "y"),
			// An instance another call made first, passed as the argument: this call's own argument is what counts.
			("Y = 'y'\nP(x) = x\nnamespace Ns with (B = Y) { L(x) = x }\nnamespace Warm { W = P(B) }\nnamespace O with (B = Y) { R2 = Ns.L(P(B)) }\nR = Warm.W & O.R2 & D", "byd", "bbd"),
			// A rule that calls itself with its arguments the other way round, called twice.
			("namespace Ns with (B = D) { L(x) = x }\nA(x, w) = ('a' & A(w, x)) | Ns.L(x)\nnamespace O with (B = D) { R2 = A(B, B) & A(B, B) }\nR = O.R2", "dd", "bb"),
			// Glue shared by two unrelated operands is not an argument passed on.
			("Y = 'y'\nnamespace Ns with (B = Y) { L(x) = x }\nM(x) = Ns.L('a' ~ 'c') & x\nnamespace O with (B = D) { R2 = M(B ~ '!') }\nnamespace P { R3 = M(B ~ '!') }\nR = O.R2 & P.R3", "acd!acb!", "acy!acy!"),
			// A `with` over a header it does not loop with.
			("Y = 'y'\nnamespace Ns with (B = D) { E = B & Y }\nR = Ns.E with (Y = B)", "db", "dy"),
			// An argument written inside the namespace, rebound by a `with` from outside to a rule of it: the
			// header has rebound the argument already, and the rule is read with it so.
			("L(x) = x\nnamespace Ns with (B = D) { G = L(B)  F(x) = x & B }\nR = Ns.G with (L = Ns.F)", "dd", "bd"),
			// An argument the caller's own header rebinds as the nested header it is passed into does.
			("Y = 'y'\nZ = 'z'\nU = Z\nparse U\nnamespace O with (B = D) { namespace Ns with (Z = Y) { L(x) = x }  R2 = Ns.L(B) }\nR = O.R2", "d", "b"),
			// Rebound to a rule of another namespace whose header does not touch what the argument means.
			("Y = 'y'\nL(x) = x\nnamespace Ns with (B = D) { G = L(B) }\nnamespace N2 with (B = Y) { F(x) = x & B }\nR = Ns.G with (L = N2.F)", "dy", "by"),
		];

		foreach (var (rule, accepted, refused) in shapes)
			foreach (var (carrier, direct) in new[] { (CarrierKind.Auto, true), (CarrierKind.Tape, false), (CarrierKind.Immediate, true) })
				data.Add(rule, accepted, refused, carrier, direct);

		return data;
	}

	/// <summary>
	/// A <c>with</c> that replaces a parameterized rule replaces the instance the header cloned.
	/// </summary>
	[Fact]
	public void A_with_expression_replaces_an_instance_a_namespace_header_cloned()
	{
		var result = Compile(
			"""
			using Ns;

			B = 'b'
			D = 'd'
			F(x) = 'f' & x

			namespace Ns with (B = D)
			{
				E(x) = B & x
				G = E('!')
			}

			R = Ns.G with (E = F)
			parse R as ParseStart
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "f!").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "d!").IsSuccess);
	}

	/// <summary>
	/// A <c>with</c> that replaces its own operand reads its other bindings in the replacement
	/// too, as a publication's <c>with</c> does.
	/// </summary>
	[Theory]
	[InlineData("R = Ns.E with (E = F, D = Y)\nparse R as ParseStart")]
	[InlineData("R = Ns.E\nparse R with (E = F, D = Y) as ParseStart")]
	public void A_with_that_replaces_its_operand_rebinds_inside_the_replacement(string rule)
	{
		var result = Compile(
			"""
			using Ns;

			B = 'b'
			D = 'd'
			Y = 'y'

			namespace Ns with (B = D)
			{
				E = B
				F = B & '!'
			}

			""" + rule);

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "y!").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "d!").IsSuccess);
	}

	/// <summary>
	/// The same-name case reads what <c>parse … with</c> reads, and says the same about it.
	/// </summary>
	[Fact]
	public void A_with_expression_over_a_rule_the_header_replaced_agrees_with_a_publications()
	{
		const string Header = "Y = 'y'\nnamespace Ns with (B = D) { E = B }\n";

		var expression  = Compile(WithAndHeader(Header + "R = Ns.E with (B = Y)"));
		var publication = Compile(WithAndHeader(Header + "R = Ns.E", publish: "parse R with (B = Y) as ParseStart"));

		Assert.Equal(
			publication.Diagnostics.Select(one => one.Id).Order(StringComparer.Ordinal),
			expression.Diagnostics.Select(one => one.Id).Order(StringComparer.Ordinal));

		var a = EmittedCode.Compile(expression.Sources[0].Text);
		var b = EmittedCode.Compile(publication.Sources[0].Text);

		foreach (var input in new[] { "b", "d", "y" })
			Assert.Equal(
				EmittedCode.Match(b, "Grammar", "TryParseStart", input).IsSuccess,
				EmittedCode.Match(a, "Grammar", "TryParseStart", input).IsSuccess);
	}

	/// <summary>
	/// A header that reaches a <c>with</c> whose operand calls back into the namespace from
	/// outside it has no order to be specialized in, and is refused like a <c>with</c> cycle.
	/// </summary>
	[Fact]
	public void A_header_and_a_with_that_reach_each_other_are_refused()
	{
		Refused(
			GrammarNormalizer.CircularWith,
			"""
			B = 'b'
			D = 'd'
			Z = 'z'
			Q = 'q'

			namespace Ns with (B = D)
			{
				F = B & Z
				E = '[' & W & ']'
			}

			W = Ns.F with (Z = Q)
			R = Ns.E
			parse R
			""");
	}

	/// <summary>
	/// A <c>with</c> inside a namespace goes before the namespace's header, so a header that
	/// replaces what the <c>with</c> already replaced has nothing left to do, and there is no
	/// cycle to refuse.
	/// </summary>
	[Fact]
	public void A_with_inside_a_namespace_goes_before_its_header()
	{
		var result = Compile(
			"""
			B = 'b'
			Y = 'y'
			X = Ns.H

			namespace Ns with (B = X)
			{
				H = B with (B = Y)
			}

			parse Ns.H as ParseStart
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "y").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "b").IsSuccess);
	}

	/// <summary>
	/// GRAM4014 for a <c>with</c> expression reads the rules the <c>with</c> actually replaces:
	/// the header put <c>D</c>'s decimal in <c>B</c>'s place, and a decimal replacing <c>D</c>
	/// fits there, as it does under <c>parse … with</c>.
	/// </summary>
	[Fact]
	public void A_with_expressions_replacement_is_checked_against_what_the_header_put_in_place()
	{
		var result = CompileResolved(
			"""
			B : @int     = 'b' => @(1)
			D : @decimal = 'd' => @(2m)
			V : @decimal = 'v' => @(3m)

			namespace Ns with (B = D)
			{
				E : B = B
			}

			R : @decimal = x: (Ns.E with (D = V)) => @(x)
			parse R as ParseStart
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal(3m, EmittedCode.Match(assembly, "Grammar", "TryParseStart", "v").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "d").IsSuccess);
	}

	/// <summary>
	/// A replacement whose type does not fit where a copy of the replaced rule's value lands is
	/// refused as GRAM4014, whichever way the rebinding is written — not reported later, on
	/// the name of a clone the author never wrote.
	/// </summary>
	[Theory]
	[InlineData("R : @decimal = x: (Ns.E with (D = S)) => @(x)\nparse R as ParseStart")]
	[InlineData("R : @decimal = x: Ns.E => @(x)\nparse R with (D = S) as ParseStart")]
	public void A_replacement_that_changes_the_type_a_fixed_type_reads_is_refused(string rule)
	{
		var result = CompileResolved(
			"""
			B : @int     = 'b' => @(1)
			D : @decimal = 'd' => @(2m)
			S : @string  = 's' => @("s")

			namespace Ns with (B = D)
			{
				E : B = B
			}

			""" + rule);

		Assert.Contains(result.Diagnostics, one => one.Id == GrammarNormalizer.IncompatibleRebinding);
		Assert.DoesNotContain(result.Diagnostics, one => one.Message.Contains("_With", StringComparison.Ordinal));
	}

	/// <summary>
	/// A rule of the name the parser gives an expression on the right of a rebinding is
	/// reported at the expression, in words about the expression.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_rule_named_like_a_rebinding_expressions_rule_is_reported_at_the_expression(bool before)
	{
		const string Mine = "Sp_With1 = 'x'\n";

		var grammar =
			(before ? Mine : "") +
			"""
			Sp = ' '
			L  = 'a' & Sp & 'b'

			namespace N with (Sp = '_')
			{
				M = L
				parse M as ParseStart
			}

			""" +
			(before ? "" : Mine);

		var diagnostics = Compile(grammar).Diagnostics;
		var expression  = grammar.IndexOf("'_'", StringComparison.Ordinal);

		var duplicate = Assert.Single(diagnostics, one => one.Id == GrammarBinder.DuplicateRule);

		Assert.Equal(expression, duplicate.Position);
		Assert.Contains("replaces 'Sp'", duplicate.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// A rule whose type is a namespace's rule, under a <c>with</c> that changes that type,
	/// declares what it now builds.
	/// </summary>
	[Fact]
	public void A_rule_typed_by_a_namespaces_rule_follows_a_with_expressions_replacement()
	{
		var result = CompileResolved(
			"""
			B : @object = 'b' => @((object)1)
			D : @object = 'd' => @((object)2)
			V : @string = 'v' => @("v")

			namespace Ns with (B = D)
			{
				E : B = B
			}

			X : Ns.E = Ns.E
			R : @string = x: (X with (D = V)) => @(x)
			parse R as ParseStart
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal("v", EmittedCode.Match(assembly, "Grammar", "TryParseStart", "v").Value);
	}

	/// <summary>
	/// What a header makes of a rule does not depend on the order the grammar declares its
	/// <c>with</c>s in: a <c>with</c> reads a namespace's clones without editing the rules
	/// another header specializes later.
	/// </summary>
	[Fact]
	public void A_header_reads_the_same_whatever_order_the_withs_are_declared_in()
	{
		const string Head =
			"""
			B = 'b'
			D = 'd'
			Y = 'y'
			Z = 'z'
			Q = 'q'
			P = 'p'

			namespace Ns with (B = D) { E = B }

			X = Ns.E & Z

			namespace Other with (B = Y) { H = X }

			""";

		const string S = "S = Other.H with (Y = P)\n";
		const string R = "R = X with (Z = Q)\n";
		const string Tail = "parse Other.H as ParseStart\nparse S\nparse R\n";

		var first  = EmittedCode.Compile(Compile(Head + S + R + Tail).Sources[0].Text);
		var second = EmittedCode.Compile(Compile(Head + R + S + Tail).Sources[0].Text);

		foreach (var input in new[] { "yz", "dz", "bz" })
			Assert.Equal(
				EmittedCode.Match(first, "Grammar", "TryParseStart", input).IsSuccess,
				EmittedCode.Match(second, "Grammar", "TryParseStart", input).IsSuccess);
	}

	/// <summary>
	/// A rule that reads the original keeps the original's type, and a publication's
	/// <c>with</c> that changes the type of its own copy is not refused on its account.
	/// </summary>
	[Fact]
	public void A_type_changing_publication_with_is_not_refused_for_a_reader_of_the_original()
	{
		var result = CompileResolved(
			"""
			B : @int    = 'b' => @(1)
			D : @string = 'd' => @("d")
			R : B = B
			F : @object = x: R => @(x)

			parse R with (B = D) as Swapped
			parse F as ParseF
			parse F with (B = D) as Widened
			""");

		Assert.DoesNotContain(result.Diagnostics, one => one.Id == GrammarNormalizer.IncompatibleRebinding);
	}

	/// <summary>
	/// A <c>with</c> that replaces its operand by a rule holding a <c>with</c> of its own reads
	/// that rule with its own <c>with</c> applied, whichever is declared first.
	/// </summary>
	[Theory]
	[InlineData("R = A with (A = B, Z = Q)\nB = Z with (Z = Y)\n")]
	[InlineData("B = Z with (Z = Y)\nR = A with (A = B, Z = Q)\n")]
	public void A_replaced_operand_is_specialized_before_the_with_that_replaces_it(string rules)
	{
		var result = Compile("A = 'a'\nZ = 'z'\nQ = 'q'\nY = 'y'\n" + rules + "parse R as ParseStart\n");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "y").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "q").IsSuccess);
	}

	/// <summary>
	/// A reader past the first is held to the type the replacement gives the rule it reads,
	/// a sequence of it where that rule is a sequence.
	/// </summary>
	[Fact]
	public void A_reader_of_a_sequence_that_follows_the_replacement_is_held_to_the_sequence()
	{
		var result = CompileResolved(
			"""
			B : @int    = 'b' => @(1)
			D : @string = 'd' => @("d")
			R : B[] = B+
			F : @System.Collections.IList = x: R => @(x)

			parse F as ParseF
			parse F with (B = D) as Widened
			""");

		Assert.DoesNotContain(result.Diagnostics, one => one.Id == GrammarNormalizer.IncompatibleRebinding);
	}

	/// <summary>
	/// A <c>with</c> that rebuilds an instance a header cloned passes the arguments the header
	/// made, and runs the <c>with</c>s the replacement holds.
	/// </summary>
	[Theory]
	[InlineData("F(x) = x", "G = E(B)", "d", "b")]
	[InlineData("F(x) = (Z with (Z = Y)) & x", "G = E('!')", "y!", "z!")]
	public void A_with_that_rebuilds_a_cloned_instance_reads_what_the_header_made(string replacement, string g, string accepted, string refused)
	{
		var result = Compile(
			$$"""
			using Ns;

			B = 'b'
			D = 'd'
			Z = 'z'
			Y = 'y'
			{{replacement}}

			namespace Ns with (B = D)
			{
				E(x) = B & x
				{{g}}
			}

			R = Ns.G with (E = F)
			parse R as ParseStart
			""");

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", accepted).IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", refused).IsSuccess);
	}

	/// <summary>
	/// A reader that expects the replaced rule's type is refused whether it reads the rule
	/// itself or a rule following it, whatever its action does with the value — and the
	/// refusal names the reader, the rule it reads and both types.
	/// </summary>
	[Theory]
	[InlineData("F : @int = x: R => @(x.GetHashCode())", "'F' reads 'R'")]
	[InlineData("F : @int = x: B => @(x.GetHashCode())", "'F' reads 'B'")]
	public void A_reader_expecting_the_replaced_type_is_refused_by_name(string reader, string named)
	{
		var result = CompileResolved(
			$$"""
			B : @int    = 'b' => @(1)
			D : @string = 'd' => @("d")
			R : B = B
			{{reader}}

			parse R
			parse F with (B = D) as Swapped
			""");

		var refusal = Assert.Single(result.Diagnostics, one => one.Id == GrammarNormalizer.IncompatibleRebinding);

		Assert.Contains(named, refusal.Message, StringComparison.Ordinal);
		Assert.Contains("'int'", refusal.Message, StringComparison.Ordinal);
		Assert.Contains("'string'", refusal.Message, StringComparison.Ordinal);
		Assert.Contains("Declare 'F' as ':", refusal.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// A header's own GRAM4014 check asks after the readers of copies of the rule it replaces
	/// among the rules it cloned, as a <c>with</c>'s does.
	/// </summary>
	[Fact]
	public void A_header_that_changes_a_type_a_copy_is_read_as_is_refused()
	{
		var result = CompileResolved(
			"""
			D  : @int    = 'd' => @(1)
			Vs : @string = 'v' => @("v")
			E  : D = D
			W  : @int = x: E => @(x)

			namespace M with (D = Vs)
			{
				K : @int = x: W => @(x)
			}

			parse M.K as ParseStart
			""");

		var refusal = Assert.Single(result.Diagnostics, one => one.Id == GrammarNormalizer.IncompatibleRebinding);

		Assert.Contains("'W' reads 'E'", refusal.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// A parameterized replacement from a namespace keeps the header in a publication's
	/// <c>with</c> too.
	/// </summary>
	[Fact]
	public void A_publications_parameterized_replacement_from_a_namespace_keeps_its_header()
	{
		var result = Compile(
			"""
			B = 'b'
			D = 'd'
			Y = 'y'
			Z = 'z'
			L(x) = x

			namespace Ns with (B = D)
			{
				F(x) = x & B
			}

			R = L(Z)
			parse R with (L = Ns.F, D = Y) as ParseStart
			""");

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "zy").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "zb").IsSuccess);
	}

	/// <summary>
	/// An instance a header rebuilt keeps the header's arguments when a publication's
	/// <c>with</c> rebuilds it again, as a <c>with</c> expression's does.
	/// </summary>
	[Theory]
	[InlineData("R = Ns.E\nparse R with (F = G) as ParseStart")]
	[InlineData("R = Ns.E with (F = G)\nparse R as ParseStart")]
	public void A_rebuilt_instance_keeps_the_arguments_its_header_made(string rule)
	{
		var result = Compile(
			"""
			B = 'b'
			D = 'd'
			L(x) = 'l' & x
			F(x) = 'f' & x
			G(x) = 'g' & x

			namespace Ns with (L = F, B = D)
			{
				E = L(B)
			}

			""" + rule);

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "gd").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "gb").IsSuccess);
	}

	/// <summary>
	/// A cycle through two namespaces names, for each <c>with</c>, the namespace it calls into.
	/// </summary>
	[Fact]
	public void A_cycle_through_two_namespaces_names_the_namespace_each_with_calls_into()
	{
		var diagnostics = Compile(
			"""
			B = 'b'
			D = 'd'
			Z = 'z'
			Q = 'q'
			Y = 'y'

			namespace N1 with (B = D) { E1 = '[' & G1 & ']' | B }
			namespace N2 with (Z = Q) { E2 = '<' & N1.E1 & '>' | Z }

			G1 = N2.E2 with (Y = B)
			R = N1.E1
			parse R
			""").Diagnostics.Where(one => one.Id == GrammarNormalizer.CircularWith).ToList();

		Assert.NotEmpty(diagnostics);

		foreach (var cycle in diagnostics.Where(one => one.Message.StartsWith("'G1'", StringComparison.Ordinal)))
			Assert.True(
				cycle.Message.StartsWith("'G1'", StringComparison.Ordinal)
					? cycle.Message.Contains("namespace 'N2'", StringComparison.Ordinal)
					: cycle.Message.Contains("namespace 'N1'", StringComparison.Ordinal),
				cycle.Message);
	}

	/// <summary>
	/// Diagnostics about a rule a specialization made name the rule the author wrote: an
	/// instance as the call that made it, a copy as the rule it copies.
	/// </summary>
	[Fact]
	public void A_refusal_names_an_instance_as_the_call_that_made_it()
	{
		var result = CompileResolved(
			"""
			B : @int    = 'b' => @(1)
			D : @string = 'd' => @("d")
			E(x) : B = B & x
			R : @int = y: E('!') => @(y.GetHashCode())

			parse R
			parse R with (B = D) as Swapped
			""");

		var refusal = Assert.Single(result.Diagnostics, one => one.Id == GrammarNormalizer.IncompatibleRebinding);

		Assert.Contains("'E('!')'", refusal.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void A_construction_refusal_does_not_name_a_copy()
	{
		var diagnostics = Compile(
			"""
			A = 'a'
			C = 'c'
			X : @int = A
			R = X with (A = C)
			parse R
			""").Diagnostics;

		Assert.Contains(diagnostics, one => one.Id == GrammarNormalizer.UnbuiltConstruction);
		Assert.DoesNotContain(diagnostics, one => one.Message.Contains("_With", StringComparison.Ordinal));
	}

	/// <summary>
	/// Substitutions that loop once stacked are refused as one header that loops is.
	/// </summary>
	[Theory]
	[InlineData("X = D\nR = (X with (D = Y)) with (Y = D)\nparse R")]
	[InlineData("namespace N with (D = Y, Y = D) { X = D }\nparse N.X")]
	[InlineData("namespace O with (B = Y) { namespace M with (Y = B) { X = B } }\nparse O.M.X")]
	// A `with` over the header of a namespace its operand calls into is one header with it.
	[InlineData("namespace N with (B = D) { X = B }\nR = N.X with (D = B)\nparse R")]
	[InlineData("namespace N with (B = D) { X = B }\nR = (N.X with (D = Y)) with (Y = B)\nparse R")]
	[InlineData("namespace N with (B = D) { X = B }\nW = N.X\nR = W with (D = B)\nparse R")]
	// Through more than one step: one header `with (B = D, Y = Z, D = Y, Z = B)` loops through four.
	[InlineData("Z = 'z'\nnamespace N with (B = D, Y = Z) { X = B & Y }\nR = N.X with (D = Y, Z = B)\nparse R")]
	[InlineData("Z = 'z'\nX = B & Y\nR = (X with (B = D, Y = Z)) with (D = Y, Z = B)\nparse R")]
	// On an operand of its own around the inner `with`, the same stacking.
	[InlineData("X = B\nR = ((X with (B = D)) & '!') with (D = B)\nparse R")]
	// A second call beside the inner `with`, with nothing between it and the outer, loops with the header.
	[InlineData("namespace N with (B = D) { X = B }\nR = ((N.X with (D = Y)) & N.X) with (D = B)\nparse R")]
	// Around an operand that calls nothing, the stacking is the same.
	[InlineData("R = (('x' with (B = D)) & '!') with (D = B)\nparse R")]
	public void Substitutions_that_loop_once_stacked_are_refused(string rules)
	{
		Refused(GrammarBinder.CircularRebinding, "B = 'b'\nD = 'd'\nY = 'y'\n" + rules + "\n");
	}

	/// <summary>
	/// A <c>with</c> inside an instance made only while another <c>with</c> is specialized is
	/// refused rather than dropped.
	/// </summary>
	[Fact]
	public void A_with_inside_an_instance_made_during_specialization_is_refused()
	{
		Refused(GrammarNormalizer.LateWith, LateInstance);
	}

	/// <summary>
	/// The same refusal wherever such an instance is made: without a namespace, by a
	/// publication's <c>with</c>, one call further in, inside an instance that reaches itself,
	/// and for a <c>with</c> on a <c>with</c> — each at the place it is written, none dropped.
	/// </summary>
	[Theory]
	[InlineData("F(x) = (Z with (Z = Y)) & x\nE(x) = B & x\nG = E(B)\nR = G with (E = F)\nparse R", 1)]
	[InlineData("F(x) = (Z with (Z = Y)) & x\nE(x) = B & x\nG = E(B)\nparse G\nparse G with (E = F) as Swapped", 1)]
	[InlineData("H(x) = (Z with (Z = Y)) & x\nF(x) = H(x) & '!'\nE(x) = B & x\nG = E(B)\nR = G with (E = F)\nparse R", 1)]
	[InlineData("F(x) = (Z with (Z = Y)) & x & ('.' & F(x))?\nE(x) = B & x\nG = E(B)\nR = G with (E = F)\nparse R", 1)]
	[InlineData("F(x) = ((Z with (Z = Y)) with (Y = W)) & x\nE(x) = B & x\nG = E(B)\nR = G with (E = F)\nparse R", 2)]
	public void A_with_inside_an_instance_made_during_specialization_is_refused_however_it_is_made(string rules, int count)
	{
		const string names = "B = 'b'\nZ = 'z'\nY = 'y'\nW = 'w'\n";

		var grammar = names + rules + "\n";
		var refused = Compile(grammar).Diagnostics.Where(one => one.Id == GrammarNormalizer.LateWith).ToArray();

		Assert.Equal(count, refused.Length);
		Assert.All(refused, one => Assert.StartsWith("Z with", grammar.Substring(one.Position).TrimStart('('), StringComparison.Ordinal));
	}

	const string LateInstance =
		"""
		using Ns;

		B = 'b'
		D = 'd'
		Z = 'z'
		Y = 'y'
		F(x) = (Z with (Z = Y)) & x

		namespace Ns with (B = D)
		{
			E(x) = B & x
			G = E(B)
		}

		R = Ns.G with (E = F)
		parse R
		""";

	/// <summary>
	/// The refusal names what the author can see: the instance, the call as written and the
	/// substitution that replaced its rule.
	/// </summary>
	[Fact]
	public void A_late_with_refusal_names_the_call_as_written()
	{
		var refusal = Assert.Single(Compile(LateInstance).Diagnostics, one => one.Id == GrammarNormalizer.LateWith);

		Assert.Contains("inside 'F(D)'", refusal.Message, StringComparison.Ordinal);
		Assert.Contains("the 'with' in 'R' replaced 'E' with 'F' in the call 'E(B)'", refusal.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// An instance a header makes while a publication's <c>with</c> reads through it is the
	/// header's, and the refusal says so.
	/// </summary>
	[Fact]
	public void A_late_with_refusal_names_the_header_that_made_the_instance()
	{
		var refusal = Assert.Single(
			Compile(
				"""
				B = 'b'
				Z = 'z'
				Y = 'y'
				E(x) = x
				F(x) = (Z with (Z = Y)) & x
				L(x) = x
				namespace Ns with (E = F) { G(x) = E(x) }
				R = L(B)
				parse R
				parse R with (L = Ns.G) as Swapped
				""").Diagnostics,
			one => one.Id == GrammarNormalizer.LateWith);

		Assert.Contains("the header of namespace 'Ns' replaced 'E' with 'F' in the call 'E(B)'", refusal.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// An argument written outside a namespace that names a rule its header rebinds is refused
	/// rather than read as the header rebinds it: at any depth of the argument, through a
	/// parameter that passes it on, under a header the namespace inherits, and where a
	/// rebinding — a <c>with</c> or a publication's — is what makes the call one into the
	/// namespace.
	/// </summary>
	[Theory]
	[InlineData("namespace Ns with (B = D) { L(x) = x & B }\nR = Ns.L(B)")]
	[InlineData("P(y) = y\nnamespace Ns with (B = D) { L(x) = x & B }\nR = Ns.L(P(B))")]
	[InlineData("namespace Ns with (B = D) { L(x) = x & B }\nR = Ns.L((Y & B))")]
	[InlineData("namespace Ns with (B = D) { L(x) = x & B }\nR = Ns.L(B?)")]
	[InlineData("namespace Ns with (B = D) { L(x) = x & B }\nM(x) = Ns.L(x)\nR = M(B)")]
	[InlineData("namespace O with (B = D) { namespace Ns { L(x) = x & B } }\nR = O.Ns.L(B)")]
	[InlineData("namespace O with (B = D) { namespace Ns with (Z = Y) { L(x) = x & B } }\nR = O.Ns.L(B)")]
	[InlineData("P(y) = y\nnamespace Ns with (P = Q) { L(x) = x }\nQ(y) = y & Y\nR = Ns.L(P(Z))")]
	[InlineData("L(x) = x\nnamespace Ns with (B = D) { F(x) = x & B }\nR = L(B) with (L = Ns.F)")]
	[InlineData("L(x) = x\nnamespace Ns with (B = D) { F(x) = x & B }\nR = L(B)\nparse R with (L = Ns.F) as Swapped")]
	[InlineData("L(x) = x\nnamespace Ns with (B = D) { F(x) = x & B }\nnamespace O with (L = Ns.F) { R2 = L(B) }\nR = O.R2")]
	// An instance shared by two calls passes each call's own argument on: the second is checked at its call.
	[InlineData("namespace Ns with (B = Y) { L(x) = x }\nM(x) = Ns.L(x)\nnamespace O with (B = D) { R2 = M(B) }\nnamespace P { R3 = M(B) }\nR = O.R2 & P.R3")]
	[InlineData("namespace Ns with (B = Y) { L(x) = x }\nM(x) = Ns.L(x)\nnamespace P { R3 = M(B) }\nnamespace O with (B = D) { R2 = M(B) }\nR = O.R2 & P.R3")]
	[InlineData("namespace Ns with (B = Y) { L(x) = x }\nM(x) = Ns.L(x)\nK(x) = M(x)\nnamespace O with (B = D) { R2 = K(B) }\nR3 = K(B)\nR = O.R2 & R3")]
	// Passed on inside another call's argument, and through a rule that reaches itself.
	[InlineData("namespace Ns with (B = Y) { L(x) = x }\nP(x) = x\nM(x) = Ns.L(P(x))\nnamespace O with (B = D) { R2 = M(B) }\nR3 = M(B)\nR = O.R2 & R3")]
	[InlineData("namespace Ns with (B = Y) { L(x) = x }\nA(x) = ('a' & K(x)) | Ns.L(x)\nK(x) = 'k' & A(x)\nnamespace O with (B = D) { R2 = A(B) }\nR3 = K(B)\nR = O.R2 & R3")]
	// Through a rule that passes two arguments on in two positions.
	[InlineData("namespace Ns with (B = Y) { L(x) = x }\nM(x, w) = Ns.L(w) & x\nK(x, w) = M(x, w) | M(w, x)\nnamespace O with (B = D) { R2 = K(B, 'q') }\nR3 = K(B, 'q')\nR = O.R2 & R3")]
	// The caller's own header binds the name, but not as the callee's does: the callee's clone reads it first.
	[InlineData("namespace Ns with (B = Y) { L(x) = x }\nM(x) = Ns.L(x)\nnamespace O with (B = D) { R2 = M(B) }\nR = O.R2")]
	[InlineData("namespace Ns with (B = Y) { L(x) = x }\nnamespace O with (B = D) { R2 = Ns.L(B) }\nR = O.R2")]
	[InlineData("P(x) = x\nnamespace Ns with (B = Y) { L(x) = x }\nnamespace Warm { W = P(B) }\nnamespace O with (B = D) { R2 = Ns.L(P(B)) }\nR = Warm.W & O.R2")]
	// A `with` inside a namespace whose header is applied after it: the argument is still as written.
	[InlineData("L(x) = x\nnamespace Ns with (B = Y) { F(x) = x }\nnamespace O with (B = D) { R2 = L(B) with (L = Ns.F) }\nR = O.R2")]
	// A name written in the rule itself beside the one passed on.
	[InlineData("namespace Ns with (B = Y) { L(x) = x }\nM(x) = Ns.L(x & B)\nnamespace O with (B = D) { R2 = M(B) }\nR = O.R2")]
	[InlineData("namespace Ns with (B = Y) { L(x) = x }\nM(x) = Ns.L(B & x)\nnamespace O with (B = D) { R2 = M(B) }\nR = O.R2")]
	public void An_argument_a_header_would_rebind_is_refused(string rules)
	{
		Refused(GrammarNormalizer.HeaderBoundArgument, "B = 'b'\nD = 'd'\nY = 'y'\nZ = 'z'\n" + rules + "\nparse R\n");
	}

	/// <summary>
	/// A <c>with</c> inside an argument of a parameterized rule is refused rather than dropped,
	/// at any depth, and once.
	/// </summary>
	[Theory]
	[InlineData("L(x) = x & B\nR = L(D with (D = Y))")]
	[InlineData("L(x) = x & B\nR = L((D & B) with (D = Y))")]
	[InlineData("L(x) = x & B\nM(y) = y\nR = L(M(D with (D = Y)))")]
	[InlineData("L(x) = x & B\nM(y) = L(y)\nR = M(D with (D = Y))")]
	[InlineData("L(x) = x & B\nR = L(D with (D = Y)) with (B = Z)")]
	public void A_with_inside_an_argument_is_refused(string rules)
	{
		var diagnostics = Compile("B = 'b'\nD = 'd'\nY = 'y'\nZ = 'z'\n" + rules + "\nparse R\n").Diagnostics;

		Assert.Single(diagnostics, one => one.Id == GrammarNormalizer.WithInArgument);
		Assert.DoesNotContain(diagnostics, one => one.Id == GrammarNormalizer.LateWith);
	}

	/// <summary>
	/// Each layer of stacked substitutions keeps a parameterized rule's signature (GRAM3009).
	/// </summary>
	[Theory]
	[InlineData("R = (A2(D) with (A2 = Z)) with (D = Y)")]
	[InlineData("R = (A2(D) with (A2 = B2)) with (B2 = Z)")]
	public void Each_layer_of_stacked_substitutions_keeps_the_signature(string rule)
	{
		Refused(
			GrammarBinder.ParameterizedRebinding,
			"B = 'b'\nD = 'd'\nY = 'y'\nZ = 'z'\nA2(x) = 'a' & x\nB2(x) = 'b' & x\n" + rule + "\nparse R\n");
	}

	/// <summary>
	/// Two namespaces of the same name in different places that each need the other first
	/// are both refused.
	/// </summary>
	[Fact]
	public void A_cycle_between_namespaces_of_the_same_name_is_refused()
	{
		Refused(
			GrammarNormalizer.CircularWith,
			"""
			B = 'b'
			D = 'd'
			Z = 'z'
			Q = 'q'

			namespace A { namespace N with (B = D) { E = '[' & K.N.F & ']' | B } }
			namespace K { namespace N with (Z = Q) { F = '<' & A.N.E & '>' | Z } }

			parse A.N.E
			""");
	}

	/// <summary>
	/// Headers stacked through three namespaces read the same whichever is declared first.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Headers_stacked_through_three_namespaces_read_the_same_in_any_order(bool first)
	{
		const string C = "namespace C with (Y = P) { G = A.E }\n";
		const string A = "namespace A with (D = Y) { E = N.F }\n";

		var result = Compile(
			"B = 'b'\nD = 'd'\nY = 'y'\nP = 'p'\n" + (first ? C + A : A + C) +
			"namespace N with (B = D) { F = B }\nparse C.G as ParseStart\n");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "p").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "d").IsSuccess);
	}

	/// <summary>
	/// A publication's other bindings reach the clones its parameterized replacement's header made.
	/// </summary>
	[Fact]
	public void A_publications_bindings_reach_what_its_replacements_header_made()
	{
		var result = Compile(
			"""
			B = 'b'
			D = 'd'
			Y = 'y'
			Z = 'z'
			H = B
			L(x) = x

			namespace Ns with (B = D)
			{
				F(x) = x & H
			}

			R = L(Z)
			parse R with (L = Ns.F, D = Y) as ParseStart
			""");

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "zy").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "zd").IsSuccess);
	}

	/// <summary>
	/// A parameterized replacement whose header sends it back to itself is specialized once.
	/// </summary>
	[Fact]
	public void A_replacement_its_header_sends_back_to_itself_is_specialized_once()
	{
		var result = Compile(
			"""
			A(x) = x
			L(x) = x

			namespace Ns with (A = Ns.F)
			{
				F(x) = '!' & A(x) | x
			}

			R = L('z')
			parse R with (L = Ns.F) as ParseStart
			""");

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "!!z").IsSuccess);
	}

	static string WithAndHeader(string rule, string publish = "parse R as ParseStart")
	{
		return "B = 'b'\nD = 'd'\n" + rule + "\n" + publish + "\n";
	}

	[Fact]
	public void And_a_chain_that_does_not_come_back_is_not_a_cycle()
	{
		// `C` reaches `B`, and nothing reaches `C`. There is an order and the pass finds it,
		// which is what the ordering was written for in the first place — the refusal must
		// not take the feature with it.
		Assert.DoesNotContain(
			Compile(
				"""
				B = X with (X = Y)
				C = B with (X = Z)
				X = 'x'
				Y = 'y'
				Z = 'z'
				Start = C
				""")
				.Diagnostics,
			diagnostic => diagnostic.Id == GrammarNormalizer.CircularWith);
	}

	// ── §7.8, the marks a parse places over an extent ────────────────────────────

	/// <summary>
	/// The property the whole design rests on: a mark changes nothing about what is read.
	/// </summary>
	/// <remarks>
	/// Asked of the parser rather than of the generated text, because that is the claim — a
	/// comparison of sources fails on state numbering, which shifts the moment two states
	/// are added and says nothing about what either parser does. So the same grammar is
	/// compiled twice, with marks over it and without, and run over inputs that succeed,
	/// that fail outright, and that fail after backtracking. The two must agree on all of
	/// it: the answer, the value, the message and where the failure is reported.
	/// </remarks>
	[Theory]
	[InlineData("1")]
	[InlineData("1+2")]
	[InlineData("1+2+3")]
	[InlineData("")]
	[InlineData("1+")]
	[InlineData("+1")]
	[InlineData("1+2x")]
	public void A_mark_changes_nothing_about_what_is_read(string input)
	{
		const string Plain =
			"""
			Start : @string = left: Digit & ('+' & right: Start)? => @(left + ":" + right)
			Digit : @string = t: ['0'..'9'] => @(t)
			""";

		const string Marked =
			"""
			state : @int
			Start : @string = left: Digit with state @(1) & ('+' & right: Start)? => @(left + ":" + right)
			Digit : @string = t: ['0'..'9'] => @(t)
			""";

		var plain  = Parsed(Plain,  input);
		var marked = Parsed(Marked, input);

		Assert.Equal(plain.IsSuccess, marked.IsSuccess);
		Assert.Equal(plain.Value,     marked.Value);
		Assert.Equal(plain.Error,     marked.Error);
		Assert.Equal(plain.Position,  marked.Position);
	}

	/// <summary>Nested marks, and the nearest one winning — which is what `checked` needs.</summary>
	/// <remarks>
	/// One rule read in three places, under two marks, one mark and none. What it is handed
	/// is what stands over it and nothing else, outermost first, so the last element is the
	/// nearest — and that is the whole of how a hook tells `checked` from the `unchecked`
	/// inside it.
	/// </remarks>
	[Fact]
	public void The_marks_a_construction_stands_under_are_the_ones_over_it()
	{
		Assert.Equal(
			"a1b2c-",
			Built(
				"""
					state : @int
					Start : @string
						= (a: Item & b: Item with state @(2)) with state @(1) & c: Item
						=> @(a + b + c)
					Item : @string
						= t: ['a'..'z']
						=> @(t + (parserState.Length == 0
							? "-"
							: parserState[parserState.Length - 1].ToString()))
					""",
				"abc"));
	}

	[Fact]
	public void A_mark_an_abandoned_reading_placed_is_not_one_the_accepted_reading_stands_under()
	{
		// Nothing unwinds a mark, because nothing has to: the entry recording it is taken
		// away with everything else the abandoned reading wrote, and the walk that reads
		// them runs over what is left. The first alternative here matches `ab`, marks it,
		// and is then given back for want of the `!` — so the second alternative's `Item`
		// must see no mark at all.
		Assert.Equal(
			"a-b-",
			Built(
				"""
					state : @int
					Start : @string
						= (x: Item & y: Item) with state @(9) & '!' => @(x + y)
						| p: Item & q: Item                        => @(p + q)
					Item : @string
						= t: ['a'..'z']
						=> @(t + (parserState.Length == 0
							? "-"
							: parserState[parserState.Length - 1].ToString()))
					""",
				"ab"));
	}

	/// <summary>One mark type for a whole parse, wherever the declarations stand.</summary>
	/// <remarks>
	/// It used to be the place that was refused: a <c>state</c> inside a namespace was
	/// <c>GRAM3015</c>, on the reasoning that a state for part of a parse is not a thing. The
	/// reasoning is right and the refusal was in the wrong place — an included grammar is put
	/// in a namespace of its own, so refusing the place made a grammar that uses
	/// <c>with state</c> impossible to inherit. What it declares is a claim to be the same
	/// type, and that is what is checked.
	/// </remarks>
	[Fact]
	public void A_state_belongs_to_the_whole_grammar()
	{
		// The same type twice is one type, and says nothing.
		Assert.DoesNotContain(
			Compile(
				"""
				state : @int
				namespace Inner
				{
					state : @int
					A = 'x'
				}
				Start = Inner.A
				""").Diagnostics,
			diagnostic => diagnostic.Id == GrammarBinder.StateNotInvariant);

		// A second type is a second type for part of one answer.
		Refused(
			GrammarBinder.StateNotInvariant,
			"""
			state : @string
			namespace Inner
			{
				state : @int
				A = 'x'
			}
			Start = Inner.A
			""");
	}

	/// <summary>And where only the included grammar declares one, that one is it.</summary>
	[Fact]
	public void And_an_included_grammar_may_be_the_only_one_that_declares_it()
	{
		Assert.Equal(
			"int",
			Graph(
				"""
				namespace Inner
				{
					state : @int
					A = 'x'
				}
				Start = Inner.A
				""").State);
	}

	[Fact]
	public void And_a_grammar_declares_one_type_for_all_of_them()
	{
		Refused(
			GrammarBinder.DuplicateState,
			"""
			state : @int
			state : @string
			Start = 'x'
			""");
	}

	/// <summary>
	/// A mark with no type to be written in is the grammar's mistake, and said as one — not a
	/// CS0103 in the consumer's build about a parameter the generator never wrote.
	/// </summary>
	[Theory]
	[InlineData("Start : @int = a: A with state @(1) => @(1)\nA = 'x'")]
	[InlineData("Start : @int = 'x' => @(parserState.Length)")]
	[InlineData("Start : @int = 'x' => @(parserMarks.Length)")]
	[InlineData("Start = 'x' & when @(parserState.IsEmpty)")]
	public void A_mark_in_a_grammar_that_declares_no_state_is_refused(string grammar)
	{
		Refused(GrammarNormalizer.MarkWithoutState, grammar);
	}

	/// <summary>And the same grammar with one declared is what §7.8 describes.</summary>
	[Fact]
	public void And_the_same_marks_are_read_once_one_is_declared()
	{
		Assert.DoesNotContain(
			Compile(
				"""
				state : @int
				Start : @int = a: A with state @(1) => @(1)
				A : @int = 'x' => @(parserState.Length + parserMarks.Length)
				""").Diagnostics,
			one => one.Id == GrammarNormalizer.MarkWithoutState);
	}

	/// <summary>
	/// A mark says what stands over a reading, the same every time its site is reached — so its
	/// value names no capture, and one that does is the grammar's mistake rather than a CS0103.
	/// </summary>
	[Theory]
	[InlineData("Start : @int = w: ('a' | 'b') & '(' & i: A with state @(w.Length) & ')' => @(i)")]
	[InlineData("Start : @int = i: A with state @(i) => @(i)")]
	public void A_mark_that_names_a_capture_is_refused(string grammar)
	{
		Refused(GrammarNormalizer.MarkNamesCapture, "state : @int\nA : @int = 'x' => @(1)\n" + grammar);
	}

	/// <summary>What only spells a capture, or is a capture of another alternative, is not one.</summary>
	[Fact]
	public void And_a_mark_beside_a_capture_it_does_not_name_is_not()
	{
		Assert.DoesNotContain(
			Compile(
				"""
				state : @int
				Start : @int = w: A & '(' & i: A with state @(Sizes.w + "w".Length) & ')' => @(i)
				             | w: A => @(w)
				             | '[' & i: A with state @(2) & ']' => @(i)
				A : @int = 'x' => @(1)
				""").Diagnostics,
			one => one.Id == GrammarNormalizer.MarkNamesCapture);
	}

	/// <summary>A name that only looks like one is not asking for anything.</summary>
	[Fact]
	public void And_a_string_that_spells_one_is_not_asking_for_it()
	{
		Assert.DoesNotContain(
			Compile("Start : @string = 'x' => @(\"parserState\")").Diagnostics,
			one => one.Id == GrammarNormalizer.MarkWithoutState);
	}

	[Fact]
	public void And_state_is_still_an_ordinary_name_for_a_rule()
	{
		// The body is what tells the two apart, the same as `context` — and `with (state =
		// other)` still rebinds this rule, because a rebinding is parenthesized and a mark
		// is not.
		Assert.Equal(
			"xy",
			Read(
				Built(
					"""
					Start = a: state
					state = "xy"
					""",
					"xy"),
				"A"));
	}

	/// <summary>What all of this was built for, written out whole.</summary>
	/// <remarks>
	/// One <c>Sum</c> rule, read the same way everywhere, whose construction asks what it
	/// stands under: `!` where overflow is checked and `+` where it is not. That is the
	/// division §7.8 rests on — the mark cannot change what is read, and here nothing about
	/// the reading differs; only the value built from it does.
	/// <para>
	/// `c(1+u(2+3)+4)` is the shape a pair of flags cancelling each other cannot express.
	/// The inner mark does not turn the outer one off — it stands over its own protraction
	/// and the outer one is in force again after it, which is what nesting means and what
	/// an arena gives for nothing.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("1+2", "(1+2)")]
	[InlineData("c(1+2)", "(1!2)")]
	[InlineData("c(1+u(2+3)+4)", "(1!((2+3)!4))")]
	[InlineData("u(c(1+2))", "(1!2)")]
	[InlineData("c(u(c(1+2)))", "(1!2)")]
	public void A_mark_is_how_one_rule_reads_two_ways(string input, string expected)
	{
		Assert.Equal(
			expected,
			Built(
				"""
				state : @int
				Sum : @string
					= left: Atom & ('+' & right: Sum)?
					=> @(right == null
						? left
						: "(" + left +
							(parserState.Length > 0 && parserState[parserState.Length - 1] == 1 ? "!" : "+") +
							right + ")")
				Atom : @string
					= 'c' & '(' & e: Sum with state @(1) & ')' => @(e)
					| 'u' & '(' & e: Sum with state @(0) & ')' => @(e)
					| d: ['0'..'9']                            => @(d)
				Start : @string = e: Sum => @(e)
				""",
				input));
	}

	// ── FIRST through recursion (§4.3) ──────────────────────────────────────────

	static FirstSets.First FirstOf(string grammar, string rule)
	{
		var graph = Normalized(grammar);

		return FirstSets.Of(graph.Bodies[graph.Rules.First(one => one.Name == rule)], graph);
	}

	/// <summary>
	/// Two rules that reach each other have the set they plainly have.
	/// </summary>
	/// <remarks>
	/// The walk this replaced had to stop somewhere and stopped at "anything", which is
	/// sound and is Top — and Top at the head of a rule is Top for everything containing it.
	/// A fixed point has no such place to stop: every rule starts at nothing and grows.
	/// </remarks>
	[Fact]
	public void A_rule_that_reaches_itself_still_has_a_first_set()
	{
		var first = FirstOf("A = B | 'a'\nB = A | 'b'\nStart = A", "A");

		Assert.True(first.IsKnown, "the set is known rather than 'anything'");
		Assert.True(first.Overlaps(FirstSets.First.Chars([new CharRange('a', 'a')])));
		Assert.True(first.Overlaps(FirstSets.First.Chars([new CharRange('b', 'b')])));

		// And nothing else got in on the way round.
		Assert.False(first.Overlaps(FirstSets.First.Chars([new CharRange('c', 'c')])));
	}

	[Fact]
	public void And_a_directly_recursive_rule_too()
	{
		// `Sum = Sum '+' Term | Term` is rewritten to a fold before this sees it, but the
		// shape a reader writes is the shape that used to answer "anything".
		Assert.True(FirstOf("A = A & 'x' | 'a'\nStart = A", "A").IsKnown);
	}

	/// <summary>An external recognizer is still "anything", and honestly so.</summary>
	/// <remarks>
	/// What it accepts is the host's knowledge. A fixed point makes recursion knowable; it
	/// does not make a C# predicate knowable, and claiming otherwise would be the unsound
	/// direction.
	/// </remarks>
	[Fact]
	public void And_a_rule_with_no_body_is_anything()
	{
		Assert.False(FirstOf("A = [@Digit] & 'x'\nStart = A", "A").IsKnown);
	}

	// ── Which context a rule was written against (§7.7) ─────────────────────────

	static RecognitionGraph Normalized(string grammar)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				DotGram.Grammar.Parsing.GramParser.Parse(
					DotGram.Grammar.Parsing.GramLexer.Tokenize(
						grammar, RoslynCSharpScanner.Instance)).File!));
	}

	static string? ContextOf(RecognitionGraph graph, string rule)
	{
		return graph.ContextOf(graph.Rules.First(one => one.Name == rule));
	}

	/// <summary>
	/// A rule is bound to the contract its own grammar named, and the root's is the
	/// effective one.
	/// </summary>
	/// <remarks>
	/// The two are the same until one grammar includes another, which is why they were one
	/// thing until they could not be. What a publication takes is the root's — the object
	/// the caller hands over — and what a rule's `when` and `=&gt;` see it through is the
	/// contract of the grammar that wrote them.
	/// </remarks>
	[Fact]
	public void A_rule_is_bound_to_the_context_its_own_grammar_declared()
	{
		var graph = Normalized(
			"""
			context : @Outer
			namespace Inner
			{
				context : @Names
				A = 'x'
			}
			Start = a: Inner.A
			""");

		Assert.Equal("Outer", graph.Context);
		Assert.Equal("Names", ContextOf(graph, "A"));
		Assert.Equal("Outer", ContextOf(graph, "Start"));
	}

	[Fact]
	public void And_a_grammar_that_declares_one_binds_every_rule_to_it()
	{
		// Where nothing is included there is one contract and it is the effective type, which
		// is the shape everything written before this had.
		Assert.Equal(
			"Names",
			ContextOf(
				Normalized("context : @Names\nA = 'x'\nStart = a: A"),
				"A"));
	}

	[Fact]
	public void And_a_grammar_that_declares_none_binds_every_rule_to_nothing()
	{
		Assert.Null(ContextOf(Normalized("A = 'x'\nStart = a: A"), "A"));
	}

	/// <summary>A specialized clone keeps the contract of the rule it was cloned from.</summary>
	/// <remarks>
	/// Which is the part that could have gone wrong and does not, because the contract is
	/// derived from where a rule was declared rather than recorded against the rule: every
	/// place that makes a clone keeps it in its original's namespace, so there is nothing to
	/// hand on. The same fact recorded per rule would have been a fifth thing for `Carry` to
	/// forget.
	/// </remarks>
	[Fact]
	public void And_a_clone_made_by_a_rebinding_keeps_it()
	{
		var graph = Normalized(
			"""
			namespace Inner
			{
				context : @Names
				Sep = ','
				List = 'a' & Sep & 'b'
			}
			namespace Other with (Inner.Sep = Semi)
			{
				Start = Inner.List
			}
			Semi = ';'
			""");

		// Every rule that came from `Inner`, clone or original, sees `Names`.
		Assert.All(
			graph.Rules.Where(rule => rule.Name.Contains("List", StringComparison.Ordinal)),
			rule => Assert.Equal("Names", graph.ContextOf(rule)));
	}

	// ── §7.7, the state a parse works out ───────────────────────────────────────

	/// <summary>
	/// A <c>context</c> inside a namespace is that grammar's own contract, not a second
	/// object and not an error.
	/// </summary>
	/// <remarks>
	/// It used to be `GRAM3013`, refused on the reasoning that a context for part of a parse
	/// is not a thing. It is one, and the reasoning confused two: the caller hands over one
	/// object for the whole parse, and the *type it is seen through* belongs to whoever wrote
	/// the code — which is what lets a grammar be included in another and keep meaning what
	/// it meant (docs/next.md, "Decided: `context` is a contract").
	/// </remarks>
	[Fact]
	public void A_context_inside_a_namespace_is_that_grammar_own_contract()
	{
		EmittedCode.Quiet(
			Compile(
				"""
				namespace Inner
				{
					context : @Names
					A = 'x'
				}
				Start = Inner.A
				parse Start
				""")
				.Diagnostics);
	}

	[Fact]
	public void And_the_one_at_the_top_is_the_one_a_caller_supplies()
	{
		// Two contracts, and the effective type — what the publication takes — is the root's.
		Assert.Contains(
			"ParseStart(string input, Outer context)",
			Compile(
				"""
				context : @Outer
				namespace Inner
				{
					context : @Names
					A : @string = t: 'x' => @(context.Seen(t))
				}
				Start : @string = a: Inner.A => @(a)
				parse Start
				""")
				.Sources[0].Text,
			StringComparison.Ordinal);
	}

	[Fact]
	public void And_a_grammar_declares_one_of_them()
	{
		Refused(
			GrammarBinder.DuplicateContext,
			"""
			context : @Names
			context : @Other
			Start = 'x'
			""");
	}

	[Fact]
	public void And_context_is_still_an_ordinary_name_for_a_rule()
	{
		// A body is what tells the two apart, so a grammar that had a rule called `context`
		// before this existed still has one.
		Assert.Equal(
			"xy",
			Read(
				Built(
					"""
					Start = a: context
					context = "xy"
					""",
					"xy"),
				"A"));
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

	// ── A rule nothing reaches (GRAM4018) ────────────────────────────────────────

	/// <summary>A rule no publication, call or rebinding reaches is said out loud.</summary>
	[Fact]
	public void A_rule_nothing_reaches_is_reported()
	{
		// The one thing the grammar is told is wrong. A remark may stand beside it — that is
		// what information is for — and what this test is about is the rule nothing reaches.
		var told = Assert.Single(
			Compile(
				"""
				Start = 'a'
				Stray = 'b'
				parse Start
				""").Diagnostics,
			diagnostic => diagnostic.Severity != GramSeverity.Info);

		Assert.Equal(GrammarNormalizer.UnusedRule, told.Id);
		Assert.Contains("Stray", told.Message, StringComparison.Ordinal);
	}

	/// <summary>And a call is not the only way to reach one.</summary>
	/// <remarks>
	/// A rebinding names its replacement and nothing else does; an element set draws a
	/// rule's items into itself and the rule is gone from the tree afterwards; a seam and
	/// a word boundary are calls by the time this is asked, so what <c>trivia</c> is made
	/// of is reached through <c>trivia</c>, and <c>wordboundary</c> is never asked about
	/// at all. Each of these was a false report before it was not.
	/// </remarks>
	[Fact]
	public void And_a_call_is_not_the_only_way_to_reach_one()
	{
		EmittedCode.Quiet(Compile(
			"""
			using Lexical;

			namespace Lexical
			{
				trivia = none

				Digit = ['0'..'9']
				Space = ' '
			}

			trivia = Space*
			wordboundary = ['a'..'z']

			Number = [Digit | '_']+
			Other  = ['x'..'z']+
			Start  = Number

			parse Start with (Number = Other) as Loose
			parse Start
			""").Diagnostics);
	}

	/// <summary>A grammar that was refused is not told what else it might tidy.</summary>
	/// <remarks>
	/// What called the rule may be the declaration that did not come out. <c>Header</c> is
	/// unreadable here and <c>Digit</c> is reached from nowhere else, so reporting it would
	/// be one mistake told as two (implementation.md §0).
	/// </remarks>
	[Fact]
	public void And_a_grammar_that_broke_is_asked_nothing()
	{
		Assert.DoesNotContain(
			Compile(OneStrayCharacter).Diagnostics,
			diagnostic => diagnostic.Id == GrammarNormalizer.UnusedRule);
	}

	/// <summary>And a grammar with nothing published is asked nothing either.</summary>
	/// <remarks>
	/// Every rule in it is unreached, which says nothing about any of them. What that
	/// grammar is missing is a publication, and that is a different remark.
	/// </remarks>
	[Fact]
	public void And_a_grammar_that_publishes_nothing_is_asked_nothing()
	{
		EmittedCode.Quiet(Compile("Start = 'a'\nStray = 'b'").Diagnostics);
	}

	// ── Parameterized rules (§4.2) ───────────────────────────────────────────────

	const string Listing =
		"List(item, sep) = item & (sep & item)*\n" +
		"Word  = ['a'..'z']+\n" +
		"Comma = ','\n";

	/// <summary>A second separator, for the one test that passes two.</summary>
	/// <remarks>
	/// Out of <see cref="Listing"/> because a rule nothing reaches is now said out loud
	/// (GRAM4018), and a fixture shared by four tests should not make three of them
	/// declare a rule they never pass.
	/// </remarks>
	const string Separator = "Semi  = ';'\n";

	[Fact]
	public void A_rule_may_take_another_rule_as_a_parameter()
	{
		// §4.2. A parameter is a compile-time thing entirely: the call becomes a rule of
		// its own with `item` and `sep` replaced by what was passed, so nothing downstream
		// ever meets a parameter and nothing is dispatched at run time.
		Assert.True(Matches(Listing + "Start = List(Word, Comma)", "ab,cd,ef"));
	}

	[Fact]
	public void And_the_same_rule_twice_with_different_arguments()
	{
		// Two specializations of one rule, side by side in one grammar, which is the whole
		// point of writing `List` once.
		Assert.True(Matches(
			Listing + Separator + "Start = List(Word, Comma) & ' ' & List(Word, Semi)",
			"ab,cd ef;gh"));
	}

	[Fact]
	public void And_what_it_was_given_still_has_to_match()
	{
		Assert.False(Matches(Listing + "Start = List(Word, Comma)", "ab;cd"));
	}

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
	public void Recovery_also_steps_over_an_element_that_fails_on_its_first_token()
	{
		// EOF cannot match at `1bad`, so a failed Row is an error even without consumption.
		Assert.True(Matches(
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
	[InlineData(GrammarNormalizer.LeftRecursion, "A = B & 'x' | 'a'\nB = A & 'y' | 'b'\nStart = A")]
	[InlineData(DotGram.Grammar.Binding.GrammarBinder.ParameterizedRebinding,
		"B(item) = item\nD = 'd'\nnamespace Ctx with (B = D) { }")]
	public void Still_refused(string expected, string grammar)
	{
		Refused(expected, grammar);
	}

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
	public void And_without_the_rule_nothing_is_inserted()
	{
		// Empty by default, so a grammar that never mentions it pays nothing — and keeps
		// the prefix match it always had.
		Assert.True(Matches("Start = \"if\" & ['a'..'z']*", "iffy"));
	}

	[Fact]
	public void And_the_check_goes_before_the_trivia()
	{
		// The other order would ask whether a letter follows the whitespace rather than
		// whether it follows the keyword, which is no question at all.
		Assert.False(Matches(
			"wordboundary = ['a'..'z']\ntrivia = ' '*\nStart = \"if\" & \"then\"",
			"iffy then"));
	}

	/// <summary>`word` is a whole word of whatever the boundary says a word is made of.</summary>
	/// <remarks>
	/// The other half of §4.6. The boundary already says what continues a word, so what a
	/// word is needs no second saying — and the places that want one wrote out every word
	/// the specification happened to have instead, which is a catalogue and not a grammar.
	/// </remarks>
	[Theory]
	[InlineData("fillfactor", true)]
	[InlineData("data_compression", true)]
	[InlineData("x1", true)]
	[InlineData("(", false)]
	[InlineData("", false)]
	public void A_word_is_a_word(string input, bool matches)
	{
		Assert.Equal(matches, Matches(
			"wordboundary = ['a'..'z' | '0'..'9' | '_']\nStart = word", input));
	}

	/// <summary>And it is the whole of one, however the parse would rather read it.</summary>
	/// <remarks>
	/// Maximal by construction and not by greed: a repetition can be handed characters back
	/// when what follows it fails, and `word & "y"` would then read `fillfactor` as
	/// `fillfactor` minus its tail. What `word` matches is a lexeme, and a lexeme is whole —
	/// the same claim §4.6 makes about a keyword, made about the other side of the boundary.
	/// </remarks>
	[Fact]
	public void And_it_is_the_whole_word()
	{
		Assert.False(Matches(
			"wordboundary = ['a'..'z']\nStart = word & \"y\"", "fillfactory"));
	}

	/// <summary>Where only some words will do, the shape is a lookahead and not a list.</summary>
	/// <remarks>
	/// What bounds a run of words is usually not which words are in it but where it ends,
	/// and a grammar that says so stops being a catalogue: T-SQL's permission names are any
	/// words up to the `ON` or `TO` that begins the clause after them.
	/// </remarks>
	[Theory]
	[InlineData("alter any database on", true)]
	[InlineData("view definition on", true)]
	[InlineData("on", false)]
	public void And_a_run_of_them_ends_where_the_next_clause_begins(string input, bool matches)
	{
		Assert.Equal(matches, Matches(
			"wordboundary = ['a'..'z']\ntrivia = ' '*\n" +
			"Start = Name & \"on\"\n" +
			"Name  = (?!\"on\" & word)+", input));
	}

	/// <summary>Each namespace's `word` is made of that namespace's own boundary.</summary>
	/// <remarks>
	/// A boundary is declared per namespace and a lexical one shields what stands outside
	/// it (§4.6), so a single `word` shared by the whole grammar would be a word of
	/// whichever namespace happened to reach it first. Here the outer boundary is letters
	/// and the inner one is digits, and each `word` reads its own.
	/// </remarks>
	[Fact]
	public void And_a_word_is_the_namespace_it_was_written_in()
	{
		const string grammar =
			"""
			wordboundary = ['a'..'z']
			trivia = ' '*
			namespace Inner
			{
				trivia = none
				wordboundary = ['0'..'9']
				Number = word
			}
			Start = word & Inner.Number
			""";

		Assert.True (Matches(grammar, "abc 123"));
		Assert.False(Matches(grammar, "abc abc"));
	}

	/// <summary>And a grammar that never said what continues a word has no words in it.</summary>
	/// <remarks>
	/// `word` would be a run of nothing: a rule that matches everywhere and consumes
	/// nothing, which is the silence GRAM4019 exists to break.
	/// </remarks>
	[Fact]
	public void And_word_without_a_boundary_is_refused()
	{
		Assert.Contains(
			Compile("Start = word\nparse Start").Diagnostics,
			diagnostic => diagnostic.Id == GrammarNormalizer.WordWithoutBoundary);
	}

	/// <summary>
	/// A capture that gathers several pieces over kinds is cut whole where they tile.
	/// </summary>
	/// <remarks>
	/// <para>
	/// §10's join. A repetition that captures every turn records one piece per turn, and the
	/// value is those pieces joined — which over characters is a buffer the pieces are copied
	/// into, since a piece is a slice of what is being read and its length is a count of
	/// characters.
	/// </para>
	/// <para>
	/// Over kinds neither half of that holds. A position indexes a token, so a piece's length
	/// is a count of tokens and the buffer came out sized in tokens — `Destination is too
	/// short` for anything but one-character lexemes — and what stood between two adjacent
	/// tokens is part of the value the character reading gives and was being dropped. Turns
	/// of a repetition tile, so the answer is one cut from the first start to the last end,
	/// which the tape already made and the direct walk did not.
	/// </para>
	/// <para>
	/// Found by writing `word+` in T-SQL, where a database setting was a run of words:
	/// `ALTER DATABASE d SET HADR AVAILABILITY GROUP = g`.
	/// </para>
	/// </remarks>
	[Fact]
	public void A_join_over_kinds_keeps_what_stood_between_the_tokens()
	{
		const string grammar =
			"""
			wordboundary = ['a'..'z']
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Name = ['a'..'z'] & ['a'..'z']*
			}
			Start : @string = t: (?!"with" & word)+ & ('=' & Lexical.Name)? => @(t)
			parse Start
			""";

		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions
			{
				ClassName     = "Grammar",
				CSharpScanner = RoslynCSharpScanner.Instance,
				Lexical       = true,
			});

		EmittedCode.Quiet(result.Diagnostics);

		var got = EmittedCode.Match(
			EmittedCode.Compile(result.Sources[0].Text), "Grammar", "TryParseStart", "aa bb cc");

		Assert.True(got.IsSuccess);
		Assert.Equal("aa bb cc", got.Value);
	}

	/// <summary>
	/// A rule that reads more than its literals is not a set of them.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A rule that is a choice of literals becomes one range over kinds at each of its call
	/// sites, which is the whole reason a fifty-way keyword choice costs one comparison
	/// (docs/design/lexical-adt-design.md). A boundary woven onto it — or written by hand as
	/// <c>(… | …) &amp; ?!\p{L}</c> — does not change what it accepts, so the search for the
	/// choice looks through a sequence to its first part.
	/// </para>
	/// <para>
	/// It used to look through <em>any</em> sequence. So <c>("::" | '.') &amp; Name</c> was
	/// called the set <c>{"::", "."}</c>, every call site became that range, and the name
	/// after it was gone: T-SQL read <c>t::a</c> as <c>t</c> with an alias, and refused
	/// everything that could follow — <c>t::a + 1</c>, <c>t::a AS q</c>, <c>t::f()</c>.
	/// Sixty statements of somebody else's corpus, and the tell was that the same rule
	/// written as <c>MemberOp &amp; Name</c> worked. The case the lookthrough exists for —
	/// a choice and then an assertion, which reads nothing — is still a set, and
	/// <see cref="LexicalSplitTests"/> asserts that a word list collapses to one range.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("select t", "t/-")]
	[InlineData("select t as q", "t/q")]
	[InlineData("select t::a", "t/-")]
	[InlineData("select t::a as q", "t/q")]
	public void A_choice_that_reads_more_than_its_literals_is_not_a_set(string input, string expected)
	{
		const string grammar =
			"""
			wordboundary = ['a'..'z']
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Name = ['a'..'z'] & ['a'..'z']*
			}
			Member = ("::" | "..") & Lexical.Name
			Item  : @string = v: Lexical.Name & Member* & alias: Tail? => @(v + "/" + (alias ?? "-"))
			Tail  : @string = "as" & t: Lexical.Name => @(t)
			Start : @string = "select" & i: Item => @(i)
			parse Start
			""";

		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions
			{
				ClassName     = "Grammar",
				CSharpScanner = RoslynCSharpScanner.Instance,
				Lexical       = true,
			});

		EmittedCode.Quiet(result.Diagnostics);

		var got = EmittedCode.Match(
			EmittedCode.Compile(result.Sources[0].Text), "Grammar", "TryParseStart", input);

		Assert.True(got.IsSuccess, input);
		Assert.Equal(expected, got.Value);
	}

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
	/// A turn that carries a seam of its own is spaced from the next one (§4.5).
	/// </summary>
	/// <remarks>
	/// <para>
	/// The seam inside a turn and the seam between turns are the same question. `Item` is
	/// two operands with a space allowed between them, so a space between one `Item` and the
	/// next is the same permission; refusing it read the first turn and stopped.
	/// </para>
	/// <para>
	/// It is written as a rule rather than inline because that is where it hid: the spacing
	/// of a repetition whose body is a sequence has always been done, and a call is not a
	/// sequence to look at. `SqlStandard92` refused `CASE WHEN a &gt; 1 THEN 'big' WHEN a
	/// &gt; 0 THEN 'small' END` for exactly this, on both of its `WHEN` forms.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("case when a end",              true)]
	[InlineData("case when a when b end",       true)]
	[InlineData("case when a when b when c end", true)]
	[InlineData("casewhenaend",                 true)]   // the seams are all optional
	[InlineData("case when end",                false)]  // a turn still needs its operand
	public void A_turn_that_holds_a_seam_is_spaced_from_the_next(string input, bool expected)
	{
		Assert.Equal(expected, Matches(
			"trivia = ' '*\n" +
			"Item  = \"when\" & ['a'..'z']\n" +
			"Start = \"case\" & Item+ & \"end\"",
			input));
	}

	/// <summary>And the same with a word boundary, which is how it was found.</summary>
	/// <remarks>
	/// §4.6 weaves `?&lt;!wordboundary` in front of a word literal, and without the seam
	/// that check ran on the character before the *space* — the last letter of the previous
	/// turn — so the second turn was refused before its literal was ever compared. The
	/// boundary still does its own work here: `casewhen` is one word and not two.
	/// </remarks>
	[Theory]
	[InlineData("case when a when b end", true)]
	[InlineData("casewhen a end",         false)]
	[InlineData("case when aend",         false)]
	public void A_word_boundary_does_not_stop_the_second_turn(string input, bool expected)
	{
		Assert.Equal(expected, Matches(
			"wordboundary = ['a'..'z' | '0'..'9' | '_']\n" +
			"trivia = ' '*\n" +
			"Item  = \"when\" & ['a'..'z']\n" +
			"Start = \"case\" & Item+ & \"end\"",
			input));
	}

	/// <summary>
	/// A substitution reaches through a word boundary and through glue, rather than
	/// falling over them.
	/// </summary>
	/// <remarks>
	/// §5.1 clones every rule a `with` site reaches and rewrites the calls inside the
	/// clones, and the clone was a switch over node kinds naming every one but two: the
	/// `?&lt;!wordboundary` §4.6 weaves in front of a word literal, and the glue `~` leaves
	/// between two operands that may not be spaced. A grammar holding either, with a
	/// `with` anywhere above it, did not compile at all — GRAM0001, the generator falling
	/// over — which the expression language found by asking for its own identifiers in
	/// ASCII.
	/// </remarks>
	[Theory]
	[InlineData("if ->", true)]
	[InlineData("if - >", false)]
	[InlineData("if +>", false)]
	public void A_substitution_reaches_through_a_boundary_and_through_glue(
		string input, bool expected)
	{
		// Nothing after `if` is a word character, so §4.6 weaves the boundary around the
		// keyword and around nothing else — the point here is the shape of the tree the
		// substitution has to copy, not what the boundary itself decides.
		Assert.Equal(expected, Matches(
			"""
			wordboundary = ['a'..'z']
			trivia = ' '*
			A     = '+'
			B     = '-'
			Word  = "if" & A ~ '>'
			Start = Word with (A = B)
			""",
			input));
	}

	/// <summary>
	/// A turn with no seam of its own is still not spaced, which is what keeps a lexeme one.
	/// </summary>
	/// <remarks>
	/// The seam is looked for in the callee's own body and against the callee's own trivia.
	/// `Word` is declared where trivia is empty, so there is none to find and none to insert
	/// — `ab cd` stays two words and never becomes one.
	/// </remarks>
	[Fact]
	public void A_turn_with_no_seam_of_its_own_is_left_alone()
	{
		// A sequence this time, not `['a'..'z']+`: the body being a sequence is exactly what
		// would have made the caller space it, and the empty trivia is what stops it.
		Assert.False(Matches(
			"trivia = ' '*\n" +
			"namespace Lexical\n" +
			"{\n" +
			"\ttrivia = none\n" +
			"\tWord = ['a'..'z'] & ['a'..'z']*\n" +
			"}\n" +
			"Start = Lexical.Word*",
			"ab cd"));
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

		// Single characters spaced on purpose: the grammar is about how its seams read.
		Assert.DoesNotContain(
			result.Diagnostics,
			static diagnostic => diagnostic.Severity != GramSeverity.Info &&
				diagnostic.Id != GrammarNormalizer.SpacedLexeme);

		return EmittedCode.Match(
			EmittedCode.Compile(Assert.Single(result.Sources).Text),
			"Grammar", "TryParseStart", input).IsSuccess;
	}

	[Theory]
	[InlineData("http", true)]
	[InlineData("https", true)]
	[InlineData("ftp", true)]
	[InlineData("httpx", false)]
	public void A_literal_a_later_one_continues_is_tried_first_and_come_back_for(string input, bool matches)
	{
		// Ordered choice: `"http"` is preferred where both fit, and the longer one is
		// reachable because the parse comes back for it when the shorter leaves the input
		// unfinished. Both readings of `https` exist; which is answered is §11's business.
		Assert.Equal(
			matches,
			Matches("Start = QhttpQ | QhttpsQ | QftpQ".Replace("Q", "\""), input));
	}

	[Theory]
	[InlineData("https", true)]
	[InlineData("httpss", true)]
	[InlineData("http", false)]
	public void And_comes_back_for_it_past_a_shorter_reading_that_did_not_work_out(string input, bool matches)
	{
		// The case the way back exists for. On `httpss` the shorter alternative matches,
		// the `"s"` after the choice matches, and a character is left over — so the parse
		// returns to the choice and spends it on the longer alternative instead. The
		// entry it resumes at was written past the characters already matched, so what
		// runs there compares one character and not five.
		Assert.Equal(
			matches,
			Matches("Start = (QhttpQ | QhttpsQ) & QsQ".Replace("Q", "\""), input));
	}

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
		EmittedCode.Quiet(Compile(
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
	public void And_two_counts_are_two_rules()
	{
		// One rule written once, two lengths asked of it, side by side.
		Assert.True(Matches(
			"Digits(n) = ['0'..'9']{n}\nStart = Digits(4) & '-' & Digits(2)",
			"2026-08"));
	}

	[Fact]
	public void A_count_passed_on_is_still_a_count()
	{
		// The argument names the caller's own parameter rather than a number, so what is
		// passed through is what the outer call was given.
		Assert.True(Matches(
			"Digits(n) = ['0'..'9']{n}\nPair(n) = Digits(n) & '-' & Digits(n)\nStart = Pair(2)",
			"20-26"));
	}

	[Fact]
	public void A_count_naming_a_parameter_that_was_not_given_one_is_refused()
	{
		// The rule takes a piece of grammar and uses it as a number, which is a rule that
		// would otherwise repeat zero times and match nothing.
		Refused(
			GrammarNormalizer.UnbuiltCall,
			"Digits(n) = ['0'..'9']{n}\nWord = ['a'..'z']\nStart = Digits(Word)");
	}

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
	public void A_call_with_the_wrong_number_of_arguments_is_refused()
	{
		Refused(GrammarNormalizer.UnbuiltCall, Listing + "Start = List(Word)");
	}

	/// <summary>
	/// A parameterized rule named without its arguments is refused where it is named. Its name
	/// alone is no rule (§4.2): only a call makes one, and the bare name went on as a call to a
	/// rule that has no body, which failed inside the compiler with an exception where the
	/// grammar should have been told. The library's rules are parameterized like any other.
	/// </summary>
	[Theory]
	[InlineData("P(x) = x & 'b'\nStart = 'a' & P & eof", "P")]
	[InlineData("Start = 'a' & Std.Nested & eof", "Std.Nested")]
	[InlineData("Start = 'a' & Std.BlockComment & eof", "Std.BlockComment")]
	[InlineData("trivia = { ' '+ | Std.Nested }\nStart = 'a' & 'b' & eof", "Std.Nested")]
	public void A_parameterized_rule_named_without_arguments_is_refused_where_it_is_named(string grammar, string name)
	{
		var told = Assert.Single(
			Compile(grammar + "\nparse Start").Diagnostics,
			static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GrammarNormalizer.UnbuiltCall, told.Id);
		Assert.Equal(grammar.IndexOf(name + " ", StringComparison.Ordinal), told.Position);
		Assert.Contains("without arguments", told.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// A refused call under a repetition is the one mistake told: what stands in its place reads
	/// nothing, and the repetition over it is not told again that its body can match nothing.
	/// </summary>
	[Theory]
	[InlineData("P(x) = x & 'b'\nStart = 'a' & P* & eof", "P*")]
	[InlineData("P(x) = x & 'b'\nStart = 'a' & (v: P)+ & eof", "P)")]
	[InlineData("P(x) = x & 'b'\nStart = 'a' & P('a', 'c')* & eof", "P(")]
	[InlineData("P(x) = x\nStart = 'a' & ($q: P & 'a'?)* & eof", "P &")]
	[InlineData("P(x) = x\nStart = $r: 'x' & (P{1,#r} & 'a'?)* & eof", "P{")]
	public void A_refused_call_under_a_repetition_is_the_one_mistake_told(string grammar, string call)
	{
		var told = Assert.Single(
			Compile(grammar + "\nparse Start").Diagnostics,
			static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GrammarNormalizer.UnbuiltCall, told.Id);
		Assert.Equal(grammar.LastIndexOf(call, StringComparison.Ordinal), told.Position);
	}

	/// <summary>
	/// And a repetition whose body matches nothing without the refused call is still told so,
	/// beside the refusal: <c>'a'?</c> makes the body match nothing on its own.
	/// </summary>
	[Fact]
	public void A_repetition_that_matches_nothing_beside_a_refused_call_is_still_told()
	{
		var errors = Compile("P(x) = x\nStart = (P | 'a'?)* & eof\nparse Start").Diagnostics
			.Where(static one => one.Severity == GramSeverity.Error)
			.Select(static one => one.Id)
			.OrderBy(static one => one, StringComparer.Ordinal)
			.ToArray();

		Assert.Equal([GrammarNormalizer.NullableRepetition, GrammarNormalizer.UnbuiltCall], errors);
	}

	/// <summary>
	/// Two repetitions of one rule that match nothing are told each where it is written, so
	/// that a build saying each diagnostic once still says both.
	/// </summary>
	[Theory]
	[InlineData("Start = ('a'?)* & ('b'?)* & eof", "('a'", "('b'")]
	// Hoisted: a text capture of every turn becomes a capture of the repetition, rebuilt.
	[InlineData("Start = (t: ('a'?))* & (u: ('b'?))* & eof", "(t:", "(u:")]
	// Copied: a namespace's header makes a copy of what it reaches.
	[InlineData("B = 'b'\nD = 'd'\nnamespace Ns with (B = D) { X = ('a'?)* & ('b'?)* & B }\nStart = Ns.X", "('a'", "('b'")]
	public void Two_repetitions_that_match_nothing_are_told_where_each_is(string grammar, string first, string second)
	{
		var told = Compile(grammar + "\nparse Start").Diagnostics
			.Where(static one => one.Id == GrammarNormalizer.NullableRepetition)
			.Select(static one => one.Position)
			.Distinct()
			.OrderBy(static one => one)
			.ToArray();

		Assert.Equal([grammar.IndexOf(first, StringComparison.Ordinal), grammar.IndexOf(second, StringComparison.Ordinal)], told);
	}

	/// <summary>
	/// A call with the wrong number of arguments is told at the call, which is where the
	/// mistake is: the declaration it was placed at is the library's own text where the rule
	/// is the library's, and no place in the grammar at all.
	/// </summary>
	[Theory]
	[InlineData(Listing + "Start = List(Word)", "List(Word)")]
	[InlineData("Start = 'a' & Std.Nested(\"/*\") & eof", "Std.Nested(")]
	public void A_call_with_the_wrong_number_of_arguments_is_told_at_the_call(string grammar, string call)
	{
		var told = Assert.Single(
			Compile(grammar + "\nparse Start").Diagnostics,
			static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GrammarNormalizer.UnbuiltCall, told.Id);
		Assert.Equal(grammar.IndexOf(call, StringComparison.Ordinal), told.Position);
	}

	// ── Publishing an expression (§6) ───────────────────────────────────────────

	[Fact]
	public void A_directive_publishes_whatever_expression_it_names()
	{
		// Where the notation refers to a rule, an expression may stand — a call, a
		// choice, a repetition, anything an operand can be. A parameterized rule could
		// not be reached from a directive at all before this.
		var result = Compile("""
			Word : @string = w: ['a'..'z']+ => @(w)
			Padded(item, pad) = pad & item & pad

			parse Padded(Word, '#') as Hashed
			parse ('a' | 'b') as Ab
			parse Word
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		// The lifted rule is a rule, so §4.1 case 4 says what its value is: the extent it
		// matched. An expression is published for what it recognizes; a value still comes
		// from a rule that declares a type and builds one.
		Assert.Equal("#ab#", EmittedCode.Match(assembly, "Grammar", "TryHashed", "#ab#").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryHashed", "ab").IsSuccess);
		Assert.Equal("b",    EmittedCode.Match(assembly, "Grammar", "TryAb", "b").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryAb", "c").IsSuccess);
		Assert.Equal("ab",   EmittedCode.Match(assembly, "Grammar", "TryParseWord", "ab").Value);
	}

	[Fact]
	public void And_the_rule_it_lifts_is_bound_where_the_directive_is_written()
	{
		// The expression is written inside the namespace, so it reads what that
		// namespace reads — its own `trivia` here, not the outer one. Which is what a
		// rule declared there would do, because that is what it becomes.
		var result = Compile("""
			trivia = ' '*
			A = 'a'
			B = 'b'

			namespace Tight
			{
				trivia = none

				parse (A & B) as Joined
			}

			parse (A & B) as Spaced
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryJoined", "ab").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryJoined", "a b").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TrySpaced", "a b").IsSuccess);
	}

	[Fact]
	public void A_directive_may_declare_the_type_of_what_it_lifts()
	{
		// The third part a rule has, in the place that reads as the method's own. It
		// belongs to the expression being lifted, which is what makes a `=>` legal there
		// — without a declared type a construction is refused (GRAM4008), and with one
		// the directive reaches everything §4.1 offers, a value included.
		var result = Compile("""
			Word : @string = w: ['a'..'z']+ => @(w)
			Padded(item, pad: char) : @string = t: item => @(t + pad)

			parse (v: Padded(Word, '#') => @(v)) as Marked : @string
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal("ab#", EmittedCode.Match(assembly, "Grammar", "TryMarked", "ab").Value);
	}

	[Fact]
	public void A_named_rule_can_repeat_its_result_type_as_a_public_contract()
	{
		var result = Compile("Word : @string = w: ['a'..'z']+ => @(w)\nparse Word as W : @string");
		EmittedCode.Quiet(result.Diagnostics);
		var assembly = EmittedCode.Compile(result.Sources.Single().Text);
		Assert.Equal("abc", EmittedCode.Match(assembly, "Grammar", "TryW", "abc").Value);
	}

	[Fact]
	public void A_rebinding_replaces_a_rule_with_an_expression()
	{
		// §5.1's other reference position. What replaces a rule may be any operand, and
		// it becomes a rule declared where the `with` is written — so the substitution
		// reads what surrounds it, exactly as a rule written there would.
		var result = Compile("""
			Comma = ','
			Item  = ['a'..'z']+
			List  = Item & (Comma & Item)*

			parse List with (Comma = (',' | ';')) as Loose
			parse List as Tight
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryLoose", "a,b;c").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryLoose", "a,b,c").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryTight", "a,b;c").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryTight", "a,b,c").IsSuccess);
	}

	/// <summary>
	/// A publication an action reads the input again with is read under the substitution the
	/// action was cloned for.
	/// </summary>
	/// <remarks>
	/// <c>Start</c>'s action hands what follows the colon to <c>TryReadName</c>. Under
	/// <c>Loose</c> that name means <c>ReadName</c> as <c>Loose</c> reads it — capitals and all —
	/// and nobody published that, so the generator does, privately.
	/// </remarks>
	[Theory]
	[InlineData("TryPlain", "ab:cd", "ab+cd")]
	[InlineData("TryPlain", "ab:CD", "ab")]
	[InlineData("TryLoose", "ab:CD", "ab+CD")]
	[InlineData("TryLoose", "Ab:CD", "Ab+CD")]
	public void A_publication_named_in_an_action_is_read_under_the_same_substitution(
		string method, string input, string expected)
	{
		var result = Compile(Rereading);

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal(expected, EmittedCode.Match(assembly, "Grammar", method, input).Value);
	}

	/// <summary>Where the author publishes it under that substitution, the name is theirs.</summary>
	[Fact]
	public void And_where_the_author_publishes_that_reading_the_name_is_that_one()
	{
		var result = Compile(Rereading + "parse Name with (Word = Wide) as ReadLooseName\n");

		EmittedCode.Quiet(result.Diagnostics);

		var source = result.Sources[0].Text;

		Assert.DoesNotContain("ReadName_With", source);
		Assert.Contains("TryReadLooseName(rest)", source);

		var assembly = EmittedCode.Compile(source);

		Assert.Equal("Ab+CD", EmittedCode.Match(assembly, "Grammar", "TryLoose", "Ab:CD").Value);
	}

	/// <summary>A substitution that changes nothing the publication reads leaves its name alone.</summary>
	[Fact]
	public void But_a_substitution_that_changes_nothing_it_reads_leaves_the_name_alone()
	{
		var result = Compile("""
			Word  : @string = w: ['a'..'z']+ => @(w)
			Mark  = ':'
			Colon = ';'
			Name  : @string = w: Word => @(w)
			Start : @string = head: Word & Mark & rest: ['a'..'z']+ => @(TryReadName(rest).IsSuccess ? head + "+" + rest : head)

			parse Name as ReadName
			parse Start with (Mark = Colon) as Semicolon
			""");

		EmittedCode.Quiet(result.Diagnostics);

		Assert.DoesNotContain("ReadName_With", result.Sources[0].Text);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal("ab+cd", EmittedCode.Match(assembly, "Grammar", "TrySemicolon", "ab;cd").Value);
	}

	/// <summary>A rule may build a generic C# type, and a sequence may collect one.</summary>
	[Fact]
	public void A_rule_may_build_a_generic_type()
	{
		var result = Compile("""
			@using System.Collections.Generic;

			Pair  : @KeyValuePair<string, int> = key: ['a'..'z']+ & '=' & value: ['0'..'9']+
				=> @(new KeyValuePair<string, int>(key, int.Parse(value)))
			Pairs : @KeyValuePair<string, int>[] = Pair & (',' & Pair)*

			parse Pairs
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal(
			new[] { new KeyValuePair<string, int>("a", 1), new KeyValuePair<string, int>("bc", 23) },
			EmittedCode.Match(assembly, "Grammar", "TryParsePairs", "a=1,bc=23").Value);
	}

	const string Rereading = """
		Word  : @string = w: ['a'..'z']+ => @(w)
		Wide  : @string = w: ['a'..'z' | 'A'..'Z']+ => @(w)
		Name  : @string = w: Word => @(w)
		Start : @string = head: Word & ':' & rest: ['a'..'z' | 'A'..'Z']+ => @(TryReadName(rest).IsSuccess ? head + "+" + rest : head)

		parse Name as ReadName
		parse Start as Plain
		parse Start with (Word = Wide) as Loose

		""";

	// ── What a publication answers with (§7.5) ──────────────────────────────────

	[Theory]
	[InlineData("ab",  "Success")]
	[InlineData("ax",  "NoMatch")]
	[InlineData("a",   "Starved")]
	[InlineData("",    "Starved")]
	[InlineData("abc", "NoMatch")]
	public void A_match_says_which_kind_of_answer_it_is(string input, string expected)
	{
		// §7.5: `IsSuccess` answers the question most callers ask; `Outcome` answers the
		// one the two failures differ on. Input that ran out is not input that did not
		// fit — a caller reading a stream wants a longer read, one reading a document
		// wants a message — and both are exact: the furthest the parse followed is
		// either the end of the input or a character that did not belong there.
		var result = Compile("""
			Start = "ab"
			parse Start
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal(expected, EmittedCode.Outcome(assembly, "Grammar", "TryParseStart", input));
	}

	[Fact]
	public void And_IsSuccess_still_answers_what_it_always_did()
	{
		// Kept as it was, and now derived: one property is what the other says about
		// success, so the two can never disagree.
		var result = Compile("Start = \"ab\"\nparse Start");
		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "ab").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "ax").IsSuccess);
	}

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

		EmittedCode.Quiet(result.Diagnostics);

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

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal("ab!", EmittedCode.Match(assembly, "Grammar", "TryParseBang",  "ab").Value);
		Assert.Equal("ab?", EmittedCode.Match(assembly, "Grammar", "TryParseQuery", "ab").Value);
	}

	[Fact]
	public void And_refuses_a_value_standing_where_a_recognizer_goes()
	{
		// The other half of the same silence: §4.2 allows a value where a value is
		// expected, and an operand is not one of those places. It used to lower to an
		// element set with nothing in it, so the parse refused everything while naming a
		// set the author never wrote — found by writing `open & item & close` against
		// `open: char` in an example.
		Refused(
			GrammarNormalizer.UnbuiltCall,
			"Bracketed(item, open: char) = open & item\nWord = ['a'..'z']+\nStart = Bracketed(Word, '[')");
	}

	[Fact]
	public void And_refuses_a_rule_where_a_value_was_declared()
	{
		// The declaration says which kind the parameter is, and a rule is not a value —
		// taken as a recognizer, it would be the declaration meaning one thing to the
		// author and another to the compiler.
		Refused(
			GrammarNormalizer.UnbuiltCall,
			"Padded(item, pad: char) = item & @(pad)\nWord = ['a'..'z']+\nSpace = ' '\nStart = Padded(Word, Space)");
	}

	[Fact]
	public void A_number_still_reaches_a_parameter_that_declared_its_type()
	{
		// The half that is built: `n: int` is a value, and a number is a value.
		Assert.True(Matches("Digits(n: int) = ['0'..'9']{n}\nStart = Digits(4)", "2026"));
	}

	// ── Backtracking (§11) ───────────────────────────────────────────────────────

	// A greedy operand that took too much has to give it back when what follows fails.
	// Every one of these matched nothing before the recognizer became a machine with a
	// stack of the points it could have gone another way.

	[Fact]
	public void Optional_gives_back()
	{
		Assert.True(Matches("Start = 'a'? & 'a'", "a"));
	}

	[Fact]
	public void Star_gives_back()
	{
		Assert.True(Matches("Start = 'a'* & 'a'", "a"));
	}

	[Fact]
	public void Plus_gives_back()
	{
		Assert.True(Matches("Start = 'a'+ & 'a'", "aa"));
	}

	[Fact]
	public void Counted_gives_back()
	{
		Assert.True(Matches("Start = 'a'{1,2} & 'a'", "aa"));
	}

	[Fact]
	public void A_choice_gives_back_across_the_rest_of_the_sequence()
	{
		// The specification's own counterexample: §11 rests the rule that alternatives
		// may never be reordered on this working.
		Assert.True(Matches("""Start = ("x" | "xy") & 'y'""", "xy"));
	}

	[Fact]
	public void And_keeps_giving_back_until_something_fits()
	{
		Assert.True(Matches("Start = ['a'..'z']* & 'c' & ['a'..'z']*", "abcde"));
	}

	[Theory]
	[InlineData("aaab", true)]
	[InlineData("aaa", false)]
	public void Nested_repetition_backtracks(string input, bool expected)
	{
		Assert.Equal(expected, Matches("Start = ('a'+)+ & 'b'", input));
	}

	[Fact]
	public void Backtracking_does_not_make_a_failing_match_succeed()
	{
		Assert.False(Matches("Start = 'a'* & 'b'", "aaa"));
	}

	[Fact]
	public void A_lookahead_leaves_nothing_behind_to_backtrack_into()
	{
		Assert.True(Matches("Start = ?=('a' | 'b') & 'a' & 'b'", "ab"));
	}

	[Theory]
	[InlineData("ab", true)]
	[InlineData("ax", false)]
	public void A_positive_lookahead_asks_without_consuming(string input, bool expected)
	{
		// It consumes nothing, so the 'b' after it matches the same character it looked at.
		Assert.Equal(expected, Matches("Start = 'a' & ?='b' & 'b'", input));
	}

	[Theory]
	[InlineData("ac", true)]
	[InlineData("ab", false)]
	public void A_negative_lookahead_refuses_what_it_finds(string input, bool expected)
	{
		// The other half of §3.6, and the only one nothing tested for its own sake: `eof`
		// lowers to `?![^ ]`, so it was exercised only through that.
		Assert.Equal(expected, Matches("Start = 'a' & ?!'b' & ['a'..'z']", input));
	}

	[Theory]
	[InlineData("Name = \"x\" | \"xy\"")]
	[InlineData("Name = \"xy\" | \"x\"")]
	public void Backtracking_crosses_a_rule_boundary(string name)
	{
		Assert.True(Matches($"Start = Name & 'y'\n{name}", "xy"));
	}

	[Fact]
	public void A_repetition_can_give_input_back_across_a_rule_boundary()
	{
		Assert.True(Matches("Start = Run & 'a' & 'b'\nRun = 'a'+", "aaab"));
		Assert.False(Matches("Start = { Run } & 'a' & 'b'\nRun = 'a'+", "aaab"));
	}

	[Fact]
	public void An_atomic_group_commits_a_called_rule_too()
	{
		Assert.False(Matches("Start = { Name } & 'y'\nName = \"xy\" | \"x\"", "xy"));
	}

	[Fact]
	public void And_the_same_expressions_in_one_rule_do_backtrack()
	{
		Assert.True(Matches("Start = (\"xy\" | \"x\") & 'y'", "xy"));
	}

	[Fact]
	public void An_atomic_group_discards_its_internal_choices_after_success()
	{
		Assert.False(Matches("Start = { \"xy\" | \"x\" } & 'y'", "xy"));
		Assert.True(Matches("Start = { \"xz\" | \"x\" } & 'y'", "xy"));
	}

	[Fact]
	public void Nesting_a_rule_deep_inside_itself()
	{
		// Backtracking is a machine inside a rule and an ordinary call between rules, so
		// nesting costs the process stack — about 2700 levels on the default one, which
		// docs/status.md states and explains. This is well under it, and is here so that a
		// change making frames heavier shows up as a failure rather than in production.
		Assert.True(Matches(
			"Expr = '(' & Expr & ')' | 'x'\nStart = Expr",
			new string('(', 1000) + "x" + new string(')', 1000)));
	}

	[Fact]
	public void Repetition_longer_than_the_first_stack_page()
	{
		// The stack starts at 48 ints and grows; this needs far more frames than that.
		Assert.True(Matches("Start = ['a'..'z']* & 'z'", new string('a', 500) + "z"));
	}

	// ── Unicode categories (§3.1) ────────────────────────────────────────────────

	[Theory]
	[InlineData("AB", true)]
	[InlineData("Ab", false)]
	public void Category_by_abbreviation(string input, bool expected)
	{
		Assert.Equal(expected, Matches(@"Start = [\p{Lu}]+", input));
	}

	[Theory]
	[InlineData("abc", true)]
	[InlineData("ab1", false)]
	public void A_category_group_is_every_category_in_it(string input, bool expected)
	{
		Assert.Equal(expected, Matches(@"Start = [\p{L}]+", input));
	}

	[Fact]
	public void Digits_by_category()
	{
		Assert.True(Matches(@"Start = [\p{Nd}]+", "2026"));
	}

	[Fact]
	public void An_unknown_category_is_refused()
	{
		Refused(GramLexer.UnknownCategory, @"Start = [\p{Zz}]");
	}

	// ── References inside an element set (§3.1) ──────────────────────────────────

	[Theory]
	[InlineData("ab", true)]
	[InlineData("a1", false)]
	public void A_set_may_name_an_elementary_rule(string input, bool expected)
	{
		Assert.Equal(expected, Matches("Letter = ['a'..'z']\nStart = [Letter]+", input));
	}

	[Theory]
	[InlineData("a1", true)]
	[InlineData("a-", false)]
	public void And_is_merged_with_the_rest_of_the_set(string input, bool expected)
	{
		Assert.Equal(expected, Matches("Letter = ['a'..'z']\nStart = [Letter | '0'..'9']+", input));
	}

	[Fact]
	public void A_rule_declared_after_the_set_that_names_it_still_merges()
	{
		Assert.True(Matches("Start = [Letter]+\nLetter = ['a'..'z']", "ab"));
	}

	[Fact]
	public void Complementing_a_set_complements_what_was_merged_into_it()
	{
		Assert.False(Matches("Letter = ['a'..'z']\nStart = [^ Letter]+", "ab"));
	}

	[Fact]
	public void A_rule_that_is_not_one_element_cannot_be_in_a_set()
	{
		Refused(GrammarNormalizer.UnsupportedElement, "Pair = 'a' & 'b'\nStart = [Pair]");
	}

	// ── Literals ────────────────────────────────────────────────────────────────

	[Fact]
	public void A_character_literal_holds_one_character()
	{
		Refused(GramLexer.MalformedCharacter, "Start = 'ab'");
	}

	[Fact]
	public void An_empty_character_literal_is_refused()
	{
		Refused(GramLexer.MalformedCharacter, "Start = ''");
	}

	[Theory]
	[InlineData(@"'\0'", "\0")]
	[InlineData(@"'\a'", "\a")]
	[InlineData(@"'\v'", "\v")]
	[InlineData(@"'é'", "é")]
	[InlineData("'\u2028'", "\u2028")]     // a line separator: legal in a grammar, not in C# source
	public void Control_and_non_ascii_characters_survive_emission(string literal, string input)
	{
		Assert.True(Matches($"Start = {literal}", input));
	}

	[Theory]
	[InlineData("http")]
	[InlineData("HTTP")]
	[InlineData("Http")]
	[InlineData("hTtP")]
	public void A_trailing_i_matches_any_case(string input)
	{
		Assert.True(Matches("""Start = "http"i""", input));
	}

	[Fact]
	public void A_trailing_i_still_refuses_a_different_word()
	{
		Assert.False(Matches("""Start = "http"i""", "htttp"));
	}

	[Theory]
	[InlineData("a")]
	[InlineData("A")]
	public void A_single_character_literal_takes_i_too(string input)
	{
		Assert.True(Matches("Start = 'a'i", input));
	}

	[Fact]
	public void A_digit_has_no_case_to_ignore_but_i_does_not_mind()
	{
		Assert.True(Matches("Start = '5'i", "5"));
	}

	[Fact]
	public void Without_i_the_literal_stays_case_sensitive()
	{
		Assert.False(Matches("""Start = "http" """, "HTTP"));
	}

	[Theory]
	[InlineData("htTP", true)]      // "ht" case-sensitive, "tp"i folded — this is what both allow
	[InlineData("HTtp", false)]     // "ht" wrong case — adjacent-literal merging must not have
									// silently dropped the second literal's own `i`
	public void Adjacent_literals_of_different_case_sensitivity_do_not_merge(string input, bool expected)
	{
		Assert.Equal(expected, Matches("""Start = "ht" & "tp"i""", input));
	}

	// ── Where a failure is reported ─────────────────────────────────────────────

	/// <summary>The message and the position a grammar refuses an input with.</summary>
	static (string Error, long Position) Refusal(string grammar, string input)
	{
		var (isSuccess, _, error, position) = Parsed(grammar, input);

		Assert.False(isSuccess, input);

		return (error!, position);
	}

	[Fact]
	public void A_refusal_names_the_position_the_input_stopped_making_sense_at()
	{
		Assert.Equal(2, Refusal("""Start = "ab" & ['c'] & ['d']""", "abXY").Position);
	}

	[Fact]
	public void The_position_is_the_character_that_did_not_fit_not_the_operand_s_start()
	{
		// `"abcd"` is one operand and it starts at 0, but the character that did not fit
		// is at 2 — the offset is recorded at each failing test rather than one position
		// at the point of giving up.
		Assert.Equal(2, Refusal("""Start = "abcd" """, "abXY").Position);
	}

	[Fact]
	public void The_same_sharpening_applies_inside_a_merged_literal_run()
	{
		// "abcd"/"abef" share the prefix "ab" and compile through CompileLiterals's own
		// merged read rather than Node.Literal's loop — this input fails inside that
		// shared prefix itself (at 'X', index 1), which needs the identical fix rather
		// than a free ride off the other site.
		Assert.Equal(1, Refusal("""Start = "abcd" | "abef" """, "aXYZ").Position);
	}

	[Fact]
	public void It_is_the_furthest_reached_and_not_the_last_tried()
	{
		// The first alternative fails at 0 and the second gets to 2 before failing. What
		// is worth reporting is how far the input could be followed, so the position only
		// ever rises — a later, shallower failure does not overwrite a deeper one.
		Assert.Equal(2, Refusal("""Start = ("abc" | "ab") & 'z' """, "abq").Position);
	}

	[Fact]
	public void A_failure_inside_a_rule_is_the_caller_s_failure_too()
	{
		// The state is threaded through the call rather than returned, so a rule boundary
		// does not flatten the position back to where the call was made.
		Assert.Equal(
			2,
			Refusal("Inner = ['a'] & ['b'] & ['c']\nStart = Inner", "abq").Position);
	}

	// ── What was expected there (§11's first tier) ───────────────────────────────

	[Fact]
	public void A_refusal_names_the_one_thing_that_would_have_fit()
	{
		// `a: 'a'` keeps the two literals from merging into one "a)"-shaped token during
		// normalization (adjacent bare literals in a sequence would otherwise become
		// one, which §11's own concerns start only after) — and unlike a repetition,
		// a plain capture tries its operand exactly once, so nothing else ties with ')'
		// at the position it fails.
		Assert.Equal("Expected ')'.", Refusal("Start = a: 'a' & ')'", "a").Error);
	}

	[Fact]
	public void And_names_every_alternative_tried_at_the_same_furthest_position()
	{
		// "ab"/"ac" share a prefix and are read once as one merged run (Machine.cs's
		// CompileLiterals) — both alternatives fail together, at the same position, and
		// both are named.
		Assert.Equal("Expected \"ab\" or \"ac\".", Refusal("""Start = "ab" | "ac" """, "ax").Error);
	}

	[Fact]
	public void And_names_a_repeated_element_the_same_way_as_a_single_one()
	{
		Assert.Equal("Expected ['0'..'9'].", Refusal("Start = ['0'..'9']+", "").Error);
	}

	[Fact]
	public void With_nothing_left_to_try_it_says_the_input_ran_out()
	{
		// The guard fails at the end of the input and names no terminal of its own; a
		// clear failure at a tied position does not erase what an earlier one recorded
		// there, so this only holds when nothing else was tried at that exact position.
		Assert.Equal(
			"Expected more input.",
			Refusal("Start = 'a' & when @(false)", "a").Error);
	}

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
	public void A_case_insensitive_literal_names_itself_with_its_own_i()
	{
		Assert.Equal("Expected \"http\"i.", Refusal("""Start = "http"i""", "xxxx").Error);
	}

	[Fact]
	public void A_lookahead_does_not_report_how_far_it_looked()
	{
		// It reached position 2 inside itself and consumed nothing. Naming 2 would point
		// at input the match never needed; what failed is the lookahead, at 0.
		Assert.Equal(
			0,
			Refusal("Start = ?=(['a'] & ['b'] & ['z']) & ['a']", "abq").Position);
	}

	// ── Captures, and the value they build (§7.3) ───────────────────────────────

	/// <summary>Compiles and runs as <see cref="Matches"/> does, and hands back the value.</summary>
	// ── A shared leading operand, folded only where the fold is invisible ────────

	/// <summary>An operand that can give back is not shared, because sharing it would show.</summary>
	/// <remarks>
	/// The whole condition on left-factoring, from the side that says no. Two alternatives
	/// prefer a shorter reading of the operand that lets a tail fit; one alternative with the
	/// operand in front prefers the operand’s own reading and then chooses. On <c>xxy</c>
	/// the spelled-out form gives back to <c>x</c> so that <c>"xy"</c> fits and the first
	/// alternative wins; a folded form would take <c>xx</c> and fall to the second. Same text
	/// consumed, different alternative matched, different <c>=&gt;</c> run — which is why the
	/// fold is refused here and <c>GRAM4016</c> is left to say so.
	/// </remarks>
	[Fact]
	public void An_operand_that_can_give_back_is_not_shared()
	{
		Assert.Equal(
			"first:x",
			Built(
				"""
				Chunk = 'x'+
				Start : @string
					= a: Chunk & "xy" => @("first:" + a)
					| a: Chunk & "y"  => @("second:" + a)
				""",
				"xxy",
				// And says that it declined, which it did not use to: the operand does not
				// lead back to the rule, and reporting only where the cost compounds is the
				// scope this widened out of.
				GrammarNormalizer.SharedPrefix));
	}

	/// <summary>And one that cannot is, with nothing to show for it but the reading saved.</summary>
	/// <remarks>
	/// <c>"ab"</c> has one reading, so which alternative matches cannot turn on how much of
	/// it was read. Both spellings answer the same thing, which is the point: the fold is
	/// admitted exactly where it cannot be seen. That it happened is visible only in the work
	/// — <c>GeneratorDriverTests.And_a_shared_operand_is_read_once</c> counts it.
	/// </remarks>
	[Fact]
	public void And_one_that_cannot_is_shared()
	{
		Assert.Equal(
			"second:ab",
			Built(
				"""
				Word = "ab"
				Start : @string
					= a: Word & "cd" => @("first:" + a)
					| a: Word & "ce" => @("second:" + a)
				""",
				"abce"));
	}

	static object? Built(string grammar, string input, string? expected = null)
	{
		var (isSuccess, value, _, _) = Parsed(grammar, input, expected);

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
	public void A_repeated_capture_is_the_text_of_the_whole_run()
	{
		// §10 binds a capture tighter than a quantifier, so this is one capture repeated.
		// §7.3 gives it the text joined, which is the run rather than its last iteration.
		Assert.Equal("8080", Read(Built("Start = digits: ['0'..'9']+", "8080"), "Digits"));
	}

	[Fact]
	public void A_run_that_matched_nothing_is_empty_rather_than_absent()
	{
		Assert.Equal("", Read(Built("Start = digits: ['0'..'9']* & 'x'", "x"), "Digits"));
	}

	[Fact]
	public void An_option_that_was_not_taken_is_absent_rather_than_empty()
	{
		Assert.Null(Read(Built("Start = (sign: '-')? & 'x'", "x"), "Sign"));
	}

	[Fact]
	public void A_capture_of_a_rule_that_builds_holds_its_value()
	{
		Assert.Equal(
			"x",
			Read(Built("Inner = letter: 'x'\nStart = inner: Inner", "x"), "Inner", "Letter"));
	}

	[Theory]
	[InlineData("y", null)]
	[InlineData("xy", "x")]
	public void An_optional_rule_capture_is_absent_or_its_value(string input, string? expected)
	{
		Assert.Equal(
			expected,
			Read(Built("Item = letter: 'x'\nStart = (item: Item)? & 'y'", input), "Item", "Letter"));
	}

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
	public void The_same_name_in_two_alternatives_is_one_member()
	{
		Assert.Equal("y", Read(Built("Start = (v: \"xy\" | v: 'y')", "y"), "V"));
	}

	[Fact]
	public void A_repeated_capture_of_a_rule_is_a_sequence_of_its_values()
	{
		var value = Built("Item = letter: ['a'..'z']\nStart = items: Item+", "abc");
		var items = (Array)Read(value, "Items")!;

		Assert.Equal(3, items.Length);
		Assert.Equal(["a", "b", "c"], items.Cast<object>().Select(item => Read(item, "Letter")));
	}

	[Fact]
	public void An_empty_run_is_an_empty_sequence_rather_than_null()
	{
		Assert.Empty((Array)Read(
			Built("Item = letter: 'x'\nStart = items: Item* & 'y'", "y"), "Items")!);
	}

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
	public void A_capture_of_text_inside_a_repetition_is_still_the_run_it_matched()
	{
		// §7.3 keeps the two apart: a quantifier over text joins it, a quantifier over a
		// rule collects. Only the second is a sequence.
		Assert.Equal("8080", Read(Built("Start = digits: ['0'..'9']+", "8080"), "Digits"));
	}

	[Fact]
	public void And_text_captured_around_something_else_is_the_turns_joined()
	{
		// This was refused while the value was the span the turns lie in — where `'y'`
		// would have been swept in with them. Each turn records an entry of its own now,
		// and §10's value is those joined, so the shape is an ordinary one.
		Assert.Equal("xx", Read(Built("Start = (a: 'x' & 'y')+", "xyxy"), "A"));
	}

	[Fact]
	public void Nor_is_one_inside_a_lookahead()
	{
		Refused(GrammarNormalizer.UnbuiltCapture, "Start = ?=(a: 'x') & 'x'");
	}

	[Fact]
	public void One_name_cannot_hold_two_different_things()
	{
		Refused(
			GrammarNormalizer.CaptureTypeMismatch,
			"Item = a: 'x'\nStart = (v: Item | v: 'y')");
	}

	/// <summary>
	/// And says so once, where a fold anywhere in the grammar has the members worked out again.
	/// </summary>
	[Fact]
	public void And_says_so_once_where_the_grammar_is_folded()
	{
		Assert.Single(
			Compile(
				"Item = a: 'x'\n" +
				"Start = (v: Item | v: 'y') & Other\n" +
				"Other = D & 'a' | D & 'b'\n" +
				"D = ['0'..'9']").Diagnostics,
			one => one.Id == GrammarNormalizer.CaptureTypeMismatch);
	}

	// ── A rule that declares its own type and builds it (§7.3) ──────────────────

	[Fact]
	public void A_rule_may_name_a_C_sharp_type_and_say_how_to_build_it()
	{
		Assert.Equal(
			42,
			Built(
				"""
				@using System.Globalization;

				Start : @int = ['0'..'9']+ => @int.Parse(parserText, @CultureInfo.InvariantCulture)
				""",
				"42"));
	}

	[Fact]
	public void The_matched_text_is_supplied_as_parserText()
	{
		Assert.Equal(
			3,
			Built("Start : @int = ['a'..'z']+ => @(parserText.Length)", "abc"));
	}

	[Fact]
	public void Captures_reach_the_expression_by_their_own_names()
	{
		Assert.Equal(
			"b-a",
			Built("""Start : @string = a: 'a' & b: 'b' => @(b + "-" + a)""", "ab"));
	}

	[Theory]
	[InlineData("12", 24)]
	[InlineData("abc", 3)]
	public void Each_alternative_may_build_the_value_its_own_way(string input, int expected)
	{
		Assert.Equal(
			expected,
			Built(
				"""
				Start : @int = digits: ['0'..'9']+   => @(int.Parse(digits) * 2)
				             | letters: ['a'..'z']+  => @(letters.Length)
				""",
				input));
	}

	[Fact]
	public void An_alternative_that_was_tried_and_given_back_does_not_build_the_value()
	{
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
	}

	[Fact]
	public void A_construction_needs_a_type_to_build()
	{
		Refused(GrammarNormalizer.UnbuiltConstruction, "Start = ['0'..'9']+ => @(1)");
	}

	[Fact]
	public void And_a_type_needs_every_alternative_to_build_it()
	{
		Refused(
			GrammarNormalizer.UnbuiltConstruction,
			"""Start : @int = "a" => @(1) | "b" """);
	}

	[Fact]
	public void A_group_construction_has_its_own_value()
	{
		Assert.Equal(3, Built(
			"""Start : @int = value: ("a" => @(1) | "b" => @(2)) & 'c' => @(value + 1)""",
			"bc"));
	}

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
	[InlineData("1+2+3", 6)]
	[InlineData("1-2-3", -4)]     // (1-2)-3, not 1-(2-3)
	[InlineData("2*3+4", 10)]
	[InlineData("2+3*4", 14)]
	[InlineData("(2+3)*4", 20)]
	[InlineData("100/5/2", 10)]     // (100/5)/2, not 100/(5/2)
	[InlineData("7", 7)]
	public void A_calculator(string input, int expected)
	{
		Assert.Equal(expected, Built(Calculator + "Start : @int = value: Sum => @(value)", input));
	}

	[Fact]
	public void An_alternative_recursive_on_both_sides_is_refused()
	{
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
	}

	[Theory]
	[InlineData("-1-2", -3)]
	[InlineData("1--2", 3)]
	[InlineData("-1", -1)]
	public void A_unary_operator_written_at_its_own_level(string input, int expected)
	{
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
	}

	[Theory]
	[InlineData("a.b.c", "((a.b).c)")]
	[InlineData("a(b)[c].d", "(((a(b))[c]).d)")]
	[InlineData("a", "a")]
	public void A_rule_may_have_as_many_recursive_alternatives_as_it_likes(string input, string expected)
	{
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
	}

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
	[InlineData("1+2", 3)]
	[InlineData("1-2-3", -4)]    // << is left-associative: (1-2)-3
	[InlineData("2+3*4", 14)]    // 3*4 is stronger, so it is taken first
	[InlineData("2*3+4", 10)]
	[InlineData("(2+3)*4", 20)]
	[InlineData("-1-2", -3)]    // §4.3.1's own example: unary is stronger than binary
	[InlineData("-(1-2)", 1)]
	public void One_rule_of_strengths_parses_a_whole_expression_language(string input, int expected)
	{
		Assert.Equal(expected, Built(Powers, input));
	}

	/// <summary>The same operator both ways round — the whole of what the markers say.</summary>
	static string Associates(string marker)
	{
		return $"""
		Start : @int = left: Start & '-' & right: Start {marker} => @(left - right)
		             | digits: ['0'..'9']+                       => @int.Parse(digits)
		""";
	}

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
	public void An_alternative_recursive_on_both_sides_is_what_a_strength_settles()
	{
		// Refused without a strength (ordered choice cannot say which way it groups), and
		// the ordinary case with one. The refusal and the feature are the same shape.
		Assert.Equal(2, Built(Associates(">> 1"), "1-2-3"));
	}

	[Fact]
	public void A_prefix_needs_no_loop_and_gets_none()
	{
		// Nothing but a prefix and an atom: every alternative is a base, so the rule climbs
		// without ever looping. `--1` works because the operand is parsed at 4, where the
		// prefix itself still lives.
		Assert.Equal(1, Built(
			"""
			Start : @int = '-' & operand: Start >> 4 => @(-operand)
			             | digits: ['0'..'9']+       => @int.Parse(digits)
			""",
			"--1"));
	}

	[Fact]
	public void A_recursive_alternative_without_a_strength_among_ones_that_have_it_is_refused()
	{
		// §4.3.1: a rule uses one convention or the other. Half of each would be two
		// answers to the same question.
		Refused(
			GrammarNormalizer.UnbuiltBinding,
			"""
			Start : @int = left: Start & '+' & right: Start << 1 => @(left + right)
			             | left: Start & '-' & right: Start      => @(left - right)
			             | digits: ['0'..'9']+                   => @int.Parse(digits)
			""");
	}

	[Fact]
	public void A_strength_on_something_with_no_operand_is_refused()
	{
		// A strength says how tightly the operand to the right is read, and there is none.
		Refused(
			GrammarNormalizer.UnbuiltBinding,
			"""
			Start : @int = left: Start & '+' & right: Start << 1 => @(left + right)
			             | digits: ['0'..'9']+              >> 9 => @int.Parse(digits)
			""");
	}

	// ── `recover` (§8.2) ────────────────────────────────────────────────────────

	const string Records = """
		Row   = name: ['a'..'z']+ & eol
		Start = rows: Row* recover eol => @(new Row("!" + parserText))
		""";

	[Fact]
	public void A_broken_element_is_stepped_over_and_the_rest_are_read()
	{
		// The second line begins a Row and breaks in the middle of one, which is an error
		// rather than the end of the sequence. What follows is read.
		Assert.Equal(
			["aa", "!b1b", "cc"],
			((Array)Read(Built(Records, "aa\nb1b\ncc\n"), "Rows")!)
				.Cast<object>()
				.Select(row => Read(row, "Name")));
	}

	[Fact]
	public void A_valid_continuation_ends_the_recovering_sequence()
	{
		// The complete continuation succeeds, so no Row is attempted at the trailer.
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

	}

	[Fact]
	public void Recovery_does_not_require_an_atomic_discriminator()
	{
		Assert.Equal(
			["!X"],
			((Array)Read(
				Built("""
					Row   = "R|" & name: ['a'..'z']+ & eol
					Start = rows: Row* recover eol => @(new Row("!" + parserText)) & "T|" & eol
					""",
					"X\nT|\n"),
				"Rows")!)
				.Cast<object>()
				.Select(row => Read(row, "Name")));

	}

	[Fact]
	public void A_broken_element_at_the_end_takes_what_is_left()
	{
		Assert.Equal(
			["aa", "!b1b"],
			((Array)Read(Built(Records, "aa\nb1b"), "Rows")!)
				.Cast<object>()
				.Select(row => Read(row, "Name")));
	}

	[Fact]
	public void A_synchronization_match_must_consume_input()
	{
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
	}

	[Fact]
	public void A_recovered_element_is_told_where_it_was_and_which_one_it_is()
	{
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
	}

	[Fact]
	public void A_recovered_element_knows_where_a_person_would_look_for_it()
	{
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
	}

	[Fact]
	public void A_recovered_element_knows_its_extent()
	{
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
	}

	[Fact]
	public void A_recovered_element_can_say_why_it_was_rejected()
	{
		// What would have fit where the input stopped being one, and where that was.
		Assert.Equal(
			["aa", "Expected ['\\r' | '\\n'] at 2:2.", "cc"],
			((Array)Read(
				Built("""
					Row   = name: ['a'..'z']+ & eol
					Start = rows: Row* recover eol => @(new Row(parserMessage))
					""",
					"aa\nb1b\ncc\n"),
				"Rows")!)
				.Cast<object>()
				.Select(row => Read(row, "Name")));
	}

	[Fact]
	public void A_recovered_element_is_told_where_it_went_wrong_and_what_would_have_fit()
	{
		// Where the element began, where reading it stopped, and what would have fit there: the
		// third line goes wrong at its comma, sixteen characters in.
		Assert.Equal(
			["2.40", "3.10", "3:1 3:17 26 '.'", "8.00"],
			((Array)Read(
				Built("""
					Row   = ' '* & name: Price & eol
					Price = ['0'..'9']+ & '.' & ['0'..'9']{2}
					Start = rows: Row* recover eol
					      => @(new Row(parserLine + ":" + parserColumn + " " +
					                   parserFailureLine + ":" + parserFailureColumn + " " + parserFailurePosition + " " +
					                   parserExpected))
					""",
					"2.40\n3.10\n" + new string(' ', 15) + "3,20\n8.00\n"),
				"Rows")!)
				.Cast<object>()
				.Select(row => Read(row, "Name")));
	}

	[Fact]
	public void An_ordinary_repetition_hands_an_element_back()
	{
		// One row, and `tail` gets the other — the repetition took both and gave one up.
		Assert.True(
			Matches("""
				Row   = name: ['a'..'z']+ & eol
				Start = rows: Row* & tail: ['a'..'z']+ & eol
				""",
				"aa\nbb\n"));
	}

	[Fact]
	public void A_recovering_repetition_asks_the_continuation_before_another_element()
	{
		// At every element boundary the complete continuation gets first refusal. Only if
		// it fails does the repetition ask for another Row, so `tail` owns the second line.
		Assert.True(
			Matches("""
				Row   = name: ['a'..'z']+ & eol
				Start = rows: Row* recover eol => @(new Row("!" + parserText)) & tail: ['a'..'z']+ & eol
				""",
				"aa\nbb\n"));
	}

	[Fact]
	public void Recover_belongs_on_a_repetition()
	{
		Refused(GrammarNormalizer.UnbuiltRecovery, "Start = 'a' recover eol");
	}

	/// <summary>
	/// A <c>recover</c> inside a lookahead is refused where it was written, over characters and
	/// over tokens alike, through a parameter the lookahead reads, inside what another recovery
	/// synchronizes on, and in trivia: the rule plans no recovery for a lookahead, so the
	/// repetition was read as a plain one and the parser refused what the grammar said it would
	/// recover from.
	/// </summary>
	[Theory]
	[InlineData("trivia = { ' '* }\nStart = ?=(Row* recover ';' & 'z') & any*", false)]
	[InlineData("trivia = { ' '* }\nStart = ?=(Row* recover ';' & 'z') & any*", true)]
	[InlineData("trivia = { ' '* }\nStart = ?!(Row* recover ';' & 'y') & any*", false)]
	[InlineData("trivia = { ' '* }\nAhead(x) = ?=(x & 'z')\nStart = Ahead(Row* recover ';') & any*", false)]
	[InlineData("Start = 'b'* recover (?=(Row* recover ';') & ';') & any*", false)]
	[InlineData("trivia = ?=(Row* recover ';') & ' '*\nStart = 'z' & 'z'", false)]
	public void Recover_inside_a_lookahead_is_refused(string rules, bool lexical)
	{
		var grammar = "Row = 'a' & ';'\n" + rules + "\nparse Start";

		var told = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Lexical = lexical })
			.Diagnostics;

		var refused = Assert.Single(told, static one => one.Id == GrammarNormalizer.RecoveryInLookahead);

		Assert.Equal(GramSeverity.Error, refused.Severity);
		Assert.Equal("Row* recover ';'", grammar.Substring(refused.Position, refused.Length));
	}

	/// <summary>
	/// The way the refusal points to: a recovering repetition named as a rule and called from a
	/// lookahead recovers there, since a rule plans its own.
	/// </summary>
	[Fact]
	public void A_rule_called_from_a_lookahead_recovers()
	{
		const string grammar = """
			trivia = { ' '* }
			Row  = 'a' & ';'
			Rows = Row* recover ';'
			Start = ?=(Rows & 'z') & any*
			""";

		Assert.True(Matches(grammar, "aa;z"));
		Assert.False(Matches(grammar, "aa;y"));
	}

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
		Assert.Contains("OnRecovered(\"Row\", text.Slice(recovered.Position, recovered.Value - recovered.Position).ToString(), recovered.Position, parser.Located.LineAt(text, recovered.Position), parser.Located.ColumnAt(text, recovered.Position), recovered.RuleIndex", source);

		// The elements that did match still collect — it is only the broken one that is
		// dropped — so what must be absent is a factory, not the collecting.
		Assert.Contains("captured0Count", source);
		Assert.DoesNotContain("_Recover(", source);
	}

	/// <summary>The C# a grammar compiles to, when what is under test is the code itself.</summary>
	static string Emitted(string grammar)
	{
		var result = Compile(grammar + "\nparse Start");

		EmittedCode.Quiet(result.Diagnostics);

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
	public void And_the_parse_still_steps_over_the_broken_one()
	{
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
	}

	[Fact]
	public void And_a_grammar_that_recovers_with_a_factory_declares_no_hook()
	{
		// The channel is emitted for the grammars that report on it and no others.
		Assert.DoesNotContain("OnRecovered", Emitted(Records));
	}

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
	public void Without_the_brackets_it_binds_tighter_than_the_choice()
	{
		// `recover` takes one operand, so the `|` belongs to the enclosing expression:
		// this is `(fields: Field* recover '|') | eol`, not a choice of two sync points.
		// Precedence, the same as `a & b | c` — and the reason the brackets are not
		// optional.
		Assert.Contains(
			"Recovering",
			GramParser.Parse(GramLexer.Tokenize(
				"Start = fields: Field* recover '|' | eol\nField = ['a'..'z']+",
				RoslynCSharpScanner.Instance)).File.ToString());
	}

	[Fact]
	public void Every_repetition_of_a_rule_may_recover()
	{
		// Each marked repetition is its own: its own sync, its own `=>`, its own sequence
		// for a rejection to arrive in, and its own plan in the arena — which has
		// dispatched a recovery by plan since there was one plan. The two factories differ
		// by name and by nothing else.
		var result = Compile("""
			Row   : @string = t: ['a'..'z']+ & eol => @(t)
			Sheet : @string = "H" & eol & head: Row* recover eol => @("!" + parserText)
			                & "B" & eol & body: Row* recover eol => @("?" + parserText)
			                & eof
			                => @(string.Join(",", head) + "|" + string.Join(",", body))

			parse Sheet
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var source = result.Sources[0].Text;

		Assert.Contains("Recognize_Sheet_Recover(",  source);
		Assert.Contains("Recognize_Sheet_Recover2(", source);

		var assembly = EmittedCode.Compile(source);

		// A line that begins a Row and breaks in the middle of one is stepped over by the
		// `recover` that owns that repetition, and each rejection is built by that
		// repetition's own `=>` — which is the whole of what having two of them means.
		var match = EmittedCode.Match(assembly, "Grammar", "TryParseSheet", "H\na\nb1b\nB\nc\nd2d\n");

		Assert.True(match.IsSuccess, match.Error + " at " + match.Position);
		Assert.Equal("a,!b1b|c,?d2d", match.Value);
	}

	[Fact]
	public void A_recovery_that_builds_needs_a_sequence_to_build_into()
	{
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
	}

	[Fact]
	public void And_the_same_repetition_without_one_is_fine()
	{
		// §8.3: no `=>`, so nothing is collected and the rejection goes to the hook. The
		// sequence that is not there is not needed.
		EmittedCode.Quiet(Compile("""
			Row   = ['a'..'z']+ & eol
			Start = rows: Row* recover eol
			parse Start
			""").Diagnostics);
	}

	[Fact]
	public void Every_alternative_of_a_rule_being_left_recursive_is_refused()
	{
		Refused(GrammarNormalizer.LeftRecursion, "Start : @int = left: Start & 'x' => @(left)");
	}

	[Fact]
	public void Indirect_left_recursion_through_a_rule_that_does_something_is_still_refused()
	{
		// `Other` is not only a name for what it forwards — one of its alternatives is a
		// literal of its own — so unfolding it would put that literal, and whatever else
		// an intermediary might carry, into the fold's tail. Refused, as §4.3 says.
		Refused(
			GrammarNormalizer.LeftRecursion,
			"""
			Start = Other & 'x'
			Other = Start | 'y'
			""");
	}

	[Fact]
	public void And_the_tail_may_capture()
	{
		// The postfix chain every expression language is written as, which is what this
		// feature is for. Unfolding turns one alternative into one per source, and the
		// one leading with the rule itself becomes the fold's step while the others stay
		// bases — so a capture written in the tail stands both inside the loop and
		// outside it. What the two must agree on is what the author sees: a slot under a
		// fold's loop collects because a step is applied once per iteration, and the
		// step's `=>` is handed one of what it collected, which is a value either way.
		var result = Compile("""
			Word  : @string = w: ['a'..'z']+ => @(w)
			Chain : @string = c: Step => @(c) | c: Word => @(c)
			Step  : @string = head: Chain & '.' & tail: Word => @(head + "[" + tail + "]")

			parse Chain
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		// Left-associative, because that is where the recursion is: `a.b.c` is `(a.b).c`.
		Assert.Equal("a",       EmittedCode.Match(assembly, "Grammar", "TryParseChain", "a").Value);
		Assert.Equal("a[b]",    EmittedCode.Match(assembly, "Grammar", "TryParseChain", "a.b").Value);
		Assert.Equal("a[b][c]", EmittedCode.Match(assembly, "Grammar", "TryParseChain", "a.b.c").Value);
	}

	[Fact]
	public void And_so_is_a_mutual_recursion_where_both_sides_build()
	{
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
	}

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

		EmittedCode.Quiet(result.Diagnostics);

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

		EmittedCode.Quiet(result.Diagnostics);

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

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseList", "a,b").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseList", "a,b,c").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseList", "a").IsSuccess);
	}

	// ── `when` guards (§8.1) ───────────────────────────────────────────────────

	[Theory]
	[InlineData("12", true)]
	[InlineData("123", false)]
	public void A_guard_asks_a_question_of_the_text_so_far(string input, bool expected)
	{
		Assert.Equal(expected, Matches("Start = ['0'..'9']+ & when @(parserText.Length < 3)", input));
	}

	[Theory]
	[InlineData("ab", true)]
	[InlineData("ax", false)]
	public void And_of_the_captures_written_before_it(string input, bool expected)
	{
		Assert.Equal(
			expected,
			Matches("""Start = a: 'a' & b: ['a'..'z'] & when @(b == "b")""", input));
	}

	[Fact]
	public void A_failing_guard_is_a_non_match_and_a_sibling_is_tried()
	{
		// Recognition failure: saying no sends the match back into the choice rather than
		// ending it.
		Assert.True(Matches(
			"""Start = (a: "ab" & when @(a == "xy") | a: "ab") & 'c'""",
			"abc"));
	}

	[Fact]
	public void A_guard_may_stand_where_nothing_has_been_captured()
	{
		Assert.True(Matches("Start = ['0'..'9']+ & when @(true)", "7"));
	}

	// §7.1's C# recognizer contracts are fixed by syntactic position; their emitted calls
	// and ordinary C# diagnostics are tested with a host compilation in GeneratorDriverTests.

	[Fact]
	public void A_capture_may_not_take_a_name_that_is_supplied()
	{
		// The supplied names become parameters of the method a `=>` turns into, so a
		// capture of the same name wants one that is already taken. The prefix makes that
		// unlikely rather than impossible, and this is the backstop: before it, the
		// generated code simply did not compile, with an error pointing at a file the
		// author never wrote and saying nothing about the grammar.
		Refused(
			GrammarNormalizer.ReservedCaptureName,
			"Start : @string = parserText: ['a'..'z']+ => @(parserText)");
	}

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
	public void And_two_operands_that_could_be_it_are_refused()
	{
		// Two answers and nothing to say which. A grammar to rewrite rather than a choice
		// for the compiler to make quietly.
		Refused(
			GrammarNormalizer.UnbuiltConstruction,
			"""
			Start : Number = Number & '+' & Number
			Number : @int = digits: ['0'..'9']+ => @int.Parse(digits)
			""");
	}

	// ── Namespaces (§5) ──────────────────────────────────────────────────────────

	[Fact]
	public void Two_namespaces_may_each_have_a_rule_of_the_same_name()
	{
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
	}

	[Theory]
	[InlineData("1.5", true)]
	[InlineData("1 . 5", false)]
	public void A_namespace_shadows_Trivia_the_other_way_round(string input, bool expected)
	{
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
	}

	// ── Repetition counts ───────────────────────────────────────────────────────

	[Fact]
	public void A_count_too_large_for_an_int_is_a_diagnostic_and_not_a_crash()
	{
		Refused(GramParser.InvalidCount, "Start = 'a'{999999999999999999999999999}");
	}

	[Fact]
	public void A_range_that_can_never_match_is_a_diagnostic()
	{
		Refused(GramParser.InvalidCount, "Start = 'a'{5,2}");
	}

	[Theory]
	[InlineData("aa", true)]
	[InlineData("aaa", true)]
	[InlineData("a", false)]
	[InlineData("aaaa", false)]
	public void Bounded_repetition(string input, bool expected)
	{
		Assert.Equal(expected, Matches("Start = 'a'{2,3}", input));
	}

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

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryDefaultA", "b").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryDefaultA", "d").IsSuccess);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryNamespaceA", "d").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryNamespaceA", "b").IsSuccess);
	}

	/// <summary>
	/// A call into a rebinding namespace from outside it reads the namespace's specialization, not
	/// the rule as written: <c>Ns.E</c> resolves <c>A</c> through <c>D</c> wherever it is called from.
	/// </summary>
	/// <remarks>
	/// Only a publication used to be pointed at the specialization. A call in a rule body kept the
	/// original target, so <c>R = Ns.E</c> read <c>E</c> as though the header had not been written.
	/// </remarks>
	[Theory]
	[MemberData(nameof(CallsIntoARebindingNamespace))]
	public void A_call_from_outside_a_rebinding_namespace_reads_its_specialization(
		string inside, string outside, string accepted, string refused, CarrierKind carrier, bool direct)
	{
		var result = GramCompiler.Compile(
			$$"""
			B = 'b'
			D = 'd'
			A = B+ & ':' & B+

			namespace Ns with (B = D)
			{
				{{inside}}
			}

			R = {{outside}}
			parse R as ParseStart
			""",
			new GramCompilerOptions
			{
				ClassName     = "Grammar",
				Carrier       = carrier,
				Direct        = direct,
				CSharpScanner = RoslynCSharpScanner.Instance,
			});

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(
			result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static source => source.Text));

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", accepted).IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", refused).IsSuccess);
	}

	public static TheoryData<string, string, string, string, CarrierKind, bool> CallsIntoARebindingNamespace()
	{
		var data = new TheoryData<string, string, string, string, CarrierKind, bool>();

		(string Inside, string Outside, string Accepted, string Refused)[] shapes =
		[
			("E = A",              "Ns.E & \" f\"", "dd:d f", "bb:b f"),
			("E = A",              "Ns.E",          "dd:d",   "bb:b"),
			("E = A",              "'<' & Ns.E",    "<dd:d",  "<bb:b"),
			("E = A & '!'",        "Ns.E & \" f\"", "d:d! f", "b:b! f"),
			("E = A\nG = E & '!'", "Ns.G",          "d:dd!",  "b:bb!"),
			("E(x) = A & x",       "Ns.E('!')",     "dd:d!",  "bb:b!"),
		];

		foreach (var (inside, outside, accepted, refused) in shapes)
			foreach (var (carrier, direct) in new[] { (CarrierKind.Auto, true), (CarrierKind.Tape, false), (CarrierKind.Immediate, true) })
				data.Add(inside, outside, accepted, refused, carrier, direct);

		return data;
	}

	[Fact]
	public void A_rule_typed_by_a_rule_of_a_rebinding_namespace_takes_the_specializations_type()
	{
		// §4.1 case 3 over §5.1: `R : Ns.E` takes the value of the `Ns.E` it calls, which is the
		// specialization's — `D`'s decimal, not `B`'s int.
		var result = CompileResolved("""
			B : @int     = 'b' => @(1)
			D : @decimal = 'd' => @(2m)

			namespace Ns with (B = D)
			{
				E : B = B
			}

			R : Ns.E = Ns.E
			parse R as ParseStart
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal(2m, EmittedCode.Match(assembly, "Grammar", "TryParseStart", "d").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "b").IsSuccess);
	}

	[Fact]
	public void A_publications_rebinding_of_a_rule_of_a_rebinding_namespace_replaces_what_is_called()
	{
		// `R` calls the specialization of `Ns.E`, so that is what the directive's own `with`
		// replaces; replacing only the original would find no call to replace.
		var result = Compile("""
			using Ns;

			B = 'b'
			D = 'd'
			F = 'f'

			namespace Ns with (B = D)
			{
				E = B
			}

			R = Ns.E
			parse R as Plain
			parse R with (E = F) as Replaced
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryPlain", "d").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryPlain", "b").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryReplaced", "f").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryReplaced", "d").IsSuccess);
	}

	[Fact]
	public void A_rule_typed_by_a_rule_of_a_rebinding_namespace_follows_a_publications_replacement()
	{
		// `R : Ns.E` takes the specialization's type, and a directive that replaces `E` again
		// replaces that type with it: the replaced reading builds `F`'s double.
		var result = CompileResolved("""
			using Ns;

			B : @int     = 'b' => @(1)
			D : @decimal = 'd' => @(2m)
			F : @double  = 'f' => @(3.0)

			namespace Ns with (B = D)
			{
				E : B = B
			}

			R : Ns.E = Ns.E
			parse R as Plain
			parse R with (E = F) as Replaced
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal(2m, EmittedCode.Match(assembly, "Grammar", "TryPlain", "d").Value);
		Assert.Equal(3.0, EmittedCode.Match(assembly, "Grammar", "TryReplaced", "f").Value);
	}

	[Fact]
	public void A_publications_rebinding_of_a_parameterized_rule_of_a_rebinding_namespace_replaces_its_instance()
	{
		// `R` calls the specialization of the instance `E('!')`; replacing `E` replaces that
		// instance with `F('!')`.
		var result = Compile("""
			using Ns;

			B = 'b'
			D = 'd'
			F(x) = 'f' & x

			namespace Ns with (B = D)
			{
				E(x) = B & x
				G = E('!')
			}

			R = Ns.E('!')
			parse Ns.G as ParseG
			parse R as Plain
			parse R with (E = F) as Replaced
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryPlain", "d!").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryPlain", "b!").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryReplaced", "f!").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryReplaced", "d!").IsSuccess);
	}

	[Fact]
	public void A_publications_rebinding_of_an_argument_from_a_rebinding_namespace_survives_rebuilding_the_instance()
	{
		// `L(Ns.E)` replaced by `F` is `F` of the same argument, the specialization of `Ns.E`, and
		// the same directive's `E = Z` replaces that argument too.
		var result = Compile("""
			using Ns;

			B = 'b'
			D = 'd'
			Z = 'z'
			L(x) = x & '!'
			F(x) = x & '?'

			namespace Ns with (B = D)
			{
				E = B
			}

			R = L(Ns.E)
			parse R as Plain
			parse R with (L = F) as Swapped
			parse R with (L = F, E = Z) as Replaced
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryPlain", "d!").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TrySwapped", "d?").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TrySwapped", "b?").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryReplaced", "z?").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryReplaced", "d?").IsSuccess);
	}

	[Fact]
	public void A_replacement_from_a_rebinding_namespace_is_checked_with_the_specializations_type()
	{
		// `Ns.E` in `B`'s place is the specialization, whose value is `D`'s decimal: it does not
		// fit where `R` takes `B`'s int, whatever the unspecialized `E` would have built.
		var result = CompileResolved("""
			B : @int     = 'b' => @(1)
			C : @int     = 'c' => @(2)
			D : @decimal = 'd' => @(3m)

			namespace Ns with (C = D)
			{
				E : C = C
			}

			R : @int = x: B => @(x + 1)
			parse R as Plain
			parse R with (B = Ns.E) as Replaced
			""");

		Assert.Contains(result.Diagnostics, one => one.Id == GrammarNormalizer.IncompatibleRebinding);
	}

	[Fact]
	public void A_replacement_another_header_specializes_again_is_checked_as_that_header_reads_it()
		// `Other` puts `Ns.E` in `B`'s place as `Ns` makes it, `D`'s decimal, and its own
		// `C = Z` goes on top, where nothing calls `C` any more: an int is expected, a decimal
		// arrives, and the grammar is refused.
	{
		var result = CompileResolved("""
			B : @int     = 'b' => @(1)
			C : @int     = 'c' => @(2)
			Z : @int     = 'z' => @(4)
			D : @decimal = 'd' => @(3m)

			namespace Ns with (C = D)
			{
				E : C = C
				parse E as ParseE
			}

			namespace Other with (B = Ns.E, C = Z)
			{
				R : @int = x: B => @(x + 1)
				parse R as ParseR
			}
			""");

		Assert.Contains(result.Diagnostics, one => one.Id == GrammarNormalizer.IncompatibleRebinding);
	}

	[Fact]
	public void An_expressions_replacement_from_a_rebinding_namespace_is_checked_with_the_specializations_type()
	{
		// The same as a publication's: `Ns.E` in `B`'s place is the specialization, a decimal.
		var result = CompileResolved("""
			B : @int     = 'b' => @(1)
			C : @int     = 'c' => @(2)
			D : @decimal = 'd' => @(3m)

			namespace Ns with (C = D)
			{
				E : C = C
			}

			R : @int = x: (B with (B = Ns.E)) => @(x + 1)
			parse R as ParseR
			""");

		Assert.Contains(result.Diagnostics, one => one.Id == GrammarNormalizer.IncompatibleRebinding);
	}

	[Fact]
	public void An_expressions_replacement_is_checked_apart_from_what_another_expression_cloned()
	{
		// `X` clones `Ns.E` for its own `with`; that says nothing about what `R`'s `with` puts in
		// `B`'s place, which is still the decimal specialization.
		var result = CompileResolved("""
			B : @int     = 'b' => @(1)
			C : @int     = 'c' => @(2)
			Z : @int     = 'z' => @(4)
			D : @decimal = 'd' => @(3m)

			namespace Ns with (C = D)
			{
				E : C = C
			}

			X = Ns.E with (C = Z)
			R : @int = x: (B with (B = Ns.E)) => @(x + 1)
			parse X as ParseX
			parse R as ParseR
			""");

		Assert.Contains(result.Diagnostics, one => one.Id == GrammarNormalizer.IncompatibleRebinding);
	}

	[Fact]
	public void A_publications_rebinding_of_a_rule_a_namespace_cloned_replaces_the_clone()
	{
		// `A` is declared outside `Ns`, and `G`'s specialization calls `Ns`'s clone of it: that
		// clone is what the directive's `with` replaces.
		var result = Compile("""
			B = 'b'
			D = 'd'
			Y = 'y'
			A = B & '!'

			namespace Ns with (B = D)
			{
				G = A
			}

			parse Ns.G as Plain
			parse Ns.G with (A = Y) as Replaced
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryPlain", "d!").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryPlain", "b!").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryReplaced", "y").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryReplaced", "d!").IsSuccess);
	}

	[Fact]
	public void A_namespace_header_may_replace_a_rule_with_an_expression()
	{
		// §5.1: what replaces a rule may be any operand. The expression is a rule declared where
		// the header is written, beside the namespace rather than inside the block it introduces.
		var result = Compile("""
			Sp = ' '
			L  = 'a' & Sp & 'b'

			namespace N with (Sp = '_')
			{
				M = L
				parse M as ParseStart
			}
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "a_b").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "a b").IsSuccess);
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

		EmittedCode.Quiet(result.Diagnostics);

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

		EmittedCode.Quiet(result.Diagnostics);

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

		EmittedCode.Quiet(result.Diagnostics);

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

		EmittedCode.Quiet(result.Diagnostics);

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

		EmittedCode.Quiet(result.Diagnostics);

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

		EmittedCode.Quiet(result.Diagnostics);

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
		// exactly like `DefaultPair`. `namespace N with (trivia = none) { ... }` is a rebinding and
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

		EmittedCode.Quiet(result.Diagnostics);

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
	public void With_rebinds_only_the_one_field_it_wraps()
	{
		// A comma is accepted where the third field's decimal point goes — the first two
		// fields still take an ordinary '.' — because `with` scopes the rebinding to `c`
		// alone, not to `Number` everywhere it is called.
		Assert.True(Matches(RowGrammar, "1.2;3.4;5,6"));
	}

	[Fact]
	public void And_the_first_two_fields_are_untouched_by_it()
	{
		// The third field still requires a comma — a '.' there is refused, because it is
		// `c`'s own clone that was rebound, and the clone requires `Comma`.
		Assert.False(Matches(RowGrammar, "1.2;3.4;5.6"));
	}

	[Fact]
	public void And_the_rebinding_does_not_leak_backward_into_the_earlier_fields()
	{
		// The first field is still the plain, unrebound `Number` — a comma there is
		// refused exactly as it would be with no `with` in the grammar at all.
		Assert.False(Matches(RowGrammar, "1,2;3.4;5,6"));
	}

	[Fact]
	public void A_publication_may_carry_its_own_with_header()
	{
		// The question that started this feature: `parse Sum with (trivia = none) as
		// Evaluate` alongside the ordinary, whitespace-tolerant publication of the same
		// rule — one directive, no block, no name for the substitution beyond the
		// publication's own. Mirrors what `namespace N with (trivia = none) { parse ... }`
		// already proved elsewhere, through the publication's own header instead.
		var result = Compile("""
			trivia = ' '*
			Pair   = A & B
			A      = 'a'
			B      = 'b'

			parse Pair as DefaultPair
			parse Pair with (trivia = none) as TightPair
			""");

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryDefaultPair", "a b").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryDefaultPair", "ab").IsSuccess);

		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryTightPair", "a b").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryTightPair", "ab").IsSuccess);
	}
}
