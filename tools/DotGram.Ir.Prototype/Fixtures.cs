using System;

using DotGram;

namespace DotGram.Ir.Prototype;

/// <summary>
/// The grammars the contracts are stated over, each compiled here into its C# parser as well: that
/// parser is what every other reading of the grammar is held to.
/// </summary>
/// <remarks>
/// A grammar whose constructions matter is written as a twin: every host fragment is a call of
/// <see cref="Twin"/> by number, so that what it builds is a text that says which fragment ran over
/// which arguments, and every such call is written down in the order it ran.
/// </remarks>
static class Grammars
{
	/// <summary>
	/// A call that has answered once is asked again: <c>Name</c> answers "xy", <c>'y'</c> then
	/// fails, and "xy" is accepted only where <c>Name</c> can be asked for "x".
	/// </summary>
	public const string Resumption = """
		Start = Name & 'y' & eof
		Name  = "xy" | "x"

		parse Start
		""";

	/// <summary>The same with recursion in <c>Name</c>.</summary>
	public const string ResumptionRecursive = """
		Start = Name & 'y' & eof
		Name  = "xy" | "x" & Name?

		parse Start
		""";

	/// <summary>The same call inside braces: its first answer is the only one.</summary>
	public const string Commit = """
		Start = { Name } & 'y' & eof
		Name  = "xy" | "x"

		parse Start
		""";

	/// <summary>
	/// Braces round something that has to find its own second reading before it commits to one.
	/// </summary>
	public const string CommitInner = """
		Start = { Name & 'y' } & 'y'? & eof
		Name  = "xy" | "x"

		parse Start
		""";

	/// <summary>A capture made on a reading that is then given back.</summary>
	public const string CaptureRollback = """
		Start : @V = (a: 'x')? & b: "xy" & eof => @(Twin.C(0, a, b))

		parse Start
		""";

	/// <summary>A capture made in an alternative that then fails.</summary>
	public const string CaptureRollbackChoice = """
		Start : @V = (a: 'x' & 'z' | b: 'x' & 'y') & eof => @(Twin.C(0, a, b))

		parse Start
		""";

	/// <summary>A binding made on a reading that is then given back, and read after it.</summary>
	public const string BindingRollback = """
		Start = $q: Name & 'b'? & '-' & $q & eof
		Name  = "ab" | "a"

		parse Start
		""";

	/// <summary>A binding, a reference to it, a count by its length and a negative look.</summary>
	public const string Fence = """
		Fenced  = ' '{0,3} & $f: '`'{3,} & [^ '`' | '\r' | '\n']* & eol
		          & { (?!(' '{0,3} & $f & '`'* & ' '* & EndLine) & Line)* }
		          & (' '{0,3} & $f & '`'* & ' '* & EndLine | eof)
		Line    = [^ '\r' | '\n']+ & EndLine | eol
		EndLine = eol | eof

		Heading = $t: Title & eol & '='{#t,} & eol
		Title   = [^ '\r' | '\n']+

		parse Fenced
		parse Heading
		""";

	/// <summary>
	/// A construction under a call that is asked again: when it runs, and how many times.
	/// </summary>
	public const string ActionTiming = """
		Start : @V = n: Name & 'y' & eof => @(Twin.C(0, n))
		Name  : @V = t: ("xy" | "x")     => @(Twin.C(1, t))

		parse Start
		""";

	/// <summary>A guard over a value, asked on each reading.</summary>
	public const string GuardTiming = """
		Start : @V = n: Name & 'y' & eof                              => @(Twin.C(0, n))
		Name  : @V = t: Word & when @(Twin.G(1, t.Length < 3, t))     => @(Twin.C(2, t))
		Word       = "xyy" | "xy" | "x"

		parse Start
		""";

	/// <summary>Captures inside a repetition, and a repetition that gives turns back.</summary>
	public const string Loop = """
		Start : @V = items: Item* & last: Letter & eof => @(Twin.C(0, items, last))
		Item  : @V = t: Letter & ','?                   => @(Twin.C(1, t))
		Letter     = ['a'..'z']

		parse Start
		""";

	/// <summary>A binding inside a repetition, which every turn writes again.</summary>
	public const string Rebinding = """
		Start = ($q: ['a'..'z'] & $q)* & eof

		parse Start
		""";

	/// <summary>A positive and a negative look, with a construction under each.</summary>
	public const string Look = """
		Start : @V = ?=Pair & a: Item & ?!Pair & b: Item & eof => @(Twin.C(0, a, b))
		Pair  : @V = x: Item & y: Item                           => @(Twin.C(1, x, y))
		Item  : @V = t: ['a'..'z']                               => @(Twin.C(2, t))

		parse Start
		""";

