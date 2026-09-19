using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Where each reading of the graph is settled: for every call and every construction a body
/// reads, the innermost point past which what it read is never taken back, so that a carrier
/// that builds as it reads may build it there (docs/design/fix-reader-2026-09-18.md §5, §7).
/// </summary>
/// <remarks>
/// <para>
/// §7.3 has a factory run once for each node of the derivation that stood. The tape keeps
/// that by building after the parse; a carrier that builds earlier keeps it only where it
/// builds past such a point. <see cref="Replay"/> answers the question for a rule as a
/// whole — the end of a rule whose readings are kept — and this refines it inside the rule:
/// FIX's <c>Field</c> is followed in its own body by a part that can fail, but past a turn
/// of the repetition it is read in, which only <c>recover</c> takes back, it is settled.
/// </para>
/// <para>
/// A point is one of four (<see cref="Kind"/>): the end of a rule that is kept; the end of a
/// turn of a repetition that never gives a completed turn back; the end of an atomic group,
/// whose first reading is its only one; or, in a rule that is not kept itself, wherever the
/// call that read the rule is settled, where every call of it is. A repetition or a group is
/// a point only where its own completed reading is kept — nothing after it can take it back
/// except a failure of the whole parse, and it is not inside a lookahead, a call's arguments
/// or a repetition that may give its turn back — and only in a rule that is kept, so that the
/// whole chain above it holds. The innermost such point is the answer; a site with none is
/// built after the parse, as the tape builds it. A point says when to build, not from what: what
/// is built there comes from the derivation that reached it, so a reading given back before it —
/// the inner call of a recursion that the rule gave up before its outer call completed — is not
/// on that derivation, and nothing built at the point comes from it.
/// </para>
/// <para>
/// A rule counts as kept here where Replay keeps it and nothing reads it where a reading is
/// thrown away whatever it says: under a lookahead, in a call's arguments, in a seam's own
/// body or a recovery's synchronization, or under a rule that is read there. Replay calls a
/// reading under a lookahead by the reason it had outside it, which says what it must about
/// the value handed on, and nothing about a factory run for a reading nobody keeps.
/// </para>
/// <para>
/// What counts as kept is <see cref="Replay.Report.Keeps"/>'s measure, the one the carrier
/// <c>Auto</c> already builds early by: a reading put back only where the whole parse fails
/// hands no value to anyone. It does admit a factory run for a parse that in the end fails,
/// as Auto's immediate carrier runs one now; a factory with an effect beyond its value sees
/// that. And the premise is Replay's: a way the emitted reader keeps
/// open past a point is a fact about the reader, not about the graph. A turn of a recovering
/// repetition drops its ways when it commits and an atomic group drops them by definition;
/// elsewhere a rendering that builds at a point asks the reader, as <c>Auto</c> asks it
/// whether a rule can be read again after it answered.
/// </para>
/// <para>
/// What a guard is handed is not moved by this: <see cref="Demand"/> calls it
/// <see cref="Demand.Kind.Always"/> and it is built where it is read, before the point.
/// </para>
/// <para>
/// It reads the graph and <see cref="Replay"/>'s answer and nothing else. Every body is
/// walked once, as Replay walks it; the calls between rules are settled by one pass over a
/// work list.
/// </para>
/// </remarks>
public static class Commit
{
	/// <summary>What a point is the end of.</summary>
	public enum Kind : byte
	{
		/// <summary>
		/// The rule the site is read in, a rule that stands: <see cref="Replay.Report.Keeps"/> keeps
		/// it, and nothing reads it where a reading is thrown away.
		/// </summary>
		Rule,

		/// <summary>
		/// A turn of a repetition that never gives a completed turn back: one marked
		/// <c>recover</c> whose continuation is known whole (<see cref="Recovering(RecognitionGraph, RuleSymbol, Node)"/>),
		/// or one <see cref="Determinism.NeverGivesBack"/> proves.
		/// </summary>
		Turn,

		/// <summary>An atomic group, whose first reading is its only one.</summary>
		Atomic,

		/// <summary>
		/// Wherever the call that read the site's rule is settled: the rule is not kept itself,
		/// but every call of it has a point, and what it read is part of what that call read.
		/// </summary>
		Caller,
	}

	/// <summary>
	/// A point: the node whose end it is — the repetition, the atomic group, or the body of
	/// the rule for <see cref="Kind.Rule"/> and <see cref="Kind.Caller"/> — and the rule it
	/// is in.
	/// </summary>
	public readonly record struct Point(Node At, Kind Kind, RuleSymbol Rule);

