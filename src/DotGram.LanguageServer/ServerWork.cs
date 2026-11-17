using System.Threading;

using Microsoft.CodeAnalysis.Text;

namespace DotGram.LanguageServer;

/// <summary>
/// The work on a whole text that an edit is not to do, counted where a test asks for it: a read of
/// the text into its lines, and a version of it made afresh rather than from the version before.
/// Counted for the flow of work a test set <see cref="Current"/> in, so that tests running beside it
/// count nothing into it.
/// </summary>
/// <remarks>
/// Nothing is done unless a test has asked for counting: until then every hook reads one static
/// field and returns, and the flow-of-work slot is not even created.
/// </remarks>
sealed class ServerWork
{
	// Set once a test asks for counting, and never by the server itself.
	static volatile bool _enabled;

	int _lineReads;
	int _copies;

	/// <summary>The slot, created the first time a test sets <see cref="Current"/>.</summary>
	static class Counting
	{
		public static readonly AsyncLocal<ServerWork?> Value = new();
	}

	/// <summary>What the work that follows counts into; null, as outside a test, counts nothing.</summary>
	public static ServerWork? Current
	{
		get => _enabled ? Counting.Value.Value : null;
		set
		{
			_enabled = true;
			Counting.Value.Value = value;
		}
	}

	/// <summary>How many times a whole text was read into its lines.</summary>
	public int LineReads => Volatile.Read(ref _lineReads);

	/// <summary>How many versions of a text were made afresh, the text before not shared with them.</summary>
	public int Copies => Volatile.Read(ref _copies);

	public static void LineRead()
	{
		if (_enabled && Counting.Value.Value is { } work)
			Interlocked.Increment(ref work._lineReads);
	}

	/// <summary>
	/// A version of a text was made from the one before: counted as a copy where it does not share it,
	/// however it was made. Roslyn knows what a text made from another changed of it; one made afresh —
	/// a string put together, a new <see cref="SourceText"/> — shares nothing, and is the whole text
	/// changed.
	/// </summary>
	public static void Made(SourceText before, SourceText after)
	{
		if (!_enabled || Counting.Value.Value is not { } work)
			return;

		var changes = after.GetChangeRanges(before);

		if (changes.Count == 1 && changes[0].Span.Start == 0 && changes[0].Span.Length == before.Length && changes[0].NewLength == after.Length)
			Interlocked.Increment(ref work._copies);
	}
}
