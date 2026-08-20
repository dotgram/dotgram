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
	readonly HashSet<int> _textCaptures = [];
	readonly Dictionary<RuleSymbol, IReadOnlyList<Machine.Factory>> _factories = [];
	readonly Dictionary<Node, int> _constructs = new(NodeIdentity.Instance);
	readonly List<string> _extra = [];
	readonly ILineMap? _lines;
	bool _usesChar;
	int _guards;
	int _captures;

	public UnifiedMachine(RecognitionGraph graph, ResultTypes results, ILineMap? lines)
	{
		_graph = graph;
		_results = results;
		_lines = lines;

		foreach (var rule in graph.Rules)
		{
			var layout = CaptureLayout.Of(
				graph.Bodies[rule], other => graph.Results[other].Count > 0 || graph.Types.ContainsKey(other));
			var factories = CSharpEmitter.FactoriesOf(graph, results, rule);

			_captureOffsets[rule] = _captures;
			_factories[rule] = factories;

			foreach (var node in NodeWalk.Descendants(graph.Bodies[rule]))
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

			_captures += layout.Slots.Count;
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
		if (graph.Recoveries.Count > 0 || graph.Climbing.Count > 0)
			return false;

		foreach (var rule in graph.Rules)
		{
			foreach (var member in graph.Results[rule])
				if (member is { Rule: null, IsSequence: true } && !graph.Folds.ContainsKey(rule))
					return false;

			foreach (var node in NodeWalk.Descendants(graph.Bodies[rule]))
				if (node is not (Node.Empty or Node.Literal or Node.Element or Node.Sequence or
					Node.Choice or Node.Repeat or Node.Lookahead or Node.Guard or Node.Call or
					Node.External or Node.Atomic or Node.Capture or Node.Construct) ||
					node is Node.Guard && graph.Results[rule].Count > 0)
					return false;

			if (CaptureInsideLookahead(graph.Bodies[rule]))
				return false;
		}

		return true;
	}

	static int IndexOf(IReadOnlyList<Machine.Factory> factories, Node construct)
	{
		for (var i = 0; i < factories.Count; i++)
			if (ReferenceEquals(factories[i].Of, construct))
				return i;

		throw new InvalidOperationException("A construction has no factory.");
	}

	static bool CaptureInsideLookahead(Node root)
	{
		var pending = new Stack<(Node Node, bool Inside)>();
		pending.Push((root, false));

		while (pending.Count > 0)
		{
			var (node, inside) = pending.Pop();
			var nested = inside || node is Node.Lookahead;

			if (nested && node is Node.Capture)
				return true;

			foreach (var child in node.Children)
				pending.Push((child, nested));
		}

		return false;
	}

	public IReadOnlyList<string> Extra => _extra;

	public string Render(RuleSymbol root, string name)
	{
		var file = new Writer(0);
		var type = _results.QualifiedOf(root);
		var output = type is null ? "" : $", out {type} value";
		var entry = _graph.Trivia.TryGetValue(root, out var trivia)
			? Compile(new Node.Sequence([trivia, _graph.Bodies[root], trivia]), Return)
			: _entries[root];

		using (file.Block(
			$"static int {name}(global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure{output})"))
		{
			if (type is not null)
			{
				file.Line("value = default!;");
				file.Line();
			}

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

				if (_usesChar)
					file.Line("var c       = '\\0';");

				for (var i = 0; i < _captures; i++)
					if (_textCaptures.Contains(i))
						file.Line($"var capture{i} = 0;");

				file.Line($"var state   = {entry};");
				file.Line();
				file.Line(
					$"entries.Add(new ParserEntry(ParserEntry.Call, {Accept}, pos, -1, -1, -1, -1, " +
					$"0, {ValueRule(root)}));");
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
				file.Line("var previousCall = returned.CallIndex;");
				file.Line("repeat = returned.RepeatIndex;");
				file.Line("lookahead = returned.LookaheadIndex;");
				file.Line();
				using (file.Block("if (returned.RuleIndex >= 0)"))
				{
					file.Line(
						"entries[call] = new ParserEntry(ParserEntry.Completed, returned.State, " +
						"returned.Position, returned.CallIndex, returned.AtomicIndex, " +
						"returned.RepeatIndex, returned.LookaheadIndex, p, returned.RuleIndex);");
				}
				file.Line("else if (entries.Count == call + 1)");
				file.Then("entries.RemoveAt(call);");
				file.Line();
				file.Line("call = previousCall;");
				file.Line("Trace(\"return\", state, p, entries.Count);");
				file.Line("goto Dispatch;");

				file.Line();
				file.Line("Accept:");
				file.Line("if (p != text.Length) goto Fail;");

				if (type is not null)
				{
					file.Line();
					Materialize(file, root, type);
				}

				file.Line("return p;");

				file.Line();
				file.Line("Fail:");
				file.Line("if (lookahead < 0 && p > failure.Position)");
				file.Then("failure.Position = p;");
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

					if (_captures > 0 || _constructs.Count > 0)
					{
						file.Line(
							"if (entry.Kind == ParserEntry.Capture || " +
							"entry.Kind == ParserEntry.Construct || entry.Kind == ParserEntry.RuleCapture)");
						file.Then("continue;");
						file.Line();
					}

					using (file.Block("if (entry.Kind == ParserEntry.Call || entry.Kind == ParserEntry.Completed)"))
					{
						file.Line("call   = entry.CallIndex;");
						file.Line("atomic = entry.AtomicIndex;");
						file.Line("repeat = entry.RepeatIndex;");
						file.Line("lookahead = entry.LookaheadIndex;");
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
		switch (node)
		{
			case Node.Empty:
				return next;

			case Node.Literal(var value):
			{
				var state = Reserve(out var writer);

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

				writer.Line("var callIndex = entries.Count;");
				writer.Line(
					$"entries.Add(new ParserEntry(ParserEntry.Call, {next}, p, call, atomic, repeat, " +
					$"lookahead, 0, {ValueRule(rule)}));");
				writer.Line("call = callIndex;");
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
				var method = "Recognize_DotGram_Guard" + _guards++;
				var helper = new Writer(0);

				helper.Line($"static bool {method}(string parserText) =>");
				CSharpEmitter.Handed(
					helper, _lines, node is Node.Guard { At: var at } ? at : -1, condition + ";");
				_extra.Add(helper.ToString());

				var state = Reserve(out var writer);

				writer.Line("global::System.Diagnostics.Debug.Assert(call >= 0 && call < entries.Count);");
				writer.Line("var ruleStart = entries[call].Position;");
				writer.Line($"if (!{method}(text.Slice(ruleStart, p - ruleStart).ToString())) goto Fail;");
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
				atCommit.Line("entries.RemoveRange(atomic, entries.Count - atomic);");
				atCommit.Line("atomic = boundary.AtomicIndex;");
				atCommit.Line("repeat = boundary.RepeatIndex;");
				atCommit.Line("lookahead = boundary.LookaheadIndex;");
				atCommit.Line($"Trace(\"commit\", {next}, p, entries.Count);");
				atCommit.Line($"goto {Label(next)};");

				return state;
			}

			case Node.Repeat(var body, var min, var max):
			{
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

				atExit.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
				atExit.Line("var finished = entries[repeat];");
				atExit.Line("global::System.Diagnostics.Debug.Assert(finished.Kind == ParserEntry.Repeat);");
				atExit.Line("var previousRepeat = finished.RepeatIndex;");
				atExit.Line("if (entries.Count == repeat + 1) entries.RemoveAt(repeat);");
				atExit.Line("repeat = previousRepeat;");
				atExit.Line("lookahead = finished.LookaheadIndex;");
				atExit.Line($"Trace(\"leave repeat\", {next}, p, entries.Count);");
				atExit.Line($"goto {Label(next)};");

				return entry;
			}

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

	int ValueRule(RuleSymbol rule) =>
		_graph.Results[rule].Count > 0 || _graph.Types.ContainsKey(rule) ? _ruleIds[rule] : -1;

	int Reserve(out Writer writer)
	{
		writer = new Writer(0);
		_states.Add(writer);

		return _states.Count - 1 + First;
	}

	void Materialize(Writer file, RuleSymbol root, string type)
	{
		file.Line("var values = parser.Materialization(entries.Count);");
		file.Line("var links  = parser.MaterializationLinks(entries.Count);");
		file.Line();

		using (file.Block("for (var derivationAt = 0; derivationAt < entries.Count; derivationAt++)"))
		{
			file.Line("var derivation = entries[derivationAt];");

			using (file.Block(
				"if (derivation.CallIndex >= 0 && (derivation.Kind == ParserEntry.Capture || " +
				"derivation.Kind == ParserEntry.RuleCapture || derivation.Kind == ParserEntry.Construct))"))
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

		file.Line($"value = ({type})values[0]!;");
	}

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

						file.Line($"var captured{memberIndex}Count = 0;");

						using (file.Block(
							$"for (var capturedAt{memberIndex} = links[completedAt]; capturedAt{memberIndex} >= 0; " +
							$"capturedAt{memberIndex} = links[entries.Count + capturedAt{memberIndex}])"))
						{
							file.Line($"var candidate = entries[capturedAt{memberIndex}];");
							file.Line(
								$"if (candidate.Kind == ParserEntry.RuleCapture && candidate.CallIndex == completedAt && " +
								$"({string.Join(" || ", slots)})) captured{memberIndex}Count++;");
						}

						file.Line($"var captured{memberIndex} = new {element}[captured{memberIndex}Count];");
						file.Line($"var captured{memberIndex}Item = captured{memberIndex}Count;");

						using (file.Block(
							$"for (var capturedAt{memberIndex} = links[completedAt]; capturedAt{memberIndex} >= 0; " +
							$"capturedAt{memberIndex} = links[entries.Count + capturedAt{memberIndex}])"))
						{
							file.Line($"var candidate = entries[capturedAt{memberIndex}];");

							using (file.Block(
								$"if (candidate.Kind == ParserEntry.RuleCapture && candidate.CallIndex == completedAt && " +
								$"({string.Join(" || ", slots)}))"))
							{
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
