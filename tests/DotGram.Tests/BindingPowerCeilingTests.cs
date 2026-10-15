using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;

using Microsoft.CodeAnalysis;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A climbing rule's loop takes only what the operand before it could not have taken (§4.3.1).
/// </summary>
/// <remarks>
/// <para>
/// A rule written with binding powers is a base and a loop of tails, each tail entered only at
/// the strength it allows. Nothing more than that stopped a turn from taking an operator the
/// operand before it gave back: <c>1/0</c> read as an operand at strength 3 could give the
/// <c>/0</c> back and the loop take it, the same text grouped the other way. Without a guard both
/// groupings are accepted or both refused, so the only visible cost was time — two readings per
/// operator, two to the power of their count on a refused text. With a guard in a tail, which may
/// refuse in the operand's loop what it accepts in the enclosing one, the second grouping was an
/// answer: <c>1+2*3</c> accepted as <c>(1+2)*3</c>.
/// </para>
/// <para>
/// The ceiling is said by what was read: after a call of the rule read at k with nothing read
/// after it, the next turn takes only operators weaker than k; after anything else, any.
/// Each grammar is read by the engine and by the reader on both carriers.
/// </para>
/// </remarks>
public sealed class BindingPowerCeilingTests
{
	/// <summary>
	/// The tail guard of the decision: <c>*</c> only after an operand longer than one character.
	/// </summary>
	const string GuardedTail = """
		Start : @string = e: E => @(e)
		E : @string = l: E & '+' & r: E << 1                         => @("(" + l + "+" + r + ")")
		            | l: E & '*' & when @(l.Length > 1) & r: E << 2  => @("(" + l + "*" + r + ")")
		            | '(' & i: E & ')'                               => @(i)
		            | d: ['0'..'9']                                  => @(d)
		""";

	[Theory]
	[InlineData("(1+2)*3", "((1+2)*3)")]
	[InlineData("1+(2+3)*4", "(1+((2+3)*4))")]
	[InlineData("1+2", "(1+2)")]
	[InlineData("1+2*3", null)]
	[InlineData("1+2+3*4", null)]
	public void A_guard_in_a_tail_no_longer_accepts_the_grouping_its_strengths_exclude(string input, string? expected)
	{
		// `*` is stronger than `+`, so `1+2*3` can only be `1+(2*3)`, and the guard refuses
		// that. Before, the operand `2` gave the `*3` back and the loop took it with `(1+2)` as
		// its left side: accepted as `(1+2)*3`.
		AllRead(GuardedTail, input, expected);
	}

	/// <summary>
	/// Over tokens nothing gives back, and the guard still decides: the operand's loop refuses
	/// the step, and the enclosing loop used to take it.
	/// </summary>
	[Theory]
	[InlineData("(1 + 2) * 3", "((1+2)*3)")]
	[InlineData("1 + 2", "(1+2)")]
	[InlineData("1 + 2 * 3", null)]
	public void Over_tokens_a_guard_in_a_tail_decides_the_same(string input, string? expected)
	{
		const string grammar = """
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Digit = ['0'..'9']
			}
			Start : @string = e: E => @(e)
			E : @string = l: E & '+' & r: E << 1                         => @("(" + l + "+" + r + ")")
			            | l: E & '*' & when @(l.Length > 1) & r: E << 2  => @("(" + l + "*" + r + ")")
			            | '(' & i: E & ')'                               => @(i)
			            | d: Lexical.Digit                               => @(d)
			parse Start
			""";

		foreach (var direct in new[] { false, true })
		{
			var result = Compile(grammar, new GramCompilerOptions { Lexical = true, Direct = direct });

			EmittedCode.Quiet(result.Diagnostics);
			Assert.DoesNotContain(result.Diagnostics, static one => one.Id == GramCompiler.NotCut);

			var (isSuccess, value, _, _) = EmittedCode.Match(
				EmittedCode.Compile(result.Sources[0].Text), "Grammar", "TryParseStart", input);

			Assert.True(expected is not null == isSuccess, $"{input}: {(direct ? "the reader" : "the engine")} answered {(isSuccess ? value : "a refusal")}");

			if (expected is not null)
				Assert.Equal(expected, (string?)value);
		}
	}

