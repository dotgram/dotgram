using System;

using DotGram.Sql.Ast;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// The DROP family's writer (Sql2023Writer.TransactSql.Drop.cs): each record written as its kind's words and the
/// slots the grammar reads for it, and every tree whose text would read back as another tree refused.
/// </summary>
public sealed class DropWriterTests
{
	static readonly Identifier X = new("x");
	static readonly Identifier Y = new("y");
	static readonly QualifiedName S = new([new Identifier("s")]);
	static readonly QualifiedName Q = new([new Identifier("dbo"), new Identifier("q")]);

	/// <summary><c>.x</c>: a name whose first part is empty.</summary>
	static readonly QualifiedName Led = new([new Identifier(""), new Identifier("x")]);

	static Expression Number(string text)
	{
		return new LiteralValue.Numeric(text, NumericLiteralKind.DecimalInteger);
	}

	static Expression Ref(string word)
	{
		return new Expression.Reference(new QualifiedName([new Identifier(word)]));
	}

	static Option Option(string name, Expression? value)
	{
		return new Option(new Identifier(name), value, []);
	}

	/// <summary><c>MOVE TO</c> the place given: <c>MOVE</c> with <c>TO</c> among its further words, the target its place.</summary>
	static Option Moved(Identifier target, SqlList<Identifier>? columns = null)
	{
		return new Option(new Identifier("MOVE"), null, []) { Words = [new Identifier("TO")], Place = new Placement(PlacementKind.MoveTo, target, columns) };
	}

	/// <summary><c>WAIT_AT_LOW_PRIORITY (MAX_DURATION = 5 MINUTES, ABORT_AFTER_WAIT = SELF)</c>.</summary>
	static Option Waited()
	{
		return new Option(new Identifier("WAIT_AT_LOW_PRIORITY"), null,
			[Option("MAX_DURATION", new Expression.Measured(Number("5"), new Identifier("MINUTES"))), Option("ABORT_AFTER_WAIT", Ref("SELF"))]);
	}

	/// <summary><c>DROP INDEX x ON s WITH (…)</c> with the options given.</summary>
	static Statement.DropIndex Indexed(params Option[] options)
	{
		return new Statement.DropIndex([new DroppedIndex.Named(X, S, [.. options])], false);
	}

