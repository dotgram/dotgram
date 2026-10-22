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
	/// A reader or a byte form asked of a grammar that names <c>eos</c>, which reads sequences of
	/// segments and nothing else.
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
	/// either, and a positive one that names it is refused.
	/// </summary>
	Node.Element Segmented(Node.Element element, Expr set)
	{
		if (!_segmented)
			return element;

		if (element.IsNegated)
			return element.Ranges.Any(static range => range.From <= Joiner && Joiner <= range.To)
				? element
				: element with { Ranges = Coalesce([new CharRange(Joiner, Joiner), .. element.Ranges]) };

		if (element.Ranges.Any(static range => range.From <= Joiner && Joiner <= range.To) ||
			element.Categories.Any(static category => UnicodeCategories.Expand(category).Contains("Control")))
			Report(
				JoinerInSegmentedGrammar,
				$"'{set}' holds U+0000, which a grammar that names 'eos' reads as the end of a segment and " +
				"never as a character of one: a segment that holds it is refused. Leave U+0000 out of the " +
				"class (docs/syntax.md §3.1.1).",
				set.At);

		return element;
	}

	/// <summary>
	/// A positive class that calls a predicate of the host, in a grammar that names <c>eos</c>:
	/// what the predicate says of the joiner is not the grammar's to know, so the class is read
	/// only where the joiner is not.
	/// </summary>
	Node SegmentedPredicates(Node.Element element)
	{
		return _segmented && !element.IsNegated && element.References.Count > 0
			? new Node.Sequence([new Node.Lookahead(false, new Node.Literal(Joiner.ToString())), element])
			: element;
	}

	/// <summary>A literal in a grammar that names <c>eos</c> may not hold the joiner.</summary>
	string Segmented(string value, Expr literal)
	{
		if (_segmented && value.IndexOf(Joiner) >= 0)
			Report(
				JoinerInSegmentedGrammar,
				$"'{literal}' holds U+0000, which a grammar that names 'eos' reads as the end of a segment and " +
				"never as a character of one: a segment that holds it is refused. Match the end of a segment " +
				"with 'eos' (docs/syntax.md §3.1.1).",
				literal.At);

		return value;
	}

	/// <summary>
	/// Text kept across an <c>eos</c>: a capture or a binding of text, or a publication whose value
	/// is the text it matched, that may hold a boundary.
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

		var reaching = new Dictionary<RuleSymbol, bool>();

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

				if (kept is not null && ReachesEndOfSegment(kept, reaching))
					Report(
						TextAcrossSegments,
						$"{what} in '{rule.Name}' may hold an 'eos', and the end of a segment is no text: a " +
						"capture is a piece of one segment. Capture the parts on either side of it, or a value " +
						"built from them (docs/syntax.md §3.1.1).",
						declared.At);
			}
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
	/// Whether what a capture holds is a value built by a rule rather than text, as the capture's
	/// slot is laid out (<see cref="CaptureLayout.ValuedCall"/>).
	/// </summary>
	bool HoldsValue(Node captured)
	{
		return captured is Node.Construct || CaptureLayout.ValuedCall(captured, BuildsValue) is not null;
	}

	/// <summary>
	/// Whether reading a node may read an <c>eos</c>, in it or in a rule it calls. Not in a look,
	/// which reads past it and keeps nothing.
	/// </summary>
	bool ReachesEndOfSegment(Node node, Dictionary<RuleSymbol, bool> reaching)
	{
		switch (node)
		{
			case Node.Lookahead:
				return false;

			case Node.Call(var rule, var arguments):
			{
				if (arguments.Any(argument => ReachesEndOfSegment(argument, reaching)))
					return true;

				if (rule.IsBuiltIn)
					return rule.Name == "eos";

				if (!reaching.TryGetValue(rule, out var reaches))
				{
					// Placed before the walk, so that a rule that reaches itself ends it.
					reaching[rule] = false;
					reaches        = _bodies.TryGetValue(rule, out var body) && ReachesEndOfSegment(body, reaching);
					reaching[rule] = reaches;
				}

				return reaches;
			}

			default:
				return node.Children.Any(child => ReachesEndOfSegment(child, reaching));
		}
	}
}
