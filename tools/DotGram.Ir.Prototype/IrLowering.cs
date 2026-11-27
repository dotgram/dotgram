using System;
using System.Text.RegularExpressions;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Emit;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;
using DotGram.Grammar.Plan;

namespace DotGram.Ir.Prototype;

/// <summary>
/// A recognition graph taken to <see cref="IrProgram"/>: a function a rule, read by replay.
/// </summary>
/// <remarks>
/// <para>
/// Throw-away, and it lowers what the test grammars need: sets, texts, sequences, ordered
/// choices, repetitions with number or length bounds, looks, braces, calls, captures of a text
/// or of a value, bindings and references to them, guards, and constructions written as host
/// text, as a sequence or as one capture handed through. Everything else becomes an
/// <see cref="IrStmt.Unsupported"/>, which is counted and never run.
/// </para>
/// <para>
/// Whether a choice or a repetition has to stay open is asked of the generator's plan: the
/// decisions its readers take about writing a way (<see cref="WriteDecisions"/>), found by the
/// generator's own planning stage over the same graph. Where the plan has no answer for a node — a
/// rule only a flat machine reads, which has no readers, or a node read in places that disagree —
/// the lowering asks <see cref="Determinism"/> itself, and counts the miss
/// (<see cref="IrProgram.PlanMisses"/>). The values are built while reading, as the generator's
/// immediate carrier builds them.
/// </para>
/// </remarks>
sealed partial class IrLowering
{
	/// <param name="AllResumable">Every choice and every repetition stays open, whatever the proofs say.</param>
	/// <param name="OmitResets">
	/// A failed alternative or turn puts no slot back. Wrong on purpose: what the tests of the
	/// rollback contract run to show that the resets are what holds it.
	/// </param>
	/// <param name="KeepsPartsWhole">
	/// No turn and no alternative is read again in place (<see cref="Retrying"/>): a turn is read
	/// again over its own decisions only past the fewest of a repetition that writes a way, an
	/// alternative only if it is not the last. Wrong on purpose: what a closed repetition whose turn
	/// gives back inside answers otherwise with.
	/// </param>
	/// <param name="ReopensInstead">
	/// With <paramref name="KeepsPartsWhole"/>, a repetition the plan closes is opened where its turn can
	/// fail after a decision of its own (<see cref="Reopen"/>): the answers are right, the ways and the
	/// host calls are not the generated reader's.
	/// </param>
	/// <param name="AsRecorded">
	/// Each place that records a refusal records what the generator's plan says its readers record
	/// there (<see cref="RecordSites"/>): the set and how, a repetition's door, a choice told apart by the
	/// next item. Otherwise each terminal says itself and no door is shut.
	/// </param>
	public sealed record Options(bool AllResumable = false, bool OmitResets = false, bool KeepsPartsWhole = false, bool ReopensInstead = false, bool AsRecorded = false);

	readonly RecognitionGraph _graph;
	readonly Options          _options;
	readonly IrProgram        _program = new();

	readonly Dictionary<RuleSymbol, IrFunction>               _functions = [];
	readonly Dictionary<string, int>                          _expected  = new(StringComparer.Ordinal);
	readonly Dictionary<int, int>                             _fragmentIds = [];
	readonly IReadOnlyDictionary<RuleSymbol, FollowSets.Continuation> _follow;

	// The function being lowered.
	RuleSymbol                         _rule = null!;
	RuleSymbol?                        _seam;
	List<IrSlot>                       _slots = [];
	Dictionary<string, IrSlot>         _members = new(StringComparer.Ordinal);
	Dictionary<TextBinding, (IrSlot From, IrSlot To)> _bindings = [];
	HashSet<TextBinding>               _rebound = [];
	IrSlot?                            _result;
	bool                               _complete;

	/// <summary>The write decisions of every machine of the plan that has readers.</summary>
	readonly IReadOnlyList<WriteDecisions> _decisions;

	/// <summary>Where the readers of every machine of the plan stop building after a second give-back.</summary>
	readonly IReadOnlyList<DeferralPlan> _deferrals;

	/// <summary>What every place of the readers of every machine of the plan records; empty where the lowering does not ask.</summary>
	readonly IReadOnlyList<RecordSites> _sites;

	IrLowering(RecognitionGraph graph, Options options, IReadOnlyList<WriteDecisions> decisions, IReadOnlyList<DeferralPlan> deferrals, IReadOnlyList<RecordSites> sites)
	{
		_sites     = sites;
		_graph     = graph;
		_options   = options;
		_follow    = FollowSets.Of(graph);
		_decisions = decisions;
		_deferrals = deferrals;
	}

	/// <summary>The rules the publications reach, lowered, asking the generator's plan of the graph.</summary>
	public static IrProgram Lower(RecognitionGraph graph, Options? options = null)
	{
		var machines = PlannedMachines(graph);

		return Lower(graph, options, everyRule: false,
			[.. machines.Select(static machine => machine.Writes).OfType<WriteDecisions>()],
			[.. machines.Select(static machine => machine.Deferral).OfType<DeferralPlan>()],
			options is { AsRecorded: true } ? [.. machines.Select(static machine => machine.Sites).OfType<RecordSites>()] : []);
	}

	/// <param name="everyRule">Every rule of the graph that has a body, reached or not: what a measurement of the lowering asks.</param>
	/// <param name="decisions">What the plan decided about ways on the tape (<see cref="Planned"/>); empty asks every node of the lowering's own proofs.</param>
	/// <param name="deferrals">Where the plan's readers stop building after a second give-back; empty, nowhere.</param>
	/// <param name="sites">What the plan's readers record at each place (<see cref="Options.AsRecorded"/>); empty, each terminal says itself.</param>
	public static IrProgram Lower(
		RecognitionGraph graph, Options? options, bool everyRule, IReadOnlyList<WriteDecisions> decisions, IReadOnlyList<DeferralPlan>? deferrals = null,
		IReadOnlyList<RecordSites>? sites = null)
	{
		var lowering = new IrLowering(graph, options ?? new Options(), decisions, deferrals ?? [], sites ?? []);

		lowering.Run(everyRule);

		return lowering._program;
	}

