using System;
using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// What comes while a standalone grammar's host is looked for, step by step as an editor's
/// resolution goes: started, a change while it reads the solution, ended with what it read.
/// </summary>
public sealed class StandaloneGrammarChangesTests : IDisposable
{
	const string DialectPath = @"P:\Parsers\Dialect.gram";
	const string BasePath    = @"P:\Parsers\Base.gram";

	readonly AdhocWorkspace _workspace = new();
	readonly ProjectId      _project;
	readonly DocumentId     _base;
	readonly DocumentId     _dialect;

	public StandaloneGrammarChangesTests()
	{
		_project = _workspace.AddProject("Parsers", LanguageNames.CSharp).Id;
		_base    = DocumentId.CreateNewId(_project);
		_dialect = DocumentId.CreateNewId(_project);

		var solution = _workspace.CurrentSolution
			.AddAdditionalDocument(_base, "Base.gram", SourceText.From("Word = 'a'\n"), filePath: BasePath)
			.AddAdditionalDocument(_dialect, "Dialect.gram", SourceText.From("A = B.Word\n"), filePath: DialectPath);

		Assert.True(_workspace.TryApplyChanges(solution));
	}

	public void Dispose()
	{
		_workspace.Dispose();
	}

	/// <summary>
	/// The first resolution knows none of the grammars the host includes, so an edit to one of them
	/// is no concern of it; asked again of what it found, it sends the host to be looked for again.
	/// </summary>
	[Fact]
	public void An_included_grammars_edit_during_the_first_resolution_is_asked_of_what_it_found()
	{
		var changes = new StandaloneGrammarChanges();

		using var first = new CancellationTokenSource();

		Assert.True(changes.Start(first.Token));

		var edit = Edit(_base);

		Assert.Equal(StandaloneGrammarConcern.None, edit.Concerns(StandaloneGrammarResolution.NotReady));
		changes.Keep(edit);

		var documents = new List<DocumentId>();

		Assert.Equal(StandaloneGrammarConcern.Full, changes.End(first.Token, Found(), documents));
		Assert.Equal(0, changes.Kept);
	}

	/// <summary>
	/// A resolution cancelled after its delay may still come to start, after the next one started
	/// and kept a change: it is refused, and what the next one kept stays the next one's.
	/// </summary>
	[Fact]
	public void A_cancelled_resolution_leaves_what_the_next_one_kept_alone()
	{
		var changes = new StandaloneGrammarChanges();

		using var first  = new CancellationTokenSource();
		using var second = new CancellationTokenSource();

		Assert.True(changes.Start(first.Token));

		first.Cancel();

		Assert.True(changes.Start(second.Token));
		changes.Keep(Edit(_base));

		Assert.False(changes.Start(first.Token));
		changes.Abandon(first.Token);
		Assert.Equal(StandaloneGrammarConcern.None, changes.End(first.Token, Found(), []));
		Assert.Equal(1, changes.Kept);

		Assert.Equal(StandaloneGrammarConcern.Full, changes.End(second.Token, Found(), []));
	}

	/// <summary>
	/// Kept is one entry for each kind of change to each document, however often it comes; never
	/// the grammar's own text; nothing while no resolution runs; nothing once the document is closed.
	/// </summary>
	[Fact]
	public void What_is_kept_is_bounded_and_let_go()
	{
		var changes = new StandaloneGrammarChanges();

		changes.Keep(Edit(_base));
		Assert.Equal(0, changes.Kept);

		using var running = new CancellationTokenSource();

		Assert.True(changes.Start(running.Token));

		for (var index = 0; index < 100; index++)
			changes.Keep(Edit(_base));

		var own = Edit(_dialect);

		Assert.True(own.OwnText);
		changes.Keep(own);
		Assert.Equal(1, changes.Kept);

		changes.Abandon(running.Token);
		Assert.Equal(0, changes.Kept);

		Assert.True(changes.Start(running.Token));
		changes.Keep(Edit(_base));
		changes.Clear();
		Assert.Equal(0, changes.Kept);
		Assert.Equal(StandaloneGrammarConcern.None, changes.End(running.Token, Found(), []));
	}

	/// <summary>Against a resolution that found no host, an edit in its project is one document to look at.</summary>
	[Fact]
	public void An_edit_in_a_project_without_a_host_is_a_document_to_look_at()
	{
		var changes = new StandaloneGrammarChanges();
		var host    = DocumentId.CreateNewId(_project);
		var edit    = new StandaloneGrammarChange(WorkspaceChangeKind.DocumentChanged, _project, host, OwnText: false, Holds: false);

		using var running = new CancellationTokenSource();

		Assert.True(changes.Start(running.Token));
		changes.Keep(edit);

		var documents = new List<DocumentId>();

		Assert.Equal(StandaloneGrammarConcern.Document, changes.End(running.Token, StandaloneGrammarResolution.NoHost(_project), documents));
		Assert.Equal([host], documents);
	}

	StandaloneGrammarChange Edit(DocumentId document)
	{
		var before = _workspace.CurrentSolution;
		var after  = before.WithAdditionalDocumentText(document, SourceText.From("Word = 'a'\nExtra = 'x'\n"));

		return StandaloneGrammarChange.Of(
			new WorkspaceChangeEventArgs(WorkspaceChangeKind.AdditionalDocumentChanged, before, after, _project, document),
			DialectPath);
	}

	StandaloneGrammarResolution Found()
	{
		var included = new StandaloneIncludedGrammar("B", "Word = 'a'\n", BasePath);

		return new StandaloneGrammarResolution(
			StandaloneGrammarState.Resolved,
			new StandaloneGrammarContext("Word = 'a'\n", [included]),
			[_project],
			[_base]);
	}
}
