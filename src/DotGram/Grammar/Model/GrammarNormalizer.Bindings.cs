using System;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>
/// Bindings, references and lengths (<c>$q: X</c>, <c>$q</c>, <c>X{#q}</c>): which symbol each
/// one stands for, and where one cannot stand (docs/syntax.md §3.5.1).
/// </summary>
/// <remarks>
/// <para>
/// A binding is resolved once, while its rule is lowered, to a <see cref="TextBinding"/>: one per
/// name in each top-level alternative. Every pass after that sees symbols, not names, so a copy of
/// a body into another rule or another specialization cannot make one binding read another's text.
/// </para>
/// <para>
/// What is checked here is checked on the graph as lowering made it — before any pass reshapes a
/// rule — because a reference is an error unless every route that reaches it bound the text first,
/// and the routes are what the author wrote.
/// </para>
/// </remarks>
public sealed partial class GrammarNormalizer
{
	/// <summary>A reference or a length read where, on some route, nothing bound the text.</summary>
	public const string UnboundReference = "GRAM4032";

	/// <summary>A binding or a reference where none can stand.</summary>
	public const string MisplacedBinding = "GRAM4033";

	/// <summary>A binding where this version of the compiler does not support one yet.</summary>
	public const string BindingNotSupported = "GRAM4034";

	/// <summary>A binding or a reference in a rule read over token kinds.</summary>
	public const string BindingOverKinds = "GRAM4035";

	/// <summary>
	/// The symbol each <c>$q:</c>, <c>$q</c> and <c>#q</c> of the body being lowered stands for,
	/// keyed by the syntax node that wrote it.
	/// </summary>
	Dictionary<object, TextBinding> _texts = new(ObjectIdentity.Instance);

	/// <summary>A rule's body lowered, with its bindings resolved for it alone.</summary>
	/// <remarks>
	/// Saved and restored around the lowering, because lowering one rule may lower another — an
	/// element set merges what the rule it names is made of — and each specialization of a
	/// parameterized rule is lowered again from the same text and gets symbols of its own.
	/// </remarks>
	Node LowerBody(RuleSymbol rule, Expr body, GrammarNamespace ns)
	{
		var outer = _texts;

		_texts = BoundTexts(rule, body);

		var lowered = Lower(body, ns);

		_texts = outer;

		return lowered;
	}

	/// <summary>
	/// One symbol per name in each top-level alternative, made for every binding, reference and
	/// length in it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Per top-level alternative and not per rule: the alternatives of a rule are routes that
	/// exclude each other, and a reference reads only what its own alternative bound. Bindings of one
	/// name inside one alternative share the symbol — those on routes that exclude each other are one
	/// binding however many times it is written, and two on one route are refused by the checks.
	/// </para>
	/// <para>
	/// A reference whose alternative binds nothing of its name gets a symbol nothing binds, so that it
	/// is reported where it stands, by the same check as any other reference read unbound.
	/// </para>
	/// </remarks>
	static Dictionary<object, TextBinding> BoundTexts(RuleSymbol rule, Expr body)
	{
		var texts        = new Dictionary<object, TextBinding>(ObjectIdentity.Instance);
		var alternatives = body is Expr.Choice(var written) ? written : [body];
		var index        = 0;

		foreach (var alternative in alternatives)
		{
			var names = new Dictionary<string, TextBinding>(StringComparer.Ordinal);

			// Bindings first, so that a reference standing to the left of its binding still reads the
			// alternative's symbol and is reported as read too early rather than as unknown.
			Walk(alternative, binds: true);
			Walk(alternative, binds: false);

			void Walk(Expr expression, bool binds)
			{
				switch (expression)
				{
					case Expr.Bind(var name, _) when binds:
						texts[expression] = Named(name, expression.At.Position);
						break;

					case Expr.Same(var name) when !binds:
						texts[expression] = Named(name, expression.At.Position);
						break;

					case Expr.Quantified quantified when !binds:
						if (quantified.MinLength is { } low)
							texts[low] = Named(low.Name, low.At.Position);

						if (quantified.MaxLength is { } high)
							texts[high] = Named(high.Name, high.At.Position);

						break;
				}

				foreach (var child in Dump.Children(expression))
					Walk(child, binds);
			}

			TextBinding Named(string name, int at)
			{
				if (!names.TryGetValue(name, out var binding))
					names[name] = binding = new TextBinding(rule, index++, name, at);

				return binding;
			}
		}

		return texts;
	}

