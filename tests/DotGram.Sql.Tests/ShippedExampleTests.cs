using System;
using System.Collections.Generic;

using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using DotGram.Tests;

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

		var from = (TableReference.Named)query.From[0];   // from.Table is "Users"

		Assert.Equal("Users", from.Table);
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

	/// <summary>
	/// And every block of both pages compiles as the page writes it: on its own, with the usings it
	/// and the blocks before it name, and no implicit ones — which is what a consumer aiming at
	/// netstandard2.0 has (D56).
	/// </summary>
	[Theory]
	[MemberData(nameof(Pages))]
	public void Every_block_of_a_page_compiles_as_it_is_written(string page, int block)
	{
		// The package's own assembly is among the references only once something has loaded it.
		_ = typeof(SqlWriter).Assembly.FullName;

		var blocks = ShippedPages.Blocks(page);
		var code   = ShippedPages.Inherited(blocks, block) + blocks[block];

		var said = ShippedPages.Compiles(code);

		Assert.True(said is null, said + Environment.NewLine + "----" + Environment.NewLine + code);
	}

	public static TheoryData<string, int> Pages => ShippedPages.Every(
		ShippedPages.PageOf(typeof(SqlWriter), "README.md"),
		ShippedPages.PageOf(typeof(SqlWriter), "SKILL.md"));

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
