using System;

namespace DotGram.Sql.Ast;

// PostgreSQL's parts of the table statements: what CREATE TABLE adds to the standard's records — the partitions, the
// inheritance, the storage, the exclusion constraint — and the key column an index, an exclusion constraint, a
// partition key and ON CONFLICT share. Every record and member here carries `PostgreSQL:` with the title of the page of
// the PostgreSQL manual it comes from.

public sealed partial record ParameterDefinition
{
	/// <summary>PostgreSQL: CREATE FUNCTION. <c>IN OUT</c> written in two words, with Mode InOut.</summary>
	[Spelling] public bool InOutTwoWords { get; init; }
}

/// <summary>
/// PostgreSQL: CREATE INDEX, CREATE TABLE, INSERT. One key of an index, of an exclusion constraint, of a partition key or
/// of an ON CONFLICT target: a column by its name, a call, or an expression in brackets, with its collation, its
/// operator class and that class's options, its direction and where its nulls sort.
/// </summary>
/// <remarks>
/// T-SQL's index keys are elements too, with a column for <see cref="Key"/>, a direction at most, and nothing else;
/// a key ORDER BY sorts by is a <see cref="SortItem"/>, which has no collation or operator class.
/// <see cref="OperatorClassOptions"/> is empty where none were written, and none can be without an operator class.
/// </remarks>
public sealed record IndexElement(Expression Key, CollationName? Collation = null, QualifiedName? OperatorClass = null, SqlList<Option> OperatorClassOptions = default, SortDirection? Direction = null, NullOrdering? Nulls = null) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