	/// <summary>The symbol a syntax node was resolved to, or a fresh one where it was not.</summary>
	/// <remarks>
	/// Fresh where lowering reaches text that no rule body holds — nothing does today, but a symbol
	/// nothing binds is still reported where it is read rather than lowered into a crash.
	/// </remarks>
	TextBinding TextOf(object written, string name, int at)
	{
		if (!_texts.TryGetValue(written, out var binding))
			_texts[written] = binding = new TextBinding(_currentRule ?? Unresolved(name), -1, name, at);

		return binding;
	}

	/// <summary><c>q: X</c>, and <c>q: ($q: X)</c> as <c>$q: (q: X)</c>.</summary>
	/// <remarks>
	/// A capture of a binding is the way to have both, and both stand on the same span: the
	/// capture outside would see a binding where it looks for the rule it takes a value from,
	/// and keep text where <c>q: X</c> keeps <c>X</c>'s value. Inside, it is <c>q: X</c> exactly.
	/// </remarks>
	Node LowerCapture(string name, Expr operand, GrammarNamespace ns)
	{
		var lowered = Lower(operand, ns);

		if (lowered is Node.Bind bind)
			return bind with { Body = new Node.Capture(name, bind.Body) };

		return new Node.Capture(name, lowered);
	}

	/// <summary><c>$q: X</c>.</summary>
	Node LowerBind(Expr expression, string name, Expr operand, GrammarNamespace ns)
	{
		var binding = TextOf(expression, name, expression.At.Position);

		return new Node.Bind(binding, Lower(operand, ns), expression.At.Position);
	}

	/// <summary><c>$q</c>.</summary>
	Node LowerSame(Expr expression, string name)
	{
		return new Node.Same(TextOf(expression, name, expression.At.Position), expression.At.Position);
	}

	/// <summary>
	/// A repetition, with a binding that is all it repeats lifted out of it: <c>($q: X)*</c> is
	/// <c>$q: (X*)</c>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// By definition and unconditionally, unlike a text capture's hoist: a binding is always text,
	/// and the turns of a repetition are contiguous, so the turns joined are the run. What that buys
	/// is that the binding happened on every route through the repetition — after no turns it is
	/// "", the text of none.
	/// </para>
	/// <para>
	/// Except for an option, <c>($q: X)?</c>, which either happened or did not: lifted, the route
	/// that skipped it would bind "" where the author wrote a binding that did not happen. Left as
	/// written, a reference after it is reported, with <c>$q: (X?)</c> as the way to say "".
	/// </para>
	/// </remarks>
	Node Repetition(Node lowered, int min, int? max, GrammarNamespace ns)
	{
		if (lowered is Node.Bind bind && (min, max) != (0, 1))
			return bind with { Body = Repeated(bind.Body, min, max, ns) };

		return Repeated(lowered, min, max, ns);
	}

	/// <summary><c>X{#q}</c>, <c>X{#q,}</c>, <c>X{n,#q}</c>, <c>X{#q,m}</c>, <c>X{#q,#r}</c>.</summary>
	/// <remarks>
	/// A side that is a number — written, or a value parameter's — is resolved to that number here,
	/// as for any count; only a length is left to be read while the parser runs. A binding that is
	/// all the count repeats is lifted out of it, as <see cref="Repetition"/> lifts one out of any
	/// repetition: a count of a binding is never an option.
	/// </remarks>
	Node LowerCounted(Expr.Quantified quantified, GrammarNamespace ns)
	{
		var lowered = Lower(quantified.Operand, ns);

		CountBound min = quantified.MinLength is { } low
			? new CountBound.Length(TextOf(low, low.Name, low.At.Position), low.At.Position)
			: new CountBound.Items(Counted(quantified.Min, quantified.MinName, quantified) ?? 0);

		CountBound? max = quantified.MaxLength is { } high
			? new CountBound.Length(TextOf(high, high.Name, high.At.Position), high.At.Position)
			: quantified.Max is null && quantified.MaxName is null
				? null
				: Counted(quantified.Max, quantified.MaxName, quantified) is { } most
					? new CountBound.Items(most)
					: null;

		if (lowered is Node.Bind bind)
			return bind with { Body = CountedRun(bind.Body, min, max, ns) };

		return CountedRun(lowered, min, max, ns);
	}

