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
		};

		_analysis.Changed += Changed;
	}

	public event EventHandler<ClassificationChangedEventArgs>? ClassificationChanged;

	public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
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
		var analysis = GramBufferAnalysis.For(buffer);
		GramClassifierProvider.Configure(analysis, buffer, _workspace, _documents);
		return new GramDiagnosticTagger(analysis) as ITagger<T>;
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
	CancellationTokenSource?     _resolveCancellation;
	StandaloneGrammarResolution  _resolution = StandaloneGrammarResolution.NotReady;
	int                          _retries;
	bool                         _configured;
	bool                         _closed;

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
		lock (_gate)
		{
			if (_configured)
				return;

			_configured   = true;
			_workspace    = workspace;
			_documents    = documents;
			_textDocument = document;
			_age.Start();
		}

		workspace.WorkspaceChanged    += WorkspaceChanged;
		documents.TextDocumentDisposed += TextDocumentDisposed;

		ScheduleResolve(0);
		_ = SettleAsync();
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
			context  = current ? _analysedContext : _resolution.Context;
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
	/// Compiled as the generator compiles it: with the host's <c>Lexical</c>, and with the
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
			? new GramAnalysisOptions { Own = tail.Length == 0 ? null : own.Length, Lexical = known.Lexical }
			: null;
		var whole   = GramLanguageService.Analyze(own + tail, options);

		return new StandaloneAnalysis(
			Project(whole, own.Length, suppressContextDiagnostics),
			tail.Length == 0 ? NoSymbols : whole.Symbols.Where(item => item.Position >= own.Length).ToArray());
	}

	/// <summary>
	/// Whether a change to the solution can change what this grammar is compiled with: its
	/// host, what the host includes, or how it is read.
	/// </summary>
	/// <remarks>
	/// The grammar's own text is not among them — it is the buffer's, and read from there —
	/// and nothing in a project that neither holds the grammar nor declares what it includes
	/// is either. Until the host is found, anything in the grammar's own project may be the
	/// change that makes it findable.
	/// </remarks>
	internal static bool Concerns(
		WorkspaceChangeEventArgs change,
		StandaloneGrammarResolution resolution,
		string filePath)
	{
		switch (change.Kind)
		{
			case WorkspaceChangeKind.SolutionAdded:
			case WorkspaceChangeKind.SolutionChanged:
			case WorkspaceChangeKind.SolutionRemoved:
			case WorkspaceChangeKind.SolutionCleared:
			case WorkspaceChangeKind.SolutionReloaded:
				return true;
		}

		if (change.ProjectId is not { } project)
			return true;

		if (change.Kind == WorkspaceChangeKind.AdditionalDocumentChanged &&
			change.DocumentId is { } edited &&
			string.Equals(
				change.NewSolution.GetAdditionalDocument(edited)?.FilePath,
				filePath,
				StringComparison.OrdinalIgnoreCase))
			return false;

		if (change.DocumentId is { } document && resolution.Documents.Contains(document))
			return true;

		if (resolution.State != StandaloneGrammarState.Resolved)
			return resolution.Projects.Contains(project) ||
				Holds(change.NewSolution, project, filePath) ||
				Holds(change.OldSolution, project, filePath);

		return resolution.Projects.Contains(project) &&
			change.Kind is not (
				WorkspaceChangeKind.DocumentChanged or
				WorkspaceChangeKind.AdditionalDocumentChanged or
				WorkspaceChangeKind.AnalyzerConfigDocumentChanged);
	}

	static bool Holds(Solution solution, ProjectId project, string filePath)
	{
		return solution.GetProject(project)?.AdditionalDocuments.Any(document =>
			string.Equals(document.FilePath, filePath, StringComparison.OrdinalIgnoreCase)) == true;
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
		StandaloneGrammarResolution resolution;
		string?                     filePath;

		lock (_gate)
		{
			if (_closed)
				return;

			resolution = _resolution;
			filePath   = _textDocument?.FilePath;
		}

		if (filePath is null || !Concerns(change, resolution, filePath))
			return;

		lock (_gate)
			_retries = 0;

		ScheduleResolve(ResolveDelayMilliseconds);
	}

	void TextDocumentDisposed(object sender, Microsoft.VisualStudio.Text.TextDocumentEventArgs e)
	{
		Workspace?                   workspace;
		ITextDocumentFactoryService? documents;

		lock (_gate)
		{
			if (_closed || e.TextDocument != _textDocument)
				return;

			_closed = true;
			_resolveCancellation?.Cancel();
			workspace = _workspace;
			documents = _documents;
		}

		if (workspace is not null)
			workspace.WorkspaceChanged -= WorkspaceChanged;

		if (documents is not null)
			documents.TextDocumentDisposed -= TextDocumentDisposed;
	}

	void ScheduleResolve(int delay)
	{
		CancellationToken cancellationToken;

		lock (_gate)
		{
			if (_closed)
				return;

			_resolveCancellation?.Cancel();
			_resolveCancellation?.Dispose();
			_resolveCancellation = new CancellationTokenSource();
			cancellationToken    = _resolveCancellation.Token;
		}

		_ = Task.Run(() => ResolveAsync(delay, cancellationToken));
	}

	async Task ResolveAsync(int delay, CancellationToken cancellationToken)
	{
		try
		{
			if (delay > 0)
				await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

			Workspace? workspace;
			string?    filePath;

			lock (_gate)
			{
				workspace = _workspace;
				filePath  = _textDocument?.FilePath;
			}

			if (workspace is null || filePath is null)
				return;

			StandaloneGrammarResolution resolution;

			try
			{
				resolution = await StandaloneGrammarInheritance.ResolveAsync(
					workspace.CurrentSolution,
					filePath,
					cancellationToken).ConfigureAwait(false);
			}
			catch (Exception exception) when (exception is not OperationCanceledException and not OutOfMemoryException)
			{
				// The project system mutates the solution while it is loading. A transient
				// snapshot failure is equivalent to the context not being ready yet.
				resolution = StandaloneGrammarResolution.NotReady;
			}

			bool reanalyse;
			var  retry = -1;

			lock (_gate)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var before = _resolution;

				_resolution = resolution;
				reanalyse   = before.State != resolution.State || !Same(before.Context, resolution.Context);

				if (reanalyse)
					_scheduledSnapshot = null;

				if (resolution.State == StandaloneGrammarState.NotReady && _retries < RetryCount)
					retry = RetryDelayMilliseconds << _retries++;
			}

			if (reanalyse)
				ScheduleAnalysis(_buffer.CurrentSnapshot, immediate: true);

			if (retry >= 0)
				ScheduleResolve(retry);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception exception) when (exception is not OutOfMemoryException)
		{
			ActivityLog.LogError("DotGram.VisualStudio", exception.ToString());
		}
	}

	/// <summary>Once the settling time is over, a grammar still waiting for its host is told about as it stands.</summary>
	async Task SettleAsync()
	{
		await Task.Delay(SettleMilliseconds).ConfigureAwait(false);

		lock (_gate)
		{
			if (_closed || _resolution.State != StandaloneGrammarState.NotReady)
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
		lock (_gate)
		{
			_document = _snapshot == change.Before && _document is not null
				? TranslateDocument(_document, change.Before, change.After)
				: EmptyDocument;
			_includedSymbols = NoSymbols;
			_snapshot = change.After;
		}

		Changed?.Invoke(change.After);
		ScheduleAnalysis(change.After, immediate: false);
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
					context = _resolution.Context;
					suppressContextDiagnostics =
						_configured &&
						_resolution.State == StandaloneGrammarState.NotReady &&
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
			Changed?.Invoke(snapshot);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception exception) when (exception is not OutOfMemoryException)
		{
			ActivityLog.LogError("DotGram.VisualStudio", exception.ToString());
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
