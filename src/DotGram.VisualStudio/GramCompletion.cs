using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.QuickInfo;
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

	public IAsyncCompletionSource GetOrCreate(ITextView textView)
	{
		return textView.Properties.GetOrCreateSingletonProperty(() =>
			new GramCompletionSource(
				textView.TextBuffer,
				GramBufferAnalysis.For(textView.TextBuffer),
				RoslynGramFeatures.For(textView.TextBuffer, Workspace, Documents)));
	}
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

	public IAsyncCompletionSource GetOrCreate(ITextView textView)
	{
		return textView.Properties.GetOrCreateSingletonProperty(() =>
			new EmbeddedGramCompletionSource(
				textView.TextBuffer,
				EmbeddedGrammarBufferAnalysis.For(textView.TextBuffer, Workspace, Documents),
				RoslynGramFeatures.For(textView.TextBuffer, Workspace, Documents)));
	}
}

/// <summary>
/// Visual Studio's completion source over the shared completion provider: the provider says what to
/// offer, this maps it to Visual Studio's items and answers their descriptions.
/// </summary>
abstract class GramCompletionSourceBase(ITextBuffer buffer, RoslynGramFeatures roslyn) : IAsyncCompletionSource
{
	readonly Dictionary<string, string> _descriptions = new(StringComparer.Ordinal);

	public CompletionStartData InitializeCompletion(
		CompletionTrigger trigger,
		SnapshotPoint triggerLocation,
		CancellationToken token)
	{
		var snapshot = triggerLocation.Snapshot;

		if (snapshot.TextBuffer != buffer ||
			Provider(snapshot).Start(snapshot.GetText(), triggerLocation.Position) is not { } span)
			return CompletionStartData.DoesNotParticipateInCompletion;

		return new CompletionStartData(
			CompletionParticipation.ProvidesItems,
			new SnapshotSpan(snapshot, span.Start, span.Length));
	}

	public async Task<CompletionContext> GetCompletionContextAsync(
		IAsyncCompletionSession session,
		CompletionTrigger trigger,
		SnapshotPoint triggerLocation,
		SnapshotSpan applicableToSpan,
		CancellationToken token)
	{
		var snapshot = triggerLocation.Snapshot;
		var items    = await Provider(snapshot).ItemsAsync(
			snapshot.GetText(),
			triggerLocation.Position,
			new TextSpan(applicableToSpan.Start.Position, applicableToSpan.Length),
			roslyn.GetItemsAsync,
			token).ConfigureAwait(false);

		_descriptions.Clear();

		foreach (var item in items)
			_descriptions[item.Label] = item.Description;

		return new CompletionContext(items.Select(Item).ToImmutableArray());
	}

	public Task<object> GetDescriptionAsync(
		IAsyncCompletionSession session,
		CompletionItem item,
		CancellationToken token)
	{
		return Task.FromResult<object>(
			_descriptions.TryGetValue(item.DisplayText, out var description)
				? description
				: "DotGram syntax");
	}

	/// <summary>What the analysis knows of the snapshot, as the shared provider reads it.</summary>
	protected abstract GramCompletionProvider Provider(ITextSnapshot snapshot);

	CompletionItem Item(GramCompletionItem item)
	{
		return item.Plain
			? new CompletionItem(item.Label, this)
			: new CompletionItem(
				item.Label,
				this,
				ImageElement.Empty,
				ImmutableArray<CompletionFilter>.Empty,
				item.Suffix,
				item.InsertText!,
				item.SortText ?? item.Label,
				item.FilterText ?? item.Label,
				ImmutableArray<ImageElement>.Empty);
	}
}

sealed class GramCompletionSource(
	ITextBuffer buffer,
	GramBufferAnalysis analysis,
	RoslynGramFeatures roslyn) : GramCompletionSourceBase(buffer, roslyn)
{
	protected override GramCompletionProvider Provider(ITextSnapshot snapshot)
	{
		return new StandaloneGramCompletionProvider(analysis.Document(snapshot));
	}
}

sealed class EmbeddedGramCompletionSource(
	ITextBuffer buffer,
	EmbeddedGrammarBufferAnalysis analysis,
	RoslynGramFeatures roslyn) : GramCompletionSourceBase(buffer, roslyn)
{
	protected override GramCompletionProvider Provider(ITextSnapshot snapshot)
	{
		return new EmbeddedGramCompletionProvider(analysis.View(snapshot));
	}
}

/// <summary>
/// Roslyn for a buffer: the shared questions of <see cref="RoslynGramCompletion"/>, and completion and
/// Quick Info, which are Roslyn's Features and are asked here, where the extension references them.
/// </summary>
sealed class RoslynGramFeatures(RoslynGramCompletion core, VisualStudioWorkspace workspace)
{
	public RoslynGramCompletion Core { get; } = core;

	public VisualStudioWorkspace Workspace { get; } = workspace;

	public static RoslynGramFeatures For(
		ITextBuffer buffer,
		VisualStudioWorkspace workspace,
		ITextDocumentFactoryService documents)
	{
		return new RoslynGramFeatures(
			new RoslynGramCompletion(
				workspace,
				() => documents.TryGetTextDocument(buffer, out var textDocument) ? textDocument.FilePath : null),
			workspace);
	}

	public async Task<IReadOnlyList<GramCompletionItem>> GetItemsAsync(
		string prefix,
		CancellationToken cancellationToken)
	{
		var project = Core.Project();
		if (project is null)
			return [];

		var document = RoslynGramCompletion.SyntheticDocument(project, prefix);
		var service = RoslynCompletionService.GetService(document);
		if (service is null)
			return [];

		var completions = await service.GetCompletionsAsync(
			document,
			RoslynGramCompletion.ExpressionOffset + prefix.Length,
			cancellationToken: cancellationToken).ConfigureAwait(false);
		if (completions is null)
			return [];

		return completions.ItemsList
			.GroupBy(static item => item.DisplayText, StringComparer.Ordinal)
			.Select(group => group.First())
			.Select(static item => new GramCompletionItem(
				item.DisplayText,
				RoslynGramCompletion.CSharpDescription,
				item.DisplayText,
				item.InlineDescription ?? "",
				item.SortText,
				item.FilterText,
				FromCSharp: true))
			.ToArray();
	}

	public async Task<RoslynGramQuickInfo?> GetQuickInfoAsync(
		string expression,
		int position,
		CancellationToken cancellationToken)
	{
		var project = Core.Project();
		if (project is null)
			return null;

		var document = RoslynGramCompletion.SyntheticDocument(project, expression);
		var service = QuickInfoService.GetService(document);
		if (service is null)
			return null;

		var item = await service.GetQuickInfoAsync(
			document,
			RoslynGramCompletion.ExpressionOffset + position,
			cancellationToken).ConfigureAwait(false);
		if (item is null)
			return null;

		return new RoslynGramQuickInfo(
			item.Sections
				.Select(static section => (IReadOnlyList<RoslynTaggedRun>)section.TaggedParts
					.Select(static part => new RoslynTaggedRun(part.Tag, part.Text))
					.ToArray())
				.ToArray());
	}

	/// <summary>Opens the first of the symbols Visual Studio can navigate to.</summary>
	public async Task<bool> NavigateAsync(RoslynDefinitionTargets? targets, CancellationToken cancellationToken)
	{
		if (targets is null)
			return false;

		foreach (var symbol in targets.Symbols)
			if (await RoslynSymbolNavigation.NavigateAsync(
				Workspace, symbol, targets.Project, cancellationToken).ConfigureAwait(false))
				return true;

		return false;
	}
}

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
