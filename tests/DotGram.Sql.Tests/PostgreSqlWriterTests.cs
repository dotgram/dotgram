using System;

using DotGram.Sql.Ast;
using DotGram.Sql.PostgreSql;

using Xunit;

namespace DotGram.Sql.Tests;

using DataType = DotGram.Sql.Ast.DataType;
using Expression = DotGram.Sql.Ast.Expression;

/// <summary>
/// <see cref="PostgreSqlWriter"/>'s ladder is the grammar's: a tree built by hand is written with the brackets its shape
/// needs under PostgreSQL's binding powers and none it does not, and reads back as itself. And a reading's own nodes
/// and spellings are its own: what only PostgreSQL reads, the standard's writer and T-SQL's refuse, and what PostgreSQL
/// reads as another node, its writer refuses.
/// </summary>
public sealed class PostgreSqlWriterTests
{
	static readonly Expression A = new Expression.Reference(new QualifiedName(new Identifier("a")));
	static readonly Expression B = new Expression.Reference(new QualifiedName(new Identifier("b")));
	static readonly Expression C = new Expression.Reference(new QualifiedName(new Identifier("c")));

	static readonly DataType Int = new DataType.Numeric(NumericTypeKind.Int);

	static Expression Number(string text)
	{
		return new LiteralValue.Numeric(text, NumericLiteralKind.DecimalInteger);
	}

	static Expression Called(string symbol, Expression? left, Expression right)
	{
		return new Expression.OperatorCall(new QualifiedName(new Identifier(symbol)), left, right);
	}

	static Expression Binary(Expression left, BinaryOperator op, Expression right)
	{
		return new Expression.Binary(left, op, right);
	}

	static Expression Equal(Expression left, Expression right)
	{
		return new Expression.Comparison(left, ComparisonOperator.Equal, right);
	}

	static Expression Not(Expression operand)
	{
		return new Expression.Unary(UnaryOperator.Not, operand);
	}

	static Expression Minus(Expression operand)
	{
		return new Expression.Unary(UnaryOperator.Minus, operand);
	}

	static Expression IsNull(Expression value)
	{
		return new Expression.IsNull(value, false);
	}

	static Expression Collated(Expression value)
	{
		return new Expression.Collate(value, new CollationName(new QualifiedName(new Identifier("\"C\"", IdentifierStyle.Delimited))));
	}

	static Expression DoubleColon(Expression value)
	{
		return new Expression.Cast(value, Int) { Spelling = CastSpelling.DoubleColon };
	}

	/// <summary>The tree each case names: an operand that binds looser than its operator under PostgreSQL's ladder, or one that does not.</summary>
	static Expression Tree(string which)
	{
		return which switch
		{
			"OperatorCall under Add"             => Binary(Called("~", A, B), BinaryOperator.Add, C),
			"Add under OperatorCall"             => Called("~", A, Binary(B, BinaryOperator.Add, C)),
			"Multiply under Power"               => Binary(Binary(A, BinaryOperator.Multiply, B), BinaryOperator.Power, C),
			"Power under Multiply"               => Binary(Binary(A, BinaryOperator.Power, B), BinaryOperator.Multiply, C),
			"Concatenate under Add"              => Binary(Binary(A, BinaryOperator.Concatenate, B), BinaryOperator.Add, C),
			"Add under Concatenate"              => Binary(A, BinaryOperator.Concatenate, Binary(B, BinaryOperator.Add, C)),
			"OperatorCall nested on the right"   => Called("~", A, Called("~", B, C)),
			"OperatorCall nested on the left"    => Called("~", Called("~", A, B), C),
			"Add under a prefix OperatorCall"    => Called("|/", null, Binary(Number("25"), BinaryOperator.Add, Number("11"))),
			"prefix OperatorCall under Multiply" => Binary(Called("|/", null, A), BinaryOperator.Multiply, B),
			"Add under a sign"                   => Minus(Binary(A, BinaryOperator.Add, B)),
			"a sign under Power"                 => Binary(Number("2"), BinaryOperator.Power, Minus(Number("3"))),
			"a sign under COLLATE"               => Collated(Minus(A)),
			"COLLATE under a sign"               => Minus(Collated(A)),
			"a sign as the zone"                 => new Expression.AtTimeZone(A, Minus(Number("1"))),
			"Add as the zone"                    => new Expression.AtTimeZone(A, Binary(B, BinaryOperator.Add, C)),
			"Add under ::"                       => DoubleColon(Binary(A, BinaryOperator.Add, B)),
			":: under a subscript"               => new Expression.Element(DoubleColon(A), Number("1")),
			"IS NULL on the left of ="           => Equal(IsNull(Number("1")), new LiteralValue.Boolean(BooleanLiteral.False)),
			"IS NULL on the right of ="          => Equal(A, IsNull(B)),
			"= under IS NULL"                    => IsNull(Equal(A, B)),
			"IS DISTINCT FROM under IS NULL"     => IsNull(new Expression.IsDistinct(A, false, B)),
			"IS DISTINCT FROM on the left of ="  => Equal(new Expression.IsDistinct(A, false, B), C),
			"= on the right of IS DISTINCT FROM" => new Expression.IsDistinct(A, false, Equal(B, C)),
			"NOT on the right of ="              => Equal(A, Not(B)),
			"NOT on the left of ="               => Equal(Not(A), B),
			"OperatorCall under BETWEEN"         => new Expression.Between(Called("~", A, B), false, null, C, C),
			"= under BETWEEN"                    => new Expression.Between(Equal(A, B), false, null, C, C),
			"= as CASE's operand"                => new Expression.Case(Equal(A, B), [new CaseWhen([Number("1")], Number("2"))], null),
			"NOT under NOT"                      => Not(Not(A)),
			"a sign under a sign"                => Minus(Minus(A)),
			_ => throw new ArgumentOutOfRangeException(nameof(which)),
		};
	}

