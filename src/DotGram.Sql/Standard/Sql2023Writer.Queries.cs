using System;
using System.Collections.Generic;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
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

			if (query is Query.Parenthesized parenthesized)
			{
				if (parenthesized.Query.With is not null)
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
					Word("OF");
					Each(locking.Of, PutIdentifier);
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
			var fetched = limit.Count is not null || limit.FetchRowWord is not null;

			if (!fetched && limit.Offset is null)
				throw Empty(limit, "Offset", "Count", "FetchRowWord");

			if (!fetched && (limit.WithTies || limit.Percent || limit.Position != FetchPosition.First))
				throw Misplaced(limit, "WithTies, Percent or Position", "without a FETCH, which alone writes them");

			if (limit.Offset is null && limit.OffsetRowWord is not null)
				throw Misplaced(limit, "OffsetRowWord", "without an Offset, which alone writes it");

			if (limit.Offset is { } offset)
			{
				Word("OFFSET");
				PutExpression(offset);
				Word(limit.OffsetRowWord == RowWord.Row ? "ROW" : "ROWS");
			}

			if (!fetched)
				return;

			Word(limit.Position == FetchPosition.First ? "FETCH FIRST" : "FETCH NEXT");

			if (limit.Count is { } count)
				PutExpression(count);

			if (limit.Percent)
				Word("PERCENT");

			Word(limit.FetchRowWord == RowWord.Row ? "ROW" : "ROWS");
			Word(limit.WithTies ? "WITH TIES" : "ONLY");
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

			if (query.Table && Explicit(query) is { } table)
			{
				Word("TABLE");
				PutName(table);

				return;
			}

			if (query.Columns.Count == 0)
				throw Empty(query, "Columns", "Values");

			Word("SELECT");
			PutQuantifier(query.Quantifier);
			PutTop(query.Top);
			Each(query.Columns, PutColumn);

			if (query.Into is { } into)
			{
				if (query.IntoTable is not null)
					throw Both(query, "Into", "IntoTable");

				Word("INTO");
				Each(into.Targets, PutExpression);
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
				PutExpression(where);
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
				PutExpression(having);
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
		static QualifiedName? Explicit(Query.Select query)
		{
			if (query is not { Columns: [SelectColumn.All], From.Sources: [TableSource.Named { Hints.Count: 0 } named], Top: null, Quantifier: null, Into: null, IntoTable: null, Where: null, GroupBy: null, Having: null, Window: null })
				return null;

			// A property pattern rather than record equality: equality probes the stack, and this runs at
			// the depth the caller last checked, so on a deep tree it could throw where the writer would
			// otherwise hand the walk to a fresh stack.
			return named.GetType() == typeof(TableSource.Named)
				&& named is { Sample: null, SystemTime: null, Alias: null, Only: false, DataSource: null, ForPath: false }
				? named.Name
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
			else if (row.Items.Count == 1)
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
					PutExpression(first.Expression);
					break;

				case SelectColumn.ExpressionItem expression:
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
					Word(assignment.Operator switch
					{
						AssignmentOperator.Assign      => "=",
						AssignmentOperator.Add         => "+=",
						AssignmentOperator.Subtract    => "-=",
						AssignmentOperator.Multiply    => "*=",
						AssignmentOperator.Divide      => "/=",
						AssignmentOperator.Modulo      => "%=",
						AssignmentOperator.BitwiseAnd  => "&=",
						AssignmentOperator.BitwiseOr   => "|=",
						AssignmentOperator.BitwiseXor  => "^=",
						AssignmentOperator.Concatenate => "||=",
						_                              => throw NoText(assignment.Operator),
					});
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
				PutSubquery(common.Query);

				if (common.Search is { } search)
				{
					Word(search.Order == SearchOrder.DepthFirst ? "SEARCH DEPTH FIRST BY" : "SEARCH BREADTH FIRST BY");
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
				case GroupingElement.Ordinary ordinary when ordinary.Parenthesized:
					Open();
					Each(ordinary.Expressions, PutExpression);
					Close();
					break;

				case GroupingElement.Ordinary ordinary:
					PutExpression(ordinary.Expressions[0]);
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

		void PutSorts(OrderByClause order)
		{
			Each(order.Items, item =>
			{
				PutExpression(item.Key);

				if (item.Direction is { } direction)
					Word(direction == SortDirection.Asc ? "ASC" : "DESC");

				if (item.NullOrdering is { } nulls)
					Word(nulls == NullOrdering.First ? "NULLS FIRST" : "NULLS LAST");
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

				default:
					Word("UNBOUNDED FOLLOWING");
					break;
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

					default:
						Word("SKIP TO");
						PutIdentifier(skip.Variable!);
						break;
				}
			}

			if (clause.Initial is { } initial)
				Word(initial == RowPatternInitial.Initial ? "INITIAL" : "SEEK");

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
				PutExpression(definition.Condition);
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

						default:
							Tight("{" + quantifier.Min + "," + quantifier.Max + "}");
							break;
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
					if (named.Name.Parts is [{ Style: IdentifierStyle.Regular } only] && only.Text.StartsWith("@", StringComparison.Ordinal))
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
									Word(symmetry == BetweenSymmetry.Asymmetric ? "ASYMMETRIC" : "SYMMETRIC");

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

					// An APPLY's right side that is a join is bracketed: written bare, the join would take the APPLY as
					// its left side.
					if (join.Kind is JoinKind.CrossApply or JoinKind.OuterApply && join.Right is TableSource.Join)
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
							PutExpression(on.Condition);
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
					Word("UNNEST");
					Call();
					Each(unnest.Expressions, PutExpression);
					Close();

					if (unnest.WithOrdinality)
						Word("WITH ORDINALITY");

					PutAlias(unnest.Alias);
					break;

				case TableSource.Function function:
					PutExpression(function.Invocation);
					PutAlias(function.Alias);
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

				case TableSource.DataChange change:
					Word(change.Option switch { ResultOption.Final => "FINAL", ResultOption.New => "NEW", ResultOption.Old => "OLD", _ => throw NoText(change.Option) });
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
				else
					Word(sample.Method == SampleMethod.Bernoulli ? "TABLESAMPLE BERNOULLI" : "TABLESAMPLE SYSTEM");

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
			PutCommon(table.Common);
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

					var unionCross = choices.UnionCross is { } uc ? (uc == JsonTableDefaultUnionCross.Union ? "UNION" : "CROSS") : null;
					var innerOuter = choices.InnerOuter is { } io ? (io == JsonTableDefaultInnerOuter.Inner ? "INNER" : "OUTER") : null;
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
				Word(error == JsonTableErrorBehavior.Error ? "ERROR ON ERROR" : "EMPTY ON ERROR");

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

				case Statement.Insert insert:
					Word("INSERT");
					PutTop(insert.Top);
					Word("INTO");
					PutSource(insert.Target);

					if (insert.SourceValue is InsertSource.DefaultValues)
					{
						Word("DEFAULT VALUES");
						break;
					}

					if (insert.Columns.Count > 0)
					{
						Open();
						Each(insert.Columns, PutIdentifier);
						Close();
					}

					PutOverride(insert.Override);

					if (insert.SourceValue is InsertSource.Values values)
					{
						Word("VALUES");
						Each(values.Rows, PutRow);
					}
					else
						PutQuery(((InsertSource.Subquery)insert.SourceValue).Query);

					break;

				case Statement.Update update:
					Word("UPDATE");
					PutTop(update.Top);
					PutSource(update.Target!);
					PutPortion(update.Portion);
					PutAlias(update.Alias);
					Word("SET");
					Each(update.Assignments, PutAssignment);
					PutWhereOrCurrent(update.Where, update.CurrentOf);
					break;

				case Statement.Delete delete:
					Word("DELETE");
					PutTop(delete.Top);
					Word("FROM");
					PutSource(delete.Target!);
					PutPortion(delete.Portion);
					PutAlias(delete.Alias);
					PutWhereOrCurrent(delete.Where, delete.CurrentOf);
					break;

				case Statement.Merge merge:
					Word("MERGE");
					PutTop(merge.Top);
					Word("INTO");
					PutSource(merge.Target);
					PutAlias(merge.Alias);
					Word("USING");
					PutSource(merge.SourceTable);
					Word("ON");
					PutExpression(merge.On);

					foreach (var clause in merge.Clauses)
					{
						Word(clause is MergeClause.Matched ? "WHEN MATCHED" : "WHEN NOT MATCHED");

						if (clause.Condition is { } condition)
						{
							Word("AND");
							PutExpression(condition);
						}

						Word("THEN");

						switch (clause)
						{
							case MergeClause.Matched { Action: MergeMatchedAction.Update matchedUpdate }:
								Word("UPDATE SET");
								Each(matchedUpdate.Assignments, PutAssignment);
								break;

							case MergeClause.Matched:
								Word("DELETE");
								break;

							default:
								var action = ((MergeClause.NotMatched)clause).Action;

								Word("INSERT");

								if (action.Columns.Count > 0)
								{
									Open();
									Each(action.Columns, PutIdentifier);
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

					break;

				case Statement.TruncateTable truncate:
					Word("TRUNCATE TABLE");
					PutSource(truncate.Target);

					if (truncate.Identity is { } identity)
						Word(identity == IdentityRestart.Continue ? "CONTINUE IDENTITY" : "RESTART IDENTITY");

					break;

				default:
					PutOtherStatement(statement);
					break;
			}
		}

		void PutOverride(OverrideKind? kind)
		{
			if (kind is { } one)
				Word(one == OverrideKind.UserValue ? "OVERRIDING USER VALUE" : "OVERRIDING SYSTEM VALUE");
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
				PutExpression(where);
			}

			if (current is not null)
			{
				Word("WHERE CURRENT OF");
				PutCursor(current);
			}
		}

		void PutAssignment(Assignment assignment)
		{
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
			PutName(target.Name);

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
				PutName(name);
			else
				PutExpression(cursor.ExtendedName!);
		}
	}
}
