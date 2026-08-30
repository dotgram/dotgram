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
internal readonly record struct HostDslSite(
	TextSpan Span,
	string LanguageId,
	string EntryRule,
	int CompletionPosition,
	IReadOnlyList<string> Expected);

internal sealed class DslEmbeddedSiteResult(
	IReadOnlyList<HostDslClassification> classifications,
	IReadOnlyList<HostDiagnostic> diagnostics,
	IReadOnlyList<HostDslSite> sites)
{
	public IReadOnlyList<HostDslClassification> Classifications { get; } = classifications;
	public IReadOnlyList<HostDiagnostic> Diagnostics { get; } = diagnostics;
	public IReadOnlyList<HostDslSite> Sites { get; } = sites;
}

internal sealed class DslPreparedLanguage(
	string fingerprint,
	RecognitionGraph graph,
	IReadOnlyList<Publication> publications,
	DslClassificationBinding binding)
{
	public string Fingerprint { get; } = fingerprint;
	public RecognitionGraph Graph { get; } = graph;
	public IReadOnlyList<Publication> Publications { get; } = publications;
	public DslClassificationBinding Binding { get; } = binding;
}

internal sealed class DslEmbeddedSiteCache
{
	readonly object _gate = new();
	readonly Dictionary<string, DslPreparedLanguage> _languages = new(StringComparer.Ordinal);

	public DslPreparedLanguage? Prepare(DslLanguageDefinition language, string grammarSource)
	{
		var key = language.ParserType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		var fingerprint = grammarSource + "\u001e" + string.Join(
			"\u001f",
			language.Classifications.Select(static item => item.Target + "\u001d" + item.Role));

		lock (_gate)
			if (_languages.TryGetValue(key, out var cached) && cached.Fingerprint == fingerprint)
				return cached;

		var prepared = DslEmbeddedSiteAnalysis.Prepare(language, grammarSource, fingerprint);

		if (prepared is not null)
			lock (_gate)
				_languages[key] = prepared;

		return prepared;
	}
}

internal static class DslEmbeddedSiteAnalysis
{
	const string RecognitionDiagnostic = "GRAM5101";

	public static async Task<DslEmbeddedSiteResult> AnalyzeAsync(
		Document document,
		SyntaxNode root,
		SemanticModel model,
		CancellationToken cancellationToken = default,
		DslEmbeddedSiteCache? cache = null)
	{
		var catalog = DslLanguageDiscovery.Discover(model.Compilation, cancellationToken);
		var classifications = new List<HostDslClassification>();
		var diagnostics = new List<HostDiagnostic>();
		var sites = new List<HostDslSite>();

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
			var markedLanguages = catalog.AttributeCarriers
				.Where(candidate => markerTypes.Any(marker => SymbolEqualityComparer.Default.Equals(
					candidate.AttributeType,
					marker)))
				.Select(static candidate => candidate.Language)
				.ToList();
			var method = parameter.ContainingSymbol as IMethodSymbol;
			var languages = markedLanguages.ToList();
			if (method is not null && parameter.Name == "input")
				languages.AddRange(catalog.Languages.Where(language => SymbolEqualityComparer.Default.Equals(
					language.ParserType,
					method.ContainingType)));

			var routes = new List<(DslLanguageDefinition Language, DslPreparedLanguage Prepared, Publication Publication)>();
			foreach (var language in languages.Distinct())
			{
				var resolution = await DslGrammarSourceResolver.ResolveAsync(
					document.Project,
					language,
					cancellationToken).ConfigureAwait(false);
				if (resolution.Kind != DslGrammarResolutionKind.Resolved || resolution.Text is null)
					continue;

				var candidate = cache?.Prepare(language, resolution.Text) ??
					Prepare(language, resolution.Text, fingerprint: "");
				if (candidate is null)
					continue;

				var generatedApi = method is not null && parameter.Name == "input" &&
					SymbolEqualityComparer.Default.Equals(language.ParserType, method.ContainingType);
				var publications = generatedApi
					? candidate.Publications.Where(publication =>
						EntryRule(language, method!) is { } descriptorEntry
							? publication.Rule.Name == descriptorEntry
							: method!.Name == publication.MethodName ||
								method.Name == "Try" + publication.MethodName).ToArray()
					: markedLanguages.Contains(language) && candidate.Publications.Count == 1
						? candidate.Publications
						: [];
				if (publications is [{ } selectedPublication])
					routes.Add((language, candidate, selectedPublication));
			}

			var distinctRoutes = routes
				.GroupBy(static route => route.Language.Id, StringComparer.Ordinal)
				.Select(static group => group.First())
				.ToArray();
			if (distinctRoutes is not [{ } route])
				continue;
			var carrier  = route.Language;
			var prepared = route.Prepared;
			var publication = route.Publication;

			var trace = DslRecognitionTrace.Recognize(
				prepared.Graph,
				publication,
				literal.Token.ValueText);
			if (sourceMap!.TryMap(0, literal.Token.ValueText.Length, out var siteSpan) &&
				sourceMap.TryMap(trace.FailurePosition, 0, out var completionSpan))
				sites.Add(new HostDslSite(
					siteSpan,
					carrier.Id,
					publication.Rule.Name,
					completionSpan.Start,
					trace.Expected));
			foreach (var classified in Classify(trace.Extents, prepared.Binding.Classifications))
				if (sourceMap!.TryMap(classified.Position, classified.Length, out var mapped))
					classifications.Add(new HostDslClassification(mapped, classified.Role));

			if (trace.Status == DslRecognitionStatus.Failure &&
				sourceMap!.TryMap(trace.FailurePosition, 0, out var failure))
			{
				diagnostics.Add(new HostDiagnostic(
					new GramDiagnostic(
						RecognitionDiagnostic,
						FailureMessage(carrier.Id, trace.Expected),
						0,
						0,
						GramSeverity.Error),
					failure,
					isExact: true));
			}
		}

		return new DslEmbeddedSiteResult(classifications, diagnostics, sites);
	}

	static string? EntryRule(DslLanguageDefinition language, IMethodSymbol method)
	{
		if (language.Entries.TryGetValue(method.Name, out var entry))
			return entry;

		return method.Name.StartsWith("Try", StringComparison.Ordinal) &&
			language.Entries.TryGetValue(method.Name.Substring(3), out entry)
				? entry
				: null;
	}

	internal static DslPreparedLanguage? Prepare(
		DslLanguageDefinition language,
		string grammarSource,
		string fingerprint)
	{
		var parsed = GramParser.Parse(GramLexer.Tokenize(grammarSource));
		var grammarModel = GrammarBinder.Bind(parsed.File);
		var graph = GrammarNormalizer.Normalize(grammarModel);
		if (parsed.Diagnostics.Any(IsError) || grammarModel.Diagnostics.Any(IsError) || graph.Diagnostics.Any(IsError))
			return null;

		var publications = graph.Publications.Where(static item => item.Kind == PublishKind.Parse).ToArray();
		if (publications.Length == 0)
			return null;

		var binding = DslClassificationBinder.Bind(language, grammarSource);
		return binding.Diagnostics.Count == 0
			? new DslPreparedLanguage(fingerprint, graph, publications, binding)
			: null;
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

	static string FailureMessage(string language, IReadOnlyList<string> expected) => expected.Count switch
	{
		0 => $"Text does not match DotGram language '{language}'.",
		1 => $"Expected {expected[0]} in DotGram language '{language}'.",
		_ => $"Expected one of {string.Join(", ", expected)} in DotGram language '{language}'.",
	};

	static bool IsError(GramDiagnostic diagnostic) => diagnostic.Severity == GramSeverity.Error;
}
