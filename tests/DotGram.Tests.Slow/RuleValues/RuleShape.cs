using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGram.Tests.RuleValues;

// The grammar descriptor of the rule-value gate: one rule under test, said by the dimensions of where its implicit
// value can come from (its type, the operands it reads on each route, the names it writes, what reads a name, left
// recursion, who calls it, a template), and the expression tree printed from them. The printer (RulePrinter) turns a
// shape into grammar text, host members and one input per route; the runner (RuleRunner) compiles it with two
// generators. Nothing here decides a value: the descriptor says what was written, and only that.

/// <summary>
/// What kind of value a type is: the families the gate's helper rules are written for.
/// </summary>
enum Family
{
	/// <summary><c>string</c>.</summary>
	Text,

	/// <summary>The generated <c>SourceSpan</c>.</summary>
	Span,

	/// <summary>
	/// The host class <c>Box</c>: constructors <c>Box(string name)</c>, <c>Box(Box inner)</c> and the tie
	/// <c>Box(int value)</c> / <c>Box(char value)</c>; prints <c>box:</c> and what built it.
	/// </summary>
	Box,

	/// <summary>The host class <c>Pr</c>, built by an initializer of its settable <c>L</c>, <c>R</c>, <c>T</c>.</summary>
	Record,

	/// <summary><c>Item</c>, the type the generator makes for <c>Item = x: Nm</c>.</summary>
	Generated,

	/// <summary><c>int</c>.</summary>
	Number,

	/// <summary><c>object</c>.</summary>
	Object,
}

/// <summary>
/// How the rule under test says its type.
/// </summary>
enum TypeSaid
{
	/// <summary><c>: @T</c>.</summary>
	Own,

	/// <summary><c>: Q</c>, the type of rule <c>Q</c>.</summary>
	Taken,

	/// <summary><c>: @T[]</c>.</summary>
	Sequence,

	/// <summary>No type.</summary>
	None,
}

/// <summary>
/// How a helper rule makes its own value, as written (not as any generator reads it).
/// </summary>
enum Making
{
	/// <summary>Every alternative has <c>=&gt;</c>.</summary>
	Arrow,

	/// <summary>Declares <c>@string</c> or <c>@SourceSpan</c>, writes no name and reads no rule.</summary>
	Extent,

	/// <summary>Declares a host type and names what its constructor or initializer takes.</summary>
	Constructed,

	/// <summary>Declares no type and names what the generated type holds.</summary>
	Generated,

	/// <summary>Takes the type of another rule and reads exactly one call of that rule between two literals.</summary>
	TakenAroundOperand,

	/// <summary>Takes the type of a text rule and reads only literals and an untyped rule.</summary>
	TakenAroundPlumbing,

	/// <summary>Declares <c>@string</c> or <c>@SourceSpan</c>, names its text and a guard reads the name.</summary>
	ExtentReadByGuard,

	/// <summary>Declares no type and names nothing.</summary>
	Plumbing,

	/// <summary>
	/// Declares <c>@string</c> or <c>@SourceSpan</c>, writes no <c>=&gt;</c> and no name, and calls a rule that builds its
	/// value (<c>=&gt;</c>) between two literals: what it makes is its own text, whatever it calls.
	/// </summary>
	ExtentAroundMaking,

	/// <summary>
	/// Takes the type of a rule that builds its value with <c>=&gt;</c> and reads exactly one call of that rule between two
	/// literals: the one operand of its type is a rule that makes a value.
	/// </summary>
	TakenAroundMakingOperand,

	/// <summary>
	/// Takes the type of a rule that builds its value with <c>=&gt;</c>, and has two routes: the first reads that rule, the
	/// second reads a text rule between two literals.
	/// </summary>
	TakenPerRoute,
}

/// <summary>
/// What a helper hands to a rule that calls it, as the helper is written.
/// </summary>
enum HelperKind
{
	/// <summary>Its value is its own text or bounds, whatever it calls: it is no operand and no element of an array.</summary>
	Text,

