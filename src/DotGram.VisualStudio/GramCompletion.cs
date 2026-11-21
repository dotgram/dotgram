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
		return EditorGuard.Run<IAsyncCompletionSource>(
			"Completion source",
			null!,
			() => textView.Properties.GetOrCreateSingletonProperty(() =>
				new GramCompletionSource(
					textView.TextBuffer,
					GramBufferAnalysis.For(textView.TextBuffer),
					RoslynGramFeatures.For(textView.TextBuffer, Workspace, Documents))));
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
		return EditorGuard.Run<IAsyncCompletionSource>(
			"Embedded completion source",
			null!,
			() => textView.Properties.GetOrCreateSingletonProperty(() =>
				new EmbeddedGramCompletionSource(
					textView.TextBuffer,
					EmbeddedGrammarBufferAnalysis.For(textView.TextBuffer, Workspace, Documents),
					RoslynGramFeatures.For(textView.TextBuffer, Workspace, Documents))));
	}
}

/// <summary>
/// What typing does to a session the DotGram sources opened: a name's character narrows the list, and
/// any other closes it without committing; Tab and Enter after a name typed in full commit it and still
/// indent and break the line.
/// </summary>
/// <remarks>
/// Without a commit manager of its own, a session of the <c>.gram</c> content type had no character that
/// ended it: opened on a name, it stayed open past every space and colon, and the next Tab replaced
/// all that was typed since with the selected item. In a C# document the same manager is asked about
/// Roslyn's sessions too, and leaves to Roslyn's every session the DotGram sources took no part in.
/// </remarks>
[Export(typeof(IAsyncCompletionCommitManagerProvider))]
[Name("DotGram completion commit")]
[ContentType(GramContentType.Name)]
[ContentType("CSharp")]
sealed class GramCompletionCommitManagerProvider : IAsyncCompletionCommitManagerProvider
{
	public IAsyncCompletionCommitManager GetOrCreate(ITextView textView)
	{
		return GramCompletionCommitManager.Instance;
	}
}

sealed class GramCompletionCommitManager : IAsyncCompletionCommitManager2
{
	public static GramCompletionCommitManager Instance { get; } = new();

	public IEnumerable<char> PotentialCommitCharacters => [];

	public void Initialize(IAsyncCompletionSession session)
	{
	}

	public CompletionContinuation HandleTypedChar(
		IAsyncCompletionSession session,
		SnapshotPoint location,
		char typedChar,
		CancellationToken token)
	{
		if (!GramCompletionSourceBase.Opened(session) || GramCompletionProvider.KeepsOpenOnTyping(typedChar))
			return CompletionContinuation.Continue;

		return CompletionContinuation.Dismiss;
	}

	public bool ShouldCommitCompletion(
		IAsyncCompletionSession session,
		SnapshotPoint location,
		char typedChar,
		CancellationToken token)
	{
		return false;
	}

	public CommitResult TryCommit(
		IAsyncCompletionSession session,
		ITextBuffer buffer,
		CompletionItem item,
		char typedChar,
		CancellationToken token)
	{
		// A name typed in full is committed as it is, and the key goes on to do what it does without a list.
		if (item.Source is GramCompletionSourceBase &&
			typedChar is '\t' or '\n' &&
			string.Equals(session.ApplicableToSpan.GetText(buffer.CurrentSnapshot), item.InsertText, StringComparison.Ordinal))
			return new CommitResult(false, CommitBehavior.RaiseFurtherReturnKeyAndTabKeyCommandHandlers);

		return CommitResult.Unhandled;
	}
}

/// <summary>
/// Visual Studio's completion source over the shared completion provider: the provider says what to
/// offer, this maps it to Visual Studio's items and answers their descriptions.
/// </summary>
abstract class GramCompletionSourceBase(ITextBuffer buffer, RoslynGramFeatures roslyn) : IAsyncCompletionSource
{
	readonly Dictionary<string, string> _descriptions = new(StringComparer.Ordinal);

	static readonly object OpenedKey = new();

	/// <summary>Whether a DotGram source took part in the session.</summary>
	public static bool Opened(IAsyncCompletionSession session)
	{
		return session.Properties.ContainsProperty(OpenedKey);
	}

