using System.Threading;

namespace DotGram.Sql;

/// <summary>
/// What a T-SQL reading under way tells the semantic actions that build its values: the text it was given, whether it
/// keeps for everybody the spans it tells (<see cref="SqlLocation.Told"/>), and the two ways a builder narrows a reading
/// it makes of a part of the text.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why not thread-static fields.</b> A reading that runs its stack low is handed by the generated reader to a thread
/// with a stack of its own, and so is a deep walk of the builders' (<see cref="SqlStack"/>). Both take the execution
/// context and the cultures across, and neither takes a thread-static field: an action on that thread read what that
/// thread last left there, a reading that was not located, or one of another caller's. So the carriage goes with the
/// context, in an <see cref="AsyncLocal{T}"/>, and is mirrored in a thread-static field that the local's change callback
/// keeps current whenever a thread takes a context up or puts it down: an action reads one field, as it always did.
/// </para>
/// <para>
/// <b>Why one a thread, written in place.</b> Setting the local copies the context, so a thread sets it once, to a
/// carriage of its own, and every reading after that writes the fields of that carriage and puts them back when it
/// ends. The thread that hands a reading off waits until the reading is done, so the carriage it shares with the thread
/// that took it is never written by both at once. A thread that takes up a context with another thread's carriage in it
/// — a task started after a reading, a continuation — and then reads takes its own for that reading and puts the other
/// back after, so that it never writes a carriage another thread may be using.
/// </para>
/// </remarks>
internal sealed class SqlReading
{
	/// <summary>The text the reading was given, which a builder reads the words a record leaves out from, or null.</summary>
	public string? Source;

	/// <summary>Whether the reading keeps the spans it tells for everybody: a located one.</summary>
	public bool Publishing;

	/// <summary>Whether the reading is skimmed: its statements hold what their rules were handed, and are not built.</summary>
	public bool Skimming;

	/// <summary>Whether the reading is a plain one a builder made, whose records have no span.</summary>
	public bool Unlocated;

	/// <summary>The carriage that goes with the execution context, onto any thread a reading is handed to.</summary>
	static readonly AsyncLocal<SqlReading?> Flowing = new(Changed);

	/// <summary>The carriage of the reading this thread is running, as <see cref="Flowing"/> says it.</summary>
	[ThreadStatic] static SqlReading? here;

	/// <summary>This thread's own carriage, made on its first reading and used by every one after.</summary>
	[ThreadStatic] static SqlReading? own;

	/// <summary>The reading this thread is running, or null where it runs none.</summary>
	public static SqlReading? Current
	{
		get
		{
			return here;
		}
	}

	/// <summary>The same, made where there is none, for a builder that narrows a reading.</summary>
	public static SqlReading Writable
	{
		get
		{
			return here ??= own ??= new SqlReading();
		}
	}

	static void Changed(AsyncLocalValueChangedArgs<SqlReading?> change)
	{
		// A context taken up or put down: a hand-off's thread begins or ends its part, or a task its work.
		if (change.ThreadContextChanged)
			here = change.CurrentValue;
	}

	/// <summary>
	/// Makes this thread's own carriage the current one for a reading that begins, and says what to put back when it ends.
	/// </summary>
	public static Prior Begin()
	{
		var outer = here;
		var mine  = own ??= new SqlReading();

		// Set once a thread, and again only where the context lost it or brought another thread's.
		if (!ReferenceEquals(Flowing.Value, mine))
			Flowing.Value = mine;

		here = mine;

		return new Prior(mine, outer, mine.Source, mine.Publishing, mine.Unlocated);
	}

	/// <summary>
	/// What stood before a reading: the fields of the carriage it wrote, and the carriage that was current where it was
	/// another thread's. A value, so that a reading allocates none.
	/// </summary>
	internal readonly struct Prior(SqlReading mine, SqlReading? outer, string? source, bool publishing, bool unlocated)
	{
		public void End()
		{
			mine.Source     = source;
			mine.Publishing = publishing;
			mine.Unlocated  = unlocated;

			// Another thread's carriage, which this reading set aside: current again, and the one the context carries on.
			if (outer is not null && !ReferenceEquals(outer, mine))
			{
				Flowing.Value = outer;
				here          = outer;
			}
		}
	}

	/// <summary>
	/// Makes <paramref name="reading"/> current on this thread, as a hand-off that carries it itself does, and says what
	/// was current before.
	/// </summary>
	public static SqlReading? Swap(SqlReading? reading)
	{
		var outer = here;

		here = reading;

		return outer;
	}
}
