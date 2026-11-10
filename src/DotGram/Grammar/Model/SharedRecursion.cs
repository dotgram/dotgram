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
	sealed class Rest(object head, Rest? tail)
	{
		public object Head { get; } = head;

		public Rest? Tail { get; } = tail;

		/// <summary>How many returns the stack holds.</summary>
		public int Returns { get; } = (head is RuleSymbol ? 1 : 0) + (tail?.Returns ?? 0);
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
			return RuntimeHelpers.GetHashCode(Head) * 31 + (Tail is null ? 0 : RuntimeHelpers.GetHashCode(Tail));
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

		public static Forbid Of(IEnumerable<int> items)
		{
			var sorted = items.Distinct().OrderBy(static item => item).ToArray();

			return sorted.Length == 0 ? None : new Forbid(sorted);
		}

		public Forbid Or(Forbid other)
		{
			if (other.IsEmpty)
				return this;

			if (IsEmpty)
				return other;

			return Of(Items.Concat(other.Items));
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
	sealed record Step(FirstSets.First? Letter, Rest? After, RuleSymbol[] Entered, bool Guarded, Forbid Forbidden);

	/// <summary>One joint letter of a witness, with the rules each side entered on it where both did, and the rules of the cycle both entered.</summary>
	sealed record Trail(string Letter, RuleSymbol[] Both, Trail? Before);

	/// <summary>A pair of configurations being simulated.</summary>
	readonly record struct State(Rest? A, Forbid ForbidA, Rest? B, Forbid ForbidB, int Joint, bool Guarded, Trail? Trail);

	/// <summary>What a pair state is told apart by.</summary>
	readonly record struct Seen(Rest? A, Forbid ForbidA, Rest? B, Forbid ForbidB, int Joint);

	/// <summary>A choice and the continuations of its alternatives.</summary>
	sealed record Candidate(Node Choice, string Shape, Rest?[] Alternatives);

	/// <summary>The rules on the way to a letter, as a list that shares its tail.</summary>
	sealed record Entry(RuleSymbol Rule, Entry? Before);

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
				_rests[key] = rest = new Rest(head, tail);

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

			var frontier = new List<State> { new(first, Forbid.None, second, Forbid.None, 0, false, null) };
			var seen     = new HashSet<Seen>();

			(int Level, bool Guarded, Trail? Trail)? best = null;

			for (var depth = 0; depth < Depth && frontier.Count > 0; depth++)
			{
				var next = new List<State>();

				foreach (var state in frontier)
				{
					var one   = Expand(state.A);
					var other = Expand(state.B);

					foreach (var a in one)
					{
						if (a.Letter is null)
							continue;

						foreach (var b in other)
						{
							if (b.Letter is null)
								continue;

							var joint = Meet(a.Letter, b.Letter);

							if (joint.Nothing)
								continue;

							var inA     = InCycle(a.Entered, component, rule, out var selfA);
							var inB     = InCycle(b.Entered, component, rule, out var selfB);
							var entered = inA.Count > 0 && inB.Count > 0;
							var count   = state.Joint + (entered ? 1 : 0);
							var guarded = state.Guarded || a.Guarded || b.Guarded;

							foreach (var (letter, forbidA, forbidB) in Advance(joint, state.ForbidA.Or(a.Forbidden), state.ForbidB.Or(b.Forbidden)))
							{
								var trail = new Trail(Spelled(letter) + (entered ? $"{{{Names(inA)}|{Names(inB)}}}" : ""), entered ? [.. inA.Where(inB.Contains)] : [], state.Trail);

								if (selfA && selfB && Survives(a.After, b.After, forbidA, forbidB))
									return (3, guarded, Written(trail), JointOf(trail));

								if (!seen.Add(new Seen(a.After, forbidA, b.After, forbidB, Math.Min(count, 2))))
									continue;

								if (seen.Count > States)
								{
									cut = true;

									return best is { } cutShort ? (cutShort.Level, cutShort.Guarded, Written(cutShort.Trail), JointOf(cutShort.Trail)) : null;
								}

								if (count >= 1 && (best is null || Math.Min(count, 2) > best.Value.Level) && Survives(a.After, b.After, forbidA, forbidB))
									best = (Math.Min(count, 2), guarded, trail);

								next.Add(new State(a.After, forbidA, b.After, forbidB, count, guarded, trail));
							}
						}
					}
				}

				frontier = next;
			}

			deep = frontier.Count > 0;

			return best is { } reached ? (reached.Level, reached.Guarded, Written(reached.Trail), JointOf(reached.Trail)) : null;
		}

		/// <summary>The rules of the cycle among those entered, and whether the rule itself is one.</summary>
		List<RuleSymbol> InCycle(RuleSymbol[] entered, int component, RuleSymbol rule, out bool self)
		{
			var inside = new List<RuleSymbol>();

			self = false;

			foreach (var one in entered)
				if (_component.TryGetValue(one, out var at) && at == component)
				{
					inside.Add(one);
					self |= ReferenceEquals(one, rule) || one.Equals(rule);
				}

			return inside;
		}

		static string Names(List<RuleSymbol> rules)
		{
			const int most = 2;

			var names = rules.Select(static rule => rule.Name).Distinct().Take(most + 1).ToList();

			return names.Count > most ? string.Join(",", names.Take(most)) + ",..." : string.Join(",", names);
		}

		/// <summary>The rules both sides entered at each joint entry of the witness, in order, the entries where they entered none left out.</summary>
		static IReadOnlyList<IReadOnlyList<RuleSymbol>> JointOf(Trail? trail)
		{
			var joint = new List<IReadOnlyList<RuleSymbol>>();

			for (var at = trail; at is not null; at = at.Before)
				if (at.Both.Length > 0)
					joint.Add(at.Both);

			joint.Reverse();

			return joint;
		}

		static string Written(Trail? trail)
		{
			var letters = new List<string>();

			for (var at = trail; at is not null; at = at.Before)
				letters.Add(at.Letter);

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

				foreach (var b in Expand(second))
				{
					if (b.Letter is null)
						continue;

					var joint = Meet(a.Letter, b.Letter);

					if (!joint.Nothing && Advance(joint, forbidA.Or(a.Forbidden), forbidB.Or(b.Forbidden)).Count > 0)
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

			var items = forbidA.Items.Select(static item => (Side: 0, Item: item)).Concat(forbidB.Items.Select(static item => (Side: 1, Item: item))).ToArray();

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

			var steps = new List<Step>();

			Expand(rest, null, false, null, Forbid.None, steps, 0, []);

			return _expanded[rest] = [.. steps];
		}

		void Expand(Rest? rest, Entry? entered, bool guarded, FirstSets.First? excluded, Forbid forbidden, List<Step> into, int nested, HashSet<Rest> looping)
		{
			if (nested > Nested)
			{
				_callsCut++;

				return;
			}

			if (rest is null)
			{
				into.Add(new Step(null, null, Entered(entered), guarded, forbidden));

				return;
			}

			var tail = rest.Tail;

			if (rest.Head is RuleSymbol)
			{
				Expand(tail, entered, guarded, excluded, forbidden, into, nested + 1, looping);

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
				Expand(next, entered, guarded, excluded, forbidden, into, nested + 1, looping);
			}

			switch ((Node)rest.Head)
			{
				case Node.Literal { Text.Length: 0 }:
					Then(tail);

					break;

				case Node.Literal literal:
				{
					var first = literal.IgnoreCase
						? FirstSets.First.Chars(CaseFold.Ranges(literal.Text[0]))
						: FirstSets.First.Chars([new CharRange(literal.Text[0], literal.Text[0])]);

					Letter(first, literal.Text.Length == 1 ? tail : Push(Suffix(literal), tail));

					break;
				}

				case Node.Element element:
					Letter(FirstSets.OfElement(element), tail);

					break;

				case Node.Sequence(var parts):
				{
					var next = tail;

					for (var at = parts.Count - 1; at >= 0; at--)
						next = Push(parts[at], next);

					Then(next);

					break;
				}

				case Node.Choice(var alternatives):
					foreach (var alternative in alternatives)
						Then(Push(alternative, tail));

					break;

				case Node.Repeat { Max: 0 }:
					Then(tail);

					break;

				case Node.Repeat { Max: 1 } option:
					Then(Push(option.Body, tail));

					if (option.Min == 0)
						Then(tail);

					break;

				case Node.Repeat or Node.Counted:
				{
					// A repetition whose body can read nothing comes back to itself without a letter.
					if (!looping.Add(rest))
						break;

					var body = rest.Head is Node.Repeat repeat ? repeat.Body : ((Node.Counted)rest.Head).Body;
					var some = rest.Head is Node.Repeat { Min: > 0 } || rest.Head is Node.Counted { Min: CountBound.Items(> 0) };

					Then(Push(body, Push(Starred((Node)rest.Head), tail)));

					if (!some)
						Then(tail);

					looping.Remove(rest);

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

					Expand(tail, entered, guarded, more, forbidden.Or(Forbid.Of(refusal.Sequences.Select(static sequence => sequence * 8))), into, nested + 1, looping);

					break;
				}

				case Node.Guard:
					Expand(tail, entered, true, excluded, forbidden, into, nested + 1, looping);

					break;

				case Node.Atomic(var body):
					Then(Push(body, tail));

					break;

				case Node.Capture(_, var body):
					Then(Push(body, tail));

					break;

				case Node.Marked(var body, _):
					Then(Push(body, tail));

					break;

				case Node.Construct(var body, _):
					Then(Push(body, tail));

					break;

				case Node.Bind(_, var body, _):
					Then(Push(body, tail));

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

					Expand(Push(callee, Push(called, tail)), now, guarded, excluded, forbidden, into, nested + 1, looping);

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

		static RuleSymbol[] Entered(Entry? entered)
		{
			if (entered is null)
				return [];

			var rules = new List<RuleSymbol>();

			for (var at = entered; at is not null; at = at.Before)
				if (!rules.Contains(at.Rule))
					rules.Add(at.Rule);

			rules.Reverse();

			return [.. rules];
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
		static FirstSets.First Meet(FirstSets.First one, FirstSets.First other)
		{
			if (one.Ends || other.Ends)
				return one.Ends && other.Ends ? FirstSets.First.End : FirstSets.First.None;

			return one.And(other);
		}

		/// <summary>What one letter reads that another does not.</summary>
		static FirstSets.First Minus(FirstSets.First one, FirstSets.First other)
		{
			if (other.Anything || one.Nothing)
				return FirstSets.First.None;

			if (one.Ends || other.Ends)
				return one.Ends && other.Ends ? FirstSets.First.None : one;

			if (other.Nothing)
				return one;

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
