using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>
/// What each rule's value is, and how it is arrived at.
/// </summary>
/// <remarks>
/// <para>
/// Five ways, and the order between them is the language's rather than this file's: a
/// rule that wrote a <c>=&gt;</c> has said it, a sequence result collects (§4.1 case 2),
/// a rule typed as another takes that one's value (§4.1 case 3), and a declared type is
/// filled by its constructor or written into (§7.3, in that order).
/// </para>
/// <para>
/// What they have in common is the shape of the answer: every one of them ends as a
/// <see cref="Construction"/> on an alternative, so nothing downstream has to know which
/// way was taken — the machine numbers the captures, gives them up on a failed
/// alternative and rebuilds them at the accepting state, the same for all five.
/// </para>
/// </remarks>
sealed partial class GrammarNormalizer
{
	static IReadOnlyList<Node> ValueAlternatives(Node body)
	{
		return body is Node.Choice(var nodes) ? nodes : [body];
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

			if (type.IsHost || IsTypeKeyword(type.Name))
			{
				_types[rule] = TypeName(type);

				continue;
			}

			// §4.1 case 3: `A : B` says A's value is B's. Nothing here knows how to make
			// that true, and what happened before this was worse than not knowing — the
			// declaration was dropped and A got a type generated from its own captures, so
			// a rule said one thing and meant another with nothing to read about it.
			// A rule taking the parameter as its type is §4.2 rather than §4.1 case 3: each
			// specialization has one concrete argument, so there is an answer, and lowering
			// wrote down what to ask. The template itself is not in `_rules`.
			// §4.2: the parameter's type, which lowering wrote down per specialization. The
			// template itself is not in `_rules`, so nothing is reported about it here.
			if (rule.Declaration.Params.Any(one => one.Name == type.Name))
				continue;

			// A specialization has answered this already, against the rules it actually
			// calls rather than the ones its namespace names (§5.1). Looking the name up
			// again here would undo that.
			if (_produces.ContainsKey(rule))
				continue;

			// §4.1 case 3: `A : B` says A's value is B's. Recorded the same way and
			// resolved with the rest — a rule named in type position is a rule whose type
			// this one takes.
			if (rule.Namespace.LookupQualified(type.Name) is { } produced)
			{
				_produces[rule] = (produced, type.IsSequence);

				continue;
			}

			Report(
				UnbuiltRuleType,
				$"'{rule.Name}' declares its type as '{type.Name}', which is neither a {HostLanguage} type " +
				$"nor a rule in view. §4.1 case 3 takes the value of a rule named here; a {HostLanguage} type " +
				"is written with '@'.",
				type.At);
		}

