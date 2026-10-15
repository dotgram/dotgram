using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Drop family: writes the nodes it owns and answers false for every other. Each kind writes its own words
		/// and only the slots the grammar reads for it (TransactSql.gram, DropBody); what would read back as another tree
		/// is refused.
		/// </summary>
		bool TryPutDrop(ISqlNode node)
		{
			switch (node)
			{
				case Statement.DropAggregate drop:
					PutDropped(drop, "AGGREGATE", drop.IfExists, drop.Names, led: false);
					return true;

				case Statement.DropDefault drop:
					PutDropped(drop, "DEFAULT", drop.IfExists, drop.Names, led: false);
					return true;

				case Statement.DropRule drop:
					PutDropped(drop, "RULE", drop.IfExists, drop.Names, led: false);
					return true;

				case Statement.DropSecurityPolicy drop:
					PutDropped(drop, "SECURITY POLICY", drop.IfExists, drop.Names, led: false);
					return true;

				case Statement.DropSynonym drop:
					PutDropped(drop, "SYNONYM", drop.IfExists, drop.Names, led: false);
					return true;

				case Statement.DropUser drop:
					PutDropped("USER", drop.IfExists, drop.Name);
					return true;

				case Statement.DropContract drop:
					PutDropped(drop, "CONTRACT", false, [drop.Name], led: false);
					return true;

				case Statement.DropExternalTable drop:
					PutDropped(drop, "EXTERNAL TABLE", false, [drop.Name], led: false);
					return true;

				case Statement.DropMessageType drop:
					PutDropped(drop, "MESSAGE TYPE", false, [drop.Name], led: false);
					return true;

				case Statement.DropQueue drop:
					PutDropped(drop, "QUEUE", false, [drop.Name], led: false);
					return true;

				case Statement.DropRemoteServiceBinding drop:
					PutDropped(drop, "REMOTE SERVICE BINDING", false, [drop.Name], led: false);
					return true;

				case Statement.DropRoute drop:
					PutDropped(drop, "ROUTE", false, [drop.Name], led: false);
					return true;

				case Statement.DropService drop:
					PutDropped(drop, "SERVICE", false, [drop.Name], led: false);
					return true;

				case Statement.DropXmlSchemaCollection drop:
					PutDropped(drop, "XML SCHEMA COLLECTION", false, [drop.Name], led: true);
					return true;

				case Statement.DropStatistics drop:
					PutDropped(drop, "STATISTICS", false, drop.Names, led: false);
					return true;

				case Statement.DropWorkloadClassifier drop:
					PutDropped(drop, "WORKLOAD CLASSIFIER", false, drop.Names, led: true);
					return true;

				case Statement.DropApplicationRole drop:
					PutDropped("APPLICATION ROLE", false, drop.Name);
					return true;

				case Statement.DropAvailabilityGroup drop:
					PutDropped("AVAILABILITY GROUP", false, drop.Name);
					return true;

				case Statement.DropBrokerPriority drop:
					PutDropped("BROKER PRIORITY", false, drop.Name);
					return true;

				case Statement.DropCertificate drop:
					PutDropped("CERTIFICATE", false, drop.Name);
					return true;

				case Statement.DropColumnEncryptionKey drop:
					PutDropped("COLUMN ENCRYPTION KEY", false, drop.Name);
					return true;

				case Statement.DropColumnMasterKey drop:
					PutDropped("COLUMN MASTER KEY", false, drop.Name);
					return true;

				case Statement.DropCredential drop:
					PutDropped("CREDENTIAL", false, drop.Name);
					return true;

				case Statement.DropCryptographicProvider drop:
					PutDropped("CRYPTOGRAPHIC PROVIDER", false, drop.Name);
					return true;

				case Statement.DropDatabaseAuditSpecification drop:
					PutDropped("DATABASE AUDIT SPECIFICATION", false, drop.Name);
					return true;

				case Statement.DropDatabaseScopedCredential drop:
					PutDropped("DATABASE SCOPED CREDENTIAL", false, drop.Name);
					return true;

				case Statement.DropEndpoint drop:
					PutDropped("ENDPOINT", false, drop.Name);
					return true;

				case Statement.DropExternalDataSource drop:
					PutDropped("EXTERNAL DATA SOURCE", false, drop.Name);
					return true;

				case Statement.DropExternalFileFormat drop:
					PutDropped("EXTERNAL FILE FORMAT", false, drop.Name);
					return true;

				case Statement.DropExternalModel drop:
					PutDropped("EXTERNAL MODEL", false, drop.Name);
					return true;

				case Statement.DropExternalResourcePool drop:
					PutDropped("EXTERNAL RESOURCE POOL", false, drop.Name);
					return true;

				case Statement.DropFullTextCatalog drop:
					PutDropped("FULLTEXT CATALOG", false, drop.Name);
					return true;

				case Statement.DropFullTextStopList drop:
					PutDropped("FULLTEXT STOPLIST", false, drop.Name);
					return true;

				case Statement.DropLogin drop:
					PutDropped("LOGIN", false, drop.Name);
					return true;

				case Statement.DropPartitionFunction drop:
					PutDropped("PARTITION FUNCTION", false, drop.Name);
					return true;

				case Statement.DropPartitionScheme drop:
					PutDropped("PARTITION SCHEME", false, drop.Name);
					return true;

				case Statement.DropResourcePool drop:
					PutDropped("RESOURCE POOL", false, drop.Name);
					return true;

				case Statement.DropSearchPropertyList drop:
					PutDropped("SEARCH PROPERTY LIST", false, drop.Name);
					return true;

				case Statement.DropServerAudit drop:
					// DropNamedKind reads SERVER AUDIT SPECIFICATION before SERVER AUDIT, and ordered choice does not come back
					// to the shorter kind when no name follows: an audit named SPECIFICATION is written delimited or not at all.
					if (Plain(drop.Name, "SPECIFICATION"))
						throw Misplaced(drop, "Name", "SPECIFICATION as a plain word, which DropNamedKind reads as SERVER AUDIT SPECIFICATION");

					PutDropped("SERVER AUDIT", false, drop.Name);
					return true;

				case Statement.DropServerAuditSpecification drop:
					PutDropped("SERVER AUDIT SPECIFICATION", false, drop.Name);
					return true;

				case Statement.DropServerRole drop:
					PutDropped("SERVER ROLE", false, drop.Name);
					return true;

				case Statement.DropWorkloadGroup drop:
					PutDropped("WORKLOAD GROUP", false, drop.Name);
					return true;

				case Statement.DropAsymmetricKey drop:
					PutDropped("ASYMMETRIC KEY", false, drop.Name);

					if (drop.RemoveProviderKey)
						Word("REMOVE PROVIDER KEY");

					return true;

				case Statement.DropSymmetricKey drop:
					PutDropped("SYMMETRIC KEY", false, drop.Name);

					if (drop.RemoveProviderKey)
						Word("REMOVE PROVIDER KEY");

					return true;

				case Statement.DropExternalLanguage drop:
					PutDropped("EXTERNAL LANGUAGE", false, drop.Name);
					PutAuthorization(drop.Authorization);
					return true;

				case Statement.DropDatabase drop:
					if (drop.Names.Count == 0)
						throw Empty(drop, "Names");

					Word("DROP DATABASE");
					PutIfExists(drop.IfExists);
					Each(drop.Names, PutIdentifier);
					return true;

				// T-SQL: DROP ASSEMBLY (Transact-SQL). IF EXISTS, the names as a table's, and WITH NO DEPENDENTS.
				case Statement.DropAssembly drop:
					PutDropped(drop, "ASSEMBLY", drop.IfExists, drop.Names, led: false);

					if (drop.NoDependents)
						Word("WITH NO DEPENDENTS");

					return true;

				// T-SQL: DROP EVENT SESSION (Transact-SQL). One name of one part, and where it lives.
				case Statement.DropEventSession drop:
					PutDropped("EVENT SESSION", false, drop.Name);
					Word(drop.On switch
					{
						EventScope.Server   => "ON SERVER",
						EventScope.Database => "ON DATABASE",
						_                   => throw NoText(drop.On),
					});
					return true;

				// T-SQL: DROP EVENT NOTIFICATION (Transact-SQL). Names of one part, and what they were on.
				case Statement.DropEventNotification drop:
					if (drop.Names.Count == 0)
						throw Empty(drop, "Names");

					Word("DROP EVENT NOTIFICATION");
					Each(drop.Names, PutIdentifier);

					switch (drop.On)
					{
						case NotificationScope.Server:
							Word("ON SERVER");
							break;

						case NotificationScope.Database:
							Word("ON DATABASE");
							break;

						case NotificationScope.Queue queue:
							Word("ON QUEUE");
							PutName(queue.Name);
							break;

						default:
							throw Unwritten(drop.On);
					}

					return true;

				// T-SQL: DROP EXTERNAL LIBRARY (Transact-SQL). One name of one part, and its owner.
				case Statement.DropExternalLibrary drop:
					PutDropped("EXTERNAL LIBRARY", false, drop.Name);
					PutAuthorization(drop.Authorization);
					return true;

				// T-SQL: DROP FULLTEXT INDEX (Transact-SQL). No name of its own: the table it is on.
				case Statement.DropFullTextIndex drop:
					Word("DROP FULLTEXT INDEX ON");
					PutName(drop.Table);
					return true;

				// T-SQL: DROP SENSITIVITY CLASSIFICATION (Transact-SQL). The columns it is dropped from.
				case Statement.DropSensitivityClassification drop:
					if (drop.Objects.Count == 0)
						throw Empty(drop, "Objects");

					Word("DROP SENSITIVITY CLASSIFICATION FROM");
					Each(drop.Objects, PutName);
					return true;

				// T-SQL: DROP MASTER KEY (Transact-SQL), DROP DATABASE ENCRYPTION KEY (Transact-SQL). One of each a database.
				case Statement.DropMasterKey:
					Word("DROP MASTER KEY");
					return true;

				case Statement.DropDatabaseEncryptionKey:
					Word("DROP DATABASE ENCRYPTION KEY");
					return true;

				case Statement.DropIndex drop:
					PutDropIndex(drop);
					return true;

				case Statement.DropSignature drop:
					PutDropSignature(drop);
					return true;

				default:
					return false;
			}
		}

		/// <summary>The words of a kind, <c>IF EXISTS</c> where the kind reads it, and its names as a table's.</summary>
		/// <remarks>
		/// A name that begins with a part (<c>DropLedName</c>, an XML schema collection's and a workload classifier's) may
		/// leave a part empty after its first and never its first: <c>DROP XML SCHEMA COLLECTION .x</c> is <c>Msg 102</c>.
		/// </remarks>
		void PutDropped(ISqlNode owner, string what, bool ifExists, SqlList<QualifiedName> names, bool led)
		{
			if (names.Count == 0)
				throw Empty(owner, "Names");

			if (led && names.Any(static one => one.Parts is [{ Text.Length: 0 }, ..]))
				throw Misplaced(owner, "Names", "with a name whose first part is empty, which DropLedName does not read");

			Word("DROP " + what);
			PutIfExists(ifExists);
			Each(names, PutName);
		}

		/// <summary>The words of a kind, <c>IF EXISTS</c> where the kind reads it, and its one name of one part.</summary>
		void PutDropped(string what, bool ifExists, Identifier name)
		{
			Word("DROP " + what);
			PutIfExists(ifExists);
			PutIdentifier(name);
		}

		void PutAuthorization(Identifier? owner)
		{
			if (owner is null)
				return;

			Word("AUTHORIZATION");
			PutIdentifier(owner);
		}

		/// <summary>
		/// T-SQL: DROP INDEX (Transact-SQL). <c>IF EXISTS</c>, then each index by its name and what it is on, with the
		/// options it is dropped with, or by one qualified name.
		/// </summary>
		void PutDropIndex(Statement.DropIndex drop)
		{
			if (drop.Indexes.Count == 0)
				throw Empty(drop, "Indexes");

			Word("DROP INDEX");
			PutIfExists(drop.IfExists);
			Each(drop.Indexes, index =>
			{
				switch (index)
				{
					case DroppedIndex.Named named:
						PutIdentifier(named.Name);
						Word("ON");
						PutName(named.On);
						PutDropIndexOptions(named, named.Options);
						break;

					case DroppedIndex.Qualified qualified:
						PutName(qualified.Name);
						break;

					default:
						throw Unwritten(index);
				}
			});
		}

		/// <summary>
		/// T-SQL: DROP INDEX (Transact-SQL). <c>WITH (…)</c>: the options of <c>DropIndexOption</c>, each once (<c>Msg 153</c>)
		/// and each in its one form — <c>MAXDOP = n</c>, <c>ONLINE = ON</c>, <c>MOVE TO</c> a place, <c>FILESTREAM_ON</c> a
		/// place, <c>DATA_COMPRESSION = PAGE</c> and its partitions, <c>WAIT_AT_LOW_PRIORITY (…)</c>; any other is
		/// <c>Msg 155</c> or <c>102</c>.
		/// </summary>
		void PutDropIndexOptions(ISqlNode owner, SqlList<Option> options)
		{
			if (options.Count == 0)
				return;

			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (var option in options)
				if (!seen.Add(option.Name.Text))
					throw Misplaced(owner, "Options", $"with {option.Name.Text} twice, which DropIndexOptions refuses (Msg 153)");

			Word("WITH");
			Open();
			Each(options, PutDropIndexOption);
			Close();
		}

		void PutDropIndexOption(Option option)
		{
			var name = option.Name.Style == IdentifierStyle.Regular ? option.Name.Text.ToUpperInvariant() : "";

			if (option.Order is not null || option.EqualsOmitted || option.Words.Count > 0 && name != "MOVE"
				|| option.Partitions is not null && name != "DATA_COMPRESSION" || option.Options.Count > 0 && name != "WAIT_AT_LOW_PRIORITY")
				throw Misplaced(option, "a part", $"that {name} does not take in DROP INDEX's options");

			switch (name)
			{
				// MAXDOP = digits.
				case "MAXDOP" when option.Value is LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger, Text: { Length: > 0 } digits } && digits.All(static one => one is >= '0' and <= '9'):
				// ONLINE = ON | OFF.
				case "ONLINE" when KeyWord(option.Value) is "ON" or "OFF":
					PutIdentifier(option.Name);
					Word("=");
					PutExpression(option.Value!);
					break;

				// DATA_COMPRESSION = NONE | ROW | PAGE | COLUMNSTORE | COLUMNSTORE_ARCHIVE [ON PARTITIONS (…)].
				case "DATA_COMPRESSION" when KeyWord(option.Value) is "NONE" or "ROW" or "PAGE" or "COLUMNSTORE" or "COLUMNSTORE_ARCHIVE":
					PutIdentifier(option.Name);
					Word("=");
					PutExpression(option.Value!);
					PutRanges(option.Partitions);
					break;

				// MOVE TO a place, but not a bare DEFAULT; FILESTREAM_ON a place.
				case "MOVE" when option is { Words: [{ Style: IdentifierStyle.Regular } to] } && Plain(to, "TO") && Place(option.Value) && KeyWord(option.Value) != "DEFAULT":
				case "FILESTREAM_ON" when option.Words.Count == 0 && Place(option.Value):
					PutIdentifier(option.Name);

					foreach (var word in option.Words)
						PutIdentifier(word);

					PutExpression(option.Value!);
					break;

				// WAIT_AT_LOW_PRIORITY (MAX_DURATION = n [MINUTES], ABORT_AFTER_WAIT = NONE | SELF | BLOCKERS).
				case "WAIT_AT_LOW_PRIORITY" when option is { Value: null, Options: [var duration, var abort] } && LowPriority(duration, abort):
					PutIdentifier(option.Name);
					Open();
					Each(option.Options, PutOption);
					Close();
					break;

				default:
					throw Misplaced(option, "Name or Value", "that DropIndexOption does not read (Msg 155, Msg 102)");
			}
		}

		/// <summary>The word a one-part plain reference is, upper-cased, or null.</summary>
		static string? KeyWord(Expression? value)
		{
			return value is Expression.Reference { Name.Parts: [{ Style: IdentifierStyle.Regular } word] } ? word.Text.ToUpperInvariant() : null;
		}

		/// <summary>
		/// T-SQL: DROP INDEX (Transact-SQL). Where rows or streams go (<c>PlacementTarget</c>): a filegroup or a scheme by a
		/// name of one part, or a string. A scheme with its column (<c>MOVE TO ps (a)</c>, <c>MOVE TO 'fg' (a)</c>) has no
		/// value of an option that holds it yet, and is refused.
		/// </summary>
		static bool Place(Expression? value)
		{
			return value switch
			{
				Expression.Reference { Name.Parts.Count: 1 } => true,
				LiteralValue.String { Kind: StringLiteralKind.Character, CharacterSet: null, UnicodeEscape: null } => true,
				_ => false,
			};
		}

		/// <summary><c>MAX_DURATION = n [MINUTES]</c> then <c>ABORT_AFTER_WAIT = NONE | SELF | BLOCKERS</c>, and nothing else.</summary>
		static bool LowPriority(Option duration, Option abort)
		{
			// Minutes = Number & "MINUTES"i?, and Number is an unsigned number literal.
			var minutes = duration.Value switch
			{
				Expression.Measured { Value: var number, Unit: var unit } => IsNumber(number) && Plain(unit, "MINUTES"),
				var number                                                => IsNumber(number),
			};

			return minutes
				&& duration is { Options.Count: 0, Words.Count: 0, Partitions: null, Order: null, EqualsOmitted: false } && Plain(duration.Name, "MAX_DURATION")
				&& abort is { Options.Count: 0, Words.Count: 0, Partitions: null, Order: null, EqualsOmitted: false } && Plain(abort.Name, "ABORT_AFTER_WAIT")
				&& KeyWord(abort.Value) is "NONE" or "SELF" or "BLOCKERS";
		}

		/// <summary>
		/// T-SQL: DROP SIGNATURE (Transact-SQL). <c>[COUNTER] SIGNATURE FROM [class::] module BY</c> and what signed it: a
		/// certificate or an asymmetric key, by a name of one part.
		/// </summary>
		void PutDropSignature(Statement.DropSignature drop)
		{
			if (drop.By.Count == 0)
				throw Empty(drop, "By");

			if (drop.Class is { } written && written is not (SecurableClass.Object or SecurableClass.Assembly or SecurableClass.Database))
				throw Misplaced(drop, "Class", "other than OBJECT, ASSEMBLY or DATABASE, which SignedModule does not read");

			if (drop.By.Any(static one => one is SignedBy.Certificate { Name.Parts.Count: not 1 } or SignedBy.AsymmetricKey { Name.Parts.Count: not 1 }))
				throw Misplaced(drop, "By", "with a name of more than one part, which SignatureCrypto does not read");

			Word(drop.Counter ? "DROP COUNTER SIGNATURE FROM" : "DROP SIGNATURE FROM");

			if (drop.Class is { } one)
			{
				Word(PermissionWords.Text(one) ?? throw NoText(one));
				Tight("::");
				Hold();
			}

			PutName(drop.Module);
			Word("BY");
			Each(drop.By, signed =>
			{
				switch (signed)
				{
					case SignedBy.Certificate certificate:
						Word("CERTIFICATE");
						PutName(certificate.Name);
						break;

					case SignedBy.AsymmetricKey key:
						Word("ASYMMETRIC KEY");
						PutName(key.Name);
						break;

					default:
						throw Unwritten(signed);
				}
			});
		}
	}
}
