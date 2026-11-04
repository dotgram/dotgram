using System;

using DotGram.Sql.Ast;
using DotGram.Sql.PostgreSql;
using DotGram.Sql.Standard;

using Xunit;

namespace DotGram.Sql.Tests;

using DataType = DotGram.Sql.Ast.DataType;
using Expression = DotGram.Sql.Ast.Expression;
using Query = DotGram.Sql.Ast.Query;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// The expressions, the data types and the core of a query as <see cref="PostgreSqlParser"/> reads them, held node
/// by node to what was written, and every text held to the round trip: written by <see cref="PostgreSqlWriter"/>,
/// read again, the same tree, and the same text again.
/// </summary>
public sealed class PostgreSqlExpressionTests
{
	/// <summary>
	/// The statement the text reads as, which must read back from its written text into an equal tree (spans aside),
	/// and write the same text again.
	/// </summary>
	static Statement Holds(string text)
	{
		var made    = PostgreSqlParser.Located.ParseStatement(text);
		var written = PostgreSqlWriter.Write(made);
		var back    = PostgreSqlParser.TryParseStatement(written);

		Assert.True(back.IsSuccess, $"{text}: written as {written}, which does not read back");
		Assert.True(made.Equals(back.Value), $"{text}: written as {written}, which reads back as {back.Value}");
		Assert.Equal(written, PostgreSqlWriter.Write(back.Value!));

		return made;
	}

	static Expression Value(string text)
	{
		Holds("SELECT " + text);

		return PostgreSqlParser.ParseExpression(text);
	}

	// ── Operators and precedence ───────────────────────────────────────────────

	[Theory]
	[InlineData("a + b * c",   "Binary(Add)")]
	[InlineData("a || b",      "Binary(Concatenate)")]
	[InlineData("a ^ b",       "Binary(Power)")]
	[InlineData("a @> b",      "OperatorCall(@>)")]
	[InlineData("a -> 'k'",    "OperatorCall(->)")]
	[InlineData("a OPERATOR(pg_catalog.+) b", "OperatorCall(pg_catalog.+)")]
	[InlineData("@ a",         "OperatorCall(@) prefix")]
	[InlineData("- a",         "Unary(Minus)")]
	[InlineData("a = b AND c", "Binary(And)")]
	[InlineData("NOT a = b",   "Unary(Not)")]
	[InlineData("a::int",      "Cast(::)")]
	[InlineData("a::int::text", "Cast(::) over Cast(::)")]
	[InlineData("a AT TIME ZONE 'UTC'", "AtTimeZone")]
	[InlineData("a AT LOCAL",  "AtTimeZone(local)")]
	[InlineData("a COLLATE \"C\"", "Collate")]
	// The binding powers are the server's: a prefix operator takes its operand at the operator level, `||` is
	// an operator like any other, and a sign binds tighter than AT TIME ZONE and COLLATE.
	[InlineData("|/ 25 + 11",  "OperatorCall prefix over Add")]
	[InlineData("a ~ b + c",   "OperatorCall over Add")]
	[InlineData("a || b + c",  "Concatenate over Add")]
	[InlineData("a ^ b ^ c",   "Power left-associative")]
	[InlineData("2 ^ - 3",     "Power over a sign")]
	[InlineData("- 2 ^ 3",     "Power under a sign")]
	[InlineData("-a COLLATE \"C\"", "Collate over a sign")]
	[InlineData("- a AT TIME ZONE 'x'", "AtTimeZone over a sign")]
	[InlineData("a AT TIME ZONE - 1", "AtTimeZone with a signed zone")]
	public void An_operator_builds_the_node_its_symbol_means(string text, string expected)
	{
		var value = Value(text);

		var holds = expected switch
		{
			"Binary(Add)"         => value is Expression.Binary { Operator: BinaryOperator.Add, Right: Expression.Binary { Operator: BinaryOperator.Multiply } },
			"Binary(Concatenate)" => value is Expression.Binary { Operator: BinaryOperator.Concatenate },
			"Binary(Power)"       => value is Expression.Binary { Operator: BinaryOperator.Power },
			"OperatorCall(@>)"    => value is Expression.OperatorCall { Operator: [{ Text: "@>" }], Left: not null, OperatorKeyword: false },
			"OperatorCall(->)"    => value is Expression.OperatorCall { Operator: [{ Text: "->" }] },
			"OperatorCall(pg_catalog.+)" => value is Expression.OperatorCall { Operator: [{ Text: "pg_catalog" }, { Text: "+" }], OperatorKeyword: true },
			"OperatorCall(@) prefix" => value is Expression.OperatorCall { Operator: [{ Text: "@" }], Left: null },
			"Unary(Minus)"        => value is Expression.Unary { Operator: UnaryOperator.Minus },
			"Binary(And)"         => value is Expression.Binary { Operator: BinaryOperator.And, Left: Expression.Comparison },
			"Unary(Not)"          => value is Expression.Unary { Operator: UnaryOperator.Not, Operand: Expression.Comparison },
			"Cast(::)"            => value is Expression.Cast { Spelling: CastSpelling.DoubleColon, Target: DataType.Numeric { Kind: NumericTypeKind.Int } },
			"Cast(::) over Cast(::)" => value is Expression.Cast { Spelling: CastSpelling.DoubleColon, Value: Expression.Cast { Spelling: CastSpelling.DoubleColon } },
			"AtTimeZone"          => value is Expression.AtTimeZone { Zone: not null },
			"AtTimeZone(local)"   => value is Expression.AtTimeZone { Zone: null },
			"Collate"             => value is Expression.Collate,
			"OperatorCall prefix over Add" => value is Expression.OperatorCall { Left: null, Right: Expression.Binary { Operator: BinaryOperator.Add } },
			"OperatorCall over Add" => value is Expression.OperatorCall { Left: Expression.Reference, Right: Expression.Binary { Operator: BinaryOperator.Add } },
			"Concatenate over Add" => value is Expression.Binary { Operator: BinaryOperator.Concatenate, Right: Expression.Binary { Operator: BinaryOperator.Add } },
			"Power left-associative" => value is Expression.Binary { Operator: BinaryOperator.Power, Left: Expression.Binary { Operator: BinaryOperator.Power } },
			"Power over a sign"   => value is Expression.Binary { Operator: BinaryOperator.Power, Right: Expression.Unary { Operator: UnaryOperator.Minus } },
			"Power under a sign"  => value is Expression.Binary { Operator: BinaryOperator.Power, Left: Expression.Unary { Operator: UnaryOperator.Minus } },
			"Collate over a sign" => value is Expression.Collate { Value: Expression.Unary },
			"AtTimeZone over a sign" => value is Expression.AtTimeZone { Value: Expression.Unary },
			"AtTimeZone with a signed zone" => value is Expression.AtTimeZone { Zone: Expression.Unary },
			_ => throw new ArgumentOutOfRangeException(nameof(expected)),
		};

		Assert.True(holds, $"{text}: {value}");
	}

