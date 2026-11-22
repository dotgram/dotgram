using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// The rules a reading may enter again where it already stands in them: a rule that reaches
/// itself on the left once a recognizer of the host may succeed without moving (docs/syntax.md
/// §7.2).
/// </summary>
/// <remarks>
/// <para>
/// The check for left recursion (§4.3) takes a recognizer at its word, that it consumes: that is
/// what its <c>ref</c> says, and a rule that reaches itself only past one is not left-recursive to
/// it. Nothing in §7.2 asks a recognizer to move when it succeeds, though, and one that does not
/// will not move on the next entry either, since its answer depends only on its arguments and the
/// text — so such a rule, entered again at the position it is already being read at, would go
/// round for ever. These are the rules <see cref="FirstSets.StandsStill"/> finds for a repetition,
/// found for a recursion.
/// </para>
/// <para>
/// A rule marked here is still compiled, and the grammar still built; what it gets is a check at
/// its entry, which refuses the parse where the rule is entered again at a position it already
/// stands at, with the same arguments (the reader and the engine print it from this fact, and the
/// diagnostic says it from the same). Every other rule is written as it always was.
/// </para>
/// <para>
/// Worked out once a graph, on the graph as it is emitted: after every pass that makes or rewrites
/// a rule, so that a cycle that appears only in a rule a lift made is found too. Not at all for a
/// grammar that calls no recognizer.
/// </para>
/// </remarks>
static class Reentry
{
	/// <summary>A rule can come back to itself without reading anything, through a recognizer of the host.</summary>
	public const string Reenters = "GRAM5021";

	/// <summary>
	/// How a marked rule comes back to itself: the other rules on the way, and the recognizers whose
	/// standing still lets it — each under the name the author wrote.
	/// </summary>
	public sealed record Cycle(string Rule, IReadOnlyList<string> Through, IReadOnlyList<string> Recognizers)
	{
		/// <summary>What the refusal of a parse that came back says; where, the refusal's position says.</summary>
		public string Said()
		{
			string[] through = [.. Through.Select(static name => $"'{name}'"), .. Recognizers.Select(static name => "@" + name)];

			return through.Length == 0
				? $"Rule '{Rule}' came back to itself without reading anything."
				: $"Rule '{Rule}' came back to itself through {Joined(through)} without reading anything.";
		}
	}

	sealed class Facts
	{
		public readonly Dictionary<RuleSymbol, Cycle> Marked = [];

		public readonly List<(RuleSymbol Closing, Cycle Cycle)> Told = [];
	}

	static readonly Facts None = new();

	static readonly ConditionalWeakTable<RecognitionGraph, Facts> _facts = new();

	/// <summary>Whether a rule may be entered again where it already stands.</summary>
	public static bool Marked(RecognitionGraph graph, RuleSymbol rule)
	{
		return Of(graph).Marked.ContainsKey(rule);
	}

	/// <summary>Whether any rule of the graph is marked: where none is, nothing of the check is written.</summary>
	public static bool Any(RecognitionGraph graph)
	{
		return Of(graph).Marked.Count > 0;
	}

	/// <summary>The marked rules of a graph, in the order of its rules.</summary>
	public static IReadOnlyList<RuleSymbol> MarkedRules(RecognitionGraph graph)
	{
		var facts = Of(graph);

		return [.. graph.Rules.Where(facts.Marked.ContainsKey)];
	}

	/// <summary>
	/// How many entries of a rule at one position can differ in what they are asked: one, or for a rule read
	/// at a strength (§4.3.1), one for every strength the grammar asks anything at, and nought, which a
	/// reading begins at.
	/// </summary>
	/// <remarks>
	/// More than the strengths this rule is entered at, where the grammar asks others of other rules; never
	/// fewer, which is what the check counts on (<c>Machine.Reentry.cs</c>).
	/// </remarks>
	public static int Keys(RecognitionGraph graph, RuleSymbol rule)
	{
		if (!graph.Climbing.ContainsKey(rule))
			return 1;

		var strengths = new HashSet<int>(graph.Powers.Values) { 0 };

		return strengths.Count;
	}

