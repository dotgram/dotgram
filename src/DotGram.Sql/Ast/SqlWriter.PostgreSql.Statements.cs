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
						// The old words, ANALYZE and VERBOSE, bare before the statement: no value, and those words alone.
						if (explain.Options.Count == 0)
							throw Misplaced(explain, nameof(explain.Bare), "with no option to write bare");

						foreach (var option in explain.Options)
						{
							if (option.Value is not null || option.Options.Count > 0 || !(IsWord(option.Name, "analyze") || IsWord(option.Name, "analyse") || IsWord(option.Name, "verbose")))
								throw Misplaced(option, nameof(option.Name), "other than ANALYZE or VERBOSE alone, which are the words EXPLAIN takes bare");

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

			PutSource(named);
		}
	}
}
