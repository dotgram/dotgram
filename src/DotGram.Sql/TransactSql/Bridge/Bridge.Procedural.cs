using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

static partial class Bridge
{
	/// <summary>
	/// The flow of a batch, transactions, cursors, variables and the session's <c>SET</c>.
	/// </summary>
	internal static class Procedural
	{
		public static readonly string[] Owns =
		[
			"Statement.Compound", "Statement.If", "Statement.While", "Statement.TryCatch", "Statement.Declare",
			"Statement.Return", "Statement.Print", "Statement.Throw", "Statement.GoTo", "Statement.Break",
			"Statement.Continue", "Statement.Label", "Statement.RaiseError", "Statement.WaitFor",
			"Statement.Transaction", "Statement.DeclareCursor", "Statement.SetCursor",
			"Statement.CursorAction", "Statement.Fetch", "Statement.SetStatement", "Statement.SetVariable",
			"Statement.Use", "Statement.ExecuteAs", "Statement.Revert", "Statement.SetUser", "Statement.LineNumber",
			"Clause.VariableDeclaration", "Clause.CursorDefinition", "Clause.CursorFor",
			.. typeof(SetExpression).GetNestedTypes()
				.SelectMany(static group => group.GetNestedTypes())
				.Where(static one => one.IsSubclassOf(typeof(SetExpression)) && !one.IsAbstract)
				.Select(NameOf),
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				_ when Owned(Owns, old) => throw Unconverted(old),
				_                       => null,
			};

			return made is not null;
		}

		/// <summary>
		/// A statement of the family on the common tree, from the T-SQL tree's record its rule read it into: the record is
		/// what the conversion reads, and no statement of any tree.
		/// </summary>
		internal static Ast.Statement Converted(Statement old)
		{
			return old switch
			{
				Statement.Return(var value)  => new Ast.Statement.Return { Value = value is null ? null : Values.Of(value) },
				Statement.Compound compound  => Compound(compound),
				Statement.If(var condition, var then, var other) =>
					new Ast.Statement.If(Values.Of(condition), StatementOf(then), other is null ? null : StatementOf(other)),
				Statement.While(var condition, var body) => new Ast.Statement.While(Values.Of(condition), StatementOf(body)),
				Statement.TryCatch(var tried, var caught) => new Ast.Statement.TryCatch(Each(tried, StatementOf), Each(caught, StatementOf)),
				Statement.Break              => new Ast.Statement.Break(),
				Statement.Continue           => new Ast.Statement.Continue(),
				Statement.GoTo(Expression.ColumnReference(var label)) => new Ast.Statement.GoTo(Values.Part(old, label)),
				Statement.Label(var label)   => new Ast.Statement.Label(Values.Part(old, label)),
				Statement.Declare declare    => new Ast.Statement.DeclareVariables(Each(declare.Variables, Declaration)),
				Statement.SetVariable set    => SetVariable(set),
				Statement.Print(var value)   => new Ast.Statement.Print(Values.Of(value)),
				Statement.RaiseError raise   => RaiseError(raise),
				Statement.Throw thrown       => Throw(thrown),
				Statement.WaitFor wait       => WaitFor(wait),
				Statement.ExecuteAs who      => ExecuteAs(who),
				Statement.Revert(var cookie) => new Ast.Statement.Revert(cookie is null ? null : Values.Of(cookie)),
				Statement.SetUser(var user, var noReset) => new Ast.Statement.SetUser(user is null ? null : Values.Of(user)) { NoReset = noReset },
				Statement.Use(Expression.ColumnReference(var database)) => new Ast.Statement.Use(Values.Part(old, database)),
				Statement.LineNumber(var line) => Values.Of(line) is Ast.LiteralValue.Numeric number
					? new Ast.Statement.LineNumber(number)
					: throw BridgeGap.Open(old, "inventory Statement.LineNumber (a line that is not a number)"),
				Statement.Transaction ended  => Transaction(ended),
				Statement.DeclareCursor(var name, var cursor) declared => DeclareCursor(declared, name, cursor),
				Statement.SetCursor(var name, var cursor) set => SetCursor(set, name, cursor),
				Statement.CursorAction acted => Cursor(acted),
				Statement.Fetch fetch        => Fetch(fetch),
				Statement.SetStatement set   => Set(set),
				Statement.Checkpoint(var duration) => new Ast.Statement.Checkpoint(duration is null ? null : Values.Of(duration)),
				_ => throw new ArgumentException($"The procedural family builds no {NameOf(old)}.", nameof(old)),
			};
		}

		/// <summary>A statement of the family, on the common tree, from the record its rule read it into.</summary>
		internal static Statement Built(Statement read)
		{
			return Carry(read, Converted);
		}