	/// <summary>A tree for each record of the family, and the text it writes.</summary>
	static Statement Tree(string which)
	{
		return which switch
		{
			"Aggregate" => new Statement.DropAggregate([S, Q]) { IfExists = true },
			"Default" => new Statement.DropDefault([S, Q]) { IfExists = true },
			"Rule" => new Statement.DropRule([S, Q]) { IfExists = true },
			"SecurityPolicy" => new Statement.DropSecurityPolicy([S, Q]) { IfExists = true },
			"Synonym" => new Statement.DropSynonym([S, Q]) { IfExists = true },
			"User" => new Statement.DropUser(X) { IfExists = true },
			"Contract" => new Statement.DropContract(Q),
			"ExternalTable" => new Statement.DropExternalTable(Q),
			"MessageType" => new Statement.DropMessageType(Q),
			"Queue" => new Statement.DropQueue(Q),
			"RemoteServiceBinding" => new Statement.DropRemoteServiceBinding(Q),
			"Route" => new Statement.DropRoute(Q),
			"Service" => new Statement.DropService(Q),
			"XmlSchemaCollection" => new Statement.DropXmlSchemaCollection(Q),
			"Statistics" => new Statement.DropStatistics([Q, S]),
			"WorkloadClassifier" => new Statement.DropWorkloadClassifier([Q, S]),
			"ApplicationRole" => new Statement.DropApplicationRole(X),
			"AvailabilityGroup" => new Statement.DropAvailabilityGroup(X),
			"BrokerPriority" => new Statement.DropBrokerPriority(X),
			"Certificate" => new Statement.DropCertificate(X),
			"ColumnEncryptionKey" => new Statement.DropColumnEncryptionKey(X),
			"ColumnMasterKey" => new Statement.DropColumnMasterKey(X),
			"Credential" => new Statement.DropCredential(X),
			"CryptographicProvider" => new Statement.DropCryptographicProvider(X),
			"DatabaseAuditSpecification" => new Statement.DropDatabaseAuditSpecification(X),
			"DatabaseScopedCredential" => new Statement.DropDatabaseScopedCredential(X),
			"Endpoint" => new Statement.DropEndpoint(X),
			"ExternalDataSource" => new Statement.DropExternalDataSource(X),
			"ExternalFileFormat" => new Statement.DropExternalFileFormat(X),
			"ExternalModel" => new Statement.DropExternalModel(X),
			"ExternalResourcePool" => new Statement.DropExternalResourcePool(X),
			"FullTextCatalog" => new Statement.DropFullTextCatalog(X),
			"FullTextStopList" => new Statement.DropFullTextStopList(X),
			"Login" => new Statement.DropLogin(X),
			"PartitionFunction" => new Statement.DropPartitionFunction(X),
			"PartitionScheme" => new Statement.DropPartitionScheme(X),
			"ResourcePool" => new Statement.DropResourcePool(X),
			"SearchPropertyList" => new Statement.DropSearchPropertyList(X),
			"ServerAudit" => new Statement.DropServerAudit(X),
			"ServerAuditSpecification" => new Statement.DropServerAuditSpecification(X),
			"ServerRole" => new Statement.DropServerRole(X),
			"WorkloadGroup" => new Statement.DropWorkloadGroup(X),
			"AsymmetricKey" => new Statement.DropAsymmetricKey(X) { RemoveProviderKey = true },
			"SymmetricKey" => new Statement.DropSymmetricKey(X) { RemoveProviderKey = true },
			"ExternalLanguage" => new Statement.DropExternalLanguage(X) { Authorization = Y },
			"Database" => new Statement.DropDatabase([X, Y]) { IfExists = true },
			"Assembly" => new Statement.DropAssembly([S, Q], true, true),
			"EventSession" => new Statement.DropEventSession(X, EventScope.Database),
			"EventNotification" => new Statement.DropEventNotification([X, Y], new NotificationScope.Queue(Q)),
			"ExternalLibrary" => new Statement.DropExternalLibrary(X, Y),
			"FullTextIndex" => new Statement.DropFullTextIndex(Q),
			"SensitivityClassification" => new Statement.DropSensitivityClassification([Q]),
			"MasterKey" => new Statement.DropMasterKey(),
			"DatabaseEncryptionKey" => new Statement.DropDatabaseEncryptionKey(),
			"Index named" => new Statement.DropIndex([new DroppedIndex.Named(X, Q, [Option("MAXDOP", Number("2")), Option("ONLINE", Ref("ON")), Moved(new Identifier("fg")), Option("DATA_COMPRESSION", Ref("PAGE")) with { Partitions = [new PartitionRange(Number("1"), Number("3"))] }, Waited()])], true),
			"Index qualified" => new Statement.DropIndex([new DroppedIndex.Qualified(Q), new DroppedIndex.Named(X, S, [Moved(new Identifier("fg", IdentifierStyle.String)), new Option(new Identifier("FILESTREAM_ON"), null, []) { Place = new Placement(PlacementKind.FilestreamOn, new Identifier("fs", IdentifierStyle.String), null) }])], false),
			"Signature" => new Statement.DropSignature(false, Q, [new SignedBy.Certificate(new QualifiedName([X])), new SignedBy.AsymmetricKey(new QualifiedName([Y]))]),
			"Counter signature" => new Statement.DropSignature(true, Q, [new SignedBy.Certificate(new QualifiedName([X]))]) { Class = SecurableClass.Object },
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};
	}