	/// <summary>
	/// The IS level and the comparisons interleave as bison reads them: tests stack and stand on either side of a
	/// comparison, a comparison's right operand may be NOT and what it negates, and IS DISTINCT FROM takes its right
	/// operand up to a comparison.
	/// </summary>
	[Theory]
	[InlineData("a IS NULL IS NULL",         "IsNull over IsNull")]
	[InlineData("1 IS NULL = false",         "Comparison over IsNull on the left")]
	[InlineData("a = b IS NULL",             "IsNull over Comparison")]
	[InlineData("a = b IS NULL = c",         "Comparison over IsNull over Comparison")]
	[InlineData("true = NOT false",          "Comparison over Not on the right")]
	[InlineData("NOT a IS NULL",             "Not over IsNull")]
	[InlineData("a IS DISTINCT FROM b = c",  "IsDistinct over Comparison on the right")]
	[InlineData("a IS DISTINCT FROM NOT b",  "IsDistinct over Not on the right")]
	[InlineData("a IS NULL IS DISTINCT FROM b", "IsDistinct over IsNull on the left")]
	public void A_test_and_a_comparison_stand_on_each_other_as_the_server_reads_them(string text, string expected)
	{
		var value = Value(text);

		var holds = expected switch
		{
			"IsNull over IsNull"   => value is Expression.IsNull { Value: Expression.IsNull },
			"Comparison over IsNull on the left" => value is Expression.Comparison { Left: Expression.IsNull, Right: LiteralValue.Boolean },
			"IsNull over Comparison" => value is Expression.IsNull { Value: Expression.Comparison },
			"Comparison over IsNull over Comparison" => value is Expression.Comparison { Left: Expression.IsNull { Value: Expression.Comparison } },
			"Comparison over Not on the right" => value is Expression.Comparison { Right: Expression.Unary { Operator: UnaryOperator.Not } },
			"Not over IsNull"      => value is Expression.Unary { Operator: UnaryOperator.Not, Operand: Expression.IsNull },
			"IsDistinct over Comparison on the right" => value is Expression.IsDistinct { Right: Expression.Comparison },
			"IsDistinct over Not on the right" => value is Expression.IsDistinct { Right: Expression.Unary { Operator: UnaryOperator.Not } },
			"IsDistinct over IsNull on the left" => value is Expression.IsDistinct { Left: Expression.IsNull },
			_ => throw new ArgumentOutOfRangeException(nameof(expected)),
		};

		Assert.True(holds, $"{text}: {value}");
	}

	/// <summary>
	/// What the server refuses is refused: the comparison level is non-associative, IS DISTINCT FROM ends its level, an
	/// array mixes no values with arrays, a quantified form takes no list, an indirection follows no call, literal, array,
	/// row or CASE, a typed literal's call holds plain arguments and at least one, NULLIF takes two values, UNIQUE is not
	/// implemented, EXTRACT's field is no key word, an integer must fit 32 bits, and a subscript is not empty.
	/// </summary>
	[Theory]
	[InlineData("a < b < c")]
	[InlineData("a = b = c")]
	[InlineData("a = ")]
	[InlineData("a IS DISTINCT FROM b IS NULL")]
	[InlineData("a IS DISTINCT FROM b IS DISTINCT FROM c")]
	[InlineData("ARRAY[1, [2]]")]
	[InlineData("a = ANY (1, 2)")]
	[InlineData("f(x).y")]
	[InlineData("f(x)[1]")]
	[InlineData("ARRAY[1, 2][1]")]
	[InlineData("'abc'[1]")]
	[InlineData("ROW(1, 2).f")]
	[InlineData("CASE a WHEN 1 THEN 2 END[1]")]
	[InlineData("f(DISTINCT a ORDER BY b) 'x'")]
	[InlineData("f(a := 1) 'x'")]
	[InlineData("f(VARIADIC a) 'x'")]
	[InlineData("f(*) 'x'")]
	[InlineData("f() 'x'")]
	[InlineData("numeric '1' 'x'")]
	[InlineData("NULLIF(a)")]
	[InlineData("NULLIF(a, b, c)")]
	[InlineData("UNIQUE (SELECT 1)")]
	[InlineData("EXTRACT(true FROM x)")]
	[InlineData("EXTRACT(name FROM x)")]
	[InlineData("POSITION(a IN b IN c)")]
	[InlineData("x::int[99999999999]")]
	[InlineData("x::int[2147483648]")]
	[InlineData("varchar(99999999999) 'x'")]
	[InlineData("interval(99999999999) '1'")]
	[InlineData("a[]")]
	public void What_the_server_refuses_is_refused(string text)
	{
		Assert.False(PostgreSqlParser.TryParseExpression(text).IsSuccess);
	}

