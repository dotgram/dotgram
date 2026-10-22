using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Sql.Ast;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// A value an enum does not name, put where the writer reads one: no text says it, so none may be written. Each statement
/// is read and carried to the common tree, and then every enum member of it in turn is given a value no enum of the tree
/// has; each writer, T-SQL's and the standard's, refuses the tree, or the test names the member it wrote text for.
/// </summary>
public sealed class UndefinedEnumTests
{
	/// <summary>Statements that between them hold the enums that choose a word: an order, a bound, a kind of key, a row word, a quantifier.</summary>
	public static readonly string[] Statements =
	[
		"SELECT t::a, t2::f(), (a.b()).A",
		"SELECT id, COUNT(c) OVER (ORDER BY id ROWS BETWEEN UNBOUNDED PRECEDING AND 1 FOLLOWING), SUM(DISTINCT a) FROM t ORDER BY id DESC",
		"SELECT DISTINCT a FROM t WHERE a != 1 AND b < 2 ORDER BY a OFFSET 5 ROWS FETCH NEXT 2 ROWS ONLY",
		"SELECT FIRST_VALUE(m) IGNORE NULLS OVER ()",
		"SELECT JSON_OBJECT('a' : 1 ABSENT ON NULL), JSON_ARRAY(1 ABSENT ON NULL)",
		"SELECT JSON_OBJECTAGG(c1 : c2 ABSENT ON NULL), JSON_ARRAYAGG(c1 ORDER BY c1 NULL ON NULL) FROM t",
		"SELECT * FROM fun2 TABLESAMPLE SYSTEM (12 PERCENT)",
		"SELECT Title FROM Production.Document WHERE FREETEXT((t2.*), N'abc')",
		"CREATE TABLE t (a INT PRIMARY KEY, b INT UNIQUE, d DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL, e DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL, PERIOD FOR SYSTEM_TIME (d, e), INDEX i (a DESC))",
		"CREATE PROCEDURE p @a INT OUTPUT AS SELECT 1",
		"CREATE TRIGGER dbo.tr ON dbo.t AFTER INSERT AS RETURN",
		"DECLARE c1 INSENSITIVE SCROLL CURSOR FOR SELECT * FROM t1",
		"DROP FUNCTION x, y",
		"REVOKE GRANT OPTION FOR EXECUTE ON EXTERNAL MODEL::MyModel TO tempuser CASCADE",

		// One statement of the corpus for each enum member the statements above leave out, the shortest that holds it.
		"CREATE DATABASE AUDIT SPECIFICATION s FOR SERVER AUDIT a ADD (SELECT ON OBJECT::dbo.t BY NULL)",
		"ALTER SECURITY POLICY p DROP BLOCK PREDICATE ON dbo.t AFTER INSERT",
		"SELECT a FROM t WHERE a !< ALL (SELECT b FROM u)",
		"CREATE SECURITY POLICY p ADD BLOCK PREDICATE dbo.f(c) ON dbo.t BEFORE DELETE NOT FOR REPLICATION",
		"CREATE TYPE t FROM decimal(10, 2) NULL",
		"ALTER AVAILABILITY GROUP ag1 DENY CREATE ANY DATABASE",
		"ALTER DATABASE ENCRYPTION KEY REGENERATE WITH ALGORITHM = AES_256",
		"ALTER ASSEMBLY a1 WITH PERMISSION_SET = EXTERNAL_ACCESS",
		"alter trigger trig1 on employees for insert as create table t1 (c1 int);",
		"DROP SIGNATURE FROM ASSEMBLY::a BY CERTIFICATE c",
		"ALTER EXTERNAL LANGUAGE Java REMOVE PLATFORM LINUX",
		"ALTER DATABASE d1 MODIFY FILEGROUP fg1 READONLY;",
		"ALTER DATABASE d1 MODIFY FILEGROUP fg1 AUTOGROW_ALL_FILES",
		"DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT a FROM t",
		"disable trigger a on all server",
		"ALTER DATABASE db1 ADD FILEGROUP fg1 CONTAINS FILESTREAM",
		"CREATE CONTRACT c (m SENT BY INITIATOR)",
		"DECLARE c1 CURSOR DYNAMIC OPTIMISTIC FOR SELECT * FROM t1",
		"CREATE DATABASE d1 ON PRIMARY (NAME = a, FILENAME = 'a'), FILEGROUP fg CONTAINS FILESTREAM (NAME = b, FILENAME = 'b')",
		"CREATE FUNCTION f1 (@storeid VARCHAR (30)) RETURNS INT WITH CALLED ON NULL INPUT AS BEGIN RETURN 1 END",
		"ALTER SERVICE s (ADD CONTRACT c)",
		"CREATE VIEW v1 AS SELECT * FROM t1 WITH CHECK OPTION",
		"DROP EVENT SESSION x ON SERVER",
		"DROP TRIGGER tr ON DATABASE",
		"alter schema sc1 transfer type::t1",
		"CREATE ASSEMBLY a FROM N'a.dll' WITH PERMISSION_SET = EXTERNAL_ACCESS",
		"CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = AES_256 ENCRYPTION BY SERVER CERTIFICATE c",
		"SELECT JSON_QUERY(N'{\"a\":1}' WITH ARRAY WRAPPER)",
		"KILL 53 WITH COMMIT",
		"rollback tran",
		"ADD SIGNATURE TO OBJECT :: p BY CERTIFICATE c",
		"ALTER AUTHORIZATION ON OBJECT::dbo.t TO SCHEMA OWNER",
		"ENABLE TRIGGER tr ON DATABASE",
		"p @a OUTPUT",
		"ALTER FULLTEXT INDEX ON t START FULL POPULATION",
		"CREATE DATABASE d1 CONTAINMENT = NONE;",
		"SELECT c1 FROM t1 GROUP BY () WITH CUBE",
		"ALTER TABLE t NOCHECK CONSTRAINT ALL",
		"ALTER EVENT SESSION s ON SERVER STATE = STOP",
		"CREATE SPATIAL INDEX i ON t(g) USING GEOMETRY_GRID",
		"CREATE TRIGGER tr ON ALL SERVER FOR LOGON AS PRINT 1",
		"ALTER VIEW View1 AS SELECT * FROM Table1",
		"BACKUP MASTER KEY TO URL = 'u' ENCRYPTION BY PASSWORD = 'p'",
		"CREATE PARTITION FUNCTION pf (INT) AS RANGE LEFT FOR VALUES ()",
		"SET TRAN ISOLATION LEVEL SNAPSHOT",
		"execute p1 with result sets none",
		"SET STATISTICS IO on",
		"SELECT TOP 5 WITH TIES a FROM t ORDER BY a",
		"SELECT TRIM (LEADING 'x' FROM 'xa')",
		"CREATE XML INDEX i ON t(c) USING XML INDEX sxi FOR PATH",
		"ALTER TABLE t ALTER COLUMN c int NULL",
		"WAITFOR TIME @t",
		"SELECT 'Year', DATETRUNC(year, @d);",
		"CREATE RULE r AS @x LIKE 'a%'",
		"CREATE TABLE t (c xml(CONTENT s) NULL)",
		"ALTER ENDPOINT e1 FOR SOAP (DROP WEBMETHOD 'm1')",
		"SELECT JSON_VALUE('c', '$' RETURNING DATE);",
		"CREATE TABLE t (c int REFERENCES u (a) ON DELETE CASCADE ON UPDATE NO ACTION)",
		"ALTER TABLE t WITH NOCHECK ADD CHECK (a > 0)",
		"SELECT * FROM t1 OPTION (KEEPFIXED PLAN)",
		"SELECT @a += 1",
		"FETCH NEXT FROM c",
		"commit tran",
		"SELECT value FROM GENERATE_SERIES (1, 5);",
		"select * from t1 for xml path",
		"CREATE EVENT SESSION es1 ON SERVER ADD EVENT b.d (WHERE a.b (b.c, 5) AND a.b = 5);",
		"GRANT EXEC ON t TO u",
		"CREATE DATABASE Archive ON (FILENAME = 'zzz') FOR ATTACH",
		"select * from t1 union select * from t2",
		"select * from t1 for json auto",
		"BACKUP DATABASE d TO DISK = 'f'",
		"ALTER TABLE t ALTER COLUMN c ADD HIDDEN",
		"select * from t1 for json auto, root",
		"select * from (t1 inner remote join t2 on 1 = 1)",
		"ALTER PROCEDURE p AS SELECT 1",
		"SET @a = 1",
		"INSERT OVER t1 SELECT 1",
		"SELECT * FROM t WITH (NOLOCK);",
		"CREATE TABLE t (x PRIMARY KEY CLUSTERED)",
		"EXECUTE AS CALLER",
		"SELECT * FROM a WHERE MATCH([N]-([E])->[N2])",
		"SELECT ~1",
		"UPDATE t SET c = 1",
		"Create columnstore Index cindx on t(c1)",
		"create table t1 as filetable",
		"CREATE QUEUE q ON fg",
		"DECLARE @a varbinary(max)",
		"CREATE CLUSTERED COLUMNSTORE INDEX cix ON t",
	];

