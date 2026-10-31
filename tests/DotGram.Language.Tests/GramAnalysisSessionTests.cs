using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// The analysis session's lifecycle, on a clock the test moves: what is analysed after a burst of
/// typing, what is dropped as stale, when the grammar's host is looked for, and when a grammar whose
/// host is not settled is told about as it stands.
/// </summary>
public sealed class GramAnalysisSessionTests
{
	const string FilePath = @"P:\Parsers\Dialect.gram";

	[Fact]
	public void Rapid_edits_publish_only_the_last_version()
	{
		var host    = new FakeHost("A = 'a'");
		var session = host.Session();
		var changed = new List<FakeVersion>();

		session.Changed += changed.Add;

		var first  = host.Edit(session, "A = 'b'");
		host.Scheduler.Advance(100);
		var second = host.Edit(session, "A = 'c'");
		host.Scheduler.Advance(100);
		var last   = host.Edit(session, "A = 'd'");

		Assert.Empty(host.Analysed);

		host.Scheduler.Advance(GramAnalysisSession.AnalysisDelayMilliseconds);

		Assert.Equal(["A = 'd'"], host.Analysed.Select(static item => item.Text));
		Assert.Same(last, session.Result?.Version);
		Assert.True(session.Result!.Timing.Published);
		Assert.Equal("A = 'd'".Length, session.Result.Timing.TextLength);

		// Each edit is told about at once, its analysis carried over; only the last is told about again.
		Assert.Equal([first, second, last, last], changed);
		Assert.Same(host.Documents[0], session.Document(last));
	}

	[Fact]
	public void A_result_for_a_superseded_version_is_dropped()
	{
		var host     = new FakeHost("A = 'a'");
		var session  = host.Session();
		var timings  = new List<GramAnalysisTiming>();
		FakeVersion? newer = null;

		session.Analyzed += timings.Add;

		// The buffer moves on while the first analysis runs.
		host.During = text =>
		{
			if (text == "A = 'b'")
				newer = host.Advance("A = 'c'");
		};

		var stale = host.Edit(session, "A = 'b'");
		host.Scheduler.Advance(GramAnalysisSession.AnalysisDelayMilliseconds);

		Assert.Null(session.Result);
		Assert.Equal([false], timings.Select(static timing => timing.Published));
		Assert.Empty(session.Document(stale).Classifications);

		host.During = null;
		session.Edited(stale, newer!);
		host.Scheduler.Advance(GramAnalysisSession.AnalysisDelayMilliseconds);

		Assert.Same(newer, session.Result?.Version);
		Assert.Equal([false, true], timings.Select(static timing => timing.Published));
	}

	[Fact]
	public void A_burst_of_solution_changes_looks_for_the_host_once()
	{
		var host    = new FakeHost("A = 'a'") { Resolution = Resolved() };
		var session = host.Session();

		Assert.True(session.Follow(FilePath));
		host.Scheduler.Advance(0);
		Assert.Equal(1, host.Resolves);

		using var workspace = new AdhocWorkspace();
		var solution = workspace.CurrentSolution;
		var project  = ProjectId.CreateNewId();

		// The solution reloaded, a project added, the host's declaration edited, within half a second.
		session.WorkspaceChanged(new WorkspaceChangeEventArgs(WorkspaceChangeKind.SolutionReloaded, solution, solution));
		host.Scheduler.Advance(200);
		session.WorkspaceChanged(new WorkspaceChangeEventArgs(WorkspaceChangeKind.ProjectChanged, solution, solution, Watched));
		host.Scheduler.Advance(200);
		session.WorkspaceChanged(new WorkspaceChangeEventArgs(
			WorkspaceChangeKind.DocumentChanged, solution, solution, project, HostDeclaration));

		host.Scheduler.Advance(GramAnalysisSession.ResolveDelayMilliseconds - 1);
		Assert.Equal(1, host.Resolves);

		host.Scheduler.Advance(1);
		Assert.Equal(2, host.Resolves);

		// A change to a project the grammar has nothing to do with looks for nothing.
		session.WorkspaceChanged(new WorkspaceChangeEventArgs(WorkspaceChangeKind.ProjectChanged, solution, solution, project));
		host.Scheduler.Advance(GramAnalysisSession.ResolveDelayMilliseconds * 4);
		Assert.Equal(2, host.Resolves);
	}

