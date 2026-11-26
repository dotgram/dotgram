using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

sealed partial class GrammarNormalizer
{
	/// <summary>
	/// What each rule's settled copy was made from (<see cref="SettleRunTails"/>), the copy first.
	/// </summary>
	readonly Dictionary<RuleSymbol, RuleSymbol> _settledFrom = [];

	/// <summary>
	/// Each node of a settled copy, with the node of its rule's body it was copied from — or, for one
	/// rebuilt in the copy, the node it was rebuilt from (<see cref="RecognitionGraph.SettledNodes"/>).
	/// </summary>
	readonly Dictionary<Node, Node> _settledNodes = new(NodeIdentity.Instance);

	/// <summary>What <see cref="RecognitionGraph.PositionalFollow"/> will say, for the follow sets asked here.</summary>
	bool _positionalFollow;

	/// <summary>What <see cref="RecognitionGraph.PositionalSplit"/> will say with it.</summary>
	bool _positionalSplit;

	/// <summary>
	/// Gives a rule that ends in a run which gives characters back a copy that does not, for the
	/// places it is called from where nothing could take a character it gave back.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A rule is compiled once, and what follows its last repetition is the union of what follows
	/// every call of it (<see cref="FollowSets"/>). <c>Name = [a-z] &amp; [a-z0-9]*</c> called once
	/// where a name of an attribute may follow and once before <c>'='</c> gives characters back
	/// at both: the first needs it, <c>&lt;ab="1"&gt;</c> is <c>&lt;a</c> and then <c>b="1"</c>. At
	/// the second every character given back is refused by the <c>'='</c> at once, and a long
	/// name refused after it is cut into two names every way there is: as many ways back as the
	/// name has characters, each of which gives back as many again.
	/// </para>
	/// <para>
	/// So the calls of such a rule are told apart. A call whose own continuation proves that the
	/// run never gives back (<see cref="Determinism.NeverGivesBack"/>) calls a copy of the rule, and
	/// the copy goes on down its tail: a call at the end of the copy to a rule that ends in the run
	/// calls that rule's copy, and a rule that ends in a call of itself calls its own copy there.
	/// What follows a copy is then what follows those calls only, worked out again by the same
	/// fixed point as for any rule, and every proof the reader is written from applies to it
	/// unchanged. One copy a rule, whichever calls want it.
	/// </para>
	/// <para>
	/// The copy reads what the rule reads and builds what it builds: only a reading that the
	/// continuation refuses at its first character is not tried. It keeps the rule's declaration,
	/// so it is named as the rule is wherever a refusal or a trace names it, and a publication goes
	/// on reading the rule itself.
	/// </para>
	/// <para>
	/// Only over characters, and only where nothing makes the copy a question of its own: not in a
	/// grammar that recovers, where where a bad element ends decides where reading resumes; not for
	/// a rule a fold, a climb, a lift or a recognizer of the host made or shapes; not where the tail
	/// can come back to itself without reading anything (<see cref="Reentry"/>), whose entry is cut
	/// by the rule it is.
	/// </para>
	/// </remarks>
	void SettleRunTails()
	{
		if (_recoveries.Count > 0 || _diagnostics.Any(static one => one.Severity == GramSeverity.Error))
			return;

		var graph  = Settling();
		var follow = FollowSets.Of(graph);

		// The rules whose tail ends in a run that gives back today, with the rules down that tail.
		var tails = new Dictionary<RuleSymbol, RunTail>();

		foreach (var rule in _rules)
			TailOf(rule, graph, follow, tails);

		if (!tails.Values.Any(static tail => tail.Giving.Count > 0))
			return;

		// What follows each call of such a rule where it is written, by the call.
		var sites = new Dictionary<Node, FollowSets.Continuation>(NodeIdentity.Instance);

		foreach (var rule in _rules)
			if (Settles(rule) && _bodies.TryGetValue(rule, out var body))
				Sites(body, follow[rule], FollowSets.SeamOf(rule, graph), graph, tails, sites);

		// Which calls go to a copy, by the rule called; and what follows each copy, which is what
		// follows those calls and, down a tail, what follows the copy whose tail calls it.
		var copies  = new Dictionary<RuleSymbol, FollowSets.Continuation>();
		var settled = new Dictionary<Node, RuleSymbol>(NodeIdentity.Instance);

		foreach (var rule in _rules)
		{
			if (!tails.TryGetValue(rule, out var tail) || tail.Giving.Count == 0)
				continue;

			var after = FollowSets.Continuation.None;
			var calls = new List<Node>();

			foreach (var site in sites)
				if (ReferenceEquals(((Node.Call)site.Key).Rule, rule) && Never(tail, site.Value, graph))
				{
					after = after.Or(site.Value);
					calls.Add(site.Key);
				}

			if (calls.Count == 0)
				continue;

			// The copies down the tail are shared with any other rule's copy that reaches them: each of
			// them is followed by both, and has to settle against both.
			var joined = tail.Down.ToDictionary(
				static down => down, down => copies.TryGetValue(down, out var held) ? held.Or(after) : after);

			if (!joined.All(pair => Never(tails[pair.Key], pair.Value, graph)))
				continue;

			foreach (var pair in joined)
				copies[pair.Key] = pair.Value;

			foreach (var call in calls)
				settled[call] = rule;
		}

		if (settled.Count == 0)
			return;

		var bodies  = new Dictionary<RuleSymbol, Node>(_bodies);
		var results = new Dictionary<RuleSymbol, IReadOnlyList<ResultMember>>(_results);
		var rules   = _rules.ToList();
		var made   = Copies(copies.Keys);

		// The calls are moved to the copies in the bodies as they stand, the copies' own among them:
		// a copy is made from its rule's body with the calls of that body moved already.
		foreach (var rule in rules)
			if (_bodies.TryGetValue(rule, out var body))
				_bodies[rule] = Retargeted(body, call => settled.TryGetValue(call, out var called) ? made[called] : null);

		foreach (var rule in copies.Keys)
		{
			var copy = made[rule];

			_bodies[copy] = Retargeted(
				Copied(_bodies[rule]),
				call => tails[rule].Down.Contains(call.Rule) && made.TryGetValue(call.Rule, out var its) ? its : null,
				tailOnly: true);

			_nullable[copy] = _nullable.TryGetValue(rule, out var nullable) && nullable;

			if (_results.TryGetValue(rule, out var members))
				_results[copy] = members;

			if (_types.TryGetValue(rule, out var type))
				_types[copy] = type;

			if (_trivia.TryGetValue(rule, out var trivia))
				_trivia[copy] = trivia;

			if (_says.TryGetValue(rule, out var says))
				_says[copy] = says;

			_settledFrom[copy] = rule;

			_rules.Insert(_rules.IndexOf(rule) + 1, copy);
		}

		// A member of a result names the rule its capture calls, and the emitter reads that rule's
		// value through it: where every call a body made to a rule now goes to the copy, so does the
		// member, or it would name a rule the parser may no longer have.
		foreach (var rule in _rules)
			if (_results.TryGetValue(rule, out var members) && _bodies.TryGetValue(rule, out var body))
				_results[rule] = Members(members, body, made);

		// What the copies were made for, asked again of the grammar they are in: the fixed point over
		// the calls as they now stand. A walk above that disagreed with it would leave a copy whose run
		// still gives back, made for nothing; that is a defect of this pass, and the grammar is then
		// compiled as though the pass had not run.
		var now   = Settling();
		var again = FollowSets.Of(now);

		foreach (var rule in copies.Keys)
		{
			var copy = made[rule];

			foreach (var end in Ends(_bodies[copy]))
				if (end is Node.Repeat repeat && repeat.Min != repeat.Max &&
					!Determinism.NeverGivesBack(repeat, again[copy].Taught, now, FollowSets.SeamOf(copy, now)))
				{
					Unsettled(bodies, results, rules);

					return;
				}
		}
	}

