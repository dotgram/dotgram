using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGram.Tests.RuleValues;

// The oracle of the rule-value gate: what a grammar of the gate must give, written from the rule alone and never from the
// generator. A rule without => is one of four kinds, by its type:
//   C (string): the pieces it matched on the route taken, joined: a call of a string rule gives its value, a call of a char
//               rule its character, an untyped rule is read through, trivia and lookaheads give nothing, anything else gives
//               its text; a piece of another type is an error.
//   S (span):   the bounds of what it matched.
//   A (array):  the value of every call of a rule of the element type, in order; untyped rules are read through.
//   O (other):  the one unnamed call of a rule of its type on every route; else the constructor its names fill; else an error.
// A left-recursive rule is its loop, a seed and steps: the value so far is the step's first piece (C), elements (A) or
// operand (O); a span is the rule's own bounds. Names change nothing in C, S and A. The oracle parses the input by the
// grammar (ordered choice, greedy repetition), then reads the value off the route the parse took.


/// <summary>The kind of a rule without =&gt;.</summary>
enum Kind
{
	C,
	S,
	A,
	O,
	Untyped,
}

/// <summary>A call of a rule of the grammar that is not a helper: a composite helper, or the rule under test from its caller.</summary>
sealed class RuleRef(Rule rule) : Expr
{
	public Rule Rule { get; } = rule;
}

/// <summary>A rule of the grammar under reading.</summary>
sealed class Rule
{
	public required string Name { get; init; }

	public required Kind Kind { get; init; }

	/// <summary>The C# type, of the element for an array; null for no type.</summary>
	public required string? Type { get; init; }

	public required IReadOnlyList<Expr> Alternatives { get; init; }

	public bool LeftRecursive { get; init; }

	/// <summary>A published rule with no type and no names, read as the published version did: its own text.</summary>
	public bool OwnText { get; init; }

	/// <summary>The decision of the alternatives of the rule; for a left-recursive rule, of its seeds and then of its steps.</summary>
	public Decision[] Groups { get; set; } = [];

	static bool IsStep(Expr alternative)
	{
		return alternative is Sequence { Items: [SelfCall, ..] };
	}

	public IEnumerable<Expr> Seeds
	{
		get
		{
			return LeftRecursive ? Alternatives.Where(static a => !IsStep(a)) : Alternatives;
		}
	}

	public IEnumerable<Expr> Steps
	{
		get
		{
			return LeftRecursive ? Alternatives.Where(IsStep) : [];
		}
	}
}

/// <summary>
/// The oracle.
/// </summary>
static class RuleOracle
{
	/// <summary>
	/// A published rule with no type written and no names (<c>Start = '&lt;' &amp; Word &amp; '&gt;'</c>) is a string rule: its
	/// value is its pieces joined. False reads it as the published version did, its own text. A rule typed by an untyped
	/// helper (<c>Start : Plain = …</c>) is a string rule under either value.
	/// </summary>
	public static bool UntypedPublishedJoins { get; } = Environment.GetEnvironmentVariable("DOTGRAM_RULEVALUES_UNTYPED_JOINS") is not ("0" or "false");

	/// <summary>What the oracle expects of the input.</summary>
	public static Expectation Expected(RuleShape shape, Mode mode, string input)
	{
		return Describe(shape).Expected(mode, input);
	}

	/// <summary>The static reading of a shape (a decision per rule, and the features the allow-list asks for).</summary>
	public static Grammar Describe(RuleShape shape)
	{
		return Analyses.GetOrAdd(shape, static s => new Grammar(s));
	}

	static readonly System.Collections.Concurrent.ConcurrentDictionary<RuleShape, Grammar> Analyses = new();
}

/// <summary>
/// The reading of one shape: its rules, the decision for each, and the evaluation of an input.
/// </summary>
sealed partial class Grammar
{
	enum Piece
	{
		Text,
		Built,
		Char,
		Other,
		ReadThrough,
	}

