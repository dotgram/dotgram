using System;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// What a rule's language holds, asked of one string at a time.
/// </summary>
/// <remarks>
/// <para>
/// A matcher and not an analysis. The lexical split needs to know whether one terminal's
/// text is also another terminal's — whether <c>SELECT</c> is an identifier, whether
/// <c>0</c> is a numeric literal — and the strings it asks about are keywords and
/// punctuation, a dozen characters at most, against rules that are the shapes a lexical
/// rule has. Running them is cheaper than reasoning about them and, more to the point,
/// exact: approximating one way refuses valid programs and the other way accepts invalid
/// ones.
/// </para>
/// <para>
/// Not a parser. Nothing is built, captured or reported; the answer is a set of positions
/// the string could have been read to. A lookahead is taken as satisfied, which admits a
/// little more than the rule does — a terminal wrongly admitted here carries one pattern
/// too many, which widens what a syntactic position accepts rather than narrowing it.
/// </para>
/// </remarks>
static class Language
{
	/// <summary>Whether a rule's language holds exactly this string.</summary>
	public static bool Accepts(RecognitionGraph graph, RuleSymbol rule, string text) =>
		graph.Bodies.TryGetValue(rule, out var body) && Ends(graph, body, text, 0).Contains(text.Length);

	/// <summary>The shortest string a node accepts, or null where it accepts none.</summary>
	/// <remarks>
	/// A witness, used to ask whether two rules can accept the same string. Depth-limited
	/// because a rule that reaches itself has no shortest string this walk can reach by
	/// recursion alone, and a rule deep enough to hit the limit has no witness a reader
	/// would want to see anyway.
	/// </remarks>
	public static string? Shortest(RecognitionGraph graph, Node node, int depth = 0)
	{
		if (depth > Deep)
			return null;

		switch (node)
		{
			case Node.Empty or Node.Guard or Node.Behind or Node.Lookahead or Node.Glue:
				return "";

			case Node.Literal(var text):
				return text;

			case Node.Element element:
				return FirstSets.OfElement(element) is { IsKnown: true, Ranges.Count: > 0 } set
					? set.Ranges[0].From.ToString()
					: null;

			case Node.Sequence(var parts):
			{
				var built = "";

				foreach (var part in parts)
					if (Shortest(graph, part, depth + 1) is { } one)
						built += one;
					else
						return null;

				return built;
			}

			case Node.Choice(var alternatives):
			{
				string? best = null;

				foreach (var alternative in alternatives)
					if (Shortest(graph, alternative, depth + 1) is { } one &&
						(best is null || one.Length < best.Length))
					{
						best = one;
					}

				return best;
			}

			case Node.Repeat(var body, var min, _):
			{
				if (min == 0)
					return "";

				if (Shortest(graph, body, depth + 1) is not { } once)
					return null;

				var built = "";

				for (var turn = 0; turn < min; turn++)
					built += once;

				return built;
			}

			case Node.Call(var called, _):
				return graph.Bodies.TryGetValue(called, out var inner)
					? Shortest(graph, inner, depth + 1)
					: null;

			case Node.Atomic(var kept):        return Shortest(graph, kept, depth + 1);
			case Node.Marked(var kept, _):     return Shortest(graph, kept, depth + 1);
			case Node.Capture(_, var held):    return Shortest(graph, held, depth + 1);
			case Node.Construct(var built, _): return Shortest(graph, built, depth + 1);

			default: return null;
		}
	}

	const int Deep = 16;

	static HashSet<int> Ends(RecognitionGraph graph, Node node, string text, int at)
	{
		switch (node)
		{
			case Node.Empty or Node.Guard or Node.Behind or Node.Lookahead or Node.Glue:
				return [at];

			case Node.Literal(var literal) one:
				return at + literal.Length <= text.Length &&
					text.AsSpan(at, literal.Length).Equals(
						literal.AsSpan(),
						one.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
					? [at + literal.Length]
					: [];

			case Node.Element element:
				return at < text.Length && FirstSets.OfElement(element) is { IsKnown: true } set &&
					set.Overlaps(FirstSets.First.Chars([new CharRange(text[at], text[at])]))
					? [at + 1]
					: [];

			case Node.Sequence(var parts):
			{
				var here = new HashSet<int> { at };

				foreach (var part in parts)
				{
					var next = new HashSet<int>();

					foreach (var one in here)
						next.UnionWith(Ends(graph, part, text, one));

					if (next.Count == 0)
						return [];

					here = next;
				}

				return here;
			}

			case Node.Choice(var alternatives):
			{
				var all = new HashSet<int>();

				foreach (var alternative in alternatives)
					all.UnionWith(Ends(graph, alternative, text, at));

				return all;
			}

			case Node.Repeat(var body, var min, var max):
			{
				var reached = new HashSet<int>();
				var here    = new HashSet<int> { at };

				if (min == 0)
					reached.Add(at);

				// A turn that consumes nothing is dropped rather than repeated: the string is
				// finite, so a run of them can only ever arrive where it already is.
				for (var turn = 1; turn <= (max ?? text.Length + 1) && here.Count > 0; turn++)
				{
					var next = new HashSet<int>();

					foreach (var one in here)
						foreach (var end in Ends(graph, body, text, one))
							if (end > one)
								next.Add(end);

					if (turn >= min)
						reached.UnionWith(next);

					here = next;
				}

				return reached;
			}

			case Node.Call(var called, _):
				return graph.Bodies.TryGetValue(called, out var inner)
					? Ends(graph, inner, text, at)
					: [];

			case Node.Atomic(var kept):        return Ends(graph, kept, text, at);
			case Node.Marked(var kept, _):     return Ends(graph, kept, text, at);
			case Node.Capture(_, var held):    return Ends(graph, held, text, at);
			case Node.Construct(var built, _): return Ends(graph, built, text, at);

			default: return [];
		}
	}
}
