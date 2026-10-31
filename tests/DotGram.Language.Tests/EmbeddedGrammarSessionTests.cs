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

	sealed class EmbeddedHost(Workspace workspace, string filePath, string text) : IEmbeddedGrammarHost<EmbeddedHost.Version>
	{
		public sealed class Version(string text)
		{
			public string Text { get; } = text;
		}

		public ThreadSafeVirtualScheduler Scheduler { get; } = new();

		public Version Current { get; private set; } = new(text);

		public string? FilePath => filePath;

		IGramScheduler IEmbeddedGrammarHost<Version>.Scheduler => Scheduler;

		public Version Edit(string next)
		{
			Current = new Version(next);
			return Current;
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
			return EmbeddedGrammarDocuments.WithText(workspace.CurrentSolution, filePath, version.Text);
		}

		public Task BeforeChangedAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public void Report(Exception exception)
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
