using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

[Export(typeof(IViewTaggerProvider))]
[ContentType(GramContentType.Name)]
[TagType(typeof(TextMarkerTag))]
sealed class GramReferenceHighlightTaggerProvider : IViewTaggerProvider
{
	public ITagger<T>? CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag =>
		buffer == textView.TextBuffer
			? new GramReferenceHighlightTagger(textView, GramBufferAnalysis.For(buffer)) as ITagger<T>
			: null;
}

[Export(typeof(IViewTaggerProvider))]
[ContentType("CSharp")]
[TagType(typeof(TextMarkerTag))]
sealed class EmbeddedGramReferenceHighlightTaggerProvider : IViewTaggerProvider
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	public ITagger<T>? CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag =>
		buffer == textView.TextBuffer
			? new EmbeddedGramReferenceHighlightTagger(
				textView,
				EmbeddedGrammarBufferAnalysis.For(buffer, Workspace, Documents)) as ITagger<T>
			: null;
}

sealed class GramReferenceHighlightTagger : ITagger<TextMarkerTag>, IDisposable
{
	const string DefinitionMarker = "DefinitionHighlightTag";
	const string ReferenceMarker  = "ReferenceHighlightTag";

	readonly ITextView         _view;
	readonly GramBufferAnalysis _analysis;

	public GramReferenceHighlightTagger(ITextView view, GramBufferAnalysis analysis)
	{
		_view     = view;
		_analysis = analysis;

		_view.Caret.PositionChanged += CaretPositionChanged;
		_view.Closed                += ViewClosed;
		_analysis.Changed           += AnalysisChanged;
	}

	public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

	public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
	{
		if (spans.Count == 0)
			yield break;

		var snapshot = spans[0].Snapshot;
		var position = _view.Caret.Position.BufferPosition.TranslateTo(snapshot, PointTrackingMode.Negative).Position;
		var document = _analysis.Document(snapshot);
		var current = document.Symbols.FirstOrDefault(symbol =>
			symbol.Position <= position && position < symbol.Position + symbol.Length);

		if (current.Length == 0)
			yield break;

		foreach (var symbol in document.Symbols)
			if (symbol.Name == current.Name)
			{
				var span = new SnapshotSpan(snapshot, symbol.Position, symbol.Length);

				if (spans.IntersectsWith(span))
					yield return new TagSpan<TextMarkerTag>(
						span,
						new TextMarkerTag(symbol.IsDefinition ? DefinitionMarker : ReferenceMarker));
			}
	}

	void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e) => RaiseChanged(e.NewPosition.BufferPosition.Snapshot);

	void AnalysisChanged(ITextSnapshot snapshot) => RaiseChanged(snapshot);

	void RaiseChanged(ITextSnapshot snapshot) =>
		TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));

	void ViewClosed(object sender, EventArgs e) => Dispose();

	public void Dispose()
	{
		_view.Caret.PositionChanged -= CaretPositionChanged;
		_view.Closed                -= ViewClosed;
		_analysis.Changed           -= AnalysisChanged;
	}
}

sealed class EmbeddedGramReferenceHighlightTagger : ITagger<TextMarkerTag>, IDisposable
{
	const string DefinitionMarker = "DefinitionHighlightTag";
	const string ReferenceMarker  = "ReferenceHighlightTag";

	readonly ITextView                    _view;
	readonly EmbeddedGrammarBufferAnalysis _analysis;

	public EmbeddedGramReferenceHighlightTagger(
		ITextView view,
		EmbeddedGrammarBufferAnalysis analysis)
	{
		_view     = view;
		_analysis = analysis;

		_view.Caret.PositionChanged += CaretPositionChanged;
		_view.Closed                += ViewClosed;
		_analysis.Changed           += AnalysisChanged;
	}

	public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

	public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
	{
		if (spans.Count == 0)
			yield break;

		var snapshot = spans[0].Snapshot;
		var position = _view.Caret.Position.BufferPosition.TranslateTo(snapshot, PointTrackingMode.Negative).Position;

		if (!_analysis.TryGetSymbols(snapshot, out var symbols))
			yield break;

		var current = symbols.FirstOrDefault(symbol => symbol.Span.Contains(position));

		if (current.Span.Length == 0)
			yield break;

		foreach (var symbol in symbols)
			if (symbol.Name == current.Name &&
				symbol.GrammarSpan == current.GrammarSpan &&
				symbol.DefinitionSpan == current.DefinitionSpan)
			{
				var span = new SnapshotSpan(snapshot, symbol.Span.Start, symbol.Span.Length);

				if (spans.IntersectsWith(span))
					yield return new TagSpan<TextMarkerTag>(
						span,
						new TextMarkerTag(symbol.IsDefinition ? DefinitionMarker : ReferenceMarker));
			}
	}

	void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e) => RaiseChanged(e.NewPosition.BufferPosition.Snapshot);

	void AnalysisChanged(ITextSnapshot snapshot) => RaiseChanged(snapshot);

	void RaiseChanged(ITextSnapshot snapshot) =>
		TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));

	void ViewClosed(object sender, EventArgs e) => Dispose();

	public void Dispose()
	{
		_view.Caret.PositionChanged -= CaretPositionChanged;
		_view.Closed                -= ViewClosed;
		_analysis.Changed           -= AnalysisChanged;
	}
}
