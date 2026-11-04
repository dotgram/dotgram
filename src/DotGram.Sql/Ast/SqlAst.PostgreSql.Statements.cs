using System;

namespace DotGram.Sql.Ast;

// PostgreSQL's short statements: the transaction statements in their PostgreSQL spellings and with the
// two-phase commit, the run-time parameters (SET, SHOW, RESET), EXPLAIN, DO, the prepared statements,
// LISTEN and NOTIFY, LOCK, DISCARD and CHECKPOINT. The standard's records are kept where PostgreSQL's
// statement means the same thing and gain a spelling or a scope; a statement the standard has not is a
// record of its own.

public abstract partial record Statement
{
	public sealed partial record StartTransaction
	{
		/// <summary>PostgreSQL: BEGIN. Written <c>BEGIN</c> rather than <c>START TRANSACTION</c>.</summary>
		[Spelling] public bool Begin { get; init; }

		/// <summary>PostgreSQL: BEGIN. <c>BEGIN TRANSACTION</c>: the word after BEGIN, which START TRANSACTION has no room for; null where none was written.</summary>
		[Spelling] public TransactionKeyword? Keyword { get; init; }

		/// <summary>PostgreSQL: BEGIN. <c>BEGIN WORK</c>, which START TRANSACTION has no room for.</summary>
		public bool Work { get; init; }
	}

	public sealed partial record Commit
	{
		/// <summary>PostgreSQL: END. Written <c>END</c> rather than <c>COMMIT</c>.</summary>
		[Spelling] public bool End { get; init; }
	}

	public sealed partial record Rollback
	{
		/// <summary>PostgreSQL: ABORT. Written <c>ABORT</c> rather than <c>ROLLBACK</c>.</summary>
		[Spelling] public bool Abort { get; init; }

		/// <summary>PostgreSQL: ROLLBACK TO SAVEPOINT. Written <c>ROLLBACK TO s</c>, without the word SAVEPOINT.</summary>
		[Spelling] public bool SavepointKeywordOmitted { get; init; }
	}

	public sealed partial record ReleaseSavepoint
	{
		/// <summary>PostgreSQL: RELEASE SAVEPOINT. Written <c>RELEASE s</c>, without the word SAVEPOINT.</summary>
		[Spelling] public bool SavepointKeywordOmitted { get; init; }
	}

	/// <summary>PostgreSQL: PREPARE TRANSACTION. <c>PREPARE TRANSACTION 'id'</c>: the first phase of a two-phase commit, under the name the string gives.</summary>
	public sealed record PrepareTransaction(LiteralValue.String Id) : Statement;

	/// <summary>PostgreSQL: COMMIT PREPARED. <c>COMMIT PREPARED 'id'</c>: the second phase of a two-phase commit.</summary>
	public sealed record CommitPrepared(LiteralValue.String Id) : Statement;

	/// <summary>PostgreSQL: ROLLBACK PREPARED. <c>ROLLBACK PREPARED 'id'</c>: a prepared transaction cancelled.</summary>
	public sealed record RollbackPrepared(LiteralValue.String Id) : Statement;

	public sealed partial record SetTransaction
	{
		/// <summary>PostgreSQL: SET TRANSACTION. Written <c>SET SESSION TRANSACTION …</c>, the word SESSION saying what no word says.</summary>
		[Spelling] public bool Session { get; init; }
	}

	public sealed partial record SetSessionCharacteristics
	{
		/// <summary>PostgreSQL: SET. <c>SET LOCAL SESSION CHARACTERISTICS …</c>, <c>SET SESSION SESSION CHARACTERISTICS …</c>: the scope word before the statement, which the grammar reads and the server then refuses or ignores; null where none was written.</summary>
		public SetScope? Scope { get; init; }
	}

	public sealed partial record SetTimeZone
	{
		/// <summary>PostgreSQL: SET. <c>SET LOCAL TIME ZONE …</c>: the scope word; null where none was written.</summary>
		public SetScope? Scope { get; init; }
	}

	public sealed partial record SetRole
	{
		/// <summary>PostgreSQL: SET ROLE. <c>SET LOCAL ROLE …</c>: the scope word; null where none was written.</summary>
		public SetScope? Scope { get; init; }
	}

	public sealed partial record SetSessionAuthorization
	{
		/// <summary>PostgreSQL: SET SESSION AUTHORIZATION. <c>SET LOCAL SESSION AUTHORIZATION …</c>: the scope word; null where none was written.</summary>
		public SetScope? Scope { get; init; }
	}

