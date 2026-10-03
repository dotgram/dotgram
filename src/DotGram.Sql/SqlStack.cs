using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace DotGram.Sql;

/// <summary>
/// The stack a walk of a deep tree runs on: whether it has room for one more level, and a stack of
/// its own to go on with where it has not.
/// </summary>
/// <remarks>
/// <para>
/// The parsers read a tree of any depth — a chain of a hundred thousand <c>+</c>, ten thousand
/// <c>CASE</c> one inside the next — because a generated reader that runs its stack low carries the
/// reading onto a thread of its own and goes on there. Whatever walks the tree afterwards by
/// recursion has to do the same, or the tree a parser gave back ends the process that asks for it
/// as SQL: a stack overflow is not an exception and nothing catches it. The writers do this; the
/// equality of the nodes throws <see cref="InsufficientExecutionStackException"/> instead
/// (<c>SqlLocation</c>), because comparing two trees has no output to go on writing.
/// </para>
/// <para>
/// The check is the runtime's, and what it leaves is the 128 KiB it asks for, which is a good deal
/// more than one level of a writer costs, so checking at every node that can hold another of its
/// kind is enough: no cycle of calls goes round without passing one.
/// </para>
/// </remarks>
static class SqlStack
{
	/// <summary>The stack a walk is carried onto, as much as a generated reader takes for its own.</summary>
	const int Size = 16 * 1024 * 1024;

	/// <summary>Whether there is stack left for one more level.</summary>
	/// <remarks>
	/// <c>TryEnsureSufficientExecutionStack</c> is .NET Core's and .NET Standard 2.1's. Where it is not
	/// there, the older call answers the same question by throwing, which is caught here and taken as
	/// a no — once, on the check that finds the margin gone, and the walk goes on elsewhere.
	/// </remarks>
	public static bool Enough()
	{
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
		return RuntimeHelpers.TryEnsureSufficientExecutionStack();
#else
		try
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();

			return true;
		}
		catch (InsufficientExecutionStackException)
		{
			return false;
		}
#endif
	}

	/// <summary>
	/// Runs <paramref name="walk"/> over <paramref name="first"/> and <paramref name="second"/> on a
	/// stack of its own and waits for it, throwing what it threw.
	/// </summary>
	/// <remarks>
	/// The walk is handed its arguments rather than capturing them, so that a caller's lambda is
	/// <c>static</c> and the caller allocates nothing on the path that does not go deeper.
	/// </remarks>
	public static void Deeper<T1, T2>(T1 first, T2 second, Action<T1, T2> walk)
	{
		Hand(() => walk(first, second));
	}

	/// <inheritdoc cref="Deeper{T1, T2}(T1, T2, Action{T1, T2})"/>
	public static void Deeper<T1, T2, T3>(T1 first, T2 second, T3 third, Action<T1, T2, T3> walk)
	{
		Hand(() => walk(first, second, third));
	}

	/// <summary>The thread this one hands its walks to, while it is still there to take them.</summary>
	[ThreadStatic]
	static Worker? _worker;

	static void Hand(Action walk)
	{
		var worker = _worker;

		if (worker is null || !worker.Take())
			_worker = worker = new Worker();

		worker.Run(walk);
	}

	/// <summary>
	/// A thread with a stack of its own that takes one walk after another from the thread that made it,
	/// and ends when it has been left idle a while.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>Why one thread for many walks.</b> A walk that runs a stack low hands over the node it is at,
	/// and when that comes back the stack is as low as it was, so every sibling after it is handed over
	/// too: a list of a hundred thousand values that begins at the margin is a hundred thousand walks.
	/// A thread made for each took 150 microseconds, and such a list was written in fifteen seconds
	/// where it takes a tenth of one; handed to a thread that is already there, each costs a wake-up.
	/// </para>
	/// <para>
	/// <b>Why it ends.</b> It holds a stack, and the pages a deep walk touched stay with it, so it does
	/// not outlive the work by more than <see cref="Linger"/>. The thread that made it may ask again at
	/// the moment it decides to go; the state says which of the two came first, and a thread that finds
	/// it gone makes another.
	/// </para>
	/// </remarks>
	sealed class Worker
	{
		const int Idle = 0;
		const int Busy = 1;
		const int Gone = 2;

		/// <summary>How long, in milliseconds, it waits for another walk before it ends.</summary>
		const int Linger = 100;

		readonly SemaphoreSlim _go   = new(0, 1);
		readonly SemaphoreSlim _done = new(0, 1);

		int        _state = Busy;
		Action?    _walk;
		Exception? _thrown;

		public Worker()
		{
			var thread = new Thread(Loop, Size)
			{
				IsBackground = true,
				Name         = "DotGram.Sql deep walk",
			};

			thread.Start();
		}

		/// <summary>Claims it for one more walk, unless it has already ended.</summary>
		public bool Take()
		{
			return Interlocked.CompareExchange(ref _state, Busy, Idle) == Idle;
		}

		/// <summary>Runs <paramref name="walk"/> there and waits for it, throwing what it threw.</summary>
		public void Run(Action walk)
		{
			_walk = walk;
			_go.Release();
			_done.Wait();

			var thrown = _thrown;

			_thrown = null;

			if (thrown is not null)
				ExceptionDispatchInfo.Capture(thrown).Throw();
		}

		void Loop()
		{
			while (true)
			{
				if (!_go.Wait(Linger))
				{
					if (Interlocked.CompareExchange(ref _state, Gone, Idle) == Idle)
						return;

					// Claimed in the moment before it could go: the walk is on its way.
					_go.Wait();
				}

				try
				{
					_walk!();
				}
				catch (Exception thrown)
				{
					_thrown = thrown;
				}

				_walk = null;

				Volatile.Write(ref _state, Idle);
				_done.Release();
			}
		}
	}
}
