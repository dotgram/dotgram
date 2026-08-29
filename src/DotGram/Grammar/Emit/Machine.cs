using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// A single threaded automaton for a whole recognition graph. Rules are shared label
/// blocks, not C# calls; a call records its continuation in the parser arena.
/// </summary>
sealed partial class Machine
{
	const int Return = 0;
	const int Accept = 1;
	const int Fail   = 2;
	const int First  = 3;

	readonly RecognitionGraph _graph;
	readonly ResultTypes _results;
	readonly List<Writer> _states = [];
	readonly Dictionary<RuleSymbol, int> _entries = [];
	readonly Dictionary<RuleSymbol, int> _ruleIds = [];
	readonly Dictionary<RuleSymbol, int> _captureOffsets = [];
	readonly Dictionary<Node, int> _captureSlots = new(NodeIdentity.Instance);
	readonly Dictionary<Node, RuleSymbol> _owners = new(NodeIdentity.Instance);
	readonly HashSet<int> _textCaptures = [];

	/// <summary>
	/// The text captures whose start cannot be held in a variable.
	/// </summary>
	/// <remarks>
	/// A capture records where it began at its opening and where it ended at its close, and
	/// a variable between the two is right for as long as nothing opens the same capture in
	/// between. A rule that can reach itself does exactly that: the inner opening overwrites
	/// the outer's start, and — the half that a variable can never get right — a failed
	/// inner attempt leaves its start behind, because backtracking restores the arena and
	/// nothing else. The outer then closes with a start from a position the parse has
	/// already given back, and its span comes back with its end before its beginning.
	/// </remarks>
	readonly HashSet<int> _nestedCaptures = [];

	/// <summary>
	/// The text captures a repetition repeats, whose value is the turns joined.
	/// </summary>
	/// <remarks>
	/// §10: repeated text is the text joined. Where the turns happen to be adjacent the
	/// join is also the span from the first start to the last end, and that is the shape
	/// <see cref="GrammarNormalizer.HoistTextCaptures"/> proves before lifting a capture
	/// out of its repetition — but a capture it left inside one has whatever else the
	/// loop's body matched standing between the turns, and there the span is longer than
	/// the text. Both are handled at once: the pieces are measured while they are
	/// collected, and the span is taken only where the measurements say it tiles.
	/// </remarks>
	readonly HashSet<int> _repeatedCaptures = [];
	readonly Dictionary<RuleSymbol, IReadOnlyList<Factory>> _factories = [];
	readonly Dictionary<Node, int> _constructs = new(NodeIdentity.Instance);
	readonly Dictionary<Node, RecoveryPlan> _recoveries = new(NodeIdentity.Instance);
	readonly List<RecoveryPlan> _recoveryPlans = [];
	readonly Dictionary<RuleSymbol, int> _wholeEntries = [];
	readonly List<string> _extra = [];

	/// <summary>Every array declared, by name, in the order they were asked for.</summary>
	readonly List<(string Name, string Declaration)> _expected = [];

	/// <summary>The names something actually wrote into a state.</summary>
	readonly HashSet<string> _expectedUsed = [];

	/// <summary>One name per distinct set, so the same list is not written out twice.</summary>
	readonly Dictionary<string, string> _expectedByItems = new(StringComparer.Ordinal);
	int _expectedCount;
	readonly ILineMap? _lines;
	readonly bool _starves;
	bool _usesChar;

	/// <summary>
	/// Whether any state the layout kept reads <c>c</c> — asked of the written bodies
	/// rather than of the compile-time flag, because compilation sets the flag for states
	/// the layout may then drop, and a variable declared for dropped states is a compiler
	/// warning in somebody else's build. Found by the differential fuzzer, as a grammar
	/// whose generated parser did not compile clean.
	/// </summary>
	bool UsesChar
	{
		get
		{
			if (!_usesChar)
				return false;

			foreach (var index in _order)
				if (_bodies[index].Contains("c = text[", StringComparison.Ordinal) ||
					_bodies[index].Contains("c == ", StringComparison.Ordinal))
					return true;

			return false;
		}
	}
	bool _usesRuns;
	bool _usesCompleted;
	bool _usesDead;
	readonly List<(int Depth, int State)> _turns = [];
	int _depth;

	/// <summary>
	/// Where a failure goes from here — <see cref="Fail"/>, the arena's dispatcher, unless
	/// something has taken responsibility for the failure itself.
	/// </summary>
	/// <remarks>
	/// Only code that has written nothing into the arena may be redirected, because the
	/// dispatcher is what would otherwise take back what was written. <see cref="Silent"/>
	/// is the test for that, and it is the only thing that sets this.
	/// </remarks>
	/// <summary>The rules this machine is built from — see the constructor's <c>only</c>.</summary>
	readonly IReadOnlyCollection<RuleSymbol> _rules;

	/// <summary>What separates this machine's names from a sibling's — the constructor's <c>tag</c>.</summary>
	readonly string _tag;

	int _fail = Fail;
	bool _materializer;
	bool _guardValues;
	int _guards;
	int _sharpens;
	int _captures;

	/// <summary>
	/// Whether a choice here may keep its way back in locals — the checkpoint class.
	/// </summary>
	/// <remarks>
	/// True only while a flat method is being asked about or written, and only outside
	/// the constructs that route failure somewhere other than <c>Fail:</c> — a silent
	/// repetition's door, an atomic group's chain, a lookahead's rewind. Inside those a
	/// pending way back would be jumped past, so the flag is put down on the way in.
	/// <see cref="Silent"/> and <see cref="Compile"/> both read it, which is what keeps
	/// the two from answering differently about the same choice.
	/// </remarks>
	bool _checkpointsAllowed;

	/// <summary>A choice compiled with its way back in locals — one per static site.</summary>
	/// <param name="Id">The site's number, which its locals are named under.</param>
	/// <param name="Count">How many alternatives it has.</param>
	/// <param name="Retries">The state that re-enters each alternative after the first.</param>
	sealed record CheckpointSite(int Id, int Count, IReadOnlyList<int> Retries);

	readonly List<CheckpointSite> _checkpoints = [];
	int _checkpointIds;

	/// <summary>The states something outside the state bodies jumps to by label.</summary>
	readonly HashSet<int> _namedOutside = [];

	/// <summary>
	/// Whether any flat method of this machine can reach the same failure position twice —
	/// which is what decides whether the emitted <c>Failure</c> struct needs
	/// <c>ExpectedMore</c> and whether the wrapper hands it over.
	/// </summary>
	public bool Ties { get; private set; }

	/// <param name="only">
	/// The rules this machine compiles, or null for all of them. A machine is built for one
	/// published rule and what that rule reaches, so a grammar with several publications has
	/// several machines and none of them carries the others' states.
	/// </param>
	/// <param name="tag">
	/// What distinguishes this machine's method and array names from another's in the same
	/// file. Empty where there is only one, so a grammar with a single publication is
	/// compiled to exactly the names it always was.
	/// </param>
	public Machine(
		RecognitionGraph graph, ResultTypes results, ILineMap? lines, bool starves = false,
		IReadOnlyCollection<RuleSymbol>? only = null, string tag = "")
	{
		_graph = graph;
		_results = results;
		_lines = lines;
		_starves = starves;
		_tag = tag;
		_rules = only ?? graph.Rules;
		_guardValues = HasTypedGuards(graph);

		var doors = Doors.ByRule(graph.Rules, graph.Bodies);

		foreach (var rule in _rules)
		{
			var layout = CaptureLayout.Of(
				graph.Bodies[rule], other => graph.Results[other].Count > 0 || graph.Types.ContainsKey(other));
			var factories = CSharpEmitter.FactoriesOf(graph, results, rule);

			_captureOffsets[rule] = _captures;
			_factories[rule] = factories;

			var looped = Looped(graph.Bodies[rule]);

			foreach (var node in NodeWalk.Descendants(graph.Bodies[rule]))
			{
				_owners[node] = rule;

				if (node is Node.Capture(_, var captured))
				{
					var slot = _captures + layout.SlotOf(node);

					_captureSlots[node] = slot;

					if (node is not Node.Capture(_, Node.Lookahead) &&
						(node is not Node.Capture(_, Node.Call(var called, _)) ||
						graph.Results[called].Count == 0 && !graph.Types.ContainsKey(called)))
					{
						_textCaptures.Add(slot);

						// A variable holds the start between the opening and the close, and it
						// is right for exactly as long as nothing opens the same capture in
						// between. Two things do, and the second is the general one:
						//
						//   * a rule that reaches itself, opening the inner before the outer
						//     closes;
						//   * *any* second reading of this capture, once the close can be
						//     reached a second time. A door inside the body is what makes
						//     that possible: the close runs, the parse goes on, the same rule
						//     is read again somewhere else and writes the variable, and then
						//     a failure unwinds to that door and runs the close again — with
						//     a start belonging to the other reading.
						//
						// So the question is only whether the body leaves a door. Where it
						// leaves none there is no way back into the close, and the variable
						// is still right — which is most captures, `port: Digit+` over a set
						// of digits included.
						if (graph.Recursive.Contains(rule) || Doors.LeavesOne(captured, doors))
							_nestedCaptures.Add(slot);

						// And a capture inside a repetition records one entry per turn, whose
						// value §10 says is the text joined. The turns need not be adjacent —
						// anything else in the loop's body stands between them — so the span
						// from the first start to the last end is not that text.
						if (looped.Contains(node))
							_repeatedCaptures.Add(slot);
					}
				}
				else if (node is Node.Construct)
					_constructs[node] = IndexOf(factories, node);
			}

			_captures += layout.Slots.Count;

			// One plan per marked repetition: each has its own sync, its own `=>` and its
			// own sequence, and the arena has always dispatched a recovery by plan.
			var recoveries = CSharpEmitter.RecoveriesIn(graph, results, rule);

			for (var found = 0; found < recoveries.Count; found++)
			{
				var (repetition, recovery, recoverySlot) = recoveries[found];
				var plan = new RecoveryPlan(
					rule, recovery, recoverySlot < 0 ? -1 : _captureOffsets[rule] + recoverySlot,
					_recoveryPlans.Count, CSharpEmitter.RecoveryMethod(rule, found),
					recoverySlot < 0 ? null : layout.Slots[recoverySlot].Rule);

				_recoveries[repetition] = plan;
				_recoveryPlans.Add(plan);
			}
		}

		// After the first pass, because qualification reads the factories of callees the
		// pass may not have reached yet; before anything compiles, because a site is a
		// run of slots and the run has to be numbered before a state names it.
		PlanSites();

		var ruleIndex = 0;

		foreach (var rule in _rules)
		{

			_ruleIds[rule] = ruleIndex++;
			_entries[rule] = Reserve(out _);
		}

		_plan    = ExecutionPlan.Of(graph);

		foreach (var rule in _rules)
			if (graph.Bodies.TryGetValue(rule, out var checking))
				foreach (var node in NodeWalk.Descendants(checking))
				{
					if (node is Node.Construct { How: Construction.Expression { Text: var built } } &&
						built.Contains("parserInput"))
					{
						UsesInput = true;
					}

					// The same, and for the same reason: the flat rendering builds a
					// factory's arguments out of its members alone, and this is not one —
					// it comes off the arena the flat rendering exists not to have.
					if (graph.State is not null &&
						node is Node.Construct { How: Construction.Expression { Text: var reading } } &&
						reading.Contains("parserState"))
					{
						ReadsState = true;
					}

					// A `when` may name it as well as a `=>`, which `parserInput` cannot —
					// the input is what a value keeps, and the context is what a guard
					// writes into.
					if (graph.Context is not null &&
						(node is Node.Construct { How: Construction.Expression { Text: var asked } } &&
							CSharpEmitter.Names(asked, "context") ||
						node is Node.Guard { Text: var condition } &&
							CSharpEmitter.Names(condition, "context")))
					{
						UsesContext = true;
					}
				}

		CollectValueTypes();

		// What follows each rule, before any of them is compiled. A body is compiled once and
		// called from everywhere, so what it is told has to be the union over its callers —
		// and that is only known once every one of them has been looked at.
		_follow = FollowSets.Of(graph);


		foreach (var rule in _rules)
		{
			_seam      = FollowSets.SeamOf(rule, graph);
			_traceRule = rule.Name;

			var body = Compile(graph.Bodies[rule], Return, Follows(rule));
			var entry = _states[_entries[rule] - First];

			// Nothing but the jump, so that `JumpOnly` can collapse this state away and the
			// callers reach the body directly. It used to trace here as well, which said
			// twice what the call site already says once — `Trace("call R", …)` sits
			// immediately before the jump in — and cost every rule a state, a dispatch case
			// and a block for the second saying of it. What the trace loses is the root
			// entry, which is not reached from a call site; that one is written once at the
			// top of the method instead.
			entry.Line($"goto {Label(body)};");
		}
	}

	/// <summary>The captures a repetition repeats — the ones that record a turn at a time.</summary>
	/// <remarks>
	/// Identity, not value: two captures written the same way in two places are two
	/// captures, and only the one a loop encloses records more than once. An optional is
	/// a repetition of at most one turn and encloses nothing in that sense — which is not
	/// a nicety: `X?` is how the model spells an optional, so counting it would put every
	/// `(':' & port: Digit+)?` in the arena for a second turn that cannot happen.
	/// </remarks>
	static HashSet<Node> Looped(Node body)
	{
		var found   = NodeWalk.ByIdentity([]);
		var pending = new Stack<(Node Node, bool Inside)>();

		pending.Push((body, false));

		while (pending.Count > 0)
		{
			var (node, inside) = pending.Pop();

			if (inside && node is Node.Capture)
				found.Add(node);

			var loops = node is Node.Repeat(_, _, var most) && most != 1;

			foreach (var child in node.Children)
				pending.Push((child, inside || loops));
		}

		return found;
	}

	/// <summary>Whether any rule this machine compiles has a guard that reads a value.</summary>
	/// <remarks>
	/// Asked of this machine's own rules rather than of the whole grammar: the incremental
	/// materializer it turns on is machinery a machine whose rules never ask for a value
	/// mid-parse has no use for, and a sibling machine's guard is not its business.
	/// </remarks>
	bool HasTypedGuards(RecognitionGraph graph)
	{
		foreach (var rule in _rules)
		{
			var layout = CaptureLayout.Of(
				graph.Bodies[rule], other => graph.Results[other].Count > 0 || graph.Types.ContainsKey(other),
				graph.Folds.TryGetValue(rule, out var fold) ? fold.Loop : null);

			foreach (var node in NodeWalk.Descendants(graph.Bodies[rule]))
			{
				if (node is not Node.Guard)
					continue;

				var before = layout.Before(node);

				foreach (var member in graph.Results[rule])
					if (member.Rule is not null)
						foreach (var slot in member.Slots)
							if (slot < before)
								return true;
			}
		}

		return false;
	}

	sealed record RecoveryPlan(
		RuleSymbol Rule, Recovery Recovery, int Slot, int Id, string Method, RuleSymbol? Element);

	public sealed record Factory(
		Node Of,
		string Method,
		IReadOnlyList<ResultMember> Members,
		string? Accumulator = null);

	static int IndexOf(IReadOnlyList<Factory> factories, Node construct)
	{
		for (var i = 0; i < factories.Count; i++)
			if (ReferenceEquals(factories[i].Of, construct))
				return i;

		throw new InvalidOperationException("A construction has no factory.");
	}

	/// <summary>
	/// What the machine needs beside its methods — helper methods, and the arrays a
	/// terminal failure names.
	/// </summary>
	/// <remarks>
	/// An array is written only where something reached <see cref="EmitTerminalFailure"/>
	/// with its name. A site may declare one and then not fail that way — a shared-prefix
	/// run that turns out settled writes neither the later texts nor the catch-all — and
	/// what it left behind was a static field, allocated when the type is first touched and
	/// held for the life of the program. In the URL grammar that was 564 of 1,137.
	/// </remarks>
	public IReadOnlyList<string> Extra
	{
		get
		{
			var kept = new List<string>(_extra);

			foreach (var (name, declaration) in _expected)
				if (_expectedUsed.Contains(name))
					kept.Add(declaration);

			return kept;
		}
	}

	/// <summary>
	/// Whether the generated <c>Parser</c> needs the value-cache fields at all: a typed
	/// guard reads a value before the parse is accepted, and <c>built[]</c> is what tells
	/// an already-materialized value from one still owed.
	/// </summary>
	public bool Caches => _guardValues;

	/// <summary>
	/// Every type a rule's value can have, each with a table of its own to sit in.
	/// </summary>
	/// <remarks>
	/// Ordered, and the order is the index: a value of the type at position <c>i</c> is
	/// written to and read from <c>values{i}</c>. Recovery values join them, because a
	/// recovered element is a value of the same kind as the ones around it.
	/// </remarks>
	public IReadOnlyList<string> ValueTypes => _valueTypes;

	/// <summary>
	/// Take the file's table order instead of this machine's own.
	/// </summary>
	/// <remarks>
	/// A machine names a value type by where it sits in this list, and the parser holds one
	/// table per entry — one parser for the file, however many machines wrote into it. So
	/// the order has to be the union of theirs, which is only known once they all exist.
	/// Called before anything is rendered and after every machine is built; nothing read
	/// during construction depends on it.
	/// </remarks>
	public void ShareValueTables(IReadOnlyList<string> tables)
	{
		_valueTypes.Clear();
		_valueTypes.AddRange(tables);
	}

	readonly List<string> _valueTypes = [];

	/// <summary>
	/// Every type that will be stored, gathered before anything is written.
	/// </summary>
	/// <remarks>
	/// The tables are declared at the top of the code that uses them, so what they are has
	/// to be settled before that line is written rather than discovered while writing the
	/// ones below it. The stores are the authority: a value only ever enters a table through
	/// one of them, and each says the type it is storing.
	/// </remarks>
	void CollectValueTypes()
	{
		foreach (var rule in _rules)
			if (ValueRule(rule) >= 0 && _results.QualifiedOf(rule) is { } type)
				Add(type);

		// A recovered element is a value of the kind the repetition collects, not of the
		// rule that holds the repetition.
		foreach (var plan in _recoveryPlans)
			Add(RecoveredType(plan));

		void Add(string type)
		{
			if (type != "SourceSpan" && !_valueTypes.Contains(type))
				_valueTypes.Add(type);
		}
	}

