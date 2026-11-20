using System;
using System.Collections.Generic;
using System.Linq;

using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;

using Microsoft.Data.SqlClient;

using Xunit;

namespace DotGram.QueryLanguage.Tests;

/// <summary>
/// The databases the queries run on: SQLite in memory always, SQL Server where <c>DOTGRAM_SQLSERVER</c> names a
/// connection (CI has none, so there those tests are skipped). Each holds the forty orders of <see cref="Shop.Rows"/>.
/// </summary>
public sealed class Databases : IDisposable
{
	/// <summary>The database SQL Server tests make for themselves and drop when they are done.</summary>
	const string Database = "QlM0Tests";

	readonly DataConnection  _sqlite;
	readonly DataConnection? _sqlServer;
	readonly string?         _server;

	public Databases()
	{
		_sqlite = new DataConnection(new DataOptions().UseSQLite("Data Source=:memory:"));

		_sqlite.CreateTable<Order>();
		_sqlite.BulkCopy(Shop.Rows());

		_server = Environment.GetEnvironmentVariable("DOTGRAM_SQLSERVER");

		if (string.IsNullOrEmpty(_server))
			return;

		using (var master = new DataConnection(new DataOptions().UseSqlServer(_server)))
			master.Execute($"IF DB_ID('{Database}') IS NULL CREATE DATABASE [{Database}]");

		_sqlServer = new DataConnection(new DataOptions().UseSqlServer(_server + ";Initial Catalog=" + Database, SqlServerVersion.v2022));

		_sqlServer.DropTable<Order>(throwExceptionIfNotExists: false);
		_sqlServer.CreateTable<Order>();
		_sqlServer.BulkCopy(new BulkCopyOptions { BulkCopyType = BulkCopyType.MultipleRows }, Shop.Rows());
	}

	public static IEnumerable<TheoryDataRow<string>> Providers()
	{
		yield return new TheoryDataRow<string>("SQLite");
		yield return new TheoryDataRow<string>("SqlServer");
	}

	/// <summary>The connection of <paramref name="provider"/>; skips the test where it is not there.</summary>
	public DataConnection Get(string provider)
	{
		if (provider == "SQLite")
			return _sqlite;

		if (_sqlServer is null)
			Assert.Skip("DOTGRAM_SQLSERVER does not name a SQL Server.");

		return _sqlServer;
	}

	public void Dispose()
	{
		_sqlite.Dispose();

		if (_sqlServer is null)
			return;

		_sqlServer.Dispose();

		SqlConnection.ClearAllPools();

		using var master = new DataConnection(new DataOptions().UseSqlServer(_server!));

		master.Execute($"IF DB_ID('{Database}') IS NOT NULL BEGIN ALTER DATABASE [{Database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{Database}] END");
	}
}

[CollectionDefinition(nameof(Databases))]
public sealed class DatabaseCollection : ICollectionFixture<Databases>;

/// <summary>Helpers the end-to-end tests share.</summary>
static class Rows
{
	public static List<object?[]> Run(QlModel<Shop> model, IDataContext db, string text, object? values = null, int tenant = 1)
	{
		return [.. model.QueryData(Shop.Of(db, tenant), text, Values(values))];
	}

	public static IReadOnlyDictionary<string, object?>? Values(object? values)
	{
		if (values is IReadOnlyDictionary<string, object?> dictionary)
			return dictionary;

		return values?.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(values));
	}

	/// <summary>The first column of every row, as <typeparamref name="T"/>.</summary>
	public static T[] First<T>(this List<object?[]> rows)
	{
		return [.. rows.Select(r => (T)r[0]!)];
	}
}
