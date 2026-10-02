using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Sql;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// <c>TransactSqlParser.Located</c>: the one parser asked to say where every value was written,
/// and where the text in front of it began.
/// </summary>
/// <remarks>
/// The located methods are the class's own, called with locations on, so they must answer what
/// the plain ones answer and build what they build; what they add is the span, kept out of the
/// tree's equality, the start of the gap in front of each node, and where the reading stopped.
/// </remarks>
public sealed class LocatedReadingTests
{
	/// <summary>A located tree and a plain one of the same text are equal, and hash alike.</summary>
	[Theory]
	[InlineData("a + b * 2", false)]
	[InlineData("(a + 1) * -b % 3 - @v", false)]
	[InlineData("a BETWEEN 1 AND 2 AND NOT b LIKE 'c%'", true)]
	public void A_located_tree_equals_the_plain_tree(string text, bool condition)
	{
		var plain   = condition ? TransactSqlParser.ParseSearchCondition(text) : TransactSqlParser.ParseValueExpression(text);
		var located = condition ? TransactSqlParser.Located.ParseSearchCondition(text) : TransactSqlParser.Located.ParseValueExpression(text);

		Assert.False(plain.Span.Known);
		Assert.True(located.Span.Known);
		Assert.Equal(plain, located);
		Assert.Equal(plain.GetHashCode(), located.GetHashCode());
	}

	/// <summary>Spacing and comments between the parts change the spans and not the tree.</summary>
	[Fact]
	public void Two_located_trees_that_differ_only_in_trivia_are_equal()
	{
		var tight = TransactSqlParser.Located.ParseValueExpression("a+b*2");
		var loose = TransactSqlParser.Located.ParseValueExpression("a  +  /* two */ b * -- c\n 2");

		Assert.NotEqual(tight.Span, loose.Span);
		Assert.Equal(tight, loose);
	}

	/// <summary>A plain reading writes no span at all, as before.</summary>
	[Fact]
	public void A_plain_reading_locates_nothing()
	{
		var statement = TransactSqlParser.ParseStatement("SELECT a, b FROM t WHERE a = 1");

		Assert.True(SqlWalker.Walk(statement, static node => !node.Span.Known && node.Span.GapStart == 0));
	}

	/// <summary>The gap in front of a node runs from the end of the token before it.</summary>
	[Fact]
	public void A_node_says_where_the_text_in_front_of_it_begins()
	{
		var text      = "SELECT a, /* about b */\n  b FROM t";
		var statement = TransactSqlParser.Located.ParseStatement(text);
		var b         = Nodes(statement).OfType<Expression.ColumnReference>().Single(node => node.Span.At == text.IndexOf("b FROM", StringComparison.Ordinal));

		Assert.Equal(text.IndexOf(',') + 1, b.Span.GapStart);
		Assert.Equal(" /* about b */\n  ", text.Substring(b.Span.GapStart, b.Span.At - b.Span.GapStart));
	}

	/// <summary>The first node of a reading has its gap from where the reading began.</summary>
	[Fact]
	public void The_first_node_of_a_window_has_its_gap_from_the_window()
	{
		var text  = "SELECT 1\n-- second\nSELECT 2";
		var at    = text.IndexOf('\n');
		var match = TransactSqlParser.Located.TryParseSql(text, at, text.Length - at);

		Assert.True(match.IsSuccess);
		Assert.Equal(at, match.Value.Single().Span.GapStart);
		Assert.Equal(text.IndexOf("SELECT 2", StringComparison.Ordinal), match.Value.Single().Span.At);

		var whole = TransactSqlParser.Located.ParseSql(text);

		Assert.Equal(0, whole[0].Span.GapStart);
		Assert.Equal(text.IndexOf('1') + 1, whole[1].Span.GapStart);
	}

