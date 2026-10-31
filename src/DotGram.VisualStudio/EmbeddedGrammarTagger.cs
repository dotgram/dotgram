using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
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

	public IClassifier GetClassifier(ITextBuffer buffer)
	{
		return buffer.Properties.GetOrCreateSingletonProperty(() =>
			new EmbeddedGrammarClassifier(
				EmbeddedGrammarBufferAnalysis.For(buffer, Workspace, Documents),
				Classifications));
	}
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
			[GramSyntaxKind.Binding]        = Type(classifications, GramClassificationTypes.Identifier),
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

	static Dictionary<string, IClassificationType> DslTypes(IClassificationTypeRegistryService types)
	{
		return new()
		{
			["Keyword"] = Type(types, GramClassificationTypes.Keyword),
			["Identifier"] = Type(types, GramClassificationTypes.Identifier),
			["Type"] = Type(types, GramClassificationTypes.DslType),
			["Variable"] = Type(types, GramClassificationTypes.DslVariable),
			["Function"] = Type(types, GramClassificationTypes.DslFunction),
			["Method"] = Type(types, GramClassificationTypes.DslFunction),
			["Property"] = Type(types, GramClassificationTypes.DslProperty),
			["Number"] = Type(types, GramClassificationTypes.Number),
			["String"] = Type(types, GramClassificationTypes.Literal),
			["Comment"] = Type(types, GramClassificationTypes.Comment),
			["Operator"] = Type(types, GramClassificationTypes.Operator),
			["Punctuation"] = Type(types, GramClassificationTypes.Punctuation),
			["Namespace"] = Type(types, GramClassificationTypes.DslNamespace),
			["Parameter"] = Type(types, GramClassificationTypes.DslParameter),
			["Label"] = Type(types, GramClassificationTypes.DslLabel),
		};
	}
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

	public ITagger<T>? CreateTagger<T>(ITextBuffer buffer) where T : ITag
	{
		return new EmbeddedGrammarDiagnosticTagger(
			EmbeddedGrammarBufferAnalysis.For(buffer, Workspace, Documents)) as ITagger<T>;
	}
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
			var (position, length) = GramDiagnosticText.Span(item.Span.Start, item.Span.Length, snapshot.Length);
			var tagged = new SnapshotSpan(snapshot, position, length);

			if (spans.IntersectsWith(tagged))
				yield return new TagSpan<ErrorTag>(
					tagged,
					new ErrorTag(
						ErrorType(item.Diagnostic.Severity),
						GramDiagnosticText.Format(item.Diagnostic)));
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
/// A C# buffer's embedded-grammar analysis session, one per buffer: the core session fed with the
/// buffer's edits and the solution's changes, and read back by every tagger and command.
/// </summary>
sealed class EmbeddedGrammarBufferAnalysis : IEmbeddedGrammarHost<ITextSnapshot>
{
	readonly ITextBuffer                          _buffer;
	readonly VisualStudioWorkspace                _workspace;
	readonly ITextDocumentFactoryService          _documents;
	readonly EmbeddedGrammarSession<ITextSnapshot> _session;

	EmbeddedGrammarBufferAnalysis(
		ITextBuffer buffer,
		VisualStudioWorkspace workspace,
		ITextDocumentFactoryService documents)
	{
		_buffer    = buffer;
		_workspace = workspace;
		_documents = documents;

		// The session before the events that feed it: a solution change may arrive on another
		// thread the moment the handler is attached.
		_session = new EmbeddedGrammarSession<ITextSnapshot>(this);

		_buffer.Changed += BufferChanged;
		_workspace.WorkspaceChanged += WorkspaceChanged;
	}

	public event Action<ITextSnapshot>? Changed
	{
		add => _session.Changed += value;
		remove => _session.Changed -= value;
	}

	public static EmbeddedGrammarBufferAnalysis For(
		ITextBuffer buffer,
		VisualStudioWorkspace workspace,
		ITextDocumentFactoryService documents)
	{
		return buffer.Properties.GetOrCreateSingletonProperty(() =>
			new EmbeddedGrammarBufferAnalysis(buffer, workspace, documents));
	}

	/// <summary>What the feature providers read of one version of the buffer.</summary>
	public EmbeddedGrammarView View(ITextSnapshot snapshot)
	{
		return _session.View(snapshot);
	}

	public bool TryGet(
		ITextSnapshot snapshot,
		out IReadOnlyList<HostClassification> classifications,
		out IReadOnlyList<HostDiagnostic> diagnostics)
	{
		var view = _session.View(snapshot);

		classifications = view.Classifications;
		diagnostics     = view.Diagnostics;

		return view.Current;
	}

	public bool TryGetDslClassifications(
		ITextSnapshot snapshot,
		out IReadOnlyList<HostDslClassification> classifications)
	{
		var view = _session.View(snapshot);

		classifications = view.DslClassifications;

		return view.Current;
	}

	public bool TryGetStructure(
		ITextSnapshot snapshot,
		out IReadOnlyList<HostBracePair> braces,
		out IReadOnlyList<HostFoldingRange> foldingRanges)
	{
		var view = _session.View(snapshot);

		braces        = view.Braces;
		foldingRanges = view.FoldingRanges;

		return view.Current;
	}

	public bool TryGetDocumentSymbols(
		ITextSnapshot snapshot,
		out IReadOnlyList<HostDocumentSymbol> symbols)
	{
		var view = _session.View(snapshot);

		symbols = view.DocumentSymbols;

		return view.Current;
	}

	void BufferChanged(object sender, TextContentChangedEventArgs change)
	{
		_session.Edited(change.Before, change.After);
	}

	void WorkspaceChanged(object sender, WorkspaceChangeEventArgs change)
	{
		_session.WorkspaceChanged(change);
	}

	IGramScheduler IEmbeddedGrammarHost<ITextSnapshot>.Scheduler => GramScheduler.Instance;

	ITextSnapshot IEmbeddedGrammarHost<ITextSnapshot>.Current => _buffer.CurrentSnapshot;

	string? IEmbeddedGrammarHost<ITextSnapshot>.FilePath =>
		_documents.TryGetTextDocument(_buffer, out var textDocument) ? textDocument.FilePath : null;

	string IEmbeddedGrammarHost<ITextSnapshot>.Text(ITextSnapshot version)
	{
		return version.GetText();
	}

	int IEmbeddedGrammarHost<ITextSnapshot>.Length(ITextSnapshot version)
	{
		return version.Length;
	}

	char IEmbeddedGrammarHost<ITextSnapshot>.At(ITextSnapshot version, int position)
	{
		return version[position];
	}

	TextSpan IEmbeddedGrammarHost<ITextSnapshot>.Translate(TextSpan span, ITextSnapshot from, ITextSnapshot to)
	{
		var translated = new SnapshotSpan(from, span.Start, span.Length)
			.TranslateTo(to, SpanTrackingMode.EdgeExclusive);

		return new TextSpan(translated.Start.Position, translated.Length);
	}

	Document? IEmbeddedGrammarHost<ITextSnapshot>.Document(ITextSnapshot version)
	{
		return _documents.TryGetTextDocument(_buffer, out var textDocument)
			? EmbeddedGrammarDocuments.WithText(_workspace.CurrentSolution, textDocument.FilePath, version.GetText())
			: null;
	}

	async Task IEmbeddedGrammarHost<ITextSnapshot>.RaiseAsync(Action raise, CancellationToken cancellationToken)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
		raise();
	}

	void IEmbeddedGrammarHost<ITextSnapshot>.Report(Exception exception)
	{
		ActivityLog.LogError("DotGram.VisualStudio", exception.ToString());
	}
}
