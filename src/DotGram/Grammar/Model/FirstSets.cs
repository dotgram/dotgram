using System;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>
/// What a construct can begin with, and what that says about a grammar.
/// </summary>
/// <remarks>
/// <para>
/// Ordered choice hides ambiguity rather than reporting it. <c>Row* &amp; Trailer</c>
/// where a trailer also reads as a record parses perfectly well: the repetition takes the
/// trailer, the rule fails, the repetition gives it back, and the parse succeeds by the
/// second reading. Nothing is wrong with the answer and nothing tells the author that
/// their grammar had two of them and backtracking picked one.
/// </para>
/// <para>
/// It is worth saying because the cost is invisible until it is not: the grammar reads as
/// though the trailer is a trailer, a reader assumes the repetition stops at it, and the
/// engine only agrees by accident. It also decides what a streamed parse may do — an
/// element handed to the caller cannot be given back, so a repetition that would ever
/// want to is exactly the one a stream cannot read.
/// </para>
/// <para>
/// The sets are approximate, and in the safe direction: a construct this cannot decide
/// about answers "anything", two "anything"s overlap, and the report is a report rather
/// than a refusal. Being told about an overlap that is not real costs a sentence; missing
/// one costs the thing this exists to prevent.
/// </para>
/// </remarks>
public static class FirstSets
{
	/// <summary>A repetition that may be unable to tell its own end from its elements.</summary>
	public const string Ambiguous = "GRAM5002";

	/// <summary>
	/// What a construct can begin with.
	/// </summary>
	/// <param name="Anything">
	/// Nothing useful is known — a complement, a Unicode category, a C# predicate. Treated
	/// as overlapping everything, which is the direction that reports too much.
	/// </param>
	/// <param name="Nothing">It consumes nothing at all, so it begins with nothing.</param>
	/// <param name="Ends">
	/// The input may end here. Not a character and not the absence of knowledge: a place
	/// where a parse that must read everything has read it. It is what tells a repetition at
	/// the end of a whole parse that nothing is waiting for the input it took — and it
	/// overlaps nothing, because no character is the end of the text.
	/// </param>
	public sealed record First(
		bool Anything, bool Nothing, IReadOnlyList<CharRange> Ranges, bool Ends = false)
	{
		public static readonly First All  = new(true,  false, []);
		public static readonly First None = new(false, true,  []);

		/// <summary>Nothing follows but the end of the input.</summary>
		public static readonly First End = new(false, false, [], Ends: true);

		/// <summary>
		/// A known set, its ranges sorted and merged.
		/// </summary>
		/// <remarks>
		/// Everything below leans on the ranges being in this form: <see cref="Covers"/> is
		/// exact only over maximal ranges, <see cref="Overlaps"/> walks the two lists once
		/// each, and the fixed point in <c>FollowSets</c> stops growing only because a union
		/// of the same sets is the same list rather than a longer spelling of it.
		/// </remarks>
		public static First Chars(IEnumerable<CharRange> ranges, bool ends = false)
		{
			var merged = Normalized(ranges);

			return merged.Count == 0 && !ends ? None : new First(false, false, merged, ends);
		}

		internal static IReadOnlyList<CharRange> Normalized(IEnumerable<CharRange> ranges)
		{
			var sorted = new List<CharRange>(ranges);

			sorted.Sort(static (a, b) => a.From.CompareTo(b.From));

			var merged = new List<CharRange>(sorted.Count);

			foreach (var range in sorted)
			{
				if (range.To < range.From)
					continue;

				// Overlapping or adjacent: one maximal range. `To + 1` is int arithmetic,
				// so the top of the character space does not wrap.
				if (merged.Count > 0 && merged[^1].To + 1 >= range.From)
				{
					if (range.To > merged[^1].To)
						merged[^1] = merged[^1] with { To = range.To };

					continue;
				}

				merged.Add(range);
			}

			return merged;
		}

		/// <summary>Whether it says anything a repetition can be held to.</summary>
		public bool IsKnown => !Anything && !Nothing;

		/// <summary>Both, for a place either could begin.</summary>
		public First Or(First other)
		{
			if (Anything || other.Anything) return All;
			if (Nothing)                    return other.Ends || !Ends ? other : Chars(other.Ranges, true);
			if (other.Nothing)              return this;

			if (Ends == (Ends || other.Ends) && Covers(other))
				return this;
			if (other.Ends == (Ends || other.Ends) && other.Covers(this))
				return other;

			var ranges = new List<CharRange>(Ranges.Count + other.Ranges.Count);

			ranges.AddRange(Ranges);
			ranges.AddRange(other.Ranges);

			return Chars(ranges, Ends || other.Ends);
		}

		/// <summary>Whether this says everything that one does.</summary>
		/// <remarks>What a fixed point is reached by: nothing new was said this time round.</remarks>
		public bool Covers(First other)
		{
			if (Anything)
				return true;

			if (other.Anything || other.Ends && !Ends)
				return false;

			// Both lists sorted and maximal, so containment is one walk: each of theirs must
			// sit inside a single one of mine, and the candidates only move forward.
			var mine = 0;

			foreach (var theirs in other.Ranges)
			{
				while (mine < Ranges.Count && Ranges[mine].To < theirs.From)
					mine++;

				if (mine >= Ranges.Count || Ranges[mine].From > theirs.From || theirs.To > Ranges[mine].To)
					return false;
			}

			return true;
		}

		public bool Overlaps(First other)
		{
			if (Nothing  || other.Nothing)  return false;
			if (Anything || other.Anything) return true;

			var mine = 0;
			var theirs = 0;

			while (mine < Ranges.Count && theirs < other.Ranges.Count)
			{
				if (Ranges[mine].To < other.Ranges[theirs].From)
					mine++;
				else if (other.Ranges[theirs].To < Ranges[mine].From)
					theirs++;
				else
					return true;
			}

			return false;
		}
	}

