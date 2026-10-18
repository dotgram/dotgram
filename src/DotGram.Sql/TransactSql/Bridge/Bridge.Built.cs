using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

// The statements the rules build on the common tree. Such a rule hands what it read to a builder of its family, which
// makes the statement of the common tree from it; the statement stands where the rules hold statements as a
// Statement.Built, which holds what was built or the gap found. A reading is told the text it is read from before it
// starts (Over), so that what a builder reads from the text it can read while the statement is read.
static partial class Bridge
{
	/// <summary>
	/// What a rule hands a builder in place of the T-SQL tree's record: the record's name, under which the gaps of the
	/// statement are counted and said, and where it was written.
	/// </summary>
	internal abstract record Given(string Record) : ISqlSpan
	{
		/// <summary>Where the statement was written, where the rule said so.</summary>
		public SqlSpan Span { get; set; }

		/// <summary>Nothing locates it: it is no node of any tree.</summary>
		void ISqlSpan.Locate(int at, int length)
		{
		}
	}

	/// <summary>
	/// What a rule that defines an object by its name hands its builder, where the T-SQL tree kept the name and the words
	/// after it (<c>Statement.Definition</c>): the same members.
	/// </summary>
	internal sealed record Defining(string Record, string Name) : Given(Record)
	{
		/// <summary>What was written after the name, or null where nothing was.</summary>
		public string? Tail { get; init; }

		/// <summary>The settings written among <see cref="Tail"/>, as nodes, where the rule read them as such.</summary>
		public SqlList<Clause>? Options { get; init; }

		/// <summary>The word the statement opened with, where the syntax lets it be more than one.</summary>
		public string? Verb { get; init; }
	}

	/// <summary>
	/// The statements of a text read again only for what its rules hand their builders, which a check compares with what
	/// the first reading handed them, or null where it is not read: nothing is built, and nothing that building would read
	/// again is read.
	/// </summary>
	static Statement[]? Skim(string text)
	{
		var outer = skimming;

		skimming = true;

		try
		{
			return Reread(text) is { IsSuccess: true, Value: var statements } ? statements : null;
		}
		finally
		{
			skimming = outer;
		}
	}

	/// <summary>
	/// The definition the words name, on the common tree, where the T-SQL tree kept the name and no more: the words as the
	/// rule names them, the name, what was written after it, the verb it opened with and the settings among the words.
	/// </summary>
	internal static Statement Defined(string what, string name, string? tail = null, string? verb = null, SqlList<Clause>? options = null)
	{
		var (record, build) = Definition(what);
		var old             = new Defining(record, name) { Tail = tail, Verb = verb is null ? null : Syntax.Squared(verb), Options = options };

		return Hold(old, build);
	}

