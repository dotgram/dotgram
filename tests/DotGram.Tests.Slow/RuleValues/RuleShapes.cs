using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGram.Tests.RuleValues;

/// <summary>
/// The enumeration of the gate: every combination of type, structure, operand kinds and names (the core); each other
/// dimension applied alone to one representative of a cell (the deviations); the rarer operand kinds on a cell each (the
/// atoms); and a seeded random tier of bodies nested two deep.
/// </summary>
static class RuleShapes
{
	/// <summary>The operand kinds of the core, in each slot.</summary>
	public static readonly Atom[] CoreAtoms = [Atom.Built, Atom.Implicit, Atom.Other, Atom.Plumbing];

	/// <summary>The operand kinds tried on one cell each, in the first slot.</summary>
	public static readonly Atom[] ExtraAtoms =
		[Atom.TakenExtent, Atom.TakenOverPlumbing, Atom.ReaderExtent, Atom.OwnOverMaking, Atom.TakenOverMaking, Atom.PerRoute, Atom.Character, Atom.Wrapper, Atom.TemplateCall, Atom.Self];

	/// <summary>
	/// The helper a slot of the given kind calls for a family, or null where the family has no such rule.
	/// </summary>
	public static Helper? AtomFor(Family family, Atom atom)
	{
		return (family, atom) switch
		{
			(_, Atom.Plumbing)                         => Helper.Plain,
			(Family.Text, Atom.Built)                  => Helper.Word,
			(Family.Text, Atom.Implicit)               => Helper.Bare,
			(Family.Text, Atom.Other)                  => Helper.Num,
			(Family.Text, Atom.TakenExtent)            => Helper.Mid,
			(Family.Text, Atom.TakenOverPlumbing)      => Helper.C4,
			(Family.Text, Atom.ReaderExtent)           => Helper.G,
			(Family.Text, Atom.OwnOverMaking)          => Helper.Ow,
			(Family.Text, Atom.TakenOverMaking)        => Helper.Mw,
			(Family.Text, Atom.PerRoute)               => Helper.Mx,
			(Family.Text, Atom.Character)              => Helper.Esc,
			(Family.Text, Atom.Wrapper)                => Helper.P2,
			(Family.Text, Atom.TemplateCall)           => Helper.Pw,
			(Family.Span, Atom.Built)                  => Helper.Sp,
			(Family.Span, Atom.Implicit)               => Helper.Span,
			(Family.Span, Atom.Other)                  => Helper.Num,
			(Family.Span, Atom.TakenExtent)            => Helper.Mids,
			(Family.Span, Atom.ReaderExtent)           => Helper.Gs,
			(Family.Span, Atom.OwnOverMaking)          => Helper.Ows,
			(Family.Span, Atom.TakenOverMaking)        => Helper.Mws,
			(Family.Box, Atom.Built)                   => Helper.Bxa,
			(Family.Box, Atom.Implicit)                => Helper.Bx,
			(Family.Box, Atom.Other)                   => Helper.Word,
			(Family.Record, Atom.Built)                => Helper.Lfa,
			(Family.Record, Atom.Implicit)             => Helper.Lf,
			(Family.Record, Atom.Other)                => Helper.Word,
			(Family.Generated, Atom.Implicit)          => Helper.Item,
			(Family.Generated, Atom.Other)             => Helper.Word,
			(Family.Number, Atom.Built)                => Helper.Num,
			(Family.Number, Atom.Other)                => Helper.Word,
			(Family.Object, Atom.Built)                => Helper.O,
			(Family.Object, Atom.Other)                => Helper.Word,
			_                                          => null,
		};
	}

	/// <summary>
	/// The calls whose names fill the family's constructor or initializer (§7.3), written in front of an alternative;
	/// null where the family has none.
	/// </summary>
	public static Expr[]? Filling(Family family)
	{
		return family switch
		{
			Family.Text      => [new Named("c", new Call(Helper.Ch, null)), new Named("count", new Call(Helper.Num, null, count: true))],
			Family.Span      => [new Named("start", new Call(Helper.Num, null, count: true)), new Named("length", new Call(Helper.Num, null, count: true))],
			Family.Box       => [new Named("name", new Call(Helper.Dg, null))],
			Family.Record    => [new Named("t", new Call(Helper.Dg, null))],
			Family.Generated => [new Named("x", new Call(Helper.Dg, null))],
			_                => null,
		};
	}

	static Expr[]? PartialFilling(Family family)
	{
		return family switch
		{
			Family.Text => [new Named("c", new Call(Helper.Ch, null))],
			Family.Span => [new Named("start", new Call(Helper.Num, null, count: true))],
			_           => null,
		};
	}

	static Expr[]? TieFilling(Family family)
	{
		return family switch
		{
			Family.Text => [new Named("value", new Call(Helper.Dg, null))],
			Family.Box  => [new Named("value", new Call(Helper.Num, null))],
			_           => null,
		};
	}

