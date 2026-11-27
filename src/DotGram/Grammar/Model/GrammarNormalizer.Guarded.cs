using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// A rule that comes back to itself past a prefix that may succeed without moving — a recognizer of the host, and
/// whatever stands with it — read as the left recursion it is without the prefix, wherever the prefix does not move
/// at the rule's entry.
/// </summary>
/// <remarks>
/// <para>
/// <c>Sum = Num | @Ws &amp; Sum &amp; '+' &amp; Num</c>. Where <c>Ws</c> moves, the call back is an ordinary call
/// from a later position. Where it stands, it is the same rule at the same position — left recursion, which §4.3
/// reads as a loop over the tails and which the recognizer only hid. A recognizer's answer depends only on the text
/// and the position (§7.2), so whether the prefix stands is settled once at the entry, and the rule becomes a choice
/// between two rules it is the name of:
/// </para>
/// <code>
/// Sum       = stands(@Ws) &amp; Sum_Left | !stands(@Ws) &amp; Sum_Plain
/// Sum_Left  = Num | Sum_Left &amp; '+' &amp; Num            (the twin: folded as §4.3 folds it)
/// Sum_Plain = Num | @Ws &amp; Sum &amp; '+' &amp; Num        (the rule as written, @Ws known to move)
/// </code>
/// <para>
/// The prefix is everything read before the call back: recognizers, lookaheads, the seam of trivia §4.5 weaves after
/// them, and rules that read nothing but those. It stands where its first reading ends where it began; that is asked
/// once, at the entry, of a test that reads nothing (<see cref="StandsTest"/>). Where the recursive alternatives
/// have different prefixes, each is asked once and the rule is split once for every set of them that may stand
/// together.
/// </para>
/// <para>
/// A rule is taken only if its twin — the rule with the standing prefixes taken off — is a left recursion §4.3
/// folds (<see cref="TwinRefusal"/>). Where the twin would be refused, the rule is read as written and the build says
/// why in the twin's terms (GRAM4066). Where what is read depends on more than the text and the position — a
/// <c>when</c> or a <c>switch</c> on the way or in a caller, a <c>context</c> — the rule is read as written, as it
/// always was (GRAM5021).
/// </para>
/// <para>
/// The value is the twin's. Built by a <c>=&gt;</c> on every alternative, the parts are folded as §4.3 folds any rule
/// and the dispatch hands their value on. Built by none, a left recursion is the record of what its bases captured
/// (§4.3), and a record is not split into rules, which would each make a type of their own from their own captures:
/// its twin is folded in place, <c>E = stands(@Ws) &amp; Bases &amp; Steps* | !stands(@Ws) &amp; (E as written)</c>,
/// and the type is made from the rule's captures as it always was. A declared type built by no <c>=&gt;</c>, or a
/// record whose steps capture, has a twin with no value, and is read as written (GRAM5021).
/// </para>
/// </remarks>
sealed partial class GrammarNormalizer
{
	/// <summary>A rule comes back to itself past a prefix that may stand, and its twin is a left recursion §4.3 refuses.</summary>
	public const string StandingRefused = "GRAM4066";

	/// <summary>The most different prefixes a rule is split for: each doubles the rules it is split into.</summary>
	const int MostPrefixes = 3;

	/// <summary>The rules a split made that are folded as left recursion: their loop is <see cref="StandingLoop"/>.</summary>
	readonly HashSet<RuleSymbol> _standing = [];

	/// <summary>
	/// The loop a rule made by the split reads its tails in: the one place the order of a standing cycle is chosen.
	/// </summary>
	/// <remarks>
	/// As many tails as what follows accepts, given back one at a time from the end — the order §4.3's own loop
	/// reads every left recursion in, so a rule past a prefix that stands answers what its twin answers. Reading the
	/// fewest first instead would be a lazy repetition here and nothing else.
	/// </remarks>
	static Node.Repeat StandingLoop(Node tails)
	{
		return new Node.Repeat(tails, 0, null);
	}

	/// <summary>Whether nullability is worked out for the bodies as they stand: once a grammar asks, and again after a split.</summary>
	bool _standingNullable;

