using System;
using System.Linq;

using DotGram.Sql.Ast;
using DotGram.Sql.Standard;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// What <see cref="SqlStandardParser"/> builds: the SQL:2023 tree (docs/design/sql-ast.md), held
/// node by node to what was written.
/// </summary>
/// <remarks>
/// <see cref="SqlStandardParserTests"/> holds the grammar to the BNF — what it reads. These hold the
/// tree to the text — what it keeps: a name's parts and its quotes, which word was written where
/// two mean the same.
/// </remarks>
public sealed class SqlStandardTreeTests
{
	// ── §5.4 Names and identifiers ─────────────────────────────────────────────

	[Theory]
	[InlineData("abc", "abc", IdentifierStyle.Regular)]
	[InlineData("\"a b\"", "\"a b\"", IdentifierStyle.Delimited)]
	[InlineData("U&\"a\\0041\"", "U&\"a\\0041\"", IdentifierStyle.UnicodeDelimited)]
	public void An_identifier_keeps_its_spelling_and_its_style(string input, string text, IdentifierStyle style)
	{
		var identifier = SqlStandardParser.ParseIdentifier(input);

		Assert.Equal(text, identifier.Text);
		Assert.Equal(style, identifier.Style);
		Assert.Null(identifier.UnicodeEscape);
	}

	/// <summary>
	/// The escape specifier is a part of the token, with nothing between it and the name, and the BNF
	/// reads the default character only — the Syntax Rules choose another.
	/// </summary>
	[Fact]
	public void A_Unicode_identifier_keeps_the_escape_character_it_names()
	{
		var identifier = SqlStandardParser.ParseIdentifier("U&\"a\\0041\"UESCAPE'\\'");

		Assert.Equal("U&\"a\\0041\"", identifier.Text);
		Assert.Equal('\\', identifier.UnicodeEscape);
	}

	[Theory]
	[InlineData("a", new[] { "a" })]
	[InlineData("a.b.c.d", new[] { "a", "b", "c", "d" })]
	[InlineData("a . \"b\"", new[] { "a", "\"b\"" })]
	public void An_identifier_chain_is_its_parts(string input, string[] parts) =>
		Assert.Equal(parts, SqlStandardParser.ParseIdentifierChain(input).Parts.Select(one => one.Text));

	[Theory]
	[InlineData("t", new[] { "t" })]
	[InlineData("c.s.t", new[] { "c", "s", "t" })]
	[InlineData("MODULE.t", new[] { "MODULE", "t" })]
	public void A_table_name_is_its_qualifiers_and_its_name(string input, string[] parts) =>
		Assert.Equal(parts, SqlStandardParser.ParseTableName(input).Parts.Select(one => one.Text));

	[Theory]
	[InlineData("a.b", new[] { "a", "b" })]
	[InlineData("module.x.y", new[] { "module", "x", "y" })]
	public void A_column_reference_is_a_reference_to_a_name(string input, string[] parts)
	{
		// Qualified: inside DotGram.Sql, `Expression` is the tree T-SQL builds.
		var reference = Assert.IsType<DotGram.Sql.Ast.Expression.Reference>(SqlStandardParser.ParseColumnReference(input));

		Assert.Equal(parts, reference.Name.Parts.Select(one => one.Text));
	}

	// ── §5.3 Literals ───────────────────────────────────────────────────────────

	[Theory]
	[InlineData("1", "1", NumericLiteralKind.DecimalInteger)]
	[InlineData("-1.5e3", "-1.5e3", NumericLiteralKind.Approximate)]
	[InlineData("+.5", "+.5", NumericLiteralKind.Decimal)]
	[InlineData("0x1F", "0x1F", NumericLiteralKind.HexInteger)]
	[InlineData("0o17", "0o17", NumericLiteralKind.OctalInteger)]
	[InlineData("0b101", "0b101", NumericLiteralKind.BinaryInteger)]
	public void A_number_keeps_its_sign_and_its_kind(string input, string text, NumericLiteralKind kind)
	{
		var literal = Assert.IsType<LiteralValue.Numeric>(SqlStandardParser.ParseLiteral(input));

		Assert.Equal(text, literal.Text);
		Assert.Equal(kind, literal.Kind);
	}

	[Theory]
	[InlineData("'abc'", "'abc'", StringLiteralKind.Character, null)]
	[InlineData("'a' 'b'", "'a' 'b'", StringLiteralKind.Character, null)]
	[InlineData("N'a'", "'a'", StringLiteralKind.National, null)]
	[InlineData("_latin1'abc'", "'abc'", StringLiteralKind.Character, "latin1")]
	[InlineData("_s.utf8'abc'", "'abc'", StringLiteralKind.Character, "s.utf8")]
	[InlineData("U&'a\\0041'", "'a\\0041'", StringLiteralKind.Unicode, null)]
	public void A_string_keeps_its_quotes_its_kind_and_its_character_set(string input, string text, StringLiteralKind kind, string? characterSet)
	{
		var literal = Assert.IsType<LiteralValue.String>(SqlStandardParser.ParseLiteral(input));

		Assert.Equal(text, literal.Text);
		Assert.Equal(kind, literal.Kind);
		Assert.Equal(characterSet, literal.CharacterSet is null ? null : string.Join(".", literal.CharacterSet.Name.Parts.Select(one => one.Text)));
		Assert.Null(literal.UnicodeEscape);
	}

	[Fact]
	public void A_Unicode_string_keeps_its_escape_character()
	{
		var literal = Assert.IsType<LiteralValue.String>(SqlStandardParser.ParseLiteral("U&'a\\0041'UESCAPE'\\'"));

		Assert.Equal("'a\\0041'", literal.Text);
		Assert.Equal('\\', literal.UnicodeEscape);
	}

	[Fact]
	public void A_binary_string_keeps_its_quotes() =>
		Assert.Equal("'0A 1B'", Assert.IsType<LiteralValue.Binary>(SqlStandardParser.ParseLiteral("X'0A 1B'")).Text);

	[Theory]
	[InlineData("DATE '2020-01-01'", DateTimeLiteralKind.Date, "'2020-01-01'")]
	[InlineData("TIME '10:00:00'", DateTimeLiteralKind.Time, "'10:00:00'")]
	[InlineData("TIMESTAMP '2020-01-01 10:00:00'", DateTimeLiteralKind.Timestamp, "'2020-01-01 10:00:00'")]
	public void A_datetime_keeps_its_kind_and_its_string(string input, DateTimeLiteralKind kind, string text)
	{
		var literal = Assert.IsType<LiteralValue.DateTime>(SqlStandardParser.ParseLiteral(input));

		Assert.Equal(kind, literal.Kind);
		Assert.Equal(text, literal.Text);
	}

	[Fact]
	public void An_interval_keeps_its_sign_its_string_and_its_qualifier()
	{
		var literal = Assert.IsType<LiteralValue.Interval>(SqlStandardParser.ParseLiteral("INTERVAL -'1:30' HOUR(2) TO MINUTE"));

		Assert.Equal("'1:30'", literal.Text);
		Assert.Equal(UnaryOperator.Minus, literal.Sign);
		Assert.Equal(new IntervalQualifier(DateTimeField.Hour, 2, DateTimeField.Minute), literal.Qualifier with { });
		Assert.Null(SqlStandardParser.ParseLiteral("INTERVAL '1' DAY") is LiteralValue.Interval { Sign: { } } ? "signed" : null);
	}

	[Theory]
	[InlineData("TRUE", BooleanLiteral.True)]
	[InlineData("false", BooleanLiteral.False)]
	[InlineData("Unknown", BooleanLiteral.Unknown)]
	public void A_truth_value_is_a_boolean_literal(string input, BooleanLiteral value) =>
		Assert.Equal(value, Assert.IsType<LiteralValue.Boolean>(SqlStandardParser.ParseLiteral(input)).Value);