	/// <summary>
	/// Every repetition whose end is not told apart from one more of its elements.
	/// </summary>
	public static IReadOnlyList<GramDiagnostic> Check(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var reported = new List<GramDiagnostic>();

		foreach (var rule in graph.Rules)
		{
			if (rule.Declaration is not { } declaration)
				continue;

			// Only a rule that asks to be read in parts. An overlap is not a defect on its
			// own — §11 makes backtracking total and a grammar is entitled to lean on it,
			// which is what `'a'+ & 'a'` does and means. It becomes a defect exactly where
			// the parse cannot go back: a rule declaring a sequence is asking to be handed
			// over an element at a time, and an element handed over cannot be taken back.
			if (!graph.Types.TryGetValue(rule, out var type) ||
				!type.EndsWith("[]", StringComparison.Ordinal))
			{
				continue;
			}

			Walk(graph.Bodies[rule], rule, declaration, reported, graph);
		}

		return reported;
	}

	static void Walk(
		Node node, RuleSymbol rule, Decl.Rule declaration,
		List<GramDiagnostic> reported, RecognitionGraph graph)
	{
		if (node is Node.Sequence(var parts))
		{
			for (var i = 0; i < parts.Count - 1; i++)
			{
				if (!Undecided(parts, i, graph))
					continue;

				reported.Add(new GramDiagnostic(
					Ambiguous,
					$"In '{rule.Name}', the repetition '{parts[i]}' can begin with the same input as " +
					"what follows it, so where it ends is decided by backtracking rather than by the " +
					"grammar. It parses, and the reading you get is the one the engine happened to "   +
					"find. It is also what stops the rule being read from a stream, where an element " +
					"handed over cannot be taken back (docs/syntax.md §6.3).",
					declaration.At.Position,
					declaration.At.Length,
					GramSeverity.Info));
			}
		}

		foreach (var child in Children(node))
			Walk(child, rule, declaration, reported, graph);
	}

	/// <summary>
	/// Whether where a repetition ends is decided by backtracking rather than by the
	/// grammar.
	/// </summary>
	/// <remarks>
	/// Only a repetition that may stop of its own accord — one with no upper bound it can
	/// reach without the input saying so. <c>X{3}</c> ends where it ends, and nothing about
	/// what follows can move it.
	/// </remarks>
	/// <param name="parts">The sequence it is one of.</param>
	/// <param name="at">Where in that sequence it is.</param>
	public static bool Undecided(IReadOnlyList<Node> parts, int at, RecognitionGraph graph)
	{
		if (parts is null) throw new ArgumentNullException(nameof(parts));
		if (graph is null) throw new ArgumentNullException(nameof(graph));

		return at < parts.Count - 1 &&
			parts[at] is Node.Repeat(var body, _, null) &&
			Of(body, graph).Overlaps(Following(parts, at + 1, graph));
	}

	/// <summary>What the rest of a sequence can begin with, skipping what may match nothing.</summary>
	public static First Following(IReadOnlyList<Node> parts, int from, RecognitionGraph graph) =>
		Following(parts, from, graph, []);

	static First Following(
		IReadOnlyList<Node> parts, int from, RecognitionGraph graph, HashSet<RuleSymbol> seen)
	{
		var ranges  = new List<CharRange>();
		var nothing = true;

		for (var i = from; i < parts.Count; i++)
		{
			var first = Of(parts[i], graph, seen);

			if (first.Anything)
				return First.All;

			if (first.Nothing)
				continue;

			nothing = false;
			ranges.AddRange(first.Ranges);

			// Something that must consume settles it; anything optional and the one after
			// it could be what actually follows.
			if (!Nullable(parts[i], graph))
				break;
		}

		return nothing ? First.None : First.Chars(ranges);
	}

