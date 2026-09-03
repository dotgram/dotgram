using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;

using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.FindResults;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

[Export(typeof(ICommandHandler))]
[Name("DotGram Find References command")]
[ContentType(GramContentType.Name)]
[ContentType("CSharp")]
[Order(Before = "default")]
sealed class GramFindReferencesCommandHandler : ICommandHandler<FindReferencesCommandArgs>
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

	public string DisplayName => "DotGram Find References";

	public CommandState GetCommandState(FindReferencesCommandArgs args)
	{
		if (args.SubjectBuffer.ContentType.IsOfType("CSharp"))
			return CommandState.Available;

		var found = Target(args);
		return found is null ? CommandState.Unavailable : CommandState.Available;
	}

	public bool ExecuteCommand(FindReferencesCommandArgs args, CommandExecutionContext executionContext)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		if (args.SubjectBuffer.ContentType.IsOfType("CSharp"))
		{
			try
			{
				var position = args.TextView.Caret.Position.BufferPosition
					.TranslateTo(args.SubjectBuffer.CurrentSnapshot, PointTrackingMode.Negative).Position;
				var references = ThreadHelper.JoinableTaskFactory.Run(() =>
					new RoslynGramCompletion(args.SubjectBuffer, Workspace, Documents)
						.FindReferencesAsync(
							position,
							CancellationToken.None,
							message => ActivityLog.LogError("DotGram.VisualStudio", message)));
				if (references is null)
				{
					ActivityLog.LogWarning("DotGram.VisualStudio", "C# Find All References found no symbol at the caret.");
					return false;
				}

				ActivityLog.LogInformation(
					"DotGram.VisualStudio",
					$"C# Find All References found {references.References.Count} results for {references.Name}.");
				Show(references);
				return true;
			}
			catch (Exception exception) when (exception is not OutOfMemoryException)
			{
				ActivityLog.LogError("DotGram.VisualStudio", exception.ToString());
				return false;
			}
		}

		var found = Target(args);

		if (found is null || !Documents.TryGetTextDocument(args.SubjectBuffer, out var document))
			return false;

		Show(found, document.FilePath, args.TextView.TextSnapshot);

		return true;
	}

	GramFindReferencesTarget? Target(FindReferencesCommandArgs args)
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

	void Show(GramFindReferencesTarget found, string filePath, ITextSnapshot snapshot)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		if (Services.GetService(typeof(SVsFindResults)) is not IFindResultsService findResults)
			return;

		var results = new List<FindResult>(found.Positions.Length);

		foreach (var position in found.Positions)
		{
			var line = snapshot.GetLineFromPosition(position);
			var column = position - line.Start.Position;

			results.Add(new FindResult(
				line.GetText(),
				line.LineNumber,
				column,
				new Span(column, found.Name.Length)));
		}

		var window = (IFindResultsWindow2)findResults.StartSearch(
			$"{found.Name} references ({found.Positions.Length})",
			$"DotGram rule references in {filePath}",
			"DotGram.FindReferences");
		window.Summary = $"{found.Positions.Length} references in 1 file";
		window.AddResults(filePath, filePath, snapshot, results);
		window.Complete();
	}

	void Show(CSharpFindReferences found)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		if (Services.GetService(typeof(SVsFindResults)) is not IFindResultsService findResults)
			return;

		var window = (IFindResultsWindow2)findResults.StartSearch(
			$"{found.Name} references ({found.References.Count})",
			"C# and DotGram references",
			"DotGram.CSharpFindReferences");
		window.Summary = $"{found.References.Count} references in {found.References.Select(static item => item.FilePath).Distinct(StringComparer.OrdinalIgnoreCase).Count()} files";

		foreach (var file in found.References.GroupBy(static item => item.FilePath, StringComparer.OrdinalIgnoreCase))
		{
			var snapshot = Buffers.CreateTextBuffer(file.First().Text, ContentTypes.GetContentType("text")).CurrentSnapshot;
			var results = new List<FindResult>();
			foreach (var reference in file)
			{
				var line = snapshot.GetLineFromPosition(reference.Position);
				var column = reference.Position - line.Start.Position;
				results.Add(new FindResult(
					line.GetText(), line.LineNumber, column, new Span(column, reference.Length)));
			}

			window.AddResults(file.Key, file.Key, snapshot, results);
		}

		window.Complete();
	}
}
