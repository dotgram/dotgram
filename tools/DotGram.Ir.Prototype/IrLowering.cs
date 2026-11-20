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
	public sealed record Options(bool AllResumable = false, bool OmitResets = false);

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

	IrLowering(RecognitionGraph graph, Options options, IReadOnlyList<WriteDecisions> decisions, IReadOnlyList<DeferralPlan> deferrals)
	{
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
			[.. machines.Select(static machine => machine.Deferral).OfType<DeferralPlan>()]);
	}

	/// <param name="everyRule">Every rule of the graph that has a body, reached or not: what a measurement of the lowering asks.</param>
	/// <param name="decisions">What the plan decided about ways on the tape (<see cref="Planned"/>); empty asks every node of the lowering's own proofs.</param>
	/// <param name="deferrals">Where the plan's readers stop building after a second give-back; empty, nowhere.</param>
	public static IrProgram Lower(RecognitionGraph graph, Options? options, bool everyRule, IReadOnlyList<WriteDecisions> decisions, IReadOnlyList<DeferralPlan>? deferrals = null)
	{
		var lowering = new IrLowering(graph, options ?? new Options(), decisions, deferrals ?? []);

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
					: new IrStmt.MatchSet(element.Ranges, element.IsNegated, Expect(element.ToString()));

			case Node.Literal literal:
				return new IrStmt.MatchText(literal.Text, literal.IgnoreCase, Expect(literal.ToString()));

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

				return new IrStmt.Choice(alternatives, resets,
					_options.AllResumable ||
					(PlannedWay(choice.Nodes[0], choice.Nodes.Count) ?? !Proved(() => Determinism.Of(choice, following, _graph, _seam))));
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
					(PlannedWay(repeat, 0) ?? !(repeat.Max == repeat.Min || Proved(() => Determinism.NeverGivesBack(repeat, following.Taught, _graph, _seam)))));

			case Node.Counted counted:
				return Repeated(
					counted.Body, Bound(counted.Min), counted.Max is { } most ? Bound(most) : null,
					FollowSets.Continuation.All, assigned, resumable: true);

			case Node.Lookahead look:
			{
				var inside = new HashSet<IrSlot>();
				var body   = Lower(look.Body, FollowSets.Continuation.Sealed, inside);

				assigned.UnionWith(inside);

				var ends = !look.IsPositive && look.Body is Node.Element { IsNegated: true, Ranges.Count: 0, Categories.Count: 0, References.Count: 0 };

				return new IrStmt.Look(look.IsPositive, body, Resets(inside),
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

				return new IrStmt.MatchSame(from, to, Expect(same.ToString()));
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

			case Node.Call call:
				return _functions.TryGetValue(call.Rule, out var target)
					? new IrStmt.Call(target, null, false)
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
		Node body, IrExpr min, IrExpr? max, FollowSets.Continuation following, HashSet<IrSlot> assigned, bool resumable)
	{
		var inside = new HashSet<IrSlot>();
		var turn   = Lower(body, following, inside);

		assigned.UnionWith(inside);

		return new IrStmt.Repeat(turn, min, max, Resets(inside), resumable);
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

			return new IrStmt.Call(target, slot, slot.Kind == IrSlotKind.List);
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

		foreach (var (rule, function) in _functions)
			if (function.Decides)
				function.Body = new IrStmt.Replay(function.Body) { Defers = DefersAt(rule, RetrySite.Rule) };

		_program.Fragments.Sort(static (one, other) => one.Id.CompareTo(other.Id));
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