	/// <summary>Something makes its value: a <c>=&gt;</c>, a construction, a generated type, or a rule that hands one on.</summary>
	Making,

	/// <summary>It has no type.</summary>
	Plumbing,
}

/// <summary>
/// A rule the rule under test calls. Each reads a fixed width of input, so that calls side by side need no separator.
/// </summary>
sealed class Helper
{
	Helper(string name, string definition, Making making, Family? family, string? type, string[] calls, Func<Samples, string> sample, Func<string, string>? value)
	{
		Name       = name;
		Definition = definition;
		Making     = making;
		Family     = family;
		Type       = type;
		Calls      = calls;
		Sample     = sample;
		Value      = value;
	}

	/// <summary>The rule's name.</summary>
	public string Name { get; }

	/// <summary>The rule as written in the grammar.</summary>
	public string Definition { get; }

	/// <summary>How it makes its own value.</summary>
	public Making Making { get; }

	/// <summary>The family of the type it has, or null for none.</summary>
	public Family? Family { get; }

	/// <summary>The C# type it has (<c>string</c>, <c>SourceSpan</c>, <c>Box</c>, …), or null for none.</summary>
	public string? Type { get; }

	/// <summary>The helpers its definition calls.</summary>
	public string[] Calls { get; }

	/// <summary>Writes an input the rule reads whole, drawing fresh letters or digits from the samples.</summary>
	public Func<Samples, string> Sample { get; }

	/// <summary>
	/// How many characters every input of the rule has. A rule with two routes (<see cref="Alternate"/>) has the width of
	/// its first route's input.
	/// </summary>
	public int Width => Sample(new Samples()).Length;

	/// <summary>The input of the rule's second route, for a rule with two routes of different width; otherwise null.</summary>
	public Func<Samples, string>? Alternate { get; private init; }

	/// <summary>
	/// The rule's body as a tree, for writing it in place of a call (the argument is how many times a body has already
	/// been written in place in the shape, so that a name it holds stays unique); null for a rule that makes a value.
	/// </summary>
	public Func<int, Expr>? Inline { get; private init; }

	/// <summary>What the rule hands to its caller.</summary>
	public HelperKind Kind
	{
		get
		{
			switch (Making)
			{
				case Making.Extent:
				case Making.ExtentReadByGuard:
				case Making.TakenAroundOperand:
				case Making.TakenAroundPlumbing:
				case Making.ExtentAroundMaking:
					return HelperKind.Text;

				case Making.Plumbing:
					return HelperKind.Plumbing;

				default:
					return HelperKind.Making;
			}
		}
	}

	/// <summary>
	/// The value the rule makes of its own input, printed as the runner prints values in mode D, where that value is not
	/// itself a question of the gate (a <c>=&gt;</c>, a construction, a rule's own extent); null where it is.
	/// </summary>
	public Func<string, string>? Value { get; }

	public override string ToString()
	{
		return Name;
	}

	// The values name their source: a value built by Word is "W:" and the text, a Box prints "box:" and what it was
	// built from, a span built by Sp is the matched bounds moved by 100, Num is the number its two digits write.

	public static readonly Helper Word = new(
		"Word", "Word : @string = c: ['a'..'z']{2} => @(\"W:\" + T(c))", Making.Arrow, RuleValues.Family.Text, "string", [],
		static s => s.Letters(), static t => "\"W:" + t + "\"");

	public static readonly Helper Bare = new(
		"Bare", "Bare : @string = ['a'..'z']{2}", Making.Extent, RuleValues.Family.Text, "string", [],
		static s => s.Letters(), static t => "\"" + t + "\"")
	{
		Inline = static _ => new Lexeme("['a'..'z']{2}", static s => s.Letters()),
	};

