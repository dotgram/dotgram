using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

public sealed partial class GrammarNormalizer
{
	/// <summary>
	/// Reads a shared leading operand once instead of once per alternative, where doing so
	/// cannot be seen from the outside.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Ordered choice is not obliged to be spelled with a common prefix shared, and the
	/// author who does not share it writes the same language and gets a different parser:
	/// every alternative after the first reads the prefix again, and the doubling compounds
	/// through nesting. Making the author write for the machine is what a generator is for
	/// avoiding, so the fold is done here.
	/// </para>
	/// <para>
	/// <b>Only where it is invisible.</b> <c>A &amp; X | A &amp; Y</c> and <c>A &amp; (X |
	/// Y)</c> are the same grammar exactly when <c>A</c> has one reading where it stands.
	/// Where it has several the two prefer different ones: the alternatives prefer a shorter
	/// reading of <c>A</c> that lets a tail fit, and the folded form takes <c>A</c>'s own
	/// reading and then chooses. That is not a subtlety about performance, it decides which
	/// alternative matched and so which <c>=&gt;</c> runs. <see cref="Determinism"/> is the
	/// proof, and where it does not reach, nothing is folded and <c>GRAM4016</c> is left to
	/// say so to the author, whose choice it then is.
	/// </para>
	/// <para>
	/// <b>After the checks, and before nothing.</b> The grammar is checked as it was
	/// written — a <c>=&gt;</c> is refused anywhere but on an alternative of the rule, and
	/// the folded shape puts one after a shared head, which the author may not write and the
	/// compiler may. Then the results are computed again, because a fold drops a duplicate
	/// capture and the numbering of the slots is what the emitter will lay out from.
	/// </para>
	/// </remarks>
	void Factor()
	{
		// A recovery is a decision about a shape, and this moves the shape. Left out of the
		// first cut rather than reasoned about carelessly.
		if (_recoveries.Count > 0)
			return;

		var graph  = Provisional();
		var follow = FollowSets.Of(graph);
		var folded = false;

		foreach (var rule in _rules)
		{
			if (!_bodies.TryGetValue(rule, out var body) ||
				!follow.TryGetValue(rule, out var after))
				continue;

			// What this rule's own guards can see, for the commit decision below: a
			// residue folded inside a capture one of them names is a residue whose
			// choice of factory a guard can read back, and it is left uncommitted.
			_guardNamed  = GuardNamed(body);
			_insideNamed = false;

			// A rule whose alternatives all hand on the same capture is written with one `=>`
			// outside the choice, and `CollapseTransparent` makes that shape out of a
			// forwarding rule whether anyone wrote it or not. Given to each alternative
			// instead it says the same thing, and says it where an alternative can be
			// replaced by the body it would have called.
			//
			// A rule's body and nowhere else. An alternative that is itself a choice — which
			// is what collapsing a forwarding rule into one alternative leaves — would become
			// a choice of constructions nested inside the choice above it, and the
			// alternatives of a rule are the ones at the top: nothing would ever give those
			// constructions a factory. Distributing there is not a fold that does not pay, it
			// is a shape the rest of the compiler does not have.
			var given = Given(body) ?? body;
			var rewritten = Folded(given, after, graph, rule);

			// The distribution on its own is a rearrangement worth nothing, so it is kept
			// only where the fold that follows it took.
			if (ReferenceEquals(rewritten, given))
				continue;

			_bodies[rule] = rewritten;
			folded        = true;
		}

		if (folded)
			ComputeResults();
	}