		/// <summary>The same, of a statement whose conversion reads the text it was written in, where the rule says.</summary>
		internal static Statement Built(Statement read, SqlSpan at)
		{
			return Carry(read, at, Converted);
		}

		/// <summary>BEGIN TRY … END TRY BEGIN CATCH … END CATCH, on the common tree.</summary>
		internal static Statement TryCatch(SqlList<Statement> tried, SqlList<Statement> caught)
		{
			return Built(new Statement.TryCatch(tried, caught));
		}

		/// <summary>A block, BEGIN … END, or a natively compiled module's atomic one, on the common tree.</summary>
		internal static Statement Compound(SqlList<Statement> statements, SqlList<Clause>? atomic = null)
		{
			return Built(new Statement.Compound(statements, atomic));
		}

		/// <summary>IF and its branches, on the common tree.</summary>
		internal static Statement If(Expression condition, Statement then, Statement? other)
		{
			return Built(new Statement.If(condition, then, other));
		}

		/// <summary>WHILE and its body, on the common tree.</summary>
		internal static Statement While(Expression condition, Statement body)
		{
			return Built(new Statement.While(condition, body));
		}

		/// <summary>BEGIN, COMMIT, ROLLBACK and SAVE of a transaction, on the common tree.</summary>
		internal static Statement Transaction(string kind, string? name, string? word = null, string? tail = null)
		{
			return Built(new Statement.Transaction(kind, name, word, tail));
		}

		/// <summary>DECLARE of a cursor, on the common tree.</summary>
		internal static Statement DeclareCursor(string name, Clause.CursorDefinition cursor)
		{
			return Built(new Statement.DeclareCursor(name, cursor));
		}

		/// <summary>DECLARE of variables, on the common tree.</summary>
		internal static Statement Declare(SqlList<Clause> variables)
		{
			return Built(new Statement.Declare(variables));
		}

		/// <summary>FETCH, on the common tree.</summary>
		internal static Statement Fetch(string? orientation, Expression? offset, bool from, Expression cursor, bool global, SqlList<Expression>? into)
		{
			return Built(new Statement.Fetch(orientation, offset, from, cursor, global, into));
		}

		/// <summary>SET of a cursor variable, on the common tree.</summary>
		internal static Statement SetCursor(string name, Clause.CursorDefinition cursor)
		{
			return Built(new Statement.SetCursor(name, cursor));
		}

		/// <summary>SET of a variable, on the common tree.</summary>
		internal static Statement SetVariable(string name, Expression value, string? @operator = null, string? through = null)
		{
			return Built(new Statement.SetVariable(name, value, @operator, through));
		}

		/// <summary>SET of the session's options, where the rule says it was written, on the common tree.</summary>
		internal static Statement SetStatement(SqlList<SetExpression> items, SqlSpan at)
		{
			return Built(new Statement.SetStatement(items), at);
		}

		/// <summary>REVERT, on the common tree.</summary>
		internal static Statement Revert(Expression? cookie = null)
		{
			return Built(new Statement.Revert(cookie));
		}

		/// <summary>SETUSER, on the common tree.</summary>
		internal static Statement SetUser(Expression? name = null, bool noReset = false)
		{
			return Built(new Statement.SetUser(name, noReset));
		}

		/// <summary>A label, on the common tree.</summary>
		internal static Statement Label(string name)
		{
			return Built(new Statement.Label(name));
		}

		/// <summary>LINENO, on the common tree.</summary>
		internal static Statement LineNumber(Expression line)
		{
			return Built(new Statement.LineNumber(line));
		}

		/// <summary>RAISERROR, where the rule says it was written, on the common tree.</summary>
		internal static Statement RaiseError(SqlList<Expression> arguments, string? tail, SqlSpan at)
		{
			return Built(new Statement.RaiseError(arguments, tail), at);
		}

		/// <summary>WAITFOR DELAY and TIME, on the common tree.</summary>
		internal static Statement WaitFor(Expression value, string kind)
		{
			return Built(new Statement.WaitFor(value, kind));
		}

		/// <summary>One of the statements that are a word and a value, on the common tree (<c>Statement.Commanded</c>).</summary>
		internal static Statement Commanded(string word, Expression? value)
		{
			return Built(Statement.Commanded(word, value));
		}

		/// <summary>The same, where the values arrived as a list that may not be there.</summary>
		internal static Statement Commanded(string word, SqlList<Expression>? values)
		{
			return Built(Statement.Commanded(word, values));
		}

		/// <summary>A cursor opened, closed or deallocated, on the common tree.</summary>
		internal static Statement Cursor(Statement.CursorAction cursor, string verb)
		{
			return Built(cursor with { Verb = verb });
		}

