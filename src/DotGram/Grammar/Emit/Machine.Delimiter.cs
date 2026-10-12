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
			turn[0] is not Node.Lookahead(false, var guard) || RunTest(turn[1]) != "true")
			return null;

		return PaddedDelimiter(guard);
	}

	(string Padding, string Stop)? PaddedDelimiter(Node guard)
	{
		if (OverKinds || _starves || DelimiterBody(guard) is not Node.Sequence(var parts) || parts.Count is < 2 or > 3 ||
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

		writer.Line("var runStart = p;");
		EmitDelimiterScan(writer, padding, stop);
		// Re-enter the original path only on a minimum-length failure, so its
		// exact diagnostic and failure position remain unchanged.
		var retry = repeat.Min > 0 ? CompileRepeat(repeat, next, following) : (int?)null;
		// A run whose continuation reads padding first and cannot begin inside it keeps no entry
		// to give back from (Determinism.SettlesPaddedRun); a give-back there only failed, after
		// reading the padding again.
		FinishScan(writer, repeat, next, retry, settled: Determinism.SettlesPaddedRun(repeat, following, _graph));

		return state;
	}

	void EmitDelimiterScan(Writer writer, string? padding, string stop)
	{
		_usesChar = true;
		writer.Line("// Linear delimiter scan: remember the start of trailing padding.");
		if (padding != null)
			writer.Line("var paddingStart = p;");

		using (writer.Block($"while ({Room(1)})"))
		{
			writer.Line($"c = {ReadAt("p")};");
			using (writer.Block($"if ({stop})"))
			{
				if (padding != null)
					writer.Line("p = paddingStart;");
				writer.Line("break;");
			}
			writer.Line("p++;");
			if (padding != null)
				writer.Line($"if (!({padding})) paddingStart = p;");
		}
	}

	/// <summary>
	/// The characters a turn of <c>(?!X &amp; any)*</c> can stop at, where they are few: the
	/// characters <c>X</c> can begin with, so that the run between two of them is turns that
	/// need not be read one at a time. Null where the repetition is not of that shape or the set
	/// is not small.
	/// </summary>
	/// <remarks>
	/// <para>
	/// At a character none of the refused bodies can begin with, every one of them fails on its
	/// first item, the look admits the turn, and <c>any</c> reads the character: the turn is
	/// taken, and nothing is said, since what a look refuses is never recorded. So a search for
	/// the next character that can begin one of them — an <c>IndexOfAny</c>, which the runtime
	/// vectorizes — lands where the loop would next have had a question to ask, and the turn is
	/// read there as it always was. That is the whole of the change: where the scan ends, what it
	/// says there and what a failure after it may ask back are the loop's own.
	/// </para>
	/// <para>
	/// It holds only where a refused body cannot match nothing (which matches everywhere, so the
	/// loop never turns), knows what it begins with, and begins with no half of a surrogate pair;
	/// and where trying it is recognition and nothing else (<see cref="OnlyRecognizes"/>): code of
	/// the host a skipped turn would not run, or a recovery, which matches characters its first set
	/// does not hold, and a look inside it may run either past the first character. A delimiter of
	/// one character is already a run (<see cref="StopCharacters"/>), and is left to it. More than
	/// five characters is a class, and a class is read.
	/// </para>
	/// </remarks>
	internal char[]? UntilStops(Node.Repeat repeat)
	{
		// The shape is asked before anything is tested: a test of a class writes its table, and a
		// table written for a question that came to nothing renumbers the file's tables.
		if (OverKinds || _starves || repeat.Max != null ||
			repeat.Body is not Node.Sequence({ Count: >= 2 } parts) ||
			parts.Take(parts.Count - 1).Any(part => part is not Node.Lookahead(false, _)) || !ReadsAnything(parts[^1]))
			return null;

		var made = new List<char>();

		for (var i = 0; i < parts.Count - 1; i++)
		{
			if (parts[i] is not Node.Lookahead(false, var refused) ||
				FirstSets.Nullable(refused, _graph) || !OnlyRecognizes(refused) ||
				FirstSets.Of(refused, _graph) is not { IsKnown: true } first)
				return null;

			foreach (var range in first.Ranges)
				for (int c = range.From; c <= range.To; c++)
				{
					if (made.Count == 5 || char.IsSurrogate((char)c) || BufferedBytes && c > 0x7F)
						return null;

					if (!made.Contains((char)c))
						made.Add((char)c);
				}
		}

		return made.Count == 0 ? null : [.. made];
	}

	/// <summary>
	/// Whether a node is <c>any</c>: one character, whichever it is. Asked of the node and of the
	/// rule it names where that rule is inlined, as a run's test is (<see cref="RunTest"/>), but
	/// without writing a test.
	/// </summary>
	bool ReadsAnything(Node node)
	{
		return node switch
		{
			Node.Element element =>
				FirstSets.OfElement(element) is { IsKnown: true, Ranges: [{ From: '\0', To: '\uFFFF' }] },
			Node.Call(var rule, _) when CanInline(rule) => ReadsAnything(_graph.Bodies[rule]),
			Node.Sequence([var only]) => ReadsAnything(only),
			_ => false,
		};
	}

	/// <summary>
	/// Whether reading a node, and every rule it reaches, only recognizes: no guard, condition,
	/// external recognizer, computed switch, mark, construction or call that hands arguments, no
	/// rule without a body, and no repetition that recovers.
	/// </summary>
	/// <remarks>
	/// Asked of the whole of it and not only of what stands before its first item, which is
	/// <see cref="RunsCodeBeforeItsFirstItem"/>'s question: a look inside it reads past the first
	/// character and comes back, and what it runs there runs at a character the node's first set
	/// does not hold.
	/// </remarks>
	bool OnlyRecognizes(Node node)
	{
		if (HoldsRecovery(node))
			return false;

		var seen    = new HashSet<RuleSymbol>();
		var pending = new Stack<Node>();

		pending.Push(node);

		while (pending.Count > 0)
			foreach (var one in NodeWalk.Descendants(pending.Pop()))
				switch (one)
				{
					case Node.Guard or Node.Condition or Node.External or Node.Marked or Node.Construct:
					case Node.Choice { Selection: not null }:
					case Node.Call(_, { Count: > 0 }):
						return false;

					case Node.Call(var rule, _) when seen.Add(rule):
						if (!_graph.Bodies.TryGetValue(rule, out var body))
							return false;

						pending.Push(body);
						break;
				}

		return true;
	}

	/// <summary>
	/// Moves <c>p</c> past the turns of <c>(?!X &amp; any)*</c> that need not be read
	/// (<see cref="UntilStops"/>): to the next character in <paramref name="stops"/>, or to the
	/// end of the input.
	/// </summary>
	/// <remarks>
	/// Where the repetition can be asked for a shorter reading, the turns passed over are kept as
	/// one thing to give back from, a character at a time, as their own exits would have been: in
	/// the engine a <c>Run</c> entry resuming at <paramref name="exit"/>; on the reader's tape a way
	/// whose alternatives are how many characters to give back, named <paramref name="gave"/>,
	/// which ends the loop where it gives any (as Reader.EmitRun's way does for a run).
	/// </remarks>
	/// <param name="turns">A count of turns to add the passed-over ones to, or null.</param>
	internal void EmitUntilSkip(
		Writer writer, IReadOnlyList<char> stops, int? exit = null, string? turns = null, string? gave = null)
	{
		using (writer.Block($"if ({Within("p")})"))
		{
			writer.Line($"c = {ReadAt("p")};");

			using (writer.Block($"if (!({string.Join(" || ", stops.Select(stop => $"c == {CSharpEmitter.Char(stop)}"))}))"))
			{
				EmitSearch(writer, "until", "p", stops);

				if (exit is not null || turns is not null || gave is not null)
					writer.Line("var untilFrom = p;");

				writer.Line($"p = until < 0 ? {EndOfInput} : until;");

				if (turns is not null)
					writer.Line($"{turns} += p - untilFrom;");

				if (exit is { } resumes)
				{
					_usesRuns = true;
					writer.Line(
						$"entries.Add(new ParserEntry(ParserEntry.Run, {Resuming(writer, resumes)}, untilFrom, " +
						"call, atomic, repeat, lookahead, p));");
				}

				if (gave is not null)
				{
					writer.Line($"var {gave} = 0;");
					writer.Line();

					using (writer.Block("if (ways.Cursor < ways.Count)"))
					{
						writer.Line($"{gave} = ways.Items[ways.Cursor * 2];");
						writer.Line("ways.Cursor++;");
					}

					writer.Line("else");

					using (writer.Block(""))
						writer.Line("ways.Open(p - untilFrom);");

					writer.Line();

					using (writer.Block($"if ({gave} > 0)"))
					{
						writer.Line($"p -= {gave};");
						writer.Line("break;");
					}
				}
			}
		}
	}

	void EmitRecoverySearch(Writer writer, Node sync)
	{
		if (OverKinds || _starves)
			return;

		var body      = DelimiterBody(sync);
		var delimiter = PaddedDelimiter(body);
		var stop      = delimiter?.Stop ?? (body is Node.Element or Node.Literal ? RunTest(body) : null);
		if (stop == null || stop == "true")
			return;

		writer.Line("var searchStart = p;");
		EmitDelimiterScan(writer, delimiter?.Padding, stop);
		// Replay the last rejected candidate before trying the delimiter (or EOF).
		// Its ordinary failure preserves the furthest diagnostic and expected tokens.
		writer.Line("if (p > searchStart) p--;");
	}
}
