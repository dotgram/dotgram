using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Name binding compared against its own dump: the scope tree, what each scope
/// imports, and the `Trivia` it sees.
/// </summary>
public sealed class GrammarBinderTests
{
	static GrammarModel Bind(string source) =>
		GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(source, RoslynCSharpScanner.Instance)).File);

	static string[] Diagnostics(string source) =>
		[.. Bind(source).Diagnostics.Select(d => d.Id)];

	[Fact]
	public void Builds_the_scope_tree()
	{
		Assert.Equal(
			"""
			scope <global>
				using @System.Text
				trivia = Trivia
				rule Common
				scope Lexical
					trivia = Trivia
					rule Identifier
				scope Syntax
					using Lexical
					trivia = Trivia
					rule Unit
			""",
			Bind("""
				@using System.Text;

				Common = 'a'

				scope Lexical
				{
					Identifier = 'b'
				}

				scope Syntax
				{
					using Lexical;

					Unit = Identifier*
				}
				""").ToString());
	}

	[Fact]
	public void Trivia_is_an_ordinary_rule_that_shadowing_switches()
	{
		// §4.5: no directive and no mode — the inner Trivia simply shadows the outer.
		var model = Bind("""
			Trivia = Whitespace

			Whitespace = ' '
			Outer      = 'a'

			scope Lexical
			{
				Trivia = none

				Inner = 'b'
			}
			""");

		var global  = model.Root;
		var lexical = global.Nested.Single();

		Assert.Equal("Trivia", model.Trivia[global].Name);
		Assert.False(model.Trivia[global].IsBuiltIn);              // the grammar's own
		Assert.Equal("Trivia", model.Trivia[lexical].Name);
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
	public void An_inner_scope_sees_outward_but_not_inward()
	{
		Assert.Empty(Diagnostics("""
			Outer = 'a'

			scope Inner
			{
				Uses = Outer
			}
			"""));

		Assert.Contains(GrammarBinder.UndefinedName, Diagnostics("""
			scope Inner
			{
				Hidden = 'a'
			}

			Uses = Hidden
			"""));
	}

	[Fact]
	public void A_qualified_name_reaches_into_a_scope()
	{
		Assert.Empty(Diagnostics("""
			scope Lexical
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
	[InlineData("scope S { }\nA = Other.X",     GrammarBinder.UndefinedName)]
	[InlineData("using Absent;\nA = 'a'",       GrammarBinder.UnknownScope)]
	[InlineData("parse Absent\nA = 'a'",        GrammarBinder.UndefinedName)]
	public void Reports(string source, string expectedId)
	{
		Assert.Contains(expectedId, Diagnostics(source));
	}

	[Fact]
	public void A_duplicate_in_a_nested_scope_is_shadowing_not_an_error()
	{
		Assert.Empty(Diagnostics("""
			A = 'a'

			scope Inner
			{
				A = 'b'
			}
			"""));
	}

	[Fact]
	public void C_sharp_names_go_through_the_resolver()
	{
		var strict = new StrictResolver();
		var model  = GrammarBinder.Bind(
			GramParser.Parse(GramLexer.Tokenize("A = @IsLetter & where @IsSmall(x)")).File,
			strict);

		Assert.Contains(GrammarBinder.UnknownCSharp, model.Diagnostics.Select(d => d.Id));
		Assert.Contains("IsLetter", strict.Asked);
		Assert.Contains("IsSmall",  strict.Asked);
	}

	sealed class StrictResolver : ISymbolResolver
	{
		public System.Collections.Generic.List<string> Asked { get; } = [];

		public bool TypeExists(string qualifiedName)
		{
			Asked.Add(qualifiedName);
			return false;
		}

		public bool TryResolveMethod(string qualifiedName, int argumentCount, out MethodRole role)
		{
			Asked.Add(qualifiedName);
			role = MethodRole.ValueTransformation;

			return false;
		}

		public bool IsAssignable(string from, string to) => false;
	}
}
