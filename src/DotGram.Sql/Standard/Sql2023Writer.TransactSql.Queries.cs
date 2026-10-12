using System;
using System.Linq;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Queries family: writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutQueries(ISqlNode node)
		{
			switch (node)
			{
				// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL).
				case TableSource.Pivot pivot:
					PutRotated(pivot, pivot.Source, pivot.In, pivot.Alias);
					Word("PIVOT");
					Open();
					PutExpression(pivot.Aggregate);
					Word("FOR");
					PutIdentifier(pivot.For);
					Word("IN");
					Open();
					Each(pivot.In, PutIdentifier);
					Close();
					Close();
					PutAlias(pivot.Alias);
					return true;

				case TableSource.Unpivot unpivot:
					PutRotated(unpivot, unpivot.Source, unpivot.In, unpivot.Alias);
					Word("UNPIVOT");
					Open();
					PutIdentifier(unpivot.Value);
					Word("FOR");
					PutIdentifier(unpivot.For);
					Word("IN");
					Open();
					Each(unpivot.In, PutIdentifier);
					Close();
					Close();
					PutAlias(unpivot.Alias);
					return true;

				// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL). In a FROM the engine takes neither hints nor a
				// sample on a table variable (Msg 319, Msg 156).
				case TableSource.Variable variable:
					if (variable.Hints.Count > 0 || variable.Sample is not null)
						throw Misplaced(variable, "Hints or Sample", "on a table variable in a FROM");

					if (variable.Alias?.Columns is not null)
						throw Misplaced(variable, "Alias.Columns", "on a table variable");

					PutVariableName(variable, variable.Name);
					PutAlias(variable.Alias);
					return true;

				// T-SQL: OPENJSON (Transact-SQL).
				case TableSource.OpenJson json:
					Word("OPENJSON");
					Call();
					PutExpression(json.Json);

					if (json.Path is { } path)
					{
						Tight(",");
						PutExpression(path);
					}

					Close();
					PutSchema(json.Schema);
					PutAlias(json.Alias);
					return true;

				// T-SQL: OPENXML (Transact-SQL).
				case TableSource.OpenXml xml:
					Word("OPENXML");
					Call();
					PutExpression(xml.Document);
					Tight(",");
					PutExpression(xml.RowPattern);

					if (xml.Flags is { } flags)
					{
						Tight(",");
						PutExpression(flags);
					}

					Close();

					if (xml.SchemaTable is { } table)
					{
						if (xml.Schema is not null)
							throw Both(xml, "Schema", "SchemaTable");

						Word("WITH");
						PutName(table);
					}
					else
						PutSchema(xml.Schema);

					PutAlias(xml.Alias);
					return true;

				// T-SQL: OPENQUERY (Transact-SQL).
				case TableSource.OpenQuery query:
					Word("OPENQUERY");
					Call();
					PutIdentifier(query.Server);
					Tight(",");
					PutExpression(query.Query);
					Close();
					PutAlias(query.Alias);
					return true;

				// T-SQL: OPENROWSET (Transact-SQL). The three parts of a connection are apart by semicolons.
				case TableSource.OpenRowset rowset:
					if (rowset.Connection.Count is not (1 or 3))
						throw Misplaced(rowset, "Connection", "with other than a provider string or a data source, a user and a password");

					Word("OPENROWSET");
					Call();
					PutExpression(rowset.Provider);
					Tight(",");

					for (var at = 0; at < rowset.Connection.Count; at++)
					{
						if (at > 0)
							Tight(";");

						PutExpression(rowset.Connection[at]);
					}

					Tight(",");
					PutExpression(rowset.Object);
					Close();
					PutAlias(rowset.Alias);
					return true;

				// T-SQL: OPENROWSET BULK (Transact-SQL). An option is a word, or a word and its value.
				case TableSource.OpenRowsetBulk bulk:
					Word("OPENROWSET");
					Call();
					Word("BULK");
					PutExpression(bulk.File);

					foreach (var option in bulk.Options)
					{
						if (option.Options.Count > 0)
							throw Misplaced(option, "Options", "on an option of OPENROWSET BULK");

						Tight(",");
						PutIdentifier(option.Name);

						// T-SQL: OPENROWSET BULK (Transact-SQL). `ORDER (…) [UNIQUE]`, under the option's name.
						if (option.Order is { } order)
						{
							if (option.Value is not null)
								throw Both(option, "Order", "Value");

							Open();
							PutSorts(new OrderByClause(order.Columns));
							Close();

							if (order.Unique)
								Word("UNIQUE");
						}
						else if (option.Value is { } value)
						{
							Word("=");
							PutExpression(value);
						}
					}

					Close();
					PutSchema(bulk.Schema);
					PutAlias(bulk.Alias);
					return true;

				// T-SQL: PREDICT (Transact-SQL).
				case TableSource.Predict predict:
					Word("PREDICT");
					Call();
					Word("MODEL =");
					PutExpression(predict.Model);
					Tight(",");
					Word("DATA =");
					PutSource(predict.Data);

					if (predict.Runtime is { } runtime)
					{
						Tight(",");
						Word("RUNTIME =");
						PutIdentifier(runtime);
					}

					Close();
					PutPredictSchema(predict.Schema);
					PutAlias(predict.Alias);
					return true;

				// T-SQL: ODBC Scalar Functions (Transact-SQL).
				case TableSource.OdbcJoin odbc:
					if (odbc.Source is TableSource.Pivot or TableSource.Unpivot)
						throw Misplaced(odbc, "Source", "that is a PIVOT or an UNPIVOT, which an ODBC escape does not hold");

					Word("{");
					Hold();
					Word("OJ");
					PutSource(odbc.Source);
					Tight("}");
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// The source a PIVOT or an UNPIVOT turns, bracketed where it is a join, which would otherwise give the
		/// rotation its right side alone; and the refusals the grammar's requirements make: an alias, without
		/// columns, and at least one name in the list.
		/// </summary>
		void PutRotated(TableSource rotation, TableSource source, SqlList<Identifier> names, Alias alias)
		{
			if (names.Count == 0)
				throw Empty(rotation, "In");

			if (alias is null || alias.Columns is not null)
				throw Misplaced(rotation, "Alias", "missing or with columns, where the grammar takes a name alone");

			if (source is TableSource.Join { Specification: null })
			{
				Open();
				PutSource(source);
				Close();
			}
			else
				PutSource(source);
		}

		/// <summary>
		/// T-SQL: FOR Clause (Transact-SQL). The mode, then each option: a name alone (<c>TYPE</c>), a name
		/// and its value in brackets (<c>ROOT ('r')</c>), or a name and the one word it takes
		/// (<c>ELEMENTS XSINIL</c>, <c>BINARY BASE64</c>), which is an option of its own inside it.
		/// </summary>
		void PutFor(ForClause clause)
		{
			switch (clause)
			{
				case ForClause.Xml xml:
					Word("FOR XML");
					Word(xml.Mode switch
					{
						XmlMode.Raw      => "RAW",
						XmlMode.Auto     => "AUTO",
						XmlMode.Explicit => "EXPLICIT",
						XmlMode.Path     => "PATH",
						_                => throw NoText(xml.Mode),
					});

					// The element a row is written as, RAW's and PATH's only.
					if (xml.ElementName is { } element)
					{
						if (xml.Mode is not (XmlMode.Raw or XmlMode.Path))
							throw Misplaced(xml, "ElementName", "with a mode other than RAW or PATH");

						Open();
						PutExpression(element);
						Close();
					}

					PutForOptions(xml.Options);
					break;

				case ForClause.Json json:
					Word("FOR JSON");
					Word(json.Mode switch
					{
						JsonMode.Auto => "AUTO",
						JsonMode.Path => "PATH",
						_             => throw NoText(json.Mode),
					});
					PutForOptions(json.Options);
					break;

				case ForClause.Browse:
					Word("FOR BROWSE");
					break;

				default:
					throw Unwritten(clause);
			}
		}

		void PutForOptions(SqlList<Option> options)
		{
			foreach (var option in options)
			{
				if (option.Order is not null)
					throw Misplaced(option, "Order", "on an option of a FOR clause");

				Tight(",");
				PutIdentifier(option.Name);

				if (option.Value is { } value)
				{
					if (option.Options.Count > 0)
						throw Both(option, "Value", "Options");

					Open();
					PutExpression(value);
					Close();
				}

				foreach (var word in option.Options)
				{
					if (word.Value is not null || word.Options.Count > 0)
						throw Misplaced(word, "Value or Options", "on the word a FOR clause's option takes");

					PutIdentifier(word.Name);
				}
			}
		}

		/// <summary>
		/// T-SQL: OPENJSON, OPENXML, OPENROWSET BULK (Transact-SQL). A rowset function's <c>WITH (…)</c>: each
		/// column's name and type, then its path or its ordinal, then <c>AS JSON</c>.
		/// </summary>
		void PutSchema(SqlList<RowsetColumn>? schema)
		{
			if (schema is not { } columns)
				return;

			if (columns.Count == 0)
				throw Empty(columns, "Schema");

			Word("WITH");
			Open();
			Each(columns, column =>
			{
				if (column.Type is null)
					throw Empty(column, "Type");

				if (column.Path is not null && column.Ordinal is not null)
					throw Both(column, "Path", "Ordinal");

				PutIdentifier(column.Name);
				PutType(column.Type);

				if (column.Collation is { } collation)
				{
					Word("COLLATE");
					PutCollation(collation);
				}

				// A path is one string literal and nothing else: it is written as it is.
				if (column.Path is { } path)
				{
					if (!OneString(path))
						throw Misplaced(column, "Path", "that is not one string literal");

					Word(path);
				}

				if (column.Ordinal is { } ordinal)
					Number(ordinal);

				if (column.AsJson)
					Word("AS JSON");
			});
			Close();
		}

		/// <summary>
		/// Whether a text is one string literal and nothing more: <c>'$.a'</c>, <c>N'x'</c>, a quote inside it doubled.
		/// </summary>
		static bool OneString(string text)
		{
			var at = text.Length > 0 && text[0] is 'N' or 'n' ? 1 : 0;

			if (text.Length < at + 2 || text[at] != '\'' || text[text.Length - 1] != '\'')
				return false;

			// A quote inside is one of a doubled pair, and the pair stops short of the closing quote.
			for (var i = at + 1; i < text.Length - 1; i++)
			{
				if (text[i] != '\'')
					continue;

				if (i + 1 >= text.Length - 1 || text[i + 1] != '\'')
					return false;

				i++;
			}

			return true;
		}

		/// <summary>
		/// T-SQL: PREDICT (Transact-SQL). Its <c>WITH (…)</c>: each column's name and type, its collation and its
		/// <c>NOT NULL</c>, and nothing else a column definition may hold.
		/// </summary>
		void PutPredictSchema(SqlList<ColumnDefinition> columns)
		{
			if (columns.Count == 0)
				throw Empty(columns, "Schema");

			Word("WITH");
			Open();
			Each(columns, column =>
			{
				if (column.Type is null)
					throw Empty(column, "Type");

				if (column.Generation is not null || column.Constraints.Any(static one => one is not Constraint.NotNull { Name: null, Characteristics: null }) || column.Constraints.Count > 1)
					throw Misplaced(column, "Generation or Constraints", "on a column of PREDICT, which takes NOT NULL alone");

				PutIdentifier(column.Name);
				PutType(column.Type);

				if (column.Collation is { } collation)
				{
					Word("COLLATE");
					PutCollation(collation);
				}

				if (column.Constraints.Count > 0)
					Word("NOT NULL");
			});
			Close();
		}

		/// <summary>T-SQL: OPTION clause (Transact-SQL). <c>OPTION (RECOMPILE, MAXDOP 2)</c>.</summary>
		void PutQueryHints(SqlList<QueryHint> hints)
		{
			if (hints.Count == 0)
				return;

			Word("OPTION");
			Open();
			Each(hints, PutQueryHint);
			Close();
		}

		/// <summary>T-SQL: Query Hints (Transact-SQL).</summary>
		void PutQueryHint(QueryHint hint)
		{
			switch (hint)
			{
				// A query hint's value is written bare or after `=`; T-SQL reads none in brackets.
				case QueryHint.Named named:
					if (named.Arguments.Count > 0 && named.Form == HintArgumentForm.Brackets)
						throw Misplaced(named, "Form", "Brackets with arguments on a query hint, which T-SQL does not read");

					PutHint(named, named.Name, named.Arguments, named.Form);
					break;

				case QueryHint.Phrase phrase:
					Word(phrase.Words switch
					{
						QueryHintPhrase.HashGroup                => "HASH GROUP",
						QueryHintPhrase.OrderGroup               => "ORDER GROUP",
						QueryHintPhrase.ConcatUnion              => "CONCAT UNION",
						QueryHintPhrase.HashUnion                => "HASH UNION",
						QueryHintPhrase.MergeUnion               => "MERGE UNION",
						QueryHintPhrase.KeepUnion                => "KEEP UNION",
						QueryHintPhrase.LoopJoin                 => "LOOP JOIN",
						QueryHintPhrase.MergeJoin                => "MERGE JOIN",
						QueryHintPhrase.HashJoin                 => "HASH JOIN",
						QueryHintPhrase.ExpandViews              => "EXPAND VIEWS",
						QueryHintPhrase.ForceOrder               => "FORCE ORDER",
						QueryHintPhrase.ForceScaleOutExecution   => "FORCE SCALEOUTEXECUTION",
						QueryHintPhrase.DisableScaleOutExecution => "DISABLE SCALEOUTEXECUTION",
						QueryHintPhrase.KeepPlan                 => "KEEP PLAN",
						QueryHintPhrase.KeepFixedPlan            => "KEEPFIXED PLAN",
						QueryHintPhrase.RobustPlan               => "ROBUST PLAN",
						_                                        => throw NoText(phrase.Words),
					});
					break;

				case QueryHint.UseHint use:
					if (use.Hints.Count == 0)
						throw Empty(use, "Hints");

					Word("USE HINT");
					Open();
					Each(use.Hints, PutExpression);
					Close();
					break;

				case QueryHint.UsePlan plan:
					Word("USE PLAN");
					PutExpression(plan.Plan);
					break;

				case QueryHint.OptimizeFor optimize:
					if (optimize.Variables.Count == 0)
						throw Empty(optimize, "Variables");

					Word("OPTIMIZE FOR");
					Open();
					Each(optimize.Variables, variable =>
					{
						if (variable.Value is not null && variable.Unknown)
							throw Both(variable, "Value", "Unknown");

						PutVariableName(variable, variable.Variable);

						if (variable.Value is { } value)
						{
							Word("=");
							PutExpression(value);
						}
						else if (variable.Unknown)
							Word("UNKNOWN");
					});
					Close();
					break;

				case QueryHint.OptimizeForUnknown:
					Word("OPTIMIZE FOR UNKNOWN");
					break;

				case QueryHint.ForTable table:
					Word("TABLE HINT");
					Open();
					PutName(table.Table);

					foreach (var one in table.Hints)
					{
						Tight(",");
						PutTableHint(one);
					}

					Close();
					break;

				default:
					throw Unwritten(hint);
			}
		}

		/// <summary>T-SQL: Table Hints (Transact-SQL). <c>WITH (NOLOCK, INDEX (ix))</c>.</summary>
		void PutTableHints(SqlList<TableHint> hints)
		{
			if (hints.Count == 0)
				return;

			Word("WITH");
			Open();
			Each(hints, PutTableHint);
			Close();
		}

		void PutTableHint(TableHint hint)
		{
			switch (hint)
			{
				case TableHint.Named named:
					PutHint(named, named.Name, named.Arguments, named.Form);
					break;

				case TableHint.ForceSeek seek:
					if (seek.Columns.Count == 0)
						throw Empty(seek, "Columns");

					Word("FORCESEEK");
					Open();
					PutExpression(seek.Index);
					Open();
					Each(seek.Columns, PutIdentifier);
					Close();
					Close();
					break;

				default:
					throw Unwritten(hint);
			}
		}

		/// <summary>
		/// A hint's name and its values: in brackets, after it, or after <c>=</c>. The last two take one value
		/// exactly; a hint with none is its name alone.
		/// </summary>
		void PutHint(ISqlNode hint, Identifier name, SqlList<Expression> arguments, HintArgumentForm form)
		{
			if (form != HintArgumentForm.Brackets && arguments.Count != 1)
				throw Misplaced(hint, "Form", $"{form} with other than one argument");

			PutIdentifier(name);

			if (arguments.Count == 0)
				return;

			switch (form)
			{
				case HintArgumentForm.Brackets:
					Open();
					Each(arguments, PutExpression);
					Close();
					break;

				case HintArgumentForm.Bare:
					PutExpression(arguments[0]);
					break;

				case HintArgumentForm.Assigned:
					Word("=");
					PutExpression(arguments[0]);
					break;

				default:
					throw NoText(form);
			}
		}

		/// <summary>A variable's name, which is a regular identifier starting with <c>@</c>, or is refused.</summary>
		void PutVariableName(ISqlNode node, Identifier name)
		{
			if (name.Style != IdentifierStyle.Regular || !name.Text.StartsWith("@", StringComparison.Ordinal))
				throw Misplaced(node, "Name", "that is not a regular name starting with @");

			PutIdentifier(name);
		}

		/// <summary>T-SQL: WITH XMLNAMESPACES (Transact-SQL).</summary>
		void PutXmlNamespaces(SqlList<XmlNamespace> namespaces)
		{
			Word("XMLNAMESPACES");
			Open();
			Each(namespaces, one =>
			{
				if (one.Prefix is { } prefix)
				{
					PutExpression(one.Uri);
					Word("AS");
					PutIdentifier(prefix);
				}
				else
				{
					Word("DEFAULT");
					PutExpression(one.Uri);
				}
			});
			Close();
		}
	}
}
