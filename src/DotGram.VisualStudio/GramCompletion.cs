using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Language;

using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

[Export(typeof(IAsyncCompletionSourceProvider))]
[Name("DotGram completion")]
[ContentType(GramContentType.Name)]
sealed class GramCompletionSourceProvider : IAsyncCompletionSourceProvider
{
	public IAsyncCompletionSource GetOrCreate(ITextView textView) =>
		textView.Properties.GetOrCreateSingletonProperty(() =>
			new GramCompletionSource(
				textView.TextBuffer,
				GramBufferAnalysis.For(textView.TextBuffer)));
}

[Export(typeof(IAsyncCompletionSourceProvider))]
[Name("DotGram embedded completion")]
[ContentType("CSharp")]
sealed class EmbeddedGramCompletionSourceProvider : IAsyncCompletionSourceProvider
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	public IAsyncCompletionSource GetOrCreate(ITextView textView) =>
		textView.Properties.GetOrCreateSingletonProperty(() =>
			new EmbeddedGramCompletionSource(
				textView.TextBuffer,
				EmbeddedGrammarBufferAnalysis.For(textView.TextBuffer, Workspace, Documents)));
}

abstract class GramCompletionSourceBase : IAsyncCompletionSource
{
	static readonly string[] BuiltIns =
	[
		"any", "none", "eol", "eof", "trivia", "KeywordBoundary",
		"using", "namespace", "parse", "find", "as", "when", "recover", "with",
	];

	readonly Dictionary<string, string> _descriptions = new(StringComparer.Ordinal);

	public CompletionStartData InitializeCompletion(
		CompletionTrigger trigger,
		SnapshotPoint triggerLocation,
		CancellationToken token)
	{
		if (!IsApplicable(triggerLocation))
			return CompletionStartData.DoesNotParticipateInCompletion;

		return new CompletionStartData(
			CompletionParticipation.ProvidesItems,
			WordSpan(triggerLocation));
	}

	public Task<CompletionContext> GetCompletionContextAsync(
		IAsyncCompletionSession session,
		CompletionTrigger trigger,
		SnapshotPoint triggerLocation,
		SnapshotSpan applicableToSpan,
		CancellationToken token)
	{
		var definitions = Definitions(triggerLocation);
		var names = definitions.Keys.Concat(BuiltIns).Distinct(StringComparer.Ordinal).OrderBy(static name => name);
		var items = ImmutableArray.CreateBuilder<CompletionItem>();

		_descriptions.Clear();

		foreach (var name in names)
		{
			var definition = definitions.TryGetValue(name, out var found) ? found : default;
			var item = definition.Signature is null
				? new CompletionItem(name, this)
				: new CompletionItem(
					name,
					this,
					ImageElement.Empty,
					ImmutableArray<CompletionFilter>.Empty,
					definition.Signature.Substring(name.Length),
					definition.ParameterCount > 0 ? name + "(" : name,
					name,
					name,
					ImmutableArray<ImageElement>.Empty);

			items.Add(item);
			_descriptions[name] = definition.Description is not null
				? definition.Description
				: BuiltInDescription(name);
		}

		return Task.FromResult(new CompletionContext(items.ToImmutable()));
	}

	public Task<object> GetDescriptionAsync(
		IAsyncCompletionSession session,
		CompletionItem item,
		CancellationToken token) =>
		Task.FromResult<object>(
			_descriptions.TryGetValue(item.DisplayText, out var description)
				? description
				: "DotGram syntax");

	protected abstract bool IsApplicable(SnapshotPoint point);
	protected abstract IReadOnlyDictionary<string, RuleCompletion> Definitions(SnapshotPoint point);

	protected readonly struct RuleCompletion(string description, string signature, int parameterCount)
	{
		public string Description { get; } = description;
		public string Signature { get; } = signature;
		public int ParameterCount { get; } = parameterCount;
	}

	protected static RuleCompletion LocalCompletion(string name, GramSymbolKind kind) =>
		new(
			kind == GramSymbolKind.Parameter
				? $"{name}: DotGram rule parameter"
				: $"{name}: DotGram capture",
			name,
			0);

