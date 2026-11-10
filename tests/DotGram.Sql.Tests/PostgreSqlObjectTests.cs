using System;

using DotGram.Sql.Ast;
using DotGram.Sql.PostgreSql;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

using DataType = DotGram.Sql.Ast.DataType;
using Expression = DotGram.Sql.Ast.Expression;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// The generic object statements as <see cref="PostgreSqlParser"/> reads them — DROP of every kind and COMMENT ON — held to
/// what was written, to the round trip, and to the trees the standard's and T-SQL's parsers build for a text they read too.
/// </summary>
public sealed class PostgreSqlObjectTests
{
	static Statement Holds(string text)
	{
		var made    = PostgreSqlParser.Located.ParseStatement(text);
		var written = PostgreSqlWriter.Write(made);
		var back    = PostgreSqlParser.TryParseStatement(written);

		Assert.True(back.IsSuccess, $"{text}: written as {written}, which does not read back");
		Assert.True(made.Equals(back.Value), $"{text}: written as {written}, which reads back as {back.Value}");
		Assert.Equal(written, PostgreSqlWriter.Write(back.Value!));
		Assert.True(made.Equals(PostgreSqlParser.ParseStatement(text)), $"{text}: the located reading differs from the plain one");

		return made;
	}

	// ── DROP ───────────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("DROP TABLE t",                                              "table")]
	[InlineData("DROP TABLE IF EXISTS a, s.b, c.s.d CASCADE",                "tables")]
	[InlineData("DROP SEQUENCE s RESTRICT",                                  "sequence")]
	[InlineData("DROP VIEW v",                                               "view")]
	[InlineData("DROP MATERIALIZED VIEW IF EXISTS m, n",                     "materialized view")]
	[InlineData("DROP SCHEMA IF EXISTS s, t CASCADE",                        "schema")]
	[InlineData("DROP INDEX i",                                              "index")]
	[InlineData("DROP INDEX CONCURRENTLY IF EXISTS s.i, j",                  "index concurrently")]
	[InlineData("DROP FOREIGN TABLE f",                                      "foreign table")]
	[InlineData("DROP TEXT SEARCH CONFIGURATION IF EXISTS c CASCADE",        "text search configuration")]
	[InlineData("DROP STATISTICS s.st",                                      "statistics")]
	[InlineData("DROP EXTENSION IF EXISTS e, f CASCADE",                     "extension")]
	[InlineData("DROP FOREIGN DATA WRAPPER w",                               "foreign data wrapper")]
	[InlineData("DROP EVENT TRIGGER et",                                     "event trigger")]
	[InlineData("DROP ACCESS METHOD am",                                     "access method")]
	[InlineData("DROP LANGUAGE plpgsql",                                     "language")]
	[InlineData("DROP SERVER s RESTRICT",                                    "server")]
	[InlineData("DROP PUBLICATION p",                                        "publication")]
	[InlineData("DROP TRIGGER IF EXISTS tr ON s.t CASCADE",                  "trigger")]
	[InlineData("DROP POLICY p ON t",                                        "policy")]
	[InlineData("DROP RULE IF EXISTS r ON t RESTRICT",                       "rule")]
	[InlineData("DROP TYPE IF EXISTS t, s.u CASCADE",                        "type")]
	[InlineData("DROP DOMAIN d",                                             "domain")]
	[InlineData("DROP FUNCTION f",                                           "function bare")]
	[InlineData("DROP FUNCTION IF EXISTS f(), s.g(int, text[]) CASCADE",     "functions")]
	[InlineData("DROP FUNCTION f(IN a int, OUT text, INOUT b int, VARIADIC int[])", "function modes")]
	[InlineData("DROP FUNCTION f(IN OUT int, double precision)",             "function in out")]
	[InlineData("DROP PROCEDURE p(int)",                                     "procedure")]
	[InlineData("DROP ROUTINE r",                                            "routine")]
	[InlineData("DROP AGGREGATE IF EXISTS a(*), b(int, \"any\")",            "aggregates")]
	[InlineData("DROP OPERATOR IF EXISTS + (int, int), pg_catalog.@@ (NONE, text), ! (bigint, NONE) CASCADE", "operators")]
	[InlineData("DROP OPERATOR CLASS c USING btree",                         "operator class")]
	[InlineData("DROP OPERATOR FAMILY IF EXISTS s.f USING hash CASCADE",     "operator family")]
	[InlineData("DROP CAST IF EXISTS (int AS text) CASCADE",                 "cast")]
	[InlineData("DROP TRANSFORM FOR int LANGUAGE sql",                       "transform")]
	[InlineData("DROP ROLE r",                                               "role")]
	[InlineData("DROP USER IF EXISTS a, b",                                  "user")]
	[InlineData("DROP GROUP g",                                              "group")]
	[InlineData("DROP DATABASE IF EXISTS d",                                 "database")]
	[InlineData("DROP TABLESPACE IF EXISTS ts",                              "tablespace")]
	[InlineData("DROP SUBSCRIPTION s CASCADE",                               "subscription")]
	[InlineData("DROP TABLE",                                                "refused: no name")]
	[InlineData("DROP TRIGGER tr",                                           "refused: a trigger with no table")]
	[InlineData("DROP SCHEMA s.t",                                           "refused: a schema of two parts")]
	[InlineData("DROP AGGREGATE a",                                          "refused: an aggregate with no arguments")]
	[InlineData("DROP AGGREGATE a(int ORDER BY int)",                        "refused: an ordered-set aggregate")]
	[InlineData("DROP OPERATOR + (int)",                                     "refused: an operator with one type")]
	[InlineData("DROP OPERATOR + (NONE, NONE)",                              "refused: an operator with no types")]
	[InlineData("DROP ROLE CURRENT_USER",                                    "refused: a role specifier")]
	[InlineData("DROP INDEX CONCURRENTLY",                                   "refused: concurrently with no name")]
	[InlineData("DROP CAST (int AS text), (text AS int)",                    "refused: two casts")]
	[InlineData("DROP TABLESPACE ts CASCADE",                                "refused: a tablespace with a behavior")]
	public void A_drop_builds_its_record(string text, string expected)
	{
		if (expected.StartsWith("refused", StringComparison.Ordinal))
		{
			Assert.False(PostgreSqlParser.TryParseStatement(text).IsSuccess);
			return;
		}

		var made  = Holds(text);
		var holds = expected switch
		{
			"table"                     => made is Statement.DropTable { Names: [{ Count: 1 }], IfExists: false, Behavior: null },
			"tables"                    => made is Statement.DropTable { Names: [{ Count: 1 }, { Count: 2 }, { Count: 3 }], IfExists: true, Behavior: DropBehavior.Cascade },
			"sequence"                  => made is Statement.DropSequence { Behavior: DropBehavior.Restrict },
			"view"                      => made is Statement.DropView { Materialized: false },
			"materialized view"         => made is Statement.DropView { Materialized: true, IfExists: true, Names.Count: 2 },
			"schema"                    => made is Statement.DropSchema { Names.Count: 2, IfExists: true },
			"index"                     => made is Statement.DropObject { Kind: ObjectKind.Index, Objects: [ObjectReference.Named { Kind: ObjectKind.Index }], Concurrently: false },
			"index concurrently"        => made is Statement.DropObject { Kind: ObjectKind.Index, Concurrently: true, IfExists: true, Objects.Count: 2 },
			"foreign table"             => made is Statement.DropObject { Kind: ObjectKind.ForeignTable },
			"text search configuration" => made is Statement.DropObject { Kind: ObjectKind.TextSearchConfiguration, Behavior: DropBehavior.Cascade },
			"statistics"                => made is Statement.DropObject { Kind: ObjectKind.Statistics, Objects: [ObjectReference.Named { Name.Count: 2 }] },
			"extension"                 => made is Statement.DropObject { Kind: ObjectKind.Extension, Objects.Count: 2 },
			"foreign data wrapper"      => made is Statement.DropObject { Kind: ObjectKind.ForeignDataWrapper },
			"event trigger"             => made is Statement.DropObject { Kind: ObjectKind.EventTrigger },
			"access method"             => made is Statement.DropObject { Kind: ObjectKind.AccessMethod },
			"language"                  => made is Statement.DropObject { Kind: ObjectKind.Language },
			"server"                    => made is Statement.DropObject { Kind: ObjectKind.Server, Behavior: DropBehavior.Restrict },
			"publication"               => made is Statement.DropObject { Kind: ObjectKind.Publication },
			"trigger"                   => made is Statement.DropTrigger { Names: [{ Count: 1 }], Table.Count: 2, Behavior: DropBehavior.Cascade, On: null },
			"policy"                    => made is Statement.DropObject { Kind: ObjectKind.Policy, Objects: [ObjectReference.OnTable { Kind: ObjectKind.Policy }] },
			"rule"                      => made is Statement.DropObject { Kind: ObjectKind.Rule, IfExists: true },
			"type"                      => made is Statement.DropType { Names.Count: 2, Behavior: DropBehavior.Cascade },
			"domain"                    => made is Statement.DropDomain { Names.Count: 1, Behavior: null },
			"function bare"             => made is Statement.DropRoutine { Routines: [{ Kind: RoutineKind.Function, Parameters: null }] },
			"functions"                 => made is Statement.DropRoutine { Routines: [{ Parameters.Count: 0 }, { Parameters: [{ Mode: null, Name: null }, { Type: DataType.Array }] }], IfExists: true },
			"function modes"            => made is Statement.DropRoutine { Routines: [{ Parameters: [{ Mode: ParameterMode.In, Name.Text: "a" }, { Mode: ParameterMode.Out, Name: null }, { Mode: ParameterMode.InOut, InOutTwoWords: false }, { Mode: ParameterMode.Variadic }] }] },
			"function in out"           => made is Statement.DropRoutine { Routines: [{ Parameters: [{ Mode: ParameterMode.InOut, InOutTwoWords: true }, { Name: null, Type: DataType.Numeric }] }] },
			"procedure"                 => made is Statement.DropRoutine { Routines: [{ Kind: RoutineKind.Procedure }], Proc: false },
			"routine"                   => made is Statement.DropRoutine { Routines: [{ Kind: RoutineKind.Routine }] },
			"aggregates"                => made is Statement.DropRoutine { Routines: [{ Kind: RoutineKind.Aggregate, Parameters.Count: 0 }, { Kind: RoutineKind.Aggregate, Parameters.Count: 2 }] },
			"operators"                 => made is Statement.DropObject { Kind: ObjectKind.Operator, Objects: [ObjectReference.Operator { Left: not null, Right: not null }, ObjectReference.Operator { Name.Count: 2, Left: null }, ObjectReference.Operator { Right: null }] },
			"operator class"            => made is Statement.DropObject { Kind: ObjectKind.OperatorClass, Objects: [ObjectReference.OperatorClass { Method.Text: "btree" }] },
			"operator family"           => made is Statement.DropObject { Kind: ObjectKind.OperatorFamily, Objects: [ObjectReference.OperatorClass { Kind: ObjectKind.OperatorFamily, Name.Count: 2 }] },
			"cast"                      => made is Statement.DropObject { Kind: ObjectKind.Cast, Objects: [ObjectReference.Cast], IfExists: true },
			"transform"                 => made is Statement.DropObject { Kind: ObjectKind.Transform, Objects: [ObjectReference.Transform { Language.Text: "sql" }] },
			"role"                      => made is Statement.DropRole { Word: null, Names.Count: 1 },
			"user"                      => made is Statement.DropRole { Word: RoleWord.User, IfExists: true, Names.Count: 2 },
			"group"                     => made is Statement.DropRole { Word: RoleWord.Group },
			"database"                  => made is Statement.DropDatabase { IfExists: true, Names.Count: 1 },
			"tablespace"                => made is Statement.DropObject { Kind: ObjectKind.Tablespace, IfExists: true },
			"subscription"              => made is Statement.DropObject { Kind: ObjectKind.Subscription, Behavior: DropBehavior.Cascade },
			_                           => throw new ArgumentOutOfRangeException(nameof(expected), expected, null),
		};

		Assert.True(holds, $"{text}: {made}");
	}

