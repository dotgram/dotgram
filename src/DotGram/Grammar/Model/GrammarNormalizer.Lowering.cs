using System;
using System.Collections.Generic;
using System.Linq;

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
	void Collect(GrammarScope scope)
	{
		// A parameterized rule is not a rule until it is called: its body names things
		// that only a call gives values to (§4.2). What goes in the graph is the
		// specializations, made where the calls are, so the template itself is left out —
		// lowering it here would report a count with nothing passed for it, and emit a
		// recognizer nobody could call.
		foreach (var rule in scope.Rules.Values)
			if (rule.Declaration is { Params.Count: 0 })
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
		Expr.Literal(_, var value)              => Bounded(value, scope),
		Expr.ElementSet(var negated, var items) => LowerElementSet(negated, items, expression),
		Expr.Group(var body)                    => Lower(body, scope),
		Expr.Capture(var name, var operand)     => new Node.Capture(name, Lower(operand, scope)),
		Expr.Lookahead(var positive, var operand) => new Node.Lookahead(positive, Lower(operand, scope)),
		Expr.Guard(var value)                   => Guarded(value),
		Expr.CSharp(var text)                   => new Node.Guard($"@({text})", StartOf(expression)),

		Expr.Construct(var pattern, var value)  => LowerConstruct(pattern, value, scope),

		Expr.Bound(var body, var isLeft, var level) => LowerBound(body, isLeft, level, scope),

		// Parsed and refused rather than parsed and ignored: a `recover` that means
		// nothing would swallow a bad record in silence.
		Expr.Recovering(var body, var sync, var factory) =>
			LowerRecovery(body, sync, factory, scope, expression),

		// §4.2: a count may be a parameter's name, and inside a specialization it stands
		// for the number the call passed.
		Expr.Quantified(var operand, var kind, var min, var minName, var max, var maxName) =>
			new Node.Repeat(
				Lower(operand, scope),
				Bounds(kind, Counted(min, minName, expression)).Min,
				Bounds(kind, Counted(max, maxName, expression)).Max),

		Expr.Sequence(var operands)             => LowerSequence(operands, scope),
		Expr.Choice(var alternatives)           => LowerChoice(alternatives, scope),

		Expr.Call(var target, var arguments) =>
			LowerCall(RuleOf(expression, target.Name), arguments, scope),

		Expr.Reference(_, var name, _) => LowerReference(expression, name),

		_ => Node.Empty.Instance,
	};

	/// <summary><c>=&gt; expr</c>.</summary>
	Node LowerConstruct(Expr pattern, Expr value, GrammarScope scope)
	{
		return new Node.Construct(
			Lower(pattern, scope), new Construction.Expression(Text(value), StartOf(value)));
	}

	static Node Guarded(Expr value) => new Node.Guard(Text(value), StartOf(value));

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
		Expr.Call(var target, _)         => target.IsCSharp ? value.At.Position + 1 : value.At.Position,
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

		if (symbol is RuleSymbol rule)
			return CallTo(rule, []);

		// §7.1's second row: the method reads the input itself. Nothing is checked about
		// what it does with the position it is handed — the `ref` is it saying that it
		// moves one, and a grammar that reaches into the parse takes the parse's
		// invariants on.
		if (symbol is CSharpSymbol reader)
			return new Node.External(reader.Name);

		return new Node.Element(false, [], [], [symbol]);
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
	Node LowerCall(RuleSymbol rule, IReadOnlyList<Expr> arguments, GrammarScope scope)
	{
		if (rule.Declaration is not { Params.Count: > 0 })
			return CallTo(rule, [.. arguments.Select(argument => Lower(argument, scope))]);

		return Specialize(rule, arguments, scope);
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
	Node Specialize(RuleSymbol rule, IReadOnlyList<Expr> arguments, GrammarScope scope)
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
				"another rule and there is no end to them — " +
				$"{_specializing[0]} … {_specializing[_specializing.Count - 1]}, and growing. " +
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

		foreach (var argument in arguments)
			if (argument is Expr.Number(var value))
			{
				passed.Add(null);
				counts.Add(value);
			}
			else if (argument is Expr.Reference(false, var named, _) &&
				_counts.TryGetValue(named, out var forwarded))
			{
				passed.Add(null);
				counts.Add(forwarded);
			}
			else
			{
				passed.Add(Lower(argument, scope));
				counts.Add(null);
			}

		// §4.2 gives a parameter's kind by what its declaration says: a C# type makes it a
		// value, anything else a recognizer. Only one value is built — a number, which a
		// quantifier count can be — and a parameter declared `pad: char` that is handed
		// anything else would silently become a recognizer instead. Refused rather than
		// taken as something the grammar did not say.
		for (var i = 0; i < declaration.Params.Count; i++)
			if (declaration.Params[i].Type is { } kind &&
				(kind.IsCSharp || IsCSharpKeyword(kind.Name)) &&
				counts[i] is null)
			{
				Report(
					UnbuiltCall,
					$"'{declaration.Params[i].Name}' is declared as the C# type " +
					$"'{TypeName(kind)}', which docs/syntax.md §4.2 makes a value, and the " +
					"only value a call may pass so far is a number. Pass a number, or drop the " +
					"type to make it a recognizer.",
					declaration.Params[i].At);

				return Node.Empty.Instance;
			}

		var key = rule.Name + "(" + string.Join(", ", passed.Select((node, i) =>
			node is null
				? counts[i]!.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
				: node.ToString())) + ")";

		if (_specialized.TryGetValue(key, out var made))
			return new Node.Call(made, []);

		var specialized = new RuleSymbol(NameFor(rule, passed, counts), rule.Scope, declaration);

		_specialized[key] = specialized;
		_rules.Add(specialized);

		// Before lowering, for the same reason an ordinary rule is: a specialization that
		// reaches itself would otherwise recurse here rather than be reported.
		_bodies[specialized] = Node.Empty.Instance;

		var outerArguments = _arguments;
		var outerCounts    = _counts;

		_arguments = new Dictionary<string, Node>(StringComparer.Ordinal);
		_counts    = new Dictionary<string, int>(StringComparer.Ordinal);

		for (var i = 0; i < arguments.Count; i++)
			if (passed[i] is { } node)
				_arguments[declaration.Params[i].Name] = node;
			else
				_counts[declaration.Params[i].Name] = counts[i]!.Value;

		_specializing.Add(specialized.Name);

		_bodies[specialized] = Lower(declaration.Body, rule.Scope);
		_arguments           = outerArguments;
		_counts              = outerCounts;

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
	string NameFor(RuleSymbol rule, IReadOnlyList<Node?> passed, IReadOnlyList<int?> counts)
	{
		var name = rule.Name;

		for (var i = 0; i < passed.Count; i++)
			name += "_" + (passed[i] switch
			{
				null                     => Text(counts[i]!.Value),
				Node.Call(var called, _) => called.Name.Replace(".", "_"),
				_                        => Text(_specialized.Count),
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
	/// §4.6: a literal that is all word characters may not be the start of a longer word.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The same shape as Trivia and for the same reason: an ordinary rule, shadowed to turn
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
	/// anything: `"select" &amp; ?!KeywordBoundary` asks whether a letter follows the
	/// keyword, while the other order would ask whether one follows the whitespace after
	/// it.
	/// </para>
	/// </remarks>
	Node Bounded(string value, GrammarScope scope)
	{
		var literal = new Node.Literal(value);

		if (value.Length == 0 || BoundaryFor(scope) is not { } boundary)
			return literal;

		foreach (var character in value)
			if (!Continues(boundary, character))
				return literal;

		return new Node.Sequence([literal, new Node.Lookahead(false, boundary)]);
	}

	/// <summary>Whether this character is one the boundary rule says continues a word.</summary>
	/// <remarks>
	/// Read out of the lowered rule rather than asked of it at run time: the class is an
	/// element set, which is exactly a set of characters, so membership is a question this
	/// side can answer while it still has the grammar in hand.
	/// </remarks>
	bool Continues(Node boundary, char character) =>
		boundary is Node.Call(var rule, _) &&
		_bodies.TryGetValue(rule, out var body) &&
		body is Node.Element(var negated, var ranges, var categories, _) &&
		categories.Count == 0 &&
		negated != ranges.Any(range => character >= range.From && character <= range.To);

	/// <summary>The `KeywordBoundary` this scope sees, or null while it matches nothing.</summary>
	Node? BoundaryFor(GrammarScope scope)
	{
		for (var at = scope; at is not null; at = at.Parent)
			if (at.Rules.TryGetValue("KeywordBoundary", out var rule) && !rule.IsBuiltIn)
				return MatchesNothing(rule, []) ? null : CallTo(rule, []);

		return null;
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
			? rule.Name is "none" or "Trivia" or "eof" or "KeywordBoundary"
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
}
