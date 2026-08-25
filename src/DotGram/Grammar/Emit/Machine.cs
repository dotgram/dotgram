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
	readonly Dictionary<RuleSymbol, IReadOnlyList<Factory>> _factories = [];
	readonly Dictionary<Node, int> _constructs = new(NodeIdentity.Instance);
	readonly Dictionary<Node, RecoveryPlan> _recoveries = new(NodeIdentity.Instance);
	readonly List<RecoveryPlan> _recoveryPlans = [];
	readonly Dictionary<RuleSymbol, int> _wholeEntries = [];
	readonly List<string> _extra = [];
	int _expectedCount;
	readonly ILineMap? _lines;
	readonly bool _starves;
	bool _usesChar;
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
	int _fail = Fail;
	bool _materializer;
	bool _guardValues;
	int _guards;
	int _captures;

	public Machine(RecognitionGraph graph, ResultTypes results, ILineMap? lines, bool starves = false)
	{
		_graph = graph;
		_results = results;
		_lines = lines;
		_starves = starves;
		_guardValues = HasTypedGuards(graph);

		foreach (var rule in graph.Rules)
		{
			var layout = CaptureLayout.Of(
				graph.Bodies[rule], other => graph.Results[other].Count > 0 || graph.Types.ContainsKey(other));
			var factories = CSharpEmitter.FactoriesOf(graph, results, rule);

			_captureOffsets[rule] = _captures;
			_factories[rule] = factories;

			foreach (var node in NodeWalk.Descendants(graph.Bodies[rule]))
			{
				_owners[node] = rule;

				if (node is Node.Capture)
				{
					var slot = _captures + layout.SlotOf(node);

					_captureSlots[node] = slot;

					if (node is not Node.Capture(_, Node.Lookahead) &&
						(node is not Node.Capture(_, Node.Call(var called, _)) ||
						graph.Results[called].Count == 0 && !graph.Types.ContainsKey(called)))
						_textCaptures.Add(slot);
				}
				else if (node is Node.Construct)
					_constructs[node] = IndexOf(factories, node);
			}

			_captures += layout.Slots.Count;

			if (CSharpEmitter.RecoveryIn(graph, results, rule) is { } recoveryFound)
			{
				var (repetition, recovery, recoverySlot) = recoveryFound;
				var plan = new RecoveryPlan(
					rule, recovery, recoverySlot < 0 ? -1 : _captureOffsets[rule] + recoverySlot,
					_recoveryPlans.Count, CSharpEmitter.MethodOf(rule) + "_Recover",
					recoverySlot < 0 ? null : layout.Slots[recoverySlot].Rule);

				_recoveries[repetition] = plan;
				_recoveryPlans.Add(plan);
			}
		}

		for (var i = 0; i < graph.Rules.Count; i++)
		{
			var rule = graph.Rules[i];

			_ruleIds[rule] = i;
			_entries[rule] = Reserve(out _);
		}

		_plan    = ExecutionPlan.Of(graph);

		CollectValueTypes();

		// What follows each rule, before any of them is compiled. A body is compiled once and
		// called from everywhere, so what it is told has to be the union over its callers —
		// and that is only known once every one of them has been looked at.
		_follow = FollowSets.Of(graph);

		foreach (var rule in graph.Rules)
		{
			var body = Compile(graph.Bodies[rule], Return, Follows(rule));
			var entry = _states[_entries[rule] - First];

			entry.Line($"Trace(\"enter {Escape(rule.Name)}\", {_entries[rule]}, p, entries.Count);");
			entry.Line($"goto {Label(body)};");
		}
	}

	static bool HasTypedGuards(RecognitionGraph graph)
	{
		foreach (var rule in graph.Rules)
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

	public IReadOnlyList<string> Extra => _extra;

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
		foreach (var rule in _graph.Rules)
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

		_wholeEntries[root] = _graph.Trivia.ContainsKey(root)
			? Compile(BodyOf(root, whole: true), Return, FirstSets.First.All)
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
	public bool CanLower(RuleSymbol rule, bool whole) =>
		Silent(BodyOf(rule, whole), whole ? FirstSets.First.End : FirstSets.First.All);

	public int Register(Node node)
	{
		var state = Compile(node, Return, FirstSets.First.All);

		_roots.Add(state);

		return state;
	}

	/// <summary>The states something outside the table jumps to.</summary>
	readonly HashSet<int> _roots = [];

	IReadOnlyDictionary<RuleSymbol, FirstSets.First> _follow =
		new Dictionary<RuleSymbol, FirstSets.First>();

	FirstSets.First Follows(RuleSymbol rule) =>
		_follow.TryGetValue(rule, out var after) ? after : FirstSets.First.All;

	public static string RenderProbe(string name, string engine, int entry, bool powers)
	{
		var file = new Writer(0);

		using (file.Block(
			$"static int {name}(global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure)"))
		{
			file.Line("object? ignored;");
			file.Line(
				$"return {engine}(text, pos, {entry}, -1{(powers ? ", 0" : "")}, " +
				"false, false, ref failure, out ignored);");
		}

		return file.ToString();
	}

	public static string RenderSyncProbe(string name, string engine, int entry, bool powers)
	{
		var file = new Writer(0);

		using (file.Block(
			$"static int {name}(global::System.ReadOnlySpan<char> text, int pos)"))
		{
			file.Line($"var failure = new {CSharpEmitter.FailureType}();");
			file.Line("object? ignored;");
			file.Line(
				$"return {engine}(text, pos, {entry}, -1{(powers ? ", 0" : "")}, " +
				"false, false, ref failure, out ignored);");
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
			$"ref {CSharpEmitter.FailureType} failure{output})"))
		{
			file.Line("object? recognized;");
			file.Line(
				$"var end = {engine}(text, pos, {entry}, {ValueRule(root)}{enginePower}, " +
				$"{(whole ? "true" : "false")}, true, ref failure, out recognized);");

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

		foreach (var rule in _graph.Rules)
			hasValues |= ValueRule(rule) >= 0;

		if (hasValues && Caches)
			EnsureMaterializer();

		using (file.Block(
			$"static int {name}(global::System.ReadOnlySpan<char> text, int pos, int state, " +
			$"int rootRule{strength}, bool whole, bool materialize, ref {CSharpEmitter.FailureType} failure, " +
			"out object? recognized)"))
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

				if (_usesChar)
					file.Line("var c       = '\\0';");
				file.Line("string[]? expected = null;");

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

				for (var i = 0; i < _captures; i++)
					if (_textCaptures.Contains(i))
						file.Line($"var capture{i} = 0;");

				file.Line();
				file.Line(
					$"entries.Add(new ParserEntry(ParserEntry.Call, {Accept}, pos, -1, -1, -1, -1, " +
					"0, rootRule));");
				file.Line("call = 0;");
				file.Line("goto Dispatch;");

				// The hottest block there is: every return from a rule and every resumption
				// after a failure comes through it. Written here, before the states, rather
				// than after all of them — a jump to the far end of a method this size is a
				// jump out of whatever the processor had ready.
				file.Line("Dispatch:");

				using (file.Block("switch (state)"))
				{
					file.Line($"case {Return}: goto Return;");
					file.Line($"case {Accept}: goto Accept;");
					file.Line($"case {Fail}:   expected = null; goto Fail;");

					// Only where the label is one that was written. A state nothing reaches
					// cannot be resumed at either, so the case for it would name a label that
					// is not there.
					for (var i = 0; i < _states.Count; i++)
						if (Written(Resolved(i + First)))
							file.Line($"case {i + First}: goto {Label(Resolved(i + First))};");

					file.Line("default: expected = null; goto Fail;");
				}

				RenderStates(file);

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
				file.Line("Trace(\"return\", state, p, entries.Count);");
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
							using (file.Block("if (rootRule >= 0)"))
							{
								if (Caches)
								{
									file.Line("var values = parser.Materialization(entries.Count);");
									DeclareTables(file);
									file.Line("var built  = parser.Materialized();");
									file.Line("if (!built[0]) values[0] = parser;");
									file.Line("Materialize_DotGram(text, parser, entries);");
									RootValue(file);
								}
								else
								{
									file.Line();
									Materialize(file, cached: false);
									RootValue(file);
								}
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
				file.Line("else if (lookahead < 0 && p == failure.Position && expected is not null)");
				file.Then(
					"(failure.ExpectedMore ??= new global::System.Collections.Generic.List<string[]>())" +
					".Add(expected);");
				if (_recoveries.Count > 0)
				{
					file.Line("if (lookahead < 0 && p > reach)");
					file.Then("reach = p;");
				}
				file.Line("Trace(\"fail\", state, p, entries.Count);");
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
						file.Line("Trace(\"resume\", state, p, entries.Count);");
						file.Line("goto Dispatch;");
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
							file.Line("Trace(\"shorten run\", state, p, entries.Count);");
							file.Line("goto Dispatch;");
						}

						file.Line();
					}

					if (_captures > 0 || _constructs.Count > 0 || _recoveries.Count > 0 || _usesDead)
					{
						var ignored =
							"entry.Kind == ParserEntry.Capture || entry.Kind == ParserEntry.Construct || " +
							"entry.Kind == ParserEntry.RuleCapture";

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
									"Trace(\"capture negative lookahead\", entry.RuleIndex, p, entries.Count);");
							}
							file.Line("Trace(\"negative lookahead succeeds\", state, p, entries.Count);");
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
	void RenderStates(Writer file)
	{
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
	int Compile(Node node, int next, FirstSets.First following)
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

	int CompileUnguarded(Node node, int next, FirstSets.First following)
	{
		switch (node)
		{
			case Node.Empty:
				return next;

			case Node.Literal(var value) { IgnoreCase: var ignoreCase }:
			{
				var state     = Reserve(out var writer);
				var arrayName = DeclareExpected([node.ToString()]);

				if (_starves)
				{
					writer.Line($"if (p + {value.Length} > text.Length)");
					using (writer.Block(""))
					{
						writer.Line("failure.Starved = true;");
						EmitTerminalFailure(writer, _fail, arrayName);
					}
				}
				else
				{
					writer.Line($"if (p + {value.Length} > text.Length)");
					using (writer.Block(""))
						EmitTerminalFailure(writer, _fail, arrayName);
				}

				for (var i = 0; i < value.Length; i++)
				{
					// ToUpperInvariant on an uncased character (a digit, punctuation) returns
					// it unchanged, so one comparison shape covers cased and uncased
					// characters alike — no per-character branching needed.
					var test = ignoreCase
						? $"global::System.Char.ToUpperInvariant(text[p + {i}]) != " +
						  $"{CSharpEmitter.Char(char.ToUpperInvariant(value[i]))}"
						: $"text[p + {i}] != {CSharpEmitter.Char(value[i])}";

					writer.Line($"if ({test})");
					using (writer.Block(""))
					{
						// The position at a terminal failure names where the character that
						// did not fit actually is, not where the whole literal started.
						if (i > 0)
							writer.Line($"p += {i};");

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

				if (_starves)
				{
					writer.Line("if (p >= text.Length)");
					using (writer.Block(""))
					{
						writer.Line("failure.Starved = true;");
						EmitTerminalFailure(writer, _fail, arrayName);
					}
				}
				else
				{
					writer.Line("if (p >= text.Length)");
					using (writer.Block(""))
						EmitTerminalFailure(writer, _fail, arrayName);
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
					after  = Precedes(nodes[i], after);
				}

				return target;
			}

			case Node.Choice(var alternatives):
			{
				if (Predictive(alternatives) is { } predicted)
					return CompilePredictedChoice(alternatives, predicted, next, following);

				var last   = alternatives.Count - 1;
				var run    = LiteralRun(alternatives, last, following);
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
					if (LiteralRun(alternatives, i, following) is var here and > 0)
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

						using (writer.Block("if (p < text.Length)"))
						{
							writer.Line("c = text[p];");

							if (mine is { } begins)
								writer.Line($"if (!({RangesTest(begins.Ranges)})) goto {Label(target)};");

							if (rest is { } after)
								writer.Line($"if (!({RangesTest(after.Ranges)})) goto {Label(first)};");
						}
					}

					writer.Line(
						$"entries.Add(new ParserEntry(ParserEntry.Choice, {target}, p, call, atomic, " +
						"repeat, lookahead, 0));");
					writer.Line($"Trace(\"push choice\", {target}, p, entries.Count);");
					writer.Line($"goto {Label(first)};");

					target = state;
					rest   = mine is null || rest is null ? null : mine.Or(rest);
				}

				return target;
			}

			case Node.Capture(_, var body):
			{
				var slot = _captureSlots[node];

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
					atClose.Line($"Trace(\"rule capture\", {slot}, p, entries.Count);");
					atClose.Line($"goto {Label(next)};");

					return state;
				}

				writer.Line($"capture{slot} = p;");
				writer.Line($"goto {Label(inner)};");

				atClose.Line(
					$"entries.Add(new ParserEntry(ParserEntry.Capture, {slot}, capture{slot}, " +
					"call, atomic, repeat, lookahead, p));");
				atClose.Line($"Trace(\"capture\", {slot}, p, entries.Count);");
				atClose.Line($"goto {Label(next)};");

				return state;
			}

			case Node.Construct(var body, _):
			{
				var factory = _constructs[node];
				var close   = Reserve(out var atClose);
				var inner   = Compile(body, close, following);
				var state   = Reserve(out var writer);

				writer.Line($"goto {Label(inner)};");
				atClose.Line(
					$"entries.Add(new ParserEntry(ParserEntry.Construct, {factory}, p, " +
					"call, atomic, repeat, lookahead, 0));");
				atClose.Line($"Trace(\"construct\", {factory}, p, entries.Count);");
				atClose.Line($"goto {Label(next)};");

				return state;
			}

			case Node.Call(var rule, _):
			{
				if (CanInline(rule))
					return Compile(_graph.Bodies[rule], next, following);

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
				writer.Line($"Trace(\"call {Escape(rule.Name)}\", {_entries[rule]}, p, entries.Count);");
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
				var method = "Recognize_DotGram_Guard" + _guards++;
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
					writer.Line("if (guardNeedsMaterialization) Materialize_DotGram(text, parser, entries);");

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

			case Node.Atomic(var body):
			{
				var commit = Reserve(out var atCommit);
				var inner  = Compile(body, commit, following);
				var state  = Reserve(out var writer);

				writer.Line("var atomicIndex = entries.Count;");
				writer.Line("entries.Add(new ParserEntry(ParserEntry.Atomic, 0, p, call, atomic, repeat, lookahead, 0));");
				writer.Line("atomic = atomicIndex;");
				writer.Line($"Trace(\"enter atomic\", {inner}, p, entries.Count);");
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
				atCommit.Line($"Trace(\"commit\", {next}, p, entries.Count);");
				atCommit.Line($"goto {Label(next)};");

				return state;
			}

			case Node.Repeat repeat:
			{
				if (_recoveries.TryGetValue(node, out var recovery))
					return CompileRecoveringRepeat(repeat, recovery, next, following);

				if (SilentRepeat(repeat, following))
					return CompileSilentRepeat(repeat, next, following);

				return RunTest(repeat.Body) is { } runTest
					? CompileRun(repeat, runTest, next, following)
					: CompileRepeat(repeat, next, following);
			}

			case Node.Lookahead(var isPositive, var body):
			{
				var success = Reserve(out var atSuccess);
				var inner   = Compile(body, success, FirstSets.First.All);
				var state   = Reserve(out var writer);

				writer.Line("var lookaheadIndex = entries.Count;");
				writer.Line(
					$"entries.Add(new ParserEntry(ParserEntry.Lookahead, {next}, p, call, atomic, " +
					$"repeat, lookahead, {(isPositive ? 1 : 0)}));");
				writer.Line("lookahead = lookaheadIndex;");
				writer.Line($"Trace(\"enter {(isPositive ? "positive" : "negative")} lookahead\", {inner}, p, entries.Count);");
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
				atSuccess.Line($"Trace(\"lookahead body matched\", {next}, p, entries.Count);");
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
		var inner   = Compile(seen, success, FirstSets.First.All);
		var state   = Reserve(out var writer);

		writer.Line("var lookaheadIndex = entries.Count;");
		writer.Line(
			$"entries.Add(new ParserEntry(ParserEntry.Lookahead, {next}, p, call, atomic, " +
			"repeat, lookahead, 1));");
		writer.Line("lookahead = lookaheadIndex;");
		writer.Line($"Trace(\"enter captured positive lookahead\", {inner}, p, entries.Count);");
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
		atSuccess.Line($"Trace(\"capture lookahead\", {slot}, seenTo, entries.Count);");
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
		var inner   = Compile(rejected, matched, FirstSets.First.All);
		var state   = Reserve(out var writer);

		writer.Line("var lookaheadIndex = entries.Count;");
		writer.Line(
			$"entries.Add(new ParserEntry(ParserEntry.Lookahead, {next}, p, call, atomic, " +
			$"repeat, lookahead, 0, {slot}));");
		writer.Line("lookahead = lookaheadIndex;");
		writer.Line($"Trace(\"enter captured negative lookahead\", {inner}, p, entries.Count);");
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

		if (shared.Length > 0)
		{
			if (_starves)
			{
				writer.Line($"if (p + {shared.Length} > text.Length)");
				using (writer.Block(""))
				{
					writer.Line("failure.Starved = true;");
					EmitTerminalFailure(writer, fail, arrayName);
				}
			}
			else
			{
				writer.Line($"if (p + {shared.Length} > text.Length)");
				using (writer.Block(""))
					EmitTerminalFailure(writer, fail, arrayName);
			}

			for (var i = 0; i < shared.Length; i++)
			{
				writer.Line($"if (text[p + {i}] != {CSharpEmitter.Char(shared[i])})");
				using (writer.Block(""))
				{
					// Same sharpening as Node.Literal's own per-character loop: name the
					// character that did not fit, not where the shared prefix started.
					if (i > 0)
						writer.Line($"p += {i};");

					EmitTerminalFailure(writer, fail, arrayName);
				}
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
			var tests = new List<string>();

			if (text.Length > shared.Length)
				tests.Add($"p + {text.Length} <= text.Length");

			for (var i = shared.Length; i < text.Length; i++)
				tests.Add($"text[p + {i}] == {CSharpEmitter.Char(text[i])}");

			settled = tests.Count == 0;

			using (writer.Block(settled ? "" : $"if ({string.Join(" && ", tests)})"))
			{
				if (text.Length > 0)
					writer.Line($"p += {text.Length};");

				writer.Line($"goto {Label(next)};");
			}

			if (settled)
				break;
		}

		// Every failure site in this run — the shared-prefix guards above and this
		// catch-all — covers the same, full set of `texts`: nothing here narrows a
		// subset the way a real trie would. One known gap accepted for now: where a
		// prefix conflict (`"p" | "q" | "pr"`) splits one grammar-level choice into
		// several entry-less `CompileLiterals` runs chained by `fail`, a later run's own
		// `expected` can overwrite an earlier one's before either reaches the real
		// `Fail:` — under-reporting, never mis-attributing or over-reporting. Left as a
		// documented first-cut gap rather than solved, per docs/implementation.md's own
		// "the corpus grows by one rule" policy.
		if (!settled)
			EmitTerminalFailure(writer, fail, arrayName);

		return state;
	}

	/// <summary>
	/// A choice one character decides: read it, jump to the alternative it belongs to.
	/// </summary>
	int CompilePredictedChoice(
		IReadOnlyList<Node> alternatives, string[] tests, int next, FirstSets.First following)
	{
		var targets = new int[alternatives.Count];

		for (var i = 0; i < alternatives.Count; i++)
			targets[i] = Compile(alternatives[i], next, following);

		var state = Reserve(out var writer);

		_usesChar = true;

		// Predicted by disjoint first sets (Predictive), which already proved every
		// alternative's first set is known and finite — none is Anything, Nothing or
		// nullable, or Predictive would have refused to predict at all — so the union of
		// their ranges is exactly what this position accepts, on either failure path
		// below.
		var arrayName = DeclareExpected([PredictedDisplay(alternatives)]);

		if (_starves)
		{
			writer.Line("if (p >= text.Length)");
			using (writer.Block(""))
			{
				writer.Line("failure.Starved = true;");
				EmitTerminalFailure(writer, _fail, arrayName);
			}
		}
		else
		{
			writer.Line("if (p >= text.Length)");
			using (writer.Block(""))
				EmitTerminalFailure(writer, _fail, arrayName);
		}

		writer.Line("c = text[p];");

		for (var i = 0; i < targets.Length; i++)
			writer.Line($"if ({tests[i]}) goto {Label(targets[i])};");

		EmitTerminalFailure(writer, _fail, arrayName);

		return state;
	}

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
	int CompileRun(Node.Repeat repeatNode, string test, int next, FirstSets.First following)
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
				writer.Line("if (p >= text.Length)");
				using (writer.Block(""))
				{
					writer.Line("failure.Starved = true;");
					writer.Line("break;");
				}
			}
			else
				writer.Line("if (p >= text.Length) break;");

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

		writer.Line($"Trace(\"run\", {next}, p, entries.Count);");
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
	int GiveBack(int next, int depth, out string start)
	{
		start = "turn" + depth;

		var state = Reserve(out var writer);

		writer.Line($"p = {start};");
		writer.Line($"goto {Label(next)};");

		_turns.Add((depth, state));

		return state;
	}

	int CompileSilentRepeat(Node.Repeat repeatNode, int next, FirstSets.First following)
	{
		var (body, min, max) = repeatNode;
		var inside = FirstSets.Of(body, _graph).Or(following);
		var target = next;

		// One local per depth rather than per repetition. Two of them are live at once only
		// where one of these is written inside another, and that nests two or three deep in
		// the grammars there are — where the count of them was sixteen, all live at once in a
		// method that already keeps the position, the frame, the arena indexes and the
		// character in locals. Registers run out long before sixteen, and what pays for that
		// is every line of the method, not the loops.
		var mine = _depth++;

		if (max is null)
		{
			var loop  = Reserve(out var atLoop);
			var saved = _fail;

			// Round again, or out — and out is through the door that puts the position back.
			_fail = GiveBack(next, mine, out var start);

			var inner = Compile(body, loop, inside);

			_fail = saved;

			atLoop.Line($"{start} = p;");
			atLoop.Line($"goto {Label(inner)};");

			target = loop;
		}
		else
			for (var turn = min; turn < max; turn++)
			{
				var saved = _fail;
				var after = target;

				_fail  = GiveBack(after, mine, out var start);
				target = Compile(body, after, inside);
				_fail  = saved;

				var began = Reserve(out var atBegan);

				atBegan.Line($"{start} = p;");
				atBegan.Line($"goto {Label(target)};");

				target = began;
			}

		for (var turn = 0; turn < min; turn++)
			target = Compile(body, target, inside);

		_depth = mine;

		return target;
	}

	int CompileRepeat(Node.Repeat repeatNode, int next, FirstSets.First following)
	{
		var (body, min, max) = repeatNode;

		if (max == 0)
			return next;

		var exit  = Reserve(out var atExit);
		var loop  = Reserve(out var atLoop);
		var after = Reserve(out var atAfter);
		var entry = Reserve(out var atEntry);
		var inner = Compile(body, after, FirstSets.Of(body, _graph).Or(following));

		atEntry.Line("var repeatIndex = entries.Count;");
		atEntry.Line("entries.Add(new ParserEntry(ParserEntry.Repeat, 0, p, call, atomic, repeat, lookahead, 0));");
		atEntry.Line("repeat = repeatIndex;");
		atEntry.Line($"Trace(\"enter repeat\", {loop}, p, entries.Count);");
		atEntry.Line($"goto {Label(loop)};");

		if (min > 0 || max is not null)
		{
			atLoop.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
			atLoop.Line("var repeating = entries[repeat];");
			atLoop.Line("global::System.Diagnostics.Debug.Assert(repeating.Kind == ParserEntry.Repeat);");
		}

		if (max is { } limit)
			atLoop.Line($"if (repeating.Value >= {limit}) goto {Label(exit)};");

		if (min == 0)
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
		// on every iteration.
		if (min > 0 || max is not null)
		{
			atAfter.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
			atAfter.Line("var repeated = entries[repeat];");
			atAfter.Line(
				"entries[repeat] = new ParserEntry(ParserEntry.Repeat, 0, repeated.Position, " +
				"repeated.CallIndex, repeated.AtomicIndex, repeated.RepeatIndex, " +
				"repeated.LookaheadIndex, repeated.Value + 1);");
		}

		atAfter.Line($"goto {Label(loop)};");

		LeaveRepeat(atExit, next);

		return entry;
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
		writer.Line($"Trace(\"leave repeat\", {next}, p, entries.Count);");
		writer.Line($"goto {Label(next)};");
	}

	int ValueRule(RuleSymbol rule) =>
		_graph.Results[rule].Count > 0 || _graph.Types.ContainsKey(rule) ? _ruleIds[rule] : -1;

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
		var name  = "Recognize_DotGram_Expected" + _expectedCount++;
		var items = string.Join(", ", display.Select(d => $"\"{EscapeExpected(d)}\""));

		_extra.Add($"static readonly string[] {name} = {{ {items} }};");

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
	static void EmitTerminalFailure(Writer writer, int fail, string arrayName)
	{
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