	/// <summary>
	/// Statements the standard's parser reads, for the enums that only the standard has: one for each member the statements above
	/// leave out, the shortest of the parser's tests that holds it. Only the standard's writer is asked of these.
	/// </summary>
	public static readonly string[] StandardStatements =
	[
		"SELECT a FROM t FOR SYSTEM_TIME BETWEEN SYMMETRIC a AND b",
		"SELECT a FROM t MATCH_RECOGNIZE (ALL ROWS PER MATCH SHOW EMPTY MATCHES PATTERN (A | B) DEFINE A AS TRUE)",
		"CREATE ROLE r WITH ADMIN CURRENT_USER",
		"ALLOCATE :c INSENSITIVE SCROLL CURSOR WITH HOLD WITHOUT RETURN FOR GLOBAL :s",
		"DROP ASSERTION x CASCADE",
		"DROP COLLATION c CASCADE",
		"ALTER TABLE t DROP CONSTRAINT k RESTRICT",
		"DROP DOMAIN d CASCADE",
		"DROP ORDERING FOR t CASCADE",
		"ALTER TABLE t DROP PERIOD FOR p CASCADE",
		"DROP SCHEMA s CASCADE",
		"ALTER TABLE t ALTER COLUMN c DROP SCOPE CASCADE",
		"DROP SEQUENCE q RESTRICT",
		"ALTER TABLE t DROP SYSTEM VERSIONING RESTRICT",
		"DROP VIEW v RESTRICT",
		"SELECT a FROM JSON_TABLE (j, '$' AS p COLUMNS (n FOR ORDINALITY, a INTEGER PATH '$.a' NULL ON EMPTY ERROR ON ERROR, b JSON FORMAT JSON PATH '$.b' WITH CONDITIONAL ARRAY WRAPPER KEEP QUOTES ON SCALAR STRING EMPTY ARRAY ON EMPTY, NESTED PATH '$.c[*]' AS q COLUMNS (d VARCHAR(10))) PLAN (p OUTER q) EMPTY ON ERROR) AS t",
		"SELECT a FROM t GROUP BY DISTINCT a COLLATE c, (b, c), CUBE ((a), b)",
		"CREATE TYPE t AS INTERVAL DAY TO SECOND FINAL",
		"SELECT NTH_VALUE(a, 1) FROM FIRST IGNORE NULLS OVER w FROM t",
		"SELECT a FROM t WHERE (a, b) MATCH UNIQUE PARTIAL (SELECT a, b FROM u)",
		"MERGE INTO t USING u ON a WHEN NOT MATCHED THEN INSERT OVERRIDING SYSTEM VALUE VALUES (1)",
		"SELECT INTERSECTION(a) MULTISET UNION FUSION(b) FROM t",
		"CREATE FUNCTION f () RETURNS INT READS SQL DATA PARAMETER STYLE SQL RETURN 1",
		"SELECT VALUE_OF(a AT CURRENT_ROW - :k) FROM t",
		"COMMIT AND CHAIN",
		"DECLARE LOCAL TEMPORARY TABLE t (a INT) ON COMMIT DELETE ROWS",
		"SELECT a FROM JSON_TABLE (j, '$' COLUMNS (NESTED '$.c' COLUMNS (d INT)) PLAN DEFAULT (INNER, CROSS)) t",
		"DROP CAST (t AS INT) RESTRICT",
		"ALTER TABLE t DROP c RESTRICT",
		"DROP TABLE t CASCADE",
		"DROP TRANSFORM ALL FOR t CASCADE",
		"CREATE FUNCTION f (t TABLE WITH SET SEMANTICS KEEP ON EMPTY) RETURNS ONLY PASS THROUGH FULFILL WITH PROCEDURE g",
		"SELECT EVERY(a = 1) IS TRUE FROM t",
		"ALTER TYPE t DROP STATIC METHOD m (INT) RESTRICT",
		"REVOKE ADMIN OPTION FOR r FROM u CASCADE",
		"CREATE CONSTRUCTOR METHOD m (a INT) FOR t RETURN 1",
		"ALLOCATE c FOR STATIC METHOD m () FOR s.t",
		"CREATE FUNCTION f () RETURNS INT NEW SAVEPOINT LEVEL RETURN 1",
		"ALTER TABLE t ALTER COLUMN c SET GENERATED ALWAYS SET INCREMENT BY 2 RESTART WITH 5",
		"SELECT a FROM t ORDER BY a DESC NULLS LAST",
		"TRUNCATE TABLE t RESTART IDENTITY",
		"GET DIAGNOSTICS :x = ALL CONDITION",
		"CREATE FUNCTION f () RETURNS TABLE RETURN TABLE (SELECT a FROM t)",
		"DROP TYPE t CASCADE",
		"CREATE FUNCTION f (t TABLE PASS THROUGH WITH ROW SEMANTICS) RETURNS ONLY PASS THROUGH RETURN 1",
		"REVOKE GRANT OPTION FOR SELECT ON t FROM u, PUBLIC GRANTED BY CURRENT_USER RESTRICT",
		"CREATE TABLE t (a INT, UNIQUE NULLS NOT DISTINCT (a), FOREIGN KEY (a) REFERENCES u MATCH FULL ON UPDATE SET NULL)",
		"ROLLBACK AND CHAIN",
		"WITH RECURSIVE r (n) AS (VALUES 1) SEARCH DEPTH FIRST BY n SET s SELECT n FROM r",
		"CREATE TABLE t OF u (REF IS id DERIVED)",
		"SET CONSTRAINTS ALL DEFERRED",
		"ALTER TRANSFORM FOR t g (DROP (TO SQL, FROM SQL RESTRICT))",
		"SELECT a FROM t WINDOW w AS (GROUPS BETWEEN CURRENT ROW AND :x FOLLOWING EXCLUDE TIES)",
		"CREATE TYPE t AS (a INT) NOT FINAL CAST (SOURCE AS REF) WITH r CAST (REF AS SOURCE) WITH s",
		"GET DIAGNOSTICS CONDITION 1 :m = MESSAGE_TEXT",
		"CREATE ASSERTION x CHECK (1 = 1) INITIALLY DEFERRED",
		"CREATE COLLATION c FOR utf8 FROM d NO PAD",
		"SET SQL DESCRIPTOR 'd' COUNT = 2",
		"CREATE PROCEDURE p () LANGUAGE C EXTERNAL NAME 'x' PARAMETER STYLE GENERAL",
		"CREATE FUNCTION f () RETURNS INT EXTERNAL EXTERNAL SECURITY IMPLEMENTATION DEFINED",
		"INSERT INTO s.t (a) OVERRIDING USER VALUE VALUES (1)",
		"SELECT FINAL FIRST(a) FROM t",
		"ALTER TYPE t ADD STATIC METHOD m () RETURNS INT LANGUAGE SQL",
		"SELECT a FROM t MATCH_RECOGNIZE (AFTER MATCH SKIP TO NEXT ROW SEEK PATTERN (A) DEFINE A AS a)",
		"SET LOCAL TRANSACTION READ ONLY",
		"CREATE TABLE t AS (SELECT a FROM u) WITH NO DATA",
		"CREATE PROCEDURE p () SQL SECURITY DEFINER CALL q()",
		"GET DIAGNOSTICS n = NUMBER",
		"ALTER PROCEDURE p NAME 'x' RESTRICT",
		"CREATE GLOBAL TEMPORARY TABLE t OF u ON COMMIT DELETE ROWS",
		"SELECT a FROM OLD TABLE (DELETE FROM t) x",
		"GET SQL DESCRIPTOR 'd' :n = COUNT",
		"SELECT a FROM t FOR UPDATE",
		"ALTER FUNCTION f (INT) FOR s.t CALLED ON NULL INPUT NO SQL RESTRICT",
		"CREATE TABLE t (c GENERATED ALWAYS AS IDENTITY)",
		"CREATE TRIGGER g AFTER INSERT ON t REFERENCING OLD o CALL p()",
		"CREATE TRIGGER g AFTER INSERT ON t FOR EACH ROW INSERT INTO u VALUES (1)",
		"CREATE ORDERING FOR t ORDER FULL BY STATE",
		"CREATE TRANSFORM FOR t g (TO SQL WITH FUNCTION f, FROM SQL WITH FUNCTION h)",
		"DROP ROUTINE f RESTRICT",
		"SELECT a FROM t MATCH_RECOGNIZE (PATTERN (A*? ) DEFINE A AS a)",
	];