	/// <summary>The name of a constructor parameter or settable member of the family's own type.</summary>
	static string? SelfParameter(Family family)
	{
		return family switch
		{
			Family.Box    => "inner",
			Family.Record => "l",
			_             => null,
		};
	}

	/// <summary>The name of a constructor parameter or settable member a group's text can fill.</summary>
	static string? TextParameter(Family family)
	{
		return family switch
		{
			Family.Box       => "name",
			Family.Record    => "t",
			Family.Generated => "x",
			_                => null,
		};
	}

	static Literal L(string text)
	{
		return new Literal(text);
	}

	/// <summary>
	/// The structures: how many slots each has, and the body it makes of the slots' expressions.
	/// </summary>
	public static readonly IReadOnlyList<(string Name, int Slots, Func<Expr, Expr, Expr[]> Body)> Structures =
	[
		("none",                0, static (_, _) => [new Sequence(L("<"), L("#"), L(">"))]),
		("bare",                1, static (o, _) => [o]),
		("one",                 1, static (o, _) => [new Sequence(L("<"), o, L(">"))]),
		("two",                 2, static (o, p) => [new Sequence(L("<"), o, L(","), p, L(">"))]),
		("group-choice",        2, static (o, p) => [new Sequence(L("<"), new Choice(new Sequence(L("("), o, L(")")), p), L(">"))]),
		("alternatives",        2, static (o, p) => [new Sequence(L("<"), o, L(">")), new Sequence(L("!"), p)]),
		("alternative-without", 1, static (o, _) => [new Sequence(L("<"), o, L(">")), new Sequence(L("!"), L("#"))]),
		("optional",            1, static (o, _) => [new Sequence(L("<"), new Wrapped(Wrap.Optional, o), L(">"))]),
		("star",                1, static (o, _) => [new Sequence(L("<"), new Wrapped(Wrap.Star, new Sequence(L(","), o)), L(">"))]),
		("plus",                1, static (o, _) => [new Sequence(L("<"), new Wrapped(Wrap.Plus, o), L(">"))]),
		("zero",                1, static (o, _) => [new Sequence(L("<"), new Wrapped(Wrap.Zero, o), L("#"), L(">"))]),
		("ahead",               1, static (o, _) => [new Sequence(L("<"), new Wrapped(Wrap.Ahead, o), new AnyItems(Width(o)), L(">"))]),
		("not-ahead",           1, static (o, _) => [new Sequence(L("<"), new Wrapped(Wrap.NotAhead, o), L("#"), L(">"))]),
		("optional-tail",       2, static (o, p) => [new Sequence(L("<"), o, new Wrapped(Wrap.Optional, new Sequence(L(","), p)), L(">"))]),
	];

	/// <summary>
	/// The name of the structure that makes the rule under test call itself first, as written (<c>R = R &amp; X | X</c>),
	/// where the <c>leftRecursive</c> dimension appends a tail to the call (<c>R = R &amp; '+' &amp; '#' | ‹body›</c>). It
	/// is not one of <see cref="Structures"/>: the left-recursive cells are their own tier (<see cref="LeftRecursion"/>).
	/// </summary>
	public const string LeftRecursiveStructure = "left-recursive";

	static readonly (string Name, int Slots, Func<Expr, Expr, Expr[]> Body) LeftRecursiveBody =
		(LeftRecursiveStructure, 1, static (o, _) => [new Sequence(new SelfCall(), o), o]);

	static int Width(Expr slot)
	{
		return slot switch
		{
			Call call => call.Rule.Width,
			_         => 2,
		};
	}

	static Expr Slot(RuleType type, Atom atom)
	{
		if (atom == Atom.Self)
			return new SelfCall();

		return new Call(AtomFor(type.Family, atom)!, atom);
	}

	static bool Exists(RuleType type, Atom atom)
	{
		return atom == Atom.Self || AtomFor(type.Family, atom) is not null;
	}

