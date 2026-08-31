using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

[Export(typeof(INavigableSymbolSourceProvider))]
[Name("DotGram navigation")]
[Order(Before = "default")]
[ContentType(GramContentType.Name)]
sealed class GramNavigableSymbolSourceProvider : INavigableSymbolSourceProvider
{
	public INavigableSymbolSource TryCreateNavigableSymbolSource(ITextView textView, ITextBuffer buffer) =>
		new GramNavigableSymbolSource(textView, buffer, GramBufferAnalysis.For(buffer));
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

	public INavigableSymbolSource TryCreateNavigableSymbolSource(ITextView textView, ITextBuffer buffer) =>
		new EmbeddedGramNavigableSymbolSource(
			textView,
			buffer,
			EmbeddedGrammarBufferAnalysis.For(buffer, Workspace, Documents));
}

sealed class GramNavigableSymbolSource(
	ITextView view,
	ITextBuffer buffer,
	GramBufferAnalysis analysis) : INavigableSymbolSource
{
	public Task<INavigableSymbol?> GetNavigableSymbolAsync(
		SnapshotSpan triggerSpan,
		CancellationToken token)
	{
		var snapshot = buffer.CurrentSnapshot;
		var position = triggerSpan.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive).Start.Position;

		foreach (var item in analysis.Document(snapshot).Symbols)
			if (item.Position <= position && position < item.Position + item.Length)
				return Task.FromResult<INavigableSymbol?>(Create(
					view,
					snapshot,
					item.Position,
					item.Length,
					item.DefinitionPosition));

		return Task.FromResult<INavigableSymbol?>(null);
	}

	internal static INavigableSymbol Create(
		ITextView view,
		ITextSnapshot snapshot,
		int symbolPosition,
		int symbolLength,
		int definitionPosition) =>
		new GramNavigableSymbol(
			view,
			new SnapshotSpan(snapshot, symbolPosition, symbolLength),
			snapshot.CreateTrackingPoint(definitionPosition, PointTrackingMode.Negative));

	public void Dispose()
	{
	}
}

sealed class EmbeddedGramNavigableSymbolSource(
	ITextView view,
	ITextBuffer buffer,
	EmbeddedGrammarBufferAnalysis analysis) : INavigableSymbolSource
{
	public Task<INavigableSymbol?> GetNavigableSymbolAsync(
		SnapshotSpan triggerSpan,
		CancellationToken token)
	{
		var snapshot = buffer.CurrentSnapshot;
		var position = triggerSpan.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive).Start.Position;

		if (!analysis.TryGetSymbols(snapshot, out var symbols))
			return Task.FromResult<INavigableSymbol?>(null);

		foreach (var item in symbols)
			if (item.Span.Contains(position))
				return Task.FromResult<INavigableSymbol?>(GramNavigableSymbolSource.Create(
					view,
					snapshot,
					item.Span.Start,
					item.Span.Length,
					item.DefinitionSpan.Start));

		// Returning null here lets the default C# provider treat the containing string
		// literal as one large navigable symbol. Claim positions inside an embedded
		// grammar even while its semantic spans are being refreshed after an edit.
		if (analysis.TryGet(snapshot, out var classifications, out _) &&
			classifications.Any(item => item.GrammarSpan.Contains(position)))
			return Task.FromResult<INavigableSymbol?>(new NonNavigableSymbol(
				new SnapshotSpan(snapshot, position, position < snapshot.Length ? 1 : 0)));

		return Task.FromResult<INavigableSymbol?>(null);
	}

	public void Dispose()
	{
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
