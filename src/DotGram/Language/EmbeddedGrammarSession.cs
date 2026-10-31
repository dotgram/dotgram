using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.Language;

/// <summary>What the analysis session of a C# document's embedded grammars asks of the editor that holds it.</summary>
/// <typeparam name="TVersion">One version of the document's text, compared by reference.</typeparam>
interface IEmbeddedGrammarHost<TVersion> where TVersion : class
{
	IGramScheduler Scheduler { get; }

	/// <summary>The document's version now.</summary>
	TVersion Current { get; }

	/// <summary>The document's file, or null while it has none.</summary>
	string? FilePath { get; }

	string Text(TVersion version);

	int Length(TVersion version);

	char At(TVersion version, int position);

	/// <summary>A span of one version where it is in another, edges not growing.</summary>
	TextSpan Translate(TextSpan span, TVersion from, TVersion to);

	/// <summary>The document as the solution has it, its text replaced by this version's; null where the solution has no such document.</summary>
	Document? Document(TVersion version);

	/// <summary>Where the session's <c>Changed</c> event may be raised.</summary>
	Task BeforeChangedAsync(CancellationToken cancellationToken);

	/// <summary>An exception the session caught and went on after.</summary>
	void Report(Exception exception);
}

/// <summary>Everything known about one version of a C# document's embedded grammars, or nothing while it is not known yet.</summary>
/// <param name="Current">Whether this is the version asked about; false while it is being analysed, and then every list is empty.</param>
sealed record EmbeddedGrammarView(
	bool Current,
	IReadOnlyList<HostClassification> Classifications,
	IReadOnlyList<HostDiagnostic> Diagnostics,
	IReadOnlyList<HostSymbolOccurrence> Symbols,
	IReadOnlyList<HostDslClassification> DslClassifications,
	IReadOnlyList<HostDslSymbol> DslSymbols,
	IReadOnlyList<HostDslSite> DslSites,
	IReadOnlyList<HostBracePair> Braces,
	IReadOnlyList<HostFoldingRange> FoldingRanges,
	IReadOnlyList<HostDocumentSymbol> DocumentSymbols,
	IReadOnlyList<HostPublishedApi> PublishedApis)
{
	public static EmbeddedGrammarView None { get; } = new(false, [], [], [], [], [], [], [], [], [], []);

	/// <summary>The expected literals of the DSL site whose completion position this is.</summary>
	public bool TryGetDslCompletions(int position, out IReadOnlyList<string> expected)
	{
		if (DslSites.FirstOrDefault(site => site.CompletionPosition == position) is { Expected.Count: > 0 } site)
		{
			expected = site.Expected;
			return true;
		}

		expected = [];
		return false;
	}
}

/// <summary>What every <see cref="EmbeddedGrammarSession{TVersion}"/> shares: its timings, and the rules for keeping what was shown.</summary>
static class EmbeddedGrammarSession
{
	internal const int AnalysisDelayMilliseconds         = 200;
	internal const int SemanticAnalysisDelayMilliseconds = 800;

	/// <summary>A document that names <c>[Gram</c> and yields no grammar is asked again this often, this many times: the project may not be loaded yet.</summary>
	internal const int RetryDelayMilliseconds = 500;
	internal const int RetryCount             = 5;

	internal static IReadOnlyList<HostClassification> MergeProvisionalClassifications(
		IReadOnlyList<HostClassification> previous,
		IReadOnlyList<HostClassification> provisional)
	{
		if (previous.Count == 0 || provisional.Count == 0)
			return provisional.Count == 0 ? previous : provisional;

		var current = provisional.OrderBy(static item => item.Span.Start).ToArray();
		var old = previous.OrderBy(static item => item.Span.Start).ToArray();
		var maximumEnds = new int[current.Length];
		for (var index = 0; index < current.Length; index++)
			maximumEnds[index] = Math.Max(index == 0 ? 0 : maximumEnds[index - 1], current[index].Span.End);

		var merged = new List<HostClassification>(current);
		foreach (var candidate in old)
		{
			var low = 0;
			var high = current.Length;
			while (low < high)
			{
				var middle = low + (high - low) / 2;
				if (current[middle].Span.Start < candidate.Span.End)
					low = middle + 1;
				else
					high = middle;
			}

			if (low == 0 || maximumEnds[low - 1] <= candidate.Span.Start)
				merged.Add(candidate);
		}

		return merged.OrderBy(static item => item.Span.Start).ToArray();
	}

