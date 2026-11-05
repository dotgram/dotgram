using System;
using System.ComponentModel.Composition;
using System.Threading;

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

	[Import]
	GramCSharpFindReferencesService FindReferences { get; set; } = null!;

	public void TextViewCreated(IWpfTextView view)
	{
		EditorGuard.Run("Rename command filter", () =>
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			Created(view);
		});
	}

	void Created(IWpfTextView view)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
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
		Func<bool>? findReferences = view.TextBuffer.ContentType.IsOfType("CSharp")
			? () => FindReferences.Find(view)
			: null;
		var filter = new GramRenameCommandFilter(view, target, findReferences);
		adapter.AddCommandFilter(filter, out var next);
		filter.Next = next;
	}
}

[Export]
sealed class GramCSharpFindReferencesService
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	[Import]
	ITextBufferFactoryService Buffers { get; set; } = null!;

	[Import]
	IContentTypeRegistryService ContentTypes { get; set; } = null!;

	[Import(typeof(SVsServiceProvider))]
	System.IServiceProvider Services { get; set; } = null!;

	public bool Find(IWpfTextView view)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		try
		{
			var snapshot = view.TextSnapshot;
			var position = view.Caret.Position.BufferPosition
				.TranslateTo(snapshot, PointTrackingMode.Negative).Position;
			var references = ThreadHelper.JoinableTaskFactory.Run(() =>
				new RoslynGramCompletion(view.TextBuffer, Workspace, Documents).FindReferencesAsync(
					position,
					CancellationToken.None,
					message => ActivityLog.LogError("DotGram.VisualStudio", message)));
			if (references is null)
				return false;

			ActivityLog.LogInformation(
				"DotGram.VisualStudio",
				$"C# Find All References found {references.References.Count} results for {references.Name}.");
			GramFindReferencesCommandHandler.Show(references, Services, Buffers, ContentTypes);
			return true;
		}
		catch (Exception exception) when (EditorGuard.Recoverable(exception))
		{
			EditorGuard.Log("C# Find All References", exception);
			return false;
		}
	}
}

sealed class GramRenameCommandFilter(
	IWpfTextView view,
	Func<ITextSnapshot, int, GramFindReferencesTarget?> target,
	Func<bool>? findReferences) : IOleCommandTarget
{
	public IOleCommandTarget Next { get; set; } = null!;

	// Only this filter's own part is guarded: what it does not handle, or fails to, goes on to the
	// next command target, whose failures are not this extension's to swallow.
	public int QueryStatus(ref Guid commandGroup, uint commandCount, OLECMD[] commands, IntPtr commandText)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		var group = commandGroup;

		if (EditorGuard.Run("Rename command filter state", false, () => Supports(group, commandCount, commands)))
			return VSConstants.S_OK;

		return Next.QueryStatus(ref commandGroup, commandCount, commands, commandText);
	}

	public int Exec(ref Guid commandGroup, uint commandId, uint options, IntPtr input, IntPtr output)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		var group = commandGroup;

		if (EditorGuard.Run("Rename command filter", false, () => Handle(group, commandId)))
			return VSConstants.S_OK;

		return Next.Exec(ref commandGroup, commandId, options, input, output);
	}

	bool Supports(Guid commandGroup, uint commandCount, OLECMD[] commands)
	{
		if (commandCount == 1 && IsRename(commandGroup, commands[0].cmdID) && Target() is not null)
		{
			commands[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
			return true;
		}
		if (commandCount == 1 && IsFindReferences(commandGroup, commands[0].cmdID) && findReferences is not null)
		{
			commands[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
			return true;
		}

		return false;
	}

	bool Handle(Guid commandGroup, uint commandId)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		if (GramRenameAdornment.TryHandleCommand(view, commandGroup, commandId))
			return true;

		if (IsFindReferences(commandGroup, commandId))
			return findReferences?.Invoke() == true;

		if (!IsRename(commandGroup, commandId))
			return false;

		var found = Target();

		if (found is null)
			return false;

		GramRenameCommandHandler.Rename(view, found);

		return true;
	}

	GramFindReferencesTarget? Target()
	{
		var snapshot = view.TextSnapshot;
		var position = view.Caret.Position.BufferPosition
			.TranslateTo(snapshot, PointTrackingMode.Negative).Position;

		return target(snapshot, position);
	}

	static bool IsRename(Guid group, uint commandId)
	{
		return group == VSConstants.VSStd2K &&
			(commandId == (uint)VSConstants.VSStd2KCmdID.RENAME ||
			 commandId == (uint)VSConstants.VSStd2KCmdID.ECMD_RENAMESYMBOL) ||
		group == VSConstants.GUID_VSStandardCommandSet97 &&
			commandId == (uint)VSConstants.VSStd97CmdID.Rename;
	}

	static bool IsFindReferences(Guid group, uint commandId)
	{
		return group == VSConstants.GUID_VSStandardCommandSet97 && commandId == (uint)VSConstants.VSStd97CmdID.FindReferences;
	}
}
