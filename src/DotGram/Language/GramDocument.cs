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
	Comment,
	Keyword,
	Identifier,
	Number,
	Character,
	String,
	CharacterClass,
	EmbeddedCode,
	Transition,
	SpecialSymbol,
	Operator,
	Punctuation,
}

/// <summary>One classified source span in a <c>.gram</c> document.</summary>
public readonly struct GramClassifiedSpan(
	int position,
	int length,
	GramSyntaxKind kind,
	string? quickInfo = null)
{
	public int Position { get; } = position;
	public int Length { get; } = length;
	public GramSyntaxKind Kind { get; } = kind;
	public string? QuickInfo { get; } = quickInfo;
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
	static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
	{
		"using", "context", "parse", "find", "as", "when", "recover",
		"any", "none", "eol", "eof", "trivia", "KeywordBoundary",
	};

	/// <summary>Analyzes a complete snapshot of a standalone <c>.gram</c> document.</summary>
	public static GramDocument Analyze(string text)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		var tokens = GramLexer.Tokenize(text, RoslynCSharpScanner.Instance);
		var parsed = GramParser.Parse(tokens);
		var classifications = new List<GramClassifiedSpan>(tokens.Count);
		var rules = RuleDefinitions(text, parsed.File.Decls);

		foreach (var token in tokens.Tokens)
			if (TryClassify(token, out var kind))
				classifications.Add(new GramClassifiedSpan(
					token.Position,
					token.Length,
					kind,
					token.Value is not null && rules.TryGetValue(token.Value, out var definition)
						? definition
						: null));

		ClassifyComments(text, tokens.Tokens, classifications);

		foreach (var classified in GramCSharpClassifier.Classify(text, parsed.File))
		{
			classifications.RemoveAll(existing => Intersects(existing, classified));
			classifications.Add(classified);
		}

		classifications.Sort(static (left, right) => left.Position.CompareTo(right.Position));

		var compilation = GramCompiler.Compile(text, new GramCompilerOptions
		{
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		return new GramDocument(classifications, compilation.Diagnostics);
	}

	static Dictionary<string, string> RuleDefinitions(string text, IReadOnlyList<Decl> declarations)
	{
		var result = new Dictionary<string, string>(StringComparer.Ordinal);

		Collect(declarations);

		return result;

		void Collect(IReadOnlyList<Decl> items)
		{
			foreach (var declaration in items)
				switch (declaration)
				{
					case Decl.Rule rule:
						var length = Math.Min(rule.At.Length, text.Length - rule.At.Position);

						if (length > 0 && !result.ContainsKey(rule.Name))
							result.Add(rule.Name, text.Substring(rule.At.Position, length).TrimEnd());

						break;
					case Decl.Context context:
						Collect(context.Decls);
						break;
				}
		}
	}

	static bool Intersects(GramClassifiedSpan left, GramClassifiedSpan right) =>
		left.Position < right.Position + right.Length && right.Position < left.Position + left.Length;

	static void ClassifyComments(
		string text,
		IReadOnlyList<Token> tokens,
		List<GramClassifiedSpan> classifications)
	{
		var previous = 0;

		foreach (var token in tokens)
		{
			ClassifyComments(text, previous, token.Position, classifications);
			previous = token.Position + token.Length;
		}
	}

	static void ClassifyComments(
		string text,
		int start,
		int end,
		List<GramClassifiedSpan> classifications)
	{
		var position = start;

		while (position + 1 < end)
		{
			if (text[position] != '/')
			{
				position++;
				continue;
			}

			var comment = position;

			if (text[position + 1] == '/')
			{
				position += 2;

				while (position < end && text[position] is not ('\r' or '\n'))
					position++;
			}
			else if (text[position + 1] == '*')
			{
				position += 2;

				while (position + 1 < end && !(text[position] == '*' && text[position + 1] == '/'))
					position++;

				position = position + 1 < end ? position + 2 : end;
			}
			else
			{
				position++;
				continue;
			}

			classifications.Add(new GramClassifiedSpan(comment, position - comment, GramSyntaxKind.Comment));
		}
	}

	static bool TryClassify(Token token, out GramSyntaxKind kind)
	{
		kind = token.Kind switch
		{
			TokenKind.Unknown => GramSyntaxKind.Invalid,
			TokenKind.Identifier when Keywords.Contains(token.Value!) => GramSyntaxKind.Keyword,
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
			TokenKind.At => GramSyntaxKind.Transition,
			TokenKind.Ampersand or TokenKind.Bar => GramSyntaxKind.SpecialSymbol,
			TokenKind.Question or TokenKind.Star or TokenKind.Plus or TokenKind.Caret => GramSyntaxKind.SpecialSymbol,
			TokenKind.DotDot or TokenKind.Less or TokenKind.Greater => GramSyntaxKind.SpecialSymbol,
			TokenKind.PositiveLookahead or TokenKind.NegativeLookahead => GramSyntaxKind.SpecialSymbol,
			TokenKind.Colon or TokenKind.Dot => GramSyntaxKind.Punctuation,
			TokenKind.EndOfFile => default,
			_ => GramSyntaxKind.Operator,
		};

		return token.Kind != TokenKind.EndOfFile;
	}
}
