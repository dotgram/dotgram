using System;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>A literal, told apart by what it says and by whether its case matters.</summary>
using Text = (string Text, bool IgnoreCase);

/// <summary>
/// What the terminals of a grammar are, once its lexical half is told from its syntactic
/// one — and where that telling fails.
/// </summary>
/// <remarks>
/// <para>
/// A pure function of the graph. It emits nothing and rewrites nothing: it answers what a
/// lexical machine would have to recognize, what numbers those results would carry, and
/// what in this particular grammar stands in the way. `docs/lexical-adt-design.md` is the
/// design it belongs to and carries the measurements that justify it.
/// </para>
/// <para>
/// <b>The boundary is a reference, not a file.</b> A rule is syntactic when it carries
/// trivia (§4.5) and lexical when it does not, and what makes a terminal is a call that
/// crosses from the first to the second. That is why nothing has to be declared: an author
/// who wrote <c>namespace Lexical { trivia = none }</c> has already drawn the line, and an
/// author whose grammar has no trivia at all has said the whole thing is lexical — which is
/// what a URL grammar is, and it gets no split and no cost.
/// </para>
/// <para>
/// <b>What it walks and what it does not.</b> Over every rule that carries trivia, in the
/// order the graph lists them, so the numbering is a function of the grammar's text and not
/// of which publication reached what. The same choice <see cref="ExecutionPlan"/> makes and
/// for the same reason: this decides nothing that depends on a caller.
/// </para>
/// </remarks>
public sealed class TerminalInventory
{
	TerminalInventory(
		bool applies,
		IReadOnlyList<Terminal> terminals,
		IReadOnlyList<Group> groups,
		IReadOnlyList<Named> named,
		IReadOnlyList<string> blocked)
	{
		Applies   = applies;
		Terminals = terminals;
		Groups    = groups;
		Sets      = named;
		Blocked   = blocked;
	}

	/// <summary>Whether the grammar has a lexical half to separate at all.</summary>
	/// <remarks>
	/// False for a scannerless grammar — one where no rule carries trivia. There is no
	/// boundary to find, and the character machine is the right machine.
	/// </remarks>
	public bool Applies { get; }

	/// <summary>The terminals, in kind order. <see cref="Terminal.Kind"/> is one-based.</summary>
	public IReadOnlyList<Terminal> Terminals { get; }

	/// <summary>The groups, each a contiguous range of kinds. They tile the terminals.</summary>
	public IReadOnlyList<Group> Groups { get; }

	/// <summary>
	/// The rules that are a set of terminals rather than a terminal, as ranges of kinds.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A rule written as a choice of literals — <c>TruthValue</c>, <c>CompOp</c>,
	/// <c>Reserved</c>, <c>Keyword</c> — recognizes nothing a terminal does not already
	/// recognize. It is a *set*, and over integers a set is a range test: the numbering is
	/// arranged so that as many of them as possible occupy one run each.
	/// </para>
	/// <para>
	/// This is what turns <c>?!Reserved &amp; RegularIdentifier</c> — a negative lookahead
	/// over fifty-six words, run at every identifier — into a subtraction and a comparison.
	/// A set that needs more than one range says so by carrying more than one, which is the
	/// truth about it rather than a failure: <c>SetQuantifier</c> is <c>DISTINCT | ALL</c>
	/// and <c>Quantifier</c> is <c>ALL | SOME | ANY</c>, they share a word without either
	/// containing the other, and no ordering makes both a single run.
	/// </para>
	/// </remarks>
	public IReadOnlyList<Named> Sets { get; }

	/// <summary>
	/// What stops this grammar from being split, in the words a reader would need.
	/// </summary>
	/// <remarks>
	/// Empty is the answer that matters; anything here is a shape the design has not
	/// decided yet, and naming it is the point of this pass. It is not a diagnostic: a
	/// grammar with entries here compiles exactly as it always did.
	/// </remarks>
	public IReadOnlyList<string> Blocked { get; }

	/// <summary>Works out the inventory for a graph.</summary>
	public static TerminalInventory Of(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var walker = new Walker(graph);

		if (!walker.Applies)
			return new TerminalInventory(false, [], [], [], []);

		foreach (var rule in graph.Rules)
			if (walker.IsSyntactic(rule) && graph.Bodies.TryGetValue(rule, out var body))
				walker.Walk(body, rule);

		return walker.Gathered();
	}

