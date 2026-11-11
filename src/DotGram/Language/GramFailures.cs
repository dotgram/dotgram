using System;
using System.Reflection;
using System.Threading;

namespace DotGram.Language;

/// <summary>
/// Which failures an editor's analysis passes over and which it reports, the same in every editor
/// and in the extension's own guard, so that the two cannot drift apart.
/// </summary>
static class GramFailures
{
	/// <summary>
	/// Whether a cancellation is the caller's: the exception carries the caller's own token, and
	/// that token has been cancelled. Any other cancellation, one of a token merely linked to the
	/// caller's included, is a failure like any other: from the exception alone nothing shows that
	/// a token is linked.
	/// </summary>
	internal static bool Canceled(OperationCanceledException exception, CancellationToken cancellationToken)
	{
		return cancellationToken.IsCancellationRequested && exception.CancellationToken == cancellationToken;
	}

	/// <summary>
	/// Everything but what leaves the process in no state to go on: running out of memory, a
	/// stack overflow, a corrupted-state access violation, an aborted thread or appdomain, code
	/// the runtime cannot load or run; also where one arrives wrapped in another (a task's, a
	/// reflection call's, a type initializer's).
	/// </summary>
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
}
