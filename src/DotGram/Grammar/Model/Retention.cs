using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>How much of a line a construct can take.</summary>
public enum LineExtent
{
	/// <summary>No path through it consumes a line terminator.</summary>
	None,

	/// <summary>It may consume one, and nothing follows. A line, terminator included.</summary>
	AtEnd,

	/// <summary>It may consume one and go on, so what it takes is not one line but many.</summary>
	Beyond,
}

/// <summary>
/// How much input a grammar can be made to hold, and therefore whether it can stream.
/// </summary>
/// <remarks>
/// <para>
/// docs/syntax.md §6.3 emits the streaming overloads only where the grammar provably works
/// with a reused buffer. Ordinary rule calls are transparent to backtracking, so naming
/// a subexpression does not shorten its retention. What must be held is determined by
/// the active streamed stage and its explicit recovery/continuation boundary.
/// </para>
/// <para>
/// A line-oriented reader gives that a unit, and the question becomes what a rule can take
/// measured in lines. Three answers are enough, and the difference that matters is not
/// whether a terminator is consumed but whether anything follows it: <c>None</c> is a
/// field, <c>AtEnd</c> is a whole record, and both fit a buffer of one line. Only
/// <c>Beyond</c> does not.
/// </para>
/// </remarks>
public static class Retention
{
	/// <summary>
	/// What each rule can take, in lines.
	/// </summary>
	/// <remarks>
	/// A fixpoint, like nullability, and for the same reason: a rule's answer depends on
	/// the rules it calls, and recursion means the dependency is not a tree. Starting at
	/// the smallest answer and growing is what makes a cycle settle — a rule that reaches a
	/// terminator only through itself reaches one never.
	/// </remarks>
	public static Dictionary<RuleSymbol, LineExtent> ExtentOf(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var consuming = Consuming(graph);
		var extents   = new Dictionary<RuleSymbol, LineExtent>();

		foreach (var rule in graph.Rules)
			extents[rule] = LineExtent.None;

		for (var changed = true; changed; )
		{
			changed = false;

			foreach (var rule in graph.Rules)
			{
				var extent = Extent(graph.Bodies[rule], extents, consuming);

				if (extent > extents[rule])
				{
					extents[rule] = extent;
					changed       = true;
				}
			}
		}

		return extents;
	}

	/// <summary>One part of a published rule, and what it costs to hold.</summary>
	/// <param name="Committed">
	/// A repetition marked <c>recover</c>: the window may move at each element, because
	/// the synchronization point is one the parse cannot return past (§8.2).
	/// </param>
	public sealed record Stage(Node Body, LineExtent Extent, bool Committed);

	/// <summary>
	/// How a published rule breaks into stages, and whether every one of them fits.
	/// </summary>
	/// <param name="Reason">Why it cannot stream, or null when it can.</param>
	public sealed record Plan(IReadOnlyList<Stage> Stages, string? Reason)
	{
		public bool CanStream => Reason is null;
	}

	/// <summary>
	/// Whether a published rule can be read from a window, and where the window moves.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The shape a streamable grammar has is a sequence of stages, each of which either
	/// fits the window or is a committed repetition of things that do. A feed is the
	/// ordinary case — a header, a run of records, a trailer — and what makes it streamable
	/// is not its length but that the run in the middle commits, so what came before it can
	/// be let go.
	/// </para>
	/// <para>
	/// Written as a decomposition rather than as "find the recovering repetition" because
	/// that generalizes: two committed runs in one rule are a perfectly ordinary feed, and
	/// a stage may itself be a rule with stages of its own. Neither is built — one
	/// <c>recover</c> per rule is still refused — but the shape does not have to change
	/// when they are.
	/// </para>
	/// </remarks>
	public static Plan PlanFor(RecognitionGraph graph, RuleSymbol rule)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		if (rule is null)
			throw new ArgumentNullException(nameof(rule));

		var extents   = ExtentOf(graph);
		var consuming = Consuming(graph);
		var parts     = graph.PartsOf(rule);
		var stages    = new List<Stage>(parts.Count);

		foreach (var part in parts)
		{
			var committed = part is Node.Repeat && graph.Recoveries.ContainsKey(part);

			// A committed run is measured by one element, not by the run: the window moves
			// between elements, so the run's own length is what streaming is *for*.
			var measured = committed && part is Node.Repeat(var element, _, _)
				? Extent(element, extents, consuming)
				: Extent(part, extents, consuming);

			stages.Add(new Stage(part, measured, committed));
		}

