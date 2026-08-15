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
public sealed class GrammarNormalizer
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
			Fallible   = normalizer._fallible,
		};
	}

	void Report(string id, string message, Location at) =>
		_diagnostics.Add(new GramDiagnostic(id, message, at.Position, at.Length, GramSeverity.Error));

	// ── Lowering ─────────────────────────────────────────────────────────────────

	/// <summary>
	/// Every declared rule, collected before any of them is lowered.
	/// </summary>
	/// <remarks>
	/// Two passes rather than one, so that lowering may ask for another rule's body and
	/// get it whatever the declaration order was — which a reference inside an element
	/// set needs, since it has to be merged into the set that names it.
	/// </remarks>
	void Collect(GrammarScope scope)
	{
		foreach (var rule in scope.Rules.Values)
			if (rule.Declaration is not null)
				_rules.Add(rule);

		foreach (var nested in scope.Nested)
			Collect(nested);
	}

	void LowerAll()
	{
		// Indexed, because lowering registers built-ins and appends them.
		for (var i = 0; i < _rules.Count; i++)
			BodyOf(_rules[i]);

		foreach (var rule in _rules)
			if (rule.Declaration is not null && TriviaFor(rule.Scope) is { } trivia)
				_trivia[rule] = trivia;
	}

	/// <summary>A rule's lowered body, lowering it now if that has not happened yet.</summary>
	Node BodyOf(RuleSymbol rule)
	{
		if (_bodies.TryGetValue(rule, out var body))
			return body;

		if (rule.Declaration is null)
			return Node.Empty.Instance;

		// Placed before lowering: a rule whose body reaches itself would otherwise
		// recurse for ever here rather than being reported by the left-recursion check.
		_bodies[rule] = Node.Empty.Instance;

		return _bodies[rule] = Lower(rule.Declaration.Body, rule.Scope);
	}

	Node Lower(Expr expression, GrammarScope scope) => expression switch
	{
		Expr.Literal(_, var value)              => new Node.Literal(value),
		Expr.ElementSet(var negated, var items) => LowerElementSet(negated, items, expression),
		Expr.Group(var body)                    => Lower(body, scope),
		Expr.Capture(var name, var operand)     => new Node.Capture(name, Lower(operand, scope)),
		Expr.Lookahead(var positive, var operand) => new Node.Lookahead(positive, Lower(operand, scope)),
		Expr.Guard(var value)                   => new Node.Guard(Text(value)),
		Expr.CSharp(var text)                   => new Node.Guard($"@({text})"),

		Expr.Construct(var pattern, var value)  => LowerConstruct(pattern, value, scope),

		Expr.Bound(var body, var isLeft, var level) => LowerBound(body, isLeft, level, scope),

		// Parsed and refused rather than parsed and ignored: a `recover` that means
		// nothing would swallow a bad record in silence.
		Expr.Recovering(var body, var sync, var factory) =>
			LowerRecovery(body, sync, factory, scope, expression),

		Expr.Quantified(var operand, var kind, var min, _, var max, _) =>
			new Node.Repeat(Lower(operand, scope), Bounds(kind, min).Min, Bounds(kind, max).Max),

		Expr.Sequence(var operands)             => LowerSequence(operands, scope),
		Expr.Choice(var alternatives)           => LowerChoice(alternatives, scope),

		Expr.Call(var target, var arguments) => CallTo(
			RuleOf(expression, target.Name),
			[.. arguments.Select(argument => Lower(argument, scope))]),

		Expr.Reference(_, var name, _) => LowerReference(expression, name),

		_ => Node.Empty.Instance,
	};

	/// <summary>The constructions whose C# may refuse the value it was given (§8.1).</summary>
	readonly HashSet<Node> _fallible = new(NodeIdentity.Instance);

	/// <summary>
	/// <c>=&gt; expr</c>, and whether that expression is allowed to say no.
	/// </summary>
	/// <remarks>
	/// §8.1 needs no notation because the shape of the C# says it: a transformation
	/// written <c>bool M(args…, out T value)</c> is one that may refuse, which is the shape
	/// <c>int.TryParse</c> already has. Recognised here rather than at emission because
	/// only the binder knows what the name resolved to, and only an <c>@Name(args)</c> can
	/// be asked — an inline <c>@(...)</c> is text this half does not read.
	/// </remarks>
	Node LowerConstruct(Expr pattern, Expr value, GrammarScope scope)
	{
		// The binder hangs a call's symbol on the call, not on the name inside it.
		var fallible = value is Expr.Call(var target, _) &&
			target.IsCSharp &&
			_model.Bindings.TryGetValue(value, out var symbol) &&
			symbol is CSharpSymbol { Role: MethodRole.FallibleTransformation };

		// The `out` argument is written in here, where the call is still a shape rather
		// than a string, so that emission has nothing to take apart.
		var text = fallible && value is Expr.Call(var called, var arguments)
			? Text(called) + "(" + string.Join(", ", arguments.Select(Text).Append("out value")) + ")"
			: Text(value);

		var construct = new Node.Construct(Lower(pattern, scope), text);

		if (fallible)
			_fallible.Add(construct);

		return construct;
	}

	/// <summary>
	/// A bare name standing where an operand goes: a rule to call, or something else.
	/// </summary>
	/// <remarks>
	/// A C# name here is §7.1 — a method that consumes input, or a predicate over one item
	/// — and the seam for calling one at run time does not exist. It used to lower to an
	/// element set with nothing in it, which is a rule that compiles, runs, and matches
	/// nothing whatever the input is.
	/// </remarks>
	Node LowerReference(Expr expression, string name)
	{
		if (!_model.Bindings.TryGetValue(expression, out var symbol))
			return new Node.Element(false, [], [], [Unresolved(name)]);

		// §4.2: inside a specialization a parameter stands for whatever the call passed.
		// It used to lower to an element set with nothing in it — a rule that compiled,
		// ran, and matched nothing whatever the input was.
		if (symbol is ParameterSymbol && _arguments.TryGetValue(name, out var argument))
			return argument;

		if (symbol is RuleSymbol rule)
			return CallTo(rule, []);

		// §7.1's second row: the method reads the input itself. Nothing is checked about
		// what it does with the position it is handed — the `ref` is it saying that it
		// moves one, and a grammar that reaches into the parse takes the parse's
		// invariants on.
		if (symbol is CSharpSymbol { Role: MethodRole.ExternalRecognizer } reader)
			return new Node.External(reader.Name);

		// §7.1's first row: `bool M(char c)` tests one input item, which is exactly what an
		// element set does, so it lowers to one — a set of no ranges and one predicate.
		if (symbol is CSharpSymbol { Role: not MethodRole.ElementPredicate } other)
			Report(
				UnsupportedElement,
				$"'@{name}' stands where an operand goes. A C# method may be one — docs/syntax.md " +
				$"§7.1 — as 'bool {name}(char c)', which tests one input item, or as " +
				$"'bool {name}(ReadOnlySpan<char> input, ref int pos)', which reads the input " +
				"itself. " +
				(other.Role is null
					? "This name is not a method in view."
					: $"This one is a {Described(other.Role.Value)}."),
				expression.At);

		return new Node.Element(false, [], [], [symbol]);
	}

	/// <summary>What a method's shape makes it, in words a message can use.</summary>
	static string Described(MethodRole role) => role switch
	{
		MethodRole.ExternalRecognizer     => "recognizer over a span",
		MethodRole.ValueTransformation    => "transformation",
		MethodRole.FallibleTransformation => "transformation that may refuse",
		_                                 => "guard",
	};

	/// <summary>What <c>&lt;&lt; n</c> or <c>&gt;&gt; n</c> said, by the alternative it was said on.</summary>
	readonly Dictionary<Node, (bool IsLeft, int Level)> _bounds = new(NodeIdentity.Instance);

	readonly Dictionary<RuleSymbol, IReadOnlyDictionary<Node, int>> _climbing = [];
	readonly Dictionary<Node, int>                                  _powers   = new(NodeIdentity.Instance);

	/// <summary>
	/// <c>… &lt;&lt; 2</c> — the alternative, with what it said about its own strength
	/// recorded beside it (§4.3.1).
	/// </summary>
	/// <remarks>
	/// Beside rather than inside, like <c>recover</c>: the alternative is an ordinary one
	/// and everything that reads a body — nullability, captures, results — must go on
	/// reading it without knowing about this.
	/// </remarks>
	Node LowerBound(Expr body, bool isLeft, int level, GrammarScope scope)
	{
		var alternative = Lower(body, scope);

		_bounds[alternative] = (isLeft, level);

		return alternative;
	}

	readonly Dictionary<Node, Recovery> _recoveries = new(NodeIdentity.Instance);

	/// <summary>
	/// <c>R* recover eol</c> — the repetition, with what to do about a broken element
	/// recorded beside it (§8.2).
	/// </summary>
	/// <remarks>
	/// The repetition itself is left an ordinary one, so backtracking, forgetting and
	/// collecting apply to it unchanged. What recovery adds is one more way out of the
	/// loop, taken where the ordinary one would have ended it.
	/// </remarks>
	Node LowerRecovery(Expr body, Expr sync, Expr? factory, GrammarScope scope, Expr at)
	{
		var repetition = Lower(body, scope);

		if (repetition is not Node.Repeat)
		{
			Report(
				UnbuiltRecovery,
				"'recover' belongs on a repetition: it says what to do about one bad element " +
				"among many (docs/syntax.md §8.2).",
				at.At);

			return repetition;
		}

		// Without a `=>` the broken element is dropped and reported out of band (§8.3) —
		// to a `partial void` the generated class declares and the consumer may implement.
		_recoveries[repetition] = new Recovery(
			Lower(sync, scope),
			factory is null ? null : Text(factory));

		return repetition;
	}

	/// <summary>Something the notation says and the compiler cannot do yet.</summary>
	Node Unbuilt(Expr body, GrammarScope scope, Expr at, string id, string message)
	{
		Report(id, message, at.At);

		return Lower(body, scope);
	}

	/// <summary>
	/// A call — and, the first time a built-in is called, the body it is a call to.
	/// </summary>
	/// <remarks>
	/// §3.1 says <c>any</c>, <c>none</c>, <c>eol</c>, <c>eof</c> and <c>Trivia</c> are
	/// ordinary standard-library rules rather than keywords. This is where that stops
	/// being a claim: they are lowered into the same nodes a grammar could have written
	/// itself, so every stage downstream — nullability, the checks, emission — treats
	/// them as what they are and needs to know nothing about them.
	/// <para>
	/// Registered on demand, so a grammar that never says <c>eol</c> carries no
	/// <c>eol</c>.
	/// </para>
	/// </remarks>
	Node CallTo(RuleSymbol rule, IReadOnlyList<Node> arguments)
	{
		if (rule.IsBuiltIn && !_bodies.ContainsKey(rule))
		{
			_rules.Add(rule);
			_bodies[rule] = BuiltInBody(rule.Name);
		}

		// §4.2: a rule with parameters is a rule per set of arguments, made here. Nothing
		// downstream ever meets a parameter — the machine, the layout, the retention
		// analysis all see the ordinary rule that a call turned into.
		if (rule.Declaration is { Params.Count: > 0 })
			return Specialize(rule, arguments);

		return new Node.Call(rule, arguments);
	}

	/// <summary>Every specialization made so far, by the rule and what it was given.</summary>
	readonly Dictionary<string, RuleSymbol> _specialized = new(StringComparer.Ordinal);

	/// <summary>
	/// One rule per set of arguments (§4.2).
	/// </summary>
	/// <remarks>
	/// <para>
	/// Substitution rather than dispatch: <c>List(Word, Comma)</c> becomes a rule whose
	/// body is <c>List</c>'s with <c>item</c> and <c>sep</c> replaced by what was passed.
	/// A parameter is therefore a compile-time thing entirely, which is what lets it be a
	/// recognizer at all — nothing here can call one at run time, and passing a rule as a
	/// value would need a delegate the emitted code deliberately does not have.
	/// </para>
	/// <para>
	/// Two calls with the same arguments share a specialization, keyed by what the
	/// arguments lower to. So <c>List(Word, Comma)</c> written twice is one rule and one
	/// recognizer, and the machine's own recursion check sees a cycle where there is one.
	/// </para>
	/// </remarks>
	Node Specialize(RuleSymbol rule, IReadOnlyList<Node> arguments)
	{
		var declaration = rule.Declaration!;

		if (declaration.Params.Count != arguments.Count)
		{
			Report(
				UnbuiltCall,
				$"'{rule.Name}' takes {declaration.Params.Count} " +
				$"{(declaration.Params.Count == 1 ? "parameter" : "parameters")} and is given " +
				$"{arguments.Count}.",
				declaration.At);

			return Node.Empty.Instance;
		}

		var key = rule.Name + "(" + string.Join(", ", arguments) + ")";

		if (_specialized.TryGetValue(key, out var made))
			return new Node.Call(made, []);

		var specialized = new RuleSymbol(NameFor(rule, arguments), rule.Scope, declaration);

		_specialized[key] = specialized;
		_rules.Add(specialized);

		// Before lowering, for the same reason an ordinary rule is: a specialization that
		// reaches itself would otherwise recurse here rather than be reported.
		_bodies[specialized] = Node.Empty.Instance;

		var outer = _arguments;

		_arguments = new Dictionary<string, Node>(StringComparer.Ordinal);

		for (var i = 0; i < arguments.Count; i++)
			_arguments[declaration.Params[i].Name] = arguments[i];

		_bodies[specialized] = Lower(declaration.Body, rule.Scope);
		_arguments           = outer;

		// Only a C# type. `: item` — the result being whatever the argument produces — is
		// §4.1 case 3 said of a parameter; it is refused where the rule is declared, and
		// copying it here would say so a second time about a rule the author never wrote.
		if (declaration.Type is { } declared && (declared.IsCSharp || IsCSharpKeyword(declared.Name)))
			_types[specialized] = TypeName(declared);

		return new Node.Call(specialized, []);
	}

	/// <summary>What each parameter stands for while a specialization is being lowered.</summary>
	Dictionary<string, Node> _arguments = new(StringComparer.Ordinal);

	/// <summary>
	/// What a specialization is called: the rule, and what it was given.
	/// </summary>
	/// <remarks>
	/// <c>List(Word, Comma)</c> is <c>List_Word_Comma</c>, so a diagnostic and a generated
	/// method both name something the author can find in the grammar. An argument that is
	/// not a call has no name of its own and is numbered instead — the alternative is a
	/// method named after a character class.
	/// </remarks>
	string NameFor(RuleSymbol rule, IReadOnlyList<Node> arguments)
	{
		var name = rule.Name;

		foreach (var argument in arguments)
			name += "_" + (argument is Node.Call(var called, _)
				? called.Name.Replace(".", "_")
				: _specialized.Count.ToString(System.Globalization.CultureInfo.InvariantCulture));

		var taken = name;

		for (var i = 2; Named(taken); i++)
			taken = name + "_" + i.ToString(System.Globalization.CultureInfo.InvariantCulture);

		return taken;
	}

	bool Named(string name)
	{
		foreach (var rule in _rules)
			if (string.Equals(rule.Name, name, StringComparison.Ordinal))
				return true;

		return false;
	}

	static Node BuiltInBody(string name) => name switch
	{
		"any" => AnyItem,
		"eol" => new Node.Choice([new Node.Literal("\r\n"), new Node.Literal("\n"), new Node.Literal("\r")]),
		"eof" => new Node.Lookahead(IsPositive: false, AnyItem),

		// `none`, and `Trivia` until a grammar shadows it with one of its own (§4.5).
		_     => Node.Empty.Instance,
	};

	/// <summary>The complement of nothing: one item, whatever it is.</summary>
	static Node.Element AnyItem => new(IsNegated: true, [], [], []);

	static (int Min, int? Max) Bounds(QuantifierKind kind, int? count) => kind switch
	{
		QuantifierKind.Optional   => (0, 1),
		QuantifierKind.ZeroOrMore => (0, null),
		QuantifierKind.OneOrMore  => (1, null),
		_                         => (count ?? 0, count),
	};

	RuleSymbol RuleOf(Expr expression, string name) =>
		_model.Bindings.TryGetValue(expression, out var symbol) && symbol is RuleSymbol rule
			? rule
			: Unresolved(name);

	/// <summary>Binding already reported it; a placeholder keeps lowering going.</summary>
	static RuleSymbol Unresolved(string name) =>
		new(name, new GrammarScope("<unresolved>", null), Declaration: null);

	/// <summary>
	/// Values in `=&gt;` and `where` are C# by the time they get here — and are rendered
	/// as C#, not described.
	/// </summary>
	/// <remarks>
	/// What comes out is pasted into the generated file and compiled by C#, so a name
	/// that resolves to nothing is C#'s error to report, on the grammar's line (§7.6).
	/// The `@` is not part of it: it marks the crossing into C# and does not survive it.
	/// </remarks>
	static string Text(Expr value) => value switch
	{
		Expr.CSharp(var text)                   => $"({text})",
		Expr.Reference(_, var name, var types)  => name + TypeArguments(types),
		Expr.Call(var target, var arguments)    =>
			Text(target) + "(" + string.Join(", ", arguments.Select(Text)) + ")",

		Expr.Literal(true,  var text)           => CharRange.Quote(text[0]),
		Expr.Literal(false, var text)           => "\"" + text.Replace("\\", @"\\").Replace("\"", "\\\"") + "\"",

		_                                       => value.ToString() ?? "",
	};

	static string TypeArguments(IReadOnlyList<TypeRef> types) =>
		types.Count == 0 ? "" : "<" + string.Join(", ", types.Select(TypeName)) + ">";

	/// <summary>A type as it is written in C#, which is as the grammar wrote it.</summary>
	internal static string TypeName(TypeRef type) => type.Name + (type.IsSequence ? "[]" : "");

	/// <summary>
	/// A set of one-item tests, with references to elementary rules merged into it.
	/// </summary>
	/// <remarks>
	/// §3.1 allows only ranges, characters and references to other elementary rules
	/// inside brackets, and merging is what makes that restriction mean something: a set
	/// stays one test against one item however it was written, and nothing downstream
	/// has to know a reference was ever there.
	/// </remarks>
	Node.Element LowerElementSet(bool negated, IReadOnlyList<Elem> items, Expr set)
	{
		var ranges     = new List<CharRange>();
		var categories = new List<string>();
		var references = new List<Symbol>();

		foreach (var item in items)
		{
			switch (item)
			{
				case Elem.Chars(var from, var to) when from.Length > 0:
					ranges.Add(new CharRange(from[0], (to ?? from)[0]));
					break;

				case Elem.Category(var name):
					categories.Add(name);
					break;

				case Elem.Ref(var reference):
					Merge(reference, ranges, categories, references);
					break;
			}
		}

		return new Node.Element(negated, Coalesce(ranges), categories, references);

		void Merge(Expr.Reference reference, List<CharRange> into, List<string> alsoInto, List<Symbol> unresolved)
		{
			if (!_model.Bindings.TryGetValue(reference, out var symbol) || symbol is not RuleSymbol rule)
			{
				// A C# predicate — `[Letter | @IsDigit]` — needs the C# seam at run time,
				// which does not exist yet. Named as unbuilt rather than silently dropped.
				Report(
					UnsupportedElement,
					symbol is CSharpSymbol
						? $"'@{reference.Name}' cannot be used inside an element set yet: C# predicates are not implemented."
						: $"'{reference.Name}' is not a rule.",
					set.At);

				unresolved.Add(symbol ?? Unresolved(reference.Name));

				return;
			}

			// Not a set but a call: what it refers to must be one item drawn from a set,
			// or the brackets would be testing something that is not an item.
			if (BodyOf(rule) is not Node.Element(false, var theirRanges, var theirCategories, var theirReferences))
			{
				Report(
					UnsupportedElement,
					$"'{rule.Name}' is not an elementary rule, so it cannot appear inside an element set. " +
					"Only ranges, characters and rules that are themselves a single element set are allowed.",
					set.At);

				return;
			}

			into.AddRange(theirRanges);
			alsoInto.AddRange(theirCategories);
			unresolved.AddRange(theirReferences);
		}
	}

	/// <summary>
	/// Sorts and merges ranges: `'a' | 'b'` becomes `'a'..'b'`, a range swallows what it
	/// contains, duplicates fall away. Order among them is not observable, since a set
	/// matches exactly one item — which is what makes this fold legal.
	/// </summary>
	static IReadOnlyList<CharRange> Coalesce(List<CharRange> ranges)
	{
		if (ranges.Count < 2)
			return ranges;

		ranges.Sort((x, y) => x.From != y.From ? x.From.CompareTo(y.From) : x.To.CompareTo(y.To));

		var merged = new List<CharRange> { ranges[0] };

		for (var i = 1; i < ranges.Count; i++)
		{
			var last = merged[merged.Count - 1];
			var next = ranges[i];

			if (next.From <= last.To || next.From == last.To + 1)
				merged[merged.Count - 1] = new CharRange(last.From, (char)Math.Max(last.To, next.To));
			else
				merged.Add(next);
		}

		return merged;
	}

	Node LowerSequence(IReadOnlyList<Expr> operands, GrammarScope scope)
	{
		var nodes  = new List<Node>();
		var trivia = TriviaFor(scope);

		foreach (var operand in operands)
		{
			if (nodes.Count > 0 && trivia is not null)
				nodes.Add(trivia);

			nodes.Add(Lower(operand, scope));
		}

		return Flatten(MergeLiterals(nodes));
	}

	/// <summary>
	/// The `Trivia` this scope sees, or null when it matches nothing — in which case the
	/// insertions are not emitted at all rather than emitted and skipped (§4.5).
	/// </summary>
	Node? TriviaFor(GrammarScope scope) =>
		_model.Trivia.TryGetValue(scope, out var trivia) && !MatchesNothing(trivia, [])
			? CallTo(trivia, [])
			: null;

	/// <summary>
	/// Whether a rule can only ever match the empty sequence. Stronger than nullable,
	/// which merely allows it — and it is the stronger property that lets an insertion
	/// be dropped rather than kept and skipped at run time.
	/// </summary>
	bool MatchesNothing(RuleSymbol rule, HashSet<RuleSymbol> seen) =>
		rule.IsBuiltIn
			? rule.Name is "none" or "Trivia" or "eof"
			: seen.Add(rule) && _bodies.TryGetValue(rule, out var body) && MatchesNothing(body, seen);

	bool MatchesNothing(Node node, HashSet<RuleSymbol> seen) => node switch
	{
		Node.Empty          => true,
		Node.Literal(var t) => t.Length == 0,
		Node.Repeat(var body, _, var max) => max == 0 || MatchesNothing(body, seen),
		Node.Sequence(var nodes)   => nodes.All(child => MatchesNothing(child, seen)),
		Node.Choice(var nodes)     => nodes.All(child => MatchesNothing(child, seen)),
		Node.Capture(_, var body)  => MatchesNothing(body, seen),
		Node.Construct(var body, _) => MatchesNothing(body, seen),
		Node.Call(var rule, _)     => MatchesNothing(rule, seen),
		_                          => false,
	};

	/// <summary>`'a' &amp; 'b'` is `"ab"`: a sequence of literals already means their
	/// concatenation.</summary>
	static List<Node> MergeLiterals(List<Node> nodes)
	{
		var merged = new List<Node>();

		foreach (var node in nodes)
		{
			if (node is Node.Literal(var text) &&
				merged.Count > 0 &&
				merged[merged.Count - 1] is Node.Literal(var previous))
			{
				merged[merged.Count - 1] = new Node.Literal(previous + text);
				continue;
			}

			merged.Add(node);
		}

		return merged;
	}

	static Node Flatten(List<Node> nodes)
	{
		var flat = new List<Node>();

		foreach (var node in nodes)
		{
			if (node is Node.Sequence(var nested))
				flat.AddRange(nested);
			else
				flat.Add(node);
		}

		return flat.Count == 1 ? flat[0] : new Node.Sequence(flat);
	}

	Node LowerChoice(IReadOnlyList<Expr> alternatives, GrammarScope scope)
	{
		var nodes = alternatives.Select(a => Lower(a, scope)).ToList();

		var merged = MergeAdjacentElements(nodes);

		// A choice of one is that one: merging alternatives into a set routinely leaves
		// a single node behind, and keeping a wrapper around it would show up in every
		// dump and in every generated switch.
		return merged.Count == 1 ? merged[0] : new Node.Choice(merged);
	}

	/// <summary>
	/// Merges a run of adjacent single-item alternatives into one set. Only a run:
	/// merging across a multi-item alternative would move something past it, and that
	/// is the mistake Roc's macro made.
	/// </summary>
	static IReadOnlyList<Node> MergeAdjacentElements(List<Node> nodes)
	{
		var merged = new List<Node>();
		var run    = new List<Node>();

		void FlushRun()
		{
			if (run.Count == 0)
				return;

			merged.Add(run.Count == 1 ? run[0] : Combine(run));
			run.Clear();
		}

		foreach (var node in nodes)
		{
			if (IsSingleItem(node))
				run.Add(node);
			else
			{
				FlushRun();
				merged.Add(node);
			}
		}

		FlushRun();

		return merged;
	}

	static bool IsSingleItem(Node node) => node switch
	{
		Node.Literal(var text) => text.Length == 1,
		Node.Element(var negated, _, _, _) => !negated,
		_ => false,
	};

	static Node.Element Combine(List<Node> run)
	{
		var ranges     = new List<CharRange>();
		var categories = new List<string>();
		var references = new List<Symbol>();

		foreach (var node in run)
		{
			switch (node)
			{
				case Node.Literal(var text):
					ranges.Add(new CharRange(text[0], text[0]));
					break;

				case Node.Element(_, var elementRanges, var elementCategories, var elementReferences):
					ranges.AddRange(elementRanges);
					categories.AddRange(elementCategories);
					references.AddRange(elementReferences);
					break;
			}
		}

		return new Node.Element(false, Coalesce(ranges), categories, references);
	}

	/// <summary>
	/// An alternative that a preceding literal shadows as a prefix can never be
	/// reached. Diagnosed rather than repaired — see docs/syntax.md §11.
	/// </summary>
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

		else if (declared && building < offered.Count)
			Report(
				UnbuiltConstruction,
				$"'{rule.Name}' declares a type, so every alternative needs a '=>' to build it. " +
				"Matching captures to a constructor by name (§7.3) is not implemented yet.",
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

		foreach (var node in Everything(_bodies[rule]))
			if (_recoveries.ContainsKey(node))
				found++;

		if (found > 1)
			Report(
				UnbuiltRecovery,
				$"'{rule.Name}' marks {found} repetitions with 'recover' and only one may be marked. " +
				"Give the other its own rule.",
				rule.Declaration!.At);

		foreach (var node in Everything(_bodies[rule]))
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

	static IEnumerable<Node> Everything(Node node)
	{
		yield return node;

		foreach (var child in Children(node))
			foreach (var inside in Everything(child))
				yield return inside;
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

		foreach (var child in Children(node))
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

		foreach (var child in Children(node))
			CheckCaptures(child, rule, inside, lookings);
	}

	void CheckRepetitions(Node node, RuleSymbol rule)
	{
		if (node is Node.Repeat(var body, _, var max) && max != 1 && IsNullable(body))
			Report(
				NullableRepetition,
				$"The body of a repetition in '{rule.Name}' can match without consuming input, so the repetition would not terminate.",
				rule.Declaration!.At);

		foreach (var child in Children(node))
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

	static IEnumerable<Node> Children(Node node) => node switch
	{
		Node.Sequence(var nodes)        => nodes,
		Node.Choice(var nodes)          => nodes,
		Node.Repeat(var body, _, _)     => [body],
		Node.Capture(_, var body)       => [body],
		Node.Construct(var body, _)     => [body],
		Node.Lookahead(_, var body)     => [body],
		Node.Call(_, var arguments)     => arguments,
		_                               => [],
	};
}