	// ── COMMENT ON ─────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("COMMENT ON TABLE s.t IS 'a table'",                         "table")]
	[InlineData("COMMENT ON COLUMN t.c IS NULL",                             "column")]
	[InlineData("COMMENT ON MATERIALIZED VIEW m IS $$x$$",                   "materialized view")]
	[InlineData("COMMENT ON INDEX i IS E'\\n'",                              "index")]
	[InlineData("COMMENT ON SCHEMA s IS 'x'",                                "schema")]
	[InlineData("COMMENT ON ROLE r IS 'x'",                                  "role")]
	[InlineData("COMMENT ON DATABASE d IS 'x'",                              "database")]
	[InlineData("COMMENT ON TYPE s.t IS 'x'",                                "type")]
	[InlineData("COMMENT ON DOMAIN d IS 'x'",                                "domain")]
	[InlineData("COMMENT ON AGGREGATE a(*) IS 'x'",                          "aggregate")]
	[InlineData("COMMENT ON FUNCTION f(a int) IS 'x'",                       "function")]
	[InlineData("COMMENT ON PROCEDURE p IS 'x'",                             "procedure")]
	[InlineData("COMMENT ON ROUTINE r() IS 'x'",                             "routine")]
	[InlineData("COMMENT ON OPERATOR ## (int, NONE) IS 'x'",                 "operator")]
	[InlineData("COMMENT ON OPERATOR CLASS c USING gist IS 'x'",             "operator class")]
	[InlineData("COMMENT ON CONSTRAINT c ON t IS 'x'",                       "constraint")]
	[InlineData("COMMENT ON CONSTRAINT c ON DOMAIN d IS 'x'",                "domain constraint")]
	[InlineData("COMMENT ON TRIGGER tr ON t IS 'x'",                         "trigger")]
	[InlineData("COMMENT ON POLICY p ON s.t IS 'x'",                         "policy")]
	[InlineData("COMMENT ON RULE r ON t IS 'x'",                             "rule")]
	[InlineData("COMMENT ON TRANSFORM FOR int LANGUAGE sql IS 'x'",          "transform")]
	[InlineData("COMMENT ON LARGE OBJECT 12345 IS 'x'",                      "large object")]
	[InlineData("COMMENT ON CAST (int AS text) IS 'x'",                      "cast")]
	[InlineData("COMMENT ON EVENT TRIGGER e IS 'x'",                         "event trigger")]
	[InlineData("COMMENT ON TEXT SEARCH DICTIONARY d IS 'x'",                "text search dictionary")]
	[InlineData("COMMENT ON COLUMN c IS 'x'",                                "column of one part")]
	[InlineData("COMMENT ON TABLE t IS 1",                                   "refused: a number")]
	[InlineData("COMMENT ON TABLE t",                                        "refused: no text")]
	[InlineData("COMMENT ON SCHEMA s.t IS 'x'",                              "refused: a schema of two parts")]
	[InlineData("COMMENT ON TRIGGER tr IS 'x'",                              "refused: a trigger with no table")]
	public void A_comment_builds_its_record(string text, string expected)
	{
		if (expected.StartsWith("refused", StringComparison.Ordinal))
		{
			Assert.False(PostgreSqlParser.TryParseStatement(text).IsSuccess);
			return;
		}

		var made  = (Statement.Comment)Holds(text);
		var holds = expected switch
		{
			"table"                  => made is { Object: ObjectReference.Named { Kind: ObjectKind.Table, Name.Count: 2 }, Text.Kind: StringLiteralKind.Character },
			"column"                 => made is { Object: ObjectReference.Named { Kind: ObjectKind.Column, Name.Count: 2 }, Text: null },
			"column of one part"     => made.Object is ObjectReference.Named { Kind: ObjectKind.Column, Name.Count: 1 },
			"materialized view"      => made is { Object: ObjectReference.Named { Kind: ObjectKind.MaterializedView }, Text.Kind: StringLiteralKind.Dollar },
			"index"                  => made is { Object: ObjectReference.Named { Kind: ObjectKind.Index }, Text.Kind: StringLiteralKind.Escaped },
			"schema"                 => made.Object is ObjectReference.Named { Kind: ObjectKind.Schema },
			"role"                   => made.Object is ObjectReference.Named { Kind: ObjectKind.Role },
			"database"               => made.Object is ObjectReference.Named { Kind: ObjectKind.Database },
			"type"                   => made.Object is ObjectReference.Named { Kind: ObjectKind.Type, Name.Count: 2 },
			"domain"                 => made.Object is ObjectReference.Named { Kind: ObjectKind.Domain },
			"aggregate"              => made.Object is ObjectReference.Routine { Designator: { Kind: RoutineKind.Aggregate, Parameters.Count: 0 } },
			"function"               => made.Object is ObjectReference.Routine { Designator: { Kind: RoutineKind.Function, Parameters: [{ Name.Text: "a" }] } },
			"procedure"              => made.Object is ObjectReference.Routine { Designator: { Kind: RoutineKind.Procedure, Parameters: null } },
			"routine"                => made.Object is ObjectReference.Routine { Designator: { Kind: RoutineKind.Routine, Parameters.Count: 0 } },
			"operator"               => made.Object is ObjectReference.Operator { Left: not null, Right: null },
			"operator class"         => made.Object is ObjectReference.OperatorClass { Kind: ObjectKind.OperatorClass },
			"constraint"             => made.Object is ObjectReference.OnTable { Kind: ObjectKind.Constraint, DomainWord: false },
			"domain constraint"      => made.Object is ObjectReference.OnTable { Kind: ObjectKind.Constraint, DomainWord: true },
			"trigger"                => made.Object is ObjectReference.OnTable { Kind: ObjectKind.Trigger },
			"policy"                 => made.Object is ObjectReference.OnTable { Kind: ObjectKind.Policy, Table.Count: 2 },
			"rule"                   => made.Object is ObjectReference.OnTable { Kind: ObjectKind.Rule },
			"transform"              => made.Object is ObjectReference.Transform,
			"large object"           => made.Object is ObjectReference.LargeObject { Id: LiteralValue.Numeric },
			"cast"                   => made.Object is ObjectReference.Cast,
			"event trigger"          => made.Object is ObjectReference.Named { Kind: ObjectKind.EventTrigger },
			"text search dictionary" => made.Object is ObjectReference.Named { Kind: ObjectKind.TextSearchDictionary },
			_                        => throw new ArgumentOutOfRangeException(nameof(expected), expected, null),
		};

		Assert.True(holds, $"{text}: {made}");
	}

