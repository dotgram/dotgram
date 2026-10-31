using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Language;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

[Export(typeof(INavigableSymbolSourceProvider))]
[Name("DotGram navigation")]
[Order(Before = "default")]
[ContentType(GramContentType.Name)]
sealed class GramNavigableSymbolSourceProvider : INavigableSymbolSourceProvider
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	[Import(typeof(SVsServiceProvider))]
	IServiceProvider Services { get; set; } = null!;

	public INavigableSymbolSource TryCreateNavigableSymbolSource(ITextView textView, ITextBuffer buffer)
	{
		var analysis = GramBufferAnalysis.For(buffer);
		GramClassifierProvider.Configure(analysis, buffer, Workspace, Documents);
		return new GramNavigableSymbolSource(textView, buffer, analysis, Services);
	}
}

[Export(typeof(INavigableSymbolSourceProvider))]
[Name("DotGram embedded navigation")]
[Order(Before = "default")]
[ContentType("CSharp")]
sealed class EmbeddedGramNavigableSymbolSourceProvider : INavigableSymbolSourceProvider
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	[Import(typeof(SVsServiceProvider))]
	IServiceProvider Services { get; set; } = null!;

	public INavigableSymbolSource TryCreateNavigableSymbolSource(ITextView textView, ITextBuffer buffer)
	{
		return new EmbeddedGramNavigableSymbolSource(
			textView,
			buffer,
			EmbeddedGrammarBufferAnalysis.For(buffer, Workspace, Documents),
			Services,
			Workspace,
			Documents);
	}
}

sealed class GramNavigableSymbolSource(
	ITextView view,
	ITextBuffer buffer,
	GramBufferAnalysis analysis,
	IServiceProvider services) : INavigableSymbolSource
{
	public Task<INavigableSymbol?> GetNavigableSymbolAsync(
		SnapshotSpan triggerSpan,
		CancellationToken token)
	{
		var snapshot = buffer.CurrentSnapshot;
		var position = triggerSpan.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive).Start.Position;

		return Task.FromResult(GramNavigationProvider.Navigable(analysis.View(snapshot), position) is { } navigable
			? Symbol(view, snapshot, services, analysis.Store, navigable)
			: null);
	}

	/// <summary>The Visual Studio symbol for what the shared provider says a click navigates to.</summary>
	internal static INavigableSymbol Symbol(
		ITextView view,
		ITextSnapshot snapshot,
		IServiceProvider services,
		IIncludedGrammarStore store,
		GramNavigable navigable)
	{
		var span = new SnapshotSpan(snapshot, navigable.Span.Start, navigable.Span.Length);

		return navigable.Target switch
		{
			GramLocation.InDocument target => new GramNavigableSymbol(
				view, span, snapshot.CreateTrackingPoint(target.Position, PointTrackingMode.Negative)),
			GramLocation.InFile target => new FileNavigableSymbol(span, services, target.Path, target.Line, target.Column),
			GramLocation.InIncludedGrammar target => new IncludedGrammarNavigableSymbol(span, services, store, target),
			_ => new NonNavigableSymbol(span),
		};
	}

	public void Dispose()
	{
	}
}

sealed class EmbeddedGramNavigableSymbolSource(
	ITextView view,
	ITextBuffer buffer,
	EmbeddedGrammarBufferAnalysis analysis,
	IServiceProvider services,
	VisualStudioWorkspace workspace,
	ITextDocumentFactoryService documents) : INavigableSymbolSource
{
	public async Task<INavigableSymbol?> GetNavigableSymbolAsync(
		SnapshotSpan triggerSpan,
		CancellationToken token)
	{
		var snapshot = buffer.CurrentSnapshot;
		var position = triggerSpan.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive).Start.Position;
		var navigable = await GramNavigationProvider.NavigableAsync(
			snapshot.GetText(),
			analysis.View(snapshot),
			position,
			RoslynGramFeatures.For(buffer, workspace, documents).Core,
			token).ConfigureAwait(false);

		return navigable is { } found
			? GramNavigableSymbolSource.Symbol(view, snapshot, services, TempDirectoryIncludedGrammarStore.Instance, found)
			: null;
	}

	public void Dispose()
	{
	}
}

sealed class FileNavigableSymbol(
	SnapshotSpan symbolSpan,
	IServiceProvider services,
	string filePath,
	int line,
	int column) : INavigableSymbol
{
	static readonly IReadOnlyCollection<INavigableRelationship> Definition =
		new[] { PredefinedNavigableRelationships.Definition };

	public SnapshotSpan SymbolSpan => symbolSpan;

	public IEnumerable<INavigableRelationship> Relationships => Definition;

	public void Navigate(INavigableRelationship relationship)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		if (relationship != PredefinedNavigableRelationships.Definition)
			return;

		Open(services, filePath, line, column);
	}

	internal static void Open(IServiceProvider services, string filePath, int line, int column)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		VsShellUtilities.OpenDocument(
			services,
			filePath,
			Guid.Empty,
			out _,
			out _,
			out IVsWindowFrame frame,
			out IVsTextView textView);
		frame.Show();
		textView.SetCaretPos(line, column);
		textView.CenterLines(line, 1);
	}
}

/// <summary>
/// A name whose definition is in an included grammar. Where that grammar is only carried by an
/// assembly, its read-only copy is written when the navigation happens, not when the name is
/// hovered.
/// </summary>
sealed class IncludedGrammarNavigableSymbol(
	SnapshotSpan symbolSpan,
	IServiceProvider services,
	IIncludedGrammarStore store,
	GramLocation.InIncludedGrammar target) : INavigableSymbol
{
	static readonly IReadOnlyCollection<INavigableRelationship> Definition =
		new[] { PredefinedNavigableRelationships.Definition };

	public SnapshotSpan SymbolSpan => symbolSpan;

	public IEnumerable<INavigableRelationship> Relationships => Definition;

	public void Navigate(INavigableRelationship relationship)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		if (relationship != PredefinedNavigableRelationships.Definition)
			return;

		if (store.PathToOpen(target.Grammar) is { } path)
			FileNavigableSymbol.Open(services, path, target.Line, target.Column);
	}
}

sealed class NonNavigableSymbol(SnapshotSpan symbolSpan) : INavigableSymbol
{
	public SnapshotSpan SymbolSpan => symbolSpan;

	public IEnumerable<INavigableRelationship> Relationships => Array.Empty<INavigableRelationship>();

	public void Navigate(INavigableRelationship relationship)
	{
	}
}

sealed class GramNavigableSymbol(
	ITextView view,
	SnapshotSpan symbolSpan,
	ITrackingPoint definition) : INavigableSymbol
{
	static readonly IReadOnlyCollection<INavigableRelationship> Definition =
		new[] { PredefinedNavigableRelationships.Definition };

	public SnapshotSpan SymbolSpan => symbolSpan;

	public IEnumerable<INavigableRelationship> Relationships => Definition;

	public void Navigate(INavigableRelationship relationship)
	{
		if (relationship != PredefinedNavigableRelationships.Definition)
			return;

		var point = definition.GetPoint(view.TextSnapshot);

		view.Caret.MoveTo(point);
		view.ViewScroller.EnsureSpanVisible(new SnapshotSpan(point, 0));
	}
}
