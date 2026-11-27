using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Xunit;

namespace DotGram.QueryLanguage.Tests;

/// <summary>
/// Values that read no row, in a list of <c>in</c>, as the receiver of a member and as the argument of an aggregate:
/// the language binds them, translates them as they stand and leaves what a provider cannot compute to the provider.
/// </summary>
[Collection(nameof(Databases))]
public sealed class ValuesThatReadNoRowTests(Databases databases)
{
	static readonly QlModel<Shop> Model = Shop.Model();

	static readonly Order[] Mine = [.. Shop.Rows().Where(o => o.TenantId == 1)];

	static IEnumerable<object?> Ids(Func<Order, bool> where)
	{
		return Mine.Where(where).Select(o => (object?)o.Id);
	}

	List<object?[]> Run(string provider, string text, object? values = null)
	{
		return Rows.Run(Model, databases.Get(provider), text, values);
	}

	static List<Expression> Nodes(string text)
	{
		var collector = new Collector();

		collector.Visit(Model.QueryData(Shop.InMemory(), text, null).Expression);

		return collector.Found;
	}

	sealed class Collector : ExpressionVisitor
	{
		public List<Expression> Found { get; } = [];

		public override Expression? Visit(Expression? node)
		{
			if (node is not null)
				Found.Add(node);

			return base.Visit(node);
		}
	}

	// ── Binding ─────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("from orders where customer in ('c1', status) select id")]
	[InlineData("from orders where customer in (@a, @b) select id")]
	[InlineData("from orders where qty in (1, qty + 1, id) select id")]
	[InlineData("from orders let k = 'c1' where customer in (k, 'c2') select id")]
	[InlineData("from orders where 'ABC'.ToLower() == 'abc' select id")]
	[InlineData("from orders where customer == ' c1 '.Trim() select id")]
	[InlineData("from orders where 'abc'.Length == 3 select id")]
	[InlineData("from orders where 'abc'.Contains(customer) select id")]
	[InlineData("from orders let k = 'ABC' where k.ToLower() == 'abc' select id")]
	[InlineData("from orders select Sum(1) as s")]
	[InlineData("from orders select Max(1 + 2) as m, Min('x') as n")]
	[InlineData("from orders let k = 2 select Sum(k) as s, Count(k) as c, Avg(k) as a")]
	public void ValuesThatReadNoRowBind(string text)
	{
		Assert.NotEmpty(Model.Describe(text).Columns);
	}

	[Fact]
	public void ThePartsOfTheResultKeepTheirTypes()
	{
		var aggregates = Model.Describe("from orders select Sum(1) as s, Avg(2) as a, Max('x') as m");

		Assert.Equal([(QlType.Int, true), (QlType.Double, true), (QlType.String, true)], aggregates.Columns.Select(c => (c.Type, c.IsNullable)));

		var members = Model.Describe("from orders select 'ABC'.ToLower() as l, 'abc'.Length as n");

		Assert.Equal([(QlType.String, false), (QlType.Int, false)], members.Columns.Select(c => (c.Type, c.IsNullable)));
	}

	/// <summary>A parameter has the type of what it meets; a member or an aggregate of it meets nothing, so its type stays unknown.</summary>
	[Theory]
	[InlineData("from orders where @p.ToUpper() == 'C1' select id", 18)]
	[InlineData("from orders select Count(@p) as c", 25)]
	[InlineData("from orders select Count(null) as c", 25)]
	public void AValueWithoutATypeIsStillATypeError(string text, int position)
	{
		var error = Assert.Throws<QlException>(() => Model.Describe(text)).Error;

		Assert.Equal((QlErrorCode.Type, (int?)position), (error.Code, error.Position));
	}

	// ── Tree shape ──────────────────────────────────────────────────────────

	[Fact]
	public void AnInListHoldsTheValuesAsTheyAre()
	{
		var array = Nodes("from orders where customer in ('c1', status) select id").OfType<NewArrayExpression>().First();

		Assert.Equal(2, array.Expressions.Count);
		Assert.IsAssignableFrom<MemberExpression>(array.Expressions[1]);
	}