		ResolveProduced();
	}

	/// <summary>
	/// The alternatives of a body, each built as the text or the bounds the rule matched: what the extent is, written as
	/// a construction, for a rule whose captures are read and so are kept.
	/// </summary>
	Node Extended(Node body, string expression)
	{
		var rewritten = new List<Node>();

		foreach (var alternative in ValueAlternatives(body))
			rewritten.Add(new Node.Construct(alternative, new Construction.Expression(expression)));

		return body is Node.Choice { Selection: not null } selected
			? selected.Rebuild(rewritten)
			: rewritten.Count == 1 ? rewritten[0] : new Node.Choice(rewritten);
	}

	/// <summary>
	/// The name of the type the grammar generates for a rule, where it will generate one: a rule that
	/// declares no type, captures, and builds nothing of its own (§4.1 case 3). Null for any other.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Asked where another rule takes this one's type, <c>Swapped : Pair</c>, before the results are
	/// worked out; <see cref="CheckNamedTypes"/> holds the answer to what they turned out to be. The
	/// type is spelled as the class it is nested in reads it, the rule's identifier, which is what
	/// <see cref="Emit.ResultTypes"/> calls it unless that name is taken, and a name the grammar
	/// writes may not be (GRAM5018).
	/// </para>
	/// <para>
	/// It used to be <c>string</c>, the text such a rule matched: a rule typed by it was handed a value
	/// of the generated type and declared to give back a string, and the generated file did not
	/// compile (CS0029).
	/// </para>
	/// </remarks>
	string? GeneratedTypeOf(RuleSymbol rule)
	{
		if (GeneratedNameOf(rule) is not { } name)
			return null;

		_namedTypes[name] = rule;

		return name;
	}

	/// <summary>The same, asked without the grammar's naming it.</summary>
	string? GeneratedNameOf(RuleSymbol rule)
	{
		// A rule that declared its extent generates no type for its names, whatever they turn out to be (DecideExtents).
		if (rule.Declaration is null || _types.ContainsKey(rule) || _produces.ContainsKey(rule) || _extents.ContainsKey(rule) ||
			!_bodies.TryGetValue(rule, out var body) || !HasCapture(body) || Constructs(body).Any())
			return null;

		return _language.GeneratedTypeName(rule);
	}

	/// <summary>
	/// The rules whose generated type the grammar names, by that name: a rule typed by one
	/// (<see cref="GeneratedTypeOf"/>) and a construction that builds one (<c>=&gt; Pair(a, b)</c>).
	/// </summary>
	readonly Dictionary<string, RuleSymbol> _namedTypes = new(StringComparer.Ordinal);

	/// <summary>Whether the grammar generates a type for this rule, made from its captures (§4.1 case 3).</summary>
	bool HasGeneratedType(RuleSymbol rule)
	{
		return !_types.ContainsKey(rule) && _results.TryGetValue(rule, out var members) && members.Count > 0;
	}

	/// <summary>
	/// What a capture holds, as the generated code declares it: a rule's declared type, the type the
	/// grammar generates for it, or text; and a sequence of that where it collects.
	/// </summary>
	string TypeOf(ResultMember member)
	{
		var type = member.Rule is not { } rule ? _language.Spelling(KnownType.Text)
			: _types.TryGetValue(rule, out var declared) ? declared
			: HasGeneratedType(rule) ? _language.GeneratedTypeName(rule)
			: _language.Spelling(KnownType.Text);

		return member.IsSequence ? _language.SequenceOf(type) : type;
	}

	/// <summary>
	/// Whether a value of one type can certainly not be put where the other is declared, as far as
	/// the grammar can tell without the host.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Only between types whose every conversion is known here: the types the grammar generates,
	/// which declare none; C#'s own types written by their keywords, which no grammar can shadow;
	/// and arrays of those. A conversion may be declared on either side, so a type of the host's on
	/// either side — a <c>Box</c> with an <c>implicit operator Box(Pair)</c>, a <c>String</c> of its own
	/// with one from <c>string[]</c> — is the C# compiler's to judge, on the grammar's line (§7.6).
	/// </para>
	/// <para>
	/// Between two of them the answer is no where one is text or a generated type and the other a
	/// different one, or one is an array and the other is not; never where both are C#'s numbers,
	/// which convert into one another, or the target is <c>object</c>.
	/// </para>
	/// </remarks>
	bool CannotHold(string to, string from)
	{
		to   = Plain(to);
		from = Plain(from);

		if (!Known(to) || !Known(from) || to == from || IsKeyword(to, KnownType.Object))
			return false;

		var toElement   = _language.ElementOf(to);
		var fromElement = _language.ElementOf(from);

		if (toElement is not null && fromElement is not null)
			return CannotHold(toElement, fromElement);

		if (toElement is null != fromElement is null)
			return true;

		// The bounds a grammar reads are the generator's own type too: nothing converts to or from them but themselves.
		if (Span(to) || Span(from))
			return !(Span(to) && Span(from));

		return IsKeyword(to, KnownType.Text) || IsKeyword(from, KnownType.Text) || Generated(to) || Generated(from);

		bool Span(string type)
		{
			return _language.Classify(type, out _) == KnownType.SourceSpan;
		}

		bool Known(string type)
		{
			return _language.ElementOf(type) is { } element
				? Known(element)
				: IsTypeKeyword(type) || Span(type) || Generated(type);
		}

		// Asked once the results are known, which is when the set is made; once, since a grammar of
		// several hundred rules asks for every construction that hands a capture back. Only of the
		// rules that stay: one nothing reaches is pruned and its type never written, and the name may
		// be a type of the host's, with conversions of its own.
		bool Generated(string type)
		{
			_generatedNames ??= [.. Kept().Where(HasGeneratedType).Select(_language.GeneratedTypeName)];

			return _generatedNames.Contains(type);
		}

		// `global::System.String` is `string` whatever the grammar imports; `String` or `System.String`
		// alone may be a type of the host's, and is left alone.
		string Plain(string type)
		{
			var kind = _language.Classify(type, out var form);

			return kind is KnownType.Text or KnownType.Object && form == TypeSpellingForm.Global ? _language.Spelling(kind) : type;
		}
	}

	/// <summary>The identifiers of the types the grammar generates (<see cref="CannotHold"/>).</summary>
	HashSet<string>? _generatedNames;

	/// <summary>
	/// The generated types the grammar names — a rule typed by another's, <c>Swapped : Pair</c>, and a
	/// construction <c>=&gt; Pair(a, b)</c> — held to what the rule they are made for turned out to be,
	/// and the values a construction builds one from held to its captures.
	/// </summary>
	void CheckNamedTypes()
	{
		foreach (var rule in _rules)
		{
			if (rule.Declaration?.Type is not { IsHost: false } written ||
				!_types.TryGetValue(rule, out var type) ||
				!_namedTypes.TryGetValue(_language.ElementOf(type) ?? type, out var named) ||
				HasGeneratedType(named))
				continue;

			Report(
				ConstructionTypeMismatch,
				$"'{rule.Name}' takes the type of '{named.Name}', and the grammar generates none for it: '{named.Name}' " +
				"builds its value with a '=>' of its own or captures nothing (§4.1).",
				written.At);
		}

		foreach (var (within, rule, call) in _built)
		{
			if (!HasGeneratedType(rule))
			{
				Report(
					ConstructionTypeMismatch,
					$"'{rule.Name}(…)' names a rule, and builds the type the grammar generates for it, which it " +
					$"generates for a rule that captures and says nothing of its value; '{rule.Name}' " +
					(_types.TryGetValue(rule, out var declared) ? $"declares its type, '{declared}'." : "captures nothing.") +
					$" A {HostLanguage} type or method is written with '@' (§2).",
					call.At);

				continue;
			}

			var members   = _results[rule];
			var arguments = call.Arguments;

			if (arguments.Count != members.Count)
			{
				Report(
					ConstructionTypeMismatch,
					$"'{rule.Name}(…)' is handed {arguments.Count} value{(arguments.Count == 1 ? "" : "s")}, and the " +
					$"type the grammar generates for '{rule.Name}' is built from its {members.Count} " +
					$"capture{(members.Count == 1 ? "" : "s")}, in the order the rule writes them: " +
					string.Join(", ", members.Select(static member => member.Name)) + ".",
					call.At);

				continue;
			}

			if (!_results.TryGetValue(within, out var own))
				continue;

			for (var i = 0; i < arguments.Count; i++)
			{
				if (arguments[i] is not Expr.Reference(false, var name, { Count: 0 }) ||
					own.FirstOrDefault(member => member.Name == name) is not { Element: null } held ||
					members[i].Element is not null)
					continue;

				var from = TypeOf(held);
				var to   = TypeOf(members[i]);

				if (CannotHold(to, from))
					Report(
						ConstructionTypeMismatch,
						$"'{name}' holds {from}, and '{members[i].Name}' of the type '{rule.Name}' is made of holds {to}.",
						arguments[i].At);
			}
		}
	}

	/// <summary>
	/// A construction that hands a capture back as the rule's value, held to the type the rule declares:
	/// <c>Items : @string[] = items: { Item }* =&gt; @(items)</c> hands back text where an array is
	/// declared. Said on the construction rather than by the C# compiler in the generated file.
	/// </summary>
	/// <remarks>
	/// Only what the grammar can tell without the host (<see cref="CannotHold"/>), and only a capture
	/// handed back whole, <c>=&gt; x</c> or <c>=&gt; @(x)</c>: what a C# expression makes of it is the
	/// C# compiler's question. A fold is left out, whose steps are handed the value so far.
	/// </remarks>
	void CheckConstructionTypes()
	{
		foreach (var rule in _rules)
		{
			// A clone or a specialization is left out: its declaration is checked as written, and what a
			// rebinding changes in it is told where the rebinding is (GRAM4014).
			if (rule.Declaration is null || rule.Name != rule.Declaration.Name || _folds.ContainsKey(rule) ||
				!_types.TryGetValue(rule, out var type) || !_results.TryGetValue(rule, out var members) ||
				!_bodies.TryGetValue(rule, out var body))
				continue;

			foreach (var alternative in ValueAlternatives(body))
			{
				if (alternative is not Node.Construct(_, Construction.Expression(var text, var at)) ||
					_language.SoleName(text, SoleNameRule.EveryBracketIdentifier) is not { } name ||
					members.FirstOrDefault(member => member.Name == name) is not { Element: null } held)
					continue;

				var from = TypeOf(held);

				if (CannotHold(type, from))
					Report(
						ConstructionTypeMismatch,
						$"'{rule.Name}' is declared {type} and hands back '{name}', which holds {from}. Capture what " +
						$"holds the declared type, or build it from the capture in {HostLanguage}.",
						at >= 0 ? new Location(at, name.Length) : rule.Declaration.At);
			}
		}
	}

	/// <summary>
	/// C# in a rule that names one of the rule's recognizer parameters, which stands for a piece of
	/// grammar and is no value C# could read: <c>Value(tag) = switch @(Kind(tag)) { … }</c>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A specialization calls the argument where the parameter is written as an operand, and puts
	/// nothing in C# for it, so the name went across as written and the C# compiler said it does not
	/// exist (CS0103), in a file the author did not write. What the author meant is a value: one is
	/// handed by a value parameter, written with its type, and is a literal or a value the caller was
	/// handed itself (§4.2) — a capture, which the parse produces, is none, and is passed by capturing
	/// the call and handing both to a <c>=&gt;</c>.
	/// </para>
	/// <para>
	/// Asked of the C# the rule's declaration writes, as written: what an argument brings in is the
	/// caller's, and may name a host member of the parameter's name (<c>Use('a' &amp; when @(x()))</c>).
	/// Said only where certain. A capture of the rule of the same name is the value C# reads; a name
	/// before a dot (<c>item.Length</c>) is asked of the host as any other is, where a namespace or a
	/// type of that name would answer. Said once for each parameter as it was written.
	/// </para>
	/// </remarks>
	void CheckRecognizersInHostCode()
	{
		if (!_language.ReadsFragments)
			return;

		var asked = new HashSet<Decl.Rule>(new DeclarationIdentity());

		foreach (var rule in _rules)
		{
			if (rule.Declaration is not { Params.Count: > 0 } declaration || !asked.Add(declaration))
				continue;

			var recognizers = declaration.Params
				.Where(parameter => parameter.Type is not { } type || !type.IsHost && !IsTypeKeyword(type.Name))
				.ToList();

			if (recognizers.Count == 0)
				continue;

			var captured = new HashSet<string>(
				Subexpressions(declaration.Body).OfType<Expr.Capture>().Select(static capture => capture.Name), StringComparer.Ordinal);

			var imports = Imports(_model.Root);

			foreach (var (named, at) in Named(declaration.Body, imports))
				foreach (var parameter in recognizers)
					if (named(parameter.Name) && !captured.Contains(parameter.Name) && Unbound(parameter.Name) &&
						_parametersInHostCode.Add(parameter))
						Report(
							RecognizerInHostCode,
							$"{HostLanguage} in '{declaration.Name}' names '{parameter.Name}', a parameter that stands for a piece of " +
							"grammar and holds no value. A value is handed by a parameter written with its type, " +
							$"'{parameter.Name}: @string', and is a literal or a value the caller was handed itself; " +
							"what the parse produces is captured where the rule is called and handed to a '=>' there " +
							"(docs/syntax.md §4.2).",
							at);
		}

		// Whether nothing in the host's C# answers to the name where the generated code stands: a member of
		// the host or of a class it derives from, a class around it or imported statically, a type or a
		// namespace. There the parameter's name is that member's, and the C# compiles (`static int x`,
		// a method handed on as `Check(x)`). Where no resolver can say — a grammar compiled with no host —
		// nothing is said: the name may be the host's, and a refusal there would refuse a grammar that
		// compiles; one that does not is told by the C# compiler on the grammar's line.
		bool Unbound(string name)
		{
			return _language.KnowsHostNames && !_language.HostAnswersTo(name, Imports(_model.Root));
		}

		// For each value the declaration writes, whether it certainly names a name, and where it stands.
		IEnumerable<(Func<string, bool> Names, Location At)> Named(Expr body, IReadOnlyList<string> imports)
		{
			foreach (var part in InValues(body))
				switch (part)
				{
					// A grammar name in a value is the rule's own name for something.
					case Expr.Reference(false, var name, _):
						yield return (one => one == name, part.At);
						break;

					// The C#'s names as the C# compiler binds them where the generated code stands, beside the
					// grammar's @using directives: one that binds to nothing there is the parameter's. Which
					// names a lambda, a local or a pattern declares, and over what, is the compiler's to say.
					case Expr.HostFragment(var text):
						var unbound = _language.NamesUnboundIn(text, imports);

						yield return (one => unbound?.Contains(one) == true, part.At);
						break;
				}
		}
	}

	/// <summary>
	/// Every part of every value a rule's body writes — a guard's, a construction's, a switch's selector,
	/// a mark's, a recovery's factory, a guard among a condition's tests, C# written as an operand.
	/// </summary>
	/// <remarks>What <see cref="CheckRecognizersInHostCode"/> asks about, and the generator asks the host about ahead of it.</remarks>
	internal static IEnumerable<Expr> InValues(Expr body)
	{
		foreach (var expression in Subexpressions(body))
		{
			IReadOnlyList<Expr> values = expression switch
			{
				Expr.Guard(var guarded)               => [guarded],
				Expr.Construct(_, var built)          => [built],
				Expr.Switch(var selector, _)          => [selector],
				Expr.Marked(_, var marked)            => [marked],
				Expr.Recovering(_, _, { } factory)    => [factory],
				Expr.Condition(var test)              => Parsing.Test.Guards(test),
				Expr.HostFragment                     => [expression],
				_                                     => [],
			};

			foreach (var value in values)
				foreach (var part in Subexpressions(value))
					yield return part;
		}
	}

	/// <summary>An expression and every expression inside it.</summary>
	static IEnumerable<Expr> Subexpressions(Expr root)
	{
		var pending = new Stack<Expr>();

		pending.Push(root);

		while (pending.Count > 0)
		{
			var current = pending.Pop();

			yield return current;

			foreach (var child in Dump.Children(current))
				pending.Push(child);
		}
	}

	/// <summary>Declarations by identity: a record compares its whole body, and clones share the one instance.</summary>
	sealed class DeclarationIdentity : IEqualityComparer<Decl.Rule>
	{
		public bool Equals(Decl.Rule? x, Decl.Rule? y)
		{
			return ReferenceEquals(x, y);
		}

		public int GetHashCode(Decl.Rule rule)
		{
			return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(rule);
		}
	}

	/// <summary>The parameters <see cref="CheckRecognizersInHostCode"/> has told of.</summary>
	readonly HashSet<Param> _parametersInHostCode = [];

	/// <summary>
	/// §4.2's <c>: item</c>: a specialization's result type is its argument's.
	/// </summary>
	/// <remarks>
	/// <para>
	/// To a fixpoint, because an argument may itself be a specialization of the same kind:
	/// <c>List(Pair(Number))</c> is a sequence of what <c>Pair(Number)</c> produces, which
	/// is what <c>Number</c> produces. Each round settles at least one, so the number of
	/// rounds is bounded by the number of specializations.
	/// </para>
	/// <para>
	/// A rule with no type of its own yields the text it matched, and <c>string</c> is what
	/// an extent is — so an argument that builds nothing gives <c>: item</c> the answer
	/// <c>string</c> rather than no answer at all.
	/// </para>
	/// </remarks>
	void ResolveProduced()
	{
		for (var settled = true; settled;)
		{
			settled = false;

			foreach (var pair in _produces)
			{
				if (_types.ContainsKey(pair.Key))
					continue;

				// Only once the argument has settled, or a rule that produces a value would
				// be read as producing its extent.
				if (!_types.TryGetValue(pair.Value.Produces, out var type))
				{
					if (_produces.ContainsKey(pair.Value.Produces))
						continue;

					type = GeneratedTypeOf(pair.Value.Produces) ?? _language.Spelling(KnownType.Text);
				}

				_types[pair.Key] = pair.Value.IsSequence ? _language.SequenceOf(type) : type;
				settled          = true;
			}
		}
	}

	/// <summary>
	/// Whether a rule's own text reads a capture by name where it is read as it goes: a guard, a mark, a switch's
	/// selector or a condition.
	/// </summary>
	/// <remarks>A name read there is read, whatever the rule's value is, and is not dropped.</remarks>
	static bool ReadsValues(Node body)
	{
		return NodeWalk.Descendants(body).Any(static node =>
			node is Node.Guard or Node.Marked or Node.Condition || node is Node.Choice { Selection: not null });
	}

	/// <summary>
	/// The node with the captures the author wrote of it taken off, each a capture of what it holds and no more.
	/// </summary>
	/// <remarks>
	/// For a rule whose value is implicit, where nothing the rule writes reads a name (§4.1): the rule is the one it
	/// would have been without them. A construction or a lookahead is left as it is.
	/// </remarks>
	Node Unnamed(Node node)
	{
		if (DeepStack.Low)
			return DeepStack.Run(() => Unnamed(node));

		switch (node)
		{
			case Node.Capture(_, var body):
				return Unnamed(body);

			case Node.Sequence(var parts):
				return new Node.Sequence([.. parts.Select(Unnamed)]);

			case Node.Choice(var alternatives):
				return ((Node.Choice)node).Rebuild([.. alternatives.Select(Unnamed)]);

			case Node.Repeat(var body, var min, var max):
			{
				var inner = Unnamed(body);

				return ReferenceEquals(inner, body) ? node : Repeated(node, new Node.Repeat(inner, min, max));
			}

			case Node.Counted(var body, var min, var max):
			{
				var inner = Unnamed(body);

				return ReferenceEquals(inner, body) ? node : Placed(node, new Node.Counted(inner, min, max));
			}

			case Node.Atomic(var body):
			{
				var inner = Unnamed(body);

				return ReferenceEquals(inner, body) ? node : new Node.Atomic(inner);
			}

			case Node.Marked(var body, var text):
			{
				var inner = Unnamed(body);

				return ReferenceEquals(inner, body) ? node : new Node.Marked(inner, text);
			}

			case Node.Bind bind:
			{
				var inner = Unnamed(bind.Body);

				return ReferenceEquals(inner, bind.Body) ? node : bind with { Body = inner };
			}

			default:
				return node;
		}
	}

	/// <summary>
	/// The names a rule's own text uses for a capture, a binding or a parameter: the ones a capture this compiler
	/// makes in the rule's place must not take.
	/// </summary>
	HashSet<string> ReservedNames(RuleSymbol rule)
	{
		var names = new HashSet<string>(StringComparer.Ordinal);

		if (rule.Declaration is { } declaration)
			foreach (var parameter in declaration.Params)
				names.Add(parameter.Name);

		if (_bodies.TryGetValue(rule, out var body))
			foreach (var node in NodeWalk.Descendants(body))
				switch (node)
				{
					case Node.Capture capture:
						names.Add(capture.Name);

						break;

					case Node.Bind bind:
						names.Add(bind.Binding.Name);

						break;
				}

		return names;
	}

	/// <summary>
	/// <paramref name="stem"/> if no name of the rule's own is that, and otherwise the stem and underscores until none is.
	/// Taken, so the next is not it either.
	/// </summary>
	static string Unreserved(string stem, HashSet<string> reserved)
	{
		var name = stem;

		while (!reserved.Add(name))
			name += "_";

		return name;
	}

	/// <summary>The rules told why nothing says their value, which <see cref="CheckConstruction"/> need not tell again.</summary>
	readonly HashSet<RuleSymbol> _unsaid = [];

	/// <summary>
	/// Whether a declared type is one of C#'s keyword types, by its keyword or by its name in
	/// <c>System</c>, nullable or not: what a resolver that cannot see the host still knows about.
	/// </summary>
	bool BuiltIn(string type)
	{
		return _language.ClassifyLoosely(_language.OptionalElementOf(type) ?? type) is >= KnownType.Text and <= KnownType.NUInt;
	}

	/// <summary>
	/// §7.3's second way: the properties these names can write, or null where the type
	/// cannot be built that way.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Every <c>required</c> property has to be covered — a type saying <c>required</c> is
	/// saying it will not compile otherwise — and at least one property has to be written,
	/// or this is not building a value so much as making an empty one.
	/// </para>
	/// <para>
	/// A constructor of no parameters is not asked for here. Whether one exists is C#'s
	/// question about the initializer this emits, and it will be asked on the grammar's
	/// own line (§7.6).
	/// </para>
	/// </remarks>
	IReadOnlyList<PropertyBinding>? Settable(string type, IReadOnlyList<string> names)
	{
		if (!TrySettable(type, out var properties))
			return null;

		var written = new List<PropertyBinding>();

		foreach (var property in properties)
		{
			var found = names.LastOrDefault(name => string.Equals(name, property.Name, StringComparison.OrdinalIgnoreCase));

			if (found is null)
			{
				if (property.IsRequired)
					return null;

				continue;
			}

			written.Add(new PropertyBinding(property.Name, found));
		}

		return written.Count > 0 ? written : null;
	}

	/// <summary>
	/// The names that fill this constructor, in its own order, or null where one of its
	/// parameters has nothing to fill it.
	/// </summary>
	static IReadOnlyList<string>? Covered(IReadOnlyList<MethodParameter> constructor, IReadOnlyList<string> names)
	{
		if (constructor.Count == 0)
			return null;

		var order = new List<string>(constructor.Count);

		foreach (var parameter in constructor)
		{
			var found = names.LastOrDefault(name => string.Equals(name, parameter.Name, StringComparison.OrdinalIgnoreCase));

			if (found is null)
				return null;

			order.Add(found);
		}

		return order;
	}

	/// <summary>
	/// §4.1's array: the values of the calls of its element type, in order, each made a capture of the alternative that
	/// reads it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Rewritten into what already works rather than given machinery of its own: each call that gives an element becomes an
	/// ordinary capture, and the alternative gets a <see cref="Construction.Sequence"/> of them — so the captures are numbered,
	/// given up and rebuilt by exactly the code that does it for a capture the author wrote, and the only new thing is what
	/// the factory's body says. Names change nothing: a call the author named is collected under his name, and what he named
	/// that gives no element is not collected.
	/// </para>
	/// <para>
	/// Groups are gone into and rule boundaries are not, except an untyped rule, which is read as if written in place, under
	/// a rule of the array's type made for it. A call of a rule of the array's own type gives its elements in place, which is
	/// how a left-recursive array is its seed and its steps.
	/// </para>
	/// </remarks>
	void CollectSequences()
	{
		for (var index = 0; index < _rules.Count; index++)
		{
			var rule = _rules[index];

			if (!_ruleValues.TryGetValue(rule, out var value) || value.Source != ValueSource.Collected)
				continue;

			var element   = _language.ElementOf(value.Type)!;
			var rewritten = new List<Node>();
			var joined    = new List<List<string>>();
			var reserved  = ReservedNames(rule);
			var taken     = 0;

			foreach (var alternative in ValueAlternatives(_bodies[rule]))
			{
				var gathered = new List<string>();
				var spreads  = 0;
				var body     = Gather(alternative, null);

				// One call of the array's own type, read once, is the whole value: handed back as it is.
				rewritten.Add(new Node.Construct(body, gathered.Count == 1 && spreads == 1 && Walk(body, part =>
					part is Node.Capture(var name, Node.Call) && name == gathered[0] ? new WalkStep(part, true) : null, out _) == (1, 1)
					? new Construction.Operand(gathered[0])
					: new Construction.Sequence(gathered)));

				Node Gather(Node node, List<string>? group)
				{
					Walk(node, part => Step(part, group), out var result);

					return result;
				}

				WalkStep? Step(Node part, List<string>? group)
				{
					switch (part)
					{
						case Node.Lookahead or Node.Guard or Node.Condition or Node.Construct:
							return new WalkStep(part, false);

						case Node.Capture(var name, Node.Call call) when Collects(call, element, value.Type, []):
							Take(name, group);

							if (Spreads(call.Rule, value.Type))
								spreads++;

							return new WalkStep(part, false);

						case Node.Call call when Collects(call, element, value.Type, []):
						{
							var name = Unreserved("item" + taken++, reserved);

							Take(name, group);

							if (Spreads(call.Rule, value.Type))
								spreads++;

							return new WalkStep(new Node.Capture(name, Fits(call, element) || Spreads(call.Rule, value.Type)
								? call
								: new Node.Call(ItemsRule(call.Rule, value.Type), call.Arguments)), false);
						}

						// A repetition that collects through more than one capture: each collects an array of its own, and
						// the order between them is kept by listing them together, for one member to gather all their slots
						// in the order they were read.
						case Node.Repeat(var body, var min, var max) when group is null && max != 1 && Sites(body, element) > 1:
						{
							var listed = new List<string>();

							joined.Add(listed);

							return new WalkStep(Repeated(part, new Node.Repeat(Gather(body, listed), min, max)), false);
						}

						case Node.Counted(var body, var min, var max) when group is null && max is not CountBound.Items(1) && Sites(body, element) > 1:
						{
							var listed = new List<string>();

							joined.Add(listed);

							return new WalkStep(Placed(part, new Node.Counted(Gather(body, listed), min, max)), false);
						}

						default:
							return null;
					}
				}

				void Take(string name, List<string>? group)
				{
					gathered.Add(name);
					group?.Add(name);
					_listed.Add((rule, name));
				}
			}

			if (joined.Count > 0)
				_joined[rule] = (element, joined);

			_bodies[rule] = Rebuilt(_bodies[rule], rewritten);
		}
	}

	/// <summary>
	/// The captures an array collects, by rule: one of text is the value of each turn it is read in, and is never made the
	/// text of the whole run (<see cref="HoistTextCaptures"/>).
	/// </summary>
	readonly HashSet<(RuleSymbol Rule, string Name)> _listed = [];

	/// <summary>The array rules made to read through an untyped rule, by the rule they read and the array's type.</summary>
	readonly Dictionary<(RuleSymbol, string), RuleSymbol> _itemsThrough = [];

	/// <summary>The rule of the array's type that reads an untyped rule in place, made once per rule and type.</summary>
	RuleSymbol ItemsRule(RuleSymbol read, string type)
	{
		if (!_itemsThrough.TryGetValue((read, type), out var made))
			_itemsThrough[(read, type)] = made = Synthetic(read, read.Name + "_Items", _bodies[read], type, ValueSource.Collected);

		return made;
	}

	/// <summary>
	/// The host asked about a type by the name the grammar wrote, then under each
	/// <c>@using</c> it declared.
	/// </summary>
	/// <remarks>
	/// The same search the binder does for a type's existence, and for the same reason: a
	/// grammar writes <c>: @Trade</c> beside an <c>@using</c> and expects to be understood,
	/// exactly as C# would. Asked only about the written name, §7.3 worked for a type in
	/// the global namespace and quietly did not for one in anybody's — which is every real
	/// project, and is how this was found.
	/// </remarks>
	bool TryConstructors(string type, out IReadOnlyList<IReadOnlyList<MethodParameter>> constructors)
	{
		// A type the grammar generates has one constructor, of the captures of the rule it is made
		// for, in the order the rule writes them; the host knows nothing of it yet.
		if (_namedTypes.TryGetValue(type, out var named) && _bodies.TryGetValue(named, out var body))
		{
			var members = _results.TryGetValue(named, out var made)
				? made.Select(static member => member.Name)
				: CaptureLayout.Of(body, BuildsValue, null).Slots.Select(static slot => slot.Name).Distinct(StringComparer.Ordinal);

			constructors = [[.. members.Select(static name => new MethodParameter(name, "", false))]];

			return true;
		}

		if (_language.TryWaysToBuild(type, out constructors))
			return true;

		foreach (var import in Imports(_model.Root))
			if (_language.TryWaysToBuild(import + "." + type, out constructors))
				return true;

		return false;
	}

	bool TrySettable(string type, out IReadOnlyList<ObjectMember> properties)
	{
		if (_language.TrySettableMembers(type, out properties))
			return true;

		foreach (var import in Imports(_model.Root))
			if (_language.TrySettableMembers(import + "." + type, out properties))
				return true;

		return false;
	}

	/// <summary>
	/// Whether two slots of one name hold the same thing.
	/// </summary>
	/// <remarks>
	/// The same rule holds the same thing trivially. Two rules do when both declared a
	/// type and the types are the same name — which is what makes a value of several
	/// shapes writable: every alternative of it produces the declared base, and the member
	/// that collects them has that one type. Declared, not assignable: a member has one
	/// type and it has to be a type both alternatives certainly produce, so widening one
	/// to the other's would be choosing for the author.
	/// </remarks>
	bool SameValue(CaptureSlot left, CaptureSlot right)
	{
		if (Equals(left.Rule, right.Rule))
			return true;

		return left.Rule is { } first &&
			right.Rule is { } second &&
			_types.TryGetValue(first, out var one) &&
			_types.TryGetValue(second, out var other) &&
			string.Equals(one, other, StringComparison.Ordinal);
	}

	/// <summary>
	/// A repetition that replaces another, carrying over what was recorded against it.
	/// </summary>
	/// <remarks>
	/// The node is new and <c>recover</c> was recorded against the old one. Everything
	/// downstream looks recovery up by node identity, so a repetition rewritten here would
	/// quietly stop recovering — which happened once, and is what
	/// <see cref="RecognitionGraph.Orphans"/> now catches. Where it was written goes the same
	/// way, for a diagnostic about the repetition.
	/// </remarks>
	Node.Repeat Repeated(Node old, Node.Repeat rewritten)
	{
		if (_recoveries.TryGetValue(old, out var recovery))
		{
			_recoveries.Remove(old);
			_recoveries[rewritten] = recovery;
		}

		Placed(old, rewritten);

		return rewritten;
	}

	/// <summary>
	/// Whether this call's value is an element of a sequence of <paramref name="element"/>.
	/// </summary>
	/// <remarks>
	/// A call of a rule whose type fits the element, a string that is its own text among them. A type the grammar generates
	/// is known to no host: a value of one fits a sequence of that type and of no other, and nothing but one fits a sequence
	/// of it.
	/// </remarks>
	bool Fits(Node part, string element)
	{
		if (part is not Node.Call(var called, _))
			return false;

		if (_namedTypes.ContainsKey(element))
			return ValueTypeOf(called) == element;

		return (_types.TryGetValue(called, out var type) || _extents.TryGetValue(called, out type)) && _language.Fits(type, element);
	}

	/// <summary>
	/// The strings that are their own text, by their type: the slice they matched is their value, so they are kept out of
	/// <c>_types</c>, which is what a caller is handed a built value of.
	/// </summary>
	readonly Dictionary<RuleSymbol, string> _extents = [];

	/// <summary>Whether a type is one a grammar may write by keyword alone, without <c>@</c> (§2).</summary>
	bool IsTypeKeyword(string name)
	{
		return _language.IsGrammarKeyword(name);
	}

	/// <summary>Whether a spelling is the language's keyword for <paramref name="kind"/>.</summary>
	bool IsKeyword(string spelling, KnownType kind)
	{
		return _language.Classify(spelling, out var form) == kind && form == TypeSpellingForm.Syntax;
	}

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
					// agree on what it holds, since a member has one type. Two different
					// rules agree when they declared the same type: `value: Object | value:
					// Array` with both `: @JsonValue` is one member of that type, and is how
					// a value that is one of several things gets written at all.
					//
					// What they must agree on is what the author sees, not how it is kept:
					// a slot under a fold's loop collects because a step is applied once per
					// iteration, and the step's `=>` is handed one of what it collected —
					// which is what lets a name written in a tail stand in the bases too,
					// where the same rule unfolded into several alternatives (§4.3).
					if (!SameValue(sharing[0], slot) || sharing[0].Collects != slot.Collects)
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

			_results[rule] = _joined.TryGetValue(rule, out var joined) ? Joined(members, joined.Element, joined.Groups) : members;
		}
	}

	/// <summary>
	/// The captures of each repetition that collects through more than one (<see cref="Gather"/>),
	/// made one member that gathers their slots in the order they were read.
	/// </summary>
	/// <remarks>
	/// The captures stay in the grammar as they are, so nothing about what is read changes;
	/// what changes is only that their values arrive as one sequence. Where the operands build
	/// values of different types the member holds the sequence's element, which each of them is.
	/// </remarks>
	List<ResultMember> Joined(List<ResultMember> members, string element, IReadOnlyList<List<string>> groups)
	{
		foreach (var group in groups)
		{
			var found = members.Where(member => group.Contains(member.Name) && member is { IsSequence: true, Rule: not null }).ToList();

			if (found.Count < 2)
				continue;

			var types  = found.Select(member => _types[member.Rule!]).Distinct(StringComparer.Ordinal).Count();
			var joined = found[0] with
			{
				Slots   = [.. found.SelectMany(static member => member.Slots).OrderBy(static slot => slot)],
				Joins   = [.. found.Skip(1).Select(static member => member.Name)],
				Element = types == 1 ? null : element,
			};

			members[members.IndexOf(found[0])] = joined;
			members.RemoveAll(member => found.Skip(1).Contains(member));
		}

		return members;
	}

	/// <summary>The captures <see cref="Gather"/> listed together, per rule, with the sequence's element.</summary>
	readonly Dictionary<RuleSymbol, (string Element, List<List<string>> Groups)> _joined = [];

	/// <summary>How many operands of this node would be collected into the sequence.</summary>
	/// <remarks>An inner repetition of one counts once: it collects in order by itself.</remarks>
	int Sites(Node node, string element)
	{
		if (Fits(node, element))
			return 1;

		return node switch
		{
			Node.Sequence(var parts)       => parts.Sum(part => Sites(part, element)),
			Node.Choice(var alternatives)  => alternatives.Sum(alternative => Sites(alternative, element)),
			Node.Repeat(var body, _, _)    => Sites(body, element),
			Node.Atomic(var body)          => Sites(body, element),
			Node.Marked(var body, _)       => Sites(body, element),
			Node.Bind(_, var body, _)      => Sites(body, element),
			Node.Counted(var body, _, _)   => Sites(body, element),
			Node.Capture(_, var body)      => Fits(body, element) ? 0 : Sites(body, element),
			_                              => 0,
		};
	}

	/// <remarks>
	/// What the author sees, as the comparison it explains does: a slot under a fold's loop is
	/// kept as a sequence and is one value to its step.
	/// </remarks>
	static string Held(CaptureSlot slot)
	{
		return slot.Rule is null
			? "text"
			: slot.Collects
				? $"a sequence of '{slot.Rule.Name}'"
				: $"the value of '{slot.Rule.Name}'";
	}

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
	bool BuildsValue(RuleSymbol rule)
	{
		return _types.ContainsKey(rule) || (_bodies.TryGetValue(rule, out var body) && HasCapture(body));
	}

	static bool HasCapture(Node node)
	{
		return node switch
		{
			Node.Capture => true,
			Node.Sequence(var nodes) => nodes.Any(HasCapture),
			Node.Choice(var nodes) => nodes.Any(HasCapture),
			Node.Repeat(var body, _, _) => HasCapture(body),
			Node.Atomic(var body) => HasCapture(body),
			Node.Marked(var body, _) => HasCapture(body),
			Node.Construct(var built, _) => HasCapture(built),

			// Not a capture itself — a binding makes no member — but what it holds may be.
			Node.Bind(_, var body, _) => HasCapture(body),
			Node.Counted(var body, _, _) => HasCapture(body),

			// Not across a call — that is another rule's result — and not into a lookahead,
			// which consumes nothing and is compiled with its captures stripped.
			_ => false,
		};
	}
}
