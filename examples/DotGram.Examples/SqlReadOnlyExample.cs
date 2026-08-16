using System;

using DotGram;

namespace DotGram.Examples;

// A guard that answers one question: can this statement write anything?
//
//     SqlReadOnly.IsReadOnly("select * from t where name = 'a''; drop table t --'")
//
// It is not a SQL parser and does not try to be. Whether the query is valid is the
// server's business — a guard that also validates has to know every dialect's syntax to
// avoid refusing queries that are perfectly good, and a guard people stop trusting is
// worse than no guard. This one reads the lexical structure exactly and then looks at
// the words that are actually words.
//
// The lexical part is where every known bypass lives, so it is the part that has to be
// right:
//
//   * `'…'` closes on a single quote, and `''` inside it is one quote rather than the
//     end — so `'a''; drop table t --'` is one string, and a reader that stops at the
//     second quote sees a statement separator that is not there.
//   * `"…"` and `[…]` are quoted identifiers: `insert` inside them is a column name.
//   * `--` runs to the end of the line and `/* … */` to its terminator, and a word
//     inside either is not a word at all.
//   * None of these start inside another: a `'` in a comment opens nothing, and `/*`
//     in a string opens nothing.
//
// What is left after that is a token stream, and the rule is a list of what is allowed
// rather than a list of what is not: the first word must be SELECT or WITH, and no word
// anywhere may be one that writes. Anything the guard does not recognise is refused,
// because the answer it exists to give is "this cannot write", and silence about a
// construct is not that answer.
//
// One consequence worth stating, because it looks like a hole and is not. An
// unterminated `/*` does not match the comment rule, so what follows is read as ordinary
// tokens — which means `select 1 /* unterminated` is allowed, since nothing in it writes,
// and `select 1 /* drop table t` is refused, since `drop` is then a word like any other.
// Refusing to close a comment buys an attacker nothing: it takes the hiding place away
// rather than extending it.
//
// SQL is case-insensitive, so each keyword is spelled as its letters in either case.
// That is what `Sel & Ect` below is doing — the language has no case-insensitive
// literal, and a rule per letter pair is the honest way to say it.

[Gram("""
	@using DotGram.Examples;

	// Every literal that is all word characters may not be the start of a longer word
	// (§4.6), so `into` does not match inside `into_stock`.
	KeywordBoundary = ['a'..'z' | 'A'..'Z' | '0'..'9' | '_' | '$']

	Trivia = none

	// A statement is read as tokens; what makes it read-only is which tokens are there.
	Query = Space & First & Token* & Space & ';'? & Space & eof

	// The only two ways a read-only statement may start.
	First = Select | With

	Token = Space & (String | Comment | Quoted | Bracketed | Word | Symbol) & Space

	// ── The lexical part, where the bypasses live ────────────────────────────────

	// `''` is an escaped quote, so it is part of the string rather than its end.
	String     = '\'' & ([^ '\''] | "''")* & '\''

	Quoted     = '"' & [^ '"']* & '"'
	Bracketed  = '[' & [^ ']']* & ']'

	Comment    = LineComment | BlockComment
	LineComment  = "--" & [^ '\n']*
	BlockComment = "/*" & (?!"*/" & any)* & "*/"

	Space      = [' ' | '\t' | '\r' | '\n']*

	// ── The words ────────────────────────────────────────────────────────────────

	// A word is anything word-shaped that is not one of the writing ones. The negative
	// lookahead is the whole guard: refused before it is ever read as a word.
	Word       = ?!Writes & ['a'..'z' | 'A'..'Z' | '_' | '$'] & ['a'..'z' | 'A'..'Z' | '0'..'9' | '_' | '$']*

	Symbol     = ['(' | ')' | ',' | '.' | '*' | '=' | '<' | '>' | '+' | '-' | '/' | '%' | '|' | ':' | '?' | '@' | '!' | '~' | '^' | '&']
	           | ['0'..'9'] & ['0'..'9' | '.' | 'e' | 'E' | '+' | '-']*

	// Anything that could write, or could hide a write behind it. A list of the refused,
	// which is safe only because everything not lexically a word is refused as well.
	Writes = Insert | Update | Delete | Merge | Into | Create | Drop | Alter
	       | Truncate | Grant | Revoke | Exec | Call | Set | Copy | Vacuum | Analyze

	// ── Keywords, spelled in either case ─────────────────────────────────────────

	A = ['a' | 'A']   B = ['b' | 'B']   C = ['c' | 'C']   D = ['d' | 'D']
	E = ['e' | 'E']   F = ['f' | 'F']   G = ['g' | 'G']   H = ['h' | 'H']
	I = ['i' | 'I']   K = ['k' | 'K']   L = ['l' | 'L']   M = ['m' | 'M']
	N = ['n' | 'N']   O = ['o' | 'O']   P = ['p' | 'P']   R = ['r' | 'R']
	S = ['s' | 'S']   T = ['t' | 'T']   U = ['u' | 'U']   V = ['v' | 'V']
	W = ['w' | 'W']   X = ['x' | 'X']   Y = ['y' | 'Y']   Z = ['z' | 'Z']

	Select   = S & E & L & E & C & T & ?!KeywordBoundary
	With     = W & I & T & H & ?!KeywordBoundary
	Insert   = I & N & S & E & R & T & ?!KeywordBoundary
	Update   = U & P & D & A & T & E & ?!KeywordBoundary
	Delete   = D & E & L & E & T & E & ?!KeywordBoundary
	Merge    = M & E & R & G & E & ?!KeywordBoundary
	Into     = I & N & T & O & ?!KeywordBoundary
	Create   = C & R & E & A & T & E & ?!KeywordBoundary
	Drop     = D & R & O & P & ?!KeywordBoundary
	Alter    = A & L & T & E & R & ?!KeywordBoundary
	Truncate = T & R & U & N & C & A & T & E & ?!KeywordBoundary
	Grant    = G & R & A & N & T & ?!KeywordBoundary
	Revoke   = R & E & V & O & K & E & ?!KeywordBoundary
	Exec     = E & X & E & C & (U & T & E)? & ?!KeywordBoundary
	Call     = C & A & L & L & ?!KeywordBoundary
	Set      = S & E & T & ?!KeywordBoundary
	Copy     = C & O & P & Y & ?!KeywordBoundary
	Vacuum   = V & A & C & U & U & M & ?!KeywordBoundary
	Analyze  = A & N & A & L & Y & (Z | S) & E & ?!KeywordBoundary

	parse Query
	""")]
public sealed partial class SqlReadOnly
{
	/// <summary>
	/// Whether this statement can only read.
	/// </summary>
	/// <remarks>
	/// False is "cannot be shown to be read-only", which includes a query that is not
	/// valid SQL at all. That direction is the safe one: the cost of refusing something
	/// harmless is a message, and the cost of admitting something harmful is the thing
	/// this exists to prevent.
	/// </remarks>
	public static bool IsReadOnly(string statement) => TryParseQuery(statement).IsSuccess;
}
