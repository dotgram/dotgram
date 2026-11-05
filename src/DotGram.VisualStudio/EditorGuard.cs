using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Shell;

namespace DotGram.VisualStudio;

/// <summary>
/// The one shape every entry point Visual Studio calls, and every task nobody awaits, is run in.
/// A failure of the extension's own is written to the activity log with the operation it broke,
/// and the editor gets the answer it would get had the extension nothing to say: no gold bar, no
/// component Visual Studio disables, no exception of an unobserved task that reaches nobody.
/// Cancellation through the caller's own token is how an editor operation ends early, and is
/// neither logged nor passed on.
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
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
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
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
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
	/// Runs work nobody awaits: the task it returns never faults, so the discard that starts it
	/// loses nothing. A failure is logged here, where it would otherwise go unobserved.
	/// </summary>
	public static async Task RunAsync(string operation, Func<Task> body, CancellationToken cancellationToken)
	{
		try
		{
			await body().ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
		}
		catch (Exception exception) when (Recoverable(exception))
		{
			Log(operation, exception);
		}
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
		catch (Exception failure) when (Recoverable(failure))
		{
			// Nowhere is left to report it, and a failure to log must not become the failure
			// the guard exists to keep out of Visual Studio.
		}
	}

	/// <summary>
	/// Everything but running out of memory, which no fallback answer survives.
	/// </summary>
	internal static bool Recoverable(Exception exception)
	{
		return exception is not OutOfMemoryException;
	}

	static void WriteToActivityLog(string message)
	{
		ActivityLog.LogError(Source, message);
	}
}
