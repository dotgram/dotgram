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
		var deep = new Deep<T1, T2, bool>(first, second, default!, (one, two, _) => walk(one, two));

		deep.Go();
	}

	/// <inheritdoc cref="Deeper{T1, T2}(T1, T2, Action{T1, T2})"/>
	public static void Deeper<T1, T2, T3>(T1 first, T2 second, T3 third, Action<T1, T2, T3> walk)
	{
		var deep = new Deep<T1, T2, T3>(first, second, third, walk);

		deep.Go();
	}

	sealed class Deep<T1, T2, T3>(T1 first, T2 second, T3 third, Action<T1, T2, T3> walk)
	{
		Exception? _thrown;

		public void Go()
		{
			var thread = new Thread(Run, Size);

			thread.Start();
			thread.Join();

			if (_thrown is not null)
				ExceptionDispatchInfo.Capture(_thrown).Throw();
		}

		void Run()
		{
			try
			{
				walk(first, second, third);
			}
			catch (Exception thrown)
			{
				_thrown = thrown;
			}
		}
	}
}
