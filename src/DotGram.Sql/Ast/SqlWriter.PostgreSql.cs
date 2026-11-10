using System;

namespace DotGram.Sql.Ast;

public static partial class SqlWriter
{
	sealed partial class Text
	{
		/// <summary>Whether the text is for PostgreSQL.</summary>
		bool postgreSql => dialect == SqlDialect.PostgreSql;

		/// <summary>
		/// PostgreSQL's own expressions (SqlAst.PostgreSql.cs): an operator called by name, a quantified operation over
		/// an array, and <c>IS DOCUMENT</c>, each refused for any other reading, which has no text for it. Answers
		/// false for every other node.
		/// </summary>
		bool TryPutPostgreSql(Expression expression)
		{
			switch (expression)
			{
				case Expression.OperatorCall call:
					OnlyPostgreSql(call);

					// A prefix operator takes its operand above the operator level, `|/ 25 + 11` but `(@ a) ~ b`; an infix one is left-associative.
					if (call.Left is { } left)
						PutOperand(left, PostgreSqlTightness.Operator);

					PutOperatorName(call.Operator, call.OperatorKeyword, false);
					PutOperand(call.Right, call.Right is Expression.OperatorCall { Left: null } ? PostgreSqlTightness.Operator : PostgreSqlTightness.Operator + 1);
					return true;

				case Expression.QuantifiedOperation quantified:
					OnlyPostgreSql(quantified);
					PutOperand(quantified.Left, PostgreSqlTightness.Operator);

					switch (quantified.Operator)
					{
						case QuantifiedOperator.Comparison comparison:
							Word(Comparison(comparison.Operator));
							break;

						case QuantifiedOperator.Like like:
							PutNot(like.Not);
							Word(like.Kind switch { LikeKind.Like => "LIKE", LikeKind.ILike => "ILIKE", _ => throw Misplaced(like, "Kind", "that no quantified operation takes: LIKE and ILIKE only") });
							break;

						case QuantifiedOperator.Named named:
							PutOperatorName(named.Operator, named.OperatorKeyword, true);
							break;

						default:
							throw Unwritten(quantified.Operator);
					}

					Word(quantified.Quantifier switch { Quantifier.All => "ALL", Quantifier.Some => "SOME", Quantifier.Any => "ANY", _ => throw NoText(quantified.Quantifier) });

					// The array stands in brackets of its own; a subquery brings its own.
					if (quantified.Right is Expression.Subquery subquery)
					{
						PutSubquery(subquery.Query);
					}
					else
					{
						Call();
						PutExpression(quantified.Right);
						Close();
					}

					return true;

				case Expression.IsDocument document:
					OnlyPostgreSql(document);
					PutOperand(document.Value, PostgreSqlTightness.Tested);
					Word("IS");
					PutNot(document.Not);
					Word("DOCUMENT");
					return true;

				default:
					return false;
			}
		}

		/// <summary>A node only PostgreSQL reads is refused for any other reading, which has no text for it.</summary>
		void OnlyPostgreSql(ISqlNode node)
		{
			if (!postgreSql)
				throw Misplaced(node, "Kind", "that only PostgreSQL reads: " + node.GetType().Name + " has no text in another reading");
		}

		/// <summary>
		/// An operator by its symbol, or by name in <c>OPERATOR(schema.op)</c>; a bare one may not be qualified, and its symbol
		/// is one the grammar reads as an operator by name — not a run no operator token holds, and not a symbol the grammar
		/// reads as the standard's node (`+`, `=`, `||`), which would read back as that node. A quantified form reads the
		/// arithmetic symbols by name too, `a + ANY (…)`, and refuses the comparisons alone, which are its own records.
		/// </summary>
		void PutOperatorName(QualifiedName name, bool keyword, bool quantified, bool any = false)
		{
			if (name.Count == 0)
				throw Misplaced(name, "Count", "of no parts: an operator has a symbol");

			if (keyword)
			{
				Word("OPERATOR");
				Call();
				PutName(name);
				Close();
				return;
			}

			if (name.Count != 1)
				throw Misplaced(name, "Count", "above one on an operator written bare: a qualified operator is written OPERATOR(schema.op)");

			var symbol = name[0].Text;
			// An exclusion's operator is any_operator: every symbol, the comparisons and the arithmetic among them.
			var own    = any ? false
				: quantified
				? symbol is "=" or "<>" or "!=" or "<" or ">" or "<=" or ">=" or "=>"
				: symbol == "||" || !PostgreSql.Nodes.IsOperator(symbol);

			if (symbol.Length == 0 || own || !PostgreSql.Nodes.IsOperatorRun(symbol))
				throw Misplaced(name, "Text", "that PostgreSQL does not read as an operator by name here: a symbol of the standard's own nodes, or no run of operator characters");

			Word(symbol);
		}
	}
}
