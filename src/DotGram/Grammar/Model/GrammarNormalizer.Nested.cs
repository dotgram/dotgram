using System;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

public sealed partial class GrammarNormalizer
{
	/// <summary>
	/// A repetition marked <c>recover</c> inside another one's element or synchronization, whose
	/// element would have to read, or be read by, something on the other side of the rule it is
	/// made into.
	/// </summary>
	public const string NestedRecoveryFlow = "GRAM4045";

	/// <summary>The rules made of a marked repetition that stood inside another one, and where from.</summary>
	readonly Dictionary<RuleSymbol, Lift> _lifted = [];

	/// <summary>
	/// Every marked repetition inside another one's element or synchronization becomes a rule of
	/// its own, called where it stood (§8.2).
	/// </summary>
	/// <remarks>
	/// <para>
	/// That is what it means: a marked repetition recovers as if it were its own rule in that
	/// place. Made into one, it is one, and a recovery runs in the rule that marks it: what it
	/// tries first is the whole continuation from there, and what its guards read is what it read
	/// itself. Outermost first, so that each rule made here is processed in its turn and the one
	/// inside it becomes a rule of its own as well, and nothing is ever nested in one rule again.
	/// </para>
	/// <para>
	/// It keeps the rule it came from: that rule's trivia, its declaration — and so its
	/// <c>on fail</c>, whether it gives back, and the name a trace and a message say — and, for one
	/// lifted out of an element, that rule's value. What the element captured is the enclosing
	/// rule's in the same slots it was written in (<see cref="RecognitionGraph.LiftedBody"/>), so an
	/// inline <c>@string[]</c> stays one flat sequence. One lifted out of a synchronization captures
	/// nothing anything reads, as a synchronization never has.
	/// </para>
	/// <para>
	/// Last, after every rule is in its final shape and every specialization made, so that every
	/// instance is lifted on its own and nothing that rewrites a body has to know about it. What a
	/// value crossing the new boundary would need — an outer capture read inside, an inner one read
	/// by a guard outside, a value built inside — is refused where the inner <c>recover</c> is
	/// written (<see cref="NestedRecoveryFlow"/>). A <c>recover</c> inside a lookahead was refused
	/// long before (<see cref="RecoveryInLookahead"/>), and nothing here looks inside one.
	/// </para>
	/// </remarks>
	void LiftNestedRecoveries()
	{
		if (_recoveries.Count < 2)
			return;

		for (var i = 0; i < _rules.Count; i++)
		{
			var rule = _rules[i];

			if (rule.Declaration is null || !_bodies.TryGetValue(rule, out var body))
				continue;

			foreach (var marked in Outermost(body))
				Lift(rule, marked);
		}
	}

	/// <summary>
	/// The marked repetitions under a node that stand inside no other under it, and not inside a
	/// lookahead.
	/// </summary>
	List<Node> Outermost(Node node)
	{
		var found = new List<Node>();

		Look(node);

		return found;

		void Look(Node at)
		{
			if (_recoveries.ContainsKey(at))
			{
				found.Add(at);

				return;
			}

			if (at is Node.Lookahead)
				return;

			foreach (var child in at.Children)
				Look(child);
		}
	}

	/// <summary>The marked repetitions inside one's element and its synchronization, each made a rule.</summary>
	void Lift(RuleSymbol rule, Node marked)
	{
		var repeat   = (Node.Repeat)marked;
		var recovery = _recoveries[marked];

		foreach (var inner in Outermost(repeat.Body))
		{
			if (Refused(rule, inner))
				continue;

			var call = LiftedCall(rule, inner, transparent: true);

			// The repetition is rebuilt around the call, and what was said of it goes with it.
			var rebuilt = Replaced(_bodies[rule], inner, call);

			_bodies[rule] = rebuilt;

			marked   = Found(rebuilt, call);
			repeat   = (Node.Repeat)marked;
			recovery = _recoveries[marked];
		}

		foreach (var inner in Outermost(recovery.Sync))
		{
			var call = LiftedCall(rule, inner, transparent: false);

			recovery = recovery with { Sync = Replaced(recovery.Sync, inner, call) };
		}

		_recoveries[marked] = recovery;
	}

