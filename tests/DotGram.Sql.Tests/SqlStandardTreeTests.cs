using System;
using System.Linq;

using DotGram.Sql.Ast;
using DotGram.Sql.Standard;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// What <see cref="SqlStandardParser"/> builds: the SQL:2023 tree (docs/design/sql-ast.md), held
/// node by node to what was written.
/// </summary>
/// <remarks>
/// <see cref="SqlStandardParserTests"/> holds the grammar to the BNF — what it reads. These hold the
/// tree to the text — what it keeps: a name's parts and its quotes, which word was written where
/// two mean the same.
/// </remarks>
public sealed class SqlStandardTreeTests
{
	// ── §5.4 Names and identifiers ─────────────────────────────────────────────

	[Theory]
	[InlineData("abc", "abc", IdentifierStyle.Regular)]
	[InlineData("\"a b\"", "\"a b\"", IdentifierStyle.Delimited)]
	[InlineData("U&\"a\\0041\"", "U&\"a\\0041\"", IdentifierStyle.UnicodeDelimited)]
	public void An_identifier_keeps_its_spelling_and_its_style(string input, string text, IdentifierStyle style)
	{
		var identifier = SqlStandardParser.ParseIdentifier(input);

		Assert.Equal(text, identifier.Text);
		Assert.Equal(style, identifier.Style);
		Assert.Null(identifier.UnicodeEscape);
	}

	/// <summary>
	/// The escape specifier is a part of the token, with nothing between it and the name, and the BNF
	/// reads the default character only — the Syntax Rules choose another.
	/// </summary>
	[Fact]
	public void A_Unicode_identifier_keeps_the_escape_character_it_names()
	{
		var identifier = SqlStandardParser.ParseIdentifier("U&\"a\\0041\"UESCAPE'\\'");

		Assert.Equal("U&\"a\\0041\"", identifier.Text);
		Assert.Equal('\\', identifier.UnicodeEscape);
	}

	[Theory]
	[InlineData("a", new[] { "a" })]
	[InlineData("a.b.c.d", new[] { "a", "b", "c", "d" })]
	[InlineData("a . \"b\"", new[] { "a", "\"b\"" })]
	public void An_identifier_chain_is_its_parts(string input, string[] parts) =>
		Assert.Equal(parts, SqlStandardParser.ParseIdentifierChain(input).Parts.Select(one => one.Text));

	[Theory]
	[InlineData("t", new[] { "t" })]
	[InlineData("c.s.t", new[] { "c", "s", "t" })]
	[InlineData("MODULE.t", new[] { "MODULE", "t" })]
	public void A_table_name_is_its_qualifiers_and_its_name(string input, string[] parts) =>
		Assert.Equal(parts, SqlStandardParser.ParseTableName(input).Parts.Select(one => one.Text));

	[Theory]
	[InlineData("a.b", new[] { "a", "b" })]
	[InlineData("module.x.y", new[] { "module", "x", "y" })]
	public void A_column_reference_is_a_reference_to_a_name(string input, string[] parts)
	{
		// Qualified: inside DotGram.Sql, `Expression` is the tree T-SQL builds.
		var reference = Assert.IsType<DotGram.Sql.Ast.Expression.Reference>(SqlStandardParser.ParseColumnReference(input));

		Assert.Equal(parts, reference.Name.Parts.Select(one => one.Text));
	}
}
