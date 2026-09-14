using System;

using DotGram.Sql.Standard;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// The SQL:2023 grammar, read: every row here is answered the same way by the standard's BNF.
/// </summary>
/// <remarks>
/// <para>
/// The verdicts are not this suite's own. Each row was put to `--standard` in DotGram.Benchmarks,
/// which reads the published BNF with an Earley recognizer and asks the grammar's rule of the same
/// name beside it, and the two agreed. A row that is added here is added there first.
/// </para>
/// <para>
/// Verdicts and not trees — the grammar builds none yet.
/// </para>
/// </remarks>
public sealed class SqlStandardParserTests
{
	// ── §5.3 Literals ────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("1")]
	[InlineData("-1")]
	[InlineData("+ 1.5")]
	[InlineData(".5")]
	[InlineData("1.")]
	[InlineData("1E5")]
	[InlineData("1e5")]
	[InlineData("1.5E-3")]
	[InlineData("1.e5")]
	[InlineData("1E+5")]
	[InlineData("0X1F")]
	[InlineData("0x1f")]
	[InlineData("0X_1")]
	[InlineData("0O17")]
	[InlineData("0B101")]
	[InlineData("1_000")]
	[InlineData("'abc'")]
	[InlineData("''")]
	[InlineData("'it''s'")]
	[InlineData("'a' 'b'")]
	[InlineData("'a'/* c */'b'")]
	[InlineData("'a' -- c")]
	[InlineData("N'abc'")]
	[InlineData("n'abc'")]
	[InlineData("N'a' 'b'")]
	[InlineData("X'0A1b'")]
	[InlineData("X'0 A'")]
	[InlineData("X'0A' 'B1'")]
	[InlineData("U&'\\0041'")]
	[InlineData("U&'a' 'b'")]
	[InlineData("_latin1'abc'")]
	[InlineData("_s.latin1'abc'")]
	[InlineData("_\"s\".latin1'a'")]
	[InlineData("_absolute.latin1'a'")]
	[InlineData("DATE '2020-01-01'")]
	[InlineData("DATE '2020-1-1'")]
	[InlineData("date'2020-01-01'")]
	[InlineData("TIME '12:00:00'")]
	[InlineData("TIME '12:00:00.'")]
	[InlineData("TIME '1:2:3'")]
	[InlineData("TIME '12:00:00+01:00'")]
	[InlineData("TIMESTAMP '2020-01-01 12:00:00.5'")]
	[InlineData("INTERVAL '1' DAY")]
	[InlineData("INTERVAL -'1-2' YEAR TO MONTH")]
	[InlineData("INTERVAL '-1' YEAR")]
	[InlineData("INTERVAL - '1' YEAR")]
	[InlineData("INTERVAL '1 2:3:4.5' DAY TO SECOND(3)")]
	[InlineData("INTERVAL '1 2' DAY TO HOUR")]
	[InlineData("INTERVAL '1:2' HOUR TO MINUTE")]
	[InlineData("INTERVAL '5.5' SECOND")]
	[InlineData("INTERVAL '5' SECOND(2, 3)")]
	[InlineData("INTERVAL '1' YEAR(2) TO MONTH")]
	[InlineData("INTERVAL '5' MONTH TO YEAR")]
	[InlineData("TRUE")]
	[InlineData("FALSE")]
	[InlineData("unknown")]
	[InlineData("- /* c */ 1")]
	[InlineData("/* a /* b */ c */ 1")]
	public void A_literal_reads(string input)
	{
		Assert.True(SqlStandardParser.TryParseLiteral(input).IsSuccess, input);
	}