	/// <summary>
	/// The table a type is kept in, or −1 for a type nothing was gathered for.
	/// </summary>
	/// <remarks>
	/// A spelling that was not gathered has no table, and falls back to the object one it
	/// always used. That costs the boxing this exists to avoid and nothing else — the wrong
	/// answer here would be a table that does not exist, and this cannot give one.
	/// </remarks>
	int TableFor(string type) => _valueTypes.IndexOf(type);

	string RecoveredType(RecoveryPlan plan) =>
		plan.Element is { } element ? _results.ValueOf(element) : _results.ValueOf(plan.Rule);

	public void Register(RuleSymbol root, bool whole)
	{
		// Named from outside the table, so the state it names is a place the parse can begin
		// however little of the grammar reaches it.
		_roots.Add(_entries[root]);

		if (!whole || _wholeEntries.ContainsKey(root))
			return;

		// After the whole-wrapped body there is the end of the input and nothing else —
		// that is what `whole` means, and the engine's Accept enforces it. "Anything" here
		// cost every repetition at the tail of a `parse` its proof.
		_seam      = FollowSets.SeamOf(root, _graph);
		_traceRule = root.Name;

		_wholeEntries[root] = _graph.Trivia.ContainsKey(root)
			? Compile(BodyOf(root, whole: true), Return, FollowSets.Continuation.End)
			: _entries[root];

		_roots.Add(_wholeEntries[root]);
	}

	/// <summary>
	/// A rule's body, wrapped in its leading and trailing <see cref="RecognitionGraph.Trivia"/>
	/// where a whole parse is asked for one and it has it.
	/// </summary>
	/// <remarks>
	/// The one thing <see cref="Register"/> and <see cref="CanLower"/> must never be allowed
	/// to disagree about: whether a rule can be lowered is a fact about the same body
	/// <see cref="Register"/> would otherwise have compiled into the shared table.
	/// </remarks>
	Node BodyOf(RuleSymbol rule, bool whole) =>
		whole && _graph.Trivia.TryGetValue(rule, out var trivia)
			? new Node.Sequence([trivia, _graph.Bodies[rule], trivia])
			: _graph.Bodies[rule];

	/// <summary>
	/// Whether a publication of <paramref name="rule"/> needs none of the three things the
	/// arena is for — no recursion, no backtracking, no deferred construction — and so could
	/// be compiled as an ordinary method instead of a state in the shared automaton.
	/// </summary>
	/// <remarks>
	/// <see cref="Silent"/>'s own recursive definition already is this test: every reachable
	/// call must be inlinable, which already excludes a rule that can reach itself, and
	/// every node kind it has no case for — a capture, a construction, a guard, an external
	/// recognizer, a lookahead, an atomic group — defaults to not silent. Asking it once at
	/// the root asks it of everything reachable.
	/// </remarks>
	public bool CanLower(RuleSymbol rule, bool whole)
	{
		_checkpointsAllowed = true;

		try
		{
			return Silent(BodyOf(rule, whole), whole ? FirstSets.First.End : FirstSets.First.All);
		}
		finally
		{
			_checkpointsAllowed = false;
		}
	}

	public int Register(Node node)
	{
		var state = Compile(node, Return, FollowSets.Continuation.All);

		_roots.Add(state);

		return state;
	}

	/// <summary>The states something outside the table jumps to.</summary>
	readonly HashSet<int> _roots = [];

	IReadOnlyDictionary<RuleSymbol, FollowSets.Continuation> _follow =
		new Dictionary<RuleSymbol, FollowSets.Continuation>();

	FollowSets.Continuation Follows(RuleSymbol rule) =>
		_follow.TryGetValue(rule, out var after) ? after : FollowSets.Continuation.All;

	/// <summary>
	/// The seam of the rule whose body is being compiled — which trivia a leading call
	/// would have to be for the pair's other half to mean anything here.
	/// </summary>
	/// <remarks>
	/// Saved and restored around an inlined call, because an inlined body composes its
	/// continuations against its own namespace's seam, exactly as
	/// <see cref="FollowSets"/>'s walk does at a call it does not inline.
	/// </remarks>
	RuleSymbol? _seam;

	/// <summary>
	/// Whether captures compile to position locals and the construction is left for
	/// Accept — the flat-value rendering, where nothing ever backtracks over either.
	/// Off for the engine, whose captures are arena entries backtracking must unwind.
	/// </summary>
	bool _valuesInLocals;

	/// <summary>Whether any repetition compiled with a standing exit.</summary>
	bool _usesLoopExits;

	/// <summary>Whether any repetition counts its turns, and so has to un-count them.</summary>
	bool _usesTurns;

	/// <summary>
	/// The rule whose body is being compiled, for the trace lines its states carry.
	/// </summary>
	/// <remarks>
	/// Written into each emitted <c>Trace</c> call as a literal, because it is knowable
	/// here and not there: at run time a state is a number, and the engine that owns it
	/// shares those numbers between every rule it inlined. An inlined body traces as the
	/// rule it was inlined into, which is where its states actually live.
	/// </remarks>
	string _traceRule = "";

	/// <summary>The trailing arguments of an emitted trace call: input and rule.</summary>
	string Traced => $", text, \"{Escape(_traceRule)}\"";

	/// <summary>
	/// Whether any construction in this grammar asks for the whole input (§8.2).
	/// </summary>
	/// <remarks>
	/// Asked once and answered for the whole grammar, because what it decides is a
	/// parameter on the engine and on the materializer: a grammar that never names it is
	/// compiled exactly as it was before the name existed. Read out of the C# for the same
	/// reason every other supplied name is — §8.2 matches by name.
	/// </remarks>
	public bool UsesInput { get; private set; }

	string InputParameter => UsesInput ? ", string parserInput" : "";

	string InputArgument  => UsesInput ? ", parserInput" : "";

	/// <summary>
	/// Whether anything in this machine names the grammar's own state (§7.7).
	/// </summary>
	/// <remarks>
	/// Read out of the C# the same way `parserInput` is, and gating the same thing: a
	/// grammar that declares a context and never names it is compiled exactly as one that
	/// declares none, and its publications take no extra argument.
	/// </remarks>
	public bool UsesContext { get; private set; }

	string ContextParameter => UsesContext ? $", {_graph.Context} context" : "";

	string ContextArgument  => UsesContext ? ", context" : "";

	/// <summary>
	/// Every <c>with state</c> site this machine compiled, in the order it reached them —
	/// the index is what an arena entry carries, and the text is the C# that says what the
	/// mark's value is (§7.8).
	/// </summary>
	/// <remarks>
	/// A site rather than a value: what a mark is worth is a C# expression, and the arena
	/// holds ints. So the entry names which site placed it and a generated switch turns
	/// that back into a value — the same trade a <c>Construct</c> entry already makes with
	/// its factory. Nothing here is boxed, and the value's type is whatever the grammar
	/// declared, not something this half had to constrain.
	/// </remarks>
	readonly List<string> _marks = [];

	/// <summary>Whether anything in this machine places a mark.</summary>
	public bool UsesMarks => _marks.Count > 0;

	/// <summary>Whether any factory in it reads the marks standing over it.</summary>
	public bool ReadsState { get; private set; }

	int MarkSite(string text)
	{
		// Two sites writing the same C# are still two sites. Merging them would be sound —
		// they place the same value — but the numbering is what a trace reads back, and a
		// grammar with `checked` written twice should say which of the two ran.
		_marks.Add(text);

		return _marks.Count - 1;
	}

	/// <summary>
	/// What a probe hands over instead, and why it may.
	/// </summary>
	/// <remarks>
	/// A probe exists only for a streamed publication, and a publication whose rules ask
	/// for the input is refused a stream (<c>Retention</c>). So a probe that runs at all
	/// runs over states that never name it — and it recognizes without materializing
	/// besides, which is the other half of the same guarantee.
	/// </remarks>
	const string NoInput = ", null!";

	public static string RenderProbe(string name, string engine, int entry, bool powers, bool input)
	{
		var file = new Writer(0);

		using (file.Block(
			$"static int {name}(global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure)"))
		{
			file.Line("object? ignored;");
			file.Line(
				$"return {engine}(text, pos, {entry}, -1{(powers ? ", 0" : "")}, " +
				$"false, false{(input ? NoInput : "")}, ref failure, out ignored);");
		}

		return file.ToString();
	}

	public static string RenderSyncProbe(string name, string engine, int entry, bool powers, bool input)
	{
		var file = new Writer(0);

		using (file.Block(
			$"static int {name}(global::System.ReadOnlySpan<char> text, int pos)"))
		{
			file.Line($"var failure = new {CSharpEmitter.FailureType}();");
			file.Line("object? ignored;");
			file.Line(
				$"return {engine}(text, pos, {entry}, -1{(powers ? ", 0" : "")}, " +
				$"false, false{(input ? NoInput : "")}, ref failure, out ignored);");
		}

		return file.ToString();
	}

	public string RenderWrapper(RuleSymbol root, string name, string engine, bool whole)
	{
		var file  = new Writer(0);
		var type  = _results.QualifiedOf(root);
		var output = type is null ? "" : $", out {type} value";
		var entry = whole ? _wholeEntries[root] : _entries[root];
		var strength = _graph.Climbing.ContainsKey(root) ? ", int power" : "";
		var enginePower = _graph.Climbing.Count > 0
			? ", " + (_graph.Climbing.ContainsKey(root) ? "power" : "0")
			: "";

		using (file.Block(
			$"static int {name}(global::System.ReadOnlySpan<char> text, int pos, " +
			$"{strength.TrimStart(',', ' ')}{(strength.Length > 0 ? ", " : "")}" +
			$"ref {CSharpEmitter.FailureType} failure{output}{InputParameter}{ContextParameter})"))
		{
			file.Line("object? recognized;");
			file.Line(
				$"var end = {engine}(text, pos, {entry}, {ValueRule(root)}{enginePower}, " +
				$"{(whole ? "true" : "false")}, true{InputArgument}{ContextArgument}, ref failure, out recognized);");

			// An extent root needs nothing that came back: the wrapper handed the position in
			// and was told the position reached, which is the whole of the answer.
			if (IsExtent(root))
				file.Line("value = end < 0 ? default : new SourceSpan(pos, end - pos);");
			else if (type is not null)
				file.Line($"value = end < 0 ? default! : ({type})recognized!;");

			file.Line("return end;");
		}

		return file.ToString();
	}

	public string RenderEngine(string name)
	{
		var file = new Writer(0);
		var strength = _graph.Climbing.Count > 0 ? ", int initialPower" : "";
		var hasValues = false;

		foreach (var rule in _rules)
			hasValues |= ValueRule(rule) >= 0;

		if (hasValues)
			EnsureMaterializer();

		using (file.Block(
			$"static int {name}(global::System.ReadOnlySpan<char> text, int pos, int state, " +
			$"int rootRule{strength}, bool whole, bool materialize{InputParameter}{ContextParameter}, " +
			$"ref {CSharpEmitter.FailureType} failure, out object? recognized)"))
		{
			file.Line("recognized = null;");
			file.Line();

			// Settled before a line of it is written, because what it decides — which states
			// exist — is what says which of the locals below anything still reads.
			PlanLayout();

			file.Line("Parser parser = null!;");
			file.Line("RentParser(ref parser);");
			// Whoever handed it over takes it back: a caller that pools its own gets it
			// returned through the hook, and one that said nothing gets the default pool.
			file.Line("var lent = parser != null;");
			file.Line("parser ??= Recycled();");
			file.Line();

			using (file.Block("try"))
			{
				file.Line("var entries = parser.Entries;");
				file.Line("var p       = pos;");
				file.Line("var call    = -1;");
				file.Line("var atomic  = -1;");
				file.Line("var repeat  = -1;");
				file.Line("var lookahead = -1;");
				if (_graph.Climbing.Count > 0)
					file.Line("var power   = initialPower;");
				if (_recoveries.Count > 0)
				{
					file.Line("var reach   = 0;");
					file.Line("var owned   = false;");
					file.Line("var syncFrom = 0;");
				}

				if (UsesChar)
					file.Line("var c       = '\\0';");
				file.Line("string[]? expected = null;");
				// Set where a room check fails and read where a failure is recorded, so
				// what it says is of the furthest failure and not of any (§7.5).

				// One per repetition written as a loop, and only where the way out that reads
				// it was kept: a turn that cannot fail after consuming has no way back to
				// write, and its state is dropped as unreachable.
				var depths = new HashSet<int>();

				foreach (var turn in _turns)
					if (Written(turn.State))
						depths.Add(turn.Depth);

				for (var i = 0; i <= _depth + _turns.Count; i++)
					if (depths.Contains(i))
						file.Line($"var turn{i} = 0;");
				if (_usesCompleted)
					file.Line("var completedCall = -1;");

				// Declared only where a written state reads or writes it: a rule every
				// call of which was compiled as a site leaves its own states unreachable,
				// and an unused local is a warning in somebody else's build.
				for (var i = 0; i < _captures; i++)
					if (_textCaptures.Contains(i) && !_nestedCaptures.Contains(i) && UsesCapture(i))
						file.Line($"var capture{i} = 0;");

				file.Line();
				file.Line(
					$"entries.Add(new ParserEntry(ParserEntry.Call, {Accept}, pos, -1, -1, -1, -1, " +
					"0, rootRule));");
				file.Line("call = 0;");
				file.Line("Trace(\"enter\", state, p, entries.Count, text, \"\");");

				// The hottest block there is: every return from a rule and every resumption
				// after a failure comes through it. Written here, before the states, rather
				// than after all of them — a jump to the far end of a method this size is a
				// jump out of whatever the processor had ready.
				//
				// Fallen into rather than jumped to: the entry above is the line before it.
				file.Line("Dispatch:");

				using (file.Block("switch (state)"))
				{
					file.Line($"case {Return}: goto Return;");
					file.Line($"case {Accept}: goto Accept;");
					file.Line($"case {Fail}:   expected = null; goto Fail;");

					// Only the states something can actually arrive at through the dispatch,
					// which is far from all of them: see `Dispatched`.
					foreach (var state in Dispatched())
						file.Line($"case {state}: goto {Label(Resolved(state))};");

					file.Line("default: expected = null; goto Fail;");
				}

				RenderStates(file, dispatched: true);

				file.Line();
				file.Line("Return:");
				file.Line("global::System.Diagnostics.Debug.Assert(call >= 0 && call < entries.Count);");
				file.Line("var returned = entries[call];");
				file.Line(
					"global::System.Diagnostics.Debug.Assert(" +
					"returned.Kind == ParserEntry.Call || returned.Kind == ParserEntry.Completed);");
				file.Line("state = returned.State;");
				if (_graph.Climbing.Count > 0)
					file.Line("power = returned.Power;");
				file.Line("var previousCall = returned.CallIndex;");
				if (_usesCompleted)
					file.Line("completedCall = call;");
				file.Line("repeat = returned.RepeatIndex;");
				file.Line("lookahead = returned.LookaheadIndex;");
				file.Line();
				using (file.Block("if (returned.RuleIndex >= 0)"))
				{
					file.Line(
						"entries[call] = new ParserEntry(ParserEntry.Completed, returned.State, " +
						"returned.Position, returned.CallIndex, returned.AtomicIndex, " +
						"returned.RepeatIndex, returned.LookaheadIndex, p, returned.RuleIndex" +
						(_graph.Climbing.Count > 0 ? ", returned.Power" : "") + ");");
				}
				if (Caches)
				{
					using (file.Block("else if (entries.Count == call + 1)"))
					{
						file.Line("parser.Truncate(call, entries);");
						file.Line("entries.RemoveAt(call);");
					}
				}
				else
				{
					file.Line("else if (entries.Count == call + 1)");
					file.Then("entries.RemoveAt(call);");
				}
				file.Line();
				file.Line("call = previousCall;");
				file.Line("Trace(\"return\", state, p, entries.Count, text, \"\");");
				file.Line("goto Dispatch;");

				file.Line();
				file.Line("Accept:");
				file.Line("if (whole && p != text.Length) { expected = null; goto Fail; }");

				if (hasValues || _recoveryPlans.Count > 0)
				{
					using (file.Block("if (materialize)"))
					{
						if (hasValues)
						{
							// A call rather than the walk written out here, and the same call
							// either way. What it buys is the size of the method around it:
							// this one is the whole automaton, and docs/next.md's "Future
							// optimization gate" measured that its size is the one thing
							// still worth moving. It also puts a name on the walk, which is
							// the difference between a profile that can say what
							// materialization costs and one that cannot.
							using (file.Block("if (rootRule >= 0)"))
							{
								file.Line("var values = parser.Materialization(entries.Count);");
								DeclareTables(file);

								if (Caches)
								{
									file.Line("var built  = parser.Materialized();");
									file.Line("if (!built[0]) values[0] = parser;");
								}

								file.Line($"Materialize_DotGram{_tag}(text, parser, entries{InputArgument}{ContextArgument});");
								RootValue(file);
							}
						}
						if (_recoveryPlans.Count > 0)
						{
							if (hasValues)
								file.Line("else");
							using (file.Block(""))
							{
								file.Line();
								ReportRecoveries(file);
							}
						}
					}
				}

				file.Line("return p;");

				file.Line();
				file.Line("Fail:");
				file.Line("if (lookahead < 0 && p > failure.Position)");
				using (file.Block(""))
				{
					file.Line("failure.Position = p;");
					file.Line("failure.Expected = expected;");
					file.Line("failure.ExpectedMore = null;");
				}
				file.Line("else if (lookahead < 0 && p == failure.Position && expected != null)");
				using (file.Block(""))
				{
					file.Line(
						"(failure.ExpectedMore ??= new global::System.Collections.Generic.List<string[]>())" +
						".Add(expected);");
				}
				if (_recoveries.Count > 0)
				{
					file.Line("if (lookahead < 0 && p > reach)");
					file.Then("reach = p;");
				}
				file.Line("Trace(\"fail\", state, p, entries.Count, text, \"\");");
				file.Line();

				using (file.Block("while (entries.Count > 0)"))
				{
					file.Line("var last = entries.Count - 1;");
					file.Line("var entry = entries[last];");
					if (Caches)
						file.Line("parser.Truncate(last, entries);");
					file.Line("entries.RemoveAt(last);");
					file.Line();

					using (file.Block("if (entry.Kind == ParserEntry.Choice)"))
					{
						file.Line("state  = entry.State;");
						file.Line("p      = entry.Position;");
						file.Line("call   = entry.CallIndex;");
						file.Line("atomic = entry.AtomicIndex;");
						file.Line("repeat = entry.RepeatIndex;");
						file.Line("lookahead = entry.LookaheadIndex;");
						file.Line("Trace(\"resume\", state, p, entries.Count, text, \"\");");
						file.Line("goto Dispatch;");
					}

					if (_usesTurns)
					{
						using (file.Block("if (entry.Kind == ParserEntry.TurnDone)"))
						{
							file.Line("var counted = entries[entry.RepeatIndex];");
							file.Line();
							file.Line("global::System.Diagnostics.Debug.Assert(counted.Kind == ParserEntry.Repeat);");
							file.Line(
								"entries[entry.RepeatIndex] = new ParserEntry(ParserEntry.Repeat, 0, " +
								"counted.Position, counted.CallIndex, counted.AtomicIndex, " +
								"counted.RepeatIndex, counted.LookaheadIndex, counted.Value - 1, " +
								"counted.RuleIndex);");
							file.Line("Trace(\"give a turn back\", entry.RepeatIndex, entry.Position, entries.Count, text, \"\");");
							file.Line("continue;");
						}
					}

					if (_usesLoopExits)
					{
						using (file.Block("if (entry.Kind == ParserEntry.LoopExit)"))
						{
							// Only the latest exit of its loop is live. Its Repeat entry —
							// always below it, so always still there — says where the last
							// completed turn ended; an exit standing anywhere else is a
							// turn the parse has since gone past.
							file.Line(
								"if (entry.RepeatIndex < 0 || " +
								"entries[entry.RepeatIndex].Kind != ParserEntry.Repeat || " +
								"entries[entry.RepeatIndex].RuleIndex != entry.Position)");
							file.Then("continue;");
							file.Line();
							file.Line("state  = entry.State;");
							file.Line("p      = entry.Position;");
							file.Line("call   = entry.CallIndex;");
							file.Line("atomic = entry.AtomicIndex;");
							file.Line("repeat = entry.RepeatIndex;");
							file.Line("lookahead = entry.LookaheadIndex;");
							file.Line("Trace(\"resume exit\", state, p, entries.Count, text, \"\");");
							file.Line("goto Dispatch;");
						}
					}

					if (_usesRuns)
					{
						using (file.Block("if (entry.Kind == ParserEntry.Run)"))
						{
							file.Line("if (entry.Value <= entry.Position) continue;");
							file.Line();
							file.Line("state  = entry.State;");
							file.Line("p      = entry.Value - 1;");
							file.Line("call   = entry.CallIndex;");
							file.Line("atomic = entry.AtomicIndex;");
							file.Line("repeat = entry.RepeatIndex;");
							file.Line("lookahead = entry.LookaheadIndex;");
							file.Line(
								"entries.Add(new ParserEntry(ParserEntry.Run, entry.State, entry.Position, " +
								"entry.CallIndex, entry.AtomicIndex, entry.RepeatIndex, " +
								"entry.LookaheadIndex, p));");
							file.Line("Trace(\"shorten run\", state, p, entries.Count, text, \"\");");
							file.Line("goto Dispatch;");
						}

						file.Line();
					}

					if (_captures > 0 || _constructs.Count > 0 || _recoveries.Count > 0 || _usesDead ||
						_marks.Count > 0)
					{
						var ignored =
							"entry.Kind == ParserEntry.Capture || entry.Kind == ParserEntry.Construct || " +
							"entry.Kind == ParserEntry.RuleCapture";

						// An opening is a mark and not a way back, and taking it away again
						// is the whole of what unwinding owes it.
						if (_nestedCaptures.Count > 0)
							ignored += " || entry.Kind == ParserEntry.CaptureOpen";

						// The same: a mark is a record and not a way back, and popping it is
						// everything unwinding owes it.
						if (_marks.Count > 0)
							ignored += " || entry.Kind == ParserEntry.StateSet || " +
								"entry.Kind == ParserEntry.StateEnd";

						if (_recoveries.Count > 0)
							ignored += " || entry.Kind == ParserEntry.Recovery || " +
								"entry.Kind == ParserEntry.PendingRecovery";

						// Passed over rather than acted on: it was a way back until something
						// committed past it, and what it is now is a hole in the stack that
						// keeps the indexes either side of it meaning what they meant.
						if (_recoveries.Count > 0 || _usesDead)
							ignored += " || entry.Kind == ParserEntry.Dead";

						file.Line($"if ({ignored})");
						file.Then("continue;");
						file.Line();
					}

					using (file.Block("if (entry.Kind == ParserEntry.Call || entry.Kind == ParserEntry.Completed)"))
					{
						file.Line("call   = entry.CallIndex;");
						file.Line("atomic = entry.AtomicIndex;");
						file.Line("repeat = entry.RepeatIndex;");
						file.Line("lookahead = entry.LookaheadIndex;");
						if (_graph.Climbing.Count > 0)
							file.Line("power  = entry.Power;");
						file.Line("p      = entry.Position;");
					}
					using (file.Block("else if (entry.Kind == ParserEntry.Atomic)"))
					{
						file.Line("atomic = entry.AtomicIndex;");
						file.Line("repeat = entry.RepeatIndex;");
						file.Line("lookahead = entry.LookaheadIndex;");
					}
					using (file.Block("else if (entry.Kind == ParserEntry.Repeat)"))

					{
						file.Line("p      = entry.Position;");
						file.Line("call   = entry.CallIndex;");
						file.Line("atomic = entry.AtomicIndex;");
						file.Line("repeat = entry.RepeatIndex;");
						file.Line("lookahead = entry.LookaheadIndex;");
					}
					file.Line("else");

					using (file.Block(""))
					{
						file.Line("global::System.Diagnostics.Debug.Assert(entry.Kind == ParserEntry.Lookahead);");
						file.Line("p         = entry.Position;");
						file.Line("call      = entry.CallIndex;");
						file.Line("atomic    = entry.AtomicIndex;");
						file.Line("repeat    = entry.RepeatIndex;");
						file.Line("lookahead = entry.LookaheadIndex;");
						file.Line();
						file.Line("if (entry.Value == 0)");

						using (file.Block(""))
						{
							file.Line("state = entry.State;");
							using (file.Block("if (entry.RuleIndex >= 0)"))
							{
								file.Line(
									"entries.Add(new ParserEntry(ParserEntry.Capture, entry.RuleIndex, p, " +
									"call, atomic, repeat, lookahead, p));");
								file.Line(
									$"Trace(\"capture negative lookahead\", entry.RuleIndex, p, entries.Count, text, \"\");");
							}
							file.Line("Trace(\"negative lookahead succeeds\", state, p, entries.Count, text, \"\");");
							file.Line("goto Dispatch;");
						}
					}
				}

				if (_recoveries.Count > 0 && _starves)
					file.Line("failure.Reach = reach;");

				file.Line();
				file.Line("return -1;");

			}

			file.Line("finally");

			using (file.Block(""))
			{
				file.Line("parser.Reset();");
				file.Line("if (lent) ReturnParser(parser); else Recycle(parser);");
			}
		}

		return file.ToString();
	}

