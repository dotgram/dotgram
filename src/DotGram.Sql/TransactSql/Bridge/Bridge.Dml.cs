using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

static partial class Bridge
{
	/// <summary>
	/// The data change statements: <c>INSERT</c>, <c>UPDATE</c>, <c>DELETE</c>, <c>MERGE</c>, <c>TRUNCATE</c>, and the text ones.
	/// </summary>
	internal static class Dml
	{
		public static readonly string[] Owns =
		[
			"Statement.Insert", "Statement.Update", "Statement.Delete", "Statement.Merge", "Statement.Truncate",
			"Statement.ReadText", "Statement.WriteText", "Statement.UpdateText",
			"Query.DefaultValues", "Query.FromFile", "Query.FromStream",
			"Clause.Set", "Clause.Output", "Clause.MergeWhen", "Clause.PartitionRange",
			"TableReference.Changed",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				Statement.Insert insert     => Insert(insert),
				Statement.Update update     => Update(update),
				Statement.Delete delete     => Delete(delete),
				Statement.Merge merge       => Merge(merge),
				TableReference.Changed changed => Changed(changed),
				Query.DefaultValues         => throw BridgeGap.Open(old, "inventory Query.DefaultValues (outside an INSERT)"),
				_ when Owned(Owns, old)     => throw Unconverted(old),
				_                           => null,
			};

			return made is not null;
		}

		/// <summary>
		/// The statement a WITH is written in front of, on the common tree where it changes rows: the WITH, and what the
		/// rules read of the statement into the T-SQL tree's record, which is then no statement of any tree. It stands
		/// where the statement and its WITH were written. A query keeps its own, and any other statement has none.
		/// </summary>
		internal static Statement Preceded(SqlList<Clause>? with, Statement statement, SqlSpan at)
		{
			// A query statement, built where it is carried: the WITH is its own.
			if (statement is Statement.Built { Given: Queries.Selecting selecting } selected)
				return with is { Length: > 0 } list ? Queries.Select(selected, selecting, list) : statement;

			// A statement held for the extent it ends with: this is it.
			if (statement is Statement.Built { Pending: not null } held)
				return Finish(held, at);

			if (statement is not (Statement.Insert or Statement.Update or Statement.Delete or Statement.Merge))
				return Syntax.Preceded(with, statement);

			var read = Syntax.Preceded(with, statement);

			read.Locate(at.At, at.Length);

			return Carry(read, static read => read switch
			{
				Statement.Insert insert => Insert(insert),
				Statement.Update update => Update(update),
				Statement.Delete delete => Delete(delete),
				_                       => Merge((Statement.Merge)read),
			});
		}

		/// <summary>TRUNCATE TABLE, on the common tree: the table and its partitions.</summary>
		internal static Statement Truncate(string name, SqlList<Clause>? partitions)
		{
			return Carry(new Statement.Truncate(name, partitions), Truncate);
		}

		/// <summary>READTEXT, on the common tree.</summary>
		internal static Statement ReadText(string column, Expression pointer, Expression offset, Expression size, bool holdLock)
		{
			return Text(new Statement.ReadText(column, pointer, offset, size, holdLock));
		}

		/// <summary>WRITETEXT, on the common tree.</summary>
		internal static Statement WriteText(string column, Expression pointer, Expression? data, bool bulk, bool log, Expression? timestamp)
		{
			return Text(new Statement.WriteText(column, pointer, data, bulk, log, timestamp));
		}

		/// <summary>UPDATETEXT, on the common tree.</summary>
		internal static Statement UpdateText(
			string column, Expression pointer, Expression offset, Expression length, Expression? data, string? source, Expression? sourcePointer,
			bool bulk, bool log, Expression? timestamp)
		{
			return Text(new Statement.UpdateText(column, pointer, offset, length, data, source, sourcePointer, bulk, log, timestamp));
		}