	/// <summary>The marked repetition that holds a call, found again after the body was rebuilt.</summary>
	Node Found(Node body, Node call)
	{
		foreach (var node in NodeWalk.Descendants(body))
			if (_recoveries.ContainsKey(node) && NodeWalk.Descendants(((Node.Repeat)node).Body).Any(one => ReferenceEquals(one, call)))
				return node;

		throw new InvalidOperationException("A lifted call is in no marked repetition.");
	}

	/// <summary>The rule a marked repetition becomes, and the call that stands where it stood.</summary>
	Node.Call LiftedCall(RuleSymbol from, Node inner, bool transparent)
	{
		var origin = _lifted.TryGetValue(from, out var outer) ? outer.Origin : from;
		var count  = _lifted.Values.Count(one => one.Origin == origin) + 1;
		var name   = origin.Name + "_Nested" + count;

		while (_rules.Exists(one => one.Namespace == from.Namespace && one.Name == name))
			name += "_";

		// The declaration it came from, so that it says what that rule says and is named what that
		// rule is named wherever a name is told; no type, as its value is not its own.
		var declaration = from.Declaration! with
		{
			Params = [],
			Type   = null,
			At     = _recoveries[inner].At,
		};
		var lifted = new RuleSymbol(name, from.Namespace, declaration);

		_rules.Add(lifted);
		_bodies[lifted]   = inner;
		_results[lifted]  = [];
		_nullable[lifted] = FirstSets.Nullable(inner, _ruleIsNullable);
		_lifted[lifted]   = new Lift(from, origin, transparent);

		if (_trivia.TryGetValue(from, out var trivia))
			_trivia[lifted] = trivia;

		return new Node.Call(lifted, []);
	}

	/// <summary>A node with one node under it replaced, every node on the way rebuilt and carrying its facts.</summary>
	Node Replaced(Node node, Node target, Node by)
	{
		if (ReferenceEquals(node, target))
			return by;

		if (!NodeWalk.Descendants(node).Any(one => ReferenceEquals(one, target)))
			return node;

		Node rebuilt = node switch
		{
			Node.Sequence sequence => sequence with { Nodes = [.. sequence.Nodes.Select(part => Replaced(part, target, by))] },
			Node.Choice choice     => choice.Rebuild([.. choice.Nodes.Select(part => Replaced(part, target, by))]),
			Node.Repeat repeat     => repeat with { Body = Replaced(repeat.Body, target, by) },
			Node.Capture capture   => capture with { Body = Replaced(capture.Body, target, by) },
			Node.Atomic atomic     => atomic with { Body = Replaced(atomic.Body, target, by) },
			Node.Marked marked     => marked with { Body = Replaced(marked.Body, target, by) },
			Node.Construct built   => built with { Body = Replaced(built.Body, target, by) },
			Node.Bind bind         => bind with { Body = Replaced(bind.Body, target, by) },
			Node.Counted counted   => counted with { Body = Replaced(counted.Body, target, by) },
			_                      => throw new InvalidOperationException($"A marked repetition stands where it cannot be lifted from: {node.GetType().Name}."),
		};

		Carry(node, rebuilt);

		return rebuilt;
	}

