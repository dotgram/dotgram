using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// A standalone grammar's host looked for and looked for again, on a clock the test moves and
/// with every resolution held until the test lets it end with what it read: which resolutions
/// run, which are replaced, and what the host is found to be in the end.
/// </summary>
public sealed class StandaloneGrammarHostTrackerTests : IDisposable
{
	const string DialectPath = @"P:\Parsers\Dialect.gram";
	const string BasePath    = @"P:\Parsers\Base.gram";
	const string Original    = "Word = 'a'\n";
	const string Edited      = "Word = 'a'\nExtra = 'x'\n";

	const int ResolveDelay = 500;
	const int RetryDelay   = 1000;
	const int MaxDeferral  = 2000;

	readonly AdhocWorkspace _workspace = new();
	readonly ProjectId      _project;
	readonly DocumentId     _base;
	readonly DocumentId     _dialect;
	readonly DocumentId     _host;

	public StandaloneGrammarHostTrackerTests()
	{
		_project = _workspace.AddProject("Parsers", LanguageNames.CSharp).Id;
		_base    = DocumentId.CreateNewId(_project);
		_dialect = DocumentId.CreateNewId(_project);
		_host    = DocumentId.CreateNewId(_project);

		var solution = _workspace.CurrentSolution
			.AddAdditionalDocument(_base, "Base.gram", SourceText.From(Original), filePath: BasePath)
			.AddAdditionalDocument(_dialect, "Dialect.gram", SourceText.From("A = B.Word\n"), filePath: DialectPath)
			.AddDocument(_host, "Host.cs", SourceText.From("partial class Host;"));

		Assert.True(_workspace.TryApplyChanges(solution));
	}

	public void Dispose()
	{
		_workspace.Dispose();
	}

	/// <summary>
	/// The first resolution knows none of the grammars the host includes, so an edit to one of them
	/// while it reads the solution is no concern of it. Asked again of what it found, the edit sends
	/// the host to be looked for again, and the host is found with the edited text.
	/// </summary>
	[Fact]
	public void An_included_grammars_edit_during_the_first_resolution_is_read_by_the_next()
	{
		var editor  = new Editor();
		var tracker = editor.Tracker();

		tracker.Follow();
		Assert.Single(editor.Resolutions);

		tracker.WorkspaceChanged(EditBase());
		Assert.Equal(1, tracker.Kept);

		editor.Resolutions[0].SetResult(Found(Original));
		Assert.Equal(Original, tracker.Resolution.Context?.AnalysisTail);
		Assert.Equal(0, tracker.Kept);

		editor.Advance(ResolveDelay);
		Assert.Equal(2, editor.Resolutions.Count);

		editor.Resolutions[1].SetResult(Found(Edited));
		Assert.Equal(Edited, tracker.Resolution.Context?.AnalysisTail);
	}

	/// <summary>
	/// The same edit while the second resolution runs, the first one replaced by a change to the
	/// solution while it read: the first one's late end neither publishes nor lets go of what the
	/// second one keeps, and the host is found with the edited text in the end.
	/// </summary>
	[Fact]
	public void A_replaced_resolution_neither_publishes_nor_loses_the_newer_ones_edit()
	{
		var editor  = new Editor();
		var tracker = editor.Tracker();

		tracker.Follow();

		var solution = _workspace.CurrentSolution;

		tracker.WorkspaceChanged(new WorkspaceChangeEventArgs(WorkspaceChangeKind.SolutionChanged, solution, solution));
		editor.Advance(ResolveDelay);
		Assert.Equal(2, editor.Resolutions.Count);

		tracker.WorkspaceChanged(EditBase());

		editor.Resolutions[0].SetResult(Found(Original));
		Assert.Equal(StandaloneGrammarState.NotReady, tracker.Resolution.State);
		Assert.Equal(1, tracker.Kept);

		editor.Resolutions[1].SetResult(Found(Original));
		editor.Advance(ResolveDelay);
		Assert.Equal(3, editor.Resolutions.Count);

		editor.Resolutions[2].SetResult(Found(Edited));
		Assert.Equal(Edited, tracker.Resolution.Context?.AnalysisTail);
		Assert.Empty(editor.Reported);
	}

