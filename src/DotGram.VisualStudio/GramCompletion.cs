using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.QuickInfo;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

using RoslynCompletionService = Microsoft.CodeAnalysis.Completion.CompletionService;

namespace DotGram.VisualStudio;

[Export(typeof(IAsyncCompletionSourceProvider))]
[Name("DotGram completion")]
[ContentType(GramContentType.Name)]
sealed class GramCompletionSourceProvider : IAsyncCompletionSourceProvider
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	public IAsyncCompletionSource GetOrCreate(ITextView textView) =>
		textView.Properties.GetOrCreateSingletonProperty(() =>
			new GramCompletionSource(
				textView.TextBuffer,
				GramBufferAnalysis.For(textView.TextBuffer),
				new RoslynGramCompletion(textView.TextBuffer, Workspace, Documents)));
}

[Export(typeof(IAsyncCompletionSourceProvider))]
[Name("DotGram embedded completion")]
[ContentType("CSharp")]
sealed class EmbeddedGramCompletionSourceProvider : IAsyncCompletionSourceProvider
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	public IAsyncCompletionSource GetOrCreate(ITextView textView) =>
		textView.Properties.GetOrCreateSingletonProperty(() =>
			new EmbeddedGramCompletionSource(
				textView.TextBuffer,
				EmbeddedGrammarBufferAnalysis.For(textView.TextBuffer, Workspace, Documents),
				new RoslynGramCompletion(textView.TextBuffer, Workspace, Documents)));
}

abstract class GramCompletionSourceBase : IAsyncCompletionSource
{
	static readonly string[] BuiltIns =
	[
		"any", "none", "eol", "eof", "trivia", "wordboundary",
		"using", "namespace", "parse", "find", "as", "when", "recover", "with",
		"context", "state",
	];

	readonly Dictionary<string, string> _descriptions = new(StringComparer.Ordinal);
	readonly HashSet<string> _csharpItems = new(StringComparer.Ordinal);

	public CompletionStartData InitializeCompletion(
		CompletionTrigger trigger,
		SnapshotPoint triggerLocation,
		CancellationToken token)
	{
		if (!IsApplicable(triggerLocation))
			return CompletionStartData.DoesNotParticipateInCompletion;

		return new CompletionStartData(
			CompletionParticipation.ProvidesItems,
			WordSpan(triggerLocation));
	}

	public async Task<CompletionContext> GetCompletionContextAsync(
		IAsyncCompletionSession session,
		CompletionTrigger trigger,
		SnapshotPoint triggerLocation,
		SnapshotSpan applicableToSpan,
		CancellationToken token)
	{
		var dslItems = DslCompletions(triggerLocation);
		if (dslItems.Count > 0)
		{
			_descriptions.Clear();
			_csharpItems.Clear();
			return new CompletionContext(dslItems.Select(DslCompletion).ToImmutableArray());
		}

		if (GramCSharpCompletionContext.TryGetPrefix(
			triggerLocation.Snapshot.GetText(), triggerLocation.Position, out var prefix))
		{
			var csharpItems = await CSharpCompletionsAsync(prefix, token).ConfigureAwait(false);
			_csharpItems.Clear();
			foreach (var item in csharpItems)
				_csharpItems.Add(item.DisplayText);
			return new CompletionContext(csharpItems);
		}

		var definitions = Definitions(triggerLocation);
		var names = definitions.Keys.Concat(BuiltIns).Distinct(StringComparer.Ordinal).OrderBy(static name => name);
		var items = ImmutableArray.CreateBuilder<CompletionItem>();

		_descriptions.Clear();
		_csharpItems.Clear();

		foreach (var name in names)
		{
			var definition = definitions.TryGetValue(name, out var found) ? found : default;
			var item = definition.Signature is null
				? new CompletionItem(name, this)
				: new CompletionItem(
					name,
					this,
					ImageElement.Empty,
					ImmutableArray<CompletionFilter>.Empty,
					definition.Signature.Substring(name.Length),
					definition.ParameterCount > 0 ? name + "(" : name,
					name,
					name,
					ImmutableArray<ImageElement>.Empty);

			items.Add(item);
			_descriptions[name] = definition.Description is not null
				? definition.Description
				: BuiltInDescription(name);
		}

		return new CompletionContext(items.ToImmutable());
	}