	/// <summary>
	/// Whether a marked repetition inside another one's element shares a value with the rest of the
	/// rule that a rule of its own could not carry, said where its <c>recover</c> is written.
	/// </summary>
	/// <remarks>
	/// What it captures stays the enclosing rule's, and is read at the end, where the rule's value
	/// is built. What is read while the rule is still being read cannot cross: a guard inside reads
	/// what the inner part captured, and a guard outside what the rule did, each on its own side.
	/// </remarks>
	bool Refused(RuleSymbol rule, Node inner)
	{
		var at      = _recoveries[inner].At;
		var owner   = rule;
		var bodies  = new List<Node>();

		while (_lifted.TryGetValue(owner, out var lift) && lift.Transparent)
			owner = lift.Parent;

		Region(_bodies[owner]);

		var inside  = Captured(inner);
		var outside = new HashSet<string>(StringComparer.Ordinal);

		foreach (var body in bodies)
			foreach (var name in Captured(body, except: inner))
				outside.Add(name);

		string? why = null;

		// What a guard inside is handed is what the part captured before it, as the rule the part
		// becomes numbers it; a name captured outside and not among those is one it cannot have.
		var layout = CaptureLayout.Of(inner, BuildsValue);

		foreach (var node in Unlifted(inner))
			if (node is Node.Construct)
				why ??= "a value is built inside it, and the value it is part of is the enclosing rule's";
			else if (node is Node.Capture(_, Node.Lookahead))
				why ??= "what a lookahead inside it saw is captured, and that capture is written where the lookahead ends, outside its call";
			else if (node is Node.Guard guard &&
				outside.FirstOrDefault(name => !Before(guard, name) && Names(guard.Text, name)) is { } read)
				why ??= $"a guard inside it reads '{read}', which is captured outside it and not inside it before the guard";

		foreach (var body in bodies)
			foreach (var node in Unlifted(body, except: inner))
				if (node is Node.Guard guard && inside.FirstOrDefault(name => Names(guard.Text, name)) is { } read)
					why ??= $"a guard outside it reads '{read}', which is captured inside it";

		if (why is null)
			return false;

		Report(
			NestedRecoveryFlow,
			$"This 'recover' stands inside another one's repetition in '{rule.Declaration!.Name}', and recovers as if it " +
			$"were a rule of its own there — but {why}, which a rule of its own cannot carry. Move the part into its " +
			"own rule and pass what it needs (docs/syntax.md §8.2).",
			at);

		return true;

		bool Before(Node guard, string name)
		{
			var before = layout.Before(guard);

			return layout.Slots.Any(slot => slot.Index < before && slot.Name == name);
		}

		// The rule's body and the bodies already lifted out of it: the one value they build.
		void Region(Node body)
		{
			bodies.Add(body);

			foreach (var node in NodeWalk.Descendants(body))
				if (node is Node.Call(var called, _) &&
					_lifted.TryGetValue(called, out var lift) && lift.Transparent &&
					_bodies.TryGetValue(called, out var lifted))
					Region(lifted);
		}
	}

	/// <summary>The nodes under one, without what stands under another node, or inside a lookahead or a synchronization.</summary>
	static IEnumerable<Node> Unlifted(Node node, Node? except = null)
	{
		if (ReferenceEquals(node, except))
			yield break;

		yield return node;

		if (node is Node.Lookahead)
			yield break;

		foreach (var child in node.Children)
			foreach (var one in Unlifted(child, except))
				yield return one;
	}

	static HashSet<string> Captured(Node node, Node? except = null)
	{
		var names = new HashSet<string>(StringComparer.Ordinal);

		foreach (var one in Unlifted(node, except))
			if (one is Node.Capture(var name, _))
				names.Add(name);

		return names;
	}

	/// <summary>Whether C# names a capture: by the scanner where there is one, by the word otherwise.</summary>
	bool Names(string text, string name)
	{
		if (_scanner?.FreeNames(text) is { } free)
			return free.Contains(name);

		for (var at = text.IndexOf(name, StringComparison.Ordinal); at >= 0;
			at = text.IndexOf(name, at + 1, StringComparison.Ordinal))
		{
			if ((at == 0 || !(Continues(text[at - 1]) || text[at - 1] == '.')) &&
				(at + name.Length == text.Length || !Continues(text[at + name.Length])))
				return true;
		}

		return false;

		static bool Continues(char c)
		{
			return UnicodeCategories.IsLetterOrDigit(c) || c is '_' or '@';
		}
	}
}
