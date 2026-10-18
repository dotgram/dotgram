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
/// <c>TransactSqlParser</c>'s entries hand back the common tree, carried across by the bridge from the T-SQL tree the
/// grammar's rules build: every text <see cref="TransactSqlTests"/> puts to the parser is answered by the entry as the
/// grammar's own reading answers it — read where it reads, refused where it refuses — and what it reads is that
/// reading carried across, through the plain door and the located one alike.
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
	/// A statement: the same answer, the same place a refusal stops at, and the carried tree; the located door's tree
	/// equals the plain one's.
	/// </summary>
	[Fact]
	public void Every_statement_reads_through_the_entry_as_the_grammar_reads_it()
	{
		var faults = new List<string>();
		var read   = 0;

		foreach (var input in Texts())
		{
			var old = OldTree.TryParseStatement(input);
			var now = TransactSqlParser.TryParseStatement(input);

			if (old.IsSuccess != now.IsSuccess)
			{
				faults.Add($"{(old.IsSuccess ? "read by the grammar and refused by the entry" : "refused by the grammar and read by the entry")}: {input}");

				continue;
			}

			if (!old.IsSuccess)
			{
				if (old.Position != now.Position)
					faults.Add($"refused at {now.Position} where the grammar stops at {old.Position}: {input}");

				continue;
			}

			read++;

			Ast.Statement carried;

			try
			{
				carried = Bridge.ToAst(OldTree.Located.ParseStatement(input), input);
			}
			catch (BridgeGap)
			{
				if (now.Value is not Ast.Statement.Extension { Dialect: "T-SQL" })
					faults.Add($"a gap of the bridge, and not an extension: {input}");

				continue;
			}

			var located = TransactSqlParser.Located.TryParseStatement(input);

			if (!carried.Equals(now.Value))
				faults.Add($"not the carried tree: {input}: {Trees.Difference(carried, now.Value, old: false)}");
			else if (!located.IsSuccess || !located.Value.Equals(now.Value))
				faults.Add($"the located door gives another tree: {input}");
		}

		output.WriteLine($"{read} read");

		foreach (var fault in faults)
			output.WriteLine(fault);

		Assert.True(read > 1_000, $"{read} statements read");
		Assert.Equal([], faults);
	}

	/// <summary>
	/// An expression and a search condition: the same answer and the same place as the grammar's reading of the same
	/// rule, and the carried tree.
	/// </summary>
	[Theory]
	[InlineData("dbo.f(@v) + [b]")]
	[InlineData("a b")]
	[InlineData("CAST (a AS int) COLLATE Latin1_General_CI_AS")]
	[InlineData("1 + 2 * 3")]
	[InlineData("(SELECT TOP (1) a AS x FROM t)")]
	public void An_expression_reads_through_the_entry_as_the_grammar_reads_it(string input)
	{
		var old = OldTree.TryParseValueExpression(input);
		var now = TransactSqlParser.TryParseExpression(input);

		Assert.Equal(old.IsSuccess, now.IsSuccess);

		if (!old.IsSuccess)
		{
			Assert.Equal(old.Position, now.Position);

			return;
		}

		Assert.Equal(Bridge.ToAst(OldTree.Located.ParseValueExpression(input), input), now.Value);
		Assert.Equal(now.Value, TransactSqlParser.Located.ParseExpression(input));
	}

	[Theory]
	[InlineData("[a] > dbo.f(@v)")]
	[InlineData("a = 1 AND b IN (SELECT c AS d FROM t)")]
	[InlineData("a =")]
	public void A_condition_reads_through_the_entry_as_the_grammar_reads_it(string input)
	{
		var old = OldTree.TryParseSearchCondition(input);
		var now = TransactSqlParser.TryParseSearchCondition(input);

		Assert.Equal(old.IsSuccess, now.IsSuccess);

		if (!old.IsSuccess)
		{
			Assert.Equal(old.Position, now.Position);

			return;
		}

		Assert.Equal(Bridge.ToAst(OldTree.Located.ParseSearchCondition(input), input), now.Value);
		Assert.Equal(now.Value, TransactSqlParser.Located.ParseSearchCondition(input));
	}

	/// <summary>
	/// A word the T-SQL tree does not keep is read from the text through either door: the plain door reads the text
	/// again with locations for it, so <c>SELECT a b</c> keeps its alias without <c>AS</c>.
	/// </summary>
	[Theory]
	[InlineData("SELECT a b FROM t", false)]
	[InlineData("SELECT a AS b FROM t", true)]
	public void A_word_the_old_tree_drops_is_read_through_either_door(string input, bool said)
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
	/// A construct the bridge has no place for yet reads, as an extension that names it, and is never thrown out of an
	/// entry: the first such statement among the tests'.
	/// </summary>
	[Fact]
	public void A_statement_the_bridge_cannot_carry_reads_as_an_extension()
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

	/// <summary>The first statement of the tests the bridge has no place for.</summary>
	static string Unplaced()
	{
		foreach (var (old, source) in BridgeOracle.TestStatements())
		{
			try
			{
				Bridge.ToAst(old, source);
			}
			catch (BridgeGap)
			{
				return BridgeOracle.Text(old, source);
			}
		}

		throw new InvalidOperationException("Every statement of the tests is carried: this test has nothing left to hold.");
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

	/// <summary>
	/// And says what the T-SQL tree's record said, for every statement of the corpus and of the tests that the bridge
	/// carries.
	/// </summary>
	[Fact]
	public void A_statement_says_the_category_the_old_tree_said()
	{
		var faults = new SortedSet<string>(StringComparer.Ordinal);

		foreach (var (old, source) in BridgeOracle.CorpusStatements().Concat(BridgeOracle.TestStatements()))
		{
			Ast.Statement made;

			try
			{
				made = Bridge.ToAst(old, source);
			}
			catch (BridgeGap)
			{
				continue;
			}

			if (made.Category() != old.Category)
				faults.Add($"{Bridge.NameOf(old)} is {old.Category}, and Statement.{made.GetType().Name} {made.Category()}");
		}

		foreach (var fault in faults)
			output.WriteLine(fault);

		Assert.Equal([], faults);
	}
}
