using System;
using System.Text;

using DotGram.Parsers;

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
	public static string Show(SqlNode? node)
	{
		var text = new StringBuilder();

		Write(text, node);

		return text.ToString();
	}

	static void Write(StringBuilder text, SqlNode? node)
	{
		switch (node)
		{
			case null:
				text.Append("()");
				break;

			case SqlNode.Binary(var op, var left, var right):
				text.Append('(').Append(op).Append(' ');
				Write(text, left);
				text.Append(' ');
				Write(text, right);
				text.Append(')');
				break;

			case SqlNode.Unary(var op, var operand):
				text.Append('(').Append(op).Append(' ');
				Write(text, operand);
				text.Append(')');
				break;

			case SqlNode.TruthTest(var operand, var negated, var truth):
				text.Append("(is ").Append(negated ? "not " : "").Append(truth).Append(' ');
				Write(text, operand);
				text.Append(')');
				break;

			case SqlNode.Predicate(var kind, var negated, var operands, var op, var word):
				text.Append('(').Append(kind);

				if (negated)
					text.Append(" not");

				if (op is not null)
					text.Append(' ').Append(op);

				if (word is not null)
					text.Append(" '").Append(word).Append('\'');

				Each(text, operands);
				text.Append(')');
				break;

			case SqlNode.Call(var name, var arguments, var word):
				text.Append("(call ").Append(name);

				if (word is not null)
					text.Append(" '").Append(word).Append('\'');

				Each(text, arguments);
				text.Append(')');
				break;

			case SqlNode.Case(var operand, var whens, var otherwise):
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

			case SqlNode.When(var test, var result):
				text.Append("(when ");
				Write(text, test);
				text.Append(' ');
				Write(text, result);
				text.Append(')');
				break;

			case SqlNode.Column(var name):
				text.Append("(name ").Append(name).Append(')');
				break;

			case SqlNode.Literal(var kind, var literal):
				text.Append('(').Append(kind).Append(' ').Append(literal).Append(')');
				break;

			case SqlNode.Row(var values):
				text.Append("(row");
				Each(text, values);
				text.Append(')');
				break;

			case SqlNode.Subquery(var query):
				text.Append("(query ").Append(query).Append(')');
				break;

			default:
				text.Append("(?").Append(node.GetType().Name).Append(')');
				break;
		}
	}

	static void Each(StringBuilder text, SqlNode[] nodes)
	{
		foreach (var one in nodes)
		{
			text.Append(' ');
			Write(text, one);
		}
	}
}
