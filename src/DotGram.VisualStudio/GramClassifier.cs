using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;

namespace DotGram.VisualStudio;

static class GramContentType
{
	public const string Name = "dotgram";

	#pragma warning disable CS0414 // MEF discovers and supplies these exported definitions.

	[Export]
	[Name(Name)]
	// "code" activates Visual Studio's generic LSP data-tip provider. A standalone
	// .gram buffer has no Roslyn/LSP document, so every hover is logged as a failed
	// textDocument/_vs_dataTipRange request. DotGram exports its editor features
	// explicitly and only needs the regular text editor foundation here.
	[BaseDefinition("text")]
	static readonly ContentTypeDefinition Definition = null!;

	[Export]
	[ContentType(Name)]
	[FileExtension(".gram")]
	static readonly FileExtensionToContentTypeDefinition Extension = null!;

	#pragma warning restore CS0414
}

[Export(typeof(IFilePathToContentTypeProvider))]
[Name("DotGram file path")]
[FileExtension(".gram")]
sealed class GramFilePathToContentTypeProvider : IFilePathToContentTypeProvider
{
	readonly IContentType _contentType;

	[ImportingConstructor]
	public GramFilePathToContentTypeProvider(IContentTypeRegistryService contentTypes)
	{
		_contentType = contentTypes.GetContentType(GramContentType.Name) ??
			throw new InvalidOperationException($"Visual Studio content type '{GramContentType.Name}' is unavailable.");
	}

	public bool TryGetContentTypeForFilePath(string filePath, out IContentType contentType)
	{
		contentType = _contentType;

		return true;
	}
}

[Export(typeof(IClassifierProvider))]
[ContentType(GramContentType.Name)]
sealed class GramClassifierProvider : IClassifierProvider
{
	readonly IClassificationTypeRegistryService _classifications;
	readonly VisualStudioWorkspace _workspace;
	readonly ITextDocumentFactoryService _documents;

	[ImportingConstructor]
	public GramClassifierProvider(
		IClassificationTypeRegistryService classifications,
		VisualStudioWorkspace workspace,
		ITextDocumentFactoryService documents)
	{
		_classifications = classifications;
		_workspace       = workspace;
		_documents       = documents;
	}

	public IClassifier GetClassifier(ITextBuffer buffer)
	{
		return EditorGuard.Run<IClassifier>("Classifier", null!, () => Create(buffer));
	}

	IClassifier Create(ITextBuffer buffer)
	{
		var analysis = GramBufferAnalysis.For(buffer);
		Configure(analysis, buffer, _workspace, _documents);
		return buffer.Properties.GetOrCreateSingletonProperty(() =>
			new GramClassifier(analysis, _classifications));
	}

	internal static void Configure(
		GramBufferAnalysis analysis,
		ITextBuffer buffer,
		VisualStudioWorkspace workspace,
		ITextDocumentFactoryService documents)
	{
		if (documents.TryGetTextDocument(buffer, out var document) && document.FilePath is not null)
			analysis.ConfigureInheritance(workspace, documents, document);
	}
}

sealed class GramClassifier : IClassifier
{
	readonly GramBufferAnalysis                  _analysis;
	readonly Dictionary<GramSyntaxKind, IClassificationType> _types;

	public GramClassifier(GramBufferAnalysis analysis, IClassificationTypeRegistryService classifications)
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
			[GramSyntaxKind.Binding]        = Type(classifications, GramClassificationTypes.Identifier),
		};

		_analysis.Changed += Changed;
	}

	public event EventHandler<ClassificationChangedEventArgs>? ClassificationChanged;

	public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
	{
		return EditorGuard.Run<IList<ClassificationSpan>>("Classification", [], () => Spans(span));
	}

	List<ClassificationSpan> Spans(SnapshotSpan span)
	{
		var document = _analysis.Document(span.Snapshot);
		var result   = new List<ClassificationSpan>();

		foreach (var item in document.Classifications)
		{
			var classified = new SnapshotSpan(span.Snapshot, item.Position, item.Length);

			if (classified.IntersectsWith(span))
				result.Add(new ClassificationSpan(classified, _types[item.Kind]));
		}

		return result;
	}

	void Changed(ITextSnapshot snapshot)
	{
		ClassificationChanged?.Invoke(
			this,
			new ClassificationChangedEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));
	}

	static IClassificationType Type(IClassificationTypeRegistryService classifications, string name)
	{
		return classifications.GetClassificationType(name) ??
		throw new InvalidOperationException($"Visual Studio classification '{name}' is unavailable.");
	}
}

