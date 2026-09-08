using System;
using System.Collections.Generic;
using System.Text;

namespace DotGram.Parsers.Sql;

/// <summary>The tree back as SQL.</summary>
/// <remarks>
/// <para>
/// <b>What this is for.</b> A parser that builds a tree can be wrong in a way no refusal
/// count sees: it reads the text, answers yes, and builds something else. Printing closes
/// that hole — parse, print, and hold the result against what a second parser makes of the
/// same input. `benchmarks --roundtrip` does exactly that, with ScriptDom's own generator as
/// the normal form on both sides so that formatting cannot be mistaken for meaning.
/// </para>
/// <para>
/// <b>It prints what the tree holds, and nothing else.</b> `TOP`, `OVER`, the hints, the
/// windows, `OUTPUT`, a named query's `WITH` and the option lists of every DDL statement are
/// read and dropped at parse time (`docs/ast.md`), so they cannot come back here. That is not
/// a gap in this file; it is the tree's shape, and the round-trip measures it rather than
/// hiding it. Where a statement keeps only a name — most of the DDL — what comes out is the
/// statement without its options, which SQL Server may well refuse. The harness counts that
/// as a loss, which is what it is.
/// </para>
/// <para>
/// <b>Parentheses are written where precedence needs them and nowhere else.</b> The tree does
/// not record that somebody wrote `(a) + b`, so this prints `a + b`. Structure survives:
/// `(a + b) * c` is a `Multiply` over an `Add` and comes back with its brackets, because
/// without them it would be a different tree.
/// </para>
/// <para>
/// <b>The dialect is the latest one.</b> Where the standard and T-SQL spell one node two ways
/// — `CONVERT (v USING cs)` against `CONVERT (type, v)` — what comes out is T-SQL's, because
/// that is the dialect this is an oracle for.
/// </para>
/// </remarks>
public static class SqlWriter
{
	public static string Write(Statement statement)
	{
		var text = new StringBuilder();

		Put(text, statement);

		return text.ToString();
	}

	public static string Write(Query query)
	{
		var text = new StringBuilder();

		Put(text, query, 0);

		return text.ToString();
	}

	public static string Write(Expression expression)
	{
		var text = new StringBuilder();

		Put(text, expression, 0);

		return text.ToString();
	}

	public static string Write(TableReference source)
	{
		var text = new StringBuilder();

		Put(text, source);

		return text.ToString();
	}

	public static string Write(Clause clause)
	{
		var text = new StringBuilder();

		Put(text, clause);

		return text.ToString();
	}

	// ── Statements ──────────────────────────────────────────────────────────────

