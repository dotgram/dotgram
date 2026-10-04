using System;
using System.Collections.Generic;
using System.Threading;

using DotGram.Grammar;

using DotGram.Generation;

using System.Reflection;

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
[Collection(typeof(Alone))]
public sealed class StackDepthTests
{
	/// <summary>
	/// Small on purpose, and the smallest one is the part that does the work.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A runner's own thread is one or four megabytes, which is where this defect hid: the
	/// reserve is a constant, so a smaller stack only reaches the cliff sooner and never changes
	/// whether there is one.
	/// </para>
	/// <para>
	/// <b>64 KiB is what would catch an interval creeping back, and the larger sizes on their
	/// own would not.</b> On a stack that small the guard has to divert within a few levels,
	/// whatever the grammar and whatever the tier, so the reading either hands itself to
	/// <c>Deepen</c> and returns or it takes the process down. Measured by sql-47 on the fixed
	/// guard: a thousand levels on 64 KiB touches 44 KiB and answers the same refusal the
	/// recursive path gives; at 256 KiB it touches 164, which is the guard letting it recurse
	/// until the margin and only then switching. With the old interval the same input on 256 KiB
	/// died — so a test that only ran at 256 and above could pass while an interval that is 1.7x
	/// too loose sat in the generator, and a test at 64 could not.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData(64)]
	[InlineData(128)]
	[InlineData(256)]
	[InlineData(512)]
	[InlineData(1024)]
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
	/// And on both carriers, because the guard is the reader's and each carrier writes its own.
	/// </summary>
	/// <remarks>
	/// The sweep above reads SQL:2023, which is carried on the tape; a grammar whose values are
	/// built where they are read goes through a different emitted reader, with its own probe
	/// sites and its own frames. A grammar small enough to compile here stands in for both: what
	/// is being tested is that the guard fires and the reading is handed on, not how wide any
	/// particular frame is — <c>StackFrameBudgetTests</c> holds the widths.
	/// <para>
	/// 64 KiB again, for the reason given above: on a stack that small the guard has to divert
	/// within a few levels whatever the carrier, so either the reading returns or the process is
	/// gone and there is nothing to argue about.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData(CarrierKind.Tape)]
	[InlineData(CarrierKind.Immediate)]
	public void Either_carrier_hands_a_deep_reading_on_rather_than_running_out(CarrierKind carrier)
	{
		var host = Compile(carrier);

		foreach (var depth in new[] { 50, 200, 300, 800, 1_500 })
		{
			var text    = new string('(', depth) + "x";
			var refused = default(bool?);
			var thrown  = default(Exception);

			var thread = new Thread(
				() =>
				{
					try
					{
						refused = !EmittedCode.Match(host, "Carried.Probe", "TryParseStart", text).IsSuccess;
					}
					catch (Exception caught)
					{
						thrown = caught;
					}
				},
				64 * 1024);

			thread.Start();
			thread.Join();

			Assert.True(
				thrown is null,
				$"{carrier} at {depth} levels on 64 KiB threw {thrown?.GetType().Name}: {thrown?.Message}");

			Assert.True(
				refused is true,
				$"{carrier} at {depth} levels on 64 KiB was " +
				$"{(refused is null ? "never answered" : "ACCEPTED")}; an unclosed parenthesis is refused " +
				"at every depth on every carrier.");
		}
	}