		/// <summary>EXECUTE AS, on the common tree: whom, and what follows it.</summary>
		internal static Statement ExecuteAs(Statement.ExecuteAs who, Statement.ExecuteAs? with, SqlSpan at)
		{
			return Built(with is null ? who : who with { NoRevert = with.NoRevert, Cookie = with.Cookie }, at);
		}

		/// <summary>
		/// <c>COMMIT</c> and <c>ROLLBACK</c>, whose T-SQL parts the new tree holds (decided 2026-09-27): the word
		/// after the verb, the name, and a commit's delayed durability.
		/// </summary>
		static Ast.Statement Transaction(Statement.Transaction old)
		{
			var kind = string.Join(' ', Words(old.Kind));
			var name = TransactionName(old);

			if (kind is "BEGIN" or "BEGIN DISTRIBUTED" or "SAVE")
			{
				var tran = old.Word?.ToUpperInvariant() switch
				{
					"TRAN"        => true,
					"TRANSACTION" => false,
					_             => throw BridgeGap.Open(old, $"inventory Statement.Transaction (the word `{old.Word}`)"),
				};

				if (kind == "SAVE")
					return name is not null && old.Tail is null
						? new Ast.Statement.SaveTransaction(name, tran)
						: throw BridgeGap.Open(old, "inventory Statement.Transaction (SAVE without a name, or with a tail)");

				return new Ast.Statement.BeginTransaction(kind == "BEGIN DISTRIBUTED", tran, name, Mark(old));
			}

			var word = old.Word?.ToUpperInvariant();

			Ast.TransactionKeyword? keyword = word switch
			{
				"TRAN"        => Ast.TransactionKeyword.Tran,
				"TRANSACTION" => Ast.TransactionKeyword.Transaction,
				_             => null,
			};

			var work = word == "WORK";

			if (word is not null && keyword is null && !work)
				throw BridgeGap.Open(old, $"inventory Statement.Transaction (the word `{old.Word}`)");

			switch (kind)
			{
				case "COMMIT":
					return new Ast.Statement.Commit { Keyword = keyword, Work = work, Name = name, DelayedDurability = Durability(old) };

				case "ROLLBACK":
					if (old.Tail is not null)
						throw BridgeGap.Open(old, $"inventory Statement.Transaction (ROLLBACK … {old.Tail})");

					return new Ast.Statement.Rollback { Keyword = keyword, Work = work, Name = name };

				default:
					throw BridgeGap.Open(old, $"inventory Statement.Transaction (`{old.Kind}`)");
			}
		}

		static bool? Durability(Statement.Transaction old)
		{
			return old.Tail is null ? null : Words(old.Tail) switch
			{
				["WITH", "(", "DELAYED_DURABILITY", "=", "ON", ")"]  => true,
				["WITH", "(", "DELAYED_DURABILITY", "=", "OFF", ")"] => false,
				_ => throw BridgeGap.Open(old, $"inventory Statement.Transaction (COMMIT … {old.Tail})"),
			};
		}

		/// <summary>
		/// <c>SET TRANSACTION ISOLATION LEVEL</c>, the one <c>SET</c> the standard has a statement for. The old
		/// tree does not keep <c>TRAN</c> against <c>TRANSACTION</c>, and the text says which.
		/// </summary>
		static Ast.Statement Set(Statement.SetStatement old)
		{
			if (old.Items is not [SetExpression.Transactions.IsolationLevel(var level)])
				return new Ast.Statement.SetOptions(Each(old.Items, one => SetOption(old, one)));

			Ast.IsolationLevel isolation = string.Join(' ', Words(level)) switch
			{
				"READ UNCOMMITTED" => Ast.IsolationLevel.ReadUncommitted,
				"READ COMMITTED"   => Ast.IsolationLevel.ReadCommitted,
				"REPEATABLE READ"  => Ast.IsolationLevel.RepeatableRead,
				"SERIALIZABLE"     => Ast.IsolationLevel.Serializable,
				"SNAPSHOT"         => Ast.IsolationLevel.Snapshot,
				_                  => throw BridgeGap.Open(old, $"inventory SetExpression.Transactions.IsolationLevel (`{level}`)"),
			};

			var tran = Recover("Statement.SetTransaction.Tran", Spelled(old), static text => Words(text) is [_, "TRAN", ..], false);

			return new Ast.Statement.SetTransaction { Modes = [new Ast.TransactionMode.Isolation(isolation)], Tran = tran };
		}

