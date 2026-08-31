using System;

using DotGram.Generation;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>
/// Turning the bound tree into nodes: one pass down a rule, and the folds that are
/// safe to make on the way (§11).
/// </summary>
public sealed partial class GrammarNormalizer
{
	/// <summary>
	/// Every declared rule, collected before any of them is lowered.
	/// </summary>
	/// <remarks>
	/// Two passes rather than one, so that lowering may ask for another rule's body and
	/// get it whatever the declaration order was — which a reference inside an element
	/// set needs, since it has to be merged into the set that names it.
	/// </remarks>
	void Collect(GrammarNamespace ns)
	{
		// A parameterized rule is not a rule until it is called: its body names things
		// that only a call gives values to (§4.2). What goes in the graph is the
		// specializations, made where the calls are, so the template itself is left out —
		// lowering it here would report a count with nothing passed for it, and emit a
		// recognizer nobody could call.
		foreach (var rule in ns.Rules.Values)
			if (rule.Declaration is { Params.Count: 0 })
				_rules.Add(rule);

		foreach (var nested in ns.Nested)
			Collect(nested);
	}

	void LowerAll()
	{
		// Indexed, because lowering registers built-ins and appends them.
		for (var i = 0; i < _rules.Count; i++)
			BodyOf(_rules[i]);

		foreach (var rule in _rules)
			if (rule.Declaration is not null && TriviaFor(rule.Namespace) is { } trivia)
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

		var outerRule = _currentRule;
		_currentRule  = rule;

		var lowered = Lower(rule.Declaration.Body, rule.Namespace);

		_currentRule = outerRule;

		return _bodies[rule] = lowered;
	}

	/// <summary>
	/// The rule whose body is currently being lowered — which of it a pending
	/// <c>with (...)</c> site's rewrite has to be spliced back into once lowering is
	/// done (§18). Saved and restored around every place a top-level <see cref="Lower"/>
	/// result is assigned into <see cref="_bodies"/>, <see cref="BodyOf"/> and
	/// <see cref="Specialize"/> — including the reentrant case where an elementary
	/// rule's own <see cref="BodyOf"/> is asked for from inside another rule's lowering.
	/// </summary>
	RuleSymbol? _currentRule;

	Node Lower(Expr expression, GrammarNamespace ns) => expression switch
	{
		Expr.Literal   (_, var value) { IgnoreCase: var ignoreCase } => Bounded(value, ns, ignoreCase),
		Expr.ElementSet(var negated, var items)          => LowerElementSet(negated, items, expression),
		Expr.Group     (var body)                        => Lower(body, ns),
		Expr.Atomic    (var body)                        => new Node.Atomic(Lower(body, ns)),
		Expr.Capture   (var name, var operand)           => new Node.Capture(name, Lower(operand, ns)),
		Expr.Lookahead (var positive, var operand)       => new Node.Lookahead(positive, Lower(operand, ns)),
		Expr.Guard     (var value)                       => Guarded(value),
		Expr.CSharp    (var text)                        => new Node.Guard(Substituted($"@({text})"), StartOf(expression)),
		Expr.Construct (var pattern, var value)          => LowerConstruct(pattern, value, ns),
		Expr.Bound     (var body, var isLeft, var level) => LowerBound(body, isLeft, level, ns),

		// Parsed and refused rather than parsed and ignored: a `recover` that means
		// nothing would swallow a bad record in silence.
		Expr.Recovering(var body, var sync, var factory) => LowerRecovery(body, sync, factory, ns, expression),

		// §4.2: a count may be a parameter's name, and inside a specialization it stands
		// for the number the call passed.
		Expr.Quantified(var operand, var kind, var min, var minName, var max, var maxName) =>
			Repeated(
				Lower(operand, ns),
				Bounds(kind, Counted(min, minName, expression)).Min,
				Bounds(kind, Counted(max, maxName, expression)).Max,
				ns),

		Expr.Sequence (var operands)              => LowerSequence(operands, ns),
		Expr.Choice   (var alternatives)          => LowerChoice(alternatives, ns),
		Expr.Call     (var target, var arguments) => LowerCall(RuleOf(expression, target.Name), arguments, ns),
		Expr.Reference(_, var name, _)            => LowerReference(expression, name),
		Expr.With     (var operand, _)            => LowerWith(expression, operand, ns),
		Expr.Marked   (var operand, var value)    => new Node.Marked(Lower(operand, ns), Substituted(Text(value))),
		_                                         => Node.Empty.Instance,
	};

	/// <summary><c>=&gt; expr</c>.</summary>
	Node LowerConstruct(Expr pattern, Expr value, GrammarNamespace ns)
	{
		return new Node.Construct(
			Lower(pattern, ns),
			new Construction.Expression(Substituted(Text(value)), StartOf(value)));
	}

