using System;
using System.Collections.Generic;

namespace DotGram.Sql.Ast;

public static partial class SqlWriter
{
	sealed partial class Text
	{
		// ── §7.13 Query expression ─────────────────────────────────────────────────

		/// <summary>A subquery: its brackets are its own, and a query in brackets in them is a second pair.</summary>
		void PutSubquery(Query query)
		{
			Open();
			PutQuery(query);
			Close();
		}

		// A query, its set operations and the clauses of its whole result. The brackets written are the
		// tree's Parenthesized nodes; a pair is added only where a tree built by hand would otherwise be
		// read back with another meaning — a list whose later operator binds tighter than an earlier one,
		// an operand whose own operations bind no tighter than the operator before it, or an operand with
		// clauses of its own. A tree read from text needs none of them.
		void PutQuery(Query query)
		{
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, query, static (into, node) => into.PutQuery(node));

				return;
			}

			if (query.With is { } with)
				PutWith(with);

			var operations = query.SetOperations;

			for (var at = Regroupings(operations); at > 0; at--)
				Open();

			// PostgreSQL: a data change statement where a query stands, which is a common table expression's body.
			if (query is Query.DataChange change)
			{
				if (!postgreSql)
					throw Misplaced(change, "Change", "a data change statement as a query, which only PostgreSQL reads (a WITH body)");

				if (change.Change is not (Statement.Insert or Statement.Update or Statement.Delete or Statement.Merge))
					throw Misplaced(change, "Change", "that is not an INSERT, UPDATE, DELETE or MERGE, which alone PostgreSQL reads as a query");

				if (operations.Count > 0 || query.OrderBy is not null || query.Limit is not null || query.Locking.Count > 0)
					throw Misplaced(change, "Change", "with set operations or clauses after it, which a data change takes none of");

				PutStatement(change.Change);

				return;
			}

			if (query is Query.Parenthesized parenthesized)
			{
				// PostgreSQL's select_with_parens holds a whole select_no_parens, WITH included.
				if (parenthesized.Query.With is not null && !postgreSql)
					throw Misplaced(parenthesized.Query, "With", "inside the brackets of a query, which take no WITH");

				Open();
				PutQuery(parenthesized.Query);
				Close();
			}
			else
				PutSpecification((Query.Select)query);

			var loosest = int.MaxValue;

			foreach (var operation in operations)
			{
				var binding = Binding(operation.Operator);

				if (binding > loosest)
				{
					Close();
					loosest = binding;
				}
				else
					loosest = Math.Min(loosest, binding);

				PutOperation(operation);
				PutOperand(operation.Query, binding);
			}

			if (query.OrderBy is { } order)
			{
				Word("ORDER BY");
				PutSorts(order);
			}

			if (query.Limit is { } limit)
				PutLimit(limit);

			if (query.For is { } forClause)
				PutFor(forClause);

