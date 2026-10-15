using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The DROP family's bridge (Bridge.Drop.cs): which record and members it builds for a text, where another record or a
/// flag set the other way writes the same or nearly the same text, and that each converts.
/// </summary>
public sealed class DropBridgeTests
{
	[Theory]
	[InlineData("DROP AGGREGATE IF EXISTS dbo.a, b",                      "Listed")]
	[InlineData("DROP SYNONYM s",                                         "Listed without IF EXISTS")]
	[InlineData("DROP USER IF EXISTS u",                                  "User")]
	[InlineData("DROP QUEUE dbo.q",                                       "Qualified")]
	[InlineData("DROP XML SCHEMA COLLECTION a..b",                        "Led")]
	[InlineData("DROP STATISTICS t.s, u.r",                               "List")]
	[InlineData("DROP WORKLOAD CLASSIFIER x, y",                          "Led list")]
	[InlineData("DROP LOGIN l",                                           "Named")]
	[InlineData("DROP SYMMETRIC KEY k REMOVE PROVIDER KEY",               "Key with provider")]
	[InlineData("DROP ASYMMETRIC KEY k",                                  "Key alone")]
	[InlineData("DROP EXTERNAL LANGUAGE x AUTHORIZATION dbo",             "Language")]
	[InlineData("DROP DATABASE IF EXISTS x, y",                           "Database")]
	[InlineData("DROP SERVER AUDIT a",                                    "Server audit")]
	[InlineData("DROP SERVER AUDIT SPECIFICATION a",                      "Server audit specification")]
	[InlineData("DROP DATABASE a",                                        "Database of one")]
	[InlineData("DROP DATABASE AUDIT SPECIFICATION a",                    "Database audit specification")]
	[InlineData("DROP DATABASE ENCRYPTION KEY",                           "Database encryption key")]
	[InlineData("DROP DATABASE SCOPED CREDENTIAL c",                      "Database scoped credential")]
	[InlineData("DROP INDEX i ON dbo.t WITH (MOVE TO ps (a), MAXDOP = 2)", "Index named")]
	[InlineData("DROP INDEX dbo.t.i",                                     "Index qualified")]
	[InlineData("DROP SIGNATURE FROM OBJECT::dbo.p BY CERTIFICATE c",     "Signature")]
	[InlineData("DROP COUNTER SIGNATURE FROM p BY ASYMMETRIC KEY k",      "Counter signature")]
	[InlineData("DROP FULLTEXT INDEX ON dbo.t",                           "Full-text index")]
	[InlineData("DROP FULLTEXT CATALOG c",                                "Full-text catalog")]
	[InlineData("DROP EVENT NOTIFICATION a, b ON QUEUE dbo.q",            "Event notification")]
	[InlineData("DROP EVENT SESSION s ON DATABASE",                       "Event session")]
	[InlineData("DROP ASSEMBLY IF EXISTS a WITH NO DEPENDENTS",           "Assembly")]
	[InlineData("DROP MASTER KEY",                                        "Master key")]
	[InlineData("DROP TABLE IF EXISTS t",                                 "ISO table")]
	public void The_bridge_builds_the_drop_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Listed" => made is Ast.Statement.DropAggregate { IfExists: true, Names: [{ Parts.Count: 2 }, { Parts.Count: 1 }] },
			"Listed without IF EXISTS" => made is Ast.Statement.DropSynonym { IfExists: false, Names.Count: 1 },
			"User" => made is Ast.Statement.DropUser { IfExists: true, Name.Text: "u" },
			"Qualified" => made is Ast.Statement.DropQueue { Name.Parts.Count: 2 },
			"Led" => made is Ast.Statement.DropXmlSchemaCollection { Name.Parts: [{ Text: "a" }, { Text: "" }, { Text: "b" }] },
			"List" => made is Ast.Statement.DropStatistics { Names.Count: 2 },
			"Led list" => made is Ast.Statement.DropWorkloadClassifier { Names.Count: 2 },
			"Named" => made is Ast.Statement.DropLogin { Name.Text: "l" },
			"Key with provider" => made is Ast.Statement.DropSymmetricKey { RemoveProviderKey: true },
			"Key alone" => made is Ast.Statement.DropAsymmetricKey { RemoveProviderKey: false },
			"Language" => made is Ast.Statement.DropExternalLanguage { Authorization.Text: "dbo" },
			"Database" => made is Ast.Statement.DropDatabase { IfExists: true, Names.Count: 2 },
			"Server audit" => made is Ast.Statement.DropServerAudit { Name.Text: "a" },
			"Server audit specification" => made is Ast.Statement.DropServerAuditSpecification { Name.Text: "a" },
			"Database of one" => made is Ast.Statement.DropDatabase { IfExists: false, Names: [{ Text: "a" }] },
			"Database audit specification" => made is Ast.Statement.DropDatabaseAuditSpecification { Name.Text: "a" },
			"Database encryption key" => made is Ast.Statement.DropDatabaseEncryptionKey,
			"Database scoped credential" => made is Ast.Statement.DropDatabaseScopedCredential { Name.Text: "c" },
			"Index named" => made is Ast.Statement.DropIndex { IfExists: false, Indexes: [Ast.DroppedIndex.Named { Name.Text: "i", On.Parts.Count: 2, Options: [{ Name.Text: "MOVE", Words: [{ Text: "TO" }], Value: Ast.Expression.Invocation }, { Name.Text: "MAXDOP" }] }] },
			"Index qualified" => made is Ast.Statement.DropIndex { Indexes: [Ast.DroppedIndex.Qualified { Name.Parts.Count: 3 }] },
			"Signature" => made is Ast.Statement.DropSignature { Counter: false, Class: Ast.SecurableClass.Object, By: [Ast.SignedBy.Certificate] },
			"Counter signature" => made is Ast.Statement.DropSignature { Counter: true, Class: null, By: [Ast.SignedBy.AsymmetricKey] },
			"Full-text index" => made is Ast.Statement.DropFullTextIndex { Table.Parts.Count: 2 },
			"Full-text catalog" => made is Ast.Statement.DropFullTextCatalog { Name.Text: "c" },
			"Event notification" => made is Ast.Statement.DropEventNotification { Names.Count: 2, On: Ast.NotificationScope.Queue { Name.Parts.Count: 2 } },
			"Event session" => made is Ast.Statement.DropEventSession { On: Ast.EventScope.Database },
			"Assembly" => made is Ast.Statement.DropAssembly { IfExists: true, NoDependents: true },
			"Master key" => made is Ast.Statement.DropMasterKey,
			"ISO table" => made is Ast.Statement.DropTable { IfExists: true },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}
}