	static readonly HashSet<string> Composites = ["Mid", "C4", "Mids", "Ow", "Ows", "Mw", "Mws", "Mx", "P2", "P(Word)"];

	readonly RuleShape _shape;
	readonly List<Rule> _rules = [];
	readonly Dictionary<string, Rule> _composites = [];
	readonly HashSet<Rule> _inProgress = [];

	/// <summary>The rule the shape is published through: <c>Start</c>.</summary>
	public Rule Start { get; }

	/// <summary>The rule under test (the same as <see cref="Start"/> for a shape nobody calls).</summary>
	public Rule Tested { get; }

	/// <summary>The first error or question among the rules, or the decision of <see cref="Start"/>.</summary>
	public Decision Decision { get; private set; }

	/// <summary>What the grammar holds that the classes of the allow-list are told by.</summary>
	public HashSet<string> Features { get; } = [];

	public Grammar(RuleShape shape)
	{
		_shape = shape;

		var type = shape.Type;
		var body = shape.Alternatives.Select(Replace).ToList();

		// A rule with no type written is untyped: with no names it is a string rule, with names it makes a type of its own,
		// which is not modelled. A rule typed by an untyped helper (which writes no names) follows the helper: a string rule,
		// whatever names the taking rule writes.
		var unsaid = type.How == TypeSaid.None;
		var takesUntyped = type.Taken is { Type: null };
		var untyped = unsaid && Walk(body, true).OfType<Named>().Any();
		var string_ = takesUntyped || (unsaid && !untyped);

		Tested = Make(shape.RuleName, string_ ? "string" : TypeOf(type), type.How == TypeSaid.Sequence, untyped, body, shape.LeftRecursive, unsaid && string_ && !RuleOracle.UntypedPublishedJoins);

		_rules.Add(Tested);

		if (shape.Template)
		{
			Start = Make("Start", Tested.Type, false, Tested.Kind == Kind.Untyped, [new RuleRef(Tested)], false);
		}
		else
		{
			switch (shape.SeenFrom)
			{
				case SeenFrom.InSequence:
					Start = Make("Start", type.ClrType, true, false,
						[new Sequence(new RuleRef(Tested), new Literal(";"), new Call(RulePrinter.BesideInSequence(type), Atom.Built))], false);
					break;

				case SeenFrom.TakenBy:
					Start = Make("Start", Tested.Type, false, false, [new Sequence(new Literal("["), new RuleRef(Tested), new Literal("]"))], false);
					break;

				case SeenFrom.BesideFilling:
					Start = Make("Start", Tested.Type, false, false, [new Sequence([.. RuleShapes.Filling(type.Family)!, new RuleRef(Tested)])], false);
					break;

				default:
					Start = Tested;
					break;
			}
		}

		if (!ReferenceEquals(Start, Tested))
			_rules.Add(Start);

		Decision = Decision.Of(Source.Extent, "");
		Read();
	}

	static string? TypeOf(RuleType type)
	{
		if (type.How == TypeSaid.None)
			return null;

		return type.Taken is { } taken ? LeafInfo(taken.Name).Type : type.ClrType;
	}

	static Rule Make(string name, string? type, bool array, bool untyped, IReadOnlyList<Expr> body, bool leftRecursive, bool ownText = false)
	{
		var kind = untyped || type is null ? Kind.Untyped : array ? Kind.A : type switch
		{
			"string"     => Kind.C,
			"SourceSpan" => Kind.S,
			_            => Kind.O,
		};

		return new Rule { Name = name, Kind = kind, Type = type, Alternatives = body, LeftRecursive = leftRecursive, OwnText = ownText };
	}

