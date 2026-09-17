using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Handwritten;

/// <summary>
/// What a value expression can still be, as the BNF of ISO/IEC 9075-2:2023 types it: a set of its
/// towers, carried beside the node as the expression is read.
/// </summary>
/// <remarks>
/// <para>
/// <b>The BNF types its expressions, and a parser has no types.</b> A numeric value expression, a
/// character one, a datetime one and an interval one are towers of their own that meet only in a
/// <c>&lt;value expression primary&gt;</c>, so <c>a || b + c</c> is none of them and the BNF refuses
/// it; <c>x + y AT LOCAL</c> is a datetime — an interval, a plus and a datetime term — and
/// <c>x - y AT LOCAL</c> is nothing. Reading the towers one after another is no way to do it: each
/// would take the first operand and stop. So the reading is the shape they share — operands, and
/// the operators between them — and this says which towers the whole still belongs to. Where none
/// is left, the input is refused.
/// </para>
/// <para>
/// This is the same reasoning the generated parser carries in <c>Towers</c>, because it is a
/// property of the BNF and not of either parser: what differs between the two is how the operands
/// are read, which is the thing being measured.
/// </para>
/// <para>
/// <b>One production cuts across the shape.</b> An <c>&lt;array element reference&gt;</c> subscripts
/// an array value expression, and a concatenation is one: <c>x || y[1]</c> is <c>x</c> joined to
/// <c>y[1]</c>, and it is <c>(x || y)[1]</c> too, a primary that a sign may stand before and an
/// interval qualifier after. So a run of <c>||</c> that ends in a subscript folds into one operand
/// before the operators are asked. The tree says the first: a subscript binds to its primary.
/// </para>
/// </remarks>
static class SqlTowers
{
	// ── The towers ─────────────────────────────────────────────────────────────

	public const int Numeric     = 1 << 0;
	public const int Interval    = 1 << 1;
	public const int Datetime    = 1 << 2;
	public const int Character   = 1 << 3;
	public const int Binary      = 1 << 4;
	public const int Array       = 1 << 5;
	public const int Multiset    = 1 << 6;
	public const int Json        = 1 << 7;
	public const int UserDefined = 1 << 8;

	/// <summary>An explicit row value constructor: `(a, b)`, `ROW(a)`.</summary>
	public const int Row = 1 << 9;

	/// <summary>A boolean value expression can be this: a <c>&lt;boolean predicand&gt;</c>, or more.</summary>
	public const int Truth = 1 << 10;

	/// <summary>A <c>&lt;basic identifier chain&gt;</c>, which a period predicate names a period by.</summary>
	public const int Chain = 1 << 11;

	/// <summary>Only a boolean can be this: a predicate, or `AND`, `OR`, `NOT` or `IS` around one.</summary>
	public const int Logical = 1 << 12;

	/// <summary>A <c>&lt;non-parenthesized value expression primary&gt;</c>.</summary>
	public const int Bare = 1 << 13;

	/// <summary>A <c>&lt;parenthesized value expression&gt;</c>.</summary>
	public const int Parenthesized = 1 << 14;

	/// <summary>A primary with a subscript among its steps.</summary>
	public const int Subscript = 1 << 15;

	/// <summary>A <c>&lt;routine invocation&gt;</c> and nothing more, which `TABLE (…)` reads as a polymorphic table function's.</summary>
	public const int Invoked = 1 << 16;

	/// <summary>A primary whose last step is `.*`, which an <c>&lt;all fields reference&gt;</c> may name columns after.</summary>
	public const int Starred = 1 << 17;

	public const int String = Character | Binary;

	/// <summary>A <c>&lt;value expression primary&gt;</c>, which has every type.</summary>
	public const int Value = Numeric | Interval | Datetime | Character | Binary | Array | Multiset | Json | UserDefined;

	/// <summary>Whatever a row value predicand is.</summary>
	public const int Any = Value | Row;

	/// <summary>A node, and what the towers say it can still be.</summary>
	public readonly record struct Typed(Expression Node, int Roles);

	// ── A primary ──────────────────────────────────────────────────────────────

	/// <summary>
	/// A step after a primary — `.name`, `->name`, `[i]`, `.*` — and what it makes of the primary. The
	/// node is the step with its target left empty, filled in by <see cref="Apply"/>.
	/// </summary>
	public readonly record struct Step(int Roles, Expression Node);

	/// <summary>
	/// What steps after a primary make of it, which is a value expression primary like any, and not a
	/// parenthesized one; nothing, where there are none.
	/// </summary>
	public static int Steps(List<Step>? steps)
	{
		var made = 0;

		foreach (var step in steps ?? [])
			made = made & ~Starred | step.Roles;

		return made;
	}