	/// <summary>
	/// Puts a rule's body where a bare pass-through call to it stood, so that a prefix one
	/// call down becomes a prefix the fold can see.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The shape that motivated left-factoring is not two alternatives sharing an operand.
	/// It is one alternative whose prefix <em>is</em> the other alternative, a call down:
	/// </para>
	/// <code>
	/// Primary   = … | c: Call | r: Reference | …
	/// Call      = target: Reference &amp; '(' &amp; … &amp; ')' =&gt; …
	/// Reference = …
	/// </code>
	/// <para>
	/// Every bare reference is read twice — once inside the `Call` that then fails for want
	/// of a bracket, once as itself — and references are most of what a grammar is made of.
	/// Nothing about the two alternatives says so where they are written; what says it is
	/// `Call`'s own first operand.
	/// </para>
	/// <para>
	/// So the alternative that only passes the call's value along is replaced by the body it
	/// would have called. That is an equality, not an approximation: the alternative built
	/// the rule's value out of the callee's value and nothing else, and the callee's body
	/// builds the callee's value. Then the prefix is a prefix where the fold can see it, and
	/// the fold decides on its own terms whether sharing it is invisible.
	/// </para>
	/// <para>
	/// Both alternatives have to be pass-throughs of a call, the two rules and this one have
	/// to declare the same type — the value travels from one to the other unchanged — and
	/// nothing the body captures may already be captured elsewhere in this rule, since after
	/// this it is captured here.
	/// </para>
	/// </remarks>
	Node Inlined(Node node, RuleSymbol owner)
	{
		if (node is not Node.Choice(var alternatives))
			return node;

		List<Node>? rewritten = null;

		for (var at = 0; at + 1 < alternatives.Count; at++)
		{
			if (!Reaching(alternatives[at], out var longer) ||
				!Reaching(alternatives[at + 1], out var shorter) ||
				!Opens(longer, shorter, owner, out var head, out var name))
				continue;

			rewritten ??= [.. alternatives];

			rewritten[at]     = CloneAndRewrite(_bodies[longer], NoTargets, [], owner.Name);
			rewritten[at + 1] = new Node.Construct(head, new Construction.Expression("(" + name + ")"));

			// One pair per choice. A second would be looking at alternatives this one has
			// just replaced, and the fold that follows is what makes anything of either.
			break;
		}

		return rewritten is null ? node : new Node.Choice(rewritten);
	}

	/// <summary>
	/// The one name an expression hands straight back, or null where it does anything else.
	/// </summary>
	/// <remarks>
	/// <c>@(c)</c> arrives here as the text <c>(c)</c> — the parentheses belong to the
	/// notation that introduced the C#, not to the C#, and they are kept because what is
	/// emitted is the text as written. So the brackets come off before the name is compared,
	/// as many layers as there are.
	/// </remarks>
	static string? Handed(string text)
	{
		var handed = text.Trim();

		while (handed.Length > 2 && handed[0] == '(' && handed[^1] == ')' && Wrapping(handed))
			handed = handed[1..^1].Trim();

		foreach (var c in handed)
			if (!char.IsLetterOrDigit(c) && c != '_')
				return null;

		return handed.Length == 0 ? null : handed;
	}

	/// <summary>Whether the first bracket is the one the last closes.</summary>
	static bool Wrapping(string text)
	{
		var depth = 0;

		for (var at = 0; at < text.Length; at++)
		{
			if (text[at] == '(')
				depth++;

			else if (text[at] == ')' && --depth == 0)
				return at == text.Length - 1;
		}

		return false;
	}

	/// <summary>Whether every alternative captures the one name the choice hands on.</summary>
	static bool Handing(IReadOnlyList<Node> alternatives, string name)
	{
		if (alternatives.Count < 2)
			return false;

		foreach (var one in alternatives)
			if (one is not Node.Capture(var captured, _) ||
				!string.Equals(captured, name, StringComparison.Ordinal))
				return false;

		return true;
	}

	/// <summary>
	/// The rule an alternative does nothing with but call and hand on, or false where it
	/// does anything else.
	/// </summary>
	static bool Reaching(Node alternative, out RuleSymbol called)
	{
		called = null!;

		return alternative is Node.Construct(
				Node.Capture(var name, Node.Call(var rule, { Count: 0 })),
				Construction.Expression(var text, _)) &&
			string.Equals(Handed(text), name, StringComparison.Ordinal) &&
			(called = rule) is not null;
	}