	/// <summary>
	/// A resolution that has taken the request to look for the host, replaced by a request that
	/// asks only to look at one document: the newer resolution takes both, and looks for the host
	/// although the document declares none. Its predecessor's late end, not ready, asks for no
	/// retry and cancels nothing.
	/// </summary>
	[Fact]
	public void What_a_replaced_resolution_had_taken_goes_to_the_one_that_replaces_it()
	{
		var editor  = new Editor();
		var tracker = editor.Tracker();

		tracker.Follow();
		editor.Resolutions[0].SetResult(StandaloneGrammarResolution.NotReadyIn(_project));

		// The retry takes the request to look for the host, and is held.
		editor.Advance(RetryDelay);
		Assert.Equal(2, editor.Resolutions.Count);

		// An edit of a document in the grammar's project: one document to look at.
		var solution = _workspace.CurrentSolution;

		tracker.WorkspaceChanged(new WorkspaceChangeEventArgs(
			WorkspaceChangeKind.DocumentChanged, solution, solution, _project, _host));

		editor.Advance(ResolveDelay);
		Assert.Equal(3, editor.Resolutions.Count);
		Assert.Empty(editor.Asked);

		editor.Resolutions[2].SetResult(Found(Edited));
		Assert.Equal(Edited, tracker.Resolution.Context?.AnalysisTail);

		editor.Resolutions[1].SetResult(StandaloneGrammarResolution.NotReady);
		editor.Advance(60_000);

		Assert.Equal(3, editor.Resolutions.Count);
		Assert.Equal(Edited, tracker.Resolution.Context?.AnalysisTail);
	}

	/// <summary>An edit of a document only, with no request to look for the host taken: the document is looked at first.</summary>
	[Fact]
	public void A_document_that_declares_no_host_is_looked_at_and_nothing_more()
	{
		var editor  = new Editor();
		var tracker = editor.Tracker();

		tracker.Follow();
		editor.Resolutions[0].SetResult(StandaloneGrammarResolution.NoHost(_project));

		var solution = _workspace.CurrentSolution;

		tracker.WorkspaceChanged(new WorkspaceChangeEventArgs(
			WorkspaceChangeKind.DocumentChanged, solution, solution, _project, _host));
		editor.Advance(ResolveDelay);

		Assert.Equal([_host], Assert.Single(editor.Asked));
		Assert.Single(editor.Resolutions);
	}

	/// <summary>Requests that come at once start one resolution, the last one's.</summary>
	[Fact]
	public void Requests_at_once_start_one_resolution()
	{
		var editor  = new Editor();
		var tracker = editor.Tracker();

		tracker.Follow();

		var solution = _workspace.CurrentSolution;

		for (var index = 0; index < 3; index++)
			tracker.WorkspaceChanged(new WorkspaceChangeEventArgs(WorkspaceChangeKind.SolutionChanged, solution, solution));

		editor.Advance(ResolveDelay);

		Assert.Equal(2, editor.Resolutions.Count);
	}

	/// <summary>
	/// What is kept is one entry for each kind of change to each document, however often it comes,
	/// and never the grammar's own text; once the grammar is closed nothing is kept or published.
	/// </summary>
	[Fact]
	public void What_is_kept_is_bounded_and_let_go_on_close()
	{
		var editor  = new Editor();
		var tracker = editor.Tracker();

		tracker.Follow();

		for (var index = 0; index < 100; index++)
			tracker.WorkspaceChanged(EditBase());

		tracker.WorkspaceChanged(Edit(_dialect));
		Assert.Equal(1, tracker.Kept);

		tracker.Close();
		Assert.Equal(0, tracker.Kept);

		editor.Resolutions[0].SetResult(Found(Original));
		tracker.WorkspaceChanged(EditBase());
		editor.Advance(60_000);

		Assert.Equal(StandaloneGrammarState.NotReady, tracker.Resolution.State);
		Assert.Single(editor.Resolutions);
	}

	/// <summary>
	/// A resolver that answers the moment it is cancelled, on the cancelling thread: the generation it
	/// belongs to is replaced before it is cancelled, so its answer is not published, and nothing is
	/// called out under the tracker's lock.
	/// </summary>
	[Fact]
	public void A_resolution_that_answers_as_it_is_cancelled_publishes_nothing()
	{
		var editor  = new Editor();
		var tracker = editor.Tracker();

		editor.Answer = cancellationToken =>
		{
			var answer = new TaskCompletionSource<StandaloneGrammarResolution>();

			cancellationToken.Register(() => answer.TrySetResult(Found(Original)));

			return answer.Task;
		};

		tracker.Follow();
		editor.Answer = null;

		var solution = _workspace.CurrentSolution;

		tracker.WorkspaceChanged(new WorkspaceChangeEventArgs(WorkspaceChangeKind.SolutionChanged, solution, solution));

		Assert.Empty(editor.Published);
		Assert.Equal(StandaloneGrammarState.NotReady, tracker.Resolution.State);

		editor.Advance(ResolveDelay);
		editor.Resolutions[0].SetResult(Found(Edited));

		Assert.Equal(Edited, Assert.Single(editor.Published).Context?.AnalysisTail);
		Assert.Empty(editor.Reported);
	}

