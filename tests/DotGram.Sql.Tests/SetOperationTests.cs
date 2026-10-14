using System;
using System.Linq;

using DotGram.Sql.Ast;
using DotGram.Sql.Standard;

using Xunit;

namespace DotGram.Sql.Tests;

// Inside this namespace an unqualified name is the old tree's, since `DotGram.Sql` encloses
// `DotGram.Sql.Tests`; the new tree's names are bound here, as Both.cs binds them.
using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Query = DotGram.Sql.Ast.Query;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// UNION, EXCEPT and INTERSECT in the SQL:2023 tree: a list on the query they follow, read as a left
/// fold, with what binds tighter an operand of its own; whose clauses are whose; brackets as a node;
/// and the writer, which gives each tree its text back and brackets a hand-built one where the text
/// would otherwise mean something else.
/// </summary>
/// <remarks>
/// Every reading goes through <see cref="Both"/>, so the handwritten parser is held to the same tree.
/// The operands are <c>TABLE a</c>, <c>TABLE b</c> and so on, the shortest query the standard has.
/// </remarks>
public sealed class SetOperationTests
{
	static Query.Select Table(string name)
	{
		return new Query.Select
		{
			Columns = [new SelectColumn.All()],
			From = new FromClause([new TableSource.Named(Name(name))]),
			Table = true,
		};
	}

	static QualifiedName Name(string name)
	{
		return new QualifiedName([new Identifier(name)]);
	}

	static Expression Column(string name)
	{
		return new Expression.Reference(Name(name));
	}

	static SetOperation Op(SetOperator op, Query operand)
	{
		return new SetOperation(op, operand);
	}

	static readonly Query.Select A = Table("a");
	static readonly Query.Select B = Table("b");
	static readonly Query.Select C = Table("c");
	static readonly Query.Select D = Table("d");

	// ── Precedence ─────────────────────────────────────────────────────────────

	[Fact]
	public void An_INTERSECT_after_a_UNION_is_an_operand_of_its_own()
	{
		Assert.Equal(
			A with { SetOperations = [Op(SetOperator.Union, B with { SetOperations = [Op(SetOperator.Intersect, C)] })] },
			Both.ParseQueryExpression("TABLE a UNION TABLE b INTERSECT TABLE c"));
	}

	[Fact]
	public void A_UNION_after_an_INTERSECT_continues_the_list()
	{
		Assert.Equal(
			A with { SetOperations = [Op(SetOperator.Intersect, B), Op(SetOperator.Union, C)] },
			Both.ParseQueryExpression("TABLE a INTERSECT TABLE b UNION TABLE c"));
	}

	[Fact]
	public void A_list_continues_past_a_group_that_binds_tighter()
	{
		Assert.Equal(
			A with
			{
				SetOperations =
				[
					Op(SetOperator.Union, B with { SetOperations = [Op(SetOperator.Intersect, C)] }),
					Op(SetOperator.Union, D),
				],
			},
			Both.ParseQueryExpression("TABLE a UNION TABLE b INTERSECT TABLE c UNION TABLE d"));
	}

	[Fact]
	public void Brackets_hold_their_compound_and_the_operations_after_them_are_outside()
	{
		Assert.Equal(
			new Query.Parenthesized(A with { SetOperations = [Op(SetOperator.Union, B)] }) { SetOperations = [Op(SetOperator.Intersect, C)] },
			Both.ParseQueryExpression("(TABLE a UNION TABLE b) INTERSECT TABLE c"));
	}

	/// <summary>
	/// The operations after a bracketed operand are never added to the list inside its brackets, even
	/// where the operator inside binds tighter and the text would mean the same.
	/// </summary>
	[Fact]
	public void A_bracketed_first_operand_is_not_added_to()
	{
		Assert.Equal(
			new Query.Parenthesized(A with { SetOperations = [Op(SetOperator.Intersect, B)] }) { SetOperations = [Op(SetOperator.Union, C)] },
			Both.ParseQueryExpression("(TABLE a INTERSECT TABLE b) UNION TABLE c"));
	}

	[Fact]
	public void Brackets_on_the_right_are_an_operand_in_brackets()
	{
		Assert.Equal(
			A with { SetOperations = [Op(SetOperator.Except, new Query.Parenthesized(B with { SetOperations = [Op(SetOperator.Except, C)] }))] },
			Both.ParseQueryExpression("TABLE a EXCEPT (TABLE b EXCEPT TABLE c)"));
	}

