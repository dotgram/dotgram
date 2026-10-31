using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

using DotGram.Grammar;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Tags;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

[Export(typeof(IAsyncQuickInfoSourceProvider))]
[Name("DotGram Quick Info")]
[Order(Before = "default")]
[ContentType(GramContentType.Name)]
sealed class GramQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	[Import]
	IClassificationFormatMapService FormatMaps { get; set; } = null!;

	[Import]
	IClassificationTypeRegistryService ClassificationTypes { get; set; } = null!;

	public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
	{
		return new GramQuickInfoSource(
			textBuffer,
			GramBufferAnalysis.For(textBuffer),
			RoslynGramFeatures.For(textBuffer, Workspace, Documents),
			FormatMaps,
			ClassificationTypes);
	}
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

	[Import]
	IClassificationFormatMapService FormatMaps { get; set; } = null!;

	[Import]
	IClassificationTypeRegistryService ClassificationTypes { get; set; } = null!;

	public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
	{
		return new EmbeddedGramQuickInfoSource(
			textBuffer,
			EmbeddedGrammarBufferAnalysis.For(textBuffer, Workspace, Documents),
			RoslynGramFeatures.For(textBuffer, Workspace, Documents),
			FormatMaps,
			ClassificationTypes);
	}
}

sealed class GramQuickInfoSource(
	ITextBuffer buffer,
	GramBufferAnalysis analysis,
	RoslynGramFeatures roslyn,
	IClassificationFormatMapService formatMaps,
	IClassificationTypeRegistryService classificationTypes) : IAsyncQuickInfoSource
{
	public async Task<QuickInfoItem?> GetQuickInfoItemAsync(
		IAsyncQuickInfoSession session,
		CancellationToken cancellationToken)
	{
		var snapshot = buffer.CurrentSnapshot;
		var point    = session.GetTriggerPoint(snapshot);

		if (point is null)
			return null;

		var answer = await GramQuickInfoProvider.StandaloneAsync(
			analysis.View(snapshot), point.Value.Position, roslyn.GetQuickInfoAsync, cancellationToken).ConfigureAwait(false);

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
		return WithDiagnostics(Item(session.TextView, formatMaps, classificationTypes, snapshot, answer.Item), answer.Diagnostics);
	}

	public void Dispose()
	{
	}

	/// <summary>The panel Visual Studio shows for what the shared provider answered.</summary>
	internal static QuickInfoItem? Item(
		ITextView view,
		IClassificationFormatMapService formatMaps,
		IClassificationTypeRegistryService classificationTypes,
		ITextSnapshot snapshot,
		GramQuickInfo? quickInfo)
	{
		if (quickInfo is null)
			return null;

		var span         = new SnapshotSpan(snapshot, quickInfo.Span.Start, quickInfo.Span.Length);
		var trackingSpan = snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive);

		return quickInfo.Kind switch
		{
			GramQuickInfoKind.Grammar    => new QuickInfoItem(trackingSpan, Expandable(view, formatMaps, classificationTypes, quickInfo, trackingSpan)),
			GramQuickInfoKind.CSharp     => CreateCSharp(view, formatMaps, classificationTypes, trackingSpan, quickInfo),
			GramQuickInfoKind.Suppressed => new QuickInfoItem(trackingSpan, new DotGramQuickInfoSuppression(trackingSpan)),
			_                            => new QuickInfoItem(trackingSpan, new DslQuickInfoContent(trackingSpan, quickInfo.Lines)),
		};
	}

	/// <summary>
	/// The squiggles' messages put in front of what DotGram shows, which the tooltip presenter
	/// shows alone (<see cref="GramDiagnosticText"/>). Where DotGram shows nothing, Visual
	/// Studio's own text for the squiggle stands, and nothing is added.
	/// </summary>
	internal static QuickInfoItem? WithDiagnostics(QuickInfoItem? item, IReadOnlyList<string> diagnostics)
	{
		if (item is null || diagnostics.Count == 0)
			return item;

		var foreground = Application.Current.TryFindResource(EnvironmentColors.ToolTipTextBrushKey) as System.Windows.Media.Brush
			?? SystemColors.InfoTextBrush;

		if (item.Item is not Panel panel ||
			item.Item is not IDotGramQuickInfoContent { ShouldDisplay: true })
		{
			var trackingSpan = item.Item is IDotGramQuickInfoContent content
				? content.TrackingSpan
				: item.ApplicableToSpan;
			panel = new InteractiveQuickInfoPanel(trackingSpan);
			item  = new QuickInfoItem(item.ApplicableToSpan, panel);
		}

		for (var index = diagnostics.Count - 1; index >= 0; index--)
			panel.Children.Insert(0, new TextBlock
			{
				Text         = diagnostics[index],
				TextWrapping = TextWrapping.Wrap,
				Foreground   = foreground,
				Margin       = new Thickness(0, 0, 0, 6),
			});

		return item;
	}

	static QuickInfoItem CreateCSharp(
		ITextView view,
		IClassificationFormatMapService formatMaps,
		IClassificationTypeRegistryService classificationTypes,
		ITrackingSpan trackingSpan,
		GramQuickInfo quickInfo)
	{
		var foreground = Application.Current.TryFindResource(EnvironmentColors.ToolTipTextBrushKey) as System.Windows.Media.Brush
			?? SystemColors.InfoTextBrush;
		var formatMap = formatMaps.GetClassificationFormatMap(view);
		var block = new TextBlock { TextWrapping = TextWrapping.Wrap };

		for (var sectionIndex = 0; sectionIndex < quickInfo.Sections.Count; sectionIndex++)
		{
			if (sectionIndex > 0)
				block.Inlines.Add(new LineBreak());

			foreach (var part in quickInfo.Sections[sectionIndex])
			{
				var run = new Run(part.Text) { Foreground = foreground };
				var type = classificationTypes.GetClassificationType(RoslynClassification(part.Tag!));
				if (type is not null)
				{
					var properties = formatMap.GetTextProperties(type);
					if (properties.ForegroundBrush is not null)
						run.Foreground = properties.ForegroundBrush;
					if (properties.Typeface is not null)
					{
						run.FontFamily = properties.Typeface.FontFamily;
						run.FontStyle = properties.Typeface.Style;
						run.FontWeight = properties.Typeface.Weight;
						run.FontStretch = properties.Typeface.Stretch;
					}
				}
				block.Inlines.Add(run);
			}
		}

		var header = new StackPanel { Orientation = Orientation.Horizontal };
		header.Children.Add(new CrispImage
		{
			Moniker = KnownMonikers.IntellisenseLightBulb,
			Width = 16,
			Height = 16,
			Margin = new Thickness(0, 0, 6, 0),
			VerticalAlignment = VerticalAlignment.Top,
		});
		header.Children.Add(block);
		var panel = new InteractiveQuickInfoPanel(trackingSpan);
		panel.Children.Add(header);
		return new QuickInfoItem(trackingSpan, panel);
	}

	static string RoslynClassification(string tag)
	{
		return tag switch
		{
			TextTags.Keyword => PredefinedClassificationTypeNames.Keyword,
			TextTags.Class => "class name",
			TextTags.Struct => "struct name",
			TextTags.Interface => "interface name",
			TextTags.Enum => "enum name",
			TextTags.Delegate => "delegate name",
			TextTags.Method => "method name",
			TextTags.ExtensionMethod => "extension method name",
			TextTags.Property => "property name",
			TextTags.Field => "field name",
			TextTags.Event => "event name",
			TextTags.Namespace => "namespace name",
			TextTags.Parameter => "parameter name",
			TextTags.Local => "local name",
			TextTags.NumericLiteral => PredefinedClassificationTypeNames.Number,
			TextTags.StringLiteral => PredefinedClassificationTypeNames.String,
			TextTags.Operator => PredefinedClassificationTypeNames.Operator,
			TextTags.Punctuation => PredefinedClassificationTypeNames.Punctuation,
			_ => PredefinedClassificationTypeNames.Text,
		};
	}

	static object Expandable(
		ITextView view,
		IClassificationFormatMapService formatMaps,
		IClassificationTypeRegistryService classificationTypes,
		GramQuickInfo quickInfo,
		ITrackingSpan trackingSpan)
	{
		var foreground = Application.Current.TryFindResource(EnvironmentColors.ToolTipTextBrushKey) as System.Windows.Media.Brush
			?? SystemColors.InfoTextBrush;
		var panel = new InteractiveQuickInfoPanel(trackingSpan);
		var header = new StackPanel { Orientation = Orientation.Horizontal };
		header.Children.Add(new CrispImage
		{
			Moniker = quickInfo.Icon switch
			{
				GramQuickInfoIcon.Method        => KnownMonikers.Method,
				GramQuickInfoIcon.LocalVariable => KnownMonikers.LocalVariable,
				_                               => KnownMonikers.IntellisenseKeyword,
			},
			Width = 16,
			Height = 16,
			Margin = new Thickness(0, 0, 6, 0),
			VerticalAlignment = VerticalAlignment.Top,
		});
		header.Children.Add(ClassifiedText(view, formatMaps, classificationTypes, quickInfo.Sections[0], foreground));
		panel.Children.Add(header);

		if (quickInfo.Details.Count == 0)
			return panel;

		var details = ClassifiedDetails(
			view,
			formatMaps,
			classificationTypes,
			quickInfo.Details,
			foreground);
		details.Margin = new Thickness(0, 6, 0, 0);
		details.Visibility = Visibility.Collapsed;
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

		panel.Children.Add(linkText);
		panel.Children.Add(new ScrollViewer
		{
			Content = details,
			MaxHeight = 500,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
		});
		return panel;
	}

	static TextBlock ClassifiedText(
		ITextView view,
		IClassificationFormatMapService formatMaps,
		IClassificationTypeRegistryService classificationTypes,
		IReadOnlyList<GramQuickInfoRun> runs,
		System.Windows.Media.Brush foreground)
	{
		var block = new TextBlock { TextWrapping = TextWrapping.Wrap };
		AppendClassified(block, view, formatMaps, classificationTypes, runs, foreground);
		return block;
	}

	static TextBlock ClassifiedDetails(
		ITextView view,
		IClassificationFormatMapService formatMaps,
		IClassificationTypeRegistryService classificationTypes,
		IReadOnlyList<IReadOnlyList<GramQuickInfoRun>> lines,
		System.Windows.Media.Brush foreground)
	{
		var block = new TextBlock { TextWrapping = TextWrapping.Wrap };
		for (var index = 0; index < lines.Count; index++)
		{
			AppendClassified(block, view, formatMaps, classificationTypes, lines[index], foreground);

			if (index + 1 < lines.Count)
				block.Inlines.Add(new LineBreak());
		}
		return block;
	}

	static void AppendClassified(
		TextBlock block,
		ITextView view,
		IClassificationFormatMapService formatMaps,
		IClassificationTypeRegistryService classificationTypes,
		IReadOnlyList<GramQuickInfoRun> runs,
		System.Windows.Media.Brush foreground)
	{
		var formatMap = formatMaps.GetClassificationFormatMap(view);

		foreach (var item in runs)
			switch (item.Style)
			{
				case GramQuickInfoStyle.Plain:
					block.Inlines.Add(new Run(item.Text) { Foreground = foreground });
					break;
				case GramQuickInfoStyle.Syntax:
					Add(Classification(item.Kind), item.Text);
					break;
				default:
					Add(PredefinedClassificationTypeNames.Text, item.Text);
					break;
			}

		void Add(string classification, string value)
		{
			var run = new Run(value) { Foreground = foreground };
			var type = classificationTypes.GetClassificationType(classification);
			if (type is not null)
			{
				var properties = formatMap.GetTextProperties(type);
				if (properties.ForegroundBrush is not null)
					run.Foreground = properties.ForegroundBrush;
				if (properties.BackgroundBrush is not null)
					run.Background = properties.BackgroundBrush;
				if (properties.Typeface is not null)
				{
					run.FontFamily  = properties.Typeface.FontFamily;
					run.FontStyle   = properties.Typeface.Style;
					run.FontWeight  = properties.Typeface.Weight;
					run.FontStretch = properties.Typeface.Stretch;
				}
				if (properties.FontRenderingEmSize > 0)
					run.FontSize = properties.FontRenderingEmSize;
			}
			block.Inlines.Add(run);
		}
	}

	static string Classification(GramSyntaxKind kind)
	{
		return kind switch
		{
			GramSyntaxKind.Invalid => GramClassificationTypes.Invalid,
			GramSyntaxKind.Comment => GramClassificationTypes.Comment,
			GramSyntaxKind.Keyword => GramClassificationTypes.Keyword,
			GramSyntaxKind.Identifier => GramClassificationTypes.Identifier,
			GramSyntaxKind.Number => GramClassificationTypes.Number,
			GramSyntaxKind.Character => GramClassificationTypes.Literal,
			GramSyntaxKind.String => GramClassificationTypes.Literal,
			GramSyntaxKind.CaseInsensitiveCharacter => GramClassificationTypes.CaseInsensitiveLiteral,
			GramSyntaxKind.CaseInsensitiveString => GramClassificationTypes.CaseInsensitiveLiteral,
			GramSyntaxKind.CharacterClass => GramClassificationTypes.Literal,
			GramSyntaxKind.EmbeddedCode => GramClassificationTypes.EmbeddedCode,
			GramSyntaxKind.Transition => GramClassificationTypes.TransitionStyle,
			GramSyntaxKind.SpecialSymbol => GramClassificationTypes.SpecialSymbol,
			GramSyntaxKind.Operator => GramClassificationTypes.Operator,
			GramSyntaxKind.Punctuation => GramClassificationTypes.Punctuation,
			GramSyntaxKind.Binding => GramClassificationTypes.Identifier,
			_ => PredefinedClassificationTypeNames.Text,
		};
	}

	sealed class InteractiveQuickInfoPanel(ITrackingSpan trackingSpan) : StackPanel, IInteractiveQuickInfoContent, IDotGramQuickInfoContent
	{
		public bool ShouldDisplay => true;
		public ITrackingSpan TrackingSpan { get; } = trackingSpan;
		public bool KeepQuickInfoOpen => false;
		public bool IsMouseOverAggregated => IsMouseOver;
	}

	static bool Contains(int start, int length, int position)
	{
		return start <= position && position < start + length;
	}
}

