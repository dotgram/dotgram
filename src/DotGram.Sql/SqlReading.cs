using System;

namespace DotGram.Sql;

/// <summary>
/// What a T-SQL reading under way tells the semantic actions that build its values: the text it was given, whether it
/// keeps for everybody the spans it tells (<see cref="SqlLocation.Told"/>), and the two ways a builder narrows a reading
/// it makes of a part of the text.
/// </summary>
/// <remarks>
/// <para>
/// <b>Carried by hand onto another stack.</b> The state is the current thread's, and a reading that runs its stack low
/// is handed to a thread with a stack of its own: by the generated reader, and by a deep walk of the builders'
/// (<see cref="SqlStack"/>). Neither takes a thread-static field across, so both are told to: the reader through the
/// hooks it calls on either side of a hand-off (<c>OnHandingOff</c> and <c>OnTakingOver</c> of
/// <c>TransactSqlParser</c>), the walk by the builder that starts it. The thread that takes the reading makes the
/// carriage it is handed current for as long as it reads, and puts its own back after (<see cref="Swap"/>). Nothing
/// rides on the execution context, so a caller that suppresses its flow loses nothing.
/// </para>
/// <para>
/// <b>Why one a thread, written in place.</b> A reading writes the fields of its thread's carriage and puts them back
/// when it ends, so that it allocates nothing. The thread that hands a reading off waits until the reading is done, so
/// the carriage it shares with the thread that took it is never written by both at once.
/// </para>
/// </remarks>
internal sealed class SqlReading
{
	/// <summary>The text the reading was given, which a builder reads the words a record leaves out from, or null.</summary>
	public string? Source;

	/// <summary>Whether the reading keeps the spans it tells for everybody: a located one.</summary>
	public bool Publishing
	{
		get
		{
			return _publishing;
		}
		set
		{
			_publishing = value;

			if (ReferenceEquals(this, here))
				publishes = value;
		}
	}

	bool _publishing;

	/// <summary>Whether the reading is skimmed: its statements hold what their rules were handed, and are not built.</summary>
	public bool Skimming;

	/// <summary>Whether the reading is a plain one a builder made, whose records have no span.</summary>
	public bool Unlocated;

	/// <summary>The carriage of the reading this thread is running.</summary>
	[ThreadStatic] static SqlReading? here;

	/// <summary>This thread's own carriage, made on its first reading and used by every one after.</summary>
	[ThreadStatic] static SqlReading? own;

	/// <summary>
	/// <see cref="Publishing"/> of the current carriage, kept beside it: what every node a reading builds asks
	/// (<see cref="SqlLocation.Told"/>), read as one field rather than through the carriage. Set wherever the current
	/// carriage changes, and by the carriage itself while it is current.
	/// </summary>
	[ThreadStatic] static bool publishes;

	/// <summary>The reading this thread is running, or null where it runs none.</summary>
	public static SqlReading? Current
	{
		get
		{
			return here;
		}
	}

	/// <summary>Whether the reading this thread is running keeps its spans for everybody; false where it runs none.</summary>
	public static bool Publishes
	{
		get
		{
			return publishes;
		}
	}

	/// <summary>The current carriage, made where there is none, for a builder that narrows a reading.</summary>
	public static SqlReading Writable
	{
		get
		{
			if (here is null)
				MakeCurrent(own ??= new SqlReading());

			return here!;
		}
	}

	static void MakeCurrent(SqlReading? reading)
	{
		here      = reading;
		publishes = reading is { _publishing: true };
	}

	/// <summary>
	/// Makes this thread's own carriage the current one for a reading that begins, and says what to put back when it ends.
	/// </summary>
	public static Prior Begin()
	{
		var outer = here;
		var mine  = own ??= new SqlReading();

		MakeCurrent(mine);

		return new Prior(mine, outer, mine.Source, mine.Publishing, mine.Unlocated);
	}

	/// <summary>
	/// What stood before a reading: the fields of the carriage it wrote, and the carriage that was current where it was
	/// another one, a reading begun on a thread that was reading for another. A value, so that a reading allocates none.
	/// </summary>
	internal readonly struct Prior(SqlReading mine, SqlReading? outer, string? source, bool publishing, bool unlocated)
	{
		public void End()
		{
			mine.Source     = source;
			mine.Publishing = publishing;
			mine.Unlocated  = unlocated;

			if (outer is not null && !ReferenceEquals(outer, mine))
				MakeCurrent(outer);
		}
	}

	/// <summary>
	/// Makes <paramref name="reading"/> current on this thread, as a thread that takes a reading over does, and says what
	/// was current before, which the same call puts back after.
	/// </summary>
	public static SqlReading? Swap(SqlReading? reading)
	{
		var outer = here;

		MakeCurrent(reading);

		return outer;
	}
}
