using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

public sealed partial class GrammarNormalizer
{
	/// <summary>
	/// A literal or a class that holds U+0000 in a grammar that names <c>eos</c>, where that
	/// character stands for the end of a segment and is never one of a segment's own.
	/// </summary>
	public const string JoinerInSegmentedGrammar = "GRAM4047";

	/// <summary>
	/// Text captured, bound or published across an <c>eos</c>: a segment boundary holds no
	/// character, so there is no text of it to keep.
	/// </summary>
	public const string TextAcrossSegments = "GRAM4048";

	/// <summary>
	/// <c>Lexical = true</c> over a grammar that names <c>eos</c>: a token over a boundary, and the
	/// boundary as a kind of token, are not built.
	/// </summary>
	public const string LexicalSegments = "GRAM4049";

	/// <summary>
	/// A buffered or a byte form asked of a grammar that names <c>eos</c>, which reads sequences of
	/// segments — a list, a sequence, or a reader as its lines — and nothing else.
	/// </summary>
	public const string BufferedSegments = "GRAM4050";

	/// <summary>
	/// What a grammar that names <c>eos</c> reads between two segments: one character no segment
	/// holds, which <c>eos</c> matches and every other test of a character refuses.
	/// </summary>
	/// <remarks>
	/// A boundary has no character (docs/syntax.md §3.1.1), and this is how it is read without
	/// one: the sequence a parser of such a grammar is handed is read as its segments with this
	/// character after each, and a segment that holds it is refused before it is read. Nothing
	/// else can match it — <c>any</c>, every negated class and every predicate a class calls are
	/// narrowed to leave it out, and a literal or a class that names it is an error — so the
	/// boundary is where every reading of a character stops, as at the end of the input, and
	/// <c>eos</c> is the one thing that reads past it. <c>eof</c> is the end of everything and is
	/// not narrowed.
	/// </remarks>
	internal const char Joiner = '\0';

	/// <summary>Whether the grammar names the built-in <c>eos</c>, which makes its input segments.</summary>
	bool _segmented;

	static bool NamesEndOfSegment(GrammarModel model)
	{
		return EndOfSegmentAt(model) is not null;
	}

	/// <summary>Where the grammar first names the built-in <c>eos</c>, or null where it does not.</summary>
	internal static Location? EndOfSegmentAt(GrammarModel model)
	{
		Location? first = null;

		foreach (var pair in model.Bindings)
			if (pair.Value is RuleSymbol { IsBuiltIn: true, Name: "eos" } &&
				(first is not { } before || pair.Key.At.Position < before.Position))
				first = pair.Key.At;

		return first;
	}

	/// <summary>
	/// A class as a grammar that names <c>eos</c> reads it: a negated one does not take the joiner
	/// either, a positive one that names it as an item of its own is refused, and one whose range
	/// merely holds it is read without it.
	/// </summary>
	/// <remarks>
	/// No segment holds U+0000, so a class read without it reads every segment as it would with it:
	/// <c>['\0'..'\x1F']</c> is <c>['\x01'..'\x1F']</c> there, and <c>\p{Cc}</c> is read only where
	/// the joiner is not (<see cref="SegmentedPredicates"/>). An item that is U+0000 and nothing else
	/// could never match, which is worth a word.
	/// </remarks>
	Node.Element Segmented(Node.Element element, IReadOnlyList<Elem> items, Expr set)
	{
		if (!_segmented)
			return element;

		if (element.IsNegated)
			return element.Ranges.Any(static range => range.From <= Joiner && Joiner <= range.To)
				? element
				: element with { Ranges = Coalesce([new CharRange(Joiner, Joiner), .. element.Ranges]) };

		if (items.Any(static item => item is Elem.Chars([Joiner], null or [Joiner])))
			Report(
				JoinerInSegmentedGrammar,
				$"'{set}' names U+0000, which a grammar that names 'eos' reads as the end of a segment and " +
				"never as a character of one: no segment holds it, so the item never matches. Match the end " +
				"of a segment with 'eos' (docs/syntax.md §3.1.1).",
				set.At);

		if (!element.Ranges.Any(static range => range.From == Joiner))
			return element;

		var ranges = new List<CharRange>();

		foreach (var range in element.Ranges)
			if (range.From != Joiner)
				ranges.Add(range);
			else if (range.To != Joiner)
				ranges.Add(new CharRange((char)(Joiner + 1), range.To));

		return element with { Ranges = ranges };
	}

