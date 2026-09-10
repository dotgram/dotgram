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
	/// The same overlap where nothing can give back what it took.
	/// </summary>
	/// <remarks>
	/// A different diagnostic because it has a different consequence, not a different cause.
	/// Over characters an overlap is resolved by backtracking and the parse succeeds by the
	/// second reading, which is <see cref="Ambiguous"/>: worth saying, and not a defect.
	/// Over kinds a rule's answer stands (docs/syntax.md §4) — the optional takes what
	/// follows it, what follows is not there any more, and the rule fails. It does not parse
	/// at all, and it stops one token past what it ate, which is the least helpful place a
	/// parse can stop.
	/// </remarks>
	public const string Swallows = "GRAM5009";

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

		/// <summary>
		/// What both admit — how a lookahead's demand narrows what a sequence begins with.
		/// </summary>
		/// <remarks>
		/// The one asymmetry to <see cref="Or"/>: "anything" is the identity here rather
		/// than the absorber, which is exactly what makes a known constraint survive an
		/// unknown operand — <c>?="@(" &amp; text: @CSharp</c> begins with <c>'@'</c>,
		/// whatever the external would say for itself.
		/// </remarks>
		public First And(First other)
		{
			if (other.Anything)           return this;
			if (Anything)                 return other;
			if (Nothing || other.Nothing) return None;

			var shared = new List<CharRange>();
			var mine   = 0;
			var theirs = 0;

			while (mine < Ranges.Count && theirs < other.Ranges.Count)
			{
				var from = (char)Math.Max(Ranges[mine].From, other.Ranges[theirs].From);
				var to   = (char)Math.Min(Ranges[mine].To,   other.Ranges[theirs].To);

				if (from <= to)
					shared.Add(new CharRange(from, to));

				if (Ranges[mine].To < other.Ranges[theirs].To)
					mine++;
				else
					theirs++;
			}

			return Chars(shared, Ends && other.Ends);
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

			var seam = graph.Trivia.TryGetValue(rule, out var seamNode) &&
				seamNode is Node.Call(var seamRule, _)
					? seamRule
					: null;

			Walk(graph.Bodies[rule], rule, declaration, reported, graph, seam);
		}

		return reported;
	}

	/// <summary>
	/// Every optional and every repetition that can take what stands after it, in a grammar
	/// that cannot give it back.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="Check"/>'s question asked of the syntactic half of a split grammar, where
	/// two of its answers change. It asks only about rules that hand over an array, because
	/// over characters backtracking is total and an overlap is a defect only where an
	/// element already handed over cannot be taken back; here nothing can be taken back at
	/// all, so it asks about every rule. And it asks only about repetitions with no upper
	/// bound, because those are the ones that stop of their own accord; an optional stops of
	/// its own accord too, and three of the five this was written for were optionals.
	/// </para>
	/// <para>
	/// The sets are approximate and in the safe direction, so this over-reports: an optional
	/// whose body begins like what follows but cannot match the whole of it fails and gives
	/// nothing back, and is fine. That is why it is a warning about a shape rather than a
	/// refusal, and why the message says what to write instead — a lookahead in front of the
	/// optional naming what must not be taken, which is what every one of the five became.
	/// Information rather than a warning for the same reason <see cref="Ambiguous"/> is: it
	/// names a shape to look at, and the author is the one who can tell whether the overlap
	/// is real.
	/// </para>
	/// </remarks>
	public static IReadOnlyList<GramDiagnostic> Committed(
		RecognitionGraph graph, TerminalInventory? inventory = null)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var reported = new List<GramDiagnostic>();
		var follow   = FollowSets.Of(graph);

		foreach (var rule in graph.Rules)
			if (rule.Declaration is { } declaration && graph.Bodies.TryGetValue(rule, out var body))
			{
				// What follows the rule itself, for the optionals that stand at the end of
				// it. `RoutineOption`'s last operand is followed by the `AS` of the rule
				// that called it, and nothing inside `RoutineOption` says so.
				var after = follow.TryGetValue(rule, out var beyond) ? beyond.Plain : First.None;

				Swallowed(body, rule, declaration, reported, graph, inventory, after);
			}

		return reported;
	}

	static void Swallowed(
		Node node, RuleSymbol rule, Decl.Rule declaration,
		List<GramDiagnostic> reported, RecognitionGraph graph, TerminalInventory? inventory,
		First after)
	{
		if (node is Node.Sequence(var parts))
		{
			for (var i = 0; i < parts.Count; i++)
			{
				if (Takes(parts, i, graph, after) is not { } taken)
					continue;

				reported.Add(new GramDiagnostic(
					Swallows,
					$"In '{rule.Name}', '{parts[i]}' can take {Spelled(taken, inventory)}, which is " +
					"what follows it — and over kinds a reading that fits is the one that stands " +
					"(docs/syntax.md §4), so nothing gives it back and the rule fails one token past " +
					"what it ate. Say what it may not take: a lookahead in front of it naming the " +
					"words that begin the clause after it.",
					declaration.At.Position,
					declaration.At.Length,
					GramSeverity.Info));
			}
		}

		// Only the body's own alternatives are followed by what follows the rule. Anything
		// nested is followed by the rest of the shape around it, which is a question this
		// does not ask and would answer wrongly by borrowing the rule's.
		var inside = node is Node.Choice ? after : First.None;

		foreach (var child in Children(node))
			Swallowed(child, rule, declaration, reported, graph, inventory, inside);
	}

	/// <summary>The overlap said as the words it stands for, where they can be looked up.</summary>
	/// <remarks>
	/// A kind is a number and a number tells an author nothing. The inventory knows which
	/// patterns each accepting state carries, so the set comes out as `WITH`, `PIVOT`,
	/// `TABLESAMPLE` — which is the whole of what makes this diagnostic answerable. Four at
	/// most: a set of thirty is a set the author will read as "a name", and that is what it
	/// is.
	/// </remarks>
	static string Spelled(First taken, TerminalInventory? inventory)
	{
		if (inventory is null)
			return "what follows it";

		var words = new List<string>();

		foreach (var range in taken.Ranges)
			for (var kind = range.From; kind <= range.To && words.Count < 5; kind++)
				if (kind - 1 < inventory.Kinds.Count && kind >= 1)
					foreach (var pattern in inventory.Kinds[kind - 1].Matched)
					{
						var said = pattern switch
						{
							TerminalInventory.Pattern.Word(_, var text, _) => "`" + text + "`",
							TerminalInventory.Pattern.Mark(_, var text, _) => "`" + text + "`",
							TerminalInventory.Pattern.Class(_, var named)  => named.Name,
							_                                              => null,
						};

						if (said is not null && !words.Contains(said))
							words.Add(said);

						break;
					}

		return words.Count switch
		{
			0 => "what follows it",
			1 => words[0],
			< 5 => string.Join(", ", words.Take(words.Count - 1)) + " and " + words[^1],
			_ => string.Join(", ", words.Take(4)) + " and more",
		};
	}

	/// <summary>Whether one part of a sequence can take what the next one needs.</summary>
	/// <remarks>
	/// <para>
	/// Any repetition that may stop before its bound: <c>X?</c> and <c>X*</c> and
	/// <c>X{1,3}</c> all decide where to stop by what they can match, and <c>X{3}</c> does
	/// not.
	/// </para>
	/// <para>
	/// Asked of what the body can match in <em>one token</em> rather than of what it can
	/// begin with, which is the difference between a warning worth reading and sixty. An
	/// optional that begins like what follows and cannot match the whole of it fails and
	/// gives nothing back: <c>SecurableClass?</c> begins with a word and needs a <c>::</c>
	/// after it, so it never takes a bare name. What bites is an optional that is <em>done</em>
	/// after one token and that token is the one the next clause needed — a run of words, a
	/// bare identifier, an option's name — and that is what this asks about.
	/// </para>
	/// </remarks>
	static First? Takes(
		IReadOnlyList<Node> parts, int at, RecognitionGraph graph, First beyond)
	{
		if (parts[at] is not Node.Repeat(var body, var min, var max) || max is int most && most <= min)
			return null;

		var only = Only(body, graph, []);
		var rest = Following(parts, at + 1, graph);

		// Where nothing after it in this sequence has to read anything, what follows is
		// whatever follows the rule — the caller's next clause, which is where two of the
		// five this was written for did their damage.
		var after = rest.Nothing ? beyond : rest;

		return after.Nothing || !only.Overlaps(after) ? null : only.And(after);
	}

	/// <summary>What a node can match when it matches exactly one token, or nothing.</summary>
	/// <remarks>
	/// A narrower question than <see cref="Of"/>, and the one a committed reading turns on.
	/// Where no reading of the node is one token long the answer is <see cref="First.None"/>,
	/// which overlaps nothing — so a shape that has to read two things before it is done
	/// cannot take one thing that belonged to somebody else.
	/// </remarks>
	static First Only(Node node, RecognitionGraph graph, HashSet<RuleSymbol> seen)
	{
		switch (node)
		{
			case Node.Literal(var text):
				return text.Length == 1 ? First.Chars([new CharRange(text[0], text[0])]) : First.None;

			case Node.Element(var negated, var ranges, _, var references):
				return negated || references.Count > 0 ? First.All : First.Chars(ranges);

			case Node.Choice(var alternatives):
			{
				var made = First.None;

				foreach (var one in alternatives)
					made = made.Or(Only(one, graph, seen));

				return made;
			}

			// One thing read and the rest reading nothing. A lookahead and a guard read
			// nothing; an optional inside may or may not, and where it does the sequence is
			// two tokens long and not this question's business.
			case Node.Sequence(var parts):
			{
				var made  = First.None;
				var found = false;

				// A refusal in front narrows what the one token may be, exactly as it does
				// in `Following` — `?!WindowClause & Identifier` reads a name and not the
				// word that opens the clause after it, and a rule that says so should not
				// be told it might take one.
				First? admits = null;

				foreach (var part in parts)
				{
					if (!found &&
						part is Node.Lookahead(false, var refused) &&
						OneCharacter(refused, graph, []) &&
						Of(refused, graph) is { IsKnown: true } barred)
					{
						var admitted = First.Chars(Complement(First.Normalized(barred.Ranges)));

						admits = admits is { } narrowed ? narrowed.And(admitted) : admitted;

						continue;
					}

					if (Silent(part) || Nullable(part, graph))
						continue;

					if (found)
						return First.None;

					made  = Only(part, graph, seen);
					found = true;
				}

				return found ? admits is { } bound ? made.And(bound) : made : First.None;
			}

			case Node.Repeat(var body, var min, var max):
				return min <= 1 && max is not 0 ? Only(body, graph, seen) : First.None;

			case Node.Capture(_, var held):    return Only(held, graph, seen);
			case Node.Atomic(var kept):        return Only(kept, graph, seen);
			case Node.Marked(var kept, _):     return Only(kept, graph, seen);
			case Node.Construct(var built, _): return Only(built, graph, seen);

			case Node.Call(var rule, _):
				return seen.Add(rule) && graph.Bodies.TryGetValue(rule, out var called)
					? Only(called, graph, seen)
					: First.None;

			default:
				return First.None;
		}
	}

	static void Walk(
		Node node, RuleSymbol rule, Decl.Rule declaration,
		List<GramDiagnostic> reported, RecognitionGraph graph, RuleSymbol? seam)
	{
		if (node is Node.Sequence(var parts))
		{
			for (var i = 0; i < parts.Count - 1; i++)
			{
				if (!Undecided(parts, i, graph, seam))
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
			Walk(child, rule, declaration, reported, graph, seam);
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
	public static bool Undecided(
		IReadOnlyList<Node> parts, int at, RecognitionGraph graph, RuleSymbol? seam = null)
	{
		if (parts is null) throw new ArgumentNullException(nameof(parts));
		if (graph is null) throw new ArgumentNullException(nameof(graph));

		if (at >= parts.Count - 1 || parts[at] is not Node.Repeat(var body, _, null))
			return false;

		// The seam is discounted on both sides. A turn that begins with the namespace's
		// trivia overlaps a continuation that begins with the same trivia on every space
		// — but the two readings of that space are one seam split two ways, not a choice
		// the input decides, so the question is asked of what stands past the seams.
		return Of(PastSeam(body, seam), graph)
			.Overlaps(Following(parts, at + 1, graph, seam));
	}

	/// <summary>The node with a leading application of the seam taken off, for First.</summary>
	static Node PastSeam(Node node, RuleSymbol? seam) =>
		seam is not null &&
		node is Node.Sequence(var parts) &&
		parts.Count > 1 &&
		parts[0] is Node.Call(var called, _) &&
		ReferenceEquals(called, seam)
			? parts.Count == 2 ? parts[1] : new Node.Sequence([.. parts.Skip(1)])
			: node;

	static bool IsSeamCall(Node node, RuleSymbol? seam) =>
		seam is not null && node is Node.Call(var called, _) && ReferenceEquals(called, seam);

	/// <summary>Whether every reading of a node consumes exactly one character.</summary>
	static bool OneCharacter(Node node, RecognitionGraph graph, HashSet<RuleSymbol> seen) =>
		node switch
		{
			Node.Literal(var text)                  => text.Length == 1,
			Node.Element(_, _, _, var references)   => references.Count == 0,
			Node.Choice(var alternatives)           => alternatives.All(one => OneCharacter(one, graph, seen)),
			Node.Sequence(var parts)                => parts.Count(part => !Silent(part)) == 1 &&
			                                           parts.All(part => Silent(part) || OneCharacter(part, graph, seen)),
			Node.Capture(_, var held)               => OneCharacter(held, graph, seen),
			Node.Atomic(var body)                   => OneCharacter(body, graph, seen),
			Node.Marked(var body, _)                => OneCharacter(body, graph, seen),
			Node.Construct(var built, _)            => OneCharacter(built, graph, seen),
			Node.Repeat(var body, var min, var max) => min == 1 && max == 1 && OneCharacter(body, graph, seen),
			Node.Call(var called, _)                => seen.Add(called) &&
			                                           graph.Bodies.TryGetValue(called, out var body) &&
			                                           OneCharacter(body, graph, seen),
			_                                       => false,
		};

	/// <summary>A part that reads nothing: a look, a guard, or nothing at all.</summary>
	static bool Silent(Node node) =>
		node is Node.Empty or Node.Guard or Node.Lookahead or Node.Behind or Node.Glue or Node.Reading;

	/// <summary>What the rest of a sequence can begin with, skipping what may match nothing.</summary>
	public static First Following(
		IReadOnlyList<Node> parts, int from, RecognitionGraph graph, RuleSymbol? seam = null) =>
		Following(parts, from, graph, seam, ByRule(graph));

	static First Following(
		IReadOnlyList<Node> parts, int from, RecognitionGraph graph, RuleSymbol? seam,
		IReadOnlyDictionary<RuleSymbol, First> byRule)
	{
		var ranges  = new List<CharRange>();
		var nothing = true;

		// A positive lookahead met before anything could consume is a constraint on
		// everything after it: whatever the rest begins with must also be what the
		// lookahead's body begins with, here. It is what lets `?="@(" & text: @CSharp`
		// begin with '@' — the external alone can only answer "anything", and that one
		// answer used to poison every first set the C# expression was reachable from,
		// which is every operand of the notation.
		First? expects = null;

		for (var i = from; i < parts.Count; i++)
		{
			if (IsSeamCall(parts[i], seam))
				continue;

			if (nothing &&
				parts[i] is Node.Lookahead(true, var expected) &&
				!Nullable(expected, graph) &&
				Of(expected, graph, byRule) is { IsKnown: true } ahead)
			{
				expects = expects is { } held ? held.And(ahead) : ahead;

				continue;
			}

			// A negative lookahead of one character is the mirror: the rest may not begin
			// with what it refuses. Only where every reading of the refused body is exactly
			// one character — a keyword over kinds, a single literal, a class — does the
			// refusal say anything about the first character alone: `?!"CASE"` over
			// characters refuses `CASE`, not every word beginning with `C`. This is what
			// tells `?!Reserved & Identifier` from the keywords over kinds, where a word is
			// one kind whether it is reserved or not.
			if (nothing &&
				parts[i] is Node.Lookahead(false, var refused) &&
				OneCharacter(refused, graph, []) &&
				Of(refused, graph, byRule) is { IsKnown: true } barred)
			{
				var admitted = First.Chars(Complement(First.Normalized(barred.Ranges)));

				expects = expects is { } narrowed ? narrowed.And(admitted) : admitted;

				continue;
			}

			var first = Of(parts[i], graph, byRule);

			if (expects is { } bound)
				first = first.And(bound);

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

			// Past a part that may have consumed, the position is no longer where the
			// lookahead looked, and the constraint may not be carried further.
			expects = null;
		}

		return nothing ? First.None : First.Chars(ranges);
	}

	/// <summary>What a node can begin with.</summary>
	public static First Of(Node node, RecognitionGraph graph) => Of(node, graph, ByRule(graph));

	/// <summary>
	/// What each rule can begin with, as the least set that satisfies every rule at once.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A grammar's rules call each other and themselves, so this is not a walk: it is the
	/// least fixed point of the same kind of equations <c>Nullable</c> and <c>FOLLOW</c> are
	/// already solved as. Every rule starts at nothing and grows until nothing grows, which
	/// settles because the step only ever adds and there are finitely many characters to add.
	/// </para>
	/// <para>
	/// What it replaces answered "anything" on re-entering a rule already being walked. That
	/// is sound, and it is Top — and Top at the head of a rule is Top for everything that
	/// contains it. <c>A = B | 'a'</c> with <c>B = A | 'b'</c> has the perfectly ordinary
	/// answer <c>{a, b}</c> and used to have none, and every proof that rests on knowing
	/// what something begins with was that much weaker for it.
	/// </para>
	/// <para>
	/// A rule with no body — a built-in, an external recognizer — stays "anything", which is
	/// the honest answer: what it accepts is the host's knowledge and not the grammar's.
	/// </para>
	/// </remarks>
	static IReadOnlyDictionary<RuleSymbol, First> ByRule(RecognitionGraph graph)
	{
		if (graph.FirstByRule is { } settled)
			return settled;

		var estimates = new Dictionary<RuleSymbol, First>();

		foreach (var rule in graph.Rules)
			estimates[rule] = graph.Bodies.ContainsKey(rule) ? First.None : First.All;

		// Set before the loop, so a rule reached while its own answer is still being worked
		// out reads the estimate rather than starting the walk again.
		graph.FirstByRule = estimates;

		// No round limit: the step is monotone over a finite lattice, so it settles. A bound
		// here would be an admission that the argument is not believed.
		for (var changed = true; changed;)
		{
			changed = false;

			foreach (var rule in graph.Rules)
			{
				if (!graph.Bodies.TryGetValue(rule, out var body))
					continue;

				var next = Of(body, graph, estimates);

				if (Same(next, estimates[rule]))
					continue;

				estimates[rule] = next;
				changed         = true;
			}
		}

		return estimates;
	}

	/// <summary>Whether two sets are the same set.</summary>
	/// <remarks>
	/// Written out because <see cref="First"/> is a record whose ranges are a list, so its
	/// own equality compares that list by reference — never equal here, and the loop above
	/// would never have stopped. <see cref="First.Normalized"/> sorts and merges, so equal
	/// sets really do have equal sequences.
	/// </remarks>
	public static bool Same(First one, First other)
	{
		if (one.Anything != other.Anything || one.Nothing != other.Nothing || one.Ends != other.Ends)
			return false;

		if (one.Ranges.Count != other.Ranges.Count)
			return false;

		for (var at = 0; at < one.Ranges.Count; at++)
			if (one.Ranges[at] != other.Ranges[at])
				return false;

		return true;
	}

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

	static First Of(Node node, RecognitionGraph graph, IReadOnlyDictionary<RuleSymbol, First> byRule)
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
			case Node.Glue:
			case Node.Reading:
				return First.None;

			case Node.Capture  (_,  var captured): return Of(captured, graph, byRule);
			case Node.Construct(var built, _):     return Of(built,    graph, byRule);
			case Node.Atomic   (var body):         return Of(body,     graph, byRule);
			case Node.Marked   (var body, _):      return Of(body,     graph, byRule);
			case Node.Repeat   (var body, _, _):   return Of(body,     graph, byRule);

			// What has to stop the walk is a cycle, and a cycle is a rule already on the way
			// down — not one met and left somewhere else. Kept as the path rather than as
			// everything visited, so that two alternatives calling the same rule do not make
			// the second one unknowable: it was the first that used the name up.
			// Read off the fixed point rather than walked into. A rule that reaches itself
			// used to answer "anything" — the walk had to stop somewhere and Top is the safe
			// place to stop — and that is the answer that poisoned everything below it: one
			// recursive rule at the head of a grammar made every set containing it
			// unknowable, and with it every proof that rests on knowing.
			case Node.Call(var called, _):
				return byRule.TryGetValue(called, out var settledFor) ? settledFor : First.All;

			case Node.Choice(var alternatives):
			{
				var ranges  = new List<CharRange>();
				var nothing = true;

				foreach (var alternative in alternatives)
				{
					var first = Of(alternative, graph, byRule);

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
				return Following(parts, 0, graph, null, byRule);

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

	/// <summary>
	/// What must begin the input where a node begins, given what must begin it where the
	/// node ends.
	/// </summary>
	/// <remarks>
	/// A node that must consume something answers for itself. One that may match nothing
	/// leaves the question to what comes after it as well as to itself, so the two are taken
	/// together — the direction that admits too much, and so proves too little, rather than
	/// the one that proves something false.
	/// </remarks>
	public static First Precedes(Node node, First after, RecognitionGraph graph)
	{
		var first = Of(node, graph);

		return first.Nothing      ? after :
			Nullable(node, graph) ? first.Or(after) :
			first;
	}

	/// <summary>Whether a node can match without consuming anything.</summary>
	public static bool Nullable(Node node, RecognitionGraph graph) => Nullable(node, graph.RuleIsNullable);

	/// <summary>
	/// The same question where the answer for a rule is not settled yet.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Which is the normalizer's case: it computes rule nullability as a fixed point, so
	/// while that is running the answer for a call has to come from the estimate rather than
	/// from a graph that does not exist. That is the only thing the two callers differ in,
	/// and it used to be the reason there were two of this function — with the two drifting
	/// apart in both directions. One did not look inside a repetition, so <c>A+</c> over a
	/// nullable <c>A</c> was called consuming; the other did not know <c>Behind</c>, so a
	/// lookbehind was too. Each was right where the other was wrong, which is what a second
	/// copy of a definition is for.
	/// </para>
	/// <para>
	/// So the shape of a node is answered here, once, and who can answer for a rule is the
	/// parameter.
	/// </para>
	/// </remarks>
	public static bool Nullable(Node node, Func<RuleSymbol, bool> rule) => node switch
	{
		Node.Empty or Node.Guard or Node.Lookahead or Node.Behind or Node.Glue or Node.Reading => true,
		Node.Literal(var text)                       => text.Length == 0,
		Node.Element                                 => false,
		Node.Atomic(var body)                        => Nullable(body,     rule),
		Node.Marked(var body, _)                     => Nullable(body,     rule),
		Node.Capture(_, var captured)                => Nullable(captured, rule),
		Node.Construct(var built, _)                 => Nullable(built,    rule),
		Node.Repeat(var body, var min, _)            => min == 0 || Nullable(body, rule),
		Node.Sequence(var parts)                     => All(parts,        rule),
		Node.Choice(var alternatives)                => Any(alternatives, rule),
		Node.Call(var called, _)                     => rule(called),
		_                                            => false,
	};

	static bool All(IReadOnlyList<Node> nodes, Func<RuleSymbol, bool> rule)
	{
		foreach (var node in nodes)
			if (!Nullable(node, rule))
				return false;

		return true;
	}

	static bool Any(IReadOnlyList<Node> nodes, Func<RuleSymbol, bool> rule)
	{
		foreach (var node in nodes)
			if (Nullable(node, rule))
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
			case Node.Marked   (var body, _):      return [body];
			default:                               return [];
		}
	}
}
