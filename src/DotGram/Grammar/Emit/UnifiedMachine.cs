using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// A single threaded automaton for a whole recognition graph. Rules are shared label
/// blocks, not C# calls; a call records its continuation in the parser arena.
/// </summary>
sealed class UnifiedMachine
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
	readonly Dictionary<RuleSymbol, IReadOnlyList<Machine.Factory>> _factories = [];
	readonly Dictionary<Node, int> _constructs = new(NodeIdentity.Instance);
	readonly Dictionary<Node, RecoveryPlan> _recoveries = new(NodeIdentity.Instance);
	readonly List<RecoveryPlan> _recoveryPlans = [];
	readonly Dictionary<RuleSymbol, int> _wholeEntries = [];
	readonly List<string> _extra = [];
	readonly ILineMap? _lines;
	readonly bool _starves;
	bool _usesChar;
	int _guards;
	int _captures;

	public UnifiedMachine(RecognitionGraph graph, ResultTypes results, ILineMap? lines, bool starves = false)
	{
		_graph = graph;
		_results = results;
		_lines = lines;
		_starves = starves;

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

		foreach (var rule in graph.Rules)
		{
			var body = Compile(graph.Bodies[rule], Return);
			var entry = _states[_entries[rule] - First];

			entry.Line($"Trace(\"enter {Escape(rule.Name)}\", {_entries[rule]}, p, entries.Count);");
			entry.Line($"goto {Label(body)};");
		}
	}

	public static bool Supports(RecognitionGraph graph)
	{
		foreach (var rule in graph.Rules)
		{
			var layout = CaptureLayout.Of(
				graph.Bodies[rule], other => graph.Results[other].Count > 0 || graph.Types.ContainsKey(other),
				graph.Folds.TryGetValue(rule, out var fold) ? fold.Loop : null);

			foreach (var node in NodeWalk.Descendants(graph.Bodies[rule]))
				if (node is not (Node.Empty or Node.Literal or Node.Element or Node.Sequence or
					Node.Choice or Node.Repeat or Node.Lookahead or Node.Guard or Node.Call or
					Node.External or Node.Atomic or Node.Capture or Node.Construct) ||
					node is Node.Guard && !SupportsGuard(graph, rule, layout, node))
					return false;
		}

		return true;
	}

	sealed record RecoveryPlan(
		RuleSymbol Rule, Recovery Recovery, int Slot, int Id, string Method, RuleSymbol? Element);

	static bool SupportsGuard(
		RecognitionGraph graph, RuleSymbol rule, CaptureLayout layout, Node guard)
	{
		var before = layout.Before(guard);

		foreach (var member in graph.Results[rule])
			foreach (var slot in member.Slots)
				if (slot < before && (member.Rule is not null || member.IsSequence))
					return false;

		return true;
	}

	static int IndexOf(IReadOnlyList<Machine.Factory> factories, Node construct)
	{
		for (var i = 0; i < factories.Count; i++)
			if (ReferenceEquals(factories[i].Of, construct))
				return i;

		throw new InvalidOperationException("A construction has no factory.");
	}

	public IReadOnlyList<string> Extra => _extra;

	public void Register(RuleSymbol root, bool whole)
	{
		if (!whole || _wholeEntries.ContainsKey(root))
			return;

		_wholeEntries[root] = _graph.Trivia.TryGetValue(root, out var trivia)
			? Compile(new Node.Sequence([trivia, _graph.Bodies[root], trivia]), Return)
			: _entries[root];
	}

	public int Register(Node node) => Compile(node, Return);

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

			if (type is not null)
				file.Line($"value = end < 0 ? default! : ({type})recognized!;");

			file.Line("return end;");
		}

		return file.ToString();
	}

	public string RenderEngine(string name)
	{
		var file = new Writer(0);
		var strength = _graph.Climbing.Count > 0 ? ", int initialPower" : "";

		using (file.Block(
			$"static int {name}(global::System.ReadOnlySpan<char> text, int pos, int state, " +
			$"int rootRule{strength}, bool whole, bool materialize, ref {CSharpEmitter.FailureType} failure, " +
			"out object? recognized)"))
		{
			file.Line("recognized = null;");
			file.Line();

			file.Line("Parser parser = null!;");
			file.Line("RentParser(ref parser);");
			file.Line("parser ??= new Parser();");
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

				for (var i = 0; i < _captures; i++)
					if (_textCaptures.Contains(i))
						file.Line($"var capture{i} = 0;");

				file.Line();
				file.Line(
					$"entries.Add(new ParserEntry(ParserEntry.Call, {Accept}, pos, -1, -1, -1, -1, " +
					"0, rootRule));");
				file.Line("call = 0;");
				file.Line("goto Dispatch;");

				for (var i = 0; i < _states.Count; i++)
				{
					file.Line();
					file.Line($"S{i + First}:");

					using (file.Block(""))
						file.AppendIndented(_states[i], 0);
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
				file.Line("else if (entries.Count == call + 1)");
				file.Then("entries.RemoveAt(call);");
				file.Line();
				file.Line("call = previousCall;");
				file.Line("Trace(\"return\", state, p, entries.Count);");
				file.Line("goto Dispatch;");

				file.Line();
				file.Line("Accept:");
				file.Line("if (whole && p != text.Length) goto Fail;");

				var hasValues = false;

				foreach (var rule in _graph.Rules)
					hasValues |= ValueRule(rule) >= 0;

				if (hasValues || _recoveryPlans.Count > 0)
				{
					using (file.Block("if (materialize)"))
					{
						if (hasValues)
						{
							using (file.Block("if (rootRule >= 0)"))
							{
								file.Line();
								Materialize(file);
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

					if (_captures > 0 || _constructs.Count > 0 || _recoveries.Count > 0)
					{
						var ignored =
							"entry.Kind == ParserEntry.Capture || entry.Kind == ParserEntry.Construct || " +
							"entry.Kind == ParserEntry.RuleCapture";

						if (_recoveries.Count > 0)
							ignored += " || entry.Kind == ParserEntry.Recovery || entry.Kind == ParserEntry.Dead || " +
								"entry.Kind == ParserEntry.PendingRecovery";

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

				file.Line();
				file.Line("Dispatch:");

				using (file.Block("switch (state)"))
				{
					file.Line($"case {Return}: goto Return;");
					file.Line($"case {Accept}: goto Accept;");
					file.Line($"case {Fail}:   goto Fail;");

					for (var i = 0; i < _states.Count; i++)
						file.Line($"case {i + First}: goto S{i + First};");

					file.Line("default: goto Fail;");
				}
			}

			file.Line("finally");

			using (file.Block(""))
			{
				file.Line("parser.Reset();");
				file.Line("ReturnParser(parser);");
			}
		}

		return file.ToString();
	}

	int Compile(Node node, int next)
	{
		if (_owners.TryGetValue(node, out var owner) &&
			_graph.Climbing.TryGetValue(owner, out var levels) &&
			levels.TryGetValue(node, out var level))
		{
			var inner = CompileUnguarded(node, next);
			var state = Reserve(out var writer);

			writer.Line($"if ({level} < power) goto Fail;");
			writer.Line($"goto {Label(inner)};");

			return state;
		}

		return CompileUnguarded(node, next);
	}

	int CompileUnguarded(Node node, int next)
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
						writer.Line("goto Fail;");
					}
				}
				else
					writer.Line($"if (p + {value.Length} > text.Length) goto Fail;");

				for (var i = 0; i < value.Length; i++)
					writer.Line($"if (text[p + {i}] != {CSharpEmitter.Char(value[i])}) goto Fail;");

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
					writer.Line("goto Fail;");

					return state;
				}

				if (_starves)
				{
					writer.Line("if (p >= text.Length)");
					using (writer.Block(""))
					{
						writer.Line("failure.Starved = true;");
						writer.Line("goto Fail;");
					}
				}
				else
					writer.Line("if (p >= text.Length) goto Fail;");

				if (test != "true")
				{
					_usesChar = true;
					writer.Line("c = text[p];");
					writer.Line($"if (!({test})) goto Fail;");
				}

				writer.Line("p++;");
				writer.Line($"goto {Label(next)};");

				return state;
			}

			case Node.Sequence(var nodes):
			{
				var target = next;

				for (var i = nodes.Count - 1; i >= 0; i--)
					target = Compile(nodes[i], target);

				return target;
			}

			case Node.Choice(var alternatives):
			{
				var target = Compile(alternatives[alternatives.Count - 1], next);

				for (var i = alternatives.Count - 2; i >= 0; i--)
				{
					var first = Compile(alternatives[i], next);
					var state = Reserve(out var writer);

					writer.Line($"entries.Add(new ParserEntry(ParserEntry.Choice, {target}, p, call, atomic, repeat, lookahead, 0));");
					writer.Line($"Trace(\"push choice\", {target}, p, entries.Count);");
					writer.Line($"goto {Label(first)};");
					target = state;
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
				var inner = Compile(body, close);
				var state = Reserve(out var writer);

				if (body is Node.Call(var capturedRule, _) && ValueRule(capturedRule) >= 0)
				{
					writer.Line($"goto {Label(inner)};");
					atClose.Line("var capturedCall = entries.Count - 1;");
					atClose.Line(
						$"while (capturedCall >= 0 && !(entries[capturedCall].Kind == ParserEntry.Completed && " +
						$"entries[capturedCall].CallIndex == call && entries[capturedCall].RuleIndex == " +
						$"{_ruleIds[capturedRule]} && entries[capturedCall].Value == p)) capturedCall--;");
					atClose.Line("global::System.Diagnostics.Debug.Assert(capturedCall >= 0);");
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
				var inner   = Compile(body, close);
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
				var parameters = new List<string> { "string parserText" };
				var arguments = new List<string>
				{
					"text.Slice(ruleStart, p - ruleStart).ToString()",
				};
				var visible = new List<(ResultMember Member, IReadOnlyList<int> Slots)>();

				foreach (var member in _graph.Results[rule])
				{
					var slots = new List<int>();

					foreach (var slot in member.Slots)
						if (slot < before)
							slots.Add(slot);

					if (slots.Count == 0)
						continue;

					var optional = member.IsOptional || slots.Count != member.Slots.Count;

					parameters.Add($"string{(optional ? "?" : "")} {ResultTypes.ParameterOf(member)}");
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

				for (var memberIndex = 0; memberIndex < visible.Count; memberIndex++)
				{
					var (member, slots) = visible[memberIndex];
					var tests = new List<string>(slots.Count);

					foreach (var slot in slots)
						tests.Add($"candidate.State == {_captureOffsets[rule] + slot}");

					writer.Line($"var guardCaptured{memberIndex}At = -1;");

					using (writer.Block("for (var candidateAt = entries.Count - 1; candidateAt > call; candidateAt--)"))
					{
						writer.Line("var candidate = entries[candidateAt];");

						using (writer.Block(
							"if (candidate.Kind == ParserEntry.Capture && candidate.CallIndex == call && " +
							$"({string.Join(" || ", tests)}))"))
						{
							writer.Line($"guardCaptured{memberIndex}At = candidateAt;");
							writer.Line("break;");
						}
					}

					writer.Line(
						$"var guardCaptured{memberIndex} = guardCaptured{memberIndex}At < 0 ? " +
						(member.IsOptional ? "null" : "string.Empty") + " : " +
						$"text.Slice(entries[guardCaptured{memberIndex}At].Position, " +
						$"entries[guardCaptured{memberIndex}At].Value - " +
						$"entries[guardCaptured{memberIndex}At].Position).ToString();");
				}

				writer.Line($"if (!{method}({string.Join(", ", arguments)})) goto Fail;");
				writer.Line($"goto {Label(next)};");

				return state;
			}

			case Node.Atomic(var body):
			{
				var commit = Reserve(out var atCommit);
				var inner  = Compile(body, commit);
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
				atCommit.Line("entries.RemoveRange(atomic, entries.Count - atomic);");
				atCommit.Line("atomic = boundary.AtomicIndex;");
				atCommit.Line("repeat = boundary.RepeatIndex;");
				atCommit.Line("lookahead = boundary.LookaheadIndex;");
				atCommit.Line($"Trace(\"commit\", {next}, p, entries.Count);");
				atCommit.Line($"goto {Label(next)};");

				return state;
			}

			case Node.Repeat repeat:
				return _recoveries.TryGetValue(node, out var recovery)
					? CompileRecoveringRepeat(repeat, recovery, next)
					: CompileRepeat(repeat, next);

			case Node.Lookahead(var isPositive, var body):
			{
				var success = Reserve(out var atSuccess);
				var inner   = Compile(body, success);
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

	int CompileLookaheadCapture(int slot, Node seen, int next)
	{
		var success = Reserve(out var atSuccess);
		var inner   = Compile(seen, success);
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

	int CompileNegativeLookaheadCapture(int slot, Node rejected, int next)
	{
		var matched = Reserve(out var atMatched);
		var inner   = Compile(rejected, matched);
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
		atMatched.Line("entries.RemoveRange(lookahead, entries.Count - lookahead);");
		atMatched.Line("p         = looked.Position;");
		atMatched.Line("call      = looked.CallIndex;");
		atMatched.Line("atomic    = looked.AtomicIndex;");
		atMatched.Line("repeat    = looked.RepeatIndex;");
		atMatched.Line("lookahead = looked.LookaheadIndex;");
		atMatched.Line("goto Fail;");

		return state;
	}

	int CompileRepeat(Node.Repeat repeatNode, int next)
	{
		var (body, min, max) = repeatNode;

		if (max == 0)
			return next;

		var exit  = Reserve(out var atExit);
		var loop  = Reserve(out var atLoop);
		var after = Reserve(out var atAfter);
		var entry = Reserve(out var atEntry);
		var inner = Compile(body, after);

		atEntry.Line("var repeatIndex = entries.Count;");
		atEntry.Line("entries.Add(new ParserEntry(ParserEntry.Repeat, 0, p, call, atomic, repeat, lookahead, 0));");
		atEntry.Line("repeat = repeatIndex;");
		atEntry.Line($"Trace(\"enter repeat\", {loop}, p, entries.Count);");
		atEntry.Line($"goto {Label(loop)};");

		atLoop.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
		atLoop.Line("var repeating = entries[repeat];");
		atLoop.Line("global::System.Diagnostics.Debug.Assert(repeating.Kind == ParserEntry.Repeat);");

		if (max is { } limit)
			atLoop.Line($"if (repeating.Value >= {limit}) goto {Label(exit)};");

		if (min == 0)
			PushRepeatExit(atLoop, exit);
		else
		{
			atLoop.Line($"if (repeating.Value >= {min})");
			atLoop.Then(
				$"entries.Add(new ParserEntry(ParserEntry.Choice, {exit}, p, call, atomic, repeat, lookahead, 0));");
		}

		atLoop.Line($"goto {Label(inner)};");

		atAfter.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
		atAfter.Line("var repeated = entries[repeat];");
		atAfter.Line(
			"entries[repeat] = new ParserEntry(ParserEntry.Repeat, 0, repeated.Position, " +
			"repeated.CallIndex, repeated.AtomicIndex, repeated.RepeatIndex, " +
			"repeated.LookaheadIndex, repeated.Value + 1);");
		atAfter.Line($"goto {Label(loop)};");

		LeaveRepeat(atExit, next);

		return entry;
	}

	int CompileRecoveringRepeat(Node.Repeat repeatNode, RecoveryPlan recovery, int next)
	{
		var (body, min, max) = repeatNode;

		if (max == 0)
			return next;

		var exit      = Reserve(out var atExit);
		var loop      = Reserve(out var atLoop);
		var attempt   = Reserve(out var atAttempt);
		var recovered = Reserve(out var atRecovered);
		var synced    = Reserve(out var atSynced);
		var advance   = Reserve(out var atAdvance);
		var scan      = Reserve(out var atScan);
		var asked     = Reserve(out var atAsked);
		var after     = Reserve(out var atAfter);
		var entry     = Reserve(out var atEntry);
		var inner     = Compile(body, after);
		var sync      = Compile(recovery.Recovery.Sync, synced);

		atEntry.Line("var repeatIndex = entries.Count;");
		atEntry.Line("entries.Add(new ParserEntry(ParserEntry.Repeat, 0, p, call, atomic, repeat, lookahead, 0));");
		atEntry.Line("repeat = repeatIndex;");
		atEntry.Line($"goto {Label(loop)};");

		atLoop.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
		atLoop.Line("var repeating = entries[repeat];");

		if (max is { } limit)
			atLoop.Line($"if (repeating.Value >= {limit}) goto {Label(exit)};");

		atLoop.Line($"if (repeating.Value >= {min})");
		atLoop.Then($"entries.Add(new ParserEntry(ParserEntry.Choice, {attempt}, p, call, atomic, repeat, lookahead, 0));");
		atLoop.Line($"if (repeating.Value >= {min}) goto {Label(exit)};");
		atLoop.Line($"goto {Label(attempt)};");

		atAttempt.Line("reach = p;");
		atAttempt.Line("owned = false;");
		atAttempt.Line($"entries.Add(new ParserEntry(ParserEntry.Choice, {asked}, p, call, atomic, repeat, lookahead, 0));");
		atAttempt.Line($"goto {Label(inner)};");

		atAsked.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
		atAsked.Line("if (!owned && reach <= p) goto Fail;");
		atAsked.Line(
			$"entries.Add(new ParserEntry(ParserEntry.PendingRecovery, {asked}, p, call, reach, repeat, lookahead, 0));");
		atAsked.Line($"goto {Label(scan)};");

		atScan.Line($"if (p >= text.Length) goto {Label(recovered)};");
		atScan.Line("syncFrom = p;");
		atScan.Line($"entries.Add(new ParserEntry(ParserEntry.Choice, {advance}, p, call, atomic, repeat, lookahead, 0));");
		atScan.Line($"goto {Label(sync)};");

		atSynced.Line($"if (p <= syncFrom) goto Fail;");
		atSynced.Line($"goto {Label(recovered)};");

		atAdvance.Line("p++;");
		atAdvance.Line($"goto {Label(scan)};");

		atRecovered.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
		atRecovered.Line("var recoveryFrom = p;");
		atRecovered.Line("var recoveryReach = p;");
		atRecovered.Line("var recoveryTo = p;");
		atRecovered.Line("var recoveryBoundary = false;");
		atRecovered.Line("for (var recoveryAt = entries.Count - 1; recoveryAt > repeat; recoveryAt--)");
		using (atRecovered.Block(""))
		{
			atRecovered.Line("var candidate = entries[recoveryAt];");
			atRecovered.Line($"if (candidate.Kind == ParserEntry.PendingRecovery && candidate.State == {asked})");
			using (atRecovered.Block(""))
			{
				atRecovered.Line("recoveryFrom = candidate.Position;");
				atRecovered.Line("recoveryReach = candidate.AtomicIndex;");
			}
			atRecovered.Line($"if (!recoveryBoundary && candidate.Kind == ParserEntry.Choice && candidate.State == {advance})");
			using (atRecovered.Block(""))
			{
				atRecovered.Line("recoveryTo = candidate.Position;");
				atRecovered.Line("recoveryBoundary = true;");
			}
		}
		DeactivateChoices(atRecovered, "repeat");
		atRecovered.Line(
			$"entries.Add(new ParserEntry(ParserEntry.Recovery, {recovery.Id}, recoveryFrom, call, recoveryReach, " +
			"repeat, lookahead, recoveryTo, entries[repeat].Value));");
		atRecovered.Line("var recoveredRepeat = entries[repeat];");
		atRecovered.Line(
			"entries[repeat] = new ParserEntry(ParserEntry.Repeat, 0, recoveredRepeat.Position, " +
			"recoveredRepeat.CallIndex, recoveredRepeat.AtomicIndex, recoveredRepeat.RepeatIndex, " +
			"recoveredRepeat.LookaheadIndex, recoveredRepeat.Value + 1);");
		atRecovered.Line($"goto {Label(loop)};");

		DeactivateChoices(atAfter, "repeat");
		atAfter.Line("var acceptedRepeat = entries[repeat];");
		atAfter.Line(
			"entries[repeat] = new ParserEntry(ParserEntry.Repeat, 0, acceptedRepeat.Position, " +
			"acceptedRepeat.CallIndex, acceptedRepeat.AtomicIndex, acceptedRepeat.RepeatIndex, " +
			"acceptedRepeat.LookaheadIndex, acceptedRepeat.Value + 1);");
		atAfter.Line($"goto {Label(loop)};");

		LeaveRepeat(atExit, next);

		return entry;
	}

	static void DeactivateChoices(Writer writer, string from)
	{
		using (writer.Block($"for (var choiceAt = entries.Count - 1; choiceAt > {from}; choiceAt--)"))
		{
			writer.Line("var choice = entries[choiceAt];");
			writer.Line("if (choice.Kind != ParserEntry.Choice) continue;");
			writer.Line(
				"entries[choiceAt] = new ParserEntry(ParserEntry.Dead, choice.State, choice.Position, " +
				"choice.CallIndex, choice.AtomicIndex, choice.RepeatIndex, choice.LookaheadIndex, " +
				"choice.Value, choice.RuleIndex);");
		}
	}

	static void LeaveRepeat(Writer writer, int next)
	{
		writer.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
		writer.Line("var finished = entries[repeat];");
		writer.Line("global::System.Diagnostics.Debug.Assert(finished.Kind == ParserEntry.Repeat);");
		writer.Line("var previousRepeat = finished.RepeatIndex;");
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

	void Materialize(Writer file)
	{
		file.Line("var values = parser.Materialization(entries.Count);");
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
		file.Line("values[0] = parser;");

		using (file.Block("for (var ownerAt = 0; ownerAt < entries.Count; ownerAt++)"))
		{
			file.Line("if (!global::System.Object.ReferenceEquals(values[ownerAt], parser)) continue;");

			using (file.Block(
				"for (var capturedAt = links[ownerAt]; capturedAt >= 0; " +
				"capturedAt = links[entries.Count + capturedAt])"))
			{
				file.Line("var candidate = entries[capturedAt];");
				file.Line("if (candidate.Kind == ParserEntry.RuleCapture)");
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
					"!global::System.Object.ReferenceEquals(values[recovered.CallIndex], parser)) continue;");

				using (file.Block("switch (recovered.State)"))
					foreach (var recovery in _recoveryPlans)
						MaterializeRecovery(file, recovery);
			}
		}

		using (file.Block(
			"for (var completedAt = entries.Count - 1; completedAt >= 0; completedAt--)"))
		{
			file.Line("var completed = entries[completedAt];");
			file.Line(
				"if (completed.Kind != ParserEntry.Completed || " +
				"!global::System.Object.ReferenceEquals(values[completedAt], parser)) continue;");

			using (file.Block("switch (completed.RuleIndex)"))
				foreach (var rule in _graph.Rules)
					if (ValueRule(rule) >= 0)
						MaterializeRule(file, rule);
		}

		file.Line("recognized = values[0];");
	}

	void ReportRecoveries(Writer file)
	{
		using (file.Block("for (var recoveryAt = 0; recoveryAt < entries.Count; recoveryAt++)"))
		{
			file.Line("var recovered = entries[recoveryAt];");
			file.Line("if (recovered.Kind != ParserEntry.Recovery) continue;");

			using (file.Block("switch (recovered.State)"))
				foreach (var recovery in _recoveryPlans)
					using (file.Block($"case {recovery.Id}:"))
					{
						if (recovery.Recovery.Factory is null)
							file.Line(
								$"{CSharpEmitter.RecoveredMethod}(\"{Escape(recovery.Element?.Name ?? "an element")}\", " +
								$"{RecoverySupplied("parserText", recovery)}, {RecoverySupplied("parserPosition", recovery)}, " +
								$"{RecoverySupplied("parserLine", recovery)}, {RecoverySupplied("parserColumn", recovery)}, " +
								$"{RecoverySupplied("parserOrdinal", recovery)}, {RecoverySupplied("parserMessage", recovery)});");

						file.Line("break;");
					}
		}
	}

	void MaterializeRecovery(Writer file, RecoveryPlan plan)
	{
		using (file.Block($"case {plan.Id}:"))
		{
			if (plan.Recovery.Factory is null)
			{
				file.Line(
					$"{CSharpEmitter.RecoveredMethod}(\"{Escape(plan.Element?.Name ?? "an element")}\", " +
					$"{RecoverySupplied("parserText", plan)}, {RecoverySupplied("parserPosition", plan)}, " +
					$"{RecoverySupplied("parserLine", plan)}, {RecoverySupplied("parserColumn", plan)}, " +
					$"{RecoverySupplied("parserOrdinal", plan)}, {RecoverySupplied("parserMessage", plan)});");
			}
			else if (plan.Slot >= 0)
			{
				var arguments = new List<string>();

				foreach (var name in plan.Recovery.Asks)
					arguments.Add(RecoverySupplied(name, plan));

				file.Line($"values[recoveryAt] = {plan.Method}({string.Join(", ", arguments)});");
			}

			file.Line("break;");
		}
	}

	static string RecoverySupplied(string name, RecoveryPlan plan) => name switch
	{
		"parserText"     => "text.Slice(recovered.Position, recovered.Value - recovered.Position).ToString()",
		"parserPosition" => "recovered.Position",
		"parserOrdinal"  => "recovered.RuleIndex",
		"parserLine"     => "LineAt(text, recovered.Position)",
		"parserColumn"   => "ColumnAt(text, recovered.Position)",
		"parserSpan"     => "new global::DotGram.SourceSpan(recovered.Position, recovered.Value - recovered.Position)",
		"parserMessage"  => $"\"Input does not match '{Escape(plan.Element?.Name ?? "an element")}' at \" + " +
			"recovered.AtomicIndex.ToString(global::System.Globalization.CultureInfo.InvariantCulture) + \".\"",
		_                => "default",
	};

	void MaterializeRule(Writer file, RuleSymbol rule)
	{
		var offset    = _captureOffsets[rule];
		var members   = _graph.Results[rule];
		var type      = _results.QualifiedOf(rule)!;
		var factories = _factories[rule];

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
										$"({element})values[capturedValueAt]!;");
								}
								else
									file.Line(
										$"captured{memberIndex}[--captured{memberIndex}Item] = " +
										$"({element})values[candidate.Position]!;");
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
							$"default({capturedType}?) : ({capturedType})values[captured{memberIndex}At]!;"
						: $"var captured{memberIndex} = ({capturedType})values[captured{memberIndex}At]!;");
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
			file.Line($"values[completedAt] = new {type}(");

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
					var arguments = new List<string>
					{
						"text.Slice(completed.Position, completed.Value - completed.Position).ToString()",
					};

					if (CSharpEmitter.Asks(factory, "parserSpan"))
						arguments.Add(
							"new global::DotGram.SourceSpan(" +
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
							$"values[completedAt] = {factory.Method}({string.Join(", ", arguments)});");
						file.Line("break;");
					}
				}
		}

		file.Line("break;");
	}
	}

	void MaterializeFold(
		Writer file, RuleSymbol rule, string type, int offset,
		IReadOnlyList<Machine.Factory> factories)
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
						var arguments = new List<string>
						{
							"text.Slice(completed.Position, completed.Value - completed.Position).ToString()",
						};

						if (CSharpEmitter.Asks(factory, "parserSpan"))
							arguments.Add(
								"new global::DotGram.SourceSpan(" +
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
		file.Line("values[completedAt] = accumulated;");
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
							$"({element})values[candidate.Position]!;");
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
					$"default({type}?) : ({type})values[entries[foldCaptured{memberIndex}At].Position]!;"
				: $"var foldCaptured{memberIndex} = " +
					$"({type})values[entries[foldCaptured{memberIndex}At].Position]!;");

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
