using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Ordered choices that defer their decision past a nested instance of the recursion they stand
/// in, so that every level of a nest is read once for each alternative that reads it before the
/// decision: r to the depth.
/// </summary>
/// <remarks>
/// <para>
/// Let <c>C = A1 | … | Am</c> stand in a rule <c>R</c> of a recursion cycle <c>S</c> of the call
/// graph. Where some <c>Ai</c> and a later <c>Aj</c> read one text <c>u</c> from where <c>C</c>
/// stands, and on the way both enter a rule of <c>S</c> at the same token, the text holds a
/// nested construct in which <c>C</c> can occur again. If the token that refuses <c>Ai</c> comes
/// after that construct, <c>Ai</c> reads it in full and is refused, and <c>Aj</c> reads it in full
/// again: <c>T(k) = 2 T(k - 1)</c> for a nest <c>k</c> deep. Every exponential reading found in the
/// SQL and expression grammars had this shape (a bracketed condition against a predicate that
/// begins with a bracket, a row against a bracketed value, a named function against a call by the
/// same name).
/// </para>
/// <para>
/// Decided by a lock-step simulation of each ordered pair of alternatives over the letters of the
/// normalized graph (kinds, where the grammar is cut in two; characters otherwise). A
/// configuration is a node to read and a continuation: the rest of the rule and the returns of the
/// calls entered. Both configurations are advanced over letters that overlap; a call pushes, a
/// return pops. Entering a rule of <c>S</c> in both configurations on the same letter is a joint
/// entry. Two joint entries are level 2, a joint entry into <c>R</c> itself is level 3, one into
/// any other rule of <c>S</c> is level 1; the pair is told only if both configurations can still
/// read one more letter together after it, which is what makes the decision come after the nest.
/// </para>
/// <para>
/// The choices asked are the explicit <c>|</c> that is not a <c>switch</c> (a <c>switch</c> is
/// decided once, by its selector, and its cases are never tried in turn), and the implicit ones:
/// an option against nothing, a repetition's next turn against its exit, and a positive lookahead
/// against what follows it, which reads its text a second time. A continuation ends at the end of
/// the rule (over kinds a rule answers once, so nothing after it gives the choice back) and at the
/// end of braces around the choice, which commit what the choice read.
/// </para>
/// <para>
/// What separates two alternatives: a <c>?!</c> whose body is a finite set of letter sequences of
/// up to <see cref="LookLetters"/> letters, honoured exactly; and over kinds, the kinds themselves,
/// so that a reserved word is not an identifier. A <c>when</c> is read as nothing, which tells too
/// much, and the pair is marked as guarded so that a reader knows a guard may separate it. Other
/// looks read nothing.
/// </para>
/// <para>
/// Not a proof either way. The search is held to cut-offs (<see cref="Recursion"/>,
/// <see cref="Stack"/>, <see cref="Depth"/>, <see cref="States"/>): a shared prefix longer than
/// <see cref="Depth"/> letters before the nested construct is not seen, nor is one that needs a
/// rule more than <see cref="Recursion"/> times on one stack. A rule marked <c>?</c>, whose answer
/// can be asked for again by its caller, is asked only within its own body.
/// </para>
/// </remarks>
static class SharedRecursion
{
	/// <summary>The level from which a pair is told and its cycle remembered: two joint entries into the cycle, or one into the rule itself. One joint entry is information only.</summary>
	public const int Remembered = 2;

	/// <summary>How many times one rule may be on a configuration's stack; a call past it is cut. A cut-off, not a proof.</summary>
	const int Recursion = 2;

	/// <summary>How many calls deep a configuration's stack may grow; a call past it is cut. A cut-off, not a proof.</summary>
	const int Stack = 48;

	/// <summary>How many letters one pair is simulated for. A cut-off, not a proof: a longer shared prefix is not seen.</summary>
	const int Depth = 14;

	/// <summary>How many pairs of configurations one pair of alternatives may visit. A cut-off, not a proof.</summary>
	const int States = 3000;

	/// <summary>The longest letter sequence a <c>?!</c> is honoured for; a longer look reads nothing.</summary>
	const int LookLetters = 3;

	/// <summary>How many sequences a <c>?!</c> may stand for before it reads nothing.</summary>
	const int LookSequences = 400;

	/// <summary>How many nodes one expansion may pass through without reading a letter before it is cut.</summary>
	const int Nested = 400;

	/// <summary>How many ways one joint letter is split by the looks that hold on it.</summary>
	const int Splits = 16;

	/// <summary>Where a witness is cut.</summary>
	const int Widest = 300;

	/// <summary>A pair of alternatives that read one nested construct before either can be refused.</summary>
	/// <param name="Rule">The rule the choice is written in, as the author wrote it.</param>
	/// <param name="Choice">The choice: a <c>|</c>, an option, a repetition or a positive lookahead.</param>
	/// <param name="Shape">Which of those: <c>|</c>, <c>?</c>, <c>*</c> or <c>?=</c>.</param>
	/// <param name="First">The earlier alternative, from one; for an implicit choice 1 is the reading, 2 what follows.</param>
	/// <param name="Second">The later alternative, from one.</param>
	/// <param name="Level">3: both enter the rule itself again; 2: two joint entries into its cycle; 1: one.</param>
	/// <param name="Guarded">Whether a <c>when</c> was read as nothing on the way, and so may separate the two.</param>
	/// <param name="Witness">The joint letters, each joint entry marked with the rules each side entered.</param>
	/// <param name="Cycle">The rules of the recursion cycle the choice stands in.</param>
	/// <param name="Joint">At each joint entry of the witness, in order, the rules of the cycle both sides entered at that letter: where a reader remembering one of them answers the second alternative without reading the nest (Machine.Memo.cs).</param>
	public sealed record Pair(RuleSymbol Rule, Node Choice, string Shape, int First, int Second, int Level, bool Guarded, string Witness, IReadOnlyList<RuleSymbol> Cycle, IReadOnlyList<IReadOnlyList<RuleSymbol>> Joint);