	public Task<object> GetDescriptionAsync(
		IAsyncCompletionSession session,
		CompletionItem item,
		CancellationToken token) =>
		Task.FromResult<object>(
			_descriptions.TryGetValue(item.DisplayText, out var description)
				? description
				: _csharpItems.Contains(item.DisplayText)
					? "C# symbol provided by Roslyn"
				: "DotGram syntax");

	protected abstract bool IsApplicable(SnapshotPoint point);
	protected abstract IReadOnlyDictionary<string, RuleCompletion> Definitions(SnapshotPoint point);
	protected abstract Task<ImmutableArray<CompletionItem>> CSharpCompletionsAsync(
		string prefix, CancellationToken cancellationToken);
	protected virtual IReadOnlyList<DslLiteralCompletion> DslCompletions(SnapshotPoint point) => [];

	CompletionItem DslCompletion(DslLiteralCompletion suggestion)
	{
		_descriptions[suggestion.Insertion] = $"DotGram expected literal: {suggestion.Display}";
		return new CompletionItem(
			suggestion.Insertion,
			this,
			ImageElement.Empty,
			ImmutableArray<CompletionFilter>.Empty,
			"",
			suggestion.Insertion,
			suggestion.Insertion,
			suggestion.Insertion,
			ImmutableArray<ImageElement>.Empty);
	}

	protected readonly struct RuleCompletion(string description, string signature, int parameterCount)
	{
		public string Description { get; } = description;
		public string Signature { get; } = signature;
		public int ParameterCount { get; } = parameterCount;
	}

	protected static RuleCompletion LocalCompletion(string name, GramSymbolKind kind) =>
		new(
			kind == GramSymbolKind.Parameter
				? $"{name}: DotGram rule parameter"
				: $"{name}: DotGram capture",
			name,
			0);

	static SnapshotSpan WordSpan(SnapshotPoint point)
	{
		var snapshot = point.Snapshot;
		var start    = point.Position;
		var end      = point.Position;

		while (start > 0 && IsNameCharacter(snapshot[start - 1])) start--;
		while (end < snapshot.Length && IsNameCharacter(snapshot[end])) end++;

		return new SnapshotSpan(snapshot, start, end - start);
	}

	static bool IsNameCharacter(char character) =>
		char.IsLetterOrDigit(character) || character == '_';

	static string BuiltInDescription(string name) => name switch
	{
		"any"             => "DotGram built-in rule: matches any character",
		"none"            => "DotGram built-in rule: never matches",
		"eol"             => "DotGram built-in rule: matches an end of line",
		"eof"             => "DotGram built-in rule: matches the end of input",
		"trivia"          => "DotGram built-in rule: matches grammar trivia",
		"wordboundary" => "DotGram built-in word-boundary rule",
		_                  => $"DotGram keyword: {name}",
	};
}

sealed class GramCompletionSource(
	ITextBuffer buffer,
	GramBufferAnalysis analysis,
	RoslynGramCompletion roslyn) : GramCompletionSourceBase
{
	protected override bool IsApplicable(SnapshotPoint point) => point.Snapshot.TextBuffer == buffer;

	protected override IReadOnlyDictionary<string, RuleCompletion> Definitions(SnapshotPoint point)
	{
		var document = analysis.Document(point.Snapshot);
		var definitions = document.Classifications
			.Where(static item => item.SymbolKind == GramSymbolKind.Rule &&
				item.DefinitionPosition == item.Position && item.QuickInfo is not null)
			.GroupBy(item => point.Snapshot.GetText(item.Position, item.Length), StringComparer.Ordinal)
			.ToDictionary(
				group => group.Key,
				group => new RuleCompletion(
					group.First().QuickInfo!,
					group.First().RuleSignature!,
					group.First().RuleParameterCount),
				StringComparer.Ordinal);

		foreach (var symbol in document.Symbols
			.Where(symbol => symbol.Kind != GramSymbolKind.Rule &&
				symbol.IsDefinition &&
				symbol.ScopeStart <= point.Position && point.Position < symbol.ScopeEnd)
			.GroupBy(symbol => symbol.Name, StringComparer.Ordinal))
		{
			var local = symbol.OrderBy(static item => item.Kind).First();
			definitions[local.Name] = LocalCompletion(local.Name, local.Kind);
		}

		return definitions;
	}

	protected override Task<ImmutableArray<CompletionItem>> CSharpCompletionsAsync(
		string prefix, CancellationToken cancellationToken) =>
		roslyn.GetItemsAsync(this, prefix, cancellationToken);

}

