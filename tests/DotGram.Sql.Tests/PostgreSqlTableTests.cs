using System;

using DotGram.Sql.Ast;
using DotGram.Sql.PostgreSql;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

using DataType = DotGram.Sql.Ast.DataType;
using MatchType = DotGram.Sql.Ast.MatchType;
using Expression = DotGram.Sql.Ast.Expression;
using Query = DotGram.Sql.Ast.Query;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// The table statements as <see cref="PostgreSqlParser"/> reads them — CREATE TABLE in all its forms, SELECT INTO — held
/// node by node to what was written and to the round trip, and held to the trees the standard's and T-SQL's parsers build
/// for a text they read too.
/// </summary>
public sealed class PostgreSqlTableTests
{
	/// <summary>
	/// The statement the text reads as, located, which must read back from its written text into an equal tree (spans
	/// aside) and write the same text again.
	/// </summary>
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

	static ColumnDefinition Column(Statement made, int at = 0)
	{
		return ((TableElement.Column)((TableContents.Elements)((Statement.CreateTable)made).Contents).Items[at]).Definition;
	}

	static Constraint Element(Statement made, int at)
	{
		return ((TableElement.TableConstraint)((TableContents.Elements)((Statement.CreateTable)made).Contents).Items[at]).Value;
	}

	// ── CREATE TABLE ───────────────────────────────────────────────────────────