		/// <summary>READTEXT, WRITETEXT and UPDATETEXT, on the common tree, from the record their rules read.</summary>
		static Statement Text(Statement read)
		{
			return Carry(read, static old => old switch
			{
					Statement.ReadText read     => new Ast.Statement.ReadText(Values.Name(read, read.Column), Values.Of(read.Pointer), Values.Of(read.Offset), Values.Of(read.Size), read.HoldLock),
					Statement.WriteText write   => new Ast.Statement.WriteText(
						Values.Name(write, write.Column), Values.Of(write.Pointer), write.Data is null ? null : Values.Of(write.Data), write.Bulk, write.Log,
						write.Timestamp is null ? null : Values.Of(write.Timestamp)),
					Statement.UpdateText update => new Ast.Statement.UpdateText(
						Values.Name(update, update.Column), Values.Of(update.Pointer), Values.Of(update.Offset), Values.Of(update.Length),
						update.Data is null ? null : Values.Of(update.Data), update.Source is null ? null : Values.Name(update, update.Source),
						update.SourcePointer is null ? null : Values.Of(update.SourcePointer), update.Bulk, update.Log,
						update.Timestamp is null ? null : Values.Of(update.Timestamp)),
				_ => throw new ArgumentException($"No text statement is a {NameOf(old)}.", nameof(old)),
			});
		}

		/// <summary>
		/// <c>(MERGE … OUTPUT …) AS c (cols)</c>, the rows a data change statement returns read as a table: the new
		/// tree's data change table with no result option, whose rows are the statement's OUTPUT.
		/// </summary>
		static Ast.TableSource Changed(TableReference.Changed old)
		{
			var said = Recover("Alias.AsKeyword of a table source", After(old.Statement, old), static text => Words(text) is [")", "AS", ..], false);

			return new Ast.TableSource.DataChange(null, StatementOf(old.Statement),
				new Ast.Alias(Values.Part(old, old.Name), old.Columns is { } columns ? Values.Parts(old, columns) : null, said));
		}

		/// <summary>The text from the end of one located node to the end of the node it stands in.</summary>
		static string? After(ISqlSpan node, ISqlSpan within)
		{
			if (!Cuttable(node.Span) || !Cuttable(within.Span))
				return null;

			return node.Span.End <= within.Span.End && within.Span.End <= source.Length ? source[node.Span.End..within.Span.End] : null;
		}

		/// <summary>
		/// One option of BULK INSERT or INSERT BULK: a word, or a word and its value; <c>ORDER (…)</c>, which the old tree
		/// keeps as the option's name with its sort list in it, read as a sort list again.
		/// </summary>
		static Ast.Option BulkOption(Clause old)
		{
			if (old is not Clause.Option { Name: var name, Value: var value, Options.Length: 0, Partitions: null, Bare: false })
				throw BridgeGap.Open(old, "inventory Clause.Option (a bulk option)");

			if (Words(name) is ["ORDER", "(", ..] && name.TrimEnd().EndsWith(')'))
			{
				var inside = name[(SqlTokens.Find(name, "(") + 1)..SqlTokens.FindLast(name, ")")];

				if (value is not null || Queries.Selected("SELECT 1 ORDER BY " + inside) is not { By: Clause.OrderBy by })
					throw BridgeGap.Open(old, "inventory Clause.Option (an ORDER that is not a sort list)");

				return new Ast.Option(new Ast.Identifier("ORDER"), null, []) { Order = new Ast.RowOrder(Queries.Order(by).Items, false) };
			}

			return name.Contains('.') || name.Any(char.IsWhiteSpace)
				? throw BridgeGap.Open(old, $"inventory Clause.Option (a bulk option `{name}`)")
				: new Ast.Option(Values.Part(old, name), value is null ? null : Values.Of(value), []);
		}

		/// <summary>The WITH a data change statement is written after, where it has one.</summary>
		static Ast.WithClause? With(SqlList<Clause>? with)
		{
			return with is { Length: > 0 } list ? Queries.With(list) : null;
		}

		static SqlList<Ast.QueryHint> Hints(SqlList<Clause>? options)
		{
			return options is { } list ? Each(list, Queries.QueryHint) : [];
		}

