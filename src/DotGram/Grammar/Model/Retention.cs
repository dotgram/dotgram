using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>How much of a line a construct can take.</summary>
public enum LineExtent
{
	/// <summary>No path through it consumes a line terminator.</summary>
	None,

	/// <summary>It may consume one, and nothing follows. A line, terminator included.</summary>
	AtEnd,

	/// <summary>It may consume one and go on, so what it takes is not one line but many.</summary>
	Beyond,
}

/// <summary>
/// How much input a grammar can be made to hold, and therefore whether it can stream.
/// </summary>
/// <remarks>
/// <para>
/// docs/syntax.md §6.3 emits the streaming overloads only where the grammar provably works
/// with a reused buffer, and the rule it rests on is frozen in §4: backtracking does not
/// cross a rule boundary, so a call reaches back not at all and a rule reaches back exactly
/// as far as it has consumed. What must be held is the extent of the outermost rule still
/// in progress.
/// </para>
/// <para>
/// A line-oriented reader gives that a unit, and the question becomes what a rule can take
/// measured in lines. Three answers are enough, and the difference that matters is not
/// whether a terminator is consumed but whether anything follows it: <c>None</c> is a
/// field, <c>AtEnd</c> is a whole record, and both fit a buffer of one line. Only
/// <c>Beyond</c> does not.
/// </para>
/// </remarks>
public static class Retention
{
	/// <summary>
	/// What each rule can take, in lines.
	/// </summary>
	/// <remarks>
	/// A fixpoint, like nullability, and for the same reason: a rule's answer depends on
	/// the rules it calls, and recursion means the dependency is not a tree. Starting at
	/// the smallest answer and growing is what makes a cycle settle — a rule that reaches a
	/// terminator only through itself reaches one never.
	/// </remarks>
	public static Dictionary<RuleSymbol, LineExtent> ExtentOf(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var consuming = Consuming(graph);
		var extents   = new Dictionary<RuleSymbol, LineExtent>();

		foreach (var rule in graph.Rules)
			extents[rule] = LineExtent.None;

		for (var changed = true; changed; )
		{
			changed = false;

			foreach (var rule in graph.Rules)
			{
				var extent = Extent(graph.Bodies[rule], extents, consuming);

				if (extent > extents[rule])
				{
					extents[rule] = extent;
					changed       = true;
				}
			}
		}

		return extents;
	}

	/// <summary>
	/// Every rule that can take at least one input item.
	/// </summary>
	/// <remarks>
	/// Its own fixpoint, and needed before the extents because a call is what tells a
	/// terminator at the end of a rule from one in the middle of it. Guessing that a call
	/// consumes would make <c>eol &amp; eof</c> two lines, which it plainly is not.
	/// </remarks>
	static HashSet<RuleSymbol> Consuming(RecognitionGraph graph)
	{
		var consuming = new HashSet<RuleSymbol>();

		for (var changed = true; changed; )
		{
			changed = false;

			foreach (var rule in graph.Rules)
				if (!consuming.Contains(rule) && Consumes(graph.Bodies[rule], consuming))
				{
					consuming.Add(rule);
					changed = true;
				}
		}

		return consuming;
	}

	/// <summary>What one node can take, given what is known of the rules so far.</summary>
	public static LineExtent Extent(
		Node                                        node,
		IReadOnlyDictionary<RuleSymbol, LineExtent> rules,
		ICollection<RuleSymbol>                     consuming)
	{
		switch (node)
		{
			case Node.Literal(var text):
				return text.IndexOf('\n') < 0 && text.IndexOf('\r') < 0
					? LineExtent.None
					: EndsWithTerminator(text) ? LineExtent.AtEnd : LineExtent.Beyond;

			case Node.Element element:
				return Admits(element) ? LineExtent.AtEnd : LineExtent.None;

			// Consumes nothing, so it takes no part of a line. What a lookahead needs to
			// *see* is a window question rather than a retention one, and §6.3 does not
			// answer it.
			case Node.Lookahead:
			case Node.Guard:
			case Node.Empty:
				return LineExtent.None;

			case Node.Call(var called, _):
				return rules.TryGetValue(called, out var known) ? known : LineExtent.None;

			case Node.Capture(_, var captured): return Extent(captured, rules, consuming);
			case Node.Construct(var built, _):  return Extent(built, rules, consuming);

			case Node.Choice(var alternatives):
			{
				var worst = LineExtent.None;

				foreach (var alternative in alternatives)
				{
					var extent = Extent(alternative, rules, consuming);

					if (extent > worst)
						worst = extent;
				}

				return worst;
			}

			// Once round is a line at most; twice round is that line and the next one.
			case Node.Repeat(var body, _, var max):
			{
				if (max == 0)
					return LineExtent.None;

				var once = Extent(body, rules, consuming);

				return once == LineExtent.AtEnd && max != 1 ? LineExtent.Beyond : once;
			}

			case Node.Sequence(var parts):
			{
				var soFar = LineExtent.None;

				foreach (var part in parts)
				{
					// A terminator with something after it is the whole of what `Beyond`
					// means: the parse is on the next line and still holding this one.
					if (soFar == LineExtent.AtEnd && Consumes(part, consuming))
						return LineExtent.Beyond;

					var extent = Extent(part, rules, consuming);

					if (extent > soFar)
						soFar = extent;

					if (soFar == LineExtent.Beyond)
						return soFar;
				}

				return soFar;
			}

			default:
				return LineExtent.None;
		}
	}

	/// <summary>
	/// Whether a node can take at least one input item.
	/// </summary>
	/// <remarks>
	/// What tells a terminator at the end from one in the middle. Answered generously —
	/// anything that might consume counts — because the cost of saying yes is a grammar
	/// that loses an overload it could have had, and of saying no is one that loses data.
	/// </remarks>
	static bool Consumes(Node node, ICollection<RuleSymbol> consuming) => node switch
	{
		Node.Empty or Node.Guard or Node.Lookahead => false,
		Node.Literal(var text)                     => text.Length > 0,
		Node.Repeat(var body, _, var max)          => max != 0 && Consumes(body, consuming),
		Node.Capture(_, var captured)              => Consumes(captured, consuming),
		Node.Construct(var built, _)               => Consumes(built, consuming),
		Node.Sequence(var parts)                   => Any(parts, consuming),
		Node.Choice(var alternatives)              => Any(alternatives, consuming),
		Node.Call(var called, _)                   => consuming.Contains(called),
		_                                          => true,
	};

	static bool Any(IReadOnlyList<Node> nodes, ICollection<RuleSymbol> consuming)
	{
		foreach (var node in nodes)
			if (Consumes(node, consuming))
				return true;

		return false;
	}

	/// <summary>Whether the last item of a literal is a terminator.</summary>
	static bool EndsWithTerminator(string text) =>
		text.Length > 0 && text[text.Length - 1] is '\n' or '\r';

	/// <summary>
	/// Whether an element set admits a line terminator.
	/// </summary>
	/// <remarks>
	/// A Unicode category is not looked into: <c>\p{Cc}</c> contains both terminators and
	/// several others could be argued about. Treated as admitting one, which is again the
	/// direction that costs an overload rather than data.
	/// </remarks>
	static bool Admits(Node.Element element)
	{
		if (element.Categories.Count > 0)
			return true;

		var named = false;

		foreach (var range in element.Ranges)
			if ((range.From <= '\n' && '\n' <= range.To) || (range.From <= '\r' && '\r' <= range.To))
			{
				named = true;

				break;
			}

		return element.IsNegated ? !named : named;
	}
}