	internal static bool ShouldPreserveEmbeddedAnalysis(
		bool hasSyntaxErrors,
		int analysisCount,
		int previousClassificationCount,
		int previousDslClassificationCount,
		int previousSymbolCount,
		int previousDslSiteCount)
	{
		return hasSyntaxErrors && analysisCount == 0 &&
		(previousClassificationCount > 0 || previousDslClassificationCount > 0 ||
		 previousSymbolCount > 0 || previousDslSiteCount > 0);
	}
}

/// <summary>
/// The analysis of the grammars written into one open C# document's <c>[Gram("…")]</c> attributes,
/// and of the DSL literals in it, kept current: colours and structure first, from C# syntax alone,
/// then everything once Roslyn has a semantic model.
/// </summary>
sealed class EmbeddedGrammarSession<TVersion> where TVersion : class
{
	readonly IEmbeddedGrammarHost<TVersion> _host;
	readonly object                         _gate = new();
	readonly DslEmbeddedSiteCache           _dslCache = new();

	CancellationTokenSource?          _cancellation;
	TVersion?                         _snapshot;
	TVersion?                         _retrySnapshot;
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

	/// <summary>Starts the first analysis of the document's current version.</summary>
	public EmbeddedGrammarSession(IEmbeddedGrammarHost<TVersion> host)
	{
		_host = host;
		Schedule(host.Current);
	}

	/// <summary>A version's analysis, or part of it, is ready: ask for it again.</summary>
	public event Action<TVersion>? Changed;

	/// <summary>What is known about a version: the published analysis, or nothing with an analysis started.</summary>
	public EmbeddedGrammarView View(TVersion snapshot)
	{
		lock (_gate)
		{
			if (_snapshot == snapshot)
				return new EmbeddedGrammarView(
					true,
					_classifications,
					_diagnostics,
					_symbols,
					_dslClassifications,
					_dslSymbols,
					_dslSites,
					_braces,
					_foldingRanges,
					_documentSymbols,
					_publishedApis);
		}

		Schedule(snapshot);

		return EmbeddedGrammarView.None;
	}

	/// <summary>
	/// The document was edited: the colours known are carried over to the new version at once;
	/// what depends on meaning waits for the next analysis.
	/// </summary>
	/// <remarks>Raises <see cref="Changed"/> on the caller's thread when something was carried over.</remarks>
	public void Edited(TVersion before, TVersion after)
	{
		var translated = false;

		lock (_gate)
		{
			if (_snapshot == before)
			{
				_classifications = TranslateClassifications(_classifications, before, after);
				(_classifications, _) = RefreshLiteralStyles(
					_classifications,
					after);
				_dslClassifications = TranslateDslClassifications(_dslClassifications, before, after);
				// Semantic spans must not be exposed against a newer snapshot. In particular,
				// Visual Studio turns stale navigable spans into Ctrl+click hyperlinks that can
				// cover the entire embedded string. Fresh analysis restores them shortly after.
				_symbols = [];
				_dslSymbols = [];
				_dslSites = [];
				_snapshot = after;
				translated = true;
			}
		}

		// An editor invalidates classifications for the changed snapshot immediately. Tell it
		// that the translated DotGram spans are already available; otherwise the C# string
		// classification is visible until the deferred analysis publishes again.
		if (translated)
			Changed?.Invoke(after);

		Schedule(after);
	}

	/// <summary>The solution changed: the document is analysed again where the change is about it or its project.</summary>
	public void WorkspaceChanged(WorkspaceChangeEventArgs change)
	{
		if (_host.FilePath is not { } filePath)
			return;

		var ids = change.NewSolution.GetDocumentIdsWithFilePath(filePath);

		if (!ids.Any() ||
			change.DocumentId is not null && !ids.Contains(change.DocumentId) ||
			change.ProjectId is not null && !ids.Any(id => id.ProjectId == change.ProjectId))
			return;

		Schedule(_host.Current);
	}

