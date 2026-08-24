using System.ComponentModel.Composition;

using Microsoft.VisualBasic;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
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

	public CommandState GetCommandState(RenameCommandArgs args) =>
		Target(args) is null ? CommandState.Unavailable : CommandState.Available;

	public bool ExecuteCommand(RenameCommandArgs args, CommandExecutionContext executionContext)
	{
		var found = Target(args);

		if (found is null)
			return false;

		Rename(args.SubjectBuffer, found);

		return true;
	}

	internal static void Rename(ITextBuffer buffer, GramFindReferencesTarget found)
	{
		var replacement = Interaction.InputBox(
			$"Rename DotGram rule '{found.Name}' to:",
			"Rename DotGram Rule",
			found.Name);

		if (replacement.Length == 0 || replacement == found.Name)
			return;

		using var edit = buffer.CreateEdit();

		foreach (var position in found.Positions)
			edit.Replace(position, found.Name.Length, replacement);

		edit.Apply();
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
