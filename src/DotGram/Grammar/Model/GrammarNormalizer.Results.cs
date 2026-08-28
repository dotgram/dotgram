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
public sealed partial class GrammarNormalizer
{
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
			// A rule taking the parameter as its type is §4.2 rather than §4.1 case 3: each
			// specialization has one concrete argument, so there is an answer, and lowering
			// wrote down what to ask. The template itself is not in `_rules`.
			// §4.2: the parameter's type, which lowering wrote down per specialization. The
			// template itself is not in `_rules`, so nothing is reported about it here.
			if (rule.Declaration.Params.Any(one => one.Name == type.Name))
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
				$"'{rule.Name}' declares its type as '{type.Name}', which is neither a C# type " +
				"nor a rule in view. §4.1 case 3 takes the value of a rule named here; a C# type " +
				"is written with '@'.",
				type.At);
		}

		ResolveProduced();
	}

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

					type = "string";
				}

				_types[pair.Key] = pair.Value.IsSequence ? type + "[]" : type;
				settled          = true;
			}
		}
	}


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
	/// <summary>
	/// §4.1 case 3: a rule whose type is another rule's takes that rule's value.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The same rewrite as a sequence result one size down: the operand that produces the
	/// value becomes a capture, and the alternative gets a <c>=&gt;</c> that hands it back.
	/// Everything about numbering, giving up and rebuilding is then the code that already
	/// does it.
	/// </para>
	/// <para>
	/// Exactly one operand may produce it. Two would be a rule with two answers and no way
	/// to say which, so it is left alone and reported as unbuilt — that is a grammar to
	/// rewrite rather than a choice for this compiler.
	/// </para>
	/// </remarks>
	/// <summary>
	/// §4.1 case 4's other half: a rule that builds nothing and captures nothing yields
	/// the extent it matched, and a rule may say out loud that it wants the bounds of
	/// that extent rather than its text.
	/// </summary>
	/// <remarks>
	/// <c>: @string</c> needs no construction at all — text is what an extent already is,
	/// and the type is recorded as absent so the machine goes on doing what it did. Bounds
	/// are a value somebody has to make, so this becomes an ordinary construction whose
	/// expression is the supplied name §8.2 already defines for exactly this
	/// (<c>parserSpan</c>). Nothing new reaches the emitter: a factory that names a
	/// supplied parameter gets it, which is the rule recovery has always been written by.
	/// </remarks>
	void ExtentValues()
	{
		foreach (var rule in _rules)
		{
			if (!_types.TryGetValue(rule, out var type) || !IsSourceSpan(type))
				continue;

			// However the grammar wrote it, the type the emitter names is the one nested in
			// the host class — there is no `DotGram.SourceSpan` to refer to any more.
			_types[rule] = "SourceSpan";

			var body = _bodies[rule];

			// A rule that captures or builds is making something of its parts; this is for
			// the one that made nothing and is asking where it was.
			if (HasCapture(body) || Constructs(body).FirstOrDefault() is not null)
				continue;

			var rewritten = new List<Node>();

			foreach (var alternative in Alternatives(body))
				rewritten.Add(new Node.Construct(alternative, new Construction.Expression("parserSpan")));

			_bodies[rule] = rewritten.Count == 1 ? rewritten[0] : new Node.Choice(rewritten);
		}
	}

	/// <summary>The one type §4.1 case 4 names besides <c>string</c>, however it is written.</summary>
	static bool IsSourceSpan(string type) =>
		type is "SourceSpan" or "DotGram.SourceSpan" or "global::DotGram.SourceSpan";

	void PassThrough()
	{
		foreach (var pair in _produces)
		{
			var rule = pair.Key;

			// A folded rule's body is the rewrite §4.3 made of it — the bases and the loop
			// over the steps — and not a choice of alternatives. Its own `=>` is what the
			// fold applies at each step, so there is nothing here to supply.
			if (_folds.ContainsKey(rule))
				continue;

			if (pair.Value.IsSequence || !_types.TryGetValue(rule, out var type))
				continue;

			var alternatives = Alternatives(_bodies[rule]);

			if (alternatives.Any(static alternative => alternative is Node.Construct))
				continue;

			var rewritten = new List<Node>(alternatives.Count);
			var taken     = 0;

			foreach (var alternative in alternatives)
			{
				var before = taken;
				var built  = Gather(alternative, type, ref taken);

				// One per alternative, and one only.
				if (taken != before + 1)
				{
					taken = -1;

					break;
				}

				rewritten.Add(new Node.Construct(built, Construction.Operand.Instance));
			}

			if (taken < 0)
				continue;

			_bodies[rule] = rewritten.Count == 1 ? rewritten[0] : new Node.Choice(rewritten);
		}
	}

	void BuildByConstructor()
	{
		foreach (var rule in _rules)
		{
			if (!_types.TryGetValue(rule, out var type) ||
				rule.Declaration?.Type is { IsSequence: true } ||
				!_results.TryGetValue(rule, out var members) ||
				members.Count == 0 ||

				// The same as the pass above: a folded rule's body is the rewrite and not
				// a choice, so the "already says how to build its value" guard below would
				// look at a sequence and see no construction where every alternative has
				// one. Left alone, the whole body was wrapped in a construction and the
				// fold's own machinery met a Construct where it laid out a Sequence.
				_folds.ContainsKey(rule))
			{
				continue;
			}

			var alternatives = Alternatives(_bodies[rule]);

			// A rule that says how to build its value has said it.
			if (alternatives.Any(static alternative => alternative is Node.Construct))
				continue;

			var chosen = (IReadOnlyList<string>?)null;
			var length = -1;
			var tied   = false;

			if (TryConstructors(type, out var constructors))
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

			// §7.3 in the order it lists them: a constructor first, and what it could not
			// answer is asked of the properties. A tie among constructors is left alone
			// rather than fallen through — the grammar did name a way to build this type
			// and it is the ambiguity that has to be reported, not a second way found.
			var written = tied || chosen is { Count: > 0 } ? null : Settable(type, members);


			if (tied || (chosen is null or { Count: 0 } && written is null))
				continue;

			var rewritten = new List<Node>(alternatives.Count);

			// The case carries what it needs, so nothing has to be looked up later by rule.
			Construction how = written is null
				? new Construction.Constructor(chosen!)
				: new Construction.Initializer(written);

			foreach (var alternative in alternatives)
				rewritten.Add(new Node.Construct(alternative, how));

			_bodies[rule] = rewritten.Count == 1 ? rewritten[0] : new Node.Choice(rewritten);
		}
	}

	/// <summary>
	/// §7.3's second way: the properties these captures can write, or null where the type
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
	IReadOnlyList<PropertyBinding>? Settable(string type, IReadOnlyList<ResultMember> members)
	{
		if (!TrySettable(type, out var properties))
			return null;

		var written = new List<PropertyBinding>();

		foreach (var property in properties)
		{
			var found = null as string;

			foreach (var member in members)
				if (string.Equals(member.Name, property.Name, StringComparison.OrdinalIgnoreCase))
					found = member.Name;

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

			// What the declaration says, unless it named a parameter: a specialization of
			// `List(item, sep) : item[]` has `item[]` written on it and `JsonMember[]`
			// worked out for it, and the host is asked about the second. Asking about the
			// first is asking whether a type called `item` exists, which is a question with
			// no true answer — and the permissive resolver says yes to it, so this held
			// together in tests and fell apart against a compilation.
			var element = _types.TryGetValue(rule, out var resolved) &&
				resolved.EndsWith("[]", StringComparison.Ordinal)
					? resolved.Substring(0, resolved.Length - "[]".Length)
					: declared.Name;

			var rewritten = new List<Node>();
			var taken     = 0;

			foreach (var alternative in Alternatives(_bodies[rule]))
				rewritten.Add(new Node.Construct(
					Gather(alternative, element, ref taken), Construction.Sequence.Instance));

			if (taken == 0)
			{
				// The near miss, told apart from the miss. An operand that would have joined
				// the sequence and is captured by hand is the likeliest way to write this
				// wrong, and "no operand of it produces one" is untrue of it — one does, and
				// it has a name already.
				Report(
					UnbuiltConstruction,
					Spoken(_bodies[rule], element) is { } name
						? $"'{rule.Name}' declares its result as a sequence of '{element}', and " +
							$"the operand that would fill it is captured as '{name}'. A sequence result " +
							"collects the operands nothing else has spoken for (§4.1 case 2), so drop " +
							"the name and let it be collected, or keep it and build the value with '=>'."
						: $"'{rule.Name}' declares its result as a sequence of '{element}', and no " +
							"operand of it produces one — every part either builds no value or builds a " +
							$"type that is not a '{element}'. §4.1 case 2 says which operands join " +
							"a sequence.",
					declared.At);

				continue;
			}

			_bodies[rule] = rewritten.Count == 1 ? rewritten[0] : new Node.Choice(rewritten);
		}
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
		if (_resolver.TryResolveConstructors(type, out constructors))
			return true;

		foreach (var import in Imports(_model.Root))
			if (_resolver.TryResolveConstructors(import + "." + type, out constructors))
				return true;

		return false;
	}

	bool TrySettable(string type, out IReadOnlyList<ObjectMember> properties)
	{
		if (_resolver.TryResolveSettableProperties(type, out properties))
			return true;

		foreach (var import in Imports(_model.Root))
			if (_resolver.TryResolveSettableProperties(import + "." + type, out properties))
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
	/// The name of a capture holding what the sequence was looking for, where there is one.
	/// </summary>
	string? Spoken(Node node, string element)
	{
		foreach (var inside in NodeWalk.Descendants(node))
			if (inside is Node.Capture(var name, var body) && Fits(body, element))
				return name;

		return null;
	}

	/// <summary>
	/// Every operand of this alternative whose value belongs in the sequence, turned into
	/// a capture — including the ones inside a group.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Groups are gone into and rule boundaries are not, which is the line §4.1 draws
	/// everywhere else: a group has no value of its own, so its operands are this rule's
	/// however they are bracketed, while what another rule is made of is that rule's
	/// business. The canonical list of §4.2 —
	/// <c>List(item, sep) : item[] = item &amp; (sep &amp; item)*</c> — therefore collects
	/// every element rather than the first, which is what the line says to a reader and is
	/// not what it did at first.
	/// </para>
	/// <para>
	/// Not into a lookahead, which consumes nothing and so contributes nothing; not into a
	/// capture the author wrote, which is a value with a name already; not into a
	/// <c>=&gt;</c>, which is another alternative's answer.
	/// </para>
	/// </remarks>
	Node Gather(Node node, string element, ref int taken)
	{
		if (Fits(node, element))
			return Collected(node, ref taken);

		switch (node)
		{
			case Node.Sequence(var parts):
			{
				var built = new List<Node>(parts.Count);

				foreach (var part in parts)
					built.Add(Gather(part, element, ref taken));

				return new Node.Sequence(built);
			}

			case Node.Choice(var alternatives):
			{
				var built = new List<Node>(alternatives.Count);

				foreach (var alternative in alternatives)
					built.Add(Gather(alternative, element, ref taken));

				return new Node.Choice(built);
			}

			case Node.Repeat(var body, var min, var max):
			{
				var inner = Gather(body, element, ref taken);

				return ReferenceEquals(inner, body)
					? node
					: Repeated(node, new Node.Repeat(inner, min, max));
			}

			case Node.Atomic(var body):
			{
				var inner = Gather(body, element, ref taken);

				return ReferenceEquals(inner, body) ? node : new Node.Atomic(inner);
			}

			default:
				return node;
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

		return Repeated(part, new Node.Repeat(new Node.Capture("item" + taken++, body), min, max));
	}

	/// <summary>
	/// A repetition that replaces another, carrying over what was recorded against it.
	/// </summary>
	/// <remarks>
	/// The node is new and <c>recover</c> was recorded against the old one. Everything
	/// downstream looks recovery up by node identity, so a repetition rewritten here would
	/// quietly stop recovering — which happened once, and is what
	/// <see cref="RecognitionGraph.Orphans"/> now catches.
	/// </remarks>
	Node.Repeat Repeated(Node old, Node.Repeat rewritten)
	{
		if (_recoveries.TryGetValue(old, out var recovery))
		{
			_recoveries.Remove(old);
			_recoveries[rewritten] = recovery;
		}

		return rewritten;
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
		Node.Capture                 => true,
		Node.Sequence(var nodes)     => nodes.Any(HasCapture),
		Node.Choice(var nodes)       => nodes.Any(HasCapture),
		Node.Repeat(var body, _, _)  => HasCapture(body),
		Node.Atomic(var body)        => HasCapture(body),
		Node.Construct(var built, _) => HasCapture(built),

		// Not across a call — that is another rule's result — and not into a lookahead,
		// which consumes nothing and is compiled with its captures stripped.
		_                            => false,
	};
}
