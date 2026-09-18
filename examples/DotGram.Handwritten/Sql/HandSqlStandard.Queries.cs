using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Handwritten;

using Typed = SqlTowers.Typed;

// ISO/IEC 9075-2:2023 §7, the query expressions: the select list and the table expression under it,
// the table references and their joins, the row pattern recognition clause, and the set operators
// that join one query to another.
partial class HandSqlStandard
{
	// ── The publications ───────────────────────────────────────────────────────

	/// <summary>Reads the whole input as a <c>&lt;query expression&gt;</c>.</summary>
	public static Statement.Select ParseQueryExpression(string input) =>
		TryParseQueryExpression(input, out var value) ? value : throw Refused(input, "query expression");

	/// <summary>Reads the whole input as a <c>&lt;query expression&gt;</c>, answering rather than throwing.</summary>
	public static bool TryParseQueryExpression(string input, out Statement.Select value)
	{
		var cursor = new SqlCursor(input);

		if (QueryExpression(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	public static Statement.Select ParseQuerySpecification(string input) =>
		TryParseQuerySpecification(input, out var value) ? value : throw Refused(input, "query specification");

	public static bool TryParseQuerySpecification(string input, out Statement.Select value)
	{
		var cursor = new SqlCursor(input);

		if (QuerySpecification(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	public static TableSource ParseTableReference(string input) =>
		TryParseTableReference(input, out var value) ? value : throw Refused(input, "table reference");

	public static bool TryParseTableReference(string input, out TableSource value)
	{
		var cursor = new SqlCursor(input);

		if (TableReference(ref cursor, out value) && cursor.AtEnd)
			return true;

		value = null!;

		return false;
	}

	// ── §7.13 Query expression ─────────────────────────────────────────────────

	/// <summary>
	/// <c>&lt;query expression&gt;</c>. The clauses around a body are the body's own where it has
	/// none of them, and a query of their own around it where it has.
	/// </summary>
	static bool QueryExpression(ref SqlCursor cursor, out Statement.Select query)
	{
		var save = cursor;
		var with = WithClause(ref cursor);

		if (!QueryExpressionBody(ref cursor, out var body))
		{
			cursor = save;
			query  = null!;

			return false;
		}

		var order  = OrderByClause(ref cursor);
		var offset = ResultOffsetClause(ref cursor);
		var fetch  = FetchFirstClause(ref cursor);

		query = Expressed(body, with, order, offset, fetch);

		return true;
	}

	static Statement.Select Expressed(Statement.Select body, WithClause? with, OrderByClause? order, OffsetClause? offset, FetchClause? fetch)
	{
		if (with is null && order is null && offset is null && fetch is null)
			return body;

		if (body.Parentheses == 0 && body.With is null && body.OrderBy is null && body.Offset is null && body.Fetch is null)
			return body with { With = with, OrderBy = order, Offset = offset, Fetch = fetch };

		return new Statement.Select { Body = new QueryOperand.Select(body), With = with, OrderBy = order, Offset = offset, Fetch = fetch };
	}

	static WithClause? WithClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlWord.With))
			return null;

		var recursive = cursor.Take(SqlWord.Recursive);
		var items     = new List<CommonTableExpression>();

		while (true)
		{
			if (!WithListElement(ref cursor, out var item))
			{
				cursor = save;

				return null;
			}

			items.Add(item);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		return new WithClause(recursive, items);
	}

	static bool WithListElement(ref SqlCursor cursor, out CommonTableExpression item)
	{
		var save = cursor;

		item = null!;

		if (!Identifier(ref cursor, out var name))
			return false;

		IReadOnlyList<Identifier>? columns = null;

		if (cursor.Kind == SqlTokenKind.LeftParen)
		{
			var bracket = cursor;

			cursor.Take();

			if (ColumnNameList(ref cursor, out var written) && cursor.Take(SqlTokenKind.RightParen))
				columns = written;
			else
				cursor = bracket;
		}

		if (!cursor.Take(SqlWord.As) || !TableSubquery(ref cursor, out var query))
		{
			cursor = save;

			return false;
		}

		SearchClause? search = null;
		CycleClause?  cycle  = null;

		if (cursor.Word == SqlWord.Search)
			search = SearchClause(ref cursor);

		if (cursor.Word == SqlWord.Cycle)
			cycle = CycleClause(ref cursor);

		item = new CommonTableExpression(name, columns ?? [], query, search, cycle);

		return true;
	}

	static SearchClause? SearchClause(ref SqlCursor cursor)
	{
		var save = cursor;

		cursor.Take();

		var depth = cursor.TakeWord("DEPTH");

		if ((depth || cursor.TakeWord("BREADTH")) && cursor.TakeWord("FIRST") && cursor.Take(SqlWord.By) &&
			ColumnNameList(ref cursor, out var columns) && cursor.Take(SqlWord.Set) && Identifier(ref cursor, out var sequence))
			return new SearchClause(depth ? SearchOrder.DepthFirst : SearchOrder.BreadthFirst, columns, sequence);

		cursor = save;

		return null;
	}

	static CycleClause? CycleClause(ref SqlCursor cursor)
	{
		var save = cursor;

		cursor.Take();

		if (ColumnNameList(ref cursor, out var columns) && cursor.Take(SqlWord.Set) && Identifier(ref cursor, out var mark))
		{
			Expression? value = null, otherwise = null;

			if (cursor.Take(SqlWord.To))
			{
				if (!Value(ref cursor, out value) || !cursor.Take(SqlWord.Default) || !Value(ref cursor, out otherwise))
				{
					cursor = save;

					return null;
				}
			}

			if (cursor.Take(SqlWord.Using) && Identifier(ref cursor, out var path))
				return new CycleClause(columns, mark, value, otherwise, path);
		}

		cursor = save;

		return null;
	}

	/// <summary>
	/// <c>&lt;query expression body&gt;</c>: query terms, and the <c>UNION</c>s and <c>EXCEPT</c>s
	/// between them, which are a repetition here because the BNF recurses on the left.
	/// </summary>
	static bool QueryExpressionBody(ref SqlCursor cursor, out Statement.Select query)
	{
		if (!QueryTerm(ref cursor, out query))
			return false;

		List<SetOperation>? rest = null;

		while (cursor.Word is SqlWord.Union or SqlWord.Except)
		{
			var save = cursor;
			var op   = cursor.Word == SqlWord.Union ? SetOperator.Union : SetOperator.Except;

			cursor.Take();

			var quantifier    = SetQuantifier(ref cursor);
			var corresponding = CorrespondingSpec(ref cursor);

			if (!QueryTerm(ref cursor, out var operand))
			{
				cursor = save;

				break;
			}

			(rest ??= []).Add(new SetOperation
			{
				Operator      = op,
				Quantifier    = quantifier,
				Corresponding = corresponding,
				Operand       = OperandOf(operand),
			});
		}

		query = Combined(query, rest);

		return true;
	}

	static bool QueryTerm(ref SqlCursor cursor, out Statement.Select query)
	{
		if (!QueryPrimary(ref cursor, out query))
			return false;

		List<SetOperation>? rest = null;

		while (cursor.Word == SqlWord.Intersect)
		{
			var save = cursor;

			cursor.Take();

			var quantifier    = SetQuantifier(ref cursor);
			var corresponding = CorrespondingSpec(ref cursor);

			if (!QueryPrimary(ref cursor, out var operand))
			{
				cursor = save;

				break;
			}

			(rest ??= []).Add(new SetOperation
			{
				Operator      = SetOperator.Intersect,
				Quantifier    = quantifier,
				Corresponding = corresponding,
				Operand       = OperandOf(operand),
			});
		}

		query = Combined(query, rest);

		return true;
	}

	/// <summary>An operand as the kind it is: a table value constructor or an explicit table alone, or a query.</summary>
	static QueryOperand OperandOf(Statement.Select query) =>
		query.Body is QueryOperand.Values or QueryOperand.Table
		&& query.Parentheses == 0 && query.SetOperations.Count == 0 && query.With is null && query.OrderBy is null && query.Offset is null && query.Fetch is null
			? query.Body
			: new QueryOperand.Select(query);

	/// <summary>An operand and the operations after it, where the first has none of its own.</summary>
	static Statement.Select Combined(Statement.Select first, List<SetOperation>? rest)
	{
		if (rest is not { Count: > 0 })
			return first;

		if (first.SetOperations.Count == 0 && first.Parentheses == 0 && first.With is null && first.OrderBy is null && first.Offset is null && first.Fetch is null)
			return first with { SetOperations = rest };

		return new Statement.Select { Body = new QueryOperand.Select(first), SetOperations = rest };
	}

	static bool QueryPrimary(ref SqlCursor cursor, out Statement.Select query)
	{
		if (SimpleTable(ref cursor, out query))
			return true;

		var save = cursor;

		if (cursor.Take(SqlTokenKind.LeftParen) && QueryExpressionBody(ref cursor, out var body))
		{
			var order  = OrderByClause(ref cursor);
			var offset = ResultOffsetClause(ref cursor);
			var fetch  = FetchFirstClause(ref cursor);

			if (cursor.Take(SqlTokenKind.RightParen))
			{
				var inner = Expressed(body, null, order, offset, fetch);

				query = inner with { Parentheses = inner.Parentheses + 1 };

				return true;
			}
		}

		cursor = save;
		query  = null!;

		return false;
	}

	/// <summary><c>&lt;simple table&gt;</c>: a query specification, a table value constructor, or an explicit table.</summary>
	static bool SimpleTable(ref SqlCursor cursor, out Statement.Select query)
	{
		if (cursor.Word == SqlWord.Select)
			return QuerySpecification(ref cursor, out query);

		var save = cursor;

		if (cursor.Take(SqlWord.Values))
		{
			var rows = new List<RowValue>();

			while (true)
			{
				if (!Value(ref cursor, out var value))
				{
					cursor = save;
					query  = null!;

					return false;
				}

				rows.Add(value is Expression.Row row ? new RowValue(row.Items, row.RowKeyword) : new RowValue([value]));

				if (!cursor.Take(SqlTokenKind.Comma))
					break;
			}

			query = new Statement.Select { Body = new QueryOperand.Values(rows) };

			return true;
		}

		if (cursor.Take(SqlWord.Table) && TableName(ref cursor, out var name))
		{
			query = new Statement.Select { Body = new QueryOperand.Table(name) };

			return true;
		}

		cursor = save;
		query  = null!;

		return false;
	}

	static CorrespondingClause? CorrespondingSpec(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlWord.Corresponding))
			return null;

		if (cursor.Take(SqlWord.By))
		{
			if (cursor.Take(SqlTokenKind.LeftParen) && ColumnNameList(ref cursor, out var columns) && cursor.Take(SqlTokenKind.RightParen))
				return new CorrespondingClause(true, columns);

			cursor = save;

			return null;
		}

		return new CorrespondingClause(false, []);
	}

	static OrderByClause? OrderByClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Order) && cursor.Take(SqlWord.By) && SortSpecificationList(ref cursor, out var order))
			return order;

