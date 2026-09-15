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

			if (value is null or false || value is int and 0)
				continue;

			parts.Add(property.Name + ": " + Show(value));
		}

		return type.Name + "(" + string.Join(", ", parts) + ")";
	}
}