	/// <summary>
	/// Builds a shape from its dimensions, or returns null where the combination does not exist (an operand kind the
	/// family has no rule for, names that need an operand or a literal the structure does not have, names the type's
	/// family has no constructor for, a caller that cannot call the rule).
	/// </summary>
	public static RuleShape? Make(
		string tier, RuleType type, string structure, Atom? first, Atom? second, NameKind names,
		ReaderKind reader = ReaderKind.None, bool leftRecursive = false, SeenFrom seenFrom = SeenFrom.Itself, bool template = false)
	{
		var (_, slots, body) = structure == LeftRecursiveStructure ? LeftRecursiveBody : Structures.First(s => s.Name == structure);

		if ((first is not null) != (slots >= 1) || (second is not null) != (slots >= 2))
			return null;

		if (first is { } a && !Exists(type, a) || second is { } b && !Exists(type, b))
			return null;

		// The rule under test calling itself is a choice's business: alone in a slot it never ends.
		if (second == Atom.Self || first == Atom.Self && structure is not ("group-choice" or "alternatives"))
			return null;

		Expr o = first is { } f ? Slot(type, f) : new Literal("?");
		Expr p = second is { } s ? Slot(type, s) : new Literal("?");

		var family = type.Family;

		switch (names)
		{
			case NameKind.OnOperand or NameKind.FillingAndOnOperand:
				if (slots == 0)
					return null;

				o = new Named("w", o);
				break;

			case NameKind.ParameterOnOperand:
				if (slots == 0 || SelfParameter(family) is not { } own)
					return null;

				o = new Named(own, o);
				break;

			case NameKind.OnGroup:
				if (slots == 0)
					return null;

				o = new Named("x", new Group(new Sequence(L("("), o, L(")"))));
				break;

			case NameKind.FillingOnGroup:
				if (slots == 0 || TextParameter(family) is not { } text)
					return null;

				o = new Named(text, new Group(new Sequence(L("("), o, L(")"))));
				break;
		}

		if (structure == "ahead" && names is NameKind.OnGroup or NameKind.FillingOnGroup)
			return null;

		var alternatives = body(o, p).ToList();

		Expr[]? prefix = names switch
		{
			NameKind.Filling or NameKind.FillingFirstAlternative or NameKind.FillingAndOnOperand => Filling(family),
			NameKind.Partial => PartialFilling(family),
			NameKind.Tie     => TieFilling(family),
			_                => [],
		};

		if (prefix is null)
			return null;

		if (names == NameKind.FillingFirstAlternative && alternatives.Count < 2)
			return null;

		if (prefix.Length > 0)
			for (var i = 0; i < alternatives.Count; i++)
				if (i == 0 || names != NameKind.FillingFirstAlternative)
					alternatives[i] = Prepend(prefix, alternatives[i]);

		if (names == NameKind.OnLiteral)
		{
			if (alternatives[0] is not Sequence { Items: [Literal first0, ..] } sequence)
				return null;

			alternatives[0] = new Sequence([new Named("x", first0), .. sequence.Items.Skip(1)]);
		}

		if (reader != ReaderKind.None)
		{
			var read = FirstName(alternatives[0]);

			Expr reading;

			switch (reader)
			{
				case ReaderKind.GuardReadsName when read is not null:
					reading = new Guard("Seen(" + read + ")", read);
					break;

				case ReaderKind.GuardReadsNone:
					reading = new Guard("true", null);
					break;

				case ReaderKind.Switch when read is not null:
					reading = new SwitchOn(read);
					break;

				default:
					return null;
			}

			alternatives[0] = Append(alternatives[0], reading);
		}

		if (leftRecursive)
			alternatives.Insert(0, new Sequence(new SelfCall(), L("+"), L("#")));

		if (seenFrom != SeenFrom.Itself)
		{
			if (type.How is TypeSaid.None or TypeSaid.Sequence || template)
				return null;

			if (seenFrom == SeenFrom.BesideFilling && Filling(family) is null)
				return null;

			if (seenFrom == SeenFrom.InSequence && AtomFor(family, Atom.Built) is null && AtomFor(family, Atom.Implicit) is null)
				return null;
		}

		if (template)
		{
			if (type.How != TypeSaid.Taken || leftRecursive)
				return null;

			var replaced = false;

			for (var i = 0; i < alternatives.Count; i++)
				alternatives[i] = ReplaceTaken(alternatives[i], type.Taken!, ref replaced);

			if (!replaced)
				return null;
		}

		var id = string.Join("/",
			tier,
			type.Name,
			structure,
			(first?.ToString() ?? "-") + "," + (second?.ToString() ?? "-"),
			names.ToString(),
			reader.ToString(),
			leftRecursive ? "LR" : "noLR",
			seenFrom.ToString(),
			template ? "template" : "written");

		return new RuleShape
		{
			Id            = id,
			Tier          = tier,
			Type          = type,
			Structure     = structure,
			First         = first,
			Second        = second,
			Names         = names,
			Reader        = reader,
			LeftRecursive = leftRecursive || structure == LeftRecursiveStructure,
			SeenFrom      = seenFrom,
			Template      = template,
			Alternatives  = alternatives,
		};
	}

	static Expr Prepend(Expr[] prefix, Expr alternative)
	{
		return alternative is Sequence sequence
			? new Sequence([.. prefix, .. sequence.Items])
			: new Sequence([.. prefix, alternative]);
	}

	static Expr Append(Expr alternative, Expr last)
	{
		return alternative is Sequence sequence
			? new Sequence([.. sequence.Items, last])
			: new Sequence(alternative, last);
	}

	/// <summary>The first name written in an expression, outside lookaheads.</summary>
	public static string? FirstName(Expr expr)
	{
		switch (expr)
		{
			case Named named:
				return named.Name;

			case Sequence sequence:
				return sequence.Items.Select(FirstName).FirstOrDefault(static n => n is not null);

			case Choice choice:
				return choice.Alternatives.Select(FirstName).FirstOrDefault(static n => n is not null);

			case Group group:
				return FirstName(group.Inner);

			case Wrapped { Wrap: not (Wrap.Ahead or Wrap.NotAhead) } wrapped:
				return FirstName(wrapped.Inner);

			default:
				return null;
		}
	}

