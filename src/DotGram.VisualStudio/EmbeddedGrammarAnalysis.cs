using System;
using System.Collections.Generic;
using System.Threading;

using DotGram.Grammar;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.VisualStudio;

/// <summary>One grammar classification mapped into its containing C# document.</summary>
public readonly struct HostClassification(
	TextSpan span,
	GramSyntaxKind kind,
	string? quickInfo,
	TextSpan? definitionSpan,
	TextSpan grammarSpan,
	string? ruleSignature,
	int ruleParameterCount,
	GramSymbolKind? symbolKind)
{
	public TextSpan      Span { get; } = span;
	public GramSyntaxKind Kind { get; } = kind;
	public string?       QuickInfo { get; } = quickInfo;
	public TextSpan?     DefinitionSpan { get; } = definitionSpan;
	public TextSpan      GrammarSpan { get; } = grammarSpan;
	public string?       RuleSignature { get; } = ruleSignature;
	public int           RuleParameterCount { get; } = ruleParameterCount;
	public GramSymbolKind? SymbolKind { get; } = symbolKind;
}

/// <summary>One grammar diagnostic mapped into its containing C# document.</summary>
public sealed class HostDiagnostic(GramDiagnostic diagnostic, TextSpan span, bool isExact)
{
	public GramDiagnostic Diagnostic { get; } = diagnostic;
	public TextSpan       Span       { get; } = span;
	public bool           IsExact    { get; } = isExact;
}

public readonly record struct HostBracePair(TextSpan OpenSpan, TextSpan CloseSpan, TextSpan GrammarSpan);

public readonly record struct HostFoldingRange(TextSpan Span, TextSpan GrammarSpan, string CollapsedText);

public readonly record struct HostPublishedApi(string MethodName, TextSpan Span, TextSpan GrammarSpan);

public sealed class HostDocumentSymbol(
	string name,
	GramDocumentSymbolKind kind,
	TextSpan span,
	TextSpan selectionSpan,
	TextSpan grammarSpan,
	IReadOnlyList<HostDocumentSymbol> children)
{
	public string Name { get; } = name;
	public GramDocumentSymbolKind Kind { get; } = kind;
	public TextSpan Span { get; } = span;
	public TextSpan SelectionSpan { get; } = selectionSpan;
	public TextSpan GrammarSpan { get; } = grammarSpan;
	public IReadOnlyList<HostDocumentSymbol> Children { get; } = children;
}

/// <summary>One grammar rule occurrence mapped into its containing C# document.</summary>
public readonly struct HostSymbolOccurrence(
	string name,
	TextSpan span,
	TextSpan definitionSpan,
	TextSpan grammarSpan,
	bool isDefinition,
	GramSymbolKind kind,
	TextSpan scopeSpan)
{
	public string   Name { get; } = name;
	public TextSpan Span { get; } = span;
	public TextSpan DefinitionSpan { get; } = definitionSpan;
	public TextSpan GrammarSpan { get; } = grammarSpan;
	public bool IsDefinition { get; } = isDefinition;
	public GramSymbolKind Kind { get; } = kind;
	public TextSpan ScopeSpan { get; } = scopeSpan;
}

/// <summary>The editor-facing analysis of one grammar embedded in a C# document.</summary>
public sealed class EmbeddedGrammarAnalysis(
	EmbeddedGrammar grammar,
	IReadOnlyList<HostClassification> classifications,
	IReadOnlyList<HostDiagnostic> diagnostics,
	IReadOnlyList<HostSymbolOccurrence> symbols,
	IReadOnlyList<HostBracePair> braces,
	IReadOnlyList<HostFoldingRange> foldingRanges,
	IReadOnlyList<HostDocumentSymbol> documentSymbols,
	IReadOnlyList<HostPublishedApi> publishedApis)
{
	public EmbeddedGrammar                   Grammar         { get; } = grammar;
	public IReadOnlyList<HostClassification> Classifications { get; } = classifications;
	public IReadOnlyList<HostDiagnostic>     Diagnostics     { get; } = diagnostics;
	public IReadOnlyList<HostSymbolOccurrence> Symbols        { get; } = symbols;
	public IReadOnlyList<HostBracePair> Braces { get; } = braces;
	public IReadOnlyList<HostFoldingRange> FoldingRanges { get; } = foldingRanges;
	public IReadOnlyList<HostDocumentSymbol> DocumentSymbols { get; } = documentSymbols;
	public IReadOnlyList<HostPublishedApi> PublishedApis { get; } = publishedApis;
}

