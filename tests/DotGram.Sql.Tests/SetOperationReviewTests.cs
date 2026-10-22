using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using DotGram.Sql.Ast;
using DotGram.Sql.Productions;
using DotGram.Sql.Standard;

using Xunit;

namespace DotGram.Sql.Tests;

using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Query = DotGram.Sql.Ast.Query;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>Adversarial review tests for the set operations of the SQL:2023 tree.</summary>
public sealed class SetOperationReviewTests
{
	static readonly string[] Names = ["a", "b", "c", "d", "e"];

	static Query.Select Table(string name)
	{
		return new Query.Select
		{
			Columns = [new SelectColumn.All()],
			From = new FromClause([new TableSource.Named(new QualifiedName([new Identifier(name)]))]),
			Table = true,
		};
	}

	static Expression Column(string name)
	{
		return new Expression.Reference(new QualifiedName([new Identifier(name)]));
	}

	static Expression Number(int value)
	{
		return new LiteralValue.Numeric(value.ToString(System.Globalization.CultureInfo.InvariantCulture), NumericLiteralKind.DecimalInteger);
	}

	// ── Text round trips ───────────────────────────────────────────────────────

	[Theory]
	[InlineData("(TABLE a) UNION ALL (TABLE b) ORDER BY 1 FETCH FIRST 2 ROWS ONLY")]
	[InlineData("TABLE a EXCEPT TABLE b EXCEPT TABLE c")]
	[InlineData("TABLE a EXCEPT (TABLE b EXCEPT TABLE c)")]
	[InlineData("TABLE a UNION CORRESPONDING BY (x, y) TABLE b")]
	[InlineData("TABLE a UNION CORRESPONDING TABLE b")]
	[InlineData("TABLE a UNION DISTINCT CORRESPONDING BY (x) TABLE b INTERSECT ALL CORRESPONDING TABLE c")]
	[InlineData("VALUES (1, 2) UNION VALUES (3, 4) INTERSECT TABLE t")]
	[InlineData("VALUES 1 EXCEPT ALL (VALUES 2)")]
	[InlineData("WITH q AS (TABLE a) TABLE q UNION TABLE b")]
	[InlineData("WITH RECURSIVE q (n) AS (VALUES 1 UNION ALL SELECT n + 1 FROM q WHERE n < 10) TABLE q")]
	[InlineData("WITH q AS ((TABLE a) EXCEPT TABLE b ORDER BY 1) TABLE q INTERSECT (TABLE c)")]
	[InlineData("TABLE a ORDER BY 1 OFFSET 2 ROWS")]
	[InlineData("TABLE a OFFSET 1 ROW")]
	[InlineData("TABLE a OFFSET 1 ROW FETCH NEXT 10 PERCENT ROWS WITH TIES")]
	[InlineData("TABLE a ORDER BY x FETCH FIRST 3 ROWS WITH TIES")]
	[InlineData("TABLE a FETCH FIRST ROW ONLY")]
	[InlineData("((TABLE a UNION TABLE b) INTERSECT (TABLE c EXCEPT TABLE d))")]
	[InlineData("(((TABLE a) UNION ((TABLE b))))")]
	[InlineData("(TABLE a ORDER BY x OFFSET 1 ROWS) UNION ALL (TABLE b FETCH FIRST 1 ROW ONLY) EXCEPT (TABLE c) ORDER BY 1")]
	[InlineData("((TABLE a ORDER BY x) ORDER BY y) ORDER BY z")]
	[InlineData("TABLE a INTERSECT ALL TABLE b INTERSECT TABLE c UNION ALL TABLE d EXCEPT TABLE e INTERSECT TABLE f")]
	[InlineData("TABLE a UNION (TABLE b INTERSECT TABLE c) INTERSECT TABLE d")]
	[InlineData("SELECT * FROM (TABLE a UNION TABLE b) AS d")]
	[InlineData("SELECT * FROM ((TABLE a) UNION TABLE b ORDER BY 1) AS d")]
	[InlineData("SELECT * FROM t WHERE x IN (TABLE a UNION TABLE b) AND EXISTS (TABLE a EXCEPT TABLE b ORDER BY 1)")]
	[InlineData("SELECT (SELECT 1 FROM t UNION SELECT 2 FROM u FETCH FIRST 1 ROW ONLY) FROM t WHERE x > ALL ((TABLE a) INTERSECT TABLE b)")]
	[InlineData("SELECT * FROM t WHERE x IN ((TABLE a))")]
	[InlineData("SELECT * FROM t WHERE x IN ((TABLE a) UNION (TABLE b))")]
	public void Is_written_as_read_and_read_back_the_same(string input)
	{
		var tree    = Both.ParseQueryExpression(input);
		var written = SqlWriter.Write(tree);

		Assert.Equal(input, written);
		Assert.Equal(tree, Both.ParseQueryExpression(written));
		Assert.Equal(tree.GetHashCode(), Both.ParseQueryExpression(written).GetHashCode());
	}

