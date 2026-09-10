using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>
/// Answers every <c>when A is B</c> and takes it out of the graph.
/// </summary>
/// <remarks>
/// <para>
/// The question is whether two recognizers have a string in common. It is asked of the
/// grammar rather than of the input, so it has an answer before anything is parsed — and
/// the point of asking it is that <c>with</c> substitutes a rule at every one of its uses,
/// so <c>parse S with (Version = "Sql2008")</c> gives every such question in that parser a
/// different answer from the one beside it.
/// </para>
/// <para>
/// Which is why it runs where it does: after all three substitution passes, so that what it
/// reads is what this parser will be, and before anything rewrites the shape of a rule. It
/// leaves nothing behind — a condition that holds becomes <see cref="Node.Empty"/>, and an
/// alternative whose condition failed is deleted. No parser tests any of this while it runs.
/// </para>
/// <para>
/// <b>Where the condition may stand.</b> As an operand of an alternative, which is where a
/// `when` is written and the only place deleting one means anything: an alternative is what
/// a grammar has to lose. Written inside a group, a repetition or a capture it is refused
/// rather than guessed at, because what it would mean there is a question nobody has asked
/// yet.
/// </para>
/// <para>
/// <b>What it can decide.</b> Both sides are reduced to the set of strings they accept,
/// following calls and unfolding choices and sequences of literals. That answers the
/// question this is for — a domain is a list of names and a range is another list — and
/// answers it exactly. Where a side is not a listable set of strings, the compiler says so
/// and keeps the alternative: refusing to decide out loud is the only honest answer, since
/// deciding it wrongly either way silently changes what the parser reads.
/// </para>
/// <para>
/// Intersection is decidable for wider languages than this — regular against regular is a
/// product automaton, and <see cref="LexicalAutomaton"/> already builds those. It is not
/// reached from here yet, and widening to it is a change to this file and nothing else.
/// </para>
/// </remarks>
public sealed partial class GrammarNormalizer
{
	/// <summary>How many strings a side may accept before this gives up on listing them.</summary>
	const int Listable = 4096;

	internal void DecideIntersections()
	{
		foreach (var rule in _bodies.Keys.ToList())
		{
			var body = _bodies[rule];

			if (!NodeWalk.Descendants(body).Concat([body]).Any(one => one is Node.Condition))
				continue;

			_bodies[rule] = Decided(rule, body);
		}
	}

	/// <summary>A rule's body with every alternative's condition answered.</summary>
	Node Decided(RuleSymbol rule, Node body)
	{
		if (body is not Node.Choice(var alternatives))
			return Kept(rule, body) ?? Nothing(rule, body);

		var left = new List<Node>(alternatives.Count);

		foreach (var alternative in alternatives)
			if (Kept(rule, alternative) is { } one)
				left.Add(one);

		return left.Count switch
		{
			0 => Nothing(rule, body),
			1 => left[0],
			_ => new Node.Choice(left),
		};
	}

	/// <summary>
	/// One alternative with its conditions answered, or null where one of them failed.
	/// </summary>
	/// <remarks>
	/// Through what an alternative wears rather than around it: a `=&gt;` and a binding power
	/// are written on the alternative and the condition is written among its operands, so
	/// the two are met on the way in and put back on the way out.
	/// </remarks>
	Node? Kept(RuleSymbol rule, Node alternative)
	{
		switch (alternative)
		{
			case Node.Construct(var built, var how):
				return Kept(rule, built) is { } made ? new Node.Construct(made, how) : null;
		}

		if (alternative is Node.Condition(var only, var where))
			return Holds(only, where) is false ? null : Node.Empty.Instance;

		if (alternative is not Node.Sequence(var operands))
			return Elsewhere(rule, alternative);

		var kept = new List<Node>(operands.Count);

		foreach (var operand in operands)
			switch (operand)
			{
				case Node.Condition(var test, var at) when Holds(test, at) is false:
					return null;

				case Node.Condition:
					// It held, so it costs nothing and says nothing. Kept as `Empty` rather
					// than dropped so that an alternative made only of conditions is still an
					// alternative that matches.
					kept.Add(Node.Empty.Instance);
					break;

				default:
					if (Elsewhere(rule, operand) is { } one)
						kept.Add(one);

					break;
			}

		return new Node.Sequence(kept);
	}

	/// <summary>
	/// A node that is not a condition, refused where a condition is hiding inside it.
	/// </summary>
	Node? Elsewhere(RuleSymbol rule, Node node)
	{
		if (!NodeWalk.Descendants(node).Any(one => one is Node.Condition))
			return node;

		Report(
			ConditionOutOfPlace,
			$"A `when … is …` in '{rule.Name}' stands inside a group, a repetition or a " +
			"capture. It is answered while the parser is built, and what is answered there is " +
			"whether an alternative is in this parser at all — so it belongs beside the " +
			"operands of one and nowhere else.",
			Where(node));

		return node;
	}

