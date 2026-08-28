using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

using DotGram;

namespace DotGram.Parsers;

// A small language that compiles to a .NET expression tree — parameters, a block with
// local variables, and `return`:
//
//     (int x, int y) => { int sum = x + y; return sum * sum; }
//
// **Every `=>` below is a call into `System.Linq.Expressions` by name.** There is no
// model of this project's own between the grammar and the API, and no dispatch on an
// operator's text either: one alternative per operator, each naming the factory that
// builds it. That is not tidiness — it is where the seam is tested. A factory that does
// not exist, or one handed the wrong type, is a C# error reported on the line of the
// grammar that asked for it (§7.6); the same choice made by a `switch` over `op` would
// be a run-time exception in a library instead, which is the C# compiler kept out of
// work it can do.
//
// **What is left in this class is what the API has nowhere to keep**, and the list is
// worth reading as exactly that:
//
//   * `Declare`/`Named` — a `ParameterExpression` is an identity, and the one made for
//     `(int x)` has to be the very object each `x` reads. Nothing in the API holds a
//     mapping from a name to it.
//   * `Return`/`Block` — a `return` is a jump to a label, and the label belongs to the
//     block rather than to any statement in it.
//
// Everything else the grammar says itself.
//
// **Two facts about the notation decide the shape**, and the first was measured rather
// than assumed:
//
//   * `=>` runs after the whole match, children before parents and, among siblings, from
//     the end of the text backwards — so a use of `x` is built *before* the parameter
//     that declares it, and no `=>` can resolve a name.
//   * `when` runs *during* the match (§8.1), in reading order.
//
// So declarations are made by guards while reading, and uses are built afterwards
// against a table that is by then complete. A guard **answers** rather than throws,
// because it also runs on readings the parse abandons.
//
// **And the grammar is shaped to the API in three places**, deliberately:
//
//   * a local says its type — `int sum = …`, not `var sum = …` — because
//     `Expression.Variable` wants one where the declaration is read, and the initializer
//     is not built until long after;
//   * a name means one thing for the whole lambda: a block does not shadow, because
//     shadowing needs a scope entered and left around a *construction*, and construction
//     is not where the reading is;
//   * nothing widens on its own — `x + 1.5` over an `int` and a `double` is refused by
//     `Expression.Add` itself, in its own words, because a language that speaks only this
//     API has no place to put a conversion the API did not ask for.
//
// All three are the API's requirements showing through, which is what wiring one up
// actually looks like.

