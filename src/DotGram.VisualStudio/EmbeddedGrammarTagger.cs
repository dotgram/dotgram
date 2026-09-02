using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

[Export(typeof(IClassifierProvider))]
[ContentType("CSharp")]
sealed class EmbeddedGrammarClassifierProvider : IClassifierProvider
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	[Import]
	IClassificationTypeRegistryService Classifications { get; set; } = null!;

	public IClassifier GetClassifier(ITextBuffer buffer) =>
		buffer.Properties.GetOrCreateSingletonProperty(() =>
			new EmbeddedGrammarClassifier(
				EmbeddedGrammarBufferAnalysis.For(buffer, Workspace, Documents),
				Classifications));
}

sealed class EmbeddedGrammarClassifier : IClassifier
{
	readonly EmbeddedGrammarBufferAnalysis       _analysis;
	readonly Dictionary<GramSyntaxKind, IClassificationType> _types;
	readonly Dictionary<string, IClassificationType> _dslTypes;

	public EmbeddedGrammarClassifier(
		EmbeddedGrammarBufferAnalysis analysis,
		IClassificationTypeRegistryService classifications)
	{
		_analysis = analysis;
		_types    = new Dictionary<GramSyntaxKind, IClassificationType>
		{
			[GramSyntaxKind.Invalid]        = Type(classifications, GramClassificationTypes.Invalid),
			[GramSyntaxKind.Comment]        = Type(classifications, GramClassificationTypes.Comment),
			[GramSyntaxKind.Keyword]        = Type(classifications, GramClassificationTypes.Keyword),
			[GramSyntaxKind.Identifier]     = Type(classifications, GramClassificationTypes.Identifier),
			[GramSyntaxKind.Number]         = Type(classifications, GramClassificationTypes.Number),
			[GramSyntaxKind.Character]      = Type(classifications, GramClassificationTypes.Literal),
			[GramSyntaxKind.String]         = Type(classifications, GramClassificationTypes.Literal),
			[GramSyntaxKind.CaseInsensitiveCharacter] = Type(classifications, GramClassificationTypes.CaseInsensitiveLiteral),
			[GramSyntaxKind.CaseInsensitiveString] = Type(classifications, GramClassificationTypes.CaseInsensitiveLiteral),
			[GramSyntaxKind.CharacterClass] = Type(classifications, GramClassificationTypes.Literal),
			[GramSyntaxKind.EmbeddedCode]   = Type(classifications, GramClassificationTypes.EmbeddedCode),
			[GramSyntaxKind.Transition]     = Type(classifications, GramClassificationTypes.TransitionStyle),
			[GramSyntaxKind.SpecialSymbol]  = Type(classifications, GramClassificationTypes.SpecialSymbol),
			[GramSyntaxKind.Operator]       = Type(classifications, GramClassificationTypes.Operator),
			[GramSyntaxKind.Punctuation]    = Type(classifications, GramClassificationTypes.Punctuation),
		};
		_dslTypes = DslTypes(classifications);

		_analysis.Changed += Changed;
	}

	public event EventHandler<ClassificationChangedEventArgs>? ClassificationChanged;

	public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
	{
		var snapshot = span.Snapshot;
		var result   = new List<ClassificationSpan>();

		if (!_analysis.TryGet(snapshot, out var classifications, out _))
			return result;

		foreach (var item in classifications)
		{
			var classified = new SnapshotSpan(snapshot, item.Span.Start, item.Span.Length);

			if (classified.IntersectsWith(span))
				result.Add(new ClassificationSpan(classified, _types[item.Kind]));
		}

		if (_analysis.TryGetDslClassifications(snapshot, out var dslClassifications))
			foreach (var item in dslClassifications)
			{
				if (!_dslTypes.TryGetValue(item.Role, out var type))
					continue;

				var classified = new SnapshotSpan(snapshot, item.Span.Start, item.Span.Length);
				if (classified.IntersectsWith(span))
					result.Add(new ClassificationSpan(classified, type));
			}

		return result;
	}