	void Schedule(TVersion snapshot)
	{
		CancellationToken cancellationToken;

		lock (_gate)
		{
			_cancellation?.Cancel();
			_cancellation?.Dispose();
			_cancellation = new CancellationTokenSource();
			cancellationToken = _cancellation.Token;
		}

		_host.Scheduler.Run(() => AnalyzeScheduledAsync(snapshot, cancellationToken));
	}

	async Task AnalyzeScheduledAsync(TVersion snapshot, CancellationToken cancellationToken)
	{
		try
		{
			await _host.Scheduler.DelayAsync(EmbeddedGrammarSession.AnalysisDelayMilliseconds, cancellationToken).ConfigureAwait(false);
			await AnalyzeAsync(snapshot, cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
		}
	}

	async Task AnalyzeAsync(TVersion snapshot, CancellationToken cancellationToken)
	{
		try
		{
			var document = _host.Document(snapshot);

			if (document is null)
				return;

			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root is null)
				return;

			// Initial colors and structure need only C# syntax. Publish those before asking
			// Roslyn for a semantic model: on large source files compilation can contend with
			// the editor's own document-load work for several seconds.
			var provisional = EmbeddedGrammarService.AnalyzeSyntactic(root, cancellationToken);
			if (provisional.Count > 0)
			{
				if (!PublishProvisional(snapshot, provisional, cancellationToken))
					return;

				await NotifyChangedAsync(snapshot).ConfigureAwait(false);
				await _host.Scheduler.DelayAsync(EmbeddedGrammarSession.SemanticAnalysisDelayMilliseconds, cancellationToken).ConfigureAwait(false);
			}

			var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			if (model is null)
				return;

			// The project's .gram files only where this document has a grammar in an attribute,
			// the one place an include is read; most C# documents have none.
			var includedFile   = provisional.Count == 0
				? null
				: await StandaloneGrammarInheritance.GramFilesAsync(document.Project, cancellationToken).ConfigureAwait(false);
			var analyses       = EmbeddedGrammarService.Analyze(model, root, cancellationToken, includedFile);
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

				if (_host.Current != snapshot)
					return;

				var preserveEmbeddedAnalysis = EmbeddedGrammarSession.ShouldPreserveEmbeddedAnalysis(
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
					_host.Text(snapshot).IndexOf("[Gram", StringComparison.Ordinal) >= 0 &&
					_retryCount++ < EmbeddedGrammarSession.RetryCount;
			}

			await NotifyChangedAsync(snapshot).ConfigureAwait(false);

			if (retry)
				_host.Scheduler.Run(() => RetryAsync(snapshot, cancellationToken));
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception exception) when (exception is not OutOfMemoryException)
		{
			_host.Report(exception);
			// An editor extension must degrade to no tags rather than destabilize its editor.
		}
	}

	bool PublishProvisional(
		TVersion snapshot,
		IReadOnlyList<EmbeddedGrammarAnalysis> analyses,
		CancellationToken cancellationToken)
	{
		var classifications = analyses.SelectMany(static analysis => analysis.Classifications).ToArray();
		var braces = analyses.SelectMany(static analysis => analysis.Braces).ToArray();
		var foldingRanges = analyses.SelectMany(static analysis => analysis.FoldingRanges).ToArray();

		lock (_gate)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (_host.Current != snapshot)
				return false;

			_snapshot = snapshot;
			_classifications = EmbeddedGrammarSession.MergeProvisionalClassifications(_classifications, classifications);
			_braces = braces;
			_foldingRanges = foldingRanges;
		}

