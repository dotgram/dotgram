using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;

namespace DotGram.Language;

/// <summary>What a change to the solution asks of a standalone grammar.</summary>
enum StandaloneGrammarConcern
{
	/// <summary>Nothing: it cannot change what the grammar is compiled with.</summary>
	None,

	/// <summary>A document was edited that may now declare the host: look at it, and only then for the host.</summary>
	Document,

	/// <summary>Look for the host again.</summary>
	Full,
}

/// <summary>
/// A change to the solution as far as a standalone grammar is concerned, without the solutions
/// themselves: what changed, where, and the two things only the solutions can tell.
/// </summary>
/// <param name="OwnText">The grammar's own file was edited, which concerns no resolution: its text is the editor's.</param>
/// <param name="Holds">The project holds the grammar's file before or after the change; asked only for what can add or remove a document.</param>
readonly record struct StandaloneGrammarChange(
	WorkspaceChangeKind Kind,
	ProjectId?          Project,
	DocumentId?         Document,
	bool                OwnText,
	bool                Holds)
{
	public static StandaloneGrammarChange Of(WorkspaceChangeEventArgs change, string filePath)
	{
		var ownText = change.Kind == WorkspaceChangeKind.AdditionalDocumentChanged &&
			change.DocumentId is { } edited &&
			string.Equals(
				change.NewSolution.GetAdditionalDocument(edited)?.FilePath,
				filePath,
				StringComparison.OrdinalIgnoreCase);

		var holds = change.ProjectId is { } project &&
			AddsOrRemoves(change.Kind) &&
			(HeldIn(change.NewSolution, project, filePath) || HeldIn(change.OldSolution, project, filePath));

		return new StandaloneGrammarChange(change.Kind, change.ProjectId, change.DocumentId, ownText, holds);
	}

	/// <summary>
	/// Whether the change can change what the grammar is compiled with, as <paramref name="resolution"/>
	/// knows it: its host, what the host includes, or how it is read.
	/// </summary>
	/// <remarks>
	/// The grammar's own text is not among them — it is the editor's, and read from there —
	/// and nothing in a project that neither holds the grammar nor declares what it includes
	/// is either. Until the host is found, an edit to a document of the grammar's own project
	/// may be the one that makes it findable; that is answered by looking at the one document
	/// (<see cref="StandaloneGrammarConcern.Document"/>), not by looking for the host again.
	/// </remarks>
	public StandaloneGrammarConcern Concerns(StandaloneGrammarResolution resolution)
	{
		switch (Kind)
		{
			case WorkspaceChangeKind.SolutionAdded:
			case WorkspaceChangeKind.SolutionChanged:
			case WorkspaceChangeKind.SolutionRemoved:
			case WorkspaceChangeKind.SolutionCleared:
			case WorkspaceChangeKind.SolutionReloaded:
				return StandaloneGrammarConcern.Full;
		}

		if (Project is not { } project)
			return StandaloneGrammarConcern.Full;

		if (OwnText)
			return StandaloneGrammarConcern.None;

		if (Document is { } document && resolution.Documents.Contains(document))
			return StandaloneGrammarConcern.Full;

		var watched = resolution.Projects.Contains(project);

		if (AddsOrRemoves(Kind))
			// What can put the grammar into a project, or take it out: asked of both solutions,
			// since a removal is only in the old one.
			return watched || resolution.State != StandaloneGrammarState.Resolved && Holds
				? StandaloneGrammarConcern.Full
				: StandaloneGrammarConcern.None;

		switch (Kind)
		{
			case WorkspaceChangeKind.DocumentChanged:
				return watched && resolution.State != StandaloneGrammarState.Resolved
					? StandaloneGrammarConcern.Document
					: StandaloneGrammarConcern.None;

			case WorkspaceChangeKind.AdditionalDocumentChanged:
			case WorkspaceChangeKind.AnalyzerConfigDocumentChanged:
				return StandaloneGrammarConcern.None;

			default:
				return watched ? StandaloneGrammarConcern.Full : StandaloneGrammarConcern.None;
		}
	}

	static bool AddsOrRemoves(WorkspaceChangeKind kind)
	{
		switch (kind)
		{
			case WorkspaceChangeKind.ProjectAdded:
			case WorkspaceChangeKind.ProjectRemoved:
			case WorkspaceChangeKind.ProjectReloaded:
			case WorkspaceChangeKind.DocumentAdded:
			case WorkspaceChangeKind.DocumentRemoved:
			case WorkspaceChangeKind.DocumentReloaded:
			case WorkspaceChangeKind.AdditionalDocumentAdded:
			case WorkspaceChangeKind.AdditionalDocumentRemoved:
			case WorkspaceChangeKind.AdditionalDocumentReloaded:
				return true;

			default:
				return false;
		}
	}

	static bool HeldIn(Solution solution, ProjectId project, string filePath)
	{
		return solution.GetProject(project)?.AdditionalDocuments.Any(document =>
			string.Equals(document.FilePath, filePath, StringComparison.OrdinalIgnoreCase)) == true;
	}
}

