using System;

using DotGram;

namespace DotGram.Examples;

// The grammar of `.gram` itself, written in `.gram`, building a tree.
//
//     GramGrammar.ParseFile(File.ReadAllText("Url.gram"))
//
// docs/syntax.md §10 used to carry a sketch of this and call it "a consistency check". It
// never was one: as written it named seven things it did not define, left comments out of
// the language, omitted `@(...)` from `Primary` although the hand-written parser accepts
// it there, and wrote its separated lists in the form §4.5 now warns about. This is that
// sketch made to run, checked against every `.gram` in the repository.
//
// Four things in it are worth looking at:
//
//   * `wordboundary` (§4.6) is what keeps `parse` from matching the start of a rule named
//     `parseHeader`. Without it every keyword here would need a hand-written check, and
//     the order of the alternatives would start to matter.
//
//   * The lexical rules live in a namespace with `trivia = none`, so an identifier is
//     letters with nothing allowed between them, while everything outside it may be
//     spaced and commented freely (§4.5). One declaration, and no rule below it mentions
//     whitespace.
//
//   * Every repetition here writes `trivia` by hand: `(trivia & '|' & ElemAlt)*`. §4.5
//     inserts trivia between the operands of a sequence and nowhere else, so the turns of
//     a repetition get none — deliberately, since that is what keeps `['0'..'9']+` from
//     reading `1 2` as one number. A spaced list has to say so, and this one is spaced.
//
//   * `@(...)` holds C#, and finding its closing parenthesis means knowing C#'s own
//     strings and comments. No grammar can do that, and this one does not try: `@CSharp`
//     is an ordinary external recognizer (§7.1, second row) that reads the input itself
//     and says how far it got. That seam is why the generator needs no runtime.
//
// What the tree is and is not. It is faithful about structure — what nests inside what,
// in the order written — and deliberately shallow about leaves: a literal keeps the text
// between its quotes rather than a decoded value, and the items of an element set are kept
// as written. Decoding those is `GramLexer`'s job, and repeating it here would say nothing
// about the notation.