	static void Put(StringBuilder text, Statement statement)
	{
		switch (statement)
		{
			case Statement.Select(var with, var of, var by, var shape, var options):
				if (with.Length > 0)
				{
					text.Append("WITH ");
					Each(text, with);
					text.Append(' ');
				}

				Put(text, of, 0);

				if (by is not null)
				{
					text.Append(' ');
					Put(text, by);
				}

				foreach (var one in shape)
				{
					text.Append(' ');
					Put(text, one);
				}

				if (options.Length > 0)
				{
					text.Append(" OPTION (");
					Each(text, options);
					text.Append(')');
				}

				break;

			// `BULK INSERT` is an insert whose rows come from a file, and it is written
			// nothing like one; the rows say which of the two this is.
			case Statement.Insert(var target, _, Query.FromFile(var file)):
				text.Append("BULK INSERT ");
				Put(text, target!);
				text.Append(" FROM ");
				Put(text, file, 0);
				break;

			case Statement.Insert(var target, var columns, var rows):
				text.Append("INSERT INTO ");
				Put(text, target!);
				Names(text, columns);
				text.Append(' ');
				Put(text, rows, 0);
				break;

			case Statement.Update(var target, var set, var from, var where):
				text.Append("UPDATE ");
				Put(text, target!);
				text.Append(" SET ");
				Each(text, set);
				From(text, from);
				Where(text, where);
				break;

			case Statement.Delete(var target, var from, var where):
				text.Append("DELETE FROM ");
				Put(text, target!);
				From(text, from);
				Where(text, where);
				break;

			case Statement.Merge(var target, var using_, var on, var whens):
				text.Append("MERGE INTO ");
				Put(text, target!);
				text.Append(" USING ");
				Put(text, using_);
				text.Append(" ON ");
				Put(text, on, 0);

				foreach (var one in whens)
				{
					text.Append(' ');
					Put(text, one);
				}

				text.Append(';');
				break;

			case Statement.Compound(var body):
				text.Append("BEGIN ");
				Block(text, body);
				text.Append(" END");
				break;

			case Statement.If(var condition, var then, var otherwise):
				text.Append("IF ");
				Put(text, condition, 0);
				text.Append(' ');
				Put(text, then);

				if (otherwise is not null)
				{
					text.Append(" ELSE ");
					Put(text, otherwise);
				}

				break;

			case Statement.While(var condition, var body):
				text.Append("WHILE ");
				Put(text, condition, 0);
				text.Append(' ');
				Put(text, body);
				break;

			case Statement.TryCatch(var tried, var caught):
				text.Append("BEGIN TRY ");
				Block(text, tried);
				text.Append(" END TRY BEGIN CATCH ");
				Block(text, caught);
				text.Append(" END CATCH");
				break;

			case Statement.Declare(var variables):
				text.Append("DECLARE ");
				Each(text, variables);
				break;

			case Statement.Transaction(var kind, var name):
				text.Append(kind);

				if (name is not null)
					text.Append(' ').Append(name);

				break;

			case Statement.Execute(var into, var name, var arguments):
				text.Append("EXECUTE ");

				if (into is not null)
					text.Append(into).Append(" = ");

				text.Append(name);

				for (var i = 0; i < arguments.Length; i++)
				{
					text.Append(i == 0 ? " " : ", ");
					Put(text, arguments[i], 0);
				}

				break;

			case Statement.TableDefinition(var name, var elements):
				text.Append("CREATE TABLE ").Append(name).Append(" (");
				Each(text, elements);
				text.Append(')');
				break;

			case Statement.CreateTableAsSelect(var name, var body):
				text.Append("CREATE TABLE ").Append(name).Append(" AS ");
				Put(text, body);
				break;

			case Statement.AlterTable(var name, var action, var elements):
				text.Append("ALTER TABLE ").Append(name).Append(' ').Append(action);

				if (elements.Length > 0)
				{
					text.Append(' ');
					Each(text, elements);
				}

				break;

			case Statement.CreateProcedure(var name, var parameters, var body):
				text.Append("CREATE PROCEDURE ").Append(name);
				Parameters(text, parameters);
				text.Append(" AS ");
				Block(text, body);
				break;

			case Statement.CreateFunction(var name, var parameters, var returns, var body):
				text.Append("CREATE FUNCTION ").Append(name);
				Parameters(text, parameters);
				text.Append(" RETURNS ").Append(returns).Append(' ');

				// An inline table function is one `RETURN` and a query; anything else is a
				// body. The tree tells them apart by what a `TABLE` function's body holds.
				if (returns == "TABLE" && body is [Statement.Select select])
				{
					text.Append("AS RETURN ");
					Put(text, select);
				}
				else
				{
					text.Append("AS ");
					Block(text, body);
				}

				break;

			case Statement.CreateTrigger(var name, var on, var events, var body):
				text.Append("CREATE TRIGGER ").Append(name).Append(" ON ").Append(on).Append(" AFTER ");
				text.Append(string.Join(", ", events));
				text.Append(" AS ");
				Block(text, body);
				break;

			case Statement.ViewDefinition(var name, var columns, var body):
				text.Append("CREATE VIEW ").Append(name);
				Names(text, columns);
				text.Append(" AS ");
				Put(text, body);
				break;

			case Statement.CreateIndex(var name, var on, var columns):
				text.Append("CREATE INDEX ").Append(name).Append(" ON ").Append(on);
				Names(text, columns);
				break;

			case Statement.AlterIndex(var name, var on, var action):
				text.Append("ALTER INDEX ").Append(name).Append(" ON ").Append(on).Append(' ').Append(action);
				break;

			case Statement.UpdateStatistics(var on):
				text.Append("UPDATE STATISTICS ").Append(on);
				break;

			case Statement.Print(var value):
				text.Append("PRINT ");
				Put(text, value, 0);
				break;

			case Statement.Return(var value):
				text.Append("RETURN");

				if (value is not null)
				{
					text.Append(' ');
					Put(text, value, 0);
				}

				break;

			case Statement.Throw(var arguments):
				text.Append("THROW");
				Arguments(text, arguments, " ");
				break;

			case Statement.GoTo(var label):
				text.Append("GOTO ");
				Put(text, label, 0);
				break;

			case Statement.Break:
				text.Append("BREAK");
				break;

			case Statement.Continue:
				text.Append("CONTINUE");
				break;

			case Statement.Checkpoint(var value):
				text.Append("CHECKPOINT");

				if (value is not null)
				{
					text.Append(' ');
					Put(text, value, 0);
				}

				break;

			case Statement.Use(var name):
				text.Append("USE ");
				Put(text, name, 0);
				break;

			case Statement.RaiseError(var arguments):
				text.Append("RAISERROR");
				Arguments(text, arguments, " ");
				break;

			case Statement.WaitFor(var value):
				text.Append("WAITFOR DELAY ");
				Put(text, value, 0);
				break;

			case Statement.SetTransactionIsolationLevel(var level):
				text.Append("SET TRANSACTION ISOLATION LEVEL ").Append(level);
				break;

			case Statement.SetIdentityInsert(var table, var on):
				text.Append("SET IDENTITY_INSERT ").Append(table).Append(on ? " ON" : " OFF");
				break;

			case Statement.SetOption(var options, var on):
				text.Append("SET ").Append(string.Join(", ", options)).Append(on ? " ON" : " OFF");
				break;

			case Statement.SetCommand(var option, var value):
				text.Append("SET ").Append(option);

				if (value is not null)
				{
					text.Append(' ');
					Put(text, value, 0);
				}

				break;

			case Statement.SetVariable(var name, var value):
				text.Append("SET ").Append(name).Append(" = ");
				Put(text, value, 0);
				break;

			case Statement.Grant(var privileges, var principals):
				Permission(text, "GRANT", privileges, "TO", principals);
				break;

			case Statement.Deny(var privileges, var principals):
				Permission(text, "DENY", privileges, "TO", principals);
				break;

			case Statement.Revoke(var privileges, var principals):
				Permission(text, "REVOKE", privileges, "FROM", principals);
				break;

			case Statement.CreateDatabase(var name, _):
				text.Append("CREATE DATABASE ").Append(name);
				break;

			case Statement.AlterDatabaseSet(var name, var settings):
				text.Append("ALTER DATABASE ").Append(name).Append(" SET ");
				Each(text, settings);
				break;

			case Statement.AlterDatabaseScopedConfiguration(_, var settings):
				text.Append("ALTER DATABASE SCOPED CONFIGURATION SET ");
				Each(text, settings);
				break;

			// Everything else is a word or two and a name, which is the whole of what the
			// tree keeps for it. The record's own name spells the words — it was made from
			// them — so one rule serves a hundred and forty statements rather than a
			// hundred and forty arms saying the same thing.
			default:
				Spelling(text, statement);
				break;
		}
	}

