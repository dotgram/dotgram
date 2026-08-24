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
	int ruleParameterCount)
{
	public TextSpan      Span { get; } = span;
	public GramSyntaxKind Kind { get; } = kind;
	public string?       QuickInfo { get; } = quickInfo;
	public TextSpan?     DefinitionSpan { get; } = definitionSpan;
	public TextSpan      GrammarSpan { get; } = grammarSpan;
	public string?       RuleSignature { get; } = ruleSignature;
	public int           RuleParameterCount { get; } = ruleParameterCount;
}

/// <summary>One grammar diagnostic mapped into its containing C# document.</summary>
public sealed class HostDiagnostic(GramDiagnostic diagnostic, TextSpan span, bool isExact)
{
	public GramDiagnostic Diagnostic { get; } = diagnostic;
	public TextSpan       Span       { get; } = span;
	public bool           IsExact    { get; } = isExact;
}

/// <summary>One grammar rule occurrence mapped into its containing C# document.</summary>
public readonly struct HostSymbolOccurrence(
	string name,
	TextSpan span,
	TextSpan definitionSpan,
	TextSpan grammarSpan,
	bool isDefinition)
{
	public string   Name { get; } = name;
	public TextSpan Span { get; } = span;
	public TextSpan DefinitionSpan { get; } = definitionSpan;
	public TextSpan GrammarSpan { get; } = grammarSpan;
	public bool IsDefinition { get; } = isDefinition;
}

/// <summary>The editor-facing analysis of one grammar embedded in a C# document.</summary>
public sealed class EmbeddedGrammarAnalysis(
	EmbeddedGrammar grammar,
	IReadOnlyList<HostClassification> classifications,
	IReadOnlyList<HostDiagnostic> diagnostics,
	IReadOnlyList<HostSymbolOccurrence> symbols)
{
	public EmbeddedGrammar                   Grammar         { get; } = grammar;
	public IReadOnlyList<HostClassification> Classifications { get; } = classifications;
	public IReadOnlyList<HostDiagnostic>     Diagnostics     { get; } = diagnostics;
	public IReadOnlyList<HostSymbolOccurrence> Symbols        { get; } = symbols;
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

			var document        = GramLanguageService.Analyze(grammar.Text);
			var classifications = new List<HostClassification>(document.Classifications.Count);
			var diagnostics     = new List<HostDiagnostic>(document.Diagnostics.Count);
			var symbols         = new List<HostSymbolOccurrence>(document.Symbols.Count);

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
						classification.RuleParameterCount));
				}

			foreach (var diagnostic in document.Diagnostics)
			{
				var exact = grammar.SourceMap.TryMap(diagnostic.Position, diagnostic.Length, out var span);

				diagnostics.Add(new HostDiagnostic(
					diagnostic,
					exact ? span : grammar.Token.Span,
					exact));
			}

			foreach (var symbol in document.Symbols)
				if (grammar.SourceMap.TryMap(symbol.Position, symbol.Length, out var span) &&
					grammar.SourceMap.TryMap(symbol.DefinitionPosition, symbol.Name.Length, out var definitionSpan))
					symbols.Add(new HostSymbolOccurrence(
						symbol.Name,
						span,
						definitionSpan,
						grammar.Token.Span,
						symbol.IsDefinition));

			analyses.Add(new EmbeddedGrammarAnalysis(grammar, classifications, diagnostics, symbols));
		}

		return analyses;
	}
}