	/// <summary>The record of the T-SQL tree a definition's words name, and its builder.</summary>
	static (string Record, Func<Defining, Ast.Statement> Build) Definition(string what)
	{
		return what switch
		{
			"TYPE"                        => ("Statement.TypeDefinition",                Tables.CreateType),
			"XML SCHEMA COLLECTION"       => ("Statement.XmlSchemaCollectionDefinition", static old => Routines.SchemaCollection(old, alter: false)),
			"ALTER XML SCHEMA COLLECTION" => ("Statement.AlterXmlSchemaCollection",      static old => Routines.SchemaCollection(old, alter: true)),
			"SYNONYM"                     => ("Statement.SynonymDefinition",             Tables.Synonym),
			"SEQUENCE"                    => ("Statement.SequenceDefinition",            Tables.Sequence),
			"FULLTEXT INDEX"              => ("Statement.FullTextIndexDefinition",       Tables.CreateFullTextIndex),
			"ALTER FULLTEXT INDEX"        => ("Statement.AlterFullTextIndex",            Tables.AlterFullTextIndex),
			"ASSEMBLY"                    => ("Statement.AssemblyDefinition",            Routines.Assembly),
			"CRYPTOGRAPHIC PROVIDER"      => ("Statement.CryptographicProviderDefinition", Security.CryptographicProvider),
			"EXTERNAL LANGUAGE"           => ("Statement.ExternalLanguageDefinition",    External.Language),
			"EXTERNAL MODEL"              => ("Statement.ExternalModelDefinition",       External.Model),
			"RULE"                        => ("Statement.RuleDefinition",                Routines.Rule),
			"DEFAULT"                     => ("Statement.DefaultDefinition",             Routines.Default),
			"AGGREGATE"                   => ("Statement.AggregateDefinition",           Routines.Aggregate),
			"CREATE SCHEMA"                 => ("Statement.SchemaDefinition", Security.Schema),
			"CREATE ROLE"                   => ("Statement.CreateRole", Security.Role),
			"ALTER ROLE"                    => ("Statement.AlterRole", static old => new Ast.Statement.AlterRole(Values.Part(old, old.Name), Security.RoleAction(old, old.Tail))),
			"CREATE SERVER ROLE"            => ("Statement.CreateServerRole", static old => new Ast.Statement.CreateServerRole(Values.Part(old, old.Name)) { Authorization = Security.Owner(old, old.Tail) }),
			"ALTER SERVER ROLE"             => ("Statement.AlterServerRole", static old => new Ast.Statement.AlterServerRole(Values.Part(old, old.Name), Security.RoleAction(old, old.Tail))),
			"CREATE APPLICATION ROLE"       => ("Statement.CreateApplicationRole", static old => Security.ApplicationRole(old, old.Tail, create: true)),
			"ALTER APPLICATION ROLE"        => ("Statement.AlterApplicationRole", static old => Security.ApplicationRole(old, old.Tail, create: false)),
			"CREATE LOGIN"                  => ("Statement.CreateLogin", Security.CreateLogin),
			"ALTER LOGIN"                   => ("Statement.AlterLogin", Security.AlterLogin),
			"CREATE USER"                   => ("Statement.CreateUser", Security.CreateUser),
			"ALTER USER"                    => ("Statement.AlterUser", Security.AlterUser),
			"ALTER SCHEMA"                  => ("Statement.AlterSchema", Security.AlterSchema),
			"ALTER AUTHORIZATION"           => ("Statement.AlterAuthorization", Security.AlterAuthorization),
			"ASYMMETRIC KEY"                => ("Statement.AsymmetricKeyDefinition", Security.CreateAsymmetricKey),
			"ALTER ASYMMETRIC KEY"          => ("Statement.AlterAsymmetricKey", Security.AlterAsymmetricKey),
			"SYMMETRIC KEY"                 => ("Statement.SymmetricKeyDefinition", Security.CreateSymmetricKey),
			"ALTER SYMMETRIC KEY"           => ("Statement.AlterSymmetricKey", Security.AlterSymmetricKey),
			"CERTIFICATE"                   => ("Statement.CertificateDefinition", Security.CreateCertificate),
			"ALTER CERTIFICATE"             => ("Statement.AlterCertificate", Security.AlterCertificate),
			"MASTER KEY"                    => ("Statement.MasterKeyDefinition", Security.CreateMasterKey),
			"ALTER MASTER KEY"              => ("Statement.AlterMasterKey", Security.AlterMasterKey),
			"ALTER SERVICE MASTER KEY"      => ("Statement.AlterServiceMasterKey", Security.AlterServiceMasterKey),
			"DATABASE ENCRYPTION KEY"       => ("Statement.DatabaseEncryptionKeyDefinition", Security.CreateDatabaseEncryptionKey),
			"ALTER DATABASE ENCRYPTION KEY" => ("Statement.AlterDatabaseEncryptionKey", Security.AlterDatabaseEncryptionKey),
			"COLUMN ENCRYPTION KEY"         => ("Statement.ColumnEncryptionKeyDefinition", Security.CreateColumnEncryptionKey),
			"ALTER COLUMN ENCRYPTION KEY"   => ("Statement.AlterColumnEncryptionKey", Security.AlterColumnEncryptionKey),
			"COLUMN MASTER KEY"             => ("Statement.ColumnMasterKeyDefinition", Security.CreateColumnMasterKey),
			"CREDENTIAL"                    => ("Statement.CredentialDefinition", Security.Credential),
			"DATABASE SCOPED CREDENTIAL"    => ("Statement.DatabaseScopedCredentialDefinition", Security.DatabaseScopedCredential),
			"SECURITY POLICY"               => ("Statement.SecurityPolicyDefinition", Security.SecurityPolicy),
			"OPEN SYMMETRIC KEY"            => ("Statement.OpenSymmetricKey", Security.OpenSymmetricKey),
			"OPEN MASTER KEY"               => ("Statement.OpenMasterKey", Security.OpenMasterKey),
			"CLOSE SYMMETRIC KEY"           => ("Statement.CloseSymmetricKey", static old => new Ast.Statement.CloseSymmetricKey(Values.Part(old, old.Name))),
			"CLOSE ALL SYMMETRIC KEYS"      => ("Statement.CloseAllSymmetricKeys", static old => new Ast.Statement.CloseAllSymmetricKeys()),
			"CLOSE MASTER KEY"              => ("Statement.CloseMasterKey", static old => new Ast.Statement.CloseMasterKey()),
			"ALTER SERVER CONFIGURATION"    => ("Statement.AlterServerConfiguration", Server.Configuration),
			"ALTER RESOURCE GOVERNOR"       => ("Statement.AlterResourceGovernor", Server.Governor),
			"RESOURCE POOL"                 => ("Statement.ResourcePoolDefinition", Server.Pool),
			"WORKLOAD GROUP"                => ("Statement.WorkloadGroupDefinition", Server.Workload),
			"AVAILABILITY GROUP"            => ("Statement.AvailabilityGroupDefinition", Server.Availability),
			"MESSAGE TYPE"                  => ("Statement.MessageTypeDefinition", Broker.MessageType),
			"CONTRACT"                      => ("Statement.ContractDefinition", Broker.Contract),
			"QUEUE"                         => ("Statement.QueueDefinition", Broker.Queue),
			"SERVICE"                       => ("Statement.ServiceDefinition", Broker.Service),
			"ROUTE"                         => ("Statement.RouteDefinition", Broker.Route),
			"REMOTE SERVICE BINDING"        => ("Statement.RemoteServiceBindingDefinition", Broker.Binding),
			"BROKER PRIORITY"               => ("Statement.BrokerPriorityDefinition", Broker.Priority),
			"EVENT NOTIFICATION"            => ("Statement.EventNotificationDefinition", Monitoring.Notification),
			"SERVER AUDIT"                  => ("Statement.ServerAuditDefinition", Monitoring.Audit),
			"AUDIT SPECIFICATION"           => ("Statement.AuditSpecificationDefinition", static old => Monitoring.Specification(old, database: false)),
			"DATABASE AUDIT SPECIFICATION"  => ("Statement.DatabaseAuditSpecificationDefinition", static old => Monitoring.Specification(old, database: true)),
			"EXTERNAL DATA SOURCE"          => ("Statement.ExternalDataSourceDefinition", External.DataSource),
			"EXTERNAL FILE FORMAT"          => ("Statement.ExternalFileFormatDefinition", External.FileFormat),
			"EXTERNAL LIBRARY"              => ("Statement.ExternalLibraryDefinition", External.Library),
			"EXTERNAL RESOURCE POOL"        => ("Statement.ExternalResourcePoolDefinition", External.Pool),
			"PARTITION FUNCTION"         => ("Statement.PartitionFunctionDefinition",  Database.Definition),
			"ALTER PARTITION FUNCTION"   => ("Statement.AlterPartitionFunction",       Database.Definition),
			"PARTITION SCHEME"           => ("Statement.PartitionSchemeDefinition",    Database.Definition),
			"ALTER PARTITION SCHEME"     => ("Statement.AlterPartitionScheme",         Database.Definition),
			"FULLTEXT CATALOG"           => ("Statement.FullTextCatalogDefinition",    Database.Definition),
			"ALTER FULLTEXT CATALOG"     => ("Statement.AlterFullTextCatalog",         Database.Definition),
			"FULLTEXT STOPLIST"          => ("Statement.FullTextStopListDefinition",   Database.Definition),
			"ALTER FULLTEXT STOPLIST"    => ("Statement.AlterFullTextStopList",        Database.Definition),
			"SEARCH PROPERTY LIST"       => ("Statement.SearchPropertyListDefinition", Database.Definition),
			"ALTER SEARCH PROPERTY LIST" => ("Statement.AlterSearchPropertyList",      Database.Definition),
			_ => throw new ArgumentOutOfRangeException(nameof(what), what, "No rule hands a definition of these words to the common tree."),
		};
	}

