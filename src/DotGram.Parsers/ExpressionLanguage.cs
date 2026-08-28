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
// **Every `=>` in the grammar below builds `System.Linq.Expressions` and nothing else.**
// There is no syntax tree of this project's own between the two, and that is the point:
// a third-party API is what the grammar constructs directly, so what is proved here is
// that one can be wired to a parser as it stands rather than through a model written to
// suit the parser.
//
// **What that costs is a shape, and the shape is the interesting part.** Two facts about
// the language decide it:
//
//   * `=>` runs after the whole match, children before parents (docs/syntax.md §7.2) —
//     so a use of `x` is built *before* the parameter that declares it, and a name
//     cannot resolve to a `ParameterExpression` at the moment it is constructed.
//   * `when` runs *during* the match (§8.1), in the order the text is read.
//
// So the declarations are made by guards while reading, and the uses are built
// afterwards against a table that is by then complete. `Declare` and `Named` below are
// the whole of it — twenty lines that hold `ParameterExpression`s by name, which is the
// one thing `System.Linq.Expressions` has no place to keep.
//
// **And the grammar is shaped to the API in two places**, deliberately:
//
//   * a local says its type — `int sum = …` rather than `var sum = …` — because
//     `Expression.Variable` needs one when the declaration is read, and the initializer
//     is not built until long after;
//   * a name means one thing for the whole lambda: a block does not shadow. Shadowing
//     needs the scope to be entered and left around a *construction*, and construction
//     is not where the reading is.
//
// Both are the API's requirements showing through, which is what a real integration
// looks like.

[Gram("""
	@using System.Linq.Expressions;
	@using DotGram.Parsers;

	using Lexical;

	namespace Lexical
	{
		trivia = none

		Word   = [\p{L} | '_'] & [\p{L} | \p{Nd} | '_']*
		Digits = ['0'..'9']+
		Number = Digits & ('.' & Digits)?
	}

	// §4.6: a keyword is a whole word, so `returned` is a name and not a jump.
	wordboundary = [\p{L} | \p{Nd} | '_']

	trivia = { (' ' | '\t' | '\r' | '\n')* }

	// ── A lambda: what it takes, and what it does ───────────────────────────────

	Lambda : @LambdaExpression
		= '(' & (first: Parameter & (',' & rest: Parameter)*)? & ')' & "=>" & body: Body
		=> @(ExpressionLanguage.Lambda(body, first, rest))

	// The guard is the declaration: it runs while the text is being read, which is the
	// only moment at which anything in this grammar happens in the order it is written.
	Parameter : @ParameterExpression
		= type: Word & name: Word & when @(ExpressionLanguage.Declare(type, name))
		=> @(ExpressionLanguage.Named(name))

	Body : @Expression = b: Block => @(b) | e: Expression => @(e)

	// ── A block, and the statements it is made of ───────────────────────────────

	Block : @Expression = '{' & statements: Statement* & '}'
	                    => @(ExpressionLanguage.Block(statements))

	Statement : @Expression = s: Local => @(s) | s: Return => @(s)

	// `int sum = …`, not `var sum = …`: `Expression.Variable` wants a type where the
	// declaration is read, and what the initializer builds is not known until later.
	Local : @Expression
		= type: Word & name: Word & when @(ExpressionLanguage.Declare(type, name))
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
		= left: Equality & op: ("==" | "!=") & right: Relational
		  => @(ExpressionLanguage.Compare(op, left, right))
		| r: Relational => @(r)

	Relational : @Expression
		= left: Relational & op: ("<=" | ">=" | '<' | '>') & right: Additive
		  => @(ExpressionLanguage.Compare(op, left, right))
		| a: Additive => @(a)

	Additive : @Expression
		= left: Additive & op: ('+' | '-') & right: Multiplicative
		  => @(ExpressionLanguage.Arithmetic(op, left, right))
		| m: Multiplicative => @(m)

	Multiplicative : @Expression
		= left: Multiplicative & op: ('*' | '/' | '%') & right: Unary
		  => @(ExpressionLanguage.Arithmetic(op, left, right))
		| u: Unary => @(u)

	Unary : @Expression = '-' & operand: Unary => @(Expression.Negate(operand))
	                    | '!' & operand: Unary => @(Expression.Not(operand))
	                    | p: Primary           => @(p)

	Primary : @Expression
		= '(' & inner: Expression & ')' => @(inner)
		| text: Number                  => @(ExpressionLanguage.Literal(text))
		| "true"                        => @(Expression.Constant(true))
		| "false"                       => @(Expression.Constant(false))
		| name: Word                    => @(ExpressionLanguage.Named(name))

	parse Lambda as ParseLambda
	""")]