	/// <summary>A counted repetition, spaced between its turns as <see cref="Repeated"/> spaces any.</summary>
	/// <remarks>
	/// A length is never 0 or 1 when the parser is generated, so a count by one is spaced unless its
	/// upper bound is a number that rules out a second turn.
	/// </remarks>
	Node CountedRun(Node body, CountBound min, CountBound? max, GrammarNamespace ns)
	{
		if (max is CountBound.Items { Count: 0 or 1 } ||
			body is not Node.Sequence(var operands) ||
			TriviaFor(ns) is not { } trivia ||
			IsSeam(operands[0], trivia))
			return new Node.Counted(body, min, max);

		var spaced = new List<Node>(operands.Count + 1) { trivia };

		spaced.AddRange(operands);

		return new Node.Counted(new Node.Sequence(spaced), min, max);
	}

	/// <summary>
	/// Reports a binding or a reference written where a value goes, which is read before or apart
	/// from the input the rule reads: C# in <c>=&gt;</c> or <c>when</c>, an argument, a condition.
	/// </summary>
	/// <returns>Whether there was one.</returns>
	bool RefuseBoundText(Expr value, string where)
	{
		var found = false;

		Visit(value);

		return found;

		void Visit(Expr expression)
		{
			var name = expression switch
			{
				Expr.Bind(var bound, _) => bound,
				Expr.Same(var read)     => read,
				_                       => null,
			};

			if (name is not null)
			{
				found = true;

				Report(
					MisplacedBinding,
					$"'${name}' stands {where}. A binding is read again only as a recognizer, later in " +
					"the alternative that bound it (docs/syntax.md §3.5.1).",
					new Location(expression.At.Position, name.Length + 1));
			}

			foreach (var child in Dump.Children(expression))
				Visit(child);
		}
	}

	// ── The checks ──────────────────────────────────────────────────────────────

	/// <summary>Whether a node is, or holds, a binding, a reference or a count by a length.</summary>
	internal static bool HoldsBoundText(Node node)
	{
		foreach (var each in NodeWalk.Descendants(node))
			if (each is Node.Bind or Node.Same or Node.Counted)
				return true;

		return false;
	}

	/// <summary>The first binding, reference or length in a node, in the order written.</summary>
	static (TextBinding Binding, int At)? FirstBoundText(Node node)
	{
		foreach (var each in NodeWalk.Descendants(node))
			foreach (var found in BoundTextAt(each))
				return found;

		return null;
	}

	/// <summary>What one node binds or reads, and where it was written.</summary>
	internal static IEnumerable<(TextBinding Binding, int At)> BoundTextAt(Node node)
	{
		switch (node)
		{
			case Node.Bind(var binding, _, var at):
				yield return (binding, at);
				break;

			case Node.Same(var binding, var at):
				yield return (binding, at);
				break;

			case Node.Counted(_, var min, var max):
				if (min is CountBound.Length(var low, var lowAt))
					yield return (low, lowAt);

				if (max is CountBound.Length(var high, var highAt) && max != min)
					yield return (high, highAt);

				break;
		}
	}

	/// <summary>Where a binding, a reference or a length was written, as a diagnostic marks it.</summary>
	static Location Marked(TextBinding binding, int at)
	{
		return at < 0 ? new Location(0, 0) : new Location(at, binding.Name.Length + 1);
	}