	/// <summary>
	/// The generator's plan of the graph — its planning stage, run as it runs for a file with no
	/// options asked — and of it the write decisions of every machine that has readers.
	/// </summary>
	public static IReadOnlyList<WriteDecisions> Planned(RecognitionGraph graph)
	{
		return [.. PlannedMachines(graph).Select(static machine => machine.Writes).OfType<WriteDecisions>()];
	}

	/// <summary>The machines of the generator's plan of the graph.</summary>
	static IReadOnlyList<MachinePlan> PlannedMachines(RecognitionGraph graph)
	{
		var context = new EmitContext { Diagnostics = new List<GramDiagnostic>() };

		CSharpEmitter.Emit(graph, new GramCompilerOptions(), context);

		return [.. (context.Plan ?? throw new InvalidOperationException("The generator made no plan of the graph.")).Machines];
	}

	/// <summary>
	/// Whether a loop of a rule that asks for another reading stops building after its second
	/// give-back, as the readers that read the rule decide it; no where none of them read it or
	/// where they do not agree, a miss counted (<see cref="IrProgram.DeferralMisses"/>).
	/// </summary>
	bool DefersAt(RuleSymbol owner, RetrySite site)
	{
		bool? agreed = null;

		foreach (var deferral in _deferrals)
		{
			bool answer;

			try
			{
				answer = deferral.DefersOnGiveBack && deferral.Excluded(owner, site) is null;
			}
			catch (InvalidOperationException)
			{
				continue;
			}

			if (agreed is { } other && other != answer)
			{
				_program.DeferralMisses++;

				return false;
			}

			agreed = answer;
		}

		if (agreed is null && _deferrals.Count > 0)
			_program.DeferralMisses++;

		return agreed ?? false;
	}

	/// <summary>
	/// What the plan decided about writing a way at a node of the rule being lowered — a repetition,
	/// or a choice by its first alternative and how many there are — or null where no machine's
	/// readers decided it there alike; a miss, counted.
	/// </summary>
	bool? PlannedWay(Node node, int count)
	{
		_program.PlanAsked++;

		// The rule's own reader first, and where it has no answer — a rule the readers write in place
		// where it is called — any reader that reads the node, where they all agree.
		var found = Agreed(_rule) ?? Agreed(null);

		if (found is null)
		{
			_program.PlanMisses++;

			return null;
		}

		return found.WritesAWay;

		WriteDecision? Agreed(RuleSymbol? owner)
		{
			WriteDecision? agreed = null;

			foreach (var decisions in _decisions)
			{
				if (decisions.Agreed(owner, node, count) is not { } decision)
					continue;

				if (agreed is not null && agreed != decision)
					return null;

				agreed = decision;
			}

			return agreed;
		}
	}

	void Run(bool everyRule)
	{
		var rules = new List<RuleSymbol>();

		if (everyRule)
			rules.AddRange(_graph.Rules.Where(_graph.Bodies.ContainsKey));
		else
		{
			var reached = new HashSet<RuleSymbol>();

			foreach (var publication in _graph.Publications)
				reached.UnionWith(_graph.Reaches(publication.Rule));

			rules.AddRange(_graph.Rules.Where(rule => reached.Contains(rule) && _graph.Bodies.ContainsKey(rule)));
		}

		NumberFragments(rules);

		foreach (var rule in rules)
		{
			var function = new IrFunction(_program.Functions.Count, rule.Name);

			_functions.Add(rule, function);
			_program.Functions.Add(function);
		}

		foreach (var rule in rules)
			LowerRule(rule);

		Unread(rules);
		Settle();

		foreach (var publication in _graph.Publications)
		{
			if (publication.Kind != PublishKind.Parse || !_functions.TryGetValue(publication.Rule, out var read))
				continue;

			var entry = new IrFunction(_program.Functions.Count, "parse_" + publication.MethodName);
			var value = new IrSlot(0, "value", IrSlotKind.Value, null);
			var from  = new IrSlot(1, "from", IrSlotKind.Position, null);

			// A rule that builds nothing is worth the text it read, which here is the whole input.
			IrStmt body = new IrStmt.Scope([value, from], new IrStmt.Block(read.Result is null
				?
				[
					new IrStmt.Assign(from, IrExpr.Position.Instance),
					new IrStmt.Call(read, null, false),
					new IrStmt.MatchEnd(Expect("end of input")),
					new IrStmt.Assign(value, new IrExpr.Text(from)),
				]
				: [new IrStmt.Call(read, value, false), new IrStmt.MatchEnd(Expect("end of input"))]));

			entry.Body      = read.Decides ? new IrStmt.Replay(body) { Defers = DefersAt(publication.Rule, RetrySite.Entry) } : body;
			entry.Result    = value;
			entry.SlotCount = 2;
			entry.Decides   = false;
			entry.Complete  = read.Complete;

			_program.Functions.Add(entry);
			_program.Entries[publication.MethodName] = entry;
		}

		foreach (var function in _program.Functions)
			_program.NodeCount += Count(function.Body);
	}

	static int Count(IrStmt stmt)
	{
		var count = 1 + stmt switch
		{
			IrStmt.Assign                => 1,
			IrStmt.Append                => 1,
			IrStmt.Guard guard           => guard.Arguments.Count,
			IrStmt.Construct build       => build.Arguments.Count,
			IrStmt.Repeat repeat         => repeat.Max is null ? 1 : 2,
			_                            => 0,
		};

		foreach (var child in stmt.Children)
			count += Count(child);

		return count;
	}

