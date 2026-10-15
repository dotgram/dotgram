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
				// T-SQL: MATCH (SQL Graph). The patterns, AND between them.
				case Expression.GraphMatch match:
					if (match.Patterns.Count == 0)
						throw Empty(match, "Patterns");

					Word("MATCH");
					Open();

					for (var at = 0; at < match.Patterns.Count; at++)
					{
						if (at > 0)
							Word("AND");

						PutGraphPattern(match.Patterns[at]);
					}

					Close();
					return true;

				// T-SQL: CHANGETABLE (Transact-SQL) (no syntax block in syntax.md).
				case TableSource.ChangeTableChanges changes:
					Word("CHANGETABLE");
					Call();
					Word("CHANGES");
					PutName(changes.Table);
					Tight(",");
					PutExpression(changes.LastSyncVersion);
					PutForceSeek(changes.ForceSeek);
					Close();
					PutAlias(changes.Alias);
					return true;

				case TableSource.ChangeTableVersion version:
					if (version.Columns.Count == 0)
						throw Empty(version, "Columns");

					if (version.Values.Count == 0)
						throw Empty(version, "Values");

					Word("CHANGETABLE");
					Call();
					Word("VERSION");
					PutName(version.Table);
					Tight(",");
					Open();
					Each(version.Columns, PutIdentifier);
					Close();
					Tight(",");
					Open();
					Each(version.Values, PutExpression);
					Close();
					PutForceSeek(version.ForceSeek);
					Close();
					PutAlias(version.Alias);
					return true;

				// T-SQL: CONTAINSTABLE (Transact-SQL), FREETEXTTABLE (Transact-SQL) (no syntax block in syntax.md).
				case TableSource.ContainsTable contains:
					PutFullTextTable(contains, "CONTAINSTABLE", contains.Table, contains.Columns, contains.ColumnsBracketed, contains.Search, contains.Language, contains.Top);
					PutAlias(contains.Alias);
					return true;

				case TableSource.FreetextTable freetext:
					PutFullTextTable(freetext, "FREETEXTTABLE", freetext.Table, freetext.Columns, freetext.ColumnsBracketed, freetext.Search, freetext.Language, freetext.Top);
					PutAlias(freetext.Alias);
					return true;

				// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL).
				case TableSource.Pivot pivot:
					PutRotated(pivot, pivot.Source, pivot.Values, "Values", pivot.Alias);
					Word("PIVOT");
					Open();
					PutExpression(pivot.Aggregate);
					Word("FOR");
					PutName(pivot.Column);
					Word("IN");
					Open();
					Each(pivot.Values, PutName);
					Close();
					Close();
					PutAlias(pivot.Alias);
					return true;

				case TableSource.Unpivot unpivot:
					PutRotated(unpivot, unpivot.Source, unpivot.Columns, "Columns", unpivot.Alias);
					Word("UNPIVOT");
					Open();
					PutName(unpivot.Value);
					Word("FOR");
					PutName(unpivot.Column);
					Word("IN");
					Open();
					Each(unpivot.Columns, PutName);
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

				// T-SQL: OPENROWSET (Transact-SQL). The three parts of a login are apart by semicolons.
				case TableSource.OpenRowset rowset:
					if (rowset.ProviderString is not null && rowset.Login is not null)
						throw Both(rowset, "ProviderString", "Login");

					// The provider and the connection are strings written out, which the grammar reads and nothing else.
					foreach (var (part, member) in new[] { (rowset.Provider, "Provider"), (rowset.ProviderString, "ProviderString"), (rowset.Login?.DataSource, "Login"), (rowset.Login?.User, "Login"), (rowset.Login?.Password, "Login") })
						if (part is not null && !StringLiteral(part))
							throw Misplaced(rowset, member, "that is not one string literal");

					Word("OPENROWSET");
					Call();
					PutExpression(rowset.Provider);
					Tight(",");

					if (rowset.Login is { } login)
					{
						PutExpression(login.DataSource);
						Tight(";");
						PutExpression(login.User);
						Tight(";");
						PutExpression(login.Password);
					}
					else
						PutExpression(rowset.ProviderString ?? throw Empty(rowset, "ProviderString or Login"));

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
						Tight(",");
						PutBulkOption(option, unique: true);
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

				// T-SQL: the ODBC outer join escape { oj … } (no syntax block in syntax.md).
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
		/// The source a PIVOT or an UNPIVOT turns, bracketed where it is a join with no ON — a CROSS JOIN, an
		/// APPLY — which would otherwise give the rotation its right side alone (after an ON the rotation turns the
		/// whole join, as the grammar reads it); and the refusals the grammar's requirements make: an alias, without
		/// columns, and at least one name in the list. As for expressions and set operations, the writer adds the
		/// brackets the meaning needs where the tree has no Parenthesized; the text reads back as Parenthesized,
		/// which a parser-built tree already carries there, so nothing is refused.
		/// </summary>
		void PutRotated(TableSource rotation, TableSource source, SqlList<QualifiedName> names, string member, Alias alias)
		{
			if (names.Count == 0)
				throw Empty(rotation, member);

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

		void PutForceSeek(bool forceSeek)
		{
			if (forceSeek)
			{
				Tight(",");
				Word("FORCESEEK");
			}
		}

		/// <summary>
		/// A full-text rowset: the table, the columns searched as the grammar reads them — a bare name, a list of
		/// them, <c>*</c> or a property, alone or in brackets — then the search, the language and the count.
		/// </summary>
		void PutFullTextTable(TableSource node, string name, QualifiedName table, SqlList<Expression> columns, bool bracketed, Expression search, Expression? language, Expression? top)
		{
			if (columns.Count == 0)
				throw Empty(node, "Columns");

			if (columns.Count > 1 && !bracketed)
				throw Misplaced(node, "ColumnsBracketed", "false with more than one column");

			foreach (var column in columns)
			{
				var read = column switch
				{
					Expression.Reference { Name.Count: 1 } => true,
					Expression.Asterisk                          => columns.Count == 1,
					Expression.Invocation { Name: [var called] } when called.Text.Equals("PROPERTY", StringComparison.OrdinalIgnoreCase) => !bracketed,
					_                                            => false,
				};

				if (!read)
					throw Misplaced(node, "Columns", "with a column other than a name of one part, * alone, or PROPERTY (…) alone and unbracketed");
			}

			Word(name);
			Call();
			PutName(table);
			Tight(",");

			if (bracketed)
				Open();

			Each(columns, PutExpression);

			if (bracketed)
				Close();

			Tight(",");
			PutExpression(search);

			if (language is not null)
			{
				Tight(",");
				Word("LANGUAGE");
				PutExpression(language);
			}

			if (top is not null)
			{
				Tight(",");
				PutExpression(top);
			}

			Close();
		}

		/// <summary>
		/// T-SQL: MATCH (SQL Graph). One pattern: a chain, the last nodes of two paths compared, or a shortest path
		/// with its anchor at the end it was written at. A drawing's arrows are written tight against what they join.
		/// </summary>
		void PutGraphPattern(GraphPattern pattern)
		{
			switch (pattern)
			{
				case GraphPattern.Chain chain:
					if (chain.Steps.Count == 0)
						throw Empty(chain, "Steps");

					PutGraphNode(chain.Start);

					foreach (var step in chain.Steps)
					{
						PutGraphEdge(step.Edge);
						PutGraphNode(step.Node);
					}

					break;

				case GraphPattern.LastNodes last:
					PutGraphNode(new GraphNode(last.Left, true));
					Word("=");
					PutGraphNode(new GraphNode(last.Right, true));
					break;

				case GraphPattern.ShortestPath path:
					if (path.Steps.Count == 0)
						throw Empty(path, "Steps");

					if (path.Steps.Any(static one => one.Node.LastNode))
						throw Misplaced(path, "Steps", "with a node that is LAST_NODE, which SHORTEST_PATH reads only as its anchor");

					Word("SHORTEST_PATH");
					Open();

					if (!path.AnchorLast)
						PutGraphNode(path.Anchor);

					Open();

					foreach (var step in path.Steps)
					{
						if (path.AnchorLast)
						{
							PutGraphNode(step.Node);
							PutGraphEdge(step.Edge);
						}
						else
						{
							PutGraphEdge(step.Edge);
							PutGraphNode(step.Node);
						}
					}

					Close();
					PutGraphQuantifier(path.Quantifier);

					if (path.AnchorLast)
						PutGraphNode(path.Anchor);

					Close();
					break;

				default:
					throw Unwritten(pattern);
			}
		}

		void PutGraphNode(GraphNode node)
		{
			if (node.LastNode)
			{
				Word("LAST_NODE");
				Open();
				PutIdentifier(node.Alias);
				Close();
			}
			else
				PutIdentifier(node.Alias);
		}

		void PutGraphEdge(GraphEdge edge)
		{
			switch (edge.Direction)
			{
				case GraphDirection.Right:
					Tight("-(");
					Hold();
					PutIdentifier(edge.Alias);
					Tight(")->");
					Hold();
					break;

				case GraphDirection.Left:
					Tight("<-(");
					Hold();
					PutIdentifier(edge.Alias);
					Tight(")-");
					Hold();
					break;

				default:
					throw NoText(edge.Direction);
			}
		}

		void PutGraphQuantifier(GraphQuantifier quantifier)
		{
			if (quantifier.Min < 0 || quantifier.Max < quantifier.Min)
				throw Misplaced(quantifier, "Min or Max", "out of order, or below nought");

			if (quantifier.Plus)
			{
				if (quantifier is not { Min: 1, Max: null })
					throw Misplaced(quantifier, "Plus", "with bounds other than {1,}");

				Tight("+");
				return;
			}

			Tight("{" + quantifier.Min.ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
				+ quantifier.Max?.ToString(System.Globalization.CultureInfo.InvariantCulture) + "}");
		}

		/// <summary>
		/// T-SQL: FOR Clause (Transact-SQL). The mode, the element name XML's modes take, then each directive
		/// in the order written.
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

					if (xml.ElementName is { } element)
					{
						if (!StringLiteral(element))
							throw Misplaced(xml, "ElementName", "that is not one string literal");

						Open();
						PutExpression(element);
						Close();
					}

					PutDirectives(xml.Directives, json: false);
					break;

				case ForClause.Json json:
					Word("FOR JSON");
					Word(json.Mode switch
					{
						JsonMode.Auto => "AUTO",
						JsonMode.Path => "PATH",
						_             => throw NoText(json.Mode),
					});
					PutDirectives(json.Directives, json: true);
					break;

				case ForClause.Browse:
					Word("FOR BROWSE");
					break;

				default:
					throw Unwritten(clause);
			}
		}

		/// <summary>
		/// The directives of <c>FOR XML</c> or <c>FOR JSON</c>, refusing a kind the other reads, a name where the
		/// kind takes none, and <c>ABSENT</c> on any kind but <c>ELEMENTS</c>.
		/// </summary>
		void PutDirectives(SqlList<ForDirective> directives, bool json)
		{
			foreach (var directive in directives)
			{
				var (text, xmlOnly, jsonOnly) = directive.Kind switch
				{
					ForDirectiveKind.BinaryBase64        => ("BINARY BASE64", true, false),
					ForDirectiveKind.Type                => ("TYPE", true, false),
					ForDirectiveKind.Root                => ("ROOT", false, false),
					ForDirectiveKind.XmlData             => ("XMLDATA", true, false),
					ForDirectiveKind.XmlSchema           => ("XMLSCHEMA", true, false),
					ForDirectiveKind.Elements            => ("ELEMENTS", true, false),
					ForDirectiveKind.ElementsXsiNil      => ("ELEMENTS XSINIL", true, false),
					ForDirectiveKind.IncludeNullValues   => ("INCLUDE_NULL_VALUES", false, true),
					ForDirectiveKind.WithoutArrayWrapper => ("WITHOUT_ARRAY_WRAPPER", false, true),
					_                                    => throw NoText(directive.Kind),
				};

				if (json ? xmlOnly : jsonOnly)
					throw Misplaced(directive, "Kind", json ? "an XML directive under FOR JSON" : "a JSON directive under FOR XML");

				if (directive.Name is not null && directive.Kind is not (ForDirectiveKind.Root or ForDirectiveKind.XmlSchema))
					throw Misplaced(directive, "Name", "on a directive other than ROOT or XMLSCHEMA");

				if (directive.Absent && directive.Kind != ForDirectiveKind.Elements)
					throw Misplaced(directive, "Absent", "on a directive other than ELEMENTS");

				Tight(",");
				Word(text);

				if (directive.Absent)
					Word("ABSENT");

				if (directive.Name is { } name)
				{
					if (!StringLiteral(name))
						throw Misplaced(directive, "Name", "that is not one string literal");

					Open();
					PutExpression(name);
					Close();
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
			Each(columns, column => PutTypedColumn(column, typed: true, "a column of PREDICT"));
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
		/// <summary>The words a query hint record of several words starts with, which no named hint may be.</summary>
		static readonly System.Collections.Generic.HashSet<string> PhraseWords = new(StringComparer.OrdinalIgnoreCase)
		{
			"HASH", "ORDER", "CONCAT", "MERGE", "KEEP", "LOOP", "EXPAND", "FORCE", "DISABLE", "KEEPFIXED", "ROBUST", "USE", "OPTIMIZE", "TABLE",
		};

		/// <summary>Whether a value is one string literal, all that the grammar reads in some places.</summary>
		static bool StringLiteral(Expression value)
		{
			return value is LiteralValue.String;
		}

		void PutQueryHint(QueryHint hint)
		{
			switch (hint)
			{
				// A query hint's value is written bare or after `=`; T-SQL reads none in brackets.
				case QueryHint.Named named:
					if (named.Arguments.Count > 0 && named.Form == HintArgumentForm.Brackets)
						throw Misplaced(named, "Form", "Brackets with arguments on a query hint, which T-SQL does not read");

					// A name another hint record starts with reads back as that record, and a name of several words not at all.
					if (named.Name.Style != IdentifierStyle.Regular || PhraseWords.Contains(named.Name.Text))
						throw Misplaced(named, "Name", "that is not a regular identifier, or is a word another hint record starts with");

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

					if (!use.Hints.All(StringLiteral))
						throw Misplaced(use, "Hints", "with a hint that is not one string literal");

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
					if (named.Arguments.Count > 0 && named.Name.Text.Equals("FORCESEEK", StringComparison.OrdinalIgnoreCase))
						throw Misplaced(named, "Arguments", "on FORCESEEK, which is TableHint.ForceSeek with them");

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

			PutOptionWord(name);

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
