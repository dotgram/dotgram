using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Handwritten;

// ISO/IEC 9075-2:2023 §14's cursors and single-row select, §16's control statements, §17's
// transactions, §18's connections, §19's session, §20's dynamic SQL, §22's direct invocation and
// §23's diagnostics.
partial class HandSqlStandard
{
	// ── The publications ───────────────────────────────────────────────────────

	public static Statement ParseDirectSQLStatement(string input)
	{
		return TryParseDirectSQLStatement(input, out var value) ? value : throw Refused(input, "direct SQL statement");
	}

	public static bool TryParseDirectSQLStatement(string input, out Statement value)
	{
		var cursor = new SqlCursor(input);

		if (DirectlyExecutableStatement(ref cursor, out value) && cursor.Take(SqlTokenKind.Semicolon) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	public static Statement ParseDirectSQLDataStatement(string input)
	{
		return TryParseDirectSQLDataStatement(input, out var value) ? value : throw Refused(input, "direct SQL data statement");
	}

	public static bool TryParseDirectSQLDataStatement(string input, out Statement value)
	{
		return Whole(input, DirectSQLDataStatement, out value);
	}

	public static Statement ParseSQLDataStatement(string input)
	{
		return TryParseSQLDataStatement(input, out var value) ? value : throw Refused(input, "SQL data statement");
	}

	public static bool TryParseSQLDataStatement(string input, out Statement value)
	{
		return Whole(input, SQLDataStatement, out value);
	}

	public static Statement ParseSQLControlStatement(string input)
	{
		return TryParseSQLControlStatement(input, out var value) ? value : throw Refused(input, "SQL control statement");
	}

	public static bool TryParseSQLControlStatement(string input, out Statement value)
	{
		return Whole(input, SQLControlStatement, out value);
	}

	public static Statement ParseSQLTransactionStatement(string input)
	{
		return TryParseSQLTransactionStatement(input, out var value) ? value : throw Refused(input, "SQL transaction statement");
	}

	public static bool TryParseSQLTransactionStatement(string input, out Statement value)
	{
		return Whole(input, SQLTransactionStatement, out value);
	}

	public static Statement ParseSQLConnectionStatement(string input)
	{
		return TryParseSQLConnectionStatement(input, out var value) ? value : throw Refused(input, "SQL connection statement");
	}

	public static bool TryParseSQLConnectionStatement(string input, out Statement value)
	{
		return Whole(input, SQLConnectionStatement, out value);
	}

	public static Statement ParseSQLSessionStatement(string input)
	{
		return TryParseSQLSessionStatement(input, out var value) ? value : throw Refused(input, "SQL session statement");
	}

	public static bool TryParseSQLSessionStatement(string input, out Statement value)
	{
		return Whole(input, SQLSessionStatement, out value);
	}

	public static Statement.GetDiagnostics ParseSQLDiagnosticsStatement(string input)
	{
		return TryParseSQLDiagnosticsStatement(input, out var value) ? value : throw Refused(input, "SQL diagnostics statement");
	}

	public static bool TryParseSQLDiagnosticsStatement(string input, out Statement.GetDiagnostics value)
	{
		var cursor = new SqlCursor(input);

		if (SQLDiagnosticsStatement(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	public static Statement ParseSQLDynamicStatement(string input)
	{
		return TryParseSQLDynamicStatement(input, out var value) ? value : throw Refused(input, "SQL dynamic statement");
	}

	public static bool TryParseSQLDynamicStatement(string input, out Statement value)
	{
		return Whole(input, SQLDynamicStatement, out value);
	}

	public static Statement ParseSQLProcedureStatement(string input)
	{
		return TryParseSQLProcedureStatement(input, out var value) ? value : throw Refused(input, "SQL procedure statement");
	}

	public static bool TryParseSQLProcedureStatement(string input, out Statement value)
	{
		return Whole(input, SQLExecutableStatement, out value);
	}

	/// <summary>What a statement's publication does: read it, and ask that nothing followed it.</summary>
	delegate bool Reading(ref SqlCursor cursor, out Statement value);

	static bool Whole(string input, Reading read, out Statement value)
	{
		var cursor = new SqlCursor(input);

		if (read(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	// ── §22 Direct invocation of SQL ───────────────────────────────────────────

	static bool DirectlyExecutableStatement(ref SqlCursor cursor, out Statement statement)
	{
		return DirectSQLDataStatement(ref cursor, out statement) ||
		SQLSchemaStatement(ref cursor, out statement) ||
		SQLTransactionStatement(ref cursor, out statement) ||
		SQLConnectionStatement(ref cursor, out statement) ||
		SQLSessionStatement(ref cursor, out statement);
	}

	static bool DirectSQLDataStatement(ref SqlCursor cursor, out Statement statement)
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

			case SqlWord.Update:
			{
				if (UpdateStatement(ref cursor, out var updated, false))
				{
					statement = updated;

					return true;
				}

				break;
			}

			case SqlWord.Truncate:
			{
				if (TruncateTableStatement(ref cursor, out var truncated))
				{
					statement = truncated;

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

			case SqlWord.Declare:
			{
				if (TemporaryTableDeclaration(ref cursor, out var declared))
				{
					statement = declared;

					return true;
				}

				break;
			}
		}

		// <cursor specification>: a query, and whether it is updated through.
		if (CursorSpecification(ref cursor, out var query))
		{
			statement = query;

			return true;
		}

		statement = null!;

		return false;
	}

	static bool CursorSpecification(ref SqlCursor cursor, out Statement.Select query)
	{
		if (!QueryExpression(ref cursor, out query))
			return false;

		var updatability = UpdatabilityClause(ref cursor);

		if (updatability is not null)
			query = query with { Updatability = updatability };

		return true;
	}

	static UpdatabilityClause? UpdatabilityClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlWord.For))
			return null;

		if (cursor.TakeWord("READ") && cursor.Take(SqlWord.Only))
			return new UpdatabilityClause(true, []);

		cursor = save;
		cursor.Take();

		if (cursor.Take(SqlWord.Update))
		{
			IReadOnlyList<Identifier>? columns = null;

			var marked = cursor;

			if (cursor.Take(SqlWord.Of) && !ColumnNameList(ref cursor, out columns!))
			{
				cursor  = marked;
				columns = null;
			}

			return new UpdatabilityClause(false, columns ?? []);
		}

		cursor = save;

		return null;
	}

	static bool TemporaryTableDeclaration(ref SqlCursor cursor, out Statement.DeclareLocalTemporaryTable statement)
	{
		var save = cursor;

		statement = null!;

		if (!cursor.Take(SqlWord.Declare) || !cursor.Take(SqlWord.Local) || !cursor.TakeWord("TEMPORARY") || !cursor.Take(SqlWord.Table) ||
			!TableName(ref cursor, out var name) || !TableElementList(ref cursor, out var elements))
		{
			cursor = save;

			return false;
		}

		statement = new Statement.DeclareLocalTemporaryTable { Name = name, Elements = elements, OnCommit = OnCommit(ref cursor) };

		return true;
	}

	/// <summary><c>ON COMMIT PRESERVE ROWS</c> and <c>ON COMMIT DELETE ROWS</c>.</summary>
	static TableCommitAction? OnCommit(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.On) && cursor.Take(SqlWord.Commit))
		{
			var preserve = cursor.TakeWord("PRESERVE");

			if ((preserve || cursor.Take(SqlWord.Delete)) && cursor.Take(SqlWord.Rows))
				return preserve ? TableCommitAction.Preserve : TableCommitAction.Delete;
		}

		cursor = save;

		return null;
	}

	// ── §14 Cursors, the single-row select, and the locators ───────────────────

	/// <summary><c>&lt;SQL data statement&gt;</c>, as a routine's body holds one.</summary>
	static bool SQLDataStatement(ref SqlCursor cursor, out Statement statement)
	{
		var save = cursor;

		switch (cursor.Word)
		{
			case SqlWord.Open:
			{
				cursor.Take();

				if (CursorName(ref cursor, out var name))
				{
					statement = new Statement.OpenCursor { Cursor = new CursorReference(name) };

					return true;
				}

				break;
			}

			case SqlWord.Close:
			{
				cursor.Take();

				if (CursorName(ref cursor, out var name))
				{
					statement = new Statement.CloseCursor { Cursor = new CursorReference(name) };

					return true;
				}

				break;
			}

			case SqlWord.Fetch:
			{
				if (FetchStatement(ref cursor, out var fetched))
				{
					statement = fetched;

					return true;
				}

				break;
			}

			case SqlWord.Select:
			{
				if (SelectStatementSingleRow(ref cursor, out var selected))
				{
					statement = selected;

					return true;
				}

				break;
			}

			case SqlWord.Free:
			case SqlWord.Hold:
			{
				var free = cursor.Word == SqlWord.Free;

				cursor.Take();

				if (cursor.TakeWord("LOCATOR"))
				{
					var locators = new List<Expression>();
					var listed   = cursor;

					while (true)
					{
						if (!LocatorReference(ref cursor, out var locator))
						{
							// The comma the list cannot go on from is given back, and refused
							// where it stands: `FREE LOCATOR :l,` is no statement.
							cursor = listed;

							break;
						}

						locators.Add(locator);
						listed = cursor;

						if (!cursor.Take(SqlTokenKind.Comma))
							break;
					}

					if (locators.Count > 0)
					{
						statement = free
							? new Statement.FreeLocator { Locators = locators }
							: new Statement.HoldLocator { Locators = locators };

						return true;
					}
				}

				break;
			}

			case SqlWord.Delete:
			{
				// A positioned delete is asked first: it ends in `WHERE CURRENT OF`, which a
				// searched one would read as far as its own `WHERE` and stop.
				if (DeleteStatement(ref cursor, out var positioned, true))
				{
					statement = positioned;

					return true;
				}

				if (DeleteStatement(ref cursor, out var searched, false))
				{
					statement = searched;

					return true;
				}

				break;
			}

			case SqlWord.Update:
			{
				if (UpdateStatement(ref cursor, out var positioned, true))
				{
					statement = positioned;

					return true;
				}

				if (UpdateStatement(ref cursor, out var searched, false))
				{
					statement = searched;

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

			case SqlWord.Truncate:
			{
				if (TruncateTableStatement(ref cursor, out var truncated))
				{
					statement = truncated;

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
		}

		cursor    = save;
		statement = null!;

		return false;
	}

	static bool FetchStatement(ref SqlCursor cursor, out Statement.FetchCursor statement)
	{
		var save = cursor;

		statement = null!;

		cursor.Take();

		var orientation = FetchOrientation(ref cursor);
		var from        = cursor.Take(SqlWord.From);

		if (!from && orientation is not null)
		{
			// `FETCH <orientation> FROM c`: the orientation is written only with `FROM`.
			cursor      = save;
			orientation = null;

			cursor.Take();
		}

		if (CursorName(ref cursor, out var name) && cursor.Take(SqlWord.Into) && TargetList(ref cursor, out var targets))
		{
			statement = new Statement.FetchCursor
			{
				Orientation = orientation,
				FromKeyword = from,
				Cursor      = new CursorReference(name),
				Into        = new DynamicArguments.Values(targets),
			};

			return true;
		}

		cursor = save;

		return false;
	}

	static FetchOrientation? FetchOrientation(ref SqlCursor cursor)
	{
		var save = cursor;

		var simple =
			cursor.TakeWord("NEXT")  ? FetchOrientationKind.Next :
			cursor.TakeWord("PRIOR") ? FetchOrientationKind.Prior :
			cursor.TakeWord("FIRST") ? FetchOrientationKind.First :
			cursor.TakeWord("LAST")  ? FetchOrientationKind.Last :
			(FetchOrientationKind?)null;

		if (simple is not null)
			return new Ast.FetchOrientation(simple.Value);

		var counted =
			cursor.TakeWord("ABSOLUTE") ? FetchOrientationKind.Absolute :
			cursor.TakeWord("RELATIVE") ? FetchOrientationKind.Relative :
			(FetchOrientationKind?)null;

		if (counted is not null && SimpleValueSpecification(ref cursor, out var value))
			return new Ast.FetchOrientation(counted.Value, value);

		cursor = save;

		return null;
	}

	static bool SelectStatementSingleRow(ref SqlCursor cursor, out Statement.Select statement)
	{
		var save = cursor;

		statement = null!;

		cursor.Take();

		var quantifier = SetQuantifier(ref cursor);

		if (SelectList(ref cursor, out var items) && cursor.Take(SqlWord.Into) && TargetList(ref cursor, out var targets) &&
			TableExpression(ref cursor, out var table))
		{
			statement = new Statement.Select
			{
				Quantifier = quantifier,
				Items      = items,
				Into       = new IntoClause(targets),
				From       = table.From,
				Where      = table.Where,
				GroupBy    = table.GroupBy,
				Having     = table.Having,
				Window     = table.Window,
			};

			return true;
		}

		cursor = save;

		return false;
	}

	static bool TargetList(ref SqlCursor cursor, out IReadOnlyList<Expression> targets)
	{
		var save    = cursor;
		var written = new List<Expression>();

		targets = written;

		while (true)
		{
			if (!TargetSpecification(ref cursor, out var target))
			{
				cursor = save;

				return false;
			}

			written.Add(target);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		return true;
	}

	/// <summary>
	/// <c>&lt;target specification&gt;</c>: a host parameter with its indicator, a name, an element of
	/// the array a name names, or a dynamic parameter.
	/// </summary>
	static bool TargetSpecification(ref SqlCursor cursor, out Expression target)
	{
		var save = cursor;

		if (cursor.Kind == SqlTokenKind.Colon || cursor.Kind == SqlTokenKind.Question)
			return GeneralValueSpecification(ref cursor, out target);

		if (cursor.Word == SqlWord.Module)
		{
			var word = cursor.TextOf(cursor.Token);

			cursor.Take();

			if (cursor.Take(SqlTokenKind.Dot) && Identifier(ref cursor, out var qualified) &&
				cursor.Take(SqlTokenKind.Dot) && Identifier(ref cursor, out var column))
			{
				target = new Expression.Reference(new QualifiedName([new Identifier(word), qualified, column]));

				return true;
			}

			cursor = save;
			target = null!;

			return false;
		}

		if (IdentifierChain(ref cursor, out var name))
		{
			if (cursor.Kind is SqlTokenKind.LeftBracket or SqlTokenKind.LeftTrigraph)
			{
				var trigraphs = cursor.Kind == SqlTokenKind.LeftTrigraph;
				var bracket   = cursor;

				cursor.Take();

				if (SimpleValueSpecification(ref cursor, out var index) && cursor.Kind is SqlTokenKind.RightBracket or SqlTokenKind.RightTrigraph)
				{
					cursor.Take();

					target = new Expression.Element(new Expression.Reference(name), index, null, trigraphs);

					return true;
				}

				cursor = bracket;
			}

			target = new Expression.Reference(name);

			return true;
		}

		target = null!;

		return false;
	}

	static bool LocatorReference(ref SqlCursor cursor, out Expression locator)
	{
		var save = cursor;

		if (cursor.Take(SqlTokenKind.Colon))
		{
			if (Identifier(ref cursor, out var name))
			{
				locator = new Expression.Parameter(ParameterKind.Host, name);

				return true;
			}

			cursor = save;
		}

		if (cursor.Take(SqlTokenKind.Question))
		{
			locator = new Expression.Parameter(ParameterKind.Dynamic);

			return true;
		}

		locator = null!;

		return false;
	}

	// ── §16 Control statements ─────────────────────────────────────────────────

	static bool SQLControlStatement(ref SqlCursor cursor, out Statement statement)
	{
		var save = cursor;

		statement = null!;

		if (cursor.Take(SqlWord.Call))
		{
			if (RoutineInvocation(ref cursor, out var invocation))
			{
				statement = new Statement.Call { Invocation = (Expression.Invocation)invocation };

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Return))
		{
			if (ValueExpression(ref cursor, out var value))
			{
				statement = new Statement.Return { Value = value.Node };

				return true;
			}

			if (cursor.Take(SqlWord.Null))
			{
				statement = new Statement.Return { NullKeyword = true };

				return true;
			}

			cursor = save;
		}

		return false;
	}

	// ── §17 Transaction management ─────────────────────────────────────────────

	static bool SQLTransactionStatement(ref SqlCursor cursor, out Statement statement)
	{
		var save = cursor;

		statement = null!;

		switch (cursor.Word)
		{
			case SqlWord.Start:
			{
				cursor.Take();

				if (cursor.TakeWord("TRANSACTION"))
				{
					statement = new Statement.StartTransaction { Modes = TransactionModes(ref cursor) };

					return true;
				}

				break;
			}

			case SqlWord.Set:
			{
				cursor.Take();

				var local = cursor.Take(SqlWord.Local);

				if (cursor.TakeWord("TRANSACTION"))
				{
					statement = new Statement.SetTransaction { Local = local, Modes = TransactionModes(ref cursor) };

					return true;
				}

				if (!local && cursor.TakeWord("CONSTRAINTS") && ConstraintNameList(ref cursor, out var target))
				{
					var deferred = cursor.TakeWord("DEFERRED");

					if (deferred || cursor.TakeWord("IMMEDIATE"))
					{
						statement = new Statement.SetConstraints
						{
							Target = target,
							Timing = deferred ? ConstraintTiming.Deferred : ConstraintTiming.Immediate,
						};

						return true;
					}
				}

				break;
			}

			case SqlWord.Savepoint:
			{
				cursor.Take();

				if (Identifier(ref cursor, out var name))
				{
					statement = new Statement.Savepoint { Name = name };

					return true;
				}

				break;
			}

			case SqlWord.Release:
			{
				cursor.Take();

				if (cursor.Take(SqlWord.Savepoint) && Identifier(ref cursor, out var name))
				{
					statement = new Statement.ReleaseSavepoint { Name = name };

					return true;
				}

				break;
			}

			case SqlWord.Commit:
			{
				cursor.Take();

				var work = cursor.TakeWord("WORK");

				statement = new Statement.Commit { Work = work, Chain = ChainClause(ref cursor) };

				return true;
			}

			case SqlWord.Rollback:
			{
				cursor.Take();

				var work  = cursor.TakeWord("WORK");
				var chain = ChainClause(ref cursor);

				Identifier? savepoint = null;

				var marked = cursor;

				if (cursor.Take(SqlWord.To))
				{
					if (!cursor.Take(SqlWord.Savepoint) || !Identifier(ref cursor, out savepoint))
					{
						cursor    = marked;
						savepoint = null;
					}
				}

				statement = new Statement.Rollback { Work = work, Chain = chain, ToSavepoint = savepoint };

				return true;
			}
		}

		cursor = save;

		return false;
	}

	static IReadOnlyList<TransactionMode> TransactionModes(ref SqlCursor cursor)
	{
		var modes = new List<TransactionMode>();

		while (true)
		{
			var save = cursor;

			if (modes.Count > 0 && !cursor.Take(SqlTokenKind.Comma))
				break;

			if (!TransactionMode(ref cursor, out var mode))
			{
				cursor = save;

				break;
			}

			modes.Add(mode);
		}

		return modes;
	}

	static bool TransactionMode(ref SqlCursor cursor, out TransactionMode mode)
	{
		var save = cursor;

		mode = null!;

		if (cursor.TakeWord("ISOLATION"))
		{
			if (cursor.TakeWord("LEVEL"))
			{
				if (cursor.TakeWord("READ"))
				{
					var uncommitted = cursor.TakeWord("UNCOMMITTED");

					if (uncommitted || cursor.TakeWord("COMMITTED"))
					{
						mode = new TransactionMode.Isolation(uncommitted ? IsolationLevel.ReadUncommitted : IsolationLevel.ReadCommitted);

						return true;
					}
				}
				else if (cursor.TakeWord("REPEATABLE") && cursor.TakeWord("READ"))
				{
					mode = new TransactionMode.Isolation(IsolationLevel.RepeatableRead);

					return true;
				}
				else if (cursor.TakeWord("SERIALIZABLE"))
				{
					mode = new TransactionMode.Isolation(IsolationLevel.Serializable);

					return true;
				}
			}

			cursor = save;

			return false;
		}

		if (cursor.TakeWord("READ"))
		{
			var only = cursor.Take(SqlWord.Only);

			if (only || cursor.TakeWord("WRITE"))
			{
				mode = new TransactionMode.Access(only ? TransactionAccess.ReadOnly : TransactionAccess.ReadWrite);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.TakeWord("DIAGNOSTICS") && cursor.TakeWord("SIZE") && SimpleValueSpecification(ref cursor, out var size))
		{
			mode = new TransactionMode.DiagnosticsSize(size);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool ConstraintNameList(ref SqlCursor cursor, out ConstraintTarget target)
	{
		if (cursor.Take(SqlWord.All))
		{
			target = new ConstraintTarget.All();

			return true;
		}

		var save  = cursor;
		var names = new List<QualifiedName>();

		while (true)
		{
			if (!Names(ref cursor, 3, out var name))
			{
				cursor = save;
				target = null!;

				return false;
			}

			names.Add(name);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		target = new ConstraintTarget.Names(names);

		return true;
	}

	static ChainMode? ChainClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.And))
		{
			var no = cursor.Take(SqlWord.No);

			if (cursor.TakeWord("CHAIN"))
				return no ? ChainMode.NoChain : ChainMode.Chain;
		}

		cursor = save;

		return null;
	}

	// ── §18 Connection management ──────────────────────────────────────────────

	static bool SQLConnectionStatement(ref SqlCursor cursor, out Statement statement)
	{
		var save = cursor;

		statement = null!;

		switch (cursor.Word)
		{
			case SqlWord.Connect:
			{
				cursor.Take();

				if (!cursor.Take(SqlWord.To))
					break;

				if (cursor.Take(SqlWord.Default))
				{
					statement = new Statement.Connect { Target = new ConnectionTarget(null, null, null, true) };

					return true;
				}

				if (SimpleValueSpecification(ref cursor, out var server))
				{
					Expression? name = null, user = null;

					var marked = cursor;

					if (cursor.Take(SqlWord.As) && !SimpleValueSpecification(ref cursor, out name))
					{
						cursor = marked;
						name   = null;
					}

					marked = cursor;

					if (cursor.Take(SqlWord.User) && !SimpleValueSpecification(ref cursor, out user))
					{
						cursor = marked;
						user   = null;
					}

					statement = new Statement.Connect { Target = new ConnectionTarget(server, name, user, false) };

					return true;
				}

				break;
			}

			case SqlWord.Set:
			{
				cursor.Take();

				if (cursor.TakeWord("CONNECTION") && ConnectionObject(ref cursor, out var connection))
				{
					statement = new Statement.SetConnection { Connection = connection };

					return true;
				}

				break;
			}

			case SqlWord.Disconnect:
			{
				cursor.Take();

				if (cursor.Take(SqlWord.All))
				{
					statement = new Statement.Disconnect { Object = new DisconnectObject.All() };

					return true;
				}

				if (cursor.Take(SqlWord.Current))
				{
					statement = new Statement.Disconnect { Object = new DisconnectObject.Current() };

					return true;
				}

				if (ConnectionObject(ref cursor, out var connection))
				{
					statement = new Statement.Disconnect { Object = new DisconnectObject.Connection(connection) };

					return true;
				}

				break;
			}
		}

		cursor = save;

		return false;
	}

	static bool ConnectionObject(ref SqlCursor cursor, out ConnectionObject connection)
	{
		if (cursor.Take(SqlWord.Default))
		{
			connection = new ConnectionObject.Default();

			return true;
		}

		if (SimpleValueSpecification(ref cursor, out var value))
		{
			connection = new ConnectionObject.Named(value);

			return true;
		}

		connection = null!;

		return false;
	}

	// ── §19 Session management ─────────────────────────────────────────────────

	static bool SQLSessionStatement(ref SqlCursor cursor, out Statement statement)
	{
		var save = cursor;

		statement = null!;

		if (!cursor.Take(SqlWord.Set))
			return false;

		// `SET SESSION AUTHORIZATION`, `SET SESSION CHARACTERISTICS`.
		if (cursor.TakeWord("SESSION"))
		{
			if (cursor.Take(SqlWord.Authorization) && ValueSpecification(ref cursor, out var authorization))
			{
				statement = new Statement.SetSessionAuthorization { Value = authorization };

				return true;
			}

			if (cursor.TakeWord("CHARACTERISTICS") && cursor.Take(SqlWord.As))
			{
				var characteristics = new List<IReadOnlyList<TransactionMode>>();

				while (true)
				{
					if (!cursor.TakeWord("TRANSACTION"))
						break;

					var modes = TransactionModes(ref cursor);

					if (modes.Count == 0)
						break;

					characteristics.Add(modes);

					if (!cursor.Take(SqlTokenKind.Comma))
						break;
				}

				if (characteristics.Count > 0)
				{
					statement = new Statement.SetSessionCharacteristics { Characteristics = characteristics };

					return true;
				}
			}

			cursor = save;

			return false;
		}

		if (cursor.TakeWord("ROLE"))
		{
			if (cursor.Take(SqlWord.None))
			{
				statement = new Statement.SetRole { None = true };

				return true;
			}

			if (ValueSpecification(ref cursor, out var role))
			{
				statement = new Statement.SetRole { Value = role };

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Time))
		{
			if (cursor.TakeWord("ZONE"))
			{
				if (cursor.Take(SqlWord.Local))
				{
					statement = new Statement.SetTimeZone { Local = true };

					return true;
				}

				if (Node(ref cursor, SqlTowers.Interval, out var zone))
				{
					statement = new Statement.SetTimeZone { Value = zone };

					return true;
				}
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Default))
		{
			if (cursor.TakeWord("TRANSFORM") && cursor.Take(SqlWord.Group) && ValueSpecification(ref cursor, out var group))
			{
				statement = new Statement.SetTransformGroup { Value = new TransformGroupCharacteristic(true, null, group) };

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.TakeWord("TRANSFORM"))
		{
			if (cursor.Take(SqlWord.Group) && cursor.Take(SqlWord.For) && cursor.TakeWord("TYPE") && Names(ref cursor, 3, out var type) &&
				ValueSpecification(ref cursor, out var group))
			{
				statement = new Statement.SetTransformGroup { Value = new TransformGroupCharacteristic(false, type, group) };

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.IsWord("COLLATION"))
		{
			cursor.Take();

			if (ValueSpecification(ref cursor, out var collation))
			{
				statement = new Statement.SetCollation { Value = collation, ForCharacterSets = ForCharacterSets(ref cursor) };

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.No))
		{
			if (cursor.IsWord("COLLATION"))
			{
				cursor.Take();

				statement = new Statement.SetCollation { NoCollation = true, ForCharacterSets = ForCharacterSets(ref cursor) };

				return true;
			}

			cursor = save;

			return false;
		}

		// `SET CATALOG`, `SET SCHEMA`, `SET NAMES`, `SET PATH`.
		var catalog = cursor.IsWord("CATALOG");
		var schema  = !catalog && cursor.TakeWord("SCHEMA") ? true : false;
		var names   = !catalog && !schema && cursor.IsWord("NAMES");
		var path    = !catalog && !schema && !names && cursor.TakeWord("PATH");

		if (catalog || names)
			cursor.Take();

		if ((catalog || schema || names || path) && ValueSpecification(ref cursor, out var value))
		{
			statement =
				catalog ? new Statement.SetCatalog { Value = value } :
				schema  ? new Statement.SetSchema { Value = value } :
				names   ? new Statement.SetNames { Value = value } :
				new Statement.SetPath { Value = value };

			return true;
		}

		cursor = save;

		return false;
	}

	static IReadOnlyList<CharacterSetName> ForCharacterSets(ref SqlCursor cursor)
	{
		var save = cursor;
		var sets = new List<CharacterSetName>();

		if (!cursor.Take(SqlWord.For))
			return sets;

		while (true)
		{
			if (!CharacterSetSpecification(ref cursor, out var set))
			{
				cursor = save;

				return [];
			}

			sets.Add(set!);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		return sets;
	}

	/// <summary><c>&lt;value specification&gt;</c>: a literal, a general value specification, or a name.</summary>
	static bool ValueSpecification(ref SqlCursor cursor, out Expression value)
	{
		if (Literal(ref cursor, out var literal))
		{
			value = new Expression.Literal(literal);

			return true;
		}

		if (GeneralValueSpecification(ref cursor, out value))
			return true;

		if (IdentifierChain(ref cursor, out var name))
		{
			value = new Expression.Reference(name);

			return true;
		}

		value = null!;

		return false;
	}

	// ── §23 Diagnostics management ─────────────────────────────────────────────

	static bool SQLDiagnosticsStatement(ref SqlCursor cursor, out Statement.GetDiagnostics statement)
	{
		var save = cursor;

		statement = null!;

		if (!cursor.Take(SqlWord.Get) || !cursor.TakeWord("DIAGNOSTICS"))
		{
			cursor = save;

			return false;
		}

		if (SQLDiagnosticsInformation(ref cursor, out var information))
		{
			statement = new Statement.GetDiagnostics { Information = information };

			return true;
		}

		cursor = save;

		return false;
	}

	static bool SQLDiagnosticsInformation(ref SqlCursor cursor, out DiagnosticsInformation information)
	{
		var save = cursor;

		information = null!;

		// `t = ALL …`, which a statement information item would read as far as its own name.
		if (SimpleTargetSpecification(ref cursor, out var target) && cursor.Take(SqlTokenKind.Equal) && cursor.Take(SqlWord.All))
		{
			AllInformationQualifier? qualifier = null;
			Expression? number = null;

			if (cursor.TakeWord("STATEMENT"))
			{
				qualifier = Ast.AllInformationQualifier.Statement;
			}
			else if (cursor.TakeWord("CONDITION"))
			{
				qualifier = Ast.AllInformationQualifier.Condition;

				var marked = cursor;

				if (!SimpleValueSpecification(ref cursor, out number))
				{
					cursor = marked;
					number = null;
				}
			}

			information = new DiagnosticsInformation.All(target, qualifier, number);

			return true;
		}

		cursor = save;

		if (cursor.TakeWord("CONDITION"))
		{
			if (SimpleValueSpecification(ref cursor, out var condition))
			{
				var items = new List<ConditionInformationItem>();
				var written = cursor;

				while (true)
				{
					if (!SimpleTargetSpecification(ref cursor, out var into) || !cursor.Take(SqlTokenKind.Equal) ||
						!ConditionInformationItemName(ref cursor, out var name))
					{
						cursor = written;

						break;
					}

					items.Add(new ConditionInformationItem(into, name));
				written = cursor;

					if (!cursor.Take(SqlTokenKind.Comma))
						break;
				}

				if (items.Count > 0)
				{
					information = new DiagnosticsInformation.Condition(condition, items);

					return true;
				}
			}

			cursor = save;

			return false;
		}

		var statements = new List<StatementInformationItem>();
		var listed     = cursor;

		while (true)
		{
			if (!SimpleTargetSpecification(ref cursor, out var into) || !cursor.Take(SqlTokenKind.Equal) ||
				!StatementInformationItemName(ref cursor, out var name))
			{
				cursor = listed;

				break;
			}

			statements.Add(new StatementInformationItem(into, name));
		listed = cursor;

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		if (statements.Count > 0)
		{
			information = new DiagnosticsInformation.Statement(statements);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool SimpleTargetSpecification(ref SqlCursor cursor, out Expression target)
	{
		var save = cursor;

		if (cursor.Take(SqlTokenKind.Colon))
		{
			if (Identifier(ref cursor, out var host))
			{
				target = new Expression.Parameter(ParameterKind.Host, host);

				return true;
			}

			cursor = save;
			target = null!;

			return false;
		}

		if (cursor.Word == SqlWord.Module)
		{
			var word = cursor.TextOf(cursor.Token);

			cursor.Take();

			if (cursor.Take(SqlTokenKind.Dot) && Identifier(ref cursor, out var qualified) &&
				cursor.Take(SqlTokenKind.Dot) && Identifier(ref cursor, out var column))
			{
				target = new Expression.Reference(new QualifiedName([new Identifier(word), qualified, column]));

				return true;
			}

			cursor = save;
			target = null!;

			return false;
		}

		if (IdentifierChain(ref cursor, out var name))
		{
			target = new Expression.Reference(name);

			return true;
		}

		target = null!;

		return false;
	}

	static bool StatementInformationItemName(ref SqlCursor cursor, out StatementInformationItemName name)
	{
		return Named(ref cursor, out name,
			"NUMBER", "MORE", "COMMAND_FUNCTION_CODE", "COMMAND_FUNCTION", "DYNAMIC_FUNCTION_CODE", "DYNAMIC_FUNCTION",
			"ROW_COUNT", "TRANSACTIONS_COMMITTED", "TRANSACTIONS_ROLLED_BACK", "TRANSACTION_ACTIVE");
	}

	static bool ConditionInformationItemName(ref SqlCursor cursor, out ConditionInformationItemName name)
	{
		return Named(ref cursor, out name,
			"CATALOG_NAME", "CLASS_ORIGIN", "COLUMN_NAME", "CONDITION_NUMBER", "CONNECTION_NAME",
			"CONSTRAINT_CATALOG", "CONSTRAINT_NAME", "CONSTRAINT_SCHEMA", "CURSOR_NAME",
			"MESSAGE_LENGTH", "MESSAGE_OCTET_LENGTH", "MESSAGE_TEXT", "PARAMETER_MODE", "PARAMETER_NAME",
			"PARAMETER_ORDINAL_POSITION", "RETURNED_SQLSTATE", "ROUTINE_CATALOG", "ROUTINE_NAME", "ROUTINE_SCHEMA",
			"SCHEMA_NAME", "SERVER_NAME", "SPECIFIC_NAME", "SUBCLASS_ORIGIN", "TABLE_NAME",
			"TRIGGER_CATALOG", "TRIGGER_NAME", "TRIGGER_SCHEMA");
	}

	static bool HeaderItemName(ref SqlCursor cursor, out DescriptorItem name)
	{
		return Named(ref cursor, out name, "COUNT", "KEY_TYPE", "DYNAMIC_FUNCTION_CODE", "DYNAMIC_FUNCTION", "TOP_LEVEL_COUNT");
	}

	static bool DescriptorItemName(ref SqlCursor cursor, out DescriptorItem name)
	{
		return Named(ref cursor, out name,
			"CARDINALITY", "CHARACTER_SET_CATALOG", "CHARACTER_SET_NAME", "CHARACTER_SET_SCHEMA",
			"COLLATION_CATALOG", "COLLATION_NAME", "COLLATION_SCHEMA", "DATA", "DATETIME_INTERVAL_CODE",
			"DATETIME_INTERVAL_PRECISION", "DEGREE", "INDICATOR", "KEY_MEMBER", "LENGTH", "LEVEL", "NAME",
			"NULLABLE", "NULL_ORDERING", "OCTET_LENGTH", "PARAMETER_MODE", "PARAMETER_ORDINAL_POSITION",
			"PARAMETER_SPECIFIC_CATALOG", "PARAMETER_SPECIFIC_NAME", "PARAMETER_SPECIFIC_SCHEMA", "PRECISION",
			"RETURNED_CARDINALITY", "RETURNED_LENGTH", "RETURNED_OCTET_LENGTH", "SCALE", "SCOPE_CATALOG",
			"SCOPE_NAME", "SCOPE_SCHEMA", "SORT_DIRECTION", "TYPE", "UNNAMED", "USER_DEFINED_TYPE_CATALOG",
			"USER_DEFINED_TYPE_NAME", "USER_DEFINED_TYPE_SCHEMA", "USER_DEFINED_TYPE_CODE");
	}

	/// <summary>
	/// One of the names the BNF spells out, as the enum member of that name: the underscores are
	/// dropped, which is what turns <c>ROW_COUNT</c> into <c>RowCount</c>.
	/// </summary>
	static bool Named<T>(ref SqlCursor cursor, out T value, params string[] spellings) where T : struct
	{
		value = default;

		if (cursor.Kind != SqlTokenKind.Word)
			return false;

		foreach (var spelling in spellings)
		{
			if (!cursor.IsWord(spelling))
				continue;

			cursor.Take();

			value = Enum.Parse<T>(spelling.Replace("_", ""), true);

			return true;
		}

		return false;
	}

	// ── §20 Dynamic SQL ────────────────────────────────────────────────────────

	static bool SQLDynamicStatement(ref SqlCursor cursor, out Statement statement)
	{
		var save = cursor;

		statement = null!;

		switch (cursor.Word)
		{
			case SqlWord.Allocate:
			{
				if (SQLDescriptorStatement(ref cursor, out statement))
					return true;

				// `ALLOCATE c <cursor properties> FOR …`, and a received cursor's.
				if (AllocateCursorStatement(ref cursor, out var allocated))
				{
					statement = allocated;

					return true;
				}

				break;
			}

			case SqlWord.Deallocate:
			{
				if (SQLDescriptorStatement(ref cursor, out statement))
					return true;

				cursor.Take();

				if (cursor.Take(SqlWord.Prepare) && SQLStatementName(ref cursor, out var prepared))
				{
					statement = new Statement.DeallocatePrepare { Statement = prepared };

					return true;
				}

				break;
			}

			case SqlWord.Get:
			case SqlWord.Set:
			{
				if (SQLDescriptorStatement(ref cursor, out statement))
					return true;

				break;
			}

			case SqlWord.Prepare:
			{
				cursor.Take();

				if (SQLStatementName(ref cursor, out var name))
				{
					Expression? attributes = null;

					var marked = cursor;

					if (cursor.TakeWord("ATTRIBUTES") && !SimpleValueSpecification(ref cursor, out attributes))
					{
						cursor     = marked;
						attributes = null;
					}

					if (cursor.Take(SqlWord.From) && SimpleValueSpecification(ref cursor, out var sql))
					{
						statement = new Statement.Prepare { Statement = name, Attributes = attributes, Sql = sql };

						return true;
					}
				}

				break;
			}

			case SqlWord.Describe:
			{
				if (DescribeStatement(ref cursor, out var described))
				{
					statement = described;

					return true;
				}

				break;
			}

			case SqlWord.Execute:
			{
				cursor.Take();

				// `EXECUTE IMMEDIATE`, asked first: a statement may be named `IMMEDIATE` as readily.
				var immediate = cursor;

				if (cursor.TakeWord("IMMEDIATE") && SimpleValueSpecification(ref cursor, out var sql))
				{
					statement = new Statement.ExecuteImmediate { Sql = sql };

					return true;
				}

				cursor = immediate;

				if (SQLStatementName(ref cursor, out var name))
				{
					var result     = OutputUsingClause(ref cursor);
					var parameters = InputUsingClause(ref cursor);

					statement = new Statement.Execute { Statement = name, Result = result, Parameters = parameters };

					return true;
				}

				break;
			}

			case SqlWord.Copy:
			{
				if (CopyDescriptorStatement(ref cursor, out var copied))
				{
					statement = copied;

					return true;
				}

				break;
			}

			case SqlWord.Name when cursor.IsWord("PIPE"):
			{
				cursor.Take();

				if (cursor.Take(SqlWord.Row) && cursor.TakeWord("PTF") && SimpleValueSpecification(ref cursor, out var descriptor))
				{
					statement = new Statement.PipeRow { Descriptor = DescriptorNameOf(null, descriptor, true) };

					return true;
				}

				break;
			}
		}

		cursor = save;

		// The dynamic cursor statements, which begin with the words their static fellows do.
		return SQLDynamicDataStatement(ref cursor, out statement);
	}

	static bool SQLDescriptorStatement(ref SqlCursor cursor, out Statement statement)
	{
		var save = cursor;

		statement = null!;

		if (cursor.Take(SqlWord.Allocate))
		{
			var sql = cursor.Take(SqlWord.Sql);

			if (cursor.TakeWord("DESCRIPTOR") && ConventionalDescriptorName(ref cursor, out var descriptor))
			{
				Expression? max = null;

				var marked = cursor;

				if (cursor.Take(SqlWord.With))
				{
					if (!cursor.TakeWord("MAX") || !SimpleValueSpecification(ref cursor, out max))
					{
						cursor = marked;
						max    = null;
					}
				}

				statement = new Statement.AllocateDescriptor { SqlKeyword = sql, Descriptor = descriptor, Max = max };

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Deallocate))
		{
			var sql = cursor.Take(SqlWord.Sql);

			if (cursor.TakeWord("DESCRIPTOR") && ConventionalDescriptorName(ref cursor, out var descriptor))
			{
				statement = new Statement.DeallocateDescriptor { SqlKeyword = sql, Descriptor = descriptor };

				return true;
			}

			cursor = save;

			return false;
		}

		var get = cursor.Take(SqlWord.Get);

		if (get || cursor.Take(SqlWord.Set))
		{
			var sql = cursor.Take(SqlWord.Sql);

			if (cursor.TakeWord("DESCRIPTOR") && DescriptorName(ref cursor, out var descriptor))
			{
				if (get)
				{
					if (GetDescriptorInformation(ref cursor, out var read))
					{
						statement = new Statement.GetDescriptor { SqlKeyword = sql, Descriptor = descriptor, Body = read };

						return true;
					}
				}
				else if (SetDescriptorInformation(ref cursor, out var written))
				{
					statement = new Statement.SetDescriptor { SqlKeyword = sql, Descriptor = descriptor, Body = written };

					return true;
				}
			}
		}

		cursor = save;

		return false;
	}

	static bool GetDescriptorInformation(ref SqlCursor cursor, out DescriptorGet information)
	{
		var save = cursor;

		information = null!;

		if (cursor.Take(SqlWord.Value))
		{
			if (SimpleValueSpecification(ref cursor, out var item))
			{
				var reads  = new List<DescriptorRead>();
				var written = cursor;

				while (true)
				{
					if (!SimpleTargetSpecification(ref cursor, out var target) || !cursor.Take(SqlTokenKind.Equal) ||
						!DescriptorItemName(ref cursor, out var name))
					{
						cursor = written;

						break;
					}

					reads.Add(new DescriptorRead(target, name));
				written = cursor;

					if (!cursor.Take(SqlTokenKind.Comma))
						break;
				}

				if (reads.Count > 0)
				{
					information = new DescriptorGet.Value(item, reads);

					return true;
				}
			}

			cursor = save;

			return false;
		}

		var header = new List<DescriptorRead>();
		var listed = cursor;

		while (true)
		{
			if (!SimpleTargetSpecification(ref cursor, out var target) || !cursor.Take(SqlTokenKind.Equal) ||
				!HeaderItemName(ref cursor, out var name))
			{
				cursor = listed;

				break;
			}

			header.Add(new DescriptorRead(target, name));
		listed = cursor;

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		if (header.Count > 0)
		{
			information = new DescriptorGet.Header(header);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool SetDescriptorInformation(ref SqlCursor cursor, out DescriptorSet information)
	{
		var save = cursor;

		information = null!;

		if (cursor.Take(SqlWord.Value))
		{
			if (SimpleValueSpecification(ref cursor, out var item))
			{
				var writes = new List<DescriptorWrite>();
				var written = cursor;

				while (true)
				{
					if (!DescriptorItemName(ref cursor, out var name) || !cursor.Take(SqlTokenKind.Equal) ||
						!SimpleValueSpecification(ref cursor, out var value))
					{
						cursor = written;

						break;
					}

					writes.Add(new DescriptorWrite(name, value));
				written = cursor;

					if (!cursor.Take(SqlTokenKind.Comma))
						break;
				}

				if (writes.Count > 0)
				{
					information = new DescriptorSet.Value(item, writes);

					return true;
				}
			}

			cursor = save;

			return false;
		}

		var header = new List<DescriptorWrite>();
		var listed = cursor;

		while (true)
		{
			if (!HeaderItemName(ref cursor, out var name) || !cursor.Take(SqlTokenKind.Equal) ||
				!SimpleValueSpecification(ref cursor, out var value))
			{
				cursor = listed;

				break;
			}

			header.Add(new DescriptorWrite(name, value));
		listed = cursor;

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		if (header.Count > 0)
		{
			information = new DescriptorSet.Header(header);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool CopyDescriptorStatement(ref SqlCursor cursor, out Statement.CopyDescriptor statement)
	{
		var save = cursor;

		statement = null!;

		cursor.Take();

		if (!DescriptorName(ref cursor, out var source))
		{
			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.To))
		{
			if (cursor.TakeWord("PTF") && SimpleValueSpecification(ref cursor, out var target))
			{
				statement = new Statement.CopyDescriptor { Body = new DescriptorCopy.Whole(source, DescriptorNameOf(null, target, true)) };

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Value) && SimpleValueSpecification(ref cursor, out var index) && cursor.Take(SqlTokenKind.LeftParen))
		{
			var options = CopyOptions(ref cursor);

			if (options is not null && cursor.Take(SqlTokenKind.RightParen) && cursor.Take(SqlWord.To) && cursor.TakeWord("PTF") &&
				SimpleValueSpecification(ref cursor, out var target) && cursor.Take(SqlWord.Value) &&
				SimpleValueSpecification(ref cursor, out var targetIndex))
			{
				statement = new Statement.CopyDescriptor
				{
					Body = new DescriptorCopy.Item(source, index, options, DescriptorNameOf(null, target, true), targetIndex),
				};

				return true;
			}
		}

		cursor = save;

		return false;
	}

	static IReadOnlyList<DescriptorCopyOption>? CopyOptions(ref SqlCursor cursor)
	{
		if (cursor.TakeWord("NAME"))
		{
			var save = cursor;

			if (cursor.Take(SqlTokenKind.Comma))
			{
				if (cursor.TakeWord("TYPE"))
					return [DescriptorCopyOption.Name, DescriptorCopyOption.Type];

				cursor = save;
			}

			return [DescriptorCopyOption.Name];
		}

		if (cursor.TakeWord("TYPE"))
			return [DescriptorCopyOption.Type];

		if (cursor.TakeWord("DATA"))
			return [DescriptorCopyOption.Data];

		return null;
	}

	static bool DescribeStatement(ref SqlCursor cursor, out Statement.Describe statement)
	{
		var save = cursor;

		statement = null!;

		cursor.Take();

		// `INPUT` and `OUTPUT` are no reserved words, and a statement may be named so.
		var input = cursor;

		if (cursor.TakeWord("INPUT") && SQLStatementName(ref cursor, out var named) && UsingDescriptor(ref cursor, out var sql, out var descriptor))
		{
			statement = new Statement.Describe { Body = new DescribeBody.Input(named, descriptor, NestingOption(ref cursor), sql) };

			return true;
		}

		cursor = input;

		var output = cursor.TakeWord("OUTPUT");

		if (DescribedObject(ref cursor, out var described) && UsingDescriptor(ref cursor, out var keyword, out var into))
		{
			statement = new Statement.Describe { Body = new DescribeBody.Output(described, into, NestingOption(ref cursor), output, keyword) };

			return true;
		}

		cursor = save;

		return false;
	}

	static bool DescribedObject(ref SqlCursor cursor, out DescribeObject described)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Cursor))
		{
			if (CursorName(ref cursor, out var name) && cursor.TakeWord("STRUCTURE"))
			{
				described = new DescribeObject.Cursor(new CursorReference(name));

				return true;
			}

			cursor = save;
			described = null!;

			return false;
		}

		if (SQLStatementName(ref cursor, out var statement))
		{
			described = new DescribeObject.Statement(statement);

			return true;
		}

		described = null!;

		return false;
	}

	static bool UsingDescriptor(ref SqlCursor cursor, out bool sql, out DescriptorReference descriptor)
	{
		var save = cursor;

		sql        = false;
		descriptor = null!;

		if (!cursor.Take(SqlWord.Using))
			return false;

		sql = cursor.Take(SqlWord.Sql);

		if (cursor.TakeWord("DESCRIPTOR") && DescriptorName(ref cursor, out descriptor))
			return true;

		cursor = save;
		sql    = false;

		return false;
	}

	static bool? NestingOption(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.With))
		{
			if (cursor.TakeWord("NESTING"))
				return true;

			cursor = save;

			return null;
		}

		if (cursor.Take(SqlWord.Without))
		{
			if (cursor.TakeWord("NESTING"))
				return false;

			cursor = save;
		}

		return null;
	}

	static DynamicArguments? InputUsingClause(ref SqlCursor cursor)
	{
		return UsingArguments(ref cursor, true);
	}

	static DynamicArguments? OutputUsingClause(ref SqlCursor cursor)
	{
		return UsingArguments(ref cursor, false);
	}

	/// <summary>
	/// <c>&lt;input using clause&gt;</c> and <c>&lt;output using clause&gt;</c>: arguments, or a
	/// descriptor. The one says <c>USING</c> and the other <c>INTO</c>.
	/// </summary>
	static DynamicArguments? UsingArguments(ref SqlCursor cursor, bool input)
	{
		var save = cursor;

		if (!(input ? cursor.Take(SqlWord.Using) : cursor.Take(SqlWord.Into)))
			return null;

		var keyword = cursor;
		var sql     = cursor.Take(SqlWord.Sql);

		if (cursor.TakeWord("DESCRIPTOR") && DescriptorName(ref cursor, out var descriptor))
			return new DynamicArguments.Descriptor(descriptor, sql);

		// `USING DESCRIPTOR` with no name is no descriptor: `DESCRIPTOR` is a column's name there,
		// and §5.2 does not reserve it.
		cursor = keyword;

		var values = new List<Expression>();

		while (true)
		{
			var read = input
				? UsingArgument(ref cursor, out var argument) ? argument : null
				: TargetSpecification(ref cursor, out var target) ? target : null;

			if (read is null)
				break;

			values.Add(read);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		if (values.Count > 0)
			return new DynamicArguments.Values(values);

		cursor = save;

		return null;
	}

	static bool UsingArgument(ref SqlCursor cursor, out Expression value)
	{
		if (GeneralValueSpecification(ref cursor, out value))
			return true;

		if (IdentifierChain(ref cursor, out var name))
		{
			value = new Expression.Reference(name);

			return true;
		}

		value = null!;

		return false;
	}

	// ── §20.12–20.23 Dynamic cursors ───────────────────────────────────────────

	static bool SQLDynamicDataStatement(ref SqlCursor cursor, out Statement statement)
	{
		var save = cursor;

		statement = null!;

		switch (cursor.Word)
		{
			case SqlWord.Open:
			{
				cursor.Take();

				if (ConventionalDynamicCursorName(ref cursor, out var name))
				{
					statement = new Statement.OpenCursor { Cursor = name, Using = InputUsingClause(ref cursor) };

					return true;
				}

				break;
			}

			case SqlWord.Close:
			{
				cursor.Take();

				if (ConventionalDynamicCursorName(ref cursor, out var name))
				{
					statement = new Statement.CloseCursor { Cursor = name };

					return true;
				}

				break;
			}

			case SqlWord.Fetch:
			{
				cursor.Take();

				var orientation = FetchOrientation(ref cursor);
				var from        = cursor.Take(SqlWord.From);

				if (!from && orientation is not null)
				{
					cursor      = save;
					orientation = null;

					cursor.Take();
				}

				if (DynamicCursorName(ref cursor, out var name) && OutputUsingClause(ref cursor) is { } into)
				{
					statement = new Statement.FetchCursor
					{
						Orientation = orientation,
						FromKeyword = from,
						Cursor      = name,
						Into        = into,
					};

					return true;
				}

				break;
			}

			case SqlWord.Delete:
			{
				cursor.Take();

				if (cursor.Take(SqlWord.From) && TargetTable(ref cursor, out var target) && cursor.Take(SqlWord.Where) &&
					cursor.Take(SqlWord.Current) && cursor.Take(SqlWord.Of) && ConventionalDynamicCursorName(ref cursor, out var name))
				{
					statement = new Statement.Delete { Target = target, CurrentOf = name };

					return true;
				}

				break;
			}

			case SqlWord.Update:
			{
				cursor.Take();

				if (TargetTable(ref cursor, out var target) && cursor.Take(SqlWord.Set) && SetClauseList(ref cursor, out var assignments) &&
					cursor.Take(SqlWord.Where) && cursor.Take(SqlWord.Current) && cursor.Take(SqlWord.Of) &&
					ConventionalDynamicCursorName(ref cursor, out var name))
				{
					statement = new Statement.Update { Target = target, Assignments = assignments, CurrentOf = name };

					return true;
				}

				break;
			}
		}

		cursor = save;

		return false;
	}

	static bool AllocateCursorStatement(ref SqlCursor cursor, out Statement.AllocateCursor statement)
	{
		var save = cursor;

		statement = null!;

		cursor.Take();

		// `ALLOCATE [GLOBAL|LOCAL] v <cursor properties> FOR [GLOBAL|LOCAL] w`.
		var scope = ScopeOption(ref cursor);

		if (SimpleValueSpecification(ref cursor, out var value) && CursorProperties(ref cursor, out var properties) && cursor.Take(SqlWord.For))
		{
			var source = ScopeOption(ref cursor);
			var routine = cursor;
			var designated = SpecificRoutineDesignator(ref routine, out _);

			if (!designated && SimpleValueSpecification(ref cursor, out var prepared))
			{
				statement = new Statement.AllocateCursor
				{
					Cursor      = CursorNameOf(scope, value, false),
					Properties  = properties,
					SourceValue = new CursorAllocationSource.Prepared(StatementNameOf(source, prepared)),
				};

				return true;
			}
		}

		cursor = save;
		cursor.Take();

		// `ALLOCATE c [CURSOR] FOR <specific routine designator>`.
		if (CursorName(ref cursor, out var name))
		{
			var keyword = cursor.Take(SqlWord.Cursor);

			if (cursor.Take(SqlWord.For) && SpecificRoutineDesignator(ref cursor, out var routine))
			{
				statement = new Statement.AllocateCursor
				{
					Cursor        = new CursorReference(name),
					CursorKeyword = keyword,
					SourceValue   = new CursorAllocationSource.Routine(routine),
				};

				return true;
			}
		}

		cursor = save;

		return false;
	}

	static bool CursorProperties(ref SqlCursor cursor, out CursorProperties properties)
	{
		var save = cursor;

		properties = null!;

		var sensitivity =
			cursor.TakeWord("SENSITIVE")   ? CursorSensitivity.Sensitive :
			cursor.TakeWord("INSENSITIVE") ? CursorSensitivity.Insensitive :
			cursor.Take(SqlWord.Asensitive) ? CursorSensitivity.Asensitive :
			(CursorSensitivity?)null;

		CursorScrollability? scrollability = null;

		if (cursor.Take(SqlWord.Scroll))
		{
			scrollability = CursorScrollability.Scroll;
		}
		else
		{
			var marked = cursor;

			if (cursor.Take(SqlWord.No) && cursor.Take(SqlWord.Scroll))
				scrollability = CursorScrollability.NoScroll;
			else
				cursor = marked;
		}

		if (!cursor.Take(SqlWord.Cursor))
		{
			cursor = save;

			return false;
		}

		CursorHoldability? holdability = null;
		CursorReturnability? returnability = null;

		var held = cursor;

		if (cursor.Take(SqlWord.With) && cursor.Take(SqlWord.Hold))
			holdability = CursorHoldability.WithHold;
		else
		{
			cursor = held;

			if (cursor.Take(SqlWord.Without) && cursor.Take(SqlWord.Hold))
				holdability = CursorHoldability.WithoutHold;
			else
				cursor = held;
		}

		var returned = cursor;

		if (cursor.Take(SqlWord.With) && cursor.Take(SqlWord.Return))
			returnability = CursorReturnability.WithReturn;
		else
		{
			cursor = returned;

			if (cursor.Take(SqlWord.Without) && cursor.Take(SqlWord.Return))
				returnability = CursorReturnability.WithoutReturn;
			else
				cursor = returned;
		}

		properties = new Ast.CursorProperties(sensitivity, scrollability, holdability, returnability);

		return true;
	}

	// ── The names a dynamic statement gives a statement, a descriptor or a cursor ──

	static string? ScopeOption(ref SqlCursor cursor)
	{
		return cursor.Take(SqlWord.Global) ? "GLOBAL" :
		cursor.Take(SqlWord.Local) ? "LOCAL" :
		null;
	}

	static bool SQLStatementName(ref SqlCursor cursor, out StatementReference name)
	{
		var save  = cursor;
		var scope = ScopeOption(ref cursor);

		if (SimpleValueSpecification(ref cursor, out var value))
		{
			name = StatementNameOf(scope, value);

			return true;
		}

		cursor = save;
		name   = null!;

		return false;
	}

	static bool DescriptorName(ref SqlCursor cursor, out DescriptorReference name)
	{
		var save = cursor;

		if (cursor.TakeWord("PTF"))
		{
			if (SimpleValueSpecification(ref cursor, out var value))
			{
				name = DescriptorNameOf(null, value, true);

				return true;
			}

			cursor = save;
			name   = null!;

			return false;
		}

		return ConventionalDescriptorName(ref cursor, out name);
	}

	static bool ConventionalDescriptorName(ref SqlCursor cursor, out DescriptorReference name)
	{
		var save  = cursor;
		var scope = ScopeOption(ref cursor);

		if (SimpleValueSpecification(ref cursor, out var value))
		{
			name = DescriptorNameOf(scope, value, false);

			return true;
		}

		cursor = save;
		name   = null!;

		return false;
	}

	static bool ConventionalDynamicCursorName(ref SqlCursor cursor, out CursorReference name)
	{
		var save = cursor;

		if (CursorName(ref cursor, out var written))
		{
			name = new CursorReference(written);

			return true;
		}

		cursor = save;

		var scope = ScopeOption(ref cursor);

		if (SimpleValueSpecification(ref cursor, out var value))
		{
			name = CursorNameOf(scope, value, false);

			return true;
		}

		cursor = save;
		name   = null!;

		return false;
	}

	static bool DynamicCursorName(ref SqlCursor cursor, out CursorReference name)
	{
		var save = cursor;

		if (cursor.TakeWord("PTF"))
		{
			if (SimpleValueSpecification(ref cursor, out var value))
			{
				name = CursorNameOf(null, value, true);

				return true;
			}

			cursor = save;
			name   = null!;

			return false;
		}

		return ConventionalDynamicCursorName(ref cursor, out name);
	}

	/// <summary>
	/// The identifier a statement, a descriptor or a cursor is named by, where it was named by one
	/// alone; with a scope before it, or a value of another kind, the name is an extended one.
	/// </summary>
	static Identifier? Alone(string? scope, Expression value)
	{
		return scope is null && value is Expression.Reference { Name.Parts.Count: 1 } reference ? reference.Name.Parts[0] : null;
	}

	static StatementReference StatementNameOf(string? scope, Expression value)
	{
		return Alone(scope, value) is { } name
			? new StatementReference(name)
			: new StatementReference(null, value, scope == "GLOBAL", scope == "LOCAL");
	}

	static DescriptorReference DescriptorNameOf(string? scope, Expression value, bool ptf)
	{
		return Alone(scope, value) is { } name
			? new DescriptorReference(name, null, ptf)
			: new DescriptorReference(null, value, ptf, scope == "GLOBAL", scope == "LOCAL");
	}

	static CursorReference CursorNameOf(string? scope, Expression value, bool ptf)
	{
		return Alone(scope, value) is { } name
			? new CursorReference(new QualifiedName([name]), null, ptf)
			: new CursorReference(null, value, ptf, scope == "GLOBAL", scope == "LOCAL");
	}

	// ── What a routine's body and a trigger's action are ───────────────────────

	/// <summary>
	/// <c>&lt;SQL executable statement&gt;</c>. A dynamic statement is asked before a data statement:
	/// a searched <c>UPDATE</c> or <c>DELETE</c> reads what comes before a dynamic cursor's
	/// <c>WHERE CURRENT OF GLOBAL :c</c> and ends there.
	/// </summary>
	static bool SQLExecutableStatement(ref SqlCursor cursor, out Statement statement)
	{
		return SQLSchemaStatement(ref cursor, out statement) ||
		SQLDynamicStatement(ref cursor, out statement) ||
		SQLDataStatement(ref cursor, out statement) ||
		SQLControlStatement(ref cursor, out statement) ||
		SQLTransactionStatement(ref cursor, out statement) ||
		SQLConnectionStatement(ref cursor, out statement) ||
		SQLSessionStatement(ref cursor, out statement) ||
		SQLDiagnosticsStatementOf(ref cursor, out statement);
	}

	static bool SQLDiagnosticsStatementOf(ref SqlCursor cursor, out Statement statement)
	{
		if (SQLDiagnosticsStatement(ref cursor, out var read))
		{
			statement = read;

			return true;
		}

		statement = null!;

		return false;
	}

	static bool SQLProcedureStatement(ref SqlCursor cursor, out Statement statement)
	{
		return SQLExecutableStatement(ref cursor, out statement);
	}
}
