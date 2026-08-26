using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Language;

namespace DotGram.VisualStudio;

public enum DslClassificationTargetKind
{
	Rule,
	Capture,
}

public enum DslClassificationBindingDiagnosticKind
{
	MalformedTarget,
	UnknownRule,
	AmbiguousRule,
	UnknownCapture,
	DuplicateTarget,
}

public sealed class DslBoundClassification(
	DslClassificationDefinition definition,
	DslClassificationTargetKind targetKind,
	int ruleDefinitionPosition,
	int? captureDefinitionPosition)
{
	public DslClassificationDefinition Definition { get; } = definition;
	public DslClassificationTargetKind TargetKind { get; } = targetKind;
	public int RuleDefinitionPosition { get; } = ruleDefinitionPosition;
	public int? CaptureDefinitionPosition { get; } = captureDefinitionPosition;
}

public sealed class DslClassificationBindingDiagnostic(
	DslClassificationBindingDiagnosticKind kind,
	DslClassificationDefinition definition,
	string message)
{
	public DslClassificationBindingDiagnosticKind Kind { get; } = kind;
	public DslClassificationDefinition Definition { get; } = definition;
	public string Message { get; } = message;
}

public sealed class DslClassificationBinding(
	IReadOnlyList<DslBoundClassification> classifications,
	IReadOnlyList<DslClassificationBindingDiagnostic> diagnostics)
{
	public IReadOnlyList<DslBoundClassification> Classifications { get; } = classifications;
	public IReadOnlyList<DslClassificationBindingDiagnostic> Diagnostics { get; } = diagnostics;
}

/// <summary>Binds author-facing classification targets to grammar symbol identities.</summary>
public static class DslClassificationBinder
{
	public static DslClassificationBinding Bind(
		DslLanguageDefinition language,
		string grammarSource)
	{
		if (language is null)
			throw new ArgumentNullException(nameof(language));
		if (grammarSource is null)
			throw new ArgumentNullException(nameof(grammarSource));

		var document = GramLanguageService.Analyze(grammarSource);
		var rules = Rules(document.DocumentSymbols).ToArray();
		var captures = document.Symbols
			.Where(static symbol => symbol.Kind == GramSymbolKind.Capture && symbol.IsDefinition)
			.ToArray();
		var bound = new List<DslBoundClassification>();
		var diagnostics = new List<DslClassificationBindingDiagnostic>();
		var targets = new HashSet<(DslClassificationTargetKind Kind, int Position)>();

		foreach (var definition in language.Classifications)
		{
			var parts = definition.Target.Split('.');
			if (parts.Length is < 1 or > 2 || parts.Any(string.IsNullOrWhiteSpace) ||
				parts.Any(static part => !string.Equals(part, part.Trim(), StringComparison.Ordinal)))
			{
				Diagnostic(DslClassificationBindingDiagnosticKind.MalformedTarget, definition,
					$"Classification target '{definition.Target}' must be 'Rule' or 'Rule.capture'.");
				continue;
			}

			var matchingRules = rules.Where(rule => rule.Name == parts[0]).ToArray();
			if (matchingRules.Length == 0)
			{
				Diagnostic(DslClassificationBindingDiagnosticKind.UnknownRule, definition,
					$"Grammar rule '{parts[0]}' was not found.");
				continue;
			}
			if (matchingRules.Length > 1)
			{
				Diagnostic(DslClassificationBindingDiagnosticKind.AmbiguousRule, definition,
					$"Grammar rule '{parts[0]}' is ambiguous.");
				continue;
			}

			var rule = matchingRules[0];
			if (parts.Length == 1)
			{
				Add(definition, DslClassificationTargetKind.Rule, rule.SelectionPosition, null);
				continue;
			}

			var matchingCaptures = captures
				.Where(capture => capture.Name == parts[1] &&
					capture.ScopeStart == rule.Position &&
					capture.Position >= rule.Position && capture.Position < rule.Position + rule.Length)
				.Select(static capture => capture.DefinitionPosition)
				.Distinct()
				.ToArray();
			if (matchingCaptures.Length == 0)
			{
				Diagnostic(DslClassificationBindingDiagnosticKind.UnknownCapture, definition,
					$"Capture '{parts[1]}' was not found in rule '{parts[0]}'.");
				continue;
			}

			Add(definition, DslClassificationTargetKind.Capture, rule.SelectionPosition, matchingCaptures[0]);
		}

		return new DslClassificationBinding(bound, diagnostics);

		void Add(
			DslClassificationDefinition definition,
			DslClassificationTargetKind kind,
			int rulePosition,
			int? capturePosition)
		{
			var identity = (kind, capturePosition ?? rulePosition);
			if (!targets.Add(identity))
			{
				Diagnostic(DslClassificationBindingDiagnosticKind.DuplicateTarget, definition,
					$"Classification target '{definition.Target}' is specified more than once.");
				return;
			}

			bound.Add(new DslBoundClassification(definition, kind, rulePosition, capturePosition));
		}

		void Diagnostic(
			DslClassificationBindingDiagnosticKind kind,
			DslClassificationDefinition definition,
			string message) => diagnostics.Add(new DslClassificationBindingDiagnostic(kind, definition, message));
	}

	static IEnumerable<GramDocumentSymbol> Rules(IReadOnlyList<GramDocumentSymbol> symbols)
	{
		foreach (var symbol in symbols)
		{
			if (symbol.Kind == GramDocumentSymbolKind.Rule)
				yield return symbol;
			foreach (var child in Rules(symbol.Children))
				yield return child;
		}
	}
}
