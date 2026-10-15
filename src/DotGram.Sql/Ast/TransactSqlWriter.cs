using System;
using System.Collections.Generic;

namespace DotGram.Sql.Ast;

/// <summary>T-SQL's text of the common tree.</summary>
/// <remarks>
/// The same text <see cref="Sql2023Writer.Write(ISqlNode)"/> prints, and in addition a refusal of the trees T-SQL would read as
/// another tree in three ways: a reserved word written plain where a name stands, a value where a condition stands and
/// a condition where a value stands, and a signed number where T-SQL reads an expression and the reverse. It promises no
/// more than that: a tree it writes is one the T-SQL grammar's own checks, beyond those three, may still refuse.
/// </remarks>
public static class TransactSqlWriter
{
	/// <summary>The tree's text, as T-SQL reads it.</summary>
	public static string Write(ISqlNode node)
	{
		return Sql2023Writer.Write(node, transactSql: true);
	}
}

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL: Reserved Keywords (Transact-SQL), a page syntax.md does not transcribe. The words T-SQL reads as key words
		/// and never as a plain name: exactly the grammar's <c>TSqlReserved</c>, which <c>TransactSqlWriterTests</c> holds
		/// this list to both ways.
		/// </summary>
		internal static readonly HashSet<string> TransactSqlReserved = new(StringComparer.OrdinalIgnoreCase)
		{
			"ADD", "ALL", "ALTER", "AND", "ANY", "AS", "ASC", "AUTHORIZATION", "BACKUP", "BEGIN", "BETWEEN", "BREAK",
			"BROWSE", "BULK", "BY", "CASCADE", "CASE", "CHECK", "CHECKPOINT", "CLOSE", "CLUSTERED", "COALESCE", "COLLATE",
			"COLUMN", "COMMIT", "COMPUTE", "CONSTRAINT", "CONTAINS", "CONTAINSTABLE", "CONTINUE", "CONVERT", "CREATE",
			"CROSS", "CURRENT", "CURRENT_DATE", "CURRENT_TIME", "CURRENT_TIMESTAMP", "CURRENT_USER", "CURSOR", "DATABASE",
			"DBCC", "DEALLOCATE", "DECLARE", "DEFAULT", "DELETE", "DENY", "DESC", "DISTINCT", "DISTRIBUTED", "DOUBLE", "DROP",
			"ELSE", "END", "ERRLVL", "ESCAPE", "EXCEPT", "EXEC", "EXECUTE", "EXISTS", "EXIT", "FETCH", "FILE", "FILLFACTOR",
			"FOR", "FOREIGN", "FREETEXT", "FREETEXTTABLE", "FROM", "FULL", "FUNCTION", "GOTO", "GRANT", "GROUP", "HAVING",
			"HOLDLOCK", "IDENTITY", "IDENTITYCOL", "IDENTITY_INSERT", "IF", "IN", "INDEX", "INNER", "INSERT", "INTERSECT",
			"INTO", "IS", "JOIN", "KEY", "KILL", "LEFT", "LIKE", "LINENO", "NATIONAL", "NOCHECK", "NONCLUSTERED", "NOT",
			"NULL", "NULLIF", "OF", "OFF", "OFFSETS", "ON", "OPEN", "OPENDATASOURCE", "OPENQUERY", "OPENROWSET", "OPENXML",
			"OPTION", "OR", "ORDER", "OUTER", "OVER", "PERCENT", "PLAN", "PRIMARY", "PRINT", "PROC", "PROCEDURE", "PUBLIC",
			"RAISERROR", "READ", "READTEXT", "RECONFIGURE", "REFERENCES", "REPLICATION", "RESTORE", "RESTRICT", "RETURN",
			"REVOKE", "RIGHT", "ROLLBACK", "ROWCOUNT", "ROWGUIDCOL", "RULE", "SAVE", "SCHEMA", "SELECT", "SESSION_USER",
			"SET", "SETUSER", "SHUTDOWN", "SOME", "STATISTICS", "SYSTEM_USER", "TABLE", "TEXTSIZE", "THEN", "TO", "TOP",
			"TRAN", "TRANSACTION", "TRIGGER", "TRUNCATE", "TSEQUAL", "UNION", "UNIQUE", "UPDATE", "UPDATETEXT", "USE", "USER",
			"VALUES", "VARYING", "VIEW", "WAITFOR", "WHEN", "WHERE", "WHILE", "WITH", "WRITETEXT", "EXTERNAL", "MERGE",
			"PIVOT", "REVERT", "SEMANTICKEYPHRASETABLE", "SEMANTICSIMILARITYDETAILSTABLE", "SEMANTICSIMILARITYTABLE",
			"TABLESAMPLE", "TRY_CONVERT", "UNPIVOT"
		};

		/// <summary>
		/// In T-SQL's reading, a name written plain that is a reserved word: T-SQL reads it as the key word, and written
		/// bracketed or quoted it is a name.
		/// </summary>
		void CheckReserved(Identifier identifier)
		{
			if (transactSql && identifier.Style == IdentifierStyle.Regular && TransactSqlReserved.Contains(identifier.Text))
				throw Misplaced(identifier, "Text", "a reserved word written plain, which T-SQL reads as the key word; written bracketed or quoted it is a name");
		}

		/// <summary>
		/// A key word the grammar reads at this place into a name or a value slot — an option's word, a catalogue's name, a
		/// function T-SQL reserves the name of — which the place has checked against its own catalogue: written plain, as
		/// <see cref="PutIdentifier"/> writes a name, and never held to the reserved words.
		/// </summary>
		void PutKeyWord(Identifier word)
		{
			if (word.Style != IdentifierStyle.Regular || word.UnicodeEscape is not null)
				throw Misplaced(word, "Style", "other than a plain word, where the grammar reads a key word");

			Word(word.Text);
		}

		/// <summary>
		/// Whether the next expression stands where T-SQL reads a search condition: set by <see cref="PutCondition"/>, and by a
		/// condition's AND, OR, NOT and bracket for their operands, and taken by the next expression written.
		/// </summary>
		bool condition;

		/// <summary>
		/// T-SQL: Search Condition (Transact-SQL). A place the grammar reads <c>SearchCondition</c> at — IF, WHILE, WHERE,
		/// HAVING, ON, WHEN, CHECK, … — whose expression is a condition: in T-SQL's reading a value there is refused (Msg 4145).
		/// </summary>
		void PutCondition(ISqlNode owner, Expression expression)
		{
			condition = transactSql;
			PutExpression(expression);
		}

		/// <summary>
		/// The flag the next expression takes: in T-SQL's reading a condition where one is asked for and a value everywhere
		/// else, each refused in the other's place; whether this one is a condition, which a logical one passes on.
		/// </summary>
		bool TakeCondition(Expression expression)
		{
			var wanted = condition;

			condition = false;

			if (!transactSql)
				return false;

			var found = IsCondition(expression);

			if (wanted && !found)
				throw Misplaced(expression, "a value", "where T-SQL reads a search condition (Msg 4145)");

			if (!wanted && found)
				throw Misplaced(expression, "a condition", "where T-SQL reads a value");

			return found;
		}

		/// <summary>An operand of AND, OR, NOT or a bracket: a condition where the expression it stands in is one.</summary>
		void PassCondition(bool logical)
		{
			condition = logical;
		}

		/// <summary>
		/// T-SQL: Search Condition (Transact-SQL), Predicates (Transact-SQL). What T-SQL's grammar reads as a condition: a
		/// comparison, BETWEEN, LIKE, IN, IS [NOT] NULL, IS [NOT] DISTINCT FROM, a quantified comparison, EXISTS, UNIQUE,
		/// MATCH, CONTAINS and FREETEXT, TSEQUAL, a trigger's UPDATE(c) and REGEXP_LIKE (<c>TSqlPredicate</c>,
		/// <c>FullTextPredicate</c>); AND, OR and NOT; and a condition in brackets.
		/// </summary>
		static bool IsCondition(Expression expression)
		{
			// A bracket is what it holds, however deep: unwrapped in a loop, which no depth of brackets can run out of stack in.
			while (expression is Expression.Parenthesized { Value: var inner })
				expression = inner;

			return expression switch
			{
				Expression.Comparison or Expression.Between or Expression.Like or Expression.In or Expression.IsNull or Expression.IsDistinct
					or Expression.QuantifiedComparison or Expression.Exists or Expression.Unique or Expression.GraphMatch => true,
				Expression.Invocation { Name: [{ Style: IdentifierStyle.Regular } name] } => Plain(name, "CONTAINS", "FREETEXT", "TSEQUAL", "UPDATE", "REGEXP_LIKE"),
				Expression.Binary { Operator: BinaryOperator.And or BinaryOperator.Or } => true,
				Expression.Unary { Operator: UnaryOperator.Not } => true,
				_ => false,
			};
		}

		/// <summary>
		/// The reserved words the grammar reads as a function's name before its bracket (<c>LeftRightCall</c>, the conversions,
		/// <c>IDENTITY</c>, the CASE abbreviations, the rowset functions, <c>FullTextName</c>, <c>FullTextTableName</c>,
		/// <c>SemanticName</c>, <c>TSqlPredicate</c>'s TSEQUAL and UPDATE).
		/// </summary>
		static readonly HashSet<string> ReservedCalls = new(StringComparer.OrdinalIgnoreCase)
		{
			"LEFT", "RIGHT", "CONVERT", "TRY_CONVERT", "IDENTITY", "NULLIF", "COALESCE", "OPENQUERY", "OPENROWSET", "OPENDATASOURCE",
			"OPENXML", "CONTAINS", "FREETEXT", "CONTAINSTABLE", "FREETEXTTABLE", "SEMANTICKEYPHRASETABLE", "SEMANTICSIMILARITYTABLE",
			"SEMANTICSIMILARITYDETAILSTABLE", "TSEQUAL", "UPDATE",
		};

		/// <summary>
		/// A call's name: one plain part that the grammar reads as a function's name although T-SQL reserves it, or any plain
		/// word where the place reads one, through <see cref="PutKeyWord"/>; every other name as a name, whose parts are held
		/// to the reserved words (<c>dbo.LEFT(1)</c> is refused).
		/// </summary>
		void PutCallName(QualifiedName name, bool anyWord)
		{
			if (name.Count == 1 && name[0] is { Style: IdentifierStyle.Regular, UnicodeEscape: null } word && (anyWord || ReservedCalls.Contains(word.Text)))
				PutKeyWord(word);
			else
				PutName(name);
		}

		/// <summary>
		/// A column by name; its last part, where it is IDENTITYCOL or ROWGUIDCOL, is the key word the grammar reads as the
		/// table's identity or row-GUID column under any prefix (<c>RowColumnName</c>).
		/// </summary>
		void PutReference(QualifiedName name)
		{
			if (name.Count > 0 && name[name.Count - 1] is { Style: IdentifierStyle.Regular, UnicodeEscape: null } last && (Plain(last, "IDENTITYCOL", "ROWGUIDCOL")))
			{
				for (var at = 0; at < name.Count - 1; at++)
				{
					PutIdentifier(name[at]);
					Tight(".");
					Hold();
				}

				PutKeyWord(last);
			}
			else
				PutName(name);
		}

		/// <summary>
		/// An option's name, a further word of it or a unit, which the grammar reads as a word (<c>OptionName</c>,
		/// <c>OptionUnit</c>, a place's catalogue): a plain one through <see cref="PutKeyWord"/>, any other as the name it is.
		/// </summary>
		void PutOptionWord(Identifier word)
		{
			if (word.Style == IdentifierStyle.Regular && word.UnicodeEscape is null)
				PutKeyWord(word);
			else
				PutIdentifier(word);
		}

		/// <summary>
		/// An option's value: a plain word, which the grammar reads as one (<c>OptionValue</c>'s ON, OFF and word, a place's
		/// catalogue), through <see cref="PutKeyWord"/>; any other as a value.
		/// </summary>
		void PutOptionValue(Expression value)
		{
			if (value is Expression.Reference { Name: [{ Style: IdentifierStyle.Regular, UnicodeEscape: null } word] })
				PutKeyWord(word);
			else
				PutExpression(value);
		}
	}
}
