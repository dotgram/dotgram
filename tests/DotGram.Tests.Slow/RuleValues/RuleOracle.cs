using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace DotGram.Tests.RuleValues;

// The oracle of the rule-value gate: what a grammar of the gate must give, written from the rule alone and never from the
// generator. A shape says how its rules are written; the oracle reads that the way the specification reads a grammar: it
// classifies each rule it calls, takes the routes of the body, picks the source of the value from the decision table, and
// then parses the input by the grammar (ordered choice, greedy repetition) to find the value on the route the input takes.

/// <summary>Where the value of a rule comes from, or why it has none.</summary>
enum Source
{
	/// <summary>Every alternative says it with <c>=&gt;</c>.</summary>
	Explicit,

	/// <summary>The one operand of the rule's type.</summary>
	Operand,

	/// <summary>Built from the names the rule writes.</summary>
	Constructed,

	/// <summary>The rule's own text or bounds.</summary>
	Extent,

	/// <summary>The operand on a route that reads one, the rule's own text on a route that reads none.</summary>
	PerRoute,

	/// <summary>An array of every operand of the element type, in order.</summary>
	Sequence,

	/// <summary>A host function.</summary>
	External,

	/// <summary>The grammar is refused: <see cref="Expectation.ErrorId"/> says why.</summary>
	Error,

	/// <summary>The specification does not say: <see cref="Expectation.Question"/> says what is missing.</summary>
	Undecided,

	/// <summary>The specification leaves it open: no answer is expected.</summary>
	Open,
}

/// <summary>
/// What the oracle expects of one input.
/// </summary>
/// <param name="Source">Where the value comes from.</param>
/// <param name="Value">The value as the runner prints it (null unless the source gives one).</param>
/// <param name="ErrorId">The identifier of the refusal, for <see cref="Source.Error"/>.</param>
/// <param name="Question">What the specification does not say, for <see cref="Source.Undecided"/> and <see cref="Source.Open"/>.</param>
/// <param name="Row">The row of the decision table that chose the source, per the rule that is published.</param>
sealed record Expectation(Source Source, string? Value, string? ErrorId, string? Question, string Row)
{
	/// <summary>The grammar is built and gives <see cref="Value"/>.</summary>
	public bool IsValue => Value is not null && Source is not (Source.Error or Source.Undecided or Source.Open);

	/// <summary>The grammar is refused.</summary>
	public bool IsError => Source == Source.Error;

	/// <summary>The oracle has no answer to compare with.</summary>
	public bool IsUnknown => Source is Source.Undecided or Source.Open;

	public override string ToString()
	{
		return Source switch
		{
			Source.Error     => "error " + ErrorId,
			Source.Undecided => "undecided " + Question,
			Source.Open      => "open " + Question,
			_                => Value + " (" + Source + ", row " + Row + ")",
		};
	}
}

/// <summary>
/// The oracle.
/// </summary>
static class RuleOracle
{
	/// <summary>
	/// Whether a rule that takes the type of a text rule hands that rule's value on where it reads one: <c>Start : Word =
	/// '&lt;' &amp; Word &amp; '&gt;'</c> gives Word's value. When false the rule is decided exactly as the declaration of
	/// its type is (its own text), and no rule takes a text rule's type to be anything but its own text. Set by the
	/// environment variable <c>DOTGRAM_RULEVALUES_TAKEN_TEXT_PASSES_ON</c> (<c>0</c> or <c>false</c> for false).
	/// </summary>
	public static bool TakenTextPassesOn { get; } =
		Environment.GetEnvironmentVariable("DOTGRAM_RULEVALUES_TAKEN_TEXT_PASSES_ON") is not ("0" or "false" or "False");

	/// <summary>What the oracle expects of the input, with the switch as the environment sets it.</summary>
	public static Expectation Expected(RuleShape shape, Mode mode, string input)
	{
		return Expected(shape, mode, input, TakenTextPassesOn);
	}

	/// <summary>What the oracle expects of the input, with the switch given.</summary>
	public static Expectation Expected(RuleShape shape, Mode mode, string input, bool takenTextPassesOn)
	{
		var model = Analyses.GetOrAdd((shape, takenTextPassesOn), static key => new Grammar(key.Item1, key.Item2));

		return model.Expected(mode, input);
	}

	/// <summary>The static reading of a shape (the decision per rule), for the allow-list and the invariants.</summary>
	public static Grammar Describe(RuleShape shape, bool takenTextPassesOn)
	{
		return Analyses.GetOrAdd((shape, takenTextPassesOn), static key => new Grammar(key.Item1, key.Item2));
	}

	static readonly System.Collections.Concurrent.ConcurrentDictionary<(RuleShape, bool), Grammar> Analyses = new();
}

