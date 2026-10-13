using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using DotGram.Sql.Ast;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// What <see cref="Sql92Parser"/> builds, held to what <see cref="SqlStandardParser"/> builds for the same
/// text, and to what <see cref="Sql2023Writer"/> writes back.
/// </summary>
/// <remarks>
/// <para>
/// The two parsers build one tree, and a construct both languages have is one shape in it: the same
/// record, the same members, the same folds and brackets. Nothing else holds the SQL-92 grammar's
/// actions to that. A wrong fold or a missing bracket node reads, round-trips through the writer and
/// passes every test of SQL-92 alone; it is only another reading of the same text that sees it.
/// </para>
/// <para>
/// The texts are the ones SQL-92 is known by: every string of <see cref="Sql92ParserTests"/>, the
/// SQL-92 rows of the benchmarks' stand, and the ScriptDom corpus, each statement as it is written and
/// as <see cref="SqlWriter"/> prints what <see cref="TransactSqlParser"/> read of it. Each is put to
/// every entry of SQL-92, and where it reads, to the standard's entry at the same level:
/// <c>ParseSearchCondition</c> to <c>ParseSearchCondition</c>, <c>ParseValueExpression</c> to
/// <c>ParseExpression</c>, and <c>ParseSelect</c> and <c>ParseQuery</c> to <c>ParseStatement</c>, whose
/// query is what <c>ParseQuery</c> is compared with. A span takes no part in either tree's equality.
/// </para>
/// </remarks>
public sealed class Sql92TreeTests
{
	/// <summary>The four entries of SQL-92, each a level the standard reads at too.</summary>
	public enum Level
	{
		Select,
		Query,
		SearchCondition,
		ValueExpression,
	}

	/// <summary>
	/// Where the two read the same text to different trees, and why that is not a defect. Each is a
	/// text SQL-92 reads and SQL:2023 reads otherwise; none may be a shape the SQL-92 grammar chose.
	/// </summary>
	static readonly Dictionary<(Level Level, string Text), string> Differences = new()
	{
		[(Level.SearchCondition, "a = ANY ((SELECT b FROM t))")] = AnyIsBoth,
		[(Level.SearchCondition, "a = SOME ((SELECT b FROM t))")] = AnyIsBoth,
		[(Level.ValueExpression, "CAST(a AS BIT)")] = "BIT is SQL-92's bit string type, which SQL:2003 removed; SQL:2023 reads the word as a user-defined type's name",
	};

	const string AnyIsBoth = "both readings are their language's own: SQL-92 has only the quantified comparison of a bracketed query, and SQL:2003 added ANY and SOME as aggregates, which SQL:2023 reads a bracketed subquery after as a call";

	/// <summary>
	/// What SQL-92 reads and SQL:2023 refuses, and why: each a word SQL:2023 reserves and SQL-92 does
	/// not, or a sign SQL:2023 does not stack. Keyed by the text with its spacing made one space, which
	/// is all that tells the corpus's copies of one statement apart.
	/// </summary>
	static readonly Dictionary<string, string> ReadBySql92Alone = new(StringComparer.Ordinal)
	{
		["RETURN"] = "RETURN is a reserved word in SQL:2023 and not in SQL-92's §5.2, so a name there",
		["SELECT - -1 FROM t"] = "a <factor> has one sign in SQL:2023 and in SQL-92 (§6.12) alike; this grammar's tower lets a sign stand before a signed value",
		["CAST(a AS BIT VARYING(8))"] = Removed,
		["CAST(a AS BIT /* x */ VARYING)"] = Removed,
		["B'0101'"] = Removed,
		["B''"] = Removed,
		["SELECT a FROM t UNION JOIN u"] = Removed,
		["SELECT a FROM t UNION JOIN u UNION JOIN v"] = Removed,
	};

	const string Removed = "SQL-92's bit string type, bit string literal and union join, which SQL:2003 removed, are SQL-92's own records";

