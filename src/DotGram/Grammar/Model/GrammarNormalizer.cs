using System;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>
/// Lowers the bound syntax tree into a recognition graph and folds what can be folded.
/// </summary>
/// <remarks>
/// <para>
/// Folding is allowed only where it provably cannot change meaning. Adjacent literals
/// merge because concatenation is what a sequence of them already means; adjacent
/// single-item alternatives merge into one set because a set matches exactly one item,
/// so their order among themselves cannot matter.
/// </para>
/// <para>
/// What is not done is reordering. Roc's macro moved every single-character
/// alternative ahead of the rest, which turns <c>"ab" | 'a'</c> into <c>'a' | "ab"</c>
/// and makes the second unreachable. Here only an adjacent run is merged, so nothing
/// crosses anything else.
/// </para>
/// <para>
/// Lowering walks the located tree and matches on the shape it carries. Both are to
/// hand at once, which is the point of keeping them apart: the deep patterns below
/// read as shapes, while a diagnostic still has a node to point at.
/// </para>
/// </remarks>
public sealed partial class GrammarNormalizer
{
	public const string NullableRepetition  = "GRAM4001";
	public const string LeftRecursion       = "GRAM4002";
	public const string TriviaNotNullable   = "GRAM4003";
	public const string UnsupportedElement  = "GRAM4005";
	public const string UnbuiltCapture      = "GRAM4006";
	public const string CaptureTypeMismatch = "GRAM4007";
	public const string UnbuiltConstruction = "GRAM4008";
	public const string UnbuiltBinding      = "GRAM4009";
	public const string UnbuiltRecovery     = "GRAM4010";
	public const string UnbuiltRuleType     = "GRAM4011";
	public const string ReservedCaptureName = "GRAM4012";
	public const string UnbuiltCall         = "GRAM4013";
	public const string AmbiguousExternal   = "GRAM4015";

	readonly GrammarModel                                      _model;
	readonly Dictionary<RuleSymbol, Node>                      _bodies      = [];
	readonly Dictionary<RuleSymbol, bool>                      _nullable    = [];
	readonly Dictionary<RuleSymbol, IReadOnlyList<ResultMember>> _results   = [];
	readonly Dictionary<RuleSymbol, string>                    _types       = [];
	readonly List<GramDiagnostic>                              _diagnostics = [];
	readonly List<RuleSymbol>                                  _rules       = [];

	readonly ISymbolResolver _resolver;

	GrammarNormalizer(GrammarModel model, ISymbolResolver resolver)
	{
		_model    = model;
		_resolver = resolver;
	}

	/// <param name="resolver">
	/// Asked one thing only: whether one C# type fits into another, which is what §4.1
	/// case 2 needs and nothing here can work out for itself.
	/// </param>
	public static RecognitionGraph Normalize(GrammarModel model, ISymbolResolver? resolver = null)
	{
		if (model is null)
			throw new ArgumentNullException(nameof(model));

		var normalizer = new GrammarNormalizer(model, resolver ?? PermissiveSymbolResolver.Instance);

		normalizer.Collect(model.Root);
		normalizer.LowerAll();

		// All three before RewriteLeftRecursion(), for the reason already there — a
		// clone's self-call must resolve to the clone before left-recursion rewriting
		// runs. Expression-scoped `with` goes first: it mutates a rule's own body in
		// place, and an enclosing `namespace (...)` clone of the same rule must see that
		// mutation already applied. A publication's own `with` goes last, after
		// `namespace (...)`, since it is the more locally written of the two and composes
		// on top of whatever the namespace already did to the rule it publishes.
		normalizer.SpecializeWithSites();
		normalizer.SpecializeNamespaces();
		normalizer.SpecializePublicationWith();

		normalizer.RewriteLeftRecursion();
		normalizer.ComputeNullability();
		normalizer.ComputeTypes();

		// After the types, for the same reason as the two passes below it feeds: a
		// whole-body value-returning external recognizer is §4.1 case 3's pass-through
		// applied to a producer ComputeTypes never sees (its Declaration is null).
		normalizer.ProduceFromExternals();

		// After the types and before the results: it reads what each rule's type is and
		// writes captures the results are then computed from (§4.1 case 2).
		normalizer.CollectSequences();

		// After the types and before the results, the same as the sequence rewrite above:
		// it writes captures that the results are then computed from (§4.1 case 3).
		normalizer.ExtentValues();
		normalizer.PassThrough();

		// Before the results are computed from the captures, so that they are computed
		// from the hoisted shape: a capture repeated is recorded once, around the loop,
		// wherever the join of the turns is the extent of the repetition.
		normalizer.HoistTextCaptures();

		// After the pass-throughs exist and before the results are computed from the
		// captures: a call to a rule that only forwards becomes the choice it forwarded.
		normalizer.CollapseTransparent();

		normalizer.ComputeResults();

		// After the results: what a rebinding's replacement must be compatible with is the
		// result each rule was just worked out to have.
		normalizer.CheckRebindingReplacements();

		// After the results, because what a constructor is matched against is the members
		// they worked out (§7.3).
		normalizer.BuildByConstructor();

		normalizer.Check();

		return new RecognitionGraph(
			normalizer._rules,
			normalizer._bodies,
			normalizer._nullable,
			normalizer._results,
			normalizer._types,
			Imports(model.Root),
			normalizer._publications,
			normalizer._diagnostics)
		{
			Folds      = normalizer._folds,
			Trivia     = normalizer._trivia,
			Recoveries = normalizer._recoveries,
			Climbing   = normalizer._climbing,
			Powers     = normalizer._powers,
			Externals  = normalizer._externals.ToDictionary(pair => pair.Value, pair => pair.Key),
		};
	}

