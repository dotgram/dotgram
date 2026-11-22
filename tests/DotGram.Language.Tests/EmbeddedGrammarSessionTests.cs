using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.Language.Tests;

public sealed class EmbeddedGrammarSessionTests
{
	[Fact]
	public void ProvisionalAnalysisKeepsTranslatedColorsForTemporarilyUnclassifiedText()
	{
		var previous = new[]
		{
			Classification(10, 4, GramSyntaxKind.Identifier),
			Classification(20, 5, GramSyntaxKind.String),
		};
		var provisional = new[] { Classification(10, 4, GramSyntaxKind.Identifier) };

		var merged = EmbeddedGrammarSession.MergeProvisionalClassifications(previous, provisional);

		Assert.Collection(
			merged,
			item => Assert.Equal(new TextSpan(10, 4), item.Span),
			item => Assert.Equal(new TextSpan(20, 5), item.Span));
	}

	[Theory]
	[InlineData(true, 0, 1, 0, 0, 0, true)]
	[InlineData(true, 0, 0, 1, 0, 0, true)]
	[InlineData(true, 0, 0, 0, 1, 0, true)]
	[InlineData(true, 0, 0, 0, 0, 1, true)]
	[InlineData(false, 0, 1, 1, 1, 1, false)]
	[InlineData(true, 1, 1, 1, 1, 1, false)]
	[InlineData(true, 0, 0, 0, 0, 0, false)]
	public void PreservesEmbeddedAnalysisAcrossTransientSyntaxErrors(
		bool hasSyntaxErrors,
		int analysisCount,
		int previousClassificationCount,
		int previousDslClassificationCount,
		int previousSymbolCount,
		int previousDslSiteCount,
		bool expected)
	{
		Assert.Equal(expected, EmbeddedGrammarSession.ShouldPreserveEmbeddedAnalysis(
			hasSyntaxErrors,
			analysisCount,
			previousClassificationCount,
			previousDslClassificationCount,
			previousSymbolCount,
			previousDslSiteCount));
	}

	static HostClassification Classification(int start, int length, GramSyntaxKind kind)
	{
		return new(new TextSpan(start, length), kind, null, null, default, null, 0, null);
	}

	/// <summary>
	/// A C# document's grammar is coloured from C# syntax first, then told about in full once Roslyn
	/// has a semantic model; an edit carries the colours over at once and drops what depends on
	/// meaning until the next analysis.
	/// </summary>
	[Fact]
	public void Colours_come_first_meaning_after_and_an_edit_keeps_the_colours()
	{
		const string source = """
			namespace DotGram
			{
				sealed class GramAttribute(string text) : System.Attribute;
			}

			[DotGram.Gram("Start = 'a'\nparse Start")]
			class Parser;
			""";

		using var workspace = new AdhocWorkspace();
		var project  = workspace.AddProject("Host", LanguageNames.CSharp)
			.AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
		var document = project.AddDocument("Parser.cs", SourceText.From(source), filePath: @"P:\Host\Parser.cs");
		Assert.True(workspace.TryApplyChanges(document.Project.Solution));

		var host    = new EmbeddedHost(workspace, @"P:\Host\Parser.cs", source);
		var changed = new SemaphoreSlim(0);
		var session = new EmbeddedGrammarSession<EmbeddedHost.Version>(host);

		session.Changed += _ => changed.Release();

		Assert.False(session.View(host.Current).Current);

		// After the pause, the colours from C# syntax alone; nothing that needs Roslyn's model yet.
		host.Scheduler.Advance(EmbeddedGrammarSession.AnalysisDelayMilliseconds);
		Assert.True(changed.Wait(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken));

		var provisional = session.View(host.Current);

		Assert.True(provisional.Current);
		Assert.Contains(provisional.Classifications, item => item.Kind == GramSyntaxKind.Keyword);
		Assert.Empty(provisional.Symbols);

		// After the semantic pause, everything.
		Assert.True(host.Scheduler.WaitForTimer(TimeSpan.FromSeconds(30)));
		host.Scheduler.Advance(EmbeddedGrammarSession.SemanticAnalysisDelayMilliseconds);
		Assert.True(changed.Wait(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken));

		var full = session.View(host.Current);

		Assert.Equal(["Start", "Start"], full.Symbols.Select(static symbol => symbol.Name));
		Assert.NotEmpty(full.Classifications);

		// An edit: the colours carried over at once, the symbols gone until the next analysis.
		var before = host.Current;
		var after  = host.Edit(source + "\n");

		session.Edited(before, after);
		Assert.True(changed.Wait(TimeSpan.Zero, TestContext.Current.CancellationToken));

		var edited = session.View(after);

		Assert.True(edited.Current);
		Assert.Equal(full.Classifications.Count, edited.Classifications.Count);
		Assert.Empty(edited.Symbols);
	}

