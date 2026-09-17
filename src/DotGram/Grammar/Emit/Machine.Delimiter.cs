using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	string? GuardedCharacterTest(Node body)
	{
		if (OverKinds || _starves || body is not Node.Sequence({ Count: 2 } parts) ||
			parts[0] is not Node.Lookahead(false, var delimiter) || RunTest(parts[1]) != "true" ||
			RunTest(delimiter) is not { } stop || stop == "true")
			return null;

		return $"!({stop})";
	}

	// Recognize only pure character tests after publication specialization. Calls with
	// arguments, captures, guards, constructions and recursive aliases keep the machine.
	Node DelimiterBody(Node node)
	{
		var seen = new HashSet<RuleSymbol>();
		while (node is Node.Call(var rule, { Count: 0 }) && seen.Add(rule) &&
			_graph.Bodies.TryGetValue(rule, out var body) &&
			!NodeWalk.Descendants(body).Any(_graph.Recoveries.ContainsKey))
			node = body;

		return node;
	}

	(string Padding, string Stop)? PaddedDelimiter(Node.Repeat repeat)
	{
		if (OverKinds || _starves || repeat.Max != null ||
			repeat.Body is not Node.Sequence({ Count: 2 } turn) ||
			turn[0] is not Node.Lookahead(false, var guard) || RunTest(turn[1]) != "true" ||
			DelimiterBody(guard) is not Node.Sequence(var parts) || parts.Count is < 2 or > 3 ||
			DelimiterBody(parts[0]) is not Node.Repeat(var padding, 0, null))
			return null;

		padding = DelimiterBody(padding);
		var stop = DelimiterBody(parts[1]);
		if (padding is not (Node.Element or Node.Literal) ||
			stop is not (Node.Element or Node.Literal) ||
			RunTest(padding) is not { } paddingTest || RunTest(stop) is not { } stopTest ||
			FirstSets.Of(padding, _graph).Overlaps(FirstSets.Of(stop, _graph)))
			return null;

		// A pure optional suffix cannot change whether the separator matches.
		if (parts.Count == 3 &&
			(DelimiterBody(parts[2]) is not Node.Repeat(var suffix, 0, null) ||
			 DelimiterBody(suffix) is not (Node.Element or Node.Literal) ||
			 RunTest(DelimiterBody(suffix)) is null))
			return null;

		return (paddingTest, stopTest);
	}

	int CompilePaddedScan(Node.Repeat repeat, string padding, string stop, int next,
		FollowSets.Continuation following)
	{
		var state = Reserve(out var writer);
		_usesRuns = true;
		_usesChar = true;

		writer.Line("// Linear delimiter scan: remember the start of trailing padding.");
		writer.Line("var runStart = p;");
		writer.Line("var paddingStart = p;");
		using (writer.Block($"while ({Room(1)})"))
		{
			writer.Line($"c = {ReadAt("p")};");
			using (writer.Block($"if ({stop})"))
			{
				writer.Line("p = paddingStart;");
				writer.Line("break;");
			}
			writer.Line("p++;");
			writer.Line($"if (!({padding})) paddingStart = p;");
		}
		// Re-enter the original path only on a minimum-length failure, so its
		// exact diagnostic and failure position remain unchanged.
		var retry = repeat.Min > 0 ? CompileRepeat(repeat, next, following) : (int?)null;
		FinishScan(writer, repeat, next, retry);

		return state;
	}
}