	void Changed(ITextSnapshot snapshot) =>
		ClassificationChanged?.Invoke(
			this,
			new ClassificationChangedEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));

	static IClassificationType Type(IClassificationTypeRegistryService classifications, string name) =>
		classifications.GetClassificationType(name) ??
		throw new InvalidOperationException($"Visual Studio classification '{name}' is unavailable.");

	static Dictionary<string, IClassificationType> DslTypes(IClassificationTypeRegistryService types) => new()
	{
		["Keyword"]     = Type(types, GramClassificationTypes.Keyword),
		["Identifier"]  = Type(types, GramClassificationTypes.Identifier),
		["Type"]        = Type(types, GramClassificationTypes.DslType),
		["Variable"]    = Type(types, GramClassificationTypes.DslVariable),
		["Function"]    = Type(types, GramClassificationTypes.DslFunction),
		["Method"]      = Type(types, GramClassificationTypes.DslFunction),
		["Property"]    = Type(types, GramClassificationTypes.DslProperty),
		["Number"]      = Type(types, GramClassificationTypes.Number),
		["String"]      = Type(types, GramClassificationTypes.Literal),
		["Comment"]     = Type(types, GramClassificationTypes.Comment),
		["Operator"]    = Type(types, GramClassificationTypes.Operator),
		["Punctuation"] = Type(types, GramClassificationTypes.Punctuation),
		["Namespace"]   = Type(types, GramClassificationTypes.DslNamespace),
		["Parameter"]   = Type(types, GramClassificationTypes.DslParameter),
		["Label"]       = Type(types, GramClassificationTypes.DslLabel),
	};
}

[Export(typeof(ITaggerProvider))]
[ContentType("CSharp")]
[TagType(typeof(ErrorTag))]
sealed class EmbeddedGrammarDiagnosticTaggerProvider : ITaggerProvider
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	public ITagger<T>? CreateTagger<T>(ITextBuffer buffer) where T : ITag =>
		new EmbeddedGrammarDiagnosticTagger(
			EmbeddedGrammarBufferAnalysis.For(buffer, Workspace, Documents)) as ITagger<T>;
}

sealed class EmbeddedGrammarDiagnosticTagger : ITagger<ErrorTag>
{
	readonly EmbeddedGrammarBufferAnalysis _analysis;

	public EmbeddedGrammarDiagnosticTagger(EmbeddedGrammarBufferAnalysis analysis)
	{
		_analysis = analysis;
		_analysis.Changed += Changed;
	}

	public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

	public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
	{
		if (spans.Count == 0)
			yield break;

		var snapshot = spans[0].Snapshot;

		if (!_analysis.TryGet(snapshot, out _, out var diagnostics))
			yield break;

		foreach (var item in diagnostics)
		{
			var tagged = Span(snapshot, item.Span.Start, item.Span.Length);

			if (spans.IntersectsWith(tagged))
				yield return new TagSpan<ErrorTag>(
					tagged,
					new ErrorTag(
						ErrorType(item.Diagnostic.Severity),
						$"{item.Diagnostic.Id}: {item.Diagnostic.Message}"));
		}
	}

	void Changed(ITextSnapshot snapshot) =>
		TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(
			new SnapshotSpan(snapshot, 0, snapshot.Length)));

	static SnapshotSpan Span(ITextSnapshot snapshot, int position, int length)
	{
		position = Math.Max(0, Math.Min(position, snapshot.Length));
		length   = Math.Max(0, Math.Min(length, snapshot.Length - position));

		if (length == 0 && snapshot.Length > 0)
		{
			if (position == snapshot.Length)
				position--;

			length = 1;
		}

		return new SnapshotSpan(snapshot, position, length);
	}

	static string ErrorType(GramSeverity severity) => severity switch
	{
		GramSeverity.Error   => PredefinedErrorTypeNames.SyntaxError,
		GramSeverity.Warning => PredefinedErrorTypeNames.Warning,
		_                    => PredefinedErrorTypeNames.Information,
	};
}

sealed class EmbeddedGrammarBufferAnalysis
{
	const int AnalysisDelayMilliseconds = 200;

	readonly ITextBuffer                _buffer;
	readonly VisualStudioWorkspace      _workspace;
	readonly ITextDocumentFactoryService _documents;
	readonly object                     _gate = new();
	readonly DslEmbeddedSiteCache       _dslCache = new();

	CancellationTokenSource?          _cancellation;
	ITextSnapshot?                    _snapshot;
	ITextSnapshot?                    _retrySnapshot;
	int                               _retryCount;
	IReadOnlyList<HostClassification> _classifications = [];
	IReadOnlyList<HostDslClassification> _dslClassifications = [];
	IReadOnlyList<HostDslSymbol>         _dslSymbols = [];
	IReadOnlyList<HostDslSite>           _dslSites = [];
	IReadOnlyList<HostDiagnostic>     _diagnostics     = [];
	IReadOnlyList<HostSymbolOccurrence> _symbols       = [];
	IReadOnlyList<HostBracePair>       _braces        = [];
	IReadOnlyList<HostFoldingRange>    _foldingRanges = [];
	IReadOnlyList<HostDocumentSymbol> _documentSymbols = [];
	IReadOnlyList<HostPublishedApi>    _publishedApis = [];