	[Fact]
	public void A_bracketed_first_query_is_told_from_a_bracketed_compound()
	{
		var first    = Both.ParseQueryExpression("(TABLE a) UNION TABLE b");
		var compound = Both.ParseQueryExpression("(TABLE a UNION TABLE b)");

		Assert.Equal(new Query.Parenthesized(A) { SetOperations = [Op(SetOperator.Union, B)] }, first);
		Assert.Equal(new Query.Parenthesized(A with { SetOperations = [Op(SetOperator.Union, B)] }), compound);
		Assert.NotEqual(first, compound);
	}

	// ── The six kinds ──────────────────────────────────────────────────────────

	[Theory]
	[InlineData("TABLE a UNION TABLE b", SetOperator.Union, false)]
	[InlineData("TABLE a UNION ALL TABLE b", SetOperator.UnionAll, false)]
	[InlineData("TABLE a UNION DISTINCT TABLE b", SetOperator.Union, true)]
	[InlineData("TABLE a INTERSECT TABLE b", SetOperator.Intersect, false)]
	[InlineData("TABLE a INTERSECT ALL TABLE b", SetOperator.IntersectAll, false)]
	[InlineData("TABLE a INTERSECT DISTINCT TABLE b", SetOperator.Intersect, true)]
	[InlineData("TABLE a EXCEPT TABLE b", SetOperator.Except, false)]
	[InlineData("TABLE a EXCEPT ALL TABLE b", SetOperator.ExceptAll, false)]
	[InlineData("TABLE a EXCEPT DISTINCT TABLE b", SetOperator.Except, true)]
	public void An_operator_is_one_of_six_kinds_and_a_written_DISTINCT_is_a_spelling(string input, SetOperator op, bool distinct)
	{
		Assert.Equal(A with { SetOperations = [Op(op, B) with { Distinct = distinct }] }, Both.ParseQueryExpression(input));
	}

	// ── Whose clauses ──────────────────────────────────────────────────────────

	[Fact]
	public void The_clauses_of_a_query_with_set_operations_are_the_compounds()
	{
		var query = Assert.IsType<Query.Select>(Both.ParseQueryExpression("SELECT a FROM t WHERE a > 1 UNION SELECT b FROM u ORDER BY 1 FETCH FIRST 2 ROWS ONLY"));

		// The specification is the first operand's, the ORDER BY and the limit the union's.
		Assert.Equal(Name("t"), Assert.IsType<TableSource.Named>(Assert.Single(query.From!.Sources)).Name);
		Assert.NotNull(query.Where);
		Assert.Single(query.SetOperations);
		Assert.NotNull(query.OrderBy);
		Assert.NotNull(query.Limit);
		Assert.Null(query.SetOperations[0].Query.OrderBy);
	}

	[Fact]
	public void An_operands_own_clauses_are_inside_its_brackets()
	{
		var query = Assert.IsType<Query.Parenthesized>(Both.ParseQueryExpression(
			"(SELECT a FROM t ORDER BY a FETCH FIRST 1 ROW ONLY) UNION SELECT b FROM u ORDER BY 1"));

		var inner = Assert.IsType<Query.Select>(query.Query);

		Assert.NotNull(inner.OrderBy);
		Assert.Equal(RowWord.Row, inner.Limit!.FetchRowWord);
		Assert.NotNull(inner.Limit.Count);
		Assert.Single(query.SetOperations);
		Assert.NotNull(query.OrderBy);
		Assert.Null(query.Limit);
	}

	/// <summary>A subquery's brackets are its own; only a second pair is a node.</summary>
	[Fact]
	public void A_subquerys_brackets_are_not_a_parenthesized_query()
	{
		var once  = Assert.IsType<Expression.In>(Both.ParseSearchCondition("x IN (TABLE a)"));
		var twice = Assert.IsType<Expression.In>(Both.ParseSearchCondition("x IN ((TABLE a))"));

		Assert.Equal(A, Assert.IsType<InSource.Subquery>(once.SourceValue).Query);
		Assert.Equal(new Query.Parenthesized(A), Assert.IsType<InSource.Subquery>(twice.SourceValue).Query);
	}

	// ── Written back ───────────────────────────────────────────────────────────