	/// <summary>The CSV of typed fields, published through a rule that is not itself the sequence.</summary>
	public const string Csv = """
		File : @V[] = rows: Csv => @(rows)
		Csv  : @V[] = Row* & eof

		Row : @V = name: Name & ',' & amount: Amount & eol => @(Twin.C(0, name, amount))

		Name : @V = text: ['a'..'z' | 'A'..'Z']+ & when @(Twin.G(1, text.Length <= 16, text)) => @(Twin.C(2, text))

		Amount : @V = digits: ['0'..'9']+ => @(Twin.C(3, digits))

		parse File
		""";

	/// <summary>
	/// Two lists in sequence: each item opens a way that outlives its turn, so a failure after the
	/// second list asks the first for another reading. Of two types, which the tree does not see:
	/// of one, the generated C# parser carries them only deferred.
	/// </summary>
	public const string ListsInSequence = """
		Start : @V = a: X* & 'y' & b: Y* & eof => @(Twin.C(0, a, b))
		X     : @V = t: ("xy" | "x")           => @(Twin.C(1, t))
		Y     : @W = t: ("xy" | "x")           => @(Twin.D(2, t))

		parse Start
		""";

	/// <summary>The same list rule read twice, and a failure after the second that asks the first again.</summary>
	public const string ListRuleTwice = """
		Start : @V = a: Body & 'y' & b: Body & eof => @(Twin.C(0, a, b))
		Body  : @V = items: X*                     => @(Twin.C(1, (object)items))
		X     : @V = t: ("xy" | "x")               => @(Twin.C(2, t))

		parse Start
		""";

	/// <summary>
	/// Repetitions the proofs settle, one inside the other: the inner one's last turn, which
	/// fails, begins where the outer one's next turn does. A repetition after them gives turns back.
	/// </summary>
	public const string NestedSettled = """
		Start : @V = groups: Group* & rest: Pair* & last: Letter & eof => @(Twin.C(0, groups, rest, last))
		Group : @V = items: Item* & ';'                                 => @(Twin.C(1, (object)items))
		Item  : @V = t: Digit                                           => @(Twin.C(2, t))
		Pair  : @W = t: Letter & ','?                                   => @(Twin.D(3, t))
		Letter     = ['a'..'z']
		Digit      = ['0'..'9']

		parse Start
		""";

	/// <summary>
	/// Constructions that throw: one in a turn that builds and is then given up, one in a turn
	/// that stands, before and inside a repetition that gives turns back.
	/// </summary>
	public const string Throwing = """
		Start : @V = ps: P* & ls: L* & last: Letter & eof => @(Twin.C(0, ps, ls, last))
		P     : @V = d: D & ':'                            => @(Twin.C(1, d))
		D     : @V = t: Digit                              => @(Twin.C(2, t))
		L     : @W = t: Letter & ','?                      => @(Twin.D(3, t))
		Letter     = ['a'..'z']
		Digit      = ['0'..'9']

		parse Start
		""";

	/// <summary>A repetition that gives a turn back twice and then stands.</summary>
	public const string GivenBackTwice = """
		Start : @V = items: Item* & a: Letter & b: Letter & eof => @(Twin.C(0, items, a, b))
		Item  : @V = t: Letter & ','?                            => @(Twin.C(1, t))
		Letter     = ['a'..'z']

		parse Start
		""";

	/// <summary>
	/// A calculator without folds: each level of precedence a repetition over the next, the power
	/// right-associative by recursion, and parentheses.
	/// </summary>
	public const string Arithmetic = """
		Expr     : @V = first: Term & rest: AddOp*  => @(Twin.C(0, first, rest))
		AddOp    : @V = op: ['+' | '-'] & t: Term   => @(Twin.C(1, op, t))
		Term     : @V = first: Power & rest: MulOp* => @(Twin.C(2, first, rest))
		MulOp    : @V = op: ['*' | '/'] & f: Power  => @(Twin.C(3, op, f))
		Power    : @V = b: Atom & e: Exponent?      => @(Twin.C(4, b, e))
		Exponent : @V = '^' & p: Power              => @(Twin.C(5, p))
		Atom     : @V
			= '(' & e: Expr & ')'     => @(Twin.C(6, e))
			| d: ['0'..'9']+          => @(Twin.C(7, d))

		parse Expr
		""";