	public static readonly Helper Plain = new(
		"Plain", "Plain = ['a'..'z']{2}", Making.Plumbing, null, null, [],
		static s => s.Letters(), null);

	public static readonly Helper Mid = new(
		"Mid", "Mid : Bare = '(' & Bare & ')'", Making.TakenAroundOperand, RuleValues.Family.Text, "string", ["Bare"],
		static s => "(" + s.Letters() + ")", null)
	{
		Inline = static _ => new Sequence(new Literal("("), new Call(Bare, RuleValues.Atom.Implicit), new Literal(")")),
	};

	public static readonly Helper C4 = new(
		"C4", "C4 : Bare = '(' & Plain & ')'", Making.TakenAroundPlumbing, RuleValues.Family.Text, "string", ["Bare", "Plain"],
		static s => "(" + s.Letters() + ")", null)
	{
		Inline = static _ => new Sequence(new Literal("("), new Call(Plain, RuleValues.Atom.Plumbing), new Literal(")")),
	};

	public static readonly Helper G = new(
		"G", "G : @string = g: ['a'..'z']{2} & when @(Seen(g))", Making.ExtentReadByGuard, RuleValues.Family.Text, "string", [],
		static s => s.Letters(), null)
	{
		Inline = static n => ReadByGuard(n),
	};

	public static readonly Helper Sp = new(
		"Sp", "Sp : @SourceSpan = x: ['a'..'z']{2} => @(Shift(parserSpan))", Making.Arrow, RuleValues.Family.Span, "SourceSpan", [],
		static s => s.Letters(), null);

	public static readonly Helper Span = new(
		"Span", "Span : @SourceSpan = ['a'..'z']{2}", Making.Extent, RuleValues.Family.Span, "SourceSpan", [],
		static s => s.Letters(), null)
	{
		Inline = static _ => new Lexeme("['a'..'z']{2}", static s => s.Letters()),
	};

	public static readonly Helper Mids = new(
		"Mids", "Mids : Span = '(' & Span & ')'", Making.TakenAroundOperand, RuleValues.Family.Span, "SourceSpan", ["Span"],
		static s => "(" + s.Letters() + ")", null)
	{
		Inline = static _ => new Sequence(new Literal("("), new Call(Span, RuleValues.Atom.Implicit), new Literal(")")),
	};

	// An own text rule over a making rule is text: its value is what it matched, whatever the rule inside it builds.
	public static readonly Helper Ow = new(
		"Ow", "Ow : @string = '(' & Word & ')'", Making.ExtentAroundMaking, RuleValues.Family.Text, "string", ["Word"],
		static s => "(" + s.Letters() + ")", null)
	{
		Inline = static _ => new Sequence(new Literal("("), new Call(Word, RuleValues.Atom.Built), new Literal(")")),
	};

	public static readonly Helper Ows = new(
		"Ows", "Ows : @SourceSpan = '(' & Sp & ')'", Making.ExtentAroundMaking, RuleValues.Family.Span, "SourceSpan", ["Sp"],
		static s => "(" + s.Letters() + ")", null)
	{
		Inline = static _ => new Sequence(new Literal("("), new Call(Sp, RuleValues.Atom.Built), new Literal(")")),
	};

	// A rule that takes its type from a making rule and reads that rule is making: its value is the one it reads.
	public static readonly Helper Mw = new(
		"Mw", "Mw : Word = '(' & Word & ')'", Making.TakenAroundMakingOperand, RuleValues.Family.Text, "string", ["Word"],
		static s => "(" + s.Letters() + ")", null);

	public static readonly Helper Mws = new(
		"Mws", "Mws : Sp = '(' & Sp & ')'", Making.TakenAroundMakingOperand, RuleValues.Family.Span, "SourceSpan", ["Sp"],
		static s => "(" + s.Letters() + ")", null);