	/// <summary>What a node can begin with.</summary>
	public static First Of(Node node, RecognitionGraph graph) => Of(node, graph, []);

	/// <summary>
	/// An element's characters, callable before a graph exists.
	/// </summary>
	/// <remarks>
	/// The normalizer asks while it is still building the graph — §4.6 has to decide at
	/// weave time whether a literal's characters continue a word — and an element needs no
	/// graph to answer: its ranges, its categories expanded, its negation complemented.
	/// A reference inside makes the honest answer "anything".
	/// </remarks>
	public static First OfElement(Node.Element element)
	{
		if (element is null)
			throw new ArgumentNullException(nameof(element));

		if (element.References.Count > 0)
			return First.All;

		var all = new List<CharRange>(element.Ranges);

		foreach (var category in element.Categories)
			all.AddRange(CategoryRanges(category));

		var known = First.Normalized(all);

		return First.Chars(element.IsNegated ? Complement(known) : known);
	}

	static First Of(Node node, RecognitionGraph graph, HashSet<RuleSymbol> seen)
	{
		switch (node)
		{
			// Everything the recognizer would fold together: the characters whose
			// upper-case form is the first character's. Worked out by the same rule the
			// emitted comparison uses, so the set is exactly the characters that can
			// begin the match — no wider and no narrower.
			case Node.Literal { IgnoreCase: true } literal:
				return literal.Text.Length == 0 ? First.None : Folded(literal.Text[0]);

			case Node.Literal(var text):
				return text.Length == 0
					? First.None
					: new First(false, false, [new CharRange(text[0], text[0])]);

			// A category is a set of characters like any other, and saying "anything"
			// about it was what poisoned every follow set downstream of an identifier:
			// `\p{L}` at the head of a rule made everything after that rule unknowable.
			// A reference is the one honest "anything" left — it is a C# predicate, and
			// what it accepts is the host's knowledge, not the grammar's.
			case Node.Element(var negated, var ranges, var categories, var references):
			{
				if (references.Count > 0)
					return First.All;

				var all = new List<CharRange>(ranges);

				foreach (var category in categories)
					all.AddRange(CategoryRanges(category));

				var known = First.Normalized(all);

				return First.Chars(negated ? Complement(known) : known);
			}

			// Consumes nothing, so it begins with nothing — and a lookahead's own first set
			// is not what the sequence begins with, because the operand after it is.
			case Node.Empty:
			case Node.Guard:
			case Node.Lookahead:
			case Node.Behind:
				return First.None;

			case Node.Capture  (_,  var captured): return Of(captured, graph, seen);
			case Node.Construct(var built, _):     return Of(built,    graph, seen);
			case Node.Atomic   (var body):         return Of(body,     graph, seen);
			case Node.Repeat   (var body, _, _):   return Of(body,     graph, seen);

			// What has to stop the walk is a cycle, and a cycle is a rule already on the way
			// down — not one met and left somewhere else. Kept as the path rather than as
			// everything visited, so that two alternatives calling the same rule do not make
			// the second one unknowable: it was the first that used the name up.
			case Node.Call(var called, _):
			{
				if (!seen.Add(called))
					return First.All;

				var first = graph.Bodies.TryGetValue(called, out var body2)
					? Of(body2, graph, seen)
					: First.All;

				seen.Remove(called);

				return first;
			}

			case Node.Choice(var alternatives):
			{
				var ranges  = new List<CharRange>();
				var nothing = true;

				foreach (var alternative in alternatives)
				{
					var first = Of(alternative, graph, seen);

					if (first.Anything)
						return First.All;

					if (first.Nothing)
						continue;

					nothing = false;
					ranges.AddRange(first.Ranges);
				}

				return nothing ? First.None : First.Chars(ranges);
			}

			case Node.Sequence(var parts):
				return Following(parts, 0, graph, seen);

			default:
				return First.All;
		}
	}

