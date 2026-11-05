using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Shell;

namespace DotGram.VisualStudio;

/// <summary>
/// The one shape every entry point Visual Studio calls, and every task nobody awaits, is run in.
/// A failure of the extension's own is written to the activity log with the operation it broke,
/// and the editor gets the answer it would get had the extension nothing to say: no gold bar, no
/// component Visual Studio disables, no exception of an unobserved task that reaches nobody.
/// Cancellation is how an editor operation ends early, and is neither logged nor passed on when
/// the caller's token has been cancelled and the exception's own token has been too: the
/// caller's, or one linked to it. Any other cancellation is a failure like any other.
/// </summary>
static class EditorGuard
{
	internal const string Source = "DotGram.VisualStudio";

	/// <summary>
	/// Where a failure is written: the activity log, which <c>devenv /log</c> keeps as a file.
	/// Replaced by the tests, which have no Visual Studio to log to.
	/// </summary>
	internal static Action<string> Write { get; set; } = WriteToActivityLog;

	public static T Run<T>(string operation, T fallback, Func<T> body, CancellationToken cancellationToken = default)
	{
		try
		{
			return body();
		}
		catch (OperationCanceledException exception) when (Canceled(exception, cancellationToken))
		{
			return fallback;
		}
		catch (Exception exception) when (Recoverable(exception))
		{
			Log(operation, exception);
			return fallback;
		}
	}

	public static void Run(string operation, Action body)
	{
		try
		{
			body();
		}
		catch (Exception exception) when (Recoverable(exception))
		{
			Log(operation, exception);
		}
	}

	public static async Task<T> RunAsync<T>(
		string operation,
		T fallback,
		Func<Task<T>> body,
		CancellationToken cancellationToken)
	{
		try
		{
			return await body().ConfigureAwait(false);
		}
		catch (OperationCanceledException exception) when (Canceled(exception, cancellationToken))
		{
			return fallback;
		}
		catch (Exception exception) when (Recoverable(exception))
		{
			Log(operation, exception);
			return fallback;
		}
	}

	/// <summary>
	/// Runs work nobody awaits. The task it returns never faults, critical failures included: a
	/// discarded task's exception reaches nobody, so rethrowing one would only lose it. Every
	/// failure but the caller's cancellation is logged here instead.
	/// </summary>
	public static async Task RunAsync(string operation, Func<Task> body, CancellationToken cancellationToken)
	{
		try
		{
			await body().ConfigureAwait(false);
		}
		catch (OperationCanceledException exception) when (Canceled(exception, cancellationToken))
		{
		}
		catch (Exception exception)
		{
			Log(operation, exception);
		}
	}

	/// <summary>
	/// Raises an event to each of its handlers under a guard of its own, so that one failing
	/// handler neither keeps the event from the others nor stops what its raiser does next.
	/// </summary>
	public static void Notify<T>(string operation, Action<T>? handlers, T argument)
	{
		if (handlers is null)
			return;

		foreach (Action<T> handler in handlers.GetInvocationList())
			Run(operation, () => handler(argument));
	}

	/// <summary>
	/// The items of a lazy sequence, such as a tagger's <c>GetTags</c>, up to the first failure
	/// in it. An iterator's body runs while its caller enumerates it, outside any guard around the
	/// call that made it, so the guard has to be on the enumeration.
	/// </summary>
	public static IEnumerable<T> Enumerate<T>(string operation, IEnumerable<T> items)
	{
		IEnumerator<T> enumerator;

		try
		{
			enumerator = items.GetEnumerator();
		}
		catch (Exception exception) when (Recoverable(exception))
		{
			Log(operation, exception);
			yield break;
		}

		try
		{
			while (true)
			{
				T current;

				try
				{
					if (!enumerator.MoveNext())
						yield break;

					current = enumerator.Current;
				}
				catch (Exception exception) when (Recoverable(exception))
				{
					Log(operation, exception);
					yield break;
				}

				yield return current;
			}
		}
		finally
		{
			Run(operation, enumerator.Dispose);
		}
	}

	public static void Log(string operation, Exception exception)
	{
		try
		{
			Write($"{operation} failed: {exception}");
		}
		catch (Exception)
		{
			// Nowhere is left to report it, out of memory included, and a failure to log must
			// not become the failure the guard exists to keep out of Visual Studio.
		}
	}

	/// <summary>
	/// Whether a cancellation is the caller's: its token has been cancelled, and so has the
	/// exception's own, which is the caller's token or one linked to it. A cancellation that
	/// comes while the caller's token stands, or that carries a token nobody cancelled, is not.
	/// </summary>
	internal static bool Canceled(OperationCanceledException exception, CancellationToken cancellationToken)
	{
		return cancellationToken.IsCancellationRequested && exception.CancellationToken.IsCancellationRequested;
	}

	/// <summary>
	/// Everything but what leaves the process in no state to go on: running out of memory, a
	/// stack overflow, a corrupted-state access violation, an aborted thread or appdomain, code
	/// the runtime cannot load or run; also where one arrives wrapped in another (a task's, a
	/// reflection call's, a type initializer's).
	/// </summary>
	/// <remarks>
	/// This departs on purpose from the SDK's <c>ErrorHandler.IsCriticalException</c>, which also
	/// counts <see cref="DivideByZeroException"/> as critical. That is an ordinary bug in the
	/// extension's own code, and keeping such bugs out of Visual Studio is what the guard is for.
	/// </remarks>
	internal static bool Recoverable(Exception exception)
	{
		return !Critical(exception);
	}

	static bool Critical(Exception? exception)
	{
		switch (exception)
		{
			case null:
				return false;

			case OutOfMemoryException or StackOverflowException or AccessViolationException or
				ThreadAbortException or AppDomainUnloadedException or
				InvalidProgramException or BadImageFormatException:
				return true;

			case AggregateException aggregate:
				foreach (var inner in aggregate.InnerExceptions)
					if (Critical(inner))
						return true;

				return false;

			case TargetInvocationException or TypeInitializationException:
				return Critical(exception.InnerException);

			default:
				return false;
		}
	}

	static void WriteToActivityLog(string message)
	{
		ActivityLog.LogError(Source, message);
	}
}
