using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
[ContentType("CSharp")]
[TextViewRole(PredefinedTextViewRoles.Document)]
sealed class EmbeddedGramNavigationMarginProvider : IWpfTextViewMarginProvider
{
	internal const string MarginName = "DotGram Embedded Navigation";

	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	public IWpfTextViewMargin CreateMargin(IWpfTextViewHost host, IWpfTextViewMargin containerMargin)
	{
		return EditorGuard.Run<IWpfTextViewMargin>(
			"Embedded navigation margin",
			null!,
			() => new EmbeddedGramNavigationMargin(
				host.TextView,
				EmbeddedGrammarBufferAnalysis.For(host.TextView.TextBuffer, Workspace, Documents)));
	}
}

/// <summary>
/// The bar above a C# document with grammars, laid out as C#'s own: the grammars on the left, the
/// rules of the one chosen there on the right, half the width each, both following the caret.
/// </summary>
sealed class EmbeddedGramNavigationMargin : Grid, IWpfTextViewMargin
{
	readonly IWpfTextView                  _view;
	readonly EmbeddedGrammarBufferAnalysis _analysis;
	readonly ComboBox                      _grammars;
	readonly ComboBox                      _symbols;
	bool _updating;

	public EmbeddedGramNavigationMargin(IWpfTextView view, EmbeddedGrammarBufferAnalysis analysis)
	{
		_view     = view;
		_analysis = analysis;

		Height     = 26;
		Background = Application.Current.TryFindResource(EnvironmentColors.ToolWindowBackgroundBrushKey) as System.Windows.Media.Brush;
		ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

		_grammars = Combo(0, new Thickness(4, 2, 2, 2));
		_symbols  = Combo(1, new Thickness(2, 2, 4, 2));
		_grammars.SelectionChanged += GrammarSelectionChanged;
		_symbols.SelectionChanged  += SymbolSelectionChanged;

		_view.Caret.PositionChanged += CaretChanged;
		_view.Closed                += ViewClosed;
		_analysis.Changed           += AnalysisChanged;

		Refresh(view.TextSnapshot);
	}

	ComboBox Combo(int column, Thickness margin)
	{
		var combo = new ComboBox
		{
			Margin              = margin,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			DisplayMemberPath   = nameof(EmbeddedNavigationItem.Display),
		};

		SetColumn(combo, column);
		Children.Add(combo);

		return combo;
	}

	void Refresh(ITextSnapshot snapshot)
	{
		if (snapshot != _view.TextSnapshot)
			return;

		// Left set by a failure, it would ignore every selection the user makes from then on.
		_updating = true;
		try
		{
			IReadOnlyList<EmbeddedGrammarItem> grammars = _analysis.TryGetOutlines(snapshot, out var outlines)
				? EmbeddedNavigationItems.Grammars(
					outlines,
					position => snapshot.GetLineNumberFromPosition(Math.Min(position, snapshot.Length)) + 1)
				: [];

			_grammars.ItemsSource = grammars;
			Visibility = grammars.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
			SelectAtCaret(snapshot);
		}
		finally
		{
			_updating = false;
		}
	}

	void AnalysisChanged(ITextSnapshot snapshot)
	{
		EditorGuard.Run("Navigation margin refresh", () =>
		{
			if (Dispatcher.CheckAccess())
				Refresh(snapshot);
			else
				ThreadHelper.JoinableTaskFactory.Run(async () =>
				{
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
					Refresh(snapshot);
				});
		});
	}

	void CaretChanged(object sender, CaretPositionChangedEventArgs args)
	{
		EditorGuard.Run("Navigation margin caret", () =>
		{
			if (_grammars.ItemsSource is null)
				return;

			_updating = true;
			try
			{
				SelectAtCaret(args.NewPosition.BufferPosition.Snapshot);
			}
			finally
			{
				_updating = false;
			}
		});
	}

	void SelectAtCaret(ITextSnapshot snapshot)
	{
		if (_grammars.ItemsSource is not IReadOnlyList<EmbeddedGrammarItem> grammars)
			return;

		var position = _view.Caret.Position.BufferPosition
			.TranslateTo(snapshot, PointTrackingMode.Negative).Position;
		var index    = EmbeddedNavigationItems.GrammarAt(grammars, position);
		var grammar  = index < 0 ? null : grammars[index];
		var symbols  = grammar?.Symbols ?? [];

		_grammars.SelectedItem = grammar;
		if (!ReferenceEquals(_symbols.ItemsSource, symbols))
			_symbols.ItemsSource = symbols;

		var symbol = EmbeddedNavigationItems.SymbolAt(symbols, position);
		_symbols.SelectedItem = symbol < 0 ? null : symbols[symbol];
	}

	void GrammarSelectionChanged(object sender, SelectionChangedEventArgs args)
	{
		EditorGuard.Run("Navigation margin grammar selection", () =>
		{
			// Like C#'s type list: the caret goes to where the grammar starts, and the right-hand
			// list follows it there.
			if (!_updating && _grammars.SelectedItem is EmbeddedGrammarItem grammar)
				MoveTo(grammar.Span.Start, 0);
		});
	}

	void SymbolSelectionChanged(object sender, SelectionChangedEventArgs args)
	{
		EditorGuard.Run("Navigation margin selection", () =>
		{
			if (!_updating && _symbols.SelectedItem is EmbeddedNavigationItem item)
				MoveTo(item.SelectionPosition, item.SelectionLength);
		});
	}

	void MoveTo(int position, int length)
	{
		var snapshot = _view.TextSnapshot;
		if (position < 0 || position > snapshot.Length)
			return;

		var point = new SnapshotPoint(snapshot, position);
		_view.Caret.MoveTo(point);
		_view.ViewScroller.EnsureSpanVisible(new SnapshotSpan(point, Math.Min(length, snapshot.Length - point.Position)));
		_view.VisualElement.Focus();
	}

	void ViewClosed(object sender, EventArgs args)
	{
		Dispose();
	}

	public FrameworkElement VisualElement => this;
	public double MarginSize => ActualHeight;
	public bool Enabled => Visibility == Visibility.Visible;
	public ITextViewMargin? GetTextViewMargin(string marginName)
	{
		return string.Equals(marginName, EmbeddedGramNavigationMarginProvider.MarginName, StringComparison.OrdinalIgnoreCase) ? this : null;
	}

	public void Dispose()
	{
		_grammars.SelectionChanged  -= GrammarSelectionChanged;
		_symbols.SelectionChanged   -= SymbolSelectionChanged;
		_view.Caret.PositionChanged -= CaretChanged;
		_view.Closed                -= ViewClosed;
		_analysis.Changed           -= AnalysisChanged;
	}
}