[Gram("""
	@using DotGram.Examples;

	using Lexical;

	namespace Lexical
	{
		// Nothing between the characters of a token: `a b` is two identifiers, and 1 2 is
		// two numbers rather than one.
		trivia = none

		Word         = [\p{L} | '_']
		WordOrDigit  = [\p{L} | \p{Nd} | '_']

		Identifier   = Word & WordOrDigit*
		Name         = Identifier & ('.' & Identifier)*
		Int          = ['0'..'9']+

		Hex          = ['0'..'9' | 'a'..'f' | 'A'..'F']
		Escape       = '\\' & (['\'' | '"' | '\\' | '0' | 'a' | 'b' | 'f' | 'n' | 'r' | 't' | 'v'] | 'u' & Hex{4})

		// A trailing `i` is the case-insensitive marker (§3.1), and only where a word does
		// not carry on: `"text"id` is a string and then the identifier `id`.
		Insensitive  = 'i' & ?!WordOrDigit

		Char         = '\'' & (Escape | [^ '\'' | '\\']) & '\'' & Insensitive?
		String       = '"' & (Escape | [^ '"' | '\\'])* & '"' & Insensitive?

		Category     = "\\p{" & Identifier & '}'

		Space        = [' ' | '\t' | '\r' | '\n']
		LineComment  = "//" & [^ '\n' | '\r']*
		BlockComment = "/*" & (?!"*/" & any)* & "*/"
	}

	// So that `parse` does not match the start of a rule called `parseHeader` (§4.6). Every
	// keyword below is a plain string literal because of this line.
	wordboundary = WordOrDigit

	trivia = (Space | LineComment | BlockComment)*

	File : @GramFile
		= (trivia & usings: Using)* & (trivia & declarations: Declaration)*
		=> @(new GramFile(usings, declarations))

	// `@using` and `using` mean the same thing: one is C#'s vocabulary, the other this
	// language's own (§2).
	Using : @GramUsing = '@'? & "using" & name: Name & ';' => @(new GramUsing(name))

	Declaration : @GramDecl
		= d: Namespace   => @(d)
		| d: Publication => @(d)
		| d: Rule        => @(d)

	Namespace : @GramDecl
		= "namespace" & name: Identifier & With?
		& '{' & (trivia & usings: Using)* & (trivia & declarations: Declaration)* & '}'
		=> @(new GramNamespace(name, usings, declarations))

	Publication : @GramDecl
		= kind: ("parse" | "find") & rule: Name & With? & ("as" & alias: Identifier)?
		=> @(new GramPublication(kind, rule, alias))

	Rule : @GramDecl
		= name: Identifier & Parameters? & (':' & type: Type)? & '=' & body: Body
		=> @(new GramRule(name, type, body))

	Parameters     = '(' & (Parameter & (trivia & ',' & Parameter)*)? & ')'
	Parameter      = Identifier & (':' & Type)?
	Type : @string = text: (Reference & "[]"?) => @(text)

	With           = "with" & Rebindings
	Rebindings     = '(' & (Rebinding & (trivia & ',' & Rebinding)*)? & ')'
	Rebinding      = Identifier & '=' & Identifier

	Body : @GramExpr
		= first: Alternative & (trivia & '|' & rest: Alternative)*
		=> @(GramGrammar.Choice(first, rest))

	Alternative : @GramExpr
		= body: Sequence & Binding? & ("=>" & value: Value)?
		=> @(value is null ? body : new GramConstruct(body, value))

	Binding        = ("<<" | ">>") & Int

	Sequence : @GramExpr
		= first: Operand & (trivia & '&' & rest: Operand)*
		=> @(GramGrammar.Sequence(first, rest))

	Operand : @GramExpr = o: Guard => @(o) | o: Quantified => @(o)

	Guard : @GramExpr = "when" & value: Value => @(new GramGuard(value))

	// `with` last, outermost of the three: `Number+ with (X = Y)` is `(Number+) with
	// (X = Y)`, and the other reading needs parentheses.
	Quantified : @GramExpr
		= body: Prefixed & quantifier: Quantifier? & recovery: Recovery? & rebound: With?
		=> @(GramGrammar.Quantified(body, quantifier, recovery, rebound))

	Quantifier : @string
		= text: ('?' | '*' | '+' | '{' & Count & (trivia & ',' & Count?)? & '}')
		=> @(text)

	Recovery : @GramExpr = "recover" & sync: Prefixed & ("=>" & Value)? => @(sync)

	Count          = Int | Identifier

	Prefixed : @GramExpr
		= prefix: ("?=" | "?!")? & body: Captured
		=> @(prefix is null ? body : new GramLookahead(prefix == "?=", body))

	Captured : @GramExpr
		= (name: Identifier & ':')? & body: Primary
		=> @(name is null ? body : new GramCapture(name, body))

	Primary : @GramExpr
		= text: Char             => @(new GramLiteral(text, true))
		| text: String           => @(new GramLiteral(text, false))
		| e: ElementSet          => @(e)
		| cs: CsExpr             => @(new GramCSharp(cs))
		| e: Call                => @(e)
		| e: Reference           => @(e)
		| '(' & body: Body & ')' => @(body)
		| '{' & body: Body & '}' => @(new GramGroup(body, true))

	Value : @string  = text: (CsExpr | Call | Reference) => @(text)
	CsExpr : @string = text: @CSharp => @(text)

	// Longest first: a call is a reference and then an argument list, so a bare reference
	// tried first would take the name and leave the parenthesis behind.
	Call : @GramExpr
		= target: Reference & '(' & (first: Argument & (trivia & ',' & rest: Argument)*)? & ')'
		=> @(GramGrammar.Call(target, first, rest))

	Argument : @GramExpr = i: Int => @(new GramRef(i)) | a: Alternative => @(a)

	Reference : @GramExpr = text: ('@'? & Name & TypeArgs?) => @(new GramRef(text))

	TypeArgs       = '<' & Type & (trivia & ',' & Type)* & '>'

	ElementSet : @GramExpr
		= '[' & negated: '^'? & first: ElemAlt & (trivia & '|' & rest: ElemAlt)* & ']'
		=> @(GramGrammar.Set(negated, first, rest))

	ElemAlt : @string = text: (Char & (".." & Char)? | Category | Reference) => @(text)

	parse File
	""")]
public partial class GramGrammar
{
	// ── What the grammar builds ──────────────────────────────────────────────────

	public sealed record GramFile(GramUsing[] Usings, GramDecl[] Declarations);

	public sealed record GramUsing(string Name);

	public abstract record GramDecl;

	public sealed record GramNamespace(string Name, GramUsing[] Usings, GramDecl[] Declarations) : GramDecl;

	public sealed record GramPublication(string Kind, string Rule, string? Alias) : GramDecl;

	public sealed record GramRule(string Name, string? Type, GramExpr Body) : GramDecl;

	public abstract record GramExpr;

	public sealed record GramChoice(GramExpr[] Alternatives) : GramExpr;

	public sealed record GramSequence(GramExpr[] Operands) : GramExpr;

	/// <summary>A quantifier, a <c>recover</c> or a <c>with</c> wrapped around an operand.</summary>
	public sealed record GramQuantified(GramExpr Body, string? Quantifier, GramExpr? Recovery, bool Rebound) : GramExpr;

