using System;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>
/// What can come after a rule, wherever it is called from.
/// </summary>
/// <remarks>
/// <para>
/// A rule is compiled once and called from everywhere, so what follows it is not a property
/// of the rule but of the place — and the compiler, having one rule and many places, has to
/// answer for all of them at once. Without that answer everything at the end of a rule's
/// body is compiled as though anything at all might come next, which is what makes a
/// repetition there keep its ways back: it cannot be shown that nothing would ever come for
/// them.
/// </para>
/// <para>
/// The answer is the union over the call sites, and it is circular — what follows a rule
/// depends on what follows the rules that call it, and a grammar may call round in a ring.
/// So it is computed as a fixed point: start with what the publications say, go round adding
/// what each call site contributes, and stop when a round adds nothing.
/// </para>
/// <para>
/// A published rule is where the answer comes from rather than where it is needed. A
/// <c>parse</c> reads the whole input, so after its root there is the end of the text and
/// nothing else — a fact, and the strongest one here. A <c>yield</c> hands its elements out
/// one at a time, and what comes after one of them is the next step the driver asks for, not
/// anything the element could be asked to give back to; so after its root there is nothing,
/// and it is <em>nothing</em> rather than <em>the end</em> because a caller may stop
/// enumerating wherever it likes and the end of the text must not be claimed. A <c>find</c>
/// may stop anywhere, so after its root there is anything — whether it, too, could say
/// nothing is a question nobody has worked through, and an unexamined <c>None</c> there would
/// be quiet daring rather than knowledge. A rule published several ways gets all of them,
/// which is the widest.
/// </para>
/// </remarks>
public static class FollowSets
{
	/// <summary>
	/// What can follow, seen twice: as it stands, and past the seam.
	/// </summary>
	/// <param name="Plain">What the continuation can begin with, as ever.</param>
	/// <param name="AfterSeam">
	/// What the continuation can begin with once a leading application of the namespace's
	/// trivia has consumed what it consumes. §4.5 puts that application at the head of
	/// every spaced seam, so a repetition whose turns lead with the trivia and the
	/// continuation behind it both start by reading the same run of it — and the question
	/// that decides whether a turn could instead have been the continuation is asked of
	/// what each reads <em>next</em>. Compared plainly the two overlap on the trivia itself
	/// and the comparison says nothing.
	/// </param>
	/// <param name="Lead">
	/// What the continuation begins to <em>consume</em> with, as a node rather than as
	/// characters: the half of the question characters cannot answer. A turn led by
	/// <c>?!X</c> begins only where <c>X</c> failed, so a continuation that is <c>X</c>
	/// cannot match there however much their first sets overlap — and
	/// <c>' '* &amp; '|' &amp; ' '*</c> overlaps a turn of <c>(?!Sep &amp; any)+</c> on the
	/// space, honestly and uselessly. Unknown wherever it is not worked out, which is what
	/// <c>default</c> is, so a continuation built anywhere that has not thought about this
	/// claims nothing.
	/// </param>
	public readonly record struct Continuation(
		FirstSets.First Plain, FirstSets.First AfterSeam, Lead Lead = default)
	{
		public static readonly Continuation All  = new(FirstSets.First.All, FirstSets.First.All, Lead.Unknown);
		public static readonly Continuation None = new(FirstSets.First.None, FirstSets.First.None, Lead.Nothing);
		public static readonly Continuation End  = new(FirstSets.First.End, FirstSets.First.End, Lead.Ending);

		public Continuation Or(Continuation other)
		{
			return new(Plain.Or(other.Plain), AfterSeam.Or(other.AfterSeam), Lead.Or(other.Lead));
		}

		public bool Covers(Continuation other)
		{
			return Plain.Covers(other.Plain) && AfterSeam.Covers(other.AfterSeam) && Lead.Covers(other.Lead);
		}
	}

