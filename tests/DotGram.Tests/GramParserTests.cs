using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The parser compared against its own tree dump, one node per line, indented.
/// </summary>
public sealed class GramParserTests
{
	static string Parse(string source) =>
		GramParser.Parse(GramLexer.Tokenize(source, RoslynCSharpScanner.Instance)).ToString();

	static string[] Diagnostics(string source) =>
		[.. GramParser.Parse(GramLexer.Tokenize(source)).Diagnostics.Select(d => d.Id)];

	[Fact]
	public void Parses_a_rule()
	{
		Assert.Equal(
			"""
			File
				Rule "Row"
					Sequence
						String "D"
						Char "|"
						Capture "symbol"
							Reference "Text"
			""",
			Parse("""Row = "D" & '|' & symbol: Text"""));
	}

	[Fact]
	public void Parses_a_typed_rule_with_parameters()
	{
		Assert.Equal(
			"""
			File
				Rule "List"
					Parameter "item"
					Parameter "sep"
					Type "item"[]
					Sequence
						Reference "item"
						ZeroOrMore
							Group
								Sequence
									Reference "sep"
									Reference "item"
			""",
			Parse("List(item, sep) : item[] = item & (sep & item)*"));
	}

	[Fact]
	public void Parses_choice_and_construction()
	{
		Assert.Equal(
			"""
			File
				Rule "Number"
					Type (C#) "int"
					Alternative
						OneOrMore
							ElementSet
								Range "0".."9"
						CSharp "int.Parse(text)"
			""",
			Parse("""Number : @int = ['0'..'9']+ => @(int.Parse(text))"""));
	}

	[Fact]
	public void Parses_lookahead_guard_and_quantifier_counts()
	{
		Assert.Equal(
			"""
			File
				Rule "Small"
					Sequence
						PositiveLookahead
							Capture "n"
								Reference "Number"
						Guard
							Call
								Reference (C#) "IsSmall"
								Reference "n"
						Count 2..4
							Reference "Digit"
			""",
			Parse("Small = ?=n: Number & when @IsSmall(n) & Digit{2,4}"));
	}

	[Fact]
	public void Distinguishes_an_atomic_group_from_a_repetition_count()
	{
		Assert.Equal(
			"""
			File
				Rule "R"
					Sequence
						Atomic
							Choice
								Char "a"
								Char "b"
						Count 2..3
							Char "c"
			""",
			Parse("R = { 'a' | 'b' } & 'c'{2,3}"));
	}

	[Fact]
	public void Parses_a_with_expression()
	{
		Assert.Equal(
			"""
			File
				Rule "ParseEuropeanNumber"
					With
						Reference "Number"
						Rebinding "Point" = "Comma"
			""",
			Parse("ParseEuropeanNumber = Number with (Point = Comma)"));
	}

	[Fact]
	public void A_preceding_capture_becomes_with_s_own_operand()
	{
		// §5.1: `with` is checked outermost of quantifier/capture/`recover` at this one
		// precedence level (it wraps what `ParseQuantifiedCore` already built), so
		// `c: Number with (...)` puts the capture inside `With`, not the other way
		// round — the property the whole splice-by-identity design depends on: lowering
		// `With`'s operand lowers the capture right along with it.
		Assert.Equal(
			"""
			File
				Rule "Row"
					Sequence
						Capture "a"
							Reference "Number"
						Char ","
						Capture "b"
							Reference "Number"
						Char ","
						With
							Capture "c"
								Reference "Number"
							Rebinding "Point" = "Comma"
			""",
			Parse("Row = a: Number & ',' & b: Number & ',' & c: Number with (Point = Comma)"));
	}

	[Fact]
	public void With_binds_outside_a_quantifier_not_inside_it()
	{
		// §5.1: checked last, outermost of quantifier/`recover`/`with` at this one
		// precedence level — `Number+ with (X=Y)` is `(Number+) with (X=Y)`, and the
		// reverse needs parens.
		Assert.Equal(
			"""
			File
				Rule "A"
					With
						OneOrMore
							Reference "Number"
						Rebinding "X" = "Y"
			""",
			Parse("A = Number+ with (X = Y)"));

		Assert.Equal(
			"""
			File
				Rule "B"
					OneOrMore
						Group
							With
								Reference "Number"
								Rebinding "X" = "Y"
			""",
			Parse("B = (Number with (X = Y))+"));
	}