	/// <summary>The type of a helper that is read as a single token, and whether it builds its value.</summary>
	static (string? Type, bool Builds) LeafInfo(string name)
	{
		return name switch
		{
			"Word" or "Nm" or "Dg" => ("string", true),
			"Bare" or "G"          => ("string", false),
			"Plain"                => (null, false),
			"Span" or "Gs"         => ("SourceSpan", false),
			"Sp"                   => ("SourceSpan", true),
			"Bx" or "Bxa"          => ("Box", true),
			"Lf" or "Lfa"          => ("Pr", true),
			"Item"                 => ("Item", true),
			"Pair"                 => ("Pair", true),
			"Num"                  => ("int", true),
			"O"                    => ("object", true),
			"Ch" or "Esc"          => ("char", true),
			_                      => throw new InvalidOperationException(name),
		};
	}

	// A helper that has a body of its own: a rule of the grammar, called through a reference.
	Rule Composite(Helper helper)
	{
		if (_composites.TryGetValue(helper.Name, out var known))
			return known;

		IReadOnlyList<Expr> body = helper.Name switch
		{
			"Mw"  => [new Sequence(new Literal("("), new Call(Helper.Word, Atom.Built), new Literal(")"))],
			"Mws" => [new Sequence(new Literal("("), new Call(Helper.Sp, Atom.Built), new Literal(")"))],
			"Mx"  => [new Call(Helper.Word, Atom.Built), new Sequence(new Literal("["), new Call(Helper.Bare, Atom.Implicit), new Literal("]"))],
			_     => [helper.Inline!(0)],
		};

		var rule = Make(helper.Name, helper.Type, false, helper.Type is null, [.. body.Select(Replace)], false);

		_composites[helper.Name] = rule;
		_rules.Insert(0, rule);

		return rule;
	}

	// The parameter of a template stands for a call of the rule passed for it, and a helper with a body of its own for a
	// reference to the rule it is.
	Expr Replace(Expr expr)
	{
		switch (expr)
		{
			case Parameter:
				return Replace(new Call(_shape.TemplateArgument!, Atom.Implicit));

			case Call call when Composites.Contains(call.Rule.Name):
				return new RuleRef(Composite(call.Rule));

			case Named named:
				return new Named(named.Name, Replace(named.Inner));

			case Bind bind:
				return new Bind(bind.Name, Replace(bind.Inner));

			case Sequence sequence:
				return new Sequence([.. sequence.Items.Select(Replace)]);

			case Choice choice:
				return new Choice([.. choice.Alternatives.Select(Replace)]);

			case Group group:
				return new Group(Replace(group.Inner));

			case Wrapped wrapped:
				return new Wrapped(wrapped.Wrap, Replace(wrapped.Inner));

			default:
				return expr;
		}
	}

	// ---- What a rule reads ----

	static bool IsCall(Expr expr)
	{
		return expr is Call or RuleRef or SelfCall;
	}

	/// <summary>
	/// Every node of the alternatives outside lookaheads and <c>{0}</c> (inside them too, where <paramref name="written"/>: what is
	/// written, not what is read), reading through the untyped rules they call.
	/// </summary>
	static IEnumerable<Expr> Walk(IEnumerable<Expr> alternatives, bool written = false)
	{
		foreach (var alternative in alternatives)
			foreach (var node in WalkOne(alternative, written))
				yield return node;
	}

	static IEnumerable<Expr> WalkOne(Expr expr, bool written)
	{
		yield return expr;

		IEnumerable<Expr> inner = expr switch
		{
			Named named                                                   => [named.Inner],
			Bind bind                                                     => [bind.Inner],
			Sequence sequence                                             => sequence.Items,
			Choice choice                                                 => choice.Alternatives,
			Group group                                                   => [group.Inner],
			Wrapped wrapped when written || wrapped.Wrap is not (Wrap.Ahead or Wrap.NotAhead or Wrap.Zero) => [wrapped.Inner],
			RuleRef { Rule.Kind: Kind.Untyped } reference                 => reference.Rule.Alternatives,
			_                                                             => [],
		};

		foreach (var node in Walk(inner, written))
			yield return node;
	}