		return true;
	}

	async Task NotifyChangedAsync(TVersion snapshot)
	{
		await _host.BeforeChangedAsync(CancellationToken.None).ConfigureAwait(false);
		Changed?.Invoke(snapshot);
	}

	IReadOnlyList<HostClassification> TranslateClassifications(
		IReadOnlyList<HostClassification> classifications,
		TVersion source,
		TVersion target)
	{
		return classifications.Select(item => new HostClassification(
			Translate(item.Span, source, target),
			item.Kind,
			item.QuickInfo,
			item.DefinitionSpan is { } definition ? Translate(definition, source, target) : null,
			Translate(item.GrammarSpan, source, target),
			item.RuleSignature,
			item.RuleParameterCount,
			item.SymbolKind)).ToArray();
	}

	(IReadOnlyList<HostClassification> Classifications, bool Changed) RefreshLiteralStyles(
		IReadOnlyList<HostClassification> classifications,
		TVersion snapshot)
	{
		var result = new HostClassification[classifications.Count];
		var changed = false;
		var length = _host.Length(snapshot);

		for (var index = 0; index < classifications.Count; index++)
		{
			var item = classifications[index];
			var span = item.Span;
			var kind = item.Kind;
			var isCharacter = kind is GramSyntaxKind.Character or GramSyntaxKind.CaseInsensitiveCharacter;
			var isString = kind is GramSyntaxKind.String or GramSyntaxKind.CaseInsensitiveString;

			if ((isCharacter || isString) && span.End <= length)
			{
				var hasSuffix = span.Length > 0 && _host.At(snapshot, span.End - 1) == 'i';
				if (!hasSuffix && span.End < length && _host.At(snapshot, span.End) == 'i')
				{
					span = new TextSpan(span.Start, span.Length + 1);
					hasSuffix = true;
				}

				var refreshed = isCharacter
					? hasSuffix ? GramSyntaxKind.CaseInsensitiveCharacter : GramSyntaxKind.Character
					: hasSuffix ? GramSyntaxKind.CaseInsensitiveString : GramSyntaxKind.String;
				changed |= refreshed != kind || span != item.Span;
				kind = refreshed;
			}

			result[index] = new HostClassification(
				span,
				kind,
				item.QuickInfo,
				item.DefinitionSpan,
				item.GrammarSpan,
				item.RuleSignature,
				item.RuleParameterCount,
				item.SymbolKind);
		}

		return (result, changed);
	}

	IReadOnlyList<HostDslClassification> TranslateDslClassifications(
		IReadOnlyList<HostDslClassification> classifications,
		TVersion source,
		TVersion target)
	{
		return classifications.Select(item => new HostDslClassification(
			Translate(item.Span, source, target),
			item.Role)).ToArray();
	}

	IReadOnlyList<HostDslSymbol> TranslateDslSymbols(
		IReadOnlyList<HostDslSymbol> symbols,
		TVersion source,
		TVersion target)
	{
		return symbols.Select(item => new HostDslSymbol(
			Translate(item.Span, source, target),
			item.Role,
			item.Target,
			item.DefinitionPath,
			item.DefinitionLine,
			item.DefinitionColumn)).ToArray();
	}

	IReadOnlyList<HostDslSite> TranslateDslSites(
		IReadOnlyList<HostDslSite> sites,
		TVersion source,
		TVersion target)
	{
		return sites.Select(item => new HostDslSite(
			Translate(item.Span, source, target),
			item.LanguageId,
			item.EntryRule,
			Translate(new TextSpan(item.CompletionPosition, 0), source, target).Start,
			item.Expected)).ToArray();
	}

	TextSpan Translate(TextSpan span, TVersion source, TVersion target)
	{
		return _host.Translate(span, source, target);
	}

	async Task RetryAsync(TVersion snapshot, CancellationToken cancellationToken)
	{
		try
		{
			await _host.Scheduler.DelayAsync(EmbeddedGrammarSession.RetryDelayMilliseconds, cancellationToken).ConfigureAwait(false);

			if (_host.Current == snapshot)
				Schedule(snapshot);
		}
		catch (OperationCanceledException)
		{
		}
	}
}

/// <summary>The C# document's text as a Roslyn document of the solution, for an editor that holds the text itself.</summary>
static class EmbeddedGrammarDocuments
{
	/// <summary>The solution's document at <paramref name="filePath"/>, its text replaced, or null.</summary>
	public static Document? WithText(Solution solution, string? filePath, string text)
	{
		if (filePath is null)
			return null;

		var id       = solution.GetDocumentIdsWithFilePath(filePath).FirstOrDefault();
		var document = id is null ? null : solution.GetDocument(id);

		return document?.WithText(SourceText.From(text, Encoding.UTF8));
	}
}
