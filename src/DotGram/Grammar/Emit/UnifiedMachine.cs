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
	readonly List<Writer> _states = [];
	readonly Dictionary<RuleSymbol, int> _entries = [];
	bool _usesChar;

	public UnifiedMachine(RecognitionGraph graph)
	{
		_graph = graph;

		foreach (var rule in graph.Rules)
			_entries[rule] = Reserve(out _);

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
		if (graph.Types.Count > 0 || graph.Recoveries.Count > 0 || graph.Climbing.Count > 0)
			return false;

		foreach (var rule in graph.Rules)
			foreach (var node in NodeWalk.Descendants(graph.Bodies[rule]))
				if (node is not (Node.Empty or Node.Literal or Node.Element or Node.Sequence or
					Node.Choice or Node.Call or Node.Atomic))
					return false;

		return true;
	}

	public string Render(RuleSymbol root, string name)
	{
		var file = new Writer(0);

		using (file.Block(
			$"static int {name}(global::System.ReadOnlySpan<char> text, int pos, ref {CSharpEmitter.FailureType} failure)"))
		{
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

				if (_usesChar)
					file.Line("var c       = '\\0';");
				file.Line($"var state   = {_entries[root]};");
				file.Line();
				file.Line($"entries.Add(new ParserEntry(ParserEntry.Call, {Accept}, pos, -1, -1));");
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
				file.Line("global::System.Diagnostics.Debug.Assert(returned.Kind == ParserEntry.Call);");
				file.Line("state = returned.State;");
				file.Line("var previousCall = returned.CallIndex;");
				file.Line();
				file.Line("if (entries.Count == call + 1)");
				file.Then("entries.RemoveAt(call);");
				file.Line();
				file.Line("call = previousCall;");
				file.Line("Trace(\"return\", state, p, entries.Count);");
				file.Line("goto Dispatch;");

				file.Line();
				file.Line("Accept:");
				file.Line("if (p == text.Length)");
				file.Then("return p;");
				file.Line("goto Fail;");

				file.Line();
				file.Line("Fail:");
				file.Line("if (p > failure.Position)");
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
						file.Line("Trace(\"resume\", state, p, entries.Count);");
						file.Line("goto Dispatch;");
					}

					using (file.Block("if (entry.Kind == ParserEntry.Call)"))
					{
						file.Line("call   = entry.CallIndex;");
						file.Line("atomic = entry.AtomicIndex;");
						file.Line("p      = entry.Position;");
					}
					file.Line("else");
					file.Then("atomic = entry.AtomicIndex;");
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

					writer.Line($"entries.Add(new ParserEntry(ParserEntry.Choice, {target}, p, call, atomic));");
					writer.Line($"Trace(\"push choice\", {target}, p, entries.Count);");
					writer.Line($"goto {Label(first)};");
					target = state;
				}

				return target;
			}

			case Node.Call(var rule, _):
			{
				var state = Reserve(out var writer);

				writer.Line("var callIndex = entries.Count;");
				writer.Line($"entries.Add(new ParserEntry(ParserEntry.Call, {next}, p, call, atomic));");
				writer.Line("call = callIndex;");
				writer.Line($"Trace(\"call {Escape(rule.Name)}\", {_entries[rule]}, p, entries.Count);");
				writer.Line($"goto {Label(_entries[rule])};");

				return state;
			}

			case Node.Atomic(var body):
			{
				var commit = Reserve(out var atCommit);
				var inner  = Compile(body, commit);
				var state  = Reserve(out var writer);

				writer.Line("var atomicIndex = entries.Count;");
				writer.Line("entries.Add(new ParserEntry(ParserEntry.Atomic, 0, p, call, atomic));");
				writer.Line("atomic = atomicIndex;");
				writer.Line($"Trace(\"enter atomic\", {inner}, p, entries.Count);");
				writer.Line($"goto {Label(inner)};");

				atCommit.Line("global::System.Diagnostics.Debug.Assert(atomic >= 0 && atomic < entries.Count);");
				atCommit.Line("var boundary = entries[atomic];");
				atCommit.Line("global::System.Diagnostics.Debug.Assert(boundary.Kind == ParserEntry.Atomic);");
				atCommit.Line("entries.RemoveRange(atomic, entries.Count - atomic);");
				atCommit.Line("atomic = boundary.AtomicIndex;");
				atCommit.Line($"Trace(\"commit\", {next}, p, entries.Count);");
				atCommit.Line($"goto {Label(next)};");

				return state;
			}

			default:
				throw new InvalidOperationException($"Unsupported unified-automaton node: {node.GetType().Name}.");
		}
	}

	int Reserve(out Writer writer)
	{
		writer = new Writer(0);
		_states.Add(writer);

		return _states.Count - 1 + First;
	}

	static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

	static string Label(int state) => state switch
	{
		Return => "Return",
		Accept => "Accept",
		Fail   => "Fail",
		_      => "S" + state,
	};
}
