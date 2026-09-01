using System;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// A grammar cut in two: a lexical machine over characters, and the same syntactic machine
/// over the integer kinds it produces.
/// </summary>
/// <remarks>
/// <para>
/// The rewrite is the whole of it, and it is small because the machine underneath does not
/// change. <see cref="RecognitionGraph"/> is a machine over an input alphabet;
/// <see cref="CharRange"/> is <c>(char From, char To)</c>, so the alphabet is sixteen bits
/// wide and a token kind fits in it. Every analysis the compiler already has —
/// <c>FirstSets</c>, <c>FollowSets</c>, <c>Determinism</c>, <c>Doors</c>, <c>Predictive</c>,
/// <c>Dispatchable</c> — runs over the result unchanged, because it never knew it was
/// looking at characters.
/// </para>
/// <para>
/// What the rewrite does to each syntactic rule:
/// </para>
/// <code>
///   "SELECT"                 -> the kinds whose set holds that keyword
///   ?&lt;!b &amp; "SELECT" &amp; ?!b   -> the same; the boundary was the lexer's all along
///   ['+' | '-']              -> the kinds of those two marks
///   RegularIdentifier        -> every kind whose set holds it — every keyword's included
///   Reserved                 -> the range those fifty-six words occupy
///   trivia                   -> nothing; whitespace never reaches this machine
/// </code>
/// <para>
/// The fourth line took two attempts. A pattern is not a kind: <c>SELECT</c> is a keyword
/// <em>and</em> an identifier, so a position that wanted an identifier has to accept the
/// keyword's kind — which is what makes a non-reserved word usable as a name, and what
/// <c>?!Reserved</c> then takes back out. Written as one kind per pattern it refused
/// <c>zone</c>, which is a word of SQL-92 and a perfectly good column name.
/// </para>
/// <para>
/// Nothing here is emitted. This produces a graph; who compiles it is the emitter's
/// business, and `docs/lexical-adt-design.md` carries the design and its measurements.
/// </para>
/// </remarks>
public sealed class LexicalSplit
{
	LexicalSplit(RecognitionGraph syntax, TerminalInventory inventory, IReadOnlyList<string> blocked)
	{
		Syntax    = syntax;
		Inventory = inventory;
		Blocked   = blocked;
	}

	/// <summary>The syntactic machine, over kinds.</summary>
	public RecognitionGraph Syntax { get; }

	/// <summary>What the kinds are, and what a lexical machine recognizes to produce them.</summary>
	public TerminalInventory Inventory { get; }

	/// <summary>What the split could not do, empty where it did all of it.</summary>
	public IReadOnlyList<string> Blocked { get; }

	/// <summary>Cuts a grammar in two, or answers null where there is nothing to cut.</summary>
	/// <remarks>
	/// Null for a scannerless grammar and for one the inventory could not number. Not a
	/// half-done split: a machine built from a partial numbering would silently accept a
	/// different language, and the character path is right there and correct.
	/// </remarks>
	public static LexicalSplit? Of(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var inventory = TerminalInventory.Of(graph);

		if (!inventory.Applies || inventory.Blocked.Count > 0)
			return null;

		return new Rewriter(graph, inventory).Split();
	}

	sealed class Rewriter(RecognitionGraph graph, TerminalInventory inventory)
	{
		readonly HashSet<RuleSymbol> _lexical = Lexicals(graph, inventory);
		readonly HashSet<string>     _sets    = [.. inventory.Sets.Select(set => set.Name)];