/// <summary>A helper as the specification classifies it: what it hands to a caller, its type, and what it reads.</summary>
sealed record HelperInfo(string Name, string? Type, bool Making, bool Plumbing = false)
{
	/// <summary>
	/// The helper's kind, in the table of the specification. Text-kind: Bare, Span, G, Gs, C4, Mid, Mids, Ow, Ows.
	/// Making: Word, Sp, Bx, Bxa, Lf, Lfa, Item, Num, O, Ch, Mw, Mws, Mx (and the two that only fill names, Nm and Dg).
	/// A rule that takes a text rule's type is its own text where the switch says so.
	/// </summary>
	public static HelperInfo Of(string name, bool takenTextPassesOn)
	{
		return name switch
		{
			"Word" or "Nm" or "Dg" => new(name, "string", true),
			"Bare" or "G" or "C4" or "Mid" or "Ow" => new(name, "string", false),
			"Mw" or "Mx"           => new(name, "string", takenTextPassesOn),
			"Sp"                   => new(name, "SourceSpan", true),
			"Span" or "Gs" or "Mids" or "Ows" => new(name, "SourceSpan", false),
			"Mws"                  => new(name, "SourceSpan", takenTextPassesOn),
			"Bx" or "Bxa"          => new(name, "Box", true),
			"Lf" or "Lfa"          => new(name, "Pr", true),
			"Item"                 => new(name, "Item", true),
			"Num"                  => new(name, "int", true),
			"O"                    => new(name, "object", true),
			"Ch"                   => new(name, "char", true),
			"Plain"                => new(name, null, false, Plumbing: true),
			_                      => throw new InvalidOperationException(name),
		};
	}
}

/// <summary>A value as the runner prints it, with what a constructor or a collection needs of it.</summary>
sealed record Val(string Type, string Raw, string Print)
{
	public static Val Str(string text)
	{
		return new Val("string", text, "\"" + text + "\"");
	}

	public static Val Bytes(string text)
	{
		return new Val("bytes", text, "bytes\"" + text + "\"");
	}

	public static Val Span(int start, int end)
	{
		var print = "[" + start.ToString(CultureInfo.InvariantCulture) + ".." + end.ToString(CultureInfo.InvariantCulture) + ")";

		return new Val("SourceSpan", print, print);
	}

	public static Val Int(int number)
	{
		var text = number.ToString(CultureInfo.InvariantCulture);

		return new Val("int", text, text);
	}

	public static Val Char(char c)
	{
		return new Val("char", c.ToString(), "'" + c + "'");
	}

	public static Val Host(string type, string print)
	{
		return new Val(type, print, print);
	}
}

/// <summary>The source and the decision of one rule, without an input.</summary>
sealed record Decision(Source Source, string Row, string? ErrorId, string? Question)
{
	public static Decision Of(Source source, string row)
	{
		return new Decision(source, row, null, null);
	}

	public static Decision Fail(string row, string id)
	{
		return new Decision(Source.Error, row, id, null);
	}

	public static Decision Ask(string row, string question)
	{
		return new Decision(Source.Undecided, row, null, question);
	}
}

/// <summary>How a rule says its type.</summary>
enum Said
{
	Own,
	Taken,
	Sequence,
	None,
}

/// <summary>A call of a rule of the grammar that is not a helper: the rule under test from a caller, which is a rule too.</summary>
sealed class RuleRef(Rule rule) : Expr
{
	public Rule Rule { get; } = rule;
}

/// <summary>
/// A rule of the grammar under reading: the rule under test, or the rule that calls it (<c>Start</c>).
/// </summary>
sealed class Rule
{
	public required string Name { get; init; }

	public required Said Said { get; init; }

	/// <summary>The C# type, of the element for a sequence; null for a rule with no type.</summary>
	public required string? Type { get; init; }

	public required IReadOnlyList<Expr> Alternatives { get; init; }

	public bool LeftRecursive { get; init; }

	/// <summary>The rule whose type this rule takes, when it takes it from one of the grammar's own.</summary>
	public Rule? TakenRule { get; init; }

	/// <summary>The type is a text type: string or SourceSpan.</summary>
	public bool IsTextType => Type is "string" or "SourceSpan";

	/// <summary>What the decision, the kind and the routes came to; set by the grammar's reading.</summary>
	public Decision? Decision { get; set; }

	/// <summary>The rule hands its value to a caller (an operand), where false it is text or plumbing.</summary>
	public bool Making { get; set; }

	/// <summary>The route profile: how many operands each route reads (capped at 2), and the names it writes.</summary>
	public HashSet<(int Operands, string Names)> Routes { get; set; } = [];
}

/// <summary>
/// The reading of one shape: its rules, the decision for each, and the evaluation of an input.
/// </summary>
sealed class Grammar
{
	readonly RuleShape _shape;
	readonly bool _passesOn;
	readonly List<Rule> _rules = [];

	/// <summary>The rule the shape is published through: <c>Start</c>.</summary>
	public Rule Start { get; }

	/// <summary>The rule under test (the same as <see cref="Start"/> for a shape nobody calls).</summary>
	public Rule Tested { get; }

	/// <summary>The decision of <see cref="Start"/> after those of the rules it calls: the first error, or its own.</summary>
	public Decision Decision { get; private set; } = Decision.Of(Source.Extent, "");

	/// <summary>A grammar that has a rule decided as a text type that hands on an operand and an extent, over bytes.</summary>
	public bool MixesTextAndValueOverBytes { get; private set; }