			foreach (var locking in query.Locking)
			{
				Word(locking.Mode switch
				{
					LockingMode.ReadOnly    => "FOR READ ONLY",
					LockingMode.Update      => "FOR UPDATE",
					LockingMode.NoKeyUpdate => "FOR NO KEY UPDATE",
					LockingMode.Share       => "FOR SHARE",
					LockingMode.KeyShare    => "FOR KEY SHARE",
					_                       => throw NoText(locking.Mode),
				});

				if (locking.Of.Count > 0)
				{
					// The standard's OF names columns, one part each; PostgreSQL's names tables, qualified where written.
					if (!postgreSql)
						foreach (var of in locking.Of)
							if (of.Count != 1)
								throw Misplaced(locking, "Of", "with a name of more than one part, which only PostgreSQL reads (FOR UPDATE OF s.t)");

					Word("OF");
					Each(locking.Of, PutName);
				}

				if (locking.Wait is { } wait)
				{
					if (!postgreSql)
						throw Misplaced(locking, "Wait", "which only PostgreSQL reads, NOWAIT and SKIP LOCKED");

					Word(wait switch { LockWait.NoWait => "NOWAIT", LockWait.SkipLocked => "SKIP LOCKED", _ => throw NoText(wait) });
				}
			}
		}

		// INTERSECT binds tighter than UNION and EXCEPT, which bind alike.
		static int Binding(SetOperator op)
		{
			return op is SetOperator.Intersect or SetOperator.IntersectAll ? 2 : 1;
		}

		// How many leading parts of a list need brackets: one for each operator that binds tighter than
		// the loosest before it since the last such pair, since the list is a left fold and the text
		// would otherwise give that operator its left neighbour alone.
		static int Regroupings(SqlList<SetOperation> operations)
		{
			var count   = 0;
			var loosest = int.MaxValue;

			foreach (var operation in operations)
			{
				var binding = Binding(operation.Operator);

				if (binding > loosest)
				{
					count++;
					loosest = binding;
				}
				else
					loosest = Math.Min(loosest, binding);
			}

			return count;
		}

		void PutOperation(SetOperation operation)
		{
			Word(operation.Operator switch
			{
				SetOperator.Union        => "UNION",
				SetOperator.UnionAll     => "UNION ALL",
				SetOperator.Intersect    => "INTERSECT",
				SetOperator.IntersectAll => "INTERSECT ALL",
				SetOperator.Except       => "EXCEPT",
				SetOperator.ExceptAll    => "EXCEPT ALL",
				_                        => throw NoText(operation.Operator),
			});

			if (operation.Distinct)
			{
				if (operation.Operator is SetOperator.UnionAll or SetOperator.IntersectAll or SetOperator.ExceptAll)
					throw Both(operation, "ALL", "Distinct");

				Word("DISTINCT");
			}

			if (operation.Corresponding is { } corresponding)
			{
				Word("CORRESPONDING");

				if (corresponding.By)
				{
					Word("BY");
					Open();
					Each(corresponding.Columns, PutIdentifier);
					Close();
				}
			}
		}

		// An operand after an operator that binds as `binding` does. Its own operations stay its own
		// without brackets only where all of them bind tighter; its own clauses never do.
		void PutOperand(Query operand, int binding)
		{
			if (operand.With is not null)
				throw Misplaced(operand, "With", "on the operand of a set operation, which takes no WITH");

			var bracketed = operand.OrderBy is not null || operand.Limit is not null
				|| operand.For is not null || operand.Locking.Count > 0;

			foreach (var operation in operand.SetOperations)
				bracketed |= Binding(operation.Operator) <= binding;

			if (bracketed)
				PutSubquery(operand);
			else
				PutQuery(operand);
		}

		// What a limit holds that only FETCH or OFFSET writes is refused where the one that writes it is
		// not there, rather than dropped.
		void PutLimit(Limit limit)
		{
			var limited = limit.LimitKeyword || limit.LimitAll;
			var fetched = limit.Count is not null || limit.FetchRowWord is not null || limited;

			if (!fetched && limit.Offset is null)
				throw Empty(limit, "Offset", "Count", "FetchRowWord");

			if (!fetched && (limit.WithTies || limit.Percent || limit.Position != FetchPosition.First))
				throw Misplaced(limit, "WithTies, Percent or Position", "without a FETCH, which alone writes them");

			if (limit.Offset is null && limit.OffsetRowWord is not null)
				throw Misplaced(limit, "OffsetRowWord", "without an Offset, which alone writes it");

			// PostgreSQL's LIMIT: the count or ALL, and none of FETCH's words; its OFFSET may follow the count, and may stand without ROWS.
			if (limited && !postgreSql)
				throw Misplaced(limit, "LimitKeyword", "or LimitAll, which only PostgreSQL writes; the standard's count is FETCH FIRST n ROWS ONLY");

			if (limited && (limit.Percent || limit.WithTies || limit.FetchRowWord is not null || limit.Position != FetchPosition.First))
				throw Misplaced(limit, "Percent, WithTies, FetchRowWord or Position", "with LIMIT, which FETCH alone writes");

			if (limit.LimitAll && limit.Count is not null)
				throw Both(limit, "LimitAll", "Count");

			if (limit.OffsetAfter && !postgreSql)
				throw Misplaced(limit, "OffsetAfter", "which only PostgreSQL reads; the standard's OFFSET stands first");

			if (limit.OffsetAfter && (limit.Offset is null || !fetched))
				throw Misplaced(limit, "OffsetAfter", "without an Offset and a count for it to stand after");

			if (limit.Offset is { } offset && !limit.OffsetAfter)
				PutOffset(limit, offset);

			if (fetched)
			{
				if (limited)
				{
					Word("LIMIT");

					if (limit.LimitAll)
						Word("ALL");
					else
						PutExpression(limit.Count!);
				}
				else
				{
					Word(limit.Position switch { FetchPosition.First => "FETCH FIRST", FetchPosition.Next => "FETCH NEXT", _ => throw NoText(limit.Position) });

					// PostgreSQL's FETCH count is a primary or a signed number (select_fetch_first_value), never a larger expression.
					if (limit.Count is { } count)
						PutFetchValue(count);

					if (limit.Percent)
						Word("PERCENT");

					Word(RowText(limit.FetchRowWord));
					Word(limit.WithTies ? "WITH TIES" : "ONLY");
				}
			}

			if (limit.Offset is { } after && limit.OffsetAfter)
				PutOffset(limit, after);
		}

		/// <summary>OFFSET and its value: with ROW or ROWS where the word was written, and in PostgreSQL without where it was not.</summary>
		void PutOffset(Limit limit, Expression offset)
		{
			Word("OFFSET");

			if (limit.OffsetRowWord is null && postgreSql)
			{
				PutExpression(offset);
				return;
			}

			if (postgreSql)
				PutFetchValue(offset);
			else
				PutExpression(offset);

			Word(RowText(limit.OffsetRowWord));
		}

		/// <summary>A FETCH count, or an OFFSET with ROWS: in PostgreSQL a primary or a signed number, so anything else is bracketed.</summary>
		void PutFetchValue(Expression value)
		{
			if (postgreSql && value is not Expression.Unary { Operator: UnaryOperator.Minus or UnaryOperator.Plus, Operand: LiteralValue.Numeric })
				PutOperand(value, Tightness.Primary);
			else
				PutExpression(value);
		}

		/// <summary>ROW or ROWS; a word left out is ROWS.</summary>
		static string RowText(RowWord? word)
		{
			return word switch { null or RowWord.Rows => "ROWS", RowWord.Row => "ROW", _ => throw NoText(word.Value) };
		}

		void PutTop(TopClause? top)
		{
			if (top is null)
				return;

			Word("TOP");

			if (top.Parentheses)
			{
				Open();
				PutExpression(top.Value);
				Close();
			}
			else
				PutExpression(top.Value);

			if (top.Percent)
				Word("PERCENT");

			switch (top.With)
			{
				case null:
					break;

				case TopWith.Ties:
					Word("WITH TIES");
					break;

				case TopWith.Approximate:
					Word("WITH APPROXIMATE");
					break;

				default:
					throw NoText(top.With.Value);
			}
		}

		void PutSpecification(Query.Select query)
		{
			if (query.Values.Count > 0)
			{
				if (query.Top is not null || query.Quantifier is not null || query.Columns.Count > 0 || query.Table
					|| query.Into is not null || query.IntoTable is not null || query.From is not null || query.Where is not null
					|| query.GroupBy is not null || query.Having is not null || query.Window is not null)
					throw Both(query, "Values", "a select list or its clauses");

				Word("VALUES");
				Each(query.Values, PutRow);

				return;
			}

			if (query.Table && Explicit(query, postgreSql) is { } table)
			{
				Word("TABLE");

				// PostgreSQL's TABLE takes a relation_expr: ONLY and the inheritance star stand with the name.
				if (table.Only)
					Word("ONLY");

				PutName(table.Name);

				if (table.Star)
					Word("*");

				return;
			}

			// PostgreSQL reads a SELECT with no target list.
			if (query.Columns.Count == 0 && !postgreSql)
				throw Empty(query, "Columns", "Values");

			Word("SELECT");
			PutQuantifier(query.Quantifier);

			// PostgreSQL's DISTINCT ON: the values after DISTINCT, in brackets.
			if (query.DistinctOn.Count > 0)
			{
				if (query.Quantifier != SetQuantifier.Distinct)
					throw Misplaced(query, "DistinctOn", "without DISTINCT as the quantifier, which `DISTINCT ON (…)` says");

				if (!postgreSql)
					throw Misplaced(query, "DistinctOn", "which only PostgreSQL reads");

				Word("ON");
				Call();
				Each(query.DistinctOn, PutExpression);
				Close();
			}
			PutTop(query.Top);

			var outer = into;
			into = query.IntoTable is not null;
			Each(query.Columns, PutColumn);
			into = outer;

			if (query.Into is { } targets)
			{
				if (query.IntoTable is not null)
					throw Both(query, "Into", "IntoTable");

				Word("INTO");
				Each(targets.Targets, PutExpression);
			}

			// T-SQL: INTO Clause (Transact-SQL).
			if (query.IntoTable is { } made)
			{
				Word("INTO");
				PutName(made.Table);

				if (made.FileGroup is { } fileGroup)
				{
					Word("ON");
					PutIdentifier(fileGroup);
				}
			}

			if (query.From is { } from)
			{
				Word("FROM");
				Each(from.Sources, PutSource);
			}

			if (query.Where is { } where)
			{
				Word("WHERE");
				PutCondition(query, where);
			}

			if (query.GroupBy is { } group)
			{
				Word("GROUP BY");

				// T-SQL: GROUP BY (Transact-SQL). `ALL` and `WITH CUBE` or `WITH ROLLUP`, which do not go together.
				if (group.AllKeyword)
				{
					if (group.Quantifier is not null)
						throw Both(group, "AllKeyword", "Quantifier");

					if (group.With is not null)
						throw Both(group, "AllKeyword", "With");

					Word("ALL");
				}

				PutQuantifier(group.Quantifier);
				Each(group.Items, PutGrouping);

				if (group.With is { } with)
					Word(with switch
					{
						GroupByWith.Cube   => "WITH CUBE",
						GroupByWith.Rollup => "WITH ROLLUP",
						_                  => throw NoText(with),
					});
			}

			if (query.Having is { } having)
			{
				Word("HAVING");
				PutCondition(query, having);
			}

			if (query.Window is { } window)
			{
				Word("WINDOW");
				Each(window.Windows, definition =>
				{
					PutIdentifier(definition.Name);
					Word("AS");
					PutWindow(definition.Specification);
				});
			}
		}

		// The table of `TABLE t` where the query is still exactly `SELECT * FROM t`; null where it has
		// been changed into anything more, which is then written in full rather than lost.
		/// <summary>The one table of a query that is `SELECT * FROM t` and nothing else; with ONLY or the star where the reading takes a relation there (PostgreSQL).</summary>
		static TableSource.Named? Explicit(Query.Select query, bool relation)
		{
			if (query is not { Columns: [SelectColumn.All], From.Sources: [TableSource.Named { Hints.Count: 0 } named], Top: null, Quantifier: null, Into: null, IntoTable: null, Where: null, GroupBy: null, Having: null, Window: null })
				return null;

			// A property pattern rather than record equality: equality probes the stack, and this runs at
			// the depth the caller last checked, so on a deep tree it could throw where the writer would
			// otherwise hand the walk to a fresh stack.
			return named.GetType() == typeof(TableSource.Named)
				&& named is { Sample: null, SystemTime: null, Alias: null, DataSource: null, ForPath: false }
				&& (relation || named is { Only: false, Star: false })
				? named
				: null;
		}

		// A row of values: its values in brackets, or a value alone, which may have brackets of its own.
		void PutRow(RowValue row)
		{
			if (row.RowKeyword)
			{
				Word("ROW");
				Call();
				Each(row.Items, PutExpression);
				Close();
			}
			// The standard writes a row of one value bare; PostgreSQL's VALUES takes rows in brackets only.
			else if (row.Items.Count == 1 && !postgreSql)
				PutExpression(row.Items[0]);
			else
			{
				Open();
				Each(row.Items, PutExpression);
				Close();
			}
		}

		/// <summary>
		/// A select column's alias: a name, or T-SQL's string, <c>'x'</c> or <c>N'x'</c>, its quotes doubled again.
		/// </summary>
		void PutColumnAlias(Identifier alias)
		{
			switch (alias.Style)
			{
				case IdentifierStyle.String:
					Word("'" + alias.Text.Replace("'", "''") + "'");
					break;

				case IdentifierStyle.NationalString:
					Word("N'" + alias.Text.Replace("'", "''") + "'");
					break;

				default:
					PutIdentifier(alias);
					break;
			}
		}

		void PutColumn(SelectColumn column)
		{
			switch (column)
			{
				case SelectColumn.All:
					Word("*");
					break;

				// T-SQL: SELECT Clause (Transact-SQL). `x = a`, the alias first; a regular alias starting with @ would
				// be SelectColumn.VariableAssignment.
				case SelectColumn.ExpressionItem { AliasFirst: true } first:
					if (first.Alias is not { } named || first.AsKeyword || named.Style == IdentifierStyle.Regular && named.Text.StartsWith("@", StringComparison.Ordinal))
						throw Misplaced(first, "AliasFirst", "without an alias, with AS, or with an alias that is a variable's name");

					PutColumnAlias(named);
					Word("=");
					identityItem = into && first.Expression is Expression.Identity;
					PutExpression(first.Expression);
					break;

				case SelectColumn.ExpressionItem expression:
					identityItem = into && expression.Alias is not null && expression.Expression is Expression.Identity;
					PutExpression(expression.Expression);

					if (expression.Alias is { } alias)
					{
						if (expression.AsKeyword)
							Word("AS");

						PutColumnAlias(alias);
					}

					break;

				// T-SQL: SELECT @local_variable (Transact-SQL).
				case SelectColumn.VariableAssignment assignment:
					PutVariableName(assignment, assignment.Variable);
					Word(AssignmentText(assignment.Operator));
					PutExpression(assignment.Value);
					break;

				default:
					var all = (SelectColumn.QualifiedAll)column;

					PutExpression(all.Qualifier);
					Tight(".*");

					if (all.RenamedFields is { } fields)
					{
						Word("AS");
						Open();
						Each(fields, PutIdentifier);
						Close();
					}

					break;
			}
		}

		void PutWith(WithClause with)
		{
			Word(with.Recursive ? "WITH RECURSIVE" : "WITH");

			// T-SQL: WITH XMLNAMESPACES (Transact-SQL). Before the common table expressions, and one of the list.
			if (with.XmlNamespaces.Count > 0)
			{
				if (with.Recursive)
					throw Both(with, "XmlNamespaces", "Recursive");

				PutXmlNamespaces(with.XmlNamespaces);

				if (with.Items.Count > 0)
					Tight(",");
			}

			// T-SQL: WITH CHANGE_TRACKING_CONTEXT (Transact-SQL). One of the list, in the namespaces' place.
			if (with.ChangeTrackingContext is { } context)
			{
				if (with.Recursive || with.XmlNamespaces.Count > 0)
					throw Misplaced(with, "ChangeTrackingContext", "beside RECURSIVE or XMLNAMESPACES, which WithClause reads in its place");

				Word("CHANGE_TRACKING_CONTEXT");
				Open();
				PutExpression(context);
				Close();

				if (with.Items.Count > 0)
					Tight(",");
			}

			Each(with.Items, common =>
			{
				PutIdentifier(common.Name);

				if (common.Columns.Count > 0)
				{
					Open();
					Each(common.Columns, PutIdentifier);
					Close();
				}

				Word("AS");

				if (common.Materialized is { } materialized)
				{
					if (!postgreSql)
						throw Misplaced(common, "Materialized", "which only PostgreSQL reads, AS [NOT] MATERIALIZED");

					Word(materialized ? "MATERIALIZED" : "NOT MATERIALIZED");
				}

				PutSubquery(common.Query);

				if (common.Search is { } search)
				{
					Word(search.Order switch { SearchOrder.DepthFirst => "SEARCH DEPTH FIRST BY", SearchOrder.BreadthFirst => "SEARCH BREADTH FIRST BY", _ => throw NoText(search.Order) });
					Each(search.Columns, PutIdentifier);
					Word("SET");
					PutIdentifier(search.SequenceColumn);
				}

				if (common.Cycle is { } cycle)
				{
					Word("CYCLE");
					Each(cycle.Columns, PutIdentifier);
					Word("SET");
					PutIdentifier(cycle.MarkColumn);

					if (cycle.MarkValue is { } mark)
					{
						Word("TO");
						PutExpression(mark);
						Word("DEFAULT");
						PutExpression(cycle.NonCycleMarkValue!);
					}

					Word("USING");
					PutIdentifier(cycle.PathColumn);
				}
			});
		}

		void PutGrouping(GroupingElement element)
		{
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, element, static (into, node) => into.PutGrouping(node));

				return;
			}

			switch (element)
			{
				// T-SQL: TSqlGroupingColumn's WITH (DISTRIBUTED_AGG), after a column and never after a list in brackets.
				case GroupingElement.Ordinary { DistributedAggregation: true } hinted when !transactSql || hinted.Parenthesized:
					throw Misplaced(hinted, "DistributedAggregation", transactSql ? "on a list in brackets, which takes no WITH (TSqlGroupingColumn)" : "in the standard, which reads none");

				case GroupingElement.Ordinary ordinary when ordinary.Parenthesized:
					Open();
					Each(ordinary.Expressions, PutExpression);
					Close();
					break;

				case GroupingElement.Ordinary ordinary:
					PutExpression(ordinary.Expressions[0]);

					if (ordinary.DistributedAggregation)
					{
						Word("WITH");
						Open();
						Word("DISTRIBUTED_AGG");
						Close();
					}

					break;

				case GroupingElement.Rollup rollup:
					Word("ROLLUP");
					Call();
					Each(rollup.Items, PutGrouping);
					Close();
					break;

				case GroupingElement.Cube cube:
					Word("CUBE");
					Call();
					Each(cube.Items, PutGrouping);
					Close();
					break;

				case GroupingElement.Sets sets:
					Word("GROUPING SETS");
					Call();
					Each(sets.Items, PutGrouping);
					Close();
					break;

				default:
					Open();
					Close();
					break;
			}
		}

		static string SortWord(SortDirection direction)
		{
			return direction switch { SortDirection.Asc => "ASC", SortDirection.Desc => "DESC", _ => throw NoText(direction) };
		}

		void PutSorts(OrderByClause order)
		{
			Each(order.Items, item =>
			{
				PutExpression(item.Key);

				if (item.Direction is { } direction)
					Word(SortWord(direction));

				if (item.NullOrdering is { } nulls)
					Word(nulls switch { NullOrdering.First => "NULLS FIRST", NullOrdering.Last => "NULLS LAST", _ => throw NoText(nulls) });
			});
		}

		// ── §7.11 Windows, §7.6 Row pattern recognition ────────────────────────────

		void PutWindow(WindowSpecification specification)
		{
			Open();

			if (specification.Existing is { } existing)
				PutIdentifier(existing);

			if (specification.PartitionBy.Count > 0)
			{
				Word("PARTITION BY");
				Each(specification.PartitionBy, PutExpression);
			}

			if (specification.OrderBy is { } order)
			{
				Word("ORDER BY");
				PutSorts(order);
			}

			if (specification.Frame is { } frame)
			{
				if (frame.Pattern is { } measured)
					PutMeasures(measured.Measures);

				Word(frame.Unit switch { WindowFrameUnit.Rows => "ROWS", WindowFrameUnit.Range => "RANGE", WindowFrameUnit.Groups => "GROUPS", _ => throw NoText(frame.Unit) });

				if (frame.Extent is WindowFrameExtent.Between between)
				{
					Word("BETWEEN");
					PutBound(between.From);
					Word("AND");
					PutBound(between.To);
				}
				else
					PutBound(((WindowFrameExtent.Single)frame.Extent).Bound);

				if (frame.Exclusion is { } exclusion)
					Word(exclusion switch
					{
						WindowFrameExclusion.CurrentRow => "EXCLUDE CURRENT ROW",
						WindowFrameExclusion.Group      => "EXCLUDE GROUP",
						WindowFrameExclusion.Ties       => "EXCLUDE TIES",
						WindowFrameExclusion.NoOthers                               => "EXCLUDE NO OTHERS",
						_                               => throw NoText(exclusion),
					});

				// A frame's pattern holds the measures before the units, and the common syntax after, where one was written.
				if (frame.Pattern is { Pattern: not null } common)
					PutCommon(common);
			}

			Close();
		}

		void PutBound(WindowFrameBound bound)
		{
			// A bound that counts says how many, and one that does not has nothing to say: an offset dropped or missing is another bound.
			var counts = bound.Kind is WindowBoundKind.Preceding or WindowBoundKind.Following;

			if (counts != bound.Offset is not null)
				throw Misplaced(bound, "Offset", counts ? "missing on a bound that counts" : "on a bound that names no count");

			switch (bound.Kind)
			{
				case WindowBoundKind.UnboundedPreceding:
					Word("UNBOUNDED PRECEDING");
					break;

				case WindowBoundKind.Preceding:
					PutExpression(bound.Offset!);
					Word("PRECEDING");
					break;

				case WindowBoundKind.CurrentRow:
					Word("CURRENT ROW");
					break;

				case WindowBoundKind.Following:
					PutExpression(bound.Offset!);
					Word("FOLLOWING");
					break;

				case WindowBoundKind.UnboundedFollowing:
					Word("UNBOUNDED FOLLOWING");
					break;

				default:
					throw NoText(bound.Kind);
			}
		}

		void PutMeasures(SqlList<RowPatternMeasure> measures)
		{
			if (measures.Count == 0)
				return;

			Word("MEASURES");
			Each(measures, measure =>
			{
				PutExpression(measure.Expression);
				Word("AS");
				PutIdentifier(measure.Name);
			});
		}

		void PutRecognition(RowPatternClause clause)
		{
			Word("MATCH_RECOGNIZE");
			Open();

			if (clause.PartitionBy.Count > 0)
			{
				Word("PARTITION BY");
				Each(clause.PartitionBy, PutExpression);
			}

			if (clause.OrderBy is { } order)
			{
				Word("ORDER BY");
				PutSorts(order);
			}

			PutMeasures(clause.Measures);

			if (clause.RowsPerMatch is { } rows)
				Word(rows switch
				{
					RowsPerMatch.One              => "ONE ROW PER MATCH",
					RowsPerMatch.All              => "ALL ROWS PER MATCH",
					RowsPerMatch.AllShowEmpty     => "ALL ROWS PER MATCH SHOW EMPTY MATCHES",
					RowsPerMatch.AllOmitEmpty     => "ALL ROWS PER MATCH OMIT EMPTY MATCHES",
					RowsPerMatch.AllWithUnmatched                             => "ALL ROWS PER MATCH WITH UNMATCHED ROWS",
					_                             => throw NoText(rows),
				});

			PutCommon(clause);
			Close();
		}

		void PutCommon(RowPatternClause clause)
		{
			if (clause.AfterMatch is { } skip)
			{
				Word("AFTER MATCH");

				// A variable is what SKIP TO FIRST, LAST and a plain SKIP TO name, and the other two name none.
				if ((skip.Kind is RowPatternSkipKind.First or RowPatternSkipKind.Last or RowPatternSkipKind.Variable) != (skip.Variable is not null))
					throw Misplaced(skip, "Variable", skip.Variable is null ? "missing where the skip names one" : "where the skip names none");

				switch (skip.Kind)
				{
					case RowPatternSkipKind.NextRow:
						Word("SKIP TO NEXT ROW");
						break;

					case RowPatternSkipKind.PastLastRow:
						Word("SKIP PAST LAST ROW");
						break;

					case RowPatternSkipKind.First:
						Word("SKIP TO FIRST");
						PutIdentifier(skip.Variable!);
						break;

					case RowPatternSkipKind.Last:
						Word("SKIP TO LAST");
						PutIdentifier(skip.Variable!);
						break;

					case RowPatternSkipKind.Variable:
						Word("SKIP TO");
						PutIdentifier(skip.Variable!);
						break;

					default:
						throw NoText(skip.Kind);
				}
			}

			if (clause.Initial is { } initial)
				Word(initial switch { RowPatternInitial.Initial => "INITIAL", RowPatternInitial.Seek => "SEEK", _ => throw NoText(initial) });

			Word("PATTERN");
			Open();

			if (clause.Pattern is { } pattern)
				PutPattern(pattern);

			Close();

			if (clause.Subsets.Count > 0)
			{
				Word("SUBSET");
				Each(clause.Subsets, subset =>
				{
					PutIdentifier(subset.Name);
					Word("=");
					Open();
					Each(subset.Variables, PutIdentifier);
					Close();
				});
			}

			Word("DEFINE");
			Each(clause.Definitions, definition =>
			{
				PutIdentifier(definition.Variable);
				Word("AS");
				PutCondition(definition, definition.Condition);
			});
		}

		void PutPattern(RowPattern pattern)
		{
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, pattern, static (into, node) => into.PutPattern(node));

				return;
			}

			switch (pattern)
			{
				case RowPattern.Variable variable:
					PutIdentifier(variable.Name);
					break;

				case RowPattern.StartAnchor:
					Word("^");
					break;

				case RowPattern.EndAnchor:
					Word("$");
					break;

				case RowPattern.Sequence sequence:
					foreach (var item in sequence.Items)
						PutPattern(item);

					break;

				case RowPattern.Alternation alternation:
					for (var at = 0; at < alternation.Items.Count; at++)
					{
						if (at > 0)
							Word("|");

						PutPattern(alternation.Items[at]);
					}

					break;

				case RowPattern.Quantified quantified:
					PutPattern(quantified.Pattern);

					var quantifier = quantified.Quantifier;

					switch (quantifier.Kind)
					{
						case RowPatternQuantifierKind.ZeroOrMore:
							Tight("*");
							break;

						case RowPatternQuantifierKind.OneOrMore:
							Tight("+");
							break;

						case RowPatternQuantifierKind.ZeroOrOne:
							Tight("?");
							break;

						case RowPatternQuantifierKind.Exact:
							Tight("{" + quantifier.Min + "}");
							break;

						case RowPatternQuantifierKind.Range:
							Tight("{" + quantifier.Min + "," + quantifier.Max + "}");
							break;

						default:
							throw NoText(quantifier.Kind);
					}

					if (quantifier.Reluctant)
						Tight("?");

					break;

				case RowPattern.Parenthesized parenthesized:
					Open();

					if (parenthesized.Pattern is { } inner)
						PutPattern(inner);

					Close();
					break;

				case RowPattern.Excluded excluded:
					Word("{-");
					PutPattern(excluded.Pattern);
					Word("-}");
					break;

				default:
					Word("PERMUTE");
					Call();
					Each(((RowPattern.Permute)pattern).Items, PutPattern);
					Close();
					break;
			}
		}

		// ── §7.6 Table references ──────────────────────────────────────────────────

		void PutSource(TableSource source)
		{
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, source, static (into, node) => into.PutSource(node));

				return;
			}

			switch (source)
			{
				case TableSource.Named named:
					// T-SQL: a name of one part starting with @ is a table variable, which is a record of its own.
					if (named.Name is [{ Style: IdentifierStyle.Regular } only] && only.Text.StartsWith("@", StringComparison.Ordinal))
						throw Misplaced(named, "Name", "of one part starting with @, which is a TableSource.Variable");

					// T-SQL: OPENDATASOURCE (Transact-SQL). The server the name is looked up on, before it; it takes a
					// correlation name and nothing else.
					if (named.DataSource is { } server)
					{
						if (named.Only)
							throw Both(named, "DataSource", "Only");

						if (named.Hints.Count > 0 || named.Sample is not null || named.SystemTime is not null || named.Alias?.Columns is not null || named.ForPath)
							throw Misplaced(named, "Hints, Sample, SystemTime, ForPath or alias columns", "on a name after OPENDATASOURCE");

						Word("OPENDATASOURCE");
						Call();
						PutExpression(server.Provider);
						Tight(",");
						PutExpression(server.InitString);
						Close();
						Tight(".");
						Hold();
					}

					if (named.Only)
					{
						Word("ONLY");
						Open();
						PutName(named.Name);
						Close();
					}
					else
						PutName(named.Name);

					// PostgreSQL's `t *`: the descendants said in so many words.
					if (named.Star)
					{
						if (!postgreSql || named.Only)
							throw Misplaced(named, "Star", named.Only ? "beside Only, which says the opposite" : "which only PostgreSQL reads");

						Word("*");
					}

					if (named.SystemTime is { } time)
					{
						Word("FOR SYSTEM_TIME");

						switch (time)
						{
							case SystemTimeSpecification.AsOf asOf:
								Word("AS OF");
								PutExpression(asOf.Point);
								break;

							case SystemTimeSpecification.Between between:
								Word("BETWEEN");

								if (between.Symmetry is { } symmetry)
									Word(symmetry switch { BetweenSymmetry.Asymmetric => "ASYMMETRIC", BetweenSymmetry.Symmetric => "SYMMETRIC", _ => throw NoText(symmetry) });

								PutExpression(between.From);
								Word("AND");
								PutExpression(between.To);
								break;

							// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL).
							case SystemTimeSpecification.ContainedIn contained:
								Word("CONTAINED IN");
								Open();
								PutExpression(contained.From);
								Tight(",");
								PutExpression(contained.To);
								Close();
								break;

							case SystemTimeSpecification.All:
								Word("ALL");
								break;

							default:
								var range = (SystemTimeSpecification.FromTo)time;

								Word("FROM");
								PutExpression(range.From);
								Word("TO");
								PutExpression(range.To);
								break;
						}
					}

					// T-SQL: MATCH (SQL Graph). After the version, before the alias.
					if (named.ForPath)
						Word("FOR PATH");

					PutAlias(named.Alias);
					break;

				case TableSource.Subquery subquery:
					if (subquery.Lateral)
						Word("LATERAL");

					PutSubquery(subquery.Query);

					// T-SQL: MATCH (SQL Graph).
					if (subquery.ForPath)
					{
						if (subquery.Lateral)
							throw Both(subquery, "Lateral", "ForPath");

						Word("FOR PATH");
					}

					PutAlias(subquery.Alias);
					break;

				case TableSource.Join join:
					PutSource(join.Left);
					PutPartition(join.LeftPartition);

					if (join.Kind == JoinKind.Cross)
					{
						if (join.Hint is not null)
							throw Misplaced(join, "Hint", "on a CROSS JOIN, which takes none");

						Word("CROSS JOIN");
					}
					// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL). APPLY takes the place of the JOIN and
					// of what a join says beside its kind.
					else if (join.Kind is JoinKind.CrossApply or JoinKind.OuterApply)
					{
						if (join.Natural || join.OuterKeyword || join.Hint is not null || join.Specification is not null)
							throw Misplaced(join, "Natural, OuterKeyword, Hint or Specification", "on an APPLY");

						Word(join.Kind == JoinKind.CrossApply ? "CROSS APPLY" : "OUTER APPLY");
					}
					else if (join.Kind == JoinKind.Union)
						PutUnionJoin(join);
					else
					{
						// T-SQL: Join hints (Transact-SQL). A hint stands inside a join type, and without one would be
						// read as the left side's alias.
						if (join.Hint is not null && join.Kind is null)
							throw Misplaced(join, "Hint", "without a join type");

						if (join.Natural)
							Word("NATURAL");

						if (join.Kind is { } kind)
							// Cross is written by the branch above, and T-SQL's two APPLY kinds are not a
							// word here at all: they replace the JOIN, so they are refused until the writer
							// takes them rather than answered with one of these.
							Word(kind switch
							{
								JoinKind.Inner => "INNER",
								JoinKind.Left  => "LEFT",
								JoinKind.Right => "RIGHT",
								JoinKind.Full  => "FULL",
								_              => throw NoText(kind),
							});

						if (join.OuterKeyword)
							Word("OUTER");

						// T-SQL: Join hints (Transact-SQL).
						if (join.Hint is { } hint)
							Word(hint switch
							{
								JoinHint.Loop   => "LOOP",
								JoinHint.Hash   => "HASH",
								JoinHint.Merge  => "MERGE",
								JoinHint.Remote => "REMOTE",
								_               => throw NoText(hint),
							});

						Word("JOIN");
					}

					// A right side that is a join is written bare only where this join writes its ON or USING after it:
					// both grammars read a qualified join's right side as a chain of joins that its specification closes
					// (SqlStandard.gram's JoinOperand: CROSS, NATURAL and qualified joins; TransactSql.gram's
					// JoinedRight: qualified joins with the pivots after their ON, CROSS JOIN and both APPLYs), so no
					// kind needs brackets there in either dialect. A CROSS JOIN, a NATURAL JOIN and an APPLY take one
					// source on the right, and a join there is bracketed, which reads back as Parenthesized.
					if (join.Right is TableSource.Join && join.Specification is null)
					{
						Open();
						PutSource(join.Right);
						Close();
					}
					else
						PutSource(join.Right);

					PutPartition(join.RightPartition);

					switch (join.Specification)
					{
						case JoinSpecification.On on:
							Word("ON");
							PutCondition(on, on.Condition);
							break;

						case JoinSpecification.Using @using:
							Word("USING");
							Open();
							Each(@using.Columns, PutIdentifier);
							Close();

							if (@using.Correlation is { } correlation)
							{
								Word("AS");
								PutIdentifier(correlation);
							}

							break;
					}

					break;

				case TableSource.Unnest unnest:
					if (unnest.Lateral)
					{
						if (!postgreSql)
							throw Misplaced(unnest, "Lateral", "which only PostgreSQL reads before unnest");

						Word("LATERAL");
					}

					Word("UNNEST");
					Call();
					Each(unnest.Expressions, PutExpression);
					Close();

					if (unnest.WithOrdinality)
						Word("WITH ORDINALITY");

					PutAlias(unnest.Alias);
					break;

				case TableSource.Function function:
					tableFunction = transactSql;

					// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (Transact-SQL). VariableSource: the variable, a dot and the
					// method's name of one part, against each other.
					if (function.Variable is { } variable)
					{
						if (!transactSql)
							throw Misplaced(function, "Variable", "in the standard, which calls no method on a variable in a FROM");

						// The method is the type's own and its arguments values (Arguments), whatever built-in its name shares.
						if (function.Invocation is not { Name: [var method], Quantifier: null, OrderBy: null, Overflow: null, From: null, Nulls: null, Semantics: null, Filter: null, Over: null, WithinGroup: null, Copartition: null, WithoutParentheses: false, GraphPath: false } called)
							throw Misplaced(function, "Variable", "with a method whose name has more than one part, or with more than a name and arguments (VariableSource: '.' & TSqlIdentifier & '(' & Arguments? & ')')");

						PutVariableName(function, variable);
						Tight(".");
						Hold();
						PutIdentifier(method);
						Call();
						Each(called.Arguments, PutArgument);
						Close();
					}
					else
					{
						// PostgreSQL's LATERAL, WITH ORDINALITY and column definition list (func_table, func_alias_clause).
						if (function.Lateral)
						{
							if (!postgreSql)
								throw Misplaced(function, "Lateral", "which only PostgreSQL reads before a function in FROM");

							Word("LATERAL");
						}

						PutExpression(function.Invocation);

						if (function.WithOrdinality)
						{
							if (!postgreSql)
								throw Misplaced(function, "WithOrdinality", "which only PostgreSQL reads after a function in FROM");

							Word("WITH ORDINALITY");
						}

						if (function.ColumnDefinitions is { } definitions)
						{
							if (!postgreSql)
								throw Misplaced(function, "ColumnDefinitions", "which only PostgreSQL reads after a function in FROM");

							if (function.Alias is { Columns: not null })
								throw Both(function, "ColumnDefinitions", "Alias.Columns");

							if (definitions.Count == 0)
								throw Empty(function, "ColumnDefinitions");

							if (function.Alias is { } named)
								PutAlias(named);
							else
								Word("AS");

							PutFieldDefinitions(definitions);
							break;
						}
					}

					tableFunction = false;
					PutAlias(function.Alias);
					break;

				case TableSource.RowsFrom rows:
					if (!postgreSql)
						throw Misplaced(rows, "Items", "that only PostgreSQL reads: ROWS FROM (…)");

					if (rows.Items.Count == 0)
						throw Empty(rows, "Items");

					if (rows.Lateral)
						Word("LATERAL");

					Word("ROWS FROM");
					Open();
					Each(rows.Items, item =>
					{
						PutExpression(item.Invocation);

						if (item.ColumnDefinitions is { } itemDefinitions)
						{
							if (itemDefinitions.Count == 0)
								throw Empty(item, "ColumnDefinitions");

							Word("AS");
							PutFieldDefinitions(itemDefinitions);
						}
					});
					Close();

					if (rows.WithOrdinality)
						Word("WITH ORDINALITY");

					if (rows.ColumnDefinitions is { } rowsDefinitions)
					{
						if (rows.Alias is { Columns: not null })
							throw Both(rows, "ColumnDefinitions", "Alias.Columns");

						if (rowsDefinitions.Count == 0)
							throw Empty(rows, "ColumnDefinitions");

						if (rows.Alias is { } rowsAlias)
							PutAlias(rowsAlias);
						else
							Word("AS");

						PutFieldDefinitions(rowsDefinitions);
						break;
					}

					PutAlias(rows.Alias);
					break;

				case TableSource.TableFunction function:
					Word("TABLE");
					Open();
					PutExpression(function.Value);
					Close();
					PutAlias(function.Alias);
					break;

				case TableSource.RowPatternRecognition recognition:
					PutSource(recognition.Source);
					PutRecognition(recognition.Clause);
					PutAlias(recognition.Alias);
					break;

				case TableSource.JsonTable json:
					PutJsonTable(json.Definition);
					PutAlias(json.Alias);
					break;

				// T-SQL: INSERT (Transact-SQL). The statement in brackets, its OUTPUT the rows, and the name they go by.
				// The grammar's ChangedStatement: an OUTPUT with no INTO and no second OUTPUT, and no OPTION or WITH.
				case TableSource.DataChange { Option: null } returned:
					var (returning, hints, with) = returned.Change switch
					{
						Statement.Insert insert => (insert.Returning, insert.Hints, insert.With),
						Statement.Update update => (update.Returning, update.Hints, update.With),
						Statement.Delete delete => (delete.Returning, delete.Hints, delete.With),
						Statement.Merge merge   => (merge.Returning, merge.Hints, merge.With),
						_                       => (null, [], null),
					};

					if (returning is not ReturningClause.Output output)
						throw Misplaced(returned, "Option", "left out on a statement with no OUTPUT");

					if (output.Into is not null || output.Next is not null)
						throw Misplaced(returned, "Change", "whose OUTPUT has INTO or a second OUTPUT, which T-SQL's form does not return");

					if (hints.Count > 0 || with is not null)
						throw Misplaced(returned, "Change", "with OPTION or WITH, which T-SQL's form does not read");

					if (returned.Alias is null)
						throw Misplaced(returned, "Alias", "missing, which T-SQL's form requires");

					if (!ReferenceEquals(returned, returnedRows))
						throw Misplaced(returned, "Option", "left out other than as the one table of an INSERT's SELECT, the one place T-SQL's form stands");

					Open();
					PutStatement(returned.Change);
					Close();
					PutAlias(returned.Alias);
					break;

				case TableSource.DataChange change:
					Word(change.Option switch { ResultOption.Final => "FINAL", ResultOption.New => "NEW", ResultOption.Old => "OLD", _ => throw NoText(change.Option!.Value) });
					Word("TABLE");
					Open();
					PutStatement(change.Change);
					Close();
					PutAlias(change.Alias);
					break;

				case TableSource.Parenthesized parenthesized:
					Open();
					PutSource(parenthesized.Source);
					Close();

					// PostgreSQL's alias of a bracketed join.
					if (parenthesized.Alias is { } bracketAlias)
					{
						if (!postgreSql)
							throw Misplaced(parenthesized, "Alias", "which only PostgreSQL reads on a bracketed join");

						PutAlias(bracketAlias);
					}

					break;

				default:
					if (TryPutTransactSql(source))
						break;

					throw Unwritten(source);
			}

			var table = source as TableSource.Named;

			// A sample clause follows a table primary and all it was given.
			if (source.Sample is { } sample)
			{
				// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL). What T-SQL adds to a sample, which it reads
				// after a table's name only.
				if ((sample.MethodOmitted || sample.Unit is not null) && table is null)
					throw Misplaced(sample, "MethodOmitted or Unit", "on the sample of a source other than a table's name");

				if (sample.MethodOmitted)
				{
					if (sample.Method != SampleMethod.System)
						throw Misplaced(sample, "MethodOmitted", "on a sample whose method is not SYSTEM");

					Word("TABLESAMPLE");
				}
				else if (transactSql && sample.Method != SampleMethod.System)
					throw Misplaced(sample, "Method", "other than SYSTEM, which is the only one T-SQL reads");
				else
					Word(sample.Method switch { SampleMethod.Bernoulli => "TABLESAMPLE BERNOULLI", SampleMethod.System => "TABLESAMPLE SYSTEM", _ => throw NoText(sample.Method) });

				Open();
				PutExpression(sample.Percentage);

				if (sample.Unit is { } unit)
					Word(unit switch
					{
						SampleUnit.Percent => "PERCENT",
						SampleUnit.Rows    => "ROWS",
						_                  => throw NoText(unit),
					});

				Close();

				if (sample.Repeat is { } repeat)
				{
					Word("REPEATABLE");
					Open();
					PutExpression(repeat);
					Close();
				}
			}

			// T-SQL: Table Hints (Transact-SQL). After the alias and the sample, last of what a table says.
			if (table is not null)
				PutTableHints(table.Hints);
		}

		void PutPartition(SqlList<Expression>? partition)
		{
			if (partition is not { } list)
				return;

			Word("PARTITION BY");
			Open();
			Each(list, PutExpression);
			Close();
		}

		/// <summary>A column definition list, <c>(a int, b text)</c>: each column's name and type.</summary>
		void PutFieldDefinitions(SqlList<FieldDefinition> definitions)
		{
			Open();
			Each(definitions, definition =>
			{
				PutIdentifier(definition.Name);
				PutType(definition.Type);
			});
			Close();
		}

		void PutAlias(Alias? alias)
		{
			if (alias is null)
				return;

			if (alias.AsKeyword)
				Word("AS");

			PutIdentifier(alias.Name);

			if (alias.Columns is { } columns)
			{
				Open();
				Each(columns, PutIdentifier);
				Close();
			}
		}

		// ── §7.11 JSON table ───────────────────────────────────────────────────────

		void PutJsonTable(JsonTableDefinition table)
		{
			Word(table.Primitive ? "JSON_TABLE_PRIMITIVE" : "JSON_TABLE");
			Call();
			PutCommon(table.Common, standard: true);
			Word("COLUMNS");
			Open();
			Each(table.Columns, PutJsonColumn);
			Close();

			switch (table.Plan)
			{
				case null:
					break;

				case JsonTablePlan.Default choices:
					Word("PLAN DEFAULT");
					Open();

					var unionCross = choices.UnionCross is { } uc ? (uc switch { JsonTableDefaultUnionCross.Union => "UNION", JsonTableDefaultUnionCross.Cross => "CROSS", _ => throw NoText(uc) }) : null;
					var innerOuter = choices.InnerOuter is { } io ? (io switch { JsonTableDefaultInnerOuter.Inner => "INNER", JsonTableDefaultInnerOuter.Outer => "OUTER", _ => throw NoText(io) }) : null;
					var first      = choices.UnionCrossFirst || innerOuter is null ? unionCross : innerOuter;
					var second     = first == unionCross ? innerOuter : unionCross;

					Word(first!);

					if (second is not null)
					{
						Tight(",");
						Word(second);
					}

					Close();
					break;

				default:
					Word("PLAN");
					Open();
					PutPlan(table.Plan);
					Close();
					break;
			}

			if (table.OnError is { } error)
				Word(error switch { JsonTableErrorBehavior.Error => "ERROR ON ERROR", JsonTableErrorBehavior.Empty => "EMPTY ON ERROR", _ => throw NoText(error) });

			Close();
		}

		void PutPlan(JsonTablePlan plan)
		{
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, plan, static (into, node) => into.PutPlan(node));

				return;
			}

			switch (plan)
			{
				case JsonTablePlan.Name name:
					PutIdentifier(name.Value);
					break;

				case JsonTablePlan.Outer outer:
					PutIdentifier(outer.Parent);
					Word("OUTER");
					PutPlan(outer.Child);
					break;

				case JsonTablePlan.Inner inner:
					PutIdentifier(inner.Parent);
					Word("INNER");
					PutPlan(inner.Child);
					break;

				case JsonTablePlan.Union union:
					for (var at = 0; at < union.Items.Count; at++)
					{
						if (at > 0)
							Word("UNION");

						PutPlan(union.Items[at]);
					}

					break;

				case JsonTablePlan.Cross cross:
					for (var at = 0; at < cross.Items.Count; at++)
					{
						if (at > 0)
							Word("CROSS");

						PutPlan(cross.Items[at]);
					}

					break;

				case JsonTablePlan.Parenthesized parenthesized:
					Open();
					PutPlan(parenthesized.Value);
					Close();
					break;

				default:
					throw Unwritten(plan);
			}
		}

		void PutJsonColumn(JsonTableColumn column)
		{
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, column, static (into, node) => into.PutJsonColumn(node));

				return;
			}

			switch (column)
			{
				case JsonTableColumn.Ordinality ordinality:
					PutIdentifier(ordinality.Name);
					Word("FOR ORDINALITY");
					break;

				case JsonTableColumn.Chaining chaining:
					PutIdentifier(chaining.Name);
					Word("FOR CHAINING");
					break;

				case JsonTableColumn.Regular regular:
					PutIdentifier(regular.Name);
					PutType(regular.Type);
					PutColumnPath(regular.Path);

					if (regular.OnEmpty is { } regularOnEmpty)
					{
						PutValueBehavior(regularOnEmpty);
						Word("ON EMPTY");
					}

					if (regular.OnError is { } regularOnError)
					{
						PutValueBehavior(regularOnError);
						Word("ON ERROR");
					}

					break;

				case JsonTableColumn.Formatted formatted:
					PutIdentifier(formatted.Name);
					PutType(formatted.Type);

					if (formatted.Format is { } format)
					{
						Word("FORMAT");
						PutRepresentation(format);
					}

					PutColumnPath(formatted.Path);

					if (formatted.Wrapper is { } wrapper)
					{
						Word(Wrapper(wrapper));
						Word("WRAPPER");
					}

					PutQuotes(formatted.Quotes);

					if (formatted.OnEmpty is { } formattedOnEmpty)
					{
						Word(QueryBehavior(formattedOnEmpty));
						Word("ON EMPTY");
					}

					if (formatted.OnError is { } formattedOnError)
					{
						Word(QueryBehavior(formattedOnError));
						Word("ON ERROR");
					}

					break;

				default:
					var nested = (JsonTableColumn.Nested)column;

					Word(nested.PathKeyword ? "NESTED PATH" : "NESTED");
					Word(nested.Path);

					if (nested.Name is { } name)
					{
						Word("AS");
						PutIdentifier(name);
					}

					Word("COLUMNS");
					Open();
					Each(nested.Columns, PutJsonColumn);
					Close();
					break;
			}
		}

		void PutColumnPath(string? path)
		{
			if (path is null)
				return;

			Word("PATH");
			Word(path);
		}

		// ── §14 Data change statements ─────────────────────────────────────────────

		void PutStatement(Statement statement)
		{
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, statement, static (into, node) => into.PutStatement(node));

				return;
			}

			switch (statement)
			{
				case Statement.Select select:
					PutQuery(select.Query);
					PutQueryHints(select.Hints);

					break;

				// T-SQL: the statement's WITH first, its OUTPUT where the grammar reads it, its hints last.
				case Statement.Insert insert:
					PutStatementWith(insert.With);
					Word("INSERT");
					PutTop(insert.Top);
					PutIntoKeyword(insert.Into);

					// T-SQL: INSERT (Transact-SQL). `INSERT t2(a) VALUES (1)` reads as the table t2 and its column a.
					if (insert.Columns.Count == 0 && insert.Target is TableSource.Function { Invocation.Arguments: { Count: > 0 } arguments }
						&& arguments.All(static one => one is { Name: null, Value: Expression.Reference { Name.Count: 1 } }))
						throw Misplaced(insert, "Target", "that is a function whose arguments read as a column list, with no columns after it");

					// BNF: <insertion target> is a <table name>, with no ONLY; T-SQL reads `INSERT ONLY (t)` as a call.
					if (insert.Target is TableSource.Named { Only: true })
						throw Misplaced(insert, "Target", "with ONLY, which no INSERT reads");

					PutDmlTarget(insert.Target);

					// PostgreSQL's alias of the target, `INSERT INTO t AS a`.
					if (insert.Alias is { } insertAlias)
					{
						if (!postgreSql)
							throw Misplaced(insert, "Alias", "which only PostgreSQL reads on an INSERT's target");

						if (insertAlias.Columns is not null)
							throw Misplaced(insert, "Alias", "with column names, which an INSERT's alias does not take");

						PutAlias(insertAlias);
					}

					if (insert.Columns.Count > 0)
					{
						Open();
						Each(insert.Columns, PutColumnName);
						Close();
					}

					PutOverride(insert.Override);
					PutReturning(insert.Returning, atEnd: false);

					if (insert.SourceValue is InsertSource.DefaultValues)
						Word("DEFAULT VALUES");
					else if (insert.SourceValue is InsertSource.Values values)
					{
						Word("VALUES");
						Each(values.Rows, PutRow);
					}
					else if (insert.SourceValue is InsertSource.Subquery subquery)
						PutInsertQuery(subquery.Query);
					else if (!TryPutTransactSql(insert.SourceValue))
						throw Unwritten(insert.SourceValue);

					PutConflicts(insert);
					PutQueryHints(insert.Hints);
					PutReturning(insert.Returning, atEnd: true);
					break;

				case Statement.Update update:
					PutStatementWith(update.With);
					Word("UPDATE");
					PutTop(update.Top);
					PutDmlTarget(update.Target!);
					PutPortion(update.Portion);
					PutAlias(update.Alias);
					Word("SET");
					Each(update.Assignments, PutAssignment);
					PutReturning(update.Returning, atEnd: false);
					PutSecondFrom(update.From);
					PutWhereOrCurrent(update.Where, update.CurrentOf);
					PutQueryHints(update.Hints);
					PutReturning(update.Returning, atEnd: true);
					break;

				case Statement.Delete delete:
					PutStatementWith(delete.With);
					Word("DELETE");
					PutTop(delete.Top);

					if (!delete.FromOmitted)
						Word("FROM");

					PutDmlTarget(delete.Target!);
					PutPortion(delete.Portion);
					PutAlias(delete.Alias);
					PutReturning(delete.Returning, atEnd: false);
					PutSecondFrom(delete.From);

					// PostgreSQL's USING: the tables joined to the target for the condition.
					if (delete.Using is { } joined)
					{
						if (!postgreSql)
							throw Misplaced(delete, "Using", "which only PostgreSQL reads; T-SQL's second table list is From");

						if (joined.Sources.Count == 0)
							throw Empty(joined, "Sources");

						Word("USING");
						Each(joined.Sources, PutSource);
					}

					PutWhereOrCurrent(delete.Where, delete.CurrentOf);
					PutQueryHints(delete.Hints);
					PutReturning(delete.Returning, atEnd: true);
					break;

				case Statement.Merge merge:
					PutStatementWith(merge.With);
					Word("MERGE");
					PutTop(merge.Top);

					if (!merge.IntoOmitted)
						Word("INTO");

					PutDmlTarget(merge.Target);
					PutAlias(merge.Alias);
					Word("USING");
					PutSource(merge.SourceTable);
					Word("ON");
					PutCondition(merge, merge.On);

					foreach (var clause in merge.Clauses)
					{
						Word(clause switch
						{
							MergeClause.Matched                   => "WHEN MATCHED",
							MergeClause.NotMatched { ByTarget: true } => "WHEN NOT MATCHED BY TARGET",
							MergeClause.NotMatched                => "WHEN NOT MATCHED",
							MergeClause.NotMatchedBySource        => "WHEN NOT MATCHED BY SOURCE",
							_                                     => throw Unwritten(clause),
						});

						if (clause.Condition is { } condition)
						{
							Word("AND");
							PutCondition(clause, condition);
						}

						Word("THEN");

						switch (clause)
						{
							case MergeClause.Matched { Action: MergeMatchedAction.Update matchedUpdate }:
								Word("UPDATE SET");
								Each(matchedUpdate.Assignments, one => PutAssignment(one, merging: true));
								break;

							// PostgreSQL: MERGE. DO NOTHING after any WHEN.
							case MergeClause.Matched { Action: MergeMatchedAction.DoNothing } or MergeClause.NotMatchedBySource { Action: MergeMatchedAction.DoNothing }:
								if (!postgreSql)
									throw Misplaced(clause, "Action", "DoNothing, which only PostgreSQL reads after WHEN MATCHED");

								Word("DO NOTHING");
								break;

							case MergeClause.Matched:
								Word("DELETE");
								break;

							// T-SQL: MERGE (Transact-SQL). A matched clause's action.
							case MergeClause.NotMatchedBySource { Action: MergeMatchedAction.Update sourceUpdate }:
								Word("UPDATE SET");
								Each(sourceUpdate.Assignments, one => PutAssignment(one, merging: true));
								break;

							case MergeClause.NotMatchedBySource:
								Word("DELETE");
								break;

							default:
								var action = ((MergeClause.NotMatched)clause).Action;

								if (action.DoNothing)
								{
									if (!postgreSql)
										throw Misplaced(action, "DoNothing", "which only PostgreSQL reads after WHEN NOT MATCHED");

									if (action.Columns.Count > 0 || action.Values.Count > 0 || action.Override is not null || action.DefaultValues)
										throw Misplaced(action, "DoNothing", "with columns, values, an override or DefaultValues");

									Word("DO NOTHING");
									break;
								}

								Word("INSERT");

								// T-SQL: MERGE (Transact-SQL). A row of defaults, with nothing else.
								if (action.DefaultValues)
								{
									if (action.Columns.Count > 0 || action.Values.Count > 0 || action.Override is not null)
										throw Misplaced(action, "DefaultValues", "with columns, values or an override");

									Word("DEFAULT VALUES");
									break;
								}

								if (action.Columns.Count > 0)
								{
									Open();
									Each(action.Columns, PutColumnName);
									Close();
								}

								PutOverride(action.Override);
								Word("VALUES");
								Open();
								Each(action.Values, PutExpression);
								Close();
								break;
						}
					}

					PutReturning(merge.Returning, atEnd: false);
					PutQueryHints(merge.Hints);
					PutReturning(merge.Returning, atEnd: true);
					break;

				case Statement.TruncateTable truncate:
					if (truncate.Identity is not null && truncate.Partitions.Count > 0)
						throw Both(truncate, "Identity", "Partitions");

					// PostgreSQL's TRUNCATE: several tables, the word TABLE left out, CASCADE or RESTRICT.
					if (!postgreSql && (truncate.Others.Count > 0 || truncate.Behavior is not null || truncate.TableOmitted))
						throw Misplaced(truncate, "Others, Behavior or TableOmitted", "which only PostgreSQL reads on TRUNCATE");

					if (truncate.Partitions.Count > 0 && (truncate.Others.Count > 0 || truncate.Behavior is not null))
						throw Both(truncate, "Partitions", "Others or Behavior");

					Word(truncate.TableOmitted ? "TRUNCATE" : "TRUNCATE TABLE");
					PutSource(truncate.Target);

					foreach (var other in truncate.Others)
					{
						Tight(",");
						PutSource(other);
					}

					if (truncate.Identity is { } identity)
						Word(identity switch { IdentityRestart.Continue => "CONTINUE IDENTITY", IdentityRestart.Restart => "RESTART IDENTITY", _ => throw NoText(identity) });

					if (truncate.Behavior is { } behavior)
						Word(behavior switch { DropBehavior.Cascade => "CASCADE", DropBehavior.Restrict => "RESTRICT", _ => throw NoText(behavior) });

					PutPartitions(truncate.Partitions);

					break;

				default:
					PutOtherStatement(statement);
					break;
			}
		}

		/// <summary>PostgreSQL's ON CONFLICT clauses, after the rows: the target, and nothing or an update.</summary>
		void PutConflicts(Statement.Insert insert)
		{
			if (insert.Conflicts.Count == 0)
				return;

			if (!postgreSql)
				throw Misplaced(insert, "Conflicts", "which only PostgreSQL reads, ON CONFLICT");

			foreach (var conflict in insert.Conflicts)
			{
				Word("ON CONFLICT");

				switch (conflict.Target)
				{
					case null:
						break;

					case ConflictTarget.Columns columns:
						if (columns.Items.Count == 0)
							throw Empty(columns, "Items");

						Open();
						Each(columns.Items, PutExpression);
						Close();

						if (columns.Where is { } where)
						{
							Word("WHERE");
							PutExpression(where);
						}

						break;

					case ConflictTarget.Constraint constraint:
						Word("ON CONSTRAINT");
						PutIdentifier(constraint.Name);
						break;

					default:
						throw Unwritten(conflict.Target);
				}

				switch (conflict.Resolution)
				{
					case ConflictResolution.Nothing:
						Word("DO NOTHING");
						break;

					case ConflictResolution.Update update:
						if (update.Assignments.Count == 0)
							throw Empty(update, "Assignments");

						Word("DO UPDATE SET");
						Each(update.Assignments, PutAssignment);

						if (update.Where is { } where)
						{
							Word("WHERE");
							PutExpression(where);
						}

						break;

					default:
						throw Unwritten(conflict.Resolution);
				}
			}
		}

		void PutOverride(OverrideKind? kind)
		{
			if (kind is { } one)
				Word(one switch { OverrideKind.UserValue => "OVERRIDING USER VALUE", OverrideKind.SystemValue => "OVERRIDING SYSTEM VALUE", _ => throw NoText(one) });
		}

		void PutPortion(PeriodPortion? portion)
		{
			if (portion is null)
				return;

			Word("FOR PORTION OF");
			PutIdentifier(portion.Name);
			Word("FROM");
			PutExpression(portion.From);
			Word("TO");
			PutExpression(portion.To);
		}

		void PutWhereOrCurrent(Expression? where, CursorReference? current)
		{
			if (where is not null)
			{
				Word("WHERE");
				PutCondition(where, where);
			}

			if (current is not null)
			{
				Word("WHERE CURRENT OF");
				PutCursor(current);
			}
		}

		void PutAssignment(Assignment assignment)
		{
			PutAssignment(assignment, merging: false);
		}

		void PutAssignment(Assignment assignment, bool merging)
		{
			// T-SQL: UPDATE (Transact-SQL). A variable, a compound operator, a method called alone.
			if (PutTransactSqlAssignment(assignment, merging))
				return;

			if (assignment.Parenthesized)
			{
				Open();
				Each(assignment.Targets, PutAssignmentTarget);
				Close();
			}
			else
				PutAssignmentTarget(assignment.Targets[0]);

			Word("=");
			PutExpression(assignment.Value);
		}

		void PutAssignmentTarget(AssignmentTarget target)
		{
			// T-SQL: UPDATE (Transact-SQL). One of the engine's own columns stands alone there and carries no prefix (WrittenColumn).
			PutName(target.Name, pseudoFrom: target.Name.Count == 1 ? 0 : int.MaxValue);

			if (target.Index is { } index)
			{
				Tight(target.Trigraphs ? "??(" : "[");
				Hold();
				PutExpression(index);
				Tight(target.Trigraphs ? "??)" : "]");
			}

			foreach (var attribute in target.MutationPath ?? [])
			{
				Tight(".");
				Hold();
				PutIdentifier(attribute);
			}
		}

		void PutCursor(CursorReference cursor)
		{
			if (cursor.Global)
				Word("GLOBAL");

			if (cursor.Local)
				Word("LOCAL");

			if (cursor.Ptf)
				Word("PTF");

			if (cursor.Name is { } name)
				PutOnePartName(name, "a cursor's name (CursorName)");
			else
				PutExpression(cursor.ExtendedName!);
		}
	}
}