	EmbeddedGrammarBufferAnalysis(
		ITextBuffer buffer,
		VisualStudioWorkspace workspace,
		ITextDocumentFactoryService documents)
	{
		_buffer    = buffer;
		_workspace = workspace;
		_documents = documents;

		_buffer.Changed += BufferChanged;
		_workspace.WorkspaceChanged += WorkspaceChanged;
		Schedule(buffer.CurrentSnapshot);
	}

	public event Action<ITextSnapshot>? Changed;

	public static EmbeddedGrammarBufferAnalysis For(
		ITextBuffer buffer,
		VisualStudioWorkspace workspace,
		ITextDocumentFactoryService documents) =>
		buffer.Properties.GetOrCreateSingletonProperty(() =>
			new EmbeddedGrammarBufferAnalysis(buffer, workspace, documents));

	public bool TryGet(
		ITextSnapshot snapshot,
		out IReadOnlyList<HostClassification> classifications,
		out IReadOnlyList<HostDiagnostic> diagnostics)
	{
		lock (_gate)
		{
			if (_snapshot == snapshot)
			{
				classifications = _classifications;
				diagnostics     = _diagnostics;

				return true;
			}
		}

		Schedule(snapshot);

		classifications = [];
		diagnostics     = [];

		return false;
	}

	public bool TryGetSymbols(
		ITextSnapshot snapshot,
		out IReadOnlyList<HostSymbolOccurrence> symbols)
	{
		lock (_gate)
		{
			if (_snapshot == snapshot)
			{
				symbols = _symbols;

				return true;
			}
		}

		Schedule(snapshot);
		symbols = [];

		return false;
	}

	public bool TryGetDslClassifications(
		ITextSnapshot snapshot,
		out IReadOnlyList<HostDslClassification> classifications)
	{
		lock (_gate)
		{
			if (_snapshot == snapshot)
			{
				classifications = _dslClassifications;
				return true;
			}
		}

		Schedule(snapshot);
		classifications = [];
		return false;
	}

	public bool TryGetDslSites(ITextSnapshot snapshot, out IReadOnlyList<HostDslSite> sites)
	{
		lock (_gate)
		{
			if (_snapshot == snapshot)
			{
				sites = _dslSites;
				return true;
			}
		}

		Schedule(snapshot);
		sites = [];
		return false;
	}

	public bool TryGetDslSymbols(ITextSnapshot snapshot, out IReadOnlyList<HostDslSymbol> symbols)
	{
		lock (_gate)
		{
			if (_snapshot == snapshot)
			{
				symbols = _dslSymbols;
				return true;
			}
		}

		Schedule(snapshot);
		symbols = [];
		return false;
	}

	public bool TryGetDslCompletions(
		ITextSnapshot snapshot,
		int position,
		out IReadOnlyList<string> expected)
	{
		if (TryGetDslSites(snapshot, out var sites) &&
			sites.FirstOrDefault(site => site.CompletionPosition == position) is { Expected.Count: > 0 } site)
		{
			expected = site.Expected;
			return true;
		}

		expected = [];
		return false;
	}

	public bool TryGetPublishedApis(
		ITextSnapshot snapshot,
		out IReadOnlyList<HostPublishedApi> publishedApis)
	{
		lock (_gate)
		{
			if (_snapshot == snapshot)
			{
				publishedApis = _publishedApis;
				return true;
			}
		}

		Schedule(snapshot);
		publishedApis = [];
		return false;
	}

	public bool TryGetStructure(
		ITextSnapshot snapshot,
		out IReadOnlyList<HostBracePair> braces,
		out IReadOnlyList<HostFoldingRange> foldingRanges)
	{
		lock (_gate)
		{
			if (_snapshot == snapshot)
			{
				braces = _braces;
				foldingRanges = _foldingRanges;
				return true;
			}
		}

		Schedule(snapshot);
		braces = [];
		foldingRanges = [];
		return false;
	}

	public bool TryGetDocumentSymbols(
		ITextSnapshot snapshot,
		out IReadOnlyList<HostDocumentSymbol> symbols)
	{
		lock (_gate)
		{
			if (_snapshot == snapshot)
			{
				symbols = _documentSymbols;
				return true;
			}
		}

		Schedule(snapshot);
		symbols = [];
		return false;
	}