	// ── Predicates and tests ───────────────────────────────────────────────────

	[Theory]
	[InlineData("a IS NULL",               "IsNull")]
	[InlineData("a NOTNULL",               "IsNull postfix not")]
	[InlineData("a IS NOT TRUE",           "IsTruth")]
	[InlineData("a IS DISTINCT FROM b",    "IsDistinct")]
	[InlineData("a IS NOT DOCUMENT",       "IsDocument")]
	[InlineData("a IS NFC NORMALIZED",     "IsNormalized")]
	[InlineData("a BETWEEN SYMMETRIC 1 AND 2 AND c", "Between under And")]
	[InlineData("a NOT IN (1, 2)",         "In values")]
	[InlineData("a IN (SELECT b FROM t)",  "In subquery")]
	[InlineData("a ILIKE 'x%' ESCAPE '!'", "Like ILike")]
	[InlineData("a NOT SIMILAR TO 'x'",    "Like Similar not")]
	[InlineData("a = ANY (SELECT b FROM t)", "QuantifiedComparison")]
	[InlineData("a = ANY (b)",             "QuantifiedOperation comparison")]
	[InlineData("a LIKE ALL (SELECT b FROM t)", "QuantifiedOperation like")]
	[InlineData("a @> SOME (b)",           "QuantifiedOperation named")]
	[InlineData("EXISTS (SELECT 1)",       "Exists")]
	public void A_predicate_builds_its_record(string text, string expected)
	{
		var value = Value(text);

		var holds = expected switch
		{
			"IsNull"               => value is Expression.IsNull { Not: false, Postfix: false },
			"IsNull postfix not"   => value is Expression.IsNull { Not: true, Postfix: true },
			"IsTruth"              => value is Expression.IsTruth { Not: true, Truth: BooleanLiteral.True },
			"IsDistinct"           => value is Expression.IsDistinct { Not: false },
			"IsDocument"           => value is Expression.IsDocument { Not: true },
			"IsNormalized"         => value is Expression.IsNormalized { Form: NormalForm.NFC },
			"Between under And"    => value is Expression.Binary { Operator: BinaryOperator.And, Left: Expression.Between { Symmetry: BetweenSymmetry.Symmetric } },
			"In values"            => value is Expression.In { Not: true, SourceValue: InSource.Values { Items.Count: 2 } },
			"In subquery"          => value is Expression.In { SourceValue: InSource.Subquery },
			"Like ILike"           => value is Expression.Like { Kind: LikeKind.ILike, Escape: not null },
			"Like Similar not"     => value is Expression.Like { Kind: LikeKind.Similar, Not: true },
			"QuantifiedComparison" => value is Expression.QuantifiedComparison { Quantifier: Quantifier.Any },
			"QuantifiedOperation comparison" => value is Expression.QuantifiedOperation { Operator: QuantifiedOperator.Comparison, Right: Expression.Reference },
			"QuantifiedOperation like" => value is Expression.QuantifiedOperation { Operator: QuantifiedOperator.Like { Kind: LikeKind.Like }, Quantifier: Quantifier.All, Right: Expression.Subquery },
			"QuantifiedOperation named" => value is Expression.QuantifiedOperation { Operator: QuantifiedOperator.Named { Operator: [{ Text: "@>" }] }, Quantifier: Quantifier.Some },
			"Exists"               => value is Expression.Exists,
			_ => throw new ArgumentOutOfRangeException(nameof(expected)),
		};

		Assert.True(holds, $"{text}: {value}");
	}

	// ── Primaries, functions and typed literals ────────────────────────────────