	/// <summary>
	/// A repetition the proofs settle, each of whose turns opens a way that outlives it: a failure
	/// at the end gives the turns back one by one, each through a way inside a turn.
	/// </summary>
	public const string Config = """
		File  : @V = entries: Entry* & eof                 => @(Twin.C(0, (object)entries))
		Entry : @V = k: Key & '=' & v: Value & ';'          => @(Twin.C(1, k, v))
		Value : @V = t: ("ab" | "a" & ['a'..'z']* & 'x')    => @(Twin.C(2, t))
		Key        = ['a'..'z']+

		parse File
		""";

	/// <summary>A settled repetition with no way in its turns, before one that gives turns back.</summary>
	public const string SettledBefore = """
		Start : @V = xs: A* & bs: B* & last: Letter & eof => @(Twin.C(0, xs, bs, last))
		A     : @V = t: '0'                                => @(Twin.C(1, t))
		B     : @W = t: Letter & ','?                      => @(Twin.D(2, t))
		Letter     = ['a'..'z']

		parse Start
		""";

	/// <summary>The same in braces: whatever the repetition kept of its turns goes with the braces.</summary>
	public const string SealedBefore = """
		Start : @V = { xs: A* } & bs: B* & last: Letter & eof => @(Twin.C(0, xs, bs, last))
		A     : @V = t: '0'                                    => @(Twin.C(1, t))
		B     : @W = t: Letter & ','?                          => @(Twin.D(2, t))
		Letter     = ['a'..'z']

		parse Start
		""";

	/// <summary>A long call that decides nothing, before a repetition that gives turns back.</summary>
	public const string CalledBefore = """
		Start : @V = Pad & bs: B* & last: Letter & eof => @(Twin.C(0, bs, last))
		Pad        = ' '*
		B     : @V = t: Letter & ','?                   => @(Twin.C(1, t))
		Letter     = ['a'..'z']

		parse Start
		""";

	/// <summary>
	/// A settled repetition whose one turn reads a long run and then fails, before a repetition
	/// that gives turns back: the failed turn lies in every replayed prefix.
	/// </summary>
	public const string FailedTurnBefore = """
		Start : @V = ws: Pair* & bs: B* & last: Letter & eof => @(Twin.C(0, ws, bs, last))
		Pair  : @V = w: Word & ';'                           => @(Twin.C(1, w))
		B     : @W = t: Letter & ','?                        => @(Twin.D(2, t))
		Word       = Letter+
		Letter     = ['a'..'z']

		parse Start
		""";

	/// <summary>Braces inside braces, the inner round a long repetition, before a repetition that gives turns back.</summary>
	public const string NestedSealedBefore = """
		Start : @V = { '<' & { xs: A* } & '>' } & bs: B* & last: Letter & eof => @(Twin.C(0, xs, bs, last))
		A     : @V = t: '0'                                                  => @(Twin.C(1, t))
		B     : @W = t: Letter & ','?                                        => @(Twin.D(2, t))
		Letter     = ['a'..'z']

		parse Start
		""";

	/// <summary>A look that reads a long run, before a repetition that gives turns back.</summary>
	public const string LongLookBefore = """
		Start : @V = ?=('0'* & 'a') & z: Zeros & bs: B* & last: Letter & eof => @(Twin.C(0, z, bs, last))
		B     : @W = t: Letter & ','?                                          => @(Twin.D(1, t))
		Zeros      = '0'*
		Letter     = ['a'..'z']

		parse Start
		""";

	/// <summary>Braces round a long repetition and a long text, both captured, before a repetition that gives turns back.</summary>
	public const string SealedCaptureBefore = """
		Start : @V = { xs: A* & t: Word } & ';' & bs: B* & last: Letter & eof => @(Twin.C(0, xs, t, bs, last))
		A     : @V = t: '0'                                                  => @(Twin.C(1, t))
		B     : @W = t: Letter & ','?                                        => @(Twin.D(2, t))
		Word       = Letter+
		Letter     = ['a'..'z']

		parse Start
		""";

	/// <summary>A recursion that decides at every level, read whole before a repetition that gives turns back.</summary>
	public const string DecidingNestBefore = """
		Start : @V = Nest & bs: B* & last: Letter & eof => @(Twin.C(0, bs, last))
		Nest       = '(' & Nest & ')' | '(' & 'y'
		B     : @W = t: Letter & ','?                    => @(Twin.D(1, t))
		Letter     = ['a'..'z']

		parse Start
		""";