	// Substituted, because a value parameter is allowed anywhere a value is expected and
	// this is one of those places (§4.2, GrammarNormalizer.Values.cs). Off every path but
	// a specialization's own: outside one there is nothing to substitute and the text is
	// handed back as it was written.
	Node Guarded(Expr value) => new Node.Guard(Substituted(Text(value)), StartOf(value));

	/// <summary>
	/// Where the C# of an expression starts, which is not always where the expression does.
	/// </summary>
	/// <remarks>
	/// The <c>@</c> is the grammar saying that C# follows (§2); it is not part of the C#
	/// and is not written out. So the text emitted begins one character further along than
	/// the expression does, and a <c>#line</c> that ignored that would put every column one
	/// to the left — under the <c>@</c> rather than under the code.
	/// </remarks>
	static int StartOf(Expr value) => value switch
	{
		Expr.CSharp                      => value.At.Position + 1,
		Expr.Call     (var target, _)    => target.IsCSharp ? value.At.Position + 1 : value.At.Position,
		Expr.Reference(var csharp, _, _) => csharp ? value.At.Position + 1 : value.At.Position,
		_                                => value.At.Position,
	};

	/// <summary>
	/// A bare name standing where an operand goes: a rule to call, or something else.
	/// </summary>
	/// <remarks>
	/// A bare C# name here is unambiguously §7.1's input-consuming recognizer. A predicate
	/// over one item appears inside an element set instead, as <c>[@Name]</c>.
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

		// A value parameter standing where an operand goes is the other half of the same
		// silence: §4.2 allows a value where a value is expected — a count, an argument of
		// `@Method`, inside `@(...)` — and this is none of those. Left alone it lowered to
		// an empty element set and the parse refused everything, naming a set the author
		// never wrote (found by writing `open & item & close` against `open: char`).
		if (symbol is ParameterSymbol && _values.ContainsKey(name))
		{
			Report(
				UnbuiltCall,
				$"'{name}' is a value (docs/syntax.md §4.2) and stands here where a piece of " +
				"grammar goes. A value is allowed where a value is expected — a count, an " +
				"argument of '@Method', inside '@(...)'. Drop its type to make it a recognizer.",
				expression.At);

			return Node.Empty.Instance;
		}

		if (symbol is RuleSymbol rule)
			return CallTo(rule, []);

		// §7.1: the method reads the input itself. Nothing is checked about what it does
		// with the position it is handed — the `ref` is it saying that it moves one, and
		// a grammar that reaches into the parse takes the parse's invariants on. Which of
		// the second row (text) or the third (a value of its own) is asked of the host,
		// since bare `@Name` does not say — the two are told apart only by which overload
		// the method has.
		if (symbol is CSharpSymbol reader)
			switch (_resolver.TryResolveExternalValue(reader.Name, against: null, out var valueType))
			{
				case ExternalValueResolution.Found:
					return CallTo(ExternalRuleFor(reader.Name, valueType!), []);

				case ExternalValueResolution.Ambiguous:
					Report(
						AmbiguousExternal,
						$"'{reader.Name}' has more than one '(System.ReadOnlySpan<char>, ref int, out T)' " +
						$"overload, with different T. Bare '@{reader.Name}' cannot say which is meant " +
						"(docs/syntax.md §7.1); give it one such overload, or none.",
						expression.At);

					goto default;

				default:
					return new Node.External(reader.Name);
			}

