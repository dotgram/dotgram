using System;

using DotGram;

namespace DotGram.Examples.Languages;

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
// SQL is case-insensitive, so each keyword below is written `"select"i` rather than
// `"select"`. §4.6's automatic keyword boundary still applies to a case-insensitive
// literal exactly as it does to an ordinary one: every character of `"select"i`
// continues a word under this grammar's own `wordboundary`, so `Select` gets the
// `?!wordboundary` check without asking for it, and `into` still does not match inside
// `into_stock`.

[Gram("""
	@using DotGram.Examples.Languages;

	// Every literal that is all word characters may not be the start of a longer word
	// (§4.6), so `into` does not match inside `into_stock`.
	wordboundary = ['a'..'z' | 'A'..'Z' | '0'..'9' | '_' | '$']

	trivia = none

	// A statement is read as tokens; what makes it read-only is which tokens are there.
	Query = Space & First & { Token* } & Space & ';'? & Space & eof

	// The only two ways a read-only statement may start.
	First = Select | With

	Token = Space & (String | Comment | Quoted | Bracketed | Word | Symbol) & Space

	// ── The lexical part, where the bypasses live ────────────────────────────────

	// `''` is an escaped quote, so it is part of the string rather than its end.
	//
	// Atomic, as the comments and words are: a string runs to the quote that ends it, and a doubled
	// quote inside it is never the end of one string and the start of the next.
	String     = '\'' & { ([^ '\''] | "''")* } & '\''

	Quoted     = '"' & [^ '"']* & '"'
	Bracketed  = '[' & [^ ']']* & ']'

	Comment    = LineComment | BlockComment
	LineComment  = "--" & { [^ '\n']* }
	BlockComment = "/*" & (?!"*/" & any)* & "*/"

	// Atomic: the blanks between two tokens belong to the one before or to the one after, and
	// given back they would be both, once for each blank, in every gap of a text refused late.
	Space      = { [' ' | '\t' | '\r' | '\n']* }

	// ── The words ────────────────────────────────────────────────────────────────

	// A word is anything word-shaped that is not one of the writing ones. The negative
	// lookahead is the whole guard: refused before it is ever read as a word.
	//
	// Atomic, as a number is below: a word is read to its end. Given back, `abc` would also be `a`
	// and `bc`, or `ab` and `c`, and a text refused late would be read every way its words could
	// be cut.
	Word       = { ?!Writes & ['a'..'z' | 'A'..'Z' | '_' | '$'] & ['a'..'z' | 'A'..'Z' | '0'..'9' | '_' | '$']* }

	Symbol     = ['(' | ')' | ',' | '.' | '*' | '=' | '<' | '>' | '+' | '%' | '|' | ':' | '?' | '@' | '!' | '~' | '^' | '&']
	           | Minus | Slash
	           | { ['0'..'9'] & (['0'..'9' | '.' | 'e' | 'E' | '+'] | Minus)* }

	// A minus that starts a line comment, or a slash that starts a block comment that ends, is
	// that comment and never a symbol. Left to read either way, one comment opener could be
	// reread inside another comment (`/* -- */ insert` as a block comment, or as `/*` and a line
	// comment), and `-- ' ...` could be two symbols and a string that runs on to a quote on a
	// later line; both hide the rest of the statement from this check but not from the server,
	// which reads the first reading. A text refused late was also read as every way of choosing
	// between the two for each comment in it. A block comment that never ends is not one, and is
	// read as the symbols and words it is made of.
	Minus      = '-' & ?!'-'
	Slash      = ?!BlockComment & '/'

	// Anything that could write, or could hide a write behind it. A list of the refused,
	// which is safe only because everything not lexically a word is refused as well.
	Writes = Insert | Update | Delete | Merge | Into | Create | Drop | Alter
	       | Truncate | Grant | Revoke | Exec | Call | Set | Copy | Vacuum | Analyze

	// ── Keywords, matched without regard to case ─────────────────────────────────

	Select   = "select"i
	With     = "with"i
	Insert   = "insert"i
	Update   = "update"i
	Delete   = "delete"i
	Merge    = "merge"i
	Into     = "into"i
	Create   = "create"i
	Drop     = "drop"i
	Alter    = "alter"i
	Truncate = "truncate"i
	Grant    = "grant"i
	Revoke   = "revoke"i
	Call     = "call"i
	Set      = "set"i
	Copy     = "copy"i
	Vacuum   = "vacuum"i

	// Each alternative is a complete spelling, so each gets its own trailing boundary
	// check — writing this as one literal with an optional suffix would put the check
	// right after "exec", refusing "execute" before the optional part was even tried.
	Exec     = "execute"i | "exec"i
	Analyze  = "analyze"i | "analyse"i

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
	public static bool IsReadOnly(string statement)
	{
		return TryParseQuery(statement).IsSuccess;
	}
}