[Gram("""
	@using System;
	@using System.Globalization;
	@using System.Linq.Expressions;

	using Lexical;

	namespace Lexical
	{
		trivia = none

		Word   = [\p{L} | '_'] & [\p{L} | \p{Nd} | '_']*
		Digits = ['0'..'9']+
		Real   = Digits & '.' & Digits

		// A suffix says which type the constant is, as it does in C#. Lexical, because
		// nothing may come between the digits and the letter — and because §4.6 weaves a
		// boundary round a word literal, and a digit is a word character, so `"L"` after
		// `1` would be refused by the very guard that keeps `int` out of `internal`. A
		// set is not a literal and carries no boundary, which is the other half of why
		// these are written as sets.
		//
		// Each hands back the digits alone: `decimal.Parse` reads a number, not a number
		// and a letter.
		Long    : @string = t: Digits          & ['L' | 'l'] => @(t)
		Decimal : @string = t: (Real | Digits) & ['m' | 'M'] => @(t)
		Double  : @string = t: (Real | Digits) & ['d' | 'D'] => @(t)
	}

	// §4.6: a keyword is a whole word, so `returned` is a name and not a jump, and
	// `internal` is a name and not the type `int`.
	wordboundary = [\p{L} | \p{Nd} | '_']

	trivia = { (' ' | '\t' | '\r' | '\n')* }

	// ── A lambda: what it takes, and what it does ───────────────────────────────

	Lambda : @LambdaExpression
		= '(' & (first: Parameter & (',' & rest: Parameter)*)? & ')' & "=>" & body: Body
		=> @(Expression.Lambda(body, ExpressionLanguage.Taking(first, rest)))

	// Each type names itself in C#, so `typeof(int)` is checked where it is written and
	// a word that is no type is not a declaration — the grammar refusing that reading
	// rather than a switch over strings refusing it at run time.
	Type : @Type = "int"     => @(typeof(int))
	             | "long"    => @(typeof(long))
	             | "double"  => @(typeof(double))
	             | "decimal" => @(typeof(decimal))
	             | "bool"    => @(typeof(bool))
	             | "string"  => @(typeof(string))

	// The guard is the declaration: it runs while the text is read, which is the only
	// moment this grammar has in the order it is written.
	Parameter : @ParameterExpression
		= type: Type & name: Word & when @(ExpressionLanguage.Declare(type, name))
		=> @(ExpressionLanguage.Named(name))

	Body : @Expression = b: Block => @(b) | e: Expression => @(e)

	// ── A block, and the statements it is made of ───────────────────────────────

	Block : @Expression = '{' & statements: Statement* & '}'
	                    => @(ExpressionLanguage.Block(statements))

	Statement : @Expression = s: Local => @(s) | s: Return => @(s)

	Local : @Expression
		= type: Type & name: Word & when @(ExpressionLanguage.Declare(type, name))
		& '=' & value: Expression & ';'
		=> @(Expression.Assign(ExpressionLanguage.Named(name), value))

	Return : @Expression = "return" & value: Expression & ';'
	                     => @(ExpressionLanguage.Return(value))

	// ── The operators, one rule per level of precedence (§4.3) ──────────────────

	Expression : @Expression = e: Or => @(e)

	Or : @Expression = left: Or & "||" & right: And => @(Expression.OrElse(left, right))
	                 | a: And                       => @(a)

	And : @Expression = left: And & "&&" & right: Equality => @(Expression.AndAlso(left, right))
	                  | e: Equality                        => @(e)

	Equality : @Expression
		= left: Equality & "==" & right: Relational => @(Expression.Equal(left, right))
		| left: Equality & "!=" & right: Relational => @(Expression.NotEqual(left, right))
		| r: Relational                             => @(r)

	Relational : @Expression
		= left: Relational & "<=" & right: Additive => @(Expression.LessThanOrEqual(left, right))
		| left: Relational & ">=" & right: Additive => @(Expression.GreaterThanOrEqual(left, right))
		| left: Relational & '<'  & right: Additive => @(Expression.LessThan(left, right))
		| left: Relational & '>'  & right: Additive => @(Expression.GreaterThan(left, right))
		| a: Additive                               => @(a)

	Additive : @Expression
		= left: Additive & '+' & right: Multiplicative => @(Expression.Add(left, right))
		| left: Additive & '-' & right: Multiplicative => @(Expression.Subtract(left, right))
		| m: Multiplicative                            => @(m)

	Multiplicative : @Expression
		= left: Multiplicative & '*' & right: Unary => @(Expression.Multiply(left, right))
		| left: Multiplicative & '/' & right: Unary => @(Expression.Divide(left, right))
		| left: Multiplicative & '%' & right: Unary => @(Expression.Modulo(left, right))
		| u: Unary                                  => @(u)

	Unary : @Expression = '-' & operand: Unary => @(Expression.Negate(operand))
	                    | '!' & operand: Unary => @(Expression.Not(operand))
	                    | p: Primary           => @(p)

	// A literal says which it is by how it is written, so the two are two alternatives
	// and each hands `Expression.Constant` a value of the type it already has.
	Primary : @Expression
		= '(' & inner: Expression & ')' => @(inner)

		// The suffixed forms first: ordered choice would otherwise read `1L` as the `1`
		// of an `int` and leave the letter to whatever comes next, and only backtracking
		// would find its way here (§11).
		// Two names, not one: a capture of a rule that builds a value and a capture of
		// plain text are two kinds of member, and §7.3 gives a rule one member per name.
		| number: Long    => @(Expression.Constant(long.Parse(number, CultureInfo.InvariantCulture)))
		| number: Decimal => @(Expression.Constant(decimal.Parse(number, CultureInfo.InvariantCulture)))
		| number: Double  => @(Expression.Constant(double.Parse(number, CultureInfo.InvariantCulture)))
		| text: Real      => @(Expression.Constant(double.Parse(text, CultureInfo.InvariantCulture)))
		| text: Digits    => @(Expression.Constant(int.Parse(text, CultureInfo.InvariantCulture)))

		| "true"        => @(Expression.Constant(true))
		| "false"       => @(Expression.Constant(false))
		| name: Word    => @(ExpressionLanguage.Named(name))

	parse Lambda as ParseLambda
	""")]
public static partial class ExpressionLanguage
{
	// ParseLambda and TryParseLambda are generated here.

