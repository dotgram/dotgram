using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Name binding compared against its own dump: the context tree, what each context
/// imports, and the `trivia` it sees.
/// </summary>
public sealed class GrammarBinderTests
{
	static GrammarModel Bind(string source) =>
		GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(source, RoslynCSharpScanner.Instance)).File);

	static string[] Diagnostics(string source) =>
		[.. Bind(source).Diagnostics.Select(d => d.Id)];

	[Fact]
	public void Builds_the_context_tree()
	{
		Assert.Equal(
			"""
			context <global>
				using @System.Text
				trivia = trivia
				rule Common
				context Lexical
					trivia = trivia
					rule Identifier
				context Syntax
					using Lexical
					trivia = trivia
					rule Unit
			""",
			Bind("""
				@using System.Text;

				Common = 'a'

				context Lexical
				{
					Identifier = 'b'
				}

				context Syntax
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

			context Lexical
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
	public void An_inner_context_sees_outward_but_not_inward()
	{
		Assert.Empty(Diagnostics("""
			Outer = 'a'

			context Inner
			{
				Uses = Outer
			}
			"""));

		Assert.Contains(GrammarBinder.UndefinedName, Diagnostics("""
			context Inner
			{
				Hidden = 'a'
			}

			Uses = Hidden
			"""));
	}

	[Fact]
	public void A_qualified_name_reaches_into_a_context()
	{
		Assert.Empty(Diagnostics("""
			context Lexical
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
	[InlineData("context S { }\nA = Other.X",   GrammarBinder.UndefinedName)]
	[InlineData("using Absent;\nA = 'a'",       GrammarBinder.UnknownContext)]
	[InlineData("parse Absent\nA = 'a'",        GrammarBinder.UndefinedName)]
	[InlineData("context S (Typo = D) { }\nD = 'd'",
		GrammarBinder.UnknownContextTarget)]
	[InlineData("context S (B = Typo) { }\nB = 'b'",
		GrammarBinder.UnknownContextReplacement)]
	[InlineData("context S (B = C, B = D) { }\nB = 'b'\nC = 'c'\nD = 'd'",
		GrammarBinder.DuplicateContextBinding)]
	[InlineData("B(item) = item\nD = 'd'\ncontext S (B = D) { }",
		GrammarBinder.ParameterizedContextBinding)]
	[InlineData("B = 'b'\nD = 'd'\ncontext S (B = D) { B = 'e' }",
		GrammarBinder.ContextBoundNameRedeclared)]
	[InlineData("B = 'b'\nD = 'd'\ncontext S (B = D) { context T { B = 'e' } }",
		GrammarBinder.ContextBoundNameRedeclared)]
	[InlineData("A = 'a'\nB = 'b'\ncontext S (A = B, B = A) { }",
		GrammarBinder.CircularContextBinding)]
	public void Reports(string source, string expectedId)
	{
		Assert.Contains(expectedId, Diagnostics(source));
	}

	[Fact]
	public void Declaring_an_unrelated_name_inside_a_bound_context_is_still_legal()
	{
		// §12's restriction is only for a name with an active *contextual* binding —
		// declaring anything else inside a bound context is ordinary, legal declaration.
		Assert.Empty(Diagnostics("""
			B = 'b'
			D = 'd'

			context S (B = D)
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

			context S (A = B, B = C)
			{
			}
			""");

		var site = model.Root.Nested.Single();
		var a    = model.Root.Rules["A"];
		var c    = model.Root.Rules["C"];

		Assert.Empty(model.Diagnostics);
		Assert.Equal(c, site.ContextBindings[a]);
	}

	[Fact]
	public void A_nested_context_inherits_and_may_override_a_binding()
	{
		var model = Bind("""
			B = 'b'
			D = 'd'
			E = 'e'

			context Outer (B = D)
			{
				context Inner (B = E)
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
		Assert.Equal(d, outer.ContextBindings[b]);
		Assert.Equal(e, inner.ContextBindings[b]);
	}

	[Fact]
	public void A_duplicate_in_a_nested_context_is_shadowing_not_an_error()
	{
		Assert.Empty(Diagnostics("""
			A = 'a'

			context Inner
			{
				A = 'b'
			}
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