	// ── One tree with the standard's and T-SQL's readings ──────────────────────

	[Theory]
	[InlineData("DROP TABLE t CASCADE")]
	[InlineData("DROP VIEW v RESTRICT")]
	[InlineData("DROP SEQUENCE s CASCADE")]
	[InlineData("DROP SCHEMA s CASCADE")]
	[InlineData("DROP TYPE t CASCADE")]
	[InlineData("DROP DOMAIN d CASCADE")]
	[InlineData("DROP FUNCTION f (INT, DATE) CASCADE")]
	[InlineData("DROP PROCEDURE s.p RESTRICT")]
	public void A_drop_the_standard_reads_is_one_tree(string text)
	{
		var standard   = SqlStandardParser.ParseStatement(text);
		var postgreSql = PostgreSqlParser.ParseStatement(text);

		Assert.True(standard.Equals(postgreSql), $"{text}:\n  the standard: {SqlWriter.Write(standard)}\n  PostgreSQL:   {PostgreSqlWriter.Write(postgreSql)}\n  {standard}\n  {postgreSql}");
	}

	[Theory]
	[InlineData("DROP TABLE t")]
	[InlineData("DROP TABLE IF EXISTS a, s.b")]
	[InlineData("DROP VIEW IF EXISTS v")]
	[InlineData("DROP SEQUENCE s")]
	[InlineData("DROP FUNCTION f")]
	[InlineData("DROP PROCEDURE IF EXISTS p, q")]
	[InlineData("DROP DATABASE IF EXISTS d")]
	[InlineData("DROP TYPE s.t")]
	[InlineData("DROP SCHEMA IF EXISTS s")]
	public void A_drop_T_SQL_reads_is_one_tree(string text)
	{
		Assert.Equal(TransactSqlParser.ParseStatement(text), PostgreSqlParser.ParseStatement(text));
	}

