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
	LexicalSplit(
		RecognitionGraph syntax,
		RecognitionGraph source,
		IReadOnlyList<RuleSymbol> trivia,
		IReadOnlyList<RuleSymbol> valued,
		TerminalInventory inventory,
		IReadOnlyList<string> blocked)
	{
		Syntax    = syntax;
		Source    = source;
		Trivia    = trivia;
		Valued    = valued;
		Inventory = inventory;
		Blocked   = blocked;
	}

	/// <summary>The syntactic machine, over kinds.</summary>
	public RecognitionGraph Syntax { get; }

	/// <summary>What the kinds are, and what a lexical machine recognizes to produce them.</summary>
	public TerminalInventory Inventory { get; }

	/// <summary>The graph this was cut out of, over characters.</summary>
	/// <remarks>
	/// Kept for one thing: the seam. §4.5's <c>trivia</c> is compiled to a scanner already —
	/// atomic-braced, nothing written down — and a tokenizer that wants whitespace skipped
	/// between terminals calls it rather than recognizing it. The rule lives here and not in
	/// <see cref="Syntax"/>, which has no seams left at all.
	/// </remarks>
	public RecognitionGraph Source { get; }

	/// <summary>The rules that are the seam, whose scanner the tokenizer calls.</summary>
	public IReadOnlyList<RuleSymbol> Trivia { get; }

	/// <summary>The terminals whose value has to be read a second time.</summary>
	/// <remarks>
	/// <para>
	/// A terminal is usually a token and its text, and a grammar that wants no more gets no
	/// more. But a rule may build: <c>Hex : @string = "0x"i &amp; t: HexRun =&gt;
	/// @(t.Replace("_", ""))</c> says three things at once — what a hexadecimal literal looks
	/// like, which part of it is the number, and that the separators come out. The lexer
	/// answers the first, and the token it hands over is <c>0x_1F</c> whole, so the parts the
	/// other two named are gone.
	/// </para>
	/// <para>
	/// So the rule is read twice: once by the lexer, for where it ends, and once by its own
	/// character machine over exactly the text the token covers, for what it is worth. It
	/// stays in <see cref="Syntax"/> as its own kind and with its declared type, and with no
	/// members at all — nothing the syntactic machine walks builds it.
	/// </para>
	/// <para>
	/// The second read is not backtracking and cannot fail: the extent is already known and
	/// the lexer accepted it by that rule. It costs one pass over one token's characters, for
	/// the tokens of a grammar that asked for values in its terminals and nothing else.
	/// </para>
	/// </remarks>
	public IReadOnlyList<RuleSymbol> Valued { get; }

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
		readonly HashSet<RuleSymbol> _valued  = Values(graph, inventory);

		/// <summary>
		/// The terminals that build a value, worked out before anything is rewritten.
		/// </summary>
		/// <remarks>
		/// Before, because a call to one of them must stay a call. Every other terminal is
		/// replaced by its kind test where it is called from — the rule was only ever a name
		/// for a set of characters — but one of these has to be entered and left, so that the
		/// arena records where it began and ended and the materializer has an extent to read
		/// again.
		/// </remarks>
		static HashSet<RuleSymbol> Values(RecognitionGraph graph, TerminalInventory inventory)
		{
			var built = new HashSet<RuleSymbol>();

			foreach (var rule in graph.Rules)
				if (inventory.PatternOf(rule) is not null &&
					(graph.Types.ContainsKey(rule) || graph.Results[rule].Count > 0))
				{
					built.Add(rule);
				}

			return built;
		}

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
			var valued  = new List<RuleSymbol>();

			foreach (var rule in graph.Rules)
			{
				if (_lexical.Contains(rule) ||
					_sets.Contains(rule.Name) ||
					!graph.Bodies.TryGetValue(rule, out var body))
				{
					// A terminal that builds keeps its place, as its own kind and with the
					// type it declared. What it no longer has is members: the parts it named
					// are inside one token now, so nothing the syntactic machine walks can
					// build it, and its value is read again from the text (see `Valued`).
					if (_valued.Contains(rule))
					{
						rules.Add(rule);
						bodies[rule] = Testing(inventory.KindsOf(inventory.PatternOf(rule)!));
						valued.Add(rule);
					}

					continue;
				}

				rules.Add(rule);
				bodies[rule] = Rewrite(body, rule, blocked);
			}

			var results = Kept(graph.Results, rules);

			foreach (var rule in valued)
				results[rule] = [];

			var syntax = new RecognitionGraph(
				rules,
				bodies,
				Kept(graph.Nullable, rules),
				results,
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
				FreeNames  = graph.FreeNames,
				Context    = graph.Context,
				State      = graph.State,

				// Keyed by node — said again in the terms of the graph that now holds those
				// nodes, see `_became`. A fold is keyed by rule and holds nodes inside it,
				// so it is rebuilt rather than looked up.
				Recoveries = Remapped(graph.Recoveries),
				Powers     = Remapped(graph.Powers),
				Climbing   = Kept(graph.Climbing, rules).ToDictionary(
					one => one.Key,
					one => (IReadOnlyDictionary<Node, int>)Remapped(one.Value)),
				Folds = Refolded(Kept(graph.Folds, rules)),
			};

			return new LexicalSplit(
				syntax,
				graph,
				[.. graph.Trivia.Values.OfType<Node.Call>().Select(call => call.Rule).Distinct()],
				valued,
				inventory,
				blocked);
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

		/// <summary>
		/// The rewrite, and a record of what each node became.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Three of the graph's dictionaries are keyed by <em>node</em> rather than by rule —
		/// <c>Climbing</c> says which alternatives of a rule are left recursive, <c>Powers</c>
		/// what binds tighter than what, <c>Recoveries</c> where a refusal may be caught. A
		/// <c>Node</c> is a record, so those keys match by structure, and the whole of what
		/// this class does is change structure. Carried across unchanged they key nothing.
		/// </para>
		/// <para>
		/// It is silent where it is wrong, which is how it was found: a climbing rule whose
		/// alternatives no longer match its <c>Climbing</c> map is emitted as ordinary
		/// recursion, so the left capture is never bound and the C# the author wrote in
		/// <c>=&gt; @(...)</c> names a variable that does not exist. That is a compile error
		/// and therefore loud. Nothing says <c>Powers</c> would be.
		/// </para>
		/// </remarks>
		/// <summary>
		/// By identity, because the dictionaries being remapped are: the normalizer keys
		/// <c>Powers</c>, <c>Recoveries</c> and a fold's accumulators by the node object,
		/// so two call sites a grammar wrote the same way are two keys with two values.
		/// Keyed by structure this collapsed them into one — every call to a climbing rule
		/// took the strength of whichever site was rewritten last, and a parenthesized
		/// operand was read at the strength of the operator around it.
		/// </summary>
		readonly Dictionary<Node, Node> _became = new(NodeIdentity.Instance);

		Node Rewrite(Node node, RuleSymbol owner, List<string> blocked)
		{
			var into = Rewritten(node, owner, blocked);

			_became[node] = into;

			return into;
		}

		/// <summary>
		/// A left-recursive rule's fold, over the loop the rewrite made of its loop.
		/// </summary>
		/// <remarks>
		/// A fold is what §4.3 leaves behind when it turns <c>Or = Or &amp; "||" &amp; And</c>
		/// into a repetition of tails: the loop node, and which capture of each tail carries
		/// the value built so far. Lose it and the rule still reads — as a plain repetition,
		/// so the tail's capture becomes an array and the accumulator is never passed. The
		/// author's <c>=&gt; @(Expression.OrElse(left, right))</c> then has no <c>left</c>.
		/// </remarks>
		Dictionary<RuleSymbol, Fold> Refolded(Dictionary<RuleSymbol, Fold> folds)
		{
			var into = new Dictionary<RuleSymbol, Fold>(folds.Count);

			foreach (var one in folds)
				if (_became.TryGetValue(one.Value.Loop, out var loop))
					into[one.Key] = new Fold(loop, Remapped(one.Value.Accumulators));

			return into;
		}

		/// <summary>Every dictionary key that survived, said in the new graph's terms.</summary>
		Dictionary<Node, T> Remapped<T>(IReadOnlyDictionary<Node, T> from)
		{
			var into = new Dictionary<Node, T>(from.Count, NodeIdentity.Instance);

			foreach (var one in from)
				if (_became.TryGetValue(one.Key, out var became))
					into[became] = one.Value;

			return into;
		}

		Node Rewritten(Node node, RuleSymbol owner, List<string> blocked)
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

				case Node.Glue:
					// The one assertion that means more on this side of the crossing than the
					// other. Over characters `~` is the absence of a woven seam and needs no
					// node at all; here the tokens were made after the trivia was skipped, so
					// whether anything stood between two of them is a question only the token
					// positions can answer, and this is what asks it.
					return node;

				case Node.Call(var called, var arguments):
				{
					// A built-in is a shape and not a terminal: `eof` is `?!any` and means the
					// same over kinds as over characters — no more input — and `any` means one
					// of whatever the alphabet holds. Rewritten through rather than looked up.
					if (called.IsBuiltIn && graph.Bodies.TryGetValue(called, out var standard))
						return Rewrite(standard, owner, blocked);

					// A terminal that builds stays a call: the kind test is its whole body now,
					// and what the syntactic machine needs from it is an entry saying where it
					// began and ended, for the second read to be run over.
					if (_valued.Contains(called))
						return node;

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
