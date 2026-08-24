using System.ComponentModel.Composition;
using System.Linq;

using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.LanguageServices;
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

	public CommandState GetCommandState(RenameCommandArgs args) =>
		Target(args) is null ? CommandState.Unavailable : CommandState.Available;

	public bool ExecuteCommand(RenameCommandArgs args, CommandExecutionContext executionContext)
	{
		var found = Target(args);

		if (found is null)
			return false;

		var snapshot   = args.TextView.TextSnapshot;
		var selections = found.Positions
			.Select(position => new Selection(
				new SnapshotSpan(snapshot, position, found.Name.Length),
				isReversed: false))
			.ToArray();
		var caretPosition = args.TextView.Caret.Position.BufferPosition
			.TranslateTo(snapshot, PointTrackingMode.Negative).Position;
		var primary = selections.First(selection => selection.Extent.SnapshotSpan.Contains(caretPosition));

		args.TextView.GetMultiSelectionBroker().SetSelectionRange(selections, primary);

		return true;
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