	static void Block(StringBuilder text, Statement[] body)
	{
		for (var i = 0; i < body.Length; i++)
		{
			if (i > 0)
				text.Append(' ');

			Put(text, body[i]);
			text.Append(';');
		}
	}

	static void Permission(
		StringBuilder text, string word, string[] privileges, string way, string[] principals) =>
		text.Append(word).Append(' ').Append(string.Join(", ", privileges))
			.Append(' ').Append(way).Append(' ').Append(string.Join(", ", principals));

	static void Parameters(StringBuilder text, Clause[] parameters)
	{
		text.Append(" (");
		Each(text, parameters);
		text.Append(')');
	}

	static void From(StringBuilder text, TableReference[] from)
	{
		if (from.Length == 0)
			return;

		text.Append(" FROM ");

		for (var i = 0; i < from.Length; i++)
		{
			if (i > 0)
				text.Append(", ");

			Put(text, from[i]);
		}
	}

	static void Where(StringBuilder text, Expression? where)
	{
		if (where is null)
			return;

		text.Append(" WHERE ");
		Put(text, where, 0);
	}

	// ── The statements that are a word and a name ───────────────────────────────

	/// <summary>The words a statement's record was named after, and its name after them.</summary>
	static void Spelling(StringBuilder text, Statement statement)
	{
		var record = statement.GetType().Name;

		if (record.StartsWith("AlterDatabase", StringComparison.Ordinal) &&
			statement.GetType().GetProperty("Name")?.GetValue(statement) is string database)
		{
			text.Append("ALTER DATABASE ").Append(database).Append(' ')
				.Append(Words(record["AlterDatabase".Length..]));

			return;
		}

		text.Append(Words(record));

		switch (statement)
		{
			case Statement.FullTextIndexDefinition(var on):
				text.Append(" ON ").Append(on);
				break;

			case Statement.AlterFullTextIndex(var on):
				text.Append(" ON ").Append(on);
				break;

			default:
				// A `DROP` names several; everything else names one, and a few name none.
				if (statement.GetType().GetProperty("Names") is { } many &&
					many.GetValue(statement) is Expression[] names)
				{
					for (var i = 0; i < names.Length; i++)
					{
						text.Append(i == 0 ? " " : ", ");
						Put(text, names[i], 0);
					}
				}
				else if (statement.GetType().GetProperty("Name")?.GetValue(statement) is string one &&
					one.Length > 0)
				{
					text.Append(' ').Append(one);
				}

				break;
		}
	}

	static readonly Dictionary<string, string> Spelled = new(StringComparer.Ordinal)
	{
		["BackupTransactionLog"]   = "BACKUP LOG",
		["RestoreFileListOnly"]    = "RESTORE FILELISTONLY",
		["RestoreHeaderOnly"]      = "RESTORE HEADERONLY",
		["RestoreLabelOnly"]       = "RESTORE LABELONLY",
		["RestoreRewindOnly"]      = "RESTORE REWINDONLY",
		["RestoreVerifyOnly"]      = "RESTORE VERIFYONLY",
		["AuditSpecificationDefinition"] = "CREATE SERVER AUDIT SPECIFICATION",
		["StatisticsDefinition"]   = "CREATE STATISTICS",
	};

