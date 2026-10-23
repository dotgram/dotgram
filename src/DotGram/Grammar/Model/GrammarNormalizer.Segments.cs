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
	/// Text kept across an <c>eos</c>: a capture or a binding of text, a look's included, a
	/// publication whose value is the text it matched, or <c>parserText</c> of what may hold a
	/// boundary, and <c>parserInput</c> anywhere.
	/// </summary>
	/// <remarks>
	/// A boundary has no character, so text that spans one would be the segments' characters with
	/// nothing where the boundary was — or the joiner, which is no character of the input. Refused
	/// until there is a reason to choose. A value built on either side of a boundary is a value and
	/// may be kept: <c>rows: (Row &amp; eos)*</c> holds rows, not text.
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

			foreach (var node in NodeWalk.Descendants(body))
			{
				var (kept, what) = node switch
				{
					Node.Capture(var name, var captured) when !HoldsValue(captured) => (captured, $"The capture '{name}'"),
					Node.Bind(var binding, var bound, _) => (bound, $"The binding '${binding.Name}'"),
					_ => (null, null),
				};

				if (kept is not null && KeepsEndOfSegment(kept, reaching))
					Report(
						TextAcrossSegments,
						$"{what} in '{rule.Name}' may hold an 'eos', and the end of a segment is no text: a " +
						"capture is a piece of one segment. Capture the parts on either side of it, or a value " +
						"built from them (docs/syntax.md §3.1.1).",
						declared.At);
			}

			CheckSuppliedText(body, false, rule, declared.At, reaching);
		}

		foreach (var pair in _recoveries)
		{
			if (pair.Value.Factory is not { } factory)
				continue;

			if (Names(factory, "parserInput"))
				Report(TextAcrossSegments, InputAcrossSegments, pair.Value.At);
			// A rejected element's text runs from where it began to the first synchronization after
			// that, which, where it is `eos`, is the end of the segment it began in.
			else if (Names(factory, "parserText") && !EndsSegment(pair.Value.Sync))
				Report(
					TextAcrossSegments,
					"'parserText' in this 'recover' is the text of a rejected element up to where it " +
					"synchronizes, which may hold an 'eos', and the end of a segment is no text. Read it where " +
					"the synchronization is 'eos', which stops at the end of the element's own segment " +
					"(docs/syntax.md §3.1.1).",
					pair.Value.At);
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

	/// <summary>What a construction, a guard or a recovery that reads <c>parserInput</c> is told.</summary>
	const string InputAcrossSegments =
		"'parserInput' would be the segments read as one text, with nothing where each one ends, and " +
		"that is no input the caller handed over: a grammar that names 'eos' does not supply it. Read " +
		"the parts you need with captures (docs/syntax.md §3.1.1).";

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
	void CheckSuppliedText(Node node, bool before, RuleSymbol rule, Location at, HashSet<RuleSymbol> reaching)
	{
		switch (node)
		{
			case Node.Guard(var condition, _):
				Supplied(condition, before, "a 'when'");
				return;

			case Node.Construct(var body, var how):
				CheckSuppliedText(body, before, rule, at, reaching);

				if (how is Construction.Expression(var built, _))
					Supplied(built, ReachesEndOfSegment(body, reaching), "a '=>'");

				return;

			case Node.Sequence(var parts):
			{
				var read = before;

				foreach (var part in parts)
				{
					CheckSuppliedText(part, read, rule, at, reaching);
					read = read || ReachesEndOfSegment(part, reaching);
				}

				return;
			}

			// A later turn's guard has read the turns before it.
			case Node.Repeat(var body, _, _):
				CheckSuppliedText(body, before || ReachesEndOfSegment(body, reaching), rule, at, reaching);
				return;

			// Another rule's guards and constructions stand over that rule's own text.
			case Node.Call:
				return;

			default:
				foreach (var child in node.Children)
					CheckSuppliedText(child, before, rule, at, reaching);

				return;
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

	/// <summary>Whether a synchronization is <c>eos</c> and nothing else.</summary>
	static bool EndsSegment(Node sync)
	{
		return sync is Node.Call({ IsBuiltIn: true, Name: "eos" }, _) || sync is Node.Literal([Joiner]);
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

			// A repetition that recovers steps over what it could not read up to where it
			// synchronizes, and both that scan and the synchronization go on past the end of a
			// segment as past any character: what is around it may hold one, whatever it
			// synchronizes on. What the recovery hands its own `=>` is answered apart.
			case Node.Repeat when _recoveries.ContainsKey(node):
				return true;

			case Node.Call(var rule, var arguments):
			{
				if (arguments.Any(argument => ReachesEndOfSegment(argument, reaching)))
					return true;

				if (rule.IsBuiltIn)
					return rule.Name == "eos";

				return reaching.Contains(rule);
			}

			default:
				return node.Children.Any(child => ReachesEndOfSegment(child, reaching));
		}
	}
}