	/// <summary>A grammar that recurses on its own opening bracket, carried as asked.</summary>
	static Assembly Compile(CarrierKind carrier, string grammar = Nested, string? members = null)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions
			{
				ClassName     = "Probe",
				Namespace     = "Carried",
				Carrier       = carrier,
				CSharpScanner = RoslynCSharpScanner.Instance,
			});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(Assert.Single(result.Sources).Text, "Probe", "Carried", members);
	}

	const string Nested =
		"""
		Start : @int = '(' & n: Start & ')' => @(n + 1)
			 | 'x' => @(1)
		parse Start
		""";

	/// <summary>
	/// A level that enters the probing rule three times — twice for something small beside the
	/// recursion, once for the recursion — is handed on wherever the stack runs low, on every carrier.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The probe fires once in four entries (<c>Machine.Interval</c>), and a reading carried onto
	/// another stack enters its rule there once more, so a level of three entries and one carried
	/// is four: once the probe has found the stack low on one of the small readings beside the
	/// recursion, it finds it there again every level after, the small reading is carried off and
	/// comes back, and the recursion goes on down the low stack until it overflows. T-SQL's
	/// <c>CASE … ELSE CASE …</c> did exactly this at ten thousand levels on a megabyte.
	/// </para>
	/// <para>
	/// Where the probe first finds the stack low decides whether it locks onto a small reading, and
	/// that is the depth the stack runs out at, so the size of the stack is swept rather than chosen.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData(CarrierKind.Tape)]
	[InlineData(CarrierKind.Immediate)]
	public void A_level_the_probe_keeps_missing_is_still_handed_on(CarrierKind carrier)
	{
		var host  = Compile(carrier, Beside);
		var depth = 5_000;
		var text  = "x;" + string.Concat(System.Linq.Enumerable.Repeat("cxtxe", depth)) + "x";

		foreach (var stackKb in new[] { 64, 80, 96, 112, 128, 160, 192, 256 })
		{
			var levels = default(int?);
			var thrown = default(Exception);

			var thread = new Thread(
				() =>
				{
					try
					{
						levels = (int)EmittedCode.Match(host, "Carried.Probe", "TryParseTop", text).Value!;
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
				$"{carrier} at {depth} levels on {stackKb} KiB threw {thrown?.GetType().Name}: {thrown?.Message}");

			Assert.Equal(depth, levels);
		}
	}

	/// <summary>
	/// A reading handed to another thread runs in the context of the parse that handed it over — its
	/// <c>AsyncLocal</c> values and its culture — and nothing of one parse is seen by the next, though the
	/// thread it is handed to is the same one.
	/// </summary>
	/// <remarks>
	/// The thread a reading is handed to lingers and takes the next hand-off from the same thread. Made
	/// by the first parse, it began in that parse's context; without the context captured at every
	/// hand-off, the second parse's semantic actions read the first one's values. On the tape the
	/// values are built after the reading, on the caller's own thread, so only the carrier that builds
	/// them where it reads them can see the difference; both are held to it.
	/// </remarks>
	[Theory]
	[InlineData(CarrierKind.Tape)]
	[InlineData(CarrierKind.Immediate)]
	public void A_reading_handed_off_runs_in_the_context_of_the_parse_that_handed_it(CarrierKind carrier)
	{
		var host    = Compile(carrier, Seeing, SeeingMembers);
		var ambient = (AsyncLocal<string>)host.GetType("Carried.Probe")!.GetField("Ambient")!.GetValue(null)!;
		var text    = new string('(', 2_000) + "x" + new string(')', 2_000);
		var seen    = new List<object?>();
		var thrown  = default(Exception);

		var thread = new Thread(
			() =>
			{
				try
				{
					foreach (var (value, culture) in new[] { ("first", "fr-FR"), ("second", "de-DE"), (null, "") })
					{
						ambient.Value = value!;
						System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo(culture);

						seen.Add(EmittedCode.Match(host, "Carried.Probe", "TryParseStart", text).Value);
					}
				}
				catch (Exception caught)
				{
					thrown = caught;
				}
			},
			128 * 1024);

		thread.Start();
		thread.Join();

		Assert.Null(thrown);
		Assert.Equal(["first/fr-FR", "second/de-DE", "/"], seen);
	}

	/// <summary>
	/// A thread interrupted while it waits for a reading it handed off still waits for it to end, and
	/// the next hand-off from that thread reads its own input rather than taking the last one's end.
	/// </summary>
	/// <remarks>
	/// The interrupt used to end the wait at once. The reading went on over there, and when it ended it
	/// left its completion behind; the next hand-off, reusing the same thread within its linger, took
	/// that completion as its own, came back before anything had been read, and cleared fields the
	/// reading still had in hand. Now the interrupt is held until the reading is done and thrown then,
	/// which is what is held first: when it is thrown, the reading it interrupted has ended. Whether the
	/// next hand-off then came back too soon depends on timing the test cannot fix, so that is asserted
	/// only as its answer.
	/// The leaf of the first reading sleeps, so the interrupt lands while the wait is on, and says when
	/// it is done, so the next hand-off is made while the thread it went to is waiting for another.
	/// </remarks>
	[Fact]
	public void An_interrupted_hand_off_leaves_nothing_behind_for_the_next()
	{
		var host  = Compile(CarrierKind.Immediate, Seeing, SeeingMembers);
		var pause = host.GetType("Carried.Probe")!.GetField("Pause")!;
		var left  = (ManualResetEventSlim)host.GetType("Carried.Probe")!.GetField("Left")!.GetValue(null)!;
		var first = default(Exception);
		var next  = default((bool IsSuccess, object? Value, string? Error, long Position));
		var other = default(Exception);
		var ended = false;

		pause.SetValue(null, 300);

		var thread = new Thread(
			() =>
			{
				System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

				try
				{
					EmittedCode.Match(host, "Carried.Probe", "TryParseStart", new string('(', 2_000) + "x" + new string(')', 2_000));
				}
				catch (TargetInvocationException caught)
				{
					first = caught.InnerException;
					ended = left.IsSet;
				}

				// Until the reading has ended over there, and its thread waits for another, which it does for
				// a tenth of a second: the next hand-off goes to it. Its leaf sleeps too, so a wait that took
				// the old completion comes back long before it.
				pause.SetValue(null, 50);
				left.Wait();
				Thread.Sleep(50);

				try
				{
					next = EmittedCode.Match(host, "Carried.Probe", "TryParseStart", new string('(', 1_500) + "x" + new string(')', 1_500));
				}
				catch (Exception caught)
				{
					other = caught;
				}
			},
			128 * 1024);

		thread.Start();
		Thread.Sleep(100);
		thread.Interrupt();
		thread.Join();

		Assert.IsType<ThreadInterruptedException>(first);
		Assert.True(ended, "the interrupt came back before the reading it interrupted had ended");
		Assert.Null(other);
		Assert.True(next.IsSuccess, next.Error);
		Assert.Equal("/", next.Value);
	}

	/// <summary>A nest whose innermost value says what it saw of its caller's context.</summary>
	const string Seeing =
		"""
		Start : @string = '(' & n: Start & ')' => @(n)
			 | 'x' => @(Seen())
		parse Start
		""";

	const string SeeingMembers =
		"""
		public static readonly global::System.Threading.AsyncLocal<string> Ambient = new global::System.Threading.AsyncLocal<string>();
		public static int Pause;
		public static readonly global::System.Threading.ManualResetEventSlim Left = new global::System.Threading.ManualResetEventSlim();
		static string Seen()
		{
			if (Pause > 0)
			{
				global::System.Threading.Thread.Sleep(Pause);
				Left.Set();
			}

			return Ambient.Value + "/" + global::System.Globalization.CultureInfo.CurrentCulture.Name;
		}
		""";

	/// <summary>A recursion with two small readings of the same rule beside it at every level.</summary>
	const string Beside =
		"""
		Top : @int = a: Start & ';' & b: Start => @(a + b)
		Start : @int = 'c' & s: Start & 't' & u: Start & 'e' & n: Start => @(n + 1)
			 | 'x' => @(0)
		parse Top
		""";

	/// <summary>
	/// Around and far past where the old form died, and up the far side of it. 250 and 300 are
	/// the two depths that were measured dying; 1,168 is the one that was measured surviving,
	/// and it is here so that a future interval cannot be tuned to pass the small end alone.
	/// </summary>
	static IEnumerable<int> Depths()
	{
		// 200 and 300 are not part of the sweep's rhythm, they are the two that killed an
		// interval of 16 -- 200 on a 512 KiB stack and 300 on a 1 MiB one -- while the depths
		// either side of each lived. They are named so that a future sweep cannot step over
		// them, since what decides is the phase and not the depth.
		yield return 200;
		yield return 300;

		for (var depth = 50; depth <= 400; depth += 25)
			yield return depth;

		yield return 600;
		yield return 800;
		yield return 1_168;
		yield return 1_500;
	}
}