	[Fact]
	public void AMemberOfAValueThatReadsNoRowIsACallOnIt()
	{
		var call = Assert.Single(Nodes("from orders where customer == ' c1 '.Trim() select id").OfType<MethodCallExpression>(), c => c.Method.Name == nameof(string.Trim));

		Assert.Equal(" c1 ", Assert.IsType<ConstantExpression>(call.Object).Value);
	}

	[Fact]
	public void AnAggregateOfAValueThatReadsNoRowTakesItAsItsSelector()
	{
		var sum = Assert.Single(Nodes("from orders select Sum(1) as s").OfType<MethodCallExpression>(), c => c.Method.Name == nameof(Enumerable.Sum) || c.Method.Name == nameof(Queryable.Sum));

		Assert.Equal(2, sum.Arguments.Count);
	}

	// ── On the databases ────────────────────────────────────────────────────

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void AnInListMayHoldAnyValueOfTheType(string provider)
	{
		Assert.Equal(Ids(o => o.Customer == "c1" || o.Customer == o.Status),                  Run(provider, "from orders where customer in ('c1', status) order by id select id").Select(r => r[0]));
		Assert.Equal(Ids(o => o.Customer is "c1" or "c2"),                                     Run(provider, "from orders where customer in (@a, @b) order by id select id", new { a = "c1", b = "c2" }).Select(r => r[0]));
		Assert.Equal(Ids(o => o.Qty == 1 || o.Qty == o.Qty + 1 || o.Qty == o.Id),              Run(provider, "from orders where qty in (1, qty + 1, id) order by id select id").Select(r => r[0]));
		Assert.Equal(Ids(o => o.Customer is "c1" or "c2"),                                     Run(provider, "from orders let k = 'c1' where customer in (k, 'c2') order by id select id").Select(r => r[0]));
		Assert.Equal(Ids(o => o.Customer != "c1" && o.Customer != "c2"),                       Run(provider, "from orders where customer not in (@a, 'c2') order by id select id", new { a = "c1" }).Select(r => r[0]));
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void AMemberOfAValueThatReadsNoRowIsComputedByTheProvider(string provider)
	{
		Assert.Equal(Ids(_ => true),               Run(provider, "from orders where 'ABC'.ToLower() == 'abc' order by id select id").Select(r => r[0]));
		Assert.Equal(Ids(o => o.Customer == "c1"), Run(provider, "from orders where customer == ' c1 '.Trim() order by id select id").Select(r => r[0]));
		Assert.Equal(Ids(_ => true),               Run(provider, "from orders where 'abc'.Length == 3 order by id select id").Select(r => r[0]));
		Assert.Equal(Ids(_ => true),               Run(provider, "from orders where 'abc'.Contains('b') order by id select id").Select(r => r[0]));
		Assert.Equal(Ids(_ => false),              Run(provider, "from orders where 'abc'.Contains(customer) order by id select id").Select(r => r[0]));
		Assert.Equal(Ids(o => o.Customer == "c1"), Run(provider, "from orders let k = ' C1 ' where customer == k.Trim().ToLower() order by id select id").Select(r => r[0]));
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void AnAggregateOfAValueThatReadsNoRowIsComputedByTheProvider(string provider)
	{
		var rows = Run(provider, "from orders select Sum(1) as s, Count(1) as c, Max(1 + 2) as m, Min(5) as n");

		Assert.Equal([20, 20, 3, 5], Assert.Single(rows).Select(Convert.ToInt32));

		var named = Run(provider, "from orders let k = 2 select Sum(k) as s, Count(k) as c");

		Assert.Equal([40, 20], Assert.Single(named).Select(Convert.ToInt32));
		Assert.Equal("x", Assert.Single(Run(provider, "from orders select Max('x') as m"))[0]);
	}
}