		/// <summary>
		/// Every rule the syntactic machine no longer holds.
		/// </summary>
		/// <remarks>
		/// Three kinds and one reason: the lexer holds them instead. What trivia and the word
		/// boundary are made of, whatever a class reaches, and any rule that turned out to be
		/// a set of terminals — the last of those becomes a range at each of its call sites,
		/// so keeping the rule would be keeping a second spelling of the same fact.
		/// </remarks>
		static HashSet<RuleSymbol> Lexicals(RecognitionGraph graph, TerminalInventory inventory)
		{
			var roots = new List<RuleSymbol>(
				graph.Trivia.Values.OfType<Node.Call>().Select(call => call.Rule));

			foreach (var rule in graph.Rules)
				if (rule.Name == "wordboundary" && !rule.IsBuiltIn ||
					inventory.PatternOf(rule) is not null)
				{
					roots.Add(rule);
				}

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

		public LexicalSplit Split()
		{
			var rules   = new List<RuleSymbol>();
			var bodies  = new Dictionary<RuleSymbol, Node>();
			var blocked = new List<string>();

			foreach (var rule in graph.Rules)
			{
				if (_lexical.Contains(rule) ||
					_sets.Contains(rule.Name) ||
					!graph.Bodies.TryGetValue(rule, out var body))
				{
					continue;
				}

				rules.Add(rule);
				bodies[rule] = Rewrite(body, rule, blocked);
			}

			var syntax = new RecognitionGraph(
				rules,
				bodies,
				Kept(graph.Nullable, rules),
				Kept(graph.Results, rules),
				Kept(graph.Types, rules),
				graph.CSharpImports,
				graph.Publications,
				graph.Diagnostics)
			{
				// Trivia is gone by construction: the lexer ate it, and a seam between kinds
				// would be a place for whitespace that cannot arrive. Everything else a
				// grammar declared is about values and control, not about the alphabet, and
				// travels unchanged.
				Externals  = graph.Externals,
				Folds      = graph.Folds,
				Recoveries = graph.Recoveries,
				Climbing   = graph.Climbing,
				Powers     = graph.Powers,
				FreeNames  = graph.FreeNames,
			};

			return new LexicalSplit(syntax, inventory, blocked);
		}

		static Dictionary<RuleSymbol, T> Kept<T>(
			IReadOnlyDictionary<RuleSymbol, T> from, IReadOnlyCollection<RuleSymbol> rules)
		{
			var kept = new Dictionary<RuleSymbol, T>(rules.Count);

			foreach (var rule in rules)
				if (from.TryGetValue(rule, out var value))
					kept[rule] = value;

			return kept;
		}

		/// <summary>A test for one kind out of a set of ranges.</summary>
		/// <remarks>
		/// One range of one kind is a literal, which is the cheapest thing the machine reads
		/// and what a mark usually comes to. Anything wider is an element set, which is the
		/// same sentence about more numbers.
		/// </remarks>
		static Node Testing(IReadOnlyList<TerminalInventory.Group> ranges) =>
			ranges is [{ From: var only, To: var same }] && only == same
				? new Node.Literal(((char)only).ToString())
				: new Node.Element(
					false,
					[.. ranges.Select(range => new CharRange((char)range.From, (char)range.To))],
					[],
					[]);

		Node Rewrite(Node node, RuleSymbol owner, List<string> blocked)
		{
			switch (node)
			{
				case Node.Literal(var text) literal:
					return inventory.PatternOf(text, literal.IgnoreCase) is { } pattern
						? Testing(inventory.KindsOf(pattern))
						: Refuse(node, owner, blocked);

				case Node.Element element:
					return Elemental(element, owner, blocked);

				case Node.Behind:
					// §4.6's look-behind is half a word boundary, and the lexer keeps words
					// whole by construction, so there is nothing left for it to ask.
					return Node.Empty.Instance;

				case Node.Call(var called, var arguments):
				{
					// A built-in is a shape and not a terminal: `eof` is `?!any` and means the
					// same over kinds as over characters — no more input — and `any` means one
					// of whatever the alphabet holds. Rewritten through rather than looked up.
					if (called.IsBuiltIn && graph.Bodies.TryGetValue(called, out var standard))
						return Rewrite(standard, owner, blocked);

					if (inventory.PatternOf(called) is { } crossing)
						return Testing(inventory.KindsOf(crossing));

					if (_sets.Contains(called.Name) && inventory.SetOf(called.Name) is { } set)
						return Testing(set.Ranges);

					// Trivia and the boundary reach here as calls to rules the syntactic
					// machine no longer holds; both matched only whitespace or nothing.
					if (_lexical.Contains(called))
						return Node.Empty.Instance;

					return arguments.Count == 0
						? node
						: new Node.Call(called, [.. arguments.Select(one => Rewrite(one, owner, blocked))]);
				}

				case Node.Sequence(var parts):
				{
					var rewritten = new List<Node>(parts.Count);

					foreach (var part in parts)
						if (Rewrite(part, owner, blocked) is var one && one is not Node.Empty)
							rewritten.Add(one);

					return rewritten.Count switch
					{
						0 => Node.Empty.Instance,
						1 => rewritten[0],
						_ => new Node.Sequence(rewritten),
					};
				}

				case Node.Choice(var alternatives):
				{
					var rewritten = alternatives.Select(one => Rewrite(one, owner, blocked)).ToList();

					return rewritten.Count == 1 ? rewritten[0] : new Node.Choice(rewritten);
				}

				// What is left of a node whose whole content was the lexer's is nothing, and
				// nothing has to stay nothing rather than become a shape that means something
				// else. `?!wordboundary` is the one that bites: its operand rewrites away, and
				// a negative lookahead over what matches the empty string refuses everywhere —
				// the grammar would accept nothing at all, which is what the first run did
				// while looking like a triumph. A repetition of nothing is milder and still
				// wrong: it made the emitter allocate a turn local it had not counted.
				case Node.Repeat(var body, var min, var max):
					return Rewrite(body, owner, blocked) is var repeated && repeated is Node.Empty
						? Node.Empty.Instance
						: new Node.Repeat(repeated, min, max);

				case Node.Lookahead(var positive, var seen):
					return Rewrite(seen, owner, blocked) is var watched && watched is Node.Empty
						? Node.Empty.Instance
						: new Node.Lookahead(positive, watched);

				case Node.Atomic(var kept):
					return Rewrite(kept, owner, blocked) is var held && held is Node.Empty
						? Node.Empty.Instance
						: new Node.Atomic(held);

				case Node.Marked(var kept, var text):
					return Rewrite(kept, owner, blocked) is var noted && noted is Node.Empty
						? Node.Empty.Instance
						: new Node.Marked(noted, text);

				case Node.Capture(var name, var captured):
					return new Node.Capture(name, Rewrite(captured, owner, blocked));

				case Node.Construct(var built, var how):
					return new Node.Construct(Rewrite(built, owner, blocked), how);

				default:
					return node;
			}
		}

		/// <summary>
		/// A character class becomes the kinds of its characters — negation and all.
		/// </summary>
		/// <remarks>
		/// Negation is carried across rather than expanded. <c>[^ '(' | ')']</c> over
		/// characters is "one item that is not a bracket"; over kinds it is the same sentence
		/// about a wider alphabet, which is what <c>Subquery</c> means by "anything balanced"
		/// and could not say before. Expanding it would have named every kind but two, and
		/// then said nothing new when a kind was added.
		/// </remarks>
		Node Elemental(Node.Element element, RuleSymbol owner, List<string> blocked)
		{
			var named = FirstSets.OfElement(element with { IsNegated = false });

			// `any` again: nothing excluded, so one of whatever the alphabet holds — which
			// over kinds is one token, whichever it is.
			if (named.Nothing)
				return new Node.Element(element.IsNegated, [], [], []);

			if (!named.IsKnown)
				return Refuse(element, owner, blocked);

			var kinds = new List<CharRange>();

			foreach (var range in named.Ranges)
				for (var c = range.From; ; c++)
				{
					if (inventory.PatternOf(c.ToString(), false) is not { } pattern)
						return Refuse(element, owner, blocked);

					foreach (var one in inventory.KindsOf(pattern))
						kinds.Add(new CharRange((char)one.From, (char)one.To));

					if (c == range.To)
						break;
				}

			return new Node.Element(element.IsNegated, FirstSets.First.Normalized(kinds), [], []);
		}

		static Node Refuse(Node node, RuleSymbol owner, List<string> blocked)
		{
			blocked.Add($"no kind for {node} in {owner.Name}");

			return node;
		}
	}
}
