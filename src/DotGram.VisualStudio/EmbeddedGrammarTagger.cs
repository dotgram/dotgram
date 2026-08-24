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

		return result;
	}

	void Changed(ITextSnapshot snapshot) =>
		ClassificationChanged?.Invoke(
			this,
			new ClassificationChangedEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));

	static IClassificationType Type(IClassificationTypeRegistryService classifications, string name) =>
		classifications.GetClassificationType(name) ??
		throw new InvalidOperationException($"Visual Studio classification '{name}' is unavailable.");
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
	readonly ITextBuffer                _buffer;
	readonly VisualStudioWorkspace      _workspace;
	readonly ITextDocumentFactoryService _documents;
	readonly object                     _gate = new();

	CancellationTokenSource?          _cancellation;
	ITextSnapshot?                    _snapshot;
	ITextSnapshot?                    _retrySnapshot;
	int                               _retryCount;
	IReadOnlyList<HostClassification> _classifications = [];
	IReadOnlyList<HostDiagnostic>     _diagnostics     = [];
	IReadOnlyList<HostSymbolOccurrence> _symbols       = [];

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

	void BufferChanged(object sender, TextContentChangedEventArgs change) => Schedule(change.After);

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

		_ = AnalyzeAsync(snapshot, cancellationToken);
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
			var diagnostics     = analyses.SelectMany(static analysis => analysis.Diagnostics).ToArray();
			var symbols         = analyses.SelectMany(static analysis => analysis.Symbols).ToArray();
			var retry           = false;

			lock (_gate)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (_buffer.CurrentSnapshot != snapshot)
					return;

				_snapshot        = snapshot;
				_classifications = classifications;
				_diagnostics     = diagnostics;
				_symbols         = symbols;

				if (_retrySnapshot != snapshot)
				{
					_retrySnapshot = snapshot;
					_retryCount    = 0;
				}

				retry = analyses.Count == 0 &&
					snapshot.GetText().IndexOf("[Gram", StringComparison.Ordinal) >= 0 &&
					_retryCount++ < 5;
			}

			Changed?.Invoke(snapshot);

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
