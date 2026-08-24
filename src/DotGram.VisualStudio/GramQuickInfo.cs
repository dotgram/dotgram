using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Language;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServices;
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
	public Task<QuickInfoItem?> GetQuickInfoItemAsync(
		IAsyncQuickInfoSession session,
		CancellationToken cancellationToken)
	{
		var snapshot = buffer.CurrentSnapshot;
		var point    = session.GetTriggerPoint(snapshot);

		if (point is null)
			return Task.FromResult<QuickInfoItem?>(null);

		foreach (var item in analysis.Document(snapshot).Classifications)
			if (Contains(item.Position, item.Length, point.Value.Position))
				return Task.FromResult<QuickInfoItem?>(
					Create(
						snapshot,
						item.Position,
						item.Length,
						item.Kind,
						item.QuickInfo));

		return Task.FromResult<QuickInfoItem?>(null);
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
			quickInfo ?? Describe(kind, text));
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
	public Task<QuickInfoItem?> GetQuickInfoItemAsync(
		IAsyncQuickInfoSession session,
		CancellationToken cancellationToken)
	{
		var snapshot = buffer.CurrentSnapshot;
		var point    = session.GetTriggerPoint(snapshot);

		if (point is null || !analysis.TryGet(snapshot, out var classifications, out _))
			return Task.FromResult<QuickInfoItem?>(null);

		foreach (var item in classifications)
			if (item.Span.Contains(point.Value.Position))
				return Task.FromResult<QuickInfoItem?>(
					GramQuickInfoSource.Create(
						snapshot,
						item.Span.Start,
						item.Span.Length,
						item.Kind,
						item.QuickInfo));

		return Task.FromResult<QuickInfoItem?>(null);
	}

	public void Dispose()
	{
	}
}
