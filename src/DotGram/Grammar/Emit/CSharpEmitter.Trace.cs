using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// What a trace build numbers: the rules and the guards its events name, and which rules only
/// forward another's value and so are framed where they were called (GramCompilerOptions.Trace).
/// </summary>
/// <remarks>
/// One a compilation, shared by its machines, so that a rule is one number in every reading of
/// the file. The grammar's rules come first in its own order; a forwarding rule nothing calls by
/// name any more, which the graph has pruned, is numbered after them where a frame first names
/// it. The tables are written once everything has been rendered, which is when they are whole.
/// </remarks>
sealed class TraceTables
{
	readonly Dictionary<RuleSymbol, int> _rules = [];
	readonly List<RuleSymbol> _named = [];
	readonly Dictionary<Node, int> _guards = new(NodeIdentity.Instance);
	readonly List<string> _guardTexts = [];
	readonly IReadOnlyDictionary<Node, IReadOnlyList<RuleSymbol>> _frames;
	readonly HashSet<RuleSymbol> _forwarding = [];

	public TraceTables(RecognitionGraph graph)
	{
		foreach (var rule in graph.Rules)
			RuleOf(rule);

		_frames = graph.Forwarded;

		foreach (var chain in _frames.Values)
			foreach (var rule in chain)
				_forwarding.Add(rule);
	}

	/// <summary>The number a rule's events carry.</summary>
	public int RuleOf(RuleSymbol rule)
	{
		if (!_rules.TryGetValue(rule, out var id))
		{
			id = _named.Count;
			_rules[rule] = id;
			_named.Add(rule);
		}

		return id;
	}

	/// <summary>The number a guard's events carry.</summary>
	public int GuardOf(Node.Guard guard)
	{
		if (!_guards.TryGetValue(guard, out var id))
		{
			id = _guardTexts.Count;
			_guards[guard] = id;
			// As the grammar writes it: `when @(…)` holds its C# in the parentheses, which are kept.
			_guardTexts.Add("@" + guard.Text);
		}

		return id;
	}

	/// <summary>
	/// The forwarding rules a node stands for, outermost first, where it is what a call to one of
	/// them was replaced by; empty otherwise.
	/// </summary>
	public IReadOnlyList<RuleSymbol> FramesOf(Node node)
	{
		return _frames.TryGetValue(node, out var chain) ? chain : [];
	}

	/// <summary>The tables, as the static fields <c>GramRead</c> reads.</summary>
	public string Render()
	{
		var text = new StringBuilder();

		text.Append("/// <summary>Every rule a trace names, by the number its events carry.</summary>").Append(Lines.Ending);
		text.Append("static readonly string[] TraceRules_DotGram = new string[]").Append(Lines.Ending);
		text.Append('{').Append(Lines.Ending);

		foreach (var rule in _named)
			text.Append('\t').Append(Quoted(rule.Declaration?.Name ?? rule.Name)).Append(',').Append(Lines.Ending);

		text.Append("};").Append(Lines.Ending).Append(Lines.Ending);

		text.Append("/// <summary>Every guard a trace names, as the grammar writes it, by the number its events carry.</summary>").Append(Lines.Ending);
		text.Append("static readonly string[] TraceGuards_DotGram = new string[]").Append(Lines.Ending);
		text.Append('{').Append(Lines.Ending);

		foreach (var guard in _guardTexts)
			text.Append('\t').Append(Quoted(guard)).Append(',').Append(Lines.Ending);

		text.Append("};").Append(Lines.Ending).Append(Lines.Ending);

		text.Append("/// <summary>Which rules only hand on another's value, and are reported where they were called.</summary>").Append(Lines.Ending);
		text.Append("static readonly bool[] TraceForwarders_DotGram = new bool[]").Append(Lines.Ending);
		text.Append('{').Append(Lines.Ending);

		foreach (var rule in _named)
			text.Append('\t').Append(_forwarding.Contains(rule) ? "true" : "false").Append(',').Append(Lines.Ending);

		text.Append("};").Append(Lines.Ending);

		return text.ToString();
	}

