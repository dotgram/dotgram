using System;

namespace DotGram.Sql;

/// <summary>
/// The groups Microsoft's page of the SET statements puts them in, as flags, so that a
/// statement whose list mixes them can say all of its groups.
/// </summary>
[Flags]
internal enum SetCategory
{
	/// <summary>No setting.</summary>
	None = 0,

	/// <summary><c>DATEFIRST</c>, <c>DATEFORMAT</c>.</summary>
	DateAndTime = 1,

	/// <summary><c>DEADLOCK_PRIORITY</c>, <c>LOCK_TIMEOUT</c>.</summary>
	Locking = 2,

	/// <summary>The rest: a language, a table's identity, the FIPS flagger and others.</summary>
	Miscellaneous = 4,

	/// <summary><c>NOCOUNT</c>, <c>PARSEONLY</c>, <c>ROWCOUNT</c>, <c>TEXTSIZE</c> and the others that shape how a query runs.</summary>
	QueryExecution = 8,

	/// <summary>The ISO settings: <c>ANSI_NULLS</c>, <c>ANSI_PADDING</c>, <c>ANSI_WARNINGS</c> and theirs.</summary>
	IsoSettings = 16,

	/// <summary><c>STATISTICS IO</c>, <c>SHOWPLAN_XML</c>, <c>FORCEPLAN</c> and the others that report on a query.</summary>
	Statistics = 32,

	/// <summary><c>TRANSACTION ISOLATION LEVEL</c>, <c>XACT_ABORT</c>, <c>IMPLICIT_TRANSACTIONS</c>.</summary>
	Transactions = 64,
}

/// <summary>Which join (§7.7), and the two T-SQL adds.</summary>
/// <remarks>
/// An apply reads its right side once per row of its left, which is a question about
/// evaluation rather than shape — but it is written differently and refused where a join
/// would be read, so the tree says which was written.
/// </remarks>
internal enum SqlJoin
{
	/// <summary>
	/// No word was written — a bare <c>JOIN</c>. What that means is an inner join, and
	/// meaning it is not the parser's to say: <c>JOIN</c> and <c>INNER JOIN</c> are two
	/// texts, and only one of them was typed.
	/// </summary>
	Unspecified,

	Cross, Inner, Left, Right, Full, Union, CrossApply, OuterApply,
}

/// <summary>Which way a sort was asked for, and whether it was asked for at all (§10.10).</summary>
internal enum SqlOrder
{
	Unspecified, Ascending, Descending,
}

/// <summary>What a literal is, where the text alone does not say.</summary>
internal enum SqlLiteralKind
{
	Number, Text, National, Bit, Hex, Date, Time, Timestamp, Interval,
	Null, Default, Parameter, Special,
}
