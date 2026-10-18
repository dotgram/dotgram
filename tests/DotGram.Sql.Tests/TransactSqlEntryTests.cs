using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Sql.Ast;
using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// <c>TransactSqlParser</c>'s entries hand back the common tree the grammar's rules build: every text
/// <see cref="TransactSqlTests"/> puts to the parser is answered by the plain door as by the located one — read where it
/// reads, refused where it refuses, at the same place — and what it reads is the same tree, spans aside.
/// </summary>
public sealed class TransactSqlEntryTests(ITestOutputHelper output)
{
	/// <summary>Every string the theories of <see cref="TransactSqlTests"/> are given first, once each.</summary>
	static IEnumerable<string> Texts()
	{
		var seen = new HashSet<string>(StringComparer.Ordinal);

		foreach (var method in typeof(TransactSqlTests).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).OrderBy(static one => one.Name, StringComparer.Ordinal))
			foreach (var row in method.GetCustomAttributes<InlineDataAttribute>())
				if (row.Data is [string input, ..] && seen.Add(input))
					yield return input;
	}

	/// <summary>
	/// A statement: the same answer, the same place a refusal stops at, and the same tree through either door; a statement
	/// the tree has no place for is T-SQL's extension.
	/// </summary>
	[Fact]
	public void Every_statement_reads_through_the_entry_as_the_grammar_reads_it()
	{
		var faults = new List<string>();
		var read   = 0;

		foreach (var input in Texts())
		{
			var located = TransactSqlParser.Located.TryParseStatement(input);
			var now     = TransactSqlParser.TryParseStatement(input);

			if (located.IsSuccess != now.IsSuccess)
			{
				faults.Add($"{(located.IsSuccess ? "read located and refused plainly" : "refused located and read plainly")}: {input}");

				continue;
			}

			if (!located.IsSuccess)
			{
				if (located.Position != now.Position)
					faults.Add($"refused at {now.Position} where the located door stops at {located.Position}: {input}");

				continue;
			}

			read++;

			if (now.Value is Ast.Statement.Extension { Dialect: not "T-SQL" })
				faults.Add($"an extension of another dialect: {input}");
			else if (!located.Value.Equals(now.Value))
				faults.Add($"the located door gives another tree: {input}");
		}

		output.WriteLine($"{read} read");

		foreach (var fault in faults)
			output.WriteLine(fault);

		Assert.True(read > 1_000, $"{read} statements read");
		Assert.Equal([], faults);
	}

	/// <summary>
	/// An expression and a search condition: the same answer and the same place through either door, and the same tree.
	/// </summary>
	[Theory]
	[InlineData("dbo.f(@v) + [b]")]
	[InlineData("a b")]
	[InlineData("CAST (a AS int) COLLATE Latin1_General_CI_AS")]
	[InlineData("1 + 2 * 3")]
	[InlineData("(SELECT TOP (1) a AS x FROM t)")]
	public void An_expression_reads_through_the_entry_as_the_grammar_reads_it(string input)
	{
		var located = TransactSqlParser.Located.TryParseExpression(input);
		var now = TransactSqlParser.TryParseExpression(input);

		Assert.Equal(located.IsSuccess, now.IsSuccess);

		if (!located.IsSuccess)
		{
			Assert.Equal(located.Position, now.Position);

			return;
		}

		Assert.Equal(TransactSqlParser.Located.ParseExpression(input), now.Value);
		Assert.Equal(now.Value, TransactSqlParser.Located.ParseExpression(input));
	}

	[Theory]
	[InlineData("[a] > dbo.f(@v)")]
	[InlineData("a = 1 AND b IN (SELECT c AS d FROM t)")]
	[InlineData("a =")]
	public void A_condition_reads_through_the_entry_as_the_grammar_reads_it(string input)
	{
		var located = TransactSqlParser.Located.TryParseSearchCondition(input);
		var now = TransactSqlParser.TryParseSearchCondition(input);

		Assert.Equal(located.IsSuccess, now.IsSuccess);

		if (!located.IsSuccess)
		{
			Assert.Equal(located.Position, now.Position);

			return;
		}

		Assert.Equal(TransactSqlParser.Located.ParseSearchCondition(input), now.Value);
		Assert.Equal(now.Value, TransactSqlParser.Located.ParseSearchCondition(input));
	}

	/// <summary>
	/// A word the rules read from the text is read through either door: both read with locations, so <c>SELECT a b</c>
	/// keeps its alias without <c>AS</c>.
	/// </summary>
	[Theory]
	[InlineData("SELECT a b FROM t", false)]
	[InlineData("SELECT a AS b FROM t", true)]
	public void A_word_read_from_the_text_is_read_through_either_door(string input, bool said)
	{
		foreach (var statement in (Ast.Statement[])[TransactSqlParser.ParseStatement(input), TransactSqlParser.Located.ParseStatement(input), TransactSqlParser.ParseSql(input)[0]])
		{
			var query  = (Ast.Query.Select)((Ast.Statement.Select)statement).Query;
			var column = (Ast.SelectColumn.ExpressionItem)query.Columns[0];

			Assert.Equal(said, column.AsKeyword);
		}
	}

	/// <summary>
	/// The located door says where a statement and what it holds were written, in the input handed in; the plain door
	/// says nothing.
	/// </summary>
	[Fact]
	public void The_located_door_says_where_a_statement_was_written()
	{
		const string text = "SELECT 1; UPDATE t SET a = 2 WHERE b = 3";

		var located = TransactSqlParser.Located.ParseSql(text);
		var plain   = TransactSqlParser.ParseSql(text);

		Assert.Equal(plain, located);
		Assert.Equal("UPDATE t SET a = 2 WHERE b = 3", text.Substring(located[1].Span.At, located[1].Span.Length));
		Assert.False(plain[1].Span.Known);

		var update = (Ast.Statement.Update)located[1];

		Assert.Equal("b = 3", text.Substring(update.Where!.Span.At, update.Where.Span.Length));
	}

	/// <summary>
	/// A construct the tree has no place for yet reads, as an extension that names it, and is never thrown out of an
	/// entry: the first such statement among the tests'.
	/// </summary>
	[Fact]
	public void A_statement_the_tree_cannot_hold_reads_as_an_extension()
	{
		var text = Unplaced();

		var read = TransactSqlParser.TryParseStatement(text);

		Assert.True(read.IsSuccess, text);

		var extension = Assert.IsType<Ast.Statement.Extension>(read.Value);

		Assert.Equal("T-SQL", extension.Dialect);
		Assert.Equal(extension, TransactSqlParser.Located.ParseStatement(text));
		Assert.IsType<Ast.Statement.Extension>(Assert.Single(TransactSqlParser.ParseSql(text)));
		Assert.ThrowsAny<NotSupportedException>(() => Ast.TransactSqlWriter.Write(extension));
	}

	/// <summary>
	/// What stands for a construct the tree has no place for is located where the grammar located the construct: a
	/// statement alone and among others, and an expression, through the located door; through the plain door nowhere.
	/// </summary>
	[Fact]
	public void An_extension_is_located_where_the_grammar_located_what_it_replaces()
	{
		const string statement = "SELECT c1 FROM t1 WITH (0)";
		const string script    = "SELECT 1;\n  SELECT c1 FROM t1 WITH (0)";
		const string value     = "JSON_OBJECTAGG()";

		var alone = Assert.IsType<Ast.Statement.Extension>(TransactSqlParser.Located.ParseStatement(statement));

		Assert.True(alone.Span.Known);
		Assert.Equal(statement, statement.Substring(alone.Span.At, alone.Span.Length));
		Assert.False(TransactSqlParser.ParseStatement(statement).Span.Known);

		var among = Assert.IsType<Ast.Statement.Extension>(TransactSqlParser.Located.ParseSql(script)[1]);

		Assert.Equal(statement, script.Substring(among.Span.At, among.Span.Length));
		Assert.False(TransactSqlParser.ParseSql(script)[1].Span.Known);

		var expression = Assert.IsType<Ast.Expression.Extension>(TransactSqlParser.Located.ParseExpression(value));

		Assert.Equal(value, value.Substring(expression.Span.At, expression.Span.Length));
		Assert.False(TransactSqlParser.ParseExpression(value).Span.Known);
	}

	/// <summary>The first statement of the tests the tree has no place for.</summary>
	static string Unplaced()
	{
		foreach (var input in Texts())
			if (TransactSqlParser.TryParseStatement(input) is { IsSuccess: true, Value: Ast.Statement.Extension })
				return input;

		throw new InvalidOperationException("Every statement of the tests is held by the tree: this test has nothing left to hold.");
	}

	/// <summary>Every statement of the common tree says what it does.</summary>
	[Fact]
	public void Every_statement_of_the_common_tree_has_a_category()
	{
		var faults = new List<string>();

		foreach (var type in typeof(Ast.Statement).GetNestedTypes(BindingFlags.Public).Where(static one => !one.IsAbstract && one.IsSubclassOf(typeof(Ast.Statement))))
		{
			try
			{
				((Ast.Statement)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type)).Category();
			}
			catch (NotSupportedException failure)
			{
				faults.Add(failure.Message);
			}
		}

		Assert.Equal([], faults);
	}
}