sealed class EmbeddedGramQuickInfoSource(
	ITextBuffer buffer,
	EmbeddedGrammarBufferAnalysis analysis,
	RoslynGramFeatures roslyn,
	IClassificationFormatMapService formatMaps,
	IClassificationTypeRegistryService classificationTypes) : IAsyncQuickInfoSource
{
	public async Task<QuickInfoItem?> GetQuickInfoItemAsync(
		IAsyncQuickInfoSession session,
		CancellationToken cancellationToken)
	{
		var snapshot = buffer.CurrentSnapshot;
		var point    = session.GetTriggerPoint(snapshot);

		if (point is null ||
			await GramQuickInfoProvider.EmbeddedAsync(
				snapshot.GetText(), analysis.View(snapshot), point.Value.Position, roslyn.GetQuickInfoAsync, cancellationToken)
				.ConfigureAwait(false) is not { } answer)
			return null;

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
		return GramQuickInfoSource.WithDiagnostics(
			GramQuickInfoSource.Item(session.TextView, formatMaps, classificationTypes, snapshot, answer.Item),
			answer.Diagnostics);
	}

	public void Dispose()
	{
	}
}

/// <summary>A DSL literal's or a DSL name's Quick Info: its lines, plain.</summary>
sealed class DslQuickInfoContent : StackPanel, IDotGramQuickInfoContent
{
	public DslQuickInfoContent(ITrackingSpan trackingSpan, IReadOnlyList<string> lines)
	{
		TrackingSpan = trackingSpan;
		var foreground = Application.Current.TryFindResource(EnvironmentColors.ToolTipTextBrushKey) as System.Windows.Media.Brush
			?? SystemColors.InfoTextBrush;

		foreach (var line in lines)
			Children.Add(new TextBlock
			{
				Text = line,
				Foreground = foreground,
			});
	}

	public bool ShouldDisplay => true;
	public ITrackingSpan TrackingSpan { get; }
}
