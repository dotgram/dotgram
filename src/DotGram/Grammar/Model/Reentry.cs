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
/// Every such rule is still compiled, and the grammar still built, and told (GRAM5021). Where nothing
/// is read after the call back (<see cref="Passes"/>), the rule is marked: it gets a check at its
/// entry, which cuts an entry at a position the rule already stands at, at once (the reader and the
/// engine print it from this fact). Where something is, a parse may need the rule entered there more
/// than once, and no check on the rule and the position is exact: such a rule is not marked, and is
/// written as it always was, as is every other rule.
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
	/// How a rule comes back to itself: the other rules on the way, and the recognizers whose standing still lets it —
	/// each under the name the author wrote.
	/// </summary>
	public sealed record Cycle(string Rule, IReadOnlyList<string> Through, IReadOnlyList<string> Recognizers);

	sealed class Facts
	{
		/// <summary>Every rule that can come back to itself where it stands, cut at once or not.</summary>
		public readonly HashSet<RuleSymbol> Found = [];

		/// <summary>The rules cut at once: those of a cycle that passes on what follows it unchanged.</summary>
		public readonly HashSet<RuleSymbol> Marked = [];

		public readonly List<(RuleSymbol Closing, Cycle Cycle, bool Cut)> Told = [];
	}

	static readonly Facts None = new();

	static readonly ConditionalWeakTable<RecognitionGraph, Facts> _facts = new();

	/// <summary>
	/// Whether a rule may be entered again where it already stands, and comes back to itself only where nothing is left
	/// to read after the call back: an entry of it at the position of a live entry of it is then read exactly as that one
	/// was, and fails at once (see the remarks).
	/// </summary>
	/// <remarks>
	/// <para>
	/// Such a rule's cycle passes on what follows it unchanged: every call around it, in the rules as they are emitted,
	/// is the last thing its alternative reads — not inside a lookahead, a repetition that may turn again, a <c>{ }</c>, a
	/// binding of the text, or a rule read at a strength, whose operators are read after the call. So an entry at the
	/// position of a live entry of the same rule has the same rule, position and continuation as that one, and reads what
	/// it read, step for step, to the same entry again: without the check it never comes back, and failing it as an
	/// alternative changes no answer a parse without the check gives.
	/// </para>
	/// <para>
	/// Not so where what is read depends on more than the text and the position: a <c>when</c> or a <c>switch</c> that
	/// reads a value the cycle builds, which grows with the depth, or a hook on the cycle that reads or writes the
	/// <c>context</c>. Such a rule, like one with something left to read after the call back, is not marked: it is written
	/// as it always was, and read as it is written.
	/// </para>
	/// </remarks>
	public static bool Marked(RecognitionGraph graph, RuleSymbol rule)
	{
		return Of(graph).Marked.Contains(rule);
	}

	/// <summary>Whether a rule can come back to itself where it stands, through a recognizer that does not move: marked or not.</summary>
	public static bool Found(RecognitionGraph graph, RuleSymbol rule)
	{
		return Of(graph).Found.Contains(rule);
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

		return [.. graph.Rules.Where(facts.Marked.Contains)];
	}

	/// <summary>The rules of a graph that can come back to themselves where they stand, marked or not, in the order of its rules.</summary>
	public static IReadOnlyList<RuleSymbol> FoundRules(RecognitionGraph graph)
	{
		var facts = Of(graph);

		return [.. graph.Rules.Where(facts.Found.Contains)];
	}

	/// <summary>
	/// What the build says of the rules that can come back to themselves: once a cycle, on the rule that closes it, as
	/// Information — the grammar is sound, and a recognizer that always moves never meets the cycle.
	/// </summary>
	public static IReadOnlyList<GramDiagnostic> Check(RecognitionGraph graph)
	{
		var told = new List<GramDiagnostic>();

		var facts = Of(graph);

		foreach (var (closing, cycle, cut) in facts.Told)
		{
			var at       = (closing.Declaration ?? graph.Origin(closing).Declaration)?.At;
			var through  = cycle.Through.Count == 0 ? "" : $" through {Joined([.. cycle.Through.Select(static name => $"'{name}'")])}";
			var standing = cycle.Recognizers.Count == 0 ? "a recognizer of the host" : Joined([.. cycle.Recognizers.Select(static name => "@" + name)]);

			// Where nothing is read after the call back, an entry where one already stands is cut at once; elsewhere a
			// parse may need several there, one for each thing pending, and the rule is read as it is written.
			var said = cut
				? "An entry of it where one already stands, with nothing read after the call back, would only repeat that " +
					"one, and fails as an alternative does instead of going round for ever."
				: "Something is read after the call back, so no check of the entry tells one that is needed from one that only " +
					"goes round, and the rule is not read as its left recursion without the recognizer either: that needs " +
					"the rule without it to be one §4.3 folds, with a value as §4.3 gives one, and nothing on the way to read " +
					"more than the text and the position, which a 'when' or 'switch' reading a value the cycle builds, or a " +
					"hook that names 'context', does. The rule is read as it is written, and where the recognizer does not " +
					"move, a parse that has to give up on that alternative goes round until the stack or memory runs out.";

			told.Add(new GramDiagnostic(
				Reenters,
				$"'{cycle.Rule}' can come back to itself{through} without reading anything where {standing} " +
				$"succeeds without moving, which a recognizer may do (docs/syntax.md §7.2). {said} Written as a " +
				"repetition — 'Frames = @ReadFrame* & eof' for 'Frames = @ReadFrame & Frames | eof' — the rule needs " +
				"no such check.",
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

			facts.Found.UnionWith(members);

			var cut = Passes(around, graph);

			if (cut)
				facts.Marked.UnionWith(members);

			// Told once, from the rule written first, on the rule whose call closes the cycle.
			var first = members.OrderBy(rule => order[rule]).First();

			var (cycle, closing) = Around(first, around, standing, graph);

			facts.Told.Add((closing, cycle, cut));
		}

		facts.Told.Sort((one, other) => order[one.Closing].CompareTo(order[other.Closing]));

		return facts;
	}

	/// <summary>
	/// Whether a cycle passes on what follows it unchanged, and reads nothing but the text and the position on the way
	/// (<see cref="Marked"/>): the rules of a component, each call among them that may be reached without moving the last
	/// thing read, none of them read at a strength, no value built around it read by a guard, and no context.
	/// </summary>
	static bool Passes(HashSet<RuleSymbol> around, RecognitionGraph graph)
	{
		// A rule read at a strength reads its operators after the call it climbs from.
		if (around.Any(graph.Climbing.ContainsKey))
			return false;

		foreach (var rule in around)
			if (graph.Bodies.TryGetValue(rule, out var body) && !Last(body, around, graph))
				return false;

		// What the cycle reads and what reads its values: every rule that reaches it, and every rule it reaches.
		var reaching = Reaching(around, graph, inward: true);
		var reached  = Reaching(around, graph, inward: false);

		foreach (var rule in reached)
			if (graph.Bodies.TryGetValue(rule, out var body) && NodeWalk.Reading(body, graph).Any(node => NamesContext(node, graph)))
				return false;

		foreach (var rule in reaching)
			if (graph.Bodies.TryGetValue(rule, out var body) && GuardsRead(body, reaching, graph))
				return false;

		return true;
	}

	/// <summary>
	/// Whether every call of a body into the component that may be reached without moving is the last thing its
	/// alternative reads: the walk of <see cref="LeftCalls"/>, told at each part whether anything is read after it.
	/// </summary>
	static bool Last(Node body, HashSet<RuleSymbol> around, RecognitionGraph graph)
	{
		var pending = new Stack<(Node Node, bool Last)>();

		pending.Push((body, true));

		while (pending.Count > 0)
		{
			var (node, last) = pending.Pop();

			switch (node)
			{
				case Node.Call(var callee, _):
					if (!last && around.Contains(callee))
						return false;

					break;

				case Node.Sequence(var nodes):
					for (var i = 0; i < nodes.Count; i++)
					{
						pending.Push((nodes[i], last && i == nodes.Count - 1));

						if (!FirstSets.MayStandStill(nodes[i], graph))
							break;
					}

					break;

				case Node.Choice(var nodes):
					foreach (var child in nodes)
						pending.Push((child, last));

					break;

				// An option reads nothing after its one turn; a repetition that may turn again reads its next turn.
				case Node.Repeat(var repeated, _, var most):
				{
					var recovers = graph.Recoveries.TryGetValue(node, out var recovery);

					pending.Push((repeated, last && most == 1 && !recovers));

					if (recovers)
						pending.Push((recovery!.Sync, false));

					break;
				}

				// What builds a value or marks the extent reads nothing; a guard on a value is asked of the whole grammar.
				case Node.Capture  (_, var captured):    pending.Push((captured, last)); break;
				case Node.Construct(var built, _):       pending.Push((built, last));    break;
				case Node.Marked   (var marked, _):      pending.Push((marked, last));   break;

				// A lookahead goes back to where it began, a { } commits to what it read, a binding is read again later.
				case Node.Bind     (_, var bound, _):    pending.Push((bound, false));   break;
				case Node.Counted  (var counted, _, _):  pending.Push((counted, false)); break;
				case Node.Atomic   (var atomic):         pending.Push((atomic, false));  break;
				case Node.Lookahead(_, var ahead):       pending.Push((ahead, false));   break;
			}
		}

		return true;
	}

	/// <summary>The rules of a component and every rule that calls into it (<paramref name="inward"/>), or that it calls.</summary>
	static HashSet<RuleSymbol> Reaching(HashSet<RuleSymbol> around, RecognitionGraph graph, bool inward)
	{
		var calls = new Dictionary<RuleSymbol, List<RuleSymbol>>();

		foreach (var pair in graph.Bodies)
			foreach (var node in NodeWalk.Reading(pair.Value, graph))
				if (node is Node.Call(var callee, _))
				{
					var (from, to) = inward ? (callee, pair.Key) : (pair.Key, callee);

					if (!calls.TryGetValue(from, out var next))
						calls[from] = next = [];

					next.Add(to);
				}

		var found   = new HashSet<RuleSymbol>(around);
		var pending = new Stack<RuleSymbol>(around);

		while (pending.Count > 0)
			if (calls.TryGetValue(pending.Pop(), out var next))
				foreach (var one in next)
					if (found.Add(one))
						pending.Push(one);

		return found;
	}

	/// <summary>
	/// Whether a <c>when</c> or a <c>switch</c> of a body reads a capture of something that reads the cycle, or names
	/// such a rule: a value that grows with how deep the cycle went.
	/// </summary>
	static bool GuardsRead(Node body, HashSet<RuleSymbol> reaching, RecognitionGraph graph)
	{
		var all    = NodeWalk.Reading(body, graph).ToList();
		var guards = new List<string>();

		// A switch's selector is a guard among the choice's children.
		foreach (var node in all)
			if (node is Node.Guard(var text))
				guards.Add(text);

		if (guards.Count == 0)
			return false;

		var names = new HashSet<string>(StringComparer.Ordinal);

		foreach (var node in all)
			if (node is Node.Capture(var name, var captured) &&
				NodeWalk.Reading(captured, graph).Any(inner => inner is Node.Call(var callee, _) && reaching.Contains(callee)))
			{
				names.Add(name);
			}

		foreach (var rule in reaching)
			names.Add(NameOf(rule, graph));

		return guards.Any(text => names.Any(name => Names(text, name, graph)));
	}

	/// <summary>Whether a node names the grammar's context: a <c>when</c>, a <c>=&gt;</c> or a recognizer that is handed it.</summary>
	static bool NamesContext(Node node, RecognitionGraph graph)
	{
		return graph.Context is not null && node switch
		{
			Node.External { UsesContext: true }                                  => true,
			Node.Guard { Text: var text }                                        => Names(text, "context", graph),
			Node.Construct { How: Construction.Expression { Text: var text } }   => Names(text, "context", graph),
			_                                                                    => false,
		};
	}

	/// <summary>
	/// Whether C# names an identifier: what the scanner found free in it where the graph has a scanner's answer, and the
	/// spelling as a whole word where it has none.
	/// </summary>
	static bool Names(string text, string name, RecognitionGraph graph)
	{
		return graph.FreeNames.TryGetValue(text, out var free)
			? free.Contains(name)
			: graph.Language.Spells(text, name, NameSearch.Word);
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