	/// <summary>Every shape above, written as it was read, and read back into the same tree.</summary>
	[Theory]
	[InlineData("TABLE a UNION TABLE b INTERSECT TABLE c")]
	[InlineData("TABLE a INTERSECT TABLE b UNION TABLE c")]
	[InlineData("TABLE a UNION TABLE b INTERSECT TABLE c UNION TABLE d")]
	[InlineData("(TABLE a UNION TABLE b) INTERSECT TABLE c")]
	[InlineData("(TABLE a INTERSECT TABLE b) UNION TABLE c")]
	[InlineData("TABLE a EXCEPT (TABLE b EXCEPT TABLE c)")]
	[InlineData("TABLE a EXCEPT (TABLE b) EXCEPT TABLE c")]
	[InlineData("(TABLE a) UNION TABLE b")]
	[InlineData("(TABLE a UNION TABLE b)")]
	[InlineData("((TABLE a))")]
	[InlineData("TABLE a UNION DISTINCT TABLE b EXCEPT ALL TABLE c INTERSECT ALL TABLE d")]
	[InlineData("SELECT a FROM t UNION ALL VALUES (1), (2) ORDER BY 1 OFFSET 1 ROWS")]
	[InlineData("(SELECT a FROM t ORDER BY a FETCH FIRST 1 ROW ONLY) UNION SELECT b FROM u ORDER BY 1")]
	[InlineData("(SELECT a FROM t ORDER BY a) ORDER BY b")]
	[InlineData("WITH q AS (TABLE a UNION TABLE b) SELECT x FROM q EXCEPT CORRESPONDING BY (x) TABLE c FETCH NEXT ROW ONLY")]
	public void A_set_operation_is_written_as_it_was_read(string input)
	{
		var tree    = Both.ParseQueryExpression(input);
		var written = Sql2023Writer.Write(tree);

		Assert.Equal(input, written);
		Assert.Equal(tree, Both.ParseQueryExpression(written));
	}

	[Fact]
	public void A_cursors_updatability_is_written_after_the_whole_query()
	{
		var statement = SqlStandardParser.ParseStatement("(TABLE a) UNION TABLE b FOR UPDATE OF x");
		var select    = Assert.IsType<Statement.Select>(statement);

		Assert.Equal(new LockingClause(LockingMode.Update, [new Identifier("x")]), Assert.Single(select.Query.Locking));
		Assert.Equal("(TABLE a) UNION TABLE b FOR UPDATE OF x", Sql2023Writer.Write(statement));
	}

	// ── A tree built by hand ───────────────────────────────────────────────────

	/// <summary>
	/// A list is a left fold whatever the operators; where a later operator binds tighter than an
	/// earlier one, the part before it is bracketed, or the text would give it its left neighbour alone.
	/// </summary>
	[Fact]
	public void A_hand_built_list_is_bracketed_where_a_later_operator_binds_tighter()
	{
		Assert.Equal(
			"(TABLE a UNION TABLE b) INTERSECT TABLE c",
			Sql2023Writer.Write(A with { SetOperations = [Op(SetOperator.Union, B), Op(SetOperator.Intersect, C)] }));

		Assert.Equal(
			"((TABLE a UNION TABLE b) INTERSECT TABLE c UNION TABLE d) INTERSECT TABLE a",
			Sql2023Writer.Write(A with
			{
				SetOperations = [Op(SetOperator.Union, B), Op(SetOperator.Intersect, C), Op(SetOperator.Union, D), Op(SetOperator.Intersect, A)],
			}));
	}

	/// <summary>
	/// An operand with operations of its own keeps them without brackets only where all of them bind
	/// tighter than the operator before it — the loosest of them decides, not the first.
	/// </summary>
	[Theory]
	[InlineData(SetOperator.Union, SetOperator.Intersect, null, "TABLE a UNION TABLE b INTERSECT TABLE c")]
	[InlineData(SetOperator.Except, SetOperator.Union, null, "TABLE a EXCEPT (TABLE b UNION TABLE c)")]
	[InlineData(SetOperator.Except, SetOperator.Except, null, "TABLE a EXCEPT (TABLE b EXCEPT TABLE c)")]
	[InlineData(SetOperator.Intersect, SetOperator.Intersect, null, "TABLE a INTERSECT (TABLE b INTERSECT TABLE c)")]
	[InlineData(SetOperator.Except, SetOperator.Intersect, SetOperator.Union, "TABLE a EXCEPT (TABLE b INTERSECT TABLE c UNION TABLE d)")]
	public void A_hand_built_operand_is_bracketed_unless_it_binds_tighter(SetOperator outer, SetOperator first, SetOperator? second, string expected)
	{
		var operations = second is { } then
			? new[] { Op(first, C), Op(then, D) }
			: new[] { Op(first, C) };

		var operand = B with { SetOperations = [.. operations] };
		var written = Sql2023Writer.Write(A with { SetOperations = [Op(outer, operand)] });

		Assert.Equal(expected, written);

		// And what is written means what was built: read back, the operand is the same, in the brackets
		// the writer added where it added them.
		var right = Both.ParseQueryExpression(written).SetOperations[0].Query;

		Assert.Equal(operand, right is Query.Parenthesized bracketed ? bracketed.Query : right);
	}

