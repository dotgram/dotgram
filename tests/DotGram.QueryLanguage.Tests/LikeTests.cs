using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using DotGram.QueryLanguage.Syntax;

using Xunit;

namespace DotGram.QueryLanguage.Tests;

/// <summary>
/// <c>like</c> and <c>not like</c>: read at the level of a comparison, bound over strings of the rows, and translated
/// as the body of the host's own LIKE with the two strings in place of its parameters.
/// </summary>
[Collection(nameof(Databases))]
public sealed class LikeTests(Databases databases)
{
	static readonly QlModel<Shop> Model = Shop.Model();

	static readonly Order[] Mine = [.. Shop.Rows().Where(o => o.TenantId == 1)];

	static ExprSyntax Condition(string text)
	{
		return ((WhereClause)Reader.Read(text, QlOptions.Default).Clauses[0]).Condition;
	}

	static QlError Refused(string text, QlModel<Shop>? model = null)
	{
		return Assert.Throws<QlException>(() => (model ?? Model).Describe(text)).Error;
	}

	// ── The text ────────────────────────────────────────────────────────────

	[Fact]
	public void LikeAndNotLikeRead()
	{
		var like = Assert.IsType<LikeExpr>(Condition("from t where a like 'x%'"));

		Assert.False(like.Negated);
		Assert.Equal("a", ((NameExpr)like.Operand).Name.Text);
		Assert.Equal("'x%'", ((LiteralExpr)like.Pattern).Text);
		Assert.Equal(15, like.Position);

		var not = Assert.IsType<LikeExpr>(Condition("from t where a NOT LIKE @p"));

		Assert.True(not.Negated);
		Assert.Equal("p", ((ParameterExpr)not.Pattern).Name);
	}

	/// <summary>Like `==`: tighter than `&amp;&amp;` and `||`, looser than `+`, and one to a predicate.</summary>
	[Fact]
	public void LikeBindsAsAComparison()
	{
		var added = Assert.IsType<LikeExpr>(Condition("from t where a like b + c"));

		Assert.Equal(BinaryOperator.Add, ((BinaryExpr)added.Pattern).Operator);

		var and = Assert.IsType<LogicalExpr>(Condition("from t where a like 'x%' && b > 1 || c not like 'y'"));

		Assert.Equal(LogicalOperator.Or, and.Operator);
		Assert.IsType<LikeExpr>(((LogicalExpr)and.Items[0]).Items[0]);
		Assert.True(Assert.IsType<LikeExpr>(and.Items[1]).Negated);

		var member = Assert.IsType<LikeExpr>(Condition("from t where a.ToLower() like b.Trim()"));

		Assert.IsType<MemberExpr>(member.Operand);
		Assert.IsType<MemberExpr>(member.Pattern);

		Assert.IsType<UnaryExpr>(Condition("from t where !(a like 'x')"));
		Assert.IsType<LikeExpr>(Condition("from t where like like 'x%'"));
	}

	[Theory]
	[InlineData("from t where a like b like c",  22)]
	[InlineData("from t where a like",           19)]
	[InlineData("from t where a not",            18)]
	[InlineData("from t where a like 'x' == b",  24)]
	public void ALikeIsOnePredicate(string text, int position)
	{
		var error = Assert.Throws<QlException>(() => Reader.Read(text, QlOptions.Default)).Error;

		Assert.Equal((QlErrorCode.Syntax, (int?)position), (error.Code, error.Position));
	}

	/// <summary>C# would read <c>!a like b</c> as <c>(!a) like b</c> and SQL as <c>NOT (a LIKE b)</c>: brackets say which.</summary>
	[Theory]
	[InlineData("from t where !a like 'x'")]
	[InlineData("from t where !a not like 'x'")]
	public void NotWithoutBracketsBeforeLikeIsRefused(string text)
	{
		var error = Assert.Throws<QlException>(() => Reader.Read(text, QlOptions.Default)).Error;

		Assert.Equal((QlErrorCode.Syntax, (int?)13), (error.Code, error.Position));
	}

	// ── Its meaning ─────────────────────────────────────────────────────────

