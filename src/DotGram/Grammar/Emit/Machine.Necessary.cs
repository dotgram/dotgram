using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	/// <summary>
	/// The literals an alternative of a chained choice is tested for before it is gone into, or
	/// null where it is gone into as it stands.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A chain tests one character before an alternative and goes in on it; what going in costs is
	/// the frame and the setup of a rule that may be about to fail on its second character. A raw
	/// string beside a plain one is the shape: both begin with a quote, the raw one with three, and
	/// every plain string called the raw rule, opened its capture, read one quote and came back.
	/// Where an alternative calls a rule that necessarily begins with two or more fixed characters
	/// (<see cref="Necessarily"/>), and some later alternative does not begin with them, those
	/// characters are tested at the link, and only an input that holds them goes in.
	/// </para>
	/// <para>
	/// The test is the rule's own first reading, written again before the call: the same literals,
	/// compiled as the rule compiles them, failing as the rule fails on them — the same position,
	/// the same expectation, the same way out through <c>Fail:</c>, which takes the chain's entry
	/// back to the next alternative as it would have taken the call's and the rule's. So a refusal
	/// says what it said before, and only the entry into a rule that was going to fail is saved.
	/// An input that passes is read again from the start by the rule, which is what it costs.
	/// </para>
	/// <para>
	/// Not under tracing, where the trace shows every rule that was called and this would call
	/// one fewer; and only in the engine over characters, whose chain pushes the entry the test
	/// fails back to.
	/// </para>
	/// </remarks>
	IReadOnlyList<Node.Literal>? Guarded(IReadOnlyList<Node> alternatives, int at)
	{
		if (OverKinds || _lowering || Tracing is not null || at == alternatives.Count - 1)
			return null;

		var literals = Necessarily(alternatives[at], out var called);

		if (!called || literals.Sum(static literal => literal.Text.Length) < 2)
			return null;

		var prefix = string.Concat(literals.Select(static literal => literal.Text));

		// Worth testing only where an input that fails it has somewhere else to go: a later
		// alternative that does not begin with the same text. Where every later one does, the
		// first character's test the chain already makes is as much as one character can say.
		for (var later = at + 1; later < alternatives.Count; later++)
		{
			var theirs = string.Concat(Necessarily(alternatives[later], out _).Select(static literal => literal.Text));

			if (!theirs.StartsWith(prefix, System.StringComparison.Ordinal))
				return literals;
		}

		return null;
	}

	/// <summary>
	/// The case-sensitive literals an alternative necessarily begins with, in order, read through
	/// what consumes nothing before them and into the first rule called; and whether that rule is
	/// called rather than inlined.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Only shapes whose first reading is the literals' own, so that the test written from them is
	/// the reading the alternative makes: a literal; a count of a one-character literal, whose
	/// required turns are that literal compiled as many times, and whose run form refuses at the
	/// same position with the same expectation; a sequence, capture, binding or construction of those; and
	/// a call to a rule with no arguments, read into once and not past — the literals after a call
	/// are compiled in the caller, whose failures may leave by another door than the callee's.
	/// Anything else ends the reading: a choice, a class, a lookahead, a reference.
	/// </para>
	/// </remarks>
	List<Node.Literal> Necessarily(Node alternative, out bool called)
	{
		var literals = new List<Node.Literal>();

		called = false;
		Lead(alternative, literals, ref called, []);

		return literals;
	}

	/// <summary>
	/// Adds what <paramref name="node"/> necessarily begins with to <paramref name="literals"/>;
	/// true where the node was read whole and the reading may go on to what follows it.
	/// </summary>
	bool Lead(Node node, List<Node.Literal> literals, ref bool called, HashSet<RuleSymbol> entered)
	{
		switch (node)
		{
			case Node.Empty:
				return true;

			case Node.Literal { IgnoreCase: false, Text.Length: > 0 } literal:
				literals.Add(literal);
				return true;

			case Node.Capture(_, var body):
				return Lead(body, literals, ref called, entered);

			case Node.Construct(var body, _):
				return Lead(body, literals, ref called, entered);

			// A binding reads what its body reads, and opens nothing a failure in it would leave by.
			case Node.Bind(_, var body, _):
				return Lead(body, literals, ref called, entered);

			case Node.Sequence(var nodes):
				foreach (var part in nodes)
					if (!Lead(part, literals, ref called, entered))
						return false;

				return true;

			// The required turns of a count of one character are that character as many times; a
			// recovering repetition is compiled another way and is not read into.
			case Node.Repeat(Node.Literal { IgnoreCase: false, Text.Length: 1 } one, var min, var max) repeat
				when min > 0 && !_recoveries.ContainsKey(repeat):
				for (var turn = 0; turn < min; turn++)
					literals.Add(one);

				return max == min;

			// Into the rule's body, where it is read by this machine and not by a scanner, and
			// only as the first thing: literals before a call fail in the caller, those in the
			// callee fail in the callee, and the test is written one way.
			case Node.Call(var rule, { Count: 0 }) call
				when !Seam(call) && ScannerOf(rule) is null && literals.Count == 0 &&
					_graph.Bodies.TryGetValue(rule, out var body) && entered.Add(rule):
				if (!CanInline(rule))
					called = true;

				Lead(body, literals, ref called, entered);

				return false;

			default:
				return false;
		}
	}

	/// <summary>
	/// The test of <paramref name="literals"/> written before <paramref name="first"/>: each
	/// compiled as the alternative compiles it, failing as it would, and the position put back
	/// where every one of them was there.
	/// </summary>
	int CompileGuard(IReadOnlyList<Node.Literal> literals, int first, FollowSets.Continuation following)
	{
		var restored = Reserve(out var restoring);

		restoring.Line($"p -= {literals.Sum(static literal => literal.Text.Length)};");
		restoring.Line($"goto {Label(restoring, first)};");

		var target = restored;
		var saved  = _fail;

		// The literals stand in a called rule, whose body fails through `Fail:` whatever door the
		// choice itself would leave by; the chain's entry is what `Fail:` finds and resumes.
		_fail = Fail;

		for (var i = literals.Count - 1; i >= 0; i--)
			target = Compile(new Node.Literal(literals[i].Text) { IgnoreCase = literals[i].IgnoreCase }, target, following);

		_fail = saved;

		return target;
	}
}