	/// <summary>
	/// Every binding, reference and length of every rule, held to where each can stand.
	/// </summary>
	/// <remarks>
	/// Right after lowering: what is asked is about the routes the author wrote, and every later
	/// pass may reshape them. Rules with none of the three are passed over at the cost of one walk.
	/// </remarks>
	void CheckBoundTexts()
	{
		foreach (var rule in _rules)
			if (_bodies.TryGetValue(rule, out var body) && HoldsBoundText(body))
				CheckBoundTexts(rule, body);

		// A recovery point is where a parse goes on past input it could not read; what it skips and
		// where it resumes are not a route a binding can be followed along yet.
		foreach (var pair in _recoveries)
			if ((FirstBoundText(pair.Key) ?? FirstBoundText(pair.Value.Sync)) is var (binding, at))
				Report(
					BindingNotSupported,
					$"'${binding.Name}' stands in a repetition that recovers ('recover', docs/syntax.md §8.2), " +
					"and a binding, a reference or a length there is not supported yet.",
					Marked(binding, at));
	}

	void CheckBoundTexts(RuleSymbol rule, Node body)
	{
		var order   = new Dictionary<Node, int>(NodeIdentity.Instance);
		var binds   = new List<(Node.Bind Node, Node[] Around)>();
		var reads   = new List<(Node Site, TextBinding Binding, int At, Node[] Around)>();
		var around  = new List<Node>();

		Walk(body);

		foreach (var (site, binding, at, outer) in reads)
		{
			if (outer.Any(one => one is Node.Bind(var bound, _, _) && ReferenceEquals(bound, binding)))
			{
				Report(
					MisplacedBinding,
					$"'${binding.Name}' is read inside its own binding in '{rule.Name}', while the text it would " +
					"match is still being bound. Read it after the binding (docs/syntax.md §3.5.1).",
					Marked(binding, at));

				continue;
			}

			if (!BindsBefore(body, site, binding))
				Report(UnboundReference, Unbound(rule, body, site, binding, binds, order), Marked(binding, at));
		}

		void Walk(Node node)
		{
			if (!order.ContainsKey(node))
				order[node] = order.Count;

			var outer = around.ToArray();

			switch (node)
			{
				case Node.Bind(var binding, _, var at) bind:
					binds.Add((bind, outer));

					// A lookahead gives back what it read, and its own records with it: a binding made
					// inside one would be gone, or would be text nothing consumed.
					if (outer.Any(one => one is Node.Lookahead))
						Report(
							MisplacedBinding,
							$"'${binding.Name}' is bound inside a lookahead in '{rule.Name}', which is not " +
							"supported: a lookahead gives back what it read. Bind it outside the lookahead; a " +
							"reference inside one, as in '?!$q', reads an outer binding (docs/syntax.md §3.5.1).",
							Marked(binding, at));

					else if (outer.Any(one => one is Node.Bind(var bound, _, _) && ReferenceEquals(bound, binding)))
						Report(
							MisplacedBinding,
							$"'${binding.Name}' is bound twice on one route in '{rule.Name}'. A name is bound once " +
							"on each route; give the inner binding another name (docs/syntax.md §3.5.1).",
							Marked(binding, at));

					// A recovery point inside the binding. The check over the recoveries in
					// CheckBoundTexts() looks inside each recovering repetition; this is the binding
					// around one, which holds the same route from outside.
					if (_recoveries.Count > 0 && NodeWalk.Descendants(bind.Body).Any(_recoveries.ContainsKey))
						Report(
							BindingNotSupported,
							$"'${binding.Name}' binds a repetition that recovers ('recover', docs/syntax.md §8.2), " +
							"and a binding over a recovery point is not supported yet.",
							Marked(binding, at));

					break;

				case Node.Same(var binding, var at):
					reads.Add((node, binding, at, outer));
					break;

				case Node.Counted:
					foreach (var (binding, at) in BoundTextAt(node))
						reads.Add((node, binding, at, outer));

					break;

				// Two operands of one sequence that both bind a name bind it twice on the route
				// through both — whichever way each binds it.
				case Node.Sequence(var nodes):
				{
					var earlier = new HashSet<TextBinding>();

					foreach (var operand in nodes)
					{
						var here = new HashSet<TextBinding>();

						foreach (var inner in NodeWalk.Descendants(operand))
							if (inner is Node.Bind(var binding, _, var at) && here.Add(binding) && earlier.Contains(binding))
								Report(
									MisplacedBinding,
									$"'${binding.Name}' is bound twice on one route in '{rule.Name}'. A name is " +
									"bound once on each route; give the second binding another name " +
									"(docs/syntax.md §3.5.1).",
									Marked(binding, at));

						earlier.UnionWith(here);
					}

					break;
				}
			}

			around.Add(node);

			foreach (var child in node.Children)
				Walk(child);

			around.RemoveAt(around.Count - 1);
		}
	}

