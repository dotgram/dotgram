namespace DotGram.Sql;

/// <summary>What a statement does, which a reader filters on without naming every record.</summary>
public enum StatementCategory
{
	/// <summary>A query: <c>SELECT</c>, and the clauses the statement wraps around it.</summary>
	Query,

	/// <summary>Rows changed: <c>INSERT</c>, <c>UPDATE</c>, <c>DELETE</c>, <c>MERGE</c>.</summary>
	Dml,

	/// <summary>An object defined, changed or removed: <c>CREATE</c>, <c>ALTER</c>, <c>DROP</c>.</summary>
	Ddl,

	/// <summary>
	/// Who may do what: <c>GRANT</c>, <c>DENY</c>, <c>REVOKE</c>, and the logins, users and roles
	/// they name.
	/// </summary>
	Dcl,

	/// <summary>
	/// The flow of a batch: a block, <c>IF</c>, <c>WHILE</c>, <c>TRY</c>, <c>GOTO</c>, <c>RETURN</c>,
	/// <c>WAITFOR</c>, and what a batch says as it goes — <c>PRINT</c>, <c>RAISERROR</c>, <c>THROW</c>.
	/// </summary>
	Control,

	/// <summary>A transaction begun, committed, rolled back or saved.</summary>
	Transaction,

	/// <summary>The session: an option <c>SET</c>, a database <c>USE</c>d, someone else run as.</summary>
	Session,

	/// <summary><c>EXECUTE</c>: a procedure called, or a string run.</summary>
	Execute,

	/// <summary>
	/// The server looked after: <c>BACKUP</c>, <c>RESTORE</c>, <c>CHECKPOINT</c>,
	/// <c>UPDATE STATISTICS</c>.
	/// </summary>
	Admin,

	/// <summary>A variable or a cursor declared, and a variable set.</summary>
	Declaration,
}
