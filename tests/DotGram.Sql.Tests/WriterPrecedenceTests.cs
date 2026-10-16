using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotGram.Sql.Ast;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

// Inside this namespace an unqualified name is the OLD tree's; see Sql2023WriterCoverageTests.
using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Query = DotGram.Sql.Ast.Query;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// The writer's brackets for a tree built by hand: an operand that binds looser than its operator is written in a
/// bracket, in the standard's reading and in T-SQL's, and nowhere else.
/// </summary>
/// <remarks>
/// <para>
/// A tree read from text holds a <see cref="Expression.Parenthesized"/> wherever its shape needed one, and is written as
/// it was read. A tree built without them would otherwise come back as another tree: <c>1 - (1 - 1)</c> written
/// <c>1 - 1 - 1</c>. So a row here builds a tree, says the text it is written as, and reads that text back: the tree
/// that comes back is the one built, with a Parenthesized where the writer added a bracket and nowhere else.
/// </para>
/// <para>
/// The random rows put the levels to the parsers themselves: a tree of random operators is written, read back and
/// compared with its brackets aside, and every bracket in the text is taken out once to see that the text then means
/// something else. Where the reading refuses the text — the standard's towers type their operands, and `a || 1` is
/// none of them — the same tree written with a bracket around every operand is refused as well, or the levels are wrong.
/// </para>
/// </remarks>
public sealed class WriterPrecedenceTests
{
	// ── Trees ────────────────────────────────────────────────────────────────────

	static Expression Ref(string name)
	{
		return new Expression.Reference(new QualifiedName([new Identifier(name)]));
	}

	static Expression Number(string text)
	{
		return new LiteralValue.Numeric(text, NumericLiteralKind.DecimalInteger);
	}

	static readonly Expression A   = Ref("a");
	static readonly Expression B   = Ref("b");
	static readonly Expression C   = Ref("c");
	static readonly Expression D   = Ref("d");
	static readonly Expression One = Number("1");

	static Expression Binary(Expression left, BinaryOperator op, Expression right)
	{
		return new Expression.Binary(left, op, right);
	}

	static Expression Unary(UnaryOperator op, Expression operand)
	{
		return new Expression.Unary(op, operand);
	}

	static Expression Equal(Expression left, Expression right)
	{
		return new Expression.Comparison(left, ComparisonOperator.Equal, right);
	}

	static Expression Collate(Expression value, string collation)
	{
		return new Expression.Collate(value, new CollationName(new QualifiedName([new Identifier(collation)])));
	}

	static Expression Zone(Expression value, Expression zone)
	{
		return new Expression.AtTimeZone(value, zone);
	}

	static Expression Multiset(Expression left, MultisetOperator op, Expression right)
	{
		return new Expression.MultisetOperation(left, op, null, right);
	}

	// ── Brackets aside ───────────────────────────────────────────────────────────

	/// <summary>
	/// The node with each of its operands put through <paramref name="operand"/>, for every node the rows build; any
	/// other is its own operands' only node and is returned as it is.
	/// </summary>
	static Expression Rebuild(Expression node, Func<Expression, Expression> operand)
	{
		switch (node)
		{
			case Expression.Binary binary:
				return binary with { Left = operand(binary.Left), Right = operand(binary.Right) };

			case Expression.Unary unary:
				return unary with { Operand = operand(unary.Operand) };

			case Expression.MultisetOperation multiset:
				return multiset with { Left = operand(multiset.Left), Right = operand(multiset.Right) };

			case Expression.Comparison comparison:
				return comparison with { Left = operand(comparison.Left), Right = operand(comparison.Right) };

			case Expression.IsNull isNull:
				return isNull with { Value = operand(isNull.Value) };

			case Expression.IsTruth isTruth:
				return isTruth with { Value = operand(isTruth.Value) };

			case Expression.IsDistinct distinct:
				return distinct with { Left = operand(distinct.Left), Right = operand(distinct.Right) };

			case Expression.Between between:
				return between with { Value = operand(between.Value), Lower = operand(between.Lower), Upper = operand(between.Upper) };

			case Expression.Like like:
				return like with { Value = operand(like.Value), Pattern = operand(like.Pattern), Escape = like.Escape is null ? null : operand(like.Escape) };

			case Expression.In @in:
				return @in with { Value = operand(@in.Value) };

			case Expression.Collate collate:
				return collate with { Value = operand(collate.Value) };

			case Expression.AtTimeZone zone:
				return zone with { Value = operand(zone.Value), Zone = zone.Zone is null ? null : operand(zone.Zone) };

			case Expression.IntervalQualified qualified:
				return qualified with { Value = operand(qualified.Value) };

			case Expression.Element element:
				return element with { Collection = operand(element.Collection) };

			case Expression.Member member:
				return member with { Target = operand(member.Target) };

			case Expression.JsonAccessor accessor:
				return accessor with { Target = operand(accessor.Target) };

			default:
				return node;
		}
	}

