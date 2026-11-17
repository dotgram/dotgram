using System;
using System.Collections.Generic;

namespace DotGram.QueryLanguage.Syntax;

// What the grammar builds: the text's shape and nothing it means. Every node knows its depth and
// its size from the moment it is made, from its children's, so the binder can refuse a tree too
// deep or too large before it recurses into it — whatever the parser's own stack could reach.

abstract class SyntaxNode(int position)
{
	public int Position { get; } = position;

	public int Depth { get; protected init; } = 1;

	public int Size { get; protected init; } = 1;

	protected static int DepthOf(params ReadOnlySpan<SyntaxNode?> children)
	{
		var depth = 0;

		foreach (var child in children)
			if (child is not null && child.Depth > depth)
				depth = child.Depth;

		return depth + 1;
	}

	protected static int SizeOf(params ReadOnlySpan<SyntaxNode?> children)
	{
		var size = 1;

		foreach (var child in children)
			if (child is not null)
				size += child.Size;

		return size;
	}
}

abstract class ExprSyntax(int position) : SyntaxNode(position);

sealed class NameSyntax(string text, bool quoted, int position) : SyntaxNode(position)
{
	public string Text   { get; } = text;
	public bool   Quoted { get; } = quoted;
}

sealed class ColumnSyntax : ExprSyntax
{
	public ColumnSyntax(NameSyntax? qualifier, NameSyntax name)
		: base(qualifier?.Position ?? name.Position)
	{
		Qualifier = qualifier;
		Name      = name;
	}

	public NameSyntax? Qualifier { get; }
	public NameSyntax  Name      { get; }
}

enum LiteralKind
{
	Number,
	String,
	True,
	False,
	Null,
	Date,
	Timestamp,
}

sealed class LiteralSyntax(LiteralKind kind, string text, int position) : ExprSyntax(position)
{
	public LiteralKind Kind { get; } = kind;
	public string      Text { get; } = text;
}

sealed class ParameterSyntax(string name, int position) : ExprSyntax(position)
{
	public string Name { get; } = name;
}

sealed class ParenthesizedSyntax : ExprSyntax
{
	public ParenthesizedSyntax(ExprSyntax inner, int position)
		: base(position)
	{
		Inner = inner;
		Depth = DepthOf(inner);
		Size  = SizeOf(inner);
	}

	public ExprSyntax Inner { get; }
}

sealed class NegateSyntax : ExprSyntax
{
	public NegateSyntax(ExprSyntax operand, int position)
		: base(position)
	{
		Operand = operand;
		Depth   = DepthOf(operand);
		Size    = SizeOf(operand);
	}

	public ExprSyntax Operand { get; }
}

sealed class NotSyntax : ExprSyntax
{
	public NotSyntax(ExprSyntax operand, int position)
		: base(position)
	{
		Operand = operand;
		Depth   = DepthOf(operand);
		Size    = SizeOf(operand);
	}

	public ExprSyntax Operand { get; }
}

enum ArithmeticOperator
{
	Add,
	Subtract,
	Multiply,
	Divide,
	Modulo,
}

sealed class ArithmeticSyntax : ExprSyntax
{
	public ArithmeticSyntax(ExprSyntax left, ArithmeticOperator op, ExprSyntax right)
		: base(left.Position)
	{
		Left     = left;
		Operator = op;
		Right    = right;
		Depth    = DepthOf(left, right);
		Size     = SizeOf(left, right);
	}

	public ExprSyntax         Left     { get; }
	public ArithmeticOperator Operator { get; }
	public ExprSyntax         Right    { get; }
}

enum LogicalOperator
{
	And,
	Or,
}

sealed class LogicalSyntax : ExprSyntax
{
	public LogicalSyntax(LogicalOperator op, ExprSyntax[] items, int position)
		: base(position)
	{
		Operator = op;
		Items    = items;
		Depth    = DepthOf(items);
		Size     = SizeOf(items);
	}

