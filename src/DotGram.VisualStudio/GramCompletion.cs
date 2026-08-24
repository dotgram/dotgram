using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
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
		"any", "none", "eol", "eof", "trivia", "KeywordBoundary",
		"using", "namespace", "parse", "find", "as", "when", "recover", "with",
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
		"KeywordBoundary" => "DotGram built-in keyword-boundary rule",
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
		if (point.Snapshot.TextBuffer != buffer ||
			!analysis.TryGet(point.Snapshot, out var classifications, out _))
			return false;

		return classifications.Any(item => item.GrammarSpan.Contains(point.Position));
	}

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

sealed class RoslynGramCompletion(
	ITextBuffer buffer,
	VisualStudioWorkspace workspace,
	ITextDocumentFactoryService documents)
{
	public async Task<ImmutableArray<CompletionItem>> GetItemsAsync(
		IAsyncCompletionSource source,
		string prefix,
		CancellationToken cancellationToken)
	{
		var project = Project();
		if (project is null)
			return [];

		const string before = "using System; class __DotGramCompletion { object __Value() { return ";
		const string after = "; } }";
		var document = project.AddDocument(
			"__DotGramCompletion.cs",
			SourceText.From(before + prefix + after));
		var service = RoslynCompletionService.GetService(document);
		if (service is null)
			return [];

		var completions = await service.GetCompletionsAsync(
			document,
			before.Length + prefix.Length,
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