	/// <summary>A rule left with no alternative at all.</summary>
	/// <remarks>
	/// Not an error: a rule that is gone from this parser is what the condition was written
	/// to say, and a construct removed from a dialect has to be removable. What it becomes is
	/// a rule that cannot match — a negative lookahead of nothing, which fails wherever it is
	/// asked — so a caller of it fails where the construct is written and nowhere else.
	/// Remarked rather than reported, because a rule that vanishes silently is still worth a
	/// word: it is as often a condition nobody meant as a dialect nobody has.
	/// </remarks>
	Node Nothing(RuleSymbol rule, Node body)
	{
		Remark(
			EmptyAfterConditions,
			$"Every alternative of '{rule.Name}' is ruled out by a `when … is …` in this " +
			"parser, so it reads nothing here. That is what a removed construct looks like, " +
			"and it is only worth a second look where no removal was meant.",
			Where(body));

		return new Node.Lookahead(false, Node.Empty.Instance);
	}

	/// <summary>
	/// Whether a condition holds, or null where this compiler cannot say.
	/// </summary>
	/// <remarks>
	/// `and` and `or` are answered from their sides and are undecided where a side is —
	/// with the two shortcuts that make an undecided side not matter: `false and anything`
	/// is false and `true or anything` is true, whatever the other half turned out to be.
	/// </remarks>
	bool? Holds(Test test, int at)
	{
		switch (test)
		{
			case Test.Meets(var left, var right, var negated):
				return Answer(left, right, at) is { } met ? met != negated : null;

			case Test.All(var left, var right):
			{
				var ours   = Holds(left, at);
				var theirs = Holds(right, at);

				return ours is false || theirs is false ? false
					: ours is null || theirs is null ? null
					: true;
			}

			case Test.Any(var left, var right):
			{
				var ours   = Holds(left, at);
				var theirs = Holds(right, at);

				return ours is true || theirs is true ? true
					: ours is null || theirs is null ? null
					: false;
			}

			default:
				return null;
		}
	}

	/// <summary>
	/// Whether the two have a string in common, or null where this compiler cannot say.
	/// </summary>
	bool? Answer(Node left, Node right, int at)
	{
		var ours   = Strings(left, []);
		var theirs = Strings(right, []);

		if (ours is not null && theirs is not null)
			return ours.Overlaps(theirs);

		Report(
			UndecidedCondition,
			"`is` asks whether two recognizers have a string in common, and this compiler can " +
			"answer that only where each side accepts a listable set of strings — a literal, a " +
			"choice of them, or a rule that is one. The alternative is kept, as though the " +
			"condition held.",
			new Location(at < 0 ? 0 : at, 0));

		return null;
	}

	/// <summary>Every string a node accepts, or null where they are not a listable set.</summary>
	HashSet<string>? Strings(Node node, HashSet<RuleSymbol> seen)
	{
		switch (node)
		{
			case Node.Empty:
				return [""];

			// A literal is the one string it spells; one written `"v1"i` is every string
			// that spells it in any case, and those are listed rather than folded to one.
			// Raising only the insensitive side answered `"v1"i is "v1"` with false, which
			// is the plainest possible wrong answer.
			case Node.Literal(var text) literal:
				return literal.IgnoreCase ? Cases(text) : [text];

			case Node.Choice(var alternatives):
			{
				var all = new HashSet<string>(StringComparer.Ordinal);

				foreach (var one in alternatives)
				{
					if (Strings(one, seen) is not { } some)
						return null;

					all.UnionWith(some);

					if (all.Count > Listable)
						return null;
				}

				return all;
			}

			case Node.Sequence(var operands):
			{
				var all = new HashSet<string>(StringComparer.Ordinal) { "" };

				foreach (var one in operands)
				{
					if (Strings(one, seen) is not { } some)
						return null;

					if (all.Count * (long)some.Count > Listable)
						return null;

					all = [.. from head in all from tail in some select head + tail];
				}

				return all;
			}

			// A call is the rule it names, and a rule that reaches itself accepts strings of
			// no bound — which is exactly what cannot be listed.
			case Node.Call(var rule, _):
				return seen.Add(rule) && _bodies.TryGetValue(rule, out var body)
					? Strings(body, seen)
					: null;

			// The wrappers that change what is built rather than what is read.
			case Node.Capture(_, var inside):
				return Strings(inside, seen);

			case Node.Atomic(var braced):
				return Strings(braced, seen);

			case Node.Construct(var built, _):
				return Strings(built, seen);

			default:
				return null;
		}
	}

	/// <summary>Every way a case-insensitive literal may be spelled, or null where too many.</summary>
	/// <remarks>
	/// Two to the power of its letters, which is why the same bound applies: a domain is a
	/// list of short names and this stays small, while `"internationalization"i` is a
	/// question this refuses rather than answers slowly.
	/// </remarks>
	static HashSet<string>? Cases(string text)
	{
		var letters = text.Count(char.IsLetter);

		if (letters > 12)
			return null;

		var all = new HashSet<string>(StringComparer.Ordinal) { "" };

		foreach (var one in text)
		{
			var lower = char.ToLowerInvariant(one);
			var upper = char.ToUpperInvariant(one);

			all = lower == upper
				? [.. all.Select(head => head + one)]
				: [.. all.SelectMany(head => new[] { head + lower, head + upper })];
		}

		return all;
	}

	/// <summary>Where a node was written, as well as the graph can say.</summary>
	static Location Where(Node node) =>
		node is Node.Condition(_, var at) && at >= 0 ? new Location(at, 0) : new Location(0, 0);
}