	/// <summary>
	/// A record's name as the words it was made from: <c>DropXmlSchemaCollection</c> is
	/// <c>DROP XML SCHEMA COLLECTION</c>, and a <c>…Definition</c> is a <c>CREATE</c>.
	/// </summary>
	static string Words(string record)
	{
		if (Spelled.TryGetValue(record, out var said))
			return said;

		var create = record.EndsWith("Definition", StringComparison.Ordinal);

		if (create)
			record = record[..^"Definition".Length];

		// Three names the reference writes as one word and the record as two, which is the
		// one place the split is not the inverse of the naming.
		record = record
			.Replace("FullText",  "Fulltext",  StringComparison.Ordinal)
			.Replace("FileGroup", "Filegroup", StringComparison.Ordinal)
			.Replace("LogFile",   "Logfile",   StringComparison.Ordinal);

		var made = new StringBuilder(create ? "CREATE" : "");

		foreach (var c in record)
		{
			if (char.IsUpper(c) && made.Length > 0)
				made.Append(' ');

			made.Append(char.ToUpperInvariant(c));
		}

		return made.ToString();
	}

	// ── Queries ─────────────────────────────────────────────────────────────────

	static void Put(StringBuilder text, Query query, int least)
	{
		var binds = query switch
		{
			Query.Union or Query.Except => 1,
			Query.Intersect             => 2,
			_                           => 3,
		};

		var wrap = binds < least;

		if (wrap)
			text.Append('(');

		switch (query)
		{
			case Query.Specification(
				var distinct, var top, var columns, var into, var from, var where, var group, var having):
				text.Append("SELECT ");

				if (distinct)
					text.Append("DISTINCT ");

				if (top is not null)
				{
					Put(text, top);
					text.Append(' ');
				}

				Each(text, columns);

				if (into is not null)
					text.Append(" INTO ").Append(into);

				From(text, from);
				Where(text, where);

				if (group is not null)
				{
					text.Append(' ');
					Put(text, group);
				}

				if (having is not null)
				{
					text.Append(" HAVING ");
					Put(text, having, 0);
				}

				break;

			case Query.TableValueConstructor(var rows):
				text.Append("VALUES ");
				Rows(text, rows);
				break;

			case Query.ExplicitTable(var name):
				text.Append("TABLE ").Append(name);
				break;

			case Query.Union(var left, var right, var all):
				Set(text, left, right, "UNION", all, binds);
				break;

			case Query.Except(var left, var right, var all):
				Set(text, left, right, "EXCEPT", all, binds);
				break;

			case Query.Intersect(var left, var right, var all):
				Set(text, left, right, "INTERSECT", all, binds);
				break;

			case Query.DefaultValues:
				text.Append("DEFAULT VALUES");
				break;

			case Query.FromFile(var file):
				Put(text, file, 0);
				break;

			case Query.FromExecute(var execute):
				Put(text, execute);
				break;
		}

		if (wrap)
			text.Append(')');
	}

	/// <summary>
	/// The rows of a <c>VALUES</c>, each in its own brackets.
	/// </summary>
	/// <remarks>
	/// A row of one value is written without them in §7.2 — <c>VALUES 1, 2</c> is two rows of
	/// one column — and the tree keeps the value rather than a row around it. T-SQL wants the
	/// brackets, so they are written here, which is the dialect this prints.
	/// </remarks>
	static void Rows(StringBuilder text, Expression[] rows)
	{
		for (var i = 0; i < rows.Length; i++)
		{
			if (i > 0)
				text.Append(", ");

			if (rows[i] is Expression.RowValueConstructor)
			{
				Put(text, rows[i], 0);
			}
			else
			{
				text.Append('(');
				Put(text, rows[i], 0);
				text.Append(')');
			}
		}
	}

	static void Set(StringBuilder text, Query left, Query right, string word, bool all, int binds)
	{
		Put(text, left, binds);
		text.Append(' ').Append(word).Append(all ? " ALL " : " ");
		Put(text, right, binds + 1);
	}

	// ── Table references ────────────────────────────────────────────────────────

