using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Procedural family: writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutProcedural(ISqlNode node)
		{
			switch (node)
			{
				// T-SQL: BEGIN...END (Transact-SQL), CREATE PROCEDURE (Transact-SQL). A block holds one statement at least
				// (StatementList), and a natively compiled one says its settings first (AtomicBody).
				case Statement.Compound compound:
					if (compound.Statements.Count == 0)
						throw Empty(compound, "Statements");

					foreach (var statement in compound.Statements)
						Unnested(compound, "Statements", statement);

					Word("BEGIN");

					if (compound.Atomic is { } atomic)
					{
						CheckAtomic(compound, atomic);
						Word("ATOMIC WITH");
						Open();
						Each(atomic, PutAtomicOption);
						Close();
					}

					PutStatements(compound.Statements);
					Word("END");
					return true;

				// T-SQL: IF...ELSE (Transact-SQL). An ELSE belongs to the innermost IF, so an IF without one cannot stand
				// as the THEN of an IF with one: the text has no place for the block the tree does not hold.
				case Statement.If @if:
					if (@if.Else is not null && Dangling(@if.Then))
						throw Misplaced(@if, "Then", "that is an IF without ELSE beside an ELSE, which the ELSE would read as its own");

					Unnested(@if, "Then", @if.Then);

					if (@if.Else is { } branch)
						Unnested(@if, "Else", branch);

					Word("IF");
					PutCondition(@if, @if.Condition);
					PutStatement(@if.Then);

					if (@if.Else is { } other)
					{
						Word("ELSE");
						PutStatement(other);
					}

					return true;

				// T-SQL: WHILE (Transact-SQL).
				case Statement.While loop:
					Unnested(loop, "Body", loop.Body);
					Word("WHILE");
					PutCondition(loop, loop.Condition);
					PutStatement(loop.Body);
					return true;

				// T-SQL: TRY...CATCH (Transact-SQL). A TRY holds one statement at least (Msg 102), a CATCH may hold none.
				case Statement.TryCatch tryCatch:
					if (tryCatch.Try.Count == 0)
						throw Empty(tryCatch, "Try");

					foreach (var statement in tryCatch.Try)
						Unnested(tryCatch, "Try", statement);

					foreach (var statement in tryCatch.Catch)
						Unnested(tryCatch, "Catch", statement);

					Word("BEGIN TRY");
					PutStatements(tryCatch.Try);
					Word("END TRY BEGIN CATCH");
					PutStatements(tryCatch.Catch);
					Word("END CATCH");
					return true;

				case Statement.Break:
					Word("BREAK");
					return true;

				case Statement.Continue:
					Word("CONTINUE");
					return true;

				case Statement.GoTo goTo:
					Word("GOTO");
					PutIdentifier(goTo.Name);
					return true;

				// T-SQL: GOTO (Transact-SQL). A word that is a whole statement by itself reads as that statement and not as a
				// label (SimpleCommand tries THROW first): THROW is not reserved, and GOTO THROW is read.
				case Statement.Label label:
					if (label.Name is { Style: IdentifierStyle.Regular, Text: var word } && word.Equals("THROW", StringComparison.OrdinalIgnoreCase))
						throw Misplaced(label, "Name", "THROW, which reads as the statement THROW (SimpleCommand)");

					PutIdentifier(label.Name);
					Tight(":");
					return true;

				// T-SQL: DECLARE @local_variable (Transact-SQL). One variable at least (Declaration).
				case Statement.DeclareVariables declare:
					if (declare.Variables.Count == 0)
						throw Empty(declare, "Variables");

					Word("DECLARE");
					Each(declare.Variables, PutVariableDeclaration);
					return true;

				// T-SQL: SET @local_variable (Transact-SQL). A property is a property's or the variable's, and a method the
				// variable's own (MemberTail): Operator null is a method called, nothing else.
				case Statement.SetVariable set:
					Word("SET");

					if (set.Operator is not { } op)
					{
						if (set.Members.Count > 0
							|| set.Value is not Expression.Member { Kind: MemberAccessKind.Dot, Arguments: not null } call
							|| call.Target != set.Variable)
							throw Misplaced(set, "Operator", "null with anything but a method called on the variable itself");

						PutExpression(call);
						return true;
					}

					PutExpression(set.Variable);

					foreach (var member in set.Members)
					{
						Tight(".");
						Hold();
						PutIdentifier(member);
					}

					Word(AssignmentText(op));
					PutValue(set, set.Value);
					return true;

				// T-SQL: SET Statements (Transact-SQL). One of SetExpressions' forms: the engine refuses a switch among values,
				// and a list of anything but switches, words or values.
				case Statement.SetOptions set:
					PutSetOptions(set);
					return true;

				// T-SQL: BEGIN TRANSACTION, BEGIN DISTRIBUTED TRANSACTION (Transact-SQL). A name or not, and a mark after it
				// or not (TransactionStatement).
				case Statement.BeginTransaction begin:
					Word("BEGIN");

					if (begin.Distributed)
						Word("DISTRIBUTED");

					Word(begin.Tran ? "TRAN" : "TRANSACTION");
					PutTranName(begin, begin.Name);

					if (begin.Mark is { } mark)
					{
						Word("WITH MARK");

						if (mark.Description is { } description)
						{
							// TranMark: a value, never DEFAULT (Msg 156) or a comparison.
							if (description is Expression.Default or Expression.Comparison)
								throw Misplaced(mark, "Description", "DEFAULT or a comparison, which no value is");

							PutExpression(description);
						}
					}

					return true;

				// T-SQL: SAVE TRANSACTION (Transact-SQL). The name is not optional.
				case Statement.SaveTransaction save:
					Word("SAVE");
					Word(save.Tran ? "TRAN" : "TRANSACTION");
					PutTranName(save, save.Name);
					return true;

				// T-SQL: DEALLOCATE (Transact-SQL). One part, GLOBAL before it or not, or a variable (CursorName).
				case Statement.DeallocateCursor deallocate:
					Word("DEALLOCATE");
					PutCursorName(deallocate, deallocate.Cursor);
					return true;

				// T-SQL: DECLARE CURSOR (Transact-SQL). The T-SQL arm takes a cursor whose form T-SQL reads; a standard
				// one — NO SCROLL, WITH HOLD, WITH RETURN, SENSITIVE, a prepared statement — is the standard's to write.
				case Statement.DeclareCursor declare:
					if (!TransactSqlCursor(declare))
						return false;

					if (declare.Cursor is not { Name: { Count: 1 } name, ExtendedName: null, Global: false, Local: false, Ptf: false })
						throw Misplaced(declare, "Cursor", "other than a name of one part (DeclareStatement)");

					Word("DECLARE");
					PutName(name);

					if (declare.Extended)
					{
						Word("CURSOR");

						if (!PutCursorWords(declare.Properties))
							throw Misplaced(declare, "Extended", "with no word after CURSOR, which reads back as the ISO form");
					}
					else
					{
						if (declare.Properties.Sensitivity is not null)
							Word("INSENSITIVE");

						if (declare.Properties.Scrollability is not null)
							Word("SCROLL");

						Word("CURSOR");
					}

					PutCursorSource(declare, declare.Properties, (CursorSource.Query)declare.SourceValue, declared: true);
					return true;

				// T-SQL: SET @local_variable (Transact-SQL). <c>SET @c = CURSOR words FOR query</c> (CursorInPlace).
				case Statement.SetCursorVariable cursor:
					if (cursor.Variable is not Expression.Variable)
						throw Misplaced(cursor, "Variable", "that is not a variable");

					if (cursor.SourceValue is not CursorSource.Query source)
						throw Misplaced(cursor, "SourceValue", "other than a query (CursorInPlace)");

					if (cursor.Properties is { Sensitivity: not null } or { Holdability: not null } or { Returnability: not null }
						|| cursor.Properties is { Scrollability: CursorScrollability.NoScroll, ForwardOnly: false })
						throw Misplaced(cursor, "Properties", "with a word of the standard's, which only T-SQL's words follow (CursorOption)");

					Word("SET");
					PutExpression(cursor.Variable);
					Word("= CURSOR");
					PutCursorWords(cursor.Properties);
					PutCursorSource(cursor, cursor.Properties, source, declared: false);
					return true;

				// T-SQL: PRINT (Transact-SQL).
				case Statement.Print print:
					Word("PRINT");
					PutValue(print, print.Value);
					return true;

				// T-SQL: RAISERROR (Transact-SQL). The arguments in brackets, one at least, and the options after WITH.
				case Statement.RaiseError raise:
					if (raise.Arguments.Count == 0)
						throw Empty(raise, "Arguments");

					Word("RAISERROR");
					Call();
					Each(raise.Arguments, argument => PutValue(raise, argument));
					Close();

					if (raise.Options.Count > 0)
					{
						Word("WITH");
						Each(raise.Options, option => Word(option switch
						{
							RaiseErrorOption.Log      => "LOG",
							RaiseErrorOption.NoWait   => "NOWAIT",
							RaiseErrorOption.SetError => "SETERROR",
							_                         => throw NoText(option),
						}));
					}

					return true;

				// T-SQL: THROW (Transact-SQL). All three, or none (Msg 102 for one or two).
				case Statement.Throw @throw:
					Word("THROW");

					if (@throw is { Number: null, Message: null, State: null })
						return true;

					if (@throw is not { Number: { } number, Message: { } message, State: { } state })
						throw Misplaced(@throw, "Number, Message or State", "without the other two");

					PutValue(@throw, number);
					Tight(",");
					PutValue(@throw, message);
					Tight(",");
					PutValue(@throw, state);
					return true;

				// T-SQL: WAITFOR (Transact-SQL). A string, a national one or a variable (WaitValue).
				case Statement.WaitFor wait:
					if (wait.Value is not (LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null } or Expression.Variable))
						throw Misplaced(wait, "Value", "other than a string or a variable (WaitValue)");

					Word("WAITFOR");
					Word(wait.Kind switch { WaitForKind.Delay => "DELAY", WaitForKind.Time => "TIME", _ => throw NoText(wait.Kind) });
					PutExpression(wait.Value);
					return true;

				// T-SQL: EXECUTE AS (Transact-SQL). CALLER alone, USER or LOGIN with a name; SELF and OWNER are a module's
				// (SessionWho); then NO REVERT or a cookie, one of the two (SessionWith).
				case Statement.ExecuteAs executeAs:
					var context = executeAs.Context;

					if (context.Kind is not (ExecutionContextKind.Caller or ExecutionContextKind.User or ExecutionContextKind.Login))
						throw Misplaced(executeAs, "Context", "SELF or OWNER, which only a module's EXECUTE AS takes (SessionWho)");

					if ((context.Kind == ExecutionContextKind.Caller) != (context.Name is null))
						throw Misplaced(executeAs, "Context.Name", "set for CALLER or left out for USER or LOGIN (SessionWho)");

					if (executeAs is { NoRevert: true, Cookie: not null })
						throw Both(executeAs, "NoRevert", "Cookie");

					Word(executeAs.Exec ? "EXEC AS" : "EXECUTE AS");

					if (context.Name is { } who)
					{
						Word(context.Kind == ExecutionContextKind.User ? "USER =" : "LOGIN =");
						PutValue(executeAs, who);
					}
					else
						Word("CALLER");

					if (executeAs.NoRevert)
						Word("WITH NO REVERT");

					if (executeAs.Cookie is { } cookie)
					{
						Word("WITH COOKIE INTO");
						PutExpression(cookie);
					}

					return true;

				// T-SQL: REVERT (Transact-SQL).
				case Statement.Revert revert:
					Word("REVERT");

					if (revert.Cookie is { } kept)
					{
						Word("WITH COOKIE =");
						PutValue(revert, kept);
					}

					return true;

				// T-SQL: SETUSER (Transact-SQL). A string, a national one or a variable (SessionUser), and NORESET only after one.
				case Statement.SetUser setUser:
					Word("SETUSER");

					if (setUser.Name is null)
					{
						if (setUser.NoReset)
							throw Misplaced(setUser, "NoReset", "without a name (SessionNoReset)");

						return true;
					}

					if (setUser.Name is not (LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null } or Expression.Variable))
						throw Misplaced(setUser, "Name", "other than a string or a variable (SessionUser)");

					PutExpression(setUser.Name);

					if (setUser.NoReset)
						Word("WITH NORESET");

					return true;

				// T-SQL: USE (Transact-SQL).
				case Statement.Use use:
					Word("USE");
					PutIdentifier(use.Database);
					return true;

				// T-SQL: LINENO. Digits, and only that: a sign, a point, a variable are each Msg 102.
				case Statement.LineNumber line:
					if (line.Line is not { Kind: NumericLiteralKind.DecimalInteger, Text: var digits } || !Unsigned(digits, NumericLiteralKind.DecimalInteger))
						throw Misplaced(line, "Line", "other than digits (LINENO)");

					Word("LINENO");
					PutExpression(line.Line);
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// The statements of a block, a TRY or a CATCH, each ended by <c>;</c>: after a statement left unended the grammar
		/// reads neither a <c>WITH</c> nor an unreserved first word (<c>SqlPiece</c>, <c>Unseparated</c>).
		/// </summary>
		void PutStatements(SqlList<Statement> statements)
		{
			foreach (var statement in statements)
			{
				PutStatement(statement);
				Tight(";");
			}
		}

		/// <summary>
		/// T-SQL: DECLARE @local_variable (Transact-SQL). One variable (Declaration, DeclaredAs): a table's body as a table
		/// variable takes it (<c>Syntax.Declared</c>, <c>Syntax.Stored</c>), or a type and a value; <c>CURSOR</c> takes no
		/// value, and a type named <c>CURSOR</c> or <c>TABLE</c> otherwise reads back as those words.
		/// </summary>
		void PutVariableDeclaration(VariableDeclaration declaration)
		{
			switch (declaration)
			{
				case VariableDeclaration.Typed typed:
					PutVariableName(typed, typed.Name);

					if (typed.AsKeyword)
						Word("AS");

					switch (TypeWord(typed.Type))
					{
						case "CURSOR" when typed.Type is DataType.Named { Name.First.Text: "CURSOR", Arguments.Count: 0 }:
							if (typed.Value is not null)
								throw Misplaced(typed, "Value", "on a CURSOR variable (Msg 102)");

							Word("CURSOR");
							return;

						case not null:
							throw Misplaced(typed, "Type", "named CURSOR or TABLE other than as the one-part Named CURSOR, which reads back as the word");
					}

					PutType(typed.Type);

					if (typed.Value is { } value)
					{
						Word("=");
						PutValue(typed, value);
					}

					return;

				case VariableDeclaration.Table table:
					PutVariableName(table, table.Name);

					if (table.AsKeyword)
						Word("AS");

					CheckDeclared(table);
					Word("TABLE");
					PutElements(table.Elements);
					return;

				default:
					throw Unwritten(declaration);
			}
		}

		/// <summary>The word a one-part plain type name is, upper-cased, where it is CURSOR or TABLE; else null.</summary>
		static string? TypeWord(DataType type)
		{
			QualifiedName? name = type switch
			{
				DataType.Named named      => named.Name,
				DataType.UserDefined user => user.Name,
				_                         => null,
			};

			var word = name is { Count: 1 } one && one[0].Style == IdentifierStyle.Regular ? one[0].Text.ToUpperInvariant() : null;

			return word is "CURSOR" or "TABLE" ? word : null;
		}

		/// <summary>
		/// T-SQL: SET @local_variable (Transact-SQL). A value (<c>SetValue</c>, <c>ValueExpression</c>): never DEFAULT
		/// (<c>Msg 4119</c>), and never a comparison, which no value reads and a variable's SET reads as a column.
		/// </summary>
		void PutValue(ISqlNode owner, Expression value)
		{
			if (value is Expression.Default or Expression.Comparison)
				throw Misplaced(owner, "Value", "DEFAULT or a comparison, which no value is");

			PutExpression(value);
		}

		/// <summary>
		/// T-SQL: DECLARE @local_variable (Transact-SQL). What a table variable's body may not hold (<c>Syntax.Declared</c>,
		/// <c>Syntax.Stored</c>, restated over this tree): a named constraint, a name on a column's DEFAULT or NULL,
		/// FILESTREAM, an identity's NOT FOR REPLICATION, a column's REFERENCES without FOREIGN KEY; and computed
		/// columns with no stored one beside them.
		/// </summary>
		static void CheckDeclared(VariableDeclaration.Table table)
		{
			if (table.Elements.Count == 0)
				throw Empty(table, "Elements");

			var computed = false;
			var stored   = false;

			foreach (var element in table.Elements)
				switch (element)
				{
					case TableElement.TableConstraint { Value.Name: not null } constraint:
						throw Misplaced(constraint, "Name", "on a table variable's constraint (Syntax.Declared)");

					case TableElement.Column { Definition: var column }:
						if (column.Constraints.Any(static one => one.Name is not null))
							throw Misplaced(column, "Constraints", "named, on a table variable's column (Syntax.Declared)");

						foreach (var option in column.Options)
							if (option.Name is not null
								|| option is ColumnOption.Filestream or ColumnOption.NotForReplication
								|| option is ColumnOption.Constraint { Value: var held } && (held.Name is not null || held is Constraint.ForeignKey { ForeignKeyWords: false }))
								throw Misplaced(option, "Name, Filestream, NotForReplication or References", "on a table variable's column (Syntax.Declared)");

						if (column.Type is null && column.Options is [ColumnOption.Computed, ..])
							computed = true;
						else
							stored = true;

						break;
				}

			if (computed && !stored)
				throw Misplaced(table, "Elements", "computed columns with none stored beside them (Syntax.Stored)");
		}

		/// <summary>
		/// T-SQL: SET Statements (Transact-SQL). <c>SetExpressions</c>: switches from its list sharing one ON or OFF;
		/// STATISTICS words sharing one; OFFSETS words sharing one; IDENTITY_INSERT alone; ROWCOUNT, TEXTSIZE, ERRLVL
		/// and FIPS_FLAGGER OFF alone; or settings given values from <see cref="SetValues"/>, each of the form its
		/// name takes, any of them twice.
		/// </summary>
		void PutSetOptions(Statement.SetOptions set)
		{
			var options = set.Options;

			if (options.Count == 0)
				throw Empty(set, "Options");

			Word("SET");

			switch (options[0])
			{
				case SetOption.Switch { On: var on }:
					Each(options, option =>
					{
						if (option is not SetOption.Switch switched || switched.On != on)
							throw Misplaced(option, "Options", "beside switches, or a switch set the other way (SetExpressions)");

						if (switched.Option.Style != IdentifierStyle.Regular || !SetSwitches.Contains(switched.Option.Text))
							throw Misplaced(switched, "Option", "that is not one of SetSwitchName's switches");

						PutKeyWord(switched.Option);
					});

					Word(on ? "ON" : "OFF");
					return;

				case SetOption.Statistics { On: var on }:
					Word("STATISTICS");
					Each(options, option =>
					{
						if (option is not SetOption.Statistics statistics || statistics.On != on)
							throw Misplaced(option, "Options", "beside STATISTICS words, or one set the other way (SetExpressions)");

						Word(statistics.Kind switch
						{
							StatisticsKind.Io      => "IO",
							StatisticsKind.Profile => "PROFILE",
							StatisticsKind.Time    => "TIME",
							StatisticsKind.Xml     => "XML",
							_                      => throw NoText(statistics.Kind),
						});
					});

					Word(on ? "ON" : "OFF");
					return;

				case SetOption.Offsets { On: var on }:
					Word("OFFSETS");
					Each(options, option =>
					{
						if (option is not SetOption.Offsets offsets || offsets.On != on)
							throw Misplaced(option, "Options", "beside OFFSETS words, or one set the other way (SetExpressions)");

						if (offsets.Keyword.Style != IdentifierStyle.Regular || !SetOffsets.Contains(offsets.Keyword.Text))
							throw Misplaced(offsets, "Keyword", "that is not one of SetOffsetName's words");

						PutKeyWord(offsets.Keyword);
					});

					Word(on ? "ON" : "OFF");
					return;

				case SetOption.IdentityInsert identity:
					if (options.Count > 1)
						throw Misplaced(options[1], "Options", "beside IDENTITY_INSERT, which stands alone (SetExpressions)");

					Word("IDENTITY_INSERT");
					PutName(identity.Table);
					Word(identity.On ? "ON" : "OFF");
					return;

				case SetOption.Value:
					Each(options, option =>
					{
						if (option is not SetOption.Value value)
							throw Misplaced(option, "Options", "beside settings given values (SetExpressions)");

						if (value.Option.Style != IdentifierStyle.Regular || !SetValues.TryGetValue(value.Option.Text, out var takes))
							throw Misplaced(value, "Option", "that is not one of SettingValue's names, written in capitals as the bridge reads them");

						// FIPS_FLAGGER OFF stands alone, and so do the three names SetExpressions reads apart.
						var off = value.Option.Text == "FIPS_FLAGGER" && value.Argument is Expression.Reference { Name: [{ Text: "OFF", Style: IdentifierStyle.Regular }] };

						if ((off || takes.Alone) && options.Count > 1)
							throw Misplaced(value, "Option", "that stands alone, in a list (SetExpressions)");

						if (!off && !takes.Form(value.Argument))
							throw Misplaced(value, "Argument", "of a form " + value.Option.Text + " does not take (SettingValue)");

						PutKeyWord(value.Option);

						// SetInteger, SetNumber and SetConstant keep a signed number as one token.
						if (off)
							PutOptionValue(value.Argument);
						else
							PutSignedNumber(value, value.Argument);
					});

					return;

				default:
					throw Unwritten(options[0]);
			}
		}

		/// <summary>T-SQL: SET Statements (Transact-SQL). <c>SetSwitchName</c>, which the old tree keeps as written.</summary>
		static readonly HashSet<string> SetSwitches = new(StringComparer.OrdinalIgnoreCase)
		{
			"ANSI_DEFAULTS", "ANSI_NULL_DFLT_OFF", "ANSI_NULL_DFLT_ON", "ANSI_NULLS", "AUTOCOMMIT", "ANSI_PADDING", "ANSI_WARNINGS",
			"ARITHABORT", "ARITHIGNORE", "CONCAT_NULL_YIELDS_NULL", "CURSOR_CLOSE_ON_COMMIT", "FMTONLY", "FORCEPLAN",
			"IMPLICIT_TRANSACTIONS", "NO_BROWSETABLE", "NOCOUNT", "NOEXEC", "NUMERIC_ROUNDABORT", "PARSEONLY", "QUOTED_IDENTIFIER",
			"RECOMMENDATIONS", "REMOTE_PROC_TRANSACTIONS", "RESULT_SET_CACHING", "SHOWPLAN_ALL", "SHOWPLAN_TEXT", "SHOWPLAN_XML",
			"XACT_ABORT",
		};

		/// <summary>T-SQL: SET OFFSETS (Transact-SQL). <c>SetOffsetName</c>.</summary>
		static readonly HashSet<string> SetOffsets = new(StringComparer.OrdinalIgnoreCase)
		{
			"SELECT", "FROM", "ORDER", "COMPUTE", "TABLE", "PROCEDURE", "PROC", "STATEMENT", "PARAM", "EXECUTE", "EXEC",
		};

		/// <summary>
		/// T-SQL: SET Statements (Transact-SQL). The settings given values (<c>SettingValue</c>, and the three
		/// <c>SetExpressions</c> reads alone), each with the form of value it takes: <c>SetConstant</c>,
		/// <c>SetInteger</c>, <c>SetNumber</c>, <c>SetRowCount</c>, <c>SetFipsLevel</c>.
		/// </summary>
		static readonly Dictionary<string, (Func<Expression, bool> Form, bool Alone)> SetValues = new(StringComparer.Ordinal)
		{
			["DATEFIRST"]                 = (SetConstant, false),
			["DATEFORMAT"]                = (SetConstant, false),
			["DEADLOCK_PRIORITY"]         = (SetConstant, false),
			["LOCK_TIMEOUT"]              = (static value => SetNumber(value, whole: true), false),
			["LANGUAGE"]                  = (SetConstant, false),
			["FIPS_FLAGGER"]              = (static value => value is LiteralValue.String { Kind: StringLiteralKind.Character, CharacterSet: null, UnicodeEscape: null }, false),
			["CONTEXT_INFO"]              = (SetConstant, false),
			["QUERY_GOVERNOR_COST_LIMIT"] = (static value => SetNumber(value, whole: false), false),
			["ROWCOUNT"]                  = (static value => value is Expression.Variable || SetNumber(value, whole: false, signed: false), true),
			["TEXTSIZE"]                  = (static value => SetNumber(value, whole: true), true),
			["ERRLVL"]                    = (static value => SetNumber(value, whole: true), true),
		};

		/// <summary>
		/// T-SQL: SET Statements (Transact-SQL). <c>SetConstant</c>: a number, negative or not, a string, a binary string, a
		/// variable or a name of one part; not an expression.
		/// </summary>
		static bool SetConstant(Expression value)
		{
			return value switch
			{
				LiteralValue.Numeric => SetNumber(value, whole: false),
				LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null } => true,
				LiteralValue.Binary { ZeroX: true } or Expression.Variable => true,
				Expression.Reference { Name: [{ Style: IdentifierStyle.Regular or IdentifierStyle.Bracketed or IdentifierStyle.Delimited } name] } => name.Text.Length > 0,
				_ => false,
			};
		}

		/// <summary>
		/// T-SQL: SET Statements (Transact-SQL). <c>SetNumber</c>, a number with a point or an exponent or not, and
		/// <c>SetInteger</c> where it is whole: a literal whose text has its minus sign in it, as the grammar reads it
		/// (<c>Syntax.Compacted</c>), and never an expression; <paramref name="signed"/> false for <c>SetRowCount</c>.
		/// </summary>
		static bool SetNumber(Expression value, bool whole, bool signed = true)
		{
			if (value is not LiteralValue.Numeric { Kind: var kind, Text: var text })
				return false;

			var digits = signed && text.StartsWith("-", StringComparison.Ordinal) ? text[1..] : text;

			return (kind == NumericLiteralKind.DecimalInteger || !whole) && Unsigned(digits, kind);
		}

		/// <summary>
		/// The text of an unsigned number of its kind (<c>UnsignedNumericLiteral</c>): digits, digits with a point, or
		/// either with an exponent; never a hexadecimal number, money or a sign.
		/// </summary>
		static bool Unsigned(string text, NumericLiteralKind kind)
		{
			return kind switch
			{
				NumericLiteralKind.DecimalInteger => Regex.IsMatch(text, @"^[0-9]+$"),
				NumericLiteralKind.Decimal        => Regex.IsMatch(text, @"^([0-9]+\.[0-9]*|\.[0-9]+)$"),
				NumericLiteralKind.Approximate    => Regex.IsMatch(text, @"^([0-9]+\.?[0-9]*|\.[0-9]+)[eE][+-]?[0-9]+$"),
				_                                 => false,
			};
		}

		/// <summary>
		/// T-SQL: BEGIN TRANSACTION (Transact-SQL), SAVE TRANSACTION (Transact-SQL). <c>TranName</c>: a name, or a
		/// variable, never both; a name that starts with <c>@</c> reads back as a variable.
		/// </summary>
		void PutTranName(ISqlNode owner, TransactionName? name)
		{
			switch (name)
			{
				case null:
					return;

				case { Name: not null, Variable: not null }:
					throw Both(name, "Name", "Variable");

				case { Name: { } named } when named.Style == IdentifierStyle.Regular && named.Text.StartsWith("@", StringComparison.Ordinal):
					throw Misplaced(owner, "Name", "starting with @, which reads back as a variable");

				case { Variable: { } variable } when variable is not Expression.Variable:
					throw Misplaced(owner, "Name", "held as an expression other than a variable (TranName)");
			}

			PutTransactionName(name);
		}

		/// <summary>
		/// T-SQL: DEALLOCATE (Transact-SQL). <c>CursorName</c>: <c>[GLOBAL] name</c> of one part, or a variable that is
		/// not one of the server's (<c>LocalVariable</c>).
		/// </summary>
		void PutCursorName(ISqlNode owner, CursorReference cursor)
		{
			if (cursor.Local || cursor.Ptf)
				throw Misplaced(owner, "Cursor", "with LOCAL or PTF, which no T-SQL cursor's name takes");

			if (cursor.Name is { } name)
			{
				if (name.Count != 1 || cursor.ExtendedName is not null)
					throw Misplaced(owner, "Cursor", "a name of more than one part, or a name and a variable");

				if (cursor.Global)
					Word("GLOBAL");

				PutName(name);
				return;
			}

			if (cursor.Global || cursor.ExtendedName is not Expression.Variable variable || Syntax.IsServerVariable(variable.Name.Text))
				throw Misplaced(owner, "Cursor", "other than a name, or a variable of the batch's own without GLOBAL (LocalVariable)");

			PutExpression(variable);
		}

		/// <summary>
		/// Whether a DECLARE CURSOR is T-SQL's to write: its properties words a form of the T-SQL page reads, its source a
		/// query, its FOR spelt READ ONLY, and a FOR before the hints only where there are hints.
		/// </summary>
		static bool TransactSqlCursor(Statement.DeclareCursor declare)
		{
			var properties = declare.Properties;

			if (properties is { Holdability: not null } or { Returnability: not null } or { Sensitivity: CursorSensitivity.Sensitive or CursorSensitivity.Asensitive }
				|| properties is { Scrollability: CursorScrollability.NoScroll, ForwardOnly: false })
				return false;

			if (!declare.Extended && (properties.Scope is not null || properties.Type is not null || properties.Concurrency is not null
				|| properties.TypeWarning || properties.ForwardOnly))
				return false;

			return declare.SourceValue is CursorSource.Query source && source.Updatability is not { Underscored: true }
				&& (!source.UpdatabilityBeforeOptions || source.Select.Hints.Count > 0);
		}

		/// <summary>
		/// T-SQL: DECLARE CURSOR (Transact-SQL). The words after CURSOR in the page's order (CursorOption): the scope, the
		/// scrolling, the kind, the concurrency, TYPE_WARNING. False where there are none.
		/// </summary>
		bool PutCursorWords(CursorProperties properties)
		{
			var any = false;

			void Put(string word)
			{
				Word(word);
				any = true;
			}

			if (properties.Sensitivity is not null)
				throw Misplaced(properties, "Sensitivity", "after CURSOR, where INSENSITIVE is not a word");

			if (properties.ForwardOnly && properties.Scrollability != CursorScrollability.NoScroll)
				throw Misplaced(properties, "ForwardOnly", "without NoScroll, which it is the spelling of");

			if (properties.Scope is { } scope)
				Put(scope switch { CursorScope.Local => "LOCAL", CursorScope.Global => "GLOBAL", _ => throw NoText(scope) });

			if (properties.Scrollability is { } scrollability)
				Put(scrollability == CursorScrollability.Scroll ? "SCROLL" : "FORWARD_ONLY");

			if (properties.Type is { } type)
				Put(type switch
				{
					CursorType.Static      => "STATIC",
					CursorType.Keyset      => "KEYSET",
					CursorType.Dynamic     => "DYNAMIC",
					CursorType.FastForward => "FAST_FORWARD",
					_                      => throw NoText(type),
				});

			if (properties.Concurrency is { } concurrency)
				Put(concurrency switch
				{
					CursorConcurrency.ReadOnly    => "READ_ONLY",
					CursorConcurrency.ScrollLocks => "SCROLL_LOCKS",
					CursorConcurrency.Optimistic  => "OPTIMISTIC",
					_                             => throw NoText(concurrency),
				});

			if (properties.TypeWarning)
				Put("TYPE_WARNING");

			return any;
		}

		/// <summary>
		/// T-SQL: DECLARE CURSOR (Transact-SQL). <c>FOR query</c> and what may be done through it (CursorQuery): a select
		/// that makes no table and has no shape (<c>Syntax.Cursorable</c>, <c>Msg 154</c>, <c>6819</c>), FOR READ ONLY or
		/// FOR UPDATE before the hints or after them; and the words that agree with it (<c>Syntax.CursorAgrees</c>,
		/// restated over this tree).
		/// </summary>
		void PutCursorSource(ISqlNode owner, CursorProperties properties, CursorSource.Query source, bool declared)
		{
			var select = source.Select;

			if (select.Query.For is not null || select.Query.Locking.Count > 0
				|| select.Query.Operands().Any(static one => one.Values.Count > 0 || one.Into is not null || one.IntoTable is not null))
				throw Misplaced(owner, "SourceValue", "a query with FOR, a locking clause, INTO or VALUES, which no cursor reads (Syntax.Cursorable)");

			var fast = properties.Type == CursorType.FastForward;

			if (fast && (properties.Scrollability == CursorScrollability.Scroll || properties.Concurrency is CursorConcurrency.ScrollLocks or CursorConcurrency.Optimistic))
				throw Misplaced(owner, "Properties", "FAST_FORWARD beside SCROLL, SCROLL_LOCKS or OPTIMISTIC (Syntax.CursorAgrees)");

			if (source.Updatability is { } updatability)
			{
				if (updatability.ReadOnly && updatability.UpdateColumns.Count > 0)
					throw Misplaced(updatability, "UpdateColumns", "beside READ ONLY");

				if (updatability.Underscored)
					throw Misplaced(updatability, "Underscored", "where CursorFor reads READ ONLY");

				var agrees = updatability.ReadOnly
					? properties.Concurrency is not (CursorConcurrency.ScrollLocks or CursorConcurrency.Optimistic) && (!declared || properties.Concurrency != CursorConcurrency.ReadOnly)
					: properties.Type is not (CursorType.Static or CursorType.FastForward) && properties.Concurrency != CursorConcurrency.ReadOnly
						&& properties.Sensitivity != CursorSensitivity.Insensitive;

				if (!agrees)
					throw Misplaced(owner, "Properties", "a word that disagrees with FOR " + (updatability.ReadOnly ? "READ ONLY" : "UPDATE") + " (Syntax.CursorAgrees)");
			}

			if (source.UpdatabilityBeforeOptions && (source.Updatability is null || select.Hints.Count == 0))
				throw Misplaced(source, "UpdatabilityBeforeOptions", "with no FOR or no hints to stand before");

			Word("FOR");
			PutQuery(select.Query);

			if (source.UpdatabilityBeforeOptions)
				PutUpdatability(source.Updatability!);

			PutQueryHints(select.Hints);

			if (!source.UpdatabilityBeforeOptions && source.Updatability is { } after)
				PutUpdatability(after);
		}

		void PutUpdatability(UpdatabilityClause updatability)
		{
			if (updatability.ReadOnly)
			{
				Word("FOR READ ONLY");
				return;
			}

			Word("FOR UPDATE");

			if (updatability.UpdateColumns.Count > 0)
			{
				Word("OF");
				Each(updatability.UpdateColumns, PutIdentifier);
			}
		}

		/// <summary>
		/// T-SQL: CREATE PROCEDURE (Transact-SQL). A natively compiled block is a natively compiled module's body and
		/// nothing else's: nested in a block, a TRY, a CATCH or a branch it is <c>Msg 102</c> (<c>AtomicBody</c>).
		/// </summary>
		static void Unnested(ISqlNode owner, string member, Statement statement)
		{
			if (statement is Statement.Compound { Atomic: not null })
				throw Misplaced(owner, member, "that is a natively compiled block, which only a natively compiled module's body is (AtomicBody)");
		}

		/// <summary>Whether a statement ends in an IF without an ELSE, which an ELSE written after it would take.</summary>
		static bool Dangling(Statement statement)
		{
			while (true)
			{
				switch (statement)
				{
					case Statement.If { Else: null }:
						return true;

					case Statement.If { Else: { } other }:
						statement = other;
						break;

					case Statement.While loop:
						statement = loop.Body;
						break;

					default:
						return false;
				}
			}
		}

		/// <summary>
		/// T-SQL: CREATE PROCEDURE (Transact-SQL). <c>AtomicOptions</c>: the level and the five settings, each once
		/// (<c>Msg 1039</c>), a level and a language among them (<c>Msg 10784</c>).
		/// </summary>
		static void CheckAtomic(ISqlNode owner, SqlList<AtomicOption> options)
		{
			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (var option in options)
			{
				var name = option switch
				{
					AtomicOption.Isolation => "TRANSACTION ISOLATION LEVEL",
					AtomicOption.Setting { Option.Name: var named } => named.Text,
					_ => throw Unwritten(option),
				};

				if (!seen.Add(name))
					throw Misplaced(owner, "Atomic", $"with {name} twice, which AtomicOptions refuses (Msg 1039)");
			}

			if (!seen.Contains("TRANSACTION ISOLATION LEVEL") || !seen.Contains("LANGUAGE"))
				throw Misplaced(owner, "Atomic", "without a level or a language, which AtomicOptions demands (Msg 10784)");
		}

		void PutAtomicOption(AtomicOption option)
		{
			switch (option)
			{
				case AtomicOption.Isolation isolation:
					Word(isolation.Tran ? "TRAN ISOLATION LEVEL =" : "TRANSACTION ISOLATION LEVEL =");
					Word(isolation.Level switch
					{
						IsolationLevel.ReadUncommitted => "READ UNCOMMITTED",
						IsolationLevel.ReadCommitted   => "READ COMMITTED",
						IsolationLevel.RepeatableRead  => "REPEATABLE READ",
						IsolationLevel.Serializable    => "SERIALIZABLE",
						IsolationLevel.Snapshot        => "SNAPSHOT",
						_                              => throw NoText(isolation.Level),
					});
					break;

				case AtomicOption.Setting { Option: var setting }:
					if (setting is not { Order: null, Place: null, Partitions: null, Options.Count: 0, Words.Count: 0, EqualsOmitted: false, Value: { } value }
						|| !AtomicSettings.TryGetValue(setting.Name.Style == IdentifierStyle.Regular ? setting.Name.Text : "", out var reads) || !reads(value))
						throw Misplaced(option, "Option", "that AtomicSetting does not read: one of its five names, set to the value it takes (Msg 102)");

					PutKeyWord(setting.Name);
					Word("=");

					// DELAYED_DURABILITY's ON or OFF is a word the setting reads; every other value is written as one.
					if (Plain(setting.Name, "DELAYED_DURABILITY"))
						PutOptionValue(value);
					else
						PutExpression(value);
					break;

				default:
					throw Unwritten(option);
			}
		}

		/// <summary>
		/// T-SQL: CREATE PROCEDURE (Transact-SQL). The settings <c>AtomicSetting</c> reads, and the value each takes.
		/// </summary>
		static readonly Dictionary<string, Func<Expression, bool>> AtomicSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["DATEFIRST"]          = AtomicConstant,
			["DATEFORMAT"]         = AtomicConstant,
			["LANGUAGE"]           = AtomicConstant,
			["TEXTSIZE"]           = static value => (value is Expression.Unary { Operator: UnaryOperator.Minus, Operand: var operand } ? operand : value)
									is LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger, Text: var digits } && Unsigned(digits, NumericLiteralKind.DecimalInteger),
			["DELAYED_DURABILITY"] = static value => Bare(value) is "ON" or "OFF",
		};

		/// <summary>
		/// T-SQL: CREATE PROCEDURE (Transact-SQL). <c>AtomicConstant</c>: a number or money, negated or not, a string, a binary
		/// string, a name, a variable, NULL, or an ODBC date or time; never a name of two parts. A reserved word as a plain
		/// name is refused by <see cref="PutIdentifier"/> in T-SQL's reading, not by this place.
		/// </summary>
		static bool AtomicConstant(Expression value)
		{
			return value switch
			{
				Expression.Unary { Operator: UnaryOperator.Minus, Operand: LiteralValue.Numeric { Kind: NumericLiteralKind.Money } } => true,
				Expression.Unary { Operator: UnaryOperator.Minus, Operand: LiteralValue.Numeric number } => Unsigned(number.Text, number.Kind),
				LiteralValue.Numeric { Kind: NumericLiteralKind.Money } => true,
				LiteralValue.Numeric number => Unsigned(number.Text, number.Kind),
				LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null } => true,
				LiteralValue.Binary or LiteralValue.Null or Expression.Variable => true,
				Expression.OdbcEscape { Called: false, Value: LiteralValue.String } => true,
				Expression.Reference { Name.Parts: [var name] } => name.Text.Length > 0 && name.Style is not (IdentifierStyle.String or IdentifierStyle.NationalString),
				_ => false,
			};
		}

		/// <summary>The word a one-part plain reference is, upper-cased, or null.</summary>
		static string? Bare(Expression? value)
		{
			return value is Expression.Reference { Name.Parts: [{ Style: IdentifierStyle.Regular } word] } ? word.Text.ToUpperInvariant() : null;
		}
	}
}