	/// <summary>Every text both read, at every level both read it, is the same tree.</summary>
	[Fact]
	public void Every_text_both_parsers_read_is_the_same_tree()
	{
		var compared = 0;
		var faults   = new List<string>();

		foreach (var text in Texts())
			foreach (var level in Enum.GetValues<Level>())
			{
				if (Read(level, text) is not { } ours)
					continue;

				if (Standard(level, text) is not { } theirs)
					continue;

				compared++;

				if (Equals(ours, theirs) || Differences.ContainsKey((level, text)))
					continue;

				faults.Add($"{level} `{text}`: SQL-92 built {Sql2023Writer.Write(ours)}, SQL:2023 built {Sql2023Writer.Write(theirs)}");
			}

		Assert.True(compared > 300, $"{compared} readings compared");
		Assert.Equal([], faults);
	}

	/// <summary>What SQL-92 reads, SQL:2023 reads too, at the same level, but for the texts listed.</summary>
	[Fact]
	public void Every_text_SQL92_reads_SQL2023_reads_too()
	{
		var faults = new List<string>();
		var listed = new HashSet<string>(StringComparer.Ordinal);

		foreach (var text in Texts())
			foreach (var level in Enum.GetValues<Level>())
			{
				if (Read(level, text) is null || Standard(level, text) is not null)
					continue;

				if (ReadBySql92Alone.ContainsKey(Spaced(text)))
					listed.Add(Spaced(text));
				else
					faults.Add($"{level} `{text}`");
			}

		Assert.Equal([], faults);
		Assert.Equal(ReadBySql92Alone.Keys.Order(StringComparer.Ordinal), listed.Order(StringComparer.Ordinal));
	}

	/// <summary>A text with every run of spacing made one space.</summary>
	static string Spaced(string text)
	{
		return string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
	}

	/// <summary>Every text written above for a construct is one SQL-92 reads, so that none is compared by being refused.</summary>
	[Fact]
	public void Every_construct_is_read()
	{
		Assert.Equal([], Constructs.Where(static text => Enum.GetValues<Level>().All(level => Read(level, text) is null)));
	}

	/// <summary>
	/// What SQL-92 does not say, the grammar refuses rather than build: an interval qualifier §10.1 does not have,
	/// and a length or a precision larger than the tree's <see cref="int"/>.
	/// </summary>
	[Theory]
	[InlineData("CAST(a AS INTERVAL TIMEZONE_HOUR)")]
	[InlineData("CAST(a AS INTERVAL DAY TO TIMEZONE_MINUTE)")]
	[InlineData("CAST(a AS INTERVAL DAY TO HOUR(2))")]
	[InlineData("CAST(a AS INTERVAL DAY(2, 3))")]
	[InlineData("CAST(a AS INTERVAL DAY(2, 3) TO SECOND)")]
	[InlineData("CAST(a AS INTERVAL DAY TO SECOND(1, 2))")]
	[InlineData("CAST(a AS INTERVAL SECOND TO MINUTE)")]
	[InlineData("INTERVAL '1' HOUR(2, 3)")]
	[InlineData("CAST(a AS CHAR(2147483648))")]
	[InlineData("CAST(a AS BIT VARYING(2147483648))")]
	[InlineData("CAST(a AS NUMERIC(10, 2147483648))")]
	[InlineData("CAST(a AS FLOAT(99999999999999999999))")]
	[InlineData("CAST(a AS TIME(2147483648))")]
	[InlineData("INTERVAL '1' DAY(2147483648)")]
	[InlineData("INTERVAL '1' SECOND(1, 2147483648)")]
	public void What_SQL92_does_not_say_is_refused(string text)
	{
		Assert.False(Sql92Parser.TryParseValueExpression(text).IsSuccess, text);
		Assert.False(Sql92Parser.TryParseSelect("SELECT " + text + " FROM t").IsSuccess, text);
	}

