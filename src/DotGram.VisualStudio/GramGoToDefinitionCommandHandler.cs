using System;
using System.Threading;
using System.ComponentModel.Composition;

using DotGram.Language;

using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DotGram.VisualStudio;

[Export(typeof(ICommandHandler))]
[Name("DotGram Go To Definition command")]
[ContentType(GramContentType.Name)]
[ContentType("CSharp")]
[Order(Before = "Go To Definition Command Handler")]
sealed class GramGoToDefinitionCommandHandler : ICommandHandler<GoToDefinitionCommandArgs>
{
	[Import]
	VisualStudioWorkspace Workspace { get; set; } = null!;

	[Import]
	ITextDocumentFactoryService Documents { get; set; } = null!;

	[Import(typeof(SVsServiceProvider))]
	System.IServiceProvider Services { get; set; } = null!;

	public string DisplayName => "DotGram Go To Definition";

	public CommandState GetCommandState(GoToDefinitionCommandArgs args)
	{
		var snapshot = args.TextView.TextSnapshot;
		var position = Position(args, snapshot);
		return args.SubjectBuffer.ContentType.IsOfType("CSharp") ||
			args.SubjectBuffer.ContentType.IsOfType(GramContentType.Name) &&
			GramNavigationProvider.CanGoToDefinition(GramBufferAnalysis.For(args.SubjectBuffer).View(snapshot), position)
			? CommandState.Available
			: CommandState.Unavailable;
	}

	public bool ExecuteCommand(GoToDefinitionCommandArgs args, CommandExecutionContext executionContext)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		var snapshot = args.TextView.TextSnapshot;
		var position = Position(args, snapshot);
		var roslyn   = RoslynGramFeatures.For(args.SubjectBuffer, Workspace, Documents);
		var store    = TempDirectoryIncludedGrammarStore.Instance;
		var location = args.SubjectBuffer.ContentType.IsOfType(GramContentType.Name)
			? ThreadHelper.JoinableTaskFactory.Run(() => GramNavigationProvider.DefinitionAsync(
				GramBufferAnalysis.For(args.SubjectBuffer).View(snapshot), position, roslyn.Core, CancellationToken.None))
			: ThreadHelper.JoinableTaskFactory.Run(() => GramNavigationProvider.DefinitionAsync(
				snapshot.GetText(),
				() => EmbeddedGrammarBufferAnalysis.For(args.SubjectBuffer, Workspace, Documents).View(snapshot),
				position,
				roslyn.Core,
				CancellationToken.None));

		switch (location)
		{
			case GramLocation.InDocument target:
				var point = new SnapshotPoint(args.TextView.TextSnapshot, target.Position);
				args.TextView.Caret.MoveTo(point);
				args.TextView.ViewScroller.EnsureSpanVisible(new SnapshotSpan(point, 0));
				return true;

			case GramLocation.InFile target:
				return Navigate(target.Path, target.Line, target.Column);

			case GramLocation.InIncludedGrammar target:
				return store.PathToOpen(target.Grammar) is { } path && Navigate(path, target.Line, target.Column);

			case GramLocation.InStandardLibrary target:
				return store.PathToOpen(target.Grammar) is { } library && Navigate(library, target.Line, target.Column);

			case GramLocation.HostSymbol target:
				return ThreadHelper.JoinableTaskFactory.Run(() => roslyn.NavigateAsync(target.Targets, CancellationToken.None));

			case GramLocation.PublishedMethod target:
				return ThreadHelper.JoinableTaskFactory.Run(() => roslyn.NavigateAsync(target.Targets, CancellationToken.None));

			default:
				return false;
		}
	}

	bool Navigate(string filePath, int line, int column)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		VsShellUtilities.OpenDocument(
			Services,
			filePath,
			Guid.Empty,
			out _,
			out _,
			out IVsWindowFrame frame,
			out IVsTextView view);

		frame.Show();
		view.SetCaretPos(line, column);
		view.CenterLines(line, 1);
		return true;
	}

	static int Position(GoToDefinitionCommandArgs args, ITextSnapshot snapshot)
	{
		return args.TextView.Caret.Position.BufferPosition
			.TranslateTo(snapshot, PointTrackingMode.Negative).Position;
	}
}