	static void Put(StringBuilder text, TableReference source)
	{
		switch (source)
		{
			case TableReference.Named(var table, var name, var columns, var hints):
				text.Append(table);
				Alias(text, name, columns);

				if (hints.Length > 0)
				{
					text.Append(" WITH (");
					Each(text, hints);
					text.Append(')');
				}

				break;

			case TableReference.Derived(var query, var name, var columns):
				text.Append('(');
				Put(text, query, 0);
				text.Append(')');
				Alias(text, name, columns);
				break;

			case TableReference.FunctionCall(var call, var name, var columns):
				Put(text, call, 0);
				Alias(text, name, columns);
				break;

			case TableReference.Joined(var kind, var natural, var left, var right, var on, var over):
				Put(text, left);
				text.Append(' ');

				if (natural)
					text.Append("NATURAL ");

				text.Append(kind switch
				{
					SqlJoin.Cross      => "CROSS JOIN ",
					SqlJoin.Left       => "LEFT JOIN ",
					SqlJoin.Right      => "RIGHT JOIN ",
					SqlJoin.Full       => "FULL JOIN ",
					SqlJoin.Union      => "UNION JOIN ",
					SqlJoin.CrossApply => "CROSS APPLY ",
					SqlJoin.OuterApply => "OUTER APPLY ",
					_                  => "INNER JOIN ",
				});

				Put(text, right);

				if (on is not null)
				{
					text.Append(" ON ");
					Put(text, on, 0);
				}
				else if (over is not null)
				{
					text.Append(" USING (").Append(string.Join(", ", over)).Append(')');
				}

				break;
		}
	}

	static void Alias(StringBuilder text, string? name, string[]? columns)
	{
		if (name is not null)
			text.Append(" AS ").Append(name);

		Names(text, columns);
	}

	static void Names(StringBuilder text, string[]? columns)
	{
		if (columns is not null)
			text.Append(" (").Append(string.Join(", ", columns)).Append(')');
	}

	// ── Clauses ─────────────────────────────────────────────────────────────────

	static void Put(StringBuilder text, Clause clause)
	{
		switch (clause)
		{
			case Clause.DerivedColumn(var value, var name):
				Put(text, value, 0);

				if (name is not null)
					text.Append(" AS ").Append(name);

				break;

			case Clause.QualifiedAsterisk(var qualifier):
				if (qualifier is not null)
					text.Append(qualifier).Append('.');

				text.Append('*');
				break;

			case Clause.SortSpecification(var value, var order):
				Put(text, value, 0);

				if (order != SqlOrder.Unspecified)
					text.Append(order == SqlOrder.Descending ? " DESC" : " ASC");

				break;

			case Clause.OrderBy(var by, var offset, var fetch):
				if (by.Length > 0)
				{
					text.Append("ORDER BY ");
					Each(text, by);
				}

				if (offset is not null)
				{
					text.Append(" OFFSET ");
					Put(text, offset, 0);
					text.Append(" ROWS");
				}

				if (fetch is not null)
				{
					text.Append(" FETCH NEXT ");
					Put(text, fetch, 0);
					text.Append(" ROWS ONLY");
				}

				break;

			case Clause.Top(var value, var percent, var ties):
				text.Append("TOP (");
				Put(text, value, 0);
				text.Append(')');

				if (percent)
					text.Append(" PERCENT");

				if (ties is not null)
					text.Append(" WITH ").Append(ties);

				break;

			case Clause.GroupBy(var all, var by, var with):
				text.Append("GROUP BY ");

				if (all)
					text.Append("ALL ");

				List(text, by);

				if (with is not null)
					text.Append(" WITH ").Append(with);

				break;

			case Clause.CommonTableExpression(var name, var columns, var query):
				text.Append(name);
				Names(text, columns);
				text.Append(" AS (");
				Put(text, query, 0);
				text.Append(')');
				break;

			case Clause.For(var kind, var options):
				text.Append("FOR ").Append(kind);

				if (options.Length > 0)
					text.Append(' ').Append(string.Join(", ", options));

				break;

			case Clause.Window(var window, var partition, var by, var frame):
				// A window that is only a name was written without brackets; anything else
				// has them, and an empty `OVER ()` is written with nothing inside.
				if (window is not null && partition.Length == 0 && by is null && frame is null)
				{
					text.Append("OVER ").Append(window);

					break;
				}

				text.Append("OVER (");

				if (window is not null)
					text.Append(window).Append(' ');

				if (partition.Length > 0)
				{
					text.Append("PARTITION BY ");
					List(text, partition);

					if (by is not null || frame is not null)
						text.Append(' ');
				}

				if (by is not null)
				{
					Put(text, by);

					if (frame is not null)
						text.Append(' ');
				}

				if (frame is not null)
					text.Append(frame);

				text.Append(')');
				break;

			case Clause.Hint(var hint):
				text.Append(hint);
				break;

			case Clause.WithOption(var option):
				text.Append(option);
				break;

			case Clause.VariableAssignment(var variable, var by, var value):
				text.Append(variable).Append(' ').Append(by).Append(' ');
				Put(text, value, 0);
				break;

			case Clause.When(var test, var result):
				text.Append("WHEN ");
				Put(text, test, 0);
				text.Append(" THEN ");
				Put(text, result, 0);
				break;

			case Clause.Set(var target, var by, var value):
				text.Append(target).Append(' ').Append(by ?? "=").Append(' ');
				Put(text, value, 0);
				break;

			case Clause.MergeWhen(var matched, var by, var condition, var action):
				text.Append(matched ? "WHEN MATCHED" : "WHEN NOT MATCHED BY " + by);

				if (condition is not null)
				{
					text.Append(" AND ");
					Put(text, condition, 0);
				}

				text.Append(" THEN ");
				Arm(text, action);
				break;

			case Clause.VariableDeclaration(var name, var type, var value):
				Declared(text, name, type, value);
				break;

			case Clause.ParameterDeclaration(var name, var type, var value):
				Declared(text, name, type, value);
				break;

			case Clause.ColumnDefinition(var name, var type, var computed, var constraints):
				text.Append(name);

				if (computed is not null)
				{
					text.Append(" AS ");
					Put(text, computed, 0);
				}
				else if (type is not null)
				{
					text.Append(' ').Append(type);
				}

				foreach (var one in constraints)
				{
					text.Append(' ');
					Put(text, one);
				}

				break;

			case Clause.ConstraintDefinition(var name, var kind, var columns, var check):
				if (name is not null)
					text.Append("CONSTRAINT ").Append(name).Append(' ');

				text.Append(kind);
				Names(text, columns);

				if (check is not null)
				{
					text.Append(" (");
					Put(text, check, 0);
					text.Append(')');
				}

				break;

			case Clause.DatabaseOption(var name, var value):
				text.Append(name);

				if (value is not null)
				{
					text.Append(" = ");
					Put(text, value, 0);
				}

				break;
		}
	}