	[Fact]
	public void A_malformed_with_header_recovers()
	{
		Assert.Contains(GramParser.ExpectedToken, Diagnostics("A = Number with (X)"));
	}

	[Fact]
	public void A_publication_may_carry_its_own_with_header()
	{
		Assert.Equal(
			"""
			File
				Publication Parse "Sum" as "Evaluate"
					Rebinding "trivia" = "none"
			""",
			Parse("parse Sum with (trivia = none) as Evaluate"));
	}

	[Fact]
	public void Where_is_not_an_alias_for_when()
	{
		var result = GramParser.Parse(GramLexer.Tokenize(
			"Small = n: Number & where @IsSmall(n)",
			RoslynCSharpScanner.Instance));

		Assert.NotEmpty(result.Diagnostics);
		Assert.DoesNotContain("Guard", result.File.ToString(), StringComparison.Ordinal);
	}

	[Fact]
	public void Parses_usings_namespaces_and_publications()
	{
		Assert.Equal(
			"""
			File
				Using (C#) "System.Text"
				Namespace "Lexical"
					Using "Common"
					Rule "Token"
						Reference "A"
				Publication Find "Row" as "ReadRows"
			""",
			Parse("""
				@using System.Text;

				namespace Lexical
				{
					using Common;

					Token = A
				}

				find Row as ReadRows
				"""));
	}

	[Fact]
	public void Parses_a_namespace_header()
	{
		Assert.Equal(
			"""
			File
				Namespace "Ctx"
					Rebinding "B" = "D"
					Rebinding "Identifier" = "SqlIdentifier"
					Rule "E"
						Reference "A"
			""",
			Parse("""
				namespace Ctx with (B = D, Identifier = SqlIdentifier)
				{
					E = A
				}
				"""));
	}

	[Fact]
	public void A_namespace_with_no_header_still_parses()
	{
		Assert.Equal(
			"""
			File
				Namespace "Ctx"
					Rule "E"
						Reference "A"
			""",
			Parse("""
				namespace Ctx
				{
					E = A
				}
				"""));
	}

	[Fact]
	public void A_malformed_namespace_header_recovers()
	{
		var result = GramParser.Parse(GramLexer.Tokenize(
			"namespace Ctx with (B) { E = A }\nGood = 'a'",
			RoslynCSharpScanner.Instance));

		Assert.NotEmpty(result.Diagnostics);
		Assert.Contains("Good", result.File.ToString(), StringComparison.Ordinal);
	}

	[Fact]
	public void A_namespace_header_without_with_is_refused_but_still_parsed()
	{
		// The check, not the capability: rebindings are still read and put in the
		// tree exactly as they would be with `with` written, so an author fixing the
		// diagnostic changes one word rather than rewriting the header.
		var result = GramParser.Parse(GramLexer.Tokenize(
			"namespace Ctx (B = D)\n{\n\tE = A\n}",
			RoslynCSharpScanner.Instance));

		Assert.Contains(GramParser.NamespaceNeedsWith, result.Diagnostics.Select(d => d.Id));
		Assert.Equal(
			"""
			File
				Namespace "Ctx"
					Rebinding "B" = "D"
					Rule "E"
						Reference "A"
			""",
			result.File.ToString());
	}

	[Fact]
	public void Contextual_keywords_are_still_ordinary_names()
	{
		// `parse`, `namespace` and `find` only mean something where a declaration can start
		// and no `=`, `:` or `(` follows.
		Assert.Equal(
			"""
			File
				Rule "parse"
					Reference "A"
				Rule "namespace"
					Reference "B"
				Rule "find"
					Reference "parse"
			""",
			Parse("""
				parse = A
				namespace = B
				find  = parse
				"""));
	}