	/// <summary>
	/// A match says where the reading stopped looking: the end of the input, of a window read to
	/// its end, or of what was read from a position.
	/// </summary>
	[Fact]
	public void A_match_says_where_the_reading_stopped_looking()
	{
		var text = "SELECT 1; -- end\n";

		Assert.Equal(text.Length, TransactSqlParser.Located.TryParseSql(text).ReadingEnd);
		Assert.Equal(text.Length, TransactSqlParser.TryParseSql(text).ReadingEnd);
		Assert.Equal(text.Length - 3, TransactSqlParser.Located.TryParseSql(text, 0, text.Length - 3).ReadingEnd);

		var from = TransactSqlParser.Located.TryParseStatement(text, 0);

		Assert.Equal(from.Position + from.Length, from.ReadingEnd);

		var batch = SqlScript.Read("SELECT 1\nGO\n-- only a comment\n").Batches.Last();

		Assert.Equal(batch.At + batch.Length, TransactSqlParser.Located.TryParseSql(batch).ReadingEnd);
	}

	/// <summary>
	/// The nodes the package rebuilds with <c>with</c> while reading keep the spans of the text
	/// they were read from: the predicate completed from its left operand, and the negated forms.
	/// </summary>
	/// <remarks>
	/// A span is the range of the rule that built the value: the rule that hands the condition
	/// back adds its <c>WHERE</c> to nothing, so the condition begins at its operand.
	/// </remarks>
	[Theory]
	[InlineData("SELECT 1 WHERE a = 1", typeof(Expression.Comparison))]
	[InlineData("SELECT 1 WHERE a BETWEEN 1 AND 2", typeof(Expression.Between))]
	[InlineData("SELECT 1 WHERE a NOT BETWEEN 1 AND 2", typeof(Expression.Between))]
	[InlineData("SELECT 1 WHERE a IN (1, 2)", typeof(Expression.In))]
	[InlineData("SELECT 1 WHERE a NOT LIKE 'x%'", typeof(Expression.Like))]
	[InlineData("SELECT 1 WHERE a IS NOT NULL", typeof(Expression.IsNull))]
	[InlineData("SELECT 1 WHERE a > ALL (SELECT b FROM t)", typeof(Expression.Quantified))]
	public void A_predicate_completed_with_with_keeps_its_spans(string text, Type predicate)
	{
		var statement = TransactSqlParser.Located.ParseStatement(text);
		var where     = text.IndexOf("WHERE", StringComparison.Ordinal);
		var node      = Nodes(statement).Single(node => node.GetType() == predicate);
		var operand   = Nodes(node).OfType<Expression.ColumnReference>().First();

		var at        = where + "WHERE ".Length;

		Assert.False(node.Span.IsStale);
		Assert.Equal(new SqlSpan(at, text.Length - at, where + "WHERE".Length), node.Span);
		Assert.False(operand.Span.IsStale);
		Assert.Equal(new SqlSpan(at, 1, where + "WHERE".Length), operand.Span);
	}

	/// <summary>Every clause that hands back what it reads leaves its keyword outside the value's span.</summary>
	[Theory]
	[InlineData("SELECT a FROM t WHERE a = 1", "a = 1")]
	[InlineData("SELECT a FROM t GROUP BY a HAVING COUNT(*) > 1", "COUNT(*) > 1")]
	[InlineData("UPDATE t SET a = 1 WHERE b = 2", "b = 2")]
	[InlineData("DELETE FROM t WHERE b = 2", "b = 2")]
	[InlineData("SELECT a FROM t JOIN u ON t.a = u.a", "t.a = u.a")]
	[InlineData("IF 1 = 1 SELECT 1 ELSE SELECT 2", "SELECT 2")]
	public void A_condition_is_spanned_without_its_keyword(string text, string condition)
	{
		var statement = TransactSqlParser.Located.ParseStatement(text);
		var at        = text.LastIndexOf(condition, StringComparison.Ordinal);

		Assert.Contains(Nodes(statement), node => node.Span.At == at && node.Span.Length == condition.Length);
		Assert.DoesNotContain(Nodes(statement), node => node is Expression or Statement && node.Span.End == text.Length && node.Span.At < at && node.Span.At > 0);
	}

