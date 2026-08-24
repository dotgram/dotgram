using System;
using System.Collections.Generic;
using System.Threading;

using DotGram.Grammar;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.VisualStudio;

/// <summary>One grammar classification mapped into its containing C# document.</summary>
public readonly struct HostClassification(TextSpan span, GramSyntaxKind kind)
{
	public TextSpan      Span { get; } = span;
	public GramSyntaxKind Kind { get; } = kind;
}

/// <summary>One grammar diagnostic mapped into its containing C# document.</summary>
public sealed class HostDiagnostic(GramDiagnostic diagnostic, TextSpan span, bool isExact)
{
	public GramDiagnostic Diagnostic { get; } = diagnostic;
	public TextSpan       Span       { get; } = span;
	public bool           IsExact    { get; } = isExact;
}

/// <summary>The editor-facing analysis of one grammar embedded in a C# document.</summary>
public sealed class EmbeddedGrammarAnalysis(
	EmbeddedGrammar grammar,
	IReadOnlyList<HostClassification> classifications,
	IReadOnlyList<HostDiagnostic> diagnostics)
{
	public EmbeddedGrammar                   Grammar         { get; } = grammar;
	public IReadOnlyList<HostClassification> Classifications { get; } = classifications;
	public IReadOnlyList<HostDiagnostic>     Diagnostics     { get; } = diagnostics;
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

			foreach (var classification in document.Classifications)
				if (grammar.SourceMap.TryMap(
					classification.Position, classification.Length, out var span))
					classifications.Add(new HostClassification(span, classification.Kind));

			foreach (var diagnostic in document.Diagnostics)
			{
				var exact = grammar.SourceMap.TryMap(diagnostic.Position, diagnostic.Length, out var span);

				diagnostics.Add(new HostDiagnostic(
					diagnostic,
					exact ? span : grammar.Token.Span,
					exact));
			}

			analyses.Add(new EmbeddedGrammarAnalysis(grammar, classifications, diagnostics));
		}

		return analyses;
	}
}