	// ── §6.1 Data types ─────────────────────────────────────────────────────────

	[Fact]
	public void A_character_type_keeps_its_spelling_length_units_character_set_and_collation()
	{
		var type = Assert.IsType<DataType.Character>(SqlStandardParser.ParseDataType("CHAR VARYING(10 CHARACTERS) CHARACTER SET s.latin1 COLLATE c"));

		Assert.Equal(CharacterTypeKind.CharVarying, type.Kind);
		Assert.Equal(10, type.Length);
		Assert.Equal(LengthUnit.Characters, type.Unit);
		Assert.Equal(new[] { "s", "latin1" }, type.CharacterSet!.Name.Parts.Select(one => one.Text));
		Assert.Equal(new[] { "c" }, type.Collation!.Name.Parts.Select(one => one.Text));
	}

	[Theory]
	[InlineData("CHARACTER", CharacterTypeKind.Character)]
	[InlineData("VARCHAR(5)", CharacterTypeKind.Varchar)]
	[InlineData("CLOB", CharacterTypeKind.Clob)]
	[InlineData("NATIONAL CHAR VARYING(3)", CharacterTypeKind.NationalCharVarying)]
	[InlineData("NCHAR LARGE OBJECT", CharacterTypeKind.NcharLargeObject)]
	[InlineData("national character large object", CharacterTypeKind.NationalCharacterLargeObject)]
	public void A_character_type_is_the_spelling_written(string input, CharacterTypeKind kind) =>
		Assert.Equal(kind, Assert.IsType<DataType.Character>(SqlStandardParser.ParseDataType(input)).Kind);

	[Fact]
	public void A_large_object_keeps_its_multiplier()
	{
		var type = Assert.IsType<DataType.Character>(SqlStandardParser.ParseDataType("CHAR LARGE OBJECT(2K OCTETS)"));

		Assert.Equal(new LargeObjectSize(2, 'K'), type.LargeObject);
		Assert.Equal(LengthUnit.Octets, type.Unit);
		Assert.Equal(new LargeObjectSize(4, 'm'), Assert.IsType<DataType.Binary>(SqlStandardParser.ParseDataType("BLOB(4 m)")).LargeObject);
	}

	[Theory]
	[InlineData("DECIMAL(10, 2)", NumericTypeKind.Decimal, 10, 2)]
	[InlineData("INT", NumericTypeKind.Int, null, null)]
	[InlineData("DOUBLE PRECISION", NumericTypeKind.DoublePrecision, null, null)]
	[InlineData("FLOAT(0x10)", NumericTypeKind.Float, 16, null)]
	public void A_numeric_type_keeps_its_spelling_precision_and_scale(string input, NumericTypeKind kind, int? precision, int? scale)
	{
		var type = Assert.IsType<DataType.Numeric>(SqlStandardParser.ParseDataType(input));

		Assert.Equal(kind, type.Kind);
		Assert.Equal(precision, type.Precision);
		Assert.Equal(scale, type.Scale);
	}

	[Fact]
	public void A_datetime_type_keeps_its_precision_and_its_time_zone()
	{
		var type = Assert.IsType<DataType.DateTime>(SqlStandardParser.ParseDataType("TIMESTAMP(3) WITHOUT TIME ZONE"));

		Assert.Equal(DateTimeTypeKind.Timestamp, type.Kind);
		Assert.Equal(3, type.Precision);
		Assert.Equal(TimeZoneMode.Without, type.TimeZone);
		Assert.Null(Assert.IsType<DataType.DateTime>(SqlStandardParser.ParseDataType("TIME")).TimeZone);
	}

	[Fact]
	public void A_collection_type_wraps_what_stands_before_it()
	{
		var multiset = Assert.IsType<DataType.Multiset>(SqlStandardParser.ParseDataType("INT ARRAY??(10??) MULTISET"));
		var array    = Assert.IsType<DataType.Array>(multiset.ElementType);

		Assert.Equal(10, array.MaximumCardinality);
		Assert.True(array.Trigraphs);
		Assert.IsType<DataType.Numeric>(array.ElementType);
	}

	[Fact]
	public void Row_reference_interval_and_user_defined_types_have_their_parts()
	{
		var row = Assert.IsType<DataType.Row>(SqlStandardParser.ParseDataType("ROW(a INT, b s.t)"));

		Assert.Equal(new[] { "a", "b" }, row.Fields.Select(one => one.Name.Text));
		Assert.Equal(new[] { "s", "t" }, Assert.IsType<DataType.UserDefined>(row.Fields[1].Type).Name.Parts.Select(one => one.Text));

		var reference = Assert.IsType<DataType.Reference>(SqlStandardParser.ParseDataType("REF(t) SCOPE s.u"));

		Assert.Equal(new[] { "s", "u" }, reference.Scope!.Parts.Select(one => one.Text));
		Assert.Equal(new IntervalQualifier(DateTimeField.Second, 2, null, 3), Assert.IsType<DataType.Interval>(SqlStandardParser.ParseDataType("INTERVAL SECOND(2, 3)")).Qualifier);
	}

	// ── §6.28 Value expressions ─────────────────────────────────────────────────

	[Theory]
	[InlineData("a + b * c", "Binary(a, Add, Binary(b, Multiply, c))")]
	[InlineData("a - b - c", "Binary(Binary(a, Subtract, b), Subtract, c)")]
	[InlineData("a * b / c + d", "Binary(Binary(Binary(a, Multiply, b), Divide, c), Add, d)")]
	[InlineData("-a * b", "Binary(Unary(Minus, a), Multiply, b)")]
	[InlineData("a || b || c", "Binary(Binary(a, Concatenate, b), Concatenate, c)")]
	[InlineData("x MULTISET UNION ALL y MULTISET INTERSECT z", "MultisetOperation(x, Union, All, MultisetOperation(y, Intersect, null, z))")]
	[InlineData("(a)", "Parenthesized(a)")]
	[InlineData("(a, b)", "Row([a, b], false)")]
	[InlineData("ROW(a, b)", "Row([a, b], true)")]
	[InlineData("a.b.c", "a.b.c")]
	[InlineData("a.b.m(1)", "Invocation(a.b.m, [Argument(1, null, false)])")]
	[InlineData("a.b.c.m(1)", "Member(a.b.c, Dot, m, [Argument(1, null, false)])")]
	[InlineData("a[1]", "Element(a, 1, null, false)")]
	[InlineData("a??(1??)", "Element(a, 1, null, true)")]
	[InlineData("x DAY", "IntervalQualified(x, IntervalQualifier(Day, null, null, null))")]
	[InlineData("x AT LOCAL", "AtTimeZone(x, null)")]
	[InlineData("x AT TIME ZONE y", "AtTimeZone(x, y)")]
	[InlineData("x COLLATE c", "Collate(x, CollationName(c))")]
	[InlineData(":h INDICATOR :i", "Parameter(Host, h, Indicator: i, IndicatorKeyword: true)")]
	[InlineData(":h :i", "Parameter(Host, h, Indicator: i)")]
	[InlineData("?", "Parameter(Dynamic, null)")]
	[InlineData("CURRENT_USER", "Current(CurrentUser, null, null)")]
	[InlineData("USER", "Current(User, null, null)")]
	[InlineData("CAST(a AS INT)", "Cast(a, Numeric(Int, null, null), null)")]
	[InlineData("CASE a WHEN 1, 2 THEN 'x' WHEN < 5 THEN 'y' ELSE NULL END", "Case(a, [CaseWhen([1, 2], 'x'), CaseWhen([Comparison(CaseOperand(), Less, 5)], 'y')], NULL)")]
	[InlineData("CASE WHEN a THEN 1 END", "Case(null, [CaseWhen([a], 1)], null)")]
	[InlineData("NULLIF(a, b)", "Invocation(NULLIF, [Argument(a, null, false), Argument(b, null, false)])")]
	[InlineData("f(a => 1, b)", "Invocation(f, [Argument(1, a, true), Argument(b, null, false)])")]
	[InlineData("T::m(1)", "Member(T, StaticMethod, m, [Argument(1, null, false)])")]
	[InlineData("NEXT VALUE FOR s.q", "NextValue(s.q)")]
	[InlineData("ARRAY[1, 2]", "Array([1, 2], false)")]
	public void A_value_expression_is_built_as_written(string input, string tree) =>
		Assert.Equal(tree, Show(SqlStandardParser.ParseValueExpression(input)));

