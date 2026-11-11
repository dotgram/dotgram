using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar;

namespace DotGram.Language;

enum DslClassificationTargetKind
{
	Rule,
	Capture,
}

enum DslClassificationBindingDiagnosticKind
{
	MalformedTarget,
	UnknownRule,
	AmbiguousRule,
	UnknownCapture,
	DuplicateTarget,
}

sealed class DslBoundClassification(
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

sealed class DslClassificationBindingDiagnostic(
	DslClassificationBindingDiagnosticKind kind,
	DslClassificationDefinition definition,
	string message)
{
	public DslClassificationBindingDiagnosticKind Kind { get; } = kind;
	public DslClassificationDefinition Definition { get; } = definition;
	public string Message { get; } = message;
}

sealed class DslClassificationBinding(
	IReadOnlyList<DslBoundClassification> classifications,
	IReadOnlyList<DslClassificationBindingDiagnostic> diagnostics)
{
	public IReadOnlyList<DslBoundClassification> Classifications { get; } = classifications;
	public IReadOnlyList<DslClassificationBindingDiagnostic> Diagnostics { get; } = diagnostics;
}

/// <summary>Binds author-facing classification targets to grammar symbol identities.</summary>
/// <remarks>
/// The binding itself is <see cref="ClassificationTargets"/>, which the build reports GRAM6001–GRAM6005 from
/// too; this hands it the language's targets and puts what it answers in the editor's terms.
/// </remarks>
static class DslClassificationBinder
{
	public static DslClassificationBinding Bind(
		DslLanguageDefinition language,
		string grammarSource)
	{
		if (language is null)
			throw new ArgumentNullException(nameof(language));
		if (grammarSource is null)
			throw new ArgumentNullException(nameof(grammarSource));

		var definitions = language.Classifications;
		var targets     = ClassificationTargets.Bind(
			grammarSource, [.. definitions.Select(static definition => definition.Target)], RoslynCSharpScanner.Instance);
		var bound       = new List<DslBoundClassification>();
		var diagnostics = new List<DslClassificationBindingDiagnostic>();

		for (var index = 0; index < definitions.Count; index++)
		{
			var definition = definitions[index];
			var target     = targets[index];

			if (target.Problem == ClassificationTargetProblem.None)
				bound.Add(new DslBoundClassification(
					definition,
					target.CapturePosition is null ? DslClassificationTargetKind.Rule : DslClassificationTargetKind.Capture,
					target.RulePosition,
					target.CapturePosition));
			else
				diagnostics.Add(new DslClassificationBindingDiagnostic(Kind(target.Problem), definition, target.Message!));
		}

		return new DslClassificationBinding(bound, diagnostics);
	}

	static DslClassificationBindingDiagnosticKind Kind(ClassificationTargetProblem problem)
	{
		return problem switch
		{
			ClassificationTargetProblem.Malformed      => DslClassificationBindingDiagnosticKind.MalformedTarget,
			ClassificationTargetProblem.UnknownRule    => DslClassificationBindingDiagnosticKind.UnknownRule,
			ClassificationTargetProblem.AmbiguousRule  => DslClassificationBindingDiagnosticKind.AmbiguousRule,
			ClassificationTargetProblem.UnknownCapture => DslClassificationBindingDiagnosticKind.UnknownCapture,
			_                                          => DslClassificationBindingDiagnosticKind.DuplicateTarget,
		};
	}
}
