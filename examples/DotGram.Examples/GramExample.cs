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
//   * No repetition writes `trivia` out, and §4.5 says why not. A repetition of a
//     sequence is spaced by itself — `('|' & ElemAlt)*`, `(',' & Type)* ` — because
//     the turns are a seam between operands like any other; a repetition of a valued
//     rule is a collection, and collections are spaced the way operands are —
//     `declarations: Declaration*` reads the newlines between declarations without
//     naming them. What stays unspaced is the valueless run — the inside of a lexeme —
//     which is what keeps `['0'..'9']+` from reading `1 2` as one number.
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

	// Atomic, and the braces are doing real work twice over. Semantically they say that
	// what a comment swallowed stays swallowed: without them §11's ordered choice lets a
	// failing parse re-read a comment's interior as syntax, one give-back at a time.
	// Mechanically that same commitment is what lets every spaced list below keep a single
	// way back instead of one per element, which is the difference between failing in
	// milliseconds and failing in minutes.
	trivia = { (Space | LineComment | BlockComment)* }

	// Out here rather than in Lexical, deliberately: a qualified name is syntax made of
	// identifier tokens, so `A . B` reads the way the hand-written parser has always read
	// it — dots are punctuation, spaces around them are legal.
	Name = Identifier & ('.' & Identifier)*

	File : @GramFile
		= usings: Using* & declarations: Declaration*
		=> @(new GramFile(usings, declarations))

	// `@using` and `using` mean the same thing: one is C#'s vocabulary, the other this
	// language's own (§2).
	Using : @GramUsing = '@'? & "using" & name: Name & ';' => @(new GramUsing(name))

	// `context : @T` and `state : @T` before the rules, because a rule may be called
	// either name: what tells a declaration from one is that a declaration stops at the
	// type, and a rule goes on to `=`. Ordered choice asks that by trying.
	Declaration : @GramDecl
		= d: Supplied    => @(d)
		| d: Namespace   => @(d)
		| d: Publication => @(d)
		| d: Rule        => @(d)

	// §7.7 and §7.8. One shape, because they are one shape: a word, a colon, a type,
	// and nothing after it.
	Supplied : @GramDecl
		= kind: ("context" | "state") & ':' & type: Type & ?!'='
		=> @(new GramSupplied(kind, type))

	Namespace : @GramDecl
		= "namespace" & name: Identifier & With?
		& '{' & usings: Using* & declarations: Declaration* & '}'
		=> @(new GramNamespace(name, usings, declarations))

	// What a directive names is an expression, not only a rule (§6) — one operand, so
	// the `with` that may follow is the directive's own rather than the operand's, and
	// the type is the third part a rule has, in the place that reads as the method's.
	Publication : @GramDecl
		= kind: ("parse" | "find") & target: QuantifiedCore & With?
		& ("as" & alias: Identifier & (':' & type: Type)?)?
		=> @(new GramPublication(kind, target, alias, type))

	Rule : @GramDecl
		= name: Identifier & Parameters? & (':' & type: Type)? & '=' & body: Body
		=> @(new GramRule(name, type, body))

	Parameters     = '(' & (Parameter & (',' & Parameter)*)? & ')'
	Parameter      = Identifier & (':' & Type)?
	Type : @string = text: (Reference & "[]"?) => @(text)

	// §7.8's mark reuses the word and is told from a rebinding by the token after it:
	// `(` opens a rebinding, and `state` can only be the other.
	With           = "with" & Rebindings
	Marking : @string = "with" & "state" & value: Value => @(value)
	Rebindings     = '(' & (Rebinding & (',' & Rebinding)*)? & ')'
	// §5.1's other half: what replaces a rule may be any operand. The left stays an
	// identifier — it opens what is being replaced, and opening is what a name is for.
	Rebinding      = Identifier & '=' & QuantifiedCore

	Body : @GramExpr
		= first: Alternative & ('|' & rest: Alternative)*
		=> @(GramGrammar.Choice(first, rest))

	Alternative : @GramExpr
		= body: Sequence & Binding? & ("=>" & value: Value)?
		=> @(value is null ? body : new GramConstruct(body, value))

	Binding        = ("<<" | ">>") & Int

	Sequence : @GramExpr
		= first: Operand & ('&' & rest: Operand)*
		=> @(GramGrammar.Sequence(first, rest))

	Operand : @GramExpr = o: Guard => @(o) | o: Quantified => @(o)

	Guard : @GramExpr = "when" & value: Value => @(new GramGuard(value))

	// `with` last, outermost of the three: `Number+ with (X = Y)` is `(Number+) with
	// (X = Y)`, and the other reading needs parentheses.
	// Both suffixes, repeated: they are different mechanisms on the same operand and
	// either may want the other around it, so what is written first ends up innermost.
	Quantified : @GramExpr
		= body: QuantifiedCore & rebound: With* & mark: Marking*
		=> @(GramGrammar.Marked(GramGrammar.Rebound(body, rebound), mark))

	// Named because two other places take exactly this and stop short of the `with`: a
	// directive's target and a rebinding's replacement, where a following `with` belongs
	// to the thing around the operand rather than to the operand.
	QuantifiedCore : @GramExpr
		= body: Prefixed & quantifier: Quantifier? & recovery: Recovery?
		=> @(GramGrammar.Quantified(body, quantifier, recovery))

	Quantifier : @string
		= text: ('?' | '*' | '+' | '{' & Count & (',' & Count?)? & '}')
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
		| e: RefOrCall           => @(e)
		| '(' & body: Body & ')' => @(body)
		| '{' & body: Body & '}' => @(new GramGroup(body, true))

	Value : @string  = text: (CsExpr | RefOrCall) => @(text)
	// The lookahead is the recognizer's first set, said in the grammar: §7.1 gives an
	// external no first set of its own, so without it every reference paid a
	// speculative call into the C# scanner just to hear "no". The hand-written lexer
	// makes the same two-character check before reading an expression.
	CsExpr : @string = ?="@(" & text: @CSharp => @(text)

	// One parse of the name, whatever follows it. Written `Call | Reference` this read
	// every bare reference twice — once inside the failing `Call`, once as itself — and
	// references are most of what a grammar is made of. The hand-written parser makes the
	// same move under the name `ParseReferenceOrCall`; §11's ordered choice is not
	// obliged to be spelled with the prefix shared.
	RefOrCall : @GramExpr
		= target: Reference & (open: '(' & (first: Argument & (',' & rest: Argument)*)? & ')')?
		=> @(open is null ? target : GramGrammar.Call(target, first, rest))

	Argument : @GramExpr = i: Int => @(new GramRef(i)) | a: Alternative => @(a)

	Reference : @GramExpr = text: ('@'? & Name & TypeArgs?) => @(new GramRef(text))

	TypeArgs       = '<' & Type & (',' & Type)* & '>'

	ElementSet : @GramExpr
		= '[' & negated: '^'? & first: ElemAlt & ('|' & rest: ElemAlt)* & ']'
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

	public sealed record GramPublication(string Kind, GramExpr Target, string? Alias, string? Type) : GramDecl;

	public sealed record GramRule(string Name, string? Type, GramExpr Body) : GramDecl;

	/// <summary>A `context : @T` or a `state : @T` — §7.7 and §7.8.</summary>
	public sealed record GramSupplied(string Kind, string Type) : GramDecl;

	public abstract record GramExpr;

	public sealed record GramChoice(GramExpr[] Alternatives) : GramExpr;

	public sealed record GramSequence(GramExpr[] Operands) : GramExpr;

	/// <summary>A quantifier, a <c>recover</c> or a <c>with</c> wrapped around an operand.</summary>
	public sealed record GramQuantified(GramExpr Body, string? Quantifier, GramExpr? Recovery, bool Rebound) : GramExpr;

	/// <summary>An operand under a `with state` mark (§7.8).</summary>
	public sealed record GramMarked(GramExpr Body, string Value) : GramExpr;

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

	public static GramExpr Quantified(GramExpr body, string? quantifier, GramExpr? recovery) =>
		quantifier is null && recovery is null
			? body
			: new GramQuantified(body, quantifier, recovery, false);

	/// <summary>The `with` suffixes that may follow an operand, where any do (§5.1).</summary>
	public static GramExpr Rebound(GramExpr body, string? rebound) =>
		string.IsNullOrEmpty(rebound)
			? body
			: body is GramQuantified quantified
				? quantified with { Rebound = true }
				: new GramQuantified(body, null, null, true);

	/// <summary>The `with state` marks that may follow an operand, where any do (§7.8).</summary>
	public static GramExpr Marked(GramExpr body, string[] mark)
	{
		foreach (var value in mark)
			body = new GramMarked(body, value);

		return body;
	}

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