	// ── §8 Predicates ───────────────────────────────────────────────────────────

	[Theory]
	[InlineData("a = 1 AND NOT b IS NULL OR c", "Binary(Binary(Comparison(a, Equal, 1), And, Unary(Not, IsNull(b, false))), Or, c)")]
	[InlineData("a BETWEEN SYMMETRIC 1 AND 2", "Between(a, false, Symmetric, 1, 2)")]
	[InlineData("a NOT IN (1, 2)", "In(a, true, Values([1, 2]))")]
	[InlineData("a NOT LIKE 'x' ESCAPE '!'", "Like(a, true, Like, 'x', '!', null)")]
	[InlineData("a IS NOT DISTINCT FROM b", "IsDistinct(a, true, b)")]
	[InlineData("a IS NOT TRUE", "IsTruth(a, true, True)")]
	[InlineData("x IS JSON OBJECT WITH UNIQUE KEYS", "JsonPredicate(x, null, false, Object, WithUniqueKeys)")]
	[InlineData("(a, b) OVERLAPS (c, d)", "Overlaps(Row([a, b], false), Row([c, d], false))")]
	[InlineData("PERIOD(a, b) CONTAINS c", "PeriodPredicate(Range(a, b), Contains, Point(c))")]
	[InlineData("p IMMEDIATELY PRECEDES q", "PeriodPredicate(Reference(p), ImmediatelyPrecedes, Period(Reference(q)))")]
	[InlineData("m NOT MEMBER OF n", "MemberOf(m, true, true, n)")]
	public void A_search_condition_is_built_as_written(string input, string tree) =>
		Assert.Equal(tree, Show(SqlStandardParser.ParseSearchCondition(input)));

	// ── §6.30–6.36 Value functions, §6.10 Windows, §10.9 Aggregates, JSON ───────

	[Theory]
	[InlineData("COUNT(*)", "Invocation(COUNT, [Argument(Asterisk(), null, false)])")]
	[InlineData("SUM(DISTINCT a) FILTER (WHERE a > 1)", "Invocation(SUM, [Argument(a, null, false)], Quantifier: Distinct, Filter: FilterClause(Comparison(a, Greater, 1)))")]
	[InlineData("RANK() OVER (PARTITION BY a ORDER BY b DESC NULLS LAST ROWS BETWEEN 1 PRECEDING AND CURRENT ROW EXCLUDE TIES)",
		"Invocation(RANK, [], Over: Specification(WindowSpecification(null, [a], OrderByClause([SortItem(b, Desc, Last)]), WindowFrame(Rows, Between(WindowFrameBound(Preceding, 1), WindowFrameBound(CurrentRow, null)), Ties, null))))")]
	[InlineData("LAG(a, 1, 0) IGNORE NULLS OVER w", "Invocation(LAG, [Argument(a, null, false), Argument(1, null, false), Argument(0, null, false)], Nulls: IgnoreNulls, Over: NameRef(w))")]
	[InlineData("NTH_VALUE(a, 2) FROM LAST OVER w", "Invocation(NTH_VALUE, [Argument(a, null, false), Argument(2, null, false)], From: Last, Over: NameRef(w))")]
	[InlineData("LISTAGG(DISTINCT a, ', ' ON OVERFLOW TRUNCATE '...' WITH COUNT) WITHIN GROUP (ORDER BY a)",
		"Invocation(LISTAGG, [Argument(a, null, false), Argument(', ', null, false)], Quantifier: Distinct, Overflow: ListaggOverflow(true, '...', true), WithinGroup: WithinGroupClause(OrderByClause([SortItem(a, null, null)])))")]
	[InlineData("ARRAY_AGG(a ORDER BY b)", "Invocation(ARRAY_AGG, [Argument(a, null, false)], OrderBy: OrderByClause([SortItem(b, null, null)]))")]
	[InlineData("RUNNING FIRST(a, 1)", "Invocation(FIRST, [Argument(a, null, false), Argument(1, null, false)], Semantics: Running)")]
	[InlineData("GROUPING(a, b)", "Invocation(GROUPING, [Argument(a, null, false), Argument(b, null, false)])")]
	[InlineData("SUBSTRING(a FROM 1 FOR 2 USING OCTETS)", "Substring(a, 1, 2, Octets)")]
	[InlineData("TRIM(LEADING 'x' FROM a)", "Trim(Leading, 'x', true, a)")]
	[InlineData("TRIM(a)", "Trim(null, null, false, a)")]
	[InlineData("EXTRACT(TIMEZONE_HOUR FROM a)", "Extract(TimezoneHour, a)")]
	[InlineData("POSITION('a' IN b)", "Position('a', b, null)")]
	[InlineData("CHAR_LENGTH(a USING CHARACTERS)", "Length(CharLength, a, Characters)")]
	[InlineData("CURRENT_TIMESTAMP(3)", "Current(Timestamp, null, 3)")]
	[InlineData("ABS(a)", "Invocation(ABS, [Argument(a, null, false)])")]
	[InlineData("JSON_VALUE(a, '$.x' RETURNING INTEGER NULL ON EMPTY ERROR ON ERROR)", "JsonValue(JsonApiCommon(a, '$.x', null, [], null), Numeric(Integer, null, null), Null(), Error())")]
	[InlineData("JSON_OBJECT(KEY 'a' VALUE 1, 'b' : 2 ABSENT ON NULL)", "JsonObject([JsonMember('a', 1, KeyValue, null), JsonMember('b', 2, Colon, null)], AbsentOnNull, null, null)")]
	[InlineData("a[$ to last]", "JsonAccessor(a, Array([JsonSubscript(Variable(Context, null), Variable(Last, null))]))")]
	[InlineData("a.double()", "Member(a, Dot, double, [])")]
	public void A_function_is_built_as_written(string input, string tree) =>
		Assert.Equal(tree, Show(SqlStandardParser.ParseValueExpression(input)));

	[Fact]
	public void A_window_frame_keeps_the_row_pattern_after_it()
	{
		var measure = Assert.IsType<DotGram.Sql.Ast.Expression.Invocation>(SqlStandardParser.ParseValueExpression("m OVER (ROWS BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING PATTERN (A B*) DEFINE A AS a > 1)"));
		var frame   = Assert.IsType<WindowReference.Specification>(measure.Over).Value.Frame!;

		Assert.True(measure.WithoutParentheses);
		Assert.Equal("Sequence([Variable(A), Quantified(Variable(B), RowPatternQuantifier(ZeroOrMore, null, null, false))])", Show(frame.Pattern!.Pattern));
		Assert.Equal("[RowPatternDefinition(A, Comparison(a, Greater, 1))]", Show(frame.Pattern.Definitions));
	}