sealed class EmbeddedGramCompletionSource(
	ITextBuffer buffer,
	EmbeddedGrammarBufferAnalysis analysis,
	RoslynGramCompletion roslyn) : GramCompletionSourceBase
{
	protected override bool IsApplicable(SnapshotPoint point)
	{
		if (point.Snapshot.TextBuffer != buffer)
			return false;

		return analysis.TryGet(point.Snapshot, out var classifications, out _) &&
			classifications.Any(item => item.GrammarSpan.Contains(point.Position)) ||
			analysis.TryGetDslCompletions(point.Snapshot, point.Position, out var expected) &&
			LiteralCompletions(expected).Count > 0;
	}

	protected override IReadOnlyList<DslLiteralCompletion> DslCompletions(SnapshotPoint point) =>
		analysis.TryGetDslCompletions(point.Snapshot, point.Position, out var expected)
			? LiteralCompletions(expected)
			: [];

	static IReadOnlyList<DslLiteralCompletion> LiteralCompletions(IReadOnlyList<string> expected) => expected
		.SelectMany(DslLiteralCompletionParser.ParseAll)
		.Distinct()
		.ToArray();

	protected override IReadOnlyDictionary<string, RuleCompletion> Definitions(SnapshotPoint point)
	{
		if (!analysis.TryGet(point.Snapshot, out var classifications, out _))
			return new Dictionary<string, RuleCompletion>();

		var definitions = classifications
			.Where(item =>
				item.GrammarSpan.Contains(point.Position) &&
				item.SymbolKind == GramSymbolKind.Rule &&
				item.DefinitionSpan == item.Span &&
				item.QuickInfo is not null)
			.GroupBy(item => point.Snapshot.GetText(item.Span.Start, item.Span.Length), StringComparer.Ordinal)
			.ToDictionary(
				group => group.Key,
				group => new RuleCompletion(
					group.First().QuickInfo!,
					group.First().RuleSignature!,
					group.First().RuleParameterCount),
				StringComparer.Ordinal);

		if (analysis.TryGetSymbols(point.Snapshot, out var symbols))
			foreach (var symbol in symbols
				.Where(symbol => symbol.Kind != GramSymbolKind.Rule &&
					symbol.IsDefinition &&
					symbol.GrammarSpan.Contains(point.Position) &&
					symbol.ScopeSpan.Contains(point.Position))
				.GroupBy(symbol => symbol.Name, StringComparer.Ordinal))
			{
				var local = symbol.OrderBy(static item => item.Kind).First();
				definitions[local.Name] = LocalCompletion(local.Name, local.Kind);
			}

		return definitions;
	}

	protected override Task<ImmutableArray<CompletionItem>> CSharpCompletionsAsync(
		string prefix, CancellationToken cancellationToken) =>
		roslyn.GetItemsAsync(this, prefix, cancellationToken);
}

internal readonly record struct DslLiteralCompletion(string Display, string Insertion);

internal static class DslLiteralCompletionParser
{
	public static IReadOnlyList<DslLiteralCompletion> ParseAll(string expected)
	{
		if (Parse(expected) is { } literal)
			return [literal];
		if (expected.Length < 2 || expected[0] != '[' || expected[expected.Length - 1] != ']')
			return [];

		var parts = expected.Substring(1, expected.Length - 2).Split(
			new[] { " | " },
			StringSplitOptions.None);
		var result = parts.Select(Parse).ToArray();
		return result.All(static item => item is not null)
			? result.Select(static item => item!.Value).ToArray()
			: [];
	}

	public static DslLiteralCompletion? Parse(string expected)
	{
		var literal = expected.EndsWith("i", StringComparison.Ordinal)
			? expected.Substring(0, expected.Length - 1)
			: expected;
		if (literal.Length >= 2 && literal[0] == '"' && literal[literal.Length - 1] == '"')
			return new DslLiteralCompletion(expected, literal.Substring(1, literal.Length - 2));
		if (literal.Length < 3 || literal[0] != '\'' || literal[literal.Length - 1] != '\'')
			return null;

		var body = literal.Substring(1, literal.Length - 2);
		var insertion = body switch
		{
			"\\n"  => "\n",
			"\\r"  => "\r",
			"\\t"  => "\t",
			"\\'"  => "'",
			"\\\\" => "\\",
			_ when body.Length == 1 => body,
			_ => null,
		};
		return insertion is null ? null : new DslLiteralCompletion(expected, insertion);
	}
}