	public Grammar(RuleShape shape, bool takenTextPassesOn)
	{
		_shape  = shape;
		_passesOn = takenTextPassesOn;

		var type = shape.Type;
		var body = new List<Expr>();

		foreach (var alternative in shape.Alternatives)
			body.Add(Replace(alternative, shape));

		Tested = new Rule
		{
			Name          = shape.RuleName,
			Said          = type.How switch
			{
				TypeSaid.Own                                                    => Said.Own,
				TypeSaid.Taken when Info(type.Taken!.Name).Type is not null     => Said.Taken,
				TypeSaid.Sequence                                               => Said.Sequence,
				_                                                               => Said.None,
			},
			Type          = type.How == TypeSaid.None ? null : type.Taken is { } taken ? Info(taken.Name).Type : type.ClrType,
			Alternatives  = body,
			LeftRecursive = shape.LeftRecursive,
		};

		_rules.Add(Tested);

		if (shape.Template)
		{
			Start = new Rule
			{
				Name         = "Start",
				Said         = Tested.Said == Said.None ? Said.None : Said.Taken,
				Type         = Tested.Type,
				Alternatives = [new RuleRef(Tested)],
				TakenRule    = Tested,
			};
		}
		else
		{
			switch (shape.SeenFrom)
			{
				case SeenFrom.Itself:
					Start = Tested;
					break;

				case SeenFrom.InSequence:
					Start = new Rule
					{
						Name         = "Start",
						Said         = Said.Sequence,
						Type         = type.ClrType,
						Alternatives = [new Sequence(new RuleRef(Tested), new Literal(";"), new Call(RulePrinter.BesideInSequence(type), Atom.Built))],
					};
					break;

				case SeenFrom.TakenBy:
					Start = new Rule
					{
						Name         = "Start",
						Said         = Tested.Said == Said.None ? Said.None : Said.Taken,
						Type         = Tested.Type,
						Alternatives = [new Sequence(new Literal("["), new RuleRef(Tested), new Literal("]"))],
						TakenRule    = Tested,
					};
					break;

				default:
					Start = new Rule
					{
						Name         = "Start",
						Said         = Tested.Said == Said.None ? Said.None : Said.Taken,
						Type         = Tested.Type,
						Alternatives = [new Sequence([.. RuleShapes.Filling(type.Family)!, new RuleRef(Tested)])],
						TakenRule    = Tested,
					};
					break;
			}
		}

		if (!ReferenceEquals(Start, Tested))
			_rules.Add(Start);

		Read();
	}

	HelperInfo Info(string name)
	{
		return HelperInfo.Of(name, _passesOn);
	}

	/// <summary>The rules of the grammar: the rule under test and, where something calls it, the rule that does.</summary>
	public IReadOnlyList<Rule> Rules => _rules;

	/// <summary>The switch this grammar was read with.</summary>
	public bool TakenTextPassesOn => _passesOn;

	/// <summary>The names the rule writes fill a construction of its type (completely or not), or are written on some routes only.</summary>
	public bool WritesConstructionNames(Rule rule)
	{
		return Fill(rule, out _, out _) != FillKind.None;
	}

	/// <summary>What a helper hands to its caller, as the specification classifies it (with the switch of this grammar).</summary>
	public HelperInfo HelperOf(string name)
	{
		return Info(name);
	}

	/// <summary>The call is an operand of the rule (what a name directly on it does not make it).</summary>
	public bool IsOperandOf(Expr call, Rule rule)
	{
		return IsOperand(call, rule, rule.Making);
	}

	/// <summary>The expression is a name directly on a call.</summary>
	public static bool IsNameOnCall(Expr inner)
	{
		return NamesACall(inner);
	}

	// The parameter of a template stands for a call of the rule that is passed for it.
	static Expr Replace(Expr expr, RuleShape shape)
	{
		switch (expr)
		{
			case Parameter:
				return new Call(shape.TemplateArgument!, Atom.Implicit);

			case Named named:
				return new Named(named.Name, Replace(named.Inner, shape));

			case Sequence sequence:
				return new Sequence([.. sequence.Items.Select(i => Replace(i, shape))]);

			case Choice choice:
				return new Choice([.. choice.Alternatives.Select(a => Replace(a, shape))]);

			case Group group:
				return new Group(Replace(group.Inner, shape));

			case Wrapped wrapped:
				return new Wrapped(wrapped.Wrap, Replace(wrapped.Inner, shape));

			default:
				return expr;
		}
	}

	// ---- The decision per rule ----

	/// <summary>The type is a text type that the rule is decided as the declaration of: own, or taken where the switch says so.</summary>
	bool DecidedAsOwnText(Rule rule)
	{
		return rule.IsTextType && (rule.Said == Said.Own || rule.Said == Said.Taken && !_passesOn);
	}

	bool DecidedAsTakenText(Rule rule)
	{
		return rule.IsTextType && rule.Said == Said.Taken && _passesOn;
	}

	static bool Fits(string? type, string? operand)
	{
		return type is not null && operand is not null && (type == operand || type == "object");
	}

	void Read()
	{
		foreach (var rule in _rules)
		{
			// The kind of a rule is found first, with its own calls of itself read as text (the least fixpoint); a rule
			// that reads an operand on some route is then making, and its calls of itself read as operands. A rule with no
			// type is plumbing; a text rule is text unless it has a construction (or, taken, reads an operand); any other
			// rule makes its value.
			var text = DecidedAsOwnText(rule) || DecidedAsTakenText(rule);

			rule.Making = rule.Said != Said.None && !text;
			rule.Routes = Routes(rule, selfIsOperand: rule.Making);

			if (text)
			{
				var making = Fill(rule, out _, out _) != FillKind.None || DecidedAsTakenText(rule) && rule.Routes.Any(static r => r.Operands > 0);

				if (making)
				{
					rule.Making = true;
					rule.Routes = Routes(rule, selfIsOperand: true);
				}
			}

			rule.Decision = Decide(rule);
		}

		var first = _rules.Select(static r => r.Decision!).FirstOrDefault(static d => d.Source is Source.Error or Source.Undecided);

		Decision = first ?? Start.Decision!;

		foreach (var rule in _rules)
			if (rule.Decision!.Source == Source.PerRoute && rule.Type == "string" && rule.Routes.Any(static r => r.Operands == 0) && rule.Routes.Any(static r => r.Operands > 0))
				MixesTextAndValueOverBytes = true;

		// A helper that is a rule with a text route beside a route that hands a string on (Mx) is refused over bytes.
		if (_passesOn && Called().Any(static h => h.Name == "Mx"))
			MixesTextAndValueOverBytes = true;
	}