/// <summary>Runs shared grammar intelligence and maps its answers into a C# document.</summary>
public static class EmbeddedGrammarService
{
	public static IReadOnlyList<EmbeddedGrammarAnalysis> Analyze(
		SemanticModel model, SyntaxNode root, CancellationToken cancellationToken = default)
	{
		var analyses = new List<EmbeddedGrammarAnalysis>();

		foreach (var grammar in EmbeddedGrammarFinder.Find(model, root, cancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();

			var document        = GramLanguageService.Analyze(grammar.AnalysisText);
			var classifications = new List<HostClassification>(document.Classifications.Count);
			var diagnostics     = new List<HostDiagnostic>(document.Diagnostics.Count);
			var symbols         = new List<HostSymbolOccurrence>(document.Symbols.Count);
			var braces          = new List<HostBracePair>(document.Braces.Count);
			var foldingRanges   = new List<HostFoldingRange>(document.FoldingRanges.Count);
			var documentSymbols = MapSymbols(document.DocumentSymbols, grammar);
			var publishedApis   = new List<HostPublishedApi>(document.PublishedApis.Count);

			foreach (var classification in document.Classifications)
				if (grammar.SourceMap.TryMap(
					classification.Position, classification.Length, out var span))
				{
					TextSpan? definitionSpan = null;

					if (classification.DefinitionPosition is int definitionPosition &&
						grammar.SourceMap.TryMap(
							definitionPosition,
							classification.Length,
							out var mappedDefinition))
						definitionSpan = mappedDefinition;

					classifications.Add(new HostClassification(
						span,
						classification.Kind,
						classification.QuickInfo,
						definitionSpan,
						grammar.Token.Span,
						classification.RuleSignature,
						classification.RuleParameterCount,
						classification.SymbolKind));
				}

			foreach (var diagnostic in document.Diagnostics)
			{
				if (diagnostic.Position > grammar.Text.Length)
					continue;

				var exact = grammar.SourceMap.TryMap(diagnostic.Position, diagnostic.Length, out var span);

				diagnostics.Add(new HostDiagnostic(
					diagnostic,
					exact ? span : grammar.Token.Span,
					exact));
			}

			foreach (var symbol in document.Symbols)
				if (grammar.SourceMap.TryMap(symbol.Position, symbol.Length, out var span) &&
					grammar.SourceMap.TryMap(symbol.DefinitionPosition, symbol.Name.Length, out var definitionSpan) &&
					grammar.SourceMap.TryMap(
						symbol.ScopeStart,
						symbol.ScopeEnd == int.MaxValue ? grammar.Text.Length : symbol.ScopeEnd - symbol.ScopeStart,
						out var scopeSpan))
					symbols.Add(new HostSymbolOccurrence(
						symbol.Name,
						span,
						definitionSpan,
						grammar.Token.Span,
						symbol.IsDefinition,
						symbol.Kind,
						scopeSpan));

			foreach (var pair in document.Braces)
				if (grammar.SourceMap.TryMap(pair.OpenPosition, pair.OpenLength, out var openSpan) &&
					grammar.SourceMap.TryMap(pair.ClosePosition, pair.CloseLength, out var closeSpan))
					braces.Add(new HostBracePair(openSpan, closeSpan, grammar.Token.Span));

			foreach (var range in document.FoldingRanges)
				if (grammar.SourceMap.TryMap(range.Position, range.Length, out var span))
					foldingRanges.Add(new HostFoldingRange(span, grammar.Token.Span, range.CollapsedText));

			foreach (var publication in document.PublishedApis)
				if (grammar.SourceMap.TryMap(publication.Position, publication.Length, out var span))
					publishedApis.Add(new HostPublishedApi(publication.MethodName, span, grammar.Token.Span));

			analyses.Add(new EmbeddedGrammarAnalysis(
				grammar,
				classifications,
				diagnostics,
				symbols,
				braces,
				foldingRanges,
				documentSymbols,
				publishedApis));
		}

		return analyses;
	}

	static IReadOnlyList<HostDocumentSymbol> MapSymbols(
		IReadOnlyList<GramDocumentSymbol> symbols,
		EmbeddedGrammar grammar)
	{
		var result = new List<HostDocumentSymbol>(symbols.Count);

		foreach (var symbol in symbols)
			if (grammar.SourceMap.TryMap(symbol.Position, symbol.Length, out var span) &&
				grammar.SourceMap.TryMap(symbol.SelectionPosition, symbol.SelectionLength, out var selectionSpan))
				result.Add(new HostDocumentSymbol(
					symbol.Name,
					symbol.Kind,
					span,
					selectionSpan,
					grammar.Token.Span,
					MapSymbols(symbol.Children, grammar)));

		return result;
	}
}