[Export(typeof(ITaggerProvider))]
[ContentType(GramContentType.Name)]
[TagType(typeof(ErrorTag))]
sealed class GramDiagnosticTaggerProvider : ITaggerProvider
{
	readonly VisualStudioWorkspace _workspace;
	readonly ITextDocumentFactoryService _documents;

	[ImportingConstructor]
	public GramDiagnosticTaggerProvider(
		VisualStudioWorkspace workspace,
		ITextDocumentFactoryService documents)
	{
		_workspace = workspace;
		_documents = documents;
	}

	public ITagger<T>? CreateTagger<T>(ITextBuffer buffer) where T : ITag
	{
		return EditorGuard.Run<ITagger<T>?>("Diagnostic tagger", null, () =>
		{
			var analysis = GramBufferAnalysis.For(buffer);
			GramClassifierProvider.Configure(analysis, buffer, _workspace, _documents);
			return new GramDiagnosticTagger(analysis) as ITagger<T>;
		});
	}
}

sealed class GramDiagnosticTagger : ITagger<ErrorTag>
{
	readonly GramBufferAnalysis _analysis;

	public GramDiagnosticTagger(GramBufferAnalysis analysis)
	{
		_analysis = analysis;
		_analysis.Changed += Changed;
	}

	public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

	public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
	{
		return EditorGuard.Enumerate("Diagnostic tags", Tags(spans));
	}

	IEnumerable<ITagSpan<ErrorTag>> Tags(NormalizedSnapshotSpanCollection spans)
	{
		if (spans.Count == 0)
			yield break;

		var snapshot = spans[0].Snapshot;

		foreach (var diagnostic in _analysis.Document(snapshot).Diagnostics)
		{
			var (position, length) = GramDiagnosticText.Span(diagnostic.Position, diagnostic.Length, snapshot.Length);
			var tagged = new SnapshotSpan(snapshot, position, length);

			if (spans.IntersectsWith(tagged))
				yield return new TagSpan<ErrorTag>(
					tagged,
					new ErrorTag(
						ErrorType(diagnostic.Severity),
						GramDiagnosticText.Format(diagnostic)));
		}
	}

	void Changed(ITextSnapshot snapshot)
	{
		TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(
			new SnapshotSpan(snapshot, 0, snapshot.Length)));
	}

	static string ErrorType(GramSeverity severity)
	{
		return severity switch
		{
			GramSeverity.Error => PredefinedErrorTypeNames.SyntaxError,
			GramSeverity.Warning => PredefinedErrorTypeNames.Warning,
			_ => PredefinedErrorTypeNames.Information,
		};
	}
}

sealed class GramBufferAnalysis
{
	const int AnalysisDelayMilliseconds = 150;

	/// <summary>
	/// How long after the last change that concerns the grammar its host is looked for again:
	/// a build, a branch switch or a solution load is a burst of changes, answered once.
	/// </summary>
	const int ResolveDelayMilliseconds = 500;

	/// <summary>
	/// The longest a stream of such changes that never pauses for <see cref="ResolveDelayMilliseconds"/>
	/// keeps the host from being looked for.
	/// </summary>
	const int ResolveDeferralMilliseconds = 2000;

	/// <summary>
	/// While the host cannot be asked yet, it is asked again this long after, doubling, a few
	/// times; every change to the project starts the count again. Nothing gives up for good.
	/// </summary>
	const int RetryDelayMilliseconds = 1000;
	const int RetryCount             = 5;

	/// <summary>
	/// How long a grammar whose host is not settled stays quiet about what depends on its
	/// includes. Past it the grammar is told about as it stands, and is told again the moment
	/// the host is found.
	/// </summary>
	const int SettleMilliseconds = 20000;

	static readonly GramDocument EmptyDocument = new([], [], [], [], [], [], []);
	static readonly IReadOnlyList<GramSymbolOccurrence> NoSymbols = [];

	readonly ITextBuffer   _buffer;
	readonly object        _gate         = new();
	readonly SemaphoreSlim _analysisGate = new(1, 1);
	readonly Stopwatch     _age          = new();