	[Theory]
	[InlineData("f(a, b)",                      "Invocation")]
	[InlineData("s.f(a => 1, b := 2)",          "Invocation named")]
	[InlineData("count(*)",                     "Invocation star")]
	[InlineData("count(DISTINCT a ORDER BY b)", "Invocation distinct order")]
	[InlineData("f(VARIADIC a)",                "Invocation variadic")]
	[InlineData("sum(a) FILTER (WHERE b) OVER (PARTITION BY c ORDER BY d)", "Invocation clothed")]
	[InlineData("rank(1) WITHIN GROUP (ORDER BY a) OVER w", "Invocation within over name")]
	[InlineData("point '(1,2)'",                "typed literal")]
	[InlineData("numeric(10, 2) '1.5'",         "typed literal core numeric")]
	[InlineData("mytype(1, 2) 'x'",             "typed literal with modifiers")]
	[InlineData("int4 '5'",                     "typed literal core int4")]
	[InlineData("INT '5'",                      "typed literal core int")]
	[InlineData("DATE '2024-01-01'",            "DateTime literal")]
	[InlineData("INTERVAL '1 day'",             "Interval literal no qualifier")]
	[InlineData("INTERVAL '1' YEAR TO MONTH",   "Interval literal qualified")]
	[InlineData("CAST(a AS int)",               "Cast")]
	[InlineData("EXTRACT(epoch FROM a)",        "Extract named field")]
	[InlineData("EXTRACT(YEAR FROM a)",         "Extract")]
	[InlineData("SUBSTRING(a FOR 2 FROM 1)",    "Substring for first")]
	[InlineData("SUBSTRING(a, 1, 2)",           "Substring as a call")]
	[InlineData("TRIM(BOTH 'x' FROM a)",        "Trim")]
	[InlineData("TRIM(a, 'x')",                 "Trim as a call")]
	[InlineData("COALESCE(a, b)",               "Invocation coalesce")]
	[InlineData("CURRENT_TIMESTAMP(3)",         "Current")]
	[InlineData("CASE a WHEN 1 THEN 2 ELSE 3 END", "Case")]
	[InlineData("ARRAY[1, 2]",                  "Array")]
	[InlineData("ARRAY(SELECT 1)",              "CollectionQuery")]
	[InlineData("ROW(1, 2)",                    "Row keyword")]
	[InlineData("(1, 2)",                       "Row")]
	[InlineData("(a)",                          "Parenthesized")]
	[InlineData("(SELECT 1)",                   "Subquery")]
	[InlineData("((SELECT 1))",                 "Parenthesized subquery")]
	[InlineData("a[1]",                         "Element")]
	[InlineData("a[1:2]",                       "Element slice")]
	[InlineData("a[:2]",                        "Element slice open low")]
	[InlineData("(a).f",                        "Member")]
	[InlineData("(a).*",                        "Wildcard")]
	[InlineData("a.b[1].c",                     "Member over Element over Reference")]
	[InlineData("$1::int",                      "Cast over Parameter")]
	[InlineData("$1[1]",                        "Element over Parameter")]
	[InlineData("(SELECT 1)[1]",                "Element over Subquery")]
	[InlineData("a[1:2][3]",                    "Element over Element slice")]
	[InlineData("a[:]",                         "Element slice open both")]
	[InlineData("1 IN ((SELECT 1), 2)",         "In values with a subquery")]
	[InlineData("1 IN ((SELECT 1))",            "In subquery bracketed")]
	[InlineData("a = ANY ((SELECT 1) || b)",    "QuantifiedOperation over Concatenate")]
	[InlineData("a = ANY ((1, 2))",             "QuantifiedOperation over Row")]
	[InlineData("((SELECT 1) UNION (SELECT 2)) + 1", "Subquery with set operations")]
	[InlineData("1 IN ((SELECT 1) INTERSECT (SELECT 2) UNION (SELECT 3))", "In subquery with set operations")]
	[InlineData("date $$2024-01-01$$",          "typed literal dollar date")]
	[InlineData("int4 E'1'",                    "typed literal escape string")]
	[InlineData("numeric(10, 2) U&'1'",         "typed literal unicode string")]
	[InlineData("timestamp(3) E'x'",            "typed literal escape string timestamp")]
	[InlineData("INTERVAL E'1' DAY",            "typed literal escape string interval")]
	[InlineData("x::int[0x10]",                 "Array bound in hex")]
	[InlineData("numeric(0x3) '1'",             "typed literal hex precision")]
	[InlineData("numeric(2147483648) '1'",      "typed literal numeric by name")]
	[InlineData("numeric(1.5) '1'",             "typed literal numeric by name")]
	[InlineData("EXTRACT(\"year\" FROM a)",     "Extract quoted field")]
	[InlineData("EXTRACT('year' FROM a)",       "Extract string field")]
	[InlineData("EXTRACT(E'year' FROM a)",      "Extract escape string field")]
	[InlineData("CURRENT_SCHEMA()",             "Invocation current_schema")]
	[InlineData("CURRENT_SCHEMA",               "Current schema")]
	[InlineData("POSITION(a = b IN c)",         "Position over Comparison")]
	public void A_primary_builds_its_record(string text, string expected)
	{
		var value = Value(text);

		var holds = expected switch
		{
			"Invocation"                 => value is Expression.Invocation { Arguments.Count: 2, Name: [{ Text: "f" }] },
			"Invocation named"           => value is Expression.Invocation { Name.Count: 2, Arguments: [{ Name.Text: "a", NamedAssignment: true, ColonEquals: false }, { Name.Text: "b", NamedAssignment: true, ColonEquals: true }] },
			"Invocation star"            => value is Expression.Invocation { Arguments: [{ Value: Expression.Asterisk }] },
			"Invocation distinct order"  => value is Expression.Invocation { Quantifier: SetQuantifier.Distinct, OrderBy: not null },
			"Invocation variadic"        => value is Expression.Invocation { Arguments: [{ Variadic: true }] },
			"Invocation clothed"         => value is Expression.Invocation { Filter: not null, Over: WindowReference.Specification { Value: { PartitionBy.Count: 1, OrderBy: not null } } },
			"Invocation within over name" => value is Expression.Invocation { WithinGroup: not null, Over: WindowReference.NameRef },
			"typed literal"              => value is Expression.Cast { Spelling: CastSpelling.TypedLiteral, Target: DataType.Named { Name: [{ Text: "point" }], Arguments.Count: 0 }, Value: LiteralValue.String },
			"typed literal with modifiers" => value is Expression.Cast { Spelling: CastSpelling.TypedLiteral, Target: DataType.Named { Arguments.Count: 2 } },
			"typed literal core numeric" => value is Expression.Cast { Spelling: CastSpelling.TypedLiteral, Target: DataType.Numeric { Kind: NumericTypeKind.Numeric, Precision: 10, Scale: 2 } },
			"typed literal core int4"    => value is Expression.Cast { Spelling: CastSpelling.TypedLiteral, Target: DataType.Numeric { Kind: NumericTypeKind.Int4 } },
			"typed literal core int"     => value is Expression.Cast { Spelling: CastSpelling.TypedLiteral, Target: DataType.Numeric { Kind: NumericTypeKind.Int } },
			"DateTime literal"           => value is LiteralValue.DateTime { Kind: DateTimeLiteralKind.Date, Text: "'2024-01-01'" },
			"Interval literal no qualifier" => value is LiteralValue.Interval { Qualifier: null, Text: "'1 day'" },
			"Interval literal qualified" => value is LiteralValue.Interval { Qualifier: { Start: DateTimeField.Year, End: DateTimeField.Month } },
			"Cast"                       => value is Expression.Cast { Spelling: null, Target: DataType.Numeric { Kind: NumericTypeKind.Int } },
			"Extract named field"        => value is Expression.Extract { Field: null, NamedField: Expression.Reference },
			"Extract"                    => value is Expression.Extract { Field: ExtractField.Year, NamedField: null },
			"Substring for first"        => value is Expression.Substring { ForFirst: true, From: not null, For: not null },
			"Substring as a call"        => value is Expression.Invocation { Name: [{ Text: "SUBSTRING" }], Arguments.Count: 3 },
			"Trim"                       => value is Expression.Trim { Specification: TrimSpecification.Both, Character: not null, FromKeyword: true },
			"Trim as a call"             => value is Expression.Invocation { Name: [{ Text: "TRIM" }], Arguments.Count: 2 },
			"Invocation coalesce"        => value is Expression.Invocation { Name: [{ Text: "COALESCE" }] },
			"Current"                    => value is Expression.Current { Kind: CurrentValue.Timestamp, Precision: not null },
			"Case"                       => value is Expression.Case { Operand: not null, Whens.Count: 1, Else: not null },
			"Array"                      => value is Expression.Array { Items.Count: 2 },
			"CollectionQuery"            => value is Expression.CollectionQuery { Kind: CollectionKind.Array },
			"Row keyword"                => value is Expression.Row { RowKeyword: true, Items.Count: 2 },
			"Row"                        => value is Expression.Row { RowKeyword: false, Items.Count: 2 },
			"Parenthesized"              => value is Expression.Parenthesized { Value: Expression.Reference },
			"Subquery"                   => value is Expression.Subquery { Query: Query.Select },
			"Parenthesized subquery"     => value is Expression.Subquery { Query: Query.Parenthesized },
			"Element"                    => value is Expression.Element { Index: not null, To: null, Colon: false },
			"Element slice"              => value is Expression.Element { Index: not null, To: not null, Colon: true },
			"Element slice open low"     => value is Expression.Element { Index: null, To: not null, Colon: true },
			"Member"                     => value is Expression.Member { Kind: MemberAccessKind.Dot, Target: Expression.Parenthesized },
			"Wildcard"                   => value is Expression.Wildcard { Kind: WildcardKind.Member },
			"Member over Element over Reference" => value is Expression.Member { Target: Expression.Element { Collection: Expression.Reference { Name.Count: 2 } } },
			"Cast over Parameter"        => value is Expression.Cast { Value: Expression.Parameter { Ordinal: 1 } },
			"Element over Parameter"     => value is Expression.Element { Collection: Expression.Parameter },
			"Element over Subquery"      => value is Expression.Element { Collection: Expression.Subquery },
			"Element over Element slice" => value is Expression.Element { Colon: false, Collection: Expression.Element { Colon: true } },
			"Element slice open both"    => value is Expression.Element { Index: null, To: null, Colon: true },
			"In values with a subquery"  => value is Expression.In { SourceValue: InSource.Values { Items: [Expression.Subquery, LiteralValue.Numeric] } },
			"In subquery bracketed"      => value is Expression.In { SourceValue: InSource.Subquery { Query: Query.Parenthesized } },
			"QuantifiedOperation over Concatenate" => value is Expression.QuantifiedOperation { Right: Expression.Binary { Operator: BinaryOperator.Concatenate, Left: Expression.Subquery } },
			"QuantifiedOperation over Row" => value is Expression.QuantifiedOperation { Right: Expression.Row { RowKeyword: false } },
			"Subquery with set operations" => value is Expression.Binary { Left: Expression.Subquery { Query: Query.Parenthesized { SetOperations: [{ Operator: SetOperator.Union, Query: Query.Parenthesized }] } } },
			"In subquery with set operations" => value is Expression.In { SourceValue: InSource.Subquery { Query: Query.Parenthesized { SetOperations: [{ Operator: SetOperator.Intersect }, { Operator: SetOperator.Union }] } } },
			"typed literal dollar date"  => value is Expression.Cast { Spelling: CastSpelling.TypedLiteral, Target: DataType.DateTime { Kind: DateTimeTypeKind.Date }, Value: LiteralValue.String { Kind: StringLiteralKind.Dollar } },
			"typed literal escape string" => value is Expression.Cast { Spelling: CastSpelling.TypedLiteral, Target: DataType.Numeric { Kind: NumericTypeKind.Int4 }, Value: LiteralValue.String { Kind: StringLiteralKind.Escaped } },
			"typed literal unicode string" => value is Expression.Cast { Spelling: CastSpelling.TypedLiteral, Target: DataType.Numeric { Precision: 10, Scale: 2 }, Value: LiteralValue.String { Kind: StringLiteralKind.Unicode } },
			"typed literal escape string timestamp" => value is Expression.Cast { Spelling: CastSpelling.TypedLiteral, Target: DataType.DateTime { Kind: DateTimeTypeKind.Timestamp, Precision: 3 }, Value: LiteralValue.String { Kind: StringLiteralKind.Escaped } },
			"typed literal escape string interval" => value is Expression.Cast { Spelling: CastSpelling.TypedLiteral, Target: DataType.Interval { Qualifier.Start: DateTimeField.Day }, Value: LiteralValue.String { Kind: StringLiteralKind.Escaped } },
			"Array bound in hex"         => value is Expression.Cast { Target: DataType.Array { MaximumCardinality: 16, Brackets: true } },
			"typed literal hex precision" => value is Expression.Cast { Target: DataType.Numeric { Precision: 3 } },
			"typed literal numeric by name" => value is Expression.Cast { Target: DataType.Named { Name: [{ Text: "numeric" }], Arguments.Count: 1 } },
			"Extract quoted field"       => value is Expression.Extract { Field: null, NamedField: Expression.Reference { Name: [{ Style: IdentifierStyle.Delimited }] } },
			"Extract string field"       => value is Expression.Extract { Field: null, NamedField: LiteralValue.String { Kind: StringLiteralKind.Character } },
			"Extract escape string field" => value is Expression.Extract { Field: null, NamedField: LiteralValue.String { Kind: StringLiteralKind.Escaped } },
			"Invocation current_schema"  => value is Expression.Invocation { Arguments.Count: 0 },
			"Current schema"             => value is Expression.Current { Kind: CurrentValue.Schema },
			"Position over Comparison"   => value is Expression.Position { Value: Expression.Comparison },
			_ => throw new ArgumentOutOfRangeException(nameof(expected)),
		};

		Assert.True(holds, $"{text}: {value}");
	}