	static void Declared(StringBuilder text, string name, string? type, Expression? value)
	{
		text.Append(name);

		if (type is not null)
			text.Append(' ').Append(type);

		if (value is not null)
		{
			text.Append(" = ");
			Put(text, value, 0);
		}
	}

	/// <summary>
	/// What a merge arm does, which is the three statements written without their target:
	/// the target is the merge's own and the syntax does not repeat it.
	/// </summary>
	static void Arm(StringBuilder text, Statement action)
	{
		switch (action)
		{
			case Statement.Update(null, var set, _, _):
				text.Append("UPDATE SET ");
				Each(text, set);
				break;

			case Statement.Delete(null, _, _):
				text.Append("DELETE");
				break;

			case Statement.Insert(null, var columns, var rows):
				text.Append("INSERT");
				Names(text, columns);
				text.Append(' ');
				Put(text, rows, 0);
				break;

			default:
				Put(text, action);
				break;
		}
	}

	static void Each(StringBuilder text, Clause[] clauses)
	{
		for (var i = 0; i < clauses.Length; i++)
		{
			if (i > 0)
				text.Append(", ");

			Put(text, clauses[i]);
		}
	}

	// ── Expressions ─────────────────────────────────────────────────────────────

	/// <summary>How tightly a node binds, so that a bracket is written only where it is due.</summary>
	static int Binds(Expression expression) => expression switch
	{
		Expression.Or  => 1,
		Expression.And => 2,
		Expression.Not => 3,

		Expression.Comparison or Expression.Quantified or Expression.Between or Expression.In or
		Expression.Like or Expression.IsNull or Expression.Exists or Expression.Unique or
		Expression.Match or Expression.Overlaps or Expression.IsDistinctFrom or
		Expression.IsTruth => 4,

		Expression.Add or Expression.Subtract or Expression.Concatenate => 5,
		Expression.Multiply or Expression.Divide                        => 6,
		Expression.Negate or Expression.Plus                            => 7,

		_ => 8,
	};