	ITextSnapshot?                      _snapshot;
	GramDocument?                       _document;
	IReadOnlyList<GramSymbolOccurrence> _includedSymbols = NoSymbols;
	StandaloneGrammarContext?           _analysedContext;
	ITextSnapshot?                      _scheduledSnapshot;
	CancellationTokenSource?            _analysisCancellation;

	Workspace?                   _workspace;
	ITextDocumentFactoryService? _documents;
	ITextDocument?               _textDocument;
	StandaloneGrammarHostTracker? _host;
	bool                          _configured;
	bool                          _copy;
	bool                          _closed;

	GramBufferAnalysis(ITextBuffer buffer)
	{
		_buffer = buffer;
		_buffer.Changed += BufferChanged;
	}

	public event Action<ITextSnapshot>? Changed;

	public static GramBufferAnalysis For(ITextBuffer buffer)
	{
		return buffer.Properties.GetOrCreateSingletonProperty(() => new GramBufferAnalysis(buffer));
	}

	/// <summary>
	/// Starts following the grammar's host: found now, and found again whenever the solution
	/// changes in a way that concerns it, for as long as the document is open.
	/// </summary>
	public void ConfigureInheritance(
		Workspace workspace,
		ITextDocumentFactoryService documents,
		ITextDocument document)
	{
		StandaloneGrammarHostTracker host;

		lock (_gate)
		{
			if (_configured)
				return;

			_configured = true;

			// A read-only copy of what an assembly carries belongs to no project and includes
			// what it includes from somewhere out of reach: it is read as it stands, quiet about
			// what depends on its includes, and nothing is looked for.
			if (document.FilePath is { } path && IncludedGrammarView.IsCopy(path))
			{
				_copy = true;
				return;
			}

			_workspace    = workspace;
			_documents    = documents;
			_textDocument = document;
			_host         = host = Track(workspace);
			_age.Start();
		}

		workspace.WorkspaceChanged    += WorkspaceChanged;
		documents.TextDocumentDisposed += TextDocumentDisposed;

		host.Follow();
		_ = EditorGuard.RunAsync("Grammar settling", SettleAsync, CancellationToken.None);
	}

	public GramDocument Document(ITextSnapshot snapshot)
	{
		lock (_gate)
		{
			if (_snapshot == snapshot && _document is not null)
				return _document;
		}

		ScheduleAnalysis(snapshot, immediate: true);
		return EmptyDocument;
	}

	/// <summary>
	/// Where a name at <paramref name="position"/> is defined, when that is in a grammar the
	/// host includes rather than in this buffer.
	/// </summary>
	public StandaloneDefinition? ExternalDefinition(ITextSnapshot snapshot, int position)
	{
		GramDocument?             document;
		StandaloneGrammarContext? context;

		lock (_gate)
		{
			var current = _snapshot == snapshot && _document is not null;

			document = current ? _document : null;
			context  = current ? _analysedContext : Resolution.Context;
		}

		return context is null
			? null
			: ExternalDefinition(snapshot.GetText(), context.Value, document?.Symbols ?? NoSymbols, position);
	}

	internal static StandaloneDefinition? ExternalDefinition(
		string text,
		StandaloneGrammarContext context,
		IReadOnlyList<GramSymbolOccurrence> symbols,
		int position)
	{
		// A use the binder resolved past the end of the text: into an included grammar, by a
		// `using`, a qualified name or a name qualified more than once.
		foreach (var symbol in symbols)
			if (symbol.Position <= position && position < symbol.Position + symbol.Length)
				return symbol.DefinitionPosition >= text.Length &&
					context.Locate(symbol.DefinitionPosition, text.Length) is { } location
						? new StandaloneDefinition(symbol.Position, symbol.Length, location)
						: null;

		// The included grammar's own name, as `using Sql92;` and `Sql92.Word` write it.
		if (text.Length == 0)
			return null;

		var bounded = Math.Max(0, Math.Min(position, text.Length - 1));
		var start   = bounded;
		var end     = bounded;

		while (start > 0 && IsIdentifier(text[start - 1]))
			start--;

		while (end < text.Length && IsIdentifier(text[end]))
			end++;

		if (start == end)
			return null;

		var name = text.Substring(start, end - start);

		foreach (var included in context.Included)
			if (name == included.Name)
				return new StandaloneDefinition(start, end - start, new StandaloneLocation(included, 0, 0, 0));

		return null;
	}

