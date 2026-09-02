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
internal readonly record struct HostDslSymbol(
	TextSpan Span,
	string Role,
	string Target,
	string? DefinitionPath,
	int DefinitionLine,
	int DefinitionColumn);
internal readonly record struct HostDslSite(
	TextSpan Span,
	string LanguageId,
	string EntryRule,
	int CompletionPosition,
	IReadOnlyList<string> Expected);

internal sealed class DslEmbeddedSiteResult(
	IReadOnlyList<HostDslClassification> classifications,
	IReadOnlyList<HostDslSymbol> symbols,
	IReadOnlyList<HostDiagnostic> diagnostics,
	IReadOnlyList<HostDslSite> sites)
{
	public IReadOnlyList<HostDslClassification> Classifications { get; } = classifications;
	public IReadOnlyList<HostDslSymbol> Symbols { get; } = symbols;
	public IReadOnlyList<HostDiagnostic> Diagnostics { get; } = diagnostics;
	public IReadOnlyList<HostDslSite> Sites { get; } = sites;
}

internal sealed class DslPreparedLanguage(
	string fingerprint,
	RecognitionGraph graph,
	IReadOnlyList<Publication> publications,
	DslClassificationBinding binding,
	IDslRecognitionContract? recognitionContract)
{
	public string Fingerprint { get; } = fingerprint;
	public RecognitionGraph Graph { get; } = graph;
	public IReadOnlyList<Publication> Publications { get; } = publications;
	public DslClassificationBinding Binding { get; } = binding;
	public IDslRecognitionContract? RecognitionContract { get; } = recognitionContract;
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
			language.Classifications.Select(static item => item.Target + "\u001d" + item.Role)) +
			"\u001e" + string.Join("\u001f", language.RecognitionContract.Guards.Select(static item =>
				"G\u001d" + item.Key + "\u001d" + item.Value)) +
			"\u001e" + string.Join("\u001f", language.RecognitionContract.Externals.Select(static item =>
				"E\u001d" + item.Key + "\u001d" + item.Value));

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
	const string StringSyntaxAttribute = "System.Diagnostics.CodeAnalysis.StringSyntaxAttribute";

	public static async Task<DslEmbeddedSiteResult> AnalyzeAsync(
		Document document,
		SyntaxNode root,
		SemanticModel model,
		CancellationToken cancellationToken = default,
		DslEmbeddedSiteCache? cache = null)
	{
		var catalog = DslLanguageDiscovery.Discover(model.Compilation, cancellationToken);
		var classifications = new List<HostDslClassification>();
		var symbols = new List<HostDslSymbol>();
		var diagnostics = new List<HostDiagnostic>();
		var sites = new List<HostDslSite>();
		var candidates = new List<(LiteralExpressionSyntax Literal, ISymbol Annotation, IMethodSymbol? Method)>();

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
				argument.Expression is not LiteralExpressionSyntax literal)
				continue;

			candidates.Add((literal, parameter, parameter.ContainingSymbol as IMethodSymbol));
		}

		foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
		{
			cancellationToken.ThrowIfCancellationRequested();

			if ((model.GetSymbolInfo(invocation, cancellationToken).Symbol as IMethodSymbol) is not
				{
					ReducedFrom.Parameters:
					[
						{ Type.SpecialType: SpecialType.System_String } receiver,
						..
					],
				} ||
				invocation.Expression is not MemberAccessExpressionSyntax
				{
					Expression: LiteralExpressionSyntax literal,
				})
				continue;

			candidates.Add((literal, receiver, null));
		}

		foreach (var declarator in root.DescendantNodes().OfType<VariableDeclaratorSyntax>())
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (declarator.Initializer?.Value is LiteralExpressionSyntax literal &&
				model.GetDeclaredSymbol(declarator, cancellationToken) is IFieldSymbol
				{
					Type.SpecialType: SpecialType.System_String,
				} field)
				candidates.Add((literal, field, null));
		}

		foreach (var property in root.DescendantNodes().OfType<PropertyDeclarationSyntax>())
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (property.Initializer?.Value is LiteralExpressionSyntax literal &&
				model.GetDeclaredSymbol(property, cancellationToken) is IPropertySymbol
				{
					Type.SpecialType: SpecialType.System_String,
				} symbol)
				candidates.Add((literal, symbol, null));
		}

		foreach (var (literal, annotation, method) in candidates)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (!CSharpStringMap.TryCreate(literal.Token, out var sourceMap))
				continue;

			var syntaxIds = annotation.GetAttributes()
				.Where(static attribute =>
					attribute.AttributeClass?.ToDisplayString() == StringSyntaxAttribute &&
					attribute.ConstructorArguments is [{ Value: string }, ..])
				.Select(static attribute => attribute.ConstructorArguments[0].Value as string)
				.Where(static id => !string.IsNullOrWhiteSpace(id))
				.Cast<string>()
				.ToHashSet(StringComparer.Ordinal);
			var syntaxLanguages = catalog.Languages
				.Where(candidate => syntaxIds.Contains(candidate.Id))
				.ToList();
			var languages = syntaxLanguages.ToList();
			var generatedApiParameter = annotation is IParameterSymbol { Name: "input" };
			if (method is not null && generatedApiParameter)
				languages.AddRange(catalog.Languages.Where(language => SymbolEqualityComparer.Default.Equals(
					language.ParserType,
					method.ContainingType)));

			var routes = new List<(
				DslLanguageDefinition Language,
				DslPreparedLanguage Prepared,
				Publication Publication,
				string? DefinitionPath,
				SourceText? DefinitionText)>();
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

				var generatedApi = method is not null && generatedApiParameter &&
					SymbolEqualityComparer.Default.Equals(language.ParserType, method.ContainingType);
				var publications = generatedApi
					? candidate.Publications.Where(publication =>
						EntryRule(language, method!) is { } descriptorEntry
							? publication.Rule.Name == descriptorEntry
							: method!.Name == publication.MethodName ||
								method.Name == "Try" + publication.MethodName).ToArray()
					: syntaxLanguages.Contains(language) && candidate.Publications.Count == 1
						? candidate.Publications
						: [];
				if (publications is [{ } selectedPublication])
				{
					var definitionText = resolution.Document is null
						? null
						: await resolution.Document.GetTextAsync(cancellationToken).ConfigureAwait(false);
					routes.Add((
						language,
						candidate,
						selectedPublication,
						resolution.Document?.FilePath,
						definitionText));
				}
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
				literal.Token.ValueText,
				prepared.RecognitionContract);
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
			foreach (var symbol in Describe(trace.Extents, prepared.Binding.Classifications))
				if (sourceMap!.TryMap(symbol.Position, symbol.Length, out var mapped))
				{
					var definition = route.DefinitionText is not null &&
						symbol.DefinitionPosition <= route.DefinitionText.Length
						? route.DefinitionText.Lines.GetLinePosition(symbol.DefinitionPosition)
						: default;
					var hasDefinition = route.DefinitionPath is not null && route.DefinitionText is not null &&
						symbol.DefinitionPosition <= route.DefinitionText.Length;
					symbols.Add(new HostDslSymbol(
						mapped,
						symbol.Role,
						symbol.Target,
						hasDefinition ? route.DefinitionPath : null,
						hasDefinition ? definition.Line : -1,
						hasDefinition ? definition.Character : -1));
				}

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

		return new DslEmbeddedSiteResult(classifications, symbols, diagnostics, sites);
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
		var contract = language.RecognitionContract.Guards.Count == 0 &&
			language.RecognitionContract.Externals.Count == 0
			? null
			: new DslDescriptorRecognitionContract(graph, language.RecognitionContract);
		return binding.Diagnostics.Count == 0
			? new DslPreparedLanguage(fingerprint, graph, publications, binding, contract)
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

	internal static IReadOnlyList<(
		int Position,
		int Length,
		string Role,
		string Target,
		int DefinitionPosition)> Describe(
		IReadOnlyList<DslRecognitionExtent> extents,
		IReadOnlyList<DslBoundClassification> bindings)
	{
		var result = new List<(
			int Position,
			int Length,
			string Role,
			string Target,
			int DefinitionPosition)>();

		foreach (var extent in extents)
		foreach (var binding in bindings)
		{
			if (extent.Rule.Declaration?.At.Position != binding.RuleDefinitionPosition)
				continue;

			if (binding.TargetKind == DslClassificationTargetKind.Rule && extent.Capture is null ||
				binding.TargetKind == DslClassificationTargetKind.Capture &&
				extent.Capture == CaptureName(binding.Definition.Target))
				result.Add((
					extent.Position,
					extent.Length,
					binding.Definition.Role,
					binding.Definition.Target,
					binding.CaptureDefinitionPosition ?? binding.RuleDefinitionPosition));
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