	public sealed partial record SetSchema
	{
		/// <summary>PostgreSQL: SET. <c>SET LOCAL SCHEMA 'x'</c>: the scope word; null where none was written.</summary>
		public SetScope? Scope { get; init; }
	}

	public sealed partial record SetNames
	{
		/// <summary>PostgreSQL: SET. <c>SET LOCAL NAMES 'x'</c>: the scope word; null where none was written.</summary>
		public SetScope? Scope { get; init; }
	}

	/// <summary>PostgreSQL: SET TRANSACTION. <c>SET TRANSACTION SNAPSHOT 'id'</c>: the transaction takes the snapshot another exported.</summary>
	public sealed record SetTransactionSnapshot(LiteralValue.String Snapshot) : Statement
	{
		/// <summary>The scope word before TRANSACTION, which the grammar reads and the server then refuses; null where none was written.</summary>
		public SetScope? Scope { get; init; }
	}

	/// <summary>
	/// PostgreSQL: SET. <c>SET [SESSION | LOCAL] name TO value, …</c>, <c>= value</c>, <c>TO DEFAULT</c>, <c>FROM CURRENT</c>: a run-time
	/// parameter set. The values are what the grammar reads there: a word, a string, a number, a boolean, or <c>DEFAULT</c>
	/// (<see cref="Expression.Default"/>). <c>SET XML OPTION DOCUMENT</c> is the parameter <c>xmloption</c> in the statement's
	/// own spelling.
	/// </summary>
	public sealed record SetParameter(QualifiedName Name, SqlList<Expression> Values) : Statement
	{
		/// <summary>The scope word; null where none was written.</summary>
		public SetScope? Scope { get; init; }

		/// <summary>Written <c>name = value</c> rather than <c>name TO value</c>.</summary>
		[Spelling] public bool EqualsSign { get; init; }

		/// <summary><c>SET name FROM CURRENT</c>: the value the session has now, for the transaction; the values are then empty.</summary>
		public bool FromCurrent { get; init; }

		/// <summary>Written in the statement's own words for the parameter, <c>SET XML OPTION DOCUMENT</c>; null where the name was written.</summary>
		[Spelling] public ParameterSpelling? Spelling { get; init; }
	}

	/// <summary>PostgreSQL: SHOW. <c>SHOW name</c>, or <c>SHOW ALL</c> where the name is null.</summary>
	public sealed record ShowParameter(QualifiedName? Name) : Statement
	{
		/// <summary>Written in the statement's own words for the parameter, <c>SHOW TIME ZONE</c> for <c>timezone</c>; null where the name was written.</summary>
		[Spelling] public ParameterSpelling? Spelling { get; init; }
	}

	/// <summary>PostgreSQL: RESET. <c>RESET name</c>, or <c>RESET ALL</c> where the name is null.</summary>
	public sealed record ResetParameter(QualifiedName? Name) : Statement
	{
		/// <summary>Written in the statement's own words for the parameter, <c>RESET TIME ZONE</c> for <c>timezone</c>; null where the name was written.</summary>
		[Spelling] public ParameterSpelling? Spelling { get; init; }
	}

	/// <summary>
	/// PostgreSQL: EXPLAIN. <c>EXPLAIN (ANALYZE, FORMAT JSON) statement</c>: the options, each a name and perhaps a value, and the
	/// statement they explain, which is a query, a data change, or an EXECUTE.
	/// </summary>
	public sealed record Explain(SqlList<Option> Options, Statement Statement) : Statement
	{
		/// <summary>Written <c>EXPLAIN ANALYZE VERBOSE …</c>, the options as bare words before the statement rather than in brackets.</summary>
		[Spelling] public bool Bare { get; init; }
	}

	/// <summary>
	/// PostgreSQL: DO. <c>DO $$ … $$</c>, <c>DO LANGUAGE plpgsql 'body'</c>: a procedure's body run once. The body is the string as it
	/// was written, in whichever quoting; the language is the word or the string after LANGUAGE, null where none was written.
	/// </summary>
	public sealed record Do(LiteralValue.String Body, Expression? Language = null) : Statement
	{
		/// <summary>Written with <c>LANGUAGE</c> before the body, <c>DO LANGUAGE sql 'body'</c>.</summary>
		[Spelling] public bool LanguageFirst { get; init; }
	}

	/// <summary>
	/// PostgreSQL: PREPARE. <c>PREPARE p (int, text) AS SELECT …</c>: a statement under a name, its parameters' types before it, for
	/// EXECUTE to run. The body is a query or a data change.
	/// </summary>
	public sealed record PreparePlan(Identifier Name, SqlList<DataType> ParameterTypes, Statement Body) : Statement;

