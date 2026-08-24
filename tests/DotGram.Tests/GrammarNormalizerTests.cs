using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
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
		// No trivia declared: nothing is inserted at all, not inserted and skipped.
		Assert.Equal(
			"""
			R = "ab"
			""",
			Normalize("""R = 'a' & 'b'""").ToString());

		Assert.Equal(
			"""
			trivia = ' '*
			R = 'a' & trivia & 'b'
			""",
			Normalize("""
				trivia = ' '*
				R      = 'a' & 'b'
				""").ToString());
	}

	[Fact]
	public void Trivia_switches_per_context_by_shadowing()
	{
		Assert.Equal(
			"""
			trivia = ' '*
			Loose = 'a' & trivia & 'b'
			trivia = none
			Tight = "ab"
			none = none
			""",
			Normalize("""
				trivia = ' '*
				Loose  = 'a' & 'b'

				context Lexical
				{
					trivia = none
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
	[InlineData("trivia = ' '+\nA = 'a' & 'b'", GrammarNormalizer.TriviaNotNullable)]
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

	// ── Contextual bindings — §22, §25 ────────────────────────────────────────────

	static RecognitionGraph Normalize(string source, ISymbolResolver resolver) =>
		GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(source, RoslynCSharpScanner.Instance)).File, resolver),
			resolver);

	[Fact]
	public void An_ordinary_context_body_declaration_stays_lexical_and_clones_nothing()
	{
		// §3, §22 test 1: `B = D` inside a header-less `context` is ordinary shadowing.
		// `F` still resolves through the outer `A` to the outer `B`, and nothing is
		// cloned — proof that a plain `context { ... }` is exactly as before this
		// feature.
		Assert.Equal(
			"""
			B = 'c'
			A = B
			B = 'd'
			E = A
			F = B
			""",
			Normalize("""
				B = 'c'
				A = B

				context Ctx
				{
					B = 'd'
					E = A
					F = B
				}
				""").ToString());
	}

	[Fact]
	public void A_contextual_binding_propagates_through_the_call_graph_and_leaves_the_outside_alone()
	{
		// §4, §22 tests 2 and 4, §25 — the primary example: `F`, outside the context,
		// still resolves `A` to the ordinary `B`; `E`, inside, resolves the same `A`
		// through the rebound `B`.
		Assert.Equal(
			"""
			C = 'c'
			B = C
			A = B
			F = A
			D = 'd'
			E = A
			A_Ctx = D
			E_Ctx = A_Ctx
			""",
			Normalize("""
				C = 'c'
				B = C
				A = B
				F = A

				context Ctx (B = D)
				{
					E = A
				}

				D = 'd'
				""").ToString());
	}

	[Fact]
	public void A_binding_propagates_transitively_with_no_rule_forwarding_anything()
	{
		// §9, §22 test 3: `E` reaches `Y` through `A` and `B`, neither of which mentions
		// `C` or `Y` — the ambient dependency §9 contrasts with threading a parameter
		// through every intermediate rule.
		Assert.Equal(
			"""
			X = 'x'
			C = X
			B = C
			A = B
			Y = 'y'
			E = A
			B_Ctx = Y
			A_Ctx = B_Ctx
			E_Ctx = A_Ctx
			""",
			Normalize("""
				X = 'x'
				C = X
				B = C
				A = B

				context Ctx (C = Y)
				{
					E = A
				}

				Y = 'y'
				""").ToString());
	}

	[Fact]
	public void A_nested_context_may_override_an_inherited_binding()
	{
		// §11, §22 test 6: the inner context's own `B = E` replaces the outer `B = D`
		// for everything declared inside it — `F`'s clone resolves through `E`, not `D`.
		Assert.Equal(
			"""
			B = 'b'
			D = 'd'
			E = 'e'
			A = B
			F = A
			A_Inner = E
			F_Inner = A_Inner
			""",
			Normalize("""
				B = 'b'
				D = 'd'
				E = 'e'
				A = B

				context Outer (B = D)
				{
					context Inner (B = E)
					{
						F = A
					}
				}
				""").ToString());
	}

	[Fact]
	public void A_context_binding_survives_recursion()
	{
		// §10, §22 test 7: the clone's own recursive call closes onto itself, not onto
		// the unbound original — otherwise a nested "(((b)))" would fall back to 'a'
		// past the first level.
		Assert.Equal(
			"""
			Atom = 'a'
			Tree = (Atom | '(' & Tree & ')')
			BAtom = 'b'
			Tree_Ctx = (BAtom | '(' & Tree_Ctx & ')')
			publish Parse Tree_Ctx -> BTree
			""",
			Normalize("""
				Atom = 'a'
				Tree = Atom | '(' & Tree & ')'

				context Ctx (Atom = BAtom)
				{
					parse Tree as BTree
				}

				BAtom = 'b'
				""").ToString());
	}

	// ── `with (...)` — an expression-scoped counterpart to `context (...)` ─────────

	[Fact]
	public void With_clones_only_what_the_binding_can_reach()
	{
		Assert.Equal(
			"""
			Digit = ['0'..'9']
			Point = '.'
			Comma = ','
			Number = Digit & Point & Digit
			ParseEuropeanNumber = Number_With1
			Number_With1 = Digit & Comma & Digit
			""",
			Normalize("""
				Digit = ['0'..'9']
				Point = '.'
				Comma = ','
				Number = Digit & Point & Digit

				ParseEuropeanNumber = Number with (Point = Comma)
				""").ToString());
	}

	[Fact]
	public void With_scoped_to_one_operand_in_a_sequence_leaves_the_others_alone()
	{
		Assert.Equal(
			"""
			Digit = ['0'..'9']
			Point = '.'
			Comma = ','
			Number = Digit & Point & Digit
			Row = a: Number & ',' & b: Number & ',' & c: Number_With1
			Number_With1 = Digit & Comma & Digit
			""",
			Normalize("""
				Digit = ['0'..'9']
				Point = '.'
				Comma = ','
				Number = Digit & Point & Digit

				Row = a: Number & ',' & b: Number & ',' & c: Number with (Point = Comma)
				""").ToString());
	}

	[Fact]
	public void A_with_site_composes_with_an_enclosing_context()
	{
		// The context's own clone of `A` must call a clone of `Number`'s with-clone —
		// not of the plain, unrebound `Number` — which is only possible because `with`
		// runs before `context (...)` is specialized and leaves `A`'s own body mutated.
		Assert.Equal(
			"""
			Digit = ['0'..'9']
			OtherDigit = ['1'..'9']
			Point = '.'
			Comma = ','
			Number = Digit & Point & Digit
			A = Number_With1
			Number_With1 = Digit & Comma & Digit
			Number_With1_Ctx = OtherDigit & Comma & OtherDigit
			A_Ctx = Number_With1_Ctx
			""",
			Normalize("""
				Digit      = ['0'..'9']
				OtherDigit = ['1'..'9']
				Point      = '.'
				Comma      = ','
				Number     = Digit & Point & Digit

				context Ctx (Digit = OtherDigit)
				{
					A = Number with (Point = Comma)
				}
				""").ToString());
	}

	[Fact]
	public void Directly_stacked_with_sites_compose()
	{
		// `Group` is transparent at lowering, so both `with`s' operand lowers to the
		// exact same node — the one case where two sites share a root and have to be
		// merged rather than cloned twice (§20).
		Assert.Equal(
			"""
			Digit = ['0'..'9']
			Point = '.'
			Comma = ','
			Space = ' '
			Number = Digit & Point & Digit
			A = Number_With2
			Number_With2 = Space & Comma & Space
			""",
			Normalize("""
				Digit  = ['0'..'9']
				Point  = '.'
				Comma  = ','
				Space  = ' '
				Number = Digit & Point & Digit

				A = (Number with (Point = Comma)) with (Digit = Space)
				""").ToString());
	}

	[Fact]
	public void An_incompatible_contextual_replacement_is_reported_at_the_binding()
	{
		// §14, §22 test 11: the diagnostic belongs at the binding itself, not at some
		// transitive call site.
		Assert.Contains(
			GrammarNormalizer.IncompatibleContextReplacement,
			Normalize("""
				Value   : @Expr   = 'v'
				RawText : @string = 'r'

				context Ctx (Value = RawText)
				{
				}
				""", new StrictAssignabilityResolver()).Diagnostics.Select(d => d.Id));
	}

	[Fact]
	public void A_compatible_contextual_replacement_is_not_reported()
	{
		Assert.DoesNotContain(
			GrammarNormalizer.IncompatibleContextReplacement,
			Normalize("""
				Value   : @string = 'v'
				RawText : @string = 'r'

				context Ctx (Value = RawText)
				{
				}
				""", new StrictAssignabilityResolver()).Diagnostics.Select(d => d.Id));
	}

	sealed class StrictAssignabilityResolver : ISymbolResolver
	{
		public bool TypeExists(string qualifiedName) => true;

		public bool IsAssignable(string from, string to) => from == to;

		public bool TryResolveConstructors(
			string qualifiedName, out IReadOnlyList<IReadOnlyList<MethodParameter>> constructors)
		{
			constructors = [];

			return false;
		}

		public bool TryResolveSettableProperties(string qualifiedName, out IReadOnlyList<ObjectMember> properties)
		{
			properties = [];

			return false;
		}

		public ExternalValueResolution TryResolveExternalValue(string methodName, string? against, out string? valueType)
		{
			valueType = null;

			return ExternalValueResolution.NotFound;
		}
	}

	// ── External recognizers with a value of their own — §7.1's third row ────────

	/// <summary>
	/// One fixed answer for `@ParseTimestamp`: it has a `(ReadOnlySpan&lt;char&gt;, ref int,
	/// out System.DateTime)` overload, assignable only to itself. Every other name is §7.1's
	/// second row, unchanged.
	/// </summary>
	sealed class TimestampResolver : ISymbolResolver
	{
		public bool TypeExists(string qualifiedName) => true;

		public bool IsAssignable(string from, string to) =>
			string.Equals(from, to, StringComparison.Ordinal);

		public bool TryResolveConstructors(
			string qualifiedName, out IReadOnlyList<IReadOnlyList<MethodParameter>> constructors)
		{
			constructors = [];

			return false;
		}

		public bool TryResolveSettableProperties(string qualifiedName, out IReadOnlyList<ObjectMember> properties)
		{
			properties = [];

			return false;
		}

		public ExternalValueResolution TryResolveExternalValue(string methodName, string? against, out string? valueType)
		{
			valueType = null;

			if (!string.Equals(methodName, "ParseTimestamp", StringComparison.Ordinal))
				return ExternalValueResolution.NotFound;

			if (against is not null && !string.Equals(against, "System.DateTime", StringComparison.Ordinal))
				return ExternalValueResolution.NotFound;

			valueType = "System.DateTime";

			return ExternalValueResolution.Found;
		}
	}

	[Fact]
	public void A_whole_body_value_returning_external_recognizer_is_rewritten_as_a_pass_through()
	{
		var graph = Normalize(
			"Timestamp : @System.DateTime = @ParseTimestamp", new TimestampResolver());

		Assert.Empty(graph.Diagnostics);
		Assert.Equal(
			"""
			Timestamp = item0: @ParseTimestamp => <operand>
			@ParseTimestamp = @ParseTimestamp
			""".Replace("\r\n", "\n"),
			graph.ToString().Replace("\r\n", "\n"));
	}

	[Fact]
	public void A_type_mismatch_falls_through_to_the_ordinary_unbuilt_construction_message()
	{
		// No diagnostic of ProduceFromExternals's own — a resolved T that does not fit the
		// declared type just means the pass-through never applies, and the rule is exactly
		// as unbuilt as any other capture-less, `=>`-less typed rule.
		Assert.Equal(
			[GrammarNormalizer.UnbuiltConstruction],
			Normalize("Elsewhere : @int = @ParseTimestamp", new TimestampResolver())
				.Diagnostics.Select(d => d.Id));
	}

	[Fact]
	public void An_ambiguous_overload_is_reported()
	{
		// The ambiguity falls back to §7.1's second row (LowerReference's `goto default`),
		// so a rule declaring a type still has no way to build it — GRAM4015 names the
		// actual cause, and the generic GRAM4008 that follows from the fallback is not a
		// second, competing diagnostic about the same thing so much as a true statement
		// about the consequence.
		Assert.Contains(
			GrammarNormalizer.AmbiguousExternal,
			Normalize("Value : @int = @Parse", new AmbiguousResolver())
				.Diagnostics.Select(d => d.Id));
	}

	sealed class AmbiguousResolver : ISymbolResolver
	{
		public bool TypeExists(string qualifiedName) => true;

		public bool IsAssignable(string from, string to) => string.Equals(from, to, StringComparison.Ordinal);

		public bool TryResolveConstructors(
			string qualifiedName, out IReadOnlyList<IReadOnlyList<MethodParameter>> constructors)
		{
			constructors = [];

			return false;
		}

		public bool TryResolveSettableProperties(string qualifiedName, out IReadOnlyList<ObjectMember> properties)
		{
			properties = [];

			return false;
		}

		public ExternalValueResolution TryResolveExternalValue(string methodName, string? against, out string? valueType)
		{
			valueType = null;

			return ExternalValueResolution.Ambiguous;
		}
	}
}