	/// <summary>
	/// Every state <see cref="PlanLayout"/> decided is written, in the order it decided,
	/// each followed by the one it jumps to where that saves the jump.
	/// </summary>
	/// <remarks>
	/// Shared between <see cref="RenderEngine"/> and <see cref="RenderFlat"/>: both write a
	/// state table, one inside the shared automaton and one on its own, and it has to be the
	/// same writing either way — <see cref="PlanLayout"/> already decided what belongs in it.
	/// </remarks>
	/// <param name="dispatched">
	/// Whether a dispatch is written above these states, and so names some of them from
	/// outside. True for the engine; false for a lowered recognizer, which has none.
	/// </param>
	void RenderStates(Writer file, bool dispatched)
	{
		// Which labels anything still names, once the jumps this method is about to drop are
		// gone. A state reached only by falling into it from the one above needs no label,
		// and writing one is a label C# warns about and a consumer's build may refuse. The
		// engine used to be exempt because its dispatch had a case for every written state;
		// now that it has one only for the states that can be resumed at, it needs the same
		// count as anything else — plus the labels those cases name.
		var named = Named();

		if (dispatched)
			foreach (var state in Dispatched())
				named.Add(Resolved(state));

		// Named from outside the state bodies — a checkpoint site's retries, which only
		// the dispatcher below `Fail:` jumps to, and a flat method's entry where the
		// layout did not put it first.
		foreach (var state in _namedOutside)
			named.Add(Resolved(state));

		for (var written = 0; written < _order.Count; written++)
		{
			var i    = _order[written];
			var body = _bodies[i];

			// Chained: what this state ends by jumping to is the state written next, so the
			// jump is the line after it either way.
			if (written + 1 < _order.Count &&
				Tail(body) is { } onward &&
				onward == _order[written + 1] + First)
			{
				body = body.Substring(0, body.LastIndexOf($"goto {Label(onward)};", StringComparison.Ordinal));
			}

			file.Line();

			if (named.Contains(i + First))
				file.Line($"S{i + First}:");

			using (file.Block(""))
				file.Write(body);
		}
	}

	/// <param name="following">
	/// What the input must begin with once this node has matched, as far as that is known
	/// here — <see cref="FirstSets.First.All"/> where it is not. It is what tells a
	/// repetition whether handing input back could ever help, so it is threaded down the
	/// tree rather than looked up: a rule compiled into its caller follows that caller's
	/// text, and the same rule compiled on its own follows whatever any caller has.
	/// </param>
	int Compile(Node node, int next, FollowSets.Continuation following)
	{
		if (_owners.TryGetValue(node, out var owner) &&
			_graph.Climbing.TryGetValue(owner, out var levels) &&
			levels.TryGetValue(node, out var level))
		{
			var inner = CompileUnguarded(node, next, following);
			var state = Reserve(out var writer);

			writer.Line($"if ({level} < power) {{ expected = null; goto Fail; }}");
			writer.Line($"goto {Label(inner)};");

			return state;
		}

		return CompileUnguarded(node, next, following);
	}

