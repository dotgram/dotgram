using System;
using System.Linq;

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
			switch (node)
			{
				// T-SQL: BULK INSERT (Transact-SQL). A file, a string or a name.
				case Statement.BulkInsert bulk:
					Word("BULK INSERT");
					PutName(bulk.Table);
					Word("FROM");
					PutExpression(bulk.File);
					PutBulkWith(bulk.Options);
					return true;

				// T-SQL: INSERT BULK (no syntax block in syntax.md). Its columns, which it must name.
				case Statement.InsertBulk stream:
					if (stream.Columns.Count == 0)
						throw Empty(stream, "Columns");

					Word("INSERT BULK");
					PutName(stream.Table);
					Open();
					Each(stream.Columns, column => PutTypedColumn(column, typed: false, "a column of INSERT BULK"));
					Close();
					PutBulkWith(stream.Options);
					return true;

				// T-SQL: READTEXT (Transact-SQL).
				case Statement.ReadText read:
					Word("READTEXT");
					PutName(read.Column);
					PutExpression(read.Pointer);
					PutExpression(read.Offset);
					PutExpression(read.Size);

					if (read.HoldLock)
						Word("HOLDLOCK");

					return true;

				// T-SQL: WRITETEXT (Transact-SQL).
				case Statement.WriteText write:
					Word("WRITETEXT");

					if (write.Bulk)
						Word("BULK");

					PutName(write.Column);
					PutExpression(write.Pointer);
					PutTextStamp(write.Timestamp);
					PutTextLog(write.Log);

					if (write.Data is { } data)
						PutExpression(data);

					return true;

				// T-SQL: UPDATETEXT (Transact-SQL).
				case Statement.UpdateText update:
					if ((update.Source is null) != (update.SourcePointer is null))
						throw Misplaced(update, "Source or SourcePointer", "without the other");

					if (update.Source is not null && update.Data is not null)
						throw Both(update, "Source", "Data");

					Word("UPDATETEXT");

					if (update.Bulk)
						Word("BULK");

					PutName(update.Column);
					PutExpression(update.Pointer);
					PutTextStamp(update.Timestamp);
					PutExpression(update.Offset);
					PutExpression(update.Length);
					PutTextLog(update.Log);

					if (update.Source is { } from)
					{
						PutName(from);
						PutExpression(update.SourcePointer!);
					}
					else if (update.Data is { } value)
						PutExpression(value);

					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// A column of PREDICT's schema or of INSERT BULK: a name, a type, a collation and <c>NOT NULL</c>, nothing
		/// else; INSERT BULK may leave the type out, and then everything after it.
		/// </summary>
		void PutTypedColumn(ColumnDefinition column, bool typed, string where)
		{
			if (column.Generation is not null || column.Constraints.Any(static one => one is not Constraint.NotNull { Name: null, Characteristics: null }) || column.Constraints.Count > 1)
				throw Misplaced(column, "Generation or Constraints", $"on {where}, which takes NOT NULL alone");

			if (column.Type is null && (typed || column.Collation is not null || column.Constraints.Count > 0))
				throw Empty(column, "Type");

			PutIdentifier(column.Name);

			if (column.Type is { } type)
				PutType(type);

			if (column.Collation is { } collation)
			{
				Word("COLLATE");
				PutCollation(collation);
			}

			if (column.Constraints.Count > 0)
				Word("NOT NULL");
		}

		/// <summary>BULK INSERT's and INSERT BULK's <c>WITH (…)</c>, where there are options.</summary>
		void PutBulkWith(SqlList<Option> options)
		{
			if (options.Count == 0)
				return;

			Word("WITH");
			Open();
			Each(options, option => PutBulkOption(option, unique: false));
			Close();
		}

		/// <summary>
		/// One bulk option: a word, a word and its value, or <c>ORDER (…)</c> under the option's name, with <c>UNIQUE</c>
		/// only where OPENROWSET BULK reads it.
		/// </summary>
		void PutBulkOption(Option option, bool unique)
		{
			if (option.Options.Count > 0)
				throw Misplaced(option, "Options", "on a bulk option");

			PutIdentifier(option.Name);

			// T-SQL: BULK INSERT (Transact-SQL), OPENROWSET BULK (Transact-SQL). `ORDER (…) [UNIQUE]`.
			if (option.Order is { } order)
			{
				if (option.Value is not null)
					throw Both(option, "Order", "Value");

				if (order.Unique && !unique)
					throw Misplaced(order, "Unique", "on BULK INSERT's or INSERT BULK's ORDER");

				if (order.Columns.Count == 0)
					throw Empty(order, "Columns");

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

		void PutTextStamp(Expression? stamp)
		{
			if (stamp is null)
				return;

			Word("TIMESTAMP");
			Word("=");
			PutExpression(stamp);
		}

		void PutTextLog(bool log)
		{
			if (log)
				Word("WITH LOG");
		}

		/// <summary>T-SQL: TRUNCATE TABLE (Transact-SQL). <c>WITH (PARTITIONS (1, 3 TO 5))</c>.</summary>
		void PutPartitions(SqlList<PartitionRange> partitions)
		{
			if (partitions.Count == 0)
				return;

			Word("WITH");
			Open();
			Word("PARTITIONS");
			Open();
			Each(partitions, partition =>
			{
				PutExpression(partition.From);

				if (partition.To is { } to)
				{
					Word("TO");
					PutExpression(to);
				}
			});
			Close();
			Close();
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

		/// <summary>T-SQL: SELECT @local_variable (Transact-SQL), UPDATE (Transact-SQL). An assignment's operator.</summary>
		static string AssignmentText(AssignmentOperator op)
		{
			return op switch
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
				_                              => throw NoText(op),
			};
		}

		/// <summary>
		/// T-SQL: UPDATE (Transact-SQL). An assignment the standard's SET does not write: a variable, then the target and
		/// the operator, then the value; or a method called on a column, alone. False for a plain assignment of targets,
		/// which the standard's arm writes. Refused: nothing assigned and no call; a variable with more than one target
		/// or brackets; a compound operator with more than one target or brackets; and in MERGE a variable with a
		/// column (Msg 10723).
		/// </summary>
		bool PutTransactSqlAssignment(Assignment assignment, bool merging)
		{
			if (assignment.Variable is null && assignment.Operator == AssignmentOperator.Assign && assignment.Targets.Count > 0)
				return false;

			if (assignment.Targets.Count > 1 || assignment.Parenthesized)
				throw Misplaced(assignment, "Variable or Operator", "with more than one target or with brackets");

			if (assignment.Variable is { } variable)
			{
				if (merging && assignment.Targets.Count > 0)
					throw Misplaced(assignment, "Variable", "with a column, which MERGE refuses (Msg 10723)");

				PutExpression(variable);

				if (assignment.Targets.Count == 0)
				{
					Word(AssignmentText(assignment.Operator));
					PutExpression(assignment.Value);

					return true;
				}

				Word("=");
			}

			if (assignment.Targets.Count == 0)
			{
				if (assignment.Value is not Expression.Member { Kind: MemberAccessKind.Dot, Arguments: not null, Target: Expression.Reference { Name.Parts.Count: 1 } } call
					|| assignment.Operator != AssignmentOperator.Assign)
					throw Misplaced(assignment, "Targets", "empty, with neither a variable nor a method called on a column");

				PutExpression(call);

				return true;
			}

			PutAssignmentTarget(assignment.Targets[0]);
			Word(AssignmentText(assignment.Operator));
			PutExpression(assignment.Value);

			return true;
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