	/// <summary>
	/// Whether a pass that copies a rule for one of its sites may copy this one: not where the copy would read a
	/// marked rule, itself or through what it calls.
	/// </summary>
	/// <remarks>
	/// For a pass that settles a run for the site it is read at, by a copy of the rule that reads it (none is
	/// written yet): a copy made after this fact was worked out is not marked, so the recursion through it
	/// would go unchecked, or be checked as another rule's and refused one entry later than the rule it was
	/// copied from. Such a rule is left as it is, and read as it is; the run it settles is only faster.
	/// </remarks>
	public static bool MayCopy(RecognitionGraph graph, RuleSymbol rule)
	{
		var facts = Of(graph);

		if (facts.Marked.Count == 0)
			return true;

		var seen    = new HashSet<RuleSymbol>();
		var pending = new Stack<RuleSymbol>();

		pending.Push(rule);

		while (pending.Count > 0)
		{
			var one = pending.Pop();

			if (!seen.Add(one))
				continue;

			if (facts.Marked.ContainsKey(one))
				return false;

			if (!graph.Bodies.TryGetValue(one, out var body))
				continue;

			foreach (var node in NodeWalk.Descendants(body))
				if (node is Node.Call(var called, _))
					pending.Push(called);
		}

		return true;
	}

	/// <summary>How a marked rule comes back to itself, for the refusal its check says.</summary>
	public static Cycle CycleOf(RecognitionGraph graph, RuleSymbol rule)
	{
		return Of(graph).Marked[rule];
	}

	/// <summary>
	/// What the build says of the marked rules: once a cycle, on the rule that closes it, as
	/// Information — the grammar is sound, and a recognizer that always moves never meets the check.
	/// </summary>
	public static IReadOnlyList<GramDiagnostic> Check(RecognitionGraph graph)
	{
		var told = new List<GramDiagnostic>();

		foreach (var (closing, cycle) in Of(graph).Told)
		{
			var at       = (closing.Declaration ?? graph.Origin(closing).Declaration)?.At;
			var through  = cycle.Through.Count == 0 ? "" : $" through {Joined([.. cycle.Through.Select(static name => $"'{name}'")])}";
			var standing = cycle.Recognizers.Count == 0 ? "a recognizer of the host" : Joined([.. cycle.Recognizers.Select(static name => "@" + name)]);

			told.Add(new GramDiagnostic(
				Reenters,
				$"'{cycle.Rule}' can come back to itself{through} without reading anything where {standing} " +
				$"succeeds without moving, which a recognizer may do (docs/syntax.md §7.2). A parse where it does " +
				"not move there is refused with a message saying so, rather than going round for ever; so is one " +
				"that relies on 'context' changing between two entries at one position. Written as a repetition — " +
				"'Frames = @ReadFrame* & eof' for 'Frames = @ReadFrame & Frames | eof' — the rule needs no such " +
				"check.",
				at?.Position ?? 0,
				at?.Length ?? 0,
				GramSeverity.Info));
		}

		return told;
	}

	static Facts Of(RecognitionGraph graph)
	{
		return _facts.GetValue(graph, static graph => Unchecked ? None : Work(graph));
	}

	/// <summary>
	/// For a test that weighs the check against its absence: marks nothing in the graphs worked out on this
	/// thread while it is set. Not a setting: a parser compiled with it may go round for ever.
	/// </summary>
	internal static bool Unchecked
	{
		get { return _unchecked; }
		set { _unchecked = value; }
	}

	[ThreadStatic]
	static bool _unchecked;

	/// <summary>
	/// For a test that weighs the two answers at the bound: on this thread, a rule entered past its bound refuses the
	/// whole parse with the message of <see cref="Cycle.Said"/> rather than failing as an alternative does.
	/// </summary>
	internal static bool Aborts
	{
		get { return _aborts; }
		set { _aborts = value; }
	}

	[ThreadStatic]
	static bool _aborts;