	public LogicalOperator Operator { get; }
	public ExprSyntax[]    Items    { get; }
}

enum CompareOperator
{
	Equal,
	NotEqual,
	Less,
	LessOrEqual,
	Greater,
	GreaterOrEqual,
}

sealed class CompareSyntax : ExprSyntax
{
	public CompareSyntax(ExprSyntax left, CompareOperator op, ExprSyntax right)
		: base(left.Position)
	{
		Left     = left;
		Operator = op;
		Right    = right;
		Depth    = DepthOf(left, right);
		Size     = SizeOf(left, right);
	}

	public ExprSyntax      Left     { get; }
	public CompareOperator Operator { get; }
	public ExprSyntax      Right    { get; }
}

sealed class IsNullSyntax : ExprSyntax
{
	public IsNullSyntax(ExprSyntax operand, bool negated)
		: base(operand.Position)
	{
		Operand = operand;
		Negated = negated;
		Depth   = DepthOf(operand);
		Size    = SizeOf(operand);
	}

	public ExprSyntax Operand { get; }
	public bool       Negated { get; }
}

sealed class InSyntax : ExprSyntax
{
	public InSyntax(ExprSyntax operand, bool negated, ExprSyntax[]? items, ParameterSyntax? parameter)
		: base(operand.Position)
	{
		Operand   = operand;
		Negated   = negated;
		Items     = items;
		Parameter = parameter;

		// A list is one node of the output tree whatever its length; its length has a cap of its own.
		Depth = DepthOf(operand, parameter) + 1;
		Size  = SizeOf(operand, parameter) + (items?.Length ?? 0);

		if (items is not null)
			foreach (var item in items)
				if (item.Depth + 1 > Depth)
					Depth = item.Depth + 1;
	}

	public ExprSyntax       Operand   { get; }
	public bool             Negated   { get; }
	public ExprSyntax[]?    Items     { get; }
	public ParameterSyntax? Parameter { get; }
}

sealed class LikeSyntax : ExprSyntax
{
	public LikeSyntax(ExprSyntax operand, bool negated, ExprSyntax pattern, ExprSyntax? escape)
		: base(operand.Position)
	{
		Operand = operand;
		Negated = negated;
		Pattern = pattern;
		Escape  = escape;
		Depth   = DepthOf(operand, pattern, escape);
		Size    = SizeOf(operand, pattern, escape);
	}

	public ExprSyntax  Operand { get; }
	public bool        Negated { get; }
	public ExprSyntax  Pattern { get; }
	public ExprSyntax? Escape  { get; }
}

sealed class BetweenSyntax : ExprSyntax
{
	public BetweenSyntax(ExprSyntax operand, bool negated, ExprSyntax low, ExprSyntax high)
		: base(operand.Position)
	{
		Operand = operand;
		Negated = negated;
		Low     = low;
		High    = high;
		Depth   = DepthOf(operand, low, high);
		Size    = SizeOf(operand, low, high);
	}

	public ExprSyntax Operand { get; }
	public bool       Negated { get; }
	public ExprSyntax Low     { get; }
	public ExprSyntax High    { get; }
}

/// <summary>
/// What may follow the left operand of a predicate, read before that operand is known to be
/// one: <see cref="Nodes.Predicated"/> puts the two together.
/// </summary>
sealed class PredicateTail
{
	PredicateTail(Func<ExprSyntax, bool, ExprSyntax> make, bool negated)
	{
		_make    = make;
		_negated = negated;
	}

	readonly Func<ExprSyntax, bool, ExprSyntax> _make;
	readonly bool                               _negated;

	public static PredicateTail Compare(CompareOperator op, ExprSyntax right)
	{
		return new PredicateTail((left, _) => new CompareSyntax(left, op, right), false);
	}

	public static PredicateTail IsNull(bool negated)
	{
		return new PredicateTail((left, n) => new IsNullSyntax(left, n), negated);
	}