	static void Put(StringBuilder text, Expression expression, int least)
	{
		var binds = Binds(expression);
		var wrap  = binds < least;

		if (wrap)
			text.Append('(');

		switch (expression)
		{
			case Expression.Or(var left, var right):    Binary(text, left, "OR", right, binds);  break;
			case Expression.And(var left, var right):   Binary(text, left, "AND", right, binds); break;
			case Expression.Add(var left, var right):   Binary(text, left, "+", right, binds);   break;
			case Expression.Subtract(var l, var r):     Binary(text, l, "-", r, binds);          break;
			case Expression.Concatenate(var l, var r):  Binary(text, l, "+", r, binds);          break;
			case Expression.Multiply(var l, var r):     Binary(text, l, "*", r, binds);          break;
			case Expression.Divide(var l, var r):       Binary(text, l, "/", r, binds);          break;

			case Expression.Not(var operand):
				text.Append("NOT ");
				Put(text, operand, 4);
				break;

			case Expression.Negate(var operand):
				text.Append('-');
				Put(text, operand, 8);
				break;

			case Expression.Plus(var operand):
				text.Append('+');
				Put(text, operand, 8);
				break;

			case Expression.IsTruth(var operand, var negated, var truth):
				Put(text, operand, 4);
				text.Append(" IS ").Append(negated ? "NOT " : "").Append(truth.ToString().ToUpperInvariant());
				break;

			case Expression.Comparison(var left, var op, var right):
				Put(text, left, 5);
				text.Append(' ').Append(Sign(op)).Append(' ');
				Put(text, right, 5);
				break;

			case Expression.Quantified(var left, var op, var quantifier, var query):
				Put(text, left, 5);
				text.Append(' ').Append(Sign(op)).Append(' ').Append(quantifier).Append(" (");
				Put(text, query, 0);
				text.Append(')');
				break;

			case Expression.Between(var value, var negated, var low, var high):
				Put(text, value, 5);
				text.Append(negated ? " NOT BETWEEN " : " BETWEEN ");
				Put(text, low, 5);
				text.Append(" AND ");
				Put(text, high, 5);
				break;

			case Expression.In(var value, var negated, var source):
				Put(text, value, 5);
				text.Append(negated ? " NOT IN " : " IN ");
				Put(text, source, 8);
				break;

			case Expression.Like(var value, var negated, var pattern, var escape):
				Put(text, value, 5);
				text.Append(negated ? " NOT LIKE " : " LIKE ");
				Put(text, pattern, 5);

				if (escape is not null)
				{
					text.Append(" ESCAPE ");
					Put(text, escape, 5);
				}

				break;

			case Expression.IsNull(var value, var negated):
				Put(text, value, 5);
				text.Append(negated ? " IS NOT NULL" : " IS NULL");
				break;

			case Expression.IsDistinctFrom(var left, var negated, var right):
				Put(text, left, 5);
				text.Append(negated ? " IS NOT DISTINCT FROM " : " IS DISTINCT FROM ");
				Put(text, right, 5);
				break;

			case Expression.Exists(var query):
				text.Append("EXISTS (");
				Put(text, query, 0);
				text.Append(')');
				break;

			case Expression.Unique(var query):
				text.Append("UNIQUE (");
				Put(text, query, 0);
				text.Append(')');
				break;

			case Expression.Match(var value, var qualifier, var query):
				Put(text, value, 5);
				text.Append(" MATCH ");

				if (qualifier is not null)
					text.Append(qualifier).Append(' ');

				text.Append('(');
				Put(text, query, 0);
				text.Append(')');
				break;

			case Expression.Overlaps(var left, var right):
				Put(text, left, 5);
				text.Append(" OVERLAPS ");
				Put(text, right, 5);
				break;

			case Expression.Case(var operand, var whens, var otherwise):
				text.Append("CASE");

				if (operand is not null)
				{
					text.Append(' ');
					Put(text, operand, 0);
				}

				foreach (var one in whens)
				{
					text.Append(' ');
					Put(text, one);
				}

				if (otherwise is not null)
				{
					text.Append(" ELSE ");
					Put(text, otherwise, 0);
				}

				text.Append(" END");
				break;

			case Expression.ColumnReference(var name):
				text.Append(name);
				break;

			case Expression.Literal(_, var literal):
				text.Append(literal);
				break;

			case Expression.RowValueConstructor(var values):
				Arguments(text, values, "");
				break;

			case Expression.WindowFunction(var function, var within, var over):
				Put(text, function, 8);

				if (within is not null)
					text.Append(' ').Append(within);

				if (over is not null)
				{
					text.Append(' ');
					Put(text, over);
				}

				break;

			case Expression.Member(var of, var by, var name, var arguments):
				Put(text, of, 8);
				text.Append(by).Append(name);

				if (arguments is not null)
				{
					text.Append('(');
					List(text, arguments);
					text.Append(')');
				}

				break;

			case Expression.Collated(var value, var collation):
				Put(text, value, 8);
				text.Append(" COLLATE ").Append(collation);
				break;

			case Expression.Prefixed(var word, var value):
				text.Append(word).Append(' ');
				Put(text, value, 0);
				break;

			case Expression.NamedArgument(var name, var value):
				text.Append(name).Append(" = ");
				Put(text, value, 0);
				break;

			case Expression.Subquery(var query):
				text.Append('(');
				Put(text, query, 0);
				text.Append(')');
				break;

			case Expression.RoutineInvocation call:
				Call(text, call);
				break;
		}

		if (wrap)
			text.Append(')');
	}

	static void Binary(StringBuilder text, Expression left, string word, Expression right, int binds)
	{
		Put(text, left, binds);
		text.Append(' ').Append(word).Append(' ');
		Put(text, right, binds + 1);
	}

