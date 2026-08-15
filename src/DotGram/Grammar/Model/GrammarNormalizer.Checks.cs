using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>
/// What the graph is asked once it is built: everything a grammar can get wrong that
/// only the whole of it shows.
/// </summary>
public sealed partial class GrammarNormalizer
{
	void Check()
	{
		foreach (var rule in _rules)
		{
			CheckRepetitions(_bodies[rule], rule);
			CheckCaptures(_bodies[rule], rule, repeated: null);
			CheckConstruction(rule);
			CheckLeftRecursion(rule);
			CheckRecovery(rule);
		}

		CheckTrivia();
	}

	/// <summary>
	/// What a capture under a repetition is allowed to be, and what it is not yet.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A repeated capture of a rule that builds is a sequence of values (§7.3), and where
	/// it sits under the repetition does not matter: every iteration appends where its
	/// value is built, and an abandoned attempt truncates back to the length it pushed.
	/// </para>
	/// <para>
	/// A repeated capture of <b>text</b> is not that. §10 binds a capture tighter than a
	/// quantifier, so <c>scheme: ['a'..'z']+</c> is a capture repeated, and §7.3 gives it
	/// the text joined — which is the extent from the first iteration to the last, and is
	/// exactly that only when the capture is the whole of what repeats. Written around
	/// something else, the text between the iterations would be swept in with them.
	/// </para>
	/// <para>
	/// Inside a lookahead a capture belongs to a machine of its own that answers yes or no
	/// and hands nothing back.
	/// </para>
	/// </remarks>
	/// <param name="repeated">
	/// What the innermost enclosing repetition repeats, or null when there is none. A
	/// repetition bounded at one iteration does not count: what is under it is written at
	/// most once, which is an option rather than a run.
	/// </param>
	/// <summary>
	/// A <c>=&gt;</c> builds the rule's value, so it has to be somewhere that is the
	/// rule's value and there has to be a type for it to build.
	/// </summary>
	void CheckConstruction(RuleSymbol rule)
	{
		var body     = _bodies[rule];
		var declared = _types.ContainsKey(rule);
		var offered  = Fold.Of(body, _folds.TryGetValue(rule, out var fold) ? fold : null);
		var building = 0;

		foreach (var alternative in offered)
			if (alternative is Node.Construct)
				building++;

		// Anywhere else it would be building the value of a group, and a group has no
		// value — the rule does.
		foreach (var construct in Constructs(body))
			if (!offered.Contains(construct))
			{
				Report(
					UnbuiltConstruction,
					$"A '=>' in '{rule.Name}' is not on an alternative of the rule. It builds the rule's " +
					"value, so it belongs at the end of one.",
					rule.Declaration!.At);

				return;
			}

		if (building > 0 && !declared)
			Report(
				UnbuiltConstruction,
				$"'{rule.Name}' says how to build its value with '=>' but does not say what type that is. " +
				"Declare one with ': @T'.",
				rule.Declaration!.At);

		// §4.1 case 4: a rule that builds nothing and captures nothing yields the extent it
		// matched, and `string` is what an extent is. Declaring that type says out loud
		// what a rule with no type says by default, and used to be refused for saying it.
		else if (declared && building == 0 && !HasCapture(body) &&
			string.Equals(_types[rule], "string", StringComparison.Ordinal))
		{
		}

		// Some alternatives say and some do not. The constructor of §7.3 is matched against
		// the rule rather than against one alternative — the captures it is filled from are
		// the rule's — so a rule that has begun answering the question has to finish.
		else if (declared && building > 0 && building < offered.Count)
			Report(
				UnbuiltConstruction,
				$"'{rule.Name}' says how to build its value on {building} of its {offered.Count} " +
				"alternatives and not on the rest. Give every alternative a '=>', or none of " +
				$"them and let the captures fill a constructor of '{_types[rule]}' (§7.3).",
				rule.Declaration!.At);

		else if (declared && building < offered.Count)
			Report(
				UnbuiltConstruction,
				$"'{rule.Name}' declares a type and does not say how to build it. Only ': @string' " +
				"can be left to the shape of the rule — §4.1 case 4, the extent it matched. " +
				(HasCapture(body)
					? $"No constructor of '{_types[rule]}' has every parameter covered by a capture " +
						"of this rule, which is what §7.3 matches against, so give every alternative " +
						"a '=>'."
					: $"Give every alternative a '=>', or declare '{rule.Name}' as ': @string'."),
				rule.Declaration!.At);
	}

