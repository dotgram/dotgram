using System;

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
/// it (<c>Statement.Built</c>).
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
		using var reading = Bridge.Over(input, false);

		return TransactSqlParser.ParseSyntaxStatement(input);
	}

	public static TransactSqlParser.Match<Statement> TryParseStatement(string input)
	{
		using var reading = Bridge.Over(input, false);

		return TransactSqlParser.TryParseSyntaxStatement(input);
	}

	public static TransactSqlParser.Match<Statement> TryParseStatement(string input, int position)
	{
		using var reading = Bridge.Over(input, false);

		return TransactSqlParser.TryParseSyntaxStatement(input, position);
	}

	public static Statement ParseSelect(string input)
	{
		using var reading = Bridge.Over(input, false);

		return TransactSqlParser.ParseSyntaxSelect(input);
	}

	public static TransactSqlParser.Match<Statement> TryParseSelect(string input)
	{
		using var reading = Bridge.Over(input, false);

		return TransactSqlParser.TryParseSyntaxSelect(input);
	}

	public static TransactSqlParser.Match<Query> TryParseQuery(string input)
	{
		using var reading = Bridge.Over(input, false);

		return Query(TransactSqlParser.TryParseSyntaxStatement(input));
	}

	public static Statement[] ParseSql(string input)
	{
		using var reading = Bridge.Over(input, false);

		return TransactSqlParser.ParseSyntaxSql(input);
	}

	public static TransactSqlParser.Match<Statement[]> TryParseSql(string input)
	{
		using var reading = Bridge.Over(input, false);

		return TransactSqlParser.TryParseSyntaxSql(input);
	}

	public static TransactSqlParser.Match<Statement[]> TryParseSql(string input, int position, int length)
	{
		using var reading = Bridge.Over(input, false);

		return TransactSqlParser.TryParseSyntaxSql(input, position, length);
	}

	/// <summary>
	/// A batch of a script, read through its window as <c>TransactSqlParser.TryParseSql(ScriptBatch)</c> reads it: its
	/// spans are positions in <see cref="ScriptBatch.Text"/>, and a reading that stops short of the batch's end is refused.
	/// </summary>
	public static TransactSqlParser.Match<Statement[]> TryParseSql(ScriptBatch batch)
	{
		using var reading = Bridge.Over(batch.Text, false);

		return Whole(batch, TransactSqlParser.TryParseSyntaxSql(batch.Text, batch.At, batch.Length));
	}

	public static Expression ParseValueExpression(string input)
	{
		using var reading = Bridge.Over(input, false);

		return TransactSqlParser.ParseSyntaxExpression(input);
	}

	public static TransactSqlParser.Match<Expression> TryParseValueExpression(string input)
	{
		using var reading = Bridge.Over(input, false);

		return TransactSqlParser.TryParseSyntaxExpression(input);
	}

	public static Expression ParseSearchCondition(string input)
	{
		using var reading = Bridge.Over(input, false);

		return TransactSqlParser.ParseSyntaxCondition(input);
	}

	public static TransactSqlParser.Match<Expression> TryParseSearchCondition(string input)
	{
		using var reading = Bridge.Over(input, false);

		return TransactSqlParser.TryParseSyntaxCondition(input);
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

		return read.Value is Statement.Select { Of: var query }
			? TransactSqlParser.Match<Query>.Success(query, read.Position, read.Length, read.ReadingEnd)
			: TransactSqlParser.Match<Query>.Failed(TransactSqlParser.Outcome.NoMatch, "Input does not match 'Query'.", read.Position, null, null);
	}

	/// <summary>The same, read with locations.</summary>
	public static class Located
	{
		public static Statement ParseStatement(string input)
		{
			using var reading = Bridge.Over(input, true);

			return TransactSqlParser.Located.ParseSyntaxStatement(input);
		}

		public static TransactSqlParser.Match<Statement> TryParseStatement(string input)
		{
			using var reading = Bridge.Over(input, true);

			return TransactSqlParser.Located.TryParseSyntaxStatement(input);
		}

		public static TransactSqlParser.Match<Statement> TryParseStatement(string input, int position)
		{
			using var reading = Bridge.Over(input, true);

			return TransactSqlParser.Located.TryParseSyntaxStatement(input, position);
		}

		public static TransactSqlParser.Match<Statement> TryParseSelect(string input)
		{
			using var reading = Bridge.Over(input, true);

			return TransactSqlParser.Located.TryParseSyntaxSelect(input);
		}

		public static TransactSqlParser.Match<Query> TryParseQuery(string input)
		{
			using var reading = Bridge.Over(input, true);

			return Query(TransactSqlParser.Located.TryParseSyntaxStatement(input));
		}

		public static Statement[] ParseSql(string input)
		{
			using var reading = Bridge.Over(input, true);

			return TransactSqlParser.Located.ParseSyntaxSql(input);
		}

		public static TransactSqlParser.Match<Statement[]> TryParseSql(string input)
		{
			using var reading = Bridge.Over(input, true);

			return TransactSqlParser.Located.TryParseSyntaxSql(input);
		}

		public static TransactSqlParser.Match<Statement[]> TryParseSql(string input, int position, int length)
		{
			using var reading = Bridge.Over(input, true);

			return TransactSqlParser.Located.TryParseSyntaxSql(input, position, length);
		}

		/// <summary>A batch of a script, as <see cref="OldTree.TryParseSql(ScriptBatch)"/> reads it.</summary>
		public static TransactSqlParser.Match<Statement[]> TryParseSql(ScriptBatch batch)
		{
			using var reading = Bridge.Over(batch.Text, true);

			return Whole(batch, TransactSqlParser.Located.TryParseSyntaxSql(batch.Text, batch.At, batch.Length));
		}

		public static Expression ParseValueExpression(string input)
		{
			using var reading = Bridge.Over(input, true);

			return TransactSqlParser.Located.ParseSyntaxExpression(input);
		}

		public static Expression ParseSearchCondition(string input)
		{
			using var reading = Bridge.Over(input, true);

			return TransactSqlParser.Located.ParseSyntaxCondition(input);
		}
	}
}
