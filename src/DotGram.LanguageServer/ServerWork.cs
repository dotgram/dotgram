using System.Threading;

namespace DotGram.LanguageServer;

/// <summary>
/// The work on a whole text that an edit is not to do, counted where a test asks for it: a read of
/// the text into its lines, and a copy of it made afresh. Counted for the flow of work a test set
/// <see cref="Current"/> in, so that tests running beside it count nothing into it.
/// </summary>
sealed class ServerWork
{
	static readonly AsyncLocal<ServerWork?> Counting = new();

	int _lineReads;
	int _copies;

	/// <summary>What the work that follows counts into; null, as outside a test, counts nothing.</summary>
	public static ServerWork? Current
	{
		get => Counting.Value;
		set => Counting.Value = value;
	}

	/// <summary>How many times a whole text was read into its lines.</summary>
	public int LineReads => Volatile.Read(ref _lineReads);

	/// <summary>How many times a whole text was made afresh as one string.</summary>
	public int Copies => Volatile.Read(ref _copies);

	public static void LineRead()
	{
		if (Counting.Value is { } work)
			Interlocked.Increment(ref work._lineReads);
	}

	public static void Copy()
	{
		if (Counting.Value is { } work)
			Interlocked.Increment(ref work._copies);
	}
}