	/// <summary>
	/// One <c>recover</c> per rule, for now.
	/// </summary>
	/// <remarks>
	/// The machine keeps one recovering repetition and would ignore a second — and a
	/// <c>recover</c> that is quietly not there is exactly the failure recovery exists to
	/// prevent. Two of them is a rule that wants splitting in two anyway.
	/// </remarks>
	void CheckRecovery(RuleSymbol rule)
	{
		var found = 0;

		foreach (var node in NodeWalk.Descendants(_bodies[rule]))
			if (_recoveries.ContainsKey(node))
				found++;

		if (found > 1)
			Report(
				UnbuiltRecovery,
				$"'{rule.Name}' marks {found} repetitions with 'recover' and only one may be marked. " +
				"Give the other its own rule.",
				rule.Declaration!.At);

		foreach (var node in NodeWalk.Descendants(_bodies[rule]))
			if (_recoveries.TryGetValue(node, out var recovery) && recovery.Factory is not null)
				CheckRecoveredElement(rule, node);
	}

	/// <summary>
	/// A <c>recover</c> with a <c>=&gt;</c> needs a sequence to put the result in.
	/// </summary>
	/// <remarks>
	/// §8.2's whole design is that a rejection arrives in the same sequence as the records,
	/// in its place — which presumes there is one. A repetition of something that builds no
	/// value collects text rather than values: <c>rows: Row*</c> where <c>Row</c> has no
	/// captures is one string, the run joined (§7.3), and there is nowhere for a rejection
	/// to go. Left alone it emitted a factory call against a list that does not exist,
	/// which the consumer's compiler reported as an undefined name in a file they never
	/// wrote.
	/// </remarks>
	void CheckRecoveredElement(RuleSymbol rule, Node repetition)
	{
		if (repetition is not Node.Repeat(var repeated, _, _))
			return;

		var element = repeated is Node.Capture(_, var captured) ? captured : repeated;

		if (element is Node.Call(var called, _) && BuildsValue(called))
			return;

		Report(
			UnbuiltRecovery,
			$"'{rule.Name}' recovers with a '=>', which puts the rejected element in the same " +
			"sequence as the ones that were read — but this repetition collects text rather " +
			"than values, so there is no sequence to put it in. Give the repeated rule a " +
			"capture of its own, or drop the '=>' and report out of band (docs/syntax.md §8.3).",
			rule.Declaration!.At);
	}

	bool IsFoldLoop(RuleSymbol rule, Node node) =>
		_folds.TryGetValue(rule, out var fold) && ReferenceEquals(fold.Loop, node);

	/// <summary>What the rule offers: its alternatives, or the body when it offers one.</summary>
	static IReadOnlyList<Node> Alternatives(Node body) =>
		body is Node.Choice(var alternatives) ? alternatives : [body];

	static IEnumerable<Node> Constructs(Node node)
	{
		if (node is Node.Construct)
			yield return node;

		foreach (var child in node.Children)
			foreach (var found in Constructs(child))
				yield return found;
	}

