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
			.. typeof(TableReference).GetNestedTypes().Where(static one => one.IsSubclassOf(typeof(TableReference)) && one != typeof(TableReference.Changed)).Select(NameOf),
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
				TableReference source when Owned(Owns, old) => Of(source),
				_ when Owned(Owns, old) => throw Inventory.Unconverted(old),
				_                       => null,
			};

			return made is not null;
		}

		static Ast.TableSource Of(TableReference old)
		{
			switch (old)
			{
				// A table variable: the name keeps its `@`; what only a table has is not its.
				case TableReference.Named { Table: var table } named when table.StartsWith('@'):
					if (named is not { SystemTime: null, ForPath: false, Sample: null, Hints.Length: 0, Server: null, Columns: null } || table.Contains('.'))
						throw BridgeGap.Open(old, "inventory TableReference.Named (a table variable with more than a name and an alias)");

					return new Ast.TableSource.Variable(new Ast.Identifier(table)) { Alias = Alias(old, null, named.Name, null) };

				case TableReference.Named named:
					return Named(named);

				case TableReference.Derived(var query, var forPath, var name, var columns):
					return new Ast.TableSource.Subquery(QueryOf(query)) { Alias = Alias(old, query, name, columns), ForPath = forPath };

				case TableReference.FunctionCall(var call, var name, var columns, var schema):
					if (schema.Length > 0 || Rowsets.Contains(call.Name))
						return Rowset((TableReference.FunctionCall)old);

					if (Values.Function(call) is not Ast.Expression.Invocation invocation)
						throw BridgeGap.Open(old, "inventory TableReference.FunctionCall (a call that is not a plain invocation)");

					return new Ast.TableSource.Function(invocation, Alias(old, call, name, columns));

				case TableReference.Joined joined:
					return Joined(joined);

				case TableReference.Parenthesized(var inner):
					return new Ast.TableSource.Parenthesized(Source(inner));

				case TableReference.Pivot(var of, var aggregate, var column, var values, var name):
					if (Values.Of(aggregate) is not Ast.Expression.Invocation summed)
						throw BridgeGap.Open(old, "inventory TableReference.Pivot (an aggregate that is not a call)");

					return new Ast.TableSource.Pivot(Source(of), summed, Values.Name(old, column), Each(values, v => Values.Name(old, v)), Closing(old, name) ?? throw BridgeGap.Open(old, "inventory TableReference.Pivot (without its alias)"));

				case TableReference.Unpivot(var of, var value, var column, var values, var name):
					return new Ast.TableSource.Unpivot(Source(of), Values.Name(old, value), Values.Name(old, column), Each(values, v => Values.Name(old, v)), Closing(old, name) ?? throw BridgeGap.Open(old, "inventory TableReference.Pivot (without its alias)"));

				case TableReference.OdbcJoin(var inner):
					return new Ast.TableSource.OdbcJoin(Source(inner));

				default:
					throw Inventory.Unconverted(old);
			}
		}

		static Ast.TableSource Named(TableReference.Named old)
		{
			var server = old.Server switch
			{
				null => null,
				{ Arguments: [var provider, var init] } => new Ast.OpenDataSource(Values.Of(provider), Values.Of(init)),
				_ => throw BridgeGap.Open(old, "inventory TableReference.Named (an OPENDATASOURCE that is not a provider and a string)"),
			};

			// A hint against the sample, `TABLESAMPLE (10) (NOLOCK)`, and the old `WITH (0)`: decided not to be built.
			if (old.Sample is not null && old.Hints.Length > 0 && Spelled(old) is { } text && !Words(text).Contains("WITH"))
				throw BridgeGap.Open(old, "inventory Clause.Hint (a hint written against the sample)");

			var made = new Ast.TableSource.Named(Values.Name(old, old.Table))
			{
				DataSource = server,
				SystemTime = old.SystemTime is null ? null : SystemTime(old.SystemTime),
				ForPath    = old.ForPath,
				Alias      = Alias(old, null, old.Name, old.Columns),
				Hints      = Each(old.Hints, static one => one is Clause.Hint(var text) && SqlTokens.Of(text) is [{ Kind: SqlTokens.Kind.Number }]
					? throw BridgeGap.Open(one, "inventory Clause.Hint (the old WITH (n))")
					: Queries.TableHint(one)),
			};

			return old.Sample is null ? made : Sample(made, old.Sample);
		}

		/// <summary>
		/// A rowset function as the typed table source it is (proposal 7), from the call the old tree keeps:
		/// its arguments, and its schema where a WITH was written.
		/// </summary>
		static Ast.TableSource Rowset(TableReference.FunctionCall old)
		{
			var call      = old.Function;
			var arguments = call.Arguments;
			var name      = call.Name.ToUpperInvariant();

			// The alias follows the schema where there is one, and the call where there is not.
			var alias = Alias(old, old.Schema.Length > 0 ? old.Schema[^1] : call, old.Name, old.Columns);

			switch (name)
			{
				case "OPENJSON" when arguments.Length is 1 or 2:
					return new Ast.TableSource.OpenJson(
						Values.Of(arguments[0]),
						arguments.Length > 1 ? Values.Of(arguments[1]) : null,
						old.Schema.Length > 0 ? Schema(old.Schema) : null,
						alias);

				case "OPENXML" when arguments.Length is 2 or 3:
					// `WITH table` is a schema of one column with no type, which no column written out is.
					var table = old.Schema is [Clause.JsonColumn { Type: null } named] ? Values.Name(old, named.Name) : null;

					return new Ast.TableSource.OpenXml(
						Values.Of(arguments[0]),
						Values.Of(arguments[1]),
						arguments.Length > 2 ? Values.Of(arguments[2]) : null,
						table is null && old.Schema.Length > 0 ? Schema(old.Schema) : null,
						table,
						alias);

				case "OPENQUERY" when arguments is [Expression.ColumnReference(var server), var query] && old.Schema.Length == 0:
					return new Ast.TableSource.OpenQuery(Values.Part(old, server), Values.Of(query), alias);

				case "OPENROWSET" when arguments is [Expression.Prefixed(var bulk, var file), ..] && bulk.Equals("BULK", StringComparison.OrdinalIgnoreCase):
					return new Ast.TableSource.OpenRowsetBulk(
						Values.Of(file),
						Each(arguments[1..], one => BulkOption(old, one)),
						old.Schema.Length > 0 ? Schema(old.Schema) : null,
						alias);

				case "OPENROWSET" when arguments.Length == 3 && old.Schema.Length == 0:
					return arguments[1] switch
					{
						Expression.Pieced([var source, var user, var password]) =>
							new Ast.TableSource.OpenRowset(Values.Of(arguments[0]), null, new Ast.RowsetLogin(Values.Of(source), Values.Of(user), Values.Of(password)), Values.Of(arguments[2]), alias),
						Expression.Pieced =>
							throw BridgeGap.Open(old, "inventory TableReference.FunctionCall (OPENROWSET's login in other than three parts)"),
						var provider =>
							new Ast.TableSource.OpenRowset(Values.Of(arguments[0]), Values.Of(provider), null, Values.Of(arguments[2]), alias),
					};

				case "PREDICT":
					return Predict(old, alias);

				case "CHANGETABLE" when old.Schema.Length == 0:
					return ChangeTable(old, alias);

				case "CONTAINSTABLE" or "FREETEXTTABLE" when old.Schema.Length == 0:
					return FullTextTable(old, name, alias);

				default:
					throw BridgeGap.Open(old, $"inventory TableReference.FunctionCall ({name} with these arguments)");
			}
		}

		/// <summary>
		/// One option of OPENROWSET BULK: a word (<c>SINGLE_BLOB</c>) or a word and its value (<c>FORMAT = 'CSV'</c>).
		/// </summary>
		static Ast.Option BulkOption(TableReference old, Expression option)
		{
			return option switch
			{
				Expression.NamedArgument(var name, var value)                 => new Ast.Option(Values.Part(old, name), Values.Of(value), []),
				Expression.ColumnReference(var word) when !word.Contains('.') => new Ast.Option(Values.Part(old, word), null, []),
				Expression.RowsetOrder(var by, var unique)                    => new Ast.Option(new Ast.Identifier("ORDER"), null, [])
				{
					Order = new Ast.RowOrder(Queries.Order(new Clause.OrderBy(by, null, null)).Items, unique),
				},
				_                                                             => throw BridgeGap.Open(old, "inventory TableReference.FunctionCall (an OPENROWSET BULK option that is not a word or a value)"),
			};
		}

		/// <summary>
		/// <c>PREDICT (MODEL = m, DATA = t AS d [, RUNTIME = ONNX]) WITH (…)</c>, whose arguments the old tree keeps
		/// as named ones.
		/// </summary>
		static Ast.TableSource Predict(TableReference.FunctionCall old, Ast.Alias? alias)
		{
			if (old.Function.Arguments is not [Expression.NamedArgument(var model, var value), Expression.NamedArgument(var data, Expression.Aliased(Expression.ColumnReference(var source), var named, var written)), .. var rest]
				|| !model.Equals("MODEL", StringComparison.OrdinalIgnoreCase) || !data.Equals("DATA", StringComparison.OrdinalIgnoreCase)
				|| old.Schema.Length == 0)
				throw BridgeGap.Open(old, "inventory TableReference.FunctionCall (a PREDICT that is not MODEL, DATA AS and a schema)");

			Ast.Identifier? runtime = rest switch
			{
				[]                                                                                                          => null,
				[Expression.NamedArgument(var word, Expression.ColumnReference(var engine))] when word.Equals("RUNTIME", StringComparison.OrdinalIgnoreCase) => Values.Part(old, engine),
				_                                                                                                           => throw BridgeGap.Open(old, "inventory TableReference.FunctionCall (a PREDICT argument after DATA that is not RUNTIME)"),
			};

			var scored = new Ast.TableSource.Named(Values.Name(old, source)) { Alias = new Ast.Alias(Values.Part(old, named), null, written) };

			return new Ast.TableSource.Predict(Values.Of(value), scored, runtime, Each(old.Schema, PredictColumn), alias);
		}

		/// <summary>
		/// A column of PREDICT's <c>WITH (…)</c>, whose type the old tree keeps with its collation and nullability
		/// after it: a name, a type, a collation and <c>NOT NULL</c>, as the standard's column definition holds them.
		/// </summary>
		static Ast.ColumnDefinition PredictColumn(Clause old)
		{
			if (old is not Clause.JsonColumn(var name, { } text, null, false, null))
				throw BridgeGap.Open(old, "inventory Clause.JsonColumn (a PREDICT column)");

			return TypedColumn(old, name, text);
		}

		/// <summary>
		/// A column's name and the text the old tree keeps after it — a type, perhaps a collation and a nullability,
		/// or nothing where the type was left out — as the standard's column definition holds them: <c>NOT NULL</c> a
		/// constraint, and an explicit <c>NULL</c> a gap, decided with CREATE TABLE's column options.
		/// </summary>
		internal static Ast.ColumnDefinition TypedColumn(Clause old, string name, string? text)
		{
			if (text is null)
				return new Ast.ColumnDefinition(Values.Part(old, name), null, null, [], null);

			var words = Words(text);
			var notNull = words is [.., "NOT", "NULL"];

			if (!notNull && words is [.., "NULL"])
				throw BridgeGap.Open(old, "inventory Clause.ColumnOption (an explicit NULL, decided with CREATE TABLE's column options)");

			if (notNull)
				text = text[..text.LastIndexOf("NOT", StringComparison.OrdinalIgnoreCase)].TrimEnd();

			var (type, collation) = Collated(old, text);

			return new Ast.ColumnDefinition(Values.Part(old, name), type, null, notNull ? [new Ast.Constraint.NotNull()] : [], collation);
		}

		/// <summary>
		/// A type the old tree keeps with its <c>COLLATE</c> after it: the type, and the collation.
		/// </summary>
		static (Ast.DataType Type, Ast.CollationName? Collation) Collated(Clause old, string text)
		{
			var at = text.IndexOf(" COLLATE ", StringComparison.OrdinalIgnoreCase);

			return at < 0
				? (Values.Type(old, text), null)
				: (Values.Type(old, text[..at].TrimEnd()), new Ast.CollationName(Values.Name(old, text[(at + " COLLATE ".Length)..].Trim())));
		}

		/// <summary>
		/// A rowset function's <c>WITH (…)</c>: each column's name and type, its path or ordinal, and AS JSON.
		/// </summary>
		static SqlList<Ast.RowsetColumn> Schema(SqlList<Clause> schema)
		{
			return Each(schema, static one =>
			{
				if (one is not Clause.JsonColumn(var name, var type, var path, var json, var ordinal) || type is null)
					throw BridgeGap.Open(one, "inventory Clause.JsonColumn (a column without a type)");

				var (typed, collation) = Collated(one, type);

				return new Ast.RowsetColumn(
					Values.Part(one, name),
					typed,
					path,
					json,
					ordinal is null ? null
						: !int.TryParse(ordinal, out var number) ? throw BridgeGap.Open(one, $"inventory Clause.JsonColumn (ordinal {ordinal})")
						: number.ToString(System.Globalization.CultureInfo.InvariantCulture) != ordinal ? throw BridgeGap.Missing(one, "RowsetColumn.Ordinal is a number, and keeps no spelling of it such as `00`")
						: number) { Collation = collation };
			});
		}

		/// <summary>
		/// The alias after a PIVOT's or an UNPIVOT's brackets, and whether AS was written before it: the text
		/// after the last bracket of the source.
		/// </summary>
		static Ast.Alias? Closing(TableReference old, string? name)
		{
			if (name is null)
				return null;

			// The words after the rotation's last bracket, read as tokens: a bracket in a quoted alias is not one.
			var said = Recover("Alias.AsKeyword of a table source", Spelled(old), static text =>
			{
				var tokens = SqlTokens.Of(text);
				var close  = tokens.FindLastIndex(static one => one is { Kind: SqlTokens.Kind.Punctuation, Text: ")" });

				return close >= 0 && close + 1 < tokens.Count && tokens[close + 1] is { Kind: SqlTokens.Kind.Word } word && word.Text.Equals("AS", StringComparison.OrdinalIgnoreCase);
			}, false);

			return new Ast.Alias(Values.Part(old, name), null, said);
		}

		/// <summary>
		/// The name a source is known by, and whether <c>AS</c> was written before it, which the old tree does
		/// not keep and the text after the source's own part says. <c>AS OF</c> of a system time is not it.
		/// </summary>
		static Ast.Alias? Alias(TableReference old, ISqlSpan? before, string? name, SqlList<string>? columns)
		{
			if (name is null)
				return null;

			var said = Recover("Alias.AsKeyword of a table source", before is null ? Spelled(old) : WordsAfter(before, old), static text =>
			{
				// A word AS, not a name `"as"` or `[as]`, and not the AS of FOR SYSTEM_TIME AS OF.
				var words = Words(text);

				for (var at = 0; at < words.Length; at++)
					if (words[at] == "AS" && (at + 1 >= words.Length || words[at + 1] != "OF"))
						return true;

				return false;
			}, false);

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
				("CONTAINED IN", 2) => new Ast.SystemTimeSpecification.ContainedIn(Values.Of(at[0]), Values.Of(at[1])),
				("ALL", 0)     => new Ast.SystemTimeSpecification.All(),
				_              => throw BridgeGap.Missing(old, $"SystemTimeSpecification has no `{kind}` (inventory Clause.SystemTime)"),
			};
		}

		/// <summary>
		/// <c>CHANGETABLE (CHANGES t, v [, FORCESEEK])</c> or <c>CHANGETABLE (VERSION t, (cols), (vals) [, FORCESEEK])</c>,
		/// which the old tree keeps as a call whose first argument carries the word.
		/// </summary>
		static Ast.TableSource ChangeTable(TableReference.FunctionCall old, Ast.Alias? alias)
		{
			var arguments = old.Function.Arguments;
			var seek      = arguments is [.., Expression.ColumnReference(var last)] && last.Equals("FORCESEEK", StringComparison.OrdinalIgnoreCase);
			var count     = arguments.Length - (seek ? 1 : 0);

			switch (arguments[0])
			{
				case Expression.Prefixed(var word, Expression.ColumnReference(var table)) when word.Equals("CHANGES", StringComparison.OrdinalIgnoreCase) && count == 2:
					return new Ast.TableSource.ChangeTableChanges(Values.Name(old, table), Values.Of(arguments[1]), seek, alias);

				case Expression.Prefixed(var word, Expression.ColumnReference(var table)) when word.Equals("VERSION", StringComparison.OrdinalIgnoreCase) && count == 3
					&& arguments[1] is Expression.RowValueConstructor(var columns) && arguments[2] is Expression.RowValueConstructor(var values):
					return new Ast.TableSource.ChangeTableVersion(
						Values.Name(old, table),
						Each(columns, one => one is Expression.ColumnReference(var column) && !column.Contains('.') ? Values.Part(old, column) : throw BridgeGap.Open(old, "inventory TableReference.FunctionCall (a CHANGETABLE key column that is not one name)")),
						Each(values, Values.Of),
						seek,
						alias);

				default:
					throw BridgeGap.Open(old, "inventory TableReference.FunctionCall (CHANGETABLE with these arguments)");
			}
		}

		/// <summary>
		/// <c>CONTAINSTABLE</c> or <c>FREETEXTTABLE</c>: a table, the columns searched, what for, perhaps a language,
		/// perhaps a count, which the old tree keeps as a call's arguments in that order.
		/// </summary>
		static Ast.TableSource FullTextTable(TableReference.FunctionCall old, string name, Ast.Alias? alias)
		{
			if (old.Function.Arguments is not [Expression.ColumnReference(var table), var searched, var search, .. var rest] || rest.Length > 2)
				throw BridgeGap.Open(old, $"inventory TableReference.FunctionCall ({name} with these arguments)");

			Ast.Expression? language = null;
			Ast.Expression? top      = null;

			foreach (var one in rest)
			{
				if (one is Expression.Prefixed(var word, var value) && word.Equals("LANGUAGE", StringComparison.OrdinalIgnoreCase) && language is null && top is null)
					language = Values.Of(value);
				else if (one is not Expression.Prefixed && top is null)
					top = Values.Of(one);
				else
					throw BridgeGap.Open(old, $"inventory TableReference.FunctionCall ({name} with these arguments)");
			}

			var bracketed = searched is Expression.RowValueConstructor;
			SqlList<Expression> items = searched is Expression.RowValueConstructor(var list) ? list : [searched];

			var columns = Each(items, one => one switch
			{
				Expression.ColumnReference("*") => new Ast.Expression.Asterisk(),
				Expression.ColumnReference(var column) when !column.Contains('.') => new Ast.Expression.Reference(new Ast.QualifiedName([Values.Part(old, column)])),
				Expression.RoutineInvocation when !bracketed => Values.Of(one),
				_ => throw BridgeGap.Open(old, $"inventory TableReference.FunctionCall ({name} searching a column that is not one name, *, or a property)"),
			});

			return name == "CONTAINSTABLE"
				? new Ast.TableSource.ContainsTable(Values.Name(old, table), columns, Values.Of(search), language, top, alias) { ColumnsBracketed = bracketed }
				: new Ast.TableSource.FreetextTable(Values.Name(old, table), columns, Values.Of(search), language, top, alias) { ColumnsBracketed = bracketed };
		}

		/// <summary>
		/// A named table with its <c>TABLESAMPLE [SYSTEM] (n [PERCENT | ROWS]) [REPEATABLE (s)]</c>: the standard's
		/// sample, and what T-SQL adds to it on the table.
		/// </summary>
		static Ast.TableSource.Named Sample(Ast.TableSource.Named made, Clause old)
		{
			if (old is not Clause.TableSample(var system, var value, var unit, var repeatable))
				throw BridgeGap.Open(old, "inventory Clause.TableSample");

			Ast.SampleUnit? counted = unit?.ToUpperInvariant() switch
			{
				null      => null,
				"PERCENT" => Ast.SampleUnit.Percent,
				"ROWS"    => Ast.SampleUnit.Rows,
				_         => throw BridgeGap.Open(old, $"inventory Clause.TableSample (unit {unit})"),
			};

			return made with
			{
				Sample = new Ast.SampleClause(Ast.SampleMethod.System, Values.Of(value), repeatable is null ? null : Values.Of(repeatable))
				{
					MethodOmitted = !system,
					Unit          = counted,
				},
			};
		}

		static Ast.TableSource Joined(TableReference.Joined old)
		{
			Ast.JoinHint? hint = old.Hint?.ToUpperInvariant() switch
			{
				null     => null,
				"LOOP"   => Ast.JoinHint.Loop,
				"HASH"   => Ast.JoinHint.Hash,
				"MERGE"  => Ast.JoinHint.Merge,
				"REMOTE" => Ast.JoinHint.Remote,
				_        => throw BridgeGap.Open(old, $"inventory TableReference.Joined (join hint {old.Hint})"),
			};

			Ast.JoinKind? kind = old.Kind switch
			{
				SqlJoin.Unspecified => null,
				SqlJoin.Cross       => Ast.JoinKind.Cross,
				SqlJoin.Inner       => Ast.JoinKind.Inner,
				SqlJoin.Left        => Ast.JoinKind.Left,
				SqlJoin.Right       => Ast.JoinKind.Right,
				SqlJoin.Full        => Ast.JoinKind.Full,
				SqlJoin.CrossApply  => Ast.JoinKind.CrossApply,
				SqlJoin.OuterApply  => Ast.JoinKind.OuterApply,
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
				Hint          = hint,
			};
		}
	}
}