	/// <summary>The tree with every <see cref="Expression.Parenthesized"/> taken out: what it means, its brackets aside.</summary>
	static Expression Unbracketed(Expression node)
	{
		while (node is Expression.Parenthesized parenthesized)
			node = parenthesized.Value;

		var rebuilt = Rebuild(node, Unbracketed);

		return rebuilt is Expression.JsonAccessor accessor ? accessor with { Accessor = Unbracketed(accessor.Accessor) } : rebuilt;
	}

	/// <summary>A JSON path's accessor with every bracket of the path language taken out.</summary>
	static JsonPathAccessor Unbracketed(JsonPathAccessor accessor)
	{
		switch (accessor)
		{
			case JsonPathAccessor.Array array:
				return array with { Subscripts = [.. array.Subscripts.Select(static one => one with { From = Unbracketed(one.From), To = one.To is null ? null : Unbracketed(one.To) })] };

			case JsonPathAccessor.Filter filter:
				return filter with { Predicate = Unbracketed(filter.Predicate) };

			default:
				return accessor;
		}
	}

	static JsonPathExpression Unbracketed(JsonPathExpression path)
	{
		while (path is JsonPathExpression.Parenthesized parenthesized)
			path = parenthesized.Value;

		switch (path)
		{
			case JsonPathExpression.Unary unary:
				return unary with { Operand = Unbracketed(unary.Operand) };

			case JsonPathExpression.Binary binary:
				return binary with { Left = Unbracketed(binary.Left), Right = Unbracketed(binary.Right) };

			case JsonPathExpression.Access access:
				return access with { Target = Unbracketed(access.Target), Accessor = Unbracketed(access.Accessor) };

			default:
				return path;
		}
	}

	static JsonPathPredicate Unbracketed(JsonPathPredicate predicate)
	{
		while (predicate is JsonPathPredicate.Parenthesized parenthesized)
			predicate = parenthesized.Predicate;

		switch (predicate)
		{
			case JsonPathPredicate.Or or:
				return or with { Items = [.. or.Items.Select(Unbracketed)] };

			case JsonPathPredicate.Comparison comparison:
				return comparison with { Left = Unbracketed(comparison.Left), Right = Unbracketed(comparison.Right) };

			default:
				return predicate;
		}
	}

	/// <summary>The tree with a bracket around every operand that is not a name or a number.</summary>
	static Expression Bracketed(Expression node)
	{
		return Rebuild(node, static operand =>
		{
			var inner = Bracketed(operand);

			return inner is Expression.Reference or LiteralValue ? inner : new Expression.Parenthesized(inner);
		});
	}

	// ── The two readings ─────────────────────────────────────────────────────────

	/// <summary>One dialect: how a tree is written, and how a text is read back into the expression it holds.</summary>
	sealed record Reading(string Name, Func<Expression, bool, string> Write, Func<string, bool, Expression?> Read);

	static readonly Reading Standard = new(
		"standard",
		static (expression, condition) =>
		{
			return Sql2023Writer.Write(expression);
		},
		static (text, condition) =>
		{
			var read = SqlStandardParser.TryParseExpression(text);

			return read.IsSuccess ? read.Value : null;
		});

	/// <summary>
	/// T-SQL, through a statement: a value in a SELECT list, a condition in a WHERE, which is where T-SQL reads each. Read
	/// into the old tree and carried onto the new one by the bridge, as everything T-SQL reads is for now.
	/// </summary>
	static readonly Reading TransactSql = new(
		"T-SQL",
		static (expression, condition) =>
		{
			var query = condition
				? new Query.Select { Columns = [new SelectColumn.ExpressionItem(One)], Where = expression }
				: new Query.Select { Columns = [new SelectColumn.ExpressionItem(expression)] };

			return TransactSqlWriter.Write(new Statement.Select { Query = query });
		},
		static (text, condition) =>
		{
			var read = TransactSqlParser.Located.TryParseStatement(text);

			if (!read.IsSuccess)
				return null;

			var query = (Query.Select)((Statement.Select)Bridge.ToAst(read.Value!, text)).Query;

			return condition ? query.Where : ((SelectColumn.ExpressionItem)query.Columns[0]).Expression;
		});

