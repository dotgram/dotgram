using System;
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
}
