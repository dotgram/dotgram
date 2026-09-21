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
/// `docs/design/lexical-adt-design.md` is the design it belongs to and carries the measurements
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

	/// <summary>
	/// The kinds that are words, as ranges — what the built-in <c>word</c> comes to over
	/// kinds.
	/// </summary>
	/// <remarks>
	/// §4.6 says what continues a word, and this is that sentence applied to a token instead
	/// of to a literal: a kind is a word where every pattern it matched is one. A keyword
	/// always is — <see cref="Pattern.Word"/> is the literal beside which the boundary was
	/// woven — and a class is where every character it can hold continues a word, which is
	/// what makes an identifier one and a quoted name not.
	/// <para>
	/// Without it a grammar has to write out the reserved words it wants to read as names,
	/// which T-SQL needed in four places and sixty-one lines: an option's name, a hint's, a
	/// permission's word, an ODBC function's. None of those lists is a fact about the
	/// language — they are a fact about what the notation could not say.
	/// </para>
	/// </remarks>
	public IReadOnlyList<Group> WordKinds { get; private init; } = [];

	/// <summary>
	/// The machine that reads the patterns together, and whose accepting states are the kinds.
	/// </summary>
	/// <remarks>
	/// Null where the grammar has no lexical half or the patterns are not all regular. It is
	/// here rather than beside the inventory because the two are one construction: the kinds
	/// are what its accepting states carry.
	/// </remarks>
	public LexicalAutomaton? Machine { get; private init; }

	/// <summary>The kinds whose set holds this pattern — what a syntactic position tests.</summary>
	public IReadOnlyList<Group> KindsOf(Pattern pattern)
	{
		_of ??= Indexed();

		return pattern is not null && _of.TryGetValue(pattern, out var ranges) ? ranges : [];
	}

	/// <summary>The pattern a literal is, or null where the grammar has no such terminal.</summary>
	/// <remarks>
	/// Only what is spelled out: a class is a rule rather than a spelling, and an external is
	/// a method. <see cref="Spelling"/> answers the empty string for both, so leaving either
	/// in would file it under a spelling nothing writes — and two of them under the same one.
	/// </remarks>
	public Pattern? PatternOf(string text, bool ignoreCase)
	{
		_literals ??= Patterns
			.Where(one => one is Pattern.Word or Pattern.Mark)
			.ToDictionary(Spelling, one => one);

		return _literals.TryGetValue((text, ignoreCase), out var pattern) ? pattern : null;
	}

	/// <summary>The pattern a crossing into a rule is, or null where the rule is not one.</summary>
	/// <remarks>
	/// A terminal the host recognizes answers here too. It is a terminal like any other to
	/// everything that asks this — which rule is the lexer's, and which the syntax keeps —
	/// and differs only in who measures it.
	/// </remarks>
	public Pattern? PatternOf(RuleSymbol rule)
	{
		_classes ??= Patterns
			.Where(one => one is Pattern.Class or Pattern.External)
			.ToDictionary(
				one => one is Pattern.Class(_, var crossed) ? crossed : ((Pattern.External)one).Rule,
				one => one);

		return rule is not null && _classes.TryGetValue(rule, out var pattern) ? pattern : null;
	}

	/// <summary>The ranges a rule that is a set of terminals occupies, or null.</summary>
	public Named? SetOf(string name)
	{
		return Sets.FirstOrDefault(set => set.Name == name);
	}

	/// <summary>The terminals host code measures, with the kind each one is.</summary>
	/// <remarks>
	/// In the order the grammar names them, which is the order they are asked in: the lexer
	/// has no way to choose between two of these, there being no automaton to run them
	/// together, so the first that matches is the one — ordered choice, as §3 has it.
	/// </remarks>
	public IReadOnlyList<(Pattern.External Pattern, int Kind)> Externals =>
		_externals ??=
		[
			.. Kinds
				.Where(kind => kind.Matched.Count == 1 && kind.Matched[0] is Pattern.External)
				.Select(kind => ((Pattern.External)kind.Matched[0], kind.Number)),
		];

	IReadOnlyList<(Pattern.External Pattern, int Kind)>? _externals;

	/// <summary>The terminals the lexer begins and something else ends, with the kind each one is.</summary>
	/// <remarks>
	/// <para>
	/// A rule written as a regular beginning and then one operand that is not regular —
	/// <c>'/*' &amp; Nested</c>, <c>'&lt;' &amp; @ReadBlob</c>. The automaton reads the beginning
	/// together with every other pattern, so it is longest match that decides a raw string's
	/// <c>"""</c> against an empty string's <c>""</c>, and nothing is asked at a position where the
	/// beginning does not stand. Where it does, the rest is measured from there: by the host,
	/// or by the rule's own machine over characters.
	/// </para>
	/// <para>
	/// A kind is the whole terminal and holds nothing else. A beginning that was also a token
	/// of its own would leave the lexer holding a string it has two meanings for and no way to
	/// pick between them short of trying both, and that is refused rather than guessed.
	/// </para>
	/// </remarks>
	public IReadOnlyList<Continuation> Continued { get; private init; } = [];

	/// <summary>One terminal the lexer begins, and what measures the rest of it.</summary>
	/// <param name="Pattern">The class, as every other stage knows it.</param>
	/// <param name="Kind">The one kind it is.</param>
	/// <param name="Tail">
	/// A <see cref="Node.External"/>, or a <see cref="Node.Call"/> to a rule: a synthesized one
	/// whose body is an external with a value, or one the grammar wrote.
	/// </param>
	public sealed record Continuation(Pattern.Class Pattern, int Kind, Node Tail)
	{
		/// <summary>The beginning, where it is one fixed spelling; what a longer token is asked to start with.</summary>
		public string? Beginning { get; init; }

		/// <summary>
		/// The kinds a longer token than the beginning can be, where it starts with the
		/// beginning, each with the kind that is both it and this terminal.
		/// </summary>
		/// <remarks>
		/// <c>'+01:00'</c> is an interval string by the automaton and a character string by
		/// its rule, eight characters either way; a kind is every pattern that accepted the
		/// longest string, so it is both. The automaton stops on the interval string's kind
		/// and never saw the character string past its quote, so the lexer measures the rest
		/// there too and takes the union where the lengths agree (Tokenize_DotGram).
		/// </remarks>
		public IReadOnlyList<(int Kind, int Union)> Extended { get; init; } = [];
	}

	static Text Spelling(Pattern pattern)
	{
		return pattern switch
		{
			Pattern.Word(_, var text, var fold) => (text, fold),
			Pattern.Mark(_, var text, var fold) => (text, fold),
			_ => ("", false),
		};
	}

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
			public override string ToString()
			{
				return $"\"{Text}\"" + (IgnoreCase ? "i" : "");
			}
		}

		/// <summary>A literal that is not a word: a bracket, an operator, punctuation.</summary>
		public sealed record Mark(int Index, string Text, bool IgnoreCase) : Pattern(Index)
		{
			public override string ToString()
			{
				return (Text.Length == 1 ? CharRange.Quote(Text[0]) : $"\"{Text}\"") + (IgnoreCase ? "i" : "");
			}
		}

		/// <summary>A class of strings — the crossing into a rule that carries no trivia.</summary>
		public sealed record Class(int Index, RuleSymbol Rule) : Pattern(Index)
		{
			public override string ToString()
			{
				return Rule.Name;
			}
		}

		/// <summary>A terminal the host recognizes: a rule whose whole body is a bare `@M`.</summary>
		/// <remarks>
		/// The lexer does not measure this one — host code does, by §7.1's
		/// <c>bool M(ReadOnlySpan&lt;char&gt;, ref int pos)</c>, which says where it ended. It
		/// is a pattern so that everything downstream can find it by rule as it finds a class,
		/// and it carries no shape: it is not a regular language, which is the whole reason it
		/// is written this way. So it never reaches the automaton, and its kind is numbered
		/// after every kind the automaton gives.
		/// </remarks>
		public sealed record External(int Index, RuleSymbol Rule, string Method) : Pattern(Index)
		{
			public override string ToString()
			{
				return $"@{Method}";
			}
		}
	}

	/// <summary>One thing the lexical machine emits: the patterns that matched.</summary>
	/// <remarks>
	/// The set and not one of it. <c>SELECT</c> is a keyword and an identifier at once, and
	/// which of the two a syntactic position wanted is that position's business.
	/// </remarks>
	public sealed record Kind(int Number, IReadOnlyList<Pattern> Matched)
	{
		public override string ToString()
		{
			return $"{Number} {{{string.Join(", ", Matched)}}}";
		}
	}

	/// <summary>A rule that is a set of terminals, and the runs of kinds it comes to.</summary>
	public sealed record Named(string Name, IReadOnlyList<Group> Ranges)
	{
		public int Count => Ranges.Sum(range => range.Count);

		public override string ToString()
		{
			return $"{Name} = {string.Join(", ", Ranges.Select(range => $"{range.From}..{range.To}"))}";
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

		public override string ToString()
		{
			return $"{Name} {From}..{To}";
		}
	}

	/// <summary>Works out the inventory for a graph.</summary>
	public static TerminalInventory Of(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var walker = new Walker(graph);

		if (!walker.Applies)
			return new TerminalInventory(false, [], [], [], []);

		if (graph.Bodies.Values.Any(body => NodeWalk.Descendants(body).Any(node => node is Node.External { UsesInputView: true })))
			return new TerminalInventory(true, [], [], [], ["input-view recognizers require character parsing"]);

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

		/// <summary>The terminals host code measures, in the order the grammar names them.</summary>
		readonly List<(RuleSymbol Rule, string Method)> _externals = [];
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

		public bool IsSyntactic(RuleSymbol rule)
		{
			return graph.Trivia.ContainsKey(rule) && !_lexical.Contains(rule);
		}

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
				case Node.Reading:
				case Node.Glue:
				case Node.Guard:
				case Node.Empty:
					return;

				case Node.Call(var called, _):
					// Four kinds of callee and only one is a pattern. A built-in carries no
					// trivia because it carries no declaration, which made `eof` look like a
					// crossing and gave a position assertion a kind of its own; it is walked
					// through instead. What trivia and the word boundary are made of is the
					// lexer's already; syntax calling syntax is walked on the callee's own
					// turn, since every rule that carries trivia gets one; and what is left is
					// the crossing.
					if (called.IsBuiltIn)
					{
						if (graph.Bodies.TryGetValue(called, out var standard))
							Walk(standard, owner);

						return;
					}

					if (_lexical.Contains(called) || graph.Trivia.ContainsKey(called))
						return;

					// A crossing into a rule that is nothing but `@M` is a terminal host code
					// measures. Written this way and not as an external standing in the syntax
					// itself, which is refused below: a terminal is a rule, and what the lexer
					// hands the syntactic half is a kind, so there has to be a rule to name it.
					if (graph.Bodies.TryGetValue(called, out var only) &&
						only is Node.External(var measured))
					{
						if (_seen.Add("external " + called.Namespace + "." + called.Name))
							_externals.Add((called, measured));

						return;
					}

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

		static (Node.Literal Literal, int Last)? Worded(IReadOnlyList<Node> parts, int at)
		{
			return parts.Count > at + 2 &&
			parts[at] is Node.Behind &&
			parts[at + 1] is Node.Literal behind &&
			IsBoundary(parts[at + 2])
				? (behind, at + 2)
				: parts.Count > at + 1 &&
					parts[at] is Node.Literal ahead &&
					IsBoundary(parts[at + 1])
					? (ahead, at + 1)
					: null;
		}

		/// <summary>The `?!wordboundary` §4.6 weaves, as against any other refusal.</summary>
		static bool IsBoundary(Node node)
		{
			return node is Node.Lookahead(false, Node.Call(var rule, _)) && rule.Name == Boundary;
		}

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

			// `any` is `[^ ]`, a negation of nothing, and it names nothing: over kinds it
			// means one of whatever the alphabet holds, which needs no terminal of its own.
			// The `Nothing` set is what an empty complement comes back as.
			if (wanted.Nothing)
				return;

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

		/// <summary>One spelling, whatever shape the occurrence it was read from had.</summary>
		/// <remarks>
		/// The key used to hold which list it went into, so a literal read as a word in one
		/// place and as punctuation in another was taken twice — and two patterns for one
		/// string is two kinds for one lexeme, which the inventory then built a dictionary
		/// of and threw on. §4.6's weaving reaches most occurrences and not all of them, and
		/// a grammar has only to say a keyword twice in the wrong two places to find one it
		/// did not: sixty words added to T-SQL's `DROP` did.
		/// <para>
		/// A word wins, since the boundary was woven onto it somewhere and over kinds both
		/// occurrences test the same kind anyway — what the shape decides is the laminar
		/// ordering and whether `word` (§4.6) reads it, and both of those are answers about
		/// the lexeme rather than about where it was written.
		/// </para>
		/// </remarks>
		void Take(List<Text> into, string text, bool ignoreCase)
		{
			if (_seen.Add((ignoreCase ? "i" : "") + text))
			{
				into.Add((text, ignoreCase));

				return;
			}

			// Read as punctuation before and as a word now: move it.
			if (into == _words && _marks.Remove((text, ignoreCase)))
				_words.Add((text, ignoreCase));
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
				case Node.Choice { Selection: not null }: return null;

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
				// changing what it accepts, and §4.6's woven one does the same. Only where
				// nothing after the choice reads anything: this used to take the first part
				// of any sequence, so `("::" | '.') & Name` was called the set `{"::", "."}`
				// and every call site became that range — with the name after it gone. It
				// read `t::a` as `t` and an alias, and refused everything that followed.
				case Node.Sequence(var parts):
					return parts.Count > 0 && Silent(parts, 1) ? Choices(parts[0]) : null;

				case Node.Atomic(var kept):    return Choices(kept);
				case Node.Marked(var kept, _): return Choices(kept);

				default: return null;
			}
		}

		/// <summary>Whether everything from <paramref name="from"/> on consumes nothing.</summary>
		/// <remarks>
		/// What a boundary is made of and nothing else: an assertion, a guard, a seam that
		/// was woven and rewrote away. Anything that reads is a part of the rule the set
		/// would throw away, and a rule that reads more than its literals is not a set of
		/// them however much its first operand looks like one.
		/// </remarks>
		static bool Silent(IReadOnlyList<Node> parts, int from)
		{
			for (var at = from; at < parts.Count; at++)
				if (!Silent(parts[at]))
					return false;

			return true;
		}

		static bool Silent(Node node)
		{
			return node switch
			{
				Node.Lookahead or Node.Behind or Node.Glue or Node.Guard or Node.Empty or Node.Reading => true,
				Node.Literal(var text) => text.Length == 0,
				Node.Marked(var body, _) => Silent(body),
				Node.Atomic(var body) => Silent(body),
				Node.Sequence(var nodes) => Silent(nodes, 0),
				Node.Repeat(_, _, var max) => max == 0,
				_ => false,
			};
		}

		/// <summary>The one literal an alternative is, boundary and all — and nothing looser.</summary>
		static Node.Literal? Only(Node node)
		{
			return node switch
			{
				Node.Literal literal => literal,
				Node.Sequence([Node.Literal literal]) => literal,
				Node.Sequence([Node.Literal literal, Node.Lookahead(false, _)]) => literal,
				Node.Sequence([Node.Behind, Node.Literal literal, Node.Lookahead(false, _)]) => literal,
				_ => null,
			};
		}

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

			// A class every character of which continues a word stands with the words and not
			// after the marks. `word` is a set like any `Laminar` orders above — the widest
			// one a grammar has — and this is the ordering that makes it one run of kinds
			// rather than two: the keywords, then the identifier, then everything else.
			foreach (var rule in _classes.Where(WordShaped))
				patterns.Add(new Pattern.Class(patterns.Count, rule));

			foreach (var mark in marks)
				patterns.Add(of[mark] = new Pattern.Mark(patterns.Count, mark.Text, mark.IgnoreCase));

			foreach (var rule in _classes.Where(one => !WordShaped(one)))
				patterns.Add(new Pattern.Class(patterns.Count, rule));

			// One machine over all of them at once, and its accepting states are the kinds.
			// Exactly, and not from witnesses: which sets of patterns some string makes
			// accept together is what a subset construction answers, and answering it any
			// other way is either approximate or a second implementation of the lexer.
			var shapes = patterns.Select(one => Shape(one)).ToList();
			var machine = shapes.Any(one => one is null)
				? null
				: LexicalAutomaton.Of(graph, [.. shapes!], _reasons);

			if (machine is null)
			{
				Refuse("the patterns are not all regular, so they cannot be read together");

				return new TerminalInventory(true, patterns, [], [], _reasons);
			}

			// In the order the machine gives them, which is the order the emitted scanner
			// prints — it renumbers its own sets by the patterns they hold, so that the
			// laminar ordering above survives into the kinds. Sorting them a second time here
			// is how two numberings came to exist, and nineteen inputs said so.
			var kinds = machine.Sets
				.Select((set, at) => new Kind(at + 1, [.. set.Select(one => patterns[one])]))
				.ToList();

			// A terminal the lexer only begins has to be told apart by that beginning alone,
			// because the beginning is all the lexer reads of it. One that can begin with
			// nothing would be begun everywhere; one whose beginning is also some other token
			// would give the lexer a string with two meanings. Both are refused.
			var continued = new List<Continuation>();

			foreach (var pattern in patterns.OfType<Pattern.Class>())
			{
				if (Continuation(pattern.Rule) is not var (prefix, tail))
					continue;

				if (Language.Shortest(graph, prefix) is { Length: 0 })
				{
					Refuse($"what begins {pattern.Rule.Name} can be nothing, so the lexer cannot tell where one stands: {prefix}");

					continue;
				}

				var holding = kinds.Where(kind => kind.Matched.Contains(pattern)).ToList();

				if (holding.Count != 1 || holding[0].Matched.Count != 1)
				{
					Refuse($"what begins {pattern.Rule.Name} is also " +
						string.Join(", ", holding.SelectMany(kind => kind.Matched).Where(one => one != pattern).Distinct()) +
						", and the lexer would read one string two ways");

					continue;
				}

				// What a longer token that starts with this beginning can be: the kinds the
				// automaton reaches past the states that accept the beginning alone. Each of
				// those tokens is one this terminal may also be, of the same length or not, and
				// only measuring says which. As many as there are longer patterns that begin
				// alike — for SQL's quote, the few quoted patterns that are not character
				// strings — so the unions below number the pairs, not the kinds squared.
				// Only ordinary patterns: where a longer token is itself one the lexer begins
				// — `$""""` past `$"""` — the longer beginning decides, and that
				// terminal is measured by its own rule, as before.
				var extended = Beyond(machine, holding[0].Number - 1)
					.Where(kind => !kinds[kind - 1].Matched.Any(one => one is Pattern.Class begun && Continuation(begun.Rule) is not null))
					.ToList();

				if (extended.Count > 0 && prefix is not Node.Literal { Text.Length: > 0 })
				{
					Refuse($"what begins {pattern.Rule.Name} begins longer tokens too, and only a beginning of one fixed spelling can be looked for under them: {prefix}");

					continue;
				}

				continued.Add(new Continuation(pattern, holding[0].Number, tail)
				{
					Beginning = (prefix as Node.Literal)?.Text,
					Extended  = [.. extended.Select(static one => (one, 0))],
				});
			}

			if (continued.Count < patterns.OfType<Pattern.Class>().Count(one => Continuation(one.Rule) is not null))
				return new TerminalInventory(true, patterns, [], [], _reasons);

			// And then the terminals the machine never saw. Appended rather than woven in, for
			// the reason the numbering exists at all: `machine.Sets` holds indices into the
			// patterns it was given, so anything added before them would renumber what the
			// automaton already decided. Each is a kind of its own and matches nothing but
			// itself — two strings cannot make one of these accept together, there being no
			// automaton to accept in.
			foreach (var (rule, method) in _externals)
			{
				var pattern = new Pattern.External(patterns.Count, rule, method);

				patterns.Add(pattern);
				kinds.Add(new Kind(kinds.Count + 1, [pattern]));
			}

			// And the kinds a token is when a terminal the lexer begins measures as long as a
			// longer pattern that starts the same way: after everything else, for the same
			// reason, and none at all where no beginning is also the start of a longer token.
			for (var i = 0; i < continued.Count; i++)
			{
				if (continued[i].Extended.Count == 0)
					continue;

				var unions = new List<(int Kind, int Union)>();

				foreach (var (kind, _) in continued[i].Extended)
				{
					kinds.Add(new Kind(kinds.Count + 1, [.. kinds[kind - 1].Matched, continued[i].Pattern]));
					unions.Add((kind, kinds.Count));
				}

				continued[i] = continued[i] with { Extended = unions };
			}

			var counted = new TerminalInventory(true, patterns, kinds, [], _reasons);
			var wordly  = Runs(
				"word",
				kinds.Where(one => one.Matched.Count > 0 && one.Matched.All(IsWord))
					.Select(one => one.Number));

			var named = _sets
				.Select(one => new Named(
					one.Name,
					Runs(one.Name, one.Members
						.Where(of.ContainsKey)
						.SelectMany(member => counted.KindsOf(of[member]))
						.SelectMany(range => Enumerable.Range(range.From, range.Count)))))
				.ToList();

			return new TerminalInventory(true, patterns, kinds, named, _reasons)
			{
				Machine   = machine,
				WordKinds = wordly,
				Continued = continued,
			};
		}

		/// <summary>Whether one pattern is a word — §4.6's question asked of a token.</summary>
		bool IsWord(Pattern pattern)
		{
			return pattern switch
			{
				Pattern.Word => true,
				Pattern.Mark => false,

				// A class is a word where everything it can hold continues one.
				Pattern.Class(_, var rule) => WordShaped(rule),

				_ => false,
			};
		}

		/// <summary>Whether everything the rule a class crosses into can hold continues a word.</summary>
		/// <remarks>
		/// Compared against the boundary's own element rather than character by character: an
		/// identifier's parts are usually the very rule the boundary names, and where they are
		/// not this answers no, which costs a grammar nothing it had.
		/// </remarks>
		bool WordShaped(RuleSymbol rule)
		{
			return Boundaries() is { } within &&
			graph.Bodies.TryGetValue(rule, out var body) &&
			Within(body, within, []);
		}

		Node.Element? _boundary;
		bool          _looked;

		/// <summary>What §4.6 says continues a word here, resolved to one element.</summary>
		/// <remarks>
		/// The outermost one, where a grammar declares several. A lexical namespace declares
		/// its own boundary for its own literals (§4.6), and those are the parts of one
		/// lexeme rather than lexemes; the question being asked here is about whole tokens,
		/// which is the boundary the syntactic half was written against.
		/// </remarks>
		Node.Element? Boundaries()
		{
			if (_looked)
				return _boundary;

			_looked = true;

			foreach (var rule in graph.Rules
				.Where(one => one.Name == Boundary)
				.OrderBy(Depth))
			{
				if (graph.Bodies.TryGetValue(rule, out var body) && Resolve(body, []) is { } element)
				{
					_boundary = element;

					break;
				}
			}

			return _boundary;
		}

		static int Depth(RuleSymbol rule)
		{
			var depth = 0;

			for (var at = rule.Namespace.Parent; at is not null; at = at.Parent)
				depth++;

			return depth;
		}

		/// <summary>A body reduced to the one element it is, through however many calls.</summary>
		Node.Element? Resolve(Node node, HashSet<RuleSymbol> seen)
		{
			return node switch
			{
				Node.Element element => element.IsNegated ? null : element,
				Node.Call(var rule, _) when seen.Add(rule) &&
					graph.Bodies.TryGetValue(rule, out var called) => Resolve(called, seen),
				Node.Sequence([var only]) => Resolve(only, seen),
				Node.Choice([var only]) { Selection: null } => Resolve(only, seen),
				Node.Atomic(var kept) => Resolve(kept, seen),
				Node.Marked(var marked, _) => Resolve(marked, seen),
				_ => null,
			};
		}

		/// <summary>Whether everything a body can hold is inside the boundary.</summary>
		bool Within(Node node, Node.Element boundary, HashSet<RuleSymbol> seen)
		{
			switch (node)
			{
				case Node.Element element:
					return !element.IsNegated && Inside(element, boundary);

				case Node.Literal(var text) literal:
					return text.Length > 0 && text.All(c => Inside(One(c), boundary));

				case Node.Sequence(var nodes): return nodes.All(one => Within(one, boundary, seen));
				case Node.Choice(var nodes):   return nodes.All(one => Within(one, boundary, seen));
				case Node.Repeat(var repeated, _, _): return Within(repeated, boundary, seen);
				case Node.Atomic(var kept):    return Within(kept, boundary, seen);
				case Node.Marked(var marked, _): return Within(marked, boundary, seen);
				case Node.Capture(_, var held): return Within(held, boundary, seen);
				case Node.Lookahead:           return true;
				case Node.Empty:               return true;

				case Node.Call(var rule, _):
					return seen.Add(rule) &&
						graph.Bodies.TryGetValue(rule, out var body) &&
						Within(body, boundary, seen);

				default: return false;
			}
		}

		static Node.Element One(char c)
		{
			return new(false, [new CharRange(c, c)], [], []);
		}

		/// <summary>Whether one element admits nothing the boundary does not.</summary>
		static bool Inside(Node.Element element, Node.Element boundary)
		{
			return element.Ranges.All(range =>
				boundary.Ranges.Any(one => one.From <= range.From && range.To <= one.To)) &&
			element.Categories.All(boundary.Categories.Contains) &&
			element.References.All(boundary.References.Contains);
		}

		/// <summary>The node a pattern recognizes, as the automaton needs to read it.</summary>
		Node? Shape(Pattern pattern)
		{
			return pattern switch
			{
				Pattern.Word(_, var word, var fold) => new Node.Literal(word) { IgnoreCase = fold },
				Pattern.Mark(_, var mark, var fold) => new Node.Literal(mark) { IgnoreCase = fold },
				Pattern.Class(_, var rule) => Continuation(rule) is var (prefix, _)
					? prefix
					: graph.Bodies.TryGetValue(rule, out var body) ? body : null,
				_ => null,
			};
		}

		/// <summary>
		/// A class that is a regular beginning and then one operand that is not, split there.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Only the last operand, and only one that cannot be read by an automaton: an external
		/// recognizer, or a call to a rule that is not a regular language. A class whose last
		/// call is regular is a class like any other and the automaton reads all of it — this is
		/// asked of what used to be refused, never of what used to be read.
		/// </para>
		/// <para>
		/// A value is looked through, and so is a capture of the last operand: <c>Tag : @T =
		/// '&lt;' &amp; b: Inner =&gt; …</c> is still where the lexer stops and something else
		/// goes on, and its value is read afterwards over the whole token as any terminal's is.
		/// </para>
		/// </remarks>
		(Node Prefix, Node Tail)? Continuation(RuleSymbol rule)
		{
			if (_tails.TryGetValue(rule, out var known))
				return known;

			if (!graph.Bodies.TryGetValue(rule, out var body))
				return null;

			while (true)
			{
				if (body is Node.Construct(var built, _))
					body = built;
				else if (body is Node.Marked(var noted, _))
					body = noted;
				else
					break;
			}

			(Node Prefix, Node Tail)? found = null;

			if (body is Node.Sequence(var parts) && parts.Count >= 2 &&
				(parts[parts.Count - 1] is Node.Capture(_, var held) ? held : parts[parts.Count - 1]) is var tail &&
				Unread(tail))
			{
				found = (parts.Count == 2 ? parts[0] : new Node.Sequence([.. parts.Take(parts.Count - 1)]), tail);
			}

			_tails[rule] = found;

			return found;
		}

		readonly Dictionary<RuleSymbol, (Node Prefix, Node Tail)?> _tails = [];

		/// <summary>
		/// The kinds the automaton can accept further on from a state that accepts only
		/// <paramref name="set"/>: the tokens longer than a beginning that start with it.
		/// </summary>
		static List<int> Beyond(LexicalAutomaton machine, int set)
		{
			var found   = new SortedSet<int>();
			var seen    = new HashSet<int>();
			var pending = new Stack<int>();

			for (var state = 0; state < machine.Accepts.Count; state++)
				if (machine.Accepts[state] == set)
					pending.Push(state);

			while (pending.Count > 0)
			{
				var state = pending.Pop();

				foreach (var next in machine.Next[state])
				{
					if (next < 0 || !seen.Add(next))
						continue;

					if (machine.Accepts[next] >= 0 && machine.Accepts[next] != set)
						found.Add(machine.Accepts[next] + 1);

					pending.Push(next);
				}
			}

			return [.. found];
		}

		/// <summary>Whether no automaton can read this operand, so something else has to.</summary>
		/// <remarks>
		/// A call whose body is <c>@M</c> is the rule the normalizer makes of a method with an
		/// overload that has a value (§7.1's third row). It has no declaration, which is what
		/// <see cref="RuleSymbol.IsBuiltIn"/> asks, so that question is kept for the rest.
		/// </remarks>
		bool Unread(Node tail)
		{
			return tail switch
			{
				Node.External => true,
				Node.Call(var called, _) when graph.Bodies.TryGetValue(called, out var body) =>
					body is Node.External || !called.IsBuiltIn && LexicalAutomaton.Of(graph, [body], []) is null,
				_ => false,
			};
		}

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