	/// <summary>
	/// A statement its rule builds on the common tree, held where the rules hold statements: what the builder made, or
	/// the gap it found, which taking the statement throws.
	/// </summary>
	/// <param name="given">What the rule handed the builder: a <see cref="Given"/>, or the T-SQL tree's record its parts were gathered into.</param>
	/// <param name="build">The builder, handed <paramref name="given"/> back, so that it captures nothing.</param>
	static Statement Hold<T>(T given, Func<T, Ast.Statement> build) where T : class
	{
		if (skimming)
			return new Statement.Built(null, null, given);

		try
		{
			return new Statement.Built(build(given), null, given);
		}
		catch (BridgeGap gap)
		{
			return new Statement.Built(null, gap, given);
		}
	}

	/// <summary>The same, where the builder needs more than what was handed over: that too is handed back.</summary>
	static Statement Hold<T, TState>(T given, TState state, Func<T, TState, Ast.Statement> build) where T : class
	{
		if (skimming)
			return new Statement.Built(null, null, given);

		try
		{
			return new Statement.Built(build(given, state), null, given);
		}
		catch (BridgeGap gap)
		{
			return new Statement.Built(null, gap, given);
		}
	}

	/// <summary>
	/// A statement its rule read into the T-SQL tree's record, built on the common tree by the conversion that reads that
	/// record: the record is what the conversion reads, and no statement of any tree.
	/// </summary>
	static Statement Carry<T>(T read, Func<T, Ast.Statement> build) where T : Statement
	{
		return Hold(read, build);
	}