	[Theory]
	[InlineData("CREATE TABLE t (a int, b text)",                                              "plain")]
	[InlineData("CREATE TABLE t ()",                                                           "no columns")]
	[InlineData("CREATE TEMP TABLE t (a int)",                                                 "temp")]
	[InlineData("CREATE TEMPORARY TABLE t (a int)",                                            "temporary")]
	[InlineData("CREATE LOCAL TEMP TABLE t (a int)",                                           "local temp")]
	[InlineData("CREATE GLOBAL TEMPORARY TABLE t (a int) ON COMMIT PRESERVE ROWS",             "global temporary")]
	[InlineData("CREATE UNLOGGED TABLE IF NOT EXISTS s.t (a int)",                             "unlogged if not exists")]
	[InlineData("CREATE TEMP TABLE t (a int) ON COMMIT DROP",                                  "on commit drop")]
	[InlineData("CREATE TABLE t (a int) INHERITS (p, s.q)",                                    "inherits")]
	[InlineData("CREATE TABLE t (a int, b text) PARTITION BY RANGE (a, (b || 'x'), lower(b) COLLATE \"C\" text_ops)", "partition by")]
	[InlineData("CREATE TABLE t (a int) PARTITION BY \"list\" (a)",                            "partition by quoted strategy")]
	[InlineData("CREATE TABLE p PARTITION OF t FOR VALUES IN (1, 2)",                          "partition of in")]
	[InlineData("CREATE TABLE p PARTITION OF t (a NOT NULL, CONSTRAINT c CHECK (a > 0)) FOR VALUES FROM (MINVALUE, 1) TO (10, MAXVALUE)", "partition of range")]
	[InlineData("CREATE TABLE p PARTITION OF t FOR VALUES WITH (MODULUS 4, REMAINDER 0) PARTITION BY HASH (b)", "partition of hash")]
	[InlineData("CREATE TABLE p PARTITION OF t DEFAULT",                                       "partition of default")]
	[InlineData("CREATE TABLE t OF typ (a WITH OPTIONS NOT NULL, b DEFAULT 1, PRIMARY KEY (a))", "of type")]
	[InlineData("CREATE TABLE t OF typ",                                                       "of type bare")]
	[InlineData("CREATE TABLE t (a int) USING heap WITH (fillfactor = 70, autovacuum_enabled = false, x) TABLESPACE ts", "clauses")]
	[InlineData("CREATE TABLE t (a int) WITHOUT OIDS",                                         "without oids")]
	[InlineData("CREATE TABLE t AS SELECT 1 AS a",                                             "as")]
	[InlineData("CREATE TEMP TABLE t (x, y) USING heap WITH (fillfactor = 10) ON COMMIT DELETE ROWS TABLESPACE ts AS (SELECT 1, 2) WITH NO DATA", "as with all")]
	[InlineData("CREATE TABLE IF NOT EXISTS t AS WITH c AS (SELECT 1) SELECT * FROM c WITH DATA", "as with a common table expression")]
	[InlineData("CREATE TABLE t (LIKE s INCLUDING ALL EXCLUDING INDEXES, a int)",              "like")]
	[InlineData("CREATE TABLE t (a int STORAGE PLAIN COMPRESSION lz4, b text STORAGE DEFAULT COMPRESSION DEFAULT)", "storage and compression")]
	[InlineData("CREATE TABLE t (a int NOT NULL DEFAULT 1, b text NULL COLLATE \"C\")",       "qualifiers")]
	[InlineData("CREATE TABLE t (a int GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT 2 CACHE 10 NO CYCLE OWNED BY t.a SEQUENCE NAME s UNLOGGED AS bigint))", "identity")]
	[InlineData("CREATE TABLE t (a int GENERATED BY DEFAULT AS IDENTITY, b int GENERATED ALWAYS AS (a * 2) STORED, c int GENERATED ALWAYS AS (a + 1) VIRTUAL, d int GENERATED ALWAYS AS (a))", "generated")]
	[InlineData("CREATE TABLE t (a int CONSTRAINT k PRIMARY KEY WITH (fillfactor = 90) USING INDEX TABLESPACE ts, b int UNIQUE NULLS NOT DISTINCT, c int CHECK (c > 0) NO INHERIT, d int NOT NULL NO INHERIT)", "column constraints")]
	[InlineData("CREATE TABLE t (a int REFERENCES u (b) MATCH FULL ON DELETE SET NULL ON UPDATE CASCADE DEFERRABLE INITIALLY DEFERRED NOT ENFORCED)", "column references")]
	[InlineData("CREATE TABLE t (a int REFERENCES u ON DELETE SET DEFAULT (a) INITIALLY IMMEDIATE NOT DEFERRABLE)", "column references with delete columns")]
	[InlineData("CREATE TABLE t (a int UNIQUE COLLATE \"C\" DEFERRABLE)",                      "an attribute after a collation")]
	[InlineData("CREATE TABLE t (a int, CONSTRAINT k PRIMARY KEY (a) INCLUDE (b) WITH (fillfactor = 80) USING INDEX TABLESPACE ts DEFERRABLE)", "table key")]
	[InlineData("CREATE TABLE t (a int, b tstzrange, UNIQUE NULLS NOT DISTINCT (a, b WITHOUT OVERLAPS))", "table unique without overlaps")]
	[InlineData("CREATE TABLE t (a int, CHECK (a > 0) NO INHERIT NOT VALID NOT ENFORCED, NOT NULL a NOT VALID)", "table check and not null")]
	[InlineData("CREATE TABLE t (a int, FOREIGN KEY (a, PERIOD b) REFERENCES u (x, PERIOD y) ON DELETE CASCADE NOT VALID NOT DEFERRABLE)", "table foreign key")]
	[InlineData("CREATE TABLE t (a int, EXCLUDE USING gist (a WITH =, b gist_ops WITH OPERATOR(pg_catalog.&&)) INCLUDE (c) WITH (fillfactor = 70) WHERE (a > 0) INITIALLY DEFERRED)", "exclude")]
	[InlineData("CREATE TABLE t (a int, exclude int, constraint_name int)",                    "names like constraint words")]
	[InlineData("CREATE TABLE t (a int, b int, CONSTRAINT c FOREIGN KEY (a, b) REFERENCES u)", "table foreign key bare")]
	[InlineData("CREATE TABLE t (a int) PARTITION BY foo (a)",                                 "refused: an unknown strategy")]
	[InlineData("CREATE TABLE t (a, b)",                                                       "refused: names with no AS")]
	[InlineData("CREATE TABLE t (a int) AS SELECT 1",                                          "refused: elements before AS")]
	[InlineData("CREATE TABLE t () AS SELECT 1",                                               "refused: an empty column list before AS")]
	[InlineData("CREATE TABLE t (a) INHERITS (p) AS SELECT 1",                                 "refused: INHERITS before AS")]
	[InlineData("CREATE TABLE t",                                                              "refused: no body")]
	[InlineData("CREATE TABLE p PARTITION OF t FOR VALUES WITH (MODULUS 4)",                   "refused: hash with no remainder")]
	[InlineData("CREATE TABLE p PARTITION OF t FOR VALUES WITH (MODULUS 4, MODULUS 2, REMAINDER 0)", "refused: hash with two moduli")]
	[InlineData("CREATE TABLE t (a int GENERATED BY DEFAULT AS (1))",                          "refused: a generation BY DEFAULT")]
	[InlineData("CREATE TABLE t (a int COLLATE \"C\" COLLATE \"POSIX\")",                     "refused: two collations")]
	[InlineData("CREATE TABLE t (a int, CHECK (a > 0) DEFERRABLE)",                            "refused: a deferrable check")]
	[InlineData("CREATE TABLE t (a int, PRIMARY KEY (a) NOT VALID)",                           "refused: a key not valid")]
	[InlineData("CREATE TABLE t (a int, UNIQUE (a) DEFERRABLE NOT DEFERRABLE)",               "refused: conflicting attributes")]
	[InlineData("CREATE TABLE t (a int, FOREIGN KEY (a) REFERENCES u NO INHERIT)",             "refused: a foreign key no inherit")]
	[InlineData("CREATE TABLE t (a int REFERENCES u ON UPDATE SET NULL (a))",                  "refused: columns after ON UPDATE")]
	[InlineData("CREATE TABLE t (a int, PRIMARY KEY NULLS NOT DISTINCT (a))",                  "refused: a primary key with nulls")]
	[InlineData("CREATE TABLE t (a int, UNIQUE (a WITHOUT OVERLAPS))",                         "refused: a period alone")]
	[InlineData("CREATE TABLE a.b.c.d (x int)",                                                "refused: a name of four parts")]
	public void A_create_table_builds_its_record(string text, string expected)
	{
		if (expected.StartsWith("refused", StringComparison.Ordinal))
		{
			Assert.False(PostgreSqlParser.TryParseStatement(text).IsSuccess);
			return;
		}

		var made  = Holds(text);
		var table = (Statement.CreateTable)made;
		var holds = expected switch
		{
			"plain"                       => table is { Scope: null, Unlogged: false, IfNotExists: false, Contents: TableContents.Elements { Items: [TableElement.Column, TableElement.Column] }, PartitionOf: null, Options.Count: 0 },
			"no columns"                  => table.Contents is TableContents.Elements { Items.Count: 0 },
			"temp"                        => table is { Scope: TableScope.Temporary, TempWord: true },
			"temporary"                   => table is { Scope: TableScope.Temporary, TempWord: false },
			"local temp"                  => table is { Scope: TableScope.LocalTemporary, TempWord: true },
			"global temporary"            => table is { Scope: TableScope.GlobalTemporary, TempWord: false, OnCommit: TableCommitAction.Preserve },
			"unlogged if not exists"      => table is { Unlogged: true, IfNotExists: true, Scope: null, Name.Count: 2 },
			"on commit drop"              => table is { OnCommit: TableCommitAction.Drop },
			"inherits"                    => table is { Inherits: [{ Count: 1 }, { Count: 2 }] },
			"partition by"                => table is { PartitionBy: { Strategy.Text: "RANGE", Keys: [{ Key: Expression.Reference }, { Key: Expression.Parenthesized }, { Key: Expression.Invocation, Collation: not null, OperatorClass: not null }] } },
			"partition by quoted strategy" => table is { PartitionBy.Strategy.Style: IdentifierStyle.Delimited },
			"partition of in"             => table is { PartitionOf: { Parent: [{ Text: "t" }], Bound: PartitionBound.In { Values.Count: 2 } }, Contents: TableContents.Elements { Items.Count: 0 } },
			"partition of range"          => table is { PartitionOf.Bound: PartitionBound.Range { From: [Expression.Reference, LiteralValue.Numeric], To.Count: 2 }, Contents: TableContents.Elements { Items: [TableElement.ColumnOptions { WithOptionsOmitted: true, Options.Constraints: [Constraint.NotNull] }, TableElement.TableConstraint { Value: Constraint.Check { Name: not null } }] } },
			"partition of hash"           => table is { PartitionOf.Bound: PartitionBound.Hash { Options: [{ Name.Text: "MODULUS" }, { Name.Text: "REMAINDER" }] }, PartitionBy.Strategy.Text: "HASH" },
			"partition of default"        => table is { PartitionOf.Bound: PartitionBound.Default },
			"of type"                     => table is { Contents: TableContents.Typed { TypeName: [{ Text: "typ" }], Under: null, Items: [TableElement.ColumnOptions { WithOptionsOmitted: false }, TableElement.ColumnOptions { WithOptionsOmitted: true, Options.Default: LiteralValue.Numeric }, TableElement.TableConstraint] } },
			"of type bare"                => table is { Contents: TableContents.Typed { Items.Count: 0 } },
			"clauses"                     => table is { AccessMethod.Text: "heap", Options: [{ Name.Text: "fillfactor", Value: LiteralValue.Numeric }, { Value: LiteralValue.Boolean }, { Value: null }], Tablespace.Text: "ts" },
			"without oids"                => table is { WithoutOids: true, Options.Count: 0 },
			"as"                          => table is { Contents: TableContents.AsQuery { Columns.Count: 0, Data: null, Query: Query.Select } },
			"as with all"                 => table is { Scope: TableScope.Temporary, AccessMethod: not null, Options.Count: 1, OnCommit: TableCommitAction.Delete, Tablespace: not null, Contents: TableContents.AsQuery { Columns.Count: 2, Data: WithDataMode.WithNoData, Query: Query.Parenthesized } },
			"as with a common table expression" => table is { IfNotExists: true, Contents: TableContents.AsQuery { Data: WithDataMode.WithData, Query.With: not null } },
			"like"                        => table.Contents is TableContents.Elements { Items: [TableElement.Like { Options: [LikeOption.IncludingAll, LikeOption.ExcludingIndexes] }, TableElement.Column] },
			"storage and compression"     => Column(made) is { Storage.Text: "PLAIN", Compression.Text: "lz4" } && Column(made, 1) is { Storage.Text: "DEFAULT", Compression.Text: "DEFAULT" },
			"qualifiers"                  => Column(made) is { Generation: ColumnGeneration.Default, Constraints: [Constraint.NotNull] } && Column(made, 1) is { Constraints: [Constraint.Null], Collation: not null },
			"identity"                    => Column(made).Generation is ColumnGeneration.Identity { Generation: IdentityGeneration.Always, Options: [SequenceOption.Start { WithOmitted: false }, SequenceOption.Increment { ByOmitted: true }, SequenceOption.Cache, SequenceOption.Cycle { Value: false }, SequenceOption.OwnedBy, SequenceOption.SequenceName, SequenceOption.Logged { Value: false }, SequenceOption.DataTypeOption] },
			"generated"                   => Column(made).Generation is ColumnGeneration.Identity { Generation: IdentityGeneration.ByDefault, Options.Count: 0 }
			                                 && Column(made, 1).Generation is ColumnGeneration.Generated { Storage: GenerationStorage.Stored }
			                                 && Column(made, 2).Generation is ColumnGeneration.Generated { Storage: GenerationStorage.Virtual }
			                                 && Column(made, 3).Generation is ColumnGeneration.Generated { Storage: null },
			"column constraints"          => Column(made).Constraints is [Constraint.Unique { Kind: UniqueKind.PrimaryKey, Name: not null, Options.Count: 1, IndexTablespace.Text: "ts" }]
			                                 && Column(made, 1).Constraints is [Constraint.Unique { Kind: UniqueKind.Unique, Nulls: NullDistinctness.NotDistinct }]
			                                 && Column(made, 2).Constraints is [Constraint.Check { NoInherit: true }]
			                                 && Column(made, 3).Constraints is [Constraint.NotNull { NoInherit: true, Column: null }],
			"column references"           => Column(made).Constraints is [Constraint.ForeignKey { Columns.Count: 0, References: { Match: MatchType.Full, OnDelete: ReferentialAction.SetNull, OnUpdate: ReferentialAction.Cascade, DeleteRuleFirst: true }, Characteristics: { Deferrable: true, Timing: ConstraintTiming.InitiallyDeferred, Enforced: false, DeferrableFirst: true } }],
			"column references with delete columns" => Column(made).Constraints is [Constraint.ForeignKey { References: { OnDelete: ReferentialAction.SetDefault, OnDeleteColumns.Count: 1, Columns.Count: 0 }, Characteristics: { Deferrable: false, Timing: ConstraintTiming.InitiallyImmediate, DeferrableFirst: false } }],
			"an attribute after a collation" => Column(made) is { Constraints: [Constraint.Unique { Characteristics.Deferrable: true }], Collation: not null },
			"table key"                   => Element(made, 1) is Constraint.Unique { Kind: UniqueKind.PrimaryKey, Columns.Count: 1, Include.Count: 1, Options.Count: 1, IndexTablespace: not null, Characteristics.Deferrable: true },
			"table unique without overlaps" => Element(made, 2) is Constraint.Unique { Nulls: NullDistinctness.NotDistinct, Columns: [{ Text: "a" }], WithoutOverlapsPeriod.Text: "b" },
			"table check and not null"    => Element(made, 1) is Constraint.Check { NoInherit: true, NotValid: true, Characteristics.Enforced: false } && Element(made, 2) is Constraint.NotNull { Column.Text: "a", NotValid: true },
			"table foreign key"           => Element(made, 1) is Constraint.ForeignKey { Columns.Count: 1, ReferencingPeriod.Text: "b", References: { Columns.Count: 1, Period.Text: "y", OnDelete: ReferentialAction.Cascade }, NotValid: true, Characteristics.Deferrable: false },
			"exclude"                     => Element(made, 1) is Constraint.Exclude { Method.Text: "gist", Elements: [{ Operator: [{ Text: "=" }], OperatorKeyword: false }, { Element.OperatorClass: not null, OperatorKeyword: true, Operator.Count: 2 }], Include.Count: 1, Options.Count: 1, Where: not null, Characteristics.Timing: ConstraintTiming.InitiallyDeferred },
			"names like constraint words" => Column(made, 1).Name.Text == "exclude",
			"table foreign key bare"      => Element(made, 2) is Constraint.ForeignKey { Columns.Count: 2, References.Columns.Count: 0, Name: not null },
			_                             => throw new ArgumentOutOfRangeException(nameof(expected), expected, null),
		};

		Assert.True(holds, $"{text}: {made}");
	}

