using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>
/// Answers every <c>when A is B</c> and takes it out of the graph.
/// </summary>
/// <remarks>
/// <para>
/// The question is whether two recognizers have a string in common. It is asked of the
/// grammar rather than of the input, so it has an answer before anything is parsed — and
/// the point of asking it is that <c>with</c> substitutes a rule at every one of its uses,
/// so <c>parse S with (Version = "Sql2008")</c> gives every such question in that parser a
/// different answer from the one beside it.
/// </para>
/// <para>
/// Which is why it runs where it does: after all three substitution passes, so that what it
/// reads is what this parser will be, and before anything rewrites the shape of a rule. It
/// leaves nothing behind — a condition that holds becomes <see cref="Node.Empty"/>, and an
/// alternative whose condition failed is deleted. No parser tests any of this while it runs.
/// </para>
/// <para>
/// <b>Where the condition may stand.</b> As an operand of an alternative, which is where a
/// `when` is written and the only place deleting one means anything: an alternative is what
/// a grammar has to lose. Written inside a group, a repetition or a capture it is refused
/// rather than guessed at, because what it would mean there is a question nobody has asked
/// yet.
/// </para>
/// <para>
/// <b>What it can decide.</b> Both sides are reduced to the set of strings they accept,
/// following calls and unfolding choices and sequences of literals. That answers the
/// question this is for — a domain is a list of names and a range is another list — and
/// answers it exactly. Where a side is not a listable set of strings, the compiler says so
/// and keeps the alternative: refusing to decide out loud is the only honest answer, since
/// deciding it wrongly either way silently changes what the parser reads.
/// </para>
/// <para>
/// Intersection is decidable for wider languages than this — regular against regular is a
/// product automaton, and <see cref="LexicalAutomaton"/> already builds those. It is not
/// reached from here yet, and widening to it is a change to this file and nothing else.
/// </para>
/// </remarks>
public sealed partial class GrammarNormalizer
{
	/// <summary>How many strings a side may accept before this gives up on listing them.</summary>
	const int Listable = 4096;

	internal void DecideIntersections()
	{
		foreach (var rule in _bodies.Keys.ToList())
		{
			var body = _bodies[rule];

			if (!NodeWalk.Descendants(body).Concat([body]).Any(one => one is Node.Condition))
				continue;

			_bodies[rule] = Decided(rule, body);
		}
	}

	/// <summary>A rule's body with every alternative's condition answered.</summary>
	Node Decided(RuleSymbol rule, Node body)
	{
		if (body is not Node.Choice(var alternatives))
			return Kept(rule, body) ?? Nothing(rule, body);

		var left = new List<Node>(alternatives.Count);

		foreach (var alternative in alternatives)
			if (Kept(rule, alternative) is { } one)
				left.Add(one);

		return left.Count switch
		{
			0 => Nothing(rule, body),
			1 => left[0],
			_ => new Node.Choice(left),
		};
	}

	/// <summary>
	/// One alternative with its conditions answered, or null where one of them failed.
	/// </summary>
	/// <remarks>
	/// Through what an alternative wears rather than around it: a `=&gt;` and a binding power
	/// are written on the alternative and the condition is written among its operands, so
	/// the two are met on the way in and put back on the way out.
	/// </remarks>
	Node? Kept(RuleSymbol rule, Node alternative)
	{
		switch (alternative)
		{
			case Node.Construct(var built, var how):
				return Kept(rule, built) is { } made ? new Node.Construct(made, how) : null;
		}

		if (alternative is Node.Condition(var only, var where))
			return Left(only, where) is { } alone ? alone : null;

		if (alternative is not Node.Sequence(var operands))
			return Elsewhere(rule, alternative);

		var kept = new List<Node>(operands.Count);

		foreach (var operand in operands)
			switch (operand)
			{
				case Node.Condition(var test, var at):
					// What is left of it: nothing where it held outright, an ordinary guard
					// where a C# half of it survived, and no alternative at all where it
					// failed. `Empty` rather than dropped, so that an alternative made only
					// of conditions is still an alternative that matches.
					if (Left(test, at) is not { } residue)
						return null;

					kept.Add(residue);
					break;

				default:
					if (Elsewhere(rule, operand) is { } one)
						kept.Add(one);

					break;
			}

		return new Node.Sequence(kept);
	}

	/// <summary>
	/// A node that is not a condition, refused where a condition is hiding inside it.
	/// </summary>
	Node? Elsewhere(RuleSymbol rule, Node node)
	{
		if (!NodeWalk.Descendants(node).Any(one => one is Node.Condition))
			return node;

		Report(
			ConditionOutOfPlace,
			$"A `when … is …` in '{rule.Name}' stands inside a group, a repetition or a " +
			"capture. It is answered while the parser is built, and what is answered there is " +
			"whether an alternative is in this parser at all — so it belongs beside the " +
			"operands of one and nowhere else.",
			Where(node));

		return node;
	}