	/// <summary>
	/// A tree built by hand is written as <paramref name="expected"/> — of a statement, in T-SQL's reading — and reads back
	/// as itself, with its brackets as the text has them; and that tree is written as the same text again.
	/// </summary>
	static void Writes(Reading reading, Expression tree, string expected, bool condition = false)
	{
		var text = reading.Write(tree, condition);

		Assert.Equal(expected, text);

		var read = reading.Read(text, condition);

		Assert.True(read is not null, $"{reading.Name} does not read {text}");
		Assert.Equal(tree, Unbracketed(read!));
		Assert.Equal(text, reading.Write(read!, condition));
	}

	static void Standardly(Expression tree, string expected)
	{
		Writes(Standard, tree, expected);
	}

	static void Value(Expression tree, string expected)
	{
		Writes(TransactSql, tree, "SELECT " + expected);
	}

	static void Condition(Expression tree, string expected)
	{
		Writes(TransactSql, tree, "SELECT 1 WHERE " + expected, condition: true);
	}

	// ── Each level and its associativity ───────────────────────────────────────

	[Fact]
	public void A_left_associative_operator_brackets_its_right_operand_at_its_own_level()
	{
		Standardly(Binary(One, BinaryOperator.Subtract, Binary(One, BinaryOperator.Subtract, One)), "1 - (1 - 1)");
		Standardly(Binary(Binary(One, BinaryOperator.Subtract, One), BinaryOperator.Subtract, One), "1 - 1 - 1");
		Standardly(Binary(A, BinaryOperator.Divide, Binary(B, BinaryOperator.Multiply, C)), "a / (b * c)");
		Standardly(Binary(A, BinaryOperator.Multiply, Binary(B, BinaryOperator.Divide, C)), "a * (b / c)");
		Standardly(Binary(A, BinaryOperator.Concatenate, Binary(B, BinaryOperator.Concatenate, C)), "a || (b || c)");

		Value(Binary(One, BinaryOperator.Subtract, Binary(One, BinaryOperator.Subtract, One)), "1 - (1 - 1)");
		Value(Binary(A, BinaryOperator.Modulo, Binary(B, BinaryOperator.Multiply, C)), "a % (b * c)");
		Value(Binary(A, BinaryOperator.Divide, Binary(B, BinaryOperator.Modulo, C)), "a / (b % c)");
		Value(Binary(A, BinaryOperator.BitwiseAnd, Binary(B, BinaryOperator.BitwiseOr, C)), "a & (b | c)");
		Value(Binary(A, BinaryOperator.Add, Binary(B, BinaryOperator.ShiftLeft, C)), "a + (b << c)");
		Value(Binary(Binary(A, BinaryOperator.BitwiseXor, B), BinaryOperator.Subtract, C), "a ^ b - c");
	}

	[Fact]
	public void A_looser_operand_is_bracketed_and_a_tighter_one_is_not()
	{
		Standardly(Binary(Binary(One, BinaryOperator.Add, One), BinaryOperator.Multiply, One), "(1 + 1) * 1");
		Standardly(Binary(One, BinaryOperator.Add, Binary(One, BinaryOperator.Multiply, One)), "1 + 1 * 1");

		Value(Binary(Binary(One, BinaryOperator.Add, One), BinaryOperator.Multiply, One), "(1 + 1) * 1");
		Value(Binary(One, BinaryOperator.ShiftLeft, Binary(One, BinaryOperator.Add, One)), "1 << (1 + 1)");
		Value(Binary(Binary(One, BinaryOperator.ShiftLeft, One), BinaryOperator.Add, One), "1 << 1 + 1");
		Value(Binary(Binary(A, BinaryOperator.BitwiseAnd, B), BinaryOperator.Multiply, C), "(a & b) * c");
		Value(Binary(A, BinaryOperator.ShiftRight, Binary(B, BinaryOperator.Modulo, C)), "a >> b % c");
	}

	[Fact]
	public void A_sign_brackets_any_operator_under_it()
	{
		Standardly(Unary(UnaryOperator.Minus, Binary(A, BinaryOperator.Add, B)), "-(a + b)");
		Standardly(Unary(UnaryOperator.Minus, Binary(A, BinaryOperator.Multiply, B)), "-(a * b)");
		Standardly(Binary(Unary(UnaryOperator.Minus, A), BinaryOperator.Multiply, B), "-a * b");
		Standardly(Binary(A, BinaryOperator.Multiply, Unary(UnaryOperator.Minus, B)), "a * -b");

		Value(Unary(UnaryOperator.Minus, Binary(A, BinaryOperator.Multiply, B)), "-(a * b)");
		Value(Unary(UnaryOperator.BitwiseNot, Binary(A, BinaryOperator.Add, B)), "~(a + b)");
		Value(Binary(Unary(UnaryOperator.BitwiseNot, A), BinaryOperator.Multiply, B), "~a * b");
	}

