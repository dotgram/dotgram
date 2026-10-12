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

		// ── MATCH ─────────────────────────────────────────────────────────────────

		/// <summary>
		/// <c>MATCH (…)</c>, whose drawing the old tree keeps as text: read again token by token, an arrow being
		/// two characters with perhaps a blank between them, as the grammar reads it.
		/// </summary>
		public static Ast.Expression.GraphMatch GraphMatch(Expression old, string pattern)
		{
			var reader   = new GraphReader(old, [.. SqlTokens.Of(pattern)]);
			var patterns = new List<Ast.GraphPattern> { reader.Pattern() };

			while (reader.Word("AND"))
				patterns.Add(reader.Pattern());

			if (!reader.Done)
				throw reader.Gap("text after the last pattern");

			return new Ast.Expression.GraphMatch([.. patterns]);
		}

		/// <summary>A drawing's tokens, read from the left.</summary>
		sealed class GraphReader(Expression old, SqlTokens.Token[] tokens)
		{
			int _at;

			public bool Done => _at == tokens.Length;

			public Exception Gap(string what)
			{
				return BridgeGap.Open(old, $"inventory Expression.GraphMatch ({what})");
			}

			bool Is(string text, int ahead = 0)
			{
				return _at + ahead < tokens.Length && tokens[_at + ahead].Text.Equals(text, StringComparison.OrdinalIgnoreCase);
			}

			public bool Word(string word)
			{
				if (!Is(word) || tokens[_at].Kind != SqlTokens.Kind.Word)
					return false;

				_at++;
				return true;
			}

			void Expect(string text)
			{
				if (!Is(text))
					throw Gap($"`{text}` expected");

				_at++;
			}

			public Ast.GraphPattern Pattern()
			{
				if (Word("SHORTEST_PATH"))
					return ShortestPath();

				var start = Node();

				// LAST_NODE (a) = LAST_NODE (b): the last nodes compared.
				if (start.LastNode && Is("="))
				{
					_at++;
					var right = Node();

					return right.LastNode ? new Ast.GraphPattern.LastNodes(start.Alias, right.Alias) : throw Gap("a last node compared with a node");
				}

				var steps = new List<Ast.GraphStep>();

				do
					steps.Add(new Ast.GraphStep(Edge(), Node()));
				while (Is("-") || Is("<"));

				return new Ast.GraphPattern.Chain(start, [.. steps]);
			}

			Ast.GraphPattern ShortestPath()
			{
				Expect("(");

				Ast.GraphPattern.ShortestPath path;

				// The anchor first, each step edge then node; or the steps first, node then edge, and the anchor last.
				if (Is("("))
				{
					_at++;
					var steps = new List<Ast.GraphStep>();

					do
					{
						var node = Alias();
						steps.Add(new Ast.GraphStep(Edge(), node));
					}
					while (!Is(")"));

					_at++;
					var quantifier = Quantifier();
					path = new Ast.GraphPattern.ShortestPath(Node(), [.. steps], quantifier, true);
				}
				else
				{
					var anchor = Node();
					Expect("(");
					var steps = new List<Ast.GraphStep>();

					do
						steps.Add(new Ast.GraphStep(Edge(), Alias()));
					while (!Is(")"));

					_at++;
					path = new Ast.GraphPattern.ShortestPath(anchor, [.. steps], Quantifier(), false);
				}

				Expect(")");
				return path;
			}

			Ast.GraphNode Node()
			{
				if (Word("LAST_NODE"))
				{
					Expect("(");
					var alias = Name();
					Expect(")");

					return new Ast.GraphNode(alias, true);
				}

				return Alias();
			}

			Ast.GraphNode Alias()
			{
				return new Ast.GraphNode(Name(), false);
			}

			Ast.Identifier Name()
			{
				if (_at >= tokens.Length || tokens[_at].Kind is not (SqlTokens.Kind.Word or SqlTokens.Kind.Delimited))
					throw Gap("a node or an edge expected");

				return Values.Part(old, tokens[_at++].Text);
			}

			Ast.GraphEdge Edge()
			{
				if (Is("<"))
				{
					_at++;
					Expect("-");
					Expect("(");
					var alias = Name();
					Expect(")");
					Expect("-");

					return new Ast.GraphEdge(alias, Ast.GraphDirection.Left);
				}

				Expect("-");
				Expect("(");
				var name = Name();
				Expect(")");
				Expect("-");
				Expect(">");

				return new Ast.GraphEdge(name, Ast.GraphDirection.Right);
			}

			Ast.GraphQuantifier Quantifier()
			{
				if (Is("+"))
				{
					_at++;
					return new Ast.GraphQuantifier(1, null, true);
				}

				Expect("{");
				var min = Number();
				Expect(",");
				int? max = Is("}") ? null : Number();
				Expect("}");

				return new Ast.GraphQuantifier(min, max, false);
			}

			int Number()
			{
				if (_at >= tokens.Length || tokens[_at].Kind != SqlTokens.Kind.Number || !int.TryParse(tokens[_at].Text, out var number))
					throw Gap("a bound expected");

				_at++;
				return number;
			}
		}

		// ── FOR, OPTION ───────────────────────────────────────────────────────────

		/// <summary>
		/// <c>FOR XML</c>, <c>FOR JSON</c> or <c>FOR BROWSE</c>, which the old tree keeps as a kind and an option's
		/// text each. The first is the mode; the rest are the directives, each its words and perhaps a string in
		/// brackets (<c>ROOT ('r')</c>).
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

			// The mode, and RAW's or PATH's element name in brackets after it.
			var (mode, element) = SqlTokens.Of(options[0]) switch
			{
				[{ Kind: SqlTokens.Kind.Word } word] => (word.Text.ToUpperInvariant(), (Ast.Expression?)null),
				[{ Kind: SqlTokens.Kind.Word } word, { Text: "(" }, { Kind: SqlTokens.Kind.String } name, { Text: ")" }] => (word.Text.ToUpperInvariant(), Values.String(name.Text)),
				_ => throw BridgeGap.Open(old, $"inventory Clause.For (a mode `{options[0]}`)"),
			};

			var rest = Each(options[1..], one => Directive(old, one));

			if (upper == "JSON")
				return element is null && mode switch { "AUTO" => Ast.JsonMode.Auto, "PATH" => Ast.JsonMode.Path, _ => (Ast.JsonMode?)null } is { } json
					? new Ast.ForClause.Json(json, rest)
					: throw BridgeGap.Open(old, $"inventory Clause.For (FOR JSON {options[0]})");

			Ast.XmlMode xml = mode switch
			{
				"RAW"      => Ast.XmlMode.Raw,
				"AUTO"     => Ast.XmlMode.Auto,
				"EXPLICIT" => Ast.XmlMode.Explicit,
				"PATH"     => Ast.XmlMode.Path,
				_          => throw BridgeGap.Open(old, $"inventory Clause.For (FOR XML {options[0]})"),
			};

			return new Ast.ForClause.Xml(xml, element, rest);
		}

		static Ast.ForDirective Directive(Clause old, string text)
		{
			SqlTokens.Token[] tokens = [.. SqlTokens.Of(text)];

			// A string in brackets after the words: ROOT's or XMLSCHEMA's name.
			Ast.Expression? name = null;

			if (tokens is [.. var words, { Text: "(" }, { Kind: SqlTokens.Kind.String } value, { Text: ")" }])
			{
				name   = Values.String(value.Text);
				tokens = words;
			}

			var said = string.Join(" ", tokens.Select(static one => one.Kind == SqlTokens.Kind.Word ? one.Text.ToUpperInvariant() : "?"));

			return said switch
			{
				"BINARY BASE64"         => new Ast.ForDirective(Ast.ForDirectiveKind.BinaryBase64, name),
				"TYPE"                  => new Ast.ForDirective(Ast.ForDirectiveKind.Type, name),
				"ROOT"                  => new Ast.ForDirective(Ast.ForDirectiveKind.Root, name),
				"XMLDATA"               => new Ast.ForDirective(Ast.ForDirectiveKind.XmlData, name),
				"XMLSCHEMA"             => new Ast.ForDirective(Ast.ForDirectiveKind.XmlSchema, name),
				"ELEMENTS"              => new Ast.ForDirective(Ast.ForDirectiveKind.Elements, name),
				"ELEMENTS ABSENT"       => new Ast.ForDirective(Ast.ForDirectiveKind.Elements, name, Absent: true),
				"ELEMENTS XSINIL"       => new Ast.ForDirective(Ast.ForDirectiveKind.ElementsXsiNil, name),
				"INCLUDE_NULL_VALUES"   => new Ast.ForDirective(Ast.ForDirectiveKind.IncludeNullValues, name),
				"WITHOUT_ARRAY_WRAPPER" => new Ast.ForDirective(Ast.ForDirectiveKind.WithoutArrayWrapper, name),
				_                       => throw BridgeGap.Open(old, $"inventory Clause.For (a directive `{text}`)"),
			};
		}

		/// <summary>The query hints of several fixed words, by their words upper-cased.</summary>
		static readonly Dictionary<string, Ast.QueryHintPhrase> Phrases = new(StringComparer.Ordinal)
		{
			["HASH GROUP"]                = Ast.QueryHintPhrase.HashGroup,
			["ORDER GROUP"]               = Ast.QueryHintPhrase.OrderGroup,
			["CONCAT UNION"]              = Ast.QueryHintPhrase.ConcatUnion,
			["HASH UNION"]                = Ast.QueryHintPhrase.HashUnion,
			["MERGE UNION"]               = Ast.QueryHintPhrase.MergeUnion,
			["KEEP UNION"]                = Ast.QueryHintPhrase.KeepUnion,
			["LOOP JOIN"]                 = Ast.QueryHintPhrase.LoopJoin,
			["MERGE JOIN"]                = Ast.QueryHintPhrase.MergeJoin,
			["HASH JOIN"]                 = Ast.QueryHintPhrase.HashJoin,
			["EXPAND VIEWS"]              = Ast.QueryHintPhrase.ExpandViews,
			["FORCE ORDER"]               = Ast.QueryHintPhrase.ForceOrder,
			["FORCE SCALEOUTEXECUTION"]   = Ast.QueryHintPhrase.ForceScaleOutExecution,
			["DISABLE SCALEOUTEXECUTION"] = Ast.QueryHintPhrase.DisableScaleOutExecution,
			["KEEP PLAN"]                 = Ast.QueryHintPhrase.KeepPlan,
			["KEEPFIXED PLAN"]            = Ast.QueryHintPhrase.KeepFixedPlan,
			["ROBUST PLAN"]               = Ast.QueryHintPhrase.RobustPlan,
		};

		/// <summary>
		/// One hint of <c>OPTION (…)</c>, which the old tree keeps as text, read into the family by its shape: words
		/// fixed by the grammar, <c>USE HINT</c>, <c>USE PLAN</c>, <c>OPTIMIZE FOR</c>, <c>TABLE HINT</c>, and the rest
		/// a name with its value.
		/// </summary>
		static Ast.QueryHint QueryHint(Clause old)
		{
			if (old is not Clause.Hint(var text))
				throw BridgeGap.Open(old, "inventory Clause.Hint");

			var tokens = SqlTokens.Of(text);
			var words  = Words(text);

			if (tokens is [{ Kind: SqlTokens.Kind.Word }, { Kind: SqlTokens.Kind.Word }] && Phrases.TryGetValue(words[0] + " " + words[1], out var phrase))
				return new Ast.QueryHint.Phrase(phrase);

			switch (words)
			{
				case ["USE", "HINT", "(", .., ")"]:
					return new Ast.QueryHint.UseHint([.. Split(tokens.Skip(3).Take(tokens.Count - 4).ToList()).Select(item => item is [{ Kind: SqlTokens.Kind.String } one]
						? Values.String(one.Text)
						: throw Unread(old))]);

				case ["USE", "PLAN", _] when tokens[2].Kind == SqlTokens.Kind.String:
					return new Ast.QueryHint.UsePlan(Values.String(tokens[2].Text));

				case ["OPTIMIZE", "FOR", "UNKNOWN"]:
					return new Ast.QueryHint.OptimizeForUnknown();

				case ["OPTIMIZE", "FOR", "(", .., ")"]:
					return new Ast.QueryHint.OptimizeFor([.. Split(tokens.Skip(3).Take(tokens.Count - 4).ToList()).Select(item => item switch
					{
						[{ Kind: SqlTokens.Kind.Word } variable] when variable.Text.StartsWith('@') =>
							new Ast.OptimizeForVariable(new Ast.Identifier(variable.Text), null, false),
						[{ Kind: SqlTokens.Kind.Word } variable, { Kind: SqlTokens.Kind.Word } unknown] when variable.Text.StartsWith('@') && unknown.Text.Equals("UNKNOWN", StringComparison.OrdinalIgnoreCase) =>
							new Ast.OptimizeForVariable(new Ast.Identifier(variable.Text), null, true),
						[{ Kind: SqlTokens.Kind.Word } variable, { Text: "=" }, .. var value] when variable.Text.StartsWith('@') && value.Length > 0 =>
							new Ast.OptimizeForVariable(new Ast.Identifier(variable.Text), HintValue(old, value), false),
						_ => throw Unread(old),
					})]);

				case ["OPTIMIZE", "FOR", ..]:
					throw BridgeGap.Open(old, "inventory Clause.Hint (OPTIMIZE FOR a name, which the grammar reads and no record holds)");

				case ["TABLE", "HINT", "(", .., ")"]:
				{
					var items = Split(tokens.Skip(3).Take(tokens.Count - 4).ToList());

					if (items[0].Length == 0 || items[0].Any(static one => one.Kind is not (SqlTokens.Kind.Word or SqlTokens.Kind.Delimited) && one.Text != "."))
						throw Unread(old);

					return new Ast.QueryHint.ForTable(
						Values.Name(old, string.Concat(items[0].Select(static one => one.Text))),
						[.. items.Skip(1).Select(item => TableHintOf(old, item))]);
				}

				default:
					var (name, arguments, form) = NamedHint(old, [.. tokens]);

					return new Ast.QueryHint.Named(name, arguments) { Form = form };
			}
		}

		/// <summary>
		/// A table hint the old tree keeps as text: <c>FORCESEEK (ix (a, b))</c>, or a name with its values.
		/// </summary>
		public static Ast.TableHint TableHint(Clause old)
		{
			return old is Clause.Hint(var text)
				? TableHintOf(old, [.. SqlTokens.Of(text)])
				: throw BridgeGap.Open(old, "inventory Clause.Hint");
		}

		static Ast.TableHint TableHintOf(Clause old, SqlTokens.Token[] tokens)
		{
			if (tokens is [{ Kind: SqlTokens.Kind.Word } seek, { Text: "(" }, var index, { Text: "(" }, .., { Text: ")" }, { Text: ")" }]
				&& seek.Text.Equals("FORCESEEK", StringComparison.OrdinalIgnoreCase))
			{
				var columns = Split(tokens.Skip(4).Take(tokens.Length - 6).ToList());

				return new Ast.TableHint.ForceSeek(
					HintValue(old, [index]),
					[.. columns.Select(column => column is [{ Kind: SqlTokens.Kind.Word or SqlTokens.Kind.Delimited } one] ? Values.Part(old, one.Text) : throw Unread(old))]);
			}

			var (name, arguments, form) = NamedHint(old, tokens);

			return new Ast.TableHint.Named(name, arguments) { Form = form };
		}

		/// <summary>
		/// A hint of one word and its values: none, in brackets (<c>INDEX (ix, 2)</c>), after it (<c>MAXDOP 2</c>), or
		/// after <c>=</c> (<c>LABEL = 'x'</c>).
		/// </summary>
		static (Ast.Identifier Name, SqlList<Ast.Expression> Arguments, Ast.HintArgumentForm Form) NamedHint(Clause old, SqlTokens.Token[] tokens)
		{
			if (tokens is not [{ Kind: SqlTokens.Kind.Word } name, ..] || name.Text[0] is '@' or '#' or '$')
				throw Unread(old);

			var hint = new Ast.Identifier(name.Text);

			return tokens switch
			{
				[_]                                   => (hint, [], Ast.HintArgumentForm.Brackets),
				[_, { Text: "=" }, .. var value]      => (hint, [HintValue(old, value)], Ast.HintArgumentForm.Assigned),
				[_, { Text: "(" }, .., { Text: ")" }] => (hint, [.. Split(tokens.Skip(2).Take(tokens.Length - 3).ToList()).Select(item => HintValue(old, item))], Ast.HintArgumentForm.Brackets),
				[_, .. var value]                     => (hint, [HintValue(old, value)], Ast.HintArgumentForm.Bare),
				_                                     => throw Unread(old),
			};
		}

		/// <summary>A hint's value: one token, <c>NULL</c>, or a value the old grammar reads.</summary>
		static Ast.Expression HintValue(Clause old, IReadOnlyList<SqlTokens.Token> tokens)
		{
			if (tokens is [var one] && HintArgument(old, one) is { } value)
				return value;

			var text = string.Join(" ", tokens.Select(static one => one.Text));
			var read = DotGram.Sql.TransactSql.TransactSqlParser.TryParseSql("SELECT " + text);

			return read.IsSuccess && read.Value is [Statement.Select { Of: Query.Specification { Columns: [Clause.DerivedColumn(var expression, null)] } }]
				? Values.Of(expression)
				: throw Unread(old);
		}

		static BridgeGap Unread(Clause old)
		{
			return BridgeGap.Open(old, "inventory Clause.Hint (a hint of a shape the family does not hold)");
		}

		/// <summary>
		/// One token of a hint that is a value by itself. A number is not among them: it takes the parse, where the
		/// literal is classified as any other is — <c>0xFF</c> binary, <c>1e2</c> approximate.
		/// </summary>
		static Ast.Expression? HintArgument(Clause old, SqlTokens.Token token)
		{
			return token.Kind switch
			{
				SqlTokens.Kind.String                                => Values.String(token.Text),
				SqlTokens.Kind.Word when token.Text.StartsWith('@')  => new Ast.Expression.Variable(new Ast.Identifier(token.Text)),
				SqlTokens.Kind.Word when token.Text.Equals("NULL", StringComparison.OrdinalIgnoreCase) => new Ast.Expression.Literal(new Ast.LiteralValue.Null()),
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
				{
					// A string alias, 'x' or N'x': its content, the doubled quotes undone.
					var alias = name![0] is '\'' or 'N' or 'n' && name.Length > 1 && name.EndsWith('\'') && name.IndexOf('\'') is 0 or 1
						? new Ast.Identifier(name[(name.IndexOf('\'') + 1)..^1].Replace("''", "'"), name[0] == '\'' ? Ast.IdentifierStyle.String : Ast.IdentifierStyle.NationalString)
						: Values.Part(old, name);

					// `x = a` and `a AS x` are one old node; where the value starts after the column, the
					// alias was written in front of it.
					var first = Recover("Clause.DerivedColumn alias = expression", Spelled(old) is not null && Spelled(value) is not null ? "" : null, _ => value.Span.At > old.Span.At, false);

					return new Ast.SelectColumn.ExpressionItem(Values.Of(value), alias,
						!first && Recover("SelectColumn.ExpressionItem.AsKeyword", WordAfter(value), static word => word == "AS", false))
					{
						AliasFirst = first,
					};
				}

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

			Ast.TopWith? said = with?.ToUpperInvariant() switch
			{
				null          => null,
				"TIES"        => Ast.TopWith.Ties,
				"APPROXIMATE" => Ast.TopWith.Approximate,
				_             => throw BridgeGap.Open(old, $"inventory Clause.Top (WITH {with})"),
			};

			return value is Expression.Parenthesized(var inner)
				? new Ast.TopClause(Values.Of(inner), true, percent, said)
				: new Ast.TopClause(Values.Of(value), false, percent, said);
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