	/// <summary>
	/// A fragment's number is the rank of where it was written, so that a copy of a rule made by
	/// normalization calls the fragment its original calls.
	/// </summary>
	void NumberFragments(List<RuleSymbol> rules)
	{
		var places = new SortedSet<int>();

		foreach (var rule in rules)
		{
			foreach (var node in NodeWalk.Descendants(_graph.Bodies[rule]))
			{
				if (node is Node.Guard { At: >= 0 } guard)
					places.Add(guard.At);
				else if (node is Node.Construct(_, Construction.Expression { At: >= 0 } expression) && PassedThrough(expression.Text) is null)
					places.Add(expression.At);
			}
		}

		foreach (var place in places)
			_fragmentIds.Add(place, _fragmentIds.Count);
	}

	void LowerRule(RuleSymbol rule)
	{
		var function = _functions[rule];

		_rule     = rule;
		_seam     = FollowSets.SeamOf(rule, _graph);
		_slots    = [];
		_members  = new(StringComparer.Ordinal);
		_bindings = [];
		_rebound  = [];
		_result   = null;
		_complete = true;

		var body = _graph.Bodies[rule];

		if (_graph.Folds.ContainsKey(rule) || _graph.Climbing.ContainsKey(rule) || _graph.Lifted.ContainsKey(rule) || _graph.Externals.ContainsKey(rule))
		{
			function.Body     = Unsupported(body, "a rule with a fold, binding powers, a lift or an external");
			function.Complete = false;

			foreach (var _ in NodeWalk.Descendants(body))
				_program.GraphNodes++;

			return;
		}

		foreach (var node in NodeWalk.Descendants(body))
			if (node is Node.Repeat { Max: not 1 } or Node.Counted)
				foreach (var inner in NodeWalk.Descendants(node))
					if (inner is Node.Bind bind)
						_rebound.Add(bind.Binding);

		if (Valued(rule))
			_result = NewSlot("result", IrSlotKind.Value, _graph.Types.TryGetValue(rule, out var type) ? type : null);

		if (_graph.Results.TryGetValue(rule, out var members))
		{
			foreach (var member in members)
			{
				var slot = NewSlot(member.Name, member.IsSequence ? IrSlotKind.List : IrSlotKind.Value,
					member.Rule is { } built && _graph.Types.TryGetValue(built, out var memberType) ? memberType : null);

				_members[member.Name] = slot;

				foreach (var joined in member.Joins)
					_members[joined] = slot;
			}

			if (members.Count > 0 && _result is null)
			{
				// What the rule hands back is a type the generator would write for its captures.
				_complete = false;
				Tally("a rule whose value is a generated type");
			}
		}

		var following = _follow.TryGetValue(rule, out var known) ? known : FollowSets.Continuation.All;
		var lowered   = Lower(body, following, []);

		function.Body      = new IrStmt.Scope(_slots, lowered);
		function.Result    = _result;
		function.SlotCount = _slots.Count;
		function.Complete  = _complete;
	}

	bool Valued(RuleSymbol rule)
	{
		return _graph.Types.ContainsKey(rule) ||
			(_graph.Bodies.TryGetValue(rule, out var body) && (body is Node.Construct || body is Node.Choice choice && choice.Nodes.Any(static one => one is Node.Construct)));
	}

	IrSlot NewSlot(string name, IrSlotKind kind, string? type)
	{
		var slot = new IrSlot(_slots.Count, name, kind, type);

		_slots.Add(slot);

		return slot;
	}

	int Guarding(int expected)
	{
		_program.Guarding.Add(expected);

		return expected;
	}

	int Expect(string label)
	{
		if (!_expected.TryGetValue(label, out var index))
		{
			_expected.Add(label, index = _program.Expected.Count);
			_program.Expected.Add(label);
		}

		return index;
	}

	void Tally(string what)
	{
		_program.UnsupportedKinds[what] = _program.UnsupportedKinds.TryGetValue(what, out var count) ? count + 1 : 1;
	}

	IrStmt Unsupported(Node node, string? what = null)
	{
		what ??= node.GetType().Name;

		_complete = false;
		_program.UnsupportedNodes++;
		Tally(what);

		return new IrStmt.Unsupported(what);
	}

	IReadOnlyList<IrSlot> Resets(HashSet<IrSlot> assigned)
	{
		return _options.OmitResets ? [] : [.. assigned.OrderBy(static slot => slot.Index)];
	}

