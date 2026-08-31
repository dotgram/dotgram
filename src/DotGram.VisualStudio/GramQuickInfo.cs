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

	public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer) =>
		new GramQuickInfoSource(
			textBuffer,
			GramBufferAnalysis.For(textBuffer),
			new RoslynGramCompletion(textBuffer, Workspace, Documents),
			FormatMaps,
			ClassificationTypes);
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

	public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer) =>
		new EmbeddedGramQuickInfoSource(
			textBuffer,
			EmbeddedGrammarBufferAnalysis.For(textBuffer, Workspace, Documents),
			new RoslynGramCompletion(textBuffer, Workspace, Documents),
			FormatMaps,
			ClassificationTypes);
}

sealed class GramQuickInfoSource(
	ITextBuffer buffer,
	GramBufferAnalysis analysis,
	RoslynGramCompletion roslyn,
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

		if (GramCSharpCompletionContext.TryGetExpression(
			snapshot.GetText(), point.Value.Position,
			out var expression, out var expressionStart, out var symbolStart, out var symbolLength))
		{
			var csharp = await roslyn.GetQuickInfoAsync(
				expression, point.Value.Position - expressionStart, cancellationToken).ConfigureAwait(false);
			if (csharp is not null)
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
				return CreateCSharp(
					session.TextView, formatMaps, classificationTypes, snapshot,
					symbolStart, symbolLength, csharp);
			}
		}

		foreach (var item in analysis.Document(snapshot).Classifications)
			if (Contains(item.Position, item.Length, point.Value.Position))
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
				return Create(
					session.TextView,
					formatMaps,
					classificationTypes,
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
		ITextView view,
		IClassificationFormatMapService formatMaps,
		IClassificationTypeRegistryService classificationTypes,
		ITextSnapshot snapshot,
		int position,
		int length,
		GramSyntaxKind kind,
		string? quickInfo)
	{
		var span = new SnapshotSpan(snapshot, position, length);
		var text = span.GetText();
		var trackingSpan = snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive);

		return new QuickInfoItem(
			trackingSpan,
			Expandable(
				view,
				formatMaps,
				classificationTypes,
				quickInfo ?? Describe(kind, text),
				trackingSpan,
				kind,
				quickInfo is not null));
	}

	internal static QuickInfoItem CreateCSharp(
		ITextView view,
		IClassificationFormatMapService formatMaps,
		IClassificationTypeRegistryService classificationTypes,
		ITextSnapshot snapshot,
		int position,
		int length,
		RoslynGramQuickInfo quickInfo)
	{
		var span = new SnapshotSpan(snapshot, position, length);
		var trackingSpan = snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive);
		var foreground = Application.Current.TryFindResource(EnvironmentColors.ToolTipTextBrushKey) as System.Windows.Media.Brush
			?? SystemColors.InfoTextBrush;
		var formatMap = formatMaps.GetClassificationFormatMap(view);
		var block = new TextBlock { TextWrapping = TextWrapping.Wrap };

		for (var sectionIndex = 0; sectionIndex < quickInfo.Sections.Length; sectionIndex++)
		{
			if (sectionIndex > 0)
				block.Inlines.Add(new LineBreak());

			foreach (var part in quickInfo.Sections[sectionIndex])
			{
				var run = new Run(part.Text) { Foreground = foreground };
				var type = classificationTypes.GetClassificationType(RoslynClassification(part.Tag));
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

	static string RoslynClassification(string tag) => tag switch
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

	static object Expandable(
		ITextView view,
		IClassificationFormatMapService formatMaps,
		IClassificationTypeRegistryService classificationTypes,
		string quickInfo,
		ITrackingSpan trackingSpan,
		GramSyntaxKind kind,
		bool isDefinition)
	{
		var referenced = quickInfo.IndexOf("\n\nReferenced rule:", StringComparison.Ordinal);
		var recursive  = quickInfo.IndexOf("\n\nRecursive reference:", StringComparison.Ordinal);
		var split = referenced < 0
			? recursive
			: recursive < 0 ? referenced : Math.Min(referenced, recursive);

		var foreground = Application.Current.TryFindResource(EnvironmentColors.ToolTipTextBrushKey) as System.Windows.Media.Brush
			?? SystemColors.InfoTextBrush;
		var panel = new InteractiveQuickInfoPanel(trackingSpan);
		var header = new StackPanel { Orientation = Orientation.Horizontal };
		header.Children.Add(new CrispImage
		{
			Moniker = isDefinition
				? KnownMonikers.Method
				: kind == GramSyntaxKind.Identifier ? KnownMonikers.LocalVariable : KnownMonikers.IntellisenseKeyword,
			Width = 16,
			Height = 16,
			Margin = new Thickness(0, 0, 6, 0),
			VerticalAlignment = VerticalAlignment.Top,
		});
		var summary = split < 0 ? quickInfo : quickInfo.Substring(0, split);
		header.Children.Add(ClassifiedText(view, formatMaps, classificationTypes, summary, kind, isDefinition, foreground));
		panel.Children.Add(header);

		if (split < 0)
			return panel;

		var details = ClassifiedDetails(
			view,
			formatMaps,
			classificationTypes,
			quickInfo.Substring(split).TrimStart(),
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
		string text,
		GramSyntaxKind fallbackKind,
		bool grammar,
		System.Windows.Media.Brush foreground)
	{
		var block = new TextBlock { TextWrapping = TextWrapping.Wrap };
		AppendClassified(block, view, formatMaps, classificationTypes, text, fallbackKind, grammar, foreground);
		return block;
	}

	static TextBlock ClassifiedDetails(
		ITextView view,
		IClassificationFormatMapService formatMaps,
		IClassificationTypeRegistryService classificationTypes,
		string text,
		System.Windows.Media.Brush foreground)
	{
		var block = new TextBlock { TextWrapping = TextWrapping.Wrap };
		var lines = text.Replace("\r", string.Empty).Split('\n');
		for (var index = 0; index < lines.Length; index++)
		{
			var line = lines[index];
			if (line.StartsWith("Referenced rule:", StringComparison.Ordinal)
				|| line.StartsWith("Recursive reference:", StringComparison.Ordinal))
				block.Inlines.Add(new Run(line) { Foreground = foreground });
			else if (line.Length > 0)
				AppendClassified(block, view, formatMaps, classificationTypes, line, GramSyntaxKind.Identifier, true, foreground);

			if (index + 1 < lines.Length)
				block.Inlines.Add(new LineBreak());
		}
		return block;
	}

	static void AppendClassified(
		TextBlock block,
		ITextView view,
		IClassificationFormatMapService formatMaps,
		IClassificationTypeRegistryService classificationTypes,
		string text,
		GramSyntaxKind fallbackKind,
		bool grammar,
		System.Windows.Media.Brush foreground)
	{
		var formatMap = formatMaps.GetClassificationFormatMap(view);
		var position = 0;

		if (grammar)
			foreach (var item in GramLanguageService.Analyze(text).Classifications)
			{
				if (item.Position < position || item.Position + item.Length > text.Length)
					continue;

				if (item.Position > position)
					Add(PredefinedClassificationTypeNames.Text, text.Substring(position, item.Position - position));

				Add(Classification(item.Kind), text.Substring(item.Position, item.Length));
				position = item.Position + item.Length;
			}

		if (position < text.Length)
			Add(grammar ? PredefinedClassificationTypeNames.Text : Classification(fallbackKind), text.Substring(position));

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

	static string Classification(GramSyntaxKind kind) => kind switch
	{
		GramSyntaxKind.Invalid        => GramClassificationTypes.Invalid,
		GramSyntaxKind.Comment        => GramClassificationTypes.Comment,
		GramSyntaxKind.Keyword        => GramClassificationTypes.Keyword,
		GramSyntaxKind.Identifier     => GramClassificationTypes.Identifier,
		GramSyntaxKind.Number         => GramClassificationTypes.Number,
		GramSyntaxKind.Character      => GramClassificationTypes.Literal,
		GramSyntaxKind.String         => GramClassificationTypes.Literal,
		GramSyntaxKind.CharacterClass => GramClassificationTypes.Literal,
		GramSyntaxKind.EmbeddedCode   => GramClassificationTypes.EmbeddedCode,
		GramSyntaxKind.Transition     => GramClassificationTypes.TransitionStyle,
		GramSyntaxKind.SpecialSymbol  => GramClassificationTypes.SpecialSymbol,
		GramSyntaxKind.Operator       => GramClassificationTypes.Operator,
		GramSyntaxKind.Punctuation    => GramClassificationTypes.Punctuation,
		_ => PredefinedClassificationTypeNames.Text,
	};

	sealed class InteractiveQuickInfoPanel(ITrackingSpan trackingSpan) : StackPanel, IInteractiveQuickInfoContent, IDotGramQuickInfoContent
	{
		public bool ShouldDisplay => true;
		public ITrackingSpan TrackingSpan { get; } = trackingSpan;
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
	EmbeddedGrammarBufferAnalysis analysis,
	RoslynGramCompletion roslyn,
	IClassificationFormatMapService formatMaps,
	IClassificationTypeRegistryService classificationTypes) : IAsyncQuickInfoSource
{
	public async Task<QuickInfoItem?> GetQuickInfoItemAsync(
		IAsyncQuickInfoSession session,
		CancellationToken cancellationToken)
	{
		var snapshot = buffer.CurrentSnapshot;
		var point    = session.GetTriggerPoint(snapshot);

		if (point is null || !analysis.TryGet(snapshot, out var classifications, out _))
			return null;

		if (analysis.TryGetDslSymbols(snapshot, out var dslSymbols) &&
			dslSymbols
				.Where(symbol => symbol.Span.Contains(point.Value.Position))
				.OrderBy(symbol => symbol.Target.IndexOf('.') < 0)
				.ThenBy(symbol => symbol.Span.Length)
				.FirstOrDefault() is { Span.Length: > 0 } dslSymbol)
		{
			var span = new SnapshotSpan(snapshot, dslSymbol.Span.Start, dslSymbol.Span.Length);
			var trackingSpan = snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive);

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
			return new QuickInfoItem(
				trackingSpan,
				new DslSymbolQuickInfoContent(
					trackingSpan,
					dslSymbol.Role,
					dslSymbol.Target));
		}

		if (analysis.TryGetDslSites(snapshot, out var dslSites))
			foreach (var site in dslSites)
				if (site.Span.Contains(point.Value.Position))
				{
					var span = new SnapshotSpan(snapshot, site.Span.Start, site.Span.Length);
					var trackingSpan = snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive);

					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
					return new QuickInfoItem(
						trackingSpan,
						new DslQuickInfoContent(trackingSpan, site.LanguageId, site.EntryRule));
				}

		if (classifications.Any(item => item.GrammarSpan.Contains(point.Value.Position)) &&
			GramCSharpCompletionContext.TryGetExpression(
				snapshot.GetText(), point.Value.Position,
				out var expression, out var expressionStart, out var symbolStart, out var symbolLength))
		{
			var csharp = await roslyn.GetQuickInfoAsync(
				expression, point.Value.Position - expressionStart, cancellationToken).ConfigureAwait(false);
			if (csharp is not null)
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
				return GramQuickInfoSource.CreateCSharp(
					session.TextView, formatMaps, classificationTypes, snapshot,
					symbolStart, symbolLength, csharp);
			}
		}

		foreach (var item in classifications)
			if (item.Span.Contains(point.Value.Position))
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
				return GramQuickInfoSource.Create(
					session.TextView,
					formatMaps,
					classificationTypes,
					snapshot,
					item.Span.Start,
					item.Span.Length,
					item.Kind,
					item.QuickInfo);
			}

		foreach (var item in classifications)
			if (item.GrammarSpan.Contains(point.Value.Position))
			{
				var span = new SnapshotSpan(snapshot, point.Value.Position, 0);
				var trackingSpan = snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive);
				return new QuickInfoItem(
					trackingSpan,
					new DotGramQuickInfoSuppression(trackingSpan));
			}

		return null;
	}

	public void Dispose()
	{
	}
}

