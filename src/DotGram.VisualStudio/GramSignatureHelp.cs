using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

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

	public void TextViewCreated(IWpfTextView textView) =>
		textView.TextBuffer.Changed += (sender, change) =>
			Trigger(textView, Broker, change, static (_, _) => true);

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
	public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer) =>
		new GramSignatureHelpSource(textBuffer, GramBufferAnalysis.For(textBuffer));
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

	public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer) =>
		new EmbeddedGramSignatureHelpSource(
			textBuffer,
			EmbeddedGrammarBufferAnalysis.For(textBuffer, Workspace, Documents));
}

abstract class GramSignatureHelpSourceBase(ITextBuffer buffer) : ISignatureHelpSource
{
	public void AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures)
	{
		var snapshot = buffer.CurrentSnapshot;
		var trigger  = session.TextView.Caret.Position.BufferPosition
			.TranslateTo(snapshot, PointTrackingMode.Negative)
			.Position;

		if (!TryFindCall(snapshot, trigger, out var name, out var openParen, out var argument))
			return;

		var available = Definitions(snapshot, trigger).ToArray();
		var definition = available.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.Ordinal));

		if (definition.Signature is null)
			return;

		// Inside a proven DotGram grammar the host C# source describes GramAttribute's
		// constructor, not the rule call under the caret.
		signatures.Clear();
		signatures.Add(new GramSignature(
			snapshot.CreateTrackingSpan(
				openParen,
				Math.Max(1, trigger - openParen),
				SpanTrackingMode.EdgeInclusive),
			definition.Signature,
			Documentation(definition.Signature, definition.Description),
			argument));
	}

	static string Documentation(string signature, string description) =>
		description.StartsWith(signature, StringComparison.Ordinal)
			? description.Substring(signature.Length).TrimStart()
			: description;

	public ISignature? GetBestMatch(ISignatureHelpSession session) =>
		session.Signatures.FirstOrDefault();

	public void Dispose()
	{
	}

	protected abstract IEnumerable<RuleSignature> Definitions(ITextSnapshot snapshot, int position);

	protected readonly struct RuleSignature(string name, string signature, string description)
	{
		public string Name { get; } = name;
		public string Signature { get; } = signature;
		public string Description { get; } = description;
	}

	static bool TryFindCall(
		ITextSnapshot snapshot,
		int position,
		out string name,
		out int openParen,
		out int argument)
	{
		var depth = 0;
		argument = 0;

		for (var current = Math.Min(position, snapshot.Length) - 1; current >= 0; current--)
			switch (snapshot[current])
			{
				case ')': depth++; break;
				case '(' when depth > 0: depth--; break;
				case ',' when depth == 0: argument++; break;
				case '(':
					openParen = current;
					var end = current;
					var start = end;

					while (start > 0 && char.IsWhiteSpace(snapshot[start - 1])) start--;
					end = start;
					while (start > 0 && (char.IsLetterOrDigit(snapshot[start - 1]) || snapshot[start - 1] == '_')) start--;

					name = snapshot.GetText(start, end - start);
					return name.Length > 0;
			}

		name = "";
		openParen = 0;
		return false;
	}
}

sealed class GramSignatureHelpSource(ITextBuffer buffer, GramBufferAnalysis analysis)
	: GramSignatureHelpSourceBase(buffer)
{
	protected override IEnumerable<RuleSignature> Definitions(ITextSnapshot snapshot, int position) =>
		analysis.Document(snapshot).Classifications
			.Where(static item => item.DefinitionPosition == item.Position && item.RuleParameterCount > 0)
			.Select(item => new RuleSignature(
				snapshot.GetText(item.Position, item.Length),
				item.RuleSignature!,
				item.QuickInfo!));
}

sealed class EmbeddedGramSignatureHelpSource(
	ITextBuffer buffer,
	EmbeddedGrammarBufferAnalysis analysis) : GramSignatureHelpSourceBase(buffer)
{
	protected override IEnumerable<RuleSignature> Definitions(ITextSnapshot snapshot, int position)
	{
		if (!analysis.TryGet(snapshot, out var classifications, out _))
			return [];

		return classifications
			.Where(item =>
				item.GrammarSpan.Contains(position) &&
				item.DefinitionSpan == item.Span &&
				item.RuleParameterCount > 0)
			.Select(item => new RuleSignature(
				snapshot.GetText(item.Span.Start, item.Span.Length),
				item.RuleSignature!,
				item.QuickInfo!));
	}
}

sealed class GramSignature : ISignature
{
	readonly ReadOnlyCollection<IParameter> _parameters;

	public GramSignature(
		ITrackingSpan applicableToSpan,
		string content,
		string documentation,
		int argument)
	{
		ApplicableToSpan = applicableToSpan;
		Content = content;
		Documentation = documentation;

		var parameters = new List<IParameter>();
		var open = content.IndexOf('(');
		var close = content.LastIndexOf(')');

		if (open >= 0 && close > open)
		{
			var start = open + 1;

			foreach (var part in content.Substring(start, close - start).Split(','))
			{
				var trimmed = part.Trim();
				var offset = content.IndexOf(trimmed, start, StringComparison.Ordinal);

				parameters.Add(new GramParameter(this, trimmed, new Span(offset, trimmed.Length)));
				start = offset + trimmed.Length;
			}
		}

		_parameters = new ReadOnlyCollection<IParameter>(parameters);
		CurrentParameter = argument < parameters.Count ? parameters[argument] : parameters.LastOrDefault();
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

sealed class GramParameter(ISignature signature, string name, Span locus) : IParameter
{
	public ISignature Signature { get; } = signature;
	public string Name { get; } = name;
	public string Documentation => "DotGram rule parameter";
	public Span Locus { get; } = locus;
	public Span PrettyPrintedLocus => Locus;
}
