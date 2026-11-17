using System;
using System.Linq;
using System.Threading.Tasks;

using DotGram.QueryLanguage.LinqToDB;

using Xunit;

namespace DotGram.QueryLanguage.Tests;

/// <summary>
/// The same questions put to SQL Server, where QL_MSSQL names a connection. One class, so that
/// its tests share the table one at a time.
/// </summary>
public sealed class SqlServerTests
{
	static global::LinqToDB.Data.DataConnection Connect()
	{
		var db = Shop.SqlServer();

		if (db is null)
			Assert.Skip("QL_MSSQL is not set.");

		return db;
	}

	[Fact]
	public async Task LikeIsPortable()
	{
		using var db = Connect();

		await QueryTests.Like(db, QlLikeDialect.SqlServer);
	}

	[Fact]
	public async Task WithoutTheDialectABracketIsAClass()
	{
		using var db = Connect();

		// What escaping '[' is for: SQL Server reads `a[b%` as 'a', one of 'b', then anything.
		Assert.Equal(new[] { 5 }, await Shop.Ids(db, "status LIKE 'a[_]b'", dialect: QlLikeDialect.Standard));
		Assert.Empty(await Shop.Ids(db, "status LIKE 'a[_]b'", dialect: QlLikeDialect.SqlServer));
	}

	[Fact]
	public async Task IntegerDivisionTruncatesInTheDatabase()
	{
		using var db = Connect();

		Assert.Equal(new[] { 2, 3 }, await Shop.Ids(db, "id / 2 = 1"));
		Assert.Equal(new[] { 1 }, await Shop.Ids(db, "-id / 2 = 0 AND id > 0"));
	}

	[Fact]
	public async Task DivisionByZeroInTheDatabaseIsAQlError()
	{
		using var db = Connect();

		var error = await Assert.ThrowsAsync<QlException>(() => Shop.Ids(db, "id / @z = 1", new() { ["z"] = 0 }));

		Assert.Equal(QlErrorKind.DivisionByZero, error.Error.Kind);
	}

	[Fact]
	public async Task OverflowInTheDatabaseIsAQlError()
	{
		using var db = Connect();

		var error = await Assert.ThrowsAsync<QlException>(() => Shop.Ids(db, "id * 2147483647 * 2 = 1"));

		Assert.Equal(QlErrorKind.Overflow, error.Error.Kind);
	}

	[Fact]
	public async Task TheProjectionCarriesEveryType()
	{
		using var db = Connect();

		var rows = await Shop.Run(db, "SELECT * FROM orders WHERE id = 1");

		Assert.Equal([1, "paid", 12.5m, 1_000_000_000L, 0.25, new DateTime(2026, 1, 2), 1, "c1", "l1"], rows.Single());
	}
}
