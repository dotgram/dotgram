using System.Linq;
using System.Threading;
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

	public CommandState GetCommandState(GoToDefinitionCommandArgs args)
	{
		var snapshot = args.TextView.TextSnapshot;
		var position = Position(args, snapshot);
		return PublishedApi(args, snapshot, position) is not null ||
			IsCSharpContext(args, snapshot, position, out _, out _) || Target(args) is not null
			? CommandState.Available
			: CommandState.Unavailable;
	}

	public bool ExecuteCommand(GoToDefinitionCommandArgs args, CommandExecutionContext executionContext)
	{
		var snapshot = args.TextView.TextSnapshot;
		var position = Position(args, snapshot);
		if (PublishedApi(args, snapshot, position) is { } publishedApi)
		{
			var roslyn = new RoslynGramCompletion(args.SubjectBuffer, Workspace, Documents);
			return Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(() =>
				roslyn.NavigateToDefinitionAsync(publishedApi, 0, CancellationToken.None));
		}

		if (IsCSharpContext(args, snapshot, position, out var expression, out var expressionPosition))
		{
			var roslyn = new RoslynGramCompletion(args.SubjectBuffer, Workspace, Documents);
			return Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(() =>
				roslyn.NavigateToDefinitionAsync(expression, expressionPosition, CancellationToken.None));
		}

		var found = Target(args);

		if (found is null)
			return false;

		var point = new SnapshotPoint(args.TextView.TextSnapshot, found.DefinitionPosition);
		args.TextView.Caret.MoveTo(point);
		args.TextView.ViewScroller.EnsureSpanVisible(new SnapshotSpan(point, 0));

		return true;
	}

	string? PublishedApi(GoToDefinitionCommandArgs args, ITextSnapshot snapshot, int position)
	{
		if (args.SubjectBuffer.ContentType.IsOfType(GramContentType.Name))
			return GramBufferAnalysis.For(args.SubjectBuffer).Document(snapshot).PublishedApis
				.FirstOrDefault(item => position >= item.Position && position < item.Position + item.Length)
				.MethodName;

		var analysis = EmbeddedGrammarBufferAnalysis.For(args.SubjectBuffer, Workspace, Documents);
		return analysis.TryGetPublishedApis(snapshot, out var publications)
			? publications.FirstOrDefault(item => item.Span.Contains(position)).MethodName
			: null;
	}

	GramFindReferencesTarget? Target(GoToDefinitionCommandArgs args)
	{
		var snapshot = args.TextView.TextSnapshot;
		var position = Position(args, snapshot);

		return args.SubjectBuffer.ContentType.IsOfType(GramContentType.Name)
			? GramFindReferencesTarget.Standalone(snapshot, position, GramBufferAnalysis.For(args.SubjectBuffer))
			: GramFindReferencesTarget.Embedded(
				snapshot,
				position,
				EmbeddedGrammarBufferAnalysis.For(args.SubjectBuffer, Workspace, Documents));
	}

	bool IsCSharpContext(
		GoToDefinitionCommandArgs args,
		ITextSnapshot snapshot,
		int position,
		out string expression,
		out int expressionPosition)
	{
		if (!args.SubjectBuffer.ContentType.IsOfType(GramContentType.Name))
		{
			var analysis = EmbeddedGrammarBufferAnalysis.For(args.SubjectBuffer, Workspace, Documents);
			if (!analysis.TryGet(snapshot, out var classifications, out _) ||
				!classifications.Any(item => item.GrammarSpan.Contains(position)))
			{
				expression = "";
				expressionPosition = 0;
				return false;
			}
		}

		if (!GramCSharpCompletionContext.TryGetExpression(
			snapshot.GetText(), position,
			out expression, out var expressionStart, out _, out _))
		{
			expressionPosition = 0;
			return false;
		}

		expressionPosition = position - expressionStart;
		return true;
	}

	static int Position(GoToDefinitionCommandArgs args, ITextSnapshot snapshot) =>
		args.TextView.Caret.Position.BufferPosition
			.TranslateTo(snapshot, PointTrackingMode.Negative).Position;
}
