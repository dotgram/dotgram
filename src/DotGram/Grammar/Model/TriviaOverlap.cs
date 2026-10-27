using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Trivia that gives back what it read, where a class beside one of its seams reads the same
/// characters.
/// </summary>
/// <remarks>
/// <para>
/// A seam is where §4.5 weaves <c>trivia</c> between two operands. Where a class, <c>any</c>
/// or a negated class at the edge of the operand on either side can read a character trivia
/// reads too, a run of such characters at the seam can be split between the two in as many
/// ways as it is long, and a trivia that is not atomic offers every split back to the
/// backtracking. An input that is accepted takes the first; one that is refused is refused
/// only after every combination of every seam has been tried, which is exponential in the
/// number of seams. <c>trivia = ' '*</c> beside <c>Field = [^ ',' | '\n']+</c> is the shape:
/// a few rows of a refused CSV file took seconds, six rows over a minute.
/// </para>
/// <para>
/// Braces around trivia commit what it read, so each seam has one reading, and silence this.
/// Only what is proven is told: the characters trivia can begin with, by
/// <see cref="FirstSets"/>, against the characters an element at the edge admits, by
/// <see cref="FirstSets.OfElement"/>. An element that asks the host (<c>[@IsSpace]</c>) is
/// not known and not told; a literal beside a seam is text the author wrote at that place
/// and not told either.
/// </para>
/// <para>
/// Asked of the graph over characters. A grammar cut in two reads trivia with a scanner,
/// which takes the longest run and never gives it back, so there is nothing to tell it.
/// </para>
/// </remarks>
public static class TriviaOverlap
{
	/// <summary>Non-atomic trivia reading characters a class beside one of its seams also reads.</summary>
	public const string Overlapping = "GRAM4051";

	public static IReadOnlyList<GramDiagnostic> Check(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var reported = new List<GramDiagnostic>();
		var judged   = new Dictionary<RuleSymbol, Judge?>();

		// Every rule some namespace weaves in as its trivia. A call to one is a seam wherever it
		// stands, and is passed over when looking for what stands beside a seam.
		var seams = new HashSet<RuleSymbol>();

		foreach (var woven in graph.Trivia.Values)
			if (woven is Node.Call(var called, _))
				seams.Add(called);

		foreach (var rule in graph.Rules)
		{
			if (!graph.Trivia.TryGetValue(rule, out var seamNode) ||
				seamNode is not Node.Call(var seam, _) ||
				!graph.Bodies.TryGetValue(rule, out var body))
				continue;

			if (!judged.TryGetValue(seam, out var judge))
				judged[seam] = judge = Judge.For(seam, seams, graph);

			if (judge is null || judge.Found is not null)
				continue;

			judge.Search(body, rule);

			if (graph.Folds.TryGetValue(rule, out var fold) && judge.Found is null)
				judge.Search(fold.Loop, rule);
		}

		// One trivia written once is one warning, however many namespaces or clones see it.
		var told = new HashSet<(int, int)>();

		foreach (var judge in judged.Values)
			if (judge?.Found is { } found && judge.Seam.Declaration is { } declaration &&
				told.Add((declaration.At.Position, declaration.At.Length)))
				reported.Add(new GramDiagnostic(
					Overlapping,
					$"'{judge.Seam.Name}' is not atomic and reads {Spelled(found.Shared)}, which " +
					$"{found.Item} in '{found.In.Name}' also reads, beside a " +
					$"seam in '{found.Seam.Name}': a refused input is read again for every way those " +
					"characters can be split between them, exponentially many in the number of seams. " +
					$"Write '{judge.Seam.Name} = {{ … }}' so that what it reads stays read (docs/syntax.md §4.5).",
					declaration.At.Position,
					declaration.At.Length,
					GramSeverity.Warning));

		return reported;
	}