	/// <summary>The type a call gives; null for an untyped rule.</summary>
	static string? CalleeType(Expr call, Rule self)
	{
		return call switch
		{
			Call c      => LeafInfo(c.Rule.Name).Type,
			RuleRef r   => r.Rule.Kind == Kind.Untyped ? null : r.Rule.Type,
			SelfCall    => self.Type,
			_           => null,
		};
	}

	Piece Classify(Expr call, Rule self)
	{
		var callee = call switch
		{
			RuleRef r => r.Rule,
			SelfCall  => self,
			_         => null,
		};

		if (callee is not null)
		{
			return callee.Kind switch
			{
				Kind.Untyped => Piece.ReadThrough,
				Kind.C       => Impure(callee) ? Piece.Built : Piece.Text,
				_            => Piece.Other,
			};
		}

		var (type, builds) = LeafInfo(((Call)call).Rule.Name);

		return type switch
		{
			"string" => builds ? Piece.Built : Piece.Text,
			"char"   => Piece.Char,
			null     => Piece.Text,
			_        => Piece.Other,
		};
	}

	// An array rule whose collected calls, on any route, hold a call that gives text over bytes and one that builds a string.
	bool MixedOverBytes(Rule rule)
	{
		if (rule.Kind != Kind.A)
			return false;

		var pieces = Walk(rule.Alternatives).Where(IsCall).Where(c => c is not SelfCall && CalleeType(c, rule) == rule.Type).Select(c => Classify(c, rule)).ToList();

		return pieces.Contains(Piece.Text) && pieces.Any(static p => p is Piece.Built or Piece.Char);
	}

	/// <summary>
	/// A string rule is not its own text where a piece builds a value that differs from its text: a built string, a
	/// character, or a rule that is not pure itself (a least fixpoint over its calls), or a woven seam that can be non-empty.
	/// </summary>
	bool Impure(Rule rule)
	{
		if (rule.OwnText || !_inProgress.Add(rule))
			return false;

		try
		{
			var nodes = Walk(rule.Alternatives).ToList();

			if (_shape.Trivia && nodes.OfType<Sequence>().Any(static s => s.Items.Length >= 2))
				return true;

			return nodes.Where(IsCall).Any(c => Classify(c, rule) is Piece.Built or Piece.Char);
		}
		finally
		{
			_inProgress.Remove(rule);
		}
	}

	static string[] Parameters(string? type)
	{
		return type switch
		{
			"Box"  => ["name", "inner", "value"],
			"Pr"   => ["t", "l", "r"],
			"Item" => ["x"],
			"Pair" => ["a", "b"],
			_      => [],
		};
	}

	// ---- The decision per rule ----

	void Read()
	{
		foreach (var rule in _rules)
		{
			rule.Groups = Decide(rule);
			Describe(rule);
		}

		Decision = _rules.SelectMany(static r => r.Groups).FirstOrDefault(static d => d.Source is Source.Error or Source.Undecided or Source.SamePublished) ?? Start.Groups[0];
	}

	Decision[] Decide(Rule rule)
	{
		var row = rule.Kind switch { Kind.C => "C", Kind.S => "S", Kind.A => "A", _ => "O" };

		// A name written inside a lookahead is refused, whatever the rule is.
		if (Walk(rule.Alternatives, true).OfType<Wrapped>().Any(static w => w.Wrap is Wrap.Ahead or Wrap.NotAhead && Walk([w.Inner], true).OfType<Named>().Any()))
			return [Decision.Fail(row, "GRAM4006")];

		var decisions = DecideRule(rule);

		// A name that is read must capture one kind. The rule's own decision tells which names it reads.
		if (CapturesDifferentKinds(rule, decisions))
			return [Decision.Fail(row, "GRAM4007")];

		return decisions;
	}