		/// <summary>
		/// One option of the session's <c>SET</c>: a switch or a STATISTICS or OFFSETS word as written, IDENTITY_INSERT,
		/// or a setting given a value, whose name the old tree keeps in its record and not as written.
		/// </summary>
		static Ast.SetOption SetOption(Statement old, SetExpression option)
		{
			return option switch
			{
				SetExpression.QueryExecution.Switch(var name, var on) => new Ast.SetOption.Switch(new Ast.Identifier(name), on),
				SetExpression.IsoSettings.Switch(var name, var on)    => new Ast.SetOption.Switch(new Ast.Identifier(name), on),
				SetExpression.Statistics.Switch(var name, var on)     => new Ast.SetOption.Switch(new Ast.Identifier(name), on),
				SetExpression.Transactions.Switch(var name, var on)   => new Ast.SetOption.Switch(new Ast.Identifier(name), on),
				SetExpression.Miscellaneous.Switch(var name, var on)  => new Ast.SetOption.Switch(new Ast.Identifier(name), on),

				// The kind is an enum, and the old tree keeps the word as written, which compares without its case.
				SetExpression.Statistics.Report(var kind, var on) when Same(old, "SetExpression.Statistics.Report.Kind", kind.ToUpperInvariant(), kind) => kind.ToUpperInvariant() switch
				{
					"IO"      => new Ast.SetOption.Statistics(Ast.StatisticsKind.Io, on),
					"PROFILE" => new Ast.SetOption.Statistics(Ast.StatisticsKind.Profile, on),
					"TIME"    => new Ast.SetOption.Statistics(Ast.StatisticsKind.Time, on),
					"XML"     => new Ast.SetOption.Statistics(Ast.StatisticsKind.Xml, on),
					_         => throw BridgeGap.Open(old, $"inventory SetExpression.Statistics.Report (the kind `{kind}`)"),
				},

				SetExpression.Statistics.Report => throw BridgeGap.Open(old, "Trees.FreeText (SetExpression.Statistics.Report.Kind, a word not in capitals)"),

				SetExpression.Miscellaneous.Offset(var keyword, var on)       => new Ast.SetOption.Offsets(new Ast.Identifier(keyword), on),
				SetExpression.Miscellaneous.IdentityInsert(var table, var on) => new Ast.SetOption.IdentityInsert(Values.Name(old, table), on),

				SetExpression.DateAndTime.DateFirst(var value)                 => Valued("DATEFIRST", value),
				SetExpression.DateAndTime.DateFormat(var value)                => Valued("DATEFORMAT", value),
				SetExpression.Locking.DeadlockPriority(var value)              => Valued("DEADLOCK_PRIORITY", value),
				SetExpression.Locking.LockTimeout(var value)                   => Valued("LOCK_TIMEOUT", value),
				SetExpression.QueryExecution.RowCount(var value)               => Valued("ROWCOUNT", value),
				SetExpression.QueryExecution.TextSize(var value)               => Valued("TEXTSIZE", value),
				SetExpression.QueryExecution.QueryGovernorCostLimit(var value) => Valued("QUERY_GOVERNOR_COST_LIMIT", value),
				SetExpression.Miscellaneous.Language(var value)                => Valued("LANGUAGE", value),
				SetExpression.Miscellaneous.FipsFlagger(var value)             => Valued("FIPS_FLAGGER", value),
				SetExpression.Miscellaneous.ContextInfo(var value)             => Valued("CONTEXT_INFO", value),
				SetExpression.Miscellaneous.ErrorLevel(var value)              => Valued("ERRLVL", value),

				_ => throw Unconverted(option),
			};

			static Ast.SetOption Valued(string name, Expression value)
			{
				return new Ast.SetOption.Value(new Ast.Identifier(name), Values.Of(value));
			}
		}

		/// <summary>
		/// A transaction's name: an identifier, a variable, or the grammar's third form, a whole number, its sign kept in
		/// its text, and a name of two parts (<c>-5:a.b</c>).
		/// </summary>
		static Ast.TransactionName? TransactionName(Statement.Transaction old)
		{
			return old.Name switch
			{
				null                         => null,
				var name when name.Contains(':') => Numbered(old, name),
				var name when name.StartsWith('@') => new Ast.TransactionName(null, new Ast.Expression.Variable(new Ast.Identifier(name))),
				var name                     => new Ast.TransactionName(Values.Part(old, name)),
			};
		}