	/// <summary>The rules that may match nothing where every recognizer of the host may: worked out once a pass, and again after a split.</summary>
	readonly Dictionary<RuleSymbol, bool> _mayStand = [];

	/// <summary>The rules that read a recognizer of the host, themselves or through the rules they call.</summary>
	readonly HashSet<RuleSymbol> _hosted = [];

	/// <summary>One alternative of a rule that calls itself: where the call is, and what is read before it.</summary>
	/// <remarks>
	/// <see cref="Through"/> names the rule the call back goes through where it is not to the rule itself: a forwarder
	/// §4.3 makes direct (<see cref="Forwards"/>), or a rule that does something of its own, which it refuses.
	/// </remarks>
	sealed record Recursive(int Index, int Head, IReadOnlyList<Node> Operands, Node.Construct? Built, string? Key)
	{
		public RuleSymbol? Through { get; init; }

		public bool Forwards { get; init; }
	}

	void RewriteGuardedLeftRecursion()
	{
		if (Reentry.Unchecked || !_bodies.Values.Any(static body => NodeWalk.Descendants(body).Any(static node => node is Node.External)))
			return;

		Standing();

		var callers = Callers();

		foreach (var rule in _rules.ToList())
			if (_bodies.TryGetValue(rule, out var body))
				Guard(rule, body, callers);
	}

	/// <summary>Which rules may stand where recognizers may, and which read one: for the bodies as they stand now.</summary>
	void Standing()
	{
		_mayStand.Clear();
		_hosted.Clear();

		foreach (var rule in _bodies.Keys)
			_mayStand[rule] = false;

		for (var changed = true; changed;)
		{
			changed = false;

			foreach (var pair in _bodies)
			{
				var (rule, body) = (pair.Key, pair.Value);

				if (!_mayStand[rule] && FirstSets.Nullable(body, one => _mayStand.TryGetValue(one, out var may) && may, _boundBodies, hosts: true))
					changed = _mayStand[rule] = true;

				if (!_hosted.Contains(rule) &&
					NodeWalk.Descendants(body).Any(node => node is Node.External || node is Node.Call(var called, _) && _hosted.Contains(called)))
					changed = _hosted.Add(rule);
			}
		}
	}

	/// <summary>For each rule, the rules that call it.</summary>
	Dictionary<RuleSymbol, HashSet<RuleSymbol>> Callers()
	{
		var callers = new Dictionary<RuleSymbol, HashSet<RuleSymbol>>();

		foreach (var pair in _bodies)
			foreach (var node in NodeWalk.Descendants(pair.Value))
				if (node is Node.Call(var callee, _))
				{
					if (!callers.TryGetValue(callee, out var set))
						callers[callee] = set = [];

					set.Add(pair.Key);
				}

		return callers;
	}

	/// <summary>The rules that may reach <paramref name="rule"/> by calls, the rule itself among them.</summary>
	static HashSet<RuleSymbol> Reaching(RuleSymbol rule, Dictionary<RuleSymbol, HashSet<RuleSymbol>> callers)
	{
		var found   = new HashSet<RuleSymbol> { rule };
		var pending = new Stack<RuleSymbol>();

		pending.Push(rule);

		while (pending.Count > 0)
			if (callers.TryGetValue(pending.Pop(), out var next))
				foreach (var one in next)
					if (found.Add(one))
						pending.Push(one);

		return found;
	}

