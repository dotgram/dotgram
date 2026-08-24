using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

[Export(typeof(IViewTaggerProvider))]
[ContentType(GramContentType.Name)]
[TagType(typeof(TextMarkerTag))]
sealed class GramBraceMatchingTaggerProvider : IViewTaggerProvider
{
	public ITagger<T>? CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag =>
		buffer == textView.TextBuffer
			? new GramBraceMatchingTagger(textView, GramBufferAnalysis.For(buffer)) as ITagger<T>
			: null;
}

[Export(typeof(IViewTaggerProvider))]
[ContentType("CSharp")]
[TagType(typeof(TextMarkerTag))]
sealed class EmbeddedGramBraceMatchingTaggerProvider : IViewTaggerProvider
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	public ITagger<T>? CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag =>
		buffer == textView.TextBuffer
			? new EmbeddedGramBraceMatchingTagger(
				textView,
				EmbeddedGrammarBufferAnalysis.For(buffer, Workspace, Documents)) as ITagger<T>
			: null;
}

sealed class GramBraceMatchingTagger : ITagger<TextMarkerTag>, IDisposable
{
	static readonly TextMarkerTag Brace = new("bracehighlight");
	readonly ITextView _view;
	readonly GramBufferAnalysis _analysis;

	public GramBraceMatchingTagger(ITextView view, GramBufferAnalysis analysis)
	{
		_view = view;
		_analysis = analysis;
		_view.Caret.PositionChanged += CaretChanged;
		_view.Closed += ViewClosed;
		_analysis.Changed += AnalysisChanged;
	}

	public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

	public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
	{
		if (spans.Count == 0)
			yield break;

		var snapshot = spans[0].Snapshot;
		var caret = _view.Caret.Position.BufferPosition.TranslateTo(snapshot, PointTrackingMode.Negative).Position;

		foreach (var pair in _analysis.Document(snapshot).Braces)
		{
			var open = new SnapshotSpan(snapshot, pair.OpenPosition, pair.OpenLength);
			var close = new SnapshotSpan(snapshot, pair.ClosePosition, pair.CloseLength);
			if (!Adjacent(open, caret) && !Adjacent(close, caret))
				continue;

			if (spans.IntersectsWith(open)) yield return new TagSpan<TextMarkerTag>(open, Brace);
			if (spans.IntersectsWith(close)) yield return new TagSpan<TextMarkerTag>(close, Brace);
		}
	}

	static bool Adjacent(SnapshotSpan span, int position) => span.Contains(position) || span.End.Position == position;

	void CaretChanged(object sender, CaretPositionChangedEventArgs args) => RaiseChanged(args.NewPosition.BufferPosition.Snapshot);
	void AnalysisChanged(ITextSnapshot snapshot) => RaiseChanged(snapshot);
	void RaiseChanged(ITextSnapshot snapshot) =>
		TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));
	void ViewClosed(object sender, EventArgs args) => Dispose();

	public void Dispose()
	{
		_view.Caret.PositionChanged -= CaretChanged;
		_view.Closed -= ViewClosed;
		_analysis.Changed -= AnalysisChanged;
	}
}

sealed class EmbeddedGramBraceMatchingTagger : ITagger<TextMarkerTag>, IDisposable
{
	static readonly TextMarkerTag Brace = new("bracehighlight");
	readonly ITextView _view;
	readonly EmbeddedGrammarBufferAnalysis _analysis;

	public EmbeddedGramBraceMatchingTagger(ITextView view, EmbeddedGrammarBufferAnalysis analysis)
	{
		_view = view;
		_analysis = analysis;
		_view.Caret.PositionChanged += CaretChanged;
		_view.Closed += ViewClosed;
		_analysis.Changed += AnalysisChanged;
	}

	public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