		/// <summary>
		/// What a data change statement writes to: a table variable keeps its hints here, which in a FROM it may
		/// not have; a call is a function; the rest is a table source as a FROM reads it.
		/// </summary>
		static Ast.TableSource Target(TableReference old)
		{
			if (old is TableReference.Named { Table: var table, Hints.Length: > 0 } named && table.StartsWith('@'))
				return Source(named with { Hints = [] }) is Ast.TableSource.Variable variable
					? variable with { Hints = Each(named.Hints, Queries.TableHint) }
					: throw BridgeGap.Open(old, "inventory TableReference.Named (a table variable target)");

			// A call is a function as the grammar reads it there, OPENJSON among them; OPENQUERY alone is a rowset.
			if (old is TableReference.FunctionCall { Function: var call, Schema.Length: 0 } && !call.Name.Equals("OPENQUERY", StringComparison.OrdinalIgnoreCase))
				return Values.Function(call) is Ast.Expression.Invocation invocation
					? new Ast.TableSource.Function(invocation)
					: throw BridgeGap.Open(old, "inventory TableReference.FunctionCall (a target that is not a plain call)");

			return Source(old);
		}

		/// <summary>
		/// T-SQL's OUTPUT: the items as a select list's, <c>INTO</c> a target and its columns, and the second OUTPUT.
		/// </summary>
		static Ast.OutputClause? Output(Clause? old)
		{
			if (old is null)
				return null;

			if (old is not Clause.Output(var items, var into, var columns, var next))
				throw BridgeGap.Open(old, "inventory Clause.Output");

			return new Ast.OutputClause(
				Each(items, static one => Convert<Ast.SelectColumn>(one)),
				into is null ? null : Target(into),
				columns is null ? null : Values.Parts(old, columns),
				Output(next));
		}

		static Ast.Statement Insert(Statement.Insert old)
		{
			switch (old.Rows)
			{
				// BULK INSERT and INSERT BULK, which the old tree keeps as an INSERT whose rows are a file or a stream.
				case Query.FromFile(var file, var options) when old.Target is TableReference.Named { Table: var table }:
					return new Ast.Statement.BulkInsert(
						Values.Name(old, table),

						// The file's quoted string is kept with its N, which is a national string and not the character one with an N in it.
						file is Expression.Literal { Kind: SqlLiteralKind.Text, Text: var token } ? Values.String(token) : Values.Of(file),
						Each(options ?? [], BulkOption));

				case Query.FromStream(var columns, var options) when old.Target is TableReference.Named { Table: var table }:
					return new Ast.Statement.InsertBulk(
						Values.Name(old, table),
						Each(columns ?? [], static one => one is Clause.ColumnDefinition(var name, var type, null, { Length: 0 })
							? Tables.TypedColumn(one, name, type)
							: throw BridgeGap.Open(one, "inventory Clause.ColumnDefinition (a column of INSERT BULK)")),
						Each(options ?? [], BulkOption));

				case Query.FromExecute:
					break;
			}

			if (old.Target is null)
				throw BridgeGap.Open(old, "inventory Statement.Insert (no target)");

			Ast.IntoKeyword? into = old.Into?.ToUpperInvariant() switch
			{
				null   => null,
				"INTO" => Ast.IntoKeyword.Into,
				"OVER" => Ast.IntoKeyword.Over,
				_      => throw BridgeGap.Open(old, $"inventory Statement.Insert (the word {old.Into} before the target)"),
			};

			Ast.InsertSource rows = old.Rows switch
			{
				Query.DefaultValues                => new Ast.InsertSource.DefaultValues(),
				Query.Built { Record: "Query.TableValueConstructor" } values => new Ast.InsertSource.Values(Rows(values)),
				Query.FromExecute execute          => Routines.InsertSource(execute),
				_                                  => new Ast.InsertSource.Subquery(QueryOf(old.Rows)),
			};

			return new Ast.Statement.Insert
			{
				With        = With(old.With),
				Top         = Queries.Topped(old.Top),
				Into        = into,
				Target      = Target(old.Target),
				Columns     = Each(old.Columns ?? [], one => Values.Name(old, one) is { Parts: [var part] } && !one.StartsWith('.')
					? part
					: throw BridgeGap.Open(old, $"inventory Statement.Insert (a qualified or prefixed column `{one}`, where the column list holds identifiers)")),
				Output      = Output(old.Output),
				SourceValue = rows,
				Hints       = Hints(old.Options),
			};
		}

