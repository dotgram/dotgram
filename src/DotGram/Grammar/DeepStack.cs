using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace DotGram.Grammar;

/// <summary>
/// Recursion that goes on where the thread's stack runs out: on a thread of its own, with a stack
/// of <see cref="Size"/>, and back again.
/// </summary>
/// <remarks>
/// <para>
/// For the walks that are recursion by nature and that a grammar can make deep — the compilation
/// of the machine, a frame of kilobytes for every level of a rule and of what is written into it
/// in place, and the writing of a scanner. Each asks <see cref="Low"/> as it enters, and where the
/// stack it is on is low it hands the rest of the walk to <see cref="Run{T}"/>, which runs it on a
/// fresh thread and waits for it. How deep the walk may go is then a question of memory and not of
/// the stack the host happened to give the generator, which on Windows is a megabyte.
/// </para>
/// <para>
/// The calling thread waits for the one it started, so the walk is still one walk: nothing runs
/// beside it, what it writes is seen when it returns, and what it throws is thrown again where it
/// was called. Nothing the generator writes reads the thread's culture, which an analyzer may not,
/// so which thread wrote it does not matter.
/// </para>
/// <para>
/// <b>What is ambient goes with it, and it is one object.</b> Thread-static state is empty on the
/// new thread, so what a walk keeps per thread and that outlives a call that may move lives in
/// <see cref="Ambient"/>, which the new thread adopts: the same object on both, so what the walk
/// wrote there is there when it returns. The emitted output is held to be the same with and
/// without moving (<see cref="Always"/>).
/// </para>
/// </remarks>
static class DeepStack
{
	/// <summary>The stack of a thread a walk moves to: reserved, and committed only as it is used.</summary>
	const int Size = 8 * 1024 * 1024;

	/// <summary>
	/// Every other check says the stack is low, on this thread and every thread a walk moves to, so
	/// that a walk moves at every other step it takes: for the tests that hold the output the same
	/// either way. Not every check, or the step that has just moved would move again, without end.
	/// </summary>
	internal static readonly AsyncLocal<bool> Always = new();

	/// <summary>How many times this thread has been asked under <see cref="Always"/>.</summary>
	[ThreadStatic]
	static int _asked;

	/// <summary>Whether the stack this runs on is too low to go deeper on it.</summary>
	/// <remarks>
	/// What the runtime calls enough for an average call, 128 KiB in a 64-bit process, is the
	/// margin: a few levels of the deepest walk, and whatever it calls at the bottom.
	/// </remarks>
	public static bool Low
	{
		get
		{
			if (Always.Value)
				return ++_asked % 2 == 0;

			try
			{
				RuntimeHelpers.EnsureSufficientExecutionStack();

				return false;
			}
			catch (InsufficientExecutionStackException)
			{
				return true;
			}
		}
	}

	/// <summary>Runs <paramref name="work"/> on a thread with a fresh stack, and hands back what it answered.</summary>
	public static T Run<T>(Func<T> work)
	{
		var result  = default(T);
		var thrown  = default(ExceptionDispatchInfo);
		var ambient = Ambient.Current;

		var thread = new Thread(
			() =>
			{
				Ambient.Adopt(ambient);

				try
				{
					result = work();
				}
				catch (Exception caught)
				{
					thrown = ExceptionDispatchInfo.Capture(caught);
				}
			},
			Size);

		thread.Start();
		thread.Join();

		thrown?.Throw();

		return result!;
	}

	/// <summary>
	/// The stack a host's entry runs the generator's own work on (<see cref="Entry{T}"/>): a quarter of
	/// a gigabyte in a 64-bit process, sixteen megabytes in a 32-bit one, reserved and committed only
	/// as it is used.
	/// </summary>
	static int EntrySize => IntPtr.Size == 8 ? 256 * 1024 * 1024 : 16 * 1024 * 1024;

	/// <summary>
	/// For the tests: where a test has put a box here, every entry that moves its work to a stack of
	/// its own counts itself in it, so that the test sees its own request moved and nobody else's.
	/// </summary>
	internal static readonly AsyncLocal<StrongBox<int>?> Moved = new();

	/// <summary>
	/// For the tests: an entry runs its work where it was asked, so that a test which calls the
	/// generator on a small stack has the walks themselves to rely on, and finds one that does not check.
	/// </summary>
	internal static readonly AsyncLocal<bool> Here = new();

	/// <summary>Whether this thread is the one an entry started.</summary>
	[ThreadStatic]
	static bool _entered;

	/// <summary>
	/// Runs a host's whole request — a grammar compiled for the generator, a document analyzed for the
	/// editor — on a thread with a stack of <see cref="EntrySize"/>, and hands back what it answered.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A net under the walks, not a replacement for them: every walk the grammar can make as deep as it
	/// likes checks the stack and moves on its own (<see cref="Low"/>), and the tests call the core on
	/// small stacks so that one which does not is found. What this buys is the host's stack not mattering
	/// — Visual Studio's threads are a megabyte — and a walk nobody has found yet having room to finish.
	/// </para>
	/// <para>
	/// Only the generator's own work goes here: whatever the host hands it, Roslyn's context included,
	/// is used on the caller's thread before and after. The cultures go along, so that nothing differs
	/// by which thread did the work; the thread is a background one, so that it never holds a process
	/// open. Where the thread cannot be had, the work runs where it was asked.
	/// </para>
	/// </remarks>
	public static T Entry<T>(Func<T> work)
	{
		if (work is null)
			throw new ArgumentNullException(nameof(work));

		if (_entered || Here.Value)
			return work();

		var result  = default(T);
		var thrown  = default(ExceptionDispatchInfo);
		var ambient = Ambient.Current;

#pragma warning disable RS1035 // Read only to hand the same cultures to the thread the work moves to; nothing here formats by them.
		var culture   = System.Globalization.CultureInfo.CurrentCulture;
		var uiCulture = System.Globalization.CultureInfo.CurrentUICulture;
#pragma warning restore RS1035

		Thread thread;

		try
		{
			thread = new Thread(
				() =>
				{
#pragma warning disable RS1035 // The caller's cultures, set on the thread that does the caller's work.
					System.Globalization.CultureInfo.CurrentCulture   = culture;
					System.Globalization.CultureInfo.CurrentUICulture = uiCulture;
#pragma warning restore RS1035

					_entered = true;
					Ambient.Adopt(ambient);

					try
					{
						result = work();
					}
					catch (Exception caught)
					{
						thrown = ExceptionDispatchInfo.Capture(caught);
					}
				},
				EntrySize)
			{
				IsBackground = true,
			};

			thread.Start();
		}
		catch (Exception unavailable) when (unavailable is OutOfMemoryException or ThreadStartException)
		{
			return work();
		}

		if (Moved.Value is { } moved)
			Interlocked.Increment(ref moved.Value);

		thread.Join();

		thrown?.Throw();

		return result!;
	}
}