	Decision[] DecideRule(Rule rule)
	{
		var calls = Walk(rule.Alternatives).Where(IsCall).ToList();

		switch (rule.Kind)
		{
			case Kind.Untyped:
				// An untyped rule that writes names makes a type of its own from them, exactly as the published version does.
				return [_composites.ContainsValue(rule) ? Decision.Of(Source.Extent, "-") : Decision.Of(Source.SamePublished, "O")];

			case Kind.S:
				return [Decision.Of(Source.Extent, "S")];

			case Kind.C:
				if (rule.OwnText)
					return [Decision.Of(Source.Extent, "C")];

				if (calls.Any(c => Classify(c, rule) == Piece.Other))
					return [Decision.Fail("C", "E-NONTEXT")];

				return [Decision.Of(Impure(rule) ? Source.Joined : Source.Extent, "C")];

			case Kind.A:
				// `@Item` names a C# type, and the type a rule generates is not one in view.
				if (rule.Type == "Item")
					return [Decision.Fail("A", "GRAM3004")];

				// No call of the element type written: one in X{0} is written, and gives no element.
				if (!Walk(rule.Alternatives, true).Where(IsCall).Any(c => c is not SelfCall && CalleeType(c, rule) == rule.Type))
					return [Decision.Fail("A", "E-SEQ")];

				// An object collects any call, so a call of an array of objects is one element or many: it is not told.
				if (rule.Type == "object" && calls.Any(c => c is SelfCall || c is RuleRef { Rule.Kind: Kind.A }))
					return [Decision.Fail("A", "E-AMBIGUOUS")];

				return [Decision.Of(Source.Collected, "A")];

			default:
				return rule.LeftRecursive
					? [DecideO(rule, rule.Seeds.ToList()), DecideO(rule, rule.Steps.ToList())]
					: [DecideO(rule, rule.Alternatives)];
		}
	}

	Decision DecideO(Rule rule, IReadOnlyList<Expr> alternatives)
	{
		var routes = alternatives.SelectMany(a => RoutesOf(a, rule, false)).Distinct().ToList();

		// The call of its type that every route has: one on each route.
		if (routes.All(static r => r.Operands == 1))
			return Decision.Of(Source.Operand, "O");

		var parameters = Parameters(rule.Type);
		var sets       = routes.Select(r => r.Names.Split(',', StringSplitOptions.RemoveEmptyEntries).Where(n => parameters.Contains(n)).Order(StringComparer.Ordinal).ToArray()).ToList();

		// A name of a value written under a repetition is an array, and no constructor takes an array for a string.
		if (Walk(alternatives, true).OfType<Wrapped>().Where(static w => w.Wrap is Wrap.Star or Wrap.Plus).SelectMany(w => Walk([w.Inner], true).OfType<Named>()).Any(n => parameters.Contains(n.Name) && KindOfCapture(n.Inner, rule) != "text"))
			return Decision.Fail("O", "CS1503");

		if (!sets.Any(static s => s.Length > 0))
		{
			if (rule.Type == "object" && routes.Any(static r => r.Operands == 2))
				Features.Add("O-object-two-calls");

			// A call written only in X{0} reads nothing: it is no call on any route.
			if (!Walk(alternatives).Any(c => IsOperand(c, rule)) && Walk(alternatives, true).Any(c => IsOperand(c, rule)))
				Features.Add("O-zero-operand");

			return Decision.Fail("O", rule.LeftRecursive ? "E-LR" : "E-VALUE");
		}

		// Names choose the construction of the whole rule: an alternative that captures none of them is refused.
		if (sets.Select(static s => string.Join(",", s)).Distinct().Count() > 1)
		{
			Features.Add("O-partial");

			return Decision.Fail("O", "E-PARTIAL");
		}

		// A name two constructors take equally chooses none.
		if (sets[0].Contains("value"))
		{
			Features.Add("O-tie");

			return Decision.Fail("O", "E-VALUE");
		}

		// A name captured more than once on one route is not modelled: the published version decides (the first capture, or a
		// refusal where the captures differ in type).
		if (CapturesTwice(alternatives, parameters))
			return Decision.Of(Source.SamePublished, "O-twice");


		return Decision.Of(Source.Constructed, "O");
	}

