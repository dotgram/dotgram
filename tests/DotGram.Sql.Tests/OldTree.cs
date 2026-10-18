using System;
using System.Collections.Generic;

using DotGram.Sql.TransactSql;

namespace DotGram.Sql.Tests;

/// <summary>
/// The T-SQL tree the grammar's rules still build (<c>SqlSyntax.cs</c>), read through the parser's internal entries,
/// under the names its public entries had when they returned it.
/// </summary>
/// <remarks>
/// <para>
/// <c>TransactSqlParser</c> hands back the common tree, carried across from this one by the bridge. What the tests
/// hold of the grammar itself — the records its actions build, their spans, the old writer and walker — and the
/// bridge's own round trip read this tree, until the rules that build it build the common tree themselves. Each reading is
/// carried with its text, as the public entries carry theirs, since a rule that builds the common tree already reads from
/// it (<c>Statement.Built</c>); and like theirs, the plain readings of statements read with locations, which what is read
/// from the text needs, and only the located ones hand the common tree's nodes their spans. A test of the grammar's own
/// plain reading reads through the internal entry itself.
/// </para>
/// <para>
/// A query is read as the query statement that holds it, and a level as no level: a test that asks a level asks
/// <c>TransactSqlParser</c>.
/// </para>
/// </remarks>
static class OldTree
{
	public static Statement ParseStatement(string input)
	{
		using var reading = Bridge.Over(input, false, count: true);

		return TransactSqlParser.Located.ParseSyntaxStatement(input);
	}

	public static TransactSqlParser.Match<Statement> TryParseStatement(string input)
	{
		using var reading = Bridge.Over(input, false, count: true);

		return TransactSqlParser.Located.TryParseSyntaxStatement(input);
	}

	public static TransactSqlParser.Match<Statement> TryParseStatement(string input, int position)
	{
		using var reading = Bridge.Over(input, false, count: true);

		return TransactSqlParser.Located.TryParseSyntaxStatement(input, position);
	}

	public static Statement ParseSelect(string input)
	{
		using var reading = Bridge.Over(input, false, count: true);

		return TransactSqlParser.Located.ParseSyntaxSelect(input);
	}

	public static TransactSqlParser.Match<Statement> TryParseSelect(string input)
	{
		using var reading = Bridge.Over(input, false, count: true);

		return TransactSqlParser.Located.TryParseSyntaxSelect(input);
	}

	public static TransactSqlParser.Match<Query> TryParseQuery(string input)
	{
		using var reading = Bridge.Over(input, false, count: true);

		return Query(TransactSqlParser.Located.TryParseSyntaxStatement(input));
	}

	public static Statement[] ParseSql(string input)
	{
		using var reading = Bridge.Over(input, false, count: true);

		return TransactSqlParser.Located.ParseSyntaxSql(input);
	}

	public static TransactSqlParser.Match<Statement[]> TryParseSql(string input)
	{
		using var reading = Bridge.Over(input, false, count: true);

		return TransactSqlParser.Located.TryParseSyntaxSql(input);
	}

	public static TransactSqlParser.Match<Statement[]> TryParseSql(string input, int position, int length)
	{
		using var reading = Bridge.Over(input, false, count: true);

		return TransactSqlParser.Located.TryParseSyntaxSql(input, position, length);
	}

	/// <summary>
	/// A batch of a script, read through its window as <c>TransactSqlParser.TryParseSql(ScriptBatch)</c> reads it: its
	/// spans are positions in <see cref="ScriptBatch.Text"/>, and a reading that stops short of the batch's end is refused.
	/// </summary>
	public static TransactSqlParser.Match<Statement[]> TryParseSql(ScriptBatch batch)
	{
		using var reading = Bridge.Over(batch.Text, false, count: true);

		return Whole(batch, TransactSqlParser.Located.TryParseSyntaxSql(batch.Text, batch.At, batch.Length));
	}

	public static Expression ParseValueExpression(string input)
	{
		using var reading = Bridge.Over(input, false, count: true);

		return TransactSqlParser.ParseSyntaxExpression(input);
	}

	public static TransactSqlParser.Match<Expression> TryParseValueExpression(string input)
	{
		using var reading = Bridge.Over(input, false, count: true);

		return TransactSqlParser.TryParseSyntaxExpression(input);
	}

	public static Expression ParseSearchCondition(string input)
	{
		using var reading = Bridge.Over(input, false, count: true);

		return TransactSqlParser.ParseSyntaxCondition(input);
	}

	public static TransactSqlParser.Match<Expression> TryParseSearchCondition(string input)
	{
		using var reading = Bridge.Over(input, false, count: true);

		return TransactSqlParser.TryParseSyntaxCondition(input);
	}

	/// <summary>
	/// The record of this tree a statement's rules read, where they read it into one and built the common tree from it
	/// (<c>Statement.Built</c>); any other statement is its own.
	/// </summary>
	public static Statement Record(Statement statement)
	{
		return statement is Statement.Built { Given: Statement read } ? read : statement;
	}

	/// <summary>
	/// What a query statement's rule handed its builder: the query, its clauses and the WITH in front of it, as the rules
	/// read them.
	/// </summary>
	public static Bridge.Queries.Selecting Selecting(Statement statement)
	{
		return (Bridge.Queries.Selecting)((Statement.Built)statement).Given;
	}

	/// <summary>The query a query statement holds, as its rule built it: a specification's keeps its select list and condition.</summary>
	public static DotGram.Sql.Query.Built QueryOf(Statement statement)
	{
		return (DotGram.Sql.Query.Built)Selecting(statement).Query;
	}