	/// <summary>
	/// A positive class that calls a predicate of the host, or names a category that holds U+0000,
	/// in a grammar that names <c>eos</c>: what the predicate says of the joiner is not the
	/// grammar's to know, and the category takes it, so the class is read only where the joiner is
	/// not.
	/// </summary>
	Node SegmentedPredicates(Node.Element element)
	{
		return _segmented && !element.IsNegated && (element.References.Count > 0 || element.Categories.Any(HoldsJoiner))
			? new Node.Sequence([new Node.Lookahead(false, new Node.Literal(Joiner.ToString())) { BeforeSegmentEnd = true }, element])
			: element;
	}

	/// <summary>Whether a Unicode category holds U+0000, the joiner.</summary>
	static bool HoldsJoiner(string category)
	{
		return UnicodeCategories.Expand(category).Contains("Control");
	}

	/// <summary>A literal in a grammar that names <c>eos</c> may not hold the joiner: it could never match.</summary>
	string Segmented(string value, Expr literal)
	{
		if (_segmented && value.IndexOf(Joiner) >= 0)
			Report(
				JoinerInSegmentedGrammar,
				$"'{literal}' holds U+0000, which a grammar that names 'eos' reads as the end of a segment and " +
				"never as a character of one: no segment holds it, so the literal never matches. Match the end " +
				"of a segment with 'eos' (docs/syntax.md §3.1.1).",
				literal.At);

		return value;
	}

	/// <summary>
	/// Every kind of node the checks of text over segments below classify. Each switch below names
	/// every one of them, and a kind not named stops the compiler there rather than being taken for
	/// one that keeps nothing.
	/// </summary>
	/// <remarks>
	/// Held by a test to every kind <see cref="Node"/> declares, so that a kind added to it fails
	/// that test until it is classified here and in each switch.
	/// </remarks>
	public static readonly IReadOnlyList<Type> SegmentClassifiedNodes =
	[
		typeof(Node.Empty), typeof(Node.Element), typeof(Node.Literal), typeof(Node.Sequence), typeof(Node.Choice),
		typeof(Node.Atomic), typeof(Node.Marked), typeof(Node.Repeat), typeof(Node.Lookahead), typeof(Node.Behind),
		typeof(Node.Reading), typeof(Node.Glue), typeof(Node.Capture), typeof(Node.Bind), typeof(Node.Same),
		typeof(Node.Counted), typeof(Node.Condition), typeof(Node.Guard), typeof(Node.Construct), typeof(Node.External),
		typeof(Node.Call),
	];

