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
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

[Export(typeof(ITaggerProvider))]
[ContentType("CSharp")]
[TagType(typeof(ClassificationTag))]
sealed class EmbeddedGrammarTaggerProvider : ITaggerProvider
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	[Import]
	IClassificationTypeRegistryService Classifications { get; set; } = null!;

	public ITagger<T>? CreateTagger<T>(ITextBuffer buffer) where T : ITag =>
		new EmbeddedGrammarTagger(
			EmbeddedGrammarBufferAnalysis.For(buffer, Workspace, Documents),
			Classifications) as ITagger<T>;
}

sealed class EmbeddedGrammarTagger : ITagger<ClassificationTag>
{
	readonly EmbeddedGrammarBufferAnalysis       _analysis;
	readonly Dictionary<GramSyntaxKind, IClassificationType> _types;

	public EmbeddedGrammarTagger(
		EmbeddedGrammarBufferAnalysis analysis,
		IClassificationTypeRegistryService classifications)
	{
		_analysis = analysis;
		_types    = new Dictionary<GramSyntaxKind, IClassificationType>
		{
			[GramSyntaxKind.Invalid]        = Type(classifications, "excluded code"),
			[GramSyntaxKind.Identifier]     = Type(classifications, "identifier"),
			[GramSyntaxKind.Number]         = Type(classifications, "number"),
			[GramSyntaxKind.Character]      = Type(classifications, "character"),
			[GramSyntaxKind.String]         = Type(classifications, "string"),
			[GramSyntaxKind.CharacterClass] = Type(classifications, "string"),
			[GramSyntaxKind.EmbeddedCode]   = Type(classifications, "code"),
			[GramSyntaxKind.Operator]       = Type(classifications, "operator"),
			[GramSyntaxKind.Punctuation]    = Type(classifications, "punctuation"),
		};

		_analysis.Changed += Changed;
	}

	public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

	public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
	{
		if (spans.Count == 0)
			yield break;

		var snapshot = spans[0].Snapshot;

		if (!_analysis.TryGet(snapshot, out var classifications, out _))
			yield break;

		foreach (var item in classifications)
		{
			var classified = new SnapshotSpan(snapshot, item.Span.Start, item.Span.Length);

			if (spans.IntersectsWith(classified))
				yield return new TagSpan<ClassificationTag>(
					classified,
					new ClassificationTag(_types[item.Kind]));
		}
	}

	void Changed(ITextSnapshot snapshot) =>
		TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(
			new SnapshotSpan(snapshot, 0, snapshot.Length)));

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
	IReadOnlyList<HostClassification> _classifications = [];
	IReadOnlyList<HostDiagnostic>     _diagnostics     = [];

	EmbeddedGrammarBufferAnalysis(
		ITextBuffer buffer,
		VisualStudioWorkspace workspace,
		ITextDocumentFactoryService documents)
	{
		_buffer    = buffer;
		_workspace = workspace;
		_documents = documents;

		_buffer.Changed += BufferChanged;
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

	void BufferChanged(object sender, TextContentChangedEventArgs change) => Schedule(change.After);

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

			lock (_gate)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (_buffer.CurrentSnapshot != snapshot)
					return;

				_snapshot        = snapshot;
				_classifications = classifications;
				_diagnostics     = diagnostics;
			}

			Changed?.Invoke(snapshot);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception exception) when (exception is not OutOfMemoryException)
		{
			// An editor extension must degrade to no tags rather than destabilize Visual Studio.
		}
	}
}
