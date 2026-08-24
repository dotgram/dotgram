using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotGram.VisualStudio;

/// <summary>One grammar string proven by Roslyn to belong to <c>DotGram.GramAttribute</c>.</summary>
public sealed class EmbeddedGrammar(string text, SyntaxToken token, CSharpStringMap sourceMap)
{
	public string          Text      { get; } = text;
	public SyntaxToken     Token     { get; } = token;
	public CSharpStringMap SourceMap { get; } = sourceMap;
}

/// <summary>Finds embedded grammars by attribute identity rather than source spelling.</summary>
public static class EmbeddedGrammarFinder
{
	const string GramAttribute = "DotGram.GramAttribute";

	public static IReadOnlyList<EmbeddedGrammar> Find(
		SemanticModel model, SyntaxNode root, CancellationToken cancellationToken = default)
	{
		if (model is null)
			throw new ArgumentNullException(nameof(model));

		if (root is null)
			throw new ArgumentNullException(nameof(root));

		var grammars = new List<EmbeddedGrammar>();

		foreach (var attribute in root.DescendantNodes().OfType<AttributeSyntax>())
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (!IsGramAttribute(model, attribute, cancellationToken) ||
				attribute.ArgumentList?.Arguments is not [{ Expression: LiteralExpressionSyntax literal }] ||
				!CSharpStringMap.TryCreate(literal.Token, out var map))
				continue;

			grammars.Add(new EmbeddedGrammar(literal.Token.ValueText, literal.Token, map!));
		}

		return grammars;
	}

	static bool IsGramAttribute(
		SemanticModel model, AttributeSyntax attribute, CancellationToken cancellationToken)
	{
		var symbolInfo = model.GetSymbolInfo(attribute, cancellationToken);
		var actual     = (symbolInfo.Symbol as IMethodSymbol)?.ContainingType ??
			symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().Select(static symbol => symbol.ContainingType).FirstOrDefault() ??
			model.GetTypeInfo(attribute, cancellationToken).Type;

		return actual?.ToDisplayString() == GramAttribute && IsAttribute(actual);
	}

	static bool IsAttribute(ITypeSymbol type)
	{
		for (var current = type.BaseType; current is not null; current = current.BaseType)
			if (current.ToDisplayString() == "System.Attribute")
				return true;

		return false;
	}
}
