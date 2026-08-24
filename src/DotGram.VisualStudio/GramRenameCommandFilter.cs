using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

[Export(typeof(IWpfTextViewCreationListener))]
[ContentType(GramContentType.Name)]
[ContentType("CSharp")]
[TextViewRole(PredefinedTextViewRoles.Editable)]
sealed class GramRenameViewListener : IWpfTextViewCreationListener
{
	[Import]
	IVsEditorAdaptersFactoryService Adapters { get; set; } = null!;

	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	public void TextViewCreated(IWpfTextView view)
	{
		var adapter = Adapters.GetViewAdapter(view);

		if (adapter is null)
			return;

		Func<ITextSnapshot, int, GramFindReferencesTarget?> target =
			view.TextBuffer.ContentType.IsOfType(GramContentType.Name)
				? (snapshot, position) => GramFindReferencesTarget.Standalone(
					snapshot,
					position,
					GramBufferAnalysis.For(view.TextBuffer))
				: (snapshot, position) => GramFindReferencesTarget.Embedded(
					snapshot,
					position,
					EmbeddedGrammarBufferAnalysis.For(view.TextBuffer, Workspace, Documents));
		var filter = new GramRenameCommandFilter(view, target);
		adapter.AddCommandFilter(filter, out var next);
		filter.Next = next;
	}
}

sealed class GramRenameCommandFilter(
	IWpfTextView view,
	Func<ITextSnapshot, int, GramFindReferencesTarget?> target) : IOleCommandTarget
{
	public IOleCommandTarget Next { get; set; } = null!;

	public int QueryStatus(ref Guid commandGroup, uint commandCount, OLECMD[] commands, IntPtr commandText)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		if (commandCount == 1 && IsRename(commandGroup, commands[0].cmdID) && Target() is not null)
		{
			commands[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
			return VSConstants.S_OK;
		}

		return Next.QueryStatus(ref commandGroup, commandCount, commands, commandText);
	}

	public int Exec(ref Guid commandGroup, uint commandId, uint options, IntPtr input, IntPtr output)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		if (!IsRename(commandGroup, commandId))
			return Next.Exec(ref commandGroup, commandId, options, input, output);

		var found = Target();

		if (found is null)
			return Next.Exec(ref commandGroup, commandId, options, input, output);

		GramRenameCommandHandler.Rename(view.TextBuffer, found);

		return VSConstants.S_OK;
	}

	GramFindReferencesTarget? Target()
	{
		var snapshot = view.TextSnapshot;
		var position = view.Caret.Position.BufferPosition
			.TranslateTo(snapshot, PointTrackingMode.Negative).Position;

		return target(snapshot, position);
	}

	static bool IsRename(Guid group, uint commandId) =>
		group == VSConstants.VSStd2K &&
			(commandId == (uint)VSConstants.VSStd2KCmdID.RENAME ||
			 commandId == (uint)VSConstants.VSStd2KCmdID.ECMD_RENAMESYMBOL) ||
		group == VSConstants.GUID_VSStandardCommandSet97 &&
			commandId == (uint)VSConstants.VSStd97CmdID.Rename;
}
