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

	static readonly TableSource T = new TableSource.Named(new QualifiedName(new Identifier("t")));

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
			"OperatorCall under a prefix OperatorCall" => Called("@", null, Called("~", A, B)),
			"a prefix OperatorCall under OperatorCall" => Called("~", Called("@", null, A), B),
			"= with NOT on the right under ="    => Equal(Equal(A, Not(B)), C),
			"= under = on the left"              => Equal(Equal(A, B), C),
			"= with NOT on the right under IS NULL" => IsNull(Equal(A, Not(B))),
			"IS DISTINCT FROM NOT under IS NULL" => IsNull(new Expression.IsDistinct(A, false, Not(B))),
			"a core operator quantified"         => new Expression.QuantifiedOperation(A, new QuantifiedOperator.Named(new QualifiedName(new Identifier("+"))), Quantifier.Any, new Expression.Array([Number("1")])),
			"a lone SECOND's fractional precision" => new Expression.Cast(A, new DataType.Interval(new IntervalQualifier(DateTimeField.Second, null, null, 3))) { Spelling = CastSpelling.DoubleColon },
			"a call under a subscript"           => new Expression.Element(new Expression.Invocation(new QualifiedName(new Identifier("f")), [new Argument(A)]), Number("1")),
			"a string under a member"            => new Expression.Member(new LiteralValue.String("'x'", StringLiteralKind.Character), MemberAccessKind.Dot, new Identifier("f")),
			"a subscript under a member"         => new Expression.Member(new Expression.Element(A, Number("1")), MemberAccessKind.Dot, new Identifier("f")),
			"a prefix OperatorCall on the right of ||" => Binary(A, BinaryOperator.Concatenate, Called("!!", null, B)),
			"a prefix OperatorCall on the right of an OperatorCall" => Called("~", A, Called("@", null, B)),
			"a prefix OperatorCall on the right of +" => Binary(A, BinaryOperator.Add, Called("@", null, B)),
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
	[InlineData("OperatorCall under a prefix OperatorCall", "@ (a ~ b)")]
	[InlineData("a prefix OperatorCall under OperatorCall", "@ a ~ b")]
	[InlineData("= with NOT on the right under =",    "(a = NOT b) = c")]
	[InlineData("= under = on the left",              "(a = b) = c")]
	[InlineData("= with NOT on the right under IS NULL", "(a = NOT b) IS NULL")]
	[InlineData("IS DISTINCT FROM NOT under IS NULL", "(a IS DISTINCT FROM NOT b) IS NULL")]
	[InlineData("a core operator quantified",         "a + ANY(ARRAY[1])")]
	[InlineData("a lone SECOND's fractional precision", "a::INTERVAL SECOND(3)")]
	[InlineData("a call under a subscript",           "(f(a))[1]")]
	[InlineData("a string under a member",            "('x').f")]
	[InlineData("a subscript under a member",         "a[1].f")]
	[InlineData("a prefix OperatorCall on the right of ||", "a || !! b")]
	[InlineData("a prefix OperatorCall on the right of an OperatorCall", "a ~ @ b")]
	[InlineData("a prefix OperatorCall on the right of +", "a + @ b")]
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
	[InlineData("IntervalQualifier.FractionalAlone")]
	[InlineData("Insert.Alias")]
	[InlineData("Insert.Conflicts")]
	[InlineData("ReturningClause.Returned")]
	[InlineData("Delete.Using")]
	[InlineData("MergeMatchedAction.DoNothing")]
	[InlineData("Query.DataChange")]
	[InlineData("TruncateTable.Others")]
	[InlineData("StartTransaction.Begin")]
	[InlineData("Commit.End")]
	[InlineData("Rollback.Abort")]
	[InlineData("TransactionMode.Deferrable")]
	[InlineData("SetTimeZone.Scope")]
	[InlineData("SetNames.Value null")]
	[InlineData("SetParameter")]
	[InlineData("ShowParameter")]
	[InlineData("Explain")]
	[InlineData("Do")]
	[InlineData("PreparePlan")]
	[InlineData("Listen")]
	[InlineData("LockTable")]
	[InlineData("Discard")]
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
			"IntervalQualifier.FractionalAlone" => new DataType.Interval(new IntervalQualifier(DateTimeField.Second, null, null, 3)),
			"Insert.Alias"         => new Statement.Insert { Target = T, Alias = new Alias(new Identifier("x"), null, true) },
			"Insert.Conflicts"     => new Statement.Insert { Target = T, Conflicts = [new ConflictClause(null, new ConflictResolution.Nothing())] },
			"ReturningClause.Returned" => new Statement.Delete { Target = T, Returning = new ReturningClause.Returned([new SelectColumn.All()]) },
			"Delete.Using"         => new Statement.Delete { Target = T, Using = new FromClause([T]) },
			"MergeMatchedAction.DoNothing" => new Statement.Merge { Target = T, SourceTable = new TableSource.Named(new QualifiedName(new Identifier("s"))), On = A, Clauses = [new MergeClause.Matched(new MergeMatchedAction.DoNothing())] },
			"Query.DataChange"     => new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.All()], From = new FromClause([T]), With = new WithClause(false, [new CommonTableExpression(new Identifier("x"), [], new Query.DataChange(new Statement.Delete { Target = T }))]) } },
			"TruncateTable.Others" => new Statement.TruncateTable { Target = T, Others = [T], Behavior = DropBehavior.Cascade, TableOmitted = true },
			"StartTransaction.Begin" => new Statement.StartTransaction { Begin = true },
			"Commit.End"           => new Statement.Commit { End = true },
			"Rollback.Abort"       => new Statement.Rollback { Abort = true },
			"TransactionMode.Deferrable" => new Statement.StartTransaction { Modes = [new TransactionMode.Deferrable(false)] },
			"SetTimeZone.Scope"    => new Statement.SetTimeZone { Local = true, Scope = SetScope.Local },
			"SetNames.Value null"  => new Statement.SetNames(),
			"SetParameter"         => new Statement.SetParameter(new QualifiedName(new Identifier("p")), [A]),
			"ShowParameter"        => new Statement.ShowParameter(null),
			"Explain"              => new Statement.Explain([], new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.All()], From = new FromClause([T]) } }),
			"Do"                   => new Statement.Do(new LiteralValue.String("'x'", StringLiteralKind.Character)),
			"PreparePlan"          => new Statement.PreparePlan(new Identifier("p"), [], new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.All()], From = new FromClause([T]) } }),
			"Listen"               => new Statement.Listen(new Identifier("c")),
			"LockTable"            => new Statement.LockTable([T]),
			"Discard"              => new Statement.Discard(DiscardTarget.All),
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
	[InlineData("Insert.Conflicts twice")]
	[InlineData("Query.DataChange.With")]
	[InlineData("TransactionKeyword.Tran")]
	[InlineData("Commit.Name")]
	[InlineData("Commit.DelayedDurability")]
	[InlineData("TransactionMode.DiagnosticsSize")]
	[InlineData("IsolationLevel.Snapshot")]
	[InlineData("StartTransaction.Work without Begin")]
	[InlineData("Rollback.ToSavepoint with Chain")]
	[InlineData("SetSessionCharacteristics twice")]
	[InlineData("Execute.Result")]
	[InlineData("Execute.Parameters descriptor")]
	[InlineData("Explain.Bare with a value")]
	[InlineData("Explain of a TRUNCATE")]
	[InlineData("PreparePlan of an EXECUTE")]
	[InlineData("SetParameter.FromCurrent with values")]
	[InlineData("ShowParameter.Spelling of another name")]
	[InlineData("Checkpoint.Duration")]
	[InlineData("LockTable of a subquery")]
	[InlineData("Discard.Temporary on PLANS")]
	[InlineData("OperatorCall +")]
	[InlineData("OperatorCall =")]
	[InlineData("OperatorCall ||")]
	[InlineData("OperatorCall =>")]
	[InlineData("OperatorCall a")]
	[InlineData("OperatorCall --")]
	[InlineData("OperatorCall <-")]
	[InlineData("OperatorCall empty")]
	[InlineData("OperatorCall qualified bare")]
	[InlineData("QuantifiedOperation =")]
	[InlineData("QuantifiedOperation =>")]
	[InlineData("ReturningClause.Output")]
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
			"QuantifiedOperation ="      => new Expression.QuantifiedOperation(A, new QuantifiedOperator.Named(new QualifiedName(new Identifier("="))), Quantifier.Any, B),
			"QuantifiedOperation =>"     => new Expression.QuantifiedOperation(A, new QuantifiedOperator.Named(new QualifiedName(new Identifier("=>"))), Quantifier.Any, B),
			"ReturningClause.Output"     => new Statement.Delete { Target = T, Returning = new ReturningClause.Output([new SelectColumn.ExpressionItem(A)], null, null, null) },
			"Insert.Conflicts twice"     => new Statement.Insert { Target = T, Conflicts = [new ConflictClause(null, new ConflictResolution.Nothing()), new ConflictClause(null, new ConflictResolution.Nothing())] },
			"TransactionKeyword.Tran"    => new Statement.Commit { Keyword = TransactionKeyword.Tran },
			"Commit.Name"                => new Statement.Commit { Name = new TransactionName(new Identifier("x")) },
			"Commit.DelayedDurability"   => new Statement.Commit { DelayedDurability = true },
			"TransactionMode.DiagnosticsSize" => new Statement.StartTransaction { Modes = [new TransactionMode.DiagnosticsSize(A)] },
			"IsolationLevel.Snapshot"    => new Statement.StartTransaction { Modes = [new TransactionMode.Isolation(IsolationLevel.Snapshot)] },
			"StartTransaction.Work without Begin" => new Statement.StartTransaction { Work = true },
			"Rollback.ToSavepoint with Chain" => new Statement.Rollback { ToSavepoint = new Identifier("s"), Chain = ChainMode.Chain },
			"SetSessionCharacteristics twice" => new Statement.SetSessionCharacteristics { Characteristics = [[new TransactionMode.Access(TransactionAccess.ReadOnly)], [new TransactionMode.Access(TransactionAccess.ReadWrite)]] },
			"Execute.Result"             => new Statement.Execute { Statement = new StatementReference(new Identifier("p")), Result = new DynamicArguments.Values([A]) },
			"Execute.Parameters descriptor" => new Statement.Execute { Statement = new StatementReference(new Identifier("p")), Parameters = new DynamicArguments.Descriptor(new DescriptorReference(new Identifier("d")), false) },
			"Explain.Bare with a value"  => new Statement.Explain([new Option(new Identifier("verbose"), A, [])], new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.All()], From = new FromClause([T]) } }) { Bare = true },
			"Explain of a TRUNCATE"      => new Statement.Explain([], new Statement.TruncateTable { Target = T }),
			"PreparePlan of an EXECUTE"  => new Statement.PreparePlan(new Identifier("p"), [], new Statement.Execute { Statement = new StatementReference(new Identifier("q")) }),
			"SetParameter.FromCurrent with values" => new Statement.SetParameter(new QualifiedName(new Identifier("p")), [A]) { FromCurrent = true },
			"ShowParameter.Spelling of another name" => new Statement.ShowParameter(new QualifiedName(new Identifier("p"))) { Spelling = ParameterSpelling.TimeZone },
			"Checkpoint.Duration"        => new Statement.Checkpoint(A),
			"LockTable of a subquery"    => new Statement.LockTable([new TableSource.Subquery(new Query.Select { Columns = [new SelectColumn.All()] }) { Alias = new Alias(new Identifier("x"), null, true) }]),
			"Discard.Temporary on PLANS" => new Statement.Discard(DiscardTarget.Plans) { Temporary = true },
			"Query.DataChange.With"      => new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.All()], With = new WithClause(false, [new CommonTableExpression(new Identifier("x"), [], new Query.DataChange(new Statement.Delete { Target = T }) { With = new WithClause(false, [new CommonTableExpression(new Identifier("y"), [], new Query.Select { Columns = [new SelectColumn.All()] })]) })]) } },
			_ => throw new ArgumentOutOfRangeException(nameof(which)),
		};

		Assert.Throws<ArgumentException>(() => PostgreSqlWriter.Write(node));
	}

	/// <summary>
	/// An item of ON CONFLICT's target that PostgreSQL's index_elem reads bare is written bare; any other expression gets
	/// the brackets, and the text reads back with them.
	/// </summary>
	[Fact]
	public void A_conflict_target_item_is_written_as_an_index_element()
	{
		var tree = new Statement.Insert
		{
			Target    = T,
			Conflicts = [new ConflictClause(new ConflictTarget.Columns([A, new Expression.Invocation(new QualifiedName(new Identifier("f")), [new Argument(A)]), new Expression.Parenthesized(Binary(A, BinaryOperator.Add, B)), new Expression.Cast(A, Int), Binary(A, BinaryOperator.Add, B), new Expression.Reference(QualifiedName.Of(new Identifier("a"), new Identifier("b")))], null), new ConflictResolution.Nothing())],
		};

		var written = PostgreSqlWriter.Write(tree);

		Assert.Equal("INSERT INTO t DEFAULT VALUES ON CONFLICT (a, f(a), (a + b), CAST(a AS INT), (a + b), (a.b)) DO NOTHING", written);
		Assert.True(PostgreSqlParser.TryParseStatement(written).IsSuccess, written);
	}
}