	// ── The writers ────────────────────────────────────────────────────────────

	static readonly QualifiedName T = new(new Identifier("t"));

	[Theory]
	[InlineData("drop object")]
	[InlineData("comment")]
	[InlineData("materialized")]
	[InlineData("user")]
	[InlineData("trigger on a table")]
	[InlineData("aggregate")]
	[InlineData("a designator's mode")]
	[InlineData("a domain list")]
	public void The_other_writers_refuse_what_only_PostgreSQL_reads(string kind)
	{
		Statement tree = kind switch
		{
			"drop object"         => new Statement.DropObject(ObjectKind.Extension, [new ObjectReference.Named(ObjectKind.Extension, T)]),
			"comment"             => new Statement.Comment(new ObjectReference.Named(ObjectKind.Table, T), null),
			"materialized"        => new Statement.DropView { Names = [T], Materialized = true },
			"user"                => new Statement.DropRole { Names = [new Identifier("u")], Word = RoleWord.User },
			"trigger on a table"  => new Statement.DropTrigger { Names = [T], Table = T },
			"aggregate"           => new Statement.DropRoutine { Routines = [new RoutineDesignator(RoutineKind.Aggregate, T, [])] },
			"a designator's mode" => new Statement.DropRoutine { Routines = [new RoutineDesignator(RoutineKind.Function, T, [new ParameterDefinition(ParameterMode.In, null, new DataType.Numeric(NumericTypeKind.Int), false, null)])] },
			_                     => new Statement.DropDomain { Names = [T, T], Behavior = DropBehavior.Cascade },
		};

		Assert.ThrowsAny<ArgumentException>(() => SqlWriter.Write(tree));
		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(tree));