	void Guard(RuleSymbol rule, Node body, Dictionary<RuleSymbol, HashSet<RuleSymbol>> callers)
	{
		if (rule.Declaration is not { } declaration || declaration.Params.Count > 0 || body is Node.Choice { Selection: not null })
			return;

		var alternatives = Alternatives(body);
		var recursive    = new List<Recursive>();

		for (var i = 0; i < alternatives.Count; i++)
			if (Recursion(rule, i, alternatives[i]) is { } found)
				recursive.Add(found);

		var keys = recursive.Where(static one => one.Key is not null).Select(static one => one.Key!).Distinct().ToList();

		if (keys.Count == 0)
			return;

		if (!_standingNullable)
		{
			ComputeNullability();

			_standingNullable = true;
		}

		// Something read after the call back on at least one of them: where nothing is, the cut at once already reads
		// the rule (Reentry).
		if (recursive.All(static one => one.Key is null || one.Operands.Count == one.Head + 1))
			return;

		var prefixes = keys.Select(key => recursive.First(one => one.Key == key)).Select(static one => one.Operands.Take(one.Head).ToList()).ToList();
		var shown    = Shown(prefixes);

		// What is read depends on more than the text and the position: a value guard on the way or in a caller, or the
		// context. Read as written, as it always was, and told so by GRAM5021.
		var reaching = Reaching(rule, callers);

		if (_context is not null || _model.Context is not null)
			return;

		foreach (var caller in reaching)
			if (_bodies.TryGetValue(caller, out var read) &&
				NodeWalk.Descendants(read).Any(static node => node is Node.Guard or Node.Condition or Node.Choice { Selection: not null }))
				return;

		// What the rule reads keeps its facts by node, and is not copied into several rules here.
		foreach (var node in NodeWalk.Descendants(body))
			if (node is Node.Bind or Node.Same or Node.Counted || node is Node.Repeat && _recoveries.ContainsKey(node))
				return;

		// A value as the twin has it: built by every alternative, or by none. Built by some and not the rest, the rule is
		// refused as written (GRAM4008), and it is not rewritten here.
		var builds = alternatives.All(static one => one is Node.Construct);

		if (builds ? !_types.ContainsKey(rule) : NodeWalk.Descendants(body).Any(static node => node is Node.Construct))
			return;

		// Built by none, and captures: the record of what its bases captured, the type made from its captures (§4.1 case 3).
		var record = !builds && !_types.ContainsKey(rule) && NodeWalk.Descendants(body).Any(static node => node is Node.Capture);

		// Built by none, a left recursion is only that record, and its steps only extend what the bases matched (§4.3).
		// A declared type then has nothing to build it from, and a capture in a step — the call back's own, or one after
		// it — nowhere to go: the twin has no value. Read as written, as it always was, and told so by GRAM5021.
		if (!builds && (_types.ContainsKey(rule) || recursive.Any(StepCaptures)))
			return;

		if (TwinRefusal(rule, alternatives, recursive) is { } refusal)
		{
			Told(new GramDiagnostic(
				StandingRefused,
				$"'{rule.Name}' comes back to itself past {shown}, which may succeed without moving (docs/syntax.md §7.2). " +
				$"Where it does not move, this is left recursion the rewrite cannot take: {refusal} (§4.3). The rule is " +
				"read as it is written there, and a parse that has to give up on that alternative goes round until the " +
				$"stack or memory runs out. Make the rule without {shown} one §4.3 accepts, or let {shown} always move.",
				declaration.At.Position,
				declaration.At.Length,
				GramSeverity.Warning));

			return;
		}

		var tests = new List<Node>();

		foreach (var prefix in prefixes)
			if (StandsTest(new Node.Sequence(prefix), []) is { } test)
				tests.Add(test);
			else
				return;

		Split(rule, alternatives, recursive, keys, tests, builds, record);

		Told(new GramDiagnostic(
			Reentry.Reenters,
			$"'{rule.Name}' comes back to itself past {shown}, which may succeed without moving (docs/syntax.md §7.2). " +
			$"Where it does not move, '{rule.Name}' is read as the left recursion it is without {shown} (§4.3): as many " +
			"steps as what follows accepts, given back one at a time from the end. Where it moves, the rule is read as " +
			"it is written.",
			declaration.At.Position,
			declaration.At.Length,
			GramSeverity.Info));
	}

	/// <summary>Said once: a rule and the copies a specialization made of it say the same thing at the same place.</summary>
	void Told(GramDiagnostic diagnostic)
	{
		if (!_diagnostics.Contains(diagnostic))
			_diagnostics.Add(diagnostic);
	}