	void BufferChanged(object sender, TextContentChangedEventArgs change)
	{
		lock (_gate)
		{
			if (_snapshot == change.Before)
			{
				_classifications = TranslateClassifications(_classifications, change.Before, change.After);
				_dslClassifications = TranslateDslClassifications(_dslClassifications, change.Before, change.After);
				// Semantic spans must not be exposed against a newer snapshot. In particular,
				// Visual Studio turns stale navigable spans into Ctrl+click hyperlinks that can
				// cover the entire embedded string. Fresh analysis restores them shortly after.
				_symbols = [];
				_dslSymbols = [];
				_dslSites = [];
				_snapshot = change.After;
			}
		}

		Schedule(change.After);
	}

	void WorkspaceChanged(object sender, WorkspaceChangeEventArgs change)
	{
		if (!_documents.TryGetTextDocument(_buffer, out var textDocument) || textDocument.FilePath is null)
			return;

		var ids = change.NewSolution.GetDocumentIdsWithFilePath(textDocument.FilePath);

		if (!ids.Any() ||
			change.DocumentId is not null && !ids.Contains(change.DocumentId) ||
			change.ProjectId is not null && !ids.Any(id => id.ProjectId == change.ProjectId))
			return;

		Schedule(_buffer.CurrentSnapshot);
	}

	void Schedule(ITextSnapshot snapshot)
	{
		CancellationToken cancellationToken;

		lock (_gate)
		{
			_cancellation?.Cancel();
			_cancellation?.Dispose();
			_cancellation = new CancellationTokenSource();
			cancellationToken = _cancellation.Token;
		}

		_ = Task.Run(() => AnalyzeScheduledAsync(snapshot, cancellationToken));
	}

	async Task AnalyzeScheduledAsync(ITextSnapshot snapshot, CancellationToken cancellationToken)
	{
		try
		{
			await Task.Delay(AnalysisDelayMilliseconds, cancellationToken).ConfigureAwait(false);
			await AnalyzeAsync(snapshot, cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
		}
	}

	async Task AnalyzeAsync(ITextSnapshot snapshot, CancellationToken cancellationToken)
	{
		try
		{
			if (!_documents.TryGetTextDocument(_buffer, out var textDocument) || textDocument.FilePath is null)
				return;

			var id       = _workspace.CurrentSolution.GetDocumentIdsWithFilePath(textDocument.FilePath).FirstOrDefault();
			var document = id is null ? null : _workspace.CurrentSolution.GetDocument(id);

			if (document is null)
				return;

			document = document.WithText(SourceText.From(snapshot.GetText(), Encoding.UTF8));

			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			if (root is null || model is null)
				return;

			var analyses       = EmbeddedGrammarService.Analyze(model, root, cancellationToken);
			var classifications = analyses.SelectMany(static analysis => analysis.Classifications).ToArray();
			var dslSites        = await DslEmbeddedSiteAnalysis.AnalyzeAsync(
				document,
				root,
				model,
				cancellationToken,
				_dslCache).ConfigureAwait(false);
			var dslDiagnostics  = await DslClassificationDiagnostics.AnalyzeAsync(
				document,
				root,
				model.Compilation,
				cancellationToken).ConfigureAwait(false);
			var diagnostics     = analyses.SelectMany(static analysis => analysis.Diagnostics)
				.Concat(dslDiagnostics)
				.Concat(dslSites.Diagnostics)
				.ToArray();
			var symbols         = analyses.SelectMany(static analysis => analysis.Symbols).ToArray();
			var braces          = analyses.SelectMany(static analysis => analysis.Braces).ToArray();
			var foldingRanges   = analyses.SelectMany(static analysis => analysis.FoldingRanges).ToArray();
			var documentSymbols = analyses.SelectMany(static analysis => analysis.DocumentSymbols).ToArray();
			var publishedApis   = analyses.SelectMany(static analysis => analysis.PublishedApis).ToArray();
			var retry           = false;

			lock (_gate)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (_buffer.CurrentSnapshot != snapshot)
					return;

				var preserveEmbeddedAnalysis = ShouldPreserveEmbeddedAnalysis(
					root.ContainsDiagnostics,
					analyses.Count,
					_classifications.Count,
					_dslClassifications.Count,
					_documentSymbols.Count,
					_dslSites.Count);

				if (preserveEmbeddedAnalysis && _snapshot is not null)
				{
					_classifications = TranslateClassifications(_classifications, _snapshot, snapshot);
					_dslClassifications = TranslateDslClassifications(_dslClassifications, _snapshot, snapshot);
					_dslSymbols = TranslateDslSymbols(_dslSymbols, _snapshot, snapshot);
					_dslSites = TranslateDslSites(_dslSites, _snapshot, snapshot);
				}
				else
				{
					_classifications = classifications;
					_dslClassifications = dslSites.Classifications;
					_dslSymbols = dslSites.Symbols;
					_dslSites = dslSites.Sites;
				}

				_snapshot        = snapshot;
				_diagnostics     = diagnostics;
				_symbols         = symbols;
				_braces          = braces;
				_foldingRanges   = foldingRanges;
				if (!preserveEmbeddedAnalysis)
					_documentSymbols = documentSymbols;
				_publishedApis   = publishedApis;

				if (_retrySnapshot != snapshot)
				{
					_retrySnapshot = snapshot;
					_retryCount    = 0;
				}

				retry = analyses.Count == 0 &&
					snapshot.GetText().IndexOf("[Gram", StringComparison.Ordinal) >= 0 &&
					_retryCount++ < 5;
			}

			await NotifyChangedAsync(snapshot);

			if (retry)
				_ = RetryAsync(snapshot, cancellationToken);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception exception) when (exception is not OutOfMemoryException)
		{
			ActivityLog.LogError("DotGram.VisualStudio", exception.ToString());
			// An editor extension must degrade to no tags rather than destabilize Visual Studio.
		}
	}