	static Expr ReplaceTaken(Expr expr, Helper taken, ref bool replaced)
	{
		switch (expr)
		{
			case Call call when call.Rule == taken && call.Atom is not null:
				replaced = true;
				return new Parameter();

			case Named named:
				return new Named(named.Name, ReplaceTaken(named.Inner, taken, ref replaced));

			case Sequence sequence:
			{
				var items = new Expr[sequence.Items.Length];

				for (var i = 0; i < items.Length; i++)
					items[i] = ReplaceTaken(sequence.Items[i], taken, ref replaced);

				return new Sequence(items);
			}

			case Choice choice:
			{
				var alternatives = new Expr[choice.Alternatives.Length];

				for (var i = 0; i < alternatives.Length; i++)
					alternatives[i] = ReplaceTaken(choice.Alternatives[i], taken, ref replaced);

				return new Choice(alternatives);
			}

			case Group group:
				return new Group(ReplaceTaken(group.Inner, taken, ref replaced));

			case Wrapped wrapped:
				return new Wrapped(wrapped.Wrap, ReplaceTaken(wrapped.Inner, taken, ref replaced));

			default:
				return expr;
		}
	}

	/// <summary>
	/// The names a structure with two slots is tried with: those about the operands as a whole. What a name does to the
	/// first operand alone (a parameter or a group on it, half the filling names, a tie) is the business of the structures
	/// with one slot.
	/// </summary>
	static readonly NameKind[] TwoSlotNames =
		[NameKind.None, NameKind.OnLiteral, NameKind.Filling, NameKind.FillingFirstAlternative, NameKind.OnOperand, NameKind.FillingAndOnOperand];

	/// <summary>
	/// Type × structure × operand kinds × names, with nothing else applied. In full for the structures with one slot; with
	/// two, the second slot holds an operand of the family or of another type (an untyped rule there reads as the first
	/// slot's next to nothing, which the structures with one slot have), never both slots another type, and the names of
	/// <see cref="TwoSlotNames"/>.
	/// </summary>
	public static IEnumerable<RuleShape> Core()
	{
		foreach (var type in RuleType.All)
			foreach (var (structure, slots, _) in Structures)
				foreach (var first in slots >= 1 ? CoreAtoms.Cast<Atom?>() : [null])
					foreach (var second in slots >= 2 ? CoreAtoms.Where(static a => a != Atom.Plumbing).Cast<Atom?>() : [null])
						foreach (var names in slots >= 2 ? TwoSlotNames : Enum.GetValues<NameKind>())
						{
							if (first is Atom.Other or Atom.Plumbing && second == Atom.Other)
								continue;

							if (Make("core", type, structure, first, second, names) is { } shape)
								yield return shape;
						}
	}

	/// <summary>
	/// The representative operands of a cell: the built rule of the family in the first slot (its implicit one where it
	/// has none) and the implicit one in the second (the other type where it has none).
	/// </summary>
	static (Atom? First, Atom? Second) Representative(RuleType type, int slots)
	{
		var first  = AtomFor(type.Family, Atom.Built) is not null ? Atom.Built : Atom.Implicit;
		var second = AtomFor(type.Family, Atom.Implicit) is not null ? Atom.Implicit : Atom.Other;

		return (slots >= 1 ? first : null, slots >= 2 ? second : null);
	}

	/// <summary>
	/// For each type and structure, with no names and with the filling names: each of reader, left recursion, caller and
	/// template applied alone. The template takes operands that are the taken rule.
	/// </summary>
	public static IEnumerable<RuleShape> Deviations()
	{
		foreach (var type in RuleType.All)
			foreach (var (structure, slots, _) in Structures)
				foreach (var names in new[] { NameKind.None, NameKind.Filling })
				{
					var (first, second) = Representative(type, slots);

					foreach (var reader in new[] { ReaderKind.GuardReadsName, ReaderKind.GuardReadsNone, ReaderKind.Switch })
						if (Make("deviation", type, structure, first, second, names, reader: reader) is { } shape)
							yield return shape;

					if (Make("deviation", type, structure, first, second, names, leftRecursive: true) is { } recursive)
						yield return recursive;

					foreach (var seen in new[] { SeenFrom.InSequence, SeenFrom.TakenBy, SeenFrom.BesideFilling })
						if (Make("deviation", type, structure, first, second, names, seenFrom: seen) is { } called)
							yield return called;

					if (type.Taken is { } taken && CoreAtoms.FirstOrDefault(a => AtomFor(type.Family, a) == taken) is var own && AtomFor(type.Family, own) == taken)
						if (Make("deviation", type, structure, slots >= 1 ? own : null, slots >= 2 ? own : null, names, template: true) is { } template)
							yield return template;
				}
	}

