using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>
/// What a construct can begin with, and what that says about a grammar.
/// </summary>
/// <remarks>
/// <para>
/// Ordered choice hides ambiguity rather than reporting it. <c>Row* &amp; Trailer</c>
/// where a trailer also reads as a record parses perfectly well: the repetition takes the
/// trailer, the rule fails, the repetition gives it back, and the parse succeeds by the
/// second reading. Nothing is wrong with the answer and nothing tells the author that
/// their grammar had two of them and backtracking picked one.
/// </para>
/// <para>
/// It is worth saying because the cost is invisible until it is not: the grammar reads as
/// though the trailer is a trailer, a reader assumes the repetition stops at it, and the
/// engine only agrees by accident. It also decides what a streamed parse may do — an
/// element handed to the caller cannot be given back, so a repetition that would ever
/// want to is exactly the one a stream cannot read.
/// </para>
/// <para>
/// The sets are approximate, and in the safe direction: a construct this cannot decide
/// about answers "anything", two "anything"s overlap, and the report is a report rather
/// than a refusal. Being told about an overlap that is not real costs a sentence; missing
/// one costs the thing this exists to prevent.
/// </para>
/// </remarks>
public static class FirstSets
{
	/// <summary>A repetition that may be unable to tell its own end from its elements.</summary>
	public const string Ambiguous = "GRAM5002";

	/// <summary>
	/// What a construct can begin with.
	/// </summary>
	/// <param name="Anything">
	/// Nothing useful is known — a complement, a Unicode category, a C# predicate. Treated
	/// as overlapping everything, which is the direction that reports too much.
	/// </param>
	/// <param name="Nothing">It consumes nothing at all, so it begins with nothing.</param>
	public sealed record First(bool Anything, bool Nothing, IReadOnlyList<CharRange> Ranges)
	{
		public static readonly First All  = new(true,  false, []);
		public static readonly First None = new(false, true,  []);

		public bool Overlaps(First other)
		{
			if (Nothing || other.Nothing)
				return false;

			if (Anything || other.Anything)
				return true;

			foreach (var mine in Ranges)
				foreach (var theirs in other.Ranges)
					if (mine.From <= theirs.To && theirs.From <= mine.To)
						return true;

			return false;
		}
	}

	/// <summary>
	/// Every repetition whose end is not told apart from one more of its elements.
	/// </summary>
	public static IReadOnlyList<GramDiagnostic> Check(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var reported = new List<GramDiagnostic>();

		foreach (var rule in graph.Rules)
		{
			if (rule.Declaration is not { } declaration)
				continue;

			// Only a rule that asks to be read in parts. An overlap is not a defect on its
			// own — §11 makes backtracking total and a grammar is entitled to lean on it,
			// which is what `'a'+ & 'a'` does and means. It becomes a defect exactly where
			// the parse cannot go back: a rule declaring a sequence is asking to be handed
			// over an element at a time, and an element handed over cannot be taken back.
			if (!graph.Types.TryGetValue(rule, out var type) ||
				!type.EndsWith("[]", StringComparison.Ordinal))
			{
				continue;
			}

			Walk(graph.Bodies[rule], rule, declaration, reported, graph);
		}

		return reported;
	}

	static void Walk(
		Node node, RuleSymbol rule, Decl.Rule declaration,
		List<GramDiagnostic> reported, RecognitionGraph graph)
	{
		if (node is Node.Sequence(var parts))
			for (var i = 0; i < parts.Count - 1; i++)
			{
				// Only a repetition that may stop — one with an upper bound it can reach
				// without the input saying so. `X{3}` ends where it ends.
				if (parts[i] is not Node.Repeat(var body, _, var max) || max is not null)
					continue;

				var follows = Following(parts, i + 1, graph);

				if (!Of(body, graph).Overlaps(follows))
					continue;

				reported.Add(new GramDiagnostic(
					Ambiguous,
					$"In '{rule.Name}', the repetition '{parts[i]}' can begin with the same input as " +
					"what follows it, so where it ends is decided by backtracking rather than by the " +
					"grammar. It parses, and the reading you get is the one the engine happened to " +
					"find. It is also what stops the rule being read from a stream, where an element " +
					"handed over cannot be taken back (docs/syntax.md §6.3).",
					declaration.At.Position,
					declaration.At.Length,
					GramSeverity.Info));
			}

		foreach (var child in Children(node))
			Walk(child, rule, declaration, reported, graph);
	}