		var written = PostgreSqlWriter.Write(tree);

		Assert.True(PostgreSqlParser.TryParseStatement(written).IsSuccess, written);
	}

	[Theory]
	[InlineData("a table through the generic drop")]
	[InlineData("an object of another kind")]
	[InlineData("concurrently on an extension")]
	[InlineData("two casts")]
	[InlineData("a column dropped")]
	[InlineData("a schema of two parts")]
	[InlineData("an aggregate with no parameters")]
	[InlineData("a trigger with no table")]
	[InlineData("a large object that is a name")]
	public void The_PostgreSQL_writer_refuses_what_it_cannot_read_back(string kind)
	{
		var cast = new ObjectReference.Cast(new DataType.Numeric(NumericTypeKind.Int), new DataType.Numeric(NumericTypeKind.Int));
		Statement tree = kind switch
		{
			"a table through the generic drop" => new Statement.DropObject(ObjectKind.Table, [new ObjectReference.Named(ObjectKind.Table, T)]),
			"an object of another kind"        => new Statement.DropObject(ObjectKind.Extension, [new ObjectReference.Named(ObjectKind.Server, T)]),
			"concurrently on an extension"     => new Statement.DropObject(ObjectKind.Extension, [new ObjectReference.Named(ObjectKind.Extension, T)], Concurrently: true),
			"two casts"                        => new Statement.DropObject(ObjectKind.Cast, [cast, cast]),
			"a column dropped"                 => new Statement.DropObject(ObjectKind.Column, [new ObjectReference.Named(ObjectKind.Column, T)]),
			"a schema of two parts"            => new Statement.Comment(new ObjectReference.Named(ObjectKind.Schema, new QualifiedName([new Identifier("a"), new Identifier("b")])), null),
			"an aggregate with no parameters"  => new Statement.DropRoutine { Routines = [new RoutineDesignator(RoutineKind.Aggregate, T)] },
			"a trigger with no table"          => new Statement.DropTrigger { Names = [T] },
			_                                  => new Statement.Comment(new ObjectReference.LargeObject(new Expression.Reference(T)), null),
		};

		Assert.ThrowsAny<ArgumentException>(() => PostgreSqlWriter.Write(tree));
	}

	/// <summary>Every kind of object is written with words that read back as that kind, in the statements that name it.</summary>
	[Fact]
	public void Every_kind_of_object_reads_back_as_itself()
	{
		foreach (var kind in Enum.GetValues<ObjectKind>())
		{
			ObjectReference reference = kind switch
			{
				ObjectKind.Function or ObjectKind.Procedure or ObjectKind.Routine or ObjectKind.Aggregate
					=> new ObjectReference.Routine(new RoutineDesignator(PostgreSql.Nodes.RoutineKindOf(kind.ToString()), T, [])),
				ObjectKind.Policy or ObjectKind.Rule or ObjectKind.Trigger or ObjectKind.Constraint => new ObjectReference.OnTable(kind, new Identifier("n"), T),
				ObjectKind.Operator       => new ObjectReference.Operator(new QualifiedName(new Identifier("+")), new DataType.Numeric(NumericTypeKind.Int), new DataType.Numeric(NumericTypeKind.Int)),
				ObjectKind.OperatorClass or ObjectKind.OperatorFamily => new ObjectReference.OperatorClass(kind, T, new Identifier("btree")),
				ObjectKind.Cast           => new ObjectReference.Cast(new DataType.Numeric(NumericTypeKind.Int), new DataType.Numeric(NumericTypeKind.BigInt)),
				ObjectKind.Transform      => new ObjectReference.Transform(new DataType.Numeric(NumericTypeKind.Int), new Identifier("sql")),
				ObjectKind.LargeObject    => new ObjectReference.LargeObject(new LiteralValue.Numeric("1", NumericLiteralKind.DecimalInteger)),
				ObjectKind.Column         => new ObjectReference.Named(kind, new QualifiedName([new Identifier("t"), new Identifier("c")])),
				_                         => new ObjectReference.Named(kind, T),
			};

			var comment = new Statement.Comment(reference, new LiteralValue.String("'x'", StringLiteralKind.Character));
			var written = PostgreSqlWriter.Write(comment);
			var back    = PostgreSqlParser.TryParseStatement(written);

			Assert.True(back.IsSuccess && comment.Equals(back.Value), $"{kind}: {written}");
		}
	}
}