	/// <summary>Each record written as the text of its kind.</summary>
	[Theory]
	[InlineData("Aggregate", "DROP AGGREGATE IF EXISTS s, dbo.q")]
	[InlineData("Default", "DROP DEFAULT IF EXISTS s, dbo.q")]
	[InlineData("Rule", "DROP RULE IF EXISTS s, dbo.q")]
	[InlineData("SecurityPolicy", "DROP SECURITY POLICY IF EXISTS s, dbo.q")]
	[InlineData("Synonym", "DROP SYNONYM IF EXISTS s, dbo.q")]
	[InlineData("User", "DROP USER IF EXISTS x")]
	[InlineData("Contract", "DROP CONTRACT dbo.q")]
	[InlineData("ExternalTable", "DROP EXTERNAL TABLE dbo.q")]
	[InlineData("MessageType", "DROP MESSAGE TYPE dbo.q")]
	[InlineData("Queue", "DROP QUEUE dbo.q")]
	[InlineData("RemoteServiceBinding", "DROP REMOTE SERVICE BINDING dbo.q")]
	[InlineData("Route", "DROP ROUTE dbo.q")]
	[InlineData("Service", "DROP SERVICE dbo.q")]
	[InlineData("XmlSchemaCollection", "DROP XML SCHEMA COLLECTION dbo.q")]
	[InlineData("Statistics", "DROP STATISTICS dbo.q, s")]
	[InlineData("WorkloadClassifier", "DROP WORKLOAD CLASSIFIER dbo.q, s")]
	[InlineData("ApplicationRole", "DROP APPLICATION ROLE x")]
	[InlineData("AvailabilityGroup", "DROP AVAILABILITY GROUP x")]
	[InlineData("BrokerPriority", "DROP BROKER PRIORITY x")]
	[InlineData("Certificate", "DROP CERTIFICATE x")]
	[InlineData("ColumnEncryptionKey", "DROP COLUMN ENCRYPTION KEY x")]
	[InlineData("ColumnMasterKey", "DROP COLUMN MASTER KEY x")]
	[InlineData("Credential", "DROP CREDENTIAL x")]
	[InlineData("CryptographicProvider", "DROP CRYPTOGRAPHIC PROVIDER x")]
	[InlineData("DatabaseAuditSpecification", "DROP DATABASE AUDIT SPECIFICATION x")]
	[InlineData("DatabaseScopedCredential", "DROP DATABASE SCOPED CREDENTIAL x")]
	[InlineData("Endpoint", "DROP ENDPOINT x")]
	[InlineData("ExternalDataSource", "DROP EXTERNAL DATA SOURCE x")]
	[InlineData("ExternalFileFormat", "DROP EXTERNAL FILE FORMAT x")]
	[InlineData("ExternalModel", "DROP EXTERNAL MODEL x")]
	[InlineData("ExternalResourcePool", "DROP EXTERNAL RESOURCE POOL x")]
	[InlineData("FullTextCatalog", "DROP FULLTEXT CATALOG x")]
	[InlineData("FullTextStopList", "DROP FULLTEXT STOPLIST x")]
	[InlineData("Login", "DROP LOGIN x")]
	[InlineData("PartitionFunction", "DROP PARTITION FUNCTION x")]
	[InlineData("PartitionScheme", "DROP PARTITION SCHEME x")]
	[InlineData("ResourcePool", "DROP RESOURCE POOL x")]
	[InlineData("SearchPropertyList", "DROP SEARCH PROPERTY LIST x")]
	[InlineData("ServerAudit", "DROP SERVER AUDIT x")]
	[InlineData("ServerAuditSpecification", "DROP SERVER AUDIT SPECIFICATION x")]
	[InlineData("ServerRole", "DROP SERVER ROLE x")]
	[InlineData("WorkloadGroup", "DROP WORKLOAD GROUP x")]
	[InlineData("AsymmetricKey", "DROP ASYMMETRIC KEY x REMOVE PROVIDER KEY")]
	[InlineData("SymmetricKey", "DROP SYMMETRIC KEY x REMOVE PROVIDER KEY")]
	[InlineData("ExternalLanguage", "DROP EXTERNAL LANGUAGE x AUTHORIZATION y")]
	[InlineData("Database", "DROP DATABASE IF EXISTS x, y")]
	[InlineData("Assembly", "DROP ASSEMBLY IF EXISTS s, dbo.q WITH NO DEPENDENTS")]
	[InlineData("EventSession", "DROP EVENT SESSION x ON DATABASE")]
	[InlineData("EventNotification", "DROP EVENT NOTIFICATION x, y ON QUEUE dbo.q")]
	[InlineData("ExternalLibrary", "DROP EXTERNAL LIBRARY x AUTHORIZATION y")]
	[InlineData("FullTextIndex", "DROP FULLTEXT INDEX ON dbo.q")]
	[InlineData("SensitivityClassification", "DROP SENSITIVITY CLASSIFICATION FROM dbo.q")]
	[InlineData("MasterKey", "DROP MASTER KEY")]
	[InlineData("DatabaseEncryptionKey", "DROP DATABASE ENCRYPTION KEY")]
	[InlineData("Index named", "DROP INDEX IF EXISTS x ON dbo.q WITH (MAXDOP = 2, ONLINE = ON, MOVE TO fg, DATA_COMPRESSION = PAGE ON PARTITIONS (1 TO 3), WAIT_AT_LOW_PRIORITY (MAX_DURATION = 5 MINUTES, ABORT_AFTER_WAIT = SELF))")]
	[InlineData("Index qualified", "DROP INDEX dbo.q, x ON s WITH (MOVE TO 'fg', FILESTREAM_ON 'fs')")]
	[InlineData("Signature", "DROP SIGNATURE FROM dbo.q BY CERTIFICATE x, ASYMMETRIC KEY y")]
	[InlineData("Counter signature", "DROP COUNTER SIGNATURE FROM OBJECT::dbo.q BY CERTIFICATE x")]
	public void Each_record_writes_its_kind(string which, string text)
	{
		Assert.Equal(text, Sql2023Writer.Write(Tree(which)));
	}