	/// <summary>Value expressions the standard's parser reads, for the enums of the expressions that no statement above holds.</summary>
	public static readonly string[] StandardExpressions =
	[
		"EXTRACT(TIMEZONE_HOUR FROM a)",
		"a IS NFC NORMALIZED",
		"JSON_ARRAY(SELECT a FROM t FORMAT JSON NULL ON NULL RETURNING JSON)",
		"JSON_OBJECTAGG('a' : b ABSENT ON NULL WITH UNIQUE KEYS)",
		"JSON_QUERY(a, '$' RETURNING JSON FORMAT JSON WITHOUT ARRAY WRAPPER OMIT QUOTES ON SCALAR STRING NULL ON ERROR)",
		"JSON_VALUE(a FORMAT JSON ENCODING UTF8, '$.x')",
		"NORMALIZE(a, NFC, 10 CHARACTERS)",
		"SUBSTRING(a SIMILAR b ESCAPE c) || TRIM(LEADING FROM a) || OVERLAY(a PLACING b FROM 1 FOR 2 USING OCTETS)",
		"POSITION(a IN b USING CHARACTERS)",
		"POSITION_REGEX(START 'a' IN b OCCURRENCE 2)",
		"JSON_EXISTS(a, 'strict $.x' TRUE ON ERROR)",
		"JSON_OBJECT('a' : 1, KEY 'b' VALUE 2 ABSENT ON NULL WITH UNIQUE KEYS RETURNING JSON)",
		"JSON(a WITH UNIQUE KEYS)",
		"JSON_QUERY(a, '$.x' WITH CONDITIONAL ARRAY WRAPPER KEEP QUOTES EMPTY OBJECT ON EMPTY)",
		"CHAR_LENGTH(a USING OCTETS)",
		"PERIOD(a, b) CONTAINS c",
		"a IS JSON OBJECT WITH UNIQUE KEYS",
		"SUBSTRING(X'0A' FROM 1 USING OCTETS)",
		"INTERVAL - '1' YEAR",
		"a[$.type()]",
		"SET(a) MULTISET EXCEPT DISTINCT b",
	];

