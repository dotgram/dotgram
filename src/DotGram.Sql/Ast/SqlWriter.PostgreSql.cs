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
		/// an array, and <c>IS DOCUMENT</c>. Answers false for every other node.
		/// </summary>
		bool TryPutPostgreSql(Expression expression)
		{
			switch (expression)
			{
				case Expression.OperatorCall call:
					if (call.Left is { } left)
						PutOperand(left, Tightness.Additive);

					PutOperatorName(call.Operator, call.OperatorKeyword);
					PutOperand(call.Right, Tightness.Additive + 1);
					return true;

				case Expression.QuantifiedOperation quantified:
					PutOperand(quantified.Left, Tightness.Additive);

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
							PutOperatorName(named.Operator, named.OperatorKeyword);
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
					PutOperand(document.Value, Tightness.Additive);
					Word("IS");
					PutNot(document.Not);
					Word("DOCUMENT");
					return true;

				default:
					return false;
			}
		}

		/// <summary>An operator by its symbol, or by name in <c>OPERATOR(schema.op)</c>; a bare one may not be qualified.</summary>
		void PutOperatorName(QualifiedName name, bool keyword)
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

			Word(name[0].Text);
		}
	}
}
