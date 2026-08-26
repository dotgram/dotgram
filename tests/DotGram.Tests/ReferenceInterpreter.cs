using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Tests;

/// <summary>
/// §11, executed naively: a backtracking interpreter over the recognition graph.
/// </summary>
/// <remarks>
/// <para>
/// The generated automaton is an optimized implementation of the semantics; this is the
/// semantics with no optimization at all — ordered choice tries alternatives in written
/// order, a repetition is greedy and hands turns back one at a time, an atomic group
/// keeps only its first reading, a lookahead consumes nothing. Every reading a construct
/// admits is enumerated lazily, in preference order, so "the parse succeeds" means what
/// §11 says it means: some path through the readings reaches the end.
/// </para>
/// <para>
/// Its worth is being <em>obviously</em> right where the automaton is <em>subtly</em>
/// right. The differential fuzzer compiles a random grammar, runs both on random inputs,
/// and any disagreement is a defect in the generator or in this — and this is thirty
/// lines of recursion per construct, so the argument is short.
/// </para>
/// <para>
/// It interprets what a grammar can mean without a host: no guards, no externals, no
/// constructions' C#. The fuzzer's grammar generator stays inside that fence.
/// </para>
/// </remarks>
public static class ReferenceInterpreter
{
	/// <summary>Whether the whole input reads as <paramref name="rule"/> (§11 `parse`).</summary>
	public static bool Parses(RecognitionGraph graph, RuleSymbol rule, string text)
	{
		if (graph is null) throw new ArgumentNullException(nameof(graph));
		if (rule is null)  throw new ArgumentNullException(nameof(rule));
		if (text is null)  throw new ArgumentNullException(nameof(text));

		var body = graph.Bodies[rule];

		if (graph.Trivia.TryGetValue(rule, out var trivia))
			body = new Node.Sequence([trivia, body, trivia]);

		foreach (var end in Ends(body, text, 0, graph))
			if (end == text.Length)
				return true;

		return false;
	}

	/// <summary>
	/// Every position the node can end at from <paramref name="at"/>, lazily, most
	/// preferred first.
	/// </summary>
	static IEnumerable<int> Ends(Node node, string text, int at, RecognitionGraph graph)
	{
		switch (node)
		{
			case Node.Empty:
				yield return at;

				break;

			case Node.Literal(var value) { IgnoreCase: var folded }:
			{
				if (at + value.Length > text.Length)
					break;

				for (var i = 0; i < value.Length; i++)
				{
					var have = folded ? char.ToUpperInvariant(text[at + i]) : text[at + i];
					var want = folded ? char.ToUpperInvariant(value[i])     : value[i];

					if (have != want)
						yield break;
				}

				yield return at + value.Length;

				break;
			}

			case Node.Element(var negated, var ranges, var categories, var references):
			{
				if (references.Count > 0)
					throw new NotSupportedException("A C# predicate has no reference semantics.");

				if (at >= text.Length)
					break;

				var inside = ranges.Any(range => range.From <= text[at] && text[at] <= range.To) ||
					categories
						.SelectMany(UnicodeCategories.Expand)
						.Any(name => CharUnicodeInfo.GetUnicodeCategory(text[at]).ToString() == name);

				if (inside != negated)
					yield return at + 1;

				break;
			}

			case Node.Sequence(var parts):
				foreach (var end in Sequenced(parts, 0, text, at, graph))
					yield return end;

				break;

			case Node.Choice(var alternatives):
				foreach (var alternative in alternatives)
					foreach (var end in Ends(alternative, text, at, graph))
						yield return end;

				break;

			// Greedy: the readings with another turn come before the one that stops here,
			// and every suffix of turns is offered — which is exactly the give-back the
			// engine implements with its ways back.
			case Node.Repeat(var body, var min, var max):
				foreach (var end in Turns(body, min, max ?? int.MaxValue, 0, text, at, graph))
					yield return end;

				break;

			// One reading only: whatever the body prefers first is what the group means,
			// and nothing ever comes back for another.
			case Node.Atomic(var kept):
				foreach (var end in Ends(kept, text, at, graph))
				{
					yield return end;

					break;
				}

				break;

			case Node.Lookahead(var positive, var seen):
			{
				var matched = Ends(seen, text, at, graph).Any();

				if (matched == positive)
					yield return at;

				break;
			}

			case Node.Capture(_, var captured):
				foreach (var end in Ends(captured, text, at, graph))
					yield return end;

				break;

			case Node.Construct(var built, _):
				foreach (var end in Ends(built, text, at, graph))
					yield return end;

				break;

			case Node.Call(var called, _):
				foreach (var end in Ends(graph.Bodies[called], text, at, graph))
					yield return end;

				break;

			default:
				throw new NotSupportedException($"{node.GetType().Name} has no reference semantics.");
		}
	}

	static IEnumerable<int> Sequenced(
		IReadOnlyList<Node> parts, int index, string text, int at, RecognitionGraph graph)
	{
		if (index == parts.Count)
		{
			yield return at;

			yield break;
		}

		foreach (var end in Ends(parts[index], text, at, graph))
			foreach (var rest in Sequenced(parts, index + 1, text, end, graph))
				yield return rest;
	}

	static IEnumerable<int> Turns(
		Node body, int min, int max, int taken, string text, int at, RecognitionGraph graph)
	{
		var zeroWidth = false;

		if (taken < max)
			foreach (var end in Ends(body, text, at, graph))
			{
				// A turn that consumed nothing is not recursed into — it would recurse for
				// ever — but it is not discarded either: the engine counts such turns, so
				// `('a'?){3}` reads the empty input by taking three of them. Its whole
				// effect is that any count up to the bound is reachable right here.
				if (end == at)
				{
					zeroWidth = true;

					continue;
				}

				foreach (var rest in Turns(body, min, max, taken + 1, text, end, graph))
					yield return rest;
			}

		if (taken >= min || zeroWidth)
			yield return at;
	}
}