	/// <summary>What the BNF refuses, and why it does.</summary>
	/// <remarks>
	/// A digit follows one underscore and not two; hexits come in pairs; SQL:2023 has no bit
	/// string; the escape character a `UESCAPE` names is not read; nothing stands between an
	/// introducer and its name, or its name and the quote, and the name is no reserved word; a
	/// date string is a date; a timestamp's two halves have one space between them; a second
	/// is not a field to end at; `NULL` is no literal; `2K` is a large object's length.
	/// </remarks>
	[Theory]
	[InlineData("1__0")]
	[InlineData("0X")]
	[InlineData(".E5")]
	[InlineData("X'0'")]
	[InlineData("B'101'")]
	[InlineData("U&'#0041' UESCAPE '#'")]
	[InlineData("_ latin1 'abc'")]
	[InlineData("_latin1 'a'")]
	[InlineData("_select.latin1'a'")]
	[InlineData("_a.b.c.latin1'a'")]
	[InlineData("DATE 'x'")]
	[InlineData("TIMESTAMP '2020-01-01  12:00:00'")]
	[InlineData("INTERVAL '1' SECOND TO SECOND")]
	[InlineData("NULL")]
	[InlineData("abc")]
	[InlineData("'abc")]
	[InlineData("1 2")]
	[InlineData("2K")]
	public void What_is_not_a_literal_is_refused(string input)
	{
		Assert.False(SqlStandardParser.TryParseLiteral(input).IsSuccess, input);
	}

	// ── §5.4 Names and identifiers ───────────────────────────────────────────────

	[Theory]
	[InlineData("a")]
	[InlineData("a1")]
	[InlineData("é")]
	[InlineData("a·b")]
	[InlineData("\"select\"")]
	[InlineData("\"a\"\"b\"")]
	[InlineData("absolute")]
	[InlineData("absolutely")]
	[InlineData("selectx")]
	[InlineData("x_select")]
	[InlineData("U&\"a\\0041\"")]
	[InlineData("U&\"\\+000041\"")]
	[InlineData("u&\"a\"")]
	[InlineData("U&\"a\"UESCAPE'\\'")]
	public void An_identifier_reads(string input)
	{
		Assert.True(SqlStandardParser.TryParseIdentifier(input).IsSuccess, input);
	}

	/// <summary>A reserved word in any case, an empty delimited identifier, and what no identifier begins or holds.</summary>
	[Theory]
	[InlineData("select")]
	[InlineData("SeLeCt")]
	[InlineData("abs")]
	[InlineData("absent")]
	[InlineData("END-EXEC")]
	[InlineData("\"\"")]
	[InlineData("_x")]
	[InlineData("1a")]
	[InlineData("a$")]
	[InlineData("a.b")]
	public void What_is_not_an_identifier_is_refused(string input)
	{
		Assert.False(SqlStandardParser.TryParseIdentifier(input).IsSuccess, input);
	}

	[Theory]
	[InlineData("a.b.c", true)]
	[InlineData("a . b", true)]
	[InlineData("a./* c */b", true)]
	[InlineData("\"a\".b", true)]
	[InlineData("a.b.c.d", true)]
	[InlineData("a.select", false)]
	[InlineData("a.", false)]
	[InlineData(".a", false)]
	[InlineData("MODULE.a.b", false)]
	public void An_identifier_chain(string input, bool reads)
	{
		Assert.Equal(reads, SqlStandardParser.TryParseIdentifierChain(input).IsSuccess);
	}

	/// <summary>
	/// A column reference is a chain, or a module's own: `MODULE.a.b`, where `MODULE` is reserved
	/// and no chain begins.
	/// </summary>
	[Theory]
	[InlineData("a.b.c", true)]
	[InlineData("MODULE.a.b", true)]
	[InlineData("MODULE.a", false)]
	public void A_column_reference(string input, bool reads)
	{
		Assert.Equal(reads, SqlStandardParser.TryParseColumnReference(input).IsSuccess);
	}

	/// <summary>
	/// A table's name has a schema's before it and a catalog's before that, and no more; the
	/// qualifier is not taken so greedily that no name is left for the end.
	/// </summary>
	[Theory]
	[InlineData("a", true)]
	[InlineData("a.b", true)]
	[InlineData("a.b.c", true)]
	[InlineData("MODULE.a", true)]
	[InlineData("a.b.c.d", false)]
	[InlineData("MODULE.a.b", false)]
	public void A_table_name(string input, bool reads)
	{
		Assert.Equal(reads, SqlStandardParser.TryParseTableName(input).IsSuccess);
	}
}