	public sealed partial record DeallocatePrepare
	{
		/// <summary>PostgreSQL: DEALLOCATE. Written <c>DEALLOCATE p</c>, without the word PREPARE.</summary>
		[Spelling] public bool PrepareOmitted { get; init; }
	}

	/// <summary>PostgreSQL: DEALLOCATE. <c>DEALLOCATE ALL</c>: every prepared statement of the session.</summary>
	public sealed record DeallocateAll : Statement
	{
		/// <summary>Written <c>DEALLOCATE PREPARE ALL</c>, with the word PREPARE.</summary>
		[Spelling] public bool PrepareKeyword { get; init; }
	}

	/// <summary>PostgreSQL: LISTEN. <c>LISTEN channel</c>.</summary>
	public sealed record Listen(Identifier Channel) : Statement;

	/// <summary>PostgreSQL: UNLISTEN. <c>UNLISTEN channel</c>, or <c>UNLISTEN *</c> where the channel is null.</summary>
	public sealed record Unlisten(Identifier? Channel) : Statement;

	/// <summary>PostgreSQL: NOTIFY. <c>NOTIFY channel, 'payload'</c>; the payload null where none was written.</summary>
	public sealed record Notify(Identifier Channel, LiteralValue.String? Payload = null) : Statement;

	/// <summary>
	/// PostgreSQL: LOCK. <c>LOCK TABLE a, ONLY b IN SHARE MODE NOWAIT</c>: the tables, the mode (null where <c>IN … MODE</c> was not
	/// written, which the server takes as ACCESS EXCLUSIVE), and NOWAIT.
	/// </summary>
	public sealed record LockTable(SqlList<TableSource> Tables) : Statement
	{
		public LockMode? Mode { get; init; }
		public bool NoWait { get; init; }

		/// <summary>Written <c>LOCK t</c>, without the word TABLE.</summary>
		[Spelling] public bool TableOmitted { get; init; }
	}

	/// <summary>PostgreSQL: DISCARD. <c>DISCARD ALL | PLANS | SEQUENCES | TEMP</c>.</summary>
	public sealed record Discard(DiscardTarget Target) : Statement
	{
		/// <summary>Written <c>DISCARD TEMPORARY</c>, the long word.</summary>
		[Spelling] public bool Temporary { get; init; }
	}

	public sealed partial record FetchCursor
	{
		/// <summary>PostgreSQL: FETCH. Written <c>FETCH … IN c</c>, the word IN where FROM may stand.</summary>
		[Spelling] public bool InKeyword { get; init; }
	}

	/// <summary>PostgreSQL: MOVE. <c>MOVE FORWARD ALL IN c</c>: a FETCH that moves the cursor and gives no row back; the same words.</summary>
	public sealed record MoveCursor : Statement
	{
		public FetchOrientation? Orientation { get; init; }
		public bool FromKeyword { get; init; }

		/// <summary>Written <c>MOVE … IN c</c>, the word IN where FROM may stand.</summary>
		[Spelling] public bool InKeyword { get; init; }

		public required CursorReference Cursor { get; init; }
	}

	/// <summary>PostgreSQL: CLOSE. <c>CLOSE ALL</c>: every open cursor.</summary>
	public sealed record CloseAllCursors : Statement;

	/// <summary>
	/// PostgreSQL: VACUUM. <c>VACUUM (FULL, ANALYZE) t (a, b), u</c>: the options, each a name and perhaps a value, and the
	/// relations with their columns; empty where the whole database is meant.
	/// </summary>
	public sealed record Vacuum(SqlList<Option> Options, SqlList<VacuumRelation> Relations) : Statement
	{
		/// <summary>Written <c>VACUUM FULL FREEZE VERBOSE ANALYZE …</c>, the options as bare words in that order rather than in brackets.</summary>
		[Spelling] public bool Bare { get; init; }
	}

	/// <summary>PostgreSQL: ANALYZE. <c>ANALYZE (VERBOSE) t (a), u</c>: the options and the relations, as VACUUM has them.</summary>
	public sealed record Analyze(SqlList<Option> Options, SqlList<VacuumRelation> Relations) : Statement
	{
		/// <summary>Written <c>ANALYZE VERBOSE …</c>, the one option as a bare word rather than in brackets.</summary>
		[Spelling] public bool Bare { get; init; }

		/// <summary>Written <c>ANALYSE</c>.</summary>
		[Spelling] public bool British { get; init; }
	}

