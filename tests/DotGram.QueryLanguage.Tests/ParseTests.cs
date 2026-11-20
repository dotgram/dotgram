using System;
using System.Diagnostics;
using System.Linq;

using DotGram.QueryLanguage.Syntax;

using Xunit;

namespace DotGram.QueryLanguage.Tests;

/// <summary>The text as written: what reads, what is refused, and where.</summary>
public sealed class ParseTests
{
	static QuerySyntax Read(string text, QlOptions? options = null)
	{
		return Reader.Read(text, options ?? QlOptions.Default);
	}

	static QlError Refused(string text, QlOptions? options = null)
	{
		return Assert.Throws<QlException>(() => Read(text, options)).Error;
	}

	[Fact]
	public void ClausesComeInAnyOrderAndAnyNumber()
	{
		var query = Read("from orders take 5 where a > 1 order by b select c where c < 2 let d = c + 1 skip 1 order by d desc select d select *");

		Assert.Equal("orders", query.Root.Text);
		Assert.Null(query.Alias);

		Assert.Equal(
			["PageClause", "WhereClause", "OrderClause", "SelectClause", "WhereClause", "LetClause", "PageClause", "OrderClause", "SelectClause", "SelectClause"],
			query.Clauses.Select(c => c.GetType().Name));

		Assert.Null(((SelectClause)query.Clauses[^1]).Items);
	}

	[Fact]
	public void AQueryWithoutClausesIsTheEntity()
	{
		Assert.Empty(Read("from orders").Clauses);
		Assert.Empty(Read("FROM Orders").Clauses);
	}

	[Theory]
	[InlineData("from orders o where o.a > 1",         "o")]
	[InlineData("from orders as o where o.a > 1",      "o")]
	[InlineData("from orders as order order by id",    "order")]
	[InlineData("from orders as where where a > 1",    "where")]
	public void AnAliasIsANameAfterTheEntity(string text, string alias)
	{
		Assert.Equal(alias, Read(text).Alias!.Text);
	}

	[Fact]
	public void AnAliasWithoutAsIsNeverAClauseWord()
	{
		var query = Read("from orders order by id");

		Assert.Null(query.Alias);
		Assert.IsType<OrderClause>(Assert.Single(query.Clauses));
	}

	[Theory]
	[InlineData("from Order order by Total")]
	[InlineData("from orders select first, last, desc, asc, nulls")]
	[InlineData("from orders where select > 1 && order == 2 && where != 3")]
	[InlineData("from orders let from = take + skip select from")]
	[InlineData("from orders order by desc desc nulls first, asc asc")]
	[InlineData("from orders where in in (1, 2) && between between 1 and 2 && is is null")]
	[InlineData("from orders select Count as as")]
	public void WordsOfTheLanguageAreNamesWhereANameStands(string text)
	{
		Read(text);
	}

	[Theory]
	[InlineData("from orders select a as true", 24)]
	[InlineData("from orders let true = 1",     16)]
	[InlineData("from false",                   5)]
	public void NullTrueAndFalseAreNeverNames(string text, int position)
	{
		var error = Refused(text);

		Assert.Equal(QlErrorCode.Syntax, error.Code);
		Assert.Equal(position, error.Position);
	}

	[Fact]
	public void BothQuotesAreStrings()
	{
		var query = Read("from t where a == 'x' || b == \"y\" || c == 'it\\'s' || d == \"say \\\"hi\\\"\"");
		var items = ((LogicalExpr)((WhereClause)query.Clauses[0]).Condition).Items;
		var texts = items.Select(i => ((LiteralExpr)((BinaryExpr)i).Right).Text);

		Assert.Equal(["'x'", "\"y\"", "'it\\'s'", "\"say \\\"hi\\\"\""], texts);
	}

	[Fact]
	public void TwoStringsInARowSayHowToWriteAQuote()
	{
		var error = Refused("from t where a == 'it''s'");

		Assert.Equal(QlErrorCode.Syntax, error.Code);
		Assert.Equal(22, error.Position);
		Assert.Contains("\\'", error.Message);
	}

	[Fact]
	public void AStringEndsOnItsLine()
	{
		Assert.Equal(QlErrorCode.Syntax, Refused("from t where a == 'x\ny'").Code);
	}

	[Fact]
	public void CommentsAreSpace()
	{
		var query = Read("from t -- the entity\n where /* a test */ a > 1 -- done");

		Assert.IsType<WhereClause>(Assert.Single(query.Clauses));
	}

