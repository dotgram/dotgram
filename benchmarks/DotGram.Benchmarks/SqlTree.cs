using System;
using System.Text;

using DotGram.Parsers;
using DotGram.Parsers.Sql;

namespace DotGram.Benchmarks;

/// <summary>
/// A tree as one line of text, so that two of them can be held to each other.
/// </summary>
/// <remarks>
/// <para>
/// Written rather than compared field by field because a record holding an array
/// compares that array by reference, and every node here holds one. A renderer says what
/// differs as well as whether anything does, which is what a failing comparison needs to
/// be worth reading.
/// </para>
/// <para>
/// It is not part of what is measured: nothing on a timed path calls it.
/// </para>
/// </remarks>
static class SqlTree
{
	public static string Show(object? node)
	{
		var text = new StringBuilder();

		Write(text, node);

		return text.ToString();
	}

	static void Write(StringBuilder text, object? node)
	{
		switch (node)
		{
			case null:
				text.Append("()");
				break;

			case Expression.Or           (var left, var right): Pair(text, "Or",          left, right); break;
			case Expression.And          (var left, var right): Pair(text, "And",         left, right); break;
			case Expression.Add          (var left, var right): Pair(text, "Add",         left, right); break;
			case Expression.Subtract     (var left, var right): Pair(text, "Subtract",    left, right); break;
			case Expression.Concatenate  (var left, var right): Pair(text, "Concatenate", left, right); break;
			case Expression.Multiply     (var left, var right): Pair(text, "Multiply",    left, right); break;
			case Expression.Divide       (var left, var right): Pair(text, "Divide",      left, right); break;
			case Expression.Overlaps     (var left, var right): Pair(text, "Overlaps",    left, right); break;

			case Expression.Not(var operand):    One(text, "Not",      operand); break;
			case Expression.Negate(var operand): One(text, "Negate",   operand); break;
			case Expression.Plus(var operand):   One(text, "Identity", operand); break;
			case Expression.Exists(var query):   One(text, "Exists",   query);   break;
			case Expression.Unique(var query):   One(text, "Unique",   query);   break;

			case Expression.Comparison(var left, var op, var right):
				text.Append("(Comparison ").Append(op);
				Each(text, left, right);
				text.Append(')');
				break;

			case Expression.Quantified(var left, var op, var word, var query):
				text.Append("(Quantified ").Append(op).Append(" '").Append(word).Append('\'');
				Each(text, left, query);
				text.Append(')');
				break;

			case Expression.Between(var value, var negated, var low, var high):
				text.Append("(Between").Append(negated ? " not" : "");
				Each(text, value, low, high);
				text.Append(')');
				break;

			case Expression.In(var value, var negated, var source):
				text.Append("(In").Append(negated ? " not" : "");
				Each(text, value, source);
				text.Append(')');
				break;

			case Expression.Like(var value, var negated, var pattern, var escape):
				text.Append("(Like").Append(negated ? " not" : "");

				if (escape is null)
					Each(text, value, pattern);
				else
					Each(text, value, pattern, escape);

				text.Append(')');
				break;

			case Expression.IsNull(var value, var negated):
				text.Append("(IsNull").Append(negated ? " not" : "");
				Each(text, value);
				text.Append(')');
				break;

			case Expression.Match(var value, var word, var query):
				text.Append("(Match");

				if (word is not null)
					text.Append(" '").Append(word).Append('\'');

				Each(text, value, query);
				text.Append(')');
				break;

			case Expression.IsDistinctFrom(var left, var negated, var right):
				text.Append("(IsDistinctFrom").Append(negated ? " not" : "");
				Each(text, left, right);
				text.Append(')');
				break;

			case Expression.IsTruth(var operand, var negated, var truth):
				text.Append("(is ").Append(negated ? "not " : "").Append(truth).Append(' ');
				Write(text, operand);
				text.Append(')');
				break;

			case Expression.RoutineInvocation(var name, var arguments, var word):
				text.Append("(call ").Append(name);

				if (word is not null)
					text.Append(" '").Append(word).Append('\'');

				Each(text, arguments);
				text.Append(')');
				break;

			case Expression.Case(var operand, var whens, var otherwise):
				text.Append("(case");

				if (operand is not null)
				{
					text.Append(' ');
					Write(text, operand);
				}

				foreach (var one in whens)
				{
					text.Append(' ');
					Write(text, one);
				}

				if (otherwise is not null)
				{
					text.Append(" else ");
					Write(text, otherwise);
				}

				text.Append(')');
				break;

			case Clause.When(var test, var result):
				text.Append("(when ");
				Write(text, test);
				text.Append(' ');
				Write(text, result);
				text.Append(')');
				break;

			case Expression.ColumnReference(var name):
				text.Append("(name ").Append(name).Append(')');
				break;

			case Expression.Literal(var kind, var literal):
				text.Append('(').Append(kind).Append(' ').Append(literal).Append(')');
				break;

			case Expression.RowValueConstructor(var values):
				text.Append("(row");
				Each(text, values);
				text.Append(')');
				break;

			case Expression.Subquery(var query):
				text.Append("(sub ");
				Write(text, query);
				text.Append(')');
				break;

			case TextQuery(var query):
				text.Append("(query ").Append(query).Append(')');
				break;

			default:
				text.Append("(?").Append(node.GetType().Name).Append(')');
				break;
		}
	}

	static void Pair(StringBuilder text, string name, object left, object right)
	{
		text.Append('(').Append(name);
		Each(text, left, right);
		text.Append(')');
	}

	static void One(StringBuilder text, string name, object operand)
	{
		text.Append('(').Append(name);
		Each(text, operand);
		text.Append(')');
	}

	static void Each(StringBuilder text, params object?[] nodes)
	{
		foreach (var one in nodes)
		{
			text.Append(' ');
			Write(text, one);
		}
	}
}