	IrStmt Lower(Node node, FollowSets.Continuation following, HashSet<IrSlot> assigned)
	{
		_program.GraphNodes++;

		switch (node)
		{
			case Node.Empty:
				return new IrStmt.Block([]);

			case Node.Element element:
				return element.Categories.Count > 0 || element.References.Count > 0
					? Unsupported(node, "a set that names a category or a rule")
					: new IrStmt.MatchSet(element.Ranges, element.IsNegated, Recorded<TerminalSite>(node) ?? Expect(element.ToString()));

			case Node.Literal literal:
				return new IrStmt.MatchText(literal.Text, literal.IgnoreCase, Recorded<TerminalSite>(node) ?? Expect(literal.ToString()));

			case Node.Sequence sequence:
			{
				// What follows each part is what the parts after it begin with, worked out from the end.
				var after = new FollowSets.Continuation[sequence.Nodes.Count];
				var next  = following;

				for (var i = sequence.Nodes.Count - 1; i >= 0; i--)
				{
					after[i] = next;
					next     = Preceding(sequence.Nodes[i], next);
				}

				var items = new IrStmt[sequence.Nodes.Count];

				for (var i = 0; i < items.Length; i++)
					items[i] = Lower(sequence.Nodes[i], after[i], assigned);

				return new IrStmt.Block(items);
			}

			case Node.Choice { Selection: not null }:
				return Unsupported(node, "a choice with a selector");

			case Node.Choice choice:
			{
				var alternatives = new IrStmt[choice.Nodes.Count];
				var resets       = new IReadOnlyList<IrSlot>[choice.Nodes.Count];

				for (var i = 0; i < alternatives.Length; i++)
				{
					var inside = new HashSet<IrSlot>();

					alternatives[i] = Lower(choice.Nodes[i], following, inside);
					resets[i]       = Resets(inside);
					assigned.UnionWith(inside);
				}

				var lowered = new IrStmt.Choice(alternatives, resets,
					_options.AllResumable ||
					(PlannedWay(choice.Nodes[0], choice.Nodes.Count) ?? !Proved(() => Determinism.Of(choice, following, _graph, _seam))));

				return _sites.Count > 0 ? AsRecorded(choice.Nodes, lowered) : lowered;
			}

			case Node.Atomic atomic:
			{
				var inside = new HashSet<IrSlot>();
				var body   = Lower(atomic.Body, following.Committed(), inside);

				assigned.UnionWith(inside);

				return new IrStmt.Commit(body, Resets(inside)) { Defers = DefersAt(_rule, RetrySite.Atomic) };
			}

			case Node.Repeat repeat when _graph.Recoveries.ContainsKey(repeat):
				return Unsupported(node, "a repetition that recovers");

			case Node.Repeat repeat:
				return Repeated(
					repeat.Body, new IrExpr.Number(repeat.Min), repeat.Max is { } max ? new IrExpr.Number(max) : null,
					Preceding(repeat, following), assigned,
					_options.AllResumable ||
					(PlannedWay(repeat, 0) ?? !(repeat.Max == repeat.Min || Proved(() => Determinism.NeverGivesBack(repeat, following.Taught, _graph, _seam)))),
					at: repeat);

			case Node.Counted counted:
				return Repeated(
					counted.Body, Bound(counted.Min), counted.Max is { } most ? Bound(most) : null,
					FollowSets.Continuation.All, assigned, resumable: true, at: counted);

			case Node.Lookahead look:
			{
				var inside = new HashSet<IrSlot>();
				var body   = Lower(look.Body, FollowSets.Continuation.Sealed, inside);

				assigned.UnionWith(inside);

				var ends = !look.IsPositive && look.Body is Node.Element { IsNegated: true, Ranges.Count: 0, Categories.Count: 0, References.Count: 0 };

				return new IrStmt.Look(look.IsPositive, body, Resets(inside), RecordedLook(look) ??
					Expect(ends ? "end of input" : (look.IsPositive ? "" : "not ") + look.Body));
			}

			case Node.Capture capture:
				return Captured(capture, following, assigned);

			case Node.Bind bind:
			{
				var (from, to) = BindingSlots(bind.Binding);

				assigned.Add(from);
				assigned.Add(to);

				return new IrStmt.Block(
				[
					new IrStmt.Assign(from, IrExpr.Position.Instance),
					Lower(bind.Body, following, assigned),
					new IrStmt.Assign(to, IrExpr.Position.Instance),
				]);
			}

			case Node.Same same:
			{
				var (from, to) = BindingSlots(same.Binding);

				return new IrStmt.MatchSame(from, to, Recorded<TerminalSite>(node) ?? Expect(same.ToString()));
			}

			case Node.Guard guard:
			{
				var fragment = Fragment(guard.Text, guard.At, isGuard: true);

				return fragment is null
					? Unsupported(node, "a guard the compiler wrote")
					: new IrStmt.Guard(fragment, Arguments(fragment), Guarding(Expect(guard.OnFail ?? "when " + fragment.Name)));
			}

			case Node.Construct construct:
				return Constructed(construct, following, assigned);

			case Node.Call { Arguments.Count: > 0 }:
				return Unsupported(node, "a call with arguments");

			// A rule of one character the readers read where it is called (MachinePlan.ReadInPlace) is
			// tested there as one character, and refused as the call is said.
			case Node.Call call when Fact<TerminalSite>(call) is { } tested && FirstSets.Of(call, _graph) is { Ends: false, Nothing: false } one:
				Asked(tested);

				return new IrStmt.MatchSet(one.Ranges, false, tested.Records ? Expect(Label(tested)) : -1);

			case Node.Call call:
				return _functions.TryGetValue(call.Rule, out var target)
					? new IrStmt.Call(target, null, false) { Says = Saying(call) }
					: Unsupported(node, "a call of a rule with no body");

			default:
				return Unsupported(node);
		}
	}

	FollowSets.Continuation Preceding(Node node, FollowSets.Continuation after)
	{
		try
		{
			return FollowSets.Precedes(node, after, _graph, _seam);
		}
		catch (Exception exception) when (exception is InvalidOperationException or KeyNotFoundException or NotSupportedException)
		{
			return FollowSets.Continuation.All;
		}
	}

	/// <summary>A proof asked of the generator; one that cannot be asked proves nothing.</summary>
	static bool Proved(Func<bool> proof)
	{
		try
		{
			return proof();
		}
		catch (Exception exception) when (exception is InvalidOperationException or KeyNotFoundException or NotSupportedException)
		{
			return false;
		}
	}

	IrExpr Bound(CountBound bound)
	{
		if (bound is CountBound.Items items)
			return new IrExpr.Number(items.Count);

		var (from, to) = BindingSlots(((CountBound.Length)bound).Binding);

		return new IrExpr.Length(from, to);
	}

