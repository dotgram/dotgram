using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.VisualStudio;

internal readonly record struct HostDslClassification(TextSpan Span, string Role);

internal sealed class DslEmbeddedSiteResult(
	IReadOnlyList<HostDslClassification> classifications,
	IReadOnlyList<HostDiagnostic> diagnostics)
{
	public IReadOnlyList<HostDslClassification> Classifications { get; } = classifications;
	public IReadOnlyList<HostDiagnostic> Diagnostics { get; } = diagnostics;
}

internal static class DslEmbeddedSiteAnalysis
{
	const string RecognitionDiagnostic = "GRAM5101";

	public static async Task<DslEmbeddedSiteResult> AnalyzeAsync(
		Document document,
		SyntaxNode root,
		SemanticModel model,
		CancellationToken cancellationToken = default)
	{
		var catalog = DslLanguageDiscovery.Discover(model.Compilation, cancellationToken);
		var classifications = new List<HostDslClassification>();
		var diagnostics = new List<HostDiagnostic>();

		foreach (var argument in root.DescendantNodes().OfType<ArgumentSyntax>())
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (model.GetOperation(argument, cancellationToken) is not IArgumentOperation
				{
					Parameter:
					{
						Type.SpecialType: SpecialType.System_String,
					} parameter,
				} ||
				argument.Expression is not LiteralExpressionSyntax literal ||
				!CSharpStringMap.TryCreate(literal.Token, out var sourceMap))
				continue;

			var markerTypes = parameter.GetAttributes()
				.Select(static attribute => attribute.AttributeClass)
				.Where(static type => type is not null)
				.Cast<INamedTypeSymbol>()
				.ToArray();
			var carriers = catalog.AttributeCarriers.Where(candidate => markerTypes.Any(marker =>
				SymbolEqualityComparer.Default.Equals(candidate.AttributeType, marker))).ToArray();
			if (carriers.Length != 1)
				continue;
			var carrier = carriers[0];

			var resolution = await DslGrammarSourceResolver.ResolveAsync(
				document.Project,
				carrier.Language,
				cancellationToken).ConfigureAwait(false);
			if (resolution.Kind != DslGrammarResolutionKind.Resolved || resolution.Text is null)
				continue;

			var parsed = GramParser.Parse(GramLexer.Tokenize(resolution.Text));
			var grammarModel = GrammarBinder.Bind(parsed.File);
			var graph = GrammarNormalizer.Normalize(grammarModel);
			if (parsed.Diagnostics.Any(IsError) || grammarModel.Diagnostics.Any(IsError) || graph.Diagnostics.Any(IsError))
				continue;

			var publication = graph.Publications
				.Where(static item => item.Kind == PublishKind.Parse)
				.ToArray();
			if (publication.Length != 1)
				continue;

			var binding = DslClassificationBinder.Bind(carrier.Language, resolution.Text);
			if (binding.Diagnostics.Count > 0)
				continue;

			var trace = DslRecognitionTrace.Recognize(graph, publication[0], literal.Token.ValueText);
			if (trace.Status == DslRecognitionStatus.Success)
			{
				foreach (var classified in Classify(trace.Extents, binding.Classifications))
					if (sourceMap!.TryMap(classified.Position, classified.Length, out var mapped))
						classifications.Add(new HostDslClassification(mapped, classified.Role));
			}
			else if (trace.Status == DslRecognitionStatus.Failure &&
				sourceMap!.TryMap(trace.FailurePosition, 0, out var failure))
			{
				diagnostics.Add(new HostDiagnostic(
					new GramDiagnostic(
						RecognitionDiagnostic,
						$"Text does not match DotGram language '{carrier.Language.Id}'.",
						0,
						0,
						GramSeverity.Error),
					failure,
					isExact: true));
			}
		}

		return new DslEmbeddedSiteResult(classifications, diagnostics);
	}

	internal static IReadOnlyList<(int Position, int Length, string Role)> Classify(
		IReadOnlyList<DslRecognitionExtent> extents,
		IReadOnlyList<DslBoundClassification> bindings)
	{
		var candidates = new List<(int Position, int Length, string Role, int Priority)>();

		foreach (var extent in extents)
		foreach (var binding in bindings)
		{
			if (extent.Rule.Declaration?.At.Position != binding.RuleDefinitionPosition)
				continue;

			if (binding.TargetKind == DslClassificationTargetKind.Rule && extent.Capture is null)
				candidates.Add((extent.Position, extent.Length, binding.Definition.Role, 1));
			else if (binding.TargetKind == DslClassificationTargetKind.Capture &&
				extent.Capture == CaptureName(binding.Definition.Target))
				candidates.Add((extent.Position, extent.Length, binding.Definition.Role, 2));
		}

		if (candidates.Count == 0)
			return [];

		var end = candidates.Max(static item => item.Position + item.Length);
		var roles = new (string? Role, int Priority, int Length)[end];

		foreach (var candidate in candidates.OrderByDescending(static item => item.Length))
			for (var position = candidate.Position; position < candidate.Position + candidate.Length; position++)
				if (candidate.Priority > roles[position].Priority ||
					candidate.Priority == roles[position].Priority && candidate.Length <= roles[position].Length)
					roles[position] = (candidate.Role, candidate.Priority, candidate.Length);

		var result = new List<(int Position, int Length, string Role)>();
		for (var position = 0; position < roles.Length;)
		{
			if (roles[position].Role is not { } role)
			{
				position++;
				continue;
			}

			var start = position++;
			while (position < roles.Length && roles[position].Role == role)
				position++;
			result.Add((start, position - start, role));
		}

		return result;
	}

	static string CaptureName(string target) => target.Substring(target.IndexOf('.') + 1);

	static bool IsError(GramDiagnostic diagnostic) => diagnostic.Severity == GramSeverity.Error;
}