	/// <summary>A call or a construction a body reads, and the rule whose body it is.</summary>
	public readonly record struct Site(Node Node, RuleSymbol Owner);

	/// <summary>What the graph says about every site in it.</summary>
	public sealed class Report
	{
		readonly Dictionary<Node, Point> _points;

		internal Report(Site[] sites, Dictionary<Node, Point> points)
		{
			Sites   = sites;
			_points = points;
		}

		/// <summary>Every call and construction the rules' bodies read, in the order they are written.</summary>
		public Site[] Sites { get; }

		/// <summary>How many of <see cref="Sites"/> have a point.</summary>
		public int Settled => _points.Count;

		/// <summary>The point past which this site's reading is settled; false where it is settled only after the parse.</summary>
		public bool TryGet(Node site, out Point point)
		{
			return _points.TryGetValue(site, out point);
		}
	}

	/// <summary>Every site of the graph and its point.</summary>
	public static Report Of(RecognitionGraph graph, Replay.Report replay)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		if (replay is null)
			throw new ArgumentNullException(nameof(replay));

		var follow = FollowSets.Of(graph);
		var pass   = new Pass(graph, replay, Called(graph));

		foreach (var rule in graph.Rules)
			if (graph.Bodies.TryGetValue(rule, out var body))
				pass.Rule(rule, body, follow.TryGetValue(rule, out var after) ? after : FollowSets.Continuation.All);