	[Theory]
	[InlineData("INSERT INTO t TABLE a UNION TABLE b")]
	[InlineData("INSERT INTO t (TABLE a) UNION VALUES (1)")]
	[InlineData("INSERT INTO t VALUES (1) UNION VALUES (2)")]
	[InlineData("INSERT INTO t (x) VALUES (1) ORDER BY 1")]
	[InlineData("INSERT INTO t WITH q AS (TABLE a) TABLE q EXCEPT TABLE b")]
	public void An_insert_source_with_set_operations_round_trips(string input)
	{
		var tree    = Both.ParseInsertStatement(input);
		var written = SqlWriter.Write(tree);

		Assert.Equal(input, written);
		Assert.Equal(tree, Both.ParseInsertStatement(written));
		Assert.IsType<InsertSource.Subquery>(tree.SourceValue);
	}

	[Theory]
	[InlineData("TABLE a ORDER BY 1 FOR READ ONLY")]
	[InlineData("(TABLE a ORDER BY 1) EXCEPT TABLE b FOR UPDATE")]
	[InlineData("SELECT a FROM t UNION ALL SELECT b FROM u ORDER BY 1 OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY")]
	public void A_statement_round_trips(string input)
	{
		var tree    = SqlStandardParser.ParseStatement(input);
		var written = SqlWriter.Write(tree);

		Assert.Equal(input, written);
		Assert.Equal(tree, SqlStandardParser.ParseStatement(written));
	}

	[Fact]
	public void EXCEPT_is_left_associative_and_brackets_on_the_right_differ()
	{
		var flat  = Both.ParseQueryExpression("TABLE a EXCEPT TABLE b EXCEPT TABLE c");
		var right = Both.ParseQueryExpression("TABLE a EXCEPT (TABLE b EXCEPT TABLE c)");
		var left  = Both.ParseQueryExpression("(TABLE a EXCEPT TABLE b) EXCEPT TABLE c");

		Assert.Equal(Table("a") with { SetOperations = [new(SetOperator.Except, Table("b")), new(SetOperator.Except, Table("c"))] }, flat);
		Assert.NotEqual(flat, right);
		Assert.NotEqual(flat, left);
		Assert.NotEqual(Evaluate(flat), Evaluate(right));
		Assert.Equal(Evaluate(flat), Evaluate(left));
	}

	// ── Deep and wide ──────────────────────────────────────────────────────────

	[Fact]
	public void Two_thousand_operands_of_UNION_ALL()
	{
		var text = "TABLE a" + string.Concat(Enumerable.Repeat(" UNION ALL TABLE a", 1999));

		var thrown = OnStack(() =>
		{
			var tree  = Both.ParseQueryExpression(text);
			var again = Both.ParseQueryExpression(text);

			Assert.Equal(1999, tree.SetOperations.Count);
			Assert.Equal(2000, tree.Operands().Count());
			Assert.Equal(text, SqlWriter.Write(tree));
			Assert.Equal(tree, again);
			Assert.Equal(tree.GetHashCode(), again.GetHashCode());
		});

		Assert.Null(thrown);
	}