	/// <summary>The example of docs/syntax.md §4.3.1, as written there.</summary>
	[Theory]
	[InlineData("12*3", "12*3")]
	[InlineData("12*3+4", "12*3+4")]
	[InlineData("1+2*3", null)]
	public void The_example_of_the_specification_reads_as_it_says(string input, string? expected)
	{
		AllRead(
			"""
			Start : @string = e: E => @(e)
			E : @string = left: E & '+' & right: E                             << 1  => @(left + "+" + right)
			            | left: E & '*' & when @(left.Length > 1) & right: E   << 2  => @(left + "*" + right)
			            | digits: ['0'..'9']+                                        => @(digits)
			""",
			input,
			expected);
	}

	/// <summary>
	/// A guard after the operand: the strength still reaches the operand, and the ceiling is
	/// still the strength it was read at.
	/// </summary>
	[Theory]
	[InlineData("1-1-1", "((1-1)-1)")]
	[InlineData("1-1+1", "((1-1)+1)")]
	[InlineData("1-0+1", null)]
	[InlineData("1-0-1", null)]
	public void A_guard_after_the_operand_does_not_hide_it_from_its_strength(string input, string? expected)
	{
		// The `<< 1` used to bind nothing — the alternative ended with the guard, not the
		// operand — so `r` was read at 0 and `1-1-1` grouped right, `1-(1-1)`. It is read at 2
		// now, as without the guard, and the zero the guard refuses cannot be dodged by
		// reading `0+1` as the right side of the `-`.
		AllRead(
			"""
			Start : @string = e: E => @(e)
			E : @string = l: E & '-' & r: E & when @(r != "0") << 1 => @("(" + l + "-" + r + ")")
			            | l: E & '+' & r: E << 1                    => @("(" + l + "+" + r + ")")
			            | d: ['0'..'9']                             => @(d)
			""",
			input,
			expected);
	}

	/// <summary>
	/// A ternary whose else is optional: its last operand, the middle or the else, is read at
	/// 0 either way, and nothing it gave back is for the loop.
	/// </summary>
	[Theory]
	[InlineData("1?11+1", "(1?(11+1):_)")]
	[InlineData("11+1?1", "((11+1)?1:_)")]
	[InlineData("1?1+1", null)]
	[InlineData("1?1:1+1", null)]
	public void A_ternary_with_an_optional_else_leaves_the_loop_nothing(string input, string? expected)
	{
		// `+` wants a left side of two characters or more. In `1?1+1` the middle cannot take
		// the `+1`, and the loop used to take it instead, after the whole ternary: accepted as
		// `(1?1:_)+1`, a grouping of the middle that it was written not to have.
		AllRead(
			"""
			Start : @string = e: E => @(e)
			E : @string = l: E & '?' & m: E & (':' & r: E)? >> 1      => @("(" + l + "?" + m + ":" + (r ?? "_") + ")")
			            | l: E & '+' & when @(l.Length > 1) & r: E << 2 => @("(" + l + "+" + r + ")")
			            | d: ['0'..'9']+                                => @(d)
			""",
			input,
			expected,
			GrammarNormalizer.UnreachedStrength);
	}

	/// <summary>
	/// A prefix written without a strength reads its operand at 0, which took every operator
	/// it could.
	/// </summary>
	[Theory]
	[InlineData("~11+1", "(~(11+1))")]
	[InlineData("~1+1", null)]
	public void A_prefix_without_a_strength_leaves_the_loop_nothing(string input, string? expected)
	{
		AllRead(
			"""
			Start : @string = e: E => @(e)
			E : @string = l: E & '+' & when @(l.Length > 1) & r: E << 2 => @("(" + l + "+" + r + ")")
			            | '~' & o: E                                     => @("(~" + o + ")")
			            | d: ['0'..'9']+                                 => @(d)
			""",
			input,
			expected);
	}

	/// <summary>
	/// An alternative that may end with its operand or with something after it leaves the loop
	/// what it left before there was a ceiling: which of the two was read last is not kept.
	/// </summary>
	[Theory]
	[InlineData("~11*2", "(~(11*2))")]
	[InlineData("~1!*2", "((~1)*2)")]
	[InlineData("~1*2", "((~1)*2)")]
	[InlineData("3[1+1]*2", "((3[(1+1))*2)")]
	[InlineData("3[11*2", "(3[(11*2))")]
	[InlineData("3[1*2", "((3[1)*2)")]
	public void An_alternative_that_may_end_either_way_leaves_the_loop_unlimited(string input, string? expected)
	{
		AllRead(
			"""
			Start : @string = e: E => @(e)
			E : @string = l: E & '*' & when @(l.Length > 1) & r: E << 2   => @("(" + l + "*" + r + ")")
			            | l: E & '+' & r: E << 1                          => @("(" + l + "+" + r + ")")
			            | l: E & '[' & i: E & ']'? << 9                   => @("(" + l + "[" + i + ")")
			            | '~' & o: E & '!'?                               => @("(~" + o + ")")
			            | d: ['0'..'9']+                                  => @(d)
			""",
			input,
			expected,
			GrammarNormalizer.UnreachedStrength);
	}

