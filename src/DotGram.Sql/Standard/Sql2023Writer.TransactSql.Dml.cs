using System;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Dml family: writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutDml(ISqlNode node)
		{
			return false;
		}

		/// <summary>
		/// T-SQL: WITH common_table_expression (Transact-SQL). What a data change statement writes before itself.
		/// </summary>
		void PutStatementWith(WithClause? with)
		{
			if (with is not null)
				PutWith(with);
		}

		/// <summary>
		/// T-SQL: INSERT (Transact-SQL), UPDATE (Transact-SQL), DELETE (Transact-SQL), MERGE (Transact-SQL). What a
		/// data change statement writes to, as the grammar reads it: a name or a variable with their hints,
		/// <c>OPENQUERY</c>, a name after <c>OPENDATASOURCE</c>, or a table-valued function. None takes an alias or
		/// a sample there; MERGE's alias is the statement's own.
		/// </summary>
		void PutDmlTarget(TableSource target)
		{
			switch (target)
			{
				case TableSource.Named named:
					if (named.Alias is not null || named.Sample is not null)
						throw Misplaced(named, "Alias or Sample", "on the target of a data change statement");

					PutSource(named);
					break;

				case TableSource.Variable variable:
					if (variable.Alias is not null || variable.Sample is not null)
						throw Misplaced(variable, "Alias or Sample", "on the target of a data change statement");

					PutVariableName(variable, variable.Name);
					PutTableHints(variable.Hints);
					break;

				case TableSource.OpenQuery { Alias: null, Sample: null }:
				case TableSource.Function { Alias: null, Sample: null }:
					PutSource(target);
					break;

				default:
					throw Misplaced(target, "Target", "that is not a name, a variable, OPENQUERY or a function, or with an alias or a sample");
			}
		}

		/// <summary>
		/// T-SQL: OUTPUT clause (Transact-SQL). The items, then perhaps <c>INTO</c> a target and its columns, then the
		/// second <c>OUTPUT</c>, which the grammar reads only after an <c>INTO</c>.
		/// </summary>
		void PutOutput(OutputClause? output)
		{
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, output, static (into, node) => into.PutOutput(node));

				return;
			}

			if (output is null)
				return;

			if (output.Items.Count == 0)
				throw Empty(output, "Items");

			Word("OUTPUT");
			Each(output.Items, PutColumn);

			if (output.Into is { } into)
			{
				Word("INTO");
				PutDmlTarget(into);

				if (output.Columns is { } columns)
				{
					if (columns.Count == 0)
						throw Empty(output, "Columns");

					Open();
					Each(columns, PutIdentifier);
					Close();
				}
			}
			else if (output.Columns is not null)
				throw Misplaced(output, "Columns", "without Into");

			if (output.Next is { } next)
			{
				if (output.Into is null)
					throw Misplaced(output, "Next", "without Into, after which alone the grammar reads a second OUTPUT");

				PutOutput(next);
			}
		}

		/// <summary>T-SQL: UPDATE (Transact-SQL), DELETE (Transact-SQL). The second FROM.</summary>
		void PutSecondFrom(FromClause? from)
		{
			if (from is null)
				return;

			if (from.Sources.Count == 0)
				throw Empty(from, "Sources");

			Word("FROM");
			Each(from.Sources, PutSource);
		}

		/// <summary>T-SQL: INSERT (Transact-SQL). The word before the target, where one was written.</summary>
		void PutIntoKeyword(IntoKeyword? into)
		{
			switch (into)
			{
				case null:
					break;

				case IntoKeyword.Into:
					Word("INTO");
					break;

				case IntoKeyword.Over:
					Word("OVER");
					break;

				default:
					throw NoText(into.Value);
			}
		}
	}
}