	// Two routes of different width: the first reads Word ("ab"), the second a text rule between brackets ("[cd]").
	public static readonly Helper Mx = new(
		"Mx", "Mx : Word = Word | '[' & Bare & ']'", Making.TakenPerRoute, RuleValues.Family.Text, "string", ["Word", "Bare"],
		static s => s.Letters(), null)
	{
		Alternate = static s => "[" + s.Letters() + "]",
	};

	public static readonly Helper Gs = new(
		"Gs", "Gs : @SourceSpan = g: ['a'..'z']{2} & when @(Seen(g))", Making.ExtentReadByGuard, RuleValues.Family.Span, "SourceSpan", [],
		static s => s.Letters(), null)
	{
		Inline = static n => ReadByGuard(n),
	};

	public static readonly Helper Nm = new(
		"Nm", "Nm : @string = n: ['a'..'z']{2} => @(T(n))", Making.Arrow, RuleValues.Family.Text, "string", [],
		static s => s.Letters(), static t => "\"" + t + "\"");

	public static readonly Helper Dg = new(
		"Dg", "Dg : @string = d: ['0'..'9']{2} => @(T(d))", Making.Arrow, RuleValues.Family.Text, "string", [],
		static s => s.Digits(), static t => "\"" + t + "\"");

	public static readonly Helper Bx = new(
		"Bx", "Bx : @Box = name: Nm", Making.Constructed, RuleValues.Family.Box, "Box", ["Nm"],
		static s => s.Letters(), static t => "box:" + t);

	public static readonly Helper Bxa = new(
		"Bxa", "Bxa : @Box = n: Nm => @(new Box(\"A\" + n))", Making.Arrow, RuleValues.Family.Box, "Box", ["Nm"],
		static s => s.Letters(), static t => "box:A" + t);

	public static readonly Helper Lf = new(
		"Lf", "Lf : @Pr = t: Nm", Making.Constructed, RuleValues.Family.Record, "Pr", ["Nm"],
		static s => s.Letters(), static t => "pr:" + t);

	public static readonly Helper Lfa = new(
		"Lfa", "Lfa : @Pr = n: Nm => @(new Pr { T = \"A\" + n })", Making.Arrow, RuleValues.Family.Record, "Pr", ["Nm"],
		static s => s.Letters(), static t => "pr:A" + t);

	public static readonly Helper Item = new(
		"Item", "Item = x: Nm", Making.Generated, RuleValues.Family.Generated, "Item", ["Nm"],
		static s => s.Letters(), static t => "Item{X=\"" + t + "\"}");

	public static readonly Helper Num = new(
		"Num", "Num : @int = d: ['0'..'9']{2} => @(N(d))", Making.Arrow, RuleValues.Family.Number, "int", [],
		static s => s.Digits(), static t => int.Parse(t, System.Globalization.CultureInfo.InvariantCulture).ToString(System.Globalization.CultureInfo.InvariantCulture));

	public static readonly Helper O = new(
		"O", "O : @object = c: ['a'..'z']{2} => @((object)(\"O:\" + T(c)))", Making.Arrow, RuleValues.Family.Object, "object", [],
		static s => s.Letters(), static t => "\"O:" + t + "\"");

	public static readonly Helper Ch = new(
		"Ch", "Ch : @char = x: ['a'..'z'] => @('c')", Making.Arrow, null, "char", [],
		static s => "k", static _ => "'c'");

	// The body of G and Gs written in place: the capture's name is made unique, since two bodies may stand in one rule.
	static Expr ReadByGuard(int ordinal)
	{
		var name = ordinal == 0 ? "g" : "g" + ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture);

		return new Sequence(new Named(name, new Lexeme("['a'..'z']{2}", static s => s.Letters())), new Guard("Seen(" + name + ")", name));
	}

	/// <summary>Every helper, by name.</summary>
	public static readonly IReadOnlyDictionary<string, Helper> All =
		new[] { Word, Bare, Plain, Mid, C4, G, Sp, Span, Mids, Gs, Ow, Ows, Mw, Mws, Mx, Nm, Dg, Bx, Bxa, Lf, Lfa, Item, Num, O, Ch }.ToDictionary(static h => h.Name);
}