	/// <summary>The members the writer does not yet refuse, as <c>Record.Member</c>, each with what it writes instead.</summary>
	static readonly Dictionary<string, string> Unrefused = new(StringComparer.Ordinal)
	{
		// The standard's writer takes the style and the kind of a literal from its text, which holds the delimiters and the digits; T-SQL's refuses them.
		["Identifier.Style (the standard)"] = "the identifier's text",
		["Numeric.Kind (the standard)"]     = "the numeric literal's text",
	};

	const int Beyond = 1000;

	/// <summary>The tree with the <paramref name="at"/>th enum member (counted in walk order) given a value beyond the enum's.</summary>
	static ISqlNode? Mutated(ISqlNode tree, int at, out string site)
	{
		var seen  = 0;
		var found = "";
		var made  = Map(tree, "", (value, where) =>
		{
			if (seen++ != at)
				return null;

			found = where;

			return Enum.ToObject(value.GetType(), Beyond);
		});

		site = found;

		return found.Length == 0 ? null : (ISqlNode)made;
	}

	/// <summary>The number of enum members a tree holds.</summary>
	static int Members(ISqlNode tree)
	{
		var count = 0;

		Map(tree, "", (_, _) =>
		{
			count++;

			return null;
		});

		return count;
	}

	/// <summary>
	/// A copy of a node with every enum member passed to <paramref name="change"/>, which answers the value to put there or null to leave it:
	/// a record cloned and its properties set, a list rebuilt item by item, a name rebuilt part by part.
	/// </summary>
	static object Map(object node, string owner, Func<object, string, object?> change)
	{
		if (node is QualifiedName name)
			return new QualifiedName(SqlList.From(Enumerable.Range(0, name.Count).Select(at => (Identifier)Map(name[at], owner, change)).ToArray()));

