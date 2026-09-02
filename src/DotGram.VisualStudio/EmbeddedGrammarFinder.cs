using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.VisualStudio;

/// <summary>One grammar string proven by Roslyn to belong to <c>DotGram.GramAttribute</c>.</summary>
public sealed class EmbeddedGrammar(
	string text,
	SyntaxToken token,
	CSharpStringMap sourceMap,
	string? analysisText = null)
{
	public string          Text      { get; } = text;
	public SyntaxToken     Token     { get; } = token;
	public CSharpStringMap SourceMap { get; } = sourceMap;
	public string AnalysisText { get; } = analysisText ?? text;
}

/// <summary>Finds embedded grammars by attribute identity rather than source spelling.</summary>
public static class EmbeddedGrammarFinder
{
	const string GramAttribute = "DotGram.GramAttribute";
	const string StringSyntaxAttribute = "System.Diagnostics.CodeAnalysis.StringSyntaxAttribute";
	const string DotGramSyntax = "DotGram";

	public static IReadOnlyList<EmbeddedGrammar> Find(
		SemanticModel model, SyntaxNode root, CancellationToken cancellationToken = default)
	{
		if (model is null)
			throw new ArgumentNullException(nameof(model));

		if (root is null)
			throw new ArgumentNullException(nameof(root));

		var grammars = new List<EmbeddedGrammar>();
		var seen = new HashSet<TextSpan>();

		foreach (var attribute in root.DescendantNodes().OfType<AttributeSyntax>())
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (!IsGramAttribute(model, attribute, cancellationToken) ||
				attribute.ArgumentList?.Arguments.FirstOrDefault(
					static argument => argument.NameEquals is null) is not
						{ Expression: LiteralExpressionSyntax literal } ||
				!CSharpStringMap.TryCreate(literal.Token, out var map))
				continue;

			var own = literal.Token.ValueText;
			var included = IncludedGrammars(model, attribute, cancellationToken);
			var analysisText = included.Count == 0
				? own
				: GrammarSplice.Join(new GrammarSplice.Part(own, null, null), included).Text;

			grammars.Add(new EmbeddedGrammar(own, literal.Token, map!, analysisText));
			seen.Add(literal.Token.Span);
		}

		foreach (var argument in root.DescendantNodes().OfType<ArgumentSyntax>())
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (argument.Expression is not LiteralExpressionSyntax literal ||
				seen.Contains(literal.Token.Span) ||
				model.GetOperation(argument, cancellationToken) is not IArgumentOperation
				{
					Parameter: { Type.SpecialType: SpecialType.System_String } parameter,
				} ||
				!HasDotGramStringSyntax(parameter) ||
				!CSharpStringMap.TryCreate(literal.Token, out var map))
				continue;

			grammars.Add(new EmbeddedGrammar(literal.Token.ValueText, literal.Token, map!));
			seen.Add(literal.Token.Span);
		}

		return grammars;
	}

	/// <summary>
	/// Finds source-spelled <c>Gram</c> attributes without requesting a semantic model.
	/// The result is intentionally provisional and is used only to make initial editor
	/// classification available while Roslyn finishes the authoritative analysis.
	/// </summary>
	public static IReadOnlyList<EmbeddedGrammar> FindSyntactic(
		SyntaxNode root,
		CancellationToken cancellationToken = default)
	{
		if (root is null)
			throw new ArgumentNullException(nameof(root));

		var grammars = new List<EmbeddedGrammar>();

		foreach (var attribute in root.DescendantNodes().OfType<AttributeSyntax>())
		{
			cancellationToken.ThrowIfCancellationRequested();

			var name = attribute.Name.ToString();
			if (name is not "Gram" and not "GramAttribute" &&
				!name.EndsWith(".Gram", StringComparison.Ordinal) &&
				!name.EndsWith(".GramAttribute", StringComparison.Ordinal) ||
				attribute.ArgumentList?.Arguments.FirstOrDefault(
					static argument => argument.NameEquals is null) is not
						{ Expression: LiteralExpressionSyntax literal } ||
				!CSharpStringMap.TryCreate(literal.Token, out var map))
				continue;

			grammars.Add(new EmbeddedGrammar(literal.Token.ValueText, literal.Token, map!));
		}

		return grammars;
	}

	static bool HasDotGramStringSyntax(IParameterSymbol parameter) =>
		parameter.GetAttributes().Any(static attribute =>
			attribute.AttributeClass?.ToDisplayString() == StringSyntaxAttribute &&
			attribute.ConstructorArguments is [{ Value: string syntax }] &&
			string.Equals(syntax, DotGramSyntax, StringComparison.OrdinalIgnoreCase));

	static bool IsGramAttribute(
		SemanticModel model, AttributeSyntax attribute, CancellationToken cancellationToken)
	{
		var symbolInfo = model.GetSymbolInfo(attribute, cancellationToken);
		var actual     = (symbolInfo.Symbol as IMethodSymbol)?.ContainingType ??
			symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().Select(static symbol => symbol.ContainingType).FirstOrDefault() ??
			model.GetTypeInfo(attribute, cancellationToken).Type;

		return actual?.ToDisplayString() == GramAttribute && IsAttribute(actual);
	}

	static IReadOnlyList<GrammarSplice.Part> IncludedGrammars(
		SemanticModel model,
		AttributeSyntax attribute,
		CancellationToken cancellationToken)
	{
		if (attribute.Parent?.Parent is not TypeDeclarationSyntax declaration ||
			model.GetDeclaredSymbol(declaration, cancellationToken) is not INamedTypeSymbol type)
			return Array.Empty<GrammarSplice.Part>();

		var included = new List<GrammarSplice.Part>();
		for (var current = type.BaseType; current is not null; current = current.BaseType)
		{
			var grammar = current.GetAttributes().FirstOrDefault(static candidate =>
				candidate.AttributeClass?.ToDisplayString() == GramAttribute);
			if (grammar?.ConstructorArguments is not [{ Value: string source }] || IsFile(source))
				continue;

			var name = grammar.NamedArguments
				.FirstOrDefault(static argument => argument.Key == "IncludedAs")
				.Value.Value as string ?? current.Name;
			included.Add(new GrammarSplice.Part(source, name, null));
		}

		return included;
	}

	static bool IsFile(string source) =>
		source.EndsWith(".gram", StringComparison.OrdinalIgnoreCase) &&
		source.IndexOf('\r') < 0 && source.IndexOf('\n') < 0;

	static bool IsAttribute(ITypeSymbol type)
	{
		for (var current = type.BaseType; current is not null; current = current.BaseType)
			if (current.ToDisplayString() == "System.Attribute")
				return true;

		return false;
	}
}