	[Theory]
	[InlineData("OperatorCall under Add",             "(a ~ b) + c")]
	[InlineData("Add under OperatorCall",             "a ~ b + c")]
	[InlineData("Multiply under Power",               "(a * b) ^ c")]
	[InlineData("Power under Multiply",               "a ^ b * c")]
	[InlineData("Concatenate under Add",              "(a || b) + c")]
	[InlineData("Add under Concatenate",              "a || b + c")]
	[InlineData("OperatorCall nested on the right",   "a ~ (b ~ c)")]
	[InlineData("OperatorCall nested on the left",    "a ~ b ~ c")]
	[InlineData("Add under a prefix OperatorCall",    "|/ 25 + 11")]
	[InlineData("prefix OperatorCall under Multiply", "(|/ a) * b")]
	[InlineData("Add under a sign",                   "-(a + b)")]
	[InlineData("a sign under Power",                 "2 ^ -3")]
	[InlineData("a sign under COLLATE",               "-a COLLATE \"C\"")]
	[InlineData("COLLATE under a sign",               "-(a COLLATE \"C\")")]
	[InlineData("a sign as the zone",                 "a AT TIME ZONE -1")]
	[InlineData("Add as the zone",                    "a AT TIME ZONE (b + c)")]
	[InlineData("Add under ::",                       "(a + b)::INT")]
	[InlineData(":: under a subscript",               "(a::INT)[1]")]
	[InlineData("IS NULL on the left of =",           "1 IS NULL = FALSE")]
	[InlineData("IS NULL on the right of =",          "a = (b IS NULL)")]
	[InlineData("= under IS NULL",                    "a = b IS NULL")]
	[InlineData("IS DISTINCT FROM under IS NULL",     "(a IS DISTINCT FROM b) IS NULL")]
	[InlineData("IS DISTINCT FROM on the left of =",  "(a IS DISTINCT FROM b) = c")]
	[InlineData("= on the right of IS DISTINCT FROM", "a IS DISTINCT FROM b = c")]
	[InlineData("NOT on the right of =",              "a = NOT b")]
	[InlineData("NOT on the left of =",               "(NOT a) = b")]
	[InlineData("OperatorCall under BETWEEN",         "a ~ b BETWEEN c AND c")]
	[InlineData("= under BETWEEN",                    "(a = b) BETWEEN c AND c")]
	[InlineData("= as CASE's operand",                "CASE a = b WHEN 1 THEN 2 END")]
	[InlineData("NOT under NOT",                      "NOT NOT a")]
	[InlineData("a sign under a sign",                "- -a")]
	public void A_tree_is_written_with_the_brackets_its_shape_needs_and_reads_back(string which, string expected)
	{
		var tree    = Tree(which);
		var written = PostgreSqlWriter.Write(tree);

		Assert.Equal(expected, written);

		var back = PostgreSqlParser.ParseExpression(written);

		// A bracket the writer added is a Parenthesized node when the text is read, so a tree written with one reads
		// back as itself in that bracket, and its text is written again unchanged; a tree written without reads back as itself.
		Assert.IsType(tree.GetType(), back);

		if (written.Contains('(', StringComparison.Ordinal))
			Assert.Equal(written, PostgreSqlWriter.Write(back));
		else
			Assert.True(tree.Equals(back), $"{which}: written as {written}, which reads back as {back}");
	}