	/// <summary>Both of the session's notices of an analysis, the colours and the meaning, are raised on the editor's main thread.</summary>
	[Fact]
	public void Changed_after_an_analysis_is_raised_on_the_editors_main_thread()
	{
		using var main      = new MainThread();
		using var workspace = new AdhocWorkspace();
		var host    = Host(workspace);
		var threads = new List<int>();
		var changed = new SemaphoreSlim(0);

		host.Main = main;

		var session = new EmbeddedGrammarSession<EmbeddedHost.Version>(host);

		session.Changed += _ =>
		{
			lock (threads)
				threads.Add(Environment.CurrentManagedThreadId);

			changed.Release();
		};

		host.Scheduler.Advance(EmbeddedGrammarSession.AnalysisDelayMilliseconds);
		Assert.True(changed.Wait(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken));
		Assert.True(host.Scheduler.WaitForTimer(TimeSpan.FromSeconds(30)));
		host.Scheduler.Advance(EmbeddedGrammarSession.SemanticAnalysisDelayMilliseconds);
		Assert.True(changed.Wait(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken));

		lock (threads)
			Assert.Equal([main.ManagedThreadId, main.ManagedThreadId], threads);
	}

	/// <summary>A burst of edits is analysed once, at its last version.</summary>
	[Fact]
	public void Rapid_edits_analyse_only_the_last_version()
	{
		using var workspace = new AdhocWorkspace();
		var host    = Host(workspace);
		var changed = new SemaphoreSlim(0);
		var session = new EmbeddedGrammarSession<EmbeddedHost.Version>(host);

		session.Changed += _ => changed.Release();

		var first = host.Current;
		var one   = host.Edit(HostSource + " ");
		session.Edited(first, one);
		host.Scheduler.Advance(100);
		var two   = host.Edit(HostSource + "  ");
		session.Edited(one, two);
		host.Scheduler.Advance(100);
		var last  = host.Edit(HostSource + "   ");
		session.Edited(two, last);

		host.Scheduler.Advance(EmbeddedGrammarSession.AnalysisDelayMilliseconds);
		Assert.True(changed.Wait(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken));

		Assert.Equal([last], host.Analysed);
		Assert.True(session.View(last).Current);
	}

	/// <summary>
	/// A question about a version the document has moved past — an editor's, about the place before the
	/// character just typed — is answered with nothing, and does not start that version's analysis in
	/// place of the current one's, which would then never be published.
	/// </summary>
	[Fact]
	public void Asking_about_a_superseded_version_leaves_the_current_analysis_alone()
	{
		using var workspace = new AdhocWorkspace();
		var host    = Host(workspace);
		var changed = new SemaphoreSlim(0);
		var session = new EmbeddedGrammarSession<EmbeddedHost.Version>(host);

		session.Changed += _ => changed.Release();

		var first = host.Current;
		var next  = host.Edit(HostSource + " ");

		session.Edited(first, next);

		Assert.False(session.View(first).Current);

		host.Scheduler.Advance(EmbeddedGrammarSession.AnalysisDelayMilliseconds);
		Assert.True(changed.Wait(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken));
		Assert.Equal([next], host.Analysed);
		Assert.True(session.View(next).Current);
	}