	public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
	{
		if (spans.Count == 0)
			yield break;

		var snapshot = spans[0].Snapshot;
		var caret = _view.Caret.Position.BufferPosition.TranslateTo(snapshot, PointTrackingMode.Negative).Position;
		if (!_analysis.TryGetStructure(snapshot, out var braces, out _))
			yield break;

		foreach (var pair in braces)
		{
			var open = new SnapshotSpan(snapshot, pair.OpenSpan.Start, pair.OpenSpan.Length);
			var close = new SnapshotSpan(snapshot, pair.CloseSpan.Start, pair.CloseSpan.Length);
			if (!Adjacent(open, caret) && !Adjacent(close, caret))
				continue;

			if (spans.IntersectsWith(open)) yield return new TagSpan<TextMarkerTag>(open, Brace);
			if (spans.IntersectsWith(close)) yield return new TagSpan<TextMarkerTag>(close, Brace);
		}
	}

	static bool Adjacent(SnapshotSpan span, int position) => span.Contains(position) || span.End.Position == position;

	void CaretChanged(object sender, CaretPositionChangedEventArgs args) => RaiseChanged(args.NewPosition.BufferPosition.Snapshot);
	void AnalysisChanged(ITextSnapshot snapshot) => RaiseChanged(snapshot);
	void RaiseChanged(ITextSnapshot snapshot) =>
		TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));
	void ViewClosed(object sender, EventArgs args) => Dispose();

	public void Dispose()
	{
		_view.Caret.PositionChanged -= CaretChanged;
		_view.Closed -= ViewClosed;
		_analysis.Changed -= AnalysisChanged;
	}
}

[Export(typeof(ITaggerProvider))]
[ContentType(GramContentType.Name)]
[TagType(typeof(IOutliningRegionTag))]
sealed class GramOutliningTaggerProvider : ITaggerProvider
{
	public ITagger<T>? CreateTagger<T>(ITextBuffer buffer) where T : ITag =>
		new GramOutliningTagger(GramBufferAnalysis.For(buffer)) as ITagger<T>;
}

[Export(typeof(ITaggerProvider))]
[ContentType("CSharp")]
[TagType(typeof(IOutliningRegionTag))]
sealed class EmbeddedGramOutliningTaggerProvider : ITaggerProvider
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	public ITagger<T>? CreateTagger<T>(ITextBuffer buffer) where T : ITag =>
		new EmbeddedGramOutliningTagger(
			EmbeddedGrammarBufferAnalysis.For(buffer, Workspace, Documents)) as ITagger<T>;
}

sealed class GramOutliningTagger : ITagger<IOutliningRegionTag>, IDisposable
{
	readonly GramBufferAnalysis _analysis;

	public GramOutliningTagger(GramBufferAnalysis analysis)
	{
		_analysis = analysis;
		_analysis.Changed += AnalysisChanged;
	}

	public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

	void AnalysisChanged(ITextSnapshot snapshot) =>
		TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));

	public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
	{
		if (spans.Count == 0)
			yield break;

		var snapshot = spans[0].Snapshot;
		foreach (var range in _analysis.Document(snapshot).FoldingRanges)
		{
			var span = new SnapshotSpan(snapshot, range.Position, range.Length);
			if (spans.IntersectsWith(span))
				yield return new TagSpan<IOutliningRegionTag>(
					span,
					new OutliningRegionTag(false, false, range.CollapsedText, range.CollapsedText));
		}
	}

	public void Dispose() => _analysis.Changed -= AnalysisChanged;
}

sealed class EmbeddedGramOutliningTagger : ITagger<IOutliningRegionTag>, IDisposable
{
	readonly EmbeddedGrammarBufferAnalysis _analysis;

	public EmbeddedGramOutliningTagger(EmbeddedGrammarBufferAnalysis analysis)
	{
		_analysis = analysis;
		_analysis.Changed += AnalysisChanged;
	}

	public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

	void AnalysisChanged(ITextSnapshot snapshot) =>
		TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));

	public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
	{
		if (spans.Count == 0)
			yield break;

		var snapshot = spans[0].Snapshot;
		if (!_analysis.TryGetStructure(snapshot, out _, out var foldingRanges))
			yield break;

		foreach (var range in foldingRanges)
		{
			var span = new SnapshotSpan(snapshot, range.Span.Start, range.Span.Length);
			if (spans.IntersectsWith(span))
				yield return new TagSpan<IOutliningRegionTag>(
					span,
					new OutliningRegionTag(false, false, range.CollapsedText, range.CollapsedText));
		}
	}

	public void Dispose() => _analysis.Changed -= AnalysisChanged;
}