	[Fact]
	public void A_hand_built_operand_with_clauses_of_its_own_is_bracketed()
	{
		var sorted = B with { OrderBy = new OrderByClause([new SortItem(Column("x"))]) };

		Assert.Equal("TABLE a UNION (TABLE b ORDER BY x)", Sql2023Writer.Write(A with { SetOperations = [Op(SetOperator.Union, sorted)] }));
	}

	[Fact]
	public void DISTINCT_beside_ALL_is_refused()
	{
		var query = A with { SetOperations = [Op(SetOperator.UnionAll, B) with { Distinct = true }] };

		Assert.Throws<ArgumentException>(() => Sql2023Writer.Write(query));
	}

	[Fact]
	public void TABLE_is_written_only_while_the_query_is_still_SELECT_star_FROM_the_table()
	{
		Assert.Equal("TABLE a", Sql2023Writer.Write(A));
		Assert.Equal("SELECT * FROM a WHERE x", Sql2023Writer.Write(A with { Where = Column("x") }));
		Assert.Equal("SELECT x FROM a", Sql2023Writer.Write(A with { Columns = [new SelectColumn.ExpressionItem(Column("x"))] }));
	}

	[Fact]
	public void Rows_beside_a_select_list_are_refused()
	{
		var rows = new Query.Select { Values = [new RowValue([Column("x")])] };

		Assert.Equal("VALUES x", Sql2023Writer.Write(rows));
		Assert.Throws<ArgumentException>(() => Sql2023Writer.Write(rows with { Columns = [new SelectColumn.All()] }));
		Assert.Throws<ArgumentException>(() => Sql2023Writer.Write(new Query.Select()));
	}

	// ── Helpers ────────────────────────────────────────────────────────────────

	[Fact]
	public void Operands_are_every_query_a_compound_combines_in_text_order()
	{
		var query = Both.ParseQueryExpression("(TABLE a UNION TABLE b) INTERSECT (TABLE c) EXCEPT SELECT x FROM d WHERE x IN (TABLE e)");

		Assert.Equal(
			["a", "b", "c", "d"],
			query.Operands().Select(one => ((TableSource.Named)one.From!.Sources[0]).Name.Parts[0].Text).ToArray());
	}

	[Fact]
	public void Combine_appends_to_the_list_and_brackets_a_query_whose_clauses_would_change_owner()
	{
		var union = Op(SetOperator.Union, C);

		Assert.Equal(A with { SetOperations = [Op(SetOperator.Intersect, B), union] }, (A with { SetOperations = [Op(SetOperator.Intersect, B)] }).Combine(union));

		var sorted = A with { OrderBy = new OrderByClause([new SortItem(Column("x"))]) };

		Assert.Equal(new Query.Parenthesized(sorted) { SetOperations = [union] }, sorted.Combine(union));
		Assert.Equal("(TABLE a ORDER BY x) UNION TABLE c", Sql2023Writer.Write(sorted.Combine(union)));
	}

	/// <summary>
	/// A WITH stands only on the outermost query of a query expression: inside a query's brackets or on a
	/// set operation's operand there is no text for it, and the writer says so rather than writing what
	/// cannot be read back.
	/// </summary>
	[Fact]
	public void A_WITH_inside_brackets_or_on_an_operand_is_refused()
	{
		var with = new WithClause(false, [new CommonTableExpression(new Identifier("q"), [], A)]);
		var q    = Table("q") with { With = with };

		Assert.Throws<ArgumentException>(() => Sql2023Writer.Write(A with { SetOperations = [Op(SetOperator.Union, q)] }));
		Assert.Throws<ArgumentException>(() => Sql2023Writer.Write(new Query.Parenthesized(q)));
		Assert.Equal("WITH q AS (TABLE a) (TABLE q)", Sql2023Writer.Write(new Query.Parenthesized(Table("q")) { With = with }));
	}