	public CompletionStartData InitializeCompletion(
		CompletionTrigger trigger,
		SnapshotPoint triggerLocation,
		CancellationToken token)
	{
		return EditorGuard.Run(
			"Completion start",
			CompletionStartData.DoesNotParticipateInCompletion,
			() => Start(trigger, triggerLocation),
			token);
	}

	public Task<CompletionContext> GetCompletionContextAsync(
		IAsyncCompletionSession session,
		CompletionTrigger trigger,
		SnapshotPoint triggerLocation,
		SnapshotSpan applicableToSpan,
		CancellationToken token)
	{
		return EditorGuard.RunAsync(
			"Completion",
			CompletionContext.Empty,
			() =>
			{
				// The session is null where a test asks for items outside one.
				if (session is not null)
					session.Properties[OpenedKey] = true;

				return ContextAsync(triggerLocation, applicableToSpan, token);
			},
			token);
	}

	public Task<object> GetDescriptionAsync(
		IAsyncCompletionSession session,
		CompletionItem item,
		CancellationToken token)
	{
		return EditorGuard.Run("Completion description", Task.FromResult<object>(""), () => DescriptionAsync(item), token);
	}

	CompletionStartData Start(CompletionTrigger trigger, SnapshotPoint triggerLocation)
	{
		var snapshot = triggerLocation.Snapshot;

		if (snapshot.TextBuffer != buffer ||
			!Opens(trigger, snapshot, triggerLocation.Position) ||
			Provider(snapshot).Start(snapshot.Length, index => snapshot[index], triggerLocation.Position) is not { } span)
			return CompletionStartData.DoesNotParticipateInCompletion;

		return new CompletionStartData(
			CompletionParticipation.ProvidesItems,
			new SnapshotSpan(snapshot, span.Start, span.Length));
	}

	/// <summary>
	/// Whether what the editor did opens a session: asking for completion always does; typing and
	/// deleting only where the shared provider says, so that whitespace, a line break or a tab never
	/// opens a list that the next Tab or Enter would commit from.
	/// </summary>
	static bool Opens(CompletionTrigger trigger, ITextSnapshot snapshot, int position)
	{
		switch (trigger.Reason)
		{
			case CompletionTriggerReason.Invoke:
			case CompletionTriggerReason.InvokeAndCommitIfUnique:
			case CompletionTriggerReason.InvokeMatchingType:
				return true;

			case CompletionTriggerReason.Insertion:
				return GramCompletionProvider.OpensOnTyping(snapshot.Length, index => snapshot[index], position, trigger.Character);

			case CompletionTriggerReason.Backspace:
			case CompletionTriggerReason.Deletion:
				return GramCompletionProvider.OpensOnDeletion(snapshot.Length, index => snapshot[index], position);

			default:
				return false;
		}
	}

	async Task<CompletionContext> ContextAsync(
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

		// Where nothing is typed yet, after `.` or where completion was asked for between words, the
		// first item is only proposed: Enter, a space or a commit character type as they would.
		return new CompletionContext(
			items.Select(Item).ToImmutableArray(),
			null,
			applicableToSpan.Length == 0 ? InitialSelectionHint.SoftSelection : InitialSelectionHint.RegularSelection);
	}

	Task<object> DescriptionAsync(CompletionItem item)
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

	/// <summary>Opens what the name binds to, or else the first of its declarations Visual Studio can navigate to.</summary>
	public async Task<bool> NavigateAsync(RoslynDefinitionTargets? targets, CancellationToken cancellationToken)
	{
		if (targets is null)
			return false;

		if (targets.Bound is { } bound && await RoslynSymbolNavigation.NavigateAsync(
			Workspace, bound, targets.Project, cancellationToken).ConfigureAwait(false))
			return true;

		if (targets.DeclarationsAsync is null)
			return false;

		foreach (var symbol in await targets.DeclarationsAsync(cancellationToken).ConfigureAwait(false))
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
		catch (Exception exception) when (EditorGuard.Recoverable(exception))
		{
			EditorGuard.Log("Roslyn symbol navigation", exception);
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
