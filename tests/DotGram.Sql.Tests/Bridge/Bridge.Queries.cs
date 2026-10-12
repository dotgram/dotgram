using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// <c>SELECT</c>: the statement, the query and its set operations, the select list, <c>ORDER BY</c>,
	/// <c>OFFSET</c> and <c>FETCH</c>, <c>TOP</c>, <c>GROUP BY</c>, <c>WITH</c>.
	/// </summary>
	internal static class Queries
	{
		public static readonly string[] Owns =
		[
			"Statement.Select",
			"Query.Specification", "Query.TableValueConstructor", "Query.Parenthesized", "Query.Ordered",
			"Query.ExplicitTable", "Query.Union", "Query.Except", "Query.Intersect",
			"Clause.DerivedColumn", "Clause.QualifiedAsterisk", "Clause.SortSpecification", "Clause.OrderBy",
			"Clause.Top", "Clause.GroupBy", "Clause.Into", "Clause.For", "Clause.Hint",
			"Clause.CommonTableExpression", "Clause.VariableAssignment",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				Statement.Select select                            => Select(select),
				Query query when Owned(Owns, query)                => Of(query),
				Clause.DerivedColumn or Clause.QualifiedAsterisk
					or Clause.VariableAssignment                   => Column((Clause)old),
				Clause.CommonTableExpression common                => Common(common),
				Clause.SortSpecification sort                      => Sort(sort),
				Clause when Owned(Owns, old)                       => throw BridgeGap.Open(old, $"inventory {NameOf(old)} (stands only inside a query)"),
				_                                                  => null,
			};

			return made is not null;
		}

		static Ast.Statement Select(Statement.Select old)
		{
			if (old.Options.Length > 0)
				throw BridgeGap.Unwritten(old.Options[0], "Statement.Select.Hints (QueryHint.Named)");

			if (old.For.Length > 0)
				throw BridgeGap.Unwritten(old.For[0], "Query.For (ForClause)");

			var query = Of(old.Of);

			if (old.OrderBy is { } by)
				query = Ordered(query, by);

			if (old.With.Length > 0)
				query = query with { With = With(old.With) };

			return new Ast.Statement.Select { Query = query };
		}

		public static Ast.WithClause With(SqlList<Clause> with)
		{
			return new Ast.WithClause(false, Each(with, static one => one is Clause.CommonTableExpression common
				? Common(common)
				: throw BridgeGap.Missing(one, "WithClause has no list for WITH XMLNAMESPACES (proposal 1)")));
		}

		static Ast.CommonTableExpression Common(Clause.CommonTableExpression old)
		{
			return new Ast.CommonTableExpression(Values.Part(old, old.Name), Values.Parts(old, old.Columns), QueryOf(old.Query));
		}

		public static Ast.Query Of(Query old)
		{
			switch (old)
			{
				case Query.Specification specification:
					return Specification(specification);

				case Query.TableValueConstructor(var rows):
					return new Ast.Query.Select { Values = Each(rows, Row) };

				case Query.ExplicitTable(var name):
					return new Ast.Query.Select
					{
						Columns = [new Ast.SelectColumn.All()],
						From    = new Ast.FromClause([new Ast.TableSource.Named(Values.Name(old, name))]),
						Table   = true,
					};

				case Query.Parenthesized(var inner):
					return new Ast.Query.Parenthesized(Of(inner));

				case Query.Ordered(var inner, var by, var shape):
					if (shape is not null)
						throw BridgeGap.Unwritten(shape, "Query.For (ForClause)");

					return by is null ? Of(inner) : Ordered(Of(inner), by);

				case Query.Union(var left, var right, var all):
					return Combined(left, all ? Ast.SetOperator.UnionAll : Ast.SetOperator.Union, right);

				case Query.Except(var left, var right, var all):
					return Combined(left, all ? Ast.SetOperator.ExceptAll : Ast.SetOperator.Except, right);

				case Query.Intersect(var left, var right, var all):
					return Combined(left, all ? Ast.SetOperator.IntersectAll : Ast.SetOperator.Intersect, right);

				default:
					return QueryOf(old);
			}
		}

		/// <summary>
		/// The old binary tree of set operations as the new left fold: the left side's list, and one more
		/// operation with the right side as its operand. The right side keeps its own list, which is what
		/// binds tighter there — <c>A UNION B INTERSECT C</c> is <c>A [UNION B [INTERSECT C]]</c>.
		/// </summary>
		static Ast.Query Combined(Query left, Ast.SetOperator op, Query right)
		{
			var first = Of(left);

			if (first.OrderBy is not null || first.Limit is not null || first.For is not null)
				throw BridgeGap.Open(left, "inventory Query.Union (a left operand with an order of its own and no brackets)");

			return first with { SetOperations = [.. first.SetOperations, new Ast.SetOperation(op, Of(right))] };
		}

		static Ast.Query Specification(Query.Specification old)
		{
			if (old.Into is not null)
				throw BridgeGap.Missing(old, "Query.Select has no member for SelectIntoTable (proposal 1)");

			return new Ast.Query.Select
			{
				Top        = Top(old.Top),
				Quantifier = Quantifier(old, old.Quantifier),
				Columns    = Each(old.Columns, Column),
				From       = old.From.Length > 0 ? new Ast.FromClause(Each(old.From, Source)) : null,
				Where      = old.Where is null ? null : Values.Of(old.Where),
				GroupBy    = old.GroupBy is null ? null : Group(old.GroupBy),
				Having     = old.Having is null ? null : Values.Of(old.Having),
				Window     = old.Windows is { Length: > 0 } windows ? new Ast.WindowClause(Each(windows, Window)) : null,
			};
		}

		static Ast.SetQuantifier? Quantifier(object owner, string? word)
		{
			return word?.ToUpperInvariant() switch
			{
				null       => null,
				"ALL"      => Ast.SetQuantifier.All,
				"DISTINCT" => Ast.SetQuantifier.Distinct,
				_          => throw BridgeGap.Open(owner, $"inventory Query.Specification (quantifier `{word}`)"),
			};
		}

		static Ast.WindowDefinition Window(Clause old)
		{
			return old is Clause.WindowDefinition(var name, Clause.Window specification)
				? new Ast.WindowDefinition(Values.Part(old, name), Values.Specification(specification))
				: throw BridgeGap.Open(old, "inventory Clause.WindowDefinition");
		}

		/// <summary>
		/// A row of <c>VALUES</c>: a row of several values in brackets, or one value, which keeps its own.
		/// </summary>
		public static Ast.RowValue Row(Expression row)
		{
			return row is Expression.RowValueConstructor(var items)
				? new Ast.RowValue(Each(items, Values.Of))
				: new Ast.RowValue([Values.Of(row)]);
		}

		// ── The select list ───────────────────────────────────────────────────────

		static Ast.SelectColumn Column(Clause old)
		{
			switch (old)
			{
				case Clause.DerivedColumn(var value, null):
					return new Ast.SelectColumn.ExpressionItem(Values.Of(value));

				case Clause.DerivedColumn(var value, var name):
					if (name![0] is '\'' or 'N' or 'n' && name.EndsWith('\''))
						throw BridgeGap.Open(old, "inventory Clause.DerivedColumn (a string alias)");

					// `x = a` and `a AS x` are one old node; where the value starts after the column, the
					// alias was written in front of it.
					if (Recover("Clause.DerivedColumn alias = expression", Spelled(old) is not null && Spelled(value) is not null ? "" : null, _ => value.Span.At > old.Span.At, false))
						throw BridgeGap.Open(old, "inventory Clause.DerivedColumn (alias = expression)");

					return new Ast.SelectColumn.ExpressionItem(Values.Of(value), Values.Part(old, name),
						Recover("SelectColumn.ExpressionItem.AsKeyword", WordAfter(value), static word => word == "AS", false));

				case Clause.QualifiedAsterisk(null):
					return new Ast.SelectColumn.All();

				case Clause.QualifiedAsterisk(var qualifier):
					return new Ast.SelectColumn.QualifiedAll(new Ast.Expression.Reference(Values.Name(old, qualifier!)));

				case Clause.VariableAssignment:
					throw BridgeGap.Missing(old, "SelectColumn.VariableAssignment (inventory Clause.VariableAssignment)");

				default:
					throw BridgeGap.Open(old, $"inventory {NameOf(old)} (in a select list)");
			}
		}

		static Ast.TopClause? Top(Clause? old)
		{
			if (old is null)
				return null;

			if (old is not Clause.Top(var value, var percent, var with))
				throw BridgeGap.Open(old, "inventory Clause.Top");

			if (with is not null && !with.Equals("TIES", StringComparison.OrdinalIgnoreCase))
				throw BridgeGap.Open(old, $"inventory Clause.Top (WITH {with})");

			return value is Expression.Parenthesized(var inner)
				? new Ast.TopClause(Values.Of(inner), true, percent, with is not null)
				: new Ast.TopClause(Values.Of(value), false, percent, with is not null);
		}

		public static Ast.TopClause? Topped(Clause? old)
		{
			return Top(old);
		}

		// ── ORDER BY, OFFSET, FETCH ───────────────────────────────────────────────

		public static Ast.OrderByClause Order(Clause.OrderBy old)
		{
			return new Ast.OrderByClause(Each(old.By, static one => one is Clause.SortSpecification sort
				? Sort(sort)
				: throw BridgeGap.Open(one, "inventory Clause.SortSpecification")));
		}

		static Ast.SortItem Sort(Clause.SortSpecification old)
		{
			Ast.SortDirection? direction = old.Order switch
			{
				SqlOrder.Ascending  => Ast.SortDirection.Asc,
				SqlOrder.Descending => Ast.SortDirection.Desc,
				_                   => null,
			};

			return new Ast.SortItem(Values.Of(old.Value), direction);
		}

		/// <summary>
		/// A query given its <c>ORDER BY</c>, and the <c>OFFSET</c> and <c>FETCH</c> the old tree keeps inside
		/// it, without the words around them: <c>ROW</c> or <c>ROWS</c>, <c>FIRST</c> or <c>NEXT</c> are read
		/// from the text, and are <c>ROWS</c> and <c>NEXT</c> where there is none.
		/// </summary>
		public static Ast.Query Ordered(Ast.Query query, Clause old)
		{
			if (old is not Clause.OrderBy by)
				throw BridgeGap.Open(old, "inventory Clause.OrderBy");

			if (query.OrderBy is not null || query.Limit is not null)
				throw BridgeGap.Open(old, "inventory Clause.OrderBy (a second order on one query)");

			Ast.Limit? limit = null;

			if (by.Offset is { } offset)
			{
				var after = by.Fetch is { } next ? Between(offset, next) : WordAfter(offset);

				limit = new Ast.Limit
				{
					Offset        = Values.Of(offset),
					OffsetRowWord = Recover("Limit.OffsetRowWord", after, static text => Words(text) is ["ROW", ..] ? Ast.RowWord.Row : Ast.RowWord.Rows, Ast.RowWord.Rows),
				};

				if (by.Fetch is { } fetch)
					limit = limit with
					{
						Count        = Values.Of(fetch),
						Position     = Recover("Limit.Position", after, static text => Words(text).Contains("FIRST") ? Ast.FetchPosition.First : Ast.FetchPosition.Next, Ast.FetchPosition.Next),
						FetchRowWord = Recover("Limit.FetchRowWord", WordAfter(fetch), static word => word == "ROW" ? Ast.RowWord.Row : Ast.RowWord.Rows, Ast.RowWord.Rows),
					};
			}
			else if (by.Fetch is not null)
				throw BridgeGap.Open(old, "inventory Clause.OrderBy (FETCH without OFFSET)");

			return query with { OrderBy = by.By.Length > 0 ? Order(by) : null, Limit = limit };
		}

		// ── GROUP BY ──────────────────────────────────────────────────────────────

		static Ast.GroupByClause Group(Clause old)
		{
			if (old is not Clause.GroupBy(var all, var by, var with))
				throw BridgeGap.Open(old, "inventory Clause.GroupBy");

			if (all)
				throw BridgeGap.Missing(old, "GroupByClause has no T-SQL ALL flag (proposal 5)");

			if (with is not null)
				throw BridgeGap.Missing(old, "GroupByClause has no WITH CUBE or WITH ROLLUP (proposal 5)");

			return new Ast.GroupByClause(null, Each(by, Grouping));
		}

		static Ast.GroupingElement Grouping(Expression old)
		{
			if (old is Expression.RoutineInvocation { Word: null } call)
			{
				var name = call.Name.ToUpperInvariant();

				if (name == "ROLLUP")
					return new Ast.GroupingElement.Rollup(Each(call.Arguments, Grouped));

				if (name == "CUBE")
					return new Ast.GroupingElement.Cube(Each(call.Arguments, Grouped));

				if (name == "GROUPING SETS")
					return new Ast.GroupingElement.Sets(Each(call.Arguments, Grouping));
			}

			return Grouped(old);
		}

		/// <summary>
		/// One element inside a <c>ROLLUP</c>, a <c>CUBE</c> or a plain list: a value, a row of them in brackets, or <c>()</c>.
		/// </summary>
		static Ast.GroupingElement Grouped(Expression old)
		{
			return old switch
			{
				Expression.RowValueConstructor { Values.Length: 0 } => new Ast.GroupingElement.Empty(),
				Expression.RowValueConstructor(var items)           => new Ast.GroupingElement.Ordinary(Each(items, Values.Of), true),
				_                                                   => new Ast.GroupingElement.Ordinary([Values.Of(old)]),
			};
		}
	}
}
