using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// What a FROM clause reads, and the statements that define tables, views and indexes.
	/// </summary>
	internal static class Tables
	{
		public static readonly string[] Owns =
		[
			.. typeof(TableReference).GetNestedTypes().Where(static one => one.IsSubclassOf(typeof(TableReference))).Select(NameOf),
			"Clause.SystemTime", "Clause.TableSample", "Clause.JsonColumn", "Clause.ColumnDefinition", "Clause.ColumnOption",
			"Clause.ConstraintDefinition", "Clause.References", "Clause.Connection", "Clause.Dropped", "Clause.Placement",
			"Clause.Option",
			"Statement.TableDefinition", "Statement.CreateTableAsSelect", "Statement.AlterTable", "Statement.ViewDefinition",
			"Statement.CreateIndex", "Statement.AlterIndex", "Statement.StatisticsDefinition", "Statement.UpdateStatistics",
			"Statement.FullTextIndexDefinition", "Statement.AlterFullTextIndex",
		];

		/// <summary>
		/// T-SQL's rowset functions, which proposal 7 gives typed table sources of their own.
		/// </summary>
		static readonly HashSet<string> Rowsets = new(StringComparer.OrdinalIgnoreCase)
		{
			"OPENROWSET", "OPENJSON", "OPENXML", "OPENQUERY", "OPENDATASOURCE", "CHANGETABLE", "CONTAINSTABLE", "FREETEXTTABLE", "PREDICT",
		};

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				TableReference source   => Of(source),
				_ when Owned(Owns, old) => throw Inventory.Unconverted(old),
				_                       => null,
			};

			return made is not null;
		}

		static Ast.TableSource Of(TableReference old)
		{
			switch (old)
			{
				case TableReference.Named named:
					return Named(named);

				case TableReference.Derived(var query, var forPath, var name, var columns):
					if (forPath)
						throw BridgeGap.Missing(old, "TableSource.Subquery has no FOR PATH (inventory TableReference.Derived)");

					return new Ast.TableSource.Subquery(QueryOf(query)) { Alias = Alias(old, query, name, columns) };

				case TableReference.FunctionCall(var call, _, _, var schema):
					if (schema.Length > 0 || Rowsets.Contains(call.Name))
						throw BridgeGap.Missing(old, "a typed T-SQL rowset table source (proposal 7)");

					throw BridgeGap.Unwritten(old, "TableSource.Function");

				case TableReference.Joined joined:
					return Joined(joined);

				case TableReference.Parenthesized(var inner):
					return new Ast.TableSource.Parenthesized(Source(inner));

				case TableReference.Pivot:
				case TableReference.Unpivot:
					throw BridgeGap.Missing(old, "TableSource.Pivot and TableSource.Unpivot (proposal 6)");

				case TableReference.OdbcJoin:
					throw BridgeGap.Missing(old, "TableSource.OdbcJoin (proposal 15)");

				case TableReference.Changed:
					throw BridgeGap.Open(old, "inventory TableReference.Changed");

				default:
					throw Inventory.Unconverted(old);
			}
		}

		static Ast.TableSource Named(TableReference.Named old)
		{
			if (old.Server is not null)
				throw BridgeGap.Missing(old, "TableSource.Named has no OPENDATASOURCE server (inventory TableReference.Named)");

			if (old.Hints.Length > 0)
				throw BridgeGap.Missing(old, "TableSource.Named has no table hints (proposal 9)");

			if (old.ForPath)
				throw BridgeGap.Missing(old, "TableSource.Named has no FOR PATH (inventory TableReference.Named)");

			var made = new Ast.TableSource.Named(Values.Name(old, old.Table))
			{
				SystemTime = old.SystemTime is null ? null : SystemTime(old.SystemTime),
				Alias      = Alias(old, null, old.Name, old.Columns),
			};

			return old.Sample is null ? made : made with { Sample = Sample(old.Sample) };
		}

		/// <summary>
		/// The name a source is known by, and whether <c>AS</c> was written before it, which the old tree does
		/// not keep and the text after the source's own part says. <c>AS OF</c> of a system time is not it.
		/// </summary>
		static Ast.Alias? Alias(TableReference old, ISqlSpan? before, string? name, SqlList<string>? columns)
		{
			if (name is null)
				return null;

			var text  = before is null ? Spelled(old) : WordsAfter(before, old);
			var words = text is null ? [] : Words(text);
			var said  = false;

			for (var at = 0; at < words.Length; at++)
				if (words[at] == "AS" && (at + 1 >= words.Length || words[at + 1] != "OF"))
					said = true;

			return new Ast.Alias(Values.Part(old, name), columns is { } list ? Values.Parts(old, list) : null, said);
		}

		/// <summary>
		/// The text from the end of one located node to the end of the node it stands in.
		/// </summary>
		static string? WordsAfter(ISqlSpan node, ISqlSpan within)
		{
			if (source is null || !node.Span.Known || !within.Span.Known || node.Span.IsStale || within.Span.IsStale)
				return null;

			var from = node.Span.End;
			var to   = within.Span.End;

			return from <= to && to <= source.Length ? source.Substring(from, to - from) : null;
		}

		static Ast.SystemTimeSpecification SystemTime(Clause old)
		{
			if (old is not Clause.SystemTime(var kind, var at))
				throw BridgeGap.Open(old, "inventory Clause.SystemTime");

			return (string.Join(' ', Words(kind)), at.Length) switch
			{
				("AS OF", 1)   => new Ast.SystemTimeSpecification.AsOf(Values.Of(at[0])),
				("BETWEEN", 2) => new Ast.SystemTimeSpecification.Between(Values.Of(at[0]), Values.Of(at[1]), null),
				("FROM", 2)    => new Ast.SystemTimeSpecification.FromTo(Values.Of(at[0]), Values.Of(at[1])),
				_              => throw BridgeGap.Missing(old, $"SystemTimeSpecification has no `{kind}` (inventory Clause.SystemTime)"),
			};
		}

		static Ast.SampleClause Sample(Clause old)
		{
			if (old is not Clause.TableSample(var system, var value, var unit, var repeatable))
				throw BridgeGap.Open(old, "inventory Clause.TableSample");

			if (!system)
				throw BridgeGap.Missing(old, "SampleClause.Method is required and T-SQL's SYSTEM may be left out (inventory Clause.TableSample)");

			if (unit is not null)
				throw BridgeGap.Missing(old, "SampleClause has no unit, PERCENT or ROWS (inventory Clause.TableSample)");

			return new Ast.SampleClause(Ast.SampleMethod.System, Values.Of(value), repeatable is null ? null : Values.Of(repeatable));
		}

		static Ast.TableSource Joined(TableReference.Joined old)
		{
			if (old.Hint is not null)
				throw BridgeGap.Unwritten(old, "TableSource.Join.Hint (JoinHint)");

			Ast.JoinKind? kind = old.Kind switch
			{
				SqlJoin.Unspecified => null,
				SqlJoin.Cross       => Ast.JoinKind.Cross,
				SqlJoin.Inner       => Ast.JoinKind.Inner,
				SqlJoin.Left        => Ast.JoinKind.Left,
				SqlJoin.Right       => Ast.JoinKind.Right,
				SqlJoin.Full        => Ast.JoinKind.Full,
				SqlJoin.CrossApply or SqlJoin.OuterApply => throw BridgeGap.Unwritten(old, "JoinKind.CrossApply and JoinKind.OuterApply"),
				_                   => throw BridgeGap.Missing(old, $"JoinKind has no {old.Kind}"),
			};

			if (old.Left is null)
				throw BridgeGap.Open(old, "inventory TableReference.Joined (no left side)");

			Ast.JoinSpecification? specification = (old.On, old.Using) switch
			{
				({ } on, _)        => new Ast.JoinSpecification.On(Values.Of(on)),
				(null, { } using_) => new Ast.JoinSpecification.Using(Values.Parts(old, using_)),
				_                  => null,
			};

			return new Ast.TableSource.Join
			{
				Left          = Source(old.Left),
				Right         = Source(old.Right),
				Kind          = kind,
				Natural       = old.Natural,
				OuterKeyword  = old.Outer,
				Specification = specification,
			};
		}
	}
}