	static Facts Work(RecognitionGraph graph)
	{
		if (!FirstSets.HostsMayStand(graph))
			return None;

		// What each rule calls on the left, once a recognizer may stand still, and through which
		// recognizers each such call was reached; and what it calls on the left as the check for
		// left recursion counts it, where a cycle is ordinary left recursion, refused or rewritten.
		var standing = new Dictionary<RuleSymbol, Dictionary<RuleSymbol, SortedSet<string>>>();

		foreach (var pair in graph.Bodies)
			standing[pair.Key] = LeftCalls(pair.Value, graph, hosts: true);

		var loose = new CallGraph(graph.Rules, rule => standing.TryGetValue(rule, out var calls) ? calls.Keys : []);

		if (!graph.Rules.Any(loose.Recurses))
			return None;

		var plain = new CallGraph(
			graph.Rules,
			rule => graph.Bodies.TryGetValue(rule, out var body) ? LeftCalls(body, graph, hosts: false).Keys : []);

		var facts = new Facts();
		var order = new Dictionary<RuleSymbol, int>();

		foreach (var rule in graph.Rules)
			order[rule] = order.Count;

		foreach (var component in loose.Components)
		{
			var members = new HashSet<RuleSymbol>(component.Where(rule => loose.Recurses(rule) && !plain.Recurses(rule)));

			if (members.Count == 0)
				continue;

			// The way back is looked for through the whole component, which reaches itself through every rule of it.
			var around = new HashSet<RuleSymbol>(component);

			foreach (var rule in members)
				facts.Marked[rule] = Around(rule, around, standing, graph).Cycle;

			// Told once, from the rule written first, on the rule whose call closes the cycle.
			var first = members.OrderBy(rule => order[rule]).First();

			var (cycle, closing) = Around(first, around, standing, graph);

			facts.Told.Add((closing, cycle));
		}

		facts.Told.Sort((one, other) => order[one.Closing].CompareTo(order[other.Closing]));

		return facts;
	}

	/// <summary>The shortest way from a rule back to itself among the members of its cycle, and the rule that closes it.</summary>
	static (Cycle Cycle, RuleSymbol Closing) Around(
		RuleSymbol start, HashSet<RuleSymbol> members,
		Dictionary<RuleSymbol, Dictionary<RuleSymbol, SortedSet<string>>> calls, RecognitionGraph graph)
	{
		var from    = new Dictionary<RuleSymbol, RuleSymbol>();
		var pending = new Queue<RuleSymbol>();

		pending.Enqueue(start);

		RuleSymbol? closing = null;

		while (pending.Count > 0 && closing is null)
		{
			var rule = pending.Dequeue();

			foreach (var called in calls[rule].Keys.OrderBy(static one => one.Name, StringComparer.Ordinal))
			{
				if (ReferenceEquals(called, start))
				{
					closing = rule;

					break;
				}

				if (members.Contains(called) && !from.ContainsKey(called))
				{
					from[called] = rule;
					pending.Enqueue(called);
				}
			}
		}

		// A member of a cycle is on a way back to itself by construction.
		var path = new List<RuleSymbol> { closing! };

		while (!ReferenceEquals(path[^1], start))
			path.Add(from[path[^1]]);

		path.Reverse();

		var recognizers = new SortedSet<string>(StringComparer.Ordinal);

		for (var i = 0; i < path.Count; i++)
			recognizers.UnionWith(calls[path[i]][i + 1 < path.Count ? path[i + 1] : start]);

		var name    = NameOf(start, graph);
		var through = path.Skip(1).Select(rule => NameOf(rule, graph)).Where(other => other != name).Distinct().ToList();

		return (new Cycle(name, through, [.. recognizers]), closing!);
	}

	/// <summary>A rule as the author wrote it: a rule a lift made is the one it was lifted from, a clone the one it was made from.</summary>
	static string NameOf(RuleSymbol rule, RecognitionGraph graph)
	{
		var origin = graph.Origin(rule);

		return origin.Declaration?.Name ?? origin.Name;
	}