/// <summary>
/// Draws the inputs of helper calls: each call gets letters or digits no other call of the same input has.
/// </summary>
sealed class Samples
{
	int _letters;
	int _digits;

	/// <summary>Two fresh lowercase letters: <c>ab</c>, <c>cd</c>, … (never <c>k</c>, which <c>Ch</c> reads).</summary>
	public string Letters()
	{
		const string pool = "abcdefghijlmnopqrstuvwxyz";

		var i = _letters++ % 12;

		return pool.Substring(i * 2, 2);
	}

	/// <summary>Two fresh digits: <c>12</c>, <c>34</c>, … — except where a count is drawn (<see cref="Count"/>).</summary>
	public string Digits()
	{
		var i = _digits++ % 4;

		return new[] { "12", "34", "56", "78" }[i];
	}

	/// <summary>The digits a <c>count</c> reads: <c>03</c>, so that <c>new string('c', count)</c> is <c>ccc</c>.</summary>
	public static string Count()
	{
		return "03";
	}
}

/// <summary>
/// The kind of operand a slot of a structure holds, per family (<see cref="RuleShapes.AtomFor"/>).
/// </summary>
enum Atom
{
	/// <summary>A rule of the family's type that builds its value with <c>=&gt;</c> (Word, Sp, Bxa, Lfa, Num, O).</summary>
	Built,

	/// <summary>
	/// A rule of the family's type that makes its value without <c>=&gt;</c>: its extent (Bare, Span), a construction (Bx,
	/// Lf) or a generated type (Item).
	/// </summary>
	Implicit,

	/// <summary>A rule of a type that is not the family's (Num beside text and spans, Word beside the rest).</summary>
	Other,

	/// <summary>A rule with no type and no names (Plain).</summary>
	Plumbing,

	/// <summary>A rule that takes the family's type from another and reads one call of it (Mid, Mids).</summary>
	TakenExtent,

	/// <summary>A rule that takes a text rule's type and reads only literals and an untyped rule (C4).</summary>
	TakenOverPlumbing,

	/// <summary>A text rule whose own extent is its value and whose name a guard reads (G, Gs).</summary>
	ReaderExtent,

	/// <summary>A rule that declares <c>@string</c> or <c>@SourceSpan</c> and calls a building rule between literals: text (Ow, Ows).</summary>
	OwnOverMaking,

	/// <summary>A rule that takes a building rule's type and reads one call of it: making (Mw, Mws).</summary>
	TakenOverMaking,

	/// <summary>A rule with a building route and a text route: making (Mx).</summary>
	PerRoute,

	/// <summary>The rule under test itself.</summary>
	Self,
}

/// <summary>
/// The names written in the rule under test (one value per shape).
/// </summary>
enum NameKind
{
	/// <summary>No name.</summary>
	None,

	/// <summary><c>x: '&lt;'</c>: a name on the first literal.</summary>
	OnLiteral,

	/// <summary>
	/// Names that fill the type's constructor or initializer, on calls that are not operands, in front of every
	/// alternative (text: <c>c: Ch &amp; count: Num</c>; span: <c>start: Num &amp; length: Num</c>; Box: <c>name: Dg</c>;
	/// Pr: <c>t: Dg</c>; Item: <c>x: Dg</c>).
	/// </summary>
	Filling,

	/// <summary>The same names in front of the first alternative only.</summary>
	FillingFirstAlternative,

	/// <summary><c>w: O</c>: a neutral name directly on the first operand.</summary>
	OnOperand,

	/// <summary>The filling names and <c>w: O</c>.</summary>
	FillingAndOnOperand,

	/// <summary>A constructor's parameter or a settable member directly on the first operand (Box: <c>inner:</c>, Pr: <c>l:</c>).</summary>
	ParameterOnOperand,

