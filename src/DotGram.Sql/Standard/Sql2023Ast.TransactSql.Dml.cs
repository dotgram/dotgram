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