sealed class DslQuickInfoContent : StackPanel, IDotGramQuickInfoContent
{
	public DslQuickInfoContent(ITrackingSpan trackingSpan, string languageId, string entryRule)
	{
		TrackingSpan = trackingSpan;
		var foreground = Application.Current.TryFindResource(EnvironmentColors.ToolTipTextBrushKey) as System.Windows.Media.Brush
			?? SystemColors.InfoTextBrush;

		Children.Add(new TextBlock
		{
			Text = $"DotGram language: {languageId}",
			Foreground = foreground,
		});
		Children.Add(new TextBlock
		{
			Text = $"Entry rule: {entryRule}",
			Foreground = foreground,
		});
	}

	public bool ShouldDisplay => true;
	public ITrackingSpan TrackingSpan { get; }
}

sealed class DslSymbolQuickInfoContent : StackPanel, IDotGramQuickInfoContent
{
	public DslSymbolQuickInfoContent(ITrackingSpan trackingSpan, string role, string target)
	{
		TrackingSpan = trackingSpan;
		var foreground = Application.Current.TryFindResource(EnvironmentColors.ToolTipTextBrushKey) as System.Windows.Media.Brush
			?? SystemColors.InfoTextBrush;

		Children.Add(new TextBlock
		{
			Text = $"{role}: {target}",
			Foreground = foreground,
		});
	}

	public bool ShouldDisplay => true;
	public ITrackingSpan TrackingSpan { get; }
}