	/// <summary>The standard reads one sign before a primary; T-SQL's stand on each other.</summary>
	[Fact]
	public void A_sign_over_a_sign_is_bracketed_only_in_the_standard()
	{
		Standardly(Unary(UnaryOperator.Minus, Unary(UnaryOperator.Minus, A)), "-(-a)");
		Standardly(Unary(UnaryOperator.Plus, Unary(UnaryOperator.Minus, A)), "+(-a)");

		Value(Unary(UnaryOperator.Minus, Unary(UnaryOperator.Minus, A)), "- -a");
		Value(Unary(UnaryOperator.Minus, Unary(UnaryOperator.BitwiseNot, A)), "-~a");
	}

	[Fact]
	public void A_predicate_takes_values_and_brackets_a_predicate()
	{
		Standardly(Equal(Binary(A, BinaryOperator.Add, B), C), "a + b = c");
		Standardly(Equal(Equal(A, B), C), "(a = b) = c");
		Standardly(Equal(A, Equal(B, C)), "a = (b = c)");
		Standardly(new Expression.IsNull(Equal(A, B), false), "(a = b) IS NULL");
		Standardly(new Expression.Between(A, false, null, B, Binary(C, BinaryOperator.And, D)), "a BETWEEN b AND (c AND d)");
		Standardly(new Expression.Like(A, false, LikeKind.Like, Binary(B, BinaryOperator.Concatenate, C)), "a LIKE b || c");

		Condition(Equal(A, Unary(UnaryOperator.Minus, B)), "a = -b");
		Condition(new Expression.IsNull(Binary(A, BinaryOperator.Add, B), true), "a + b IS NOT NULL");
		Condition(new Expression.IsDistinct(A, true, Binary(B, BinaryOperator.BitwiseOr, C)), "a IS NOT DISTINCT FROM b | c");
	}

	/// <summary>The standard's NOT is over a boolean test, which a NOT is not; T-SQL's stand on each other.</summary>
	[Fact]
	public void NOT_brackets_AND_and_OR_and_in_the_standard_another_NOT()
	{
		var p = Equal(A, One);
		var q = Equal(B, One);

		Standardly(Unary(UnaryOperator.Not, Binary(p, BinaryOperator.And, q)), "NOT (a = 1 AND b = 1)");
		Standardly(Unary(UnaryOperator.Not, Unary(UnaryOperator.Not, p)), "NOT (NOT a = 1)");
		Standardly(Unary(UnaryOperator.Not, p), "NOT a = 1");
		Standardly(Unary(UnaryOperator.Not, new Expression.IsTruth(A, false, BooleanLiteral.True)), "NOT a IS TRUE");

		Condition(Unary(UnaryOperator.Not, Binary(p, BinaryOperator.Or, q)), "NOT (a = 1 OR b = 1)");
		Condition(Unary(UnaryOperator.Not, Unary(UnaryOperator.Not, p)), "NOT NOT a = 1");
		Condition(Binary(Unary(UnaryOperator.Not, p), BinaryOperator.And, q), "NOT a = 1 AND b = 1");
	}

	[Fact]
	public void AND_binds_tighter_than_OR_and_both_go_to_the_left()
	{
		var p = Equal(A, One);
		var q = Equal(B, One);
		var r = Equal(C, One);

		Standardly(Binary(Binary(p, BinaryOperator.Or, q), BinaryOperator.And, r), "(a = 1 OR b = 1) AND c = 1");
		Standardly(Binary(p, BinaryOperator.Or, Binary(q, BinaryOperator.Or, r)), "a = 1 OR (b = 1 OR c = 1)");

		Condition(Binary(Binary(p, BinaryOperator.Or, q), BinaryOperator.And, r), "(a = 1 OR b = 1) AND c = 1");
		Condition(Binary(p, BinaryOperator.And, Binary(q, BinaryOperator.And, r)), "a = 1 AND (b = 1 AND c = 1)");
		Condition(Binary(p, BinaryOperator.Or, Binary(q, BinaryOperator.And, r)), "a = 1 OR b = 1 AND c = 1");
	}