	void CheckCaptures(Node node, RuleSymbol rule, Node? repeated, bool inLookahead = false)
	{
		if (node is Node.Capture(var name, var captured))
		{
			var collects = captured is Node.Call(var called, _) && BuildsValue(called);

			// The supplied names of §7.3 and §8.2 become parameters of the method a `=>`
			// turns into, so a capture of the same name wants a parameter that is already
			// taken. The prefix makes that unlikely; this is what happens when an author
			// writes one anyway. Refused rather than resolved either way round:
			// `parserText` would otherwise mean the matched extent in one rule and
			// something else in the next, and the alternative — generated code that does
			// not compile — points at a file the author did not write.
			if (Recovery.Supplied.Contains(name))
				Report(
					ReservedCaptureName,
					$"'{name}' is one of the names the parser supplies to every '=>' and 'where' " +
					"(docs/syntax.md §7.3), so a capture may not take it. Every one of them begins " +
					"with 'parser', which is what that prefix is for.",
					rule.Declaration!.At);

			if (inLookahead)
				Report(
					UnbuiltCapture,
					$"'{name}' is captured inside a lookahead in '{rule.Name}', which is not built: " +
					"a lookahead consumes nothing and answers only whether it matched.",
					rule.Declaration!.At);

			else if (repeated is not null && !collects && !ReferenceEquals(repeated, node))
				Report(
					UnbuiltCapture,
					$"'{name}' captures text inside a repetition in '{rule.Name}' without being the whole of " +
					"what repeats, which is not built yet: the text of the iterations cannot be told from " +
					"the text between them. Move the quantifier inside the capture.",
					rule.Declaration!.At);
		}

		// The fold loop is the generator's, not the author's: a capture under it is
		// consumed by the fold on the iteration that wrote it (§4.3).
		var inside = node is Node.Repeat(var body, _, not 1) && !IsFoldLoop(rule, node)
			? body
			: repeated;

		var lookings = inLookahead || node is Node.Lookahead;

		foreach (var child in node.Children)
			CheckCaptures(child, rule, inside, lookings);
	}

	void CheckRepetitions(Node node, RuleSymbol rule)
	{
		if (node is Node.Repeat(var body, _, var max) && max != 1 && IsNullable(body))
			Report(
				NullableRepetition,
				$"The body of a repetition in '{rule.Name}' can match without consuming input, so the repetition would not terminate.",
				rule.Declaration!.At);

		foreach (var child in node.Children)
			CheckRepetitions(child, rule);
	}

	/// <summary>
	/// A rule that can reach itself without consuming anything first. Nullability is
	/// what makes this more than a syntactic check: `A = B &amp; A` is left-recursive
	/// exactly when `B` is nullable.
	/// </summary>
	void CheckLeftRecursion(RuleSymbol start)
	{
		// Direct left recursion is rewritten (§4.3), so what is left to refuse is what the
		// rewrite cannot take: a rule reaching itself through another one.
		if (Reaches(_bodies[start], start, []))
			Report(
				LeftRecursion,
				$"'{start.Name}' is left-recursive, which is not built yet (docs/syntax.md §4.3); " +
				"write the loop with a quantifier instead.",
				start.Declaration!.At);
	}

	bool Reaches(Node node, RuleSymbol target, HashSet<RuleSymbol> seen)
	{
		switch (node)
		{
			case Node.Call(var rule, _) when ReferenceEquals(rule, target):
				return true;

			case Node.Call(var rule, _):
				return seen.Add(rule) &&
					_bodies.TryGetValue(rule, out var body) &&
					Reaches(body, target, seen);

			case Node.Sequence(var nodes):

				foreach (var child in nodes)
				{
					if (Reaches(child, target, seen))
						return true;

					if (!IsNullable(child))
						return false;
				}

				return false;

			case Node.Choice(var nodes):        return nodes.Any(child => Reaches(child, target, seen));
			case Node.Capture(_, var captured): return Reaches(captured, target, seen);
			case Node.Construct(var built, _):  return Reaches(built, target, seen);
			case Node.Repeat(var repeated, _, _): return Reaches(repeated, target, seen);
			case Node.Lookahead(_, var ahead):  return Reaches(ahead, target, seen);

			default: return false;
		}
	}

	/// <summary>
	/// `Trivia` has to accept empty input. That single condition is what lets it be
	/// inserted everywhere without doubling (§4.5), so it is worth a message of its own.
	/// </summary>
	void CheckTrivia()
	{
		foreach (var trivia in _model.Trivia.Values.Distinct())
		{
			if (trivia.Declaration is null || IsNullable(_bodies[trivia]))
				continue;

			Report(
				TriviaNotNullable,
				"'Trivia' must accept empty input: it is inserted between every pair of operands, and a required match would demand whitespace everywhere.",
				trivia.Declaration.At);
		}
	}
}
