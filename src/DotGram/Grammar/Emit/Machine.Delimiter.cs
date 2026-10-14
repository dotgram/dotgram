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
	/// What a search for the end of <c>(?!X &amp; any)*</c> looks for (<see cref="UntilStops"/>): the
	/// characters <c>X</c> can begin with, and the longest literal it holds, short of which from the
	/// end of the input trying it says the input ran out.
	/// </summary>
	/// <param name="Through">
	/// Where the turns passed over are those of a choice (<see cref="ThroughTurns"/>): the characters
	/// a turn reads alone, as a class. <see cref="Stops"/> holds the rest where they are few enough
	/// to search for, and is empty where the class is read through instead. Only a loop no failure
	/// after it asks back passes over such turns: a turn given back would have tried the choice's
	/// other alternatives at its character, and said what they refused there.
	/// </param>
	internal sealed record UntilScan(char[] Stops, int Longest, IReadOnlyList<CharRange>? Through = null);

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
	/// It holds only where trying a refused body at such a character does nothing but fail there.
	/// So the body cannot match nothing (which matches everywhere, so the loop never turns), knows
	/// what it begins with, and begins with no half of a surrogate pair; it is recognition and
	/// nothing else (<see cref="OnlyRecognizes"/>), since code of the host a skipped turn would not
	/// run, and a recovery matches characters its first set does not hold; and nothing in it looks
	/// ahead before its first item (<see cref="LooksBeforeItsFirstItem"/>), which reads past the
	/// character and may note there that the input ran out. The one other trace a failing literal
	/// leaves is that note where it lacks room, which the search keeps by not passing the last
	/// characters a literal of <see cref="UntilScan.Longest"/> would not fit in
	/// (<see cref="EmitUntilSkip"/>). Not in a parser that traces, whose sink hears every rule a
	/// look tries. A delimiter of one character is already a run (<see cref="StopCharacters"/>),
	/// and is left to it. More than five characters is a class, and a class is read.
	/// </para>
	/// </remarks>
	internal UntilScan? UntilStops(Node.Repeat repeat)
	{
		// The shape is asked before anything is tested: a test of a class writes its table, and a
		// table written for a question that came to nothing renumbers the file's tables.
		if (OverKinds || _starves || Tracing is not null || repeat.Max != null)
			return null;

		if (repeat.Body is not Node.Sequence({ Count: >= 2 } parts) ||
			parts.Take(parts.Count - 1).Any(part => part is not Node.Lookahead(false, _)) || !ReadsAnything(parts[^1]))
			return ThroughTurns(repeat.Body);

		var made    = new List<char>();
		var longest = 1;

		for (var i = 0; i < parts.Count - 1; i++)
		{
			if (parts[i] is not Node.Lookahead(false, var refused) ||
				FirstSets.Nullable(refused, _graph) || !OnlyRecognizes(refused, ref longest) ||
				LooksBeforeItsFirstItem(refused, []) ||
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

		// Over a buffer the end is not known until it is read, and the search does not stop short
		// of it.
		if (made.Count == 0 || BufferedInput && longest > 1)
			return null;

		return new UntilScan([.. made], longest);
	}

	/// <summary>
	/// The turns of a repetition of a choice that read one character alone, where those are a
	/// class: <c>([' '..'!' | '#'..'['] | '\\' &amp; Escape)*</c>, the body of a quoted string, or
	/// <c>(?!q &amp; (e &amp; any | ?!e &amp; any))*</c>, Std's <c>Escaped</c>. Null where the body is
	/// not of that shape.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A character of the class is a turn the choice takes by one alternative that reads it and
	/// keeps nothing, every alternative before it failing on its first item, and every one after
	/// it unable to begin there: so a run of such characters is that many turns, each of which
	/// leaves nothing behind but the position. Read through as a class, inside the rule, rather
	/// than a call of the choice per character — the per-character call that was the cost of a
	/// string's text — they end where the loop would next have had a question to ask, and the
	/// turn there is read as it always was.
	/// </para>
	/// <para>
	/// What the alternatives tried first at such a character refuse is the one thing not kept: a
	/// reading that records said it at the character. Every such place is before where the
	/// passed-over turns end, and the turn there and what follows are read as before, so a
	/// refusal at or past that end replaces whatever was said before it, as the furthest does. The
	/// alternatives obey what <see cref="UntilStops"/> asks of the bodies its looks refuse:
	/// none matches nothing, each knows what it begins with, recognizes and nothing else, and
	/// looks at nothing before its first item but the looks the class is read past; and the class
	/// holds no character another alternative begins with or its looks read first (<see cref="Touches"/>),
	/// so that none tried before the one taken reads further, and none after it could match instead.
	/// </para>
	/// </remarks>
	UntilScan? ThroughTurns(Node body)
	{
		// A body that is itself a character, or a lookahead before a character, is a run or an
		// until scan of its own, read elsewhere.
		if (OneCharacter(body) is not null || body is not (Node.Choice or Node.Sequence))
			return null;

		var longest = 1;

		if (Turns(body, ref longest) is not { Count: > 0 } through ||
			through is [{ From: '\0', To: '\uFFFF' }])
			return null;

		// Only where the class is what the loop mostly reads: the other turns begin with a few
		// characters, an escape. A class of a few characters beside alternatives that begin with
		// many — `(Pchar | '/' | '?')*` with `Pchar` a rule of its own — would be tested at every
		// turn and seldom read past one.
		var others = 0;

		if (FirstSets.Of(body, _graph) is not { IsKnown: true } begins)
			return null;

		foreach (var range in Without(begins.Ranges, FirstSets.First.Chars(through)))
			if ((others += range.To - range.From + 1) > 5)
				return null;

		// The characters that end the run, where they are few: Std's `Escaped` stops at its quote
		// and its escape, which a search finds as the until scan's does.
		var stops = new List<char>();

		foreach (var range in Without([new CharRange('\0', '\uFFFF')], FirstSets.First.Chars(through)))
			for (int c = range.From; c <= range.To && stops.Count <= 5; c++)
				stops.Add((char)c);

		var searched = stops.Count <= 5 && !stops.Any(char.IsSurrogate) && !(BufferedBytes && stops.Any(one => one > 0x7F));

		// Over a buffer the end is not known until it is read, and a class is read only over a span.
		if (BufferedInput && (longest > 1 || !searched))
			return null;

		return new UntilScan(searched ? [.. stops] : [], longest, through);
	}

	/// <summary>
	/// The characters at which a turn of <paramref name="node"/> reads that one character and
	/// nothing else (<see cref="ThroughTurns"/>), or null where that cannot be told.
	/// </summary>
	/// <param name="longest">Raised to the length of the longest literal in what is tried before.</param>
	IReadOnlyList<CharRange>? Turns(Node node, ref int longest)
	{
		if (OneCharacter(node) is { } one)
			return one;

		switch (node)
		{
			case Node.Sequence([var only]):
				return Turns(only, ref longest);

			// Looks that refuse before a character: a turn the looks admit where none of what they
			// refuse can begin.
			case Node.Sequence({ Count: >= 2 } parts):
			{
				var refusedFirst = FirstSets.First.None;

				for (var i = 0; i < parts.Count - 1; i++)
				{
					if (parts[i] is not Node.Lookahead(false, var refused) || !TriedAndRefused(refused, ref longest))
						return null;

					refusedFirst = refusedFirst.Or(FirstSets.Of(refused, _graph));
				}

				return Turns(parts[^1], ref longest) is { } read ? Without(read, refusedFirst) : null;
			}

			case Node.Choice({ Count: >= 2 } alternatives) { Selection: null }:
			{
				var firsts = new FirstSets.First[alternatives.Count];

				for (var i = 0; i < alternatives.Count; i++)
				{
					if (!TriedAndRefused(alternatives[i], ref longest, looks: true))
						return null;

					firsts[i] = Touches(alternatives[i]);
				}

				var made = new List<CharRange>();

				for (var i = 0; i < alternatives.Count; i++)
				{
					var single = Turns(alternatives[i], ref longest);

					if (single is null)
						continue;

					var others = FirstSets.First.None;

					for (var j = 0; j < alternatives.Count; j++)
						if (j != i)
							others = others.Or(firsts[j]);

					made.AddRange(Without(single, others));
				}

				return FirstSets.First.Normalized(made);
			}

			default:
				return null;
		}
	}

	/// <summary>
	/// The characters trying a node reads first: what it begins with, and what the looks before its
	/// first item begin with, which they read at a character the node cannot begin with.
	/// </summary>
	FirstSets.First Touches(Node node)
	{
		switch (node)
		{
			case Node.Sequence(var parts):
			{
				var touched = FirstSets.Of(node, _graph);

				foreach (var part in parts)
				{
					if (part is not Node.Lookahead(false, var refused))
						return touched.Or(Touches(part));

					touched = touched.Or(FirstSets.Of(refused, _graph));
				}

				return touched;
			}

			case Node.Choice(var alternatives):
			{
				var touched = FirstSets.Of(node, _graph);

				foreach (var alternative in alternatives)
					touched = touched.Or(Touches(alternative));

				return touched;
			}

			default:
				return FirstSets.Of(node, _graph);
		}
	}

	/// <summary>
	/// Whether a node tried at a character it cannot begin with does nothing but fail there: it
	/// cannot match nothing, knows what it begins with, only recognizes, and — unless it is an
	/// alternative whose own looks <see cref="Turns"/> answers for — looks at nothing before its
	/// first item.
	/// </summary>
	bool TriedAndRefused(Node node, ref int longest, bool looks = false)
	{
		return !FirstSets.Nullable(node, _graph) && FirstSets.Of(node, _graph).IsKnown &&
			OnlyRecognizes(node, ref longest) &&
			(!LooksBeforeItsFirstItem(node, []) || looks && Turns(node, ref longest) is not null);
	}

	/// <summary>
	/// The characters a node reads as one, keeping nothing: a class, a literal of one character
	/// or a rule inlined as one (<see cref="RunTest"/>'s cases, asked without writing a test). A
	/// class that holds a category or a predicate of the host is not one here.
	/// </summary>
	IReadOnlyList<CharRange>? OneCharacter(Node node)
	{
		switch (node)
		{
			case Node.Element { Categories.Count: 0, References.Count: 0 } element:
				return FirstSets.OfElement(element) is { IsKnown: true } first ? first.Ranges : null;

			case Node.Literal(var value) { IgnoreCase: false } when value.Length == 1:
				return [new CharRange(value[0], value[0])];

			case Node.Call(var rule, { Count: 0 }) when CanInline(rule):
				return OneCharacter(_graph.Bodies[rule]);

			case Node.Sequence([var only]):
				return OneCharacter(only);

			case Node.Choice(var alternatives) { Selection: null }:
			{
				var made = new List<CharRange>();

				foreach (var alternative in alternatives)
					if (OneCharacter(alternative) is { } ranges)
						made.AddRange(ranges);
					else
						return null;

				return FirstSets.First.Normalized(made);
			}

			default:
				return null;
		}
	}

	/// <summary>The characters of <paramref name="ranges"/> that <paramref name="first"/> does not hold.</summary>
	static IReadOnlyList<CharRange> Without(IReadOnlyList<CharRange> ranges, FirstSets.First first)
	{
		if (first.Anything)
			return [];

		if (first.Nothing || first.Ranges.Count == 0)
			return ranges;

		var made = new List<CharRange>();

		foreach (var range in ranges)
		{
			int from = range.From;

			foreach (var taken in first.Ranges)
			{
				if (taken.To < from || taken.From > range.To)
					continue;

				if (taken.From > from)
					made.Add(new CharRange((char)from, (char)(taken.From - 1)));

				from = taken.To + 1;
			}

			if (from <= range.To)
				made.Add(new CharRange((char)from, range.To));
		}

		return made;
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
	/// external recognizer, predicate of the host in a class, computed switch, mark, construction
	/// or call that hands arguments, no rule without a body, and no repetition that recovers.
	/// </summary>
	/// <remarks>
	/// Asked of the whole of it and not only of what stands before its first item, which is
	/// <see cref="RunsCodeBeforeItsFirstItem"/>'s question: a look inside it reads past the first
	/// character and comes back, and what it runs there runs at a character the node's first set
	/// does not hold.
	/// </remarks>
	/// <param name="longest">Raised to the length of the longest literal met.</param>
	bool OnlyRecognizes(Node node, ref int longest)
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
					case Node.Element { References.Count: > 0 }:
					case Node.Behind { Test.References.Count: > 0 }:
						return false;

					case Node.Literal(var text):
						longest = Math.Max(longest, text.Length);
						break;

					case Node.Call(var rule, _) when seen.Add(rule):
						if (!_graph.Bodies.TryGetValue(rule, out var body))
							return false;

						pending.Push(body);
						break;
				}

		return true;
	}

	/// <summary>
	/// Whether a look stands before a node's first item, in it or in a rule it begins with: what
	/// reads ahead of the character a node is tried at before that character has refused it.
	/// </summary>
	bool LooksBeforeItsFirstItem(Node node, HashSet<RuleSymbol> seen)
	{
		switch (node)
		{
			case Node.Lookahead or Node.Behind:
				return true;

			case Node.Choice(var alternatives):
				return alternatives.Any(one => LooksBeforeItsFirstItem(one, seen));

			case Node.Sequence(var parts):
				foreach (var part in parts)
				{
					if (LooksBeforeItsFirstItem(part, seen))
						return true;

					if (!FirstSets.Nullable(part, _graph))
						return false;
				}

				return false;

			case Node.Call(var rule, _):
				return seen.Add(rule) && _graph.Bodies.TryGetValue(rule, out var body) && LooksBeforeItsFirstItem(body, seen);

			default:
				return node.Children.Any(one => LooksBeforeItsFirstItem(one, seen));
		}
	}

	/// <summary>
	/// Moves <c>p</c> past the turns of <c>(?!X &amp; any)*</c> that need not be read
	/// (<see cref="UntilStops"/>): to the next character in its stops, or to the end of the input —
	/// or, where <c>X</c> holds a literal of more than one character, to where that literal would
	/// no longer fit, from which on the turns are read as they were.
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
		Writer writer, UntilScan until, int? exit = null, string? turns = null, string? gave = null)
	{
		if (until.Through is not null && (exit is not null || gave is not null))
			throw new InvalidOperationException("The turns of a choice are passed over only where nothing asks them back.");

		if (until.Stops.Length == 0)
		{
			EmitThrough(writer, until, turns);

			return;
		}

		var stops = until.Stops;

		using (writer.Block($"if ({Within("p")})"))
		{
			writer.Line($"c = {ReadAt("p")};");

			using (writer.Block($"if (!({string.Join(" || ", stops.Select(stop => $"c == {CSharpEmitter.Char(stop)}"))}))"))
			{
				var end    = until.Longest > 1 ? "untilEnd" : EndOfInput;
				var before = (IDisposable?)null;

				if (until.Longest > 1)
				{
					writer.Line($"var untilEnd = {EndOfInput} - {until.Longest - 1};");
					writer.Line();
					before = writer.Block("if (p < untilEnd)");
				}

				EmitSearch(writer, "until", "p", stops, until.Longest > 1 ? "untilEnd - p" : null);

				if (exit is not null || turns is not null || gave is not null)
					writer.Line("var untilFrom = p;");

				writer.Line($"p = until < 0 ? {end} : until;");

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

				before?.Dispose();
			}
		}
	}

	/// <summary>
	/// Moves <c>p</c> past the turns <see cref="ThroughTurns"/> reads through as a class: to the
	/// first character outside it, or to the end of the input — or, where an alternative tried
	/// before holds a literal of more than one character, to where that literal would no longer
	/// fit, as <see cref="EmitUntilSkip"/> stops short.
	/// </summary>
	/// <param name="turns">A count of turns to add the passed-over ones to, or null.</param>
	void EmitThrough(Writer writer, UntilScan until, string? turns)
	{
		var test = RangesTest(until.Through!, Tabulate);

		using (writer.Block($"if ({Within("p")})"))
		{
			writer.Line($"c = {ReadAt("p")};");

			using (writer.Block($"if ({test})"))
			{
				var within = Within("p");
				var before = (IDisposable?)null;

				if (until.Longest > 1)
				{
					writer.Line($"var untilEnd = {EndOfInput} - {until.Longest - 1};");
					writer.Line();
					before = writer.Block("if (p < untilEnd)");
					within = "p < untilEnd";
				}

				if (turns is not null)
					writer.Line("var untilFrom = p;");

				writer.Line("p++;");

				using (writer.Block($"while ({within})"))
				{
					writer.Line($"c = {ReadAt("p")};");
					writer.Line($"if (!({test})) break;");
					writer.Line("p++;");
				}

				if (turns is not null)
					writer.Line($"{turns} += p - untilFrom;");

				before?.Dispose();
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