	[Fact]
	public void A_JSON_exists_predicate_keeps_what_happens_on_error() =>
		Assert.Equal("JsonExists(JsonApiCommon(a, '$.x', null, [], null), True)", Show(SqlStandardParser.ParseSearchCondition("JSON_EXISTS(a, '$.x' TRUE ON ERROR)")));

	// ── §7 Query expressions ───────────────────────────────────────────────────

	[Theory]
	[InlineData("SELECT a, b AS c, d e FROM t",
		"Select(Items: [ExpressionItem(a, null, false), ExpressionItem(b, c, true), ExpressionItem(d, e, false)], From: FromClause([Named(t)]))")]
	[InlineData("SELECT DISTINCT t.*, u.* AS (x, y) FROM t, u",
		"Select(Quantifier: Distinct, Items: [QualifiedAll(t, null), QualifiedAll(u, [x, y])], From: FromClause([Named(t), Named(u)]))")]
	[InlineData("SELECT * FROM t WHERE a = 1 GROUP BY ROLLUP (a, (b, c)), () HAVING COUNT(*) > 1",
		"Select(Items: [All()], From: FromClause([Named(t)]), Where: Comparison(a, Equal, 1), GroupBy: GroupByClause(null, [Rollup([Ordinary([a], false), Ordinary([b, c], true)]), Empty()]), Having: Comparison(Invocation(COUNT, [Argument(Asterisk(), null, false)]), Greater, 1))")]
	[InlineData("SELECT a FROM t UNION ALL SELECT b FROM u INTERSECT SELECT c FROM v ORDER BY 1",
		"Select(Items: [ExpressionItem(a, null, false)], From: FromClause([Named(t)]), SetOperations: [SetOperation(Operator: Union, Quantifier: All, Operand: Select(Select(Items: [ExpressionItem(b, null, false)], From: FromClause([Named(u)]), SetOperations: [SetOperation(Operator: Intersect, Operand: Select(Select(Items: [ExpressionItem(c, null, false)], From: FromClause([Named(v)]))))])))], OrderBy: OrderByClause([SortItem(1, null, null)]))")]
	[InlineData("SELECT a FROM t INTERSECT SELECT b FROM u EXCEPT CORRESPONDING BY (a) TABLE v",
		"Select(SetOperations: [SetOperation(Operator: Except, Corresponding: CorrespondingClause(true, [a]), Operand: Table(v))], Body: Select(Select(Items: [ExpressionItem(a, null, false)], From: FromClause([Named(t)]), SetOperations: [SetOperation(Operator: Intersect, Operand: Select(Select(Items: [ExpressionItem(b, null, false)], From: FromClause([Named(u)]))))])))")]
	[InlineData("(SELECT a FROM t ORDER BY a) ORDER BY b OFFSET 1 ROW FETCH NEXT 10 PERCENT ROWS WITH TIES",
		"Select(OrderBy: OrderByClause([SortItem(b, null, null)]), Offset: OffsetClause(1, Row), Fetch: FetchClause(Next, 10, true, Rows, WithTies), Body: Select(Select(Items: [ExpressionItem(a, null, false)], From: FromClause([Named(t)]), OrderBy: OrderByClause([SortItem(a, null, null)]), Parentheses: 1)))")]
	[InlineData("((SELECT a FROM t))", "Select(Items: [ExpressionItem(a, null, false)], From: FromClause([Named(t)]), Parentheses: 2)")]
	[InlineData("VALUES (1, 2), ROW(3, 4), 5", "Select(Body: Values([RowValue([1, 2], false), RowValue([3, 4], true), RowValue([5], false)]))")]
	[InlineData("WITH RECURSIVE r (n) AS (VALUES 1) SEARCH DEPTH FIRST BY n SET s SELECT n FROM r",
		"Select(With: WithClause(true, [CommonTableExpression(r, [n], Select(Body: Values([RowValue([1], false)])), SearchClause(DepthFirst, [n], s), null)]), Items: [ExpressionItem(n, null, false)], From: FromClause([Named(r)]))")]
	public void A_query_is_built_as_written(string input, string tree) =>
		Assert.Equal(tree, Show(SqlStandardParser.ParseQueryExpression(input)));

	[Theory]
	[InlineData("t AS x (a) LEFT OUTER JOIN u USING (a) CROSS JOIN v TABLESAMPLE BERNOULLI (10)",
		"Join(Left: Join(Left: Named(t, Alias: Alias(x, [a], true)), Right: Named(u), Kind: Left, OuterKeyword: true, Specification: Using([a], null)), Right: Named(v, Sample: SampleClause(Bernoulli, 10, null)), Kind: Cross)")]
	[InlineData("t PARTITION BY (a) NATURAL FULL JOIN u", "Join(Left: Named(t), Right: Named(u), Kind: Full, Natural: true, LeftPartition: [a])")]
	[InlineData("t JOIN u JOIN v ON b ON a", "Join(Left: Named(t), Right: Join(Left: Named(u), Right: Named(v), Specification: On(b)), Specification: On(a))")]
	[InlineData("(t JOIN u ON a = b)", "Parenthesized(Join(Left: Named(t), Right: Named(u), Specification: On(Comparison(a, Equal, b))))")]
	[InlineData("LATERAL (SELECT 1 FROM t) AS s", "Subquery(Select(Items: [ExpressionItem(1, null, false)], From: FromClause([Named(t)])), Lateral: true, Alias: Alias(s, null, true))")]
	[InlineData("ONLY (t)", "Named(t, Only: true)")]
	[InlineData("t FOR SYSTEM_TIME FROM a TO b s", "Named(t, SystemTime: FromTo(a, b), Alias: Alias(s, null, false))")]
	[InlineData("UNNEST(a, b) WITH ORDINALITY AS u", "Unnest([a, b], true, Alias(u, null, true))")]
	[InlineData("TABLE (f(a))", "TableFunction(Invocation(f, [Argument(a, null, false)]), null)")]
	[InlineData("t MATCH_RECOGNIZE (PATTERN (A) DEFINE A AS a > 1) AS m",
		"RowPatternRecognition(Named(t), RowPatternClause([], null, [], null, null, null, Variable(A), [], [RowPatternDefinition(A, Comparison(a, Greater, 1))]), Alias(m, null, true))")]
	[InlineData("JSON_TABLE(j, '$' COLUMNS (id FOR ORDINALITY, a INTEGER PATH '$.a' DEFAULT 0 ON EMPTY, NESTED PATH '$.b' COLUMNS (b INTEGER FORMAT JSON)) ERROR ON ERROR) AS jt",
		"JsonTable(JsonTableDefinition(JsonApiCommon(j, '$', null, [], null), [Ordinality(id), Regular(a, Numeric(Integer, null, null), '$.a', Default(0), null), Nested('$.b', null, [Formatted(b, Numeric(Integer, null, null), JsonRepresentation(null, null), null, null, null, null, null)], true)], null, Error, false), Alias(jt, null, true))")]
	public void A_table_reference_is_built_as_written(string input, string tree) =>
		Assert.Equal(tree, Show(SqlStandardParser.ParseTableReference(input)));

	[Theory]
	[InlineData("EXISTS (SELECT 1 FROM t)", "Exists(Select(Items: [ExpressionItem(1, null, false)], From: FromClause([Named(t)])))")]
	[InlineData("a IN (VALUES 1)", "In(a, false, Query(Select(Body: Values([RowValue([1], false)]))))")]
	[InlineData("a = ANY (TABLE t)", "QuantifiedComparison(a, Equal, Any, Select(Body: Table(t)))")]
	public void A_subquery_in_a_predicate_is_built_as_written(string input, string tree) =>
		Assert.Equal(tree, Show(SqlStandardParser.ParseSearchCondition(input)));