	/// <summary>
	/// The rarer operand kinds, in the first slot of each type and structure, with no names and with the filling names.
	/// </summary>
	public static IEnumerable<RuleShape> Atoms()
	{
		foreach (var type in RuleType.All)
			foreach (var (structure, slots, _) in Structures)
				foreach (var names in new[] { NameKind.None, NameKind.Filling })
					foreach (var first in ExtraAtoms)
					{
						var (_, second) = Representative(type, slots);

						if (slots >= 1 && Make("atoms", type, structure, first, second, names) is { } shape)
							yield return shape;
					}
	}

	/// <summary>
	/// The rule under test calling itself first, with no tail: <c>Start : T = Start &amp; X | X</c>. For each type of a text
	/// or span family that is said by its own type or by a taken rule (a rule that makes its value and one that is its own
	/// text), and each kind of operand <c>X</c>: a rule that is text, one that makes, one that is text over a making rule,
	/// one that takes a making rule's type, one with a text route and a building route.
	/// </summary>
	public static IEnumerable<RuleShape> LeftRecursion()
	{
		var types = new[] { "@string", ":Word", ":Bare", "@SourceSpan", ":Sp", ":Span" };
		var atoms = new[] { Atom.Built, Atom.Implicit, Atom.TakenExtent, Atom.TakenOverPlumbing, Atom.ReaderExtent, Atom.OwnOverMaking, Atom.TakenOverMaking, Atom.PerRoute };

		foreach (var type in types)
			foreach (var atom in atoms)
				if (Make("leftrec", RuleType.Named(type), LeftRecursiveStructure, atom, null, NameKind.None) is { } shape)
					yield return shape;
	}

	static RuleShape Direct(string label, string type, params Expr[] alternatives)
	{
		return Direct(label, RuleType.Named(type), alternatives);
	}

	static RuleShape Direct(string label, RuleType type, params Expr[] alternatives)
	{
		return new RuleShape
		{
			Id           = "fixed/" + label,
			Tier         = "fixed",
			Type         = type,
			Structure    = "fixed",
			Names        = NameKind.None,
			LeftRecursive = alternatives.Any(static a => a is SelfCall or Sequence { Items: [SelfCall, ..] }),
			Alternatives = alternatives,
		};
	}

	/// <summary>A grammar that declares trivia, with or without <c>Lexical</c>.</summary>
	static RuleShape Spaced(string label, string type, bool lexical, params Expr[] alternatives)
	{
		return new RuleShape
		{
			Id           = "fixed/" + label,
			Tier         = "fixed",
			Type         = RuleType.Named(type),
			Structure    = "fixed",
			Names        = NameKind.None,
			Trivia       = true,
			Lexical      = lexical,
			Alternatives = alternatives,
		};
	}

	static Call C(Helper rule, Atom atom = Atom.Implicit)
	{
		return new Call(rule, atom);
	}

