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
/// </remarks>
[Collection(nameof(Alone))]
public sealed class StackFrameBudgetTests
{
	/// <summary>What one probe to the next may cost, in KiB, before the guard can be stepped over.</summary>
	/// <remarks>
	/// 111 KiB is the budget, measured by spending it: 111 KiB past the refusal returns and 112
	/// kills the process, the same on a 256 KiB stack and a 1 MiB one (sql-47, 2026-09-24). It is
	/// neither the 128 KiB <c>TryEnsureSufficientExecutionStack</c> asks for, which counts the
	/// operating system's guard page, nor the 88-92 KiB of reserve below the pointer, which leaves
	/// out committed pages that are still free.
	/// <para>
	/// <see cref="HandOff"/> is what <c>Deepen</c> needs of that budget, and it is an upper bound
	/// rather than a figure: between 24.6 and 67.8 KiB, bracketed by which intervals live and die.
	/// A bare thread creation alone is about 21. The larger end is used here, so this test is
	/// strict in the direction that matters.
	/// </para>
	/// </remarks>
	const double Budget = 111;

	const double HandOff = 67.8;

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
		var shallow = Committed(grammar, Shallow);
		var deep    = Committed(grammar, Deep);
		var level   = (deep - shallow) / (double)(Deep - Shallow) / 1024;

		Assert.True(
			level > 0,
			$"{grammar}: the level measured as {level:F2} KiB, which means the measurement failed " +
			"rather than that the frames are free.");

		var between = level * Interval;

		Assert.True(
			between + HandOff <= Budget,
			$"{grammar}: {level:F2} KiB a level, so {between:F1} KiB between two probes at an " +
			$"interval of {Interval}, and {between + HandOff:F1} KiB with the hand-off's {HandOff} " +
			$"against a budget of {Budget}. A grammar this wide can step over the guard between " +
			"two probes; the interval in Machine.Reader.cs is what has to come down.");
	}

	/// <summary>What the stack of one parse of a nested input committed, in bytes.</summary>
	/// <remarks>
	/// On its own thread, so the high-water mark is that parse's and nothing else's, and read after
	/// the parse returns: a stack commits as it grows and never gives the pages back.
	/// </remarks>
	static long Committed(string grammar, int depth)
	{
		var committed = 0L;

		var thread = new Thread(
			() =>
			{
				Read(grammar, depth);

				committed = OperatingSystem.IsWindows() ? OnWindows() : OnLinux();
			},
			MeasuringStackKiB * 1024);

		thread.Start();
		thread.Join();

		return committed;
	}

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
	static long OnLinux()
	{
		var want  = (ulong)MeasuringStackKiB * 1024;
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