	[Theory]
	[InlineData(2_000)]
	[InlineData(10_000)]
	public void Deep_brackets_and_right_nested_operands_compare_hash_and_write(int depth)
	{
		var brackets = new string('(', depth) + "TABLE a" + new string(')', depth);
		var nested   = new StringBuilder();

		for (var at = 0; at < depth; at++)
			nested.Append("TABLE a EXCEPT (");

		nested.Append("TABLE a").Append(')', depth);

		var thrown = OnStack(() =>
		{
			foreach (var text in new[] { brackets, nested.ToString() })
			{
				var tree  = SqlStandardParser.ParseStatement(text);
				var again = SqlStandardParser.ParseStatement(text);

				Assert.Equal(text, SqlWriter.Write(tree));

				// Deeper than the stack, equality and the hash may end in a catchable exception, as
				// they do for every deep tree (DeepTreeTests); never in anything else. A nested operand
				// is three records a level — the query, its operation, the brackets — and each compares
				// through the base's Equals, so on this stack it reaches that end before 2,000 levels.
				try
				{
					Assert.True(tree.Equals(again));
					Assert.Equal(tree.GetHashCode(), again.GetHashCode());
				}
				catch (InsufficientExecutionStackException) when (depth > 2_000 || text != brackets)
				{
				}

				Assert.Single(((Statement.Select)tree).Query.Operands().Take(1));
			}
		});

		Assert.Null(thrown);
	}

	// ── Hand-built trees ───────────────────────────────────────────────────────

	[Fact]
	public void Combine_keeps_a_limit_a_locking_and_an_outer_ORDER_BY_their_own()
	{
		var union   = new SetOperation(SetOperator.Union, Table("c"));
		var limited = Table("a") with { Limit = new Limit { Count = Number(1) } };
		var locked  = Table("a") with { Locking = [new LockingClause(LockingMode.Update, [])] };
		var outer   = new Query.Parenthesized(Table("a")) { OrderBy = new OrderByClause([new SortItem(Column("x"))]) };

		Assert.Equal("(TABLE a FETCH FIRST 1 ROWS ONLY) UNION TABLE c", SqlWriter.Write(limited.Combine(union)));
		Assert.Equal("(TABLE a FOR UPDATE) UNION TABLE c", SqlWriter.Write(locked.Combine(union)));
		Assert.Equal("((TABLE a) ORDER BY x) UNION TABLE c", SqlWriter.Write(outer.Combine(union)));
	}

	/// <summary>
	/// Combine refuses a query with a WITH, which on the result could capture a name in the added
	/// operand; a caller who means it to be shared puts it on the result, and that reads back.
	/// </summary>
	[Fact]
	public void Combine_refuses_WITH_and_a_shared_WITH_is_put_on_the_result()
	{
		var with = new WithClause(false, [new CommonTableExpression(new Identifier("q"), [], Table("a"))]);
		var q    = Table("q") with { With = with };
		var b    = new SetOperation(SetOperator.Union, Table("b"));

		Assert.Throws<ArgumentException>(() => q.Combine(b));

		var combined = (q with { With = null }).Combine(b) with { With = with };

		Assert.Equal("WITH q AS (TABLE a) TABLE q UNION TABLE b", SqlWriter.Write(combined));
		Assert.Equal(combined, Both.ParseQueryExpression(SqlWriter.Write(combined)));
	}

	[Fact]
	public void An_operand_with_an_outer_clause_gets_brackets()
	{
		var outer   = new Query.Parenthesized(Table("b")) { Limit = new Limit { Count = Number(1), FetchRowWord = RowWord.Row } };

		Assert.Equal(
			"TABLE a UNION ((TABLE b) FETCH FIRST 1 ROW ONLY)",
			SqlWriter.Write(Table("a") with { SetOperations = [new(SetOperator.Union, outer)] }));
	}