	// ── Data types ─────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("int",                        "Numeric Int")]
	[InlineData("int8",                       "Numeric Int8")]
	[InlineData("float8",                     "Numeric Float8")]
	[InlineData("numeric(10, 2)",             "Numeric with scale")]
	[InlineData("double precision",           "DoublePrecision")]
	[InlineData("bool",                       "Boolean abbreviated")]
	[InlineData("varchar(10)",                "Character varchar")]
	[InlineData("character varying",          "Character varying")]
	[InlineData("timestamptz(3)",             "DateTime abbreviated")]
	[InlineData("timestamp with time zone",   "DateTime with")]
	[InlineData("time(2) without time zone",  "DateTime without")]
	[InlineData("interval",                   "Interval bare")]
	[InlineData("interval(3)",                "Interval precision")]
	[InlineData("interval day to second(2)",  "Interval qualified")]
	[InlineData("bit varying(8)",             "BitString")]
	[InlineData("text",                       "Named")]
	[InlineData("pg_catalog.int4",            "Named qualified")]
	[InlineData("\"int4\"",                   "Named quoted")]
	[InlineData("int[]",                      "Array brackets")]
	[InlineData("int[3][]",                   "Array nested")]
	[InlineData("int ARRAY[3]",               "Array standard")]
	public void A_type_builds_its_record(string text, string expected)
	{
		var type = Assert.IsType<Expression.Cast>(Value("a::" + text)).Target;

		var holds = expected switch
		{
			"Numeric Int"         => type is DataType.Numeric { Kind: NumericTypeKind.Int },
			"Numeric Int8"        => type is DataType.Numeric { Kind: NumericTypeKind.Int8 },
			"Numeric Float8"      => type is DataType.Numeric { Kind: NumericTypeKind.Float8 },
			"Numeric with scale"  => type is DataType.Numeric { Kind: NumericTypeKind.Numeric, Precision: 10, Scale: 2 },
			"DoublePrecision"     => type is DataType.Numeric { Kind: NumericTypeKind.DoublePrecision },
			"Boolean abbreviated" => type is DataType.Boolean { Abbreviated: true },
			"Character varchar"   => type is DataType.Character { Kind: CharacterTypeKind.Varchar, Length: 10 },
			"Character varying"   => type is DataType.Character { Kind: CharacterTypeKind.CharacterVarying },
			"DateTime abbreviated" => type is DataType.DateTime { Kind: DateTimeTypeKind.Timestamp, Precision: 3, TimeZone: TimeZoneMode.With, Abbreviated: true },
			"DateTime with"       => type is DataType.DateTime { Kind: DateTimeTypeKind.Timestamp, TimeZone: TimeZoneMode.With, Abbreviated: false },
			"DateTime without"    => type is DataType.DateTime { Kind: DateTimeTypeKind.Time, Precision: 2, TimeZone: TimeZoneMode.Without },
			"Interval bare"       => type is DataType.Interval { Qualifier: null, Precision: null },
			"Interval precision"  => type is DataType.Interval { Qualifier: null, Precision: 3 },
			"Interval qualified"  => type is DataType.Interval { Qualifier: { Start: DateTimeField.Day, End: DateTimeField.Second, FractionalPrecision: 2 } },
			"BitString"           => type is DataType.BitString { Kind: BitStringTypeKind.BitVarying, Length: 8 },
			"Named"               => type is DataType.Named { Name: [{ Text: "text" }] },
			"Named qualified"     => type is DataType.Named { Name.Count: 2 },
			"Named quoted"        => type is DataType.Named { Name: [{ Style: IdentifierStyle.Delimited }] },
			"Array brackets"      => type is DataType.Array { Brackets: true, MaximumCardinality: null, ElementType: DataType.Numeric },
			"Array nested"        => type is DataType.Array { Brackets: true, MaximumCardinality: null, ElementType: DataType.Array { MaximumCardinality: 3 } },
			"Array standard"      => type is DataType.Array { Brackets: false, MaximumCardinality: 3 },
			_ => throw new ArgumentOutOfRangeException(nameof(expected)),
		};

		Assert.True(holds, $"{text}: {type}");
	}