	IEnumerable<Helper> Called()
	{
		var found = new List<Helper>();

		void Visit(Expr expr)
		{
			switch (expr)
			{
				case Call call:
					found.Add(call.Rule);
					break;

				case Named named:
					Visit(named.Inner);
					break;

				case Sequence sequence:
					foreach (var item in sequence.Items)
						Visit(item);

					break;

				case Choice choice:
					foreach (var alternative in choice.Alternatives)
						Visit(alternative);

					break;

				case Group group:
					Visit(group.Inner);
					break;

				case Wrapped wrapped:
					Visit(wrapped.Inner);
					break;
			}
		}

		foreach (var rule in _rules)
			foreach (var alternative in rule.Alternatives)
				Visit(alternative);

		return found;
	}

	// The names a type's constructor or initializer takes.
	static string[] Parameters(string? type)
	{
		return type switch
		{
			"string"     => ["c", "count"],
			"SourceSpan" => ["start", "length"],
			"Box"        => ["name", "inner", "value"],
			"Pr"         => ["t", "l", "r"],
			"Item"       => ["x"],
			_            => [],
		};
	}

	enum FillKind
	{
		None,
		Ok,
		Partial,
		SpanConstructor,
		Tie,
		SomeRoutes,
	}

	/// <summary>
	/// What the names the rule writes make of its type: nothing, a construction, an incomplete one, a refused span
	/// constructor, a tie between constructors, or names written on some routes only (the specification does not say).
	/// </summary>
	FillKind Fill(Rule rule, out string[] names, out bool anyRoute)
	{
		var parameters = Parameters(rule.Type);
		var sets       = rule.Routes.Select(r => r.Names.Split(',', StringSplitOptions.RemoveEmptyEntries).Where(n => parameters.Contains(n)).OrderBy(static n => n, StringComparer.Ordinal).ToArray()).ToList();

		names    = [];
		anyRoute = sets.Any(static s => s.Length > 0);

		if (!anyRoute)
			return FillKind.None;

		var distinct = sets.Select(static s => string.Join(",", s)).Distinct().ToList();

		if (distinct.Count > 1)
			return FillKind.SomeRoutes;

		names = sets[0];

		if (names.Contains("value"))
			return FillKind.Tie;

		switch (rule.Type)
		{
			case "string":
				return names.Length == 2 ? FillKind.Ok : FillKind.Partial;

			case "SourceSpan":
				if (rule.Said == Said.Own || rule.Said == Said.Taken && !_passesOn)
					return FillKind.SpanConstructor;

				return names.Length == 2 ? FillKind.Ok : FillKind.Partial;

			default:
				return FillKind.Ok;
		}
	}

	Decision Decide(Rule rule)
	{
		if (rule.Said == Said.None)
			return Decision.Ask("-", "a published rule with no type: the specification gives its value no source");

		var fill = Fill(rule, out _, out _);

		// Row 4: a sequence collects the operands of its element type.
		if (rule.Said == Said.Sequence)
		{
			if (Mentions(rule, static e => e is SelfCall))
				return Decision.Ask("4", "a rule that is an array of T calls itself: the call is an array, not an element, and the specification does not say what it collects");

			if (!rule.Alternatives.Any(a => HasOperand(a, rule)))
				return Decision.Fail("4", "E-SEQ");

			return Decision.Of(Source.Sequence, "4");
		}

		var text = DecidedAsOwnText(rule) || DecidedAsTakenText(rule);

		// Rows 5, 5a and 6: a rule that calls itself first.
		if (rule.LeftRecursive)
		{
			if (DecidedAsOwnText(rule))
				return Decision.Of(Source.Extent, "5");

			if (DecidedAsTakenText(rule))
			{
				var reads = rule.Routes.Any(static r => r.Operands > 0);

				if (!reads && fill == FillKind.None)
					return Decision.Of(Source.Extent, "5");

				if (reads)
					return Decision.Fail("5a", "E-LR");

				return Decision.Ask("5, 5a", "a left-recursive rule that takes a text rule's type, reads no operand and writes names that fill a construction: it is not text-kind (it has a construction) and reads no operand");
			}

			return Decision.Fail("6", "E-LR");
		}

		if (fill == FillKind.SomeRoutes)
			return Decision.Ask("7, 10", "names that fill a construction written on some routes of the rule and not on others");

		if (fill == FillKind.Tie)
			return Decision.Ask("7, 10", "a name that two constructors of the type take equally");

		// Rows 7 and 8: the declaration of a text type.
		if (DecidedAsOwnText(rule))
		{
			switch (fill)
			{
				case FillKind.None:
					return Decision.Of(Source.Extent, "8");

				case FillKind.SpanConstructor:
					return Decision.Fail("7", "E-SPANCTOR");

				case FillKind.Partial:
					return Decision.Fail("7", "E-PARTIAL");

				default:
					return Decision.Of(Source.Constructed, "7");
			}
		}

		// Row 9: one operand on every route.
		if (rule.Routes.All(static r => r.Operands == 1))
			return Decision.Of(Source.Operand, "9");

		// Row 10: names that fill a construction.
		if (fill != FillKind.None)
			return fill == FillKind.Partial ? Decision.Fail("10", "E-PARTIAL") : Decision.Of(Source.Constructed, "10");

		// Row 11: a taken text rule with at most one operand on every route.
		if (text && rule.Routes.All(static r => r.Operands <= 1))
			return Decision.Of(Source.PerRoute, "11");

		return Decision.Fail("12", "E-VALUE");
	}

