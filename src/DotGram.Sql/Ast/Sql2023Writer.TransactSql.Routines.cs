using System;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Routines family: writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutRoutines(ISqlNode node)
		{
			switch (node)
			{
				// T-SQL: EXECUTE (Transact-SQL). ExecuteBody's second form, or BareCall: without the word only as the
				// statement being written, a batch's first, with no AT and a module held by name (?!TSql.Variable).
				case Statement.ExecuteModule execute:
					PutExecuteModule(execute);
					return true;

				// T-SQL: EXECUTE (Transact-SQL). ExecuteBody's first form.
				case Statement.ExecuteString execute:
					PutExecuteString(execute);
					return true;

				// T-SQL: INSERT (Transact-SQL). <c>INSERT t EXEC …</c>: the call follows the target, never bare.
				case InsertSource.ExecuteModule source:
					if (source.Statement.Keyword is null)
						throw Misplaced(source, "Statement", "a call without EXEC, which only a batch's first statement is");

					PutExecuteModule(source.Statement);
					return true;

				case InsertSource.ExecuteString source:
					PutExecuteString(source.Statement);
					return true;

				default:
					return false;
			}
		}

		void PutExecuteModule(Statement.ExecuteModule execute)
		{
			if (execute.Keyword is { } keyword)
				Word(keyword switch { ExecuteKeyword.Exec => "EXEC", ExecuteKeyword.Execute => "EXECUTE", _ => throw NoText(keyword) });
			else if (written.Length > 0)
				throw Misplaced(execute, "Keyword", "null inside another statement, where only a batch's first statement is a bare call");
			else if (execute.At is not null || execute.Module is ModuleReference.Variable)
				throw Misplaced(execute, "Keyword", "null with AT or a module held in a variable, which BareCall does not read");

			if (execute.ReturnStatus is { } status)
			{
				PutVariableName(execute, status);
				Word("=");
			}

			switch (execute.Module)
			{
				case ModuleReference.Named named:
					if (named.DataSource is { } source)
					{
						if (source.Provider is not LiteralValue.String || source.InitString is not LiteralValue.String || named.Name.Last.Text.Length == 0)
							throw Misplaced(named, "DataSource", "with other than two strings, or a name ending empty (AdHocServer, AdHocObject)");

						Word("OPENDATASOURCE");
						Call();
						PutExpression(source.Provider);
						Tight(",");
						PutExpression(source.InitString);
						Close();
						Tight(".");
						Hold();
					}

					PutName(named.Name);
					break;

				case ModuleReference.Variable variable:
					if (execute.Number is not null)
						throw Misplaced(execute, "Number", "after a module held in a variable (ExecTarget)");

					PutVariableName(variable, variable.Name);
					break;

				default:
					throw Unwritten(execute.Module);
			}

			if (execute.Number is { } number)
			{
				if (number.Kind != NumericLiteralKind.DecimalInteger || !Unsigned(number.Text, NumericLiteralKind.DecimalInteger))
					throw Misplaced(execute, "Number", "other than digits (ExecNumber)");

				Tight(";");
				Hold();
				PutExpression(number);
			}

			Each(execute.Arguments, PutExecuteArgument);

			if (execute.At is { } at)
			{
				Word("AT");
				PutIdentifier(at);
			}

			PutExecuteOptions(execute.Options);
		}

		/// <summary>
		/// T-SQL: EXECUTE (Transact-SQL). ExecArgument: <c>[@name =] value [OUTPUT | OUT]</c>, the value of a form
		/// ExecValue reads, passed back only from a variable (<c>Syntax.Passes</c>, <c>Msg 179</c>); a plain AT by
		/// position reads as the AT clause.
		/// </summary>
		void PutExecuteArgument(ExecuteArgument argument)
		{
			if (!ExecValue(argument.Value))
				throw Misplaced(argument, "Value", "of a form ExecValue does not read: an expression, a sign but on a number or money, a name of two parts");

			if (argument.Output is not null && argument.Value is not Expression.Variable)
				throw Misplaced(argument, "Output", "on a value that is not a variable (Syntax.Passes, Msg 179)");

			if (argument.Name is { } name)
			{
				PutVariableName(argument, name);
				Word("=");
			}
			else if (argument.Value is Expression.Reference { Name: [{ Style: IdentifierStyle.Regular } word] } && word.Text.Equals("AT", StringComparison.OrdinalIgnoreCase))
				throw Misplaced(argument, "Value", "the word AT by position, which reads as the AT clause (ExecArgument's ?!\"AT\")");

			PutExpression(argument.Value);

			if (argument.Output is { } output)
				Word(output switch { OutputKeyword.Output => "OUTPUT", OutputKeyword.Out => "OUT", _ => throw NoText(output) });
		}

		/// <summary>
		/// T-SQL: EXECUTE (Transact-SQL). ExecValue: DEFAULT, an ODBC literal, a number or money with its sign in the
		/// literal, a string, a binary string, NULL, a variable, or a name of one part — not an expression.
		/// </summary>
		static bool ExecValue(Expression value)
		{
			return value switch
			{
				Expression.Default or LiteralValue.Null or Expression.Variable => true,
				LiteralValue.Numeric { Kind: NumericLiteralKind.Money } => true,
				LiteralValue.Numeric number => Unsigned(number.Text.StartsWith("-", StringComparison.Ordinal) ? number.Text.Substring(1) : number.Text, number.Kind),
				LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null } => true,
				LiteralValue.Binary { ZeroX: true } => true,
				Expression.OdbcEscape { Called: false, Kind.Text: var kind, Value: LiteralValue.String { Kind: StringLiteralKind.Character } }
					=> kind.ToUpperInvariant() is "TS" or "D" or "T" or "GUID",
				Expression.Reference { Name: [{ Style: IdentifierStyle.Regular or IdentifierStyle.Bracketed or IdentifierStyle.Delimited, Text.Length: > 0 }] } => true,
				_ => false,
			};
		}

		void PutExecuteString(Statement.ExecuteString execute)
		{
			if (execute.At is not null && execute.DataSource is not null)
				throw Both(execute, "At", "DataSource");

			if (execute.Values.Count > 0 && execute.At is null && execute.DataSource is null)
				throw Misplaced(execute, "Values", "without AT or AT DATA_SOURCE, which only a linked server or a data source takes");

			Word(execute.Exec ? "EXEC" : "EXECUTE");
			Open();
			PutExpression(execute.Sql);

			foreach (var value in execute.Values)
			{
				Tight(",");
				PutExpression(value);
			}

			Close();

			if (execute.Context is { } context)
			{
				if (context.Kind is not (ExecutionContextKind.Login or ExecutionContextKind.User)
					|| context.Name is not LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null })
					throw Misplaced(execute, "Context", "other than LOGIN or USER and a string (ExecContext)");

				Word(context.Kind == ExecutionContextKind.Login ? "AS LOGIN =" : "AS USER =");
				PutExpression(context.Name);
			}

			if (execute.DataSource is { } source)
			{
				Word("AT DATA_SOURCE");
				PutIdentifier(source);
			}

			if (execute.At is { } at)
			{
				Word("AT");
				PutIdentifier(at);
			}

			PutExecuteOptions(execute.Options);
		}

		/// <summary>
		/// T-SQL: EXECUTE (Transact-SQL). ExecWith: words, and RESULT SETS UNDEFINED, NONE or described, its sets
		/// written exactly when described; a set's columns each with its type (ResultColumns, <c>Syntax.Typed</c>).
		/// </summary>
		void PutExecuteOptions(SqlList<ExecuteOption> options)
		{
			if (options.Count == 0)
				return;

			Word("WITH");
			Each(options, option =>
			{
				switch (option)
				{
					case ExecuteOption.Named named:
						PutIdentifier(named.Name);
						break;

					case ExecuteOption.ResultSets sets:
						if ((sets.Kind == ResultSetsKind.Described) != (sets.Sets.Count > 0))
							throw Misplaced(sets, "Sets", "written other than exactly when described");

						Word("RESULT SETS");

						switch (sets.Kind)
						{
							case ResultSetsKind.Undefined:
								Word("UNDEFINED");
								break;

							case ResultSetsKind.None:
								Word("NONE");
								break;

							case ResultSetsKind.Described:
								Open();
								Each(sets.Sets, PutResultSet);
								Close();
								break;

							default:
								throw NoText(sets.Kind);
						}

						break;

					default:
						throw Unwritten(option);
				}
			});
		}

		void PutResultSet(ResultSet set)
		{
			switch (set)
			{
				case ResultSet.Columns columns:
					if (columns.Elements.Count == 0)
						throw Empty(columns, "Elements");

					if (columns.Elements.Any(static one => one is not TableElement.Column { Definition.Type: not null }))
						throw Misplaced(columns, "Elements", "other than columns each with its type (ResultColumns, Syntax.Typed)");

					PutElements(columns.Elements);
					break;

				case ResultSet.AsObject named:
					Word("AS OBJECT");
					PutName(named.Name);
					break;

				case ResultSet.AsType typed:
					Word("AS TYPE");
					PutName(typed.Name);
					break;

				case ResultSet.AsForXml:
					Word("AS FOR XML");
					break;

				default:
					throw Unwritten(set);
			}
		}
	}
}