	[Fact]
	public void One_broken_rule_costs_one_diagnostic()
	{
		var result = GramParser.Parse(GramLexer.Tokenize("""
			Good1 = A
			Broken = & &
			Good2 = B
			"""));

		// The rules on either side still parse: recovery resumes at the next
		// declaration rather than swallowing the rest of the file.
		var rules = result.File.Decls.OfType<Decl.Rule>().Select(r => r.Name);

		Assert.Equal(["Good1", "Broken", "Good2"], rules);
		Assert.NotEmpty(result.Diagnostics);
	}

	[Theory]
	[InlineData("A = ",              GramParser.ExpectedExpression)]
	[InlineData("A B",               GramParser.ExpectedToken)]
	[InlineData("= A",               GramParser.ExpectedDeclaration)]
	[InlineData("A = B{x,",          GramParser.ExpectedToken)]
	[InlineData("A = (B",            GramParser.ExpectedToken)]
	[InlineData("A = 'a'\nparse ('a' | 'b')",  GramParser.PublicationNeedsName)]
	[InlineData("A = 'a'\nfind 'a'+",          GramParser.PublicationNeedsName)]
	public void Reports(string source, string expectedId)
	{
		Assert.Contains(expectedId, Diagnostics(source));
	}

	[Fact]
	public void A_directive_publishes_an_expression_by_lifting_it_into_a_rule()
	{
		// §6 over §11's own principle: where the notation refers to a rule, an
		// expression may stand. The rule it becomes is declared where the directive is
		// written and named by the `as` the directive had to give, so everything after
		// the parser reads a publication of a rule exactly as it always did.
		var result = GramParser.Parse(
			GramLexer.Tokenize("Word = ['a'..'z']+\nparse ('a' | 'b') as Ab", null));

		Assert.Empty(result.Diagnostics);

		var rules = result.File.Decls.OfType<Decl.Rule>().Select(rule => rule.Name);

		Assert.Equal(["Word", "Ab"], rules);
		Assert.Equal("Ab", Assert.Single(result.File.Decls.OfType<Decl.Publish>()).RuleName);
	}

	[Fact]
	public void And_a_bare_name_is_still_the_name_it_always_was()
	{
		// No rule is lifted and no `as` is needed: the directive names a rule, which is
		// what it has always meant and what the method name is still derived from.
		var result = GramParser.Parse(GramLexer.Tokenize("Word = ['a'..'z']+\nparse Word", null));

		Assert.Empty(result.Diagnostics);
		Assert.Single(result.File.Decls.OfType<Decl.Rule>());
		Assert.Null(Assert.Single(result.File.Decls.OfType<Decl.Publish>()).Alias);
	}

	[Fact]
	public void And_a_rule_called_parse_is_still_a_rule()
	{
		// The word is contextual, and a parenthesis after it no longer settles it: a
		// declaration has an `=` or a `: Type` past its parameters and a directive does
		// not, which is the only thing that tells the two apart.
		var result = GramParser.Parse(
			GramLexer.Tokenize("parse(item) = item & ';'\nfind = 'f'\nStart = parse('a') & find", null));

		Assert.Empty(result.Diagnostics);
		Assert.Equal(["parse", "find", "Start"], result.File.Decls.OfType<Decl.Rule>().Select(rule => rule.Name));
		Assert.Empty(result.File.Decls.OfType<Decl.Publish>());
	}

	[Fact]
	public void A_correct_grammar_reports_nothing()
	{
		var result = GramParser.Parse(GramLexer.Tokenize("""
			@using System;

			parse Feed

			Feed : FeedItem[] = Header & Row* & Trailer & eof

			Header  = "H" & '|' & date: Date & eol
			Row     = "D" & '|' & symbol: Text & when @IsSupported(symbol) & eol
			Trailer = "T" & '|' & count: Number & eol

			Text   : string = [^ '|' | '\r' | '\n']+
			Number : @int   = ['0'..'9']+
			"""));

		Assert.Empty(result.Diagnostics);
		Assert.Equal(7, result.File.Decls.Count);
	}
}