	/// <summary>
	/// The grammars the differences from the published version are measured on: each is written out, not enumerated, and
	/// run in every enumeration, so that each class of difference has a cell that stands for it.
	/// </summary>
	public static IReadOnlyList<RuleShape> Fixed()
	{
		var filling = Filling(Family.Text)!;

		return
		[
			// Names fill a construction and one operand is read on every route.
			Make("fixed", RuleType.Named("@Box"), "bare", Atom.Implicit, null, NameKind.Filling)!,
			Direct("box, names, an operand in one of two routes each", ":Bx",
				new Sequence(new Named("name", new Call(Helper.Dg, null)), new Choice(new Sequence(L("<"), C(Helper.Bx), L(">")), C(Helper.Bx)))),
			Direct("box, a filling name on a group that holds the operand", ":Bx",
				new Named("name", new Group(new Sequence(L("<"), C(Helper.Bx), L(">"))))),
			Direct("text, filling names, an operand in one of two routes each", ":Word",
				new Sequence([.. filling, new Choice(new Sequence(L("<"), C(Helper.Word, Atom.Built), L(">")), C(Helper.Word, Atom.Built))])),

			// A taken span over a span that makes its value, and two of them.
			Make("fixed", RuleType.Named(":Sp"), "one", Atom.Built, null, NameKind.None)!,
			Direct("taken span, two built spans", ":Sp", new Sequence(C(Helper.Sp, Atom.Built), L(" "), C(Helper.Sp, Atom.Built))),

			// An array of spans, one of them an extent.
			Direct("span array, two extent spans", "@SourceSpan[]", new Sequence(C(Helper.Span), L(";"), C(Helper.Span))),
			Direct("span array, an extent span and a built one", "@SourceSpan[]", new Sequence(C(Helper.Span), L(";"), C(Helper.Sp, Atom.Built))),

			// Names in an array.
			Direct("text array, a name on the literal between two built operands", "@string[]",
				new Sequence(C(Helper.Word, Atom.Built), new Named("x", L(";")), C(Helper.Word, Atom.Built))),
			Direct("text array, a name on the first of two built operands", "@string[]",
				new Sequence(new Named("w", C(Helper.Word, Atom.Built)), L(";"), C(Helper.Word, Atom.Built))),

			// A capture named like the one the compiler writes.
			Direct("taken text, a name that the compiler uses on the last literal", ":Word",
				new Sequence(L("<"), C(Helper.Word, Atom.Built), new Named("items", L(">")))),

			// An optional operand, with and without names that fill a construction.
			Make("fixed", RuleType.Named(":Word"), "optional", Atom.Built, null, NameKind.None)!,
			Direct("box, filling name, an optional operand", ":Bx", new Sequence(new Named("name", new Call(Helper.Dg, null)), new Wrapped(Wrap.Optional, C(Helper.Bx)))),
			Direct("text, filling names, an operand in one route and an extent in the other", ":Word",
				new Sequence([.. filling, new Choice(new Sequence(L("<"), C(Helper.Word, Atom.Built), L(">")), C(Helper.Bare))])),
			Make("fixed", RuleType.Named(":Item"), "optional", Atom.Implicit, null, NameKind.None)!,

			// A text rule with a route that builds a value and one that does not, as a rule of its own.
			Direct("taken text, a built route and a text route", ":Word", C(Helper.Word, Atom.Built), new Sequence(L("["), C(Helper.Bare), L("]"))),

			// A text extent over bytes with a name that a guard reads.
			Make("fixed", RuleType.Named("@string"), "one", Atom.ReaderExtent, null, NameKind.None)!,

			// A string rule over a building rule, and the same taken from one: the pieces are joined, own literals included.
			Direct("string, a built string between literals", "@string", new Sequence(L("<"), C(Helper.Word, Atom.Built), L(">"))),
			Direct("string, a built string in a recursive rule", "@string",
				new Sequence(L("("), new SelfCall(), L(")")), C(Helper.Word, Atom.Built)),
			Direct("taken text, a built operand between literals", ":Word", new Sequence(L("<"), C(Helper.Word, Atom.Built), L(">"))),
			Direct("taken text, a built operand and a literal", ":Word", new Sequence(C(Helper.Word, Atom.Built), L(";"))),
			Direct("string, an optional literal and a text rule", "@string", new Sequence(new Wrapped(Wrap.Optional, L("-")), C(Helper.Bare))),
			Direct("taken text, an optional built operand between literals", ":Word",
				new Sequence(L("<"), new Wrapped(Wrap.Optional, C(Helper.Word, Atom.Built)), L(">"))),

			// A piece of a type that is not text, in a string rule.
			Direct("string, an int piece", "@string", new Sequence(L("<"), C(Helper.Num, Atom.Other), L(">"))),
			Direct("string, a span piece", "@string", new Sequence(L("<"), C(Helper.Span, Atom.Other), L(">"))),
			Direct("string, a built span piece", "@string", new Sequence(L("<"), C(Helper.Sp, Atom.Other), L(">"))),
			Direct("string, a generated type piece", "@string", new Sequence(L("<"), C(Helper.Item, Atom.Other), L(">"))),
			Direct("string, a host type piece", "@string", new Sequence(L("<"), C(Helper.Bx, Atom.Other), L(">"))),
			Direct("string, a taken span piece", "@string", new Sequence(L("<"), C(Helper.Mids, Atom.Other), L(">"))),
			Direct("string, names that fill a constructor", "@string",
				new Sequence(new Named("c", new Call(Helper.Ch, null)), new Named("count", new Call(Helper.Num, null, count: true)))),
			Direct("string, one name of the constructor", "@string", new Sequence(new Named("c", new Call(Helper.Ch, null)), L("<"))),

			// A rule of type char.
			Direct("string, a char rule between quotes", "@string", new Sequence(L("\""), C(Helper.Esc, Atom.Character), L("\""))),
			Direct("string, a repetition of a char rule or an untyped rule", "@string",
				new Sequence(L("\""), new Wrapped(Wrap.Star, new Choice(C(Helper.Esc, Atom.Character), C(Helper.Plain, Atom.Plumbing))), L("\""))),

			// An untyped rule and an untyped template, read through.
			Direct("string, an untyped wrapper of a built string", "@string", new Sequence(L("<"), C(Helper.P2, Atom.Wrapper), L(">"))),
			Direct("string, a template call over a built string", "@string", new Sequence(L("<"), C(Helper.Pw, Atom.TemplateCall), L(">"))),
			Direct("string array, an untyped wrapper of a built string", "@string[]", new Sequence(C(Helper.P2, Atom.Wrapper), L(";"), C(Helper.Word, Atom.Built))),

			// A binding is a call, and its reference matches the text again.
			Direct("string, a binding and its reference", "@string", new Sequence(new Bind("q", C(Helper.Word, Atom.Built)), L(","), new BackRef("q"))),
			Direct("taken text, a binding and its reference", ":Word", new Sequence(new Bind("q", C(Helper.Word, Atom.Built)), L(","), new BackRef("q"))),

			// Arrays: a text rule is collected, a literal never.
			Direct("text array, a text rule, then a repetition of text rules", "@string[]",
				new Sequence(C(Helper.Bare), new Wrapped(Wrap.Star, new Sequence(L(","), C(Helper.Bare))))),
			Direct("text array, a text rule and a built one", "@string[]", new Sequence(C(Helper.Bare), L(","), C(Helper.Word, Atom.Built))),
			Direct("text array, a text rule and an array of a built one", "@string[]",
				new Sequence(L("<"), C(Helper.Bare), L(","), C(Helper.Ws, Atom.Built), L(">"))),
			Direct("text array, a text rule and a template array of a built one", "@string[]",
				new Sequence(L("<"), C(Helper.Bare), L(","), C(Helper.Lw, Atom.Built), L(">"))),

			// A host or generated type: a name that the compiler uses.
			Direct("box, a name that the compiler uses on the last literal", ":Bx", new Sequence(L("<"), C(Helper.Bx), new Named("items", L(">")))),

			// Trivia, woven between the operands, and a grammar read over tokens.
			Spaced("trivia, a string rule over two text rules", "@string", false, new Sequence(C(Helper.Bare), L("."), C(Helper.Bare))),
			Spaced("trivia, a string rule over two built strings", "@string", false, new Sequence(C(Helper.Word, Atom.Built), L("."), C(Helper.Word, Atom.Built))),
			Spaced("trivia, a string rule of one token", "@string", false, C(Helper.Bare)),
			Spaced("trivia, an array over two built strings", "@string[]", false, new Sequence(C(Helper.Word, Atom.Built), L(";"), C(Helper.Word, Atom.Built))),
			Spaced("kinds, a string rule over two text rules", "@string", true, new Sequence(C(Helper.Bare), L("."), C(Helper.Bare))),
			Spaced("kinds, a string rule over two built strings", "@string", true, new Sequence(C(Helper.Word, Atom.Built), L("."), C(Helper.Word, Atom.Built))),
			Spaced("kinds, a taken text rule over a built string", ":Word", true, new Sequence(L("<"), C(Helper.Word, Atom.Built), L(">"))),
			new RuleShape
			{
				Id           = "fixed/trivia, a template that starts with trivia over a built string",
				Tier         = "fixed",
				Type         = RuleType.Named(":Word"),
				Structure    = "fixed",
				Names        = NameKind.None,
				Trivia       = true,
				Template     = true,
				Alternatives = [new Sequence(new TriviaRef(), new Parameter())],
			},

			// Left recursion read as a seed and steps: a span, an array, a string and a type with a constructing seed.
			Direct("left recursion, span", "@SourceSpan", C(Helper.Num, Atom.Other), new Sequence(new SelfCall(), L("+"), C(Helper.Num, Atom.Other))),
			Direct("left recursion, array", "@string[]", C(Helper.Word, Atom.Built), new Sequence(new SelfCall(), L(","), C(Helper.Word, Atom.Built))),
			Direct("right recursion, array", "@string[]",
				new Sequence(C(Helper.Word, Atom.Built), new Wrapped(Wrap.Optional, new Sequence(L(","), new SelfCall())))),
			Direct("left recursion, taken text", ":Word", C(Helper.Word, Atom.Built), new Sequence(new SelfCall(), L("+"), C(Helper.Word, Atom.Built))),
			Direct("left recursion, own text over text rules", "@string", C(Helper.Bare), new Sequence(new SelfCall(), L("+"), C(Helper.Bare))),
			Direct("left recursion, a type with a constructing seed", RuleType.PairTaken,
				new Sequence(new SelfCall(), L("+"), C(Helper.Num, Atom.Other)),
				new Sequence(new Named("a", new Call(Helper.Num, null)), L("="), new Named("b", new Call(Helper.Num, null)))),
		];
	}

