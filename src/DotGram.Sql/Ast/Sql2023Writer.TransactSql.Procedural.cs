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