	/// <summary>
	/// Whether one rule's body opens with a call to the other, and everything else this
	/// needs is true of them.
	/// </summary>
	bool Opens(RuleSymbol longer, RuleSymbol shorter, RuleSymbol owner, out Node head, out string name)
	{
		head = null!;
		name = "";

		if (ReferenceEquals(longer, shorter) ||
			_folds.ContainsKey(longer) ||
			longer.Declaration is null ||
			!_bodies.TryGetValue(longer, out var body) ||
			!_types.TryGetValue(owner, out var mine) ||
			!_types.TryGetValue(longer, out var his) ||
			!_types.TryGetValue(shorter, out var hers) ||
			!string.Equals(mine, his, StringComparison.Ordinal) ||
			!string.Equals(mine, hers, StringComparison.Ordinal))
			return false;

		var inner = body is Node.Construct(var built, _) ? built : body;

		if (inner is not Node.Sequence(var parts) ||
			parts.Count < 2 ||
			parts[0] is not Node.Capture(var opening, Node.Call(var first, { Count: 0 })) ||
			!ReferenceEquals(first, shorter))
			return false;

		// After this the callee's captures are this rule's, so a name it uses must not
		// already mean something else here.
		var taken = new HashSet<string>(StringComparer.Ordinal);

		foreach (var node in NodeWalk.Descendants(_bodies[owner]))
			if (node is Node.Capture(var used, _))
				taken.Add(used);

		foreach (var node in NodeWalk.Descendants(body))
			if (node is Node.Capture(var used, _) && taken.Contains(used) &&
				!string.Equals(used, opening, StringComparison.Ordinal))
				return false;

		head = parts[0];
		name = opening;

		return true;
	}

	/// <summary>
	/// A body written as one construction over a choice, given to each alternative instead —
	/// or null where it is not that shape.
	/// </summary>
	Node.Choice? Given(Node body) =>
		body is Node.Construct(Node.Choice(var shared), Construction.Expression(var text, _) how) &&
		Handed(text) is { } handed &&
		Handing(shared, handed)
			? new Node.Choice([.. shared.Select(one => (Node)new Node.Construct(one, how))])
			: null;

	/// <summary>The graph as it stands, for the sets this pass reasons with.</summary>
	/// <remarks>
	/// FIRST and FOLLOW are asked of the grammar before the fold and used to decide it,
	/// which is sound in the direction it matters: a fold rearranges the inside of a choice
	/// and changes nothing about what can follow that choice, so what these say about the
	/// position is what they would still say afterwards.
	/// </remarks>
	RecognitionGraph Provisional() =>
		new(_rules, _bodies, _nullable, _results, _types, [], _publications, _diagnostics)
		{
			Folds      = _folds,
			Trivia     = _trivia,
			Recoveries = _recoveries,
			Climbing   = _climbing,
			Powers     = _powers,
		};

	/// <summary>This node with every choice inside it folded where one can be.</summary>
	Node Folded(Node node, FollowSets.Continuation following, RecognitionGraph graph, RuleSymbol owner)
	{
		switch (node)
		{
			case Node.Choice(var alternatives):
			{
				var inner = Each(alternatives, following, graph, owner, sequence: false) ?? alternatives;

				// An alternative that only hands on a call is replaced by the body it would
				// have called, but only where the fold then takes: on its own it duplicates
				// a body and saves nothing, so an inline that does not lead to a shared
				// operand is put back.
				if (Inlined(new Node.Choice(inner), owner) is Node.Choice(var opened) &&
					Share(opened, following, graph, owner) is var wider &&
					(wider is not Node.Choice(var kept) || kept.Count < opened.Count))
					return Instead(node, wider);

				return Instead(node, Share(inner, following, graph, owner));
			}

			case Node.Sequence(var parts):
			{
				var inner = Each(parts, following, graph, owner);

				return inner is null ? node : Instead(node, new Node.Sequence(inner));
			}

			case Node.Capture(var name, var body):
			{
				var outer = _insideNamed;

				_insideNamed = outer || _guardNamed is null || _guardNamed.Contains(name);

				var inner = Folded(body, following, graph, owner);

				_insideNamed = outer;

				return ReferenceEquals(inner, body) ? node : Instead(node, new Node.Capture(name, inner));
			}

			case Node.Construct(var body, var how):
			{
				var inner = Folded(body, following, graph, owner);

				return ReferenceEquals(inner, body) ? node : Instead(node, new Node.Construct(inner, how));
			}

			case Node.Atomic(var body):
			{
				var inner = Folded(body, following, graph, owner);

				return ReferenceEquals(inner, body) ? node : Instead(node, new Node.Atomic(inner));
			}

			case Node.Repeat(var body, var min, var max):
			{
				// A turn is followed by another turn or by what follows the repetition.
				var inner = Folded(
					body,
					following.Or(new FollowSets.Continuation(
						FirstSets.Of(body, graph), FirstSets.Of(body, graph))),
					graph,
					owner);

				return ReferenceEquals(inner, body) ? node : Instead(node, new Node.Repeat(inner, min, max));
			}

			// Left alone rather than walked into. A mark stands over an extent and a
			// lookahead is rewound; both are shapes this first cut does not move.
			default: return node;
		}
	}