	/// <summary>Why a reference may be read unbound, in the words that say how to fix it.</summary>
	static string Unbound(
		RuleSymbol rule, Node body, Node site, TextBinding binding,
		List<(Node.Bind Node, Node[] Around)> binds, Dictionary<Node, int> order)
	{
		var name  = "$" + binding.Name;
		var mine  = binds.Where(one => ReferenceEquals(one.Node.Binding, binding)).ToList();
		var start = $"'{name}' may be read in '{rule.Name}' where nothing bound it";

		if (mine.Count == 0)
			return binds.Any(one => one.Node.Binding.Name == binding.Name)
				? $"'{name}' is bound in another alternative of '{rule.Name}', and a reference reads only what " +
					"its own alternative bound (docs/syntax.md §3.5.1)."
				: $"'{name}' is not bound in this alternative of '{rule.Name}' (docs/syntax.md §3.5.1).";

		if (mine.All(one => order[one.Node] > order[site]))
			return $"'{name}' is read in '{rule.Name}' before it is bound: its binding stands to the right of " +
				"it. A reference reads text bound to its left (docs/syntax.md §3.5.1).";

		// The repetition a binding of it stands in, where the reference is not inside it too: an
		// option, which may have been skipped, or a run whose turns hold more than the binding.
		bool Skipped(bool option)
		{
			return mine.Any(one => one.Around.Any(outer => !Holds(outer, site) && outer switch
			{
				Node.Repeat { Min: 0, Max: 1 } => option,
				Node.Repeat or Node.Counted    => !option,
				_                              => false,
			}));
		}

		if (Skipped(option: true))
			return start + $": '({name}: X)?' binds nothing on the route that skips it. Write '{name}: (X?)', " +
				"which binds \"\" there (docs/syntax.md §3.5.1).";

		if (Skipped(option: false))
			return start + ": it is bound inside a repetition whose turns hold more than the binding, so it " +
				$"is read only within the turn that bound it. Bind the whole run instead, as in " +
				$"'{name}: (X & ',')*' (docs/syntax.md §3.5.1).";

		return start + $": on some route before it, '{name}:' is not matched. A reference needs its text bound " +
			"on every route to it (docs/syntax.md §3.5.1).";
	}

	/// <summary>
	/// Whether every route that reaches <paramref name="site"/> binds <paramref name="binding"/>
	/// before it.
	/// </summary>
	/// <remarks>
	/// <see cref="WritesBefore"/>'s question, asked of a binding rather than a capture, and with two
	/// differences. A lookahead holding the site is a route to it — a reference inside <c>?!$q</c>
	/// reads the outer binding. And what binds is <see cref="Binds"/>, where a repetition never does:
	/// one that only repeats a binding was lifted out of it while lowering.
	/// </remarks>
	internal static bool BindsBefore(Node node, Node site, TextBinding binding)
	{
		if (ReferenceEquals(node, site))
			return false;

		switch (node)
		{
			case Node.Sequence(var nodes):
			{
				var held = false;
				var all  = true;

				for (var index = 0; index < nodes.Count; index++)
				{
					if (!Holds(nodes[index], site))
						continue;

					held = true;
					var here = BindsBefore(nodes[index], site, binding);

					for (var earlier = 0; earlier < index && !here; earlier++)
						here = Binds(nodes[earlier], binding);

					all &= here;
				}

				return held && all;
			}

			case Node.Choice(var nodes):
			{
				var held = false;
				var all  = true;

				foreach (var one in nodes)
					if (Holds(one, site))
					{
						held = true;
						all &= BindsBefore(one, site, binding);
					}

				return held && all;
			}

			case Node.Repeat   (var body, _, _): return BindsBefore(body,   site, binding);
			case Node.Counted  (var body, _, _): return BindsBefore(body,   site, binding);
			case Node.Bind     (_, var body, _): return BindsBefore(body,   site, binding);
			case Node.Capture  (_, var body):    return BindsBefore(body,   site, binding);
			case Node.Atomic   (var body):       return BindsBefore(body,   site, binding);
			case Node.Marked   (var body, _):    return BindsBefore(body,   site, binding);
			case Node.Construct(var body, _):    return BindsBefore(body,   site, binding);
			case Node.Lookahead(_, var body):    return BindsBefore(body,   site, binding);
			default:                             return false;
		}
	}

