using System;
using System.Diagnostics;
using System.Threading;

using Xunit;

namespace DotGram.LanguageServer.Tests;

/// <summary>What runs MSBuild for the workspace: a cancellation ends the wait, whatever the process left behind.</summary>
public sealed class ProjectEvaluationTests
{
	/// <summary>
	/// A process that leaves a child holding its output open — a build server, a project's own tool —
	/// ends; a cancellation then ends the wait for that output too, rather than waiting for the child.
	/// </summary>
	[Fact]
	public void A_cancellation_ends_the_wait_for_output_a_child_holds_open()
	{
		Assert.SkipWhen(OperatingSystem.IsWindows(), "The child is started by sh.");

		using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

		var clock = Stopwatch.StartNew();

		Assert.Throws<OperationCanceledException>(() =>
			ProjectEvaluation.Run("sh", Environment.CurrentDirectory, ["-c", "sleep 60 & echo started"], cancellation.Token));

		Assert.True(clock.Elapsed < TimeSpan.FromSeconds(20), $"The wait took {clock.Elapsed.TotalSeconds:0} s.");
	}
}