	/// <summary>An analysis of a version the document has moved past while it ran is not published.</summary>
	[Fact]
	public void A_result_for_a_superseded_version_is_dropped()
	{
		using var workspace = new AdhocWorkspace();
		var host    = Host(workspace);
		var changed = new SemaphoreSlim(0);
		var session = new EmbeddedGrammarSession<EmbeddedHost.Version>(host);
		var stale   = host.Current;
		EmbeddedHost.Version? newer = null;

		session.Changed += _ => changed.Release();
		host.During = version =>
		{
			if (version == stale)
				newer = host.Edit(HostSource + " ");
		};

		host.Scheduler.Advance(EmbeddedGrammarSession.AnalysisDelayMilliseconds);

		Assert.True(SpinWait.SpinUntil(() => host.AnalysedCount == 1, TimeSpan.FromSeconds(30)));
		Assert.False(changed.Wait(TimeSpan.FromMilliseconds(500), TestContext.Current.CancellationToken));
		Assert.False(session.View(stale).Current);

		host.During = null;
		session.Edited(stale, newer!);
		host.Scheduler.Advance(EmbeddedGrammarSession.AnalysisDelayMilliseconds);
		Assert.True(changed.Wait(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken));
		Assert.True(session.View(newer!).Current);
	}

	/// <summary>
	/// A document that names <c>[Gram</c> and yields no grammar — its project not loaded yet — is
	/// analysed again a few times, and then left until it changes.
	/// </summary>
	[Fact]
	public void A_document_that_names_Gram_and_yields_none_is_asked_again_a_few_times()
	{
		using var workspace = new AdhocWorkspace();
		var host    = Host(workspace, "// [Gram(...)] is what this file will hold.\nclass Parser;\n");
		var session = new EmbeddedGrammarSession<EmbeddedHost.Version>(host);

		// Every analysis and every retry waits on a timer of its own, and a timer is set from whatever
		// thread the work is on, so each step waits for the next one as long as a slow runner needs.
		// The last analysis takes the last timer, so a step ends as well when the analyses are all begun.
		while (true)
		{
			Assert.True(
				SpinWait.SpinUntil(() => host.AnalysedCount > EmbeddedGrammarSession.RetryCount || host.Scheduler.WaitForTimer(TimeSpan.Zero), TimeSpan.FromSeconds(30)),
				$"analysed {host.AnalysedCount} times, no timer pending");

			if (host.AnalysedCount > EmbeddedGrammarSession.RetryCount)
				break;

			host.Scheduler.Advance(EmbeddedGrammarSession.RetryDelayMilliseconds + EmbeddedGrammarSession.AnalysisDelayMilliseconds);
		}

		// The last analysis has begun; it is over when the session shows the document as current.
		Assert.True(SpinWait.SpinUntil(() => session.View(host.Current).Current, TimeSpan.FromSeconds(30)));
		Assert.False(host.Scheduler.WaitForTimer(TimeSpan.FromSeconds(1)));
		Assert.Equal(1 + EmbeddedGrammarSession.RetryCount, host.AnalysedCount);
	}

	const string Library = "namespace Library { [DotGram.Gram(\"Base.gram\", DefaultAlias = \"B\")] public partial class Base { } }";

	const string LibraryGrammarPath = @"P:\Library\Base.gram";

	/// <summary>
	/// A host project referencing a library whose class exports Base.gram; the host's C# document is
	/// <paramref name="source"/>. The library's grammar defines <c>FromLibrary</c>.
	/// </summary>
	static (EmbeddedHost Host, ProjectId Library, DocumentId Grammar) WithLibrary(AdhocWorkspace workspace, string source)
	{
		return WithLibrary(workspace, source, Library, out _, out _);
	}