	static string Quoted(string text)
	{
		var quoted = new StringBuilder(text.Length + 2);

		quoted.Append('"');

		foreach (var one in text)
		{
			switch (one)
			{
				case '\\': quoted.Append("\\\\"); break;
				case '"':  quoted.Append("\\\""); break;
				case '\n': quoted.Append("\\n");  break;
				case '\r': quoted.Append("\\r");  break;
				case '\t': quoted.Append("\\t");  break;
				default:
					if (char.IsControl(one))
						quoted.Append("\\u").Append(((int)one).ToString("X4", System.Globalization.CultureInfo.InvariantCulture));
					else
						quoted.Append(one);

					break;
			}
		}

		return quoted.Append('"').ToString();
	}
}

public static partial class CSharpEmitter
{
	/// <summary>The names a trace build declares in the class beside the parser, which a rule's type may not take.</summary>
	internal static readonly string[] TraceNames = ["GramTrace", "GramRead", "GramWhy", "GramTraceLog", "GramProfile", "Tracing"];

	/// <summary>
	/// What a trace build writes beside its parser: the sink and its scope, the helpers its
	/// publications call, the tables its events are numbered by, and the ready-made sinks.
	/// </summary>
	/// <param name="quiet">Whether the failure has a <c>Quiet</c> field to say a reading records nothing.</param>
	/// <param name="more">Whether the failure has <c>ExpectedMore</c>, the sets that tied.</param>
	/// <param name="lexical">Whether a lexer can stop a call before any rule is read.</param>
	static string TraceSupport(TraceTables tables, bool quiet, bool more, bool lexical)
	{
		var text = new StringBuilder();

		text.Append(Lines.Normalize(TraceApi)
			.Replace("{{quiet}}", quiet ? "failure.Quiet" : "false")
			.Replace("{{more}}", more ? "failure.ExpectedMore" : "null")
			.Replace("{{match}}", MatchType));

		if (lexical)
			text.Append(Lines.Normalize(TraceUnlexed));

		text.Append(tables.Render()).Append(Lines.Ending);
		text.Append(Lines.Normalize(TraceSinks));

		return text.ToString();
	}

	/// <summary>
	/// A reading begun in a publication: the line that tells the sink, naming the local that holds
	/// what it was told.
	/// </summary>
	/// <param name="read">The local the reading is held in.</param>
	/// <param name="name">The rule as the grammar publishes it.</param>
	/// <param name="start">Where the reading begins, in its own unit.</param>
	/// <param name="machine">What reads it: <c>methods</c>, <c>engine</c> or <c>flat</c>.</param>
	/// <param name="text">The text it reads, as C#.</param>
	/// <param name="tokens">Over tokens, the starts, the count and where the tokens end, as C#; null over characters.</param>
	static string TraceBegin(
		string read, string name, string start, string machine, string text, string? tokens, bool finding = false)
	{
		return $"var {read} = Began_DotGram(ref failure, {Quoted(name)}, {(finding ? "true" : "false")}, {start}, " +
			$"\"{machine}\", {text}, {tokens ?? "null, 0, 0"});";
	}

	/// <summary>A reading ends: the line that tells the sink.</summary>
	static string TraceEnd(string read)
	{
		return $"Ended_DotGram(ref failure, {read}, end);";
	}

	/// <summary>
	/// A traced reading: <c>end = call</c>, guarded so that one an exception leaves still ends for
	/// the sink, and then the end told.
	/// </summary>
	/// <param name="value">The value's type, declared ahead where the call hands one out; null where it does not.</param>
	/// <param name="declare">Whether <c>end</c> is declared here rather than assigned again.</param>
	static void TracedRead(Writer file, string call, string? value, bool declare)
	{
		if (declare)
		{
			if (value is not null && call.Contains("out var recognized", StringComparison.Ordinal))
				file.Line($"{value} recognized;");

			file.Line("int end;");
			file.Line();
		}

		using (file.Block("try"))
			file.Line($"end = {call.Replace("out var recognized", "out recognized")};");

		file.Line("catch (global::System.Exception) when (Thrown_DotGram(ref failure, read))");

		using (file.Block(""))
			file.Line("throw;");

		file.Line();
		file.Line(TraceEnd("read"));
	}
}