	/// <summary>
	/// A member chain and a zone, which the package closes with <c>with</c> while reading, are told the
	/// whole range they stand on and are not left stale.
	/// </summary>
	[Fact]
	public void A_chain_closed_while_reading_spans_all_of_it_and_is_fresh()
	{
		var text      = "SELECT (c1).P1.P2, d AT TIME ZONE 'UTC'";
		var statement = TransactSqlParser.Located.ParseStatement(text);
		var chain     = text.IndexOf("(c1)", StringComparison.Ordinal);
		var members   = Nodes(statement).OfType<Expression.Member>().ToList();

		Assert.DoesNotContain(Nodes(statement), static node => node.Span.IsStale);
		Assert.Equal(2, members.Count);
		Assert.Contains(members, node => node.Span.At == chain && node.Span.Length == "(c1).P1".Length);
		Assert.Contains(members, node => node.Span.At == chain && node.Span.Length == "(c1).P1.P2".Length);
		Assert.Contains(Nodes(statement).OfType<Expression.RoutineInvocation>(), node => text.Substring(node.Span.At, node.Span.Length) == "d AT TIME ZONE 'UTC'");
	}

	/// <summary>
	/// The words that stand for a value of their own — <c>NULL</c>, <c>DEFAULT</c> — are a node per
	/// place they are written, so each has its own span, also when readings run at once.
	/// </summary>
	[Fact]
	public void A_null_is_a_node_of_its_own_in_every_reading()
	{
		var first  = TransactSqlParser.Located.ParseStatement("UPDATE t SET a = NULL");
		var second = TransactSqlParser.Located.ParseStatement("UPDATE t SET a = 1, bb = NULL");
		var one    = Nodes(first).OfType<Expression.Literal>().Single(static node => node.Kind == SqlLiteralKind.Null);
		var two    = Nodes(second).OfType<Expression.Literal>().Single(static node => node.Kind == SqlLiteralKind.Null);

		Assert.NotSame(one, two);
		Assert.Equal(17, one.Span.At);
		Assert.Equal(25, two.Span.At);

		System.Threading.Tasks.Parallel.For(0, 64, i =>
		{
			var text    = new string(' ', i) + "UPDATE t SET a = NULL";
			var literal = Nodes(TransactSqlParser.Located.ParseStatement(text)).OfType<Expression.Literal>().Single();

			Assert.Equal(i + 17, literal.Span.At);
		});

		Assert.Equal(17, one.Span.At);
	}

	/// <summary>A window, named or written out, is spanned without the <c>OVER</c> in front of it.</summary>
	[Theory]
	[InlineData("SELECT SUM(x) OVER w FROM t WINDOW w AS (ORDER BY x)", "w", "OVER")]
	[InlineData("SELECT SUM(x) OVER (ORDER BY x) FROM t", "ORDER BY x", "OVER (")]
	public void A_window_is_spanned_without_its_over(string text, string window, string before)
	{
		var statement = TransactSqlParser.Located.ParseStatement(text);
		var gap       = text.IndexOf(before, StringComparison.Ordinal) + before.Length;
		var spanned   = Nodes(statement).OfType<Clause.Window>().First(node => node.Span.GapStart == gap);

		Assert.Equal(window, text.Substring(spanned.Span.At, spanned.Span.Length));
		Assert.DoesNotContain(Nodes(statement).OfType<Clause.Window>(), node => text.Substring(node.Span.At, node.Span.Length).StartsWith("OVER", StringComparison.Ordinal));
	}

