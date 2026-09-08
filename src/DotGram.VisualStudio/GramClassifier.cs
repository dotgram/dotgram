using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
	public GramFilePathToContentTypeProvider(IContentTypeRegistryService contentTypes) =>
		_contentType = contentTypes.GetContentType(GramContentType.Name) ??
			throw new InvalidOperationException($"Visual Studio content type '{GramContentType.Name}' is unavailable.");

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
			analysis.ConfigureInheritance(workspace, document.FilePath);
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

	void Changed(ITextSnapshot snapshot) =>
		ClassificationChanged?.Invoke(
			this,
			new ClassificationChangedEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));

	static IClassificationType Type(IClassificationTypeRegistryService classifications, string name) =>
		classifications.GetClassificationType(name) ??
		throw new InvalidOperationException($"Visual Studio classification '{name}' is unavailable.");
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
			var tagged = Span(snapshot, diagnostic.Position, diagnostic.Length);

			if (spans.IntersectsWith(tagged))
				yield return new TagSpan<ErrorTag>(
					tagged,
					new ErrorTag(
						ErrorType(diagnostic.Severity),
						$"{diagnostic.Id}: {diagnostic.Message}"));
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
		_                            => PredefinedErrorTypeNames.Information,
	};
}

sealed class GramBufferAnalysis
{
	readonly ITextBuffer _buffer;
	readonly object      _gate = new();

	ITextSnapshot? _snapshot;
	GramDocument?  _document;
	string?        _inheritedText;
	bool           _inheritanceStarted;

	GramBufferAnalysis(ITextBuffer buffer)
	{
		_buffer = buffer;
		_buffer.Changed += BufferChanged;
	}

	public event Action<ITextSnapshot>? Changed;

	public static GramBufferAnalysis For(ITextBuffer buffer) =>
		buffer.Properties.GetOrCreateSingletonProperty(() => new GramBufferAnalysis(buffer));

	public void ConfigureInheritance(Workspace workspace, string filePath)
	{
		lock (_gate)
		{
			if (_inheritanceStarted)
				return;

			_inheritanceStarted = true;
		}

		_ = LoadInheritanceAsync(workspace, filePath);
	}

	public GramDocument Document(ITextSnapshot snapshot)
	{
		lock (_gate)
		{
			if (_snapshot == snapshot && _document is not null)
				return _document;

			_snapshot = snapshot;
			var own = snapshot.GetText();
			_document = Project(
				GramLanguageService.Analyze(own + (_inheritedText ?? "")),
				own.Length);

			return _document;
		}
	}

	async Task LoadInheritanceAsync(Workspace workspace, string filePath)
	{
		string? inherited;
		try
		{
			inherited = await StandaloneGrammarInheritance.ResolveAsync(
				workspace.CurrentSolution,
				filePath,
				CancellationToken.None).ConfigureAwait(false);
		}
		catch (Exception exception) when (exception is not OutOfMemoryException)
		{
			// Project context improves inherited grammars, but a project-system race must
			// never take classification for the standalone document down with it.
			return;
		}

		if (string.IsNullOrEmpty(inherited))
			return;

		lock (_gate)
		{
			_inheritedText = inherited;
			_snapshot      = null;
			_document      = null;
		}

		Changed?.Invoke(_buffer.CurrentSnapshot);
	}

	static GramDocument Project(GramDocument document, int length) =>
		new(
			document.Classifications.Where(item => item.Position < length).ToArray(),
			document.Diagnostics.Where(item => item.Position < length).ToArray(),
			document.Symbols.Where(item => item.Position < length).ToArray(),
			document.Braces.Where(item => item.OpenPosition < length && item.ClosePosition < length).ToArray(),
			document.FoldingRanges.Where(item => item.Position < length).ToArray(),
			document.DocumentSymbols.Where(item => item.Position < length).ToArray(),
			document.PublishedApis.Where(item => item.Position < length).ToArray());

	void BufferChanged(object sender, TextContentChangedEventArgs change)
	{
		lock (_gate)
		{
			_snapshot = null;
			_document = null;
		}

		Changed?.Invoke(change.After);
	}
}