	/// <summary>A long text read again by reference, before a repetition that gives turns back.</summary>
	public const string SameBefore = """
		Start : @V = $q: Zeros & '=' & $q & bs: B* & last: Letter & eof => @(Twin.C(0, bs, last))
		B     : @W = t: Letter & ','?                                    => @(Twin.D(1, t))
		Zeros      = '0'+
		Letter     = ['a'..'z']

		parse Start
		""";

	/// <summary>A line as a YAML reader reads it: a long indent and a long key, each a text, before a repetition that gives turns back.</summary>
	public const string YamlLineBefore = """
		Start : @V = i: Indent & k: Key & ':' & bs: B* & last: Letter & eof => @(Twin.C(0, i, k, bs, last))
		B     : @W = t: Letter & ','?                                        => @(Twin.D(1, t))
		Indent     = ' '*
		Key        = ['0'..'9']+
		Letter     = ['a'..'z']

		parse Start
		""";

	/// <summary>
	/// A deep nest whose innermost reading can be split many ways, each of which stands inside:
	/// a failure at the top asks the innermost again, and every replay goes down the whole nest to it.
	/// </summary>
	public const string DeepSplit = """
		Start = P & '!'
		P     = '(' & P & ')' | Bs & Cs
		Bs    = 'b'*
		Cs    = 'b'*

		parse Start
		""";

	/// <summary>
	/// <see cref="DeepSplit"/> with nothing to read on the way back up: what a replay pays to go
	/// down the nest, apart from what it pays to come back.
	/// </summary>
	public const string DeepSplitOpen = """
		Start = P & '!'
		P     = '(' & P | Bs & Cs
		Bs    = 'b'*
		Cs    = 'b'*

		parse Start
		""";

	/// <summary>
	/// The XML subset as the example read it before its runs were made atomic: the name of an
	/// element gives characters back to an attribute written against it (<c>&lt;ab="1"&gt;</c> is
	/// <c>a</c> with the attribute <c>b</c>), inside the first of two alternatives that begin alike.
	/// </summary>
	public const string XmlNameGivesBack = """
		trivia = none

		Xml     = Element & eof
		Element = '<' & $name: Name & Attribute* & Space & '>' & Content* & "</" & $name & Space & '>'
		        | '<' & Name & Attribute* & Space & "/>"
		Content = Element | Chars
		Chars   = { [^ '<']+ }
		Attribute = Space & Name & '=' & Value
		Value   = '"' & Quoted & '"' | '\'' & Quoted & '\''
		Name    = ['a'..'z' | 'A'..'Z' | '_'] & ['a'..'z' | 'A'..'Z' | '0'..'9' | '_' | '-' | '.']*
		Space   = [' ' | '\t' | '\r' | '\n']*
		Quoted  = [^ '"' | '\'']*

		parse Xml
		""";

	/// <summary>
	/// A turn that fails after a choice inside it took its first alternative, where the second
	/// would have let the turn read: the turn's own decision is moved only once the choice's is spent.
	/// </summary>
	public const string TurnGivesBack = """
		Start = (("a" | "ab") & 'c')* & Y? & eof
		Y     = "ab"

		parse Start
		""";

	/// <summary>RFC 8259 as DotGram.Web writes it, every construction a numbered call.</summary>
	public const string Json = """
		trivia = none

		Ws = [' ' | '\t' | '\n' | '\r']*

		JsonText : @V = Ws & value: Value & Ws => @(Twin.C(0, value))

		Value : @V
			= "false"             => @(Twin.C(1))
			| "null"              => @(Twin.C(2))
			| "true"              => @(Twin.C(3))
			| members: ObjectBody => @(Twin.C(4, members))
			| items: ArrayBody    => @(Twin.C(5, items))
			| text: NumberText    => @(Twin.C(6, text))
			| text: StringText    => @(Twin.C(7, text))

		ObjectBody : @V = '{' & Ws & members: Members & '}' => @(Twin.C(8, members))

		Members : @V = (first: Member & rest: NextMember*)? => @(Twin.C(9, first, rest))

		Member : @V = name: StringText & Ws & ':' & Ws & value: Value & Ws => @(Twin.C(10, name, value))

		NextMember : @V = ',' & Ws & member: Member => @(Twin.C(11, member))

		ArrayBody : @V = '[' & Ws & items: Items & ']' => @(Twin.C(12, items))

		Items : @V = (first: Element & rest: NextElement*)? => @(Twin.C(13, first, rest))

		Element : @V = value: Value & Ws => @(Twin.C(14, value))

		NextElement : @V = ',' & Ws & element: Element => @(Twin.C(15, element))

		NumberText = '-'? & ('0' | ['1'..'9'] & ['0'..'9']*) & ('.' & ['0'..'9']+)? & (['e' | 'E'] & ['+' | '-']? & ['0'..'9']+)?

		StringText = '"' & ([' '..'!' | '#'..'[' | ']'..'￿'] | '\\' & (['"' | '\\' | '/' | 'b' | 'f' | 'n' | 'r' | 't'] | 'u' & Hexdig{4}))* & '"'

		Hexdig = ['0'..'9' | 'a'..'f' | 'A'..'F']

		parse JsonText as ParseJson
		""";
}