	/// <summary>
	/// A window the lexer stopped short in was not read to its end, and a match says so: through
	/// either door, the reading stopped looking where the tokens stopped.
	/// </summary>
	[Fact]
	public void A_window_cut_short_by_the_lexer_says_where_it_stopped()
	{
		var text    = "SELECT 1 \uFFFF and more";
		var plain   = TransactSqlParser.TryParseSql(text, 0, text.Length);
		var located = TransactSqlParser.Located.TryParseSql(text, 0, text.Length);

		Assert.True(located.IsSuccess);
		Assert.Equal("SELECT 1".Length, located.ReadingEnd);
		Assert.Equal(located.ReadingEnd, plain.ReadingEnd);

		var whole = TransactSqlParser.Located.TryParseSql(text, 0, "SELECT 1 ".Length);

		Assert.Equal("SELECT 1 ".Length, whole.ReadingEnd);
	}

	/// <summary>
	/// A batch in which a variable was substituted is read in its own text: spans, gaps and where the
	/// reading stopped are positions in <see cref="ScriptBatch.Text"/>, which holds the substitution.
	/// </summary>
	[Fact]
	public void A_substituted_batch_is_located_in_its_own_text()
	{
		var script = ":setvar table Orders\nSELECT a FROM $(table) /* t */ WHERE b = 1 -- end\nGO\n";
		var batch  = SqlScript.Read(script).Batches.First(static one => !one.IsVerbatim);
		var match  = TransactSqlParser.Located.TryParseSql(batch);

		Assert.True(match.IsSuccess);
		Assert.Equal(batch.At + batch.Length, match.ReadingEnd);

		var text      = batch.Text;
		var statement = Assert.Single(match.Value);
		var table     = Nodes(statement).OfType<TableReference.Named>().Single();
		var condition = Nodes(statement).OfType<Expression.Comparison>().Single();

		Assert.Equal("Orders", text.Substring(table.Span.At, table.Span.Length));
		Assert.Equal("b = 1", text.Substring(condition.Span.At, condition.Span.Length));
		Assert.Equal(" ", text.Substring(condition.Span.GapStart, condition.Span.At - condition.Span.GapStart));
		Assert.Equal(" /* t */ ", text.Substring(table.Span.End, text.IndexOf("WHERE", table.Span.End, StringComparison.Ordinal) - table.Span.End));
		Assert.Contains("-- end", text.Substring(statement.Span.End, (int)match.ReadingEnd - statement.Span.End), StringComparison.Ordinal);
		Assert.True(Trivia(text.Substring(statement.Span.End, (int)match.ReadingEnd - statement.Span.End)));
	}

	/// <summary>
	/// A span is written as it was when it was a positional record: named arguments, an object
	/// initializer, <c>with</c> and deconstruction all still compile and mean what they meant.
	/// </summary>
	[Fact]
	public void A_span_is_written_as_it_always_was()
	{
		var span = new SqlSpan(At: 1, Length: 2);
		var (at, length) = span;

		Assert.Equal((1, 2), (at, length));
		Assert.Equal(1, span.GapStart);
		Assert.Equal(new SqlSpan(1, 3), span with { Length = 3 });
		Assert.Equal(7, new SqlSpan { At = 3, Length = 4 }.End);
		Assert.Equal(5, new SqlSpan(At: 6, Length: 1, GapStart: 5).GapStart);
	}

	/// <summary>A backup's tail is put on the statement with <c>with</c>, and the statement keeps its span.</summary>
	[Theory]
	[InlineData("BACKUP DATABASE d TO DISK = 'x' WITH INIT")]
	[InlineData("RESTORE DATABASE d FROM DISK = 'x' WITH REPLACE")]
	public void A_statement_given_its_tail_with_with_keeps_its_span(string text)
	{
		var statement = TransactSqlParser.Located.ParseStatement(text);

		Assert.Equal(new SqlSpan(0, text.Length, 0), statement.Span);
	}