	[Fact]
	public void An_edit_while_the_host_is_not_ready_asks_the_document_before_the_solution()
	{
		var project = Watched;
		var host    = new FakeHost("A = 'a'") { Resolution = StandaloneGrammarResolution.NotReadyIn(project), MayHost = false };
		var session = host.Session();

		session.Follow(FilePath);
		host.Scheduler.Advance(0);

		using var workspace = new AdhocWorkspace();
		var solution = workspace.CurrentSolution;
		var edited   = DocumentId.CreateNewId(project);

		// Past the retries, so that only the edit can ask again.
		host.Scheduler.Advance(60_000);
		var before = host.Resolves;

		session.WorkspaceChanged(new WorkspaceChangeEventArgs(WorkspaceChangeKind.DocumentChanged, solution, solution, project, edited));
		host.Scheduler.Advance(GramAnalysisSession.ResolveDelayMilliseconds);

		Assert.Equal([edited], host.Asked.Single());
		Assert.Equal(before, host.Resolves);

		host.MayHost = true;
		session.WorkspaceChanged(new WorkspaceChangeEventArgs(WorkspaceChangeKind.DocumentChanged, solution, solution, project, edited));
		host.Scheduler.Advance(GramAnalysisSession.ResolveDelayMilliseconds);

		Assert.Equal(before + 1, host.Resolves);
	}

	[Fact]
	public void A_host_not_found_is_asked_again_and_the_grammar_told_about_as_it_stands_after_the_settling_time()
	{
		var host    = new FakeHost("A = 'a'") { Resolution = StandaloneGrammarResolution.NotReady };
		var session = host.Session();

		session.Follow(FilePath);
		host.Scheduler.Advance(0);
		session.Document(host.Current);
		host.Scheduler.Advance(0);

		// Quiet about what depends on the includes while the host may yet be found.
		Assert.Equal([true], host.Analysed.Select(static item => item.Suppressed));

		// Asked again after 1, 2, 4, 8 and 16 seconds, and then not again until something changes.
		var asked = new List<long>();
		for (var second = 1; second <= 40; second++)
		{
			var before = host.Resolves;
			host.Scheduler.Advance(1000);
			if (host.Resolves > before)
				asked.Add(host.Scheduler.Now / 1000);
		}

		Assert.Equal([1L, 3L, 7L, 15L, 31L], asked);

		// After twenty seconds the grammar is analysed once more, as it stands.
		Assert.Equal([true, false], host.Analysed.Select(static item => item.Suppressed));
	}

	[Fact]
	public void A_host_found_after_the_grammar_was_analysed_analyses_it_again_with_what_it_includes()
	{
		var host    = new FakeHost("A = 'a'") { Resolution = StandaloneGrammarResolution.NotReady };
		var session = host.Session();

		session.Follow(FilePath);
		host.Scheduler.Advance(0);
		session.Document(host.Current);
		host.Scheduler.Advance(0);

		host.Resolution = Resolved();
		host.Scheduler.Advance(1000);

		Assert.Equal(2, host.Analysed.Count);
		Assert.False(host.Analysed[1].Suppressed);
		Assert.NotNull(host.Analysed[1].Context);
		Assert.Equal(StandaloneGrammarState.Resolved, session.Result?.Resolution.State);
	}

	[Fact]
	public void A_copy_never_looks_for_a_host()
	{
		var host    = new FakeHost("A = 'a'") { Resolution = Resolved() };
		var session = host.Session();
		var copy    = host.Store.PathToOpen(new StandaloneIncludedGrammar("Sql92", "W = 'w'", null))!;

		Assert.False(session.Follow(copy));
		host.Scheduler.Advance(60_000);
		session.Document(host.Current);
		host.Scheduler.Advance(0);

		Assert.Equal(0, host.Resolves);
		Assert.Equal([true], host.Analysed.Select(static item => item.Suppressed));

		using var workspace = new AdhocWorkspace();
		session.WorkspaceChanged(new WorkspaceChangeEventArgs(
			WorkspaceChangeKind.SolutionChanged, workspace.CurrentSolution, workspace.CurrentSolution));
		host.Scheduler.Advance(60_000);

		Assert.Equal(0, host.Resolves);
	}

	[Fact]
	public void A_closed_document_looks_for_nothing_more()
	{
		var host    = new FakeHost("A = 'a'") { Resolution = StandaloneGrammarResolution.NotReady };
		var session = host.Session();

		session.Follow(FilePath);
		host.Scheduler.Advance(0);
		session.Close();
		host.Scheduler.Advance(60_000);

		Assert.Equal(1, host.Resolves);
	}

	[Fact]
	public void The_real_analysis_reports_its_parts_and_publishes_the_document()
	{
		var host    = new FakeHost("Digits = ['0'..'9']+\nparse Digits");
		var session = new GramAnalysisSession<FakeVersion>(host);

		session.Document(host.Current);
		host.Scheduler.Advance(0);

		var result = Assert.IsType<GramAnalysisResult<FakeVersion>>(session.Result);

		Assert.NotEmpty(result.Document.Classifications);
		Assert.True(result.Timing.Compile > TimeSpan.Zero);
		Assert.True(result.Timing.PostProcessing >= TimeSpan.Zero);
		Assert.True(result.Timing.AllocatedBytes > 0);
		Assert.Equal(result.Timing.Compile + result.Timing.PostProcessing, result.Timing.Total);
	}

