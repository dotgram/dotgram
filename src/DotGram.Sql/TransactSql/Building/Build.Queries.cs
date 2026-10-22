using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

static partial class Build
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
			"Clause.SortSpecification", "Clause.OrderBy",
			"Clause.Top", "Clause.GroupBy", "Clause.Into", "Clause.For", "Clause.Hint",
			"Clause.CommonTableExpression",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				Clause.CommonTableExpression common                => Common(common),
				Clause.SortSpecification sort                      => Sort(sort),
				Clause when Owned(Owns, old)                       => throw BuildGap.Open(old, $"inventory {NameOf(old)} (stands only inside a query)"),
				_                                                  => null,
			};

			return made is not null;
		}

		// ── The query rules' builders ─────────────────────────────────────────────
		//
		// Each query rule hands what it read to one of these, which builds the query of the common tree at once, its
		// parts that are still records of their own built as their builders build them: a query it holds was built by
		// its own rule, and is carried as what it holds. The query stands where the T-SQL tree holds one as a
		// Query.Built, with what the rules that hold a query ask of it.

		/// <summary>SELECT and the clauses under it, on the common tree.</summary>
		internal static Query Specification(
			string? quantifier, Clause? top, SqlList<Ast.SelectColumn> columns, Clause? into, SqlList<TableReference> from, Ast.Expression? where,
			Clause? group, Ast.Expression? having, SqlList<Clause>? windows)
		{
			var assigns = columns.Exists(static one => Assigns(one));
			var facts   = (assigns ? Query.Built.Facts.Assigns : Query.Built.Facts.None)
				| (assigns && from.IsEmpty ? Query.Built.Facts.AssignsFromNothing : Query.Built.Facts.None)
				| (into is null ? Query.Built.Facts.Cursorable : Query.Built.Facts.None);

			Building((quantifier, top, columns, into, from, where, group, having, windows), static parts => (Ast.Query)new Ast.Query.Select
			{
				Top        = Top(parts.top),
				IntoTable  = parts.into is null ? null : IntoTable(parts.into),
				Quantifier = Quantifier(SpecificationName, parts.quantifier),
				Columns    = Columns(parts.columns),
				From       = parts.from.Length > 0 ? new Ast.FromClause(Each(parts.from, Source)) : null,
				Where      = parts.where is null ? null : Values.Of(parts.where),
				GroupBy    = parts.group is null ? null : Group(parts.group),
				Having     = parts.having is null ? null : Values.Of(parts.having),
				Window     = parts.windows is { Length: > 0 } list ? new Ast.WindowClause(Each(list, Window)) : null,
			}, out var made, out var gap);

			return new Query.Built(made, gap, SpecificationName.Record, facts) { Columns = columns, Where = where };
		}

		/// <summary>VALUES (…), (…): the rows, on the common tree; the query of them is made when it is asked for.</summary>
		internal static Query Constructed(SqlList<Ast.Expression> rows)
		{
			Building(rows, static rows => Each(rows, Row), out SqlList<Ast.RowValue> made, out var gap);

			return new Query.Built(null, gap, "Query.TableValueConstructor", Query.Built.Facts.None)
			{
				Rows   = rows,
				Values = gap is null ? made : null,
			};
		}

		/// <summary>TABLE t, which is every column and every row of one table.</summary>
		internal static Query Table(string name)
		{
			Building(name, static name => (Ast.Query)new Ast.Query.Select
			{
				Columns = [new Ast.SelectColumn.All()],
				From    = new Ast.FromClause([new Ast.TableSource.Named(Values.Name(TableName, name))]),
				Table   = true,
			}, out var made, out var gap);

			return new Query.Built(made, gap, TableName.Record, Query.Built.Facts.Cursorable);
		}

		/// <summary>A query in brackets.</summary>
		internal static Query Parenthesized(Query query)
		{
			Building(query, static query => (Ast.Query)new Ast.Query.Parenthesized(QueryOf(query)), out var made, out var gap);

			// The brackets hide nothing a rule asks about the query in them.
			var facts = (Rules.Assigns(query) ? Query.Built.Facts.Assigns : Query.Built.Facts.None)
				| (Rules.Cursorable(query) ? Query.Built.Facts.Cursorable : Query.Built.Facts.None)
				| (Rules.AssignsCombined(query) ? Query.Built.Facts.AssignsCombined : Query.Built.Facts.None);

			return new Query.Built(made, gap, "Query.Parenthesized", facts);
		}

		/// <summary>
		/// A query given an order, an OFFSET … FETCH or a FOR of its own, where no statement holds them: in brackets, a named
		/// query, an INSERT's or a cursor's. The query itself where none was written.
		/// </summary>
		internal static Query Ordered(Query query, SqlList<Clause>? by, Clause.OrderBy? window, Clause? shape)
		{
			var order = Clause.Ordered(by, window);

			if (order is null && shape is null)
				return query;

			Building((query, order, shape), static parts =>
			{
				var ordered = parts.order is null ? QueryOf(parts.query) : Ordered(QueryOf(parts.query), parts.order);

				return parts.shape is null ? ordered : ordered with { For = For(parts.shape) };
			}, out var made, out var gap);

			return new Query.Built(made, gap, "Query.Ordered", Rules.Cursorable(query) ? Query.Built.Facts.Cursorable : Query.Built.Facts.None);
		}

		/// <summary>
		/// <c>UNION</c>, <c>EXCEPT</c> or <c>INTERSECT</c>, by the operator's word, and whether <c>ALL</c> was written: the
		/// new tree's left fold, one more operation on the left side's list with the right side as its operand. The right
		/// side keeps its own list, which is what binds tighter there — <c>A UNION B INTERSECT C</c> is
		/// <c>A [UNION B [INTERSECT C]]</c>.
		/// </summary>
		internal static Query Combined(string operatorText, string? all, Query left, Query right)
		{
			var (record, op) = (operatorText[0] | 0x20) switch
			{
				'u' => ("Query.Union", all is null ? Ast.SetOperator.Union : Ast.SetOperator.UnionAll),
				'e' => ("Query.Except", all is null ? Ast.SetOperator.Except : Ast.SetOperator.ExceptAll),
				_   => ("Query.Intersect", all is null ? Ast.SetOperator.Intersect : Ast.SetOperator.IntersectAll),
			};

			Building((left, op, right), static parts =>
			{
				var first = QueryOf(parts.left);

				if (first.OrderBy is not null || first.Limit is not null || first.For is not null)
					throw BuildGap.Open(parts.left, "inventory Query.Union (a left operand with an order of its own and no brackets)");

				return first with { SetOperations = [.. first.SetOperations, new Ast.SetOperation(parts.op, QueryOf(parts.right))] };
			}, out var made, out var gap);

			var facts = (Rules.Cursorable(left) && Rules.Cursorable(right) ? Query.Built.Facts.Cursorable : Query.Built.Facts.None)
				| (Rules.Assigns(left) || Rules.Assigns(right) || Rules.AssignsCombined(left) || Rules.AssignsCombined(right)
					? Query.Built.Facts.AssignsCombined
					: Query.Built.Facts.None);

			return new Query.Built(made, gap, record, facts);
		}

		/// <summary>What the builders of a specification and of TABLE t say a gap of theirs under: the record they replace.</summary>
		static readonly Naming SpecificationName = new("Query.Specification");

		static readonly Naming TableName = new("Query.ExplicitTable");

		/// <summary>A name to say a gap under, where a builder has no record of the T-SQL tree to say it of.</summary>
		sealed record Naming(string Record) : Given(Record);

		/// <summary>
		/// What the rule of a query statement hands its builder: the query, its order and its OFFSET … FETCH, its FOR and
		/// its hints, and the WITH in front of it, which the rule that reads a statement with its WITH adds.
		/// </summary>
		internal sealed record Selecting(Query Query, Clause? By, Clause? Shape, SqlList<Clause> Options) : Given("Statement.Select")
		{
			/// <summary>The common table expressions and namespaces in front of the statement.</summary>
			public SqlList<Clause> With { get; init; }
		}

		/// <summary>
		/// A query statement, built where it is carried: its query is built already, and the WITH in front of it is read
		/// by the rule around it (<see cref="Dml.Preceded"/>), which hands it over.
		/// </summary>
		internal static Statement Select(SqlList<Clause> with, Query query, Clause? by, Clause? shape, SqlList<Clause> options)
		{
			return new Statement.Built(null, null, new Selecting(query, by, shape, options) { With = with }) { Pending = BuildSelect };
		}

		/// <summary>A query statement held, given the WITH in front of it.</summary>
		internal static Statement Select(Statement.Built held, Selecting selecting, SqlList<Clause> with)
		{
			return new Statement.Built(null, null, selecting with { With = with }) { Pending = held.Pending };
		}

		static readonly Func<object, Ast.Statement> BuildSelect = static one => Statemented((Selecting)one);

		/// <summary>A query statement on the common tree: its query, order, FOR, WITH and hints, in that order.</summary>
		internal static Ast.Statement Statemented(Selecting old)
		{
			var query = QueryOf(old.Query);

			if (old.By is { } by)
				query = Ordered(query, by);

			if (old.Shape is { } shape)
				query = query with { For = For(shape) };

			if (old.With.Length > 0)
				query = query with { With = With(old.With) };

			return new Ast.Statement.Select { Query = query, Hints = Each(old.Options, QueryHint) };
		}

		/// <summary>
		/// The query statement a text a builder writes to read a part again reads as, as its rule handed it over; null
		/// where the text does not read as one query statement.
		/// </summary>
		internal static Selecting? Selected(string text)
		{
			return Reread(text) is { IsSuccess: true, Value: [Statement.Built { Given: Selecting selecting }] } ? selecting : null;
		}

		/// <summary>The same, where it reads as one specification: the specification, as its rule read it.</summary>
		internal static Query.Built? Specified(string text)
		{
			return Selected(text) is { Query: Query.Built { Record: "Query.Specification" } specified } ? specified : null;
		}

		/// <summary>
		/// A WITH: its common table expressions, and T-SQL's <c>XMLNAMESPACES (…)</c> or <c>CHANGE_TRACKING_CONTEXT (…)</c>,
		/// which the old tree keeps as an option whose name is the text and which stands first.
		/// </summary>
		public static Ast.WithClause With(SqlList<Clause> with)
		{
			var namespaces = with is [Clause.Option { Name: var text } first, ..] && Words(text) is ["XMLNAMESPACES", ..]
				? XmlNamespaces(first, text)
				: [];
			var context    = with is [Clause.Option { Name: var said } leading, ..] && Words(said) is ["CHANGE_TRACKING_CONTEXT", "(", ..]
				? Context(leading, said)
				: null;

			return new Ast.WithClause(false, Each(namespaces.Count > 0 || context is not null ? with[1..] : with, static one => one is Clause.CommonTableExpression common
				? Common(common)
				: throw BuildGap.Open(one, $"inventory {NameOf(one)} (in a WITH)")))
			{
				XmlNamespaces         = namespaces,
				ChangeTrackingContext = context,
			};
		}

		/// <summary><c>CHANGE_TRACKING_CONTEXT (value)</c>, read from the old option's text: the value read again as a value.</summary>
		static Ast.Expression Context(Clause old, string text)
		{
			var open  = SqlTokens.Find(text, "(");
			var close = SqlTokens.FindLast(text, ")");
			var read  = close > open ? Specified("SELECT " + text.Substring(open + 1, close - open - 1)) : null;

			return read is { Columns: [Ast.SelectColumn.ExpressionItem { Expression: var value, Alias: null }] }
				? Values.Of(value)
				: throw BuildGap.Open(old, $"inventory Clause.Option (a CHANGE_TRACKING_CONTEXT `{text}` that does not read)");
		}

		/// <summary>
		/// <c>XMLNAMESPACES ('uri' AS ns, DEFAULT 'uri')</c>, read from the old option's text.
		/// </summary>
		internal static SqlList<Ast.XmlNamespace> XmlNamespaces(object old, string text)
		{
			var tokens = SqlTokens.Of(text);

			if (tokens is not [_, { Text: "(" }, .., { Text: ")" }])
				throw BuildGap.Open(old, "inventory Clause.Option (an XMLNAMESPACES that does not read)");

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

					_ => throw BuildGap.Open(old, "inventory Clause.Option (an XMLNAMESPACES item that does not read)"),
				});
			}

			return [.. made];
		}

		/// <summary>
		/// Tokens cut at the commas that stand outside brackets.
		/// </summary>
		public static List<List<SqlTokens.Token>> Split(IReadOnlyList<SqlTokens.Token> tokens)
		{
			var items = new List<List<SqlTokens.Token>>();
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
		public static Ast.Expression.GraphMatch GraphMatch(object old, string pattern)
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
		sealed class GraphReader(object old, SqlTokens.Token[] tokens)
		{
			int _at;

			public bool Done => _at == tokens.Length;

			public Exception Gap(string what)
			{
				return BuildGap.Open(old, $"inventory Expression.GraphMatch ({what})");
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
				throw BuildGap.Open(old, "inventory Clause.For");

			var upper = kind.ToUpperInvariant();

			if (upper == "BROWSE")
				return options.Length == 0 ? new Ast.ForClause.Browse() : throw BuildGap.Open(old, "inventory Clause.For (BROWSE with options)");

			if (upper is not ("XML" or "JSON") || options.Length == 0)
				throw BuildGap.Open(old, $"inventory Clause.For (FOR {kind})");

			// The mode, and RAW's or PATH's element name in brackets after it.
			var (mode, element) = SqlTokens.Of(options[0]) switch
			{
				[{ Kind: SqlTokens.Kind.Word } word] => (word.Text.ToUpperInvariant(), (Ast.Expression?)null),
				[{ Kind: SqlTokens.Kind.Word } word, { Text: "(" }, { Kind: SqlTokens.Kind.String } name, { Text: ")" }] => (word.Text.ToUpperInvariant(), Values.String(name.Text)),
				_ => throw BuildGap.Open(old, $"inventory Clause.For (a mode `{options[0]}`)"),
			};

			var rest = Each(options[1..], one => Directive(old, one));

			if (upper == "JSON")
				return element is null && mode switch { "AUTO" => Ast.JsonMode.Auto, "PATH" => Ast.JsonMode.Path, _ => (Ast.JsonMode?)null } is { } json
					? new Ast.ForClause.Json(json, rest)
					: throw BuildGap.Open(old, $"inventory Clause.For (FOR JSON {options[0]})");

			Ast.XmlMode xml = mode switch
			{
				"RAW"      => Ast.XmlMode.Raw,
				"AUTO"     => Ast.XmlMode.Auto,
				"EXPLICIT" => Ast.XmlMode.Explicit,
				"PATH"     => Ast.XmlMode.Path,
				_          => throw BuildGap.Open(old, $"inventory Clause.For (FOR XML {options[0]})"),
			};

			return new Ast.ForClause.Xml(xml, element, rest);
		}

		static Ast.ForDirective Directive(Clause old, string text)
		{
			List<SqlTokens.Token> tokens = [.. SqlTokens.Of(text)];

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
				_                       => throw BuildGap.Open(old, $"inventory Clause.For (a directive `{text}`)"),
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
		internal static Ast.QueryHint QueryHint(Clause old)
		{
			if (old is not Clause.Hint(var text))
				throw BuildGap.Open(old, "inventory Clause.Hint");

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
						[{ Kind: SqlTokens.Kind.Word } variable, { Text: "=" }, .. var value] when variable.Text.StartsWith('@') && value.Count > 0 =>
							new Ast.OptimizeForVariable(new Ast.Identifier(variable.Text), HintValue(old, value), false),
						_ => throw Unread(old),
					})]);

				case ["OPTIMIZE", "FOR", ..]:
					throw BuildGap.Open(old, "inventory Clause.Hint (OPTIMIZE FOR a name, which the grammar reads and no record holds)");

				case ["TABLE", "HINT", "(", .., ")"]:
				{
					var items = Split(tokens.Skip(3).Take(tokens.Count - 4).ToList());

					if (items[0].Count == 0 || items[0].Any(static one => one.Kind is not (SqlTokens.Kind.Word or SqlTokens.Kind.Delimited) && one.Text != "."))
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
				: throw BuildGap.Open(old, "inventory Clause.Hint");
		}

		static Ast.TableHint TableHintOf(Clause old, List<SqlTokens.Token> tokens)
		{
			if (tokens is [{ Kind: SqlTokens.Kind.Word } seek, { Text: "(" }, var index, { Text: "(" }, .., { Text: ")" }, { Text: ")" }]
				&& seek.Text.Equals("FORCESEEK", StringComparison.OrdinalIgnoreCase))
			{
				var columns = Split(tokens.Skip(4).Take(tokens.Count - 6).ToList());

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
		static (Ast.Identifier Name, SqlList<Ast.Expression> Arguments, Ast.HintArgumentForm Form) NamedHint(Clause old, List<SqlTokens.Token> tokens)
		{
			if (tokens is not [{ Kind: SqlTokens.Kind.Word } name, ..] || name.Text[0] is '@' or '#' or '$')
				throw Unread(old);

			var hint = new Ast.Identifier(name.Text);

			return tokens switch
			{
				[_]                                   => (hint, [], Ast.HintArgumentForm.Brackets),
				[_, { Text: "=" }, .. var value]      => (hint, [HintValue(old, value)], Ast.HintArgumentForm.Assigned),
				[_, { Text: "(" }, .., { Text: ")" }] => (hint, [.. Split(tokens.Skip(2).Take(tokens.Count - 3).ToList()).Select(item => HintValue(old, item))], Ast.HintArgumentForm.Brackets),
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

			return Specified("SELECT " + text) is { Columns: [Ast.SelectColumn.ExpressionItem { Expression: var expression, Alias: null }] }
				? Values.Of(expression)
				: throw Unread(old);
		}

		static BuildGap Unread(Clause old)
		{
			return BuildGap.Open(old, "inventory Clause.Hint (a hint of a shape the family does not hold)");
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
				SqlTokens.Kind.Word when token.Text.Equals("NULL", StringComparison.OrdinalIgnoreCase) => new Ast.LiteralValue.Null(),
				SqlTokens.Kind.Word or SqlTokens.Kind.Delimited      => new Ast.Expression.Reference(new Ast.QualifiedName([Values.Part(old, token.Text)])),
				_                                                    => null,
			};
		}

		static Ast.CommonTableExpression Common(Clause.CommonTableExpression old)
		{
			return new Ast.CommonTableExpression(Values.Part(old, old.Name), Values.Parts(old, old.Columns), QueryOf(old.Query));
		}

		static Ast.SelectIntoTable IntoTable(Clause old)
		{
			return old is Clause.Into(var table, var on)
				? new Ast.SelectIntoTable(Values.Name(old, table), on is null ? null : Values.Part(old, on))
				: throw BuildGap.Open(old, "inventory Clause.Into");
		}

		static Ast.SetQuantifier? Quantifier(object owner, string? word)
		{
			return word?.ToUpperInvariant() switch
			{
				null       => null,
				"ALL"      => Ast.SetQuantifier.All,
				"DISTINCT" => Ast.SetQuantifier.Distinct,
				_          => throw BuildGap.Open(owner, $"inventory Query.Specification (quantifier `{word}`)"),
			};
		}

		static Ast.WindowDefinition Window(Clause old)
		{
			return old is Clause.WindowDefinition(var name, Clause.Window specification)
				? new Ast.WindowDefinition(Values.Part(old, name), Values.Specification(specification))
				: throw BuildGap.Open(old, "inventory Clause.WindowDefinition");
		}

		/// <summary>
		/// A row of <c>VALUES</c>: a row of several values in brackets, or one value, which keeps its own.
		/// </summary>
		public static Ast.RowValue Row(Ast.Expression row)
		{
			return row is Ast.Expression.Row(var items, _)
				? new Ast.RowValue(Each(items, Values.Of))
				: new Ast.RowValue([Values.Of(row)]);
		}

		// ── The select list ───────────────────────────────────────────────────────

		// Each item is built as its rule reads it, on the common tree, with its value as read; building the query resolves the
		// values (Columns), which is where a value with no place on the tree says its gap, as it would anywhere else. An item
		// that has no place itself is held with its gap (Unplaced) until then, since a rule's action does not throw.

		/// <summary>A value, perhaps given a name: written after it, with or without <c>AS</c>, or in front of it with <c>=</c>.</summary>
		internal static Ast.SelectColumn Item(Ast.Expression value, string? name, bool front)
		{
			if (name is null)
				return new Ast.SelectColumn.ExpressionItem(value);

			Ast.Identifier alias;

			try
			{
				// A string alias, 'x' or N'x': its content, the doubled quotes undone.
				alias = name[0] is '\'' or 'N' or 'n' && name.Length > 1 && name.EndsWith('\'') && name.IndexOf('\'') is 0 or 1
					? new Ast.Identifier(name[(name.IndexOf('\'') + 1)..^1].Replace("''", "'"), name[0] == '\'' ? Ast.IdentifierStyle.String : Ast.IdentifierStyle.NationalString)
					: Values.Part(DerivedColumn, name);
			}
			catch (BuildGap gap)
			{
				return new Unplaced(gap, value, false);
			}

			// The alias is said to stand in front where the text it was read from says so, and AS to be written where it
			// follows the value; a reading with no text to look at says neither.
			var first = front && Cut(value);

			return new Ast.SelectColumn.ExpressionItem(value, alias, !first && Recover(WordAfter(value), static word => word == "AS", false))
			{
				AliasFirst = first,
			};
		}

		/// <summary><c>*</c>, or <c>t.*</c>.</summary>
		internal static Ast.SelectColumn All(string? qualifier)
		{
			if (qualifier is null)
				return new Ast.SelectColumn.All();

			try
			{
				return new Ast.SelectColumn.QualifiedAll(new Ast.Expression.Reference(Values.Name(QualifiedAsterisk, qualifier)));
			}
			catch (BuildGap gap)
			{
				return new Unplaced(gap, null, false);
			}
		}

		/// <summary>T-SQL's <c>SELECT @v = e</c>, <c>SELECT @v += e</c>.</summary>
		internal static Ast.SelectColumn Assigned(string variable, string op, Ast.Expression value)
		{
			Ast.AssignmentOperator? by = op switch
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
				_     => null,
			};

			return by is { } known
				? new Ast.SelectColumn.VariableAssignment(new Ast.Identifier(variable), known, value)
				: new Unplaced(BuildGap.Open(VariableAssignment, $"inventory Clause.VariableAssignment (operator `{op}`)"), value, true);
		}

		/// <summary>Whether an item takes a value into a variable rather than returning it.</summary>
		internal static bool Assigns(Ast.SelectColumn item)
		{
			return item is Ast.SelectColumn.VariableAssignment or Unplaced { Assigns: true };
		}

		/// <summary>
		/// A select list as its items were read, with every value resolved: the list itself where none needed it, and the
		/// gap of the first value or item with no place on the common tree.
		/// </summary>
		internal static SqlList<Ast.SelectColumn> Columns(SqlList<Ast.SelectColumn> read)
		{
			Ast.SelectColumn[]? made = null;

			for (var at = 0; at < read.Count; at++)
			{
				var one      = read[at];
				var resolved = one switch
				{
					Unplaced unplaced                                                       => throw unplaced.Gap,
					Ast.SelectColumn.ExpressionItem { Expression: Values.Held held } item      => item with { Expression = Values.Of(held) },
					Ast.SelectColumn.VariableAssignment { Value: Values.Held held } assignment => assignment with { Value = Values.Of(held) },
					_                                                                       => one,
				};

				if (!ReferenceEquals(resolved, one))
				{
					made ??= read.ToArray();
					made[at] = resolved;
				}
			}

			return made is null ? read : SqlList.Own(made);
		}

		/// <summary>The records a select list's items were read as, under which their gaps are said.</summary>
		const string DerivedColumn = "Clause.DerivedColumn", QualifiedAsterisk = "Clause.QualifiedAsterisk", VariableAssignment = "Clause.VariableAssignment";

		/// <summary>An item of a select list with no place on the common tree, and why: what building its query throws.</summary>
		/// <param name="Gap">What the common tree has no place for.</param>
		/// <param name="Value">The value it was read over, which a rule's condition asks of (<see cref="Rules.HasIdentity"/>).</param>
		/// <param name="Assigns">Whether it takes a value into a variable.</param>
		internal sealed record Unplaced(BuildGap Gap, Ast.Expression? Value, bool Assigns) : Ast.SelectColumn;

		static Ast.TopClause? Top(Clause? old)
		{
			if (old is null)
				return null;

			if (old is not Clause.Top(var value, var percent, var with))
				throw BuildGap.Open(old, "inventory Clause.Top");

			Ast.TopWith? said = with?.ToUpperInvariant() switch
			{
				null          => null,
				"TIES"        => Ast.TopWith.Ties,
				"APPROXIMATE" => Ast.TopWith.Approximate,
				_             => throw BuildGap.Open(old, $"inventory Clause.Top (WITH {with})"),
			};

			return Values.Bracketed(value) is { } inner
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
				: throw BuildGap.Open(one, "inventory Clause.SortSpecification")));
		}

		internal static Ast.SortItem Sort(Clause.SortSpecification old)
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
				throw BuildGap.Open(old, "inventory Clause.OrderBy");

			if (query.OrderBy is not null || query.Limit is not null)
				throw BuildGap.Open(old, "inventory Clause.OrderBy (a second order on one query)");

			Ast.Limit? limit = null;

			if (by.Offset is { } offset)
			{
				var after = by.Fetch is { } next ? Between(offset, next) : WordAfter(offset);

				limit = new Ast.Limit
				{
					Offset        = Values.Of(offset),
					OffsetRowWord = Recover(after, static text => Words(text) is ["ROW", ..] ? Ast.RowWord.Row : Ast.RowWord.Rows, Ast.RowWord.Rows),
				};

				if (by.Fetch is { } fetch)
					limit = limit with
					{
						Count        = Values.Of(fetch),
						Position     = Recover(after, static text => Words(text).Contains("FIRST") ? Ast.FetchPosition.First : Ast.FetchPosition.Next, Ast.FetchPosition.Next),
						FetchRowWord = Recover(WordAfter(fetch), static word => word == "ROW" ? Ast.RowWord.Row : Ast.RowWord.Rows, Ast.RowWord.Rows),
					};
			}
			else if (by.Fetch is not null)
				throw BuildGap.Open(old, "inventory Clause.OrderBy (FETCH without OFFSET)");

			return query with { OrderBy = by.By.Length > 0 ? Order(by) : null, Limit = limit };
		}

		// ── GROUP BY ──────────────────────────────────────────────────────────────

		static Ast.GroupByClause Group(Clause old)
		{
			if (old is not Clause.GroupBy(var all, var by, var with))
				throw BuildGap.Open(old, "inventory Clause.GroupBy");

			Ast.GroupByWith? suffix = with?.ToUpperInvariant() switch
			{
				null     => null,
				"CUBE"   => Ast.GroupByWith.Cube,
				"ROLLUP" => Ast.GroupByWith.Rollup,
				_        => throw BuildGap.Open(old, $"inventory Clause.GroupBy (WITH {with})"),
			};

			return new Ast.GroupByClause(null, Each(by, Grouping)) { AllKeyword = all, With = suffix };
		}

		static Ast.GroupingElement Grouping(Ast.Expression old)
		{
			if (old is Values.Call { Word: null } call)
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
		static Ast.GroupingElement Grouped(Ast.Expression old)
		{
			return old switch
			{
				Ast.Expression.Row { Items.Length: 0 } => new Ast.GroupingElement.Empty(),
				Ast.Expression.Row(var items, _)           => new Ast.GroupingElement.Ordinary(Each(items, Values.Of), true),

				// TSqlGroupingColumn's WITH (DISTRIBUTED_AGG), which the old tree keeps as words after the column.
				Values.Hinted(var column, var words) when Words(words) is ["WITH", "(", "DISTRIBUTED_AGG", ")"]
					=> new Ast.GroupingElement.Ordinary([Values.Of(column)]) { DistributedAggregation = true },

				_                                                   => new Ast.GroupingElement.Ordinary([Values.Of(old)]),
			};
		}
	}
}