	/// <summary>One terminal, and what a lexical machine would have to do to recognize it.</summary>
	/// <remarks>
	/// One base and one level of descendants. The kind is the whole of the runtime
	/// representation: a word carries no payload, because the number <em>is</em> the word,
	/// and only a class needs an extent to say which of its strings this one was.
	/// </remarks>
	public abstract record Terminal(int Kind)
	{
		/// <summary>A literal every character of which continues a word (§4.6) — a keyword.</summary>
		public sealed record Word(int Kind, string Text, bool IgnoreCase) : Terminal(Kind)
		{
			public override string ToString() => $"\"{Text}\"" + (IgnoreCase ? "i" : "");
		}

		/// <summary>A literal that is not a word: a bracket, an operator, punctuation.</summary>
		public sealed record Mark(int Kind, string Text, bool IgnoreCase) : Terminal(Kind)
		{
			public override string ToString() =>
				(Text.Length == 1 ? CharRange.Quote(Text[0]) : $"\"{Text}\"") + (IgnoreCase ? "i" : "");
		}

		/// <summary>A class of strings — the crossing into a rule that carries no trivia.</summary>
		public sealed record Class(int Kind, RuleSymbol Rule) : Terminal(Kind)
		{
			public override string ToString() => Rule.Name;
		}
	}

	/// <summary>A rule that is a set of terminals, and the runs of kinds it comes to.</summary>
	public sealed record Named(string Name, IReadOnlyList<Group> Ranges)
	{
		/// <summary>How many terminals it holds.</summary>
		public int Count => Ranges.Sum(range => range.Count);

		public override string ToString() =>
			$"{Name} = {string.Join(", ", Ranges.Select(range => $"{range.From}..{range.To}"))}";
	}

	/// <summary>A contiguous run of kinds, which is what makes membership a range test.</summary>
	/// <remarks>
	/// <c>(uint)(kind - From) &lt;= (uint)(To - From)</c>: one subtract and one compare, no
	/// memory touched, whatever the alphabet grows to. It is also what makes the sum type
	/// free — a variant is a range and a tag is the number itself.
	/// </remarks>
	public readonly record struct Group(string Name, int From, int To)
	{
		public int Count => To - From + 1;

		public override string ToString() => $"{Name} {From}..{To}";
	}

	/// <summary>
	/// The walk, and the three lists it fills.
	/// </summary>
	/// <remarks>
	/// Written as a class rather than as a fold because the answer is three growing lists
	/// and a visited set, and threading four accumulators through a tree is worse to read
	/// than the fields.
	/// </remarks>
	sealed class Walker(RecognitionGraph graph)
	{

		readonly List<Text> _words = [];
		readonly List<Text> _marks = [];
		readonly List<RuleSymbol>                     _classes = [];
		readonly List<(string Name, List<Text> Members)> _sets = [];
		readonly HashSet<string>                      _seen = [];
		readonly HashSet<string>                      _blocked = [];
		readonly List<string>                         _reasons = [];

		/// <summary>
		/// Everything `trivia` and `wordboundary` are made of, which no walk may enter.
		/// </summary>
		/// <remarks>
		/// Both are ordinary rules (§4.5, §4.6) declared in the same spaced namespace as
		/// everything else, so without this they would be walked like syntax and whitespace
		/// would come back as a keyword. The closure and not just the two: `trivia =
		/// { (Whitespace | LineComment | BlockComment)* }` is three more rules, and each of
		/// them carries a trivia entry of its own.
		/// </remarks>
		readonly HashSet<RuleSymbol> _lexical = Closure(graph);

		static HashSet<RuleSymbol> Closure(RecognitionGraph graph)
		{
			var roots = new List<RuleSymbol>(graph.Trivia.Values.OfType<Node.Call>().Select(call => call.Rule));

			foreach (var rule in graph.Rules)
				if (rule.Name == Boundary && !rule.IsBuiltIn)
					roots.Add(rule);

			var reached = new HashSet<RuleSymbol>();
			var pending = new Stack<RuleSymbol>(roots);

			while (pending.Count > 0)
			{
				var rule = pending.Pop();

				if (!reached.Add(rule) || !graph.Bodies.TryGetValue(rule, out var body))
					continue;

				foreach (var node in NodeWalk.Descendants(body))
					if (node is Node.Call(var called, _))
						pending.Push(called);
			}

			return reached;
		}

		public bool Applies => graph.Trivia.Count > 0;

		public bool IsSyntactic(RuleSymbol rule) =>
			graph.Trivia.ContainsKey(rule) && !_lexical.Contains(rule);