	void Report(string id, string message, Location at) =>
		_diagnostics.Add(new GramDiagnostic(id, message, at.Position, at.Length, GramSeverity.Error));

	// ── Nullability and the checks that need it ──────────────────────────────────

	/// <summary>
	/// Whether a rule can match without consuming anything. Reached by fixpoint, since
	/// rules call one another.
	/// </summary>
	void ComputeNullability()
	{
		SeedBuiltIns(_model.Root);

		foreach (var rule in _rules)
			_nullable[rule] = false;

		for (var changed = true; changed; )
		{
			changed = false;

			foreach (var rule in _rules)
			{
				var nullable = IsNullable(_bodies[rule]);

				if (nullable != _nullable[rule])
				{
					_nullable[rule] = nullable;
					changed         = true;
				}
			}
		}
	}

	/// <summary>
	/// Built-in rules have no body to compute from, so their nullability is stated:
	/// `none`, `eof` and the default `trivia` consume nothing, `any` and `eol` consume.
	/// </summary>
	void SeedBuiltIns(GrammarNamespace ns)
	{
		for (var outer = ns; outer is not null; outer = outer.Parent)
			foreach (var rule in outer.Rules.Values)
				if (rule.IsBuiltIn)
					_nullable[rule] = rule.Name is "none" or "eof" or "trivia" or "wordboundary";
	}

	bool IsNullable(Node node) => node switch
	{
		Node.Empty                           => true,
		Node.Literal  (var text)             => text.Length == 0,
		Node.Element                         => false,
		Node.Guard                           => true,
		Node.Lookahead                       => true,
		Node.Atomic   (var body)             => IsNullable(body),
		Node.Capture  (_, var body)          => IsNullable(body),
		Node.Construct(var body, _)          => IsNullable(body),
		Node.Repeat   (var body, var min, _) => min == 0 || IsNullable(body),
		Node.Sequence (var nodes)            => nodes.All(IsNullable),
		Node.Choice   (var nodes)            => nodes.Any(IsNullable),
		Node.Call     (var rule, _)          => _nullable.TryGetValue(rule, out var nullable) && nullable,
		_                                    => false,
	};

	// ── Results ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// What each rule's value is made of. A rule that captures nothing has no members and
	/// keeps the value it always had — the text it matched.
	/// </summary>
	// ── Left recursion (§4.3) ────────────────────────────────────────────────────

	readonly Dictionary<RuleSymbol, Fold> _folds  = [];
	readonly Dictionary<RuleSymbol, Node> _trivia = [];

	/// <summary>
	/// Turns a left-recursive rule into a base and a loop of tails.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>R = left: R &amp; op &amp; right | base</c> becomes <c>base &amp; (op &amp;
	/// right)*</c>. The leading self-call is what makes an alternative recursive and what
	/// the rewrite removes; <c>left</c> stops being a capture and becomes the value built
	/// so far, which the alternative's own <c>=&gt;</c> receives.
	/// </para>
	/// <para>
	/// The loop is an ordinary repetition, so backtracking, forgetting and everything
	/// else apply to it unchanged. What is different is only that a capture under it is
	/// consumed by the fold on the same iteration that wrote it, so it is a value rather
	/// than a sequence.
	/// </para>
	/// </remarks>
	void RewriteLeftRecursion()
	{
		foreach (var rule in _rules)
		{
			var alternatives = Alternatives(_bodies[rule]);
			var bases        = new List<Node>();
			var tails        = new List<Node>();
			var accumulators = new Dictionary<Node, string>(NodeIdentity.Instance);

			// A rule that states its own strengths is climbed rather than folded: the same
			// loop over the same tails, entered on a comparison instead of unconditionally.
			if (RewriteBindingPowers(rule, alternatives))
				continue;

			var ambiguous = false;

			foreach (var alternative in alternatives)
				if (Tail(alternative, rule) is var (tail, accumulator))
				{
					// Recursive on both sides. The leading call is the accumulator and the
					// trailing one would take everything to the right, so what is written
					// left-associative would parse right-associative — §4.3 refuses it
					// rather than answer differently from how it reads.
					if (EndsWith(tail, rule))
					{
						Report(
							LeftRecursion,
							$"An alternative of '{rule.Name}' is recursive on both sides. Ordered choice cannot " +
							"settle which way it groups: the trailing call would take everything to the right. " +
							"Write the operands at the next level of precedence down (docs/syntax.md §4.3).",
							rule.Declaration!.At);

						ambiguous = true;
					}

					tails.Add(tail);
					accumulators[tail] = accumulator;
				}
				else
				{
					bases.Add(alternative);
				}

			if (tails.Count == 0 || ambiguous)
				continue;

			if (bases.Count == 0)
			{
				Report(
					LeftRecursion,
					$"Every alternative of '{rule.Name}' is left-recursive, so there is nothing to start from.",
					rule.Declaration!.At);

				continue;
			}

			var loop = new Node.Repeat(
				tails.Count == 1 ? tails[0] : new Node.Choice(tails), 0, null);

			_bodies[rule] = new Node.Sequence(
				[bases.Count == 1 ? bases[0] : new Node.Choice(bases), loop]);

			_folds[rule] = new Fold(loop, accumulators);
		}
	}