	/// <summary>
	/// Whether an alternative calls <paramref name="rule"/> on the left, either at once or past a prefix that may
	/// stand; the prefix's key tells prefixes apart, and is null for a call at once.
	/// </summary>
	Recursive? Recursion(RuleSymbol rule, int index, Node alternative)
	{
		var built = alternative as Node.Construct;

		if ((built?.Body ?? alternative) is not Node.Sequence(var operands))
			return null;

		for (var at = 0; at < operands.Count; at++)
		{
			var operand = operands[at];
			var head    = operand is Node.Capture(_, var captured) ? captured : operand;

			if (head is Node.Call(var called, var arguments) && ReferenceEquals(called, rule))
			{
				if (arguments.Count > 0)
					return null;

				if (at == 0)
					return new Recursive(index, 0, operands, built, null);

				var prefix = operands.Take(at).ToList();

				return Prefixed(prefix) ? new Recursive(index, at, operands, built, string.Join(" & ", prefix)) : null;
			}

			if (PrefixPart(operand, rule, []))
				continue;

			// The first thing that is no part of a prefix: past a prefix of recognizers, a call back through another
			// rule. Through one that only forwards what it calls, §4.3 makes it direct and the twin folds; through one
			// that does more, it does not, and the rule is told so.
			if (at == 0 || head is not Node.Call(var other, { Count: 0 }) || !Prefixed([.. operands.Take(at)]))
				return null;

			var key = string.Join(" & ", operands.Take(at));

			_forwarders ??= Forwarders();

			if (_forwarders.TryGetValue(other, out var sources) && sources.Contains(rule) &&
				(operand is not Node.Capture || other.Declaration?.Type is not null))
				return new Recursive(index, at, operands, built, key) { Through = other, Forwards = true };

			return LeftReaches(operand, rule) is not null ? new Recursive(index, at, operands, built, key) { Through = other } : null;
		}

		return null;
	}

	/// <summary>
	/// Whether the operands read before a call may be the prefix of a call back: a recognizer of the host among them, and
	/// nothing that does not stand or does more than read.
	/// </summary>
	bool Prefixed(IReadOnlyList<Node> prefix)
	{
		return Hosted(prefix) && prefix.All(part => PrefixPart(part, null, []));
	}

	/// <summary>
	/// Whether a node may be read before the call back as part of a prefix: it reads nothing but the text, records
	/// nothing, and may succeed without moving once recognizers are allowed to.
	/// </summary>
	bool PrefixPart(Node node, RuleSymbol? rule, HashSet<RuleSymbol> seen)
	{
		return MayStand(node) && Pure(node, rule, seen);
	}

	/// <summary>Whether a node reads only the text and does nothing else: no value, no binding, no hook, no call of <paramref name="rule"/>.</summary>
	bool Pure(Node node, RuleSymbol? rule, HashSet<RuleSymbol> seen)
	{
		// Follows calls, so a chain of them is as deep as the grammar makes it (DeepStack).
		if (DeepStack.Low)
			return DeepStack.Run(() => Pure(node, rule, seen));

		switch (node)
		{
			case Node.External { HasValue: false, UsesContext: false, Stands: null }:
			case Node.Empty or Node.Element or Node.Literal or Node.Glue or Node.Behind:
				return true;

			case Node.Lookahead or Node.Sequence or Node.Atomic:
			case Node.Choice { Selection: null }:
			case Node.Repeat repeat when !_recoveries.ContainsKey(repeat):
				return node.Children.All(child => Pure(child, rule, seen));

			case Node.Call(var called, var arguments) when arguments.Count == 0 && !ReferenceEquals(called, rule):
				return !seen.Add(called) || _bodies.TryGetValue(called, out var body) && !_types.ContainsKey(called) && Pure(body, rule, seen);

			default:
				return false;
		}
	}

	/// <summary>Whether a node may succeed without moving where every recognizer may: nullable with the hosts.</summary>
	bool MayStand(Node node)
	{
		return FirstSets.Nullable(node, one => _mayStand.TryGetValue(one, out var may) && may, _boundBodies, hosts: true);
	}