	(IrSlot From, IrSlot To) BindingSlots(TextBinding binding)
	{
		if (!_bindings.TryGetValue(binding, out var slots))
		{
			var kind = _rebound.Contains(binding) ? IrSlotKind.Rebound : IrSlotKind.Position;

			_bindings.Add(binding, slots = (NewSlot(binding.Name + "From", kind, null), NewSlot(binding.Name + "To", kind, null)));
		}

		return slots;
	}

	IrStmt Repeated(
		Node body, IrExpr min, IrExpr? max, FollowSets.Continuation following, HashSet<IrSlot> assigned, bool resumable, Node? at = null)
	{
		var inside = new HashSet<IrSlot>();
		var turn   = Lower(body, following, inside);

		assigned.UnionWith(inside);

		var repeat = new IrStmt.Repeat(turn, min, max, Resets(inside), resumable);

		// Behind the door the reader shuts, no turn is read and nothing of it is said.
		if (_sites.Count > 0 && at is not null && Asked(Fact<DoorSite>(at), misses: false) is { } door)
			repeat = repeat with
			{
				Door          = door.Begins,
				DoorPastLeast = door.PastFewest,
				DoorQuietOnly = door.QuietOnly,
				DoorShort     = door.Items.Count > 0 ? Expect(Label(door)) : -1,
			};

		return repeat;
	}

	/// <summary>
	/// What the plan's readers record at a place of the rule being lowered (<see cref="Options.AsRecorded"/>):
	/// the place keyed as the generator keys it, the rule a settled copy was made from and the node it was
	/// copied from. Null where the lowering does not ask, or the plan has no such place there.
	/// </summary>
	/// <remarks>
	/// A terminal and a look always have a place where the readers read them, and one the plan does not
	/// hold is a miss (<see cref="IrProgram.SiteMisses"/>); a choice or a repetition has one only where the
	/// readers decide it by the next item, and is counted where it does.
	/// </remarks>
	T? Fact<T>(Node node, int count = 0)
		where T : RecordSite
	{
		if (_sites.Count == 0)
			return null;

		var owner = _graph.Counted(_rule);
		var at    = _graph.Original(node);

		foreach (var sites in _sites)
			if (sites.Held<T>(owner, at, count) is { } held)
				return held;

		return null;
	}

	/// <summary>Counts a place asked of the plan, and a miss where it had no answer; the answer, read.</summary>
	T? Asked<T>(T? site, bool misses = true)
		where T : class
	{
		if (site is not null || misses)
			_program.SitesAsked++;

		if (site is null && misses)
			_program.SiteMisses++;

		if (site is RecordSite read)
			_read.Add(read);

		return site;
	}

	/// <summary>The places of the plan the lowering read (<see cref="Asked"/>).</summary>
	readonly HashSet<RecordSite> _read = [];

	/// <summary>
	/// Counts as misses the places the plan holds in the bodies of the rules lowered that the lowering did not
	/// read — a kind it does not lower, a choice told apart past the seam, a group it could not map — by kind:
	/// what the generated reader records there, the lowering does not. A place is read where any machine's
	/// answer for it was.
	/// </summary>
	void Unread(List<RuleSymbol> rules)
	{
		if (_sites.Count == 0)
			return;

		var owners = rules.Select(_graph.Counted).ToHashSet();
		var nodes  = NodeWalk.ByIdentity(rules.SelectMany(rule => NodeWalk.Descendants(_graph.Bodies[rule])));
		var read   = new HashSet<(RuleSymbol, Node, int, Type)>(PlaceIdentity.Instance);

		foreach (var sites in _sites)
			foreach (var (owner, node, count, site) in sites.All())
				if (_read.Contains(site))
					read.Add((owner, node, count, site.GetType()));

		foreach (var sites in _sites)
			foreach (var (owner, node, count, site) in sites.All())
			{
				if (!owners.Contains(owner) || !nodes.Contains(node) || !read.Add((owner, node, count, site.GetType())))
					continue;

				var kind = site is DispatchSite { Seam: true } ? "DispatchSite past the seam" : site.GetType().Name;

				_program.SitesAsked++;
				_program.SiteMisses++;
				_program.UnreadKinds[kind] = _program.UnreadKinds.GetValueOrDefault(kind) + 1;
			}
	}

	/// <summary>A place by the identity of its rule and node.</summary>
	sealed class PlaceIdentity : IEqualityComparer<(RuleSymbol Owner, Node Node, int Count, Type Kind)>
	{
		public static readonly PlaceIdentity Instance = new();

		public bool Equals((RuleSymbol Owner, Node Node, int Count, Type Kind) one, (RuleSymbol Owner, Node Node, int Count, Type Kind) other)
		{
			return ReferenceEquals(one.Owner, other.Owner) && ReferenceEquals(one.Node, other.Node) && one.Count == other.Count && one.Kind == other.Kind;
		}

		public int GetHashCode((RuleSymbol Owner, Node Node, int Count, Type Kind) place)
		{
			return HashCode.Combine(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(place.Owner),
				System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(place.Node), place.Count, place.Kind);
		}
	}

	/// <summary>The rule's own words a call records where the rule refused having read nothing, as the plan says; -1 for none.</summary>
	int Saying(Node.Call call)
	{
		return Asked(Fact<MessageSite>(call), misses: false) is { } said ? Expect(Label(said)) : -1;
	}

	/// <summary>A set as the readers say it: its items, one label.</summary>
	static string Label(RecordSite site)
	{
		return string.Join(" | ", site.Items);
	}

	/// <summary>What a terminal records, as the plan says: its set, or nothing where it records for none (-1); null where the plan was not asked.</summary>
	int? Recorded<T>(Node node)
		where T : RecordSite
	{
		if (_sites.Count == 0)
			return null;

		return Asked(Fact<T>(node)) is { } site ? site.Records ? Expect(Label(site)) : -1 : null;
	}