		return pass.Settle();
	}

	/// <summary>
	/// The continuation a repetition marked <c>recover</c> tries first at every boundary, where it
	/// is known whole, or null where it is not: the scenario of a repetition whose only way back
	/// is <c>recover</c> (docs/design/fix-reader-2026-09-18.md §8).
	/// </summary>
	/// <remarks>
	/// <para>
	/// §8.2 tries the complete continuation at every boundary first. It is known where nothing
	/// outside the rule follows it — no rule calls the rule, so only a publication reads it — and
	/// the repetition is either the last thing the rule reads, so that what follows is the end
	/// the publication reads to (the continuation is empty), or is followed in the rule's own
	/// sequence by parts that end in <c>eof</c>. A continuation that succeeds there has read to
	/// the end of the input, and one that fails leaves the turn to the element or its recovery:
	/// nothing takes a completed turn back, and its end is a point (<see cref="Kind.Turn"/>).
	/// </para>
	/// <para>
	/// A repetition with a bound, or whose element can be empty, is not taken - except the step a
	/// <c>yield</c> is lowered to, a <c>recover</c> repetition of exactly one turn marked as the
	/// step: its continuation is the next step the driver asks for, and past its one turn the
	/// driver has handed the element out.
	/// </para>
	/// </remarks>
	public static Node[]? Recovering(RecognitionGraph graph, RuleSymbol rule, Node node)
	{
		foreach (var body in graph.Bodies.Values)
			foreach (var one in NodeWalk.Descendants(body))
				if (one is Node.Call(var called, _) && called == rule)
					return null;

		return Recovering(graph, rule, node, called: false);
	}

	static Node[]? Recovering(RecognitionGraph graph, RuleSymbol rule, Node node, bool called)
	{
		if (called || node is not Node.Repeat repeat || !graph.Recoveries.TryGetValue(node, out var recovery) ||
			!(repeat.Max is null || recovery.YieldStep && repeat is { Min: 1, Max: 1 }) ||
			FirstSets.Nullable(repeat.Body, graph) || !graph.Bodies.TryGetValue(rule, out var top))
			return null;

		while (top is Node.Construct(var built, _))
			top = built;

		if (Holds(top, node))
			return [];

		if (top is Node.Sequence(var parts))
			for (var i = 0; i < parts.Count; i++)
				if (Holds(parts[i], node))
				{
					// Last, it reads to the end the publication reads to; followed, what follows
					// has to end there itself.
					if (i < parts.Count - 1 && !IsEnd(parts[parts.Count - 1]))
						return null;

					var continuation = new Node[parts.Count - i - 1];

					for (var j = 0; j < continuation.Length; j++)
						continuation[j] = parts[i + 1 + j];

					return continuation;
				}

		return null;

		// The repetition itself, or the capture of it.
		static bool Holds(Node part, Node repetition)
		{
			return ReferenceEquals(part, repetition) || part is Node.Capture(_, var held) && ReferenceEquals(held, repetition);
		}

		// `eof`, called or as the normalizer lowers its body: nothing may follow.
		static bool IsEnd(Node part)
		{
			return
				part is Node.Call(var called, _) && called.IsBuiltIn && called.Name == "eof" ||
				part is Node.Lookahead { IsPositive: false, Body: Node.Element { IsNegated: true, Ranges.Count: 0, Categories.Count: 0, References.Count: 0 } };
		}
	}

	/// <summary>The rules some body calls.</summary>
	static HashSet<RuleSymbol> Called(RecognitionGraph graph)
	{
		var called = new HashSet<RuleSymbol>();

		foreach (var body in graph.Bodies.Values)
			foreach (var node in NodeWalk.Descendants(body))
				if (node is Node.Call(var rule, _))
					called.Add(rule);

		return called;
	}

	/// <summary>
	/// A site as the walk found it: the innermost point inside its rule, if any, which counts
	/// only where the rule stands, and whether it is read where nothing is settled at all —
	/// under a lookahead or in a call's arguments.
	/// </summary>
	readonly record struct Found(Site Site, Point? Inner, bool Lost);

	/// <summary>The walk of every body, and what it found.</summary>
	sealed class Pass(RecognitionGraph graph, Replay.Report replay, HashSet<RuleSymbol> called)
	{
		readonly List<Found>                     _found = [];
		readonly Dictionary<Node, int>           _index = new(NodeIdentity.Instance);
		readonly Dictionary<RuleSymbol, List<int>> _calls = [];
		readonly HashSet<RuleSymbol>             _lost  = [];

		RuleSymbol  _rule = null!;
		RuleSymbol? _seam;

		/// <summary>Walks one rule's body.</summary>
		public void Rule(RuleSymbol rule, Node body, FollowSets.Continuation after)
		{
			_rule = rule;
			_seam = FollowSets.SeamOf(rule, graph);

			Walk(body, Replay.Because.Stands, elsewhere: false, after, inner: null, lost: false);
		}

		/// <summary>
		/// One node, with what Replay knows of it — <paramref name="taken"/>, the reason the
		/// reading may be taken back from outside, and <paramref name="elsewhere"/>, whether a
		/// failure has somewhere to go — and the innermost point around it.
		/// </summary>
		void Walk(Node node, Replay.Because taken, bool elsewhere, FollowSets.Continuation after, Point? inner, bool lost)
		{
			switch (node)
			{
				case Node.Call(var callee, var arguments):
					Add(node, inner, lost);

					if (!_calls.TryGetValue(_rule, out var owned))
						_calls[_rule] = owned = [];

					owned.Add(_found.Count - 1);

					if (lost)
						_lost.Add(callee);

					foreach (var argument in arguments)
						Walk(argument, taken, elsewhere, FollowSets.Continuation.All, inner, lost: true);

					break;

				case Node.Construct(var body, _):
					Add(node, inner, lost);
					Walk(body, taken, elsewhere, after, inner, lost);

					break;

				// As Replay: a part is put back when anything after it fails.
				case Node.Sequence(var parts):
					for (var i = 0; i < parts.Count; i++)
					{
						var put = taken;

						for (var j = i + 1; j < parts.Count; j++)
							if (Replay.CanFail(parts[j], graph))
							{
								var here = elsewhere ? Replay.Because.Follows : Replay.Because.Losing;

								if (Replay.Weaker(put, here))
									put = here;

								break;
							}

						Walk(parts[i], put, elsewhere, Replay.Next(parts, i + 1, after, _seam, graph), inner, lost);
					}

					break;

				// A completed choice is not reopened; a failure inside an alternative goes to a
				// later one where it can begin where this one began. Neither is a point.
				case Node.Choice(var alternatives):
					for (var i = 0; i < alternatives.Count; i++)
						Walk(
							alternatives[i], taken, elsewhere || !elsewhere && Replay.Replaced(alternatives, i, after.Plain, graph),
							after, inner, lost);

					break;

				case Node.Repeat(var body, _, _) repeat:
				{
					var stops = Replay.Seamed(body, _seam, out var past)
						? Replay.Begins(past, after.AfterSeam, graph) is not { } beyond || !after.AfterSeam.IsKnown || beyond.Overlaps(after.AfterSeam)
						: Replay.Begins(body, after.Plain, graph) is not { } begins || !after.Plain.IsKnown || begins.Overlaps(after.Plain);

					// A failed turn of a recovering repetition is replaced by its recovery.
					var recovers = Recovering(graph, _rule, node, called.Contains(_rule)) is not null;
					var turn     = inner;
					var within   = taken;

					if (recovers || Determinism.NeverGivesBack(repeat, after, graph, _seam))
					{
						if (!lost && Kept(taken))
							turn = new Point(node, Kind.Turn, _rule);
					}
					else if (Replay.Weaker(within, Replay.Because.Turn))
						within = Replay.Because.Turn;

					Walk(body, within, elsewhere || stops || recovers, after, turn, lost);

					break;
				}

				case Node.Lookahead(_, var body):
					Walk(body, Replay.Worse(taken, Replay.Because.Lookahead), elsewhere: true, FollowSets.Continuation.All, inner, lost: true);

					break;

				case Node.Atomic(var body):
				{
					var group = inner;

					if (!lost && Kept(taken))
						group = new Point(node, Kind.Atomic, _rule);

					Walk(body, taken, elsewhere, after, group, lost);

					break;
				}

				case Node.Capture(_, var body):
					Walk(body, taken, elsewhere, after, inner, lost);

					break;

				case Node.Marked(var body, _):
					Walk(body, taken, elsewhere, after, inner, lost);

					break;
			}
		}

		void Add(Node node, Point? inner, bool lost)
		{
			_index[node] = _found.Count;
			_found.Add(new Found(new Site(node, _rule), inner, lost));
		}

		/// <summary>
		/// The points: in a rule that stands, each site's own where it has one inside the rule and
		/// the rule's end where it has none; in one that does not, its callers', where every call of
		/// the rule has a point — found by taking away, from every rule some body calls, each rule
		/// one of whose calls has none, until nothing more goes.
		/// </summary>
		public Report Settle()
		{
			// A call the walk did not read as a site — in a seam's own body or a recovery's
			// synchronization — is thrown away like one under a lookahead.
			foreach (var body in graph.Trivia.Values)
				Unwalked(body);

			foreach (var recovery in graph.Recoveries.Values)
				Unwalked(recovery.Sync);

			// And so is everything read under what is thrown away.
			var pending = new Stack<RuleSymbol>(_lost);

			while (pending.Count > 0)
				if (_calls.TryGetValue(pending.Pop(), out var owned))
					foreach (var at in owned)
						if (_lost.Add(((Node.Call)_found[at].Site.Node).Rule))
							pending.Push(((Node.Call)_found[at].Site.Node).Rule);

			var settled = new HashSet<RuleSymbol>(called);

			foreach (var publication in graph.Publications)
				settled.Remove(publication.Rule);

			settled.ExceptWith(_lost);

			foreach (var found in _found)
				if (found.Site.Node is Node.Call(var callee, _) && settled.Contains(callee) && !Has(found, settled))
				{
					settled.Remove(callee);
					pending.Push(callee);
				}

			while (pending.Count > 0)
				if (_calls.TryGetValue(pending.Pop(), out var owned))
					foreach (var at in owned)
					{
						var found  = _found[at];
						var callee = ((Node.Call)found.Site.Node).Rule;

						if (settled.Contains(callee) && !Has(found, settled))
						{
							settled.Remove(callee);
							pending.Push(callee);
						}
					}

			var sites  = new Site[_found.Count];
			var points = new Dictionary<Node, Point>(NodeIdentity.Instance);

			for (var i = 0; i < _found.Count; i++)
			{
				var found = _found[i];
				var owner = found.Site.Owner;

				sites[i] = found.Site;

				if (!Has(found, settled))
					continue;

				points[found.Site.Node] = Stands(owner)
					? found.Inner ?? new Point(graph.Bodies[owner], Kind.Rule, owner)
					: new Point(graph.Bodies[owner], Kind.Caller, owner);
			}

			return new Report(sites, points);
		}

		/// <summary>Whether a rule's own end is a point: Replay keeps it, and nothing reads it where a reading is thrown away.</summary>
		bool Stands(RuleSymbol rule)
		{
			return replay.Keeps(rule) && !_lost.Contains(rule);
		}

		bool Has(Found found, HashSet<RuleSymbol> settled)
		{
			return !found.Lost && (Stands(found.Site.Owner) || settled.Contains(found.Site.Owner));
		}

		void Unwalked(Node body)
		{
			foreach (var node in NodeWalk.Descendants(body))
				if (node is Node.Call(var callee, _) && !_index.ContainsKey(node))
					_lost.Add(callee);
		}

		/// <summary>Whether a reading taken back only for this reason is kept.</summary>
		static bool Kept(Replay.Because taken)
		{
			return taken is Replay.Because.Stands or Replay.Because.Losing;
		}
	}
}