	/// <summary>
	/// The members of a rule's result, each naming the copy of the rule it named where the body calls
	/// the copy and no longer the rule.
	/// </summary>
	static IReadOnlyList<ResultMember> Members(
		IReadOnlyList<ResultMember> members, Node body, Dictionary<RuleSymbol, RuleSymbol> made)
	{
		var called = new HashSet<RuleSymbol>(NodeWalk.Descendants(body).OfType<Node.Call>().Select(static call => call.Rule));
		var moved  = false;
		var result = new List<ResultMember>(members.Count);

		foreach (var member in members)
		{
			if (member.Rule is { } named && made.TryGetValue(named, out var copy) && called.Contains(copy) && !called.Contains(named))
			{
				result.Add(member with { Rule = copy });
				moved = true;
			}
			else
				result.Add(member);
		}

		return moved ? result : members;
	}

	/// <summary>
	/// How many grammars <see cref="SettleRunTails"/> made copies for that did not settle, on any thread
	/// since the process began: a test asks that it stays none.
	/// </summary>
	internal static int Unsettles => _unsettles;

	static int _unsettles;

	/// <summary>Takes back every copy <see cref="SettleRunTails"/> made, and the calls it moved to them.</summary>
	void Unsettled(
		Dictionary<RuleSymbol, Node> bodies, Dictionary<RuleSymbol, IReadOnlyList<ResultMember>> results, List<RuleSymbol> rules)
	{
		System.Threading.Interlocked.Increment(ref _unsettles);

		foreach (var copy in _settledFrom.Keys)
		{
			_bodies.Remove(copy);
			_nullable.Remove(copy);
			_results.Remove(copy);
			_types.Remove(copy);
			_trivia.Remove(copy);
			_says.Remove(copy);
		}

		foreach (var pair in bodies)
			_bodies[pair.Key] = pair.Value;

		foreach (var pair in results)
			_results[pair.Key] = pair.Value;

		_settledFrom.Clear();
		_settledNodes.Clear();
		_rules.Clear();
		_rules.AddRange(rules);
	}