	/// <summary>
	/// What a look records where it refuses: the end, the rule it is the body of, or nothing named — recorded
	/// where it stands (-2), or not at all (-1). Null where the plan was not asked or has none of them.
	/// </summary>
	int? RecordedLook(Node.Lookahead look)
	{
		if (_sites.Count == 0)
			return null;

		RecordSite? site = Fact<EndSite>(look) ?? Fact<NamedSite>(look) ?? (RecordSite?)Fact<SilentSite>(look);

		return Asked(site) switch
		{
			null                       => null,
			SilentSite { Records: true } => -2,
			SilentSite                   => -1,
			var said                     => Expect(Label(said)),
		};
	}

	/// <summary>
	/// A choice as the plan's readers read it where they decide it by the next item: told apart into the
	/// groups the item tells apart, each tried in order inside, with the widest read by the call it begins
	/// with; or behind a door, refused (or, an optional, noted) where the item begins no alternative.
	/// </summary>
	IrStmt AsRecorded(IReadOnlyList<Node> nodes, IrStmt.Choice choice)
	{
		var count = nodes.Count;

		// Past the seam the item that tells the alternatives apart is not the next one: not lowered, a miss (Unread).
		if (Fact<DispatchSite>(nodes[0], count) is { Seam: false } dispatch)
		{
			var over         = Fact<OverSite>(nodes[0], count);
			var otherwise    = Expect(Label(dispatch));
			var alternatives = new List<IrStmt>();
			var resets       = new List<IReadOnlyList<IrSlot>>();

			for (var g = 0; g < dispatch.Members.Count; g++)
			{
				var members = dispatch.Members[g];

				if (members.Any(static one => one < 0))
					return choice;

				IrStmt group = members.Count == 1
					? choice.Alternatives[members[0]]
					: new IrStmt.Choice([.. members.Select(one => choice.Alternatives[one])], [.. members.Select(one => choice.Resets[one])],
						_options.AllResumable || (PlannedWay(nodes[members[0]], members.Count) ?? choice.Resumable));

				if (g == dispatch.Called)
					group = RefusingWith(group, otherwise, over is null ? -1 : Expect(Label(over)));

				alternatives.Add(group);
				resets.Add([.. members.SelectMany(one => choice.Resets[one]).Distinct()]);
			}

			Asked(dispatch, misses: false);

			if (over is not null && dispatch.Called >= 0)
				Asked(over, misses: false);

			return new IrStmt.Choice(alternatives, resets, false)
			{
				Dispatch = new IrDispatch(dispatch.Begins, otherwise) { Called = dispatch.Called },
			};
		}

		if (Asked(Fact<PredictedSite>(nodes[0], count), misses: false) is { } predicted)
			return choice with { Gate = new IrGate(predicted.Begins, Expect(Label(predicted)), Empty: false) };

		if (Asked(Fact<OptionalSite>(nodes[0], count), misses: false) is { } optional)
			return choice with { Gate = new IrGate(optional.Begins, Expect(Label(optional)), Empty: true) };

		// Not every choice is decided by its next item: one tried in order records nothing of its own.
		return choice;
	}

	/// <summary>A block whose first step that reads is given <see cref="RefusingWith"/>: a capture's start before it reads nothing.</summary>
	static IrStmt Leading(IrStmt.Block block, int expected, int over)
	{
		var items = block.Items.ToList();
		var first = items.FindIndex(static one => one is not IrStmt.Assign);

		if (first >= 0)
			items[first] = RefusingWith(items[first], expected, over);

		return new IrStmt.Block(items);
	}

	/// <summary>The part, with the call it begins with recording the choice's set where it refuses, over what it said.</summary>
	static IrStmt RefusingWith(IrStmt part, int expected, int over)
	{
		return part switch
		{
			IrStmt.Call call                              => call with { RefusesWith = expected, Over = over },
			IrStmt.Scope scope                            => scope with { Body = RefusingWith(scope.Body, expected, over) },
			IrStmt.Block block                            => Leading(block, expected, over),
			_                                             => part,
		};
	}

	IrStmt Captured(Node.Capture capture, FollowSets.Continuation following, HashSet<IrSlot> assigned)
	{
		if (!_members.TryGetValue(capture.Name, out var slot))
			return Unsupported(capture, "a capture that is no member of the rule's value");

		assigned.Add(slot);

		// What another rule built is taken from the call; anything else is the text that was read.
		if (capture.Body is Node.Call { Arguments.Count: 0 } call && _functions.TryGetValue(call.Rule, out var target) && Valued(call.Rule))
		{
			_program.GraphNodes++;

			return new IrStmt.Call(target, slot, slot.Kind == IrSlotKind.List) { Says = Saying(call) };
		}

		var from = NewSlot(capture.Name + "From", IrSlotKind.Position, null);
		var body = Lower(capture.Body, following, assigned);
		var text = new IrExpr.Text(from);

		return new IrStmt.Scope([from], new IrStmt.Block(
		[
			new IrStmt.Assign(from, IrExpr.Position.Instance),
			body,
			slot.Kind == IrSlotKind.List ? new IrStmt.Append(slot, text) : new IrStmt.Assign(slot, text),
		]));
	}