	int CompileUnguarded(Node node, int next, FollowSets.Continuation following)
	{
		switch (node)
		{
			case Node.Empty:
				return next;

			case Node.Literal(var value) { IgnoreCase: var ignoreCase }:
			{
				var state     = Reserve(out var writer);
				var arrayName = DeclareExpected([node.ToString()]);

				// Two or more characters are one comparison, not one per character.
				// `SequenceEqual` against a constant string is folded by the JIT into
				// word-sized compares — "abcd" becomes a single 64-bit `cmp` against
				// 0x64006300620061 — and it is bounds-checked once. The chain of
				// `text[p + i]` it replaces was checked once per character despite the
				// room check above having proved every one of them in range: `p + 4 <=
				// Length` does not tell the range-check eliminator that `p + 1 < Length`
				// without also knowing `p` cannot overflow, so it kept all four. Nor could
				// it widen the chain itself — four short-circuiting comparisons are four
				// branches with an order that is observable, and only what the JIT
				// recognizes as one comparison is emitted as one.
				//
				// Case-insensitive stays as it was. What it compares is each character
				// folded, which is not the comparison any span method makes.
				if (value.Length > 1 && !ignoreCase)
				{
					writer.Line($"if ({Short(value.Length)})");
					using (writer.Block(""))
					{
						if (_starves)
							writer.Line("failure.Starved = true;");

						writer.Line("failure.OutOfInput = p + 1;");
						EmitTerminalFailure(writer, _fail, arrayName);
					}

					writer.Line(
						"if (!global::System.MemoryExtensions.SequenceEqual(" +
						$"text.Slice(p, {value.Length}), {Spanned(value)}))");

					using (writer.Block(""))
					{
						// Where the character that did not fit actually is, worked out on a
						// branch already taken rather than on the way in. The comparison has
						// said they differ; this only says where, and nothing reaches it
						// unless the parse is failing anyway.
						Sharpen(writer, value);

						EmitTerminalFailure(writer, _fail, arrayName);
					}

					writer.Line($"p += {value.Length};");
					writer.Line($"goto {Label(next)};");

					return state;
				}

				// The room check and the first character's test fail the same way, so they
				// are one question wherever nothing is written between them — and the only
				// thing that writes between them is starvation, which marks the failure
				// before reporting it and belongs to the length alone. Folded rather than
				// left as two `if`s with the same body, which is what a reader sees.
				var room = _starves || value.Length == 0 ? null : Short(value.Length);

				if (room is null)
				{
					writer.Line($"if ({Short(value.Length)})");
					using (writer.Block(""))
					{
						if (_starves)
							writer.Line("failure.Starved = true;");

						writer.Line("failure.OutOfInput = p + 1;");
						EmitTerminalFailure(writer, _fail, arrayName);
					}
				}

				for (var i = 0; i < value.Length; i++)
				{
					// ToUpperInvariant on an uncased character (a digit, punctuation) returns
					// it unchanged, so one comparison shape covers cased and uncased
					// characters alike — no per-character branching needed.
					var test = ignoreCase
						? $"global::System.Char.ToUpperInvariant({At(i)}) != " +
						  $"{CSharpEmitter.Char(char.ToUpperInvariant(value[i]))}"
						: $"{At(i)} != {CSharpEmitter.Char(value[i])}";

					writer.Line($"if ({(i == 0 && room is not null ? room + " || " : "")}{test})");
					using (writer.Block(""))
					{
						// The position at a terminal failure names where the character that
						// did not fit actually is, not where the whole literal started.
						if (i > 0)
							writer.Line($"p += {i};");

						// Which half of the folded test fired, asked where it is already
						// failing: the room check stays folded into the first character's,
						// so nothing is added to the path that matches. Only where more
						// than one character was wanted — a test of one fails for want of
						// room exactly at the end of the input, and the boundary reads
						// that off the position itself (§7.5).
						if (i == 0 && room is not null && value.Length > 1)
							writer.Line($"if ({room}) failure.OutOfInput = p + 1;");

						EmitTerminalFailure(writer, _fail, arrayName);
					}
				}

				writer.Line($"p += {value.Length};");
				writer.Line($"goto {Label(next)};");

				return state;
			}

			case Node.Element element:
			{
				var state     = Reserve(out var writer);
				var test      = CSharpEmitter.Test(element);
				var arrayName = DeclareExpected([node.ToString()]);

				if (test == "false")
				{
					EmitTerminalFailure(writer, _fail, arrayName);

					return state;
				}

				{
					writer.Line("if ((uint)p >= (uint)text.Length)");
					using (writer.Block(""))
					{
						if (_starves)
							writer.Line("failure.Starved = true;");

						EmitTerminalFailure(writer, _fail, arrayName);
					}
				}

				if (test != "true")
				{
					_usesChar = true;
					writer.Line("c = text[p];");
					writer.Line($"if (!({test}))");
					using (writer.Block(""))
						EmitTerminalFailure(writer, _fail, arrayName);
				}

				writer.Line("p++;");
				writer.Line($"goto {Label(next)};");

				return state;
			}

			case Node.Sequence(var nodes):
			{
				var target = next;
				var after  = following;

				for (var i = nodes.Count - 1; i >= 0; i--)
				{
					target = Compile(nodes[i], target, after);
					after  = FollowSets.Precedes(nodes[i], after, _graph, _seam);
				}

				return target;
			}

			case Node.Choice(var alternatives):
			{
				if (Predictive(alternatives) is { } predicted)
					return CompilePredictedChoice(alternatives, predicted, next, following);

				// The checkpoint class: a way back the locals hold. Admitted by the same
				// three questions `Silent`'s own Choice case asks, in the same order, so
				// the two cannot disagree — a run of literals that never comes back is
				// compiled below as it always was, and only a choice that does need
				// coming back to is given its doors.
				if (LiteralRun(alternatives, alternatives.Count - 1, following.Plain) != alternatives.Count &&
					CheckpointSilent(alternatives, following.Plain))
					return CompileCheckpointChoice(alternatives, next, following);

				var last   = alternatives.Count - 1;
				var run    = LiteralGroup(alternatives, last, following.Plain);
				var target = run > 0
					? CompileLiterals(alternatives, last - run + 1, last, next, Fail)
					: Compile(alternatives[last], next, following);
				var rest   = run > 0 ? Begins(alternatives, last - run + 1, last) : Decidable(alternatives[last]);

				for (var i = last - run - (run > 0 ? 0 : 1); i >= 0; i--)
				{
					// Alternatives that are all text need neither. Nothing is written down to
					// come back to and the position is not moved until one of them has
					// matched, so where they differ is where the next is tried — which is
					// what a common prefix is worth, and it is worth it whether or not there
					// is one.
					if (LiteralGroup(alternatives, i, following.Plain) is var here and > 0)
					{
						var from = i - here + 1;

						rest   = Begins(alternatives, from, i).Or(rest is null ? FirstSets.First.All : rest);
						target = CompileLiterals(alternatives, from, i, next, target);
						i      = from;

						continue;
					}

					var first = Compile(alternatives[i], next, following);
					var mine  = Decidable(alternatives[i]);
					var state = Reserve(out var writer);

					// One character can say two things here, and each saves something
					// different. That this alternative cannot begin here saves going into it;
					// that none of the ones after it can saves the entry that would have let
					// the parse come back for them.
					//
					// Both are kept because both were measured. Keeping only the second — on
					// the reasoning that the first merely repeats the test the alternative
					// makes anyway — came out level with doing neither: what going in costs
					// is not the character test but everything around it, the frame and the
					// setup of a rule that was never going to match.
					//
					// Neither is `Predictive`, which needs every alternative told apart from
					// every other. This asks about one at a time and takes what it is given.
					// And only at a character there is: at the end of the input nothing is
					// asked and the entry is written, which is what always happened.
					if (mine is not null || rest is not null)
					{
						_usesChar = true;

						using (writer.Block("if ((uint)p < (uint)text.Length)"))
						{
							writer.Line("c = text[p];");

							// Not to the next alternative's own test but past it, wherever
							// that test is one this jump has already answered. Reaching it
							// means `c` is outside this alternative's set, so a next one
							// whose set is inside this one asks a question with a known
							// answer and jumps straight on — which was two states and two
							// reads of the same character to arrive where one goes now.
							//
							// The way back written below is untouched, and not because
							// nothing is known there — something is. It is pushed only on
							// the path this test let through, so whatever resumes at it
							// resumes with `c` inside this alternative's set, and the link
							// it names asks a question it could answer. What stops the same
							// trick there is the other way in: the entry is pushed at the
							// end of the input too, where no test ran, and that path goes
							// through the link rather than past it. Skipping it would drop a
							// failure the parse reports, so `expected` would change even
							// though what is accepted would not. Left, with the trap named
							// (docs/next.md).
							if (mine is { } begins)
								writer.Line($"if (!({RangesTest(begins.Ranges)})) goto {Label(Skipped(begins, target))};");

							// The second is read knowing the first did not fire, where there
							// was a first: `c` is in this alternative's own set by then, so
							// what is being asked is whether that set holds anything outside
							// the later ones'. Written as it stood, the two ignored each
							// other and said things that could not be true — `if (!(c ==
							// 'h')) goto X; if (!(c == 'h' || c == 'f')) goto Y;`, where the
							// second cannot fire at all.
							if (rest is { } after)
							{
								if (mine is not { } known)
									writer.Line($"if (!({RangesTest(after.Ranges)})) goto {Label(first)};");
								else if (!known.Overlaps(after))
									writer.Line($"goto {Label(first)};");     // always fires
								else if (!after.Covers(known))               // never fires: not written
									writer.Line($"if (!({RangesTest(after.Ranges)})) goto {Label(first)};");
							}
						}
					}

					writer.Line(
						$"entries.Add(new ParserEntry(ParserEntry.Choice, {target}, p, call, atomic, " +
						"repeat, lookahead, 0));");
					writer.Line($"Trace(\"push choice\", {target}, p, entries.Count{Traced});");
					writer.Line($"goto {Label(first)};");

					if (mine is not null)
						_dispatchers[state] = (mine, target);

					target = state;
					rest   = mine is null || rest is null ? null : mine.Or(rest);
				}

				return target;
			}

			case Node.Capture(_, var body):
			{
				var slot = SlotOf(node);

				// Two locals and nothing else: where the span began, and where it ended.
				// Sound because `CanLowerValued` admitted no shape that could backtrack
				// over a finished capture, and the sentinel start is what tells an
				// optional capture that never ran (null) from one that matched nothing.
				if (_valuesInLocals)
				{
					// A capture of a flat-valued call is a site: the callee's body
					// compiled in place under an instance of its own — the same rule
					// inlined twice may not share locals — with its factory run at
					// Accept, guarded by this slot's sentinel saying the site ran.
					if (SiteCallee(node) is { } called)
					{
						var parent = _flatInstance;
						var site   = _flatInstances++;

						_flatSites.Add(new FlatSite(site, called, parent, slot));
						_flatLocals.Add((parent, slot, true));
						_flatRuleOf[site] = called;
						_flatInstance     = site;

						var siteBody = Compile(_graph.Bodies[called], next, following);

						_flatInstance = parent;

						var siteState = Reserve(out var atSiteOpen);

						atSiteOpen.Line($"flat{parent}_{slot}Start = p;");
						atSiteOpen.Line($"goto {Label(siteBody)};");

						return siteState;
					}

					_flatLocals.Add((_flatInstance, slot, false));

					var flatClose = Reserve(out var atFlatClose);
					var flatInner = Compile(body, flatClose, following);
					var flatState = Reserve(out var atFlatOpen);

					atFlatOpen.Line($"flat{_flatInstance}_{slot}Start = p;");
					atFlatOpen.Line($"goto {Label(flatInner)};");

					atFlatClose.Line($"flat{_flatInstance}_{slot}End = p;");
					atFlatClose.Line($"goto {Label(next)};");

					return flatState;
				}

				// A captured call whose callee is the flat-value shape: the body stands
				// where the call was, its captures record into the site's own slots, and
				// the materializer builds the member from those spans — no Call entry, no
				// Completed rewrite, no RuleCapture, no dispatch (Machine.Sites.cs).
				if (_sites.TryGetValue(node, out var sitePlan))
					return CompileSite((Node.Capture)node, sitePlan, next, following);

				if (body is Node.Lookahead(true, var seen))
					return CompileLookaheadCapture(slot, seen, next);
				if (body is Node.Lookahead(false, var rejected))
					return CompileNegativeLookaheadCapture(slot, rejected, next);

				var close = Reserve(out var atClose);
				var inner = Compile(body, close, following);
				var state = Reserve(out var writer);

				if (body is Node.Call(var capturedRule, _) && ValueRule(capturedRule) >= 0)
				{
					_usesCompleted = true;

					writer.Line($"goto {Label(inner)};");

					// The entry to record is the one the call just turned into, and `Return`
					// has its index in hand at the moment it turns it. Searching back for it
					// read the arena until something matched on four fields — a scan for every
					// capture, over everything the parse had built so far.
					atClose.Line("var capturedCall = completedCall;");
					atClose.Line("global::System.Diagnostics.Debug.Assert(capturedCall >= 0);");
					atClose.Line(
						"global::System.Diagnostics.Debug.Assert(" +
						"entries[capturedCall].Kind == ParserEntry.Completed && " +
						"entries[capturedCall].CallIndex == call && " +
						$"entries[capturedCall].RuleIndex == {_ruleIds[capturedRule]} && " +
						"entries[capturedCall].Value == p);");
					atClose.Line(
						$"entries.Add(new ParserEntry(ParserEntry.RuleCapture, {slot}, capturedCall, " +
						"call, atomic, repeat, lookahead, p));");
					atClose.Line($"Trace(\"rule capture\", {slot}, p, entries.Count{Traced});");
					atClose.Line($"goto {Label(next)};");

					return state;
				}

				// A capture the parse can open again before it closes keeps its start in the
				// arena instead of a variable, because the arena is the only thing
				// backtracking puts back. The opening writes an entry of its own and the
				// close finds it by counting openings against closes, the way brackets are
				// counted — never by marking one closed, because an in-place mark survives
				// backtracking that the close which wrote it does not, and the next way in
				// would find an opening that says it is spoken for.
				if (_nestedCaptures.Contains(slot))
				{
					writer.Line(
						$"entries.Add(new ParserEntry(ParserEntry.CaptureOpen, {slot}, p, " +
						"call, atomic, repeat, lookahead, 0));");
					writer.Line($"Trace(\"open capture\", {slot}, p, entries.Count{Traced});");
					writer.Line($"goto {Label(inner)};");

					atClose.Line("var closed  = 0;");
					atClose.Line("var openedAt = entries.Count - 1;");
					atClose.Line();

					using (atClose.Block("for (; openedAt >= 0; openedAt--)"))
					{
						atClose.Line("var opened = entries[openedAt];");
						atClose.Line();
						atClose.Line($"if (opened.State != {slot}) continue;");
						atClose.Line();

						using (atClose.Block("if (opened.Kind == ParserEntry.Capture)"))
						{
							atClose.Line("closed++;");
							atClose.Line("continue;");
						}

						atClose.Line();
						atClose.Line("if (opened.Kind != ParserEntry.CaptureOpen)");
						atClose.Then("continue;");
						atClose.Line();
						atClose.Line("if (closed == 0)");
						atClose.Then("break;");
						atClose.Line();
						atClose.Line("closed--;");
					}

					atClose.Line();
					atClose.Line("global::System.Diagnostics.Debug.Assert(openedAt >= 0);");
					atClose.Line(
						$"entries.Add(new ParserEntry(ParserEntry.Capture, {slot}, " +
						"entries[openedAt].Position, call, atomic, repeat, lookahead, p));");
					atClose.Line($"Trace(\"capture\", {slot}, p, entries.Count{Traced});");
					atClose.Line($"goto {Label(next)};");

					return state;
				}

				writer.Line($"capture{slot} = p;");
				writer.Line($"goto {Label(inner)};");

				atClose.Line(
					$"entries.Add(new ParserEntry(ParserEntry.Capture, {slot}, capture{slot}, " +
					"call, atomic, repeat, lookahead, p));");
				atClose.Line($"Trace(\"capture\", {slot}, p, entries.Count{Traced});");
				atClose.Line($"goto {Label(next)};");

				return state;
			}

			case Node.Construct(var body, _):
			{
				// The factory runs at Accept, once the whole parse is decided — deferred
				// construction, kept without an entry. Where the body is a choice of
				// constructions, which alternative matched is a tag local written as the
				// alternative closes, and Accept switches on it.
				if (_valuesInLocals)
				{
					var owner = _owners.TryGetValue(node, out var of) ? of : null;

					if (owner is not null &&
						_flatRuleOf.TryGetValue(_flatInstance, out var live) &&
						ReferenceEquals(owner, live) &&
						_factories[owner].Count > 1)
					{
						_flatTags.Add(_flatInstance);

						var tagged = Reserve(out var atTag);

						atTag.Line($"flatWhich{_flatInstance} = {IndexOf(_factories[owner], node)};");
						atTag.Line($"goto {Label(next)};");

						return Compile(body, tagged, following);
					}

					return Compile(body, next, following);
				}

				// The entry answers one question — which construction ran — and a rule
				// with one factory was never going to answer anything else. The
				// materializer calls that factory without looking. A fold keeps its
				// entries: there, each one is an iteration, not a choice.
				if (_owners.TryGetValue(node, out var constructed) &&
					_factories[constructed].Count == 1 &&
					!_graph.Folds.ContainsKey(constructed))
					return Compile(body, next, following);

				var factory = _constructs[node];
				var close   = Reserve(out var atClose);
				var inner   = Compile(body, close, following);
				var state   = Reserve(out var writer);

				writer.Line($"goto {Label(inner)};");
				atClose.Line(
					$"entries.Add(new ParserEntry(ParserEntry.Construct, {factory}, p, " +
					"call, atomic, repeat, lookahead, 0));");
				atClose.Line($"Trace(\"construct\", {factory}, p, entries.Count{Traced});");
				atClose.Line($"goto {Label(next)};");

				return state;
			}

			case Node.Call(var rule, _):
			{
				// A rule that recognizes and remembers nothing costs a call, the way it
				// would in a hand-written parser. Nullable scanners cannot say no, and
				// trivia — the scanner every spaced grammar calls at every seam — is one.
				if (ScannerOf(rule) is { } scanner)
				{
					var scanned = Reserve(out var atScan);

					if (FirstSets.Nullable(_graph.Bodies[rule] is Node.Atomic(var inside)
							? inside
							: _graph.Bodies[rule], _graph))
					{
						atScan.Line($"p = {scanner}(text, p);");
					}
					else
					{
						var arrayName = DeclareExpected([rule.Name]);

						atScan.Line($"var scanned = {scanner}(text, p);");
						atScan.Line("if (scanned < 0)");
						using (atScan.Block(""))
							EmitTerminalFailure(atScan, _fail, arrayName);
						atScan.Line("p = scanned;");
					}

					atScan.Line($"goto {Label(next)};");

					return scanned;
				}

				if (CanInline(rule))
				{
					// The inlined body composes continuations against its own seam. Where
					// that seam is another namespace's, what this site knows past its own
					// is no use inside — the same crossing FollowSets makes at a call it
					// does not inline.
					var outerSeam = _seam;
					var handed    = following;

					_seam = FollowSets.SeamOf(rule, _graph);

					if (!ReferenceEquals(_seam, outerSeam))
						handed = new FollowSets.Continuation(following.Plain, FirstSets.First.All);

					var inlined = Compile(_graph.Bodies[rule], next, handed);

					_seam = outerSeam;

					return inlined;
				}

				var state = Reserve(out var writer);
				var calledPower = _graph.Climbing.ContainsKey(rule)
					? (_graph.Powers.TryGetValue(node, out var requested) ? requested : 0)
					: 0;

				writer.Line("var callIndex = entries.Count;");
				writer.Line(
					$"entries.Add(new ParserEntry(ParserEntry.Call, {next}, p, call, atomic, repeat, " +
					$"lookahead, 0, {ValueRule(rule)}" +
					(_graph.Climbing.Count > 0 ? ", power" : "") + "));");
				writer.Line("call = callIndex;");
				if (_graph.Climbing.Count > 0)
					writer.Line($"power = {calledPower};");
				writer.Line($"Trace(\"call {Escape(rule.Name)}\", {_entries[rule]}, p, entries.Count{Traced});");
				writer.Line($"goto {Label(_entries[rule])};");

				return state;
			}

			case Node.External(var method) { HasValue: var hasValue }:
			{
				var state = Reserve(out var writer);

				// Recognition only ever needs the bool and the moved position — the value,
				// where there is one, is recovered later by re-invoking the method against
				// the recorded start position (Machine.Materialization.cs), not trusted from
				// a call that may run on an abandoned path.
				writer.Line(hasValue
					? $"if (!{method}(text, ref p, out _)) {{ expected = null; goto Fail; }}"
					: $"if (!{method}(text, ref p)) {{ expected = null; goto Fail; }}");
				writer.Line($"goto {Label(next)};");

				return state;
			}

			case Node.Guard(var condition):
			{
				var rule = _owners[node];
				var layout = CaptureLayout.Of(
					_graph.Bodies[rule],
					other => _graph.Results[other].Count > 0 || _graph.Types.ContainsKey(other),
					_graph.Folds.TryGetValue(rule, out var fold) ? fold.Loop : null);
				var before = layout.Before(node);
				var method = $"Recognize_DotGram{_tag}_Guard" + _guards++;
				var helper = new Writer(0);
				var parameters = new List<string>();
				var arguments  = new List<string>();

				// A guard runs at every position the rule reaches it, and what the rule has
				// matched so far is a string built to run it. Built only where the condition
				// names it — most conditions ask about the captures, not about the run.
				if (node is Node.Guard { Text: var guardText } && guardText.Contains("parserText"))
				{
					parameters.Add("string parserText");
					arguments.Add("text.Slice(ruleStart, p - ruleStart).ToString()");
				}

				// The same extent unread. A guard runs before its rule is finished, so this
				// is the rule from where it began to where the parse now stands — which is
				// what "the current rule's span" can mean at a point that is not the end,
				// and the only thing here that says *where* rather than *what*.
				if (node is Node.Guard { Text: var spanning } && spanning.Contains("parserSpan"))
				{
					parameters.Add("SourceSpan parserSpan");
					arguments.Add("new SourceSpan(ruleStart, p - ruleStart)");
				}

				// The grammar's own state (§7.7), where the condition names it. A guard is
				// where one is usually written into: it runs while the text is read, which
				// is the only moment a grammar has in the order it is written.
				if (node is Node.Guard { Text: var stateful } &&
					_graph.ContextOf(rule) is { } contract &&
					CSharpEmitter.Names(stateful, "context"))
				{
					// Typed by this rule's own contract, not the effective type — see the
					// factory's own parameters for why. The argument is the same object
					// either way; passing it upcasts.
					parameters.Add($"{contract} context");
					arguments.Add("context");
				}
				var visible = new List<(ResultMember Member, IReadOnlyList<int> Slots)>();

				foreach (var member in _graph.Results[rule])
				{
					var slots = new List<int>();

					foreach (var slot in member.Slots)
						if (slot < before)
							slots.Add(slot);

					if (slots.Count == 0)
						continue;

					// Only what the condition names. Every one of these is materialized to run
					// it — a rule's value built, a run cut into a string — and a condition
					// asking about one capture was handed all of them. Read as text, like the
					// supplied names above: a name inside a string literal costs one value
					// built for nothing, and reading it exactly would mean lexing C# here.
					if (node is Node.Guard { Text: var asked } &&
						!asked.Contains(ResultTypes.ParameterOf(member)))
					{
						continue;
					}

					var optional = member.IsOptional || slots.Count != member.Slots.Count;

					var parameterType = member.Rule is null
						? "string"
						: _results.ValueOf(member.Rule) + (member.IsSequence ? "[]" : "");

					parameters.Add(
						$"{parameterType}{(optional && !member.IsSequence ? "?" : "")} " +
						ResultTypes.ParameterOf(member));
					arguments.Add($"guardCaptured{visible.Count}");
					visible.Add((member with { IsOptional = optional }, slots));
				}

				helper.Line($"static bool {method}({string.Join(", ", parameters)}) =>");
				CSharpEmitter.Handed(
					helper, _lines, node is Node.Guard { At: var at } ? at : -1, condition + ";");
				_extra.Add(helper.ToString());

				var state = Reserve(out var writer);

				writer.Line("global::System.Diagnostics.Debug.Assert(call >= 0 && call < entries.Count);");
				writer.Line("var ruleStart = entries[call].Position;");

				var hasTyped = false;

				foreach (var item in visible)
					hasTyped |= item.Member.Rule is not null;

				if (hasTyped)
				{
					writer.Line("var guardValues = parser.Materialization(entries.Count);");
					DeclareTables(writer);
					writer.Line("var guardBuilt  = parser.Materialized();");
					writer.Line("var guardNeedsMaterialization = false;");
				}

				for (var memberIndex = 0; memberIndex < visible.Count; memberIndex++)
				{
					var (member, slots) = visible[memberIndex];
					var tests = new List<string>(slots.Count);

					foreach (var slot in slots)
						tests.Add($"candidate.State == {_captureOffsets[rule] + slot}");

					if (member.Rule is not null && member.IsSequence)
					{
						var collected = GuardSequenceTest(rule, slots);

						using (writer.Block("for (var candidateAt = call + 1; candidateAt < entries.Count; candidateAt++)"))
						{
							writer.Line("var candidate = entries[candidateAt];");

							using (writer.Block($"if ({collected})"))
							{
								writer.Line("var guardValueAt = candidate.Kind == ParserEntry.Recovery ? candidateAt : candidate.Position;");
								using (writer.Block("if (!guardBuilt[guardValueAt])"))
								{
									writer.Line("guardValues[guardValueAt] = parser;");
									writer.Line("guardNeedsMaterialization = true;");
								}
							}
						}

						continue;
					}

					writer.Line($"var guardCaptured{memberIndex}At = -1;");

					using (writer.Block("for (var candidateAt = entries.Count - 1; candidateAt > call; candidateAt--)"))
					{
						writer.Line("var candidate = entries[candidateAt];");

						using (writer.Block(
							$"if (candidate.Kind == {(member.Rule is null ? "ParserEntry.Capture" : "ParserEntry.RuleCapture")} && " +
							"candidate.CallIndex == call && " +
							$"({string.Join(" || ", tests)}))"))
						{
							writer.Line($"guardCaptured{memberIndex}At = " +
								(member.Rule is null ? "candidateAt;" : "candidate.Position;"));
							writer.Line("break;");
						}
					}

					if (member.Rule is null)
						writer.Line(
							$"var guardCaptured{memberIndex} = guardCaptured{memberIndex}At < 0 ? " +
							(member.IsOptional ? "null" : "string.Empty") + " : " +
							$"text.Slice(entries[guardCaptured{memberIndex}At].Position, " +
							$"entries[guardCaptured{memberIndex}At].Value - " +
							$"entries[guardCaptured{memberIndex}At].Position).ToString();");
					else
						using (writer.Block(
							$"if (guardCaptured{memberIndex}At >= 0 && !guardBuilt[guardCaptured{memberIndex}At])"))
						{
							writer.Line($"guardValues[guardCaptured{memberIndex}At] = parser;");
							writer.Line("guardNeedsMaterialization = true;");
						}
				}

				if (hasTyped)
				{
					writer.Line(
						$"if (guardNeedsMaterialization) Materialize_DotGram{_tag}(text, parser, " +
						$"entries{InputArgument}{ContextArgument});");

					for (var memberIndex = 0; memberIndex < visible.Count; memberIndex++)
					{
						var (member, slots) = visible[memberIndex];

						if (member.Rule is null)
							continue;

						var type = _results.ValueOf(member.Rule);

						if (!member.IsSequence)
						{
							if (!member.IsOptional)
								writer.Line($"global::System.Diagnostics.Debug.Assert(guardCaptured{memberIndex}At >= 0);");

							writer.Line(member.IsOptional
								? $"{type}? guardCaptured{memberIndex} = guardCaptured{memberIndex}At < 0 ? " +
									$"default({type}?) : " +
									ValueFrom(type, $"guardCaptured{memberIndex}At") + ";"
								: $"var guardCaptured{memberIndex} = " +
									ValueFrom(type, $"guardCaptured{memberIndex}At") + ";");

							continue;
						}

						var tests = new List<string>(slots.Count);

						foreach (var slot in slots)
							tests.Add($"candidate.State == {_captureOffsets[rule] + slot}");

						var collected = GuardSequenceTest(rule, slots);

						writer.Line($"var guardCaptured{memberIndex}Count = 0;");

						using (writer.Block("for (var candidateAt = call + 1; candidateAt < entries.Count; candidateAt++)"))
						{
							writer.Line("var candidate = entries[candidateAt];");
							writer.Line($"if ({collected}) guardCaptured{memberIndex}Count++;");
						}

						writer.Line($"var guardCaptured{memberIndex} = new {type}[guardCaptured{memberIndex}Count];");
						writer.Line($"var guardCaptured{memberIndex}Item = 0;");

						using (writer.Block("for (var candidateAt = call + 1; candidateAt < entries.Count; candidateAt++)"))
						{
							writer.Line("var candidate = entries[candidateAt];");

							using (writer.Block($"if ({collected})"))
							{
								writer.Line("var guardValueAt = candidate.Kind == ParserEntry.Recovery ? candidateAt : candidate.Position;");
								writer.Line(
									$"guardCaptured{memberIndex}[guardCaptured{memberIndex}Item++] = " +
									ValueFrom(type, "guardValueAt") + ";");
							}
						}
					}
				}

				writer.Line($"if (!{method}({string.Join(", ", arguments)})) {{ expected = null; goto Fail; }}");
				writer.Line($"goto {Label(next)};");

				return state;
			}

			case Node.Marked(var body, var text):
			{
				// Two entries and no dispatch: nothing reads a mark while the text is read,
				// so neither of these is a state the engine ever comes back to. The opening
				// stands over what follows until the close is reached, and unwinding needs
				// no case of its own — an abandoned reading takes both away with everything
				// else it wrote, which is the whole of what restoring a mark means here.
				var site   = MarkSite(text);
				var closed = Reserve(out var atClose);
				var inner  = Compile(body, closed, following);
				var opened = Reserve(out var atOpen);

				atOpen.Line(
					$"entries.Add(new ParserEntry(ParserEntry.StateSet, {site}, p, " +
					"call, atomic, repeat, lookahead, 0));");
				atOpen.Line($"Trace(\"set state\", {site}, p, entries.Count{Traced});");
				atOpen.Line($"goto {Label(inner)};");

				atClose.Line(
					$"entries.Add(new ParserEntry(ParserEntry.StateEnd, {site}, p, " +
					"call, atomic, repeat, lookahead, 0));");
				atClose.Line($"Trace(\"end state\", {site}, p, entries.Count{Traced});");
				atClose.Line($"goto {Label(next)};");

				return opened;
			}

			case Node.Atomic(var body):
			{
				// First-match-commits held in locals: each alternative is tried through
				// the give-back door, and the first that matches is final. The same test
				// `Silent`'s own Atomic case asks — recoveries included, whose owned
				// mark only the engine's commit writes — so the two agree.
				if (_recoveries.Count == 0 &&
					(body is Node.Choice(var options)
						? AllSilent(options, following.Plain, sequence: false)
						: Silent(body, following.Plain)))
				{
					// No pending site may open inside: the chain's doors are where a
					// failure goes here, and a way back they jumped past would stand
					// armed for ever. The same flag `Silent`'s Atomic case put down.
					var checkpoints = _checkpointsAllowed;

					_checkpointsAllowed = false;

					if (body is not Node.Choice(var tried) || tried.Count == 1)
					{
						var kept = Compile(body is Node.Choice(var only) ? only[0] : body, next, following);

						_checkpointsAllowed = checkpoints;

						return kept;
					}

					var mine  = _depth++;
					var doors = false;

					// Built back to front: the last alternative fails outward, and each
					// earlier one fails into the door that rewinds and tries the next —
					// unless it fails where it began, in which case the next alternative
					// is the failure target directly. A capture a failed alternative
					// opened is unset on the way through.
					var target = Compile(tried[tried.Count - 1], next, following);

					for (var at = tried.Count - 2; at >= 0; at--)
					{
						List<string>? undone = null;

						if (_valuesInLocals)
							foreach (var descendant in NodeWalk.Descendants(tried[at]))
								if (descendant is Node.Capture)
									(undone ??= []).Add(
										$"flat{_flatInstance}_{_captureSlots[descendant]}Start");

						var saved  = _fail;
						var direct = undone is null && FailsWhereItBegan(tried[at]);

						if (!direct)
							doors = true;

						_fail  = direct ? target : GiveBack(target, mine, out _, undone);
						target = Compile(tried[at], next, following);
						_fail  = saved;
					}

					var entered = Reserve(out var atEnter);

					if (doors)
						atEnter.Line($"turn{mine} = p;");
					atEnter.Line($"goto {Label(target)};");

					_depth = mine;
					_checkpointsAllowed = checkpoints;

					return entered;
				}

				var commit = Reserve(out var atCommit);
				var inner  = Compile(body, commit, following);
				var state  = Reserve(out var writer);

				writer.Line("var atomicIndex = entries.Count;");
				writer.Line("entries.Add(new ParserEntry(ParserEntry.Atomic, 0, p, call, atomic, repeat, lookahead, 0));");
				writer.Line("atomic = atomicIndex;");
				writer.Line($"Trace(\"enter atomic\", {inner}, p, entries.Count{Traced});");
				writer.Line($"goto {Label(inner)};");

				atCommit.Line("global::System.Diagnostics.Debug.Assert(atomic >= 0 && atomic < entries.Count);");
				atCommit.Line("var boundary = entries[atomic];");
				atCommit.Line("global::System.Diagnostics.Debug.Assert(boundary.Kind == ParserEntry.Atomic);");
				if (_recoveries.Count > 0)
					atCommit.Line("owned = true;");

				// The arena holds two unlike things — where the parse could return to, and
				// what it recognised on the way — and committing is about the first only.
				// Taking the length off the end took both, which is why a capture written
				// inside `{ … }` did not come out of it.
				//
				// Where the group recognised nothing worth keeping, the length still comes
				// off: nothing above the boundary is named by anything below it, and a group
				// under a repetition would otherwise leave its entries behind on every turn.
				// Where it did, the ways back are put out and everything stays where it is,
				// because an entry's index is its name — a capture of a rule's value names
				// the entry the call completed into — and closing the gaps would rename them.
				if (KeepsRecords(body))
				{
					_usesDead = true;

					using (atCommit.Block("for (var back = entries.Count - 1; back > atomic; back--)"))
					{
						atCommit.Line("var inside = entries[back];");
						atCommit.Line(
							"if (inside.Kind != ParserEntry.Choice && inside.Kind != ParserEntry.Run && " +
							"inside.Kind != ParserEntry.Lookahead) continue;");
						atCommit.Line(
							"entries[back] = new ParserEntry(ParserEntry.Dead, inside.State, inside.Position, " +
							"inside.CallIndex, inside.AtomicIndex, inside.RepeatIndex, inside.LookaheadIndex, " +
							"inside.Value);");
					}
				}
				else
				{
					if (Caches)
						atCommit.Line("parser.Truncate(atomic, entries);");

					atCommit.Line("entries.RemoveRange(atomic, entries.Count - atomic);");
				}

				atCommit.Line("atomic = boundary.AtomicIndex;");
				atCommit.Line("repeat = boundary.RepeatIndex;");
				atCommit.Line("lookahead = boundary.LookaheadIndex;");
				atCommit.Line($"Trace(\"commit\", {next}, p, entries.Count{Traced});");
				atCommit.Line($"goto {Label(next)};");

				return state;
			}

			case Node.Repeat repeat:
			{
				if (_recoveries.TryGetValue(node, out var recovery))
					return CompileRecoveringRepeat(repeat, recovery, next, following);

				if (SilentRepeat(repeat, following.Plain))
					return CompileSilentRepeat(repeat, next, following);

				return RunTest(repeat.Body) is { } runTest
					? CompileRun(repeat, runTest, next, following)
					: CompileRepeat(repeat, next, following);
			}

			// One comparison against the character behind, and nothing else: no entry,
			// no state of its own beyond this one, and failing it is an ordinary failure.
			case Node.Behind(var boundary):
			{
				var state     = Reserve(out var writer);
				var arrayName = DeclareExpected([node.ToString()]);

				_usesChar = true;

				using (writer.Block("if (p > 0)"))
				{
					writer.Line("c = text[p - 1];");
					writer.Line($"if ({CSharpEmitter.Test(boundary)})");
					using (writer.Block(""))
						EmitTerminalFailure(writer, _fail, arrayName);
				}

				writer.Line($"goto {Label(next)};");

				return state;
			}

			case Node.Lookahead(var isPositive, var body):
			{
				// A silent body needs no entry: entering is a checkpoint local, leaving is
				// putting the position back — the same door a possessive turn leaves by.
				// The same test `Silent`'s own Lookahead case asks, so the two agree.
				if (Silent(body, FirstSets.First.All))
				{
					// A body one character decides needs no checkpoint either: the
					// lookahead is its test and nothing else — no local, no consuming,
					// no rewinding. §4.6 weaves one of these around every word literal,
					// so the boundary of a keyword is one comparison each side.
					if (RunTest(body) is { } asked && asked != "true")
					{
						_usesChar = true;

						var predicate = Reserve(out var atAsk);
						var askedName = DeclareExpected([node.ToString()]);

						if (isPositive)
						{
							atAsk.Line("if ((uint)p >= (uint)text.Length)");
							using (atAsk.Block(""))
								EmitTerminalFailure(atAsk, _fail, askedName);
							atAsk.Line("c = text[p];");
							atAsk.Line($"if (!({asked}))");
							using (atAsk.Block(""))
								EmitTerminalFailure(atAsk, _fail, askedName);
						}
						else
						{
							using (atAsk.Block("if ((uint)p < (uint)text.Length)"))
							{
								atAsk.Line("c = text[p];");
								atAsk.Line($"if ({asked})");
								using (atAsk.Block(""))
									EmitTerminalFailure(atAsk, _fail, askedName);
							}
						}

						atAsk.Line($"goto {Label(next)};");

						return predicate;
					}

					var mine = _depth++;

					// The rewind doors are where the body's outcomes go, and a pending
					// site opened inside would be jumped past — the flag `Silent`'s own
					// Lookahead case put down.
					var checkpoints = _checkpointsAllowed;

					_checkpointsAllowed = false;

					if (isPositive)
					{
						// Body matched: rewind and carry on. Body failed: rewind first,
						// then fail outward — a lookahead does not report how far it
						// looked, and both doors go through the same local.
						var rewind    = GiveBack(next, mine, out var start);
						var outward   = _fail;
						var backOut   = GiveBack(outward, mine, out _);

						_fail = backOut;

						var flatInner = Compile(body, rewind, FollowSets.Continuation.All);

						_fail = outward;

						var flatState = Reserve(out var atFlatEnter);

						atFlatEnter.Line($"{start} = p;");
						atFlatEnter.Line($"goto {Label(flatInner)};");

						_depth = mine;
						_checkpointsAllowed = checkpoints;

						return flatState;
					}

					// Negative: the body failing is this succeeding, so the body's own
					// failures rewind and continue; the body matching is this failing.
					var resume    = GiveBack(next, mine, out var begun);
					var matched   = Reserve(out var atMatched);
					var arrayName = DeclareExpected([node.ToString()]);
					var saved     = _fail;

					_fail = resume;

					var refused = Compile(body, matched, FollowSets.Continuation.All);

					_fail = saved;
					_checkpointsAllowed = checkpoints;

					var entered = Reserve(out var atEnter);

					atEnter.Line($"{begun} = p;");
					atEnter.Line($"goto {Label(refused)};");

					atMatched.Line($"p = {begun};");
					EmitTerminalFailure(atMatched, _fail, arrayName);

					_depth = mine;

					return entered;
				}

				var success = Reserve(out var atSuccess);
				var inner   = Compile(body, success, FollowSets.Continuation.All);
				var state   = Reserve(out var writer);

				writer.Line("var lookaheadIndex = entries.Count;");
				writer.Line(
					$"entries.Add(new ParserEntry(ParserEntry.Lookahead, {next}, p, call, atomic, " +
					$"repeat, lookahead, {(isPositive ? 1 : 0)}));");
				writer.Line("lookahead = lookaheadIndex;");
				writer.Line($"Trace(\"enter {(isPositive ? "positive" : "negative")} lookahead\", {inner}, p, entries.Count{Traced});");
				writer.Line($"goto {Label(inner)};");

				atSuccess.Line("global::System.Diagnostics.Debug.Assert(lookahead >= 0 && lookahead < entries.Count);");
				atSuccess.Line("var looked = entries[lookahead];");
				atSuccess.Line("global::System.Diagnostics.Debug.Assert(looked.Kind == ParserEntry.Lookahead);");
				if (Caches)
					atSuccess.Line("parser.Truncate(lookahead, entries);");
				atSuccess.Line("entries.RemoveRange(lookahead, entries.Count - lookahead);");
				atSuccess.Line("p         = looked.Position;");
				atSuccess.Line("call      = looked.CallIndex;");
				atSuccess.Line("atomic    = looked.AtomicIndex;");
				atSuccess.Line("repeat    = looked.RepeatIndex;");
				atSuccess.Line("lookahead = looked.LookaheadIndex;");
				atSuccess.Line($"Trace(\"lookahead body matched\", {next}, p, entries.Count{Traced});");
				atSuccess.Line($"goto {(isPositive ? Label(next) : "Fail")};");

				return state;
			}

			default:
				throw new InvalidOperationException($"Unsupported unified-automaton node: {node.GetType().Name}.");
		}
	}