	bool Mentions(Rule rule, Func<Expr, bool> test)
	{
		bool Visit(Expr expr)
		{
			if (test(expr))
				return true;

			return expr switch
			{
				Named named       => Visit(named.Inner),
				Sequence sequence => sequence.Items.Any(Visit),
				Choice choice     => choice.Alternatives.Any(Visit),
				Group group       => Visit(group.Inner),
				Wrapped wrapped   => Visit(wrapped.Inner),
				_                 => false,
			};
		}

		return rule.Alternatives.Any(Visit);
	}

	// ---- Operands and routes ----

	/// <summary>The call is of a rule that makes a value of the type the rule under reading takes as an operand.</summary>
	bool IsOperand(Expr call, Rule rule, bool selfIsOperand)
	{
		switch (call)
		{
			case Call { Atom: null }:
				return false;

			case Call c:
			{
				var info = Info(c.Rule.Name);

				return info.Making && !info.Plumbing && Fits(rule.Type, info.Type);
			}

			case RuleRef reference:
				return reference.Rule.Making && reference.Rule.Said != Said.Sequence && Fits(rule.Type, reference.Rule.Type);

			case SelfCall:
				return selfIsOperand && rule.Said != Said.Sequence && Fits(rule.Type, rule.Type) && !rule.LeftRecursive;

			default:
				return false;
		}
	}

	static bool IsCall(Expr expr)
	{
		return expr is Call or SelfCall or RuleRef;
	}

	/// <summary>A name directly on a call: <c>x: R</c>, <c>x: R?</c>, <c>x: R+</c>, <c>x: (R)</c>.</summary>
	static bool NamesACall(Expr inner)
	{
		return inner switch
		{
			Group { Inner: var g }                                                                      => NamesACall(g),
			Wrapped { Wrap: Wrap.Optional or Wrap.Plus or Wrap.Star, Inner: var w }                     => NamesACall(w),
			_                                                                                           => IsCall(inner),
		};
	}

	bool HasOperand(Expr expr, Rule rule)
	{
		var found = false;

		Walk(expr, rule, selfIsOperand: false, (node, author) =>
		{
			if (!author && IsOperand(node, rule, selfIsOperand: false))
				found = true;
		});

		return found;
	}

	/// <summary>Visits every call of an expression outside lookaheads and <c>{0}</c>, saying whether a name is directly on it.</summary>
	void Walk(Expr expr, Rule rule, bool selfIsOperand, Action<Expr, bool> visit)
	{
		switch (expr)
		{
			case Call or SelfCall or RuleRef:
				visit(expr, false);
				break;

			case Named named when NamesACall(named.Inner):
				VisitNamed(named.Inner, rule, selfIsOperand, visit);
				break;

			case Named named:
				Walk(named.Inner, rule, selfIsOperand, visit);
				break;

			case Sequence sequence:
				foreach (var item in sequence.Items)
					Walk(item, rule, selfIsOperand, visit);

				break;

			case Choice choice:
				foreach (var alternative in choice.Alternatives)
					Walk(alternative, rule, selfIsOperand, visit);

				break;

			case Group group:
				Walk(group.Inner, rule, selfIsOperand, visit);
				break;

			case Wrapped { Wrap: Wrap.Ahead or Wrap.NotAhead or Wrap.Zero }:
				break;

			case Wrapped wrapped:
				Walk(wrapped.Inner, rule, selfIsOperand, visit);
				break;
		}
	}

	void VisitNamed(Expr inner, Rule rule, bool selfIsOperand, Action<Expr, bool> visit)
	{
		switch (inner)
		{
			case Group group:
				VisitNamed(group.Inner, rule, selfIsOperand, visit);
				break;

			case Wrapped wrapped:
				VisitNamed(wrapped.Inner, rule, selfIsOperand, visit);
				break;

			default:
				visit(inner, true);
				break;
		}
	}

	/// <summary>
	/// The routes of the rule: for each way through the body, how many operands it reads (two stand for more than one) and
	/// the names it writes. A repetition that holds an operand counts as more than one on every route through it.
	/// </summary>
	HashSet<(int Operands, string Names)> Routes(Rule rule, bool selfIsOperand)
	{
		var all = new HashSet<(int, string)>();

		foreach (var alternative in rule.Alternatives)
			all.UnionWith(RoutesOf(alternative, rule, selfIsOperand, author: false));

		return all;
	}

	static string Join(string a, string b)
	{
		if (a.Length == 0)
			return b;

		if (b.Length == 0 || a == b)
			return a;

		return string.Join(",", (a + "," + b).Split(',').Distinct().OrderBy(static n => n, StringComparer.Ordinal));
	}

	static HashSet<(int, string)> Product(HashSet<(int Operands, string Names)> a, HashSet<(int Operands, string Names)> b)
	{
		var result = new HashSet<(int, string)>();

		foreach (var x in a)
			foreach (var y in b)
				result.Add((Math.Min(2, x.Operands + y.Operands), Join(x.Names, y.Names)));

		return result;
	}

	static readonly HashSet<(int, string)> Nothing = [(0, "")];