	/// <summary>
	/// The node a continuation begins to consume with, joined over the places it can have
	/// come from: nothing yet, the end of the input, one named rule, one literal, or unknown.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A lattice and not a guess. <c>default</c> is unknown — the top, which claims nothing —
	/// so a <see cref="Continuation"/> put together by code that has never heard of this is
	/// as safe as it was. <see cref="Nothing"/> is the bottom, what a continuation that
	/// cannot be entered at all carries, and it is what lets a choice join its alternatives
	/// starting from <see cref="Continuation.None"/> without poisoning the answer.
	/// </para>
	/// <para>
	/// Two leads that are not the same node join to unknown, because the question this
	/// answers — is every way into the continuation the very thing a look refused? — has no
	/// answer once there are two of them. A rule and a literal are the only keys kept: they
	/// are what a negative look is written over, and an element written twice is two nodes
	/// that no comparison of identity would call equal anyway.
	/// </para>
	/// </remarks>
	public readonly record struct Lead
	{
		readonly byte _kind;

		Lead(byte kind, RuleSymbol? rule, string? literal, bool ends)
		{
			_kind   = kind;
			Rule    = rule;
			Literal = literal;
			Ends    = ends;
		}

		/// <summary>The rule a continuation leading with a call names, where it leads with one.</summary>
		public RuleSymbol? Rule { get; }

		/// <summary>The text a continuation leading with a literal reads, where it leads with one.</summary>
		public string? Literal { get; }

		/// <summary>Whether the end of the input is one of the ways in.</summary>
		public bool Ends { get; }

		/// <summary>Nothing is known: the top of the lattice, and what <c>default</c> is.</summary>
		public static readonly Lead Unknown = default;

		/// <summary>There is no way in at all: the bottom, which joins with anything to it.</summary>
		public static readonly Lead Nothing = new(1, null, null, false);

		/// <summary>The end of the input, and nothing else.</summary>
		public static readonly Lead Ending = new(2, null, null, true);

		/// <summary>A continuation that begins by calling one named rule.</summary>
		public static Lead Calling(RuleSymbol rule)
		{
			return new(2, rule, null, false);
		}

		/// <summary>A continuation that begins by reading one literal.</summary>
		public static Lead Reading(string literal)
		{
			return new(2, null, literal, false);
		}

		public bool IsUnknown => _kind == 0;

		public bool IsNothing => _kind == 1;

		/// <summary>The join: what is known of two ways in at once.</summary>
		public Lead Or(Lead other)
		{
			if (IsNothing)  return other;
			if (other.IsNothing) return this;
			if (IsUnknown || other.IsUnknown) return Unknown;

			// The end joins with a node rather than fighting it: a continuation may be the
			// end of the input or that one rule, which is the shape `(Separator | eof)` has.
			if (ReferenceEquals(Rule, other.Rule) && string.Equals(Literal, other.Literal, StringComparison.Ordinal))
				return new(2, Rule, Literal, Ends || other.Ends);

			if (Rule is null && Literal is null)      return new(2, other.Rule, other.Literal, true);
			if (other.Rule is null && other.Literal is null) return new(2, Rule, Literal, true);

			return Unknown;
		}

		/// <summary>
		/// Whether this already holds everything that one does — the lattice's <c>≥</c>, and
		/// not equality: the fixed point stops going round when every contribution is covered,
		/// so a test that answered "no" to two values whose join is the first of them would
		/// send the walk round for ever.
		/// </summary>
		public bool Covers(Lead other)
		{
			return IsUnknown || other.IsNothing ||
			!IsNothing &&
			(ReferenceEquals(Rule, other.Rule) && string.Equals(Literal, other.Literal, StringComparison.Ordinal) ||
				other.Rule is null && other.Literal is null) &&
			(Ends || !other.Ends);
		}

		/// <summary>
		/// Whether every way into this continuation is the node a look refused, or the end of
		/// the input. The end is admitted because the question is asked at the start of a turn
		/// that consumed: there are characters left there, so the end is not one of them.
		/// </summary>
		public bool Refuses(Node refused)
		{
			return !IsUnknown && !IsNothing && refused switch
			{
				Node.Call(var called, { Count: 0 }) => ReferenceEquals(Rule, called),
				Node.Literal(var text) => Literal is not null &&
													   string.Equals(Literal, text, StringComparison.Ordinal),
				_ => false,
			};
		}
	}