		static Ast.Statement Update(Statement.Update old)
		{
			if (old.Target is null)
				throw BridgeGap.Open(old, "inventory Statement.Update (no target)");

			return new Ast.Statement.Update
			{
				With        = With(old.With),
				Top         = Queries.Topped(old.Top),
				Target      = Target(old.Target),
				Assignments = Each(old.Set, Assignment),
				Output      = Output(old.Output),
				From        = old.From.Length > 0 ? new Ast.FromClause(Each(old.From, Source)) : null,
				Where       = Where(old.Where),
				CurrentOf   = CurrentOf(old, old.Where),
				Hints       = Hints(old.Options),
			};
		}

		static Ast.Statement Delete(Statement.Delete old)
		{
			if (old.Target is null)
				throw BridgeGap.Open(old, "inventory Statement.Delete (no target)");

			return new Ast.Statement.Delete
			{
				With        = With(old.With),
				Top         = Queries.Topped(old.Top),
				FromOmitted = Recover("Statement.Delete.FromOmitted", Before(old, old.Target), static text => Words(text) is not [.., "DELETE", "FROM"] and not ["FROM"], false),
				Target      = Target(old.Target),
				Output      = Output(old.Output),
				From        = old.From.Length > 0 ? new Ast.FromClause(Each(old.From, Source)) : null,
				Where       = Where(old.Where),
				CurrentOf   = CurrentOf(old, old.Where),
				Hints       = Hints(old.Options),
			};
		}

		/// <summary>
		/// The text a statement's target is written after: from the end of its TOP, or else from its start, which
		/// may hold a WITH.
		/// </summary>
		static string? Before(Statement.Delete old, TableReference target)
		{
			if (!Cuttable(old.Span) || !Cuttable(target.Span))
				return null;

			var from = old.Top is { Span: { Known: true, IsStale: false } top } ? top.End : old.Span.At;

			return from <= target.Span.At && target.Span.At <= source.Length ? source[from..target.Span.At] : null;
		}

		/// <summary>
		/// <c>WHERE CURRENT OF c</c>, which the old tree keeps as a column reference whose text is the words, and
		/// the new tree as the cursor it names.
		/// </summary>
		static Ast.CursorReference? CurrentOf(Statement old, Expression? where)
		{
			if (where is not Expression.ColumnReference(var text) || !text.StartsWith("CURRENT OF ", StringComparison.OrdinalIgnoreCase))
				return null;

			var name   = text["CURRENT OF ".Length..].TrimStart();
			var global = name.StartsWith("GLOBAL ", StringComparison.OrdinalIgnoreCase);

			if (global)
				name = name["GLOBAL ".Length..].TrimStart();

			return new Ast.CursorReference(Values.Name(old, name), Global: global);
		}

		static Ast.Expression? Where(Expression? where)
		{
			return where is null || where is Expression.ColumnReference(var text) && text.StartsWith("CURRENT OF ", StringComparison.OrdinalIgnoreCase)
				? null
				: Values.Of(where);
		}

		static Ast.Assignment Assignment(Clause old)
		{
			if (old is not Clause.Set(var target, var op, var value, var through))
				throw BridgeGap.Open(old, "inventory Clause.Set");

			// A method called on a column, `SET c.WRITE (…)`: the call is the value, on the column's reference.
			if (op == "")
			{
				if (value is not Expression.RoutineInvocation(var method, var arguments, null) || !method.StartsWith('.') || target.Contains('.'))
					throw BridgeGap.Open(old, "inventory Clause.Set (a method call that is not one on a column)");

				return new Ast.Assignment([], new Ast.Expression.Member(
					new Ast.Expression.Reference(Values.Name(old, target)),
					Ast.MemberAccessKind.Dot,
					Values.Part(old, method[1..]),
					Each(arguments, static one => new Ast.Argument(Values.Of(one)))));
			}

			var assigned = op switch
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
				_           => throw BridgeGap.Open(old, $"inventory Clause.Set (operator `{op}`)"),
			};

			// A variable, alone or taking the new value of the column after it.
			if (target.StartsWith('@'))
				return new Ast.Assignment(through is null ? [] : [new Ast.AssignmentTarget(Values.Name(old, through))], Values.Of(value))
				{
					Variable = new Ast.Expression.Variable(new Ast.Identifier(target)),
					Operator = assigned,
				};

