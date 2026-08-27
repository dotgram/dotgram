using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// A captured call compiled as its callee's body, in place, inside the engine.
/// </summary>
/// <remarks>
/// <para>
/// A valueless call that writes nothing has been inlined since the first proofs; a
/// valued call kept its ceremony — a Call entry, its Completed rewrite, a RuleCapture,
/// two passes through the dispatcher — because the value had to be materialized at the
/// rule's boundary. Where the callee is the flat-value shape — one factory over
/// captures that are spans of the input — the boundary says nothing the spans do not:
/// the site compiles the callee's body in place, the callee's captures record into
/// slots of the site's own, and the materializer builds the member by calling the
/// callee's factory over those spans directly. Nothing about the site is written that
/// was not already paid for, which is the lesson the eager experiment bought: the
/// ceremony's cost is the writing, and only a generation-time decision writes less.
/// </para>
/// <para>
/// No silence is asked of anything. The site's captures are ordinary arena records —
/// backtracking into or over the site unwinds them the way it unwinds any capture —
/// so every call site of a qualifying rule qualifies, settled or not. Construction is
/// untouched: the factory still runs at Accept, off the records of the accepted
/// derivation, deferred exactly as before.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>One captured call compiled in place.</summary>
	/// <param name="Callee">The rule whose body stands where the call was.</param>
	/// <param name="Base">Where the site's run of slots begins.</param>
	/// <param name="Members">The callee's one factory's members, in argument order.</param>
	/// <param name="Slots">The callee's capture nodes, renumbered into the site's run.</param>
	sealed record SitePlan(
		RuleSymbol Callee, int Base, IReadOnlyList<ResultMember> Members,
		IReadOnlyDictionary<Node, int> Slots);

	// By identity, not record equality: `t: Line` written in two rules is two sites,
	// and their nodes compare equal as records (CaptureLayout.cs's NodeIdentity).
	readonly Dictionary<Node, SitePlan> _sites = new(NodeIdentity.Instance);
	readonly Dictionary<int, SitePlan> _siteOfSlot = [];

	/// <summary>The slot renumbering of the site being compiled, or null outside one.</summary>
	IReadOnlyDictionary<Node, int>? _siteSlots;

	int SlotOf(Node capture) =>
		_siteSlots is not null && _siteSlots.TryGetValue(capture, out var sited)
			? sited
			: _captureSlots[capture];

	/// <summary>Whether any written state touches the capture local for a slot.</summary>
	/// <remarks>
	/// A rule every call of which was compiled as a site leaves its own states
	/// unreachable, and an unused local is a warning in somebody else's build — the
	/// same lesson <see cref="UsesChar"/> carries, checked the same way.
	/// </remarks>
	bool UsesCapture(int slot)
	{
		foreach (var index in _order)
			if (_bodies[index].Contains($"capture{slot} ", StringComparison.Ordinal) ||
				_bodies[index].Contains($"capture{slot},", StringComparison.Ordinal))
				return true;

		return false;
	}

	/// <summary>The site a scalar member's one slot was compiled as, or null.</summary>
	SitePlan? SiteFor(int offset, ResultMember member) =>
		member is { Rule: not null, IsSequence: false, Slots.Count: 1 } &&
		_siteOfSlot.TryGetValue(offset + member.Slots[0], out var plan)
			? plan
			: null;

	/// <summary>
	/// Find every captured call whose callee can stand in place, and give each site a run
	/// of slots of its own — the same rule inlined twice may not share records.
	/// </summary>
	/// <remarks>
	/// After the first constructor pass, because qualification reads <see cref="_factories"/>
	/// of callees that pass may not have reached yet. A machine that recovers plans no
	/// sites: a recovery reads elements off the rule-capture protocol this replaces.
	/// </remarks>
	void PlanSites()
	{
		if (_recoveryPlans.Count > 0)
			return;

		foreach (var rule in _rules)
		{
			// A fold's factories are iterations read off Construct entries per completed
			// call, a shape the sited member walk does not speak. A rule with a guard
			// reads members mid-parse through the completed-call protocol a site removes.
			if (_graph.Folds.ContainsKey(rule) ||
				_graph.Climbing.ContainsKey(rule) ||
				NodeWalk.Descendants(_graph.Bodies[rule]).Any(static node => node is Node.Guard))
				continue;

			var offset  = _captureOffsets[rule];
			var members = _graph.Results[rule];

			foreach (var node in NodeWalk.Descendants(_graph.Bodies[rule]))
			{
				if (node is not Node.Capture(_, Node.Call(var called, { Count: 0 })) ||
					!SitedValued(called))
					continue;

				// The capturing member must be a scalar with this one slot: a sequence
				// member counts per-turn records, and a member filled from two branches
				// would need two sites told apart — both stay on the ceremony.
				var slot   = _captureSlots[node];
				var scalar = false;

				foreach (var member in members)
					if (member.Slots.Count == 1 && offset + member.Slots[0] == slot)
					{
						scalar = member.Rule is not null && !member.IsSequence;

						break;
					}

				if (!scalar)
					continue;

				var layout = CaptureLayout.Of(
					_graph.Bodies[called],
					other => _graph.Results[other].Count > 0 || _graph.Types.ContainsKey(other));
				var slots = new Dictionary<Node, int>(NodeIdentity.Instance);

				foreach (var captured in NodeWalk.Descendants(_graph.Bodies[called]))
					if (captured is Node.Capture)
					{
						var sited = _captures + layout.SlotOf(captured);

						slots[captured] = sited;
						_textCaptures.Add(sited);

						if (_graph.Recursive.Contains(rule))
							_nestedCaptures.Add(sited);
					}

				var plan = new SitePlan(called, _captures, _factories[called][0].Members, slots);

				_sites[node]      = plan;
				_siteOfSlot[slot] = plan;
				_captures        += layout.Slots.Count;
			}
		}
	}

	/// <summary>
	/// Whether a rule's value can be built from spans a call site records itself: one
	/// factory, every member a required-or-optional span of the input, at least one
	/// required so that an absent site is tellable from a present one.
	/// </summary>
	bool SitedValued(RuleSymbol rule)
	{
		if (_sitedValued.TryGetValue(rule, out var known))
			return known;

		var answer = ComputeSitedValued(rule);

		_sitedValued[rule] = answer;

		return answer;
	}

	readonly Dictionary<RuleSymbol, bool> _sitedValued = [];

	bool ComputeSitedValued(RuleSymbol rule)
	{
		if (!_graph.Types.ContainsKey(rule) ||
			_results.QualifiedOf(rule) is null ||
			_graph.Folds.ContainsKey(rule) ||
			_graph.Climbing.ContainsKey(rule) ||
			_graph.Recursive.Contains(rule) ||
			!_graph.Bodies.TryGetValue(rule, out var body) ||
			!_factories.TryGetValue(rule, out var factories) ||
			factories.Count != 1)
			return false;

		if (body is not Node.Construct(var built, _) || !ReferenceEquals(factories[0].Of, body))
			return false;

		var factory = factories[0];

		if (CSharpEmitter.WantsText(factory) ||
			CSharpEmitter.Asks(factory, "parserSpan") ||
			CSharpEmitter.Asks(factory, "parserInput") ||
			factory.Accumulator is not null)
			return false;

		var witnessed = false;

		foreach (var member in factory.Members)
		{
			if (member.Rule is not null || member.IsSequence || member.Slots.Count != 1)
				return false;

			witnessed |= !member.IsOptional;
		}

		return witnessed && SpanCaptures(built, repeated: false);
	}

	/// <summary>
	/// Every capture is a span of the input under no repetition that could multiply it —
	/// pure text below, nothing valued, nothing sited further down.
	/// </summary>
	static bool SpanCaptures(Node node, bool repeated) =>
		node switch
		{
			Node.Capture(_, var captured)     => !repeated && Extent(captured),
			Node.Sequence(var parts)          => parts.All(part => SpanCaptures(part, repeated)),
			Node.Choice(var alternatives)     => alternatives.All(part => SpanCaptures(part, repeated)),
			Node.Repeat(var body, _, var max) => SpanCaptures(body, repeated || max != 1),
			Node.Atomic(var body)             => SpanCaptures(body, repeated),
			Node.Lookahead(_, var seen)       => NodeWalk.Descendants(seen).All(
			                                         static inner => inner is not Node.Capture),
			Node.Construct                    => false,
			Node.Guard                        => false,
			Node.External { HasValue: true }  => false,
			_                                 => true,
		};

	/// <summary>The callee's body compiled where its captured call stood.</summary>
	int CompileSite(Node.Capture capture, SitePlan site, int next, FollowSets.Continuation following)
	{
		// The inlined body composes continuations against its own namespace's seam,
		// exactly as the valueless inline above does.
		var outerSeam = _seam;
		var handed    = following;

		_seam = FollowSets.SeamOf(site.Callee, _graph);

		if (!ReferenceEquals(_seam, outerSeam))
			handed = new FollowSets.Continuation(following.Plain, FirstSets.First.All);

		var saved = _siteSlots;

		_siteSlots = site.Slots;

		var inlined = Compile(_graph.Bodies[site.Callee], next, handed);

		_siteSlots = saved;
		_seam      = outerSeam;

		return inlined;
	}
}
