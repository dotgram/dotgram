using System;
using System.Runtime.InteropServices;
using System.Threading;

using DotGram.Sql.Standard;

namespace DotGram.Benchmarks;

/// <summary>
/// What SQL:2023's own <c>SearchCondition</c> costs a level of nesting to refuse, on the shipped
/// immediate carrier (<see cref="SqlStandardParser"/>) beside the tape
/// (<see cref="TapeSqlStandard"/>): the same measurement <c>StackFrameBudgetTests</c> takes of
/// the shipped grammars (<c>tests/DotGram.Tests.Slow/StackFrameBudgetTests.cs</c>), adapted to hold
/// two carriers of the same grammar against each other rather than a carrier against a budget.
/// </summary>
/// <remarks>
/// The immediate carrier recurses through a factory call per alternative tried, where the tape
/// carrier recurses through the reader alone; if it costs more a level, its refusal depth is
/// correspondingly shallower for the same stack, which is part of what this comparison answers.
/// Prints, and asks nothing of a benchmark: <c>--sql2023-stack</c>.
/// <para>
/// <b>It runs under <c>DOTNET_TieredCompilation=0</c></b>: a process started without the variable
/// starts itself again with it (Program.cs); one that sets it gets what it set. Unlike the test,
/// this does not pin the
/// reading to tier 0 on a fresh copy of the grammar: with tiering on, the shallow parse can run on
/// tier-0 code and the deep one on promoted code, whose frames are thinner, and the level then
/// reads low, zero or negative. With tiering off both run on optimized code, which is a steady
/// state and good for comparing two carriers, but is narrower than the tier-0 frames the test
/// holds to the budget.
/// </para>
/// </remarks>
static class Sql2023Stack
{
	const int Shallow = 400, Deep = 800;

	/// <summary>
	/// A stack no parse will exhaust, so the measurement is of frames and not of the guard.
	/// </summary>
	const int MeasuringStackKiB = 63 * 1024;

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

		var thread = new Thread(
			() =>
			{
				read(Shallow);
				shallow = OperatingSystem.IsWindows() ? OnWindows() : OnLinux();

				read(Deep);
				deep = OperatingSystem.IsWindows() ? OnWindows() : OnLinux();
			},
			MeasuringStackKiB * 1024);

		thread.Start();
		thread.Join();

		return (shallow, deep);
	}

	static bool Tape(int depth)
	{
		return TapeSqlStandard.TryParseSearchCondition(new string('(', depth) + "a = 1").IsSuccess;
	}

	static bool Immediate(int depth)
	{
		return SqlStandardParser.TryParseSearchCondition(new string('(', depth) + "a = 1").IsSuccess;
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

	/// <summary>
	/// The resident bytes of this thread's own stack range (StackFrameBudgetTests.OnLinux's note on
	/// why not a mapping found by its size).
	/// </summary>
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