	/// <summary>
	/// PostgreSQL: COPY. <c>COPY t (a, b) FROM 'file' WITH (FORMAT csv)</c>, <c>COPY (SELECT …) TO STDOUT</c>: a table and its
	/// columns or a query, the direction, the file (or a program, or the standard stream) and the options. The old option
	/// words — <c>CSV HEADER DELIMITER ','</c> — are the same options under the names the bracketed form gives them, and
	/// <c>Bare</c> says they were written as words. <c>FORCE_QUOTE *</c> holds <see cref="Expression.Asterisk"/>.
	/// </summary>
	public sealed record Copy : Statement
	{
		public TableSource? Table { get; init; }
		public SqlList<Identifier> Columns { get; init; } = [];
		public Query? Query { get; init; }
		public required CopyDirection Direction { get; init; }

		/// <summary>The file, or the program's command line where <c>Program</c> is set; null where the standard stream is meant.</summary>
		public LiteralValue.String? File { get; init; }
		public bool Program { get; init; }

		/// <summary><c>STDIN</c> or <c>STDOUT</c>, where no file was named; the grammar reads either with either direction, as the server's does.</summary>
		public StandardStream? Stream { get; init; }

		public SqlList<Option> Options { get; init; } = [];

		/// <summary>The condition after WHERE, which only a COPY FROM takes.</summary>
		public Expression? Where { get; init; }

		/// <summary>Written with <c>WITH</c> before the options.</summary>
		[Spelling] public bool WithKeyword { get; init; }

		/// <summary>Written with the old option words rather than the bracketed list; set only where an old word was read.</summary>
		[Spelling] public bool Bare { get; init; }

		/// <summary>Written <c>COPY BINARY t …</c>: the option <c>format binary</c> as the word before the table.</summary>
		[Spelling] public bool BinaryBeforeTable { get; init; }

		/// <summary>Written <c>… DELIMITERS 'x'</c> after the file: the option <c>delimiter</c> in the old clause of its own.</summary>
		[Spelling] public bool DelimitersClause { get; init; }
	}
}

public sealed partial record CursorProperties
{
	/// <summary>PostgreSQL: DECLARE. <c>BINARY</c>: the rows in binary form.</summary>
	public bool Binary { get; init; }
}

public sealed partial record FetchOrientation
{
	/// <summary>PostgreSQL: FETCH. <c>FORWARD ALL</c>, <c>BACKWARD ALL</c>: every row that way; false where a count or nothing was written.</summary>
	public bool All { get; init; }

	/// <summary>PostgreSQL: FETCH. Written <c>FETCH 5 c</c> or <c>FETCH ALL c</c>, the word FORWARD left out.</summary>
	[Spelling] public bool WordOmitted { get; init; }
}

/// <summary>PostgreSQL: VACUUM, ANALYZE. One relation and the columns named after it, <c>t (a, b)</c>; the columns empty where none were.</summary>
public sealed record VacuumRelation(TableSource Relation, SqlList<Identifier> Columns) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>PostgreSQL: COPY. Which way the rows go.</summary>
public enum CopyDirection { From, To }

/// <summary>PostgreSQL: COPY. The standard stream named where no file is.</summary>
public enum StandardStream { Stdin, Stdout }

public abstract partial record TransactionMode
{
	/// <summary>PostgreSQL: SET TRANSACTION. <c>DEFERRABLE</c> or <c>NOT DEFERRABLE</c>.</summary>
	public sealed record Deferrable(bool Not) : TransactionMode;
}

/// <summary>PostgreSQL: SET. The scope word of a SET: <c>SESSION</c>, the default said, or <c>LOCAL</c>, the transaction's.</summary>
public enum SetScope { Session, Local }

/// <summary>
/// PostgreSQL: SET, SHOW, RESET. A run-time parameter the statement has its own words for: <c>TIME ZONE</c> is <c>timezone</c>,
/// <c>TRANSACTION ISOLATION LEVEL</c> is <c>transaction_isolation</c>, <c>SESSION AUTHORIZATION</c> is <c>session_authorization</c>,
/// <c>XML OPTION</c> is <c>xmloption</c>. The tree holds the parameter's name; the spelling says the words were written.
/// </summary>
public enum ParameterSpelling { TimeZone, TransactionIsolationLevel, SessionAuthorization, XmlOption }

/// <summary>PostgreSQL: LOCK. The lock modes, from the weakest to the strongest.</summary>
public enum LockMode { AccessShare, RowShare, RowExclusive, ShareUpdateExclusive, Share, ShareRowExclusive, Exclusive, AccessExclusive }

/// <summary>PostgreSQL: DISCARD. What a DISCARD lets go of.</summary>
public enum DiscardTarget { All, Plans, Sequences, Temp }