	/// <summary><c>x: ('(' &amp; O &amp; ')')</c>: a neutral name on a group holding the first operand.</summary>
	OnGroup,

	/// <summary>A filling name on a group holding the first operand (Box: <c>name:</c>, Pr: <c>t:</c>, Item: <c>x:</c>).</summary>
	FillingOnGroup,

	/// <summary>Half of the filling names (text: <c>c: Ch</c>; span: <c>start: Num</c>).</summary>
	Partial,

	/// <summary>A name two constructors take equally (<c>value:</c>: <c>string(char[])</c> / <c>string(ReadOnlySpan&lt;char&gt;)</c>; Box(int) / Box(char)).</summary>
	Tie,
}

/// <summary>
/// What reads a name of the rule under test, written at the end of its first alternative.
/// </summary>
enum ReaderKind
{
	/// <summary>Nothing.</summary>
	None,

	/// <summary><c>when @(Seen(n))</c> over the rule's first name.</summary>
	GuardReadsName,

	/// <summary><c>when @(true)</c>.</summary>
	GuardReadsNone,

	/// <summary><c>switch @(Pick(n)) { case 1: any{0}; default: any{0} }</c> over the rule's first name.</summary>
	Switch,
}

/// <summary>
/// Who calls the rule under test.
/// </summary>
enum SeenFrom
{
	/// <summary>It is <c>Start</c>, published itself.</summary>
	Itself,

	/// <summary><c>Start : @E[] = R &amp; ';' &amp; B</c>, beside a built operand <c>B</c> of its element type.</summary>
	InSequence,

	/// <summary><c>Start : R = '[' &amp; R &amp; ']'</c>: the one operand of a rule that takes its type.</summary>
	TakenBy,

	/// <summary><c>Start : R = ‹filling names› &amp; R</c>: an operand beside names that fill a constructor.</summary>
	BesideFilling,
}

/// <summary>
/// One value of the type dimension: what is written after the rule's colon.
/// </summary>
sealed class RuleType
{
	RuleType(string name, TypeSaid how, Family family, string? text, string clrType, Helper? taken)
	{
		Name    = name;
		How     = how;
		Family  = family;
		Text    = text;
		ClrType = clrType;
		Taken   = taken;
	}

	/// <summary>A short name for reports: <c>@string</c>, <c>:Word</c>, <c>@int[]</c>, <c>none</c>.</summary>
	public string Name { get; }

	/// <summary>How the type is said.</summary>
	public TypeSaid How { get; }

	/// <summary>The family of the type, of the element type for a sequence; text for a rule with no type.</summary>
	public Family Family { get; }

	/// <summary>What is printed after the colon, or null for no type.</summary>
	public string? Text { get; }

	/// <summary>The C# type (of the element, for a sequence); <c>string</c> for a rule with no type.</summary>
	public string ClrType { get; }

	/// <summary>The rule whose type is taken, for <see cref="TypeSaid.Taken"/>.</summary>
	public Helper? Taken { get; }

	public override string ToString()
	{
		return Name;
	}

	static RuleType Own(string type, Family family)
	{
		return new("@" + type, TypeSaid.Own, family, "@" + type, type, null);
	}

	static RuleType Taking(Helper rule)
	{
		return new(":" + rule.Name, TypeSaid.Taken, rule.Family ?? RuleValues.Family.Text, rule.Name, rule.Type ?? "string", rule);
	}

	static RuleType Sequence(string type, Family family)
	{
		return new("@" + type + "[]", TypeSaid.Sequence, family, "@" + type + "[]", type, null);
	}