	/// <summary>A rule left with no alternative at all.</summary>
	/// <remarks>
	/// <para>
	/// Not an error, and not a warning either: a rule gone from this parser is what the
	/// condition was written to say, and a construct removed from a dialect has to be
	/// removable. A grammar written per version removes something in nearly every parser it
	/// publishes, and a warning each time is noise that a project building with warnings as
	/// errors cannot get past — which is how the first versioned publication of T-SQL met it.
	/// Said as information and not left unsaid, because a rule that vanishes silently is still
	/// worth a word to whoever goes looking: it is as often a condition nobody meant as a
	/// dialect nobody has.
	/// </para>
	/// <para>
	/// What it becomes is the body it had behind a negative lookahead of nothing, which fails
	/// wherever it is asked. The body and not a bare refusal, because everything downstream
	/// reads it: a rule declares a type and the `=&gt;` that builds one is written on the
	/// alternatives, so a rule emptied to a refusal is a rule that declares `: @int` and says
	/// nothing about how to build one. The alternatives are still there, still typed, and
	/// never reached.
	/// </para>
	/// </remarks>
	Node Nothing(RuleSymbol rule, Node body)
	{
		// At the rule's own declaration, which is where a reader goes to see what was removed;
		// a clone made by `with` keeps the declaration it was cloned from.
		var at = rule.Declaration?.At ?? Where(body);

		_whenSound.Add(new GramDiagnostic(
			EmptyAfterConditions,
			$"Every alternative of '{rule.Name}' is ruled out by a `when … is …` in this " +
			"parser, so it reads nothing here. That is what a removed construct looks like, " +
			"and it is only worth a second look where no removal was meant.",
			at.Position,
			at.Length,
			GramSeverity.Info));

		return body is Node.Choice(var alternatives)
			? new Node.Choice([.. alternatives.Select(Unreachable)])
			: Unreachable(body);
	}

	/// <summary>One alternative that can never be taken, and is otherwise as it was.</summary>
	/// <remarks>
	/// The refusal goes among the operands and not around the alternative: a `=&gt;` builds
	/// the rule's value and belongs at the end of an alternative, so wrapping one in anything
	/// puts it where it does not belong. Inside, it is the first thing asked and the last.
	/// </remarks>
	static Node Unreachable(Node alternative) => alternative switch
	{
		Node.Construct(var body, var how) => new Node.Construct(Unreachable(body), how),
		Node.Sequence(var operands)       =>
			new Node.Sequence([new Node.Lookahead(false, Node.Empty.Instance), .. operands.Select(Answered)]),
		_ => new Node.Sequence([new Node.Lookahead(false, Node.Empty.Instance), Answered(alternative)]),
	};

	/// <summary>
	/// An operand with its condition gone, whatever the condition said.
	/// </summary>
	/// <remarks>
	/// Nothing downstream knows what a condition is — it is this pass's node and this pass
	/// takes it out — so an alternative kept for its shape has to lose its conditions too,
	/// even though it is the conditions that made it unreachable.
	/// </remarks>
	static Node Answered(Node operand) =>
		operand is Node.Condition ? Node.Empty.Instance : operand;

	/// <summary>
	/// What a condition leaves behind: nothing, a guard to ask later, or no alternative.
	/// </summary>
	/// <remarks>
	/// The fold produces a residue rather than a verdict, which is what lets a condition and
	/// a C# guard stand in one `when`. A statically false half deletes the alternative and
	/// the C# beside it is never compiled into anything; a statically true half leaves the
	/// C# behind as an ordinary guard; and two C# halves are joined into one, bracketed,
	/// because what they were written with was `and` and not `&amp;&amp;`.
	/// </remarks>
	Node? Left(Test test, int at) =>
		Folded(test, at) is { } residue
			? residue.Text is { } text ? new Node.Guard(text, residue.At) : Node.Empty.Instance
			: null;

	/// <summary>A residue: the C# still to ask, or none. Null is the alternative going.</summary>
	readonly record struct Residue(string? Text, int At);