	/// <summary>
	/// The graph as it stands, for the follow sets this pass asks and the proofs it asks them for:
	/// what the emitter's graph says about them, built from the normalizer's tables.
	/// </summary>
	RecognitionGraph Settling()
	{
		return new(_rules, _bodies, _nullable, _results, _types, [], _publications, _diagnostics)
		{
			Language         = _language,
			Folds            = _folds,
			Trivia           = _trivia,
			Recoveries       = _recoveries,
			Lifted           = _lifted,
			Says             = _says,
			Forwarded        = _forwarded,
			Climbing         = _climbing,
			Powers           = _powers,
			Ceilings         = _ceilings,
			Externals        = _externals.ToDictionary(pair => pair.Value, pair => pair.Key),
			Segmented        = _segmented,
			PositionalFollow = _positionalFollow,
			PositionalSplit  = _positionalFollow && _positionalSplit,
		};
	}

	/// <summary>
	/// What a rule ends with, down its tail: the rules on the way, the rule itself first, and the
	/// runs at the end of any of them that give back where the rule is compiled today.
	/// </summary>
	/// <param name="Down">The rule and every rule its tail calls, directly or through another.</param>
	/// <param name="Giving">Each run at the end of one of them that writes a way today, with that rule.</param>
	sealed record RunTail(
		IReadOnlyList<RuleSymbol> Down,
		IReadOnlyList<(RuleSymbol Owner, Node.Repeat Repeat)> Giving);

	/// <summary>A rule with no tail this pass would copy.</summary>
	static readonly RunTail NoRunTail = new([], []);

