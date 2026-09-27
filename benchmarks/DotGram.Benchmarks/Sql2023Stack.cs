using System;
using System.Runtime.InteropServices;
using System.Threading;

using DotGram.Sql.Standard;

namespace DotGram.Benchmarks;

/// <summary>
/// What SQL:2023's own <c>SearchCondition</c> costs a level of nesting to refuse, on the shipped
/// tape carrier (<see cref="SqlStandardParser"/>) beside the immediate one
/// (<see cref="ImmediateSqlStandard"/>): the same measurement <c>StackFrameBudgetTests</c> takes of
/// the shipped grammars (<c>tests/DotGram.Tests.Slow/StackFrameBudgetTests.cs</c>), adapted to hold
/// two carriers of the same grammar against each other rather than a carrier against a budget.
/// </summary>
/// <remarks>
/// The immediate carrier recurses through a factory call per alternative tried, where the tape
/// carrier recurses through the reader alone; if it costs more a level, its refusal depth is
/// correspondingly shallower for the same stack, which is part of what this comparison answers.
/// Prints, and asks nothing of a benchmark: <c>--sql2023-stack</c>.
/// </remarks>
static class Sql2023Stack
{
	const int Shallow = 400, Deep = 800;

	/// <summary>
	/// A stack no parse will exhaust, so the measurement is of frames and not of the guard. Odd
	/// enough a size to be found by, on Linux, where the mapping is matched by it.
	/// </summary>
	const int MeasuringStackKiB = 63 * 1024;

	static int _measured;

	public static void Run()
	{
		var (tapeShallow, tapeDeep)           = Committed(Tape);
		var (immediateShallow, immediateDeep) = Committed(Immediate);

		var tapeLevel      = (tapeDeep - tapeShallow) / (double)(Deep - Shallow) / 1024;
		var immediateLevel = (immediateDeep - immediateShallow) / (double)(Deep - Shallow) / 1024;

		Console.WriteLine("SQL:2023 SearchCondition, refused at two depths, stack committed on a thread of its own:");
		Console.WriteLine($"  tape:      shallow {tapeShallow,10:N0} B, deep {tapeDeep,10:N0} B, {tapeLevel,7:F2} KiB/level");
		Console.WriteLine($"  immediate: shallow {immediateShallow,10:N0} B, deep {immediateDeep,10:N0} B, {immediateLevel,7:F2} KiB/level");
		Console.WriteLine($"  ratio (immediate / tape): {immediateLevel / tapeLevel:F2}x");
	}

	/// <summary>Both parses on one thread of its own, the shallow first, so the second reading is the first plus what the deeper parse needed and nothing else (StackFrameBudgetTests.Committed's own note on why one thread, not two).</summary>
	static (long Shallow, long Deep) Committed(Func<int, bool> read)
	{
		var shallow = 0L;
		var deep    = 0L;
		var size    = (MeasuringStackKiB + 4 * Interlocked.Increment(ref _measured)) * 1024;

		var thread = new Thread(
			() =>
			{
				read(Shallow);
				shallow = OperatingSystem.IsWindows() ? OnWindows() : OnLinux(size);

				read(Deep);
				deep = OperatingSystem.IsWindows() ? OnWindows() : OnLinux(size);
			},
			size);

		thread.Start();
		thread.Join();

		return (shallow, deep);
	}

	static bool Tape(int depth)
	{
		return SqlStandardParser.TryParseSearchCondition(new string('(', depth) + "a = 1").IsSuccess;
	}

	static bool Immediate(int depth)
	{
		return ImmediateSqlStandard.TryParseSearchCondition(new string('(', depth) + "a = 1").IsSuccess;
	}

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

	static long OnLinux(int size)
	{
		var want  = (ulong)size;
		var found = false;

		foreach (var line in System.IO.File.ReadLines("/proc/self/smaps"))
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