	/// <summary>Whether a prefix reads a recognizer of the host, itself or through a rule it calls.</summary>
	bool Hosted(IEnumerable<Node> prefix)
	{
		return prefix.Any(part => NodeWalk.Descendants(part).Any(node => node is Node.External || node is Node.Call(var called, _) && _hosted.Contains(called)));
	}

	/// <summary>
	/// A node that reads nothing and succeeds exactly where the first reading of <paramref name="node"/> ends where it
	/// began, or null where that cannot be asked of it.
	/// </summary>
	/// <remarks>
	/// A recognizer is asked from a copy of the position and compared with it (<see cref="Node.External.Stands"/>);
	/// a lookahead is asked as it is; a repetition's first reading is empty where its body does not match; a choice's
	/// is its first alternative's where that matches and the next one's where it does not; a sequence's, where each
	/// part's is. That last is the first reading only where no part has to give back for the next to match, which a
	/// prefix of recognizers, lookaheads and trivia does not need.
	/// </remarks>
	Node? StandsTest(Node node, HashSet<RuleSymbol> seen)
	{
		if (DeepStack.Low)
			return DeepStack.Run(() => StandsTest(node, seen));

		switch (node)
		{
			case Node.External external:
				return external with { Stands = true };

			case Node.Lookahead or Node.Behind or Node.Glue:
				return CloneAndRewrite(node, NoTargets, [], "Standing");

			case Node.Empty:
				return node;

			case Node.Atomic(var body):
				return StandsTest(body, seen);

			case Node.Sequence(var parts):
			{
				var tests = new List<Node>();

				foreach (var part in parts)
					if (StandsTest(part, seen) is { } test)
						tests.Add(test);
					else
						return null;

				return tests.Count == 1 ? tests[0] : new Node.Sequence(tests);
			}

			case Node.Choice(var alternatives):
			{
				var first = StandsTest(alternatives[0], seen);

				if (first is null || alternatives.Count == 1 || IsNullable(alternatives[0]) && Pure(alternatives[0], null, []) && !Hosted([alternatives[0]]))
					return first;

				var rest = StandsTest(alternatives.Count == 2 ? alternatives[1] : new Node.Choice([.. alternatives.Skip(1)]), seen);

				return rest is null
					? null
					: new Node.Choice([first, new Node.Sequence([new Node.Lookahead(false, CloneAndRewrite(alternatives[0], NoTargets, [], "Standing")), rest])]);
			}

			case Node.Repeat(var body, 0, _) when !MayStand(body):
				return new Node.Lookahead(false, CloneAndRewrite(body, NoTargets, [], "Standing"));

			case Node.Call(var called, _) when seen.Add(called) && _bodies.TryGetValue(called, out var body):
				return StandsTest(body, seen);

			default:
				return null;
		}
	}

	/// <summary>
	/// Why the twin — the rule with every prefix that may stand taken off — is not a left recursion §4.3 folds, in its
	/// words, or null where it is.
	/// </summary>
	string? TwinRefusal(RuleSymbol rule, IReadOnlyList<Node> alternatives, IReadOnlyList<Recursive> recursive)
	{
		var bases = alternatives.Where((_, i) => recursive.All(one => one.Index != i)).ToList();

		// Through a forwarder, each rule it forwards other than this one is a base of the twin, as §4.3 makes it.
		foreach (var one in recursive)
			if (one is { Forwards: true, Through: { } forwarder } && _forwarders is { } forwards && forwards.TryGetValue(forwarder, out var sources))
				foreach (var source in sources)
					if (!ReferenceEquals(source, rule))
						bases.Add(new Node.Sequence([new Node.Call(source, []), .. one.Operands.Skip(one.Head + 1)]));

		// The way back first: where the call back goes through a rule that does more than forward it, that rule is what
		// a reader can change, and "no alternative to start from" is only what is left of this one without it.
		if (recursive.FirstOrDefault(static one => one.Through is not null && !one.Forwards) is { Through: { } indirect })
			return $"the call back goes through '{indirect.Name}', which does more than forward what it calls";

		if (bases.Count == 0)
			return "it has no alternative to start from";

		if (alternatives.Any(one => BoundOn(one) is not null))
			return "it states strengths ('<<', '>>', §4.3.1), and a rule past a prefix is not climbed";

		var steps = recursive.Where(static one => one.Operands.Count > one.Head + 1).Select(static one => new Node.Sequence([.. one.Operands.Skip(one.Head + 1)])).ToList();

		foreach (var step in steps)
		{
			if (EndsWith(step, rule))
				return "an alternative is recursive on both sides";

			if (IsNullable(step))
				return "a step may read nothing";
		}

		// A base that reaches the rule again before it has read anything, a recognizer that may stand counted as reading
		// nothing: there the twin reaches itself on the left through another rule.
		foreach (var one in bases)
			if (LeftReaches(one, rule) is { } through)
				return $"the call back goes through '{through.Name}', which does more than forward what it calls";

		return null;
	}