	async Task NotifyChangedAsync(ITextSnapshot snapshot)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
		Changed?.Invoke(snapshot);
	}

	internal static bool ShouldPreserveEmbeddedAnalysis(
		bool hasSyntaxErrors,
		int analysisCount,
		int previousClassificationCount,
		int previousDslClassificationCount,
		int previousSymbolCount,
		int previousDslSiteCount) =>
		hasSyntaxErrors && analysisCount == 0 &&
		(previousClassificationCount > 0 || previousDslClassificationCount > 0 ||
		 previousSymbolCount > 0 || previousDslSiteCount > 0);

	static IReadOnlyList<HostClassification> TranslateClassifications(
		IReadOnlyList<HostClassification> classifications,
		ITextSnapshot source,
		ITextSnapshot target) =>
		classifications.Select(item => new HostClassification(
			Translate(item.Span, source, target),
			item.Kind,
			item.QuickInfo,
			item.DefinitionSpan is { } definition ? Translate(definition, source, target) : null,
			Translate(item.GrammarSpan, source, target),
			item.RuleSignature,
			item.RuleParameterCount,
			item.SymbolKind)).ToArray();

	static IReadOnlyList<HostDslClassification> TranslateDslClassifications(
		IReadOnlyList<HostDslClassification> classifications,
		ITextSnapshot source,
		ITextSnapshot target) =>
		classifications.Select(item => new HostDslClassification(
			Translate(item.Span, source, target),
			item.Role)).ToArray();

	static IReadOnlyList<HostDslSymbol> TranslateDslSymbols(
		IReadOnlyList<HostDslSymbol> symbols,
		ITextSnapshot source,
		ITextSnapshot target) =>
		symbols.Select(item => new HostDslSymbol(
			Translate(item.Span, source, target),
			item.Role,
			item.Target,
			item.DefinitionPath,
			item.DefinitionLine,
			item.DefinitionColumn)).ToArray();

	static IReadOnlyList<HostDslSite> TranslateDslSites(
		IReadOnlyList<HostDslSite> sites,
		ITextSnapshot source,
		ITextSnapshot target) =>
		sites.Select(item => new HostDslSite(
			Translate(item.Span, source, target),
			item.LanguageId,
			item.EntryRule,
			Translate(new TextSpan(item.CompletionPosition, 0), source, target).Start,
			item.Expected)).ToArray();

	static TextSpan Translate(TextSpan span, ITextSnapshot source, ITextSnapshot target)
	{
		var translated = new SnapshotSpan(source, span.Start, span.Length)
			.TranslateTo(target, SpanTrackingMode.EdgeExclusive);

		return new TextSpan(translated.Start.Position, translated.Length);
	}

	async Task RetryAsync(ITextSnapshot snapshot, CancellationToken cancellationToken)
	{
		try
		{
			await Task.Delay(500, cancellationToken).ConfigureAwait(false);

			if (_buffer.CurrentSnapshot == snapshot)
				Schedule(snapshot);
		}
		catch (OperationCanceledException)
		{
		}
	}
}