		cursor = save;

		return null;
	}

	static OffsetClause? ResultOffsetClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Offset) && SimpleValueSpecification(ref cursor, out var value))
		{
			var word = RowWord(ref cursor);

			if (word is not null)
				return new OffsetClause(value, word.Value);
		}

		cursor = save;

		return null;
	}

	static RowWord? RowWord(ref SqlCursor cursor) =>
		cursor.Take(SqlWord.Row)  ? Ast.RowWord.Row :
		cursor.Take(SqlWord.Rows) ? Ast.RowWord.Rows :
		null;

	static FetchClause? FetchFirstClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlWord.Fetch))
			return null;

		var first = cursor.TakeWord("FIRST");

		if (!first && !cursor.TakeWord("NEXT"))
		{
			cursor = save;

			return null;
		}

		Expression? quantity = null;
		var percent = false;

		var marked = cursor;

		if (SimpleValueSpecification(ref cursor, out var written))
		{
			quantity = written;
			percent  = cursor.Take(SqlWord.Percent);
		}
		else
		{
			cursor = marked;
		}

		var rows = RowWord(ref cursor);

		if (rows is not null)
		{
			if (cursor.Take(SqlWord.Only))
				return new FetchClause(first ? FetchPosition.First : FetchPosition.Next, quantity, percent, rows.Value, FetchMode.Only);

			if (cursor.Take(SqlWord.With) && cursor.TakeWord("TIES"))
				return new FetchClause(first ? FetchPosition.First : FetchPosition.Next, quantity, percent, rows.Value, FetchMode.WithTies);
		}

		cursor = save;

		return null;
	}

	// ── §10.10 Sort specification list ─────────────────────────────────────────

	static bool SortSpecificationList(ref SqlCursor cursor, out OrderByClause? order)
	{
		var save  = cursor;
		var items = new List<SortItem>();

		order = null;

		while (true)
		{
			if (!Value(ref cursor, out var key))
			{
				cursor = save;

				return false;
			}

			var direction =
				cursor.TakeWord("ASC")  ? SortDirection.Asc :
				cursor.TakeWord("DESC") ? SortDirection.Desc :
				(SortDirection?)null;

			NullOrdering? nulls = null;

			var marked = cursor;

			if (cursor.TakeWord("NULLS"))
			{
				nulls =
					cursor.TakeWord("FIRST") ? NullOrdering.First :
					cursor.TakeWord("LAST")  ? NullOrdering.Last :
					(NullOrdering?)null;

				if (nulls is null)
					cursor = marked;
			}

			items.Add(new SortItem(key, direction, nulls));

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		order = new OrderByClause(items);

		return true;
	}

	// ── §7.12 Query specification ──────────────────────────────────────────────

	static bool QuerySpecification(ref SqlCursor cursor, out Statement.Select query)
	{
		var save = cursor;

		query = null!;

		if (!cursor.Take(SqlWord.Select))
			return false;

		var quantifier = SetQuantifier(ref cursor);

		if (!SelectList(ref cursor, out var items) || !TableExpression(ref cursor, out var table))
		{
			cursor = save;

			return false;
		}

		query = new Statement.Select
		{
			Quantifier = quantifier,
			Items      = items,
			From       = table.From,
			Where      = table.Where,
			GroupBy    = table.GroupBy,
			Having     = table.Having,
			Window     = table.Window,
		};

		return true;
	}

	static bool SelectList(ref SqlCursor cursor, out IReadOnlyList<SelectItem> items)
	{
		if (cursor.Take(SqlTokenKind.Asterisk))
		{
			items = [new SelectItem.All()];

			return true;
		}

		var save    = cursor;
		var sublist = new List<SelectItem>();

		items = sublist;

		while (true)
		{
			if (!SelectSublist(ref cursor, out var item))
			{
				cursor = save;

				return false;
			}

			sublist.Add(item);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		return true;
	}

	/// <summary>
	/// A derived column or a qualified asterisk: each begins with a value expression — an asterisked
	/// identifier chain is a column reference in shape — so the expression is read once, and its
	/// <c>.*</c> is a step of the primary.
	/// </summary>
	static bool SelectSublist(ref SqlCursor cursor, out SelectItem item)
	{
		var save = cursor;

		item = null!;

		if (!ValueExpression(ref cursor, out var value))
			return false;

		// `AS (a, b)` renames the fields of an all-fields reference, and asks that `.*` was the last step.
		var fields = cursor;

		if (cursor.Take(SqlWord.As) && cursor.Take(SqlTokenKind.LeftParen))
		{
			if ((value.Roles & SqlTowers.Starred) != 0 && ColumnNameList(ref cursor, out var columns) && cursor.Take(SqlTokenKind.RightParen))
			{
				item = Selected(value.Node, null, false, columns);

				return true;
			}

			cursor = save;

			return false;
		}

		cursor = fields;

		var keyword = cursor.Take(SqlWord.As);

		if (Identifier(ref cursor, out var alias))
		{
			item = Selected(value.Node, alias, keyword, null);

			return true;
		}

		if (keyword)
		{
			cursor = save;

			return false;
		}

		item = Selected(value.Node, null, false, null);

		return true;
	}

	static SelectItem Selected(Expression value, Identifier? alias, bool keyword, IReadOnlyList<Identifier>? columns)
	{
		if (value is Expression.Wildcard { Kind: WildcardKind.Member } all && alias is null)
			return new SelectItem.QualifiedAll(all.Target, columns);

		return new SelectItem.ExpressionItem(value, alias, keyword);
	}

	// ── §7.4 Table expression ──────────────────────────────────────────────────

	/// <summary>A from clause and the clauses after it, which a query specification spreads out.</summary>
	readonly record struct TableExpressionParts(FromClause From, Expression? Where, GroupByClause? GroupBy, Expression? Having, WindowClause? Window);

	static bool TableExpression(ref SqlCursor cursor, out TableExpressionParts table)
	{
		var save = cursor;

		table = default;

		if (!FromClause(ref cursor, out var from))
			return false;

		Expression? where = null, having = null;

		if (cursor.Take(SqlWord.Where) && !SearchCondition(ref cursor, out where))
		{
			cursor = save;

			return false;
		}

		var group = GroupByClause(ref cursor);

		if (cursor.Take(SqlWord.Having) && !SearchCondition(ref cursor, out having))
		{
			cursor = save;

			return false;
		}

		var window = WindowClause(ref cursor);

		table = new TableExpressionParts(from, where, group, having, window);

		return true;
	}

	static bool FromClause(ref SqlCursor cursor, out FromClause from)
	{
		var save   = cursor;
		var tables = new List<TableSource>();

		from = null!;

		if (!cursor.Take(SqlWord.From))
			return false;

		while (true)
		{
			if (!TableReference(ref cursor, out var table))
			{
				cursor = save;

				return false;
			}

			tables.Add(table);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		from = new FromClause(tables);

		return true;
	}

	static GroupByClause? GroupByClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlWord.Group) || !cursor.Take(SqlWord.By))
		{
			cursor = save;

			return null;
		}

		var quantifier = SetQuantifier(ref cursor);
		var elements   = new List<GroupingElement>();

		while (true)
		{
			if (!GroupingElement(ref cursor, out var element))
			{
				cursor = save;

				return null;
			}

			elements.Add(element);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		return new GroupByClause(quantifier, elements);
	}

	static bool GroupingElement(ref SqlCursor cursor, out GroupingElement element)
	{
		var save = cursor;

		element = null!;

		switch (cursor.Word)
		{
			case SqlWord.Rollup:
			case SqlWord.Cube:
			{
				var rollup = cursor.Word == SqlWord.Rollup;

				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen))
				{
					var sets = new List<GroupingElement>();

					while (true)
					{
						if (!OrdinaryGroupingSet(ref cursor, out var set))
							break;

						sets.Add(set);

						if (!cursor.Take(SqlTokenKind.Comma))
							break;
					}

					if (sets.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
					{
						element = rollup ? new GroupingElement.Rollup(sets) : new GroupingElement.Cube(sets);

						return true;
					}
				}

				cursor = save;

				return false;
			}

			case SqlWord.Grouping:
			{
				cursor.Take();

				if (cursor.TakeWord("SETS") && cursor.Take(SqlTokenKind.LeftParen))
				{
					var sets = new List<GroupingElement>();

					while (true)
					{
						if (!GroupingElement(ref cursor, out var set))
							break;

						sets.Add(set);

						if (!cursor.Take(SqlTokenKind.Comma))
							break;
					}

					if (sets.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
					{
						element = new GroupingElement.Sets(sets);

						return true;
					}
				}

				cursor = save;

				return false;
			}
		}

		// `()`, the empty grouping set, before an ordinary one that would take its bracket.
		if (cursor.Kind == SqlTokenKind.LeftParen)
		{
			var bracket = cursor;

			cursor.Take();

			if (cursor.Take(SqlTokenKind.RightParen))
			{
				element = new GroupingElement.Empty();

				return true;
			}

			cursor = bracket;
		}

		return OrdinaryGroupingSet(ref cursor, out element);
	}

	static bool OrdinaryGroupingSet(ref SqlCursor cursor, out GroupingElement element)
	{
		var save = cursor;

		element = null!;

		if (cursor.Take(SqlTokenKind.LeftParen))
		{
			var columns = new List<Expression>();

			while (true)
			{
				if (!GroupingColumnReference(ref cursor, out var column))
					break;

				columns.Add(column);

				if (!cursor.Take(SqlTokenKind.Comma))
					break;
			}

			if (columns.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
			{
				element = new GroupingElement.Ordinary(columns, true);

				return true;
			}

			cursor = save;

			return false;
		}

		if (GroupingColumnReference(ref cursor, out var one))
		{
			element = new GroupingElement.Ordinary([one]);

			return true;
		}

		return false;
	}

	static bool GroupingColumnReference(ref SqlCursor cursor, out Expression column)
	{
		if (!ColumnReference(ref cursor, out column))
			return false;

		if (CollateClause(ref cursor, out var collation))
			column = new Expression.Collate(column, collation);

		return true;
	}

	static bool ColumnNameList(ref SqlCursor cursor, out IReadOnlyList<Identifier> columns)
	{
		var save  = cursor;
		var names = new List<Identifier>();

		columns = names;

		if (!Identifier(ref cursor, out var first))
			return false;

		names.Add(first);

		while (true)
		{
			// The comma the list cannot go on from is given back: what follows it is a period's
			// name, or nothing of the list's.
			var comma = cursor;

			if (!cursor.Take(SqlTokenKind.Comma) || !Identifier(ref cursor, out var name))
			{
				cursor = comma;

				break;
			}

			names.Add(name);
		}

		return true;
	}

	// ── §7.11 Window clause ────────────────────────────────────────────────────

	static WindowClause? WindowClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlWord.Window))
			return null;

		var definitions = new List<WindowDefinition>();

		while (true)
		{
			if (!Identifier(ref cursor, out var name) || !cursor.Take(SqlWord.As) || !WindowSpecification(ref cursor, out var specification))
			{
				cursor = save;

				return null;
			}

			definitions.Add(new WindowDefinition(name, specification));

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		return new WindowClause(definitions);
	}

	static bool WindowSpecification(ref SqlCursor cursor, out WindowSpecification specification)
	{
		var save = cursor;

		specification = null!;

		if (!cursor.Take(SqlTokenKind.LeftParen))
			return false;

		var existing = ExistingWindowName(ref cursor);
		var partition = WindowPartitionClause(ref cursor);
		var order = OrderByClause(ref cursor);
		var frame = WindowFrameClause(ref cursor);

		if (cursor.Take(SqlTokenKind.RightParen))
		{
			specification = new WindowSpecification(existing, partition ?? [], order, frame);

			return true;
		}

		cursor = save;

		return false;
	}

	/// <summary>
	/// An existing window's name, which is a name only where what may follow a name follows it: a
	/// frame may begin with <c>MEASURES</c>, which §5.2 does not reserve.
	/// </summary>
	static Identifier? ExistingWindowName(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!Identifier(ref cursor, out var name))
			return null;

		if (cursor.Kind == SqlTokenKind.RightParen || cursor.Word is SqlWord.Partition or SqlWord.Order or SqlWord.Rows or SqlWord.Range or SqlWord.Groups ||
			cursor.IsWord("MEASURES"))
			return name;

		cursor = save;

		return null;
	}

	static IReadOnlyList<Expression>? WindowPartitionClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlWord.Partition) || !cursor.Take(SqlWord.By))
		{
			cursor = save;

			return null;
		}

		var columns = new List<Expression>();

		while (true)
		{
			if (!GroupingColumnReference(ref cursor, out var column))
			{
				cursor = save;

				return null;
			}

			columns.Add(column);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		return columns;
	}

	static WindowFrame? WindowFrameClause(ref SqlCursor cursor)
	{
		var save     = cursor;
		var measures = RowPatternMeasures(ref cursor);

		var unit =
			cursor.Take(SqlWord.Rows)   ? WindowFrameUnit.Rows :
			cursor.Take(SqlWord.Range)  ? WindowFrameUnit.Range :
			cursor.Take(SqlWord.Groups) ? WindowFrameUnit.Groups :
			(WindowFrameUnit?)null;

		if (unit is null || !WindowFrameExtent(ref cursor, out var extent))
		{
			cursor = save;

			return null;
		}

		var exclusion = WindowFrameExclusion(ref cursor);
		var common    = RowPatternCommonSyntax(ref cursor);

		var pattern = common is null && measures is null
			? null
			: (common ?? new RowPatternClause([], null, [], null, null, null, null, [], [])) with { Measures = measures ?? [] };

		return new WindowFrame(unit.Value, extent, exclusion, pattern);
	}

	static bool WindowFrameExtent(ref SqlCursor cursor, out WindowFrameExtent extent)
	{
		var save = cursor;

		extent = null!;

		if (cursor.Take(SqlWord.Between))
		{
			if (WindowFrameBound(ref cursor, out var from) && cursor.Take(SqlWord.And) && WindowFrameBound(ref cursor, out var to))
			{
				extent = new WindowFrameExtent.Between(from, to);

				return true;
			}

			cursor = save;

			return false;
		}

		if (WindowFrameStart(ref cursor, out var bound))
		{
			extent = new WindowFrameExtent.Single(bound);

			return true;
		}

		return false;
	}

	static bool WindowFrameStart(ref SqlCursor cursor, out WindowFrameBound bound)
	{
		var save = cursor;

		bound = null!;

		if (cursor.TakeWord("UNBOUNDED"))
		{
			if (cursor.TakeWord("PRECEDING"))
			{
				bound = new WindowFrameBound(WindowBoundKind.UnboundedPreceding);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Current))
		{
			if (cursor.Take(SqlWord.Row))
			{
				bound = new WindowFrameBound(WindowBoundKind.CurrentRow);

				return true;
			}

			cursor = save;

			return false;
		}

		if (UnsignedValueSpecification(ref cursor, out var value) && cursor.TakeWord("PRECEDING"))
		{
			bound = new WindowFrameBound(WindowBoundKind.Preceding, value);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool WindowFrameBound(ref SqlCursor cursor, out WindowFrameBound bound)
	{
		var save = cursor;

		if (WindowFrameStart(ref cursor, out bound))
			return true;

		if (cursor.TakeWord("UNBOUNDED"))
		{
			if (cursor.TakeWord("FOLLOWING"))
			{
				bound = new WindowFrameBound(WindowBoundKind.UnboundedFollowing);

				return true;
			}

			cursor = save;

			return false;
		}

		if (UnsignedValueSpecification(ref cursor, out var value) && cursor.TakeWord("FOLLOWING"))
		{
			bound = new WindowFrameBound(WindowBoundKind.Following, value);

			return true;
		}

		cursor = save;
		bound  = null!;

		return false;
	}

	static WindowFrameExclusion? WindowFrameExclusion(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.TakeWord("EXCLUDE"))
			return null;

		if (cursor.Take(SqlWord.Current) && cursor.Take(SqlWord.Row))
			return Ast.WindowFrameExclusion.CurrentRow;

		if (cursor.Take(SqlWord.Group))
			return Ast.WindowFrameExclusion.Group;

		if (cursor.TakeWord("TIES"))
			return Ast.WindowFrameExclusion.Ties;

		if (cursor.Take(SqlWord.No) && cursor.TakeWord("OTHERS"))
			return Ast.WindowFrameExclusion.NoOthers;

		cursor = save;

		return null;
	}

	/// <summary><c>&lt;unsigned value specification&gt;</c>.</summary>
	static bool UnsignedValueSpecification(ref SqlCursor cursor, out Expression value)
	{
		if (UnsignedLiteral(ref cursor, out var literal))
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

	/// <summary><c>&lt;simple value specification&gt;</c>.</summary>
	static bool SimpleValueSpecification(ref SqlCursor cursor, out Expression value)
	{
		if (Literal(ref cursor, out var literal))
		{
			value = new Expression.Literal(literal);

			return true;
		}

		var save = cursor;

		if (cursor.Take(SqlTokenKind.Colon))
		{
			if (Identifier(ref cursor, out var host))
			{
				value = new Expression.Parameter(ParameterKind.Host, host);

				return true;
			}

			cursor = save;
		}

		if (IdentifierChain(ref cursor, out var name))
		{
			value = new Expression.Reference(name);

			return true;
		}

		value = null!;

		return false;
	}

	// ── §7.6 Table reference, §7.10 Joined table ───────────────────────────────

	/// <summary>
	/// <c>&lt;table reference&gt;</c>: a table factor and the joins after it, which the BNF recurses
	/// through on the left and which are steps here, folded from the left.
	/// </summary>
	static bool TableReference(ref SqlCursor cursor, out TableSource table)
	{
		if (!TableFactor(ref cursor, out var factor))
		{
			table = null!;

			return false;
		}

		var joins = Joins(ref cursor);

		table = Joined(factor.Source, joins);

		return true;
	}

	/// <summary>A table source, and whether it is a joined table in brackets.</summary>
	readonly record struct Factor(TableSource Source, bool JoinedTable);

	/// <summary>The joins read after a factor, and the partitioning of the factor before the first.</summary>
	readonly record struct JoinList(IReadOnlyList<Expression>? Partition, List<Joining>? Steps);

	/// <summary>One join: its type, whether it is natural, the table on its right, and its specification.</summary>
	readonly record struct Joining(JoinKind? Kind, bool Natural, bool Outer, TableSource Right, JoinSpecification? Specification, IReadOnlyList<Expression>? RightPartition);

	static TableSource Joined(TableSource left, JoinList joins)
	{
		if (joins.Steps is not { Count: > 0 } steps)
			return left;

		for (var at = 0; at < steps.Count; at++)
		{
			var step = steps[at];

			left = new TableSource.Join
			{
				Left           = left,
				Right          = step.Right,
				Kind           = step.Kind,
				Natural        = step.Natural,
				OuterKeyword   = step.Outer,
				Specification  = step.Specification,
				LeftPartition  = at == 0 ? joins.Partition : null,
				RightPartition = step.RightPartition,
			};
		}

		return left;
	}

	/// <summary>The joins after a factor: none, or steps, the first of which may have the factor's partitioning.</summary>
	static JoinList Joins(ref SqlCursor cursor)
	{
		var save      = cursor;
		var partition = JoinPartitioning(ref cursor);

		if (partition is not null)
		{
			if (!PartitionedJoin(ref cursor, out var first))
			{
				cursor = save;

				return new JoinList(null, null);
			}

			var steps = new List<Joining> { first };

			while (JoinStep(ref cursor, out var next))
				steps.Add(next);

			return new JoinList(partition, steps);
		}

		List<Joining>? written = null;

		while (JoinStep(ref cursor, out var step))
			(written ??= []).Add(step);

		return new JoinList(null, written);
	}

	/// <summary>
	/// A qualified join's right side: a table and its partitioning, or the joins it goes on into.
	/// Unlike the joins after a factor, a partitioning here needs no join after it: it is the right
	/// operand's own, and the join it belongs to is the one being read.
	/// </summary>
	static JoinList JoinOperandTail(ref SqlCursor cursor)
	{
		var partition = JoinPartitioning(ref cursor);

		if (partition is not null)
		{
			List<Joining>? partitioned = null;

			if (PartitionedJoin(ref cursor, out var first))
			{
				partitioned = [first];

				while (JoinStep(ref cursor, out var next))
					partitioned.Add(next);
			}

			return new JoinList(partition, partitioned);
		}

		List<Joining>? steps = null;

		while (JoinStep(ref cursor, out var step))
			(steps ??= []).Add(step);

		return new JoinList(null, steps);
	}

	static bool JoinStep(ref SqlCursor cursor, out Joining step)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Cross))
		{
			if (cursor.Take(SqlWord.Join) && TableFactor(ref cursor, out var factor))
			{
				step = new Joining(JoinKind.Cross, false, false, factor.Source, null, null);

				return true;
			}

			cursor = save;
			step   = default;

			return false;
		}

		return PartitionedJoin(ref cursor, out step);
	}

	/// <summary>A qualified or a natural join, from <c>JOIN</c> on.</summary>
	static bool PartitionedJoin(ref SqlCursor cursor, out Joining step)
	{
		var save = cursor;

		step = default;

		if (cursor.Take(SqlWord.Natural))
		{
			var natural = JoinType(ref cursor, out var outer);

			if (cursor.Take(SqlWord.Join) && TableFactor(ref cursor, out var factor))
			{
				var partition = JoinPartitioning(ref cursor);

				step = new Joining(natural, true, outer, factor.Source, null, partition);

				return true;
			}

			cursor = save;

			return false;
		}

		var kind = JoinType(ref cursor, out var keyword);

		if (!cursor.Take(SqlWord.Join))
		{
			cursor = save;

			return false;
		}

		// The right side is read as far as it goes: a join specification must follow it, and one a
		// join inside it took could not have been the outer join's.
		if (!TableFactor(ref cursor, out var right))
		{
			cursor = save;

			return false;
		}

		var tail = JoinOperandTail(ref cursor);
		var source = tail.Steps is { Count: > 0 } ? Joined(right.Source, tail) : right.Source;
		var partitioned = tail.Steps is { Count: > 0 } ? null : tail.Partition;

		if (!JoinSpecification(ref cursor, out var specification))
		{
			cursor = save;

			return false;
		}

		step = new Joining(kind, false, keyword, source, specification, partitioned);

		return true;
	}

	static JoinKind? JoinType(ref SqlCursor cursor, out bool outer)
	{
		outer = false;

		if (cursor.Take(SqlWord.Inner))
			return JoinKind.Inner;

		var kind =
			cursor.Take(SqlWord.Left)  ? JoinKind.Left :
			cursor.Take(SqlWord.Right) ? JoinKind.Right :
			cursor.Take(SqlWord.Full)  ? JoinKind.Full :
			(JoinKind?)null;

		if (kind is not null)
			outer = cursor.Take(SqlWord.Outer);

		return kind;
	}

	static bool JoinSpecification(ref SqlCursor cursor, out JoinSpecification specification)
	{
		var save = cursor;

		specification = null!;

		if (cursor.Take(SqlWord.On))
		{
			if (SearchCondition(ref cursor, out var condition))
			{
				specification = new JoinSpecification.On(condition);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Using) && cursor.Take(SqlTokenKind.LeftParen) && ColumnNameList(ref cursor, out var columns) &&
			cursor.Take(SqlTokenKind.RightParen))
		{
			Identifier? name = null;

			var marked = cursor;

			if (cursor.Take(SqlWord.As) && !Identifier(ref cursor, out name))
			{
				cursor = marked;
				name   = null;
			}

			specification = new JoinSpecification.Using(columns, name);

			return true;
		}

		cursor = save;

		return false;
	}

	static IReadOnlyList<Expression>? JoinPartitioning(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlWord.Partition) || !cursor.Take(SqlWord.By) || !cursor.Take(SqlTokenKind.LeftParen))
		{
			cursor = save;

			return null;
		}

		var columns = new List<Expression>();

		while (true)
		{
			if (!ColumnReference(ref cursor, out var column))
			{
				cursor = save;

				return null;
			}

			columns.Add(column);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		if (cursor.Take(SqlTokenKind.RightParen))
			return columns;

		cursor = save;

		return null;
	}

	/// <summary>A table primary and the sample clause after it, which ends a parenthesized joined table.</summary>
	static bool TableFactor(ref SqlCursor cursor, out Factor factor)
	{
		if (!TablePrimary(ref cursor, out factor))
			return false;

		var sample = SampleClause(ref cursor);

		if (sample is not null)
			factor = new Factor(factor.Source with { Sample = sample }, false);

		return true;
	}

	static SampleClause? SampleClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlWord.Tablesample))
			return null;

		var method =
			cursor.TakeWord("BERNOULLI") ? SampleMethod.Bernoulli :
			cursor.TakeWord("SYSTEM")    ? SampleMethod.System :
			(SampleMethod?)null;

		if (method is not null && cursor.Take(SqlTokenKind.LeftParen) && Numeric(ref cursor, out var percentage) &&
			cursor.Take(SqlTokenKind.RightParen))
		{
			Expression? repeat = null;

			var marked = cursor;

			if (cursor.TakeWord("REPEATABLE"))
			{
				if (!cursor.Take(SqlTokenKind.LeftParen) || !Numeric(ref cursor, out repeat) || !cursor.Take(SqlTokenKind.RightParen))
				{
					cursor = marked;
					repeat = null;
				}
			}

			return new SampleClause(method.Value, percentage, repeat);
		}

		cursor = save;

		return null;
	}

	/// <summary><c>&lt;table primary&gt;</c>, and whether it is a joined table in brackets.</summary>
	static bool TablePrimary(ref SqlCursor cursor, out Factor factor)
	{
		var save = cursor;

		factor = default;

		switch (cursor.Word)
		{
			case SqlWord.Lateral:
			{
				cursor.Take();

				if (TableSubquery(ref cursor, out var query) && Correlation(ref cursor, out var correlation, true))
				{
					factor = Correlate(new TableSource.Subquery(query) { Lateral = true }, correlation);

					return true;
				}

				cursor = save;

				return false;
			}

			case SqlWord.Unnest:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen))
				{
					var collections = new List<Expression>();

					while (true)
					{
						if (!Node(ref cursor, SqlTowers.Array | SqlTowers.Multiset, out var collection))
							break;

						collections.Add(collection);

						if (!cursor.Take(SqlTokenKind.Comma))
							break;
					}

					if (collections.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
					{
						var ordinality = false;
						var marked     = cursor;

						if (cursor.Take(SqlWord.With))
						{
							ordinality = cursor.TakeWord("ORDINALITY");

							if (!ordinality)
								cursor = marked;
						}

						if (Correlation(ref cursor, out var correlation, true))
						{
							factor = Correlate(new TableSource.Unnest(collections, ordinality, null), correlation);

							return true;
						}
					}
				}

				cursor = save;

				return false;
			}

			case SqlWord.Table:
			{
				// `TABLE (f(x))`: a collection's derived table, which must be named, or a polymorphic
				// table function's, which need not.
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && CommonValueExpression(ref cursor, out var value) &&
					(value.Roles & (SqlTowers.Array | SqlTowers.Multiset)) != 0 && cursor.Take(SqlTokenKind.RightParen))
				{
					Correlation(ref cursor, out var correlation, false);

					if (correlation.Alias is not null || correlation.Clause is not null || (value.Roles & SqlTowers.Invoked) != 0)
					{
						factor = Correlate(new TableSource.TableFunction(value.Node), correlation);

						return true;
					}
				}

				cursor = save;

				return false;
			}

			case SqlWord.Only:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && TableName(ref cursor, out var name) && cursor.Take(SqlTokenKind.RightParen))
				{
					Correlation(ref cursor, out var correlation, false);

					factor = Correlate(new TableSource.Named(name) { Only = true }, correlation);

					return true;
				}

				cursor = save;

				return false;
			}

			case SqlWord.JsonTable:
			{
				if (JSONTable(ref cursor, out var defined) && Correlation(ref cursor, out var correlation, true))
				{
					factor = Correlate(new TableSource.JsonTable(defined), correlation);

					return true;
				}

				cursor = save;

				return false;
			}

			case SqlWord.JsonTablePrimitive:
			{
				if (JSONTablePrimitive(ref cursor, out var primitive) && Identifier(ref cursor, out var name))
				{
					factor = new Factor(new TableSource.JsonTable(primitive, new Alias(name)), false);

					return true;
				}

				cursor = save;

				return false;
			}
		}

		// A data change delta table is asked first: `FINAL` is not reserved, and would be taken for
		// a table's name.
		if (DataChangeDeltaTable(ref cursor, out var change))
		{
			Correlation(ref cursor, out var correlation, false);

			factor = Correlate(change, correlation);

			return true;
		}

		if (cursor.Kind == SqlTokenKind.LeftParen)
		{
			// A subquery, or a joined table in brackets: both begin with a bracket, and the subquery
			// is asked first, as the BNF asks it.
			var bracket = cursor;

			if (TableSubquery(ref cursor, out var query) && Correlation(ref cursor, out var correlation, true))
			{
				factor = Correlate(new TableSource.Subquery(query), correlation);

				return true;
			}

			cursor = bracket;

			if (ParenthesizedJoinedTable(ref cursor, out var joined))
			{
				factor = new Factor(new TableSource.Parenthesized(joined), true);

				return true;
			}

			cursor = save;

			return false;
		}

		if (TableName(ref cursor, out var table))
		{
			var time = QuerySystemTimePeriodSpecification(ref cursor);

			Correlation(ref cursor, out var named, false);

			factor = Correlate(new TableSource.Named(table) { SystemTime = time }, named);

			return true;
		}

		cursor = save;

		return false;
	}

	/// <summary>A joined table in brackets, or one such in brackets again.</summary>
	static bool ParenthesizedJoinedTable(ref SqlCursor cursor, out TableSource table)
	{
		var save = cursor;

		table = null!;

		if (!cursor.Take(SqlTokenKind.LeftParen) || !TableFactor(ref cursor, out var factor))
		{
			cursor = save;

			return false;
		}

		var joins = Joins(ref cursor);

		if (cursor.Take(SqlTokenKind.RightParen) && (factor.JoinedTable || joins.Steps is { Count: > 0 }))
		{
			table = Joined(factor.Source, joins);

			return true;
		}

		cursor = save;

		return false;
	}

	/// <summary>A correlation name, a row pattern recognition clause with the names around it, or both.</summary>
	readonly record struct CorrelationParts(Alias? Alias, RowPatternClause? Clause, Alias? ClauseAlias);

	static bool Correlation(ref SqlCursor cursor, out CorrelationParts correlation, bool required)
	{
		var alias = Correlated(ref cursor);
		var save  = cursor;
		var clause = RowPatternRecognitionClause(ref cursor);

		if (clause is null)
		{
			cursor      = save;
			correlation = new CorrelationParts(alias, null, null);

			return !required || alias is not null;
		}

		correlation = new CorrelationParts(alias, clause, Correlated(ref cursor));

		return true;
	}

	/// <summary>
	/// A correlation name, which is a word §5.2 does not reserve — and not a word that goes on into a
	/// clause of the function around it, since no table's name could stand there either.
	/// </summary>
	static Alias? Correlated(ref SqlCursor cursor)
	{
		var save = cursor;

		if (Goes(ref cursor))
		{
			cursor = save;

			return null;
		}

		var keyword = cursor.Take(SqlWord.As);

		if (!Identifier(ref cursor, out var name))
		{
			cursor = save;

			return null;
		}

		IReadOnlyList<Identifier>? columns = null;

		var bracket = cursor;

		if (cursor.Take(SqlTokenKind.LeftParen))
		{
			if (ColumnNameList(ref cursor, out var written) && cursor.Take(SqlTokenKind.RightParen))
				columns = written;
			else
				cursor = bracket;
		}

		return new Alias(name, columns, keyword);
	}

	/// <summary>Whether what stands here goes on with a clause of the JSON function around the query.</summary>
	static bool Goes(ref SqlCursor cursor)
	{
		var save = cursor;

		var goes =
			cursor.TakeWord("FORMAT")    && cursor.Take(SqlWord.Json) ||
			cursor.TakeWord("RETURNING") && DataType(ref cursor, out _) ||
			cursor.Take(SqlWord.Absent)  && cursor.Take(SqlWord.On) && cursor.Take(SqlWord.Null);

		cursor = save;

		return goes;
	}

	static Factor Correlate(TableSource source, CorrelationParts correlation)
	{
		if (correlation.Alias is { } alias)
			source = source switch
			{
				TableSource.Named named         => named with { Alias = alias },
				TableSource.Subquery subquery   => subquery with { Alias = alias },
				TableSource.Unnest unnest       => unnest with { Alias = alias },
				TableSource.Function function   => function with { Alias = alias },
				TableSource.TableFunction table => table with { Alias = alias },
				TableSource.JsonTable json      => json with { Alias = alias },
				TableSource.DataChange change   => change with { Alias = alias },
				_                               => throw new ArgumentOutOfRangeException(nameof(source), source, "A table source that takes no correlation name."),
			};

		if (correlation.Clause is { } clause)
			source = new TableSource.RowPatternRecognition(source, clause, correlation.ClauseAlias);

		return new Factor(source, false);
	}

	static SystemTimeSpecification? QuerySystemTimePeriodSpecification(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlWord.For) || !cursor.Take(SqlWord.SystemTime))
		{
			cursor = save;

			return null;
		}

		if (cursor.Take(SqlWord.As) && cursor.Take(SqlWord.Of) && Datetime(ref cursor, out var point))
			return new SystemTimeSpecification.AsOf(point);

		cursor = save;
		cursor.Take();
		cursor.Take();

		if (cursor.Take(SqlWord.Between))
		{
			var symmetry =
				cursor.Take(SqlWord.Asymmetric) ? BetweenSymmetry.Asymmetric :
				cursor.Take(SqlWord.Symmetric)  ? BetweenSymmetry.Symmetric :
				(BetweenSymmetry?)null;

			if (Datetime(ref cursor, out var low) && cursor.Take(SqlWord.And) && Datetime(ref cursor, out var high))
				return new SystemTimeSpecification.Between(low, high, symmetry);
		}
		else if (cursor.Take(SqlWord.From))
		{
			if (Datetime(ref cursor, out var low) && cursor.Take(SqlWord.To) && Datetime(ref cursor, out var high))
				return new SystemTimeSpecification.FromTo(low, high);
		}

		cursor = save;

		return null;
	}

	// ── §7.15 Subquery ─────────────────────────────────────────────────────────

	/// <summary>A subquery's brackets are the subquery's, and the query in them has none of its own.</summary>
	static bool Subquery(ref SqlCursor cursor, out Statement.Select query)
	{
		var save = cursor;

		if (cursor.Take(SqlTokenKind.LeftParen) && QueryExpression(ref cursor, out query) && cursor.Take(SqlTokenKind.RightParen))
			return true;

		cursor = save;
		query  = null!;

		return false;
	}

	static bool ScalarSubquery(ref SqlCursor cursor, out Statement.Select query) => Subquery(ref cursor, out query);

	static bool TableSubquery(ref SqlCursor cursor, out Statement.Select query) => Subquery(ref cursor, out query);

	// ── §7.6 Row pattern recognition clause ────────────────────────────────────

	static RowPatternClause? RowPatternRecognitionClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlWord.MatchRecognize) || !cursor.Take(SqlTokenKind.LeftParen))
		{
			cursor = save;

			return null;
		}

		IReadOnlyList<Expression>? partition = null;

		if (cursor.Word == SqlWord.Partition)
		{
			partition = RowPatternPartitionBy(ref cursor);

			if (partition is null)
			{
				cursor = save;

				return null;
			}
		}

		var order    = OrderByClause(ref cursor);
		var measures = RowPatternMeasures(ref cursor);
		var rows     = RowPatternRowsPerMatch(ref cursor);
		var common   = RowPatternCommonSyntax(ref cursor);

		if (common is null || !cursor.Take(SqlTokenKind.RightParen))
		{
			cursor = save;

			return null;
		}

		return common with { PartitionBy = partition ?? [], OrderBy = order, Measures = measures ?? [], RowsPerMatch = rows };
	}

	static IReadOnlyList<Expression>? RowPatternPartitionBy(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlWord.Partition) || !cursor.Take(SqlWord.By))
		{
			cursor = save;

			return null;
		}

		var columns = new List<Expression>();

		while (true)
		{
			if (!GroupingColumnReference(ref cursor, out var column))
			{
				cursor = save;

				return null;
			}

			columns.Add(column);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		return columns;
	}

	static IReadOnlyList<RowPatternMeasure>? RowPatternMeasures(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.TakeWord("MEASURES"))
			return null;

		var measures = new List<RowPatternMeasure>();

		while (true)
		{
			if (!Value(ref cursor, out var value) || !cursor.Take(SqlWord.As) || !Identifier(ref cursor, out var name))
			{
				cursor = save;

				return null;
			}

			measures.Add(new RowPatternMeasure(value, name));

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		return measures;
	}

	static RowsPerMatch? RowPatternRowsPerMatch(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.One))
		{
			if (cursor.Take(SqlWord.Row) && cursor.Take(SqlWord.Per) && cursor.Take(SqlWord.Match))
				return RowsPerMatch.One;

			cursor = save;

			return null;
		}

		if (cursor.Take(SqlWord.All))
		{
			if (cursor.Take(SqlWord.Rows) && cursor.Take(SqlWord.Per) && cursor.Take(SqlWord.Match))
			{
				var marked = cursor;

				if (cursor.TakeWord("SHOW") && cursor.Take(SqlWord.Empty) && cursor.TakeWord("MATCHES"))
					return RowsPerMatch.AllShowEmpty;

				cursor = marked;

				if (cursor.Take(SqlWord.Omit) && cursor.Take(SqlWord.Empty) && cursor.TakeWord("MATCHES"))
					return RowsPerMatch.AllOmitEmpty;

				cursor = marked;

				if (cursor.Take(SqlWord.With) && cursor.TakeWord("UNMATCHED") && cursor.Take(SqlWord.Rows))
					return RowsPerMatch.AllWithUnmatched;

				cursor = marked;

				return RowsPerMatch.All;
			}

			cursor = save;

			return null;
		}

		return null;
	}

	/// <summary>The common syntax of a row pattern, which a window frame ends in too.</summary>
	static RowPatternClause? RowPatternCommonSyntax(ref SqlCursor cursor)
	{
		var save = cursor;
		var skip = RowPatternSkipTo(ref cursor);

		var initial =
			cursor.Take(SqlWord.Initial) ? RowPatternInitial.Initial :
			cursor.Take(SqlWord.Seek)    ? RowPatternInitial.Seek :
			(RowPatternInitial?)null;

		if (!cursor.Take(SqlWord.Pattern) || !cursor.Take(SqlTokenKind.LeftParen) || !RowPattern(ref cursor, out var pattern) ||
			!cursor.Take(SqlTokenKind.RightParen))
		{
			cursor = save;

			return null;
		}

		var subsets = RowPatternSubsetClause(ref cursor);

		if (!cursor.Take(SqlWord.Define))
		{
			cursor = save;

			return null;
		}

		var definitions = new List<RowPatternDefinition>();

		while (true)
		{
			if (!Identifier(ref cursor, out var name) || !cursor.Take(SqlWord.As) || !SearchCondition(ref cursor, out var condition))
			{
				cursor = save;

				return null;
			}

			definitions.Add(new RowPatternDefinition(name, condition));

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		return new RowPatternClause([], null, [], null, skip, initial, pattern, subsets ?? [], definitions);
	}

	/// <summary>
	/// <c>&lt;row pattern skip to&gt;</c>. A variable's name is an identifier, and <c>FIRST</c>,
	/// <c>LAST</c> and <c>NEXT</c> are none of them reserved.
	/// </summary>
	static RowPatternSkip? RowPatternSkipTo(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.TakeWord("AFTER") || !cursor.Take(SqlWord.Match))
		{
			cursor = save;

			return null;
		}

		if (!cursor.Take(SqlWord.Skip))
		{
			cursor = save;

			return null;
		}

		var marked = cursor;

		if (cursor.Take(SqlWord.To) && cursor.TakeWord("NEXT") && cursor.Take(SqlWord.Row))
			return new RowPatternSkip(RowPatternSkipKind.NextRow);

		cursor = marked;

		if (cursor.TakeWord("PAST") && cursor.TakeWord("LAST") && cursor.Take(SqlWord.Row))
			return new RowPatternSkip(RowPatternSkipKind.PastLastRow);

		cursor = marked;

		if (cursor.Take(SqlWord.To))
		{
			var to = cursor;

			var which =
				cursor.TakeWord("FIRST") ? RowPatternSkipKind.First :
				cursor.TakeWord("LAST")  ? RowPatternSkipKind.Last :
				(RowPatternSkipKind?)null;

			if (which is not null && Identifier(ref cursor, out var variable))
				return new RowPatternSkip(which.Value, variable);

			cursor = to;

			if (Identifier(ref cursor, out var named))
				return new RowPatternSkip(RowPatternSkipKind.Variable, named);
		}

		cursor = save;

		return null;
	}

	static IReadOnlyList<RowPatternSubset>? RowPatternSubsetClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlWord.Subset))
			return null;

		var subsets = new List<RowPatternSubset>();

		while (true)
		{
			if (!Identifier(ref cursor, out var name) || !cursor.Take(SqlTokenKind.Equal) || !cursor.Take(SqlTokenKind.LeftParen))
			{
				cursor = save;

				return null;
			}

			var items = new List<Identifier>();

			while (true)
			{
				if (!Identifier(ref cursor, out var item))
				{
					cursor = save;

					return null;
				}

				items.Add(item);

				if (!cursor.Take(SqlTokenKind.Comma))
					break;
			}

			if (!cursor.Take(SqlTokenKind.RightParen))
			{
				cursor = save;

				return null;
			}

			subsets.Add(new RowPatternSubset(name, items));

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		return subsets;
	}

	static bool RowPattern(ref SqlCursor cursor, out RowPattern pattern)
	{
		if (!RowPatternTerm(ref cursor, out pattern))
			return false;

		List<RowPattern>? rest = null;

		while (cursor.Kind == SqlTokenKind.Unknown && cursor.Span.Length == 1 && cursor.Span[0] == '|')
		{
			var save = cursor;

			cursor.Take();

			if (!RowPatternTerm(ref cursor, out var next))
			{
				cursor = save;

				break;
			}

			(rest ??= [pattern]).Add(next);
		}

		if (rest is not null)
			pattern = new RowPattern.Alternation(rest);

		return true;
	}

	static bool RowPatternTerm(ref SqlCursor cursor, out RowPattern pattern)
	{
		if (!RowPatternFactor(ref cursor, out pattern))
			return false;

		List<RowPattern>? rest = null;

		while (RowPatternFactor(ref cursor, out var next))
			(rest ??= [pattern]).Add(next);

		if (rest is not null)
			pattern = new RowPattern.Sequence(rest);

		return true;
	}

	static bool RowPatternFactor(ref SqlCursor cursor, out RowPattern pattern)
	{
		if (!RowPatternPrimary(ref cursor, out pattern))
			return false;

		var quantifier = RowPatternQuantifier(ref cursor);

		if (quantifier is not null)
			pattern = new RowPattern.Quantified(pattern, quantifier);

		return true;
	}

	static bool RowPatternPrimary(ref SqlCursor cursor, out RowPattern pattern)
	{
		var save = cursor;

		pattern = null!;

		if (cursor.IsWord("PERMUTE"))
		{
			cursor.Take();

			if (cursor.Take(SqlTokenKind.LeftParen))
			{
				var items = new List<RowPattern>();

				while (true)
				{
					if (!RowPattern(ref cursor, out var item))
						break;

					items.Add(item);

					if (!cursor.Take(SqlTokenKind.Comma))
						break;
				}

				if (items.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
				{
					pattern = new RowPattern.Permute(items);

					return true;
				}
			}

			// What follows is no permutation, so the word is a variable's name.
			cursor = save;
		}

		if (Identifier(ref cursor, out var variable))
		{
			pattern = new RowPattern.Variable(variable);

			return true;
		}

		if (cursor.Kind == SqlTokenKind.Unknown && cursor.Span.Length == 1)
		{
			var ch = cursor.Span[0];

			if (ch == '$')
			{
				cursor.Take();

				pattern = new RowPattern.EndAnchor();

				return true;
			}

			if (ch == '^')
			{
				cursor.Take();

				pattern = new RowPattern.StartAnchor();

				return true;
			}
		}

		if (cursor.Take(SqlTokenKind.LeftParen))
		{
			RowPattern? inner = null;

			if (RowPattern(ref cursor, out var written))
				inner = written;

			if (cursor.Take(SqlTokenKind.RightParen))
			{
				pattern = new RowPattern.Parenthesized(inner);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlTokenKind.LeftBraceMinus))
		{
			if (RowPattern(ref cursor, out var excluded) && cursor.Take(SqlTokenKind.MinusRightBrace))
			{
				pattern = new RowPattern.Excluded(excluded);

				return true;
			}

			cursor = save;

			return false;
		}

		return false;
	}

	static RowPatternQuantifier? RowPatternQuantifier(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlTokenKind.Asterisk))
			return new RowPatternQuantifier(RowPatternQuantifierKind.ZeroOrMore, Reluctant: cursor.Take(SqlTokenKind.Question));

		if (cursor.Take(SqlTokenKind.Plus))
			return new RowPatternQuantifier(RowPatternQuantifierKind.OneOrMore, Reluctant: cursor.Take(SqlTokenKind.Question));

		if (cursor.Take(SqlTokenKind.Question))
			return new RowPatternQuantifier(RowPatternQuantifierKind.ZeroOrOne, Reluctant: cursor.Take(SqlTokenKind.Question));

		if (cursor.Take(SqlTokenKind.LeftBrace))
		{
			int? low = null, high = null;

			Unsigned(ref cursor, out low);

			if (cursor.Take(SqlTokenKind.Comma))
			{
				Unsigned(ref cursor, out high);

				if (cursor.Take(SqlTokenKind.RightBrace))
					return new RowPatternQuantifier(RowPatternQuantifierKind.Range, low, high, cursor.Take(SqlTokenKind.Question));
			}
			else if (low is not null && cursor.Take(SqlTokenKind.RightBrace))
			{
				return new RowPatternQuantifier(RowPatternQuantifierKind.Exact, low, low);
			}

			cursor = save;
		}

		return null;
	}
}
