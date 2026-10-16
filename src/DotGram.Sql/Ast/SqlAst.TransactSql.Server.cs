using System;

namespace DotGram.Sql.Ast;

// T-SQL's Server family: what runs beside the databases and the commands that govern it — KILL, RECONFIGURE, SHUTDOWN and DBCC, the
// server's configuration, the resource governor, availability groups and endpoints. A setting that is a name and a value is an
// Option (proposal 20); a place that is one of several things is a small family named after the grammar's words.

public abstract partial record Statement
{
	/// <summary>
	/// T-SQL: KILL (Transact-SQL). <c>KILL session [WITH STATUSONLY | COMMIT | ROLLBACK]</c>: a session by its number, which a minus
	/// may precede, or a unit of work by its string.
	/// </summary>
	public sealed record Kill(Expression Target) : Statement
	{
		/// <summary>T-SQL: KILL (Transact-SQL). <c>WITH STATUSONLY</c>, <c>COMMIT</c> or <c>ROLLBACK</c>; null where none is written.</summary>
		public KillOption? With { get; init; }
	}

	/// <summary>T-SQL: KILL STATS JOB (Transact-SQL). <c>KILL STATS JOB job</c>: a number, which a minus may precede.</summary>
	public sealed record KillStatsJob(LiteralValue.Numeric Job) : Statement;

	/// <summary>
	/// T-SQL: KILL QUERY NOTIFICATION SUBSCRIPTION. <c>KILL QUERY NOTIFICATION SUBSCRIPTION { ALL | subscription }</c>: every
	/// subscription, or the one a number names, which a minus may precede; exactly one of the two.
	/// </summary>
	public sealed record KillQueryNotificationSubscription : Statement
	{
		/// <summary>T-SQL: KILL QUERY NOTIFICATION SUBSCRIPTION. <c>ALL</c>.</summary>
		public bool All { get; init; }

		/// <summary>T-SQL: KILL QUERY NOTIFICATION SUBSCRIPTION. The subscription's number; null where <see cref="All"/> is said.</summary>
		public LiteralValue.Numeric? Subscription { get; init; }
	}

	/// <summary>T-SQL: RECONFIGURE (Transact-SQL). <c>RECONFIGURE [WITH OVERRIDE]</c>.</summary>
	public sealed record Reconfigure(bool Override) : Statement;

	/// <summary>T-SQL: SHUTDOWN (Transact-SQL). <c>SHUTDOWN [WITH NOWAIT]</c>.</summary>
	public sealed record Shutdown(bool NoWait) : Statement;

	/// <summary>
	/// T-SQL: DBCC (Transact-SQL). <c>DBCC command [(argument, …)] [WITH option, …]</c>: one record for every command, the documented
	/// ones and the rest, as the engine reads them all one way. The command is a plain word; an argument is a value or a name and a
	/// value, as an EXECUTE's is.
	/// </summary>
	public sealed record Dbcc(Identifier Command) : Statement
	{
		/// <summary>
		/// T-SQL: DBCC (Transact-SQL). The arguments in brackets, each a value the grammar's <c>DbccValue</c> reads or a name and a
		/// value, <c>SIZE = 10</c>; null where no brackets are written, and empty where they hold nothing.
		/// </summary>
		public SqlList<ExecuteArgument>? Arguments { get; init; }

		/// <summary>
		/// T-SQL: DBCC (Transact-SQL). <c>WITH option, …</c>: words, MAXDOP and WAIT_AT_LOW_PRIORITY with their own values; the
		/// first may be a run of words joined by <c>JOIN</c>; none where no <c>WITH</c> is written.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];
	}
}

/// <summary>T-SQL: KILL (Transact-SQL). What a KILL does to the work it ends: <c>STATUSONLY</c>, <c>COMMIT</c> or <c>ROLLBACK</c>.</summary>
public enum KillOption { StatusOnly, Commit, Rollback }