		public void Walk(Node node, RuleSymbol owner)
		{
			switch (node)
			{
				case Node.Literal(var text) literal:
					if (text.Length > 0)
						Take(_marks, text, literal.IgnoreCase);

					return;

				// A character class standing in syntax is a set of one-character terminals,
				// and each of them is a terminal in its own right. Bounded on purpose: a
				// negated set or a Unicode category names thousands, and thousands of kinds
				// is not an alphabet, it is the character machine with extra steps.
				case Node.Element element:
					Elemental(element, owner);

					return;

				case Node.Call(var called, _):
					// Three kinds of callee and only one of them is a terminal. What trivia
					// and the word boundary are made of is the lexer's already; syntax
					// calling syntax is walked on the callee's own turn, since every rule
					// that carries trivia gets one; and what is left is the crossing.
					if (_lexical.Contains(called) || graph.Trivia.ContainsKey(called))
						return;

					Cross(called);

					return;

				// An external recognizer is a class recognized by C# instead of by a rule
				// (§7.2), which is the same crossing by another road.
				case Node.External(var name):
					Block($"an external recognizer in syntactic position: @{name} in {owner.Name}");

					return;

				case Node.Behind:
				case Node.Guard:
				case Node.Empty:
					return;

				case Node.Choice(var alternatives):
					foreach (var alternative in alternatives)
						Walk(alternative, owner);

					return;

				// §4.6 leaves a word wearing its boundary — `?<!boundary & literal &
				// ?!boundary`, or the tail alone where the boundary rule is not one element
				// — and normalization then flattens that triple into whatever sequence
				// surrounded it. So the shape is looked for at every position rather than as
				// a whole body, which is what a first attempt got wrong: half of SQL's
				// keywords came back as punctuation, the half that stood beside something
				// else. Finding it is also how a keyword is told from a bracket without
				// asking the boundary rule anything.
				case Node.Sequence(var nodes):
					for (var i = 0; i < nodes.Count; i++)
						if (Worded(nodes, i) is var (literal, last))
						{
							Take(_words, literal.Text, literal.IgnoreCase);

							i = last;
						}
						else
						{
							Walk(nodes[i], owner);
						}

					return;

				case Node.Repeat(var repeated, _, _):
					Walk(repeated, owner);

					return;

				case Node.Atomic(var kept):
					Walk(kept, owner);

					return;

				case Node.Marked(var marked, _):
					Walk(marked, owner);

					return;

				case Node.Capture(_, var captured):
					Walk(captured, owner);

					return;

				case Node.Construct(var built, _):
					Walk(built, owner);

					return;

				// A lookahead's operand is syntax and its terminals are the same terminals —
				// `?!Reserved` names every reserved word, and those words are keywords
				// whether or not this particular reading consumes one. Except the boundary's
				// own, which is §4.6 machinery and stands alone wherever the weaving found
				// no `Behind` to pair it with.
				case Node.Lookahead(_, var seen):
					if (!IsBoundary(node))
						Walk(seen, owner);

					return;
			}
		}

		/// <summary>
		/// The literal a §4.6 weaving begins at <paramref name="at"/>, and where it ends.
		/// </summary>
		/// <remarks>
		/// The lookahead is checked against the boundary rule by name rather than taken for
		/// any negative lookahead: <c>?!Reserved</c> is one too, and the word before it is
		/// not thereby a keyword.
		/// </remarks>
		static (Node.Literal Literal, int Last)? Worded(IReadOnlyList<Node> parts, int at) =>
			parts.Count > at + 2 &&
			parts[at] is Node.Behind &&
			parts[at + 1] is Node.Literal behind &&
			IsBoundary(parts[at + 2])
				? (behind, at + 2)
				: parts.Count > at + 1 &&
					parts[at] is Node.Literal ahead &&
					IsBoundary(parts[at + 1])
					? (ahead, at + 1)
					: null;

		/// <summary>The `?!wordboundary` §4.6 weaves, as against any other refusal.</summary>
		static bool IsBoundary(Node node) =>
			node is Node.Lookahead(false, Node.Call(var rule, _)) && rule.Name == Boundary;

		const string Boundary = "wordboundary";

		void Elemental(Node.Element element, RuleSymbol owner)
		{
			var first = FirstSets.Of(element, graph);

			if (!first.IsKnown)
			{
				Block($"a character class that names no fixed set: {element} in {owner.Name}");

				return;
			}

			var count = 0;

			foreach (var range in first.Ranges)
				count += range.To - range.From + 1;

			// Eight is where a set stops being a handful of punctuation and starts being an
			// alphabet. The sets this admits are the ones a syntactic rule actually holds —
			// `['+' | '-']`, a comparison operator, a bracket pair.
			if (count > Named)
			{
				Block($"a character class of {count} characters in syntactic position: " +
					$"{element} in {owner.Name}");

				return;
			}

			foreach (var range in first.Ranges)
				for (var c = range.From; ; c++)
				{
					Take(_marks, c.ToString(), false);

					if (c == range.To)
						break;
				}
		}