	IrStmt Constructed(Node.Construct construct, FollowSets.Continuation following, HashSet<IrSlot> assigned)
	{
		if (_result is null)
			return Unsupported(construct, "a construction in a rule with no value");

		var body = Lower(construct.Body, following, assigned);

		IrStmt build;

		switch (construct.How)
		{
			case Construction.Expression expression when PassedThrough(expression.Text) is { } name && _members.TryGetValue(name, out var passed):
				build = new IrStmt.Assign(_result, Read(passed));
				break;

			case Construction.Expression expression:
				var fragment = Fragment(expression.Text, expression.At, isGuard: false);

				if (fragment is null)
					return Unsupported(construct, "a construction the compiler wrote");

				build = new IrStmt.Construct(fragment, Arguments(fragment), _result);
				break;

			case Construction.Sequence or Construction.Operand:
				var members = _graph.Results[_rule];
				var member  = construct.How is Construction.Sequence
					? members.FirstOrDefault(static one => one.IsSequence)
					: members.Count == 1 ? members[0] : null;

				if (member is null)
					return Unsupported(construct, "a sequence or an operand with no one member to take");

				build = new IrStmt.Assign(_result, Read(_members[member.Name]));
				break;

			default:
				return Unsupported(construct, "a construction by constructor or initializer");
		}

		assigned.Add(_result);

		return new IrStmt.Block([body, build]);
	}

	static IrExpr Read(IrSlot slot)
	{
		return slot.Kind == IrSlotKind.List ? new IrExpr.Items(slot) : new IrExpr.Slot(slot);
	}

	IReadOnlyList<IrExpr> Arguments(IrFragment fragment)
	{
		return [.. fragment.Parameters.Select(name => Read(_members[name]))];
	}

	[GeneratedRegex(@"^\s*\(?\s*([A-Za-z_][A-Za-z0-9_]*)\s*\)?\s*$")]
	private static partial Regex OneName();

	/// <summary>The capture a construction hands on as it is, where it is nothing but that capture's name.</summary>
	static string? PassedThrough(string text)
	{
		var match = OneName().Match(text);

		return match.Success ? match.Groups[1].Value : null;
	}

	IrFragment? Fragment(string text, int at, bool isGuard)
	{
		if (at < 0 || !_fragmentIds.TryGetValue(at, out var id))
			return null;

		foreach (var known in _program.Fragments)
			if (known.Id == id)
				return known;

		// The captures it names, in the order the rule declares them: what the host function
		// lifted out of it takes.
		var names = _graph.FreeNames.TryGetValue(text, out var free)
			? free
			: [.. Regex.Matches(text, @"[A-Za-z_][A-Za-z0-9_]*").Select(static match => match.Value)];

		var parameters = _graph.Results.TryGetValue(_rule, out var members)
			? members.Where(member => names.Contains(member.Name)).Select(static member => member.Name).ToList()
			: [];

		var namesParser = names.Any(static name => name.StartsWith("parser", StringComparison.Ordinal) || name == "context");

		if (namesParser)
			Tally("a fragment that names what the parser supplies");

		var fragment = new IrFragment(id, isGuard, text, at, parameters, _rule.Name) { NamesParser = namesParser };

		_program.Fragments.Add(fragment);

		return fragment;
	}

	/// <summary>
	/// Which functions can leave a decision on the tape, and a replay region round the body of
	/// each that can: a failure after such a decision is answered by running the body again.
	/// </summary>
	void Settle()
	{
		var changed = true;

		while (changed)
		{
			changed = false;

			foreach (var function in _program.Functions)
			{
				if (!function.Decides && Decides(function.Body))
				{
					function.Decides = true;
					changed          = true;
				}
			}
		}

		// Neither reading a part again in place nor opening a repetition whose turn decides already makes
		// a function decide that did not.
		if (!_options.KeepsPartsWhole)
			foreach (var (rule, function) in _functions)
				function.Body = Retrying(function.Body, rule);
		else if (_options.ReopensInstead)
			foreach (var function in _functions.Values)
				function.Body = Reopen(function.Body);

		foreach (var (rule, function) in _functions)
			if (function.Decides)
				function.Body = new IrStmt.Replay(function.Body) { Defers = DefersAt(rule, RetrySite.Rule) };

		_program.Fragments.Sort(static (one, other) => one.Id.CompareTo(other.Id));
	}

	/// <summary>
	/// Every turn and every alternative of an open choice that can leave a decision on the tape, read
	/// again in place over its own decisions before it is given up (<see cref="IrStmt.Repeat.Retries"/>,
	/// <see cref="IrStmt.Choice.Retries"/>), and stopping building as the plan's readers do at that kind
	/// of loop (<see cref="DefersAt"/>); counted (<see cref="IrProgram.Retrying"/>).
	/// </summary>
	/// <remarks>
	/// The generated reader writes such a loop round every turn that writes a way, whatever it decided
	/// about the repetition's own way and below the fewest too, and round every alternative that writes
	/// one of a choice it reads with a way, the last too (<c>RetrySite.Turn</c>, <c>RetrySite.Alternative</c>).
	/// A closed decision of the plan says only that nothing after the node wants it back: the turn's own
	/// shorter readings are still asked for, here as there, and the replay around is not what asks them.
	/// </remarks>
	IrStmt Retrying(IrStmt stmt, RuleSymbol rule)
	{
		switch (stmt)
		{
			case IrStmt.Block block:
				return block with { Items = [.. block.Items.Select(item => Retrying(item, rule))] };

			case IrStmt.Scope scope:
				return scope with { Body = Retrying(scope.Body, rule) };

			case IrStmt.Choice choice:
			{
				var alternatives = choice.Alternatives.Select(alternative => Retrying(alternative, rule)).ToList();

				if (!choice.Resumable || !alternatives.Any(Decides))
					return choice with { Alternatives = alternatives };

				_program.Retrying++;

				return choice with { Alternatives = alternatives, Retries = true, Defers = DefersAt(rule, RetrySite.Alternative) };
			}

			case IrStmt.Repeat repeat:
			{
				var body = Retrying(repeat.Body, rule);

				if (!Decides(body))
					return repeat with { Body = body };

				_program.Retrying++;

				return repeat with { Body = body, Retries = true, Defers = DefersAt(rule, RetrySite.Turn) };
			}

			case IrStmt.Look look:
				return look with { Body = Retrying(look.Body, rule) };

			case IrStmt.Commit commit:
				return commit with { Body = Retrying(commit.Body, rule) };

			case IrStmt.Replay replay:
				return replay with { Body = Retrying(replay.Body, rule) };

			default:
				return stmt;
		}
	}