	/// <summary>
	/// The server's own refusals in analysis, of what a column says twice or says against itself, which the tree has one slot
	/// for: the grammar refuses them where the server does, and the oracle fixture records each with the server's verdict.
	/// </summary>
	[Theory]
	[InlineData("CREATE TABLE t (a int DEFAULT 1 DEFAULT 2)")]
	[InlineData("CREATE TABLE t (a int DEFAULT 1 GENERATED ALWAYS AS IDENTITY)")]
	[InlineData("CREATE TABLE t (a int GENERATED ALWAYS AS (1) STORED GENERATED ALWAYS AS (2) STORED)")]
	[InlineData("CREATE TABLE t (a int NULL NOT NULL)")]
	[InlineData("CREATE TABLE t (a int DEFERRABLE)")]
	[InlineData("CREATE TABLE t (a int NOT NULL DEFERRABLE)")]
	[InlineData("CREATE TABLE t (a int CHECK (a > 0) INITIALLY DEFERRED)")]
	[InlineData("CREATE TABLE t (a int UNIQUE ENFORCED)")]
	[InlineData("CREATE TABLE t (a int UNIQUE DEFERRABLE DEFERRABLE)")]
	[InlineData("CREATE TABLE t (a int REFERENCES u INITIALLY DEFERRED NOT DEFERRABLE)")]
	[InlineData("CREATE TABLE t (a int DEFAULT 1 DEFERRABLE)")]
	public void A_column_said_twice_or_against_itself_is_refused(string text)
	{
		Assert.False(PostgreSqlParser.TryParseStatement(text).IsSuccess, text);
	}

