using System;

namespace DotGram.QueryLanguage.Syntax;

// What the grammar builds: the shape of the text and nothing it means. Every node knows, from the moment it is
// made, how deep it nests as written, how deep it is without its parentheses, how many nodes it counts and how
// many operands its arithmetic has — from its children's — so that a text too large or too deep is refused
// before anything walks it, whatever the parser's own stacks reached.

/// <summary>A word or a symbol of the text and where it starts.</summary>
readonly struct Token(string text, int position)
{
	public string Text     { get; } = text;
	public int    Position { get; } = position;
}

abstract class Node(int position)
{
	public int Position { get; } = position;

	/// <summary>Nesting as written: parentheses, <c>!</c> and unary minus each count.</summary>
	public int Depth { get; protected init; } = 1;

	/// <summary>Nesting without the parentheses: the depth of the expression the text means.</summary>
	public int TreeDepth { get; protected init; } = 1;

	/// <summary>Nodes, parentheses not counted.</summary>
	public int Size { get; protected init; } = 1;

	protected static int DepthOf(Node? a, Node? b = null, Node? c = null)
	{
		return Math.Max(a?.Depth ?? 0, Math.Max(b?.Depth ?? 0, c?.Depth ?? 0)) + 1;
	}

	protected static int TreeDepthOf(Node? a, Node? b = null, Node? c = null)
	{
		return Math.Max(a?.TreeDepth ?? 0, Math.Max(b?.TreeDepth ?? 0, c?.TreeDepth ?? 0)) + 1;
	}

	protected static int SizeOf(Node? a, Node? b = null, Node? c = null)
	{
		return (a?.Size ?? 0) + (b?.Size ?? 0) + (c?.Size ?? 0) + 1;
	}
}

sealed class NameSyntax(string text, int position) : Node(position)
{
	public string Text { get; } = text;
}

abstract class ExprSyntax(int position) : Node(position)
{
	/// <summary>Operands of the arithmetic this node is the top of, through parentheses and unary minus; 1 for anything else.</summary>
	public int Operands { get; protected init; } = 1;
}

/// <summary>A name standing alone: a field, a <c>let</c>, a column of a <c>select</c>, or the entity's alias.</summary>
sealed class NameExpr(NameSyntax name) : ExprSyntax(name.Position)
{
	public NameSyntax Name { get; } = name;
}

enum LiteralKind
{
	Number,
	String,
	True,
	False,
	Null,
}

/// <summary>A literal, its text as written: a string with its quotes and escapes.</summary>
sealed class LiteralExpr(LiteralKind kind, string text, int position) : ExprSyntax(position)
{
	public LiteralKind Kind { get; } = kind;
	public string      Text { get; } = text;
}

/// <summary>A literal a name types: <c>timestamp '2026-01-01 10:00'</c>.</summary>
sealed class TypedLiteralExpr : ExprSyntax
{
	public TypedLiteralExpr(NameSyntax type, LiteralExpr value)
		: base(type.Position)
	{
		Type  = type;
		Value = value;
	}

	public NameSyntax  Type  { get; }
	public LiteralExpr Value { get; }
}

sealed class ParameterExpr(string name, int position) : ExprSyntax(position)
{
	public string Name { get; } = name;
}

sealed class ParenthesizedExpr : ExprSyntax
{
	public ParenthesizedExpr(ExprSyntax inner, int position)
		: base(position)
	{
		Inner     = inner;
		Depth     = DepthOf(inner);
		TreeDepth = inner.TreeDepth;
		Size      = inner.Size;
		Operands  = inner.Operands;
	}

	public ExprSyntax Inner { get; }
}

/// <summary><c>x.Name</c> or <c>x.Name(args)</c>: a member of a value, or a field after an alias.</summary>
sealed class MemberExpr : ExprSyntax
{
	public MemberExpr(ExprSyntax target, NameSyntax name, ExprSyntax[]? arguments)
		: base(name.Position)
	{
		Target    = target;
		Name      = name;
		Arguments = arguments;

		var depth     = target.Depth;
		var treeDepth = target.TreeDepth;
		var size      = target.Size + 1;

		foreach (var argument in arguments ?? [])
		{
			depth     = Math.Max(depth,     argument.Depth);
			treeDepth = Math.Max(treeDepth, argument.TreeDepth);
			size     += argument.Size;
		}

		Depth     = depth + 1;
		TreeDepth = treeDepth + 1;
		Size      = size;
	}