	[Fact]
	public void IS_TRUE_is_over_a_predicate_and_not_over_another_test()
	{
		Standardly(new Expression.IsTruth(Equal(A, B), false, BooleanLiteral.True), "a = b IS TRUE");
		Standardly(new Expression.IsTruth(new Expression.IsTruth(A, false, BooleanLiteral.True), true, BooleanLiteral.False), "(a IS TRUE) IS NOT FALSE");
		Standardly(new Expression.IsTruth(Binary(A, BinaryOperator.And, B), false, BooleanLiteral.Unknown), "(a AND b) IS UNKNOWN");
	}

	/// <summary>
	/// The standard puts one of COLLATE, AT and an interval qualifier after a primary. T-SQL puts its zones after a primary,
	/// each with a COLLATE that may stand before its AT, and one COLLATE after them.
	/// </summary>
	[Fact]
	public void What_follows_a_primary_brackets_what_is_not_one()
	{
		Standardly(Collate(Binary(A, BinaryOperator.Concatenate, B), "x"), "(a || b) COLLATE x");
		Standardly(Collate(Collate(A, "x"), "y"), "(a COLLATE x) COLLATE y");
		Standardly(Zone(Binary(A, BinaryOperator.Add, B), C), "(a + b) AT TIME ZONE c");
		Standardly(Zone(A, Binary(B, BinaryOperator.Add, C)), "a AT TIME ZONE (b + c)");
		Standardly(Zone(Zone(A, B), C), "(a AT TIME ZONE b) AT TIME ZONE c");
		Standardly(Zone(A, Unary(UnaryOperator.Minus, B)), "a AT TIME ZONE (-b)");
		Standardly(new Expression.IntervalQualified(Binary(A, BinaryOperator.Subtract, B), new IntervalQualifier(DateTimeField.Day)), "(a - b) DAY");

		Value(Collate(Collate(A, "x"), "y"), "(a COLLATE x) COLLATE y");
		Value(Collate(Unary(UnaryOperator.Minus, A), "x"), "(-a) COLLATE x");
		Value(Unary(UnaryOperator.Minus, Collate(A, "x")), "-a COLLATE x");
		Value(Collate(Binary(A, BinaryOperator.Add, B), "x"), "(a + b) COLLATE x");
		Value(Collate(Zone(A, B), "x"), "a AT TIME ZONE b COLLATE x");
		Value(Zone(Collate(A, "x"), B), "a COLLATE x AT TIME ZONE b");
		Value(Zone(Zone(A, B), C), "a AT TIME ZONE b AT TIME ZONE c");
		Value(Zone(Collate(Zone(A, B), "x"), C), "a AT TIME ZONE b COLLATE x AT TIME ZONE c");
		Value(Zone(Unary(UnaryOperator.Minus, A), B), "(-a) AT TIME ZONE b");
		Value(Unary(UnaryOperator.Minus, Zone(A, B)), "-a AT TIME ZONE b");
		Value(Zone(A, Unary(UnaryOperator.Minus, B)), "a AT TIME ZONE -b");
		Value(Zone(A, Unary(UnaryOperator.Minus, Binary(B, BinaryOperator.Add, C))), "a AT TIME ZONE -(b + c)");
		Value(Zone(A, Unary(UnaryOperator.Minus, Collate(B, "x"))), "a AT TIME ZONE (-b COLLATE x)");
		Value(Zone(A, Collate(B, "x")), "a AT TIME ZONE (b COLLATE x)");
		Value(Zone(A, Zone(B, C)), "a AT TIME ZONE (b AT TIME ZONE c)");
		Value(Binary(Zone(A, B), BinaryOperator.Add, C), "a AT TIME ZONE b + c");
	}

	[Fact]
	public void MULTISET_INTERSECT_binds_tighter_than_UNION_and_EXCEPT()
	{
		Standardly(Multiset(Multiset(A, MultisetOperator.Union, B), MultisetOperator.Intersect, C), "(a MULTISET UNION b) MULTISET INTERSECT c");
		Standardly(Multiset(A, MultisetOperator.Union, Multiset(B, MultisetOperator.Intersect, C)), "a MULTISET UNION b MULTISET INTERSECT c");
		Standardly(Multiset(A, MultisetOperator.Except, Multiset(B, MultisetOperator.Union, C)), "a MULTISET EXCEPT (b MULTISET UNION c)");
	}

	[Fact]
	public void A_subscripted_or_dereferenced_value_is_a_primary()
	{
		Standardly(new Expression.Element(Binary(A, BinaryOperator.Concatenate, B), One), "(a || b)[1]");
		Standardly(new Expression.Member(Binary(A, BinaryOperator.Add, B), MemberAccessKind.Dot, new Identifier("m"), []), "(a + b).m()");
	}