	[Fact]
	public void MembersAndCallsRead()
	{
		var query = Read("from t where name.Trim().ToUpper().StartsWith(@p) && name.Length > 3 select Count(*) as c, Sum(total), created.Year");
		var where = (LogicalExpr)((WhereClause)query.Clauses[0]).Condition;
		var call  = (MemberExpr)where.Items[0];

		Assert.Equal("StartsWith", call.Name.Text);
		Assert.Equal("ToUpper", ((MemberExpr)call.Target).Name.Text);
		Assert.Empty(((MemberExpr)call.Target).Arguments!);

		var items = ((SelectClause)query.Clauses[1]).Items!;

		Assert.True(((CallExpr)items[0].Value).Star);
		Assert.Equal("c", items[0].Alias!.Text);
		Assert.Equal("Sum", ((CallExpr)items[1].Value).Name.Text);
		Assert.Null(((MemberExpr)items[2].Value).Arguments);
	}

	[Fact]
	public void ANameBeforeAStringTypesIt()
	{
		var query = Read("from t where created > timestamp '2026-01-01 10:00:00'");
		var typed = (TypedLiteralExpr)((BinaryExpr)((WhereClause)query.Clauses[0]).Condition).Right;

		Assert.Equal("timestamp", typed.Type.Text);
		Assert.Equal("'2026-01-01 10:00:00'", typed.Value.Text);
	}

	[Fact]
	public void PredicatesRead()
	{
		var query = Read("from t where a is null && b is not null && c in (1, 2) && d not in @ds && e between 1 and 2 && f not between 3 and 4 && !(g == 1)");
		var items = ((LogicalExpr)((WhereClause)query.Clauses[0]).Condition).Items;

		Assert.False(((IsNullExpr)items[0]).Negated);
		Assert.True(((IsNullExpr)items[1]).Negated);
		Assert.Equal(2, ((InExpr)items[2]).Items!.Length);
		Assert.Equal("ds", ((InExpr)items[3]).List!.Name);
		Assert.True(((InExpr)items[3]).Negated);
		Assert.False(((BetweenExpr)items[4]).Negated);
		Assert.True(((BetweenExpr)items[5]).Negated);
		Assert.Equal(UnaryOperator.Not, ((UnaryExpr)items[6]).Operator);
	}

	[Fact]
	public void NotIsLooserThanAComparison()
	{
		var not = (UnaryExpr)((WhereClause)Read("from t where !a == b").Clauses[0]).Condition;

		Assert.IsType<BinaryExpr>(not.Operand);
	}

	[Fact]
	public void ArithmeticGroupsAsWritten()
	{
		var minus = (BinaryExpr)((LetClause)Read("from t let x = a - b - c * d").Clauses[0]).Value;

		Assert.Equal(BinaryOperator.Subtract, minus.Operator);
		Assert.Equal(BinaryOperator.Subtract, ((BinaryExpr)minus.Left).Operator);
		Assert.Equal(BinaryOperator.Multiply, ((BinaryExpr)minus.Right).Operator);
		Assert.Equal(4, minus.Operands);
	}

	[Fact]
	public void OrderItemsTakeDirectionAndNulls()
	{
		var items = ((OrderClause)Read("from t order by a, b desc, c asc nulls last, d nulls first").Clauses[0]).Items;

		Assert.Equal([false, true, false, false], items.Select(i => i.Descending));
		Assert.Equal([null, null, (bool?)false, true], items.Select(i => i.NullsFirst));
	}

	[Theory]
	[InlineData("select a from t",                    0)]
	[InlineData("from",                               4)]
	[InlineData("from t where",                       12)]
	[InlineData("from t where a >",                   16)]
	[InlineData("from t wher a > 1",                  12)]
	[InlineData("from t where a = 1",                 15)]
	[InlineData("from t where a & b",                 15)]
	[InlineData("from t skip -1",                     12)]
	[InlineData("from t take x",                      12)]
	[InlineData("from t select a b",                  16)]
	[InlineData("from t select",                      13)]
	[InlineData("from t order a",                     13)]
	[InlineData("from t where (a > 1",                19)]
	[InlineData("from t where a in ()",               19)]
	[InlineData("from t where a.b(",                  17)]
	public void ASyntaxErrorSaysWhere(string text, int position)
	{
		var error = Refused(text);

		Assert.Equal(QlErrorCode.Syntax, error.Code);
		Assert.Equal(position, error.Position);
		Assert.False(string.IsNullOrEmpty(error.Message));
	}

