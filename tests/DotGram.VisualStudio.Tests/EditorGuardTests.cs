using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.VisualStudio;

using Xunit;

namespace DotGram.VisualStudio.Tests;

/// <summary>
/// What every entry point of the extension is run in. The activity log it writes to needs a
/// running Visual Studio; here the messages are collected instead, and each test reads only the
/// ones naming its own operation.
/// </summary>
public sealed class EditorGuardTests
{
	static readonly ConcurrentQueue<string> Messages = new();

	static EditorGuardTests()
	{
		EditorGuard.Write = Messages.Enqueue;
	}

	static string[] Logged(string operation)
	{
		return Messages.Where(message => message.StartsWith(operation + " failed: ", StringComparison.Ordinal)).ToArray();
	}

	[Fact]
	public void ReturnsWhatTheBodyReturnsAndLogsNothing()
	{
		Assert.Equal(42, EditorGuard.Run("guard value", 0, () => 42, TestContext.Current.CancellationToken));
		Assert.Empty(Logged("guard value"));
	}

	[Fact]
	public void LogsAFailureWithItsOperationAndReturnsTheFallback()
	{
		var result = EditorGuard.Run<int>(
			"guard failure",
			-1,
			() => throw new InvalidOperationException("boom in the body"),
			TestContext.Current.CancellationToken);

		Assert.Equal(-1, result);
		var message = Assert.Single(Logged("guard failure"));
		Assert.Contains("System.InvalidOperationException: boom in the body", message);
	}

	[Fact]
	public void CancellationByTheCallersTokenIsNotLogged()
	{
		using var source = new CancellationTokenSource();
		source.Cancel();

		var result = EditorGuard.Run<int>(
			"guard cancelled",
			-1,
			() => throw new OperationCanceledException(source.Token),
			source.Token);

		Assert.Equal(-1, result);
		Assert.Empty(Logged("guard cancelled"));
	}

	[Fact]
	public void CancellationNobodyAskedForIsLogged()
	{
		var result = EditorGuard.Run<int>(
			"guard stray cancellation",
			-1,
			() => throw new OperationCanceledException(),
			CancellationToken.None);

		Assert.Equal(-1, result);
		Assert.Single(Logged("guard stray cancellation"));
	}

	[Fact]
	public void OutOfMemoryIsNotCaught()
	{
		Assert.Throws<OutOfMemoryException>(() => EditorGuard.Run("guard memory", () => throw new OutOfMemoryException()));
		Assert.Empty(Logged("guard memory"));
	}

	[Fact]
	public async Task AnAsynchronousFailureIsLoggedAndAnsweredWithTheFallback()
	{
		var result = await EditorGuard.RunAsync<string?>(
			"guard async failure",
			null,
			async () =>
			{
				await Task.Yield();
				throw new InvalidOperationException("boom after a yield");
			},
			CancellationToken.None);

		Assert.Null(result);
		Assert.Contains("boom after a yield", Assert.Single(Logged("guard async failure")));
	}

	[Fact]
	public async Task AnAsynchronousCancellationByTheCallerIsNotLogged()
	{
		using var source = new CancellationTokenSource();
		source.Cancel();

		var result = await EditorGuard.RunAsync(
			"guard async cancelled",
			"fallback",
			async () =>
			{
				await Task.Delay(1000, source.Token);
				return "done";
			},
			source.Token);

		Assert.Equal("fallback", result);
		Assert.Empty(Logged("guard async cancelled"));
	}

	[Fact]
	public async Task WorkNobodyAwaitsNeverFaultsAndItsFailureIsLogged()
	{
		var synchronous  = EditorGuard.RunAsync("guard forgotten sync", () => throw new InvalidOperationException("thrown before any await"), CancellationToken.None);
		var asynchronous = EditorGuard.RunAsync(
			"guard forgotten async",
			async () =>
			{
				await Task.Yield();
				throw new InvalidOperationException("thrown after an await");
			},
			CancellationToken.None);

		await Task.WhenAll(synchronous, asynchronous);

		Assert.Equal(TaskStatus.RanToCompletion, synchronous.Status);
		Assert.Equal(TaskStatus.RanToCompletion, asynchronous.Status);
		Assert.Contains("thrown before any await", Assert.Single(Logged("guard forgotten sync")));
		Assert.Contains("thrown after an await", Assert.Single(Logged("guard forgotten async")));
	}

	// A tagger's GetTags is an iterator: its body runs while Visual Studio enumerates the result,
	// after a guard around the call that made it has returned. The guard is on the enumeration.
	[Fact]
	public void AnIteratorFailingMidwayKeepsWhatItYieldedAndIsLogged()
	{
		var disposed = false;

		IEnumerable<int> Tags()
		{
			try
			{
				yield return 1;
				yield return 2;
				throw new InvalidOperationException("boom in the iterator");
			}
			finally
			{
				disposed = true;
			}
		}

		var tags = EditorGuard.Enumerate("guard iterator", Tags()).ToArray();

		Assert.Equal(new[] { 1, 2 }, tags);
		Assert.True(disposed);
		Assert.Contains("boom in the iterator", Assert.Single(Logged("guard iterator")));
	}

	[Fact]
	public void AnIteratorIsNotRunBeforeItIsEnumerated()
	{
		var started = false;

		IEnumerable<int> Tags()
		{
			started = true;
			yield return 1;
		}

		var tags = EditorGuard.Enumerate("guard lazy", Tags());

		Assert.False(started);
		Assert.Equal(new[] { 1 }, tags.ToArray());
		Assert.Empty(Logged("guard lazy"));
	}

	[Fact]
	public void ASequenceThatCannotBeEnumeratedIsEmpty()
	{
		Assert.Empty(EditorGuard.Enumerate("guard enumerator", new Unenumerable()));
		Assert.Single(Logged("guard enumerator"));
	}

	[Fact]
	public void AFailureToLogIsSwallowed()
	{
		var write = EditorGuard.Write;
		EditorGuard.Write = _ => throw new InvalidOperationException("the log is gone");

		try
		{
			Assert.Equal(-1, EditorGuard.Run<int>(
				"guard no log",
				-1,
				() => throw new InvalidOperationException("boom"),
				TestContext.Current.CancellationToken));
		}
		finally
		{
			EditorGuard.Write = write;
		}
	}

	sealed class Unenumerable : IEnumerable<int>
	{
		public IEnumerator<int> GetEnumerator()
		{
			throw new InvalidOperationException("no enumerator");
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