	/// <summary>Whether matching the whole of a node always binds <paramref name="binding"/>.</summary>
	internal static bool Binds(Node node, TextBinding binding)
	{
		return node switch
		{
			Node.Bind(var bound, var body, _) => ReferenceEquals(bound, binding) || Binds(body, binding),
			Node.Capture(_, var body)         => Binds(body, binding),
			Node.Atomic(var body)             => Binds(body, binding),
			Node.Marked(var body, _)          => Binds(body, binding),
			Node.Construct(var body, _)       => Binds(body, binding),
			Node.Sequence(var nodes)          => nodes.Any(child => Binds(child, binding)),
			Node.Choice(var nodes)            => nodes.All(child => Binds(child, binding)),

			// A repetition binds nothing a reading after it can rely on: one that repeats only a
			// binding was lifted out of it while lowering, and what is left is an option — which may
			// not have happened — or turns that each rebind it with more around it. A lookahead
			// gives back what it read, and a binding inside one is refused.
			_ => false,
		};
	}

	/// <summary>
	/// Reports each binding a lifted constructing group would carry out of the rule that reads it,
	/// and each reference it would carry away from its binding.
	/// </summary>
	/// <remarks>
	/// A constructing group becomes a rule of its own, and a binding is read only in the rule that
	/// bound it, so a binding and its references stand both inside the group or both outside it.
	/// </remarks>
	void RefuseCrossing(RuleSymbol owner, Node group, Node whole)
	{
		if (!HoldsBoundText(whole))
			return;

		var inside = NodeWalk.ByIdentity(NodeWalk.Descendants(group));
		var bound  = new HashSet<TextBinding>();

		foreach (var node in inside)
			if (node is Node.Bind(var binding, _, _))
				bound.Add(binding);

		foreach (var node in NodeWalk.Descendants(whole))
			if (node is not Node.Bind)
				foreach (var (binding, at) in BoundTextAt(node))
					if (inside.Contains(node) != bound.Contains(binding))
						Report(
							MisplacedBinding,
							$"'${binding.Name}' is read across a constructing group '( … => … )' in '{owner.Name}'. " +
							"The group builds its value as a rule of its own, and a binding is read only in the " +
							"rule that bound it: bind and read it both inside the group or both outside it " +
							"(docs/syntax.md §3.5.1).",
							Marked(binding, at));
	}

	/// <summary>Reports a step of a left-recursive rule that binds, reads or counts by a length.</summary>
	/// <remarks>
	/// A fold's steps are read as a loop of their own after the base, each step once per operator,
	/// so what one binds is not there for the next, and a step reading the base's binding reads
	/// across the loop. Written in a rule the step calls, the same binding is read per call.
	/// </remarks>
	void RefuseFoldStep(RuleSymbol rule, Node step)
	{
		if (FirstBoundText(step) is var (binding, at))
			Report(
				MisplacedBinding,
				$"'${binding.Name}' stands in a step of the left-recursive rule '{rule.Name}' (docs/syntax.md §4.3), " +
				"where it is not supported: the steps are read as a loop of their own. Bind and read it inside " +
				"a rule the step calls.",
				Marked(binding, at));
	}

	/// <summary>Compares syntax nodes and the counts they hold by identity.</summary>
	sealed class ObjectIdentity : IEqualityComparer<object>
	{
		public static readonly ObjectIdentity Instance = new();

		public new bool Equals(object? x, object? y)
		{
			return ReferenceEquals(x, y);
		}

		public int GetHashCode(object node)
		{
			return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(node);
		}
	}
}