	HashSet<(int Operands, string Names)> RoutesOf(Expr expr, Rule rule, bool selfIsOperand, bool author)
	{
		switch (expr)
		{
			case Call or SelfCall or RuleRef:
				return !author && IsOperand(expr, rule, selfIsOperand) ? [(1, "")] : [(0, "")];

			case Named named:
			{
				var inner = RoutesOf(named.Inner, rule, selfIsOperand, author || NamesACall(named.Inner));

				return [.. inner.Select(r => (r.Operands, Join(r.Names, named.Name)))];
			}

			case Sequence sequence:
			{
				var result = Nothing;

				foreach (var item in sequence.Items)
					result = Product(result, RoutesOf(item, rule, selfIsOperand, author));

				return result;
			}

			case Choice choice:
			{
				var result = new HashSet<(int, string)>();

				foreach (var alternative in choice.Alternatives)
					result.UnionWith(RoutesOf(alternative, rule, selfIsOperand, author));

				return result;
			}

			case Group group:
				return RoutesOf(group.Inner, rule, selfIsOperand, author);

			case Wrapped { Wrap: Wrap.Optional } optional:
			{
				var result = RoutesOf(optional.Inner, rule, selfIsOperand, author);

				result.Add((0, ""));

				return result;
			}

			case Wrapped { Wrap: Wrap.Star } star:
			{
				var inner  = RoutesOf(star.Inner, rule, selfIsOperand, author);
				var result = Product(inner, inner);

				result.Add((0, ""));

				return result;
			}

			case Wrapped { Wrap: Wrap.Plus } plus:
			{
				var inner = RoutesOf(plus.Inner, rule, selfIsOperand, author);

				return Product(inner, inner);
			}

			default:
				return [(0, "")];
		}
	}

	// ---- The value of an input ----

	/// <summary>The expectation for one input in one mode.</summary>
	public Expectation Expected(Mode mode, string input)
	{
		var decision = Decision;

		if (decision.Source == Source.Undecided)
			return new Expectation(Source.Undecided, null, null, decision.Question, decision.Row);

		if (decision.Source == Source.Error)
			return new Expectation(Source.Error, null, decision.ErrorId, null, decision.Row);

		if (mode == Mode.B && MixesTextAndValueOverBytes)
			return new Expectation(Source.Error, null, "GRAM4026", null, "11");

		var evaluation = new Evaluation(this, mode, input);

		try
		{
			return evaluation.Run();
		}
		catch (UndecidedException e)
		{
			return new Expectation(Source.Undecided, null, null, e.Message, Decision.Row);
		}
	}

	sealed class UndecidedException(string message) : Exception(message)
	{
	}

	/// <summary>A parsed region: the rule or call it is, where it lies, and what it holds.</summary>
	sealed class Node(Expr expr, int start, int end)
	{
		public Expr Expr { get; } = expr;

		public int Start { get; } = start;

		public int End { get; } = end;

		public List<Node> Kids { get; } = [];

		/// <summary>The rule a call of one matched, and the tree of its body.</summary>
		public Rule? Rule { get; init; }

		public Node? Body { get; set; }
	}

	sealed class Evaluation
	{
		readonly Grammar _grammar;
		readonly Mode _mode;
		readonly string _input;
		readonly Dictionary<(Rule, int), Node?> _seeds = [];

		public Evaluation(Grammar grammar, Mode mode, string input)
		{
			_grammar = grammar;
			_mode    = mode;
			_input   = input;
		}

		public Expectation Run()
		{
			var root = MatchRule(_grammar.Start, 0);

			if (root is null || root.End != _input.Length)
				throw new UndecidedException("the oracle's reading of the grammar does not match the input " + _input);

			var value    = Eval(_grammar.Start, root);
			var decision = _grammar.Start.Decision!;

			// A text extent over bytes, in a grammar where a guard reads a name: the specification leaves it open.
			if (_mode == Mode.B && value.Type == "bytes" && _grammar.ReadsNameByGuard())
				return new Expectation(Source.Open, null, null, "a text extent whose name a guard reads, over bytes", decision.Row);

			return new Expectation(decision.Source, value.Print, null, null, decision.Row);
		}

		// ---- Matching: ordered choice, greedy repetition, a left-recursive rule grown from its seed ----

		Node? MatchRule(Rule rule, int position)
		{
			if (!rule.LeftRecursive)
				return MatchAlternatives(rule, position);

			if (_seeds.TryGetValue((rule, position), out var seeded))
				return seeded;

			Node? best = null;

			_seeds[(rule, position)] = null;

			while (true)
			{
				var next = MatchAlternatives(rule, position);

				if (next is null || best is not null && next.End <= best.End)
					break;

				best = next;
				_seeds[(rule, position)] = best;
			}

			_seeds.Remove((rule, position));

			return best;
		}

		Node? MatchAlternatives(Rule rule, int position)
		{
			foreach (var alternative in rule.Alternatives)
			{
				var matched = Match(alternative, position, rule);

				if (matched is not null)
					return matched;
			}

			return null;
		}