	static ProjectId Watched { get; } = ProjectId.CreateNewId();

	static DocumentId HostDeclaration { get; } = DocumentId.CreateNewId(Watched);

	static StandaloneGrammarResolution Resolved()
	{
		var included = new StandaloneIncludedGrammar("Sql92", "W = 'w'", null);

		return new StandaloneGrammarResolution(
			StandaloneGrammarState.Resolved,
			new StandaloneGrammarContext("namespace Sql92\n{\nW = 'w'\n}\n", [included]),
			[Watched],
			[HostDeclaration]);
	}

	internal sealed class FakeVersion(string text)
	{
		public string Text { get; } = text;
	}

	sealed record Analysis(string Text, StandaloneGrammarContext? Context, bool Suppressed);

	sealed class FakeHost(string text) : IGramAnalysisHost<FakeVersion>
	{
		public VirtualScheduler Scheduler { get; } = new();

		public InMemoryIncludedGrammarStore Store { get; } = new();

		public FakeVersion Current { get; private set; } = new(text);

		public string? FilePath { get; set; } = GramAnalysisSessionTests.FilePath;

		public StandaloneGrammarResolution Resolution { get; set; } = StandaloneGrammarResolution.NotReady;

		public bool MayHost { get; set; }

		public int Resolves { get; private set; }

		public List<IReadOnlyList<DocumentId>> Asked { get; } = [];

		public List<Analysis> Analysed { get; } = [];

		public List<GramDocument> Documents { get; } = [];

		public Action<string>? During { get; set; }

		public List<Exception> Reported { get; } = [];

		IGramScheduler IGramAnalysisHost<FakeVersion>.Scheduler => Scheduler;

		IIncludedGrammarStore IGramAnalysisHost<FakeVersion>.Store => Store;

		public GramAnalysisSession<FakeVersion> Session()
		{
			return new GramAnalysisSession<FakeVersion>(this, Analyze);
		}

		public FakeVersion Advance(string next)
		{
			Current = new FakeVersion(next);
			return Current;
		}

		public FakeVersion Edit(GramAnalysisSession<FakeVersion> session, string next)
		{
			var before = Current;
			var after  = Advance(next);

			session.Edited(before, after);

			return after;
		}

		StandaloneAnalysis Analyze(string analysed, StandaloneGrammarContext? context, bool suppressed, GramAnalysisClock clock)
		{
			Analysed.Add(new Analysis(analysed, context, suppressed));
			During?.Invoke(analysed);

			var document = new GramDocument([new GramClassifiedSpan(0, 1, GramSyntaxKind.Identifier, null)], [], [], [], [], [], []);
			Documents.Add(document);

			return new StandaloneAnalysis(document, []);
		}

		public string Text(FakeVersion version)
		{
			return version.Text;
		}

		public GramDocument Translate(GramDocument document, FakeVersion from, FakeVersion to)
		{
			return document;
		}

		public Task<StandaloneGrammarResolution> ResolveAsync(string filePath, CancellationToken cancellationToken)
		{
			Resolves++;
			return Task.FromResult(Resolution);
		}

		public Task<bool> MayHostAsync(IReadOnlyList<DocumentId> documents, string filePath, CancellationToken cancellationToken)
		{
			Asked.Add(documents);
			return Task.FromResult(MayHost);
		}

		public Task BeforeChangedAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public void Report(Exception exception)
		{
			Reported.Add(exception);
		}
	}
}

/// <summary>A clock that moves only when told to, and runs what it starts at once, on the caller's thread.</summary>
sealed class VirtualScheduler : IGramScheduler
{
	readonly List<(long Due, long Order, TaskCompletionSource<bool> Done)> _timers = [];
	long _order;

	public long Now { get; private set; }

	public Task DelayAsync(int milliseconds, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return Task.FromCanceled(cancellationToken);

		var done = new TaskCompletionSource<bool>();

		cancellationToken.Register(() => done.TrySetCanceled(cancellationToken));
		_timers.Add((Now + milliseconds, _order++, done));

		return done.Task;
	}

	public void Run(Func<Task> work)
	{
		_ = work();
	}

	/// <summary>Moves the clock on, firing every timer that falls due on the way in the order it falls due.</summary>
	public void Advance(long milliseconds)
	{
		var target = Now + milliseconds;

		while (true)
		{
			_timers.RemoveAll(static timer => timer.Done.Task.IsCompleted);

			var due = _timers.Where(timer => timer.Due <= target).OrderBy(static timer => timer.Due).ThenBy(static timer => timer.Order).ToArray();

			if (due.Length == 0)
				break;

			var next = due[0];

			_timers.Remove(next);
			Now = Math.Max(Now, next.Due);
			next.Done.TrySetResult(true);
		}

		Now = target;
	}
}