	/// <summary>What the analysis found, and how much it asked.</summary>
	/// <param name="Found">Every pair told, levels 1 to 3, a pair of a rule's clones once.</param>
	/// <param name="Rules">The rules of the graph asked about.</param>
	/// <param name="Cycles">The strongly connected components of the call graph that hold an edge.</param>
	/// <param name="Recursive">The rules in those components.</param>
	/// <param name="Choices">The choices asked, explicit and implicit, in rules of a cycle.</param>
	/// <param name="Pairs">The ordered pairs of alternatives asked.</param>
	/// <param name="PastStates">Pairs whose simulation reached <see cref="States"/> and was cut.</param>
	/// <param name="PastDepth">Pairs that could still read together after <see cref="Depth"/> letters.</param>
	/// <param name="CallsCut">Calls not entered because of <see cref="Recursion"/> or <see cref="Stack"/>, and expansions past <see cref="Nested"/>.</param>
	public sealed record Result(
		IReadOnlyList<Pair> Found, int Rules, int Cycles, int Recursive, int Choices, int Pairs, int PastStates, int PastDepth, int CallsCut);

	/// <param name="graph">The grammar over characters.</param>
	/// <param name="lexical">The grammar cut in two, where it is: then its syntax over kinds is asked instead.</param>
	public static Result Check(RecognitionGraph graph, LexicalSplit? lexical)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var over = lexical?.Syntax ?? graph;

