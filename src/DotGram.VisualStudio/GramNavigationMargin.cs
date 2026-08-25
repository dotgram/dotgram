using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

[Export(typeof(IWpfTextViewMarginProvider))]
[Name(MarginName)]
[MarginContainer(PredefinedMarginNames.Top)]
[ContentType(GramContentType.Name)]
[TextViewRole(PredefinedTextViewRoles.Document)]
sealed class GramNavigationMarginProvider : IWpfTextViewMarginProvider
{
	internal const string MarginName = "DotGram Navigation";

	public IWpfTextViewMargin CreateMargin(IWpfTextViewHost host, IWpfTextViewMargin containerMargin) =>
		new StandaloneGramNavigationMargin(host.TextView, GramBufferAnalysis.For(host.TextView.TextBuffer));
}

[Export(typeof(IWpfTextViewMarginProvider))]
[Name(MarginName)]
[MarginContainer(PredefinedMarginNames.Top)]
[ContentType("CSharp")]
[TextViewRole(PredefinedTextViewRoles.Document)]
sealed class EmbeddedGramNavigationMarginProvider : IWpfTextViewMarginProvider
{
	internal const string MarginName = "DotGram Embedded Navigation";

	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	public IWpfTextViewMargin CreateMargin(IWpfTextViewHost host, IWpfTextViewMargin containerMargin) =>
		new EmbeddedGramNavigationMargin(
			host.TextView,
			EmbeddedGrammarBufferAnalysis.For(host.TextView.TextBuffer, Workspace, Documents));
}

abstract class GramNavigationMargin : Grid, IWpfTextViewMargin
{
	readonly IWpfTextView _view;
	readonly ComboBox _symbols;
	bool _updating;

	protected GramNavigationMargin(IWpfTextView view)
	{
		_view = view;
		Height = 26;
		Background = Application.Current.TryFindResource(EnvironmentColors.ToolWindowBackgroundBrushKey) as System.Windows.Media.Brush;

		_symbols = new ComboBox
		{
			Margin = new Thickness(4, 2, 4, 2),
			MinWidth = 220,
			HorizontalAlignment = HorizontalAlignment.Left,
			DisplayMemberPath = nameof(NavigationItem.Display),
		};
		_symbols.SelectionChanged += SelectionChanged;
		Children.Add(_symbols);

		_view.Caret.PositionChanged += CaretChanged;
		_view.Closed += ViewClosed;
	}

	protected abstract IReadOnlyList<NavigationItem> Items(ITextSnapshot snapshot);
	protected abstract void Unsubscribe();

	protected void Refresh(ITextSnapshot snapshot)
	{
		if (snapshot != _view.TextSnapshot)
			return;

		_updating = true;
		var items = Items(snapshot);
		_symbols.ItemsSource = items;
		Visibility = items.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
		SelectAtCaret(items, snapshot);
		_updating = false;
	}

	protected void RequestRefresh(ITextSnapshot snapshot)
	{
		if (Dispatcher.CheckAccess())
			Refresh(snapshot);
		else
			ThreadHelper.JoinableTaskFactory.Run(async () =>
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				Refresh(snapshot);
			});
	}

	void CaretChanged(object sender, CaretPositionChangedEventArgs args)
	{
		var items = _symbols.ItemsSource as IReadOnlyList<NavigationItem>;
		if (items is null)
			return;

		_updating = true;
		SelectAtCaret(items, args.NewPosition.BufferPosition.Snapshot);
		_updating = false;
	}

	void SelectAtCaret(IReadOnlyList<NavigationItem> items, ITextSnapshot snapshot)
	{
		var position = _view.Caret.Position.BufferPosition
			.TranslateTo(snapshot, PointTrackingMode.Negative).Position;
		_symbols.SelectedItem = items
			.Where(item => item.Position <= position && position < item.Position + item.Length)
			.OrderBy(item => item.Length)
			.FirstOrDefault();
	}

	void SelectionChanged(object sender, SelectionChangedEventArgs args)
	{
		if (_updating || _symbols.SelectedItem is not NavigationItem item)
			return;

		var snapshot = _view.TextSnapshot;
		if (item.SelectionPosition < 0 || item.SelectionPosition > snapshot.Length)
			return;

		var point = new SnapshotPoint(snapshot, item.SelectionPosition);
		_view.Caret.MoveTo(point);
		_view.ViewScroller.EnsureSpanVisible(new SnapshotSpan(point, Math.Min(item.SelectionLength, snapshot.Length - point.Position)));
		_view.VisualElement.Focus();
	}

	void ViewClosed(object sender, EventArgs args) => Dispose();

	public FrameworkElement VisualElement => this;
	public double MarginSize => ActualHeight;
	public bool Enabled => Visibility == Visibility.Visible;
	public ITextViewMargin? GetTextViewMargin(string marginName) =>
		string.Equals(marginName, MarginName, StringComparison.OrdinalIgnoreCase) ? this : null;
	protected abstract string MarginName { get; }

	public void Dispose()
	{
		_symbols.SelectionChanged -= SelectionChanged;
		_view.Caret.PositionChanged -= CaretChanged;
		_view.Closed -= ViewClosed;
		Unsubscribe();
	}

	protected sealed record NavigationItem(
		string Display,
		int Position,
		int Length,
		int SelectionPosition,
		int SelectionLength);
}