	/// <summary>
	/// One node in place of another, with everything recorded against the first handed to
	/// the second.
	/// </summary>
	/// <remarks>
	/// Which is what <see cref="Carry"/> is for, and why this pass may run over a rule whose
	/// shape something else names by node identity: a binding power, a fold's loop, a
	/// recovery. Rebuilding a node without this is how those come to name nothing.
	/// </remarks>
	Node Instead(Node from, Node to)
	{
		Carry(from, to);

		return to;
	}

	/// <summary>
	/// The alternatives with every run of them that shares a determinate leading operand
	/// replaced by one alternative that reads it once.
	/// </summary>
	Node Share(IReadOnlyList<Node> alternatives, FollowSets.Continuation following, RecognitionGraph graph, RuleSymbol owner)
	{
		List<Node>? folded = null;

		for (var at = 0; at < alternatives.Count; at++)
		{
			var last = Run(alternatives, at, owner);

			if (last > at)
			{
				if (Sharing(alternatives, at, last, following, graph, owner) is { } one)
				{
					folded ??= [.. alternatives.Take(at)];
					folded.Add(one);
					at = last;

					continue;
				}

				Declined(last - at + 1, owner);
			}

			folded?.Add(alternatives[at]);
		}

		if (folded is null)
			return new Node.Choice(alternatives);

		return folded.Count == 1 ? folded[0] : new Node.Choice(folded);
	}

	/// <summary>
	/// Says that a run of alternatives sharing an operand was found and not shared.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Said here rather than by a check of its own, and that is the whole of the point.
	/// A check has to guess at what this pass will do — it asked <c>Doors</c> where this
	/// asks <c>Determinism</c>, which are not the same question — so it spoke where the
	/// operand was shared anyway and stayed silent where it was not. Here there is nothing
	/// to guess: the run was found, the proof was asked for, and the answer was no.
	/// </para>
	/// <para>
	/// Once per rule. A rule whose alternatives share an operand usually has one place
	/// where they do, and a body walked to a fixed point would otherwise say it again on
	/// every round.
	/// </para>
	/// </remarks>
	void Declined(int run, RuleSymbol owner)
	{
		if (owner.Declaration is null || !_declined.Add(owner))
			return;

		Warn(
			SharedPrefix,
			$"{run} alternatives of '{owner.Name}' begin with the same operand, and ordered choice " +
			$"reads it once for each of them — {run} readings where one would do. It is not shared " +
			"for you because it is not shown to have one reading where it stands: two alternatives " +
			"prefer a shorter reading of it that lets their own tail fit, and one shared reading " +
			"prefers its own, so sharing it would be a different grammar rather than the same one " +
			"written once. Saying the operand is read once — braces around the lexeme it is, §4.5 — " +
			"is what lets it be shared; writing the alternatives with the shared operand in front " +
			"and the rest of each behind it is the same choice made by hand.",
			owner.Declaration.At);
	}

	readonly HashSet<RuleSymbol> _declined = [];