	/// <summary>
	/// Every member of the graph's records, and every table of the normalizer, that holds a node,
	/// with how the checks of text over segments reach what it holds.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A tree that hangs off a node without being one of its <see cref="Node.Children"/> is read by
	/// the parser all the same, and a walk of the bodies does not see it: a recovery's
	/// synchronization was one, and its guards were handed text over a boundary unchecked. Held by a
	/// test to every such member the assembly declares, so one added later fails that test until it
	/// is said here how it is reached.
	/// </para>
	/// <para>
	/// <c>Walked</c>: the checks walk it as a tree a rule reads. <c>Child</c>: it is a node's
	/// child, reached by every walk. <c>Index</c>: it names nodes that stand in a walked tree
	/// already, or answers something about them. <c>Lowering</c>: it is read while rules are
	/// lowered, and what it holds is in a body by the time the checks run. <c>Decided</c>: it is
	/// answered while the parser is generated and never read as input. <c>Boundary</c>: the class
	/// of the <c>wordboundary</c> rule, read behind a keyword as that rule's body is read, which
	/// was narrowed to leave the joiner out when it was lowered and calls no predicate (a class that
	/// does is no plain class, and no look behind is made of it).
	/// </para>
	/// </remarks>
	public static readonly IReadOnlyDictionary<string, string> SegmentClassifiedHolders = new Dictionary<string, string>(StringComparer.Ordinal)
	{
		["GrammarNormalizer._bodies"]             = "Walked",
		["Recovery.Sync"]                         = "Walked",

		["Node.Atomic.Body"]                      = "Child",
		["Node.Bind.Body"]                        = "Child",
		["Node.Call.Arguments"]                   = "Child",
		["Node.Capture.Body"]                     = "Child",
		["Node.Choice.Nodes"]                     = "Child",
		["Node.Construct.Body"]                   = "Child",
		["Node.Counted.Body"]                     = "Child",
		["Node.Lookahead.Body"]                   = "Child",
		["Node.Marked.Body"]                      = "Child",
		["Node.Repeat.Body"]                      = "Child",
		["Node.Sequence.Nodes"]                   = "Child",
		["Node.SwitchSelection.Selector"]         = "Child",

		["Node.Behind.Test"]                      = "Boundary",
		["Test.Meets.Left"]                       = "Decided",
		["Test.Meets.Right"]                      = "Decided",

		// The loop a left recursion became is the last part of the rule's body.
		["Fold.Accumulators"]                     = "Index",
		["Fold.Loop"]                             = "Index",
		// Keyed by the recovering repetition; what it holds is its synchronization, above.
		["GrammarNormalizer._recoveries"]         = "Index",
		// A call of the trivia rule, with no arguments: that rule's body is walked with the rest.
		["GrammarNormalizer._trivia"]             = "Index",
		["GrammarNormalizer._annotations"]        = "Index",
		["GrammarNormalizer._argumentOrigins"]    = "Index",
		["GrammarNormalizer._boundBodies"]        = "Index",
		["GrammarNormalizer._boundaryCharacters"] = "Index",
		["GrammarNormalizer._bounds"]             = "Index",
		["GrammarNormalizer._ceilings"]           = "Index",
		["GrammarNormalizer._climbing"]           = "Index",
		["GrammarNormalizer._forwarded"]          = "Index",
		["GrammarNormalizer._powers"]             = "Index",
		["GrammarNormalizer._refused"]            = "Index",
		["GrammarNormalizer._repetitionsAt"]      = "Index",
		["GrammarNormalizer._withRoots"]          = "Index",

		["GrammarNormalizer._arguments"]          = "Lowering",
		["GrammarNormalizer._callArguments"]      = "Lowering",
		["GrammarNormalizer._origins"]            = "Lowering",
		["GrammarNormalizer.WithSite.Root"]       = "Lowering",
	};

	/// <summary>What a switch below is told of a kind of node it does not name.</summary>
	static InvalidOperationException Unclassified(Node node)
	{
		return new InvalidOperationException(
			$"'{node.GetType().Name}' is not classified by the checks of text over segments (SegmentClassifiedNodes).");
	}

