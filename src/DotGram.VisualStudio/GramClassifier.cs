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
			[GramSyntaxKind.Binding]        = Type(classifications, GramClassificationTypes.Identifier),
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

/// <summary>
/// A standalone <c>.gram</c> buffer's analysis session, one per buffer: the core session fed with the
/// buffer's edits and the solution's changes, and read back by every tagger and command.
/// </summary>
sealed class GramBufferAnalysis : IGramAnalysisHost<ITextSnapshot>
{
	readonly ITextBuffer                          _buffer;
	readonly GramAnalysisSession<ITextSnapshot>   _session;
	readonly object                               _gate = new();

	Workspace?                   _workspace;
	ITextDocumentFactoryService? _documents;
	ITextDocument?               _textDocument;
	bool                         _configured;
	bool                         _closed;

	GramBufferAnalysis(ITextBuffer buffer)
	{
		_buffer  = buffer;
		_session = new GramAnalysisSession<ITextSnapshot>(this);
		_buffer.Changed += BufferChanged;
	}

	public event Action<ITextSnapshot>? Changed
	{
		add => _session.Changed += value;
		remove => _session.Changed -= value;
	}

	public IIncludedGrammarStore Store => _session.Store;

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
		}

		if (document.FilePath is not { } path || !_session.Follow(path))
			return;

		workspace.WorkspaceChanged    += WorkspaceChanged;
		documents.TextDocumentDisposed += TextDocumentDisposed;
	}

	public GramDocument Document(ITextSnapshot snapshot)
	{
		return _session.Document(snapshot);
	}

	/// <summary>What the feature providers read of one version of the buffer.</summary>
	public GramStandaloneView View(ITextSnapshot snapshot)
	{
		return new GramStandaloneView(
			snapshot.GetText,
			snapshot.Length,
			Document(snapshot),
			position => _session.ExternalDefinition(snapshot, position),
			(name, definitionPosition) => _session.IncludedReferences(snapshot, name, definitionPosition));
	}

	/// <summary>
	/// Where a name at <paramref name="position"/> is defined, when that is in a grammar the
	/// host includes rather than in this buffer.
	/// </summary>
	public StandaloneDefinition? ExternalDefinition(ITextSnapshot snapshot, int position)
	{
		return _session.ExternalDefinition(snapshot, position);
	}

	void WorkspaceChanged(object sender, WorkspaceChangeEventArgs change)
	{
		_session.WorkspaceChanged(change);
	}

	void TextDocumentDisposed(object sender, Microsoft.VisualStudio.Text.TextDocumentEventArgs e)
	{
		Workspace?                   workspace;
		ITextDocumentFactoryService? documents;

		lock (_gate)
		{
			if (_closed || e.TextDocument != _textDocument)
				return;

			_closed   = true;
			workspace = _workspace;
			documents = _documents;
		}

		_session.Close();

		if (workspace is not null)
			workspace.WorkspaceChanged -= WorkspaceChanged;

		if (documents is not null)
			documents.TextDocumentDisposed -= TextDocumentDisposed;
	}

	void BufferChanged(object sender, TextContentChangedEventArgs change)
	{
		_session.Edited(change.Before, change.After);
	}

	IGramScheduler IGramAnalysisHost<ITextSnapshot>.Scheduler => GramScheduler.Instance;

	IIncludedGrammarStore IGramAnalysisHost<ITextSnapshot>.Store => TempDirectoryIncludedGrammarStore.Instance;

	ITextSnapshot IGramAnalysisHost<ITextSnapshot>.Current => _buffer.CurrentSnapshot;

	string? IGramAnalysisHost<ITextSnapshot>.FilePath
	{
		get
		{
			lock (_gate)
				return _textDocument?.FilePath;
		}
	}

	string IGramAnalysisHost<ITextSnapshot>.Text(ITextSnapshot version)
	{
		return version.GetText();
	}

	GramDocument IGramAnalysisHost<ITextSnapshot>.Translate(GramDocument document, ITextSnapshot from, ITextSnapshot to)
	{
		return TranslateDocument(document, from, to);
	}

	Task<StandaloneGrammarResolution> IGramAnalysisHost<ITextSnapshot>.ResolveAsync(string filePath, CancellationToken cancellationToken)
	{
		return StandaloneGrammarInheritance.ResolveAsync(Workspace().CurrentSolution, filePath, cancellationToken);
	}

	async Task<bool> IGramAnalysisHost<ITextSnapshot>.MayHostAsync(
		IReadOnlyList<DocumentId> documents,
		string filePath,
		CancellationToken cancellationToken)
	{
		var solution = Workspace().CurrentSolution;

		foreach (var id in documents)
			if (solution.GetDocument(id) is { } document &&
				await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false) is { } root &&
				StandaloneGrammarInheritance.MayHost(root, filePath))
				return true;

		return false;
	}

	async Task IGramAnalysisHost<ITextSnapshot>.RaiseAsync(Action raise, CancellationToken cancellationToken)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
		raise();
	}

	void IGramAnalysisHost<ITextSnapshot>.Report(Exception exception)
	{
		ActivityLog.LogError("DotGram.VisualStudio", exception.ToString());
	}

	Workspace Workspace()
	{
		lock (_gate)
			return _workspace ?? throw new InvalidOperationException("The grammar's host is looked for before the buffer follows a workspace.");
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