	/// <summary>
	/// The uses of a rule inside the grammars the host includes, and its definition where it
	/// is one of theirs: what Find All References adds to what is in the buffer.
	/// </summary>
	public IReadOnlyList<StandaloneReference> IncludedReferences(
		ITextSnapshot snapshot,
		string name,
		int definitionPosition)
	{
		IReadOnlyList<GramSymbolOccurrence> included;
		StandaloneGrammarContext?           context;

		lock (_gate)
		{
			if (_snapshot != snapshot || _document is null)
				return [];

			included = _includedSymbols;
			context  = _analysedContext;
		}

		return context is null
			? []
			: IncludedReferences(included, context.Value, snapshot.Length, name, definitionPosition);
	}

	internal static IReadOnlyList<StandaloneReference> IncludedReferences(
		IReadOnlyList<GramSymbolOccurrence> included,
		StandaloneGrammarContext context,
		int ownLength,
		string name,
		int definitionPosition)
	{
		var result = new List<StandaloneReference>();

		foreach (var symbol in included)
			if (symbol.Name == name &&
				symbol.DefinitionPosition == definitionPosition &&
				context.Locate(symbol.Position, ownLength) is { } location)
				result.Add(new StandaloneReference(location, symbol.Length, symbol.IsDefinition));

		return result;
	}

	/// <summary>
	/// A grammar read with what it includes spliced on after it, and told about only as far as
	/// its own text goes. Until the includes are known, what depends on them is not said: a
	/// name from an included grammar is not missing, it has not been looked up yet.
	/// </summary>
	/// <remarks>
	/// Compiled as the generator compiles it: with the host's options, and with the
	/// own text's length where something is included, so that a <c>parse</c> in an included
	/// grammar is that grammar's and not this one's.
	/// </remarks>
	internal static StandaloneAnalysis Analyze(
		string own,
		StandaloneGrammarContext? context,
		bool suppressContextDiagnostics)
	{
		var tail    = context?.AnalysisTail ?? "";
		var options = context is { } known
			? new GramAnalysisOptions
			{
				Own     = tail.Length == 0 ? null : own.Length,
				Cuts    = tail.Length == 0
					? null
					: GrammarSplice.Cuts(
						own + tail, known.Included.Select(item => (own.Length + item.Start, item.Text.Length))),
				Lexical = known.Lexical,
				Host    = known.Host,
				Build   = known.Build,
			}
			: null;
		var whole   = GramLanguageService.Analyze(own + tail, options);

		return new StandaloneAnalysis(
			Project(whole, own.Length, suppressContextDiagnostics),
			tail.Length == 0 ? NoSymbols : whole.Symbols.Where(item => item.Position >= own.Length).ToArray());
	}

	/// <summary>
	/// Whether a change to the solution can change what this grammar is compiled with
	/// (<see cref="StandaloneGrammarChange.Concerns"/>).
	/// </summary>
	internal static StandaloneGrammarConcern Concerns(
		WorkspaceChangeEventArgs change,
		StandaloneGrammarResolution resolution,
		string filePath)
	{
		return StandaloneGrammarChange.Of(change, filePath).Concerns(resolution);
	}

	static bool IsIdentifier(char character)
	{
		return character == '_' || char.IsLetterOrDigit(character);
	}

	static GramDocument Project(GramDocument document, int length, bool suppressContextDiagnostics)
	{
		return new(
			document.Classifications.Where(item => item.Position < length).ToArray(),
			document.Diagnostics.Where(item =>
				item.Position < length &&
				(!suppressContextDiagnostics || IsSyntaxDiagnostic(item.Id))).ToArray(),
			document.Symbols.Where(item => item.Position < length).ToArray(),
			document.Braces.Where(item => item.OpenPosition < length && item.ClosePosition < length).ToArray(),
			document.FoldingRanges.Where(item => item.Position < length).ToArray(),
			document.DocumentSymbols.Where(item => item.Position < length).ToArray(),
			document.PublishedApis.Where(item => item.Position < length).ToArray());
	}