	public ExprSyntax    Target    { get; }
	public NameSyntax    Name      { get; }
	public ExprSyntax[]? Arguments { get; }
}

/// <summary><c>Count(*)</c>, <c>Sum(x)</c>: a function of the language, by its name.</summary>
sealed class CallExpr : ExprSyntax
{
	public CallExpr(NameSyntax name, ExprSyntax[] arguments, bool star)
		: base(name.Position)
	{
		Name      = name;
		Arguments = arguments;
		Star      = star;

		var depth     = 0;
		var treeDepth = 0;
		var size      = 1;

		foreach (var argument in arguments)
		{
			depth     = Math.Max(depth,     argument.Depth);
			treeDepth = Math.Max(treeDepth, argument.TreeDepth);
			size     += argument.Size;
		}

		Depth     = depth + 1;
		TreeDepth = treeDepth + 1;
		Size      = size;
	}

	public NameSyntax   Name      { get; }
	public ExprSyntax[] Arguments { get; }

	/// <summary>Whether the argument is <c>*</c>.</summary>
	public bool Star { get; }
}

enum UnaryOperator
{
	Negate,
	Not,
}

sealed class UnaryExpr : ExprSyntax
{
	public UnaryExpr(UnaryOperator op, ExprSyntax operand, int position)
		: base(position)
	{
		Operator  = op;
		Operand   = operand;
		Depth     = DepthOf(operand);
		TreeDepth = TreeDepthOf(operand);
		Size      = SizeOf(operand);
		Operands  = op == UnaryOperator.Negate ? operand.Operands : 1;
	}

	public UnaryOperator Operator { get; }
	public ExprSyntax    Operand  { get; }
}

enum BinaryOperator
{
	Add,
	Subtract,
	Multiply,
	Divide,
	Modulo,
	Equal,
	NotEqual,
	Less,
	LessOrEqual,
	Greater,
	GreaterOrEqual,
}

sealed class BinaryExpr : ExprSyntax
{
	public BinaryExpr(ExprSyntax left, Token op, ExprSyntax right)
		: base(op.Position)
	{
		Left      = left;
		Right     = right;
		Depth     = DepthOf(left, right);
		TreeDepth = TreeDepthOf(left, right);
		Size      = SizeOf(left, right);

		Operator = op.Text switch
		{
			"+"  => BinaryOperator.Add,
			"-"  => BinaryOperator.Subtract,
			"*"  => BinaryOperator.Multiply,
			"/"  => BinaryOperator.Divide,
			"%"  => BinaryOperator.Modulo,
			"==" => BinaryOperator.Equal,
			"!=" => BinaryOperator.NotEqual,
			"<"  => BinaryOperator.Less,
			"<=" => BinaryOperator.LessOrEqual,
			">"  => BinaryOperator.Greater,
			_    => BinaryOperator.GreaterOrEqual,
		};

		Operands = IsArithmetic ? left.Operands + right.Operands : 1;
	}

	public ExprSyntax     Left     { get; }
	public BinaryOperator Operator { get; }
	public ExprSyntax     Right    { get; }

	public bool IsArithmetic => Operator <= BinaryOperator.Modulo;
}

enum LogicalOperator
{
	And,
	Or,
}

/// <summary><c>a &amp;&amp; b &amp;&amp; c</c> as one list, so that the tree built from it can be balanced.</summary>
sealed class LogicalExpr : ExprSyntax
{
	public LogicalExpr(LogicalOperator op, ExprSyntax[] items)
		: base(items[0].Position)
	{
		Operator = op;
		Items    = items;

		var depth     = 0;
		var treeDepth = 0;
		var size      = 1;

		foreach (var item in items)
		{
			depth     = Math.Max(depth,     item.Depth);
			treeDepth = Math.Max(treeDepth, item.TreeDepth);
			size     += item.Size;
		}

		Depth     = depth + 1;
		TreeDepth = treeDepth + 1;
		Size      = size;
	}

	public LogicalOperator Operator { get; }
	public ExprSyntax[]    Items    { get; }
}