	// ── §14 Data change statements ─────────────────────────────────────────────

	[Theory]
	[InlineData("INSERT INTO t (a, b) OVERRIDING SYSTEM VALUE VALUES (1, DEFAULT), ROW(NULL, 2), (DEFAULT), 3",
		"Insert(Target: t, Columns: [a, b], Override: SystemValue, SourceValue: Values([RowValue([1, Default()], false), RowValue([NULL, 2], true), RowValue([Parenthesized(Default())], false), RowValue([3], false)]))")]
	[InlineData("INSERT INTO t SELECT a FROM u", "Insert(Target: t, SourceValue: Query(Select(Items: [ExpressionItem(a, null, false)], From: FromClause([Named(u)]))))")]
	[InlineData("INSERT INTO t DEFAULT VALUES", "Insert(Target: t, SourceValue: DefaultValues())")]
	public void An_insert_is_built_as_written(string input, string tree) =>
		Assert.Equal(tree, Show(SqlStandardParser.ParseInsertStatement(input)));

	[Theory]
	[InlineData("UPDATE ONLY (t) FOR PORTION OF p FROM a TO b AS x SET c = DEFAULT, (d, e) = ROW(1, 2), f??(1??) = 3, g.h.i = 4 WHERE c > 0",
		"Update(Target: TableTarget(t, true), Portion: PeriodPortion(p, a, b), Alias: Alias(x, null, true), Assignments: [Assignment([AssignmentTarget(c, null, null, false)], Default(), false), Assignment([AssignmentTarget(d, null, null, false), AssignmentTarget(e, null, null, false)], Row([1, 2], true), true), Assignment([AssignmentTarget(f, 1, null, true)], 3, false), Assignment([AssignmentTarget(g, null, [h, i], false)], 4, false)], Where: Comparison(c, Greater, 0))")]
	[InlineData("UPDATE t SET (a) = (1)", "Update(Target: TableTarget(t, false), Assignments: [Assignment([AssignmentTarget(a, null, null, false)], Parenthesized(1), true)])")]
	public void A_searched_update_is_built_as_written(string input, string tree) =>
		Assert.Equal(tree, Show(SqlStandardParser.ParseUpdateStatementSearched(input)));

	[Fact]
	public void A_positioned_statement_names_its_cursor()
	{
		Assert.Equal("Update(Target: TableTarget(t, false), Assignments: [Assignment([AssignmentTarget(a, null, null, false)], 1, false)], CurrentOf: CursorReference(MODULE.c, null, false, false, false))",
			Show(SqlStandardParser.ParseUpdateStatementPositioned("UPDATE t SET a = 1 WHERE CURRENT OF MODULE.c")));
		Assert.Equal("Delete(Target: TableTarget(t, false), Alias: Alias(x, null, false), CurrentOf: CursorReference(c, null, false, false, false))",
			Show(SqlStandardParser.ParseDeleteStatementPositioned("DELETE FROM t x WHERE CURRENT OF c")));
	}

	[Theory]
	[InlineData("DELETE FROM t WHERE a = 1", "Delete(Target: TableTarget(t, false), Where: Comparison(a, Equal, 1))")]
	public void A_searched_delete_is_built_as_written(string input, string tree) =>
		Assert.Equal(tree, Show(SqlStandardParser.ParseDeleteStatementSearched(input)));

	[Fact]
	public void A_merge_keeps_its_clauses_in_order() =>
		Assert.Equal("Merge(Target: TableTarget(t, false), Alias: Alias(x, null, false), SourceTable: Named(u), On: Comparison(a, Equal, b), Clauses: [Matched(Update([Assignment([AssignmentTarget(c, null, null, false)], 1, false)]), Condition: Comparison(c, Greater, 0)), Matched(Delete()), NotMatched(MergeInsertAction([a], UserValue, [1, Default()]))])",
			Show(SqlStandardParser.ParseMergeStatement("MERGE INTO t x USING u ON a = b WHEN MATCHED AND c > 0 THEN UPDATE SET c = 1 WHEN MATCHED THEN DELETE WHEN NOT MATCHED THEN INSERT (a) OVERRIDING USER VALUE VALUES (1, DEFAULT)")));

	[Theory]
	[InlineData("TRUNCATE TABLE t RESTART IDENTITY", "TruncateTable(Target: TableTarget(t, false), Identity: Restart)")]
	[InlineData("TRUNCATE TABLE t", "TruncateTable(Target: TableTarget(t, false))")]
	public void A_truncate_is_built_as_written(string input, string tree) =>
		Assert.Equal(tree, Show(SqlStandardParser.ParseTruncateTableStatement(input)));

	// ── §11 Schema definition and manipulation, §12 Access control ─────────────