	/// <summary>A name that nothing reads is not checked for the kind it captures, in any kind of rule.</summary>
	const bool UnreadNamesAreNotChecked = true;

	// A name is read by a guard or a selector, by a construction (it fills a parameter of the type the rule builds) and as a
	// member of a generated type.
	bool CapturesDifferentKinds(Rule rule, Decision[] decisions)
	{
		var reads = Walk(rule.Alternatives, true).Select(static e => e switch { Guard g => g.Reads, SwitchOn o => o.Reads, _ => null }).OfType<string>().ToHashSet(StringComparer.Ordinal);
		var all   = false;

		if (rule.Kind == Kind.Untyped && !_composites.ContainsValue(rule))
			all = true;
		else if (rule.Kind == Kind.O && !decisions.All(static d => d.Source == Source.Operand))
			reads.UnionWith(Parameters(rule.Type));

		foreach (var group in Walk(rule.Alternatives, true).OfType<Named>().GroupBy(static n => n.Name))
		{
			if (UnreadNamesAreNotChecked && !all && !reads.Contains(group.Key))
				continue;

			if (group.Select(n => KindOfCapture(n.Inner, rule)).Distinct().Count() > 1)
				return true;
		}

		return false;
	}

	// What a name captures: text (a literal, a group, a call of an untyped or an own-text string rule), or a value of the type
	// of the building rule it calls. Two rules of one type give one kind; text and a value are two kinds.
	string KindOfCapture(Expr inner, Rule self)
	{
		while (true)
		{
			switch (inner)
			{
				case Group group:
					inner = group.Inner;
					continue;

				case Wrapped { Wrap: Wrap.Optional or Wrap.Star or Wrap.Plus } wrapped:
					inner = wrapped.Inner;
					continue;

				case Call call:
				{
					var (type, builds) = LeafInfo(call.Rule.Name);

					return type is null || type == "string" && !builds ? "text" : "value of " + type;
				}

				case RuleRef reference:
					return reference.Rule.Kind switch
					{
						Kind.Untyped => "text",
						Kind.C       => Impure(reference.Rule) ? "value of string" : "text",
						_            => "value of " + reference.Rule.Type,
					};

				case SelfCall:
					return self.Kind == Kind.C ? Impure(self) ? "value of string" : "text" : "value of " + self.Type;

				default:
					return "text";
			}
		}
	}