	/// <summary>The rule a namespace applies at its seams, for the rule being walked.</summary>
	public static RuleSymbol? SeamOf(RuleSymbol rule, RecognitionGraph graph)
	{
		return graph is not null && graph.Trivia.TryGetValue(rule, out var trivia) &&
		trivia is Node.Call(var seam, _)
			? seam
			: null;
	}

	/// <summary>What may follow each rule, as far as the grammar settles it.</summary>
	public static IReadOnlyDictionary<RuleSymbol, Continuation> Of(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		return graph.FollowByRule ??= Settle(graph);
	}

	static IReadOnlyDictionary<RuleSymbol, Continuation> Settle(RecognitionGraph graph)
	{
		var follow = new Dictionary<RuleSymbol, Continuation>();

		foreach (var rule in graph.Rules)
			follow[rule] = Continuation.None;

		// A publication is entered through the shape the entry method is written in — the
		// namespace's trivia, the rule, the trivia again — and that leading application is
		// a call site like any other. Left out of the walk, the trivia is never told that a
		// look may stand right behind it and want to read what the trivia would otherwise
		// swallow.
		var entries = new List<(Node Body, Continuation After, RuleSymbol? Seam)>();

		foreach (var publication in graph.Publications)
		{
			if (!follow.ContainsKey(publication.Rule))
				continue;

			var after = publication.Kind switch
			{
				PublishKind.Parse => Continuation.End,
				PublishKind.Yield => Continuation.None,
				_                 => Continuation.All,
			};

			if (graph.Trivia.TryGetValue(publication.Rule, out var around))
				entries.Add((
					new Node.Sequence([around, new Node.Call(publication.Rule, []), around]),
					after,
					SeamOf(publication.Rule, graph)));
			else
				follow[publication.Rule] = follow[publication.Rule].Or(after);
		}

		// Every body contributes once, even with an empty FOLLOW: its internal call
		// sites can have constant continuations. Later only changed rules need a walk.
		var queue = new Queue<RuleSymbol>(graph.Rules);
		var queued = new HashSet<RuleSymbol>(graph.Rules);

		foreach (var (body, after, seam) in entries)
			Contribute(body, after, seam);

		while (queue.Count > 0)
		{
			var rule = queue.Dequeue();

			queued.Remove(rule);

			if (graph.Bodies.TryGetValue(rule, out var body))
				Contribute(body, follow[rule], SeamOf(rule, graph));
		}

		void Contribute(Node node, Continuation after, RuleSymbol? seam)
		{
			switch (node)
			{
				case Node.Call(var called, _):
				{
					// A rule lowered under another namespace peels a different seam, so
					// what this site knows past its own is no use to it. The plain half
					// travels regardless.
					var told = ReferenceEquals(SeamOf(called, graph), seam)
						? after
						: new Continuation(after.Plain, FirstSets.First.All);

					if (!follow.TryGetValue(called, out var held) || held.Covers(told))
						return;

					follow[called] = held.Or(told);
					if (queued.Add(called))
						queue.Enqueue(called);

					return;
				}

				// Each part is followed by the rest of the sequence, and by what follows
				// the sequence where the rest can match nothing.
				case Node.Sequence(var parts):
				{
					var next = after;

					for (var i = parts.Count - 1; i >= 0; i--)
					{
						Contribute(parts[i], next, seam);

						next = Precedes(parts[i], next, graph, seam);
					}

					return;
				}

				// Every alternative is followed by whatever the choice is.
				case Node.Choice(var alternatives):
				{
					foreach (var alternative in alternatives)
						Contribute(alternative, after, seam);

					return;
				}

				// A turn is followed by another turn or by whatever the repetition is
				// followed by — except that an optional has no other turn, and telling
				// it that one might follow poisons everything upstream of its own first
				// set. `(Argument & …)?` inside a call was telling `Argument` that
				// anything could follow it, and that "anything" walked back through
				// every rule a value can name.
				case Node.Repeat(var body, _, var max):
					Contribute(
						body,
						max == 1 ? after : Precedes(body, after, graph, seam).Or(after),
						seam);

					return;

				case Node.Capture(_, var captured): Contribute(captured, after, seam); return;
				case Node.Construct(var built, _):  Contribute(built,    after, seam); return;
				case Node.Atomic(var kept):         Contribute(kept,     after, seam); return;
				case Node.Marked(var kept, _):      Contribute(kept,     after, seam); return;

				// Inside, nothing follows — the atomic group's sentence, and for the same
				// reason: a look is decided at its first match and gives back everything it
				// read, so nothing after it can ever ask its body for a different reading.
				// What is read again by whatever comes next follows the *look*, not its body,
				// and that is the caller's continuation rather than anything contributed here.
				// What follows a part inside the body is unaffected: the sequence above
				// threads it, so `?(A* & B)` still hands B's first set to the star.
				case Node.Lookahead(_, var seen):
					Contribute(seen, Continuation.None, seam);

					return;
			}
		}
		return follow;
	}