	// ── One tree with the standard's and T-SQL's readings ──────────────────────

	[Theory]
	[InlineData("CREATE TABLE t (a INT NOT NULL, b VARCHAR(10) DEFAULT 'x', PRIMARY KEY (a), CHECK (a > 0), FOREIGN KEY (b) REFERENCES u (c) ON DELETE CASCADE)")]
	[InlineData("CREATE GLOBAL TEMPORARY TABLE t (a INT) ON COMMIT PRESERVE ROWS")]
	[InlineData("CREATE LOCAL TEMPORARY TABLE t (a INT) ON COMMIT DELETE ROWS")]
	[InlineData("CREATE TABLE t (a INT CONSTRAINT k UNIQUE, b INT REFERENCES u MATCH SIMPLE ON UPDATE NO ACTION ON DELETE SET NULL)")]
	[InlineData("CREATE TABLE t (a INT, CONSTRAINT u UNIQUE NULLS NOT DISTINCT (a) DEFERRABLE INITIALLY DEFERRED)")]
	[InlineData("CREATE TABLE t (a INT, CONSTRAINT c CHECK (a > 0) INITIALLY IMMEDIATE NOT DEFERRABLE)")]
	[InlineData("CREATE TABLE t (a INT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 2 MAXVALUE 10 NO MINVALUE CYCLE), b INT GENERATED ALWAYS AS (a + 1))")]
	[InlineData("CREATE TABLE t (a INT, b INT, PRIMARY KEY (a, b WITHOUT OVERLAPS))")]
	[InlineData("CREATE TABLE t (a INT, b INT, FOREIGN KEY (a, PERIOD b) REFERENCES u (c, PERIOD d))")]
	[InlineData("CREATE TABLE t (a INT COLLATE \"C\", LIKE u INCLUDING DEFAULTS EXCLUDING IDENTITY)")]
	[InlineData("CREATE TABLE t OF typ")]
	public void A_table_the_standard_reads_is_one_tree(string text)
	{
		var standard   = SqlStandardParser.ParseStatement(text);
		var postgreSql = PostgreSqlParser.ParseStatement(text);

		Assert.True(standard.Equals(postgreSql), $"{text}:\n  the standard: {SqlWriter.Write(standard)}\n  PostgreSQL:   {PostgreSqlWriter.Write(postgreSql)}\n  {standard}\n  {postgreSql}");
	}