		/// <summary><c>-5:a.b</c>: the number with its sign, and the name of two parts after the colon.</summary>
		static Ast.TransactionName Numbered(Statement.Transaction old, string text)
		{
			var colon  = text.IndexOf(':');
			var number = string.Concat(text[..colon].Where(static one => !char.IsWhiteSpace(one)));
			var name   = Values.Name(old, string.Join(".", Split(text[(colon + 1)..])));

			return number.TrimStart('-') is { Length: > 0 } digits && digits.All(char.IsDigit) && name.Count == 2
				? new Ast.TransactionName(null) { Number = new Ast.LiteralValue.Numeric(number, Ast.NumericLiteralKind.DecimalInteger), Qualified = name }
				: throw BridgeGap.Open(old, $"inventory Statement.Transaction (a name `{text}`)");

			// The two parts as tokens, the blanks around the dot dropped and a delimited part's own kept.
			static IEnumerable<string> Split(string parts)
			{
				return SqlTokens.Of(parts).Where(static one => one.Text != ".").Select(static one => one.Text);
			}
		}

		/// <summary>
		/// <c>WITH MARK</c> and its description, which the old tree keeps as the text of the tail: a string, a variable, or none.
		/// </summary>
		static Ast.TransactionMark? Mark(Statement.Transaction old)
		{
			if (old.Tail is null)
				return null;

			var tokens = SqlTokens.Of(old.Tail).ToArray();

			return Words(old.Tail) switch
			{
				["WITH", "MARK"] => new Ast.TransactionMark(),
				["WITH", "MARK", _] when tokens[2].Kind == SqlTokens.Kind.String => new Ast.TransactionMark(Values.String(tokens[2].Text)),
				["WITH", "MARK", _] when tokens[2].Text.StartsWith('@') => new Ast.TransactionMark(new Ast.Expression.Variable(new Ast.Identifier(tokens[2].Text))),
				_ => throw BridgeGap.Lost(old, $"Statement.Transaction (a mark's description other than a string or a variable, kept as text: `{old.Tail}`)"),
			};
		}

		/// <summary>
		/// <c>DECLARE c CURSOR …</c>: the ISO form's words before CURSOR, or T-SQL's after it, and the query.
		/// </summary>
		static Ast.Statement DeclareCursor(Statement old, string name, Clause.CursorDefinition cursor)
		{
			var (properties, extended) = CursorProperties(old, cursor);

			return new Ast.Statement.DeclareCursor
			{
				Cursor      = new Ast.CursorReference(Values.Name(old, name)),
				Properties  = properties,
				SourceValue = CursorSource(old, cursor),
				Extended    = extended,
			};
		}

		/// <summary><c>SET @c = CURSOR … FOR …</c>: T-SQL's words, and the query.</summary>
		static Ast.Statement SetCursor(Statement old, string name, Clause.CursorDefinition cursor)
		{
			var (properties, _) = CursorProperties(old, cursor);

			return new Ast.Statement.SetCursorVariable(new Ast.Expression.Variable(new Ast.Identifier(name)), properties, CursorSource(old, cursor));
		}

		/// <summary>
		/// The words of a cursor. The tree holds what they say and not their order or a word said twice, so only words in
		/// the order the writer prints them carry over; any other order is kept by the old tree alone.
		/// </summary>
		static (Ast.CursorProperties Properties, bool Extended) CursorProperties(Statement old, Clause.CursorDefinition cursor)
		{
			if (cursor.Before.Length > 0)
			{
				var iso = new Ast.CursorProperties(
					cursor.Before.Contains("INSENSITIVE") ? Ast.CursorSensitivity.Insensitive : null,
					cursor.Before.Contains("SCROLL") ? Ast.CursorScrollability.Scroll : null,
					null,
					null);

				string[] printed = [.. cursor.Before.Where(static one => one == "INSENSITIVE").Take(1), .. cursor.Before.Where(static one => one == "SCROLL").Take(1)];

				return printed.SequenceEqual(cursor.Before)
					? (iso, false)
					: throw BridgeGap.Lost(old, "Clause.CursorDefinition (the ISO form's words in another order, or twice)");
			}

			string[] order = ["LOCAL", "GLOBAL", "FORWARD_ONLY", "SCROLL", "STATIC", "KEYSET", "DYNAMIC", "FAST_FORWARD", "READ_ONLY", "SCROLL_LOCKS", "OPTIMISTIC", "TYPE_WARNING"];

			if (!cursor.Options.All(order.Contains) || !order.Where(cursor.Options.Contains).SequenceEqual(cursor.Options))
				throw BridgeGap.Lost(old, "Clause.CursorDefinition (T-SQL's words in another order, or twice)");

			var words = cursor.Options;

			var properties = new Ast.CursorProperties(
				null,
				words.Contains("SCROLL") ? Ast.CursorScrollability.Scroll : words.Contains("FORWARD_ONLY") ? Ast.CursorScrollability.NoScroll : null,
				null,
				null)
			{
				Scope       = words.Contains("LOCAL") ? Ast.CursorScope.Local : words.Contains("GLOBAL") ? Ast.CursorScope.Global : null,
				Type        = words.Contains("STATIC") ? Ast.CursorType.Static : words.Contains("KEYSET") ? Ast.CursorType.Keyset
					: words.Contains("DYNAMIC") ? Ast.CursorType.Dynamic : words.Contains("FAST_FORWARD") ? Ast.CursorType.FastForward : null,
				Concurrency = words.Contains("READ_ONLY") ? Ast.CursorConcurrency.ReadOnly : words.Contains("SCROLL_LOCKS") ? Ast.CursorConcurrency.ScrollLocks
					: words.Contains("OPTIMISTIC") ? Ast.CursorConcurrency.Optimistic : null,
				TypeWarning = words.Contains("TYPE_WARNING"),
				ForwardOnly = words.Contains("FORWARD_ONLY"),
			};

			return (properties, words.Length > 0);
		}