	/// <summary>
	/// Text kept across an <c>eos</c>: a capture or a binding of text, a look's included, a
	/// publication whose value is the text it matched, <c>parserText</c> of what may hold a
	/// boundary, the text a recovery hands its hook, and <c>parserInput</c> anywhere.
	/// </summary>
	/// <remarks>
	/// A boundary has no character, so text that spans one would be the segments' characters with
	/// nothing where the boundary was — or the joiner, which is no character of the input. Refused
	/// until there is a reason to choose. A value built on either side of a boundary is a value and
	/// may be kept: <c>rows: (Row &amp; eos)*</c> holds rows, not text. A recognizer of the host is
	/// handed its own segment and nothing else (<see cref="Emit.CSharpEmitter.SegmentOf"/>), so
	/// neither what it reads nor the value it makes of it can hold one.
	/// </remarks>
	void CheckSegmentedText()
	{
		if (!_segmented)
			return;

		var reaching = RulesReachingEndOfSegment();

		foreach (var rule in _rules)
		{
			if (!_bodies.TryGetValue(rule, out var body) || rule.Declaration is not { } declared)
				continue;

			foreach (var (tree, after) in ReadTrees(body))
			{
				foreach (var node in Running(tree))
					if (KeptText(node) is ({ } kept, { } what) && KeepsEndOfSegment(kept, reaching))
						Report(
							TextAcrossSegments,
							$"{what} in '{rule.Name}' may hold an 'eos', and the end of a segment is no text: a " +
							"capture is a piece of one segment. Capture the parts on either side of it, or a value " +
							"built from them (docs/syntax.md §3.1.1).",
							declared.At);

				CheckSuppliedText(tree, after, rule, declared.At, reaching, []);
			}
		}

		// A rejected element's text runs from where it began to the first synchronization after
		// that, which, where it is `eos`, is the end of the segment it began in. Anything else may
		// be found past the end of that segment, or be the end of a segment and more.
		foreach (var pair in _recoveries)
		{
			var recovery = pair.Value;

			if (NeverRuns(pair.Key))
				continue;

			if (recovery.Factory is not { } factory)
			{
				// OnRecovered, and a streamed reading's rejection, are handed that text whether or
				// not anything listens: whether the host implements the hook is not known here.
				if (!EndsSegment(recovery.Sync))
					Report(
						TextAcrossSegments,
						"A 'recover' without a '=>' hands OnRecovered the text of the element it steps over, up " +
						"to where it synchronizes, which may hold an 'eos', and the end of a segment is no text. " +
						"Synchronize on 'eos', which stops at the end of the element's own segment, or build the " +
						"rejection with a '=>' (docs/syntax.md §3.1.1).",
						recovery.At);
			}
			else if (Names(factory, "parserInput"))
				Report(TextAcrossSegments, InputAcrossSegments, recovery.At);
			else if (Names(factory, "parserText") && !EndsSegment(recovery.Sync))
				Report(
					TextAcrossSegments,
					"'parserText' in this 'recover' is the text of a rejected element up to where it " +
					"synchronizes, which may hold an 'eos', and the end of a segment is no text. Read it where " +
					"the synchronization is 'eos', which stops at the end of the element's own segment " +
					"(docs/syntax.md §3.1.1).",
					recovery.At);
		}

		foreach (var publication in _publications)
			if (!BuildsValue(publication.Rule) && ReachesEndOfSegment(new Node.Call(publication.Rule, []), reaching))
				Report(
					TextAcrossSegments,
					$"'{publication.Rule.Name}' has no value of its own, so what it publishes is the text it " +
					"matched, and it may hold an 'eos': the end of a segment is no text. Give the rule a value " +
					"(docs/syntax.md §3.1.1).",
					publication.At);
	}

	/// <summary>
	/// The trees a rule's reading runs: its body, and the synchronization of every repetition in it
	/// that recovers — one in a synchronization included, and none that never runs — each with
	/// whether what the rule read before it may hold an <c>eos</c>.
	/// </summary>
	/// <remarks>
	/// A synchronization is not a child of its repetition (<see cref="Recovery.Sync"/>), so a walk
	/// of the body does not reach it, yet it is read in the rule as the body is: its guards see the
	/// rule's text so far and its captures are the rule's. It is read after the element it steps
	/// over, whose scan goes on past the end of a segment as past any character, so what stands
	/// before it may always hold one.
	/// </remarks>
	IEnumerable<(Node Tree, bool After)> ReadTrees(Node body)
	{
		var pending = new Stack<(Node Tree, bool After)>();
		var seen    = new HashSet<Node>(NodeIdentity.Instance);

		pending.Push((body, false));

		while (pending.Count > 0)
		{
			var read = pending.Pop();

			yield return read;

			foreach (var node in Running(read.Tree))
				if (_recoveries.TryGetValue(node, out var recovery) && seen.Add(recovery.Sync))
					pending.Push((recovery.Sync, true));
		}
	}

	/// <summary>What a construction, a guard or a recovery that reads <c>parserInput</c> is told.</summary>
	const string InputAcrossSegments =
		"'parserInput' would be the segments read as one text, with nothing where each one ends, and " +
		"that is no input the caller handed over: a grammar that names 'eos' does not supply it. Read " +
		"the parts you need with captures (docs/syntax.md §3.1.1).";