sealed class IsNullExpr : ExprSyntax
{
	public IsNullExpr(ExprSyntax operand, bool negated, int position)
		: base(position)
	{
		Operand   = operand;
		Negated   = negated;
		Depth     = DepthOf(operand);
		TreeDepth = TreeDepthOf(operand);
		Size      = SizeOf(operand);
	}

	public ExprSyntax Operand { get; }
	public bool       Negated { get; }
}

/// <summary><c>x in (a, b)</c> or <c>x in @list</c>. A list counts one node whatever its length: its length has a bound of its own.</summary>
sealed class InExpr : ExprSyntax
{
	public InExpr(ExprSyntax operand, bool negated, ExprSyntax[]? items, ParameterExpr? list, int position)
		: base(position)
	{
		Operand = operand;
		Negated = negated;
		Items   = items;
		List    = list;

		var depth     = operand.Depth;
		var treeDepth = operand.TreeDepth;

		foreach (var item in items ?? [])
		{
			depth     = Math.Max(depth,     item.Depth);
			treeDepth = Math.Max(treeDepth, item.TreeDepth);
		}

		Depth     = depth + 1;
		TreeDepth = treeDepth + 1;
		Size      = operand.Size + 2;
	}

	public ExprSyntax     Operand { get; }
	public bool           Negated { get; }
	public ExprSyntax[]?  Items   { get; }
	public ParameterExpr? List    { get; }
}

sealed class BetweenExpr : ExprSyntax
{
	public BetweenExpr(ExprSyntax operand, bool negated, ExprSyntax low, ExprSyntax high, int position)
		: base(position)
	{
		Operand   = operand;
		Negated   = negated;
		Low       = low;
		High      = high;
		Depth     = DepthOf(operand, low, high);
		TreeDepth = TreeDepthOf(operand, low, high);
		Size      = SizeOf(operand, low, high);
	}

	public ExprSyntax Operand { get; }
	public bool       Negated { get; }
	public ExprSyntax Low     { get; }
	public ExprSyntax High    { get; }
}

/// <summary><c>x like p</c>: the pattern is the host's LIKE's, and the language reads nothing in it.</summary>
sealed class LikeExpr : ExprSyntax
{
	public LikeExpr(ExprSyntax operand, bool negated, ExprSyntax pattern, int position)
		: base(position)
	{
		Operand   = operand;
		Negated   = negated;
		Pattern   = pattern;
		Depth     = DepthOf(operand, pattern);
		TreeDepth = TreeDepthOf(operand, pattern);
		Size      = SizeOf(operand, pattern);
	}

	public ExprSyntax Operand { get; }
	public bool       Negated { get; }
	public ExprSyntax Pattern { get; }
}

/// <summary>What may follow the left operand of a predicate, read before it is known to be one.</summary>
sealed class PredicateTail
{
	PredicateTail(Func<ExprSyntax, ExprSyntax> make)
	{
		_make = make;
	}

	readonly Func<ExprSyntax, ExprSyntax> _make;

	public static PredicateTail Compare(Token op, ExprSyntax right)
	{
		return new PredicateTail(left => new BinaryExpr(left, op, right));
	}

	public static PredicateTail IsNull(bool negated, int position)
	{
		return new PredicateTail(left => new IsNullExpr(left, negated, position));
	}

	public static PredicateTail In(bool negated, InTarget target, int position)
	{
		return new PredicateTail(left => new InExpr(left, negated, target.Items, target.List, position));
	}

	public static PredicateTail Between(bool negated, ExprSyntax low, ExprSyntax high, int position)
	{
		return new PredicateTail(left => new BetweenExpr(left, negated, low, high, position));
	}

	public static PredicateTail Like(bool negated, ExprSyntax pattern, int position)
	{
		return new PredicateTail(left => new LikeExpr(left, negated, pattern, position));
	}

	public ExprSyntax Apply(ExprSyntax left)
	{
		return _make(left);
	}
}

/// <summary>What <c>in</c> is followed by.</summary>
sealed class InTarget(ExprSyntax[]? items, ParameterExpr? list)
{
	public ExprSyntax[]?  Items { get; } = items;
	public ParameterExpr? List  { get; } = list;
}

/// <summary>What a name may be followed by in a primary: a string, making a typed literal, or arguments, making a call.</summary>
sealed class NameTail(LiteralExpr? text, ExprSyntax[]? arguments, bool star)
{
	public LiteralExpr?  Text      { get; } = text;
	public ExprSyntax[]? Arguments { get; } = arguments;
	public bool          Star      { get; } = star;
}