	/// <summary>Whether a recursive alternative captures in its step: the call back itself, or what is read after it.</summary>
	static bool StepCaptures(Recursive one)
	{
		return one.Operands.Skip(one.Head).Any(static operand => NodeWalk.Descendants(operand).Any(static node => node is Node.Capture));
	}

	/// <summary>
	/// The rule through which a base reaches <paramref name="rule"/> before it has read anything, a recognizer that may
	/// stand counted as reading nothing, or null where it does not.
	/// </summary>
	RuleSymbol? LeftReaches(Node start, RuleSymbol rule)
	{
		var seen    = new HashSet<RuleSymbol>();
		var pending = new Stack<(Node Node, RuleSymbol? Through)>();

		pending.Push((start, null));

		while (pending.Count > 0)
		{
			var (node, through) = pending.Pop();

			switch (node)
			{
				case Node.Call(var called, _):
					if (ReferenceEquals(called, rule))
						return through ?? called;

					if (seen.Add(called) && _bodies.TryGetValue(called, out var body))
						pending.Push((body, through ?? called));

					break;

				case Node.Sequence(var nodes):
					foreach (var child in nodes)
					{
						pending.Push((child, through));

						if (!MayStand(child))
							break;
					}

					break;

				case Node.Choice or Node.Capture or Node.Bind or Node.Counted or Node.Construct or Node.Atomic or Node.Marked or Node.Repeat or Node.Lookahead:
					foreach (var child in node.Children)
						pending.Push((child, through));

					break;
			}
		}

		return null;
	}

