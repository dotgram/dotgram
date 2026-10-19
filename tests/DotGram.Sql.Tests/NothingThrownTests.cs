using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// No public <c>Try</c> entry of <see cref="TransactSqlParser"/> throws: a text is read into the common tree, with an
/// extension where it has no place yet, or refused. Held over the corpus and over the shapes that once reached a throw
/// on the way to the common tree: a result set that is more than columns, a statement holding one inside a script that is then
/// refused, a comment between the words of what is dropped, a comment inside a type the writer cannot print, and a line
/// comment among the words a statement keeps as text.
/// </summary>
public sealed class NothingThrownTests
{
	[Theory]
	[InlineData("EXECUTE p WITH RESULT SETS ((a INT, CHECK (a > 0)))")]
	[InlineData("EXECUTE p WITH RESULT SETS ((a INT CHECK (a > 0)))")]
	[InlineData("EXECUTE p WITH RESULT SETS ((a INT, CONSTRAINT k PRIMARY KEY (a)))")]
	[InlineData("EXECUTE p WITH RESULT SETS ((a INT NOT NULL PRIMARY KEY))")]
	[InlineData("EXECUTE p WITH RESULT SETS ((a INT, INDEX ix (a)))")]
	[InlineData("EXECUTE p WITH RESULT SETS ((a INT, b AS a + 1))")]
	[InlineData("EXECUTE (@SQL) WITH RESULT SETS ((Dobidoo BIGINT NOT NULL, CHECK (EXISTS (SELECT 1 FROM u AS a))))")]
	[InlineData("DECLARE @SQL NVARCHAR(max)\r\nSET @SQL = N'SELECT a FROM t1 WHERE 1 = 1'\r\nexec(@SQL)\r\nwith RESULT SETS((Dobidoo BIGINT NOT NULL, CHECK (EXISTS (SELECT 1 FROM u AS a))))\r\ngo\r\nexecute p1 with result sets none\r\ngo\r\n")]
	[InlineData("DECLARE @SQL AS NVARCHAR (MAX);\r\n\r\nSET @SQL = N'SELECT a FROM t1 WHERE 1 = 1';\r\n\r\nEXECUTE (@SQL)\r\n    WITH RESULT SETS (\r\n        (Dobidoo BIGINT NOT NULL, CHECK (a IN (SELECT 1 FROM u a TABLESAMPLE (10 ROWS) (NOLOCK))))\r\n                     );\r\n\r\n\r\nGO\r\nEXECUTE p1 WITH RESULT SETS NONE;\r\n")]
	[InlineData("DROP SERVER /*x*/ AUDIT a")]
	[InlineData("DROP SYMMETRIC/**/KEY k")]
	[InlineData("DROP MASTER /* */ KEY")]
	[InlineData("DROP MASTER --x\rKEY")]
	[InlineData("DROP MASTER --x\r\nKEY")]
	[InlineData("DROP SERVER AUDIT --x\rSPECIFICATION a")]
	[InlineData("CREATE SEARCH PROPERTY LIST p AUTHORIZATION --c\ra")]
	[InlineData("CREATE FULLTEXT INDEX ON t (a, --c\rb) KEY INDEX ix")]
	[InlineData("CREATE FULLTEXT INDEX ON t (a, --c\r\nb) KEY INDEX ix")]
	[InlineData("CREATE PROCEDURE p1 @lastname NVARCHAR/**/ (100) AS SELECT 1")]
	[InlineData("CREATE AGGREGATE a (@x INT/**/ NULL) RETURNS INT EXTERNAL NAME asm")]
	[InlineData("DECLARE @b NVARCHAR/**/ (50)")]
	[InlineData("CREATE SEARCH PROPERTY LIST p AUTHORIZATION --c\na")]
	[InlineData("CREATE FULLTEXT INDEX ON t (a, --c\nb) KEY INDEX ix")]
	[InlineData("CREATE FULLTEXT INDEX ON t (a) KEY INDEX ix ON (cat, --c\nFILEGROUP fg)")]
	[InlineData("CREATE FULLTEXT INDEX ON t (/*a)*/a) KEY INDEX ix")]
	[InlineData("CREATE FULLTEXT INDEX ON t (a /*)]*/, b --)]\n) KEY INDEX ix")]
	[InlineData("DROP MASTER\u200BKEY")]
	[InlineData("DROP MASTER\u0001KEY")]
	[InlineData("DROP\u00A0SERVER\u3000AUDIT\u0085a")]
	[InlineData("CREATE STATISTICS s ON t ([a]]  b], \"c\"\"  d\")")]
	[InlineData("DROP MASTER /* a /* b */ c */ KEY")]
	public void A_shape_that_once_threw_is_read_carried_or_refused(string input)
	{
		foreach (var (entry, read) in Entries())
		{
			var thrown = Record.Exception(() => read(input));

			Assert.True(thrown is null, $"{entry} threw {thrown?.GetType().Name}: {thrown?.Message}");
		}
	}