/// <summary>
/// The changes to the solution that come while a standalone grammar's host is looked for. A
/// resolution reads the solution as it is when it starts, and a change that comes while it runs
/// is judged against the resolution before it, which may not know what the change is about: an
/// included grammar is not among a not-ready resolution's documents. Such a change is kept, and
/// what it asks is asked again of the resolution that comes out.
/// </summary>
/// <remarks>
/// Kept are the changes, not the solutions they carry: one entry for each kind of change to each
/// document or project, however often it comes. Not thread-safe; its owner calls it under its lock,
/// the one under which it cancels a resolution and starts the next, so that a token not cancelled
/// there is the latest resolution's, and judges a change against its resolution under it too.
/// </remarks>
sealed class StandaloneGrammarChanges
{
	readonly HashSet<StandaloneGrammarChange> _kept = [];
	CancellationToken                         _running;
	bool                                      _isRunning;

	/// <summary>How many changes are kept: one for each kind of change to each document or project.</summary>
	public int Kept => _kept.Count;

	/// <summary>
	/// A resolution starts reading the solution: what was kept is in what it reads. False for one
	/// already cancelled: a newer one has started, or is about to, and nothing here is its own.
	/// </summary>
	public bool Start(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return false;

		_running   = cancellationToken;
		_isRunning = true;
		_kept.Clear();

		return true;
	}

	/// <summary>
	/// A change that is no concern of the resolution it was judged against: kept while a resolution
	/// runs, unless it is the grammar's own text, which is no concern of any.
	/// </summary>
	public void Keep(StandaloneGrammarChange change)
	{
		if (_isRunning && !change.OwnText)
			_kept.Add(change);
	}

	/// <summary>
	/// The running resolution ended with <paramref name="resolution"/>: what the changes kept meanwhile
	/// ask of it, the documents to look at added to <paramref name="documents"/>. Nothing for a
	/// resolution that is not the running one.
	/// </summary>
	public StandaloneGrammarConcern End(
		CancellationToken cancellationToken,
		StandaloneGrammarResolution resolution,
		ICollection<DocumentId> documents)
	{
		if (!IsRunning(cancellationToken))
			return StandaloneGrammarConcern.None;

		_isRunning = false;

		var result = StandaloneGrammarConcern.None;

		foreach (var change in _kept)
		{
			var concern = change.Concerns(resolution);

			if (concern == StandaloneGrammarConcern.Full)
			{
				result = StandaloneGrammarConcern.Full;
				break;
			}

			if (concern == StandaloneGrammarConcern.Document)
			{
				documents.Add(change.Document!);
				result = StandaloneGrammarConcern.Document;
			}
		}

		_kept.Clear();

		return result;
	}

	/// <summary>
	/// The running resolution ended with nothing to show for it: the resolution before stands, and
	/// so does what was judged against it.
	/// </summary>
	public void Abandon(CancellationToken cancellationToken)
	{
		if (!IsRunning(cancellationToken))
			return;

		_isRunning = false;
		_kept.Clear();
	}

	/// <summary>The document is closed: nothing more is looked for.</summary>
	public void Clear()
	{
		_isRunning = false;
		_kept.Clear();
	}

	bool IsRunning(CancellationToken cancellationToken)
	{
		return _isRunning && _running == cancellationToken && !cancellationToken.IsCancellationRequested;
	}
}