	/// <summary>
	/// Combine refuses a query with a WITH, bracketed or not: on the result, the CTE <c>q</c> would
	/// capture the table <c>q</c> the added operand reads. A WITH meant to be shared is put on the
	/// result by the caller.
	/// </summary>
	[Fact]
	public void Combine_refuses_a_WITH_that_would_capture_a_name_in_the_added_operand()
	{
		var with      = new WithClause(false, [new CommonTableExpression(new Identifier("q"), [], A)]);
		var plain     = Table("q") with { With = with };
		var sorted    = plain with { OrderBy = new OrderByClause([new SortItem(Column("x"))]) };
		var collision = Op(SetOperator.UnionAll, Table("q"));

		Assert.Throws<ArgumentException>(() => plain.Combine(collision));
		Assert.Throws<ArgumentException>(() => sorted.Combine(collision));

		var shared = (sorted with { With = null }).Combine(collision) with { With = with };

		Assert.Equal("WITH q AS (TABLE a) (TABLE q ORDER BY x) UNION ALL TABLE q", Sql2023Writer.Write(shared));
		Assert.Equal(shared, Both.ParseQueryExpression(Sql2023Writer.Write(shared)));
	}

	[Fact]
	public void A_limit_that_holds_what_its_text_has_no_place_for_is_refused()
	{
		Assert.Throws<ArgumentException>(() => Sql2023Writer.Write(A with { Limit = new Limit() }));
		Assert.Throws<ArgumentException>(() => Sql2023Writer.Write(A with { Limit = new Limit { Count = Column("n"), OffsetRowWord = RowWord.Row } }));
		Assert.Throws<ArgumentException>(() => Sql2023Writer.Write(A with { Limit = new Limit { Offset = Column("n"), Position = FetchPosition.Next } }));
		Assert.Equal("TABLE a OFFSET n ROWS", Sql2023Writer.Write(A with { Limit = new Limit { Offset = Column("n") } }));
	}

	/// <summary>
	/// The walker visits a query in the order it is written: its WITH, then what the member holds — the
	/// select list and FROM, or the query in the brackets — then the set operations and the clauses of
	/// the whole result, though those are declared on the family's base.
	/// </summary>
	[Theory]
	[InlineData("WITH q AS (TABLE e) SELECT x FROM a UNION TABLE b ORDER BY y", "e x a b y")]
	[InlineData("WITH q AS (TABLE e) (TABLE a ORDER BY z) UNION TABLE b ORDER BY y", "e a z b y")]
	public void The_walker_visits_a_query_in_text_order(string input, string order)
	{
		var seen = new System.Collections.Generic.List<string>();

		SqlWalker.Walk(Both.ParseQueryExpression(input), node =>
		{
			if (node is TableSource.Named named)
				seen.Add(named.Name.Parts[0].Text);
			else if (node is Expression.Reference reference)
				seen.Add(reference.Name.Parts[0].Text);

			return true;
		});

		Assert.Equal(order, string.Join(" ", seen));
	}

	// ── What only T-SQL writes ─────────────────────────────────────────────────

	/// <summary>
	/// TOP is of one query specification and of a data change statement's target. No grammar builds
	/// these nodes yet — T-SQL still reads into the old tree — so the writer's TOP is held here.
	/// </summary>
	[Fact]
	public void TOP_is_written_on_a_query_and_on_the_data_change_statements()
	{
		var top = new TopClause(new LiteralValue.Numeric("10", NumericLiteralKind.DecimalInteger), Parentheses: true, Percent: true, With: null);
		var t   = new TableSource.Named(Name("t"));

		Assert.Equal(
			"SELECT DISTINCT TOP (10) PERCENT x FROM t",
			Sql2023Writer.Write(new Query.Select
			{
				Quantifier = SetQuantifier.Distinct,
				Top = top,
				Columns = [new SelectColumn.ExpressionItem(Column("x"))],
				From = new FromClause([t]),
			}));
		Assert.Equal("DELETE TOP (10) PERCENT FROM t", Sql2023Writer.Write(new Statement.Delete { Top = top, Target = t }).TrimEnd(';', ' '));
		Assert.Equal(
			"UPDATE TOP (10) PERCENT t SET x = 1",
			Sql2023Writer.Write(new Statement.Update
			{
				Top = top,
				Target = t,
				Assignments = [new Assignment([new AssignmentTarget(Name("x"))], new LiteralValue.Numeric("1", NumericLiteralKind.DecimalInteger))],
			}).TrimEnd(';', ' '));
		Assert.StartsWith("INSERT TOP (10) PERCENT INTO t", Sql2023Writer.Write(new Statement.Insert { Top = top, Target = t }), StringComparison.Ordinal);
	}

	[Fact]
	public void A_statements_hints_are_written_after_its_query()
	{
		var statement = new Statement.Select
		{
			Query = A,
			Hints = [new QueryHint.Named(new Identifier("RECOMPILE"), [])],
		};

		Assert.EndsWith(" OPTION (RECOMPILE)", Sql2023Writer.Write(statement).TrimEnd(';', ' '), StringComparison.Ordinal);
	}
}
