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
				// T-SQL: BULK INSERT (Transact-SQL). A file, a string or a name: the grammar's BulkFile reads no other.
				case Statement.BulkInsert bulk:
					if (bulk.File is not (Expression.Reference or LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null }))
						throw Misplaced(bulk, "File", "that is neither a string nor a name, which is all BULK INSERT reads after FROM");

					Word("BULK INSERT");
					PutName(bulk.Table);
					Word("FROM");
					PutExpression(bulk.File);
					PutBulkWith(bulk.Options, stream: false);
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
					PutBulkWith(stream.Options, stream: true);
					return true;

				// T-SQL: READTEXT (Transact-SQL).
				case Statement.ReadText read:
					Word("READTEXT");
					PutName(read.Column);
					PutTextValue(read, "Pointer", read.Pointer, TextPointer);
					PutTextValue(read, "Offset", read.Offset, TextAmount);
					PutTextValue(read, "Size", read.Size, TextAmount);

					if (read.HoldLock)
						Word("HOLDLOCK");

					return true;

				// T-SQL: WRITETEXT (Transact-SQL).
				case Statement.WriteText write:
					Word("WRITETEXT");

					if (write.Bulk)
						Word("BULK");

					PutName(write.Column);
					PutTextValue(write, "Pointer", write.Pointer, TextPointer);
					PutTextStamp(write, write.Timestamp);
					PutTextLog(write.Log);

					if (write.Data is { } data)
						PutTextValue(write, "Data", data, TextData);

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
					PutTextValue(update, "Pointer", update.Pointer, TextPointer);
					PutTextStamp(update, update.Timestamp);
					PutTextValue(update, "Offset", update.Offset, TextPlace);
					PutTextValue(update, "Length", update.Length, TextPlace);
					PutTextLog(update.Log);

					if (update.Source is { } from)
					{
						PutName(from);
						PutTextValue(update, "SourcePointer", update.SourcePointer!, TextPointer);
					}
					else if (update.Data is { } value)
						PutTextValue(update, "Data", value, TextData);

					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// A column of PREDICT's schema or of INSERT BULK: a name, a type, a collation and <c>NULL</c> or
		/// <c>NOT NULL</c>, nothing else, held as T-SQL's options in that order or as the standard's members; INSERT
		/// BULK may leave the type out, and then everything after it.
		/// </summary>
		void PutTypedColumn(ColumnDefinition column, bool typed, string where)
		{
			if (column.Options.Count > 0)
			{
				if (column.Generation is not null || column.Constraints.Count > 0 || column.Collation is not null)
					throw Misplaced(column, "Options", "beside the standard's Generation, Constraints or Collation");

				var at = column.Options[0] is ColumnOption.Collation { Name: null } ? 1 : 0;

				if (column.Options.Count - at > 1 || at < column.Options.Count
					&& column.Options[at] is not (ColumnOption.Null { Name: null } or ColumnOption.Constraint { Name: null, Value: Constraint.NotNull { Name: null, Characteristics: null } }))
					throw Misplaced(column, "Options", $"on {where}, which takes a collation and NULL or NOT NULL alone");

				if (column.Type is null)
					throw Empty(column, "Type");

				PutIdentifier(column.Name);
				PutType(column.Type);

				foreach (var option in column.Options)
					PutColumnOption(option);

				return;
			}

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

		/// <summary>
		/// BULK INSERT's and INSERT BULK's <c>WITH (…)</c>, where there are options: each once, under a name the
		/// statement reads and in the form it reads that name's value in.
		/// </summary>
		void PutBulkWith(SqlList<Option> options, bool stream)
		{
			if (options.Count == 0)
				return;

			foreach (var option in options)
				CheckBulkOption(option, stream);

			if (options.Select(static option => option.Name.Text).Distinct(StringComparer.OrdinalIgnoreCase).Count() < options.Count)
				throw Misplaced(options[0], "Name", "repeated in one WITH, which a bulk insert reads each option once in (Msg 102)");

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

			CheckSingleWord(option);

			if (option.Partitions is not null)
				throw Misplaced(option, "Partitions", "on a bulk option, which reads no ON PARTITIONS");

			if (option.Place is not null)
				throw Misplaced(option, "Place", "on a bulk option, which reads no MOVE TO");

			PutOptionWord(option.Name);

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

		/// <summary>
		/// T-SQL: BULK INSERT (Transact-SQL); INSERT BULK has no syntax block. The grammar's BulkOption and
		/// BulkStreamOption: a name the statement reads, and its value in the one form the name takes.
		/// </summary>
		void CheckBulkOption(Option option, bool stream)
		{
			var form = option.Name.Style == IdentifierStyle.Regular ? BulkForm(option.Name.Text.ToUpperInvariant(), stream) : null;

			if (form is null)
				throw Misplaced(option, "Name", stream ? "that INSERT BULK does not read" : "that BULK INSERT does not read");

			if ((form == BulkValue.Order) != (option.Order is not null))
				throw Misplaced(option, "Order", "under a name other than ORDER, or missing under it");

			var value = option.Value;
			var read = form switch
			{
				BulkValue.Order or BulkValue.Flag => value is null,
				BulkValue.Count    => IsUnsigned(value, decimals: true),
				BulkValue.Whole    => IsUnsigned(value, decimals: false),
				BulkValue.CodePage => IsUnsigned(value, decimals: false) || BulkString(value) is not null,
				BulkValue.Text     => BulkString(value) is not null,
				BulkValue.FileType => BulkString(value) is { } type && type is "char" or "native" or "widechar" or "widenative" or "widechar_ansi" or "dts_buffers",
				_                  => BulkString(value) is "csv",
			};

			if (!read)
				throw Misplaced(option, "Value", "in a form the option does not read");
		}

		/// <summary>What a bulk option's value is written as, by the option's name; null for a name the statement does not read.</summary>
		static BulkValue? BulkForm(string name, bool stream)
		{
			return name switch
			{
				"ORDER"                                                         => BulkValue.Order,
				"CHECK_CONSTRAINTS" or "FIRE_TRIGGERS" or "TABLOCK" or "NO_TRIGGERS" => BulkValue.Flag,
				"ROWS_PER_BATCH"                                                => BulkValue.Count,
				"KILOBYTES_PER_BATCH"                                           => BulkValue.Whole,
				_ when stream                                                   => null,
				"KEEPIDENTITY" or "KEEPNULLS"                                   => BulkValue.Flag,
				"BATCHSIZE" or "FIRSTROW" or "LASTROW"                          => BulkValue.Count,
				"MAXERRORS"                                                     => BulkValue.Whole,
				"CODEPAGE"                                                      => BulkValue.CodePage,
				"DATAFILETYPE"                                                  => BulkValue.FileType,
				"FORMAT"                                                        => BulkValue.Format,
				"ERRORFILE" or "ERRORFILE_DATA_SOURCE" or "FORMATFILE" or "FORMATFILE_DATA_SOURCE" or "DATA_SOURCE"
					or "FIELDTERMINATOR" or "ROWTERMINATOR" or "FIELDQUOTE" or "ESCAPECHAR" or "CREDENTIAL" => BulkValue.Text,
				_                                                               => null,
			};
		}

		/// <summary>An unsigned number with no exponent, and with no point where <paramref name="decimals"/> is false.</summary>
		static bool IsUnsigned(Expression? value, bool decimals)
		{
			return value is LiteralValue.Numeric { Kind: var kind, Text: var text }
				&& (kind == NumericLiteralKind.DecimalInteger || decimals && kind == NumericLiteralKind.Decimal)
				&& text.Length > 0 && (char.IsDigit(text[0]) || text[0] == '.');
		}

		/// <summary>What a string written out holds, lower-cased, or null for a value that is no such string.</summary>
		static string? BulkString(Expression? value)
		{
			return value is LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null, Text: var text }
				? text.Substring(1, text.Length - 2).ToLowerInvariant()
				: null;
		}

		enum BulkValue { Order, Flag, Count, Whole, CodePage, Text, FileType, Format }

		/// <summary>The forms a value of a text statement is read in, after the grammar's rules of the same names.</summary>
		[Flags]
		enum TextForms { Variable = 1, Binary = 2, Number = 4, Negative = 8, Null = 16, String = 32 }

		const TextForms TextPointer = TextForms.Variable | TextForms.Binary | TextForms.Number;
		const TextForms TextAmount  = TextForms.Number | TextForms.Variable;
		const TextForms TextPlace   = TextForms.Null | TextForms.Number | TextForms.Negative | TextForms.Variable;
		const TextForms TextData    = TextForms.String | TextForms.Binary | TextForms.Variable | TextForms.Null;

		/// <summary>
		/// T-SQL: READTEXT, WRITETEXT, UPDATETEXT (Transact-SQL). A value where the grammar reads only some forms: a
		/// variable that is not the server's, a <c>0x</c> string, a number with no exponent (signed only where
		/// <see cref="TextForms.Negative"/> says), <c>NULL</c>, a string written out.
		/// </summary>
		void PutTextValue(Statement statement, string member, Expression value, TextForms forms)
		{
			var form = value switch
			{
				Expression.Variable variable when !variable.Name.Text.StartsWith("@@", StringComparison.Ordinal) => TextForms.Variable,
				LiteralValue.Binary { ZeroX: true } => TextForms.Binary,
				LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger or NumericLiteralKind.Decimal, Text: var text }
					=> text.StartsWith("-", StringComparison.Ordinal) ? TextForms.Number | TextForms.Negative : TextForms.Number,
				LiteralValue.Null => TextForms.Null,
				LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null } => TextForms.String,
				_ => (TextForms)0,
			};

			if (form == 0 || (form & forms) != form)
				throw Misplaced(statement, member, "in a form the text statements do not read there");

			PutSignedNumber(statement, value);
		}

		/// <summary>T-SQL: WRITETEXT, UPDATETEXT (Transact-SQL). <c>TIMESTAMP = 0x…</c>, a binary string alone.</summary>
		void PutTextStamp(Statement statement, Expression? stamp)
		{
			if (stamp is null)
				return;

			if (stamp is not LiteralValue.Binary { ZeroX: true })
				throw Misplaced(statement, "Timestamp", "that is not a 0x string, which is all TIMESTAMP = reads");

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
			PutRangeList(partitions);
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
		/// a sample there, nor a name <c>FOR PATH</c> or <c>FOR SYSTEM_TIME</c>; MERGE's alias is the statement's own.
		/// <c>OUTPUT … INTO</c> reads no function (<paramref name="into"/>): <c>INTO t2(a)</c> is a table and its columns.
		/// </summary>
		void PutDmlTarget(TableSource target, bool into = false)
		{
			switch (target)
			{
				case TableSource.Named named:
					if (named.Alias is not null || named.Sample is not null)
						throw Misplaced(named, "Alias or Sample", "on the target of a data change statement");

					if (named.ForPath || named.SystemTime is not null)
						throw Misplaced(named, "ForPath or SystemTime", "on the target of a data change statement");

					PutSource(named);
					break;

				case TableSource.Variable variable:
					if (variable.Alias is not null || variable.Sample is not null)
						throw Misplaced(variable, "Alias or Sample", "on the target of a data change statement");

					PutVariableName(variable, variable.Name);
					PutTableHints(variable.Hints);
					break;

				case TableSource.Function when into:
					throw Misplaced(target, "Target", "that is a function, which OUTPUT INTO does not read");

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

			if (output.Items.Any(static item => item is SelectColumn.All or SelectColumn.VariableAssignment))
				throw Misplaced(output, "Items", "with a bare * or a variable assigned, which OUTPUT does not read");

			Word("OUTPUT");
			Each(output.Items, PutColumn);

			if (output.Into is { } into)
			{
				Word("INTO");
				PutDmlTarget(into, into: true);

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

		/// <summary>The data change table an INSERT's SELECT reads its rows from, while that SELECT is written.</summary>
		TableSource? returnedRows;

		/// <summary>
		/// T-SQL: INSERT (Transact-SQL). An INSERT's query, where alone T-SQL's data change table stands, as the one
		/// table of a SELECT's FROM (the grammar's ChangedSource). The rest of what that SELECT may not hold —
		/// grouping, order, TOP, DISTINCT, a set operator — is not asked here.
		/// </summary>
		void PutInsertQuery(Query query)
		{
			var outer = returnedRows;

			returnedRows = query is Query.Select { From.Sources: [TableSource.DataChange { Option: null } rows] } ? rows : null;
			PutQuery(query);
			returnedRows = outer;
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
		/// column (Msg 10723). Refused too, as text that reads back as another assignment: a variable given a
		/// comparison (<c>SET @v = c = 2</c> is a variable and a column); a target with an index or a mutation path
		/// beside a variable or a compound operator (<c>SET a.b += 1</c> is the name <c>a.b</c>); and DEFAULT, which is
		/// assigned plainly to a column alone, <c>SET @v = c = DEFAULT</c> included.
		/// </summary>
		bool PutTransactSqlAssignment(Assignment assignment, bool merging)
		{
			if (assignment.Variable is null && assignment.Operator == AssignmentOperator.Assign && assignment.Targets.Count > 0)
				return false;

			if (assignment.Targets.Count > 1 || assignment.Parenthesized)
				throw Misplaced(assignment, "Variable or Operator", "with more than one target or with brackets");

			if (assignment.Variable is not null && assignment.Value is Expression.Comparison)
				throw Misplaced(assignment, "Value", "that is a comparison, given to a variable: it reads back as a variable and a column");

			if (assignment.Targets is [{ Index: not null } or { MutationPath: not null }])
				throw Misplaced(assignment, "Targets", "with an index or a mutation path, beside a variable or a compound operator");

			if (assignment.Value is Expression.Default && (assignment.Targets.Count == 0 || assignment.Operator != AssignmentOperator.Assign))
				throw Misplaced(assignment, "Value", "DEFAULT, given to a variable or after a compound operator; DEFAULT is assigned plainly to a column");

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
				if (assignment.Value is not Expression.Member { Kind: MemberAccessKind.Dot, Arguments: not null, Target: Expression.Reference { Name.Count: 1 } } call
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