		Node? Match(Expr expr, int position, Rule rule)
		{
			switch (expr)
			{
				case Literal literal:
					return position + literal.Text.Length <= _input.Length && string.CompareOrdinal(_input, position, literal.Text, 0, literal.Text.Length) == 0
						? new Node(expr, position, position + literal.Text.Length)
						: null;

				case Lexeme:
					return Letters(position, 2) is { } end ? new Node(expr, position, end) : null;

				case AnyItems any:
					return position + any.Count <= _input.Length ? new Node(expr, position, position + any.Count) : null;

				case Guard or SwitchOn:
					return new Node(expr, position, position);

				case Call call:
					return MatchHelper(call, position);

				case SelfCall:
				{
					var inner = MatchRule(rule, position);

					return inner is null ? null : new Node(expr, position, inner.End) { Rule = rule, Body = inner };
				}

				case RuleRef reference:
				{
					var inner = MatchRule(reference.Rule, position);

					return inner is null ? null : new Node(expr, position, inner.End) { Rule = reference.Rule, Body = inner };
				}

				case Named named:
				{
					var inner = Match(named.Inner, position, rule);

					if (inner is null)
						return null;

					var node = new Node(expr, position, inner.End);

					node.Kids.Add(inner);

					return node;
				}

				case Group group:
					return Match(group.Inner, position, rule);

				case Sequence sequence:
				{
					var node = new Node(expr, position, position);
					var at   = position;
					var kids = new List<Node>();

					foreach (var item in sequence.Items)
					{
						var inner = Match(item, at, rule);

						if (inner is null)
							return null;

						kids.Add(inner);
						at = inner.End;
					}

					var whole = new Node(expr, position, at);

					whole.Kids.AddRange(kids);

					return whole;
				}

				case Choice choice:
				{
					foreach (var alternative in choice.Alternatives)
					{
						var inner = Match(alternative, position, rule);

						if (inner is null)
							continue;

						var node = new Node(expr, position, inner.End);

						node.Kids.Add(inner);

						return node;
					}

					return null;
				}

				case Wrapped wrapped:
					return MatchWrapped(wrapped, position, rule);

				default:
					throw new InvalidOperationException(expr.GetType().Name);
			}
		}

		Node? MatchWrapped(Wrapped wrapped, int position, Rule rule)
		{
			switch (wrapped.Wrap)
			{
				case Wrap.Optional:
				{
					var inner = Match(wrapped.Inner, position, rule);
					var node  = new Node(wrapped, position, inner?.End ?? position);

					if (inner is not null)
						node.Kids.Add(inner);

					return node;
				}

				case Wrap.Star or Wrap.Plus:
				{
					var at   = position;
					var kids = new List<Node>();

					while (Match(wrapped.Inner, at, rule) is { } turn && turn.End > at)
					{
						kids.Add(turn);
						at = turn.End;
					}

					if (wrapped.Wrap == Wrap.Plus && kids.Count == 0)
						return null;

					var node = new Node(wrapped, position, at);

					node.Kids.AddRange(kids);

					return node;
				}

				case Wrap.Zero:
					return new Node(wrapped, position, position);

				case Wrap.Ahead:
					return Match(wrapped.Inner, position, rule) is null ? null : new Node(wrapped, position, position);

				default:
					return Match(wrapped.Inner, position, rule) is null ? new Node(wrapped, position, position) : null;
			}
		}

		int? Letters(int position, int count)
		{
			if (position + count > _input.Length)
				return null;

			for (var i = 0; i < count; i++)
				if (_input[position + i] is < 'a' or > 'z')
					return null;

			return position + count;
		}

		int? Digits(int position, int count)
		{
			if (position + count > _input.Length)
				return null;

			for (var i = 0; i < count; i++)
				if (_input[position + i] is < '0' or > '9')
					return null;

			return position + count;
		}

		// The shape of what each helper reads.
		Node? MatchHelper(Call call, int position)
		{
			var name = call.Rule.Name;

			int? end = name switch
			{
				"Num" or "Dg"                                           => Digits(position, 2),
				"Ch"                                                    => Letters(position, 1),
				"Mid" or "Mids" or "C4" or "Ow" or "Ows" or "Mw" or "Mws" => Wrapped(position),
				"Mx"                                                    => Letters(position, 2) ?? Bracketed(position),
				_                                                       => Letters(position, 2),
			};

			return end is { } e ? new Node(call, position, e) : null;
		}

		int? Wrapped(int position)
		{
			if (position < _input.Length && _input[position] == '(' && Letters(position + 1, 2) is { } inner && inner < _input.Length && _input[inner] == ')')
				return inner + 1;

			return null;
		}

		int? Bracketed(int position)
		{
			if (position < _input.Length && _input[position] == '[' && Letters(position + 1, 2) is { } inner && inner < _input.Length && _input[inner] == ']')
				return inner + 1;

			return null;
		}

		// ---- Values ----

		Val Text(int start, int end)
		{
			var text = _input.Substring(start, end - start);

			return _mode == Mode.B ? Val.Bytes(text) : Val.Str(text);
		}

		Val Extent(Rule rule, Node node)
		{
			if (rule.Type == "SourceSpan")
				return Val.Span(node.Start, node.End);

			return Text(node.Start, node.End);
		}

		Val Eval(Rule rule, Node node)
		{
			var decision = rule.Decision!;

			switch (decision.Source)
			{
				case Source.Extent:
					return Extent(rule, node);

				case Source.Operand:
					return OperandValue(rule, node);

				case Source.PerRoute:
				{
					var operands = Operands(rule, node);

					return operands.Count == 1 ? Value(operands[0]) : Extent(rule, node);
				}

				case Source.Constructed:
					return Construct(rule, node);

				case Source.Sequence:
				{
					var parts = Operands(rule, node).Select(Value).ToList();

					return new Val("array", "", "[" + string.Join(",", parts.Select(static p => p.Print)) + "]");
				}

				default:
					throw new InvalidOperationException(decision.Source.ToString());
			}
		}

		Val OperandValue(Rule rule, Node node)
		{
			var operands = Operands(rule, node);

			if (operands.Count != 1)
				throw new UndecidedException("the oracle read " + operands.Count + " operands where the rule has one on every route");

			return Value(operands[0]);
		}