	/// <summary>
	/// A copy made with <c>with</c> keeps where its original was written and the gap in front of it,
	/// and says it is stale; the original does not change, and an unlocated node's copy is unlocated.
	/// </summary>
	[Fact]
	public void A_copy_keeps_the_span_and_is_stale()
	{
		var text     = "SELECT 1 WHERE  a = 1";
		var original = (Expression.Comparison)Nodes(TransactSqlParser.Located.ParseStatement(text)).OfType<Expression.Comparison>().Single();
		var copy     = original with { Left = original.Right };

		Assert.False(original.Span.IsStale);
		Assert.True(copy.Span.IsStale);
		Assert.Equal(original.Span.At, copy.Span.At);
		Assert.Equal(original.Span.Length, copy.Span.Length);
		Assert.Equal(original.Span.GapStart, copy.Span.GapStart);

		var plain = (Expression.Comparison)Nodes(TransactSqlParser.ParseStatement(text)).OfType<Expression.Comparison>().Single();

		Assert.False((plain with { Left = plain.Right }).Span.IsStale);
	}

	/// <summary>
	/// The records that hold a span declare nothing else, which is what lets their hand-written copy
	/// constructors copy only it: a field added beside it would be dropped by every <c>with</c>.
	/// </summary>
	[Fact]
	public void A_record_that_holds_a_span_holds_nothing_else()
	{
		var holders = typeof(Statement).Assembly.GetTypes()
			.Where(static type => type.Namespace == "DotGram.Sql" && type.IsAbstract && !type.IsInterface && type.GetProperty(nameof(ISqlSpan.Span))?.DeclaringType == type &&
				typeof(ISqlSpan).IsAssignableFrom(type))
			.ToList();

		Assert.Equal(6, holders.Count);

		foreach (var holder in holders)
			Assert.Equal(
				["_location"],
				holder.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Select(static field => field.Name));
	}

	/// <summary>
	/// Over the corpus the located reading answers as the plain one does at every form — the whole
	/// text, from a position, in a window and through the script reader — and builds the same trees;
	/// every node's gap holds only trivia, and after the last statement of a reading there is only
	/// trivia and <c>;</c> up to where the reading stopped looking. No node of a fresh reading is stale.
	/// </summary>
	[Fact]
	public void The_located_reading_agrees_with_the_plain_one_over_the_corpus()
	{
		var root  = Path.GetFullPath(Path.Combine(Here(), "..", "Corpus", "ScriptDom"));
		var files = Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories);
		var gaps  = 0;
		var ends  = 0;

		Assert.True(files.Length > 1000, root);

		foreach (var file in files)
		{
			var text = File.ReadAllText(file);

			Assert.Equal(TransactSqlParser.TryParseScript(text).IsSuccess, TransactSqlParser.Located.TryParseScript(text).IsSuccess);

			Agree(TransactSqlParser.TryParseSql(text), TransactSqlParser.Located.TryParseSql(text), text, 0);

			for (int at = 0, guard = 0; at < text.Length && guard < 10_000; guard++)
			{
				var plain   = TransactSqlParser.TryParseStatement(text, at);
				var located = TransactSqlParser.Located.TryParseStatement(text, at);

				Assert.Equal(plain.IsSuccess, located.IsSuccess);
				Assert.Equal(plain.Position, located.Position);
				Assert.Equal(plain.Length, located.Length);

				if (!plain.IsSuccess || plain.Position + plain.Length <= at)
					break;

				gaps += Gaps(located.Value, text, 0);
				at    = (int)(plain.Position + plain.Length);
			}

			foreach (var batch in SqlScript.Read(text).Batches)
			{
				Agree(
					TransactSqlParser.TryParseSql(batch.Text, batch.At, batch.Length),
					TransactSqlParser.Located.TryParseSql(batch.Text, batch.At, batch.Length),
					batch.Text, batch.At);
				Agree(TransactSqlParser.TryParseSql(batch), TransactSqlParser.Located.TryParseSql(batch), batch.Text, batch.At);
			}
		}

		Assert.True(gaps > 100_000, $"{gaps} gaps");
		Assert.True(ends > 4_000, $"{ends} reading ends");

