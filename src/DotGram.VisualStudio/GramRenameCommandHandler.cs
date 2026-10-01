using System.ComponentModel.Composition;

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

	internal static void Rename(IWpfTextView view, GramFindReferencesTarget found)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		// A rule an included grammar declares or uses is renamed there too, which this
		// buffer cannot do; renaming half of it is worse than not renaming it.
		if (!found.Renamable)
		{
			if (ServiceProvider.GlobalProvider.GetService(typeof(SVsStatusbar)) is IVsStatusbar status)
				status.SetText($"'{found.Name}' is declared or used in an included grammar and cannot be renamed from here.");

			return;
		}

		GramRenameAdornment.Show(view, found.Name, replacement =>
		{
			if (replacement == found.Name)
				return;

			using var edit = view.TextBuffer.CreateEdit();

			foreach (var position in found.Positions)
				edit.Replace(position, found.Name.Length, replacement);

			edit.Apply();
		});
	}

	GramFindReferencesTarget? Target(RenameCommandArgs args)
	{
		var snapshot = args.TextView.TextSnapshot;
		var position = args.TextView.Caret.Position.BufferPosition
			.TranslateTo(snapshot, PointTrackingMode.Negative).Position;

		return args.SubjectBuffer.ContentType.IsOfType(GramContentType.Name)
			? GramFindReferencesTarget.Standalone(snapshot, position, GramBufferAnalysis.For(args.SubjectBuffer))
			: GramFindReferencesTarget.Embedded(
				snapshot,
				position,
				EmbeddedGrammarBufferAnalysis.For(args.SubjectBuffer, Workspace, Documents));
	}
}