	/// <summary>What the rest of a sequence can begin with, skipping what may match nothing.</summary>
	static First Following(IReadOnlyList<Node> parts, int from, RecognitionGraph graph)
	{
		var ranges  = new List<CharRange>();
		var nothing = true;

		for (var i = from; i < parts.Count; i++)
		{
			var first = Of(parts[i], graph);

			if (first.Anything)
				return First.All;

			if (first.Nothing)
				continue;

			nothing = false;
			ranges.AddRange(first.Ranges);

			// Something that must consume settles it; anything optional and the one after
			// it could be what actually follows.
			if (!Nullable(parts[i], graph))
				break;
		}

		return nothing ? First.None : new First(false, false, ranges);
	}

	/// <summary>What a node can begin with.</summary>
	public static First Of(Node node, RecognitionGraph graph) => Of(node, graph, []);

	static First Of(Node node, RecognitionGraph graph, HashSet<RuleSymbol> seen)
	{
		switch (node)
		{
			case Node.Literal(var text):
				return text.Length == 0
					? First.None
					: new First(false, false, [new CharRange(text[0], text[0])]);

			case Node.Element(var negated, var ranges, var categories, var references):
				return negated || categories.Count > 0 || references.Count > 0
					? First.All
					: new First(false, false, ranges);

			// Consumes nothing, so it begins with nothing — and a lookahead's own first set
			// is not what the sequence begins with, because the operand after it is.
			case Node.Empty:
			case Node.Guard:
			case Node.Lookahead:
				return First.None;

			case Node.Capture(_, var captured):  return Of(captured, graph, seen);
			case Node.Construct(var built, _):   return Of(built,    graph, seen);
			case Node.Repeat(var body, _, _):    return Of(body,     graph, seen);

			case Node.Call(var called, _):
				return !seen.Add(called) || !graph.Bodies.TryGetValue(called, out var body2)
					? First.All
					: Of(body2, graph, seen);

			case Node.Choice(var alternatives):
			{
				var ranges  = new List<CharRange>();
				var nothing = true;

				foreach (var alternative in alternatives)
				{
					var first = Of(alternative, graph, seen);

					if (first.Anything)
						return First.All;

					if (first.Nothing)
						continue;

					nothing = false;
					ranges.AddRange(first.Ranges);
				}

				return nothing ? First.None : new First(false, false, ranges);
			}

			case Node.Sequence(var parts):
				return Following(parts, 0, graph);

			default:
				return First.All;
		}
	}

	/// <summary>Whether a node can match without consuming anything.</summary>
	static bool Nullable(Node node, RecognitionGraph graph) => node switch
	{
		Node.Empty or Node.Guard or Node.Lookahead => true,
		Node.Literal(var text)                     => text.Length == 0,
		Node.Repeat(_, var min, _)                 => min == 0,
		Node.Capture(_, var captured)              => Nullable(captured, graph),
		Node.Construct(var built, _)               => Nullable(built,    graph),
		Node.Sequence(var parts)                   => All(parts, graph),
		Node.Choice(var alternatives)              => Any(alternatives, graph),
		Node.Call(var called, _)                   => graph.Nullable.TryGetValue(called, out var yes) && yes,
		_                                          => false,
	};

	static bool All(IReadOnlyList<Node> nodes, RecognitionGraph graph)
	{
		foreach (var node in nodes)
			if (!Nullable(node, graph))
				return false;

		return true;
	}

	static bool Any(IReadOnlyList<Node> nodes, RecognitionGraph graph)
	{
		foreach (var node in nodes)
			if (Nullable(node, graph))
				return true;

		return false;
	}

	static IEnumerable<Node> Children(Node node)
	{
		switch (node)
		{
			case Node.Sequence(var parts):       return parts;
			case Node.Choice(var alternatives):  return alternatives;
			case Node.Repeat(var body, _, _):    return [body];
			case Node.Capture(_, var captured):  return [captured];
			case Node.Construct(var built, _):   return [built];
			default:                             return [];
		}
	}
}