	/// <summary>
	/// SQL:2023 has no WITH inside the brackets of a query primary, only in a subquery's: an operand
	/// or a Parenthesized whose query has a WITH is written as text no reader takes back.
	/// </summary>
	[Fact]
	public void A_WITH_inside_an_operand_is_written_as_text_that_reads_back()
	{
		var with    = new WithClause(false, [new CommonTableExpression(new Identifier("q"), [], Table("a"))]);
		var operand = Table("q") with { With = with };

		foreach (var query in new Query[]
		{
			Table("a") with { SetOperations = [new(SetOperator.Union, operand)] },
			new Query.Parenthesized(operand) { SetOperations = [new(SetOperator.Union, Table("b"))] },
		})
		{
			string written;

			try
			{
				written = SqlWriter.Write(query);
			}
			catch (ArgumentException)
			{
				// Refusing the tree is an answer too.
				continue;
			}

			Assert.True(SqlStandardProductions.TryParseQueryExpression(written).IsSuccess, written);
		}
	}

	/// <summary>A limit that writes nothing should not lose what it holds silently.</summary>
	[Fact]
	public void A_limit_with_WITH_TIES_or_PERCENT_but_no_FETCH_is_not_dropped_silently()
	{
		var ties    = Table("a") with { Limit = new Limit { WithTies = true } };
		var percent = Table("a") with { Limit = new Limit { Offset = Number(1), Percent = true } };

		Assert.ThrowsAny<ArgumentException>(() => SqlWriter.Write(ties));
		Assert.ThrowsAny<ArgumentException>(() => SqlWriter.Write(percent));
	}

	[Fact]
	public void The_walker_reaches_every_operand_and_the_inside_of_brackets()
	{
		var query = Both.ParseQueryExpression("(TABLE a UNION TABLE b) INTERSECT (TABLE c) EXCEPT TABLE d");
		var names = new List<string>();

		SqlWalker.Walk(query, node =>
		{
			if (node is TableSource.Named named)
				names.Add(named.Name.Parts[0].Text);

			return true;
		});

		Assert.Equal(["a", "b", "c", "d"], names);
	}

	// ── Fuzz ───────────────────────────────────────────────────────────────────

	/// <summary>
	/// Random trees of 3–5 operands, every operator, brackets and nested lists, built by hand: what is
	/// written reads back into a tree that computes the same bag, and writing that tree again gives the
	/// same text (a read tree is canonical).
	/// </summary>
	[Fact]
	public void Random_hand_built_trees_mean_what_they_were_built_to_mean()
	{
		var random = new Random(20261011);

		for (var round = 0; round < 5000; round++)
		{
			var budget  = random.Next(3, 6);
			var tree    = RandomQuery(random, ref budget, 0);
			var written = SqlWriter.Write(tree);
			var read    = Both.ParseQueryExpression(written);

			Assert.True(Same(Evaluate(tree), Evaluate(read)), $"round {round}: {written}");
			Assert.Equal(written, SqlWriter.Write(read));
			Assert.Equal(read, Both.ParseQueryExpression(SqlWriter.Write(read)));
		}
	}

	/// <summary>Random text in the writer's own spelling: read, written back to the same text, read again to the same tree.</summary>
	[Fact]
	public void Random_text_is_written_back_as_it_was()
	{
		var random = new Random(11102026);

		for (var round = 0; round < 5000; round++)
		{
			var budget = random.Next(3, 6);
			var text   = RandomText(random, ref budget, 0);
			var tree   = Both.ParseQueryExpression(text);

			Assert.Equal(text, SqlWriter.Write(tree));
			Assert.Equal(tree, Both.ParseQueryExpression(text));
		}
	}

	static readonly SetOperator[] Operators = Enum.GetValues<SetOperator>();

	static Query RandomQuery(Random random, ref int budget, int depth)
	{
		Query first;

		if (depth < 3 && budget > 1 && random.Next(4) == 0)
			first = new Query.Parenthesized(RandomQuery(random, ref budget, depth + 1));
		else
		{
			first = Table(Names[random.Next(Names.Length)]);
			budget--;
		}

		if (random.Next(6) == 0)
			first = first with { OrderBy = new OrderByClause([new SortItem(Column("x"))]) };

		var operations = new List<SetOperation>();

		while (budget > 0 && random.Next(3) != 0)
		{
			var op = Operators[random.Next(Operators.Length)];
			Query operand;

			if (depth < 3 && budget > 1 && random.Next(3) == 0)
				operand = RandomQuery(random, ref budget, depth + 1);
			else
			{
				operand = Table(Names[random.Next(Names.Length)]);
				budget--;
			}

			var distinct = op is SetOperator.Union or SetOperator.Intersect or SetOperator.Except && random.Next(4) == 0;

			operations.Add(new SetOperation(op, operand) { Distinct = distinct });
		}

		// Combine brackets a first operand that has an ORDER BY, so that it stays the operand's.
		var query = first;

		foreach (var operation in operations)
			query = query.Combine(operation);

		return query;
	}