sealed class StandaloneGramNavigationMargin : GramNavigationMargin
{
	readonly IWpfTextView _view;
	readonly GramBufferAnalysis _analysis;

	public StandaloneGramNavigationMargin(IWpfTextView view, GramBufferAnalysis analysis) : base(view)
	{
		_view = view;
		_analysis = analysis;
		_analysis.Changed += AnalysisChanged;
		Refresh(view.TextSnapshot);
	}

	protected override string MarginName => GramNavigationMarginProvider.MarginName;

	protected override IReadOnlyList<NavigationItem> Items(ITextSnapshot snapshot)
	{
		var result = new List<NavigationItem>();
		Append(_analysis.Document(snapshot).DocumentSymbols, 0, result);
		return result;
	}

	static void Append(IReadOnlyList<DotGram.Language.GramDocumentSymbol> symbols, int depth, List<NavigationItem> result)
	{
		foreach (var symbol in symbols)
		{
			result.Add(new NavigationItem(
				new string(' ', depth * 2) + symbol.Name,
				symbol.Position,
				symbol.Length,
				symbol.SelectionPosition,
				symbol.SelectionLength));
			Append(symbol.Children, depth + 1, result);
		}
	}

	void AnalysisChanged(ITextSnapshot snapshot) => RequestRefresh(snapshot);
	protected override void Unsubscribe() => _analysis.Changed -= AnalysisChanged;
}

sealed class EmbeddedGramNavigationMargin : GramNavigationMargin
{
	readonly IWpfTextView _view;
	readonly EmbeddedGrammarBufferAnalysis _analysis;

	public EmbeddedGramNavigationMargin(IWpfTextView view, EmbeddedGrammarBufferAnalysis analysis) : base(view)
	{
		_view = view;
		_analysis = analysis;
		_analysis.Changed += AnalysisChanged;
		Refresh(view.TextSnapshot);
	}

	protected override string MarginName => EmbeddedGramNavigationMarginProvider.MarginName;

	protected override IReadOnlyList<NavigationItem> Items(ITextSnapshot snapshot)
	{
		var result = new List<NavigationItem>();
		if (_analysis.TryGetDocumentSymbols(snapshot, out var symbols))
			Append(symbols, 0, result);
		return result;
	}

	static void Append(IReadOnlyList<HostDocumentSymbol> symbols, int depth, List<NavigationItem> result)
	{
		foreach (var symbol in symbols)
		{
			result.Add(new NavigationItem(
				new string(' ', depth * 2) + symbol.Name,
				symbol.Span.Start,
				symbol.Span.Length,
				symbol.SelectionSpan.Start,
				symbol.SelectionSpan.Length));
			Append(symbol.Children, depth + 1, result);
		}
	}

	void AnalysisChanged(ITextSnapshot snapshot) => RequestRefresh(snapshot);
	protected override void Unsubscribe() => _analysis.Changed -= AnalysisChanged;
}