	static string Sign(SqlComparison operation) => operation switch
	{
		SqlComparison.Equal          => "=",
		SqlComparison.NotEqual       => "<>",
		SqlComparison.Less           => "<",
		SqlComparison.LessOrEqual    => "<=",
		SqlComparison.Greater        => ">",
		_                            => ">=",
	};

	// ── The calls that are not an argument list ─────────────────────────────────
	//
	// A dozen functions have a syntax of their own — the standard writes them out in §6.16
	// and T-SQL adds its own — and the tree keeps what they carry in `Word`. What each of
	// them means there is what this switch says.

	static void Call(StringBuilder text, Expression.RoutineInvocation call)
	{
		var (name, arguments, word) = call;

		switch (name.ToUpperInvariant())
		{
			case "CAST":
			case "TRY_CAST":
				text.Append(name).Append('(');
				Put(text, arguments[0], 0);
				text.Append(" AS ").Append(word).Append(')');
				return;

			case "CONVERT":
			case "TRY_CONVERT":
				// T-SQL's, which names the type first. The standard's `CONVERT (v USING cs)`
				// is the same record and comes out in this spelling — see the remarks above.
				text.Append(name).Append('(').Append(word);
				Arguments(text, arguments, ", ", opened: true);
				text.Append(')');
				return;

			case "PARSE":
			case "TRY_PARSE":
				text.Append(name).Append('(');
				Put(text, arguments[0], 0);
				text.Append(" AS ").Append(word);

				if (arguments.Length > 1)
				{
					text.Append(" USING ");
					Put(text, arguments[1], 0);
				}

				text.Append(')');
				return;

			case "TRANSLATE" when word is not null:
				text.Append("TRANSLATE(");
				Put(text, arguments[0], 0);
				text.Append(" USING ").Append(word).Append(')');
				return;

			case "EXTRACT":
				text.Append("EXTRACT(").Append(word).Append(" FROM ");
				Put(text, arguments[0], 0);
				text.Append(')');
				return;

			case "POSITION":
				text.Append("POSITION(");
				Put(text, arguments[0], 0);
				text.Append(" IN ");
				Put(text, arguments[1], 0);
				text.Append(')');
				return;

			case "TRIM":
				text.Append("TRIM(");

				if (word is not null)
					text.Append(word).Append(' ');

				if (arguments.Length == 2)
				{
					Put(text, arguments[0], 0);
					text.Append(" FROM ");
					Put(text, arguments[1], 0);
				}
				else
				{
					Put(text, arguments[0], 0);
				}

				text.Append(')');
				return;

			case "AT TIME ZONE":
				Put(text, arguments[0], 8);
				text.Append(" AT TIME ZONE ");
				Put(text, arguments[1], 8);
				return;

			case "NEXT VALUE FOR":
				text.Append("NEXT VALUE FOR ").Append(word);
				return;

			case "CURRENT_DATE":
			case "CURRENT_TIME":
			case "CURRENT_TIMESTAMP":
				text.Append(name);

				if (word is not null)
					text.Append('(').Append(word).Append(')');

				return;

			case "JSON_OBJECT":
			case "JSON_OBJECTAGG":
				text.Append(name).Append('(');

				for (var i = 0; i < arguments.Length; i++)
				{
					if (i > 0)
						text.Append(", ");

					// A key and its value, which the tree keeps as a row of two.
					if (arguments[i] is Expression.RowValueConstructor([var key, var value]))
					{
						Put(text, key, 0);
						text.Append(':');
						Put(text, value, 0);
					}
					else
					{
						Put(text, arguments[i], 0);
					}
				}

				text.Append(')');
				return;
		}

		text.Append(name).Append('(');

		// `*` for a count, and a set quantifier for the aggregates: both stand in front of
		// the arguments rather than among them.
		if (word is not null)
			text.Append(word).Append(arguments.Length > 0 ? " " : "");

		for (var i = 0; i < arguments.Length; i++)
		{
			if (i > 0)
				text.Append(", ");

			Put(text, arguments[i], 0);
		}

		text.Append(')');
	}

	/// <summary>A comma list with no brackets around it: the rows of a `VALUES`, a `GROUP BY`.</summary>
	static void List(StringBuilder text, Expression[] values)
	{
		for (var i = 0; i < values.Length; i++)
		{
			if (i > 0)
				text.Append(", ");

			Put(text, values[i], 0);
		}
	}

	static void Arguments(
		StringBuilder text, Expression[] arguments, string before, bool opened = false)
	{
		if (!opened)
		{
			text.Append(before);
			text.Append('(');
		}

		for (var i = 0; i < arguments.Length; i++)
		{
			if (i > 0 || opened)
				text.Append(", ");

			Put(text, arguments[i], 0);
		}

		if (!opened)
			text.Append(')');
	}
}