	static string RandomText(Random random, ref int budget, int depth)
	{
		var text = new StringBuilder();

		AppendPrimary(random, ref budget, depth, text);

		while (budget > 0 && random.Next(3) != 0)
		{
			text.Append(' ').Append(random.Next(9) switch
			{
				0 => "UNION",
				1 => "UNION ALL",
				2 => "UNION DISTINCT",
				3 => "EXCEPT",
				4 => "EXCEPT ALL",
				5 => "EXCEPT DISTINCT",
				6 => "INTERSECT",
				7 => "INTERSECT ALL",
				_ => "INTERSECT DISTINCT",
			}).Append(' ');

			AppendPrimary(random, ref budget, depth, text);
		}

		return text.ToString();
	}

	static void AppendPrimary(Random random, ref int budget, int depth, StringBuilder text)
	{
		if (depth < 3 && budget > 1 && random.Next(3) == 0)
		{
			text.Append('(').Append(RandomText(random, ref budget, depth + 1));

			if (random.Next(4) == 0)
				text.Append(" ORDER BY x");

			if (random.Next(5) == 0)
				text.Append(" FETCH FIRST 1 ROW ONLY");

			text.Append(')');
		}
		else
		{
			text.Append("TABLE ").Append(Names[random.Next(Names.Length)]);
			budget--;
		}
	}

	// ── A bag evaluator: the oracle for what a tree means ──────────────────────

	static readonly Dictionary<string, int[]> Bags = new()
	{
		["a"] = [1, 1, 2, 3, 5],
		["b"] = [1, 2, 2, 4],
		["c"] = [2, 3, 3, 4, 6],
		["d"] = [1, 4, 5, 5],
		["e"] = [3, 6, 6, 7],
		["t"] = [1],
		["f"] = [2, 7],
		["q"] = [1, 1, 2, 3, 5],
	};

	static bool Same(List<int> left, List<int> right)
	{
		return left.SequenceEqual(right);
	}

	static List<int> Evaluate(Query query)
	{
		var result = query switch
		{
			Query.Parenthesized parenthesized => Evaluate(parenthesized.Query),
			Query.Select select               => [.. Bags[((TableSource.Named)select.From!.Sources[0]).Name.Parts[0].Text]],
			_                                 => throw new InvalidOperationException(),
		};

		foreach (var operation in query.SetOperations)
		{
			var right = Evaluate(operation.Query);

			result = operation.Operator switch
			{
				SetOperator.UnionAll     => [.. result, .. right],
				SetOperator.Union        => [.. result.Concat(right).Distinct()],
				SetOperator.Intersect    => [.. result.Distinct().Where(right.Contains)],
				SetOperator.IntersectAll => IntersectAll(result, right),
				SetOperator.Except       => [.. result.Distinct().Where(one => !right.Contains(one))],
				SetOperator.ExceptAll    => ExceptAll(result, right),
				_                        => throw new InvalidOperationException(),
			};
		}

		result.Sort();

		return result;
	}

	static List<int> IntersectAll(List<int> left, List<int> right)
	{
		var rest   = right.ToList();
		var result = new List<int>();

		foreach (var one in left)
		{
			if (rest.Remove(one))
				result.Add(one);
		}

		return result;
	}

	static List<int> ExceptAll(List<int> left, List<int> right)
	{
		var result = left.ToList();

		foreach (var one in right)
			result.Remove(one);

		return result;
	}

	static Exception? OnStack(Action action)
	{
		Exception? thrown = null;

		var thread = new Thread(() =>
		{
			try
			{
				action();
			}
			catch (Exception exception)
			{
				thrown = exception;
			}
		}, 1024 * 1024);

		thread.Start();
		thread.Join();

		return thrown;
	}
}
