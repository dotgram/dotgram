using System;
using System.Collections.Generic;
using System.Linq;

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
		normalizer.RewriteLeftRecursion();
		normalizer.ComputeNullability();
		normalizer.ComputeTypes();

		// After the types and before the results: it reads what each rule's type is and
		// writes captures the results are then computed from (§4.1 case 2).
		normalizer.CollectSequences();

		normalizer.ComputeResults();

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
			model.Publications,
			normalizer._diagnostics)
		{
			Folds      = normalizer._folds,
			Trivia     = normalizer._trivia,
			Recoveries = normalizer._recoveries,
			Climbing   = normalizer._climbing,
			Powers     = normalizer._powers,
			Fallible     = normalizer._fallible,
			Constructions = normalizer._constructions,
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
	/// `none`, `eof` and the default `Trivia` consume nothing, `any` and `eol` consume.
	/// </summary>
	void SeedBuiltIns(GrammarScope scope)
	{
		for (var outer = scope; outer is not null; outer = outer.Parent)
			foreach (var rule in outer.Rules.Values)
				if (rule.IsBuiltIn)
					_nullable[rule] = rule.Name is "none" or "eof" or "Trivia";
	}

	bool IsNullable(Node node) => node switch
	{
		Node.Empty                     => true,
		Node.Literal(var text)         => text.Length == 0,
		Node.Element                   => false,
		Node.Guard                     => true,
		Node.Lookahead                 => true,
		Node.Capture(_, var body)      => IsNullable(body),
		Node.Construct(var body, _)    => IsNullable(body),
		Node.Repeat(var body, var min, _) => min == 0 || IsNullable(body),
		Node.Sequence(var nodes)       => nodes.All(IsNullable),
		Node.Choice(var nodes)         => nodes.Any(IsNullable),
		Node.Call(var rule, _)         => _nullable.TryGetValue(rule, out var nullable) && nullable,
		_                              => false,
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
				case Node.Construct(var built, _):    node = built; break;
				case Node.Capture(_, var captured):   node = captured; break;
				case Node.Sequence(var operands):     node = operands[operands.Count - 1]; break;
				case Node.Call(var called, _):        return ReferenceEquals(called, rule) ? (Node.Call)node : null;
				default:                              return null;
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

		Node tail = rest.Count == 1 ? rest[0] : new Node.Sequence(rest);

		return (built is null ? tail : built with { Body = tail }, name ?? "");
	}

	/// <summary>Whether the last thing an alternative matches is a call to this rule.</summary>
	static bool EndsWith(Node node, RuleSymbol rule)
	{
		while (true)
			switch (node)
			{
				case Node.Construct(var built, _):    node = built; break;
				case Node.Capture(_, var captured):   node = captured; break;
				case Node.Sequence(var operands):     node = operands[operands.Count - 1]; break;
				case Node.Call(var called, _):        return ReferenceEquals(called, rule);
				default:                              return false;
			}
	}

	/// <summary>Every <c>@using</c> in the grammar, outermost scope first.</summary>
	static IReadOnlyList<string> Imports(GrammarScope scope)
	{
		var imports = new List<string>(scope.CSharpImports);

		foreach (var nested in scope.Nested)
			foreach (var import in Imports(nested))
				if (!imports.Contains(import))
					imports.Add(import);

		return imports;
	}

	/// <summary>
	/// The C# type a rule declared for itself, if it declared one.
	/// </summary>
	/// <remarks>
	/// Only a C# type: <c>: @T</c> and the keywords that are always C# (§2). A type that
	/// names a rule is §4.1 case 3 and is not built, so it is left for the rule's own
	/// value to be worked out from its captures.
	/// </remarks>
	void ComputeTypes()
	{
		foreach (var rule in _rules)
		{
			if (rule.Declaration?.Type is not { } type)
				continue;

			if (type.IsCSharp || IsCSharpKeyword(type.Name))
			{
				// §4.1 case 4: `: @string` over a rule that builds nothing and captures
				// nothing says out loud what such a rule says by default — the extent it
				// matched. Recorded as no declared type at all, because a declared one is
				// what tells the emitter to expect a value the machine never builds.
				if (string.Equals(TypeName(type), "string", StringComparison.Ordinal) &&
					_bodies.TryGetValue(rule, out var said) &&
					!HasCapture(said) &&
					Constructs(said).FirstOrDefault() is null)
				{
					continue;
				}

				_types[rule] = TypeName(type);

				continue;
			}

			// §4.1 case 3: `A : B` says A's value is B's. Nothing here knows how to make
			// that true, and what happened before this was worse than not knowing — the
			// declaration was dropped and A got a type generated from its own captures, so
			// a rule said one thing and meant another with nothing to read about it.
			Report(
				UnbuiltRuleType,
				$"'{rule.Name}' declares its type as the rule '{type.Name}', which is docs/syntax.md " +
				"§4.1 case 3 and is not built. Declare a C# type with ': @T' and build it with '=>'.",
				type.At);
		}
	}

	/// <summary>
	/// The text a <c>=&gt;</c> carries when the grammar wrote none and the result is a
	/// sequence — the emitter builds the body rather than compiling an expression.
	/// </summary>
	public const string SequenceMarker = "<sequence>";

	/// <summary>
	/// And the one it carries when the value is built by calling the declared type's
	/// constructor (§7.3). <see cref="RecognitionGraph.Constructions"/> says with what.
	/// </summary>
	public const string ConstructorMarker = "<constructor>";

	readonly Dictionary<RuleSymbol, IReadOnlyList<string>> _constructions = [];

	/// <summary>
	/// §7.3's first way of filling a result in: a constructor whose every parameter is
	/// covered by a capture.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Rewritten into what already works, the same as §4.1 case 2 above: the alternative
	/// gets a <c>=&gt;</c> carrying <see cref="ConstructorMarker"/>, and the order the
	/// captures go in is recorded beside it. Everything about numbering captures, giving
	/// them up on a failed alternative and rebuilding them at the accepting state is then
	/// the code that already does it.
	/// </para>
	/// <para>
	/// Names are matched without regard to case, which is the mechanical transform §7.3
	/// describes: the capture <c>symbol</c> fits the parameter <c>symbol</c> and the
	/// property <c>Symbol</c>. Types are not checked here — whether the capture's value
	/// goes in that parameter is C#'s question, and it will be asked on the grammar's own
	/// line now that §7.6 puts it there.
	/// </para>
	/// <para>
	/// The longest constructor every parameter of which is covered. Two of the same length
	/// both covered is an ambiguity the grammar cannot resolve, so nothing is chosen and
	/// the rule is left to be reported as unbuilt — a wrong constructor called silently is
	/// the failure worth avoiding.
	/// </para>
	/// </remarks>
	void BuildByConstructor()
	{
		foreach (var rule in _rules)
		{
			if (!_types.TryGetValue(rule, out var type) ||
				rule.Declaration?.Type is { IsSequence: true } ||
				!_results.TryGetValue(rule, out var members) ||
				members.Count == 0)
			{
				continue;
			}

			var alternatives = Alternatives(_bodies[rule]);

			// A rule that says how to build its value has said it.
			if (alternatives.Any(static alternative => alternative is Node.Construct))
				continue;

			if (!_resolver.TryResolveConstructors(type, out var constructors))
				continue;

			var chosen = (IReadOnlyList<string>?)null;
			var length = -1;
			var tied   = false;

			foreach (var constructor in constructors)
			{
				if (Covered(constructor, members) is not { } order)
					continue;

				if (order.Count > length)
				{
					(chosen, length, tied) = (order, order.Count, false);
				}
				else if (order.Count == length)
				{
					tied = true;
				}
			}

			if (chosen is null || tied)
				continue;

			var rewritten = new List<Node>(alternatives.Count);

			foreach (var alternative in alternatives)
				rewritten.Add(new Node.Construct(alternative, ConstructorMarker));

			_bodies[rule] = rewritten.Count == 1 ? rewritten[0] : new Node.Choice(rewritten);
			_constructions[rule] = chosen;
		}
	}

	/// <summary>
	/// The captures that fill this constructor, in its own order, or null where one of its
	/// parameters has nothing to fill it.
	/// </summary>
	static IReadOnlyList<string>? Covered(
		IReadOnlyList<MethodParameter> constructor, IReadOnlyList<ResultMember> members)
	{
		if (constructor.Count == 0)
			return null;

		var order = new List<string>(constructor.Count);

		foreach (var parameter in constructor)
		{
			var found = (string?)null;

			foreach (var member in members)
				if (string.Equals(member.Name, parameter.Name, StringComparison.OrdinalIgnoreCase))
					found = member.Name;

			if (found is null)
				return null;

			order.Add(found);
		}

		return order;
	}

	/// <summary>
	/// §4.1 case 2: a rule whose type is <c>T[]</c> collects the operands that fit.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Rewritten into what already works rather than given machinery of its own. Each
	/// operand whose value is assignable to <c>T</c> becomes an ordinary capture, and the
	/// alternative gets a <c>=&gt;</c> whose text is <see cref="SequenceMarker"/> — so the
	/// captures are numbered, given up and rebuilt by exactly the code that does it for a
	/// capture the author wrote, and the only new thing is what the factory's body says.
	/// </para>
	/// <para>
	/// Only the operands of the alternative itself. Reaching inside a group would collect
	/// what a nested rule already collected — the sequence is what this rule is made of,
	/// and what its parts are made of is their own business (§4.1).
	/// </para>
	/// </remarks>
	void CollectSequences()
	{
		foreach (var rule in _rules)
		{
			if (rule.Declaration?.Type is not { IsSequence: true } declared)
				continue;

			// A rule that says how to build its value has said it; this is for the one that
			// left it to the shape of the rule.
			if (Alternatives(_bodies[rule]).Any(static alternative => alternative is Node.Construct))
				continue;

			var rewritten = new List<Node>();
			var taken     = 0;

			foreach (var alternative in Alternatives(_bodies[rule]))
			{
				var parts = alternative is Node.Sequence(var sequence) ? sequence : [alternative];
				var built = new List<Node>(parts.Count);

				foreach (var part in parts)
					built.Add(Fits(part, declared.Name) ? Collected(part, ref taken) : part);

				rewritten.Add(new Node.Construct(
					built.Count == 1 ? built[0] : new Node.Sequence(built),
					SequenceMarker));
			}

			if (taken == 0)
			{
				Report(
					UnbuiltConstruction,
					$"'{rule.Name}' declares its result as a sequence of '{declared.Name}', and no " +
					"operand of it produces one — every part either builds no value or builds a type " +
					$"that is not a '{declared.Name}'. §4.1 case 2 says which operands join a sequence.",
					declared.At);

				continue;
			}

			_bodies[rule] = rewritten.Count == 1 ? rewritten[0] : new Node.Choice(rewritten);
		}
	}

	/// <summary>
	/// An operand wrapped in the capture that puts it in the sequence.
	/// </summary>
	/// <remarks>
	/// Inside a repetition rather than around it, which is where a capture the author
	/// wrote ends up: <c>rows: Row*</c> parses as <c>(rows: Row)*</c>, because a capture
	/// binds tighter than a quantifier (§10). Written the other way round the slot holds
	/// the text of the whole run instead of collecting the values.
	/// </remarks>
	Node Collected(Node part, ref int taken)
	{
		if (part is not Node.Repeat(var body, var min, var max))
			return new Node.Capture("item" + taken++, part);

		var repetition = new Node.Repeat(new Node.Capture("item" + taken++, body), min, max);

		// The node is new, and `recover` was recorded against the old one. Everything
		// downstream looks recovery up by node identity, so a repetition rewritten here
		// would quietly stop recovering.
		if (_recoveries.TryGetValue(part, out var recovery))
		{
			_recoveries.Remove(part);
			_recoveries[repetition] = recovery;
		}

		return repetition;
	}

	/// <summary>
	/// Whether this operand's value belongs in a sequence of <paramref name="element"/>.
	/// </summary>
	/// <remarks>
	/// A call to a rule that declared a C# type, or a repetition of one. A rule with no
	/// declared type builds a type generated from its captures, which is nobody's
	/// ancestor and so fits nothing but a sequence of <c>@object</c> — it is left out
	/// rather than guessed at.
	/// </remarks>
	bool Fits(Node part, string element)
	{
		var called = part switch
		{
			Node.Call(var rule, _)                     => rule,
			Node.Repeat(Node.Call(var rule, _), _, _)  => rule,
			_                                          => null,
		};

		return called is not null &&
			_types.TryGetValue(called, out var type) &&
			_resolver.IsAssignable(type, element);
	}

	static bool IsCSharpKeyword(string name) => name is
		"bool" or "byte" or "sbyte" or "char" or "decimal" or "double" or "float" or
		"int" or "uint" or "long" or "ulong" or "short" or "ushort" or "string" or "object";

	void ComputeResults()
	{
		foreach (var rule in _rules)
		{
			var body    = _bodies[rule];
			var layout  = CaptureLayout.Of(body, BuildsValue, _folds.TryGetValue(rule, out var fold) ? fold.Loop : null);
			var members = new List<ResultMember>();
			var slots   = new Dictionary<string, List<CaptureSlot>>(StringComparer.Ordinal);

			foreach (var slot in layout.Slots)
			{
				if (slots.TryGetValue(slot.Name, out var sharing))
				{
					// The same name in two alternatives is one member — but only if the two
					// agree on what it holds, since a member has one type.
					if (sharing[0].Rule != slot.Rule || sharing[0].IsSequence != slot.IsSequence)
						Report(
							CaptureTypeMismatch,
							$"'{slot.Name}' is captured twice in '{rule.Name}' with different types: " +
							$"{Held(sharing[0])} and {Held(slot)}.",
							rule.Declaration!.At);

					sharing.Add(slot);

					continue;
				}

				slots[slot.Name] = [slot];

				members.Add(new ResultMember(
					slot.Name,
					slot.Rule,
					slot.IsSequence,

					// A sequence is never absent: no iterations is an empty one, the same way
					// a run of no text is "".
					IsOptional: !slot.IsSequence && !Writes(body, slot.Name),
					[]));
			}

			for (var i = 0; i < members.Count; i++)
				members[i] = members[i] with
				{
					Slots = slots[members[i].Name].Select(slot => slot.Index).ToList(),
				};

			_results[rule] = members;
		}
	}

	static string Held(CaptureSlot slot) =>
		slot.Rule is null
			? "text"
			: slot.IsSequence
				? $"a sequence of '{slot.Rule.Name}'"
				: $"the value of '{slot.Rule.Name}'";

	/// <summary>Whether a rule has a value of its own — which is to say, any capture.</summary>
	/// <summary>
	/// Whether a rule has a value of its own, rather than the text it matched.
	/// </summary>
	/// <remarks>
	/// Captures give a rule a generated type; a declared type with a <c>=&gt;</c> gives it
	/// the author's own. The second was missing, so <c>Header : @Item = 'H' &amp; eol =&gt;
	/// @(new Head())</c> — a rule that plainly has a value and no captures — was treated as
	/// text, and a capture of it held the characters instead of the <c>Item</c>.
	/// </remarks>
	bool BuildsValue(RuleSymbol rule) =>
		_types.ContainsKey(rule) || (_bodies.TryGetValue(rule, out var body) && HasCapture(body));

	static bool HasCapture(Node node) => node switch
	{
		Node.Capture                       => true,
		Node.Sequence(var nodes)           => nodes.Any(HasCapture),
		Node.Choice(var nodes)             => nodes.Any(HasCapture),
		Node.Repeat(var body, _, _)        => HasCapture(body),
		Node.Construct(var built, _)       => HasCapture(built),

		// Not across a call — that is another rule's result — and not into a lookahead,
		// which consumes nothing and is compiled with its captures stripped.
		_                                  => false,
	};

	/// <summary>
	/// Whether every way through this node writes <paramref name="name"/>. What decides
	/// whether the member can be null, and so whether the generated property is nullable.
	/// </summary>
	internal static bool Writes(Node node, string name) => node switch
	{
		Node.Capture(var captured, var body) => captured == name || Writes(body, name),
		Node.Sequence(var nodes)             => nodes.Any(child => Writes(child, name)),
		Node.Choice(var nodes)               => nodes.All(child => Writes(child, name)),
		Node.Construct(var built, _)         => Writes(built, name),

		// A run that may be empty still writes: the text of no iterations is "". Only a
		// genuine option — `X?`, which either happened or did not — leaves it unwritten.
		Node.Repeat(var body, var min, var max) => (min > 0 || max != 1) && Writes(body, name),

		_                                    => false,
	};
}