	static bool IsSyntaxDiagnostic(string id)
	{
		return id.StartsWith("GRAM1", StringComparison.Ordinal) ||
		id.StartsWith("GRAM2", StringComparison.Ordinal);
	}

	void WorkspaceChanged(object sender, WorkspaceChangeEventArgs change)
	{
		EditorGuard.Run("Grammar workspace change", () => WorkspaceChanged(change));
	}

	void WorkspaceChanged(WorkspaceChangeEventArgs change)
	{
		StandaloneGrammarHostTracker? host;

		lock (_gate)
			host = _closed ? null : _host;

		host?.WorkspaceChanged(change);
	}

	void TextDocumentDisposed(object sender, Microsoft.VisualStudio.Text.TextDocumentEventArgs e)
	{
		EditorGuard.Run("Grammar document close", () => DocumentDisposed(e));
	}

	void DocumentDisposed(Microsoft.VisualStudio.Text.TextDocumentEventArgs e)
	{
		Workspace?                   workspace;
		ITextDocumentFactoryService? documents;

		lock (_gate)
		{
			if (_closed || e.TextDocument != _textDocument)
				return;

			_closed = true;
			_host?.Close();
			workspace = _workspace;
			documents = _documents;
		}

		if (workspace is not null)
			workspace.WorkspaceChanged -= WorkspaceChanged;

		if (documents is not null)
			documents.TextDocumentDisposed -= TextDocumentDisposed;
	}

	/// <summary>How the grammar's host stands now.</summary>
	StandaloneGrammarResolution Resolution => _host?.Resolution ?? StandaloneGrammarResolution.NotReady;

	/// <summary>The grammar's host in this workspace, looked for again as the solution changes.</summary>
	StandaloneGrammarHostTracker Track(Workspace workspace)
	{
		return new StandaloneGrammarHostTracker(
			new StandaloneGrammarHostServices(
				FilePath: () =>
				{
					lock (_gate)
						return _textDocument?.FilePath;
				},
				ResolveAsync: (filePath, cancellationToken) =>
					StandaloneGrammarInheritance.ResolveAsync(workspace.CurrentSolution, filePath, cancellationToken),
				MayHostAsync: (documents, filePath, cancellationToken) =>
					AnyMayHostAsync(workspace.CurrentSolution, documents, filePath, cancellationToken),
				Now: static () => Stopwatch.GetTimestamp() * 1000 / Stopwatch.Frequency,
				DelayAsync: Task.Delay,
				Run: static work => _ = Task.Run(work),
				Resolved: Resolved,
				Recoverable: EditorGuard.Recoverable,
				Report: static exception => EditorGuard.Log("Grammar host resolution", exception)),
			ResolveDelayMilliseconds,
			RetryDelayMilliseconds,
			RetryCount,
			ResolveDeferralMilliseconds);
	}

	/// <summary>The host was looked for: what depends on it is analysed again where it changed.</summary>
	void Resolved(StandaloneGrammarResolution before, StandaloneGrammarResolution after)
	{
		if (before.State == after.State && Same(before.Context, after.Context))
			return;

		lock (_gate)
			_scheduledSnapshot = null;

		ScheduleAnalysis(_buffer.CurrentSnapshot, immediate: true);
	}

	static async Task<bool> AnyMayHostAsync(
		Solution solution,
		IReadOnlyList<DocumentId> documents,
		string filePath,
		CancellationToken cancellationToken)
	{
		foreach (var id in documents)
			if (solution.GetDocument(id) is { } document &&
				await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false) is { } root &&
				StandaloneGrammarInheritance.MayHost(root, filePath))
				return true;

