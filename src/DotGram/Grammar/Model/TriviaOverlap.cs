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
/// Only what is proven is told, because a grammar is built with warnings as errors: the
/// characters trivia can begin with, by <see cref="FirstSets"/>, against the characters an
/// element at the edge admits, by <see cref="FirstSets.OfElement"/>. An element that asks the
/// host (<c>[@IsSpace]</c>) is not known and not told; a literal beside a seam is text the
/// author wrote at that place and not told either. The end of a braced operand is committed
/// and gives nothing back, so it is not an edge; its beginning still is, since the trivia in
/// front of it can give characters to it. A negative look in front of an element, the
/// <c>(?!X ~ any)</c> idiom, takes what it refuses out of the element's characters.
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
		return Check(graph, out _);
	}

	/// <param name="graph">The grammar over characters.</param>
	/// <param name="steps">How many nodes were asked for their edges: the work done, for a test to hold to the grammar's size.</param>
	public static IReadOnlyList<GramDiagnostic> Check(RecognitionGraph graph, out int steps)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		steps = 0;

		var reported = new List<GramDiagnostic>();
		var judged   = new Dictionary<RuleSymbol, Judge?>();

		// Every rule some namespace weaves in as its trivia. A call to one is a seam wherever it
		// stands, and is passed over when looking for what stands beside a seam.
		var seams = new HashSet<RuleSymbol>();

		foreach (var woven in graph.Trivia.Values)
			if (woven is Node.Call(var called, _))
				seams.Add(called);

		// What every rule begins and ends with does not depend on which trivia is asked about,
		// so it is worked out once for all of them, and only where some trivia needs it.
		Edges? edges = null;

		foreach (var rule in graph.Rules)
		{
			if (!graph.Trivia.TryGetValue(rule, out var seamNode) ||
				seamNode is not Node.Call(var seam, _) ||
				!graph.Bodies.TryGetValue(rule, out var body))
				continue;

			if (!judged.TryGetValue(seam, out var judge))
				judged[seam] = judge = Judge.For(seam, graph);

			if (judge is null || judge.Found is not null)
				continue;

			edges ??= new Edges(seams, graph);

			judge.Search(body, rule, edges);

			if (graph.Folds.TryGetValue(rule, out var fold) && judge.Found is null)
				judge.Search(fold.Loop, rule, edges);
		}

		steps = edges?.Steps ?? 0;

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

	/// <summary>The first element found at an edge that reads a character of <see cref="Reads"/>.</summary>
	sealed class Witness(FirstSets.First reads)
	{
		public FirstSets.First Reads { get; } = reads;

		public (string Item, RuleSymbol In, FirstSets.First Shared)? Found;

		/// <summary>The rules already entered, so a rule reached again from inside itself is not.</summary>
		public HashSet<RuleSymbol> Entered { get; } = [];
	}

	/// <summary>One trivia's search, which stops at the first overlap it proves.</summary>
	sealed class Judge(RuleSymbol seam, FirstSets.First reads, RecognitionGraph graph)
	{
		public RuleSymbol Seam { get; } = seam;

		public Overlap? Found { get; private set; }

		/// <summary>The judge of a trivia that may give back characters it can read; null for any other.</summary>
		public static Judge? For(RuleSymbol seam, RecognitionGraph graph)
		{
			if (!graph.Bodies.TryGetValue(seam, out var body) || Atomic(body, graph, []))
				return null;

			var reads = FirstSets.Of(body, graph);

			return reads.Anything || reads.Ranges.Count == 0 ? null : new Judge(seam, reads, graph);
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
		public void Search(Node body, RuleSymbol rule, Edges edges)
		{
			var pending = new Stack<Node>();

			pending.Push(body);

			while (pending.Count > 0 && Found is null)
			{
				var node = pending.Pop();

				if (node is Node.Sequence(var parts))
					Seams(parts, rule, wraps: false, edges);

				// The turns of a repetition are seams of their own: the end of one turn stands
				// beside the beginning of the next.
				else if (node is Node.Repeat(Node.Sequence(var turn), _, var max) && max is null or > 1)
				{
					Seams(turn, rule, wraps: true, edges);

					foreach (var part in turn)
						pending.Push(part);

					continue;
				}

				foreach (var child in node.Children)
					pending.Push(child);
			}
		}

		void Seams(IReadOnlyList<Node> parts, RuleSymbol rule, bool wraps, Edges edges)
		{
			for (var i = 0; i < parts.Count && Found is null; i++)
			{
				if (!(parts[i] is Node.Call(var called, _) && ReferenceEquals(called, Seam)))
					continue;

				// At the start or the end of a turn, what stands on the far side is the other end
				// of the previous or the next turn.
				var before = i > 0 || !wraps ? (i - 1, -1) : (parts.Count - 1, -1);
				var after  = i < parts.Count - 1 || !wraps ? (i + 1, +1) : (0, +1);

				if (wraps && i > 0 && Nullable(parts, 0, i))
					before = (parts.Count - 1, -1);

				if (wraps && i < parts.Count - 1 && Nullable(parts, i + 1, parts.Count))
					after = (0, +1);

				foreach (var (from, step) in new[] { before, after })
				{
					if (!Overlaps(edges.Ends(parts, from, step, rule, null)))
						continue;

					var witness = new Witness(reads);

					edges.Ends(parts, from, step, rule, witness);

					if (witness.Found is var (item, owner, shared))
					{
						Found = new Overlap(item, owner, rule, shared);
						break;
					}
				}
			}
		}

		bool Overlaps(FirstSets.First edge)
		{
			return reads.And(edge).Ranges.Count > 0;
		}

		bool Nullable(IReadOnlyList<Node> parts, int from, int to)
		{
			for (var i = from; i < to; i++)
				if (!FirstSets.Nullable(parts[i], graph))
					return false;

			return true;
		}
	}

	/// <summary>
	/// The characters every rule can begin and end with, read by an element at that edge: the same
	/// for every trivia, so worked out once for the graph.
	/// </summary>
	/// <remarks>
	/// Rules call each other and themselves, so this is a fixed point rather than a walk, the way
	/// <see cref="FirstSets"/> settles what a rule begins with: a rule's sets only grow, and a change
	/// is passed on to the rules that call it. A walk that cut recursion instead would answer
	/// differently depending on which rule it met first.
	/// </remarks>
	sealed class Edges
	{
		static readonly FirstSets.First Every = FirstSets.First.Chars([new CharRange(char.MinValue, char.MaxValue)]);

		readonly HashSet<RuleSymbol> _seams;
		readonly RecognitionGraph    _graph;

		readonly Dictionary<RuleSymbol, FirstSets.First> _heads = [];
		readonly Dictionary<RuleSymbol, FirstSets.First> _tails = [];

		public int Steps { get; private set; }

		public Edges(HashSet<RuleSymbol> seams, RecognitionGraph graph)
		{
			_seams = seams;
			_graph = graph;

			var pending = new Queue<RuleSymbol>(graph.Rules);
			var queued  = new HashSet<RuleSymbol>(graph.Rules);

			while (pending.Count > 0)
			{
				var rule = pending.Dequeue();

				queued.Remove(rule);

				if (!graph.Bodies.TryGetValue(rule, out var body) || seams.Contains(rule))
					continue;

				var changed = Learn(_heads, rule, body, head: true) | Learn(_tails, rule, body, head: false);

				if (changed)
					foreach (var caller in graph.Calls.CalledBy(rule))
						if (queued.Add(caller))
							pending.Enqueue(caller);
			}
		}

		bool Learn(Dictionary<RuleSymbol, FirstSets.First> known, RuleSymbol rule, Node body, bool head)
		{
			var found = Edge(body, head, rule, null, null);

			if (known.TryGetValue(rule, out var already) && FirstSets.Same(already, found))
				return false;

			known[rule] = found;

			return true;
		}

		/// <summary>
		/// What a run of parts begins or ends with, walked from <paramref name="from"/> in direction
		/// <paramref name="step"/> past what may read nothing.
		/// </summary>
		public FirstSets.First Ends(IReadOnlyList<Node> parts, int from, int step, RuleSymbol owner, Witness? witness)
		{
			var found = FirstSets.First.None;

			for (var i = from; i >= 0 && i < parts.Count; i += step)
			{
				found = found.Or(Edge(parts[i], step > 0, owner, Refused(parts, i), witness));

				if (!FirstSets.Nullable(parts[i], _graph))
					break;
			}

			return found;
		}

		/// <summary>
		/// The characters the negative looks right in front of a part refuse, where each is a class
		/// followed by nothing or by <c>any</c>: <c>?![' ' | ','] ~ any</c> reads no space or comma.
		/// </summary>
		FirstSets.First? Refused(IReadOnlyList<Node> parts, int at)
		{
			FirstSets.First? refused = null;

			for (var i = at - 1; i >= 0; i--)
			{
				switch (parts[i])
				{
					case Node.Lookahead(false, var body) when OneClass(body) is { } one:
						refused = refused is null ? one : refused.Or(one);
						continue;

					case Node.Lookahead or Node.Glue or Node.Guard or Node.Condition or Node.Empty or Node.Reading:
						continue;
				}

				break;
			}

			return refused;
		}

		/// <summary>The characters a look refuses, where it is a class and at most <c>any</c> after it.</summary>
		static FirstSets.First? OneClass(Node body)
		{
			switch (body)
			{
				case Node.Element { References.Count: 0 } element:
					return FirstSets.OfElement(element);

				case Node.Sequence(var parts):
				{
					var reading = parts.Where(static part => part is not Node.Glue).ToArray();

					return reading.Length == 2 && reading[1] is Node.Call({ IsBuiltIn: true, Name: "any" }, _)
						? OneClass(reading[0])
						: null;
				}

				case Node.Choice(var alternatives):
				{
					var all = FirstSets.First.None;

					foreach (var alternative in alternatives)
					{
						if (OneClass(alternative) is not { } one)
							return null;

						all = all.Or(one);
					}

					return all;
				}

				default:
					return null;
			}
		}

		/// <summary>The characters an element at one edge of a node reads, those a look in front refuses taken out.</summary>
		FirstSets.First Edge(Node node, bool head, RuleSymbol owner, FirstSets.First? refused, Witness? witness)
		{
			Steps++;

			switch (node)
			{
				case Node.Element { References.Count: 0 } element:
					return Read(element.ToString(), FirstSets.OfElement(element), owner, refused, witness);

				case Node.Call(var called, _) when _seams.Contains(called):
					return FirstSets.First.None;

				case Node.Call(var called, _) when called.IsBuiltIn:
					return called.Name == "any" ? Read("any", Every, owner, refused, witness) : FirstSets.First.None;

				case Node.Call(var called, _):
				{
					var known = (head ? _heads : _tails).TryGetValue(called, out var settled) ? settled : FirstSets.First.None;

					// Looking for the element: into the rule, if what it is known to hold is wanted.
					if (witness is { Found: null } &&
						witness.Reads.And(known).Ranges.Count > 0 &&
						witness.Entered.Add(called) &&
						_graph.Bodies.TryGetValue(called, out var body))
						Edge(body, head, called, null, witness);

					return known;
				}

				case Node.Sequence(var parts):
					return head
						? Ends(parts, 0, +1, owner, witness)
						: Ends(parts, parts.Count - 1, -1, owner, witness);

				case Node.Choice(var alternatives):
				{
					var found = FirstSets.First.None;

					foreach (var alternative in alternatives)
						found = found.Or(Edge(alternative, head, owner, null, witness));

					return found;
				}

				// A repetition can stop a turn early, and then the turn it did not take stands at
				// its end: what a turn begins with is at the end of the repetition too.
				case Node.Repeat(var body, _, _):
					return head
						? Edge(body, head, owner, refused, witness)
						: Edge(body, false, owner, refused, witness).Or(Edge(body, true, owner, refused, witness));

				// A braced operand keeps what it read: its end gives nothing back. What it begins
				// with is still at the mercy of the trivia in front of it.
				case Node.Atomic(var body):
					return head ? Edge(body, head, owner, null, witness) : FirstSets.First.None;

				case Node.Marked(var body, _):     return Edge(body, head, owner, refused, witness);
				case Node.Capture(_, var body):    return Edge(body, head, owner, refused, witness);
				case Node.Construct(var body, _):  return Edge(body, head, owner, refused, witness);
				case Node.Bind(_, var body, _):    return Edge(body, head, owner, refused, witness);
				case Node.Counted(var body, _, _): return Edge(body, head, owner, refused, witness);

				default:
					return FirstSets.First.None;
			}
		}

		/// <summary>An element's characters, less what a look refuses; recorded where they are what a witness looks for.</summary>
		static FirstSets.First Read(string item, FirstSets.First reads, RuleSymbol owner, FirstSets.First? refused, Witness? witness)
		{
			if (refused is { Ranges.Count: > 0 })
				reads = reads.And(FirstSets.First.Chars(FirstSets.Complement(refused.Ranges)));

			if (witness is { Found: null } && witness.Reads.And(reads) is { Ranges.Count: > 0 } shared)
				witness.Found = (item, owner, shared);

			return reads;
		}
	}
}