	// ── The query core ─────────────────────────────────────────────────────────

	[Theory]
	[InlineData("SELECT",                                         "empty")]
	[InlineData("SELECT DISTINCT ON (a, b) c FROM t",             "distinct on")]
	[InlineData("SELECT a AS x, b y, c, * FROM t",                "labels")]
	[InlineData("SELECT 1 year",                                  "refused: a label that needs AS")]
	[InlineData("SELECT * FROM ONLY t AS a (c, d)",               "only and alias")]
	[InlineData("SELECT * FROM ONLY (t) a",                        "only bracketed")]
	[InlineData("SELECT * FROM (a JOIN b ON x) AS j",              "bracketed join alias")]
	[InlineData("SELECT * FROM t *",                              "inheritance star")]
	[InlineData("SELECT * FROM a NATURAL LEFT OUTER JOIN b",      "natural left outer")]
	[InlineData("SELECT * FROM a JOIN b USING (x, y) AS j",       "using alias")]
	[InlineData("SELECT * FROM a CROSS JOIN b, c",                "cross and comma")]
	[InlineData("SELECT * FROM (a JOIN b ON a.x = b.x) JOIN c ON c.x = a.x", "bracketed join")]
	[InlineData("SELECT * FROM LATERAL (SELECT 1) s",             "lateral subquery")]
	[InlineData("SELECT * FROM (SELECT 1) AS s WHERE s.x > 1",    "subquery where")]
	[InlineData("SELECT 1 UNION ALL SELECT 2 INTERSECT SELECT 3", "intersect tighter")]
	[InlineData("(SELECT 1) UNION (SELECT 2)",                    "bracketed operands")]
	[InlineData("SELECT (SELECT 1) FROM t",                       "subquery in the list")]
	[InlineData("SELECT ALL FROM t",                              "all and no targets")]
	// What the server refuses: a bracketed source that is no join, a join with neither NATURAL nor its condition or
	// with both, OUTER with no direction, ONLY with the inheritance star, DISTINCT with no target list.
	[InlineData("SELECT * FROM (a)",                              "refused: a bracketed name")]
	[InlineData("SELECT * FROM ((SELECT 1) x)",                   "refused: a bracketed subquery source")]
	[InlineData("SELECT * FROM t JOIN u",                         "refused: a join without its condition")]
	[InlineData("SELECT * FROM t NATURAL JOIN u ON true",         "refused: NATURAL with a condition")]
	[InlineData("SELECT * FROM t CROSS JOIN u ON true",           "refused: CROSS JOIN with a condition")]
	[InlineData("SELECT * FROM t OUTER JOIN u ON true",           "refused: OUTER without a direction")]
	[InlineData("SELECT * FROM t INNER OUTER JOIN u ON true",     "refused: INNER OUTER")]
	[InlineData("SELECT * FROM ONLY t *",                         "refused: ONLY with the star")]
	[InlineData("SELECT DISTINCT FROM t",                         "refused: DISTINCT without targets")]
	[InlineData("SELECT DISTINCT ON (a) FROM t",                  "refused: DISTINCT ON without targets")]
	public void A_query_builds_its_record(string text, string expected)
	{
		if (expected.StartsWith("refused", StringComparison.Ordinal))
		{
			Assert.False(PostgreSqlParser.TryParseStatement(text).IsSuccess);
			return;
		}

		var query = Assert.IsType<Statement.Select>(Holds(text)).Query;

		var holds = expected switch
		{
			"empty"                 => query is Query.Select { Columns.Count: 0, From: null },
			"distinct on"           => query is Query.Select { Quantifier: SetQuantifier.Distinct, DistinctOn.Count: 2, Columns.Count: 1 },
			"labels"                => query is Query.Select { Columns: [SelectColumn.ExpressionItem { Alias.Text: "x", AsKeyword: true }, SelectColumn.ExpressionItem { Alias.Text: "y", AsKeyword: false }, SelectColumn.ExpressionItem { Alias: null }, SelectColumn.All] },
			"only and alias"        => query is Query.Select { From.Sources: [TableSource.Named { Only: true, Alias: { Name.Text: "a", Columns.Count: 2, AsKeyword: true } }] },
			"only bracketed"        => query is Query.Select { From.Sources: [TableSource.Named { Only: true, Alias.Name.Text: "a" }] },
			"bracketed join alias"  => query is Query.Select { From.Sources: [TableSource.Parenthesized { Source: TableSource.Join, Alias.Name.Text: "j" }] },
			"inheritance star"      => query is Query.Select { From.Sources: [TableSource.Named { Star: true }] },
			"natural left outer"    => query is Query.Select { From.Sources: [TableSource.Join { Natural: true, Kind: JoinKind.Left, OuterKeyword: true, Specification: null }] },
			"using alias"           => query is Query.Select { From.Sources: [TableSource.Join { Specification: JoinSpecification.Using { Columns.Count: 2, Correlation.Text: "j" } }] },
			"cross and comma"       => query is Query.Select { From.Sources: [TableSource.Join { Kind: JoinKind.Cross }, TableSource.Named] },
			"bracketed join"        => query is Query.Select { From.Sources: [TableSource.Join { Left: TableSource.Parenthesized { Source: TableSource.Join }, Specification: JoinSpecification.On }] },
			"lateral subquery"      => query is Query.Select { From.Sources: [TableSource.Subquery { Lateral: true, Alias.Name.Text: "s" }] },
			"subquery where"        => query is Query.Select { From.Sources: [TableSource.Subquery { Lateral: false }], Where: Expression.Comparison },
			"intersect tighter"     => query is Query.Select { SetOperations: [{ Operator: SetOperator.UnionAll, Query: Query.Select { SetOperations: [{ Operator: SetOperator.Intersect }] } }] },
			"bracketed operands"    => query is Query.Parenthesized { SetOperations: [{ Operator: SetOperator.Union, Query: Query.Parenthesized }] },
			"subquery in the list"  => query is Query.Select { Columns: [SelectColumn.ExpressionItem { Expression: Expression.Subquery }] },
			"all and no targets"    => query is Query.Select { Quantifier: SetQuantifier.All, Columns.Count: 0, From: not null },
			_ => throw new ArgumentOutOfRangeException(nameof(expected)),
		};

		Assert.True(holds, $"{text}: {query}");
	}