	/// <summary>
	/// The text a node keeps for the host, and what it is called in a message, or null where it
	/// keeps none of its own.
	/// </summary>
	/// <remarks>
	/// Only what is kept as text: what a construction or a guard reads is
	/// <see cref="CheckSuppliedText"/>'s to answer, and a value is not text.
	/// </remarks>
	(Node Kept, string What)? KeptText(Node node)
	{
		switch (node)
		{
			// A capture of a value keeps the value.
			case Node.Capture(var name, var captured):
				return HoldsValue(captured) ? null : (captured, $"The capture '{name}'");

			case Node.Bind(var binding, var bound, _):
				return (bound, $"The binding '${binding.Name}'");

			// What reads a binding again, or counts it, reads the text the binding is held to.
			case Node.Same or Node.Counted:
				return null;

			// Handed its own segment and nothing else, and a value made of that.
			case Node.External:
				return null;

			case Node.Construct or Node.Guard or Node.Condition or Node.Marked:
				return null;

			case Node.Empty or Node.Element or Node.Literal or Node.Sequence or Node.Choice or Node.Atomic
				or Node.Repeat or Node.Lookahead or Node.Behind or Node.Reading or Node.Glue or Node.Call:
				return null;

			default:
				throw Unclassified(node);
		}
	}

	/// <summary>
	/// What a capture or a binding keeps: the text its operand matched, which for a positive
	/// lookahead is the text the look read, though the look keeps the position where it was.
	/// </summary>
	bool KeepsEndOfSegment(Node kept, HashSet<RuleSymbol> reaching)
	{
		return kept switch
		{
			Node.Lookahead(true, var seen) => ReachesEndOfSegment(seen, reaching),
			Node.Atomic(var body)          => KeepsEndOfSegment(body, reaching),
			Node.Sequence([var only])      => KeepsEndOfSegment(only, reaching),
			_                              => ReachesEndOfSegment(kept, reaching),
		};
	}