	RunTail TailOf(
		RuleSymbol rule, RecognitionGraph graph, IReadOnlyDictionary<RuleSymbol, FollowSets.Continuation> follow,
		Dictionary<RuleSymbol, RunTail> tails)
	{
		if (tails.TryGetValue(rule, out var known))
			return known;

		// Down the tail first, so that a tail that comes back to the rule finds it on the way.
		var down    = new List<RuleSymbol>();
		var queue   = new Queue<RuleSymbol>([rule]);
		var seen    = new HashSet<RuleSymbol> { rule };
		var giving  = new List<(RuleSymbol, Node.Repeat)>();
		var seam    = FollowSets.SeamOf(rule, graph);

		while (queue.Count > 0)
		{
			var at = queue.Dequeue();

			if (!Copyable(at, graph) || !ReferenceEquals(FollowSets.SeamOf(at, graph), seam))
				return tails[rule] = NoRunTail;

			down.Add(at);

			foreach (var end in Ends(_bodies[at]))
			{
				if (end is Node.Call(var called, _))
				{
					if (seen.Add(called))
						queue.Enqueue(called);

					continue;
				}

				var repeat = (Node.Repeat)end;

				if (repeat.Min != repeat.Max &&
					!Determinism.NeverGivesBack(repeat, follow[at].Taught, graph, FollowSets.SeamOf(at, graph)))
					giving.Add((at, repeat));
			}
		}

		return tails[rule] = new RunTail(down, giving);
	}

	/// <summary>
	/// Whether a rule can be copied for this pass: one the author declared, read over characters as
	/// it is written, and that nothing outside its body shapes or reads by its identity.
	/// </summary>
	bool Copyable(RuleSymbol rule, RecognitionGraph graph)
	{
		if (rule.Declaration is null || rule.GivesBack || !_bodies.TryGetValue(rule, out var body))
			return false;

		if (_folds.ContainsKey(rule) || _climbing.ContainsKey(rule) || _lifted.ContainsKey(rule) ||
			_externals.ContainsValue(rule) || _settledFrom.ContainsKey(rule))
			return false;

		// A namespace's trivia is known by the rule its seams call: a seam that called a copy would be
		// read as a call like any other, by everything that tells a seam from what it stands between.
		foreach (var seam in _trivia.Values)
			if (seam is Node.Call(var woven, _) && ReferenceEquals(woven, rule))
				return false;

		// The entry of such a rule is cut where one already stands, by the rule it is: a copy would be
		// another rule, entered again without being cut.
		if (Reentry.Found(graph, rule))
			return false;

		foreach (var node in NodeWalk.Descendants(body))
			if (_powers.ContainsKey(node) || _ceilings.ContainsKey(node) || _forwarded.ContainsKey(node))
				return false;

		return true;
	}

	/// <summary>
	/// Whether a rule's body is one this pass reads for calls: not one it would not copy, whose
	/// calls are where its shape says they are and are left as they stand.
	/// </summary>
	bool Settles(RuleSymbol rule)
	{
		return !_folds.ContainsKey(rule) && !_climbing.ContainsKey(rule) && !_lifted.ContainsKey(rule);
	}

	/// <summary>
	/// What a body reads last: the repetitions and the calls without arguments that nothing in the
	/// body reads after, through a choice, a capture, a construction and a sequence's last part.
	/// </summary>
	static IEnumerable<Node> Ends(Node node)
	{
		// Follows the body only, so it is as deep as the rule is written (DeepStack).
		if (DeepStack.Low)
			return DeepStack.Run(() => Ends(node).ToList());

		return Ending(node);
	}

	static IEnumerable<Node> Ending(Node node)
	{
		switch (node)
		{
			case Node.Repeat repeat:
				yield return repeat;

				break;

			case Node.Call { Arguments.Count: 0 } call:
				yield return call;

				break;

			case Node.Choice { Selection: null } choice:
				foreach (var alternative in choice.Nodes)
					foreach (var end in Ends(alternative))
						yield return end;

				break;

			case Node.Sequence { Nodes.Count: > 0 } sequence:
				foreach (var end in Ends(sequence.Nodes[^1]))
					yield return end;

				break;

			case Node.Capture(_, var captured):
				foreach (var end in Ends(captured))
					yield return end;

				break;

			case Node.Construct(var built, _):
				foreach (var end in Ends(built))
					yield return end;

				break;
		}
	}