	/// <summary>
	/// Text a statement keeps as words and reads again outside the grammar is read the way the grammar reads it: a
	/// bracket inside a comment is no bracket, the grammar's spacing parts two words, a line comment ends at its line,
	/// a closing quote written twice stays inside the name, and a comment holding another is one comment.
	/// </summary>
	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(4)]
	[InlineData(5)]
	[InlineData(6)]
	public void Text_read_again_is_read_as_the_grammar_reads_it(int row)
	{
		var table = new Ast.QualifiedName(new Ast.Identifier("t"));
		var key   = new Ast.Identifier("ix");

		var (text, expected) = row switch
		{
			0 => ("CREATE FULLTEXT INDEX ON t (/*a)*/a) KEY INDEX ix",
				(Ast.Statement)new Ast.Statement.CreateFullTextIndex(table, [new Ast.FullTextIndexColumn(new Ast.Identifier("a"))], key)),
			1 => ("DROP MASTER\u200BKEY", new Ast.Statement.DropMasterKey()),
			2 => ("DROP MASTER\u0001KEY", new Ast.Statement.DropMasterKey()),
			3 => ("CREATE FULLTEXT INDEX ON t (a, --x\rb) KEY INDEX ix",
				new Ast.Statement.CreateFullTextIndex(table,
					[new Ast.FullTextIndexColumn(new Ast.Identifier("a")), new Ast.FullTextIndexColumn(new Ast.Identifier("b"))], key)),
			6 => ("CREATE FULLTEXT INDEX ON t (a, --x\nb) KEY INDEX ix",
				new Ast.Statement.CreateFullTextIndex(table,
					[new Ast.FullTextIndexColumn(new Ast.Identifier("a")), new Ast.FullTextIndexColumn(new Ast.Identifier("b"))], key)),
			4 => ("CREATE FULLTEXT INDEX ON t ([a]]  b]) KEY INDEX ix",
				new Ast.Statement.CreateFullTextIndex(table, [new Ast.FullTextIndexColumn(new Ast.Identifier("[a]]  b]", Ast.IdentifierStyle.Bracketed))], key)),
			_ => ("CREATE PROCEDURE p WITH NATIVE_COMPILATION, SCHEMABINDING AS BEGIN ATOMIC WITH (TRANSACTION ISOLATION /* a /* b */ c */ LEVEL = SNAPSHOT, LANGUAGE = N'us_english') SELECT 1 END",
				TransactSqlParser.ParseStatement("CREATE PROCEDURE p WITH NATIVE_COMPILATION, SCHEMABINDING AS BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N'us_english') SELECT 1 END")),
		};

		Assert.Equal(expected, TransactSqlParser.ParseStatement(text));
		Assert.Equal(expected, TransactSqlParser.Located.ParseStatement(text));
		Assert.Equal([expected], TransactSqlParser.ParseSql(text));

		if (row == 5)
			Assert.IsType<Ast.AtomicOption.Isolation>(((Ast.RoutineBody.Statements)((Ast.Statement.CreateRoutine)expected).Definition.Body).List[0] is Ast.Statement.Compound { Atomic: [var first, ..] } ? first : null);
	}

	/// <summary>
	/// A block comment in text read again is a comment before anything inside it is read: a <c>--</c>, a quote or a
	/// bracket in it is none of them, and a <c>/*</c> after a line comment's <c>--</c> is the line comment's. The
	/// masking function's string is the same as without the comment.
	/// </summary>
	[Theory]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD MASKED WITH (FUNCTION = /* -- */ 'default()')")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD MASKED WITH (FUNCTION = /* ' */ 'default()')")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD MASKED WITH (FUNCTION = /* [ */ 'default()')")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD MASKED WITH (FUNCTION = /* a /* -- ' */ b */ 'default()')")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD MASKED WITH ( -- /*\nFUNCTION = 'default()')")]
	public void A_comment_in_text_read_again_hides_what_is_inside_it(string text)
	{
		var expected = TransactSqlParser.ParseStatement("ALTER TABLE t ALTER COLUMN c ADD MASKED WITH (FUNCTION = 'default()')");

		Assert.Equal(expected, TransactSqlParser.ParseStatement(text));
		Assert.Equal(expected, TransactSqlParser.Located.ParseStatement(text));
		Assert.Equal(
			"'default()'",
			Assert.IsType<Ast.LiteralValue.String>(Assert.IsType<Ast.AlterColumnAction.Property>(
				Assert.IsType<Ast.AlterTableAction.AlterColumn>(Assert.Single(((Ast.Statement.AlterTable)expected).Actions)).Action).MaskFunction).Text);
	}

	/// <summary>
	/// A quote inside a block comment in front of a string the statement's text is read again for leaves the string as
	/// written: its two spaces stay two, as without the comment.
	/// </summary>
	[Theory]
	[InlineData("CREATE DEFAULT d AS /* ' */ 'a  b'", "CREATE DEFAULT d AS 'a  b'")]
	[InlineData("CREATE STATISTICS s ON t (a) WHERE a = /* ' */ 'a  b'", "CREATE STATISTICS s ON t (a) WHERE a = 'a  b'")]
	public void A_quote_in_a_comment_leaves_the_string_after_it_as_written(string text, string plain)
	{
		var expected = TransactSqlParser.ParseStatement(plain);

		Assert.Equal(expected, TransactSqlParser.ParseStatement(text));
		Assert.Equal(expected, TransactSqlParser.Located.ParseStatement(text));
		Assert.Contains(
			"'a  b'",
			Ast.TransactSqlWriter.Write(TransactSqlParser.ParseStatement(text)),
			StringComparison.Ordinal);
	}

	/// <summary>Every batch of the corpus, through the script entry of both doors.</summary>
	[Fact]
	public void No_batch_of_the_corpus_throws()
	{
		var batches = 0;

		foreach (var file in Directory.GetFiles(Corpus(), "*.sql", SearchOption.AllDirectories).OrderBy(static one => one, StringComparer.Ordinal))
			foreach (var batch in SqlScript.Read(File.ReadAllText(file)).Batches)
			{
				batches++;

				var plain   = Record.Exception(() => TransactSqlParser.TryParseSql(batch));
				var located = Record.Exception(() => TransactSqlParser.Located.TryParseSql(batch));

				if ((plain ?? located) is { } thrown)
					Assert.Fail($"{file}: {thrown.GetType().Name}: {thrown.Message}");
			}

		Assert.True(batches > 2_000, $"{batches} batches");
	}

	static string Corpus([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Corpus", "ScriptDom"));
	}

	static IEnumerable<(string Entry, Action<string> Read)> Entries()
	{
		yield return ("TryParseSql", static text => TransactSqlParser.TryParseSql(text));
		yield return ("TryParseStatement", static text => TransactSqlParser.TryParseStatement(text));
		yield return ("TryParseSelect", static text => TransactSqlParser.TryParseSelect(text));
		yield return ("TryParseExpression", static text => TransactSqlParser.TryParseExpression(text));
		yield return ("TryParseSearchCondition", static text => TransactSqlParser.TryParseSearchCondition(text));
		yield return ("TryParseScript", static text => TransactSqlParser.TryParseScript(text));
		yield return ("Located.TryParseSql", static text => TransactSqlParser.Located.TryParseSql(text));
		yield return ("Located.TryParseStatement", static text => TransactSqlParser.Located.TryParseStatement(text));
		yield return ("Located.TryParseSelect", static text => TransactSqlParser.Located.TryParseSelect(text));
		yield return ("Located.TryParseExpression", static text => TransactSqlParser.Located.TryParseExpression(text));
		yield return ("Located.TryParseSearchCondition", static text => TransactSqlParser.Located.TryParseSearchCondition(text));
		yield return ("Located.TryParseScript", static text => TransactSqlParser.Located.TryParseScript(text));
	}
}
