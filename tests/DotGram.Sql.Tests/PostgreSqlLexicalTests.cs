using System;

using DotGram.Sql.Ast;
using DotGram.Sql.PostgreSql;

using Xunit;

namespace DotGram.Sql.Tests;

using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;

/// <summary>
/// The lexical elements of PostgreSQL as <see cref="PostgreSqlParser"/> reads them, held node by node to what was
/// written: which kind of string, which kind of number, which class of key word may be a name where.
/// </summary>
/// <remarks>
/// <see cref="PostgreSqlOracleTests"/> holds the same texts to the server's own parser; these hold what the tree keeps
/// of each.
/// </remarks>
public sealed class PostgreSqlLexicalTests
{
	// ── Strings ────────────────────────────────────────────────────────────────

	/// <summary>A string's kind is the letter that begins it, and its text is kept from its first quote.</summary>
	[Theory]
	[InlineData("'a'",                 StringLiteralKind.Character, "'a'")]
	[InlineData("'it''s'",             StringLiteralKind.Character, "'it''s'")]
	[InlineData("'a'\n'b'",            StringLiteralKind.Character, "'a'\n'b'")]
	[InlineData("'a' -- c\n  'b'",     StringLiteralKind.Character, "'a' -- c\n  'b'")]
	[InlineData("E'a\\n\\''",          StringLiteralKind.Escaped,   "'a\\n\\''")]
	[InlineData("e'\\u0041'",          StringLiteralKind.Escaped,   "'\\u0041'")]
	[InlineData("N'a'",                StringLiteralKind.National,  "'a'")]
	[InlineData("U&'d\\0061t'",        StringLiteralKind.Unicode,   "'d\\0061t'")]
	[InlineData("$$ a $$",             StringLiteralKind.Dollar,    "$$ a $$")]
	[InlineData("$tag$ a $x$ b $tag$", StringLiteralKind.Dollar,    "$tag$ a $x$ b $tag$")]
	[InlineData("$a_1$$$a_1$",         StringLiteralKind.Dollar,    "$a_1$$$a_1$")]
	public void A_string_keeps_its_kind_and_its_text(string input, StringLiteralKind kind, string text)
	{
		var value = Assert.IsType<LiteralValue.String>(PostgreSqlParser.ParseExpression(input));

		Assert.Equal(kind, value.Kind);
		Assert.Equal(text, value.Text);
		Assert.Null(value.UnicodeEscape);
	}

	/// <summary>A Unicode string's <c>UESCAPE</c> names its escape character, which stands beside the text.</summary>
	[Fact]
	public void A_unicode_string_keeps_its_escape_character()
	{
		var value = Assert.IsType<LiteralValue.String>(PostgreSqlParser.ParseExpression("U&'d!0061t' UESCAPE '!'"));

		Assert.Equal(StringLiteralKind.Unicode, value.Kind);
		Assert.Equal("'d!0061t'", value.Text);
		Assert.Equal('!', value.UnicodeEscape);
	}

	/// <summary>
	/// Two strings on one line are two tokens, a bracketed comment continues nothing, a tag is case-sensitive and
	/// cannot start with a digit, and a dollar quote is closed by its own tag or not at all.
	/// </summary>
	[Theory]
	[InlineData("'a' 'b'")]
	[InlineData("'a' /* \n */ 'b'")]
	[InlineData("$Tag$ a $tag$")]
	[InlineData("$1a$ b $1a$")]
	[InlineData("$a$ b")]
	[InlineData("$a$ b $a")]
	public void A_string_that_is_not_one_is_refused(string input)
	{
		Assert.False(PostgreSqlParser.TryParseExpression(input).IsSuccess);
	}

	/// <summary>A string of bits, written in bits or in hexits, keeps which.</summary>
	[Theory]
	[InlineData("B'0101'", "'0101'", false)]
	[InlineData("x'1FF'",  "'1FF'",  true)]
	public void A_bit_string_keeps_its_spelling(string input, string text, bool hexadecimal)
	{
		var value = Assert.IsType<LiteralValue.BitString>(PostgreSqlParser.ParseExpression(input));

		Assert.Equal(text, value.Text);
		Assert.Equal(hexadecimal, value.Hexadecimal);
	}

	// ── Numbers and parameters ─────────────────────────────────────────────────

	[Theory]
	[InlineData("42",        NumericLiteralKind.DecimalInteger)]
	[InlineData("1_000_000", NumericLiteralKind.DecimalInteger)]
	[InlineData("0x1F",      NumericLiteralKind.HexInteger)]
	[InlineData("0o17",      NumericLiteralKind.OctalInteger)]
	[InlineData("0b101",     NumericLiteralKind.BinaryInteger)]
	[InlineData("1.5",       NumericLiteralKind.Decimal)]
	[InlineData(".5",        NumericLiteralKind.Decimal)]
	[InlineData("5.",        NumericLiteralKind.Decimal)]
	[InlineData("1e10",      NumericLiteralKind.Approximate)]
	[InlineData("1.5e-3",    NumericLiteralKind.Approximate)]
	public void A_number_keeps_its_text_and_says_its_kind(string input, NumericLiteralKind kind)
	{
		var value = Assert.IsType<LiteralValue.Numeric>(PostgreSqlParser.ParseExpression(input));

		Assert.Equal(input, value.Text);
		Assert.Equal(kind, value.Kind);
	}