	/// <summary>
	/// The constructions and guards of a rule that read <c>parserText</c> of what they stand over
	/// where that may hold an <c>eos</c>, and any that reads <c>parserInput</c>.
	/// </summary>
	/// <param name="before">
	/// Whether what the rule read before <paramref name="node"/> may hold an <c>eos</c>: a guard's
	/// <c>parserText</c> is what its rule has matched so far.
	/// </param>
	/// <param name="inPlace">
	/// The rules read in place so far, with what stood before them, so that a rule called from
	/// many places, or from itself, is read once for each.
	/// </param>
	void CheckSuppliedText(
		Node node, bool before, RuleSymbol rule, Location at, HashSet<RuleSymbol> reaching, HashSet<(RuleSymbol, bool)> inPlace)
	{
		switch (node)
		{
			case Node.Guard(var condition, _):
				Supplied(condition, before, "a 'when'");
				return;

			// What a construction is handed as `parserText` is its rule's text from where the rule
			// began, and a left recursion's step is read after its seed and the steps before it
			// (§4.3): what stood before the construction is in that text as well.
			case Node.Construct(var body, var how):
				CheckSuppliedText(body, before, rule, at, reaching, inPlace);

				if (how is Construction.Expression(var built, _))
					Supplied(built, before || ReachesEndOfSegment(body, reaching), "a '=>'");

				return;

			case Node.Marked(var body, var state):
				CheckSuppliedText(body, before, rule, at, reaching, inPlace);
				Supplied(state, before || ReachesEndOfSegment(body, reaching), "a 'with state'");
				return;

			case Node.Sequence(var parts):
			{
				var read = before;

				foreach (var part in parts)
				{
					CheckSuppliedText(part, read, rule, at, reaching, inPlace);
					read = read || ReachesEndOfSegment(part, reaching);
				}

				return;
			}

			// A later turn's guard has read the turns before it, and what a recovery stepped over
			// between them: the repetition as a whole, recovery and all, stands before every turn.
			// What a repetition that never runs would read is not read.
			case Node.Repeat or Node.Counted when NeverRuns(node):
				return;

			// A repetition whose most is a written one has no turn before its only one: that turn
			// reads after what stood before the repetition and nothing else, recovery or not —
			// what a recovery steps over comes after the turn that broke.
			case Node.Repeat(var body, _, 1):
				CheckSuppliedText(body, before, rule, at, reaching, inPlace);
				return;

			case Node.Counted(var body, _, CountBound.Items { Count: 1 }):
				CheckSuppliedText(body, before, rule, at, reaching, inPlace);
				return;

			case Node.Repeat(var body, _, _):
				CheckSuppliedText(body, before || ReachesEndOfSegment(node, reaching), rule, at, reaching, inPlace);
				return;

			case Node.Counted(var body, _, _):
				CheckSuppliedText(body, before || ReachesEndOfSegment(node, reaching), rule, at, reaching, inPlace);
				return;

			// Another rule's guards and constructions stand over that rule's own text; what is
			// handed to it is read there, after whatever that rule read first.
			//
			// A rule that builds no value may be compiled where it is called (ExecutionPlan), and
			// a guard in it then reads the text of the rule it is written into, from where that
			// one began: it is read here as well, after what stood before the call.
			case Node.Call(var called, var arguments):
				foreach (var argument in arguments)
					CheckSuppliedText(argument, true, rule, at, reaching, inPlace);

				if (!called.IsBuiltIn && !BuildsValue(called) && _bodies.TryGetValue(called, out var inner) &&
					!NodeWalk.Descendants(inner).Any(static one => one is Node.Construct or Node.Bind) &&
					inPlace.Add((called, before)))
					CheckSuppliedText(inner, before, rule, at, reaching, inPlace);

				return;

			case Node.Choice or Node.Atomic or Node.Capture or Node.Bind or Node.Lookahead:
				foreach (var child in node.Children)
					CheckSuppliedText(child, before, rule, at, reaching, inPlace);

				return;

			// Decided while the parser is generated, and holds no C#.
			case Node.Condition:
				return;

			case Node.Empty or Node.Element or Node.Literal or Node.Behind or Node.Reading or Node.Glue
				or Node.Same or Node.External:
				return;

			default:
				throw Unclassified(node);
		}

		void Supplied(string text, bool reaches, string where)
		{
			if (Names(text, "parserInput"))
				Report(TextAcrossSegments, InputAcrossSegments, at);
			else if (reaches && Names(text, "parserText"))
				Report(
					TextAcrossSegments,
					$"'parserText' in {where} of '{rule.Name}' is text that may hold an 'eos', and the end of a " +
					"segment is no text. Capture the parts on either side of it, or read 'parserSpan', which " +
					"is two numbers (docs/syntax.md §3.1.1).",
					at);
		}
	}

	/// <summary>
	/// The node and everything under it that may be read: not the body of a repetition whose most
	/// is a written nought, nor what it would recover with.
	/// </summary>
	static IEnumerable<Node> Running(Node tree)
	{
		var pending = new Stack<Node>();

		pending.Push(tree);

		while (pending.Count > 0)
		{
			var node = pending.Pop();

			if (NeverRuns(node))
				continue;

			yield return node;

			foreach (var child in node.Children.Reverse())
				pending.Push(child);
		}
	}

	/// <summary>Whether a repetition's most is a written nought, so that it never runs, nor recovers.</summary>
	static bool NeverRuns(Node node)
	{
		return node is Node.Repeat(_, _, 0) or Node.Counted(_, _, CountBound.Items { Count: 0 });
	}

	/// <summary>
	/// Whether a synchronization is <c>eos</c> and nothing else: written so, or a call of a rule
	/// whose body is that and nothing else, through any chain of such rules.
	/// </summary>
	bool EndsSegment(Node sync)
	{
		var seen = new HashSet<RuleSymbol>();

		while (true)
		{
			switch (sync)
			{
				case Node.Call({ IsBuiltIn: true, Name: "eos" }, _) or Node.Literal([Joiner]):
					return true;

				case Node.Call({ IsBuiltIn: false } called, []) when seen.Add(called) && _bodies.TryGetValue(called, out var body):
					sync = body;
					break;

				default:
					return false;
			}
		}
	}

