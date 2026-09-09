using System;
using System.Collections.Generic;

using DotGram;

namespace DotGram.Examples;

// A selector — `orders[2].lines.total(net)` — read as the chain of steps it is.
//
// This is the shape every expression language has and the one levels-as-rules cannot
// write: a postfix step applies to whatever came before it, including another postfix
// step, so the rule for "a selector with a step on it" begins with the rule for "any
// selector", which begins with the rule for "a selector with a step on it". Written as
// it reads that is indirect left recursion, and §4.3 refuses it — unless every rule
// between the two only forwards, which is what `Selector` does here:
//
//   Selector = s: Applied => @(s) | s: Root => @(s)
//   Applied  = target: Selector & step: Step => @(...)
//
// `Selector` is a name for the two things it forwards and nothing else, so the leading
// `Selector` inside `Applied` is the choice of those two, the alternative distributes
// over it, and what is left is `Applied` calling itself leftmost — direct recursion,
// which the generator folds. Nothing is written differently from how it reads.
//
// The grouping is left-associative because that is where the recursion is (§4.3):
// `a.b.c` is `(a.b).c`, so `Steps` below comes back in reading order.
//
// **All the postfix steps go in one rule**, and that is not a style choice. Written as
// three recursive rules — `Member`, `Index`, `Apply`, each beginning with `Selector` —
// unfolding leaves them recursive through *each other*, which no rewrite can remove
// and §4.3 rightly refuses. One rule whose tail is a choice of steps is the same
// language and folds.
//
// Two smaller things worth copying:
//
//   * `Bracketed(item, open, close)` is one rule used twice. A parameter names a piece
//     of grammar (§4.2), and a literal is one, so a call passes `'['` and the
//     specialization matches that character — no delegate, no indirection, the same cost
//     as writing the bracket where the call is.
//
//   * `parse (b: Bracketed(Digits, '[', ']') => @(b)) as ParseSubscript : @string`
//     publishes an expression rather than a rule (§6). Without it a parameterized rule
//     cannot be reached from a directive at all; the type is what makes the `=>` inside
//     it legal.

[Gram("""
	using Lexical;

	namespace Lexical
	{
		trivia = none

		Name   = [\p{L} | '_'] & [\p{L} | \p{Nd} | '_']*
		Digits = ['0'..'9']+
	}

	trivia = { ' '* }

	// `Selector` is only a name for the two things a selector can be.
	Selector : @Step = s: Applied => @(s) | s: Root => @(s)

	Applied  : @Step = target: Selector & step: Step => @(new Step(target, step))
	Root     : @Step = name: Name                    => @(new Step(null, name))

	// Every postfix step, in one rule: `.name`, `[0]`, `(arg)`.
	Step : @string = t: ('.' & Name | Subscript | Arguments) => @(t)

	// One rule, two pairs of brackets: an argument is a piece of grammar, and a literal
	// is one.
	Bracketed(item, open, close) : @string = t: (open & item & close) => @(t)

	Subscript : @string = t: Bracketed(Digits, '[', ']') => @(t)
	Arguments : @string = t: Bracketed(Name,   '(', ')') => @(t)

	parse Selector as ParseSelector

	parse (b: Bracketed(Digits, '[', ']') => @(b)) as ParseSubscript : @string
	""")]
public static partial class Selectors
{
	// ParseSelector, TryParseSelector, ParseSubscript and TryParseSubscript are
	// generated here.

	/// <summary>One step of a selector, and what it was applied to.</summary>
	/// <remarks>
	/// The chain is held the way it is read — each step pointing at the one before it —
	/// because that is what a left-recursive fold builds. <see cref="Steps"/> turns it
	/// back into reading order for a caller who would rather have a list.
	/// </remarks>
	public sealed record Step(Step? Target, string Text);

	/// <summary>The steps of a selector, in the order they were written.</summary>
	public static IReadOnlyList<string> Steps(Step selector)
	{
		if (selector is null)
			throw new ArgumentNullException(nameof(selector));

		var steps = new List<string>();

		for (var at = selector; at is not null; at = at.Target)
			steps.Add(at.Text);

		steps.Reverse();

		return steps;
	}

	/// <summary>The selector as it would be written again, which is what it was read from.</summary>
	public static string Written(Step selector) => string.Concat(Steps(selector));
}