	/// <summary>
	/// The standard's table made of a query holds the query inside its brackets, which its grammar requires; PostgreSQL's
	/// keeps the brackets as the node they are, since it reads the query without them as well.
	/// </summary>
	[Fact]
	public void A_table_made_of_a_bracketed_query_keeps_its_brackets()
	{
		const string text = "CREATE TABLE t (a, b) AS (SELECT c, d FROM u) WITH NO DATA";

		var standard   = (Statement.CreateTable)SqlStandardParser.ParseStatement(text);
		var postgreSql = (Statement.CreateTable)PostgreSqlParser.ParseStatement(text);
		var held       = (TableContents.AsQuery)postgreSql.Contents;

		Assert.Equal(standard with { Contents = held with { Query = ((Query.Parenthesized)held.Query).Query } }, postgreSql with { Contents = held with { Query = ((Query.Parenthesized)held.Query).Query } });
		Assert.IsType<Query.Parenthesized>(held.Query);
	}

	[Theory]
	[InlineData("CREATE TABLE s.t (a INT, CHECK (a > 0), FOREIGN KEY (a) REFERENCES u (b))")]
	[InlineData("CREATE TABLE t (a INT, b INT, CONSTRAINT c CHECK (a > b))")]
	public void A_table_T_SQL_reads_is_one_tree(string text)
	{
		Assert.Equal(TransactSqlParser.ParseStatement(text), PostgreSqlParser.ParseStatement(text));
	}

