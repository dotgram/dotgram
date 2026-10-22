using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Sql.Ast;
using DotGram.Sql.Standard;

using Xunit;

namespace DotGram.Sql.Tests;

// Inside the namespace, where they are asked before DotGram.Sql's own: the tree T-SQL builds.
using DataType = DotGram.Sql.Ast.DataType;
using Expression = DotGram.Sql.Ast.Expression;

/// <summary>
/// <see cref="QualifiedName"/> is a value of its first part and an array of the others: what a value
/// type lets slip — a name of no parts, a span or a location lost on a copy, an optional name no longer
/// null — held here.
/// </summary>
public sealed class QualifiedNameTests
{
	static readonly Identifier S = new("s");
	static readonly Identifier T = new("t");

	[Fact]
	public void A_name_reads_its_parts_without_making_a_list()
	{
		var one = new QualifiedName(T);
		var two = new QualifiedName(S, [T]);

		Assert.Equal(1, one.Count);
		Assert.Same(T, one[0]);
		Assert.Same(T, one.First);
		Assert.Same(T, one.Last);

		Assert.Equal(2, two.Count);
		Assert.Same(S, two.First);
		Assert.Same(T, two.Last);
		Assert.Throws<ArgumentOutOfRangeException>(() => two[2]);
		Assert.Throws<ArgumentOutOfRangeException>(() => two[-1]);

		var walked = new List<Identifier>();

		foreach (var part in two)
			walked.Add(part);

		Assert.Equal(new[] { S, T }, walked);
		Assert.Equal(new[] { S, T }, two.Parts);
		Assert.True(two is [{ Text: "s" }, { Text: "t" }]);
	}

	[Fact]
	public void Every_way_of_making_a_name_makes_the_same_name()
	{
		var made = new QualifiedName(S, [T]);

		Assert.Equal(made, QualifiedName.Of(S, T));
		Assert.Equal(made, new QualifiedName([S, T]));
		Assert.Equal(new QualifiedName(T), new QualifiedName(T, []));
		Assert.Equal(new QualifiedName(T), new QualifiedName(T, null));
		Assert.NotEqual(new QualifiedName(T), made);
		Assert.Throws<ArgumentException>(() => new QualifiedName(default(SqlList<Identifier>)));
		Assert.Throws<ArgumentNullException>(() => new QualifiedName(null!));
	}

	/// <summary>Where a name was written is no part of what it says: equal names hash alike wherever they stand.</summary>
	[Fact]
	public void Where_a_name_was_written_is_left_out_of_equality()
	{
		var here  = new QualifiedName(new Identifier("s"), [new Identifier("t")]);
		var there = new QualifiedName(new Identifier("s"), [new Identifier("t")]);

		here.First.Locate(0, 1);
		here.Last.Locate(2, 1);
		there.First.Locate(10, 1);
		there.Last.Locate(14, 1);

		Assert.Equal(new SqlSpan(0, 3), here.Span);
		Assert.Equal(new SqlSpan(10, 5), there.Span);
		Assert.Equal(here, there);
		Assert.Equal(here.GetHashCode(), there.GetHashCode());
		Assert.Equal(new Expression.Reference(here), new Expression.Reference(there));
	}

	/// <summary>
	/// A name keeps no span, so a copy cannot lose one: a name copied out of the record that holds it,
	/// or boxed by the walker, spans its parts as the one in the record does.
	/// </summary>
	[Fact]
	public void A_copy_of_a_name_spans_what_the_name_does()
	{
		var reference = new Expression.Reference(new QualifiedName(new Identifier("s"), [new Identifier("t")]));

		reference.Name.First.Locate(4, 1);
		reference.Name.Last.Locate(6, 1);

		var copy = reference.Name;
		ISqlNode boxed = reference.Name;

		boxed.Locate(0, 100);

		Assert.Equal(new SqlSpan(4, 3), reference.Name.Span);
		Assert.Equal(new SqlSpan(4, 3), copy.Span);
		Assert.Equal(new SqlSpan(4, 3), boxed.Span);
	}

	/// <summary><c>default(QualifiedName)</c> is no name: nothing writes it and nothing walks it.</summary>
	[Fact]
	public void A_name_of_no_parts_is_refused()
	{
		QualifiedName none = default;

		Assert.Equal(0, none.Count);
		Assert.Null(none.First);
		Assert.True(none.Parts.IsEmpty);
		Assert.False(none.Span.Known);

		var reference = new Expression.Reference(none);

		Assert.Throws<ArgumentException>(() => SqlWriter.Write(reference));
		Assert.Throws<ArgumentException>(() => SqlWriter.Write(new LiteralValue.String("'abc'", StringLiteralKind.Character, new CharacterSetName(none))));
		Assert.Throws<InvalidOperationException>(() => SqlWalker.Walk(reference, static _ => true));
	}

	/// <summary>A name the text left out is null, as it was when a name was a reference; one written is there.</summary>
	[Fact]
	public void A_name_left_out_is_null()
	{
		var bare   = Assert.IsType<DataType.Reference>(SqlStandardParser.ParseDataType("REF(t)"));
		var scoped = Assert.IsType<DataType.Reference>(SqlStandardParser.ParseDataType("REF(t) SCOPE s.u"));

		Assert.Null(bare.Scope);
		Assert.True(bare is { Scope: null });
		Assert.True(scoped is { Scope: [{ Text: "s" }, { Text: "u" }] });
		Assert.Equal("REF(t)", SqlWriter.Write(bare));
		Assert.Equal("REF(t) SCOPE s.u", SqlWriter.Write(scoped));
	}

	/// <summary>The walker hands out each part of a name once, after the name, in order.</summary>
	[Fact]
	public void The_walker_hands_out_each_part_once()
	{
		var statement = SqlStandardParser.ParseStatement("UPDATE c.s.t SET a = x.y WHERE z = 1");
		var visited   = new List<ISqlSpan>();

		SqlWalker.Walk(statement, node =>
		{
			visited.Add(node);

			return true;
		});

		var parts = visited.OfType<Identifier>().Select(static part => part.Text).ToList();

		Assert.Equal(new[] { "c", "s", "t", "a", "x", "y", "z" }, parts);
		Assert.Equal(visited.OfType<Identifier>().Count(), visited.OfType<Identifier>().Distinct(ReferenceEqualityComparer.Instance).Count());

		var name = visited.IndexOf(visited.OfType<QualifiedName>().First());

		Assert.Equal("c", ((Identifier)visited[name + 1]).Text);
	}
}