	[Fact]
	public void AModelWithoutLikeRefusesIt()
	{
		var error = Refused("from orders where customer like 'c1%' select id", Shop.Model(like: false));

		Assert.Equal((QlErrorCode.Type, (int?)27), (error.Code, error.Position));
		Assert.Equal("`like` is not available in this model: the host has not declared what it means.", error.Message);
	}

	[Theory]
	[InlineData("from orders where 'c1' like customer select id",       23, "`like` matches a value of the rows")]
	[InlineData("from orders where @p like customer select id",         21, "`like` matches a value of the rows")]
	[InlineData("from orders where null like 'x' select id",            23, "`like` matches a value of the rows")]
	[InlineData("from orders where ('c1') like customer select id",     25, "`like` matches a value of the rows")]
	[InlineData("from orders where qty like '1%' select id",            22, "`like` matches strings, and this is a int")]
	[InlineData("from orders where customer like 1 select id",          32, "The pattern of `like` is a string, and this is a int")]
	[InlineData("from orders where customer like created select id",    32, "The pattern of `like` is a string")]
	[InlineData("from orders select customer like 'c%' + 1 as m",       38, "`+` is not defined for strings")]
	public void LikeMatchesAStringOfTheRowsAgainstAString(string text, int position, string message)
	{
		var error = Refused(text);

		Assert.Equal((QlErrorCode.Type, (int?)position), (error.Code, error.Position));
		Assert.StartsWith(message, error.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void LikeIsACondition()
	{
		var info = Model.Describe("from orders where status like @p && customer not like @q || status like customer select id, status like 'p%' as paid let x = paid where !x");

		Assert.Equal([QlType.Int, QlType.Bool], info.Columns.Select(c => c.Type));
		Assert.False(info.Columns[1].IsNullable);
		Assert.Equal([("p", QlType.String, true), ("q", QlType.String, true)], info.Parameters.Select(p => (p.Name, p.Type, p.IsNullable)));
	}

	// ── The tree ────────────────────────────────────────────────────────────

	/// <summary>A stand-in for a provider's LIKE, which the tree is read for.</summary>
	static bool Match(string? s, string? p)
	{
		return s is not null && p is not null && s.StartsWith(p, StringComparison.Ordinal);
	}

	[Fact]
	public void TheHooksBodyStandsWithTheStringsInPlaceOfItsParameters()
	{
		var hook = new QlModelBuilder<Shop>();

		hook.Entity("orders", s => s.AllOrders).Column("customer", o => o.Customer).Column("id", o => o.Id);
		hook.Like((s, p) => Match(s, p));

		var query = hook.Build().QueryData(Shop.InMemory(), "from orders where customer like @p && customer not like 'c2' select id", new Dictionary<string, object?> { ["p"] = "c" });
		var calls = Calls(query.Expression).Where(c => c.Method.Name == nameof(Match)).ToList();

		Assert.Equal(2, calls.Count);

		foreach (var call in calls)
			Assert.Equal(nameof(Order.Customer), Assert.IsAssignableFrom<MemberExpression>(call.Arguments[0]).Member.Name);

		Assert.Equal("c2", Assert.IsType<ConstantExpression>(calls[1].Arguments[1]).Value);
		Assert.Empty(Nodes(query.Expression).OfType<InvocationExpression>());
		Assert.Single(Nodes(query.Expression).OfType<UnaryExpression>(), u => u.NodeType == ExpressionType.Not && u.Operand == calls[1]);
		Assert.DoesNotContain(Nodes(query.Expression).OfType<ParameterExpression>(), p => p.Name is "s" or "p");

		Assert.Equal(Shop.Rows().Where(o => o.Customer != "c2").Select(o => (object?)o.Id), query.ToList().Select(r => r[0]));
	}

	static IEnumerable<MethodCallExpression> Calls(Expression expression)
	{
		return Nodes(expression).OfType<MethodCallExpression>();
	}

	static List<Expression> Nodes(Expression expression)
	{
		var collector = new Collector();

		collector.Visit(expression);

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

	// ── On the databases ────────────────────────────────────────────────────

	List<object?[]> Run(string provider, string text, object? values = null)
	{
		return Rows.Run(Model, databases.Get(provider), text, values);
	}

	static IEnumerable<object?> Ids(Func<Order, bool> where)
	{
		return Mine.Where(where).Select(o => (object?)o.Id);
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void LikeIsTheDatabasesLike(string provider)
	{
		Assert.Equal(Ids(o => o.Status == "paid"),                         Run(provider, "from orders where status like 'pa%' order by id select id").Select(r => r[0]));
		Assert.Equal(Ids(o => o.Customer == "c1"),                         Run(provider, "from orders where customer like @p order by id select id", new { p = "c1" }).Select(r => r[0]));
		Assert.Equal(Ids(o => o.Customer == "c1"),                         Run(provider, "from orders where customer like '_1' order by id select id").Select(r => r[0]));
		Assert.Equal(Ids(o => o.Status is not null),                       Run(provider, "from orders where status like customer || status like '%' order by id select id").Select(r => r[0]));

		// A class of characters is SQL Server's and not SQLite's: the language passes the pattern on as it is.
		var classes = provider == "SqlServer" ? Ids(o => o.Customer is "c1" or "c3") : [];

		Assert.Equal(classes, Run(provider, "from orders where customer like @p order by id select id", new { p = "c[13]" }).Select(r => r[0]));
	}

	/// <summary>
	/// The escape is the one the host's LIKE names, here a backslash: the language's string writes it doubled, a
	/// parameter as it is. Without it `%` and `_` are wildcards wherever they come from.
	/// </summary>
	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void TheEscapeIsTheHosts(string provider)
	{
		Assert.Equal(Ids(o => o.Status == "100%"),                         Run(provider, "from orders where status like '%\\\\%' order by id select id").Select(r => r[0]));
		Assert.Equal(Ids(o => o.Status == "a_b"),                          Run(provider, "from orders where status like @p order by id select id", new { p = "a\\_b" }).Select(r => r[0]));
		Assert.Equal(Ids(o => o.Status is not null && o.Status.Contains('%', StringComparison.Ordinal)), Run(provider, "from orders where status like '%\\\\%%' order by id select id").Select(r => r[0]));
		Assert.Equal(Ids(o => o.Status is not null),                       Run(provider, "from orders where status like @p order by id select id", new { p = "%" }).Select(r => r[0]));
	}

	/// <summary>
	/// <c>not like</c> is the host's LIKE under NOT, and what a null makes of it is the provider's: linq2db adds no
	/// <c>IS NULL</c>, so a null status is in neither result, unlike <c>!=</c>.
	/// </summary>
	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void NotLikeIsTheDatabasesNotLike(string provider)
	{
		Assert.Equal(Ids(o => o.Status is not null && !o.Status.StartsWith("pa", StringComparison.Ordinal)), Run(provider, "from orders where status not like 'pa%' order by id select id").Select(r => r[0]));
		Assert.Equal(Ids(o => o.Status is not null && !o.Status.StartsWith("pa", StringComparison.Ordinal)), Run(provider, "from orders where !(status like 'pa%') order by id select id").Select(r => r[0]));
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void LikeIsAValueToo(string provider)
	{
		var rows = Run(provider, "from orders let c1 = customer like 'c1' where id <= 6 order by id select id, c1, customer not like '%1' as other");

		Assert.Equal(Mine.Where(o => o.Id <= 6).Select(o => new object?[] { o.Id, o.Customer == "c1", o.Customer != "c1" }), rows);
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void TheSqlIsTheProvidersLike(string provider)
	{
		var sql = LinqToDB.LinqExtensions.ToSqlQuery(Model.QueryData(Shop.Of(databases.Get(provider), 1), "from orders where status like @p && customer not like 'c%' select id", new Dictionary<string, object?> { ["p"] = "a%" })).Sql;

		Assert.Contains("LIKE", sql, StringComparison.Ordinal);
		Assert.Contains("NOT LIKE", sql, StringComparison.Ordinal);
		Assert.Contains("ESCAPE", sql, StringComparison.Ordinal);
	}
}