	/// <summary>How far a run of alternatives sharing one leading operand reaches.</summary>
	int Run(IReadOnlyList<Node> alternatives, int from, RuleSymbol owner)
	{
		if (Spoken(alternatives[from], owner) ||
			!Splits(alternatives[from], out _, out var head, out _) ||
			!Movable(head))
			return from;

		var last = from;

		for (var at = from + 1; at < alternatives.Count; at++)
		{
			if (Spoken(alternatives[at], owner) ||
				!Splits(alternatives[at], out _, out var other, out _) ||
				!Movable(other) ||
				!SameShape(head, other) ||
				!Renamable(alternatives[at], Named(head)))
				break;

			last = at;
		}

		return last;
	}

	/// <summary>
	/// The one alternative a run becomes, or null where the shared operand is not shown to
	/// have a single reading and the fold would therefore be visible.
	/// </summary>
	Node? Sharing(
		IReadOnlyList<Node> alternatives, int from, int last, FollowSets.Continuation following,
		RecognitionGraph graph, RuleSymbol owner)
	{
		var tails = new List<Node>(last - from + 1);
		var after = FollowSets.Continuation.None;
		var named = Named(Head(alternatives[from]));

		for (var at = from; at <= last; at++)
		{
			Splits(alternatives[at], out var how, out var mine, out var tail);

			var rest = tail ?? new Node.Empty();

			// Its own name is about to stop existing, and what it did with it was hand it
			// back, so it hands back the one that survives instead.
			if (!string.Equals(Named(mine), named, StringComparison.Ordinal))
				how = new Construction.Expression("(" + named + ")");

			// What follows the shared operand in this alternative is this tail, and past it
			// whatever follows the choice, where the tail can match nothing.
			after = after.Or(FollowSets.Precedes(rest, following, graph, FollowSets.SeamOf(owner, graph)));
			tails.Add(how is null ? rest : new Node.Construct(rest, how));
		}

		Splits(alternatives[from], out _, out var head, out _);

		// The whole condition. Reading it once instead of once per alternative is the same
		// reading only where there was one reading to begin with.
		if (!Determinism.Of(head, after, graph, FollowSets.SeamOf(owner, graph)))
			return null;

		return new Node.Sequence(
			[head, Committed(new Node.Choice(tails), following, graph, owner)]);
	}

	/// <summary>
	/// The residue of a fold, committed where coming back to it could never change the
	/// parse — wrapped in an atomic group, so the first tail to succeed is final.
	/// </summary>
	/// <remarks>
	/// <para>
	/// What the fold leaves behind is a choice among tails, and the engine keeps every
	/// untried tail alive in case a later failure means another should have been taken.
	/// Where the tails read input that is the machinery working; where they read none it
	/// is a trap. A guarded pair over one operand — <c>t: Dec &amp; when @(fits) =&gt;
	/// int | t: Dec =&gt; long</c> — folds into tails that consume nothing, so resuming
	/// the second can only end where the first did and rerun everything after it toward
	/// the same failure. Every such site multiplies a refusal by two, and a nest of them
	/// made refusing exponential where accepting was linear.
	/// </para>
	/// <para>
	/// Three things have to hold, and each guards a way the commit could be seen.
	/// <b>The tails must read nothing</b> — guards, factories, and at most the woven
	/// trivia, which every tail of a spaced rule leads with. <b>Positions must
	/// converge</b>: a tail that read the trivia ends past it and the bare tail does not,
	/// so the trivia must have one reading (atomic or empty) and nothing that can follow
	/// this choice may begin with a character the trivia begins with, unless it begins by
	/// reading the same trivia itself — which is what <see
	/// cref="FollowSets.Continuation.AfterSeam"/> holds, escalated to "anything" exactly
	/// where a continuation crosses into another namespace or could open mid-span.
	/// <b>The choice of factory must not be readable back</b>: a <c>when</c> elsewhere
	/// that materializes a value built over this rule would see which tail ran, so a rule
	/// any guard-named capture can reach is left uncommitted, and so is a residue folded
	/// inside a capture the rule's own guards name.
	/// </para>
	/// <para>
	/// What a commit forgoes beyond the rereading is the rerunning of downstream guards
	/// on those doomed retries — fewer spurious <c>context</c> mutations, not more.
	/// </para>
	/// </remarks>
	Node Committed(
		Node.Choice residue, FollowSets.Continuation following, RecognitionGraph graph,
		RuleSymbol owner)
	{
		if (_insideNamed)
			return residue;

		var seam     = FollowSets.SeamOf(owner, graph);
		var seamSeen = false;

		foreach (var tail in residue.Nodes)
			if (!Ethereal(tail, seam, ref seamSeen))
				return residue;

		if (seamSeen)
		{
			if (seam is null || !graph.Bodies.TryGetValue(seam, out var trivia) ||
				trivia is not (Node.Atomic or Node.Empty) ||
				following.AfterSeam.Overlaps(FirstSets.Of(trivia, graph)))
				return residue;
		}

		if (Observed().Contains(owner))
			return residue;

		return new Node.Atomic(residue);
	}

