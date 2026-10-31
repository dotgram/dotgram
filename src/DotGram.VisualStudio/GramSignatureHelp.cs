using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

using DotGram.Grammar;
using DotGram.Language;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

[Export(typeof(IWpfTextViewCreationListener))]
[Name("DotGram signature help trigger")]
[ContentType(GramContentType.Name)]
[TextViewRole(PredefinedTextViewRoles.Editable)]
sealed class GramSignatureHelpTrigger : IWpfTextViewCreationListener
{
	[Import]
	ISignatureHelpBroker Broker { get; set; } = null!;

	public void TextViewCreated(IWpfTextView textView)
	{
		textView.TextBuffer.Changed += (sender, change) =>
			Trigger(textView, Broker, change, static (_, _) => true);
	}

	internal static void Trigger(
		IWpfTextView view,
		ISignatureHelpBroker broker,
		TextContentChangedEventArgs change,
		Func<ITextSnapshot, int, bool> isApplicable)
	{
		var edit = change.Changes.LastOrDefault();

		if (edit is null || edit.NewText.Length != 1 || edit.NewText[0] is not ('(' or ',' or ')'))
			return;

		_ = TriggerAsync();

		async Task TriggerAsync()
		{
			await Task.Delay(250).ConfigureAwait(false);
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var snapshot = view.TextSnapshot;
			var position = Math.Min(edit.NewPosition + 1, snapshot.Length);

			if (!isApplicable(snapshot, position))
				return;

			broker.DismissAllSessions(view);

			if (edit.NewText[0] != ')')
				broker.TriggerSignatureHelp(view);
		}
	}
}

[Export(typeof(IWpfTextViewCreationListener))]
[Name("DotGram embedded signature help trigger")]
[ContentType("CSharp")]
[TextViewRole(PredefinedTextViewRoles.Editable)]
sealed class EmbeddedGramSignatureHelpTrigger : IWpfTextViewCreationListener
{
	[Import]
	ISignatureHelpBroker Broker { get; set; } = null!;

	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	public void TextViewCreated(IWpfTextView textView)
	{
		var analysis = EmbeddedGrammarBufferAnalysis.For(textView.TextBuffer, Workspace, Documents);

		textView.TextBuffer.Changed += (sender, change) =>
			GramSignatureHelpTrigger.Trigger(
				textView,
				Broker,
				change,
				(snapshot, position) =>
					analysis.TryGet(snapshot, out var classifications, out _) &&
					classifications.Any(item => item.GrammarSpan.Contains(position)));
	}
}

[Export(typeof(ISignatureHelpSourceProvider))]
[Name("DotGram signature help")]
[Order(After = "default")]
[ContentType(GramContentType.Name)]
sealed class GramSignatureHelpSourceProvider : ISignatureHelpSourceProvider
{
	public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer)
	{
		return new GramSignatureHelpSource(textBuffer, GramBufferAnalysis.For(textBuffer));
	}
}

[Export(typeof(ISignatureHelpSourceProvider))]
[Name("DotGram embedded signature help")]
[Order(After = "default")]
[ContentType("CSharp")]
sealed class EmbeddedGramSignatureHelpSourceProvider : ISignatureHelpSourceProvider
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer)
	{
		return new EmbeddedGramSignatureHelpSource(
			textBuffer,
			EmbeddedGrammarBufferAnalysis.For(textBuffer, Workspace, Documents));
	}
}

/// <summary>Visual Studio's signature help source over the shared provider.</summary>
abstract class GramSignatureHelpSourceBase(ITextBuffer buffer) : ISignatureHelpSource
{
	public void AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures)
	{
		var snapshot = buffer.CurrentSnapshot;
		var trigger  = session.TextView.Caret.Position.BufferPosition
			.TranslateTo(snapshot, PointTrackingMode.Negative)
			.Position;

		if (Provider(snapshot).At(snapshot.GetText(), trigger) is not { } help)
			return;

		// Inside a proven DotGram grammar the host C# source describes GramAttribute's
		// constructor, not the rule call under the caret.
		signatures.Clear();

		foreach (var signature in help.Signatures)
			signatures.Add(new GramSignature(
				snapshot.CreateTrackingSpan(
					help.ApplicableSpan.Start,
					help.ApplicableSpan.Length,
					SpanTrackingMode.EdgeInclusive),
				signature,
				help.ActiveParameter));
	}

	/// <summary>What the analysis knows of the snapshot, as the shared provider reads it.</summary>
	protected abstract GramSignatureHelpProvider Provider(ITextSnapshot snapshot);

	public ISignature? GetBestMatch(ISignatureHelpSession session)
	{
		return session.Signatures.FirstOrDefault();
	}

	public void Dispose()
	{
	}
}

sealed class GramSignatureHelpSource(ITextBuffer buffer, GramBufferAnalysis analysis)
	: GramSignatureHelpSourceBase(buffer)
{
	protected override GramSignatureHelpProvider Provider(ITextSnapshot snapshot)
	{
		return new StandaloneGramSignatureHelpProvider(analysis.Document(snapshot));
	}
}

sealed class EmbeddedGramSignatureHelpSource(
	ITextBuffer buffer,
	EmbeddedGrammarBufferAnalysis analysis) : GramSignatureHelpSourceBase(buffer)
{
	protected override GramSignatureHelpProvider Provider(ITextSnapshot snapshot)
	{
		return new EmbeddedGramSignatureHelpProvider(analysis.View(snapshot));
	}
}

sealed class GramSignature : ISignature
{
	readonly ReadOnlyCollection<IParameter> _parameters;

	public GramSignature(
		ITrackingSpan applicableToSpan,
		Language.GramSignature signature,
		int activeParameter)
	{
		ApplicableToSpan = applicableToSpan;
		Content = signature.Label;
		Documentation = signature.Documentation;

		var parameters = signature.Parameters
			.Select(parameter => (IParameter)new GramParameter(
				this, parameter.Name, parameter.Documentation, new Span(parameter.Locus.Start, parameter.Locus.Length)))
			.ToList();

		_parameters = new ReadOnlyCollection<IParameter>(parameters);
		CurrentParameter = activeParameter >= 0 ? parameters[activeParameter] : null;
	}

	public ITrackingSpan ApplicableToSpan { get; }
	public string Content { get; }
	public string PrettyPrintedContent => Content;
	public string Documentation { get; }
	public ReadOnlyCollection<IParameter> Parameters => _parameters;
	public IParameter? CurrentParameter { get; }
	public event EventHandler<CurrentParameterChangedEventArgs> CurrentParameterChanged
	{
		add { }
		remove { }
	}
}

sealed class GramParameter(ISignature signature, string name, string documentation, Span locus) : IParameter
{
	public ISignature Signature { get; } = signature;
	public string Name { get; } = name;
	public string Documentation { get; } = documentation;
	public Span Locus { get; } = locus;
	public Span PrettyPrintedLocus => Locus;
}
