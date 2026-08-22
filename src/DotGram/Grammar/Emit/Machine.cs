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

		_plan = ExecutionPlan.Of(graph);

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
	public bool CachesGuardValues => _guardValues;

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

		_wholeEntries[root] = _graph.Trivia.TryGetValue(root, out var trivia)
			? Compile(new Node.Sequence([trivia, _graph.Bodies[root], trivia]), Return, FirstSets.First.All)
			: _entries[root];

		_roots.Add(_wholeEntries[root]);
	}

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

		if (hasValues && _guardValues)
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
					file.Line($"case {Fail}:   goto Fail;");

					// Only where the label is one that was written. A state nothing reaches
					// cannot be resumed at either, so the case for it would name a label that
					// is not there.
					for (var i = 0; i < _states.Count; i++)
						if (Written(Resolved(i + First)))
							file.Line($"case {i + First}: goto {Label(Resolved(i + First))};");

					file.Line("default: goto Fail;");
				}

				for (var written = 0; written < _order.Count; written++)
				{
					var i    = _order[written];
					var body = _bodies[i];

					// Chained: what this state ends by jumping to is the state written next,
					// so the jump is the line after it either way.
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
				if (_guardValues)
				{
					using (file.Block("else if (entries.Count == call + 1)"))
					{
						file.Line("parser.Truncate(call);");
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
				file.Line("if (whole && p != text.Length) goto Fail;");

				if (hasValues || _recoveryPlans.Count > 0)
				{
					using (file.Block("if (materialize)"))
					{
						if (hasValues)
						{
							using (file.Block("if (rootRule >= 0)"))
							{
								if (_guardValues)
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
				file.Then("failure.Position = p;");
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
					if (_guardValues)
						file.Line("parser.Truncate(last);");
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

			writer.Line($"if ({level} < power) goto Fail;");
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

			case Node.Literal(var value):
			{
				var state = Reserve(out var writer);

				if (_starves)
				{
					writer.Line($"if (p + {value.Length} > text.Length)");
					using (writer.Block(""))
					{
						writer.Line("failure.Starved = true;");
						writer.Line($"goto {Label(_fail)};");
					}
				}
				else
					writer.Line($"if (p + {value.Length} > text.Length) goto {Label(_fail)};");

				for (var i = 0; i < value.Length; i++)
					writer.Line(
						$"if (text[p + {i}] != {CSharpEmitter.Char(value[i])}) goto {Label(_fail)};");

				writer.Line($"p += {value.Length};");
				writer.Line($"goto {Label(next)};");

				return state;
			}

			case Node.Element element:
			{
				var state = Reserve(out var writer);
				var test  = CSharpEmitter.Test(element);

				if (test == "false")
				{
					writer.Line($"goto {Label(_fail)};");

					return state;
				}

				if (_starves)
				{
					writer.Line("if (p >= text.Length)");
					using (writer.Block(""))
					{
						writer.Line("failure.Starved = true;");
						writer.Line($"goto {Label(_fail)};");
					}
				}
				else
					writer.Line($"if (p >= text.Length) goto {Label(_fail)};");

				if (test != "true")
				{
					_usesChar = true;
					writer.Line("c = text[p];");
					writer.Line($"if (!({test})) goto {Label(_fail)};");
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
				var run    = LiteralRun(alternatives, last);
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
					if (LiteralRun(alternatives, i) is var here and > 0)
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

			case Node.External(var method):
			{
				var state = Reserve(out var writer);

				writer.Line($"if (!{method}(text, ref p)) goto Fail;");
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

				writer.Line($"if (!{method}({string.Join(", ", arguments)})) goto Fail;");
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
					if (_guardValues)
						atCommit.Line("parser.Truncate(atomic);");

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
				if (_guardValues)
					atSuccess.Line("parser.Truncate(lookahead);");
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
		if (_guardValues)
			atSuccess.Line("parser.Truncate(lookahead);");
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
		if (_guardValues)
			atMatched.Line("parser.Truncate(lookahead);");
		atMatched.Line("entries.RemoveRange(lookahead, entries.Count - lookahead);");
		atMatched.Line("p         = looked.Position;");
		atMatched.Line("call      = looked.CallIndex;");
		atMatched.Line("atomic    = looked.AtomicIndex;");
		atMatched.Line("repeat    = looked.RepeatIndex;");
		atMatched.Line("lookahead = looked.LookaheadIndex;");
		atMatched.Line("goto Fail;");

		return state;
	}

	/// <summary>
	/// What must begin the input where <paramref name="node"/> begins, given what must
	/// begin it where the node ends.
	/// </summary>
	/// <remarks>
	/// A node that must consume something answers for itself. One that may match nothing
	/// leaves the question to what comes after it as well as to itself, so the two are taken
	/// together — the direction that admits too much, and so proves too little, rather than
	/// the one that proves something false.
	/// </remarks>
	FirstSets.First Precedes(Node node, FirstSets.First after)
	{
		var first = FirstSets.Of(node, _graph);

		return first.Nothing                      ? after :
			FirstSets.Nullable(node, _graph) ? first.Or(after) :
			first;
	}

	int WeightOfAll(IReadOnlyList<Node> nodes, int budget)
	{
		var total = 0;

		foreach (var node in nodes)
		{
			total += Weight(node, budget - total);

			if (total > budget)
				break;
		}

		return total;
	}

	/// <summary>
	/// Whether a node writes nothing into the arena, so that its failure is nobody's business
	/// but its own.
	/// </summary>
	/// <remarks>
	/// The arena is what a failure is unwound through: an entry written on the way in is
	/// taken back on the way out, and jumping past the dispatcher would leave it there. A
	/// node that writes none — text matched against the input, alternatives one character
	/// tells apart, rules small enough to be compiled in place — has nothing to take back,
	/// and its failure can go straight wherever the caller wants it.
	/// </remarks>
	bool Silent(Node node, FirstSets.First following) =>
		(_graph.Climbing.Count == 0 || !_owners.ContainsKey(node)) &&
		node switch
		{
			Node.Empty or Node.Literal or Node.Element => true,
			Node.Sequence(var parts)                   => AllSilent(parts, following),
			Node.Choice(var alternatives)              => Predictive(alternatives) is not null &&
			                                              AllSilent(alternatives, following, sequence: false),
			Node.Call(var rule, _)                     => CanInline(rule) &&
			                                              _graph.Bodies.TryGetValue(rule, out var called) &&
			                                              Silent(called, following),

			// A repetition inside another is silent exactly when it is itself the loop and
			// nothing else — which is the same question, asked of it. `Path = ('/' & Segment)*`
			// with `Segment` a repetition of its own is the shape this was refusing, and it is
			// the shape most path-like grammars are written in.
			Node.Repeat repeat                         => SilentRepeat(repeat, following),

			_                                          => false,
		};

	/// <summary>
	/// Whether a repetition is a loop and nothing else — no entry, no count, no way back.
	/// </summary>
	/// <remarks>
	/// Asked in two places and it has to answer the same in both: here, to know whether the
	/// thing around it writes nothing, and at the point of compiling it, to decide what to
	/// write. Different answers would mean jumping past entries that were made after all.
	/// </remarks>
	bool SilentRepeat(Node.Repeat repeat, FirstSets.First following) =>
		(repeat.Max ?? repeat.Min + 1) * Weight(repeat.Body, Unrollable) <= Unrollable &&
		Possessive(repeat.Body, following) &&
		Silent(repeat.Body, FirstSets.Of(repeat.Body, _graph).Or(following));

	/// <summary>
	/// Every one of them, each followed by what follows it.
	/// </summary>
	/// <remarks>
	/// Threaded the way compilation threads it, because it is the same question about the
	/// same nodes: a part of a sequence is followed by the rest of the sequence, and an
	/// alternative of a choice is followed by whatever the choice is.
	/// </remarks>
	bool AllSilent(IReadOnlyList<Node> nodes, FirstSets.First following, bool sequence = true)
	{
		var after = following;

		for (var i = nodes.Count - 1; i >= 0; i--)
		{
			if (!Silent(nodes[i], after))
				return false;

			if (sequence)
				after = Precedes(nodes[i], after);
		}

		return true;
	}

	/// <summary>
	/// How much a repetition may be written out one after another rather than looped, counted
	/// in the states the turns would come to.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Unrolling is what removes the count, and with it the last thing the arena was holding
	/// for a repetition that needs it for nothing else. Generated size is not a cost this
	/// project minimizes, but it is not unbounded either, and it does not add — it multiplies.
	/// <c>(H16 &amp; ':'){6}</c> is six copies of <c>H16</c>, each of which is
	/// <c>Hex{1,4}</c>, and the rule that holds it has nine alternatives; counting turns
	/// alone would call each of those small and arrive at hundreds of copies of one character
	/// test.
	/// </para>
	/// <para>
	/// So the budget is turns times what a turn weighs, and a turn weighs what it will
	/// actually be written as — through the calls that are compiled in place, and through the
	/// repetitions inside it, which multiply in their turn.
	/// </para>
	/// </remarks>
	const int Unrollable = 24;

	/// <summary>
	/// About how many states a node will come to, stopping once that is more than is being
	/// asked about.
	/// </summary>
	int Weight(Node node, int budget)
	{
		if (budget <= 0)
			return 1;

		switch (node)
		{
			case Node.Empty:
				return 0;

			case Node.Sequence(var parts):
				return WeightOfAll(parts, budget);

			case Node.Choice(var alternatives):
				return WeightOfAll(alternatives, budget);

			case Node.Capture(_, var captured):
				return 1 + Weight(captured, budget - 1);

			case Node.Construct(var built, _):
				return 1 + Weight(built, budget - 1);

			case Node.Atomic(var kept):
				return 1 + Weight(kept, budget - 1);

			case Node.Lookahead(_, var seen):
				return 1 + Weight(seen, budget - 1);

			// An unbounded one is written once and gone round, so what it weighs is a turn
			// and the going round; a bounded one is written out as many times as it is
			// allowed to happen.
			case Node.Repeat(var body, _, var max):
				return (max ?? 2) * Weight(body, budget);

			case Node.Call(var rule, _) when CanInline(rule) && _graph.Bodies.TryGetValue(rule, out var called):
				return Weight(called, budget);

			default:
				return 1;
		}
	}

	/// <summary>
	/// Whether a repetition can be run to its end and never asked to give any of it back.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A repetition normally leaves one resume point per turn, because a later failure may
	/// mean it went one turn too far. Two facts together say it never did.
	/// </para>
	/// <para>
	/// The first is that what follows cannot begin with what the body begins with. Every
	/// place the repetition could stop short is a place a turn began, so the character there
	/// is one the body starts with; the continuation would have to start with that same
	/// character and, by disjointness, cannot. The second is that the body matches in one way
	/// only. Without it the first is not enough: a body that can match two lengths can end
	/// the repetition somewhere no turn ever began, and nothing has been said about the
	/// character there. <c>("ab" | "a")*</c> against <c>aab</c> is that case, and it is why
	/// the length has to be settled before the first sets are allowed to decide anything.
	/// </para>
	/// <para>
	/// Both are asked of what is known here. An unknown first set is "anything", which
	/// overlaps; an unknown continuation is nothing, which proves nothing; either answers no,
	/// and the general machinery stays.
	/// </para>
	/// </remarks>
	bool Possessive(Node body, FirstSets.First following, HashSet<RuleSymbol>? seen = null) =>
		following.IsKnown &&
		!FirstSets.Nullable(body, _graph) &&
		!FirstSets.Of(body, _graph).Overlaps(following) &&
		Deterministic(body, seen ?? [], FirstSets.Of(body, _graph).Or(following));

	/// <summary>
	/// Whether a node has at most one match at any position — one length, not a choice of
	/// them.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Alternatives settle it when one character tells them apart, which is what
	/// <see cref="Predictive"/> already decides.
	/// </para>
	/// <para>
	/// A repetition settles it exactly when it is possessive. Where a repetition stops is
	/// otherwise a choice — the very choice this is asking about — but a possessive one
	/// cannot stop anywhere but where the input stopped it, so it has the one length. That
	/// is why this needs to know what follows: possessiveness is a fact about a repetition
	/// in a place, not about a repetition.
	/// </para>
	/// </remarks>
	bool Deterministic(Node node, HashSet<RuleSymbol> seen, FirstSets.First following) =>
		node switch
		{
			Node.Empty or Node.Guard or Node.Lookahead => true,
			Node.Literal or Node.Element               => true,
			Node.Capture(_, var body)                  => Deterministic(body, seen, following),
			Node.Construct(var body, _)                => Deterministic(body, seen, following),
			Node.Atomic(var body)                      => Deterministic(body, seen, following),
			Node.Sequence(var parts)                   => AllDeterministic(parts, seen, following),
			Node.Choice(var alternatives)              => Predictive(alternatives) is not null &&
			                                              AllDeterministic(
			                                                  alternatives, seen, following, sequence: false),
			Node.Repeat(var body, _, _)                => Possessive(body, following, seen),
			Node.Call(var rule, _)                     => Deterministic(rule, seen, following),
			_                                          => false,
		};

	/// <summary>
	/// Whether a call is to something determinate, guarded against calling round in a ring.
	/// </summary>
	/// <remarks>
	/// The guard is the path down, not everything met on the way: a rule taken off again on
	/// the way out, so that meeting it twice in two places is not mistaken for recursion.
	/// <c>Pair = Atom &amp; Atom</c> is determinate and was being called anything but, which
	/// cost the repetitions around it their proof. The same mistake was made and corrected in
	/// <see cref="FirstSets"/>, and it was not corrected here.
	/// </remarks>
	bool Deterministic(RuleSymbol rule, HashSet<RuleSymbol> seen, FirstSets.First following)
	{
		if (!seen.Add(rule))
			return false;

		var settled = _graph.Bodies.TryGetValue(rule, out var called) &&
			Deterministic(called, seen, following);

		seen.Remove(rule);

		return settled;
	}

	bool AllDeterministic(
		IReadOnlyList<Node> nodes, HashSet<RuleSymbol> seen, FirstSets.First following,
		bool sequence = true)
	{
		var after = following;

		for (var i = nodes.Count - 1; i >= 0; i--)
		{
			if (!Deterministic(nodes[i], seen, after))
				return false;

			if (sequence)
				after = Precedes(nodes[i], after);
		}

		return true;
	}

	/// <summary>
	/// The character tests that decide a choice outright, or null where the input does not.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A choice normally has to be able to come back: it takes the first alternative that
	/// starts and, if that one fails later, tries the next. The entry it leaves behind is
	/// what makes coming back possible, and it is written on every visit whether or not
	/// anything ever comes back for it.
	/// </para>
	/// <para>
	/// Nothing ever does when the alternatives cannot begin with the same character. Suppose
	/// the character at hand belongs to the first set of one alternative and that alternative
	/// then fails. Any other alternative that could match here would have to begin with that
	/// same character, and by disjointness none does — so the choice fails with it, and the
	/// entry that would have been popped to discover this is pure cost. One character decides
	/// which alternative it is, and having decided, there is no second reading to keep.
	/// </para>
	/// <para>
	/// Every alternative must also consume something. An alternative that can match nothing
	/// matches everywhere, so it stays reachable after another has failed, and that is
	/// exactly the alternative an entry is needed for. First sets are approximate in the
	/// direction that says "anything" when unsure, and two of those overlap, so an
	/// alternative this cannot read gives up the optimization rather than mis-taking it.
	/// </para>
	/// </remarks>
	string[]? Predictive(IReadOnlyList<Node> alternatives)
	{
		if (alternatives.Count < 2)
			return null;

		var firsts = new FirstSets.First[alternatives.Count];

		for (var i = 0; i < alternatives.Count; i++)
		{
			var first = FirstSets.Of(alternatives[i], _graph);

			if (first.Anything || first.Nothing || FirstSets.Nullable(alternatives[i], _graph))
				return null;

			firsts[i] = first;
		}

		for (var i = 0; i < firsts.Length; i++)
			for (var j = i + 1; j < firsts.Length; j++)
				if (firsts[i].Overlaps(firsts[j]))
					return null;

		var tests = new string[firsts.Length];

		for (var i = 0; i < firsts.Length; i++)
			tests[i] = RangesTest(firsts[i].Ranges);

		return tests;
	}

	/// <summary>
	/// How many alternatives ending at <paramref name="at"/> are plain text, up to two or
	/// more.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Two is where it starts paying: one literal alternative is compiled as it always was,
	/// and the saving is in not writing down where to come back to between one literal and
	/// the next.
	/// </para>
	/// <para>
	/// And only where none of them is the beginning of another. Where one is, both match at
	/// the same place — the shorter is taken, being first or not — and if what follows the
	/// choice then fails, the longer has to be tried. That is a way back, and a way back is
	/// what the entry is. <c>"ab" | "abc"</c> is such a pair and is compiled as it always
	/// was; <c>"abc_x" | "abc_y"</c> is not, and at most one of them can match anywhere.
	/// </para>
	/// </remarks>
	static int LiteralRun(IReadOnlyList<Node> alternatives, int at)
	{
		var run = 0;

		while (at - run >= 0 && alternatives[at - run] is Node.Literal)
			run++;

		if (run < 2)
			return 0;

		for (var i = at - run + 1; i <= at; i++)
			for (var j = i + 1; j <= at; j++)
				if (alternatives[i] is Node.Literal(var one) &&
					alternatives[j] is Node.Literal(var other) &&
					(one.StartsWith(other, StringComparison.Ordinal) ||
					 other.StartsWith(one, StringComparison.Ordinal)))
				{
					return 0;
				}

		return run;
	}

	/// <summary>What a run of literal alternatives can begin with.</summary>
	static FirstSets.First Begins(IReadOnlyList<Node> alternatives, int from, int to)
	{
		var ranges = new List<CharRange>();

		for (var i = from; i <= to; i++)
			if (alternatives[i] is Node.Literal(var text))
			{
				if (text.Length == 0)
					return FirstSets.First.All;

				ranges.Add(new CharRange(text[0], text[0]));
			}

		return new FirstSets.First(false, false, ranges);
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
		var texts = new List<string>(to - from + 1);

		for (var i = from; i <= to; i++)
			if (alternatives[i] is Node.Literal(var text))
				texts.Add(text);

		var shared = texts[0];

		foreach (var text in texts)
		{
			var common = 0;

			while (common < shared.Length && common < text.Length && shared[common] == text[common])
				common++;

			shared = shared.Substring(0, common);
		}

		var state = Reserve(out var writer);

		if (shared.Length > 0)
		{
			if (_starves)
			{
				writer.Line($"if (p + {shared.Length} > text.Length)");
				using (writer.Block(""))
				{
					writer.Line("failure.Starved = true;");
					writer.Line($"goto {Label(fail)};");
				}
			}
			else
				writer.Line($"if (p + {shared.Length} > text.Length) goto {Label(fail)};");

			for (var i = 0; i < shared.Length; i++)
				writer.Line($"if (text[p + {i}] != {CSharpEmitter.Char(shared[i])}) goto {Label(fail)};");
		}

		foreach (var text in texts)
		{
			var tests = new List<string>();

			if (text.Length > shared.Length)
				tests.Add($"p + {text.Length} <= text.Length");

			for (var i = shared.Length; i < text.Length; i++)
				tests.Add($"text[p + {i}] == {CSharpEmitter.Char(text[i])}");

			using (writer.Block(tests.Count == 0 ? "" : $"if ({string.Join(" && ", tests)})"))
			{
				if (text.Length > 0)
					writer.Line($"p += {text.Length};");

				writer.Line($"goto {Label(next)};");
			}

		}

		writer.Line($"goto {Label(fail)};");

		return state;
	}

	/// <summary>
	/// What an alternative must begin with, where that is known well enough to turn one
	/// away by.
	/// </summary>
	/// <remarks>
	/// Three things make it unusable, and all three are the approximation admitting it does
	/// not know: a first set of "anything" excludes no character, one of "nothing" describes
	/// something that consumes none, and an alternative that can match empty matches
	/// everywhere whatever its first set says. Any of them and the alternative is tried as it
	/// always was.
	/// </remarks>
	FirstSets.First? Decidable(Node alternative)
	{
		var first = FirstSets.Of(alternative, _graph);

		return first.Anything || first.Nothing || FirstSets.Nullable(alternative, _graph) ? null : first;
	}

	/// <summary>A test over <c>c</c> for membership of a set of ranges.</summary>
	static string RangesTest(IReadOnlyList<CharRange> ranges)
	{
		var tests = new string[ranges.Count];

		for (var i = 0; i < ranges.Count; i++)
			tests[i] = ranges[i].IsSingle
				? $"c == {CSharpEmitter.Char(ranges[i].From)}"
				: $"(c >= {CSharpEmitter.Char(ranges[i].From)} && c <= {CSharpEmitter.Char(ranges[i].To)})";

		return string.Join(" || ", tests);
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

		if (_starves)
		{
			writer.Line("if (p >= text.Length)");
			using (writer.Block(""))
			{
				writer.Line("failure.Starved = true;");
				writer.Line($"goto {Label(_fail)};");
			}
		}
		else
			writer.Line($"if (p >= text.Length) goto {Label(_fail)};");

		writer.Line("c = text[p];");

		for (var i = 0; i < targets.Length; i++)
			writer.Line($"if ({tests[i]}) goto {Label(targets[i])};");

		writer.Line($"goto {Label(_fail)};");

		return state;
	}

	/// <summary>
	/// The character test a repetition's body is, or null where the body is anything more.
	/// </summary>
	/// <remarks>
	/// A body that consumes exactly one character and keeps nothing is the case where the
	/// general machinery is pure overhead: it has no choice to resume, no capture to record
	/// and no frame to return to, so every iteration's arena traffic is bookkeeping about
	/// nothing. The test is written against <c>c</c>, like every other element test.
	/// </remarks>
	string? RunTest(Node body)
	{
		switch (body)
		{
			case Node.Element element:
			{
				var test = CSharpEmitter.Test(element);

				return test == "false" ? null : test;
			}

			case Node.Literal(var value) when value.Length == 1:
				return $"c == {CSharpEmitter.Char(value[0])}";

			// A rule that is inlined anyway is its body written somewhere else, and a grammar
			// names its character classes far more often than it spells them out.
			case Node.Call(var rule, _) when CanInline(rule):
				return RunTest(_graph.Bodies[rule]);

			case Node.Sequence(var nodes) when nodes.Count == 1:
				return RunTest(nodes[0]);

			// Alternatives that each consume exactly one character and keep nothing are a
			// disjunction, not a choice: whichever one matched, the position afterwards is the
			// same and so is the continuation, so there is nothing to come back to.
			case Node.Choice(var alternatives):
			{
				var tests = new string[alternatives.Count];

				for (var i = 0; i < alternatives.Count; i++)
					if (RunTest(alternatives[i]) is { } test)
						tests[i] = test == "true" ? "true" : $"({test})";
					else
						return null;

				return string.Join(" || ", tests);
			}

			default:
				return null;
		}
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
			writer.Line($"if (p < {floor}) goto Fail;");

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
		if (_guardValues)
		{
			using (writer.Block("if (entries.Count == repeat + 1)"))
			{
				writer.Line("parser.Truncate(repeat);");
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

	void EnsureMaterializer()
	{
		if (_materializer)
			return;

		_materializer = true;

		var helper = new Writer(0);

		using (helper.Block(
			"static void Materialize_DotGram(global::System.ReadOnlySpan<char> text, Parser parser, " +
			"ParserArena entries)"))
			Materialize(helper, cached: true);

		_extra.Add(helper.ToString());
	}

	void Materialize(Writer file, bool cached)
	{
		file.Line("var values = parser.Materialization(entries.Count);");
		DeclareTables(file);
		if (cached)
			file.Line("var built  = parser.Materialized();");
		file.Line("var links  = parser.MaterializationLinks(entries.Count);");
		file.Line();

		using (file.Block("for (var derivationAt = 0; derivationAt < entries.Count; derivationAt++)"))
		{
			file.Line("var derivation = entries[derivationAt];");

			var linked =
				"derivation.Kind == ParserEntry.Capture || derivation.Kind == ParserEntry.RuleCapture || " +
				"derivation.Kind == ParserEntry.Construct";

			if (_recoveryPlans.Count > 0)
				linked += " || derivation.Kind == ParserEntry.Recovery";

			using (file.Block($"if (derivation.CallIndex >= 0 && ({linked}))"))
			{
				file.Line("links[entries.Count + derivationAt] = links[derivation.CallIndex];");
				file.Line("links[derivation.CallIndex] = derivationAt;");
			}
		}

		// A transparent rule may have completed before a surrounding path backtracked and
		// selected another derivation. Such completed entries can remain useful as history,
		// but only calls reached through RuleCapture from the accepted root may run user
		// construction code. Call entries precede their children, so one forward pass marks
		// the complete accepted value tree without recursion or another typed collection.
		file.Line();
		if (!cached)
			file.Line("values[0] = parser;");
		using (file.Block("for (var ownerAt = 0; ownerAt < entries.Count; ownerAt++)"))
		{
			file.Line("if (!global::System.Object.ReferenceEquals(values[ownerAt], parser)) continue;");

			using (file.Block(
				"for (var capturedAt = links[ownerAt]; capturedAt >= 0; " +
				"capturedAt = links[entries.Count + capturedAt])"))
			{
				file.Line("var candidate = entries[capturedAt];");
				file.Line("if (candidate.Kind == ParserEntry.RuleCapture" +
					(cached ? " && !built[candidate.Position]" : "") + ")");
				file.Then("values[candidate.Position] = parser;");
			}
		}

		if (_recoveryPlans.Count > 0)
		{
			file.Line();

			using (file.Block("for (var recoveryAt = 0; recoveryAt < entries.Count; recoveryAt++)"))
			{
				file.Line("var recovered = entries[recoveryAt];");
				file.Line(
					"if (recovered.Kind != ParserEntry.Recovery || recovered.CallIndex < 0 || " +
					(cached ? "built[recoveryAt] || " : "") +
					"!global::System.Object.ReferenceEquals(values[recoveryAt], parser) && " +
					"!global::System.Object.ReferenceEquals(values[recovered.CallIndex], parser)) continue;");

				using (file.Block("switch (recovered.State)"))
					foreach (var recovery in _recoveryPlans)
						MaterializeRecovery(file, recovery);

				if (cached)
					file.Line("built[recoveryAt] = true;");
			}
		}

		using (file.Block(
			"for (var completedAt = entries.Count - 1; completedAt >= 0; completedAt--)"))
		{
			file.Line("var completed = entries[completedAt];");
			file.Line(
				"if (completed.Kind != ParserEntry.Completed || " +
				(cached ? "built[completedAt] || " : "") +
				"!global::System.Object.ReferenceEquals(values[completedAt], parser)) continue;");

			using (file.Block("switch (completed.RuleIndex)"))
				foreach (var rule in _graph.Rules)
					if (ValueRule(rule) >= 0)
						MaterializeRule(file, rule);

			if (cached)
				file.Line("built[completedAt] = true;");
		}
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
	bool IsExtent(RuleSymbol rule) =>
		_graph.Types.TryGetValue(rule, out var type) && type == "SourceSpan";

	/// <summary>
	/// Reading a rule's value out of the parse — which for an extent means not reading it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The value table is <c>object?</c> because one automaton serves every rule: what
	/// completed at a position could be any of their types, and the same slot doubles as the
	/// mark saying a value is reachable and unbuilt. That is what puts a struct in a box.
	/// </para>
	/// <para>
	/// An extent needs no slot at all. The entry left where the rule completed already holds
	/// the position it began at and the position it reached — the span is those two numbers,
	/// one array along, and copying them into the table would box them to say what is already
	/// said.
	/// </para>
	/// </remarks>
	string ValueFrom(string type, string index) =>
		type == "SourceSpan"
			? $"new SourceSpan(entries[{index}].Position, entries[{index}].Value - entries[{index}].Position)"
			: TableFor(type) is var table && table >= 0
				? $"values{table}[{index}]"
				: $"({type})values[{index}]!";

	/// <summary>Writing a rule's value where whatever reads it will look.</summary>
	/// <remarks>
	/// The object table keeps the mark rather than the value, so it is cleared here: what
	/// stops a value being built twice is that the mark is gone, and the mark is the only
	/// thing that was ever in there.
	/// </remarks>
	/// <remarks>
	/// A value used to take the mark's place by being written over it. It no longer does —
	/// the mark says a value is reachable and unbuilt, and it now outlives the building. What
	/// stops a second build is <c>built</c>, which is what the guard path always used; the
	/// walk that only runs once never needed either.
	/// </remarks>
	string ValueInto(string type, string index) =>
		TableFor(type) is var table && table >= 0 ? $"values{table}[{index}]" : $"values[{index}]";

	/// <summary>
	/// Handing the root's value out, which is the one place a type has to be forgotten.
	/// </summary>
	/// <remarks>
	/// It leaves through <c>out object?</c>, because the recognizer serves every publication
	/// and they do not agree on a type. So a struct is boxed here — once, at the boundary,
	/// where a caller is about to be handed the answer anyway, rather than once per value
	/// built on the way to it. Which table to take it from is the one thing not known until
	/// the call: <c>rootRule</c> says which rule was asked for.
	/// </remarks>
	void RootValue(Writer file)
	{
		using (file.Block("switch (rootRule)"))
		{
			foreach (var rule in _graph.Rules)
			{
				if (ValueRule(rule) < 0)
					continue;

				file.Line($"case {_ruleIds[rule]}:");

				using (file.Indent())
				{
					// An extent was never put anywhere: the wrapper works it out from the
					// position it gave and the one it was told.
					file.Line(IsExtent(rule)
						? "recognized = null;"
						: $"recognized = {ValueFrom(_results.QualifiedOf(rule)!, "0")};");
					file.Line("break;");
				}
			}

			file.Line("default:");

			using (file.Indent())
			{
				file.Line("recognized = values[0];");
				file.Line("break;");
			}
		}
	}

	/// <summary>The tables in view wherever values are read or written.</summary>
	void DeclareTables(Writer writer)
	{
		for (var i = 0; i < _valueTypes.Count; i++)
			writer.Line($"var values{i} = parser.Materialization{i}();");
	}

	void MaterializeRule(Writer file, RuleSymbol rule)
	{
		var offset    = _captureOffsets[rule];
		var members   = _graph.Results[rule];
		var type      = _results.QualifiedOf(rule)!;
		var factories = _factories[rule];

		// Its value is the entry it left, so the walk has nothing to run here. The case is
		// still written: the switch is over every rule that has a value, and this one has.
		if (IsExtent(rule))
		{
			using (file.Block($"case {_ruleIds[rule]}:"))
				file.Line("break;");

			return;
		}

		using (file.Block($"case {_ruleIds[rule]}:"))
		{
			if (_graph.Folds.ContainsKey(rule))
			{
				MaterializeFold(file, rule, type, offset, factories);
				file.Line("break;");

				return;
			}

			for (var memberIndex = 0; memberIndex < members.Count; memberIndex++)
			{
				var member = members[memberIndex];
				var slots  = new List<string>(member.Slots.Count);

				foreach (var slot in member.Slots)
					slots.Add($"candidate.State == {offset + slot}");

				if (member.Rule is not null)
				{
					if (member.IsSequence)
					{
						var element = _results.ValueOf(member.Rule);
						var recovered = new List<string>();

						foreach (var plan in _recoveryPlans)
							if (plan.Rule == rule && plan.Recovery.Factory is not null)
								foreach (var slot in member.Slots)
									if (plan.Slot == offset + slot)
										recovered.Add($"candidate.Kind == ParserEntry.Recovery && candidate.State == {plan.Id}");

						var accepted =
							$"candidate.Kind == ParserEntry.RuleCapture && candidate.CallIndex == completedAt && " +
							$"({string.Join(" || ", slots)})";
						var collected = recovered.Count == 0
							? accepted
							: $"({accepted}) || ({string.Join(" || ", recovered)})";

						file.Line($"var captured{memberIndex}Count = 0;");

						using (file.Block(
							$"for (var capturedAt{memberIndex} = links[completedAt]; capturedAt{memberIndex} >= 0; " +
							$"capturedAt{memberIndex} = links[entries.Count + capturedAt{memberIndex}])"))
						{
							file.Line($"var candidate = entries[capturedAt{memberIndex}];");
							file.Line($"if ({collected}) captured{memberIndex}Count++;");
						}

						file.Line($"var captured{memberIndex} = new {element}[captured{memberIndex}Count];");
						file.Line($"var captured{memberIndex}Item = captured{memberIndex}Count;");

						using (file.Block(
							$"for (var capturedAt{memberIndex} = links[completedAt]; capturedAt{memberIndex} >= 0; " +
							$"capturedAt{memberIndex} = links[entries.Count + capturedAt{memberIndex}])"))
						{
							file.Line($"var candidate = entries[capturedAt{memberIndex}];");

							using (file.Block($"if ({collected})"))
							{
								if (recovered.Count > 0)
								{
									file.Line($"var capturedValueAt = candidate.Kind == ParserEntry.Recovery ? capturedAt{memberIndex} : candidate.Position;");
									file.Line(
										$"captured{memberIndex}[--captured{memberIndex}Item] = " +
										ValueFrom(element, "capturedValueAt") + ";");
								}
								else
									file.Line(
										$"captured{memberIndex}[--captured{memberIndex}Item] = " +
										ValueFrom(element, "candidate.Position") + ";");
							}
						}

						file.Line();

						continue;
					}

					file.Line($"var captured{memberIndex}At = -1;");

					using (file.Block(
						$"for (var capturedAt{memberIndex} = links[completedAt]; capturedAt{memberIndex} >= 0; " +
						$"capturedAt{memberIndex} = links[entries.Count + capturedAt{memberIndex}])"))
					{
						file.Line($"var candidate = entries[capturedAt{memberIndex}];");

						using (file.Block(
							$"if (candidate.Kind == ParserEntry.RuleCapture && candidate.CallIndex == completedAt && " +
							$"({string.Join(" || ", slots)}))"))
						{
							file.Line($"captured{memberIndex}At = candidate.Position;");
							file.Line("break;");
						}
					}

					var capturedType = _results.ValueOf(member.Rule);

					if (!member.IsOptional)
						file.Line($"global::System.Diagnostics.Debug.Assert(captured{memberIndex}At >= 0);");

					file.Line(member.IsOptional
						? $"{capturedType}? captured{memberIndex} = captured{memberIndex}At < 0 ? " +
							$"default({capturedType}?) : " +
							ValueFrom(capturedType, $"captured{memberIndex}At") + ";"
						: $"var captured{memberIndex} = " +
							ValueFrom(capturedType, $"captured{memberIndex}At") + ";");
					file.Line();

					continue;
				}

				file.Line($"var captured{memberIndex}From = -1;");
				file.Line($"var captured{memberIndex}To   = -1;");

				using (file.Block(
					$"for (var capturedAt{memberIndex} = links[completedAt]; capturedAt{memberIndex} >= 0; " +
					$"capturedAt{memberIndex} = links[entries.Count + capturedAt{memberIndex}])"))
				{
					file.Line($"var candidate = entries[capturedAt{memberIndex}];");

					using (file.Block(
						$"if (candidate.Kind == ParserEntry.Capture && candidate.CallIndex == completedAt && " +
						$"({string.Join(" || ", slots)}))"))
					{
						file.Line($"if (captured{memberIndex}To < 0)");
						file.Then($"captured{memberIndex}To = candidate.Value;");
						file.Line($"captured{memberIndex}From = candidate.Position;");
					}
				}

				file.Line(
					$"var captured{memberIndex} = captured{memberIndex}From < 0 ? " +
					(member.IsOptional ? "null" : "string.Empty") + " : " +
					$"text.Slice(captured{memberIndex}From, captured{memberIndex}To - " +
					$"captured{memberIndex}From).ToString();");
				file.Line();
			}

		if (factories.Count == 0)
		{
			file.Line($"{ValueInto(type, "completedAt")} = new {type}(");

			using (file.Indent())
				for (var i = 0; i < members.Count; i++)
					file.Line(
						$"captured{i}{(members[i].IsOptional ? "" : "!")}" +
						(i + 1 < members.Count ? "," : ");"));
		}
		else
		{
			file.Line("var chosen = -1;");

			using (file.Block(
				"for (var chosenAt = links[completedAt]; chosenAt >= 0; " +
				"chosenAt = links[entries.Count + chosenAt])"))
			{
				file.Line("var candidate = entries[chosenAt];");

				using (file.Block(
					"if (candidate.Kind == ParserEntry.Construct && candidate.CallIndex == completedAt)"))
				{
					file.Line("chosen = candidate.State;");
					file.Line("break;");
				}
			}

			file.Line("global::System.Diagnostics.Debug.Assert(chosen >= 0);");

			using (file.Block("switch (chosen)"))
				for (var factoryIndex = 0; factoryIndex < factories.Count; factoryIndex++)
				{
					var factory = factories[factoryIndex];
					var arguments = new List<string>();

					// Materialized only where the expression names it. It is the whole of
					// what the rule matched, so building it for an expression that never
					// looks at it doubles what a parse allocates — twice the string, for a
					// rule whose value is the capture inside it.
					if (CSharpEmitter.WantsText(factory))
						arguments.Add(
							"text.Slice(completed.Position, completed.Value - completed.Position).ToString()");

					if (CSharpEmitter.Asks(factory, "parserSpan"))
						arguments.Add(
							"new SourceSpan(" +
							"completed.Position, completed.Value - completed.Position)");

					foreach (var member in factory.Members)
					{
						if (member.Name == "parserText" || member.Name == factory.Accumulator)
							continue;

						for (var memberIndex = 0; memberIndex < members.Count; memberIndex++)
							if (members[memberIndex].Name == member.Name)
							{
								arguments.Add(
									!member.IsOptional && members[memberIndex] is { Rule: not null, IsOptional: true }
										? $"({_results.ValueOf(members[memberIndex].Rule)})captured{memberIndex}!"
										: $"captured{memberIndex}{(member.IsOptional ? "" : "!")}");
								break;
							}
					}

					file.Line($"case {factoryIndex}:");

					using (file.Indent())
					{
						file.Line(
							$"{ValueInto(type, "completedAt")} = " +
							$"{factory.Method}({string.Join(", ", arguments)});");
						file.Line("break;");
					}
				}
		}

		file.Line("break;");
	}
	}

	void MaterializeFold(
		Writer file, RuleSymbol rule, string type, int offset,
		IReadOnlyList<Factory> factories)
	{
		file.Line($"{type} accumulated = default!;");
		file.Line("var hasAccumulated = false;");
		file.Line("var partFrom = completedAt + 1;");
		file.Line();

		using (file.Block(
			"for (var constructAt = completedAt + 1; constructAt < entries.Count; constructAt++)"))
		{
			file.Line("var construct = entries[constructAt];");
			file.Line(
				"if (construct.Kind != ParserEntry.Construct || construct.CallIndex != completedAt) continue;");
			file.Line();

			using (file.Block("switch (construct.State)"))
				for (var factoryIndex = 0; factoryIndex < factories.Count; factoryIndex++)
				{
					var factory = factories[factoryIndex];

					file.Line($"case {factoryIndex}:");

					using (file.Indent())
					using (file.Block(""))
					{
						var arguments = new List<string>();

						if (CSharpEmitter.WantsText(factory))
							arguments.Add(
								"text.Slice(completed.Position, completed.Value - completed.Position).ToString()");

						if (CSharpEmitter.Asks(factory, "parserSpan"))
							arguments.Add(
								"new SourceSpan(" +
								"completed.Position, completed.Value - completed.Position)");

						if (factory.Accumulator is not null)
						{
							file.Line("global::System.Diagnostics.Debug.Assert(hasAccumulated);");
							arguments.Add("accumulated");
						}

						var captured = 0;

						foreach (var member in factory.Members)
						{
							if (member.Name == "parserText" || member.Name == factory.Accumulator)
								continue;

							arguments.Add(MaterializeFoldMember(
								file, rule, member, captured++, offset,
								sequence: member.IsSequence && factory.Accumulator is null));
						}

						file.Line(
							$"accumulated = {factory.Method}({string.Join(", ", arguments)});");
						file.Line("hasAccumulated = true;");
						file.Line("break;");
					}
				}

			file.Line();
			file.Line("partFrom = constructAt + 1;");
		}

		file.Line();
		file.Line("global::System.Diagnostics.Debug.Assert(hasAccumulated);");
		file.Line($"{ValueInto(type, "completedAt")} = accumulated;");
	}

	string MaterializeFoldMember(
		Writer file, RuleSymbol rule, ResultMember member, int memberIndex, int offset, bool sequence)
	{
		var slots = new List<string>(member.Slots.Count);

		foreach (var slot in member.Slots)
			slots.Add($"candidate.State == {offset + slot}");

		var kind = member.Rule is null ? "ParserEntry.Capture" : "ParserEntry.RuleCapture";
		var test =
			$"candidate.Kind == {kind} && candidate.CallIndex == completedAt && " +
			$"({string.Join(" || ", slots)})";

		if (sequence)
		{
			var element = member.Rule is null ? "string" : _results.ValueOf(member.Rule);

			file.Line($"var foldCaptured{memberIndex}Count = 0;");

			using (file.Block(
				$"for (var candidateAt = partFrom; candidateAt < constructAt; candidateAt++)"))
			{
				file.Line("var candidate = entries[candidateAt];");
				file.Line($"if ({test}) foldCaptured{memberIndex}Count++;");
			}

			file.Line($"var foldCaptured{memberIndex} = new {element}[foldCaptured{memberIndex}Count];");
			file.Line($"var foldCaptured{memberIndex}Item = 0;");

			using (file.Block(
				$"for (var candidateAt = partFrom; candidateAt < constructAt; candidateAt++)"))
			{
				file.Line("var candidate = entries[candidateAt];");

				using (file.Block($"if ({test})"))
					file.Line(member.Rule is null
						? $"foldCaptured{memberIndex}[foldCaptured{memberIndex}Item++] = " +
							"text.Slice(candidate.Position, candidate.Value - candidate.Position).ToString();"
						: $"foldCaptured{memberIndex}[foldCaptured{memberIndex}Item++] = " +
							ValueFrom(element, "candidate.Position") + ";");
			}

			return $"foldCaptured{memberIndex}";
		}

		file.Line($"var foldCaptured{memberIndex}At = -1;");

		using (file.Block(
			$"for (var candidateAt = partFrom; candidateAt < constructAt; candidateAt++)"))
		{
			file.Line("var candidate = entries[candidateAt];");

			using (file.Block($"if ({test})"))
				file.Line($"foldCaptured{memberIndex}At = candidateAt;");
		}

		var type = member.Rule is null ? "string" : _results.ValueOf(member.Rule);

		if (!member.IsOptional)
			file.Line($"global::System.Diagnostics.Debug.Assert(foldCaptured{memberIndex}At >= 0);");

		file.Line(member.Rule is null
			? $"var foldCaptured{memberIndex} = foldCaptured{memberIndex}At < 0 ? " +
				(member.IsOptional ? "null" : "string.Empty") + " : " +
				$"text.Slice(entries[foldCaptured{memberIndex}At].Position, " +
				$"entries[foldCaptured{memberIndex}At].Value - " +
				$"entries[foldCaptured{memberIndex}At].Position).ToString();"
			: member.IsOptional
				? $"{type}? foldCaptured{memberIndex} = foldCaptured{memberIndex}At < 0 ? " +
					$"default({type}?) : " +
					ValueFrom(type, $"entries[foldCaptured{memberIndex}At].Position") + ";"
				: $"var foldCaptured{memberIndex} = " +
					ValueFrom(type, $"entries[foldCaptured{memberIndex}At].Position") + ";");

		return $"foldCaptured{memberIndex}{(member.IsOptional ? "" : "!")}";
	}

	static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

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