		/// <summary>The query a cursor is over, as a SELECT statement with its hints, and FOR READ ONLY or FOR UPDATE.</summary>
		static Ast.CursorSource CursorSource(Statement old, Clause.CursorDefinition cursor)
		{
			if (StatementOf(new Statement.Select(cursor.With, cursor.Query, null, [], cursor.Hints)) is not Ast.Statement.Select select)
				throw BridgeGap.Open(old, "inventory Clause.CursorDefinition (a query)");

			var updatability = cursor.Access switch
			{
				null => null,
				Clause.CursorFor(false, null) => new Ast.UpdatabilityClause(true, []),
				Clause.CursorFor(true, var columns) => new Ast.UpdatabilityClause(false, Each(columns ?? [], one => Values.Part(old, one))),
				_ => throw BridgeGap.Open(old, $"inventory {NameOf(cursor.Access)}"),
			};

			return new Ast.CursorSource.Query(select, updatability, cursor.AccessFirst);
		}

		/// <summary>
		/// <c>RAISERROR (…) WITH …</c>. The grammar reads the arguments without brackets as well, which no member holds,
		/// and the old tree keeps the options as written and compares them exactly.
		/// </summary>
		static Ast.Statement RaiseError(Statement.RaiseError old)
		{
			if (Spelled(old) is not { } text || Words(text) is not ["RAISERROR", "(", ..])
				throw BridgeGap.Missing(old, "Statement.RaiseError (the arguments without brackets)");

			var options = new List<Ast.RaiseErrorOption>();

			if (old.Tail is { } tail)
			{
				var words = Words(tail);

				for (var at = 1; at < words.Count; at += 2)
					options.Add(words[at] switch
					{
						"LOG"      => Ast.RaiseErrorOption.Log,
						"NOWAIT"   => Ast.RaiseErrorOption.NoWait,
						"SETERROR" => Ast.RaiseErrorOption.SetError,
						_          => throw BridgeGap.Open(old, $"inventory Statement.RaiseError (the option `{words[at]}`)"),
					});

				if (!Same(old, "Statement.RaiseError.Tail", "WITH " + string.Join(", ", words.Where(static (_, at) => at % 2 == 1)), tail))
					throw BridgeGap.Open(old, "Trees.FreeText (Statement.RaiseError.Tail, written otherwise than the writer prints it)");
			}

			return new Ast.Statement.RaiseError(Each(old.Arguments, Values.Of)) { Options = options.ToSqlList() };
		}

		/// <summary><c>THROW</c>, alone or with its three values; the grammar reads one or two, which no member holds.</summary>
		static Ast.Statement Throw(Statement.Throw old)
		{
			return old.Arguments switch
			{
				[] => new Ast.Statement.Throw(null, null, null),
				[var number, var message, var state] => new Ast.Statement.Throw(Values.Of(number), Values.Of(message), Values.Of(state)),
				_ => throw BridgeGap.Missing(old, "Statement.Throw (one, two or four values)"),
			};
		}

		static Ast.Statement WaitFor(Statement.WaitFor old)
		{
			Ast.WaitForKind kind = old.Kind switch
			{
				"DELAY" => Ast.WaitForKind.Delay,
				"TIME"  => Ast.WaitForKind.Time,
				_       => throw BridgeGap.Open(old, $"inventory Statement.WaitFor (`{old.Kind}`)"),
			};

			return new Ast.Statement.WaitFor(kind, Values.Of(old.Value));
		}