	/// <summary>
	/// Each record's tree, written and read back by TransactSqlParser, as the same tree.
	/// </summary>
	[Theory]
	[InlineData("Aggregate")]
	[InlineData("Default")]
	[InlineData("Rule")]
	[InlineData("SecurityPolicy")]
	[InlineData("Synonym")]
	[InlineData("User")]
	[InlineData("Contract")]
	[InlineData("ExternalTable")]
	[InlineData("MessageType")]
	[InlineData("Queue")]
	[InlineData("RemoteServiceBinding")]
	[InlineData("Route")]
	[InlineData("Service")]
	[InlineData("XmlSchemaCollection")]
	[InlineData("Statistics")]
	[InlineData("WorkloadClassifier")]
	[InlineData("ApplicationRole")]
	[InlineData("AvailabilityGroup")]
	[InlineData("BrokerPriority")]
	[InlineData("Certificate")]
	[InlineData("ColumnEncryptionKey")]
	[InlineData("ColumnMasterKey")]
	[InlineData("Credential")]
	[InlineData("CryptographicProvider")]
	[InlineData("DatabaseAuditSpecification")]
	[InlineData("DatabaseScopedCredential")]
	[InlineData("Endpoint")]
	[InlineData("ExternalDataSource")]
	[InlineData("ExternalFileFormat")]
	[InlineData("ExternalModel")]
	[InlineData("ExternalResourcePool")]
	[InlineData("FullTextCatalog")]
	[InlineData("FullTextStopList")]
	[InlineData("Login")]
	[InlineData("PartitionFunction")]
	[InlineData("PartitionScheme")]
	[InlineData("ResourcePool")]
	[InlineData("SearchPropertyList")]
	[InlineData("ServerAudit")]
	[InlineData("ServerAuditSpecification")]
	[InlineData("ServerRole")]
	[InlineData("WorkloadGroup")]
	[InlineData("AsymmetricKey")]
	[InlineData("SymmetricKey")]
	[InlineData("ExternalLanguage")]
	[InlineData("Database")]
	[InlineData("Assembly")]
	[InlineData("EventSession")]
	[InlineData("EventNotification")]
	[InlineData("ExternalLibrary")]
	[InlineData("FullTextIndex")]
	[InlineData("SensitivityClassification")]
	[InlineData("MasterKey")]
	[InlineData("DatabaseEncryptionKey")]
	[InlineData("Index named")]
	[InlineData("Index qualified")]
	[InlineData("Signature")]
	[InlineData("Counter signature")]
	public void Each_record_reads_back_as_itself(string which)
	{
		var tree = Tree(which);
		var text = TransactSqlWriter.Write(tree);

		Assert.Equal(tree, TransactSqlParser.Located.ParseStatement(text));
	}

