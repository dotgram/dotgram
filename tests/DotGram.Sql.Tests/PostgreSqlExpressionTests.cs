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
	[InlineData("@ a ~ b",     "OperatorCall over a prefix OperatorCall")]
	[InlineData("a || !! b",   "Concatenate over a prefix OperatorCall on the right")]
	[InlineData("a + @ b * c", "Add over a prefix OperatorCall on the right")]
	[InlineData("a ~ @ b",     "OperatorCall over a prefix OperatorCall on the right")]
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
			"OperatorCall over a prefix OperatorCall" => value is Expression.OperatorCall { Operator: [{ Text: "~" }], Left: Expression.OperatorCall { Left: null, Right: Expression.Reference } },
			"Concatenate over a prefix OperatorCall on the right" => value is Expression.Binary { Operator: BinaryOperator.Concatenate, Right: Expression.OperatorCall { Left: null } },
			"Add over a prefix OperatorCall on the right" => value is Expression.Binary { Operator: BinaryOperator.Add, Right: Expression.OperatorCall { Left: null, Right: Expression.Binary { Operator: BinaryOperator.Multiply } } },
			"OperatorCall over a prefix OperatorCall on the right" => value is Expression.OperatorCall { Left: Expression.Reference, Right: Expression.OperatorCall { Left: null } },
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
	[InlineData("(a = NOT b) = c",           "Comparison over a bracketed comparison")]
	[InlineData("a = NOT b = c",             "Comparison over Not over Comparison")]
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
			"Comparison over a bracketed comparison" => value is Expression.Comparison { Left: Expression.Parenthesized { Value: Expression.Comparison { Right: Expression.Unary } }, Right: Expression.Reference },
			"Comparison over Not over Comparison" => value is Expression.Comparison { Left: Expression.Reference, Right: Expression.Unary { Operand: Expression.Comparison } },
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
	[InlineData("a OVERLAPS (1, 2)")]
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
	[InlineData("a + ANY (ARRAY[1])",      "QuantifiedOperation core named")]
	[InlineData("EXISTS (SELECT 1)",       "Exists")]
	[InlineData("(1, 2) OVERLAPS (3, 4)",  "Overlaps")]
	[InlineData("ROW(1, 2) OVERLAPS (3, 4)", "Overlaps row keyword")]
	[InlineData("a IN (-1, null::int, f(1))", "In values of any shape")]
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
			"QuantifiedOperation core named" => value is Expression.QuantifiedOperation { Operator: QuantifiedOperator.Named { Operator: [{ Text: "+" }] }, Right: Expression.Array },
			"Exists"               => value is Expression.Exists,
			"Overlaps"             => value is Expression.Overlaps { Left: Expression.Row { RowKeyword: false }, Right: Expression.Row },
			"Overlaps row keyword" => value is Expression.Overlaps { Left: Expression.Row { RowKeyword: true } },
			"In values of any shape" => value is Expression.In { SourceValue: InSource.Values { Items: [Expression.Unary, Expression.Cast, Expression.Invocation] } },
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
	[InlineData("timestamptz(99999999999) 'x'", "typed literal one-word type by name")]
	[InlineData("timetz(99999999999) 'x'",      "typed literal one-word type by name")]
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
			"typed literal one-word type by name" => value is Expression.Cast { Target: DataType.Named { Arguments.Count: 1 } },
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
	[InlineData("interval second(3)",         "Interval second fractional")]
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
			"Interval second fractional" => type is DataType.Interval { Qualifier: { Start: DateTimeField.Second, End: null, LeadingPrecision: null, FractionalPrecision: 3 } },
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
	[InlineData("SELECT * FROM ((a JOIN b ON true)) j",           "double bracket alias")]
	[InlineData("SELECT * FROM ((a JOIN b ON true))",             "double bracket")]
	// The clauses of the whole result.
	[InlineData("SELECT a FROM t ORDER BY a DESC NULLS LAST LIMIT 10 OFFSET 5", "limit offset")]
	[InlineData("SELECT a FROM t OFFSET 5 ROWS FETCH FIRST 10 ROWS ONLY", "offset fetch")]
	[InlineData("SELECT a FROM t OFFSET 5 LIMIT 10",               "offset limit")]
	[InlineData("SELECT a FROM t LIMIT ALL",                      "limit all")]
	[InlineData("SELECT a FROM t ORDER BY a FETCH NEXT 3 ROWS WITH TIES", "fetch with ties")]
	[InlineData("SELECT a FROM t FETCH FIRST ROW ONLY",           "fetch no count")]
	[InlineData("SELECT a FROM t FETCH FIRST -1 ROWS ONLY OFFSET (SELECT 1)", "fetch signed, offset primary")]
	[InlineData("SELECT a FROM t FOR UPDATE OF s.t NOWAIT",       "for update of nowait")]
	[InlineData("SELECT a FROM t FOR NO KEY UPDATE SKIP LOCKED FOR KEY SHARE", "two locks")]
	[InlineData("SELECT a FROM t FOR SHARE LIMIT 1",              "lock then limit")]
	[InlineData("SELECT a FROM t LIMIT 1 FOR SHARE",              "limit then lock")]
	[InlineData("SELECT a FROM t FOR READ ONLY",                  "for read only")]
	[InlineData("WITH RECURSIVE r (n) AS NOT MATERIALIZED (SELECT 1) SEARCH DEPTH FIRST BY n SET o CYCLE n SET c USING p SELECT * FROM r", "with recursive search cycle")]
	[InlineData("WITH x AS MATERIALIZED (SELECT 1) SELECT 1",     "with materialized")]
	[InlineData("(WITH x AS (SELECT 1) SELECT 2)",                "with in brackets")]
	[InlineData("(SELECT 1 ORDER BY 1 LIMIT 1) UNION (SELECT 2) ORDER BY 1 LIMIT 2", "bracketed operands with clauses")]
	[InlineData("(SELECT 1) ORDER BY 1",                          "bracketed query with order")]
	[InlineData("SELECT (SELECT a FROM t ORDER BY a LIMIT 1)",    "subquery with clauses")]
	[InlineData("SELECT a FROM t GROUP BY DISTINCT ROLLUP (a, (b, c)), CUBE (a), GROUPING SETS ((a), ()), (), a + 1 HAVING count(*) > 1", "group by families")]
	[InlineData("SELECT a FROM t GROUP BY (a, b)",                "group by row")]
	[InlineData("SELECT a FROM t WINDOW w AS (PARTITION BY a ORDER BY b ROWS BETWEEN UNBOUNDED PRECEDING AND 1 FOLLOWING EXCLUDE TIES), v AS (w RANGE CURRENT ROW)", "window clause")]
	[InlineData("SELECT sum(a) OVER (GROUPS 2 PRECEDING) FROM t", "frame groups")]
	[InlineData("VALUES (1, 2), (3, 4)",                          "values")]
	[InlineData("TABLE ONLY t",                                   "table only")]
	[InlineData("SELECT * FROM f(1) AS t (a int, b text)",        "function with definitions")]
	[InlineData("SELECT * FROM f(1) AS (a int)",                  "function with definitions alone")]
	[InlineData("SELECT * FROM f(1) t (a, b)",                    "function with column names")]
	[InlineData("SELECT * FROM LATERAL f(x) WITH ORDINALITY AS t", "lateral function with ordinality")]
	[InlineData("SELECT * FROM unnest(a, b) WITH ORDINALITY AS u (x, y)", "unnest")]
	[InlineData("SELECT * FROM LATERAL unnest(a)",                "lateral unnest")]
	[InlineData("SELECT * FROM s.unnest(a)",                      "qualified unnest is a function")]
	[InlineData("SELECT * FROM ROWS FROM (f(1), g(2) AS (a int)) WITH ORDINALITY AS r", "rows from")]
	[InlineData("SELECT * FROM t TABLESAMPLE SYSTEM (10) REPEATABLE (42)", "tablesample")]
	[InlineData("SELECT t.*, a.b.* FROM t",                        "qualified stars")]
	[InlineData("SELECT * FROM ((SELECT 1) x JOIN y ON true) z", "nested derived table")]
	[InlineData("SELECT * FROM (((SELECT 1) UNION (SELECT 2) ORDER BY 1 LIMIT 1) x JOIN y ON true) w", "nested set operation with clauses")]
	[InlineData("SELECT * FROM ((SELECT 1)) x",                  "doubly bracketed subquery")]
	[InlineData("SELECT * FROM ROWS FROM (f()) AS t (b int)",    "rows from with definitions")]
	[InlineData("SELECT * FROM ROWS FROM (f()) AS (b int)",      "rows from with definitions alone")]
	[InlineData("SELECT a FROM t FETCH FIRST rows ROWS ONLY",    "fetch a column named rows")]
	[InlineData("SELECT a FROM t GROUP BY ROLLUP(ROLLUP(a)), CUBE(cube(b))", "rollup over a call")]
	[InlineData("SELECT * FROM (SELECT a FROM t ORDER BY a FETCH FIRST 1 ROWS WITH TIES) s FOR UPDATE SKIP LOCKED", "ties inside, skip locked outside")]
	[InlineData("SELECT * FROM coalesce(b) AS c(d int), LATERAL nullif(1, 2) WITH ORDINALITY AS n(e, f)", "key word calls as tables")]
	[InlineData("SELECT * FROM a.b.c",                                     "three-part table")]
	[InlineData("SELECT * FROM a.b.c.d",                                   "refused: four-part table")]
	[InlineData("SELECT * FROM CAST(1 AS int) AS c",                       "refused: CAST as a table")]
	[InlineData("(SELECT 1 FOR UPDATE SKIP LOCKED) ORDER BY 1 FETCH FIRST 1 ROW WITH TIES", "refused: SKIP LOCKED in the brackets and WITH TIES after")]
	[InlineData("((SELECT 1 FOR UPDATE SKIP LOCKED)) ORDER BY 1 FETCH FIRST 1 ROW WITH TIES", "refused: SKIP LOCKED two brackets deep and WITH TIES after")]
	// What gram.y refuses in its actions, and what its lists refuse.
	[InlineData("(SELECT 1 ORDER BY 1) ORDER BY 1",               "refused: two ORDER BYs")]
	[InlineData("(SELECT 1 LIMIT 1) LIMIT 1",                     "refused: two LIMITs")]
	[InlineData("(SELECT 1 OFFSET 1) OFFSET 1",                   "refused: two OFFSETs")]
	[InlineData("WITH x AS (SELECT 1) (WITH y AS (SELECT 2) SELECT 3)", "refused: two WITHs")]
	[InlineData("SELECT a FROM t LIMIT 1, 2",                     "refused: LIMIT a, b")]
	[InlineData("SELECT a FROM t FETCH FIRST 3 ROWS WITH TIES",   "refused: WITH TIES without ORDER BY")]
	[InlineData("SELECT a FROM t FETCH FIRST a + 1 ROWS ONLY",    "refused: FETCH count that is no primary")]
	[InlineData("SELECT a FROM t OFFSET 1 + 1 ROWS",              "refused: OFFSET with ROWS that is no primary")]
	[InlineData("SELECT a FROM t FOR READ ONLY FOR UPDATE",       "refused: FOR READ ONLY beside a lock")]
	[InlineData("SELECT sum(a) OVER (ROWS 1 FOLLOWING) FROM t",   "refused: a single following bound")]
	[InlineData("SELECT sum(a) OVER (ROWS BETWEEN UNBOUNDED FOLLOWING AND CURRENT ROW) FROM t", "refused: a start at UNBOUNDED FOLLOWING")]
	[InlineData("SELECT sum(a) OVER (ROWS BETWEEN CURRENT ROW AND 1 PRECEDING) FROM t", "refused: a start after its end")]
	[InlineData("SELECT * FROM f(1) (a int)",                     "refused: definitions without AS or a name")]
	[InlineData("SELECT * FROM f(1) AS t (a int, b)",             "refused: names and definitions mixed")]
	[InlineData("SELECT * FROM ROWS FROM (f(1) AS (a, b))",       "refused: ROWS FROM item with names")]
	[InlineData("SELECT a FROM t GROUP BY ROLLUP()",              "refused: ROLLUP of nothing")]
	[InlineData("SELECT a FROM t GROUP BY ROLLUP(a ORDER BY b)",  "refused: ROLLUP with ORDER BY")]
	[InlineData("SELECT a FROM t GROUP BY CUBE(a, x := 1)",       "refused: CUBE with a named argument")]
	[InlineData("SELECT a FROM t GROUP BY rollup(DISTINCT a)",    "refused: rollup with DISTINCT")]
	[InlineData("(SELECT 1 LIMIT 1) FETCH FIRST ROW ONLY",        "refused: FETCH after a bracketed LIMIT")]
	[InlineData("SELECT a FROM t ORDER BY a FETCH FIRST 1 ROWS WITH TIES FOR UPDATE SKIP LOCKED", "refused: WITH TIES and SKIP LOCKED")]
	[InlineData("SELECT sum(a) OVER (partition) FROM t",          "refused: OVER (partition)")]
	[InlineData("SELECT * FROM ((a JOIN b ON true) x) y",         "refused: an aliased bracket in a bracket")]
	[InlineData("SELECT * FROM LATERAL (a JOIN b ON true)",       "refused: LATERAL before a join")]
	[InlineData("SELECT * FROM ((SELECT 1) x)",                   "refused: an aliased subquery in a bracket")]
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
			"double bracket alias"  => query is Query.Select { From.Sources: [TableSource.Parenthesized { Source: TableSource.Parenthesized { Source: TableSource.Join }, Alias.Name.Text: "j" }] },
			"double bracket"        => query is Query.Select { From.Sources: [TableSource.Parenthesized { Source: TableSource.Parenthesized { Source: TableSource.Join }, Alias: null }] },
			"limit offset"          => query is Query.Select { OrderBy.Items: [{ Direction: SortDirection.Desc, NullOrdering: NullOrdering.Last }], Limit: { Count: not null, Offset: not null, LimitKeyword: true, OffsetAfter: true, OffsetRowWord: null, FetchRowWord: null } },
			"offset fetch"          => query is Query.Select { Limit: { Count: not null, Offset: not null, LimitKeyword: false, OffsetAfter: false, OffsetRowWord: RowWord.Rows, FetchRowWord: RowWord.Rows, Position: FetchPosition.First } },
			"offset limit"          => query is Query.Select { Limit: { Count: not null, Offset: not null, LimitKeyword: true, OffsetAfter: false, OffsetRowWord: null } },
			"limit all"             => query is Query.Select { Limit: { LimitAll: true, LimitKeyword: true, Count: null } },
			"fetch with ties"       => query is Query.Select { Limit: { WithTies: true, Position: FetchPosition.Next, FetchRowWord: RowWord.Rows } },
			"fetch no count"        => query is Query.Select { Limit: { Count: null, FetchRowWord: RowWord.Row } },
			"fetch signed, offset primary" => query is Query.Select { Limit: { Count: Expression.Unary, Offset: Expression.Subquery, OffsetAfter: true } },
			"for update of nowait"  => query is Query.Select { Locking: [{ Mode: LockingMode.Update, Of: [{ Count: 2 }], Wait: LockWait.NoWait }] },
			"two locks"             => query is Query.Select { Locking: [{ Mode: LockingMode.NoKeyUpdate, Wait: LockWait.SkipLocked }, { Mode: LockingMode.KeyShare, Wait: null }] },
			"lock then limit"       => query is Query.Select { Locking.Count: 1, Limit: not null },
			"limit then lock"       => query is Query.Select { Locking.Count: 1, Limit: not null },
			"for read only"         => query is Query.Select { Locking: [{ Mode: LockingMode.ReadOnly }] },
			"with recursive search cycle" => query is Query.Select { With: { Recursive: true, Items: [{ Columns.Count: 1, Materialized: false, Search: { Order: SearchOrder.DepthFirst }, Cycle: { MarkValue: null } }] } },
			"with materialized"     => query is Query.Select { With.Items: [{ Materialized: true, Columns.Count: 0 }] },
			"with in brackets"      => query is Query.Parenthesized { Query.With: not null, With: null },
			"bracketed operands with clauses" => query is Query.Parenthesized { SetOperations: [{ Query: Query.Parenthesized }], OrderBy: not null, Limit: not null, Query: { OrderBy: not null, Limit: not null } },
			"bracketed query with order" => query is Query.Parenthesized { SetOperations.Count: 0, OrderBy: not null, Query.OrderBy: null },
			"subquery with clauses" => query is Query.Select { Columns: [SelectColumn.ExpressionItem { Expression: Expression.Subquery { Query: { OrderBy: not null, Limit: not null } } }] },
			"group by families"     => query is Query.Select { GroupBy: { Quantifier: SetQuantifier.Distinct, Items: [GroupingElement.Rollup { Items: [GroupingElement.Ordinary { Parenthesized: false }, GroupingElement.Ordinary { Parenthesized: true, Expressions.Count: 2 }] }, GroupingElement.Cube, GroupingElement.Sets { Items: [GroupingElement.Ordinary, GroupingElement.Empty] }, GroupingElement.Empty, GroupingElement.Ordinary { Expressions: [Expression.Binary] }] }, Having: Expression.Comparison },
			"group by row"          => query is Query.Select { GroupBy.Items: [GroupingElement.Ordinary { Parenthesized: true, Expressions.Count: 2 }] },
			"window clause"         => query is Query.Select { Window.Windows: [{ Specification: { PartitionBy.Count: 1, OrderBy: not null, Frame: { Unit: WindowFrameUnit.Rows, Extent: WindowFrameExtent.Between { From.Kind: WindowBoundKind.UnboundedPreceding, To: { Kind: WindowBoundKind.Following, Offset: not null } }, Exclusion: WindowFrameExclusion.Ties } } }, { Specification: { Existing.Text: "w", Frame: { Unit: WindowFrameUnit.Range, Extent: WindowFrameExtent.Single { Bound.Kind: WindowBoundKind.CurrentRow } } } }] },
			"frame groups"          => query is Query.Select { Columns: [SelectColumn.ExpressionItem { Expression: Expression.Invocation { Over: WindowReference.Specification { Value.Frame: { Unit: WindowFrameUnit.Groups, Extent: WindowFrameExtent.Single { Bound: { Kind: WindowBoundKind.Preceding, Offset: not null } } } } } }] },
			"values"                => query is Query.Select { Values.Count: 2, Columns.Count: 0 },
			"table only"            => query is Query.Select { Table: true, From.Sources: [TableSource.Named { Only: true }] },
			"function with definitions" => query is Query.Select { From.Sources: [TableSource.Function { Alias: { Name.Text: "t", Columns: null }, ColumnDefinitions.Count: 2, Lateral: false, WithOrdinality: false }] },
			"function with definitions alone" => query is Query.Select { From.Sources: [TableSource.Function { Alias: null, ColumnDefinitions.Count: 1 }] },
			"function with column names" => query is Query.Select { From.Sources: [TableSource.Function { Alias.Columns.Count: 2, ColumnDefinitions: null }] },
			"lateral function with ordinality" => query is Query.Select { From.Sources: [TableSource.Function { Lateral: true, WithOrdinality: true, Alias.Name.Text: "t" }] },
			"unnest"                => query is Query.Select { From.Sources: [TableSource.Unnest { Expressions.Count: 2, WithOrdinality: true, Alias.Columns.Count: 2, Lateral: false }] },
			"lateral unnest"        => query is Query.Select { From.Sources: [TableSource.Unnest { Lateral: true, Alias: null }] },
			"qualified unnest is a function" => query is Query.Select { From.Sources: [TableSource.Function { Invocation.Name.Count: 2 }] },
			"rows from"             => query is Query.Select { From.Sources: [TableSource.RowsFrom { Items: [{ ColumnDefinitions: null }, { ColumnDefinitions.Count: 1 }], WithOrdinality: true, Alias.Name.Text: "r", Lateral: false }] },
			"tablesample"           => query is Query.Select { From.Sources: [TableSource.Named { Sample: { Method: SampleMethod.System, Repeat: not null } }] },
			"qualified stars"       => query is Query.Select { Columns: [SelectColumn.ExpressionItem { Expression: Expression.Wildcard { Target: Expression.Reference { Name.Count: 1 } } }, SelectColumn.ExpressionItem { Expression: Expression.Wildcard { Target: Expression.Reference { Name.Count: 2 } } }] },
			"nested derived table"  => query is Query.Select { From.Sources: [TableSource.Parenthesized { Source: TableSource.Join { Left: TableSource.Subquery { Alias.Name.Text: "x", Query: Query.Select } }, Alias.Name.Text: "z" }] },
			"nested set operation with clauses" => query is Query.Select { From.Sources: [TableSource.Parenthesized { Source: TableSource.Join { Left: TableSource.Subquery { Query: Query.Parenthesized { SetOperations.Count: 1, OrderBy: not null, Limit: not null } } } }] },
			"doubly bracketed subquery" => query is Query.Select { From.Sources: [TableSource.Subquery { Query: Query.Parenthesized { Query: Query.Select }, Alias.Name.Text: "x" }] },
			"rows from with definitions" => query is Query.Select { From.Sources: [TableSource.RowsFrom { ColumnDefinitions.Count: 1, Alias: { Name.Text: "t", Columns: null } }] },
			"rows from with definitions alone" => query is Query.Select { From.Sources: [TableSource.RowsFrom { ColumnDefinitions.Count: 1, Alias: null }] },
			"fetch a column named rows" => query is Query.Select { Limit: { Count: Expression.Reference, FetchRowWord: RowWord.Rows } },
			"rollup over a call"    => query is Query.Select { GroupBy.Items: [GroupingElement.Rollup { Items: [GroupingElement.Ordinary { Expressions: [Expression.Invocation] }] }, GroupingElement.Cube { Items: [GroupingElement.Ordinary { Expressions: [Expression.Invocation] }] }] },
			"ties inside, skip locked outside" => query is Query.Select { From.Sources: [TableSource.Subquery { Query.Limit.WithTies: true }], Locking: [{ Wait: LockWait.SkipLocked }] },
			"key word calls as tables" => query is Query.Select { From.Sources: [TableSource.Function { Invocation.Name.Count: 1, ColumnDefinitions.Count: 1, Lateral: false }, TableSource.Function { Invocation.Arguments.Count: 2, Lateral: true, WithOrdinality: true, Alias.Columns.Count: 2 }] },
			"three-part table"      => query is Query.Select { From.Sources: [TableSource.Named { Name.Count: 3 }] },
			_ => throw new ArgumentOutOfRangeException(nameof(expected)),
		};

		Assert.True(holds, $"{text}: {query}");
	}

	/// <summary>A text of statements holds empty ones too, as the server's stmtmulti does; one statement is one, with its semicolon or without.</summary>
	// ── The short statements ───────────────────────────────────────────────────

	[Theory]
	[InlineData("BEGIN",                                                  "begin")]
	[InlineData("BEGIN WORK ISOLATION LEVEL SERIALIZABLE, READ ONLY DEFERRABLE", "begin work with modes")]
	[InlineData("BEGIN TRANSACTION NOT DEFERRABLE",                       "begin transaction not deferrable")]
	[InlineData("START TRANSACTION ISOLATION LEVEL REPEATABLE READ READ WRITE", "start transaction")]
	[InlineData("COMMIT",                                                 "commit")]
	[InlineData("COMMIT WORK AND CHAIN",                                  "commit work and chain")]
	[InlineData("END TRANSACTION AND NO CHAIN",                           "end transaction and no chain")]
	[InlineData("ABORT",                                                  "abort")]
	[InlineData("ROLLBACK WORK",                                          "rollback work")]
	[InlineData("ROLLBACK TO SAVEPOINT s",                                "rollback to savepoint")]
	[InlineData("ROLLBACK TRANSACTION TO s",                              "rollback transaction to")]
	[InlineData("SAVEPOINT s",                                            "savepoint")]
	[InlineData("RELEASE SAVEPOINT s",                                    "release savepoint")]
	[InlineData("RELEASE s",                                              "release")]
	[InlineData("PREPARE TRANSACTION 'gid'",                              "prepare transaction")]
	[InlineData("COMMIT PREPARED 'gid'",                                  "commit prepared")]
	[InlineData("ROLLBACK PREPARED E'g\\id'",                             "rollback prepared")]
	[InlineData("SET TRANSACTION ISOLATION LEVEL READ COMMITTED",         "set transaction")]
	[InlineData("SET LOCAL TRANSACTION READ ONLY",                        "set local transaction")]
	[InlineData("SET SESSION TRANSACTION READ WRITE",                     "set session transaction")]
	[InlineData("SET TRANSACTION SNAPSHOT '000003A1-1'",                  "set transaction snapshot")]
	[InlineData("SET SESSION CHARACTERISTICS AS TRANSACTION READ ONLY",   "set session characteristics")]
	[InlineData("SET LOCAL SESSION CHARACTERISTICS AS TRANSACTION READ ONLY", "set local session characteristics")]
	[InlineData("SET SESSION AUTHORIZATION u",                            "set session authorization")]
	[InlineData("SET SESSION AUTHORIZATION DEFAULT",                      "set session authorization default")]
	[InlineData("SET SESSION SESSION AUTHORIZATION 'u'",                  "set session session authorization")]
	[InlineData("SET TIME ZONE 'UTC'",                                    "set time zone string")]
	[InlineData("SET LOCAL TIME ZONE LOCAL",                              "set local time zone local")]
	[InlineData("SET TIME ZONE INTERVAL '+01:00' HOUR TO MINUTE",         "set time zone interval")]
	[InlineData("SET TIME ZONE -7",                                       "set time zone number")]
	[InlineData("SET TIME ZONE DEFAULT",                                  "set time zone default")]
	[InlineData("SET TIME ZONE utc",                                      "set time zone word")]
	[InlineData("SET SCHEMA 'public'",                                    "set schema")]
	[InlineData("SET NAMES 'UTF8'",                                       "set names")]
	[InlineData("SET NAMES",                                              "set names bare")]
	[InlineData("SET NAMES DEFAULT",                                      "set names default")]
	[InlineData("SET ROLE r",                                             "set role")]
	[InlineData("SET LOCAL ROLE NONE",                                    "set local role none")]
	[InlineData("SET XML OPTION DOCUMENT",                                "set xml option")]
	[InlineData("SET search_path TO a, \"B\", 'c'",                       "set parameter to list")]
	[InlineData("SET LOCAL s.p = on",                                     "set local parameter equals")]
	[InlineData("SET p TO DEFAULT",                                       "set parameter to default")]
	[InlineData("SET p = -1.5",                                           "set parameter number")]
	[InlineData("SET p = true",                                           "set parameter boolean")]
	[InlineData("SET p FROM CURRENT",                                     "set parameter from current")]
	[InlineData("SET time TO 1",                                          "set a parameter named time")]
	[InlineData("SET role TO 'x'",                                        "set a parameter named role")]
	[InlineData("SET names TO 'x'",                                       "set a parameter named names")]
	[InlineData("SET session TO 1",                                       "set a parameter named session")]
	[InlineData("SET transaction = 1",                                    "set a parameter named transaction")]
	[InlineData("SET CONSTRAINTS ALL DEFERRED",                           "set constraints all")]
	[InlineData("SET CONSTRAINTS a, s.b IMMEDIATE",                       "set constraints named")]
	[InlineData("SHOW search_path",                                       "show")]
	[InlineData("SHOW ALL",                                               "show all")]
	[InlineData("SHOW TIME ZONE",                                         "show time zone")]
	[InlineData("SHOW TRANSACTION ISOLATION LEVEL",                       "show transaction isolation level")]
	[InlineData("SHOW SESSION AUTHORIZATION",                             "show session authorization")]
	[InlineData("RESET s.p",                                              "reset")]
	[InlineData("RESET ALL",                                              "reset all")]
	[InlineData("RESET TIME ZONE",                                        "reset time zone")]
	[InlineData("RESET time",                                             "reset a parameter named time")]
	[InlineData("EXPLAIN SELECT 1",                                       "explain")]
	[InlineData("EXPLAIN ANALYZE VERBOSE INSERT INTO t DEFAULT VALUES",   "explain analyze verbose")]
	[InlineData("EXPLAIN VERBOSE WITH x AS (SELECT 1) SELECT * FROM x",   "explain verbose with")]
	[InlineData("EXPLAIN (ANALYSE, COSTS off, FORMAT json, BUFFERS, TIMING false, SERIALIZE 'text') EXECUTE p(1)", "explain options")]
	[InlineData("CALL p(1, b => 2)",                                      "call")]
	[InlineData("DO $$ BEGIN END $$",                                     "do")]
	[InlineData("DO LANGUAGE plpgsql 'x'",                                "do language first")]
	[InlineData("DO 'x' LANGUAGE 'sql'",                                  "do language after")]
	[InlineData("PREPARE p (int, text[]) AS SELECT $1, $2",               "prepare plan")]
	[InlineData("PREPARE p AS INSERT INTO t VALUES ($1)",                 "prepare plan insert")]
	[InlineData("EXECUTE p",                                              "execute")]
	[InlineData("EXECUTE p(1, 'x')",                                      "execute arguments")]
	[InlineData("DEALLOCATE p",                                           "deallocate")]
	[InlineData("DEALLOCATE PREPARE p",                                   "deallocate prepare")]
	[InlineData("DEALLOCATE ALL",                                         "deallocate all")]
	[InlineData("DEALLOCATE PREPARE ALL",                                 "deallocate prepare all")]
	[InlineData("DEALLOCATE prepare",                                     "deallocate a plan named prepare")]
	[InlineData("LISTEN c",                                               "listen")]
	[InlineData("UNLISTEN c",                                             "unlisten")]
	[InlineData("UNLISTEN *",                                             "unlisten all")]
	[InlineData("NOTIFY c",                                               "notify")]
	[InlineData("NOTIFY c, 'payload'",                                    "notify payload")]
	[InlineData("LOCK TABLE a, ONLY b, c * IN SHARE UPDATE EXCLUSIVE MODE NOWAIT", "lock")]
	[InlineData("LOCK t",                                                 "lock bare")]
	[InlineData("LOCK t IN ACCESS EXCLUSIVE MODE",                        "lock access exclusive")]
	[InlineData("DISCARD ALL",                                            "discard all")]
	[InlineData("DISCARD TEMPORARY",                                      "discard temporary")]
	[InlineData("DISCARD TEMP",                                           "discard temp")]
	[InlineData("CHECKPOINT",                                             "checkpoint")]
	[InlineData("DECLARE c BINARY INSENSITIVE NO SCROLL CURSOR WITH HOLD FOR SELECT 1", "declare")]
	[InlineData("DECLARE c SCROLL ASENSITIVE SCROLL CURSOR WITHOUT HOLD FOR WITH x AS (SELECT 1) SELECT * FROM x FOR UPDATE", "declare with and locking")]
	[InlineData("DECLARE c CURSOR FOR SELECT 1 FOR READ ONLY",             "declare for read only")]
	[InlineData("FETCH c",                                                "fetch")]
	[InlineData("FETCH NEXT FROM c",                                      "fetch next from")]
	[InlineData("FETCH PRIOR IN c",                                       "fetch prior in")]
	[InlineData("FETCH ABSOLUTE -5 c",                                    "fetch absolute")]
	[InlineData("FETCH RELATIVE +2 c",                                    "fetch relative")]
	[InlineData("FETCH 5 c",                                              "fetch count")]
	[InlineData("FETCH ALL c",                                            "fetch all")]
	[InlineData("FETCH FORWARD c",                                        "fetch forward")]
	[InlineData("FETCH FORWARD 3 c",                                      "fetch forward count")]
	[InlineData("FETCH FORWARD ALL c",                                    "fetch forward all")]
	[InlineData("FETCH BACKWARD 3 IN c",                                  "fetch backward count in")]
	[InlineData("FETCH BACKWARD ALL c",                                   "fetch backward all")]
	[InlineData("FETCH next",                                             "fetch a cursor named next")]
	[InlineData("MOVE c",                                                 "move")]
	[InlineData("MOVE FORWARD ALL IN c",                                  "move forward all in")]
	[InlineData("MOVE -3 c",                                              "move count")]
	[InlineData("CLOSE c",                                                "close")]
	[InlineData("CLOSE ALL",                                              "close all")]
	[InlineData("VACUUM",                                                 "vacuum")]
	[InlineData("VACUUM FULL FREEZE VERBOSE ANALYZE t (a, b), ONLY u",    "vacuum bare")]
	[InlineData("VACUUM (FULL, ANALYZE, SKIP_LOCKED false, INDEX_CLEANUP auto, PARALLEL 2) t", "vacuum options")]
	[InlineData("ANALYZE",                                                "analyze")]
	[InlineData("ANALYSE VERBOSE t (a)",                                  "analyse verbose")]
	[InlineData("ANALYZE (VERBOSE, SKIP_LOCKED) t, u",                    "analyze options")]
	[InlineData("COPY t FROM STDIN",                                      "copy from stdin")]
	[InlineData("COPY t TO STDOUT",                                       "copy to stdout")]
	[InlineData("COPY t FROM STDOUT",                                     "copy from stdout")]
	[InlineData("COPY BINARY t (a, b) FROM PROGRAM 'cat' USING DELIMITERS '|' WITH CSV HEADER FORCE NOT NULL a, b WHERE a > 1", "copy old words")]
	[InlineData("COPY t FROM 'f' WITH",                                   "copy with alone")]
	[InlineData("COPY t FROM 'f' DELIMITER AS ',' NULL '' QUOTE '\"' ESCAPE '\\' FREEZE FORCE QUOTE * FORCE NULL a ENCODING 'UTF8'", "copy every old word")]
	[InlineData("COPY (SELECT 1) TO STDOUT WITH (FORMAT csv, HEADER, FORCE_QUOTE *, FORCE_NULL (a, 'b'), DELIMITER ',', NULL '', ENCODING 'UTF8', freeze true, header 1, x DEFAULT)", "copy query options")]
	[InlineData("COPY (INSERT INTO t DEFAULT VALUES RETURNING *) TO STDOUT", "copy data change")]
	[InlineData("COPY t FROM 'f' (format csv) WHERE a",                   "copy bracketed where")]
	[InlineData("EXPLAIN DECLARE c CURSOR FOR SELECT 1",                  "explain declare")]
	[InlineData("COPY t TO stdout CSV FORCE  QUOTE a",                    "copy force quote with two spaces")]
	[InlineData("COPY t TO stdout CSV FORCE/*x*/QUOTE a",                 "copy force quote with a comment")]
	[InlineData("COPY t FROM stdin CSV FORCE  NOT NULL a",                "copy force not null with two spaces")]
	[InlineData("COPY BINARY t FROM stdin (on_error stop)",               "copy binary with brackets")]
	[InlineData("COPY t FROM stdin USING DELIMITERS ',' (on_error ignore)", "copy using delimiters with brackets")]
	[InlineData("COPY t FROM stdin DELIMITERS ',' (on_error ignore)",     "copy delimiters with brackets")]
	[InlineData("COPY BINARY t FROM 'f' USING DELIMITERS '|' WITH HEADER", "copy binary and delimiters with old words")]
	[InlineData("ROLLBACK TO savepoint",                                  "rollback to a savepoint named savepoint")]
	[InlineData("ROLLBACK TO SAVEPOINT",                                  "rollback to a savepoint named SAVEPOINT")]
	[InlineData("RELEASE savepoint",                                      "release a savepoint named savepoint")]
	[InlineData("SAVEPOINT savepoint",                                    "a savepoint named savepoint")]
	[InlineData("FETCH 2147483647 c",                                     "fetch the largest count")]
	[InlineData("FETCH 0x10 c",                                           "fetch a hexadecimal count")]
	[InlineData("FETCH 0o17 c",                                           "fetch an octal count")]
	[InlineData("FETCH 0b101 c",                                          "fetch a binary count")]
	[InlineData("FETCH ABSOLUTE 0x10 c",                                  "fetch absolute hexadecimal")]
	[InlineData("FETCH FORWARD 0b1 c",                                    "fetch forward binary")]
	[InlineData("FETCH 0x7FFFFFFF c",                                     "fetch the largest hexadecimal count")]
	[InlineData("FETCH -0x10 c",                                          "fetch a negative hexadecimal count")]
	[InlineData("FETCH 0x80000000 c",                                     "refused: a hexadecimal count past 32 bits")]
	[InlineData("FETCH -2147483647 c",                                    "fetch the smallest count")]
	[InlineData("FETCH 2147483648 c",                                     "refused: a count past 32 bits")]
	[InlineData("FETCH -2147483648 c",                                    "refused: a negative count past 32 bits")]
	[InlineData("FETCH ABSOLUTE 99999999999999999999 c",                  "refused: ABSOLUTE past 32 bits")]
	[InlineData("MOVE FORWARD 2147483648 c",                              "refused: MOVE past 32 bits")]
	[InlineData("SET CONSTRAINTS a.b.c.d DEFERRED",                       "refused: SET CONSTRAINTS of a four-part name")]
	[InlineData("SET CONSTRAINTS a, b.c.d.e IMMEDIATE",                   "refused: SET CONSTRAINTS of a four-part name later")]
	[InlineData("DECLARE c CURSOR FOR INSERT INTO t DEFAULT VALUES RETURNING *", "refused: DECLARE of a data change")]
	[InlineData("FETCH ABSOLUTE c",                                       "refused: ABSOLUTE without a count")]
	[InlineData("VACUUM FREEZE FULL t",                                   "refused: VACUUM words out of order")]
	[InlineData("ANALYZE FULL t",                                         "refused: ANALYZE FULL")]
	[InlineData("COPY t TO 'f' WHERE a > 1",                              "refused: COPY TO with WHERE")]
	[InlineData("COPY (TRUNCATE t) TO STDOUT",                            "refused: COPY of a TRUNCATE")]
	[InlineData("WITH x AS (SELECT 1) COMMIT",                            "refused: WITH before COMMIT")]
	[InlineData("BEGIN, READ ONLY",                                       "refused: a comma before the first mode")]
	[InlineData("ROLLBACK TO s AND CHAIN",                                "refused: ROLLBACK TO with a chain")]
	[InlineData("SET CATALOG 'x'",                                        "refused: SET CATALOG")]
	[InlineData("SET LOCAL AUTHORIZATION x",                              "refused: AUTHORIZATION after LOCAL")]
	[InlineData("SET CHARACTERISTICS AS TRANSACTION READ ONLY",           "refused: CHARACTERISTICS with no SESSION")]
	[InlineData("SET p",                                                  "refused: SET with no value")]
	[InlineData("EXPLAIN EXPLAIN SELECT 1",                               "refused: EXPLAIN of an EXPLAIN")]
	[InlineData("EXPLAIN TRUNCATE t",                                     "refused: EXPLAIN of a TRUNCATE")]
	[InlineData("PREPARE p AS TRUNCATE t",                                "refused: PREPARE of a TRUNCATE")]
	[InlineData("PREPARE p AS EXECUTE q",                                 "refused: PREPARE of an EXECUTE")]
	[InlineData("DO LANGUAGE sql",                                        "refused: DO with no body")]
	[InlineData("DO 'x' 'y'",                                             "refused: DO with two bodies")]
	[InlineData("DO 'x' LANGUAGE a LANGUAGE b",                           "refused: DO with two languages")]
	[InlineData("EXECUTE p()",                                            "refused: EXECUTE with empty brackets")]
	[InlineData("CALL f(a) OVER ()",                                      "refused: CALL with OVER")]
	[InlineData("LOCK t IN SHARE MODE MODE",                              "refused: LOCK with a word too many")]
	[InlineData("DISCARD PLAN",                                           "refused: DISCARD of a wrong word")]
	public void A_short_statement_builds_its_record(string text, string expected)
	{
		if (expected.StartsWith("refused", StringComparison.Ordinal))
		{
			Assert.False(PostgreSqlParser.TryParseStatement(text).IsSuccess);
			return;
		}

		var made  = Holds(text);
		var holds = expected switch
		{
			"begin"                    => made is Statement.StartTransaction { Begin: true, Keyword: null, Work: false, Modes.Count: 0 },
			"begin work with modes"    => made is Statement.StartTransaction { Begin: true, Work: true, Modes: [TransactionMode.Isolation { Level: IsolationLevel.Serializable }, TransactionMode.Access { Mode: TransactionAccess.ReadOnly }, TransactionMode.Deferrable { Not: false }] },
			"begin transaction not deferrable" => made is Statement.StartTransaction { Begin: true, Keyword: TransactionKeyword.Transaction, Modes: [TransactionMode.Deferrable { Not: true }] },
			"start transaction"        => made is Statement.StartTransaction { Begin: false, Modes.Count: 2 },
			"commit"                   => made is Statement.Commit { End: false, Work: false, Chain: null, Keyword: null },
			"commit work and chain"    => made is Statement.Commit { Work: true, Chain: ChainMode.Chain },
			"end transaction and no chain" => made is Statement.Commit { End: true, Keyword: TransactionKeyword.Transaction, Chain: ChainMode.NoChain },
			"abort"                    => made is Statement.Rollback { Abort: true, ToSavepoint: null },
			"rollback work"            => made is Statement.Rollback { Abort: false, Work: true },
			"rollback to savepoint"    => made is Statement.Rollback { ToSavepoint.Text: "s", SavepointKeywordOmitted: false },
			"rollback transaction to"  => made is Statement.Rollback { Keyword: TransactionKeyword.Transaction, ToSavepoint.Text: "s", SavepointKeywordOmitted: true },
			"savepoint"                => made is Statement.Savepoint { Name.Text: "s" },
			"release savepoint"        => made is Statement.ReleaseSavepoint { SavepointKeywordOmitted: false },
			"release"                  => made is Statement.ReleaseSavepoint { SavepointKeywordOmitted: true },
			"prepare transaction"      => made is Statement.PrepareTransaction { Id.Text: "'gid'" },
			"commit prepared"          => made is Statement.CommitPrepared,
			"rollback prepared"        => made is Statement.RollbackPrepared { Id.Kind: StringLiteralKind.Escaped },
			"set transaction"          => made is Statement.SetTransaction { Local: false, Session: false, Modes: [TransactionMode.Isolation { Level: IsolationLevel.ReadCommitted }] },
			"set local transaction"    => made is Statement.SetTransaction { Local: true, Session: false },
			"set session transaction"  => made is Statement.SetTransaction { Local: false, Session: true },
			"set transaction snapshot" => made is Statement.SetTransactionSnapshot { Scope: null },
			"set session characteristics" => made is Statement.SetSessionCharacteristics { Scope: null, Characteristics: [[TransactionMode.Access]] },
			"set local session characteristics" => made is Statement.SetSessionCharacteristics { Scope: SetScope.Local },
			"set session authorization" => made is Statement.SetSessionAuthorization { Scope: null, Value: Expression.Reference },
			"set session authorization default" => made is Statement.SetSessionAuthorization { Value: Expression.Default },
			"set session session authorization" => made is Statement.SetSessionAuthorization { Scope: SetScope.Session, Value: LiteralValue.String },
			"set time zone string"     => made is Statement.SetTimeZone { Value: LiteralValue.String, Local: false, Scope: null },
			"set local time zone local" => made is Statement.SetTimeZone { Value: null, Local: true, Scope: SetScope.Local },
			"set time zone interval"   => made is Statement.SetTimeZone { Value: LiteralValue.Interval },
			"set time zone number"     => made is Statement.SetTimeZone { Value: Expression.Unary { Operator: UnaryOperator.Minus } },
			"set time zone default"    => made is Statement.SetTimeZone { Value: Expression.Default },
			"set time zone word"       => made is Statement.SetTimeZone { Value: Expression.Reference },
			"set schema"               => made is Statement.SetSchema { Value: LiteralValue.String },
			"set names"                => made is Statement.SetNames { Value: LiteralValue.String },
			"set names bare"           => made is Statement.SetNames { Value: null },
			"set names default"        => made is Statement.SetNames { Value: Expression.Default },
			"set role"                 => made is Statement.SetRole { None: false, Value: Expression.Reference },
			"set local role none"      => made is Statement.SetRole { None: true, Value: null, Scope: SetScope.Local },
			"set xml option"           => made is Statement.SetParameter { Spelling: ParameterSpelling.XmlOption, Name: [{ Text: "xmloption" }], Values: [Expression.Reference] },
			"set parameter to list"    => made is Statement.SetParameter { Name: [{ Text: "search_path" }], Values: [Expression.Reference, Expression.Reference { Name: [{ Style: IdentifierStyle.Delimited }] }, LiteralValue.String], EqualsSign: false, Scope: null },
			"set local parameter equals" => made is Statement.SetParameter { Name.Count: 2, Values: [Expression.Reference { Name: [{ Text: "on" }] }], EqualsSign: true, Scope: SetScope.Local },
			"set parameter to default" => made is Statement.SetParameter { Values: [Expression.Default] },
			"set parameter number"     => made is Statement.SetParameter { Values: [Expression.Unary { Operand: LiteralValue.Numeric }] },
			"set parameter boolean"    => made is Statement.SetParameter { Values: [LiteralValue.Boolean] },
			"set parameter from current" => made is Statement.SetParameter { FromCurrent: true, Values.Count: 0 },
			"set a parameter named time" => made is Statement.SetParameter { Name: [{ Text: "time" }] },
			"set a parameter named role" => made is Statement.SetParameter { Name: [{ Text: "role" }] },
			"set a parameter named names" => made is Statement.SetParameter { Name: [{ Text: "names" }] },
			"set a parameter named session" => made is Statement.SetParameter { Name: [{ Text: "session" }], Scope: null },
			"set a parameter named transaction" => made is Statement.SetParameter { Name: [{ Text: "transaction" }], EqualsSign: true },
			"set constraints all"      => made is Statement.SetConstraints { Target: ConstraintTarget.All, Timing: ConstraintTiming.Deferred },
			"set constraints named"    => made is Statement.SetConstraints { Target: ConstraintTarget.Names { Values.Count: 2 }, Timing: ConstraintTiming.Immediate },
			"show"                     => made is Statement.ShowParameter { Name: [{ Text: "search_path" }], Spelling: null },
			"show all"                 => made is Statement.ShowParameter { Name: null },
			"show time zone"           => made is Statement.ShowParameter { Name: [{ Text: "timezone" }], Spelling: ParameterSpelling.TimeZone },
			"show transaction isolation level" => made is Statement.ShowParameter { Spelling: ParameterSpelling.TransactionIsolationLevel },
			"show session authorization" => made is Statement.ShowParameter { Spelling: ParameterSpelling.SessionAuthorization },
			"reset"                    => made is Statement.ResetParameter { Name.Count: 2 },
			"reset all"                => made is Statement.ResetParameter { Name: null },
			"reset time zone"          => made is Statement.ResetParameter { Spelling: ParameterSpelling.TimeZone },
			"reset a parameter named time" => made is Statement.ResetParameter { Name: [{ Text: "time" }], Spelling: null },
			"explain"                  => made is Statement.Explain { Options.Count: 0, Bare: false, Statement: Statement.Select },
			"explain analyze verbose"  => made is Statement.Explain { Options: [{ Name.Text: "ANALYZE" }, { Name.Text: "VERBOSE" }], Bare: true, Statement: Statement.Insert },
			"explain verbose with"     => made is Statement.Explain { Options.Count: 1, Bare: true, Statement: Statement.Select { Query.With: not null } },
			"explain options"          => made is Statement.Explain { Options: [{ Name.Text: "ANALYSE", Value: null }, { Value: Expression.Reference }, { Value: Expression.Reference }, { Value: null }, { Value: LiteralValue.Boolean }, { Value: LiteralValue.String }], Bare: false, Statement: Statement.Execute { Parameters: DynamicArguments.Values } },
			"call"                     => made is Statement.Call { Invocation.Arguments.Count: 2 },
			"do"                       => made is Statement.Do { Body.Kind: StringLiteralKind.Dollar, Language: null },
			"do language first"        => made is Statement.Do { Language: Expression.Reference, LanguageFirst: true },
			"do language after"        => made is Statement.Do { Language: LiteralValue.String, LanguageFirst: false },
			"prepare plan"             => made is Statement.PreparePlan { Name.Text: "p", ParameterTypes: [DataType.Numeric, DataType.Array], Body: Statement.Select },
			"prepare plan insert"      => made is Statement.PreparePlan { ParameterTypes.Count: 0, Body: Statement.Insert },
			"execute"                  => made is Statement.Execute { Statement.Name.Text: "p", Parameters: null, Result: null },
			"execute arguments"        => made is Statement.Execute { Parameters: DynamicArguments.Values { Items.Count: 2 } },
			"deallocate"               => made is Statement.DeallocatePrepare { PrepareOmitted: true },
			"deallocate prepare"       => made is Statement.DeallocatePrepare { PrepareOmitted: false },
			"deallocate all"           => made is Statement.DeallocateAll { PrepareKeyword: false },
			"deallocate prepare all"   => made is Statement.DeallocateAll { PrepareKeyword: true },
			"deallocate a plan named prepare" => made is Statement.DeallocatePrepare { Statement.Name.Text: "prepare", PrepareOmitted: true },
			"listen"                   => made is Statement.Listen { Channel.Text: "c" },
			"unlisten"                 => made is Statement.Unlisten { Channel.Text: "c" },
			"unlisten all"             => made is Statement.Unlisten { Channel: null },
			"notify"                   => made is Statement.Notify { Payload: null },
			"notify payload"           => made is Statement.Notify { Payload: not null },
			"lock"                     => made is Statement.LockTable { Tables: [TableSource.Named, TableSource.Named { Only: true }, TableSource.Named { Star: true }], Mode: LockMode.ShareUpdateExclusive, NoWait: true, TableOmitted: false },
			"lock bare"                => made is Statement.LockTable { Tables.Count: 1, Mode: null, NoWait: false, TableOmitted: true },
			"lock access exclusive"    => made is Statement.LockTable { Mode: LockMode.AccessExclusive },
			"discard all"              => made is Statement.Discard { Target: DiscardTarget.All },
			"discard temporary"        => made is Statement.Discard { Target: DiscardTarget.Temp, Temporary: true },
			"discard temp"             => made is Statement.Discard { Target: DiscardTarget.Temp, Temporary: false },
			"checkpoint"               => made is Statement.Checkpoint { Duration: null },
			"declare"                  => made is Statement.DeclareCursor { Cursor.Name: [{ Text: "c" }], Properties: { Binary: true, Sensitivity: CursorSensitivity.Insensitive, Scrollability: CursorScrollability.NoScroll, Holdability: CursorHoldability.WithHold, Returnability: null }, SourceValue: CursorSource.Query { Updatability: null, Select.Query: Query.Select }, Extended: false },
			"declare with and locking" => made is Statement.DeclareCursor { Properties: { Binary: false, Sensitivity: CursorSensitivity.Asensitive, Scrollability: CursorScrollability.Scroll, Holdability: CursorHoldability.WithoutHold }, SourceValue: CursorSource.Query { Select.Query: { With: not null, Locking.Count: 1 } } },
			"declare for read only"    => made is Statement.DeclareCursor { Properties: { Sensitivity: null, Scrollability: null, Holdability: null }, SourceValue: CursorSource.Query { Select.Query.Locking: [{ Mode: LockingMode.ReadOnly }] } },
			"fetch"                    => made is Statement.FetchCursor { Orientation: null, FromKeyword: false, InKeyword: false, Cursor.Name: [{ Text: "c" }], Into: null },
			"fetch next from"          => made is Statement.FetchCursor { Orientation: { Kind: FetchOrientationKind.Next, Offset: null }, FromKeyword: true },
			"fetch prior in"           => made is Statement.FetchCursor { Orientation.Kind: FetchOrientationKind.Prior, InKeyword: true, FromKeyword: false },
			"fetch absolute"           => made is Statement.FetchCursor { Orientation: { Kind: FetchOrientationKind.Absolute, Offset: LiteralValue.Numeric { Text: "-5" } } },
			"fetch relative"           => made is Statement.FetchCursor { Orientation: { Kind: FetchOrientationKind.Relative, Offset: LiteralValue.Numeric { Text: "+2" } } },
			"fetch count"              => made is Statement.FetchCursor { Orientation: { Kind: FetchOrientationKind.Forward, Offset: LiteralValue.Numeric, All: false, WordOmitted: true } },
			"fetch all"                => made is Statement.FetchCursor { Orientation: { Kind: FetchOrientationKind.Forward, Offset: null, All: true, WordOmitted: true } },
			"fetch forward"            => made is Statement.FetchCursor { Orientation: { Kind: FetchOrientationKind.Forward, Offset: null, All: false, WordOmitted: false } },
			"fetch forward count"      => made is Statement.FetchCursor { Orientation: { Kind: FetchOrientationKind.Forward, Offset: not null, WordOmitted: false } },
			"fetch forward all"        => made is Statement.FetchCursor { Orientation: { Kind: FetchOrientationKind.Forward, All: true, WordOmitted: false } },
			"fetch backward count in"  => made is Statement.FetchCursor { Orientation: { Kind: FetchOrientationKind.Backward, Offset: not null }, InKeyword: true },
			"fetch backward all"       => made is Statement.FetchCursor { Orientation: { Kind: FetchOrientationKind.Backward, All: true } },
			"fetch a cursor named next" => made is Statement.FetchCursor { Orientation: null, Cursor.Name: [{ Text: "next" }] },
			"move"                     => made is Statement.MoveCursor { Orientation: null, Cursor.Name: [{ Text: "c" }] },
			"move forward all in"      => made is Statement.MoveCursor { Orientation: { Kind: FetchOrientationKind.Forward, All: true }, InKeyword: true },
			"move count"               => made is Statement.MoveCursor { Orientation: { Kind: FetchOrientationKind.Forward, Offset: LiteralValue.Numeric { Text: "-3" }, WordOmitted: true } },
			"close"                    => made is Statement.CloseCursor { Cursor.Name: [{ Text: "c" }] },
			"close all"                => made is Statement.CloseAllCursors,
			"vacuum"                   => made is Statement.Vacuum { Options.Count: 0, Relations.Count: 0 },
			"vacuum bare"              => made is Statement.Vacuum { Options: [{ Name.Text: "FULL" }, { Name.Text: "FREEZE" }, { Name.Text: "VERBOSE" }, { Name.Text: "ANALYZE" }], Bare: true, Relations: [{ Relation: TableSource.Named, Columns.Count: 2 }, { Relation: TableSource.Named { Only: true }, Columns.Count: 0 }] },
			"vacuum options"           => made is Statement.Vacuum { Options: [{ Value: null }, { Value: null }, { Value: LiteralValue.Boolean }, { Value: Expression.Reference }, { Value: LiteralValue.Numeric }], Bare: false, Relations.Count: 1 },
			"analyze"                  => made is Statement.Analyze { Options.Count: 0, Relations.Count: 0, British: false },
			"analyse verbose"          => made is Statement.Analyze { Options: [{ Name.Text: "VERBOSE" }], Bare: true, British: true, Relations: [{ Columns.Count: 1 }] },
			"analyze options"          => made is Statement.Analyze { Options.Count: 2, Bare: false, Relations.Count: 2 },
			"copy from stdin"          => made is Statement.Copy { Table: TableSource.Named, Direction: CopyDirection.From, File: null, Stream: StandardStream.Stdin, Options.Count: 0, Bare: false, WithKeyword: false },
			"copy to stdout"           => made is Statement.Copy { Direction: CopyDirection.To, Stream: StandardStream.Stdout },
			"copy from stdout"         => made is Statement.Copy { Direction: CopyDirection.From, Stream: StandardStream.Stdout },
			"copy old words"           => made is Statement.Copy { Columns.Count: 2, Program: true, File: not null, Stream: null, Options: [{ Name.Text: "format", Value: Expression.Reference }, { Name.Text: "delimiter", Value: LiteralValue.String }, { Name.Text: "format" }, { Name.Text: "header", Value: LiteralValue.Boolean }, { Name.Text: "force_not_null", Value: Expression.Row { Items.Count: 2 } }], WithKeyword: true, Bare: true, Where: not null },
			"copy with alone"          => made is Statement.Copy { Options.Count: 0, WithKeyword: true, Bare: false },
			"copy every old word"      => made is Statement.Copy { Options: [{ Name.Text: "delimiter" }, { Name.Text: "null" }, { Name.Text: "quote" }, { Name.Text: "escape" }, { Name.Text: "freeze" }, { Name.Text: "force_quote", Value: Expression.Asterisk }, { Name.Text: "force_null", Value: Expression.Row }, { Name.Text: "encoding" }], Bare: true },
			"copy query options"       => made is Statement.Copy { Table: null, Query: Query.Select, Direction: CopyDirection.To, Stream: StandardStream.Stdout, Options: [{ Value: Expression.Reference }, { Value: null }, { Value: Expression.Asterisk }, { Value: Expression.Row { Items: [Expression.Reference, LiteralValue.String] } }, { Value: LiteralValue.String }, { Value: LiteralValue.String }, { Value: LiteralValue.String }, { Value: LiteralValue.Boolean }, { Value: LiteralValue.Numeric }, { Value: Expression.Default }], Bare: false, WithKeyword: true },
			"copy data change"         => made is Statement.Copy { Query: Query.DataChange },
			"copy bracketed where"     => made is Statement.Copy { Options.Count: 1, Bare: false, WithKeyword: false, Where: not null },
			"explain declare"          => made is Statement.Explain { Statement: Statement.DeclareCursor },
			"copy force quote with two spaces" => made is Statement.Copy { Options: [{ Name.Text: "format" }, { Name.Text: "force_quote", Value: Expression.Row }] },
			"copy force quote with a comment" => made is Statement.Copy { Options: [{ Name.Text: "format" }, { Name.Text: "force_quote" }] },
			"copy force not null with two spaces" => made is Statement.Copy { Options: [{ Name.Text: "format" }, { Name.Text: "force_not_null" }] },
			"copy binary with brackets" => made is Statement.Copy { Options: [{ Name.Text: "format" }, { Name.Text: "on_error" }], Bare: false, BinaryBeforeTable: true, DelimitersClause: false },
			"copy using delimiters with brackets" => made is Statement.Copy { Options: [{ Name.Text: "delimiter" }, { Name.Text: "on_error" }], Bare: false, DelimitersClause: true },
			"copy delimiters with brackets" => made is Statement.Copy { Options.Count: 2, Bare: false, DelimitersClause: true },
			"copy binary and delimiters with old words" => made is Statement.Copy { Options: [{ Name.Text: "format" }, { Name.Text: "delimiter" }, { Name.Text: "header" }], Bare: true, BinaryBeforeTable: true, DelimitersClause: true, WithKeyword: true },
			"rollback to a savepoint named savepoint" => made is Statement.Rollback { ToSavepoint.Text: "savepoint", SavepointKeywordOmitted: true },
			"rollback to a savepoint named SAVEPOINT" => made is Statement.Rollback { ToSavepoint.Text: "SAVEPOINT", SavepointKeywordOmitted: true },
			"release a savepoint named savepoint" => made is Statement.ReleaseSavepoint { Name.Text: "savepoint", SavepointKeywordOmitted: true },
			"a savepoint named savepoint" => made is Statement.Savepoint { Name.Text: "savepoint" },
			"fetch the largest count"  => made is Statement.FetchCursor { Orientation.Offset: LiteralValue.Numeric { Text: "2147483647" } },
			"fetch a hexadecimal count" => made is Statement.FetchCursor { Orientation.Offset: LiteralValue.Numeric { Text: "0x10", Kind: NumericLiteralKind.HexInteger } },
			"fetch an octal count"     => made is Statement.FetchCursor { Orientation.Offset: LiteralValue.Numeric { Kind: NumericLiteralKind.OctalInteger } },
			"fetch a binary count"     => made is Statement.FetchCursor { Orientation.Offset: LiteralValue.Numeric { Kind: NumericLiteralKind.BinaryInteger } },
			"fetch absolute hexadecimal" => made is Statement.FetchCursor { Orientation: { Kind: FetchOrientationKind.Absolute, Offset: LiteralValue.Numeric { Kind: NumericLiteralKind.HexInteger } } },
			"fetch forward binary"     => made is Statement.FetchCursor { Orientation: { Kind: FetchOrientationKind.Forward, Offset: LiteralValue.Numeric { Kind: NumericLiteralKind.BinaryInteger } } },
			"fetch the largest hexadecimal count" => made is Statement.FetchCursor { Orientation.Offset: LiteralValue.Numeric { Text: "0x7FFFFFFF", Kind: NumericLiteralKind.HexInteger } },
			"fetch a negative hexadecimal count" => made is Statement.FetchCursor { Orientation.Offset: LiteralValue.Numeric { Text: "-0x10", Kind: NumericLiteralKind.HexInteger } },
			"fetch the smallest count" => made is Statement.FetchCursor { Orientation.Offset: LiteralValue.Numeric { Text: "-2147483647" } },
			_ => throw new ArgumentOutOfRangeException(nameof(expected)),
		};

		Assert.True(holds, $"{text}: {made}");
	}

	// ── The data change statements ─────────────────────────────────────────────

	[Theory]
	[InlineData("INSERT INTO t (a, b) VALUES (1, 2), (3, 4)",              "insert values")]
	[InlineData("INSERT INTO t AS x DEFAULT VALUES RETURNING *",          "insert alias default returning")]
	[InlineData("INSERT INTO t OVERRIDING SYSTEM VALUE SELECT 1",         "insert overriding query")]
	[InlineData("INSERT INTO t ((SELECT 1))",                             "insert bracketed query")]
	[InlineData("INSERT INTO t VALUES (1) ON CONFLICT (a, (b + 1), lower(c)) WHERE a > 0 DO UPDATE SET a = EXCLUDED.a WHERE t.a < 1", "on conflict columns do update")]
	[InlineData("INSERT INTO t VALUES (1) ON CONFLICT ON CONSTRAINT c DO NOTHING", "on conflict constraint do nothing")]
	[InlineData("INSERT INTO t VALUES (1) ON CONFLICT DO NOTHING",         "on conflict without target")]
	[InlineData("INSERT INTO t VALUES (1) RETURNING WITH (OLD AS o, NEW AS n) o.a, n.a AS b", "returning old and new")]
	[InlineData("WITH x AS (SELECT 1) INSERT INTO t SELECT * FROM x",      "insert with")]
	[InlineData("UPDATE ONLY t AS u SET a = 1, (b, c) = (2, 3), d[1] = 4, e.f = 5 FROM v WHERE u.a = v.a RETURNING u.a", "update")]
	[InlineData("UPDATE t u SET a = 1 WHERE CURRENT OF c",                "update current of")]
	[InlineData("DELETE FROM t * USING u, v WHERE t.a = u.a RETURNING *",  "delete using")]
	[InlineData("DELETE FROM t WHERE CURRENT OF c",                       "delete current of")]
	[InlineData("MERGE INTO t AS x USING s ON t.a = s.a WHEN MATCHED AND s.b > 0 THEN UPDATE SET b = s.b WHEN MATCHED THEN DELETE WHEN NOT MATCHED BY SOURCE THEN DO NOTHING WHEN NOT MATCHED BY TARGET THEN INSERT (a, b) OVERRIDING USER VALUE VALUES (s.a, s.b) WHEN NOT MATCHED THEN DO NOTHING RETURNING merge_action(), t.*", "merge")]
	[InlineData("MERGE INTO t USING s ON true WHEN NOT MATCHED THEN INSERT DEFAULT VALUES", "merge insert default values")]
	[InlineData("WITH moved AS (DELETE FROM t RETURNING *) INSERT INTO u SELECT * FROM moved", "data change body in insert")]
	[InlineData("WITH x AS (UPDATE t SET a = 1 RETURNING a) SELECT * FROM x", "data change body in select")]
	[InlineData("TRUNCATE a, ONLY b, c * RESTART IDENTITY CASCADE",         "truncate")]
	[InlineData("TRUNCATE TABLE t CONTINUE IDENTITY RESTRICT",            "truncate table")]
	[InlineData("INSERT INTO t (values) VALUES (1)",                      "insert into a column named values")]
	[InlineData("INSERT INTO t (values, a) SELECT 1, 2",                  "insert into columns named values and a")]
	[InlineData("INSERT INTO t (VALUES (1))",                             "insert bracketed values")]
	[InlineData("INSERT INTO t VALUES (1) ON CONFLICT (coalesce(a, 0), (CAST(a AS int)), CAST(b AS int), EXTRACT(year FROM d)) DO NOTHING", "on conflict key word calls")]
	[InlineData("INSERT INTO t VALUES (1) ON CONFLICT (a COLLATE \"C\" text_pattern_ops DESC, lower(b) nulls, c gist_ops (siglen = 8) NULLS FIRST) DO NOTHING", "on conflict index elements")]
	[InlineData("INSERT INTO a.b.c VALUES (1)",                           "insert into a three-part name")]
	[InlineData("WITH x AS (SELECT 1) TRUNCATE t",                        "refused: WITH before TRUNCATE")]
	[InlineData("WITH x AS (TRUNCATE t) SELECT 1",                        "refused: TRUNCATE as a WITH body")]
	[InlineData("MERGE INTO t USING s ON true WHEN NOT MATCHED THEN INSERT (a) DEFAULT VALUES", "refused: MERGE INSERT with columns and DEFAULT VALUES")]
	[InlineData("MERGE INTO t USING s ON true WHEN NOT MATCHED THEN INSERT OVERRIDING SYSTEM VALUE DEFAULT VALUES", "refused: MERGE INSERT with OVERRIDING and DEFAULT VALUES")]
	[InlineData("UPDATE t SET a = 1 RETURNING WITH (OLD AS o, OLD AS p) *", "refused: OLD named twice")]
	[InlineData("UPDATE t SET a = 1 RETURNING WITH (NEW AS n, OLD AS o, NEW AS m) *", "refused: NEW named twice")]
	[InlineData("INSERT INTO a.b.c.d VALUES (1)",                         "refused: four-part INSERT target")]
	[InlineData("UPDATE a.b.c.d SET x = 1",                               "refused: four-part UPDATE target")]
	[InlineData("DELETE FROM a.b.c.d",                                    "refused: four-part DELETE target")]
	[InlineData("TRUNCATE a, ONLY (a.b.c.d)",                             "refused: four-part TRUNCATE target")]
	[InlineData("MERGE INTO a.b.c.d USING s ON true WHEN MATCHED THEN DELETE", "refused: four-part MERGE target")]
	[InlineData("DELETE t WHERE a = 1",                                   "refused: DELETE without FROM")]
	[InlineData("MERGE INTO t USING s ON true",                           "refused: MERGE without WHEN")]
	[InlineData("INSERT INTO t VALUES (1) ON CONFLICT DO UPDATE",         "refused: DO UPDATE without SET")]
	public void A_data_change_builds_its_record(string text, string expected)
	{
		if (expected.StartsWith("refused", StringComparison.Ordinal))
		{
			Assert.False(PostgreSqlParser.TryParseStatement(text).IsSuccess);
			return;
		}

		var made  = Holds(text);
		var holds = expected switch
		{
			"insert values"            => made is Statement.Insert { Columns.Count: 2, SourceValue: InsertSource.Values { Rows.Count: 2 }, Alias: null, Conflicts.Count: 0, Returning: null },
			"insert alias default returning" => made is Statement.Insert { Alias.Name.Text: "x", SourceValue: InsertSource.DefaultValues, Returning: ReturningClause.Returned { Items: [SelectColumn.All], OldAlias: null } },
			"insert overriding query"  => made is Statement.Insert { Override: OverrideKind.SystemValue, SourceValue: InsertSource.Subquery { Query: Query.Select } },
			"insert bracketed query"   => made is Statement.Insert { Columns.Count: 0, SourceValue: InsertSource.Subquery { Query: Query.Parenthesized } },
			"on conflict columns do update" => made is Statement.Insert { Conflicts: [{ Target: ConflictTarget.Columns { Items: [{ Key: Expression.Reference }, { Key: Expression.Parenthesized }, { Key: Expression.Invocation }], Where: not null }, Resolution: ConflictResolution.Update { Assignments.Count: 1, Where: not null } }] },
			"on conflict constraint do nothing" => made is Statement.Insert { Conflicts: [{ Target: ConflictTarget.Constraint { Name.Text: "c" }, Resolution: ConflictResolution.Nothing }] },
			"on conflict without target" => made is Statement.Insert { Conflicts: [{ Target: null, Resolution: ConflictResolution.Nothing }] },
			"returning old and new"    => made is Statement.Insert { Returning: ReturningClause.Returned { OldAlias.Text: "o", NewAlias.Text: "n", Items.Count: 2 } },
			"insert with"              => made is Statement.Insert { With.Items.Count: 1, SourceValue: InsertSource.Subquery },
			"update"                   => made is Statement.Update { Target: TableSource.Named { Only: true }, Alias: { Name.Text: "u", AsKeyword: true }, Assignments: [{ Parenthesized: false }, { Parenthesized: true, Targets.Count: 2 }, { Targets: [{ Index: not null }] }, { Targets: [{ MutationPath.Count: 1 }] }], From.Sources.Count: 1, Where: not null, CurrentOf: null, Returning: ReturningClause.Returned },
			"update current of"        => made is Statement.Update { Alias: { Name.Text: "u", AsKeyword: false }, Where: null, CurrentOf.Name: not null },
			"delete using"             => made is Statement.Delete { Target: TableSource.Named { Star: true }, Using.Sources.Count: 2, Where: not null, Returning: ReturningClause.Returned },
			"delete current of"        => made is Statement.Delete { CurrentOf: not null, Using: null },
			"merge"                    => made is Statement.Merge { Alias.Name.Text: "x", Clauses: [MergeClause.Matched { Condition: not null, Action: MergeMatchedAction.Update }, MergeClause.Matched { Condition: null, Action: MergeMatchedAction.Delete }, MergeClause.NotMatchedBySource { Action: MergeMatchedAction.DoNothing }, MergeClause.NotMatched { ByTarget: true, Action: { Columns.Count: 2, Override: OverrideKind.UserValue, Values.Count: 2 } }, MergeClause.NotMatched { ByTarget: false, Action.DoNothing: true }], Returning: ReturningClause.Returned { Items.Count: 2 } },
			"merge insert default values" => made is Statement.Merge { Clauses: [MergeClause.NotMatched { Action: { DefaultValues: true, DoNothing: false } }] },
			"insert into a column named values" => made is Statement.Insert { Columns: [{ Text: "values" }], SourceValue: InsertSource.Values { Rows.Count: 1 } },
			"insert into columns named values and a" => made is Statement.Insert { Columns.Count: 2, SourceValue: InsertSource.Subquery { Query: Query.Select } },
			"insert bracketed values"  => made is Statement.Insert { Columns.Count: 0, SourceValue: InsertSource.Subquery { Query: Query.Parenthesized { Query: Query.Select { Values.Count: 1 } } } },
			"on conflict index elements" => made is Statement.Insert { Conflicts: [{ Target: ConflictTarget.Columns { Items: [
				{ Key: Expression.Reference, Collation.Name: [{ Text: "\"C\"" }], OperatorClass: [{ Text: "text_pattern_ops" }], Direction: SortDirection.Desc, Nulls: null },
				{ Key: Expression.Invocation, OperatorClass: [{ Text: "nulls" }], Direction: null },
				{ OperatorClass: [{ Text: "gist_ops" }], OperatorClassOptions: [{ Name.Text: "siglen", Value: LiteralValue.Numeric }], Nulls: NullOrdering.First }] } }] },
			"on conflict key word calls" => made is Statement.Insert { Conflicts: [{ Target: ConflictTarget.Columns { Items: [{ Key: Expression.Invocation }, { Key: Expression.Parenthesized }, { Key: Expression.Cast }, { Key: Expression.Extract }] } }] },
			"insert into a three-part name" => made is Statement.Insert { Target: TableSource.Named { Name.Count: 3 } },
			"data change body in insert" => made is Statement.Insert { With.Items: [{ Query: Query.DataChange { Change: Statement.Delete { Returning: not null } } }] },
			"data change body in select" => made is Statement.Select { Query.With.Items: [{ Query: Query.DataChange { Change: Statement.Update } }] },
			"truncate"                 => made is Statement.TruncateTable { TableOmitted: true, Target: TableSource.Named { Name.Count: 1 }, Others: [TableSource.Named { Only: true }, TableSource.Named { Star: true }], Identity: IdentityRestart.Restart, Behavior: DropBehavior.Cascade },
			"truncate table"           => made is Statement.TruncateTable { TableOmitted: false, Others.Count: 0, Identity: IdentityRestart.Continue, Behavior: DropBehavior.Restrict },
			_ => throw new ArgumentOutOfRangeException(nameof(expected)),
		};

		Assert.True(holds, $"{text}: {made}");
	}

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
	[InlineData("SELECT a FROM t ORDER BY a OFFSET 5 ROWS FETCH FIRST 10 ROWS ONLY")]
	[InlineData("SELECT a FROM t FOR UPDATE OF t")]
	[InlineData("SELECT a FROM t FOR READ ONLY")]
	[InlineData("WITH RECURSIVE r (n) AS (SELECT 1 FROM t) SELECT n FROM r")]
	[InlineData("SELECT a FROM t GROUP BY ROLLUP (a, b) HAVING count(*) > 1")]
	[InlineData("SELECT a FROM t GROUP BY (a, b)")]
	[InlineData("SELECT a FROM t WINDOW w AS (PARTITION BY a ORDER BY b ROWS UNBOUNDED PRECEDING)")]
	[InlineData("VALUES (1, 2), (3, 4)")]
	[InlineData("TABLE t")]
	[InlineData("SELECT * FROM UNNEST(a) WITH ORDINALITY AS u")]
	[InlineData("(SELECT 1 FROM t) UNION (SELECT 2 FROM u) ORDER BY 1")]
	[InlineData("SELECT (a, b) OVERLAPS (c, d) FROM t")]
	[InlineData("INSERT INTO t (a, b) VALUES (1, 2)")]
	[InlineData("INSERT INTO t DEFAULT VALUES")]
	[InlineData("UPDATE t SET a = 1 WHERE b = 2")]
	[InlineData("UPDATE t SET (a, b) = (1, 2) WHERE c = 3")]
	[InlineData("DELETE FROM t WHERE a = 1")]
	[InlineData("MERGE INTO t USING s ON t.a = s.a WHEN MATCHED THEN DELETE WHEN NOT MATCHED THEN INSERT (a) VALUES (s.a)")]
	[InlineData("TRUNCATE TABLE t RESTART IDENTITY")]
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

	/// <summary>A query a parenthesis in FROM made from a bare subquery has a span: the parentheses', or theirs to the last clause.</summary>
	[Theory]
	[InlineData("SELECT * FROM ((SELECT 1)) x",                                         "(SELECT 1)")]
	[InlineData("SELECT * FROM ((SELECT 1) UNION (SELECT 2)) x",                        "(SELECT 1) UNION (SELECT 2)")]
	[InlineData("SELECT * FROM ((SELECT 1) UNION (SELECT 2) ORDER BY 1 LIMIT 1 FOR UPDATE) x", "(SELECT 1) UNION (SELECT 2) ORDER BY 1 LIMIT 1 FOR UPDATE")]
	public void A_query_a_parenthesis_in_FROM_made_is_located(string text, string expected)
	{
		var located = (Statement.Select)PostgreSqlParser.Located.ParseStatement(text);
		var source  = Assert.IsType<TableSource.Subquery>(((Query.Select)located.Query).From!.Sources[0]);

		Assert.True(source.Query.Span.Known, $"{text}: {source.Query}");
		Assert.Equal(expected, text.Substring(source.Query.Span.At, source.Query.Span.Length));
	}

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
