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
}
