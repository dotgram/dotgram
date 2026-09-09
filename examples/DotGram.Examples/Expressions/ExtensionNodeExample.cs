using System;
using System.Globalization;
using System.Linq.Expressions;

using DotGram;

namespace DotGram.Examples;

// A `=>` may build a node `System.Linq.Expressions` has no factory for.
//
// The API's hundred and twenty factories are what it knows how to make. What it also has
// — and what is not a factory, because it is reached by deriving rather than by calling —
// is the extension node: a class of your own over `Expression`, carrying whatever it
// likes, that says how to become ordinary nodes when something finally asks. `Compile`
// asks. Until then the tree holds your node, with your name on it and your operands
// where you put them.
//
//     var tree = ClampedExample.Read("clamp(x, 0, 10)");
//
//     tree.Body                      // a ClampExpression, not the four nodes it becomes
//     ((ClampExpression)tree.Body).CanReduce   // true
//     tree.Compile()(15)             // 10
//
// Nothing in the notation or the generator knows about any of this, and that is the whole
// point of the example: a `=>` is C#, a rule's `: @T` is a C# type, and `ClampExpression`
// is an `Expression`. A language whose values are trees of some API is not limited to the
// nodes that API ships — which is worth knowing before writing the visitor that would
// otherwise have to recognize the expansion and guess what it had been.
//
// Why not expand it in the `=>` and be done? Because the expansion is a decision, and
// making it at parse time throws away the only moment anything could have decided
// otherwise. A tree that still says `clamp` can be printed as `clamp`, rewritten by a
// pass that knows what clamping is, or compiled — and the compiling is the one path that
// needs the expansion, which is exactly when `Reduce` runs.

// One thing about the writing rather than about the idea: this namespace already has an
// `Expression` — the record tree `ExpressionTreeExample` builds — and a name declared in
// the namespace beats one imported into it. So the API's is written out in full wherever
// the grammar names it. Copy this into a file whose namespace has no such thing and the
// `@using` alone is enough.

[Gram("""
	@using System.Globalization;
	@using System.Linq.Expressions;
	@using DotGram.Examples;

	trivia = [' ' | '\t']*

	// One parameter, so that the example is about the node and not about scopes.
	Body : @System.Linq.Expressions.LambdaExpression
		= body: Sum
		=> @(System.Linq.Expressions.Expression.Lambda(body, ClampedExample.Argument))

	Sum : @System.Linq.Expressions.Expression
		= left: Sum & '+' & right: Term
		  => @(System.Linq.Expressions.Expression.Add(left, right))
		| t: Term => @(t)

	Term : @System.Linq.Expressions.Expression
		// The node the API does not have. `=>` names its constructor the way every other
		// alternative here names a factory, and nothing tells the two apart from here.
		= "clamp" & '(' & value: Sum & ',' & low: Sum & ',' & high: Sum & ')'
		  => @(new ClampExpression(value, low, high))

		| '(' & inner: Sum & ')' => @(inner)
		| "x"                    => @(ClampedExample.Argument)
		| digits: ['0'..'9']+
		  => @(System.Linq.Expressions.Expression.Constant(
			int.Parse(digits, CultureInfo.InvariantCulture)))

	parse Body as Read
	""")]
public static partial class ClampedExample
{
	// Read and TryRead are generated here.

	/// <summary>The one parameter every expression this example reads is written over.</summary>
	/// <remarks>
	/// A field rather than something the grammar declares, because this example is about
	/// what a `=&gt;` may build and not about how a parameter comes to be named — the
	/// expression language in <c>DotGram.Parsers</c> is where that question is answered.
	/// </remarks>
	public static readonly ParameterExpression Argument =
		System.Linq.Expressions.Expression.Parameter(typeof(int), "x");
}

/// <summary>`clamp(value, low, high)`, which stays itself until something needs it not to.</summary>
/// <remarks>
/// <para>
/// The three things an extension node is: <c>NodeType</c> is
/// <see cref="ExpressionType.Extension"/>, <see cref="CanReduce"/> says it can become
/// something else, and <see cref="Reduce"/> says what. Everything that walks a tree —
/// <c>ExpressionVisitor</c>, the compiler, a printer of your own — knows those three and
/// so knows this node without ever having heard of it.
/// </para>
/// <para>
/// <see cref="VisitChildren"/> is what makes it rewritable: a visitor that replaces an
/// operand gets a new clamp rather than a clamp that quietly kept the old one. Skipping it
/// is the usual bug in a first extension node, and it is silent.
/// </para>
/// </remarks>
public sealed class ClampExpression : System.Linq.Expressions.Expression
{
	public ClampExpression(
		System.Linq.Expressions.Expression value,
		System.Linq.Expressions.Expression low,
		System.Linq.Expressions.Expression high)
	{
		Value = value ?? throw new ArgumentNullException(nameof(value));
		Low   = low   ?? throw new ArgumentNullException(nameof(low));
		High  = high  ?? throw new ArgumentNullException(nameof(high));
	}

	public System.Linq.Expressions.Expression Value { get; }
	public System.Linq.Expressions.Expression Low   { get; }
	public System.Linq.Expressions.Expression High  { get; }

	public override ExpressionType NodeType => ExpressionType.Extension;
	public override Type           Type     => Value.Type;
	public override bool           CanReduce => true;

	/// <summary>What it becomes, and only where something asks.</summary>
	public override System.Linq.Expressions.Expression Reduce() =>
		Condition(
			LessThan(Value, Low),
			Low,
			Condition(GreaterThan(Value, High), High, Value));

	/// <summary>Rewritten with whatever a visitor made of the operands.</summary>
	protected override System.Linq.Expressions.Expression VisitChildren(ExpressionVisitor visitor)
	{
		if (visitor is null)
			throw new ArgumentNullException(nameof(visitor));

		var value = visitor.Visit(Value);
		var low   = visitor.Visit(Low);
		var high  = visitor.Visit(High);

		return ReferenceEquals(value, Value) &&
			ReferenceEquals(low, Low) &&
			ReferenceEquals(high, High)
				? this
				: new ClampExpression(value, low, high);
	}

	public override string ToString() => $"clamp({Value}, {Low}, {High})";
}
