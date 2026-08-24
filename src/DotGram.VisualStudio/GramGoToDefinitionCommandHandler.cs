using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

[Export(typeof(ICommandHandler))]
[Name("DotGram Go To Definition command")]
[ContentType(GramContentType.Name)]
[ContentType("CSharp")]
[Order(Before = "default")]
sealed class GramGoToDefinitionCommandHandler : ICommandHandler<GoToDefinitionCommandArgs>
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	public string DisplayName => "DotGram Go To Definition";

	public CommandState GetCommandState(GoToDefinitionCommandArgs args) =>
		Target(args) is null ? CommandState.Unavailable : CommandState.Available;

	public bool ExecuteCommand(GoToDefinitionCommandArgs args, CommandExecutionContext executionContext)
	{
		var found = Target(args);

		if (found is null)
			return false;

		var point = new SnapshotPoint(args.TextView.TextSnapshot, found.DefinitionPosition);
		args.TextView.Caret.MoveTo(point);
		args.TextView.ViewScroller.EnsureSpanVisible(new SnapshotSpan(point, 0));

		return true;
	}

	GramFindReferencesTarget? Target(GoToDefinitionCommandArgs args)
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