	/// <summary>
	/// Whether a call to this rule is compiled as the rule's own code, in place of the call.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A call costs a frame in the arena, a jump away and a dispatch back. None of that buys
	/// anything for a rule that produces no value and cannot reach itself: its body is
	/// ordinary control flow, and control flow is what the caller already is. Expansion
	/// terminates because the call graph beneath a non-recursive rule is a DAG, and what the
	/// duplication costs is code size, which this project spends freely.
	/// </para>
	/// <para>
	/// The conditions are each about something the frame is the only place to keep. A
	/// declared type or a result means a value is materialized at the rule's boundary; a
	/// capture inside the body means a span is recorded against that boundary; recursion
	/// means the depth is bounded by the input rather than by the grammar. Anything else is
	/// a rule only in the source text.
	/// </para>
	/// </remarks>
	bool CanInline(RuleSymbol rule) => _plan.CompiledInPlace.Contains(rule);

	readonly ExecutionPlan _plan;

	int CompileLookaheadCapture(int slot, Node seen, int next)
	{
		var success = Reserve(out var atSuccess);
		var inner   = Compile(seen, success, FollowSets.Continuation.All);
		var state   = Reserve(out var writer);

		writer.Line("var lookaheadIndex = entries.Count;");
		writer.Line(
			$"entries.Add(new ParserEntry(ParserEntry.Lookahead, {next}, p, call, atomic, " +
			"repeat, lookahead, 1));");
		writer.Line("lookahead = lookaheadIndex;");
		writer.Line($"Trace(\"enter captured positive lookahead\", {inner}, p, entries.Count{Traced});");
		writer.Line($"goto {Label(inner)};");

		atSuccess.Line("global::System.Diagnostics.Debug.Assert(lookahead >= 0 && lookahead < entries.Count);");
		atSuccess.Line("var seenTo = p;");
		atSuccess.Line("var looked = entries[lookahead];");
		atSuccess.Line("global::System.Diagnostics.Debug.Assert(looked.Kind == ParserEntry.Lookahead);");
		if (Caches)
			atSuccess.Line("parser.Truncate(lookahead, entries);");
		atSuccess.Line("entries.RemoveRange(lookahead, entries.Count - lookahead);");
		atSuccess.Line("p         = looked.Position;");
		atSuccess.Line("call      = looked.CallIndex;");
		atSuccess.Line("atomic    = looked.AtomicIndex;");
		atSuccess.Line("repeat    = looked.RepeatIndex;");
		atSuccess.Line("lookahead = looked.LookaheadIndex;");
		atSuccess.Line(
			$"entries.Add(new ParserEntry(ParserEntry.Capture, {slot}, p, call, atomic, " +
			"repeat, lookahead, seenTo));");
		atSuccess.Line($"Trace(\"capture lookahead\", {slot}, seenTo, entries.Count{Traced});");
		atSuccess.Line($"goto {Label(next)};");

		return state;
	}