	[Theory]
	[InlineData("CREATE GLOBAL TEMPORARY TABLE t (a INT DEFAULT 1 NOT NULL, CONSTRAINT pk PRIMARY KEY (a) NOT DEFERRABLE) ON COMMIT PRESERVE ROWS",
		"CreateTable(Scope: GlobalTemporary, Name: t, Contents: Elements([Column(ColumnDefinition(a, Numeric(Int, null, null), Default(1), [NotNull()], null)), TableConstraint(Unique(PrimaryKey, null, [a], null, Name: pk, Characteristics: ConstraintCharacteristics(null, false, null, true)))]), OnCommit: Preserve)")]
	[InlineData("CREATE TABLE t (a INT REFERENCES u (b) MATCH FULL ON DELETE CASCADE ON UPDATE SET NULL, LIKE v INCLUDING DEFAULTS, PERIOD FOR p (s, e))",
		"CreateTable(Name: t, Contents: Elements([Column(ColumnDefinition(a, Numeric(Int, null, null), null, [ForeignKey([], null, ReferencesSpecification(u, [b], null, Full, SetNull, Cascade, true))], null)), Like(v, [IncludingDefaults]), Period(PeriodDefinition(ApplicationTime, p, s, e))]))")]
	[InlineData("CREATE TABLE t AS (SELECT a FROM u) WITH NO DATA",
		"CreateTable(Name: t, Contents: AsQuery([], Select(Items: [ExpressionItem(a, null, false)], From: FromClause([Named(u)])), WithNoData))")]
	[InlineData("ALTER TABLE t ALTER COLUMN a SET GENERATED BY DEFAULT RESTART WITH 5 SET INCREMENT BY 2",
		"AlterTable(Name: t, Action: AlterColumn(a, SetIdentityGeneration(ByDefault, [Restart(5), Increment(2)]), true))")]
	[InlineData("CREATE VIEW v (a) AS SELECT a FROM t WITH LOCAL CHECK OPTION",
		"CreateView(Name: v, Specification: Regular([a]), Query: Select(Items: [ExpressionItem(a, null, false)], From: FromClause([Named(t)])), CheckOption: Local)")]
	[InlineData("CREATE SEQUENCE s AS BIGINT START WITH 1 NO MAXVALUE CYCLE",
		"CreateSequence(Name: s, Options: [DataTypeOption(Numeric(BigInt, null, null)), Start(1), Max(null, true), Cycle(true)])")]
	[InlineData("CREATE COLLATION c FOR utf8 FROM d NO PAD",
		"CreateCollation(Name: CollationName(c), CharacterSet: CharacterSetName(utf8), Source: CollationName(d), Padding: NoPad)")]
	[InlineData("GRANT SELECT (a), UPDATE ON TABLE t TO PUBLIC, r WITH GRANT OPTION GRANTED BY CURRENT_ROLE",
		"Grant(Body: Privileges([Privilege(Select, [a], []), Privilege(Update, [], [])], PrivilegeObject(Table, t, null, true), [Public(), Identifier(AuthorizationIdentifier(r))], false, true, CurrentRole))")]
	[InlineData("GRANT EXECUTE ON SPECIFIC FUNCTION s.f TO u",
		"Grant(Body: Privileges([Privilege(Execute, [], [])], PrivilegeObject(Routine, s.f, RoutineDesignator(Function, s.f, null, null, true, null), false), [Identifier(AuthorizationIdentifier(u))], false, false, null))")]
	[InlineData("REVOKE ADMIN OPTION FOR r1, r2 FROM u CASCADE",
		"Revoke(Body: Roles(true, [r1, r2], [Identifier(AuthorizationIdentifier(u))], null, Cascade))")]
	[InlineData("CREATE SCHEMA s AUTHORIZATION u PATH s, t DEFAULT CHARACTER SET utf8 CREATE TABLE x (a INT) CREATE ROLE r",
		"CreateSchema(Name: s, PathFirst: true, Authorization: AuthorizationIdentifier(u), DefaultCharacterSet: CharacterSetName(utf8), Path: PathSpecification([s, t]), Elements: [CreateTable(Name: x, Contents: Elements([Column(ColumnDefinition(a, Numeric(Int, null, null), null, [], null))])), CreateRole(Name: r)])")]
	[InlineData("DROP TYPE t CASCADE", "DropType(Name: t, Behavior: Cascade)")]
	[InlineData("CREATE TYPE s.t UNDER u AS (a INT DEFAULT 1) NOT FINAL REF IS SYSTEM GENERATED CAST (SOURCE AS DISTINCT) WITH f OVERRIDING METHOD m () RETURNS INT, STATIC METHOD n (x INT) RETURNS INT SELF AS RESULT LANGUAGE SQL",
		"CreateType(Definition: UserDefinedTypeDefinition(s.t, u, Members([AttributeDefinition(a, Numeric(Int, null, null), 1, null)]), [Final(false), Reference(SystemGenerated()), Cast(ToDistinct, f)], [MethodSpecification(null, m, [], ReturnsDefinition(Numeric(Int, null, null), null, false), null, false, false, [], true), MethodSpecification(Static, n, [ParameterDefinition(null, x, Numeric(Int, null, null), false, null, false)], ReturnsDefinition(Numeric(Int, null, null), null, false), null, true, false, [Language(SQL)], false)]))")]
	[InlineData("ALTER TYPE t DROP STATIC METHOD m (INT) RESTRICT", "AlterType(Name: t, Action: DropMethod(MethodDesignator(Static, m, [Numeric(Int, null, null)])))")]
	[InlineData("CREATE CAST (t AS INT) WITH SPECIFIC FUNCTION f AS ASSIGNMENT",
		"CreateCast(SourceType: UserDefined(t), TargetType: Numeric(Int, null, null), Function: RoutineDesignator(Function, f, null, null, true, null), AsAssignment: true)")]
	[InlineData("CREATE ORDERING FOR t ORDER FULL BY STATE s", "CreateOrdering(TypeName: t, Ordering: OrderingDefinition(Full, State(s)))")]
	[InlineData("CREATE TRANSFORMS FOR t g (TO SQL WITH FUNCTION f, FROM SQL WITH FUNCTION h)",
		"CreateTransform(PluralKeyword: true, TypeName: t, Groups: [TransformGroup(g, [TransformElement(ToSql, RoutineDesignator(Function, f, null, null, false, null)), TransformElement(FromSql, RoutineDesignator(Function, h, null, null, false, null))])])")]
	[InlineData("ALTER TRANSFORM FOR t g (DROP (TO SQL, FROM SQL RESTRICT))",
		"AlterTransform(TypeName: t, Groups: [TransformAlterGroup(g, [TransformAlterAction(false, [], [ToSql, FromSql], Restrict)])])")]
	[InlineData("DROP TRANSFORM ALL FOR t CASCADE", "DropTransform(Target: All(), TypeName: t, Behavior: Cascade)")]
	[InlineData("DROP TRIGGER g", "DropTrigger(Name: g)")]
	[InlineData("CREATE TRIGGER s.g BEFORE UPDATE OF a ON t REFERENCING OLD ROW AS o NEW TABLE n FOR EACH ROW WHEN (a > 1) BEGIN ATOMIC SET SCHEMA 's'; COMMIT; END",
		"CreateTrigger(Name: s.g, Time: Before, Event: TriggerEvent(Update, [a]), Table: t, Referencing: [TransitionReference(OldRow, o, true, true), TransitionReference(NewTable, n, false, false)], Action: TriggerAction(Row, Comparison(a, Greater, 1), [SetSchema(Value: 's'), Commit()], true))")]
	[InlineData("CREATE PROCEDURE p (IN a INT, OUT b TABLE PASS THROUGH WITH SET SEMANTICS KEEP ON EMPTY) LANGUAGE SQL NOT DETERMINISTIC SQL SECURITY DEFINER CALL q(a)",
		"CreateRoutine(Definition: RoutineDefinition(Procedure, p, [ParameterDefinition(In, a, Numeric(Int, null, null), false, null, false), ParameterDefinition(Out, b, GenericTable(PassThrough, Set, KeepOnEmpty), false, null, false)], null, [Language(SQL), Deterministic(false)], Sql(Call(Invocation: Invocation(q, [Argument(a, null, false)])), Definer), null, null, false, false))")]
	[InlineData("CREATE FUNCTION f (x INT) RETURNS TABLE (a INT) READS SQL DATA RETURNS NULL ON NULL INPUT DYNAMIC RESULT SETS 2 STATIC DISPATCH EXTERNAL NAME 'lib' PARAMETER STYLE GENERAL TRANSFORM GROUP g FOR TYPE t EXTERNAL SECURITY IMPLEMENTATION DEFINED",
		"CreateRoutine(Definition: RoutineDefinition(Function, f, [ParameterDefinition(null, x, Numeric(Int, null, null), false, null, false)], ReturnsDefinition(null, [FieldDefinition(a, Numeric(Int, null, null))], false, TableKeyword: true), [DataAccess(ReadsSqlData), NullCall(ReturnsNullOnNullInput), DynamicResultSets(2)], External('lib', General, TransformGroupSpecification(null, [TransformGroupForType(g, t)]), ImplementationDefined), null, null, true, false))")]
	[InlineData("ALTER SPECIFIC PROCEDURE s.p NAME x MODIFIES SQL DATA RESTRICT",
		"AlterRoutine(Routine: RoutineDesignator(Procedure, s.p, null, null, true, null), Characteristics: [ExternalName(x), DataAccess(ModifiesSqlData)], Behavior: Restrict)")]
	[InlineData("DROP FUNCTION f (INT, DATE) FOR s.t CASCADE",
		"DropRoutine(Routine: RoutineDesignator(Function, f, [Numeric(Int, null, null), DateTime(Date, null, null)], s.t, false, null), Behavior: Cascade)")]
	public void A_schema_statement_is_built_as_written(string input, string tree) =>
		Assert.Equal(tree, Show(SqlStandardParser.ParseSQLSchemaStatement(input)));

	// ── §14, §16–§23 Statements ────────────────────────────────────────────────

