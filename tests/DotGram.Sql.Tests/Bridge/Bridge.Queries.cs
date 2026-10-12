using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// <c>SELECT</c>: the statement, the query and its set operations, the select list, <c>ORDER BY</c>,
	/// <c>OFFSET</c> and <c>FETCH</c>, <c>TOP</c>, <c>GROUP BY</c>, <c>WITH</c>.
	/// </summary>
	internal static class Queries
	{
		public static readonly string[] Owns =
		[
			"Statement.Select",
			"Query.Specification", "Query.TableValueConstructor", "Query.Parenthesized", "Query.Ordered",
			"Query.ExplicitTable", "Query.Union", "Query.Except", "Query.Intersect",
			"Clause.DerivedColumn", "Clause.QualifiedAsterisk", "Clause.SortSpecification", "Clause.OrderBy",
			"Clause.Top", "Clause.GroupBy", "Clause.Into", "Clause.For", "Clause.Hint",
			"Clause.CommonTableExpression", "Clause.VariableAssignment",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				Statement.Select select                            => Select(select),
				Query query when Owned(Owns, query)                => Of(query),
				Clause.DerivedColumn or Clause.QualifiedAsterisk
					or Clause.VariableAssignment                   => Column((Clause)old),
				Clause.CommonTableExpression common                => Common(common),
				Clause.SortSpecification sort                      => Sort(sort),
				Clause when Owned(Owns, old)                       => throw BridgeGap.Open(old, $"inventory {NameOf(old)} (stands only inside a query)"),
				_                                                  => null,
			};

			return made is not null;
		}

		static Ast.Statement Select(Statement.Select old)
		{
			if (old.For.Length > 1)
				throw BridgeGap.Open(old.For[1], "inventory Clause.For (a second FOR on one statement)");

			var query = Of(old.Of);

			if (old.OrderBy is { } by)
				query = Ordered(query, by);

			if (old.For is [var shape])
				query = query with { For = For(shape) };

			if (old.With.Length > 0)
				query = query with { With = With(old.With) };

			return new Ast.Statement.Select { Query = query, Hints = Each(old.Options, QueryHint) };
		}

		/// <summary>
		/// A WITH: its common table expressions, and T-SQL's <c>XMLNAMESPACES (…)</c>, which the old tree keeps as
		/// an option whose name is the text and which stands first.
		/// </summary>
		public static Ast.WithClause With(SqlList<Clause> with)
		{
			var namespaces = with is [Clause.Option { Name: var text } first, ..] && Words(text) is ["XMLNAMESPACES", ..]
				? XmlNamespaces(first, text)
				: [];

			return new Ast.WithClause(false, Each(namespaces.Count > 0 ? with[1..] : with, static one => one is Clause.CommonTableExpression common
				? Common(common)
				: throw BridgeGap.Open(one, $"inventory {NameOf(one)} (in a WITH)")))
			{
				XmlNamespaces = namespaces,
			};
		}

		/// <summary>
		/// <c>XMLNAMESPACES ('uri' AS ns, DEFAULT 'uri')</c>, read from the old option's text.
		/// </summary>
		static SqlList<Ast.XmlNamespace> XmlNamespaces(Clause old, string text)
		{
			var tokens = SqlTokens.Of(text);

			if (tokens is not [_, { Text: "(" }, .., { Text: ")" }])
				throw BridgeGap.Open(old, "inventory Clause.Option (an XMLNAMESPACES that does not read)");

			var made = new List<Ast.XmlNamespace>();

			foreach (var item in Split(tokens[2..^1]))
			{
				made.Add(item switch
				{
					[{ Kind: SqlTokens.Kind.String } uri, { Kind: SqlTokens.Kind.Word } word, var prefix] when word.Text.Equals("AS", StringComparison.OrdinalIgnoreCase)
						&& prefix.Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited =>
						new Ast.XmlNamespace(Values.String(uri.Text), Values.Part(old, prefix.Text)),

					[{ Kind: SqlTokens.Kind.Word } word, { Kind: SqlTokens.Kind.String } uri] when word.Text.Equals("DEFAULT", StringComparison.OrdinalIgnoreCase) =>
						new Ast.XmlNamespace(Values.String(uri.Text), null),

					_ => throw BridgeGap.Open(old, "inventory Clause.Option (an XMLNAMESPACES item that does not read)"),
				});
			}

			return [.. made];
		}

		/// <summary>
		/// Tokens cut at the commas that stand outside brackets.
		/// </summary>
		public static List<SqlTokens.Token[]> Split(IReadOnlyList<SqlTokens.Token> tokens)
		{
			var items = new List<SqlTokens.Token[]>();
			var item  = new List<SqlTokens.Token>();
			var depth = 0;

			foreach (var token in tokens)
			{
				if (token.Kind == SqlTokens.Kind.Punctuation)
				{
					if (token.Text == "(")
						depth++;
					else if (token.Text == ")")
						depth--;
					else if (token.Text == "," && depth == 0)
					{
						items.Add([.. item]);
						item.Clear();

						continue;
					}
				}

				item.Add(token);
			}

			items.Add([.. item]);

			return items;
		}

		// ── FOR, OPTION ───────────────────────────────────────────────────────────

		/// <summary>
		/// <c>FOR XML</c>, <c>FOR JSON</c> or <c>FOR BROWSE</c>, which the old tree keeps as a kind and an option's
		/// text each. The first is the mode; the rest are a name (<c>TYPE</c>), a name and its value in brackets
		/// (<c>ROOT ('r')</c>), or a name and the one word it takes (<c>ELEMENTS XSINIL</c>).
		/// </summary>
		public static Ast.ForClause For(Clause old)
		{
			if (old is not Clause.For(var kind, var options))
				throw BridgeGap.Open(old, "inventory Clause.For");

			var upper = kind.ToUpperInvariant();

			if (upper == "BROWSE")
				return options.Length == 0 ? new Ast.ForClause.Browse() : throw BridgeGap.Open(old, "inventory Clause.For (BROWSE with options)");

			if (upper is not ("XML" or "JSON") || options.Length == 0)
				throw BridgeGap.Open(old, $"inventory Clause.For (FOR {kind})");

			if (SqlTokens.Of(options[0]) is not [{ Kind: SqlTokens.Kind.Word } mode])
				throw BridgeGap.Missing(old, "ForClause has no element name for RAW or PATH ('…') (proposal 1)");

			var rest = Each(options[1..], one => ForOption(old, one));

			return upper == "XML"
				? new Ast.ForClause.Xml(new Ast.Identifier(mode.Text), rest)
				: new Ast.ForClause.Json(new Ast.Identifier(mode.Text), rest);
		}

		static Ast.Option ForOption(Clause old, string text)
		{
			return SqlTokens.Of(text) switch
			{
				[{ Kind: SqlTokens.Kind.Word } name] =>
					new Ast.Option(new Ast.Identifier(name.Text), null, []),

				[{ Kind: SqlTokens.Kind.Word } name, { Text: "(" }, { Kind: SqlTokens.Kind.String } value, { Text: ")" }] =>
					new Ast.Option(new Ast.Identifier(name.Text), Values.String(value.Text), []),

				[{ Kind: SqlTokens.Kind.Word } name, { Kind: SqlTokens.Kind.Word } word] =>
					new Ast.Option(new Ast.Identifier(name.Text), null, [new Ast.Option(new Ast.Identifier(word.Text), null, [])]),

				_ => throw BridgeGap.Open(old, "inventory Clause.For (an option that is not a word, a word and a string, or two words)"),
			};
		}

		/// <summary>
		/// One hint of <c>OPTION (…)</c>, which the old tree keeps as text.
		/// </summary>
		static Ast.QueryHint QueryHint(Clause old)
		{
			var (name, arguments) = Hint(old);

			return new Ast.QueryHint.Named(name, arguments);
		}

		/// <summary>
		/// A hint the old tree keeps as text, as a name and the values in its brackets: <c>RECOMPILE</c>,
		/// <c>INDEX (ix, 2)</c>. Every other shape — <c>MAXDOP 2</c>, <c>HASH JOIN</c>, <c>INDEX = ix</c>,
		/// <c>OPTIMIZE FOR (@v = 1)</c> — is not decided (proposal 9 names only the long tail's member).
		/// </summary>
		public static (Ast.Identifier Name, SqlList<Ast.Expression> Arguments) Hint(Clause old)
		{
			if (old is not Clause.Hint(var text))
				throw BridgeGap.Open(old, "inventory Clause.Hint");

			var tokens = SqlTokens.Of(text);

			if (tokens is [{ Kind: SqlTokens.Kind.Word } bare] && bare.Text[0] is not ('@' or '#' or '$'))
				return (new Ast.Identifier(bare.Text), []);

			if (tokens is [{ Kind: SqlTokens.Kind.Word } name, { Text: "(" }, .., { Text: ")" }] && name.Text[0] is not ('@' or '#' or '$') && tokens.Count > 3)
			{
				var arguments = new List<Ast.Expression>();

				foreach (var item in Split(tokens.Skip(2).Take(tokens.Count - 3).ToList()))
				{
					if (item is not [var one] || HintArgument(old, one) is not { } value)
						throw BridgeGap.Open(old, "inventory Clause.Hint (a hint that is not a name and the values in its brackets)");

					arguments.Add(value);
				}

				return (new Ast.Identifier(name.Text), [.. arguments]);
			}

			throw BridgeGap.Open(old, "inventory Clause.Hint (a hint that is not a name and the values in its brackets)");
		}

		static Ast.Expression? HintArgument(Clause old, SqlTokens.Token token)
		{
			return token.Kind switch
			{
				SqlTokens.Kind.Number                                => new Ast.Expression.Literal(new Ast.LiteralValue.Numeric(token.Text, token.Text.All(char.IsDigit) ? Ast.NumericLiteralKind.DecimalInteger : Ast.NumericLiteralKind.Decimal)),
				SqlTokens.Kind.String                                => Values.String(token.Text),
				SqlTokens.Kind.Word when token.Text.StartsWith('@')  => new Ast.Expression.Variable(new Ast.Identifier(token.Text)),
				SqlTokens.Kind.Word or SqlTokens.Kind.Delimited      => new Ast.Expression.Reference(new Ast.QualifiedName([Values.Part(old, token.Text)])),
				_                                                    => null,
			};
		}

		static Ast.CommonTableExpression Common(Clause.CommonTableExpression old)
		{
			return new Ast.CommonTableExpression(Values.Part(old, old.Name), Values.Parts(old, old.Columns), QueryOf(old.Query));
		}

		public static Ast.Query Of(Query old)
		{
			switch (old)
			{
				case Query.Specification specification:
					return Specification(specification);

				case Query.TableValueConstructor(var rows):
					return new Ast.Query.Select { Values = Each(rows, Row) };

				case Query.ExplicitTable(var name):
					return new Ast.Query.Select
					{
						Columns = [new Ast.SelectColumn.All()],
						From    = new Ast.FromClause([new Ast.TableSource.Named(Values.Name(old, name))]),
						Table   = true,
					};

				case Query.Parenthesized(var inner):
					return new Ast.Query.Parenthesized(Of(inner));

				// A bracketed query's own ORDER BY, OFFSET … FETCH and FOR, which are the query's as a statement's are.
				case Query.Ordered(var inner, var by, var shape):
				{
					var ordered = by is null ? Of(inner) : Ordered(Of(inner), by);

					return shape is null ? ordered : ordered with { For = For(shape) };
				}

				case Query.Union(var left, var right, var all):
					return Combined(left, all ? Ast.SetOperator.UnionAll : Ast.SetOperator.Union, right);

				case Query.Except(var left, var right, var all):
					return Combined(left, all ? Ast.SetOperator.ExceptAll : Ast.SetOperator.Except, right);

				case Query.Intersect(var left, var right, var all):
					return Combined(left, all ? Ast.SetOperator.IntersectAll : Ast.SetOperator.Intersect, right);

				default:
					return QueryOf(old);
			}
		}

		/// <summary>
		/// The old binary tree of set operations as the new left fold: the left side's list, and one more
		/// operation with the right side as its operand. The right side keeps its own list, which is what
		/// binds tighter there — <c>A UNION B INTERSECT C</c> is <c>A [UNION B [INTERSECT C]]</c>.
		/// </summary>
		static Ast.Query Combined(Query left, Ast.SetOperator op, Query right)
		{
			var first = Of(left);

			if (first.OrderBy is not null || first.Limit is not null || first.For is not null)
				throw BridgeGap.Open(left, "inventory Query.Union (a left operand with an order of its own and no brackets)");

			return first with { SetOperations = [.. first.SetOperations, new Ast.SetOperation(op, Of(right))] };
		}

		static Ast.Query Specification(Query.Specification old)
		{
			return new Ast.Query.Select
			{
				Top        = Top(old.Top),
				IntoTable  = old.Into is null ? null : IntoTable(old.Into),
				Quantifier = Quantifier(old, old.Quantifier),
				Columns    = Each(old.Columns, Column),
				From       = old.From.Length > 0 ? new Ast.FromClause(Each(old.From, Source)) : null,
				Where      = old.Where is null ? null : Values.Of(old.Where),
				GroupBy    = old.GroupBy is null ? null : Group(old.GroupBy),
				Having     = old.Having is null ? null : Values.Of(old.Having),
				Window     = old.Windows is { Length: > 0 } windows ? new Ast.WindowClause(Each(windows, Window)) : null,
			};
		}

		static Ast.SelectIntoTable IntoTable(Clause old)
		{
			return old is Clause.Into(var table, var on)
				? new Ast.SelectIntoTable(Values.Name(old, table), on is null ? null : Values.Part(old, on))
				: throw BridgeGap.Open(old, "inventory Clause.Into");
		}

		static Ast.SetQuantifier? Quantifier(object owner, string? word)
		{
			return word?.ToUpperInvariant() switch
			{
				null       => null,
				"ALL"      => Ast.SetQuantifier.All,
				"DISTINCT" => Ast.SetQuantifier.Distinct,
				_          => throw BridgeGap.Open(owner, $"inventory Query.Specification (quantifier `{word}`)"),
			};
		}

		static Ast.WindowDefinition Window(Clause old)
		{
			return old is Clause.WindowDefinition(var name, Clause.Window specification)
				? new Ast.WindowDefinition(Values.Part(old, name), Values.Specification(specification))
				: throw BridgeGap.Open(old, "inventory Clause.WindowDefinition");
		}

		/// <summary>
		/// A row of <c>VALUES</c>: a row of several values in brackets, or one value, which keeps its own.
		/// </summary>
		public static Ast.RowValue Row(Expression row)
		{
			return row is Expression.RowValueConstructor(var items)
				? new Ast.RowValue(Each(items, Values.Of))
				: new Ast.RowValue([Values.Of(row)]);
		}

		// ── The select list ───────────────────────────────────────────────────────

		static Ast.SelectColumn Column(Clause old)
		{
			switch (old)
			{
				case Clause.DerivedColumn(var value, null):
					return new Ast.SelectColumn.ExpressionItem(Values.Of(value));

				case Clause.DerivedColumn(var value, var name):
					if (name![0] is '\'' or 'N' or 'n' && name.EndsWith('\''))
						throw BridgeGap.Open(old, "inventory Clause.DerivedColumn (a string alias)");

					// `x = a` and `a AS x` are one old node; where the value starts after the column, the
					// alias was written in front of it.
					if (Recover("Clause.DerivedColumn alias = expression", Spelled(old) is not null && Spelled(value) is not null ? "" : null, _ => value.Span.At > old.Span.At, false))
						throw BridgeGap.Open(old, "inventory Clause.DerivedColumn (alias = expression)");

					return new Ast.SelectColumn.ExpressionItem(Values.Of(value), Values.Part(old, name),
						Recover("SelectColumn.ExpressionItem.AsKeyword", WordAfter(value), static word => word == "AS", false));

				case Clause.QualifiedAsterisk(null):
					return new Ast.SelectColumn.All();

				case Clause.QualifiedAsterisk(var qualifier):
					return new Ast.SelectColumn.QualifiedAll(new Ast.Expression.Reference(Values.Name(old, qualifier!)));

				// T-SQL's `SELECT @v = e`, `SELECT @v += e`.
				case Clause.VariableAssignment(var variable, var op, var value):
					return new Ast.SelectColumn.VariableAssignment(new Ast.Identifier(variable), op switch
					{
						"="   => Ast.AssignmentOperator.Assign,
						"+="  => Ast.AssignmentOperator.Add,
						"-="  => Ast.AssignmentOperator.Subtract,
						"*="  => Ast.AssignmentOperator.Multiply,
						"/="  => Ast.AssignmentOperator.Divide,
						"%="  => Ast.AssignmentOperator.Modulo,
						"&="  => Ast.AssignmentOperator.BitwiseAnd,
						"|="  => Ast.AssignmentOperator.BitwiseOr,
						"^="  => Ast.AssignmentOperator.BitwiseXor,
						"||=" => Ast.AssignmentOperator.Concatenate,
						_     => throw BridgeGap.Open(old, $"inventory Clause.VariableAssignment (operator `{op}`)"),
					}, Values.Of(value));

				default:
					throw BridgeGap.Open(old, $"inventory {NameOf(old)} (in a select list)");
			}
		}

		static Ast.TopClause? Top(Clause? old)
		{
			if (old is null)
				return null;

			if (old is not Clause.Top(var value, var percent, var with))
				throw BridgeGap.Open(old, "inventory Clause.Top");

			if (with is not null && !with.Equals("TIES", StringComparison.OrdinalIgnoreCase))
				throw BridgeGap.Open(old, $"inventory Clause.Top (WITH {with})");

			return value is Expression.Parenthesized(var inner)
				? new Ast.TopClause(Values.Of(inner), true, percent, with is not null)
				: new Ast.TopClause(Values.Of(value), false, percent, with is not null);
		}

		public static Ast.TopClause? Topped(Clause? old)
		{
			return Top(old);
		}

		// ── ORDER BY, OFFSET, FETCH ───────────────────────────────────────────────

		public static Ast.OrderByClause Order(Clause.OrderBy old)
		{
			return new Ast.OrderByClause(Each(old.By, static one => one is Clause.SortSpecification sort
				? Sort(sort)
				: throw BridgeGap.Open(one, "inventory Clause.SortSpecification")));
		}

		static Ast.SortItem Sort(Clause.SortSpecification old)
		{
			Ast.SortDirection? direction = old.Order switch
			{
				SqlOrder.Ascending  => Ast.SortDirection.Asc,
				SqlOrder.Descending => Ast.SortDirection.Desc,
				_                   => null,
			};

			return new Ast.SortItem(Values.Of(old.Value), direction);
		}

		/// <summary>
		/// A query given its <c>ORDER BY</c>, and the <c>OFFSET</c> and <c>FETCH</c> the old tree keeps inside
		/// it, without the words around them: <c>ROW</c> or <c>ROWS</c>, <c>FIRST</c> or <c>NEXT</c> are read
		/// from the text, and are <c>ROWS</c> and <c>NEXT</c> where there is none.
		/// </summary>
		public static Ast.Query Ordered(Ast.Query query, Clause old)
		{
			if (old is not Clause.OrderBy by)
				throw BridgeGap.Open(old, "inventory Clause.OrderBy");

			if (query.OrderBy is not null || query.Limit is not null)
				throw BridgeGap.Open(old, "inventory Clause.OrderBy (a second order on one query)");

			Ast.Limit? limit = null;

			if (by.Offset is { } offset)
			{
				var after = by.Fetch is { } next ? Between(offset, next) : WordAfter(offset);

				limit = new Ast.Limit
				{
					Offset        = Values.Of(offset),
					OffsetRowWord = Recover("Limit.OffsetRowWord", after, static text => Words(text) is ["ROW", ..] ? Ast.RowWord.Row : Ast.RowWord.Rows, Ast.RowWord.Rows),
				};

				if (by.Fetch is { } fetch)
					limit = limit with
					{
						Count        = Values.Of(fetch),
						Position     = Recover("Limit.Position", after, static text => Words(text).Contains("FIRST") ? Ast.FetchPosition.First : Ast.FetchPosition.Next, Ast.FetchPosition.Next),
						FetchRowWord = Recover("Limit.FetchRowWord", WordAfter(fetch), static word => word == "ROW" ? Ast.RowWord.Row : Ast.RowWord.Rows, Ast.RowWord.Rows),
					};
			}
			else if (by.Fetch is not null)
				throw BridgeGap.Open(old, "inventory Clause.OrderBy (FETCH without OFFSET)");

			return query with { OrderBy = by.By.Length > 0 ? Order(by) : null, Limit = limit };
		}

		// ── GROUP BY ──────────────────────────────────────────────────────────────

		static Ast.GroupByClause Group(Clause old)
		{
			if (old is not Clause.GroupBy(var all, var by, var with))
				throw BridgeGap.Open(old, "inventory Clause.GroupBy");

			Ast.GroupByWith? suffix = with?.ToUpperInvariant() switch
			{
				null     => null,
				"CUBE"   => Ast.GroupByWith.Cube,
				"ROLLUP" => Ast.GroupByWith.Rollup,
				_        => throw BridgeGap.Open(old, $"inventory Clause.GroupBy (WITH {with})"),
			};

			return new Ast.GroupByClause(null, Each(by, Grouping)) { AllKeyword = all, With = suffix };
		}

		static Ast.GroupingElement Grouping(Expression old)
		{
			if (old is Expression.RoutineInvocation { Word: null } call)
			{
				var name = call.Name.ToUpperInvariant();

				if (name == "ROLLUP")
					return new Ast.GroupingElement.Rollup(Each(call.Arguments, Grouped));

				if (name == "CUBE")
					return new Ast.GroupingElement.Cube(Each(call.Arguments, Grouped));

				if (name == "GROUPING SETS")
					return new Ast.GroupingElement.Sets(Each(call.Arguments, Grouping));
			}

			return Grouped(old);
		}

		/// <summary>
		/// One element inside a <c>ROLLUP</c>, a <c>CUBE</c> or a plain list: a value, a row of them in brackets, or <c>()</c>.
		/// </summary>
		static Ast.GroupingElement Grouped(Expression old)
		{
			return old switch
			{
				Expression.RowValueConstructor { Values.Length: 0 } => new Ast.GroupingElement.Empty(),
				Expression.RowValueConstructor(var items)           => new Ast.GroupingElement.Ordinary(Each(items, Values.Of), true),
				_                                                   => new Ast.GroupingElement.Ordinary([Values.Of(old)]),
			};
		}
	}
}