	string GuardSequenceTest(RuleSymbol rule, IReadOnlyList<int> slots)
	{
		var states = new List<string>(slots.Count);

		foreach (var slot in slots)
			states.Add($"candidate.State == {_captureOffsets[rule] + slot}");

		var accepted =
			"candidate.Kind == ParserEntry.RuleCapture && candidate.CallIndex == call && " +
			$"({string.Join(" || ", states)})";
		var recovered = new List<string>();

		foreach (var plan in _recoveryPlans)
			if (plan.Rule == rule && plan.Recovery.Factory is not null)
				foreach (var slot in slots)
					if (plan.Slot == _captureOffsets[rule] + slot)
						recovered.Add(
							$"candidate.Kind == ParserEntry.Recovery && candidate.CallIndex == call && " +
							$"candidate.State == {plan.Id}");

		return recovered.Count == 0
			? accepted
			: $"({accepted}) || ({string.Join(" || ", recovered)})";
	}

	int CompileNegativeLookaheadCapture(int slot, Node rejected, int next)
	{
		var matched = Reserve(out var atMatched);
		var inner   = Compile(rejected, matched, FollowSets.Continuation.All);
		var state   = Reserve(out var writer);

		writer.Line("var lookaheadIndex = entries.Count;");
		writer.Line(
			$"entries.Add(new ParserEntry(ParserEntry.Lookahead, {next}, p, call, atomic, " +
			$"repeat, lookahead, 0, {slot}));");
		writer.Line("lookahead = lookaheadIndex;");
		writer.Line($"Trace(\"enter captured negative lookahead\", {inner}, p, entries.Count{Traced});");
		writer.Line($"goto {Label(inner)};");

		atMatched.Line("global::System.Diagnostics.Debug.Assert(lookahead >= 0 && lookahead < entries.Count);");
		atMatched.Line("var looked = entries[lookahead];");
		atMatched.Line("global::System.Diagnostics.Debug.Assert(looked.Kind == ParserEntry.Lookahead);");
		if (Caches)
			atMatched.Line("parser.Truncate(lookahead, entries);");
		atMatched.Line("entries.RemoveRange(lookahead, entries.Count - lookahead);");
		atMatched.Line("p         = looked.Position;");
		atMatched.Line("call      = looked.CallIndex;");
		atMatched.Line("atomic    = looked.AtomicIndex;");
		atMatched.Line("repeat    = looked.RepeatIndex;");
		atMatched.Line("lookahead = looked.LookaheadIndex;");
		atMatched.Line("expected  = null;");
		atMatched.Line("goto Fail;");

		return state;
	}

	/// <summary>
	/// A run of literal alternatives, read once and decided in place.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Ordinarily a choice writes down where to come back to, goes into the first
	/// alternative, and — when that one fails halfway through — unwinds to the entry, puts
	/// the position back and reads the same characters again from the start.
	/// <c>"abc_x" | "abc_y"</c> reads <c>abc_</c> twice for one character of disagreement.
	/// </para>
	/// <para>
	/// Text alternatives need none of it. What they have in common is read once, and the
	/// position is not moved until one of them has matched whole — so where two of them
	/// differ is simply where the next one is tried, with nothing written down and nothing
	/// unwound. The prefix is what makes it cheap; not moving the position is what makes it
	/// possible, and that holds whether there is a prefix or not.
	/// </para>
	/// </remarks>
	/// <summary>
	/// What is left of the alternatives that continue one already matched, tried in the order
	/// they were written.
	/// </summary>
	/// <remarks>
	/// Reached only by resuming the way back <see cref="CompileLiterals"/> pushes once the
	/// shorter alternative has matched and moved the position, so the characters they share
	/// are behind it and none of them is compared again.
	/// </remarks>
	int CompileCarries(int matched, IReadOnlyList<string> carries, IReadOnlyList<string> displays, int next, int fail)
	{
		var state     = Reserve(out var writer);
		var arrayName = DeclareExpected(displays);

		foreach (var carry in carries)
		{
			var rest  = carry.Substring(matched);
			var tests = new List<string> { Room(rest.Length) };

			if (rest.Length > 1)
				tests.Add(
					$"global::System.MemoryExtensions.SequenceEqual(text.Slice(p, {rest.Length}), " +
					$"{Spanned(rest)})");
			else
				tests.Add($"{At(0)} == {CSharpEmitter.Char(rest[0])}");

			using (writer.Block($"if ({string.Join(" && ", tests)})"))
			{
				writer.Line($"p += {rest.Length};");
				writer.Line($"goto {Label(next)};");
			}
		}

		EmitTerminalFailure(writer, fail, arrayName);

		return state;
	}

	int CompileLiterals(IReadOnlyList<Node> alternatives, int from, int to, int next, int fail)
	{
		var texts    = new List<string>(to - from + 1);
		var displays = new List<string>(to - from + 1);

		for (var i = from; i <= to; i++)
			if (alternatives[i] is Node.Literal(var text))
			{
				texts.Add(text);
				displays.Add(alternatives[i].ToString());
			}

		var shared = texts[0];

		foreach (var text in texts)
		{
			var common = 0;

			while (common < shared.Length && common < text.Length && shared[common] == text[common])
				common++;

			shared = shared.Substring(0, common);
		}

		var state     = Reserve(out var writer);
		var arrayName = DeclareExpected(displays);

		if (shared.Length > 1)
		{
			// One comparison rather than one per character, the same trade Node.Literal makes
			// for a literal of its own and for the same reason: SequenceEqual against a
			// constant is folded into word-sized compares and bounds-checked once, where the
			// chain of text[p + i] it replaces was checked once per character despite the room
			// test above having proved every one of them in range.
			writer.Line($"if ({Short(shared.Length)})");
			using (writer.Block(""))
			{
				if (_starves)
					writer.Line("failure.Starved = true;");

				writer.Line("failure.OutOfInput = p + 1;");
				EmitTerminalFailure(writer, fail, arrayName);
			}

			writer.Line(
				"if (!global::System.MemoryExtensions.SequenceEqual(" +
				$"text.Slice(p, {shared.Length}), {Spanned(shared)}))");

			using (writer.Block(""))
			{
				// Name the character that did not fit, not where the shared prefix started —
				// worked out on a branch the comparison has already failed rather than on the
				// way in, so what it costs is a cost of failing.
				//
				// Only where the failure goes to `Fail:`, which puts the position back from
				// the arena. A prefix conflict chains several of these runs through `fail`,
				// and the run this jumps to reads from where this one started.
				if (fail == Fail)
					Sharpen(writer, shared);

				EmitTerminalFailure(writer, fail, arrayName);
			}
		}
		else if (shared.Length == 1)
		{
			// A single character has nothing to widen. Its test and the room check fail the
			// same way, so they are folded into one question wherever starvation does not
			// have to be marked between them — the same as Node.Literal's own.
			var room = _starves ? null : Short(1);

			if (room is null)
			{
				writer.Line($"if ({Short(1)})");
				using (writer.Block(""))
				{
					writer.Line("failure.Starved = true;");
					EmitTerminalFailure(writer, fail, arrayName);
				}
			}

			writer.Line($"if ({(room is not null ? room + " || " : "")}{At(0)} != {CSharpEmitter.Char(shared[0])})");
			using (writer.Block(""))
			{
				EmitTerminalFailure(writer, fail, arrayName);
			}
		}

		// One of the texts may be the shared prefix itself — a shorter alternative that
		// begins a longer one, admitted by `PrefixSettled` because nothing can come back
		// for it. Its own test is then empty, which is to say it matches wherever the
		// shared prefix did: it takes the position unconditionally, and neither the
		// alternatives written after it nor the catch-all below can be reached. Writing
		// them anyway is a CS0162 in somebody else's build.
		var settled = false;

		foreach (var text in texts)
		{
			// Not written at all where something before it is its own beginning: this chain
			// is reached by falling past the earlier tests, and a text that continues one of
			// them cannot match where that one did not. It is reached instead by the way
			// back that one pushes, which is where its remainder is compared.
			var reached = true;

			foreach (var earlier in texts)
			{
				if (ReferenceEquals(earlier, text))
					break;

				reached &= !text.StartsWith(earlier, StringComparison.Ordinal);
			}

			if (!reached)
				continue;

			var tests = new List<string>();

			if (text.Length > shared.Length)
				tests.Add(Room(text.Length));

			// What is left of this alternative once the shared prefix is behind it, as one
			// comparison rather than one per character — the same trade `Node.Literal`
			// makes, and for the same reason: the room test above has proved every one of
			// these in range and the range-check eliminator will not take its word for it,
			// while `SequenceEqual` against a constant is checked once and compared a
			// machine word at a time. Nothing is sharpened here, and nothing needs to be:
			// an alternative that does not fit is not a failure, it is the next
			// alternative. Where they all fail to fit, the catch-all below walks them
			// together to find how far any of them got.
			var rest = text.Substring(shared.Length);

			if (rest.Length > 1)
				tests.Add(
					"global::System.MemoryExtensions.SequenceEqual(text.Slice(" +
					$"{(shared.Length == 0 ? "p" : $"p + {shared.Length}")}, {rest.Length}), " +
					$"{Spanned(rest)})");
			else if (rest.Length == 1)
				tests.Add($"{At(shared.Length)} == {CSharpEmitter.Char(rest[0])}");

			settled = tests.Count == 0;

			// Everything written after this one that continues it. `"http"` matching does
			// not mean `"https"` cannot: ordered choice prefers the one written first and
			// comes back for the longer if the parse goes wrong past here.
			var carries = new List<string>();

			foreach (var later in texts.GetRange(texts.IndexOf(text) + 1, texts.Count - texts.IndexOf(text) - 1))
				if (later.Length > text.Length && later.StartsWith(text, StringComparison.Ordinal))
					carries.Add(later);

			using (writer.Block(settled ? "" : $"if ({string.Join(" && ", tests)})"))
			{
				if (text.Length > 0)
					writer.Line($"p += {text.Length};");

				// Pushed after the position has moved, which is the whole point: what
				// resumes there resumes past the characters this alternative matched, and
				// the continuation compares only its own remainder. Written the other way
				// round the parse would compare `"http"` a second time to find `"https"`.
				if (carries.Count > 0)
				{
					var carry = CompileCarries(text.Length, carries, displays, next, fail);

					writer.Line(
						$"entries.Add(new ParserEntry(ParserEntry.Choice, {carry}, p, call, atomic, " +
						"repeat, lookahead, 0));");
					writer.Line($"Trace(\"push choice\", {carry}, p, entries.Count{Traced});");
				}

				writer.Line($"goto {Label(next)};");
			}

			if (settled)
				break;
		}

		// The shared-prefix guards above name the full set of `texts`, which is right:
		// where the shared prefix itself did not fit, none of them did and none got
		// further than another. This catch-all is where they differ, and `SharpenAll`
		// walks them together — moving to the deepest character any of them agreed with
		// and naming the ones that were still agreeing there.
		//
		// One known gap remains: where a prefix conflict (`"p" | "q" | "pr"`) splits one
		// grammar-level choice into several entry-less `CompileLiterals` runs chained by
		// `fail`, a later run's own `expected` can overwrite an earlier one's before
		// either reaches the real `Fail:` — under-reporting, never mis-attributing or
		// over-reporting.
		if (!settled)
		{
			// How far any of them agreed, and which of them were still agreeing there —
			// which together decide whether this failure or another one is the one
			// reported, and what it says. The shared-prefix guard above sharpens its own
			// branch; this covers the characters past it, where the alternatives differ
			// and each was compared whole.
			if (fail == Fail)
			{
				_expectedUsed.Add(arrayName);
				writer.Line($"expected = {arrayName};");

				SharpenAll(writer, texts, displays);

				writer.Line($"goto {Label(fail)};");
			}
			else
				EmitTerminalFailure(writer, fail, arrayName);
		}

		return state;
	}

	/// <summary>
	/// A choice whose way back lives in three locals instead of an arena entry — the
	/// checkpoint class. C, E and F of the Minimal catalog are the shapes it exists for.
	/// </summary>
	/// <remarks>
	/// <para>
	/// What an arena entry holds for a choice is where to resume, from where, and which
	/// ways back were pending before it — and for a choice no repetition sits over, each
	/// is one local: <c>way</c> is the position, <c>alt</c> the next alternative to try,
	/// <c>over</c> the site that was pending before this one opened. <c>pending</c> is
	/// the dispatcher's stack pointer: any later failure, the continuation's included,
	/// goes to <c>Fail:</c>, which records it and resumes the innermost open site — the
	/// engine's pop, without the engine.
	/// </para>
	/// <para>
	/// No repetition may stand over a site, because one set of locals holds one
	/// activation; <see cref="Deterministic"/> already refuses the choice, so a silent
	/// repetition never contains one, and <see cref="_checkpointsAllowed"/> is put down
	/// inside every construct that routes failure around <c>Fail:</c>. Re-entering the
	/// site from an outer resume runs its entry again, which re-arms all three locals.
	/// </para>
	/// </remarks>
	int CompileCheckpointChoice(
		IReadOnlyList<Node> alternatives, int next, FollowSets.Continuation following)
	{
		var id      = ++_checkpointIds;
		var entries = new int[alternatives.Count];
		var retries = new int[alternatives.Count - 1];

		for (var at = alternatives.Count - 1; at >= 0; at--)
			entries[at] = Compile(alternatives[at], next, following);

		// One state per alternative after the first: rewind, say which comes after it,
		// and go in. Reached only from the dispatcher below `Fail:`, which no state
		// names — so each is a root of its own, or the layout would drop it.
		for (var at = 1; at < alternatives.Count; at++)
		{
			var retry = Reserve(out var atRetry);

			atRetry.Line($"p = way{id};");
			atRetry.Line($"alt{id} = {at + 1};");
			atRetry.Line($"goto {Label(entries[at])};");

			retries[at - 1] = retry;
			_roots.Add(retry);
			_namedOutside.Add(retry);
		}

		_checkpoints.Add(new CheckpointSite(id, alternatives.Count, retries));

		var state = Reserve(out var writer);

		writer.Line($"way{id} = p;");
		writer.Line($"alt{id} = 1;");
		writer.Line($"over{id} = pending;");
		writer.Line($"pending = {id};");
		writer.Line($"goto {Label(entries[0])};");

		return state;
	}

	/// <summary>
	/// A choice one character decides: read it, jump to the alternative it belongs to.
	/// </summary>
	int CompilePredictedChoice(
		IReadOnlyList<Node> alternatives, string[] tests, int next, FollowSets.Continuation following)
	{
		var targets = new int[alternatives.Count];

		var advanced = new bool[alternatives.Count];

		for (var i = 0; i < alternatives.Count; i++)
			if (CompileTested(alternatives[i], tests[i], next, following) is { } entered)
				(targets[i], advanced[i]) = (entered, true);
			else
				targets[i] = Compile(alternatives[i], next, following);

		var state = Reserve(out var writer);

		_usesChar = true;

		// Predicted by disjoint first sets (Predictive), which already proved every
		// alternative's first set is known and finite — none is Anything, Nothing or
		// nullable, or Predictive would have refused to predict at all — so the union of
		// their ranges is exactly what this position accepts, on either failure path
		// below.
		var arrayName = DeclareExpected([PredictedDisplay(alternatives)]);

		writer.Line("if ((uint)p >= (uint)text.Length)");
		using (writer.Block(""))
		{
			if (_starves)
				writer.Line("failure.Starved = true;");

			EmitTerminalFailure(writer, _fail, arrayName);
		}

		writer.Line("c = text[p];");

		for (var i = 0; i < targets.Length; i++)
			writer.Line(advanced[i]
				? $"if ({tests[i]}) {{ p++; goto {Label(targets[i])}; }}"
				: $"if ({tests[i]}) goto {Label(targets[i])};");

		EmitTerminalFailure(writer, _fail, arrayName);

		return state;
	}

	/// <summary>
	/// Where an alternative continues once a leading terminal the dispatch has already tested
	/// is stepped over — or <c>null</c> where it does not begin with exactly that terminal.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A predicted dispatch reads <c>text[p]</c>, proves it in range, and tests it against
	/// each alternative's first set. An alternative that is a terminal, or begins with one,
	/// then asked all three questions over again — its own bounds check, its own load, its
	/// own comparison — about a character nothing had moved past. The loops a grammar spends
	/// its time in are exactly this shape: <c>(Unreserved | SubDelim | PctEncoded | ':')*</c>
	/// paid for it once per character of every host and every path segment.
	/// </para>
	/// <para>
	/// So the dispatch advances and jumps past that terminal itself, and what comes back here
	/// is where to land. Nothing is shared and nothing is rewritten — the untested form is
	/// simply never compiled, and this is the only edge that would have reached it. A
	/// predicted choice writes no way back and no arena entry names the state, so there is no
	/// second way in for a resume to arrive by.
	/// </para>
	/// <para>
	/// The <c>p++</c> goes in the dispatch's own branch rather than into a state of its own,
	/// which is not a matter of taste. A state is a block the resume switch can jump to, so
	/// it survives every optimizer as a block, and a two-line one between a test and its
	/// continuation is a block the profile-guided layout can put anywhere. Measured with one:
	/// four inputs faster and the fifth 9% slower, and the fifth turned into a 6% gain the
	/// moment dynamic PGO was switched off — an unmistakeable signature of layout rather than
	/// of work (docs/next.md).
	/// </para>
	/// <para>
	/// The two tests are compared as text on purpose. Identical text is the whole of what has
	/// to hold — the same expression over the same <c>c</c> — and where the first-set builder
	/// and the terminal's own builder word the same set differently, this declines and the
	/// ordinary form is compiled. Wrong is not among the answers; missed is. The one wording
	/// difference bridged here is the outer bracket: <see cref="CSharpEmitter.Test"/> wraps
	/// what it builds and <c>RangesTest</c> does not, and a negated element's <c>!(…)</c>
	/// cannot collide with the bracketed form whatever it contains.
	/// </para>
	/// </remarks>
	int? CompileTested(Node node, string test, int next, FollowSets.Continuation following)
	{
		// Asked before anything is written: a sequence's tail would otherwise be compiled
		// only to be abandoned when its head turned out not to qualify, and an abandoned
		// state is still a state in the method.
		//
		// "true" and "false" cannot arrive here — Predictive refuses a first set that is
		// Anything or Nothing — but neither would be a terminal anybody had tested.
		if (test is "true" or "false" || !BeginsWith(node, test))
			return null;

		return Entered(node, next, following);

		int Entered(Node at, int to, FollowSets.Continuation after) =>
			at switch
			{
				// The terminal itself contributes no state: the dispatch steps over it.
				Node.Element => to,

				// The same step <see cref="Compile"/> takes for a call it inlines, so the
				// body is entered exactly where the ordinary form would have entered it.
				Node.Call(var rule, _) => Entered(_graph.Bodies[rule], to, after),

				Node.Sequence(var nodes) => Threaded(nodes, to, after),

				// BeginsWith admits these three and nothing else.
				_ => throw new InvalidOperationException($"{at.GetType().Name} does not begin with a terminal."),
			};

		int Threaded(IReadOnlyList<Node> nodes, int to, FollowSets.Continuation after)
		{
			var target = to;
			var rest   = after;

			for (var i = nodes.Count - 1; i >= 1; i--)
			{
				target = Compile(nodes[i], target, rest);
				rest   = FollowSets.Precedes(nodes[i], rest, _graph, _seam);
			}

			return Entered(nodes[0], target, rest);
		}
	}

