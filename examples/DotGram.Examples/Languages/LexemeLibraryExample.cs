using System;

using DotGram;

namespace DotGram.Examples.Languages;

// One grammar written to be built on, and two written on it.
//
// `Lexemes` says what a word, a number and a quoted string look like. It is not a parser
// of anything: it is the vocabulary two other grammars share, and the point of the
// example is that they share it without either of them copying it.
//
//     [GramInclude(typeof(Lexemes), As = "Lex")]
//
// `As` is the *includer's* name for it, not the library's — the same library arrives as
// `Lex` in one grammar and `Token` in the other, and neither had to be told. Each include
// is spliced into a namespace of its own, which is the whole of why names cannot collide:
// `Settings` uses `Lex.Word`, and `Filter` declares a `Word` of its own beside
// `Token.Word` with nothing to settle between them.
//
// What travels is the grammar, not a parser. Across a project reference the generator
// reads the text off the class it was compiled into and compiles it again here, under
// this grammar's own substitutions — which is what makes a dialect the size of its
// difference rather than a fork. `DotGram.Parsers` is the real one: T-SQL is written as
// SQL-92 and the places it parts from it.
//
// One thing to know before writing a library. A rule an including grammar never reaches
// is reported to *that* grammar as GRAM4018, so a library of forty lexemes warns in every
// consumer that uses three. The standard library `Std` is exempt; an included grammar is
// not. Keep a library to what its consumers use, or expect to answer for the rest.

/// <summary>A setting read out of a settings file.</summary>
public sealed record Setting(string Key, string Value);

/// <summary>One test of a filter: a field, a comparison and a value.</summary>
public sealed record FilterTest(string Field, string Op, string Value);

// ── The library ──────────────────────────────────────────────────────────────

// No trivia: a lexeme is its characters with nothing between them. Each rule is published
// as well, so the vocabulary can be used on its own and so nothing here goes uncompiled.
[Gram("""
	trivia = none

	Word   = [\p{L} | '_'] & [\p{L} | \p{Nd} | '_' | '-']*
	Number = ['0'..'9']+
	Quoted = '"' & ("\"\"" | [^ '"'])* & '"'

	parse Word
	parse Number
	parse Quoted
	""")]
public static partial class Lexemes;

// ── One grammar written on it ────────────────────────────────────────────────

[GramInclude(typeof(Lexemes), As = "Lex")]
[Gram("""
	@using DotGram.Examples.Languages;

	trivia = [' ' | '\t']*

	Setting : @Setting
		= key: Lex.Word & '=' & value: (Lex.Number | Lex.Quoted | Lex.Word)
		=> @(new Setting(key, value))

	File : @Setting[] = (Setting & eol)* & eof

	parse File as ParseSettings
	""")]
public static partial class SettingsFile;

// ── And another, under a different name ──────────────────────────────────────

[GramInclude(typeof(Lexemes), As = "Token")]
[Gram("""
	@using DotGram.Examples.Languages;

	trivia = [' ' | '\t']*

	// This grammar's own `Word`, which is a conjunction and nothing like the library's.
	// They are `Word` and `Token.Word` and cannot be confused for one another.
	Word = "and"i | "or"i

	Test : @FilterTest
		= field: Token.Word & op: ('=' | '>') & value: (Token.Number | Token.Quoted)
		=> @(new FilterTest(field, op, value))

	Filter : @FilterTest[] = Test & (Word & Test)* & eof

	parse Filter
	""")]
public static partial class FilterFile;