	static bool CapturesTwice(IReadOnlyList<Expr> alternatives, string[] parameters)
	{
		foreach (var sequence in Walk(alternatives).OfType<Sequence>())
		{
			var seen = new HashSet<string>(StringComparer.Ordinal);

			foreach (var item in sequence.Items)
			{
				foreach (var name in Walk([item]).OfType<Named>().Select(static n => n.Name).Where(parameters.Contains).Distinct())
				{
					if (!seen.Add(name))
						return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// The call is of a rule whose type the rule under reading takes as an operand: the same type, or any typed rule where the
	/// rule is an object (every value converts to it).
	/// </summary>
	bool IsOperand(Expr expr, Rule rule)
	{
		var callee = IsCall(expr) ? CalleeType(expr, rule) : null;

		return callee is not null && (callee == rule.Type || rule.Type == "object") && !(expr is RuleRef { Rule.Kind: Kind.A });
	}

	/// <summary>A name directly on a call: <c>x: R</c>, <c>x: R?</c>, <c>x: R+</c>, <c>x: (R)</c>.</summary>
	static bool NamesACall(Expr inner)
	{
		return inner switch
		{
			Group { Inner: var g }                                                  => NamesACall(g),
			Wrapped { Wrap: Wrap.Optional or Wrap.Plus or Wrap.Star, Inner: var w } => NamesACall(w),
			_                                                                       => IsCall(inner),
		};
	}

	static string Join(string a, string b)
	{
		if (a.Length == 0)
			return b;

		if (b.Length == 0 || a == b)
			return a;

		return string.Join(",", (a + "," + b).Split(',').Distinct().Order(StringComparer.Ordinal));
	}

	static HashSet<(int Operands, string Names)> Product(HashSet<(int Operands, string Names)> a, HashSet<(int Operands, string Names)> b)
	{
		return [.. a.SelectMany(x => b.Select(y => (Math.Min(2, x.Operands + y.Operands), Join(x.Names, y.Names))))];
	}

	/// <summary>
	/// The ways through an expression: how many operands each reads (two stand for more than one) and the names it writes.
	/// A name directly on a call makes the call the author's, not an operand.
	/// </summary>
	HashSet<(int Operands, string Names)> RoutesOf(Expr expr, Rule rule, bool author)
	{
		switch (expr)
		{
			case Call or RuleRef or SelfCall:
				return !author && IsOperand(expr, rule) ? [(1, "")] : [(0, "")];

			case Named named:
				return [.. RoutesOf(named.Inner, rule, author || NamesACall(named.Inner)).Select(r => (r.Operands, Join(r.Names, named.Name)))];

			case Bind bind:
				return RoutesOf(bind.Inner, rule, author);

			case Sequence sequence:
				return sequence.Items.Aggregate(new HashSet<(int Operands, string Names)> { (0, "") }, (all, item) => Product(all, RoutesOf(item, rule, author)));

			case Choice choice:
				return [.. choice.Alternatives.SelectMany(a => RoutesOf(a, rule, author))];

			case Group group:
				return RoutesOf(group.Inner, rule, author);

			case Wrapped { Wrap: Wrap.Optional } optional:
				return [.. RoutesOf(optional.Inner, rule, author), (0, "")];

			case Wrapped { Wrap: Wrap.Star } star:
			{
				var inner = RoutesOf(star.Inner, rule, author);

				// With no turn a name written in the repetition is an empty capture.
				return [.. Product(inner, inner), (0, inner.Aggregate("", static (all, r) => Join(all, r.Names)))];
			}

			case Wrapped { Wrap: Wrap.Plus } plus:
			{
				var inner = RoutesOf(plus.Inner, rule, author);

				return Product(inner, inner);
			}

			case Wrapped { Wrap: Wrap.Zero } zero:
				// X{0} gives no call, but a name written in it is written: it fills the constructor with an empty text.
				return [.. RoutesOf(zero.Inner, rule, author).Select(r => (0, r.Names))];

			default:
				return [(0, "")];
		}
	}

	// The features the allow-list asks for: what the grammar holds, whatever the input.
	void Describe(Rule rule)
	{
		var nodes = Walk(rule.Alternatives).ToList();
		var calls = nodes.Where(IsCall).ToList();

		if (nodes.OfType<Named>().Any(static n => n.Name == "items"))
			Features.Add("compiler-name");

		switch (rule.Kind)
		{
			case Kind.C when rule.OwnText:
				break;

			case Kind.C:
			{
				var taken = _shape.Type.How == TypeSaid.Taken;

				foreach (var call in calls.Where(static c => c is not Call { Atom: null }))
				{
					switch (Classify(call, rule))
					{
						case Piece.Built:
							Features.Add(taken ? "C-taken-built" : "C-built");
							break;

						case Piece.Char:
							Features.Add("C-char");
							break;

						case Piece.Other:
							Features.Add("C-non-text");
							break;
					}
				}

				if (_shape.Trivia && nodes.OfType<Sequence>().Any(static s => s.Items.Length >= 2))
					Features.Add("C-seam");

				if (nodes.OfType<Named>().Any(static n => n.Inner is Call { Atom: null }))
					Features.Add("C-names-of-constructor");

				if (nodes.OfType<Wrapped>().Any(static w => w is { Wrap: Wrap.Optional, Inner: Call or RuleRef }))
					Features.Add("C-optional-call");

				if (calls.OfType<RuleRef>().Any(r => r.Rule.Kind == Kind.Untyped && Walk(r.Rule.Alternatives).Where(IsCall).Any(c => Classify(c, rule) == Piece.Built)))
					Features.Add("C-read-through");

				break;
			}

			case Kind.A:
				if (calls.Any(c => c is not SelfCall && CalleeType(c, rule) == rule.Type && !(c is Call { Rule.Name: "Word" or "Nm" or "Dg" or "Sp" or "Num" })))
					Features.Add("A-text-rule");

				if (MixedOverBytes(rule))
					Features.Add("A-mixed-bytes");

				if (nodes.OfType<Named>().Any(static n => n.Inner is Literal or Lexeme or Call { Rule.Name: "Plain" }))
					Features.Add("A-named-text");

				if (calls.Any(c => c is SelfCall || c is RuleRef { Rule.Kind: Kind.A }))
					Features.Add("A-splice");

				if (calls.OfType<RuleRef>().Any(r => r.Rule.Kind == Kind.Untyped))
					Features.Add("A-read-through");

				break;

			case Kind.O:
				if (rule.Groups.Any(static d => d.Source == Source.Operand) && nodes.OfType<Named>().Any(n => Parameters(rule.Type).Contains(n.Name)))
					Features.Add("O-operand-beats-construction");

				break;
		}
	}

	// ---- The value of an input ----

	// An untyped rule's generated type holds a raw capture where a name captures text: over bytes that is refused.
	bool RawCapture(Rule rule)
	{
		return Walk(rule.Alternatives, true).OfType<Named>().Any(n => KindOfCapture(n.Inner, rule) == "text");
	}

	/// <summary>A name of the rule captures text.</summary>
	internal bool NameIsText(Rule rule, string name)
	{
		return Walk(rule.Alternatives, true).OfType<Named>().Any(n => n.Name == name && KindOfCapture(n.Inner, rule) == "text");
	}

	/// <summary>The expectation for one input in one mode.</summary>
	public Expectation Expected(Mode mode, string input)
	{
		if (Decision.Source == Source.Undecided)
			return new Expectation(Source.Undecided, null, null, Decision.Question, Decision.Row);

		if (Decision.Source == Source.Error)
			return new Expectation(Source.Error, null, Decision.ErrorId, null, Decision.Row);

		if (Decision.Source == Source.SamePublished)
		{
			// An untyped published rule with names: what its generated type holds, where the oracle can say it.
			if (Decision.Row == "O" && ReferenceEquals(Start, Tested) && Start.Kind == Kind.Untyped)
			{
				try
				{
					var run = new Evaluation(this, mode, input).Run();

					if (run.Source is Source.NoMatch or Source.SamePublished)
						return run;
				}
				catch (Exception e) when (e is InvalidOperationException or KeyNotFoundException)
				{
				}
			}

			return new Expectation(Source.SamePublished, null, mode == Mode.B && RawCapture(Tested) ? "GRAM4026" : null, null, Decision.Row);
		}

		// Over bytes a string cannot be joined from a built string or a character without decoding.
		if (mode == Mode.B && _rules.Any(r => r.Kind == Kind.C && Impure(r)))
			return new Expectation(Source.Error, null, "GRAM4026", null, "C");

		// Over bytes one array cannot hold both the bytes of a text call and the string of a building one.
		if (mode == Mode.B && _rules.Any(MixedOverBytes))
			return new Expectation(Source.Error, null, "GRAM4026", null, "A");

		try
		{
			return new Evaluation(this, mode, input).Run();
		}
		catch (BytesIntoConstructor)
		{
			return new Expectation(Source.Error, null, "CS1503", null, "O");
		}
		catch (Exception e) when (e is InvalidOperationException or KeyNotFoundException)
		{
			return new Expectation(Source.Undecided, null, null, e.Message, Start.Groups[0].Row);
		}
	}
}