		void Cross(RuleSymbol called)
		{
			if (_seen.Add("class " + Named_(called)))
				_classes.Add(called);
		}

		void Take(List<Text> into, string text, bool ignoreCase)
		{
			if (_seen.Add((into == _words ? "word " : "mark ") + (ignoreCase ? "i" : "") + text))
				into.Add((text, ignoreCase));
		}

		void Block(string reason)
		{
			if (_blocked.Add(reason))
				_reasons.Add(reason);
		}

		static string Named_(RuleSymbol rule) => rule.Namespace + "." + rule.Name;

		/// <summary>How many characters a class in syntactic position may name.</summary>
		const int Named = 8;

		/// <summary>
		/// The rules that are a set of terminals rather than a terminal of their own.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A rule written as a choice of literals recognizes nothing its literals do not
		/// recognize already — <c>TruthValue</c>, <c>CompOp</c>, <c>Quantifier</c>,
		/// <c>Reserved</c>. Over characters that is a choice and costs a choice; over
		/// integers it is a set, and a set that occupies one run of kinds is a subtraction
		/// and a comparison.
		/// </para>
		/// <para>
		/// Only where every one of its literals is already a terminal. A rule listing a word
		/// that no syntax ever writes has a string in it that nothing else numbers, and
		/// promoting it here would invent a terminal out of a lookahead; such a rule stays
		/// whatever it was.
		/// </para>
		/// <para>
		/// Both halves of the grammar are asked, and the difference between them is the point.
		/// <c>ExpressionLanguage</c>'s <c>Keyword</c> is lexical and would otherwise be a
		/// class whose strings are also terminals — <c>if</c> would have two kinds.
		/// <c>SqlStandard92</c>'s <c>Reserved</c> is syntactic and is walked into the word
		/// group already; what it needs is not a kind but the knowledge that those
		/// fifty-six words are a range.
		/// </para>
		/// </remarks>
		public void Collect()
		{
			var known = new Dictionary<Text, int>();

			for (var i = 0; i < _words.Count; i++) known[_words[i]] = i;
			for (var i = 0; i < _marks.Count; i++) known[_marks[i]] = ~i;

			foreach (var rule in graph.Rules)
			{
				if (_lexical.Contains(rule) || !graph.Bodies.TryGetValue(rule, out var body))
					continue;

				if (Choices(body) is not { Count: > 1 } literals)
					continue;

				var members = new List<Text>(literals.Count);

				foreach (var literal in literals)
					if (known.ContainsKey((literal.Text, literal.IgnoreCase)))
						members.Add((literal.Text, literal.IgnoreCase));
					else
						goto next;

				_sets.Add((rule.Name, members));

				next: ;
			}

			// A class that turned out to be a set is not a class: its strings are terminals
			// already, and giving it a kind as well would be the `if` with two kinds.
			var named = new HashSet<string>(_sets.Select(one => one.Name), StringComparer.Ordinal);

			_classes.RemoveAll(one => named.Contains(one.Name));
		}

		/// <summary>
		/// Orders terminals so that as many named sets as possible are one run of kinds.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Greedy and laminar: take the largest set that divides what is left, put its
		/// members before the rest, and go on inside each half. Every set nested in another
		/// stays whole, and every set disjoint from the rest stays whole — which is the shape
		/// almost all of them have.
		/// </para>
		/// <para>
		/// Two sets that cross — sharing a member with neither containing the other — cannot
		/// both be one run under any order, and this is where they are split rather than
		/// where the fact is hidden. <c>SetQuantifier</c> is <c>DISTINCT | ALL</c> and
		/// <c>Quantifier</c> is <c>ALL | SOME | ANY</c>: one of them ends up as two ranges,
		/// which is two comparisons and still not a fifty-way choice.
		/// </para>
		/// </remarks>
		static List<Text> Laminar(List<Text> members, IReadOnlyList<HashSet<Text>> sets)
		{
			HashSet<Text>? largest = null;

			foreach (var set in sets)
			{
				var inside = members.Count(set.Contains);

				if (inside == 0 || inside == members.Count)
					continue;

				if (largest is null || inside > members.Count(largest.Contains))
					largest = set;
			}

			if (largest is null)
				return members;

			var within  = members.Where(largest.Contains).ToList();
			var without = members.Where(one => !largest.Contains(one)).ToList();

			return [.. Laminar(within, sets), .. Laminar(without, sets)];
		}