	[Fact]
	public void A_binding_power_is_at_most_9999()
	{
		var result = Compile(
			"""
			Start = E
			E = E & '+' & E << 10000
			  | ['0'..'9']
			parse Start
			""",
			new GramCompilerOptions());

		Assert.Contains(result.Diagnostics, static one => one.Id == DotGram.Grammar.Parsing.GramParser.InvalidCount);
	}

	/// <summary>
	/// A caller's guard that reads the climbed value: the only reading left at each end is
	/// the one the strengths say.
	/// </summary>
	[Theory]
	[InlineData("1/0", "(1/0)")]
	[InlineData("1+1/0", null)]
	[InlineData("1/0+1/0", null)]
	public void A_guarded_caller_is_handed_only_the_grouping_the_strengths_say(string input, string? expected)
	{
		AllRead(
			"""
			Start : @string = e: E & when @(e.EndsWith("/0)")) => @(e)
			E : @string = l: E & '+' & r: E << 1 => @("(" + l + "+" + r + ")")
			            | l: E & '/' & r: E << 2 => @("(" + l + "/" + r + ")")
			            | d: ['0'..'9']          => @(d)
			""",
			input,
			expected);
	}

	[Theory]
	[InlineData("1+2*3^2^2", "(1+(2*(3^(2^2))))")]
	[InlineData("-2^2", "(-(2^2))")]
	[InlineData("1-2+3", "((1-2)+3)")]
	[InlineData("2*3!+1", "((2*(3!))+1)")]
	[InlineData("2!^2", "((2!)^2)")]
	public void What_a_grammar_without_guards_reads_is_unchanged(string input, string expected)
	{
		AllRead(
			"""
			Start : @string = e: E => @(e)
			E : @string = l: E & '+' & r: E << 1 => @("(" + l + "+" + r + ")")
			            | l: E & '-' & r: E << 1 => @("(" + l + "-" + r + ")")
			            | l: E & '*' & r: E << 2 => @("(" + l + "*" + r + ")")
			            | l: E & '^' & r: E >> 3 => @("(" + l + "^" + r + ")")
			            | l: E & '!' << 5        => @("(" + l + "!)")
			            | '-' & o: E >> 3        => @("(-" + o + ")")
			            | d: ['0'..'9']          => @(d)
			""",
			input,
			expected);
	}

	[Fact]
	public void A_strength_that_cannot_reach_its_operand_is_told()
	{
		// The else is optional, so the operand the alternative may end with is inside it and the
		// marker cannot bind it: it says where the alternative is entered and nothing more.
		var result = Compile(
			"""
			Start = E
			E = E & '?' & E & (':' & E)? >> 1
			  | ['0'..'9']
			parse Start
			""",
			new GramCompilerOptions());

		var told = Assert.Single(result.Diagnostics, static one => one.Id == GrammarNormalizer.UnreachedStrength);

		Assert.Equal(GramSeverity.Warning, told.Severity);
	}