// ── Clauses ─────────────────────────────────────────────────────────────────

abstract class ClauseSyntax(int position) : Node(position);

sealed class WhereClause : ClauseSyntax
{
	public WhereClause(ExprSyntax condition, int position)
		: base(position)
	{
		Condition = condition;
		Depth     = condition.Depth;
		TreeDepth = condition.TreeDepth;
		Size      = condition.Size + 1;
	}

	public ExprSyntax Condition { get; }
}

sealed class LetClause : ClauseSyntax
{
	public LetClause(NameSyntax name, ExprSyntax value, int position)
		: base(position)
	{
		Name      = name;
		Value     = value;
		Depth     = value.Depth;
		TreeDepth = value.TreeDepth;
		Size      = value.Size + 1;
	}

	public NameSyntax Name  { get; }
	public ExprSyntax Value { get; }
}

sealed class OrderItemSyntax(ExprSyntax key, bool descending, bool? nullsFirst)
{
	public ExprSyntax Key        { get; } = key;
	public bool       Descending { get; } = descending;

	/// <summary>Where nulls go when the text says: true first, false last, null as the provider puts them.</summary>
	public bool? NullsFirst { get; } = nullsFirst;
}

sealed class OrderClause : ClauseSyntax
{
	public OrderClause(OrderItemSyntax[] items, int position)
		: base(position)
	{
		Items = items;

		var depth     = 0;
		var treeDepth = 0;
		var size      = 1;

		foreach (var item in items)
		{
			depth     = Math.Max(depth,     item.Key.Depth);
			treeDepth = Math.Max(treeDepth, item.Key.TreeDepth);
			size     += item.Key.Size;
		}

		Depth     = depth;
		TreeDepth = treeDepth;
		Size      = size;
	}

	public OrderItemSyntax[] Items { get; }
}

sealed class SelectItemSyntax(ExprSyntax value, NameSyntax? alias)
{
	public ExprSyntax  Value { get; } = value;
	public NameSyntax? Alias { get; } = alias;
}

sealed class SelectClause : ClauseSyntax
{
	public SelectClause(SelectItemSyntax[]? items, int position)
		: base(position)
	{
		Items = items;

		var depth     = 0;
		var treeDepth = 0;
		var size      = 1;

		foreach (var item in items ?? [])
		{
			depth     = Math.Max(depth,     item.Value.Depth);
			treeDepth = Math.Max(treeDepth, item.Value.TreeDepth);
			size     += item.Value.Size;
		}

		Depth     = depth;
		TreeDepth = treeDepth;
		Size      = size;
	}

	/// <summary>The items; null for <c>select *</c>.</summary>
	public SelectItemSyntax[]? Items { get; }
}

/// <summary><c>skip n</c> or <c>take n</c>: a count, a number or a parameter.</summary>
sealed class PageClause : ClauseSyntax
{
	public PageClause(bool skip, ExprSyntax count, int position)
		: base(position)
	{
		Skip  = skip;
		Count = count;
		Size  = 2;
	}

	public bool       Skip  { get; }
	public ExprSyntax Count { get; }
}

sealed class QuerySyntax(NameSyntax root, NameSyntax? alias, ClauseSyntax[] clauses)
{
	public NameSyntax     Root    { get; } = root;
	public NameSyntax?    Alias   { get; } = alias;
	public ClauseSyntax[] Clauses { get; } = clauses;
}

/// <summary>The constructions the grammar calls where a node is not made by its constructor alone.</summary>
static class Nodes
{
	public static ExprSyntax Logical(LogicalOperator op, ExprSyntax[] items)
	{
		return items.Length == 1 ? items[0] : new LogicalExpr(op, items);
	}

	public static ExprSyntax Predicated(ExprSyntax left, PredicateTail? tail)
	{
		return tail is null ? left : tail.Apply(left);
	}

	public static ExprSyntax Named(NameSyntax name, NameTail? tail)
	{
		if (tail is null)
			return new NameExpr(name);

		if (tail.Text is { } text)
			return new TypedLiteralExpr(name, text);

		return new CallExpr(name, tail.Arguments ?? [], tail.Star);
	}
}