	/// <param name="library">The library's C#.</param>
	/// <param name="hostProject">The host's project.</param>
	/// <param name="other">A second C# document of the host's project, empty: Parser.Other.cs.</param>
	static (EmbeddedHost Host, ProjectId Library, DocumentId Grammar) WithLibrary(
		AdhocWorkspace workspace, string source, string library, out ProjectId hostProject, out DocumentId other)
	{
		var markers  = DotGram.Grammar.GramCompiler.EmitMarkerAttributes();
		var solution = workspace.CurrentSolution;
		var libraryId = ProjectId.CreateNewId("Library");
		var host      = ProjectId.CreateNewId("Host");
		var grammar   = DocumentId.CreateNewId(libraryId);

		hostProject = host;
		other       = DocumentId.CreateNewId(host);

		ProjectInfo Project(ProjectId id, string name)
		{
			return ProjectInfo.Create(
				id, VersionStamp.Default, name, name, LanguageNames.CSharp,
				parseOptions: Microsoft.CodeAnalysis.CSharp.CSharpParseOptions.Default.WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview),
				metadataReferences: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location), MetadataReference.CreateFromFile(typeof(System.Diagnostics.CodeAnalysis.StringSyntaxAttribute).Assembly.Location)]);
		}

		solution = solution
			.AddProject(Project(libraryId, "Library"))
			.AddDocument(DocumentId.CreateNewId(libraryId), markers.HintName, SourceText.From(markers.Text))
			.AddDocument(DocumentId.CreateNewId(libraryId), "Base.cs", SourceText.From(library))
			.AddAdditionalDocument(grammar, "Base.gram", SourceText.From("Word = 'a'\nFromLibrary = 'l'\n"), filePath: LibraryGrammarPath)
			.AddProject(Project(host, "Host"))
			.AddProjectReference(host, new ProjectReference(libraryId))
			.AddDocument(DocumentId.CreateNewId(host), markers.HintName, SourceText.From(markers.Text))
			.AddDocument(DocumentId.CreateNewId(host), "Parser.cs", SourceText.From(source), filePath: HostPath)
			.AddDocument(other, "Parser.Other.cs", SourceText.From(""), filePath: @"P:\Host\Parser.Other.cs");

		Assert.True(workspace.TryApplyChanges(solution));

		return (new EmbeddedHost(workspace, HostPath, source), libraryId, grammar);
	}

	/// <summary>Runs the session's pending analyses to the end: a timer at a time, each told of.</summary>
	static EmbeddedGrammarView Analysed(EmbeddedHost host, EmbeddedGrammarSession<EmbeddedHost.Version> session, SemaphoreSlim changed)
	{
		while (host.Scheduler.WaitForTimer(TimeSpan.FromSeconds(5)))
		{
			host.Scheduler.Advance(EmbeddedGrammarSession.SemanticAnalysisDelayMilliseconds + EmbeddedGrammarSession.AnalysisDelayMilliseconds);
			Assert.True(changed.Wait(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken));
		}

		return session.View(host.Current);
	}

	/// <summary>
	/// A grammar in C# whose attribute is named by an alias includes a library's class as the build does,
	/// with the grammar the library exports: the attribute is known by what it is, not by how it is spelled.
	/// </summary>
	[Fact]
	public void A_grammar_whose_attribute_is_an_alias_includes_what_the_library_exports()
	{
		using var workspace = new AdhocWorkspace();

		var (host, _, _) = WithLibrary(workspace, """
			using G = DotGram.GramAttribute;

			[G("Start = B.FromLibrary\nparse Start")]
			[DotGram.GramInclude(typeof(Library.Base))]
			partial class Parser;
			""");
		var changed = new SemaphoreSlim(0);
		var session = new EmbeddedGrammarSession<EmbeddedHost.Version>(host);

		session.Changed += _ => changed.Release();

		var view = Analysed(host, session, changed);

		Assert.NotEmpty(view.Symbols);
		Assert.DoesNotContain(view.Diagnostics, static diagnostic => diagnostic.Diagnostic.Id == "GRAM3002");
	}

	/// <summary>
	/// The library's grammar a C# document's grammar includes is changed: the document is analysed
	/// again, and what the change took away is told of.
	/// </summary>
	[Fact]
	public void A_change_to_an_included_grammar_has_the_document_analysed_again()
	{
		using var workspace = new AdhocWorkspace();

		var (host, library, grammar) = WithLibrary(workspace, """
			[DotGram.Gram("Start = B.FromLibrary\nparse Start")]
			[DotGram.GramInclude(typeof(Library.Base))]
			partial class Parser;
			""");
		var changed = new SemaphoreSlim(0);
		var session = new EmbeddedGrammarSession<EmbeddedHost.Version>(host);

		session.Changed += _ => changed.Release();

		Assert.DoesNotContain(Analysed(host, session, changed).Diagnostics, static diagnostic => diagnostic.Diagnostic.Id == "GRAM3002");

		var before = workspace.CurrentSolution;

		Assert.True(workspace.TryApplyChanges(before.WithAdditionalDocumentText(grammar, SourceText.From("Word = 'a'\n"))));

		session.WorkspaceChanged(new WorkspaceChangeEventArgs(WorkspaceChangeKind.AdditionalDocumentChanged, before, workspace.CurrentSolution, library, grammar));

		Assert.Contains(Analysed(host, session, changed).Diagnostics, static diagnostic => diagnostic.Diagnostic.Id == "GRAM3002");
	}

	/// <summary>Applies a change to the workspace and tells the session of it, as the editor does.</summary>
	static void Apply(AdhocWorkspace workspace, EmbeddedGrammarSession<EmbeddedHost.Version> session, Func<Solution, Solution> change, WorkspaceChangeKind kind, ProjectId project, DocumentId? document)
	{
		var before = workspace.CurrentSolution;

		Assert.True(workspace.TryApplyChanges(change(before)));

		session.WorkspaceChanged(new WorkspaceChangeEventArgs(kind, before, workspace.CurrentSolution, project, document));
	}

	/// <summary>
	/// An included grammar of another project changes while an analysis is under way, after it has taken
	/// the solution: that analysis is not what is shown; the one after the change is.
	/// </summary>
	[Fact]
	public void A_change_while_an_analysis_runs_is_not_lost()
	{
		using var workspace = new AdhocWorkspace();

		var (host, library, grammar) = WithLibrary(workspace, """
			[DotGram.Gram("Start = B.FromLibrary\nparse Start")]
			[DotGram.GramInclude(typeof(Library.Base))]
			partial class Parser;
			""");
		var changed = new SemaphoreSlim(0);
		var session = new EmbeddedGrammarSession<EmbeddedHost.Version>(host);
		var once    = 0;

		session.Changed += _ => changed.Release();

		// The first analysis takes the solution; then the library's grammar loses the rule.
		host.Taken = () =>
		{
			if (Interlocked.Exchange(ref once, 1) == 0)
				Apply(workspace, session, solution => solution.WithAdditionalDocumentText(grammar, SourceText.From("Word = 'a'\n")), WorkspaceChangeKind.AdditionalDocumentChanged, library, grammar);
		};

		Assert.Contains(Analysed(host, session, changed).Diagnostics, static diagnostic => diagnostic.Diagnostic.Id == "GRAM3002");
	}

	/// <summary>
	/// A class of a library that does not export its grammar is included with the host project's file of
	/// its name, which is not there at first: when it is added, the document is analysed again.
	/// </summary>
	[Fact]
	public void A_grammar_file_added_to_the_host_project_has_the_document_analysed_again()
	{
		using var workspace = new AdhocWorkspace();

		var (host, _, _) = WithLibrary(
			workspace,
			"""
			[DotGram.Gram("Start = B.FromConsumer\nparse Start")]
			[DotGram.GramInclude(typeof(Library.Base))]
			partial class Parser;
			""",
			"namespace Library { [DotGram.Gram(\"Base.gram\", DefaultAlias = \"B\", ExportGrammar = false)] public partial class Base { } }",
			out var project,
			out _);
		var changed = new SemaphoreSlim(0);
		var session = new EmbeddedGrammarSession<EmbeddedHost.Version>(host);

		session.Changed += _ => changed.Release();

		Assert.Contains(Analysed(host, session, changed).Diagnostics, static diagnostic => diagnostic.Diagnostic.Id == "GRAM3002");

		var added = DocumentId.CreateNewId(project);

		Apply(
			workspace,
			session,
			solution => solution.AddAdditionalDocument(added, "Base.gram", SourceText.From("Word = 'a'\nFromConsumer = 'c'\n"), filePath: @"P:\Host\Base.gram"),
			WorkspaceChangeKind.AdditionalDocumentAdded,
			project,
			added);

		Assert.DoesNotContain(Analysed(host, session, changed).Diagnostics, static diagnostic => diagnostic.Diagnostic.Id == "GRAM3002");
	}

	/// <summary>
	/// The host's include written on another part of the class, in another file: taken away there, the
	/// document is analysed again, and what it took away is told of.
	/// </summary>
	[Fact]
	public void A_change_to_another_part_of_the_host_has_the_document_analysed_again()
	{
		using var workspace = new AdhocWorkspace();

		var (host, _, _) = WithLibrary(
			workspace,
			"""
			[DotGram.Gram("Start = B.FromLibrary\nparse Start")]
			partial class Parser;
			""",
			Library,
			out var project,
			out var other);
		var changed = new SemaphoreSlim(0);
		var session = new EmbeddedGrammarSession<EmbeddedHost.Version>(host);

		Assert.True(workspace.TryApplyChanges(workspace.CurrentSolution.WithDocumentText(other, SourceText.From("[DotGram.GramInclude(typeof(Library.Base))]\npartial class Parser;\n"))));

		session.Changed += _ => changed.Release();

		Assert.DoesNotContain(Analysed(host, session, changed).Diagnostics, static diagnostic => diagnostic.Diagnostic.Id == "GRAM3002");

		Apply(workspace, session, solution => solution.WithDocumentText(other, SourceText.From("partial class Parser;\n")), WorkspaceChangeKind.DocumentChanged, project, other);

		Assert.Contains(Analysed(host, session, changed).Diagnostics, static diagnostic => diagnostic.Diagnostic.Id == "GRAM3002");
	}

	const string HostSource = """
		namespace DotGram
		{
			sealed class GramAttribute(string text) : System.Attribute;
		}

		[DotGram.Gram("Start = 'a'\nparse Start")]
		class Parser;
		""";

	const string HostPath = @"P:\Host\Parser.cs";

	static EmbeddedHost Host(AdhocWorkspace workspace, string source = HostSource)
	{
		var project  = workspace.AddProject("Host", LanguageNames.CSharp)
			.AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
		var document = project.AddDocument("Parser.cs", SourceText.From(source), filePath: HostPath);
		Assert.True(workspace.TryApplyChanges(document.Project.Solution));

		return new EmbeddedHost(workspace, HostPath, source);
	}

	sealed class EmbeddedHost(Workspace workspace, string filePath, string text) : IEmbeddedGrammarHost<EmbeddedHost.Version>
	{
		public sealed class Version(string text)
		{
			public string Text { get; } = text;
		}

		public ThreadSafeVirtualScheduler Scheduler { get; } = new();

		volatile Version _current = new(text);

		public Version Current => _current;

		/// <summary>The versions an analysis was started on, in order.</summary>
		public List<Version> Analysed { get; } = [];

		/// <summary>Called as an analysis starts, on whatever thread it runs on.</summary>
		public Action<Version>? During { get; set; }

		/// <summary>Called once an analysis has taken the solution it reads.</summary>
		public Action? Taken { get; set; }

		public string? FilePath => filePath;

		IGramScheduler IEmbeddedGrammarHost<Version>.Scheduler => Scheduler;

		public Version Edit(string next)
		{
			_current = new Version(next);
			return _current;
		}

		public int AnalysedCount
		{
			get
			{
				lock (Analysed)
					return Analysed.Count;
			}
		}

		public string Text(Version version)
		{
			return version.Text;
		}

		public int Length(Version version)
		{
			return version.Text.Length;
		}

		public char At(Version version, int position)
		{
			return version.Text[position];
		}

		public TextSpan Translate(TextSpan span, Version from, Version to)
		{
			return span;
		}

		public Document? Document(Version version)
		{
			lock (Analysed)
				Analysed.Add(version);

			During?.Invoke(version);

			var document = EmbeddedGrammarDocuments.WithText(workspace.CurrentSolution, filePath, version.Text);

			Taken?.Invoke();

			return document;
		}

		/// <summary>The editor's main thread, where the session's event is raised; on the caller's thread where null.</summary>
		public MainThread? Main { get; set; }

		public async Task RaiseAsync(Action raise, CancellationToken cancellationToken)
		{
			if (Main is not null)
				await Main.SwitchTo();

			raise();
		}

		public void Report(string operation, Exception exception)
		{
			throw new InvalidOperationException("The session reported an exception.", exception);
		}
	}
}

