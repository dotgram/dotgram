using System;
using System.Collections.Generic;

namespace DotGram.Sql.Standard;

/// <summary>
/// What a value expression can still be, as the BNF of SQL:2023 types it: a set of its towers,
/// carried beside the reading by <c>SqlStandard.gram</c>.
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

	public const int String = Character | Binary;

	/// <summary>A <c>&lt;value expression primary&gt;</c>, which has every type.</summary>
	public const int Value = Numeric | Interval | Datetime | Character | Binary | Array | Multiset | Json | UserDefined;

	/// <summary>Whatever a row value predicand is.</summary>
	public const int Any = Value | Row;

	// ── A primary ──────────────────────────────────────────────────────────────

	/// <summary>
	/// What steps after a primary make of it — `.name`, `->name`, `[i]` — which is a value expression
	/// primary like any, and not a parenthesized one; nothing, where there are none.
	/// </summary>
	public static int Steps(int[]? steps)
	{
		var made = 0;

		foreach (var step in steps ?? [])
			made = made & ~Starred | step;

		return made;
	}

	public static int Stepped(int primary, int steps) =>
		steps == 0 ? primary : Value | Truth | Bare | steps;

	// ── An operand ─────────────────────────────────────────────────────────────

	/// <summary>What follows a primary: an interval qualifier, a time zone, a collate clause, a type's name, and a collate clause after that.</summary>
	public const int Qualified = 1, Zoned = 2, Collated = 3, Typed = 4, TypedCollated = 5;

	/// <summary>A primary, with its sign and what follows it.</summary>
	public readonly record struct Piece(bool Signed, int Primary, int Postfix);

	/// <summary>
	/// What an operand can be. A sign leaves a numeric and an interval factor; an interval qualifier
	/// makes an interval primary of a value expression primary, and `.SPECIFICTYPE` a character value
	/// function of one; `AT` keeps a datetime factor and `COLLATE` a character one. Only a primary left
	/// alone keeps being a predicand, a chain, a primary of its kind, or an explicit row.
	/// </summary>
	public static int Roles(Piece piece)
	{
		var primary = piece.Primary;

		var roles = piece.Postfix switch
		{
			Qualified     => (primary & Value) == Value ? Interval : 0,
			Zoned         => primary & Datetime,
			Collated      => primary & Character,
			Typed         => (primary & Value) == Value ? String : 0,
			TypedCollated => (primary & Value) == Value ? Character : 0,
			_             => primary & Value,
		};

		if (piece.Signed)
			return roles & (Numeric | Interval);

		return piece.Postfix == 0 ? roles | primary & (Truth | Chain | Bare | Parenthesized | Row | Invoked | Starred) : roles;
	}

	// ── The operators ──────────────────────────────────────────────────────────

	public const int Concatenate = 1, MultisetOperator = 2, Times = 3, Divided = 4, Plus = 5, Minus = 6;

	public readonly record struct Operated(int Operator, Piece Operand);

	/// <summary>
	/// What the expression can be, or nothing where it can be nothing. An explicit row is read where a
	/// bracket is, and is something only alone: no operator joins one.
	/// </summary>
	public static int Common(Piece first, Operated[]? rest)
	{
		var pieces    = new List<Piece> { first };
		var operators = new List<int>();

		foreach (var (op, operand) in rest ?? [])
		{
			operators.Add(op);
			pieces.Add(operand);
		}

		Fold(pieces, operators);

		var roles = Join(pieces, operators);

		return (roles & (Value | Row)) != 0 ? roles : 0;
	}

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
			if ((pieces[last].Primary & Subscript) == 0 || pieces[last].Signed || operators[last - 1] != Concatenate)
				continue;

			var first = last;

			while (first > 0 && operators[first - 1] == Concatenate && pieces[first - 1] is { Signed: false, Postfix: 0 } && (pieces[first - 1].Primary & Array) != 0)
				first--;

			if (first > 0 && operators[first - 1] == Concatenate && pieces[first - 1] is { Signed: true, Postfix: 0 } && (pieces[first - 1].Primary & Array) != 0)
				first--;

			if (first == last)
				continue;

			var whole = new Piece(pieces[first].Signed, Value | Truth | Bare | Subscript, pieces[last].Postfix);

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
	public static int Connect(int first, int[]? rest)
	{
		if (rest is not { Length: > 0 })
			return first;

		if ((first & Truth) == 0)
			return 0;

		foreach (var one in rest)
			if ((one & Truth) == 0)
				return 0;

		return Logical | Truth;
	}

	// ── Arguments ──────────────────────────────────────────────────────────────

	/// <summary>
	/// A character operation, or a binary one where no length unit is named: `POSITION`, `SUBSTRING`
	/// and `OVERLAY` are each written twice by the BNF, and only the character one takes `USING`.
	/// </summary>
	public static bool Characters(int operands, bool units) =>
		(operands & Character) != 0 || !units && (operands & Binary) != 0;
}