	/// <summary>Every value of the dimension.</summary>
	public static readonly IReadOnlyList<RuleType> All =
	[
		Own("string", Family.Text),
		Own("SourceSpan", Family.Span),
		Own("Box", Family.Box),
		Own("Pr", Family.Record),
		Own("int", Family.Number),
		Own("object", Family.Object),
		Taking(Helper.Word),
		Taking(Helper.Bare),
		Taking(Helper.Plain),
		Taking(Helper.Sp),
		Taking(Helper.Span),
		Taking(Helper.Bx),
		Taking(Helper.Lf),
		Taking(Helper.Item),
		Taking(Helper.Num),
		Taking(Helper.O),
		new("none", TypeSaid.None, Family.Text, null, "string", null),
		Sequence("string", Family.Text),
		Sequence("int", Family.Number),
		Sequence("SourceSpan", Family.Span),
	];

	/// <summary>The value of the dimension with the given short name.</summary>
	public static RuleType Named(string name)
	{
		return All.First(t => t.Name == name);
	}
}

/// <summary>
/// A node of the body of the rule under test.
/// </summary>
abstract class Expr
{
}

/// <summary>A literal: <c>'&lt;'</c>.</summary>
sealed class Literal(string text) : Expr
{
	/// <summary>The literal's text, which is also its input.</summary>
	public string Text { get; } = text;
}

/// <summary>
/// A piece of lexical grammar written in place (a helper's body, inlined): <see cref="Pattern"/> is printed and
/// <see cref="Sample"/> writes the input it reads.
/// </summary>
sealed class Lexeme(string pattern, Func<Samples, string> sample) : Expr
{
	/// <summary>The grammar text.</summary>
	public string Pattern { get; } = pattern;

	/// <summary>Writes an input the piece reads.</summary>
	public Func<Samples, string> Sample { get; } = sample;
}

/// <summary><c>any{n}</c> after a lookahead: reads what the lookahead looked at.</summary>
sealed class AnyItems(int count) : Expr
{
	/// <summary>How many items it reads.</summary>
	public int Count { get; } = count;
}

/// <summary>A call of a helper rule.</summary>
sealed class Call(Helper rule, Atom? atom, bool count = false) : Expr
{
	/// <summary>The rule called.</summary>
	public Helper Rule { get; } = rule;

	/// <summary>The kind of operand the slot holds, or null for a call that fills a name and is no slot of the structure.</summary>
	public Atom? Atom { get; } = atom;

	/// <summary>The call reads the digits of a count (<see cref="Samples.Count"/>) rather than fresh ones.</summary>
	public bool Count { get; } = count;
}

/// <summary>A call of the rule under test itself.</summary>
sealed class SelfCall : Expr
{
}

/// <summary>The parameter <c>x</c> of a template; the shape's <see cref="RuleShape.TemplateArgument"/> is passed for it.</summary>
sealed class Parameter : Expr
{
}

/// <summary><c>name: inner</c>.</summary>
sealed class Named(string name, Expr inner) : Expr
{
	/// <summary>The name.</summary>
	public string Name { get; } = name;

	/// <summary>What is named.</summary>
	public Expr Inner { get; } = inner;
}

/// <summary><c>a &amp; b &amp; …</c>.</summary>
sealed class Sequence(params Expr[] items) : Expr
{
	/// <summary>The items, in order.</summary>
	public Expr[] Items { get; } = items;
}

/// <summary><c>(a | b | …)</c>: a choice inside an alternative, printed in parentheses.</summary>
sealed class Choice(params Expr[] alternatives) : Expr
{
	/// <summary>The alternatives, in order.</summary>
	public Expr[] Alternatives { get; } = alternatives;
}

/// <summary><c>(inner)</c>: a group, written for a name to stand on.</summary>
sealed class Group(Expr inner) : Expr
{
	/// <summary>What the group holds.</summary>
	public Expr Inner { get; } = inner;
}

/// <summary>The repetitions and lookaheads.</summary>
enum Wrap
{
	/// <summary><c>X?</c>.</summary>
	Optional,

	/// <summary><c>X*</c>.</summary>
	Star,

	/// <summary><c>X+</c>.</summary>
	Plus,

