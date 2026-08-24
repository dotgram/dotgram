using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Name binding compared against its own dump: the namespace tree, what each namespace
/// imports, and the `trivia` it sees.
/// </summary>
public sealed class GrammarBinderTests
{
	static GrammarModel Bind(string source) =>
		GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(source, RoslynCSharpScanner.Instance)).File);

	static string[] Diagnostics(string source) =>
		[.. Bind(source).Diagnostics.Select(d => d.Id)];

	[Fact]
	public void Builds_the_namespace_tree()
	{
		Assert.Equal(
			"""
			namespace <global>
				using @System.Text
				trivia = trivia
				rule Common
				namespace Lexical
					trivia = trivia
					rule Identifier
				namespace Syntax
					using Lexical
					trivia = trivia
					rule Unit
			""",
			Bind("""
				@using System.Text;

				Common = 'a'

				namespace Lexical
				{
					Identifier = 'b'
				}

				namespace Syntax
				{
					using Lexical;

					Unit = Identifier*
				}
				""").ToString());
	}

	[Fact]
	public void Trivia_is_an_ordinary_rule_that_shadowing_switches()
	{
		// §4.5: no directive and no mode — the inner trivia simply shadows the outer.
		var model = Bind("""
			trivia = Whitespace

			Whitespace = ' '
			Outer      = 'a'

			namespace Lexical
			{
				trivia = none

				Inner = 'b'
			}
			""");

		var global  = model.Root;
		var lexical = global.Nested.Single();

		Assert.Equal("trivia", model.Trivia[global].Name);
		Assert.False(model.Trivia[global].IsBuiltIn);              // the grammar's own
		Assert.Equal("trivia", model.Trivia[lexical].Name);
		Assert.NotSame(model.Trivia[global], model.Trivia[lexical]);
		Assert.Empty(model.Diagnostics);
	}

	[Fact]
	public void Standard_library_rules_are_visible_and_shadowable()
	{
		Assert.Empty(Diagnostics("A = any & eol & eof & none"));

		// Declaring one of them is not an error — it is the mechanism (§3.1).
		Assert.Empty(Diagnostics("eol = '\\n'\nA = eol"));
	}

	[Fact]
	public void Declaration_order_does_not_matter()
	{
		// Mutual recursion requires it: Factor refers to Expr declared above it, and
		// Expr to Term declared below.
		Assert.Empty(Diagnostics("""
			Expr   = Term & (['+' | '-'] & Term)*
			Term   = Factor & (['*' | '/'] & Factor)*
			Factor = Number | '(' & Expr & ')'
			Number = ['0'..'9']+
			"""));
	}

	[Fact]
	public void An_inner_namespace_sees_outward_but_not_inward()
	{
		Assert.Empty(Diagnostics("""
			Outer = 'a'

			namespace Inner
			{
				Uses = Outer
			}
			"""));

		Assert.Contains(GrammarBinder.UndefinedName, Diagnostics("""
			namespace Inner
			{
				Hidden = 'a'
			}

			Uses = Hidden
			"""));
	}

	[Fact]
	public void A_qualified_name_reaches_into_a_namespace()
	{
		Assert.Empty(Diagnostics("""
			namespace Lexical
			{
				Token = 'a'
			}

			Uses = Lexical.Token
			"""));
	}

	[Fact]
	public void Parameters_are_in_scope_inside_their_rule_only()
	{
		Assert.Empty(Diagnostics("List(item, sep) : item[] = item & (sep & item)*"));

		Assert.Contains(GrammarBinder.UndefinedName, Diagnostics("""
			List(item) = item
			Other      = item
			"""));
	}

	[Theory]
	[InlineData("A = 'a'\nA = 'b'",             GrammarBinder.DuplicateRule)]
	[InlineData("A = Missing",                  GrammarBinder.UndefinedName)]
	[InlineData("namespace S { }\nA = Other.X",   GrammarBinder.UndefinedName)]
	[InlineData("using Absent;\nA = 'a'",       GrammarBinder.UnknownNamespace)]
	[InlineData("parse Absent\nA = 'a'",        GrammarBinder.UndefinedName)]
	[InlineData("namespace S (Typo = D) { }\nD = 'd'",
		GrammarBinder.UnknownRebindingTarget)]
	[InlineData("namespace S (B = Typo) { }\nB = 'b'",
		GrammarBinder.UnknownRebindingReplacement)]
	[InlineData("namespace S (B = C, B = D) { }\nB = 'b'\nC = 'c'\nD = 'd'",
		GrammarBinder.DuplicateRebinding)]
	[InlineData("B(item) = item\nD = 'd'\nnamespace S (B = D) { }",
		GrammarBinder.ParameterizedRebinding)]
	[InlineData("B = 'b'\nD = 'd'\nnamespace S (B = D) { B = 'e' }",
		GrammarBinder.NamespaceBoundNameRedeclared)]
	[InlineData("B = 'b'\nD = 'd'\nnamespace S (B = D) { namespace T { B = 'e' } }",
		GrammarBinder.NamespaceBoundNameRedeclared)]
	[InlineData("A = 'a'\nB = 'b'\nnamespace S (A = B, B = A) { }",
		GrammarBinder.CircularRebinding)]
	public void Reports(string source, string expectedId)
	{
		Assert.Contains(expectedId, Diagnostics(source));
	}

	[Fact]
	public void Declaring_an_unrelated_name_inside_a_bound_namespace_is_still_legal()
	{
		// §12's restriction is only for a name with an active binding —
		// declaring anything else inside a bound namespace is ordinary, legal declaration.
		Assert.Empty(Diagnostics("""
			B = 'b'
			D = 'd'

			namespace S (B = D)
			{
				E = 'e'
			}
			"""));
	}

	[Fact]
	public void Bindings_in_one_header_chain_regardless_of_written_order()
	{
		// §7/§8: `(A = B, B = C)` resolves `A` straight through to `C` — one hop, not a
		// repeated lookup wherever the binding is used.
		var model = Bind("""
			A = 'a'
			B = 'b'
			C = 'c'

			namespace S (A = B, B = C)
			{
			}
			""");

		var site = model.Root.Nested.Single();
		var a    = model.Root.Rules["A"];
		var c    = model.Root.Rules["C"];

		Assert.Empty(model.Diagnostics);
		Assert.Equal(c, site.Rebindings[a]);
	}

	[Fact]
	public void A_nested_namespace_inherits_and_may_override_a_binding()
	{
		var model = Bind("""
			B = 'b'
			D = 'd'
			E = 'e'

			namespace Outer (B = D)
			{
				namespace Inner (B = E)
				{
				}
			}
			""");

		var outer = model.Root.Nested.Single();
		var inner = outer.Nested.Single();
		var b     = model.Root.Rules["B"];
		var d     = model.Root.Rules["D"];
		var e     = model.Root.Rules["E"];

		Assert.Empty(model.Diagnostics);
		Assert.Equal(d, outer.Rebindings[b]);
		Assert.Equal(e, inner.Rebindings[b]);
	}

	[Fact]
	public void A_duplicate_in_a_nested_namespace_is_shadowing_not_an_error()
	{
		// Still legal — still not GrammarBinder.DuplicateRule — but worth a note now:
		// shadowing an enclosing rule from inside a nested namespace is one parenthesis away
		// from a namespace binding that would have meant something else (§5.1).
		Assert.Equal(
			[GrammarBinder.ShadowsEnclosingRule],
			Diagnostics("""
				A = 'a'

				namespace Inner
				{
					A = 'b'
				}
				"""));
	}

	[Fact]
	public void The_warning_fires_regardless_of_whether_the_namespace_already_has_a_header()
	{
		// The risk is "was a header entry meant here", not "does this specific block
		// already use one" — a header for an unrelated name does not change that.
		Assert.Equal(
			[GrammarBinder.ShadowsEnclosingRule],
			Diagnostics("""
				A = 'a'
				C = 'c'

				namespace Inner (C = A)
				{
					A = 'b'
				}
				"""));
	}

	[Fact]
	public void The_warning_reaches_two_levels_deep_the_same_way()
	{
		Assert.Equal(
			[GrammarBinder.ShadowsEnclosingRule],
			Diagnostics("""
				A = 'a'

				namespace Outer
				{
					namespace Inner
					{
						A = 'b'
					}
				}
				"""));
	}

	[Fact]
	public void A_name_shadowed_only_through_an_import_is_a_known_first_cut_gap()
	{
		// Declare (where the check runs) is pass one; ResolveImports runs after it, so an
		// imported name is not yet visible on Lookup's import branch at the point this asks.
		// Reaching it would mean moving the check to pass two — accepted as under-reporting
		// rather than done, the same shape as this project's other documented diagnostic
		// narrowings: it never mis-attributes, it simply has less to say than the full
		// mechanism eventually could.
		Assert.Empty(Diagnostics("""
			namespace Lib
			{
				A = 'a'
			}

			namespace Inner
			{
				using Lib;

				A = 'b'
			}
			"""));
	}

	[Fact]
	public void Re_shadowing_the_standard_library_a_second_time_stays_silent()
	{
		// `trivia` was already shadowed once at the top level; shadowing it again inside a
		// nested namespace is the same always-legal mechanism, not a new grammar rule.
		Assert.Empty(Diagnostics("""
			trivia = none

			namespace Inner
			{
				trivia = [' ']
			}
			"""));
	}

	[Theory]
	[InlineData("A = Number with (Typo = D)\nNumber = 'n'\nD = 'd'",
		GrammarBinder.UnknownRebindingTarget)]
	[InlineData("A = Number with (Number = Typo)\nNumber = 'n'",
		GrammarBinder.UnknownRebindingReplacement)]
	[InlineData("A = Number with (B = D)\nB(item) = item\nD = 'd'\nNumber = 'n'",
		GrammarBinder.ParameterizedRebinding)]
	[InlineData("A = Number with (B = C, B = D)\nNumber = 'n'\nB = 'b'\nC = 'c'\nD = 'd'",
		GrammarBinder.DuplicateRebinding)]
	public void With_reuses_the_namespace_header_s_own_rebinding_diagnostics(string source, string expectedId)
	{
		Assert.Contains(expectedId, Diagnostics(source));
	}

	[Fact]
	public void With_never_reports_a_bound_name_redeclared()
	{
		// `with` declares nothing of its own — `NamespaceBoundNameRedeclared` is a check
		// against a namespace *block*'s own declarations and has nothing to port here.
		Assert.DoesNotContain(
			GrammarBinder.NamespaceBoundNameRedeclared,
			Diagnostics("""
				Number = 'n'
				Point  = '.'
				Comma  = ','

				A = Number with (Point = Comma)
				"""));
	}

	[Theory]
	[InlineData("parse Number with (Typo = D) as X\nNumber = 'n'\nD = 'd'",
		GrammarBinder.UnknownRebindingTarget)]
	[InlineData("parse Number with (Number = Typo) as X\nNumber = 'n'",
		GrammarBinder.UnknownRebindingReplacement)]
	[InlineData("parse Number with (B = C, B = D) as X\nNumber = 'n'\nB = 'b'\nC = 'c'\nD = 'd'",
		GrammarBinder.DuplicateRebinding)]
	public void A_publication_s_own_with_reuses_the_same_rebinding_diagnostics(string source, string expectedId)
	{
		Assert.Contains(expectedId, Diagnostics(source));
	}

	[Fact]
	public void A_publication_s_with_does_not_stop_it_being_reported_as_undefined()
	{
		Assert.Contains(
			GrammarBinder.UndefinedName,
			Diagnostics("parse Missing with (A = B) as X\nA = 'a'\nB = 'b'"));
	}

	[Fact]
	public void With_resolves_its_operand_under_the_same_namespace_it_sits_in()
	{
		Assert.Empty(Diagnostics("""
			Number = 'n'
			Point  = '.'
			Comma  = ','

			A = Number with (Point = Comma)
			"""));
	}

	[Fact]
	public void C_sharp_methods_do_not_go_through_the_resolver()
	{
		var strict = new StrictResolver();
		var model  = GrammarBinder.Bind(
			GramParser.Parse(GramLexer.Tokenize("A = x: [@IsLetter] & @Read & when @IsSmall(x)")).File,
			strict);

		Assert.Empty(model.Diagnostics);
		Assert.Empty(strict.Asked);
	}

	sealed class StrictResolver : ISymbolResolver
	{
		public System.Collections.Generic.List<string> Asked { get; } = [];

		public bool TypeExists(string qualifiedName)
		{
			Asked.Add(qualifiedName);
			return false;
		}

		public bool IsAssignable(string from, string to) => false;

		public bool TryResolveSettableProperties(
			string qualifiedName, out System.Collections.Generic.IReadOnlyList<ObjectMember> properties)
		{
			properties = [];

			return false;
		}

		public bool TryResolveConstructors(
			string qualifiedName,
			out System.Collections.Generic.IReadOnlyList<System.Collections.Generic.IReadOnlyList<MethodParameter>> constructors)
		{
			constructors = [];

			return false;
		}

		public ExternalValueResolution TryResolveExternalValue(string methodName, string? against, out string? valueType)
		{
			valueType = null;

			return ExternalValueResolution.NotFound;
		}
	}
}