		void Agree(TransactSqlParser.Match<Statement[]> plain, TransactSqlParser.Match<Statement[]> located, string text, int floor)
		{
			Assert.Equal(plain.IsSuccess, located.IsSuccess);
			Assert.Equal(plain.Position, located.Position);
			Assert.Equal(plain.Length, located.Length);
			Assert.Equal(plain.ReadingEnd, located.ReadingEnd);

			if (!plain.IsSuccess)
				return;

			Assert.Equal(plain.Value.Length, located.Value.Length);

			for (var i = 0; i < plain.Value.Length; i++)
			{
				Assert.Equal(Shape(plain.Value[i]), Shape(located.Value[i]));
				gaps += Gaps(located.Value[i], text, floor);
			}

			// What a reading leaves after its last statement, where it read to the end it looked to.
			var last = located.Value.Length == 0 ? floor : located.Value[^1].Span.End;

			if (located.ReadingEnd == located.Position + located.Length && located.Value.Length > 0)
				return;

			ends++;

			foreach (var piece in text.Substring(last, (int)located.ReadingEnd - last).Split(';'))
				Assert.True(Trivia(piece), $"'{piece}' after the last statement");
		}
	}

	/// <summary>Every located node's gap, which must run from no earlier than the reading to its start, over trivia.</summary>
	static int Gaps(Statement statement, string text, int floor)
	{
		var count = 0;
		var seen  = new HashSet<ISqlSpan>(ReferenceEqualityComparer.Instance);

		SqlWalker.Walk(statement, node =>
		{
			var span = node.Span;

			// A node told where it was written is this reading's own: one object shared between
			// two places, or with another reading, would have one span written over the other.
			Assert.True(seen.Add(node), $"{node.GetType().Name} at {span.At} is reached twice");

			if (!span.Known && span.GapStart == 0)
				return true;

			Assert.InRange(span.GapStart, floor, span.At);
			Assert.False(span.IsStale, $"{node.GetType().Name} at {span.At} is stale in a fresh reading");
			Assert.True(Trivia(text.Substring(span.GapStart, span.At - span.GapStart)), $"gap of {node.GetType().Name} at {span.At}");
			count++;

			return true;
		});

		return count;
	}

	/// <summary>What a tree says, without where: every node's type and every value it holds.</summary>
	static string Shape(Statement statement)
	{
		var shape = new System.Text.StringBuilder();

		SqlWalker.Walk(statement, node =>
		{
			shape.Append(node.GetType().FullName).Append('{');

			foreach (var property in node.GetType().GetProperties())
				if (property.Name is not ("Span" or "EqualityContract") && property.GetIndexParameters().Length == 0 &&
					property.GetValue(node) is { } value && (value is string || value.GetType().IsPrimitive || value.GetType().IsEnum))
					shape.Append(property.Name).Append('=').Append(value).Append(';');

			shape.Append('}');

			return true;
		});

		return shape.ToString();
	}

	/// <summary>Spacing and comments, line and nested block, and nothing else.</summary>
	static bool Trivia(string text)
	{
		for (var i = 0; i < text.Length;)
		{
			if (char.IsWhiteSpace(text[i]) || text[i] < ' ' || text[i] == '​')
			{
				i++;
			}
			else if (text[i] == '-' && i + 1 < text.Length && text[i + 1] == '-')
			{
				while (i < text.Length && text[i] != '\n')
					i++;
			}
			else if (text[i] == '/' && i + 1 < text.Length && text[i + 1] == '*')
			{
				var depth = 0;

				do
				{
					if (text[i] == '/' && i + 1 < text.Length && text[i + 1] == '*')
					{
						depth++;
						i += 2;
					}
					else if (text[i] == '*' && i + 1 < text.Length && text[i + 1] == '/')
					{
						depth--;
						i += 2;
					}
					else
					{
						i++;
					}
				}
				while (depth > 0 && i < text.Length);
			}
			else
			{
				return false;
			}
		}

		return true;
	}

	static List<ISqlSpan> Nodes(ISqlSpan root)
	{
		var nodes = new List<ISqlSpan>();

		SqlWalker.Walk(root, node =>
		{
			nodes.Add(node);

			return true;
		});

		return nodes;
	}

	static string Here([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.GetDirectoryName(here)!;
	}
}