	/// <summary>
	/// The repetitions the plan left without a way whose turns can fail after a decision made inside
	/// them, opened (<see cref="IrProgram.Reopened"/>): what <see cref="Options.ReopensInstead"/> lowers.
	/// </summary>
	/// <remarks>
	/// <para>
	/// That the plan writes no way for a repetition says only that nothing after it wants a turn back.
	/// The generated reader still reads every turn in a loop of its own: a turn that fails is read again
	/// with each decision made inside it moved before the repetition gives the turn up and stops
	/// (<c>RetrySite.Turn</c>). The tree has that reading of a turn only as part of the repetition's own
	/// way: a turn past the fewest of a repetition that is not resumable is given up as it first failed,
	/// and the shorter reading inside it is never asked for — <c>(B* &amp; Letter &amp; ';')+</c> with
	/// <c>B = Letter &amp; ','?</c> refuses <c>a,b;c;</c>, whose second turn needs <c>B*</c> to give the
	/// <c>c</c> back.
	/// </para>
	/// <para>
	/// So the closed decision is not lowered where it would drop that reading: where a turn past the
	/// fewest can be read at all (the bounds differ) and can fail after a decision it leaves on the
	/// tape (<see cref="FailsAfterDeciding"/>). Opened, the repetition also offers to give its turns
	/// back, which the proof says no failure after it ever takes: what it changes is the cost and not
	/// the answer. A turn below the fewest needs no loop of its own for the answer: its failure is the
	/// repetition's, and the replay around it moves the decision inside.
	/// </para>
	/// </remarks>
	IrStmt Reopen(IrStmt stmt)
	{
		switch (stmt)
		{
			case IrStmt.Block block:
				return block with { Items = [.. block.Items.Select(Reopen)] };

			case IrStmt.Scope scope:
				return scope with { Body = Reopen(scope.Body) };

			case IrStmt.Choice choice:
				return choice with { Alternatives = [.. choice.Alternatives.Select(Reopen)] };

			case IrStmt.Repeat repeat:
			{
				var body    = Reopen(repeat.Body);
				var reopens = !repeat.Resumable && !Bounded(repeat) && FailsAfterDeciding(body);

				if (reopens)
					_program.Reopened++;

				return repeat with { Body = body, Resumable = repeat.Resumable || reopens };
			}

			case IrStmt.Look look:
				return look with { Body = Reopen(look.Body) };

			case IrStmt.Commit commit:
				return commit with { Body = Reopen(commit.Body) };

			case IrStmt.Replay replay:
				return replay with { Body = Reopen(replay.Body) };

			default:
				return stmt;
		}

		// Every turn it may take it must: none is past the fewest.
		static bool Bounded(IrStmt.Repeat repeat)
		{
			return repeat.Min is IrExpr.Number min && repeat.Max is IrExpr.Number max && min.Value == max.Value;
		}
	}

	/// <summary>
	/// Whether a reading of the statement can fail inside it after a decision made inside it that is
	/// still on the tape: a decision followed in a sequence by something that can fail, or a repetition
	/// whose turns below the fewest, two or more, decide and can fail. A call decides where its rule
	/// does, and what fails inside it its own replay takes back; so does what braces or a look hold.
	/// </summary>
	static bool FailsAfterDeciding(IrStmt stmt)
	{
		switch (stmt)
		{
			case IrStmt.Call or IrStmt.Look or IrStmt.Commit:
				return false;

			case IrStmt.Block block:
			{
				var decided = false;

				foreach (var item in block.Items)
				{
					if (decided && Failable(item) || FailsAfterDeciding(item))
						return true;

					decided |= Decides(item);
				}

				return false;
			}

			case IrStmt.Repeat repeat when repeat.Min is not IrExpr.Number { Value: < 2 } && Decides(repeat.Body) && Failable(repeat.Body):
				return true;
		}

		return stmt.Children.Any(FailsAfterDeciding);
	}

	/// <summary>
	/// Whether a statement can fail: all can but what only assigns or builds, a repetition that may
	/// take no turn, and a choice tried in order with an alternative that cannot.
	/// </summary>
	static bool Failable(IrStmt stmt)
	{
		return stmt switch
		{
			IrStmt.Assign or IrStmt.Append or IrStmt.Construct => false,
			IrStmt.Block block => block.Items.Any(Failable),
			IrStmt.Scope or IrStmt.Replay or IrStmt.Commit => Failable(stmt.Children.Single()),
			IrStmt.Choice { Dispatch: not null } => true,
			IrStmt.Choice choice => choice.Alternatives.All(Failable),
			IrStmt.Repeat repeat => repeat.Min is not IrExpr.Number { Value: 0 } && Failable(repeat.Body),
			_ => true,
		};
	}

	/// <summary>Whether a step can leave a decision on the tape. What a look or braces hold never does.</summary>
	internal static bool Decides(IrStmt stmt)
	{
		switch (stmt)
		{
			case IrStmt.Look or IrStmt.Commit:        return false;
			case IrStmt.Choice { Resumable: true }:   return true;
			case IrStmt.Repeat { Resumable: true }:   return true;
			case IrStmt.Call call:                    return call.Target.Decides;
		}

		foreach (var child in stmt.Children)
			if (Decides(child))
				return true;

		return false;
	}
}
