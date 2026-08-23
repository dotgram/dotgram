using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Normalization stated as before and after, the way Roc's macro tested itself: the
/// grammar as written on one side, as folded on the other.
/// </summary>
public sealed class GrammarNormalizerTests
{
	static RecognitionGraph Normalize(string source) =>
		GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(source, RoslynCSharpScanner.Instance)).File));

	static string[] Diagnostics(string source) =>
		[.. Normalize(source).Diagnostics.Select(d => d.Id)];

	[Fact]
	public void Merges_adjacent_literals()
	{
		Assert.Equal(
			"""
			R = "abc"
			""",
			Normalize("R = 'a' & 'b' & 'c'").ToString());
	}

	[Fact]
	public void Preserves_an_atomic_boundary_while_normalizing_its_body()
	{
		Assert.Equal(
			"""
			R = { "ab" } & 'c'{2}
			""",
			Normalize("R = { 'a' & 'b' } & 'c'{2}").ToString());
	}

	[Fact]
	public void Folds_character_alternatives_into_ranges()
	{
		Assert.Equal(
			"""
			R1 = ['a'..'b']
			R2 = ['a'..'c']
			R3 = ['a'..'d']
			""",
			Normalize("""
				R1 = 'a' | 'b'
				R2 = ['a'..'c'] | 'b'
				R3 = ['a'..'b'] | ['c'..'d']
				""").ToString());
	}

	[Fact]
	public void Never_moves_an_alternative_past_another()
	{
		// Roc's macro hoisted every single character ahead of the rest, which turns
		// this into ('a' | "ab") and makes the string unreachable. Only an adjacent
		// run may merge.
		Assert.Equal(
			"""
			R = ("ab" | 'a')
			""",
			Normalize("""R = "ab" | 'a'""").ToString());
	}

	[Fact]
	public void Merges_only_adjacent_runs()
	{
		Assert.Equal(
			"""
			R = (['a'..'b'] | "xy" | ['c'..'d'])
			""",
			Normalize("""R = 'a' | 'b' | "xy" | 'c' | 'd'""").ToString());
	}

	[Fact]
	public void Inserts_trivia_only_where_it_is_not_empty()
	{
		// No Trivia declared: nothing is inserted at all, not inserted and skipped.
		Assert.Equal(
			"""
			R = "ab"
			""",
			Normalize("""R = 'a' & 'b'""").ToString());

		Assert.Equal(
			"""
			Trivia = ' '*
			R = 'a' & Trivia & 'b'
			""",
			Normalize("""
				Trivia = ' '*
				R      = 'a' & 'b'
				""").ToString());
	}

	[Fact]
	public void Trivia_switches_per_context_by_shadowing()
	{
		Assert.Equal(
			"""
			Trivia = ' '*
			Loose = 'a' & Trivia & 'b'
			Trivia = none
			Tight = "ab"
			none = none
			""",
			Normalize("""
				Trivia = ' '*
				Loose  = 'a' & 'b'

				context Lexical
				{
					Trivia = none
					Tight  = 'a' & 'b'
				}
				""").ToString());
	}

	[Fact]
	public void Keeps_rule_boundaries()
	{
		// Roc inlined rule references. We do not: diagnostics are phrased in terms of
		// rules, and an inlined rule has no name left to name.
		Assert.Equal(
			"""
			A = B & B
			B = 'x'
			""",
			Normalize("""
				A = B & B
				B = 'x'
				""").ToString());
	}

	[Theory]
	[InlineData("A = ('x'?)*",                GrammarNormalizer.NullableRepetition)]
	[InlineData("A = A & 'x'",                GrammarNormalizer.LeftRecursion)]
	[InlineData("A = B & 'x'\nB = A",         GrammarNormalizer.LeftRecursion)]
	[InlineData("Trivia = ' '+\nA = 'a' & 'b'", GrammarNormalizer.TriviaNotNullable)]
	public void Reports(string source, string expectedId)
	{
		Assert.Contains(expectedId, Diagnostics(source));
	}

	[Fact]
	public void A_nullable_prefix_makes_recursion_left_recursion()
	{
		// The check is not syntactic: whether A = B & A recurses on the left depends
		// entirely on whether B can match nothing.
		Assert.Contains(GrammarNormalizer.LeftRecursion, Diagnostics("""
			A = B & A
			B = 'x'?
			"""));

		Assert.DoesNotContain(GrammarNormalizer.LeftRecursion, Diagnostics("""
			A = B & A
			B = 'x'
			"""));
	}

	[Fact]
	public void The_reordered_form_is_accepted()
	{
		Assert.Empty(Diagnostics("""R = "https" | "http" """));
	}

	[Fact]
	public void Binding_powers_become_the_same_loop_the_levels_do()
	{
		// §4.3.1 written out and folded: the alternatives that begin with a call to the
		// rule are the tails, the rest are the bases, and the numbers ride beside them.
		Assert.Equal(
			"E = ('-' & operand: E => (-operand) | digits: ['0'..'9']+ => int.Parse(digits))" +
			" & ('+' & right: E => (left + right)" +
			" | '*' & right: E => (left * right)" +
			" | '^' & right: E => (left - right))*",
			Normalize("""
				E : @int = left: E & '+' & right: E << 1 => @(left + right)
				         | left: E & '*' & right: E << 2 => @(left * right)
				         | left: E & '^' & right: E >> 3 => @(left - right)
				         | '-' & operand: E         >> 4 => @(-operand)
				         | digits: ['0'..'9']+           => @int.Parse(digits)
				""").ToString().Split('\n')[0].TrimEnd());
	}

	[Fact]
	public void Only_a_leading_self_call_is_rewritten()
	{
		// How left is told from right: it is not. The one question asked is whether an
		// alternative begins with a call to its own rule, because that one cannot be
		// compiled as written. A self-call anywhere else is an ordinary call, left where
		// the author put it, and nothing anywhere records that the rule was recursive.
		Assert.Equal(
			"R = 'x' & ('+' & R2)*",
			Normalize("R = R & '+' & R2 | 'x'\nR2 = 'x'").ToString().Split('\n')[0].TrimEnd());

		Assert.Equal(
			"R = (R2 & '+' & R | 'x')",
			Normalize("R = R2 & '+' & R | 'x'\nR2 = 'x'").ToString().Split('\n')[0].TrimEnd());
	}

	/// <summary>
	/// What a <c>=&gt;</c> or a <c>when</c> carries is C# to be pasted into the
	/// generated file, so it is rendered rather than described.
	/// </summary>
	[Fact]
	public void C_sharp_values_come_through_as_C_sharp() =>
		Assert.Equal(
			"""
			N = ['0'..'9']+ => int.Parse(text, CultureInfo.InvariantCulture)
			P = ['a'..'z']+ & when IsKnown(text, "prefix")
			Q = ['a'..'z']+ => Make<Row, int>(text)
			R = ['a'..'z']+ => (text.Length * 2)
			""",
			Normalize("""
				N : @int = ['0'..'9']+ => @int.Parse(text, CultureInfo.InvariantCulture)
				P        = ['a'..'z']+ & when @IsKnown(text, "prefix")
				Q : @Row = ['a'..'z']+ => @Make<@Row, @int>(text)
				R : @int = ['a'..'z']+ => @(text.Length * 2)
				""").ToString());

	[Fact]
	public void A_correct_grammar_normalizes_without_complaint()
	{
		var graph = Normalize("""
			Expr   = Term & (['+' | '-'] & Term)*
			Term   = Factor & (['*' | '/'] & Factor)*
			Factor = Number | '(' & Expr & ')'
			Number = ['0'..'9']+
			""");

		Assert.Empty(graph.Diagnostics);
		Assert.False(graph.Nullable[graph.Rules.Single(r => r.Name == "Number")]);
	}
}