	/// <summary>
	/// The path language of a JSON accessor has its own levels — `+ -`, `* / %`, signs, a primary and its accessors — and a
	/// disjunction whose items are read as one list.
	/// </summary>
	[Fact]
	public void A_JSON_path_brackets_by_its_own_levels()
	{
		static Expression Path(JsonPathExpression path)
		{
			return new Expression.JsonAccessor(A, new JsonPathAccessor.Array([new JsonSubscript(path)]));
		}

		var context = new JsonPathExpression.Variable(JsonPathVariableKind.Context);
		var last    = new JsonPathExpression.Variable(JsonPathVariableKind.Last);

		static JsonPathExpression Operated(JsonPathExpression left, JsonPathBinaryOperator op, JsonPathExpression right)
		{
			return new JsonPathExpression.Binary(left, op, right);
		}

		static JsonPathPredicate Greater(JsonPathExpression left, JsonPathExpression right)
		{
			return new JsonPathPredicate.Comparison(left, JsonPathComparisonOperator.Greater, right);
		}

		Standardly(Path(Operated(context, JsonPathBinaryOperator.Subtract, Operated(context, JsonPathBinaryOperator.Subtract, last))), "a[$ - ($ - LAST)]");
		Standardly(Path(Operated(Operated(context, JsonPathBinaryOperator.Subtract, context), JsonPathBinaryOperator.Subtract, last)), "a[$ - $ - LAST]");
		Standardly(Path(Operated(Operated(context, JsonPathBinaryOperator.Add, context), JsonPathBinaryOperator.Modulo, last)), "a[($ + $) % LAST]");
		Standardly(Path(Operated(context, JsonPathBinaryOperator.Add, Operated(context, JsonPathBinaryOperator.Multiply, last))), "a[$ + $ * LAST]");
		Standardly(Path(new JsonPathExpression.Unary(JsonPathUnaryOperator.Minus, Operated(context, JsonPathBinaryOperator.Add, last))), "a[-($ + LAST)]");
		Standardly(Path(new JsonPathExpression.Unary(JsonPathUnaryOperator.Minus, new JsonPathExpression.Unary(JsonPathUnaryOperator.Minus, last))), "a[- -LAST]");
		Standardly(Path(new JsonPathExpression.Access(Operated(context, JsonPathBinaryOperator.Add, last), new JsonPathAccessor.WildcardArray())), "a[($ + LAST)[*]]");

		var or = new JsonPathPredicate.Or([Greater(context, last), new JsonPathPredicate.Or([Greater(last, context), Greater(context, context)])]);

		Standardly(Path(new JsonPathExpression.Access(context, new JsonPathAccessor.Filter(or))), "a[$ ? ($ > LAST || (LAST > $ || $ > $))]");
	}

	/// <summary>
	/// T-SQL's set operators bind as the standard's: INTERSECT tighter than UNION and EXCEPT, all three to the left. A list
	/// built by hand is bracketed where a later operator binds tighter, and an operand where its own operations do not.
	/// </summary>
	[Fact]
	public void A_TSQL_set_operation_built_by_hand_is_bracketed_as_the_standard_s()
	{
		static Query.Select Select(int value)
		{
			return new Query.Select { Columns = [new SelectColumn.ExpressionItem(Number(value.ToString()))] };
		}

		var flat = Select(1) with
		{
			SetOperations = [new SetOperation(SetOperator.Union, Select(2)), new SetOperation(SetOperator.Intersect, Select(3))],
		};
		var nested = Select(1) with
		{
			SetOperations = [new SetOperation(SetOperator.Except, Select(2) with { SetOperations = [new SetOperation(SetOperator.Except, Select(3))] })],
		};

		foreach (var (query, expected) in new[] { (flat, "(SELECT 1 UNION SELECT 2) INTERSECT SELECT 3"), (nested, "SELECT 1 EXCEPT (SELECT 2 EXCEPT SELECT 3)") })
		{
			var text = TransactSqlWriter.Write(new Statement.Select { Query = query });

			Assert.Equal(expected, text);

			var read = (Statement.Select)Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

			Assert.Equal(text, TransactSqlWriter.Write(read));
			Assert.Equal(new[] { 1, 2, 3 }, read.Query.Operands().Select(static one => int.Parse(((LiteralValue.Numeric)((SelectColumn.ExpressionItem)one.Columns[0]).Expression).Text)));
		}
	}