			return new Ast.Assignment([new Ast.AssignmentTarget(Values.Name(old, target))], Values.Of(value)) { Operator = assigned };
		}

		static Ast.Statement Merge(Statement.Merge old)
		{
			if (old.Target is null)
				throw BridgeGap.Open(old, "inventory Statement.Merge (no target)");

			// `MERGE t AS x` and `MERGE t x` are one old node; the text between the target and USING says which.
			var alias = old.Alias is { } name
				? new Ast.Alias(Values.Part(old, name), AsKeyword: Recover("Alias.AsKeyword of MERGE's target", Between(old.Target, old.Using), static text => Words(text).Contains("AS"), true))
				: null;

			return new Ast.Statement.Merge
			{
				With        = With(old.With),
				Top         = Queries.Topped(old.Top),
				IntoOmitted = !old.Into,
				Target      = Target(old.Target),
				Alias       = alias,
				SourceTable = Source(old.Using),
				On          = Values.Of(old.On),
				Clauses     = Each(old.Whens, When),
				Output      = Output(old.Output),
				Hints       = Hints(old.Options),
			};
		}

		static Ast.MergeClause When(Clause old)
		{
			if (old is not Clause.MergeWhen(var matched, var by, var condition, var action))
				throw BridgeGap.Open(old, "inventory Clause.MergeWhen");

			var source = "SOURCE".Equals(by, StringComparison.OrdinalIgnoreCase);
			var target = "TARGET".Equals(by, StringComparison.OrdinalIgnoreCase);

			if (by is not null && !source && !target)
				throw BridgeGap.Open(old, $"inventory Clause.MergeWhen (BY {by})");

			Ast.MergeClause made = (matched, source, action) switch
			{
				(true, false, Statement.Update { Set: var set }) => new Ast.MergeClause.Matched(new Ast.MergeMatchedAction.Update(Each(set, Assignment))),
				(true, false, Statement.Delete)                  => new Ast.MergeClause.Matched(new Ast.MergeMatchedAction.Delete()),
				(false, true, Statement.Update { Set: var set }) => new Ast.MergeClause.NotMatchedBySource(new Ast.MergeMatchedAction.Update(Each(set, Assignment))),
				(false, true, Statement.Delete)                  => new Ast.MergeClause.NotMatchedBySource(new Ast.MergeMatchedAction.Delete()),
				(false, false, Statement.Insert insert)          => new Ast.MergeClause.NotMatched(Inserted(insert)) { ByTarget = target },
				_                                                => throw BridgeGap.Open(old, $"inventory Clause.MergeWhen (a {NameOf(action)} action)"),
			};

			return condition is null ? made : made with { Condition = Values.Of(condition) };
		}

		/// <summary>The rows of a table value constructor its rule built, carried as the query it stands for would be.</summary>
		static SqlList<Ast.RowValue> Rows(Query.Built values)
		{
			if (values.Recovered is { } read)
				recovered?.AddRange(read);

			return values.Values ?? throw values.Gap!;
		}

		static Ast.MergeInsertAction Inserted(Statement.Insert old)
		{
			return old.Rows switch
			{
				Query.Built { Record: "Query.TableValueConstructor", Rows: [Expression.RowValueConstructor(var items)] } =>
					new Ast.MergeInsertAction(Values.Parts(old, old.Columns), null, Each(items, Values.Of)),
				Query.Built { Record: "Query.TableValueConstructor", Rows: [Expression.Parenthesized(var one)] } =>
					new Ast.MergeInsertAction(Values.Parts(old, old.Columns), null, [Values.Of(one)]),
				Query.DefaultValues when old.Columns is null => new Ast.MergeInsertAction([], null, []) { DefaultValues = true },
				_                   => throw BridgeGap.Open(old, "inventory Clause.MergeWhen (an INSERT that is not one row)"),
			};
		}

		static Ast.Statement Truncate(Statement.Truncate old)
		{
			return new Ast.Statement.TruncateTable
			{
				Target     = new Ast.TableSource.Named(Values.Name(old, old.Table)),
				Partitions = old.Partitions is { } partitions
					? Each(partitions, static one => one is Clause.PartitionRange(var from, var to)
						? new Ast.PartitionRange(Values.Of(from), to is null ? null : Values.Of(to))
						: throw BridgeGap.Open(one, "inventory Clause.PartitionRange"))
					: [],
			};
		}
	}
}
