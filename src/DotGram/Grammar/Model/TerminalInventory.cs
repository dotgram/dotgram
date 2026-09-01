using System;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>A literal, told apart by what it says and by whether its case matters.</summary>
using Text = (string Text, bool IgnoreCase);

/// <summary>
/// What a grammar's terminals are, once its lexical half is told from its syntactic one:
/// the patterns a lexical machine recognizes, and the kinds it emits.
/// </summary>
/// <remarks>
/// <para>
/// A pure function of the graph. It emits nothing and rewrites nothing.
/// `docs/lexical-adt-design.md` is the design it belongs to and carries the measurements
/// that justify it.
/// </para>
/// <para>
/// <b>The boundary is a reference, not a file.</b> A rule is syntactic when it carries
/// trivia (§4.5) and lexical when it does not, and a pattern is a call that crosses from
/// the first to the second. Nothing has to be declared: an author who wrote
/// <c>namespace Lexical { trivia = none }</c> has already drawn the line, and one whose
/// grammar has no trivia at all has said the whole thing is lexical — which is what a URL
/// grammar is, and it gets no split and no cost.
/// </para>
/// <para>
/// <b>A kind is a set of patterns, not a pattern.</b> This is the thing the first version
/// got wrong and it is worth stating plainly: <c>SELECT</c> is matched by the keyword
/// <em>and</em> by <c>RegularIdentifier</c>; <c>0</c> by <c>Digits</c> <em>and</em> by
/// <c>UnsignedNumericLiteral</c>; <c>'x'</c> by <c>QuotedString</c> <em>and</em> by
/// <c>CharacterStringLiteral</c>. A lexer forced to answer with one of them would make
/// every syntactic position that wanted the other stop reading, and the first version
/// therefore refused three grammars that had nothing wrong with them. So a kind is the
/// whole set, the test for a pattern is "the kind's set holds it" — a set of kinds,
/// computed here and lowered to a range test — and nothing is refused.
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
		IReadOnlyList<Pattern> patterns,
		IReadOnlyList<Kind> kinds,
		IReadOnlyList<Named> named,
		IReadOnlyList<string> blocked)
	{
		Applies  = applies;
		Patterns = patterns;
		Kinds    = kinds;
		Sets     = named;
		Blocked  = blocked;
	}

	/// <summary>Whether the grammar has a lexical half to separate at all.</summary>
	/// <remarks>
	/// False for a scannerless grammar — one where no rule carries trivia. There is no
	/// boundary to find, and the character machine is the right machine.
	/// </remarks>
	public bool Applies { get; }

	/// <summary>What the lexical machine has to recognize.</summary>
	public IReadOnlyList<Pattern> Patterns { get; }

	/// <summary>What it emits, in kind order. <see cref="Kind.Number"/> is one-based.</summary>
	public IReadOnlyList<Kind> Kinds { get; }

	/// <summary>
	/// The rules that are a set of terminals rather than a terminal, as ranges of kinds.
	/// </summary>
	/// <remarks>
	/// A rule written as a choice of literals — <c>TruthValue</c>, <c>CompOp</c>,
	/// <c>Reserved</c>, <c>Keyword</c> — recognizes nothing its literals do not recognize
	/// already. Over integers it is a range test, which is what turns
	/// <c>?!Reserved &amp; RegularIdentifier</c> from a fifty-six-way lookahead at every
	/// identifier into a subtraction and a comparison.
	/// </remarks>
	public IReadOnlyList<Named> Sets { get; }

	/// <summary>What could not be numbered, empty where nothing stood in the way.</summary>
	public IReadOnlyList<string> Blocked { get; }

	/// <summary>The kinds whose set holds this pattern — what a syntactic position tests.</summary>
	public IReadOnlyList<Group> KindsOf(Pattern pattern)
	{
		_of ??= Indexed();

		return pattern is not null && _of.TryGetValue(pattern, out var ranges) ? ranges : [];
	}

	/// <summary>The pattern a literal is, or null where the grammar has no such terminal.</summary>
	public Pattern? PatternOf(string text, bool ignoreCase)
	{
		_literals ??= Patterns
			.Where(one => one is not Pattern.Class)
			.ToDictionary(Spelling, one => one);

		return _literals.TryGetValue((text, ignoreCase), out var pattern) ? pattern : null;
	}

	/// <summary>The pattern a crossing into a rule is, or null where the rule is not one.</summary>
	public Pattern? PatternOf(RuleSymbol rule)
	{
		_classes ??= Patterns.OfType<Pattern.Class>().ToDictionary(one => one.Rule, one => (Pattern)one);

		return rule is not null && _classes.TryGetValue(rule, out var pattern) ? pattern : null;
	}

	/// <summary>The ranges a rule that is a set of terminals occupies, or null.</summary>
	public Named? SetOf(string name) => Sets.FirstOrDefault(set => set.Name == name);

	static Text Spelling(Pattern pattern) =>
		pattern switch
		{
			Pattern.Word(_, var text, var fold) => (text, fold),
			Pattern.Mark(_, var text, var fold) => (text, fold),
			_                                   => ("", false),
		};

	Dictionary<Pattern, IReadOnlyList<Group>> Indexed()
	{
		var of = new Dictionary<Pattern, List<int>>();

		foreach (var kind in Kinds)
			foreach (var pattern in kind.Matched)
			{
				if (!of.TryGetValue(pattern, out var numbers))
					of[pattern] = numbers = [];

				numbers.Add(kind.Number);
			}

		return of.ToDictionary(
			one => one.Key,
			one => (IReadOnlyList<Group>)Runs(one.Key.ToString(), one.Value));
	}

	Dictionary<Pattern, IReadOnlyList<Group>>? _of;
	Dictionary<Text, Pattern>?                 _literals;
	Dictionary<RuleSymbol, Pattern>?           _classes;

	/// <summary>One thing the lexical machine recognizes.</summary>
	/// <remarks>
	/// One base and one level of descendants. A pattern is not a kind: several patterns can
	/// match one string, and what the lexer emits is which of them did.
	/// </remarks>
	public abstract record Pattern(int Index)
	{
		/// <summary>A literal every character of which continues a word (§4.6) — a keyword.</summary>
		public sealed record Word(int Index, string Text, bool IgnoreCase) : Pattern(Index)
		{
			public override string ToString() => $"\"{Text}\"" + (IgnoreCase ? "i" : "");
		}

		/// <summary>A literal that is not a word: a bracket, an operator, punctuation.</summary>
		public sealed record Mark(int Index, string Text, bool IgnoreCase) : Pattern(Index)
		{
			public override string ToString() =>
				(Text.Length == 1 ? CharRange.Quote(Text[0]) : $"\"{Text}\"") + (IgnoreCase ? "i" : "");
		}

		/// <summary>A class of strings — the crossing into a rule that carries no trivia.</summary>
		public sealed record Class(int Index, RuleSymbol Rule) : Pattern(Index)
		{
			public override string ToString() => Rule.Name;
		}
	}

	/// <summary>One thing the lexical machine emits: the patterns that matched.</summary>
	/// <remarks>
	/// The set and not one of it. <c>SELECT</c> is a keyword and an identifier at once, and
	/// which of the two a syntactic position wanted is that position's business.
	/// </remarks>
	public sealed record Kind(int Number, IReadOnlyList<Pattern> Matched)
	{
		public override string ToString() => $"{Number} {{{string.Join(", ", Matched)}}}";
	}

	/// <summary>A rule that is a set of terminals, and the runs of kinds it comes to.</summary>
	public sealed record Named(string Name, IReadOnlyList<Group> Ranges)
	{
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

	/// <summary>The runs a set of numbers comes to.</summary>
	static List<Group> Runs(string name, IEnumerable<int> numbers)
	{
		var runs   = new List<Group>();
		var sorted = numbers.Distinct().OrderBy(number => number).ToList();

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

	sealed class Walker(RecognitionGraph graph)
	{
		readonly List<Text>       _words   = [];
		readonly List<Text>       _marks   = [];
		readonly List<RuleSymbol> _classes = [];
		readonly HashSet<string>  _seen    = [];
		readonly HashSet<string>  _refused = [];
		readonly List<string>     _reasons = [];
		readonly List<(string Name, List<Text> Members)> _sets = [];

		/// <summary>
		/// Everything `trivia` and `wordboundary` are made of, which no walk may enter.
		/// </summary>
		/// <remarks>
		/// Both are ordinary rules (§4.5, §4.6) declared in the same spaced namespace as the
		/// syntax, so without this they would be walked like syntax and whitespace would come
		/// back as a keyword. The closure and not just the two roots: `trivia =
		/// { (Whitespace | LineComment | BlockComment)* }` is three more rules, and each of
		/// them carries a trivia entry of its own.
		/// </remarks>
		readonly HashSet<RuleSymbol> _lexical = Closure(graph);

		static HashSet<RuleSymbol> Closure(RecognitionGraph graph)
		{
			var roots = new List<RuleSymbol>(
				graph.Trivia.Values.OfType<Node.Call>().Select(call => call.Rule));

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

				case Node.Element element:
					Elemental(element, owner);

					return;

				case Node.Behind:
				case Node.Guard:
				case Node.Empty:
					return;

				case Node.Call(var called, _):
					// Three kinds of callee and only one is a pattern. What trivia and the
					// word boundary are made of is the lexer's already; syntax calling syntax
					// is walked on the callee's own turn, since every rule that carries trivia
					// gets one; and what is left is the crossing.
					if (_lexical.Contains(called) || graph.Trivia.ContainsKey(called))
						return;

					if (_seen.Add("class " + called.Namespace + "." + called.Name))
						_classes.Add(called);

					return;

				case Node.External(var name):
					Refuse($"an external recognizer in syntactic position: @{name} in {owner.Name}");

					return;

				case Node.Choice(var alternatives):
					foreach (var alternative in alternatives)
						Walk(alternative, owner);

					return;

				// §4.6 leaves a word wearing its boundary — `?<!boundary & literal &
				// ?!boundary`, or the tail alone where the boundary rule is not one element —
				// and normalization then flattens that triple into whatever sequence
				// surrounded it. So the shape is looked for at every position rather than as a
				// whole body, which is what a first attempt got wrong: half of SQL's keywords
				// came back as punctuation, the half that stood beside something else.
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
				// `?!Reserved` names every reserved word, and those are terminals whether or
				// not this particular reading consumes one. Except the boundary's own, which
				// is §4.6 machinery and stands alone wherever the weaving found no `Behind`.
				case Node.Lookahead(_, var seen):
					if (!IsBoundary(node))
						Walk(seen, owner);

					return;
			}
		}

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

		/// <summary>
		/// A character class in syntactic position, and which of its characters are terminals.
		/// </summary>
		/// <remarks>
		/// A positive one is its characters, each a pattern of its own — <c>['+' | '-']</c> is
		/// two, and a grammar writes that everywhere. A negated one names nothing new:
		/// <c>[^ '(' | ')']</c> is "one item that is not a bracket", and over kinds it is the
		/// same sentence about a wider alphabet — so what it <em>excludes</em> has to be
		/// numbered and it itself does not. Bounded either way, because a Unicode category
		/// names thousands, and thousands of kinds is the character machine wearing a hat.
		/// </remarks>
		void Elemental(Node.Element element, RuleSymbol owner)
		{
			var wanted = FirstSets.OfElement(element with { IsNegated = false });

			if (!wanted.IsKnown)
			{
				Refuse($"a character class whose members cannot be listed: {element} in {owner.Name}");

				return;
			}

			var count = 0;

			foreach (var range in wanted.Ranges)
				count += range.To - range.From + 1;

			if (count > Named)
			{
				Refuse($"a character class of {count} characters in syntactic position: " +
					$"{element} in {owner.Name}");

				return;
			}

			foreach (var range in wanted.Ranges)
				for (var c = range.From; ; c++)
				{
					Take(_marks, c.ToString(), false);

					if (c == range.To)
						break;
				}
		}

		/// <summary>How many characters a class in syntactic position may name.</summary>
		const int Named = 8;

		void Take(List<Text> into, string text, bool ignoreCase)
		{
			if (_seen.Add((into == _words ? "word " : "mark ") + (ignoreCase ? "i" : "") + text))
				into.Add((text, ignoreCase));
		}

		void Refuse(string reason)
		{
			if (_refused.Add(reason))
				_reasons.Add(reason);
		}

		/// <summary>
		/// The rules that are a set of terminals rather than a terminal of their own.
		/// </summary>
		/// <remarks>
		/// Only where every one of its literals is already a terminal. A rule listing a word
		/// that no syntax ever writes has a string in it that nothing else numbers, and
		/// promoting it here would invent a terminal out of a lookahead; such a rule stays
		/// whatever it was.
		/// </remarks>
		void Collect()
		{
			var known = new HashSet<Text>(_words.Concat(_marks));

			foreach (var rule in graph.Rules)
			{
				if (_lexical.Contains(rule) || !graph.Bodies.TryGetValue(rule, out var body))
					continue;

				if (Choices(body) is not { Count: > 1 } literals)
					continue;

				var members = new List<Text>(literals.Count);

				foreach (var literal in literals)
					if (known.Contains((literal.Text, literal.IgnoreCase)))
						members.Add((literal.Text, literal.IgnoreCase));
					else
						goto next;

				_sets.Add((rule.Name, members));

				next: ;
			}

			var named = new HashSet<string>(_sets.Select(one => one.Name), StringComparer.Ordinal);

			_classes.RemoveAll(one => named.Contains(one.Name));
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

		/// <summary>The one literal an alternative is, boundary and all — and nothing looser.</summary>
		static Node.Literal? Only(Node node) =>
			node switch
			{
				Node.Literal literal                  => literal,
				Node.Sequence([Node.Literal literal]) => literal,
				Node.Sequence([Node.Literal literal, Node.Lookahead(false, _)]) => literal,
				Node.Sequence([Node.Behind, Node.Literal literal, Node.Lookahead(false, _)]) => literal,
				_                                     => null,
			};

		/// <summary>
		/// Orders patterns so that as many named sets as possible are one run of kinds.
		/// </summary>
		/// <remarks>
		/// Greedy and laminar: take the largest set that divides what is left, put its members
		/// before the rest, and go on inside each half. Every set nested in another stays
		/// whole, and so does every set disjoint from the rest. Two that cross — sharing a
		/// member with neither containing the other — cannot both be one run under any order,
		/// and one of them ends up as two ranges, which is two comparisons and still not a
		/// fifty-way choice.
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

		public TerminalInventory Gathered()
		{
			Collect();

			var sets  = _sets.Select(one => new HashSet<Text>(one.Members)).ToList();
			var words = Laminar(_words, sets);
			var marks = Laminar(_marks, sets);

			var patterns = new List<Pattern>(words.Count + marks.Count + _classes.Count);
			var of       = new Dictionary<Text, Pattern>();

			foreach (var word in words)
				patterns.Add(of[word] = new Pattern.Word(patterns.Count, word.Text, word.IgnoreCase));

			foreach (var mark in marks)
				patterns.Add(of[mark] = new Pattern.Mark(patterns.Count, mark.Text, mark.IgnoreCase));

			var classes = new List<Pattern.Class>(_classes.Count);

			foreach (var rule in _classes)
			{
				var one = new Pattern.Class(patterns.Count, rule);

				patterns.Add(one);
				classes.Add(one);
			}

			// One machine over all of them at once, and its accepting states are the kinds.
			// Exactly, and not from witnesses: which sets of patterns some string makes
			// accept together is what a subset construction answers, and answering it any
			// other way is either approximate or a second implementation of the lexer.
			var shapes = patterns.Select(one => Shape(one)).ToList();
			var found  = shapes.Any(one => one is null)
				? null
				: LexicalAutomaton.Sets(graph, [.. shapes!], _reasons);

			if (found is null)
			{
				Refuse("the patterns are not all regular, so they cannot be read together");

				return new TerminalInventory(true, patterns, [], [], _reasons);
			}

			// Numbered by the patterns they hold and not by the order the machine met them.
			// The laminar ordering above put the patterns where a named set is one run of
			// them, and that only survives into the kinds if the kinds follow the patterns:
			// a word's kind holds that word and nothing earlier, so sorting by the lowest
			// pattern in the set puts the word kinds in word order and the rest after them.
			var kinds = found
				.OrderBy(set => set[0])
				.ThenBy(set => set.Count)
				.ThenBy(set => string.Join(",", set))
				.Select((set, at) => new Kind(at + 1, [.. set.Select(one => patterns[one])]))
				.ToList();

			var counted = new TerminalInventory(true, patterns, kinds, [], _reasons);

			var named = _sets
				.Select(one => new Named(
					one.Name,
					Runs(one.Name, one.Members
						.Where(of.ContainsKey)
						.SelectMany(member => counted.KindsOf(of[member]))
						.SelectMany(range => Enumerable.Range(range.From, range.Count)))))
				.ToList();

			return new TerminalInventory(true, patterns, kinds, named, _reasons);
		}

		/// <summary>The node a pattern recognizes, as the automaton needs to read it.</summary>
		Node? Shape(Pattern pattern) =>
			pattern switch
			{
				Pattern.Word(_, var word, var fold) => new Node.Literal(word) { IgnoreCase = fold },
				Pattern.Mark(_, var mark, var fold) => new Node.Literal(mark) { IgnoreCase = fold },
				Pattern.Class(_, var rule)          => graph.Bodies.TryGetValue(rule, out var body) ? body : null,
				_                                   => null,
			};

		/// <summary>
		/// The sets of classes that can match one string at once, over-approximated.
		/// </summary>
		/// <remarks>
		/// Two classes overlap where either one's shortest string is accepted by the other —
		/// which catches <c>Digits</c> against <c>UnsignedNumericLiteral</c> (<c>"0"</c>),
		/// <c>QuotedString</c> against <c>CharacterStringLiteral</c> (<c>"''"</c>) and
		/// <c>Word</c> against <c>TypeName</c> (<c>"A"</c>), and does not cry wolf over
		/// <c>Decimals</c> against <c>Number</c>, where the shortest is <c>"0m"</c> and only
		/// one of them takes it. Cliques of the resulting graph are the candidate sets, and
		/// the graphs are two or three edges wide, so enumerating them is nothing.
		/// </remarks>
		List<List<Pattern>> Cliques(List<Pattern.Class> classes)
		{
			var overlaps = classes.ToDictionary(one => one, _ => new HashSet<Pattern.Class>());

			var shortest = classes.ToDictionary(
				one => one,
				one => graph.Bodies.TryGetValue(one.Rule, out var body)
					? Language.Shortest(graph, body)
					: null);

			for (var i = 0; i < classes.Count; i++)
				for (var j = i + 1; j < classes.Count; j++)
					if (shortest[classes[j]] is { } later  && Language.Accepts(graph, classes[i].Rule, later) ||
						shortest[classes[i]] is { } sooner && Language.Accepts(graph, classes[j].Rule, sooner))
					{
						overlaps[classes[i]].Add(classes[j]);
						overlaps[classes[j]].Add(classes[i]);
					}

			var found = new List<List<Pattern>>();

			foreach (var one in classes)
			{
				found.Add([one]);

				foreach (var other in overlaps[one])
					if (other.Index > one.Index)
						found.Add([one, other]);
			}

			return found;
		}
	}
}