	Residue? Folded(Test test, int at)
	{
		switch (test)
		{
			case Test.Runs(var text, var where):
				return new Residue(text, where);

			case Test.Meets(var left, var right, var negated):
			{
				// Undecided is kept as though it held, which is what GRAM4021 says it does.
				var met = Answer(left, right, at);

				return met is null || met != negated ? new Residue(null, at) : null;
			}

			case Test.All(var left, var right):
			{
				if (Folded(left, at) is not { } ours || Folded(right, at) is not { } theirs)
					return null;

				return new Residue(Joined(ours.Text, theirs.Text, "&&"), Where(ours, theirs, at));
			}

			case Test.Any(var left, var right):
			{
				var ours   = Folded(left, at);
				var theirs = Folded(right, at);

				// One half true outright and the other is not asked — not even at run time,
				// since nothing it could answer would change the reading.
				if (ours is { Text: null } || theirs is { Text: null })
					return new Residue(null, at);

				if (ours is null)
					return theirs;

				if (theirs is null)
					return ours;

				return new Residue(
					Joined(ours.Value.Text, theirs.Value.Text, "||"), Where(ours.Value, theirs.Value, at));
			}

			default:
				return new Residue(null, at);
		}
	}

	/// <summary>Two pieces of C#, bracketed so that what joins them is what was written.</summary>
	static string? Joined(string? ours, string? theirs, string with) =>
		ours is null ? theirs
			: theirs is null ? ours
			: $"({ours}) {with} ({theirs})";

	/// <summary>Where the surviving C# was written, which is the first of it there is.</summary>
	static int Where(Residue ours, Residue theirs, int at) =>
		ours.Text is not null ? ours.At : theirs.Text is not null ? theirs.At : at;

	/// <summary>
	/// Whether the two have a string in common, or null where this compiler cannot say.
	/// </summary>
	bool? Answer(Node left, Node right, int at)
	{
		var ours   = Settled(Spellings(left, []));
		var theirs = Settled(Spellings(right, []));

		if (ours is not null && theirs is not null)
			return ours.Overlaps(theirs);

		Report(
			UndecidedCondition,
			"`is` asks whether two recognizers have a string in common, and this compiler can " +
			"answer that only where each side accepts a listable set of strings — a literal, a " +
			"choice of them, or a rule that is one. The alternative is kept, as though the " +
			"condition held.",
			new Location(at < 0 ? 0 : at, 0));

		return null;
	}

	/// <summary>One string a side may accept, and the looks it has still to pass.</summary>
	/// <remarks>
	/// A look reads nothing and asks about what stands beside it. Every keyword in a grammar
	/// with a <c>wordboundary</c> wears two — nothing of a word before it, and nothing after —
	/// and neither can be answered where it is met: a rule called from a sequence does not
	/// know what follows it there. So a spelling carries its looks, each with the offset it
	/// stands at, and they are asked once the whole string is written down.
	/// </remarks>
	readonly record struct Spelling(string Text, Look[] Looks);

	/// <summary>A lookahead or a lookbehind, and where in its spelling it stands.</summary>
	readonly record struct Look(int At, Node Test);

	static readonly Look[] NoLooks = [];

	/// <summary>
	/// The strings a side accepts, read as a whole input — or null where they are not a
	/// listable set, or where a look among them cannot be answered.
	/// </summary>
	/// <remarks>
	/// As a whole input because that is what the question is: two recognizers with a string
	/// in common. Nothing stands before a spelling and nothing after it, which is what gives
	/// every look it carries an answer — `"100" &amp; ?!wordboundary` holds at the end of the
	/// input, as it holds wherever the keyword is not the start of a longer word.
	/// </remarks>
	HashSet<string>? Settled(List<Spelling>? spellings)
	{
		if (spellings is null)
			return null;

		var all = new HashSet<string>(StringComparer.Ordinal);

		foreach (var one in spellings)
			switch (Passes(one.Looks, one.Text, 0))
			{
				case null:
					return null;

				case true:
					all.Add(one.Text);
					break;
			}

		return all;
	}

	/// <summary>
	/// Whether every look holds over this text, its offsets moved on by <paramref name="shift"/>.
	/// </summary>
	/// <remarks>One that fails settles it, whatever the others could not say.</remarks>
	bool? Passes(Look[] looks, string text, int shift)
	{
		bool? passes = true;

		foreach (var look in looks)
			switch (Holds(look.Test, text, shift + look.At))
			{
				case false:
					return false;

				case null:
					passes = null;
					break;
			}

		return passes;
	}

	/// <summary>Whether one look holds at this offset of a text that is all there is.</summary>
	bool? Holds(Node test, string text, int at) => test switch
	{
		// Only where the item before, if there is one, is outside the set.
		Node.Behind(var element) =>
			at == 0 ? true : Within(element, text[at - 1]) is { } inside ? !inside : null,

		Node.Lookahead(var positive, var body) =>
			Starts(body, text, at) is { } starts ? starts == positive : null,

		_ => null,
	};

