using System;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// What the graph is asked once it is built: everything a grammar can get wrong that
/// only the whole of it shows.
/// </summary>
public sealed partial class GrammarNormalizer
{
	/// <summary>A rule nothing reaches, reported once, where it was declared.</summary>
	/// <remarks>
	/// <para>
	/// What reaches a rule is a publication, a call from a rule already reached, or a
	/// rebinding naming it as a replacement — <c>with (Word = AsciiWord)</c> is the only
	/// thing in a grammar that reaches <c>AsciiWord</c>, and it reaches it as surely as a
	/// call would. A seam and a word boundary are calls like any other by the time this
	/// runs, so what <c>trivia</c> is made of is reached through <c>trivia</c>.
	/// </para>
	/// <para>
	/// The standard library's own names are never reported. Declaring <c>trivia</c> or
	/// <c>wordboundary</c> is configuration rather than a rule anything calls (§4.5,
	/// §4.6): a grammar that sets whitespace handling and then has no seam to weave it
	/// into has still said what it meant, and a grammar with no keyword in it has still
	/// said what a word is.
	/// </para>
	/// <para>
	/// A warning and not a refusal. An unreached rule is dead grammar — a rule renamed
	/// in one place and not the other, a publication deleted, an alternative rewritten
	/// past its last operand — and it is also what a grammar under construction looks
	/// like halfway through being written.
	/// </para>
	/// </remarks>
	void CheckUnused()
	{
		// A grammar that publishes nothing has no roots, so everything in it is unreached
		// and saying so of every rule says nothing. What that grammar is missing is a
		// publication, which is a different remark and not this one.
		if (_model.Publications.Count == 0)
			return;

		var reached = new HashSet<RuleSymbol>();
		var pending = new Stack<RuleSymbol>();

		foreach (var publication in _model.Publications)
			Enter(publication.Rule);

		foreach (var rule in _rules)
			if (Array.IndexOf(GrammarBinder.StandardLibrary, rule.Name) >= 0)
				Enter(rule);

		foreach (var replacement in Replacements())
			Enter(replacement);

		foreach (var merged in _mergedElements)
			Enter(merged);

		while (pending.Count > 0)
		{
			var rule = pending.Pop();

			if (!_bodies.TryGetValue(rule, out var body))
				continue;

			Follow(body);

			if (_trivia.TryGetValue(rule, out var seam))
				Follow(seam);
		}

		foreach (var rule in _rules)
			if (rule.Declaration is { } declared && !reached.Contains(rule) &&
				Array.IndexOf(GrammarBinder.StandardLibrary, rule.Name) < 0)
			{
				Remark(
					UnusedRule,
					$"Nothing reaches '{rule.Name}': no publication names it, no rule the grammar " +
					"reads calls it, and no rebinding puts it in place of one. Publish it, call it, " +
					"or delete it.",
					declared.At);
			}

		void Enter(RuleSymbol rule)
		{
			if (reached.Add(rule))
				pending.Push(rule);
		}

		void Follow(Node from)
		{
			foreach (var node in NodeWalk.Descendants(from))
			{
				if (node is Node.Call(var called, _))
					Enter(called);

				// A recovery's synchronizer hangs off the repetition rather than standing in
				// it, so walking the body alone never meets it.
				if (_recoveries.TryGetValue(node, out var recovery))
					Follow(recovery.Sync);
			}
		}
	}

	/// <summary>Every rule a rebinding anywhere in the grammar puts in place of another.</summary>
	IEnumerable<RuleSymbol> Replacements()
	{
		foreach (var site in _pendingWith)
			foreach (var replacement in site.Targets.Values)
				yield return replacement;

		foreach (var publication in _model.Publications)
			foreach (var replacement in publication.Rebindings.Values)
				yield return replacement;

		foreach (var replacement in Headers(_model.Root))
			yield return replacement;

		static IEnumerable<RuleSymbol> Headers(GrammarNamespace ns)
		{
			foreach (var rebinding in ns.OwnRebindings)
				yield return rebinding.Right;

			foreach (var nested in ns.Nested)
				foreach (var replacement in Headers(nested))
					yield return replacement;
		}
	}