	/// <summary>
	/// The rule split: one rule for each set of prefixes that may stand together at the entry, each the rule with those
	/// prefixes taken off, and the rule itself a dispatch on which of them stand.
	/// </summary>
	void Split(RuleSymbol rule, IReadOnlyList<Node> alternatives, IReadOnlyList<Recursive> recursive, IReadOnlyList<string> keys, IReadOnlyList<Node> tests, bool builds, bool record)
	{
		var sets     = 1 << keys.Count;
		var parts    = new RuleSymbol[sets];
		var inline   = new Node[sets];
		var forwards = _forwarders ??= Forwarders();

		// A record is not split into rules: each would be a type of its own, made from its own captures, and the rule's
		// would be made from none. Its twin is folded in place instead, which for a record is the whole of the fold — its
		// steps capture nothing, so there is nothing to hand from one to the next — and the type is made from the
		// captures of the rule as it always was.
		for (var set = 0; set < sets; set++)
			parts[set] = record
				? new RuleSymbol(rule.Name + "_Standing", rule.Namespace, rule.Declaration)
				: Made(rule, keys.Count == 1 ? set == 0 ? "_Plain" : "_Left" : $"_Stood{set}");

		for (var set = 0; set < sets; set++)
		{
			var made  = parts[set];
			var built = new List<Node>();

			for (var i = 0; i < alternatives.Count; i++)
			{
				var one = recursive.FirstOrDefault(each => each.Index == i);

				// A base, as it is: the rule that reads where nothing stands keeps the nodes the passes before know.
				if (one is null)
				{
					built.Add(set == 0 ? alternatives[i] : CloneAndRewrite(alternatives[i], NoTargets, [], "Standing"));

					continue;
				}

				var stood = one.Key is null || (set & (1 << IndexOf(keys, one.Key))) != 0;

				if (stood)
				{
					// A turn of the loop: the call back is this rule's own, at once. One with nothing after the call reads
					// only what the entry it came back to reads, and is left out.
					if (one.Operands.Count == one.Head + 1)
						continue;

					// Through a forwarder: the alternative as §4.3 makes it direct, one for each rule it forwards, the
					// call back to this rule's own.
					if (one.Through is not null)
					{
						var twin = Rebuilt(one, [.. one.Operands.Skip(one.Head).Select(node => CloneAndRewrite(node, NoTargets, [], "Standing"))]);

						foreach (var each in Distribute(rule, twin, forwards))
							built.Add(Retargeted(each, rule, made));

						continue;
					}

					var head = one.Operands[one.Head] is Node.Capture(var name, _)
						? (Node)new Node.Capture(name, new Node.Call(made, []))
						: new Node.Call(made, []);

					built.Add(Rebuilt(one, [head, .. one.Operands.Skip(one.Head + 1).Select(node => CloneAndRewrite(node, NoTargets, [], "Standing"))]));
				}
				else
				{
					// The prefix moves here: the alternative as written, its prefix read once, and known to move.
					var prefix = one.Operands.Take(one.Head).Select(node => MovingPrefix(node, [])).ToList();
					var read   = prefix.All(static node => node is Node.External or Node.Lookahead or Node.Empty)
						? prefix
						: [new Node.Atomic(prefix.Count == 1 ? prefix[0] : new Node.Sequence(prefix))];

					built.Add(Rebuilt(one, [.. read, .. one.Operands.Skip(one.Head).Select(node => CloneAndRewrite(node, NoTargets, [], "Standing"))]));
				}
			}

			if (record)
			{
				inline[set] = Folded(built, made);

				continue;
			}

			_bodies[made] = built.Count == 1 ? built[0] : new Node.Choice(built);
			_standing.Add(made);
		}

		// The bodies changed: what stands, what reads a recognizer, what forwards and what is nullable is asked again.
		_standingNullable = false;
		_forwarders       = null;

		Standing();

		var dispatch = new List<Node>();

		for (var set = sets - 1; set >= 0; set--)
		{
			var leads = new List<Node>();

			for (var key = 0; key < keys.Count; key++)
				leads.Add((set & (1 << key)) != 0 ? CloneAndRewrite(tests[key], NoTargets, [], "Standing") : NotStands(tests[key]));

			dispatch.Add(record ? new Node.Sequence([.. leads, inline[set]])
				: builds ? new Node.Construct(new Node.Sequence([.. leads, new Node.Capture("item0", new Node.Call(parts[set], []))]), Construction.Operand.Instance)
				: new Node.Sequence([.. leads, new Node.Call(parts[set], [])]));
		}

		_bodies[rule] = new Node.Choice(dispatch);

		static int IndexOf(IReadOnlyList<string> keys, string key)
		{
			for (var i = 0; i < keys.Count; i++)
				if (keys[i] == key)
					return i;

			return -1;
		}
	}

	/// <summary>
	/// The alternatives of a record's twin, folded as §4.3 folds a left recursion: the ones that begin with the call back
	/// to <paramref name="twin"/> are the steps of a loop after the rest.
	/// </summary>
	static Node Folded(IReadOnlyList<Node> alternatives, RuleSymbol twin)
	{
		var bases = new List<Node>();
		var steps = new List<Node>();

		foreach (var alternative in alternatives)
			if (Tail(alternative, twin) is var (step, _))
				steps.Add(step);
			else
				bases.Add(alternative);

		var start = bases.Count == 1 ? bases[0] : new Node.Choice(bases);

		return steps.Count == 0 ? start : new Node.Sequence([start, StandingLoop(steps.Count == 1 ? steps[0] : new Node.Choice(steps))]);
	}