	/// <summary>
	/// <c>E &lt;&lt; 1 | E &gt;&gt; 3 | …</c> — one rule holding a whole expression
	/// language (§4.3.1). Returns whether this rule was one.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The rewrite is the one §4.3 already does: the alternatives that begin with a call
	/// to the rule itself become the tails of a loop over the ones that do not. What
	/// binding powers add is two numbers on top of that shape — which strength a tail may
	/// be entered at, and which strength its own operand is parsed at — and those are what
	/// turn a fold into precedence climbing.
	/// </para>
	/// <para>
	/// So an alternative recursive on both sides, which §4.3 refuses because ordered
	/// choice cannot settle it, is exactly what this accepts: <c>&lt;&lt; n</c> and
	/// <c>&gt;&gt; n</c> are the settling. <c>&lt;&lt;</c> parses the operand one strength
	/// tighter, so the same operator cannot appear in it and the grouping goes left;
	/// <c>&gt;&gt;</c> parses it at the same strength, so it can, and the grouping goes
	/// right.
	/// </para>
	/// </remarks>
	bool RewriteBindingPowers(RuleSymbol rule, IReadOnlyList<Node> alternatives)
	{
		var bound = 0;

		foreach (var alternative in alternatives)
			if (BoundOn(alternative) is not null)
				bound++;

		if (bound == 0)
			return false;

		var bases        = new List<Node>();
		var tails        = new List<Node>();
		var accumulators = new Dictionary<Node, string>(NodeIdentity.Instance);
		var levels       = new Dictionary<Node, int>(NodeIdentity.Instance);
		var powers       = new Dictionary<Node, int>(NodeIdentity.Instance);
		var refused      = false;

		foreach (var alternative in alternatives)
		{
			var bind     = BoundOn(alternative);
			var known    = bind is not null;
			var strength = bind ?? default;
			var head     = Tail(alternative, rule);

			// An operand of the rule itself, at the end: the right side of an infix or the
			// operand of a prefix. `<<` parses it one tighter, `>>` at the same strength —
			// which is the whole of what the two markers say.
			if (known && SelfCallAt(head is var (tail0, _) ? tail0 : alternative, rule) is { } operand)
				powers[operand] = strength.IsLeft ? strength.Level + 1 : strength.Level;

			if (head is var (tail, accumulator))
			{
				if (!known)
				{
					Report(
						UnbuiltBinding,
						$"An alternative of '{rule.Name}' is recursive but states no strength, and its " +
						"siblings do. A rule uses one convention or the other — levels as rules, or " +
						"'<<' and '>>' on every recursive alternative (docs/syntax.md §4.3.1).",
						rule.Declaration!.At);

					refused = true;
				}

				tails.Add(tail);
				accumulators[tail] = accumulator;
				levels[tail]       = strength.Level;
			}
			else
			{
				// A strength on something with no operand of its own says nothing: there is
				// nothing for it to be parsed at.
				if (known && !powers.ContainsKey(alternative) && SelfCallAt(alternative, rule) is null)
				{
					Report(
						UnbuiltBinding,
						$"An alternative of '{rule.Name}' states a strength and has no operand of its own " +
						"to parse at it. A strength says how tightly the operand to the right is read " +
						"(docs/syntax.md §4.3.1).",
						rule.Declaration!.At);

					refused = true;
				}

				bases.Add(alternative);
			}
		}

		if (refused)
			return true;

		if (bases.Count == 0)
		{
			Report(
				LeftRecursion,
				$"Every alternative of '{rule.Name}' is recursive on the left, so there is nothing to " +
				"start from.",
				rule.Declaration!.At);

			return true;
		}

		var start = bases.Count == 1 ? bases[0] : new Node.Choice(bases);

		// A rule of nothing but prefixes and atoms — all strength, no infix — climbs
		// without looping, and an empty repetition would only be a nullable one to refuse.
		if (tails.Count > 0)
		{
			var loop = new Node.Repeat(
				tails.Count == 1 ? tails[0] : new Node.Choice(tails), 0, null);

			_bodies[rule] = new Node.Sequence([start, loop]);
			_folds[rule]  = new Fold(loop, accumulators);
		}
		else
		{
			_bodies[rule] = start;
		}

		_climbing[rule] = levels;

		foreach (var power in powers)
			_powers[power.Key] = power.Value;

		return true;
	}