	/// <summary>The characters two sets share, as a class would spell them, the first few of them.</summary>
	static string Spelled(FirstSets.First shared)
	{
		const int most = 4;

		var ranges = shared.Ranges;
		var said   = string.Join(" | ", ranges.Take(most).Select(static range => range.ToString()));

		return ranges.Count > most ? $"[{said} | …]" : ranges.Count == 1 && ranges[0].IsSingle ? said : $"[{said}]";
	}

	/// <summary>What was found: the element, the rule it is written in, and the rule whose seam it stands beside.</summary>
	sealed record Overlap(string Item, RuleSymbol In, RuleSymbol Seam, FirstSets.First Shared);

	/// <summary>One trivia's search, which stops at the first overlap it proves.</summary>
	sealed class Judge(RuleSymbol seam, HashSet<RuleSymbol> seams, FirstSets.First reads, RecognitionGraph graph)
	{
		public RuleSymbol Seam { get; } = seam;

		public Overlap? Found { get; private set; }

		/// <summary>An overlapping element each rule can begin with, and end with; null where none is known.</summary>
		Dictionary<RuleSymbol, Edged?>? _heads;
		Dictionary<RuleSymbol, Edged?>? _tails;

		/// <summary>The judge of a trivia that may give back characters it can read; null for any other.</summary>
		public static Judge? For(RuleSymbol seam, HashSet<RuleSymbol> seams, RecognitionGraph graph)
		{
			if (!graph.Bodies.TryGetValue(seam, out var body) || Atomic(body, graph, []))
				return null;

			var reads = FirstSets.Of(body, graph);

			return reads.Anything || reads.Ranges.Count == 0 ? null : new Judge(seam, seams, reads, graph);
		}

		/// <summary>Whether a trivia commits what it read: braced, or a rule that forwards to a braced one.</summary>
		static bool Atomic(Node body, RecognitionGraph graph, HashSet<RuleSymbol> seen)
		{
			return body switch
			{
				Node.Atomic => true,
				Node.Call(var called, { Count: 0 }) =>
					seen.Add(called) && graph.Bodies.TryGetValue(called, out var forwarded) && Atomic(forwarded, graph, seen),
				_ => false,
			};
		}

		/// <summary>Every seam in a rule's body, with what stands on either side of it.</summary>
		public void Search(Node body, RuleSymbol rule)
		{
			Settle();

			var pending = new Stack<Node>();

			pending.Push(body);

			while (pending.Count > 0 && Found is null)
			{
				var node = pending.Pop();

				if (node is Node.Sequence(var parts))
					Seams(parts, rule, wraps: false);

				// The turns of a repetition are seams of their own: the end of one turn stands
				// beside the beginning of the next.
				else if (node is Node.Repeat(Node.Sequence(var turn), _, var max) && max is null or > 1)
				{
					Seams(turn, rule, wraps: true);

					foreach (var part in turn)
						pending.Push(part);

					continue;
				}

				foreach (var child in node.Children)
					pending.Push(child);
			}
		}

		void Seams(IReadOnlyList<Node> parts, RuleSymbol rule, bool wraps)
		{
			for (var i = 0; i < parts.Count && Found is null; i++)
			{
				if (!IsSeam(parts[i]))
					continue;

				var before = Ends(parts, i - 1, -1, rule);
				var after  = Ends(parts, i + 1, +1, rule);

				// At the start or the end of a turn, what stands on the far side is the other end
				// of the previous or the next turn.
				if (wraps && before is null && Nullable(parts, 0, i))
					before = Ends(parts, parts.Count - 1, -1, rule);

				if (wraps && after is null && Nullable(parts, i + 1, parts.Count))
					after = Ends(parts, 0, +1, rule);

				if ((before ?? after) is { } found)
					Found = new Overlap(found.Item, found.In, rule, found.Shared);
			}
		}

		bool IsSeam(Node node)
		{
			return node is Node.Call(var called, _) && ReferenceEquals(called, Seam);
		}

		bool Nullable(IReadOnlyList<Node> parts, int from, int to)
		{
			for (var i = from; i < to; i++)
				if (!FirstSets.Nullable(parts[i], graph))
					return false;

			return true;
		}