public static partial class ExpressionLanguage
{
	// ParseLambda and TryParseLambda are generated here.

	/// <summary>Reads the text as a lambda over an expression tree.</summary>
	/// <exception cref="FormatException">
	/// The text is not this language, or does not mean anything in it: a name nothing
	/// declares, a type it does not have, an operator its operands do not support.
	/// </exception>
	public static LambdaExpression Parse(string text)
	{
		_names.Clear();
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

	// ── The one thing System.Linq.Expressions has nowhere to keep ───────────────

	/// <summary>
	/// The variables of the lambda being read, by the name the text calls them.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A <c>ParameterExpression</c> is an identity: the one made for <c>(int x)</c> has
	/// to be the very object every <c>x</c> in the body reads, or the compiled lambda
	/// closes over a variable nothing assigns. Nothing in the API holds that mapping, so
	/// this does — filled by the guards while the text is read, and read by every
	/// <c>=&gt;</c> afterwards.
	/// </para>
	/// <para>
	/// Thread-static, and cleared where a parse begins: a table shared between two parses
	/// would let one lambda's <c>x</c> answer the other's.
	/// </para>
	/// </remarks>
	[ThreadStatic]
	static Dictionary<string, ParameterExpression>? _declared;

	static Dictionary<string, ParameterExpression> _names =>
		_declared ??= new Dictionary<string, ParameterExpression>(StringComparer.Ordinal);

	/// <summary>Where a <c>return</c> goes, made once the first one says what it yields.</summary>
	[ThreadStatic]
	static LabelTarget? _returns;

	/// <summary>
	/// A declaration, made while the text is read (§8.1) — which is the only moment this
	/// grammar has in the order it is written.
	/// </summary>
	/// <returns>
	/// Whether this reads as a declaration at all. A guard answers rather than throws,
	/// and it has to: <c>when</c> runs during the match, so it runs on readings the parse
	/// goes on to abandon — <c>int x</c> is also two words <c>in</c> and <c>t</c> while a
	/// repetition is giving characters back, and the answer for that one is no, which
	/// sends the parse to the reading that is a declaration.
	/// </returns>
	/// <remarks>
	/// The cost of declaring here rather than in a <c>=&gt;</c> is that an abandoned
	/// reading leaves its name behind, so a use of a name only a failed path declared
	/// resolves to a variable no lambda holds — which <c>Expression.Lambda</c> refuses,
	/// with a worse message than this could give. That, and no shadowing, is what this
	/// language pays for building the API's own objects and nothing else.
	/// </remarks>
	public static bool Declare(string type, string name)
	{
		if (TypeOf(type) is not { } declared)
			return false;

		_names[name] = Expression.Variable(declared, name);

		return true;
	}

	/// <summary>The variable that name was declared as.</summary>
	public static ParameterExpression Named(string name) =>
		_names.TryGetValue(name, out var found)
			? found
			: throw new FormatException($"nothing named '{name}' is in scope here.");

	// ── The rest is System.Linq.Expressions, said in the grammar's own terms ────

	/// <summary>A lambda over the parameters it declared, in the order it wrote them.</summary>
	public static LambdaExpression Lambda(Expression body, ParameterExpression? first, ParameterExpression[] rest)
	{
		var parameters = new List<ParameterExpression>();

		if (first is not null)
		{
			parameters.Add(first);
			parameters.AddRange(rest);
		}

		return Expression.Lambda(body, parameters);
	}

	/// <summary>
	/// A block: the variables it declared, its statements, and the label its returns go to.
	/// </summary>
	/// <remarks>
	/// The variables are read back out of the assignments rather than collected
	/// separately — an assignment to a variable is what a declaration became, so the
	/// block's own statements already say which they are.
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

		var body = new List<Expression>(statements);

		// The label is the block's value, and the tree asks for a default for the path
		// that falls off the end — which this language does not allow and the tree has no
		// way to know.
		body.Add(Expression.Label(_returns, Expression.Default(_returns.Type)));

		return Expression.Block(variables, body);
	}

