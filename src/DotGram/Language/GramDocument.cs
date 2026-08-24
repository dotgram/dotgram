using System;
using System.Collections.Generic;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Parsing;

namespace DotGram.Language;

/// <summary>Editor-neutral syntax categories produced for a <c>.gram</c> document.</summary>
public enum GramSyntaxKind
{
	Invalid,
	Identifier,
	Number,
	Character,
	String,
	CharacterClass,
	EmbeddedCode,
	Operator,
	Punctuation,
}

/// <summary>One classified source span in a <c>.gram</c> document.</summary>
public readonly struct GramClassifiedSpan(int position, int length, GramSyntaxKind kind)
{
	public int Position { get; } = position;
	public int Length { get; } = length;
	public GramSyntaxKind Kind { get; } = kind;
}

/// <summary>The editor-neutral analysis of one immutable <c>.gram</c> document.</summary>
public sealed class GramDocument(
	IReadOnlyList<GramClassifiedSpan> classifications,
	IReadOnlyList<GramDiagnostic> diagnostics)
{
	public IReadOnlyList<GramClassifiedSpan> Classifications { get; } = classifications;
	public IReadOnlyList<GramDiagnostic> Diagnostics { get; } = diagnostics;
}

/// <summary>
/// Adapts the existing compiler front-end to editor operations without reproducing
/// grammar recognition in an editor integration.
/// </summary>
public static class GramLanguageService
{
	/// <summary>Analyzes a complete snapshot of a standalone <c>.gram</c> document.</summary>
	public static GramDocument Analyze(string text)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		var tokens = GramLexer.Tokenize(text, RoslynCSharpScanner.Instance);
		var classifications = new List<GramClassifiedSpan>(tokens.Count);

		foreach (var token in tokens.Tokens)
			if (TryClassify(token.Kind, out var kind))
				classifications.Add(new GramClassifiedSpan(token.Position, token.Length, kind));

		var compilation = GramCompiler.Compile(text, new GramCompilerOptions
		{
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		return new GramDocument(classifications, compilation.Diagnostics);
	}

	static bool TryClassify(TokenKind token, out GramSyntaxKind kind)
	{
		kind = token switch
		{
			TokenKind.Unknown => GramSyntaxKind.Invalid,
			TokenKind.Identifier => GramSyntaxKind.Identifier,
			TokenKind.Integer => GramSyntaxKind.Number,
			TokenKind.Character => GramSyntaxKind.Character,
			TokenKind.String => GramSyntaxKind.String,
			TokenKind.UnicodeCategory => GramSyntaxKind.CharacterClass,
			TokenKind.CSharpExpression => GramSyntaxKind.EmbeddedCode,
			TokenKind.OpenParen or TokenKind.CloseParen => GramSyntaxKind.Punctuation,
			TokenKind.OpenBracket or TokenKind.CloseBracket => GramSyntaxKind.Punctuation,
			TokenKind.OpenBrace or TokenKind.CloseBrace => GramSyntaxKind.Punctuation,
			TokenKind.Comma or TokenKind.Semicolon => GramSyntaxKind.Punctuation,
			TokenKind.Colon or TokenKind.Dot or TokenKind.At => GramSyntaxKind.Punctuation,
			TokenKind.EndOfFile => default,
			_ => GramSyntaxKind.Operator,
		};

		return token != TokenKind.EndOfFile;
	}
}
