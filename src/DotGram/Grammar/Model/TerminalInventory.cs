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
	/// By kind number, the kind a token is where the character after it continues a word: the
	/// same patterns without the keywords, 0 where nothing else matched. Empty where no kind
	/// holds a keyword.
	/// </summary>
	/// <remarks>
	/// <para>
	/// §4.6 puts a word boundary after a keyword, and an automaton cannot ask it: longest
	/// match decides between the patterns, and where none of them is longer the token ends
	/// inside a word. <c>gogo</c> came out as two keywords <c>go</c> wherever no class covered
	/// the longer word, and <c>gox</c> as a keyword and a mark — both of which the reading over
	/// characters refuses. So the lexer asks the character after a token that holds a keyword,
	/// and where it continues a word takes this kind instead — and where that is none, the
	/// longest shorter token, the keyword never having matched.
	/// </para>
	/// <para>
	/// Not the character before. §4.6 guards that side too, and over tokens a keyword glued to
	/// the number before it — <c>1MB</c> — is read as two tokens all the same, which is how the
	/// T-SQL engine reads it and what the T-SQL tests hold the grammar to.
	/// </para>
	/// <para>
	/// A kind of its own where the keyword had none: <c>go</c> read by <c>"go"</c> and by a
	/// class that matches only it is one kind, and the class alone is not a kind the automaton
	/// gives. Those are numbered after every other kind, so nothing already numbered moves.
	/// A kind that is not asked maps to itself: one that holds no keyword, one whose keywords'
	/// boundaries have no character in common, and one holding a spelling some syntax reads with
	/// no boundary woven around it — over characters that reading matches inside a word.
	/// </para>
	/// </remarks>
	public IReadOnlyList<int> Unworded { get; private init; } = [];

	/// <summary>
	/// The boundaries the lexer asks after a keyword, and by kind number which one, -1 where it
	/// asks none. Both empty where <see cref="Unworded"/> is.
	/// </summary>
	/// <remarks>
	/// The boundary a keyword was woven with, which is its namespace's (§4.6) and not one for the
	/// whole grammar: a grammar that inherits another reads the inherited keywords under the
	/// inherited boundary. A kind whose keywords were woven under several asks what all of them
	/// agree continues a word, so it refuses nothing the reading over characters reads.
	/// </remarks>
	public IReadOnlyList<Node.Element> WordBoundaries { get; private init; } = [];

	/// <inheritdoc cref="WordBoundaries"/>
	public IReadOnlyList<int> WordBoundaryOf { get; private init; } = [];

	/// <summary>
	/// The kinds the lexer has to ask at all, as runs: those that can end a token right before a
	/// character that continues a word. Empty where none can, and then nothing is asked.
	/// </summary>
	/// <remarks>
	/// Most cannot, which is what makes the question nearly free. A keyword that is also an
	/// identifier — <c>SELECT</c>, read by <c>"SELECT"</c> and by the name class — ends where the
	/// automaton stopped, and from there every character that continues a word leads on to an
	/// accepting state: longest match has already taken the longer word, so no such character can
	/// follow the token. Only a state with some word character that does not lead on is asked;
	/// a kind made by measuring a continuation is asked wherever it holds a keyword.
	/// </remarks>
	public IReadOnlyList<Group> Asked { get; private init; } = [];

	/// <summary>
	/// The machine that reads the patterns together, and whose accepting states are the kinds.
	/// </summary>
	/// <remarks>
	/// Null where the grammar has no lexical half or the patterns are not all regular. It is
	/// here rather than beside the inventory because the two are one construction: the kinds
	/// are what its accepting states carry.
	/// </remarks>
	public LexicalAutomaton? Machine { get; private init; }

	/// <summary>
	/// The same machine without the terminals that bind text, for the position where one of
	/// them was begun and its rule refused: read again as if the terminal were not there, which
	/// is the answer an ordinary token in its place would have given. Null where no terminal
	/// binds text.
	/// </summary>
	/// <remarks>
	/// Its accepting states are numbered by <see cref="MaskedKinds"/> into the kinds of
	/// <see cref="Machine"/>: what it accepts, the first machine accepts too, with the bound
	/// terminal beside it at most — and a string the two machines accept alike is one kind.
	/// Where the first machine never accepted such a set alone, the set is a kind appended for it.
	/// </remarks>
	public LexicalAutomaton? Masked { get; private init; }

	/// <summary>By accepting set of <see cref="Masked"/>, the kind it is; empty where there is no such machine.</summary>
	public IReadOnlyList<int> MaskedKinds { get; private init; } = [];

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
		/// <summary>
		/// Where the rest is Std.Nested's, what it opens and closes with: the lexer reads it with a
		/// counter from where the beginning, which ends with <c>open</c>, ended.
		/// </summary>
		internal NestedPair.Delimiters? Counted { get; init; }

		/// <summary>
		/// Whether the rest is measured by the terminal's own rule read again from the token's
		/// start, rather than by <see cref="Tail"/> from where the beginning ended.
		/// </summary>
		/// <remarks>
		/// A terminal that binds text (<c>$q: X</c>) and reads it again: the binding stands in
		/// the beginning and the reference in the rest, and a rest measured from where the
		/// beginning ended would have no text to compare with. So the automaton reads only what
		/// it can of such a terminal — enough to say that one stands here — and the rule reads
		/// the whole token from its start, binding and all. <see cref="Tail"/> is then a call to
		/// the terminal itself, and costs the beginning's few characters read twice.
		/// </remarks>
		public bool FromStart { get; init; }

		/// <summary>
		/// The kinds that hold this terminal's beginning together with another token's string
		/// as long — empty but for a terminal that binds text, whose own rule then says which
		/// of the two stands: longer than the string, the terminal; refused, the other token,
		/// found by reading the position again without the terminal; as long, both, and the
		/// syntax asks for the one it wants.
		/// </summary>
		public IReadOnlyList<int> Shared { get; init; } = [];

		/// <summary>
		/// The machine over what every alternative that binds text necessarily begins with — its
		/// operands through the binding's — run from the token's start before the rule is: where
		/// it accepts nothing, no such alternative could have been taken, the automaton read the
		/// token through an alternative that binds nothing, whole, and the rule is not read again.
		/// Null where the terminal is always read again.
		/// </summary>
		public LexicalAutomaton? Tagged { get; init; }

		/// <summary>
		/// The alternatives that bind nothing, as written, where <see cref="Tagged"/> stands: a
		/// token the tags passed was read whole through one of them, and its value may be read
		/// through that one alone (CSharpEmitter.Through).
		/// </summary>
		public IReadOnlyList<Node> Unbound { get; init; } = [];

		/// <summary>The beginning, where it is one fixed spelling; what a longer token is asked to start with.</summary>
		public string? Beginning { get; init; }

		/// <summary>Whether <see cref="Beginning"/> is a literal that ignores case, and is asked for so.</summary>
		public bool BeginningFolds { get; init; }

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

	/// <summary>
	/// The bindings, references and lengths a grammar cut in two cannot read: every one, each where
	/// it was written.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Asked before anything is numbered, and answered with errors rather than with a reason the cut
	/// was not made. A reason would leave the grammar to be read over characters with a warning,
	/// where a binding works — but what was asked for was a grammar read over tokens, and an author
	/// who asked for one is told plainly why there is none.
	/// </para>
	/// <para>
	/// Three answers. A rule with trivia is read over token kinds, where a binding would bind kinds
	/// and not text, and is never right; a guard comparing two captures says what was meant. A
	/// terminal that binds text is read by the lexer as far as an automaton can read it and then
	/// whole by its own rule (<see cref="Continuation.FromStart"/>), which needs a beginning the
	/// automaton can read; one with none is refused, naming the terminal. And what trivia is made
	/// of is skipped between tokens by a scanner, which does not read a binding yet.
	/// </para>
	/// </remarks>
	public static IReadOnlyList<GramDiagnostic> BoundTextRefusals(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var walker   = new Walker(graph);
		var refusals = new List<GramDiagnostic>();

		if (!walker.Applies)
			return refusals;

		// Which rules are terminals is what the walk over the syntax finds, as the inventory
		// finds them: a binding is judged by the terminal it stands in.
		foreach (var rule in graph.Rules)
			if (walker.IsSyntactic(rule) && graph.Bodies.TryGetValue(rule, out var body))
				walker.Walk(body, rule);

		foreach (var rule in graph.Rules)
		{
			if (!graph.Bodies.TryGetValue(rule, out var body) || !GrammarNormalizer.HoldsBoundText(body))
				continue;

			var reasons = new List<string>();

			if (walker.IsSyntactic(rule))
			{
				reasons.Add(
					$"is in '{rule.Name}', which carries trivia and is read over tokens once the grammar is " +
					"cut in two (Lexical = true): there it would bind token kinds, not text. Compare two " +
					"captures with a guard instead — 'close: Name & when @(close == open)' — or read the " +
					"rule over characters (docs/syntax.md §3.5.1).");
			}
			else
			{
				if (walker.InTrivia(rule))
					reasons.Add(
						$"is in '{rule.Name}', which is skipped between tokens once the grammar is cut in two " +
						"(Lexical = true), and the scanner that skips trivia does not read a binding yet. " +
						"Read this grammar over characters: leave Lexical off.");

				foreach (var terminal in walker.Classes)
					if (graph.Reaches(terminal).Contains(rule) && !walker.Begins(terminal))
						reasons.Add(
							(terminal == rule
								? $"is in '{rule.Name}', which the lexer reads once the grammar is cut in two (Lexical = true). "
								: $"is in '{rule.Name}', which the lexer reads as part of '{terminal.Name}' once the grammar is cut in two (Lexical = true). ") +
							"The lexer begins a terminal that binds text with what it can read of it and the terminal's own rule " +
							$"reads the rest from the token's start, and nothing the lexer can read begins '{terminal.Name}': " +
							"start it with a literal or a class, or read this grammar over characters (leave Lexical off).");
			}

			if (reasons.Count == 0)
				continue;

			var id = walker.IsSyntactic(rule) ? GrammarNormalizer.BindingOverKinds : GrammarNormalizer.BindingNotSupported;

			foreach (var node in NodeWalk.Descendants(body))
				foreach (var (binding, at) in GrammarNormalizer.BoundTextAt(node))
					foreach (var reason in reasons)
						refusals.Add(new GramDiagnostic(
							id,
							$"'${binding.Name}' {reason}",
							Math.Max(at, 0), at < 0 ? 0 : binding.Name.Length + 1, GramSeverity.Error));
		}

		return refusals;
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

		/// <summary>The boundaries each keyword was woven with: what the lexer asks after its token.</summary>
		readonly Dictionary<Text, List<Node.Element?>> _woven = [];

		/// <summary>The literals some syntax reads with no boundary woven around them.</summary>
		readonly HashSet<Text> _unguarded = [];

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

		/// <summary>Whether a rule is part of what trivia or the word boundary is made of.</summary>
		public bool InTrivia(RuleSymbol rule)
		{
			return _lexical.Contains(rule);
		}

		/// <summary>The terminals the syntax walked so far crosses into, in the order it named them.</summary>
		public IReadOnlyList<RuleSymbol> Classes => _classes;

		/// <summary>
		/// Whether a terminal that binds text has a beginning the lexer can read it by
		/// (<see cref="Continuation.FromStart"/>).
		/// </summary>
		public bool Begins(RuleSymbol terminal)
		{
			return !graph.ReachesBoundText(terminal) || Continuation(terminal) is not null;
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

							if (!_woven.TryGetValue((literal.Text, literal.IgnoreCase), out var with))
								_woven[(literal.Text, literal.IgnoreCase)] = with = [];

							with.Add(nodes[i] is Node.Behind behind ? behind.Test : Element(nodes[last], []));

							i = last;
						}
						else
						{
							Walk(nodes[i], owner);
						}

					return;

				// A repetition marked `recover` reads what it synchronizes on as much as its body
				// (§8.2), and a separator only the synchronization names is a token of its own.
				case Node.Repeat(var repeated, _, _):
					Walk(repeated, owner);

					if (graph.Recoveries.TryGetValue(node, out var recovery))
						Walk(recovery.Sync, owner);

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
			// Read somewhere with no boundary woven around it: over characters that occurrence
			// matches inside a word, so the lexer may not refuse the spelling for being in one.
			if (into == _marks)
				_unguarded.Add((text, ignoreCase));

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

			// A class read whole by the automaton is the token; one the lexer only begins is read
			// again by its own rule from the token's start.
			bool Exactly(int index)
			{
				return patterns[index] is not Pattern.Class(_, var rule) || Continuation(rule) is null;
			}
			var machine = shapes.Any(one => one is null)
				? null
				: LexicalAutomaton.Of(graph, [.. shapes!], _reasons, Exactly);

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
				if (Continuation(pattern.Rule) is not var (prefix, tail, pair, fromStart))
					continue;

				if (Language.Shortest(graph, prefix) is { Length: 0 })
				{
					Refuse($"what begins {pattern.Rule.Name} can be nothing, so the lexer cannot tell where one stands: {prefix}");

					continue;
				}

				var holding = kinds.Where(kind => kind.Matched.Contains(pattern)).ToList();

				// A terminal that binds text is held to more than a beginning of its own, and to
				// less: its beginning may be another token's string, as long, since its own rule
				// then says which of the two stands.
				if (fromStart)
				{
					if (Bound(pattern, prefix, tail, holding, kinds, machine, patterns) is { } begun)
						continued.Add(begun);

					continue;
				}

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

				// A bound literal, `$q: "ab"`, is the one fixed spelling its literal is.
				var spelled = prefix is Node.Bind(_, var bound, _) ? bound : prefix;

				if (extended.Count > 0 && spelled is not Node.Literal { Text.Length: > 0 })
				{
					Refuse($"what begins {pattern.Rule.Name} begins longer tokens too, and only a beginning of one fixed spelling can be looked for under them: {prefix}");

					continue;
				}

				continued.Add(new Continuation(pattern, holding[0].Number, tail)
				{
					Counted   = pair,
					FromStart = fromStart,
					Beginning = (spelled as Node.Literal)?.Text,
					BeginningFolds = spelled is Node.Literal { IgnoreCase: true },
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

			// And the machine without the terminals that bind text, for the position where one
			// was begun and its rule refused. Its sets are the first machine's kinds, less the
			// bound terminal; a set the first machine never accepted on its own — the string was
			// always the terminal's beginning as well — is a kind appended here.
			LexicalAutomaton? masked      = null;
			var                maskedKinds = new List<int>();

			var hidden = new HashSet<int>(continued.Where(static one => one.FromStart).Select(static one => one.Pattern.Index));

			// Only where it could find something: a failed reading began with a beginning string,
			// and what the second machine could find at that place is a token matching a prefix
			// of it — a longer one is refused at build time. Where no other kind accepts along the
			// way to a beginning, there is nothing to look for, and no machine is written.
			if (hidden.Count > 0 && RelexFinds(machine, hidden))
			{
				masked = LexicalAutomaton.Of(
					graph,
					[.. shapes.Select((shape, index) => hidden.Contains(index) ? Nothing : shape!)],
					_reasons,
					Exactly);

				if (masked is null)
					return new TerminalInventory(true, patterns, [], [], _reasons);

				foreach (var set in masked.Sets)
				{
					var members = set.Select(one => patterns[one]).ToList();
					var same    = kinds.FirstOrDefault(kind =>
						kind.Matched.Count == members.Count && members.All(kind.Matched.Contains));

					if (same is null)
						kinds.Add(same = new Kind(kinds.Count + 1, members));

					maskedKinds.Add(same.Number);
				}
			}

			var (woven, wovenWith, wovenOf) = Unworded(kinds);
			var asked = Asked(machine, kinds, woven, wovenWith, wovenOf, masked, maskedKinds);

			var (unworded, boundaries, boundaryOf) = Settled(kinds, woven, wovenWith, wovenOf, asked);

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
				Machine        = machine,
				Masked         = masked,
				MaskedKinds    = maskedKinds,
				WordKinds      = wordly,
				Continued      = continued,
				Unworded       = unworded,
				WordBoundaries = boundaries,
				WordBoundaryOf = boundaryOf,
				Asked          = asked,
			};
		}

		/// <summary>
		/// By kind number, whether it holds a keyword to ask about — 0 where nothing else is in
		/// the kind, -1 where something is, the kind itself where nothing is asked — and the
		/// boundary asked after it; all three empty where no kind holds one.
		/// </summary>
		(List<int> Unworded, List<Node.Element> Boundaries, List<int> BoundaryOf) Unworded(List<Kind> kinds)
		{
			var unworded   = new List<int>(kinds.Count + 1) { 0 };
			var boundaries = new List<Node.Element>();
			var boundaryOf = new List<int>(kinds.Count + 1) { -1 };

			for (var at = 0; at < kinds.Count; at++)
			{
				var matched  = kinds[at].Matched;
				var words    = matched.OfType<Pattern.Word>().ToList();
				var boundary = words.Count == 0
					? null
					: Narrowest(words.SelectMany(word => !_unguarded.Contains((word.Text, word.IgnoreCase)) &&
						_woven.TryGetValue((word.Text, word.IgnoreCase), out var with) ? with : [null]));

				if (boundary is null)
				{
					unworded.Add(kinds[at].Number);
					boundaryOf.Add(-1);

					continue;
				}

				var index = boundaries.FindIndex(one => Same(one, boundary));

				if (index < 0)
				{
					index = boundaries.Count;
					boundaries.Add(boundary);
				}

				boundaryOf.Add(index);

				// Which kind it is without them is settled once it is known to be asked at all,
				// so that a kind nobody asks for is not numbered.
				unworded.Add(matched.Any(one => one is not Pattern.Word) ? -1 : 0);
			}

			return boundaries.Count == 0 ? ([], [], []) : (unworded, boundaries, boundaryOf);
		}

		/// <summary>
		/// Each kind that holds a keyword mapped to the kind it is without them; all empty where
		/// nothing is asked.
		/// </summary>
		/// <remarks>
		/// Every such kind and not only the asked ones: a token the lexer shortened, having refused
		/// the longer, ends where longest match did not stop, and the pruning says nothing of it.
		/// A set that is no kind yet is appended as one, and maps to itself: it holds no keyword.
		/// </remarks>
		static (List<int> Unworded, List<Node.Element> Boundaries, List<int> BoundaryOf) Settled(
			List<Kind> kinds,
			List<int> unworded,
			List<Node.Element> boundaries,
			List<int> boundaryOf,
			IReadOnlyList<Group> asked)
		{
			if (asked.Count == 0)
				return ([], [], []);

			var known = kinds.Count;

			for (var number = 1; number <= known; number++)
			{
				if (unworded[number] != -1)
					continue;

				var rest = kinds[number - 1].Matched.Where(one => one is not Pattern.Word).ToList();
				var same = kinds.FirstOrDefault(kind =>
					kind.Matched.Count == rest.Count && kind.Matched.All(rest.Contains));

				if (same is null)
				{
					kinds.Add(same = new Kind(kinds.Count + 1, rest));
					unworded.Add(same.Number);
					boundaryOf.Add(-1);
				}

				unworded[number] = same.Number;
			}

			return (unworded, boundaries, boundaryOf);
		}

		/// <summary>The kinds that can end a token right before a character that continues a word.</summary>
		static List<Group> Asked(
			LexicalAutomaton machine,
			List<Kind> kinds,
			List<int> unworded,
			List<Node.Element> boundaries,
			List<int> boundaryOf,
			LexicalAutomaton? masked,
			IReadOnlyList<int> maskedKinds)
		{
			if (unworded.Count == 0)
				return [];

			var asked = new HashSet<int>();

			// A kind past the automaton's own was made by measuring a continuation, and the
			// automaton says nothing of where it ends.
			for (var number = machine.Sets.Count + 1; number <= kinds.Count; number++)
				if (unworded[number] != number)
					asked.Add(number);

			var words = boundaries.Select(one => FirstSets.OfElement(one)).ToList();

			for (var state = 0; state < machine.Accepts.Count; state++)
			{
				var number = machine.Accepts[state] + 1;

				if (number == 0 || unworded[number] == number || asked.Contains(number))
					continue;

				if (StopsBeforeWord(machine, state, words[boundaryOf[number]]))
					asked.Add(number);
			}

			// And the states of the machine without the terminals that bind text, which stops
			// where the first would have gone on into such a terminal's beginning: a keyword it
			// ends on there can stand right before a character that continues a word.
			if (masked is not null)
				for (var state = 0; state < masked.Accepts.Count; state++)
				{
					if (masked.Accepts[state] < 0)
						continue;

					var number = maskedKinds[masked.Accepts[state]];

					if (unworded[number] == number || asked.Contains(number))
						continue;

					if (StopsBeforeWord(masked, state, words[boundaryOf[number]]))
						asked.Add(number);
				}

			return Runs("asked", asked);
		}

		/// <summary>
		/// Whether some character that continues a word does not lead on from this state to an
		/// accepting one — so a token ending here can stand right before it.
		/// </summary>
		static bool StopsBeforeWord(LexicalAutomaton machine, int state, FirstSets.First word)
		{
			if (!word.IsKnown)
				return !word.Nothing;

			var row = machine.Next[state];

			foreach (var range in word.Ranges)
			{
				var at = (int)range.From;

				for (var atom = 0; atom < machine.Atoms.Count && at <= range.To; atom++)
				{
					if (machine.Atoms[atom].To < at)
						continue;

					if (machine.Atoms[atom].From > at || row[atom] < 0 || machine.Accepts[row[atom]] < 0)
						return true;

					at = machine.Atoms[atom].To + 1;
				}

				if (at <= range.To)
					return true;
			}

			return false;
		}

		/// <summary>
		/// What every one of the boundaries says continues a word, as one element; null where that
		/// is nothing, or where one of them cannot be listed.
		/// </summary>
		/// <remarks>
		/// One of them as written where it is inside all the others — the usual case, an inherited
		/// grammar's boundary being a narrower one than the grammar's own — so that a category stays
		/// a category rather than the thousand ranges it is made of.
		/// </remarks>
		static Node.Element? Narrowest(IEnumerable<Node.Element?> boundaries)
		{
			var distinct = new List<Node.Element>();

			foreach (var boundary in boundaries)
			{
				if (boundary is null)
					return null;

				if (!distinct.Any(one => Same(one, boundary)))
					distinct.Add(boundary);
			}

			if (distinct.Count == 1)
				return distinct[0];

			var sets = distinct.Select(FirstSets.OfElement).ToList();

			if (sets.Any(set => !set.IsKnown))
				return null;

			for (var i = 0; i < distinct.Count; i++)
				if (sets.All(set => set.Covers(sets[i])))
					return distinct[i];

			var shared = sets.Aggregate((left, right) => left.And(right));

			return shared.IsKnown ? new Node.Element(false, shared.Ranges, [], []) : null;
		}

		/// <summary>Whether two elements are written alike.</summary>
		static bool Same(Node.Element left, Node.Element right)
		{
			return left.IsNegated == right.IsNegated &&
			left.Ranges.SequenceEqual(right.Ranges) &&
			left.Categories.SequenceEqual(right.Categories) &&
			left.References.SequenceEqual(right.References);
		}

		/// <summary>The element a boundary's lookahead comes down to, negated or not.</summary>
		Node.Element? Element(Node node, HashSet<RuleSymbol> seen)
		{
			return node switch
			{
				Node.Element element => element,
				Node.Lookahead(false, var body) => Element(body, seen),
				Node.Call(var rule, _) when seen.Add(rule) &&
					graph.Bodies.TryGetValue(rule, out var called) => Element(called, seen),
				Node.Sequence([var only]) => Element(only, seen),
				Node.Choice([var only]) { Selection: null } => Element(only, seen),
				Node.Atomic(var kept) => Element(kept, seen),
				Node.Marked(var marked, _) => Element(marked, seen),
				_ => null,
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
				Pattern.Class(_, var rule) => Continuation(rule) is var (prefix, _, _, _)
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
		/// <para>
		/// A terminal that binds text is split differently, wherever the binding stands: the
		/// beginning is what the automaton can read of it (<see cref="Beginning"/>), and the
		/// rest is the terminal itself, read from the token's start
		/// (<see cref="Continuation.FromStart"/>). Null where nothing regular begins it.
		/// </para>
		/// </remarks>
		(Node Prefix, Node Tail, NestedPair.Delimiters? Counted, bool FromStart)? Continuation(RuleSymbol rule)
		{
			if (_tails.TryGetValue(rule, out var known))
				return known;

			if (!graph.Bodies.TryGetValue(rule, out var body))
				return null;

			if (graph.ReachesBoundText(rule))
			{
				// Where the terminal's alternatives that bind no text can be read whole by the
				// automaton, they are, and the rule is not read again for a token none of the
				// others could have been (Skipping); else the longest regular prefix begins it.
				if (Skipping(rule, body) is var (shape, tags, unbound))
				{
					_skips[rule]   = tags;
					_unbound[rule] = unbound;
					_tails[rule] = (shape, new Node.Call(rule, []), null, true);

					return _tails[rule];
				}

				var begun = Beginning(body, []);

				_tails[rule] = begun is Node.Empty ? null : (begun, new Node.Call(rule, []), null, true);

				return _tails[rule];
			}

			// Std.Nested as the terminal itself: read as a call to it, which is what the branch
			// below knows how to split.
			if (NestedPair.Is(rule))
				body = new Node.Call(rule, []);

			while (true)
			{
				if (body is Node.Construct(var built, _))
					body = built;
				else if (body is Node.Marked(var noted, _))
					body = noted;
				else
					break;
			}

			(Node Prefix, Node Tail, NestedPair.Delimiters? Counted, bool FromStart)? found = null;

			// Std.Nested, last or alone: its `open` is a beginning like any other, and the rest
			// is read by a counter (NestedPair) rather than by the rule's machine over
			// characters. The tail is the call, which is what the rest is a reading of.
			var last = body is Node.Sequence({ Count: >= 1 } run) ? run[run.Count - 1] : body;

			if ((last is Node.Capture(_, var captured) ? captured : last) is Node.Call(var nested, _) &&
				NestedPair.Is(nested) && NestedPair.Of(nested, graph) is { } pair)
			{
				var open = new Node.Literal(pair.Open) { IgnoreCase = pair.OpenFolds };

				found = (Before(body is Node.Sequence({ Count: >= 2 } before) ? before.Take(before.Count - 1) : [], open), last, pair, false);
			}
			else if (body is Node.Sequence(var parts) && parts.Count >= 2 &&
				(parts[parts.Count - 1] is Node.Capture(_, var held) ? held : parts[parts.Count - 1]) is var tail &&
				Unread(tail))
			{
				found = (parts.Count == 2 ? parts[0] : new Node.Sequence([.. parts.Take(parts.Count - 1)]), tail, null, false);
			}

			_tails[rule] = found;

			return found;
		}

		readonly Dictionary<RuleSymbol, (Node Prefix, Node Tail, NestedPair.Delimiters? Counted, bool FromStart)?> _tails = [];

		/// <summary>
		/// What the automaton can read of a terminal that binds text, from its start: the longest
		/// prefix of its body that is a regular language, or <see cref="Node.Empty"/> where nothing is.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Such a terminal is not a regular language, and no automaton reads it whole: a reference
		/// matches text the automaton would have to remember. What it can read is a beginning —
		/// the leading operands up to the first that is not regular, and into that one as far as
		/// its own leading operands go — and that is all the lexer needs to say that the terminal
		/// stands here, since the rule itself then reads the whole token from its start. The
		/// beginning is every string the terminal can begin with, which is what makes it a safe
		/// place to stop the automaton: where it does not stand, neither does the terminal. Taken
		/// as far as it goes rather than only through the binding, because a longer beginning
		/// tells the terminal from the other tokens better — a heredoc's <c>"&lt;&lt;" &amp; Word
		/// &amp; eol</c> is not the operator <c>&lt;&lt;</c> where no line ends.
		/// </para>
		/// <para>
		/// A binding is read as its body; a choice begins as each alternative does, so a heredoc
		/// written as two alternatives begins with either, and nothing where one of them begins
		/// with nothing; a repetition of at least one turn begins as its body does, and one of none
		/// with nothing; a call is followed into the rule where the automaton cannot read the call
		/// itself. A reference, a count by a length, a lookahead and a host recognizer begin with
		/// nothing the automaton reads.
		/// </para>
		/// </remarks>
		Node Beginning(Node node, HashSet<RuleSymbol> inside)
		{
			if (Regular(node))
				return node;

			switch (node)
			{
				case Node.Sequence(var parts):
				{
					var leading = new List<Node>();

					foreach (var part in parts)
					{
						if (Regular(part))
						{
							leading.Add(part);

							continue;
						}

						if (Beginning(part, inside) is var begun && begun is not Node.Empty)
							leading.Add(begun);

						break;
					}

					return leading.Count switch
					{
						0 => Node.Empty.Instance,
						1 => leading[0],
						_ => new Node.Sequence(leading),
					};
				}

				case Node.Choice { Selection: not null }:
					return Node.Empty.Instance;

				case Node.Choice(var alternatives):
				{
					var begun = new List<Node>(alternatives.Count);

					foreach (var alternative in alternatives)
					{
						if (Beginning(alternative, inside) is var one && one is Node.Empty)
							return Node.Empty.Instance;

						// Two alternatives that begin as one rule does begin alike, and the
						// beginning is that rule's once: a diamond of such rules, each a choice of
						// two that call the next, would otherwise unfold into a tree exponential in
						// its depth for every machine built over it.
						if (!begun.Any(known => ReferenceEquals(known, one)))
							begun.Add(one);
					}

					return begun.Count == 1 ? begun[0] : new Node.Choice(begun);
				}

				case Node.Repeat(var turns, var min, _):
					return min > 0 ? Beginning(turns, inside) : Node.Empty.Instance;

				case Node.Bind(_, var bound, _):  return Beginning(bound, inside);
				case Node.Atomic(var kept):       return Beginning(kept, inside);
				case Node.Marked(var kept, _):    return Beginning(kept, inside);
				case Node.Capture(_, var held):   return Beginning(held, inside);
				case Node.Construct(var made, _): return Beginning(made, inside);

				case Node.Call(var called, _):
				{
					if (_beginnings.TryGetValue(called, out var known))
						return known;

					if (!inside.Add(called) || !graph.Bodies.TryGetValue(called, out var body))
						return Node.Empty.Instance;

					var begun = Beginning(body, inside);

					inside.Remove(called);

					// Remembered even where a rule reached again inside it was cut short to
					// nothing: a shorter beginning is a safe one, which only begins the terminal
					// at more places than it stands.
					_beginnings[called] = begun;

					return begun;
				}

				default:
					return Node.Empty.Instance;
			}
		}

		/// <summary>Whether the automaton can read a node on its own.</summary>
		/// <remarks>
		/// Remembered by node, as <see cref="Beginning"/> is by rule: a beginning is asked of every
		/// operand on every path, and a diamond of rules — each a choice of two that call the same
		/// next one — has paths exponential in its depth, where the nodes and rules are linear.
		/// </remarks>
		bool Regular(Node node)
		{
			if (!_regular.TryGetValue(node, out var regular))
				_regular[node] = regular = LexicalAutomaton.Of(graph, [node], []) is not null;

			return regular;
		}

		readonly Dictionary<Node, bool>       _regular    = new(NodeIdentity.Instance);
		readonly Dictionary<RuleSymbol, Node> _beginnings = [];

		/// <summary>By terminal, what its alternatives that bind text necessarily begin with (<see cref="Skipping"/>).</summary>
		readonly Dictionary<RuleSymbol, Node> _skips = [];

		/// <summary>By terminal, its alternatives that bind nothing, as written (<see cref="Skipping"/>).</summary>
		readonly Dictionary<RuleSymbol, IReadOnlyList<Node>> _unbound = [];

		/// <summary>By rule, what it necessarily begins with through its binding (<see cref="Necessary"/>), remembered as a beginning is.</summary>
		readonly Dictionary<RuleSymbol, Node> _necessary = [];

		/// <summary>
		/// For a terminal that binds text, the shape the automaton reads where the rule need not
		/// read every token again, and the tags that say when it must; null where it always must.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A terminal written as one atomic choice of forms — a raw string and a plain one — binds
		/// text in some alternatives and not in others. A token read through an alternative that
		/// binds nothing is a regular token, and the automaton reads it whole; the rule read again
		/// from the token's start would only say the same, at the cost of a reading. It is not read
		/// again where that cannot change the answer, which is when none of the alternatives before
		/// the one taken could have been: those that bind text are tagged by what they necessarily
		/// begin with — their operands through the binding's — and a tag that accepts no prefix of
		/// the text at the token's start rules its alternative out; those that bind nothing are
		/// held apart at build time, no earlier one matching a string that begins a later one's.
		/// A lookahead in an alternative that binds nothing is dropped from what the automaton
		/// reads and must be answered by the tags instead: where it would refuse, a tag must have
		/// accepted, which is a containment of regular languages, decided here.
		/// </para>
		/// <para>
		/// The token is then the alternative's longest match, as every regular token's is, where
		/// the rule read again would give its first reading; the two differ only where the
		/// alternative's own reading can stop short of its longest, which is the ordinary contract
		/// of a token (docs/syntax.md §3.5.1).
		/// </para>
		/// </remarks>
		(Node Shape, Node Tags, IReadOnlyList<Node> Unbound)? Skipping(RuleSymbol rule, Node body)
		{
			var inner = body;

			while (true)
			{
				switch (inner)
				{
					case Node.Construct(var built, _): inner = built; continue;
					case Node.Marked(var noted, _):    inner = noted; continue;
					case Node.Capture(_, var held):    inner = held;  continue;
				}

				break;
			}

			if (inner is not Node.Atomic(var kept))
				return null;

			var alternatives = kept is Node.Choice { Selection: null } choice ? choice.Nodes : [kept];
			var shapes       = new List<Node>(alternatives.Count);
			var tags         = new List<Node>();
			var whole        = new List<Node>();
			var unbound      = new List<Node>();
			var looks        = new List<(Node Before, Node Seen)>();

			foreach (var alternative in alternatives)
			{
				if (ReachesBinding(alternative))
				{
					var tag = Necessary(alternative, []);

					if (tag is Node.Empty || !Regular(tag) || Language.Shortest(graph, tag) is not { Length: > 0 })
						return null;

					tags.Add(tag);
					shapes.Add(Beginning(alternative, []));

					continue;
				}

				if (Relaxed(alternative, looks, []) is not { } relaxed || !Regular(relaxed))
					return null;

				shapes.Add(relaxed);
				whole.Add(relaxed);
				unbound.Add(alternative);
			}

			if (tags.Count == 0 || whole.Count == 0 || shapes.Any(static one => one is Node.Empty))
				return null;

			var tagged = tags.Count == 1 ? tags[0] : new Node.Choice(tags);

			// Every lookahead's failure, from the alternative's start and whatever follows, lies
			// inside what some tag accepts and whatever follows: then a token the tags passed is
			// one the lookahead passes.
			foreach (var (before, seen) in looks)
			{
				var failing = new Node.Sequence([before, Over(seen, []), Anything]);
				var within  = LexicalAutomaton.Of(graph, [failing, .. tags.Select(static tag => new Node.Sequence([tag, Anything]))], []);

				if (within is null || within.Sets.Any(static set => set.Contains(0) && set.Count == 1))
					return null;
			}

			// The alternatives that bind nothing, and every choice inside them, must be free of one
			// alternative matching a string that begins another's: there the rule's first reading
			// and the automaton's longest match can part — the automaton takes the longer reading
			// where the rule, trying them in order, may stop at the shorter.
			if (!PrefixFree(whole))
				return null;

			foreach (var one in whole)
				foreach (var nested in Choices(one, []))
					if (!PrefixFree(nested.Nodes))
						return null;

			return (shapes.Count == 1 ? shapes[0] : new Node.Choice(shapes), tagged, unbound);
		}

		/// <summary>Whether no alternative matches a string that begins, or is, a string another matches.</summary>
		bool PrefixFree(IReadOnlyList<Node> alternatives)
		{
			for (var i = 0; i < alternatives.Count; i++)
				for (var j = 0; j < alternatives.Count; j++)
				{
					if (i == j)
						continue;

					var pair = LexicalAutomaton.Of(graph, [alternatives[i], alternatives[j]], []);

					if (pair is null || Prefixes(pair, 0, 1))
						return false;
				}

			return true;
		}

		/// <summary>Every ordered choice a node holds, the rules it calls included.</summary>
		IEnumerable<Node.Choice> Choices(Node node, HashSet<RuleSymbol> inside)
		{
			foreach (var each in NodeWalk.Descendants(node))
			{
				if (each is Node.Choice { Selection: null } choice)
					yield return choice;

				if (each is Node.Call(var called, _) && inside.Add(called) && graph.Bodies.TryGetValue(called, out var body))
					foreach (var inner in Choices(body, inside))
						yield return inner;
			}
		}

		/// <summary>
		/// An alternative that binds nothing, as the automaton reads it: its lookaheads taken out
		/// and recorded with what stands before each; null where it is no sequence of regular
		/// operands and negative lookaheads among them.
		/// </summary>
		Node? Relaxed(Node alternative, List<(Node Before, Node Seen)> looks, HashSet<RuleSymbol> inside)
		{
			var node = alternative;

			while (true)
			{
				switch (node)
				{
					case Node.Construct(var built, _): node = built; continue;
					case Node.Marked(var noted, _):    node = noted; continue;
					case Node.Capture(_, var held):    node = held;  continue;
					case Node.Atomic(var held):        node = held;  continue;

					case Node.Call(var called, _) when !inside.Contains(called) && graph.Bodies.TryGetValue(called, out var body):
						inside.Add(called);
						node = body;

						continue;
				}

				break;
			}

			// Only where the automaton's reading of it is the rule's: what the automaton takes as
			// an empty match or reads with a way back the rule does not have is not.
			if (!Exact(node, node, []))
				return null;

			if (Regular(node))
				return node;

			if (node is not Node.Sequence(var parts))
				return null;

			var read = new List<Node>(parts.Count);

			foreach (var part in parts)
			{
				if (part is Node.Lookahead(var positive, var seen))
				{
					if (positive)
						return null;

					looks.Add((read.Count == 0 ? Node.Empty.Instance : read.Count == 1 ? read[0] : new Node.Sequence([.. read]), seen));

					continue;
				}

				if (!Regular(part))
					return null;

				read.Add(part);
			}

			return read.Count switch
			{
				0 => Node.Empty.Instance,
				1 => read[0],
				_ => new Node.Sequence(read),
			};
		}

		/// <summary>
		/// Whether the automaton reads <paramref name="node"/>, an alternative that binds nothing
		/// with its wrappers taken off, as the rule reads it — so that a token the automaton read
		/// whole is one the rule would have read, and need not be read again.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The automaton approximates, always in the direction of accepting more. A guard is an
		/// empty match to it, where the rule runs the C# and may refuse (<c>'a' &amp; when
		/// @(…)</c> read <c>a</c> as a token the rule refuses). An atomic group is its body to it,
		/// where the rule gives nothing back out of it: <c>{ 'a'+ } &amp; 'a'</c> refuses
		/// <c>aa</c>, the run having taken both, where the automaton splits them. A glue, a
		/// binding, a reference or a count by a length it reads as nothing or not at all. Any of
		/// them anywhere in the alternative, the rules it calls included, and the token is read
		/// again — but for one atomic shape that gives back nothing anyone would take: a run of
		/// one character (<c>{ C+ }</c>, no upper bound) that nothing after it can begin with
		/// (<see cref="Follow"/>), the run itself through the repetition around it excepted — a
		/// run followed by the same run is one run. The possessive reading then takes exactly the
		/// characters any reading would, but in fewer turns of that repetition: one that has to
		/// turn more than once is owed turns the merged run cannot pay (<c>({ 'a'+ }){2} &amp;
		/// 'c'</c> refuses <c>aac</c>), so every repetition around the run whose body the run can
		/// begin must be free to turn once or not at all; an upper bound takes nothing away, since
		/// merging only lessens the turns. A bounded run, <c>{ C{2,5} }</c>, can give back: five
		/// and one is not three and three.
		/// </para>
		/// <para>
		/// A lookahead's body is not looked into: a lookahead at the top is answered by the tags
		/// over a superset of what it refuses, and accepting more there is the safe direction.
		/// The one lookahead the automaton reads, the "until" of <c>(?!L &amp; X)*</c>, it reads
		/// exactly only where L follows the repetition and cannot overlap itself. Choices are the business of
		/// <see cref="PrefixFree"/>, asked of every one of them.
		/// </para>
		/// </remarks>
		bool Exact(Node node, Node scope, HashSet<RuleSymbol> inside)
		{
			switch (node)
			{
				case Node.Guard or Node.Glue or Node.Bind or Node.Same or Node.Counted:
					return false;

				case Node.Lookahead:
					return true;

				// "Everything up to a delimiter", `(?!L & X)* & L`, which the automaton reads as the
				// strings L does not occur in: exact where L follows, since L is what stops both
				// readings; standing anywhere else the automaton admits more — the lookahead sees
				// past what the repetition consumed, the automaton cannot — and `ab` is a token of
				// `(?!"ab" & ['a' | 'b'])* & 'b'` to it where the rule refuses the first character.
				case Node.Repeat(Node.Sequence([Node.Lookahead(false, Node.Literal { IgnoreCase: false }), _]), 0, null):
					return false;

				case Node.Sequence(var parts):
					for (var i = 0; i < parts.Count; i++)
					{
						if (parts[i] is Node.Repeat(Node.Sequence([Node.Lookahead(false, Node.Literal(var delimiter) { IgnoreCase: false }), var item]), 0, null) &&
							delimiter.Length > 0)
						{
							if (i + 1 >= parts.Count || parts[i + 1] is not Node.Literal(var next) { IgnoreCase: false } || next != delimiter)
								return false;

							// And only a delimiter that cannot overlap itself: the rule stops the run where the
							// delimiter first begins, and the automaton lets it run on to any place the delimiter
							// follows — `(?!"aa" & any)* & "aa"` on `aaa` is `aa` to the rule and `aaa` to it, the
							// run having taken one `a` first. A delimiter with no proper prefix that is also its
							// suffix cannot begin inside its own occurrence, and the two agree.
							if (Enumerable.Range(1, delimiter.Length - 1).Any(k => string.CompareOrdinal(delimiter, 0, delimiter, delimiter.Length - k, k) == 0))
								return false;

							if (!Exact(item, scope, inside))
								return false;

							continue;
						}

						if (!Exact(parts[i], scope, inside))
							return false;
					}

					return true;

				case Node.Atomic(Node.Repeat(var one, _, null) run) when Single(one, []):
				{
					var first = FirstSets.Of(one, graph);

					return Follow(scope, node, first, FirstSets.First.None, []) is { } follow && !follow.Overlaps(first) && Exact(run, scope, inside);
				}

				case Node.Atomic(var kept):
					return !GivesBack(kept, []) && Exact(kept, scope, inside);

				case Node.Call(var called, _):
					if (!inside.Add(called))
						return true;

					var exact = !graph.Bodies.TryGetValue(called, out var body) || Exact(body, scope, inside);

					inside.Remove(called);

					return exact;

				default:
					foreach (var child in node.Children)
						if (!Exact(child, scope, inside))
							return false;

					return true;
			}
		}

		/// <summary>Whether the node reads one character: a class, a one-character literal, or a rule that is one.</summary>
		bool Single(Node node, HashSet<RuleSymbol> inside)
		{
			switch (node)
			{
				case Node.Element:                           return true;
				case Node.Literal { Text.Length: 1 }:        return true;
				case Node.Capture(_, var held):              return Single(held, inside);
				case Node.Marked(var kept, _):               return Single(kept, inside);
				case Node.Call(var called, { Count: 0 }):    return inside.Add(called) && graph.Bodies.TryGetValue(called, out var body) && Single(body, inside);
				default:                                     return false;
			}
		}

		/// <summary>Whether anything under the node, the rules it calls included, is a repetition — the one shape that gives back.</summary>
		bool GivesBack(Node node, HashSet<RuleSymbol> inside)
		{
			foreach (var each in NodeWalk.Descendants(node))
			{
				if (each is Node.Repeat or Node.Counted)
					return true;

				if (each is Node.Call(var called, _) && inside.Add(called) && graph.Bodies.TryGetValue(called, out var body) && GivesBack(body, inside))
					return true;
			}

			return false;
		}

		/// <summary>
		/// What can follow <paramref name="target"/> wherever it stands under <paramref name="scope"/>:
		/// the first characters of what comes after it, through what may match nothing, and of the
		/// body of a repetition around it entered again — the target's own contribution to that
		/// left out. Everything, where a repetition around it that the target's characters
		/// (<paramref name="first"/>) can begin must turn more than once: the merged run would owe
		/// it turns. Null where the target is not under the scope.
		/// </summary>
		FirstSets.First? Follow(Node scope, Node target, FirstSets.First first, FirstSets.First follow, HashSet<RuleSymbol> inside)
		{
			if (ReferenceEquals(scope, target))
				return follow;

			switch (scope)
			{
				case Node.Sequence(var parts):
				{
					FirstSets.First? found = null;

					for (var i = 0; i < parts.Count; i++)
					{
						if (!Holds(parts[i], target, []))
							continue;

						var after    = FirstSets.First.None;
						var nullable = true;

						for (var j = i + 1; j < parts.Count && nullable; j++)
						{
							after     = after.Or(FirstSets.Of(parts[j], graph));
							nullable &= FirstSets.Nullable(parts[j], graph);
						}

						if (nullable)
							after = after.Or(follow);

						if (Follow(parts[i], target, first, after, inside) is { } inner)
							found = found is null ? inner : found.Or(inner);
					}

					return found;
				}

				case Node.Choice(var alternatives):
				{
					FirstSets.First? found = null;

					foreach (var alternative in alternatives)
						if (Follow(alternative, target, first, follow, inside) is { } inner)
							found = found is null ? inner : found.Or(inner);

					return found;
				}

				case Node.Repeat(var body, var min, var max):
				{
					if (!Holds(body, target, []))
						return null;

					if (min > 1 && FirstSets.Of(body, graph).Overlaps(first))
						return FirstSets.First.All;

					var again = max == 1 ? FirstSets.First.None : FirstWithout(body, target, []).First;

					return Follow(body, target, first, again.Or(follow), inside);
				}

				case Node.Call(var called, _):
				{
					if (!inside.Add(called) || !graph.Bodies.TryGetValue(called, out var body))
						return null;

					var found = Follow(body, target, first, follow, inside);

					inside.Remove(called);

					return found;
				}

				case Node.Lookahead:
					return null;

				default:
				{
					FirstSets.First? found = null;

					foreach (var child in scope.Children)
						if (Follow(child, target, first, follow, inside) is { } inner)
							found = found is null ? inner : found.Or(inner);

					return found;
				}
			}
		}

		/// <summary>Whether the target stands under the node, the rules it calls included.</summary>
		bool Holds(Node node, Node target, HashSet<RuleSymbol> inside)
		{
			foreach (var each in NodeWalk.Descendants(node))
			{
				if (ReferenceEquals(each, target))
					return true;

				if (each is Node.Call(var called, _) && inside.Add(called) && graph.Bodies.TryGetValue(called, out var body) && Holds(body, target, inside))
					return true;
			}

			return false;
		}

		/// <summary>
		/// What the node can begin with and whether it may match nothing, with <paramref name="target"/>
		/// counted as beginning with nothing: what the body of a repetition begins with other than
		/// through the run being asked about.
		/// </summary>
		(FirstSets.First First, bool Nullable) FirstWithout(Node node, Node target, HashSet<RuleSymbol> inside)
		{
			if (ReferenceEquals(node, target))
				return (FirstSets.First.None, FirstSets.Nullable(node, graph));

			switch (node)
			{
				case Node.Sequence(var parts):
				{
					var first    = FirstSets.First.None;
					var nullable = true;

					foreach (var part in parts)
					{
						if (!nullable)
							break;

						var (one, empty) = FirstWithout(part, target, inside);

						first     = first.Or(one);
						nullable &= empty;
					}

					return (first, nullable);
				}

				case Node.Choice(var alternatives):
				{
					var first    = FirstSets.First.None;
					var nullable = false;

					foreach (var alternative in alternatives)
					{
						var (one, empty) = FirstWithout(alternative, target, inside);

						first     = first.Or(one);
						nullable |= empty;
					}

					return (first, nullable);
				}

				case Node.Repeat(var body, var min, _):
				{
					var (first, nullable) = FirstWithout(body, target, inside);

					return (first, min == 0 || nullable);
				}

				case Node.Atomic(var kept):       return FirstWithout(kept, target, inside);
				case Node.Marked(var kept, _):    return FirstWithout(kept, target, inside);
				case Node.Capture(_, var held):   return FirstWithout(held, target, inside);
				case Node.Construct(var made, _): return FirstWithout(made, target, inside);

				case Node.Call(var called, _) when inside.Add(called) && graph.Bodies.TryGetValue(called, out var body):
				{
					var found = FirstWithout(body, target, inside);

					inside.Remove(called);

					return found;
				}

				default:
					return (FirstSets.Of(node, graph), FirstSets.Nullable(node, graph));
			}
		}

		/// <summary>
		/// What a node that binds text necessarily begins with: its leading operands through the
		/// first that binds, the binding read as its body; <see cref="Node.Empty"/> where nothing
		/// regular begins it.
		/// </summary>
		Node Necessary(Node node, HashSet<RuleSymbol> inside)
		{
			switch (node)
			{
				case Node.Sequence(var parts):
				{
					var leading = new List<Node>();

					foreach (var part in parts)
					{
						if (!ReachesBinding(part))
						{
							if (!Regular(part))
								break;

							leading.Add(part);

							continue;
						}

						if (Necessary(part, inside) is var through && through is not Node.Empty)
							leading.Add(through);

						break;
					}

					return leading.Count switch
					{
						0 => Node.Empty.Instance,
						1 => leading[0],
						_ => new Node.Sequence(leading),
					};
				}

				case Node.Choice { Selection: not null }:
					return Node.Empty.Instance;

				case Node.Choice(var alternatives):
				{
					var begun = new List<Node>(alternatives.Count);

					foreach (var alternative in alternatives)
					{
						if (Necessary(alternative, inside) is var one && one is Node.Empty)
							return Node.Empty.Instance;

						// As in Beginning: two alternatives that begin as one rule does, once.
						if (!begun.Any(known => ReferenceEquals(known, one)))
							begun.Add(one);
					}

					return begun.Count == 1 ? begun[0] : new Node.Choice(begun);
				}

				case Node.Bind(_, var bound, _):  return Regular(bound) ? bound : Necessary(bound, inside);
				case Node.Repeat(var turns, var min, _): return min > 0 ? Necessary(turns, inside) : Node.Empty.Instance;
				case Node.Atomic(var kept):       return Necessary(kept, inside);
				case Node.Marked(var kept, _):    return Necessary(kept, inside);
				case Node.Capture(_, var held):   return Necessary(held, inside);
				case Node.Construct(var made, _): return Necessary(made, inside);

				case Node.Call(var called, _):
				{
					if (_necessary.TryGetValue(called, out var known))
						return known;

					if (!inside.Add(called) || !graph.Bodies.TryGetValue(called, out var body))
						return Node.Empty.Instance;

					var through = Necessary(body, inside);

					inside.Remove(called);
					_necessary[called] = through;

					return through;
				}

				default:
					return Node.Empty.Instance;
			}
		}

		/// <summary>Whether a node binds text, or calls a rule that does.</summary>
		bool ReachesBinding(Node node)
		{
			if (GrammarNormalizer.HoldsBoundText(node))
				return true;

			foreach (var each in NodeWalk.Descendants(node))
				if (each is Node.Call(var called, _) && graph.ReachesBoundText(called))
					return true;

			return node is Node.Call(var rule, _) && graph.ReachesBoundText(rule);
		}

		/// <summary>
		/// Whether, in a machine over two patterns, some string the first accepts begins a string
		/// the second accepts — the string itself included.
		/// </summary>
		static bool Prefixes(LexicalAutomaton machine, int a, int b)
		{
			bool Holds(int state, int which)
			{
				return machine.Accepts[state] >= 0 && machine.Sets[machine.Accepts[state]].Contains(which);
			}

			for (var state = 0; state < machine.Accepts.Count; state++)
				if (Holds(state, a) && (Holds(state, b) || Onward(machine, state).Any(next => Holds(next, b))))
					return true;

			return false;
		}

		/// <summary>
		/// Whether some kind other than a terminal that binds text accepts along the way to one of
		/// their beginnings — so that reading the position again without them can find a token.
		/// </summary>
		static bool RelexFinds(LexicalAutomaton machine, HashSet<int> hidden)
		{
			bool Bound(int state)
			{
				return machine.Accepts[state] >= 0 && machine.Sets[machine.Accepts[state]].Any(hidden.Contains);
			}

			// The states from which a beginning is still reachable, found backwards.
			var before  = new List<int>[machine.Next.Count];
			var leading = new HashSet<int>();
			var pending = new Stack<int>();

			for (var state = 0; state < machine.Next.Count; state++)
				before[state] = [];

			for (var state = 0; state < machine.Next.Count; state++)
				foreach (var next in machine.Next[state])
					if (next >= 0)
						before[next].Add(state);

			for (var state = 0; state < machine.Next.Count; state++)
				if (Bound(state) && leading.Add(state))
					pending.Push(state);

			while (pending.Count > 0)
				foreach (var earlier in before[pending.Pop()])
					if (leading.Add(earlier))
						pending.Push(earlier);

			// State 0 is the start, before any character: a kind accepting there accepts the
			// empty string, which no reading again looks for.
			for (var state = 1; state < machine.Next.Count; state++)
				if (leading.Contains(state) && machine.Accepts[state] >= 0 &&
					machine.Sets[machine.Accepts[state]].Any(one => !hidden.Contains(one)))
				{
					return true;
				}

			return false;
		}

		/// <summary>A node that reads nothing: what a terminal held back from a machine is replaced by.</summary>
		static readonly Node Nothing = new Node.Element(false, [], [], []);

		/// <summary>Any text at all, <c>any*</c>: what a shape no automaton reads is widened to.</summary>
		static readonly Node Anything = new Node.Repeat(new Node.Element(true, [], [], []), 0, null);

		/// <summary>
		/// A terminal that binds text, told whether the lexer may begin it, and how; null where
		/// it may not, the reason refused.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The lexer begins such a terminal on its beginning and lets its rule read the whole
		/// token from the start, so the token is that rule's first reading and not a longest
		/// match. Over characters the same rule gives the same reading only where nothing can
		/// take it back: the body is atomic, and no other token — nor the trivia skipped before
		/// every token — matches a longer text that one of the beginning's strings begins. Both
		/// are asked here, the second of the machine that reads every pattern together: from a
		/// state where the beginning has accepted, no further state may accept another kind.
		/// What the lexer begins and measures elsewhere is asked both ways, since how far its
		/// rule reads is not in the machine: its beginning and this one may not begin each other.
		/// The trivia is asked both ways too, as what the machine reads of it, widened where a
		/// part of it is no pattern.
		/// </para>
		/// <para>
		/// A string the beginning shares with another token, as long, is allowed: the rule then
		/// decides. Longer than the string, the token is the terminal; refused, the position is
		/// read again without the terminal and the other token stands; as long, the kind is both,
		/// and the syntax asks for the one it wants. So the terminal has a kind of its own even
		/// where the machine never accepted it alone, appended here.
		/// </para>
		/// </remarks>
		Continuation? Bound(
			Pattern.Class pattern,
			Node prefix,
			Node tail,
			List<Kind> holding,
			List<Kind> kinds,
			LexicalAutomaton machine,
			List<Pattern> patterns)
		{
			var name = pattern.Rule.Name;

			if (!graph.Bodies.TryGetValue(pattern.Rule, out var body) || !IsAtomic(body))
			{
				Refuse($"'{name}' binds text, so write `{{ }}` around its body to read it as one token; this part is read over characters");

				return null;
			}

			var mine = new HashSet<int>(holding.Select(static kind => kind.Number));

			// A string the beginning shares with another terminal the lexer begins is two
			// terminals to measure at one place, and only one would be: the two must begin
			// apart, as long as well as longer.
			foreach (var kind in holding)
				foreach (var other in kind.Matched)
					if (other != pattern && other is Pattern.Class(_, var begun) && Continuation(begun) is not null)
					{
						Refuse(Longer(pattern, other));

						return null;
					}

			// Past the beginning, nothing else may accept: what does could be longer than the
			// rule's reading of the terminal.
			foreach (var kind in holding)
				foreach (var further in Beyond(machine, kind.Number - 1))
					if (!mine.Contains(further))
					{
						foreach (var other in kinds[further - 1].Matched)
							if (other != pattern)
								Refuse(Longer(pattern, other));

						return null;
					}

			// And the beginning may not go on past another terminal the lexer begins and
			// measures elsewhere, since how far that one reads is not in the machine.
			foreach (var other in patterns.OfType<Pattern.Class>())
			{
				if (other == pattern || Continuation(other.Rule) is null)
					continue;

				foreach (var kind in kinds.Where(kind => kind.Matched.Contains(other)))
					if (Beyond(machine, kind.Number - 1).Any(mine.Contains))
					{
						Refuse(Longer(pattern, other));

						return null;
					}
			}

			// The trivia is skipped before the automaton runs, so it may neither begin where the
			// terminal begins nor read on past its beginning; asked of what a machine can read of
			// it, widened where a part of it is no pattern.
			foreach (var seam in graph.Trivia.Values.OfType<Node.Call>().Select(static call => call.Rule).Distinct())
			{
				if (!graph.Bodies.TryGetValue(seam, out var skipped))
					continue;

				var together = LexicalAutomaton.Of(graph, [prefix, Over(skipped, [])], []);

				if (together is null || Comparable(together, 0, 1))
				{
					Refuse($"'{name}' binds text and trivia can begin where '{name}' begins: keep the trivia from starting as '{name}' does, or from reading on past its beginning; this part is read over characters");

					return null;
				}
			}

			var alone  = holding.FirstOrDefault(static kind => kind.Matched.Count == 1);
			var shared = holding.Where(static kind => kind.Matched.Count > 1).Select(static kind => kind.Number).ToList();

			if (alone is null)
				kinds.Add(alone = new Kind(kinds.Count + 1, [pattern]));

			var spelled = prefix is Node.Bind(_, var bound, _) ? bound : prefix;

			return new Continuation(pattern, alone.Number, tail)
			{
				FromStart      = true,
				Shared         = shared,
				Tagged         = _skips.TryGetValue(pattern.Rule, out var tags) ? LexicalAutomaton.Of(graph, [tags], []) : null,
				Unbound        = _unbound.TryGetValue(pattern.Rule, out var unbound) ? unbound : [],
				Beginning      = (spelled as Node.Literal)?.Text,
				BeginningFolds = spelled is Node.Literal { IgnoreCase: true },
			};
		}

		/// <summary>Why a terminal that binds text is not begun, where another token can read on past its beginning.</summary>
		string Longer(Pattern.Class terminal, Pattern other)
		{
			return other is Pattern.Class(_, var rule) && Continuation(rule) is { FromStart: true }
				? $"'{terminal.Rule.Name}' and '{rule.Name}' both bind text and one's beginning begins the other's; this part is read over characters"
				: $"'{terminal.Rule.Name}' binds text and '{other}' can match a longer text that begins like '{terminal.Rule.Name}': " +
					$"make '{other}' unable to continue the beginning of '{terminal.Rule.Name}', or put both in one terminal as alternatives; " +
					"this part is read over characters";
		}

		/// <summary>Whether a body is one atomic group, under whatever names or builds it.</summary>
		static bool IsAtomic(Node body)
		{
			while (true)
			{
				switch (body)
				{
					case Node.Atomic:               return true;
					case Node.Construct(var built, _): body = built; break;
					case Node.Marked(var noted, _):    body = noted; break;
					case Node.Capture(_, var held):    body = held;  break;
					default:                        return false;
				}
			}
		}

		/// <summary>
		/// Whether, in a machine over two patterns, some non-empty string one of them accepts is
		/// also accepted by the other, or begins a string the other accepts.
		/// </summary>
		static bool Comparable(LexicalAutomaton machine, int a, int b)
		{
			bool Holds(int state, int which)
			{
				return machine.Accepts[state] >= 0 && machine.Sets[machine.Accepts[state]].Contains(which);
			}

			// State 0 is where the machine starts, before any character: a pattern accepting
			// there accepts the empty string, which begins nothing.
			for (var state = 1; state < machine.Accepts.Count; state++)
			{
				if (Holds(state, a) && (Holds(state, b) || Onward(machine, state).Any(next => Holds(next, b))))
					return true;

				if (Holds(state, b) && Onward(machine, state).Any(next => Holds(next, a)))
					return true;
			}

			return false;
		}

		/// <summary>Every state some non-empty string leads to from one.</summary>
		static IEnumerable<int> Onward(LexicalAutomaton machine, int from)
		{
			var seen    = new HashSet<int>();
			var pending = new Stack<int>();

			pending.Push(from);

			while (pending.Count > 0)
			{
				var state = pending.Pop();

				foreach (var next in machine.Next[state])
				{
					if (next < 0 || !seen.Add(next))
						continue;

					yield return next;

					pending.Push(next);
				}
			}
		}

		/// <summary>
		/// A node widened to a regular language that holds everything it reads: itself where an
		/// automaton reads it, and otherwise its parts widened, what no automaton reads at all
		/// standing for any text.
		/// </summary>
		/// <remarks>
		/// For the trivia, which is compared with a beginning and need not be a pattern: a
		/// nested comment inside it is read by a counter, and here stands for its opening and
		/// anything after. A lookahead reads nothing and is dropped, which widens too.
		/// </remarks>
		Node Over(Node node, HashSet<RuleSymbol> inside)
		{
			if (Regular(node))
				return node;

			switch (node)
			{
				case Node.Sequence(var parts):         return new Node.Sequence([.. parts.Select(part => Over(part, inside))]);
				case Node.Choice { Selection: not null }: return Anything;
				case Node.Choice(var alternatives):    return new Node.Choice([.. alternatives.Select(one => Over(one, inside))]);
				case Node.Repeat(var turns, var min, var max): return new Node.Repeat(Over(turns, inside), min, max);
				case Node.Bind(_, var bound, _):       return Over(bound, inside);
				case Node.Atomic(var kept):            return Over(kept, inside);
				case Node.Marked(var kept, _):         return Over(kept, inside);
				case Node.Capture(_, var held):        return Over(held, inside);
				case Node.Construct(var made, _):      return Over(made, inside);
				case Node.Lookahead or Node.Behind or Node.Guard or Node.Reading or Node.Glue or Node.Empty:
					return Node.Empty.Instance;

				case Node.Call(var called, _):
				{
					if (!inside.Add(called) || !graph.Bodies.TryGetValue(called, out var body))
						return Anything;

					var over = Over(body, inside);

					inside.Remove(called);

					return over;
				}

				default:
					return Anything;
			}
		}

		/// <summary>
		/// What stands before Std.Nested's <c>open</c>, and the <c>open</c>: one literal where a
		/// literal read alike stands right before it, so that <c>'#' &amp; Std.Nested("(", ")")</c>
		/// begins with the one spelling <c>#(</c> a longer token is asked to start with.
		/// </summary>
		static Node Before(IEnumerable<Node> leading, Node.Literal open)
		{
			var parts = leading.ToList();

			if (parts.Count > 0 && parts[parts.Count - 1] is Node.Literal last && last.IgnoreCase == open.IgnoreCase)
			{
				parts[parts.Count - 1] = new Node.Literal(last.Text + open.Text) { IgnoreCase = open.IgnoreCase };
			}
			else
				parts.Add(open);

			return parts.Count == 1 ? parts[0] : new Node.Sequence(parts);
		}

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