	/// <summary>A number a word runs into is one token no rule takes, as the server refuses it ("trailing junk").</summary>
	[Theory]
	[InlineData("123abc")]
	[InlineData("1e+")]
	[InlineData("0x")]
	[InlineData("1__0")]
	public void A_number_with_junk_after_it_is_refused(string input)
	{
		Assert.False(PostgreSqlParser.TryParseExpression(input).IsSuccess);
	}

	[Fact]
	public void A_positional_parameter_keeps_its_number()
	{
		var value = Assert.IsType<Expression.Parameter>(PostgreSqlParser.ParseExpression("$12"));

		Assert.Equal(ParameterKind.Positional, value.Kind);
		Assert.Equal(12, value.Ordinal);
		Assert.Null(value.Name);
	}

	// ── Names and the classes of key word ──────────────────────────────────────

	[Theory]
	[InlineData("abc",     "abc",     IdentifierStyle.Regular)]
	[InlineData("a$b",     "a$b",     IdentifierStyle.Regular)]
	[InlineData("ünïcode", "ünïcode", IdentifierStyle.Regular)]
	[InlineData("_x1",     "_x1",     IdentifierStyle.Regular)]
	[InlineData("\"a b\"", "\"a b\"", IdentifierStyle.Delimited)]
	[InlineData("\"a\"\"b\"", "\"a\"\"b\"", IdentifierStyle.Delimited)]
	[InlineData("\"select\"", "\"select\"", IdentifierStyle.Delimited)]
	public void A_name_keeps_its_spelling_and_its_style(string input, string text, IdentifierStyle style)
	{
		var reference = Assert.IsType<Expression.Reference>(PostgreSqlParser.ParseExpression(input));
		var name      = Assert.Single(reference.Name.Parts);

		Assert.Equal(text, name.Text);
		Assert.Equal(style, name.Style);
	}

	[Fact]
	public void A_unicode_name_keeps_its_escape_character()
	{
		var reference = Assert.IsType<Expression.Reference>(PostgreSqlParser.ParseExpression("U&\"d!0061t\" UESCAPE '!'"));
		var name      = Assert.Single(reference.Name.Parts);

		Assert.Equal("U&\"d!0061t\"", name.Text);
		Assert.Equal(IdentifierStyle.UnicodeDelimited, name.Style);
		Assert.Equal('!', name.UnicodeEscape);
	}

	/// <summary>A qualified name's parts after the first dot may be any key word (gram.y attr_name).</summary>
	[Theory]
	[InlineData("a.b.c",    3)]
	[InlineData("a.select", 2)]
	[InlineData("t.year",   2)]
	public void A_qualified_name_keeps_its_parts(string input, int parts)
	{
		var reference = Assert.IsType<Expression.Reference>(PostgreSqlParser.ParseExpression(input));

		Assert.Equal(parts, reference.Name.Parts.Count);
	}

	/// <summary>
	/// Where a column stands, a key word reserved or kept for function and type names is refused and every other is
	/// a name (gram.y ColId); a quoted one is a name anywhere.
	/// </summary>
	[Theory]
	[InlineData("year",     true)]
	[InlineData("between",  true)]
	[InlineData("abort",    true)]
	[InlineData("select",   false)]
	[InlineData("join",     false)]
	[InlineData("\"join\"", true)]
	[InlineData("\"\"",     false)]
	public void A_key_word_is_a_column_name_by_its_class(string input, bool read)
	{
		Assert.Equal(read, PostgreSqlParser.TryParseExpression(input).IsSuccess);
	}

	// ── Comments ───────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("/* a /* b */ c */ 1", true)]
	[InlineData("-- c\n1 -- d",        true)]
	[InlineData("/* a /* b */ 1",      false)]
	public void A_bracketed_comment_holds_others_to_any_depth(string input, bool read)
	{
		Assert.Equal(read, PostgreSqlParser.TryParseExpression(input).IsSuccess);
	}

	// ── The doors ──────────────────────────────────────────────────────────────

	/// <summary>The located door reads what the plain one reads, to the same tree, and says where a value was written.</summary>
	[Theory]
	[InlineData("'a'")]
	[InlineData("  $t$ x $t$  ")]
	[InlineData("a.b")]
	[InlineData("$3")]
	public void The_located_door_reads_the_same_tree_and_says_where(string input)
	{
		var plain   = PostgreSqlParser.ParseExpression(input);
		var located = PostgreSqlParser.Located.ParseExpression(input);

		Assert.Equal(plain, located);
		Assert.False(plain.Span.Known);
		Assert.True(located.Span.Known);
		Assert.Equal(input.Trim(), input.Substring(located.Span.At, located.Span.Length));
	}
}