	/// <summary>
	/// The same, of a statement whose conversion reads the text it was written in: the record is told where, the rule's
	/// own extent.
	/// </summary>
	static Statement Carry<T>(T read, SqlSpan at, Func<T, Ast.Statement> build) where T : Statement
	{
		return Defer(read, at, build);
	}

	/// <summary>
	/// A statement whose conversion reads the text it was written in, held until the extent it ends with is known: the
	/// rule that reads the WITH in front of a statement and the `;` after it finishes it there (<see cref="Finish"/>), with
	/// that rule's extent, as the T-SQL tree's record was told it. <paramref name="at"/>, the rule's own extent, is what it
	/// is built with where no such rule follows.
	/// </summary>
	static Statement Defer<T>(T given, SqlSpan at, Func<T, Ast.Statement> build) where T : class
	{
		Situate(given, at);

		if (skimming)
			return new Statement.Built(null, null, given);

		return new Statement.Built(null, null, given) { Pending = one => build((T)one) };
	}

	/// <summary>A held statement built, told the extent it ends with.</summary>
	internal static Statement Finish(Statement.Built held, SqlSpan at)
	{
		Situate(held.Given, at);

		return Hold(held.Given, held.Pending!);
	}

	/// <summary>What a rule handed its builder, told where the statement stands.</summary>
	static void Situate(object given, SqlSpan at)
	{
		if (given is Given handed)
			handed.Span = at;
		else
			((Statement)given).Locate(at.At, at.Length);
	}

	/// <summary>Whether the reading under way is skimmed: its statements hold what their rules were handed, and are not built.</summary>
	[ThreadStatic] static bool skimming;

	/// <summary>
	/// A reading told its text, which the builders read the words their records leave out from, and whether it publishes
	/// where each node was written, for as long as it lasts.
	/// </summary>
	internal static Prior Over(string text, bool locate)
	{
		var outer = new Prior(source, locating, unlocated);

		source                 = text;
		SqlLocation.Publishing = locate;
		unlocated              = false;

		return outer;
	}

	/// <summary>The carriage that stood before a reading, put back when it ends; a value, so that a reading allocates none.</summary>
	internal readonly struct Prior(string? source, bool locating, bool unlocated) : IDisposable
	{
		public void Dispose()
		{
			Bridge.source          = source;
			SqlLocation.Publishing = locating;
			Bridge.unlocated       = unlocated;
		}
	}

	/// <summary>
	/// A text the bridge writes, or cuts out of a statement's, to read a part of it again, read through an internal entry:
	/// the plain one, which tells its records no span, or the located one. A builder that tells a record its rule's extent
	/// (<see cref="Tables.Sourced"/>) tells it only where the reading locates: in a plain one, as before the rules built
	/// the common tree, no record has a span to cut the text by, and none of the checks that read it is made.
	/// </summary>
	internal static TransactSqlParser.Match<Statement[]> Reread(string text, bool located = false)
	{
		var outer = unlocated;

		unlocated = !located;

		try
		{
			return located ? TransactSqlParser.Located.TryParseSyntaxSql(text) : TransactSqlParser.TryParseSyntaxSql(text);
		}
		finally
		{
			unlocated = outer;
		}
	}

	/// <summary>Whether the reading under way is a plain one the bridge made (<see cref="Reread"/>), whose records have no span.</summary>
	[ThreadStatic] static bool unlocated;

	/// <summary>The statements, queries and sources the rules built on the common tree: each taken as what it holds.</summary>
	internal static class Built
	{
		public static bool TryConvert(object read, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			switch (read)
			{
				case Statement.Built built:
					// Held for an extent no rule gave it: built with its rule's own.
					if (built.Pending is { } build)
						built = (Statement.Built)Hold(built.Given, build);

					made = built.Made ?? throw built.Gap!;

					return true;

				case Query.Built query:
					made = query.Made ?? throw query.Gap!;

					return true;

				case TableReference.Built source:
					made = source.Made ?? throw source.Gap!;

					return true;

				default:
					made = null;

					return false;
			}
		}
	}

	/// <summary>
	/// What a builder makes of what it is handed, built the moment its rule is read: the node, or the gap it found. A
	/// skimmed reading builds its queries and sources too, since the values its rules build take them at once.
	/// </summary>
	/// <param name="state">What the rule read, handed back to the builder, so that it captures nothing.</param>
	static void Building<T, TState>(TState state, Func<TState, T> build, out T? made, out BridgeGap? gap)
	{
		made = default;
		gap  = null;

		try
		{
			made = build(state);
		}
		catch (BridgeGap found)
		{
			gap = found;
		}
	}
}