	/// <summary>
	/// A tree whose text would not read back as the same tree is refused; each row names the rule or the engine's
	/// message that does not read the text.
	/// </summary>
	[Theory]
	[InlineData("a list of none")]
	[InlineData("a database list of none")]
	[InlineData("an XML schema collection with a leading dot")]
	[InlineData("a workload classifier with a leading dot")]
	[InlineData("an assembly list of none")]
	[InlineData("an event notification of no name")]
	[InlineData("an event session of no scope")]
	[InlineData("a sensitivity classification of no column")]
	[InlineData("an index list of none")]
	[InlineData("an index option twice")]
	[InlineData("an index option outside DropIndexOption")]
	[InlineData("ONLINE set to other than ON or OFF")]
	[InlineData("MAXDOP set to other than digits")]
	[InlineData("a compression the index has no word for")]
	[InlineData("MOVE TO a bare DEFAULT")]
	[InlineData("MOVE without TO")]
	[InlineData("FILESTREAM_ON a scheme with its column")]
	[InlineData("MOVE TO a scheme with its column")]
	[InlineData("MOVE TO DEFAULT with a column")]
	[InlineData("MAXDOP set to a signed number")]
	[InlineData("a low priority wait of a hexadecimal number")]
	[InlineData("a low priority wait of money")]
	[InlineData("a server audit named SPECIFICATION")]
	[InlineData("words on an option other than MOVE")]
	[InlineData("an = left out of an index option")]
	[InlineData("partitions on an option other than DATA_COMPRESSION")]
	[InlineData("a low priority wait without its abort")]
	[InlineData("a low priority wait in other minutes")]
	[InlineData("a signature by nothing")]
	[InlineData("a signature off a module of another class")]
	[InlineData("a signature by a certificate of two parts")]
	public void A_drop_with_no_text_is_refused(string which)
	{
		ISqlNode node = which switch
		{
			// DropNames reads one name at least.
			"a list of none" => new Statement.DropAggregate([]),
			// DropOneParts reads one name at least.
			"a database list of none" => new Statement.DropDatabase([]),
			// DropLedName begins with an Identifier: Msg 102.
			"an XML schema collection with a leading dot" => new Statement.DropXmlSchemaCollection(Led),
			// DropLedNames begins each name with an Identifier: Msg 102.
			"a workload classifier with a leading dot" => new Statement.DropWorkloadClassifier([S, Led]),
			// DropNames reads one name at least.
			"an assembly list of none" => new Statement.DropAssembly([], false, true),
			// DropOneParts reads one name at least.
			"an event notification of no name" => new Statement.DropEventNotification([], new NotificationScope.Server()),
			// the tail reads ON SERVER or ON DATABASE only.
			"an event session of no scope" => new Statement.DropEventSession(X, (EventScope)7),
			// DropNames reads one name at least.
			"a sensitivity classification of no column" => new Statement.DropSensitivityClassification([]),
			// DropIndexName is read once at least.
			"an index list of none" => new Statement.DropIndex([], false),
			// DropIndexOptions: Syntax.NamedOnce, Msg 153.
			"an index option twice" => Indexed(Option("MAXDOP", Number("1")), Option("maxdop", Number("2"))),
			// DropIndexOption has no PAD_INDEX: Msg 102.
			"an index option outside DropIndexOption" => Indexed(Option("PAD_INDEX", Ref("ON"))),
			// DropIndexOption reads ONLINE = OnOff.
			"ONLINE set to other than ON or OFF" => Indexed(Option("ONLINE", Number("1"))),
			// DropIndexOption reads MAXDOP = Digits.
			"MAXDOP set to other than digits" => Indexed(Option("MAXDOP", Ref("x"))),
			// RowCompression | ColumnstoreCompression.
			"a compression the index has no word for" => Indexed(Option("DATA_COMPRESSION", Ref("PREFIX"))),
			// DropIndexOption reads MOVE TO ?!DEFAULT: Msg 156.
			"MOVE TO a bare DEFAULT" => Indexed(Moved(new Identifier("DEFAULT"))),
			// DropIndexOption reads MOVE TO, and MOVE alone is Msg 155.
			"MOVE without TO" => Indexed(new Option(new Identifier("MOVE"), Ref("fg"), [])),
			// DropIndexOption: FILESTREAM_ON when p is not { Columns: not null }.
			"FILESTREAM_ON a scheme with its column" => Indexed(new Option(new Identifier("FILESTREAM_ON"), null, []) { Place = new Placement(PlacementKind.FilestreamOn, new Identifier("ps"), [new Identifier("a")]) }),
			// MOVE TO's target is its place (Option.Place), never its value: DropIndexOption reads no value after MOVE TO.
			"MOVE TO a scheme with its column" => Indexed(new Option(new Identifier("MOVE"), new Expression.Invocation(new QualifiedName([new Identifier("ps")]), [new Argument(Ref("a"))]), []) { Words = [new Identifier("TO")] }),
			// DropIndexOption reads MOVE TO ?!DEFAULT, with or without a column after it.
			"MOVE TO DEFAULT with a column" => Indexed(Moved(new Identifier("DEFAULT"), [new Identifier("a")])),
			// DropIndexOption reads MAXDOP = Digits: no sign.
			"MAXDOP set to a signed number" => Indexed(Option("MAXDOP", Number("-1"))),
			// Minutes = Number, an UnsignedNumericLiteral: no 0x.
			"a low priority wait of a hexadecimal number" => Indexed(new Option(new Identifier("WAIT_AT_LOW_PRIORITY"), null, [Option("MAX_DURATION", new LiteralValue.Numeric("0x1F", NumericLiteralKind.HexInteger)), Option("ABORT_AFTER_WAIT", Ref("NONE"))])),
			// Minutes = Number, an UnsignedNumericLiteral: no $.
			"a low priority wait of money" => Indexed(new Option(new Identifier("WAIT_AT_LOW_PRIORITY"), null, [Option("MAX_DURATION", new LiteralValue.Numeric("$5", NumericLiteralKind.Money)), Option("ABORT_AFTER_WAIT", Ref("NONE"))])),
			// DropNamedKind reads SERVER AUDIT SPECIFICATION first and then wants a name: the text does not read.
			"a server audit named SPECIFICATION" => new Statement.DropServerAudit(new Identifier("SPECIFICATION")),
			// only MOVE TO is two words in DropIndexOption.
			"words on an option other than MOVE" => Indexed(Option("ONLINE", Ref("ON")) with { Words = [new Identifier("TO")] }),
			// DropIndexOption reads MAXDOP with its =.
			"an = left out of an index option" => Indexed(Option("MAXDOP", Number("1")) with { EqualsOmitted = true }),
			// OnPartitions follows DATA_COMPRESSION alone.
			"partitions on an option other than DATA_COMPRESSION" => Indexed(Option("ONLINE", Ref("ON")) with { Partitions = [new PartitionRange(Number("1"), null)] }),
			// LowPriorityWait reads MAX_DURATION and ABORT_AFTER_WAIT, both.
			"a low priority wait without its abort" => Indexed(new Option(new Identifier("WAIT_AT_LOW_PRIORITY"), null, [Option("MAX_DURATION", Number("5"))])),
			// Minutes = Number & MINUTES?.
			"a low priority wait in other minutes" => Indexed(new Option(new Identifier("WAIT_AT_LOW_PRIORITY"), null, [Option("MAX_DURATION", new Expression.Measured(Number("5"), new Identifier("HOURS"))), Option("ABORT_AFTER_WAIT", Ref("NONE"))])),
			// SignatureCrypto is read once at least.
			"a signature by nothing" => new Statement.DropSignature(false, Q, []),
			// SignedModule reads OBJECT, ASSEMBLY or DATABASE before ::.
			"a signature off a module of another class" => new Statement.DropSignature(false, Q, [new SignedBy.Certificate(new QualifiedName([X]))]) { Class = SecurableClass.Schema },
			// SignatureCrypto reads CERTIFICATE Identifier.
			"a signature by a certificate of two parts" => new Statement.DropSignature(false, Q, [new SignedBy.Certificate(Q)]),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => Sql2023Writer.Write(node));
	}
}