	/// <summary>Reads the text as a lambda over an expression tree.</summary>
	/// <exception cref="FormatException">The text is not this language.</exception>
	/// <exception cref="ArgumentException">
	/// It is this language and means nothing in it — an operator its operands do not
	/// support, a lambda over a variable nothing declares. Thrown by
	/// <c>System.Linq.Expressions</c> itself, in its own words: this language holds no
	/// opinion the API does not already hold.
	/// </exception>
	public static LambdaExpression Parse(string text)
	{
		_declared?.Clear();
		_returns = null;

		return ParseLambda(text);
	}

	/// <summary>The same, compiled to a delegate of the caller's own type.</summary>
	/// <remarks>
	/// Where the two halves meet a caller: what the text declares has to match what the
	/// delegate takes, and <c>Expression.Lambda</c> is what says so — in a message naming
	/// both, which is better than anything this could invent.
	/// </remarks>
	public static TDelegate Compile<TDelegate>(string text)
		where TDelegate : Delegate
	{
		var lambda = Parse(text);

		return (TDelegate)Expression.Lambda(typeof(TDelegate), lambda.Body, lambda.Parameters).Compile();
	}

	// ── What System.Linq.Expressions has nowhere to keep ────────────────────────

	/// <summary>The variables of the lambda being read, by the name the text calls them.</summary>
	/// <remarks>
	/// A <c>ParameterExpression</c> is an identity: the one made for <c>(int x)</c> has to
	/// be the very object every <c>x</c> in the body reads, or the compiled lambda closes
	/// over a variable nothing assigns. Thread-static and cleared where a parse begins,
	/// so one lambda's <c>x</c> cannot answer another's.
	/// </remarks>
	[ThreadStatic]
	static Dictionary<string, ParameterExpression>? _declared;

	/// <summary>Where a <c>return</c> goes, made once the first one says what it yields.</summary>
	[ThreadStatic]
	static LabelTarget? _returns;

	/// <summary>
	/// A declaration, made while the text is read (§8.1) — the only moment this grammar
	/// has in the order it is written.
	/// </summary>
	/// <returns>
	/// Whether this reads as a declaration at all. A guard answers rather than throws,
	/// and it has to: <c>when</c> runs during the match, so it also runs on readings the
	/// parse goes on to abandon.
	/// </returns>
	/// <remarks>
	/// What that costs is that an abandoned reading leaves its name behind, so a use of a
	/// name only a failed path declared resolves to a variable no lambda holds — which
	/// <c>Expression.Lambda</c> refuses, in its own words.
	/// </remarks>
	public static bool Declare(Type type, string name)
	{
		if (type is null)
			throw new ArgumentNullException(nameof(type));

		(_declared ??= new Dictionary<string, ParameterExpression>(StringComparer.Ordinal))[name] =
			Expression.Variable(type, name);

		return true;
	}

	/// <summary>The variable that name was declared as.</summary>
	public static ParameterExpression Named(string name) =>
		_declared is not null && _declared.TryGetValue(name, out var found)
			? found
			: throw new FormatException($"nothing named '{name}' is declared here.");

	/// <summary>The parameters a lambda takes, in the order it wrote them.</summary>
	public static ParameterExpression[] Taking(ParameterExpression? first, ParameterExpression[] rest)
	{
		if (first is null)
			return [];

		var parameters = new ParameterExpression[rest.Length + 1];

		parameters[0] = first;
		rest.CopyTo(parameters, 1);

		return parameters;
	}

	/// <summary>A jump to the block's label, which the first <c>return</c> is what makes.</summary>
	public static Expression Return(Expression value)
	{
		if (value is null)
			throw new ArgumentNullException(nameof(value));

		_returns ??= Expression.Label(value.Type, "return");

		return Expression.Return(_returns, value);
	}

	/// <summary>
	/// A block: the variables it declared, its statements, and the label its returns go to.
	/// </summary>
	/// <remarks>
	/// The variables are read back out of the assignments rather than collected
	/// separately — an assignment to a variable is what a declaration became, so the
	/// statements already say which they are.
	/// </remarks>
	public static Expression Block(Expression[] statements)
	{
		if (statements is null)
			throw new ArgumentNullException(nameof(statements));

		if (_returns is null)
			throw new FormatException("a block has to end in a return.");

		var variables = new List<ParameterExpression>();

		foreach (var statement in statements)
			if (statement is BinaryExpression { NodeType: ExpressionType.Assign, Left: ParameterExpression variable })
				variables.Add(variable);

		var body = new List<Expression>(statements)
		{
			// The label is the block's value, and the tree asks for a default for the
			// path that falls off the end — which this language does not allow and the
			// tree has no way to know.
			Expression.Label(_returns, Expression.Default(_returns.Type)),
		};

		return Expression.Block(variables, body);
	}
}