	[Theory]
	[InlineData("DECLARE LOCAL TEMPORARY TABLE t (a INT) ON COMMIT DELETE ROWS;",
		"DeclareLocalTemporaryTable(Name: t, Elements: [Column(ColumnDefinition(a, Numeric(Int, null, null), null, [], null))], OnCommit: Delete)")]
	[InlineData("SELECT a FROM t FOR UPDATE OF a;",
		"Select(Items: [ExpressionItem(a, null, false)], From: FromClause([Named(t)]), Updatability: UpdatabilityClause(false, [a]))")]
	[InlineData("START TRANSACTION ISOLATION LEVEL READ COMMITTED, READ ONLY, DIAGNOSTICS SIZE 5;",
		"StartTransaction(Modes: [Isolation(ReadCommitted), Access(ReadOnly), DiagnosticsSize(5)])")]
	[InlineData("ROLLBACK WORK AND NO CHAIN TO SAVEPOINT s;", "Rollback(Work: true, Chain: NoChain, ToSavepoint: s)")]
	[InlineData("SET CONSTRAINTS a, s.b DEFERRED;", "SetConstraints(Target: Names([a, s.b]), Timing: Deferred)")]
	[InlineData("CONNECT TO 'srv' AS 'c' USER 'u';", "Connect(Target: ConnectionTarget('srv', 'c', 'u', false))")]
	[InlineData("DISCONNECT DEFAULT;", "Disconnect(Object: Connection(Default()))")]
	[InlineData("SET SESSION CHARACTERISTICS AS TRANSACTION READ WRITE, TRANSACTION ISOLATION LEVEL SERIALIZABLE;",
		"SetSessionCharacteristics(Characteristics: [[Access(ReadWrite)], [Isolation(Serializable)]])")]
	[InlineData("SET NO COLLATION FOR utf8;", "SetCollation(NoCollation: true, ForCharacterSets: [CharacterSetName(utf8)])")]
	[InlineData("SET SCHEMA 's';", "SetSchema(Value: 's')")]
	public void A_direct_statement_is_built_as_written(string input, string tree) =>
		Assert.Equal(tree, Show(SqlStandardParser.ParseDirectSQLStatement(input)));

	[Theory]
	[InlineData("FETCH ABSOLUTE 3 FROM c INTO :a, b[1]",
		"FetchCursor(Orientation: FetchOrientation(Absolute, 3), FromKeyword: true, Cursor: CursorReference(c, null, false, false, false), Into: Values([Parameter(Host, a), Element(b, 1, null, false)]))")]
	[InlineData("SELECT a INTO :x FROM t",
		"Select(Items: [ExpressionItem(a, null, false)], Into: IntoClause([Parameter(Host, x)]), From: FromClause([Named(t)]))")]
	[InlineData("CALL p(1)", "Call(Invocation: Invocation(p, [Argument(1, null, false)]))")]
	[InlineData("RETURN NULL", "Return(NullKeyword: true)")]
	[InlineData("GET DIAGNOSTICS :n = ROW_COUNT, m = MORE",
		"GetDiagnostics(Information: Statement([StatementInformationItem(Parameter(Host, n), RowCount), StatementInformationItem(m, More)]))")]
	[InlineData("GET DIAGNOSTICS CONDITION 1 :t = RETURNED_SQLSTATE",
		"GetDiagnostics(Information: Condition(1, [ConditionInformationItem(Parameter(Host, t), ReturnedSqlstate)]))")]
	[InlineData("PREPARE s ATTRIBUTES 'a' FROM 'SELECT 1'",
		"Prepare(Statement: StatementReference(s, null, false, false), Attributes: 'a', Sql: 'SELECT 1')")]
	[InlineData("EXECUTE s INTO SQL DESCRIPTOR GLOBAL 'd' USING :b",
		"Execute(Statement: StatementReference(s, null, false, false), Result: Descriptor(DescriptorReference(null, 'd', false, true, false), true), Parameters: Values([Parameter(Host, b)]))")]
	[InlineData("DESCRIBE OUTPUT CURSOR c STRUCTURE USING DESCRIPTOR d WITH NESTING",
		"Describe(Body: Output(Cursor(CursorReference(c, null, false, false, false)), DescriptorReference(d, null, false, false, false), true, true, false))")]
	[InlineData("GET SQL DESCRIPTOR d VALUE 1 :a = NAME, :b = DATA",
		"GetDescriptor(SqlKeyword: true, Descriptor: DescriptorReference(d, null, false, false, false), Body: Value(1, [DescriptorRead(Parameter(Host, a), Name), DescriptorRead(Parameter(Host, b), Data)]))")]
	[InlineData("ALLOCATE GLOBAL :c INSENSITIVE SCROLL CURSOR WITH HOLD FOR s",
		"AllocateCursor(Cursor: CursorReference(null, Parameter(Host, c), false, true, false), Properties: CursorProperties(Insensitive, Scroll, WithHold, null), SourceValue: Prepared(StatementReference(s, null, false, false)))")]
	[InlineData("COPY d VALUE 1 (NAME, TYPE) TO PTF p VALUE 2",
		"CopyDescriptor(Body: Item(DescriptorReference(d, null, false, false, false), 1, [Name, Type], DescriptorReference(p, null, true, false, false), 2))")]
	public void A_procedure_statement_is_built_as_written(string input, string tree) =>
		Assert.Equal(tree, Show(SqlStandardParser.ParseSQLProcedureStatement(input)));

	[Fact]
	public void A_data_change_delta_table_holds_its_statement() =>
		Assert.Equal("DataChange(New, Insert(Target: t, SourceValue: Values([RowValue([1], false)])), Alias(x, null, true))",
			Show(SqlStandardParser.ParseTableReference("NEW TABLE (INSERT INTO t VALUES 1) AS x")));

	// ── The tree written back ──────────────────────────────────────────────────

	/// <summary>
	/// <see cref="Sql2023Writer"/> writes what the tree holds, so what it writes reads back into the same
	/// tree and is written again the same way. `benchmarks --standard "~production" file` asks it of every
	/// fuzz line; these are a line or two of each chapter, held here so a build asks it too.
	/// </summary>
	[Theory]
	[InlineData("a || b[1] || c COLLATE x")]
	[InlineData("a * -b[1] + (c) - d / e")]
	[InlineData("d MULTISET UNION ALL e MULTISET INTERSECT f")]
	[InlineData("d AT TIME ZONE e")]
	[InlineData("f AT LOCAL")]
	[InlineData("(a + b) * c - (d)")]
	[InlineData("CASE a WHEN < 1, 2 THEN 'x' ELSE NULL END")]
	[InlineData("CAST(a AS DECIMAL(10, 2) ARRAY[3]) || U&\"a\\0041\"UESCAPE'\\' || N'n' 'm' || _latin1'l' || X'0A'")]
	[InlineData("SUBSTRING(a SIMILAR b ESCAPE c) || TRIM(LEADING FROM a) || OVERLAY(a PLACING b FROM 1 FOR 2 USING OCTETS)")]
	[InlineData("RUNNING FIRST(a, 1) + LAG(a, 1, 0) IGNORE NULLS OVER (PARTITION BY b ORDER BY c DESC NULLS LAST ROWS BETWEEN 1 PRECEDING AND CURRENT ROW EXCLUDE TIES)")]
	[InlineData("JSON_QUERY(a FORMAT JSON, '$.x' PASSING b AS c RETURNING CLOB WITH CONDITIONAL ARRAY WRAPPER KEEP QUOTES ON SCALAR STRING EMPTY OBJECT ON EMPTY)")]
	[InlineData("a[$ to last, - -$ + $].double() || (a AS t).m(b AS u)")]
	[InlineData("INTERVAL -'1:30' HOUR(2) TO SECOND(3) + a")]
	[InlineData("a.SPECIFICTYPE() COLLATE c || b")]
	public void A_value_expression_is_written_back_as_the_tree_it_was(string input) =>
		WrittenBack(input, SqlStandardParser.ParseValueExpression);