	/// <summary>
	/// Whether the node begins with exactly the terminal a dispatch has already tested.
	/// </summary>
	/// <remarks>
	/// Looks through an inlined call, because that is what compiling one does — the rule
	/// <c>Unreserved = [Digit | 'a'..'z' | 'A'..'Z' | '-' | '.' | '_' | '~']</c> is a
	/// character class wearing a name, and an alternative that is a call to it begins with
	/// its element as surely as if the class had been written in place.
	/// </remarks>
	bool BeginsWith(Node node, string test) =>
		node switch
		{
			Node.Element element     => CSharpEmitter.Test(element) == $"({test})",
			Node.Call(var rule, _)   => CanInline(rule) &&
			                            _graph.Bodies.TryGetValue(rule, out var body) &&
			                            BeginsWith(body, test),
			Node.Sequence(var nodes) => nodes.Count > 0 && BeginsWith(nodes[0], test),
			_                        => false,
		};

	/// <summary>What a predicted choice's disjoint first sets accept, rendered as one element set.</summary>
	string PredictedDisplay(IReadOnlyList<Node> alternatives)
	{
		var ranges = new List<CharRange>();

		foreach (var alternative in alternatives)
			ranges.AddRange(FirstSets.Of(alternative, _graph).Ranges);

		return new Node.Element(false, ranges, [], []).ToString();
	}

	/// <summary>
	/// A repetition of a single-character body, compiled as a run: one scan, one entry.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The general form pays a list append and two struct rewrites per character, because it
	/// must be able to resume the body at each iteration. Here the body cannot be resumed —
	/// it either matched that one character or it did not — so the only thing a later failure
	/// can ask for is a shorter run. The states a shorter run can take are the interval from
	/// the minimum to the end reached, and an interval is two integers rather than a stack of
	/// them.
	/// </para>
	/// <para>
	/// So the scan is a plain loop over the span, and what it leaves behind is one
	/// <c>Run</c> entry holding the floor and the end. Failing back into it hands one
	/// character back and re-enters the continuation, which is exactly what unwinding the
	/// per-iteration choices did, at one entry instead of one per character. The entry is
	/// only written at all when the run is longer than the minimum: a run with nothing to
	/// give back leaves no trace.
	/// </para>
	/// </remarks>
	int CompileRun(Node.Repeat repeatNode, string test, int next, FollowSets.Continuation following)
	{
		var (_, min, max) = repeatNode;

		if (max == 0)
			return next;

		var state = Reserve(out var writer);

		_usesRuns = true;

		writer.Line("var runStart = p;");

		using (writer.Block("while (true)"))
		{
			if (max is { } limit)
				writer.Line($"if (p - runStart >= {limit}) break;");

			if (_starves)
			{
				writer.Line("if ((uint)p >= (uint)text.Length)");
				using (writer.Block(""))
				{
					writer.Line("failure.Starved = true;");
					writer.Line("break;");
				}
			}
			else
				writer.Line("if ((uint)p >= (uint)text.Length) break;");

			if (test != "true")
			{
				_usesChar = true;
				writer.Line("c = text[p];");
				writer.Line($"if (!({test})) break;");
			}

			writer.Line("p++;");
		}

		var floor = min == 0 ? "runStart" : $"runStart + {min}";

		if (min > 0)
		{
			var arrayName = DeclareExpected([repeatNode.Body.ToString()]);

			writer.Line($"if (p < {floor})");
			using (writer.Block(""))
				EmitTerminalFailure(writer, Fail, arrayName);
		}

		writer.Line($"if (p > {floor})");
		writer.Then(
			$"entries.Add(new ParserEntry(ParserEntry.Run, {next}, {floor}, " +
			"call, atomic, repeat, lookahead, p));");

		writer.Line($"Trace(\"run\", {next}, p, entries.Count{Traced});");
		writer.Line($"goto {Label(next)};");

		return state;
	}

	/// <summary>
	/// A repetition that is a loop and nothing else: no entry, no count, no way back.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Everything the arena was doing for a repetition is gone here, and each piece for its
	/// own reason. The resume points are gone because <see cref="Possessive"/> proved nothing
	/// would come back for them. The entry that held the count is gone because the required
	/// turns are written out one after another instead of counted. And the way out is a plain
	/// jump because <see cref="Silent"/> proved the body leaves nothing behind that failing
	/// past the dispatcher would strand.
	/// </para>
	/// <para>
	/// What is left is the loop the grammar meant: match, go round, and leave by the door
	/// when the input stops matching. A required turn keeps the ordinary failure, because
	/// failing one of those is the repetition failing rather than ending.
	/// </para>
	/// </remarks>
	/// <summary>
	/// The way out of a turn that failed after consuming: put the position back to where the
	/// turn began, then leave.
	/// </summary>
	/// <remarks>
	/// A body of one character cannot fail after consuming, which is why the run form needs
	/// none of this. A body of several parts can — <c>'%' &amp; Hex &amp; Hex</c> eats the
	/// per cent and then finds no digit — and the general machinery puts the position back
	/// out of the entry it wrote. Written out as a loop there is no entry, so where the turn
	/// began is kept in a local of its own and the way out reads it. The repetition ends
	/// where its last whole turn ended, not where a broken one stopped.
	/// </remarks>
	/// <summary>
	/// Whether every failure of this node happens before it has moved the position — so
	/// the door that puts the position back would put back what never changed.
	/// </summary>
	/// <remarks>
	/// One character is the shape: the test refuses before <c>p++</c>. A literal longer
	/// than one is not, and not only for the obvious reason — its failure branch moves
	/// <c>p</c> to the character that did not fit, for the diagnostic.
	/// </remarks>
	static bool FailsWhereItBegan(Node node) =>
		node switch
		{
			Node.Empty or Node.Behind => true,
			Node.Element              => true,
			Node.Literal(var text)    => text.Length == 1,
			Node.Atomic(var kept)     => FailsWhereItBegan(kept),
			Node.Marked(var kept, _)  => FailsWhereItBegan(kept),
			_                         => false,
		};

	int GiveBack(int next, int depth, out string start, IReadOnlyList<string>? resetLocals = null)
	{
		start = "turn" + depth;

		var state = Reserve(out var writer);

		writer.Line($"p = {start};");

		// A capture the given-back turn opened is a record backtracking would have
		// unwound; kept in locals, the door it leaves by has to unset it (the sentinel
		// is what tells an optional capture that never happened from one that did).
		if (resetLocals is not null)
			foreach (var local in resetLocals)
				writer.Line($"{local} = -1;");

		writer.Line($"goto {Label(next)};");

		_turns.Add((depth, state));

		return state;
	}

	int CompileSilentRepeat(Node.Repeat repeatNode, int next, FollowSets.Continuation following)
	{
		var (body, min, max) = repeatNode;
		var inside = new FollowSets.Continuation(
			FirstSets.Of(body, _graph).Or(following.Plain), FirstSets.First.All);
		var target = next;

		// A turn's failure leaves by the loop's door, and one set of site locals holds
		// one activation — the flag `SilentRepeat`'s own body question put down.
		var checkpoints = _checkpointsAllowed;

		_checkpointsAllowed = false;

		// One local per depth rather than per repetition. Two of them are live at once only
		// where one of these is written inside another, and that nests two or three deep in
		// the grammars there are — where the count of them was sixteen, all live at once in a
		// method that already keeps the position, the frame, the arena indexes and the
		// character in locals. Registers run out long before sixteen, and what pays for that
		// is every line of the method, not the loops.
		var mine = _depth++;

		// The capture locals a given-back turn would leave set (flat-value rendering
		// only; the engine's captures unwind with the arena). A site's inner locals
		// need no reset of their own — its parent slot here is the guard they sit
		// behind at Accept.
		List<string>? resets = null;

		if (_valuesInLocals)
			foreach (var descendant in NodeWalk.Descendants(body))
				if (descendant is Node.Capture)
					(resets ??= []).Add($"flat{_flatInstance}_{_captureSlots[descendant]}Start");

		// A body whose every failure happens where its turn began needs no door at all:
		// there is nothing to put back, and the way out is a plain jump.
		var direct = resets is null && FailsWhereItBegan(body);

		if (max is null)
		{
			var loop  = Reserve(out var atLoop);
			var saved = _fail;

			// Round again, or out — and out is through the door that puts the position back.
			_fail = direct ? next : GiveBack(next, mine, out _, resets);

			var inner = Compile(body, loop, inside);

			_fail = saved;

			if (!direct)
				atLoop.Line($"turn{mine} = p;");
			atLoop.Line($"goto {Label(inner)};");

			target = loop;
		}
		else
			for (var turn = min; turn < max; turn++)
			{
				var saved = _fail;
				var after = target;

				_fail  = direct ? after : GiveBack(after, mine, out _, resets);
				target = Compile(body, after, inside);
				_fail  = saved;

				var began = Reserve(out var atBegan);

				if (!direct)
					atBegan.Line($"turn{mine} = p;");
				atBegan.Line($"goto {Label(target)};");

				target = began;
			}

		for (var turn = 0; turn < min; turn++)
			target = Compile(body, target, inside);

		_depth = mine;
		_checkpointsAllowed = checkpoints;

		return target;
	}

	int CompileRepeat(Node.Repeat repeatNode, int next, FollowSets.Continuation following)
	{
		var (body, min, max) = repeatNode;

		if (max == 0)
			return next;

		// One standing way out instead of one per turn. The proof is NeverGivesBack's;
		// what it buys is that a failure behind the repetition resumes one exit and pops
		// past the stale ones, where it used to resume every one of them and re-read the
		// suffix per turn — the exponential this engine was measured to have.
		// An optional is a repetition of one, and its skip is a way back like any other:
		// three of them in a row are eight readings of the same failure. The mechanism
		// already handles a bounded loop — the count-exit marks the standing exit spent —
		// so nothing excludes them.
		var settled = NeverGivesBack(repeatNode, following);

		// A settled optional whose body one character decides needs no arena at all. The
		// character says whether the body is entered; entered, it must finish, because
		// skipping instead would ask the continuation to begin on the character the test
		// let through — which is what settled rules out. What was a Repeat entry, a
		// standing exit, a count and their unwinding per optional becomes one comparison,
		// and the grammar of the notation carries several optionals per operand.
		if (settled && min == 0 && max == 1 && Decidable(body) is { } begins)
		{
			_usesChar = true;

			var entered = Compile(body, next, following);
			var state   = Reserve(out var atTest);

			using (atTest.Block("if ((uint)p < (uint)text.Length)"))
			{
				atTest.Line("c = text[p];");
				atTest.Line($"if ({RangesTest(begins.Ranges)}) goto {Label(entered)};");
			}

			atTest.Line($"goto {Label(next)};");

			return state;
		}


		if (settled)
			_usesLoopExits = true;

		var exit  = Reserve(out var atExit);
		var loop  = Reserve(out var atLoop);
		var after = Reserve(out var atAfter);
		var entry = Reserve(out var atEntry);

		// What a turn is followed by: another turn, or what the repetition is followed
		// by — unless there is no other turn to be followed by (§FollowSets, the same
		// reasoning).
		var inner = Compile(
			body, after,
			max == 1
				? following
				: new FollowSets.Continuation(
					FirstSets.Of(body, _graph).Or(following.Plain),
					FirstSets.Of(body, _graph).Or(following.Plain)));

		atEntry.Line("var repeatIndex = entries.Count;");
		atEntry.Line("entries.Add(new ParserEntry(ParserEntry.Repeat, 0, p, call, atomic, repeat, lookahead, 0));");
		atEntry.Line("repeat = repeatIndex;");
		atEntry.Line($"Trace(\"enter repeat\", {loop}, p, entries.Count{Traced});");
		atEntry.Line($"goto {Label(loop)};");

		if (settled || min > 0 || max is not null)
		{
			atLoop.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
			atLoop.Line("var repeating = entries[repeat];");
			atLoop.Line("global::System.Diagnostics.Debug.Assert(repeating.Kind == ParserEntry.Repeat);");
		}

		if (max is { } limit)
			atLoop.Line($"if (repeating.Value >= {limit}) goto {Label(exit)};");

		if (settled)
		{
			// The standing exit, and the note of where it stands. The Repeat entry's
			// second field holds where the last completed turn ended, which is what makes
			// every earlier LoopExit visibly stale; it is not a state and Layout's
			// Resumable list must not think it is.
			using (atLoop.Block(min == 0 ? "" : $"if (repeating.Value >= {min})"))
			{
				atLoop.Line(
					$"entries.Add(new ParserEntry(ParserEntry.LoopExit, {exit}, p, call, atomic, " +
					"repeat, lookahead, 0));");
				atLoop.Line(
					"entries[repeat] = new ParserEntry(ParserEntry.Repeat, 0, repeating.Position, " +
					"repeating.CallIndex, repeating.AtomicIndex, repeating.RepeatIndex, " +
					"repeating.LookaheadIndex, repeating.Value, p);");
				atLoop.Line($"Trace(\"stand exit\", {exit}, p, entries.Count{Traced});");
			}
		}
		else if (min == 0)
			PushRepeatExit(atLoop, exit);
		else
		{
			atLoop.Line($"if (repeating.Value >= {min})");
			atLoop.Then(
				$"entries.Add(new ParserEntry(ParserEntry.Choice, {exit}, p, call, atomic, repeat, " +
				"lookahead, 0));");
		}

		atLoop.Line($"goto {Label(inner)};");

		// The count is only ever read to decide whether a bound has been reached. An
		// unbounded repetition with nothing to reach has no such decision to make, and
		// counting for a reader that does not exist costs a read and a write of the entry
		// on every iteration. A settled repetition goes through here whatever its bounds:
		// the standing exit's position is written by the loop head it is about to re-enter.
		if (min > 0 || max is not null)
		{
			_usesTurns = true;

			atAfter.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
			atAfter.Line("var repeated = entries[repeat];");
			atAfter.Line(
				"entries[repeat] = new ParserEntry(ParserEntry.Repeat, 0, repeated.Position, " +
				"repeated.CallIndex, repeated.AtomicIndex, repeated.RepeatIndex, " +
				"repeated.LookaheadIndex, repeated.Value + 1" + (settled ? ", repeated.RuleIndex" : "") + ");");

			// The count rewritten above survives backtracking; this is what does not. A
			// resume into this turn's own machinery pops it, and popping it takes the
			// turn back out of the count before the body re-completes and counts again.
			atAfter.Line(
				"entries.Add(new ParserEntry(ParserEntry.TurnDone, 0, p, call, atomic, repeat, " +
				"lookahead, 0));");
		}

		atAfter.Line($"goto {Label(loop)};");

		// Out through the count rather than through the standing exit, which is the one
		// path that leaves it standing and valid. Marked spent, or a failure after the
		// repetition would come back and end a run that has already ended.
		if (settled && max is not null)
		{
			atExit.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
			atExit.Line("var closing = entries[repeat];");
			atExit.Line(
				"entries[repeat] = new ParserEntry(ParserEntry.Repeat, 0, closing.Position, " +
				"closing.CallIndex, closing.AtomicIndex, closing.RepeatIndex, " +
				"closing.LookaheadIndex, closing.Value, -1);");
		}

		LeaveRepeat(atExit, next);

		// A repetition that may take nothing, met where its body cannot begin, takes
		// nothing — and one character says so before the machinery is built, where the
		// machinery is a Repeat entry, a way out, a failed probe and their unwinding.
		// The same test a choice link makes before going into an alternative, kept for
		// the same measured reason; unlike the settled form above, entering commits to
		// nothing — every way back the general machinery keeps is still kept. The body
		// must not match empty: taking nothing and matching nothing differ by exactly
		// the records §10 tells apart, and only the first may be chosen here.
		var barred = Decidable(body) is { } heads
			? RangesTest(heads.Ranges)
			: !FirstSets.Nullable(body, _graph)
				? EntryTest(body)
				: null;

		if (min == 0 && barred is { } could && could != "true")
		{
			_usesChar = true;

			var probed = Reserve(out var atProbe);

			using (atProbe.Block("if ((uint)p < (uint)text.Length)"))
			{
				atProbe.Line("c = text[p];");
				atProbe.Line($"if ({could}) goto {Label(entry)};");
			}

			atProbe.Line($"goto {Label(next)};");

			return probed;
		}

		return entry;
	}

