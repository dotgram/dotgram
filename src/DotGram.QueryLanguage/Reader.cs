using System;
using System.Collections.Generic;

using DotGram.QueryLanguage.Syntax;

namespace DotGram.QueryLanguage;

/// <summary>Reads a text into its syntax, holding it to the bounds that are about the text as written.</summary>
static class Reader
{
	public static QuerySyntax Read(string text, QlOptions options)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		if (text.Length > options.MaxTextLength)
			throw new QlException(QlErrorCode.Limit, $"The query is longer than {options.MaxTextLength} characters.", options.MaxTextLength);

		var match = QlGrammar.TryParseQuery(text);

		if (!match.IsSuccess)
		{
			var position = (int)match.Position;

			throw new QlException(QlErrorCode.Syntax, Explain(text, position, match.Error), position);
		}

		var query = match.Value;

		Check(query, options);

		return query;
	}

	/// <summary>The parser's message, or a better one where the text says what was meant.</summary>
	static string Explain(string text, int position, string? error)
	{
		// `'it''s'` is SQL's quote inside a string; here it is two strings in a row.
		if (position > 0 && position < text.Length && text[position] is '\'' or '"' && text[position - 1] == text[position])
			return "Two strings in a row: write \\' for a quote inside a string.";

		// A quote whose string the line ends before it does.
		if (position < text.Length && text[position] is '\'' or '"' && error is not null && error.StartsWith("Unexpected character", StringComparison.Ordinal))
			return "The string is not closed on its line.";

		// `select … from …` is SQL's order.
		if (position == 0 && text.TrimStart().StartsWith("select", StringComparison.OrdinalIgnoreCase))
			return "A query starts with `from` and the entity: `from orders where … select …`.";

		// SQL's habits with `*`: `o.*` and `* as x`; `*` is a list item that stands alone, without a qualifier or a name.
		if (position < text.Length && text[position] is '*' && position > 0 && text[position - 1] == '.')
			return "`*` stands alone: write `select *`, not after a name.";

		var before = text.AsSpan(0, Math.Min(position, text.Length)).TrimEnd();

		if (before.Length > 0 && before[^1] == '*' && position < text.Length && (text[position] == '*' || IsWord(text, position, "as")) && IsStarItem(before))
			return "`*` takes no name and no operator: it stands for the columns of the row; write `select *, x as y` for more.";

		// `from orders in where …`: before there was `from b in books`, a bare alias `in` was read; now it needs `as`.
		if (IsWord(text, position, "in") && StartsWithHead(before))
			return "`in` is not an alias without `as`: write `from orders as in`, or `from o in orders`.";

		return error ?? "This is not a query.";
	}

	static bool IsWord(string text, int position, string word)
	{
		return position >= 0
			&& position + word.Length <= text.Length
			&& string.Compare(text, position, word, 0, word.Length, StringComparison.OrdinalIgnoreCase) == 0
			&& (position + word.Length == text.Length || !(char.IsLetterOrDigit(text[position + word.Length]) || text[position + word.Length] == '_'));
	}

	/// <summary>Whether the text read so far ends with a `*` that is an item of a `select`: the one before it is `select` or a comma.</summary>
	static bool IsStarItem(ReadOnlySpan<char> before)
	{
		var rest = before[..^1].TrimEnd();

		return rest.EndsWith(",", StringComparison.Ordinal) || (rest.Length >= 6 && rest[^6..].Equals("select", StringComparison.OrdinalIgnoreCase));
	}

	/// <summary>Whether the text read so far is a head of the form `from x in y`, and nothing more.</summary>
	static bool StartsWithHead(ReadOnlySpan<char> before)
	{
		return System.Text.RegularExpressions.Regex.IsMatch(before.ToString(), @"^\s*from\s+\w+\s+in\s+\w+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
	}

	/// <summary>The bounds on the text as written: its nesting, its size, its arithmetic, its lists and its columns.</summary>
	static void Check(QuerySyntax query, QlOptions options)
	{
		var nodes = 1;

		foreach (var clause in query.Clauses)
		{
			if (clause.Depth > options.MaxSyntaxDepth)
				throw new QlException(QlErrorCode.Limit, $"The query nests deeper than {options.MaxSyntaxDepth}.", Deepest(clause, options.MaxSyntaxDepth, static n => n.Depth).Position);

			if (clause.TreeDepth > options.MaxTreeDepth)
				throw new QlException(QlErrorCode.Limit, $"An expression is deeper than {options.MaxTreeDepth}.", Unwrap(Deepest(clause, options.MaxTreeDepth, static n => n.TreeDepth)));

			nodes += clause.Size;

			if (nodes > options.MaxNodes)
				throw new QlException(QlErrorCode.Limit, $"The query has more than {options.MaxNodes} parts.", clause.Position);

			if (clause is SelectClause { Items: { } items } select && items.Length > options.MaxColumns)
				throw new QlException(QlErrorCode.Limit, $"A select has more than {options.MaxColumns} columns.", select.Position);

			foreach (var expression in Expressions(clause))
				Walk(expression, options);
		}
	}

	static IEnumerable<ExprSyntax> Expressions(ClauseSyntax clause)
	{
		switch (clause)
		{
			case WhereClause where:
				yield return where.Condition;
				break;

			case LetClause let:
				yield return let.Value;
				break;

			case OrderClause order:
				foreach (var item in order.Items)
					yield return item.Key;

				break;

			case SelectClause { Items: { } items }:
				foreach (var item in items)
					yield return item.Value;

				break;
		}
	}

	/// <summary>Arithmetic too long and lists too long, wherever they are. The depth is already known to be bounded.</summary>
	static void Walk(ExprSyntax node, QlOptions options)
	{
		if (node.Operands > options.MaxArithmeticOperands)
			throw new QlException(QlErrorCode.Limit, $"An arithmetic expression has more than {options.MaxArithmeticOperands} operands.", node.Position);

		CheckNot(node);

		switch (node)
		{
			case ParenthesizedExpr parenthesized:
				Walk(parenthesized.Inner, options);
				break;

			case UnaryExpr unary:
				Walk(unary.Operand, options);
				break;

			case BinaryExpr binary:
				Walk(binary.Left,  options);
				Walk(binary.Right, options);
				break;

			case LogicalExpr logical:
				foreach (var item in logical.Items)
					Walk(item, options);

				break;

			case MemberExpr member:
				Walk(member.Target, options);

				foreach (var argument in member.Arguments ?? [])
					Walk(argument, options);

				break;

			case CallExpr call:
				foreach (var argument in call.Arguments)
					Walk(argument, options);

				break;

			case IsNullExpr isNull:
				Walk(isNull.Operand, options);
				break;

			case BetweenExpr between:
				Walk(between.Operand, options);
				Walk(between.Low,     options);
				Walk(between.High,    options);
				break;

			case InExpr @in:
				if (@in.Items is { } items && items.Length > options.MaxInList)
					throw new QlException(QlErrorCode.Limit, $"An `in` list has more than {options.MaxInList} values.", @in.Position);

				Walk(@in.Operand, options);

				foreach (var item in @in.Items ?? [])
					Walk(item, options);

				break;

			case LikeExpr like:
				Walk(like.Operand, options);
				Walk(like.Pattern, options);
				break;
		}
	}

	/// <summary>
	/// Refuses <c>!x</c> without brackets on the left of a comparison, <c>is</c>, <c>in</c>, <c>between</c> or <c>like</c>.
	/// C# reads <c>!a == b</c> as <c>(!a) == b</c> and SQL's NOT as <c>!(a == b)</c>; the two differ where a value is null
	/// (<c>!flag is null</c> is never true one way and always the other), so whoever wrote it says which.
	/// </summary>
	static void CheckNot(ExprSyntax node)
	{
		var left = node switch
		{
			BinaryExpr { IsArithmetic: false } binary => binary.Left,
			IsNullExpr isNull                         => isNull.Operand,
			InExpr @in                                => @in.Operand,
			BetweenExpr between                       => between.Operand,
			LikeExpr like                             => like.Operand,
			_                                         => null,
		};

		if (left is UnaryExpr { Operator: UnaryOperator.Not } not)
			throw new QlException(QlErrorCode.Syntax, "Bracket what `!` applies to before a comparison: write `!(a == b)` or `(!a) == b`.", not.Position);
	}

	/// <summary>
	/// Where a clause too deep goes past the bound: down its deepest path to the first node that many levels
	/// below its top.
	/// </summary>
	static Node Deepest(ClauseSyntax clause, int bound, Func<Node, int> depth)
	{
		var target = depth(clause) - bound;
		var node   = (Node)clause;

		while (depth(node) > target)
		{
			Node? next = null;

			foreach (var child in Children(node))
				if (next is null || depth(child) > depth(next))
					next = child;

			if (next is null)
				break;

			node = next;
		}

		return node;
	}

	/// <summary>The position of a node, or of what it holds where it is only parentheses, which add no depth to an expression.</summary>
	static int Unwrap(Node node)
	{
		while (node is ParenthesizedExpr parenthesized)
			node = parenthesized.Inner;

		return node.Position;
	}

	static IEnumerable<Node> Children(Node node)
	{
		switch (node)
		{
			case ClauseSyntax clause:
				foreach (var expression in Expressions(clause))
					yield return expression;

				break;

			case ParenthesizedExpr parenthesized:
				yield return parenthesized.Inner;
				break;

			case UnaryExpr unary:
				yield return unary.Operand;
				break;

			case BinaryExpr binary:
				yield return binary.Left;
				yield return binary.Right;
				break;

			case LogicalExpr logical:
				foreach (var item in logical.Items)
					yield return item;

				break;

			case MemberExpr member:
				yield return member.Target;

				foreach (var argument in member.Arguments ?? [])
					yield return argument;

				break;

			case CallExpr call:
				foreach (var argument in call.Arguments)
					yield return argument;

				break;

			case IsNullExpr isNull:
				yield return isNull.Operand;
				break;

			case BetweenExpr between:
				yield return between.Operand;
				yield return between.Low;
				yield return between.High;
				break;

			case InExpr @in:
				yield return @in.Operand;

				foreach (var item in @in.Items ?? [])
					yield return item;

				break;

			case LikeExpr like:
				yield return like.Operand;
				yield return like.Pattern;
				break;
		}
	}
}
