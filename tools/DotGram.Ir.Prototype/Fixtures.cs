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
[Gram(Grammars.Json)]                  static partial class JsonTwinParser;