	/// <summary>
	/// Whether a tail reads nothing: guards, factories, and the rule's own woven trivia,
	/// in any arrangement — and nothing else.
	/// </summary>
	static bool Ethereal(Node node, RuleSymbol? seam, ref bool seamSeen)
	{
		switch (node)
		{
			case Node.Empty or Node.Guard:
				return true;

			case Node.Call(var called, { Count: 0 }) when
				seam is not null && ReferenceEquals(called, seam):
			{
				seamSeen = true;

				return true;
			}

			case Node.Construct(var body, _):
				return Ethereal(body, seam, ref seamSeen);

			case Node.Sequence(var parts):
			{
				foreach (var part in parts)
					if (!Ethereal(part, seam, ref seamSeen))
						return false;

				return true;
			}

			default:
				return false;
		}
	}

	/// <summary>
	/// The rules whose values some guard can read back: everything reachable through
	/// calls from a capture a guard names. Materializing such a capture replays the
	/// factories of everything inside it, so which factory a committed residue chose
	/// would be visible there.
	/// </summary>
	/// <remarks>
	/// Which captures a guard names is the scanner's answer; without one, every capture
	/// of a rule that has guards is assumed named, which refuses the commit rather than
	/// risks it.
	/// </remarks>
	HashSet<RuleSymbol> Observed()
	{
		if (_observed is not null)
			return _observed;

		_observed = [];

		foreach (var body in _bodies.Values)
		{
			var names = GuardNamed(body);

			if (names is { Count: 0 })
				continue;

			foreach (var node in NodeWalk.Descendants(body))
				if (node is Node.Capture(var name, var captured) &&
					(names is null || names.Contains(name)))
					Reach(captured);
		}

		return _observed;

		void Reach(Node from)
		{
			var pending = new Stack<Node>();

			pending.Push(from);

			while (pending.Count > 0)
				foreach (var node in NodeWalk.Descendants(pending.Pop()))
					if (node is Node.Call(var called, _) && _observed.Add(called) &&
						_bodies.TryGetValue(called, out var next))
						pending.Push(next);
		}
	}

	HashSet<RuleSymbol>? _observed;

	/// <summary>
	/// The capture names a body's guards use. Empty for a body with no guards; null —
	/// every name — where a guard's C# could not be asked.
	/// </summary>
	HashSet<string>? GuardNamed(Node body)
	{
		HashSet<string>? names = [];

		foreach (var node in NodeWalk.Descendants(body))
		{
			if (node is not Node.Guard(var text))
				continue;

			if (_scanner?.FreeNames(text) is { } free)
				names?.UnionWith(free);
			else
				names = null;
		}

		return names;
	}

	/// <summary>The current rule's <see cref="GuardNamed"/>, for the walk to consult.</summary>
	HashSet<string>? _guardNamed = [];

	/// <summary>Whether the walk stands inside a capture one of those guards names.</summary>
	bool _insideNamed;

	/// <summary>
	/// Whether something outside this pass names this node, and would be talking about
	/// nothing if two alternatives became one.
	/// </summary>
	/// <remarks>
	/// A rebuild is a bookkeeping problem and <see cref="Instead"/> answers it. This is the
	/// other kind: an alternative of a climbing rule carries a binding power, and a step of a
	/// left-recursive fold carries the name of the accumulator it takes — facts about *that*
	/// alternative, which folding a run of them into one would not move but destroy. So those
	/// are left where they are, and the rest of the rule is still walked.
	/// </remarks>
	bool Spoken(Node alternative, RuleSymbol owner) =>
		_climbing.TryGetValue(owner, out var levels) && levels.ContainsKey(alternative) ||
		_folds.TryGetValue(owner, out var fold) &&
			(fold.Accumulators.ContainsKey(alternative) || ReferenceEquals(fold.Loop, alternative));

