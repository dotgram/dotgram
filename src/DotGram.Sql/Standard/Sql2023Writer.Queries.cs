using System;
using System.Collections.Generic;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		// ── §7.13 Query expression ─────────────────────────────────────────────────

		/// <summary>A subquery: its brackets are its own, and the query in them has none.</summary>
		void PutSubquery(Statement.Select query)
		{
			Open();
			PutQuery(query);
			Close();
		}

		// The brackets counted around a query hold its body, its set operations and its clauses; what a
		// cursor specification says of it after them stands outside.
		void PutQuery(Statement.Select query)
		{
			for (var at = 0; at < query.Parentheses; at++)
				Open();

			if (query.With is { } with)
				PutWith(with);

			if (query.Body is { } body)
				PutOperand(body);
			else
				PutSpecification(query);

			foreach (var operation in query.SetOperations)
			{
				Word(operation.Operator switch { SetOperator.Union => "UNION", SetOperator.Except => "EXCEPT", _ => "INTERSECT" });
				PutQuantifier(operation.Quantifier);

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

				PutOperand(operation.Operand);
			}

			if (query.OrderBy is { } order)
			{
				Word("ORDER BY");
				PutSorts(order);
			}

			if (query.Offset is { } offset)
			{
				Word("OFFSET");
				PutExpression(offset.Count);
				Word(offset.RowWord == RowWord.Row ? "ROW" : "ROWS");
			}

			if (query.Fetch is { } fetch)
			{
				Word(fetch.Position == FetchPosition.First ? "FETCH FIRST" : "FETCH NEXT");

				if (fetch.Quantity is { } quantity)
					PutExpression(quantity);

				if (fetch.Percent)
					Word("PERCENT");

				Word(fetch.RowWord == RowWord.Row ? "ROW" : "ROWS");
				Word(fetch.Mode == FetchMode.Only ? "ONLY" : "WITH TIES");
			}

			for (var at = 0; at < query.Parentheses; at++)
				Close();

			if (query.Updatability is { } updatability)
			{
				if (updatability.ReadOnly)
					Word("FOR READ ONLY");
				else
				{
					Word("FOR UPDATE");

					if (updatability.UpdateColumns.Count > 0)
					{
						Word("OF");
						Each(updatability.UpdateColumns, PutIdentifier);
					}
				}
			}
		}

		void PutSpecification(Statement.Select query)
		{
			Word("SELECT");
			PutQuantifier(query.Quantifier);
			Each(query.Items, PutItem);

			if (query.Into is { } into)
			{
				Word("INTO");
				Each(into.Targets, PutExpression);
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
				PutQuantifier(group.Quantifier);
				Each(group.Items, PutGrouping);
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

		void PutOperand(QueryOperand operand)
		{
			switch (operand)
			{
				case QueryOperand.Select select:
					PutQuery(select.Query);
					break;

				case QueryOperand.Values values:
					Word("VALUES");
					Each(values.Rows, PutRow);
					break;

				default:
					Word("TABLE");
					PutName(((QueryOperand.Table)operand).Name);
					break;
			}
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

		void PutItem(SelectItem item)
		{
			switch (item)
			{
				case SelectItem.All:
					Word("*");
					break;

				case SelectItem.ExpressionItem expression:
					PutExpression(expression.Expression);

					if (expression.Alias is { } alias)
					{
						if (expression.AsKeyword)
							Word("AS");

						PutIdentifier(alias);
					}

					break;

				default:
					var all = (SelectItem.QualifiedAll)item;

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

		void PutSorts(OrderByClause order) =>
			Each(order.Items, item =>
			{
				PutExpression(item.Key);

				if (item.Direction is { } direction)
					Word(direction == SortDirection.Asc ? "ASC" : "DESC");

				if (item.NullOrdering is { } nulls)
					Word(nulls == NullOrdering.First ? "NULLS FIRST" : "NULLS LAST");
			});

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

				Word(frame.Unit switch { WindowFrameUnit.Rows => "ROWS", WindowFrameUnit.Range => "RANGE", _ => "GROUPS" });

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
						_                               => "EXCLUDE NO OTHERS",
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

		void PutMeasures(IReadOnlyList<RowPatternMeasure> measures)
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
					_                             => "ALL ROWS PER MATCH WITH UNMATCHED ROWS",
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
			switch (source)
			{
				case TableSource.Named named:
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

							default:
								var range = (SystemTimeSpecification.FromTo)time;

								Word("FROM");
								PutExpression(range.From);
								Word("TO");
								PutExpression(range.To);
								break;
						}
					}

					PutAlias(named.Alias);
					break;

				case TableSource.Subquery subquery:
					if (subquery.Lateral)
						Word("LATERAL");

					PutSubquery(subquery.Query);
					PutAlias(subquery.Alias);
					break;

				case TableSource.Join join:
					PutSource(join.Left);
					PutPartition(join.LeftPartition);

					if (join.Kind == JoinKind.Cross)
						Word("CROSS JOIN");
					else
					{
						if (join.Natural)
							Word("NATURAL");

						if (join.Kind is { } kind)
							Word(kind switch { JoinKind.Inner => "INNER", JoinKind.Left => "LEFT", JoinKind.Right => "RIGHT", _ => "FULL" });

						if (join.OuterKeyword)
							Word("OUTER");

						Word("JOIN");
					}

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
					Word(change.Option switch { ResultOption.Final => "FINAL", ResultOption.New => "NEW", _ => "OLD" });
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
					throw Unwritten(source);
			}

			// A sample clause follows a table primary and all it was given.
			if (source.Sample is { } sample)
			{
				Word(sample.Method == SampleMethod.Bernoulli ? "TABLESAMPLE BERNOULLI" : "TABLESAMPLE SYSTEM");
				Open();
				PutExpression(sample.Percentage);
				Close();

				if (sample.Repeat is { } repeat)
				{
					Word("REPEATABLE");
					Open();
					PutExpression(repeat);
					Close();
				}
			}
		}

		void PutPartition(IReadOnlyList<Expression>? partition)
		{
			if (partition is null)
				return;

			Word("PARTITION BY");
			Open();
			Each(partition, PutExpression);
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
			switch (statement)
			{
				case Statement.Select select:
					PutQuery(select);
					break;

				case Statement.Insert insert:
					Word("INSERT INTO");
					PutName(insert.Target);

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
						PutQuery(((InsertSource.Query)insert.SourceValue).Select);

					break;

				case Statement.Update update:
					Word("UPDATE");
					PutTarget(update.Target!);
					PutPortion(update.Portion);
					PutAlias(update.Alias);
					Word("SET");
					Each(update.Assignments, PutAssignment);
					PutWhereOrCurrent(update.Where, update.CurrentOf);
					break;

				case Statement.Delete delete:
					Word("DELETE FROM");
					PutTarget(delete.Target!);
					PutPortion(delete.Portion);
					PutAlias(delete.Alias);
					PutWhereOrCurrent(delete.Where, delete.CurrentOf);
					break;

				case Statement.Merge merge:
					Word("MERGE INTO");
					PutTarget(merge.Target);
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
					PutTarget(truncate.Target);

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

		void PutTarget(TableTarget target)
		{
			if (target.Only)
			{
				Word("ONLY");
				Open();
				PutName(target.Name);
				Close();
			}
			else
				PutName(target.Name);
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