	/// <summary>Whether no run down a tail gives back before what begins <paramref name="after"/>.</summary>
	static bool Never(RunTail tail, FollowSets.Continuation after, RecognitionGraph graph)
	{
		foreach (var (owner, repeat) in tail.Giving)
			if (!Determinism.NeverGivesBack(repeat, after.Taught, graph, FollowSets.SeamOf(owner, graph)))
				return false;

		return true;
	}

	/// <summary>
	/// Every call of a rule with a giving tail in a body, and what follows it there: the walk
	/// <see cref="FollowSets"/> makes, kept by the call rather than folded into the rule called.
	/// </summary>
	static void Sites(
		Node node, FollowSets.Continuation after, RuleSymbol? seam, RecognitionGraph graph,
		Dictionary<RuleSymbol, RunTail> tails, Dictionary<Node, FollowSets.Continuation> sites)
	{
		// Follows the body only, so it is as deep as the rule is written (DeepStack).
		if (DeepStack.Low)
		{
			DeepStack.Run(() =>
			{
				Sites(node, after, seam, graph, tails, sites);

				return true;
			});

			return;
		}

		switch (node)
		{
			case Node.Call(var called, var arguments):
			{
				if (arguments.Count != 0 || !tails.TryGetValue(called, out var tail) || tail.Giving.Count == 0)
					return;

				var told = ReferenceEquals(FollowSets.SeamOf(called, graph), seam) ? after : after.Unseamed();

				sites[node] = sites.TryGetValue(node, out var held) ? held.Or(told) : told;

				return;
			}

			case Node.Sequence(var parts):
			{
				var next = after;

				for (var i = parts.Count - 1; i >= 0; i--)
				{
					Sites(parts[i], next, seam, graph, tails, sites);

					next = FollowSets.Precedes(parts[i], next, graph, seam);
				}

				return;
			}

			case Node.Choice(var alternatives):
				foreach (var alternative in alternatives)
					Sites(alternative, after, seam, graph, tails, sites);

				return;

			case Node.Repeat(var body, _, var max):
				Sites(body, max == 1 ? after : FollowSets.Precedes(body, after, graph, seam).Or(after), seam, graph, tails, sites);

				return;

			case Node.Counted(var body, _, var max):
				Sites(
					body,
					max is CountBound.Items(1) ? after : FollowSets.Precedes(body, after, graph, seam).Or(after),
					seam, graph, tails, sites);

				return;

			case Node.Capture(_, var captured): Sites(captured, after, seam, graph, tails, sites); return;
			case Node.Construct(var built, _):  Sites(built,    after, seam, graph, tails, sites); return;
			case Node.Atomic(var kept):         Sites(kept,     after.Committed(), seam, graph, tails, sites); return;
			case Node.Marked(var kept, _):      Sites(kept,     after, seam, graph, tails, sites); return;
			case Node.Bind(_, var bound, _):    Sites(bound,    after, seam, graph, tails, sites); return;

			case Node.Lookahead(_, var seen):
				Sites(seen, FollowSets.Continuation.Sealed, seam, graph, tails, sites);

				return;
		}
	}

	/// <summary>A copy of each rule, named for the rule and declared as it is.</summary>
	Dictionary<RuleSymbol, RuleSymbol> Copies(IEnumerable<RuleSymbol> rules)
	{
		var made = new Dictionary<RuleSymbol, RuleSymbol>();

		foreach (var rule in rules)
		{
			var name  = rule.Name + "_Settled";
			var taken = name;

			for (var i = 2; Named(taken); i++)
				taken = name + "_" + Text(i);

			made[rule] = new RuleSymbol(taken, rule.Namespace, rule.Declaration);
		}

		return made;
	}

