using System;

namespace DotGram.Sql.Ast;

public static partial class SqlWriter
{
	sealed partial class Text
	{
		/// <summary>
		/// PostgreSQL's own statements (SqlAst.PostgreSql.Statements.cs), each refused for any other reading, which has no text for
		/// it. Answers false for every other node.
		/// </summary>
		bool TryPutPostgreSql(Statement statement)
		{
			switch (statement)
			{
				case Statement.DropObject drop:
					PutDropObject(drop);
					return true;

				case Statement.Comment comment:
					PutComment(comment);
					return true;

				case Statement.PrepareTransaction prepare:
					OnlyPostgreSql(prepare);
					Word("PREPARE TRANSACTION");
					PutLiteral(prepare.Id);
					return true;

				case Statement.CommitPrepared commit:
					OnlyPostgreSql(commit);
					Word("COMMIT PREPARED");
					PutLiteral(commit.Id);
					return true;

				case Statement.RollbackPrepared rollback:
					OnlyPostgreSql(rollback);
					Word("ROLLBACK PREPARED");
					PutLiteral(rollback.Id);
					return true;

				case Statement.SetTransactionSnapshot snapshot:
					OnlyPostgreSql(snapshot);
					PutSet(snapshot, snapshot.Scope);
					Word("TRANSACTION SNAPSHOT");
					PutLiteral(snapshot.Snapshot);
					return true;

				case Statement.SetParameter set:
					OnlyPostgreSql(set);
					PutSet(set, set.Scope);

					// SET XML OPTION DOCUMENT: the statement's own words for xmloption and its two values.
					if (set.Spelling is { } spelling)
					{
						if (spelling != ParameterSpelling.XmlOption)
							throw Misplaced(set, nameof(set.Spelling), "other than XML OPTION, the one spelling SET has of its own");

						if (set.FromCurrent || set.EqualsSign || !ParameterIs(set.Name, spelling)
							|| set.Values is not [Expression.Reference { Name: [{ Style: IdentifierStyle.Regular, Text: var word }] }]
							|| !(word.Equals("document", StringComparison.OrdinalIgnoreCase) || word.Equals("content", StringComparison.OrdinalIgnoreCase)))
							throw Misplaced(set, nameof(set.Spelling), "XML OPTION with other than the parameter xmloption set TO document or content");

						Word("XML OPTION");
						Word(word);
						return true;
					}

					PutName(set.Name);

					if (set.FromCurrent)
					{
						if (set.Values.Count > 0 || set.EqualsSign)
							throw Misplaced(set, nameof(set.FromCurrent), "with values or an equals sign, where FROM CURRENT stands alone");

						Word("FROM CURRENT");
						return true;
					}

					if (set.Values.Count == 0)
						throw Empty(set, nameof(set.Values));

					Word(set.EqualsSign ? "=" : "TO");

					if (set.Values is [Expression.Default])
						Word("DEFAULT");
					else
						Each(set.Values, PutParameterValue);

					return true;

				case Statement.ShowParameter show:
					OnlyPostgreSql(show);
					Word("SHOW");
					PutParameterRef(show, show.Name, show.Spelling);
					return true;

				case Statement.ResetParameter reset:
					OnlyPostgreSql(reset);
					Word("RESET");
					PutParameterRef(reset, reset.Name, reset.Spelling);
					return true;

				case Statement.Explain explain:
					OnlyPostgreSql(explain);
					Word("EXPLAIN");

					if (explain.Bare)
					{
						// The old words bare before the statement: ANALYZE, then VERBOSE, each at most once and in that order.
						if (explain.Options.Count is 0 or > 2)
							throw Misplaced(explain, nameof(explain.Bare), "with no option to write bare, or more than ANALYZE and VERBOSE");

						for (var at = 0; at < explain.Options.Count; at++)
						{
							var option  = explain.Options[at];
							var analyze = IsWord(option.Name, "analyze") || IsWord(option.Name, "analyse");
							var verbose = IsWord(option.Name, "verbose");

							// ANALYZE only first; VERBOSE first alone, or second after ANALYZE.
							if (option.Value is not null || option.Options.Count > 0 || !(analyze || verbose) || (analyze && at > 0) || (verbose && at == 0 && explain.Options.Count > 1))
								throw Misplaced(option, nameof(option.Name), "other than ANALYZE then VERBOSE, each alone and once, which are the words EXPLAIN takes bare");

							Word(option.Name.Text);
						}
					}
					else if (explain.Options.Count > 0)
					{
						Open();
						Each(explain.Options, PutUtilityOption);
						Close();
					}

					PutExplained(explain, explain.Statement);
					return true;

				case Statement.Do @do:
					OnlyPostgreSql(@do);
					Word("DO");

					if (@do.LanguageFirst)
					{
						if (@do.Language is null)
							throw Misplaced(@do, nameof(@do.LanguageFirst), "with no language to put first");

						Word("LANGUAGE");
						PutLanguage(@do, @do.Language);
						PutLiteral(@do.Body);
					}
					else
					{
						PutLiteral(@do.Body);

						if (@do.Language is { } language)
						{
							Word("LANGUAGE");
							PutLanguage(@do, language);
						}
					}

					return true;

				case Statement.PreparePlan prepare:
					OnlyPostgreSql(prepare);
					Word("PREPARE");
					PutIdentifier(prepare.Name);

					if (prepare.ParameterTypes.Count > 0)
					{
						Open();
						Each(prepare.ParameterTypes, PutType);
						Close();
					}

					if (prepare.Body is not (Statement.Select or Statement.Insert or Statement.Update or Statement.Delete or Statement.Merge))
						throw Misplaced(prepare, nameof(prepare.Body), "other than a query or a data change, which alone PREPARE takes");

					Word("AS");
					PutStatement(prepare.Body);
					return true;

				case Statement.DeallocateAll deallocate:
					OnlyPostgreSql(deallocate);
					Word(deallocate.PrepareKeyword ? "DEALLOCATE PREPARE ALL" : "DEALLOCATE ALL");
					return true;

				case Statement.Listen listen:
					OnlyPostgreSql(listen);
					Word("LISTEN");
					PutIdentifier(listen.Channel);
					return true;

				case Statement.Unlisten unlisten:
					OnlyPostgreSql(unlisten);
					Word("UNLISTEN");

					if (unlisten.Channel is { } channel)
						PutIdentifier(channel);
					else
						Word("*");

					return true;

				case Statement.Notify notify:
					OnlyPostgreSql(notify);
					Word("NOTIFY");
					PutIdentifier(notify.Channel);

					if (notify.Payload is { } payload)
					{
						Tight(",");
						PutLiteral(payload);
					}

					return true;

				case Statement.LockTable @lock:
					OnlyPostgreSql(@lock);

					if (@lock.Tables.Count == 0)
						throw Empty(@lock, nameof(@lock.Tables));

					Word(@lock.TableOmitted ? "LOCK" : "LOCK TABLE");
					Each(@lock.Tables, PutLockedTable);

					if (@lock.Mode is { } mode)
					{
						Word("IN");
						Word(mode switch
						{
							LockMode.AccessShare          => "ACCESS SHARE",
							LockMode.RowShare             => "ROW SHARE",
							LockMode.RowExclusive         => "ROW EXCLUSIVE",
							LockMode.ShareUpdateExclusive => "SHARE UPDATE EXCLUSIVE",
							LockMode.Share                => "SHARE",
							LockMode.ShareRowExclusive    => "SHARE ROW EXCLUSIVE",
							LockMode.Exclusive            => "EXCLUSIVE",
							LockMode.AccessExclusive      => "ACCESS EXCLUSIVE",
							_                             => throw NoText(mode),
						});
						Word("MODE");
					}

					if (@lock.NoWait)
						Word("NOWAIT");

					return true;

				// PostgreSQL: DECLARE. The standard's record in PostgreSQL's words: the options before CURSOR, WITH or
				// WITHOUT HOLD after it, the query after FOR. Another reading's cursor is the T-SQL arm's.
				case Statement.DeclareCursor declare when postgreSql:
					if (declare.Cursor is not { Name: { Count: 1 } name, ExtendedName: null, Global: false, Local: false, Ptf: false })
						throw Misplaced(declare, nameof(declare.Cursor), "other than a name of one part, which alone PostgreSQL's DECLARE takes");

					if (declare.Extended)
						throw Misplaced(declare, nameof(declare.Extended), "the words after CURSOR, which are T-SQL's");

					var properties = declare.Properties;

					if (properties is { Returnability: not null } or { Sensitivity: CursorSensitivity.Sensitive } or { Scope: not null } or { Type: not null } or { Concurrency: not null } or { TypeWarning: true } or { ForwardOnly: true })
						throw Misplaced(declare, nameof(declare.Properties), "a word PostgreSQL's DECLARE does not read: WITH RETURN, SENSITIVE, or T-SQL's");

					if (declare.SourceValue is not CursorSource.Query { Updatability: null, UpdatabilityBeforeOptions: false, Select.Hints.Count: 0 } source)
						throw Misplaced(declare, nameof(declare.SourceValue), "other than a query with no FOR READ ONLY or hints, which PostgreSQL reads as the query's own clauses");

					Word("DECLARE");
					PutName(name);

					if (properties.Binary)
						Word("BINARY");

					if (properties.Sensitivity is { } sensitivity)
						Word(sensitivity == CursorSensitivity.Insensitive ? "INSENSITIVE" : "ASENSITIVE");

					if (properties.Scrollability is { } scrollability)
						Word(scrollability == CursorScrollability.Scroll ? "SCROLL" : "NO SCROLL");

					Word("CURSOR");

					if (properties.Holdability is { } holdability)
						Word(holdability == CursorHoldability.WithHold ? "WITH HOLD" : "WITHOUT HOLD");

					Word("FOR");
					PutQuery(source.Select.Query);
					return true;

				case Statement.MoveCursor move:
					OnlyPostgreSql(move);
					Word("MOVE");
					PutFetchArgs(move, move.Orientation, move.FromKeyword, move.InKeyword, move.Cursor);
					return true;

				case Statement.CloseAllCursors close:
					OnlyPostgreSql(close);
					Word("CLOSE ALL");
					return true;

				case Statement.Vacuum vacuum:
					OnlyPostgreSql(vacuum);
					Word("VACUUM");

					if (vacuum.Bare)
					{
						// The old words in their order: FULL, FREEZE, VERBOSE, ANALYZE.
						var rank = -1;

						foreach (var option in vacuum.Options)
						{
							var next = option.Value is null && option.Options.Count == 0
								? IsWord(option.Name, "full") ? 0 : IsWord(option.Name, "freeze") ? 1 : IsWord(option.Name, "verbose") ? 2 : IsWord(option.Name, "analyze") || IsWord(option.Name, "analyse") ? 3 : -1
								: -1;

							if (next <= rank)
								throw Misplaced(option, nameof(option.Name), "other than FULL, FREEZE, VERBOSE and ANALYZE alone, in that order, which are the words VACUUM takes bare");

							rank = next;
							Word(option.Name.Text);
						}
					}
					else if (vacuum.Options.Count > 0)
					{
						Open();
						Each(vacuum.Options, PutUtilityOption);
						Close();
					}

					Each(vacuum.Relations, PutVacuumRelation);
					return true;

				case Statement.Analyze analyze:
					OnlyPostgreSql(analyze);
					Word(analyze.British ? "ANALYSE" : "ANALYZE");

					if (analyze.Bare)
					{
						foreach (var option in analyze.Options)
						{
							if (option.Value is not null || option.Options.Count > 0 || !IsWord(option.Name, "verbose"))
								throw Misplaced(option, nameof(option.Name), "other than VERBOSE alone, which is the word ANALYZE takes bare");

							Word(option.Name.Text);
						}

						if (analyze.Options.Count > 1)
							throw Misplaced(analyze, nameof(analyze.Options), "more than the one word ANALYZE takes bare");
					}
					else if (analyze.Options.Count > 0)
					{
						Open();
						Each(analyze.Options, PutUtilityOption);
						Close();
					}

					Each(analyze.Relations, PutVacuumRelation);
					return true;

				case Statement.Copy copy:
					OnlyPostgreSql(copy);
					PutCopy(copy);
					return true;

				case Statement.Discard discard:
					OnlyPostgreSql(discard);
					Word("DISCARD");

					if (discard.Temporary && discard.Target != DiscardTarget.Temp)
						throw Misplaced(discard, nameof(discard.Temporary), "on other than TEMP, the one target with a long word");

					Word(discard.Target switch
					{
						DiscardTarget.All       => "ALL",
						DiscardTarget.Plans     => "PLANS",
						DiscardTarget.Sequences => "SEQUENCES",
						DiscardTarget.Temp      => discard.Temporary ? "TEMPORARY" : "TEMP",
						_                       => throw NoText(discard.Target),
					});
					return true;

				default:
					return false;
			}
		}

