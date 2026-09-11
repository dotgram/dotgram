using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using DotGram.Parsers.Sql;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// T-SQL as a dialect of SQL-92, read.
/// </summary>
/// <remarks>
/// <para>
/// What is under test is not that T-SQL reads. <see cref="SqlStandard92Tests"/> already asks
/// that of the rules these ones stand in for, and asking it again of a dialect would say
/// nothing new. What is under test is that <em>a dialect is five rules and a header</em>:
/// <c>TransactSql.gram</c> replaces an identifier, a value specification, a value primary, a
/// query specification and a table primary, and §5.1's rebinding is what carries the
/// replacement into every rule that leads to one.
/// </para>
/// <para>
/// So the theories come in pairs. What T-SQL adds reads here and is refused by the class
/// this one derives from — which is what says the dialect did not leak into its base. And it
/// reads in positions the header never names — a bracketed name inside a join condition, a
/// variable inside a <c>HAVING</c> — which is the only way to tell a substitution that
/// reached from one written out at the top that stopped there.
/// </para>
/// </remarks>
public sealed class TransactSqlTests
{
	// ── The five seams, each read through the rule that replaced it ──────────────

	/// <summary>
	/// What T-SQL has and the standard does not: it reads here, and there it does not.
	/// </summary>
	/// <remarks>
	/// Both halves are the claim. The first says the replacement happened; the second says
	/// it happened <em>here</em> — a base and a derived class are two whole parsers, and the
	/// base still reads the language it always read.
	/// </remarks>
	[Theory]

	// §5.2's identifier, widened: brackets, `]]` for a bracket inside, and a temporary name.
	[InlineData("SELECT [a] FROM t")]
	[InlineData("SELECT [a]]b] FROM t")]
	[InlineData("SELECT [select] FROM [group]")]
	[InlineData("SELECT a FROM #tmp")]
	[InlineData("SELECT a FROM ##shared")]

	// §6.3's general value specification, widened: a variable stands where a parameter does.
	[InlineData("SELECT @v")]
	[InlineData("SELECT @@ROWCOUNT")]
	[InlineData("SELECT a FROM t WHERE b = @v")]

	// §6.11's value primary, widened: a call on a name this grammar never heard of, and
	// the window a call may be computed over.
	[InlineData("SELECT dbo.f(1, a) FROM t")]
	[InlineData("SELECT ISNULL(a, 0) FROM t")]
	[InlineData("SELECT COUNT(*) OVER () FROM t")]
	[InlineData("SELECT SUM(a) OVER (PARTITION BY b ORDER BY c) FROM t")]

	// §7.9's query specification, widened: `FROM` is optional and `TOP` stands before the
	// select list.
	[InlineData("SELECT 1")]
	[InlineData("SELECT TOP 10 a FROM t")]
	[InlineData("SELECT TOP (10) PERCENT a FROM t")]
	[InlineData("SELECT TOP 5 WITH TIES a FROM t ORDER BY a")]
	[InlineData("SELECT DISTINCT TOP 3 a FROM t")]

	// §7.4's table primary, widened: a table-valued function and a table variable.
	[InlineData("SELECT a FROM dbo.f(1) AS x")]
	[InlineData("SELECT a FROM @rows")]
	[InlineData("SELECT a FROM @rows AS r")]
	public void What_the_dialect_adds_reads_here_and_not_in_the_standard(string input)
	{
		Assert.True(TransactSql.TryParseSelect(input).IsSuccess, input);
		Assert.False(SqlStandard92.TryParseSelect(input).IsSuccess, input);
	}

	// ── That the substitution reached ────────────────────────────────────────────

	/// <summary>And it reads where the header never names it.</summary>
	/// <remarks>
	/// <c>namespace Dialect with (...)</c> names five rules. None of these positions is one
	/// of them: a join condition, a subquery's select list, a <c>CASE</c> arm, a
	/// <c>GROUP BY</c> key, a sort key, a set operand. Each is some other rule of the
	/// standard that leads to a replaced one, and reading here is the whole of what §5.1
	/// claims to do.
	/// </remarks>
	[Theory]
	[InlineData("SELECT a FROM t JOIN u ON t.[id] = u.[id]")]
	[InlineData("SELECT a FROM t WHERE b IN (SELECT [c] FROM u)")]
	[InlineData("SELECT CASE WHEN [a] > 1 THEN @v ELSE dbo.f([b]) END FROM t")]
	[InlineData("SELECT a FROM t GROUP BY [b] HAVING COUNT(*) > @n")]
	[InlineData("SELECT a FROM t ORDER BY [b] DESC")]
	[InlineData("SELECT a FROM t UNION SELECT [b] FROM u")]
	[InlineData("SELECT a FROM (SELECT TOP 1 [b] FROM u) AS d")]
	[InlineData("SELECT a FROM t WHERE b > (SELECT MAX([c]) FROM #tmp)")]
	[InlineData("SELECT a FROM t WHERE b BETWEEN @lo AND @hi")]
	[InlineData("SELECT a FROM t WHERE CAST([b] AS INTEGER) = dbo.f(@v)")]
	[InlineData("SELECT a FROM t JOIN dbo.f(@v) AS x ON t.id = x.id")]
	[InlineData("SELECT a FROM t WHERE EXISTS (SELECT 1 FROM u WHERE u.[id] = t.[id])")]
	public void The_replacement_reaches_where_it_is_never_named(string input) =>
		Assert.True(TransactSql.TryParseSelect(input).IsSuccess, input);

	// ── And that nothing was traded for it ───────────────────────────────────────

	/// <summary>What the standard reads, the dialect reads too.</summary>
	/// <remarks>
	/// A replacement restates the alternatives of the rule it replaces rather than calling
	/// it — a rule that named the rule it replaces would be rewritten into itself — so what
	/// the standard said is said again by hand, and this is what asks whether all of it was.
	/// </remarks>
	[Theory]
	[InlineData("SELECT * FROM t")]
	[InlineData("SELECT DISTINCT a, b FROM t WHERE a > 1")]
	[InlineData("SELECT t.* FROM t")]
	[InlineData("SELECT a AS x, b y FROM t")]
	[InlineData("SELECT a FROM t, u")]
	[InlineData("SELECT a FROM t LEFT OUTER JOIN u ON t.id = u.id")]
	[InlineData("SELECT a FROM t CROSS JOIN u")]
	[InlineData("SELECT a FROM (SELECT b FROM u) AS d (c)")]
	[InlineData("SELECT COUNT(*) FROM t GROUP BY a, b HAVING COUNT(*) > 1")]
	[InlineData("SELECT (SELECT MAX(b) FROM u) FROM t")]
	[InlineData("SELECT a FROM t UNION ALL SELECT b FROM u EXCEPT SELECT c FROM v")]
	[InlineData("SELECT a FROM t ORDER BY a DESC, 2 ASC")]
	[InlineData("SELECT \"select\" FROM t")]
	[InlineData("SELECT a COLLATE SQL_Latin1_General_CP1_CI_AS FROM t")]
	[InlineData("VALUES (1, 2), (3, 4)")]
	[InlineData("TABLE t")]
	[InlineData("(SELECT a FROM t)")]
	public void What_the_standard_reads_the_dialect_reads_too(string input)
	{
		Assert.True(SqlStandard92.TryParseSelect(input).IsSuccess, input);
		Assert.True(TransactSql  .TryParseSelect(input).IsSuccess, input);
	}

	// ── The value layer ─────────────────────────────────────────────────────────

	/// <summary>
	/// What T-SQL puts where a value goes, and the standard does not.
	/// </summary>
	/// <remarks>
	/// Every one of these came off the list <c>--kinds</c> makes: of the statements
	/// Microsoft's parser calls a query, what stood where this one stopped. So the theory is
	/// the work list, and a row of it that stops failing is a row that stops appearing.
	/// </remarks>
	[Theory]

	// The types a cast may name, and `MAX` where a length stands.
	[InlineData("SELECT CAST (a AS VARCHAR (MAX)) FROM t")]
	[InlineData("SELECT CAST (a AS NVARCHAR (300)) FROM t")]
	[InlineData("SELECT CAST (a AS VARBINARY (20)) FROM t")]
	[InlineData("SELECT CAST (a AS DATETIME2) FROM t")]
	[InlineData("SELECT CAST (a AS JSON) FROM t")]
	[InlineData("SELECT CAST (a AS VECTOR (3)) FROM t")]
	[InlineData("SELECT CAST (a AS MONEY) FROM t")]
	[InlineData("SELECT CAST (a AS dbo.MyType) FROM t")]

	// And the casts that are casts under another name.
	[InlineData("SELECT TRY_CAST ('12345' AS INT)")]
	[InlineData("SELECT CONVERT (INT, a) FROM t")]
	[InlineData("SELECT CONVERT (VARCHAR (10), a, 121) FROM t")]
	[InlineData("SELECT TRY_CONVERT (INT, a) FROM t")]

	// The literals.
	[InlineData("SELECT $492050157978986.2129")]
	[InlineData("SELECT 0xabcdef")]
	[InlineData("SELECT SET_BIT (0x00, 2) AS VARBIN1")]

	// A condition where a value stands, which is what `IIF` needs and a value cannot be.
	[InlineData("SELECT IIF (3 > 4, 'A', 'B')")]
	[InlineData("SELECT IIF (NOT a > 1, 1, 0) FROM t")]
	[InlineData("SELECT CHOOSE (2, 'a', 'b', 'c')")]

	// A truth where a condition stands, which is the same trade on the other side.
	[InlineData("SELECT a FROM t WHERE CONTAINS (b, 'foo')")]
	[InlineData("SELECT a FROM t WHERE FREETEXT ((b), 'vital safety')")]
	[InlineData("SELECT 1 WHERE REGEXP_LIKE ('abc', '^a')")]
	[InlineData("SELECT CASE WHEN REGEXP_LIKE ('abc', '^a') THEN 1 ELSE 0 END")]
	[InlineData("SELECT a FROM t WHERE b IS NOT DISTINCT FROM 1")]
	[InlineData("SELECT a FROM t WHERE b IS DISTINCT FROM 1")]

	// A member of whatever was just read.
	[InlineData("SELECT (c1).SomeProperty FROM t")]
	[InlineData("SELECT t::a FROM u")]
	[InlineData("SELECT dbo.[type 1]::[Property] FROM t")]
	[InlineData("SELECT c1.f1().SomeProperty FROM t")]

	// A time zone, and a sequence.
	[InlineData("SELECT a AT TIME ZONE 'UTC' FROM t")]
	[InlineData("SELECT CAST ('1212-12-12 12:12:12' AS DATETIME2) AT TIME ZONE @tz")]
	[InlineData("SELECT NEXT VALUE FOR seq1")]
	[InlineData("SELECT NEXT VALUE FOR seq1 OVER (ORDER BY a) FROM t")]

	// And everything a window may say.
	[InlineData("SELECT COUNT (a) OVER (ORDER BY b) FROM t")]
	[InlineData("SELECT COUNT (a) OVER (ROWS UNBOUNDED PRECEDING) FROM t")]
	[InlineData("SELECT COUNT (a) OVER (ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) FROM t")]
	[InlineData("SELECT COUNT (a) OVER (ROWS 1 PRECEDING) FROM t")]
	[InlineData("SELECT COUNT (a) OVER (RANGE CURRENT ROW) FROM t")]
	[InlineData("SELECT FIRST_VALUE (a) OVER () FROM t")]
	[InlineData("SELECT FIRST_VALUE (a) RESPECT NULLS FROM t")]
	[InlineData("SELECT FIRST_VALUE (a) RESPECT NULLS OVER Win1 FROM t")]
	[InlineData("SELECT FIRST_VALUE (a) IGNORE NULLS OVER () FROM t")]
	[InlineData("SELECT FIRST_VALUE (a) WITHIN GROUP (ORDER BY a) OVER () FROM t")]
	[InlineData("SELECT FIRST_VALUE (a) RESPECT NULLS OVER () FROM t")]
	[InlineData("SELECT COUNT (a) OVER (ORDER BY b ROWS 1 PRECEDING) FROM t")]
	[InlineData("SELECT COUNT (a) OVER (ORDER BY b ROWS BETWEEN 1 FOLLOWING AND 1 FOLLOWING) FROM t")]
	[InlineData("SELECT COUNT (a) OVER (ORDER BY b RANGE BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) FROM t")]
	[InlineData("SELECT SUM (a) OVER Win1 FROM t")]
	[InlineData("SELECT STRING_AGG (a, ',') WITHIN GROUP (ORDER BY a) FROM t")]
	public void The_value_layer_reads(string input)
	{
		var match = TransactSql.TryParseSelect(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	// ── The query layer ─────────────────────────────────────────────────────────

	/// <summary>What T-SQL puts around a query, and the standard does not.</summary>
	/// <remarks>
	/// The clauses that belong to the statement rather than to the query — how many rows to
	/// skip, what shape to return them in, how to run it — and the three places inside one
	/// where T-SQL says more than §7 does: a select list that may assign, a group by that
	/// groups by sets, and a from clause that may apply, hint and pivot.
	/// </remarks>
	[Theory]

	// What the query returns, and where it puts it.
	[InlineData("SELECT @a += 1")]
	[InlineData("SELECT @a = b FROM t")]
	[InlineData("SELECT alias = b FROM t")]
	[InlineData("SELECT b AS 'Original string' FROM t")]
	[InlineData("SELECT c1 INTO t2 FROM t1")]
	[InlineData("SELECT c1 INTO t2 ON fg FROM t1")]
	[InlineData("SELECT TOP 10 WITH APPROXIMATE a FROM t ORDER BY a")]

	// The windows a select list may name rather than write out.
	[InlineData("SELECT SUM (c1) OVER Win1 FROM t1 WINDOW Win1 AS (PARTITION BY c1)")]

	// Grouping by sets of columns rather than by columns.
	[InlineData("SELECT a FROM t GROUP BY CUBE (a)")]
	[InlineData("SELECT a FROM t GROUP BY ROLLUP (a, b)")]
	[InlineData("SELECT a FROM t GROUP BY GROUPING SETS ((CUBE (a), ROLLUP (b), c), (a), ())")]
	[InlineData("SELECT a FROM t GROUP BY ALL ()")]

	// Applying, hinting, sampling and pivoting a source.
	[InlineData("SELECT * FROM t1 CROSS APPLY (SELECT * FROM u) AS x")]
	[InlineData("SELECT * FROM t1 OUTER APPLY dbo.f (t1.a) AS x")]
	[InlineData("SELECT * FROM t1 INNER HASH JOIN t10 ON t1.c1 = t10.c1")]
	[InlineData("SELECT * FROM t1 INNER REMOTE JOIN t10 ON t1.c1 = t10.c1")]
	[InlineData("SELECT c1 FROM t1 AS table1 WITH (INDEX (0, 1, ind2), HOLDLOCK, NOLOCK)")]
	[InlineData("SELECT * FROM t WITH (FORCESEEK (i134 (c1, c3, c4)))")]
	[InlineData("SELECT TOP (5) * FROM r WITH (SPATIAL_WINDOW_MAX_CELLS = 512)")]
	[InlineData("SELECT * FROM t1 TABLESAMPLE (12 ROWS)")]
	[InlineData("SELECT * FROM fun2 TABLESAMPLE SYSTEM (12 PERCENT) REPEATABLE (100)")]
	[InlineData("SELECT VendorID, [164] AS Emp1 FROM t PIVOT (SUM (a) FOR b IN ([164])) AS pvt")]
	[InlineData("SELECT a, b FROM (SELECT * FROM t) AS d UNPIVOT (v FOR k IN (x, y)) AS u")]

	// How many rows, what shape, and how to run it.
	[InlineData("SELECT * FROM T ORDER BY a OFFSET 5 ROWS FETCH NEXT 2 ROWS ONLY")]
	[InlineData("SELECT a FROM t ORDER BY a OFFSET 5 ROWS")]
	[InlineData("SELECT * FROM t1 FOR JSON AUTO")]
	[InlineData("SELECT * FROM t1 FOR JSON PATH, ROOT ('r')")]
	[InlineData("SELECT * FROM t1 FOR XML AUTO, ELEMENTS")]
	[InlineData("SELECT * FROM t1 FOR BROWSE")]
	[InlineData("SELECT a FROM t OPTION (RECOMPILE)")]
	[InlineData("SELECT * FROM t1 OPTION (MERGE UNION, HASH JOIN)")]
	[InlineData("SELECT * FROM t1 OPTION (HASH GROUP, CONCAT UNION, LOOP JOIN, FAST 10, FORCE ORDER)")]
	[InlineData("SELECT * FROM t1 OPTION (MAX_GRANT_PERCENT = 50)")]
	[InlineData("SELECT * FROM t1 OPTION (OPTIMIZE FOR (@v1 = 20, @v2 = UNKNOWN))")]
	[InlineData("SELECT * FROM t1 OPTION (USE PLAN N'zzz')")]
	[InlineData("SELECT * FROM t1 OPTION (USE HINT ('DISABLE_OPTIMIZED_NESTED_LOOP'))")]
	[InlineData("SELECT * FROM t WHERE c1 = 3 OPTION (TABLE HINT (t, FORCESCAN))")]
	[InlineData("SELECT * FROM t1 OPTION (RECOMPILE, EXPAND VIEWS)")]
	public void The_query_layer_reads(string input)
	{
		var match = TransactSql.TryParseSelect(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>And what the standard reads, the dialect is right to refuse.</summary>
	/// <remarks>
	/// <para>
	/// A dialect narrows as well as widens, and nothing here was measuring that: a refusal
	/// count sees only what is not read, never what is read and should not be. These came
	/// out of writing the `FROM` clause from the published syntax rather than from the
	/// corpus, and every one was checked against SQL Server 2025 itself — `SET PARSEONLY
	/// ON` and the statement, which asks the engine the one question this is about.
	/// </para>
	/// <para>
	/// `NATURAL JOIN` and `USING` are SQL-92 and are not T-SQL at all; the engine reads the
	/// first as a correlation name and refuses what follows, and the second as an old-style
	/// table hint list. A sample stands before the hints and not after. And a bare
	/// parenthesised list after a table name is a hint list, not a column alias list —
	/// which is why the standard's spelling of a correlation may not be written here.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("SELECT a FROM t NATURAL FULL JOIN u")]
	[InlineData("SELECT a FROM t INNER JOIN u USING (id, k)")]
	[InlineData("SELECT a FROM t EXCEPT ALL SELECT b FROM u")]
	[InlineData("SELECT a FROM t INTERSECT ALL SELECT b FROM u")]
	[InlineData("SELECT a FROM t UNION CORRESPONDING SELECT b FROM u")]
	[InlineData("SELECT a FROM t JOIN u USING (id)")]
	public void What_is_the_standard_and_not_the_dialect_is_refused(string input)
	{
		Assert.True (SqlStandard92.TryParseSelect(input).IsSuccess, input);
		Assert.False(TransactSql  .TryParseSelect(input).IsSuccess, input);
	}

	/// <summary>What the engine refuses, this refuses too.</summary>
	/// <remarks>
	/// <para>
	/// Each of these was written into the grammar from the corpus or from a guess about the
	/// order of two clauses, and each was taken back out after SQL Server 2025 was asked —
	/// `SET PARSEONLY ON` and the statement, which is the one question a syntax has an
	/// answer to and needs no schema to answer.
	/// </para>
	/// <para>
	/// The first is a finding rather than a correction: <c>INNER LOCAL MERGE JOIN</c> is in
	/// ScriptDom's own test corpus, and the engine answers
	/// <c>'LOCAL' is not a recognized join option</c>. The other parser reads something the
	/// server does not, which is what comparing three sources is for and what comparing two
	/// could never have shown.
	/// </para>
	/// <para>
	/// One the engine refuses and this does not, deliberately: <c>FROM t1 AS a (c2)</c> is
	/// answered with <c>"c2" is not a recognized table hints option</c>, which settles that
	/// the parentheses are the old spelling of a hint list and not a column alias list — but
	/// refusing it is a check against the list of hint names, and this grammar reads a hint
	/// name as an identifier on purpose (see <c>TableHint</c>). So the shape is right here
	/// and the vocabulary is not checked, which is a looseness and not a defect.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("SELECT * FROM t1 INNER LOCAL MERGE JOIN t10 ON t1.c1 = t10.c1")]
	[InlineData("SELECT JSON_OBJECT (NULL ON NULL)")]
	[InlineData("SELECT JSON_ARRAY (NULL ON NULL)")]
	[InlineData("SELECT a FROM t ORDER BY a FETCH NEXT 2 ROWS ONLY")]
	[InlineData("SELECT trim(*) FROM t")]
	[InlineData("SELECT c1 FROM t1 AS a WITH (NOLOCK) TABLESAMPLE (10 PERCENT)")]
	[InlineData("SELECT SUM (c) OVER (w) FROM t WINDOW w AS (PARTITION BY c)")]
	public void What_the_engine_refuses_this_refuses_too(string input) =>
		Assert.False(TransactSql.TryParseSelect(input).IsSuccess, input);

	/// <summary>Statements ScriptDom reads and SQL Server refuses, refused here as well.</summary>
	/// <remarks>
	/// `HIDDEN` on any column, where the published syntax has it on a period column only; a
	/// column generated from `SUSER_SID`, which is in ScriptDom's grammar and tests and in no
	/// product found; and `FOR SECONDARY` in front of the one action that is not a `SET`.
	/// </remarks>
	[Theory]
	[InlineData("CREATE TABLE t (a INT HIDDEN)")]
	[InlineData("ALTER TABLE t1 ALTER COLUMN c1 INT HIDDEN NULL")]
	[InlineData("ALTER TABLE t1 ALTER COLUMN c1 VARBINARY (85) GENERATED ALWAYS AS SUSER_SID START")]
	[InlineData("CREATE TABLE t (u NVARCHAR (128) GENERATED ALWAYS AS SUSER_SNAME END NOT NULL)")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY CLEAR PROCEDURE_CACHE")]
	[InlineData("WITH a AS (SELECT 1 AS x) WITH b AS (SELECT 2 AS y) SELECT * FROM a, b")]
	[InlineData("WITH a AS (SELECT 1 AS x) WITH b AS (SELECT 2 AS y) UPDATE t SET c = 1")]
	public void Statements_the_engine_refuses_are_refused(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And the forms beside them that it reads.</summary>
	[Theory]
	[InlineData("CREATE TABLE t (a INT, s DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL, e DATETIME2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL, PERIOD FOR SYSTEM_TIME (s, e))")]
	[InlineData("CREATE TABLE t (a INT, b BIGINT GENERATED ALWAYS AS TRANSACTION_ID START HIDDEN NOT NULL)")]
	[InlineData("CREATE TABLE t (a INT, b BIGINT GENERATED ALWAYS AS SEQUENCE_NUMBER END NOT NULL)")]
	[InlineData("ALTER TABLE t ALTER COLUMN s ADD HIDDEN")]
	[InlineData("ALTER TABLE t ALTER COLUMN s DROP HIDDEN")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION CLEAR PROCEDURE_CACHE")]
	public void Statements_the_engine_reads_are_read(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The keys' catalogue: what the engine refuses in one, refused here.</summary>
	/// <remarks>
	/// Each written from the published block and then put to SQL Server 2025, which settled
	/// what the blocks leave open. A name the catalogue does not have is refused; a password is
	/// a literal; a symmetric key's encryptions follow its options without a comma; the two
	/// options that belong to a provider are refused without one; a database encryption key
	/// needs both its algorithm and its encryptor, and altering one does one of the two.
	/// </remarks>
	[Theory]
	[InlineData("CREATE SYMMETRIC KEY k WITH ALGORITHM = AES_257 ENCRYPTION BY CERTIFICATE c")]
	[InlineData("CREATE SYMMETRIC KEY k WITH FOO = 1 ENCRYPTION BY CERTIFICATE c")]
	[InlineData("CREATE SYMMETRIC KEY k WITH ALGORITHM = AES_256 ENCRYPTION BY PASSWORD = @p")]
	[InlineData("CREATE SYMMETRIC KEY k WITH ALGORITHM = AES_256, ENCRYPTION BY CERTIFICATE c")]
	[InlineData("CREATE SYMMETRIC KEY k WITH ALGORITHM = AES_256")]
	[InlineData("CREATE SYMMETRIC KEY k WITH ENCRYPTION BY CERTIFICATE c")]
	[InlineData("CREATE SYMMETRIC KEY k WITH ALGORITHM = AES_256 ENCRYPTION BY SERVER CERTIFICATE c")]
	[InlineData("CREATE ASYMMETRIC KEY k WITH ALGORITHM = RSA_2048, PROVIDER_KEY_NAME = 'x'")]
	[InlineData("CREATE ASYMMETRIC KEY k WITH ALGORITHM = RSA_2048 ENCRYPTION BY CERTIFICATE c")]
	[InlineData("CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = AES_256")]
	[InlineData("CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = AES_256 ENCRYPTION BY CERTIFICATE c")]
	[InlineData("ALTER DATABASE ENCRYPTION KEY REGENERATE WITH ALGORITHM = AES_256 ENCRYPTION BY SERVER CERTIFICATE c")]
	[InlineData("CREATE CERTIFICATE c WITH SUBJECT = 's', FOO = 'x'")]
	[InlineData("CREATE CERTIFICATE c FROM FILE = 'f' WITH PRIVATE KEY (FILE = 'k', FOO = 'p')")]
	[InlineData("CREATE COLUMN MASTER KEY k WITH (KEY_STORE_PROVIDER_NAME = N'p')")]
	[InlineData("CREATE COLUMN MASTER KEY k WITH (KEY_STORE_PROVIDER_NAME = N'p', KEY_PATH = N'x' ENCLAVE_COMPUTATIONS (SIGNATURE = 0x01))")]
	[InlineData("CREATE COLUMN ENCRYPTION KEY k WITH VALUES (COLUMN_MASTER_KEY = m, ALGORITHM = RSA_OAEP, ENCRYPTED_VALUE = 0x01)")]
	[InlineData("ALTER COLUMN ENCRYPTION KEY k DROP VALUE (COLUMN_MASTER_KEY = m, ALGORITHM = 'RSA_OAEP', ENCRYPTED_VALUE = 0x01)")]
	[InlineData("CREATE CREDENTIAL c WITH SECRET = 's', IDENTITY = 'i'")]
	[InlineData("CREATE ASYMMETRIC KEY k WITH ALGORITHM = FOO")]
	[InlineData("CREATE ASYMMETRIC KEY k FROM FILE = 'f' WITH ALGORITHM = RSA_2048")]
	[InlineData("CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = DES ENCRYPTION BY SERVER CERTIFICATE c")]
	[InlineData("ALTER DATABASE ENCRYPTION KEY REGENERATE WITH ALGORITHM = RSA_2048")]
	public void The_keys_catalogue_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("CREATE SYMMETRIC KEY k WITH IDENTITY_VALUE = 'x', ALGORITHM = AES_256, KEY_SOURCE = N'y' ENCRYPTION BY PASSWORD = 'p', CERTIFICATE c")]
	[InlineData("CREATE SYMMETRIC KEY #k WITH ALGORITHM = AES_256 ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("CREATE SYMMETRIC KEY k WITH ALGORITHM = AES_256 ENCRYPTION BY CERTIFICATE c, PASSWORD = 'p'")]
	[InlineData("CREATE SYMMETRIC KEY k FROM PROVIDER p WITH PROVIDER_KEY_NAME = 'k', CREATION_DISPOSITION = OPEN_EXISTING")]
	[InlineData("CREATE ASYMMETRIC KEY k FROM PROVIDER p WITH ALGORITHM = RSA_2048, PROVIDER_KEY_NAME = 'x', CREATION_DISPOSITION = CREATE_NEW")]
	[InlineData("CREATE ASYMMETRIC KEY k AUTHORIZATION u FROM EXECUTABLE FILE = 'f' ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = AES_128 ENCRYPTION BY SERVER ASYMMETRIC KEY k")]
	[InlineData("ALTER DATABASE ENCRYPTION KEY ENCRYPTION BY SERVER CERTIFICATE c")]
	[InlineData("CREATE CERTIFICATE c ENCRYPTION BY PASSWORD = 'p' WITH START_DATE = '20200101', SUBJECT = 's'")]
	[InlineData("CREATE CERTIFICATE c FROM FILE = 'f' WITH FORMAT = 'PFX', PRIVATE KEY (DECRYPTION BY PASSWORD = 'p', FILE = 'k')")]
	[InlineData("CREATE CERTIFICATE c FROM BINARY = 0x01 WITH PRIVATE KEY (BINARY = 0x02, DECRYPTION BY PASSWORD = 'p')")]
	[InlineData("ALTER CERTIFICATE c WITH PRIVATE KEY (ENCRYPTION BY PASSWORD = 'p', DECRYPTION BY PASSWORD = 'q')")]
	[InlineData("ALTER MASTER KEY FORCE REGENERATE WITH ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("ALTER MASTER KEY DROP ENCRYPTION BY SERVICE MASTER KEY")]
	[InlineData("CREATE COLUMN MASTER KEY k WITH (KEY_PATH = N'x', KEY_STORE_PROVIDER_NAME = N'p', ENCLAVE_COMPUTATIONS (SIGNATURE = 0x01))")]
	[InlineData("CREATE COLUMN ENCRYPTION KEY k WITH VALUES (ALGORITHM = 'RSA_OAEP', COLUMN_MASTER_KEY = m, ENCRYPTED_VALUE = 0x01), (COLUMN_MASTER_KEY = n, ALGORITHM = N'RSA_OAEP', ENCRYPTED_VALUE = 0x02)")]
	[InlineData("ALTER COLUMN ENCRYPTION KEY k DROP VALUE (COLUMN_MASTER_KEY = m)")]
	[InlineData("CREATE CREDENTIAL c WITH IDENTITY = 'i', SECRET = 's' FOR CRYPTOGRAPHIC PROVIDER p")]
	[InlineData("ALTER DATABASE SCOPED CREDENTIAL c WITH IDENTITY = 'i'")]

	// One list of algorithms for both kinds of key, as the engine reads them, and every one
	// of them at every level where a provider makes the key.
	[InlineData("CREATE ASYMMETRIC KEY k WITH ALGORITHM = AES_128")]
	[InlineData("CREATE SYMMETRIC KEY k WITH ALGORITHM = RSA_2048 ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("CREATE ASYMMETRIC KEY k FROM PROVIDER p WITH ALGORITHM = RC2, PROVIDER_KEY_NAME = 'x'")]
	[InlineData("create symmetric key k1 from provider p1 with provider_key_name = 'key1', algorithm = rc4, CREATION_DISPOSITION = OPEN_EXISTING")]
	[InlineData("CREATE SYMMETRIC KEY k FROM PROVIDER p")]
	[InlineData("CREATE SYMMETRIC KEY k FROM PROVIDER p ENCRYPTION BY CERTIFICATE c")]
	[InlineData("ALTER ASYMMETRIC KEY a1 ATTESTED BY 'zzz'")]
	[InlineData("ALTER CERTIFICATE c1 REMOVE ATTESTED OPTION")]
	public void The_keys_catalogue_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The index options' catalogue: what the engine refuses in one, refused here.</summary>
	/// <remarks>
	/// One list for each place an index is built or rebuilt, and the lists differ: an option
	/// one of them has and another has not is refused in the other, as the engine refuses it.
	/// A value is a literal, the old unbracketed list has its six and no more, and an option
	/// in brackets ends at a comma or at the bracket.
	/// </remarks>
	[Theory]
	[InlineData("CREATE INDEX i ON t (a) WITH (FILLFACTOR = @f)")]
	[InlineData("CREATE INDEX i ON t (a) WITH (FOO = ON)")]
	[InlineData("CREATE INDEX i ON t (a) WITH (LOB_COMPACTION = ON)")]
	[InlineData("CREATE INDEX i ON t (a) WITH (COMPRESSION_DELAY = 10 MINUTES)")]
	[InlineData("CREATE INDEX i ON t (a) WITH (DATA_COMPRESSION = COLUMNSTORE)")]
	[InlineData("CREATE INDEX i ON t (a) WITH ALLOW_ROW_LOCKS = ON")]
	[InlineData("CREATE INDEX i ON t (a) WITH (PAD_INDEX)")]
	[InlineData("CREATE INDEX i ON t (a) WITH (ONLINE = ON (MAXDOP = 2))")]
	[InlineData("CREATE INDEX i ON t (a) WITH (ONLINE = OFF (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1, ABORT_AFTER_WAIT = NONE)))")]
	[InlineData("CREATE INDEX i ON t (a) WITH (IGNORE_DUP_KEY = OFF (SUPPRESS_MESSAGES = ON))")]
	[InlineData("CREATE NONCLUSTERED COLUMNSTORE INDEX i ON t (a) WITH (FILLFACTOR = 80)")]
	[InlineData("CREATE PRIMARY XML INDEX x ON t (c) WITH (ONLINE = ON)")]
	[InlineData("CREATE PRIMARY XML INDEX x ON t (c) WITH (DATA_COMPRESSION = PAGE)")]
	[InlineData("ALTER INDEX i ON t REBUILD WITH (OPTIMIZE_FOR_SEQUENTIAL_KEY = ON)")]
	[InlineData("ALTER INDEX i ON t REBUILD WITH (DROP_EXISTING = ON)")]
	[InlineData("ALTER INDEX i ON t REBUILD PARTITION = 2 WITH (FILLFACTOR = 80)")]
	[InlineData("ALTER INDEX i ON t REORGANIZE WITH (FILLFACTOR = 80)")]
	[InlineData("ALTER INDEX i ON t SET (FILLFACTOR = 80)")]
	[InlineData("ALTER INDEX i ON t RESUME WITH (ONLINE = ON)")]
	[InlineData("CREATE TABLE t (a INT, CONSTRAINT pk PRIMARY KEY (a) WITH (ONLINE = ON))")]
	[InlineData("CREATE TABLE t (a INT PRIMARY KEY WITH PAD_INDEX)")]
	[InlineData("CREATE TABLE t (a INT, INDEX ix (a) WITH (DROP_EXISTING = ON))")]
	[InlineData("CREATE TABLE t (a INT, INDEX ix (a) WITH FILLFACTOR = 80)")]
	[InlineData("ALTER TABLE t ADD CONSTRAINT pk PRIMARY KEY (a) WITH (DROP_EXISTING = ON)")]
	[InlineData("CREATE STATISTICS s ON t (a) WITH FULLSCAN NORECOMPUTE")]
	[InlineData("CREATE STATISTICS s ON t (a) WITH RESAMPLE")]
	[InlineData("CREATE STATISTICS s ON t (a) WITH ROWCOUNT = 10")]
	[InlineData("CREATE INDEX i ON t (a) WITH (ONLINE = ON, RESUMABLE = ON, MAX_DURATION = 5 MINUTE)")]
	[InlineData("UPDATE STATISTICS t WITH FOO")]
	[InlineData("UPDATE STATISTICS t WITH SAMPLE @n PERCENT")]
	public void The_index_catalogue_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("CREATE INDEX i ON t (a) WITH (ONLINE = ON (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 5 MINUTES, ABORT_AFTER_WAIT = SELF)), RESUMABLE = ON)")]
	[InlineData("CREATE INDEX i ON t (a) WITH (IGNORE_DUP_KEY = ON (SUPPRESS_MESSAGES = ON), OPTIMIZE_FOR_SEQUENTIAL_KEY = ON)")]
	[InlineData("CREATE INDEX i ON t (a) WITH (DATA_COMPRESSION = PAGE ON PARTITIONS (1 TO 3), XML_COMPRESSION = OFF ON PARTITIONS (2))")]
	[InlineData("CREATE INDEX i ON t (a) WITH PAD_INDEX, FILLFACTOR = 80, SORT_IN_TEMPDB, IGNORE_DUP_KEY, STATISTICS_NORECOMPUTE, DROP_EXISTING")]
	[InlineData("CREATE INDEX i ON t (a) WITH PAD_INDEX ON [PRIMARY]")]
	[InlineData("CREATE CLUSTERED COLUMNSTORE INDEX i ON t WITH (DROP_EXISTING = ON, MAXDOP = 2, COMPRESSION_DELAY = 10, DATA_COMPRESSION = COLUMNSTORE_ARCHIVE)")]
	[InlineData("CREATE PRIMARY XML INDEX x ON t (c) WITH (IGNORE_DUP_KEY = OFF, ONLINE = OFF, STATISTICS_NORECOMPUTE = ON, XML_COMPRESSION = ON)")]
	[InlineData("ALTER INDEX ALL ON t REBUILD WITH (STATISTICS_INCREMENTAL = ON, IGNORE_DUP_KEY = ON)")]
	[InlineData("ALTER INDEX i ON t REBUILD PARTITION = 2 WITH (DATA_COMPRESSION = PAGE ON PARTITIONS (2), SORT_IN_TEMPDB = ON)")]
	[InlineData("ALTER INDEX i ON t REBUILD PARTITION = ALL WITH (FILLFACTOR = 80)")]
	[InlineData("ALTER INDEX i ON t REORGANIZE WITH (LOB_COMPACTION = ON, COMPRESS_ALL_ROW_GROUPS = ON)")]
	[InlineData("ALTER INDEX i ON t SET (ALLOW_ROW_LOCKS = ON, COMPRESSION_DELAY = 10 MINUTES)")]
	[InlineData("ALTER INDEX i ON t RESUME WITH (MAXDOP = 2, WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1, ABORT_AFTER_WAIT = SELF))")]
	[InlineData("CREATE TABLE t (a INT PRIMARY KEY WITH FILLFACTOR = 80, b INT)")]
	[InlineData("CREATE TABLE t (i INT NOT NULL PRIMARY KEY NONCLUSTERED HASH WITH (BUCKET_COUNT = 10000))")]
	[InlineData("ALTER TABLE t ADD CONSTRAINT pk PRIMARY KEY (a) WITH (ONLINE = ON, SORT_IN_TEMPDB = ON, MAXDOP = 2, RESUMABLE = ON)")]
	[InlineData("ALTER TABLE t ADD c INT CONSTRAINT u UNIQUE WITH (ONLINE = ON)")]
	[InlineData("CREATE STATISTICS s ON t (a) WITH NORECOMPUTE, FULLSCAN, AUTO_DROP = ON")]
	[InlineData("UPDATE STATISTICS t WITH INDEX, RESAMPLE ON PARTITIONS (1), MAXDOP = 2")]
	[InlineData("CREATE STATISTICS s ON t (a) WITH STATS_STREAM = 0x01, NORECOMPUTE")]
	[InlineData("UPDATE STATISTICS t WITH STATS_STREAM = 0x01, ROWCOUNT = 10, PAGECOUNT = 2")]
	[InlineData("CREATE CLUSTERED COLUMNSTORE INDEX cci ON t WITH (COMPRESSION_DELAY = 1 MINUTE)")]
	[InlineData("ALTER INDEX i ON t SET (COMPRESSION_DELAY = 0 MINUTE)")]
	public void The_index_catalogue_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>
	/// What a database is set to and configured to, as the engine answers it: a closed list,
	/// with `=` where each setting has it and nowhere else.
	/// </summary>
	/// <remarks>
	/// `ALTER DATABASE d SET MAXDOP = 1` is a scoped configuration's setting written where a
	/// database's goes; it was read while the list was a run of words.
	/// </remarks>
	[Theory]
	[InlineData("ALTER DATABASE d SET FOO ON")]
	[InlineData("ALTER DATABASE d SET MAXDOP = 1")]
	[InlineData("ALTER DATABASE d SET AUTO_CLOSE = ON")]
	[InlineData("ALTER DATABASE d SET RECOVERY = SIMPLE")]
	[InlineData("ALTER DATABASE d SET CHANGE_TRACKING ON")]
	[InlineData("ALTER DATABASE d SET CHANGE_TRACKING = ON (CHANGE_RETENTION = 2 DAY)")]
	[InlineData("ALTER DATABASE d SET CONTAINMENT PARTIAL")]
	[InlineData("ALTER DATABASE d SET AUTO_CREATE_STATISTICS OFF (INCREMENTAL = ON)")]
	[InlineData("ALTER DATABASE d SET AUTO_UPDATE_STATISTICS ON (INCREMENTAL = ON)")]
	[InlineData("ALTER DATABASE d SET QUERY_STORE (QUERY_CAPTURE_MODE = FOO)")]
	[InlineData("ALTER DATABASE d SET QUERY_STORE = OFF (OPERATION_MODE = READ_ONLY)")]
	[InlineData("ALTER DATABASE d SET QUERY_STORE (INTERVAL_LENGTH_MINUTES = 15.5)")]
	[InlineData("ALTER DATABASE d SET TARGET_RECOVERY_TIME = 1 MINUTE")]
	[InlineData("ALTER DATABASE d SET DEFAULT_LANGUAGE = 'English'")]
	[InlineData("ALTER DATABASE d SET TWO_DIGIT_YEAR_CUTOFF = 2049.5")]
	[InlineData("ALTER DATABASE d SET HADR ON")]
	[InlineData("ALTER DATABASE d SET PARTNER TIMEOUT 10.5")]
	[InlineData("ALTER DATABASE d SET PERSISTENT_LOG_BUFFER = ON")]
	[InlineData("ALTER DATABASE d SET AUTO_CLOSE ON WITH ROLLBACK AFTER @x")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION SET FOO_BAR = ON")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 4, IDENTITY_CACHE = OFF")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION SET ELEVATE_ONLINE = ON")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = @x")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = +4")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = 1")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION CLEAR PROCEDURE_CACHE @h")]
	public void The_database_catalogue_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("ALTER DATABASE d SET PARTNER TIMEOUT 10")]
	[InlineData("ALTER DATABASE d SET PARTNER SAFETY OFF, PARTNER SAFETY FULL, PARTNER OFF")]
	[InlineData("ALTER DATABASE d SET MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT ON, MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT = OFF")]
	[InlineData("ALTER DATABASE d SET QUERY_STORE = ON (QUERY_CAPTURE_MODE = CUSTOM, " +
		"QUERY_CAPTURE_POLICY = (STALE_CAPTURE_POLICY_THRESHOLD = 1 DAY, EXECUTION_COUNT = 30))")]
	[InlineData("ALTER DATABASE d SET QUERY_STORE = OFF (FORCED)")]
	[InlineData("ALTER DATABASE d SET REMOTE_DATA_ARCHIVE (SERVER = N's', FEDERATED_SERVICE_ACCOUNT = OFF, CREDENTIAL = [c])")]
	[InlineData("ALTER DATABASE d SET FILESTREAM (DIRECTORY_NAME = NULL, NON_TRANSACTED_ACCESS = FULL)")]
	[InlineData("ALTER DATABASE d SET ACCELERATED_DATABASE_RECOVERY = ON (PERSISTENT_VERSION_STORE_FILEGROUP = [fg])")]
	[InlineData("ALTER DATABASE d SET SUSPEND_FOR_SNAPSHOT_BACKUP = ON (MODE = COPY_ONLY)")]
	[InlineData("ALTER DATABASE d SET DEFAULT_LANGUAGE = [us_english], DEFAULT_FULLTEXT_LANGUAGE = 1033")]
	[InlineData("ALTER DATABASE d SET SUPPLEMENTAL_LOGGING ON, VARDECIMAL_STORAGE_FORMAT OFF")]
	[InlineData("ALTER DATABASE d SET TARGET_RECOVERY_TIME = 42.5 SECONDS")]
	[InlineData("ALTER DATABASE d SET PARTNER = 'x', WITNESS = 'y' WITH NO_WAIT")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = -1")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = PRIMARY")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION SET ELEVATE_RESUMABLE = WHEN_SUPPORTED")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION SET LEDGER_DIGEST_STORAGE_ENDPOINT = N'https://x'")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION SET DW_COMPATIBILITY_LEVEL = AUTO")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION CLEAR PROCEDURE_CACHE 0x06")]
	public void The_database_catalogue_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>What a table is created with, set to and rebuilt with, as the engine answers it.</summary>
	[Theory]
	[InlineData("CREATE TABLE t (a INT) WITH (FOO = 1)")]
	[InlineData("CREATE TABLE t (a INT) WITH (DATA_COMPRESSION = FOO)")]
	[InlineData("CREATE TABLE t (a INT) WITH (DATA_COMPRESSION PAGE)")]
	[InlineData("CREATE TABLE t (a INT) WITH (DURABILITY = FOO)")]
	[InlineData("CREATE TABLE t (a INT) WITH (SYSTEM_VERSIONING ON)")]
	[InlineData("CREATE TABLE t (a INT) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = 'h'))")]
	[InlineData("CREATE TABLE t (a INT) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.h, HISTORY_RETENTION_PERIOD = 6))")]
	[InlineData("CREATE TABLE t (a INT) WITH (LEDGER = ON (FOO = ON))")]
	[InlineData("CREATE TABLE t (a INT) WITH (REMOTE_DATA_ARCHIVE = ON)")]
	[InlineData("CREATE TABLE t (a INT) WITH (HEAP)")]
	[InlineData("CREATE TABLE t (a INT) WITH (CLUSTERED COLUMNSTORE INDEX)")]
	[InlineData("CREATE TABLE t (a INT) WITH (LOCK_ESCALATION = AUTO)")]
	[InlineData("CREATE TABLE t AS FILETABLE WITH (FILETABLE_DIRECTORY = d)")]
	[InlineData("ALTER TABLE t SET (LOCK_ESCALATION = FOO)")]
	[InlineData("ALTER TABLE t SET (FILESTREAM_ON = NULL)")]
	[InlineData("ALTER TABLE t SET (DATA_COMPRESSION = PAGE)")]
	[InlineData("ALTER TABLE t SET (MEMORY_OPTIMIZED = ON)")]
	[InlineData("ALTER TABLE t REBUILD WITH (RESUMABLE = ON)")]
	[InlineData("ALTER TABLE t REBUILD WITH (BUCKET_COUNT = 10)")]
	[InlineData("ALTER TABLE t REBUILD PARTITION = 1 WITH (FILLFACTOR = 80)")]
	public void The_table_catalogue_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("CREATE TABLE t (a INT) WITH (DATA_COMPRESSION = PAGE ON PARTITIONS (1, 2 TO 4), XML_COMPRESSION = ON ON PARTITIONS (5))")]
	[InlineData("CREATE TABLE t (a INT) WITH (MEMORY_OPTIMIZED = ON, DURABILITY = SCHEMA_ONLY)")]
	[InlineData("CREATE TABLE t (a INT) WITH (SYSTEM_VERSIONING = ON " +
		"(DATA_CONSISTENCY_CHECK = ON, HISTORY_TABLE = dbo.h, HISTORY_RETENTION_PERIOD = 1.5 DAYS))")]
	[InlineData("CREATE TABLE t (a INT) WITH (LEDGER = ON " +
		"(LEDGER_VIEW = dbo.v (TRANSACTION_ID_COLUMN_NAME = t, SEQUENCE_NUMBER_COLUMN_NAME = s), APPEND_ONLY = OFF))")]
	[InlineData("CREATE TABLE t (a INT) WITH (REMOTE_DATA_ARCHIVE = ON (FILTER_PREDICATE = dbo.f(a), MIGRATION_STATE = PAUSED))")]
	[InlineData("CREATE TABLE t (a INT) WITH (DISTRIBUTION = HASH(a, b))")]
	[InlineData("CREATE TABLE t AS FILETABLE WITH (FILETABLE_DIRECTORY = N'd', FILETABLE_COLLATE_FILENAME = database_default)")]
	[InlineData("ALTER TABLE t SET (LOCK_ESCALATION = AUTO, FILESTREAM_ON = \"default\")")]
	[InlineData("ALTER TABLE t SET (FILETABLE_DIRECTORY = 'foo')")]
	[InlineData("ALTER TABLE t SET (REMOTE_DATA_ARCHIVE = OFF_WITHOUT_DATA_RECOVERY (MIGRATION_STATE = PAUSED))")]
	[InlineData("ALTER TABLE t SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.h, HISTORY_RETENTION_PERIOD = INFINITE))")]
	[InlineData("ALTER TABLE t REBUILD PARTITION = ALL WITH (DATA_COMPRESSION = PAGE, " +
		"ONLINE = ON (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1 MINUTES, ABORT_AFTER_WAIT = BLOCKERS)), FILLFACTOR = 80)")]
	[InlineData("ALTER TABLE t REBUILD PARTITION = 1 WITH (SORT_IN_TEMPDB = ON, MAXDOP = 2)")]
	public void The_table_catalogue_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>What a backup and a restore are given, as the engine answers it.</summary>
	[Theory]
	[InlineData("BACKUP DATABASE d TO DISK = 'x' WITH FOO")]
	[InlineData("BACKUP DATABASE d TO DISK = 'x' WITH NAME = n")]
	[InlineData("BACKUP DATABASE d TO DISK = 'x' WITH COMPRESSION (ALGORITHM = FOO)")]
	[InlineData("BACKUP DATABASE d TO DISK = 'x' WITH COMPRESSION (LEVEL = HIGH)")]
	[InlineData("BACKUP DATABASE d TO DISK = 'x' WITH ENCRYPTION")]
	[InlineData("BACKUP DATABASE d TO DISK = 'x' WITH ENCRYPTION (SERVER CERTIFICATE = c, ALGORITHM = AES_128)")]
	[InlineData("BACKUP DATABASE d TO URL = 'x' WITH CREDENTIAL")]
	[InlineData("BACKUP LOG d TO DISK = 'x' WITH TRUNCATE_ONLY")]
	[InlineData("BACKUP DATABASE d TO DISK = 'x' WITH REPLACE")]
	[InlineData("RESTORE DATABASE d FROM DISK = 'x' WITH FOO")]
	[InlineData("RESTORE DATABASE d FROM DISK = 'x' WITH MOVE a TO b")]
	[InlineData("RESTORE DATABASE d FROM DISK = 'x' WITH FILE = 'x'")]
	[InlineData("RESTORE DATABASE d FROM DISK = 'x' WITH DIFFERENTIAL")]
	[InlineData("RESTORE DATABASE d FROM DISK = 'x' WITH BASE = 'y'")]
	[InlineData("RESTORE DATABASE d FROM DATABASE_SNAPSHOT = s")]
	[InlineData("RESTORE DATABASE d FROM DISK = 'x' WITH FILESTREAM (DIRECTORY_NAME = @v)")]
	[InlineData("RESTORE DATABASE d FROM DISK = 'x' WITH KEEP_TEMPORAL_RETENTION = ON")]
	[InlineData("BACKUP DATABASE d TO DISK = 'x' WITH KEEP_TEMPORAL_RETENTION")]
	public void The_backup_catalogue_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("BACKUP DATABASE d TO DISK = 'x' WITH COPY_ONLY, COMPRESSION (ALGORITHM = ZSTD, LEVEL = HIGH), CHECKSUM, STATS = 10")]
	[InlineData("BACKUP DATABASE d TO DISK = 'x' WITH ENCRYPTION (ALGORITHM = AES_256, SERVER ASYMMETRIC KEY = k), NAME = @n")]
	[InlineData("BACKUP DATABASE d TO DISK = 'x' WITH DESCRIPTION = 5, RETAINDAYS = 'x', MEDIANAME = 'm', BLOCKSIZE = @b")]
	[InlineData("BACKUP LOG d TO DISK = 'x' WITH NORECOVERY, STANDBY = @u, NO_TRUNCATE, NO_LOG")]
	[InlineData("BACKUP GROUP d1, d2 TO DISK = 'x' WITH METADATA_ONLY")]
	[InlineData("RESTORE DATABASE d FROM DISK = 'x' WITH MOVE 'a' TO 'b', MOVE @c TO @d, REPLACE, STATS")]
	[InlineData("RESTORE DATABASE d FROM DISK = 'x' WITH FILE = 2, DBNAME = @n, SNAPSHOT, KEEP_CDC, NEW_BROKER")]
	[InlineData("RESTORE LOG d FROM DISK = 'x' WITH STOPATMARK = 'm' AFTER '2020-01-01', RECOVERY")]
	[InlineData("RESTORE DATABASE d FROM DATABASE_SNAPSHOT = 's'")]
	[InlineData("RESTORE HEADERONLY FROM DISK = 'x' WITH RECOVERY, MOVE 'a' TO 'b'")]
	[InlineData("RESTORE DATABASE d FROM DISK = 'x' WITH FILESTREAM (DIRECTORY_NAME = 'd')")]
	[InlineData("RESTORE DATABASE d FROM DISK = 'x' WITH FILESTREAM (DIRECTORY_NAME = NULL)")]
	[InlineData("RESTORE LOG d WITH STANDBY = 'f', NOREWIND, NOUNLOAD, STATS, KEEP_TEMPORAL_RETENTION")]
	public void The_backup_catalogue_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>What a login, a user and an application role are given, as the engine answers it.</summary>
	[Theory]
	[InlineData("CREATE LOGIN l WITH PASSWORD = @p")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 0x0100")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 0x0100 MUST_CHANGE")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 'p' UNLOCK")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 'p' OLD_PASSWORD = 'q'")]
	[InlineData("CREATE LOGIN l WITH SID = 0x01, PASSWORD = 'p'")]
	[InlineData("CREATE LOGIN l WITH DEFAULT_DATABASE = d")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 'p', SID = 'x'")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 'p', DEFAULT_DATABASE = 'd'")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 'p', DEFAULT_LANGUAGE = 1033")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 'p', NO CREDENTIAL")]
	[InlineData("CREATE LOGIN l FROM WINDOWS WITH CHECK_POLICY = ON")]
	[InlineData("CREATE LOGIN l FROM EXTERNAL PROVIDER WITH CREDENTIAL = c")]
	[InlineData("CREATE LOGIN l FROM CERTIFICATE c WITH DEFAULT_DATABASE = d")]
	[InlineData("CREATE LOGIN l FROM CERTIFICATE s.c")]
	[InlineData("ALTER LOGIN l WITH PASSWORD = 'p' OLD_PASSWORD = 'q' MUST_CHANGE")]
	[InlineData("ALTER LOGIN l WITH PASSWORD = 'p' HASHED OLD_PASSWORD = 'q'")]
	[InlineData("ALTER LOGIN l WITH PASSWORD = 'p' UNLOCK UNLOCK")]
	[InlineData("ALTER LOGIN l WITH PASSWORD = 'p', OLD_PASSWORD = 'q'")]
	[InlineData("ALTER LOGIN l WITH NAME = 'n'")]
	[InlineData("ALTER LOGIN l WITH SID = 0x01")]
	[InlineData("ALTER LOGIN l WITH MUST_CHANGE")]
	[InlineData("ALTER LOGIN l WITH PASSWORD = 0x01 UNLOCK MUST_CHANGE")]
	[InlineData("ALTER LOGIN l WITH PASSWORD = 'p' MUST_CHANGE HASHED MUST_CHANGE")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 'p' HASHED UNLOCK")]
	[InlineData("CREATE LOGIN l FROM EXTERNAL PROVIDER WITH TYPE = 'E'")]
	[InlineData("CREATE LOGIN l FROM WINDOWS WITH TYPE = E")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 'p', TYPE = E")]
	[InlineData("CREATE USER u FOR LOGIN a.b")]
	[InlineData("CREATE USER u WITH DEFAULT_SCHEMA = NULL")]
	[InlineData("CREATE USER u WITH DEFAULT_LANGUAGE = 'us_english'")]
	[InlineData("CREATE USER u WITH ALLOW_ENCRYPTED_VALUE_MODIFICATIONS")]
	[InlineData("CREATE USER u WITH PASSWORD = 'p' MUST_CHANGE")]
	[InlineData("CREATE USER u WITH LOGIN = l")]
	[InlineData("CREATE USER u WITH TYPE = 'E'")]
	[InlineData("ALTER USER u FROM EXTERNAL PROVIDER")]
	[InlineData("ALTER USER u FOR CERTIFICATE c")]
	[InlineData("ALTER USER u WITH SID = 0x01")]
	[InlineData("ALTER USER u WITH PASSWORD = 'p' MUST_CHANGE")]
	[InlineData("CREATE APPLICATION ROLE r WITH PASSWORD = 'p', NAME = n")]
	[InlineData("CREATE APPLICATION ROLE r WITH DEFAULT_LANGUAGE = NONE")]
	[InlineData("ALTER APPLICATION ROLE r WITH DEFAULT_SCHEMA = NULL")]
	[InlineData("ALTER APPLICATION ROLE r WITH DEFAULT_LANGUAGE = 1033")]
	public void The_principal_catalogue_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("CREATE LOGIN l WITH PASSWORD = N'p' HASHED MUST_CHANGE, SID = 0x01, DEFAULT_LANGUAGE = NONE")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 0x0100 MUST_CHANGE HASHED, CHECK_POLICY = OFF, CREDENTIAL = c")]
	[InlineData("CREATE LOGIN [d\\u] FROM WINDOWS WITH DEFAULT_LANGUAGE = l, DEFAULT_DATABASE = [d]")]
	[InlineData("CREATE LOGIN l FROM EXTERNAL PROVIDER WITH OBJECT_ID = 'x', SID = 0x01, DEFAULT_DATABASE = d")]
	[InlineData("CREATE LOGIN l FROM ASYMMETRIC KEY [k] WITH CREDENTIAL = [c]")]
	[InlineData("ALTER LOGIN l WITH PASSWORD = 0x01 HASHED UNLOCK MUST_CHANGE, CHECK_EXPIRATION = ON")]
	[InlineData("ALTER LOGIN l WITH PASSWORD = N'p' OLD_PASSWORD = 'q', NAME = [n], NO CREDENTIAL")]
	[InlineData("ALTER LOGIN l WITH PASSWORD = 'p' MUST_CHANGE HASHED UNLOCK, TYPE = E")]
	[InlineData("ALTER LOGIN l WITH PASSWORD = 0x01 MUST_CHANGE UNLOCK HASHED")]
	[InlineData("CREATE LOGIN [l] FROM EXTERNAL PROVIDER WITH SID = 0x01, TYPE = [X], DEFAULT_LANGUAGE = l")]
	[InlineData("CREATE USER u FOR EXTERNAL PROVIDER")]
	[InlineData("CREATE USER u FOR CERTIFICATE c WITH DEFAULT_SCHEMA = s")]
	[InlineData("CREATE USER u WITH SID = 0x01, TYPE = X, OBJECT_ID = x, DEFAULT_LANGUAGE = 0")]
	[InlineData("CREATE USER u FOR LOGIN l WITH PASSWORD = 'p'")]
	[InlineData("ALTER USER u WITH DEFAULT_SCHEMA = NULL, LOGIN = l, PASSWORD = 'p' OLD_PASSWORD = 'q'")]
	[InlineData("ALTER USER u WITH DEFAULT_LANGUAGE = 1033, ALLOW_ENCRYPTED_VALUE_MODIFICATIONS = ON")]
	[InlineData("CREATE APPLICATION ROLE r WITH DEFAULT_SCHEMA = s, PASSWORD = N'p'")]
	[InlineData("ALTER APPLICATION ROLE r WITH DEFAULT_LANGUAGE = us_english, LOGIN = l, NAME = [n]")]
	public void The_principal_catalogue_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>What an endpoint is given, as the engine answers it.</summary>
	[Theory]
	[InlineData("CREATE ENDPOINT e STATE = FOO AS TCP (LISTENER_PORT = 1) FOR TSQL ()")]
	[InlineData("CREATE ENDPOINT e STATE = STARTED, AFFINITY = 1 AS TCP (LISTENER_PORT = 1) FOR TSQL ()")]
	[InlineData("CREATE ENDPOINT e STATE = STARTED, FOO = 1 AS TCP (LISTENER_PORT = 1) FOR TSQL ()")]
	[InlineData("CREATE ENDPOINT e AS TCP () FOR TSQL ()")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = '1') FOR TSQL ()")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1.5) FOR TSQL ()")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1, LISTENER_IP = '::1') FOR TSQL ()")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1, LISTENER_IP = (1.2.3)) FOR TSQL ()")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1, LISTENER_IP = (1.2.3.4.5)) FOR TSQL ()")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1, LISTENER_IP = (ALL)) FOR TSQL ()")]
	[InlineData("CREATE ENDPOINT e AS FOO (LISTENER_PORT = 1) FOR TSQL ()")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1) FOR FOO ()")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1) FOR TSQL (FOO = 1)")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1) FOR TSQL (AUTHENTICATION = WINDOWS)")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1) FOR SERVICE_BROKER (ROLE = ALL)")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1) FOR SERVICE_BROKER (AUTHENTICATION = WINDOWS NTLM KERBEROS)")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1) FOR SERVICE_BROKER (AUTHENTICATION = CERTIFICATE c CERTIFICATE d)")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1) FOR SERVICE_BROKER (ENCRYPTION = DISABLED ALGORITHM AES)")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1) FOR SERVICE_BROKER (ENCRYPTION = REQUIRED ALGORITHM AES AES)")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1) FOR SERVICE_BROKER (MESSAGE_FORWARDING = ON)")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1) FOR DATABASE_MIRRORING (ROLE = FOO)")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1) FOR DATABASE_MIRRORING (MESSAGE_FORWARD_SIZE = 10)")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1) FOR TSQL () AS TCP (LISTENER_PORT = 2)")]
	[InlineData("ALTER ENDPOINT e AUTHORIZATION sa STATE = STARTED")]
	public void The_endpoint_catalogue_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("CREATE ENDPOINT e AFFINITY = NONE, STATE = STARTED AS TCP (LISTENER_PORT = 1) FOR TSQL ()")]
	[InlineData("CREATE ENDPOINT e STATE = DISABLED, AFFINITY = ADMIN AS TCP (LISTENER_PORT = -1) FOR TSQL (ENCRYPTION = STRICT)")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1, LISTENER_IP = (1 . 2 . 3 . 4)) FOR TSQL (ENCRYPTION = REQUIRED ALGORITHM AES)")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_IP = (N'::1'), LISTENER_PORT = 1) FOR TSQL ()")]
	[InlineData("CREATE ENDPOINT e FOR TSQL () AS TCP (LISTENER_PORT = 1)")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1) FOR SERVICE_BROKER (AUTHENTICATION = CERTIFICATE [c] WINDOWS NEGOTIATE, " +
		"ENCRYPTION = SUPPORTED ALGORITHM RC4 AES, MESSAGE_FORWARDING = ENABLED, MESSAGE_FORWARD_SIZE = 10)")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1) FOR DATABASE_MIRRORING (ROLE = WITNESS, AUTHENTICATION = WINDOWS, ENCRYPTION = NEGOTIATED)")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1) FOR SERVICE_BROKER ()")]
	[InlineData("ALTER ENDPOINT e AFFINITY = ADMIN")]
	[InlineData("ALTER ENDPOINT e FOR DATABASE_MIRRORING (ROLE = ALL)")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_IP = (1 . 1.1.1 : 10.10.20. 30)) FOR DATA_MIRRORING (ROLE = ALL)")]
	public void The_endpoint_catalogue_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The full-text predicates, as the engine answers them.</summary>
	[Theory]
	[InlineData("SELECT a FROM t WHERE CONTAINS ((t.*, b), 'x')")]
	[InlineData("SELECT a FROM t WHERE CONTAINS ((b, *), 'x')")]
	[InlineData("SELECT a FROM t WHERE CONTAINS ((), 'x')")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (((b)), 'x')")]
	[InlineData("SELECT a FROM t WHERE CONTAINS ($ IDENTITY, 'x')")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (@v, 'x')")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (PROPERTY (b, @p), 'x')")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (b)")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (b, x)")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (b, 1)")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (b, ('x'))")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (b, 'x' COLLATE Latin1_General_CI_AS)")]
	[InlineData("SELECT a FROM t WHERE FREETEXT (b, 'x' + @s)")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (b, 'x', 1033)")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (b, 'x', LANGUAGE)")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (b, 'x', LANGUAGE -1)")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (b, 'x', LANGUAGE (1033))")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (b, 'x', LANGUAGE 1033.5)")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (b, 'x', LANGUAGE English)")]
	public void The_full_text_predicates_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("SELECT a FROM t WHERE CONTAINS ((b, c), 'x')")]
	[InlineData("SELECT a FROM t WHERE CONTAINS ((t.b, c, d), @s, LANGUAGE @l)")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (*, 'x')")]
	[InlineData("SELECT a FROM t WHERE CONTAINS ((*), 'x')")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (s.t.*, N'x')")]
	[InlineData("SELECT a FROM t WHERE CONTAINS ((t.*), 'x', LANGUAGE 0x409)")]
	[InlineData("SELECT a FROM t WHERE CONTAINS ($IDENTITY, 'x')")]
	[InlineData("SELECT a FROM t WHERE CONTAINS ((t.$IDENTITY), 'x')")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (($IDENTITY, b), 'x', LANGUAGE 1033)")]
	[InlineData("SELECT a FROM t WHERE CONTAINS ($ROWGUID, 'x')")]
	[InlineData("SELECT a FROM t WHERE CONTAINS (PROPERTY (t.b, N'p'), 'x')")]
	[InlineData("SELECT a FROM t WHERE FREETEXT (b, N'x', LANGUAGE N'English')")]
	[InlineData("SELECT a FROM t WHERE NOT FREETEXT ((t2.*), N'abc') AND CONTAINS (b, 'x')")]
	public void The_full_text_predicates_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The full-text rowset functions, as the engine answers them.</summary>
	[Theory]
	[InlineData("SELECT * FROM CONTAINSTABLE (@t, b, 'x') AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE ('t', b, 'x') AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, t.*, 'x') AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, t.b, 'x') AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, (t.b, c), 'x') AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, $IDENTITY, 'x') AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, (b, $IDENTITY), 'x') AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, (PROPERTY (b, 'p')), 'x') AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, PROPERTY (b, @p), 'x') AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, b) AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, b, x) AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, b, 'x' + 'y') AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, b, 'x', LANGUAGE English) AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, b, 'x', 5 + 1) AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, b, 'x', -5) AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, b, 'x', '5') AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, b, 'x', 5, 6) AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, b, 'x', 5, LANGUAGE 1033) AS k")]
	[InlineData("SELECT * FROM FREETEXTTABLE (t, $IDENTITY, 'x') AS k")]
	[InlineData("SELECT * FROM FREETEXTTABLE (t, b, 'x', 10, LANGUAGE 1033) AS k")]
	public void The_full_text_rowsets_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("SELECT * FROM CONTAINSTABLE (d.s.t, [b], N'x') AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, (*), 'x', LANGUAGE 0x409) AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, (b, c), @s, LANGUAGE N'English', @n) AS k")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t, PROPERTY (b, 'p'), 'x', 5) k")]
	[InlineData("SELECT * FROM FREETEXTTABLE (t, *, 'x', 10) AS k")]
	[InlineData("SELECT k.[KEY], k.RANK FROM t INNER JOIN FREETEXTTABLE (t, (b), 'x') AS k ON t.a = k.[KEY]")]
	public void The_full_text_rowsets_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Whom the session runs as, as the engine answers it.</summary>
	[Theory]
	[InlineData("EXECUTE AS SELF")]
	[InlineData("EXECUTE AS OWNER")]
	[InlineData("EXECUTE AS 'dbo'")]
	[InlineData("EXECUTE AS FOO = 'u'")]
	[InlineData("EXECUTE AS USER 'u'")]
	[InlineData("EXECUTE AS USER")]
	[InlineData("EXECUTE AS USER = 'u', NO REVERT")]
	[InlineData("EXECUTE AS USER = 'u' WITH NO REVERT, COOKIE INTO @c")]
	[InlineData("EXECUTE AS USER = 'u' WITH NO REVERT, NO REVERT")]
	[InlineData("EXECUTE AS USER = 'u' WITH COOKIE INTO c")]
	[InlineData("EXECUTE AS USER = 'u' WITH NORESET")]
	[InlineData("EXECUTE AS CALLER, NO REVERT")]
	[InlineData("EXEC AS CALLER WITH COOKIE INTO @c, NO REVERT")]
	[InlineData("EXECUTE AS LOGIN = 'l' WITH COOKIE INTO @c, NO REVERT")]
	[InlineData("REVERT WITH COOKIE @c")]
	[InlineData("REVERT WITH COOKIE")]
	[InlineData("REVERT WITH NO REVERT")]
	[InlineData("SETUSER u")]
	[InlineData("SETUSER 'u' + 'v'")]
	[InlineData("SETUSER WITH NORESET")]
	[InlineData("SETUSER 'u' WITH FOO")]
	public void The_session_statements_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("EXECUTE AS CALLER")]
	[InlineData("EXEC AS CALLER WITH COOKIE INTO @c")]
	[InlineData("EXECUTE AS USER = N'u'")]
	[InlineData("EXECUTE AS USER = dbo.fn_getuser() WITH NO REVERT")]
	[InlineData("EXECUTE AS USER = 'u' + @v WITH COOKIE INTO @@c")]
	[InlineData("EXEC AS LOGIN = @l WITH COOKIE INTO @c")]
	[InlineData("EXECUTE AS LOGIN = 'l' WITH NO REVERT")]
	[InlineData("REVERT")]
	[InlineData("REVERT WITH COOKIE = @c + 1")]
	[InlineData("REVERT WITH COOKIE = 0x01")]
	[InlineData("SETUSER")]
	[InlineData("SETUSER ''")]
	[InlineData("SETUSER N'u' WITH NORESET")]
	[InlineData("SETUSER @u WITH NORESET")]
	public void The_session_statements_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>
	/// A statement not ended by <c>;</c> is not followed by one whose first word is not
	/// reserved: the engine reads the word as a continuation of the statement before it.
	/// </summary>
	[Theory]
	[InlineData("END CONVERSATION 10 ENABLE TRIGGER t1 ON o1", false)]
	[InlineData("END CONVERSATION 10 DISABLE TRIGGER t1 ON o1", false)]
	[InlineData("PRINT 1 ENABLE TRIGGER t1 ON o1", false)]
	[InlineData("SET NOCOUNT ON ENABLE TRIGGER t1 ON o1", false)]
	[InlineData("ENABLE TRIGGER t1 ON o1 ENABLE TRIGGER t2 ON o2", false)]
	[InlineData("END CONVERSATION 10 RECEIVE TOP (1) * FROM q", false)]
	[InlineData("END CONVERSATION 10 THROW", false)]
	[InlineData("PRINT 1 THROW", false)]
	[InlineData("PRINT 1 RECEIVE * FROM q", false)]
	[InlineData("END CONVERSATION 10 MOVE CONVERSATION 10 TO 20", false)]
	[InlineData("END CONVERSATION 10 GET CONVERSATION GROUP @g FROM q", false)]
	[InlineData("END CONVERSATION 10 SEND ON CONVERSATION 10 MESSAGE TYPE m", false)]
	[InlineData("SELECT 1 DISABLE TRIGGER t1 ON o1", false)]
	[InlineData("END CONVERSATION 10 ; ENABLE TRIGGER t1 ON o1", true)]
	[InlineData("PRINT 1; ENABLE TRIGGER t1 ON o1", true)]
	[InlineData("ENABLE TRIGGER t1 ON o1; ENABLE TRIGGER t2 ON o2", true)]
	[InlineData("END CONVERSATION 10 PRINT 1", true)]
	[InlineData("END CONVERSATION 10 TRUNCATE TABLE t", true)]
	[InlineData("PRINT 1 PRINT 2", true)]
	[InlineData("DECLARE @a INT PRINT 1", true)]
	[InlineData("BEGIN ENABLE TRIGGER t1 ON o1 END", true)]
	[InlineData("END CONVERSATION 10 SELECT 1", true)]
	[InlineData("END CONVERSATION 10 WAITFOR DELAY '00:00:01'", true)]
	[InlineData("END CONVERSATION 10 RECONFIGURE", true)]
	[InlineData("END CONVERSATION 10 BULK INSERT t FROM 'f'", true)]
	[InlineData("END CONVERSATION 10 REVERT", true)]
	[InlineData("END CONVERSATION 10 CHECKPOINT", true)]
	[InlineData("END CONVERSATION 10 DBCC CHECKDB", true)]
	[InlineData("END CONVERSATION 10 ALTER TABLE t ADD a INT", true)]
	[InlineData("END CONVERSATION 10 CREATE TABLE t (a INT)", true)]
	[InlineData("END CONVERSATION 10 UPDATE t SET c = 1", true)]
	[InlineData("END CONVERSATION 10 KILL 53", true)]
	[InlineData("END CONVERSATION 10 OPEN c", true)]
	[InlineData("END CONVERSATION 10 FETCH NEXT FROM c", true)]
	[InlineData("END CONVERSATION 10 GRANT SELECT ON t TO u", true)]
	[InlineData("END CONVERSATION 10 BACKUP DATABASE d TO DISK = 'f'", true)]
	public void A_statement_not_ended_is_followed_by_a_reserved_word(string input, bool read) =>
		Assert.Equal(read, TransactSql.TryParseSql(input).IsSuccess);

	/// <summary>The schema a rowset function takes, and a <c>CATCH</c> with nothing in it, as the engine answers them.</summary>
	[Theory]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH ([a] INT '@b') AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH ([a] INT 1.5) AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH ([a] INT -1) AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH ([a] INT 0) AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH ([a] INT '1') AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH ([a] INT 1 COLLATE latin1_general_bin2) AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH ([a] INT '$.a' 1) AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH ([a] INT 1 '@b') AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH ([a] INT 9223372036854775808) AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH t AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH dbo.t AS x")]
	[InlineData("SELECT * FROM OPENXML(@h, '/x') WITH (a INT 1)")]
	[InlineData("SELECT * FROM OPENXML(@h, '/x') WITH (a INT '@b', c VARCHAR (10) 2)")]
	[InlineData("SELECT * FROM OPENXML(@h, '/x') WITH (a INT 1, b INT 2)")]
	[InlineData("SELECT * FROM OPENXML(@h, '/x') WITH (a INT 0)")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', SINGLE_CLOB) WITH ([a] INT) AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', SINGLE_BLOB) WITH ([a] INT) AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', SINGLE_NCLOB) WITH ([a] INT) AS x")]
	[InlineData("SELECT * FROM OPENROWSET ('p', 'q', 'r') WITH ([a] INT) AS x")]
	[InlineData("SELECT * FROM CHANGETABLE (CHANGES t, 0) WITH ([a] INT) AS x")]
	[InlineData("SELECT * FROM CHANGETABLE (VERSION t, (id), (1)) WITH ([a] INT) AS x")]
	[InlineData("SELECT * FROM STRING_SPLIT('a,b', ',') WITH ([a] INT)")]
	[InlineData("SELECT * FROM GENERATE_SERIES(1, 10) WITH ([a] INT)")]
	[InlineData("SELECT * FROM OPENQUERY(s, 'q') WITH ([a] INT)")]
	[InlineData("BEGIN TRY END TRY BEGIN CATCH SELECT 1; END CATCH")]
	[InlineData("BEGIN TRY END TRY BEGIN CATCH END CATCH")]
	[InlineData("BEGIN TRY ; END TRY BEGIN CATCH ; END CATCH")]
	[InlineData("BEGIN TRY SELECT 1; END TRY BEGIN CATCH ; END CATCH")]
	[InlineData("BEGIN TRY BEGIN END END TRY BEGIN CATCH END CATCH")]
	public void Schemas_and_catches_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH ([a] INT 1) AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH ([a] INT 1, [b] VARCHAR (10) COLLATE latin1_general_bin2 2, [c] INT) AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH ([a] INT 2147483647) AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH ([a] INT COLLATE latin1_general_bin2 2) AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH ([a] INT) AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV') WITH ([a] INT COLLATE latin1_general_bin2) AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'PARQUET') WITH ([region] VARCHAR (100) 1) AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', FORMAT = 'CSV', SINGLE_CLOB) WITH ([a] INT) AS x")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f1', SINGLE_CLOB) AS x")]
	[InlineData("SELECT * FROM OPENROWSET ('p', 'q', 'r') AS x")]
	[InlineData("SELECT * FROM CHANGETABLE (CHANGES t, 0) AS x")]
	[InlineData("SELECT * FROM OPENXML(@h, '/x') WITH (a INT)")]
	[InlineData("SELECT * FROM OPENXML(@h, '/x') WITH (a INT '@b')")]
	[InlineData("SELECT * FROM OPENXML(@h, '/x') WITH (a INT COLLATE latin1_general_bin2 '@b')")]
	[InlineData("SELECT * FROM OPENXML(@h, '/x') WITH t")]
	[InlineData("SELECT * FROM OPENXML(@h, '/x') WITH dbo.t")]
	[InlineData("BEGIN TRY SELECT 1; END TRY BEGIN CATCH END CATCH")]
	[InlineData("BEGIN TRY SELECT 1; END TRY BEGIN CATCH SELECT 2; END CATCH")]
	[InlineData("CREATE PROCEDURE p1 AS BEGIN BEGIN TRY END CONVERSATION 10; ENABLE TRIGGER t1 ON o1; END TRY BEGIN CATCH END CONVERSATION 10; ENABLE TRIGGER t1 ON o1; END CATCH END")]
	[InlineData("BEGIN TRY END CONVERSATION 10; END TRY BEGIN CATCH END CATCH")]
	[InlineData("BEGIN TRY END CONVERSATION 10 END TRY BEGIN CATCH END CATCH")]
	[InlineData("BEGIN TRY END CONVERSATION @h; END TRY BEGIN CATCH END CATCH")]
	[InlineData("BEGIN TRY END CONVERSATION 10; SELECT 1; END TRY BEGIN CATCH END CATCH")]
	[InlineData("BEGIN TRY BEGIN END CONVERSATION 10; END END TRY BEGIN CATCH END CATCH")]
	public void Schemas_and_catches_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>A constraint dropped with its index, a placement by name or string, and a column altered online, as the engine answers them.</summary>
	[Theory]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1 MINUTE, ABORT_AFTER_WAIT = BLOCKERS))")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1))")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (WAIT_AT_LOW_PRIORITY (ABORT_AFTER_WAIT = NONE))")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (WAIT_AT_LOW_PRIORITY (ABORT_AFTER_WAIT = NONE, MAX_DURATION = 1))")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (WAIT_AT_LOW_PRIORITY ())")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (WAIT_AT_LOW_PRIORITY)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1, ABORT_AFTER_WAIT = NONE), WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1, ABORT_AFTER_WAIT = NONE))")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (RESUMABLE = ON)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (FOO = 1)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH ONLINE = ON")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MAXDOP = 1, MAXDOP = 2)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1, ABORT_AFTER_WAIT = FOO))")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1 HOURS, ABORT_AFTER_WAIT = NONE))")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (ONLINE = ON (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1, ABORT_AFTER_WAIT = NONE)))")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MOVE TO DEFAULT (c1))")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MOVE TO DEFAULT)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MOVE TO fg, MOVE TO fg2)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MAXDOP = x)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MAXDOP = '1')")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MAXDOP = -1)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MAXDOP = 1.5)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MAXDOP = @m)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (ONLINE = FOO)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (ONLINE = 1)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (ONLINE = ON, ONLINE = OFF)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (ONLINE OFF)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH ()")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1, ABORT_AFTER_WAIT = NONE,))")]
	[InlineData("ALTER TABLE t DROP COLUMN c WITH (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1, ABORT_AFTER_WAIT = NONE))")]
	[InlineData("ALTER TABLE t DROP INDEX i WITH (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1, ABORT_AFTER_WAIT = NONE))")]
	[InlineData("ALTER TABLE t DROP PERIOD FOR SYSTEM_TIME WITH (ONLINE = ON)")]
	[InlineData("CREATE TABLE t (a INT) TEXTIMAGE_ON 'fg' (a)")]
	[InlineData("ALTER INDEX i ON t REBUILD WITH (MOVE TO 'fg' (a))")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD ROWGUIDCOL WITH (ONLINE = ON)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c DROP ROWGUIDCOL WITH (ONLINE = ON)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD NOT FOR REPLICATION WITH (ONLINE = ON)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c DROP NOT FOR REPLICATION WITH (ONLINE = ON)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD PERSISTED WITH (ONLINE = ON)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c DROP PERSISTED WITH (ONLINE = ON)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD HIDDEN WITH (ONLINE = ON)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD ROWGUIDCOL WITH (MAXDOP = 1)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD ROWGUIDCOL WITH (FOO = 1)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c INT WITH (MAXDOP = 1)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c INT WITH (FOO = 1)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c INT WITH (ONLINE = ON, ONLINE = OFF)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD COLUMN_SET FOR ALL_SPARSE_COLUMNS WITH (ONLINE = ON)")]
	public void Dropped_and_altered_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("ALTER TABLE T1 DROP COLUMN C1, CONSTRAINT C21 WITH (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 10 MINUTES, ABORT_AFTER_WAIT = NONE)), CS1 WITH (MOVE TO 'DEFAULT' (COL1), ONLINE = OFF, MAXDOP = 21)")]
	[InlineData("ALTER TABLE T1 DROP CONSTRAINT CS1 WITH (ONLINE = OFF, MOVE TO MYFILEGROUP, WAIT_AT_LOW_PRIORITY (MAX_DURATION = 10 MINUTES, ABORT_AFTER_WAIT = NONE)), CS2 WITH (MAXDOP = 21)")]
	[InlineData("ALTER TABLE T1 DROP CONSTRAINT CS1 WITH (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 10 MINUTES, ABORT_AFTER_WAIT = NONE), ONLINE = ON)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1, ABORT_AFTER_WAIT = SELF))")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT IF EXISTS c WITH (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1, ABORT_AFTER_WAIT = NONE))")]
	[InlineData("ALTER TABLE t DROP c WITH (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1, ABORT_AFTER_WAIT = NONE))")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MOVE TO ps (c), ONLINE = ON)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MAXDOP = 1), d WITH (ONLINE = ON)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MOVE TO 'fg' (c1))")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MOVE TO \"default\" (c1))")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MOVE TO [fg] (c1))")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MOVE TO 'fg')")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MOVE TO \"default\")")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MOVE TO ps (c1, c2))")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MAXDOP = 0)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 0 MINUTES, ABORT_AFTER_WAIT = BLOCKERS))")]
	[InlineData("CREATE TABLE t (a INT) ON 'fg' (a)")]
	[InlineData("CREATE TABLE t (a INT) ON \"default\" (a)")]
	[InlineData("CREATE INDEX i ON t (a) ON 'fg' (a)")]
	[InlineData("DROP INDEX i ON t WITH (MOVE TO 'fg' (a))")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD ROWGUIDCOL WITH (ONLINE = OFF)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c DROP ROWGUIDCOL WITH (ONLINE = OFF)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD NOT FOR REPLICATION WITH (ONLINE = OFF)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD SPARSE WITH (ONLINE = ON)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD SPARSE WITH (ONLINE = OFF)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c DROP SPARSE WITH (ONLINE = ON)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c DROP MASKED WITH (ONLINE = ON)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD ROWGUIDCOL")]
	[InlineData("ALTER TABLE t ALTER COLUMN c ADD SPARSE")]
	[InlineData("ALTER TABLE t ALTER COLUMN c INT WITH (ONLINE = ON)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c INT WITH (ONLINE = OFF)")]
	public void Dropped_and_altered_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>An audit's <c>NULL</c> principal, a column sized <c>MAX</c>, a chained <c>DEFAULT</c> and a numbered savepoint, as the engine answers them.</summary>
	[Theory]
	[InlineData("CREATE DATABASE AUDIT SPECIFICATION s FOR SERVER AUDIT a ADD (SELECT ON t BY 'x')")]
	[InlineData("CREATE DATABASE AUDIT SPECIFICATION s FOR SERVER AUDIT a ADD (SELECT ON t BY NULL AS x)")]
	[InlineData("CREATE TYPE ssn FROM DECIMAL (MAX)")]
	[InlineData("DECLARE @d DECIMAL (MAX)")]
	[InlineData("SELECT CAST(1 AS DECIMAL (MAX))")]
	[InlineData("CREATE TABLE t1 (c1 DECIMAL (MAX, 2))")]
	[InlineData("CREATE TABLE t (c NVARCHAR (MAX, 1))")]
	[InlineData("UPDATE t1 SET c1 -= DEFAULT")]
	[InlineData("UPDATE t1 SET c1 += DEFAULT")]
	[InlineData("SET @a += DEFAULT")]
	[InlineData("SET @a = DEFAULT")]
	[InlineData("UPDATE t1 SET c1 *= DEFAULT, c1 /= DEFAULT")]
	[InlineData("SAVE TRANSACTION 5:a")]
	[InlineData("SAVE TRANSACTION 5:a.b.c")]
	[InlineData("SAVE TRANSACTION +5:a.b")]
	[InlineData("SAVE TRANSACTION a:b")]
	[InlineData("SAVE TRANSACTION 0x05:a.b")]
	[InlineData("SAVE TRANSACTION 5:a..b")]
	[InlineData("SAVE TRANSACTION 5:@a.b")]
	[InlineData("SAVE TRANSACTION 5e1:a.b")]
	[InlineData("SAVE TRANSACTION $5:a.b")]
	public void Old_forms_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("CREATE DATABASE AUDIT SPECIFICATION s FOR SERVER AUDIT a ADD (EXECUTE, RECEIVE, REFERENCES ON zzz BY PUBLIC, NULL, dbo)")]
	[InlineData("CREATE DATABASE AUDIT SPECIFICATION s FOR SERVER AUDIT a ADD (SELECT ON t BY NULL)")]
	[InlineData("CREATE DATABASE AUDIT SPECIFICATION s FOR SERVER AUDIT a ADD (SELECT ON t BY dbo, NULL)")]
	[InlineData("CREATE DATABASE AUDIT SPECIFICATION s FOR SERVER AUDIT a ADD (SELECT ON t BY [NULL])")]
	[InlineData("CREATE DATABASE AUDIT SPECIFICATION s FOR SERVER AUDIT a ADD (SELECT ON t BY NULL, NULL)")]
	[InlineData("ALTER DATABASE AUDIT SPECIFICATION s ADD (SELECT ON t BY NULL)")]
	[InlineData("ALTER DATABASE AUDIT SPECIFICATION s DROP (SELECT ON t BY NULL)")]
	[InlineData("CREATE DATABASE AUDIT SPECIFICATION s FOR SERVER AUDIT a ADD (SELECT ON OBJECT::dbo.t BY NULL)")]
	[InlineData("CREATE TABLE t1 (c1 DECIMAL (MAX))")]
	[InlineData("CREATE TABLE t1 (c1 decimal(max) not null)")]
	[InlineData("CREATE TABLE t1 (c1 NUMERIC (MAX))")]
	[InlineData("CREATE TABLE t1 (c1 INT (MAX))")]
	[InlineData("CREATE TABLE t1 (c1 FLOAT (MAX))")]
	[InlineData("ALTER TABLE t ADD c DECIMAL (MAX)")]
	[InlineData("DECLARE @t TABLE (c DECIMAL (MAX))")]
	[InlineData("CREATE TYPE tt AS TABLE (c DECIMAL (MAX))")]
	[InlineData("ALTER TABLE t ALTER COLUMN c DECIMAL (MAX)")]
	[InlineData("CREATE TABLE t (c DECIMAL (MAX) NOT NULL PRIMARY KEY)")]
	[InlineData("CREATE TABLE t (c DECIMAL ( MAX ))")]
	[InlineData("CREATE TABLE t (c BIT (MAX))")]
	[InlineData("CREATE TABLE t (c DATETIME2 (MAX))")]
	[InlineData("CREATE FUNCTION f () RETURNS @t TABLE (c DECIMAL (MAX)) AS BEGIN RETURN END")]
	[InlineData("CREATE TABLE t (c DECIMAL (MAX) COLLATE Latin1_General_CI_AS)")]
	[InlineData("CREATE TABLE t (c DOUBLE PRECISION (MAX))")]
	[InlineData("CREATE TABLE t1 (c1 VARCHAR (MAX))")]
	[InlineData("UPDATE t1 SET @a = c1 -= DEFAULT")]
	[InlineData("UPDATE t1 SET @a = c1 *= DEFAULT")]
	[InlineData("UPDATE t1 SET @a = c1 %= DEFAULT, @b = c2 ^= DEFAULT")]
	[InlineData("MERGE t USING s ON 1 = 1 WHEN MATCHED THEN UPDATE SET @a = c1 -= DEFAULT")]
	[InlineData("UPDATE t1 SET @a = c1 -= DEFAULT FROM t1")]
	[InlineData("UPDATE t1 SET @a = c1 = DEFAULT")]
	[InlineData("UPDATE t1 set @a = c1 += null, @a = c1 -= default, @a = c1 *= 1 + 1, @a = c1 /= 1, @a = c1 %= 1, @a = c1 &= 1, @a = c1 |= 1, @a = c1 ^= 1")]
	[InlineData("SAVE TRANSACTION 5:a.b")]
	[InlineData("SAVE TRANSACTION -5:a.b")]
	[InlineData("SAVE TRANSACTION -100:a.b")]
	[InlineData("SAVE TRANSACTION 5 : a.b")]
	[InlineData("SAVE TRANSACTION 5:[a].[b]")]
	[InlineData("SAVE TRANSACTION 1.5:a.b")]
	[InlineData("SAVE TRAN 5:a.b")]
	[InlineData("BEGIN TRANSACTION 5:a.b")]
	[InlineData("COMMIT TRANSACTION 5:a.b")]
	[InlineData("ROLLBACK TRANSACTION 5:a.b")]
	[InlineData("BEGIN TRAN 5:a.b WITH MARK 'x'")]
	[InlineData("BEGIN DISTRIBUTED TRANSACTION 5:a.b")]
	[InlineData("SAVE TRANSACTION 5:\"a\".\"b\"")]
	[InlineData("COMMIT TRAN -1:x.y")]
	[InlineData("ROLLBACK TRAN 1:x.y")]
	public void Old_forms_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The options a backup and a restore take for a URL, and a certificate's key algorithm, as the engine answers them.</summary>
	[Theory]
	[InlineData("BACKUP DATABASE d TO URL = 'u' WITH BACKUP_OPTIONS = x")]
	[InlineData("BACKUP DATABASE d TO URL = 'u' WITH RESTORE_OPTIONS = '{}'")]
	[InlineData("BACKUP DATABASE d TO URL = 'u' WITH BACKUP_OPTIONS")]
	[InlineData("RESTORE DATABASE d FROM URL = 'u' WITH BACKUP_OPTIONS = '{}'")]
	[InlineData("RESTORE DATABASE d FROM URL = 'u' WITH RESTORE_OPTIONS = x")]
	[InlineData("BACKUP CERTIFICATE c TO FILE = 'f' WITH FORMAT = 'PFX', PRIVATE KEY (ENCRYPTION BY PASSWORD = 'p', ALGORITHM = AES_256)")]
	[InlineData("BACKUP CERTIFICATE c TO FILE = 'f' WITH FORMAT = 'PFX', PRIVATE KEY (ENCRYPTION BY PASSWORD = 'p', ALGORITHM = 'AES_256', ALGORITHM = 'AES_256')")]
	[InlineData("BACKUP CERTIFICATE c TO FILE = 'f' WITH FORMAT = 'PFX', PRIVATE KEY (ENCRYPTION BY PASSWORD = 'p', ALGORITHM = @a)")]
	[InlineData("BACKUP MASTER KEY TO FILE = 'f' ENCRYPTION BY PASSWORD = 'p' ALGORITHM = 'AES_256'")]
	[InlineData("BACKUP SYMMETRIC KEY k TO FILE = 'f' ENCRYPTION BY PASSWORD = 'p' ALGORITHM = 'AES_256'")]
	[InlineData("CREATE CERTIFICATE c FROM FILE = 'f' WITH FORMAT = 'PFX', PRIVATE KEY (DECRYPTION BY PASSWORD = 'p', ALGORITHM = 'AES_256')")]
	[InlineData("CREATE CERTIFICATE c FROM FILE = 'f' WITH PRIVATE KEY (FILE = 'k', DECRYPTION BY PASSWORD = 'p', ALGORITHM = 'AES_256')")]
	[InlineData("ALTER CERTIFICATE c WITH PRIVATE KEY (FILE = 'k', DECRYPTION BY PASSWORD = 'p', ALGORITHM = 'AES_256')")]
	public void Backup_options_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("BACKUP DATABASE d TO URL = 'u' WITH BACKUP_OPTIONS = '{}'")]
	[InlineData("BACKUP DATABASE d TO URL = 'u' WITH BACKUP_OPTIONS = N'{}'")]
	[InlineData("BACKUP DATABASE d TO URL = 'u' WITH BACKUP_OPTIONS = @o")]
	[InlineData("BACKUP DATABASE d TO URL = 'u' WITH BACKUP_OPTIONS = '{}', BACKUP_OPTIONS = '{}'")]
	[InlineData("BACKUP DATABASE d TO URL = 'u' WITH BACKUP_OPTIONS = '{\"s3\": {\"region\":\"us-west-2\"}}', COMPRESSION, FORMAT, MAXTRANSFERSIZE = 20971520")]
	[InlineData("BACKUP LOG d TO URL = 'u' WITH BACKUP_OPTIONS = '{}'")]
	[InlineData("BACKUP DATABASE d TO DISK = 'f' WITH BACKUP_OPTIONS = '{}'")]
	[InlineData("BACKUP DATABASE d TO URL = 'u' WITH BACKUP_OPTIONS = 1")]
	[InlineData("RESTORE DATABASE d FROM URL = 'u' WITH RESTORE_OPTIONS = '{}'")]
	[InlineData("RESTORE DATABASE d FROM URL = 'u' WITH MOVE 'a' TO 'b', MOVE 'c' TO 'd', STATS = 10, RECOVERY, REPLACE, RESTORE_OPTIONS = '{\"s3\": {\"region\":\"us-west-2\"}}'")]
	[InlineData("RESTORE LOG d FROM URL = 'u' WITH RESTORE_OPTIONS = '{}'")]
	[InlineData("RESTORE HEADERONLY FROM URL = 'u' WITH RESTORE_OPTIONS = '{}'")]
	[InlineData("RESTORE FILELISTONLY FROM URL = 'u' WITH RESTORE_OPTIONS = '{}'")]
	[InlineData("RESTORE VERIFYONLY FROM URL = 'u' WITH RESTORE_OPTIONS = '{}'")]
	[InlineData("RESTORE DATABASE d FROM URL = 'u' WITH RESTORE_OPTIONS = @o")]
	[InlineData("RESTORE DATABASE d FROM URL = 'u' WITH RESTORE_OPTIONS = N'{}'")]
	[InlineData("BACKUP CERTIFICATE c TO FILE = 'f' WITH FORMAT = 'PFX', PRIVATE KEY (ENCRYPTION BY PASSWORD = 'p', ALGORITHM = 'AES_256')")]
	[InlineData("BACKUP CERTIFICATE c TO FILE = 'f' WITH FORMAT = 'PFX', PRIVATE KEY (ENCRYPTION BY PASSWORD = 'p', ALGORITHM = 'TRIPLE_DES_3KEY')")]
	[InlineData("BACKUP CERTIFICATE c TO FILE = 'f' WITH FORMAT = 'PFX', PRIVATE KEY (ENCRYPTION BY PASSWORD = 'p', ALGORITHM = 'FOO')")]
	[InlineData("BACKUP CERTIFICATE c TO FILE = 'f' WITH FORMAT = 'PFX', PRIVATE KEY (ENCRYPTION BY PASSWORD = 'p', ALGORITHM = N'AES_256')")]
	[InlineData("BACKUP CERTIFICATE c TO FILE = 'f' WITH FORMAT = 'PFX', PRIVATE KEY (ENCRYPTION BY PASSWORD = 'p', DECRYPTION BY PASSWORD = 'q', ALGORITHM = 'AES_256')")]
	[InlineData("BACKUP CERTIFICATE c TO FILE = 'f' WITH PRIVATE KEY (FILE = 'k', ENCRYPTION BY PASSWORD = 'p', ALGORITHM = 'AES_256')")]
	[InlineData("BACKUP CERTIFICATE c TO FILE = 'f' WITH PRIVATE KEY (ALGORITHM = 'AES_256', FILE = 'k', ENCRYPTION BY PASSWORD = 'p')")]
	[InlineData("BACKUP CERTIFICATE c TO FILE = 'f' WITH FORMAT = 'PFX', PRIVATE KEY (ALGORITHM = 'AES_256')")]
	public void Backup_options_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>A table's body ended by a comma, and a table made of a query without its options, as the engine answers them.</summary>
	[Theory]
	[InlineData("CREATE TABLE t (a INT,,)")]
	[InlineData("CREATE TABLE t (,a INT)")]
	[InlineData("DECLARE @t TABLE (a INT,)")]
	[InlineData("CREATE TYPE tt AS TABLE (a INT,)")]
	[InlineData("ALTER TABLE t ADD a INT,")]
	[InlineData("CREATE FUNCTION f () RETURNS @t TABLE (a INT,) AS BEGIN RETURN END")]
	[InlineData("EXEC p WITH RESULT SETS ((a INT,))")]
	[InlineData("CREATE TABLE t AS SELECT 1 AS a")]
	[InlineData("CREATE TABLE t (a INT) AS SELECT 1")]
	[InlineData("CREATE TABLE t AS SELECT * FROM u")]
	[InlineData("CREATE TABLE #t AS SELECT 1 AS a")]
	public void Table_edges_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("CREATE TABLE dbo.T1 ( column_1 int IDENTITY, column_2 uniqueidentifier, )")]
	[InlineData("CREATE TABLE t (a INT,)")]
	[InlineData("CREATE TABLE t (a INT, PRIMARY KEY (a),)")]
	[InlineData("CREATE TABLE t (a INT, INDEX i (a),)")]
	[InlineData("CREATE TABLE t (a, b) AS SELECT 1, 2")]
	[InlineData("CREATE TABLE t (a) AS SELECT 1")]
	[InlineData("CREATE TABLE t (a, b) AS SELECT 1, 2 UNION SELECT 3, 4")]
	[InlineData("CREATE TABLE t (a, b) AS WITH c AS (SELECT 1 x, 2 y) SELECT * FROM c")]
	[InlineData("CREATE TABLE t (a, b) AS (SELECT 1, 2)")]
	[InlineData("CREATE TABLE t (a, b) AS SELECT 1, 2 OPTION (LABEL = 'x')")]
	[InlineData("CREATE TABLE t (a, b) AS SELECT 1, 2 ORDER BY 1")]
	[InlineData("CREATE TABLE t WITH (DISTRIBUTION = ROUND_ROBIN) AS SELECT 1 AS a")]
	public void Table_edges_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary><c>PREDICT</c> and the schema of its own it must say, as the engine answers them.</summary>
	[Theory]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d, RUNTIME = ONNX) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d, RUNTIME = ONNX) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH () AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT DEFAULT 1) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT PRIMARY KEY) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s AS 1) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT '$.a') AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT) AS p (x)")]
	[InlineData("SELECT * FROM PREDICT(MODEL = (SELECT m FROM ms WHERE id = 4), DATA = t AS d) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT) AS p WITH (NOLOCK)")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT NOT NULL NULL) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT NULL COLLATE Latin1_General_CI_AS) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d, RUNTIME = FOO) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(DATA = t AS d) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT,) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT AS JSON) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH s AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d, FOO = 1) WITH (s FLOAT) AS p")]
	public void Predictions_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT)")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT) p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT, u INT NOT NULL, v NVARCHAR(10) COLLATE Latin1_General_CI_AS NULL) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT, s INT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = 0x01, DATA = t AS d) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = 'm', DATA = t AS d) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = N'm', DATA = t AS d) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = m, DATA = t AS d) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = 1, DATA = t AS d) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM t CROSS APPLY PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT) AS p JOIN u ON 1 = 1")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s dbo.udt) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH ([s] FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = (SELECT 1 x) AS d) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(DATA = t AS d, MODEL = @m) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s VARCHAR(MAX)) AS p")]
	[InlineData("INSERT INTO s (c1, score) SELECT d.c1, p.score FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (score FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT COLLATE Latin1_General_CI_AS) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT NULL) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t AS d) WITH (s FLOAT NOT NULL, u INT NULL) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = dbo.t AS d) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = @tv AS d) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = t d) WITH (s FLOAT) AS p")]
	public void Predictions_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>A secondary XML index, and the kind or the path it says, as the engine answers them.</summary>
	[Theory]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR ('path1')")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR (p1, p2)")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR ()")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR path1")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR FOO")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR (N'path1')")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR (dbo.path1)")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR (path1) ON fg")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX dbo.sxi FOR (p)")]
	[InlineData("CREATE PRIMARY XML INDEX i ON t(c) USING XML INDEX sxi FOR VALUE")]
	[InlineData("CREATE PRIMARY XML INDEX i ON t(c) USING XML INDEX sxi")]
	[InlineData("CREATE PRIMARY XML INDEX i ON t(c) USING XML INDEX sxi FOR (path1)")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR (path1) WITH (ONLINE = ON)")]
	public void Secondary_XML_indexes_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR (path1)")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR ([path1])")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR (VALUE)")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR ( path1 )")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR (PATH)")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR (\"p\")")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR (#p)")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX [sxi] FOR ([p])")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR (path1) WITH (PAD_INDEX = ON)")]
	[InlineData("CREATE XML INDEX i ON dbo.t(c) USING XML INDEX sxi FOR (path1) WITH (FILLFACTOR = 80, MAXDOP = 2)")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR (p) WITH (DROP_EXISTING = ON, ALLOW_ROW_LOCKS = OFF)")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR VALUE")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR PATH")]
	[InlineData("CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR PROPERTY")]
	public void Secondary_XML_indexes_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>A column written without a type, as the engine answers them.</summary>
	[Theory]
	[InlineData("EXEC p WITH RESULT SETS ((a, b))")]
	[InlineData("EXEC p WITH RESULT SETS ((a INT, b))")]
	[InlineData("CREATE TABLE t (timestamp COLLATE Latin1_General_CI_AS)")]
	[InlineData("ALTER TABLE t ALTER COLUMN timestamp")]
	[InlineData("CREATE TABLE t (x GENERATED ALWAYS AS ROW START)")]
	[InlineData("CREATE TABLE t (x MASKED WITH (FUNCTION = 'default()'))")]
	[InlineData("CREATE TABLE t (x NOT FOR REPLICATION)")]
	[InlineData("CREATE TABLE t (x ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = k, ENCRYPTION_TYPE = DETERMINISTIC, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256'))")]
	public void Untyped_columns_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("CREATE TABLE t (timestamp)")]
	[InlineData("CREATE TABLE t (a INT, timestamp)")]
	[InlineData("CREATE TABLE t (timestamp NOT NULL)")]
	[InlineData("CREATE TABLE t (timestamp NOT NULL PRIMARY KEY, CHECK (1 < 2))")]
	[InlineData("CREATE TABLE t (TIMESTAMP)")]
	[InlineData("CREATE TABLE t ([timestamp])")]
	[InlineData("CREATE TABLE t (rowversion)")]
	[InlineData("CREATE TABLE t (timestamp NULL)")]
	[InlineData("CREATE TABLE t (timestamp DEFAULT 1)")]
	[InlineData("CREATE TABLE t (timestamp CONSTRAINT c UNIQUE)")]
	[InlineData("CREATE TABLE t (timestamp, timestamp)")]
	[InlineData("CREATE TABLE t (x)")]
	[InlineData("CREATE TABLE t (a INT, x)")]
	[InlineData("ALTER TABLE t ADD timestamp")]
	[InlineData("DECLARE @t TABLE (timestamp)")]
	[InlineData("CREATE TYPE tt AS TABLE (timestamp)")]
	[InlineData("CREATE TABLE t (\"timestamp\")")]
	[InlineData("CREATE TABLE t (timestamp IDENTITY)")]
	[InlineData("CREATE TABLE t (timestamp ROWGUIDCOL)")]
	[InlineData("CREATE TABLE t (timestamp INDEX i)")]
	[InlineData("CREATE TABLE t (PriKey int PRIMARY KEY, timestamp)")]
	[InlineData("ALTER TABLE t ADD a INT, timestamp")]
	[InlineData("CREATE FUNCTION f () RETURNS @t TABLE (x) AS BEGIN RETURN END")]
	[InlineData("CREATE TABLE t (x NOT NULL CONSTRAINT c DEFAULT 1)")]
	[InlineData("CREATE TABLE t (x) ON [PRIMARY]")]
	[InlineData("CREATE TABLE t (x NULL, y NOT NULL, z DEFAULT 0)")]
	[InlineData("CREATE TABLE t (x PRIMARY KEY CLUSTERED)")]
	[InlineData("CREATE TABLE t (x REFERENCES u (y))")]
	[InlineData("CREATE TABLE t (x FOREIGN KEY REFERENCES u (y))")]
	[InlineData("CREATE TABLE t (x CHECK (x > 0))")]
	[InlineData("CREATE TABLE t (x IDENTITY (1, 1) NOT NULL)")]
	[InlineData("CREATE TABLE t (x, PRIMARY KEY (x))")]
	[InlineData("CREATE TABLE t (x, INDEX i (x))")]
	[InlineData("CREATE TABLE t (x, PERIOD FOR SYSTEM_TIME (a, b))")]
	[InlineData("CREATE TABLE t (period)")]
	[InlineData("CREATE TABLE t (x AS 1, y)")]
	[InlineData("CREATE TABLE t (x SPARSE NULL)")]
	[InlineData("CREATE TABLE t (x FILESTREAM)")]
	public void Untyped_columns_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>A statement's <c>OUTPUT</c> read as the rows an <c>INSERT</c> inserts, as the engine answers it.</summary>
	[Theory]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1 INTO @v) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1)")]
	[InlineData("SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao JOIN t5 ON 1 = 1")]
	[InlineData("INSERT INTO t SELECT * FROM t5, (DELETE t3 OUTPUT deleted.c1) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao GROUP BY c1")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao ORDER BY c1")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao HAVING 1 = 1")]
	[InlineData("INSERT INTO t SELECT DISTINCT c1 FROM (DELETE t3 OUTPUT deleted.c1) AS ao")]
	[InlineData("INSERT INTO t SELECT TOP 1 c1 FROM (DELETE t3 OUTPUT deleted.c1) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao UNION SELECT 1")]
	[InlineData("INSERT INTO t SELECT * FROM ((DELETE t3 OUTPUT deleted.c1)) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (WITH c AS (SELECT 1 x) DELETE t3 OUTPUT deleted.c1) AS ao")]
	[InlineData("UPDATE t SET c = 1 FROM (DELETE t3 OUTPUT deleted.c1) AS ao")]
	[InlineData("DELETE t FROM (DELETE t3 OUTPUT deleted.c1) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (SELECT 1 x) AS d, (DELETE t3 OUTPUT deleted.c1) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao CROSS APPLY (SELECT 1 y) z")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1 OUTPUT deleted.c2) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao WITH (NOLOCK)")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao TABLESAMPLE (10 PERCENT)")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao FOR XML AUTO")]
	[InlineData("INSERT INTO t SELECT * INTO t9 FROM (DELETE t3 OUTPUT deleted.c1) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao WHERE c1 IN (SELECT * FROM (DELETE t4 OUTPUT deleted.c1) AS b)")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1 OPTION (RECOMPILE)) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1;) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao) AS d")]
	public void Changed_rows_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao")]
	[InlineData("INSERT INTO t SELECT c1 FROM (UPDATE t3 SET c1 = 10 OUTPUT inserted.c1) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (INSERT INTO t2 OUTPUT inserted.c1 SELECT * FROM t4) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (MERGE INTO t2 USING t1 ON (t2.a = t1.a) WHEN MATCHED THEN DELETE OUTPUT $action) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) ao")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao (x)")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao WHERE x = 1")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao WHERE x = 1 OPTION (RECOMPILE)")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao WHERE EXISTS (SELECT 1)")]
	[InlineData("INSERT INTO t (c1) SELECT c1 FROM (DELETE t3 OUTPUT deleted.c1) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE TOP (1) t3 OUTPUT deleted.c1 WHERE c1 = 1) AS ao")]
	[InlineData("WITH c AS (SELECT 1 x) INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao")]
	[InlineData("INSERT INTO t SELECT ao.* FROM (DELETE t3 OUTPUT deleted.*) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (UPDATE t3 SET c1 = 1 OUTPUT inserted.c1 FROM t3 JOIN t4 ON 1 = 1) AS ao")]
	[InlineData("INSERT t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao")]
	[InlineData("INSERT INTO t WITH (TABLOCK) SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao")]
	[InlineData("INSERT INTO t OUTPUT inserted.c1 SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1) AS [ao] ([x])")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE FROM t3 OUTPUT deleted.c1 FROM t3 AS x WHERE x.c1 = 1) AS ao")]
	[InlineData("INSERT INTO t SELECT * FROM (DELETE t3 OUTPUT deleted.c1 AS c) AS ao")]
	[InlineData("INSERT INTO Production.ZeroInventory (DeletedProductID, RemovedOnDate) SELECT ProductID, GETDATE() FROM (MERGE Production.ProductInventory AS pi USING (SELECT ProductID, SUM(OrderQty) FROM Sales.SalesOrderDetail AS sod JOIN Sales.SalesOrderHeader AS soh ON sod.SalesOrderID = soh.SalesOrderID AND soh.OrderDate = '20070401' GROUP BY ProductID) AS src (ProductID, OrderQty) ON (pi.ProductID = src.ProductID) WHEN MATCHED AND pi.Quantity - src.OrderQty <= 0 THEN DELETE WHEN MATCHED THEN UPDATE SET pi.Quantity = pi.Quantity - src.OrderQty OUTPUT $action, deleted.ProductID) AS Changes (Action, ProductID) WHERE Action = 'DELETE'")]
	public void Changed_rows_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>A statement takes the empty ones after it, as a text of statements does.</summary>
	[Theory]
	[InlineData("SELECT 1;;")]
	[InlineData("SELECT 1; ;")]
	[InlineData("sp_who ; ;")]
	[InlineData("INSERT INTO likes($edge_id, $from_id, $to_id, rating) SELECT 1, 2, 3, 4 FROM OPENROWSET (BULK 'f.csv', DATA_SOURCE = 'ds', FORMATFILE = 'f.xml', FORMATFILE_DATA_SOURCE = 'fs', FIRSTROW = 2) AS staging_data;\n;")]
	public void A_statement_takes_the_empty_ones_after_it(string input) =>
		Assert.True(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>A spatial index and the list of options it reads as no other list is read, as the engine answers them.</summary>
	[Theory]
	[InlineData("CREATE SPATIAL INDEX i ON t(g, h)")]
	[InlineData("CREATE UNIQUE SPATIAL INDEX i ON t(g)")]
	[InlineData("CREATE SPATIAL INDEX i ON @t(g)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g ASC)")]
	[InlineData("CREATE SPATIAL INDEX i ON t()")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WHERE g IS NOT NULL")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) INCLUDE (h)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (0, 0, 500, 200)) FILESTREAM_ON fg")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING 'GEOMETRY_GRID'")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING GEOMETRY_GRID, GEOGRAPHY_GRID")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING GEOMETRY_GRID USING GEOMETRY_GRID")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) ON fg USING GEOMETRY_GRID")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) ON fg WITH (FOO = 1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING GEOMETRY_GRID WITH (BOUNDING_BOX = (0, 0, 1, 1)) ON fg (c)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = 80) ON fg WITH (PAD_INDEX = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING GEOGRAPHY_GRID WITH ()")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH BOUNDING_BOX = (0, 0, 500, 200)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (PAD_INDEX = ON,)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (, PAD_INDEX = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (PAD_INDEX ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO (x = 1))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH ([FOO] = 1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (\"FOO\" = 1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (CELLS_PER_OBJECT = 64 BOUNDING_BOX = (0, 0, 500, 200))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (0, 0, 500, 200) CELLS_PER_OBJECT = 64)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (ONLINE = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (ONLINE = on)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (IGNORE_DUP_KEY = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (RESUMABLE = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (OPTIMIZE_FOR_SEQUENTIAL_KEY = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (IGNORE_DUP_KEY = ON (SUPPRESS_MESSAGES = OFF))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (ONLINE = OFF (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1, ABORT_AFTER_WAIT = NONE)))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (XML_COMPRESSION = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (XML_COMPRESSION = OFF)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (XML_COMPRESSION = FOO)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (STATISTICS_INCREMENTAL = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (STATISTICS_INCREMENTAL = OFF)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = 101)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = 0)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = -1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = -0)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = '50')")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = 1e1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = x)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = NULL)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = (50))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = $50)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = 0x32)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = -1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = 32768)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = 1.5)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = '1')")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = x)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = (1))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = $1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = 0x01)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = FOO)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = 'PAGE')")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = 1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = (PAGE))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = PAGE ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = PAGE ON PARTITIONS (1))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (CELLS_PER_OBJECT = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = OFF)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = ())")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = ())")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (xmin = 0, 0, 500, 200))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = (LEVEL_1 = LOW, MEDIUM))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = (LEVEL_1 = LOW LEVEL_2 = LOW))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (1 + 1, 0, 500, 200))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (@x, 0, 500, 200))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (CELLS_PER_OBJECT = @x)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = @x)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = (@x))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = DEFAULT)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = x.y)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = a b)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = (a b))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = ((1)))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = (x = (1)))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = (1, (2)))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = +1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = -x)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = (-x))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = (ON))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = (x = ON))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = NULL)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = (OFF))")]
	[InlineData("CREATE SPATIAL INDEX i ON t($node_id)")]
	public void Spatial_indexes_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("CREATE SPATIAL INDEX i ON t(g)")]
	[InlineData("CREATE SPATIAL INDEX i ON dbo.t(g)")]
	[InlineData("CREATE SPATIAL INDEX i ON db.dbo.t(g)")]
	[InlineData("CREATE SPATIAL INDEX i ON srv.db.dbo.t(g)")]
	[InlineData("CREATE SPATIAL INDEX [i] ON [t]([g]) USING GEOGRAPHY_GRID")]
	[InlineData("CREATE SPATIAL INDEX i ON dbo.t([g])")]
	[InlineData("CREATE SPATIAL INDEX i ON t (g)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING GEOMETRY_AUTO_GRID")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING GEOMETRY_GRID")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING GEOGRAPHY_AUTO_GRID")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING FOO")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING geometry_grid")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING [GEOMETRY_GRID]")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) ON [PRIMARY]")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) ON \"default\"")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (0, 0, 500, 200)) ON 'fg'")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING GEOMETRY_GRID ON fg")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING FOO WITH (FOO = 1) ON fg")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING GEOMETRY_AUTO_GRID WITH (BOUNDING_BOX = (0, 0, 500, 200))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING GEOMETRY_GRID WITH (BOUNDING_BOX = (0, 0, 500, 200), GRIDS = (LOW, LOW, MEDIUM, HIGH), CELLS_PER_OBJECT = 64, PAD_INDEX = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING GEOMETRY_GRID WITH (BOUNDING_BOX = (xmin = 0, ymin = 0, xmax = 500, ymax = 200), GRIDS = (LEVEL_4 = HIGH, LEVEL_3 = MEDIUM))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING GEOGRAPHY_GRID WITH (GRIDS = (MEDIUM, LOW, MEDIUM, HIGH), CELLS_PER_OBJECT = 64, PAD_INDEX = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) USING GEOGRAPHY_AUTO_GRID WITH (BOUNDING_BOX = (0, 0, 1, 1))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (CELLS_PER_OBJECT = 64) ON fg")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = PAGE, MAXDOP = 2, ONLINE = OFF, IGNORE_DUP_KEY = OFF)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (PAD_INDEX = ON, FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = 1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = BAR)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = 'x', BAR = N'y', BAZ = 0x01, QUX = 1.5, QUUX = -1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = NULL)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = $1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = 1e3)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = [x])")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = \"x\")")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = [ON])")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = (x = y))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = (x = 'y'))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = (x = -1))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = ($1))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = (NULL))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = (0x01))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = (LOW, LOW, LOW))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = (LEVEL_5 = LOW))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = (low, medium, high, low))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = (LOW))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = (FOO, LOW, LOW, LOW))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = (LOW, LOW, LOW, LOW, LOW))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = (LEVEL_1 = FOO))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = (LEVEL_1 = 1))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = (1, 2, 3, 4))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = LOW)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = (LEVEL_1 = LOW, LEVEL_1 = HIGH))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = (LEVEL_1 = LOW, LEVEL_2 = MEDIUM, LEVEL_3 = HIGH, LEVEL_4 = LOW), CELLS_PER_OBJECT = 16)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (0, 0, 500))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (0, 0, 500, 200, 1))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (-1.5, 0, 5e2, 200))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (-1, -2, 3, 4))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (ymin = 0, xmin = 0, ymax = 200, xmax = 500))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (xmin = -1, ymin = -2, xmax = 3, ymax = 4))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (xmin = 0, ymin = 0, xmax = 500))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (foo = 0, ymin = 0, xmax = 500, ymax = 1))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (a, 0, 500, 200))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = ('0', 0, 500, 200))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = 1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (0, 0, 500, 200), BOUNDING_BOX = (0, 0, 1, 1))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (GRIDS = (LOW, LOW, MEDIUM, HIGH), BOUNDING_BOX = (0, 0, 500, 200))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BOUNDING_BOX = (0, 0, 500, 200), CELLS_PER_OBJECT = 64)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (CELLS_PER_OBJECT = 64, CELLS_PER_OBJECT = 16)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (CELLS_PER_OBJECT = '64')")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (CELLS_PER_OBJECT = (64))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (CELLS_PER_OBJECT = 8193)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (CELLS_PER_OBJECT = 0)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (CELLS_PER_OBJECT = -1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (CELLS_PER_OBJECT = 1.5)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (CELLS_PER_OBJECT = x)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (CELLS_PER_OBJECT = NULL)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (CELLS_PER_OBJECT = 1, FOO = BAR, GRIDS = (LEVEL_2 = x), BOUNDING_BOX = (a, b, c, d))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = 100)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = 1.5)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = 101.5)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = 0.5)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = 00100)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = 50, FILLFACTOR = 60)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = 0)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = -0)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = 64)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = 65)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = 32767)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = NONE)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = ROW)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = none)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = COLUMNSTORE)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = COLUMNSTORE_ARCHIVE)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = [PAGE])")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = ROW, DATA_COMPRESSION = PAGE)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = 0, DATA_COMPRESSION = ROW)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (IGNORE_DUP_KEY = OFF)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (ONLINE = OFF)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (ONLINE = FOO)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (ONLINE = (1))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (ONLINE = [ON])")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (RESUMABLE = OFF)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (RESUMABLE = FOO)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (SORT_IN_TEMPDB = FOO)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (STATISTICS_NORECOMPUTE = FOO)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (ALLOW_ROW_LOCKS = FOO)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (PAD_INDEX = 1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (PAD_INDEX = (1))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (PAD_INDEX = NULL)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (PAD_INDEX = 'ON')")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (PAD_INDEX = [ON])")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (PAD_INDEX = ON, PAD_INDEX = OFF)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (PAD_INDEX = ON, PAD_INDEX = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DROP_EXISTING = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAX_DURATION = 1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (COMPRESSION_DELAY = 1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (BUCKET_COUNT = 1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = -$1)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = (-1.5, -$1))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = (N'x', 1e3))")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = #x)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = OFF)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (MAXDOP = 00064)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = -1.5)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FILLFACTOR = 100.0)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = \"PAGE\")")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DATA_COMPRESSION = [COLUMNSTORE])")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (SORT_IN_TEMPDB = OFF)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (DROP_EXISTING = OFF)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (ALLOW_PAGE_LOCKS = OFF, ALLOW_ROW_LOCKS = OFF, STATISTICS_NORECOMPUTE = ON)")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (IGNORE_DUP_KEY = [ON], RESUMABLE = [ON], OPTIMIZE_FOR_SEQUENTIAL_KEY = [ON])")]
	[InlineData("CREATE SPATIAL INDEX i ON t(g) WITH (FOO = (x = N'y', z = 0x01, w = NULL, v = $1))")]
	public void Spatial_indexes_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Text read and written through its pointer, a configuration taken up and the service master key altered, as the engine answers them.</summary>
	[Theory]
	[InlineData("READTEXT c @p 0 1")]
	[InlineData("READTEXT srv.db.dbo.t.c @p 0 1")]
	[InlineData("READTEXT t.c NULL 0 1")]
	[InlineData("READTEXT t.c @@SPID 0 1")]
	[InlineData("READTEXT t.c @p -1 1")]
	[InlineData("READTEXT t.c @p 0 -1")]
	[InlineData("READTEXT t.c @p 1 + 1 1")]
	[InlineData("READTEXT t.c @p 0, 1")]
	[InlineData("READTEXT t.c @p 0")]
	[InlineData("READTEXT t.c @p 0 1 NOLOCK")]
	[InlineData("READTEXT t.c @p '0' 1")]
	[InlineData("READTEXT t.c @p 0x0 1")]
	[InlineData("READTEXT t.c (SELECT 0x01) 0 1")]
	[InlineData("READTEXT t.c @p (0) 1")]
	[InlineData("READTEXT t.c @p 0 1e1")]
	[InlineData("READTEXT t.c @p $1 1")]
	[InlineData("READTEXT t.c @p @@SPID 1")]
	[InlineData("READTEXT t.c @p 0 1 HOLDLOCK HOLDLOCK")]
	[InlineData("READTEXT t.c @p 0 NULL")]
	[InlineData("WRITETEXT BULK t.c @p 'x'")]
	[InlineData("WRITETEXT BULK t.c @p WITH LOG 'x'")]
	[InlineData("WRITETEXT t.c @p")]
	[InlineData("WRITETEXT t.c @p 1")]
	[InlineData("WRITETEXT t.c @p 'a' + 'b'")]
	[InlineData("WRITETEXT c @p 'x'")]
	[InlineData("WRITETEXT t.c @p x")]
	[InlineData("WRITETEXT t.c @p ('x')")]
	[InlineData("WRITETEXT t.c @p WITH 'x'")]
	[InlineData("WRITETEXT t.c @p $1")]
	[InlineData("WRITETEXT t.c @p -1")]
	[InlineData("WRITETEXT t.c @p @@SPID")]
	[InlineData("WRITETEXT t.c @p {d '2020-01-01'}")]
	[InlineData("WRITETEXT t.c @p DEFAULT")]
	[InlineData("WRITETEXT t.c NULL 'x'")]
	[InlineData("WRITETEXT t.c @@SPID 'x'")]
	[InlineData("WRITETEXT srv.db.dbo.t.c @p 'x'")]
	[InlineData("UPDATETEXT t.c @p 0 1 'a' + 'b'")]
	[InlineData("UPDATETEXT t.c @p 0")]
	[InlineData("UPDATETEXT t.c @p 0 1 1")]
	[InlineData("UPDATETEXT c @p 0 1 'x'")]
	[InlineData("UPDATETEXT t.c @p 1 + 1 1 'x'")]
	[InlineData("UPDATETEXT t.c @p 0 1 t2.c2")]
	[InlineData("UPDATETEXT BULK t.c @p 0 1 'x'")]
	[InlineData("UPDATETEXT t.c @p $1 NULL")]
	[InlineData("UPDATETEXT t.c @p 0 1e1")]
	[InlineData("UPDATETEXT t.c @p 0 1 c2 @p")]
	[InlineData("UPDATETEXT t.c @p @@SPID 1")]
	[InlineData("UPDATETEXT t.c @p 0 1 $1")]
	[InlineData("UPDATETEXT t.c @p 0 1 x")]
	[InlineData("UPDATETEXT t.c NULL 0 1 'x'")]
	[InlineData("UPDATETEXT srv.db.dbo.t.c @p 0 1 'x'")]
	[InlineData("RECONFIGURE WITH FOO")]
	[InlineData("RECONFIGURE OVERRIDE")]
	[InlineData("RECONFIGURE WITH OVERRIDE, OVERRIDE")]
	[InlineData("ALTER SERVICE MASTER KEY")]
	[InlineData("ALTER SERVICE MASTER KEY WITH OLD_PASSWORD = 'p', OLD_ACCOUNT = 'a'")]
	[InlineData("ALTER SERVICE MASTER KEY WITH NEW_PASSWORD = 'p', NEW_ACCOUNT = 'a'")]
	[InlineData("ALTER SERVICE MASTER KEY WITH OLD_ACCOUNT = 'a'")]
	[InlineData("ALTER SERVICE MASTER KEY WITH OLD_ACCOUNT = 'a', NEW_PASSWORD = 'p'")]
	[InlineData("ALTER SERVICE MASTER KEY WITH OLD_ACCOUNT = @a, OLD_PASSWORD = 'p'")]
	[InlineData("ALTER SERVICE MASTER KEY FORCE")]
	[InlineData("ALTER SERVICE MASTER KEY REGENERATE WITH OLD_ACCOUNT = 'a', OLD_PASSWORD = 'p'")]
	[InlineData("ALTER SERVICE MASTER KEY WITH OLD_ACCOUNT = a, OLD_PASSWORD = 'p'")]
	[InlineData("ALTER SERVICE MASTER KEY WITH OLD_ACCOUNT = 'a' OLD_PASSWORD = 'p'")]
	[InlineData("ALTER SERVICE MASTER KEY WITH OLD_ACCOUNT = 'a', OLD_PASSWORD = 'p', OLD_ACCOUNT = 'b'")]
	[InlineData("ALTER SERVICE MASTER KEY REGENERATE FORCE")]
	public void Text_and_configuration_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("READTEXT t.c @p 0 1")]
	[InlineData("READTEXT t.c @p 0 1 HOLDLOCK")]
	[InlineData("READTEXT dbo.t.c @p 0 1")]
	[InlineData("READTEXT db.dbo.t.c @p 0 1")]
	[InlineData("READTEXT [t].[c] @p 0 1")]
	[InlineData("READTEXT t.c 0x01 0 1")]
	[InlineData("READTEXT t.c 0x0102 0 1")]
	[InlineData("READTEXT t.c @p @o @s")]
	[InlineData("READTEXT t.c @p 0 1.5")]
	[InlineData("READTEXT t.c @p 1.5 1")]
	[InlineData("WRITETEXT t.c @p 'x'")]
	[InlineData("WRITETEXT BULK t.c @p")]
	[InlineData("WRITETEXT BULK t.c @p WITH LOG")]
	[InlineData("WRITETEXT t.c @p WITH LOG 'x'")]
	[InlineData("WRITETEXT t.c @p N'x'")]
	[InlineData("WRITETEXT t.c @p @d")]
	[InlineData("WRITETEXT t.c @p 0x01")]
	[InlineData("WRITETEXT t.c @p NULL")]
	[InlineData("WRITETEXT t.c @p WITH LOG NULL")]
	[InlineData("WRITETEXT t.c 0x01 'x'")]
	[InlineData("WRITETEXT dbo.t.c @p 'x'")]
	[InlineData("WRITETEXT db.dbo.t.c @p 'x'")]
	[InlineData("WRITETEXT [t].[c] @p 'x'")]
	[InlineData("UPDATETEXT t.c @p 0 NULL 'x'")]
	[InlineData("UPDATETEXT t.c @p NULL NULL")]
	[InlineData("UPDATETEXT t.c @p 0 1")]
	[InlineData("UPDATETEXT t.c @p 0 1 WITH LOG 'x'")]
	[InlineData("UPDATETEXT t.c @p 0 1 WITH LOG")]
	[InlineData("UPDATETEXT BULK t.c @p 0 1")]
	[InlineData("UPDATETEXT BULK t.c @p 0 1 WITH LOG")]
	[InlineData("UPDATETEXT t.c @p 0 1 t2.c2 @q")]
	[InlineData("UPDATETEXT t.c @p 0 1 WITH LOG t2.c2 @q")]
	[InlineData("UPDATETEXT t.c @p 0 1 dbo.t2.c2 @q")]
	[InlineData("UPDATETEXT t.c @p 0 1 db.dbo.t2.c2 @q")]
	[InlineData("UPDATETEXT t.c @p NULL 0 WITH LOG t2.c2 0x01")]
	[InlineData("UPDATETEXT t.c @p 0 1 @d")]
	[InlineData("UPDATETEXT t.c @p 0 1 0x01")]
	[InlineData("UPDATETEXT t.c @p 0 1 N'x'")]
	[InlineData("UPDATETEXT t.c @p @o @l 'x'")]
	[InlineData("UPDATETEXT t.c @p -1 1 'x'")]
	[InlineData("UPDATETEXT t.c @p - 1 1")]
	[InlineData("UPDATETEXT t.c @p 0 -1")]
	[InlineData("UPDATETEXT t.c @p 1.5 NULL")]
	[InlineData("UPDATETEXT t.c @p 0 1 NULL")]
	[InlineData("UPDATETEXT t.c @p 0 1 WITH LOG NULL")]
	[InlineData("UPDATETEXT dbo.t.c @p 0 1 'x'")]
	[InlineData("UPDATETEXT t.c 0x01 0 1 'x'")]
	[InlineData("RECONFIGURE")]
	[InlineData("RECONFIGURE WITH OVERRIDE")]
	[InlineData("RECONFIGURE;")]
	[InlineData("RECONFIGURE WITH override")]
	[InlineData("ALTER SERVICE MASTER KEY REGENERATE")]
	[InlineData("ALTER SERVICE MASTER KEY FORCE REGENERATE")]
	[InlineData("ALTER SERVICE MASTER KEY WITH OLD_ACCOUNT = 'a', OLD_PASSWORD = 'p'")]
	[InlineData("ALTER SERVICE MASTER KEY WITH NEW_ACCOUNT = 'a', NEW_PASSWORD = 'p'")]
	[InlineData("ALTER SERVICE MASTER KEY WITH OLD_ACCOUNT = N'a', OLD_PASSWORD = N'p'")]
	public void Text_and_configuration_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Assemblies, cryptographic providers, external languages, rules, defaults, aggregates and signatures, as the engine answers them.</summary>
	[Theory]
	[InlineData("CREATE ASSEMBLY dbo.a FROM 0x01")]
	[InlineData("CREATE ASSEMBLY a FROM 'a' WITH PERMISSION_SET = FOO")]
	[InlineData("CREATE ASSEMBLY a FROM 'a' WITH VISIBILITY = ON")]
	[InlineData("CREATE ASSEMBLY a FROM 'a' WITH UNCHECKED DATA")]
	[InlineData("CREATE ASSEMBLY a FROM 'a' WITH PERMISSION_SET = SAFE, PERMISSION_SET = SAFE")]
	[InlineData("CREATE ASSEMBLY a AUTHORIZATION dbo.x FROM 'a'")]
	[InlineData("CREATE ASSEMBLY a FROM")]
	[InlineData("CREATE ASSEMBLY a")]
	[InlineData("CREATE ASSEMBLY a FROM 'a' AS 'b'")]
	[InlineData("ALTER ASSEMBLY a")]
	[InlineData("ALTER ASSEMBLY dbo.a FROM 'a'")]
	[InlineData("ALTER ASSEMBLY a FROM 0x01, 0x02")]
	[InlineData("ALTER ASSEMBLY a WITH UNCHECKED")]
	[InlineData("ALTER ASSEMBLY a WITH UNCHECKED DATA, UNCHECKED DATA")]
	[InlineData("ALTER ASSEMBLY a WITH PERMISSION_SET = SAFE, PERMISSION_SET = UNSAFE")]
	[InlineData("ALTER ASSEMBLY a WITH VISIBILITY = ON FROM 'a.dll'")]
	[InlineData("ALTER ASSEMBLY a DROP FILE ALL, 'x'")]
	[InlineData("ALTER ASSEMBLY a DROP FILE x")]
	[InlineData("ALTER ASSEMBLY a ADD FILE FROM 'a' AS f")]
	[InlineData("ALTER ASSEMBLY a ADD FILE FROM 'a' DROP FILE ALL")]
	[InlineData("ALTER ASSEMBLY a AUTHORIZATION dbo FROM 'a'")]
	[InlineData("CREATE CRYPTOGRAPHIC PROVIDER p")]
	[InlineData("CREATE CRYPTOGRAPHIC PROVIDER dbo.p FROM FILE = 'x'")]
	[InlineData("CREATE CRYPTOGRAPHIC PROVIDER p FROM FILE = 'x' + 'y'")]
	[InlineData("CREATE CRYPTOGRAPHIC PROVIDER p FROM FILE = @f")]
	[InlineData("ALTER CRYPTOGRAPHIC PROVIDER p")]
	[InlineData("ALTER CRYPTOGRAPHIC PROVIDER p FROM FILE = 'x' ENABLE")]
	[InlineData("ALTER CRYPTOGRAPHIC PROVIDER dbo.p ENABLE")]
	[InlineData("CREATE EXTERNAL LANGUAGE dbo.Java FROM (CONTENT = 'a', FILE_NAME = 'x')")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = 'a')")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (FILE_NAME = 'x')")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = 'a', FILE_NAME = 'x', ENVIRONMENT_VARIABLES = 'e')")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = 'a', FILE_NAME = 'x'), (CONTENT = 'b', FILE_NAME = 'y')")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = 'a', FILE_NAME = 'x', PLATFORM = LINUX), (CONTENT = 'b', FILE_NAME = 'y', PLATFORM = LINUX)")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = 'a', FILE_NAME = 'x', PLATFORM = WINDOWS), (CONTENT = 'b', FILE_NAME = 'y')")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = 'a', FILE_NAME = 'x', PLATFORM = WINDOWS), (CONTENT = 'b', FILE_NAME = 'y', PLATFORM = LINUX), (CONTENT = 'c', FILE_NAME = 'z')")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = 'a', FILE_NAME = x)")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = 'a', FILE_NAME = @f)")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = 'a', FILE_NAME = 'x', PLATFORM = 'LINUX')")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = 'a', FILE_NAME = 'x', PARAMETERS = p)")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM ()")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM CONTENT = 'a', FILE_NAME = 'x'")]
	[InlineData("ALTER EXTERNAL LANGUAGE Java")]
	[InlineData("ALTER EXTERNAL LANGUAGE Java AUTHORIZATION dbo")]
	[InlineData("ALTER EXTERNAL LANGUAGE Java SET (CONTENT = 'a', FILE_NAME = 'x'), (CONTENT = 'b', FILE_NAME = 'y')")]
	[InlineData("ALTER EXTERNAL LANGUAGE Java REMOVE PLATFORM FOO")]
	[InlineData("ALTER EXTERNAL LANGUAGE Java REMOVE PLATFORM")]
	[InlineData("ALTER EXTERNAL LANGUAGE Java SET (ENVIRONMENT_VARIABLES = 'p')")]
	[InlineData("ALTER EXTERNAL LANGUAGE Java SET ()")]
	[InlineData("ALTER EXTERNAL LANGUAGE Java REMOVE (PLATFORM = LINUX)")]
	[InlineData("CREATE RULE r AS 1 = 1")]
	[InlineData("CREATE RULE r AS LEN('a') > 1")]
	[InlineData("CREATE RULE r AS 1 IN (1, 2)")]
	[InlineData("CREATE RULE r AS @@SPID > 1")]
	[InlineData("CREATE RULE r AS @x > 1 OR @y < 2")]
	[InlineData("CREATE RULE r AS @x > 1 AND @x2 < 2")]
	[InlineData("CREATE RULE r AS @x = NEXT VALUE FOR s")]
	[InlineData("CREATE RULE db.dbo.r AS @x > 1")]
	[InlineData("CREATE RULE r @x > 1")]
	[InlineData("CREATE DEFAULT db.dbo.d AS 1")]
	[InlineData("CREATE DEFAULT d AS DEFAULT")]
	[InlineData("CREATE DEFAULT d AS 1 = 1")]
	[InlineData("CREATE DEFAULT d 1")]
	[InlineData("CREATE DEFAULT d AS NEXT VALUE FOR s")]
	[InlineData("CREATE DEFAULT d AS 1 + NEXT VALUE FOR s")]
	[InlineData("CREATE AGGREGATE a () RETURNS INT EXTERNAL NAME asm")]
	[InlineData("CREATE AGGREGATE a (@x INT = 1) RETURNS INT EXTERNAL NAME asm")]
	[InlineData("CREATE AGGREGATE a (@x INT OUTPUT) RETURNS INT EXTERNAL NAME asm")]
	[InlineData("CREATE AGGREGATE a (@x INT READONLY) RETURNS INT EXTERNAL NAME asm")]
	[InlineData("CREATE AGGREGATE a (@x CURSOR VARYING OUTPUT) RETURNS INT EXTERNAL NAME asm")]
	[InlineData("CREATE AGGREGATE a (@x INT) RETURNS INT NULL EXTERNAL NAME asm")]
	[InlineData("CREATE AGGREGATE a (@x INT) RETURNS INT EXTERNAL NAME asm.ns.cls")]
	[InlineData("CREATE AGGREGATE a (@x INT) RETURNS INT")]
	[InlineData("CREATE AGGREGATE a (@x INT) RETURNS TABLE EXTERNAL NAME asm")]
	[InlineData("CREATE AGGREGATE db.dbo.a (@x INT) RETURNS INT EXTERNAL NAME asm")]
	[InlineData("CREATE AGGREGATE a @x INT RETURNS INT EXTERNAL NAME asm")]
	[InlineData("CREATE AGGREGATE a (x INT) RETURNS INT EXTERNAL NAME asm")]
	[InlineData("CREATE AGGREGATE a (@x INT) RETURNS INT WITH EXECUTE AS CALLER EXTERNAL NAME asm")]
	[InlineData("ADD SIGNATURE TO p")]
	[InlineData("ADD SIGNATURE TO p BY CERTIFICATE c WITH SIGNATURE = 'x'")]
	[InlineData("ADD SIGNATURE TO p BY CERTIFICATE c WITH SIGNATURE = 1")]
	[InlineData("ADD SIGNATURE TO p BY CERTIFICATE c WITH SIGNATURE = @s")]
	[InlineData("ADD SIGNATURE TO p BY CERTIFICATE c WITH PASSWORD = 'x', SIGNATURE = 0x01")]
	[InlineData("ADD SIGNATURE TO p BY CERTIFICATE c WITH PASSWORD = 'x' WITH PASSWORD = 'y'")]
	[InlineData("ADD SIGNATURE TO p BY CERTIFICATE c WITH FOO = 'x'")]
	[InlineData("ADD SIGNATURE TO FOO::a BY CERTIFICATE c")]
	[InlineData("ADD SIGNATURE TO TYPE::p BY CERTIFICATE c")]
	[InlineData("ADD SIGNATURE TO SCHEMA::p BY CERTIFICATE c")]
	[InlineData("ADD SIGNATURE TO p BY CERTIFICATE dbo.c")]
	[InlineData("ADD SIGNATURE TO p BY ASYMMETRIC KEY dbo.k")]
	[InlineData("ADD SIGNATURE TO p BY PASSWORD = @x")]
	[InlineData("ADD SIGNATURE TO p BY SYMMETRIC KEY k WITH PASSWORD = 'x'")]
	[InlineData("ADD SIGNATURE TO p BY MASTER KEY")]
	[InlineData("ADD SIGNATURE TO @p BY CERTIFICATE c")]
	[InlineData("ADD SIGNATURE TO 'p' BY CERTIFICATE c")]
	[InlineData("DROP SIGNATURE FROM p BY CERTIFICATE dbo.c")]
	[InlineData("DROP SIGNATURE FROM p BY ASYMMETRIC KEY dbo.k")]
	[InlineData("DROP SIGNATURE FROM TYPE::p BY CERTIFICATE c")]
	[InlineData("DROP SIGNATURE FROM p BY SYMMETRIC KEY k WITH PASSWORD = 'x'")]
	public void Code_objects_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("CREATE ASSEMBLY a FROM 'a.dll'")]
	[InlineData("CREATE ASSEMBLY [a] AUTHORIZATION [dbo] FROM 'a.dll', 0x4D5A WITH PERMISSION_SET = UNSAFE")]
	[InlineData("CREATE ASSEMBLY a FROM N'a.dll' WITH PERMISSION_SET = EXTERNAL_ACCESS")]
	[InlineData("CREATE ASSEMBLY a FROM (SELECT 0x01)")]
	[InlineData("CREATE ASSEMBLY a FROM @bits")]
	[InlineData("CREATE ASSEMBLY a FROM 1 + 1")]
	[InlineData("CREATE ASSEMBLY a FROM NULL")]
	[InlineData("ALTER ASSEMBLY a FROM 'a.dll'")]
	[InlineData("ALTER ASSEMBLY a FROM @b")]
	[InlineData("ALTER ASSEMBLY a FROM NULL")]
	[InlineData("ALTER ASSEMBLY a WITH PERMISSION_SET = SAFE, VISIBILITY = OFF, UNCHECKED DATA")]
	[InlineData("ALTER ASSEMBLY a DROP FILE ALL")]
	[InlineData("ALTER ASSEMBLY a DROP FILE ALL ADD FILE FROM 'a'")]
	[InlineData("ALTER ASSEMBLY a FROM 'a' WITH PERMISSION_SET = SAFE DROP FILE ALL ADD FILE FROM 'b'")]
	[InlineData("ALTER ASSEMBLY a DROP FILE 'x', N'y'")]
	[InlineData("ALTER ASSEMBLY a ADD FILE FROM 'a', 'b' AS 'c'")]
	[InlineData("ALTER ASSEMBLY a ADD FILE FROM 0x01 AS N'f', 'g.cs'")]
	[InlineData("ALTER ASSEMBLY a ADD FILE FROM NULL AS 'x'")]
	[InlineData("CREATE CRYPTOGRAPHIC PROVIDER p FROM FILE = 'x'")]
	[InlineData("CREATE CRYPTOGRAPHIC PROVIDER p FROM FILE = N'x'")]
	[InlineData("ALTER CRYPTOGRAPHIC PROVIDER [p] ENABLE")]
	[InlineData("ALTER CRYPTOGRAPHIC PROVIDER p DISABLE")]
	[InlineData("ALTER CRYPTOGRAPHIC PROVIDER p FROM FILE = N'x'")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = 'a', FILE_NAME = 'x')")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java AUTHORIZATION dbo FROM (FILE_NAME = 'x', CONTENT = 0x01, PLATFORM = WINDOWS)")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = 'a', FILE_NAME = 'x', PLATFORM = LINUX), (CONTENT = 'b', FILE_NAME = 'y', PLATFORM = WINDOWS)")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = 'a', FILE_NAME = 'x'), (CONTENT = 'b', FILE_NAME = 'y', PLATFORM = LINUX)")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = 'a', FILE_NAME = 'x', PLATFORM = WINDOWS, PLATFORM = LINUX), (CONTENT = 'b', FILE_NAME = 'y', PLATFORM = WINDOWS)")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = @c, FILE_NAME = N'x', PARAMETERS = N'p')")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = NULL, FILE_NAME = 'x', CONTENT = 'b', FILE_NAME = 'y')")]
	[InlineData("CREATE EXTERNAL LANGUAGE Java FROM (CONTENT = 'a', FILE_NAME = 'x', PARAMETERS = 'p', PLATFORM = LINUX, CONTENT = 'b')")]
	[InlineData("ALTER EXTERNAL LANGUAGE Java SET (CONTENT = 'a')")]
	[InlineData("ALTER EXTERNAL LANGUAGE Java SET (PLATFORM = LINUX)")]
	[InlineData("ALTER EXTERNAL LANGUAGE Java AUTHORIZATION dbo SET (CONTENT = 'a', FILE_NAME = 'x', PLATFORM = LINUX)")]
	[InlineData("ALTER EXTERNAL LANGUAGE Java ADD (FILE_NAME = 'x')")]
	[InlineData("ALTER EXTERNAL LANGUAGE Java REMOVE PLATFORM LINUX")]
	[InlineData("ALTER EXTERNAL LANGUAGE Java AUTHORIZATION dbo REMOVE PLATFORM WINDOWS")]
	[InlineData("CREATE RULE r AS @x > 1")]
	[InlineData("CREATE RULE dbo.r AS @x IN (1, 2) AND @X < 5")]
	[InlineData("CREATE RULE r AS @x LIKE 'a%'")]
	[InlineData("CREATE RULE r AS 1 > @x")]
	[InlineData("CREATE RULE r AS 1 = 1 AND @x = 1")]
	[InlineData("CREATE RULE r AS EXISTS (SELECT 1)")]
	[InlineData("CREATE RULE r AS NOT EXISTS (SELECT 1)")]
	[InlineData("CREATE RULE r AS (SELECT 1) = 1")]
	[InlineData("CREATE RULE r AS @x = 1 AND EXISTS (SELECT @y)")]
	[InlineData("CREATE RULE r AS @x IN (SELECT c FROM t WHERE c = @x)")]
	[InlineData("CREATE RULE r AS @x > @@SPID")]
	[InlineData("CREATE RULE r AS (@x > 1)")]
	[InlineData("CREATE RULE r AS @x BETWEEN 1 AND 2")]
	[InlineData("CREATE RULE r AS @x IS NULL")]
	[InlineData("CREATE RULE r AS LEN(@x) > 1")]
	[InlineData("CREATE RULE r AS CASE WHEN @x > 1 THEN 1 ELSE 0 END = 1")]
	[InlineData("CREATE RULE r AS @x > 1 AND EXISTS (SELECT NEXT VALUE FOR s)")]
	[InlineData("CREATE DEFAULT d AS 1")]
	[InlineData("CREATE DEFAULT dbo.d AS GETDATE()")]
	[InlineData("CREATE DEFAULT d AS NULL")]
	[InlineData("CREATE DEFAULT d AS @v")]
	[InlineData("CREATE DEFAULT d AS @@SPID")]
	[InlineData("CREATE DEFAULT d AS (SELECT 1)")]
	[InlineData("CREATE DEFAULT d AS 'a' + 'b'")]
	[InlineData("CREATE DEFAULT d AS -1")]
	[InlineData("CREATE DEFAULT d AS x")]
	[InlineData("CREATE DEFAULT d AS (SELECT NEXT VALUE FOR s)")]
	[InlineData("CREATE AGGREGATE a (@x INT) RETURNS INT EXTERNAL NAME asm.cls")]
	[InlineData("CREATE AGGREGATE a (@x INT NULL) RETURNS INT EXTERNAL NAME asm")]
	[InlineData("CREATE AGGREGATE dbo.a (@x AS dbo.udt NOT NULL, @y NVARCHAR(10) NULL) RETURNS dbo.udt EXTERNAL NAME [asm].[ns.cls]")]
	[InlineData("CREATE AGGREGATE a (@x INT) RETURNS INT EXTERNAL NAME [asm]")]
	[InlineData("ADD SIGNATURE TO p BY CERTIFICATE c")]
	[InlineData("ADD COUNTER SIGNATURE TO OBJECT::dbo.p BY CERTIFICATE c WITH PASSWORD = N'x'")]
	[InlineData("ADD SIGNATURE TO ASSEMBLY::a BY ASYMMETRIC KEY k WITH SIGNATURE = 0x01")]
	[InlineData("ADD SIGNATURE TO DATABASE::p BY PASSWORD = 'x', CERTIFICATE c")]
	[InlineData("ADD SIGNATURE TO OBJECT :: p BY CERTIFICATE c")]
	[InlineData("ADD SIGNATURE TO db.dbo.p BY SYMMETRIC KEY k")]
	[InlineData("ADD SIGNATURE TO [p] BY CERTIFICATE [c], ASYMMETRIC KEY k")]
	[InlineData("ADD SIGNATURE TO p BY CERTIFICATE c WITH SIGNATURE = 0x")]
	[InlineData("ADD SIGNATURE TO OBJECT::p BY CERTIFICATE c WITH PASSWORD = 'x', CERTIFICATE d")]
	[InlineData("ADD SIGNATURE TO p BY ASYMMETRIC KEY k WITH PASSWORD = 'x'")]
	[InlineData("DROP SIGNATURE FROM ASSEMBLY::a BY CERTIFICATE c")]
	[InlineData("DROP SIGNATURE FROM OBJECT::dbo.p BY CERTIFICATE c")]
	[InlineData("DROP COUNTER SIGNATURE FROM p BY PASSWORD = 'x'")]
	[InlineData("DROP SIGNATURE FROM p BY ASYMMETRIC KEY k WITH PASSWORD = 'x'")]
	[InlineData("DROP SIGNATURE FROM p BY SYMMETRIC KEY k")]
	[InlineData("DROP SIGNATURE FROM p BY CERTIFICATE c WITH SIGNATURE = 0x01")]
	public void Code_objects_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>A procedure's arguments, and its call without the word, as the engine answers them.</summary>
	[Theory]
	[InlineData("EXEC p 1 + 1")]
	[InlineData("EXEC p (1)")]
	[InlineData("EXEC p +1")]
	[InlineData("EXEC p -0x01")]
	[InlineData("EXEC p 'a' COLLATE Latin1_General_CI_AS")]
	[InlineData("EXEC p x.y")]
	[InlineData("EXEC p GETDATE()")]
	[InlineData("EXEC p CAST(1 AS INT)")]
	[InlineData("EXEC p @a + 1")]
	[InlineData("EXEC p -@a")]
	[InlineData("EXEC p - @a")]
	[InlineData("EXEC p -x")]
	[InlineData("EXEC p -'a'")]
	[InlineData("EXEC p -NULL")]
	[InlineData("EXEC p -+1")]
	[InlineData("EXEC p +$1")]
	[InlineData("EXEC p ~1")]
	[InlineData("EXEC p 'a' + 'b'")]
	[InlineData("EXEC p @x = 1 + 1")]
	[InlineData("EXEC p @x = x.y")]
	[InlineData("EXEC p 1 OUTPUT")]
	[InlineData("EXEC p x OUTPUT")]
	[InlineData("EXEC p NULL OUTPUT")]
	[InlineData("EXEC p DEFAULT OUTPUT")]
	[InlineData("EXEC p N'x' OUTPUT")]
	[InlineData("EXEC p [x] OUTPUT")]
	[InlineData("EXEC p @x = DEFAULT OUTPUT")]
	[InlineData("EXEC p CURRENT_TIMESTAMP")]
	[InlineData("EXEC p USER")]
	[InlineData("EXEC p *")]
	[InlineData("p 1 AT srv")]
	[InlineData("p AT srv")]
	[InlineData("p (1)")]
	[InlineData("p 1 + 1")]
	[InlineData("p 'a' 'b'")]
	[InlineData("p 1,")]
	[InlineData("p, 1")]
	[InlineData("p WITH")]
	[InlineData("p OPTION (RECOMPILE)")]
	[InlineData("p 1 OUTPUT")]
	public void Procedure_calls_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("EXEC p -1")]
	[InlineData("EXEC p - 1")]
	[InlineData("EXEC p 1.5e3")]
	[InlineData("EXEC p -1.5e3")]
	[InlineData("EXEC p .5")]
	[InlineData("EXEC p 1.")]
	[InlineData("EXEC p $1")]
	[InlineData("EXEC p -$1")]
	[InlineData("EXEC p $-1")]
	[InlineData("EXEC p £1")]
	[InlineData("EXEC p 0x")]
	[InlineData("EXEC p 0x1")]
	[InlineData("EXEC p N'x'")]
	[InlineData("EXEC p [x]")]
	[InlineData("EXEC p \"x\"")]
	[InlineData("EXEC p TRUE")]
	[InlineData("EXEC p @@SPID")]
	[InlineData("EXEC p {d '2020-01-01'}")]
	[InlineData("EXEC p @x = x")]
	[InlineData("EXEC p @x = [y]")]
	[InlineData("EXEC p @x = DEFAULT")]
	[InlineData("EXEC p @x = NULL")]
	[InlineData("EXEC p @x = -1")]
	[InlineData("EXEC p @x = @a OUT")]
	[InlineData("EXEC p @x = @@SPID")]
	[InlineData("EXEC p @a OUTPUT")]
	[InlineData("EXEC p 1, DEFAULT, NULL, x, @a, N'x', 0x01, $1.5")]
	[InlineData("sp_who")]
	[InlineData("sp_who;")]
	[InlineData("dbo.sp_who 1")]
	[InlineData("db.dbo.p 1, 2")]
	[InlineData("srv.db.dbo.p 1")]
	[InlineData("p..q 1")]
	[InlineData("sp_detach_db Archive")]
	[InlineData("sp_detach_db Archive, 'true'")]
	[InlineData("p @a OUTPUT")]
	[InlineData("p @x = @a OUTPUT")]
	[InlineData("p @a")]
	[InlineData("p @x = 1")]
	[InlineData("@r = p 1")]
	[InlineData("@p 1")]
	[InlineData("p x")]
	[InlineData("p DEFAULT")]
	[InlineData("p -1")]
	[InlineData("p N'x', 0x01, 1.5, $1, NULL")]
	[InlineData("p 1 WITH RECOMPILE")]
	[InlineData("p 1 WITH RECOMPILE, RESULT SETS NONE")]
	[InlineData("[p] 1")]
	[InlineData("\"p\" 1")]
	public void Procedure_calls_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>
	/// A call without the word stands first in its batch and nowhere else: after a `;`, another
	/// statement, in a block or in a procedure's body it is refused.
	/// </summary>
	[Theory]
	[InlineData("p 1; SELECT 1", true)]
	[InlineData("p 1 SELECT 1", true)]
	[InlineData("p 1 PRINT 1", true)]
	[InlineData("p ;WITH a AS (SELECT 1 x) SELECT x FROM a", true)]
	[InlineData("/* c */ p 1", true)]
	[InlineData("p 1 -- c", true)]
	[InlineData("THROW", true)]
	[InlineData("SELECT 1\nGO\np 1", true)]
	[InlineData("SELECT 1; p", false)]
	[InlineData("SELECT 1 p 1", false)]
	[InlineData("PRINT 1; p 1", false)]
	[InlineData(";p 1", false)]
	[InlineData(";;sp_who", false)]
	[InlineData("p;p", false)]
	[InlineData("BEGIN p 1 END", false)]
	[InlineData("IF 1 = 1 p 1", false)]
	[InlineData("CREATE PROCEDURE pp AS p 1", false)]
	[InlineData("CREATE PROCEDURE pp AS SELECT 1; p 1", false)]
	[InlineData("p WITH a AS (SELECT 1 x) SELECT x FROM a", false)]
	public void A_call_without_the_word_stands_first_in_its_batch(string input, bool read) =>
		Assert.Equal(read, TransactSql.TryParseScript(input).IsSuccess);

	/// <summary>A table emptied, a trigger switched, a session killed and a column classified, as the engine answers them.</summary>
	[Theory]
	[InlineData("TRUNCATE TABLE t WITH (PARTITIONS ())")]
	[InlineData("TRUNCATE TABLE t WITH PARTITIONS (1)")]
	[InlineData("TRUNCATE TABLE t WITH (PARTITIONS (1), PARTITIONS (2))")]
	[InlineData("TRUNCATE TABLE t WITH (FOO = 1)")]
	[InlineData("TRUNCATE t")]
	[InlineData("TRUNCATE TABLE @t")]
	[InlineData("TRUNCATE TABLE t, u")]
	[InlineData("DISABLE TRIGGER db.dbo.tr ON t")]
	[InlineData("ENABLE TRIGGER tr")]
	[InlineData("ENABLE TRIGGER ON t")]
	[InlineData("ENABLE TRIGGER ALL, tr ON t")]
	[InlineData("ENABLE tr ON t")]
	[InlineData("KILL @s")]
	[InlineData("KILL 53 + 1")]
	[InlineData("KILL")]
	[InlineData("KILL 53 WITH")]
	[InlineData("KILL 53, 54")]
	[InlineData("KILL QUERY NOTIFICATION SUBSCRIPTION @x")]
	[InlineData("KILL QUERY NOTIFICATION SUBSCRIPTION")]
	[InlineData("KILL QUERY NOTIFICATION SUBSCRIPTION 'x'")]
	[InlineData("KILL STATS JOB @x")]
	[InlineData("KILL STATS JOB 'x'")]
	[InlineData("KILL STATS JOB")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO c WITH (LABEL = 'a')")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO db.dbo.t.c WITH (LABEL = 'a')")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO t.c WITH (LABEL = a)")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO t.c WITH (RANK = 'HIGH')")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO t.c WITH (RANK = FOO)")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO t.c WITH (FOO = 'a')")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO t.c WITH (LABEL = 'a', LABEL = 'b')")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO t.c WITH ()")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO t.c")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO t.c WITH LABEL = 'a'")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO t.c WITH (LABEL = 'a' RANK = LOW)")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO t.c WITH (LABEL_ID = 0x01)")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO t.c WITH (LABEL = @l)")]
	public void Maintenance_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("TRUNCATE TABLE t")]
	[InlineData("TRUNCATE TABLE db.dbo.t")]
	[InlineData("TRUNCATE TABLE srv.db.dbo.t")]
	[InlineData("TRUNCATE TABLE t WITH (PARTITIONS (1))")]
	[InlineData("TRUNCATE TABLE t WITH (PARTITIONS (1, 3 TO 5))")]
	[InlineData("TRUNCATE TABLE t WITH (PARTITIONS (1 + 1))")]
	[InlineData("TRUNCATE TABLE t WITH (PARTITIONS (@p))")]
	[InlineData("TRUNCATE TABLE #t")]
	[InlineData("TRUNCATE TABLE t WITH (PARTITIONS ($PARTITION.pf(1)))")]
	[InlineData("TRUNCATE TABLE t WITH (PARTITIONS (5 TO 3))")]
	[InlineData("ENABLE TRIGGER tr ON t")]
	[InlineData("ENABLE TRIGGER dbo.tr ON dbo.t")]
	[InlineData("ENABLE TRIGGER tr1, tr2 ON t")]
	[InlineData("ENABLE TRIGGER ALL ON t")]
	[InlineData("ENABLE TRIGGER ALL ON DATABASE")]
	[InlineData("ENABLE TRIGGER ALL ON ALL SERVER")]
	[InlineData("ENABLE TRIGGER tr ON DATABASE")]
	[InlineData("DISABLE TRIGGER tr ON ALL SERVER")]
	[InlineData("ENABLE TRIGGER tr ON db.dbo.t")]
	[InlineData("ENABLE TRIGGER tr ON srv.db.dbo.t")]
	[InlineData("ENABLE TRIGGER tr ON SERVER")]
	[InlineData("ENABLE TRIGGER [tr] ON [t]")]
	[InlineData("KILL 53")]
	[InlineData("KILL 53 WITH STATUSONLY")]
	[InlineData("KILL '8C8A3B2B-9F5B-4E1A-8E2F-1234567890AB'")]
	[InlineData("KILL '8C8A3B2B-9F5B-4E1A-8E2F-1234567890AB' WITH COMMIT")]
	[InlineData("KILL '8C8A3B2B-9F5B-4E1A-8E2F-1234567890AB' WITH ROLLBACK")]
	[InlineData("KILL 53 WITH COMMIT")]
	[InlineData("KILL -1")]
	[InlineData("KILL N'x'")]
	[InlineData("KILL QUERY NOTIFICATION SUBSCRIPTION ALL")]
	[InlineData("KILL QUERY NOTIFICATION SUBSCRIPTION 73")]
	[InlineData("KILL STATS JOB 53")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO dbo.t.c WITH (LABEL = 'Highly Confidential')")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO t.c, dbo.t.d WITH (LABEL = 'a', INFORMATION_TYPE = 'b', RANK = HIGH)")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO t.c WITH (LABEL_ID = 'guid', INFORMATION_TYPE_ID = 'guid')")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO t.c WITH (RANK = CRITICAL)")]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO t.c WITH (LABEL = N'a')")]
	public void Maintenance_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Types, schema collections and synonyms, as the engine answers them.</summary>
	[Theory]
	[InlineData("CREATE TYPE t FROM int NULL NOT NULL")]
	[InlineData("CREATE TYPE t FROM int DEFAULT 1")]
	[InlineData("CREATE TYPE t EXTERNAL NAME a.b.c")]
	[InlineData("CREATE TYPE t AS TABLE (a INT REFERENCES u(a))")]
	[InlineData("CREATE TYPE t AS TABLE (a INT, FOREIGN KEY (a) REFERENCES u(a))")]
	[InlineData("CREATE TYPE t AS TABLE (a INT CONSTRAINT c PRIMARY KEY)")]
	[InlineData("CREATE TYPE t AS TABLE ()")]
	[InlineData("CREATE TYPE t AS TABLE")]
	[InlineData("CREATE TYPE t")]
	[InlineData("CREATE TYPE t FROM")]
	[InlineData("CREATE TYPE t AS INT")]
	[InlineData("CREATE TYPE t AS TABLE (a INT SPARSE)")]
	[InlineData("CREATE TYPE t AS TABLE (a INT) WITH (DATA_COMPRESSION = PAGE)")]
	[InlineData("CREATE TYPE t AS TABLE (a INT) ON [PRIMARY]")]
	[InlineData("CREATE XML SCHEMA COLLECTION c")]
	[InlineData("ALTER XML SCHEMA COLLECTION c")]
	[InlineData("CREATE SYNONYM db.dbo.s FOR t")]
	[InlineData("CREATE SYNONYM s FOR 't'")]
	[InlineData("CREATE SYNONYM s")]
	[InlineData("CREATE SYNONYM s FOR @t")]
	[InlineData("ALTER SYNONYM s FOR t")]
	public void Types_and_synonyms_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("CREATE TYPE t FROM int")]
	[InlineData("CREATE TYPE dbo.t FROM varchar(10) NOT NULL")]
	[InlineData("CREATE TYPE t FROM decimal(10, 2) NULL")]
	[InlineData("CREATE TYPE t FROM nvarchar(max)")]
	[InlineData("CREATE TYPE db.dbo.t FROM int")]
	[InlineData("CREATE TYPE t FROM dbo.other")]
	[InlineData("CREATE TYPE t EXTERNAL NAME a.[b.c]")]
	[InlineData("CREATE TYPE t EXTERNAL NAME a")]
	[InlineData("CREATE TYPE t AS TABLE (a INT)")]
	[InlineData("CREATE TYPE t AS TABLE (a INT PRIMARY KEY, b VARCHAR(10) NOT NULL DEFAULT 'x', c AS a + 1)")]
	[InlineData("CREATE TYPE t AS TABLE (a INT, INDEX ix (a))")]
	[InlineData("CREATE TYPE t AS TABLE (a INT, PRIMARY KEY (a), UNIQUE (a), CHECK (a > 0))")]
	[InlineData("CREATE TYPE t AS TABLE (a INT IDENTITY(1,1))")]
	[InlineData("CREATE TYPE t AS TABLE (a INT) WITH (MEMORY_OPTIMIZED = ON)")]
	[InlineData("CREATE TYPE t AS TABLE (a INT ROWGUIDCOL)")]
	[InlineData("CREATE TYPE t AS TABLE (a INT, INDEX ix NONCLUSTERED HASH (a) WITH (BUCKET_COUNT = 8))")]
	[InlineData("CREATE TYPE t AS TABLE (a INT, PERIOD FOR SYSTEM_TIME (s, e))")]
	[InlineData("CREATE TYPE t AS TABLE (a INT PRIMARY KEY WITH (IGNORE_DUP_KEY = ON))")]
	[InlineData("CREATE TYPE t AS TABLE (a INT PRIMARY KEY NONCLUSTERED)")]
	[InlineData("CREATE TYPE t AS TABLE (a INT COLLATE Latin1_General_BIN)")]
	[InlineData("CREATE XML SCHEMA COLLECTION c AS '<schema/>'")]
	[InlineData("CREATE XML SCHEMA COLLECTION dbo.c AS N'<schema/>'")]
	[InlineData("CREATE XML SCHEMA COLLECTION c AS @s")]
	[InlineData("CREATE XML SCHEMA COLLECTION c AS 'a' + 'b'")]
	[InlineData("CREATE XML SCHEMA COLLECTION c AS (SELECT x FROM t)")]
	[InlineData("CREATE XML SCHEMA COLLECTION c AS CAST('x' AS XML)")]
	[InlineData("CREATE XML SCHEMA COLLECTION db.dbo.c AS 'x'")]
	[InlineData("ALTER XML SCHEMA COLLECTION c ADD '<schema/>'")]
	[InlineData("ALTER XML SCHEMA COLLECTION dbo.c ADD N'x'")]
	[InlineData("ALTER XML SCHEMA COLLECTION c ADD @s")]
	[InlineData("ALTER XML SCHEMA COLLECTION c ADD 'a' + 'b'")]
	[InlineData("CREATE SYNONYM s FOR t")]
	[InlineData("CREATE SYNONYM dbo.s FOR srv.db.dbo.t")]
	[InlineData("CREATE SYNONYM s FOR db..t")]
	[InlineData("CREATE SYNONYM s FOR srv...t")]
	[InlineData("CREATE SYNONYM s FOR srv..dbo.t")]
	[InlineData("CREATE SYNONYM s FOR a.b.c.d.e")]
	[InlineData("CREATE SYNONYM s FOR [t]")]
	public void Types_and_synonyms_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Sequences, as the engine answers them.</summary>
	[Theory]
	[InlineData("CREATE SEQUENCE db.dbo.s")]
	[InlineData("CREATE SEQUENCE s START WITH 1e1")]
	[InlineData("CREATE SEQUENCE s START WITH (1)")]
	[InlineData("CREATE SEQUENCE s START WITH 1 + 1")]
	[InlineData("CREATE SEQUENCE s START WITH @x")]
	[InlineData("CREATE SEQUENCE s START WITH '1'")]
	[InlineData("CREATE SEQUENCE s START 1")]
	[InlineData("CREATE SEQUENCE s MINVALUE")]
	[InlineData("CREATE SEQUENCE s MAXVALUE")]
	[InlineData("CREATE SEQUENCE s START WITH 1 START WITH 2")]
	[InlineData("CREATE SEQUENCE s MINVALUE 1 NO MINVALUE")]
	[InlineData("CREATE SEQUENCE s CYCLE NO CYCLE")]
	[InlineData("CREATE SEQUENCE s CACHE 10 NO CACHE")]
	[InlineData("CREATE SEQUENCE s START WITH 1, INCREMENT BY 1")]
	[InlineData("CREATE SEQUENCE s RESTART WITH 1")]
	[InlineData("CREATE SEQUENCE s NOCYCLE")]
	[InlineData("CREATE SEQUENCE s NO MINVALUE 1")]
	[InlineData("CREATE SEQUENCE s CACHE -1")]
	[InlineData("CREATE SEQUENCE s CACHE 0x10")]
	[InlineData("CREATE SEQUENCE s START WITH $1")]
	[InlineData("CREATE SEQUENCE s AS INT AS BIGINT")]
	[InlineData("ALTER SEQUENCE s")]
	[InlineData("ALTER SEQUENCE s START WITH 1")]
	[InlineData("ALTER SEQUENCE s AS BIGINT")]
	[InlineData("ALTER SEQUENCE s MINVALUE")]
	[InlineData("ALTER SEQUENCE s MAXVALUE")]
	[InlineData("ALTER SEQUENCE s RESTART RESTART")]
	public void Sequences_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("CREATE SEQUENCE s")]
	[InlineData("CREATE SEQUENCE dbo.s")]
	[InlineData("CREATE SEQUENCE s AS INT")]
	[InlineData("CREATE SEQUENCE s AS dbo.myint")]
	[InlineData("CREATE SEQUENCE s AS DECIMAL(10, 0)")]
	[InlineData("CREATE SEQUENCE s START WITH 1 INCREMENT BY 1")]
	[InlineData("CREATE SEQUENCE s START WITH -1")]
	[InlineData("CREATE SEQUENCE s START WITH +1")]
	[InlineData("CREATE SEQUENCE s START WITH 1.0")]
	[InlineData("CREATE SEQUENCE s START WITH - 1")]
	[InlineData("CREATE SEQUENCE s INCREMENT BY -5")]
	[InlineData("CREATE SEQUENCE s MINVALUE 1 MAXVALUE 10")]
	[InlineData("CREATE SEQUENCE s NO MINVALUE NO MAXVALUE")]
	[InlineData("CREATE SEQUENCE s CYCLE")]
	[InlineData("CREATE SEQUENCE s NO CYCLE")]
	[InlineData("CREATE SEQUENCE s CACHE 10")]
	[InlineData("CREATE SEQUENCE s CACHE")]
	[InlineData("CREATE SEQUENCE s NO CACHE")]
	[InlineData("CREATE SEQUENCE s INCREMENT BY 1 START WITH 1")]
	[InlineData("CREATE SEQUENCE s CYCLE AS INT")]
	[InlineData("CREATE SEQUENCE s AS INT START WITH 1 INCREMENT BY 1 MINVALUE 1 MAXVALUE 100 CYCLE CACHE 5")]
	[InlineData("ALTER SEQUENCE s RESTART")]
	[InlineData("ALTER SEQUENCE s RESTART WITH 100")]
	[InlineData("ALTER SEQUENCE dbo.s RESTART WITH 100 INCREMENT BY 50 MINVALUE 50 MAXVALUE 200 NO CYCLE NO CACHE")]
	[InlineData("ALTER SEQUENCE s CACHE")]
	[InlineData("ALTER SEQUENCE s INCREMENT BY 1 RESTART WITH 1")]
	[InlineData("ALTER SEQUENCE s RESTART WITH -5")]
	[InlineData("ALTER SEQUENCE Test. TestSeq RESTART WITH 100")]
	[InlineData("ALTER SEQUENCE s CYCLE")]
	[InlineData("ALTER SEQUENCE s NO MINVALUE NO MAXVALUE NO CACHE")]
	public void Sequences_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Partition functions and schemes, as the engine answers them.</summary>
	[Theory]
	[InlineData("CREATE PARTITION FUNCTION pf (INT, INT) AS RANGE LEFT FOR VALUES (1)")]
	[InlineData("CREATE PARTITION FUNCTION pf () AS RANGE LEFT FOR VALUES (1)")]
	[InlineData("CREATE PARTITION FUNCTION dbo.pf (INT) AS RANGE LEFT FOR VALUES (1)")]
	[InlineData("CREATE PARTITION FUNCTION pf (INT) RANGE LEFT FOR VALUES (1)")]
	[InlineData("CREATE PARTITION FUNCTION pf (INT) AS RANGE LEFT VALUES (1)")]
	[InlineData("CREATE PARTITION FUNCTION pf (INT) AS RANGE LEFT FOR VALUES 1")]
	[InlineData("CREATE PARTITION FUNCTION pf INT AS RANGE LEFT FOR VALUES (1)")]
	[InlineData("CREATE PARTITION FUNCTION pf (INT) AS RANGE LEFT FOR VALUES (DEFAULT)")]
	[InlineData("CREATE PARTITION FUNCTION pf (INT) AS RANGE LEFT FOR VALUES (1,)")]
	[InlineData("ALTER PARTITION FUNCTION pf SPLIT RANGE (500)")]
	[InlineData("ALTER PARTITION FUNCTION pf () SPLIT RANGE (1, 2)")]
	[InlineData("ALTER PARTITION FUNCTION pf () SPLIT RANGE ()")]
	[InlineData("ALTER PARTITION FUNCTION pf () SPLIT RANGE 500")]
	[InlineData("ALTER PARTITION FUNCTION pf (INT) SPLIT RANGE (500)")]
	[InlineData("ALTER PARTITION FUNCTION dbo.pf () SPLIT RANGE (500)")]
	[InlineData("ALTER PARTITION FUNCTION pf ()")]
	[InlineData("CREATE PARTITION SCHEME ps AS PARTITION pf ALL TO (PRIMARY)")]
	[InlineData("CREATE PARTITION SCHEME ps AS PARTITION pf TO ()")]
	[InlineData("CREATE PARTITION SCHEME ps AS PARTITION dbo.pf TO (fg1)")]
	[InlineData("CREATE PARTITION SCHEME dbo.ps AS PARTITION pf TO (fg1)")]
	[InlineData("CREATE PARTITION SCHEME ps AS PARTITION pf TO fg1")]
	[InlineData("CREATE PARTITION SCHEME ps PARTITION pf TO (fg1)")]
	[InlineData("ALTER PARTITION SCHEME ps NEXT USED fg4, fg5")]
	[InlineData("ALTER PARTITION SCHEME dbo.ps NEXT USED fg4")]
	[InlineData("ALTER PARTITION SCHEME ps NEXT fg4")]
	[InlineData("ALTER PARTITION SCHEME ps")]
	public void Partitions_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("CREATE PARTITION FUNCTION pf (INT) AS RANGE LEFT FOR VALUES (1, 100, 1000)")]
	[InlineData("CREATE PARTITION FUNCTION pf (INT) AS RANGE RIGHT FOR VALUES (1)")]
	[InlineData("CREATE PARTITION FUNCTION pf (INT) AS RANGE FOR VALUES (1)")]
	[InlineData("CREATE PARTITION FUNCTION pf (INT) AS RANGE LEFT FOR VALUES ()")]
	[InlineData("CREATE PARTITION FUNCTION pf (datetime2(7)) AS RANGE RIGHT FOR VALUES ('2020-01-01', '2021-01-01')")]
	[InlineData("CREATE PARTITION FUNCTION pf (nvarchar(10)) AS RANGE RIGHT FOR VALUES (N'a', N'b')")]
	[InlineData("CREATE PARTITION FUNCTION pf (INT) AS RANGE LEFT FOR VALUES (1 + 1, -5)")]
	[InlineData("CREATE PARTITION FUNCTION pf (INT) AS RANGE LEFT FOR VALUES (@x)")]
	[InlineData("CREATE PARTITION FUNCTION pf (INT) AS RANGE LEFT FOR VALUES (NULL)")]
	[InlineData("CREATE PARTITION FUNCTION pf (INT) AS RANGE LEFT FOR VALUES ((SELECT 1))")]
	[InlineData("CREATE PARTITION FUNCTION pf (INT) AS RANGE LEFT FOR VALUES (f(1))")]
	[InlineData("CREATE PARTITION FUNCTION pf (INT) AS RANGE LEFT FOR VALUES (CAST('1' AS INT))")]
	[InlineData("CREATE PARTITION FUNCTION [pf] (INT) AS RANGE LEFT FOR VALUES (1)")]
	[InlineData("CREATE PARTITION FUNCTION pf (dbo.mytype) AS RANGE LEFT FOR VALUES (1)")]
	[InlineData("create partition function pf (int) as range left for values (1)")]
	[InlineData("ALTER PARTITION FUNCTION pf () SPLIT RANGE (500)")]
	[InlineData("ALTER PARTITION FUNCTION pf() MERGE RANGE (100)")]
	[InlineData("ALTER PARTITION FUNCTION pf () SPLIT RANGE (1 + 1)")]
	[InlineData("ALTER PARTITION FUNCTION pf () SPLIT RANGE (@x)")]
	[InlineData("ALTER PARTITION FUNCTION pf () SPLIT RANGE (NULL)")]
	[InlineData("ALTER PARTITION FUNCTION pf () SPLIT RANGE ((SELECT 1))")]
	[InlineData("CREATE PARTITION SCHEME ps AS PARTITION pf TO (fg1, fg2, fg3)")]
	[InlineData("CREATE PARTITION SCHEME ps AS PARTITION pf ALL TO ([PRIMARY])")]
	[InlineData("CREATE PARTITION SCHEME ps AS PARTITION pf TO ([PRIMARY], fg2)")]
	[InlineData("CREATE PARTITION SCHEME ps AS PARTITION pf TO ('fg1')")]
	[InlineData("CREATE PARTITION SCHEME ps AS PARTITION pf ALL TO (fg1, fg2)")]
	[InlineData("CREATE PARTITION SCHEME ps AS PARTITION pf TO ([DEFAULT])")]
	[InlineData("CREATE PARTITION SCHEME ps AS PARTITION pf TO (\"fg1\")")]
	[InlineData("ALTER PARTITION SCHEME ps NEXT USED fg4")]
	[InlineData("ALTER PARTITION SCHEME ps NEXT USED")]
	[InlineData("ALTER PARTITION SCHEME ps NEXT USED [PRIMARY]")]
	[InlineData("ALTER PARTITION SCHEME ps NEXT USED 'fg4'")]
	public void Partitions_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Names, literals and sources a query is written with, as the engine answers them.</summary>
	[Theory]
	[InlineData("SELECT CHECKSUM_AGG(*) FROM t")]
	[InlineData("SELECT BINARY_CHECKSUM(*, a) FROM t")]
	[InlineData("SELECT BINARY_CHECKSUM(t.*) FROM t")]
	[InlineData("SELECT FOO(*) FROM t")]
	[InlineData("SELECT 0xabc\\ def AS c")]
	[InlineData("SELECT 0xabc\\\nAS c")]
	[InlineData("SELECT 1\\\n2 AS c")]
	[InlineData("SELECT 0xabc\\\n def AS c")]
	[InlineData("SELECT 1 AS [a]#")]
	[InlineData("SELECT@a = 1")]
	[InlineData("SELECT * FROM OPENROWSET(BULK (), FORMAT = 'PARQUET') AS r")]
	[InlineData("SELECT * FROM OPENROWSET('SQLNCLI', 'Server=s;', 'SELECT 1').a")]
	[InlineData("SELECT * FROM OPENQUERY(srv, 'SELECT 1').a")]
	[InlineData("SELECT * FROM OPENDATASOURCE('MSOLEDBSQL', 'Server=s')")]
	[InlineData("SELECT * FROM OPENDATASOURCE('MSOLEDBSQL', 'Server=s').db.dbo.t WITH (NOLOCK)")]
	[InlineData("SELECT * FROM OPENDATASOURCE('P', 's').a.b.")]
	[InlineData("SELECT * FROM OPENDATASOURCE('P', 's').db.dbo.t x (a, b)")]
	[InlineData("SELECT * FROM OPENDATASOURCE('P', 's').db.dbo.t FOR SYSTEM_TIME ALL")]
	[InlineData("SELECT * FROM OPENDATASOURCE('P', 's').db.dbo.t TABLESAMPLE (10 PERCENT)")]
	[InlineData("SELECT * FROM OPENDATASOURCE('P').db.dbo.t")]
	[InlineData("SELECT * FROM OPENDATASOURCE('P', 's', 'x').db.dbo.t")]
	[InlineData("SELECT * FROM OPENDATASOURCE().db.dbo.t")]
	[InlineData("SELECT * FROM OPENDATASOURCE(@p, 's').db.dbo.t")]
	[InlineData("SELECT * FROM OPENDATASOURCE('P', 's' + 'x').db.dbo.t")]
	[InlineData("SELECT OPENDATASOURCE('MSOLEDBSQL', 'Server=s').db.dbo.t.a FROM t")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m AS m, DATA = dbo.t) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM OPENROWSET(BULK 'f', FORMATFILE = 'x' AS y) AS x")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = dbo.t, RUNTIME = ONNX) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, RUNTIME = ONNX, DATA = dbo.t AS d) WITH (s FLOAT) AS p")]
	[InlineData("SELECT d.*, p.Score FROM PREDICT(MODEL = (SELECT Model FROM Models WHERE Id = 4), DATA = testData AS d, RUNTIME=ONNX) WITH (Score float) AS p")]
	public void Names_and_sources_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("SELECT BINARY_CHECKSUM(*) FROM t")]
	[InlineData("SELECT 1 AS N'Year'")]
	[InlineData("SELECT 1 N'Year'")]
	[InlineData("SELECT N'Year' = 1")]
	[InlineData("SELECT DATEPART(yyyy, OrderDate) AS N'Year', SUM(TotalDue) AS N'Total Order Amount' FROM t GROUP BY DATEPART(yyyy, OrderDate)")]
	[InlineData("SELECT a AS N'x' FROM t ORDER BY N'x'")]
	[InlineData("SELECT 1 AS Row#")]
	[InlineData("SELECT 1 AS a#")]
	[InlineData("SELECT 1 AS a#b#c")]
	[InlineData("SELECT 1 AS _#")]
	[InlineData("SELECT 1 AS a$")]
	[InlineData("SELECT 1 AS a@")]
	[InlineData("SELECT 1 AS a@@b")]
	[InlineData("SELECT 1 AS ab#1")]
	[InlineData("SELECT a#b.c#d FROM t")]
	[InlineData("CREATE TABLE t (a#b INT)")]
	[InlineData("SELECT Row# FROM t")]
	[InlineData("SELECT 1 AS SELECT#")]
	[InlineData("SELECT FROM#")]
	[InlineData("SELECT 1 AS#x")]
	[InlineData("SELECT x FROM t#u")]
	[InlineData("SET @a#b = 1")]
	[InlineData("SET @a$b = 1")]
	[InlineData("SELECT 0xabc\\\ndef AS c")]
	[InlineData("SELECT 0xabc\\\r\ndef AS c")]
	[InlineData("SELECT 0x\\\nabc AS c")]
	[InlineData("SELECT 'abc\\\ndef' AS c")]
	[InlineData("SELECT 'abc\\def' AS c")]
	[InlineData("SELECT * FROM OPENROWSET(BULK ('/a/*.parquet', '/b/*.parquet'), FORMAT = 'PARQUET') AS r")]
	[InlineData("SELECT * FROM OPENROWSET(BULK ('/a/*.parquet'), FORMAT = 'PARQUET') AS r")]
	[InlineData("SELECT * FROM OPENDATASOURCE('MSOLEDBSQL', 'Server=s').AdventureWorks2022.HumanResources.Department")]
	[InlineData("SELECT * FROM OPENDATASOURCE('P', 's')..t")]
	[InlineData("SELECT * FROM OPENDATASOURCE('P', 's').db..t")]
	[InlineData("SELECT * FROM OPENDATASOURCE('P', 's').db...t")]
	[InlineData("SELECT * FROM OPENDATASOURCE('P', 's').t")]
	[InlineData("SELECT * FROM OPENDATASOURCE('P', 's').a.b.c.d.e")]
	[InlineData("SELECT * FROM OPENDATASOURCE('P', 's') . db . dbo . t")]
	[InlineData("SELECT * FROM OPENDATASOURCE('P', 's').[db].[dbo].[t]")]
	[InlineData("SELECT * FROM OPENDATASOURCE(N'P', N's').db.dbo.t AS x")]
	[InlineData("SELECT * FROM t JOIN OPENDATASOURCE('P', 's').db.dbo.u x ON 1 = 1")]
	[InlineData("EXEC OPENDATASOURCE('P', 's').db.dbo.p")]
	[InlineData("EXEC OPENDATASOURCE('P', 's').db..p 1, 2")]
	[InlineData("EXEC OPENDATASOURCE('P', 's').p @a = 1")]
	[InlineData("EXEC @r = OPENDATASOURCE('P', 's').db.dbo.p")]
	[InlineData("DELETE FROM OPENDATASOURCE('P', 's').db.dbo.t")]
	[InlineData("DELETE OPENDATASOURCE('P', 's').db.dbo.t WHERE a = 1")]
	[InlineData("MERGE INTO OPENDATASOURCE('P', 's').db.dbo.t AS x USING u ON 1 = 1 WHEN MATCHED THEN DELETE;")]
	[InlineData("INSERT OPENDATASOURCE('P', 's').db.dbo.t (a) VALUES (1)")]
	[InlineData("INSERT INTO OPENDATASOURCE('P', 's').db.dbo.t VALUES (1)")]
	[InlineData("UPDATE OPENDATASOURCE('P', 's').db.dbo.t SET a = 1")]
	[InlineData("SELECT d.*, p.Score FROM PREDICT(MODEL = @model, DATA = dbo.mytable AS d) WITH (Score FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = dbo.t d) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(DATA = dbo.t AS d, MODEL = @m) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = (SELECT 1 a) AS d) WITH (s FLOAT) AS p")]
	[InlineData("SELECT * FROM PREDICT(MODEL = @m, DATA = dbo.t AS [d]) WITH (s FLOAT) AS p")]
	public void Names_and_sources_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>A variable's members, and keys opened and closed, as the engine answers them.</summary>
	[Theory]
	[InlineData("SET @x.a.b('a')")]
	[InlineData("SET @x.modify('a') = 1")]
	[InlineData("SET @g.STSrid = 1, @g.Z = 2")]
	[InlineData("SET @g::STSrid = 1")]
	[InlineData("SET @g.STSrid")]
	[InlineData("SET @g.STSrid = DEFAULT")]
	[InlineData("SET @a = @g.STSrid = 1")]
	[InlineData("SET @x.modify('a').x = 1")]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY PASSWORD = @p")]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY CERTIFICATE c, PASSWORD = 'p'")]
	[InlineData("OPEN SYMMETRIC KEY k")]
	[InlineData("OPEN SYMMETRIC KEY a.k DECRYPTION BY CERTIFICATE c")]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY CERTIFICATE c WITH PASSWORD = p")]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY SYMMETRIC KEY s WITH PASSWORD = 'p'")]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY PASSWORD 'p'")]
	[InlineData("OPEN MASTER KEY DECRYPTION BY PASSWORD = @p")]
	[InlineData("OPEN MASTER KEY")]
	[InlineData("OPEN MASTER KEY DECRYPTION BY CERTIFICATE c")]
	[InlineData("CLOSE SYMMETRIC KEY a.k")]
	[InlineData("CLOSE SYMMETRIC KEY k, j")]
	[InlineData("CLOSE ALL SYMMETRIC KEY")]
	[InlineData("CLOSE MASTER KEY k")]
	[InlineData("CLOSE ASYMMETRIC KEY k")]
	public void Members_and_keys_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("SET @x.modify('delete /a')")]
	[InlineData("SET @x.modify(N'delete /a')")]
	[InlineData("SET @x.modify(@s)")]
	[InlineData("SET @x.modify('a' + 'b')")]
	[InlineData("SET @x.modify()")]
	[InlineData("SET @x.modify('a', 'b')")]
	[InlineData("SET @x.modify ('delete /a')")]
	[InlineData("SET @x . modify('delete /a')")]
	[InlineData("SET @x.[modify]('delete /a')")]
	[InlineData("SET @x.MODIFY('delete /a')")]
	[InlineData("SET @x.foo('a')")]
	[InlineData("SET @@x.modify('a')")]
	[InlineData("SET @g.STSrid = 4267")]
	[InlineData("SET @g.STSrid += 1")]
	[InlineData("SET @g.[STSrid] = 1")]
	[InlineData("SET @g.a.b = 1")]
	[InlineData("SET @g.STSrid = @g.STSrid + 1")]
	[InlineData("SET @g.STSrid = (SELECT 1)")]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY CERTIFICATE c")]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY CERTIFICATE c WITH PASSWORD = 'p'")]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY ASYMMETRIC KEY a")]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY ASYMMETRIC KEY a WITH PASSWORD = 'p'")]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY SYMMETRIC KEY s")]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY PASSWORD = 'p'")]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY PASSWORD = N'p'")]
	[InlineData("OPEN SYMMETRIC KEY #k DECRYPTION BY PASSWORD = 'p'")]
	[InlineData("OPEN SYMMETRIC KEY [k] DECRYPTION BY CERTIFICATE [c]")]
	[InlineData("OPEN MASTER KEY DECRYPTION BY PASSWORD = 'p'")]
	[InlineData("OPEN MASTER KEY DECRYPTION BY PASSWORD = N'p'")]
	[InlineData("CLOSE SYMMETRIC KEY k")]
	[InlineData("CLOSE SYMMETRIC KEY [k]")]
	[InlineData("CLOSE SYMMETRIC KEY #k")]
	[InlineData("CLOSE ALL SYMMETRIC KEYS")]
	[InlineData("CLOSE MASTER KEY")]
	[InlineData("open symmetric key k decryption by certificate c")]
	public void Members_and_keys_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The server's configuration, as the engine answers it.</summary>
	[Theory]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = f)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = db.dbo.f)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = dbo.f, MAX_OUTSTANDING_IO_PER_VOLUME = 20)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH ()")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH CLASSIFIER_FUNCTION = dbo.f")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = 'dbo.f')")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (MAX_OUTSTANDING_IO_PER_VOLUME = 'x')")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (MAX_OUTSTANDING_IO_PER_VOLUME = @n)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (MAX_OUTSTANDING_IO_PER_VOLUME = -1)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = dbo.f, CLASSIFIER_FUNCTION = NULL)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (FOO = 1)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = dbo.f())")]
	[InlineData("ALTER RESOURCE GOVERNOR")]
	[InlineData("ALTER RESOURCE GOVERNOR RECONFIGURE WITH (CLASSIFIER_FUNCTION = NULL)")]
	[InlineData("ALTER RESOURCE GOVERNOR RESET")]
	[InlineData("ALTER RESOURCE GOVERNOR ENABLE")]
	[InlineData("ALTER RESOURCE GOVERNOR RECONFIGURE RESET STATISTICS")]
	[InlineData("ALTER RESOURCE GOVERNOR RESET STATISTICS DISABLE")]
	[InlineData("ALTER RESOURCE GOVERNOR RESET STATISTICS RESET STATISTICS")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = dbo..f)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (MAX_OUTSTANDING_IO_PER_VOLUME = 0x1)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (MAX_OUTSTANDING_IO_PER_VOLUME = 1e1)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (MAX_OUTSTANDING_IO_PER_VOLUME = $1)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (MAX_OUTSTANDING_IO_PER_VOLUME = NULL)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = DEFAULT)")]
	[InlineData("ALTER SERVER CONFIGURATION SET PROCESS AFFINITY NUMANODE = AUTO")]
	[InlineData("ALTER SERVER CONFIGURATION SET PROCESS AFFINITY CPU = (1)")]
	[InlineData("ALTER SERVER CONFIGURATION SET PROCESS AFFINITY CPU = @n")]
	[InlineData("ALTER SERVER CONFIGURATION SET PROCESS AFFINITY CPU = -1")]
	[InlineData("ALTER SERVER CONFIGURATION SET PROCESS AFFINITY CPU = 1.5")]
	[InlineData("ALTER SERVER CONFIGURATION SET PROCESS AFFINITY CPU = 1,")]
	[InlineData("ALTER SERVER CONFIGURATION SET PROCESS AFFINITY")]
	[InlineData("ALTER SERVER CONFIGURATION SET PROCESS AFFINITY CPU = 1 NUMANODE = 0")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG MAX_SIZE = '10' MB")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG MAX_SIZE = 10")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG MAX_SIZE = 10 GB")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG MAX_FILES = '10'")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG ON, OFF")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG FOO = 1")]
	[InlineData("ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY VerboseLogging = '2'")]
	[InlineData("ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY SqlDumperDumpFlags = 0x001F")]
	[InlineData("ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY Foo = 1")]
	[InlineData("ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY VerboseLogging = -1")]
	[InlineData("ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY VerboseLogging = N'2'")]
	[InlineData("ALTER SERVER CONFIGURATION SET HADR CLUSTER CONTEXT = foo")]
	[InlineData("ALTER SERVER CONFIGURATION SET BUFFER POOL EXTENSION ON (FILENAME = 'F:\\x.BPE', SIZE = 50)")]
	[InlineData("ALTER SERVER CONFIGURATION SET BUFFER POOL EXTENSION ON (SIZE = 50 GB, FILENAME = 'F:\\x.BPE')")]
	[InlineData("ALTER SERVER CONFIGURATION SET BUFFER POOL EXTENSION ON (FILENAME = 'F:\\x.BPE')")]
	[InlineData("ALTER SERVER CONFIGURATION SET BUFFER POOL EXTENSION ON")]
	[InlineData("ALTER SERVER CONFIGURATION SET BUFFER POOL EXTENSION ON (FILENAME = 'F:\\x.BPE', SIZE = 50 TB)")]
	[InlineData("ALTER SERVER CONFIGURATION SET BUFFER POOL EXTENSION ON (FILENAME = 'F:\\x.BPE', SIZE = 1.5 GB)")]
	[InlineData("ALTER SERVER CONFIGURATION SET SOFTNUMA = ON")]
	[InlineData("ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED TEMPDB_METADATA = OFF (RESOURCE_POOL = 'p')")]
	[InlineData("ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED TEMPDB_METADATA = ON (RESOURCE_POOL = p)")]
	[InlineData("ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED ON, OFF")]
	[InlineData("ALTER SERVER CONFIGURATION SET HARDWARE_OFFLOAD ON")]
	[InlineData("ALTER SERVER CONFIGURATION SET HARDWARE_OFFLOAD OFF")]
	[InlineData("ALTER SERVER CONFIGURATION SET HARDWARE_OFFLOAD = ON")]
	[InlineData("ALTER SERVER CONFIGURATION SET SET SUSPEND_FOR_SNAPSHOT_BACKUP = ON")]
	[InlineData("ALTER SERVER CONFIGURATION SET SUSPEND_FOR_SNAPSHOT_BACKUP = ON (GROUP = db1)")]
	[InlineData("ALTER SERVER CONFIGURATION SET")]
	[InlineData("ALTER SERVER CONFIGURATION SET SOFTNUMA ON, HARDWARE_OFFLOAD ON")]
	[InlineData("ALTER SERVER CONFIGURATION SET FOO ON")]
	[InlineData("ALTER SERVER CONFIGURATION SOFTNUMA ON")]
	public void The_server_configuration_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("ALTER RESOURCE GOVERNOR RECONFIGURE")]
	[InlineData("ALTER RESOURCE GOVERNOR DISABLE")]
	[InlineData("ALTER RESOURCE GOVERNOR RESET STATISTICS")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = dbo.f)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = NULL)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (MAX_OUTSTANDING_IO_PER_VOLUME = 20)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (MAX_OUTSTANDING_IO_PER_VOLUME = 1.5)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (MAX_OUTSTANDING_IO_PER_VOLUME = DEFAULT)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = [dbo].[f])")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = \"dbo\".\"f\")")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = dbo.[f])")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = null)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (MAX_OUTSTANDING_IO_PER_VOLUME = .5)")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (max_outstanding_io_per_volume = 1)")]
	[InlineData("ALTER RESOURCE GOVERNOR RESET STATISTICS RECONFIGURE")]
	[InlineData("ALTER RESOURCE GOVERNOR DISABLE RECONFIGURE")]
	[InlineData("ALTER RESOURCE GOVERNOR RECONFIGURE RECONFIGURE")]
	[InlineData("ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = dbo.f) RECONFIGURE")]
	[InlineData("alter resource governor reconfigure")]
	[InlineData("ALTER SERVER CONFIGURATION SET PROCESS AFFINITY CPU = AUTO")]
	[InlineData("ALTER SERVER CONFIGURATION SET PROCESS AFFINITY CPU=0 TO 63, 128 TO 191")]
	[InlineData("ALTER SERVER CONFIGURATION SET PROCESS AFFINITY CPU = 1, 3, 5 TO 7")]
	[InlineData("ALTER SERVER CONFIGURATION SET PROCESS AFFINITY NUMANODE = 0 TO 3, 5")]
	[InlineData("ALTER SERVER CONFIGURATION SET PROCESS AFFINITY CPU = 3 TO 1")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG ON")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG OFF")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG PATH = 'C:\\logs'")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG PATH = DEFAULT")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG PATH = N'x'")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG MAX_SIZE = 10 MB")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG MAX_SIZE = 10 mb")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG MAX_SIZE = DEFAULT")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG MAX_FILES = 10")]
	[InlineData("ALTER SERVER CONFIGURATION SET DIAGNOSTICS LOG MAX_FILES = DEFAULT")]
	[InlineData("ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY VerboseLogging = 2")]
	[InlineData("ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY VerboseLogging = DEFAULT")]
	[InlineData("ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY SqlDumperDumpPath = 'C:\\x'")]
	[InlineData("ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY SqlDumperDumpTimeOut = 10")]
	[InlineData("ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY FailureConditionLevel = 3")]
	[InlineData("ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY HealthCheckTimeout = 15000")]
	[InlineData("ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY ClusterConnectionOptions = 'Encrypt=Mandatory'")]
	[InlineData("ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY verboselogging = 1")]
	[InlineData("ALTER SERVER CONFIGURATION SET FAILOVER CLUSTER PROPERTY ClusterConnectionOptions = DEFAULT")]
	[InlineData("ALTER SERVER CONFIGURATION SET HADR CLUSTER CONTEXT = 'clus01.xyz.com'")]
	[InlineData("ALTER SERVER CONFIGURATION SET HADR CLUSTER CONTEXT = LOCAL")]
	[InlineData("ALTER SERVER CONFIGURATION SET HADR CLUSTER CONTEXT = N'x'")]
	[InlineData("ALTER SERVER CONFIGURATION SET BUFFER POOL EXTENSION ON (FILENAME = 'F:\\x.BPE', SIZE = 50 GB)")]
	[InlineData("ALTER SERVER CONFIGURATION SET BUFFER POOL EXTENSION ON (FILENAME = 'F:\\x.BPE', SIZE = 50 KB)")]
	[InlineData("ALTER SERVER CONFIGURATION SET BUFFER POOL EXTENSION ON (FILENAME = N'F:\\x.BPE', SIZE = 50 GB)")]
	[InlineData("ALTER SERVER CONFIGURATION SET BUFFER POOL EXTENSION OFF")]
	[InlineData("ALTER SERVER CONFIGURATION SET SOFTNUMA ON")]
	[InlineData("ALTER SERVER CONFIGURATION SET SOFTNUMA OFF")]
	[InlineData("ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED ON")]
	[InlineData("ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED OFF")]
	[InlineData("ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED TEMPDB_METADATA = ON")]
	[InlineData("ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED TEMPDB_METADATA = ON (RESOURCE_POOL = 'p')")]
	[InlineData("ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED TEMPDB_METADATA = OFF")]
	[InlineData("ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED HYBRID_BUFFER_POOL = ON")]
	[InlineData("ALTER SERVER CONFIGURATION SET MEMORY_OPTIMIZED HYBRID_BUFFER_POOL = OFF")]
	[InlineData("ALTER SERVER CONFIGURATION SET SUSPEND_FOR_SNAPSHOT_BACKUP = ON")]
	[InlineData("ALTER SERVER CONFIGURATION SET SUSPEND_FOR_SNAPSHOT_BACKUP = OFF")]
	[InlineData("ALTER SERVER CONFIGURATION SET SUSPEND_FOR_SNAPSHOT_BACKUP = ON (GROUP = (db1, db2))")]
	[InlineData("ALTER SERVER CONFIGURATION SET SUSPEND_FOR_SNAPSHOT_BACKUP = ON (GROUP = (db1), MODE = COPY_ONLY)")]
	[InlineData("ALTER SERVER CONFIGURATION SET SUSPEND_FOR_SNAPSHOT_BACKUP = ON (MODE = COPY_ONLY)")]
	[InlineData("ALTER SERVER CONFIGURATION SET SUSPEND_FOR_SNAPSHOT_BACKUP = ON (GROUP = ([db1], [db 2]))")]
	[InlineData("ALTER SERVER CONFIGURATION SET SUSPEND_FOR_SNAPSHOT_BACKUP = OFF (GROUP = (db1))")]
	[InlineData("alter server configuration set softnuma on")]
	public void The_server_configuration_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Service Broker's objects, as the engine answers them.</summary>
	[Theory]
	[InlineData("CREATE MESSAGE TYPE m VALIDATION = VALID_XML")]
	[InlineData("CREATE MESSAGE TYPE m VALIDATION = WELL_FORMED_XML AUTHORIZATION dbo")]
	[InlineData("CREATE MESSAGE TYPE a.b")]
	[InlineData("CREATE MESSAGE TYPE 'm'")]
	[InlineData("ALTER MESSAGE TYPE m")]
	[InlineData("ALTER MESSAGE TYPE m AUTHORIZATION dbo VALIDATION = EMPTY")]
	[InlineData("CREATE CONTRACT c (DEFAULT SENT BY ANY)")]
	[InlineData("CREATE CONTRACT c ()")]
	[InlineData("CREATE CONTRACT c")]
	[InlineData("CREATE CONTRACT c (m SENT BY FOO)")]
	[InlineData("CREATE CONTRACT c (m)")]
	[InlineData("CREATE CONTRACT c (a.b SENT BY ANY)")]
	[InlineData("CREATE CONTRACT c ('m' SENT BY ANY)")]
	[InlineData("ALTER CONTRACT c (m SENT BY ANY)")]
	[InlineData("CREATE QUEUE q WITH STATUS = ON RETENTION = OFF")]
	[InlineData("CREATE QUEUE q WITH ACTIVATION (PROCEDURE_NAME = p)")]
	[InlineData("CREATE QUEUE q WITH ACTIVATION (STATUS = ON PROCEDURE_NAME = p MAX_QUEUE_READERS = 5 EXECUTE AS SELF)")]
	[InlineData("CREATE QUEUE q WITH POISON_MESSAGE_HANDLING ()")]
	[InlineData("CREATE QUEUE q ON DEFAULT")]
	[InlineData("CREATE QUEUE q WITH")]
	[InlineData("CREATE QUEUE q WITH STATUS = ON, STATUS = OFF")]
	[InlineData("CREATE QUEUE q WITH ACTIVATION (DROP)")]
	[InlineData("CREATE QUEUE q WITH FOO = ON")]
	[InlineData("CREATE QUEUE q WITH STATUS = 1")]
	[InlineData("ALTER QUEUE q")]
	[InlineData("ALTER QUEUE q ON fg")]
	[InlineData("ALTER QUEUE q WITH STATUS = OFF REBUILD")]
	[InlineData("CREATE SERVICE s ON QUEUE q ()")]
	[InlineData("CREATE SERVICE s ON QUEUE q (DEFAULT)")]
	[InlineData("CREATE SERVICE s")]
	[InlineData("CREATE SERVICE a.b ON QUEUE q")]
	[InlineData("ALTER SERVICE s")]
	[InlineData("ALTER SERVICE s (c)")]
	[InlineData("CREATE ROUTE r WITH SERVICE_NAME = 's'")]
	[InlineData("CREATE ROUTE r WITH LIFETIME = 'x', ADDRESS = 'LOCAL'")]
	[InlineData("CREATE ROUTE r WITH ADDRESS = 'LOCAL' MIRROR_ADDRESS = 'x'")]
	[InlineData("CREATE ROUTE r WITH ADDRESS = 'LOCAL', ADDRESS = 'x'")]
	[InlineData("CREATE ROUTE r WITH ADDRESS = @a")]
	[InlineData("CREATE ROUTE r WITH ADDRESS = a")]
	[InlineData("ALTER ROUTE r WITH SERVICE_NAME = 's' ADDRESS = 'x'")]
	[InlineData("ALTER ROUTE r")]
	[InlineData("ALTER ROUTE r AUTHORIZATION dbo WITH ADDRESS = 'x'")]
	[InlineData("CREATE REMOTE SERVICE BINDING b TO SERVICE 's'")]
	[InlineData("CREATE REMOTE SERVICE BINDING b TO SERVICE s WITH USER = u")]
	[InlineData("CREATE REMOTE SERVICE BINDING b TO SERVICE 's' WITH USER = 'u'")]
	[InlineData("CREATE REMOTE SERVICE BINDING b TO SERVICE 's' WITH USER = u ANONYMOUS = ON")]
	[InlineData("ALTER REMOTE SERVICE BINDING b WITH")]
	[InlineData("CREATE BROKER PRIORITY p FOR CONVERSATION SET (CONTRACT_NAME = c PRIORITY_LEVEL = 5)")]
	[InlineData("CREATE BROKER PRIORITY p FOR CONVERSATION SET ()")]
	[InlineData("CREATE BROKER PRIORITY p FOR CONVERSATION SET (REMOTE_SERVICE_NAME = r)")]
	[InlineData("CREATE BROKER PRIORITY p FOR CONVERSATION SET (LOCAL_SERVICE_NAME = a.b)")]
	[InlineData("CREATE BROKER PRIORITY p FOR CONVERSATION SET (PRIORITY_LEVEL = @n)")]
	[InlineData("CREATE BROKER PRIORITY p FOR CONVERSATION SET (CONTRACT_NAME = c, CONTRACT_NAME = d)")]
	[InlineData("CREATE BROKER PRIORITY p")]
	[InlineData("ALTER BROKER PRIORITY p FOR CONVERSATION")]
	[InlineData("ALTER BROKER PRIORITY p FOR CONVERSATION SET ()")]
	public void Broker_objects_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("CREATE MESSAGE TYPE m")]
	[InlineData("CREATE MESSAGE TYPE [//a/b] AUTHORIZATION dbo VALIDATION = WELL_FORMED_XML")]
	[InlineData("CREATE MESSAGE TYPE m VALIDATION = NONE")]
	[InlineData("CREATE MESSAGE TYPE m VALIDATION = VALID_XML WITH SCHEMA COLLECTION dbo.s")]
	[InlineData("CREATE MESSAGE TYPE m VALIDATION = FOO")]
	[InlineData("ALTER MESSAGE TYPE m VALIDATION = EMPTY")]
	[InlineData("CREATE CONTRACT c (m SENT BY INITIATOR)")]
	[InlineData("CREATE CONTRACT c AUTHORIZATION dbo ([//a] SENT BY ANY, [DEFAULT] SENT BY TARGET)")]
	[InlineData("CREATE QUEUE q")]
	[InlineData("CREATE QUEUE dbo.q")]
	[InlineData("CREATE QUEUE db.dbo.q WITH STATUS = ON, RETENTION = OFF")]
	[InlineData("CREATE QUEUE q WITH ACTIVATION (STATUS = ON, PROCEDURE_NAME = dbo.p, MAX_QUEUE_READERS = 5, EXECUTE AS SELF)")]
	[InlineData("CREATE QUEUE q WITH ACTIVATION (PROCEDURE_NAME = p, MAX_QUEUE_READERS = 5, EXECUTE AS 'u')")]
	[InlineData("CREATE QUEUE q WITH ACTIVATION (MAX_QUEUE_READERS = 5, PROCEDURE_NAME = p, EXECUTE AS OWNER)")]
	[InlineData("CREATE QUEUE q WITH POISON_MESSAGE_HANDLING (STATUS = OFF)")]
	[InlineData("CREATE QUEUE q WITH STATUS = ON, ACTIVATION (PROCEDURE_NAME = p, MAX_QUEUE_READERS = 1, EXECUTE AS SELF), POISON_MESSAGE_HANDLING (STATUS = ON)")]
	[InlineData("CREATE QUEUE q ON fg")]
	[InlineData("CREATE QUEUE q ON [DEFAULT]")]
	[InlineData("CREATE QUEUE q WITH STATUS = ON ON fg")]
	[InlineData("CREATE QUEUE q WITH RETENTION = ON, STATUS = ON")]
	[InlineData("ALTER QUEUE q WITH STATUS = OFF")]
	[InlineData("ALTER QUEUE q WITH ACTIVATION (DROP)")]
	[InlineData("ALTER QUEUE q WITH ACTIVATION (EXECUTE AS OWNER)")]
	[InlineData("ALTER QUEUE q REBUILD")]
	[InlineData("ALTER QUEUE q REBUILD WITH (MAXDOP = 2)")]
	[InlineData("ALTER QUEUE q REORGANIZE WITH (LOB_COMPACTION = ON)")]
	[InlineData("ALTER QUEUE q MOVE TO fg")]
	[InlineData("ALTER QUEUE q MOVE TO [default]")]
	[InlineData("CREATE SERVICE s ON QUEUE q")]
	[InlineData("CREATE SERVICE s AUTHORIZATION dbo ON QUEUE dbo.q ([//c], [DEFAULT])")]
	[InlineData("CREATE SERVICE [//a/b] ON QUEUE db.dbo.q (c)")]
	[InlineData("ALTER SERVICE s ON QUEUE q")]
	[InlineData("ALTER SERVICE s (ADD CONTRACT c)")]
	[InlineData("ALTER SERVICE s ON QUEUE q (ADD CONTRACT c, DROP CONTRACT [DEFAULT])")]
	[InlineData("CREATE ROUTE r WITH ADDRESS = 'LOCAL'")]
	[InlineData("CREATE ROUTE r AUTHORIZATION dbo WITH SERVICE_NAME = 's', BROKER_INSTANCE = 'x', LIFETIME = 10, ADDRESS = 'TCP://h:4022', MIRROR_ADDRESS = 'TCP://m:4022'")]
	[InlineData("CREATE ROUTE r WITH ADDRESS = N'LOCAL', SERVICE_NAME = 's'")]
	[InlineData("ALTER ROUTE r WITH SERVICE_NAME = 's'")]
	[InlineData("ALTER ROUTE r WITH ADDRESS = 'x', SERVICE_NAME = 's'")]
	[InlineData("CREATE REMOTE SERVICE BINDING b TO SERVICE 's' WITH USER = u")]
	[InlineData("CREATE REMOTE SERVICE BINDING b AUTHORIZATION dbo TO SERVICE N's' WITH USER = [u], ANONYMOUS = ON")]
	[InlineData("CREATE REMOTE SERVICE BINDING b TO SERVICE 's' WITH ANONYMOUS = ON, USER = u")]
	[InlineData("ALTER REMOTE SERVICE BINDING b WITH ANONYMOUS = OFF")]
	[InlineData("ALTER REMOTE SERVICE BINDING b WITH ANONYMOUS = OFF, USER = u")]
	[InlineData("CREATE BROKER PRIORITY p FOR CONVERSATION")]
	[InlineData("CREATE BROKER PRIORITY p FOR CONVERSATION SET (CONTRACT_NAME = c, LOCAL_SERVICE_NAME = s, REMOTE_SERVICE_NAME = 'r', PRIORITY_LEVEL = 5)")]
	[InlineData("CREATE BROKER PRIORITY p FOR CONVERSATION SET (CONTRACT_NAME = ANY, LOCAL_SERVICE_NAME = ANY, REMOTE_SERVICE_NAME = ANY, PRIORITY_LEVEL = DEFAULT)")]
	[InlineData("CREATE BROKER PRIORITY p FOR CONVERSATION SET (PRIORITY_LEVEL = 5, LOCAL_SERVICE_NAME = [//s])")]
	[InlineData("ALTER BROKER PRIORITY p FOR CONVERSATION SET (PRIORITY_LEVEL = 3)")]
	public void Broker_objects_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Service Broker's conversations, as the engine answers them.</summary>
	[Theory]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE t")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE 's' TO SERVICE 't'")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't' WITH LIFETIME = 60 ENCRYPTION = OFF")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't' WITH RELATED_CONVERSATION = @h, RELATED_CONVERSATION_GROUP = @h")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't' WITH LIFETIME = 60, LIFETIME = 60")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't' ON CONTRACT 'c'")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't', 'x', 'y'")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't' WITH")]
	[InlineData("BEGIN DIALOG h FROM SERVICE s TO SERVICE 't'")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't' WITH ENCRYPTION = 1")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't' WITH FOO = 1")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't' + 'u'")]
	[InlineData("BEGIN CONVERSATION TIMER @h TIMEOUT = 120")]
	[InlineData("BEGIN CONVERSATION TIMER (@h) TIMEOUT 120")]
	[InlineData("BEGIN CONVERSATION TIMER (@h, @h) TIMEOUT = 1")]
	[InlineData("END CONVERSATION @h WITH ERROR = 1, DESCRIPTION = 'x'")]
	[InlineData("END CONVERSATION @h WITH ERROR = 1")]
	[InlineData("END CONVERSATION @h WITH ERROR = -1 DESCRIPTION = N'x'")]
	[InlineData("END CONVERSATION @h WITH ERROR = 1 + 1 DESCRIPTION = 'x'")]
	[InlineData("END CONVERSATION @h WITH ERROR = 1 DESCRIPTION = 'a' + 'b'")]
	[InlineData("END CONVERSATION @h WITH ERROR = 'x' DESCRIPTION = 'x'")]
	[InlineData("END CONVERSATION @h WITH DESCRIPTION = 'x' ERROR = 1")]
	[InlineData("END CONVERSATION @h WITH CLEANUP, CLEANUP")]
	[InlineData("GET CONVERSATION GROUP @g FROM q, TIMEOUT 60")]
	[InlineData("WAITFOR (GET CONVERSATION GROUP @g FROM q), TIMEOUT 60 + 1")]
	[InlineData("GET CONVERSATION GROUP 'x' FROM q")]
	[InlineData("WAITFOR (GET CONVERSATION GROUP @g FROM q), TIMEOUT 'x'")]
	[InlineData("WAITFOR (GET CONVERSATION GROUP @g FROM q), TIMEOUT (60)")]
	[InlineData("WAITFOR ((GET CONVERSATION GROUP @g FROM q))")]
	[InlineData("RECEIVE TOP 1 * FROM q")]
	[InlineData("RECEIVE * FROM q WHERE a = 1")]
	[InlineData("RECEIVE * FROM q WHERE conversation_handle = @h AND conversation_group_id = @h")]
	[InlineData("RECEIVE DISTINCT * FROM q")]
	[InlineData("RECEIVE * FROM q ORDER BY 1")]
	[InlineData("WAITFOR (SELECT 1)")]
	[InlineData("WAITFOR (RECEIVE * FROM q) TIMEOUT 100")]
	[InlineData("RECEIVE * FROM q, TIMEOUT 100")]
	[InlineData("RECEIVE TOP (1) PERCENT * FROM q")]
	[InlineData("RECEIVE q.* FROM q")]
	[InlineData("RECEIVE x = a FROM q")]
	[InlineData("RECEIVE a FROM q AS z")]
	[InlineData("RECEIVE a FROM q WITH (NOLOCK)")]
	[InlineData("RECEIVE a FROM @t")]
	[InlineData("RECEIVE * FROM q WHERE conversation_handle = @h + 1")]
	[InlineData("RECEIVE * FROM q WHERE [conversation_handle] = 'x'")]
	[InlineData("RECEIVE * FROM q WHERE 'x' = conversation_handle")]
	[InlineData("RECEIVE * FROM q INTO t")]
	[InlineData("RECEIVE * FROM q WHERE conversation_group_id = 'x' INTO @t")]
	[InlineData("RECEIVE TOP (1) FROM q")]
	[InlineData("WAITFOR (RECEIVE * FROM q), TIMEOUT 100, TIMEOUT 5")]
	[InlineData("SEND ON CONVERSATION @h, @h")]
	[InlineData("SEND ON CONVERSATION @h MESSAGE TYPE 'm'")]
	[InlineData("SEND ON CONVERSATION @h ('x') ('y')")]
	[InlineData("SEND ON CONVERSATION @h MESSAGE TYPE m (SELECT 1)")]
	[InlineData("SEND ON CONVERSATION @h MESSAGE TYPE m ()")]
	public void Conversations_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("BEGIN DIALOG CONVERSATION @h FROM SERVICE s TO SERVICE 't'")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE [s] TO SERVICE 't', 'CURRENT DATABASE' ON CONTRACT c WITH RELATED_CONVERSATION = @h, LIFETIME = 60, ENCRYPTION = OFF")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE N't'")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE @t")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE a.b TO SERVICE 't'")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't' WITH ENCRYPTION = OFF, LIFETIME = 60")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't' WITH RELATED_CONVERSATION_GROUP = @h")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't' WITH LIFETIME = 1 + 1")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't' ON CONTRACT [c]")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE s TO SERVICE 't', @i")]
	[InlineData("BEGIN DIALOG @h FROM SERVICE [//a/b] TO SERVICE '//t' WITH RELATED_CONVERSATION = 'x'")]
	[InlineData("BEGIN CONVERSATION TIMER (@h) TIMEOUT = 120")]
	[InlineData("BEGIN CONVERSATION TIMER (@h) TIMEOUT = 60 * 2")]
	[InlineData("BEGIN CONVERSATION TIMER ('x') TIMEOUT = 1")]
	[InlineData("END CONVERSATION @h")]
	[InlineData("END CONVERSATION @h WITH CLEANUP")]
	[InlineData("END CONVERSATION @h WITH ERROR = 1 DESCRIPTION = 'x'")]
	[InlineData("END CONVERSATION @h WITH ERROR = @e DESCRIPTION = @d")]
	[InlineData("END CONVERSATION @h WITH ERROR = 1.5 DESCRIPTION = N'x'")]
	[InlineData("END CONVERSATION (@h)")]
	[InlineData("END CONVERSATION 'x'")]
	[InlineData("END CONVERSATION f()")]
	[InlineData("END CONVERSATION @h + 1")]
	[InlineData("MOVE CONVERSATION @h TO @h")]
	[InlineData("MOVE CONVERSATION (@h) TO 'x'")]
	[InlineData("MOVE CONVERSATION a TO b")]
	[InlineData("GET CONVERSATION GROUP @g FROM q")]
	[InlineData("GET CONVERSATION GROUP @g FROM [q]")]
	[InlineData("WAITFOR (GET CONVERSATION GROUP @g FROM db.dbo.q), TIMEOUT 60")]
	[InlineData("WAITFOR (GET CONVERSATION GROUP @g FROM q)")]
	[InlineData("WAITFOR (GET CONVERSATION GROUP @g FROM q), TIMEOUT @n")]
	[InlineData("WAITFOR (GET CONVERSATION GROUP @g FROM q), TIMEOUT -1")]
	[InlineData("RECEIVE * FROM q")]
	[InlineData("RECEIVE TOP (1) * FROM q")]
	[InlineData("RECEIVE TOP (@n) * FROM q")]
	[InlineData("RECEIVE TOP (1 + 1) * FROM q")]
	[InlineData("RECEIVE conversation_handle, message_body FROM q")]
	[InlineData("RECEIVE a AS x, CAST(b AS INT) y, * FROM q")]
	[InlineData("RECEIVE * FROM q INTO @t")]
	[InlineData("RECEIVE * FROM q WHERE conversation_handle = @h")]
	[InlineData("RECEIVE * FROM q WHERE CONVERSATION_GROUP_ID = 'x'")]
	[InlineData("RECEIVE @x = a FROM q")]
	[InlineData("RECEIVE * FROM db.dbo.q")]
	[InlineData("RECEIVE 'a' AS x, (SELECT 1) FROM q")]
	[InlineData("RECEIVE * FROM q INTO @t WHERE conversation_group_id = 'x'")]
	[InlineData("WAITFOR (RECEIVE * FROM q), TIMEOUT 100")]
	[InlineData("WAITFOR (RECEIVE TOP (1) * FROM q)")]
	[InlineData("SEND ON CONVERSATION @h")]
	[InlineData("SEND ON CONVERSATION @h MESSAGE TYPE m (N'x')")]
	[InlineData("SEND ON CONVERSATION (@h, @h) MESSAGE TYPE [m]")]
	[InlineData("SEND ON CONVERSATION @h (N'x' + N'y')")]
	[InlineData("SEND ON CONVERSATION @h MESSAGE TYPE a.b")]
	[InlineData("SEND ON CONVERSATION (@h)")]
	[InlineData("SEND ON CONVERSATION 'x'")]
	[InlineData("SEND ON CONVERSATION (@@x)")]
	[InlineData("SEND ON CONVERSATION (@h + 1, 'x')")]
	[InlineData("SEND ON CONVERSATION @h MESSAGE TYPE m (NULL)")]
	[InlineData("SEND ON CONVERSATION @h MESSAGE TYPE [//a/b] (N'x')")]
	public void Conversations_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>DBCC, as the engine answers it.</summary>
	[Theory]
	[InlineData("DBCC [FOO]")]
	[InlineData("DBCC 'FOO'")]
	[InlineData("DBCC FOO.BAR")]
	[InlineData("DBCC FOO BAR")]
	[InlineData("DBCC @x")]
	[InlineData("DBCC SELECT")]
	[InlineData("DBCC CHECKDB (db) WITH")]
	[InlineData("DBCC CHECKIDENT ('t', RESEED, 1 + 1)")]
	[InlineData("DBCC CHECKIDENT (t.u)")]
	[InlineData("DBCC FREEPROCCACHE (COMPUTE)")]
	[InlineData("DBCC CHECKDB WITH NO_INFOMSGS NO_INFOMSGS")]
	[InlineData("DBCC CHECKDB (db) WITH MAXDOP = 1 + 1")]
	[InlineData("DBCC CHECKDB (db) WITH TABLERESULTS = ON")]
	[InlineData("DBCC FOO WITH BAR = 1")]
	[InlineData("DBCC FOO (DEFAULT)")]
	[InlineData("DBCC FOO ('a' + 'b')")]
	[InlineData("DBCC FOO (+2)")]
	[InlineData("DBCC FOO (a.b)")]
	[InlineData("DBCC FOO (f())")]
	[InlineData("DBCC FOO (-@n)")]
	[InlineData("DBCC FOO (-'1')")]
	[InlineData("DBCC FOO (a,)")]
	[InlineData("DBCC FOO (,a)")]
	[InlineData("DBCC FOO (a) (b)")]
	[InlineData("DBCC FOO ((1))")]
	[InlineData("DBCC FOO WITH NO_INFOMSGS WITH TABLERESULTS")]
	[InlineData("DBCC FOO WITH MAXDOP = @n")]
	[InlineData("DBCC FOO WITH MAXDOP = -1")]
	[InlineData("DBCC FOO WITH MAXDOP = 'x'")]
	[InlineData("DBCC FOO WITH MAXDOP 4")]
	[InlineData("DBCC FOO WITH MAXDOP = 1e1")]
	[InlineData("DBCC FOO WITH WAIT_AT_LOW_PRIORITY ()")]
	[InlineData("DBCC FOO WITH BAR (ABORT_AFTER_WAIT = SELF)")]
	[InlineData("DBCC FOO WITH [NO_INFOMSGS]")]
	[InlineData("DBCC FOO WITH 'x'")]
	[InlineData("DBCC FOO WITH ALL")]
	[InlineData("DBCC FOO WITH WAIT_AT_LOW_PRIORITY (ABORT_AFTER_WAIT = SELF, ABORT_AFTER_WAIT = NONE)")]
	[InlineData("DBCC FOO WITH WAIT_AT_LOW_PRIORITY (X = 'a')")]
	[InlineData("DBCC FOO WITH WAIT_AT_LOW_PRIORITY (ABORT_AFTER_WAIT = [SELF])")]
	[InlineData("DBCC FOO WITH ABORT_AFTER_WAIT = SELF")]
	[InlineData("DBCC SHRINKLOG (SIZE = 10 GB)")]
	[InlineData("DBCC SHRINKLOG (SIZE = DEFAULT)")]
	[InlineData("DBCC FOO ([a] = 1)")]
	[InlineData("DBCC FOO (KEY)")]
	[InlineData("DBCC FOO (SELECT 1 AS a)")]
	public void Dbcc_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them, the commands it does not document among them.</summary>
	[Theory]
	[InlineData("DBCC CHECKDB")]
	[InlineData("DBCC FOO")]
	[InlineData("DBCC PAGE (1, 1, 1, 3)")]
	[InlineData("DBCC CHECKDB (N'db', NOINDEX) WITH NO_INFOMSGS, ALL_ERRORMSGS")]
	[InlineData("DBCC CHECKDB (db, REPAIR_ALLOW_DATA_LOSS) WITH MAXDOP = 4")]
	[InlineData("DBCC CHECKDB ()")]
	[InlineData("DBCC CHECKIDENT ('t', RESEED, -10)")]
	[InlineData("DBCC CHECKIDENT ([dbo.t])")]
	[InlineData("DBCC FREEPROCCACHE (0x0600)")]
	[InlineData("DBCC FREESYSTEMCACHE ('ALL', [default]) WITH MARK_IN_USE_FOR_REMOVAL")]
	[InlineData("DBCC SQLPERF ('sys.dm_os_wait_stats', CLEAR)")]
	[InlineData("DBCC TRACEON (3205, 1204, -1) WITH NO_INFOMSGS")]
	[InlineData("DBCC SHOW_STATISTICS (\"Person.Address\", AK_Address_rowguid)")]
	[InlineData("dbcc checkdb")]
	[InlineData("DBCC xp_sample (FREE)")]
	[InlineData("DBCC FOO (- 2)")]
	[InlineData("DBCC FOO ($3)")]
	[InlineData("DBCC FOO (1e1)")]
	[InlineData("DBCC FOO (1.)")]
	[InlineData("DBCC FOO (.5)")]
	[InlineData("DBCC FOO (NULL)")]
	[InlineData("DBCC FOO (@@x)")]
	[InlineData("DBCC FOO (#t)")]
	[InlineData("DBCC FOO (NEXT)")]
	[InlineData("DBCC FOO (a = 'x', @x = 1, b = [c])")]
	[InlineData("DBCC SHRINKLOG (SIZE = 10)")]
	[InlineData("DBCC FOO WITH maxdop = 4, MAXDOP, NO_INFOMSGS")]
	[InlineData("DBCC FOO WITH MAXDOP = .5")]
	[InlineData("DBCC FOO WITH WAIT_AT_LOW_PRIORITY")]
	[InlineData("DBCC FOO WITH WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1 MINUTES, ABORT_AFTER_WAIT = SELF), NO_INFOMSGS")]
	[InlineData("DBCC FOO WITH wait_at_low_priority (abort_after_wait = foo)")]
	[InlineData("DBCC FOO WITH #x, _x, x1")]
	[InlineData("DBCC _x")]
	[InlineData("DBCC #x")]
	[InlineData("DBCC FOO(1)")]
	[InlineData("DBCC CHECKDB WITH NO_INFOMSGS, NO_INFOMSGS")]
	public void Dbcc_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Cursors, as the engine answers them.</summary>
	[Theory]
	[InlineData("DECLARE c CURSOR LOCAL GLOBAL FOR SELECT 1")]
	[InlineData("DECLARE c CURSOR STATIC DYNAMIC FOR SELECT 1")]
	[InlineData("DECLARE c CURSOR FAST_FORWARD SCROLL FOR SELECT 1")]
	[InlineData("DECLARE c CURSOR FAST_FORWARD OPTIMISTIC FOR SELECT 1")]
	[InlineData("DECLARE c CURSOR STATIC FOR SELECT a FROM t FOR UPDATE")]
	[InlineData("DECLARE c CURSOR READ_ONLY FOR SELECT a FROM t FOR READ ONLY")]
	[InlineData("DECLARE c CURSOR SCROLL_LOCKS FOR SELECT a FROM t FOR READ ONLY")]
	[InlineData("DECLARE c CURSOR INSENSITIVE FOR SELECT 1")]
	[InlineData("DECLARE c LOCAL CURSOR FOR SELECT 1")]
	[InlineData("DECLARE c SCROLL CURSOR LOCAL FOR SELECT 1")]
	[InlineData("DECLARE c INSENSITIVE CURSOR FOR SELECT a FROM t FOR UPDATE OF a")]
	[InlineData("DECLARE c CURSOR FOR SELECT a FROM t FOR READ ONLY FOR UPDATE")]
	[InlineData("DECLARE c CURSOR FOR SELECT a INTO #t FROM t")]
	[InlineData("DECLARE c CURSOR FOR VALUES (1)")]
	[InlineData("DECLARE c CURSOR FOR SELECT 1 FOR XML PATH")]
	[InlineData("DECLARE c CURSOR FOR SELECT a FROM t FOR BROWSE")]
	[InlineData("DECLARE c CURSOR FOR (SELECT a FROM t ORDER BY a)")]
	[InlineData("DECLARE c CURSOR FOR SELECT a FROM t FOR UPDATE OF (a)")]
	[InlineData("DECLARE c CURSOR FOR EXEC p")]
	[InlineData("DECLARE c.d CURSOR FOR SELECT 1")]
	[InlineData("DECLARE c CURSOR")]
	[InlineData("SET @c = CURSOR INSENSITIVE FOR SELECT 1")]
	[InlineData("SET @c = INSENSITIVE CURSOR FOR SELECT 1")]
	[InlineData("SET @c = CURSOR LOCAL GLOBAL FOR SELECT 1")]
	[InlineData("SET @c += CURSOR FOR SELECT 1")]
	[InlineData("SET @c = (CURSOR FOR SELECT 1)")]
	[InlineData("SELECT @c = CURSOR FOR SELECT 1")]
	[InlineData("OPEN c.d")]
	[InlineData("OPEN 'c'")]
	[InlineData("OPEN GLOBAL @c")]
	[InlineData("DEALLOCATE CURSOR c")]
	[InlineData("CLOSE ALL")]
	[InlineData("FETCH NEXT c")]
	[InlineData("FETCH ABSOLUTE +1 FROM c")]
	[InlineData("FETCH ABSOLUTE 1 + 1 FROM c")]
	[InlineData("FETCH ABSOLUTE (1) FROM c")]
	[InlineData("FETCH ABSOLUTE '1' FROM c")]
	[InlineData("FETCH ABSOLUTE 1e1 FROM c")]
	[InlineData("FETCH ABSOLUTE 0x01 FROM c")]
	[InlineData("FETCH ABSOLUTE @@ROWCOUNT FROM c")]
	[InlineData("FETCH RELATIVE -@n FROM c")]
	[InlineData("FETCH RELATIVE FROM c")]
	[InlineData("FETCH c INTO @a,")]
	[InlineData("FETCH c INTO a")]
	[InlineData("FETCH NEXT FROM c INTO @@ROWCOUNT")]
	[InlineData("FETCH NEXT FROM GLOBAL @c")]
	[InlineData("FETCH NEXT FROM c.d")]
	[InlineData("FETCH c INTO @a, @@ERROR")]
	[InlineData("OPEN @@ROWCOUNT")]
	[InlineData("FETCH ABSOLUTE @@error FROM c")]
	[InlineData("CREATE PROCEDURE p @c CURSOR OUTPUT AS SELECT 1")]
	[InlineData("CREATE PROCEDURE p @c CURSOR VARYING AS SELECT 1")]
	[InlineData("CREATE PROCEDURE p @c CURSOR AS SELECT 1")]
	[InlineData("CREATE PROCEDURE p @c CURSOR VARYING OUTPUT READONLY AS SELECT 1")]
	[InlineData("CREATE PROCEDURE p @c CURSOR = NULL VARYING OUTPUT AS SELECT 1")]
	[InlineData("CREATE PROCEDURE p @c CURSOR VARYING = NULL OUTPUT AS SELECT 1")]
	[InlineData("CREATE PROCEDURE p @c CURSOR NULL VARYING OUTPUT AS SELECT 1")]
	[InlineData("CREATE PROCEDURE p @c INT VARYING OUTPUT AS SELECT 1")]
	[InlineData("CREATE FUNCTION f (@c CURSOR VARYING OUTPUT) RETURNS INT AS BEGIN RETURN 1 END")]
	[InlineData("OPEN master")]
	[InlineData("OPEN symmetric")]
	[InlineData("OPEN asymmetric")]
	[InlineData("CLOSE master")]
	[InlineData("CLOSE symmetric")]
	[InlineData("CLOSE asymmetric")]
	public void Cursors_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("DECLARE c CURSOR FOR SELECT 1")]
	[InlineData("DECLARE c CURSOR LOCAL SCROLL STATIC READ_ONLY TYPE_WARNING FOR SELECT 1")]
	[InlineData("DECLARE c CURSOR TYPE_WARNING LOCAL LOCAL FOR SELECT 1")]
	[InlineData("DECLARE c CURSOR FORWARD_ONLY FAST_FORWARD READ_ONLY FOR SELECT 1")]
	[InlineData("DECLARE c CURSOR SCROLL FOR SELECT a FROM t FOR UPDATE")]
	[InlineData("DECLARE c CURSOR FAST_FORWARD FOR SELECT a FROM t FOR READ ONLY")]
	[InlineData("DECLARE c INSENSITIVE CURSOR FOR SELECT 1")]
	[InlineData("DECLARE c SCROLL INSENSITIVE CURSOR FOR SELECT a FROM t FOR READ ONLY")]
	[InlineData("DECLARE c INSENSITIVE INSENSITIVE CURSOR FOR SELECT 1")]
	[InlineData("DECLARE c SCROLL CURSOR FOR SELECT a FROM t FOR UPDATE OF a")]
	[InlineData("DECLARE [c] CURSOR FOR SELECT a FROM t ORDER BY a OFFSET 0 ROWS")]
	[InlineData("DECLARE c CURSOR FOR WITH x AS (SELECT 1 a) SELECT a FROM x ORDER BY a FOR READ ONLY")]
	[InlineData("DECLARE c CURSOR FOR WITH XMLNAMESPACES ('u' AS p) SELECT 1")]
	[InlineData("DECLARE c CURSOR FOR SELECT a FROM t OPTION (MAXDOP 1) FOR READ ONLY")]
	[InlineData("DECLARE c CURSOR FOR SELECT a FROM t FOR UPDATE OF dbo.t.a, b OPTION (MAXDOP 1)")]
	[InlineData("DECLARE c CURSOR FOR (SELECT 1) ORDER BY 1")]
	[InlineData("DECLARE c CURSOR FOR SELECT 1 UNION SELECT 2 ORDER BY 1")]
	[InlineData("SET @c = CURSOR FOR SELECT 1")]
	[InlineData("SET @c = CURSOR LOCAL SCROLL FOR SELECT 1 FOR READ ONLY")]
	[InlineData("SET @c = CURSOR READ_ONLY FOR SELECT a FROM t FOR READ ONLY")]
	[InlineData("SET @c = CURSOR FOR WITH x AS (SELECT 1 a) SELECT a FROM x OPTION (MAXDOP 1)")]
	[InlineData("SET @c = c")]
	[InlineData("OPEN c")]
	[InlineData("OPEN GLOBAL c")]
	[InlineData("OPEN [c]")]
	[InlineData("OPEN @c")]
	[InlineData("OPEN GLOBAL")]
	[InlineData("OPEN GLOBAL GLOBAL")]
	[InlineData("CLOSE GLOBAL c")]
	[InlineData("CLOSE @c")]
	[InlineData("DEALLOCATE c")]
	[InlineData("DEALLOCATE GLOBAL c")]
	[InlineData("DEALLOCATE @c")]
	[InlineData("FETCH c")]
	[InlineData("FETCH FROM c")]
	[InlineData("FETCH GLOBAL c")]
	[InlineData("FETCH next from c")]
	[InlineData("FETCH PRIOR FROM GLOBAL c")]
	[InlineData("FETCH LAST FROM @c")]
	[InlineData("FETCH ABSOLUTE -1 FROM c")]
	[InlineData("FETCH ABSOLUTE - 1 FROM c")]
	[InlineData("FETCH ABSOLUTE 1.5 FROM c")]
	[InlineData("FETCH RELATIVE @n FROM c")]
	[InlineData("FETCH NEXT FROM c INTO @a, @b")]
	[InlineData("FETCH FROM GLOBAL")]
	[InlineData("FETCH NEXT")]
	[InlineData("FETCH ABSOLUTE")]
	[InlineData("FETCH NEXT FROM NEXT")]
	[InlineData("OPEN @@x")]
	[InlineData("FETCH ABSOLUTE @@x FROM c INTO @a, @@b")]
	[InlineData("CREATE PROCEDURE p @c CURSOR VARYING OUTPUT AS SELECT 1")]
	[InlineData("CREATE PROCEDURE p @c CURSOR VARYING OUT AS SELECT 1")]
	[InlineData("CREATE PROCEDURE p @c AS CURSOR VARYING OUTPUT AS SELECT 1")]
	[InlineData("CREATE PROCEDURE p (@c CURSOR VARYING OUTPUT, @a INT) AS SELECT 1")]
	[InlineData("OPEN [master]")]
	[InlineData("OPEN GLOBAL master")]
	[InlineData("CLOSE [symmetric]")]
	[InlineData("DEALLOCATE master")]
	[InlineData("FETCH master")]
	[InlineData("OPEN mastery")]
	[InlineData("OPEN keys")]
	public void Cursors_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>A collation in <c>OPENJSON</c>'s schema, as the engine answers it.</summary>
	[Theory]
	[InlineData("SELECT * FROM OPENJSON (N'[]') WITH (c NVARCHAR (MAX) N'$.c' COLLATE Latin1_General_Bin2)")]
	[InlineData("SELECT * FROM OPENJSON (N'[]') WITH (c NVARCHAR (MAX) AS JSON COLLATE Latin1_General_Bin2)")]
	[InlineData("SELECT * FROM OPENJSON (N'[]') WITH (c VARCHAR (10) COLLATE [Latin1_General_Bin2])")]
	[InlineData("SELECT * FROM OPENJSON (N'[]') WITH (c VARCHAR (10) COLLATE 'Latin1_General_Bin2')")]
	[InlineData("SELECT * FROM OPENJSON (N'[]') WITH (c VARCHAR (10) COLLATE Latin1_General_Bin2 COLLATE Latin1_General_CI_AS)")]
	[InlineData("SELECT * FROM OPENJSON (N'[]') WITH (c VARCHAR (10) COLLATE)")]
	[InlineData("SELECT * FROM OPENXML (@h, '/r') WITH (c VARCHAR (10) '@c' COLLATE Latin1_General_Bin2)")]
	[InlineData("SELECT * FROM OPENXML (@h, '/r') WITH (c VARCHAR (10) AS JSON)")]
	public void A_json_schema_refuses_the_collations_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the ones beside them.</summary>
	[Theory]
	[InlineData("SELECT * FROM OPENJSON (@var, N'$') WITH (a INT, c NVARCHAR (MAX) COLLATE Latin1_General_Bin2 N'$.b.c')")]
	[InlineData("SELECT * FROM OPENJSON (N'[]') WITH (c NVARCHAR (MAX) COLLATE Latin1_General_Bin2)")]
	[InlineData("SELECT * FROM OPENJSON (N'[]') WITH (f NVARCHAR (MAX) COLLATE Latin1_General_Bin2 N'$.b' AS JSON)")]
	[InlineData("SELECT * FROM OPENJSON (N'[]') WITH (c VARCHAR (10) COLLATE database_default, d INT '$.d')")]
	[InlineData("SELECT * FROM OPENXML (@h, '/r') WITH (c VARCHAR (10) COLLATE Latin1_General_Bin2 '@c')")]
	[InlineData("SELECT * FROM OPENXML (@h, '/r') WITH t")]
	public void A_json_schema_reads_the_collations_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The shape the rows come back in, as the engine answers it.</summary>
	[Theory]
	[InlineData("SELECT a FROM t FOR XML")]
	[InlineData("SELECT a FROM t FOR XML FOO")]
	[InlineData("SELECT a FROM t FOR XML RAW (r)")]
	[InlineData("SELECT a FROM t FOR XML RAW ()")]
	[InlineData("SELECT a FROM t FOR XML AUTO ELEMENTS")]
	[InlineData("SELECT a FROM t FOR XML AUTO, BINARY")]
	[InlineData("SELECT a FROM t FOR XML AUTO, ROOT (r)")]
	[InlineData("SELECT a FROM t FOR XML AUTO, ELEMENTS FOO")]
	[InlineData("SELECT a FROM t FOR XML AUTO, INCLUDE_NULL_VALUES")]
	[InlineData("SELECT a FROM t FOR JSON")]
	[InlineData("SELECT a FROM t FOR JSON RAW")]
	[InlineData("SELECT a FROM t FOR JSON PATH ('r')")]
	[InlineData("SELECT a FROM t FOR JSON PATH, TYPE")]
	[InlineData("SELECT a FROM t FOR JSON PATH, ELEMENTS")]
	[InlineData("SELECT a FROM t FOR BROWSE, TYPE")]
	[InlineData("SELECT a FROM t FOR READ")]
	public void The_row_shapes_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("SELECT a FROM t FOR XML AUTO, XMLDATA, ELEMENTS, BINARY BASE64")]
	[InlineData("SELECT a FROM t FOR XML RAW (N'r'), ELEMENTS XSINIL, XMLSCHEMA ('urn:x'), ROOT ('r'), TYPE, BINARY BASE64")]
	[InlineData("SELECT a FROM t FOR XML PATH (''), ELEMENTS ABSENT")]
	[InlineData("SELECT a FROM t FOR XML EXPLICIT, XMLDATA")]
	[InlineData("SELECT a FROM t FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER")]
	[InlineData("SELECT a FROM t FOR JSON AUTO, ROOT ('r')")]
	[InlineData("SELECT a FROM t FOR UPDATE")]
	[InlineData("SELECT a FROM t FOR UPDATE OF a, b")]
	[InlineData("SELECT a FROM t ORDER BY a FOR READ ONLY")]
	public void The_row_shapes_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The permission statements, as the engine answers them.</summary>
	[Theory]
	[InlineData("GRANT SELECT ON t () TO u")]
	[InlineData("GRANT SELECT ON t (s.c1) TO u")]
	[InlineData("GRANT SELECT ON t TO u AS NULL")]
	[InlineData("GRANT SELECT ON t TO 'u'")]
	[InlineData("GRANT SELECT ON t TO @u")]
	[InlineData("REVOKE SELECT ON t TO NULL AS NULL")]
	[InlineData("GRANT FOO TO u")]
	[InlineData("GRANT FOO BAR ON t TO u")]
	[InlineData("GRANT SELECT INSERT ON t TO u")]
	[InlineData("GRANT create control alter ON [a] TO NULL, user2 AS [all]")]
	[InlineData("GRANT CREATE TO u")]
	[InlineData("GRANT CREATE ANY FOO TO u")]
	[InlineData("GRANT VIEW FOO TO u")]
	public void The_permission_statements_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("GRANT SELECT ON t (c1, c2) TO u")]
	[InlineData("GRANT SELECT (c1) ON t (c2) TO u")]
	[InlineData("GRANT INSERT ON ..t1 (c1) TO PUBLIC")]
	[InlineData("DENY ALL PRIVILEGES ON ENDPOINT::a.b..d (c1, c2) TO PUBLIC")]
	[InlineData("REVOKE GRANT OPTION FOR CONTROL ON t1 (c1) TO PUBLIC AS [p1]")]
	[InlineData("GRANT SELECT ON t TO u, NULL")]
	[InlineData("DENY SELECT ON t TO NULL CASCADE")]
	[InlineData("REVOKE SELECT ON t FROM NULL")]
	[InlineData("GRANT ALL TO NULL WITH GRANT OPTION")]
	[InlineData("GRANT ALL, SELECT (c1, c2), INSERT, DELETE, UPDATE, EXEC, EXECUTE, REFERENCES (c1) ON t TO u")]
	[InlineData("GRANT ALL PRIVILEGES (c1) ON t TO u")]
	[InlineData("GRANT SELECT, ALL PRIVILEGES TO u")]
	[InlineData("GRANT ALTER ANY DATABASE EVENT SESSION ADD EVENT, ALTER ANY DATABASE, ALTER TO u")]
	[InlineData("GRANT VIEW ANY COLUMN ENCRYPTION KEY DEFINITION, CONNECT SQL, CONNECT TO u")]
	[InlineData("GRANT EXEC, EXECUTE ANY EXTERNAL SCRIPT ON p TO u")]
	[InlineData("GRANT SELECT ALL USER SECURABLES, TAKE OWNERSHIP TO u")]
	public void The_permission_statements_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>A table's constraints switched, and a graph table's columns carried by an index.</summary>
	[Theory]
	[InlineData("ALTER TABLE t WITH CHECK")]
	[InlineData("ALTER TABLE t WITH CHECK DROP CONSTRAINT c1")]
	[InlineData("ALTER TABLE t CHECK CONSTRAINT")]
	[InlineData("CREATE INDEX i ON n (c1) INCLUDE (t.c2)")]
	[InlineData("CREATE CLUSTERED COLUMNSTORE INDEX i ON n ORDER ($NODE_ID)")]
	public void Switched_constraints_and_carried_columns_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("ALTER TABLE t WITH NOCHECK CHECK CONSTRAINT ALL")]
	[InlineData("ALTER TABLE t WITH CHECK NOCHECK CONSTRAINT c1, c2")]
	[InlineData("CREATE INDEX i ON n (c1) INCLUDE ($node_id, c2)")]
	[InlineData("CREATE INDEX i ON e (c1) INCLUDE ($FROM_ID, $TO_ID, [$EDGE_ID])")]
	[InlineData("CREATE TABLE n (c1 INT, INDEX i (c1) INCLUDE ($NODE_ID)) AS NODE")]
	public void Switched_constraints_and_carried_columns_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Keys and certificates to a file and back, and a security policy, as the engine answers them.</summary>
	[Theory]
	[InlineData("BACKUP CERTIFICATE c TO FILE = @f")]
	[InlineData("BACKUP CERTIFICATE c TO URL = 'u'")]
	[InlineData("BACKUP CERTIFICATE s.c TO FILE = 'f'")]
	[InlineData("BACKUP CERTIFICATE c TO FILE = 'f' ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("BACKUP CERTIFICATE c TO FILE = 'f' WITH FORMAT = PFX, PRIVATE KEY (FILE = 'k', ENCRYPTION BY PASSWORD = 'p')")]
	[InlineData("BACKUP CERTIFICATE c TO FILE = 'f' WITH PRIVATE KEY (FILE = @k, ENCRYPTION BY PASSWORD = @p)")]
	[InlineData("BACKUP CERTIFICATE c TO FILE = 'f' WITH PRIVATE KEY ()")]
	[InlineData("BACKUP MASTER KEY TO FILE = 'f'")]
	[InlineData("BACKUP MASTER KEY TO FILE = 'f' ENCRYPTION BY PASSWORD = @p")]
	[InlineData("BACKUP MASTER KEY TO FILE = 'f' ENCRYPTION BY PASSWORD = 'p' FORCE")]
	[InlineData("BACKUP SERVICE MASTER KEY TO URL = 'u' ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("BACKUP SYMMETRIC KEY s.k TO FILE = 'f' ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("RESTORE MASTER KEY FROM FILE = 'f' ENCRYPTION BY PASSWORD = 'q' DECRYPTION BY PASSWORD = 'p'")]
	[InlineData("RESTORE MASTER KEY FROM FILE = 'f' DECRYPTION BY PASSWORD = 'p' FORCE ENCRYPTION BY PASSWORD = 'q'")]
	[InlineData("RESTORE SERVICE MASTER KEY FROM FILE = 'f' DECRYPTION BY PASSWORD = 'p' ENCRYPTION BY PASSWORD = 'q'")]
	[InlineData("RESTORE SYMMETRIC KEY k FROM FILE = 'f' DECRYPTION BY PASSWORD = 'p'")]
	[InlineData("ALTER SECURITY POLICY p NOT FOR REPLICATION")]
	[InlineData("ALTER SECURITY POLICY p WITH (STATE = ON) ADD NOT FOR REPLICATION")]
	[InlineData("CREATE SECURITY POLICY p ADD PREDICATE dbo.f(c) ON dbo.t")]
	[InlineData("CREATE SECURITY POLICY p ADD FILTER PREDICATE dbo.f(c) ON dbo.t AFTER INSERT")]
	[InlineData("CREATE SECURITY POLICY p ADD BLOCK PREDICATE dbo.f(c) ON dbo.t AFTER DELETE")]
	[InlineData("CREATE SECURITY POLICY p ADD BLOCK PREDICATE dbo.f(c) ON dbo.t BEFORE INSERT")]
	public void Keys_and_policies_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("BACKUP CERTIFICATE c TO FILE = N'f' WITH FORMAT = N'PFX', PRIVATE KEY (FILE = N'k', ENCRYPTION BY PASSWORD = N'p')")]
	[InlineData("BACKUP CERTIFICATE [c] TO FILE = 'f' WITH PRIVATE KEY (DECRYPTION BY PASSWORD = 'q', FILE = 'k', ENCRYPTION BY PASSWORD = 'p')")]
	[InlineData("BACKUP MASTER KEY TO URL = 'u' ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("BACKUP SYMMETRIC KEY k TO URL = 'u' ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("RESTORE MASTER KEY FROM URL = 'u' DECRYPTION BY PASSWORD = 'p' ENCRYPTION BY PASSWORD = 'q' FORCE")]
	[InlineData("RESTORE SERVICE MASTER KEY FROM FILE = 'f' DECRYPTION BY PASSWORD = 'p' FORCE")]
	[InlineData("RESTORE SYMMETRIC KEY [k] FROM URL = 'u' DECRYPTION BY PASSWORD = 'p' ENCRYPTION BY PASSWORD = 'q' FORCE")]
	[InlineData("ALTER SECURITY POLICY p ADD NOT FOR REPLICATION")]
	[InlineData("ALTER SECURITY POLICY p DROP NOT FOR REPLICATION")]
	[InlineData("CREATE SECURITY POLICY p ADD BLOCK PREDICATE dbo.f(c) ON dbo.t BEFORE DELETE NOT FOR REPLICATION")]
	[InlineData("ALTER SECURITY POLICY p DROP BLOCK PREDICATE ON dbo.t AFTER INSERT")]
	public void Keys_and_policies_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The SET statements, as the engine answers them with its variables declared.</summary>
	[Theory]
	[InlineData("SET DATEFIRST 7 + 1")]
	[InlineData("SET DEADLOCK_PRIORITY 5 + 1")]
	[InlineData("SET LOCK_TIMEOUT @i")]
	[InlineData("SET LOCK_TIMEOUT '10'")]
	[InlineData("SET LOCK_TIMEOUT 1.5")]
	[InlineData("SET LOCK_TIMEOUT x")]
	[InlineData("SET FIPS_FLAGGER ON")]
	[InlineData("SET FIPS_FLAGGER @s")]
	[InlineData("SET FIPS_FLAGGER N'FULL'")]
	[InlineData("SET QUERY_GOVERNOR_COST_LIMIT @i")]
	[InlineData("SET QUERY_GOVERNOR_COST_LIMIT '10'")]
	[InlineData("SET ROWCOUNT -1")]
	[InlineData("SET ROWCOUNT '5'")]
	[InlineData("SET ROWCOUNT x")]
	[InlineData("SET TEXTSIZE @i")]
	[InlineData("SET TEXTSIZE '10'")]
	[InlineData("SET USER 'x'")]
	[InlineData("SET NOCOUNT")]
	[InlineData("SET NOCOUNT 1")]
	[InlineData("SET NOCOUNT 'ON'")]
	[InlineData("SET PARSEONLY")]
	[InlineData("SET LANGUAGE")]
	[InlineData("SET NOCOUNT, STATISTICS IO ON")]
	[InlineData("SET STATISTICS IO")]
	[InlineData("SET OFFSETS SELECT")]
	[InlineData("SET IDENTITY_INSERT t 1")]
	[InlineData("SET TRANSACTION ISOLATION LEVEL FOO")]
	[InlineData("SET LOCK_TIMEOUT 10,")]
	[InlineData("SET LOCK_TIMEOUT 1000, ROWCOUNT 5")]
	[InlineData("SET NOCOUNT ON, LOCK_TIMEOUT 10")]
	[InlineData("SET @a = 1, LOCK_TIMEOUT 10")]
	[InlineData("SET FIPS_FLAGGER OFF, DATEFIRST 7")]
	[InlineData("SET DATEFIRST 7, FIPS_FLAGGER OFF")]
	[InlineData("SET DATEFIRST 7, ERRLVL 0")]
	[InlineData("SET DATEFIRST 7, NOCOUNT OFF")]
	[InlineData("SET NOCOUNT ON, XACT_ABORT ON")]
	[InlineData("SET STATISTICS IO OFF, TIME OFF")]
	[InlineData("SET OFFSETS GROUP ON")]
	[InlineData("SET [NOCOUNT] ON")]
	[InlineData("SET NOCOUNTON")]
	public void The_set_statements_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>
	/// A name the engine does not know, which it parses with whatever follows it to the end of
	/// the statement and refuses when the statement runs: `Msg 195`, "not a recognized SET
	/// option", or statistics option, or offset option.
	/// </summary>
	[Theory]
	[InlineData("SET FOO ON")]
	[InlineData("SET FOO 1")]
	[InlineData("SET A, B")]
	[InlineData("SET A, B 5")]
	[InlineData("SET DATEFIRST 1, FOO 2")]
	[InlineData("SET NOCOUNT, FOO 1")]
	[InlineData("SET DATEFIRST ON")]
	[InlineData("SET LANGUAGE OFF")]
	[InlineData("SET LOCK_TIMEOUT, DEADLOCK_PRIORITY 5")]
	[InlineData("SET DISABLE_DEF_CNST_CHK ON")]
	[InlineData("SET STATISTICS FOO ON")]
	[InlineData("SET STATISTICS IO, NOCOUNT ON")]
	[InlineData("SET OFFSETS FOO ON")]
	public void The_set_statements_refuse_the_names_the_engine_does_not_know(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("SET DATEFIRST -1")]
	[InlineData("SET DATEFIRST x")]
	[InlineData("SET DATEFORMAT N'dmy'")]
	[InlineData("SET DEADLOCK_PRIORITY -10")]
	[InlineData("SET DEADLOCK_PRIORITY @s")]
	[InlineData("SET LOCK_TIMEOUT -1")]
	[InlineData("SET LANGUAGE [us_english]")]
	[InlineData("SET FIPS_FLAGGER 'FULL'")]
	[InlineData("SET FIPS_FLAGGER OFF")]
	[InlineData("SET QUERY_GOVERNOR_COST_LIMIT 1.5")]
	[InlineData("SET CONTEXT_INFO @b")]
	[InlineData("SET ROWCOUNT @i")]
	[InlineData("SET TEXTSIZE -1")]
	[InlineData("SET ERRLVL 1")]
	[InlineData("SET ANSI_NULLS, NOCOUNT, XACT_ABORT OFF")]
	[InlineData("SET QUOTED_IDENTIFIER, NO_BROWSETABLE ON")]
	[InlineData("SET NOCOUNT, NOCOUNT OFF")]
	[InlineData("SET RESULT_SET_CACHING ON")]
	[InlineData("SET RECOMMENDATIONS OFF")]
	[InlineData("SET FIPS_FLAGGER 'FULL', QUERY_GOVERNOR_COST_LIMIT 10")]
	[InlineData("SET QUERY_GOVERNOR_COST_LIMIT 10, FIPS_FLAGGER 'ENTRY'")]
	[InlineData("SET STATISTICS IO, TIME ON")]
	[InlineData("SET STATISTICS PROFILE, XML OFF")]
	[InlineData("SET OFFSETS SELECT, FROM, ORDER, COMPUTE, TABLE, PROCEDURE, EXECUTE, STATEMENT, PARAM ON")]
	[InlineData("SET OFFSETS PROC, EXEC OFF")]
	[InlineData("SET IDENTITY_INSERT d.s.t ON")]
	[InlineData("SET TRAN ISOLATION LEVEL SNAPSHOT")]
	[InlineData("SET DATEFIRST 1, DATEFORMAT dmy")]
	[InlineData("SET DATEFIRST 7, DATEFIRST 7")]
	[InlineData("SET LANGUAGE us_english, DATEFIRST 7")]
	[InlineData("SET CONTEXT_INFO 0x01, DATEFIRST 7")]
	[InlineData("SET DATEFIRST 7, QUERY_GOVERNOR_COST_LIMIT 0")]
	[InlineData("SET LOCK_TIMEOUT -1, DEADLOCK_PRIORITY NORMAL")]
	public void The_set_statements_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>
	/// The graph pattern, as the engine answers it at every level: no comma and no brackets,
	/// which the syntax shows, a path of any length only inside `SHORTEST_PATH`, one to it,
	/// and `LAST_NODE` only at its ends.
	/// </summary>
	[Theory]
	[InlineData("SELECT * FROM a WHERE MATCH(N-(E)->N2, N2-(E2)->N3)")]
	[InlineData("SELECT * FROM a WHERE MATCH((N-(E)->N2) AND N2-(E)->N3)")]
	[InlineData("SELECT * FROM a WHERE MATCH(N(-(E)->N2)+)")]
	[InlineData("SELECT * FROM a WHERE MATCH(N = N2)")]
	[InlineData("SELECT * FROM a WHERE MATCH(LAST_NODE(N) = N2)")]
	[InlineData("SELECT * FROM a WHERE MATCH(LAST_NODE(dbo.N) = LAST_NODE(N3))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH(N-(E)->N2))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH(N(-(E)->N2)))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH(N(-(E)->N2)+ AND N2(-(E)->N3)+))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH((LAST_NODE(N)-(E)->)+N2))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH(N(-(E)->LAST_NODE(N2))+))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH((N-(E)->N2)+N3))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH((N-(E)->N2-(E2)->)+))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH(N(-(E)->N2){,3}))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH((N-(E)->){3}N2))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH(N(-(E)->N2){1,3}+))")]
	[InlineData("SELECT * FROM (SELECT * FROM t) FOR PATH")]
	[InlineData("SELECT * FROM (SELECT * FROM t) AS x FOR PATH")]
	[InlineData("SELECT * FROM (SELECT * FROM t) FOR SYSTEM_TIME ALL AS x")]
	[InlineData("SELECT * FROM t FOR PATH FOR SYSTEM_TIME ALL AS x")]
	[InlineData("SELECT * FROM dbo.f() FOR PATH AS x")]
	[InlineData("SELECT * FROM @t FOR PATH AS x")]
	[InlineData("SELECT * FROM OPENJSON('[]') FOR PATH AS x")]
	public void The_graph_pattern_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH((N-(E)->N2-(E2)->)+N3))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH((N-(E)->N2<-(E2)-)+N3))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH(N(-(E)->N2-(E2)->N3)+))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH(N(-(E)->N2){1,}))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH(N(-(E)->N2){ 1 , 3 }))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH(LAST_NODE(N)(-(E)->N2)+))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH((N-(E)->){1,5}LAST_NODE(N2)))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH(N(-(E)->N2)+) AND SHORTEST_PATH(N(-(E)->N3)+))")]
	[InlineData("SELECT * FROM a WHERE MATCH(SHORTEST_PATH(N(-(E)->N2)+) AND LAST_NODE(N2) = LAST_NODE(N3))")]
	[InlineData("SELECT * FROM a WHERE MATCH(LAST_NODE(N)-(E)->N2)")]
	[InlineData("SELECT * FROM a WHERE MATCH(N-(E)->LAST_NODE(N2)-(E2)->N3)")]
	[InlineData("SELECT * FROM a WHERE MATCH([N]-([E])->[N2])")]
	[InlineData("SELECT * FROM (SELECT * FROM t) FOR PATH AS x")]
	[InlineData("SELECT * FROM (SELECT * FROM t) FOR PATH x (c1)")]
	[InlineData("SELECT * FROM (VALUES (1)) FOR PATH AS x (c)")]
	[InlineData("SELECT * FROM t FOR SYSTEM_TIME ALL FOR PATH AS x")]
	[InlineData("SELECT * FROM t FOR PATH AS x WITH (NOLOCK)")]
	[InlineData("SELECT * FROM (SELECT * FROM t) FOR PATH AS x, (SELECT * FROM u) FOR PATH AS y WHERE MATCH(SHORTEST_PATH(x(-(y)->x)+))")]
	public void The_graph_pattern_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>
	/// The reference's examples the grammar read and the engine refused, and the forms beside
	/// them: a comment in a comment, a date part, a row of `VALUES`, the words the engine
	/// reserves, a selective XML index's name and a search property's options.
	/// </summary>
	[Theory]
	[InlineData("/* a /* b */ SELECT 1")]
	[InlineData("SELECT 1 /* /* */")]
	[InlineData("SELECT DATENAME(datepart, SYSDATETIME())")]
	[InlineData("SELECT DATEPART(foo, SYSDATETIME())")]
	[InlineData("SELECT DATEADD(weekdays, 1, SYSDATETIME())")]
	[InlineData("SELECT DATEPART([foo], SYSDATETIME())")]
	[InlineData("SELECT DATEPART(t.a, SYSDATETIME()) FROM t")]
	[InlineData("INSERT INTO t (a, b) VALUES ('Helmet', 25.50), (SELECT a, b FROM u)")]
	[InlineData("INSERT INTO t (a) VALUES (SELECT a FROM u)")]
	[InlineData("INSERT INTO t (a) VALUES 1, 2")]
	[InlineData("ALTER SEARCH PROPERTY LIST p ADD 'x' WITH (PROPERTY_DESCRIPTION = 'd', PROPERTY_SET_GUID = 'g', PROPERTY_INT_ID = 4)")]
	[InlineData("ALTER SEARCH PROPERTY LIST p ADD 'x' WITH (PROPERTY_SET_GUID = 'g')")]
	[InlineData("CREATE SELECTIVE XML INDEX ON T1(C1) FOR ( path1 = '/a' )")]
	[InlineData("SELECT external FROM t")]
	[InlineData("SELECT 1 AS merge")]
	[InlineData("SELECT t.pivot FROM t")]
	[InlineData("SELECT a FROM TABLESAMPLE")]
	public void The_reference_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("/* a /* b */ c */ SELECT 1")]
	[InlineData("/* /* */ */ SELECT 1")]
	[InlineData("SELECT /*/ 1 */ 1")]
	[InlineData("SELECT DATEPART(w, x), DATENAME(isowk, x), DATEADD(tz, 1, x), DATEDIFF_BIG(ns, 1, 2) FROM t")]
	[InlineData(@"SELECT DATEPART([yy], x), DATEPART(""yy"", x), DATEPART(YEAR, x), DATEPART((year), x) FROM t")]
	[InlineData("SELECT DATEPART(N'year', x), DATEPART(@p + N'', x), DATEPART(dbo.f(), x), DATEPART(1, x) FROM t")]
	[InlineData("SELECT DATETRUNC(dw, x), DATE_BUCKET(week, 1, x), dbo.DATEPART(foo, 1) FROM t")]
	[InlineData("INSERT INTO t (a) VALUES ((SELECT a FROM u))")]
	[InlineData("INSERT INTO t (a) VALUES (DEFAULT), (NULL), (1)")]
	[InlineData("SELECT * FROM (VALUES (1), ((SELECT 2))) AS v (a)")]
	[InlineData("ALTER SEARCH PROPERTY LIST p ADD 'x' WITH (PROPERTY_SET_GUID = 'g', PROPERTY_INT_ID = 4, PROPERTY_DESCRIPTION = 'd')")]
	[InlineData("ALTER SEARCH PROPERTY LIST p ADD N'x' WITH (PROPERTY_SET_GUID = N'g', PROPERTY_INT_ID = 4)")]
	[InlineData("CREATE SELECTIVE XML INDEX sxi ON T1(C1) WITH XMLNAMESPACES ('https://www.tempuri.org/' as myns) FOR ( path1 = '/myns:book/myns:author/text()' )")]
	[InlineData("SELECT a FROM [External].Orders")]
	[InlineData("SELECT * FROM SEMANTICKEYPHRASETABLE(d, c)")]
	[InlineData("SELECT disk FROM dump")]
	public void The_reference_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>A string run, its context and where it runs, as the engine answers it with its variables declared.</summary>
	[Theory]
	[InlineData("EXEC ('SELECT 1', 5, @a)")]
	[InlineData("EXEC ('SELECT 1') AS USER = @s")]
	[InlineData("EXEC ('SELECT 1') AS USER = 'u' + 'v'")]
	[InlineData("EXEC ('SELECT 1') AS CALLER")]
	[InlineData("EXEC ('SELECT 1') AS OWNER")]
	[InlineData("EXEC ('SELECT 1') AS LOGIN")]
	[InlineData("EXEC ('SELECT 1') AT srv AS LOGIN = 'x'")]
	[InlineData("EXEC ('SELECT 1') WITH RESULT SETS NONE AS USER = 'u'")]
	[InlineData("EXEC p AS USER = 'u'")]
	[InlineData("EXEC @s AS USER = 'u'")]
	[InlineData("EXEC ('SELECT 1') AT a.b")]
	[InlineData("EXEC ('SELECT 1') AT DATA_SOURCE a.b")]
	public void A_string_run_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside it.</summary>
	[Theory]
	[InlineData("EXECUTE ('SELECT 1') AS LOGIN = 'x'")]
	[InlineData("EXEC ('SELECT 1') AS login = 'x'")]
	[InlineData("EXEC ('SELECT 1') AS USER = N'u'")]
	[InlineData("EXEC ('SELECT 1') AS LOGIN = 'x' AT srv")]
	[InlineData("EXEC ('SELECT ?', 1) AT [srv]")]
	[InlineData("EXEC ('SELECT 1') AT DATA_SOURCE ds")]
	[InlineData("EXEC ('SELECT ?', 1) AT DATA_SOURCE [ds]")]
	[InlineData("EXEC ('SELECT 1') AS USER = 'u' AT DATA_SOURCE ds")]
	[InlineData("EXEC ('SELECT 1') AS USER = 'u' WITH RESULT SETS NONE")]
	[InlineData("EXEC (@s) AS USER = 'u'")]
	[InlineData("INSERT t EXEC ('SELECT 1') AS USER = 'u'")]
	public void A_string_run_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>
	/// A query with an order and a shape of its own, inside brackets and in an `INSERT`, as the
	/// engine answers it.
	/// </summary>
	[Theory]
	[InlineData("(SELECT TOP 1 a FROM t ORDER BY a) UNION SELECT 1")]
	[InlineData("SELECT 1 UNION (SELECT TOP 1 a FROM t ORDER BY a)")]
	[InlineData("SELECT (SELECT a FROM t ORDER BY a OPTION (MAXDOP 1))")]
	public void An_ordered_query_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside it.</summary>
	[Theory]
	[InlineData("SELECT (SELECT TOP 1 a FROM t ORDER BY a)")]
	[InlineData("SELECT (SELECT a FROM t ORDER BY a OFFSET 1 ROWS)")]
	[InlineData("SELECT (SELECT a FROM t ORDER BY a FOR XML PATH)")]
	[InlineData("SELECT * FROM u WHERE a IN (SELECT TOP 1 a FROM t ORDER BY a) AND EXISTS (SELECT TOP 1 a FROM t ORDER BY a)")]
	[InlineData("SELECT * FROM (SELECT TOP 1 a FROM t ORDER BY a) AS x")]
	[InlineData("SELECT * FROM (SELECT * FROM t FOR XML AUTO) AS x(c)")]
	[InlineData("SELECT * FROM (SELECT a FROM t ORDER BY a FOR JSON AUTO) AS x(j)")]
	[InlineData("SELECT * FROM u CROSS APPLY (SELECT * FROM t ORDER BY 1 OFFSET 1 ROWS) AS x")]
	[InlineData("WITH c AS (SELECT TOP 1 a FROM t ORDER BY a) SELECT * FROM c")]
	[InlineData("INSERT INTO x SELECT TOP 1 y FROM x ORDER BY y")]
	[InlineData("INSERT INTO x SELECT y FROM x ORDER BY y OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY")]
	[InlineData("INSERT INTO x SELECT 1 UNION SELECT 2 ORDER BY 1")]
	[InlineData("SELECT TOP (SELECT TOP 1 1 FROM t ORDER BY 1) a FROM t")]
	[InlineData("SELECT TOP (SELECT 1 FROM t FOR XML PATH) a FROM t")]
	public void An_ordered_query_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The order is the whole query's, and the tree keeps it where it was written.</summary>
	[Fact]
	public void An_ordered_query_keeps_its_order()
	{
		var select = Assert.IsType<Statement.Select>(
			TransactSql.TryParseStatement("SELECT * FROM (SELECT TOP 1 a FROM t UNION SELECT b FROM u ORDER BY 1) AS x").Value);
		var outer  = Assert.IsType<Query.Specification>(select.Of);
		var derived = Assert.IsType<TableReference.Derived>(Assert.Single(outer.From));
		var ordered = Assert.IsType<Query.Ordered>(derived.Query);

		Assert.IsType<Query.Union>(ordered.Query);
		Assert.NotNull(ordered.By);
		Assert.Null(ordered.For);

		var insert = Assert.IsType<Statement.Insert>(TransactSql.TryParseStatement("INSERT INTO x SELECT y FROM z").Value);

		Assert.IsType<Query.Specification>(insert.Rows);
	}

	/// <summary>A subquery as the count of a `TOP`, whose brackets are the subquery's own.</summary>
	[Theory]
	[InlineData("SELECT TOP (WITH x AS (SELECT 1 AS n) SELECT n FROM x) a FROM t")]
	[InlineData("SELECT TOP (VALUES (1)) a FROM t")]
	public void A_subquery_counted_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside it.</summary>
	[Theory]
	[InlineData("DELETE TOP (SELECT * FROM t2) PERCENT t1")]
	[InlineData("INSERT TOP (SELECT * FROM t2) @v1 DEFAULT VALUES")]
	[InlineData("UPDATE TOP (SELECT * FROM t2) t1 SET c1 = 23 + 10")]
	[InlineData("SELECT TOP ((SELECT * FROM t1) EXCEPT (SELECT c1, c2 FROM t2)) WITH TIES c1 FROM t1 ORDER BY c1")]
	[InlineData("SELECT TOP (SELECT 1 UNION SELECT 2) PERCENT a FROM t")]
	[InlineData("SELECT TOP ((SELECT 1)) a, TOP_ = 1 FROM t")]
	[InlineData("SELECT TOP (1 + (SELECT 1)) a FROM t")]
	[InlineData("SELECT TOP ((SELECT 1) UNION (SELECT 2)) a FROM t")]
	public void A_subquery_counted_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>A bare `*` as an item of the list, as the engine answers it.</summary>
	[Theory]
	[InlineData("SELECT * AS x FROM t")]
	[InlineData("SELECT (*) FROM t")]
	[InlineData("SELECT @a = *, 1 FROM t")]
	[InlineData("SELECT x = * FROM t")]
	public void A_star_in_the_list_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside it.</summary>
	[Theory]
	[InlineData("SELECT 1, *, 2 FROM t")]
	[InlineData("SELECT *, *, * FROM t")]
	[InlineData("SELECT *, t.*, a, * FROM t")]
	[InlineData("SELECT COUNT(*), *, @a + 10 FROM t")]
	[InlineData("SELECT *, 1")]
	[InlineData("SELECT * FROM t UNION SELECT 1, * FROM u")]
	public void A_star_in_the_list_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>`IDENTITY(…)` in a `SELECT … INTO`, as the engine answers it with its variable declared.</summary>
	[Theory]
	[InlineData("SELECT IDENTITY(INT, @a, 1) AS id INTO #t FROM t")]
	[InlineData("SELECT IDENTITY(INT, 1 + 1, 1) AS id INTO #t FROM t")]
	[InlineData("SELECT IDENTITY(INT, (1), 1) AS id INTO #t FROM t")]
	[InlineData("SELECT IDENTITY(INT, 0x1, 1) AS id INTO #t FROM t")]
	[InlineData("SELECT IDENTITY(INT, 1) AS id INTO #t FROM t")]
	[InlineData("SELECT IDENTITY(1, 1) AS id INTO #t FROM t")]
	[InlineData("SELECT IDENTITY(INT, 1, 1) INTO #t FROM t")]
	[InlineData("SELECT IDENTITY(INT, 1, 1) + 1 AS id INTO #t FROM t")]
	[InlineData("SELECT IDENTITY(INT, 1, 1) AS id FROM t")]
	[InlineData("INSERT INTO t SELECT IDENTITY(INT, 1, 1) AS id FROM u")]
	public void An_identity_column_made_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside it.</summary>
	[Theory]
	[InlineData("SELECT IDENTITY(INT) AS id INTO #t FROM t")]
	[InlineData("SELECT IDENTITY(INT, 1, 1) AS id INTO #t FROM t")]
	[InlineData("SELECT IDENTITY(DECIMAL(10, 0), 1, 1) AS id INTO #t FROM t")]
	[InlineData("SELECT IDENTITY(INT, -1, 2) AS id INTO #t FROM t")]
	[InlineData("SELECT IDENTITY(INT, 1.5, +1) id INTO #t FROM t")]
	[InlineData("SELECT a, IDENTITY(INT, 1, 1) AS id, IDENTITY(INT, 1, 1) AS id2 INTO #t FROM t")]
	[InlineData("SELECT IDENTITY(INT, 1, 1) AS id INTO #t")]
	[InlineData("SELECT identity(int, 1, 1) AS 'id' INTO #t FROM t WHERE 1 = 1 ORDER BY a")]
	[InlineData("SELECT id = IDENTITY(INT, 1, 1), 'id2' = IDENTITY(INT, 1, 1) INTO #t FROM t")]
	public void An_identity_column_made_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>
	/// Reserved words as values: `LEFT` and `RIGHT` called, `IDENTITYCOL` and `ROWGUIDCOL` as
	/// columns, and a name of one part called only bare, as the engine answers them.
	/// </summary>
	[Theory]
	[InlineData("SELECT dbo.LEFT(1)")]
	[InlineData("SELECT LEFT FROM t")]
	[InlineData("SELECT a AS LEFT FROM t")]
	[InlineData("SELECT LEFT('a', 1) OVER ()")]
	[InlineData("SELECT [LEFT]('a', 1)")]
	[InlineData("SELECT [LEN]('a')")]
	[InlineData(@"SELECT ""LEN""('a')")]
	[InlineData("SELECT a AS IDENTITYCOL FROM t")]
	[InlineData("UPDATE t SET IDENTITYCOL = 1")]
	[InlineData("SELECT IDENTITYCOL() FROM t")]
	public void Reserved_values_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("SELECT LEFT('Team System', 4), RIGHT('Team System', 6)")]
	[InlineData("SELECT left ('a', 1)")]
	[InlineData("SELECT LEFT('a')")]
	[InlineData("SELECT [LEFT]")]
	[InlineData("SELECT foo(1), dbo.[LEN]('a'), [dbo].[f](1)")]
	[InlineData("SELECT IDENTITYCOL, t.IDENTITYCOL, a.b.ROWGUIDCOL, a.b.c.IDENTITYCOL FROM t")]
	[InlineData("SELECT * FROM t WHERE IDENTITYCOL > 10 AND ROWGUIDCOL IS NOT NULL ORDER BY IDENTITYCOL")]
	[InlineData("SELECT IDENTITYCOL AS x, (IDENTITYCOL * 10), IDENTITYCOL + 1 FROM t")]
	[InlineData("CREATE INDEX ind1 ON t1(c1) WHERE IDENTITYCOL > 10")]
	[InlineData("SELECT a FROM t ORDER BY a + 1")]
	[InlineData("SELECT a FROM t ORDER BY LEN(a), 1, t.ROWGUIDCOL DESC")]
	[InlineData("SELECT a FROM t ORDER BY (SELECT 1)")]
	[InlineData("SELECT a FROM t ORDER BY CASE WHEN a = 1 THEN 0 END")]
	[InlineData("SELECT a FROM t ORDER BY a COLLATE Latin1_General_CI_AS DESC")]
	[InlineData("SELECT ROW_NUMBER() OVER (ORDER BY a + 1) FROM t")]
	public void Reserved_values_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>
	/// A variable assigned, in its own `SET` and in an `UPDATE`, as the engine answers it with
	/// the variable declared: `DEFAULT` is a column's, and a column between the variable and
	/// its value an `UPDATE`'s.
	/// </summary>
	[Theory]
	[InlineData("SET @a = DEFAULT")]
	[InlineData("SET @a = c = 1")]
	[InlineData("SET @a = c += 1")]
	[InlineData("UPDATE t SET @a = DEFAULT")]
	[InlineData("UPDATE t SET @a = c = @a = 1")]
	[InlineData("UPDATE t SET @a = @a = 1")]
	[InlineData("UPDATE t SET c = @a = 1")]
	public void A_variable_assigned_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("SET @a += 1")]
	[InlineData("SET @a = NULL")]
	[InlineData("UPDATE t SET @a = c = DEFAULT")]
	[InlineData("UPDATE t SET @a = a.b = DEFAULT")]
	[InlineData("UPDATE t SET @a = c = 1")]
	[InlineData("UPDATE t SET @a = c += NULL")]
	[InlineData("UPDATE t SET @a = c, c = DEFAULT, @a += 1")]
	public void A_variable_assigned_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>A table-valued function as what a statement writes to, as the engine answers it.</summary>
	[Theory]
	[InlineData("UPDATE dbo.f() WITH (NOLOCK) SET c = 1")]
	[InlineData("UPDATE dbo.f() AS x SET c = 1")]
	[InlineData("UPDATE dbo.f() x SET c = 1")]
	[InlineData("DELETE dbo.f() WITH (NOLOCK)")]
	[InlineData("DELETE dbo.f() AS x")]
	[InlineData("INSERT INTO dbo.f() WITH (TABLOCK) VALUES (1)")]
	[InlineData("UPDATE @t.f() SET c = 1")]
	public void A_function_target_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside it.</summary>
	[Theory]
	[InlineData("UPDATE dbo.f() SET c = 1")]
	[InlineData("UPDATE dbo.tvf(-1, 2, DEFAULT) SET c1 = 2")]
	[InlineData("UPDATE f() SET c = 1")]
	[InlineData("UPDATE TOP (1) dbo.f() SET c = 1 OUTPUT c INTO @t (c1) FROM t WHERE 1 = 1")]
	[InlineData("DELETE dbo.f()")]
	[InlineData("DELETE FROM dbo.f(1) WHERE c = 1")]
	[InlineData("INSERT dbo.f() SELECT * FROM t2 UNION SELECT * FROM t3")]
	[InlineData("INSERT dbo.f() (c1) DEFAULT VALUES")]
	[InlineData("INSERT INTO dbo.tvf(1, -1, DEFAULT) VALUES (2, 3, 4)")]
	[InlineData("INSERT dbo.f() EXEC p")]
	[InlineData("MERGE dbo.f() AS t USING s ON 1 = 1 WHEN MATCHED THEN DELETE;")]
	public void A_function_target_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>`INSERT t (c1)` is a table and its columns, and not a call.</summary>
	[Fact]
	public void A_column_list_is_not_a_call()
	{
		var insert = Assert.IsType<Statement.Insert>(TransactSql.TryParseStatement("INSERT t (c1) VALUES (1)").Value);

		Assert.IsType<TableReference.Named>(insert.Target);
		Assert.Equal(new[] { "c1" }, insert.Columns);

		var called = Assert.IsType<Statement.Insert>(TransactSql.TryParseStatement("INSERT dbo.f() (c1) DEFAULT VALUES").Value);

		Assert.IsType<TableReference.FunctionCall>(called.Target);
		Assert.Equal(new[] { "c1" }, called.Columns);
	}

	/// <summary>
	/// A space is what the engine takes for one: every control character below the space,
	/// Unicode's spaces and separators, and the zero-width space — in the standard's rules
	/// as much as in T-SQL's, `WHERE` being the standard's.
	/// </summary>
	[Theory]
	[InlineData("SELECT\u202F1 AS x")]
	[InlineData("SELECT\u200B1 AS x")]
	[InlineData("SELECT\u30001 AS x")]
	[InlineData("SELECT\u00011 AS x")]
	[InlineData("SELECT\u00A01 AS x")]
	[InlineData("SELECT a FROM t\u2028WHERE a\u202F=\u202F1")]
	[InlineData("SELECT GREATEST\u202F(\u202F'6.62', 3.1415, N'7'\u202F)\u202FAS\u202FGreatestVal")]
	public void A_space_is_what_the_engine_takes_for_one(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>And not what it does not.</summary>
	[Theory]
	[InlineData("SELECT\u200C1 AS x")]
	[InlineData("SELECT\uFEFF1 AS x")]
	[InlineData("SELECT\u007F1 AS x")]
	[InlineData("SELECT\u00AD1 AS x")]
	[InlineData("SELECT\u180E1 AS x")]
	public void Nothing_else_is_a_space(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>
	/// The operators group as the engine computes them — asked with numbers, since a parse
	/// says nothing of grouping: `&amp;` as weak as `+` and read left to right with it, `%` as
	/// strong as `*`, `~` a sign, and a comparison weaker than all of them.
	/// </summary>
	[Fact]
	public void The_operators_group_as_the_engine_computes()
	{
		var and = Assert.IsType<Expression.BitwiseAnd>(TransactSql.TryParseValueExpression("2 + 5 & 4").Value);
		Assert.IsType<Expression.Add>(and.Left);

		var add = Assert.IsType<Expression.Add>(TransactSql.TryParseValueExpression("5 & 4 + 2").Value);
		Assert.IsType<Expression.BitwiseAnd>(add.Left);

		var times = Assert.IsType<Expression.Multiply>(TransactSql.TryParseValueExpression("7 % 3 * 2").Value);
		Assert.IsType<Expression.Modulo>(times.Left);

		var inverted = Assert.IsType<Expression.Multiply>(TransactSql.TryParseValueExpression("~2 * 3").Value);
		Assert.IsType<Expression.BitwiseNot>(inverted.Left);

		var masked = Assert.IsType<Expression.BitwiseAnd>(TransactSql.TryParseValueExpression("6 & 3 * 2").Value);
		Assert.IsType<Expression.Multiply>(masked.Right);

		var compared = Assert.IsType<Expression.Comparison>(TransactSql.TryParseSearchCondition("1 & 3 = 1").Value);
		Assert.IsType<Expression.BitwiseAnd>(compared.Left);

		Assert.Equal(
			SqlComparison.NotEqualBang,
			Assert.IsType<Expression.Comparison>(TransactSql.TryParseSearchCondition("a ! = 1").Value).Operator);
	}

	/// <summary>T-SQL's operators, as the engine answers them.</summary>
	[Theory]
	[InlineData("SELECT a FROM t WHERE a !<> 1")]
	[InlineData("SELECT a FROM t WHERE a !! 1")]
	[InlineData("SELECT a FROM t WHERE a !<= 1")]
	[InlineData("SELECT a FROM t WHERE a *= b")]
	[InlineData("SELECT a FROM t WHERE a =* b")]
	public void The_operators_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("SELECT ~1")]
	[InlineData("SELECT ~ ~1")]
	[InlineData("SELECT -~1")]
	[InlineData("SELECT ~-1")]
	[InlineData("SELECT a & ~b FROM t")]
	[InlineData("SELECT a | b ^ b & a FROM t")]
	[InlineData("SELECT a % b FROM t")]
	[InlineData("SELECT ~(a + b) FROM t")]
	[InlineData("SELECT a FROM t WHERE ~a = 1")]
	[InlineData("SELECT a FROM t WHERE a !< 1")]
	[InlineData("SELECT a FROM t WHERE a !> 1")]
	[InlineData("SELECT a FROM t WHERE a != 1")]
	[InlineData("SELECT a FROM t WHERE a ! < 1")]
	[InlineData("SELECT a FROM t WHERE a ! = 1")]
	[InlineData("SELECT a FROM t WHERE a < > 1")]
	[InlineData("SELECT a FROM t WHERE a > = 1")]
	[InlineData("SELECT a FROM t WHERE a !< ALL (SELECT b FROM u)")]
	[InlineData("CREATE TABLE t (A1 INT CHECK (A1 !< 4))")]
	[InlineData("CREATE TABLE t (A1 INT DEFAULT +-++~+23)")]
	[InlineData("SELECT a AT TIME ZONE ~ b FROM T")]
	public void The_operators_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>`AT TIME ZONE` and `WAITFOR`, as the engine answers them with its variables declared.</summary>
	[Theory]
	[InlineData("SELECT a COLLATE Latin1_General_CI_AS COLLATE Latin1_General_CI_AS FROM T")]
	[InlineData("SELECT a AT TIME ZONE NOT b FROM T")]
	[InlineData("WAITFOR TIME 1")]
	[InlineData("WAITFOR TIME (@t)")]
	[InlineData("WAITFOR TIME '10:00' + ''")]
	[InlineData("WAITFOR DELAY 1")]
	[InlineData("WAITFOR DELAY (@t)")]
	[InlineData("WAITFOR TIME")]
	public void Zones_and_waits_refuse_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And read the forms beside them.</summary>
	[Theory]
	[InlineData("SELECT a AT TIME ZONE b AT TIME ZONE c FROM T")]
	[InlineData("SELECT @v AT TIME ZONE @z AT TIME ZONE @z AT TIME ZONE @z")]
	[InlineData("SELECT * FROM T WHERE a AT TIME ZONE b AT TIME ZONE c < @v")]
	[InlineData("SELECT a COLLATE Latin1_General_CI_AS AT TIME ZONE b FROM T")]
	[InlineData("SELECT a COLLATE Latin1_General_CI_AS AT TIME ZONE b COLLATE Latin1_General_CI_AS FROM T")]
	[InlineData("SELECT a AT TIME ZONE b COLLATE Latin1_General_CI_AS AT TIME ZONE c FROM T")]
	[InlineData("SELECT a + b COLLATE Latin1_General_CI_AS AT TIME ZONE c FROM T")]
	[InlineData("SELECT a AT TIME ZONE - b AT TIME ZONE c FROM T")]
	[InlineData("SELECT a AT TIME ZONE - - b FROM T")]
	[InlineData("SELECT a AT TIME ZONE + b FROM T")]
	[InlineData("SELECT a AT TIME ZONE b::c FROM T")]
	[InlineData("WAITFOR TIME '10:00'")]
	[InlineData("waitfor time 'time'")]
	[InlineData("WAITFOR TIME N'10:00'")]
	[InlineData("WAITFOR TIME @t")]
	[InlineData("WAITFOR DELAY N'00:00:00'")]
	public void Zones_and_waits_read_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Table, query and join hints, as the engine answers them.</summary>
	[Theory]
	[InlineData("SELECT * FROM t WITH (FOO)")]
	[InlineData("SELECT * FROM t WITH (FASTFIRSTROW)")]
	[InlineData("SELECT * FROM t WITH (FORCESEEK (i))")]
	[InlineData("SELECT * FROM t WITH (SPATIAL_WINDOW_MAX_CELLS = 1.5)")]
	[InlineData("SELECT * FROM t WITH (NOLOCK, , READPAST)")]
	[InlineData("SELECT * FROM dbo.f() WITH (NOLOCK)")]
	[InlineData("SELECT 1 OPTION (FOO)")]
	[InlineData("SELECT 1 OPTION (HASH ORDER)")]
	[InlineData("SELECT 1 OPTION (PARAMETERIZATION SIMPLE)")]
	[InlineData("SELECT 1 OPTION (MAXDOP = 2)")]
	[InlineData("SELECT 1 OPTION (FAST 1.5)")]
	[InlineData("SELECT 1 OPTION (LABEL = x)")]
	[InlineData("SELECT 1 OPTION (FORCE EXTERNALPUSHDOWN)")]
	[InlineData("SELECT 1 OPTION (TABLE HINT (t NOLOCK))")]
	[InlineData("SELECT * FROM a CROSS LOOP JOIN b")]
	[InlineData("INSERT t EXEC p OPTION (RECOMPILE)")]
	[InlineData("SELECT * FROM t1 OPTION (OPTIMIZE CORRELATED UNION ALL)")]
	[InlineData("SELECT * FROM t1 OPTION (BYPASS OPTIMIZER_QUEUE)")]
	public void The_hint_catalogue_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("SELECT * FROM t WITH (NOLOCK INDEX(i))")]
	[InlineData("SELECT * FROM t WITH (INDEX (i) INDEX (j), FORCESEEK (i (a, b)), SPATIAL_WINDOW_MAX_CELLS = 512)")]
	[InlineData("SELECT * FROM t WITH (INDEX = [i], NOEXPAND, KEEPIDENTITY)")]
	[InlineData("SELECT * FROM t WITH (XLOCK ROWLOCK)")]
	[InlineData("SELECT 1 OPTION (MAX_GRANT_PERCENT = 10.5, MAXDOP 2, FORCE SCALEOUTEXECUTION, LABEL = N'x')")]
	[InlineData("SELECT 1 OPTION (OPTIMIZE FOR (@a UNKNOWN, @b = 1, @c), USE HINT ('A', N'B'), USE PLAN N'<p/>')")]
	[InlineData("SELECT * FROM t OPTION (TABLE HINT (dbo.t, NOLOCK INDEX(i), FORCESEEK))")]
	[InlineData("SELECT * FROM a FULL OUTER HASH JOIN b ON 1 = 1 OPTION (LOOP JOIN, CONCAT UNION, HASH GROUP)")]
	[InlineData("UPDATE t WITH (IGNORE_TRIGGERS) SET a = 1 OPTION (RECOMPILE)")]
	[InlineData("INSERT t (a) SELECT 1 FROM u OPTION (MAXDOP 1)")]
	[InlineData("INSERT t VALUES (1) OPTION (RECOMPILE)")]
	[InlineData("INSERT t DEFAULT VALUES OPTION (RECOMPILE)")]
	[InlineData("SELECT * FROM t1 OPTION (CHECKCONSTRAINTS PLAN, USEPLAN 2, SHRINKDB PLAN, ALTERCOLUMN PLAN, KEEP UNION)")]
	public void The_hint_catalogue_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>What a database is created with, and the files it is made of, as the engine answers it.</summary>
	[Theory]
	[InlineData("CREATE DATABASE d CONTAINMENT = FOO")]
	[InlineData("CREATE DATABASE d WITH TRUSTWORTHY = ON")]
	[InlineData("CREATE DATABASE d WITH NESTED_TRIGGERS ON")]
	[InlineData("CREATE DATABASE d WITH DEFAULT_LANGUAGE = 'English'")]
	[InlineData("CREATE DATABASE d WITH TWO_DIGIT_YEAR_CUTOFF = 2000.5")]
	[InlineData("CREATE DATABASE d WITH FILESTREAM (NON_TRANSACTED_ACCESS = ON)")]
	[InlineData("CREATE DATABASE d WITH PERSISTENT_LOG_BUFFER = ON")]
	[InlineData("CREATE DATABASE d WITH LEDGER ON")]
	[InlineData("CREATE DATABASE d WITH RESTRICTED_USER")]
	[InlineData("CREATE DATABASE d WITH AUTO_CLOSE ON")]
	[InlineData("CREATE DATABASE d WITH FOO = 1")]
	[InlineData("CREATE DATABASE d ON (NAME = f, FILENAME = 'f.mdf') FOR ATTACH WITH FILESTREAM (NON_TRANSACTED_ACCESS = FULL)")]
	[InlineData("CREATE DATABASE d ON (NAME = f, FILENAME = 'f.mdf', SIZE = 10 PB)")]
	[InlineData("CREATE DATABASE d ON (NAME = f, FILENAME = 'f.mdf', SIZE = 10.5 MB)")]
	[InlineData("CREATE DATABASE d ON (NAME = f, FILENAME = 'f.mdf', SIZE = 10 %)")]
	[InlineData("CREATE DATABASE d ON (NAME = f, FILENAME = 'f.mdf', FOO = 1)")]
	[InlineData("CREATE DATABASE d ON FILEGROUP g (NAME = g1, FILENAME = 'g1.ndf')")]
	[InlineData("CREATE DATABASE d ON (NAME = f, FILENAME = 'f.mdf'), FILEGROUP g CONTAINS FOO (NAME = g1, FILENAME = 'g1.ndf')")]
	[InlineData("ALTER DATABASE d ADD FILE (NAME = f, FILENAME = 'f.ndf', SIZE = 1 MB, FOO = 1)")]
	public void The_creation_catalogue_refuses_what_the_engine_does(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>And reads the forms beside them.</summary>
	[Theory]
	[InlineData("CREATE DATABASE d CONTAINMENT = PARTIAL WITH NESTED_TRIGGERS = ON, TRUSTWORTHY ON, " +
		"TRANSFORM_NOISE_WORDS=OFF, DEFAULT_LANGUAGE=[french], DEFAULT_FULLTEXT_LANGUAGE=1033, TWO_DIGIT_YEAR_CUTOFF=2000")]
	[InlineData("CREATE DATABASE d WITH FILESTREAM (DIRECTORY_NAME = NULL, NON_TRANSACTED_ACCESS = OFF), LEDGER = ON")]
	[InlineData("CREATE DATABASE d WITH PERSISTENT_LOG_BUFFER = ON (DIRECTORY_NAME = 'x'), CATALOG_COLLATION = DATABASE_DEFAULT")]
	[InlineData("CREATE DATABASE d ON (NAME = f, FILENAME = 'f.mdf') FOR ATTACH WITH ENABLE_BROKER, RESTRICTED_USER, TRUSTWORTHY ON")]
	[InlineData("CREATE DATABASE d ON (NAME = f, FILENAME = 'f.mdf') FOR ATTACH WITH FILESTREAM (DIRECTORY_NAME = NULL)")]
	[InlineData("CREATE DATABASE s ON (NAME = f, FILENAME = 'f.ss') AS SNAPSHOT OF d WITH TRUSTWORTHY ON")]
	[InlineData("CREATE DATABASE d LOG ON (NAME = l, FILENAME = 'l.ldf')")]
	[InlineData("CREATE DATABASE d ON PRIMARY (NAME = f, FILENAME = 'f.mdf', SIZE = 10 MB, MAXSIZE = UNLIMITED, FILEGROWTH = 10 %), " +
		"FILEGROUP g CONTAINS FILESTREAM DEFAULT (NAME = fs, FILENAME = 'c:\\fs') LOG ON (NAME = l, FILENAME = 'l.ldf')")]
	[InlineData("CREATE DATABASE d ON (NAME = [f], FILENAME = N'f.mdf', OFFLINE, NEWNAME = 'g', FILEGROWTH = 10%)")]
	[InlineData("ALTER DATABASE d MODIFY FILE (NAME = f, SIZE = 10 MB)")]
	[InlineData("ALTER DATABASE d ADD FILEGROUP g CONTAINS MEMORY_OPTIMIZED_DATA")]
	public void The_creation_catalogue_reads_what_the_engine_does(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>
	/// A creation's options end where its list does, so the statement after it is a
	/// statement: `--split` found this cut wrong in the corpus.
	/// </summary>
	[Fact]
	public void A_creation_does_not_take_the_next_statement_for_an_option() =>
		Assert.Equal(2, TransactSql.ParseSql(
			"CREATE DATABASE d ON (NAME = f, FILENAME = 'f.mdf') FOR ATTACH WITH RESTRICTED_USER\n" +
			"ALTER DATABASE d SET HADR SUSPEND").Length);

	/// <summary>
	/// A script, cut at its <c>GO</c> lines the way ScriptDom cuts it: the number of batches
	/// and of statements in them, and the line that ended the first.
	/// </summary>
	[Theory]
	[InlineData("SELECT 1\nGO\nSELECT 2", 2, 2, "GO")]
	[InlineData("SELECT 1\r\nGO\r\nSELECT 2", 2, 2, "GO")]
	[InlineData("SELECT 1 GO SELECT 2", 1, 2, null)]
	[InlineData("SELECT 1\n  go  -- c\nSELECT 2", 2, 2, "GO")]
	[InlineData("SELECT 1\nGO;\nSELECT 2", 2, 2, "GO")]
	[InlineData("SELECT 1\nGO /* c */\nSELECT 2", 2, 2, "GO")]
	[InlineData("SELECT 1 -- x\nGO\nSELECT 2", 2, 2, "GO")]
	[InlineData("SELECT 1\n/*\nGO\n*/\nSELECT 2", 1, 2, null)]
	[InlineData("SELECT 'a\nGO\nb'", 1, 1, null)]
	[InlineData("SELECT go FROM t", 1, 1, null)]
	[InlineData("SELECT 1\nGOTO x", 1, 2, null)]
	[InlineData("SELECT 1\nGO\n", 1, 1, "GO")]
	[InlineData("CREATE TABLE t (a INT)\nGO", 1, 1, "GO")]
	[InlineData("SELECT 1;\ngo 2", 1, 1, "GO 2")]
	[InlineData("SELECT 1\nGO\nGO\nSELECT 2", 3, 2, "GO")]
	[InlineData("CREATE PROCEDURE p AS SELECT 1\nGO\nEXEC p", 2, 2, "GO")]
	[InlineData("SELECT 1\nGO 5\nSELECT 2", 2, 2, "GO 5")]
	[InlineData("GO\nSELECT 1", 2, 1, "GO")]
	[InlineData("SELECT 1\nGO\nGO", 2, 1, "GO")]
	[InlineData("p 1\nGO", 1, 1, "GO")]
	public void A_script_is_cut_where_ScriptDom_cuts_it(string input, int batches, int statements, string? first)
	{
		var match = TransactSql.TryParseScript(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
		Assert.Equal(batches, match.Value.Length);
		Assert.Equal(statements, match.Value.Sum(static batch => batch.Statements.Length));
		Assert.Equal(first, match.Value[0].Go);
	}

	/// <summary>
	/// A <c>GO</c> first on a line ends a batch wherever it stands, as ScriptDom has it, so a
	/// statement it falls in the middle of is cut; and a text of statements is one batch,
	/// with no <c>GO</c> in it.
	/// </summary>
	[Theory]
	[InlineData("SELECT a,\ngo\nFROM t")]
	[InlineData("SELECT 1\nGO\nFROM T")]
	public void A_GO_line_ends_a_batch_wherever_it_stands(string input) =>
		Assert.False(TransactSql.TryParseScript(input).IsSuccess, input);

	[Fact]
	public void A_text_of_statements_has_no_GO_in_it() =>
		Assert.False(TransactSql.TryParseSql("SELECT 1\nGO\nSELECT 2").IsSuccess);

	/// <summary>A text of several statements, as one call to the server carries them.</summary>
	[Theory]
	[InlineData("SELECT 1 SELECT 2", 2)]
	[InlineData("SELECT 1; SELECT 2;", 2)]
	[InlineData(";;SELECT 1;; ;", 1)]
	[InlineData("WITH a AS (SELECT 1 AS x) SELECT * FROM a; SELECT * FROM a", 2)]
	[InlineData("SELECT 1; WITH a AS (SELECT 1 AS x) SELECT * FROM a", 2)]
	[InlineData("ALTER DATABASE d SET ONLINE SELECT 1", 2)]
	[InlineData("", 0)]
	public void A_text_is_read_as_its_statements(string input, int count)
	{
		var match = TransactSql.TryParseSql(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
		Assert.Equal(count, match.Value.Length);
	}

	/// <summary>
	/// A `WITH` that is not first needs the statement before it ended, in a text and in a
	/// block alike; empty statements are read in both.
	/// </summary>
	[Theory]
	[InlineData("SELECT 1 WITH a AS (SELECT 1 AS x) SELECT * FROM a", false)]
	[InlineData("PRINT 1 WITH XMLNAMESPACES ('u' AS ns) SELECT 1", false)]
	[InlineData("WITH a AS (SELECT 1 AS x) SELECT * FROM a WITH b AS (SELECT 2 AS y) SELECT * FROM b", false)]
	[InlineData("BEGIN SELECT 1 WITH a AS (SELECT 1 AS x) SELECT * FROM a END", false)]
	[InlineData("BEGIN SELECT 1; WITH a AS (SELECT 1 AS x) SELECT * FROM a END", true)]
	[InlineData("BEGIN ; SELECT 1 ; ; END", true)]
	[InlineData("BEGIN ; END", false)]
	[InlineData("IF 1 = 1 SELECT 1; WITH a AS (SELECT 1 AS x) SELECT * FROM a", true)]
	[InlineData("IF 1 = 1 SELECT 1 WITH a AS (SELECT 1 AS x) SELECT * FROM a", false)]
	[InlineData("IF 1 = 1 SELECT 1; ELSE SELECT 2; WITH a AS (SELECT 1 AS x) SELECT * FROM a", true)]
	[InlineData("WHILE 1 = 0 BREAK; WITH a AS (SELECT 1 AS x) SELECT * FROM a", true)]
	public void A_WITH_after_a_statement_needs_it_ended(string input, bool read) =>
		Assert.Equal(read, TransactSql.TryParseSql(input).IsSuccess);

	/// <summary>A construct taken out of the language: read up to a level and refused after it.</summary>
	/// <remarks>
	/// The weak algorithms, as the engine answers them: everything but AES and the longer RSA
	/// keys up to 120, and RC4 at 100 alone.
	/// </remarks>
	[Theory]
	[InlineData("CREATE SYMMETRIC KEY k WITH ALGORITHM = TRIPLE_DES ENCRYPTION BY CERTIFICATE c", 120)]
	[InlineData("CREATE SYMMETRIC KEY k WITH ALGORITHM = DESX ENCRYPTION BY PASSWORD = 'p'",      120)]
	[InlineData("CREATE SYMMETRIC KEY k WITH ALGORITHM = RC4_128 ENCRYPTION BY CERTIFICATE c",    100)]
	[InlineData("CREATE ASYMMETRIC KEY k WITH ALGORITHM = RSA_1024",                              120)]
	[InlineData("CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = TRIPLE_DES_3KEY ENCRYPTION BY SERVER ASYMMETRIC KEY k", 120)]
	[InlineData("ALTER DATABASE ENCRYPTION KEY REGENERATE WITH ALGORITHM = TRIPLE_DES_3KEY",      120)]
	public void Each_level_reads_until_where_the_engine_stops(string input, int until)
	{
		foreach (var level in new[] { 100, 110, 120, 130, 140, 150, 160, 170 })
		{
			var match = level switch
			{
				100 => TransactSql.TryParseStatement100(input),
				110 => TransactSql.TryParseStatement110(input),
				120 => TransactSql.TryParseStatement120(input),
				130 => TransactSql.TryParseStatement130(input),
				140 => TransactSql.TryParseStatement140(input),
				150 => TransactSql.TryParseStatement150(input),
				160 => TransactSql.TryParseStatement160(input),
				_   => TransactSql.TryParseStatement170(input),
			};

			Assert.True(match.IsSuccess == level <= until, $"{input}  at {level}");
		}

		Assert.True(TransactSql.TryParseStatement(input).IsSuccess, input);
	}

	/// <summary>A parser per compatibility level, each reading from where the engine does.</summary>
	/// <remarks>
	/// The levels are readings of one grammar sharing one machine, and the conditions that
	/// tell them apart are the engine's own map (`--levels`): each row here is a construct
	/// the engine refuses below the level named and reads from it on. The union, which names
	/// no level, reads every one — and `OPENXML`'s schema, the same rule as `OPENJSON`'s, is
	/// here to hold the one that is not gated to where it was.
	/// </remarks>
	[Theory]
	[InlineData("SELECT STRING_AGG (c, ',') WITHIN GROUP (ORDER BY c) FROM t", 110)]
	[InlineData("SELECT * FROM OPENJSON (N'[]') WITH (a INT)",                  130)]
	[InlineData("SELECT 1 FROM t WINDOW w AS (ORDER BY c)",                     160)]
	[InlineData("SELECT TRIM (LEADING 'x' FROM 'xa')",                          160)]
	[InlineData("SELECT * FROM SEMANTICKEYPHRASETABLE (t1, *) AS x",            110)]
	[InlineData("SELECT * FROM SEMANTICKEYPHRASETABLE (t1, (c1, c2)) AS x",     110)]
	[InlineData("SELECT * FROM OPENXML (@h, '/r', 1) WITH (a INT)",             100)]
	[InlineData("SELECT * FROM SEMANTICKEYPHRASETABLE (t1, c1) AS x",           100)]
	[InlineData("SELECT * FROM CONTAINSTABLE (t1, *, 'x') AS x",                100)]
	public void Each_level_reads_from_where_the_engine_does(string input, int from)
	{
		foreach (var level in new[] { 100, 110, 120, 130, 140, 150, 160, 170 })
		{
			var match = level switch
			{
				100 => TransactSql.TryParseStatement100(input),
				110 => TransactSql.TryParseStatement110(input),
				120 => TransactSql.TryParseStatement120(input),
				130 => TransactSql.TryParseStatement130(input),
				140 => TransactSql.TryParseStatement140(input),
				150 => TransactSql.TryParseStatement150(input),
				160 => TransactSql.TryParseStatement160(input),
				_   => TransactSql.TryParseStatement170(input),
			};

			Assert.True(match.IsSuccess == level >= from, $"{input}  at {level}");
		}

		Assert.True(TransactSql.TryParseStatement(input).IsSuccess, input);
	}

	// ── The temporal and grouping clauses ───────────────────────────────────────

	/// <summary>What the published `FROM` and `GROUP BY` say and the examples did not.</summary>
	[Theory]
	[InlineData("SELECT * FROM T FOR SYSTEM_TIME AS OF '01/02/03'")]
	[InlineData("SELECT * FROM T FOR SYSTEM_TIME FROM '2013-01-01' TO '2014-01-01'")]
	[InlineData("SELECT * FROM T FOR SYSTEM_TIME BETWEEN '2013-01-01' AND '2014-01-01'")]
	[InlineData("SELECT * FROM T FOR SYSTEM_TIME CONTAINED IN ('2013-01-01', '2014-01-01')")]
	[InlineData("SELECT * FROM T FOR SYSTEM_TIME ALL WHERE ManagerID = 5")]
	[InlineData("SELECT * FROM T FOR SYSTEM_TIME AS OF @at AS d WHERE d.a = 1")]
	[InlineData("SELECT c1 FROM t1 GROUP BY () WITH CUBE")]
	[InlineData("SELECT c1 FROM t1 GROUP BY c1 WITH ROLLUP")]
	[InlineData("SELECT c1 FROM t1 GROUP BY c1, c2 WITH CUBE")]
	[InlineData("SELECT c1 FROM t1 GROUP BY ROLLUP ((c1, c2), c3)")]
	[InlineData("SELECT c1 FROM t1 GROUP BY GROUPING SETS ((CUBE (c1), ROLLUP (c1), c1), (c1), ())")]
	[InlineData("SELECT c1 FROM t1 GROUP BY c1 WITH (DISTRIBUTED_AGG)")]
	public void The_published_clauses_read(string input)
	{
		var match = TransactSql.TryParseSelect(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The JSON constructors, a named query, and three smaller things.</summary>
	/// <remarks>
	/// All written from the published syntax. `JSON_OBJECT` is the one place in T-SQL where
	/// a colon joins two values instead of introducing a parameter, and it carries two
	/// clauses an argument list has not — what a null does, and what type comes back.
	/// </remarks>
	[Theory]
	[InlineData("SELECT JSON_OBJECT ()")]
	[InlineData("SELECT JSON_OBJECT ('name':'value', 'type':1)")]
	[InlineData("SELECT JSON_OBJECT ('name':'value', 'type':NULL ABSENT ON NULL)")]
	[InlineData("SELECT JSON_OBJECT ('a':1 RETURNING json)")]
	[InlineData("SELECT JSON_OBJECT ('name':JSON_ARRAY (1, 2))")]
	[InlineData("SELECT JSON_OBJECT ('name':JSON_OBJECT ('id':1))")]
	[InlineData("SELECT JSON_ARRAY (1, 2 NULL ON NULL)")]
	[InlineData("SELECT JSON_ARRAYAGG (name ABSENT ON NULL) OVER (PARTITION BY dept) FROM t")]
	[InlineData("SELECT JSON_VALUE ('c', '$' RETURNING INT)")]

	// A query given a name in front of the statement that uses it.
	[InlineData("WITH cte AS (SELECT * FROM t) SELECT * FROM cte")]
	[InlineData("WITH cte (a, b) AS (SELECT x, y FROM t) SELECT a FROM cte")]
	[InlineData("WITH a AS (SELECT 1 AS x), b AS (SELECT 2 AS y) SELECT * FROM a, b")]
	[InlineData("WITH r (n) AS (SELECT 1 UNION ALL SELECT n + 1 FROM r) SELECT n FROM r OPTION (MAXRECURSION 2)")]
	[InlineData("WITH c AS (SELECT TOP 1 a FROM t ORDER BY a) SELECT * FROM c")]

	// And three smaller ones the work list named.
	[InlineData("SELECT * FROM t1 OPTION (OPTIMIZE FOR (@v1 = 20, @v2 = NULL))")]
	[InlineData("SELECT SUM (c1) OVER (Win1 ORDER BY c1) FROM t1 WINDOW Win1 AS (PARTITION BY c1)")]
	[InlineData("SELECT SUM (c) OVER w2 FROM t WINDOW w1 AS (PARTITION BY c), w2 AS (w1)")]
	[InlineData("SELECT c1 INTO myDb..t2 FROM t1")]
	[InlineData("SELECT * FROM myDb..t1")]
	[InlineData("SELECT t.* FROM myDb..t1 AS t")]
	public void The_json_and_named_query_layer_reads(string input)
	{
		var match = TransactSql.TryParseSelect(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The smaller shapes the work list named, one line of grammar each.</summary>
	[Theory]
	[InlineData("SELECT CASE a WHEN NULL THEN 1 ELSE 2 END FROM t")]
	[InlineData("SELECT CASE tz WHEN NULL THEN d AT TIME ZONE 'UTC' ELSE d END FROM t")]
	[InlineData("SELECT * FROM .[MyDb].dbo.t1")]
	[InlineData("SELECT [db].$PARTITION.f1 (@a) FROM t")]
	[InlineData("SELECT $PARTITION.f1 (12 + 1) FROM t")]
	[InlineData("SELECT * FROM master..sysprocesses AS p CROSS APPLY ::fn_get_sql (p.sql_handle) AS s")]
	[InlineData("WITH XMLNAMESPACES (DEFAULT 'u') SELECT c1 FROM t1")]
	[InlineData("WITH XMLNAMESPACES ('u' AS ns) SELECT c1 FROM t1 FOR XML AUTO")]
	[InlineData("SELECT a FROM t WHERE a IS DISTINCT FROM ANY (SELECT b FROM u)")]
	[InlineData("SELECT a FROM t WHERE a IS NOT DISTINCT FROM ALL (SELECT b FROM u)")]
	public void The_smaller_shapes_read(string input)
	{
		var match = TransactSql.TryParseSelect(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The graph pattern, which is a drawing rather than an expression.</summary>
	/// <remarks>
	/// Every one of these is from the published `MATCH` syntax or its examples. The arrows
	/// are two characters read as two: a `&lt;-` lexeme would be produced everywhere, and
	/// `a &lt;-1` would stop being a comparison with a negative number.
	/// </remarks>
	[Theory]
	[InlineData("SELECT p2.name FROM Person p1, friend, Person p2 WHERE MATCH (p1-(friend)->p2)")]
	[InlineData("SELECT p2.name FROM Person p1, friend, Person p2 WHERE MATCH (p2<-(friend)-p1)")]
	[InlineData("SELECT a FROM P p1, f f1, P p2, f f2, P p0 WHERE MATCH (p1-(f1)->p0<-(f2)-p2)")]
	[InlineData("SELECT a FROM P p1, f f1, P p2, f f2, P p0 WHERE MATCH (p1-(f1)->p0 AND p2-(f2)->p0)")]
	[InlineData("SELECT a FROM P p1, f, P p2, P p3 WHERE MATCH (p1-(f)->p2-(f)->p3)")]
	[InlineData("SELECT a FROM P p1, f FOR PATH AS fo, P FOR PATH AS p2 WHERE MATCH (SHORTEST_PATH (p1(-(fo)->p2){1,3}))")]
	[InlineData("SELECT a FROM P p1, f FOR PATH AS fo, P FOR PATH AS p2 WHERE MATCH (SHORTEST_PATH (p1(-(fo)->p2)+))")]
	[InlineData("SELECT a FROM N n1, E e, N n2 WHERE MATCH (LAST_NODE (n1)-(e)->n2)")]
	[InlineData("SELECT a FROM N n1, N n2 WHERE MATCH (LAST_NODE (n1) = LAST_NODE (n2))")]
	[InlineData("SELECT a FROM P p1, f, P p2 WHERE MATCH (p1-(f)->p2) AND p1.name = 'Alice'")]
	[InlineData("SELECT STRING_AGG (p2.name, '->') WITHIN GROUP (GRAPH PATH) FROM P p2")]
	[InlineData("SELECT $node_id FROM Person")]
	[InlineData("SELECT p.$edge_id, p.$from_id, p.$to_id FROM friend AS p")]

	// And the comparison the arrows must not have taken away.
	[InlineData("SELECT a FROM t WHERE a < -1")]
	[InlineData("SELECT a FROM t WHERE a <-1")]
	public void The_graph_pattern_reads(string input)
	{
		var match = TransactSql.TryParseSelect(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The rowset functions, and the escapes ODBC left behind.</summary>
	/// <remarks>
	/// A dozen small languages rather than one: `OPENJSON` takes a schema, `OPENROWSET` a
	/// file and a list of options, `CHANGETABLE` a word and then a table, and the full-text
	/// four a column list where a value would stand. What they share is the shape around
	/// them, so it is written once and the option names are left to the catalogue — the
	/// same trade the table hints make, and for the same reason.
	/// </remarks>
	[Theory]

	// The schema a rowset function may declare, which `OPENJSON` and `OPENXML` share.
	[InlineData("SELECT c1 FROM OPENJSON (@j) WITH (c1 INT '$.a.b[2]')")]
	[InlineData("SELECT * FROM OPENJSON (@j, 'lax $.location') WITH (street VARCHAR (500), lon INT '$.geo.longitude') AS location")]
	[InlineData("SELECT * FROM OPENJSON (@j) WITH ([Order] NVARCHAR (MAX) AS JSON)")]
	[InlineData("SELECT * FROM OPENJSON (@var)")]
	[InlineData("SELECT * FROM OPENXML (@idoc, '/ROOT/Customer', 1) WITH (CustomerID VARCHAR (10) '@id')")]
	[InlineData("SELECT * FROM OPENXML (@idoc, '/root', 1) WITH Customers")]

	// A bulk source, its options, and the older spelling that joins a provider by semicolons.
	[InlineData("SELECT * FROM OPENROWSET (BULK 'df1', SINGLE_NCLOB) AS a")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'df1', SINGLE_NCLOB, ORDER (c1 ASC)) AS a")]
	[InlineData("SELECT * FROM OPENROWSET (BULK 'f', FORMATFILE = 'x', FIRSTROW = 2, ORDER (c1 ASC, c2 DESC) UNIQUE) AS a")]
	[InlineData("SELECT a.* FROM OPENROWSET ('SQLOLEDB', N'seattle1'; 'manager'; 'MyPass', 'SELECT 1') AS a")]
	[InlineData("SELECT * FROM OPENQUERY (OracleSvr, 'SELECT name FROM joe.titles') AS a")]

	// The tracking and full-text four, which take a word and then a table.
	[InlineData("SELECT * FROM CHANGETABLE (CHANGES t1, 10) AS a")]
	[InlineData("SELECT * FROM CHANGETABLE (VERSION s1.d1.dbo.t1, (c1), (1)) AS a")]
	[InlineData("SELECT * FROM CHANGETABLE (VERSION z..t1, (c1, c2), ('a', 'b')) AS a (z1, z2)")]
	[InlineData("SELECT * FROM CONTAINSTABLE (t1, PROPERTY (c1, 'p'), 'foo', LANGUAGE 1033, 5) AS k")]
	[InlineData("SELECT * FROM SEMANTICKEYPHRASETABLE (db1.s1.t1, (c1, c2, c3), -10) AS k")]
	[InlineData("SELECT * FROM SEMANTICKEYPHRASETABLE (t1, *) AS t_alias")]
	[InlineData("SELECT * FROM STRING_SPLIT (NULL, N',')")]

	// What a model is asked for, and the escapes every driver has understood since before
	// this language had a standard.
	[InlineData("SELECT AI_GENERATE_EMBEDDINGS ('text' USE MODEL MyDefaultModel)")]
	[InlineData("SELECT { FN convert (@a, sql_int) }, { FN database () }")]
	[InlineData("SELECT { d '2020-01-01' }, { ts '2020-01-01 00:00:00' }")]
	[InlineData("SELECT * FROM { oj t LEFT OUTER JOIN u ON t.a = u.a }")]
	[InlineData("SELECT @a ||= 1")]
	[InlineData("SELECT dbo.f (DEFAULT, 1)")]
	public void The_rowset_functions_read(string input)
	{
		var match = TransactSql.TryParseSelect(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	// ── The statements that change rows ─────────────────────────────────────────

	/// <summary>The four statements that write, read through the one entry point.</summary>
	/// <remarks>
	/// The query level was the whole of this grammar until now, and these are written on top
	/// of it rather than beside it: an `INSERT` takes a query, an `UPDATE` takes a `FROM`
	/// clause, a `MERGE` takes a table source and two search conditions. Almost nothing is
	/// new — what is new is the frame around what was already read.
	/// </remarks>
	[Theory]
	[InlineData("INSERT INTO t VALUES (1, 2), (DEFAULT, 0), (NULL, NULL)")]
	[InlineData("INSERT t (a, b) SELECT x, y FROM u")]
	[InlineData("INSERT INTO t DEFAULT VALUES")]
	[InlineData("INSERT TOP (10) INTO t (a) SELECT x FROM u")]
	[InlineData("INSERT INTO t (a) OUTPUT INSERTED.a INTO @v VALUES (1)")]
	[InlineData("INSERT INTO t WITH (TABLOCK) (a) VALUES (1)")]

	[InlineData("UPDATE t SET a = 1")]
	[InlineData("UPDATE t SET a = 1, b = DEFAULT, c = NULL WHERE d > 0")]
	[InlineData("UPDATE TOP (10) t SET a = a * 1.25, b = GETDATE ()")]
	[InlineData("UPDATE t SET a += 1, b -= 2, c ||= 'x'")]
	[InlineData("UPDATE t SET @v = a = 1")]
	[InlineData("UPDATE t SET @v += 1")]
	[InlineData("UPDATE t SET s.DocumentSummary.WRITE (N'features', 28, 10)")]
	[InlineData("UPDATE t SET a = 1 OUTPUT DELETED.a, INSERTED.a INTO @v FROM t AS x JOIN u ON x.id = u.id")]
	[InlineData("UPDATE t SET a = 1 WHERE CURRENT OF GLOBAL c1")]
	[InlineData("UPDATE @rows SET a = 1")]
	[InlineData("WITH c AS (SELECT * FROM t) UPDATE c SET a = 1")]

	[InlineData("DELETE FROM t")]
	[InlineData("DELETE t WHERE a > 1")]
	[InlineData("DELETE TOP (20) FROM t WHERE d < '20020701'")]
	[InlineData("DELETE t OUTPUT DELETED.* WHERE a = 1")]
	[InlineData("DELETE x FROM t AS x INNER JOIN u ON x.id = u.id WHERE u.a > 1")]
	[InlineData("DELETE t WHERE CURRENT OF c1")]
	[InlineData("DELETE OPENQUERY (srv, 'SELECT a FROM t')")]

	[InlineData("MERGE t AS d USING u AS s ON d.id = s.id WHEN MATCHED THEN UPDATE SET d.a = s.a")]
	[InlineData("MERGE INTO t USING u ON t.id = u.id WHEN MATCHED THEN DELETE")]
	[InlineData("MERGE t USING u ON t.id = u.id WHEN NOT MATCHED THEN INSERT (a) VALUES (1)")]
	[InlineData("MERGE t USING u ON t.id = u.id WHEN NOT MATCHED BY TARGET THEN INSERT DEFAULT VALUES")]
	[InlineData("MERGE t USING u ON t.id = u.id WHEN NOT MATCHED BY SOURCE THEN DELETE")]
	[InlineData("MERGE t USING (SELECT a, b FROM v) AS s (a, b) ON t.a = s.a WHEN MATCHED AND t.q <= 0 THEN DELETE WHEN MATCHED THEN UPDATE SET t.q = s.b")]
	[InlineData("MERGE TOP (5) t USING u ON t.id = u.id WHEN MATCHED THEN DELETE OUTPUT $ACTION, DELETED.a")]
	public void The_statements_that_write_read(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>And a statement holds a query, and a query holds a statement.</summary>
	/// <remarks>
	/// Which is why they are one ADT and not two: `INSERT … SELECT` one way, and a merge
	/// standing where a derived table does the other.
	/// </remarks>
	[Fact]
	public void A_statement_and_a_query_hold_each_other()
	{
		var written = Assert.IsType<Statement.Insert>(
			TransactSql.TryParseStatement("INSERT INTO t (a) SELECT b FROM u WHERE b > 1").Value);

		Assert.Equal(new[] { "a" }, written.Columns);

		var query = Assert.IsType<Query.Specification>(written.Rows);

		Assert.Equal("u", Assert.IsType<TableReference.Named>(Assert.Single(query.From)).Table);
		Assert.NotNull(query.Where);

		var merged = Assert.IsType<Statement.Merge>(
			TransactSql.TryParseStatement(
				"MERGE t USING u ON t.id = u.id WHEN MATCHED THEN UPDATE SET a = 1").Value);

		var arm = Assert.IsType<Clause.MergeWhen>(Assert.Single(merged.Whens));

		Assert.True(arm.OnMatch);
		Assert.Null(arm.Condition);

		var change = Assert.IsType<Statement.Update>(arm.Action);

		Assert.Equal("a", Assert.IsType<Clause.Set>(Assert.Single(change.Set)).Target);
	}

	/// <summary>And the query entry point still reads only queries.</summary>
	[Theory]
	[InlineData("INSERT INTO t VALUES (1)")]
	[InlineData("UPDATE t SET a = 1")]
	[InlineData("DELETE FROM t")]
	[InlineData("MERGE t USING u ON t.id = u.id WHEN MATCHED THEN DELETE")]
	public void What_writes_is_not_a_query(string input)
	{
		Assert.False(TransactSql.TryParseSelect   (input).IsSuccess, input);
		Assert.True (TransactSql.TryParseStatement(input).IsSuccess, input);
	}

	/// <summary>The procedural level: the frame everything else stands inside.</summary>
	/// <remarks>
	/// Written before the rest of the statement language and not after, because
	/// `CREATE PROCEDURE`, `CREATE TRIGGER` and `CREATE FUNCTION` are a header and a body and
	/// the body is this — so none of the three can be read at all until it exists, and
	/// `INSERT … EXEC` cannot either.
	/// </remarks>
	[Theory]
	[InlineData("BEGIN SELECT 1 END")]
	[InlineData("BEGIN SELECT 1; SELECT 2; END")]
	[InlineData("IF @a > 1 SELECT 1")]
	[InlineData("IF @a > 1 BEGIN SELECT 1 END ELSE BEGIN SELECT 2 END")]
	[InlineData("WHILE @a > 1 BEGIN SET @a = @a - 1; IF @a = 5 BREAK; ELSE CONTINUE; END")]
	[InlineData("BEGIN TRY SELECT 1 END TRY BEGIN CATCH THROW END CATCH")]

	[InlineData("DECLARE @a INT")]
	[InlineData("DECLARE @a AS REAL = 1.2")]
	[InlineData("DECLARE @a INT, @b NVARCHAR (50) = N'x', @c AS TABLE (id INT NOT NULL, n VARCHAR (30))")]
	[InlineData("DECLARE @t TABLE (a INT IDENTITY (1, 5) NOT NULL, b AS a * 2, c MONEY DEFAULT 0)")]
	[InlineData("DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT a FROM t")]
	[InlineData("DECLARE @c AS CURSOR")]

	[InlineData("SET @a = 1")]
	[InlineData("SET @a += 1")]
	[InlineData("SET @a = (SELECT MAX (b) FROM t)")]
	[InlineData("SET ANSI_NULLS ON")]
	[InlineData("SET ANSI_NULLS, QUOTED_IDENTIFIER OFF")]
	[InlineData("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED")]
	[InlineData("SET IDENTITY_INSERT dbo.t ON")]
	[InlineData("SET ROWCOUNT 100")]
	[InlineData("SET TEXTSIZE 2048")]

	[InlineData("EXEC dbo.p")]
	[InlineData("EXECUTE dbo.p @a = 1, @b = DEFAULT, @c OUTPUT")]
	[InlineData("EXEC @rc = dbo.p 1, 2")]
	[InlineData("EXEC (N'SELECT 1')")]
	[InlineData("EXEC dbo.p WITH RECOMPILE")]
	[InlineData("EXEC dbo.p AT linked")]
	[InlineData("INSERT INTO t (a) EXEC dbo.p")]

	[InlineData("BEGIN TRANSACTION")]
	[InlineData("BEGIN TRAN t1 WITH MARK N'x'")]
	[InlineData("BEGIN DISTRIBUTED TRANSACTION")]
	[InlineData("COMMIT")]
	[InlineData("COMMIT TRANSACTION t1")]
	[InlineData("ROLLBACK TRAN @v")]
	[InlineData("SAVE TRANSACTION s1")]

	[InlineData("PRINT 'Number of rows is ' + CAST (@@ROWCOUNT AS CHAR (3))")]
	[InlineData("RETURN")]
	[InlineData("RETURN 1")]
	[InlineData("GOTO label1")]
	[InlineData("THROW 51000, 'x', 1")]
	[InlineData("RAISERROR ('x', 16, 1) WITH NOWAIT")]
	[InlineData("WAITFOR DELAY '00:00:02'")]
	[InlineData("USE master")]
	[InlineData("CHECKPOINT")]

	[InlineData("BULK INSERT t FROM 'f.dat'")]
	[InlineData("BULK INSERT dbo.t FROM 'f.dat' WITH (FIELDTERMINATOR = ',', FIRSTROW = 2, TABLOCK)")]
	public void The_procedural_level_reads(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Tables declared and tables changed, which share their column definitions.</summary>
	/// <remarks>
	/// The largest single kind in the corpus and the one that pulls the most behind it: a
	/// column definition is what `ALTER TABLE … ADD` adds and what `ALTER TABLE … ALTER
	/// COLUMN` changes, and the table body a variable declares is the same thing smaller.
	/// The option lists are written as a shape with an open vocabulary, as the table hints
	/// are and for the same reason.
	/// </remarks>
	[Theory]
	[InlineData("CREATE TABLE t (a INT)")]
	[InlineData("CREATE TABLE dbo.t (a INT NOT NULL, b NVARCHAR (50) NULL)")]
	[InlineData("CREATE TABLE t (a INT IDENTITY (1, 5) NOT NULL, b AS a * 2 PERSISTED)")]
	[InlineData("CREATE TABLE t (a INT SPARSE, b NCHAR (10) COLLATE Albanian_BIN, c VARBINARY (MAX) FILESTREAM)")]
	[InlineData("CREATE TABLE t (a INT CONSTRAINT pk PRIMARY KEY CLUSTERED)")]
	[InlineData("CREATE TABLE t (a INT, CONSTRAINT pk PRIMARY KEY (a ASC, b DESC) WITH (FILLFACTOR = 80) ON [PRIMARY])")]
	[InlineData("CREATE TABLE t (a INT, b INT, FOREIGN KEY (a) REFERENCES u (id) ON DELETE CASCADE ON UPDATE NO ACTION)")]
	[InlineData("CREATE TABLE t (a INT REFERENCES u (id) NOT FOR REPLICATION)")]
	[InlineData("CREATE TABLE t (a INT, CHECK (a > 0))")]
	[InlineData("CREATE TABLE t (a INT DEFAULT 0, b INT CONSTRAINT df DEFAULT 1)")]
	[InlineData("CREATE TABLE t (a INT, INDEX ix NONCLUSTERED (a) WHERE a > 0 WITH (DATA_COMPRESSION = PAGE))")]
	[InlineData("CREATE TABLE t (a INT, INDEX ix CLUSTERED COLUMNSTORE ORDER (a))")]
	[InlineData("CREATE TABLE t (a INT, s DATETIME2 GENERATED ALWAYS AS ROW START, e DATETIME2 GENERATED ALWAYS AS ROW END, PERIOD FOR SYSTEM_TIME (s, e)) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.h))")]
	[InlineData("CREATE TABLE t (a INT) ON ps (a) TEXTIMAGE_ON [PRIMARY]")]
	[InlineData("CREATE TABLE t (a XML COLUMN_SET FOR ALL_SPARSE_COLUMNS)")]
	[InlineData("CREATE TABLE n1 (c1 INT) AS NODE")]
	[InlineData("CREATE TABLE e01 (c1 INT, CONSTRAINT cnst CONNECTION (N1 TO N2)) AS EDGE")]
	[InlineData("CREATE TABLE t AS FILETABLE WITH (FILETABLE_DIRECTORY = 'd')")]
	[InlineData("CREATE TABLE t (c1 INT, INDEX idx NONCLUSTERED ($NODE_ID))")]
	[InlineData("CREATE TABLE t (i INT NOT NULL INDEX ix NONCLUSTERED HASH WITH (BUCKET_COUNT = 16))")]
	[InlineData("ALTER TABLE t REBUILD WITH (ONLINE = ON (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1440 MINUTES, ABORT_AFTER_WAIT = NONE)))")]
	[InlineData("SELECT c1 FROM t1 GROUP BY c1 WITH (DISTRIBUTED_AGG), c2")]
	[InlineData("CREATE TABLE t (a INT) WITH (DATA_COMPRESSION = PAGE ON PARTITIONS (1 TO 4, 6))")]

	[InlineData("ALTER TABLE t ADD c5 VARBINARY (MAX) FILESTREAM")]
	[InlineData("ALTER TABLE t ADD CONSTRAINT pk PRIMARY KEY (a)")]
	[InlineData("ALTER TABLE t WITH NOCHECK ADD CHECK (a > 0)")]
	[InlineData("ALTER TABLE t ALTER COLUMN c1 INT NOT NULL")]
	[InlineData("ALTER TABLE t ALTER COLUMN c1 ADD SPARSE")]
	[InlineData("ALTER TABLE t DROP COLUMN a, CONSTRAINT c")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT IF EXISTS c")]
	[InlineData("ALTER TABLE t NOCHECK CONSTRAINT ALL")]
	[InlineData("ALTER TABLE t ENABLE TRIGGER tr1, tr2")]
	[InlineData("ALTER TABLE t SET (SYSTEM_VERSIONING = OFF)")]
	[InlineData("ALTER TABLE t REBUILD PARTITION = ALL WITH (DATA_COMPRESSION = ROW)")]
	[InlineData("ALTER TABLE t SWITCH PARTITION 1 TO u PARTITION 2")]

	// And the table a variable declares, which is the same body read the same way.
	[InlineData("DECLARE @t TABLE (a INT PRIMARY KEY, b AS a * 2, CHECK (a > 0))")]
	public void The_table_statements_read(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>And a table says what is in it.</summary>
	[Fact]
	public void A_created_table_says_its_columns()
	{
		var made = Assert.IsType<Statement.TableDefinition>(
			TransactSql.TryParseStatement(
				"CREATE TABLE dbo.t (a INT NOT NULL, b AS a * 2, CONSTRAINT pk PRIMARY KEY (a))").Value);

		Assert.Equal("dbo.t", made.Name);
		Assert.Equal(3, made.Elements.Length);

		var first = Assert.IsType<Clause.ColumnDefinition>(made.Elements[0]);

		Assert.Equal("a", first.Name);
		Assert.Equal("INT", first.Type);
		Assert.Null(first.Computed);

		var second = Assert.IsType<Clause.ColumnDefinition>(made.Elements[1]);

		Assert.Equal("b", second.Name);
		Assert.Null(second.Type);
		Assert.NotNull(second.Computed);

		var third = Assert.IsType<Clause.ConstraintDefinition>(made.Elements[2]);

		Assert.Equal("pk", third.Name);
		Assert.Equal("PRIMARY KEY", third.Kind);

		var column = Assert.IsType<Clause.SortSpecification>(Assert.Single(third.Columns));

		Assert.Equal("a", Assert.IsType<Expression.ColumnReference>(column.Value).Text);
		Assert.Equal(SqlOrder.Unspecified, column.Order);
	}

	/// <summary>The four that are a header and a body.</summary>
	/// <remarks>
	/// None of them could be read at all until the procedural level existed, because each is
	/// a header and then whatever T-SQL somebody put inside it — 341 statements of the corpus
	/// between them, and the body was the part already done.
	/// </remarks>
	[Theory]
	[InlineData("CREATE PROCEDURE p AS SELECT 1")]
	[InlineData("CREATE PROC dbo.p @a INT, @b NVARCHAR (50) = N'x' OUTPUT AS BEGIN SELECT @a END")]
	[InlineData("CREATE PROCEDURE p (@a INT = 0 READONLY) WITH RECOMPILE, ENCRYPTION AS SELECT 1")]
	[InlineData("CREATE OR ALTER PROCEDURE p AS SELECT 1")]
	[InlineData("ALTER PROCEDURE p AS SELECT 1")]
	[InlineData("CREATE PROCEDURE p WITH EXECUTE AS OWNER AS SELECT 1")]
	[InlineData("CREATE PROCEDURE p AS EXTERNAL NAME asm.cls.method")]
	[InlineData("CREATE PROCEDURE p AS BEGIN INSERT INTO t1 VALUES (1, 2), (DEFAULT, 0); RETURN 0 END")]

	[InlineData("CREATE FUNCTION f (@a INT) RETURNS INT AS BEGIN RETURN @a * 2 END")]
	[InlineData("CREATE FUNCTION f () RETURNS TABLE AS RETURN (SELECT a FROM t)")]
	[InlineData("CREATE FUNCTION f (@a INT) RETURNS TABLE RETURN SELECT a FROM t WHERE b = @a")]
	[InlineData("CREATE FUNCTION f () RETURNS @r TABLE (a INT NOT NULL) AS BEGIN INSERT INTO @r VALUES (1); RETURN END")]
	[InlineData("CREATE FUNCTION f (@a INT) RETURNS INT WITH SCHEMABINDING, RETURNS NULL ON NULL INPUT AS BEGIN RETURN 1 END")]

	[InlineData("CREATE TRIGGER tr ON Sales.Customer AFTER INSERT, UPDATE AS RAISERROR ('x', 16, 10)")]
	[InlineData("CREATE TRIGGER tr ON t INSTEAD OF DELETE AS SELECT 1")]
	[InlineData("CREATE TRIGGER tr ON t FOR INSERT NOT FOR REPLICATION AS SELECT 1")]
	[InlineData("CREATE TRIGGER safety ON DATABASE FOR DROP_SYNONYM AS RAISERROR ('x', 10, 1)")]
	[InlineData("CREATE TRIGGER tr ON ALL SERVER FOR CREATE_DATABASE AS PRINT 'made'")]
	[InlineData("CREATE TRIGGER tr ON ALL SERVER WITH EXECUTE AS 'login_test' FOR LOGON AS BEGIN ROLLBACK END")]

	[InlineData("CREATE VIEW v AS SELECT a FROM t")]
	[InlineData("CREATE VIEW dbo.v (a, b) WITH SCHEMABINDING AS SELECT x, y FROM t WITH CHECK OPTION")]
	[InlineData("CREATE OR ALTER VIEW v AS SELECT a FROM t UNION SELECT b FROM u")]
	public void The_routines_read(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Indexes and permissions, which share almost everything with what came before.</summary>
	/// <remarks>
	/// An index inside a `CREATE TABLE` is the same thing said in the same words, so only the
	/// header is new. And a permission's *name* is made of words T-SQL reserves — `ALTER ANY
	/// DATABASE`, `VIEW SERVER STATE`, `BACKUP LOG` — which an identifier cannot be, so they
	/// are named the way the option names are.
	/// </remarks>
	[Theory]
	[InlineData("CREATE INDEX ix ON t (a)")]
	[InlineData("CREATE UNIQUE CLUSTERED INDEX ix ON dbo.t (a ASC, b DESC) INCLUDE (c) WHERE a > 0")]
	[InlineData("CREATE INDEX ix ON t (a) WITH (PAD_INDEX = ON, FILLFACTOR = 80, ONLINE = ON) ON ps (a)")]
	[InlineData("CREATE INDEX ix ON t (a) WITH (DATA_COMPRESSION = PAGE ON PARTITIONS (1 TO 4))")]
	[InlineData("CREATE CLUSTERED COLUMNSTORE INDEX cix ON t")]
	[InlineData("CREATE NONCLUSTERED COLUMNSTORE INDEX cix ON t (a, b) WHERE a > 0")]
	[InlineData("ALTER INDEX ALL ON t REBUILD PARTITION = ALL")]
	[InlineData("ALTER INDEX ix ON t REORGANIZE WITH (LOB_COMPACTION = ON)")]
	[InlineData("ALTER INDEX ix ON t DISABLE")]
	[InlineData("ALTER INDEX ix ON t SET (ALLOW_PAGE_LOCKS = OFF)")]
	[InlineData("ALTER INDEX ix ON t RESUME WITH (MAXDOP = 2)")]

	[InlineData("GRANT SELECT ON t TO u")]
	[InlineData("GRANT SELECT (a, b), UPDATE ON dbo.t TO u, v WITH GRANT OPTION")]
	[InlineData("GRANT alter ON SERVER ROLE::serverRole1 TO serverRole2")]
	[InlineData("GRANT EXECUTE ON OBJECT::dbo.p TO PUBLIC AS dbo")]
	[InlineData("GRANT VIEW SERVER STATE TO login_test")]
	[InlineData("GRANT ALTER ANY DATABASE DDL TRIGGER TO u")]
	[InlineData("GRANT ALL PRIVILEGES ON t TO u")]
	[InlineData("DENY VIEW DEFINITION ON SCHEMA::s TO u CASCADE")]
	[InlineData("REVOKE GRANT OPTION FOR SELECT ON t FROM u CASCADE AS dbo")]
	[InlineData("REVOKE CREATE TABLE FROM u")]
	public void The_indexes_and_permissions_read(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The database, which is a header and an option list.</summary>
	/// <remarks>
	/// What `ALTER DATABASE … SET` may set is a catalogue, held to the engine in
	/// <see cref="The_database_catalogue_refuses_what_the_engine_does"/>. A `CREATE DATABASE`'s
	/// settings are still a run of words and then, sometimes, what it is set to, and the syntax
	/// says where the run ends: `ON` and `OFF` are values, and `WITH` begins the next clause.
	/// </remarks>
	[Theory]
	[InlineData("CREATE DATABASE d1")]
	[InlineData("CREATE DATABASE d1 CONTAINMENT = PARTIAL")]
	[InlineData("CREATE DATABASE d1 COLLATE Estonian_CS_AS")]
	[InlineData("CREATE DATABASE d1 ON PRIMARY (NAME = a, FILENAME = 'a.mdf', SIZE = 10 MB, " +
		"MAXSIZE = 1 GB, FILEGROWTH = 10 %) LOG ON (NAME = b, FILENAME = 'b.ldf')")]
	[InlineData("CREATE DATABASE d1 ON (FILENAME = 'a.mdf'), (FILENAME = 'b.ldf') FOR ATTACH WITH ENABLE_BROKER")]
	[InlineData("CREATE DATABASE d1 ON PRIMARY (NAME = a, FILENAME = 'a'), " +
		"FILEGROUP fg CONTAINS FILESTREAM (NAME = b, FILENAME = 'b')")]
	[InlineData("CREATE DATABASE s1 ON (NAME = a, FILENAME = 'a.ss') AS SNAPSHOT OF d1")]

	[InlineData("ALTER DATABASE d1 SET SINGLE_USER")]
	[InlineData("ALTER DATABASE d1 SET SINGLE_USER WITH ROLLBACK IMMEDIATE")]
	[InlineData("ALTER DATABASE d1 SET READ_ONLY WITH ROLLBACK AFTER 10 SECONDS")]
	[InlineData("ALTER DATABASE CURRENT SET COMPATIBILITY_LEVEL = 160")]
	[InlineData("ALTER DATABASE d1 SET ENCRYPTION ON, ENCRYPTION OFF")]
	[InlineData("ALTER DATABASE d1 SET HADR AVAILABILITY GROUP = g1")]
	[InlineData("ALTER DATABASE d1 SET HADR SUSPEND")]
	[InlineData("ALTER DATABASE d1 SET TARGET_RECOVERY_TIME = 42 SECONDS")]
	[InlineData("ALTER DATABASE d1 SET CHANGE_TRACKING (CHANGE_RETENTION = 3 DAYS, AUTO_CLEANUP = OFF)")]
	[InlineData("ALTER DATABASE d1 SET QUERY_STORE = ON (DESIRED_STATE = READ_ONLY, MAX_PLANS_PER_QUERY = 200)")]
	[InlineData("ALTER DATABASE d1 SET QUERY_STORE CLEAR ALL")]
	[InlineData("ALTER DATABASE d1 SET AUTO_CREATE_STATISTICS ON (INCREMENTAL = ON), AUTO_UPDATE_STATISTICS ON")]
	[InlineData("ALTER DATABASE d1 COLLATE Estonian_CS_AS")]
	[InlineData("ALTER DATABASE d1 MODIFY NAME = d2")]
	[InlineData("ALTER DATABASE d1 MODIFY (MAXSIZE = 1 GB, EDITION = 'basic')")]
	[InlineData("ALTER DATABASE d1 MODIFY FILEGROUP fg1 AUTOGROW_ALL_FILES")]
	[InlineData("ALTER DATABASE d1 MODIFY FILEGROUP fg1 READ_ONLY WITH ROLLBACK AFTER 10 SECONDS")]
	[InlineData("ALTER DATABASE d1 ADD FILEGROUP fg1 CONTAINS MEMORY_OPTIMIZED_DATA")]
	[InlineData("ALTER DATABASE d1 ADD FILE (FILENAME = 'a', NAME = b) TO FILEGROUP [MY FILEGROUP]")]
	[InlineData("ALTER DATABASE d1 ADD LOG FILE (FILENAME = 'log'), (FILENAME = 'log2')")]
	[InlineData("ALTER DATABASE d1 REMOVE FILE a")]
	[InlineData("ALTER DATABASE d1 REBUILD LOG")]
	[InlineData("ALTER DATABASE SCOPED COLLATE Estonian_CS_AS")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 1")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY")]
	[InlineData("ALTER DATABASE SCOPED CONFIGURATION CLEAR PROCEDURE_CACHE")]
	public void The_database_reads(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>
	/// And a statement that only begins like a database is read as the statement it is.
	/// </summary>
	/// <remarks>
	/// Each of these is a statement of its own, and the rules for a database must not take
	/// one for a database called `ENCRYPTION` doing something called `KEY`. That is why the
	/// two word-only actions of `ALTER DATABASE` are written out and not read as `word`: a
	/// rule wide enough for `REBUILD LOG` is wide enough for these. They were refused until
	/// the keys were read; now they are read, and by the right rule.
	/// </remarks>
	[Theory]
	[InlineData("ALTER DATABASE ENCRYPTION KEY REGENERATE WITH ALGORITHM = AES_256",
		"AlterDatabaseEncryptionKey")]
	[InlineData("CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = AES_128 ENCRYPTION BY SERVER CERTIFICATE c1",
		"DatabaseEncryptionKeyDefinition")]
	public void And_what_only_begins_like_a_database_is_read_as_itself(string input, string node)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
		Assert.Equal(node, match.Value!.GetType().Name);
	}


	/// <summary>What a second reading of the published syntax found missing.</summary>
	/// <remarks>
	/// Written clause by clause against Microsoft's own blocks rather than from the corpus:
	/// each of these is a line of `<column_definition>`, `<column_constraint>`,
	/// `ALTER TABLE`, `CREATE PROCEDURE` or `<data_type>` that the grammar did not have.
	/// The corpus is what noticed; the syntax is what said what to write.
	/// </remarks>
	[Theory]
	// `<data_type>`: xml takes a schema collection where everything else takes a precision.
	[InlineData("CREATE TABLE t (c1 XML (CONTENT dbo.sc), c2 XML (sc), c3 XML)")]

	// `[ [ CONSTRAINT constraint_name ] { NULL | NOT NULL } ]` — a nullability may be named.
	[InlineData("CREATE TABLE t (c1 INT CONSTRAINT nn NOT NULL)")]

	// `GENERATED ALWAYS AS { ROW | TRANSACTION_ID | SEQUENCE_NUMBER } { START | END } [ HIDDEN ]`,
	// and no other source: ScriptDom's `SUSER_SID` is refused by the engine at every level.
	[InlineData("CREATE TABLE t (a DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL)")]

	// `HIDDEN` after `ADD` and `DROP`; on its own it belongs to a period column and no other.
	[InlineData("ALTER TABLE t ALTER COLUMN c1 ADD HIDDEN")]
	[InlineData("ALTER TABLE t ALTER COLUMN c1 DROP MASKED")]
	[InlineData("ALTER TABLE t ALTER COLUMN c1 ADD ROWGUIDCOL WITH (ONLINE = OFF)")]

	// The published `ALTER TABLE` actions that were not there.
	[InlineData("ALTER TABLE t ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON)")]
	[InlineData("ALTER TABLE t DISABLE FILETABLE_NAMESPACE")]
	[InlineData("ALTER TABLE t ALTER INDEX ix REBUILD WITH (BUCKET_COUNT = 1)")]
	[InlineData("ALTER TABLE t MERGE RANGE (NULL)")]
	[InlineData("ALTER TABLE t SPLIT RANGE (10)")]
	[InlineData("ALTER TABLE t DROP CONSTRAINT c WITH (MOVE TO fg, ONLINE = ON)")]

	// `DECLARE` gives a variable a nullability, which nothing about a variable suggests.
	[InlineData("DECLARE @v AS INT NOT NULL = 4")]
	[InlineData("DECLARE @v AS INT NULL")]

	// `[ NULL | NOT NULL ] [ = default ]`, in that order, from the natively compiled form.
	[InlineData("CREATE PROCEDURE p @p1 INT, @p2 INT NULL = NULL, @p3 INT NOT NULL AS SELECT 1")]
	[InlineData("CREATE PROCEDURE p WITH NATIVE_COMPILATION, SCHEMABINDING AS " +
		"BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N'us_english') SELECT 1 END")]

	// An option's name is a run of words in four places at once, and its value may be a list.
	[InlineData("ALTER DATABASE db SET QUERY_STORE (CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 367))")]
	[InlineData("CREATE TABLE t WITH (CLUSTERED COLUMNSTORE INDEX, DISTRIBUTION = HASH(a)) AS SELECT a FROM u")]

	// The selective XML index, which is the one index whose shape is its own.
	[InlineData("CREATE SELECTIVE XML INDEX sxi ON t (c) FOR (path1 = '/a/b')")]
	[InlineData("CREATE SELECTIVE XML INDEX sxi ON t (c) WITH XMLNAMESPACES ('urn:a' AS ns) " +
		"FOR (p1 = '/a/b' AS XQUERY 'xs:double', p2 = '/a/c' AS XQUERY 'xs:string' MAXLENGTH (200) SINGLETON, " +
		"p3 = '/a/d' AS SQL NVARCHAR (100)) WITH (PAD_INDEX = ON)")]
	[InlineData("ALTER INDEX sxi ON t FOR (REMOVE path1)")]
	[InlineData("ALTER INDEX sxi ON t WITH XMLNAMESPACES ('urn:a' AS ns) FOR (ADD p9 = '/a/e')")]
	[InlineData("CREATE PRIMARY XML INDEX pxi ON t (c)")]
	[InlineData("CREATE XML INDEX xi ON t (c) USING XML INDEX pxi FOR PATH")]

	// A warehouse's table and view, which say the distribution before the query.
	[InlineData("CREATE TABLE dbo.t1 (c1, c2) WITH (DISTRIBUTION = ROUND_ROBIN) AS SELECT a, b FROM u")]
	[InlineData("CREATE MATERIALIZED VIEW v WITH (DISTRIBUTION = HASH(c5)) AS SELECT c5 FROM t")]

	// `<collate clause>` follows a character expression and not only a bare column.
	[InlineData("SELECT a COLLATE Albanian_BIN")]
	[InlineData("SELECT (a) COLLATE Albanian_BIN")]
	[InlineData("SELECT t.a COLLATE Albanian_BIN")]

	// `WITH CHANGE_TRACKING_CONTEXT ( context )` in front of the statement, on its own —
	// and with named queries behind the comma, which the reference does not write out and
	// seven statements across four files of the corpus do. It is `XMLNAMESPACES` beside it
	// in the same clause, and it is spelled the same way.
	[InlineData("WITH CHANGE_TRACKING_CONTEXT (0xff) INSERT INTO t (a) VALUES (1)")]
	[InlineData("WITH CHANGE_TRACKING_CONTEXT (0xff), c (a) AS (SELECT a FROM t) INSERT INTO u (a) SELECT a FROM c")]

	// `SET LANGUAGE us_english` — a setting and one value, which a third of them take.
	[InlineData("SET LANGUAGE us_english")]
	[InlineData("SET DATEFORMAT mdy")]
	[InlineData("COMMIT TRANSACTION WITH (DELAYED_DURABILITY = ON)")]
	public void The_published_syntax_reads(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>And where the engine and ScriptDom disagree, the engine wins above 90.</summary>
	/// <remarks>
	/// ScriptDom reads a cursor variable with a default. The engine answers
	/// `Incorrect syntax`, and the published syntax gives a cursor no default.
	/// <para>
	/// A `CHANGE_TRACKING_CONTEXT` joined to a named query by a comma was here too, on the
	/// same argument, and the corpus is what took it out: seven statements carry the shape
	/// and it is read above with the rest of the clause.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("DECLARE @c AS CURSOR = 'x'")]
	public void And_what_only_ScriptDom_reads_is_refused(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>`NULL` is a constant of this dialect and stands wherever a value does.</summary>
	/// <remarks>
	/// The standard's is a <c>&lt;null specification&gt;</c> and stands in a handful of
	/// named places — a row constructor, a `CAST` operand, a `SET`. Microsoft's expression
	/// reference lists it among the constants, which is a different language: `1 + NULL` is
	/// an expression, `trim('[]' FROM NULL)` is a call, and `DEFAULT NULL` is a constraint
	/// whose value is one. Found by the corpus refusing all three.
	/// </remarks>
	[Theory]
	[InlineData("SELECT NULL")]
	[InlineData("SELECT 1 + NULL")]
	[InlineData("SELECT TRIM('[]' FROM NULL)")]
	[InlineData("SELECT COALESCE(NULL, 1)")]
	[InlineData("INSERT INTO t (a) VALUES (NULL)")]
	[InlineData("CREATE TABLE t (a INT CONSTRAINT d DEFAULT NULL)")]

	// A table-valued function written in the CLR declares its columns where an inline one
	// would say `RETURN`, and `ORDER` says what the assembly promises about them.
	[InlineData("CREATE FUNCTION f () RETURNS TABLE (c1 INT) AS EXTERNAL NAME a.b.c")]
	[InlineData("CREATE FUNCTION f () RETURNS TABLE (c1 INT) ORDER (c1 ASC) AS EXTERNAL NAME a.b.c")]

	// And a table-valued method on a variable of a user-defined type.
	[InlineData("SELECT c1 FROM @v.f(1) AS t (c)")]
	[InlineData("SELECT c1 FROM @v.f() AS t")]
	[InlineData("SELECT c1 FROM @rows")]
	public void And_the_rest_of_the_second_reading(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>A member reached through `::`, and everything that may follow one.</summary>
	/// <remarks>
	/// `SELECT t::a` read and `SELECT t::a + 1` did not, which was not the grammar:
	/// <c>("::" | '.') &amp; Identifier</c> was being taken for a set of two literals and
	/// every call site became that range, with the identifier after it gone. See
	/// <see cref="SemanticTests.A_choice_that_reads_more_than_its_literals_is_not_a_set"/>.
	/// </remarks>
	[Theory]
	[InlineData("SELECT t::a")]
	[InlineData("SELECT t::a + 1")]
	[InlineData("SELECT t::a AS q")]
	[InlineData("SELECT t::f()")]
	[InlineData("SELECT t::a COLLATE Albanian_BIN")]
	[InlineData("SELECT t::a, t2::f(), dbo.[type 1]::[Property]")]
	[InlineData("SELECT (c1).SomeProperty")]
	public void A_member_reads_and_the_value_goes_on(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>`DROP`, which is sixty-six statements and one shape.</summary>
	/// <remarks>
	/// Written from the sixty-six published blocks, which agree about everything but the
	/// words in the middle. Those are written out and here they are the syntax rather than a
	/// catalogue: `DROP TABLE` and `DROP VIEW` are different statements, `DROP MASTER KEY`
	/// names no object at all, and a rule wide enough for any word would read `DROP FOO x`
	/// and call it one of them.
	/// </remarks>
	[Theory]
	[InlineData("DROP TABLE t")]
	[InlineData("DROP TABLE IF EXISTS dbo.t, dbo.u")]
	[InlineData("DROP VIEW IF EXISTS v")]
	[InlineData("DROP PROC p")]
	[InlineData("DROP PROCEDURE IF EXISTS dbo.p, dbo.q")]
	[InlineData("DROP FUNCTION IF EXISTS f")]
	[InlineData("DROP DATABASE IF EXISTS d1, d2")]
	[InlineData("DROP DATABASE AUDIT SPECIFICATION s")]
	[InlineData("DROP DATABASE SCOPED CREDENTIAL c")]
	[InlineData("DROP SERVER AUDIT SPECIFICATION s")]
	[InlineData("DROP SERVER AUDIT a")]
	[InlineData("DROP SERVER ROLE r")]
	[InlineData("DROP XML SCHEMA COLLECTION dbo.sc")]
	[InlineData("DROP SEARCH PROPERTY LIST pl")]
	[InlineData("DROP CRYPTOGRAPHIC PROVIDER p")]
	[InlineData("DROP EXTERNAL DATA SOURCE ds")]
	[InlineData("DROP EXTERNAL FILE FORMAT ff")]
	[InlineData("DROP EXTERNAL RESOURCE POOL rp")]
	[InlineData("DROP WORKLOAD CLASSIFIER wc")]

	// The five that say something after the names.
	[InlineData("DROP ASYMMETRIC KEY k REMOVE PROVIDER KEY")]
	[InlineData("DROP SYMMETRIC KEY k")]
	[InlineData("DROP ASSEMBLY IF EXISTS a1, a2 WITH NO DEPENDENTS")]
	[InlineData("DROP EXTERNAL LIBRARY l AUTHORIZATION dbo")]
	[InlineData("DROP EVENT SESSION s ON SERVER")]
	[InlineData("DROP EVENT NOTIFICATION n1, n2 ON QUEUE dbo.q")]

	// The two that name nothing: there is one of each per database.
	[InlineData("DROP MASTER KEY")]
	[InlineData("DROP DATABASE ENCRYPTION KEY")]

	// An index is named twice, and the older spelling puts both in one qualified name.
	[InlineData("DROP INDEX ix ON dbo.t")]
	[InlineData("DROP INDEX IF EXISTS ix ON t WITH (ONLINE = ON, MOVE TO fg)")]
	[InlineData("DROP INDEX dbo.t.ix")]
	[InlineData("DROP INDEX ix1 ON t1, ix2 ON t2")]
	[InlineData("DROP FULLTEXT INDEX ON dbo.t")]

	// A signature comes off a module and a classification off a column.
	[InlineData("DROP SIGNATURE FROM dbo.p BY CERTIFICATE c")]
	[InlineData("DROP COUNTER SIGNATURE FROM dbo.p BY ASYMMETRIC KEY k, CERTIFICATE c")]
	[InlineData("DROP SENSITIVITY CLASSIFICATION FROM dbo.t.c1, dbo.t.c2")]

	// And a trigger says which of the three kinds it is by what it is on.
	[InlineData("DROP TRIGGER IF EXISTS tr1, tr2")]
	[InlineData("DROP TRIGGER tr ON DATABASE")]
	[InlineData("DROP TRIGGER tr ON ALL SERVER")]
	public void The_drop_family_reads(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>And a word that names no object of this language is not one.</summary>
	[Theory]
	[InlineData("DROP FOO x")]
	[InlineData("DROP TABLE")]
	[InlineData("DROP MASTER KEY k")]
	public void And_a_drop_of_nothing_is_refused(string input) =>
		Assert.False(TransactSql.TryParseStatement(input).IsSuccess, input);

	/// <summary>Who may connect, and as whom.</summary>
	/// <remarks>
	/// Logins, users, roles and schemas: one family because they are one shape — a name,
	/// where it came from, and a list of settings, closed for each statement. A password is
	/// followed by words with no commas between them, `PASSWORD = 'p' OLD_PASSWORD = 'q'`,
	/// which no other option list in T-SQL does.
	/// </remarks>
	[Theory]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 'p'")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 'p' MUST_CHANGE, CHECK_POLICY = ON, SID = 0x01")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 0x0100 HASHED, DEFAULT_DATABASE = master")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 'p' MUST_CHANGE HASHED")]
	[InlineData("CREATE LOGIN l FROM WINDOWS WITH DEFAULT_DATABASE = master")]
	[InlineData("CREATE LOGIN l FROM EXTERNAL PROVIDER")]
	[InlineData("CREATE LOGIN l FROM CERTIFICATE c WITH CREDENTIAL = cr")]
	[InlineData("CREATE LOGIN l FROM ASYMMETRIC KEY k")]
	[InlineData("ALTER LOGIN l ENABLE")]
	[InlineData("ALTER LOGIN l DISABLE")]
	[InlineData("ALTER LOGIN l WITH PASSWORD = 'p' OLD_PASSWORD = 'q'")]
	[InlineData("ALTER LOGIN l WITH NAME = m, NO CREDENTIAL")]
	[InlineData("ALTER LOGIN l ADD CREDENTIAL cr")]
	[InlineData("ALTER LOGIN l DROP CREDENTIAL cr")]

	[InlineData("CREATE USER u")]
	[InlineData("CREATE USER u FOR LOGIN l")]
	[InlineData("CREATE USER u FROM LOGIN l WITH DEFAULT_SCHEMA = dbo")]
	[InlineData("CREATE USER u WITHOUT LOGIN WITH DEFAULT_SCHEMA = dbo")]
	[InlineData("CREATE USER u FOR CERTIFICATE c")]
	[InlineData("CREATE USER u FROM ASYMMETRIC KEY k")]
	[InlineData("CREATE USER u WITH PASSWORD = 'p', DEFAULT_LANGUAGE = 1033")]
	[InlineData("ALTER USER u WITH NAME = v, DEFAULT_SCHEMA = NULL")]

	[InlineData("CREATE ROLE r")]
	[InlineData("CREATE ROLE r AUTHORIZATION dbo")]
	[InlineData("CREATE SERVER ROLE r AUTHORIZATION sa")]
	[InlineData("ALTER ROLE r ADD MEMBER u")]
	[InlineData("ALTER ROLE r DROP MEMBER u")]
	[InlineData("ALTER SERVER ROLE r WITH NAME = q")]
	[InlineData("CREATE APPLICATION ROLE a WITH PASSWORD = 'p', DEFAULT_SCHEMA = dbo")]
	[InlineData("ALTER APPLICATION ROLE a WITH NAME = b, PASSWORD = 'p'")]

	[InlineData("CREATE SCHEMA s")]
	[InlineData("CREATE SCHEMA s AUTHORIZATION dbo")]
	[InlineData("CREATE SCHEMA AUTHORIZATION dbo")]
	[InlineData("CREATE SCHEMA s AUTHORIZATION dbo CREATE TABLE t (a INT) GRANT SELECT ON t TO u")]
	[InlineData("ALTER SCHEMA s TRANSFER dbo.t")]
	[InlineData("ALTER SCHEMA s TRANSFER OBJECT::dbo.t")]

	// A class is a run of words, which `XML SCHEMA COLLECTION::` is three of.
	[InlineData("ALTER SCHEMA s TRANSFER XML SCHEMA COLLECTION::c")]
	[InlineData("ALTER AUTHORIZATION ON dbo.t TO u")]
	[InlineData("ALTER AUTHORIZATION ON OBJECT::dbo.t TO SCHEMA OWNER")]
	[InlineData("ALTER AUTHORIZATION ON XML SCHEMA COLLECTION::Parts.Sprockets TO [c1]")]
	[InlineData("ALTER AUTHORIZATION ON SEARCH PROPERTY LIST::list1 TO [c1]")]
	public void The_principals_read(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>What lives outside the database, and what the server spends on it.</summary>
	/// <remarks>
	/// Fourteen published blocks and almost all of them are a name and an option list,
	/// which is the shape this grammar has been reading since `CREATE TABLE`. What is new
	/// is four clauses that are not a name and a value: an affinity, a placement, a sample,
	/// and a file specification.
	/// </remarks>
	[Theory]
	[InlineData("CREATE EXTERNAL DATA SOURCE ds WITH (LOCATION = 'hdfs://x:8020', TYPE = HADOOP)")]
	[InlineData("ALTER EXTERNAL DATA SOURCE ds SET LOCATION = 'abs://x', CREDENTIAL = cr")]
	[InlineData("CREATE EXTERNAL FILE FORMAT ff WITH (FORMAT_TYPE = DELIMITEDTEXT, " +
		"FORMAT_OPTIONS (FIELD_TERMINATOR = '|', USE_TYPE_DEFAULT = TRUE))")]
	[InlineData("CREATE EXTERNAL TABLE dbo.t (a INT, b NVARCHAR (10) COLLATE Latin1_General_CI_AS NULL) " +
		"WITH (LOCATION = '/f', DATA_SOURCE = ds, FILE_FORMAT = ff, REJECT_TYPE = value, REJECT_VALUE = 0)")]
	[InlineData("CREATE EXTERNAL TABLE dbo.t WITH (LOCATION = '/f', DATA_SOURCE = ds) AS SELECT a FROM u")]
	[InlineData("CREATE EXTERNAL LIBRARY l FROM (CONTENT = 'c:\\a.zip', PLATFORM = WINDOWS) WITH (LANGUAGE = 'R')")]
	[InlineData("ALTER EXTERNAL LIBRARY l SET (CONTENT = 0x0100) WITH (LANGUAGE = 'R')")]

	// A pool's affinity is a value that is itself a name and a value.
	[InlineData("CREATE RESOURCE POOL p WITH (MIN_CPU_PERCENT = 10, MAX_CPU_PERCENT = 20)")]
	[InlineData("CREATE RESOURCE POOL p WITH (AFFINITY SCHEDULER = AUTO)")]
	[InlineData("CREATE RESOURCE POOL p WITH (AFFINITY SCHEDULER = (0 TO 3, 7))")]
	[InlineData("ALTER RESOURCE POOL p WITH (AFFINITY SCHEDULER = NUMANODE = (0))")]
	[InlineData("CREATE EXTERNAL RESOURCE POOL p WITH (MAX_CPU_PERCENT = 1)")]
	[InlineData("ALTER EXTERNAL RESOURCE POOL p WITH (MAX_MEMORY_PERCENT = 5)")]
	[InlineData("CREATE WORKLOAD GROUP g WITH (IMPORTANCE = HIGH) USING p_int, EXTERNAL p_ext")]
	[InlineData("ALTER WORKLOAD GROUP g USING EXTERNAL p_ext")]

	// `SAMPLE 50 PERCENT` is not a run of words: a number is a lexeme whose class holds a
	// `.`, so it is not a word however much §4.6 says a digit continues one.
	[InlineData("CREATE STATISTICS s ON dbo.t (a, b) WITH FULLSCAN")]
	[InlineData("CREATE STATISTICS s ON t (a) WHERE a > 5 WITH SAMPLE 12 ROWS, NORECOMPUTE")]
	[InlineData("CREATE STATISTICS s ON t (a) WITH SAMPLE 50 PERCENT, PERSIST_SAMPLE_PERCENT = ON")]
	[InlineData("UPDATE STATISTICS dbo.t")]
	[InlineData("UPDATE STATISTICS dbo.t ix WITH FULLSCAN")]
	[InlineData("UPDATE STATISTICS [dbo].t1 (c1, c2) WITH NORECOMPUTE, SAMPLE 1 PERCENT, COLUMNS")]
	[InlineData("UPDATE STATISTICS t WITH RESAMPLE ON PARTITIONS (1, 3 TO 5)")]
	public void What_lives_outside_reads(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>What the server watches, and who it listens to.</summary>
	/// <remarks>
	/// Audits, extended-event sessions, event notifications and endpoints — eleven published
	/// blocks and three shapes: a name and where its output goes, a list of things added and
	/// dropped a bracket at a time, and a bracketed argument list per protocol.
	/// </remarks>
	[Theory]
	[InlineData("CREATE SERVER AUDIT a TO FILE (FILEPATH = 'c:\\l', MAXSIZE = 10 GB, MAX_FILES = 2)")]
	[InlineData("CREATE SERVER AUDIT a TO APPLICATION_LOG WITH (QUEUE_DELAY = 1000, ON_FAILURE = CONTINUE)")]
	[InlineData("CREATE SERVER AUDIT a TO SECURITY_LOG WHERE database_name = 'x' AND object_id > 5")]
	[InlineData("ALTER SERVER AUDIT a REMOVE WHERE")]
	[InlineData("ALTER SERVER AUDIT a MODIFY NAME = b")]
	[InlineData("ALTER SERVER AUDIT a WITH (STATE = OFF)")]

	[InlineData("CREATE SERVER AUDIT SPECIFICATION s FOR SERVER AUDIT a ADD (FAILED_LOGIN_GROUP) WITH (STATE = ON)")]
	[InlineData("ALTER SERVER AUDIT SPECIFICATION s FOR SERVER AUDIT a DROP (FAILED_LOGIN_GROUP)")]
	[InlineData("CREATE DATABASE AUDIT SPECIFICATION s FOR SERVER AUDIT a ADD (SELECT ON dbo.t BY dbo)")]
	[InlineData("ALTER DATABASE AUDIT SPECIFICATION s ADD (SELECT ON t BY dbo), " +
		"DROP (INSERT, UPDATE ON t BY dbo) WITH (STATE = ON)")]

	[InlineData("CREATE EVENT SESSION es ON SERVER ADD EVENT b.c")]
	[InlineData("CREATE EVENT SESSION es ON SERVER ADD EVENT b.c ADD TARGET b.c.d")]
	[InlineData("CREATE EVENT SESSION es ON SERVER ADD EVENT b.c (SET b = -5.1, c = 5 ACTION (b.c))")]
	[InlineData("CREATE EVENT SESSION es ON DATABASE ADD EVENT b.d (WHERE a.b (b.c, 5) AND a.b = 5)")]
	[InlineData("CREATE EVENT SESSION es ON SERVER ADD EVENT b.d (WHERE NOT a.b (b.c, 5))")]
	[InlineData("CREATE EVENT SESSION es ON SERVER ADD EVENT b.c WITH (MAX_MEMORY = 4 MB, " +
		"MAX_DISPATCH_LATENCY = 3 SECONDS, TRACK_CAUSALITY = ON)")]
	[InlineData("ALTER EVENT SESSION es ON SERVER STATE = START")]
	[InlineData("ALTER EVENT SESSION es ON SERVER DROP EVENT b.c, ADD TARGET b.d (SET filename = 'x')")]
	[InlineData("CREATE EVENT NOTIFICATION n ON SERVER WITH FAN_IN FOR DDL_LOGIN_EVENTS " +
		"TO SERVICE 'svc', 'current database'")]

	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 4022) FOR TSQL()")]
	[InlineData("CREATE ENDPOINT e AUTHORIZATION l STATE = STARTED AS TCP (LISTENER_IP = ALL, " +
		"LISTENER_PORT = 4022) FOR SERVICE_BROKER (AUTHENTICATION = WINDOWS NTLM CERTIFICATE c, " +
		"ENCRYPTION = SUPPORTED ALGORITHM AES RC4)")]
	[InlineData("CREATE ENDPOINT e STATE = STOPPED AS TCP (LISTENER_IP = (1.2.3.4))")]
	[InlineData("ALTER ENDPOINT e STATE = STARTED, AFFINITY = NONE")]
	public void What_the_server_watches_reads(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>Every word the grammar reads after `DROP` has a record to be read into.</summary>
	/// <remarks>
	/// `DropKind` in the grammar and `Statement.Dropped` in the tree are two spellings of one
	/// catalogue and have to agree. Nothing in either says so, so this does: the words are
	/// taken out of the grammar itself, every one of them is put to the parser, and what
	/// comes back has to be a record of its own rather than one shared by two of them.
	/// </remarks>
	[Fact]
	public void Every_word_after_drop_has_a_record()
	{
		var kinds = DropKinds();

		Assert.InRange(kinds.Count, 50, 100);

		var seen = new Dictionary<string, string>(StringComparer.Ordinal);

		foreach (var kind in kinds)
		{
			var input = $"DROP {kind} x";
			var match = TransactSql.TryParseStatement(input);

			Assert.True(match.IsSuccess, input);

			var made = Assert.IsAssignableFrom<Statement>(match.Value).GetType().Name;

			Assert.StartsWith("Drop", made, StringComparison.Ordinal);

			// `PROC` and `PROCEDURE` are the one statement written two ways, and nothing
			// else here may share a record: two kinds reading into one is the catalogue
			// having drifted in the direction the compiler cannot see.
			if (seen.TryGetValue(made, out var already))
				Assert.Equal("PROCEDURE", already);

			seen[made] = kind;
		}
	}

	/// <summary>The words of `DropKind`, out of the grammar rather than out of a list here.</summary>
	static List<string> DropKinds()
	{
		// Read with its line endings squared up, since what is looked for below is a blank
		// line and the file's own are CRLF.
		var text = File
			.ReadAllText(
				Path.Combine(Root(AppContext.BaseDirectory), "src", "DotGram.Parsers", "Sql", "TransactSql", "TransactSql.gram"))
			.Replace("\r\n", "\n", StringComparison.Ordinal);

		var body  = text[text.IndexOf("DropKind\n", StringComparison.Ordinal)..];
		var kinds = new List<string>();

		foreach (var line in body[..body.IndexOf("\n\n", StringComparison.Ordinal)].Split('\n').Skip(1))
		{
			var words = Regex.Matches(line, "\"([A-Za-z_]+)\"i").Select(one => one.Groups[1].Value).ToArray();

			if (words.Length > 0)
				kinds.Add(string.Join(' ', words));
		}

		return kinds;
	}

	static string Root(string from)
	{
		var at = new DirectoryInfo(from);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? from;
	}

	/// <summary>A statement comes back as the record its production is named after.</summary>
	/// <remarks>
	/// The claim the tree makes: a consumer switches on the record and is done, and never
	/// reads a word to find out which statement it has. These were one record with a string
	/// in it until the tree was made to say what the grammar says.
	/// </remarks>
	[Theory]
	[InlineData("PRINT 1",                          "Print")]
	[InlineData("RETURN",                           "Return")]
	[InlineData("BREAK",                            "Break")]
	[InlineData("GOTO done",                        "GoTo")]
	[InlineData("USE master",                       "Use")]
	[InlineData("WAITFOR DELAY '00:01'",            "WaitFor")]
	[InlineData("RAISERROR ('x', 1, 1)",            "RaiseError")]

	[InlineData("DROP TABLE t",                     "DropTable")]
	[InlineData("DROP VIEW v",                      "DropView")]
	[InlineData("DROP PROC p",                      "DropProcedure")]
	[InlineData("DROP INDEX ix ON t",               "DropIndex")]
	[InlineData("DROP MASTER KEY",                  "DropMasterKey")]

	[InlineData("GRANT SELECT ON t TO u",           "Grant")]
	[InlineData("DENY SELECT ON t TO u",            "Deny")]
	[InlineData("REVOKE SELECT ON t FROM u",        "Revoke")]

	[InlineData("SET TRANSACTION ISOLATION LEVEL SNAPSHOT", "SetStatement")]
	[InlineData("SET IDENTITY_INSERT t ON",         "SetStatement")]
	[InlineData("SET ANSI_NULLS, ANSI_PADDING ON",  "SetStatement")]
	[InlineData("SET LANGUAGE us_english",          "SetStatement")]
	[InlineData("SET @a = 1",                       "SetVariable")]

	[InlineData("CREATE DATABASE d",                "CreateDatabase")]
	[InlineData("ALTER DATABASE d SET AUTO_CLOSE ON", "AlterDatabaseSet")]
	[InlineData("ALTER DATABASE d COLLATE Estonian_CS_AS", "AlterDatabaseCollate")]
	[InlineData("ALTER DATABASE d MODIFY NAME = e", "AlterDatabaseModifyName")]
	[InlineData("ALTER DATABASE d REBUILD LOG",     "AlterDatabaseRebuildLog")]

	[InlineData("CREATE LOGIN l WITH PASSWORD = 'p'", "CreateLogin")]
	[InlineData("CREATE USER u",                    "CreateUser")]
	[InlineData("CREATE SCHEMA s",                  "SchemaDefinition")]
	[InlineData("ALTER AUTHORIZATION ON t TO u",    "AlterAuthorization")]

	[InlineData("CREATE EXTERNAL FILE FORMAT f WITH (FORMAT_TYPE = PARQUET)", "ExternalFileFormatDefinition")]
	[InlineData("CREATE WORKLOAD GROUP g",          "WorkloadGroupDefinition")]
	[InlineData("CREATE EVENT SESSION es ON SERVER ADD EVENT a.b", "EventSessionDefinition")]
	[InlineData("CREATE ENDPOINT e AS TCP (LISTENER_PORT = 1)", "EndpointDefinition")]
	[InlineData("EXECUTE AS USER = 'u' WITH NO REVERT", "ExecuteAs")]
	[InlineData("REVERT WITH COOKIE = @c",          "Revert")]
	[InlineData("SETUSER 'u' WITH NORESET",         "SetUser")]
	[InlineData("RESTORE SYMMETRIC KEY k FROM FILE = 'f' DECRYPTION BY PASSWORD = 'p' ENCRYPTION BY PASSWORD = 'q'",
		"RestoreSymmetricKey")]

	[InlineData("CREATE TABLE t (a INT)",           "TableDefinition")]
	[InlineData("CREATE VIEW v AS SELECT a FROM t", "ViewDefinition")]
	[InlineData("SELECT a FROM t",                  "Select")]
	[InlineData("SELECT a FROM t ORDER BY a",       "Select")]
	[InlineData("INSERT INTO t (a) VALUES (1)",     "Insert")]
	[InlineData("BEGIN PRINT 1 END",                "Compound")]
	public void A_statement_is_the_record_its_production_is_named_after(string input, string node)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
		Assert.Equal(node, match.Value!.GetType().Name);
	}

	/// <summary>The full-text catalogue, and copying a database out and back.</summary>
	/// <remarks>
	/// Ten published blocks. A full-text index is the one index with no name of its own —
	/// it is named by the table it is on, there being one per table — and the one whose
	/// columns carry a language and a type column beside them. `BACKUP` and `RESTORE` are
	/// one shape between them: what is being copied, the devices it goes to or comes from,
	/// and a long option list.
	/// </remarks>
	[Theory]
	[InlineData("CREATE FULLTEXT INDEX ON t KEY INDEX ix")]
	[InlineData("CREATE FULLTEXT INDEX ON t (a, b TYPE COLUMN c LANGUAGE 1033 STATISTICAL_SEMANTICS) " +
		"KEY INDEX ix ON (cat, FILEGROUP fg) WITH (CHANGE_TRACKING = AUTO, STOPLIST = SYSTEM)")]
	[InlineData("CREATE FULLTEXT INDEX ON t KEY INDEX ix WITH CHANGE_TRACKING MANUAL")]
	[InlineData("ALTER FULLTEXT INDEX ON t ENABLE")]
	[InlineData("ALTER FULLTEXT INDEX ON t SET CHANGE_TRACKING = OFF")]
	[InlineData("ALTER FULLTEXT INDEX ON t ADD (a LANGUAGE 1033) WITH NO POPULATION")]
	[InlineData("ALTER FULLTEXT INDEX ON t ALTER COLUMN a ADD STATISTICAL_SEMANTICS")]
	[InlineData("ALTER FULLTEXT INDEX ON t DROP (a, b)")]
	[InlineData("ALTER FULLTEXT INDEX ON t START FULL POPULATION")]
	[InlineData("ALTER FULLTEXT INDEX ON t PAUSE POPULATION")]

	[InlineData("CREATE FULLTEXT CATALOG c ON FILEGROUP fg IN PATH 'c:\\x' WITH ACCENT_SENSITIVITY = ON AS DEFAULT AUTHORIZATION dbo")]
	[InlineData("ALTER FULLTEXT CATALOG c REBUILD WITH ACCENT_SENSITIVITY = OFF")]
	[InlineData("CREATE FULLTEXT STOPLIST s FROM SYSTEM STOPLIST")]
	[InlineData("ALTER FULLTEXT STOPLIST s ADD 'the' LANGUAGE 1033")]
	[InlineData("ALTER FULLTEXT STOPLIST s DROP ALL LANGUAGE 1033")]
	[InlineData("CREATE SEARCH PROPERTY LIST p FROM dbo.q AUTHORIZATION dbo")]
	[InlineData("ALTER SEARCH PROPERTY LIST p ADD 'title' WITH (PROPERTY_SET_GUID = 'g', PROPERTY_INT_ID = 1)")]

	[InlineData("BACKUP DATABASE d TO device WITH COMPRESSION")]
	[InlineData("BACKUP DATABASE d TO DISK = 'c:\\d.bak' WITH DIFFERENTIAL, NAME = 'x', STATS = 10")]
	[InlineData("BACKUP DATABASE d FILE = 'f1', FILEGROUP = 'g' TO TAPE = '\\\\.\\tape1'")]
	[InlineData("BACKUP DATABASE d TO DISK = 'a' MIRROR TO DISK = 'b' WITH FORMAT")]
	[InlineData("BACKUP LOG d TO DISK = 'a' WITH NORECOVERY")]
	[InlineData("BACKUP MASTER KEY TO FILE = 'k' ENCRYPTION BY PASSWORD = 'p'")]

	[InlineData("RESTORE DATABASE d")]
	[InlineData("RESTORE DATABASE d FROM DISK = 'a' WITH RECOVERY, MOVE 'x' TO 'y', REPLACE")]
	[InlineData("RESTORE DATABASE d FROM DATABASE_SNAPSHOT = 's'")]
	[InlineData("RESTORE LOG d FROM DISK = 'a' WITH STOPAT = '2020-01-01'")]
	[InlineData("RESTORE HEADERONLY FROM DISK = 'a'")]
	[InlineData("RESTORE MASTER KEY FROM FILE = 'k' DECRYPTION BY PASSWORD = 'p' ENCRYPTION BY PASSWORD = 'q' FORCE")]
	public void The_catalogue_and_the_copies_read(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>The keys, and what is locked with them.</summary>
	/// <remarks>
	/// Seventeen published blocks and one idea running through them: a key is made from
	/// somewhere, locked by something, and the something is a certificate, a password or
	/// another key. `ENCRYPTION BY` is written in six of the seventeen and reads the same in
	/// all six, so it is one rule.
	/// </remarks>
	[Theory]
	[InlineData("CREATE ASYMMETRIC KEY k AUTHORIZATION dbo FROM FILE = 'k.snk'")]
	[InlineData("CREATE ASYMMETRIC KEY k FROM EXECUTABLE FILE = 'a.exe' ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("ALTER ASYMMETRIC KEY k REMOVE PRIVATE KEY")]
	[InlineData("ALTER ASYMMETRIC KEY k WITH PRIVATE KEY (DECRYPTION BY PASSWORD = 'a', ENCRYPTION BY PASSWORD = 'b')")]

	[InlineData("CREATE SYMMETRIC KEY k WITH ALGORITHM = AES_256 ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("CREATE SYMMETRIC KEY k AUTHORIZATION dbo FROM PROVIDER p WITH PROVIDER_KEY_NAME = 'n', CREATION_DISPOSITION = CREATE_NEW")]
	[InlineData("ALTER SYMMETRIC KEY k ADD ENCRYPTION BY CERTIFICATE c, PASSWORD = 'p'")]
	[InlineData("ALTER SYMMETRIC KEY k DROP ENCRYPTION BY ASYMMETRIC KEY a")]

	[InlineData("CREATE CERTIFICATE c WITH SUBJECT = 'x'")]
	[InlineData("CREATE CERTIFICATE c AUTHORIZATION u ENCRYPTION BY PASSWORD = 'p' WITH SUBJECT = 'x', START_DATE = '2020-01-01'")]
	[InlineData("CREATE CERTIFICATE c FROM FILE = 'c.cer' WITH PRIVATE KEY (FILE = 'k.pvk', DECRYPTION BY PASSWORD = 'p')")]
	[InlineData("CREATE CERTIFICATE c FROM ASSEMBLY a")]
	[InlineData("ALTER CERTIFICATE c REMOVE PRIVATE KEY")]
	[InlineData("ALTER CERTIFICATE c WITH ACTIVE FOR BEGIN_DIALOG = ON")]

	[InlineData("CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("ALTER MASTER KEY REGENERATE WITH ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("ALTER MASTER KEY ADD ENCRYPTION BY SERVICE MASTER KEY")]
	[InlineData("CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = AES_256 ENCRYPTION BY SERVER CERTIFICATE c")]
	[InlineData("ALTER DATABASE ENCRYPTION KEY REGENERATE WITH ALGORITHM = AES_128")]

	[InlineData("CREATE COLUMN ENCRYPTION KEY k WITH VALUES (COLUMN_MASTER_KEY = m, ALGORITHM = 'a', ENCRYPTED_VALUE = 0x01)")]
	[InlineData("CREATE COLUMN MASTER KEY m WITH (KEY_STORE_PROVIDER_NAME = 'p', KEY_PATH = 'x')")]

	[InlineData("CREATE CREDENTIAL c WITH IDENTITY = 'i', SECRET = 's'")]
	[InlineData("ALTER CREDENTIAL c WITH IDENTITY = 'i'")]
	[InlineData("CREATE CREDENTIAL c WITH IDENTITY = 'i' FOR CRYPTOGRAPHIC PROVIDER p")]
	[InlineData("CREATE DATABASE SCOPED CREDENTIAL c WITH IDENTITY = 'i', SECRET = 's'")]

	[InlineData("CREATE SECURITY POLICY dbo.p ADD FILTER PREDICATE dbo.f(a) ON dbo.t WITH (STATE = ON)")]
	[InlineData("CREATE SECURITY POLICY p ADD BLOCK PREDICATE dbo.f(a, b) ON dbo.t AFTER INSERT, " +
		"ADD BLOCK PREDICATE dbo.f(a) ON dbo.u BEFORE UPDATE NOT FOR REPLICATION")]
	[InlineData("ALTER SECURITY POLICY dbo.p WITH (STATE = ON)")]
	[InlineData("ALTER SECURITY POLICY dbo.p DROP FILTER PREDICATE ON dbo.t")]
	public void The_keys_read(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	/// <summary>What GRAM5009 named, put to the parser.</summary>
	/// <remarks>
	/// The diagnostic says an optional can take what follows it; whether it does is a
	/// question for the parser, and these are the answers. The old unbracketed `WITH` did:
	/// `PAD_INDEX` was set to the `ON` that began the placement after it, and the placement
	/// was left to nobody. The others do not — ordered choice at the rule level gives the
	/// alternative back and another one reads it — and they are here because a reader of the
	/// diagnostic's list deserves to know which kind each of the eighteen is.
	/// </remarks>
	[Theory]
	[InlineData("CREATE INDEX ix ON t (a) WITH PAD_INDEX ON ps (a)")]
	[InlineData("CREATE INDEX ix ON t (a) WITH (PAD_INDEX = ON) ON ps (a)")]
	[InlineData("BEGIN INSERT INTO t (a) OUTPUT inserted.a VALUES (1) PRINT 1 END")]
	[InlineData("BEGIN DELETE FROM t OUTPUT deleted.a PRINT 1 END")]
	[InlineData("BEGIN BEGIN TRAN WITH MARK PRINT 1 END")]
	[InlineData("CREATE SYMMETRIC KEY k WITH ALGORITHM = AES_256 ENCRYPTION BY PASSWORD = 'p'")]
	public void What_the_diagnostic_named_reads(string input)
	{
		var match = TransactSql.TryParseStatement(input);

		Assert.True(match.IsSuccess, input + "  ||  stopped at: " + input.Substring((int)match.Position));
	}

	// ── And builds the standard's tree ───────────────────────────────────────────

	/// <summary>
	/// A call says what it calls and with what, which is the one node the dialect adds.
	/// </summary>
	[Fact]
	public void A_call_says_its_name_and_its_arguments()
	{
		var query = Selected("SELECT dbo.f(a, 1) FROM t");

		var call = Assert.IsType<Expression.RoutineInvocation>(
			Assert.IsType<Clause.DerivedColumn>(Assert.Single(query.Columns)).Value);

		Assert.Equal("dbo.f", call.Name);
		Assert.Equal(2, call.Arguments.Length);
		Assert.Equal("a", Assert.IsType<Expression.ColumnReference>(call.Arguments[0]).Text);
	}

	/// <summary>A variable is a parameter, which is the rule it was widened into.</summary>
	[Theory]
	[InlineData("SELECT @v",         "@v")]
	[InlineData("SELECT @@ROWCOUNT", "@@ROWCOUNT")]
	public void A_variable_stands_where_a_parameter_does(string input, string text)
	{
		var query = Selected(input);

		var literal = Assert.IsType<Expression.Literal>(
			Assert.IsType<Clause.DerivedColumn>(Assert.Single(query.Columns)).Value);

		Assert.Equal(SqlLiteralKind.Parameter, literal.Kind);
		Assert.Equal(text, literal.Text);
	}

	/// <summary>
	/// And what is read and dropped is dropped: the tree a dialect builds is the standard's.
	/// </summary>
	/// <remarks>
	/// Both were read and dropped until the tree was made lossless: <c>TOP</c> says how many
	/// rows come back and <c>OVER</c> says which rows a call sees, and neither says what the
	/// rows or the call <em>are</em>. They are part of the text all the same, and a tree that
	/// cannot print back what it read cannot be checked against another parser.
	/// </remarks>
	[Fact]
	public void A_count_and_a_window_say_what_was_written()
	{
		var topped = Selected("SELECT TOP 10 PERCENT a FROM t");
		var top    = Assert.IsType<Clause.Top>(topped.Top);

		Assert.True(top.Percent);
		Assert.Null(top.With);
		Assert.Equal("10", Assert.IsType<Expression.Literal>(top.Value).Text);
		Assert.Null(Selected("SELECT a FROM t").Top);

		var over = Assert.IsType<Expression.WindowFunction>(
			Assert.IsType<Clause.DerivedColumn>(
				Assert.Single(Selected("SELECT COUNT(*) OVER (PARTITION BY b) FROM t").Columns)).Value);

		var window = Assert.IsType<Clause.Window>(over.Over);

		Assert.Equal("COUNT", Assert.IsType<Expression.RoutineInvocation>(over.Function).Name);
		Assert.Equal("b", Assert.IsType<Expression.ColumnReference>(
			Assert.Single(window.PartitionBy)).Text);
	}

	/// <summary>And the clauses the statement wraps a query in.</summary>
	[Fact]
	public void A_select_keeps_the_clauses_written_around_it()
	{
		var read = Assert.IsType<Statement.Select>(
			TransactSql.TryParseStatement(
				"WITH c AS (SELECT a FROM u) SELECT a INTO #t FROM c ORDER BY a " +
				"OFFSET 5 ROWS FETCH NEXT 2 ROWS ONLY FOR XML AUTO OPTION (MAXDOP 2)").Value);

		var named = Assert.IsType<Clause.CommonTableExpression>(Assert.Single(read.With));
		var by    = Assert.IsType<Clause.OrderBy>(read.OrderBy);

		Assert.Equal("c", named.Name);
		Assert.Equal("#t", Assert.IsType<Clause.Into>(Assert.IsType<Query.Specification>(read.Of).Into).Table);
		Assert.Equal("5", Assert.IsType<Expression.Literal>(by.Offset).Text);
		Assert.Equal("2", Assert.IsType<Expression.Literal>(by.Fetch).Text);
		Assert.Equal("XML", Assert.IsType<Clause.For>(Assert.Single(read.For)).Kind);
		Assert.Equal("MAXDOP 2", Assert.IsType<Clause.Hint>(Assert.Single(read.Options)).Text);
	}

	/// <summary>What the dialect read, where it was a query.</summary>
	static Query.Specification Selected(string input) =>
		Assert.IsType<Query.Specification>(
			Assert.IsType<Statement.Select>(TransactSql.TryParseSelect(input).Value).Of);

	/// <summary>A bracketed name may be a reserved word, which is what brackets are for.</summary>
	/// <remarks>
	/// The lookahead that refuses <c>SELECT</c> as a name stands in front of the regular
	/// spelling only, exactly as it does in the standard.
	/// </remarks>
	[Fact]
	public void A_bracketed_name_may_be_a_reserved_word()
	{
		var query = Selected("SELECT [select] FROM t");

		Assert.Equal(
			"[select]",
			Assert.IsType<Expression.ColumnReference>(
				Assert.IsType<Clause.DerivedColumn>(Assert.Single(query.Columns)).Value).Text);

		Assert.False(TransactSql.TryParseSelect("SELECT select FROM t").IsSuccess);
	}

	/// <summary>A table variable is where the rows come from.</summary>
	[Fact]
	public void A_table_variable_is_a_source()
	{
		var query  = Selected("SELECT a FROM @rows AS r");
		var source = Assert.IsType<TableReference.Named>(Assert.Single(query.From));

		Assert.Equal("@rows", source.Table);
		Assert.Equal("r", source.Name);
	}

	// ── And the other three publications came across ─────────────────────────────

	/// <summary>
	/// A dialect publishes four entry points, and each is the standard's rule read through
	/// the substitution rather than a rule of its own.
	/// </summary>
	[Fact]
	public void Every_entry_point_reads_the_dialect()
	{
		Assert.True(TransactSql.TryParseQuery          ("SELECT [a] FROM t UNION SELECT @v").IsSuccess);
		Assert.True(TransactSql.TryParseSearchCondition("[a] > dbo.f(@v)")                  .IsSuccess);
		Assert.True(TransactSql.TryParseValueExpression("dbo.f(@v) + [b]")                  .IsSuccess);

		Assert.False(SqlStandard92.TryParseSearchCondition("[a] > dbo.f(@v)").IsSuccess);
		Assert.False(SqlStandard92.TryParseValueExpression("dbo.f(@v) + [b]").IsSuccess);
	}

	/// <summary>And what is not read yet is refused, where the reading stopped.</summary>
	[Theory]
	[InlineData("SELECT TOP")]
	[InlineData("SELECT [a")]
	[InlineData("SELECT @")]
	public void What_is_not_read_yet_is_refused(string input)
	{
		var match = TransactSql.TryParseSelect(input);

		Assert.False(match.IsSuccess, input);
		Assert.InRange(match.Position, 0, input.Length);
	}
}