[Gram(Grammars.Resumption)]            static partial class ResumptionParser;
[Gram(Grammars.ResumptionRecursive)]   static partial class ResumptionRecursiveParser;
[Gram(Grammars.Commit)]                static partial class CommitParser;
[Gram(Grammars.CommitInner)]           static partial class CommitInnerParser;
[Gram(Grammars.CaptureRollback)]       static partial class CaptureRollbackParser;
[Gram(Grammars.CaptureRollbackChoice)] static partial class CaptureRollbackChoiceParser;
[Gram(Grammars.BindingRollback)]       static partial class BindingRollbackParser;
[Gram(Grammars.Fence)]                 static partial class FenceParser;
[Gram(Grammars.ActionTiming)]          static partial class ActionTimingParser;
[Gram(Grammars.GuardTiming)]           static partial class GuardTimingParser;
[Gram(Grammars.Loop)]                  static partial class LoopParser;
[Gram(Grammars.Rebinding)]             static partial class RebindingParser;
[Gram(Grammars.Look)]                  static partial class LookParser;
[Gram(Grammars.Csv)]                   static partial class CsvParser;
[Gram(Grammars.ListsInSequence, Carrier = GramCarrier.Immediate)]       static partial class ListsInSequenceParser;
[Gram(Grammars.ListRuleTwice, Carrier = GramCarrier.Immediate)]         static partial class ListRuleTwiceParser;
[Gram(Grammars.NestedSettled, Carrier = GramCarrier.Immediate)]         static partial class NestedSettledParser;
[Gram(Grammars.Throwing, Carrier = GramCarrier.Immediate)]              static partial class ThrowingParser;
[Gram(Grammars.GivenBackTwice, Carrier = GramCarrier.Immediate)]        static partial class GivenBackTwiceParser;
[Gram(Grammars.Arithmetic, Carrier = GramCarrier.Immediate)]            static partial class ArithmeticParser;
[Gram(Grammars.Config, Carrier = GramCarrier.Immediate)]                static partial class ConfigParser;
[Gram(Grammars.SettledBefore, Carrier = GramCarrier.Immediate)]         static partial class SettledBeforeParser;
[Gram(Grammars.SealedBefore, Carrier = GramCarrier.Immediate)]          static partial class SealedBeforeParser;
[Gram(Grammars.CalledBefore, Carrier = GramCarrier.Immediate)]          static partial class CalledBeforeParser;
[Gram(Grammars.FailedTurnBefore, Carrier = GramCarrier.Immediate)]    static partial class FailedTurnBeforeParser;
[Gram(Grammars.NestedSealedBefore, Carrier = GramCarrier.Immediate)]  static partial class NestedSealedBeforeParser;
[Gram(Grammars.LongLookBefore, Carrier = GramCarrier.Immediate)]      static partial class LongLookBeforeParser;
[Gram(Grammars.SealedCaptureBefore, Carrier = GramCarrier.Immediate)] static partial class SealedCaptureBeforeParser;
[Gram(Grammars.DecidingNestBefore, Carrier = GramCarrier.Immediate)]  static partial class DecidingNestBeforeParser;
[Gram(Grammars.SameBefore, Carrier = GramCarrier.Immediate)]          static partial class SameBeforeParser;
[Gram(Grammars.YamlLineBefore, Carrier = GramCarrier.Immediate)]      static partial class YamlLineBeforeParser;
[Gram(Grammars.DeepSplit)]                                            static partial class DeepSplitParser;
[Gram(Grammars.XmlNameGivesBack)]                                     static partial class XmlNameGivesBackParser;
[Gram(Grammars.TurnGivesBack)]                                        static partial class TurnGivesBackParser;
[Gram(Grammars.DeepSplitOpen)]                                        static partial class DeepSplitOpenParser;
[Gram(Grammars.Json)]                  static partial class JsonTwinParser;