		/// <summary>The runs of kinds a set comes to, once the numbering is settled.</summary>
		static List<Group> Runs(string name, IEnumerable<int> kinds)
		{
			var runs   = new List<Group>();
			var sorted = kinds.OrderBy(kind => kind).ToList();

			for (var i = 0; i < sorted.Count; )
			{
				var j = i;

				while (j + 1 < sorted.Count && sorted[j + 1] == sorted[j] + 1)
					j++;

				runs.Add(new Group(name, sorted[i], sorted[j]));

				i = j + 1;
			}

			return runs;
		}

		/// <summary>The literals of a choice of plain literals, however it is wrapped.</summary>
		static List<Node.Literal>? Choices(Node node)
		{
			switch (node)
			{
				case Node.Choice(var alternatives):
				{
					var literals = new List<Node.Literal>(alternatives.Count);

					foreach (var alternative in alternatives)
						if (Only(alternative) is { } literal)
							literals.Add(literal);
						else
							return null;

					return literals;
				}

				// A hand-written boundary — `(… | …) & ?!\p{L}` — wraps the choice without
				// changing what it accepts, and §4.6's woven one does the same.
				case Node.Sequence(var parts):
					return parts.Count > 0 ? Choices(parts[0]) : null;

				case Node.Atomic(var kept):    return Choices(kept);
				case Node.Marked(var kept, _): return Choices(kept);

				default: return null;
			}
		}

		/// <summary>
		/// The one literal an alternative is, boundary and all — and nothing looser.
		/// </summary>
		/// <remarks>
		/// A first attempt admitted <c>[Literal, anything]</c>, which would have called
		/// <c>"cast" &amp; '('</c> a bare word. Only the two shapes §4.6 can leave behind and
		/// the literal alone are taken.
		/// </remarks>
		static Node.Literal? Only(Node node) =>
			node switch
			{
				Node.Literal literal                                  => literal,
				Node.Sequence([Node.Literal literal])                 => literal,
				Node.Sequence([Node.Literal literal, Node.Lookahead(false, _)])            => literal,
				Node.Sequence([Node.Behind, Node.Literal literal, Node.Lookahead(false, _)]) => literal,
				_                                                     => null,
			};

		public TerminalInventory Gathered()
		{
			Collect();

			// Each group ordered against the sets that live in it. A set spanning two groups
			// cannot be one run whatever is done inside either, so nothing pretends otherwise.
			var sets  = _sets.Select(one => new HashSet<Text>(one.Members)).ToList();
			var words = Laminar(_words, sets);
			var marks = Laminar(_marks, sets);

			var terminals = new List<Terminal>(words.Count + marks.Count + _classes.Count);
			var groups    = new List<Group>(3);
			var kind      = 1;

			// Words first, then marks, then classes. The order is what a set difference wants
			// — "a word that is not one of these" is one range against another — and it costs
			// nothing to choose it now rather than to renumber for it afterwards.
			kind = Run(groups, terminals, "Word", kind, words.Count,
				at => new Terminal.Word(at.Kind, words[at.Index].Text, words[at.Index].IgnoreCase));

			kind = Run(groups, terminals, "Mark", kind, marks.Count,
				at => new Terminal.Mark(at.Kind, marks[at.Index].Text, marks[at.Index].IgnoreCase));

			Run(groups, terminals, "Class", kind, _classes.Count,
				at => new Terminal.Class(at.Kind, _classes[at.Index]));

			var of = new Dictionary<Text, int>();

			for (var i = 0; i < words.Count; i++) of[words[i]] = i + 1;
			for (var i = 0; i < marks.Count; i++) of[marks[i]] = words.Count + i + 1;

            var named = _sets
                .Select(one => new Named(one.Name, Runs(one.Name, one.Members.Select(m => of[m]))))
                .ToList();

			return new TerminalInventory(true, terminals, groups, named, _reasons);
		}

		static int Run(
			List<Group> groups,
			List<Terminal> into,
			string name,
			int from,
			int count,
			Func<(int Kind, int Index), Terminal> make)
		{
			if (count == 0)
				return from;

			for (var i = 0; i < count; i++)
				into.Add(make((from + i, i)));

			groups.Add(new Group(name, from, from + count - 1));

			return from + count;
		}
	}
}
