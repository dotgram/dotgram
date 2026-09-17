using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Handwritten;

// ISO/IEC 9075-2:2023 §14's data change statements — the insert, the update, the delete, the merge
// and the truncate — and §7.6's data change delta table, which is what one of them changed, read as
// a table.
partial class HandSqlStandard
{
	// ── The publications ───────────────────────────────────────────────────────

	public static Statement.Insert ParseInsertStatement(string input) =>
		TryParseInsertStatement(input, out var value) ? value : throw Refused(input, "insert statement");

	public static bool TryParseInsertStatement(string input, out Statement.Insert value)
	{
		var cursor = new SqlCursor(input);

		if (InsertStatement(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	public static Statement.Update ParseUpdateStatementSearched(string input) =>
		TryParseUpdateStatementSearched(input, out var value) ? value : throw Refused(input, "update statement: searched");

	public static bool TryParseUpdateStatementSearched(string input, out Statement.Update value)
	{
		var cursor = new SqlCursor(input);

		if (UpdateStatement(ref cursor, out value, false) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	public static Statement.Update ParseUpdateStatementPositioned(string input) =>
		TryParseUpdateStatementPositioned(input, out var value) ? value : throw Refused(input, "update statement: positioned");

	public static bool TryParseUpdateStatementPositioned(string input, out Statement.Update value)
	{
		var cursor = new SqlCursor(input);

		if (UpdateStatement(ref cursor, out value, true) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	public static Statement.Delete ParseDeleteStatementSearched(string input) =>
		TryParseDeleteStatementSearched(input, out var value) ? value : throw Refused(input, "delete statement: searched");

	public static bool TryParseDeleteStatementSearched(string input, out Statement.Delete value)
	{
		var cursor = new SqlCursor(input);

		if (DeleteStatement(ref cursor, out value, false) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	public static Statement.Delete ParseDeleteStatementPositioned(string input) =>
		TryParseDeleteStatementPositioned(input, out var value) ? value : throw Refused(input, "delete statement: positioned");

	public static bool TryParseDeleteStatementPositioned(string input, out Statement.Delete value)
	{
		var cursor = new SqlCursor(input);

		if (DeleteStatement(ref cursor, out value, true) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	public static Statement.Merge ParseMergeStatement(string input) =>
		TryParseMergeStatement(input, out var value) ? value : throw Refused(input, "merge statement");

	public static bool TryParseMergeStatement(string input, out Statement.Merge value)
	{
		var cursor = new SqlCursor(input);

		if (MergeStatement(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	public static Statement.TruncateTable ParseTruncateTableStatement(string input) =>
		TryParseTruncateTableStatement(input, out var value) ? value : throw Refused(input, "truncate table statement");

	public static bool TryParseTruncateTableStatement(string input, out Statement.TruncateTable value)
	{
		var cursor = new SqlCursor(input);

		if (TruncateTableStatement(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	// ── §14.8 Delete, §14.9 Truncate table ─────────────────────────────────────

	/// <summary>
	/// <c>&lt;delete statement: searched&gt;</c> and <c>&lt;delete statement: positioned&gt;</c>: a
	/// positioned one ends in <c>WHERE CURRENT OF</c>, and names no period.
	/// </summary>
	static bool DeleteStatement(ref SqlCursor cursor, out Statement.Delete statement, bool positioned)
	{
		var save = cursor;

		statement = null!;

		if (!cursor.Take(SqlWord.Delete) || !cursor.Take(SqlWord.From) || !TargetTable(ref cursor, out var target))
		{
			cursor = save;

			return false;
		}

		PeriodPortion? portion = null;

		if (!positioned)
			portion = ForPortionOf(ref cursor);

		var alias = TargetCorrelation(ref cursor);

		if (positioned)
		{
			if (cursor.Take(SqlWord.Where) && cursor.Take(SqlWord.Current) && cursor.Take(SqlWord.Of) && CursorName(ref cursor, out var name))
			{
				statement = new Statement.Delete { Target = target, Alias = alias, CurrentOf = new CursorReference(name) };

				return true;
			}

			cursor = save;

			return false;
		}

		Expression? where = null;

		if (cursor.Take(SqlWord.Where) && !SearchCondition(ref cursor, out where))
		{
			cursor = save;

			return false;
		}

		statement = new Statement.Delete { Target = target, Portion = portion, Alias = alias, Where = where };

		return true;
	}

	static bool TruncateTableStatement(ref SqlCursor cursor, out Statement.TruncateTable statement)
	{
		var save = cursor;

		statement = null!;

		if (!cursor.Take(SqlWord.Truncate) || !cursor.Take(SqlWord.Table) || !TargetTable(ref cursor, out var target))
		{
			cursor = save;

			return false;
		}

		IdentityRestart? identity = null;

		var marked = cursor;

		if (cursor.TakeWord("CONTINUE"))
			identity = IdentityRestart.Continue;
		else if (cursor.TakeWord("RESTART"))
			identity = IdentityRestart.Restart;

		if (identity is not null && !cursor.Take(SqlWord.Identity))
		{
			cursor   = marked;
			identity = null;
		}

		statement = new Statement.TruncateTable { Target = target, Identity = identity };

		return true;
	}

	/// <summary><c>&lt;target table&gt;</c>: a table's name, or <c>ONLY (t)</c>.</summary>
	static bool TargetTable(ref SqlCursor cursor, out TableSource target)
	{
		var save = cursor;

		target = null!;

		if (cursor.Take(SqlWord.Only))
		{
			if (cursor.Take(SqlTokenKind.LeftParen) && TableName(ref cursor, out var only) && cursor.Take(SqlTokenKind.RightParen))
			{
				target = new TableSource.Named(only) { Only = true };

				return true;
			}

			cursor = save;

			return false;
		}

		if (TableName(ref cursor, out var name))
		{
			target = new TableSource.Named(name);

			return true;
		}

		return false;
	}

	/// <summary>The correlation name a delete, an update and a merge may give their target.</summary>
	static Alias? TargetCorrelation(ref SqlCursor cursor)
	{
		var save    = cursor;
		var keyword = cursor.Take(SqlWord.As);

		if (Identifier(ref cursor, out var name))
			return new Alias(name, null, keyword);

		cursor = save;

		return null;
	}

	/// <summary>The portion of an application time period a searched delete or update may name.</summary>
	static PeriodPortion? ForPortionOf(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.For) && cursor.Take(SqlWord.Portion) && cursor.Take(SqlWord.Of) && Identifier(ref cursor, out var name) &&
			cursor.Take(SqlWord.From) && Datetime(ref cursor, out var from) && cursor.Take(SqlWord.To) && Datetime(ref cursor, out var to))
			return new PeriodPortion(name, from, to);

		cursor = save;

		return null;
	}

	/// <summary><c>&lt;cursor name&gt;</c>, which is a local qualified name.</summary>
	static bool CursorName(ref SqlCursor cursor, out QualifiedName name)
	{
		var save = cursor;

		if (cursor.Word == SqlWord.Module)
		{
			var word = cursor.TextOf(cursor.Token);

			cursor.Take();

			if (cursor.Take(SqlTokenKind.Dot) && Identifier(ref cursor, out var qualified))
			{
				name = new QualifiedName([new Identifier(word), qualified]);

				return true;
			}

			cursor = save;
			name   = null!;

			return false;
		}

		if (Identifier(ref cursor, out var written))
		{
			name = new QualifiedName([written]);

			return true;
		}

		name = null!;

		return false;
	}

	// ── §14.11 Insert statement ────────────────────────────────────────────────

	static bool InsertStatement(ref SqlCursor cursor, out Statement.Insert statement)
	{
		var save = cursor;

		statement = null!;

		if (!cursor.Take(SqlWord.Insert) || !cursor.Take(SqlWord.Into) || !TableName(ref cursor, out var name))
		{
			cursor = save;

			return false;
		}

		// `DEFAULT VALUES`, and otherwise the columns, the override clause and what the columns are given.
		var defaults = cursor;

		if (cursor.Take(SqlWord.Default) && cursor.Take(SqlWord.Values))
		{
			statement = new Statement.Insert { Target = new TableSource.Named(name) };

			return true;
		}

		cursor = defaults;

		IReadOnlyList<Identifier>? columns = null;

		if (cursor.Kind == SqlTokenKind.LeftParen)
		{
			var bracket = cursor;

			cursor.Take();

			if (ColumnNameList(ref cursor, out var written) && cursor.Take(SqlTokenKind.RightParen))
				columns = written;
			else
				cursor = bracket;
		}

		var over = OverrideClause(ref cursor);

		if (InsertValues(ref cursor, out var source))
		{
			statement = new Statement.Insert
			{
				Target      = new TableSource.Named(name),
				Columns     = columns ?? [],
				Override    = over,
				SourceValue = source,
			};

			return true;
		}

		cursor = save;

		return false;
	}

	/// <summary>
	/// What an insert's columns are given: rows, or a query. The constructor is asked first, since it
	/// reads what a query's table value constructor does and <c>DEFAULT</c> too.
	/// </summary>
	static bool InsertValues(ref SqlCursor cursor, out InsertSource source)
	{
		var save = cursor;

		if (ContextuallyTypedTableValueConstructor(ref cursor, out var rows))
		{
			// The rows are a query's table value constructor too, and a query may go on where the
			// constructor stops: `VALUES 1 ORDER BY a` is a query, and the BNF's ordered choice
			// comes back for it when the insert statement it left behind does not end there.
			if (cursor.Word is not (SqlWord.Order or SqlWord.Offset or SqlWord.Fetch or SqlWord.Union or SqlWord.Except or SqlWord.Intersect))
			{
				source = new InsertSource.Values(rows);

				return true;
			}

			cursor = save;
		}

		if (QueryExpression(ref cursor, out var query))
		{
			source = new InsertSource.Query(query);

			return true;
		}

		source = null!;

		return false;
	}

	static OverrideKind? OverrideClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.TakeWord("OVERRIDING"))
		{
			var user = cursor.Take(SqlWord.User);

			if ((user || cursor.TakeWord("SYSTEM")) && cursor.Take(SqlWord.Value))
				return user ? OverrideKind.UserValue : OverrideKind.SystemValue;
		}

		cursor = save;

		return null;
	}

	/// <summary>
	/// <c>&lt;contextually typed table value constructor&gt;</c>: rows whose values may be
	/// <c>DEFAULT</c>, <c>NULL</c> or an empty collection, which take their type from where they go.
	/// </summary>
	static bool ContextuallyTypedTableValueConstructor(ref SqlCursor cursor, out IReadOnlyList<RowValue> rows)
	{
		var save    = cursor;
		var written = new List<RowValue>();

		rows = written;

		if (!cursor.Take(SqlWord.Values))
			return false;

		while (true)
		{
			if (!ContextuallyTypedRowValueExpression(ref cursor, out var value))
			{
				cursor = save;

				return false;
			}

			written.Add(value is Expression.Row row ? new RowValue(row.Items, row.RowKeyword) : new RowValue([value]));

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		return true;
	}

	static bool ContextuallyTypedRowValueExpression(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		if (ContextuallyTypedValueSpecification(ref cursor, out value))
			return true;

		if (cursor.Take(SqlTokenKind.LeftParen))
		{
			// A bracket holds such a specification alone, or two or more elements.
			if (ContextuallyTypedValueSpecification(ref cursor, out var alone) && cursor.Take(SqlTokenKind.RightParen))
			{
				value = new Expression.Parenthesized(alone);

				return true;
			}

			cursor = save;
			cursor.Take();

			var items = new List<Expression>();

			while (true)
			{
				if (!ContextuallyTypedElement(ref cursor, out var item))
					break;

				items.Add(item);

				if (!cursor.Take(SqlTokenKind.Comma))
					break;
			}

			if (items.Count > 1 && cursor.Take(SqlTokenKind.RightParen))
			{
				value = new Expression.Row(items);

				return true;
			}

			cursor = save;
		}

		if (cursor.Take(SqlWord.Row))
		{
			if (cursor.Take(SqlTokenKind.LeftParen))
			{
				var items = new List<Expression>();

				while (true)
				{
					if (!ContextuallyTypedElement(ref cursor, out var item))
						break;

					items.Add(item);

					if (!cursor.Take(SqlTokenKind.Comma))
						break;
				}

				if (items.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
				{
					value = new Expression.Row(items, true);

					return true;
				}
			}

			cursor = save;
		}

		if (ValueExpression(ref cursor, out var read))
		{
			value = read.Node;

			return true;
		}

		cursor = save;
		value  = null!;

		return false;
	}

	static bool ContextuallyTypedElement(ref SqlCursor cursor, out Expression value)
	{
		if (ContextuallyTypedValueSpecification(ref cursor, out value))
			return true;

		if (ValueExpression(ref cursor, out var read))
		{
			value = read.Node;

			return true;
		}

		value = null!;

		return false;
	}

	// ── §14.12 Merge statement ─────────────────────────────────────────────────

	static bool MergeStatement(ref SqlCursor cursor, out Statement.Merge statement)
	{
		var save = cursor;

		statement = null!;

		if (!cursor.Take(SqlWord.Merge) || !cursor.Take(SqlWord.Into) || !TargetTable(ref cursor, out var target))
		{
			cursor = save;

			return false;
		}

		var alias = TargetCorrelation(ref cursor);

		if (!cursor.Take(SqlWord.Using) || !TableReference(ref cursor, out var source) || !cursor.Take(SqlWord.On) ||
			!SearchCondition(ref cursor, out var on))
		{
			cursor = save;

			return false;
		}

		var clauses = new List<MergeClause>();

		while (MergeWhenClause(ref cursor, out var clause))
			clauses.Add(clause);

		if (clauses.Count == 0)
		{
			cursor = save;

			return false;
		}

		statement = new Statement.Merge { Target = target, Alias = alias, SourceTable = source, On = on, Clauses = clauses };

		return true;
	}

	static bool MergeWhenClause(ref SqlCursor cursor, out MergeClause clause)
	{
		var save = cursor;

		clause = null!;

		if (!cursor.Take(SqlWord.When))
			return false;

		var matched = !cursor.Take(SqlWord.Not);

		if (!cursor.TakeWord("MATCHED"))
		{
			cursor = save;

			return false;
		}

		Expression? condition = null;

		if (cursor.Take(SqlWord.And) && !SearchCondition(ref cursor, out condition))
		{
			cursor = save;

			return false;
		}

		if (!cursor.Take(SqlWord.Then))
		{
			cursor = save;

			return false;
		}

		if (matched)
		{
			if (cursor.Take(SqlWord.Update) && cursor.Take(SqlWord.Set) && SetClauseList(ref cursor, out var assignments))
			{
				clause = new MergeClause.Matched(new MergeMatchedAction.Update(assignments)) { Condition = condition };

				return true;
			}

			if (cursor.Take(SqlWord.Delete))
			{
				clause = new MergeClause.Matched(new MergeMatchedAction.Delete()) { Condition = condition };

				return true;
			}

			cursor = save;

			return false;
		}

		if (MergeInsertSpecification(ref cursor, out var insert))
		{
			clause = new MergeClause.NotMatched(insert) { Condition = condition };

			return true;
		}

		cursor = save;

		return false;
	}

	static bool MergeInsertSpecification(ref SqlCursor cursor, out MergeInsertAction action)
	{
		var save = cursor;

		action = null!;

		if (!cursor.Take(SqlWord.Insert))
			return false;

		IReadOnlyList<Identifier>? columns = null;

		if (cursor.Kind == SqlTokenKind.LeftParen)
		{
			var bracket = cursor;

			cursor.Take();

			if (ColumnNameList(ref cursor, out var written) && cursor.Take(SqlTokenKind.RightParen))
				columns = written;
			else
				cursor = bracket;
		}

		var over = OverrideClause(ref cursor);

		if (cursor.Take(SqlWord.Values) && cursor.Take(SqlTokenKind.LeftParen))
		{
			var values = new List<Expression>();

			while (true)
			{
				if (!ContextuallyTypedElement(ref cursor, out var value))
					break;

				values.Add(value);

				if (!cursor.Take(SqlTokenKind.Comma))
					break;
			}

			if (values.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
			{
				action = new MergeInsertAction(columns ?? [], over, values);

				return true;
			}
		}

		cursor = save;

		return false;
	}

	// ── §14.13, §14.14 Update statement ────────────────────────────────────────

	static bool UpdateStatement(ref SqlCursor cursor, out Statement.Update statement, bool positioned)
	{
		var save = cursor;

		statement = null!;

		if (!cursor.Take(SqlWord.Update) || !TargetTable(ref cursor, out var target))
		{
			cursor = save;

			return false;
		}

		PeriodPortion? portion = null;

		if (!positioned)
			portion = ForPortionOf(ref cursor);

		var alias = TargetCorrelation(ref cursor);

		if (!cursor.Take(SqlWord.Set) || !SetClauseList(ref cursor, out var assignments))
		{
			cursor = save;

			return false;
		}

		if (positioned)
		{
			if (cursor.Take(SqlWord.Where) && cursor.Take(SqlWord.Current) && cursor.Take(SqlWord.Of) && CursorName(ref cursor, out var name))
			{
				statement = new Statement.Update { Target = target, Alias = alias, Assignments = assignments, CurrentOf = new CursorReference(name) };

				return true;
			}

			cursor = save;

			return false;
		}

		Expression? where = null;

		if (cursor.Take(SqlWord.Where) && !SearchCondition(ref cursor, out where))
		{
			cursor = save;

			return false;
		}

		statement = new Statement.Update { Target = target, Portion = portion, Alias = alias, Assignments = assignments, Where = where };

		return true;
	}

	// ── §14.15 Set clause list ─────────────────────────────────────────────────

	static bool SetClauseList(ref SqlCursor cursor, out IReadOnlyList<Assignment> assignments)
	{
		var save    = cursor;
		var written = new List<Assignment>();

		assignments = written;

		while (true)
		{
			if (!SetClause(ref cursor, out var assignment))
			{
				cursor = save;

				return false;
			}

			written.Add(assignment);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		return true;
	}

	/// <summary>Several targets and a row, or a target and a value.</summary>
	static bool SetClause(ref SqlCursor cursor, out Assignment assignment)
	{
		var save = cursor;

		assignment = null!;

		if (cursor.Take(SqlTokenKind.LeftParen))
		{
			var targets = new List<AssignmentTarget>();

			while (true)
			{
				if (!SetTarget(ref cursor, out var one))
					break;

				targets.Add(one);

				if (!cursor.Take(SqlTokenKind.Comma))
					break;
			}

			if (targets.Count > 0 && cursor.Take(SqlTokenKind.RightParen) && cursor.Take(SqlTokenKind.Equal) &&
				ContextuallyTypedRowValueExpression(ref cursor, out var row))
			{
				assignment = new Assignment(targets, row, true);

				return true;
			}

			cursor = save;

			return false;
		}

		if (SetTarget(ref cursor, out var target) && cursor.Take(SqlTokenKind.Equal) && ContextuallyTypedElement(ref cursor, out var value))
		{
			assignment = new Assignment([target], value);

			return true;
		}

		cursor = save;

		return false;
	}

	/// <summary>
	/// A column, an element of one, or a column's attribute set by a mutator, as often as there are
	/// attributes on the way.
	/// </summary>
	static bool SetTarget(ref SqlCursor cursor, out AssignmentTarget target)
	{
		var save = cursor;

		target = null!;

		if (!Identifier(ref cursor, out var column))
			return false;

		if (cursor.Kind is SqlTokenKind.LeftBracket or SqlTokenKind.LeftTrigraph)
		{
			var trigraphs = cursor.Kind == SqlTokenKind.LeftTrigraph;
			var bracket   = cursor;

			cursor.Take();

			if (SimpleValueSpecification(ref cursor, out var index) && cursor.Kind is SqlTokenKind.RightBracket or SqlTokenKind.RightTrigraph)
			{
				cursor.Take();

				target = new AssignmentTarget(new QualifiedName([column]), index, null, trigraphs);

				return true;
			}

			cursor = bracket;
		}

		List<Identifier>? path = null;

		while (true)
		{
			var dotted = cursor;

			if (!cursor.Take(SqlTokenKind.Dot) || !Identifier(ref cursor, out var method))
			{
				cursor = dotted;

				break;
			}

			(path ??= []).Add(method);
		}

		target = new AssignmentTarget(new QualifiedName([column]), null, path);

		return true;
	}

	// ── §7.6 Data change delta table ───────────────────────────────────────────

	/// <summary>What a data change statement changed, as a table.</summary>
	static bool DataChangeDeltaTable(ref SqlCursor cursor, out TableSource table)
	{
		var save = cursor;

		table = null!;

		// `FINAL`, `NEW` and `OLD`: §5.2 reserves the last two and not the first.
		var option =
			cursor.IsWord("FINAL")     ? ResultOption.Final :
			cursor.Word == SqlWord.New ? ResultOption.New :
			cursor.Word == SqlWord.Old ? ResultOption.Old :
			(ResultOption?)null;

		if (option is null)
			return false;

		cursor.Take();

		if (cursor.Take(SqlWord.Table) && cursor.Take(SqlTokenKind.LeftParen) && DataChangeStatement(ref cursor, out var change) &&
			cursor.Take(SqlTokenKind.RightParen))
		{
			table = new TableSource.DataChange(option.Value, change);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool DataChangeStatement(ref SqlCursor cursor, out Statement statement)
	{
		switch (cursor.Word)
		{
			case SqlWord.Delete:
			{
				if (DeleteStatement(ref cursor, out var deleted, false))
				{
					statement = deleted;

					return true;
				}

				break;
			}

			case SqlWord.Insert:
			{
				if (InsertStatement(ref cursor, out var inserted))
				{
					statement = inserted;

					return true;
				}

				break;
			}

			case SqlWord.Merge:
			{
				if (MergeStatement(ref cursor, out var merged))
				{
					statement = merged;

					return true;
				}

				break;
			}

			case SqlWord.Update:
			{
				if (UpdateStatement(ref cursor, out var updated, false))
				{
					statement = updated;

					return true;
				}

				break;
			}
		}

		statement = null!;

		return false;
	}
}