	static SnapshotSpan WordSpan(SnapshotPoint point)
	{
		var snapshot = point.Snapshot;
		var start    = point.Position;
		var end      = point.Position;

		while (start > 0 && IsNameCharacter(snapshot[start - 1])) start--;
		while (end < snapshot.Length && IsNameCharacter(snapshot[end])) end++;

		return new SnapshotSpan(snapshot, start, end - start);
	}

	static bool IsNameCharacter(char character) =>
		char.IsLetterOrDigit(character) || character == '_';

	static string BuiltInDescription(string name) => name switch
	{
		"any"             => "DotGram built-in rule: matches any character",
		"none"            => "DotGram built-in rule: never matches",
		"eol"             => "DotGram built-in rule: matches an end of line",
		"eof"             => "DotGram built-in rule: matches the end of input",
		"trivia"          => "DotGram built-in rule: matches grammar trivia",
		"KeywordBoundary" => "DotGram built-in keyword-boundary rule",
		_                  => $"DotGram keyword: {name}",
	};
}

sealed class GramCompletionSource(ITextBuffer buffer, GramBufferAnalysis analysis) : GramCompletionSourceBase
{
	protected override bool IsApplicable(SnapshotPoint point) => point.Snapshot.TextBuffer == buffer;

	protected override IReadOnlyDictionary<string, RuleCompletion> Definitions(SnapshotPoint point)
	{
		var document = analysis.Document(point.Snapshot);
		var definitions = document.Classifications
			.Where(static item => item.SymbolKind == GramSymbolKind.Rule &&
				item.DefinitionPosition == item.Position && item.QuickInfo is not null)
			.GroupBy(item => point.Snapshot.GetText(item.Position, item.Length), StringComparer.Ordinal)
			.ToDictionary(
				group => group.Key,
				group => new RuleCompletion(
					group.First().QuickInfo!,
					group.First().RuleSignature!,
					group.First().RuleParameterCount),
				StringComparer.Ordinal);

		foreach (var symbol in document.Symbols
			.Where(symbol => symbol.Kind != GramSymbolKind.Rule &&
				symbol.IsDefinition &&
				symbol.ScopeStart <= point.Position && point.Position < symbol.ScopeEnd)
			.GroupBy(symbol => symbol.Name, StringComparer.Ordinal))
		{
			var local = symbol.OrderBy(static item => item.Kind).First();
			definitions[local.Name] = LocalCompletion(local.Name, local.Kind);
		}

		return definitions;
	}

}

sealed class EmbeddedGramCompletionSource(
	ITextBuffer buffer,
	EmbeddedGrammarBufferAnalysis analysis) : GramCompletionSourceBase
{
	protected override bool IsApplicable(SnapshotPoint point)
	{
		if (point.Snapshot.TextBuffer != buffer ||
			!analysis.TryGet(point.Snapshot, out var classifications, out _))
			return false;

		return classifications.Any(item => item.GrammarSpan.Contains(point.Position));
	}

	protected override IReadOnlyDictionary<string, RuleCompletion> Definitions(SnapshotPoint point)
	{
		if (!analysis.TryGet(point.Snapshot, out var classifications, out _))
			return new Dictionary<string, RuleCompletion>();

		var definitions = classifications
			.Where(item =>
				item.GrammarSpan.Contains(point.Position) &&
				item.SymbolKind == GramSymbolKind.Rule &&
				item.DefinitionSpan == item.Span &&
				item.QuickInfo is not null)
			.GroupBy(item => point.Snapshot.GetText(item.Span.Start, item.Span.Length), StringComparer.Ordinal)
			.ToDictionary(
				group => group.Key,
				group => new RuleCompletion(
					group.First().QuickInfo!,
					group.First().RuleSignature!,
					group.First().RuleParameterCount),
				StringComparer.Ordinal);

		if (analysis.TryGetSymbols(point.Snapshot, out var symbols))
			foreach (var symbol in symbols
				.Where(symbol => symbol.Kind != GramSymbolKind.Rule &&
					symbol.IsDefinition &&
					symbol.GrammarSpan.Contains(point.Position) &&
					symbol.ScopeSpan.Contains(point.Position))
				.GroupBy(symbol => symbol.Name, StringComparer.Ordinal))
			{
				var local = symbol.OrderBy(static item => item.Kind).First();
				definitions[local.Name] = LocalCompletion(local.Name, local.Kind);
			}

		return definitions;
	}
}
