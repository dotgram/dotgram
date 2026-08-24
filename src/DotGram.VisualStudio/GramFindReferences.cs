using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

[Export(typeof(IVsTextViewCreationListener))]
[ContentType(GramContentType.Name)]
[TextViewRole(PredefinedTextViewRoles.Document)]
sealed class GramFindReferencesViewListener : IVsTextViewCreationListener
{
	[Import]
	IVsEditorAdaptersFactoryService Adapters { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	[Import(typeof(SVsServiceProvider))]
	System.IServiceProvider Services { get; set; } = null!;

	public void VsTextViewCreated(IVsTextView textViewAdapter)
	{
		var view = Adapters.GetWpfTextView(textViewAdapter);

		if (view is null)
			return;

		var filter = new GramFindReferencesCommandFilter(
			view,
			Documents,
			Services,
			(snapshot, position) => GramFindReferencesTarget.Standalone(
				snapshot,
				position,
				GramBufferAnalysis.For(view.TextBuffer)));
		textViewAdapter.AddCommandFilter(filter, out var next);
		filter.Next = next;
	}
}

[Export(typeof(IVsTextViewCreationListener))]
[ContentType("CSharp")]
[TextViewRole(PredefinedTextViewRoles.Document)]
sealed class EmbeddedGramFindReferencesViewListener : IVsTextViewCreationListener
{
	[Import]
	IVsEditorAdaptersFactoryService Adapters { get; set; } = null!;

	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	[Import(typeof(SVsServiceProvider))]
	System.IServiceProvider Services { get; set; } = null!;

	public void VsTextViewCreated(IVsTextView textViewAdapter)
	{
		var view = Adapters.GetWpfTextView(textViewAdapter);

		if (view is null)
			return;

		var analysis = EmbeddedGrammarBufferAnalysis.For(view.TextBuffer, Workspace, Documents);
		var filter = new GramFindReferencesCommandFilter(
			view,
			Documents,
			Services,
			(snapshot, position) => GramFindReferencesTarget.Embedded(snapshot, position, analysis));
		textViewAdapter.AddCommandFilter(filter, out var next);
		filter.Next = next;
	}
}

sealed class GramFindReferencesCommandFilter(
	IWpfTextView view,
	ITextDocumentFactoryService documents,
	System.IServiceProvider services,
	Func<ITextSnapshot, int, GramFindReferencesTarget?> target) : IOleCommandTarget
{
	static readonly Guid PaneId = new("CB113D62-0AA2-4DC7-BD15-601A963979C0");

	public IOleCommandTarget Next { get; set; } = null!;

	public int QueryStatus(ref Guid commandGroup, uint commandCount, OLECMD[] commands, IntPtr commandText)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		if (commandGroup == VSConstants.GUID_VSStandardCommandSet97 &&
			commandCount == 1 &&
			commands[0].cmdID == (uint)VSConstants.VSStd97CmdID.FindReferences &&
			Target() is not null)
		{
			commands[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);

			return VSConstants.S_OK;
		}

		return Next.QueryStatus(ref commandGroup, commandCount, commands, commandText);
	}

	public int Exec(ref Guid commandGroup, uint commandId, uint options, IntPtr input, IntPtr output)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		if (commandGroup != VSConstants.GUID_VSStandardCommandSet97 ||
			commandId != (uint)VSConstants.VSStd97CmdID.FindReferences)
			return Next.Exec(ref commandGroup, commandId, options, input, output);

		var found = Target();

		if (found is null || !documents.TryGetTextDocument(view.TextBuffer, out var document))
			return Next.Exec(ref commandGroup, commandId, options, input, output);

		Show(found, document.FilePath, view.TextSnapshot);

		return VSConstants.S_OK;
	}

	GramFindReferencesTarget? Target()
	{
		var snapshot = view.TextSnapshot;
		var position = view.Caret.Position.BufferPosition.TranslateTo(snapshot, PointTrackingMode.Negative).Position;

		return target(snapshot, position);
	}

	void Show(GramFindReferencesTarget found, string filePath, ITextSnapshot snapshot)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		if (services.GetService(typeof(SVsOutputWindow)) is not IVsOutputWindow output)
			return;

		var paneId = PaneId;
		output.CreatePane(ref paneId, "DotGram References", 1, 0);
		output.GetPane(ref paneId, out var pane);
		pane.Clear();
		pane.OutputString($"{found.Name} references ({found.Positions.Length})\r\n");

		foreach (var position in found.Positions)
		{
			var line = snapshot.GetLineFromPosition(position);
			var column = position - line.Start.Position;
			var text = line.GetText().Trim();

			pane.OutputTaskItemString(
				$"{filePath}({line.LineNumber + 1},{column + 1}): {text}\r\n",
				VSTASKPRIORITY.TP_NORMAL,
				VSTASKCATEGORY.CAT_CODESENSE,
				string.Empty,
				0,
				filePath,
				(uint)line.LineNumber,
				found.Name);
		}

		pane.Activate();
	}
}

sealed class GramFindReferencesTarget(string name, int[] positions)
{
	public string Name { get; } = name;
	public int[] Positions { get; } = positions;

	public static GramFindReferencesTarget? Standalone(
		ITextSnapshot snapshot,
		int position,
		GramBufferAnalysis analysis)
	{
		var symbols = analysis.Document(snapshot).Symbols;
		var current = symbols.FirstOrDefault(symbol =>
			symbol.Position <= position && position < symbol.Position + symbol.Length);

		return current.Length == 0
			? null
			: new GramFindReferencesTarget(
				current.Name,
				symbols.Where(symbol => symbol.Name == current.Name).Select(symbol => symbol.Position).ToArray());
	}

	public static GramFindReferencesTarget? Embedded(
		ITextSnapshot snapshot,
		int position,
		EmbeddedGrammarBufferAnalysis analysis)
	{
		if (!analysis.TryGetSymbols(snapshot, out var symbols))
			return null;

		var current = symbols.FirstOrDefault(symbol => symbol.Span.Contains(position));

		return current.Span.Length == 0
			? null
			: new GramFindReferencesTarget(
				current.Name,
				symbols
					.Where(symbol => symbol.Name == current.Name &&
						symbol.GrammarSpan == current.GrammarSpan &&
						symbol.DefinitionSpan == current.DefinitionSpan)
					.Select(symbol => symbol.Span.Start)
					.ToArray());
	}
}