	/// <summary>A text of statements holds empty ones too, as the server's stmtmulti does; one statement is one, with its semicolon or without.</summary>
	[Fact]
	public void A_text_of_statements_is_read_each_ended_by_a_semicolon_or_the_end()
	{
		var statements = PostgreSqlParser.ParseSql("SELECT 1; SELECT 2;\nSELECT 3");

		Assert.Equal(3, statements.Length);
		Assert.Empty(PostgreSqlParser.ParseSql(""));
		Assert.Empty(PostgreSqlParser.ParseSql(";;"));
		Assert.Single(PostgreSqlParser.ParseSql(";; SELECT 1;;"));
		Assert.True(PostgreSqlParser.TryParseStatement("SELECT 1;").IsSuccess);
		Assert.True(PostgreSqlParser.TryParseStatement("SELECT 1;;").IsSuccess);
		Assert.False(PostgreSqlParser.TryParseStatement("SELECT 1; SELECT 2").IsSuccess);
		Assert.False(PostgreSqlParser.TryParseStatement("").IsSuccess);
	}

	// ── Identity with the standard's reading ───────────────────────────────────

	/// <summary>A text both grammars read builds one tree: the standard's parser and PostgreSQL's agree, spans aside.</summary>
	[Theory]
	[InlineData("SELECT a FROM t WHERE b = 1")]
	[InlineData("SELECT a, b AS c FROM t AS u")]
	[InlineData("SELECT DISTINCT a FROM t")]
	[InlineData("SELECT * FROM a LEFT JOIN b ON a.x = b.x")]
	[InlineData("SELECT * FROM a JOIN b USING (x)")]
	[InlineData("SELECT 1 FROM t UNION ALL SELECT 2 FROM u")]
	[InlineData("SELECT 1 FROM t UNION SELECT 2 FROM u INTERSECT SELECT 3 FROM v")]
	[InlineData("SELECT (SELECT 1 FROM u) FROM t")]
	[InlineData("SELECT CAST(x AS INTEGER) FROM t")]
	[InlineData("SELECT DATE '2024-01-01' FROM t")]
	[InlineData("SELECT x IS DISTINCT FROM y FROM t")]
	[InlineData("SELECT x IS NOT NULL FROM t")]
	[InlineData("SELECT a + b * c FROM t")]
	[InlineData("SELECT a || b FROM t")]
	[InlineData("SELECT ARRAY[1, 2] FROM t")]
	[InlineData("SELECT ROW(1, 2) FROM t")]
	[InlineData("SELECT x = ANY (SELECT y FROM t) FROM u")]
	[InlineData("SELECT EXISTS (SELECT 1 FROM t) FROM u")]
	[InlineData("SELECT x IN (1, 2) FROM t")]
	[InlineData("SELECT x BETWEEN 1 AND 2 FROM t")]
	[InlineData("SELECT x LIKE 'a%' ESCAPE '!' FROM t")]
	[InlineData("SELECT CASE WHEN a THEN 1 ELSE 2 END FROM t")]
	[InlineData("SELECT EXTRACT(YEAR FROM d) FROM t")]
	[InlineData("SELECT SUBSTRING(s FROM 1 FOR 2) FROM t")]
	[InlineData("SELECT TRIM(LEADING 'x' FROM s) FROM t")]
	[InlineData("SELECT COUNT(*) FROM t")]
	[InlineData("SELECT SUM(DISTINCT a) FILTER (WHERE b) FROM t")]
	[InlineData("SELECT a FROM t WHERE NOT (a = 1 OR b = 2) AND c")]
	[InlineData("SELECT 0x1F, 1_000, U&\"a\" FROM t")]
	[InlineData("SELECT a FROM t WHERE a IN ((SELECT a FROM u))")]
	[InlineData("SELECT a FROM t WHERE NOT a IS NULL")]
	[InlineData("SELECT (a), ((a)), ((SELECT 1 FROM u)) FROM t")]
	[InlineData("SELECT a FROM t WHERE a = b OR c IS NULL AND NOT d")]
	public void A_text_both_grammars_read_is_one_tree(string text)
	{
		var standard   = SqlStandardParser.ParseStatement(text);
		var postgreSql = PostgreSqlParser.ParseStatement(text);

		Assert.True(standard.Equals(postgreSql), $"{text}:\n  the standard: {SqlWriter.Write(standard)}\n  PostgreSQL:   {PostgreSqlWriter.Write(postgreSql)}\n  {standard}\n  {postgreSql}");
	}

	/// <summary>Where the two readings differ by the dialect's meaning, they differ on purpose.</summary>
	[Theory]
	[InlineData("SELECT X'1F' FROM t")]
	public void A_text_the_dialects_read_differently_is_two_trees(string text)
	{
		Assert.NotEqual(SqlStandardParser.ParseStatement(text), PostgreSqlParser.ParseStatement(text));
		Assert.IsType<LiteralValue.BitString>(((Query.Select)((Statement.Select)PostgreSqlParser.ParseStatement(text)).Query).Columns[0] is SelectColumn.ExpressionItem item ? item.Expression : null);
	}

	// ── The doors ──────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("SELECT a + b FROM t WHERE c")]
	[InlineData("SELECT f(x) FILTER (WHERE y) FROM t")]
	public void The_located_door_reads_the_same_statement_and_locates_its_parts(string text)
	{
		var plain   = PostgreSqlParser.ParseStatement(text);
		var located = PostgreSqlParser.Located.ParseStatement(text);

		Assert.Equal(plain, located);
		Assert.True(located.Span.Known);
		Assert.True(((Statement.Select)located).Query.Span.Known);
	}
}
