using System;
using System.Collections.Generic;
using System.Threading;

using DotGram.Sql.Standard;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A deeply nested input is refused, on a thread far smaller than the one a test runner
/// happens to give, and at every depth rather than at one.
/// </summary>
/// <remarks>
/// <para>
/// The emitted reader probes the stack and, when it is low, carries the reading onto a stack
/// of its own. Until 2026-09-24 it probed once in sixty-four entries, and the arithmetic of
/// that never worked: <c>TryEnsureSufficientExecutionStack</c> reserves <b>128 KiB</b> — the
/// same 128 on a 256 KiB stack, a 1 MiB stack and a 4 MiB one — while one nesting level of
/// the SQL:2023 reader is ten frames and <b>2.38 KiB</b>. Sixty-four levels is 152 KiB, so
/// the guard was stepped over and the process died.
/// </para>
/// <para>
/// <b>What it cost is the reason this test is worth its time.</b> One of the refusal ladders
/// drives that reader to three hundred levels, so <c>DotGram.Tests.Slow</c> died on every run
/// — and the runner printed a summary of whatever had finished, "total: 28, failed: 0" beside
/// an exit code of 0xC00000FD. A suite that never completed was read as a suite that passed,
/// for months.
/// </para>
/// <para>
/// <b>Why a sweep and not a depth.</b> While the interval existed, survival did not track
/// depth: at 256 KiB, depth 250 died and 1,168 survived, because what mattered was the phase
/// of the probe counter, which the parse carried with it from every site it had already
/// passed. A test that picked a depth would have passed on the defect at most depths. The
/// counter is gone, so the phase is gone with it — and the sweep stays, because it is what
/// would catch an interval being reintroduced under any future constant.
/// </para>
/// <para>
/// <b>What this asserts, and what only the process can assert.</b> A stack overflow cannot be
/// caught: it tears the process down, and nothing here runs afterwards. So the assertion that
/// the guard holds is the suite still being alive to report — and, because that is invisible
/// when it passes, every reading is also held to a definite verdict, which catches a guard
/// that survives by answering something else. Run under either parallel mode: the stack that
/// matters is the one this test asks for, not the one the runner would have handed it.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class StackDepthTests
{
	/// <summary>
	/// Small on purpose. A runner's own thread is one or four megabytes, which is where this
	/// defect hid: the reserve is a constant, so a smaller stack only reaches the cliff sooner
	/// and never changes whether there is one.
	/// </summary>
	[Theory]
	[InlineData(256)]
	[InlineData(512)]
	public void A_deeply_nested_input_is_refused_rather_than_taking_the_process_with_it(int stackKb)
	{
		foreach (var depth in Depths())
		{
			var text    = new string('(', depth) + "a = 1";
			var refused = default(bool?);
			var thrown  = default(Exception);

			var thread = new Thread(
				() =>
				{
					try
					{
						refused = !SqlStandardParser.TryParseSearchCondition(text).IsSuccess;
					}
					catch (Exception caught)
					{
						thrown = caught;
					}
				},
				stackKb * 1024);

			thread.Start();
			thread.Join();

			Assert.True(
				thrown is null,
				$"{depth} levels on a {stackKb} KiB stack threw {thrown?.GetType().Name}: {thrown?.Message}");

			Assert.True(
				refused is true,
				$"{depth} levels on a {stackKb} KiB stack was {(refused is null ? "never answered" : "ACCEPTED")}; " +
				"an unclosed parenthesis is refused at every depth, and a reading that answers something " +
				"else here is a guard that saved the process by changing the verdict.");
		}
	}

	/// <summary>
	/// Around and far past where the old form died, and up the far side of it. 250 and 300 are
	/// the two depths that were measured dying; 1,168 is the one that was measured surviving,
	/// and it is here so that a future interval cannot be tuned to pass the small end alone.
	/// </summary>
	static IEnumerable<int> Depths()
	{
		for (var depth = 50; depth <= 400; depth += 25)
			yield return depth;

		yield return 600;
		yield return 800;
		yield return 1_168;
		yield return 1_500;
	}
}
