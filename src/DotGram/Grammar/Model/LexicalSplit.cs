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
///   "SELECT"                 -> one character standing for that kind
///   ?&lt;!b &amp; "SELECT" &amp; ?!b   -> the same; the boundary was the lexer's all along
///   ['+' | '-']              -> the set of the kinds those characters carry
///   RegularIdentifier        -> the kind of the class, the crossing being the terminal
///   Reserved                 -> the *range* those fifty-six words occupy
///   trivia                   -> nothing; whitespace never reaches this machine
/// </code>
/// <para>
/// The fifth line is the one that pays. <c>Identifier = ?!Reserved &amp; RegularIdentifier</c>
/// is a fifty-six-way negative lookahead at every identifier position over characters, and
/// over kinds it is a lookahead over one range — which <c>Determinism</c> then folds into
/// the surrounding choice like any other one-character test.
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
		TerminalInventory inventory,
		IReadOnlyList<RuleSymbol> lexical,
		IReadOnlyList<string> blocked)
	{
		Syntax    = syntax;
		Inventory = inventory;
		Lexical   = lexical;
		Blocked   = blocked;
	}

	/// <summary>The syntactic machine, over kinds.</summary>
	public RecognitionGraph Syntax { get; }

	/// <summary>What the kinds are.</summary>
	public TerminalInventory Inventory { get; }

	/// <summary>The rules the lexical machine has to recognize — the classes, in kind order.</summary>
	public IReadOnlyList<RuleSymbol> Lexical { get; }

	/// <summary>What the split could not do, empty where it did all of it.</summary>
	public IReadOnlyList<string> Blocked { get; }

	/// <summary>Cuts a grammar in two, or answers null where it cannot be cut.</summary>
	/// <remarks>
	/// Null and not a half-done split: a grammar with a shape the inventory could not number
	/// would compile into a machine that silently accepts a different language, and the
	/// scannerless path is right there and correct. What refuses is reported rather than
	/// guessed at.
	/// </remarks>
	public static LexicalSplit? Of(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var inventory = TerminalInventory.Of(graph);

		if (!inventory.Applies || inventory.Blocked.Count > 0)
			return null;

		var rewriter = new Rewriter(graph, inventory);

		return rewriter.Split();
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
			var roots = new List<RuleSymbol>(graph.Trivia.Values.OfType<Node.Call>().Select(call => call.Rule));

			foreach (var rule in graph.Rules)
				if (rule.Name == "wordboundary" && !rule.IsBuiltIn || inventory.KindOf(rule) > 0)
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

		public LexicalSplit Split()
		{
			var rules   = new List<RuleSymbol>();
			var bodies  = new Dictionary<RuleSymbol, Node>();
			var blocked = new List<string>();

			foreach (var rule in graph.Rules)
			{
				if (_lexical.Contains(rule) || _sets.Contains(rule.Name) || !graph.Bodies.TryGetValue(rule, out var body))
					continue;

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
				Externals    = graph.Externals,
				Folds        = graph.Folds,
				Recoveries   = graph.Recoveries,
				Climbing     = graph.Climbing,
				Powers       = graph.Powers,
				FreeNames    = graph.FreeNames,
			};

			var classes = inventory.Terminals
				.OfType<TerminalInventory.Terminal.Class>()
				.Select(one => one.Rule)
				.ToList();

			Overlaps(classes, blocked);

			return new LexicalSplit(syntax, inventory, classes, blocked);
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

		/// <summary>
		/// Two classes that can accept the same string, which no numbering can survive.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A token carries one kind. Where two classes both accept <c>10</c> — SQL-92's
		/// <c>Digits</c> and its <c>UnsignedNumericLiteral</c> do — the lexer must pick one,
		/// and every syntactic position that wanted the other stops reading. <c>Length = '('
		/// &amp; Digits &amp; ')'</c> is such a position, and <c>NUMERIC(10, 2)</c> is what
		/// stops.
		/// </para>
		/// <para>
		/// Nothing here can fix it. Making them one kind widens the language — a position
		/// that wanted digits would take <c>1.5</c> — and choosing between them narrows it.
		/// It is a fact about the grammar, and the only useful thing to do with it is to say
		/// which two rules and which string, so that an author can decide what they meant.
		/// </para>
		/// <para>
		/// The witness is the shortest string each class accepts, tried against the other.
		/// Cheap, and exact for the shapes a lexical rule has: it catches <c>Digits</c>
		/// against <c>UnsignedNumericLiteral</c> — <c>"0"</c> is accepted by both — and does
		/// not cry wolf over <c>ExpressionLanguage</c>'s <c>Decimals</c> against its
		/// <c>Number</c>, where the shortest is <c>"0m"</c> and only one of them takes it.
		/// </para>
		/// </remarks>
		void Overlaps(IReadOnlyList<RuleSymbol> classes, List<string> blocked)
		{
			var shortest = new Dictionary<RuleSymbol, string?>();

			foreach (var one in classes)
				shortest[one] = graph.Bodies.TryGetValue(one, out var body) ? Shortest(body, 0) : null;

			for (var i = 0; i < classes.Count; i++)
				for (var j = i + 1; j < classes.Count; j++)
				{
					var witness =
						shortest[classes[j]] is { } later  && Accepts(classes[i], later)  ? later  :
						shortest[classes[i]] is { } sooner && Accepts(classes[j], sooner) ? sooner :
						null;

					if (witness is not null)
						blocked.Add(
							$"{classes[i].Name} and {classes[j].Name} both accept \"{witness}\", " +
							"and a token carries one kind");
				}
		}

		/// <summary>The shortest string a node accepts, or null where it accepts none.</summary>
		/// <remarks>
		/// Depth-limited, because a rule that reaches itself has no shortest string this walk
		/// can reach by recursion alone — and a lexical rule deep enough to hit the limit is
		/// past the point where a witness would be readable anyway.
		/// </remarks>
		string? Shortest(Node node, int depth)
		{
			if (depth > 16)
				return null;

			switch (node)
			{
				case Node.Empty or Node.Guard or Node.Behind or Node.Lookahead:
					return "";

				case Node.Literal(var text):
					return text;

				case Node.Element element:
					return FirstSets.OfElement(element) is { IsKnown: true, Ranges.Count: > 0 } set
						? set.Ranges[0].From.ToString()
						: null;

				case Node.Sequence(var parts):
				{
					var built = "";

					foreach (var part in parts)
						if (Shortest(part, depth + 1) is { } one)
							built += one;
						else
							return null;

					return built;
				}

				case Node.Choice(var alternatives):
				{
					string? best = null;

					foreach (var alternative in alternatives)
						if (Shortest(alternative, depth + 1) is { } one && (best is null || one.Length < best.Length))
							best = one;

					return best;
				}

				case Node.Repeat(var body, var min, _):
				{
					if (min == 0)
						return "";

					if (Shortest(body, depth + 1) is not { } once)
						return null;

					var built = "";

					for (var turn = 0; turn < min; turn++)
						built += once;

					return built;
				}

				case Node.Call(var called, _):
					return graph.Bodies.TryGetValue(called, out var inner) ? Shortest(inner, depth + 1) : null;

				case Node.Atomic(var kept):        return Shortest(kept, depth + 1);
				case Node.Marked(var kept, _):     return Shortest(kept, depth + 1);
				case Node.Capture(_, var held):    return Shortest(held, depth + 1);
				case Node.Construct(var built, _): return Shortest(built, depth + 1);

				default: return null;
			}
		}

		/// <summary>
		/// Whether a lexical rule's language holds this exact string.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Asked of every word terminal against every class, because the answer is what keeps
		/// a non-reserved keyword usable as a name. <c>zone</c> is a word of SQL-92 — <c>WITH
		/// TIME ZONE</c> — and it is not reserved, so <c>Identifier = ?!Reserved &amp;
		/// RegularIdentifier</c> accepts it over characters. Over kinds it arrives as the
		/// keyword's kind and never reaches <c>RegularIdentifier</c> at all, so the class has
		/// to stand for itself <em>and</em> for the words it would have matched. That union is
		/// the set difference the design promised, arrived at from the other side.
		/// </para>
		/// <para>
		/// A matcher and not an analysis: the strings are keywords, a dozen characters at
		/// most, and the rules are the shapes a lexical rule has. It answers exactly rather
		/// than approximating, which matters because approximating one way refuses valid
		/// programs and the other way accepts invalid ones.
		/// </para>
		/// </remarks>
		bool Accepts(RuleSymbol rule, string text) =>
			graph.Bodies.TryGetValue(rule, out var body) && Ends(body, text, 0).Contains(text.Length);

		HashSet<int> Ends(Node node, string text, int at)
		{
			switch (node)
			{
				case Node.Empty or Node.Guard or Node.Behind:
					return [at];

				// A lookahead consumes nothing; taking it as satisfied admits a little more
				// than the rule does, and a word wrongly admitted here gets one kind too many
				// rather than one too few.
				case Node.Lookahead:
					return [at];

				case Node.Literal(var literal) one:
					return at + literal.Length <= text.Length &&
						text.AsSpan(at, literal.Length).Equals(
							literal.AsSpan(),
							one.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
						? [at + literal.Length]
						: [];

				case Node.Element element:
					return at < text.Length && FirstSets.OfElement(element) is { IsKnown: true } set &&
						set.Overlaps(FirstSets.First.Chars([new CharRange(text[at], text[at])]))
						? [at + 1]
						: [];

				case Node.Sequence(var parts):
				{
					var here = new HashSet<int> { at };

					foreach (var part in parts)
					{
						var next = new HashSet<int>();

						foreach (var one in here)
							next.UnionWith(Ends(part, text, one));

						if (next.Count == 0)
							return [];

						here = next;
					}

					return here;
				}

				case Node.Choice(var alternatives):
				{
					var all = new HashSet<int>();

					foreach (var alternative in alternatives)
						all.UnionWith(Ends(alternative, text, at));

					return all;
				}

				case Node.Repeat(var body, var min, var max):
				{
					var reached = new HashSet<int>();
					var here    = new HashSet<int> { at };

					if (min == 0)
						reached.Add(at);

					for (var turn = 1; turn <= (max ?? text.Length + 1) && here.Count > 0; turn++)
					{
						var next = new HashSet<int>();

						foreach (var one in here)
							foreach (var end in Ends(body, text, one))
								if (end > one)
									next.Add(end);

						if (turn >= min)
							reached.UnionWith(next);

						here = next;
					}

					return reached;
				}

				case Node.Call(var called, _):
					return graph.Bodies.TryGetValue(called, out var inner) ? Ends(inner, text, at) : [];

				case Node.Atomic(var kept):        return Ends(kept, text, at);
				case Node.Marked(var kept, _):     return Ends(kept, text, at);
				case Node.Capture(_, var held):    return Ends(held, text, at);
				case Node.Construct(var built, _): return Ends(built, text, at);

				default: return [];
			}
		}

		/// <summary>The kinds a class stands for: itself, and the words it would have matched.</summary>
		Node Crossing(RuleSymbol called, int kind)
		{
			if (!_union.TryGetValue(called, out var kinds))
			{
				kinds = [new CharRange((char)kind, (char)kind)];

				foreach (var word in inventory.Terminals.OfType<TerminalInventory.Terminal.Word>())
					if (Accepts(called, word.Text))
						kinds.Add(new CharRange((char)word.Kind, (char)word.Kind));

				_union[called] = kinds;
			}

			return kinds.Count == 1
				? Terminal(kind)
				: new Node.Element(false, FirstSets.First.Normalized(kinds), [], []);
		}

		readonly Dictionary<RuleSymbol, List<CharRange>> _union = [];

		/// <summary>One character, standing for a kind.</summary>
		static Node Terminal(int kind) => new Node.Literal(((char)kind).ToString());

		/// <summary>A set of kinds, as the element set that tests one of them.</summary>
		static Node Terminals(IEnumerable<TerminalInventory.Group> ranges) =>
			new Node.Element(
				false,
				[.. ranges.Select(range => new CharRange((char)range.From, (char)range.To))],
				[],
				[]);

		Node Rewrite(Node node, RuleSymbol owner, List<string> blocked)
		{
			switch (node)
			{
				case Node.Literal(var text) literal:
					return inventory.KindOf(text, literal.IgnoreCase) is var kind and > 0
						? Terminal(kind)
						: Refuse(node, owner, blocked);

				case Node.Element element:
					return Elemental(element, owner, blocked);

				case Node.Behind:
					// §4.6's look-behind is half a word boundary and the lexer keeps words
					// whole by construction, so there is nothing left for it to ask.
					return Node.Empty.Instance;

				case Node.Call(var called, var arguments):
				{
					if (_lexical.Contains(called) && inventory.KindOf(called) is var crossing and > 0)
						return Crossing(called, crossing);

					if (_sets.Contains(called.Name) && inventory.SetOf(called.Name) is { } set)
						return Terminals(set.Ranges);

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
				// nothing has to stay nothing rather than become a shape that means
				// something else. `?!wordboundary` is the one that bites: its operand
				// rewrites away, and a negative lookahead over what matches the empty string
				// refuses everywhere — the grammar would accept nothing at all. A repetition
				// of nothing is milder and still wrong: it made the emitter allocate a turn
				// local it had not counted, and the generated file would not compile.
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

			if (!named.IsKnown)
				return Refuse(element, owner, blocked);

			var kinds = new List<CharRange>();

			foreach (var range in named.Ranges)
				for (var c = range.From; ; c++)
				{
					var kind = inventory.KindOf(c.ToString(), false);

					if (kind == 0)
						return Refuse(element, owner, blocked);

					kinds.Add(new CharRange((char)kind, (char)kind));

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