		return new Node.Element(false, [], [], [symbol]);
	}

	/// <summary>Every value-returning external recognizer synthesized so far, by method name.</summary>
	readonly Dictionary<string, RuleSymbol> _externals = new(StringComparer.Ordinal);

	/// <summary>
	/// The one rule standing for a value-returning external recognizer named this — §7.1's
	/// third row, given a rule-shaped identity so everything downstream (<c>BuildsValue</c>,
	/// <c>ValueRule</c>, materialization) treats it as an ordinary typed rule and needs no
	/// case of its own.
	/// </summary>
	/// <remarks>
	/// Never looked up by name — nothing in a grammar can write this rule's name — so its
	/// namespace is a placeholder rather than one that matters: nothing downstream reads
	/// <see cref="RuleSymbol.Namespace"/> for a rule whose <see cref="RuleSymbol.Declaration"/>
	/// is null (confirmed at <c>ComputeTypes</c>'s first line, the same guarantee every
	/// built-in already relies on).
	/// </remarks>
	RuleSymbol ExternalRuleFor(string method, string valueType)
	{
		if (_externals.TryGetValue(method, out var existing))
			return existing;

		var rule = new RuleSymbol("@" + method, new GrammarNamespace("<external>", null), Declaration: null);

		_externals[method] = rule;
		_rules.Add(rule);
		_bodies[rule] = new Node.External(method) { HasValue = true };
		_types[rule]  = valueType;

		return rule;
	}

	/// <summary>
	/// A rule whose whole body is a call to a value-returning external recognizer takes
	/// that value — the same shape as §4.1 case 3's pass-through, applied to a producer
	/// that is not a rule the grammar wrote.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Rewritten directly rather than through <c>_produces</c>/<c>PassThrough</c>'s own
	/// generic machinery, and for a specific reason: that machinery settles whether an
	/// operand's value fits the declared type by asking <see cref="ISymbolResolver.IsAssignable"/> as an ordinary, separately cached question — which needs both types
	/// known from grammar syntax alone, ahead of asking. The type on this side of the
	/// question is discovered by <em>this same call</em> (<see cref="ISymbolResolver.TryResolveExternalValue"/>'s <c>against</c> parameter already verified it), so
	/// nothing upstream could have pre-asked an ordinary <c>Fits</c> question about it —
	/// asking one here would be the exact "question the collector did not foresee" defect
	/// <see cref="AnsweredSymbolResolver"/> exists to catch. Fitness is settled once, by the
	/// one call already made for this exact (method, declared type) pair; what remains is
	/// mechanical — wrap the call in the same implicit capture <c>Collected</c> would have
	/// produced and give it the same <see cref="Construction.Operand"/> marker
	/// <c>PassThrough</c> would have.
	/// </para>
	/// <para>
	/// Deliberately narrow: only a rule with no capture of its own and a body that is
	/// exactly one call to a synthesized external rule. A captured use is already handled,
	/// for free, by <see cref="CaptureLayout"/> once <see cref="ExternalRuleFor"/> gives
	/// the callee a type — and is left alone here.
	/// </para>
	/// </remarks>
	void ProduceFromExternals()
	{
		foreach (var rule in _rules)
			if (rule.Declaration?.Type is { } type &&
				_bodies[rule] is Node.Call(var called, _) &&
				_bodies[called] is Node.External(var method) { HasValue: true } &&
				_resolver.TryResolveExternalValue(method, TypeName(type), out _) == ExternalValueResolution.Found)
			{
				_bodies[rule] = new Node.Construct(
					new Node.Capture("item0", _bodies[rule]), Construction.Operand.Instance);
			}
	}

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
	Node LowerBound(Expr body, bool isLeft, int level, GrammarNamespace ns)
	{
		var alternative = Lower(body, ns);

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
	Node LowerRecovery(Expr body, Expr sync, Expr? factory, GrammarNamespace ns, Expr at)
	{
		var repetition = Lower(body, ns);

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
			Lower(sync, ns),
			factory is null ? null : Text(factory));

		return repetition;
	}

	/// <summary>
	/// <c>Number with (Point = Comma)</c> — lowered to exactly what the operand alone
	/// would be, no wrapper node, so a capture, a sequence or another <c>with</c> stays
	/// literally embedded in the enclosing rule's own body (§5.1). What is recorded is
	/// where to come back once every rule is lowered and the whole call graph is known —
	/// the same ordering <see cref="SpecializeNamespaces"/> already relies on for
	/// <c>namespace (...)</c>.
	/// </summary>
	Node LowerWith(Expr expression, Expr operand, GrammarNamespace ns)
	{
		var lowered = Lower(operand, ns);

		if (_model.WithBindings.TryGetValue(expression, out var targets) && targets.Count > 0)
			_pendingWith.Add(new WithSite(_currentRule!, lowered, targets, "With" + (++_withCounter)));

		return lowered;
	}

	/// <summary>Something the notation says and the compiler cannot do yet.</summary>
	Node Unbuilt(Expr body, GrammarNamespace ns, Expr at, string id, string message)
	{
		Report(id, message, at.At);

		return Lower(body, ns);
	}

	/// <summary>
	/// A call — and, the first time a built-in is called, the body it is a call to.
	/// </summary>
	/// <remarks>
	/// §3.1 says <c>any</c>, <c>none</c>, <c>eol</c>, <c>eof</c> and <c>trivia</c> are
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

		return new Node.Call(rule, arguments);
	}

	/// <summary>
	/// A call, which for a parameterized rule is a rule of its own (§4.2).
	/// </summary>
	/// <remarks>
	/// The arguments arrive unlowered, because whether one is a piece of grammar or a
	/// value is decided here: a number is neither a recognizer nor lowerable into one, and
	/// stands where a count goes rather than where an operand does.
	/// </remarks>
	Node LowerCall(RuleSymbol rule, IReadOnlyList<Expr> arguments, GrammarNamespace ns)
	{
		if (rule.Declaration is not { Params.Count: > 0 })
			return CallTo(rule, [.. arguments.Select(argument => Lower(argument, ns))]);

		return Specialize(rule, arguments, ns);
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
	Node Specialize(RuleSymbol rule, IReadOnlyList<Expr> arguments, GrammarNamespace ns)
	{
		var declaration = rule.Declaration!;

		// §4.2: a recursive call that wraps its own argument specializes for ever, and
		// "for ever" in a source generator is a stack overflow — which is not an exception
		// and takes the process with it, so the author's IDE loses the compiler rather than
		// showing a message about their grammar. Checked on the way in, before the
		// arguments are lowered, because lowering an argument is where the recursion is.
		if (_specializing.Count >= SpecializationDepth)
		{
			Report(
				UnbuiltCall,
				$"'{rule.Name}' is called with an argument built from its own, so every call needs " +
				"another rule and there is no end to them — "                                        +
				$"{_specializing[0]} … {_specializing[^1]}, and growing. "                           +
				"§4.2 rejects this when the grammar is built rather than letting it run.",
				declaration.At);

			return Node.Empty.Instance;
		}

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

		// Lowered in the caller's own substitution, because an argument may name one of
		// the caller's parameters: `Pair(item) = List(item, Comma)` passes on what it was
		// given rather than a parameter of its own.
		var passed = new List<Node?>(arguments.Count);
		var counts = new List<int?>(arguments.Count);
		var values = new List<string?>(arguments.Count);

		// §4.2 gives a parameter's kind by what its own declaration says — a C# type makes
		// it a value, anything else a recognizer — and the argument is read as that kind.
		// The same `' '` is a piece of grammar where the parameter is a recognizer and a
		// `char` where it is a value, which is why this pairs the two rather than reading
		// the argument alone.
		for (var i = 0; i < arguments.Count; i++)
		{
			var parameter = declaration.Params[i];
			var argument  = arguments[i];

			if (parameter.Type is { } kind && (kind.IsCSharp || IsCSharpKeyword(kind.Name)))
			{
				// A value is a literal, or a value this rule was handed itself. §4.2's
				// other spelling — a value captured earlier in the parse — is not one: a
				// specialization is made before anything runs, and a captured value
				// exists only while it does (docs/next.md).
				var value = argument switch
				{
					Expr.Number(var number) => Text(number),
					Expr.Literal            => Text(argument),
					Expr.Reference(false, var named, _) when _values.TryGetValue(named, out var carried)
						=> carried,
					_ => null,
				};

				if (value is null)
				{
					Report(
						UnbuiltCall,
						$"'{parameter.Name}' is declared as the C# type '{TypeName(kind)}', which " +
						"docs/syntax.md §4.2 makes a value, and a value is a literal or a value this " +
						"rule was handed itself. Pass one, or drop the type to make it a recognizer.",
						parameter.At);

					return Node.Empty.Instance;
				}

				// What C# will make of the literal, against what the parameter says it is.
				// The resolver is the seam that knows; one that admits everything leaves
				// the answer to the consumer's own compiler, which is where §7.4 puts
				// every other question about the C# a grammar wrote.
				if (LiteralType(argument) is { } actual &&
					!_resolver.IsAssignable(actual, TypeName(kind)))
				{
					Report(
						UnbuiltCall,
						$"'{parameter.Name}' is declared as '{TypeName(kind)}' and is passed a {actual}.",
						parameter.At);

					return Node.Empty.Instance;
				}

				passed.Add(null);
				values.Add(value);

				// A number is the one value a quantifier count can also be.
				counts.Add(argument switch
				{
					Expr.Number(var number) => number,
					Expr.Reference(false, var named, _) when _counts.TryGetValue(named, out var forwarded)
						=> forwarded,
					_ => null,
				});

				continue;
			}

			// A number is a value whatever the parameter's declaration says, because a
			// quantifier count is where one goes and `Digits(n) = ['0'..'9']{n}` is how
			// §4.2's own example writes it — the type is what a rule needs only when the
			// value reaches C#.
			if (argument is Expr.Number(var count))
			{
				passed.Add(null);
				counts.Add(count);
				values.Add(Text(count));

				continue;
			}

			if (argument is Expr.Reference(false, var handed, _) &&
				(_counts.ContainsKey(handed) || _values.ContainsKey(handed)))
			{
				passed.Add(null);
				counts.Add(_counts.TryGetValue(handed, out var number) ? number : null);
				values.Add(_values.TryGetValue(handed, out var held) ? held : null);

				continue;
			}

			passed.Add(Lower(argument, ns));
			counts.Add(null);
			values.Add(null);
		}

		return Instantiate(rule, passed, counts, values);
	}

	/// <summary>The C# type a literal argument is, or null where the argument is not one.</summary>
	static string? LiteralType(Expr argument) => argument switch
	{
		Expr.Number                => "int",
		Expr.Literal(true,  _)     => "char",
		Expr.Literal(false, _)     => "string",
		_                          => null,
	};

	/// <summary>
	/// The specialization itself, from arguments already lowered — split from
	/// <see cref="Specialize"/> so a parameterized rebinding can build the replacement's
	/// specialization for the same arguments a call already carried (§5.1), long after
	/// <c>LowerAll</c> has run.
	/// </summary>
	Node Instantiate(
		RuleSymbol rule, IReadOnlyList<Node?> passed, IReadOnlyList<int?> counts,
		IReadOnlyList<string?> values)
	{
		var declaration = rule.Declaration!;

		// Keyed by what the arguments are, so two calls that pass the same things share
		// one specialization — and a value is part of that: `Padded(Word, ' ')` and
		// `Padded(Word, '\t')` are two rules, not one.
		var key = rule.Name + "(" + string.Join(", ", passed.Select((node, i) =>
			node?.ToString() ?? values[i] ?? "?")) + ")";

		if (_specialized.TryGetValue(key, out var made))
			return new Node.Call(made, []);

		var specialized = new RuleSymbol(NameFor(rule, passed, counts, values), rule.Namespace, declaration);

		_specialized[key] = specialized;
		_rules.Add(specialized);

		// What this specialization is an instance of, and of what arguments — the one
		// fact a parameterized rebinding needs later: a call to it is a call to the rule
		// with these arguments, and the binding replaces the rule, not the instance.
		_origins[specialized] = (rule, passed, counts, values);

		// Before lowering, for the same reason an ordinary rule is: a specialization that
		// reaches itself would otherwise recurse here rather than be reported.
		_bodies[specialized] = Node.Empty.Instance;

		var outerArguments = _arguments;
		var outerCounts    = _counts;
		var outerValues    = _values;

		_arguments = new Dictionary<string, Node>(StringComparer.Ordinal);
		_counts    = new Dictionary<string, int>(StringComparer.Ordinal);
		_values    = new Dictionary<string, string>(StringComparer.Ordinal);

		for (var i = 0; i < passed.Count; i++)
		{
			if (passed[i] is { } node)
				_arguments[declaration.Params[i].Name] = node;

			if (counts[i] is { } count)
				_counts[declaration.Params[i].Name] = count;

			if (values[i] is { } value)
				_values[declaration.Params[i].Name] = value;
		}

		_specializing.Add(specialized.Name);

		var outerRule = _currentRule;
		_currentRule  = specialized;

		_bodies[specialized] = Lower(declaration.Body, rule.Namespace);
		_currentRule         = outerRule;
		_arguments           = outerArguments;
		_counts              = outerCounts;
		_values             = outerValues;

		_specializing.RemoveAt(_specializing.Count - 1);

		if (declaration.Type is { } declared)
		{
			if (declared.IsCSharp || IsCSharpKeyword(declared.Name))
			{
				_types[specialized] = TypeName(declared);
			}

			// §4.2: `: item` is the result being whatever the argument produces, and that
			// is knowable here and only here — this specialization has one concrete
			// argument. What it produces is not known yet, though: rule types are worked
			// out after every body is lowered, so the pairing is recorded and resolved
			// there. §4.1 case 3 — a type naming a rule rather than a parameter — stays
			// refused where it is declared.
			// §4.2: the result is whatever the argument produces, and which argument that is
			// is knowable here and only here — this specialization has one concrete one.
			// What it produces is not known yet, since rule types are worked out after every
			// body is lowered, so the pairing is recorded and resolved there.
			else if (declaration.Params.Any(one => one.Name == declared.Name))
			{
				for (var i = 0; i < declaration.Params.Count; i++)
					if (declaration.Params[i].Name == declared.Name && passed[i] is Node.Call(var produced, _))
						_produces[specialized] = (produced, declared.IsSequence);
			}
		}

		return new Node.Call(specialized, []);
	}

	/// <summary>
	/// A specialization whose result type is its argument's, and whether it collects them
	/// (§4.2). Resolved once every rule's own type is known.
	/// </summary>
	readonly Dictionary<RuleSymbol, (RuleSymbol Produces, bool IsSequence)> _produces = [];

	/// <summary>
	/// What each specialization is an instance of, and of what arguments — read by a
	/// parameterized rebinding (§5.1), which replaces the rule a call named and keeps
	/// the call's arguments, so it must be able to say "the same arguments, of the
	/// replacement" long after the call itself was lowered away.
	/// </summary>
	readonly Dictionary<RuleSymbol, (
		RuleSymbol Origin, IReadOnlyList<Node?> Passed, IReadOnlyList<int?> Counts,
		IReadOnlyList<string?> Values)> _origins = [];

	/// <summary>A repetition count: written, or the name of a parameter that carries one.</summary>
	int? Counted(int? written, string? name, Expr at)
	{
		if (written is not null || name is null)
			return written;

		if (_counts.TryGetValue(name, out var passed))
			return passed;

		Report(
			UnbuiltCall,
			$"'{name}' is a repetition count and no number was passed for it. A count may name " +
			"a parameter of the rule it is in, and that parameter has to be given a number at " +
			"every call (docs/syntax.md §4.2).",
			at.At);

		return null;
	}

	/// <summary>What each numeric parameter stands for while a specialization is lowered.</summary>
	Dictionary<string, int> _counts = new(StringComparer.Ordinal);

	/// <summary>How deep specializations may nest before the grammar is called runaway.</summary>
	/// <remarks>
	/// Generous, because a grammar built out of <c>Lex(List(Padded(…)))</c> is doing
	/// nothing wrong and nesting is how it says so. What this stops is unbounded growth,
	/// which passes this depth almost at once.
	/// </remarks>
	const int SpecializationDepth = 24;

	/// <summary>The specializations being lowered, outermost first.</summary>
	readonly List<string> _specializing = [];

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
	string NameFor(
		RuleSymbol rule, IReadOnlyList<Node?> passed, IReadOnlyList<int?> counts,
		IReadOnlyList<string?> values)
	{
		var name = rule.Name;

		for (var i = 0; i < passed.Count; i++)
			name += "_" + (passed[i] switch
			{
				// A number reads as itself; any other value is a piece of C# and its
				// characters are not all ones an identifier may hold, so it is named by
				// where it stands instead. The name only has to be distinct and legible —
				// and by position rather than by a running count, so that two values in
				// one call do not come out under the same word.
				null when counts[i] is { } count => Text(count),
				null                             => "value" + Text(i),
				Node.Call(var called, _)         => called.Name.Replace(".", "_"),
				_                                => Text(_specialized.Count),
			});

		var taken = name;

		for (var i = 2; Named(taken); i++)
			taken = name + "_" + Text(i);

		return taken;
	}

	static string Text(int value) =>
		value.ToString(System.Globalization.CultureInfo.InvariantCulture);

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

		// `none`, and `trivia` until a grammar shadows it with one of its own (§4.5).
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
		new(name, new GrammarNamespace("<unresolved>", null), Declaration: null);

	/// <summary>
	/// Values in `=&gt;` and `when` are C# by the time they get here — and are rendered
	/// as C#, not described.
	/// </summary>
	/// <remarks>
	/// What comes out is pasted into the generated file and compiled by C#, so a name
	/// that resolves to nothing is C#'s error to report, on the grammar's line (§7.6).
	/// The `@` is not part of it: it marks the crossing into C# and does not survive it.
	/// </remarks>
	static string Text(Expr value) => value switch
	{
		Expr.CSharp   (var text)                  => $"({text})",
		Expr.Reference(_, var name, var types)    => name + TypeArguments(types),
		Expr.Call     (var target, var arguments) =>
			Text(target) + "(" + string.Join(", ", arguments.Select(Text)) + ")",
		// A character literal with no character in it is not something an author can
		// mean, but it is something a broken file can hold — and this runs on a grammar
		// the lexer has already reported, because normalization does not stop at the
		// first diagnostic. Answered rather than thrown on (found by the fuzzer).
		Expr.Literal  (true,  var text)           => text.Length == 0
			? "''"
			: CharRange.Quote(text[0]),
		Expr.Literal  (false, var text)           => "\"" + text.Replace("\\", @"\\").Replace("\"", "\\\"") + "\"",
		_                                         => value.ToString(),
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
				// Both ends, not just the first. An empty upper bound reaches here from a
				// malformed range the parser has already reported — `['a'..'']` — and the
				// guard let it through to be indexed, which is a generator crash rather than
				// a grammar error. What it lowers to is nothing: the set goes on without it,
				// and the diagnostic that is already there is what the author reads.
				case Elem.Chars({ Length: > 0 } from, var to) when (to is null || to.Length > 0):
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
			if (!_model.Bindings.TryGetValue(reference, out var symbol))
			{
				Report(
					UnsupportedElement,
					$"'{reference.Name}' is not a rule.",
					set.At);

				unresolved.Add(Unresolved(reference.Name));

				return;
			}

			// The brackets are the contract: this C# method tests exactly one input item.
			// Emission writes Name(c), and the C# compiler resolves that overload.
			if (symbol is CSharpSymbol)
			{
				unresolved.Add(symbol);
				return;
			}

			if (symbol is not RuleSymbol rule)
			{
				Report(UnsupportedElement, $"'{reference.Name}' is not a rule.", set.At);
				unresolved.Add(symbol);
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
			var last = merged[^1];
			var next = ranges[i];

			if (next.From <= last.To || next.From == last.To + 1)
				merged[^1] = new CharRange(last.From, (char)Math.Max(last.To, next.To));
			else
				merged.Add(next);
		}

		return merged;
	}

	/// <summary>
	/// A repetition, spaced between its turns where what it repeats has seams of its own.
	/// </summary>
	/// <remarks>
	/// <para>
	/// §4.5 inserts trivia at every seam between the operands of a sequence. The turns of a
	/// repetition are such a seam wherever the thing repeated is itself a sequence:
	/// <c>A &amp; (S &amp; A)*</c> is how one writes <c>A S A S A</c>, and in that reading
	/// every join is between operands — the one that wraps around included. An author who
	/// wrote <c>S &amp; A</c> has already said that a space may stand between <c>S</c> and
	/// <c>A</c>; refusing one between <c>A</c> and the next <c>S</c> is the same seam
	/// answered two ways.
	/// </para>
	/// <para>
	/// What that keeps is the reason the insertion was ever held back. A lexeme is a
	/// repetition of a *single* operand — <c>['0'..'9']+</c>, <c>Letter+</c>, <c>Word*</c> —
	/// and a single operand has no seam inside a turn, so it gets none between them either.
	/// <c>1 2</c> is still two numbers and <c>a b</c> still two names.
	/// </para>
	/// <para>
	/// An optional is left alone: it has no second turn, so it has no seam to space.
	/// A repetition of a choice is left alone too, which is what keeps <c>trivia</c>'s own
	/// usual shape — <c>(Space | Comment)*</c> — from being asked to space itself. Spacing a
	/// repetition of one thing is the case that cannot be inferred, and stays what §4.5 says
	/// it is: <c>Attribute &amp; (trivia &amp; Attribute)*</c>, written out.
	/// </para>
	/// </remarks>
	Node Repeated(Node body, int min, int? max, GrammarNamespace ns)
	{
		if (max is 0 or 1 || body is not Node.Sequence(var operands) || TriviaFor(ns) is not { } trivia)
			return new Node.Repeat(body, min, max);

		// The same restraint as the sequence seam: a turn that already leads with the
		// author's own trivia needs none prepended. A repetition whose turn is a valued
		// rule is a list too, and is spaced by SpaceLists — after the types exist,
		// because valuedness is what tells a collected thing from a lexeme's inside.
		if (IsSeam(operands[0], trivia))
			return new Node.Repeat(body, min, max);

		var spaced = new List<Node>(operands.Count + 1) { trivia };

		spaced.AddRange(operands);

		return new Node.Repeat(new Node.Sequence(spaced), min, max);
	}

	Node LowerSequence(IReadOnlyList<Expr> operands, GrammarNamespace ns)
	{
		var nodes  = new List<Node>();
		var trivia = TriviaFor(ns);

		foreach (var operand in operands)
		{
			var lowered = Lower(operand, ns);

			// Not next to trivia the author already wrote. §4.5's own argument for
			// unconditional insertion is that a second application consumes nothing;
			// withholding ours where theirs already stands is the same statement made
			// without multiplying readings — `trivia & trivia` is one seam, and every
			// spelling of it the search would otherwise walk is the same span split two
			// ways.
			//
			// And not before a `when`. §4.5 separates operands, and a guard is not one:
			// it reads nothing, so there is no token on its other side for a seam to
			// separate. Weaving one anyway cost a trivia scan per guard and did
			// something worse than cost — it made the extent of an alternative depend
			// on whether its guard stood last, so a rule read more input when a guard
			// passed than when its unguarded twin matched the same text. The guard
			// evaluates where the operand before it ended, which is also what its
			// `parserSpan` now says.
			if (nodes.Count > 0 && trivia is not null && lowered is not Node.Guard &&
				!IsSeam(nodes[^1], trivia) && !IsSeam(lowered, trivia))
				nodes.Add(trivia);

			nodes.Add(lowered);
		}

		return Flatten(MergeLiterals(nodes));
	}

	/// <summary>Whether a node is a bare application of the namespace's own trivia.</summary>
	static bool IsSeam(Node node, Node trivia) =>
		node is Node.Call(var called, _) &&
		trivia is Node.Call(var seam, _) &&
		ReferenceEquals(called, seam);

	/// <summary>
	/// §4.6: a literal that is all word characters may not be the start of a longer word.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The same shape as trivia and for the same reason: an ordinary rule, shadowed to turn
	/// it on, and the insertion dropped entirely while it is empty — so a regex or a feed
	/// grammar pays nothing and a language grammar pays one line.
	/// </para>
	/// <para>
	/// Which literals qualify is decided here, when the grammar is built: every character
	/// of <c>"select"</c> continues a word, so it gets the check; <c>"("</c> does not, and
	/// asking whether a word character follows a bracket would refuse <c>(a)</c>.
	/// </para>
	/// <para>
	/// Before the trivia insertion, which is what §4.6 says and what makes it mean
	/// anything: `"select" &amp; ?!wordboundary` asks whether a letter follows the
	/// keyword, while the other order would ask whether one follows the whitespace after
	/// it.
	/// </para>
	/// </remarks>
	Node Bounded(string value, GrammarNamespace ns, bool ignoreCase)
	{
		var literal = new Node.Literal(value) { IgnoreCase = ignoreCase };

		if (value.Length == 0 || BoundaryFor(ns) is not { } boundary)
			return literal;

		foreach (var character in value)
			if (!Continues(boundary, character))
				return literal;

		// Both edges, not one. The lookahead alone kept `"as"` from starting a longer
		// word but not from ending one: backtracking could hand an identifier's tail
		// back and match the keyword mid-word, reading `Xas` as `X as` — which no
		// author means and no lexer would do. The lookbehind completes the symmetry:
		// a word lexeme is delimited by the boundary on both sides.
		return BoundaryElement(boundary) is { } element
			? new Node.Sequence([new Node.Behind(element), literal, new Node.Lookahead(false, boundary)])
			: new Node.Sequence([literal, new Node.Lookahead(false, boundary)]);
	}

	/// <summary>
	/// The boundary's own element, where the rule is one — which is the same shape
	/// <see cref="Continues"/> already requires to decide anything at all.
	/// </summary>
	Node.Element? BoundaryElement(Node boundary) => ElementOf(boundary);

	/// <summary>Whether this character is one the boundary rule says continues a word.</summary>
	/// <remarks>
	/// Read out of the lowered rule rather than asked of it at run time: the class is an
	/// element set, which is exactly a set of characters, so membership is a question this
	/// side can answer while it still has the grammar in hand.
	/// </remarks>
	bool Continues(Node boundary, char character) =>
		ElementOf(boundary) is { } element &&
		FirstSets.OfElement(element) is { Anything: false } characters &&
		characters.Overlaps(FirstSets.First.Chars([new CharRange(character, character)]));

	/// <summary>
	/// The element a node comes down to, through any chain of plain references.
	/// </summary>
	/// <remarks>
	/// `wordboundary = WordOrDigit` with `WordOrDigit = [\p{L} | '_']` names the element
	/// through a rule, and requiring the element to stand directly in the boundary's own
	/// body made §4.6 silently inert for exactly the grammar shape §4.6's own example
	/// recommends. Found when the symmetric weave did not fire; the asymmetric one had
	/// not been firing either, and nothing said so.
	/// </remarks>
	Node.Element? ElementOf(Node node, HashSet<RuleSymbol>? seen = null) => node switch
	{
		Node.Element element => element,
		Node.Call(var rule, _) when (seen ??= []).Add(rule)
			=> ElementOf(BodyOf(rule), seen),
		_ => null,
	};

	/// <summary>The `wordboundary` this namespace sees, or null while it matches nothing.</summary>
	Node? BoundaryFor(GrammarNamespace ns)
	{
		for (var at = ns; at is not null; at = at.Parent)
		{
			if (at.Rules.TryGetValue("wordboundary", out var rule) && !rule.IsBuiltIn)
				return MatchesNothing(rule, []) ? null : CallTo(rule, []);

			// A lexical namespace shields what stands outside it. Its literals are the
			// parts of one lexeme, not lexemes standing next to each other, and a word
			// boundary inherited across `trivia = none` guarded the 'u' of '￿'
			// against the hex digit that always follows it. A namespace that declares
			// its own boundary beside its empty trivia keeps it — that is the scannerless
			// keyword grammar, and the check above has already answered for it.
			if (_model.Trivia.TryGetValue(at, out var declared) && MatchesNothing(declared, []))
				return null;
		}

		return null;
	}

	/// <summary>
	/// The `trivia` this namespace sees, or null when it matches nothing — in which case the
	/// insertions are not emitted at all rather than emitted and skipped (§4.5).
	/// </summary>
	Node? TriviaFor(GrammarNamespace ns) =>
		_model.Trivia.TryGetValue(ns, out var trivia) && !MatchesNothing(trivia, [])
			? CallTo(trivia, [])
			: null;

	/// <summary>
	/// Whether a rule can only ever match the empty sequence. Stronger than nullable,
	/// which merely allows it — and it is the stronger property that lets an insertion
	/// be dropped rather than kept and skipped at run time.
	/// </summary>
	bool MatchesNothing(RuleSymbol rule, HashSet<RuleSymbol> seen) =>
		rule.IsBuiltIn
			? rule.Name is "none" or "trivia" or "eof" or "wordboundary"
			: seen.Add(rule) && _bodies.TryGetValue(rule, out var body) && MatchesNothing(body, seen);

	bool MatchesNothing(Node node, HashSet<RuleSymbol> seen) => node switch
	{
		Node.Empty          => true,
		Node.Literal(var t) => t.Length == 0,
		Node.Atomic(var body) => MatchesNothing(body, seen),
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
			if (node is Node.Literal(var text) { IgnoreCase: var ignoreCase } &&
				merged.Count > 0 &&
				merged[^1] is Node.Literal(var previous) { IgnoreCase: var previousIgnoreCase } &&
				ignoreCase == previousIgnoreCase)
			{
				merged[^1] = new Node.Literal(previous + text) { IgnoreCase = ignoreCase };
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

	Node LowerChoice(IReadOnlyList<Expr> alternatives, GrammarNamespace ns)
	{
		var nodes = alternatives.Select(a => Lower(a, ns)).ToList();

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
		Node.Literal(var text) { IgnoreCase: false } => text.Length == 1,
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
}
