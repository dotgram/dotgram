using System;

using DotGram;

namespace DotGram.Examples;

// The grammar of `.gram` itself, written in `.gram`.
//
//     GramGrammar.IsGrammar(File.ReadAllText("Url.gram"))
//
// docs/syntax.md §10 carries a sketch of this and calls it "a consistency check". It was
// never one: as written it names seven things it does not define — `Identifier`,
// `QualifiedName`, `Int`, `Char`, `String`, `Balanced` and any notion of trivia — leaves
// comments out entirely, and omits `@(...)` from `Primary` although the hand-written
// parser accepts it there. This is that sketch made to run, checked against every `.gram`
// in the repository.
//
// Three things in it are worth looking at:
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
// What this is not: it recognizes, and builds nothing. Comparing a tree it built against
// the one `GramParser` builds is a larger question — the two would have to agree on a
// shape neither has a reason to — and is a separate piece of work.

[Gram("""
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

	File         = (trivia & Using)* & (trivia & Declaration)*

	// `@using` and `using` mean the same thing: one is C#'s vocabulary, the other this
	// language's own (§2).
	Using        = '@'? & "using" & Name & ';'

	Declaration  = Namespace | Publication | Rule

	Namespace    = "namespace" & Identifier & With? & '{' & (trivia & Using)* & (trivia & Declaration)* & '}'
	Publication  = ("parse" | "find") & Name & With? & ("as" & Identifier)?
	Rule         = Identifier & Parameters? & (':' & Type)? & '=' & Body

	Parameters   = '(' & (Parameter & (trivia & ',' & Parameter)*)? & ')'
	Parameter    = Identifier & (':' & Type)?
	Type         = Reference & "[]"?

	With         = "with" & Rebindings
	Rebindings   = '(' & (Rebinding & (trivia & ',' & Rebinding)*)? & ')'
	Rebinding    = Identifier & '=' & Identifier

	Body         = Alternative & (trivia & '|' & Alternative)*
	Alternative  = Sequence & Binding? & ("=>" & Value)?
	Binding      = ("<<" | ">>") & Int
	Sequence     = Operand & (trivia & '&' & Operand)*
	Operand      = Guard | Quantified
	Guard        = "when" & Value

	// `with` last, outermost of the three: `Number+ with (X = Y)` is `(Number+) with
	// (X = Y)`, and the other reading needs parentheses.
	Quantified   = Prefixed & Quantifier? & Recovery? & With?
	Quantifier   = '?' | '*' | '+' | '{' & Count & (',' & Count?)? & '}'
	Recovery     = "recover" & Prefixed & ("=>" & Value)?
	Count        = Int | Identifier

	Prefixed     = ("?=" | "?!")? & Captured
	Captured     = (Identifier & ':')? & Primary

	Primary      = Char | String | ElementSet | CsExpr | Call | Reference
	             | '(' & Body & ')' | '{' & Body & '}'

	Value        = CsExpr | Call | Reference
	CsExpr       = @CSharp

	// Longest first: a call is a reference and then an argument list, so a bare reference
	// tried first would take the name and leave the parenthesis behind.
	Call         = Reference & '(' & (Argument & (trivia & ',' & Argument)*)? & ')'
	Argument     = Int | Alternative
	Reference    = '@'? & Name & TypeArgs?
	TypeArgs     = '<' & Type & (trivia & ',' & Type)* & '>'

	ElementSet   = '[' & '^'? & ElemAlt & (trivia & '|' & ElemAlt)* & ']'
	ElemAlt      = Char & (".." & Char)? | Category | Reference

	parse File
	""")]
public partial class GramGrammar
{
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
	/// <remarks>
	/// The value is the text it covered — a rule that captures nothing has no other — so
	/// the answer is in whether it matched at all, not in what came back.
	/// </remarks>
	public static bool IsGrammar(string text) => TryParseFile(text).IsSuccess;
}