	/// <summary>Whether a node matches some beginning of the text from this offset.</summary>
	bool? Starts(Node body, string text, int at)
	{
		// One item, which is what a word boundary comes down to.
		if (ElementOf(body) is { } element)
			return at < text.Length ? Within(element, text[at]) : false;

		if (Spellings(body, []) is not { } spellings)
			return null;

		bool? starts = false;

		foreach (var one in spellings)
			if (at + one.Text.Length <= text.Length &&
				string.CompareOrdinal(text, at, one.Text, 0, one.Text.Length) == 0)
				switch (Passes(one.Looks, text, at))
				{
					case true:
						return true;

					case null:
						starts = null;
						break;
				}

		return starts;
	}

	/// <summary>Whether a character is in an element's set, or null where the set is not known.</summary>
	static bool? Within(Node.Element element, char character) =>
		FirstSets.OfElement(element) is { Anything: false } characters
			? characters.Overlaps(FirstSets.First.Chars([new CharRange(character, character)]))
			: null;

	/// <summary>Every string a node accepts, or null where they are not a listable set.</summary>
	List<Spelling>? Spellings(Node node, HashSet<RuleSymbol> seen)
	{
		switch (node)
		{
			case Node.Empty:
			case Node.Glue:
				return [new Spelling("", NoLooks)];

			case Node.Lookahead or Node.Behind:
				return [new Spelling("", [new Look(0, node)])];

			// A literal is the one string it spells; one written `"v1"i` is every string
			// that spells it in any case, and those are listed rather than folded to one.
			// Raising only the insensitive side answered `"v1"i is "v1"` with false, which
			// is the plainest possible wrong answer.
			case Node.Literal(var text) literal:
				if (!literal.IgnoreCase)
					return [new Spelling(text, NoLooks)];

				return Cases(text) is { } cases ? [.. cases.Select(static one => new Spelling(one, NoLooks))] : null;

			case Node.Choice(var alternatives):
			{
				var all = new List<Spelling>();

				foreach (var one in alternatives)
				{
					if (Spellings(one, seen) is not { } some)
						return null;

					all.AddRange(some);

					if (all.Count > Listable)
						return null;
				}

				return all;
			}

			case Node.Sequence(var operands):
			{
				var all = new List<Spelling> { new("", NoLooks) };

				foreach (var one in operands)
				{
					if (Spellings(one, seen) is not { } some)
						return null;

					if (all.Count * (long)some.Count > Listable)
						return null;

					all = [.. from head in all from tail in some select Then(head, tail)];
				}

				return all;
			}

			// A call is the rule it names, and a rule that reaches itself accepts strings of
			// no bound — which is exactly what cannot be listed. Along one path and not over
			// the whole walk: `Digit & Digit` calls one rule twice and reaches nothing twice.
			case Node.Call(var rule, _):
			{
				if (!_bodies.TryGetValue(rule, out var body) || !seen.Add(rule))
					return null;

				var found = Spellings(body, seen);

				seen.Remove(rule);

				return found;
			}

			// The wrappers that change what is built rather than what is read.
			case Node.Capture(_, var inside):
				return Spellings(inside, seen);

			case Node.Atomic(var braced):
				return Spellings(braced, seen);

			case Node.Marked(var marked, _):
				return Spellings(marked, seen);

			case Node.Construct(var built, _):
				return Spellings(built, seen);

			default:
				return null;
		}
	}

	/// <summary>One spelling followed by another, the second's looks moved along by the first.</summary>
	static Spelling Then(Spelling head, Spelling tail) =>
		new(head.Text + tail.Text,
			tail.Looks.Length == 0
				? head.Looks
				: [.. head.Looks, .. tail.Looks.Select(look => look with { At = look.At + head.Text.Length })]);

	/// <summary>Every way a case-insensitive literal may be spelled, or null where too many.</summary>
	/// <remarks>
	/// Two to the power of its letters, which is why the same bound applies: a domain is a
	/// list of short names and this stays small, while `"internationalization"i` is a
	/// question this refuses rather than answers slowly.
	/// </remarks>
	static HashSet<string>? Cases(string text)
	{
		var letters = text.Count(char.IsLetter);

		if (letters > 12)
			return null;

		var all = new HashSet<string>(StringComparer.Ordinal) { "" };

		foreach (var one in text)
		{
			var lower = char.ToLowerInvariant(one);
			var upper = char.ToUpperInvariant(one);

			all = lower == upper
				? [.. all.Select(head => head + one)]
				: [.. all.SelectMany(head => new[] { head + lower, head + upper })];
		}

		return all;
	}

	/// <summary>Where a node was written, as well as the graph can say.</summary>
	static Location Where(Node node) =>
		node is Node.Condition(_, var at) && at >= 0 ? new Location(at, 0) : new Location(0, 0);
}