	/// <summary>A copy of a body that shares no node with it, carrying what each node carries.</summary>
	Node Copied(Node body)
	{
		var copy = CloneBody(body, NoTargets, [], "Settled");

		Carried(body, copy);

		return copy;
	}

	/// <summary>Carries what is recorded against each node of a body onto its copy, node for node.</summary>
	void Carried(Node from, Node to)
	{
		// Follows the body only, so it is as deep as the rule is written (DeepStack).
		if (DeepStack.Low)
		{
			DeepStack.Run(() =>
			{
				Carried(from, to);

				return true;
			});

			return;
		}

		Carry(from, to);

		_settledNodes[to] = from;

		using var children = from.Children.GetEnumerator();
		using var copies   = to.Children.GetEnumerator();

		while (children.MoveNext() && copies.MoveNext())
			Carried(children.Current, copies.Current);
	}

	/// <summary>
	/// A body with each call that <paramref name="to"/> names a rule for calling that rule instead,
	/// rebuilt only on the way to such a call and carrying what each rebuilt node carried.
	/// </summary>
	/// <param name="tailOnly">Only the calls the body reads last (<see cref="Ends"/>).</param>
	Node Retargeted(Node node, Func<Node.Call, RuleSymbol?> to, bool tailOnly = false)
	{
		// Follows the body only, so it is as deep as the rule is written (DeepStack).
		if (DeepStack.Low)
			return DeepStack.Run(() => Retargeted(node, to, tailOnly));

		var rebuilt = node switch
		{
			Node.Call { Arguments.Count: 0 } call when to(call) is { } called => new Node.Call(called, []),
			Node.Call => node,

			Node.Sequence sequence when tailOnly =>
				sequence.Nodes.Count == 0 ? node : Rebuilt(sequence, sequence.Nodes.Count - 1),
			Node.Sequence sequence => Each(sequence, sequence.Nodes, nodes => sequence with { Nodes = nodes }),
			Node.Choice { Selection: not null } when tailOnly => node,
			Node.Choice choice => Each(choice, choice.Nodes, choice.Rebuild),

			Node.Capture capture   => One(capture.Body, body => capture with { Body = body }),
			Node.Construct built   => One(built.Body,   body => built with { Body = body }),
			_ when tailOnly        => node,
			Node.Atomic atomic     => One(atomic.Body,  body => atomic with { Body = body }),
			Node.Marked marked     => One(marked.Body,  body => marked with { Body = body }),
			Node.Repeat repeat     => One(repeat.Body,  body => repeat with { Body = body }),
			Node.Counted counted   => One(counted.Body, body => counted with { Body = body }),
			Node.Lookahead look    => One(look.Body,    body => look with { Body = body }),
			Node.Bind bind         => One(bind.Body,    body => bind with { Body = body }),
			_                      => node,
		};

		Carry(node, rebuilt);

		if (tailOnly && !ReferenceEquals(node, rebuilt))
			_settledNodes[rebuilt] = node;

		return rebuilt;

		Node One(Node inner, Func<Node, Node> with)
		{
			var changed = Retargeted(inner, to, tailOnly);

			return ReferenceEquals(changed, inner) ? node : with(changed);
		}

		Node Each(Node whole, IReadOnlyList<Node> parts, Func<IReadOnlyList<Node>, Node> with)
		{
			var changed = parts.Select(part => Retargeted(part, to, tailOnly)).ToList();

			return changed.Where((part, i) => !ReferenceEquals(part, parts[i])).Any() ? with(changed) : whole;
		}

		Node Rebuilt(Node.Sequence sequence, int last)
		{
			var changed = Retargeted(sequence.Nodes[last], to, tailOnly);

			return ReferenceEquals(changed, sequence.Nodes[last])
				? sequence
				: sequence with { Nodes = [.. sequence.Nodes.Take(last), changed] };
		}
	}
}