	[Theory]
	[InlineData("from",                "Expected a name.")]
	[InlineData("from t where",        "Expected a value: a name, a number, a string, a parameter or a bracket.")]
	[InlineData("from t where a >",    "Expected a value: a name, a number, a string, a parameter or a bracket.")]
	[InlineData("from t where a = 1",  "Expected an operator, or the next clause: `where`, `let`, `order by`, `select`, `skip` or `take`.")]
	[InlineData("from t skip -1",      "Expected a count: a number or a parameter.")]
	[InlineData("from t where a == 'x\ny'", "The string is not closed on its line.")]
	public void ASyntaxErrorSaysWhatWasExpectedInTheLanguagesWords(string text, string message)
	{
		Assert.Equal(message, Refused(text).Message);
	}

	[Fact]
	public void SqlOrderIsTold()
	{
		Assert.Contains("starts with `from`", Refused("select a from t").Message);
	}

	[Fact]
	public void ATextTooLongIsRefusedBeforeItIsRead()
	{
		var error = Refused("from t where " + new string(' ', 100) + "(", QlOptions.Default with { MaxTextLength = 50 });

		Assert.Equal(QlErrorCode.Limit, error.Code);
	}

	[Fact]
	public void NestingAsWrittenIsBounded()
	{
		var options = QlOptions.Default with { MaxSyntaxDepth = 5, MaxTreeDepth = 100 };

		Read("from t where ((((a))))", options);

		var error = Refused("from t where (((((a)))))", options);

		Assert.Equal(QlErrorCode.Limit, error.Code);
		Assert.Equal(18, error.Position);

		Assert.Equal(QlErrorCode.Limit, Refused("from t where !!!!!a", options).Code);
		Assert.Equal(QlErrorCode.Limit, Refused("from t where - - - - -a > 1", options).Code);
	}

	[Fact]
	public void TheDepthOfAnExpressionIsBoundedWithoutItsParentheses()
	{
		var options = QlOptions.Default with { MaxTreeDepth = 3 };

		Read("from t where ((((((a > 1))))))", options);

		var error = Refused("from t where -(-(-a)) > 1", options);

		Assert.Equal(QlErrorCode.Limit, error.Code);
		Assert.Equal(17, error.Position);
	}

	[Fact]
	public void NodesAreBounded()
	{
		var options = QlOptions.Default with { MaxNodes = 11 };

		Read("from t where a > 1 && b > 2 select a", options);

		var error = Refused("from t where a > 1 && b > 2 select a, b, c", options);

		Assert.Equal(QlErrorCode.Limit, error.Code);
		Assert.Equal(28, error.Position);
	}

	[Fact]
	public void ArithmeticOperandsAreCountedThroughParentheses()
	{
		var options = QlOptions.Default with { MaxArithmeticOperands = 4 };

		Read("from t let x = (a + b) * (c - d)", options);

		var error = Refused("from t let x = (a + b) * (c - -(d + e))", options);

		Assert.Equal(QlErrorCode.Limit, error.Code);
		Assert.Equal(23, error.Position);
	}

	[Fact]
	public void InListsAndColumnsAreBounded()
	{
		var options = QlOptions.Default with { MaxInList = 3, MaxColumns = 2 };

		Read("from t where a in (1, 2, 3) select a, b", options);

		Assert.Equal((QlErrorCode.Limit, (int?)15), Code(Refused("from t where a in (1, 2, 3, 4)", options)));
		Assert.Equal((QlErrorCode.Limit, (int?)7),  Code(Refused("from t select a, b, c", options)));

		static (QlErrorCode, int?) Code(QlError error)
		{
			return (error.Code, error.Position);
		}
	}

	[Fact]
	public void BracketsCostTheirDepthOnce()
	{
		// Read over tokens, a rule that failed at a token is not read there again: a nest of brackets is read
		// once, however many alternatives could have begun inside it. The text bound is lifted so that the
		// whole nest is read before the depth bound refuses it.
		var options = QlOptions.Default with { MaxTextLength = 1 << 20 };

		var small = Time(2000);
		var large = Time(4000);

		Assert.True(large < small * 6 + 50, $"2000 deep: {small} ms, 4000 deep: {large} ms");

		long Time(int depth)
		{
			var text = "from t where " + new string('(', depth) + "a" + new string(')', depth) + " > 1";
			var best = long.MaxValue;

			for (var i = 0; i < 3; i++)
			{
				var watch = Stopwatch.StartNew();

				Assert.Equal(QlErrorCode.Limit, Refused(text, options).Code);

				best = Math.Min(best, watch.ElapsedMilliseconds);
			}

			return best;
		}
	}
}