	public sealed record GramCapture(string Name, GramExpr Body) : GramExpr;

	public sealed record GramLookahead(bool Positive, GramExpr Body) : GramExpr;

	/// <summary>The literal as written, quotes and escapes and all — see the note above.</summary>
	public sealed record GramLiteral(string Text, bool IsCharacter) : GramExpr;

	public sealed record GramSet(bool Negated, string[] Items) : GramExpr;

	public sealed record GramRef(string Name) : GramExpr;

	public sealed record GramCall(GramExpr Target, GramExpr[] Arguments) : GramExpr;

	public sealed record GramCSharp(string Text) : GramExpr;

	public sealed record GramGroup(GramExpr Body, bool Atomic) : GramExpr;

	public sealed record GramConstruct(GramExpr Body, string Value) : GramExpr;

	public sealed record GramGuard(string Value) : GramExpr;

	// ── The factories the grammar calls ──────────────────────────────────────────

	/// <summary>
	/// A rule with one alternative is that alternative, not a choice of one.
	/// </summary>
	/// <remarks>
	/// The notation has no way to write a choice of one, so building one would put a node
	/// in the tree that nothing in the text asked for. The same goes for a sequence, a
	/// quantifier that is not there, and a lookahead that is not there.
	/// </remarks>
	public static GramExpr Choice(GramExpr first, GramExpr[] rest) =>
		rest.Length == 0 ? first : new GramChoice(Joined(first, rest));

	public static GramExpr Sequence(GramExpr first, GramExpr[] rest) =>
		rest.Length == 0 ? first : new GramSequence(Joined(first, rest));

	public static GramExpr Quantified(GramExpr body, string? quantifier, GramExpr? recovery, string? rebound) =>
		quantifier is null && recovery is null && rebound is null
			? body
			: new GramQuantified(body, quantifier, recovery, rebound is not null);

	public static GramExpr Call(GramExpr target, GramExpr? first, GramExpr[] rest) =>
		new GramCall(target, first is null ? [] : Joined(first, rest));

	public static GramExpr Set(string? negated, string first, string[] rest) =>
		new GramSet(negated is not null, Joined(first, rest));

	static T[] Joined<T>(T first, T[] rest)
	{
		var all = new T[rest.Length + 1];

		all[0] = first;
		rest.CopyTo(all, 1);

		return all;
	}

	// ── The one thing no grammar can read ────────────────────────────────────────

	/// <summary>
	/// <c>@(</c> through the parenthesis that closes it, which means reading past C#'s own
	/// strings, characters and comments — a <c>)</c> inside <c>")"</c> closes nothing.
	/// </summary>
	/// <remarks>
	/// §7.1's second row: a bare <c>@CSharp</c> stands where an operand goes, reads what it
	/// likes, and moves the position to say how much it took. Saying no is an ordinary
	/// non-match, so the grammar around it carries on as it would from any other failure.
	/// </remarks>
	static bool CSharp(ReadOnlySpan<char> input, ref int pos)
	{
		if (pos + 1 >= input.Length || input[pos] != '@' || input[pos + 1] != '(')
			return false;

		var at    = pos + 2;
		var depth = 1;

		while (at < input.Length)
		{
			var c = input[at];

			switch (c)
			{
				case '(':
					depth++;
					at++;
					break;

				case ')':
					depth--;
					at++;

					if (depth == 0)
					{
						pos = at;

						return true;
					}

					break;

				case '"':
				case '\'':
					at = Quoted(input, at, c);
					break;

				case '/' when at + 1 < input.Length && input[at + 1] == '/':
					while (at < input.Length && input[at] is not ('\n' or '\r'))
						at++;
					break;

				case '/' when at + 1 < input.Length && input[at + 1] == '*':
					at += 2;

					while (at + 1 < input.Length && !(input[at] == '*' && input[at + 1] == '/'))
						at++;

					at = at + 1 < input.Length ? at + 2 : input.Length;
					break;

				default:
					at++;
					break;
			}
		}

		// Ran out of input with the parenthesis still open: not a match, and the position
		// stays where it was.
		return false;
	}

	/// <summary>Past a C# string or character literal, escapes and all.</summary>
	static int Quoted(ReadOnlySpan<char> input, int at, char quote)
	{
		at++;

		while (at < input.Length)
		{
			if (input[at] == '\\' && at + 1 < input.Length)
			{
				at += 2;

				continue;
			}

			if (input[at] == quote)
				return at + 1;

			at++;
		}

		return at;
	}

	/// <summary>Whether <paramref name="text"/> is a whole, well-formed grammar.</summary>
	public static bool IsGrammar(string text) => TryParseFile(text).IsSuccess;
}