sealed class RoslynGramCompletion(
	ITextBuffer buffer,
	VisualStudioWorkspace workspace,
	ITextDocumentFactoryService documents)
{
	const string Before = "using System; class __DotGramCompletion { object __Value() { return ";
	const string After = "; } }";

	public async Task<ImmutableArray<CompletionItem>> GetItemsAsync(
		IAsyncCompletionSource source,
		string prefix,
		CancellationToken cancellationToken)
	{
		var project = Project();
		if (project is null)
			return [];

		var document = SyntheticDocument(project, prefix);
		var service = RoslynCompletionService.GetService(document);
		if (service is null)
			return [];

		var completions = await service.GetCompletionsAsync(
			document,
			Before.Length + prefix.Length,
			cancellationToken: cancellationToken).ConfigureAwait(false);
		if (completions is null)
			return [];

		return completions.ItemsList
			.GroupBy(static item => item.DisplayText, StringComparer.Ordinal)
			.Select(group => group.First())
			.Select(item => new CompletionItem(
				item.DisplayText,
				source,
				ImageElement.Empty,
				ImmutableArray<CompletionFilter>.Empty,
				item.InlineDescription ?? "",
				item.DisplayText,
				item.SortText,
				item.FilterText,
				ImmutableArray<ImageElement>.Empty))
			.ToImmutableArray();
	}

	public async Task<RoslynGramQuickInfo?> GetQuickInfoAsync(
		string expression,
		int position,
		CancellationToken cancellationToken)
	{
		var project = Project();
		if (project is null)
			return null;

		var document = SyntheticDocument(project, expression);
		var service = QuickInfoService.GetService(document);
		if (service is null)
			return null;

		var item = await service.GetQuickInfoAsync(
			document,
			Before.Length + position,
			cancellationToken).ConfigureAwait(false);
		if (item is null)
			return null;

		return new RoslynGramQuickInfo(
			item.Sections.Select(static section => section.TaggedParts).ToImmutableArray());
	}

	public async Task<bool> NavigateToDefinitionAsync(
		string expression,
		int position,
		CancellationToken cancellationToken)
	{
		var project = Project();
		if (project is null)
			return false;

		var document = SyntheticDocument(project, expression);
		var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (model is null)
			return false;

		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		var absolutePosition = Before.Length + position;
		var token = root?.FindToken(Math.Max(0, Math.Min(absolutePosition, root.FullSpan.End - 1)));
		var symbol = await QualifiedMemberAsync(
			project, expression, position, cancellationToken).ConfigureAwait(false) ??
			token?.Parent?.AncestorsAndSelf()
			.Select(node => model.GetSymbolInfo(node, cancellationToken))
			.Select(info => info.Symbol ?? info.CandidateSymbols
				.OrderByDescending(static candidate => candidate is IFieldSymbol)
				.ThenByDescending(static candidate => candidate is IPropertySymbol)
				.FirstOrDefault())
			.FirstOrDefault(static candidate => candidate is not null) ??
			await SymbolFinder.FindSymbolAtPositionAsync(
				model,
				absolutePosition,
				workspace,
				cancellationToken).ConfigureAwait(false);
		if (symbol is not null && await RoslynSymbolNavigation.NavigateAsync(
			workspace, symbol, project, cancellationToken).ConfigureAwait(false))
			return true;

		var name = IdentifierAt(expression, position);
		if (name.Length == 0)
			return false;

		var declarations = await SymbolFinder.FindDeclarationsAsync(
			project, name, ignoreCase: false, cancellationToken).ConfigureAwait(false);
		var hostType = await HostTypeAsync(project, cancellationToken).ConfigureAwait(false);
		foreach (var declaration in declarations
			.Where(candidate => candidate.Name == name)
			.OrderByDescending(candidate => hostType is not null && SymbolEqualityComparer.Default.Equals(
				candidate.ContainingType, hostType))
			.ThenByDescending(static candidate => candidate.Locations.Any(static location => location.IsInSource)))
			if (await RoslynSymbolNavigation.NavigateAsync(
				workspace, declaration, project, cancellationToken).ConfigureAwait(false))
				return true;

		return false;
	}

	internal static async Task<ISymbol?> QualifiedMemberAsync(
		Project project,
		string expression,
		int position,
		CancellationToken cancellationToken)
	{
		var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(expression, cancellationToken: cancellationToken);
		var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
		var boundedPosition = Math.Max(0, Math.Min(position, root.FullSpan.End - 1));
		var member = root.FindToken(boundedPosition).Parent?.AncestorsAndSelf()
			.OfType<MemberAccessExpressionSyntax>()
			.FirstOrDefault(candidate => candidate.Name.Span.Contains(boundedPosition));
		if (member?.Expression is not IdentifierNameSyntax typeName)
			return null;

		var declarations = await SymbolFinder.FindDeclarationsAsync(
			project, typeName.Identifier.ValueText, ignoreCase: false, cancellationToken).ConfigureAwait(false);
		return declarations
			.OfType<INamedTypeSymbol>()
			.SelectMany(type => type.GetMembers(member.Name.Identifier.ValueText))
			.OrderByDescending(static candidate => candidate is IFieldSymbol)
			.ThenByDescending(static candidate => candidate is IPropertySymbol)
			.FirstOrDefault();
	}

	public async Task<GeneratedApiSource?> GeneratedApiSourceAsync(
		int position,
		CancellationToken cancellationToken)
	{
		if (!documents.TryGetTextDocument(buffer, out var textDocument) || textDocument.FilePath is null)
			return null;

		var solution = workspace.CurrentSolution;
		var documentId = solution.GetDocumentIdsWithFilePath(textDocument.FilePath).FirstOrDefault();
		var document = documentId is null ? null : solution.GetDocument(documentId);
		if (document is null)
		{
			var fileName = FileName(textDocument.FilePath);
			foreach (var project in solution.Projects)
			{
				var generated = await project.GetSourceGeneratedDocumentsAsync(cancellationToken).ConfigureAwait(false);
				document = generated.FirstOrDefault(candidate =>
					string.Equals(candidate.Name, fileName, StringComparison.OrdinalIgnoreCase) ||
					candidate.FilePath is not null && string.Equals(
						FileName(candidate.FilePath), fileName, StringComparison.OrdinalIgnoreCase));
				if (document is not null)
					break;
			}
		}
		if (document is null)
			return null;

		var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (model is null || root is null)
			return null;

		var boundedPosition = Math.Max(0, Math.Min(position, root.FullSpan.End - 1));
		var token = root.FindToken(boundedPosition);
		var symbol = token.Parent?.AncestorsAndSelf()
			.Select(node => model.GetDeclaredSymbol(node, cancellationToken) ??
				model.GetSymbolInfo(node, cancellationToken).Symbol ??
				model.GetSymbolInfo(node, cancellationToken).CandidateSymbols.FirstOrDefault())
			.FirstOrDefault(candidate => candidate is IMethodSymbol) ??
			await SymbolFinder.FindSymbolAtPositionAsync(
				model, boundedPosition, workspace, cancellationToken).ConfigureAwait(false);
		if (symbol is not IMethodSymbol method)
			return symbol is null
				? null
				: await GrammarReferenceSourceAsync(symbol, document.Project, cancellationToken).ConfigureAwait(false);

		var names = method.Name.StartsWith("Try", StringComparison.Ordinal) && method.Name.Length > 3
			? new[] { method.Name, method.Name.Substring(3) }
			: new[] { method.Name };

		foreach (var attribute in method.ContainingType.GetAttributes())
		{
			if (attribute.AttributeClass?.ToDisplayString() != "DotGram.GramAttribute" ||
				attribute.ConstructorArguments.Length == 0 ||
				attribute.ConstructorArguments[0].Value is not string source)
				continue;

			if (source.EndsWith(".gram", StringComparison.OrdinalIgnoreCase) &&
				source.IndexOf('\n') < 0 && source.IndexOf('\r') < 0)
			{
				var grammar = solution.Projects
					.SelectMany(static project => project.AdditionalDocuments)
					.FirstOrDefault(candidate => candidate.FilePath is not null &&
						string.Equals(FileName(candidate.FilePath), FileName(source), StringComparison.OrdinalIgnoreCase));
				if (grammar?.FilePath is null)
					continue;

				var text = await grammar.GetTextAsync(cancellationToken).ConfigureAwait(false);
				var api = GramLanguageService.Analyze(text.ToString()).PublishedApis
					.FirstOrDefault(candidate => names.Contains(candidate.MethodName, StringComparer.Ordinal));
				if (api.MethodName is not null)
				{
					var location = text.Lines.GetLinePosition(api.Position);
					return new GeneratedApiSource(grammar.FilePath, location.Line, location.Character);
				}
			}
			else if (attribute.ApplicationSyntaxReference is { } syntaxReference &&
				await syntaxReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false) is AttributeSyntax syntax &&
				syntax.ArgumentList?.Arguments is [{ Expression: LiteralExpressionSyntax literal }] &&
				CSharpStringMap.TryCreate(literal.Token, out var map))
			{
				var api = GramLanguageService.Analyze(literal.Token.ValueText).PublishedApis
					.FirstOrDefault(candidate => names.Contains(candidate.MethodName, StringComparer.Ordinal));
				var host = solution.GetDocument(syntax.SyntaxTree);
				if (api.MethodName is not null && host?.FilePath is not null &&
					map!.TryMap(api.Position, api.Length, out var span))
				{
					var hostText = await host.GetTextAsync(cancellationToken).ConfigureAwait(false);
					var location = hostText.Lines.GetLinePosition(span.Start);
					return new GeneratedApiSource(host.FilePath, location.Line, location.Character);
				}
			}
		}

		return await GrammarReferenceSourceAsync(method, document.Project, cancellationToken).ConfigureAwait(false);
	}

	public async Task<CSharpFindReferences?> FindReferencesAsync(
		int position,
		CancellationToken cancellationToken)
	{
		if (!documents.TryGetTextDocument(buffer, out var textDocument) || textDocument.FilePath is null)
			return null;

		var solution = workspace.CurrentSolution;
		var documentId = solution.GetDocumentIdsWithFilePath(textDocument.FilePath).FirstOrDefault();
		var document = documentId is null ? null : solution.GetDocument(documentId);
		if (document is null)
			return null;

		var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (model is null || root is null)
			return null;

		var boundedPosition = Math.Max(0, Math.Min(position, root.FullSpan.End - 1));
		var symbol = root.FindToken(boundedPosition).Parent?.AncestorsAndSelf()
			.Select(node => model.GetDeclaredSymbol(node, cancellationToken) ?? model.GetSymbolInfo(node, cancellationToken).Symbol)
			.FirstOrDefault(static candidate => candidate is not null) ??
			await SymbolFinder.FindSymbolAtPositionAsync(model, boundedPosition, workspace, cancellationToken).ConfigureAwait(false);
		if (symbol is null)
			return null;

		var found = new List<CSharpFindReference>();
		foreach (var location in symbol.Locations.Where(static location => location.IsInSource))
			await AddSourceReferenceAsync(solution, location, found, cancellationToken).ConfigureAwait(false);

		foreach (var referenced in await SymbolFinder.FindReferencesAsync(symbol, solution, cancellationToken).ConfigureAwait(false))
			foreach (var location in referenced.Locations)
				await AddSourceReferenceAsync(solution, location.Location, found, cancellationToken).ConfigureAwait(false);

		found.AddRange(await GrammarReferencesAsync(symbol, document.Project, cancellationToken).ConfigureAwait(false));
		return new CSharpFindReferences(
			symbol.Name,
			found.GroupBy(static item => (item.FilePath, item.Position))
				.Select(static group => group.First())
				.OrderBy(static item => item.FilePath, StringComparer.OrdinalIgnoreCase)
				.ThenBy(static item => item.Position)
				.ToArray());
	}

	static async Task AddSourceReferenceAsync(
		Solution solution,
		Location location,
		ICollection<CSharpFindReference> found,
		CancellationToken cancellationToken)
	{
		var document = location.SourceTree is null ? null : solution.GetDocument(location.SourceTree);
		if (document?.FilePath is null)
			return;

		var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
		found.Add(new CSharpFindReference(document.FilePath, text.ToString(), location.SourceSpan.Start, location.SourceSpan.Length));
	}

	async Task<GeneratedApiSource?> GrammarReferenceSourceAsync(
		ISymbol symbol,
		Project project,
		CancellationToken cancellationToken)
	{
		var references = await GrammarReferencesAsync(symbol, project, cancellationToken).ConfigureAwait(false);
		if (references.Count == 0)
			return null;

		var reference = references[0];
		var text = SourceText.From(reference.Text);
		var location = text.Lines.GetLinePosition(reference.Position);
		return new GeneratedApiSource(reference.FilePath, location.Line, location.Character);
	}

	async Task<IReadOnlyList<CSharpFindReference>> GrammarReferencesAsync(
		ISymbol symbol,
		Project project,
		CancellationToken cancellationToken)
	{
		var found = new List<CSharpFindReference>();
		foreach (var grammar in project.AdditionalDocuments.Where(document =>
			document.FilePath?.EndsWith(".gram", StringComparison.OrdinalIgnoreCase) == true))
		{
			var grammarHost = grammar.FilePath is null
				? null
				: await HostTypeAsync(project, FileName(grammar.FilePath), cancellationToken).ConfigureAwait(false);
			var text = await grammar.GetTextAsync(cancellationToken).ConfigureAwait(false);
			var source = text.ToString();
			for (var position = source.IndexOf(symbol.Name, StringComparison.Ordinal);
				position >= 0;
				position = source.IndexOf(symbol.Name, position + symbol.Name.Length, StringComparison.Ordinal))
			{
				if (!IsIdentifier(source, position, symbol.Name.Length) ||
					!GramCSharpCompletionContext.TryGetExpression(
						source, position, out var expression, out var expressionStart, out _, out _))
					continue;

				var referenced = await SymbolInExpressionAsync(
					project,
					expression,
					position - expressionStart,
					cancellationToken).ConfigureAwait(false);
				if (!SymbolEqualityComparer.Default.Equals(referenced?.OriginalDefinition, symbol.OriginalDefinition) &&
					!IsUnqualifiedHostMember(expression, position - expressionStart, symbol, grammarHost))
					continue;

				if (grammar.FilePath is not null)
					found.Add(new CSharpFindReference(grammar.FilePath, source, position, symbol.Name.Length));
			}
		}

		return found;
	}

	internal static bool IsUnqualifiedHostMember(
		string expression,
		int position,
		ISymbol symbol,
		INamedTypeSymbol? grammarHost)
	{
		if (grammarHost is null || !SymbolEqualityComparer.Default.Equals(symbol.ContainingType, grammarHost))
			return false;

		var root = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(expression).GetRoot();
		var boundedPosition = Math.Max(0, Math.Min(position, root.FullSpan.End - 1));
		var name = root.FindToken(boundedPosition).Parent?.AncestorsAndSelf()
			.OfType<IdentifierNameSyntax>()
			.FirstOrDefault(candidate => candidate.Span.Contains(boundedPosition));
		return name?.Identifier.ValueText == symbol.Name && name.Parent is not MemberAccessExpressionSyntax;
	}

	static async Task<ISymbol?> SymbolInExpressionAsync(
		Project project,
		string expression,
		int position,
		CancellationToken cancellationToken)
	{
		var document = SyntheticDocument(project, expression);
		var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (model is null || root is null)
			return null;

		var absolute = Before.Length + position;
		var token = root.FindToken(Math.Max(0, Math.Min(absolute, root.FullSpan.End - 1)));
		return token.Parent?.AncestorsAndSelf()
			.Select(node => model.GetSymbolInfo(node, cancellationToken))
			.Select(info => info.Symbol ?? info.CandidateSymbols.FirstOrDefault())
			.FirstOrDefault(static candidate => candidate is not null);
	}

	static bool IsIdentifier(string source, int position, int length) =>
		(position == 0 || !IsIdentifierCharacter(source[position - 1])) &&
		(position + length == source.Length || !IsIdentifierCharacter(source[position + length]));

	async Task<INamedTypeSymbol?> HostTypeAsync(Project project, CancellationToken cancellationToken)
	{
		if (!documents.TryGetTextDocument(buffer, out var textDocument) || textDocument.FilePath is null)
			return null;

		return await HostTypeAsync(project, Path.GetFileName(textDocument.FilePath), cancellationToken).ConfigureAwait(false);
	}

	static async Task<INamedTypeSymbol?> HostTypeAsync(
		Project project,
		string grammarFile,
		CancellationToken cancellationToken)
	{
		foreach (var document in project.Documents)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			if (root is null || model is null)
				continue;

			foreach (var declaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
			{
				var type = model.GetDeclaredSymbol(declaration, cancellationToken) as INamedTypeSymbol;
				if (type is null)
					continue;

				foreach (var attribute in type.GetAttributes())
				{
					if (attribute.AttributeClass?.ToDisplayString() != "DotGram.GramAttribute")
						continue;

					var source = attribute.ConstructorArguments.Length > 0
						? attribute.ConstructorArguments[0].Value as string
						: null;
					if (source is not null && (!source.EndsWith(".gram", StringComparison.OrdinalIgnoreCase) ||
						source.IndexOf('\n') >= 0 || source.IndexOf('\r') >= 0))
						continue;

					var wanted = source is null ? type.Name + ".gram" : FileName(source);
					if (string.Equals(wanted, grammarFile, StringComparison.OrdinalIgnoreCase))
						return type;
				}
			}
		}

		return null;
	}

	static string FileName(string path)
	{
		var separator = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
		return separator < 0 ? path : path.Substring(separator + 1);
	}

	static string IdentifierAt(string expression, int position)
	{
		var start = Math.Min(position, expression.Length);
		var end = start;
		while (start > 0 && IsIdentifierCharacter(expression[start - 1])) start--;
		while (end < expression.Length && IsIdentifierCharacter(expression[end])) end++;
		return expression.Substring(start, end - start);
	}

	static bool IsIdentifierCharacter(char character) =>
		char.IsLetterOrDigit(character) || character == '_';

	static Document SyntheticDocument(Project project, string expression) =>
		project.AddDocument(
			"__DotGramCompletion.cs",
			SourceText.From(Before + expression + After));

	Project? Project()
	{
		if (documents.TryGetTextDocument(buffer, out var textDocument) &&
			textDocument.FilePath is not null)
		{
			var solution = workspace.CurrentSolution;
			var id = solution.GetDocumentIdsWithFilePath(textDocument.FilePath).FirstOrDefault();
			if (id is not null)
				return solution.GetProject(id.ProjectId);


			var additionalProject = solution.Projects.FirstOrDefault(project =>
				project.AdditionalDocuments.Any(document =>
					string.Equals(document.FilePath, textDocument.FilePath, StringComparison.OrdinalIgnoreCase)));
			if (additionalProject is not null)
				return additionalProject;
		}

		return workspace.CurrentSolution.Projects.FirstOrDefault(
			static project => project.Language == LanguageNames.CSharp);
	}
}