	public static PredicateTail InList(ExprSyntax[] items)
	{
		return new PredicateTail((left, n) => new InSyntax(left, n, items, null), false);
	}

	public static PredicateTail InParameter(ParameterSyntax parameter)
	{
		return new PredicateTail((left, n) => new InSyntax(left, n, null, parameter), false);
	}

	public static PredicateTail Like(ExprSyntax pattern, ExprSyntax? escape)
	{
		return new PredicateTail((left, n) => new LikeSyntax(left, n, pattern, escape), false);
	}

	public static PredicateTail Between(ExprSyntax low, ExprSyntax high)
	{
		return new PredicateTail((left, n) => new BetweenSyntax(left, n, low, high), false);
	}

	public PredicateTail Negate(bool negated)
	{
		return negated ? new PredicateTail(_make, true) : this;
	}

	public ExprSyntax Apply(ExprSyntax left)
	{
		return _make(left, _negated);
	}
}

sealed class SelectItemSyntax(ColumnSyntax column, NameSyntax? alias)
{
	public ColumnSyntax Column { get; } = column;
	public NameSyntax?  Alias  { get; } = alias;
}

sealed class OrderItemSyntax(ColumnSyntax column, bool descending)
{
	public ColumnSyntax Column     { get; } = column;
	public bool         Descending { get; } = descending;
}

sealed class QuerySyntax(
	SelectItemSyntax[]  select,
	NameSyntax          root,
	NameSyntax?         alias,
	ExprSyntax?         where,
	OrderItemSyntax[]?  order,
	ExprSyntax?         limit,
	ExprSyntax?         offset)
{
	/// <summary>
	/// Empty for <c>SELECT *</c>.
	/// </summary>
	public SelectItemSyntax[] Select { get; } = select;
	public NameSyntax         Root   { get; } = root;
	public NameSyntax?        Alias  { get; } = alias;
	public ExprSyntax?        Where  { get; } = where;
	public OrderItemSyntax[]  Order  { get; } = order ?? [];
	public ExprSyntax?        Limit  { get; } = limit;
	public ExprSyntax?        Offset { get; } = offset;
}

/// <summary>
/// The constructions the grammar calls where a node is not built by its constructor alone.
/// </summary>
static class Nodes
{
	public static ExprSyntax Logical(LogicalOperator op, ExprSyntax[] items, int position)
	{
		return items.Length == 1 ? items[0] : new LogicalSyntax(op, items, position);
	}

	public static ExprSyntax Predicated(ExprSyntax left, PredicateTail? tail)
	{
		return tail is null ? left : tail.Apply(left);
	}

	public static ExprSyntax Arithmetic(ExprSyntax left, string op, ExprSyntax right)
	{
		var kind = op switch
		{
			"+" => ArithmeticOperator.Add,
			"-" => ArithmeticOperator.Subtract,
			"*" => ArithmeticOperator.Multiply,
			"/" => ArithmeticOperator.Divide,
			_   => ArithmeticOperator.Modulo,
		};

		return new ArithmeticSyntax(left, kind, right);
	}

	public static CompareOperator Comparison(string op)
	{
		return op switch
		{
			"="          => CompareOperator.Equal,
			"<>" or "!=" => CompareOperator.NotEqual,
			"<"          => CompareOperator.Less,
			"<="         => CompareOperator.LessOrEqual,
			">"          => CompareOperator.Greater,
			_            => CompareOperator.GreaterOrEqual,
		};
	}

	public static bool Descending(string? direction)
	{
		return direction is not null && direction.Equals("DESC", StringComparison.OrdinalIgnoreCase);
	}

	public static string Unquote(string quoted, char quote)
	{
		var inner = quoted.Substring(1, quoted.Length - 2);

		return inner.IndexOf(quote) < 0 ? inner : inner.Replace(new string(quote, 2), quote.ToString());
	}
}