	/// <summary>
	/// What <c>&lt;&lt;</c> or <c>&gt;&gt;</c> said on this alternative, or null.
	/// </summary>
	/// <remarks>
	/// The marker binds to the pattern and the <c>=&gt;</c> wraps that, so the alternative
	/// the body holds is the construct and the strength was recorded against what is
	/// inside it.
	/// </remarks>
	(bool IsLeft, int Level)? BoundOn(Node alternative) =>
		_bounds.TryGetValue(alternative is Node.Construct(var built, _) ? built : alternative, out var found)
			? found
			: null;

	/// <summary>
	/// The call to <paramref name="rule"/> that an alternative ends with, or null.
	/// </summary>
	/// <remarks>
	/// The node itself and not merely whether there is one: it is what the strength is
	/// recorded against, and what the machine reads when it emits the call.
	/// </remarks>
	static Node.Call? SelfCallAt(Node node, RuleSymbol rule)
	{
		while (true)
			switch (node)
			{
				case Node.Construct(var built,  _)           : node = built; break;
				case Node.Capture  (_,          var captured): node = captured; break;
				case Node.Sequence (var operands)            : node = operands[^1]; break;
				case Node.Call     (var called, _) call      : return ReferenceEquals(called, rule) ? call : null;
				default                                      : return null;
			}
	}

	/// <summary>
	/// An alternative with its leading call to <paramref name="rule"/> taken off, and the
	/// name that call was captured under — or null when it does not begin with one.
	/// </summary>
	static (Node Tail, string Accumulator)? Tail(Node alternative, RuleSymbol rule)
	{
		var built = alternative as Node.Construct;
		var body  = built?.Body ?? alternative;

		if (body is not Node.Sequence(var operands) || operands.Count < 2)
			return null;

		var head = operands[0];
		var name = head is Node.Capture(var captured, var inner) ? captured : null;

		if ((head is Node.Capture(_, var call) ? call : head) is not Node.Call(var called, _) ||
			!ReferenceEquals(called, rule))
			return null;

		var rest = new List<Node>(operands.Count - 1);

		for (var i = 1; i < operands.Count; i++)
			rest.Add(operands[i]);

		var tail = rest.Count == 1 ? rest[0] : new Node.Sequence(rest);

		return (built is null ? tail : built with { Body = tail }, name ?? "");
	}

	/// <summary>Whether the last thing an alternative matches is a call to this rule.</summary>
	static bool EndsWith(Node node, RuleSymbol rule)
	{
		while (true)
			switch (node)
			{
				case Node.Construct(var built, _)   : node = built; break;
				case Node.Capture  (_, var captured): node = captured; break;
				case Node.Sequence (var operands)   : node = operands[^1]; break;
				case Node.Call     (var called, _)  : return ReferenceEquals(called, rule);
				default:                              return false;
			}
	}

	/// <summary>Every <c>@using</c> in the grammar, outermost namespace first.</summary>
	static IReadOnlyList<string> Imports(GrammarNamespace ns)
	{
		var imports = new List<string>(ns.CSharpImports);

		foreach (var nested in ns.Nested)
			foreach (var import in Imports(nested))
				if (!imports.Contains(import))
					imports.Add(import);

		return imports;
	}

	internal static bool Writes(Node node, string name) => node switch
	{
		Node.Capture  (var captured, var body)  => captured == name || Writes(body, name),
		Node.Atomic   (var body)                => Writes(body, name),
		Node.Sequence (var nodes)               => nodes.Any(child => Writes(child, name)),
		Node.Choice   (var nodes)               => nodes.All(child => Writes(child, name)),
		Node.Construct(var built, _)            => Writes(built, name),

		// A run that may be empty still writes: the text of no iterations is "". Only a
		// genuine option — `X?`, which either happened or did not — leaves it unwritten.
		Node.Repeat(var body, var min, var max) => (min > 0 || max != 1) && Writes(body, name),

		_                                       => false,
	};
}
