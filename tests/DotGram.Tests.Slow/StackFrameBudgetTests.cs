using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every grammar we ship stays inside the stack budget at the interval the generator probes at.
/// </summary>
/// <remarks>
/// <para>
/// The emitted reader probes the stack once in <see cref="Interval"/> entries to a rule that can
/// reach itself, and that number is only safe for a grammar whose frames are narrow enough. The
/// arithmetic is written where the interval is set (<c>Machine.Reader.cs</c>); this is the half of
/// it that cannot be written down, because <b>bytes-a-level is a property of the code the JIT
/// makes from what the emitter wrote</b>, not of the emitter. It varies five-fold between our own
/// two SQL dialects. So it is measured, here, on every grammar we have.
/// </para>
/// <para>
/// <b>What fails this test is a grammar whose frame has grown</b> — a rule given more locals, an
/// alternative that spills more, a tree type that went back to being a struct — and what it buys
/// is that such a grammar is a red test rather than a consumer's process dying on a deep input.
/// If it fails, the interval is what has to move, not this bound.
/// </para>
/// <para>
/// It is deliberately measured in whatever configuration the suite runs in. Debug frames are about
/// twenty per cent wider than Release, and a consumer debugs their application; the numbers the
/// interval was derived from are the Debug ones for that reason.
/// </para>
/// <para>
/// The levels quoted in the generator's own remark — 5.40 Debug, 4.50 Release — were taken
/// before <c>bafdcf00</c> made the towers' value types classes. The current levels are 4.15 and
/// 3.36, so that arithmetic reserves against a level the parser no longer has. This test measures
/// the level afresh on every run, so it is never reading a stale one.
/// </para>
/// <para>
/// Those are Windows x64 figures. On Linux x64 the same Release tier-0 frames are narrower —
/// SQL:2023 2.38 KiB a level against 3.36, about a hundred bytes on each of its ten frames a
/// level. That is the size of what the Windows calling convention adds to a frame (the 32-byte
/// home area of every call, RSI and RDI saved by the callee), which is the likely account; it has
/// not been measured frame by frame, and the two figures are from different machines.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class StackFrameBudgetTests
{
	/// <summary>What one probe to the next may cost, in KiB, before the guard can be stepped over.</summary>
	/// <remarks>
	/// <b>114.4 KiB is the room a RECURSING reader has past the refusal</b>, measured by spending
	/// it and bisecting on survival (sql-47, 2026-09-24). It is not the 128 KiB
	/// <c>TryEnsureSufficientExecutionStack</c> asks for, which counts the operating system's
	/// guard page.
	/// <para>
	/// <b>And it is not a constant of the runtime: it depends on how the stack is spent.</b> The
	/// same room spent in one <c>stackalloc</c> rather than in recursion is 97.4 KiB, because
	/// Windows commits ahead of a growing stack and moves the guard as it goes, so where the
	/// fatal page lands depends on the touching pattern. A reader recurses, so 114.4 is the
	/// figure that applies here — and it may not be carried to an instrument that does not.
	/// </para>
	/// </remarks>
	const double Budget = 114.4;

	/// <summary>What <c>Deepen</c> needs of that budget, on the call that costs most.</summary>
	/// <remarks>
	/// Measured on the emitted <c>Deepen</c> by ballast injected at its entry and bisected on
	/// survival, at three ballast widths so the burner's own overhead is solved for rather than
	/// assumed: <b>27.8 KiB on the first call in a process</b> and 5.1 KiB on every call after.
	/// A bare <c>new Thread(16 MB)</c> with Start and Join is only 2.7 KiB, so nearly all of the
	/// cold figure is compiling the hand-off path, on the leaving thread at its deepest point.
	/// <para>
	/// <b>The cold one is what a budget has to carry</b>: a process's first deep input is exactly
	/// when it is paid. An earlier version of this file used 67.8, which was the upper end of a
	/// bracket taken by subtracting one instrument's ceiling from another instrument's arm — a
	/// subtraction that does not hold across two burners, and the number was wrong by 2.4x.
	/// </para>
	/// <para>
	/// <b>It is used here for every grammar, and that needed its own witness.</b> The emitter
	/// writes one <c>Deepen</c> per grammar, carrying that grammar's own state, failure and
	/// registers, so a figure taken on <c>SqlStandardParser</c> had no right to stand as a
	/// constant for all readers. Bisected the same way on T-SQL — a reader of a wholly different
	/// size and carried set — it comes out identical to within one ballast level, about 0.35 KiB
	/// (248 levels against 249 at 256 B, 77 against 77 at 1 KiB).
	/// </para>
	/// <para>
	/// And the mechanism says why, which is what makes it sound rather than lucky: what the cold
	/// call compiles is the hand-off path, and that path is emitted from one template in every
	/// grammar — the grammar-specific part of it is field copying, which is cheap to compile. So
	/// the term is <b>stable rather than guaranteed</b>: a grammar carrying a very much larger
	/// register set would compile a fatter <c>Deepen</c>, and what would catch that is this same
	/// bisect run on such a reader.
	/// </para>
	/// </remarks>
	const double HandOff = 27.8;

	/// <summary>How much of the level this holds in hand for a machine that is not this one.</summary>
	/// <remarks>
	/// <b>Chosen, not measured.</b> Everything above is measured on this machine, this runtime and
	/// this JIT, and bytes-a-level is the term most likely to differ elsewhere — it already moves
	/// 20% between Debug and Release and two to three times between tiers. Doubling the measured
	/// level before the arithmetic is what stands in for that, and it is a judgement rather than
	/// a finding, which is why it is a constant with a name instead of a factor buried in the
	/// assertion.
	/// </remarks>
	const double Reserve = 2;

	/// <summary>The generator's own interval, written out because <c>Machine</c> is internal to it.</summary>
	const int Interval = 4;

	/// <summary>A stack no parse will exhaust, so the measurement is of frames and not of the guard.</summary>
	const int MeasuringStackKiB = 63 * 1024;

	/// <summary>How deep the warming parse goes: enough to reach every rule the nest reaches.</summary>
	const int Warm = 8;

	/// <summary>How many fresh copies of a grammar may be spent on a reading the runtime did not disturb.</summary>
	const int Attempts = 3;

	public static TheoryData<string> Grammars()
	{
		return new("SQL:2023", "T-SQL", "expression language");
	}

	[Theory]
	[MemberData(nameof(Grammars))]
	public void A_grammar_costs_little_enough_a_level_to_be_probed_at_the_interval(string grammar)
	{
		var shallowDepth    = Shallow(grammar);
		var (shallow, deep) = Committed(grammar, shallowDepth);
		var level           = (deep - shallow) / (double)shallowDepth / 1024;

		Assert.True(
			level > 0,
			$"{grammar}: the level measured as {level:F2} KiB ({shallow:N0} B at {shallowDepth} levels, " +
			$"{deep:N0} B at {2 * shallowDepth}), which means the measurement failed rather than that " +
			"the frames are free.");

		var between = level * Reserve * Interval;

		Assert.True(
			between + HandOff <= Budget,
			$"{grammar}: {level:F2} KiB a level, so {between:F1} KiB between two probes at an " +
			$"interval of {Interval} with a reserve of {Reserve}x, and {between + HandOff:F1} KiB " +
			$"with the hand-off's {HandOff} " +
			$"against a budget of {Budget}. A grammar this wide can step over the guard between " +
			"two probes; the interval in Machine.Reader.cs is what has to come down.");
	}

	/// <summary>The shallower of the two depths the level is measured between; the deeper is twice it.</summary>
	/// <remarks>
	/// Far enough apart that the fixed part cancels, and shallow enough that a reading ends inside
	/// the tiering delay (<see cref="Committed"/>) even on a loaded machine: at 200 and 400 SQL:2023
	/// took 9 ms alone and 41 ms inside the whole suite. T-SQL is read shallower still because it
	/// refuses this input in time that grows with the cube of the depth at tier 0 — 1.9 s at 400
	/// levels, 14.6 s at 800 — and takes 20 ms at 40 and 80. Every grammar's level is flat in the
	/// depth from 25 levels up (T-SQL 1.28 to 1.40 KiB, the spread being one page in the
	/// difference), so the shallower pairs cost only resolution.
	/// </remarks>
	static int Shallow(string grammar)
	{
		return grammar == "T-SQL" ? 40 : 100;
	}

	/// <summary>What the stack of a thread had committed after the shallow parse, and after the deep one, in bytes.</summary>
	/// <remarks>
	/// <para>
	/// Both parses on ONE thread of its own, the shallow first: a stack commits as it grows and never
	/// gives the pages back, so the second reading is the first plus what the deeper parse needed, and
	/// nothing else.
	/// </para>
	/// <para>
	/// <b>And both on code the JIT compiled at tier 0, which a reading has to make sure of.</b>
	/// Tiered compilation promotes a method once it is hot, and a promoted frame is thinner: a
	/// third of the tier-0 one for T-SQL (0.41 KiB a level against 1.3). A reading that straddles a
	/// promotion reads a fat shallow parse and a thin deep one. That is what failed this test on
	/// CI — 0.00 KiB for the expression language in one run and not the next, and -1.28 KiB for
	/// SQL:2023 when the two depths still had threads of their own — and whether it happened
	/// depended on how long the grammar took and on what the suite had run before. So:
	/// </para>
	/// <list type="bullet">
	/// <item>The grammar is loaded afresh into a context of its own (<see cref="Cold"/>), so what
	/// the suite has already run is never what is measured. The context is not collectible: code in
	/// a collectible context is not tiered at all and is compiled optimized from the start, 1.78 KiB
	/// a level for SQL:2023 against 2.38 at tier 0.</item>
	/// <item>Parses on another thread compile it first, so no compilation lands on the measuring
	/// stack.</item>
	/// <item>The reading begins by calling a method nothing has called (<see cref="Marked"/>, in a
	/// fresh copy of this assembly). The runtime counts no call toward a promotion until
	/// <see cref="TieringDelay"/> has passed with no such first call, so a reading that ends
	/// inside the delay ran on tier-0 code alone; one that does not is taken again on a fresh
	/// copy. The bound is conservative: T-SQL read at growing depths first reads wrong at 630 ms.</item>
	/// </list>
	/// <para>
	/// Tier 0 is also the frame a consumer's first deep input runs on.
	/// </para>
	/// </remarks>
	static (long Shallow, long Deep) Committed(string grammar, int shallowDepth)
	{
		var took = TimeSpan.Zero;

		for (var attempt = 0; attempt < Attempts; attempt++)
		{
			var cold    = new Cold();
			var read    = Reader(cold, grammar);
			var marked  = Marker(cold);
			var shallow = 0L;
			var deep    = 0L;

			OnThread(
				() =>
				{
					for (var warming = 0; warming < 3; warming++)       // reflection settles on its second call
						read(Warm);

					_ = Reading();
				});

			OnThread(
				() =>
				{
					var mark = Stopwatch.GetTimestamp();

					marked();

					read(shallowDepth);
					shallow = Reading();

					read(2 * shallowDepth);
					deep = Reading();

					took = Stopwatch.GetElapsedTime(mark);
				});

			if (took < TieringDelay)
				return (shallow, deep);
		}

		throw new InvalidOperationException(
			$"{grammar}: none of {Attempts} readings ended inside the runtime's tiering delay of " +
			$"{TieringDelay.TotalMilliseconds} ms (the last took {took.TotalMilliseconds:F0} ms), so none is known " +
			"to be of tier-0 frames alone.");
	}

	/// <summary>The runtime's default <c>TC_CallCountingDelayMs</c>, which nothing in the suite changes.</summary>
	static readonly TimeSpan TieringDelay = TimeSpan.FromMilliseconds(100);

	/// <summary>A method of this assembly's copy in that context, which nothing has called: the first call of it restarts the runtime's tiering delay.</summary>
	static Action Marker(Cold cold)
	{
		var method = cold.LoadFromAssemblyPath(typeof(StackFrameBudgetTests).Assembly.Location)
			.GetType(typeof(StackFrameBudgetTests).FullName!, throwOnError: true)!
			.GetMethod(nameof(Marked), BindingFlags.NonPublic | BindingFlags.Static)!;

		return () => method.Invoke(null, null);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static void Marked()
	{
	}

	static void OnThread(Action action)
	{
		var thread = new Thread(() => action(), MeasuringStackKiB * 1024);

		thread.Start();
		thread.Join();
	}

	/// <summary>The nested input of each grammar, read by the copy in that context and refused in every case so the reading goes all the way down.</summary>
	static Action<int> Reader(Cold cold, string grammar)
	{
		var caller = typeof(StackFrameBudgetTests).Assembly;

		return grammar switch
		{
			"SQL:2023"            => Parse(cold, "DotGram.Sql", "DotGram.Sql.Standard.SqlStandardParser", "TryParseSearchCondition", [typeof(string)], depth => [new string('(', depth) + "a = 1"]),
			"T-SQL"               => Parse(cold, "DotGram.Sql", "DotGram.Sql.TransactSql.TransactSqlParser", "TryParseSearchCondition", [typeof(string)], depth => [new string('(', depth) + "a = 1"]),
			"expression language" => Parse(cold, "DotGram.ExpressionLanguage", "DotGram.ExpressionLanguage.ExpressionParser", "TryParse", [typeof(string), typeof(Assembly)], depth => ["(int x) => " + new string('(', depth) + "x", caller]),
			_                     => throw new ArgumentOutOfRangeException(nameof(grammar), grammar, "no such grammar"),
		};
	}

	static Action<int> Parse(Cold cold, string assembly, string type, string method, Type[] parameters, Func<int, object[]> arguments)
	{
		var parser = cold.LoadFromAssemblyName(new AssemblyName(assembly)).GetType(type, throwOnError: true)!;
		var parse  = parser.GetMethod(method, parameters) ?? throw new MissingMethodException(type, method);

		return depth => parse.Invoke(null, arguments(depth));
	}

	/// <summary>A context of its own for the DotGram assemblies, so their code is compiled afresh.</summary>
	/// <remarks>Everything else — the framework — is the default context's.</remarks>
	sealed class Cold() : AssemblyLoadContext(nameof(StackFrameBudgetTests))
	{
		protected override Assembly? Load(AssemblyName name)
		{
			return name.Name is { } simple && (simple == "DotGram" || simple.StartsWith("DotGram.", StringComparison.Ordinal))
				? LoadFromAssemblyPath(Path.Combine(AppContext.BaseDirectory, simple + ".dll"))
				: null;
		}
	}

	static long Reading()
	{
		return OperatingSystem.IsWindows() ? OnWindows() : OnLinux();
	}

	/// <summary>Walking this thread's own stack region and adding up what is committed.</summary>
	static long OnWindows()
	{
		GetCurrentThreadStackLimits(out var low, out var high);

		var used = 0L;

		for (var at = low; at < high;)
		{
			if (VirtualQuery(at, out var region, (nuint)Marshal.SizeOf<MemoryBasicInformation>()) == 0)
				break;

			if (region.State == 0x1000)            // MEM_COMMIT
				used += (long)region.RegionSize;

			at = region.BaseAddress + region.RegionSize;
		}

		return used;
	}

	/// <summary>How many bytes of this thread's own stack range are resident, which is the same quantity.</summary>
	/// <remarks>
	/// <para>
	/// A Linux thread stack is mapped whole up front and a page becomes resident when it is first
	/// touched, so residency is the high-water mark just as commitment is on Windows. The range is
	/// asked of the thread itself (<c>pthread_getattr_np</c>, which leaves out the guard) and counted
	/// page by page with <c>mincore</c>, so nothing depends on how the kernel lays out or merges
	/// mappings. Checked on a burner of known width: 256, 1,024 and 4,096 bytes a level read as the
	/// width plus the same 176 bytes of frame each time.
	/// </para>
	/// <para>
	/// Until 2026-09-28 the mapping was found in <c>/proc/self/smaps</c> by its size and its
	/// <c>Rss</c> read. The mapping is not the size asked for everywhere: glibc 2.43 on kernel 7.0
	/// keeps the guard page inside the stack's own mapping, which is then a page larger, where 2.39
	/// splits it off with <c>mprotect</c>. Nothing matched, both readings were 0, and every grammar
	/// read 0.00 KiB a level.
	/// </para>
	/// </remarks>
	static long OnLinux()
	{
		var attributes = Marshal.AllocHGlobal(1024);    // pthread_attr_t: 56 bytes on x64 glibc, 64 on arm64

		try
		{
			if (pthread_getattr_np(pthread_self(), attributes) != 0)
				throw new InvalidOperationException("pthread_getattr_np failed.");

			try
			{
				if (pthread_attr_getstack(attributes, out var low, out var size) != 0)
					throw new InvalidOperationException("pthread_attr_getstack failed.");

				var page  = Environment.SystemPageSize;
				var pages = new byte[(size + page - 1) / page];

				if (mincore(low, size, pages) != 0)
					throw new InvalidOperationException($"mincore failed with errno {Marshal.GetLastPInvokeError()}.");

				var resident = 0L;

				foreach (var state in pages)
					resident += state & 1;

				return resident * page;
			}
			finally
			{
				_ = pthread_attr_destroy(attributes);
			}
		}
		finally
		{
			Marshal.FreeHGlobal(attributes);
		}
	}

	[DllImport("libc")]
	static extern nint pthread_self();

	[DllImport("libc")]
	static extern int pthread_getattr_np(nint thread, nint attributes);

	[DllImport("libc")]
	static extern int pthread_attr_getstack(nint attributes, out nint low, out nint size);

	[DllImport("libc")]
	static extern int pthread_attr_destroy(nint attributes);

	[DllImport("libc", SetLastError = true)]
	static extern int mincore(nint address, nint length, byte[] pages);

	[DllImport("kernel32.dll")]
	static extern void GetCurrentThreadStackLimits(out nuint low, out nuint high);

	[DllImport("kernel32.dll")]
	static extern nuint VirtualQuery(nuint address, out MemoryBasicInformation buffer, nuint length);

	[StructLayout(LayoutKind.Sequential)]
	struct MemoryBasicInformation
	{
		public nuint BaseAddress;
		public nuint AllocationBase;
		public uint  AllocationProtect;
		public uint  PartitionId;
		public nuint RegionSize;
		public uint  State;
		public uint  Protect;
		public uint  Type;
	}
}