	/// <summary>The whole bounded enumeration: core, deviations, atoms and left recursion, each shape once.</summary>
	public static IReadOnlyList<RuleShape> Bounded()
	{
		return [.. Core().Concat(Deviations()).Concat(Atoms()).Concat(LeftRecursion())];
	}

	/// <summary>
	/// The helpers a shape reads as operands (outside the calls that only fill a name), each once, in order; the template's
	/// argument for the parameter of a template. The rule under test calling itself is no helper and is left out.
	/// </summary>
	public static IReadOnlyList<Helper> Operands(RuleShape shape)
	{
		var found = new List<Helper>();

		void Visit(Expr expr)
		{
			switch (expr)
			{
				case Call { Atom: not null } call:
					if (!found.Contains(call.Rule))
						found.Add(call.Rule);

					break;

				case Parameter when shape.TemplateArgument is { } argument:
					if (!found.Contains(argument))
						found.Add(argument);

					break;

				case Named named:
					Visit(named.Inner);
					break;

				case Bind bind:
					Visit(bind.Inner);
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

		foreach (var alternative in shape.Alternatives)
			Visit(alternative);

		return found;
	}

	/// <summary>
	/// The shape with the bodies of its text rules written in place of their calls (every operand call of a rule that
	/// declares a text value without building it, <see cref="Helper.Inline"/>), or null where it calls none. The shape and
	/// its extraction read the same input; whether they must agree is the oracle's question.
	/// </summary>
	public static RuleShape? Inlined(RuleShape shape)
	{
		// A grammar read over tokens keeps its terminals in a namespace of their own: written in place they are not terminals.
		if (shape.Lexical)
			return null;

		var count = 0;

		Expr Replace(Expr expr)
		{
			switch (expr)
			{
				case Call { Atom: not null, Rule.Inline: { } inline }:
					return inline(count++);

				case Named named:
					return new Named(named.Name, Replace(named.Inner));

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

		var alternatives = shape.Alternatives.Select(Replace).ToList();

		if (count == 0)
			return null;

		return new RuleShape
		{
			Id            = "inlined/" + shape.Id,
			Tier          = "inlined",
			InlinedFrom   = shape,
			Type          = shape.Type,
			Structure     = shape.Structure,
			First         = shape.First,
			Second        = shape.Second,
			Names         = shape.Names,
			Reader        = shape.Reader,
			LeftRecursive = shape.LeftRecursive,
			SeenFrom      = shape.SeenFrom,
			Template      = shape.Template,
			Trivia        = shape.Trivia,
			Lexical       = shape.Lexical,
			Alternatives  = alternatives,
		};
	}

	/// <summary>
	/// Random bodies nested two deep over the same types, operand kinds and names, from a seed: what the bounded set does
	/// not write. A shape found there is reproduced by its id (the seed and index) and belongs in the enumeration.
	/// </summary>
	public static IEnumerable<RuleShape> Random(int seed, int count)
	{
		var random = new System.Random(seed);

		for (var index = 0; index < count; index++)
		{
			var type         = RuleType.All[random.Next(RuleType.All.Count)];
			var alternatives = new List<Expr>();
			var count0       = random.Next(1, 3);

			for (var a = 0; a < count0; a++)
			{
				Expr body = new Sequence(L(a == 0 ? "<" : "!"), Node(random, type, 2), a == 0 ? L(">") : L("#"));

				if (random.Next(4) == 0 && Filling(type.Family) is { } filling)
					body = Prepend(filling, body);

				alternatives.Add(body);
			}

			var reader = random.Next(6) switch
			{
				0 => ReaderKind.GuardReadsNone,
				1 => ReaderKind.GuardReadsName,
				_ => ReaderKind.None,
			};

			if (reader == ReaderKind.GuardReadsName)
			{
				if (FirstName(alternatives[0]) is { } read)
					alternatives[0] = Append(alternatives[0], new Guard("Seen(" + read + ")", read));
				else
					reader = ReaderKind.None;
			}
			else if (reader == ReaderKind.GuardReadsNone)
			{
				alternatives[0] = Append(alternatives[0], new Guard("true", null));
			}

			yield return new RuleShape
			{
				Id           = "random/" + seed + "/" + index + "/" + type.Name,
				Tier         = "random",
				Type         = type,
				Structure    = "random",
				Reader       = reader,
				Alternatives = alternatives,
			};
		}
	}

	static Expr Node(System.Random random, RuleType type, int depth)
	{
		var atoms = CoreAtoms.Concat(ExtraAtoms).Where(a => a != Atom.Self && AtomFor(type.Family, a) is not null).ToArray();

		Expr Leaf()
		{
			var atom = atoms[random.Next(atoms.Length)];
			Expr call = new Call(AtomFor(type.Family, atom)!, atom);

			return random.Next(5) switch
			{
				0 => new Named("w", call),
				_ => call,
			};
		}

		if (depth == 0)
			return Leaf();

		switch (random.Next(9))
		{
			case 0:
				return new Sequence(Node(random, type, depth - 1), L(","), Node(random, type, depth - 1));

			case 1:
				return new Choice(new Sequence(L("("), Node(random, type, depth - 1), L(")")), Node(random, type, depth - 1));

			case 2:
				return new Wrapped(Wrap.Optional, Node(random, type, depth - 1));

			case 3:
				return new Wrapped(Wrap.Star, new Sequence(L(","), Node(random, type, depth - 1)));

			case 4:
				return new Wrapped(Wrap.Plus, new Sequence(L(":"), Node(random, type, depth - 1)));

			case 5:
				return new Named(random.Next(2) == 0 ? "x" : TextParameter(type.Family) ?? "x", new Group(new Sequence(L("("), Node(random, type, depth - 1), L(")"))));

			case 6:
				return new Sequence(new Wrapped(Wrap.NotAhead, Node(random, type, depth - 1)), L("#"));

			default:
				return Leaf();
		}
	}
}