		return new Search(over, lexical?.Inventory).Run();
	}

	/// <summary>The rest of a reading: a node to read or a rule to return from, then the rest. Interned, so compared by reference.</summary>
	/// <remarks>
	/// Hashed by the number it was interned under rather than by reference. Most rests are hashed
	/// a few times and then dropped, and the hash the runtime keeps for an object is slow to work
	/// out the first time it is asked for: that first time was a large part of the search.
	/// </remarks>
	sealed class Rest(object head, Rest? tail, int number)
	{
		public object Head { get; } = head;

		public Rest? Tail { get; } = tail;

		/// <summary>How many returns the stack holds.</summary>
		public int Returns { get; } = (head is RuleSymbol ? 1 : 0) + (tail?.Returns ?? 0);

		/// <summary>The order it was interned in: unique within one search.</summary>
		public int Number { get; } = number;

		public override bool Equals(object? obj)
		{
			return ReferenceEquals(this, obj);
		}

		public override int GetHashCode()
		{
			return Number;
		}
	}

	/// <summary>The key a <see cref="Rest"/> is interned by: its head and tail, by reference.</summary>
	readonly struct Link(object head, Rest? tail) : IEquatable<Link>
	{
		public object Head { get; } = head;

		public Rest? Tail { get; } = tail;

		public bool Equals(Link other)
		{
			return ReferenceEquals(Head, other.Head) && ReferenceEquals(Tail, other.Tail);
		}

		public override bool Equals(object? obj)
		{
			return obj is Link other && Equals(other);
		}

		public override int GetHashCode()
		{
			return RuntimeHelpers.GetHashCode(Head) * 31 + (Tail?.Number ?? -1);
		}
	}

	/// <summary>
	/// The letter sequences of the <c>?!</c> looks a configuration is still inside: each a sequence's
	/// number and how many of its letters were read. Compared by value.
	/// </summary>
	sealed class Forbid : IEquatable<Forbid>
	{
		public static readonly Forbid None = new([]);

		readonly int _hash;

		Forbid(int[] items)
		{
			Items = items;

			var hash = 17;

			foreach (var item in items)
				hash = hash * 31 + item;

			_hash = hash;
		}

		/// <summary>Sorted: <c>sequence * 8 + read</c>.</summary>
		public int[] Items { get; }

		public bool IsEmpty => Items.Length == 0;

		public static Forbid Of(List<int> items)
		{
			if (items.Count == 0)
				return None;

			var sorted = items.ToArray();

			Array.Sort(sorted);

			var count = 1;

			for (var at = 1; at < sorted.Length; at++)
				if (sorted[at] != sorted[count - 1])
					sorted[count++] = sorted[at];

			if (count < sorted.Length)
				Array.Resize(ref sorted, count);

			return new Forbid(sorted);
		}

		/// <summary>Each sequence's start: <c>sequence * 8</c>, read nothing yet.</summary>
		public static Forbid Starting(int[] sequences)
		{
			if (sequences.Length == 0)
				return None;

			var items = new List<int>(sequences.Length);

			foreach (var sequence in sequences)
				items.Add(sequence * 8);

			return Of(items);
		}

		/// <summary>Both, merged in order: the two are sorted and without repeats already.</summary>
		public Forbid Or(Forbid other)
		{
			if (other.IsEmpty)
				return this;

			if (IsEmpty)
				return other;

			var merged = new int[Items.Length + other.Items.Length];
			var mine   = 0;
			var theirs = 0;
			var count  = 0;

			while (mine < Items.Length || theirs < other.Items.Length)
			{
				int item;

				if (theirs >= other.Items.Length || mine < Items.Length && Items[mine] < other.Items[theirs])
					item = Items[mine++];
				else if (mine >= Items.Length || other.Items[theirs] < Items[mine])
					item = other.Items[theirs++];
				else
				{
					item = Items[mine++];
					theirs++;
				}

				merged[count++] = item;
			}

			if (count < merged.Length)
				Array.Resize(ref merged, count);

			return new Forbid(merged);
		}

		public bool Equals(Forbid? other)
		{
			if (other is null || _hash != other._hash || Items.Length != other.Items.Length)
				return false;

			for (var at = 0; at < Items.Length; at++)
				if (Items[at] != other.Items[at])
					return false;

			return true;
		}

		public override bool Equals(object? obj)
		{
			return obj is Forbid other && Equals(other);
		}

		public override int GetHashCode()
		{
			return _hash;
		}
	}

	/// <summary>One letter a configuration can read next, and what it is then.</summary>
	/// <param name="Letter">What it reads; null where the reading ends here (the end of the rule, or of the braces).</param>
	/// <param name="After">The configuration after the letter.</param>
	/// <param name="Entered">The rules of a cycle entered on the way to the letter.</param>
	/// <param name="Guarded">Whether a <c>when</c> was passed on the way.</param>
	/// <param name="Forbidden">The <c>?!</c> sequences that begin at this letter.</param>
	sealed record Step(FirstSets.First? Letter, Rest? After, RuleSymbol[] Entered, bool Guarded, Forbid Forbidden)
	{
		/// <summary>The rule <see cref="InCycle"/> and <see cref="Self"/> were last worked out for; a pair's rule is asked about every step many times over.</summary>
		public RuleSymbol? AskedFor { get; set; }

		/// <summary>Whether a rule of that rule's cycle was entered on the way to the letter.</summary>
		public bool InCycle { get; set; }

		/// <summary>Whether that rule itself was.</summary>
		public bool Self { get; set; }
	}

	/// <summary>
	/// One joint letter of a witness, with what each side entered on the way to it where both
	/// entered the cycle. Spelled out only for the pair that is told: most states never are.
	/// </summary>
	sealed record Trail(FirstSets.First Letter, RuleSymbol[]? EnteredA, RuleSymbol[]? EnteredB, Trail? Before);

	/// <summary>A pair of configurations being simulated.</summary>
	readonly record struct State(Rest? A, Forbid ForbidA, Rest? B, Forbid ForbidB, int Joint, bool Guarded, Trail? Trail);

	/// <summary>What a pair state is told apart by: the two rests by reference, hashed by their numbers.</summary>
	readonly struct Seen(Rest? a, Forbid forbidA, Rest? b, Forbid forbidB, int joint) : IEquatable<Seen>
	{
		readonly Rest?  _a       = a;
		readonly Forbid _forbidA = forbidA;
		readonly Rest?  _b       = b;
		readonly Forbid _forbidB = forbidB;
		readonly int    _joint   = joint;

		public bool Equals(Seen other)
		{
			return ReferenceEquals(_a, other._a) && ReferenceEquals(_b, other._b) && _joint == other._joint &&
				_forbidA.Equals(other._forbidA) && _forbidB.Equals(other._forbidB);
		}

		public override bool Equals(object? obj)
		{
			return obj is Seen other && Equals(other);
		}

		public override int GetHashCode()
		{
			var hash = (_a?.Number ?? -1) * 31 + (_b?.Number ?? -1);

			hash = hash * 31 + _forbidA.GetHashCode();
			hash = hash * 31 + _forbidB.GetHashCode();

			return hash * 4 + _joint;
		}
	}

	/// <summary>A choice and the continuations of its alternatives.</summary>
	sealed record Candidate(Node Choice, string Shape, Rest?[] Alternatives);

	/// <summary>The rules on the way to a letter, as a list that shares its tail.</summary>
	sealed record Entry(RuleSymbol Rule, Entry? Before)
	{
		/// <summary>What <see cref="Search.Entered"/> made of it, kept: every step read past the call shares it.</summary>
		public RuleSymbol[]? Rules { get; set; }
	}

	/// <summary>What a <c>?!</c> stands for: the letters it refuses outright, and its longer sequences.</summary>
	sealed record Refusal(FirstSets.First? Single, int[] Sequences);

	sealed class Search(RecognitionGraph graph, TerminalInventory? inventory)
	{
		readonly Dictionary<Link, Rest>                 _rests     = [];
		readonly Dictionary<Rest, Step[]>               _expanded  = [];
		readonly Dictionary<Node, Node>                 _starred   = new(NodeIdentity.Instance);
		readonly Dictionary<Node, Node>                 _suffixes  = new(NodeIdentity.Instance);
		readonly Dictionary<Node, Refusal?>             _refusals  = new(NodeIdentity.Instance);
		readonly HashSet<Node>                          _refusing  = new(NodeIdentity.Instance);
		readonly List<FirstSets.First[]>                _sequences = [];
		readonly Dictionary<RuleSymbol, int>            _component = [];
		readonly HashSet<RuleSymbol>                    _seams     = [];

		// Kept for the next pair: one search simulates thousands of them.
		List<State>                                     _frontier  = [];
		List<State>                                     _next      = [];
		readonly HashSet<Seen>                          _seen      = [];

		List<Step>?                                     _steps;

		readonly List<CharRange>                        _ranges    = [];
		readonly Dictionary<int, FirstSets.First>       _literals  = [];
		readonly Dictionary<Node, FirstSets.First>      _elements  = new(NodeIdentity.Instance);

		static readonly Step[] Ending = [new Step(null, null, [], false, Forbid.None)];

		int _callsCut;

		public Result Run()
		{
			foreach (var woven in graph.Trivia.Values)
				if (woven is Node.Call(var called, _))
					_seams.Add(called);

			var rules = graph.Rules.Where(rule => !rule.IsBuiltIn && graph.Bodies.ContainsKey(rule)).ToList();
			var calls = new CallGraph(rules, rule => NodeWalk.Descendants(graph.Bodies[rule]).OfType<Node.Call>().Select(static call => call.Rule));

			var cycles    = 0;
			var recursive = 0;
			var members   = new List<IReadOnlyList<RuleSymbol>>();

			foreach (var component in calls.Components)
			{
				if (component.Count == 1 && !calls.Recurses(component[0]))
					continue;

				foreach (var rule in component)
					_component[rule] = cycles;

				members.Add([.. component]);

				cycles++;
				recursive += component.Count;
			}

			var found      = new List<Pair>();
			var said       = new HashSet<(object, int, int, int)>();
			var choices    = 0;
			var pairs      = 0;
			var pastStates = 0;
			var pastDepth  = 0;

			foreach (var rule in rules)
			{
				if (!_component.TryGetValue(rule, out var component) || Spliced(graph.Origin(rule)))
					continue;

				var candidates = new List<Candidate>();

				Collect(graph.Bodies[rule], null, candidates);

				var origin = graph.Origin(rule);

				for (var at = 0; at < candidates.Count; at++)
				{
					var candidate = candidates[at];

					choices++;

					for (var i = 0; i < candidate.Alternatives.Length; i++)
						for (var j = i + 1; j < candidate.Alternatives.Length; j++)
						{
							pairs++;

							var told = Simulate(rule, component, candidate.Alternatives[i], candidate.Alternatives[j], out var cut, out var deep);

							if (cut)
								pastStates++;

							if (deep)
								pastDepth++;

							// A clone of a rule is walked in the same order, so a choice's place among its
							// rule's is the same in every clone: one finding for what is written once.
							if (told is { } pair && said.Add(((object?)origin.Declaration ?? origin, at, i, j)))
								found.Add(new Pair(origin, candidate.Choice, candidate.Shape, i + 1, j + 1, pair.Level, pair.Guarded, pair.Witness, members[component], pair.Joint));
						}
				}
			}

			return new Result(found, rules.Count, cycles, recursive, choices, pairs, pastStates, pastDepth, _callsCut);
		}

		/// <summary>Whether a rule came in with a grammar spliced onto the author's, rather than being written by the author.</summary>
		static bool Spliced(RuleSymbol rule)
		{
			for (var ns = rule.Namespace; ns is not null; ns = ns.Parent)
				if (ns.IsRoot)
					return true;

			return false;
		}

		Rest Push(object head, Rest? tail)
		{
			var key = new Link(head, tail);

			if (!_rests.TryGetValue(key, out var rest))
				_rests[key] = rest = new Rest(head, tail, _rests.Count);

			return rest;
		}

		/// <summary>A repetition after one turn: any number more, at most as many as it allows (over-approximated as any).</summary>
		Node Starred(Node loop)
		{
			if (loop is Node.Repeat { Min: 0, Max: null })
				return loop;

			if (!_starred.TryGetValue(loop, out var starred))
			{
				var body = loop switch
				{
					Node.Repeat repeat   => repeat.Body,
					Node.Counted counted => counted.Body,
					_                    => loop,
				};

				_starred[loop] = starred = new Node.Repeat(body, 0, null);
			}

			return starred;
		}

		/// <summary>Every choice of a body with the rest of the rule after it, in the order written.</summary>
		void Collect(Node node, Rest? rest, List<Candidate> into)
		{
			switch (node)
			{
				case Node.Choice { Selection: null } choice:
					into.Add(new Candidate(choice, "|", [.. choice.Nodes.Select(alternative => (Rest?)Push(alternative, rest))]));

					foreach (var alternative in choice.Nodes)
						Collect(alternative, rest, into);

					break;

				// A switch is decided once: its cases are never tried in turn.
				case Node.Choice switched:
					foreach (var alternative in switched.Nodes)
						Collect(alternative, rest, into);

					break;

				case Node.Sequence(var parts):
					for (var at = 0; at < parts.Count; at++)
					{
						var after = rest;

						for (var next = parts.Count - 1; next > at; next--)
							after = Push(parts[next], after);

						Collect(parts[at], after, into);
					}

					break;

				case Node.Repeat { Max: 0 }:
					break;

				case Node.Repeat { Min: 0, Max: 1 } option:
					into.Add(new Candidate(option, "?", [Push(option.Body, rest), rest]));
					Collect(option.Body, rest, into);

					break;

				case Node.Repeat { Max: 1 } once:
					Collect(once.Body, rest, into);

					break;

				case Node.Repeat or Node.Counted:
				{
					var body  = node is Node.Repeat repeat ? repeat.Body : ((Node.Counted)node).Body;
					var again = Push(Starred(node), rest);

					into.Add(new Candidate(node, "*", [Push(body, again), rest]));
					Collect(body, again, into);

					break;
				}

				case Node.Lookahead(true, var body):
					into.Add(new Candidate(node, "?=", [Push(body, null), rest]));
					Collect(body, null, into);

					break;

				case Node.Lookahead(false, var body):
					Collect(body, null, into);

					break;

				// Braces commit what they read: nothing after them gives a choice inside back.
				case Node.Atomic(var body):
					Collect(body, null, into);

					break;

				case Node.Capture(_, var body):
					Collect(body, rest, into);

					break;

				case Node.Marked(var body, _):
					Collect(body, rest, into);

					break;

				case Node.Construct(var body, _):
					Collect(body, rest, into);

					break;

				case Node.Bind(_, var body, _):
					Collect(body, rest, into);

					break;
			}
		}

		/// <summary>
		/// Both alternatives in lock step: the level reached, whether a guard was on the way, and the
		/// witness; null where they never enter the cycle together.
		/// </summary>
		(int Level, bool Guarded, string Witness, IReadOnlyList<IReadOnlyList<RuleSymbol>> Joint)? Simulate(RuleSymbol rule, int component, Rest? first, Rest? second, out bool cut, out bool deep)
		{
			cut  = false;
			deep = false;

			var frontier = _frontier;
			var next     = _next;
			var seen     = _seen;
			var past     = false;

			frontier.Add(new State(first, Forbid.None, second, Forbid.None, 0, false, null));

			(int Level, bool Guarded, Trail? Trail)? best = null;

			for (var depth = 0; depth < Depth && frontier.Count > 0; depth++)
			{
				foreach (var state in frontier)
				{
					var one   = Expand(state.A);
					var other = Expand(state.B);

					foreach (var a in one)
					{
						if (a.Letter is null)
							continue;

						var inA       = InCycle(a, component, rule);
						var forbidOne = state.ForbidA.Or(a.Forbidden);

						foreach (var b in other)
						{
							if (b.Letter is null)
								continue;

							var joint = Meet(a.Letter, b.Letter);

							if (joint.Nothing)
								continue;

							var entered   = inA && InCycle(b, component, rule);
							var self      = entered && a.Self && b.Self;
							var count     = state.Joint + (entered ? 1 : 0);
							var guarded   = state.Guarded || a.Guarded || b.Guarded;
							var forbidTwo = state.ForbidB.Or(b.Forbidden);

							// Most pairs are inside no ?! at all, and then the joint letter is the one outcome.
							if (forbidOne.IsEmpty && forbidTwo.IsEmpty)
							{
								if (Reached(joint, Forbid.None, Forbid.None))
								{
									cut = past;

									return Told();
								}

								continue;
							}

							foreach (var (letter, forbidA, forbidB) in Advance(joint, forbidOne, forbidTwo))
							{
								if (Reached(letter, forbidA, forbidB))
								{
									cut = past;

									return Told();
								}
							}

							// Whether the simulation ends at this outcome: at the rule itself, with the pair in
							// best, or past the state budget, with what is told then in best.
							bool Reached(FirstSets.First letter, Forbid forbidA, Forbid forbidB)
							{
								if (self && Survives(a.After, b.After, forbidA, forbidB))
								{
									best = (3, guarded, Along(letter));

									return true;
								}

								var level = Math.Min(count, 2);

								if (!seen.Add(new Seen(a.After, forbidA, b.After, forbidB, level)))
									return false;

								if (seen.Count > States)
								{
									past = true;

									return true;
								}

								var trail = Along(letter);

								if (count >= 1 && (best is null || level > best.Value.Level) && Survives(a.After, b.After, forbidA, forbidB))
									best = (level, guarded, trail);

								next.Add(new State(a.After, forbidA, b.After, forbidB, count, guarded, trail));

								return false;
							}

							Trail Along(FirstSets.First letter)
							{
								return new Trail(letter, entered ? a.Entered : null, entered ? b.Entered : null, state.Trail);
							}
						}
					}
				}

				(frontier, next) = (next, frontier);

				next.Clear();
			}

			deep = frontier.Count > 0;

			return Told();

			// The lists are kept for the next pair, and the witness is spelled out for the pair told
			// alone, not for every state on the way.
			(int Level, bool Guarded, string Witness, IReadOnlyList<IReadOnlyList<RuleSymbol>> Joint)? Told()
			{
				frontier.Clear();
				next.Clear();
				seen.Clear();

				return best is { } reached ? (reached.Level, reached.Guarded, Written(reached.Trail, component), JointOf(reached.Trail, component)) : null;
			}
		}

		/// <summary>
		/// Whether a step entered a rule of the cycle on the way to its letter, its <see cref="Step.Self"/>
		/// set to whether the rule itself was one: worked out once per step and rule.
		/// </summary>
		bool InCycle(Step step, int component, RuleSymbol rule)
		{
			if (ReferenceEquals(step.AskedFor, rule))
				return step.InCycle;

			var inside = false;
			var self   = false;

			foreach (var one in step.Entered)
				if (_component.TryGetValue(one, out var at) && at == component)
				{
					inside = true;
					self  |= ReferenceEquals(one, rule) || one.Equals(rule);
				}

			step.AskedFor = rule;
			step.InCycle  = inside;
			step.Self     = self;

			return inside;
		}

		/// <summary>The rules of the cycle among those entered, in the order entered.</summary>
		List<RuleSymbol> Inside(RuleSymbol[] entered, int component)
		{
			var inside = new List<RuleSymbol>();

			foreach (var one in entered)
				if (_component.TryGetValue(one, out var at) && at == component)
					inside.Add(one);

			return inside;
		}

		static string Names(List<RuleSymbol> rules)
		{
			const int most = 2;

			var names = new List<string>();

			foreach (var rule in rules)
				if (!names.Contains(rule.Name))
				{
					names.Add(rule.Name);

					if (names.Count > most)
						break;
				}

			return names.Count > most ? string.Join(",", names.GetRange(0, most)) + ",..." : string.Join(",", names);
		}

		/// <summary>The rules both sides entered at each joint entry of the witness, in order, the entries where they entered none left out.</summary>
		IReadOnlyList<IReadOnlyList<RuleSymbol>> JointOf(Trail? trail, int component)
		{
			var joint = new List<IReadOnlyList<RuleSymbol>>();

			for (var at = trail; at is not null; at = at.Before)
			{
				if (at.EnteredA is null || at.EnteredB is null)
					continue;

				var inA  = Inside(at.EnteredA, component);
				var inB  = Inside(at.EnteredB, component);
				var both = new List<RuleSymbol>();

				foreach (var rule in inA)
					if (inB.Contains(rule))
						both.Add(rule);

				if (both.Count > 0)
					joint.Add([.. both]);
			}

			joint.Reverse();

			return joint;
		}

		/// <summary>The joint letters of a witness, each joint entry marked with the rules of the cycle each side entered.</summary>
		string Written(Trail? trail, int component)
		{
			var letters = new List<string>();

			for (var at = trail; at is not null; at = at.Before)
			{
				var letter = Spelled(at.Letter);

				if (at.EnteredA is not null && at.EnteredB is not null)
					letter += $"{{{Names(Inside(at.EnteredA, component))}|{Names(Inside(at.EnteredB, component))}}}";

				letters.Add(letter);
			}

			letters.Reverse();

			var written = string.Join(" ", letters);

			return written.Length <= Widest ? written : written.Substring(0, Widest) + " ...";
		}

		/// <summary>Whether both can read one more letter together.</summary>
		bool Survives(Rest? first, Rest? second, Forbid forbidA, Forbid forbidB)
		{
			foreach (var a in Expand(first))
			{
				if (a.Letter is null)
					continue;

				var forbidOne = forbidA.Or(a.Forbidden);

				foreach (var b in Expand(second))
				{
					if (b.Letter is null)
						continue;

					var joint = Meet(a.Letter, b.Letter);

					if (joint.Nothing)
						continue;

					var forbidTwo = forbidB.Or(b.Forbidden);

					if (forbidOne.IsEmpty && forbidTwo.IsEmpty || Advance(joint, forbidOne, forbidTwo).Count > 0)
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// The joint letter split by the <c>?!</c> sequences each side is inside: where a sequence's
		/// next letter holds the joint letter, the sequence moves on and, read to its end, refuses that
		/// side; where it does not, the sequence is done with. A letter that is partly inside is split.
		/// </summary>
		List<(FirstSets.First Letter, Forbid A, Forbid B)> Advance(FirstSets.First joint, Forbid forbidA, Forbid forbidB)
		{
			var outcomes = new List<(FirstSets.First, Forbid, Forbid)>();

			if (forbidA.IsEmpty && forbidB.IsEmpty)
			{
				outcomes.Add((joint, Forbid.None, Forbid.None));

				return outcomes;
			}

			var items = new (int Side, int Item)[forbidA.Items.Length + forbidB.Items.Length];

			for (var at = 0; at < forbidA.Items.Length; at++)
				items[at] = (0, forbidA.Items[at]);

			for (var at = 0; at < forbidB.Items.Length; at++)
				items[forbidA.Items.Length + at] = (1, forbidB.Items[at]);

			Refine(joint, 0, [], []);

			return outcomes;

			void Refine(FirstSets.First letter, int at, List<int> keptA, List<int> keptB)
			{
				// Past the split budget, what is left is done with: telling more, never less.
				if (at == items.Length || outcomes.Count >= Splits)
				{
					outcomes.Add((letter, Forbid.Of(keptA), Forbid.Of(keptB)));

					return;
				}

				var (side, item) = items[at];
				var sequence     = _sequences[item / 8];
				var read         = item % 8;
				var inside       = Meet(letter, sequence[read]);
				var outside      = Minus(letter, sequence[read]);

				if (!inside.Nothing && read + 1 < sequence.Length)
				{
					List<int> a = side == 0 ? [.. keptA, item + 1] : keptA;
					List<int> b = side == 1 ? [.. keptB, item + 1] : keptB;

					Refine(inside, at + 1, a, b);
				}

				if (!outside.Nothing)
					Refine(outside, at + 1, keptA, keptB);
			}
		}

		/// <summary>Every letter a configuration can read next. Remembered per configuration.</summary>
		Step[] Expand(Rest? rest)
		{
			if (rest is null)
				return Ending;

			if (_expanded.TryGetValue(rest, out var known))
				return known;

			var steps = _steps ?? [];

			// Taken while in use: an expansion can ask for another one, through a look's sequences.
			_steps = null;

			Expand(rest.Head, rest.Tail, null, false, null, Forbid.None, steps, 0, null);

			known = [.. steps];

			steps.Clear();

			_steps = steps;

			return _expanded[rest] = known;
		}

		/// <summary>
		/// The letters a configuration can read next: a node or a return to read, <paramref name="head"/>,
		/// and the rest after it, or nothing where both are null.
		/// </summary>
		/// <remarks>
		/// The head is not pushed onto the rest: what is read at once is never a configuration of its
		/// own, and interning every such step on the way was most of the rests the search made and
		/// kept. Only what a letter leaves to read, and the returns, are interned.
		/// </remarks>
		/// <param name="looping">The repetitions being expanded on the way here, by head and rest; made at the first, since most expansions pass none.</param>
		void Expand(object? head, Rest? tail, Entry? entered, bool guarded, FirstSets.First? excluded, Forbid forbidden, List<Step> into, int nested, HashSet<Link>? looping)
		{
			if (nested > Nested)
			{
				_callsCut++;

				return;
			}

			if (head is null)
			{
				into.Add(new Step(null, null, Entered(entered), guarded, forbidden));

				return;
			}

			if (head is RuleSymbol)
			{
				Expand(tail?.Head, tail?.Tail, entered, guarded, excluded, forbidden, into, nested + 1, looping);

				return;
			}

			void Letter(FirstSets.First letter, Rest? after)
			{
				if (excluded is not null)
					letter = Minus(letter, excluded);

				if (!letter.Nothing)
					into.Add(new Step(letter, after, Entered(entered), guarded, forbidden));
			}

			void Then(Rest? next)
			{
				Expand(next?.Head, next?.Tail, entered, guarded, excluded, forbidden, into, nested + 1, looping);
			}

			void Read(Node first, Rest? next)
			{
				Expand(first, next, entered, guarded, excluded, forbidden, into, nested + 1, looping);
			}

			switch ((Node)head)
			{
				case Node.Literal { Text.Length: 0 }:
					Then(tail);

					break;

				case Node.Literal literal:
				{
					var key = literal.Text[0] * 2 + (literal.IgnoreCase ? 1 : 0);

					if (!_literals.TryGetValue(key, out var first))
						_literals[key] = first = literal.IgnoreCase
							? FirstSets.First.Chars(CaseFold.Ranges(literal.Text[0]))
							: FirstSets.First.Chars([new CharRange(literal.Text[0], literal.Text[0])]);

					Letter(first, literal.Text.Length == 1 ? tail : Push(Suffix(literal), tail));

					break;
				}

				case Node.Element element:
				{
					if (!_elements.TryGetValue(element, out var first))
						_elements[element] = first = FirstSets.OfElement(element);

					Letter(first, tail);

					break;
				}

				case Node.Sequence(var parts):
				{
					if (parts.Count == 0)
					{
						Then(tail);

						break;
					}

					var next = tail;

					for (var at = parts.Count - 1; at > 0; at--)
						next = Push(parts[at], next);

					Read(parts[0], next);

					break;
				}

				case Node.Choice(var alternatives):
					foreach (var alternative in alternatives)
						Read(alternative, tail);

					break;

				case Node.Repeat { Max: 0 }:
					Then(tail);

					break;

				case Node.Repeat { Max: 1 } option:
					Read(option.Body, tail);

					if (option.Min == 0)
						Then(tail);

					break;

				case Node.Repeat or Node.Counted:
				{
					// A repetition whose body can read nothing comes back to itself without a letter.
					looping ??= [];

					var self = new Link(head, tail);

					if (!looping.Add(self))
						break;

					var body = head is Node.Repeat repeat ? repeat.Body : ((Node.Counted)head).Body;
					var some = head is Node.Repeat { Min: > 0 } || head is Node.Counted { Min: CountBound.Items(> 0) };

					Read(body, Push(Starred((Node)head), tail));

					if (!some)
						Then(tail);

					looping.Remove(self);

					break;
				}

				case Node.Lookahead(false, var body):
				{
					if (Refused(body) is not { } refusal)
					{
						Then(tail);

						break;
					}

					var more = refusal.Single is null ? excluded : excluded is null ? refusal.Single : excluded.Or(refusal.Single);

					Expand(tail?.Head, tail?.Tail, entered, guarded, more, forbidden.Or(Forbid.Starting(refusal.Sequences)), into, nested + 1, looping);

					break;
				}

				case Node.Guard:
					Expand(tail?.Head, tail?.Tail, entered, true, excluded, forbidden, into, nested + 1, looping);

					break;

				case Node.Atomic(var body):
					Read(body, tail);

					break;

				case Node.Capture(_, var body):
					Read(body, tail);

					break;

				case Node.Marked(var body, _):
					Read(body, tail);

					break;

				case Node.Construct(var body, _):
					Read(body, tail);

					break;

				case Node.Bind(_, var body, _):
					Read(body, tail);

					break;

				// What the host or a binding reads is not known here: anything.
				case Node.Same or Node.External:
					Letter(FirstSets.First.All, tail);

					break;

				case Node.Call({ IsBuiltIn: true, Name: "any" }, _):
					Letter(FirstSets.First.All, tail);

					break;

				case Node.Call({ IsBuiltIn: true, Name: "eof" }, _):
					Letter(FirstSets.First.End, tail);

					break;

				case Node.Call(var called, _):
				{
					if (_seams.Contains(called) || !graph.Bodies.TryGetValue(called, out var callee))
					{
						Then(tail);

						break;
					}

					if (tail is not null && (tail.Returns >= Stack || OnStack(tail, called) >= Recursion))
					{
						_callsCut++;

						break;
					}

					var now = _component.ContainsKey(called) ? new Entry(called, entered) : entered;

					Expand(callee, Push(called, tail), now, guarded, excluded, forbidden, into, nested + 1, looping);

					break;
				}

				// Positive looks, guards decided while generating, glue, a look behind, a reading's
				// test, nothing: none reads a letter.
				default:
					Then(tail);

					break;
			}
		}

		static int OnStack(Rest rest, RuleSymbol rule)
		{
			var count = 0;

			for (Rest? at = rest; at is not null; at = at.Tail)
				if (at.Head is RuleSymbol returned && ReferenceEquals(returned, rule))
					count++;

			return count;
		}

		/// <summary>
		/// The rules on the way, each once, in the order of the last time each was entered: the
		/// rules before the last call without that call's rule, then that rule. Kept on the entry.
		/// </summary>
		static RuleSymbol[] Entered(Entry? entered)
		{
			if (entered is null)
				return [];

			if (entered.Rules is { } known)
				return known;

			var before = Entered(entered.Before);
			var rules  = new RuleSymbol[before.Length + 1];
			var count  = 0;

			foreach (var rule in before)
				if (!rule.Equals(entered.Rule))
					rules[count++] = rule;

			rules[count++] = entered.Rule;

			if (count < rules.Length)
				Array.Resize(ref rules, count);

			return entered.Rules = rules;
		}

		/// <summary>A literal past its first character.</summary>
		Node Suffix(Node.Literal literal)
		{
			if (!_suffixes.TryGetValue(literal, out var suffix))
				_suffixes[literal] = suffix = new Node.Literal(literal.Text.Substring(1)) { IgnoreCase = literal.IgnoreCase };

			return suffix;
		}

		/// <summary>
		/// What a <c>?!</c> refuses, where its body reads a finite set of letter sequences, none empty,
		/// none longer than <see cref="LookLetters"/>, with no guard and no look of its own on the way;
		/// null where it is not that, and then it reads nothing.
		/// </summary>
		Refusal? Refused(Node body)
		{
			if (_refusals.TryGetValue(body, out var known))
				return known;

			// A look reached again while its own sequences are being listed reads nothing there.
			if (!_refusing.Add(body))
				return null;

			Refusal? refusal = null;

			try
			{
				var sequences = new List<FirstSets.First[]>();
				var pending   = new Stack<(Rest? At, FirstSets.First[] Read)>();
				var whole     = true;

				pending.Push((Push(body, null), []));

				while (pending.Count > 0 && whole)
				{
					var (at, read) = pending.Pop();

					foreach (var step in Expand(at))
					{
						if (step.Letter is null)
						{
							if (read.Length == 0)
								whole = false;
							else
								sequences.Add(read);

							continue;
						}

						if (step.Guarded || !step.Forbidden.IsEmpty || step.Letter.Ends || read.Length >= LookLetters)
						{
							whole = false;

							break;
						}

						pending.Push((step.After, [.. read, step.Letter]));
					}

					if (sequences.Count + pending.Count > LookSequences)
						whole = false;
				}

				if (whole && sequences.Count > 0)
				{
					FirstSets.First? single = null;

					var longer = new List<int>();

					foreach (var sequence in sequences)
						if (sequence.Length == 1)
							single = single is null ? sequence[0] : single.Or(sequence[0]);
						else
						{
							longer.Add(_sequences.Count);
							_sequences.Add(sequence);
						}

					refusal = new Refusal(single, [.. longer]);
				}
			}
			finally
			{
				_refusing.Remove(body);
			}

			_refusals[body] = refusal;

			return refusal;
		}

		/// <summary>What two letters both read; the end of the input meets only itself.</summary>
		/// <remarks>
		/// Asked for every pair of letters of every pair of configurations: most are apart, and many
		/// meet in one of the two, so a new set is made only where what both read is neither.
		/// </remarks>
		FirstSets.First Meet(FirstSets.First one, FirstSets.First other)
		{
			if (one.Ends || other.Ends)
				return one.Ends && other.Ends ? FirstSets.First.End : FirstSets.First.None;

			if (one.Stops || other.Stops || one.Anything || other.Anything)
				return one.And(other);

			if (one.Nothing || other.Nothing)
				return FirstSets.First.None;

			var mine   = one.Ranges;
			var others = other.Ranges;

			if (!Maximal(mine) || !Maximal(others))
				return one.Overlaps(other) ? one.And(other) : FirstSets.First.None;

			var shared = _ranges;
			var left   = 0;
			var right  = 0;

			shared.Clear();

			while (left < mine.Count && right < others.Count)
			{
				var from = (char)Math.Max(mine[left].From, others[right].From);
				var to   = (char)Math.Min(mine[left].To,   others[right].To);

				if (from <= to)
					shared.Add(new CharRange(from, to));

				if (mine[left].To < others[right].To)
					left++;
				else
					right++;
			}

			return Made(shared, one, other);
		}

		/// <summary>
		/// A set of the ranges worked out, which are sorted and maximal: one of the two given where it
		/// is the same, nothing where there are none.
		/// </summary>
		static FirstSets.First Made(List<CharRange> ranges, FirstSets.First one, FirstSets.First? other)
		{
			if (ranges.Count == 0)
				return FirstSets.First.None;

			if (Same(ranges, one.Ranges))
				return one;

			if (other is not null && Same(ranges, other.Ranges))
				return other;

			return new FirstSets.First(false, false, ranges.ToArray());
		}

		static bool Same(List<CharRange> ranges, IReadOnlyList<CharRange> other)
		{
			if (ranges.Count != other.Count)
				return false;

			for (var at = 0; at < ranges.Count; at++)
				if (ranges[at] != other[at])
					return false;

			return true;
		}

		/// <summary>Whether ranges are in the form <see cref="FirstSets.First.Chars"/> gives them: sorted, apart and none empty.</summary>
		static bool Maximal(IReadOnlyList<CharRange> ranges)
		{
			for (var at = 0; at < ranges.Count; at++)
			{
				var range = ranges[at];

				if (range.To < range.From || at > 0 && range.From <= ranges[at - 1].To + 1)
					return false;
			}

			return true;
		}

		/// <summary>What one letter reads that another does not.</summary>
		FirstSets.First Minus(FirstSets.First one, FirstSets.First other)
		{
			if (other.Anything || one.Nothing)
				return FirstSets.First.None;

			if (one.Ends || other.Ends)
				return one.Ends && other.Ends ? FirstSets.First.None : one;

			if (other.Nothing)
				return one;

			if (!one.Anything && !one.Stops && !other.Stops && Maximal(one.Ranges) && Maximal(other.Ranges))
			{
				var kept   = _ranges;
				var others = other.Ranges;
				var right  = 0;

				kept.Clear();

				foreach (var range in one.Ranges)
				{
					int start = range.From;

					while (right < others.Count && others[right].To < start)
						right++;

					for (var at = right; at < others.Count && others[at].From <= range.To; at++)
					{
						if (others[at].From > start)
							kept.Add(new CharRange((char)start, (char)(others[at].From - 1)));

						start = others[at].To + 1;
					}

					if (start <= range.To)
						kept.Add(new CharRange((char)start, range.To));
				}

				return Made(kept, one, null);
			}

			var complement = new List<CharRange>();
			var from       = 0;

			foreach (var range in other.Ranges)
			{
				if (range.From > from)
					complement.Add(new CharRange((char)from, (char)(range.From - 1)));

				from = range.To + 1;
			}

			if (from <= char.MaxValue)
				complement.Add(new CharRange((char)from, char.MaxValue));

			var rest = FirstSets.First.Chars(complement);

			return one.Anything ? rest : one.And(rest);
		}

		/// <summary>A letter as a witness writes it.</summary>
		string Spelled(FirstSets.First letter)
		{
			if (letter.Ends)
				return "eof";

			if (letter.Anything)
				return "any";

			var ranges = letter.Ranges;
			var count  = ranges.Sum(static range => range.To - range.From + 1);

			if (inventory is not null)
			{
				if (count == 1 && ranges[0].From > 0 && ranges[0].From - 1 < inventory.Kinds.Count)
					return Kind(inventory.Kinds[ranges[0].From - 1]);

				// A letter of many kinds is most often a name class with every word it holds: the
				// class is what the author wrote.
				TerminalInventory.Pattern? named = null;

				foreach (var range in ranges)
					for (var at = (int)range.From; at <= range.To && named is null; at++)
						if (at > 0 && at - 1 < inventory.Kinds.Count)
							named = inventory.Kinds[at - 1].Matched.FirstOrDefault(static pattern => pattern is TerminalInventory.Pattern.Class);

				if (named is not null)
					return $"[{named} +{count - 1}]";

				return ranges[0].From > 0 && ranges[0].From - 1 < inventory.Kinds.Count
					? $"[{Kind(inventory.Kinds[ranges[0].From - 1])} +{count - 1}]"
					: $"[{count} kinds]";
			}

			if (count == 1)
				return CharRange.Quote(ranges[0].From);

			return ranges.Count == 1 ? $"[{ranges[0]}]" : $"[{ranges[0]} | ...]";
		}

		static string Kind(TerminalInventory.Kind kind)
		{
			if (kind.Matched.Count == 0)
				return kind.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);

			var named = kind.Matched.FirstOrDefault(static pattern => pattern is not TerminalInventory.Pattern.Class) ?? kind.Matched[0];

			return named.ToString();
		}
	}

	/// <summary>A pair as one line of a report.</summary>
	public static string Describe(Pair pair)
	{
		if (pair is null)
			throw new ArgumentNullException(nameof(pair));

		var text = new StringBuilder();

		text.Append('L').Append(pair.Level).Append(pair.Guarded ? " guarded " : " open    ");
		text.Append(pair.Rule.Name).Append(' ').Append(pair.Shape).Append(" alts ").Append(pair.First).Append('/').Append(pair.Second);
		text.Append(": ").Append(pair.Witness);

		return text.ToString();
	}
}