	/// <summary>
	/// The characters a Unicode category holds, as ranges over the UTF-16 code units the
	/// recognizer reads.
	/// </summary>
	/// <remarks>
	/// Found by asking <see cref="System.Globalization.CharUnicodeInfo"/> once per code
	/// unit and once per category, and cached: the scan is a compile-time cost paid per
	/// distinct category a grammar names, and what it buys is that `\p{L}` stops being
	/// "anything" — which is what let follow sets stay known across an identifier.
	/// </remarks>
	static IReadOnlyList<CharRange> CategoryRanges(string name)
	{
		lock (_categoryRanges)
		{
			if (_categoryRanges.TryGetValue(name, out var cached))
				return cached;
		}

		var ranges = new List<CharRange>();

		// The graph carries the abbreviation as written — `L`, `Lu` — and the same table
		// the emitter renders tests from says which .NET categories that stands for.
		var members = new List<System.Globalization.UnicodeCategory>();

		foreach (var member in UnicodeCategories.Expand(name))
			if (Enum.TryParse<System.Globalization.UnicodeCategory>(member, out var category))
				members.Add(category);

		if (members.Count > 0)
		{
			var start = -1;

			for (var c = 0; c <= char.MaxValue; c++)
			{
				var inside =
					members.Contains(System.Globalization.CharUnicodeInfo.GetUnicodeCategory((char)c));

				if (inside && start < 0)
					start = c;
				else if (!inside && start >= 0)
				{
					ranges.Add(new CharRange((char)start, (char)(c - 1)));
					start = -1;
				}
			}

			if (start >= 0)
				ranges.Add(new CharRange((char)start, char.MaxValue));
		}
		else
		{
			// A name this cannot place is not a licence to guess: the whole space is the
			// honest answer, and it arrives as one known range rather than as "anything"
			// so that negation still works over it.
			ranges.Add(new CharRange(char.MinValue, char.MaxValue));
		}

		lock (_categoryRanges)
			_categoryRanges[name] = ranges;

		return ranges;
	}

	static readonly Dictionary<string, IReadOnlyList<CharRange>> _categoryRanges = [];

	/// <summary>Everything outside a normalized set of ranges.</summary>
	static IReadOnlyList<CharRange> Complement(IReadOnlyList<CharRange> ranges)
	{
		var outside = new List<CharRange>(ranges.Count + 1);
		var next    = 0;

		foreach (var range in ranges)
		{
			if (range.From > next)
				outside.Add(new CharRange((char)next, (char)(range.From - 1)));

			next = range.To + 1;
		}

		if (next <= char.MaxValue)
			outside.Add(new CharRange((char)next, char.MaxValue));

		return outside;
	}

	/// <summary>
	/// The characters the case-folded comparison would accept where <paramref name="first"/>
	/// is the literal's first character.
	/// </summary>
	static First Folded(char first)
	{
		lock (_folded)
		{
			if (_folded.TryGetValue(first, out var cached))
				return cached;
		}

		var upper  = char.ToUpperInvariant(first);
		var ranges = new List<CharRange>();
		var start  = -1;

		for (var c = 0; c <= char.MaxValue; c++)
		{
			var inside = char.ToUpperInvariant((char)c) == upper;

			if (inside && start < 0)
				start = c;
			else if (!inside && start >= 0)
			{
				ranges.Add(new CharRange((char)start, (char)(c - 1)));
				start = -1;
			}
		}

		if (start >= 0)
			ranges.Add(new CharRange((char)start, char.MaxValue));

		var folded = First.Chars(ranges);

		lock (_folded)
			_folded[first] = folded;

		return folded;
	}

	static readonly Dictionary<char, First> _folded = [];

	/// <summary>Whether a node can match without consuming anything.</summary>
	public static bool Nullable(Node node, RecognitionGraph graph) => node switch
	{
		Node.Empty or Node.Guard or Node.Lookahead or Node.Behind => true,
		Node.Literal(var text)                     => text.Length == 0,
		Node.Repeat(_, var min, _)                 => min == 0,
		Node.Atomic(var body)                      => Nullable(body, graph),
		Node.Capture(_, var captured)              => Nullable(captured, graph),
		Node.Construct(var built, _)               => Nullable(built,    graph),
		Node.Sequence(var parts)                   => All(parts, graph),
		Node.Choice(var alternatives)              => Any(alternatives, graph),
		Node.Call(var called, _)                   => graph.Nullable.TryGetValue(called, out var yes) && yes,
		_                                          => false,
	};

	static bool All(IReadOnlyList<Node> nodes, RecognitionGraph graph)
	{
		foreach (var node in nodes)
			if (!Nullable(node, graph))
				return false;

		return true;
	}

	static bool Any(IReadOnlyList<Node> nodes, RecognitionGraph graph)
	{
		foreach (var node in nodes)
			if (Nullable(node, graph))
				return true;

		return false;
	}

	static IEnumerable<Node> Children(Node node)
	{
		switch (node)
		{
			case Node.Sequence (var parts):        return parts;
			case Node.Choice   (var alternatives): return alternatives;
			case Node.Repeat   (var body, _, _):   return [body];
			case Node.Capture  (_, var captured):  return [captured];
			case Node.Construct(var built, _):     return [built];
			case Node.Atomic   (var body):         return [body];
			default:                               return [];
		}
	}
}