	/// <summary>An alternative as its leading operand and what comes after it.</summary>
	static bool Splits(Node alternative, out Construction? how, out Node head, out Node? tail)
	{
		how = null;

		var inner = alternative;

		if (alternative is Node.Construct(var built, var construction))
		{
			how   = construction;
			inner = built;
		}

		if (inner is Node.Sequence(var parts))
		{
			if (parts.Count == 0)
			{
				head = inner;
				tail = null;

				return false;
			}

			head = parts[0];
			tail = parts.Count switch
			{
				1 => null,
				2 => parts[1],
				_ => new Node.Sequence([.. parts.Skip(1)]),
			};

			return true;
		}

		head = inner;
		tail = null;

		return true;
	}

	/// <summary>The name the operand is captured under, or the empty string for none.</summary>
	static string Named(Node head) => head is Node.Capture(var name, _) ? name : "";

	/// <summary>
	/// Whether an alternative can live with the run's operand being called something else.
	/// </summary>
	/// <remarks>
	/// <para>
	/// One operand survives a fold and the rest are dropped, so the name the survivor is
	/// captured under is the name everything in the run will see. An alternative that already
	/// uses that name has nothing to do. One that uses another has to be rewritten, and this
	/// pass rewrites exactly the case it can be sure of: an alternative that does nothing with
	/// the operand but hand it straight back, whose whole `=&gt;` is that one name.
	/// </para>
	/// <para>
	/// Anything else names its own capture inside C# the author wrote — <c>@(f(b))</c> — and
	/// renaming it would mean editing that text. Declined rather than attempted: an author's
	/// expression is not this pass's to rewrite.
	/// </para>
	/// </remarks>
	bool Renamable(Node alternative, string name) =>
		string.Equals(Named(Head(alternative)), name, StringComparison.Ordinal) ||
		HandsBack(alternative);

	static Node Head(Node alternative)
	{
		Splits(alternative, out _, out var head, out _);

		return head;
	}

	/// <summary>Whether an alternative is the operand and a `=&gt;` that hands it back.</summary>
	static bool HandsBack(Node alternative) =>
		alternative is Node.Construct(Node.Capture(var name, _), Construction.Expression(var text, _)) &&
		string.Equals(Handed(text), name, StringComparison.Ordinal);

	/// <summary>
	/// Whether an operand may be the one that survives, which is to say whether dropping the
	/// copies of it loses nothing.
	/// </summary>
	/// <remarks>
	/// A capture of the whole operand is kept by keeping the operand. Anything written down
	/// inside it — another capture, a construction, a mark, a guard — belongs to the copies
	/// being dropped as much as to the one being kept, and this does not move those.
	/// </remarks>
	static bool Movable(Node head)
	{
		var core = head is Node.Capture(_, var body) ? body : head;

		if (core is Node.Marked)
			return false;

		foreach (var node in NodeWalk.Descendants(core))
			if (node is Node.Capture or Node.Construct or Node.Marked or Node.Guard)
				return false;

		return true;
	}

	/// <summary>
	/// Each of them, folded, threading what follows — or null where none of them changed.
	/// </summary>
	IReadOnlyList<Node>? Each(
		IReadOnlyList<Node> nodes, FollowSets.Continuation following, RecognitionGraph graph, RuleSymbol owner,
		bool sequence = true)
	{
		List<Node>? rewritten = null;
		var         after     = following;

		for (var at = nodes.Count - 1; at >= 0; at--)
		{
			var one = Folded(nodes[at], after, graph, owner);

			if (!ReferenceEquals(one, nodes[at]))
			{
				rewritten ??= [.. nodes];
				rewritten[at] = one;
			}

			if (sequence)
				after = FollowSets.Precedes(nodes[at], after, graph, FollowSets.SeamOf(owner, graph));
		}

		return rewritten;
	}
}
