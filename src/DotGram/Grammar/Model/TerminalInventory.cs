using System;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

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
		IReadOnlyList<string> blocked)
	{
		Applies   = applies;
		Terminals = terminals;
		Groups    = groups;
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

	/// <summary>The groups, each a contiguous range of kinds.</summary>
	public IReadOnlyList<Group> Groups { get; }

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
			return new TerminalInventory(false, [], [], []);

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
		readonly List<(string Text, bool IgnoreCase)> _words = [];
		readonly List<(string Text, bool IgnoreCase)> _marks = [];
		readonly List<RuleSymbol>                     _classes = [];
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

		void Take(List<(string, bool)> into, string text, bool ignoreCase)
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
		/// A class whose strings are words already numbered wants a range, not a kind.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <c>ExpressionLanguage</c> has the shape and it is the one that matters:
		/// <c>Keyword = ("as" | "bool" | … | "while") &amp; ?![\p{L} | \p{Nd} | '_']</c> sits
		/// in a lexical namespace and is reached only through <c>Name = ?!Keyword &amp;
		/// Word</c>, while every one of those words also stands in the syntax as a literal of
		/// its own. Give <c>Keyword</c> a kind and the lexer has to decide whether <c>if</c>
		/// is the word or the class, and either answer breaks the other reading.
		/// </para>
		/// <para>
		/// The answer the design wants is that it is neither a kind nor a lookahead: it is
		/// the range those words already occupy, and <c>?!Keyword &amp; Word</c> becomes one
		/// set difference over integers. Recognizing the shape is this pass's job; doing
		/// something about it is the next one's.
		/// </para>
		/// <para>
		/// <c>SqlStandard92</c> does not have it, and the reason is worth reading: its
		/// <c>Reserved</c> is declared where trivia is *not* empty, so it is syntax, and its
		/// words are walked into the word group like any others. The same list, one namespace
		/// apart, is two different problems.
		/// </para>
		/// </remarks>
		void Overlapping(RuleSymbol called)
		{
			if (!graph.Bodies.TryGetValue(called, out var body) || Choices(body) is not { } texts)
				return;

			var words = new HashSet<string>(_words.Select(word => word.Text), StringComparer.Ordinal);

			if (texts.Count == 0 || !texts.All(words.Contains))
				return;

			Block(
				$"a class that is a set of words already numbered: {called.Name} is " +
				$"{texts.Count} of them, {string.Join(", ", texts.Take(3))} among the rest, " +
				"so it wants a range rather than a kind of its own");
		}

		/// <summary>The texts of a choice of plain literals, however it is wrapped.</summary>
		static List<string>? Choices(Node node)
		{
			switch (node)
			{
				case Node.Choice(var alternatives):
				{
					var texts = new List<string>(alternatives.Count);

					foreach (var alternative in alternatives)
						if (Only(alternative) is { } literal)
							texts.Add(literal.Text);
						else
							return null;

					return texts;
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
			foreach (var called in _classes)
				Overlapping(called);

			var terminals = new List<Terminal>(_words.Count + _marks.Count + _classes.Count);
			var groups    = new List<Group>(3);
			var kind      = 1;

			// Words first, then marks, then classes. The order is what a set difference wants
			// later — "a word that is not one of these" is one range against another — and it
			// costs nothing to choose it now rather than to renumber for it afterwards.
			kind = Run(groups, terminals, "Word", kind, _words.Count,
				at => new Terminal.Word(at.Kind, _words[at.Index].Text, _words[at.Index].IgnoreCase));

			kind = Run(groups, terminals, "Mark", kind, _marks.Count,
				at => new Terminal.Mark(at.Kind, _marks[at.Index].Text, _marks[at.Index].IgnoreCase));

			Run(groups, terminals, "Class", kind, _classes.Count,
				at => new Terminal.Class(at.Kind, _classes[at.Index]));

			return new TerminalInventory(true, terminals, groups, _reasons);
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