	/// <summary>
	/// Whether what a capture holds is a value built by a rule rather than text, as the capture's
	/// slot is laid out (<see cref="CaptureLayout.ValuedCall"/>).
	/// </summary>
	bool HoldsValue(Node captured)
	{
		return captured is Node.Construct || CaptureLayout.ValuedCall(captured, BuildsValue) is not null;
	}

	/// <summary>
	/// The rules whose reading may read an <c>eos</c>, in their own body or in a rule they call.
	/// </summary>
	/// <remarks>
	/// The least answer that holds of every rule at once: every rule starts as reaching none, and a
	/// pass over all of them adds those whose body reaches one by what is known so far, until a pass
	/// adds nothing. A rule that calls itself is answered by the rest of its body, and no answer is
	/// kept that a rule later in the walk could still change, so the order the rules are asked in,
	/// and the order they are published in, change nothing.
	/// </remarks>
	HashSet<RuleSymbol> RulesReachingEndOfSegment()
	{
		var reaching = new HashSet<RuleSymbol>();

		for (var changed = true; changed;)
		{
			changed = false;

			foreach (var pair in _bodies)
				if (!reaching.Contains(pair.Key) && ReachesEndOfSegment(pair.Value, reaching))
				{
					reaching.Add(pair.Key);
					changed = true;
				}
		}

		return reaching;
	}

	/// <summary>
	/// Whether reading a node may read an <c>eos</c>, in it or in a rule it calls, by what
	/// <paramref name="reaching"/> says of the rules. Not in a look, which reads past it and keeps
	/// nothing.
	/// </summary>
	bool ReachesEndOfSegment(Node node, HashSet<RuleSymbol> reaching)
	{
		switch (node)
		{
			case Node.Lookahead:
				return false;

			// A repetition whose most is a written nought never runs its body, and so never
			// recovers either. A count that is a parameter's is a number here already, the
			// instance's own, and a length is never nought ahead of time, so neither is taken for one.
			case Node.Repeat or Node.Counted when NeverRuns(node):
				return false;

			// A repetition that recovers steps over what it could not read up to where it
			// synchronizes, and both that scan and the synchronization go on past the end of a
			// segment as past any character: what is around it may hold one, whatever it
			// synchronizes on. What the recovery hands its own `=>` is answered apart.
			case Node.Repeat or Node.Counted when _recoveries.ContainsKey(node):
				return true;

			case Node.Repeat(var body, _, _):
				return ReachesEndOfSegment(body, reaching);

			case Node.Counted(var body, _, _):
				return ReachesEndOfSegment(body, reaching);

			case Node.Call(var rule, var arguments):
			{
				if (arguments.Any(argument => ReachesEndOfSegment(argument, reaching)))
					return true;

				if (rule.IsBuiltIn)
					return rule.Name == "eos";

				return reaching.Contains(rule);
			}

			// `eos` itself, once it is its body, and nothing an author wrote (GRAM4047).
			case Node.Literal(var text):
				return text.IndexOf(Joiner) >= 0;

			// A class reads the joiner only where it was left in: a negated one that does not name
			// it, or a positive one that does. A predicate or a category a class calls is read
			// behind a look that keeps it off the joiner (SegmentedPredicates).
			case Node.Element element:
				return element.IsNegated
					? !element.Ranges.Any(static range => range.From <= Joiner && Joiner <= range.To)
					: element.Ranges.Any(static range => range.From <= Joiner && Joiner <= range.To);

			// A reference matches what its binding matched, which may not hold one (KeptText).
			case Node.Same:
				return false;

			// Handed its own segment and nothing else.
			case Node.External:
				return false;

			case Node.Empty or Node.Behind or Node.Reading or Node.Glue or Node.Guard or Node.Condition:
				return false;

			case Node.Sequence or Node.Choice or Node.Atomic or Node.Marked or Node.Capture or Node.Bind
				or Node.Construct:
				return node.Children.Any(child => ReachesEndOfSegment(child, reaching));

			default:
				throw Unclassified(node);
		}
	}
}
