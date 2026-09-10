using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Parsers;
using DotGram.Parsers.Sql;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// <see cref="SqlWalker"/>: every node of a tree, each put to one question.
/// </summary>
/// <remarks>
/// A check is not a method of its own. It is a lambda that matches the nodes it is about and
/// says whether to go on, and the walker is the one thing that knows what a node holds.
/// </remarks>
public sealed class SqlWalkerTests
{
	[Fact]
	public void Every_node_is_visited_the_root_first()
	{
		var statement = TransactSql.ParseStatement("SELECT a + 1 FROM t WHERE b = 2");
		var seen      = new List<ISqlSpan>();

		Assert.True(SqlWalker.Walk(statement, node =>
		{
			seen.Add(node);

			return true;
		}));

		Assert.Same(statement, seen[0]);
		Assert.Contains(seen, static node => node is Expression.Add);
		Assert.Contains(seen, static node => node is Expression.Comparison);
		Assert.Contains(seen, static node => node is TableReference.Named);
	}

	[Fact]
	public void A_visit_that_answers_false_ends_the_walk()
	{
		var statement = TransactSql.ParseStatement("SELECT a + 1 FROM t WHERE b = 2");
		var all       = 0;
		var seen      = 0;

		SqlWalker.Walk(statement, _ => ++all > 0);

		Assert.False(SqlWalker.Walk(statement, node => ++seen > 0 && node is not Expression.Add));
		Assert.True(seen < all);
	}

	/// <summary>An option written twice in one list, which is semantics and not syntax.</summary>
	/// <remarks>
	/// The engine answers it as either — a repeated `SUBJECT` is `Incorrect syntax`, a repeated
	/// `SORT_IN_TEMPDB` is read and objected to later — and neither answer is the parser's to
	/// give, since the parser reads what may be written. So it is a check, and a check is a
	/// lambda over the nodes it is about. `DATA_COMPRESSION` twice over different partitions is
	/// how the syntax says it, and is not a repetition.
	/// </remarks>
	[Theory]
	[InlineData("CREATE INDEX i ON t (a) WITH (SORT_IN_TEMPDB = ON, SORT_IN_TEMPDB = OFF)", "SORT_IN_TEMPDB")]
	[InlineData("CREATE INDEX i ON t (a) WITH (DATA_COMPRESSION = ROW ON PARTITIONS (1), DATA_COMPRESSION = PAGE ON PARTITIONS (2))", "")]
	[InlineData("CREATE TABLE t (a INT, INDEX ix (a) WITH (FILLFACTOR = 80, PAD_INDEX = ON, FILLFACTOR = 90))", "FILLFACTOR")]
	public void A_check_is_a_lambda_over_the_nodes_it_is_about(string input, string repeated)
	{
		var found = new List<string>();

		SqlWalker.Walk(TransactSql.ParseStatement(input), node =>
		{
			if (node is Clause.ConstraintDefinition { Options: { } options })
				found.AddRange(
					from option in options.OfType<Clause.Option>()
					group option by (Name: option.Name.ToUpperInvariant(), option.Partitions) into same
					where same.Count() > 1
					select same.Key.Name);

			return true;
		});

		Assert.Equal(repeated, string.Join(",", found));
	}
}
