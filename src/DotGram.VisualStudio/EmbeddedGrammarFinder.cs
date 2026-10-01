using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using DotGram.Grammar;
using DotGram.Language;

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
	string? analysisText = null,
	GramAnalysisOptions? options = null)
{
	public string          Text      { get; } = text;
	public SyntaxToken     Token     { get; } = token;
	public CSharpStringMap SourceMap { get; } = sourceMap;
	public string AnalysisText { get; } = analysisText ?? text;

	/// <summary>What the host's attribute says about compiling it, or null where nothing is known.</summary>
	public GramAnalysisOptions? Options { get; } = options;
}

/// <summary>Finds embedded grammars by attribute identity rather than source spelling.</summary>
public static class EmbeddedGrammarFinder
{
	const string GramAttribute = "DotGram.GramAttribute";
	const string StringSyntaxAttribute = "System.Diagnostics.CodeAnalysis.StringSyntaxAttribute";
	const string DotGramSyntax = "DotGram";
	const string DotGramExtensionSyntax = ".gram";

	/// <param name="includedFile">
	/// The text of a <c>.gram</c> file in the host's project by the name an included grammar's
	/// <c>[Gram]</c> gives it, or null where it is not there. Without it, an included grammar
	/// that is a file is read from what its class carries, as across an assembly reference.
	/// </param>
	public static IReadOnlyList<EmbeddedGrammar> Find(
		SemanticModel model,
		SyntaxNode root,
		CancellationToken cancellationToken = default,
		Func<string, string?>? includedFile = null)
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
				IsFile(literal.Token.ValueText) ||
				!CSharpStringMap.TryCreate(literal.Token, out var map))
				continue;

			var own = literal.Token.ValueText;
			if (IsFile(own))
				continue;

			var (included, lexical) = Host(model, attribute, includedFile, cancellationToken);
			var analysisText = included.Count == 0
				? own
				: GrammarSplice.Join(new GrammarSplice.Part(own, null, null), included).Text;
			var options = new GramAnalysisOptions
			{
				Own     = included.Count == 0 ? null : own.Length,
				Lexical = lexical,
			};

			grammars.Add(new EmbeddedGrammar(own, literal.Token, map!, analysisText, options));
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
				IsFile(literal.Token.ValueText) ||
				!CSharpStringMap.TryCreate(literal.Token, out var map))
				continue;

			grammars.Add(new EmbeddedGrammar(literal.Token.ValueText, literal.Token, map!));
		}

		return grammars;
	}

	static bool HasDotGramStringSyntax(IParameterSymbol parameter)
	{
		return parameter.GetAttributes().Any(static attribute =>
			attribute.AttributeClass?.ToDisplayString() == StringSyntaxAttribute &&
			attribute.ConstructorArguments is [{ Value: string syntax }] &&
			(string.Equals(syntax, DotGramSyntax, StringComparison.OrdinalIgnoreCase) ||
			 string.Equals(syntax, DotGramExtensionSyntax, StringComparison.OrdinalIgnoreCase)));
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

	/// <summary>
	/// What the class an attribute is written on includes, read the way a standalone grammar's
	/// host is read, and whether the attribute asks for the grammar to be read as tokens.
	/// </summary>
	static (IReadOnlyList<GrammarSplice.Part> Included, bool Lexical) Host(
		SemanticModel model,
		AttributeSyntax attribute,
		Func<string, string?>? includedFile,
		CancellationToken cancellationToken)
	{
		if (attribute.Parent?.Parent is not TypeDeclarationSyntax declaration ||
			model.GetDeclaredSymbol(declaration, cancellationToken) is not INamedTypeSymbol type)
			return (Array.Empty<GrammarSplice.Part>(), false);

		var included = new List<GrammarSplice.Part>();

		// The file first, from the host's own project, which is where the generator looks; what
		// the class carries is what there is across an assembly reference.
		foreach (var include in StandaloneGrammarInheritance.Includes(type))
		{
			var text = IsFile(include.Source) ? includedFile?.Invoke(include.Source) : include.Source;

			text ??= include.Carried;

			if (text is not null)
				included.Add(new GrammarSplice.Part(text, include.Name, null));
		}

		var lexical = type.GetAttributes()
			.FirstOrDefault(candidate => candidate.ApplicationSyntaxReference?.Span == attribute.Span)?
			.NamedArguments.FirstOrDefault(static argument => argument.Key == "Lexical")
			.Value.Value as bool? ?? false;

		return (included, lexical);
	}

	static bool IsFile(string source)
	{
		return StandaloneGrammarInheritance.IsFile(source);
	}

	static bool IsAttribute(ITypeSymbol type)
	{
		for (var current = type.BaseType; current is not null; current = current.BaseType)
			if (current.ToDisplayString() == "System.Attribute")
				return true;

		return false;
	}
}
