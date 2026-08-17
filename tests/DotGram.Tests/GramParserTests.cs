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
	public void Where_is_not_an_alias_for_when()
	{
		var result = GramParser.Parse(GramLexer.Tokenize(
			"Small = n: Number & where @IsSmall(n)",
			RoslynCSharpScanner.Instance));

		Assert.NotEmpty(result.Diagnostics);
		Assert.DoesNotContain("Guard", result.File.ToString(), StringComparison.Ordinal);
	}

	[Fact]
	public void Parses_usings_scopes_and_publications()
	{
		Assert.Equal(
			"""
			File
				Using (C#) "System.Text"
				Scope "Lexical"
					Using "Common"
					Rule "Token"
						Reference "A"
				Publication Find "Row" as "ReadRows"
			""",
			Parse("""
				@using System.Text;

				scope Lexical
				{
					using Common;

					Token = A
				}

				find Row as ReadRows
				"""));
	}

	[Fact]
	public void Contextual_keywords_are_still_ordinary_names()
	{
		// `parse`, `scope` and `find` only mean something where a declaration can start
		// and no `=`, `:` or `(` follows.
		Assert.Equal(
			"""
			File
				Rule "parse"
					Reference "A"
				Rule "scope"
					Reference "B"
				Rule "find"
					Reference "parse"
			""",
			Parse("""
				parse = A
				scope = B
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
	public void Reports(string source, string expectedId)
	{
		Assert.Contains(expectedId, Diagnostics(source));
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
