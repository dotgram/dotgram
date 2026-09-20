using System.Collections.Generic;

using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The code the package's own pages show, run rather than read.
/// </summary>
/// <remarks>
/// Copied from the fences of <c>README.md</c> and <c>SKILL.md</c> as they stand, call for call,
/// rather than written again to the same meaning: a page that leaves out what its example needs
/// reads as if it works, and only a literal copy finds that out. Where a copy does not run, the
/// page is what is wrong.
/// </remarks>
public sealed class ShippedExampleTests
{
	/// <summary>The README's opening example, to its last line.</summary>
	[Fact]
	public void The_readme_example_runs()
	{
		var match = TransactSqlParser.TryParseSelect("select name from Users where id > @id");

		var select = (Statement.Select)match.Value;
		var query  = (Query.Specification)select.Of;

		Assert.Equal("Users", Assert.IsType<TableReference.Named>(query.From[0]).Table);
	}

	/// <summary>The SKILL's first example: the match, the writer, and the walk that collects tables.</summary>
	[Fact]
	public void The_skill_example_runs()
	{
		var match = TransactSqlParser.TryParseStatement("select name from Users where id > @id");

		Assert.True(match.IsSuccess);                       // the page returns here instead

		var select = (Statement.Select)match.Value;
		string text = SqlWriter.Write(select);              // SELECT name FROM Users WHERE id > @id

		var tables = new List<string>();
		SqlWalker.Walk(select, node =>
		{
			if (node is TableReference.Named named)
				tables.Add(named.Table);                    // Users
			return true;                                    // false stops the walk
		});

		Assert.Equal("SELECT name FROM Users WHERE id > @id", text);
		Assert.Equal(["Users"], tables);
	}

	/// <summary>The SKILL's second example: the standard's own parser, its writer, and what it refuses.</summary>
	[Fact]
	public void The_skill_standard_example_runs()
	{
		Ast.Expression e = SqlStandardParser.ParseValueExpression("a + b * 2");
		string again     = Ast.Sql2023Writer.Write(e);     // a + b * 2

		bool standard = SqlStandardParser.TryParseQueryExpression("SELECT TOP 1 a FROM t").IsSuccess;  // false

		Assert.Equal("a + b * 2", again);
		Assert.False(standard);
	}
}
