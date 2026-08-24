using System;
using System.Collections.Generic;

using DotGram.Grammar.Parsing;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotGram.Language;

static class GramCSharpClassifier
{
	public static IReadOnlyList<GramClassifiedSpan> Classify(string text, GrammarFile file)
	{
		var result = new List<GramClassifiedSpan>();

		foreach (var declaration in file.Decls)
			Classify(declaration, text, result);

		return result;
	}

	static void Classify(Decl declaration, string text, List<GramClassifiedSpan> result)
	{
		switch (declaration)
		{
			case Decl.Rule(_, var parameters, var type, var body):
				foreach (var parameter in parameters)
					Classify(parameter.Type, text, result);

				Classify(type, text, result);
				Classify(body, text, result);
				break;

			case Decl.Context(_, _, _, var declarations):
				foreach (var nested in declarations)
					Classify(nested, text, result);
				break;
		}
	}

	static void Classify(TypeRef? type, string text, List<GramClassifiedSpan> result)
	{
		if (type is not { IsCSharp: true } || !AfterAt(text, type.At, out var start, out var length))
			return;

		var syntax = SyntaxFactory.ParseTypeName(text.Substring(start, length));
		Classify(syntax.DescendantTokens(), start, result);
	}

	static void Classify(Expr expression, string text, List<GramClassifiedSpan> result)
	{
		switch (expression)
		{
			case Expr.CSharp(var csharpText):
			{
				var start  = expression.At.Position + 2;
				var syntax = SyntaxFactory.ParseExpression(csharpText);

				result.Add(new GramClassifiedSpan(expression.At.Position, 1, GramSyntaxKind.Transition));
				result.Add(new GramClassifiedSpan(expression.At.Position + 1, 1, GramSyntaxKind.Punctuation));
				Classify(syntax.DescendantTokens(), start, result);
				result.Add(new GramClassifiedSpan(start + csharpText.Length, 1, GramSyntaxKind.Punctuation));

				break;
			}

			case Expr.Reference { IsCSharp: true } reference
				when AfterAt(text, reference.At, out var start, out var length):
			{
				var syntax = SyntaxFactory.ParseExpression(text.Substring(start, length));
				Classify(syntax.DescendantTokens(), start, result);
				break;
			}
		}

		foreach (var child in Dump.Children(expression))
			Classify(child, text, result);
	}

	static void Classify(
		IEnumerable<Microsoft.CodeAnalysis.SyntaxToken> tokens,
		int offset,
		List<GramClassifiedSpan> result)
	{
		foreach (var token in tokens)
		{
			Classify(token.LeadingTrivia, offset, result);

			if (token.IsMissing || token.Span.Length == 0)
			{
				Classify(token.TrailingTrivia, offset, result);
				continue;
			}

			result.Add(new GramClassifiedSpan(offset + token.SpanStart, token.Span.Length, Kind(token)));
			Classify(token.TrailingTrivia, offset, result);
		}
	}

	static void Classify(
		Microsoft.CodeAnalysis.SyntaxTriviaList trivia,
		int offset,
		List<GramClassifiedSpan> result)
	{
		foreach (var item in trivia)
			if ((SyntaxKind)item.RawKind is
				SyntaxKind.SingleLineCommentTrivia or
				SyntaxKind.MultiLineCommentTrivia or
				SyntaxKind.SingleLineDocumentationCommentTrivia or
				SyntaxKind.MultiLineDocumentationCommentTrivia)
				result.Add(new GramClassifiedSpan(offset + item.SpanStart, item.Span.Length, GramSyntaxKind.Comment));
	}

	static GramSyntaxKind Kind(Microsoft.CodeAnalysis.SyntaxToken token)
	{
		var kind = token.Kind();

		if (SyntaxFacts.IsKeywordKind(kind))
			return GramSyntaxKind.Keyword;

		return kind switch
		{
			SyntaxKind.IdentifierToken => GramSyntaxKind.Identifier,
			SyntaxKind.NumericLiteralToken => GramSyntaxKind.Number,
			SyntaxKind.CharacterLiteralToken => GramSyntaxKind.Character,
			SyntaxKind.StringLiteralToken or
			SyntaxKind.SingleLineRawStringLiteralToken or
			SyntaxKind.MultiLineRawStringLiteralToken => GramSyntaxKind.String,
			_ when token.Text.Length == 1 && "()[]{}.,;".IndexOf(token.Text[0]) >= 0 => GramSyntaxKind.Punctuation,
			_ => GramSyntaxKind.Operator,
		};
	}

	static bool AfterAt(string text, Location location, out int start, out int length)
	{
		start  = location.Position;
		length = location.Length;

		if (length <= 1 || start < 0 || start >= text.Length || text[start] != '@')
			return false;

		start++;
		length--;

		return true;
	}
}