readonly record struct GeneratedApiSource(string FilePath, int Line, int Column);

readonly record struct CSharpFindReference(string FilePath, string Text, int Position, int Length);

sealed record CSharpFindReferences(string Name, IReadOnlyList<CSharpFindReference> References);

static class RoslynSymbolNavigation
{
	public static async Task<bool> NavigateAsync(
		Workspace workspace,
		ISymbol symbol,
		Project project,
		CancellationToken cancellationToken)
	{
		try
		{
			var features = typeof(QuickInfoService).Assembly;
			var serviceType = features.GetType("Microsoft.CodeAnalysis.Navigation.ISymbolNavigationService");
			var locationType = features.GetType("Microsoft.CodeAnalysis.Navigation.INavigableLocation");
			var optionsType = features.GetType("Microsoft.CodeAnalysis.Navigation.NavigationOptions");
			if (serviceType is null || locationType is null || optionsType is null)
				return false;

			var getService = workspace.Services.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.FirstOrDefault(method => method.Name == "GetService" &&
					method.IsGenericMethodDefinition && method.GetParameters().Length == 0);
			var service = getService?.MakeGenericMethod(serviceType).Invoke(workspace.Services, null);
			if (service is null)
				return false;

			var getLocation = serviceType.GetMethod("GetNavigableLocationAsync");
			var pendingLocation = getLocation?.Invoke(service, [symbol, project, cancellationToken]);
			var location = pendingLocation is null ? null : await ResultAsync(pendingLocation).ConfigureAwait(false);
			if (location is null)
				return false;

			var options = Activator.CreateInstance(optionsType, true, true);
			var navigate = locationType.GetMethod("NavigateToAsync");
			var pendingNavigation = navigate?.Invoke(location, [options, cancellationToken]);
			var result = pendingNavigation is null ? null : await ResultAsync(pendingNavigation).ConfigureAwait(false);
			return result is true;
		}
		catch (Exception exception) when (exception is not OutOfMemoryException)
		{
			Microsoft.VisualStudio.Shell.ActivityLog.LogError("DotGram.VisualStudio", exception.ToString());
			return false;
		}
	}

	static async Task<object?> ResultAsync(object awaitable)
	{
		var task = awaitable as Task;
		if (task is null)
		{
			var asTask = awaitable.GetType().GetMethod("AsTask", Type.EmptyTypes);
			task = asTask?.Invoke(awaitable, null) as Task;
		}
		if (task is null)
			return null;

		await task.ConfigureAwait(false);
		return task.GetType().GetProperty("Result")?.GetValue(task);
	}
}

sealed class RoslynGramQuickInfo(ImmutableArray<ImmutableArray<TaggedText>> Sections)
{
	public ImmutableArray<ImmutableArray<TaggedText>> Sections { get; } = Sections;
}
