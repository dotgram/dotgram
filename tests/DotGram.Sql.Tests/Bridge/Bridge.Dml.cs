using System;
using System.Diagnostics.CodeAnalysis;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

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
			"Query.DefaultValues", "Query.FromFile", "Query.FromStream", "Query.FromExecute",
			"Clause.Set", "Clause.Output", "Clause.MergeWhen", "Clause.PartitionRange",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				Statement.Insert insert     => Insert(insert),
				Statement.Update update     => Update(update),
				Statement.Delete delete     => Delete(delete),
				Statement.Merge merge       => Merge(merge),
				Statement.Truncate truncate => Truncate(truncate),
				Query.FromFile              => throw BridgeGap.Missing(old, "Statement.BulkInsert (inventory Query.FromFile)"),
				Query.FromStream            => throw BridgeGap.Missing(old, "Statement.InsertBulk (inventory Query.FromStream)"),
				Query.FromExecute           => throw BridgeGap.Missing(old, "InsertSource.Execute (inventory Query.FromExecute)"),
				Query.DefaultValues         => throw BridgeGap.Open(old, "inventory Query.DefaultValues (outside an INSERT)"),
				_ when Owned(Owns, old)     => throw Inventory.Unconverted(old),
				_                           => null,
			};

			return made is not null;
		}

		/// <summary>
		/// The T-SQL parts of a data change statement that proposal 2 gives it and the new tree does not have yet.
		/// </summary>
		static void Plain(Statement old, SqlList<Clause>? with, Clause? output, SqlList<Clause>? options)
		{
			if (with is { Length: > 0 })
				throw BridgeGap.Missing(old, $"{NameOf(old)} has no WITH in the new tree (proposal 2)");

			if (output is not null)
				throw BridgeGap.Missing(old, $"{NameOf(old)} has no OutputClause in the new tree (proposal 2)");

			if (options is { Length: > 0 })
				throw BridgeGap.Missing(old, $"{NameOf(old)} has no OPTION hints in the new tree (proposal 2)");
		}

		static Ast.Statement Insert(Statement.Insert old)
		{
			switch (old.Rows)
			{
				case Query.FromFile:
					throw BridgeGap.Missing(old.Rows, "Statement.BulkInsert (inventory Query.FromFile)");

				case Query.FromStream:
					throw BridgeGap.Missing(old.Rows, "Statement.InsertBulk (inventory Query.FromStream)");

				case Query.FromExecute:
					throw BridgeGap.Missing(old.Rows, "InsertSource.Execute (inventory Query.FromExecute)");
			}

			Plain(old, old.With, old.Output, old.Options);

			if (!"INTO".Equals(old.Into, StringComparison.OrdinalIgnoreCase))
				throw BridgeGap.Missing(old, "Statement.Insert has no member for whether INTO was written (proposal 2)");

			if (old.Target is null)
				throw BridgeGap.Open(old, "inventory Statement.Insert (no target)");

			Ast.InsertSource rows = old.Rows switch
			{
				Query.DefaultValues                => new Ast.InsertSource.DefaultValues(),
				Query.TableValueConstructor(var r) => new Ast.InsertSource.Values(Each(r, Queries.Row)),
				_                                  => new Ast.InsertSource.Subquery(QueryOf(old.Rows)),
			};

			return new Ast.Statement.Insert
			{
				Top         = Queries.Topped(old.Top),
				Target      = Source(old.Target),
				Columns     = Values.Parts(old, old.Columns),
				SourceValue = rows,
			};
		}

		static Ast.Statement Update(Statement.Update old)
		{
			Plain(old, old.With, old.Output, old.Options);

			if (old.From.Length > 0)
				throw BridgeGap.Missing(old, "Statement.Update has no second FROM (proposal 2)");

			if (old.Target is null)
				throw BridgeGap.Open(old, "inventory Statement.Update (no target)");

			return new Ast.Statement.Update
			{
				Top         = Queries.Topped(old.Top),
				Target      = Source(old.Target),
				Assignments = Each(old.Set, Assignment),
				Where       = Where(old.Where),
				CurrentOf   = CurrentOf(old, old.Where),
			};
		}

		static Ast.Statement Delete(Statement.Delete old)
		{
			Plain(old, old.With, old.Output, old.Options);

			if (old.From.Length > 0)
				throw BridgeGap.Missing(old, "Statement.Delete has no second FROM (proposal 2)");

			if (old.Target is null)
				throw BridgeGap.Open(old, "inventory Statement.Delete (no target)");

			return new Ast.Statement.Delete
			{
				Top    = Queries.Topped(old.Top),
				Target    = Source(old.Target),
				Where     = Where(old.Where),
				CurrentOf = CurrentOf(old, old.Where),
			};
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

			if (op == "")
				throw BridgeGap.Missing(old, "`.WRITE (…)` as a member call on the target (proposal 4)");

			if (op is not null and not "=")
				throw BridgeGap.Missing(old, "Assignment has no compound operator (proposal 4)");

			if (through is not null)
				throw BridgeGap.Missing(old, "Assignment has no Through for `@v = column = value` (proposal 4)");

			return new Ast.Assignment([new Ast.AssignmentTarget(Values.Name(old, target))], Values.Of(value));
		}

		static Ast.Statement Merge(Statement.Merge old)
		{
			Plain(old, old.With, old.Output, old.Options);

			if (!old.Into)
				throw BridgeGap.Missing(old, "Statement.Merge has no member for whether INTO was written (proposal 2)");

			if (old.Target is null)
				throw BridgeGap.Open(old, "inventory Statement.Merge (no target)");

			// `MERGE t AS x` and `MERGE t x` are one old node; the text between the target and USING says which.
			var alias = old.Alias is { } name
				? new Ast.Alias(Values.Part(old, name), AsKeyword: Recover("Alias.AsKeyword of MERGE's target", Between(old.Target, old.Using), static text => Words(text).Contains("AS"), true))
				: null;

			return new Ast.Statement.Merge
			{
				Top         = Queries.Topped(old.Top),
				Target      = Source(old.Target),
				Alias       = alias,
				SourceTable = Source(old.Using),
				On          = Values.Of(old.On),
				Clauses     = Each(old.Whens, When),
			};
		}

		static Ast.MergeClause When(Clause old)
		{
			if (old is not Clause.MergeWhen(var matched, var by, var condition, var action))
				throw BridgeGap.Open(old, "inventory Clause.MergeWhen");

			if (by is not null)
				throw BridgeGap.Missing(old, "MergeClause.NotMatchedBySource and the BY TARGET flag (proposal 3)");

			Ast.MergeClause made = (matched, action) switch
			{
				(true, Statement.Update { Set: var set }) => new Ast.MergeClause.Matched(new Ast.MergeMatchedAction.Update(Each(set, Assignment))),
				(true, Statement.Delete)                  => new Ast.MergeClause.Matched(new Ast.MergeMatchedAction.Delete()),
				(false, Statement.Insert insert)          => new Ast.MergeClause.NotMatched(Inserted(insert)),
				_                                         => throw BridgeGap.Open(old, $"inventory Clause.MergeWhen (a {NameOf(action)} action)"),
			};

			return condition is null ? made : made with { Condition = Values.Of(condition) };
		}

		static Ast.MergeInsertAction Inserted(Statement.Insert old)
		{
			return old.Rows switch
			{
				Query.TableValueConstructor { Rows: [Expression.RowValueConstructor(var items)] } =>
					new Ast.MergeInsertAction(Values.Parts(old, old.Columns), null, Each(items, Values.Of)),
				Query.TableValueConstructor { Rows: [Expression.Parenthesized(var one)] } =>
					new Ast.MergeInsertAction(Values.Parts(old, old.Columns), null, [Values.Of(one)]),
				Query.DefaultValues => throw BridgeGap.Missing(old, "MergeInsertAction has no DEFAULT VALUES (proposal 3)"),
				_                   => throw BridgeGap.Open(old, "inventory Clause.MergeWhen (an INSERT that is not one row)"),
			};
		}

		static Ast.Statement Truncate(Statement.Truncate old)
		{
			if (old.Partitions is not null)
				throw BridgeGap.Missing(old, "Statement.TruncateTable has no partitions (PartitionRange is declared, no member holds it)");

			return new Ast.Statement.TruncateTable { Target = new Ast.TableSource.Named(Values.Name(old, old.Table)) };
		}
	}
}