	/// <summary><c>X{0}</c>.</summary>
	Zero,

	/// <summary><c>?=(X)</c>.</summary>
	Ahead,

	/// <summary><c>?!(X)</c>.</summary>
	NotAhead,
}

/// <summary>A repetition or lookahead of <see cref="Inner"/>.</summary>
sealed class Wrapped(Wrap wrap, Expr inner) : Expr
{
	/// <summary>Which.</summary>
	public Wrap Wrap { get; } = wrap;

	/// <summary>What it repeats or looks at.</summary>
	public Expr Inner { get; } = inner;
}

/// <summary><c>when @(code)</c>; <see cref="Reads"/> is the name it reads, or null.</summary>
sealed class Guard(string code, string? reads) : Expr
{
	/// <summary>The C# inside <c>@( )</c>.</summary>
	public string Code { get; } = code;

	/// <summary>The name it reads, or null.</summary>
	public string? Reads { get; } = reads;
}

/// <summary><c>switch @(Pick(name)) { case 1: any{0}; default: any{0} }</c>: reads a name, matches nothing.</summary>
sealed class SwitchOn(string reads) : Expr
{
	/// <summary>The name its selector reads.</summary>
	public string Reads { get; } = reads;
}

/// <summary>
/// One grammar of the gate: the rule under test, said by its dimensions and its body.
/// </summary>
sealed class RuleShape
{
	/// <summary>
	/// A stable id built from the dimensions (or the seed and index of a random shape): what a report, an environment
	/// filter and the allow-list name it by.
	/// </summary>
	public required string Id { get; init; }

	/// <summary>Which tier made it: <c>core</c>, <c>deviation</c>, <c>atoms</c>, <c>random</c>, <c>landmark</c>.</summary>
	public required string Tier { get; init; }

	/// <summary>
	/// The shape this one was made from by writing the bodies of its text rules in place of their calls, or null: the two
	/// are a pair, and one is the other's extraction.
	/// </summary>
	public RuleShape? InlinedFrom { get; init; }

	/// <summary>The type of the rule under test.</summary>
	public required RuleType Type { get; init; }

	/// <summary>The structure the body was made from (<see cref="RuleShapes.Structures"/>), or <c>random</c>.</summary>
	public required string Structure { get; init; }

	/// <summary>The kind of operand in the structure's first slot, or null where it has none.</summary>
	public Atom? First { get; init; }

	/// <summary>The kind of operand in the structure's second slot, or null where it has none.</summary>
	public Atom? Second { get; init; }

	/// <summary>The names written.</summary>
	public NameKind Names { get; init; }

	/// <summary>What reads a name.</summary>
	public ReaderKind Reader { get; init; }

	/// <summary>The rule is left-recursive: <c>R = R &amp; '+' &amp; '#' | ‹body›</c>.</summary>
	public bool LeftRecursive { get; init; }

	/// <summary>Who calls the rule under test.</summary>
	public SeenFrom SeenFrom { get; init; }

	/// <summary>
	/// The rule is written as the template <c>P(x) : x = ‹body with x for the taken rule›</c> and called as
	/// <c>Start : Q = P(Q)</c>; <see cref="TemplateArgument"/> is <c>Q</c>.
	/// </summary>
	public bool Template { get; init; }

	/// <summary>The rule passed for the template's parameter.</summary>
	public Helper? TemplateArgument => Template ? Type.Taken : null;

	/// <summary>The body of the rule under test: its top-level alternatives, after names, reader and left recursion.</summary>
	public required IReadOnlyList<Expr> Alternatives { get; init; }

	/// <summary>The name of the rule under test in the grammar: <c>Start</c>, or <c>R</c> when something calls it.</summary>
	public string RuleName => Template ? "P" : SeenFrom == SeenFrom.Itself ? "Start" : "R";

	public override string ToString()
	{
		return Id;
	}
}