	/// <summary>A jump to the block's label, which the first return is what makes.</summary>
	public static Expression Return(Expression value)
	{
		if (value is null)
			throw new ArgumentNullException(nameof(value));

		_returns ??= Expression.Label(value.Type, "return");

		if (_returns.Type != value.Type)
			throw new FormatException(
				$"one return yields '{_returns.Type}' and another '{value.Type}'.");

		return Expression.Return(_returns, value);
	}

	/// <summary>A number, read as the narrowest of <c>int</c> and <c>double</c> that holds it.</summary>
	/// <remarks>
	/// The rule C# applies to an unsuffixed literal, and the reason the grammar hands the
	/// text over rather than a value: what a literal <em>is</em> is a question about the
	/// host's types, not about the characters.
	/// </remarks>
	public static Expression Literal(string text) =>
		text.IndexOf('.') < 0 && int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var whole)
			? Expression.Constant(whole)
			: Expression.Constant(double.Parse(text, CultureInfo.InvariantCulture));

	public static Expression Arithmetic(string op, Expression left, Expression right)
	{
		(left, right) = Widened(left, right);

		return op switch
		{
			"+" => Expression.Add(left, right),
			"-" => Expression.Subtract(left, right),
			"*" => Expression.Multiply(left, right),
			"/" => Expression.Divide(left, right),
			"%" => Expression.Modulo(left, right),
			_   => throw new FormatException($"'{op}' is not an arithmetic operator."),
		};
	}

	public static Expression Compare(string op, Expression left, Expression right)
	{
		(left, right) = Widened(left, right);

		return op switch
		{
			"<"  => Expression.LessThan(left, right),
			">"  => Expression.GreaterThan(left, right),
			"<=" => Expression.LessThanOrEqual(left, right),
			">=" => Expression.GreaterThanOrEqual(left, right),
			"==" => Expression.Equal(left, right),
			"!=" => Expression.NotEqual(left, right),
			_    => throw new FormatException($"'{op}' is not a comparison."),
		};
	}

	/// <summary>The two operands at one type, the wider of them.</summary>
	/// <remarks>
	/// An <c>int</c> added to a <c>double</c> is a <c>double</c> addition, as in C#. The
	/// tree does not widen on its own: it asks for two operands of one type and says so
	/// by throwing, so the widening is written here where the language means it.
	/// </remarks>
	static (Expression Left, Expression Right) Widened(Expression left, Expression right)
	{
		if (left is null)  throw new ArgumentNullException(nameof(left));
		if (right is null) throw new ArgumentNullException(nameof(right));

		if (left.Type == right.Type)
			return (left, right);

		var rank = new[] { typeof(int), typeof(long), typeof(double), typeof(decimal) };
		var wide = Array.IndexOf(rank, left.Type);
		var thin = Array.IndexOf(rank, right.Type);

		if (wide < 0 || thin < 0)
			throw new FormatException($"'{left.Type}' and '{right.Type}' have no type in common.");

		return wide > thin
			? (left, Expression.Convert(right, left.Type))
			: (Expression.Convert(left, right.Type), right);
	}

	/// <summary>The types this language names, and only those.</summary>
	/// <remarks>
	/// A closed list rather than a lookup into the host's types: what a parser accepts
	/// should be readable from the parser, and a lambda meaning two things in two
	/// assemblies is not a language.
	/// </remarks>
	static Type? TypeOf(string name) => name switch
	{
		"int"     => typeof(int),
		"long"    => typeof(long),
		"double"  => typeof(double),
		"decimal" => typeof(decimal),
		"bool"    => typeof(bool),
		"string"  => typeof(string),
		_         => null,
	};
}
