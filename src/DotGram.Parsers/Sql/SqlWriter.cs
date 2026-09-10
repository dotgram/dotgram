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
/// <b>It prints what the tree holds, and nothing else.</b> That used to leave a great deal
/// out: `TOP`, `OVER`, the hints, the windows, `OUTPUT`, a named query's `WITH` and the DDL
/// option lists were dropped at parse time and so could not come back here. They are in the
/// tree now, and this is why — a decoration nothing prints is a decoration nothing checks,
/// so the round trip is what made keeping them worth the fields. What is still dropped is
/// the short list in `docs/ast.md`. Where a statement keeps only a name, what comes out is
/// the statement without its options, which SQL Server may well refuse; the harness counts
/// that as a loss, which is what it is.
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
			case Statement.Insert(var target, _, Query.FromFile(var file, var bulk), _, _, _, _):
				text.Append("BULK INSERT ");
				Put(text, target!);
				text.Append(" FROM ");
				Put(text, file, 0);
				Optioned(text, bulk);
				break;

			case Statement.Insert(var target, var columns, var rows, var with, var top, var output, var into) insert:
				With(text, with);
				text.Append("INSERT ");
				Top(text, top);

				if (into)
					text.Append("INTO ");

				Put(text, target!);
				Names(text, columns);
				Output(text, output);
				text.Append(' ');
				Put(text, rows, 0);
				Hinted(text, insert.Options);
				break;

			case Statement.Update(var target, var set, var from, var where, var with, var top, var output, var options):
				With(text, with);
				text.Append("UPDATE ");
				Top(text, top);
				Put(text, target!);
				text.Append(" SET ");
				Each(text, set);
				Output(text, output);
				From(text, from);
				Where(text, where);
				Hinted(text, options);
				break;

			case Statement.Delete(var target, var from, var where, var with, var top, var output, var options):
				With(text, with);
				text.Append("DELETE ");
				Top(text, top);
				text.Append("FROM ");
				Put(text, target!);
				Output(text, output);
				From(text, from);
				Where(text, where);
				Hinted(text, options);
				break;

			case Statement.Merge(var target, var using_, var on, var whens, var with, var top, var alias, var output, var options, var into):
				With(text, with);
				text.Append("MERGE ");
				Top(text, top);

				if (into)
					text.Append("INTO ");

				Put(text, target!);

				if (alias is not null)
					text.Append(" AS ").Append(alias);

				text.Append(" USING ");
				Put(text, using_);
				text.Append(" ON ");
				Put(text, on, 0);

				foreach (var one in whens)
				{
					text.Append(' ');
					Put(text, one);
				}

				Output(text, output);
				Hinted(text, options);
				text.Append(';');
				break;

			case Statement.Compound(var body, var atomic):
				text.Append("BEGIN ");

				if (atomic is not null)
				{
					text.Append("ATOMIC WITH (");
					Each(text, atomic);
					text.Append(") ");
				}

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

			case Statement.Transaction(var kind, var name, var word, var mark):
				text.Append(kind);

				if (word is not null)
					text.Append(' ').Append(word);

				if (name is not null)
					text.Append(' ').Append(name);

				if (mark is not null)
					text.Append(' ').Append(mark);

				break;

			case Statement.Execute(var into, var name, var arguments, var at, var with):
				text.Append("EXECUTE ");

				if (into is not null)
					text.Append(into).Append(" = ");

				text.Append(name);

				for (var i = 0; i < arguments.Length; i++)
				{
					text.Append(i == 0 ? " " : ", ");
					Put(text, arguments[i], 0);
				}

				// `EXECUTE ( … )` is the one spelling whose name is a bracket, and the
				// bracket has to be closed.
				if (name == "(")
					text.Append(" )");

				if (at is not null)
					text.Append(" AT ").Append(at);

				if (with is not null)
					text.Append(' ').Append(with);

				break;

			case Statement.TableDefinition(var name, var kind, var elements, var placements, var options, var external):
				text.Append(external ? "CREATE EXTERNAL TABLE " : "CREATE TABLE ").Append(name);

				// `AS FILETABLE` stands before the columns, of which it has none; `AS NODE` and
				// `AS EDGE` stand after them. Two syntaxes, and which one is which the word says.
				var before = kind is not null && kind.Equals("FILETABLE", StringComparison.OrdinalIgnoreCase);

				if (before)
					text.Append(" AS ").Append(kind);

				if (elements.Length > 0 || kind is null)
				{
					text.Append(" (");
					Each(text, elements);
					text.Append(')');
				}

				if (kind is not null && !before)
					text.Append(" AS ").Append(kind);

				Placed(text, placements);
				Optioned(text, options);
				break;

			case Statement.CreateTableAsSelect(var name, var columns, var options, var body, var external):
				text.Append(external ? "CREATE EXTERNAL TABLE " : "CREATE TABLE ").Append(name);
				Names(text, columns);
				Optioned(text, options);
				text.Append(" AS ");
				Put(text, body);
				break;

			case Statement.AlterTable(var name, var action, var elements, var options, var tail):
				text.Append("ALTER TABLE ").Append(name).Append(' ').Append(action);

				if (elements.Length > 0)
				{
					text.Append(' ');
					Each(text, elements);
				}

				if (tail is not null)
					text.Append(' ').Append(tail);

				Optioned(text, options);
				break;

			case Statement.CreateProcedure(var name, var parameters, var body, var options, var replication, var external, var number, var verb):
				text.Append(verb).Append(" PROCEDURE ").Append(name);

				if (number is not null)
					text.Append(';').Append(number);

				// Without brackets where there is nothing to bracket: `p ()` is not a
				// procedure header to SQL Server.
				if (parameters.Length > 0)
					Parameters(text, parameters);

				if (options is not null)
				{
					text.Append(" WITH ");
					Each(text, options);
				}

				if (replication)
					text.Append(" FOR REPLICATION");

				text.Append(" AS ");

				if (external is not null)
					text.Append("EXTERNAL NAME ").Append(external);
				else
					Block(text, body);

				break;

			case Statement.CreateFunction(var name, var parameters, var returns, var body, var options, var columns, var variable, var order, var external, var verb):
				text.Append(verb).Append(" FUNCTION ").Append(name);
				Parameters(text, parameters);
				text.Append(" RETURNS ");

				if (variable is not null)
					text.Append(variable).Append(' ');

				text.Append(returns);

				if (columns is not null)
				{
					text.Append(" (");
					Each(text, columns);
					text.Append(')');
				}

				if (order is not null)
					Columns(text, order, "ORDER");

				if (options is not null)
				{
					text.Append(" WITH ");
					Each(text, options);
				}

				text.Append(' ');

				// An inline table function is one `RETURN` and a query; anything else is a
				// body. The tree tells them apart by what a `TABLE` function's body holds.
				if (external is not null)
				{
					text.Append("AS EXTERNAL NAME ").Append(external);
				}
				else if (returns == "TABLE" && columns is null && body is [Statement.Select select])
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

			case Statement.CreateTrigger(var name, var on, var events, var body, var when, var options, var append, var replication, var external, var verb):
				text.Append(verb).Append(" TRIGGER ").Append(name).Append(" ON ").Append(on);

				if (options is not null)
				{
					text.Append(" WITH ");
					Each(text, options);
				}

				text.Append(' ').Append(when).Append(' ').Append(string.Join(", ", events));

				if (append)
					text.Append(" WITH APPEND");

				if (replication)
					text.Append(" NOT FOR REPLICATION");

				text.Append(" AS ");

				if (external is not null)
					text.Append("EXTERNAL NAME ").Append(external);
				else
					Block(text, body);

				break;

			case Statement.ViewDefinition(var name, var columns, var body, var options, var check, var materialized, var verb):
				text.Append(verb).Append(materialized ? " MATERIALIZED VIEW " : " VIEW ").Append(name);
				Names(text, columns);

				if (options is not null && materialized)
				{
					Optioned(text, options);
				}
				else if (options is not null)
				{
					text.Append(" WITH ");
					Each(text, options);
				}

				text.Append(" AS ");
				Put(text, body);

				if (check)
					text.Append(" WITH CHECK OPTION");

				break;

			case Statement.CreateIndex(var on, var index, var kind, var over):
				text.Append("CREATE ");

				if (kind is not null)
					text.Append(kind).Append(' ');

				// `USING XML INDEX p FOR PATH` and a selective index's `FOR (…)` stand
				// between the columns and the `WITH`, so the constraint prints them.
				Put(text, (Clause.ConstraintDefinition)index, on, over);
				break;

			case Statement.AlterIndex(var name, var on, var action, var partition, var options, var paths, var namespaces):
				text.Append("ALTER INDEX ").Append(name).Append(" ON ").Append(on);

				if (namespaces is not null)
					text.Append(" WITH ").Append(namespaces);

				text.Append(' ').Append(action);

				if (partition is not null)
				{
					text.Append(" PARTITION = ");
					Put(text, partition, 0);
				}

				if (paths is not null)
					text.Append(" (").Append(string.Join(", ", paths)).Append(')');

				if (action == "SET" && options is not null)
				{
					text.Append(" (");
					Each(text, options);
					text.Append(')');
				}
				else
				{
					Optioned(text, options);
				}

				break;

			case Statement.UpdateStatistics(var on) update:
				text.Append("UPDATE STATISTICS ").Append(on);

				if (update.Tail is { } how)
					text.Append(' ').Append(how);

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
				// `THROW 51000, 'x', 1` and not `THROW (…)`: the three are arguments of the
				// statement and not of a call, which is the one place a list stands bare.
				text.Append("THROW");

				for (var i = 0; i < arguments.Length; i++)
				{
					text.Append(i == 0 ? " " : ", ");
					Put(text, arguments[i], 0);
				}

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

			case Statement.RaiseError(var arguments, var logged):
				text.Append("RAISERROR");
				Arguments(text, arguments, " ");

				if (logged is not null)
					text.Append(' ').Append(logged);

				break;

			case Statement.WaitFor(var value, var kind):
				text.Append("WAITFOR ").Append(kind).Append(' ');
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

			case Statement.SetVariable(var name, var value, var by, var through):
				text.Append("SET ").Append(name);

				if (through is not null)
					text.Append(" = ").Append(through);

				text.Append(' ').Append(by ?? "=").Append(' ');
				Put(text, value, 0);
				break;

			case Statement.Grant(var privileges, var principals, var on, var option, var runAs):
				Permission(text, "GRANT", privileges, on, "TO", principals);

				if (option)
					text.Append(" WITH GRANT OPTION");

				if (runAs is not null)
					text.Append(" AS ").Append(runAs);

				break;

			case Statement.Deny(var privileges, var principals, var on, var cascade, var runAs):
				Permission(text, "DENY", privileges, on, "TO", principals);

				if (cascade)
					text.Append(" CASCADE");

				if (runAs is not null)
					text.Append(" AS ").Append(runAs);

				break;

			case Statement.Revoke(var privileges, var principals, var on, var optionFor, var from, var cascade, var runAs):
				text.Append("REVOKE ");

				if (optionFor)
					text.Append("GRANT OPTION FOR ");

				text.Append(string.Join(", ", privileges));

				if (on is not null)
					text.Append(" ON ").Append(on);

				text.Append(from ? " FROM " : " TO ").Append(string.Join(", ", principals));

				if (cascade)
					text.Append(" CASCADE");

				if (runAs is not null)
					text.Append(" AS ").Append(runAs);

				break;

			case Statement.CreateDatabase(var name, var files, var primary, var log, var containment, var collation, var tail, var options, var with):
				text.Append("CREATE DATABASE ").Append(name);

				// The Azure spelling: a bracket of options and nothing else.
				if (tail == "")
				{
					text.Append(" (");
					Each(text, options ?? Clause.None);
					text.Append(')');
					break;
				}

				if (containment is not null)
					text.Append(" CONTAINMENT = ").Append(containment);

				if (files.Length > 0)
				{
					text.Append(primary ? " ON PRIMARY " : " ON ");
					Each(text, files);
				}

				if (log is not null)
				{
					text.Append(" LOG ON ");
					Each(text, log);
				}

				if (collation is not null)
					text.Append(" COLLATE ").Append(collation);

				if (tail is not null)
					text.Append(' ').Append(tail);

				if (options is not null)
				{
					text.Append(" (");
					Each(text, options);
					text.Append(')');
				}

				if (with is not null)
				{
					text.Append(" WITH ");
					Each(text, with);
				}

				break;

			case Statement.EventSessionDefinition(var name, var verb, var on, var pieces, var options, var state):
				text.Append(verb).Append(" EVENT SESSION ").Append(name).Append(" ON ").Append(on);

				// A comma between two pieces of one kind — event after event, target after
				// target — and only a space where the kind changes, which is how the syntax
				// groups them.
				Clause.EventPiece? previous = null;

				foreach (var piece in pieces ?? Clause.None)
				{
					text.Append(previous is not null && piece is Clause.EventPiece next && next.Action == previous.Action ? ", " : " ");
					Put(text, piece);
					previous = piece as Clause.EventPiece;
				}

				Optioned(text, options);

				if (state is not null)
					text.Append(" STATE = ").Append(state);

				break;

			case Statement.EndpointDefinition(var name, var verb, var owner, var state, var stateOptions, var protocol, var protocolOptions, var payload, var payloadOptions):
				text.Append(verb).Append(" ENDPOINT ").Append(name);

				if (owner is not null)
					text.Append(" AUTHORIZATION ").Append(owner);

				if (state is not null)
				{
					text.Append(" STATE = ").Append(state);

					foreach (var one in stateOptions ?? Clause.None)
					{
						text.Append(", ");
						Put(text, one);
					}
				}
				else if (stateOptions is { Length: > 0 })
				{
					// `AFFINITY = NONE` with no state in front of it, or before one.
					text.Append(' ');
					Each(text, stateOptions);
				}

				if (protocol is not null)
				{
					text.Append(" AS ").Append(protocol).Append(" (");
					Each(text, protocolOptions ?? Clause.None);
					text.Append(')');
				}

				if (payload is not null)
				{
					text.Append(" FOR ").Append(payload).Append(" (");
					Each(text, payloadOptions ?? Clause.None);
					text.Append(')');
				}

				break;

			case Statement.AlterDatabaseSet(var name, var settings, var termination):
				text.Append("ALTER DATABASE ").Append(name).Append(" SET ");
				Each(text, settings);

				if (termination is not null)
					text.Append(' ').Append(termination);

				break;

			case Statement.AlterDatabaseModify(var name, var options, var with):
				text.Append("ALTER DATABASE ").Append(name).Append(" MODIFY");

				if (options is not null)
				{
					text.Append(" (");
					Each(text, options);
					text.Append(')');
				}

				if (with is not null)
				{
					text.Append(" WITH ");
					Each(text, with);
				}

				break;

			case Statement.AlterDatabaseScopedConfiguration(_, var action, var settings, var secondary, var argument):
				text.Append("ALTER DATABASE SCOPED CONFIGURATION ");

				if (secondary)
					text.Append("FOR SECONDARY ");

				text.Append(action);

				if (settings.Length > 0)
				{
					text.Append(' ');
					Each(text, settings);
				}

				if (argument is not null)
				{
					text.Append(' ');
					Put(text, argument, 0);
				}

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
		StringBuilder text, string word, string[] privileges, string? on, string way, string[] principals)
	{
		text.Append(word).Append(' ').Append(string.Join(", ", privileges));

		if (on is not null)
			text.Append(" ON ").Append(on);

		text.Append(' ').Append(way).Append(' ').Append(string.Join(", ", principals));
	}

	/// <summary>The common table expressions in front of a statement, or nothing.</summary>
	static void With(StringBuilder text, Clause[]? with)
	{
		if (with is not { Length: > 0 })
			return;

		text.Append("WITH ");
		Each(text, with);
		text.Append(' ');
	}

	/// <summary>A <c>TOP</c> and the space after it, or nothing.</summary>
	static void Top(StringBuilder text, Clause? top)
	{
		if (top is null)
			return;

		Put(text, top);
		text.Append(' ');
	}

	/// <summary>An <c>OUTPUT</c> clause after a space, or nothing.</summary>
	static void Output(StringBuilder text, Clause? output)
	{
		if (output is null)
			return;

		text.Append(' ');
		Put(text, output);
	}

	/// <summary>A statement's <c>OPTION (…)</c>, or nothing.</summary>
	static void Hinted(StringBuilder text, Clause[]? options)
	{
		if (options is not { Length: > 0 })
			return;

		text.Append(" OPTION (");
		Each(text, options);
		text.Append(')');
	}

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

		// The statements SQL Server will not read without their terminator. The published
		// syntax writes the semicolon into the block for each of them, and ScriptDom holds
		// them to it — which is how this list was found and is how it grows.
		Said(text, statement, record);

		if (Terminated.Contains(record))
			text.Append(';');
	}

	/// <summary>The statements that are not read back without a semicolon after them.</summary>
	static readonly HashSet<string> Terminated = new(StringComparer.Ordinal)
	{
		"FullTextStopListDefinition",
		"AlterFullTextStopList",
		"SearchPropertyListDefinition",
		"AlterSearchPropertyList",
		"DropSensitivityClassification",
		"DropFulltextStoplist",
		"DropSearchPropertyList",
	};

	static void Said(StringBuilder text, Statement statement, string record)
	{

		if (record.StartsWith("AlterDatabase", StringComparison.Ordinal) &&
			statement.GetType().GetProperty("Name")?.GetValue(statement) is string database)
		{
			text.Append("ALTER DATABASE ").Append(database).Append(' ')
				.Append(Words(record["AlterDatabase".Length..]));

			Tailed(text, statement);

			return;
		}

		// The verb the author wrote, where the statement has more than one and the record's
		// own name cannot say which.
		text.Append(statement switch
		{
			Statement.Definition { Verb: { } verb }    => verb + Words(record)["CREATE".Length..],
			Statement.DropSignature { Counter: true }  => "DROP COUNTER SIGNATURE",
			_                                          => Words(record),
		});

		// A word of the statement rather than of the names, so it goes between the two.
		if (statement is Statement.Removal { IfExists: true })
			text.Append(" IF EXISTS");

		switch (statement)
		{
			case Statement.DropFulltextIndex([var index]):
				// The one drop that names not what it removes but what it removes it from.
				text.Append(" ON ");
				Put(text, index, 0);
				break;

			case Statement.DropSignature(var by, var module, _):
				text.Append(" FROM ").Append(module).Append(" BY ");

				for (var i = 0; i < by.Length; i++)
				{
					if (i > 0)
						text.Append(", ");

					Put(text, by[i], 0);
				}

				break;

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

		Tailed(text, statement);
	}

	/// <summary>The catalogue's own words after the name, as they were written.</summary>
	static void Tailed(StringBuilder text, Statement statement)
	{
		if (statement is Statement.Definition { Tail: { Length: > 0 } tail })
			text.Append(' ').Append(tail);
		else if (statement is Statement.Removal { Tail: { Length: > 0 } after })
			text.Append(' ').Append(after);
	}

	static readonly Dictionary<string, string> Spelled = new(StringComparer.Ordinal)
	{
		["BackupTransactionLog"]   = "BACKUP LOG",
		["PerformCutover"]         = "PERFORM_CUTOVER",
		["RestoreFileListOnly"]    = "RESTORE FILELISTONLY",
		["RestoreHeaderOnly"]      = "RESTORE HEADERONLY",
		["RestoreLabelOnly"]       = "RESTORE LABELONLY",
		["RestoreRewindOnly"]      = "RESTORE REWINDONLY",
		["RestoreVerifyOnly"]      = "RESTORE VERIFYONLY",
		["AuditSpecificationDefinition"]         = "CREATE SERVER AUDIT SPECIFICATION",
		["DatabaseAuditSpecificationDefinition"] = "CREATE DATABASE AUDIT SPECIFICATION",
		["DropSensitivityClassification"]        = "DROP SENSITIVITY CLASSIFICATION FROM",
		["FullTextStopListDefinition"]           = "CREATE FULLTEXT STOPLIST",
		["AlterFullTextStopList"]                = "ALTER FULLTEXT STOPLIST",
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

		// Two names the reference writes as one word and the record as two, which is the
		// one place the split is not the inverse of the naming. Two arguments and not
		// three: the overload taking a StringComparison is not on netstandard2.0, and
		// this one is ordinal by definition, which is what the third argument said.
		record = record
			.Replace("FullText",  "Fulltext")
			.Replace("FileGroup", "Filegroup");

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
				var quantifier, var top, var columns, var into, var from, var where, var group, var having, var windows):
				text.Append("SELECT ");

				if (quantifier is not null)
					text.Append(quantifier).Append(' ');

				if (top is not null)
				{
					Put(text, top);
					text.Append(' ');
				}

				Each(text, columns);

				if (into is not null)
				{
					text.Append(' ');
					Put(text, into);
				}

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

				if (windows is not null)
				{
					text.Append(" WINDOW ");
					Each(text, windows);
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

			case Query.Parenthesized(var inner):
				text.Append('(');
				Put(text, inner, 0);
				text.Append(')');
				break;

			case Query.DefaultValues:
				text.Append("DEFAULT VALUES");
				break;

			case Query.FromFile(var file, _):
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

			if (rows[i] is Expression.RowValueConstructor or Expression.Parenthesized)
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
			case TableReference.Named(var table, var version, var name, var columns, var sample, var hints):
				text.Append(table);

				if (version is not null)
				{
					text.Append(' ');
					Put(text, version);
				}

				Alias(text, name, columns);

				if (sample is not null)
				{
					text.Append(' ');
					Put(text, sample);
				}

				Hints(text, hints);
				break;

			case TableReference.Parenthesized(var inner):
				text.Append('(');
				Put(text, inner);
				text.Append(')');
				break;

			case TableReference.OdbcJoin(var joined):
				text.Append("{ OJ ");
				Put(text, joined);
				text.Append(" }");
				break;

			case TableReference.Pivot(var of, var aggregate, var by, var names, var alias):
				Put(text, of);
				text.Append(" PIVOT (");
				Put(text, aggregate, 0);
				text.Append(" FOR ").Append(by).Append(" IN (");
				text.Append(string.Join(", ", names)).Append(")) AS ").Append(alias);
				break;

			case TableReference.Unpivot(var of, var value, var by, var names, var alias):
				Put(text, of);
				text.Append(" UNPIVOT (").Append(value);
				text.Append(" FOR ").Append(by).Append(" IN (");
				text.Append(string.Join(", ", names)).Append(")) AS ").Append(alias);
				break;

			case TableReference.Derived(var query, var name, var columns):
				text.Append('(');
				Put(text, query, 0);
				text.Append(')');
				Alias(text, name, columns);
				break;

			case TableReference.FunctionCall(var call, var name, var columns, var schema):
				Put(text, call, 0);

				// `OPENXML (…) WITH t1` names a table whose columns say the shape, and
				// `WITH (c1 INT, …)` writes the shape out. A schema column has a type where
				// it was written out, so one column with none is the first spelling.
				if (schema is [Clause.JsonColumn(var only, null, null, false)])
				{
					text.Append(" WITH ").Append(only);
				}
				else if (schema.Length > 0)
				{
					text.Append(" WITH (");
					Each(text, schema);
					text.Append(')');
				}

				Alias(text, name, columns);
				break;

			case TableReference.Joined(var kind, var outer, var natural, var left, var right, var on, var over, var hint):
				Put(text, left);
				text.Append(' ');

				if (natural)
					text.Append("NATURAL ");

				text.Append(kind switch
				{
					SqlJoin.Cross       => "CROSS",
					SqlJoin.Left        => "LEFT",
					SqlJoin.Right       => "RIGHT",
					SqlJoin.Full        => "FULL",
					SqlJoin.Union       => "UNION",
					SqlJoin.CrossApply  => "CROSS",
					SqlJoin.OuterApply  => "OUTER",
					SqlJoin.Inner       => "INNER",
					_                   => "",
				});

				if (outer)
					text.Append(" OUTER");

				if (kind != SqlJoin.Unspecified)
					text.Append(' ');

				if (hint is not null)
					text.Append(hint).Append(' ');

				text.Append(kind is SqlJoin.CrossApply or SqlJoin.OuterApply ? "APPLY " : "JOIN ");

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

	static void Hints(StringBuilder text, Clause[] hints)
	{
		if (hints.Length == 0)
			return;

		text.Append(" WITH (");
		Each(text, hints);
		text.Append(')');
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

	/// <summary>Sort specifications in brackets, or nothing where there are none.</summary>
	static void Columns(StringBuilder text, Clause[]? columns)
	{
		if (columns is not { Length: > 0 })
			return;

		text.Append(" (");
		Each(text, columns);
		text.Append(')');
	}

	/// <summary>A word and sort specifications in brackets, or nothing where there are none.</summary>
	static void Columns(StringBuilder text, Clause[]? columns, string word)
	{
		if (columns is not { Length: > 0 })
			return;

		text.Append(' ').Append(word);
		Columns(text, columns);
	}

	/// <summary>A <c>WITH (…)</c>, or nothing where there is none.</summary>
	static void Optioned(StringBuilder text, Clause[]? options, bool bracketed = true)
	{
		if (options is not { Length: > 0 })
			return;

		text.Append(bracketed ? " WITH (" : " WITH ");
		Each(text, options);

		if (bracketed)
			text.Append(')');
	}

	/// <summary>What was written, one space between the pieces and nothing for a piece that was not.</summary>
	static void Pieces(StringBuilder text, params string?[] pieces)
	{
		var first = true;

		foreach (var piece in pieces)
		{
			if (piece is not { Length: > 0 })
				continue;

			if (!first)
				text.Append(' ');

			text.Append(piece);
			first = false;
		}
	}

	/// <summary>Placements one after another, each with its word.</summary>
	static void Placed(StringBuilder text, Clause[]? placements)
	{
		foreach (var one in placements ?? Clause.None)
		{
			text.Append(' ');
			Put(text, one);
		}
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
				text.Append("TOP ");
				Put(text, value, 0);

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

			case Clause.Window(var window, var partition, var by, var frame) over:
				// A window that is only a name was written without brackets; anything else
				// has them, and an empty `OVER ()` is written with nothing inside.
				if (window is not null && partition.Length == 0 && by is null && frame is null)
				{
					text.Append("OVER ").Append(window);

					break;
				}

				text.Append("OVER (");
				Window(text, over);
				text.Append(')');
				break;

			case Clause.WindowDefinition(var defined, var specification):
				text.Append(defined).Append(" AS (");
				Window(text, (Clause.Window)specification);
				text.Append(')');
				break;

			case Clause.Into(var table, var on):
				text.Append("INTO ").Append(table);

				if (on is not null)
					text.Append(" ON ").Append(on);

				break;

			case Clause.SystemTime(var kind, var at):
				// `FOR PATH` is a graph table's, written without the SYSTEM_TIME.
				text.Append(kind == "PATH" ? "FOR " : "FOR SYSTEM_TIME ").Append(kind);

				if (at.Length == 1)
				{
					text.Append(' ');
					Put(text, at[0], 0);
				}
				else if (at.Length == 2)
				{
					// Two times, and the word between them is the clause's own.
					text.Append(' ');

					if (kind == "CONTAINED IN")
					{
						text.Append('(');
						Put(text, at[0], 0);
						text.Append(", ");
						Put(text, at[1], 0);
						text.Append(')');
					}
					else
					{
						Put(text, at[0], 0);
						text.Append(kind == "BETWEEN" ? " AND " : " TO ");
						Put(text, at[1], 0);
					}
				}

				break;

			case Clause.TableSample(var system, var value, var unit, var seed):
				text.Append("TABLESAMPLE ");

				if (system)
					text.Append("SYSTEM ");

				text.Append('(');
				Put(text, value, 0);

				if (unit is not null)
					text.Append(' ').Append(unit);

				text.Append(')');

				if (seed is not null)
				{
					text.Append(" REPEATABLE (");
					Put(text, seed, 0);
					text.Append(')');
				}

				break;

			case Clause.JsonColumn(var column, var type, var path, var json):
				text.Append(column);

				if (type is not null)
					text.Append(' ').Append(type);

				if (path is not null)
					text.Append(' ').Append(path);

				if (json)
					text.Append(" AS JSON");

				break;

			case Clause.Hint(var hint):
				text.Append(hint);
				break;

			case Clause.Option(var name, var value, var options, var partitions, var bare):
				text.Append(name);

				if (value is not null)
				{
					text.Append(bare ? " " : " = ");
					Put(text, value, 0);
				}

				// A placement under `MOVE TO` stands after the words; a list stands in brackets,
				// after the `=` where the list is the whole of what was assigned.
				if (options is [Clause.Placement placement])
				{
					text.Append(' ');
					Put(text, placement);
				}
				else if (options.Length > 0)
				{
					text.Append(value is null && !bare ? " = (" : " (");
					Each(text, options);
					text.Append(')');
				}

				if (partitions is not null)
					text.Append(' ').Append(partitions);

				break;

			case Clause.Placement(var kind, var target, var columns):
				if (kind.Length > 0)
					text.Append(kind).Append(' ');

				text.Append(target);
				Names(text, columns);
				break;

			case Clause.ColumnOption(var kind, var arguments, var options, var constraintName):
				if (constraintName is not null)
					text.Append("CONSTRAINT ").Append(constraintName).Append(' ');

				text.Append(kind);

				if (arguments.Length > 0 && kind == "COLLATE")
				{
					text.Append(' ');
					Put(text, arguments[0], 0);
				}
				else if (arguments.Length > 0)
				{
					text.Append(" (");

					for (var i = 0; i < arguments.Length; i++)
					{
						if (i > 0)
							text.Append(", ");

						Put(text, arguments[i], 0);
					}

					text.Append(')');
				}

				if (options.Length > 0 && kind is "MASKED" or "ENCRYPTED")
				{
					Optioned(text, options);
				}
				else
				{
					foreach (var one in options)
					{
						text.Append(' ');
						Put(text, one);
					}
				}

				break;

			case Clause.References(var table, var columns, var onDelete, var onUpdate, var replication):
				text.Append("REFERENCES ").Append(table);
				Names(text, columns);

				if (onDelete is not null)
					text.Append(" ON DELETE ").Append(onDelete);

				if (onUpdate is not null)
					text.Append(" ON UPDATE ").Append(onUpdate);

				if (replication)
					text.Append(" NOT FOR REPLICATION");

				break;

			case Clause.Connection(var from, var to):
				text.Append(from).Append(" TO ").Append(to);
				break;

			case Clause.Dropped(var kind, var dropped, var ifExists, var how):
				// The kind is empty where the author did not write the word: `DROP
				// CONSTRAINT c1 WITH (…), c2 WITH (…)` drops two and says `CONSTRAINT` once.
				Pieces(text, kind, ifExists ? "IF EXISTS" : null, dropped, how);
				break;

			case Clause.DatabaseFile(var options):
				text.Append('(');
				Each(text, options);
				text.Append(')');
				break;

			case Clause.FileGroup(var name, var contains, var isDefault, var files):
				text.Append("FILEGROUP ").Append(name);

				if (contains is not null)
					text.Append(" CONTAINS ").Append(contains);

				if (isDefault)
					text.Append(" DEFAULT");

				text.Append(' ');
				Each(text, files);
				break;

			case Clause.EventPiece(var action, var name, var set, var actions, var where):
				text.Append(action).Append(' ').Append(name);

				if (set is not null || actions is not null || where is not null)
				{
					text.Append(" (");

					if (set is not null)
					{
						text.Append("SET ");
						Each(text, set);
					}

					if (actions is not null)
					{
						if (set is not null)
							text.Append(' ');

						text.Append("ACTION (").Append(string.Join(", ", actions)).Append(')');
					}

					if (where is not null)
					{
						if (set is not null || actions is not null)
							text.Append(' ');

						text.Append("WHERE ").Append(where);
					}

					text.Append(')');
				}

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

			case Clause.Set(var target, var by, var value, var through):
				text.Append(target);

				// `.WRITE (…)`: the target names the method and the value is its arguments.
				if (by == "")
				{
					text.Append(' ');
					Put(text, value, 0);
					break;
				}

				if (through is not null)
					text.Append(" = ").Append(through);

				text.Append(' ').Append(by ?? "=").Append(' ');
				Put(text, value, 0);
				break;

			case Clause.Output(var items, var into, var columns, var next):
				text.Append("OUTPUT ");
				Each(text, items);

				if (into is not null)
				{
					text.Append(" INTO ");
					Put(text, into);
					Names(text, columns);
				}

				if (next is not null)
				{
					text.Append(' ');
					Put(text, next);
				}

				break;

			case Clause.MergeWhen(var matched, var by, var condition, var action):
				text.Append(matched ? "WHEN MATCHED" : by is null ? "WHEN NOT MATCHED" : "WHEN NOT MATCHED BY " + by);

				if (condition is not null)
				{
					text.Append(" AND ");
					Put(text, condition, 0);
				}

				text.Append(" THEN ");
				Arm(text, action);
				break;

			case Clause.VariableDeclaration(
					var name, var type, var value, var elements, var nullability, var tail, var said):
				text.Append(name);

				if (said)
					text.Append(" AS");

				if (type is not null)
					text.Append(' ').Append(type);

				if (elements is not null)
				{
					text.Append(" (");
					Each(text, elements);
					text.Append(')');
				}

				if (nullability is not null)
					text.Append(' ').Append(nullability);

				if (value is not null)
				{
					text.Append(" = ");
					Put(text, value, 0);
				}

				if (tail is not null)
					text.Append(' ').Append(tail);

				break;

			case Clause.ParameterDeclaration(var name, var type, var value, var nullability, var varying, var ways):
				text.Append(name);

				if (type is not null)
					text.Append(' ').Append(type);

				if (varying)
					text.Append(" VARYING");

				if (nullability is not null)
					text.Append(' ').Append(nullability);

				if (value is not null)
				{
					text.Append(" = ");
					Put(text, value, 0);
				}

				foreach (var way in ways ?? Syntax.NoNames)
					text.Append(' ').Append(way);

				break;

			case Clause.ColumnDefinition(var name, var type, var computed, var options):
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

				foreach (var one in options)
				{
					text.Append(' ');
					Put(text, one);
				}

				break;

			case Clause.ConstraintDefinition constraint:
				Put(text, constraint, null);
				break;

		}
	}

	/// <summary>
	/// A constraint or an index with everything T-SQL writes after it, in its order — and,
	/// given a table, as the <c>CREATE INDEX</c> that stands on its own.
	/// </summary>
	static void Put(
		StringBuilder text, Clause.ConstraintDefinition constraint, string? on, string? over = null)
	{
		var name    = constraint.Name;
		var kind    = constraint.Kind;
		var columns = constraint.Columns;
		var check   = constraint.Check;
		var index   = kind is "INDEX" or "UNIQUE INDEX";
		var began   = text.Length;

		// An index is `INDEX name`, or `UNIQUE CLUSTERED INDEX name ON t` on its own; a
		// constraint is `CONSTRAINT name` and then what it is.
		if (index && on is not null)
		{
			if (kind == "UNIQUE INDEX")
				text.Append("UNIQUE ");

			if (constraint.Clustering is { } clustered)
				text.Append(clustered).Append(' ');

			if (constraint.Columnstore)
				text.Append("COLUMNSTORE ");

			text.Append("INDEX ").Append(name).Append(" ON ").Append(on);
		}
		else if (index)
		{
			text.Append("INDEX ").Append(name);

			if (kind == "UNIQUE INDEX")
				text.Append(" UNIQUE");

			if (constraint.Clustering is { } clustering)
				text.Append(' ').Append(clustering);

			if (constraint.Columnstore)
				text.Append(" COLUMNSTORE");
		}
		else
		{
			if (name is not null)
				text.Append("CONSTRAINT ").Append(name).Append(' ');

			// A column's bare `REFERENCES t (c)` is a constraint whose kind is the word the
			// reference writes for itself, and writing the kind too would say it twice.
			if (kind != "REFERENCES")
				text.Append(kind);

			if (constraint.Clustering is { } clustering)
				text.Append(' ').Append(clustering);

			if (constraint.Columnstore)
				text.Append(" COLUMNSTORE");
		}

		if (constraint.Hash)
			text.Append(" HASH");

		// A connection's pairs and a check's `NOT FOR REPLICATION` stand where a key's columns
		// do; the options of anything else are its `WITH`.
		var pairs   = constraint.Options?.OfType<Clause.Connection>().ToArray() ?? [];
		var words   = kind is "CHECK" or "CONNECTION" ? constraint.Options?.OfType<Clause.Option>().ToArray() ?? [] : [];
		var options = kind is "CHECK" or "CONNECTION" ? null : constraint.Options;

		if (kind == "CHECK")
			foreach (var word in words)
				text.Append(' ').Append(word.Name);

		if (pairs.Length > 0)
		{
			text.Append(" (");
			Each(text, pairs);
			text.Append(')');
		}

		Columns(text, columns);

		if (over is not null)
			text.Append(' ').Append(over);

		Names(text, constraint.Order is null ? null : constraint.Order, "ORDER");
		Names(text, constraint.Include, "INCLUDE");

		// A check is written in brackets; a default is written as the author wrote it, whose
		// own brackets the tree keeps — adding a pair would add one per round.
		if (check is not null && kind == "DEFAULT")
		{
			text.Append(' ');
			Put(text, check, 0);
		}
		else if (check is not null)
		{
			text.Append(" (");
			Put(text, check, 0);
			text.Append(')');
		}

		if (constraint.ForColumn is { } forColumn)
			text.Append(" FOR ").Append(forColumn);

		if (constraint.Filter is { } filter)
		{
			text.Append(" WHERE ");
			Put(text, filter, 0);
		}

		if (constraint.Referenced is { } references)
		{
			if (text.Length > began)
				text.Append(' ');

			Put(text, references);
		}

		if (kind == "CONNECTION")
			foreach (var word in words)
				text.Append(' ').Append(word.Name);

		Optioned(text, options, constraint.Bracketed);
		Placed(text, constraint.Placements);

		if (constraint.Enforced is { } enforced)
			text.Append(enforced ? " ENFORCED" : " NOT ENFORCED");
	}

	/// <summary>The inside of a window's brackets: its base, its partition, its order, its frame.</summary>
	static void Window(StringBuilder text, Clause.Window window)
	{
		var (name, partition, by, frame) = window;

		if (name is not null)
			text.Append(name).Append(' ');

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
	}

	/// <summary>A word and names in brackets, or nothing where there are no names.</summary>
	static void Names(StringBuilder text, string[]? columns, string word)
	{
		if (columns is not null)
		{
			text.Append(' ').Append(word);
			Names(text, columns);
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
			case Statement.Update(null, var set, _, _, _, _, _, _):
				text.Append("UPDATE SET ");
				Each(text, set);
				break;

			case Statement.Delete(null, _, _, _, _, _, _):
				text.Append("DELETE");
				break;

			case Statement.Insert(null, var columns, var rows, _, _, _, _):
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

	/// <summary>A sign, and what it is applied to.</summary>
	/// <remarks>
	/// `- -1` and not `-(-1)`: two of the same sign need a space between them and nothing
	/// else, since `--` begins a comment — and a sign in front of the other sign needs not
	/// even that. Brackets the author did not write are not the tree's to add.
	/// </remarks>
	static void Signed(StringBuilder text, char sign, Expression operand)
	{
		text.Append(sign);

		var next = operand switch
		{
			Expression.Negate                      => '-',
			Expression.Plus                        => '+',
			Expression.Literal { Text: ['-', ..] } => '-',
			Expression.Literal { Text: ['+', ..] } => '+',
			_                                      => '\0',
		};

		if (next == sign)
			text.Append(' ');

		Put(text, operand, next == '\0' ? 8 : 0);
	}

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
			case Expression.Concatenate(var l, var r):  Binary(text, l, "||", r, binds);         break;
			case Expression.Multiply(var l, var r):     Binary(text, l, "*", r, binds);          break;
			case Expression.Divide(var l, var r):       Binary(text, l, "/", r, binds);          break;

			case Expression.Not(var operand):
				text.Append("NOT ");
				Put(text, operand, 4);
				break;

			case Expression.Negate(var operand): Signed(text, '-', operand); break;
			case Expression.Plus(var operand):   Signed(text, '+', operand); break;

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

			case Expression.Measured(var value, var unit):
				Put(text, value, 8);
				text.Append(' ').Append(unit);
				break;

			case Expression.Hinted(var value, var words):
				Put(text, value, 0);
				text.Append(' ').Append(words);
				break;

			case Expression.GraphMatch(var pattern):
				text.Append("MATCH (").Append(pattern).Append(')');
				break;

			case Expression.JsonPair(var key, var value):
				Put(text, key, 0);
				text.Append(':');
				Put(text, value, 0);
				break;

			case Expression.OdbcEscape(var kind, var value, var bracketed):
				text.Append("{ ").Append(kind).Append(' ');

				// The name inside `{ fn … }` is ODBC's and not this language's. `{ fn convert
				// (@a, sql_int) }` is ODBC's two-argument convert and `{ fn current_date () }`
				// keeps its brackets, so neither may be printed through the syntax T-SQL gives
				// that word — which is what `Call` is a catalogue of.
				if (bracketed && value is Expression.RoutineInvocation(var called, var given, null))
				{
					text.Append(called).Append('(');

					for (var i = 0; i < given.Length; i++)
					{
						if (i > 0)
							text.Append(", ");

						Put(text, given[i], 0);
					}

					text.Append(')');
				}
				else
				{
					Put(text, value, 0);
				}

				text.Append(" }");
				break;

			case Expression.Pieced(var parts):
				for (var i = 0; i < parts.Length; i++)
				{
					if (i > 0)
						text.Append("; ");

					Put(text, parts[i], 0);
				}

				break;

			case Expression.RowsetOrder(var by, var unique):
				text.Append("ORDER (");
				Each(text, by);
				text.Append(unique ? ") UNIQUE" : ")");
				break;

			case Expression.Prefixed(var word, var value):
				text.Append(word).Append(' ');
				Put(text, value, 0);
				break;

			case Expression.NamedArgument(var name, var value):
				text.Append(name).Append(" = ");
				Put(text, value, 0);
				break;

			case Expression.Parenthesized(var inner):
				text.Append('(');
				Put(text, inner, 0);
				text.Append(')');
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
				text.Append(name).Append('(').Append(word).Append(" FROM ");
				Put(text, arguments[0], 0);
				text.Append(')');
				return;

			case "POSITION":
				text.Append(name).Append('(');
				Put(text, arguments[0], 0);
				text.Append(" IN ");
				Put(text, arguments[1], 0);
				text.Append(')');
				return;

			case "TRIM":
				text.Append(name).Append('(');

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
				text.Append(' ').Append(name).Append(' ');
				Put(text, arguments[1], 8);
				return;

			case "NEXT VALUE FOR":
				text.Append(name).Append(' ').Append(word);
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