	[Theory]
	[InlineData("CREATE TABLE t (a INT NOT NULL, b VARCHAR(10), CONSTRAINT k PRIMARY KEY (a))")]
	[InlineData("CREATE TABLE t (a INT DEFAULT 1, b INT UNIQUE)")]
	public void A_table_T_SQL_reads_is_another_tree(string text)
	{
		var transactSql = TransactSqlParser.ParseStatement(text);
		var postgreSql  = PostgreSqlParser.ParseStatement(text);

		// T-SQL keeps a column's qualifiers in its own Options, in the order written, and a key's columns as keys with their
		// directions; PostgreSQL builds the standard's slots, as the standard's parser does. The trees differ there, on
		// purpose, and the standard's is PostgreSQL's.
		Assert.NotEqual(transactSql, postgreSql);
		Assert.Equal(SqlStandardParser.ParseStatement(text), postgreSql);
	}

	// ── The writers ────────────────────────────────────────────────────────────

	static readonly QualifiedName T = new(new Identifier("t"));
	static readonly Identifier A = new("a");

	static Statement.CreateTable Table(TableElement element)
	{
		return new Statement.CreateTable { Name = T, Contents = new TableContents.Elements([element]) };
	}

	static TableElement.Column Int(Constraint? constraint = null, ColumnGeneration? generation = null)
	{
		return new TableElement.Column(new ColumnDefinition(A, new DataType.Numeric(NumericTypeKind.Int), generation, constraint is null ? [] : [constraint], null));
	}