		/// <summary>
		/// <c>EXECUTE AS …</c>. The old tree does not keep <c>EXEC</c> against <c>EXECUTE</c>, which is read from the text.
		/// </summary>
		static Ast.Statement ExecuteAs(Statement.ExecuteAs old)
		{

			Ast.ExecutionContextKind kind = old.Kind switch
			{
				"CALLER" => Ast.ExecutionContextKind.Caller,
				"USER"   => Ast.ExecutionContextKind.User,
				"LOGIN"  => Ast.ExecutionContextKind.Login,
				_        => throw BridgeGap.Open(old, $"inventory Statement.ExecuteAs (`{old.Kind}`)"),
			};

			Ast.Expression.Variable? cookie = old.Cookie is null ? null
				: Values.Of(old.Cookie) as Ast.Expression.Variable ?? throw BridgeGap.Open(old, "inventory Statement.ExecuteAs (a cookie that is not a variable)");

			return new Ast.Statement.ExecuteAs(new Ast.ExecutionContext(kind, old.Name is null ? null : Values.Of(old.Name)))
			{
				NoRevert = old.NoRevert,
				Cookie   = cookie,
				Exec     = Recover("Statement.ExecuteAs.Exec", Spelled(old), static text => Words(text) is ["EXEC", ..], false),
			};
		}

		static Ast.CursorReference Reference(Statement old, Expression cursor, bool global)
		{
			return cursor is Expression.ColumnReference(var name)
				? new Ast.CursorReference(Values.Name(old, name), Global: global)
				: new Ast.CursorReference(null, Values.Of(cursor), Global: global);
		}

		static Ast.Statement Cursor(Statement.CursorAction old)
		{
			return string.Join(' ', Words(old.Verb)) switch
			{
				"OPEN"       => new Ast.Statement.OpenCursor { Cursor = Reference(old, old.Cursor, old.Global) },
				"CLOSE"      => new Ast.Statement.CloseCursor { Cursor = Reference(old, old.Cursor, old.Global) },
				"DEALLOCATE" => new Ast.Statement.DeallocateCursor(Reference(old, old.Cursor, old.Global)),
				_            => throw BridgeGap.Open(old, $"inventory Statement.CursorAction (`{old.Verb}`)"),
			};
		}

		static Ast.Statement Fetch(Statement.Fetch old)
		{
			Ast.FetchOrientation? orientation = old.Orientation?.ToUpperInvariant() switch
			{
				null       => null,
				"NEXT"     => new Ast.FetchOrientation(Ast.FetchOrientationKind.Next),
				"PRIOR"    => new Ast.FetchOrientation(Ast.FetchOrientationKind.Prior),
				"FIRST"    => new Ast.FetchOrientation(Ast.FetchOrientationKind.First),
				"LAST"     => new Ast.FetchOrientation(Ast.FetchOrientationKind.Last),
				"ABSOLUTE" => new Ast.FetchOrientation(Ast.FetchOrientationKind.Absolute, old.Offset is null ? null : Values.Of(old.Offset)),
				"RELATIVE" => new Ast.FetchOrientation(Ast.FetchOrientationKind.Relative, old.Offset is null ? null : Values.Of(old.Offset)),
				_          => throw BridgeGap.Open(old, $"inventory Statement.Fetch (`{old.Orientation}`)"),
			};

			return new Ast.Statement.FetchCursor
			{
				Orientation = orientation,
				FromKeyword = old.From,
				Cursor      = Reference(old, old.Cursor, old.Global),
				Into        = old.Into is { } into ? new Ast.DynamicArguments.Values(Each(into, Values.Of)) : null,
			};
		}

		/// <summary>
		/// One variable declared: a table, with the body CREATE TABLE's bridge reads; <c>CURSOR</c>, the one-part type
		/// a cursor parameter has; or a type and a value.
		/// </summary>
		static Ast.VariableDeclaration Declaration(Clause old)
		{
			if (old is not Clause.VariableDeclaration(var name, { } type, var value, var elements, null, var asKeyword))
				throw BridgeGap.Open(old, "inventory Clause.VariableDeclaration");

			var variable = new Ast.Identifier(name);

			if (type == "TABLE" && elements is { } body)
			{
				// CREATE TABLE's body as the Tables family reads it.
				return new Ast.VariableDeclaration.Table(variable, Tables.Elements(body)) { AsKeyword = asKeyword };
			}

			if (elements is not null)
				throw BridgeGap.Open(old, "inventory Clause.VariableDeclaration (a body without TABLE)");

			if (type == "CURSOR")
				return value is null
					? new Ast.VariableDeclaration.Typed(variable, new Ast.DataType.Named(new Ast.QualifiedName([new Ast.Identifier("CURSOR")]), []), null) { AsKeyword = asKeyword }
					: throw BridgeGap.Open(old, "inventory Clause.VariableDeclaration (a CURSOR with a value)");

			// The old tree keeps the type as written, and compares it token by token: only a type written as the writer prints
			// it, but for the case of its key words and its blanks, reads back as itself.
			var typed = Values.Type(old, type);

			if (!Same(old, "Clause.VariableDeclaration.Type", Ast.TransactSqlWriter.Write(typed), type))
				throw BridgeGap.Open(old, "Trees.FreeText (Clause.VariableDeclaration.Type, a type written otherwise than the writer prints it)");

			return new Ast.VariableDeclaration.Typed(variable, typed, value is null ? null : Values.Of(value)) { AsKeyword = asKeyword };
		}