	/// <summary>
	/// What a body calls before it has to consume, each with the recognizers that may have stood
	/// still before the call; with <paramref name="hosts"/> false, as the check for left recursion
	/// counts it.
	/// </summary>
	/// <remarks>
	/// The walk of that check (<c>GrammarNormalizer.LeftCalls</c>), with a part of a sequence let match
	/// nothing where a recognizer may — at any operand, not only the first — and the synchronization of
	/// a recovering repetition, which is read where the turn it recovers began.
	/// </remarks>
	static Dictionary<RuleSymbol, SortedSet<string>> LeftCalls(Node body, RecognitionGraph graph, bool hosts)
	{
		var called  = new Dictionary<RuleSymbol, SortedSet<string>>();
		var pending = new Stack<(Node Node, SortedSet<string> Past)>();
		var none    = new SortedSet<string>(StringComparer.Ordinal);

		pending.Push((body, none));

		while (pending.Count > 0)
		{
			var (node, past) = pending.Pop();

			switch (node)
			{
				case Node.Call(var callee, _):
					if (!called.TryGetValue(callee, out var through))
						called[callee] = through = new SortedSet<string>(StringComparer.Ordinal);

					through.UnionWith(past);

					break;

				// What leads the sequence: each part up to the first that has to consume, and past
				// each part that matches nothing only where a recognizer does, the recognizers in it.
				case Node.Sequence(var nodes):
				{
					var before = past;

					foreach (var child in nodes)
					{
						pending.Push((child, before));

						if (hosts ? !FirstSets.MayStandStill(child, graph) : !FirstSets.Nullable(child, graph))
							break;

						if (hosts && !FirstSets.Nullable(child, graph))
							before = new SortedSet<string>(before.Concat(Recognizers(child, graph)), StringComparer.Ordinal);
					}

					break;
				}

				case Node.Choice(var nodes):
					foreach (var child in nodes)
						pending.Push((child, past));

					break;

				case Node.Repeat(var repeated, _, _):
					pending.Push((repeated, past));

					if (hosts && graph.Recoveries.TryGetValue(node, out var recovery))
						pending.Push((recovery.Sync, past));

					break;

				case Node.Capture  (_, var captured):    pending.Push((captured, past)); break;
				case Node.Bind     (_, var bound, _):    pending.Push((bound, past));    break;
				case Node.Counted  (var counted, _, _):  pending.Push((counted, past));  break;
				case Node.Construct(var built, _):       pending.Push((built, past));    break;
				case Node.Atomic   (var atomic):         pending.Push((atomic, past));   break;
				case Node.Marked   (var marked, _):      pending.Push((marked, past));   break;
				case Node.Lookahead(_, var ahead):       pending.Push((ahead, past));    break;
			}
		}

		return called;
	}

	/// <summary>The recognizers a part reads, itself or through the rules it calls that may stand still.</summary>
	static IEnumerable<string> Recognizers(Node part, RecognitionGraph graph)
	{
		var found   = new SortedSet<string>(StringComparer.Ordinal);
		var seen    = new HashSet<RuleSymbol>();
		var pending = new Stack<Node>();

		pending.Push(part);

		while (pending.Count > 0)
		{
			var node = pending.Pop();

			if (node is Node.External(var name))
			{
				found.Add(name);

				continue;
			}

			if (node is Node.Call(var callee, _))
			{
				if (graph.Externals.TryGetValue(callee, out var external))
					found.Add(external);
				else if (seen.Add(callee) && graph.Bodies.TryGetValue(callee, out var body) && FirstSets.MayStandStill(body, graph))
					pending.Push(body);

				continue;
			}

			foreach (var child in node.Children)
				pending.Push(child);
		}

		return found;
	}

	/// <summary>"a", "a and b", "a, b and c".</summary>
	static string Joined(IReadOnlyList<string> names)
	{
		return names.Count switch
		{
			0 => "",
			1 => names[0],
			_ => string.Join(", ", names.Take(names.Count - 1)) + " and " + names[^1],
		};
	}
}