		foreach (var stage in stages)
			if (stage.Extent == LineExtent.Beyond)
				return new Plan(
					stages,
					$"'{rule.Name}' has no streaming overload — {Describe(stage)} may take more " +
					"than one line, so retention would grow with the input.");

		// Every stage fits, and still nothing may be let go unless one of them says so.
		foreach (var stage in stages)
			if (stage.Committed)
				return new Plan(stages, null);

		return new Plan(
			stages,
			$"'{rule.Name}' has no streaming overload — every part of it fits a line, but " +
			"none of them commits, so the window could never move. Mark the repetition that " +
			"reads the body of the input with 'recover' (docs/syntax.md §8.2).");
	}

	/// <summary>
	/// A publication that would have got a reader overload if the grammar let it.
	/// </summary>
	public const string NotStreamable = "GRAM5001";

	/// <summary>
	/// Why nothing in this grammar may be read through a window, or null.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A C# recognizer is handed a span and told nothing about where it came from, so it
	/// cannot tell the end of a window from the end of the input — and cannot say which it
	/// hit, the way a literal does. Over a window it would read a record cut in half as a
	/// record that ended, which is the one failure streaming must not have.
	/// </para>
	/// <para>
	/// Said of the grammar rather than of the rule, because a recognizer anywhere in it is
	/// reachable from anything published.
	/// </para>
	/// </remarks>
	public static string? Reads(RecognitionGraph graph, RuleSymbol root)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		if (root is null)
			throw new ArgumentNullException(nameof(root));

		// A span says where, and where is into the text it was measured against. A streamed
		// parse holds a window that moves, so an offset handed out of one is a reference to
		// something that will not be there — and it would look like an offset either way,
		// which is the failure this refuses rather than risks.
		foreach (var owner in Reachable(graph, root))
			if (graph.Types.TryGetValue(owner, out var type) && type == "SourceSpan")
				return $"'{owner.Name}' has a 'SourceSpan' for its value (docs/syntax.md §4.1), " +
					"which says where in the input it matched. A streamed parse reads through a " +
					"window that moves on, so the place a span points at is gone by the time " +
					"anyone could look at it.";

		// The same fact reached by another road. A rule need not have a span for its value to
		// hand one out: a construction can ask for `parserSpan` and put it inside a type of
		// its own, and what comes out is still an offset into a window that will have moved.
		foreach (var owner in Reachable(graph, root))
			foreach (var node in Everything(graph.Bodies[owner]))
				if (node is Node.Construct { How: Construction.Expression { Text: var built } } &&
					built.Contains("parserSpan"))
				{
					return $"'{owner.Name}' builds its value with 'parserSpan' (docs/syntax.md §8.2), " +
						"which says where in the input it matched. A streamed parse reads through a " +
						"window that moves on, so the place a span points at is gone by the time " +
						"anyone could look at it.";
				}

		foreach (var owner in Reachable(graph, root))
			foreach (var node in Everything(graph.Bodies[owner]))
				if (node is Node.External(var method))
					return $"'{owner.Name}' reads through '@{method}', a C# recognizer over a span " +
						"(docs/syntax.md §7.1). It is handed whatever is held and cannot be told that " +
						"more is coming, so it would read the edge of the window as the end of the " +
						"input.";

		return null;
	}

	/// <summary>
	/// The rules a publication can reach, and only those.
	/// </summary>
	/// <remarks>
	/// What stops a stream is what the stream would run into. A grammar may hold a rule that
	/// reads through C# or hands back a span and never publish it anywhere a window is used —
	/// and asking about every rule in the file refused such a grammar for something it does
	/// not do.
	/// </remarks>
	static IEnumerable<RuleSymbol> Reachable(RecognitionGraph graph, RuleSymbol root)
	{
		var seen    = new HashSet<RuleSymbol>();
		var pending = new Stack<RuleSymbol>();

		pending.Push(root);

		while (pending.Count > 0)
		{
			var rule = pending.Pop();

			if (!seen.Add(rule) || !graph.Bodies.TryGetValue(rule, out var body))
				continue;

			yield return rule;

			foreach (var node in Everything(body))
				if (node is Node.Call(var called, _))
					pending.Push(called);
		}
	}

	static IEnumerable<Node> Everything(Node node)
	{
		yield return node;

		foreach (var child in Children(node))
			foreach (var inside in Everything(child))
				yield return inside;
	}

	/// <summary>
	/// Whether this rule is asking to be streamed at all.
	/// </summary>
	/// <remarks>
	/// Most grammars are not feeds and want nothing to do with a reader, and telling every
	/// one of them what it did not get would be noise on every build. A result that comes
	/// in parts is what says the author had a stream in mind — it is the only shape that
	/// can be handed over one element at a time, so declaring it is asking for the
	/// possibility. <c>recover</c> alone is not: plenty of grammars recover over a string
	/// and never want a reader.
	/// </remarks>
	static bool Streamable(RecognitionGraph graph, RuleSymbol rule) =>
		graph.Types.TryGetValue(rule, out var type) && type.EndsWith("[]", StringComparison.Ordinal);

	/// <summary>
	/// Why a <c>parse</c> cannot be read from a reader, or null when it can.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Three things have to hold, and the second is the one worth explaining. A streamed
	/// parse hands each element over as it is read, and handing it over is a commitment:
	/// the caller has it, and no later failure can take it back. Over a string the parse
	/// may still change its mind, because a repetition that took too much gives it back.
	/// </para>
	/// <para>
	/// <c>recover</c> is what removes the difference, and by construction rather than by
	/// promise: §8.2 makes a marked repetition possessive, so an element it took was
	/// either read or explicitly rejected and there is no shorter reading to come back
	/// for. That is the same property, said about the repetition instead of about the
	/// stream.
	/// </para>
	/// <para>
	/// <b>It is a conservative test, not the real one.</b> What actually has to hold is
	/// that no element handed over would ever have been given back — which for an
	/// unambiguous grammar is true whether or not anything is marked. A grammar where the
	/// question even arises, one whose trailer also reads as a record, is ambiguous and
	/// wants a diagnostic of its own rather than a stricter rule here. Until there is one,
	/// the mark is the thing this can check.
	/// </para>
	/// </remarks>
	/// <summary>Whether this part is a repetition the driver could hand elements over from.</summary>
	static bool Yields(Node part) => part is
		Node.Repeat(Node.Capture(_, Node.Call), _, _) or
		Node.Repeat(Node.Call, _, _);

	public static string? StreamedParse(RecognitionGraph graph, RuleSymbol rule)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		if (rule is null)
			throw new ArgumentNullException(nameof(rule));

		if (!graph.Types.TryGetValue(rule, out var type) || !type.EndsWith("[]", StringComparison.Ordinal))
			return $"'{rule.Name}' does not build a sequence, and a stream can only hand over " +
				"what comes in parts. Declare its result as ': @T[]' (docs/syntax.md §4.1 case 2).";

		var body = graph.Bodies[rule];

		if (body is Node.Construct(var built, _))
			body = built;

		if (body is Node.Choice)
			return $"'{rule.Name}' is a choice of alternatives, and which one is being read is " +
				"not settled until it has been. A streamed parse reads its parts in order.";

		if (Reads(graph, rule) is { } reader)
			return reader;

		var parts = graph.PartsOf(rule);

		// What the driver hands over is the elements of a repetition of a rule, because a
		// rule is what has a recognizer of its own to call one element at a time. A
		// repetition of something else — a choice, a group — is read whole by one machine,
		// and there is nothing to yield between iterations. Said here rather than left to
		// the emitter, which would otherwise write an iterator with no `yield` in it and
		// leave the consumer's compiler to complain about a file they did not write.
		if (!parts.Any(Yields))
			return $"'{rule.Name}' has no repetition of a rule to hand over, so a streamed parse " +
				"would have nothing to give the caller between reads. Give the repeated part its " +
				"own rule (docs/syntax.md §6.3).";

		for (var i = 0; i < parts.Count; i++)
		{
			// Marked, so possessive: an element it took was either read or explicitly
			// rejected, and there is no shorter reading to come back for (§8.2).
			if (graph.Recoveries.ContainsKey(parts[i]))
				continue;

			if (!FirstSets.Undecided(parts, i, graph))
				continue;

			return $"in '{rule.Name}', the repetition '{parts[i]}' can begin with the same input as " +
				"what follows it, so where it ends is settled by backtracking. A streamed parse " +
				"hands each element to the caller as it reads it and cannot take one back. Make the " +
				"two tellable apart, or mark the repetition 'recover', which commits it " +
				"(docs/syntax.md §8.2).";
		}

		var extents   = ExtentOf(graph);
		var consuming = Consuming(graph);

		// Measured a part at a time, and a repetition by one of its elements: the window
		// moves between them, so the run's own length is what streaming is *for*. What has
		// to fit is the largest thing the parse is ever in the middle of.
		foreach (var part in parts)
		{
			var measured = part is Node.Repeat(var element, _, _)
				? Extent(element, extents, consuming)
				: Extent(part, extents, consuming);

			if (measured == LineExtent.Beyond)
				return $"in '{rule.Name}', '{part}' may take more than one line, so what would have " +
					"to be held grows with the input.";
		}

		return null;
	}

	/// <summary>
	/// What each publication did not get, and why.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Said rather than left to be discovered, because the alternative is what the author
	/// actually meets: a call that does not bind, `cannot convert from TextReader to
	/// string`, naming neither the rule responsible nor anything they could do about it.
	/// The overload is missing on purpose and the purpose is worth a sentence.
	/// </para>
	/// <para>
	/// Only where the grammar is the reason. A <c>parse</c> has no reader overload today
	/// because the windowed driver for it is not built, which is a fact about this
	/// compiler rather than about the grammar in front of it — reporting that on every
	/// build of every grammar would be noise, and it belongs in docs/status.md where it
	/// is.
	/// </para>
	/// </remarks>
	public static IReadOnlyList<GramDiagnostic> Check(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var reported  = new List<GramDiagnostic>();
		var extents   = ExtentOf(graph);
		var consuming = Consuming(graph);

		foreach (var publication in graph.Publications)
		{
			string? why;

			if (publication.Kind == PublishKind.Find)
			{
				if (!extents.TryGetValue(publication.Rule, out var extent) || extent != LineExtent.Beyond)
					continue;

				why =
					$"{Culprit(graph.Bodies[publication.Rule], extents, consuming)} may take more than " +
					$"one line, so reading '{publication.Rule.Name}' from one would have to hold the " +
					"input rather than one occurrence.";
			}
			else if (Streamable(graph, publication.Rule) &&
				StreamedParse(graph, publication.Rule) is { } refused)
			{
				why = refused;
			}
			else
			{
				continue;
			}

			reported.Add(new GramDiagnostic(
				NotStreamable,
				$"'{publication.MethodName}' gets no overload taking a reader: {why} " +
				"docs/syntax.md §6.3 says which rules get one, and why.",
				publication.At.Position,
				publication.At.Length,
				GramSeverity.Info));
		}

		return reported;
	}

	/// <summary>
	/// The smallest part of a rule that takes more than a line.
	/// </summary>
	/// <remarks>
	/// The innermost one, so that a message names <c>any*</c> rather than the whole body
	/// it is buried in. A node all of whose children fit is itself the answer, because
	/// then what does not fit is the composition and there is nothing smaller to point at.
	/// </remarks>
	static string Culprit(
		Node                                        node,
		IReadOnlyDictionary<RuleSymbol, LineExtent> rules,
		ICollection<RuleSymbol>                     consuming)
	{
		foreach (var child in Children(node))
			if (Extent(child, rules, consuming) == LineExtent.Beyond)
				return Culprit(child, rules, consuming);

		return "'" + node + "'";
	}

	static IEnumerable<Node> Children(Node node)
	{
		switch (node)
		{
			case Node.Sequence(var parts):        return parts;
			case Node.Choice(var alternatives):   return alternatives;
			case Node.Repeat(var body, _, _):     return [body];
			case Node.Capture(_, var captured):   return [captured];
			case Node.Construct(var built, _):    return [built];
			case Node.Atomic(var body):            return [body];
			default:                              return [];
		}
	}

	static string Describe(Stage stage) =>
		stage.Committed ? "an element of " + stage.Body : stage.Body.ToString();

	/// <summary>
	/// Every rule that can take at least one input item.
	/// </summary>
	/// <remarks>
	/// Its own fixpoint, and needed before the extents because a call is what tells a
	/// terminator at the end of a rule from one in the middle of it. Guessing that a call
	/// consumes would make <c>eol &amp; eof</c> two lines, which it plainly is not.
	/// </remarks>
	static HashSet<RuleSymbol> Consuming(RecognitionGraph graph)
	{
		var consuming = new HashSet<RuleSymbol>();

		for (var changed = true; changed; )
		{
			changed = false;

			foreach (var rule in graph.Rules)
				if (!consuming.Contains(rule) && Consumes(graph.Bodies[rule], consuming))
				{
					consuming.Add(rule);
					changed = true;
				}
		}

		return consuming;
	}

	/// <summary>What one node can take, given what is known of the rules so far.</summary>
	public static LineExtent Extent(
		Node                                        node,
		IReadOnlyDictionary<RuleSymbol, LineExtent> rules,
		ICollection<RuleSymbol>                     consuming)
	{
		switch (node)
		{
			case Node.Literal(var text):
				return text.IndexOf('\n') < 0 && text.IndexOf('\r') < 0
					? LineExtent.None
					: EndsWithTerminator(text) ? LineExtent.AtEnd : LineExtent.Beyond;

			case Node.Element element:
				return Admits(element) ? LineExtent.AtEnd : LineExtent.None;

			// Consumes nothing, so it takes no part of a line. What a lookahead needs to
			// *see* is a window question rather than a retention one, and §6.3 does not
			// answer it.
			case Node.Lookahead:
			case Node.Guard:
			case Node.Empty:
				return LineExtent.None;

			case Node.Call(var called, _):
				return rules.TryGetValue(called, out var known) ? known : LineExtent.None;

			case Node.Capture(_, var captured): return Extent(captured, rules, consuming);
			case Node.Construct(var built, _):  return Extent(built, rules, consuming);
			case Node.Atomic(var body):          return Extent(body, rules, consuming);

			case Node.Choice(var alternatives):
			{
				var worst = LineExtent.None;

				foreach (var alternative in alternatives)
				{
					var extent = Extent(alternative, rules, consuming);

					if (extent > worst)
						worst = extent;
				}

				return worst;
			}

			// Once round is a line at most; twice round is that line and the next one.
			case Node.Repeat(var body, _, var max):
			{
				if (max == 0)
					return LineExtent.None;

				var once = Extent(body, rules, consuming);

				return once == LineExtent.AtEnd && max != 1 ? LineExtent.Beyond : once;
			}

			case Node.Sequence(var parts):
			{
				var soFar = LineExtent.None;

				foreach (var part in parts)
				{
					// A terminator with something after it is the whole of what `Beyond`
					// means: the parse is on the next line and still holding this one.
					if (soFar == LineExtent.AtEnd && Consumes(part, consuming))
						return LineExtent.Beyond;

					var extent = Extent(part, rules, consuming);

					if (extent > soFar)
						soFar = extent;

					if (soFar == LineExtent.Beyond)
						return soFar;
				}

				return soFar;
			}

			default:
				return LineExtent.None;
		}
	}

	/// <summary>
	/// Whether a node can take at least one input item.
	/// </summary>
	/// <remarks>
	/// What tells a terminator at the end from one in the middle. Answered generously —
	/// anything that might consume counts — because the cost of saying yes is a grammar
	/// that loses an overload it could have had, and of saying no is one that loses data.
	/// </remarks>
	static bool Consumes(Node node, ICollection<RuleSymbol> consuming) => node switch
	{
		Node.Empty or Node.Guard or Node.Lookahead => false,
		Node.Literal(var text)                     => text.Length > 0,
		Node.Repeat(var body, _, var max)          => max != 0 && Consumes(body, consuming),
		Node.Atomic(var body)                      => Consumes(body, consuming),
		Node.Capture(_, var captured)              => Consumes(captured, consuming),
		Node.Construct(var built, _)               => Consumes(built, consuming),
		Node.Sequence(var parts)                   => Any(parts, consuming),
		Node.Choice(var alternatives)              => Any(alternatives, consuming),
		Node.Call(var called, _)                   => consuming.Contains(called),
		_                                          => true,
	};

	static bool Any(IReadOnlyList<Node> nodes, ICollection<RuleSymbol> consuming)
	{
		foreach (var node in nodes)
			if (Consumes(node, consuming))
				return true;

		return false;
	}

	/// <summary>Whether the last item of a literal is a terminator.</summary>
	static bool EndsWithTerminator(string text) =>
		text.Length > 0 && text[text.Length - 1] is '\n' or '\r';

	/// <summary>
	/// Whether an element set admits a line terminator.
	/// </summary>
	/// <remarks>
	/// A Unicode category is not looked into: <c>\p{Cc}</c> contains both terminators and
	/// several others could be argued about. Treated as admitting one, which is again the
	/// direction that costs an overload rather than data.
	/// </remarks>
	static bool Admits(Node.Element element)
	{
		if (element.Categories.Count > 0)
			return true;

		var named = false;

		foreach (var range in element.Ranges)
			if ((range.From <= '\n' && '\n' <= range.To) || (range.From <= '\r' && '\r' <= range.To))
			{
				named = true;

				break;
			}

		return element.IsNegated ? !named : named;
	}
}