	/// <summary>
	/// What must begin the input where a node begins, given what must begin it where the
	/// node ends — both halves at once.
	/// </summary>
	/// <remarks>
	/// The same walk the compiler makes over a sequence, and it has to be the same: what a
	/// rule is told about its callers has to agree with what the compiler works out inside
	/// them, or a repetition would be held to something nobody meant.
	/// </remarks>
	public static Continuation Precedes(
		Node node, Continuation after, RecognitionGraph graph, RuleSymbol? seam)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var cache = graph.Continuations;
		if (cache is null)
			return ComputePrecedes(node, after, graph, seam);

		var key = cache.Of(node, after, seam);
		if (!cache.Precedes.TryGetValue(key, out var result))
			cache.Precedes[key] = result = ComputePrecedes(node, after, graph, seam);
		return result;
	}

	/// <summary>
	/// What a continuation that begins with <paramref name="node"/> and goes on as
	/// <paramref name="after"/> says begins to <em>consume</em>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Two rules hold this to what it can actually prove, and they are the proof itself.
	/// <b>A node that may read nothing answers unknown</b> rather than handing the question
	/// on to what follows it: where the continuation can match empty, what decides a
	/// give-back is whatever stands behind it and in the end the end of the input, and
	/// chasing that is reasoning nobody here has written down. <b>Only a call and a literal
	/// are keys</b>, because those are what a negative look is written over; an element is
	/// a fresh node at every mention, so identity would call two spellings of one class
	/// different and prove nothing.
	/// </para>
	/// <para>
	/// The other half of the proof lives where the answer is used
	/// (<see cref="Determinism.NeverGivesBack"/>): the turn must consume, so that the start
	/// of a completed turn has input left and the end of the text is not one of the ways in.
	/// </para>
	/// </remarks>
	static Lead LeadOf(Node node, Continuation after, RecognitionGraph graph)
	{
		if (AtEnd(node, graph))
			return Lead.Ending;

		switch (node)
		{
			// A call of a rule that must read something is the way in, and the rule is the key.
			case Node.Call(var called, { Count: 0 }):
				return FirstSets.Nullable(node, graph) ? Lead.Unknown : Lead.Calling(called);

			case Node.Literal(var text):
				return text.Length == 0 ? after.Lead : Lead.Reading(text);

			// Zero-width: it decides nothing about what is consumed, so the question passes
			// through to whatever consumes next.
			case Node.Empty or Node.Guard or Node.Lookahead or Node.Behind or Node.Glue or Node.Reading:
				return after.Lead;

			case Node.Capture(_, var captured):  return LeadOf(captured, after, graph);
			case Node.Construct(var built, _):   return LeadOf(built,    after, graph);
			case Node.Atomic(var kept):          return LeadOf(kept,     after, graph);
			case Node.Marked(var kept, _):       return LeadOf(kept,     after, graph);

			case Node.Repeat(var body, var min, _):
				return min == 0 ? Lead.Unknown : LeadOf(body, after, graph);

			case Node.Choice(var alternatives):
			{
				var merged = Lead.Nothing;

				foreach (var alternative in alternatives)
					merged = merged.Or(LeadOf(alternative, after, graph));

				return merged;
			}

			case Node.Sequence(var parts):
			{
				foreach (var part in parts)
				{
					if (FirstSets.Nullable(part, graph))
					{
						// Zero-width parts are stepped over; a part that may read nothing but
						// could have read something is the unknown this refuses to guess at.
						if (part is Node.Empty or Node.Guard or Node.Lookahead or Node.Behind or Node.Glue or Node.Reading)
							continue;

						return Lead.Unknown;
					}

					return LeadOf(part, after, graph);
				}

				return after.Lead;
			}

			default:
				return Lead.Unknown;
		}
	}

	static Continuation ComputePrecedes(
		Node node, Continuation after, RecognitionGraph graph, RuleSymbol? seam)
	{
		return Characters(node, after, graph, seam) with { Lead = LeadOf(node, after, graph) };
	}

	/// <summary>The two character halves, which is what this answered before there was a third.</summary>
	static Continuation Characters(
		Node node, Continuation after, RecognitionGraph graph, RuleSymbol? seam)
	{
		switch (node)
		{
			// The end: it reads nothing, and nothing after it can be read.
			case var end when AtEnd(end, graph):
				return Continuation.End;

			// The seam itself, standing first: what follows once it has been read is the
			// old continuation as it plainly was. That is the definition of the other half.
			case Node.Call(var called, _) when seam is not null && ReferenceEquals(called, seam):
				return new Continuation(Plainly(node, after.Plain, graph), after.Plain);

			// A rule of the same namespace that begins by reading the seam: past it, what the
			// rest of the rule begins with. A publication's rule is entered after the seam and
			// almost always opens with a repetition spaced by it.
			case Node.Call(var called, { Count: 0 }) when seam is not null &&
				ReferenceEquals(SeamOf(called, graph), seam) &&
				graph.Bodies.TryGetValue(called, out var calledBody) &&
				Unwrapped(calledBody) is Node.Sequence(var calledParts) && calledParts.Count > 1 &&
				calledParts[0] is Node.Call(var leading, _) && ReferenceEquals(leading, seam):
				return new Continuation(
					Plainly(node, after.Plain, graph),
					Plainly(new Node.Sequence([.. calledParts.Skip(1)]), after.Plain, graph));

			// Structure is walked rather than summarized, or a sequence that merely leads
			// with the seam would be taxed for beginning with trivia characters — which is
			// precisely the shape every spaced continuation has.
			case Node.Sequence(var parts):
			{
				var next = after;

				for (var i = parts.Count - 1; i >= 0; i--)
					next = Precedes(parts[i], next, graph, seam);

				return next;
			}

			case Node.Choice(var alternatives):
			{
				var merged = Continuation.None;

				foreach (var alternative in alternatives)
					merged = merged.Or(Precedes(alternative, after, graph, seam));

				return merged;
			}

			// A look consumes nothing but it still reads, so what stands before it is
			// followed by what the look reads as well as by what follows the look. A
			// refusal reads too: it has to see the characters it refuses before it can say
			// they are not there, and where they are not there the rest reads them
			// instead. Either way the answer is the same union, and leaving it out is what
			// let a run before a look be called settled when the look was about to want a
			// character the run had taken.
			case Node.Lookahead(_, var watched):
			{
				var read  = FirstSets.Of(watched, graph);
				var plain = read.Or(Plainly(node, after.Plain, graph));

				var seamFirst = seam is not null && graph.Bodies.TryGetValue(seam, out var body)
					? FirstSets.Of(body, graph)
					: FirstSets.First.None;

				var past = read.Overlaps(seamFirst) ? FirstSets.First.All : read;

				return new Continuation(plain, past.Or(after.AfterSeam));
			}

			case Node.Capture(_, var captured):  return Precedes(captured, after, graph, seam);
			case Node.Construct(var built, _):   return Precedes(built,    after, graph, seam);
			case Node.Atomic(var kept):          return Precedes(kept,     after, graph, seam);
			case Node.Marked(var kept, _):       return Precedes(kept,     after, graph, seam);

			// A turn is either taken — and then it stands before another turn or before the
			// continuation, with anything past its own seam unknowable from here — or, for
			// a run that may be empty, not taken at all.
			case Node.Repeat(var body, var min, _):
			{
				var plain = Plainly(node, after.Plain, graph);
				var turn = Precedes(
					body,
					new Continuation(plain, FirstSets.First.All),
					graph, seam);

				return new Continuation(
					plain,
					min == 0 ? turn.AfterSeam.Or(after.AfterSeam) : turn.AfterSeam);
			}

			default:
			{
				var plain = Plainly(node, after.Plain, graph);
				var first = FirstSets.Of(node, graph);

				if (first.Nothing)
					return after with { Plain = plain };

				// A leaf that does not lead with the seam. A turn's seam may still have
				// consumed input where this continuation would have to begin, so its first
				// set counts past the seam only where the seam could not even have begun
				// there — and whether the seam could have *carried on* over it is
				// <c>Contained</c>'s question, asked where the peel is used, not here.
				var seamFirst = seam is not null && graph.Bodies.TryGetValue(seam, out var seamBody)
					? FirstSets.Of(seamBody, graph)
					: FirstSets.First.None;

				var past = first.Overlaps(seamFirst) ? FirstSets.First.All : first;

				return new Continuation(
					plain,
					FirstSets.Nullable(node, graph) ? past.Or(after.AfterSeam) : past);
			}
		}
	}

	/// <summary>The plain half alone, as it always was.</summary>
	public static FirstSets.First Plainly(Node node, FirstSets.First after, RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		if (AtEnd(node, graph))
			return FirstSets.First.End;

		// Part by part, so that an end inside it stops what follows from being let through.
		if (node is Node.Sequence(var parts))
		{
			for (var i = parts.Count - 1; i >= 0; i--)
				after = Plainly(parts[i], after, graph);

			return after;
		}

		var first = FirstSets.Of(node, graph);

		return first.Nothing                 ? after :
			FirstSets.Nullable(node, graph) ? first.Or(after) :
			first;
	}

	/// <summary>A rule's body as it reads, past what builds and names it.</summary>
	static Node Unwrapped(Node body)
	{
		return body switch
		{
			Node.Construct(var built, _) => Unwrapped(built),
			Node.Capture(_, var captured) => Unwrapped(captured),
			_ => body,
		};
	}

	/// <summary>
	/// Whether a node matches only at the end of the input: <c>?!any</c>, or a rule that is
	/// nothing else — <c>eof</c>.
	/// </summary>
	/// <remarks>
	/// It reads nothing, so it is nullable and what follows it would ordinarily be let through as
	/// what may begin the input where it stands. Where it succeeds nothing follows, so the end is
	/// the whole answer.
	/// </remarks>
	internal static bool AtEnd(Node node, RecognitionGraph graph)
	{
		return node switch
		{
			Node.Lookahead(false, Node.Element { IsNegated: true, Ranges.Count: 0, Categories.Count: 0, References.Count: 0 }) => true,
			Node.Call(var called, { Count: 0 }) => graph.Bodies.TryGetValue(called, out var body) &&
				body is Node.Lookahead(false, Node.Element { IsNegated: true, Ranges.Count: 0, Categories.Count: 0, References.Count: 0 }),
			_ => false,
		};
	}
}
