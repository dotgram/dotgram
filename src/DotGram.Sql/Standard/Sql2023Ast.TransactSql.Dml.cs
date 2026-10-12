using System;

namespace DotGram.Sql.Ast;

// T-SQL's parts of the data change statements (proposals 2, 3, 4 and 8): what INSERT, UPDATE, DELETE and MERGE
// add to the standard's records, and the statements of its own that change rows.

public abstract partial record Statement
{
	public partial record Insert
	{
		/// <summary>
		/// T-SQL: WITH common_table_expression (Transact-SQL). The common table expressions or XML namespaces
		/// written before the statement.
		/// </summary>
		public WithClause? With { get; init; }

		/// <summary>T-SQL: OUTPUT clause (Transact-SQL). After the column list, before the rows.</summary>
		public OutputClause? Output { get; init; }

		/// <summary>T-SQL: INSERT (Transact-SQL). <c>OPTION (…)</c> after the rows.</summary>
		public SqlList<QueryHint> Hints { get; init; } = [];

		/// <summary>
		/// T-SQL: INSERT (Transact-SQL). The word before the target: <c>INTO</c>, <c>OVER</c>, which the grammar
		/// reads and no page describes, or null where none was written.
		/// </summary>
		[Spelling] public IntoKeyword? Into { get; init; } = IntoKeyword.Into;
	}

	public partial record Update
	{
		/// <summary>
		/// T-SQL: WITH common_table_expression (Transact-SQL). The common table expressions or XML namespaces
		/// written before the statement.
		/// </summary>
		public WithClause? With { get; init; }

		/// <summary>T-SQL: OUTPUT clause (Transact-SQL). After SET, before the second FROM.</summary>
		public OutputClause? Output { get; init; }

		/// <summary>T-SQL: UPDATE (Transact-SQL). <c>OPTION (…)</c>, last.</summary>
		public SqlList<QueryHint> Hints { get; init; } = [];

		/// <summary>
		/// T-SQL: UPDATE (Transact-SQL). The second FROM, the tables the rows to change are found by joining.
		/// </summary>
		public FromClause? From { get; init; }
	}

	public partial record Delete
	{
		/// <summary>
		/// T-SQL: WITH common_table_expression (Transact-SQL). The common table expressions or XML namespaces
		/// written before the statement.
		/// </summary>
		public WithClause? With { get; init; }

		/// <summary>T-SQL: OUTPUT clause (Transact-SQL). After the target, before the second FROM.</summary>
		public OutputClause? Output { get; init; }

		/// <summary>T-SQL: DELETE (Transact-SQL). <c>OPTION (…)</c>, last.</summary>
		public SqlList<QueryHint> Hints { get; init; } = [];

		/// <summary>
		/// T-SQL: DELETE (Transact-SQL). The second FROM, the tables the rows to remove are found by joining.
		/// </summary>
		public FromClause? From { get; init; }

		/// <summary>T-SQL: DELETE (Transact-SQL). <c>DELETE t</c> against <c>DELETE FROM t</c>.</summary>
		[Spelling] public bool FromOmitted { get; init; }
	}

	public partial record Merge
	{
		/// <summary>
		/// T-SQL: WITH common_table_expression (Transact-SQL). The common table expressions or XML namespaces
		/// written before the statement.
		/// </summary>
		public WithClause? With { get; init; }

		/// <summary>T-SQL: OUTPUT clause (Transact-SQL). After the last WHEN.</summary>
		public OutputClause? Output { get; init; }

		/// <summary>T-SQL: MERGE (Transact-SQL). <c>OPTION (…)</c>, last.</summary>
		public SqlList<QueryHint> Hints { get; init; } = [];

		/// <summary>T-SQL: MERGE (Transact-SQL). <c>MERGE t</c> against <c>MERGE INTO t</c>.</summary>
		[Spelling] public bool IntoOmitted { get; init; }
	}
}

/// <summary>
/// T-SQL: INSERT (Transact-SQL). The word written before an INSERT's target: <c>INTO</c>, or <c>OVER</c>. Nullable
/// with no <c>None</c> member, as <see cref="TransactionKeyword"/> is: null is no word.
/// </summary>
public enum IntoKeyword { Into, Over }

public abstract partial record MergeClause
{
	/// <summary>
	/// T-SQL: MERGE (Transact-SQL). <c>WHEN NOT MATCHED BY SOURCE [AND c] THEN UPDATE SET … | DELETE</c>: a row of the
	/// target no row of the source matched, which may be changed or removed as a matched one may.
	/// </summary>
	public sealed record NotMatchedBySource(MergeMatchedAction Action) : MergeClause;

	public partial record NotMatched
	{
		/// <summary>T-SQL: MERGE (Transact-SQL). <c>WHEN NOT MATCHED BY TARGET</c>, which means what the words without BY TARGET mean.</summary>
		[Spelling] public bool ByTarget { get; init; }
	}
}

public sealed partial record MergeInsertAction
{
	/// <summary>
	/// T-SQL: MERGE (Transact-SQL). <c>INSERT DEFAULT VALUES</c>: a row of defaults; the writer refuses it with columns or
	/// values.
	/// </summary>
	public bool DefaultValues { get; init; }
}

/// <remarks>
/// T-SQL: UPDATE (Transact-SQL). What T-SQL's SET adds. <c>SET @v = e</c> and <c>SET @v += e</c> are a
/// <see cref="Variable"/> with no targets, the operator the variable's. <c>SET @v = c = e</c> and <c>SET @v = c += e</c>
/// are a variable and one target, the variable taking the column's new value and the operator the column's (the
/// variable's <c>=</c> is always plain). A method called on a column, <c>SET c.WRITE (@x, 0, NULL)</c> or
/// <c>SET p.SetXY (1, 2)</c>, has neither targets nor a variable: its <see cref="Value"/> is the call, an
/// <see cref="Expression.Member"/> on the column's one-part reference, and the column it writes is that receiver,
/// not in <see cref="Targets"/>.
/// </remarks>
public sealed partial record Assignment
{
	/// <summary>T-SQL: UPDATE (Transact-SQL). The operator the targets are assigned with: <c>=</c>, <c>+=</c>, …, <c>||=</c>.</summary>
	public AssignmentOperator Operator { get; init; } = AssignmentOperator.Assign;

	/// <summary>T-SQL: UPDATE (Transact-SQL). The variable assigned, alone or with the column it takes the new value of.</summary>
	public Expression.Variable? Variable { get; init; }
}