		/// <summary>
		/// The first overlapping element at the edge of a run of parts, walked from
		/// <paramref name="from"/> in direction <paramref name="step"/> past what may read nothing.
		/// </summary>
		Edged? Ends(IReadOnlyList<Node> parts, int from, int step, RuleSymbol owner)
		{
			for (var i = from; i >= 0 && i < parts.Count; i += step)
			{
				if (parts[i] is Node.Call(var woven, _) && seams.Contains(woven))
					continue;

				if (Edge(parts[i], step > 0, owner) is { } found)
					return found;

				if (!FirstSets.Nullable(parts[i], graph))
					break;
			}

			return null;
		}

		/// <summary>An element at one edge of a node that reads a character trivia reads.</summary>
		/// <remarks>A call is answered from what is known of the called rule so far (<see cref="Settle"/>).</remarks>
		Edged? Edge(Node node, bool head, RuleSymbol owner)
		{
			switch (node)
			{
				case Node.Element { References.Count: 0 } element:
				{
					var shared = reads.And(FirstSets.OfElement(element));

					return shared.Ranges.Count > 0 ? new Edged(element.ToString(), owner, shared) : null;
				}

				case Node.Call(var called, _) when seams.Contains(called):
					return null;

				case Node.Call(var called, _) when called.IsBuiltIn:
					return called.Name == "any" ? new Edged("any", owner, reads) : null;

				case Node.Call(var called, _):
					return (head ? _heads : _tails)!.TryGetValue(called, out var known) ? known : null;

				case Node.Sequence(var parts):
					return head ? Ends(parts, 0, +1, owner) : Ends(parts, parts.Count - 1, -1, owner);

				case Node.Choice(var alternatives):
					foreach (var alternative in alternatives)
						if (Edge(alternative, head, owner) is { } found)
							return found;

					return null;

				case Node.Repeat(var body, _, _):  return Edge(body, head, owner);
				case Node.Atomic(var body):        return Edge(body, head, owner);
				case Node.Marked(var body, _):     return Edge(body, head, owner);
				case Node.Capture(_, var body):    return Edge(body, head, owner);
				case Node.Construct(var body, _):  return Edge(body, head, owner);
				case Node.Bind(_, var body, _):    return Edge(body, head, owner);
				case Node.Counted(var body, _, _): return Edge(body, head, owner);

				default:
					return null;
			}
		}

		/// <summary>
		/// What every rule begins and ends with that overlaps, as the least answer that satisfies
		/// every rule at once.
		/// </summary>
		/// <remarks>
		/// Rules call each other and themselves, so this is a fixed point rather than a walk, the
		/// way <see cref="FirstSets"/> settles what a rule begins with: a rule's answer only ever
		/// goes from none to an element, and a change is passed on to the rules that call it.
		/// </remarks>
		void Settle()
		{
			if (_heads is not null)
				return;

			_heads = [];
			_tails = [];

			var pending = new Queue<RuleSymbol>(graph.Rules);
			var queued  = new HashSet<RuleSymbol>(graph.Rules);

			while (pending.Count > 0)
			{
				var rule = pending.Dequeue();

				queued.Remove(rule);

				if (!graph.Bodies.TryGetValue(rule, out var body))
					continue;

				var changed = Learn(_heads, rule, body, head: true) | Learn(_tails, rule, body, head: false);

				if (changed)
					foreach (var caller in graph.Calls.CalledBy(rule))
						if (queued.Add(caller))
							pending.Enqueue(caller);
			}
		}

		bool Learn(Dictionary<RuleSymbol, Edged?> known, RuleSymbol rule, Node body, bool head)
		{
			if (known.TryGetValue(rule, out var already) && already is not null)
				return false;

			var found = Edge(body, head, rule);

			known[rule] = found;

			return found is not null;
		}
	}

	/// <summary>An element at an edge, the rule it is written in, and what it shares with trivia.</summary>
	sealed record Edged(string Item, RuleSymbol In, FirstSets.First Shared);
}