	// ── Random trees ─────────────────────────────────────────────────────────────

	/// <summary>Random trees of the standard's operators, over names and numbers, read back as themselves.</summary>
	[Fact]
	public void A_random_tree_reads_back_as_itself_in_the_standard()
	{
		RoundTrip(Standard, new StandardTrees(new System.Random(20261016)), count: 3000);
	}

	/// <summary>Random trees of T-SQL's operators, values in a SELECT list and conditions in a WHERE, read back as themselves.</summary>
	[Fact]
	public void A_random_tree_reads_back_as_itself_in_TSQL()
	{
		RoundTrip(TransactSql, new TransactSqlTrees(new System.Random(20261017)), count: 3000);
	}

	static void RoundTrip(Reading reading, Trees trees, int count)
	{
		var read    = 0;
		var refused = 0;

		for (var at = 0; at < count; at++)
		{
			var condition = trees.Next(2) == 0;
			var tree      = condition ? trees.Condition(4) : trees.Value(4);
			var text      = reading.Write(tree, condition);
			var back      = reading.Read(text, condition);

			if (back is null)
			{
				// A text the reading refuses for its types, not for its brackets: bracketed throughout it is refused too.
				var bracketed = reading.Write(Bracketed(tree), condition);

				Assert.True(reading.Read(bracketed, condition) is null, $"{reading.Name} refuses {text} but reads {bracketed}");
				refused++;

				continue;
			}

			Assert.True(tree.Equals(Unbracketed(back)), $"{reading.Name} reads {text} as another tree");

			// Every bracket is needed: without any one of them the text means something else, or nothing.
			foreach (var (open, close) in Pairs(text))
			{
				var without = text.Remove(close, 1).Remove(open, 1);
				var other   = reading.Read(without, condition);

				Assert.False(other is not null && tree.Equals(Unbracketed(other)), $"{reading.Name}: {text} needs no bracket at {open}, {without} means the same");
			}

			read++;
		}

		// The rows are about the trees that are read; a generator whose trees the reading mostly refuses tests little.
		Assert.True(read >= count * 3 / 4, $"{reading.Name} read {read} of {count} random trees and refused {refused}");
	}

	/// <summary>The brackets of a text, each opening with its closing one.</summary>
	static IEnumerable<(int Open, int Close)> Pairs(string text)
	{
		var opened = new Stack<int>();
		var pairs  = new List<(int, int)>();

		for (var at = 0; at < text.Length; at++)
		{
			if (text[at] == '(')
				opened.Push(at);
			else if (text[at] == ')')
				pairs.Add((opened.Pop(), at));
		}

		return pairs;
	}

	/// <summary>A generator of trees: values and conditions of a dialect's operators, to a depth.</summary>
	abstract class Trees(System.Random random)
	{
		static readonly string[] Names = ["a", "b", "c", "d", "e"];

		public int Next(int bound)
		{
			return random.Next(bound);
		}

		protected Expression Leaf(bool names)
		{
			return names || Next(3) > 0 ? Ref(Names[Next(Names.Length)]) : Number((Next(9) + 1).ToString());
		}

		protected Expression Collation(Expression value)
		{
			return Collate(value, Next(2) == 0 ? "x" : "y");
		}

		public abstract Expression Value(int depth);

		/// <summary>The value a predicate holds, which the standard types and T-SQL does not.</summary>
		protected virtual Expression Operand(int depth)
		{
			return Value(depth);
		}

		/// <summary>A pattern or an escape of LIKE.</summary>
		protected virtual Expression Pattern(int depth)
		{
			return Value(depth);
		}

		protected virtual Expression? Truth(int depth)
		{
			return null;
		}

		public Expression Condition(int depth)
		{
			if (depth <= 0)
				return Equal(Leaf(names: false), Leaf(names: false));

			switch (Next(12))
			{
				case 0:
				case 1:
					return Binary(Condition(depth - 1), BinaryOperator.And, Condition(depth - 1));

				case 2:
				case 3:
					return Binary(Condition(depth - 1), BinaryOperator.Or, Condition(depth - 1));

				case 4:
					return Unary(UnaryOperator.Not, Condition(depth - 1));

				case 5:
					return new Expression.IsNull(Operand(depth - 1), Next(2) == 0);

				case 6:
					return new Expression.Between(Operand(depth - 1), Next(2) == 0, null, Operand(depth - 1), Operand(depth - 1));

				case 7:
					return new Expression.Like(Pattern(depth - 1), Next(2) == 0, LikeKind.Like, Pattern(depth - 1), Next(3) == 0 ? Pattern(depth - 1) : null);

				case 8:
					return new Expression.In(Operand(depth - 1), Next(2) == 0, new InSource.Values(Next(2) == 0 ? [Leaf(names: false)] : [Leaf(names: false), Leaf(names: false)]));

				case 9:
					return new Expression.IsDistinct(Operand(depth - 1), Next(2) == 0, Operand(depth - 1));

				case 10:
					return Truth(depth) ?? Unary(UnaryOperator.Not, Condition(depth - 1));

				default:
					return new Expression.Comparison(Operand(depth - 1), (ComparisonOperator)Next(6), Operand(depth - 1));
			}
		}
	}