	/// <summary>What only PostgreSQL reads has no text in the standard's reading or T-SQL's, and both writers refuse it.</summary>
	[Theory]
	[InlineData("OperatorCall")]
	[InlineData("QuantifiedOperation")]
	[InlineData("IsDocument")]
	[InlineData("Cast.DoubleColon")]
	[InlineData("Numeric.Int4")]
	[InlineData("Boolean.Abbreviated")]
	[InlineData("DateTime.Abbreviated")]
	[InlineData("Interval.Precision")]
	[InlineData("Array.Brackets")]
	[InlineData("Element.Colon")]
	[InlineData("Substring.ForFirst")]
	[InlineData("Like.ILike")]
	[InlineData("IsNull.Postfix")]
	[InlineData("Parameter.Positional")]
	[InlineData("Argument.Variadic")]
	[InlineData("Argument.ColonEquals")]
	[InlineData("String.Escaped")]
	[InlineData("String.Dollar")]
	public void What_only_PostgreSQL_reads_is_refused_by_the_other_writers(string which)
	{
		ISqlNode node = which switch
		{
			"OperatorCall"         => Called("~", A, B),
			"QuantifiedOperation"  => new Expression.QuantifiedOperation(A, new QuantifiedOperator.Comparison(ComparisonOperator.Equal), Quantifier.Any, B),
			"IsDocument"           => new Expression.IsDocument(A, false),
			"Cast.DoubleColon"     => DoubleColon(A),
			"Numeric.Int4"         => new DataType.Numeric(NumericTypeKind.Int4),
			"Boolean.Abbreviated"  => new DataType.Boolean { Abbreviated = true },
			"DateTime.Abbreviated" => new DataType.DateTime(DateTimeTypeKind.Timestamp, null, TimeZoneMode.With) { Abbreviated = true },
			"Interval.Precision"   => new DataType.Interval(null) { Precision = 3 },
			"Array.Brackets"       => new DataType.Array(Int, null) { Brackets = true },
			"Element.Colon"        => new Expression.Element(A, Number("1"), Number("2")) { Colon = true },
			"Substring.ForFirst"   => new Expression.Substring(A, Number("1"), Number("2"), null) { ForFirst = true },
			"Like.ILike"           => new Expression.Like(A, false, LikeKind.ILike, B),
			"IsNull.Postfix"       => new Expression.IsNull(A, false) { Postfix = true },
			"Parameter.Positional" => new Expression.Parameter(ParameterKind.Positional) { Ordinal = 1 },
			"Argument.Variadic"    => new Expression.Invocation(new QualifiedName(new Identifier("f")), [new Argument(A) { Variadic = true }]),
			"Argument.ColonEquals" => new Expression.Invocation(new QualifiedName(new Identifier("f")), [new Argument(A, new Identifier("p"), true) { ColonEquals = true }]),
			"String.Escaped"       => new LiteralValue.String("'x'", StringLiteralKind.Escaped),
			"String.Dollar"        => new LiteralValue.String("$$x$$", StringLiteralKind.Dollar),
			_ => throw new ArgumentOutOfRangeException(nameof(which)),
		};

		Assert.Throws<ArgumentException>(() => SqlWriter.Write(node));
		Assert.Throws<ArgumentException>(() => TransactSqlWriter.Write(node));
		Assert.False(string.IsNullOrEmpty(PostgreSqlWriter.Write(node)));
	}

	/// <summary>
	/// What PostgreSQL reads as another node is refused by its writer, so that no text reads back as a different tree:
	/// T-SQL's bitwise operators are operators by name there, and an operator by name is none of the standard's and is
	/// one run of operator characters.
	/// </summary>
	[Theory]
	[InlineData("Binary.BitwiseAnd")]
	[InlineData("Binary.BitwiseOr")]
	[InlineData("Binary.BitwiseXor")]
	[InlineData("Binary.ShiftLeft")]
	[InlineData("Binary.ShiftRight")]
	[InlineData("Unary.BitwiseNot")]
	[InlineData("OperatorCall +")]
	[InlineData("OperatorCall =")]
	[InlineData("OperatorCall ||")]
	[InlineData("OperatorCall =>")]
	[InlineData("OperatorCall a")]
	[InlineData("OperatorCall --")]
	[InlineData("OperatorCall <-")]
	[InlineData("OperatorCall empty")]
	[InlineData("OperatorCall qualified bare")]
	public void What_PostgreSQL_reads_as_another_node_is_refused_by_its_writer(string which)
	{
		ISqlNode node = which switch
		{
			"Binary.BitwiseAnd"          => Binary(A, BinaryOperator.BitwiseAnd, B),
			"Binary.BitwiseOr"           => Binary(A, BinaryOperator.BitwiseOr, B),
			"Binary.BitwiseXor"          => Binary(A, BinaryOperator.BitwiseXor, B),
			"Binary.ShiftLeft"           => Binary(A, BinaryOperator.ShiftLeft, B),
			"Binary.ShiftRight"          => Binary(A, BinaryOperator.ShiftRight, B),
			"Unary.BitwiseNot"           => new Expression.Unary(UnaryOperator.BitwiseNot, A),
			"OperatorCall +"             => Called("+", A, B),
			"OperatorCall ="             => Called("=", A, B),
			"OperatorCall ||"            => Called("||", A, B),
			"OperatorCall =>"            => Called("=>", A, B),
			"OperatorCall a"             => Called("a", A, B),
			"OperatorCall --"            => Called("--", A, B),
			"OperatorCall <-"            => Called("<-", A, B),
			"OperatorCall empty"         => Called("", A, B),
			"OperatorCall qualified bare" => new Expression.OperatorCall(new QualifiedName(new Identifier("s"), [new Identifier("~")]), A, B),
			_ => throw new ArgumentOutOfRangeException(nameof(which)),
		};

		Assert.Throws<ArgumentException>(() => PostgreSqlWriter.Write(node));
	}
}
