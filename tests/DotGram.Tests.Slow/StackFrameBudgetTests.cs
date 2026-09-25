using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using DotGram.ExpressionLanguage;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

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

	/// <summary>The sizes the level is measured between, far enough apart that the fixed part cancels.</summary>
	const int Shallow = 400, Deep = 800;

	/// <summary>
	/// A stack no parse will exhaust, so the measurement is of frames and not of the guard, and a
	/// size distinctive enough to find the mapping by on Linux.
	/// </summary>
	const int MeasuringStackKiB = 63 * 1024;

	public static TheoryData<string> Grammars()
	{
		return new("SQL:2023", "T-SQL", "expression language");
	}

	[Theory]
	[MemberData(nameof(Grammars))]
	public void A_grammar_costs_little_enough_a_level_to_be_probed_at_the_interval(string grammar)
	{
		var (shallow, deep) = Committed(grammar);
		var level           = (deep - shallow) / (double)(Deep - Shallow) / 1024;

		Assert.True(
			level > 0,
			$"{grammar}: the level measured as {level:F2} KiB, which means the measurement failed " +
			"rather than that the frames are free.");

		var between = level * Reserve * Interval;

		Assert.True(
			between + HandOff <= Budget,
			$"{grammar}: {level:F2} KiB a level, so {between:F1} KiB between two probes at an " +
			$"interval of {Interval} with a reserve of {Reserve}x, and {between + HandOff:F1} KiB " +
			$"with the hand-off's {HandOff} " +
			$"against a budget of {Budget}. A grammar this wide can step over the guard between " +
			"two probes; the interval in Machine.Reader.cs is what has to come down.");
	}

	/// <summary>What the stack of a thread had committed after the shallow parse, and after the deep one, in bytes.</summary>
	/// <remarks>
	/// <para>
	/// Both parses on ONE thread of its own, the shallow first: a stack commits as it grows and never
	/// gives the pages back, so the second reading is the first plus what the deeper parse needed, and
	/// nothing else. Until 2026-09-25 each depth had a thread of its own, and on Linux, where the
	/// mapping is found by its size, the second thread could read a mapping the first had left — a
	/// level of -1.28 KiB on a CI runner.
	/// </para>
	/// <para>
	/// And each measuring thread's stack is a size no other thread has had, for the same reason.
	/// </para>
	/// </remarks>
	static (long Shallow, long Deep) Committed(string grammar)
	{
		var shallow = 0L;
		var deep    = 0L;
		var size    = (MeasuringStackKiB + 4 * Interlocked.Increment(ref _measured)) * 1024;

		var thread = new Thread(
			() =>
			{
				Read(grammar, Shallow);
				shallow = OperatingSystem.IsWindows() ? OnWindows() : OnLinux(size);

				Read(grammar, Deep);
				deep = OperatingSystem.IsWindows() ? OnWindows() : OnLinux(size);
			},
			size);

		thread.Start();
		thread.Join();

		return (shallow, deep);
	}

	static int _measured;

	/// <summary>The nested input of each grammar, refused in every case so the reading goes all the way down.</summary>
	static bool Read(string grammar, int depth)
	{
		return grammar switch
		{
			"SQL:2023"            => SqlStandardParser.TryParseSearchCondition(new string('(', depth) + "a = 1").IsSuccess,
			"T-SQL"               => TransactSqlParser.TryParseSearchCondition(new string('(', depth) + "a = 1").IsSuccess,
			"expression language" => ExpressionParser.TryParse("(int x) => " + new string('(', depth) + "x").IsSuccess,
			_                     => throw new ArgumentOutOfRangeException(nameof(grammar), grammar, "no such grammar"),
		};
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

	/// <summary>
	/// The resident size of this thread's stack mapping, which is the same quantity.
	/// </summary>
	/// <remarks>
	/// Found by the mapping's SIZE rather than by an address or by the <c>[stack]</c> label: only
	/// the main thread's mapping is labelled, and taking the address of a local would need an
	/// unsafe context that no project here enables. <see cref="MeasuringStackKiB"/> is an odd
	/// enough size to be this thread's and nothing else's.
	/// </remarks>
	static long OnLinux(int size)
	{
		var want  = (ulong)size;
		var found = false;

		foreach (var line in File.ReadLines("/proc/self/smaps"))
		{
			var dash = line.IndexOf('-');

			if (dash > 0 && Uri.IsHexDigit(line[0]))
			{
				var from = Convert.ToUInt64(line[..dash], 16);
				var rest = line[(dash + 1)..];
				var end  = rest.IndexOf(' ');
				var to   = Convert.ToUInt64(end < 0 ? rest : rest[..end], 16);

				found = to - from == want;

				continue;
			}

			if (found && line.StartsWith("Rss:", StringComparison.Ordinal))
				return long.Parse(line[4..].Replace("kB", "").Trim()) * 1024;
		}

		return 0;
	}

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