	/// <summary>
	/// The standard's operators, each in a tower its operands can stand in: numbers under `+ - * /` and the signs,
	/// characters under `||` and COLLATE, datetimes under AT, multisets under MULTISET; and now and then a condition
	/// where a value stands, which is a boolean predicand.
	/// </summary>
	sealed class StandardTrees(System.Random random) : Trees(random)
	{
		static readonly BinaryOperator[] Arithmetic = [BinaryOperator.Add, BinaryOperator.Subtract, BinaryOperator.Multiply, BinaryOperator.Divide];

		public override Expression Value(int depth)
		{
			if (depth <= 0)
				return Leaf(names: false);

			switch (Next(10))
			{
				case 0:
					return Characters(depth);

				case 1:
					return Zone(Datetime(depth - 1), Numeric(depth - 1, names: true));

				case 2:
					return Multisets(depth);

				case 3:
					return Condition(depth - 1);

				default:
					return Numeric(depth, names: false);
			}
		}

		Expression Numeric(int depth, bool names)
		{
			if (depth <= 0 || Next(4) == 0)
				return Leaf(names);

			if (Next(4) == 0)
				return Unary(Next(2) == 0 ? UnaryOperator.Minus : UnaryOperator.Plus, Numeric(depth - 1, names));

			return Binary(Numeric(depth - 1, names), Arithmetic[Next(Arithmetic.Length)], Numeric(depth - 1, names));
		}

		Expression Characters(int depth)
		{
			if (depth <= 0 || Next(4) == 0)
				return Leaf(names: true);

			return Next(3) == 0
				? Collation(Characters(depth - 1))
				: Binary(Characters(depth - 1), BinaryOperator.Concatenate, Characters(depth - 1));
		}

		Expression Datetime(int depth)
		{
			return depth <= 0 || Next(2) == 0 ? Leaf(names: true) : Zone(Datetime(depth - 1), Leaf(names: true));
		}

		Expression Multisets(int depth)
		{
			if (depth <= 0 || Next(4) == 0)
				return Leaf(names: true);

			return Multiset(Multisets(depth - 1), (MultisetOperator)Next(3), Multisets(depth - 1));
		}

		protected override Expression Operand(int depth)
		{
			return Next(5) == 0 ? Value(depth) : Numeric(depth, names: false);
		}

		protected override Expression Pattern(int depth)
		{
			return Characters(depth);
		}

		protected override Expression? Truth(int depth)
		{
			return new Expression.IsTruth(Condition(depth - 1), Next(2) == 0, (BooleanLiteral)Next(3));
		}
	}

	/// <summary>T-SQL's operators, which its grammar does not type: any value under any of them.</summary>
	sealed class TransactSqlTrees(System.Random random) : Trees(random)
	{
		static readonly BinaryOperator[] Operators =
		[
			BinaryOperator.Add, BinaryOperator.Subtract, BinaryOperator.Multiply, BinaryOperator.Divide, BinaryOperator.Modulo,
			BinaryOperator.Concatenate, BinaryOperator.BitwiseAnd, BinaryOperator.BitwiseOr, BinaryOperator.BitwiseXor,
			BinaryOperator.ShiftLeft, BinaryOperator.ShiftRight,
		];

		static readonly UnaryOperator[] Signs = [UnaryOperator.Minus, UnaryOperator.Plus, UnaryOperator.BitwiseNot];

		public override Expression Value(int depth)
		{
			if (depth <= 0 || Next(5) == 0)
				return Leaf(names: false);

			switch (Next(10))
			{
				case 0:
				case 1:
					return Unary(Signs[Next(Signs.Length)], Value(depth - 1));

				case 2:
					return Collation(Value(depth - 1));

				case 3:
					return Zone(Value(depth - 1), Value(depth - 1));

				default:
					return Binary(Value(depth - 1), Operators[Next(Operators.Length)], Value(depth - 1));
			}
		}
	}
}