	public static int Stepped(int primary, int steps) =>
		steps == 0 ? primary : Value | Truth | Bare | steps;

	/// <summary>Steps read, and what they make of a primary.</summary>
	public readonly record struct Stepping(int Roles, List<Step>? Steps);

	/// <summary>A primary and the steps after it, each taking what stands before it as its target.</summary>
	public static Typed Stepped(Typed primary, Stepping steps) =>
		new(Apply(primary.Node, steps.Steps), Stepped(primary.Roles, steps.Roles));

	/// <summary>A value function's subscript and the steps after it.</summary>
	public static Stepping Subscripted(Step first, List<Step>? rest)
	{
		var all = new List<Step> { first };

		if (rest is not null)
			all.AddRange(rest);

		return new Stepping(Subscript | Steps(rest), all);
	}

	/// <summary>
	/// What follows a bracket's value expression: the closing bracket, the rest of a row, or `AS` and a
	/// type, a method and its arguments.
	/// </summary>
	public readonly record struct Bracket(int Kind, List<Expression>? Rest = null, DataType? Type = null, Identifier? Method = null, IReadOnlyList<Argument>? Arguments = null);

	/// <summary>What follows a name: the rest of an identifier chain, or `OVER` and a window.</summary>
	public readonly record struct Chained(bool Measure, List<Identifier>? Rest, WindowReference? Over = null);

	/// <summary>A predicate's second part: the predicate with its left side empty, and what it takes there.</summary>
	public readonly record struct Tail(int Roles, Expression? Node);

	public static Expression Apply(Expression primary, List<Step>? steps)
	{
		foreach (var step in steps ?? [])
			primary = step.Node switch
			{
				Expression.Member m   => m with { Target = primary },
				Expression.Element e  => e with { Collection = primary },
				Expression.Wildcard w => w with { Target = primary },
				Expression.JsonAccessor j => j with { Target = primary },
				_                     => throw new ArgumentOutOfRangeException(nameof(steps), step.Node, "A step this method cannot complete."),
			};

		return primary;
	}

	// ── An operand ─────────────────────────────────────────────────────────────

	/// <summary>What follows a primary: an interval qualifier, a time zone, a collate clause, a type's name, and a collate clause after that.</summary>
	public const int Qualified = 1, Zoned = 2, Collated = 3, Typed_ = 4, TypedCollated = 5;

	/// <summary>
	/// What follows a primary, and the node it makes of it, the value left empty: an interval qualifier,
	/// a time zone, a collate clause, `.SPECIFICTYPE`, or `.SPECIFICTYPE` and a collate clause.
	/// </summary>
	public readonly record struct After(int Kind, Expression? Node);

	/// <summary>A primary, with its sign and what follows it.</summary>
	public readonly record struct Piece(UnaryOperator? Sign, Typed Primary, After Postfix)
	{
		public bool Signed => Sign is not null;
	}

	/// <summary>
	/// What an operand can be. A sign leaves a numeric and an interval factor; an interval qualifier
	/// makes an interval primary of a value expression primary, and `.SPECIFICTYPE` a character value
	/// function of one; `AT` keeps a datetime factor and `COLLATE` a character one. Only a primary left
	/// alone keeps being a predicand, a chain, a primary of its kind, or an explicit row.
	/// </summary>
	public static int Roles(Piece piece)
	{
		var primary = piece.Primary.Roles;

		var roles = piece.Postfix.Kind switch
		{
			Qualified     => (primary & Value) == Value ? Interval : 0,
			Zoned         => primary & Datetime,
			Collated      => primary & Character,
			Typed_        => (primary & Value) == Value ? String : 0,
			TypedCollated => (primary & Value) == Value ? Character : 0,
			_             => primary & Value,
		};

		if (piece.Signed)
			return roles & (Numeric | Interval);

		return piece.Postfix.Kind == 0 ? roles | primary & (Truth | Chain | Bare | Parenthesized | Row | Invoked | Starred) : roles;
	}

	/// <summary>An operand's node: its primary, what follows the primary, and its sign around both.</summary>
	public static Expression Node(Piece piece)
	{
		var value = Follow(piece.Primary.Node, piece.Postfix.Node);

		return piece.Sign is { } sign ? new Expression.Unary(sign, value) : value;
	}

	static Expression Follow(Expression value, Expression? after) =>
		after switch
		{
			null                                   => value,
			Expression.IntervalQualified q         => q with { Value = value },
			Expression.AtTimeZone z                => z with { Value = value },
			Expression.Collate { Value: null } c   => c with { Value = value },
			Expression.Collate c                   => c with { Value = Follow(value, c.Value) },
			Expression.Member m                    => m with { Target = value },
			_                                      => throw new ArgumentOutOfRangeException(nameof(after), after, "What follows a primary this method cannot complete."),
		};