/// <summary>
/// A clock that moves only when told to, for a session whose work goes on on other threads between the
/// waits: Roslyn answers some questions asynchronously, so a wait may be asked for from any thread.
/// </summary>
sealed class ThreadSafeVirtualScheduler : IGramScheduler
{
	readonly object _gate = new();
	readonly List<(long Due, long Order, TaskCompletionSource<bool> Done)> _timers = [];
	long _order;
	long _now;

	public long Now
	{
		get
		{
			lock (_gate)
				return _now;
		}
	}

	public Task DelayAsync(int milliseconds, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return Task.FromCanceled(cancellationToken);

		var done = new TaskCompletionSource<bool>();

		cancellationToken.Register(() => done.TrySetCanceled(cancellationToken));

		lock (_gate)
		{
			_timers.Add((_now + milliseconds, _order++, done));
			Monitor.PulseAll(_gate);
		}

		return done.Task;
	}

	public void Run(Func<Task> work)
	{
		_ = work();
	}

	/// <summary>Waits for a timer to be pending, which a session sets from whatever thread its work is on.</summary>
	public bool WaitForTimer(TimeSpan timeout)
	{
		lock (_gate)
		{
			var until = DateTime.UtcNow + timeout;

			while (!_timers.Any(static timer => !timer.Done.Task.IsCompleted))
			{
				var left = until - DateTime.UtcNow;

				if (left <= TimeSpan.Zero || !Monitor.Wait(_gate, left))
					return _timers.Any(static timer => !timer.Done.Task.IsCompleted);
			}

			return true;
		}
	}

	/// <summary>Moves the clock on, firing every timer that falls due on the way, each outside the lock.</summary>
	public void Advance(long milliseconds)
	{
		long target;

		lock (_gate)
			target = _now + milliseconds;

		while (true)
		{
			TaskCompletionSource<bool> next;

			lock (_gate)
			{
				_timers.RemoveAll(static timer => timer.Done.Task.IsCompleted);

				var due = _timers.Where(timer => timer.Due <= target).OrderBy(static timer => timer.Due).ThenBy(static timer => timer.Order).ToArray();

				if (due.Length == 0)
				{
					_now = target;
					return;
				}

				_timers.Remove(due[0]);
				_now = Math.Max(_now, due[0].Due);
				next = due[0].Done;
			}

			next.TrySetResult(true);
		}
	}
}
