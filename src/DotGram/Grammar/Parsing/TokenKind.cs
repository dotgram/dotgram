using System;

namespace DotGram.Grammar.Parsing;

/// <summary>Every lexeme `.gram` source is made of.</summary>
public enum TokenKind
{
	/// <summary>Something the lexer could not make sense of. Carries the offending text.</summary>
	Unknown,

	EndOfFile,

	// Carry a value.
	Identifier,
	Integer,
	Character,
	String,

	/// <summary><c>'x'i</c> — a character literal matched without regard to case.</summary>
	CaseInsensitiveCharacter,

	/// <summary><c>"text"i</c> — a string literal matched without regard to case.</summary>
	CaseInsensitiveString,

	/// <summary><c>\p{Lu}</c> — carries the category name.</summary>
	UnicodeCategory,

	/// <summary><c>@(…)</c> — carries the raw C# text between the parentheses.</summary>
	CSharpExpression,

	// Fixed spellings.
	Ampersand,          // &
	Bar,                // |
	OpenParen,          // (
	CloseParen,         // )
	OpenBracket,        // [
	CloseBracket,       // ]
	OpenBrace,          // {
	CloseBrace,         // }
	Comma,              // ,
	Semicolon,          // ;
	Colon,              // :
	Equals,             // =
	Arrow,              // =>
	Question,           // ?
	Star,               // *
	Plus,               // +
	Caret,              // ^
	Dot,                // .
	DotDot,             // ..
	Less,               // <
	Greater,            // >
	At,                 // @
	Tilde,              // ~
	PositiveLookahead,  // ?=
	NegativeLookahead,  // ?!
}

public static class TokenKindExtensions
{
	/// <summary>The one spelling a fixed-spelling kind has, or null when it carries a value.</summary>
	public static string? Spelling(this TokenKind kind) => kind switch
	{
		TokenKind.Ampersand         => "&",
		TokenKind.Tilde             => "~",
		TokenKind.Bar               => "|",
		TokenKind.OpenParen         => "(",
		TokenKind.CloseParen        => ")",
		TokenKind.OpenBracket       => "[",
		TokenKind.CloseBracket      => "]",
		TokenKind.OpenBrace         => "{",
		TokenKind.CloseBrace        => "}",
		TokenKind.Comma             => ",",
		TokenKind.Semicolon         => ";",
		TokenKind.Colon             => ":",
		TokenKind.Equals            => "=",
		TokenKind.Arrow             => "=>",
		TokenKind.Question          => "?",
		TokenKind.Star              => "*",
		TokenKind.Plus              => "+",
		TokenKind.Caret             => "^",
		TokenKind.Dot               => ".",
		TokenKind.DotDot            => "..",
		TokenKind.Less              => "<",
		TokenKind.Greater           => ">",
		TokenKind.At                => "@",
		TokenKind.PositiveLookahead => "?=",
		TokenKind.NegativeLookahead => "?!",
		_                           => null,
	};
}