	/// <summary>
	/// Hands every node under <paramref name="root"/> to <paramref name="visit"/>, as <see cref="SqlWalker"/> does, and
	/// with them what a query statement or a query whose rule builds the common tree keeps of the parts its rules read: the
	/// statement's query and clauses, a specification's select list and condition, a table value constructor's rows.
	/// </summary>
	public static bool Walk(ISqlSpan root, Func<ISqlSpan, bool> visit)
	{
		return SqlWalker.Walk(root, node =>
		{
			if (!visit(node))
				return false;

			foreach (var part in Kept(node))
				if (!Walk(part, visit))
					return false;

			return true;
		});
	}

	/// <summary>Every node <see cref="Walk"/> hands out, in its order.</summary>
	public static List<ISqlSpan> Nodes(ISqlSpan root)
	{
		var nodes = new List<ISqlSpan>();

		Walk(root, node =>
		{
			nodes.Add(node);

			return true;
		});

		return nodes;
	}

	static IEnumerable<ISqlSpan> Kept(ISqlSpan node)
	{
		switch (node)
		{
			case Statement.Built { Given: Bridge.Queries.Selecting selecting }:
				foreach (var one in selecting.With)
					yield return one;

				yield return selecting.Query;

				if (selecting.By is { } by)
					yield return by;

				if (selecting.Shape is { } shape)
					yield return shape;

				foreach (var one in selecting.Options)
					yield return one;

				break;

			case DotGram.Sql.Query.Built query:
				foreach (var one in query.Columns)
					yield return one;

				if (query.Where is { } where)
					yield return where;

				foreach (var one in query.Rows)
					yield return one;

				break;
		}
	}

	static TransactSqlParser.Match<Statement[]> Whole(ScriptBatch batch, TransactSqlParser.Match<Statement[]> read)
	{
		return !read.IsSuccess || read.ReadingEnd == batch.At + batch.Length
			? read
			: TransactSqlParser.Match<Statement[]>.Failed(TransactSqlParser.Outcome.NoMatch, "Input does not match 'Sql'.", read.ReadingEnd, null, null);
	}

	/// <summary>The query a statement holds, where it is a query statement.</summary>
	static TransactSqlParser.Match<Query> Query(TransactSqlParser.Match<Statement> read)
	{
		if (!read.IsSuccess)
			return read.Refusal<Query>(read.Position, "Input does not match 'Query'.");

		return read.Value is Statement.Built { Given: Bridge.Queries.Selecting { Query: var query } }
			? TransactSqlParser.Match<Query>.Success(query, read.Position, read.Length, read.ReadingEnd)
			: TransactSqlParser.Match<Query>.Failed(TransactSqlParser.Outcome.NoMatch, "Input does not match 'Query'.", read.Position, null, null);
	}

	/// <summary>The same, read with locations.</summary>
	public static class Located
	{
		public static Statement ParseStatement(string input)
		{
			using var reading = Bridge.Over(input, true, count: true);

			return TransactSqlParser.Located.ParseSyntaxStatement(input);
		}

		public static TransactSqlParser.Match<Statement> TryParseStatement(string input)
		{
			using var reading = Bridge.Over(input, true, count: true);

			return TransactSqlParser.Located.TryParseSyntaxStatement(input);
		}

		public static TransactSqlParser.Match<Statement> TryParseStatement(string input, int position)
		{
			using var reading = Bridge.Over(input, true, count: true);

			return TransactSqlParser.Located.TryParseSyntaxStatement(input, position);
		}

		public static TransactSqlParser.Match<Statement> TryParseSelect(string input)
		{
			using var reading = Bridge.Over(input, true, count: true);

			return TransactSqlParser.Located.TryParseSyntaxSelect(input);
		}

		public static TransactSqlParser.Match<Query> TryParseQuery(string input)
		{
			using var reading = Bridge.Over(input, true, count: true);

			return Query(TransactSqlParser.Located.TryParseSyntaxStatement(input));
		}

		public static Statement[] ParseSql(string input)
		{
			using var reading = Bridge.Over(input, true, count: true);

			return TransactSqlParser.Located.ParseSyntaxSql(input);
		}

		public static TransactSqlParser.Match<Statement[]> TryParseSql(string input)
		{
			using var reading = Bridge.Over(input, true, count: true);

			return TransactSqlParser.Located.TryParseSyntaxSql(input);
		}

		public static TransactSqlParser.Match<Statement[]> TryParseSql(string input, int position, int length)
		{
			using var reading = Bridge.Over(input, true, count: true);

			return TransactSqlParser.Located.TryParseSyntaxSql(input, position, length);
		}

		/// <summary>A batch of a script, as <see cref="OldTree.TryParseSql(ScriptBatch)"/> reads it.</summary>
		public static TransactSqlParser.Match<Statement[]> TryParseSql(ScriptBatch batch)
		{
			using var reading = Bridge.Over(batch.Text, true, count: true);

			return Whole(batch, TransactSqlParser.Located.TryParseSyntaxSql(batch.Text, batch.At, batch.Length));
		}

		public static Expression ParseValueExpression(string input)
		{
			using var reading = Bridge.Over(input, true, count: true);

			return TransactSqlParser.Located.ParseSyntaxExpression(input);
		}

		public static Expression ParseSearchCondition(string input)
		{
			using var reading = Bridge.Over(input, true, count: true);

			return TransactSqlParser.Located.ParseSyntaxCondition(input);
		}
	}
}