		var type = node.GetType();

		if (node is not ISqlNode || type.Namespace != typeof(DotGram.Sql.Ast.Statement).Namespace || !type.IsClass || type.GetMethod("<Clone>$") is not { } clone)
			return node;

		object? copy = null;

		foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (!property.CanWrite || property.GetIndexParameters().Length > 0 || property.GetValue(node) is not { } value)
				continue;

			var where = type.Name + "." + property.Name;
			object? made = null;

			if (value.GetType().IsEnum)
				made = change(value, where);
			else if (value is ISqlNode)
				made = Map(value, where, change);
			else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(SqlList<>))
			{
				var item   = property.PropertyType.GetGenericArguments()[0];
				var items  = ((IEnumerable)value).Cast<object>().ToArray();
				var mapped = items.Select(one => one is ISqlNode ? Map(one, where, change) : one).ToArray();

				if (!mapped.Zip(items, ReferenceEquals).All(static same => same))
				{
					var typed = Array.CreateInstance(item, mapped.Length);

					Array.Copy(mapped, typed, mapped.Length);
					made = typeof(SqlList).GetMethod(nameof(SqlList.From))!.MakeGenericMethod(item).Invoke(null, [typed]);
				}
			}

			if (made is not null && !ReferenceEquals(made, value))
			{
				copy ??= clone.Invoke(node, null)!;
				property.SetValue(copy, made);
			}
		}

		return copy ?? node;
	}

	/// <summary>The members at which the text cannot say what the tree does, and the writer writes it anyway, as <c>Record.Member</c>, each with why.</summary>
	static readonly Dictionary<string, string> Lossy = new(StringComparer.Ordinal)
	{
		// The standard's writer takes the style of a name and the kind of a number from its text, which holds the delimiters and the digits.
		["Identifier.Style"] = "the text decides, and a style of another kind than the text's is read back as the text's",
		["Numeric.Kind"]     = "the text decides, and a kind of another kind than the number's is read back as the number's",
	};

	/// <summary>
	/// Every member an enum names, put in turn where the writer reads one: the tree is refused, or its text reads as a tree that holds
	/// the same enum members. A text that does not read at all is left alone, and so is a text that reads as a tree whose other parts
	/// differ (the written name of a type that the kind no longer matches); text that reads as another meaning is a silent change.
	/// </summary>
	[Fact]
	public void A_member_an_enum_names_is_refused_or_reads_back_as_itself()
	{
		var results = new Dictionary<string, string>(StringComparer.Ordinal);

		void Check(string source, ISqlNode tree, Func<ISqlNode, string> write, Func<string, ISqlNode> read, string reading)
		{
			var count = Members(tree);

			for (var at = 0; at < count; at++)
			{
				object? original = null;
				var     site     = "";
				var     seen     = 0;

				Map(tree, "", (value, where) =>
				{
					if (seen++ == at)
					{
						original = value;
						site     = where;
					}

					return null;
				});

				foreach (var member in Enum.GetValues(original!.GetType()))
				{
					if (member.Equals(original))
						continue;

					var key     = site + " = " + member + " (" + reading + ")";
					var visited = 0;
					var changed = (ISqlNode)Map(tree, "", (_, _) => visited++ == at ? member : null);
					string text;

					try
					{
						text = write(changed);
					}
					catch (Exception thrown) when (thrown is ArgumentException or NotSupportedException or InvalidOperationException)
					{
						continue;
					}
					catch (Exception thrown)
					{
						results[key] = "crashed with " + thrown.GetType().Name + " in " + source;

						continue;
					}

					ISqlNode back;

					try
					{
						back = read(text);
					}
					catch (Exception)
					{
						continue;
					}

					if (results.TryGetValue(key, out var kept) && !kept.StartsWith("read as", StringComparison.Ordinal))
						continue;

					results[key] = Enums(back).SequenceEqual(Enums(changed)) || back.Equals(changed)
						? "fine"
						: "read as another meaning, from " + text;
				}
			}
		}

		foreach (var text in Statements)
			Check(text, TransactSqlParser.Located.ParseStatement(text), static node => TransactSqlWriter.Write(node), static text => TransactSqlParser.Located.ParseStatement(text), "T-SQL");

		foreach (var text in StandardStatements)
			Check(text, SqlStandardParser.ParseStatement(text), static node => SqlWriter.Write(node), static text => SqlStandardParser.ParseStatement(text), "the standard");

		foreach (var text in StandardExpressions)
			Check(text, SqlStandardParser.ParseExpression(text), static node => SqlWriter.Write(node), static text => SqlStandardParser.ParseExpression(text), "the standard");

		static string Site(string key)
		{
			return key.Substring(0, key.IndexOf(" = ", StringComparison.Ordinal));
		}

		var wrong = results.Where(static one => one.Value != "fine").ToList();
		var bad   = wrong.Where(one => !Lossy.ContainsKey(Site(one.Key)) || one.Key.Contains("(T-SQL)", StringComparison.Ordinal)).Select(static one => one.Key + ": " + one.Value).Order(StringComparer.Ordinal).ToList();
		var stale = Lossy.Keys.Where(one => !wrong.Any(found => Site(found.Key) == one)).ToList();

		Assert.True(results.Count(static one => one.Value == "fine") > 500, "the members read back as themselves are too few: " + results.Count);
		Assert.True(bad.Count == 0, "the writer wrote text that reads as another meaning, or crashed, for a member the enum names:\n" + string.Join("\n", bad));
		Assert.True(stale.Count == 0, "these are no longer lossy:\n" + string.Join("\n", stale));
	}

	/// <summary>The enum members of a tree, in a stable order, as <c>Record.Member=value</c>.</summary>
	static List<string> Enums(ISqlNode tree)
	{
		var list = new List<string>();

		Map(tree, "", (value, where) =>
		{
			list.Add(where + "=" + value);

			return null;
		});
		list.Sort(StringComparer.Ordinal);

		return list;
	}

	[Fact]
	public void An_enum_value_no_enum_names_is_refused()
	{
		var written = new List<string>();
		var crashed = new List<string>();
		var members = 0;

		foreach (var text in Statements)
		{
			var tree  = (ISqlNode)TransactSqlParser.Located.ParseStatement(text);
			var count = Members(tree);

			Assert.True(count > 0, text);
			members += count;

			// A statement T-SQL's writer refuses as it is would make every one of its mutations refused for that, and test nothing.
			Assert.NotEmpty(TransactSqlWriter.Write(tree));

			for (var at = 0; at < count; at++)
			{
				var changed = Mutated(tree, at, out var site)!;

				foreach (var (write, writer) in new (Func<ISqlNode, string>, string)[] { (TransactSqlWriter.Write, "T-SQL"), (SqlWriter.Write, "the standard") })
					try
					{
						written.Add(site + " (" + writer + ") -> " + write(changed));
					}
					catch (Exception thrown) when (thrown is ArgumentException or NotSupportedException or InvalidOperationException)
					{
					}
					catch (Exception thrown)
					{
						crashed.Add(site + " (" + writer + ") -> " + thrown.GetType().Name);
					}
			}
		}

		foreach (var text in StandardStatements.Concat(StandardExpressions))
		{
			var tree  = StandardExpressions.Contains(text) ? (ISqlNode)SqlStandardParser.ParseExpression(text) : SqlStandardParser.ParseStatement(text);
			var count = Members(tree);

			Assert.True(count > 0, text);
			members += count;
			Assert.NotEmpty(SqlWriter.Write(tree));

			for (var at = 0; at < count; at++)
			{
				var changed = Mutated(tree, at, out var site)!;

				try
				{
					written.Add(site + " (the standard) -> " + SqlWriter.Write(changed));
				}
				catch (Exception thrown) when (thrown is ArgumentException or NotSupportedException or InvalidOperationException)
				{
				}
				catch (Exception thrown)
				{
					crashed.Add(site + " (the standard) -> " + thrown.GetType().Name);
				}
			}
		}

		Assert.True(members > 40, "the statements hold " + members + " enum members; this test expects them to hold the enums that choose a word");

		var sites      = written.Select(static one => one.Substring(0, one.IndexOf(" -> ", StringComparison.Ordinal))).Distinct().ToList();
		var unexpected = sites.Where(one => !Unrefused.ContainsKey(one)).Select(one => written.First(text => text.StartsWith(one + " -> ", StringComparison.Ordinal))).ToList();
		var fixedNow   = Unrefused.Keys.Where(one => !sites.Contains(one)).ToList();

		Assert.True(crashed.Count == 0, "the writer crashed, rather than refused, on a value no enum names:\n" + string.Join("\n", crashed.Distinct().Order(StringComparer.Ordinal)));
		Assert.True(unexpected.Count == 0, "the writer wrote text for a value no enum names:\n" + string.Join("\n", unexpected.Order(StringComparer.Ordinal)));
		Assert.True(fixedNow.Count == 0, "the writer now refuses these, so they are no longer unrefused:\n" + string.Join("\n", fixedNow));
	}
}