	[Theory]
	[InlineData("WITH RECURSIVE r (n) AS (VALUES 1) SEARCH DEPTH FIRST BY n SET s SELECT DISTINCT t.*, u.* AS (x, y), n AS m FROM r")]
	[InlineData("(SELECT a FROM t ORDER BY a) UNION ALL CORRESPONDING BY (a) TABLE u INTERSECT VALUES (1, 2), ROW(3, 4) ORDER BY 1 OFFSET 1 ROW FETCH NEXT 10 PERCENT ROWS WITH TIES")]
	[InlineData("SELECT * FROM t PARTITION BY (a) NATURAL FULL OUTER JOIN u PARTITION BY (b), LATERAL (SELECT 1 FROM v) AS w (c) TABLESAMPLE SYSTEM (5) REPEATABLE (7)")]
	[InlineData("SELECT a FROM t MATCH_RECOGNIZE (PARTITION BY a ORDER BY b MEASURES m AS n ONE ROW PER MATCH AFTER MATCH SKIP TO FIRST A PATTERN (^ A+? {- B -} (C | D){1,3} $) SUBSET S = (A, B) DEFINE A AS a > 1) AS m")]
	[InlineData("SELECT a FROM JSON_TABLE(j, '$' COLUMNS (id FOR ORDINALITY, NESTED PATH '$.b' AS p COLUMNS (b INTEGER FORMAT JSON OMIT QUOTES)) PLAN DEFAULT (UNION, INNER) EMPTY ON ERROR) AS jt GROUP BY GROUPING SETS (ROLLUP (a), ()) HAVING COUNT(*) > 1 WINDOW w AS (ORDER BY a)")]
	[InlineData("SELECT a FROM NEW TABLE (INSERT INTO t (a) OVERRIDING SYSTEM VALUE VALUES (DEFAULT), 1) AS x")]
	public void A_query_is_written_back_as_the_tree_it_was(string input) =>
		WrittenBack(input, SqlStandardParser.ParseQueryExpression);

	[Theory]
	[InlineData("CREATE GLOBAL TEMPORARY TABLE t (a INT DEFAULT 1 NOT NULL, CONSTRAINT pk PRIMARY KEY (a) NOT DEFERRABLE, FOREIGN KEY (a) REFERENCES u (b) MATCH FULL ON DELETE CASCADE ON UPDATE SET NULL) ON COMMIT PRESERVE ROWS")]
	[InlineData("ALTER TABLE t ALTER COLUMN a SET GENERATED BY DEFAULT RESTART WITH 5 SET INCREMENT BY 2")]
	[InlineData("CREATE SCHEMA s AUTHORIZATION u PATH s, t DEFAULT CHARACTER SET utf8 CREATE TABLE x (a INT) CREATE ROLE r")]
	[InlineData("GRANT SELECT (a), UPDATE ON TABLE t TO PUBLIC, r WITH GRANT OPTION GRANTED BY CURRENT_ROLE")]
	[InlineData("CREATE TRIGGER s.g BEFORE UPDATE OF a ON t REFERENCING OLD ROW AS o NEW TABLE n FOR EACH ROW WHEN (a > 1) BEGIN ATOMIC SET SCHEMA 's'; COMMIT; END")]
	[InlineData("CREATE FUNCTION f (x INT) RETURNS TABLE (a INT) READS SQL DATA STATIC DISPATCH EXTERNAL NAME 'lib' PARAMETER STYLE GENERAL TRANSFORM GROUP g FOR TYPE t EXTERNAL SECURITY IMPLEMENTATION DEFINED")]
	[InlineData("CREATE TYPE s.t UNDER u AS (a INT DEFAULT 1) NOT FINAL REF IS SYSTEM GENERATED CAST (SOURCE AS DISTINCT) WITH f OVERRIDING METHOD m () RETURNS INT")]
	[InlineData("ALTER TRANSFORM FOR t g (DROP (TO SQL, FROM SQL RESTRICT))")]
	public void A_schema_statement_is_written_back_as_the_tree_it_was(string input) =>
		WrittenBack(input, SqlStandardParser.ParseSQLSchemaStatement);

	[Theory]
	[InlineData("UPDATE ONLY (t) AS x SET (d, e) = ROW(1, 2), f??(1??) = 3, g.h.i = 4 WHERE CURRENT OF MODULE.c")]
	[InlineData("UPDATE t FOR PORTION OF p FROM a TO b SET a = DEFAULT WHERE b")]
	[InlineData("UPDATE t SET a = 1 WHERE CURRENT OF GLOBAL :c")]
	[InlineData("MERGE INTO t x USING u ON a = b WHEN MATCHED AND c > 0 THEN UPDATE SET c = 1 WHEN NOT MATCHED THEN INSERT (a) VALUES (1, DEFAULT)")]
	[InlineData("FETCH ABSOLUTE 3 FROM c INTO :a INDICATOR :i, b[1]")]
	[InlineData("GET DIAGNOSTICS CONDITION 1 :t = RETURNED_SQLSTATE, u = MESSAGE_TEXT")]
	[InlineData("EXECUTE s INTO SQL DESCRIPTOR GLOBAL 'd' USING :b")]
	[InlineData("ALLOCATE GLOBAL :c INSENSITIVE SCROLL CURSOR WITH HOLD FOR s")]
	[InlineData("SET SESSION CHARACTERISTICS AS TRANSACTION READ WRITE, TRANSACTION ISOLATION LEVEL SERIALIZABLE")]
	public void A_procedure_statement_is_written_back_as_the_tree_it_was(string input) =>
		WrittenBack(input, SqlStandardParser.ParseSQLProcedureStatement);

	static void WrittenBack<T>(string input, Func<string, T> parse) where T : ISqlNode
	{
		var tree    = parse(input);
		var written = Sql2023Writer.Write(tree);
		var again   = parse(written);

		Assert.Equal(Show(tree), Show(again));
		Assert.Equal(written, Sql2023Writer.Write(again));
	}

	/// <summary>
	/// A node as one line: its record's name and its positional values in order, then what else it
	/// holds that is not a default; a name, a reference and a literal as they were written.
	/// </summary>
	static string Show(object? node) =>
		node switch
		{
			null                           => "null",
			string text                    => text,
			bool flag                      => flag ? "true" : "false",
			Enum value                     => value.ToString(),
			Identifier identifier          => identifier.Text,
			QualifiedName name             => string.Join(".", name.Parts.Select(one => one.Text)),
			Ast.Expression.Reference named => Show(named.Name),
			Ast.Expression.Literal literal => literal.Value switch
			{
				LiteralValue.Numeric number => number.Text,
				LiteralValue.String text    => text.Text,
				LiteralValue.Null           => "NULL",
				var other                   => Record(other),
			},
			System.Collections.IEnumerable list => "[" + string.Join(", ", list.Cast<object?>().Select(Show)) + "]",
			_                              => node.GetType().IsPrimitive ? node.ToString()! : Record(node),
		};

	static string Record(object node)
	{
		var type        = node.GetType();
		var constructor = type.GetConstructors()
			.Where(one => !(one.GetParameters().Length == 1 && one.GetParameters()[0].ParameterType == type))
			.OrderByDescending(one => one.GetParameters().Length)
			.FirstOrDefault();
		var positional  = constructor?.GetParameters().Select(one => one.Name!).ToArray() ?? [];
		var parts       = positional.Select(name => Show(type.GetProperty(name)!.GetValue(node))).ToList();

		foreach (var property in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
		{
			if (positional.Contains(property.Name) || property.Name is "Span" or "EqualityContract")
				continue;

			var value = property.GetValue(node);

			if (value is null or false || value is int and 0 || value is System.Collections.ICollection { Count: 0 })
				continue;

			parts.Add(property.Name + ": " + Show(value));
		}

		return type.Name + "(" + string.Join(", ", parts) + ")";
	}
}