		/// <summary>The operand calls on the route a parse took, in order.</summary>
		List<Node> Operands(Rule rule, Node node)
		{
			var found = new List<Node>();

			void Visit(Node n, bool author)
			{
				switch (n.Expr)
				{
					case Call or SelfCall or RuleRef:
						if (!author && _grammar.IsOperand(n.Expr, rule, selfIsOperand: rule.Making))
							found.Add(n);

						break;

					case Named named:
						foreach (var kid in n.Kids)
							Visit(kid, author || NamesACall(named.Inner));

						break;

					case Wrapped { Wrap: Wrap.Ahead or Wrap.NotAhead or Wrap.Zero }:
						break;

					default:
						foreach (var kid in n.Kids)
							Visit(kid, author);

						break;
				}
			}

			Visit(node, author: false);

			return found;
		}

		/// <summary>The value of a call that is an operand, or the capture of a name on a call.</summary>
		Val Value(Node call)
		{
			switch (call.Expr)
			{
				case Call c:
					return HelperValue(c.Rule.Name, call.Start, call.End);

				case SelfCall or RuleRef:
					return Eval(call.Rule!, call.Body!);

				default:
					throw new InvalidOperationException(call.Expr.GetType().Name);
			}
		}

		Val HelperValue(string name, int start, int end)
		{
			var text = _input.Substring(start, end - start);

			switch (name)
			{
				case "Word":
					return Val.Str("W:" + text);

				case "Nm" or "Dg":
					return Val.Str(text);

				case "Num":
					return Val.Int(int.Parse(text, CultureInfo.InvariantCulture));

				case "O":
					return new Val("object", "O:" + text, "\"O:" + text + "\"");

				case "Ch":
					return Val.Char('c');

				case "Bx":
					return Val.Host("Box", "box:" + text);

				case "Bxa":
					return Val.Host("Box", "box:A" + text);

				case "Lf":
					return Val.Host("Pr", "pr:" + text);

				case "Lfa":
					return Val.Host("Pr", "pr:A" + text);

				case "Item":
					return Val.Host("Item", "Item{X=\"" + text + "\"}");

				case "Sp":
					return Val.Span(start + 100, end + 100);

				case "Mws":
					return Val.Span(start + 101, end + 99);

				case "Mw":
					return Val.Str("W:" + text.Substring(1, 2));

				case "Mx":
					return text[0] == '[' ? Text(start, end) : Val.Str("W:" + text);

				case "Span" or "Gs" or "Mids" or "Ows":
					return Val.Span(start, end);

				default:
					// Bare, G, C4, Mid, Ow: their own text.
					return Text(start, end);
			}
		}

		/// <summary>A capture's value: the value of the call it names, or the text it matched.</summary>
		Val Capture(Node named)
		{
			var inner = named.Kids[0];

			while (inner.Expr is Group or Wrapped && inner.Kids.Count == 1)
				inner = inner.Kids[0];

			if (inner.Expr is Call or SelfCall or RuleRef)
				return Value(inner);

			if (_mode == Mode.B)
				throw new UndecidedException("a name that captures raw text, over bytes, fills a construction");

			return Val.Str(_input.Substring(inner.Start, inner.End - inner.Start));
		}

		Val Construct(Rule rule, Node node)
		{
			var captured = new Dictionary<string, Val>(StringComparer.Ordinal);
			var parameters = Parameters(rule.Type);

			void Visit(Node n)
			{
				switch (n.Expr)
				{
					case Named named:
						if (parameters.Contains(named.Name))
							captured[named.Name] = Capture(n);

						break;

					case Wrapped { Wrap: Wrap.Ahead or Wrap.NotAhead or Wrap.Zero }:
						return;
				}

				if (n.Expr is not (Call or SelfCall or RuleRef))
					foreach (var kid in n.Kids)
						Visit(kid);
			}

			Visit(node);

			switch (rule.Type)
			{
				case "string":
					return Val.Str(new string(captured["c"].Raw[0], int.Parse(captured["count"].Raw, CultureInfo.InvariantCulture)));

				case "SourceSpan":
				{
					var start  = int.Parse(captured["start"].Raw, CultureInfo.InvariantCulture);
					var length = int.Parse(captured["length"].Raw, CultureInfo.InvariantCulture);

					return Val.Span(start, start + length);
				}

				case "Box":
					return captured.TryGetValue("inner", out var inner)
						? Val.Host("Box", "box:[" + inner.Raw + "]")
						: Val.Host("Box", "box:" + captured["name"].Raw);

				case "Pr":
				{
					var print = "pr:" + (captured.TryGetValue("t", out var t) ? t.Raw : "")
						+ (captured.TryGetValue("l", out var l) ? "[" + l.Raw + "]" : "")
						+ (captured.TryGetValue("r", out var r) ? "[" + r.Raw + "]" : "");

					return Val.Host("Pr", print);
				}

				case "Item":
					return Val.Host("Item", "Item{X=\"" + captured["x"].Raw + "\"}");

				default:
					throw new UndecidedException("a construction of " + rule.Type);
			}
		}
	}

	/// <summary>A guard or a switch reads a name somewhere in the grammar's rules, or a helper that does is called.</summary>
	public bool ReadsNameByGuard()
	{
		return Called().Any(static h => h.Name is "G" or "Gs") || _rules.Any(r => Mentions(r, static e => e is Guard { Reads: not null } or SwitchOn));
	}
}
