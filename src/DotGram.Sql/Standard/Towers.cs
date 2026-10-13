using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

namespace DotGram.Sql.Standard;

// Inside the namespace, where they are asked before DotGram.Sql's own Expression and Query, the tree T-SQL builds.
using Expression = DotGram.Sql.Ast.Expression;
using Query = DotGram.Sql.Ast.Query;

/// <summary>
/// What a value expression can still be, as the BNF of SQL:2023 types it: a set of its towers,
/// carried beside the node by <c>SqlStandard.gram</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>The BNF types its expressions, and a parser has no types.</b> A numeric value expression, a
/// character one, a datetime one and an interval one are towers of their own that meet only in a
/// <c>&lt;value expression primary&gt;</c>, so `a || b + c` is none of them and the BNF refuses it;
/// `x + y AT LOCAL` is a datetime — an interval, a plus and a datetime term — and `x - y AT LOCAL` is
/// nothing. An ordered choice cannot try the towers one after another, since each would take the
/// first operand and stop; so the grammar reads the shape they share, operands and the operators
/// between them, and this says which towers the whole still belongs to. Where none is left the
/// reading is refused.
/// </para>
/// <para>
/// <b>One production cuts across the shape.</b> An <c>&lt;array element reference&gt;</c> subscripts
/// an array value expression, and a concatenation is one: `x || y[1]` is `x` joined to `y[1]`, and
/// it is `(x || y)[1]` too, a primary that a sign may stand before and an interval qualifier after.
/// So a run of `||` that ends in a subscript folds into one operand before the operators are asked.
/// The tree says the first: a subscript binds to its primary.
/// </para>
/// <para>
/// <b>The node is built from the same pieces.</b> What the towers are asked of — a primary, its sign,
/// what follows it, the operators between operands — is what the tree is made of, so each piece
/// carries its node and <see cref="Expression(Piece, Operated[])"/> answers both questions at once.
/// </para>
/// </remarks>
static class Towers
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

	/// <summary>
	/// A datetime less a datetime term, which is no value: §6.33 reads it only in brackets with an
	/// interval qualifier after them, `(a - CURRENT_DATE) DAY`. A bracket keeps it, a qualifier makes
	/// an interval of it, and nothing else takes it.
	/// </summary>
	public const int Difference = 1 << 18;

	// A ROLE ADDED HERE IS A VALUE EXPRESSION UNLESS IT IS ADDED TO THAT MASK AS WELL. `ValueExpression`
	// and `RowValuePredicand` in the grammar both guard with `(RolesOf(e) & ~Difference) != 0` -- any role
	// but this one -- so they accept by exclusion, and `Difference` is the only thing excluded. A new bit
	// therefore says "this may stand where a value expression is wanted" by saying nothing, and it widens
	// rather than narrows, so no test written for something else would fail on it.
	//
	// The rows that would are `A_bare_query_expression_is_not_a_value_expression` and
	// `..._is_not_a_row_value_predicand` in SqlStandardParserTests: they try the illegal input, which is
	// the only way a widening is caught. Add to them when a role is added.

	public const int String = Character | Binary;

	/// <summary>A <c>&lt;value expression primary&gt;</c>, which has every type.</summary>
	public const int Value = Numeric | Interval | Datetime | Character | Binary | Array | Multiset | Json | UserDefined;

	/// <summary>Whatever a row value predicand is.</summary>
	public const int Any = Value | Row;

	/// <summary>A node, and what the towers say it can still be.</summary>
	public sealed record Typed(Expression Node, int Roles)
	{
		/// <summary>What <c>default(Typed)</c> was while this was a struct: no node, no roles.</summary>
		public static readonly Typed None = new(null!, 0);
	}

	// ── What a guard asks ──────────────────────────────────────────────────────
	//
	// The generator writes a guard into more than one reader, and a captured struct is the struct in
	// one of them and a nullable struct in another. A parameter of the nullable type takes both.

	public static int RolesOf(Typed? value)
	{
		return value?.Roles ?? 0;
	}

	public static int RolesOf(Tail? value)
	{
		return value?.Roles ?? 0;
	}

	public static int RolesOf(Stepping? value)
	{
		return value?.Roles ?? 0;
	}

	public static int KindOf(Bracket? value)
	{
		return value?.Kind ?? 0;
	}

	/// <summary>What an operand can be, asked of its parts before they are made one.</summary>
	public static int OperandRoles(UnaryOperator? sign, Typed? primary, After? postfix)
	{
		return Roles(sign is not null, primary?.Roles ?? 0, postfix?.Kind ?? 0);
	}

	// ── A primary ──────────────────────────────────────────────────────────────

	/// <summary>
	/// A step after a primary — `.name`, `->name`, `[i]`, `.*` — and what it makes of the primary. The
	/// node is the step with its target left empty, filled in by <see cref="Apply(Expression, Step[])"/>.
	/// </summary>
	public readonly record struct Step(int Roles, Expression Node);

	/// <summary>
	/// What steps after a primary make of it, which is a value expression primary like any, and not a
	/// parenthesized one; nothing, where there are none.
	/// </summary>
	public static int Steps(Step[]? steps)
	{
		var made = 0;

		foreach (var step in steps ?? [])
			made = made & ~Starred | step.Roles;

		return made;
	}

	public static int Stepped(int primary, int steps)
	{
		return steps == 0 ? primary : Value | Truth | Bare | steps;
	}

	/// <summary>Steps read, and what they make of a primary.</summary>
	public sealed record Stepping(int Roles, Step[]? Steps)
	{
		/// <summary>What <c>default(Stepping)</c> was while this was a struct: no roles, no steps.</summary>
		public static readonly Stepping None = new(0, null);
	}

	/// <summary>
	/// The steps read after a primary, or <see cref="Stepping.None"/> where none were: most primaries have
	/// none, and share it.
	/// </summary>
	public static Stepping Following(Step[]? steps)
	{
		return steps is { Length: > 0 } ? new Stepping(Steps(steps), steps) : Stepping.None;
	}

	/// <summary>
	/// A primary and the steps after it, each taking what stands before it as its target; a primary with
	/// none is itself.
	/// </summary>
	public static Typed Stepped(Typed primary, Stepping steps)
	{
		if (steps.Steps is not { Length: > 0 })
			return primary;

		return new(Apply(primary.Node, steps.Steps), Stepped(primary.Roles, steps.Roles));
	}

	/// <summary>A value function's subscript and the steps after it.</summary>
	public static Stepping Subscripted(Step first, Step[]? rest)
	{
		var all = new Step[(rest?.Length ?? 0) + 1];

		all[0] = first;
		rest?.CopyTo(all, 1);

		return new Stepping(Subscript | Steps(rest), all);
	}

	/// <summary>
	/// What follows a bracket's value expression: the closing bracket, the rest of a row, or `AS` and a
	/// type, a method and its arguments.
	/// </summary>
	public readonly record struct Bracket(int Kind, Expression[]? Rest = null, DataType? Type = null, Identifier? Method = null, SqlList<Argument>? Arguments = null);

	/// <summary>What a name turned out to begin: an identifier chain, a window row pattern measure, a routine invocation, or a static method invocation.</summary>
	public const int AsChain = 0, AsMeasure = 1, AsRoutine = 2, AsStaticMethod = 3;

	/// <summary>
	/// What follows a name, read once for the four primaries that begin with one: the rest of an
	/// identifier chain; `OVER` and a window; the rest of a routine's name and its arguments; the rest of
	/// a type's name, `::`, a method and its arguments.
	/// </summary>
	public readonly record struct Chained(int Kind, Identifier[]? Rest, WindowReference? Over = null, Identifier? Method = null, SqlList<Argument>? Arguments = null);

	/// <summary>What a tail holds: a predicate built with its left side empty, a comparison's right side, or a quantified comparison's quantifier and subquery.</summary>
	public const int Built = 0, Comparing = 1, Quantifying = 2;

	/// <summary>
	/// A predicate's second part, and what it takes before it. A comparison is not built until its left
	/// side is known: the tail keeps its operator and its right side, or its quantifier and subquery, and
	/// <c>Nodes.Predicated</c> builds the node once. Every other predicate is built with its left side
	/// empty, and `NOT`, where it was written, goes in with the left side.
	/// </summary>
	public readonly record struct Tail(int Roles, Expression? Node, int Kind = Built, ComparisonOperator Operator = default, Ast.Quantifier Quantifier = default, Query? Subquery = null, bool Not = false);

	public static Expression Apply(Expression primary, Step[]? steps)
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
	public sealed record After(int Kind, Expression? Node)
	{
		/// <summary>What <c>default(After)</c> was while this was a struct: nothing follows.</summary>
		public static readonly After None = new(0, null);
	}

	/// <summary>A primary, with its sign and what follows it.</summary>
	public sealed record Piece(UnaryOperator? Sign, Typed Primary, After Postfix)
	{
		/// <summary>What <c>default(Piece)</c> was while this was a struct: an unsigned nothing.</summary>
		/// <remarks>
		/// Needed by <see cref="Common"/>, whose first operand is nullable because a guard may ask
		/// before the capture is bound. As a struct that call read `GetValueOrDefault()` and got a
		/// piece with no node and no roles; this is the same piece under a name. The alternative was
		/// `first!`, which is not the same thing -- it asserts rather than answers, and a null would
		/// reach `Roles` and throw where the struct returned no roles at all.
		/// </remarks>
		public static readonly Piece None = new(null, Typed.None, After.None);

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
		return Roles(piece.Signed, piece.Primary.Roles, piece.Postfix.Kind);
	}

	static int Roles(bool signed, int primary, int postfix)
	{
		var roles = postfix switch
		{
			Qualified     => (primary & Value) == Value || (primary & Difference) != 0 ? Interval : 0,
			Zoned         => primary & Datetime,
			Collated      => primary & Character,
			Typed_        => (primary & Value) == Value ? String : 0,
			TypedCollated => (primary & Value) == Value ? Character : 0,
			_             => primary & Value,
		};

		if (signed)
			return roles & (Numeric | Interval);

		return postfix == 0 ? roles | primary & (Truth | Chain | Bare | Parenthesized | Row | Invoked | Starred) : roles;
	}

	/// <summary>An operand's node: its primary, what follows the primary, and its sign around both.</summary>
	public static Expression Node(Piece piece)
	{
		var value = Follow(piece.Primary.Node, piece.Postfix.Node);

		return piece.Sign is { } sign ? new Expression.Unary(sign, value) : value;
	}

	static Expression Follow(Expression value, Expression? after)
	{
		return after switch
		{
			null => value,
			Expression.IntervalQualified q => q with { Value = value },
			Expression.AtTimeZone z => z with { Value = value },
			Expression.Collate { Value: null } c => c with { Value = value },
			Expression.Collate c => c with { Value = Follow(value, c.Value) },
			Expression.Member m => m with { Target = value },
			_ => throw new ArgumentOutOfRangeException(nameof(after), after, "What follows a primary this method cannot complete."),
		};
	}

	// ── The operators ──────────────────────────────────────────────────────────

	public const int Concatenate = 1, MultisetOperator = 2, Times = 3, Divided = 4, Plus = 5, Minus = 6;

	/// <summary>An operator: which, and for a multiset's, which of the three and its quantifier.</summary>
	public readonly record struct Op(int Kind, Ast.MultisetOperator Multiset = default, SetQuantifier? Quantifier = null);

	public readonly record struct Operated(Op Operator, Piece Operand);

	/// <summary>
	/// Operands and the operators between them: the node, and what the towers say it can be — nothing,
	/// where it can be nothing. A lone primary that no sign, postfix or operator touched is its own
	/// answer, and is returned as it came.
	/// </summary>
	public static Typed Expression(Piece first, Operated[]? rest)
	{
		var roles = Common(first, rest);

		if (rest is not { Length: > 0 } && !first.Signed && first.Postfix.Kind == 0 && roles == first.Primary.Roles)
			return first.Primary;

		return new Typed(roles == 0 ? first.Primary.Node : Build(first, rest), roles);
	}

	/// <summary>
	/// What the expression can be, or nothing where it can be nothing. An explicit row is read where a
	/// bracket is, and is something only alone: no operator joins one.
	/// </summary>
	/// <remarks>
	/// Read over the operands where they stand, in the operand and the array of operated ones the rule
	/// captured: nothing is copied for the common shapes — one operand, or a run of one operator kind.
	/// Only a run of `||` that ends in a subscript folds, and only then are the operands copied into
	/// lists to be folded.
	/// </remarks>
	public static int Common(Piece? first, Operated[]? rest)
	{
		var run   = new Run(first ?? Piece.None, rest);
		var roles = Join(Folds(run) ? Fold(run) : run);

		return (roles & (Value | Row | Difference)) != 0 ? roles : 0;
	}

	/// <summary>
	/// The node of operands and operators: `*`, `/` and `MULTISET INTERSECT` bind tighter than `+`, `-`,
	/// `||`, `MULTISET UNION` and `MULTISET EXCEPT`, and each strength is read from the left. A tight
	/// operator joins its operand to the term on its left as it is read; a loose one closes that term
	/// into the sum before it.
	/// </summary>
	static Expression Build(Piece first, Operated[]? rest)
	{
		if (rest is not { Length: > 0 })
			return Node(first);

		Expression? sum   = null;
		Op          loose = default;
		var         term  = Node(first);

		foreach (var (op, operand) in rest)
		{
			if (op.Kind is Times or Divided || op.Kind == MultisetOperator && op.Multiset == Ast.MultisetOperator.Intersect)
			{
				term = Combine(term, op, Node(operand));

				continue;
			}

			sum   = sum is null ? term : Combine(sum, loose, term);
			loose = op;
			term  = Node(operand);
		}

		return sum is null ? term : Combine(sum, loose, term);
	}

	static Expression Combine(Expression left, Op op, Expression right)
	{
		return op.Kind switch
		{
			Concatenate => new Expression.Binary(left, BinaryOperator.Concatenate, right),
			MultisetOperator => new Expression.MultisetOperation(left, op.Multiset, op.Quantifier, right),
			Times => new Expression.Binary(left, BinaryOperator.Multiply, right),
			Divided => new Expression.Binary(left, BinaryOperator.Divide, right),
			Plus => new Expression.Binary(left, BinaryOperator.Add, right),
			_ => new Expression.Binary(left, BinaryOperator.Subtract, right),
		};
	}

	/// <summary>
	/// The operands and operators the towers are asked about, where they stand: the first operand and the
	/// operated ones as the rule captured them, or the lists a fold made of them. Operand <c>i</c> has
	/// operator <c>i - 1</c> before it.
	/// </summary>
	readonly struct Run
	{
		readonly Piece        _first;
		readonly Operated[]?  _rest;
		readonly List<Piece>? _pieces;
		readonly List<int>?   _operators;

		public Run(Piece first, Operated[]? rest)
		{
			_first = first;
			_rest  = rest;
		}

		public Run(List<Piece> pieces, List<int> operators)
		{
			_first     = pieces[0];
			_pieces    = pieces;
			_operators = operators;
		}

		public int Count => _pieces?.Count ?? (_rest?.Length ?? 0) + 1;

		public Piece this[int at] => _pieces is not null ? _pieces[at] : at == 0 ? _first : _rest![at - 1].Operand;

		public int Operator(int at)
		{
			return _operators is not null ? _operators[at] : _rest![at].Operator.Kind;
		}
	}

	/// <summary>
	/// Whether a run holds what a fold begins at — a subscripted, unsigned operand after a `||` — so that
	/// only a run that may fold is copied to be folded.
	/// </summary>
	static bool Folds(in Run run)
	{
		for (var at = 1; at < run.Count; at++)
			if ((run[at].Primary.Roles & Subscript) != 0 && !run[at].Signed && run.Operator(at - 1) == Concatenate)
				return true;

		return false;
	}

	/// <summary>
	/// A run of `||` that ends in a subscripted operand is also one array element reference: the run
	/// is the array, the sign of its first operand stands before the whole and what follows its last
	/// after it. Folding takes operators away and constrains nothing, so a run is taken as far as it
	/// goes: over array primaries, unsigned and with nothing after them, and over one signed one
	/// where the run begins. The operands are copied into lists for this, which only a run that may
	/// fold pays: one with a subscripted operand after a `||`, whether or not the run before it is arrays.
	/// </summary>
	static Run Fold(in Run run)
	{
		var pieces    = new List<Piece>(run.Count);
		var operators = new List<int>(run.Count - 1);

		for (var at = 0; at < run.Count; at++)
		{
			if (at > 0)
				operators.Add(run.Operator(at - 1));

			pieces.Add(run[at]);
		}

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

		return new Run(pieces, operators);
	}

	/// <summary>The operators of one tower only: `||`, or `MULTISET`'s, or arithmetic.</summary>
	static int Join(in Run run)
	{
		if (run.Count == 1)
			return Roles(run[0]);

		var kind = Kind(run.Operator(0));

		for (var at = 1; at < run.Count; at++)
			if (Kind(run.Operator(at - 1)) != kind)
				return 0;

		return kind switch
		{
			Concatenate      => Every(run, String | Array),
			MultisetOperator => Every(run, Multiset),
			_                => Arithmetic(run),
		};
	}

	static int Kind(int op)
	{
		return op is Concatenate or MultisetOperator ? op : Times;
	}

	/// <summary>`||` joins character, binary and array operands, and a `MULTISET` operator multisets: all of one kind.</summary>
	static int Every(in Run run, int kinds)
	{
		for (var at = 0; at < run.Count; at++)
			kinds &= Roles(run[at]);

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
		public static Product Of(int roles)
		{
			return new((roles & Numeric) != 0, (roles & Interval) != 0, roles);
		}

		public Product Then(bool multiplies, int roles)
		{
			return new(
			AllNumeric && (roles & Numeric) != 0,
			OneInterval && (roles & Numeric) != 0 || AllNumeric && multiplies && (roles & Interval) != 0,
			0);
		}

		public int Roles => (AllNumeric ? Numeric : 0) | (OneInterval ? Interval : 0) | Alone & ~(Numeric | Interval);
	}

	/// <summary>
	/// A run of products between `+` and `-`. Numeric terms make a numeric sum and interval terms an
	/// interval one. A datetime is one datetime term among interval terms, with a `+` before it where
	/// anything is: §6.35 adds an interval to a datetime on either side and subtracts one only after it.
	/// </summary>
	static int Arithmetic(in Run run)
	{
		var operators = run.Count - 1;
		var product   = Product.Of(Roles(run[0]));
		var adds      = true;
		var terms     = 0;
		var roles     = 0;

		bool numeric = false, interval = false, datetime = false, difference = false;

		for (var at = 0; at <= operators; at++)
		{
			var op = at < operators ? run.Operator(at) : Plus;

			if (op is Times or Divided)
			{
				product = product.Then(op == Times, Roles(run[at + 1]));
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
				difference = datetime && !adds && (term & Datetime) != 0;
				datetime   = datetime && (term & Interval) != 0 || interval && adds && (term & Datetime) != 0;
				numeric  &= (term & Numeric) != 0;
				interval &= (term & Interval) != 0;
			}

			if (at < operators)
			{
				adds    = op == Plus;
				product = Product.Of(Roles(run[at + 1]));
			}
		}

		if (terms == 1)
			return roles;

		return (numeric ? Numeric : 0) | (interval ? Interval : 0) | (datetime ? Datetime : 0) | (difference ? Difference : 0);
	}

	// ── Booleans ───────────────────────────────────────────────────────────────

	/// <summary>
	/// Operands joined by `AND` or `OR` are each a boolean; one alone is whatever it is. A boolean
	/// predicand is a value expression primary, so `a AND b` is one and `a + 1 AND b` is not.
	/// </summary>
	public static int Connect(Typed? first, Typed[]? rest)
	{
		var roles = RolesOf(first);

		if (rest is not { Length: > 0 })
			return roles;

		if ((roles & Truth) == 0)
			return 0;

		foreach (var one in rest)
			if ((one.Roles & Truth) == 0)
				return 0;

		return Logical | Truth;
	}

	/// <summary>
	/// Operands joined by one of `AND` and `OR`, from the left: the node, and what the whole can be; one
	/// operand alone is itself.
	/// </summary>
	public static Typed Connected(Typed first, Typed[]? rest, BinaryOperator op)
	{
		if (rest is not { Length: > 0 })
			return first;

		var roles = Connect(first, rest);

		if (roles == 0)
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
	public static bool Characters(int operands, bool units)
	{
		return (operands & Character) != 0 || !units && (operands & Binary) != 0;
	}
}
