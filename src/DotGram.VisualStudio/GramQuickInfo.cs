using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

using DotGram.Language;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

[Export(typeof(IAsyncQuickInfoSourceProvider))]
[Name("DotGram Quick Info")]
[Order(Before = "default")]
[ContentType(GramContentType.Name)]
sealed class GramQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
{
	public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer) =>
		new GramQuickInfoSource(textBuffer, GramBufferAnalysis.For(textBuffer));
}

[Export(typeof(IAsyncQuickInfoSourceProvider))]
[Name("DotGram embedded Quick Info")]
[Order(Before = "default")]
[ContentType("CSharp")]
sealed class EmbeddedGramQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer) =>
		new EmbeddedGramQuickInfoSource(
			textBuffer,
			EmbeddedGrammarBufferAnalysis.For(textBuffer, Workspace, Documents));
}

sealed class GramQuickInfoSource(ITextBuffer buffer, GramBufferAnalysis analysis) : IAsyncQuickInfoSource
{
	public async Task<QuickInfoItem?> GetQuickInfoItemAsync(
		IAsyncQuickInfoSession session,
		CancellationToken cancellationToken)
	{
		var snapshot = buffer.CurrentSnapshot;
		var point    = session.GetTriggerPoint(snapshot);

		if (point is null)
			return null;

		foreach (var item in analysis.Document(snapshot).Classifications)
			if (Contains(item.Position, item.Length, point.Value.Position))
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
				return Create(
					snapshot,
					item.Position,
					item.Length,
					item.Kind,
					item.QuickInfo);
			}

		return null;
	}

	public void Dispose()
	{
	}

	internal static QuickInfoItem Create(
		ITextSnapshot snapshot,
		int position,
		int length,
		GramSyntaxKind kind,
		string? quickInfo)
	{
		var span = new SnapshotSpan(snapshot, position, length);
		var text = span.GetText();

		return new QuickInfoItem(
			snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive),
			Expandable(quickInfo) ?? Describe(kind, text));
	}

	static object? Expandable(string? quickInfo)
	{
		if (quickInfo is null)
			return null;

		var referenced = quickInfo.IndexOf("\n\nReferenced rule:", StringComparison.Ordinal);
		var recursive  = quickInfo.IndexOf("\n\nRecursive reference:", StringComparison.Ordinal);
		var split = referenced < 0
			? recursive
			: recursive < 0 ? referenced : Math.Min(referenced, recursive);

		if (split < 0)
			return quickInfo;

		var foreground = Application.Current.TryFindResource(EnvironmentColors.ToolTipTextBrushKey) as System.Windows.Media.Brush
			?? SystemColors.InfoTextBrush;
		var details = new TextBlock
		{
			Text = quickInfo.Substring(split).TrimStart(),
			Margin = new Thickness(0, 6, 0, 0),
			Visibility = Visibility.Collapsed,
			Foreground = foreground,
		};
		var link = new Hyperlink(new Run("Show referenced rules"))
		{
			Cursor = Cursors.Hand,
		};
		var linkText = new TextBlock
		{
			Margin = new Thickness(0, 6, 0, 0),
			Foreground = foreground,
		};
		linkText.Inlines.Add(link);
		link.Click += (_, _) =>
		{
			var expanded = details.Visibility == Visibility.Visible;
			details.Visibility = expanded ? Visibility.Collapsed : Visibility.Visible;
			link.Inlines.Clear();
			link.Inlines.Add(new Run(expanded ? "Show referenced rules" : "Hide referenced rules"));
		};

		var panel = new InteractiveQuickInfoPanel();
		panel.Children.Add(new TextBlock
		{
			Text = quickInfo.Substring(0, split),
			Foreground = foreground,
		});
		panel.Children.Add(linkText);
		panel.Children.Add(new ScrollViewer
		{
			Content = details,
			MaxHeight = 500,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
		});
		return panel;
	}

	sealed class InteractiveQuickInfoPanel : StackPanel, IInteractiveQuickInfoContent
	{
		public bool KeepQuickInfoOpen => false;
		public bool IsMouseOverAggregated => IsMouseOver;
	}

	static string Describe(GramSyntaxKind kind, string text) => kind switch
	{
		GramSyntaxKind.Keyword        => $"DotGram keyword: {text}",
		GramSyntaxKind.Identifier     => $"DotGram rule or binding: {text}",
		GramSyntaxKind.Number         => "DotGram numeric literal",
		GramSyntaxKind.Character      => "DotGram character literal",
		GramSyntaxKind.String         => "DotGram string literal",
		GramSyntaxKind.CharacterClass => "DotGram character class",
		GramSyntaxKind.EmbeddedCode   => "Embedded C# expression",
		GramSyntaxKind.Transition     => "Switch from DotGram grammar to C#",
		GramSyntaxKind.SpecialSymbol  => SpecialSymbol(text),
		GramSyntaxKind.Comment        => "DotGram comment",
		GramSyntaxKind.Invalid        => "Unrecognized DotGram syntax",
		GramSyntaxKind.Operator       => $"DotGram operator: {text}",
		_                             => $"DotGram syntax: {text}",
	};

	static string SpecialSymbol(string text) => text switch
	{
		"*"  => "DotGram repetition: zero or more",
		"+"  => "DotGram repetition: one or more",
		"?"  => "DotGram optional expression",
		"?!" => "DotGram negative lookahead",
		"?=" => "DotGram positive lookahead",
		"|"  => "DotGram alternative",
		"&"  => "DotGram sequence",
		"^"  => "DotGram recovery marker",
		".." => "DotGram range",
		_     => $"DotGram special symbol: {text}",
	};

	static bool Contains(int start, int length, int position) =>
		start <= position && position < start + length;
}

sealed class EmbeddedGramQuickInfoSource(
	ITextBuffer buffer,
	EmbeddedGrammarBufferAnalysis analysis) : IAsyncQuickInfoSource
{
	public async Task<QuickInfoItem?> GetQuickInfoItemAsync(
		IAsyncQuickInfoSession session,
		CancellationToken cancellationToken)
	{
		var snapshot = buffer.CurrentSnapshot;
		var point    = session.GetTriggerPoint(snapshot);

		if (point is null || !analysis.TryGet(snapshot, out var classifications, out _))
			return null;

		foreach (var item in classifications)
			if (item.Span.Contains(point.Value.Position))
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
				return GramQuickInfoSource.Create(
					snapshot,
					item.Span.Start,
					item.Span.Length,
					item.Kind,
					item.QuickInfo);
			}

		return null;
	}

	public void Dispose()
	{
	}
}