	[Fact]
	public void A_strength_before_a_trailing_guard_is_not_told()
	{
		var result = Compile(
			"""
			Start : @int = e: E => @(e)
			E : @int = l: E & '-' & r: E & when @(r != 0) << 1 => @(l - r)
			         | d: ['0'..'9']                           => @(int.Parse(d))
			parse Start
			""",
			new GramCompilerOptions());

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity != GramSeverity.Info);
	}

	/// <summary>
	/// Written only where it can refuse something: over characters, or over kinds where a
	/// tail has a guard. Asked of the compilation built, not of the one requested.
	/// </summary>
	[Theory]
	[InlineData(false, false, true)]
	[InlineData(true,  false, false)]
	[InlineData(true,  true,  true)]
	public void The_ceiling_is_written_where_it_can_refuse(bool lexical, bool guarded, bool written)
	{
		var grammar =
			"trivia = { ' '* }\n" +
			"namespace Lexical\n" +
			"{\n" +
			"\ttrivia = none\n" +
			"\tName = ['a'..'z']+\n" +
			"}\n" +
			"Start = Expr & eof\n" +
			"Expr = Expr & '+' & Expr << 1\n" +
			(guarded
				? "     | Expr & '*' & when @(true) & Expr << 2\n"
				: "     | Expr & '*' & Expr << 2\n") +
			"     | Lexical.Name\n" +
			"parse Start";

		var result = Compile(grammar, new GramCompilerOptions { Lexical = lexical });

		Assert.DoesNotContain(result.Diagnostics, static one => one.Id == GramCompiler.NotCut);
		Assert.Equal(written, Ceiled(result.Sources[0].Text));
	}

	[Fact]
	public void A_lexical_request_read_over_characters_is_given_the_ceiling()
	{
		// No trivia, so nothing tells a token from a character: the request falls back
		// (GRAM5004), and the parser built reads characters, where every rule gives back.
		var result = Compile(
			"""
			Start = E
			E = E & '+' & E << 1
			  | ['0'..'9']
			parse Start
			""",
			new GramCompilerOptions { Lexical = true });

		Assert.Contains(result.Diagnostics, static one => one.Id == GramCompiler.NotCut);
		Assert.True(Ceiled(result.Sources[0].Text));
	}

	/// <summary>
	/// A lexical grammar included into one read over characters, and read again over
	/// characters beside its own reading: each compilation decides for itself.
	/// </summary>
	[Fact]
	public void An_included_or_optioned_reading_over_characters_is_given_the_ceiling()
	{
		var run = GeneratorDriverTests.RunGenerator(
			""""
			using DotGram;

			[Gram("""
				trivia = { ' '* }
				namespace Lexical
				{
					trivia = none
					Name = ['a'..'z']+
				}
				Expr = Expr & '+' & Expr << 1
				     | Lexical.Name
				parse Expr
				""", Lexical = true)]
			[GramOptions(Suffix = "Characters", Lexical = false)]
			public static partial class Sums;

			[GramInclude(typeof(Sums), As = "S")]
			[Gram("""
				Start = S.Expr & ';'
				parse Start
				""")]
			public static partial class Statements;
			"""");

		Assert.Empty(run.Diagnostics.Where(static one => one.Severity == DiagnosticSeverity.Error));

		var sources = run.Results
			.SelectMany(static one => one.GeneratedSources)
			.Where(static one => !one.HintName.Contains("DotGramReport", StringComparison.Ordinal))
			.ToDictionary(static one => one.HintName, static one => one.SourceText.ToString());

		Assert.False(Ceiled(Single(sources, "Sums.g.cs")));
		Assert.True(Ceiled(Single(sources, "Sums.Characters.g.cs")));
		Assert.True(Ceiled(Single(sources, "Statements.g.cs")));
	}

	/// <summary>
	/// Whether a generated file carries a ceiling: the reader's two locals, or the engine's high
	/// half of the strength.
	/// </summary>
	static bool Ceiled(string source)
	{
		return source.Contains("ceiled", StringComparison.Ordinal) ||
			source.Contains("power & 0xFFFF", StringComparison.Ordinal);
	}

	static string Single(System.Collections.Generic.Dictionary<string, string> sources, string hint)
	{
		return Assert.Single(sources, one => one.Key.EndsWith(hint, StringComparison.Ordinal)).Value;
	}

	/// <summary>
	/// The grammar read by the engine and by the reader on each carrier, every one of them
	/// expected to answer <paramref name="expected"/>: the value built, or a refusal (null).
	/// </summary>
	static void AllRead(string grammar, string input, string? expected, string? told = null)
	{
		foreach (var (direct, carrier) in new (bool, CarrierKind?)[]
		{
			(false, null),
			(true,  CarrierKind.Tape),
			(true,  CarrierKind.Immediate),
		})
		{
			var options = new GramCompilerOptions { Direct = direct };

			if (carrier is { } kind)
				options.Carrier = kind;

			var result = Compile(grammar + "\nparse Start", options);

			EmittedCode.Quiet(result.Diagnostics.Where(one => one.Id != told));

			var (isSuccess, value, _, _) = EmittedCode.Match(
				EmittedCode.Compile(result.Sources[0].Text), "Grammar", "TryParseStart", input);
			var reading = direct ? $"the reader on {carrier}" : "the engine";

			Assert.True(expected is not null == isSuccess, $"{input}: {reading} answered {(isSuccess ? value : "a refusal")}");

			if (expected is not null)
				Assert.True(expected == (string?)value, $"{input}: {reading} built {value}, not {expected}");
		}
	}

	static GramCompilation Compile(string grammar, GramCompilerOptions options)
	{
		options.ClassName     = "Grammar";
		options.CSharpScanner = RoslynCSharpScanner.Instance;

		return GramCompiler.Compile(grammar, options);
	}
}