	/// <summary>An alternative whose leading call is to <paramref name="rule"/>, that call made to <paramref name="made"/>.</summary>
	static Node Retargeted(Node alternative, RuleSymbol rule, RuleSymbol made)
	{
		var built = alternative as Node.Construct;

		if ((built?.Body ?? alternative) is not Node.Sequence(var operands))
			return alternative;

		Node head = operands[0] switch
		{
			Node.Capture(var name, Node.Call(var called, _)) when ReferenceEquals(called, rule) => new Node.Capture(name, new Node.Call(made, [])),
			Node.Call(var called, _) when ReferenceEquals(called, rule)                         => new Node.Call(made, []),
			var other                                                                           => other,
		};

		if (ReferenceEquals(head, operands[0]))
			return alternative;

		Node body = new Node.Sequence([head, .. operands.Skip(1)]);

		return built is null ? body : built with { Body = body };
	}

	/// <summary>The alternative a recursive one was, its operands replaced, its <c>=&gt;</c> kept.</summary>
	static Node Rebuilt(Recursive one, IReadOnlyList<Node> operands)
	{
		Node body = new Node.Sequence(operands);

		return one.Built is { } built ? new Node.Construct(body, built.How) : body;
	}

	/// <summary>The test that a prefix does not stand: a recognizer asked so, or the negation of the whole test.</summary>
	Node NotStands(Node test)
	{
		return test is Node.External { Stands: true } external
			? external with { Stands = false }
			: new Node.Lookahead(false, CloneAndRewrite(test, NoTargets, [], "Standing"));
	}

	/// <summary>
	/// A part of a prefix read where the prefix does not stand: its recognizers known to move, so that no analysis finds
	/// the call back on the left, and a rule that only wraps one read in place, so that its recognizers can be marked.
	/// </summary>
	Node MovingPrefix(Node node, HashSet<RuleSymbol> seen)
	{
		if (DeepStack.Low)
			return DeepStack.Run(() => MovingPrefix(node, seen));

		return node switch
		{
			Node.External external => external with { Moves = true },
			Node.Call(var called, _) when Hosted([node]) && seen.Add(called) && _bodies.TryGetValue(called, out var body) => MovingPrefix(body, seen),
			Node.Sequence(var nodes) => new Node.Sequence([.. nodes.Select(one => MovingPrefix(one, seen))]),
			Node.Choice choice       => choice.Rebuild([.. choice.Nodes.Select(one => MovingPrefix(one, seen))]),
			Node.Atomic(var body)    => new Node.Atomic(MovingPrefix(body, seen)),
			_                        => CloneAndRewrite(node, NoTargets, [], "Standing"),
		};
	}

	/// <summary>The prefixes as the author wrote them, for a message: the recognizers and the rules, without the seams.</summary>
	string Shown(IReadOnlyList<List<Node>> prefixes)
	{
		var names = new List<string>();

		foreach (var prefix in prefixes)
			foreach (var part in prefix)
			{
				var name = part switch
				{
					Node.External external                                       => "@" + external.Name,
					Node.Call(var called, _) when !_model.Trivia.Values.Contains(called) => called.Name,
					_                                                            => null,
				};

				if (name is not null && !names.Contains(name))
					names.Add(name);
			}

		return names.Count == 0 ? "a recognizer of the host" : string.Join(" and ", names.Select(static name => $"'{name}'"));
	}

	/// <summary>A rule made beside <paramref name="rule"/>, of its type and its trivia, named after it.</summary>
	RuleSymbol Made(RuleSymbol rule, string suffix)
	{
		var name = rule.Name + suffix;

		while (_rules.Exists(one => one.Namespace == rule.Namespace && one.Name == name))
			name += "_";

		var made = new RuleSymbol(name, rule.Namespace, rule.Declaration);

		_rules.Add(made);

		if (_types.TryGetValue(rule, out var type))
			_types[made] = type;

		if (_trivia.TryGetValue(rule, out var trivia))
			_trivia[made] = trivia;

		var origin = _lifted.TryGetValue(rule, out var outer) ? outer.Origin : rule;

		_lifted[made] = new Lift(rule, origin, Transparent: false);

		return made;
	}
}