	[Theory]
	[InlineData("unlogged")]
	[InlineData("temp word")]
	[InlineData("temporary scope")]
	[InlineData("on commit drop")]
	[InlineData("inherits")]
	[InlineData("partition of")]
	[InlineData("tablespace")]
	[InlineData("storage")]
	[InlineData("generation storage")]
	[InlineData("no inherit")]
	[InlineData("not valid")]
	[InlineData("null")]
	[InlineData("exclude")]
	[InlineData("include")]
	[InlineData("existing index")]
	[InlineData("delete columns")]
	[InlineData("like comments")]
	[InlineData("options without the words")]
	public void The_other_writers_refuse_what_only_PostgreSQL_reads(string kind)
	{
		Statement tree = kind switch
		{
			"unlogged"           => Table(Int()) with { Unlogged = true },
			"temp word"          => Table(Int()) with { Scope = TableScope.LocalTemporary, TempWord = true },
			"temporary scope"    => Table(Int()) with { Scope = TableScope.Temporary },
			"on commit drop"     => Table(Int()) with { Scope = TableScope.GlobalTemporary, OnCommit = TableCommitAction.Drop },
			"inherits"           => Table(Int()) with { Inherits = [T] },
			"partition of"       => new Statement.CreateTable { Name = T, Contents = new TableContents.Elements([]), PartitionOf = new PartitionParent(T, new PartitionBound.Default()) },
			"tablespace"         => Table(Int()) with { Tablespace = A },
			"storage"            => Table(new TableElement.Column(((TableElement.Column)Int()).Definition with { Storage = A })),
			"generation storage" => Table(Int(null, new ColumnGeneration.Generated(new LiteralValue.Numeric("1", NumericLiteralKind.DecimalInteger)) { Storage = GenerationStorage.Stored })),
			"no inherit"         => Table(Int(new Constraint.NotNull { NoInherit = true })),
			"not valid"          => Table(new TableElement.TableConstraint(new Constraint.Check(new Expression.Reference(new QualifiedName(A))) { NotValid = true })),
			"null"               => Table(Int(new Constraint.Null())),
			"exclude"            => Table(new TableElement.TableConstraint(new Constraint.Exclude(null, [new ExclusionElement(new IndexElement(new Expression.Reference(new QualifiedName(A))), new QualifiedName(new Identifier("=")))]))),
			"include"            => Table(new TableElement.TableConstraint(new Constraint.Unique(UniqueKind.Unique, null, [A]) { Include = [A] })),
			"existing index"     => Table(new TableElement.TableConstraint(new Constraint.Unique(UniqueKind.Unique, null, []) { ExistingIndex = A })),
			"delete columns"     => Table(Int(new Constraint.ForeignKey([], null, new ReferencesSpecification(T, [], null, null, null, ReferentialAction.SetNull) { OnDeleteColumns = [A] }))),
			"like comments"      => Table(new TableElement.Like(T, [LikeOption.IncludingComments])),
			_                    => new Statement.CreateTable { Name = T, Contents = new TableContents.Typed(T, null, [new TableElement.ColumnOptions(A, new ColumnOptionList(null, null, [])) { WithOptionsOmitted = true }]) },
		};

		Assert.ThrowsAny<ArgumentException>(() => SqlWriter.Write(tree));
		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(tree));