	void Check()
	{
		var doors = Doors.ByRule(_rules, _bodies);

		foreach (var rule in _rules)
		{
			CheckRepetitions  (_bodies[rule], rule);
			CheckCaptures     (_bodies[rule], rule);
			CheckConstruction (rule);
			CheckLeftRecursion(rule);
			CheckRecovery     (rule);
		}

		CheckTrivia();
	}

	/// <summary>
	/// A <c>=&gt;</c> builds the rule's value, so it has to be somewhere that is the
	/// rule's value and there has to be a type for it to build.
	/// </summary>
	void CheckConstruction(RuleSymbol rule)
	{
		// Every built-in and every synthesized rule (a value-returning external
		// recognizer's own, §7.1) has no declaration to report against. A built-in is
		// never `declared` (never in `_types`), so it never reached a reporting branch
		// below anyway; a synthesized external rule *is* declared — its type is set
		// directly rather than through a `: @T` this rule reads — so without this guard
		// it would reach one and dereference a null `Declaration`.
		if (rule.Declaration is null)
			return;

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
	/// As many <c>recover</c>s per rule as it marks repetitions — except in a stream.
	/// </summary>
	/// <remarks>
	/// Each marked repetition is its own: its own sync, its own <c>=&gt;</c>, its own
	/// sequence for a rejection to arrive in, and its own plan in the arena, which has
	/// dispatched a recovery by plan since there was one plan. A streamed parse is the
	/// exception, and not for want of machinery: the driver steps over a bad element as
	/// it hands the good ones back, reading one repetition at a time, so a second
	/// <c>recover</c> in the rule it streams would be one that quietly does not happen —
	/// exactly the failure recovery exists to prevent.
	/// </remarks>
	/// <summary>The operand an alternative begins with, past whatever builds its value.</summary>
	static Node? Leading(Node alternative)
	{
		var body = alternative is Node.Construct(var built, _) ? built : alternative;

		return body is Node.Sequence(var parts) ? parts.Count > 0 ? parts[0] : null : body;
	}

	/// <summary>The same operand written twice, whatever the two captures of it are called.</summary>
	/// <remarks>
	/// The names may differ and usually do — the alternatives mean different things by it —
	/// and what is read twice is the same either way.
	/// </remarks>
	static bool SameShape(Node one, Node other) =>
		(one, other) switch
		{
			(Node.Capture(_, var a), Node.Capture(_, var b))       => SameShape(a, b),
			(Node.Call(var a, { Count: 0 }), Node.Call(var b, { Count: 0 })) => a == b,
			(Node.Literal a, Node.Literal b)                       => a == b,
			(Node.Element a, Node.Element b)                       => SameSet(a, b),
			(Node.Behind(var a), Node.Behind(var b))               => SameSet(a, b),
			(Node.Empty, Node.Empty)                               => true,
			(Node.Glue, Node.Glue)                                 => true,

			// A lookahead reads nothing, so two of the same shape are the same
			// condition on what follows and may be shared like anything else.
			(Node.Lookahead(var ap, var a), Node.Lookahead(var bp, var b))
				=> ap == bp && SameShape(a, b),
			(Node.Atomic(var a), Node.Atomic(var b))               => SameShape(a, b),

			// The marks themselves are not compared: a mark changes nothing about what is
			// read, so two alternatives that differ only in one really do share a prefix.
			(Node.Marked(var a, _), Node.Marked(var b, _))         => SameShape(a, b),

			_                                                      => false,
		};

	/// <summary>The same set of characters, compared by what is in it.</summary>
	/// <remarks>
	/// <see cref="Node.Element"/> is a record whose ranges are a list, so its own equality
	/// compares that list by reference — two identical sets written twice are never equal
	/// by it, which is exactly the case this asks about.
	/// </remarks>
	static bool SameSet(Node.Element one, Node.Element other) =>
		one.IsNegated == other.IsNegated &&
		one.Ranges.SequenceEqual(other.Ranges) &&
		one.Categories.SequenceEqual(other.Categories) &&
		one.References.SequenceEqual(other.References);

	/// <summary>Whether reading this can reach that rule again, so the cost compounds.</summary>
	bool Reaches(Node from, RuleSymbol rule)
	{
		var pending = new Stack<RuleSymbol>();
		var walked  = new HashSet<RuleSymbol>();

		foreach (var node in NodeWalk.Descendants(from))
			if (node is Node.Call(var called, _))
				pending.Push(called);

		while (pending.Count > 0)
		{
			var at = pending.Pop();

			if (at == rule)
				return true;

			if (!walked.Add(at) || !_bodies.TryGetValue(at, out var body))
				continue;

			foreach (var node in NodeWalk.Descendants(body))
				if (node is Node.Call(var called, _))
					pending.Push(called);
		}

		return false;
	}

	void CheckRecovery(RuleSymbol rule)
	{
		var found = 0;

		foreach (var node in NodeWalk.Descendants(_bodies[rule]))
			if (_recoveries.ContainsKey(node))
				found++;

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

	/// <summary>What a capture is not allowed to be, which is now one thing.</summary>
	/// <remarks>
	/// <para>
	/// A capture under a repetition used to be the other: text captured somewhere that was
	/// not the whole of what repeats was refused, because the value was the span from the
	/// first turn to the last and anything else in the loop lay inside it. The turns are
	/// joined now, so the shape is ordinary — and it was never refused as widely as it
	/// read, since the check looked only at the innermost repetition.
	/// </para>
	/// <para>
	/// Inside a lookahead a capture belongs to a machine of its own that answers yes or no
	/// and hands nothing back, and that is still not built.
	/// </para>
	/// </remarks>
	void CheckCaptures(Node node, RuleSymbol rule, bool inLookahead = false)
	{
		if (node is Node.Capture(var name, _))
		{
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
					$"'{name}' is one of the names the parser supplies to every '=>' and 'when' " +
					"(docs/syntax.md §7.3), so a capture may not take it. Every one of them begins " +
					"with 'parser', which is what that prefix is for.",
					rule.Declaration!.At);

			// A capture inside a repetition that is not the whole of what repeats used to be
			// refused here, on the grounds that the text of the turns could not be told from
			// the text between them. It can: each turn records an entry of its own, and §10's
			// value is those joined rather than the span they lie in. The refusal was also
			// never quite the rule it read as — it looked only at the innermost repetition,
			// so `(t: A+ & '-'){2}` passed it and answered with the dashes in.
			if (inLookahead)
				Report(
					UnbuiltCapture,
					$"'{name}' is captured inside a lookahead in '{rule.Name}', which is not built: " +
					"a lookahead consumes nothing and answers only whether it matched.",
					rule.Declaration!.At);
		}

		var lookings = inLookahead || node is Node.Lookahead;

		foreach (var child in node.Children)
			CheckCaptures(child, rule, lookings);
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

			case Node.Choice   (var nodes):          return nodes.Any(child => Reaches(child, target, seen));
			case Node.Capture  (_, var captured):    return Reaches(captured, target, seen);
			case Node.Construct(var built, _):       return Reaches(built, target, seen);
			case Node.Atomic   (var atomic):         return Reaches(atomic, target, seen);
			case Node.Marked   (var marked, _):      return Reaches(marked, target, seen);
			case Node.Repeat   (var repeated, _, _): return Reaches(repeated, target, seen);
			case Node.Lookahead(_, var ahead):       return Reaches(ahead, target, seen);

			default: return false;
		}
	}

	/// <summary>
	/// `trivia` has to accept empty input. That single condition is what lets it be
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
				"'trivia' must accept empty input: it is inserted between every pair of operands, and a required match would demand whitespace everywhere.",
				trivia.Declaration.At);
		}
	}
}