		static bool IsWord(Identifier identifier, string word)
		{
			return identifier.Style == IdentifierStyle.Regular && identifier.Text.Equals(word, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>Whether a parameter's name is the one a spelling's words stand for.</summary>
		static bool ParameterIs(QualifiedName name, ParameterSpelling spelling)
		{
			return name is [{ } one] && IsWord(one, PostgreSql.Nodes.ParameterOf(spelling));
		}

		/// <summary>A parameter SHOW or RESET names: ALL where the name is null, the statement's own words where the spelling says, the name otherwise.</summary>
		void PutParameterRef(ISqlNode statement, QualifiedName? name, ParameterSpelling? spelling)
		{
			if (spelling is { } spelled)
			{
				if (name is null || !ParameterIs(name.Value, spelled))
					throw Misplaced(statement, "Spelling", "whose words name another parameter than the name does");

				Word(spelled switch
				{
					ParameterSpelling.TimeZone                  => "TIME ZONE",
					ParameterSpelling.TransactionIsolationLevel => "TRANSACTION ISOLATION LEVEL",
					ParameterSpelling.SessionAuthorization      => "SESSION AUTHORIZATION",
					ParameterSpelling.XmlOption                 => throw Misplaced(statement, "Spelling", "XML OPTION, which only SET has words for"),
					_                                           => throw NoText(spelled),
				});

				return;
			}

			if (name is { } named)
				PutName(named);
			else
				Word("ALL");
		}

		/// <summary>A value a SET or an EXPLAIN option takes: a word, a string, a number under its sign, or a boolean (var_value).</summary>
		void PutParameterValue(Expression value)
		{
			switch (value)
			{
				case Expression.Reference { Name.Count: 1 }:
				case LiteralValue.String:
				case LiteralValue.Numeric:
				case LiteralValue.Boolean:
				case Expression.Unary { Operator: UnaryOperator.Minus or UnaryOperator.Plus, Operand: LiteralValue.Numeric }:
					PutExpression(value);
					break;

				default:
					throw Misplaced(value, "Kind", "other than a word, a string, a number or a boolean, which are the values a parameter takes");
			}
		}

		/// <summary>One option of EXPLAIN in brackets: the name, and the value where one was written (utility_option_elem).</summary>
		void PutUtilityOption(Option option)
		{
			if (option.Options.Count > 0)
				throw Misplaced(option, nameof(option.Options), "under an EXPLAIN option, which takes a value alone");

			PutIdentifier(option.Name);

			if (option.Value is { } value)
				PutParameterValue(value);
		}

		/// <summary>The statement EXPLAIN explains: a query, a data change, an EXECUTE or a cursor's declaration.</summary>
		void PutExplained(Statement.Explain explain, Statement explained)
		{
			if (!PostgreSql.Nodes.Explainable(explained))
				throw Misplaced(explain, nameof(explain.Statement), "other than a query, a data change, an EXECUTE or a DECLARE, which alone EXPLAIN explains");

			PutStatement(explained);
		}

		/// <summary>DO's language: a word, or a string.</summary>
		void PutLanguage(Statement.Do @do, Expression language)
		{
			if (language is not (Expression.Reference { Name.Count: 1 } or LiteralValue.String))
				throw Misplaced(@do, nameof(@do.Language), "other than a word or a string");

			PutExpression(language);
		}

		/// <summary>A table LOCK names: a name, ONLY and the star as a relation_expr has them, nothing else.</summary>
		void PutLockedTable(TableSource table)
		{
			if (table is not TableSource.Named { Alias: null, Sample: null } named)
				throw Misplaced(table, "Kind", "other than a table's name, which alone LOCK takes");

			if (named.Name.Count > 3)
				throw Misplaced(named, "Name", "of more than three parts, which PostgreSQL refuses (improper qualified name)");

			PutSource(named);
		}

		/// <summary>A relation VACUUM or ANALYZE names, and its columns in brackets where any were.</summary>
		void PutVacuumRelation(VacuumRelation relation)
		{
			PutLockedTable(relation.Relation);

			if (relation.Columns.Count > 0)
			{
				Open();
				Each(relation.Columns, PutIdentifier);
				Close();
			}
		}

		/// <summary>COPY: the table and its columns or the query, the direction, the file or the stream, the options, WHERE.</summary>
		void PutCopy(Statement.Copy copy)
		{
			Word("COPY");

			// BINARY before the table and DELIMITERS after the file are two of the options in old clauses of their own:
			// each is written there and left out of the list, and must be in the list to be written at all.
			var binaryAt    = copy.BinaryBeforeTable ? IndexOfOption(copy.Options, "format", static value => value is Expression.Reference { Name: [{ } word] } && IsWord(word, "binary")) : -1;
			var delimiterAt = copy.DelimitersClause ? IndexOfOption(copy.Options, "delimiter", static value => value is LiteralValue.String) : -1;

			if (copy.BinaryBeforeTable && (binaryAt < 0 || copy.Query is not null))
				throw Misplaced(copy, nameof(copy.BinaryBeforeTable), "with no option format binary to stand for, or before a query, which takes no BINARY");

			if (copy.DelimitersClause && (delimiterAt < 0 || copy.Query is not null))
				throw Misplaced(copy, nameof(copy.DelimitersClause), "with no option delimiter to stand for, or after a query, which takes no DELIMITERS");

			if (copy.Query is { } query)
			{
				if (copy.Table is not null || copy.Columns.Count > 0 || copy.Where is not null || copy.Direction != CopyDirection.To)
					throw Misplaced(copy, nameof(copy.Query), "beside a table, columns or a WHERE, or with FROM, where a query goes TO alone");

				Open();
				PutQuery(query);
				Close();
			}
			else
			{
				if (copy.Table is null)
					throw Empty(copy, nameof(copy.Table), nameof(copy.Query));

				if (copy.BinaryBeforeTable)
					Word("BINARY");

				PutLockedTable(copy.Table);

				if (copy.Columns.Count > 0)
				{
					Open();
					Each(copy.Columns, PutIdentifier);
					Close();
				}
			}

			Word(copy.Direction switch { CopyDirection.From => "FROM", CopyDirection.To => "TO", _ => throw NoText(copy.Direction) });

			if (copy.Program)
				Word("PROGRAM");

			if (copy.File is { } file)
			{
				if (copy.Stream is not null)
					throw Both(copy, nameof(copy.File), nameof(copy.Stream));

				PutLiteral(file);
			}
			else
			{
				if (copy.Program)
					throw Misplaced(copy, nameof(copy.Program), "with no command line, where a stream stands");

				Word(copy.Stream switch { StandardStream.Stdin => "STDIN", StandardStream.Stdout => "STDOUT", null => throw Empty(copy, nameof(copy.File), nameof(copy.Stream)), _ => throw NoText(copy.Stream.Value) });
			}

			if (delimiterAt >= 0)
			{
				Word("DELIMITERS");
				PutLiteral((LiteralValue)copy.Options[delimiterAt].Value!);
			}

			if (copy.WithKeyword)
				Word("WITH");

			var listed = new List<Option>(copy.Options.Count);

			for (var at = 0; at < copy.Options.Count; at++)
			{
				if (at != binaryAt && at != delimiterAt)
					listed.Add(copy.Options[at]);
			}

			if (copy.Bare)
			{
				if (listed.Count == 0)
					throw Misplaced(copy, nameof(copy.Bare), "with no option to write as an old word");

				foreach (var option in listed)
					PutLegacyCopyOption(option);
			}
			else if (listed.Count > 0)
			{
				Open();

				for (var at = 0; at < listed.Count; at++)
				{
					if (at > 0)
						Tight(",");

					PutCopyOption(listed[at]);
				}

				Close();
			}

			if (copy.Where is { } where)
			{
				if (copy.Direction != CopyDirection.From)
					throw Misplaced(copy, nameof(copy.Where), "with TO, where the server's grammar refuses a WHERE");

				Word("WHERE");
				PutExpression(where);
			}
		}

		/// <summary>The first option of a name whose value fits, or -1.</summary>
		static int IndexOfOption(SqlList<Option> options, string name, Func<Expression?, bool> fits)
		{
			for (var at = 0; at < options.Count; at++)
			{
				if (IsWord(options[at].Name, name) && options[at].Options.Count == 0 && fits(options[at].Value))
					return at;
			}

			return -1;
		}

		/// <summary>One COPY option in brackets: the name, and the value, the star, DEFAULT or the list where one was written.</summary>
		void PutCopyOption(Option option)
		{
			if (option.Options.Count > 0)
				throw Misplaced(option, nameof(option.Options), "under a COPY option, which takes a value alone");

			PutIdentifier(option.Name);

			switch (option.Value)
			{
				case null:
					break;

				case Expression.Asterisk:
					Word("*");
					break;

				case Expression.Default:
					Word("DEFAULT");
					break;

				case Expression.Row { RowKeyword: false } row:
					Open();
					Each(row.Items, PutParameterValue);
					Close();
					break;

				default:
					PutParameterValue(option.Value);
					break;
			}
		}

		/// <summary>One COPY option as the old word it has: BINARY, CSV, HEADER, FREEZE, DELIMITER 'x' and the rest; an option with no old word is refused.</summary>
		void PutLegacyCopyOption(Option option)
		{
			if (option.Options.Count > 0)
				throw Misplaced(option, nameof(option.Options), "under a COPY option, which takes a value alone");

			var name = option.Name.Style == IdentifierStyle.Regular ? option.Name.Text.ToLowerInvariant() : "";

			switch (name)
			{
				case "format" when option.Value is Expression.Reference { Name: [{ } format] } && (IsWord(format, "binary") || IsWord(format, "csv")):
					Word(format.Text.ToUpperInvariant());
					return;

				case "header" or "freeze" when option.Value is LiteralValue.Boolean { Value: BooleanLiteral.True }:
					Word(name.ToUpperInvariant());
					return;

				case "delimiter" or "null" or "quote" or "escape" or "encoding" when option.Value is LiteralValue.String text:
					Word(name.ToUpperInvariant());
					PutLiteral(text);
					return;

				case "force_quote" or "force_not_null" or "force_null" when option.Value is Expression.Asterisk or Expression.Row { RowKeyword: false }:
					Word(name switch { "force_quote" => "FORCE QUOTE", "force_not_null" => "FORCE NOT NULL", "force_null" => "FORCE NULL", _ => throw Misplaced(option, nameof(option.Name), "no old word") });

					if (option.Value is Expression.Row row)
						Each(row.Items, PutParameterValue);
					else
						Word("*");

					return;

				default:
					throw Misplaced(option, nameof(option.Name), "an option COPY has no old word for; it is written in brackets");
			}
		}
	}
}