	// ── The operators ──────────────────────────────────────────────────────────

	public const int Concatenate = 1, MultisetOperator = 2, Times = 3, Divided = 4, Plus = 5, Minus = 6;

	/// <summary>An operator: which, and for a multiset's, which of the three and its quantifier.</summary>
	public readonly record struct Op(int Kind, Ast.MultisetOperator Multiset = default, SetQuantifier? Quantifier = null);

	public readonly record struct Operated(Op Operator, Piece Operand);

	/// <summary>Operands and the operators between them: the node, and what the towers say it can be — nothing, where it can be nothing.</summary>
	public static Typed Expression(Piece first, List<Operated>? rest)
	{
		var roles = Common(first, rest);

		return new Typed(roles == 0 ? first.Primary.Node : Build(first, rest), roles);
	}

	/// <summary>
	/// What the expression can be, or nothing where it can be nothing. An explicit row is read where a
	/// bracket is, and is something only alone: no operator joins one.
	/// </summary>
	public static int Common(Piece first, List<Operated>? rest)
	{
		var pieces    = new List<Piece> { first };
		var operators = new List<int>();

		foreach (var (op, operand) in rest ?? [])
		{
			operators.Add(op.Kind);
			pieces.Add(operand);
		}

		Fold(pieces, operators);

		var roles = Join(pieces, operators);

		return (roles & (Value | Row)) != 0 ? roles : 0;
	}

	/// <summary>
	/// The node of operands and operators: `*`, `/` and `MULTISET INTERSECT` bind tighter than `+`, `-`,
	/// `||`, `MULTISET UNION` and `MULTISET EXCEPT`, and each strength is read from the left.
	/// </summary>
	static Expression Build(Piece first, List<Operated>? rest)
	{
		if (rest is not { Count: > 0 })
			return Node(first);

		var terms = new List<Expression> { Node(first) };
		var loose = new List<Op>();

		foreach (var (op, operand) in rest)
		{
			if (op.Kind is Times or Divided || op.Kind == MultisetOperator && op.Multiset == Ast.MultisetOperator.Intersect)
				terms[terms.Count - 1] = Combine(terms[terms.Count - 1], op, Node(operand));
			else
			{
				loose.Add(op);
				terms.Add(Node(operand));
			}
		}

		var node = terms[0];

		for (var at = 0; at < loose.Count; at++)
			node = Combine(node, loose[at], terms[at + 1]);

		return node;
	}

	static Expression Combine(Expression left, Op op, Expression right) =>
		op.Kind switch
		{
			Concatenate      => new Expression.Binary(left, BinaryOperator.Concatenate, right),
			MultisetOperator => new Expression.MultisetOperation(left, op.Multiset, op.Quantifier, right),
			Times            => new Expression.Binary(left, BinaryOperator.Multiply, right),
			Divided          => new Expression.Binary(left, BinaryOperator.Divide, right),
			Plus             => new Expression.Binary(left, BinaryOperator.Add, right),
			_                => new Expression.Binary(left, BinaryOperator.Subtract, right),
		};

	/// <summary>
	/// A run of `||` that ends in a subscripted operand is also one array element reference: the run
	/// is the array, the sign of its first operand stands before the whole and what follows its last
	/// after it. Folding takes operators away and constrains nothing, so a run is taken as far as it
	/// goes: over array primaries, unsigned and with nothing after them, and over one signed one
	/// where the run begins.
	/// </summary>
	static void Fold(List<Piece> pieces, List<int> operators)
	{
		for (var last = 1; last < pieces.Count; last++)
		{
			if ((pieces[last].Primary.Roles & Subscript) == 0 || pieces[last].Signed || operators[last - 1] != Concatenate)
				continue;

			var first = last;

			while (first > 0 && operators[first - 1] == Concatenate && pieces[first - 1] is { Signed: false, Postfix.Kind: 0 } && (pieces[first - 1].Primary.Roles & Array) != 0)
				first--;

			if (first > 0 && operators[first - 1] == Concatenate && pieces[first - 1] is { Signed: true, Postfix.Kind: 0 } && (pieces[first - 1].Primary.Roles & Array) != 0)
				first--;

			if (first == last)
				continue;

			var whole = new Piece(pieces[first].Sign, new Typed(pieces[first].Primary.Node, Value | Truth | Bare | Subscript), pieces[last].Postfix);

			pieces.RemoveRange(first, last - first + 1);
			pieces.Insert(first, whole);
			operators.RemoveRange(first, last - first);

			last = first;
		}
	}