	/// <summary>
	/// Changes that concern the host coming faster than the solution settles, for 40 seconds: a
	/// generation still starts no later than the limit after the first request it answers.
	/// </summary>
	[Fact]
	public void A_stream_of_changes_that_never_pauses_still_has_the_host_looked_for()
	{
		var editor  = new Editor();
		var tracker = editor.Tracker();

		tracker.Follow();
		editor.Resolutions[0].SetResult(Found(Original));

		var solution = _workspace.CurrentSolution;

		for (var index = 0; index < 100; index++)
		{
			tracker.WorkspaceChanged(new WorkspaceChangeEventArgs(WorkspaceChangeKind.SolutionChanged, solution, solution));
			editor.Advance(400);

			// The first request of the stream came at 0; the limit runs out at 2 seconds.
			if (index == 3)
				Assert.Single(editor.Resolutions);

			if (index == 4)
				Assert.Equal(2, editor.Resolutions.Count);
		}

		Assert.True(editor.Resolutions.Count >= 1 + 100 * 400 / MaxDeferral, $"{editor.Resolutions.Count} resolutions in 40 seconds.");
	}

	WorkspaceChangeEventArgs EditBase()
	{
		return Edit(_base);
	}

	WorkspaceChangeEventArgs Edit(DocumentId document)
	{
		var before = _workspace.CurrentSolution;
		var after  = before.WithAdditionalDocumentText(document, SourceText.From(Edited));

		return new WorkspaceChangeEventArgs(WorkspaceChangeKind.AdditionalDocumentChanged, before, after, _project, document);
	}

	StandaloneGrammarResolution Found(string tail)
	{
		return new StandaloneGrammarResolution(
			StandaloneGrammarState.Resolved,
			new StandaloneGrammarContext(tail, [new StandaloneIncludedGrammar("B", tail, BasePath)]),
			[_project],
			[_base, _host]);
	}

	/// <summary>The editor around the tracker: a clock that moves when told, and resolutions held until let go.</summary>
	sealed class Editor
	{
		readonly List<(long Due, long Order, TaskCompletionSource<bool> Done)> _timers = [];
		long _now;
		long _order;
		StandaloneGrammarHostTracker? _tracker;

		public List<TaskCompletionSource<StandaloneGrammarResolution>> Resolutions { get; } = [];

		public List<IReadOnlyList<DocumentId>> Asked { get; } = [];

		public List<Exception> Reported { get; } = [];

		public StandaloneGrammarHostTracker Tracker()
		{
			_tracker = new StandaloneGrammarHostTracker(
				new StandaloneGrammarHostServices(
					FilePath: static () => DialectPath,
					ResolveAsync: (_, cancellationToken) =>
					{
						if (Answer is { } answer)
							return answer(cancellationToken);

						var resolution = new TaskCompletionSource<StandaloneGrammarResolution>();

						Resolutions.Add(resolution);

						return resolution.Task;
					},
					MayHostAsync: (documents, _, _) =>
					{
						Asked.Add([.. documents]);

						return Task.FromResult(false);
					},
					Now: () => _now,
					DelayAsync: DelayAsync,
					Run: static work => _ = work(),
					Resolved: (before, after) =>
					{
						Unlocked();
						Published.Add(after);
					},
					Recoverable: static exception => exception is not OutOfMemoryException,
					Report: Reported.Add),
				ResolveDelay,
				RetryDelay,
				retryCount: 5,
				MaxDeferral);

			return _tracker;
		}

		/// <summary>What the tracker's resolver answers in place of a held resolution; null holds it.</summary>
		public Func<CancellationToken, Task<StandaloneGrammarResolution>>? Answer { get; set; }

		public List<StandaloneGrammarResolution> Published { get; } = [];

		/// <summary>The tracker's lock is free: another thread can read through it at once.</summary>
		void Unlocked()
		{
			Assert.True(
				Task.Run(() => _tracker!.Kept).Wait(TimeSpan.FromSeconds(10)),
				"The tracker called out under its lock.");
		}

		/// <summary>Moves the clock on, firing every timer that falls due on the way in the order it falls due.</summary>
		public void Advance(long milliseconds)
		{
			var target = _now + milliseconds;

			while (true)
			{
				var due = _timers
					.Where(timer => timer.Due <= target)
					.OrderBy(static timer => timer.Due)
					.ThenBy(static timer => timer.Order)
					.FirstOrDefault();

				if (due.Done is null)
					break;

				_timers.Remove(due);
				_now = due.Due;
				due.Done.TrySetResult(true);
			}

			_now = target;
		}

		Task DelayAsync(int milliseconds, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
				return Task.FromCanceled(cancellationToken);

			var done = new TaskCompletionSource<bool>();

			cancellationToken.Register(() => done.TrySetCanceled(cancellationToken));
			_timers.Add((_now + milliseconds, _order++, done));

			return done.Task;
		}
	}
}
