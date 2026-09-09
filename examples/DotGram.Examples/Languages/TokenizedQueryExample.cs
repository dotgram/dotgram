using System;

using DotGram;

namespace DotGram.Examples.Languages;

// A query language read over tokens rather than over characters:
//
//     select a, b from users where a = 1
//     SELECT id FROM t -- a comment
//
// `Lexical = true` cuts the grammar in two. A lexical half turns the text into tokens,
// and the syntactic half above it reads those — which is how a parser is written by hand,
// and it changes two things a reader should know about.
//
//   The lexer settles the longest match. `<>` is one token because the lexical rules say
//   so, not because an alternative was ordered before another and backtracking found it.
//   Over characters, `"<>" | "<"` has to be written in that order and means something
//   subtly different; here the order of those two alternatives does not matter at all.
//
//   A choice, once matched, is not revisited. The syntactic half decides by the token in
//   front of it and never goes back — again as a hand-written parser does. Over
//   characters a failure further on returns and tries the next alternative.
//
// What makes the cut possible is two things and nothing else (docs/syntax.md §6.3):
// a namespace whose own `trivia` is `none`, so a token is its characters with nothing
// between them, and a `trivia` in braces outside it, so the seam between tokens is
// skipped by the scanner rather than woven into the rules. Where the grammar cannot be
// cut, GRAM5004 says so and the parser is the one it would have been anyway.
//
// The keywords are word literals and `wordboundary` is declared, so `selectx` is an
// identifier and not `select` followed by `x` (§4.6).
//
// One thing this mode does not do: read from a `TextReader`. A token parse holds its
// input in memory.

/// <summary>What a query says, once it has been read.</summary>
public sealed record Select(string[] Columns, string Table, Comparison? Where);

/// <summary>The one comparison a `where` may carry here.</summary>
public sealed record Comparison(string Column, string Op, string Value);

[Gram("""
	@using DotGram.Examples.Languages;

	using Lex;

	// A token is its characters and nothing between them, which is what `trivia = none`
	// says. Everything in here is a lexeme.
	namespace Lex
	{
		trivia = none

		Name   = [\p{L} | '_'] & [\p{L} | \p{Nd} | '_']*
		Number = ['0'..'9']+
		Text   = '\'' & ("''" | [^ '\''])* & '\''
	}

	// In braces, so the scanner skips the seam rather than the rules weaving it.
	trivia = { (Whitespace | Comment)* }

	Whitespace = [' ' | '\t' | '\r' | '\n']+
	Comment    = "--" & [^ '\r' | '\n']*

	// What continues a word, so a keyword has to end where one ends.
	wordboundary = [\p{L} | \p{Nd} | '_']

	Query : @Select
		= "select"i & columns: Columns & "from"i & table: Name & where: Where? & eof
		=> @(new Select(columns, table, where))

	Columns : @string[] = Column & (',' & Column)*
	Column  : @string   = n: Name => @(n)

	Where : @Comparison
		= "where"i & column: Name & op: Operator & value: Value
		=> @(new Comparison(column, op, value))

	// The lexer decides how far each of these reaches, so the order they are written in
	// says nothing. Over characters it would.
	Operator : @string = t: ("=" | "<>" | "<" | ">") => @(t)
	Value    : @string = t: (Number | Text)          => @(t)

	parse Query
	""", Lexical = true)]
public static partial class TokenizedQuery
{
	/// <summary>A query written back out, for somebody to read.</summary>
	public static string Describe(string text)
	{
		var query = TryParseQuery(text);

		if (!query.IsSuccess)
			return query.Error + " at " + query.Position;

		var select = query.Value!;

		return string.Join(", ", select.Columns) + " from " + select.Table +
			(select.Where is { } where ? $" where {where.Column} {where.Op} {where.Value}" : "");
	}
}