	/// <summary>
	/// What only SQL-92 reads is built as SQL-92's own records, pinned here: the shape check cannot see them,
	/// and the writer's round trip agrees with itself whatever they are.
	/// </summary>
	[Fact]
	public void SQL92s_own_constructs_build_their_records()
	{
		Assert.Equal(
			new Ast.Expression.Cast(Column("a"), new Ast.DataType.BitString(Ast.BitStringTypeKind.BitVarying, 8)),
			Sql92Parser.ParseValueExpression("CAST(a AS BIT VARYING(8))"));
		Assert.Equal(
			new Ast.Expression.Cast(Column("a"), new Ast.DataType.BitString(Ast.BitStringTypeKind.Bit)),
			Sql92Parser.ParseValueExpression("CAST(a AS BIT)"));
		Assert.Equal(
			new Ast.Expression.Literal(new Ast.LiteralValue.BitString("'0101'")),
			Sql92Parser.ParseValueExpression("B'0101'"));

		var select = Assert.IsType<Ast.Query.Select>(Sql92Parser.ParseQuery("SELECT a FROM t UNION JOIN u"));
		var join   = Assert.IsType<Ast.TableSource.Join>(Assert.Single(select.From!.Sources));

		Assert.Equal(Ast.JoinKind.Union, join.Kind);
		Assert.Null(join.Specification);
		Assert.False(join.Natural);
		Assert.False(join.OuterKeyword);
	}

	static Ast.Expression Column(string name)
	{
		return new Ast.Expression.Reference(new Ast.QualifiedName([new Ast.Identifier(name)]));
	}

	/// <summary>
	/// A difference listed is one that is there: a text that no longer differs is taken off the list
	/// rather than left to excuse something else.
	/// </summary>
	[Fact]
	public void Every_listed_difference_is_one()
	{
		foreach (var ((level, text), why) in Differences)
		{
			var ours = Read(level, text);

			Assert.True(ours is not null, $"SQL-92 does not read `{text}` ({why})");
			Assert.False(Equals(ours, Standard(level, text)), $"`{text}` is no longer read differently ({why})");
		}
	}

	/// <summary>
	/// What SQL-92 reads, <see cref="Sql2023Writer"/> writes, and SQL-92 reads back to the same tree.
	/// </summary>
	[Fact]
	public void Every_text_SQL92_reads_is_the_same_tree_after_the_writer_and_a_reparse()
	{
		var read   = 0;
		var faults = new List<string>();

		foreach (var text in Texts())
			foreach (var level in Enum.GetValues<Level>())
			{
				if (Read(level, text) is not { } first)
					continue;

				read++;

				string written;

				try
				{
					written = Sql2023Writer.Write(first);
				}
				catch (Exception failure)
				{
					faults.Add($"{level} `{text}`: the writer threw {failure.GetType().Name}: {failure.Message}");

					continue;
				}

				var again = Read(level, written);

				if (again is null)
					faults.Add($"{level} `{text}` was written as `{written}`, which SQL-92 refuses");
				else if (!Equals(first, again))
					faults.Add($"{level} `{text}` was written as `{written}`, which SQL-92 reads to another tree");
			}

		Assert.True(read > 300, $"{read} readings written");
		Assert.Equal([], faults);
	}

	// ── The readings ────────────────────────────────────────────────────────────

	/// <summary>What SQL-92 reads at a level, or null.</summary>
	public static ISqlNode? Read(Level level, string text)
	{
		return level switch
		{
			Level.Select          => Sql92Parser.TryParseSelect(text) is { IsSuccess: true } select ? select.Value : null,
			Level.Query           => Sql92Parser.TryParseQuery(text) is { IsSuccess: true } query ? query.Value : null,
			Level.SearchCondition => Sql92Parser.TryParseSearchCondition(text) is { IsSuccess: true } condition ? condition.Value : null,
			_                     => Sql92Parser.TryParseValueExpression(text) is { IsSuccess: true } value ? value.Value : null,
		};
	}

	/// <summary>What SQL:2023 reads at the same level, or null: a query is a statement's.</summary>
	public static ISqlNode? Standard(Level level, string text)
	{
		switch (level)
		{
			case Level.Select:
				return SqlStandardParser.TryParseStatement(text) is { IsSuccess: true } select ? select.Value : null;

			case Level.Query:
				return SqlStandardParser.TryParseStatement(text) is { IsSuccess: true, Value: Ast.Statement.Select query } ? query.Query : null;

			case Level.SearchCondition:
				return SqlStandardParser.TryParseSearchCondition(text) is { IsSuccess: true } condition ? condition.Value : null;

			default:
				return SqlStandardParser.TryParseExpression(text) is { IsSuccess: true } value ? value.Value : null;
		}
	}

	// ── The texts ───────────────────────────────────────────────────────────────