	/// <summary>
	/// The test over <c>c</c> for whether a node could begin at the character standing
	/// here — or null where that is not knowable as one test.
	/// </summary>
	/// <remarks>
	/// Wider than <see cref="Decidable"/> on purpose: that one goes through a
	/// <see cref="FirstSets.First"/>, and a Unicode category is a few hundred ranges no
	/// rendering should spell out — where the same category as an element is one
	/// classification call (<see cref="CSharpEmitter.Test"/>). So this walks the node
	/// instead, to the first thing that must consume, and lets each leaf write the test
	/// it already knows how to write. Sound in one direction only: the test may admit
	/// more than the body would (a nullable head contributes alongside what follows it),
	/// never less — a false positive builds machinery that was going to be built anyway.
	/// </remarks>
	string? EntryTest(Node node, HashSet<RuleSymbol>? seen = null)
	{
		switch (node)
		{
			case Node.Literal(var text) { IgnoreCase: false } when text.Length > 0:
				return $"c == {CSharpEmitter.Char(text[0])}";

			case Node.Element element:
			{
				var test = CSharpEmitter.Test(element);

				return test == "false" ? null : test;
			}

			case Node.Capture(_, var captured):  return EntryTest(captured, seen);
			case Node.Construct(var built, _):   return EntryTest(built, seen);
			case Node.Atomic(var kept):          return EntryTest(kept, seen);
			case Node.Marked(var kept, _):       return EntryTest(kept, seen);

			case Node.Repeat(var repeated, var least, _):
				return least > 0 ? EntryTest(repeated, seen) : null;

			case Node.Call(var called, _):
			{
				seen ??= [];

				if (!seen.Add(called) || !_graph.Bodies.TryGetValue(called, out var calledBody))
					return null;

				var inner = EntryTest(calledBody, seen);

				seen.Remove(called);

				return inner;
			}

			case Node.Choice(var alternatives):
			{
				var tests = new List<string>(alternatives.Count);

				foreach (var alternative in alternatives)
				{
					if (EntryTest(alternative, seen) is not { } one)
						return null;

					if (one == "true")
						return "true";

					tests.Add(one);
				}

				return Joined(tests);
			}

			case Node.Sequence(var parts):
			{
				var tests = new List<string>();

				foreach (var part in parts)
				{
					if (part is Node.Empty or Node.Lookahead or Node.Behind or Node.Guard)
						continue;

					if (EntryTest(part, seen) is not { } head)
						return null;

					if (head == "true")
						return "true";

					tests.Add(head);

					// A part that must consume settles what the sequence begins with;
					// a nullable one and the next could each be what actually begins.
					if (!FirstSets.Nullable(part, _graph))
						break;
				}

				return tests.Count == 0 ? null : Joined(tests);
			}

			default:
				return null;
		}

		static string Joined(List<string> tests) =>
			tests.Count == 1
				? tests[0]
				: string.Join(" || ", tests.Select(static test => $"({test})"));
	}

	void LeaveRepeat(Writer writer, int next)
	{
		writer.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
		writer.Line("var finished = entries[repeat];");
		writer.Line("global::System.Diagnostics.Debug.Assert(finished.Kind == ParserEntry.Repeat);");
		writer.Line("var previousRepeat = finished.RepeatIndex;");
		if (Caches)
		{
			using (writer.Block("if (entries.Count == repeat + 1)"))
			{
				writer.Line("parser.Truncate(repeat, entries);");
				writer.Line("entries.RemoveAt(repeat);");
			}
		}
		else
			writer.Line("if (entries.Count == repeat + 1) entries.RemoveAt(repeat);");
		writer.Line("repeat = previousRepeat;");
		writer.Line("lookahead = finished.LookaheadIndex;");
		writer.Line($"Trace(\"leave repeat\", {next}, p, entries.Count{Traced});");
		writer.Line($"goto {Label(next)};");
	}

	int ValueRule(RuleSymbol rule) =>
		_graph.Results[rule].Count > 0 || _graph.Types.ContainsKey(rule) ? _ruleIds[rule] : -1;

	/// <summary>
	/// The states something still names once the chained jumps are dropped.
	/// </summary>
	/// <remarks>
	/// A jump is dropped exactly where its target is the state written next, which is what
	/// laying the states out in execution order made common — so this has to be worked out
	/// against the same order rather than against the bodies as compiled.
	/// </remarks>
	HashSet<int> Named()
	{
		var named = new HashSet<int>();

		for (var written = 0; written < _order.Count; written++)
		{
			var body = _bodies[_order[written]];
			var next = written + 1 < _order.Count ? _order[written + 1] + First : -1;
			var tail = Tail(body);

			foreach (Match match in Gotos.Matches(body))
			{
				var target = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);

				// The one occurrence this method drops is the trailing jump to the next
				// state, and only when it is that state.
				if (target == next && target == tail && body.LastIndexOf($"goto {Label(target)};", StringComparison.Ordinal) == match.Index)
					continue;

				named.Add(target);
			}
		}

		return named;
	}

	/// <summary>
	/// Where a jump taken because <c>c</c> is outside <paramref name="known"/> should land.
	/// </summary>
	/// <remarks>
	/// A choice is a chain, and each link begins by asking whether its own alternative can
	/// start here. Arriving because the link before it said no is arriving with that
	/// question already answered, wherever this link's set is inside the previous one's —
	/// the two literals of <c>"http" | "https"</c> both begin with <c>h</c>, so the second
	/// link asked whether <c>c</c> was <c>h</c> having been reached only when it was not.
	/// Followed to the first link that could say something new.
	/// </remarks>
	int Skipped(FirstSets.First known, int target)
	{
		var at    = target;
		var steps = 0;

		while (_dispatchers.TryGetValue(at, out var link) &&
			known.Covers(link.Mine) &&
			steps++ <= _dispatchers.Count)
		{
			at = link.Target;
		}

		return at;
	}

	/// <summary>Each choice link's own first set, and where it goes when that set says no.</summary>
	readonly Dictionary<int, (FirstSets.First Mine, int Target)> _dispatchers = [];

	/// <summary>Whether any state's body jumps to this one.</summary>
	/// <remarks>
	/// Asked of the first state written and of nothing else, so what
	/// <see cref="RenderStates"/> strips as it goes cannot change the answer: a trailing
	/// jump is dropped only where it names the state written next, and nothing is written
	/// before the first.
	/// </remarks>
	bool Entered(int state)
	{
		var jump = $"goto {Label(state)};";

		foreach (var body in _bodies)
			if (body is not null && body.Contains(jump))
				return true;

		return false;
	}

	/// <summary>
	/// The character <paramref name="offset"/> along from the position, said the short way
	/// at zero.
	/// </summary>
	/// <remarks>
	/// <c>text[p + 0]</c> and <c>text[p]</c> are one instruction either way; the difference
	/// is that somebody reads this file. The same goes for <see cref="Short"/> and
	/// <see cref="Room"/> beside it — a literal of one character asks whether there is one
	/// character left, and <c>p + 1 &gt; text.Length</c> is the general form of that
	/// question rather than the question.
	/// </remarks>
	static string At(int offset) => offset == 0 ? "text[p]" : $"text[p + {offset}]";

	/// <summary>Whether the input is too short for <paramref name="count"/> more.</summary>
	/// <remarks>
	/// Unsigned at one, which is not a flourish. Every one of these guards is followed by a
	/// read of <c>text[p]</c>, and that read carries a bounds check of its own — an unsigned
	/// one, because that is how a bounds check is written. A signed <c>p &gt;= text.Length</c>
	/// beside it is a different comparison, so the range-check eliminator keeps both, and the
	/// disassembly of a character run showed exactly that: <c>cmp/jge</c> and then <c>cmp/jae</c>
	/// on the same two registers, once per character. Written the same way, they are one
	/// comparison and the throw path disappears with them.
	///
	/// Nothing is given up. <c>p</c> is never negative — it starts at a position and moves
	/// forward or is restored from one — and were it ever to be, the unsigned form refuses
	/// where the signed one would have read out of bounds.
	/// </remarks>
	/// <remarks>
	/// <para>
	/// More than one is asked the other way round — <c>text.Length - p</c> against the
	/// count, rather than <c>p + count</c> against the length. Two reasons, and the first is
	/// not a nicety.
	/// </para>
	/// <para>
	/// <c>p + count</c> can overflow. A span may hold <c>int.MaxValue</c> characters, so a
	/// position near the end plus a literal's length wraps negative, the check passes, and
	/// what was an ordinary refusal to match becomes an exception out of a slice. It takes a
	/// four-gigabyte input to reach and it is still a wrong answer rather than a slow one.
	/// Subtracting cannot overflow: both sides are non-negative and the difference is
	/// between them.
	/// </para>
	/// <para>
	/// Signed, and deliberately, where the single-character form above is unsigned. There
	/// the unsigned comparison is the same one the indexer's own bounds check makes, which
	/// is the whole point of writing it that way. Here it would be wrong: were <c>p</c> ever
	/// past the end, <c>text.Length - p</c> is negative, and casting that to
	/// <c>uint</c> makes it enormous — the check would report room where there is none,
	/// which is the one direction a room check may not fail in.
	/// </para>
	/// </remarks>
	static string Short(int count) =>
		count == 1 ? "(uint)p >= (uint)text.Length" : $"text.Length - p < {count}";

	/// <summary>Whether there is room for <paramref name="count"/> more.</summary>
	/// <remarks>The same the other way up — see <see cref="Short"/>.</remarks>
	static string Room(int count) =>
		count == 1 ? "(uint)p < (uint)text.Length" : $"text.Length - p >= {count}";

	/// <summary>The literal as a span, for a comparison to be made against.</summary>
	/// <remarks>
	/// <c>AsSpan</c> written out rather than left to the implicit conversion, which arrived
	/// with .NET Core 2.1: the emitted file may land in a <c>netstandard2.0</c> compilation,
	/// where <c>string</c> does not convert to <c>ReadOnlySpan&lt;char&gt;</c> on its own and
	/// this is a compile error in somebody else's build. <c>DotGram.Compatibility</c> is
	/// there to catch exactly this, and did.
	/// </remarks>
	static string Spanned(string value) =>
		$"global::System.MemoryExtensions.AsSpan({Quoted(value)})";

	/// <summary>The literal as C# source, with anything unprintable spelled out.</summary>
	/// <remarks>
	/// Everything outside printable ASCII goes as an escape rather than as itself. This
	/// file is written by us and read by a compiler that is not, and a literal newline or a
	/// U+2028 inside a string is what breaks one build and not another.
	/// </remarks>
	static string Quoted(string value)
	{
		var text = "";

		foreach (var c in value)
			text += c switch
			{
				'\\' => "\\\\",
				'"'  => "\\\"",
				_    => c is >= ' ' and <= '~' ? c.ToString() : $"\\u{(int)c:X4}",
			};

		return $"\"{text}\"";
	}

	/// <summary>
	/// Move <c>p</c> to the character of a literal that did not fit, knowing one of them
	/// did not.
	/// </summary>
	/// <remarks>
	/// Written only inside a branch the comparison has already failed, so what it costs is
	/// a cost of failing. The last character needs no test of its own: if every earlier one
	/// matched and the whole did not, it is the one.
	/// </remarks>
	/// <summary>
	/// The same for a run of them: move <c>p</c> to the deepest character any of these
	/// still agreed with, knowing that none of them matched whole.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A failed literal's position is not a caret, it is a selector: <c>Fail:</c> keeps the
	/// furthest failure and reports that one's expectation, so how far a literal got is what
	/// decides whether the reader is told the one thing they almost wrote or every thing
	/// that could have stood there. A run of alternatives left unsharpened reported the
	/// start of the run — `"abcdef" | "abq"` against `abcdez` naming both, where five
	/// characters into the first is plainly the better answer.
	/// </para>
	/// <para>
	/// Written as the walk of a trie rather than as one <see cref="Sharpen"/> per text,
	/// because the alternatives share their beginnings and so should share the reading of
	/// them. On the failing branch only, like everything else here.
	/// </para>
	/// </remarks>
	void SharpenAll(Writer writer, IReadOnlyList<string> texts, IReadOnlyList<string> displays)
	{
		// Out of line, and that is a measurement rather than tidiness. Written into the
		// recognizer, this cold walk sat between hot states and cost the URL corpus five
		// per cent on inputs that never reach it — a method of ten thousand lines has
		// nowhere to put anything without moving something else. A call on the failing
		// branch costs nothing that is not already failing.
		var method = $"Recognize_DotGram{_tag}_Sharpen" + _sharpens++;
		var helper = new Writer(0);

		helper.Line(
			$"static int {method}(global::System.ReadOnlySpan<char> text, int p, " +
			"ref string[]? expected)");

		using (helper.Block(""))
		{
			var body = new Writer(0);

			Deepest([.. Enumerable.Range(0, texts.Count)], 0, body);

			helper.Write(body.ToString());
			helper.Line("return p;");
		}

		_extra.Add(helper.ToString());

		writer.Line($"p = {method}(text, p, ref expected);");

		void Deepest(IReadOnlyList<int> here, int depth, Writer writer)
		{
			var branches = here
				.Where(one => texts[one].Length > depth)
				.GroupBy(one => texts[one][depth])
				.ToList();

			if (branches.Count == 0)
				return;

			var first = true;

			foreach (var branch in branches)
			{
				// `p` moves as the walk descends, so every level reads the character it
				// stands on and steps one — which is also why the room test is the same
				// one at every level.
				writer.Line(
					$"{(first ? "if" : "else if")} ({Room(1)} && {At(0)} == {CSharpEmitter.Char(branch.Key)})");

				using (writer.Block(""))
				{
					writer.Line("p += 1;");

					// And which of them are still agreeing, which is the other half of what
					// a reader is told. Written only where the walk has actually narrowed
					// the set: naming the same texts again would be one more assignment for
					// nothing.
					if (branch.Count() < here.Count)
					{
						var narrowed = DeclareExpected([.. branch.Select(one => displays[one])]);

						_expectedUsed.Add(narrowed);
						writer.Line($"expected = {narrowed};");
					}

					Deepest([.. branch], depth + 1, writer);
				}

				first = false;
			}
		}
	}

	static void Sharpen(Writer writer, string value)
	{
		writer.Line($"if ({At(0)} == {CSharpEmitter.Char(value[0])})");

		if (value.Length == 2)
		{
			writer.Then("p += 1;");

			return;
		}

		using (writer.Block(""))
		{
			for (var i = 1; i < value.Length - 1; i++)
			{
				writer.Line($"{(i == 1 ? "if" : "else if")} ({At(i)} != {CSharpEmitter.Char(value[i])})");
				writer.Then($"p += {i};");
			}

			writer.Line("else");
			writer.Then($"p += {value.Length - 1};");
		}
	}

	int Reserve(out Writer writer)
	{
		writer = new Writer(0);
		_states.Add(writer);

		return _states.Count - 1 + First;
	}

	bool KeepsRecords(Node body)
	{
		foreach (var node in NodeWalk.Descendants(body))
			switch (node)
			{
				case Node.Capture:
				case Node.Construct:
					return true;

				case Node.Call(var rule, _) when ValueRule(rule) >= 0:
					return true;
			}

		return false;
	}

	/// <summary>Whether a rule's value is where it matched rather than something built.</summary>
	static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

	/// <summary>
	/// A precomputed, once-per-occurrence table of what a terminal test would have
	/// wanted, named so the site that failed can point at it.
	/// </summary>
	/// <remarks>
	/// One array per occurrence, not deduplicated by content — the same shape as every
	/// other per-occurrence `Reserve`, and for the same reason: two occurrences of `'a'`
	/// in different places are two different things that can fail.
	/// </remarks>
	string DeclareExpected(IReadOnlyList<string> display)
	{
		var items = string.Join(", ", display.Select(d => $"\"{EscapeExpected(d)}\""));

		// The same set asked for twice is the same array. Two terminals that accept the
		// same thing are commonplace — a rule called from two places, a character class
		// written out in two alternatives — and each used to get a field of its own.
		if (_expectedByItems.TryGetValue(items, out var already))
			return already;

		var name = $"Recognize_DotGram{_tag}_Expected" + _expectedCount++;

		_expectedByItems[items] = name;
		_expected.Add((name, $"static readonly string[] {name} = {{ {items} }};"));

		return name;
	}

	/// <summary>
	/// Wider than <see cref="Escape"/>: this embeds <see cref="Node.ToString"/> output —
	/// arbitrary grammar text — rather than a rule name, and needs the same per-character
	/// care <see cref="CSharpEmitter.Char"/> already takes for a matched character, not
	/// just <c>\</c>/<c>"</c>. A multi-character <see cref="Node.Literal"/> does not
	/// escape control characters the way a single one does through
	/// <see cref="CharRange.Quote"/>, and even a single one only escapes the common
	/// cases — U+2028 LINE SEPARATOR, "legal in a grammar, not in C# source"
	/// (<c>SemanticTests.cs</c>'s own words for it), is one of the characters C# itself
	/// treats as a newline inside a string literal and would otherwise break the file
	/// this lands in.
	/// </summary>
	static string EscapeExpected(string value)
	{
		var text = "";

		foreach (var character in value)
			text += character switch
			{
				'\\'              => "\\\\",
				'"'               => "\\\"",
				>= ' ' and <= '~' => character.ToString(),
				'\0'              => "\\0",
				'\a'              => "\\a",
				'\b'              => "\\b",
				'\f'              => "\\f",
				'\n'              => "\\n",
				'\r'              => "\\r",
				'\t'              => "\\t",
				'\v'              => "\\v",
				_                 => $"\\u{(int)character:X4}",
			};

		return text;
	}

	/// <summary>
	/// A failure that names what would have fit — a terminal test's own `goto`, with
	/// `expected` set right before it so <c>Fail:</c> knows what to blame this on.
	/// </summary>
	void EmitTerminalFailure(Writer writer, int fail, string arrayName)
	{
		_expectedUsed.Add(arrayName);

		writer.Line($"expected = {arrayName};");
		writer.Line($"goto {Label(fail)};");
	}

	/// <summary>
	/// A failure that is not a terminal test — a binding-power guard, a `when`, an
	/// external recognizer, leftover input, and the rest of the "clear" sites this
	/// mechanism has to account for. Must clear `expected` rather than leave whatever the
	/// last terminal test set: `Fail:` cannot otherwise tell a stale value from one that
	/// belongs to this failure.
	/// </summary>
	static void EmitFailure(Writer writer, int fail)
	{
		writer.Line("expected = null;");
		writer.Line($"goto {Label(fail)};");
	}

	static void PushRepeatExit(Writer writer, int exit) =>
		writer.Line(
			$"entries.Add(new ParserEntry(ParserEntry.Choice, {exit}, p, call, atomic, repeat, lookahead, 0));");

	static string Label(int state) => state switch
	{
		Return => "Return",
		Accept => "Accept",
		Fail   => "Fail",
		_      => "S" + state,
	};
}