		/// <summary>
		/// <c>SET @v op= value</c>, a property of the variable assigned, or a method of it called. The old tree keeps the
		/// variable and its members as one text, the variable first and with no dot in it.
		/// </summary>
		static Ast.Statement SetVariable(Statement.SetVariable old)
		{
			if (old.Through is not null)
				throw BridgeGap.Open(old, "inventory Statement.SetVariable (a column beside the variable)");

			var dot      = old.Name.IndexOf('.');
			var variable = new Ast.Expression.Variable(new Ast.Identifier(dot < 0 ? old.Name : old.Name[..dot]));
			var members  = dot < 0 ? [] : Values.Name(old, old.Name[(dot + 1)..]).Parts;

			if (old.Operator == "")
			{
				if (members is not [var method] || old.Value is not Expression.RoutineInvocation("", var arguments, null))
					throw BridgeGap.Open(old, "inventory Statement.SetVariable (a call that is not a method's of the variable)");

				var call = new Ast.Expression.Member(variable, Ast.MemberAccessKind.Dot, method, Each(arguments, static one => new Ast.Argument(Values.Of(one))));

				return new Ast.Statement.SetVariable(variable, call) { Operator = null };
			}

			Ast.AssignmentOperator assigned = old.Operator switch
			{
				null or "=" => Ast.AssignmentOperator.Assign,
				"+="        => Ast.AssignmentOperator.Add,
				"-="        => Ast.AssignmentOperator.Subtract,
				"*="        => Ast.AssignmentOperator.Multiply,
				"/="        => Ast.AssignmentOperator.Divide,
				"%="        => Ast.AssignmentOperator.Modulo,
				"&="        => Ast.AssignmentOperator.BitwiseAnd,
				"|="        => Ast.AssignmentOperator.BitwiseOr,
				"^="        => Ast.AssignmentOperator.BitwiseXor,
				"||="       => Ast.AssignmentOperator.Concatenate,
				_           => throw BridgeGap.Open(old, $"inventory Statement.SetVariable (operator `{old.Operator}`)"),
			};

			return new Ast.Statement.SetVariable(variable, Values.Of(old.Value)) { Operator = assigned, Members = members };
		}

		/// <summary>
		/// A block, and a natively compiled one's settings: the isolation level, whose name says TRAN or TRANSACTION, and
		/// the five others, a name and a value each.
		/// </summary>
		static Ast.Statement Compound(Statement.Compound old)
		{
			return new Ast.Statement.Compound(Each(old.Statements, StatementOf))
			{
				Atomic = old.Atomic is { } atomic ? Each(atomic, one => AtomicOption(old, one)) : null,
			};
		}

		static Ast.AtomicOption AtomicOption(Statement old, Clause option)
		{
			if (option is not Clause.Option { Name: var name, Value: { } value, Options.Length: 0, Partitions: null, Bare: false })
				throw BridgeGap.Open(old, "inventory Statement.Compound (an atomic option that is not a name and a value)");

			var words = Words(name);

			if (words is ["TRANSACTION" or "TRAN", "ISOLATION", "LEVEL"])
			{
				Ast.IsolationLevel level = value is Expression.ColumnReference(var text) ? string.Join(' ', Words(text)) switch
				{
					"READ UNCOMMITTED" => Ast.IsolationLevel.ReadUncommitted,
					"READ COMMITTED"   => Ast.IsolationLevel.ReadCommitted,
					"REPEATABLE READ"  => Ast.IsolationLevel.RepeatableRead,
					"SERIALIZABLE"     => Ast.IsolationLevel.Serializable,
					"SNAPSHOT"         => Ast.IsolationLevel.Snapshot,
					_                  => throw BridgeGap.Open(old, $"inventory Statement.Compound (the level `{text}`)"),
				} : throw BridgeGap.Open(old, "inventory Statement.Compound (a level that is not words)");

				return new Ast.AtomicOption.Isolation(level) { Tran = words[0] == "TRAN" };
			}

			if (words.Count != 1)
				throw BridgeGap.Open(old, $"inventory Statement.Compound (the atomic option `{name}`)");

			return new Ast.AtomicOption.Setting(new Ast.Option(new Ast.Identifier(name), Values.Of(value), []));
		}
	}
}
