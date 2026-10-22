using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
/// A binding (<c>$q: X</c>) is a record of where its body began and ended, carried along each
/// path as an immutable map from binding to span: a path that gives an item back takes the
/// shorter record with it, which is the whole of what §3.5.1 says about backtracking. A call
/// starts with no bindings and hands its caller's back unchanged, because a binding never
/// crosses a rule.
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

		foreach (var (end, _) in Ends(body, text, 0, Unbound, graph))
			if (end == text.Length)
				return true;

		return false;
	}

	/// <summary>
	/// Where a reading of <paramref name="rule"/> begun at <paramref name="at"/> ends, or -1
	/// where there is none (§6.3: no end of input is demanded, so the first reading is the one).
	/// </summary>
	/// <remarks>
	/// The trivia before the rule is read and the trivia after it is not: what a reading from a
	/// position leaves after the value is the leading trivia of the next one.
	/// </remarks>
	public static int Reads(RecognitionGraph graph, RuleSymbol rule, string text, int at)
	{
		if (graph is null) throw new ArgumentNullException(nameof(graph));
		if (rule is null)  throw new ArgumentNullException(nameof(rule));
		if (text is null)  throw new ArgumentNullException(nameof(text));

		var body = graph.Bodies[rule];

		if (graph.Trivia.TryGetValue(rule, out var trivia))
			body = new Node.Sequence([trivia, body]);

		foreach (var (end, _) in Ends(body, text, at, Unbound, graph))
			return end;

		return -1;
	}

	/// <summary>Where each binding on a path began and ended.</summary>
	static readonly ImmutableDictionary<TextBinding, (int From, int To)> Unbound =
		ImmutableDictionary.Create<TextBinding, (int From, int To)>(ReferenceEqualityComparer.Instance);

	/// <summary>
	/// Every position the node can end at from <paramref name="at"/>, with the bindings the path
	/// to it made, lazily, most preferred first.
	/// </summary>
	static IEnumerable<(int End, ImmutableDictionary<TextBinding, (int From, int To)> Bound)> Ends(
		Node node, string text, int at, ImmutableDictionary<TextBinding, (int From, int To)> bound, RecognitionGraph graph)
	{
		switch (node)
		{
			case Node.Empty:
				yield return (at, bound);

				break;

			case Node.Literal(var value) { IgnoreCase: var folded }:
			{
				if (at + value.Length > text.Length)
					break;

				for (var i = 0; i < value.Length; i++)
					if (folded ? !CaseFold.Matches(value[i], text[at + i]) : text[at + i] != value[i])
						yield break;

				yield return (at + value.Length, bound);

				break;
			}

			case Node.Element element:
				if (Admits(element, text, at))
					yield return (at + 1, bound);

				break;

			case Node.Sequence(var parts):
				foreach (var end in Sequenced(parts, 0, text, at, bound, graph))
					yield return end;

				break;

			case Node.Choice(var alternatives):
				foreach (var alternative in alternatives)
					foreach (var end in Ends(alternative, text, at, bound, graph))
						yield return end;

				break;

			// Greedy: the readings with another turn come before the one that stops here,
			// and every suffix of turns is offered — which is exactly the give-back the
			// engine implements with its ways back.
			case Node.Repeat(var body, var min, var max):
				foreach (var end in Turns(body, min, max ?? int.MaxValue, 0, text, at, bound, graph))
					yield return end;

				break;

			// The same, with a bound that is the length of a binding read here, on this path
			// (§3.5.1). A count that cannot be met — more at least than at most — reads nothing.
			case Node.Counted(var body, var min, var max):
			{
				var least = Count(min, bound);
				var most  = max is null ? int.MaxValue : Count(max, bound);

				if (least > most)
					break;

				foreach (var end in Turns(body, least, most, 0, text, at, bound, graph))
					yield return end;

				break;
			}

			// One reading only: whatever the body prefers first is what the group means,
			// and nothing ever comes back for another.
			case Node.Atomic(var kept):
				foreach (var end in Ends(kept, text, at, bound, graph))
				{
					yield return end;

					break;
				}

				break;

			// A look reads the bindings outside it and makes none that outlive it.
			case Node.Lookahead(var positive, var seen):
			{
				var matched = Ends(seen, text, at, bound, graph).Any();

				if (matched == positive)
					yield return (at, bound);

				break;
			}

			case Node.Behind(var boundary):
				if (at == 0 || !Admits(boundary, text, at - 1))
					yield return (at, bound);

				break;

			case Node.Capture(_, var captured):
				foreach (var end in Ends(captured, text, at, bound, graph))
					yield return end;

				break;

			case Node.Construct(var built, _):
				foreach (var end in Ends(built, text, at, bound, graph))
					yield return end;

				break;

			// A rule's bindings are its own: the callee begins with none, and the caller's
			// are what they were when it returns.
			case Node.Call(var called, _):
				foreach (var (end, _) in Ends(graph.Bodies[called], text, at, Unbound, graph))
					yield return (end, bound);

				break;

			// Each reading of the body is a record of its own: a greedy body that gives an
			// item back binds the shorter text on that path.
			case Node.Bind(var binding, var body, _):
				foreach (var (end, inner) in Ends(body, text, at, bound, graph))
					yield return (end, inner.SetItem(binding, (at, end)));

				break;

			case Node.Same(var binding, _):
			{
				var (from, to) = Recorded(binding, bound);
				var length     = to - from;

				if (at + length > text.Length)
					break;

				if (string.CompareOrdinal(text, from, text, at, length) != 0)
					break;

				yield return (at + length, bound);

				break;
			}

			default:
				throw new NotSupportedException($"{node.GetType().Name} has no reference semantics.");
		}
	}

	static bool Admits(Node.Element element, string text, int at)
	{
		if (element.References.Count > 0)
			throw new NotSupportedException("A C# predicate has no reference semantics.");

		if (at >= text.Length)
			return false;

		var inside = element.Ranges.Any(range => range.From <= text[at] && text[at] <= range.To) ||
			element.Categories
				.SelectMany(UnicodeCategories.Expand)
				.Any(name => UnicodeCategories.Of(text[at]).ToString() == name);

		return inside != element.IsNegated;
	}

	static (int From, int To) Recorded(TextBinding binding, ImmutableDictionary<TextBinding, (int From, int To)> bound)
	{
		// The compiler refuses a reference some route reaches unbound (GRAM4032), so a path
		// that reaches one here is a defect of that check, and says so rather than guessing.
		if (!bound.TryGetValue(binding, out var span))
			throw new InvalidOperationException($"{binding} is read on a path that never bound it.");

		return span;
	}

	static int Count(CountBound count, ImmutableDictionary<TextBinding, (int From, int To)> bound)
	{
		switch (count)
		{
			case CountBound.Items(var items):
				return items;

			case CountBound.Length(var binding, _):
			{
				var (from, to) = Recorded(binding, bound);

				return to - from;
			}

			default:
				throw new NotSupportedException($"{count.GetType().Name} has no reference semantics.");
		}
	}

	static IEnumerable<(int End, ImmutableDictionary<TextBinding, (int From, int To)> Bound)> Sequenced(
		IReadOnlyList<Node> parts, int index, string text, int at, ImmutableDictionary<TextBinding, (int From, int To)> bound, RecognitionGraph graph)
	{
		if (index == parts.Count)
		{
			yield return (at, bound);

			yield break;
		}

		foreach (var (end, after) in Ends(parts[index], text, at, bound, graph))
			foreach (var rest in Sequenced(parts, index + 1, text, end, after, graph))
				yield return rest;
	}

	static IEnumerable<(int End, ImmutableDictionary<TextBinding, (int From, int To)> Bound)> Turns(
		Node body, int min, int max, int taken, string text, int at, ImmutableDictionary<TextBinding, (int From, int To)> bound, RecognitionGraph graph)
	{
		var zeroWidth = false;

		if (taken < max)
			foreach (var (end, after) in Ends(body, text, at, bound, graph))
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

				foreach (var rest in Turns(body, min, max, taken + 1, text, end, after, graph))
					yield return rest;
			}

		if (taken >= min || zeroWidth)
			yield return (at, bound);
	}
}
