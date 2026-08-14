using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// How much input a grammar can be made to hold, and therefore whether it can stream.
/// </summary>
/// <remarks>
/// <para>
/// docs/syntax.md §6.3 emits the streaming overloads only where the grammar provably
/// works with a reused buffer, and the rule it rests on is frozen in §4: backtracking does
/// not cross a rule boundary, so a call reaches back not at all, and a rule reaches back
/// exactly as far as it has consumed. What must be held is therefore the extent of the
/// outermost rule still in progress.
/// </para>
/// <para>
/// A line-oriented reader gives that a unit. If nothing the parse is in the middle of can
/// span more than one line, a line is all that need be held; if something can, the buffer
/// is whatever that thing turns out to be, and there is no bound to promise. So the
/// question this answers, per rule, is <b>can it consume a line terminator</b> — and the
/// rule that may is the only kind that costs anything.
/// </para>
/// <para>
/// This is one half of the analysis. It says what a rule can span; deciding that a
/// <i>published</i> rule streams also needs the commit points that let the window move —
/// which today means <c>recover</c>, whose synchronization expression is a position the
/// parse cannot return past (§8.2). That part is not built, and until it is, nothing is
/// emitted from any of this.
/// </para>
/// </remarks>
public static class Retention
{
	/// <summary>
	/// Every rule that can consume a line terminator, directly or through what it calls.
	/// </summary>
	/// <remarks>
	/// A fixpoint, like nullability, and for the same reason: a rule's answer depends on
	/// the rules it calls, and recursion means the dependency is not a tree. Starting at
	/// "no" and growing is what makes a cycle settle — a rule that only reaches a newline
	/// through itself does not reach one at all.
	/// </remarks>
	public static HashSet<RuleSymbol> RulesThatSpanLines(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var spanning = new HashSet<RuleSymbol>();

		for (var changed = true; changed; )
		{
			changed = false;

			foreach (var rule in graph.Rules)
				if (!spanning.Contains(rule) && Spans(graph.Bodies[rule]))
				{
					spanning.Add(rule);
					changed = true;
				}
		}

		return spanning;

		bool Spans(Node node)
		{
			switch (node)
			{
				case Node.Literal(var text):
					return text.IndexOf('\n') >= 0 || text.IndexOf('\r') >= 0;

				// A set admits a terminator when it names one, or when it is a complement
				// that does not exclude one — `[^ '|']` matches a newline perfectly well,
				// and a grammar whose field type is written that way is why this matters.
				case Node.Element element:
					return Admits(element);

				case Node.Call(var called, var arguments):
					foreach (var argument in arguments)
						if (Spans(argument))
							return true;

					return spanning.Contains(called);

				// A lookahead consumes nothing, so it spans nothing — but it does read, and
				// what it reads has to be there. That is a window question rather than a
				// retention one, and §6.3's analysis of it is not built.
				case Node.Lookahead:
					return false;

				case Node.Sequence(var nodes):
					foreach (var part in nodes)
						if (Spans(part))
							return true;

					return false;

				case Node.Choice(var nodes):
					foreach (var alternative in nodes)
						if (Spans(alternative))
							return true;

					return false;

				case Node.Repeat(var body, _, var max):
					return max != 0 && Spans(body);

				case Node.Capture(_, var captured): return Spans(captured);
				case Node.Construct(var built, _):  return Spans(built);

				default: return false;
			}
		}
	}

	/// <summary>Whether an element set admits a line terminator.</summary>
	static bool Admits(Node.Element element)
	{
		var named = false;

		foreach (var range in element.Ranges)
			if (range.From <= '\n' && '\n' <= range.To || range.From <= '\r' && '\r' <= range.To)
			{
				named = true;

				break;
			}

		// A Unicode category is not looked into: `\p{Cc}` contains both terminators and
		// several categories could be argued about. Treated as admitting one, which is the
		// safe direction — a rule wrongly said to span a line loses an overload it could
		// have had, and a rule wrongly said not to would lose data.
		if (element.Categories.Count > 0)
			return true;

		return element.IsNegated ? !named : named;
	}
}
