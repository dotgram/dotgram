using System;
using System.Collections.Generic;
using System.Linq;

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

					Word("IF");
					PutExpression(@if.Condition);
					PutStatement(@if.Then);

					if (@if.Else is { } other)
					{
						Word("ELSE");
						PutStatement(other);
					}

					return true;

				// T-SQL: WHILE (Transact-SQL).
				case Statement.While loop:
					Word("WHILE");
					PutExpression(loop.Condition);
					PutStatement(loop.Body);
					return true;

				// T-SQL: TRY...CATCH (Transact-SQL). A TRY holds one statement at least (Msg 102), a CATCH may hold none.
				case Statement.TryCatch tryCatch:
					if (tryCatch.Try.Count == 0)
						throw Empty(tryCatch, "Try");

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

				case Statement.Label label:
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
					PutVariableValue(set, set.Value);
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
						PutVariableValue(typed, value);
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
		/// T-SQL: SET @local_variable (Transact-SQL). A value given to a variable (<c>SetValue</c>): never DEFAULT
		/// (<c>Msg 4119</c>), and never a comparison, which reads as a column assigned beside the variable.
		/// </summary>
		void PutVariableValue(ISqlNode owner, Expression value)
		{
			if (value is Expression.Default or Expression.Comparison)
				throw Misplaced(owner, "Value", "DEFAULT or a comparison, which no variable is given");

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
					if (setting is not { Order: null, Partitions: null, Options.Count: 0, Words.Count: 0, EqualsOmitted: false, Value: { } value }
						|| !AtomicSettings.TryGetValue(setting.Name.Style == IdentifierStyle.Regular ? setting.Name.Text : "", out var reads) || !reads(value))
						throw Misplaced(option, "Option", "that AtomicSetting does not read: one of its five names, set to the value it takes (Msg 102)");

					PutIdentifier(setting.Name);
					Word("=");
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
			["TEXTSIZE"]           = static value => value is LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger }
									or Expression.Unary { Operator: UnaryOperator.Minus, Operand: LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger } },
			["DELAYED_DURABILITY"] = static value => Bare(value) is "ON" or "OFF",
		};

		/// <summary>
		/// T-SQL: CREATE PROCEDURE (Transact-SQL). <c>AtomicConstant</c>: a number or money, negated or not, a string, a binary
		/// string, a name, a variable, NULL, or an ODBC date or time; never DEFAULT, ON or a name of two parts.
		/// </summary>
		static bool AtomicConstant(Expression value)
		{
			return value switch
			{
				Expression.Unary { Operator: UnaryOperator.Minus, Operand: LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger or NumericLiteralKind.Decimal or NumericLiteralKind.Approximate or NumericLiteralKind.Money } } => true,
				LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger or NumericLiteralKind.Decimal or NumericLiteralKind.Approximate or NumericLiteralKind.Money } => true,
				LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null } => true,
				LiteralValue.Binary or LiteralValue.Null or Expression.Variable => true,
				Expression.OdbcEscape { Called: false, Value: LiteralValue.String } => true,
				Expression.Reference { Name.Parts: [var name] } => name.Style != IdentifierStyle.Regular || Bare(value) is not ("DEFAULT" or "ON" or "OFF" or "NULL"),
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