		return false;
	}

	/// <summary>Once the settling time is over, a grammar still waiting for its host is told about as it stands.</summary>
	async Task SettleAsync()
	{
		await Task.Delay(SettleMilliseconds).ConfigureAwait(false);

		lock (_gate)
		{
			if (_closed || Resolution.State != StandaloneGrammarState.NotReady)
				return;

			_scheduledSnapshot = null;
		}

		ScheduleAnalysis(_buffer.CurrentSnapshot, immediate: true);
	}

	static bool Same(StandaloneGrammarContext? left, StandaloneGrammarContext? right)
	{
		return left is { } one && right is { } other
			? one.SameAs(other)
			: left is null && right is null;
	}

	void BufferChanged(object sender, TextContentChangedEventArgs change)
	{
		EditorGuard.Run("Grammar buffer change", () => BufferChanged(change));
	}

	void BufferChanged(TextContentChangedEventArgs change)
	{
		lock (_gate)
		{
			_document = _snapshot == change.Before && _document is not null
				? TranslateDocument(_document, change.Before, change.After)
				: EmptyDocument;
			_includedSymbols = NoSymbols;
			_snapshot = change.After;
		}

		// Analysis is asked for first: a handler that fails must not leave the buffer without it,
		// with the emptied translated document standing in for an analysis until the next edit.
		ScheduleAnalysis(change.After, immediate: false);
		EditorGuard.Notify("Grammar buffer change notification", Changed, change.After);
	}

	void ScheduleAnalysis(ITextSnapshot snapshot, bool immediate)
	{
		CancellationToken cancellationToken;
		lock (_gate)
		{
			if (_scheduledSnapshot == snapshot)
				return;

			_analysisCancellation?.Cancel();
			_analysisCancellation?.Dispose();
			_analysisCancellation = new CancellationTokenSource();
			_scheduledSnapshot = snapshot;
			cancellationToken = _analysisCancellation.Token;
		}

		_ = Task.Run(() => AnalyzeAsync(snapshot, immediate, cancellationToken));
	}

	async Task AnalyzeAsync(
		ITextSnapshot snapshot,
		bool immediate,
		CancellationToken cancellationToken)
	{
		try
		{
			if (!immediate)
				await Task.Delay(AnalysisDelayMilliseconds, cancellationToken).ConfigureAwait(false);
			await _analysisGate.WaitAsync(cancellationToken).ConfigureAwait(false);

			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				StandaloneGrammarContext? context;
				bool suppressContextDiagnostics;
				lock (_gate)
				{
					context = Resolution.Context;
					suppressContextDiagnostics =
						_copy ||
						_configured &&
						Resolution.State == StandaloneGrammarState.NotReady &&
						_age.ElapsedMilliseconds < SettleMilliseconds;
				}

				var analysis = Analyze(snapshot.GetText(), context, suppressContextDiagnostics);

				lock (_gate)
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (_buffer.CurrentSnapshot != snapshot)
						return;

					_snapshot        = snapshot;
					_document        = analysis.Document;
					_includedSymbols = analysis.Included;
					_analysedContext = context;
				}
			}
			finally
			{
				_analysisGate.Release();
			}

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
			EditorGuard.Notify("Grammar analysis notification", Changed, snapshot);
		}
		catch (OperationCanceledException exception) when (EditorGuard.Canceled(exception, cancellationToken))
		{
		}
		catch (Exception exception) when (EditorGuard.Recoverable(exception))
		{
			EditorGuard.Log("Grammar analysis", exception);
		}
	}

	static GramDocument TranslateDocument(
		GramDocument document,
		ITextSnapshot source,
		ITextSnapshot target)
	{
		return new(
			document.Classifications.Select(item =>
			{
				var span = Translate(item.Position, item.Length, source, target);
				int? definition = item.DefinitionPosition is int position && position < source.Length
					? Translate(position, 0, source, target).Start
					: null;
				return new GramClassifiedSpan(
					span.Start,
					span.Length,
					item.Kind,
					item.QuickInfo,
					definition,
					item.RuleSignature,
					item.RuleParameterCount,
					item.SymbolKind);
			}).ToArray(),
			[], [], [], [], [], []);
	}

	static Span Translate(
		int position,
		int length,
		ITextSnapshot source,
		ITextSnapshot target)
	{
		var translated = new SnapshotSpan(source, position, length)
			.TranslateTo(target, SpanTrackingMode.EdgeExclusive);
		return new Span(translated.Start.Position, translated.Length);
	}
}

/// <summary>A standalone grammar's analysis, and the symbols of what it includes kept for Find All References.</summary>
readonly record struct StandaloneAnalysis(GramDocument Document, IReadOnlyList<GramSymbolOccurrence> Included);

/// <summary>A name in the buffer whose definition is in an included grammar.</summary>
readonly record struct StandaloneDefinition(int Position, int Length, StandaloneLocation Target);

/// <summary>A use of a rule in an included grammar, or its definition there.</summary>
readonly record struct StandaloneReference(StandaloneLocation Location, int Length, bool IsDefinition);
