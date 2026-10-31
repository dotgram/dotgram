using System.ComponentModel.Composition;

using DotGram.Language;

using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

[Export(typeof(ICommandHandler))]
[Name("DotGram Rename command")]
[ContentType(GramContentType.Name)]
[ContentType("CSharp")]
[Order(Before = "default")]
sealed class GramRenameCommandHandler : ICommandHandler<RenameCommandArgs>
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	public string DisplayName => "DotGram Rename";

	public CommandState GetCommandState(RenameCommandArgs args)
	{
		return Target(args) is null ? CommandState.Unavailable : CommandState.Available;
	}

	public bool ExecuteCommand(RenameCommandArgs args, CommandExecutionContext executionContext)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		var found = Target(args);

		if (found is null || args.TextView is not IWpfTextView view)
			return false;

		Rename(view, found);

		return true;
	}

	internal static void Rename(IWpfTextView view, GramReferences found)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		if (GramRenameProvider.Refusal(found) is { } refusal)
		{
			if (ServiceProvider.GlobalProvider.GetService(typeof(SVsStatusbar)) is IVsStatusbar status)
				status.SetText(refusal);

			return;
		}

		GramRenameAdornment.Show(view, found.Name, replacement =>
		{
			var rename = GramRenameProvider.Rename(found, replacement);

			if (rename.Edits.Count == 0)
				return;

			using var edit = view.TextBuffer.CreateEdit();

			foreach (var change in rename.Edits)
				edit.Replace(change.Span.Start, change.Span.Length, change.NewText);

			edit.Apply();
		});
	}

	GramReferences? Target(RenameCommandArgs args)
	{
		var snapshot = args.TextView.TextSnapshot;
		var position = args.TextView.Caret.Position.BufferPosition
			.TranslateTo(snapshot, PointTrackingMode.Negative).Position;

		return GramFindReferencesTarget.At(args.SubjectBuffer, snapshot, position, Workspace, Documents);
	}
}