	/// <summary>Every text, once.</summary>
	public static IEnumerable<string> Texts()
	{
		var seen = new HashSet<string>(StringComparer.Ordinal);

		foreach (var text in TestTexts().Concat(Constructs).Concat(StandTexts()).Concat(CorpusTexts()))
			if (seen.Add(text))
				yield return text;
	}

	/// <summary>Every string of <see cref="Sql92ParserTests"/>, read or refused: what is refused is not compared.</summary>
	static IEnumerable<string> TestTexts()
	{
		foreach (var method in typeof(Sql92ParserTests).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).OrderBy(static one => one.Name, StringComparer.Ordinal))
			foreach (var row in method.GetCustomAttributes<InlineDataAttribute>())
				if (row.Data is [string text, ..])
					yield return text;
	}

	/// <summary>
	/// A text for every construct the grammar builds, which the strings of the parser's tests and the
	/// corpus leave out: every predicate, value function, literal, type and clause.
	/// </summary>
	static readonly string[] Constructs =
	[
		// Conditions and predicates.
		"a = 1 IS NOT TRUE", "(a = 1) IS UNKNOWN", "NOT a = 1", "a = 1 OR b = 2 AND c = 3", "NOT (a = 1 OR b = 2)",
		"EXISTS (SELECT * FROM t)", "UNIQUE (SELECT a FROM t)",
		"a = ALL (SELECT b FROM t)", "a < SOME (SELECT b FROM t)", "a >= ANY (SELECT b FROM t)", "a <> b", "a <= b",
		"a NOT BETWEEN 1 AND 2", "a NOT IN (1, 2)", "a IN (SELECT b FROM t)", "a NOT LIKE 'x%'", "a LIKE 'x' ESCAPE '!'",
		"a IS NULL", "a IS NOT NULL", "a MATCH (SELECT b FROM t)", "(a, b) MATCH UNIQUE PARTIAL (SELECT a, b FROM t)",
		"a MATCH FULL (SELECT b FROM t)", "(a, b) OVERLAPS (c, d)", "(a, b) = (1, 2)", "(a) = 1", "((a = 1))",
		"a = (SELECT b FROM t)", "((SELECT b FROM t)) = a", "(SELECT a FROM t) = (SELECT b FROM u)",

		// Values.
		"-a * +b", "a - -b", "a || 'x'", "a COLLATE x", "a COLLATE \"de\" || b", "-(a + b)", "(a)", "((a))",
		"CASE a WHEN 1 THEN 'x' ELSE NULL END", "CASE WHEN a = 1 THEN NULL END", "NULLIF(a, b)", "COALESCE(a, b, c)",
		"CAST(NULL AS INTEGER)", "CAST(a AS CHARACTER VARYING(10))", "CAST(a AS CHAR VARYING)", "CAST(a AS VARCHAR(1))",
		"CAST(a AS NATIONAL CHAR VARYING(5))", "CAST(a AS NATIONAL CHARACTER)", "CAST(a AS NCHAR(3))",
		"CAST(a AS NCHAR VARYING(3))", "CAST(a AS CHAR(3) CHARACTER SET latin1)", "CAST(a AS CHARACTER)",
		"CAST(a AS DECIMAL(10, 2))", "CAST(a AS NUMERIC(10))", "CAST(a AS DEC)", "CAST(a AS FLOAT(5))", "CAST(a AS FLOAT)",
		"CAST(a AS REAL)", "CAST(a AS DOUBLE PRECISION)", "CAST(a AS SMALLINT)", "CAST(a AS INT)", "CAST(a AS INTEGER)",
		"CAST(a AS DATE)", "CAST(a AS TIME(3) WITH TIME ZONE)", "CAST(a AS TIME)", "CAST(a AS TIMESTAMP(6))",
		"CAST(a AS INTERVAL DAY TO SECOND(3))", "CAST(a AS INTERVAL YEAR(2) TO MONTH)", "CAST(a AS INTERVAL SECOND(2, 3))",
		"CAST(a AS INTERVAL HOUR)", "CAST(a AS INTERVAL MINUTE(4) TO SECOND)",
		"POSITION('a' IN b)", "EXTRACT(YEAR FROM d)", "EXTRACT(TIMEZONE_HOUR FROM d)", "CHAR_LENGTH(a)",
		"CHARACTER_LENGTH(a)", "OCTET_LENGTH(a)", "BIT_LENGTH(a)", "SUBSTRING(a FROM 1)", "SUBSTRING(a FROM 1 FOR 2)",
		"UPPER(a)", "LOWER(a)", "CONVERT(a USING x)", "TRANSLATE(a USING s.x)", "TRIM(a)", "TRIM(FROM a)",
		"TRIM(LEADING FROM a)", "TRIM(BOTH 'x' FROM a)", "TRIM('x' FROM a)", "TRIM(TRAILING 'x' FROM a)",
		"CURRENT_DATE", "CURRENT_TIME", "CURRENT_TIME(3)", "CURRENT_TIMESTAMP", "CURRENT_TIMESTAMP(6)",
		"COUNT(*)", "COUNT(DISTINCT a)", "SUM(ALL a)", "MAX(a)", "MIN(a) + AVG(b)",
		"1.5", ".5", "1.", "1E10", "1.5e-3", "'it''s'", "N'abc'", "X'0F'", "_latin1'abc'", "DATE '2020-01-01'",
		"TIME '10:00:00'", "TIMESTAMP '2020-01-01 10:00:00'", "INTERVAL '1' DAY", "INTERVAL -'1-2' YEAR TO MONTH",
		"INTERVAL '10:20' HOUR TO MINUTE", "INTERVAL '1' SECOND(2, 3)",
		":a", ":a INDICATOR :b", ":a :b", "?", "USER", "CURRENT_USER", "SESSION_USER", "SYSTEM_USER", "VALUE",
		"a.b.c", "\"a\".b", "(SELECT a FROM t)", "((SELECT a FROM t))", "(SELECT a FROM t) + 1",

		// Queries.
		"SELECT ALL a FROM t", "SELECT a FROM t GROUP BY a COLLATE x HAVING COUNT(*) > 1", "SELECT a FROM t GROUP BY s.t.a",
		"SELECT a FROM t UNION CORRESPONDING SELECT a FROM u", "SELECT a FROM t EXCEPT CORRESPONDING BY (a, b) SELECT a FROM u",
		"SELECT a FROM t INTERSECT ALL SELECT a FROM u", "SELECT a FROM t EXCEPT ALL SELECT a FROM u",
		"(SELECT a FROM t) UNION (SELECT b FROM u)", "((SELECT a FROM t))", "(SELECT a FROM t UNION SELECT b FROM u) INTERSECT SELECT c FROM v",
		"SELECT a FROM t UNION SELECT b FROM u ORDER BY 1", "VALUES 1, 2", "VALUES (1, 2)", "VALUES (1, 2), (3, 4) UNION VALUES 5",
		"TABLE s.t", "SELECT a FROM t AS x (c, d)", "SELECT a FROM t x", "SELECT a FROM (SELECT a FROM t) x",
		"SELECT a FROM t LEFT JOIN u USING (a) RIGHT OUTER JOIN v ON a = b", "SELECT a FROM (t JOIN u ON a = b)",
		"SELECT a FROM t NATURAL JOIN u", "SELECT a FROM t FULL OUTER JOIN u ON 1 = 1", "SELECT a FROM t NATURAL LEFT JOIN u",
		"SELECT a FROM t CROSS JOIN u CROSS JOIN v", "SELECT a FROM (t CROSS JOIN u) JOIN v ON 1 = 1",
		"SELECT a FROM t ORDER BY a COLLATE x ASC, 1 DESC", "SELECT a AS \"x y\" FROM t", "SELECT t.a, u.* FROM t, u",
		"SELECT s.t.* FROM s.t", "SELECT \"x\"\"y\".* FROM t", "SELECT s /* . t */ . \"a.b\" -- c\n . * FROM t",
		"SELECT a.b, a.*, a.b.c FROM t", "SELECT a FROM t WHERE a IN (SELECT b FROM u WHERE b = t.a)",
		"SELECT (SELECT b FROM u) AS c FROM t", "SELECT CASE WHEN a = 1 THEN b END FROM t",

		// Words read as one capture, with comments between them.
		"CAST(a AS CHAR --c\rVARYING(10))", "CAST(a AS NATIONAL --c\nCHAR(10))", "CAST(a AS NCHAR /* VARYING */ VARYING)",
		"INTERVAL - /* ' */ '1' DAY", "SELECT a FROM t /* AS */ x", "SELECT a FROM t AS -- c\n x", "SELECT a /* AS */ b FROM t",
		":a INDICATOR /* : */ :b",

		// What SQL-92 has and SQL:2003 removed, which are SQL-92's own records.
		"CAST(a AS BIT)", "CAST(a AS BIT VARYING(8))", "CAST(a AS BIT /* x */ VARYING)", "B'0101'", "B''",
		"SELECT a FROM t UNION JOIN u", "SELECT a FROM t UNION JOIN u UNION JOIN v",

		// The largest number the tree holds, and a precision that is a literal and keeps its digits.
		"CAST(a AS CHAR(2147483647))", "CAST(a AS NUMERIC(2147483647, 0))", "INTERVAL '1' DAY(2147483647)",
		"SELECT CAST(a AS VARCHAR(2147483647)) FROM t", "CURRENT_TIME(2147483648)", "CURRENT_TIMESTAMP(99999999999999999999999999)",

		// An interval primary (§6.15): a field's word after a value is its qualifier, not a name for it.
		"SELECT b DAY FROM t", "SELECT b YEAR TO MONTH FROM t", "(a - b) DAY TO SECOND", "a HOUR(2) > b",

		// A quantified comparison whose query is in brackets of its own, which SQL:2023 reads otherwise.
		"a = ANY ((SELECT b FROM t))", "a = SOME ((SELECT b FROM t))",

		// An ORDER BY after brackets is the outer query's, not the bracketed one's.
		"(SELECT a FROM t) ORDER BY a", "((SELECT a FROM t) UNION SELECT b FROM u) ORDER BY 1",
		"(SELECT a FROM t UNION SELECT b FROM u) ORDER BY 1",
	];

	/// <summary>The SQL-92 rows of the stand (benchmarks/DotGram.Benchmarks/Stand.cs) that read.</summary>
	static IEnumerable<string> StandTexts()
	{
		yield return "42";
		yield return string.Join(" + ", Enumerable.Repeat("a + b * (c - d) / e + f * g - h * (i + j) / k", 20));
		yield return string.Join(" AND ", Enumerable.Range(0, 100).Select(static i => "a" + i + " = 1"));
		yield return "SELECT a FROM t";
		yield return "SELECT " + string.Join(", ", Enumerable.Range(0, 20).Select(static i => "a" + i)) + " FROM t WHERE a0 = 1";
		yield return "SELECT c.CustomerId, c.Name, o.OrderId, o.Total FROM Customers AS c INNER JOIN Orders AS o ON o.CustomerId = c.CustomerId WHERE o.Total > 100 AND c.Region = 'EU' ORDER BY o.Total DESC";
	}

	/// <summary>
	/// The ScriptDom corpus: each statement as its file writes it, and each query
	/// <see cref="TransactSqlParser"/> reads as <see cref="SqlWriter"/> prints it, which is where the
	/// SQL-92 the corpus holds is found — the files are T-SQL, and their brackets and words are not.
	/// </summary>
	static IEnumerable<string> CorpusTexts()
	{
		foreach (var file in Directory.GetFiles(Corpus(), "*.sql", SearchOption.AllDirectories).OrderBy(static one => one, StringComparer.Ordinal))
			foreach (var batch in SqlScript.Read(File.ReadAllText(file)).Batches)
			{
				foreach (var statement in batch.Text.Split(';'))
					if (statement.Trim() is { Length: > 0 } one)
						yield return one;

				var read = TransactSqlParser.TryParseSql(batch);

				if (!read.IsSuccess)
					continue;

				foreach (var statement in read.Value!)
				{
					if (statement is not Statement.Select)
						continue;

					string? printed;

					try
					{
						printed = SqlWriter.Write(statement);
					}
					catch (Exception)
					{
						printed = null; // The writer's own fault, which TreeEqualityTests holds; not this test's.
					}

					if (printed is not null)
						yield return printed;
				}
			}
	}

	static string Corpus([CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Corpus", "ScriptDom"));
	}
}
