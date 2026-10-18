using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

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
			"Statement.TypeDefinition",
			"Statement.SynonymDefinition",
			"Statement.SequenceDefinition",
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
				_ when Owned(Owns, old) => throw Unconverted(old),
				_                       => null,
			};

			return made is not null;
		}

		/// <summary>
		/// A source a rule read whole, on the common tree, built at once and held as a <c>TableReference.Built</c>: the
		/// record its parts were gathered into — a pivot hung on the source it turns, the joins after a source hung on it
		/// — carried across as the bridge carries one. The record is told the rule's extent first, as it was told when the
		/// rule handed it back, since its conversion reads the source's own text.
		/// </summary>
		internal static TableReference Sourced(TableReference read, SqlSpan at)
		{
			// The joins after a source, where none were: the source itself, built by its own rule.
			if (read is TableReference.Built)
				return read;

			// Only where the reading locates: a plain reading's records have no span, and its checks that read the text by one
			// are not made (Reread).
			if (!unlocated)
				read.Locate(at.At, at.Length);

			Building(read, static read => Of(read), out var made, out var gap, out var taken);

			var shape = read switch
			{
				TableReference.Joined        => TableReference.Built.Kind.Joined,
				TableReference.Parenthesized => TableReference.Built.Kind.Parenthesized,
				TableReference.OdbcJoin      => TableReference.Built.Kind.OdbcJoin,
				TableReference.Pivot         => TableReference.Built.Kind.Pivot,
				TableReference.Unpivot       => TableReference.Built.Kind.Unpivot,
				_                            => TableReference.Built.Kind.Other,
			};

			return new TableReference.Built(made, gap, taken, NameOf(read), shape, Syntax.Bracketable(read));
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
					// AI_GENERATE_CHUNKS from level 170 names its arguments; below, it is a function of the user's.
					if (schema.Length > 0 || Rowsets.Contains(call.Name) || call is { Name: var chunks, Arguments: [Expression.NamedArgument, ..] } && chunks.Equals("AI_GENERATE_CHUNKS", StringComparison.OrdinalIgnoreCase))
						return Rowset((TableReference.FunctionCall)old);

					// VariableSource: `@v.f (1)`, a method on a variable, which the old tree keeps as one name; a variable's
					// name has no dot, so the first one ends it.
					// The method is the type's own, never the built-in of its name: `@v.DATEPART (yy, x)` reads its arguments
					// as values.
					var method = call.Name.StartsWith('@') ? call.Name.IndexOf('.') : -1;

					var invocation = method >= 0
						? new Ast.Expression.Invocation(new Ast.QualifiedName([Values.Part(old, call.Name.Substring(method + 1))]), Each(call.Arguments, Values.Argument))
						: Values.Function(call) as Ast.Expression.Invocation
							?? throw BridgeGap.Open(old, "inventory TableReference.FunctionCall (a call that is not a plain invocation)");

					// The method's call is built inside VariableSource and has no place in the text of its own: the source's
					// whole text is read for the AS instead, outside the brackets.
					return new Ast.TableSource.Function(invocation, Alias(old, method < 0 ? call : null, name, columns))
					{
						Variable = method < 0 ? null : new Ast.Identifier(call.Name.Substring(0, method)),
					};

				case TableReference.Joined joined:
					return Joined(joined);

				case TableReference.Parenthesized(var inner):
					return new Ast.TableSource.Parenthesized(Source(inner));

				case TableReference.Pivot(var of, var aggregate, var column, var values, var name):
					if (Values.Of(aggregate) is not Ast.Expression.Invocation summed)
						throw BridgeGap.Open(old, "inventory TableReference.Pivot (an aggregate that is not a call)");

					return new Ast.TableSource.Pivot(Source(of), summed, Values.Name(old, column), Names(old, values), Closing(old, name) ?? throw BridgeGap.Open(old, "inventory TableReference.Pivot (without its alias)"));

				case TableReference.Unpivot(var of, var value, var column, var values, var name):
					return new Ast.TableSource.Unpivot(Source(of), Values.Name(old, value), Values.Name(old, column), Names(old, values), Closing(old, name) ?? throw BridgeGap.Open(old, "inventory TableReference.Pivot (without its alias)"));

				case TableReference.OdbcJoin(var inner):
					return new Ast.TableSource.OdbcJoin(Source(inner));

				default:
					throw Unconverted(old);
			}
		}

		/// <summary>A rotation's names, each split into its parts; apart from <see cref="Of"/>, whose every call would make the closure.</summary>
		static SqlList<Ast.QualifiedName> Names(TableReference old, SqlList<string> names)
		{
			return Each(names, one => Values.Name(old, one));
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
					var table = old.Schema is [Clause.JsonColumn { Type: null } named] ? Values.Name(old, named.Name) : (Ast.QualifiedName?)null;

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

				case "AI_GENERATE_CHUNKS" when old.Schema.Length == 0 && old.Columns is null:
					return Chunks(old, alias);

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
		/// <c>AI_GENERATE_CHUNKS (SOURCE = v, CHUNK_TYPE = FIXED, CHUNK_SIZE = v [, OVERLAP = v [, ENABLE_CHUNK_SET_ID = v]])</c>,
		/// whose arguments the old tree keeps as named ones, in the order ChunksFunction reads them.
		/// </summary>
		static Ast.TableSource Chunks(TableReference.FunctionCall old, Ast.Alias? alias)
		{
			string[] names = ["SOURCE", "CHUNK_TYPE", "CHUNK_SIZE", "OVERLAP", "ENABLE_CHUNK_SET_ID"];
			var arguments  = old.Function.Arguments;

			if (arguments.Length is < 3 or > 5 || arguments[1] is not Expression.NamedArgument(_, Expression.ColumnReference(var type))
				|| Enumerable.Range(0, arguments.Length).Any(at => arguments[at] is not Expression.NamedArgument(var name, _) || !name.Equals(names[at], StringComparison.OrdinalIgnoreCase)))
				throw BridgeGap.Open(old, "inventory TableReference.FunctionCall (AI_GENERATE_CHUNKS with these arguments)");

			Ast.Expression Value(int at)
			{
				return Values.Of(((Expression.NamedArgument)arguments[at]).Value);
			}

			return new Ast.TableSource.AiGenerateChunks(Value(0), Values.Part(old, type), Value(2))
			{
				Overlap          = arguments.Length > 3 ? Value(3) : null,
				EnableChunkSetId = arguments.Length > 4 ? Value(4) : null,
				Alias            = alias,
			};
		}

		/// <summary>
		/// <c>PREDICT (MODEL = m, DATA = t AS d [, RUNTIME = ONNX]) WITH (…)</c>, whose arguments the old tree keeps
		/// as named ones: the model and the data in either order, the data a table or a query with its alias.
		/// </summary>
		static Ast.TableSource Predict(TableReference.FunctionCall old, Ast.Alias? alias)
		{
			var arguments = old.Function.Arguments;
			var dataFirst = arguments is [Expression.NamedArgument(var leading, _), ..] && leading.Equals("DATA", StringComparison.OrdinalIgnoreCase);

			if (arguments.Length < 2 || arguments[dataFirst ? 1 : 0] is not Expression.NamedArgument(var model, var value) || arguments[dataFirst ? 0 : 1] is not Expression.NamedArgument(var data, Expression.Aliased(var source, var named, var written))
				|| source is not (Expression.ColumnReference or Expression.Subquery)
				|| !model.Equals("MODEL", StringComparison.OrdinalIgnoreCase) || !data.Equals("DATA", StringComparison.OrdinalIgnoreCase)
				|| old.Schema.Length == 0)
				throw BridgeGap.Open(old, "inventory TableReference.FunctionCall (a PREDICT that is not MODEL, DATA AS and a schema)");

			var rest = arguments.Skip(2).ToList();

			Ast.Identifier? runtime = rest switch
			{
				[]                                                                                                          => null,
				[Expression.NamedArgument(var word, Expression.ColumnReference(var engine))] when word.Equals("RUNTIME", StringComparison.OrdinalIgnoreCase) => Values.Part(old, engine),
				_                                                                                                           => throw BridgeGap.Open(old, "inventory TableReference.FunctionCall (a PREDICT argument after DATA that is not RUNTIME)"),
			};

			var called = new Ast.Alias(Values.Part(old, named), null, written);
			Ast.TableSource scored = source is Expression.ColumnReference(var table)
				? new Ast.TableSource.Named(Values.Name(old, table)) { Alias = called }
				: new Ast.TableSource.Subquery(((Ast.Expression.Subquery)Values.Of(source)).Query) { Alias = called };

			return new Ast.TableSource.Predict(Values.Of(value), scored, runtime, Each(old.Schema, PredictColumn), alias) { DataFirst = dataFirst };
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
		/// or nothing where the type was left out — as T-SQL's column holds them: the collation and the nullability
		/// in its options, in the order written, <c>NOT NULL</c> a constraint and an explicit <c>NULL</c> its own.
		/// </summary>
		internal static Ast.ColumnDefinition TypedColumn(Clause old, string name, string? text)
		{
			if (text is null)
				return new Ast.ColumnDefinition(Values.Part(old, name), null, null, [], null);

			var words = Words(text);
			Ast.ColumnOption? nullability = null;

			if (words is [.., "NOT", "NULL"])
			{
				nullability = new Ast.ColumnOption.Constraint(new Ast.Constraint.NotNull());
				text        = text[..text.LastIndexOf("NOT", StringComparison.OrdinalIgnoreCase)].TrimEnd();
			}
			else if (words is [.., "NULL"])
			{
				nullability = new Ast.ColumnOption.Null();
				text        = text[..text.LastIndexOf("NULL", StringComparison.OrdinalIgnoreCase)].TrimEnd();
			}

			var (type, collation) = Collated(old, text);
			var options = new List<Ast.ColumnOption>();

			if (collation is not null)
				options.Add(new Ast.ColumnOption.Collation(collation));

			if (nullability is not null)
				options.Add(nullability);

			return new Ast.ColumnDefinition(Values.Part(old, name), type, null, [], null) { Options = options.ToSqlList() };
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
				// A word AS, not a name `"as"` or `[as]`, not the AS of FOR SYSTEM_TIME AS OF, and not one inside
				// brackets, a method's arguments (`@v.f (CAST (1 AS INT))`).
				var words = Words(text);
				var depth = 0;

				for (var at = 0; at < words.Count; at++)
				{
					depth += words[at] switch { "(" => 1, ")" => -1, _ => 0 };

					if (depth == 0 && words[at] == "AS" && (at + 1 >= words.Count || words[at + 1] != "OF"))
						return true;
				}

				return false;
			}, false);

			return new Ast.Alias(Values.Part(old, name), columns is { } list ? Values.Parts(old, list) : null, said);
		}

		/// <summary>
		/// The text from the end of one located node to the end of the node it stands in.
		/// </summary>
		static string? WordsAfter(ISqlSpan node, ISqlSpan within)
		{
			if (!Cuttable(node.Span) || !Cuttable(within.Span))
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

		// ── CREATE TABLE, CREATE TABLE AS SELECT, ALTER TABLE (proposals 16, 17, 20) ────────────────

		/// <summary>
		/// A table declared: its name, its kind, its elements, placements and options. A temporary table is its name.
		/// </summary>
		internal static Statement CreateTable(string name, string? kind, SqlList<Clause> elements, SqlList<Clause> placements, SqlList<Clause> options)
		{
			var old = new Defining("Statement.TableDefinition", name);

			return Hold(old, (kind, elements, placements, options), static (old, state) => new Ast.Statement.CreateTable
			{
				Name       = Values.Name(old, old.Name),
				Contents   = new Ast.TableContents.Elements(Elements(state.elements)),
				Kind       = Kind(old, state.kind),
				Placements = Each(state.placements, Placement),
				Options    = Each(state.options, Option),
			});
		}

		static Ast.TableKind? Kind(object owner, string? kind)
		{
			return kind?.ToUpperInvariant() switch
			{
				null        => null,
				"FILETABLE" => Ast.TableKind.FileTable,
				"NODE"      => Ast.TableKind.Node,
				"EDGE"      => Ast.TableKind.Edge,
				_           => throw BridgeGap.Open(owner, $"inventory Statement.TableDefinition (AS {kind})"),
			};
		}

		/// <summary>
		/// A table made of a query, Synapse's: the column names, the options before the AS, and the query with its hints,
		/// which writes no <c>WITH [NO] DATA</c>.
		/// </summary>
		internal static Statement CreateTableAsSelect(string name, SqlList<string>? columns, SqlList<Clause> options, Statement body)
		{
			var old = new Defining("Statement.CreateTableAsSelect", name) { Options = options };

			return Hold(old, (columns, options, body), static (old, state) =>
			{
				if (StatementOf(state.body) is not Ast.Statement.Select select)
					throw BridgeGap.Open(old, "inventory Statement.CreateTableAsSelect (a query that is no SELECT)");

				return new Ast.Statement.CreateTable
				{
					Name     = Values.Name(old, old.Name),
					Contents = new Ast.TableContents.AsQuery(Values.Parts(old, state.columns), select.Query, null) { Hints = select.Hints },
					Options  = Each(state.options, GenericOption),
				};
			});
		}

		internal static SqlList<Ast.TableElement> Elements(SqlList<Clause> elements)
		{
			return Each(elements, Element);
		}

		/// <summary>One element of a table's body: a column, a constraint, an index, a period.</summary>
		static Ast.TableElement Element(Clause old)
		{
			switch (old)
			{
				case Clause.ColumnDefinition column:
					return new Ast.TableElement.Column(Column(column));

				case Clause.ConstraintDefinition { Kind: "PERIOD FOR SYSTEM_TIME", Columns: [Clause.SortSpecification(Expression.ColumnReference(var begin), SqlOrder.Unspecified), Clause.SortSpecification(Expression.ColumnReference(var end), SqlOrder.Unspecified)], Name: null }:
					return new Ast.TableElement.Period(new Ast.PeriodDefinition(Ast.PeriodKind.SystemTime, null, Values.Part(old, begin), Values.Part(old, end)));

				case Clause.ConstraintDefinition { Kind: "INDEX" or "UNIQUE INDEX" } index:
					return new Ast.TableElement.Index(Index(index));

				case Clause.ConstraintDefinition constraint:
					return new Ast.TableElement.TableConstraint(Constraint(constraint, table: true));

				default:
					throw BridgeGap.Open(old, $"inventory Statement.TableDefinition (an element {NameOf(old)})");
			}
		}

		/// <summary>
		/// A column as T-SQL declares it: its name, its type where it has one, and everything said about it after
		/// that in <see cref="Ast.ColumnDefinition.Options"/>, in the order written; the standard's members empty.
		/// </summary>
		static Ast.ColumnDefinition Column(Clause.ColumnDefinition old)
		{
			var options = new List<Ast.ColumnOption>();

			if (old.Computed is { } computed)
				options.Add(new Ast.ColumnOption.Computed(Values.Of(computed)));

			foreach (var one in old.Options)
				options.AddRange(ColumnOptions(one));

			return new Ast.ColumnDefinition(Values.Part(old, old.Name), old.Type is null ? null : Values.Type(old, old.Type), null, [], null)
			{
				Options = options.ToSqlList(),
			};
		}

		/// <summary>
		/// What one thing said about a column becomes: one option, or two where <c>HIDDEN</c> was written after
		/// <c>GENERATED ALWAYS AS ROW START</c>, which the old tree keeps inside it.
		/// </summary>
		static IEnumerable<Ast.ColumnOption> ColumnOptions(Clause old)
		{
			switch (old)
			{
				case Clause.ColumnOption { Kind: var kind, ConstraintName: var name } option:
					var words = kind.ToUpperInvariant();

					if (name is not null && words is not ("NULL" or "NOT NULL"))
						throw BridgeGap.Open(old, $"inventory Clause.ColumnOption (a name on {kind})");

					if (words.StartsWith("GENERATED ALWAYS AS ", StringComparison.Ordinal))
					{
						yield return new Ast.ColumnOption.GeneratedAlways(words["GENERATED ALWAYS AS ".Length..] switch
						{
							"ROW START"             => Ast.GeneratedAlwaysKind.RowStart,
							"ROW END"               => Ast.GeneratedAlwaysKind.RowEnd,
							"TRANSACTION_ID START"  => Ast.GeneratedAlwaysKind.TransactionIdStart,
							"TRANSACTION_ID END"    => Ast.GeneratedAlwaysKind.TransactionIdEnd,
							"SEQUENCE_NUMBER START" => Ast.GeneratedAlwaysKind.SequenceNumberStart,
							"SEQUENCE_NUMBER END"   => Ast.GeneratedAlwaysKind.SequenceNumberEnd,
							_                       => throw BridgeGap.Open(old, $"inventory Clause.ColumnOption ({kind})"),
						});

						if (option.Options is [Clause.Option { Name: var hidden }] && hidden.Equals("HIDDEN", StringComparison.OrdinalIgnoreCase))
							yield return new Ast.ColumnOption.Hidden();
						else if (option.Options.Length > 0)
							throw BridgeGap.Open(old, $"inventory Clause.ColumnOption ({kind} with more than HIDDEN after it)");

						yield break;
					}

					yield return ColumnOption(option, words, name);
					break;

				case Clause.ConstraintDefinition { Kind: "DEFAULT", Check: { } value, ForColumn: null } @default when WithValues(@default) is { } valued:
					yield return new Ast.ColumnOption.Default(Values.Of(value)) { Name = @default.Name is { } named ? Values.Part(old, named) : null, WithValues = valued };
					break;

				case Clause.ConstraintDefinition { Kind: "DEFAULT" }:
					throw BridgeGap.Open(old, "inventory Clause.ConstraintDefinition (a column's DEFAULT with FOR or WITH VALUES)");

				case Clause.ConstraintDefinition { Kind: "INDEX" or "UNIQUE INDEX" } index:
					yield return new Ast.ColumnOption.Index(Index(index));
					break;

				case Clause.ConstraintDefinition constraint:
					yield return new Ast.ColumnOption.Constraint(Constraint(constraint, table: false));
					break;

				default:
					throw BridgeGap.Open(old, $"inventory Clause.ColumnDefinition (an option {NameOf(old)})");
			}
		}

		/// <summary>
		/// Whether a default says WITH VALUES, which the old tree keeps as the one option VALUES, unbracketed; null where it
		/// holds anything else.
		/// </summary>
		static bool? WithValues(Clause.ConstraintDefinition old)
		{
			return old switch
			{
				{ Options: null or { Length: 0 }, Bracketed: true } => false,
				{ Options: [Clause.Option { Name: var name, Value: null, Options.Length: 0, Partitions: null }], Bracketed: false } when name.Equals("VALUES", StringComparison.OrdinalIgnoreCase) => true,
				_ => null,
			};
		}

		static Ast.ColumnOption ColumnOption(Clause.ColumnOption old, string words, string? name)
		{
			if (words is not ("IDENTITY" or "COLLATE") && old.Arguments.Length > 0 || words is not ("MASKED" or "ENCRYPTED") && old.Options.Length > 0)
				throw BridgeGap.Open(old, $"inventory Clause.ColumnOption ({old.Kind} with arguments or options)");

			switch (words)
			{
				case "FILESTREAM":
					return new Ast.ColumnOption.Filestream();

				case "SPARSE":
					return new Ast.ColumnOption.Sparse();

				case "ROWGUIDCOL":
					return new Ast.ColumnOption.RowGuidCol();

				case "COLUMN_SET FOR ALL_SPARSE_COLUMNS":
					return new Ast.ColumnOption.ColumnSet();

				case "PERSISTED":
					return new Ast.ColumnOption.Persisted();

				case "NOT FOR REPLICATION":
					return new Ast.ColumnOption.NotForReplication();

				case "NULL":
					return new Ast.ColumnOption.Null { Name = name is null ? null : Values.Part(old, name) };

				case "NOT NULL":
					return new Ast.ColumnOption.Constraint(new Ast.Constraint.NotNull { Name = name is null ? null : Values.Name(old, name) });

				case "COLLATE" when old.Arguments is [Expression.ColumnReference(var collation)]:
					return new Ast.ColumnOption.Collation(new Ast.CollationName(Values.Name(old, collation)));

				case "IDENTITY" when old.Arguments.Length <= 2:
					return new Ast.ColumnOption.Identity(
						old.Arguments.Length > 0 ? Values.Of(old.Arguments[0]) : null,
						old.Arguments.Length > 1 ? Values.Of(old.Arguments[1]) : null);

				case "MASKED" when old.Options is [Clause.Option { Name: var function, Value: { } value, Options.Length: 0, Partitions: null, Bare: false }]
					&& function.Equals("FUNCTION", StringComparison.OrdinalIgnoreCase):
					return new Ast.ColumnOption.Masked(Values.Of(value));

				case "ENCRYPTED":
					return new Ast.ColumnOption.Encrypted(Each(old.Options, Option));

				default:
					throw BridgeGap.Open(old, $"inventory Clause.ColumnOption ({old.Kind})");
			}
		}

		/// <summary>
		/// A constraint, at the table or on a column: a key with T-SQL's clustering, keys, options and placement; a
		/// foreign key; a check; and at the table T-SQL's default for a column and a graph's connection.
		/// </summary>
		static Ast.Constraint Constraint(Clause.ConstraintDefinition old, bool table)
		{
			if (old.Columnstore || old.Order is not null || old.Include is not null || old.Filter is not null || old.Hash && old.Kind is not ("PRIMARY KEY" or "UNIQUE"))
				throw BridgeGap.Open(old, $"inventory Clause.ConstraintDefinition ({old.Kind} with COLUMNSTORE, ORDER, INCLUDE or a filter, or HASH on other than a key)");

			if (old.Kind is not ("PRIMARY KEY" or "UNIQUE") && (old.Clustering is not null || old.Placements is not null || !old.Bracketed && old.Kind != "DEFAULT"
				|| old.Options is { Length: > 0 } && old.Kind is not ("CHECK" or "CONNECTION" or "DEFAULT")))
				throw BridgeGap.Open(old, $"inventory Clause.ConstraintDefinition ({old.Kind} with a key's parts)");

			Ast.Constraint made = old.Kind switch
			{
				"PRIMARY KEY" or "UNIQUE" => Key(old),
				"FOREIGN KEY"             => new Ast.Constraint.ForeignKey(KeyNames(old), null, References(old)) { ForeignKeyWords = old.Columns.Length == 0 },
				"REFERENCES"              => new Ast.Constraint.ForeignKey([], null, References(old)),
				"CHECK"                   => Check(old),
				"CONNECTION" when table   => Connection(old),
				"DEFAULT" when table && old is { Check: { } value, ForColumn: { } column } && WithValues(old) is { } valued
					=> new Ast.Constraint.Default(Values.Of(value), Values.Part(old, column)) { WithValues = valued },
				_ => throw BridgeGap.Open(old, $"inventory Clause.ConstraintDefinition ({old.Kind}{(table ? " at the table" : " on a column")})"),
			};

			if (old.Enforced is { } enforced)
			{
				if (old.Kind is not ("PRIMARY KEY" or "UNIQUE" or "FOREIGN KEY" or "REFERENCES"))
					throw BridgeGap.Open(old, $"inventory Clause.ConstraintDefinition (ENFORCED on {old.Kind})");

				made = made with { Characteristics = new Ast.ConstraintCharacteristics(null, null, enforced) };
			}

			return made with { Name = old.Name is { } name ? Values.Name(old, name) : null };
		}

		/// <summary>
		/// A PRIMARY KEY or a UNIQUE as T-SQL writes it: the keys with their directions in <c>Keys</c> whether a
		/// direction was written or not, and the standard's <c>Columns</c> empty.
		/// </summary>
		static Ast.Constraint.Unique Key(Clause.ConstraintDefinition old)
		{
			Ast.Placement? on = old.Placements switch
			{
				null or []                                                       => null,
				[Clause.Placement { Kind: var kind } one] when kind.Equals("ON", StringComparison.OrdinalIgnoreCase) => Placement(one),
				_                                                                => throw BridgeGap.Open(old, "inventory Clause.ConstraintDefinition (a key placed other than ON)"),
			};

			return new Ast.Constraint.Unique(old.Kind == "PRIMARY KEY" ? Ast.UniqueKind.PrimaryKey : Ast.UniqueKind.Unique, null, [])
			{
				Clustering         = Clustering(old, old.Clustering),
				Storage            = old.Hash ? Ast.IndexStorage.Hash : null,
				Keys               = old.Columns.Length == 0 ? null : Each(old.Columns, Sorted),
				Options            = Each(old.Options ?? [], Option),
				OptionsUnbracketed = !old.Bracketed,
				On                 = on,
			};
		}

		static Ast.IndexClustering? Clustering(object owner, string? clustering)
		{
			return clustering?.ToUpperInvariant() switch
			{
				null           => null,
				"CLUSTERED"    => Ast.IndexClustering.Clustered,
				"NONCLUSTERED" => Ast.IndexClustering.Nonclustered,
				_              => throw BridgeGap.Open(owner, $"inventory Clause.ConstraintDefinition (clustering {clustering})"),
			};
		}

		/// <summary>A key's or an index's column with the direction written, which may be a graph's own <c>$node_id</c>.</summary>
		static Ast.SortItem Sorted(Clause old)
		{
			return old is Clause.SortSpecification sort
				? Queries.Sort(sort)
				: throw BridgeGap.Open(old, "inventory Clause.SortSpecification (a key's column)");
		}

		/// <summary>A foreign key's own columns, which T-SQL reads with a direction and which the standard names alone.</summary>
		static SqlList<Ast.Identifier> KeyNames(Clause.ConstraintDefinition old)
		{
			return Each(old.Columns, one => one is Clause.SortSpecification(Expression.ColumnReference(var name), SqlOrder.Unspecified)
				? Values.Part(old, name)
				: throw BridgeGap.Open(one, "inventory Clause.ConstraintDefinition (a foreign key's column with a direction)"));
		}

		static Ast.ReferencesSpecification References(Clause.ConstraintDefinition old)
		{
			if (old.Referenced is not Clause.References(var table, var columns, var onDelete, var onUpdate, var replication) references)
				throw BridgeGap.Open(old, "inventory Clause.ConstraintDefinition (a foreign key with no REFERENCES)");

			// The old tree keeps the two actions and not their order, which the text has; a delete rule alone is first.
			var deleteFirst = onDelete is not null && (onUpdate is null || Recover("ReferencesSpecification.DeleteRuleFirst", Spelled(references), static text =>
			{
				var words = string.Join(" ", Words(text));

				return words.IndexOf("ON DELETE", StringComparison.Ordinal) < words.IndexOf("ON UPDATE", StringComparison.Ordinal);
			}, false));

			return new Ast.ReferencesSpecification(Values.Name(old, table), Values.Parts(old, columns), null, null, Action(old, onUpdate), Action(old, onDelete), deleteFirst)
			{
				NotForReplication = replication,
			};
		}

		static Ast.ReferentialAction? Action(object owner, string? action)
		{
			return action switch
			{
				null          => null,
				"CASCADE"     => Ast.ReferentialAction.Cascade,
				"SET NULL"    => Ast.ReferentialAction.SetNull,
				"SET DEFAULT" => Ast.ReferentialAction.SetDefault,
				"NO ACTION"   => Ast.ReferentialAction.NoAction,
				_             => throw BridgeGap.Open(owner, $"inventory Clause.References (ON … {action})"),
			};
		}

		/// <summary>A check, and <c>NOT FOR REPLICATION</c>, which the old tree keeps as its one option.</summary>
		static Ast.Constraint.Check Check(Clause.ConstraintDefinition old)
		{
			var replication = old.Options switch
			{
				null or []                                                                                  => false,
				[Clause.Option { Name: var words }] when words.Equals("NOT FOR REPLICATION", StringComparison.OrdinalIgnoreCase) => true,
				_                                                                                            => throw BridgeGap.Open(old, "inventory Clause.ConstraintDefinition (a CHECK with options)"),
			};

			return new Ast.Constraint.Check(Values.Of(old.Check ?? throw BridgeGap.Open(old, "inventory Clause.ConstraintDefinition (a CHECK with no condition)")))
			{
				NotForReplication = replication,
			};
		}

		/// <summary>
		/// A graph edge's <c>CONNECTION</c>: the pairs, and <c>ON DELETE CASCADE</c>, the one action the tree holds; the
		/// old tree keeps the actions as options after the pairs.
		/// </summary>
		static Ast.Constraint.Connection Connection(Clause.ConstraintDefinition old)
		{
			var pairs   = new List<Ast.EdgeConnection>();
			var cascade = false;

			foreach (var one in old.Options ?? [])
			{
				if (one is Clause.Connection(var from, var to) && !cascade)
					pairs.Add(new Ast.EdgeConnection(Values.Name(one, from), Values.Name(one, to)));
				else if (one is Clause.Option { Name: "ON DELETE CASCADE" } && !cascade)
					cascade = true;
				else
					throw BridgeGap.Open(old, "inventory Clause.ConstraintDefinition (a CONNECTION action other than one ON DELETE CASCADE)");
			}

			return new Ast.Constraint.Connection(pairs.ToSqlList(), cascade);
		}

		/// <summary>
		/// An index written at the table or on a column, or made by CREATE INDEX: its columnstore ORDER, and its ON and
		/// FILESTREAM_ON in the order written, and whether its options were written without brackets.
		/// </summary>
		static Ast.IndexDefinition Index(Clause.ConstraintDefinition old)
		{
			if (old.Hash && old.Columnstore)
				throw BridgeGap.Open(old, "inventory Clause.ConstraintDefinition (an index both COLUMNSTORE and HASH)");

			if (old.Check is not null || old.Referenced is not null || old.Enforced is not null || old.ForColumn is not null)
				throw BridgeGap.Open(old, "inventory Clause.ConstraintDefinition (an index with a constraint's parts)");

			return new Ast.IndexDefinition(IndexName(old))
			{
				Unique             = old.Kind == "UNIQUE INDEX",
				Clustering         = Clustering(old, old.Clustering),
				Storage            = old.Columnstore ? Ast.IndexStorage.Columnstore : old.Hash ? Ast.IndexStorage.Hash : null,
				Columns            = Each(old.Columns, Sorted),
				Order              = Values.Parts(old, old.Order),
				Include            = Values.Parts(old, old.Include),
				Where              = old.Filter is null ? null : Values.Of(old.Filter),
				Options            = Each(old.Options ?? [], Option),
				Placements         = Each(old.Placements ?? [], Placement),
				OptionsUnbracketed = !old.Bracketed,
			};
		}

		/// <summary>
		/// Where a table, a key or an index is put: the word, a filegroup, a quoted name, a string, or a scheme and
		/// its column.
		/// </summary>
		static Ast.Placement Placement(Clause old)
		{
			if (old is not Clause.Placement(var kind, var target, var columns))
				throw BridgeGap.Open(old, "inventory Clause.Placement");

			var placed = kind.ToUpperInvariant() switch
			{
				"ON"            => Ast.PlacementKind.On,
				"TEXTIMAGE_ON"  => Ast.PlacementKind.TextImageOn,
				"FILESTREAM_ON" => Ast.PlacementKind.FilestreamOn,
				_               => throw BridgeGap.Open(old, $"inventory Clause.Placement ({kind})"),
			};

			return new Ast.Placement(placed, PlaceTarget(old, target), columns is null ? null : Values.Parts(old, columns));
		}

		/// <summary>A placement's target: a string as the string-style name it is, any other as a name of one part.</summary>
		internal static Ast.Identifier PlaceTarget(object old, string target)
		{
			return target.StartsWith('\'')
				? new Ast.Identifier(target[1..^1].Replace("''", "'"), Ast.IdentifierStyle.String)
				: Values.Part(old, target);
		}

		/// <summary>
		/// One option of a <c>WITH (…)</c> (proposal 20): a name, a value, the options of its own, the partitions after
		/// a compression. A key word it is set to is a one-part reference, as every value here is what
		/// <see cref="Values.Of"/> makes of it; a value with a unit is a measured one. A bracketed list of options after
		/// the name with no <c>=</c> is the option's own list. What the record has no place for — a run of words, a
		/// bracketed list after <c>=</c> — is a gap.
		/// </summary>
		internal static Ast.Option Option(Clause old)
		{
			return Option(old, generic: false);
		}

		/// <summary>
		/// An option where the generic rule stands whole (OptionsWith, RoutineOption): OptionTail reads a bracketed list
		/// after the name both after an <c>=</c> and with none, <c>X = (Y = 1)</c> against <c>X (Y = 1)</c>, and
		/// <see cref="Ast.Option.EqualsOmitted"/> keeps which. Elsewhere the place fixes the form, and the one with the
		/// <c>=</c> is a gap.
		/// </summary>
		internal static Ast.Option GenericOption(Clause old)
		{
			return Option(old, generic: true);
		}

		static Ast.Option Option(Clause old, bool generic)
		{
			if (old is not Clause.Option(var name, var value, var options, var partitions, var bare))
				throw BridgeGap.Open(old, $"inventory Clause.Option (an option that is a {NameOf(old)})");

			// MOVE TO and where it moves to (OptionSetting): the option's place, a placement of kind MoveTo.
			if (Words(name) is ["MOVE", "TO"] && value is null && options is [Clause.Placement(_, var target, var columns)] && partitions is null)
			{
				var words = SqlTokens.Of(name);

				return new Ast.Option(Values.Part(old, words[0].Text), null, [])
				{
					Words = [Values.Part(old, words[1].Text)],
					Place = new Ast.Placement(Ast.PlacementKind.MoveTo, PlaceTarget(old, target), columns is null ? null : Values.Parts(old, columns)),
				};
			}

			if (name.Contains('.') || name.Contains('('))
				throw BridgeGap.Open(old, $"inventory Clause.Option (a name of several words, `{name}`)");

			// A name of several words (the generic rule's OptionName): the first word is the name, the rest its words.
			var named = SqlTokens.Of(name);

			if (named.Count == 0 || named.Any(static one => one.Kind != SqlTokens.Kind.Word))
				throw BridgeGap.Open(old, $"inventory Clause.Option (a name of several words, `{name}`)");

			if (bare && value is not null)
				throw BridgeGap.Open(old, "inventory Clause.Option (a value written after the name with no =)");

			if (!generic && !bare && value is null && options.Length > 0)
				throw BridgeGap.Open(old, "inventory Clause.Option (a bracketed list after =)");

			return new Ast.Option(Values.Part(old, named[0].Text), value is null ? null : OptionValue(old, value), Each(options, one => Option(one, generic)))
			{
				Partitions    = partitions is null ? null : Ranges(old, partitions),
				Words         = [.. named.Skip(1).Select(one => Values.Part(old, one.Text))],
				EqualsOmitted = generic && bare && value is null && options.Length > 0,
			};
		}

		/// <summary>
		/// One option of a spatial index, as SpatialSet reads it: a value, a bracket of values by position, which is the
		/// option's value as a row (<c>BOUNDING_BOX = (0, 0, 500, 200)</c>), or a bracket of settings by name, which are the
		/// option's own options (<c>GRIDS = (LEVEL_1 = HIGH)</c>), always after an equals sign.
		/// </summary>
		static Ast.Option SpatialOption(Clause old)
		{
			switch (old)
			{
				case Clause.Option(var name, Expression.RoutineInvocation { Name: "", Arguments: var items }, [], null, false):
					return new Ast.Option(Values.Part(old, name), new Ast.Expression.Row(Each(items, Values.Of)), []);

				case Clause.Option(var name, null, { Length: > 0 } settings, null, false):
					return new Ast.Option(Values.Part(old, name), null, Each(settings, Option));

				default:
					return Option(old);
			}
		}

		static Ast.Expression OptionValue(Clause owner, Expression value)
		{
			switch (value)
			{
				// A blank inside a bracketed or quoted name is the name's own: the tokens are the text less its blanks.
				case Expression.ColumnReference(var words) when SqlTokens.Of(words).Sum(static one => one.Text.Length) != words.Length || words.StartsWith('('):
					throw BridgeGap.Open(owner, $"inventory Clause.Option (a value that is a run of words or numbers, `{words}`)");

				case Expression.RoutineInvocation { Name: "" }:
					throw BridgeGap.Open(owner, "inventory Clause.Option (a bracketed list of values)");

				case Expression.Measured(var measured, var unit):
					return new Ast.Expression.Measured(Values.Of(measured), new Ast.Identifier(unit));

				default:
					return Values.Of(value);
			}
		}

		/// <summary>
		/// ALTER TABLE: one action a statement, as the old tree keeps it — the word, the elements, the options, and
		/// the rest of the action as text, which is read again here.
		/// </summary>
		/// <summary>
		/// ALTER TABLE, on the common tree: the table, and the action its rules read, which they gather into the T-SQL
		/// tree's record; it stands where the statement was written, as the text it was read from says.
		/// </summary>
		internal static Statement AlterTable(string name, Statement.AlterTable action, SqlSpan at)
		{
			var read = action with { Name = name };

			return Defer(read, at, static read => AlterTable(read));
		}

		static Ast.Statement AlterTable(Statement.AlterTable old)
		{
			var words = Words(old.Action);
			Ast.ConstraintCheck? check = null;

			if (words is ["WITH", var mode, .. var rest])
			{
				check = Checked(old, mode);
				words = rest;
			}

			var action = words switch
			{
				["ADD"]                 => Added(old),
				["ALTER", "COLUMN"]     => AlteredColumn(old),
				["ALTER"]               => AlteredIndex(old),
				["DROP"]                => Dropped(old),
				["CHECK" or "NOCHECK"]  => CheckConstraints(old, Checked(old, words[0])),
				["ENABLE" or "DISABLE"] => Enabled(old, words[0] == "ENABLE"),
				["SPLIT" or "MERGE"]    => Ranged(old, words[0] == "SPLIT"),
				["SET"]                 => new Ast.AlterTableAction.SetOptions(OptionsOf(old, Tail(old))),
				["REBUILD"]             => Rebuild(old),
				["SWITCH"]              => Switch(old),
				_                       => throw BridgeGap.Open(old, $"inventory Statement.AlterTable ({old.Action})"),
			};

			if (check is not null && action is not (Ast.AlterTableAction.AddColumn or Ast.AlterTableAction.AddConstraint or Ast.AlterTableAction.Add or Ast.AlterTableAction.CheckConstraints))
				throw BridgeGap.Open(old, $"inventory Statement.AlterTable ({old.Action})");

			return new Ast.Statement.AlterTable
			{
				Name    = Values.Name(old, old.Name),
				Actions = [action],
				Check   = check,
			};
		}

		static Ast.ConstraintCheck Checked(object owner, string word)
		{
			return word switch
			{
				"CHECK"   => Ast.ConstraintCheck.Check,
				"NOCHECK" => Ast.ConstraintCheck.NoCheck,
				_         => throw BridgeGap.Open(owner, $"inventory Statement.AlterTable (WITH {word})"),
			};
		}

		static string Tail(Statement.AlterTable old)
		{
			return old.Tail ?? throw BridgeGap.Open(old, $"inventory Statement.AlterTable ({old.Action} with nothing after it)");
		}

		/// <summary>
		/// ADD: one column or one constraint is the standard's action, a period the standard's too, and one index or
		/// two elements or more T-SQL's; a default's <c>WITH VALUES</c> its own. The constraint the grammar reads with a
		/// <c>FOR</c> it then drops is a gap.
		/// </summary>
		static Ast.AlterTableAction Added(Statement.AlterTable old)
		{
			if (old.Options is { Length: > 0 } || old.Tail is not null)
				throw BridgeGap.Open(old, "inventory Statement.AlterTable (ADD with options)");

			// `ADD CHECK (…) FOR c`: the grammar reads the FOR and keeps no trace of it, so the text alone says so.
			if (old.Elements is [Clause.ConstraintDefinition { Kind: not "DEFAULT" }] && Spelled(old) is { } text && Words(text) is [.., not "NOT", "FOR", _])
				throw BridgeGap.Open(old, "inventory Statement.AlterTable (a constraint added with a FOR the old tree drops)");

			var elements = Elements(old.Elements);

			return elements switch
			{
				[Ast.TableElement.Column(var column)]                => new Ast.AlterTableAction.AddColumn(column),
				[Ast.TableElement.TableConstraint(var constraint)]   => new Ast.AlterTableAction.AddConstraint(constraint),
				[Ast.TableElement.Period(var period)]                => new Ast.AlterTableAction.AddPeriod(period, []),
				[Ast.TableElement.Index] or { Count: > 1 }           => new Ast.AlterTableAction.Add(elements),
				_                                                    => throw BridgeGap.Open(old, "inventory Statement.AlterTable (ADD of nothing)"),
			};
		}

		/// <summary>
		/// ALTER COLUMN: a type, with a collation and a nullability, written alone; or a property added or dropped.
		/// </summary>
		static Ast.AlterTableAction AlteredColumn(Statement.AlterTable old)
		{
			if (old.Elements is not [Clause.ColumnDefinition { Computed: null } column] || old.Tail is not null)
				throw BridgeGap.Open(old, "inventory Statement.AlterTable (ALTER COLUMN of other than one column)");

			var options = Each(old.Options ?? [], Option);

			if (column.Type is null)
			{
				if (column.Options is not [Clause.ColumnOption { Kind: var flag, Arguments.Length: 0, Options.Length: 0, ConstraintName: null }])
					throw BridgeGap.Open(old, "inventory Statement.AlterTable (ALTER COLUMN with neither a type nor one property)");

				return new Ast.AlterTableAction.AlterColumn(Values.Part(old, column.Name), Property(old, flag, options), true);
			}

			// Syntax.AlteredColumn: the front of a column's tail, in the order written, and one nullability last.
			Ast.ColumnNullability? nullability = null;
			IEnumerable<Clause> front = column.Options;

			if (column.Options.Length > 0 && column.Options[column.Options.Length - 1] is Clause.ColumnOption { Kind: "NULL" or "NOT NULL", ConstraintName: null } last)
			{
				nullability = last.Kind == "NULL" ? Ast.ColumnNullability.Null : Ast.ColumnNullability.NotNull;
				front       = column.Options.Take(column.Options.Length - 1);
			}

			var type = new Ast.AlterColumnAction.SetDataType(Values.Type(old, column.Type))
			{
				SetDataTypeOmitted = true,
				ColumnOptions      = [.. front.SelectMany(ColumnOptions)],
				Nullability        = nullability,
				Options            = options,
			};

			return new Ast.AlterTableAction.AlterColumn(Values.Part(old, column.Name), type, true);
		}

		/// <summary>
		/// <c>ADD ROWGUIDCOL</c>, <c>DROP PERSISTED</c>, <c>ADD MASKED WITH (FUNCTION = 'f()')</c>, <c>DROP MASKED</c>:
		/// the words the old tree keeps as the option's kind, read again.
		/// </summary>
		static Ast.AlterColumnAction Property(Statement.AlterTable old, string flag, SqlList<Ast.Option> options)
		{
			var tokens = SqlTokens.Of(flag);
			var words  = Words(flag);

			var add = words[0] switch
			{
				"ADD"  => true,
				"DROP" => false,
				_      => throw BridgeGap.Open(old, $"inventory Statement.AlterTable (ALTER COLUMN {flag})"),
			};

			Ast.ColumnPropertyKind kind;
			Ast.Expression? function = null;

			switch (words[1..])
			{
				case ["ROWGUIDCOL"]: kind = Ast.ColumnPropertyKind.RowGuidCol; break;
				case ["PERSISTED"]: kind = Ast.ColumnPropertyKind.Persisted; break;
				case ["NOT", "FOR", "REPLICATION"]: kind = Ast.ColumnPropertyKind.NotForReplication; break;
				case ["SPARSE"]: kind = Ast.ColumnPropertyKind.Sparse; break;
				case ["HIDDEN"]: kind = Ast.ColumnPropertyKind.Hidden; break;
				case ["MASKED"]: kind = Ast.ColumnPropertyKind.Masked; break;

				case ["MASKED", "WITH", "(", "FUNCTION", "=", _, ")"] when tokens[6].Kind == SqlTokens.Kind.String:
					kind     = Ast.ColumnPropertyKind.Masked;
					function = Values.String(tokens[6].Text);
					break;

				default:
					throw BridgeGap.Open(old, $"inventory Statement.AlterTable (ALTER COLUMN {flag})");
			}

			return new Ast.AlterColumnAction.Property(add, kind, function, options);
		}

		/// <summary>
		/// DROP: one item is the standard's action, two or more T-SQL's list. An item without its word is of the kind
		/// of the one before it, and a first one a constraint, as the grammar reads it.
		/// </summary>
		static Ast.AlterTableAction Dropped(Statement.AlterTable old)
		{
			if (old.Options is { Length: > 0 } || old.Tail is not null)
				throw BridgeGap.Open(old, "inventory Statement.AlterTable (DROP with options)");

			// `DROP INDEX i`, a memory-optimized table's, alone: a DROP list holds columns and constraints.
			if (old.Elements is [Clause.Dropped("INDEX", { } index, false, null)])
				return new Ast.AlterTableAction.DropIndex(Values.Part(old, index));

			var items = new List<Ast.AlterTableAction>();

			foreach (var element in old.Elements)
			{
				// `PERIOD FOR SYSTEM_TIME`, with no behavior, which T-SQL does not write.
				if (element is Clause.Dropped("PERIOD FOR SYSTEM_TIME", null, false, null))
				{
					items.Add(new Ast.AlterTableAction.DropPeriod(Ast.PeriodKind.SystemTime, null, null));
					continue;
				}

				if (element is not Clause.Dropped(var kind, { } name, var exists, var tail))
					throw BridgeGap.Open(element, "inventory Clause.Dropped (an index in a list, which no action holds)");

				var column = kind == "COLUMN" || kind == "" && items.Count > 0 && items[^1] is Ast.AlterTableAction.DropColumn;

				if (kind is not ("COLUMN" or "CONSTRAINT" or ""))
					throw BridgeGap.Open(element, $"inventory Clause.Dropped ({kind})");

				if (column)
				{
					if (tail is not null)
						throw BridgeGap.Open(element, "inventory Clause.Dropped (a column dropped with options)");

					items.Add(new Ast.AlterTableAction.DropColumn(Values.Part(element, name), null, kind == "COLUMN") { IfExists = exists });
				}
				else
					items.Add(new Ast.AlterTableAction.DropConstraint(new Ast.QualifiedName([Values.Part(element, name)]), null)
					{
						IfExists                 = exists,
						Options                  = tail is null ? [] : OptionsOf(element, tail.TrimStart()["WITH".Length..]),
						ConstraintKeywordOmitted = kind == "",
					});
			}

			return items.Count == 1 ? items[0] : new Ast.AlterTableAction.Drop(items.ToSqlList());
		}

		/// <summary><c>CHECK CONSTRAINT ALL</c>, <c>NOCHECK CONSTRAINT a, b</c>: the names after the word, or ALL.</summary>
		static Ast.AlterTableAction CheckConstraints(Statement.AlterTable old, Ast.ConstraintCheck check)
		{
			var (all, names) = NamesOrAll(old, Tail(old), "CONSTRAINT");

			return new Ast.AlterTableAction.CheckConstraints(check, all, names);
		}

		/// <summary>ENABLE and DISABLE: of triggers, or of change tracking; a file table's namespace is a gap.</summary>
		static Ast.AlterTableAction Enabled(Statement.AlterTable old, bool enable)
		{
			var tail  = Tail(old);
			var words = Words(tail);

			if (words is ["TRIGGER", ..])
			{
				var (all, names) = NamesOrAll(old, tail, "TRIGGER");

				return new Ast.AlterTableAction.Triggers(enable, all, names);
			}

			if (words is ["CHANGE_TRACKING"])
				return new Ast.AlterTableAction.ChangeTracking(enable, []);

			if (words is ["FILETABLE_NAMESPACE"])
				return new Ast.AlterTableAction.FileTableNamespace(enable);

			if (words is ["CHANGE_TRACKING", "WITH", "(", ..])
				return new Ast.AlterTableAction.ChangeTracking(enable, OptionsOf(old, tail[(tail.IndexOf("WITH", StringComparison.OrdinalIgnoreCase) + "WITH".Length)..]));

			throw BridgeGap.Open(old, $"inventory Statement.AlterTable ({old.Action} {words[0]})");
		}

		/// <summary><c>SPLIT RANGE (v)</c> and <c>MERGE RANGE (v)</c>, the boundary kept as text and read again as a value.</summary>
		static Ast.AlterTableAction Ranged(Statement.AlterTable old, bool split)
		{
			var tail  = Tail(old);
			var open  = tail.IndexOf('(');
			var close = tail.LastIndexOf(')');

			if (Words(tail) is not ["RANGE", "(", .., ")"] || close <= open)
				throw BridgeGap.Open(old, $"inventory Statement.AlterTable ({old.Action} {tail})");

			var boundary = ValueOf(old, tail.Substring(open + 1, close - open - 1));

			return split ? new Ast.AlterTableAction.SplitRange(boundary) : new Ast.AlterTableAction.MergeRange(boundary);
		}

		/// <summary>The word, then ALL or the names separated by commas.</summary>
		static (bool All, SqlList<Ast.Identifier> Names) NamesOrAll(Statement.AlterTable old, string tail, string word)
		{
			var tokens = SqlTokens.Of(tail);

			if (tokens is not [{ Kind: SqlTokens.Kind.Word } first, ..] || !first.Text.Equals(word, StringComparison.OrdinalIgnoreCase))
				throw BridgeGap.Open(old, $"inventory Statement.AlterTable ({old.Action} {tail})");

			if (tokens is [_, { Kind: SqlTokens.Kind.Word, Text: var all }] && all.Equals("ALL", StringComparison.OrdinalIgnoreCase))
				return (true, []);

			var names = new List<Ast.Identifier>();

			for (var at = 1; at < tokens.Count; at += 2)
			{
				if (tokens[at].Kind is not (SqlTokens.Kind.Word or SqlTokens.Kind.Delimited) || at + 1 < tokens.Count && tokens[at + 1].Text != ",")
					throw BridgeGap.Open(old, $"inventory Statement.AlterTable ({old.Action} {tail})");

				names.Add(Values.Part(old, tokens[at].Text));
			}

			return (false, names.ToSqlList());
		}

		/// <summary>REBUILD: <c>PARTITION = n</c>, <c>PARTITION = ALL</c> or neither, and the options.</summary>
		static Ast.AlterTableAction Rebuild(Statement.AlterTable old)
		{
			var tail = old.Tail ?? "";
			var with = WithAt(tail);
			var head = with < 0 ? tail : tail[..with];

			Ast.Expression? partition = null;
			var all = false;
			var words = Words(head);

			if (words is ["PARTITION", "=", "ALL"])
				all = true;
			else if (words is ["PARTITION", "=", ..])
				partition = ValueOf(old, head[(head.IndexOf('=') + 1)..]);
			else if (words.Count > 0)
				throw BridgeGap.Open(old, $"inventory Statement.AlterTable (REBUILD {tail})");

			return new Ast.AlterTableAction.Rebuild(partition, all, with < 0 ? [] : OptionsOf(old, tail[(with + "WITH".Length)..]));
		}

		/// <summary>SWITCH: the partition switched, the table and its partition, and the options.</summary>
		static Ast.AlterTableAction Switch(Statement.AlterTable old)
		{
			var tail = Tail(old);
			var with = WithAt(tail);
			var head = with < 0 ? tail : tail[..with];

			var tokens = SqlTokens.Of(head);
			var to     = tokens.FindIndex(static one => one.Kind == SqlTokens.Kind.Word && one.Text.Equals("TO", StringComparison.OrdinalIgnoreCase));

			if (to < 0 || to + 1 >= tokens.Count)
				throw BridgeGap.Open(old, $"inventory Statement.AlterTable (SWITCH {tail})");

			Ast.Expression? Partition(IReadOnlyList<SqlTokens.Token> part)
			{
				if (part.Count == 0)
					return null;

				if (part[0].Kind != SqlTokens.Kind.Word || !part[0].Text.Equals("PARTITION", StringComparison.OrdinalIgnoreCase) || part.Count < 2)
					throw BridgeGap.Open(old, $"inventory Statement.AlterTable (SWITCH {tail})");

				return ValueOf(old, string.Join(" ", part.Skip(1).Select(static one => one.Text)));
			}

			// The table is a name of one or more parts, dots between; what follows it is its partition.
			var end = to + 1;

			while (end + 2 < tokens.Count + 1 && end + 1 < tokens.Count && tokens[end + 1].Text == ".")
				end += 2;

			var target = string.Concat(tokens.Skip(to + 1).Take(end - to).Select(static one => one.Text));

			return new Ast.AlterTableAction.Switch(
				Partition(tokens.Take(to).ToList()),
				Values.Name(old, target),
				Partition(tokens.Skip(end + 1).ToList()),
				with < 0 ? [] : OptionsOf(old, tail[(with + "WITH".Length)..]));
		}

		/// <summary>Where the word WITH begins a tail's options, or -1.</summary>
		static int WithAt(string tail)
		{
			var words = SqlTokens.Of(tail);
			var at    = 0;

			foreach (var token in words)
			{
				var found = tail.IndexOf(token.Text, at, StringComparison.Ordinal);

				if (token.Kind == SqlTokens.Kind.Word && token.Text.Equals("WITH", StringComparison.OrdinalIgnoreCase))
					return found;

				at = found + token.Text.Length;
			}

			return -1;
		}

		/// <summary>
		/// A bracketed list of options kept as text, read again by the grammar's own option list — the one a table
		/// made of a query is written with — and carried as every option is.
		/// </summary>
		static SqlList<Ast.Option> OptionsOf(object owner, string text)
		{
			return Skim("CREATE TABLE t (a) WITH " + text.Trim() + " AS SELECT 1") is [Statement.Built { Given: Defining { Options: { } options } }]
				? Each(options, Option)
				: throw BridgeGap.Open(owner, $"inventory Statement.AlterTable (options `{text.Trim()}` the option list does not read)");
		}

		/// <summary>A value kept as text, read again as a select list's one value.</summary>
		static Ast.Expression ValueOf(object owner, string text)
		{
			return Queries.Specified("SELECT " + text.Trim()) is { Columns: [Clause.DerivedColumn { Value: var value, Name: null }] }
				? Values.Of(value)
				: throw BridgeGap.Open(owner, $"inventory Statement.AlterTable (a value `{text.Trim()}`)");
		}

		// ── Indexes, full-text indexes, views, statistics, types, sequences, synonyms (task 5b) ────────────

		/// <summary>
		/// CREATE INDEX: a relational or columnstore index is the index a table declares; an XML index and a spatial one
		/// are records of their own, their kind and what they are built over kept by the old tree as words. A selective
		/// XML index, a JSON index and a vector index are gaps.
		/// </summary>
		/// <summary>CREATE INDEX, on the common tree: the table, the index as its rules read it, its kind and what it is built over.</summary>
		internal static Statement CreateIndex(string on, Clause index, string? kind = null, string? over = null)
		{
			var read = new Statement.CreateIndex(on, index, kind, over);

			return Hold(read, static read => CreateIndex(read));
		}

		static Ast.Statement CreateIndex(Statement.CreateIndex old)
		{
			if (old.Index is not Clause.ConstraintDefinition index)
				throw BridgeGap.Open(old, "inventory Statement.CreateIndex (an index that is not one)");

			var table = Values.Name(old, old.On);

			switch (old.Kind?.ToUpperInvariant())
			{
				case null:
					if (old.Using is not null)
						throw BridgeGap.Open(old, "inventory Statement.CreateIndex (USING on a relational index)");

					return new Ast.Statement.CreateIndex(table, Index(index));

				case "PRIMARY XML" or "XML":
					var xml = new Ast.Statement.CreateXmlIndex(table, IndexName(index), Column(index))
					{
						Primary = old.Kind.Equals("PRIMARY XML", StringComparison.OrdinalIgnoreCase),
						Options = Each(index.Options ?? [], Option),
					};

					if (old.Using is null)
						return xml;

					// `USING XML INDEX p FOR PATH`, or FOR a selective index's path in brackets.
					var tokens = SqlTokens.Of(old.Using);

					if (Words(old.Using) is ["USING", "XML", "INDEX", _, "FOR", "(", _, ")"] && tokens[3].Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited
						&& tokens[6].Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited)
						return xml with { Using = Values.Part(old, tokens[3].Text), ForPath = Values.Part(old, tokens[6].Text) };

					if (Words(old.Using) is not ["USING", "XML", "INDEX", _, "FOR", var kind] || tokens[3].Kind is not (SqlTokens.Kind.Word or SqlTokens.Kind.Delimited))
						throw BridgeGap.Open(old, "inventory Statement.CreateIndex (a secondary XML index FOR a selective index's path)");

					return xml with
					{
						Using = Values.Part(old, tokens[3].Text),
						For   = kind switch
						{
							"VALUE"    => Ast.XmlIndexFor.Value,
							"PATH"     => Ast.XmlIndexFor.Path,
							"PROPERTY" => Ast.XmlIndexFor.Property,
							_          => throw BridgeGap.Open(old, $"inventory Statement.CreateIndex (FOR {kind})"),
						},
					};

				case "SELECTIVE XML":
					return SelectiveXmlIndex(old, table, index);

				// FOR ('$.a', …), which the old tree keeps as text: the paths, strings each.
				case "JSON":
					var over  = old.Using is null ? [] : SqlTokens.Of(old.Using);
					var paths = old.Using is null ? [] : over is [{ Text: var word }, { Text: "(" }, _, ..] && over[^1].Text == ")" && word.Equals("FOR", StringComparison.OrdinalIgnoreCase)
						? Queries.Split(over.GetRange(2, over.Count - 3)).Select(one => one is [{ Kind: SqlTokens.Kind.String } path] ? (Ast.LiteralValue.String)Values.String(path.Text) : throw BridgeGap.Open(old, $"inventory Statement.CreateIndex (a JSON index's paths `{old.Using}`)")).ToSqlList()
						: throw BridgeGap.Open(old, $"inventory Statement.CreateIndex (a JSON index's paths `{old.Using}`)");

					return new Ast.Statement.CreateJsonIndex(table, IndexName(index), Column(index))
					{
						Paths   = paths,
						Options = Each(index.Options ?? [], Option),
					};

				case "SPATIAL":
					// The grid is any one name to the grammar: one of the four, bare or in brackets, carries over; any other
					// word is the engine's to refuse, and a gap.
					var grid      = old.Using is null ? null : SqlTokens.Of(old.Using) is [_, var scheme] ? scheme : default(SqlTokens.Token?);
					var bracketed = grid is { Kind: SqlTokens.Kind.Delimited, Text: var written } && written.StartsWith('[');

					return new Ast.Statement.CreateSpatialIndex(table, IndexName(index), Column(index))
					{
						Using = old.Using is null ? null : (bracketed ? Values.Unquoted(grid!.Value.Text) : grid?.Kind == SqlTokens.Kind.Word ? grid.Value.Text : "").ToUpperInvariant() switch
						{
							"GEOMETRY_GRID"       => Ast.SpatialTessellation.GeometryGrid,
							"GEOGRAPHY_GRID"      => Ast.SpatialTessellation.GeographyGrid,
							"GEOMETRY_AUTO_GRID"  => Ast.SpatialTessellation.GeometryAutoGrid,
							"GEOGRAPHY_AUTO_GRID" => Ast.SpatialTessellation.GeographyAutoGrid,
							_                     => throw BridgeGap.Open(old, $"inventory Statement.CreateIndex (a spatial index {old.Using})"),
						},
						UsingBracketed = bracketed,
						Options    = Each(index.Options ?? [], SpatialOption),
						Placements = Each(index.Placements ?? [], Placement),
					};

				default:
					throw BridgeGap.Open(old, $"inventory Statement.CreateIndex ({old.Kind} INDEX, which no record holds)");
			}
		}

		static Ast.Identifier IndexName(Clause.ConstraintDefinition old)
		{
			return Values.Part(old, old.Name ?? throw BridgeGap.Open(old, "inventory Clause.ConstraintDefinition (an index with no name)"));
		}

		/// <summary>The one column an XML or a spatial index is over, bare.</summary>
		static Ast.Identifier Column(Clause.ConstraintDefinition old)
		{
			return old.Columns is [Clause.SortSpecification(Expression.ColumnReference(var column), SqlOrder.Unspecified)]
				? Values.Part(old, column)
				: throw BridgeGap.Open(old, "inventory Statement.CreateIndex (an XML or spatial index over other than one column)");
		}

		/// <summary>
		/// CREATE SELECTIVE XML INDEX, whose namespaces and paths the old tree keeps as text after the column: each path
		/// read as PromotedPath reads it. A path that adds, alters or removes, which only ALTER INDEX means, is a gap.
		/// </summary>
		static Ast.Statement SelectiveXmlIndex(Statement.CreateIndex old, Ast.QualifiedName table, Clause.ConstraintDefinition index)
		{
			var text    = old.Using ?? throw BridgeGap.Open(old, "inventory Statement.CreateIndex (a selective XML index with no paths)");
			var tokens  = Located(text);
			var at      = 0;
			SqlList<Ast.XmlNamespace> namespaces = [];

			if (Is(tokens, 0, "WITH") && Is(tokens, 1, "XMLNAMESPACES") && tokens.Count > 2 && tokens[2].Token.Text == "(")
			{
				var close = Closing(tokens, 2);

				namespaces = Queries.XmlNamespaces(old, text[tokens[1].At..(tokens[close].At + 1)]);
				at         = close + 1;
			}

			if (!Is(tokens, at, "FOR") || at + 1 >= tokens.Count || tokens[at + 1].Token.Text != "(" || Closing(tokens, at + 1) != tokens.Count - 1)
				throw BridgeGap.Open(old, $"inventory Statement.CreateIndex (a selective XML index's paths `{text}`)");

			var paths = Listed(text[(tokens[at + 1].At + 1)..tokens[^1].At]).Select(one => Promoted(old, one)).ToSqlList();

			return new Ast.Statement.CreateSelectiveXmlIndex(table, IndexName(index), Column(index), paths)
			{
				XmlNamespaces = namespaces,
				Options       = Each(index.Options ?? [], Option),
			};
		}

		/// <summary>
		/// One promoted path, as PromotedPath reads it: a name, an equals sign and the node path, or the node path alone, and
		/// then what it is said to be, in the order written — <c>AS XQUERY ['type'] [MAXLENGTH (n)]</c>, <c>AS SQL type</c>,
		/// <c>SINGLETON</c>. PathHint's <c>AS word</c>, which no page publishes, is a gap.
		/// </summary>
		static Ast.PromotedPath Promoted(object owner, string text)
		{
			var tokens = Located(text);
			var named  = tokens.Count > 2 && tokens[0].Token.Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited && tokens[1].Token.Text == "=";
			var at     = named ? 2 : 0;

			if (at >= tokens.Count || tokens[at].Token is not { Kind: SqlTokens.Kind.String } path || path.Text[0] != '\'')
				throw BridgeGap.Open(owner, $"inventory Statement.CreateIndex (a promoted path `{text.Trim()}`)");

			var hints = new List<Ast.PathHint>();

			at++;

			while (at < tokens.Count)
			{
				if (Is(tokens, at, "SINGLETON"))
				{
					hints.Add(new Ast.PathHint.Singleton());
					at++;
				}
				else if (Is(tokens, at, "AS") && Is(tokens, at + 1, "XQUERY"))
				{
					at += 2;

					Ast.LiteralValue.String? type = null;
					Ast.LiteralValue.Numeric? length = null;

					if (at < tokens.Count && tokens[at].Token is { Kind: SqlTokens.Kind.String } typed && typed.Text[0] == '\'')
					{
						type = (Ast.LiteralValue.String)Values.String(typed.Text);
						at++;
					}

					if (Is(tokens, at, "MAXLENGTH") && at + 3 < tokens.Count && tokens[at + 1].Token.Text == "(" && tokens[at + 2].Token.Kind == SqlTokens.Kind.Number && tokens[at + 3].Token.Text == ")")
					{
						length = (Ast.LiteralValue.Numeric)Values.Of(new Expression.Literal(SqlLiteralKind.Number, tokens[at + 2].Token.Text));
						at    += 4;
					}

					hints.Add(new Ast.PathHint.XQuery(type, length));
				}
				else if (Is(tokens, at, "AS") && Is(tokens, at + 1, "SQL") && at + 2 < tokens.Count)
				{
					// The type runs to the next hint: an AS or a SINGLETON outside its brackets.
					var end   = at + 2;
					var depth = 0;

					while (end < tokens.Count && !(depth == 0 && end > at + 2 && (Is(tokens, end, "AS") || Is(tokens, end, "SINGLETON"))))
					{
						depth += tokens[end].Token.Text == "(" ? 1 : tokens[end].Token.Text == ")" ? -1 : 0;
						end++;
					}

					hints.Add(new Ast.PathHint.Sql(Values.Type(owner, text[tokens[at + 2].At..(end < tokens.Count ? tokens[end].At : text.Length)].Trim())));
					at = end;
				}
				else
					throw BridgeGap.Open(owner, $"inventory Statement.CreateIndex (a promoted path's hint `{text.Trim()}`)");
			}

			return new Ast.PromotedPath(named ? Values.Part(owner, tokens[0].Token.Text) : null, (Ast.LiteralValue.String)Values.String(path.Text))
			{
				Hints = [.. hints],
			};
		}

		/// <summary>
		/// ALTER INDEX … FOR (…): a selective index's paths added, altered or removed, each kept as text and read as
		/// SelectivePath reads it. A path that does none of the three, which only CREATE means, is a gap.
		/// </summary>
		static Ast.IndexAlteration Paths(Statement.AlterIndex old)
		{
			var actions = Each(old.Paths ?? [], one =>
			{
				var tokens = Located(one);

				if (Is(tokens, 0, "ADD"))
					return (Ast.PathAction)new Ast.PathAction.Add(Promoted(old, one[tokens[1].At..]));

				if (Is(tokens, 0, "ALTER") && Is(tokens, 1, "PATH") && tokens.Count > 2)
					return new Ast.PathAction.Alter(Promoted(old, one[tokens[2].At..]));

				if (Is(tokens, 0, "REMOVE") && tokens.Count == 2 && tokens[1].Token.Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited)
					return new Ast.PathAction.Remove(Values.Part(old, tokens[1].Token.Text));

				throw BridgeGap.Open(old, $"inventory Statement.AlterIndex (a selective index's path `{one.Trim()}` that is not added, altered or removed)");
			});

			return new Ast.IndexAlteration.Paths(actions)
			{
				XmlNamespaces = old.Namespaces is null ? [] : Queries.XmlNamespaces(old, old.Namespaces),
				Options       = Each(old.Options ?? [], Option),
			};
		}

		/// <summary>ALTER INDEX: one index or ALL, and the action, a selective XML index's paths among them.</summary>
		/// <summary>ALTER INDEX of a selective XML index's paths, on the common tree.</summary>
		internal static Statement AlterIndexPaths(string name, string on, SqlList<Clause>? options, SqlList<string>? paths, string? namespaces)
		{
			var read = new Statement.AlterIndex(name, on, "FOR", null, options, paths, namespaces);

			return Hold(read, static read => AlterIndex(read));
		}

		/// <summary>ALTER INDEX, on the common tree: the index or ALL, the table, and the action its rules read.</summary>
		internal static Statement AlterIndex(string name, string on, Statement.AlterIndex action)
		{
			var read = action with { Name = name, On = on };

			return Hold(read, static read => AlterIndex(read));
		}

		static Ast.Statement AlterIndex(Statement.AlterIndex old)
		{
			var all = old.Name == "ALL";

			return new Ast.Statement.AlterIndex(Values.Name(old, old.On), old.Paths is not null ? Paths(old) : IndexAlteration(old, old.Action, old.Partition, old.Options))
			{
				Name = all ? null : Values.Part(old, old.Name),
				All  = all,
			};
		}

		static Ast.IndexAlteration IndexAlteration(object owner, string action, Expression? partition, SqlList<Clause>? options)
		{
			var made = Each(options ?? [], Option);

			if (partition is not null && action is not ("REBUILD" or "REORGANIZE"))
				throw BridgeGap.Open(owner, $"inventory Statement.AlterIndex (a partition on {action})");

			if (made.Count > 0 && action is "DISABLE" or "PAUSE" or "ABORT")
				throw BridgeGap.Open(owner, $"inventory Statement.AlterIndex (options on {action})");

			return action switch
			{
				"REBUILD" when partition is Expression.ColumnReference("ALL") => new Ast.IndexAlteration.Rebuild(null, true, made),
				"REBUILD"    => new Ast.IndexAlteration.Rebuild(partition is null ? null : Values.Of(partition), false, made),
				"REORGANIZE" => new Ast.IndexAlteration.Reorganize(partition is null ? null : Values.Of(partition), made),
				"DISABLE"    => new Ast.IndexAlteration.Disable(),
				"SET"        => new Ast.IndexAlteration.SetOptions(made),
				"RESUME"     => new Ast.IndexAlteration.Resume(made),
				"PAUSE"      => new Ast.IndexAlteration.Pause(),
				"ABORT"      => new Ast.IndexAlteration.Abort(),
				_            => throw BridgeGap.Open(owner, $"inventory Statement.AlterIndex ({action})"),
			};
		}

		/// <summary>ALTER TABLE … ALTER INDEX i REBUILD WITH (…), which the old tree keeps as the action's tail.</summary>
		static Ast.AlterTableAction AlteredIndex(Statement.AlterTable old)
		{
			var tail   = Tail(old);
			var tokens = SqlTokens.Of(tail);
			var with   = WithAt(tail);

			if (tokens is not [{ Text: var word }, { Kind: SqlTokens.Kind.Word or SqlTokens.Kind.Delimited } name, { Text: var rebuild }, ..]
				|| !word.Equals("INDEX", StringComparison.OrdinalIgnoreCase) || !rebuild.Equals("REBUILD", StringComparison.OrdinalIgnoreCase) || with < 0)
				throw BridgeGap.Open(old, $"inventory Statement.AlterTable (ALTER {tail})");

			return new Ast.AlterTableAction.AlterIndex(Values.Part(old, name.Text), new Ast.IndexAlteration.Rebuild(null, false, OptionsOf(old, tail[(with + "WITH".Length)..])));
		}

		/// <summary>
		/// <c>ON PARTITIONS (1, 3 TO 5)</c>, which the old tree keeps as text, read again by TRUNCATE TABLE's list of
		/// partitions.
		/// </summary>
		static SqlList<Ast.PartitionRange> Ranges(object owner, string text)
		{
			var words = Words(text);

			if (words is not ["ON", "PARTITIONS", "(", ..])
				throw BridgeGap.Open(owner, $"inventory Clause.Option (partitions `{text}`)");

			var list = text[text.IndexOf('(')..];
			return Skim("TRUNCATE TABLE t WITH (PARTITIONS " + list + ")") is [Statement.Built { Given: Statement.Truncate { Partitions: { } partitions } }]
				? Each(partitions, one => one is Clause.PartitionRange(var from, var to)
					? new Ast.PartitionRange(Values.Of(from), to is null ? null : Values.Of(to))
					: throw BridgeGap.Open(owner, "inventory Clause.PartitionRange"))
				: throw BridgeGap.Open(owner, $"inventory Clause.Option (partitions `{text}` the list does not read)");
		}

		/// <summary>
		/// CREATE VIEW, ALTER VIEW, CREATE OR ALTER VIEW: the standard's view, with T-SQL's verb and attributes; Synapse's
		/// materialized view, its query and its options. A query with OPTION is a gap.
		/// </summary>
		internal static Ast.Statement View(Statement.ViewDefinition old)
		{
			if (StatementOf(old.Body) is not Ast.Statement.Select { Hints.Count: 0 } select)
				throw BridgeGap.Open(old, "inventory Statement.ViewDefinition (a query with OPTION, which CreateView cannot hold)");

			// The grammar reads ALTER and CREATE OR ALTER before MATERIALIZED VIEW … AS SELECT, which the engine does not
			// (its ALTER MATERIALIZED VIEW rebuilds or disables); the record holds CREATE alone.
			if (old.Materialized && !old.Verb.Equals("CREATE", StringComparison.OrdinalIgnoreCase))
				throw BridgeGap.Open(old, $"inventory Statement.ViewDefinition (a materialized view with the verb {old.Verb})");

			if (old.Materialized)
				return new Ast.Statement.CreateMaterializedView(Values.Name(old, old.Name), select.Query)
				{
					Options = Each(old.Options ?? [], GenericOption),
				};

			return new Ast.Statement.CreateView
			{
				Name          = Values.Name(old, old.Name),
				Specification = new Ast.ViewSpecification.Regular(Values.Parts(old, old.Columns)),
				Query         = select.Query,
				CheckOption   = old.CheckOption ? Ast.CheckOption.Unqualified : null,
				Verb          = old.Verb.ToUpperInvariant() switch
				{
					"CREATE"          => null,
					"ALTER"           => Ast.DefinitionVerb.Alter,
					"CREATE OR ALTER" => Ast.DefinitionVerb.CreateOrAlter,
					_                 => throw BridgeGap.Open(old, $"inventory Statement.ViewDefinition (the verb {old.Verb})"),
				},
				Attributes    = Each(old.Options ?? [], one => one is Clause.Option { Name: var name, Value: null, Options.Length: 0, Partitions: null } ? name.ToUpperInvariant() switch
				{
					"ENCRYPTION"    => Ast.ViewAttribute.Encryption,
					"SCHEMABINDING" => Ast.ViewAttribute.SchemaBinding,
					"VIEW_METADATA" => Ast.ViewAttribute.ViewMetadata,
					_               => throw BridgeGap.Open(old, $"inventory Statement.ViewDefinition (the attribute {name})"),
				} : throw BridgeGap.Open(old, "inventory Statement.ViewDefinition (an attribute that is not one word)")),
			};
		}

		/// <summary>
		/// CREATE STATISTICS, which the old tree keeps as text after the name: the table, the columns, the filter and
		/// the options.
		/// </summary>
		/// <summary>CREATE STATISTICS, on the common tree: its name and the words after it.</summary>
		internal static Statement CreateStatistics(string name, string? tail)
		{
			var old = new Defining("Statement.StatisticsDefinition", name) { Tail = tail };

			return Hold(old, static old => CreateStatistics(old));
		}

		/// <summary>UPDATE STATISTICS, on the common tree: the table and the words after it.</summary>
		internal static Statement UpdateStatistics(string on, string? tail)
		{
			var old = new Defining("Statement.UpdateStatistics", on) { Tail = tail };

			return Hold(old, static old => UpdateStatistics(old));
		}

		static Ast.Statement CreateStatistics(Defining old)
		{
			var tail   = old.Tail ?? throw BridgeGap.Open(old, "inventory Statement.StatisticsDefinition (nothing after the name)");
			var tokens = Located(tail);

			if (tokens is not [{ Token.Text: var on }, ..] || !on.Equals("ON", StringComparison.OrdinalIgnoreCase))
				throw BridgeGap.Open(old, $"inventory Statement.StatisticsDefinition ({tail})");

			var open  = tokens.FindIndex(static one => one.Token.Text == "(");
			var close = open < 0 ? -1 : Closing(tokens, open);

			if (close < 0)
				throw BridgeGap.Open(old, $"inventory Statement.StatisticsDefinition ({tail})");

			var table   = Values.Name(old, tail[tokens[1].At..tokens[open].At].Trim());
			var columns = Listed(tail[(tokens[open].At + 1)..tokens[close].At]).Select(one => Values.Part(old, one)).ToSqlList();
			var rest    = tail[(tokens[close].At + 1)..];
			var with    = TopLevel(rest, "WITH");
			var head    = with < 0 ? rest : rest[..with];

			Ast.Expression? where = null;

			if (Words(head) is ["WHERE", ..])
				where = Condition(old, head[(head.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase) + "WHERE".Length)..]);
			else if (Words(head).Count > 0)
				throw BridgeGap.Open(old, $"inventory Statement.StatisticsDefinition ({tail})");

			return new Ast.Statement.CreateStatistics(table, Values.Part(old, old.Name), columns)
			{
				Where   = where,
				Options = with < 0 ? [] : StatisticsOptions(old, rest[(with + "WITH".Length)..]),
			};
		}

		/// <summary>UPDATE STATISTICS, which the old tree keeps as text after the table: the names, and the options.</summary>
		static Ast.Statement UpdateStatistics(Defining old)
		{
			var tail = old.Tail ?? "";
			var with = TopLevel(tail, "WITH");
			var head = (with < 0 ? tail : tail[..with]).Trim();

			SqlList<Ast.Identifier> names = [];
			var unbracketed = false;

			if (head.StartsWith('(') && head.EndsWith(')'))
				names = Listed(head[1..^1]).Select(one => Values.Part(old, one)).ToSqlList();
			else if (head.Length > 0)
			{
				names       = [Values.Part(old, head)];
				unbracketed = true;
			}

			return new Ast.Statement.UpdateStatistics(Values.Name(old, old.Name))
			{
				Names           = names,
				NameUnbracketed = unbracketed,
				Options         = with < 0 ? [] : StatisticsOptions(old, tail[(with + "WITH".Length)..]),
			};
		}

		/// <summary>
		/// Statistics' options, as text: a word, a word <c>=</c> a value, <c>SAMPLE n PERCENT</c> or <c>ROWS</c> as a
		/// measured value, and <c>RESAMPLE ON PARTITIONS (…)</c>.
		/// </summary>
		static SqlList<Ast.Option> StatisticsOptions(object owner, string text)
		{
			return Listed(text).Select(one =>
			{
				var tokens = SqlTokens.Of(one);
				var words  = Words(one);

				switch (words)
				{
					case [_] when tokens[0].Kind == SqlTokens.Kind.Word:
						return new Ast.Option(new Ast.Identifier(tokens[0].Text), null, []);

					case ["SAMPLE", _, "PERCENT" or "ROWS"] when tokens[1].Kind == SqlTokens.Kind.Number:
						return new Ast.Option(new Ast.Identifier(tokens[0].Text), new Ast.Expression.Measured(ValueOf(owner, tokens[1].Text), new Ast.Identifier(tokens[2].Text)), []);

					case ["RESAMPLE", "ON", "PARTITIONS", ..]:
						return new Ast.Option(new Ast.Identifier(tokens[0].Text), null, []) { Partitions = Ranges(owner, one[one.IndexOf(tokens[1].Text, StringComparison.Ordinal)..]) };

					// A key word a value is set to is a one-part reference, as every option's is.
					case [_, "=", _, ..] when tokens[0].Kind == SqlTokens.Kind.Word:
						var value = one[(one.IndexOf('=') + 1)..].Trim();

						return new Ast.Option(new Ast.Identifier(tokens[0].Text), words is [_, _, _] && tokens[2].Kind == SqlTokens.Kind.Word
							? Values.Of(new Expression.ColumnReference(tokens[2].Text))
							: ValueOf(owner, value), []);

					default:
						throw BridgeGap.Open(owner, $"inventory {NameOf(owner)} (the option `{one.Trim()}`)");
				}
			}).ToSqlList();
		}

		/// <summary>A search condition kept as text, read again as a query's WHERE.</summary>
		static Ast.Expression Condition(object owner, string text)
		{
			return Queries.Specified("SELECT 1 WHERE " + text.Trim()) is { Where: { } where }
				? Values.Of(where)
				: throw BridgeGap.Open(owner, $"inventory {NameOf(owner)} (a condition `{text.Trim()}`)");
		}

		/// <summary>
		/// CREATE TYPE, which the old tree keeps as text after the name: a type made of another, with its nullability,
		/// or of a table, whose body is read again as a table's. A type of an assembly's class is a gap.
		/// </summary>
		internal static Ast.Statement CreateType(Defining old)
		{
			var tail  = old.Tail ?? throw BridgeGap.Open(old, "inventory Statement.TypeDefinition (nothing after the name)");
			var words = Words(tail);
			var name  = Values.Name(old, old.Name);

			switch (words)
			{
				case ["FROM", ..]:
					var type        = tail[(tail.IndexOf("FROM", StringComparison.OrdinalIgnoreCase) + "FROM".Length)..].Trim();
					Ast.ColumnNullability? nullability = null;

					if (words is [.., "NOT", "NULL"])
					{
						nullability = Ast.ColumnNullability.NotNull;
						type        = Cut(type, 2);
					}
					else if (words is [.., "NULL"])
					{
						nullability = Ast.ColumnNullability.Null;
						type        = Cut(type, 1);
					}

					return new Ast.Statement.CreateAliasType(name, Values.Type(old, type)) { Nullability = nullability };

				case ["AS", "TABLE", "(", ..]:
					// The body read as a table's, its parts carried with the text it is read from.
					var body = "CREATE TABLE t " + tail[(tail.IndexOf('('))..];
					var read = Inside(body, () => Reread(body, located: true));

					if (read is not { IsSuccess: true, Value: [Statement.Built table] }
						|| StatementOf(table) is not Ast.Statement.CreateTable { Contents: Ast.TableContents.Elements(var elements), Kind: null, Placements.Count: 0 } made)
						throw BridgeGap.Open(old, "inventory Statement.TypeDefinition (a table type's body that a table's does not read)");

					return new Ast.Statement.CreateTableType(name, elements) { Options = made.Options };

				case ["EXTERNAL", "NAME", ..]:
					return Routines.ClrType(old);

				default:
					throw BridgeGap.Open(old, $"inventory Statement.TypeDefinition ({tail})");
			}
		}

		/// <summary>A text less its last tokens, which are words.</summary>
		static string Cut(string text, int words)
		{
			var tokens = Located(text);

			return text[..tokens[^words].At].Trim();
		}

		/// <summary>CREATE SYNONYM, which the old tree keeps as text after the name: <c>FOR</c> and the name stood for.</summary>
		internal static Ast.Statement Synonym(Defining old)
		{
			var tail = old.Tail ?? "";

			if (Words(tail) is not ["FOR", ..])
				throw BridgeGap.Open(old, $"inventory Statement.SynonymDefinition ({tail})");

			return new Ast.Statement.CreateSynonym(Values.Name(old, old.Name), Values.Name(old, tail[(tail.IndexOf("FOR", StringComparison.OrdinalIgnoreCase) + "FOR".Length)..].Trim()));
		}

		/// <summary>
		/// CREATE FULLTEXT INDEX, which the old tree keeps as text after the table: the columns, the key index, the
		/// catalog and filegroup, and the options. The options are carried as every option is; a catalog alone in
		/// brackets, and the filegroup before the catalog, are gaps.
		/// </summary>
		internal static Ast.Statement CreateFullTextIndex(Defining old)
		{
			var tail   = old.Tail ?? throw BridgeGap.Open(old, "inventory Statement.FullTextIndexDefinition (nothing after the table)");
			var tokens = Located(tail);
			var at     = 0;

			SqlList<Ast.FullTextIndexColumn> columns = [];

			if (tokens is [{ Token.Text: "(" }, ..])
			{
				var close = Closing(tokens, 0);

				columns = Listed(tail[1..tokens[close].At]).Select(one => FullTextColumn(old, one)).ToSqlList();
				at      = close + 1;
			}

			if (!Is(tokens, at, "KEY") || !Is(tokens, at + 1, "INDEX") || at + 2 >= tokens.Count)
				throw BridgeGap.Open(old, $"inventory Statement.FullTextIndexDefinition ({tail})");

			var key = Values.Part(old, tokens[at + 2].Token.Text);

			at += 3;

			Ast.Identifier? catalog   = null;
			Ast.Identifier? fileGroup = null;
			var first     = false;
			var bracketed = false;

			if (Is(tokens, at, "ON"))
			{
				if (at + 1 < tokens.Count && tokens[at + 1].Token.Text == "(")
				{
					var close  = Closing(tokens, at + 1);
					var places = Listed(tail[(tokens[at + 1].At + 1)..tokens[close].At]);

					// FullTextPlace, in either order: the filegroup with its word, the catalog alone, each once.
					foreach (var place in places)
					{
						if (Words(place) is ["FILEGROUP", _] && fileGroup is null)
						{
							fileGroup = Values.Part(old, SqlTokens.Of(place)[1].Text);
							first     = catalog is null;
						}
						else if (Words(place).Count == 1 && catalog is null)
							catalog = Values.Part(old, place.Trim());
						else
							throw BridgeGap.Open(old, "inventory Statement.FullTextIndexDefinition (a place of other words, or one said twice)");
					}

					first     = first && catalog is not null;
					bracketed = fileGroup is null;
					at        = close + 1;
				}
				else
				{
					catalog = Values.Part(old, tokens[at + 1].Token.Text);
					at     += 2;
				}
			}

			SqlList<Ast.Option> options = [];
			var unbracketed = false;

			if (Is(tokens, at, "WITH"))
			{
				var list = tail[(tokens[at].At + "WITH".Length)..].Trim();

				unbracketed = !(list.StartsWith('(') && Closing(Located(list), 0) == Located(list).Count - 1);
				options     = Listed(unbracketed ? list : list.Trim()[1..^1]).Select(one => FullTextOption(old, one)).ToSqlList();
			}
			else if (at < tokens.Count)
				throw BridgeGap.Open(old, $"inventory Statement.FullTextIndexDefinition ({tail})");

			return new Ast.Statement.CreateFullTextIndex(Values.Name(old, old.Name), columns, key)
			{
				Catalog            = catalog,
				FileGroup          = fileGroup,
				FileGroupFirst     = first,
				CatalogBracketed   = bracketed,
				Options            = options,
				OptionsUnbracketed = unbracketed,
			};
		}

		/// <summary>
		/// One option of a full-text index, as the grammar's FullTextOption reads it: <c>CHANGE_TRACKING [=] mode</c>,
		/// <c>STOPLIST [=] s</c>, <c>SEARCH PROPERTY LIST [=] p</c>, and <c>NO POPULATION</c>. A name of several words keeps
		/// its first word as the name and the rest as its words; the value is the word or name after them; and the equals
		/// sign, which the place reads or not for each name, is kept as <c>EqualsOmitted</c>.
		/// </summary>
		static Ast.Option FullTextOption(object owner, string text)
		{
			var tokens = SqlTokens.Of(text);
			var words  = tokens.Select(static one => one.Kind == SqlTokens.Kind.Word ? one.Text.ToUpperInvariant() : one.Text).ToList();

			Ast.Option Valued(int named)
			{
				var omitted = words[named] != "=";
				var at      = omitted ? named : named + 1;

				if (at != tokens.Count - 1 || tokens[at].Kind is not (SqlTokens.Kind.Word or SqlTokens.Kind.Delimited))
					throw BridgeGap.Open(owner, $"inventory Statement.FullTextIndexDefinition (an option `{text.Trim()}`)");

				return new Ast.Option(Values.Part(owner, tokens[0].Text), new Ast.Expression.Reference(new Ast.QualifiedName([Values.Part(owner, tokens[at].Text)])), [])
				{
					Words         = [.. tokens.Take(named).Skip(1).Select(one => Values.Part(owner, one.Text))],
					EqualsOmitted = omitted,
				};
			}

			return words switch
			{
				["NO", "POPULATION"]                                          => new Ast.Option(Values.Part(owner, tokens[0].Text), null, []) { Words = [Values.Part(owner, tokens[1].Text)] },
				["CHANGE_TRACKING" or "STOPLIST", _, ..] when tokens.Count <= 3 => Valued(1),
				["SEARCH", "PROPERTY", "LIST", _, ..] when tokens.Count <= 5     => Valued(3),
				_ => throw BridgeGap.Open(owner, $"inventory Statement.FullTextIndexDefinition (an option `{text.Trim()}`)"),
			};
		}

		/// <summary>One column of a full-text index, as text: the name, its type column, its language, its semantics.</summary>
		static Ast.FullTextIndexColumn FullTextColumn(object owner, string text)
		{
			var tokens = Located(text);
			var column = new Ast.FullTextIndexColumn(Values.Part(owner, tokens[0].Token.Text));
			var at     = 1;

			if (Is(tokens, at, "TYPE") && Is(tokens, at + 1, "COLUMN") && at + 2 < tokens.Count)
			{
				column = column with { TypeColumn = Values.Part(owner, tokens[at + 2].Token.Text) };
				at    += 3;
			}

			var semantics = tokens.Count > at && Is(tokens, tokens.Count - 1, "STATISTICAL_SEMANTICS");
			var end       = semantics ? tokens.Count - 1 : tokens.Count;

			if (Is(tokens, at, "LANGUAGE") && at + 1 < end)
			{
				var language = text[tokens[at + 1].At..(end < tokens.Count ? tokens[end].At : text.Length)];

				// A lone word is a name, unless it is a key word the grammar's ValueExpression reads as a literal: NULL.
				column = column with { Language = Words(language) is [var word] && !word.Equals("NULL", StringComparison.OrdinalIgnoreCase)
					&& SqlTokens.Of(language)[0].Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited
					? Values.Of(new Expression.ColumnReference(language.Trim()))
					: ValueOf(owner, language) };
				at = end;
			}

			if (at != end)
				throw BridgeGap.Open(owner, $"inventory Statement.FullTextIndexDefinition (a column `{text.Trim()}`)");

			return column with { StatisticalSemantics = semantics };
		}

		/// <summary>
		/// ALTER FULLTEXT INDEX, which the old tree keeps as text after the table: one action, and <c>WITH NO
		/// POPULATION</c> after it as the action's one option, the name <c>NO</c> with the word <c>POPULATION</c>.
		/// </summary>
		internal static Ast.Statement AlterFullTextIndex(Defining old)
		{
			var tail   = old.Tail ?? throw BridgeGap.Open(old, "inventory Statement.AlterFullTextIndex (nothing after the table)");
			var tokens = Located(tail);
			var words  = Words(tail);

			SqlList<Ast.Option> population = [];

			if (words is [.., "WITH", "NO", "POPULATION"])
			{
				population = [new Ast.Option(Values.Part(old, tokens[^2].Token.Text), null, []) { Words = [Values.Part(old, tokens[^1].Token.Text)] }];
				tail       = tail[..tokens[^3].At].TrimEnd();
				tokens     = Located(tail);
				words      = Words(tail);
			}

			Ast.Identifier Name(int at)
			{
				return Values.Part(old, tokens[at].Token.Text);
			}

			Ast.FullTextAlteration action = words switch
			{
				["ENABLE"]  => new Ast.FullTextAlteration.Enable(),
				["DISABLE"] => new Ast.FullTextAlteration.Disable(),
				["SET", "CHANGE_TRACKING", "=", _]                           => new Ast.FullTextAlteration.SetChangeTracking(Name(3)),
				["SET", "CHANGE_TRACKING", _]                                => new Ast.FullTextAlteration.SetChangeTracking(Name(2)) { EqualsOmitted = true },
				["SET", "STOPLIST", "=", _]                                  => StopList(old, tokens, 3, false),
				["SET", "STOPLIST", _]                                       => StopList(old, tokens, 2, true),
				["SET", "SEARCH", "PROPERTY", "LIST", "=", _]                => PropertyList(old, tokens, 5, false),
				["SET", "SEARCH", "PROPERTY", "LIST", _]                     => PropertyList(old, tokens, 4, true),
				["ADD", "(", .., ")"]                                        => new Ast.FullTextAlteration.AddColumns(Listed(tail[(tokens[1].At + 1)..tokens[^1].At]).Select(one => FullTextColumn(old, one)).ToSqlList()),
				["ALTER", "COLUMN", _, "ADD" or "DROP", "STATISTICAL_SEMANTICS"] => new Ast.FullTextAlteration.AlterColumn(new Ast.FullTextIndexColumn(Name(2)), words[3] == "ADD"),
				["DROP", "(", .., ")"]                                       => new Ast.FullTextAlteration.DropColumns(Listed(tail[(tokens[1].At + 1)..tokens[^1].At]).Select(one => Values.Part(old, one)).ToSqlList()),
				["START", "FULL" or "INCREMENTAL" or "UPDATE", "POPULATION"] => new Ast.FullTextAlteration.Population(Ast.PopulationAction.Start, words[1] switch
				{
					"FULL"        => Ast.PopulationKind.Full,
					"INCREMENTAL" => Ast.PopulationKind.Incremental,
					_             => Ast.PopulationKind.Update,
				}),
				["STOP", "POPULATION"]   => new Ast.FullTextAlteration.Population(Ast.PopulationAction.Stop, null),
				["PAUSE", "POPULATION"]  => new Ast.FullTextAlteration.Population(Ast.PopulationAction.Pause, null),
				["RESUME", "POPULATION"] => new Ast.FullTextAlteration.Population(Ast.PopulationAction.Resume, null),
				_                        => throw BridgeGap.Open(old, $"inventory Statement.AlterFullTextIndex ({tail})"),
			};

			return new Ast.Statement.AlterFullTextIndex(Values.Name(old, old.Name), action with { Options = population });
		}

		static Ast.FullTextAlteration StopList(object owner, List<(SqlTokens.Token Token, int At)> tokens, int at, bool omitted)
		{
			var word = tokens[at].Token;

			return word.Kind == SqlTokens.Kind.Word && word.Text.Equals("OFF", StringComparison.OrdinalIgnoreCase) ? new Ast.FullTextAlteration.SetStopList(null, true, false) { EqualsOmitted = omitted }
				: word.Kind == SqlTokens.Kind.Word && word.Text.Equals("SYSTEM", StringComparison.OrdinalIgnoreCase) ? new Ast.FullTextAlteration.SetStopList(null, false, true) { EqualsOmitted = omitted }
				: new Ast.FullTextAlteration.SetStopList(Values.Part(owner, word.Text), false, false) { EqualsOmitted = omitted };
		}

		static Ast.FullTextAlteration PropertyList(object owner, List<(SqlTokens.Token Token, int At)> tokens, int at, bool omitted)
		{
			var word = tokens[at].Token;

			return word.Kind == SqlTokens.Kind.Word && word.Text.Equals("OFF", StringComparison.OrdinalIgnoreCase)
				? new Ast.FullTextAlteration.SetSearchPropertyList(null, true) { EqualsOmitted = omitted }
				: new Ast.FullTextAlteration.SetSearchPropertyList(Values.Part(owner, word.Text), false) { EqualsOmitted = omitted };
		}

		// ── Text the old tree keeps, cut where its brackets and commas say ───────────────────────────────

		/// <summary>The tokens of a text, each with where it starts in it.</summary>
		static List<(SqlTokens.Token Token, int At)> Located(string text)
		{
			var located = new List<(SqlTokens.Token, int)>();
			var at      = 0;

			foreach (var token in SqlTokens.Of(text))
			{
				var found = text.IndexOf(token.Text, at, StringComparison.Ordinal);

				located.Add((token, found));
				at = found + token.Text.Length;
			}

			return located;
		}

		/// <summary>Whether the token at a position is the word given.</summary>
		static bool Is(List<(SqlTokens.Token Token, int At)> tokens, int at, string word)
		{
			return at < tokens.Count && tokens[at].Token.Kind == SqlTokens.Kind.Word && tokens[at].Token.Text.Equals(word, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>The bracket that closes the one at a position, or -1.</summary>
		static int Closing(List<(SqlTokens.Token Token, int At)> tokens, int open)
		{
			var depth = 0;

			for (var at = open; at < tokens.Count; at++)
			{
				if (tokens[at].Token is { Kind: SqlTokens.Kind.Punctuation, Text: "(" })
					depth++;
				else if (tokens[at].Token is { Kind: SqlTokens.Kind.Punctuation, Text: ")" } && --depth == 0)
					return at;
			}

			return -1;
		}

		/// <summary>Where a word stands outside every bracket, or -1.</summary>
		static int TopLevel(string text, string word)
		{
			var depth = 0;

			foreach (var (token, at) in Located(text))
			{
				if (token is { Kind: SqlTokens.Kind.Punctuation, Text: "(" })
					depth++;
				else if (token is { Kind: SqlTokens.Kind.Punctuation, Text: ")" })
					depth--;
				else if (depth == 0 && token.Kind == SqlTokens.Kind.Word && token.Text.Equals(word, StringComparison.OrdinalIgnoreCase))
					return at;
			}

			return -1;
		}

		/// <summary>A text cut at its commas outside brackets, each piece trimmed.</summary>
		static List<string> Listed(string text)
		{
			var pieces = new List<string>();
			var depth  = 0;
			var from   = 0;

			foreach (var (token, at) in Located(text))
			{
				if (token is { Kind: SqlTokens.Kind.Punctuation, Text: "(" })
					depth++;
				else if (token is { Kind: SqlTokens.Kind.Punctuation, Text: ")" })
					depth--;
				else if (depth == 0 && token is { Kind: SqlTokens.Kind.Punctuation, Text: "," })
				{
					pieces.Add(text[from..at].Trim());
					from = at + 1;
				}
			}

			pieces.Add(text[from..].Trim());

			return pieces;
		}

		/// <summary>
		/// The words that open a sequence's option, after which a data type's text ends.
		/// </summary>
		static readonly string[] Openers = ["START", "INCREMENT", "MINVALUE", "MAXVALUE", "NO", "CYCLE", "CACHE", "RESTART"];

		/// <summary>
		/// <c>CREATE SEQUENCE</c> and <c>ALTER SEQUENCE</c>. The old tree keeps the options twice: as text, which
		/// says <c>NO</c> and the type, and as nodes, which hold the values; the two are read side by side, in the
		/// order written.
		/// </summary>
		internal static Ast.Statement Sequence(Defining old)
		{
			var words   = old.Tail is { } tail ? Words(tail) : [];
			var options = old.Options ?? [];
			var made    = new List<Ast.SequenceOption>();
			var at      = 0;

			foreach (var option in options)
			{
				if (option is not Clause.Option(var name, var value, _, _, _))
					throw BridgeGap.Open(old, "inventory Statement.SequenceDefinition (an option that is not one)");

				var word = name.ToUpperInvariant();
				bool no;

				// Find the option's own word in the text, and whether NO stood before it.
				while (at < words.Count && words[at] != word)
					at++;

				if (at >= words.Count)
					throw BridgeGap.Lost(old, $"the sequence option {name} is not where the text has it");

				no = at > 0 && words[at - 1] == "NO";
				at++;

				made.Add(word switch
				{
					"AS"        => new Ast.SequenceOption.DataTypeOption(Values.Type(old, TypeText(old.Tail!, word))),
					"START"     => new Ast.SequenceOption.Start(Values.Of(value!)),
					"INCREMENT" => new Ast.SequenceOption.Increment(Values.Of(value!)),
					"MINVALUE"  => no ? new Ast.SequenceOption.Min(null, true) : new Ast.SequenceOption.Min(Values.Of(value!)),
					"MAXVALUE"  => no ? new Ast.SequenceOption.Max(null, true) : new Ast.SequenceOption.Max(Values.Of(value!)),
					"CYCLE"     => new Ast.SequenceOption.Cycle(!no),
					"RESTART"   => new Ast.SequenceOption.Restart(value is null ? null : Values.Of(value)),
					"CACHE"     => no ? new Ast.SequenceOption.NoCache() : new Ast.SequenceOption.Cache(value is null ? null : Values.Of(value)),
					_           => throw BridgeGap.Open(old, $"inventory Statement.SequenceDefinition (the option {name})"),
				});
			}

			var sequence = Values.Name(old, old.Name);

			return old.Verb?.ToUpperInvariant() switch
			{
				"CREATE" => new Ast.Statement.CreateSequence { Name = sequence, Options = [.. made] },
				"ALTER"  => new Ast.Statement.AlterSequence { Name = sequence, Options = [.. made] },
				_        => throw BridgeGap.Open(old, $"inventory Statement.SequenceDefinition (the verb {old.Verb})"),
			};
		}

		/// <summary>
		/// The text of the type after <c>AS</c>: up to the word that opens the next option.
		/// </summary>
		static string TypeText(string tail, string word)
		{
			var start = tail.IndexOf(word, StringComparison.OrdinalIgnoreCase) + word.Length;
			var end   = tail.Length;

			foreach (var opener in Openers)
			{
				var at = tail.IndexOf(" " + opener, start, StringComparison.OrdinalIgnoreCase);

				if (at >= 0 && at < end)
					end = at;
			}

			return tail[start..end].Trim();
		}
	}
}