	/// <summary>The operators of one tower only: `||`, or `MULTISET`'s, or arithmetic.</summary>
	static int Join(List<Piece> pieces, List<int> operators)
	{
		if (operators.Count == 0)
			return Roles(pieces[0]);

		var kind = Kind(operators[0]);

		foreach (var op in operators)
			if (Kind(op) != kind)
				return 0;

		return kind switch
		{
			Concatenate      => Every(pieces, String | Array),
			MultisetOperator => Every(pieces, Multiset),
			_                => Arithmetic(pieces, operators),
		};
	}

	static int Kind(int op) => op is Concatenate or MultisetOperator ? op : Times;

	/// <summary>`||` joins character, binary and array operands, and a `MULTISET` operator multisets: all of one kind.</summary>
	static int Every(List<Piece> pieces, int kinds)
	{
		foreach (var piece in pieces)
			kinds &= Roles(piece);

		return kinds;
	}

	// ── Products and sums ──────────────────────────────────────────────────────

	/// <summary>
	/// A run of operands between `*` and `/`. A numeric term is numeric factors. An interval term has
	/// one interval factor among numeric ones, and a `*` before it where anything is before it:
	/// `2 * a DAY` and `a DAY / 2`, and not `2 / a DAY`.
	/// </summary>
	readonly record struct Product(bool AllNumeric, bool OneInterval, int Alone)
	{
		public static Product Of(int roles) => new((roles & Numeric) != 0, (roles & Interval) != 0, roles);

		public Product Then(bool multiplies, int roles) => new(
			AllNumeric && (roles & Numeric) != 0,
			OneInterval && (roles & Numeric) != 0 || AllNumeric && multiplies && (roles & Interval) != 0,
			0);

		public int Roles => (AllNumeric ? Numeric : 0) | (OneInterval ? Interval : 0) | Alone & ~(Numeric | Interval);
	}

	/// <summary>
	/// A run of products between `+` and `-`. Numeric terms make a numeric sum and interval terms an
	/// interval one. A datetime is one datetime term among interval terms, with a `+` before it where
	/// anything is: §6.35 adds an interval to a datetime on either side and subtracts one only after it.
	/// </summary>
	static int Arithmetic(List<Piece> pieces, List<int> operators)
	{
		var product = Product.Of(Roles(pieces[0]));
		var adds    = true;
		var terms   = 0;
		var roles   = 0;

		bool numeric = false, interval = false, datetime = false;

		for (var at = 0; at <= operators.Count; at++)
		{
			var op = at < operators.Count ? operators[at] : Plus;

			if (op is Times or Divided)
			{
				product = product.Then(op == Times, Roles(pieces[at + 1]));
				continue;
			}

			var term = product.Roles;

			if (terms++ == 0)
			{
				roles    = term;
				numeric  = (term & Numeric) != 0;
				interval = (term & Interval) != 0;
				datetime = (term & Datetime) != 0;
			}
			else
			{
				datetime  = datetime && (term & Interval) != 0 || interval && adds && (term & Datetime) != 0;
				numeric  &= (term & Numeric) != 0;
				interval &= (term & Interval) != 0;
			}

			if (at < operators.Count)
			{
				adds    = op == Plus;
				product = Product.Of(Roles(pieces[at + 1]));
			}
		}

		if (terms == 1)
			return roles;

		return (numeric ? Numeric : 0) | (interval ? Interval : 0) | (datetime ? Datetime : 0);
	}

	// ── Booleans ───────────────────────────────────────────────────────────────

	/// <summary>
	/// Operands joined by `AND` or `OR` are each a boolean; one alone is whatever it is. A boolean
	/// predicand is a value expression primary, so `a AND b` is one and `a + 1 AND b` is not.
	/// </summary>
	public static int Connect(Typed first, List<Typed>? rest)
	{
		var roles = first.Roles;

		if (rest is not { Count: > 0 })
			return roles;

		if ((roles & Truth) == 0)
			return 0;

		foreach (var one in rest)
			if ((one.Roles & Truth) == 0)
				return 0;

		return Logical | Truth;
	}

	/// <summary>Operands joined by one of `AND` and `OR`, from the left: the node, and what the whole can be.</summary>
	public static Typed Connected(Typed first, List<Typed>? rest, BinaryOperator op)
	{
		var roles = Connect(first, rest);

		if (roles == 0 || rest is not { Count: > 0 })
			return new Typed(first.Node, roles);

		var node = first.Node;

		foreach (var one in rest)
			node = new Expression.Binary(node, op, one.Node);

		return new Typed(node, roles);
	}

	// ── Arguments ──────────────────────────────────────────────────────────────

	/// <summary>
	/// A character operation, or a binary one where no length unit is named: `POSITION`, `SUBSTRING`
	/// and `OVERLAY` are each written twice by the BNF, and only the character one takes `USING`.
	/// </summary>
	public static bool Characters(int operands, bool units) =>
		(operands & Character) != 0 || !units && (operands & Binary) != 0;
}