		var written = PostgreSqlWriter.Write(tree);

		Assert.True(PostgreSqlParser.TryParseStatement(written).IsSuccess, written);
	}

	[Theory]
	[InlineData("a deferrable check at the table")]
	[InlineData("a not valid column constraint")]
	[InlineData("a deferrable column check")]
	[InlineData("a key at the table with no columns")]
	[InlineData("a table not null with no column")]
	[InlineData("a column not null naming a column")]
	[InlineData("a partition of with a query")]
	[InlineData("a hash bound with one modulus")]
	[InlineData("T-SQL's kind")]
	[InlineData("a temp word with no scope")]
	[InlineData("unlogged and temporary")]
	[InlineData("delete columns after cascade")]
	public void The_PostgreSQL_writer_refuses_what_it_cannot_read_back(string kind)
	{
		var check = new Constraint.Check(new Expression.Reference(new QualifiedName(A)));
		Statement tree = kind switch
		{
			"a deferrable check at the table"   => Table(new TableElement.TableConstraint(check with { Characteristics = new ConstraintCharacteristics(null, true, null) })),
			"a not valid column constraint"     => Table(Int(check with { NotValid = true })),
			"a deferrable column check"         => Table(Int(check with { Characteristics = new ConstraintCharacteristics(null, true, null) })),
			"a key at the table with no columns" => Table(new TableElement.TableConstraint(new Constraint.Unique(UniqueKind.Unique, null, []))),
			"a table not null with no column"   => Table(new TableElement.TableConstraint(new Constraint.NotNull())),
			"a column not null naming a column" => Table(Int(new Constraint.NotNull { Column = A })),
			"a partition of with a query"       => new Statement.CreateTable { Name = T, Contents = new TableContents.AsQuery([], new Query.Select { Columns = [new SelectColumn.All()] }, null), PartitionOf = new PartitionParent(T, new PartitionBound.Default()) },
			"a hash bound with one modulus"     => new Statement.CreateTable { Name = T, Contents = new TableContents.Elements([]), PartitionOf = new PartitionParent(T, new PartitionBound.Hash([new Option(new Identifier("modulus"), new LiteralValue.Numeric("4", NumericLiteralKind.DecimalInteger), [])])) },
			"T-SQL's kind"                      => Table(Int()) with { Kind = TableKind.Node },
			"a temp word with no scope"         => Table(Int()) with { TempWord = true },
			"unlogged and temporary"            => Table(Int()) with { Unlogged = true, Scope = TableScope.Temporary },
			_                                   => Table(Int(new Constraint.ForeignKey([], null, new ReferencesSpecification(T, [], null, null, null, ReferentialAction.Cascade) { OnDeleteColumns = [A] }))),
		};

		Assert.ThrowsAny<ArgumentException>(() => PostgreSqlWriter.Write(tree));
	}
}
