using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

static partial class Build
{
	/// <summary>
	/// The sixty-five <c>DROP</c> statements. The standard's take a list, <c>IF EXISTS</c> and a behavior that
	/// may be left out (decided 2026-09-15); the rest have a record each (SqlAst.TransactSql.Drop.cs).
	/// </summary>
	/// <remarks>
	/// Their rules build the common tree (<see cref="Dropped"/>, <see cref="Signature"/>), so the family carries nothing:
	/// it owns the records those rules built before, under which their statements are counted.
	/// </remarks>
	internal static class Drop
	{
		/// <summary>
		/// What a <c>DROP</c> rule hands its builder: the T-SQL tree's record the statement was, the names, what was written
		/// after them and whether <c>IF EXISTS</c> was.
		/// </summary>
		sealed record Dropping(string Record, SqlList<Ast.Expression> Names, string? Tail, bool IfExists) : Given(Record);

		/// <summary>
		/// The <c>DROP</c> the words name, on the common tree: the kind as it was written, the names, what was written after
		/// them and the <c>IF EXISTS</c>, where it was.
		/// </summary>
		internal static Statement Dropped(string kind, SqlList<Ast.Expression>? some, string? tail = null, string? ifExists = null)
		{
			// The words as tokens: a comment between two of them (`DROP SERVER /*x*/ AUDIT a`) is no part of the kind.
			var what    = Syntax.Squared(string.Join(" ", SqlTokens.Of(kind).Select(static one => one.Text)));
			var removal = new Dropping(Record(what), some ?? [], Syntax.Tail(tail), ifExists is not null);

			return Hold(removal, what, static (removal, what) => Made(what, removal));
		}

		static Ast.Statement Made(string what, Dropping removal)
		{
			return what switch
			{
				"TABLE"                        => new Ast.Statement.DropTable { Names = Names(removal), IfExists = removal.IfExists },
				"VIEW"                         => new Ast.Statement.DropView { Names = Names(removal), IfExists = removal.IfExists },
				"SCHEMA"                       => new Ast.Statement.DropSchema { Names = Names(removal), IfExists = removal.IfExists },
				"SEQUENCE"                     => new Ast.Statement.DropSequence { Names = Names(removal), IfExists = removal.IfExists },
				"TYPE"                         => new Ast.Statement.DropType { Names = Names(removal), IfExists = removal.IfExists },
				"ROLE"                         => new Ast.Statement.DropRole { Names = Each(Names(removal), one => Single(removal, one)), IfExists = removal.IfExists },
				"FUNCTION"                     => Routines(removal, Ast.RoutineKind.Function, false),
				"PROCEDURE" or "PROC"          => Routines(removal, Ast.RoutineKind.Procedure, what == "PROC"),
				"TRIGGER"                      => Trigger(removal),
				"AGGREGATE"                    => new Ast.Statement.DropAggregate(Names(removal)) { IfExists = removal.IfExists },
				"DEFAULT"                      => new Ast.Statement.DropDefault(Names(removal)) { IfExists = removal.IfExists },
				"RULE"                         => new Ast.Statement.DropRule(Names(removal)) { IfExists = removal.IfExists },
				"SECURITY POLICY"              => new Ast.Statement.DropSecurityPolicy(Names(removal)) { IfExists = removal.IfExists },
				"SYNONYM"                      => new Ast.Statement.DropSynonym(Names(removal)) { IfExists = removal.IfExists },
				"USER"                         => new Ast.Statement.DropUser(Part(removal)) { IfExists = removal.IfExists },
				"CONTRACT"                     => new Ast.Statement.DropContract(One(removal)),
				"EXTERNAL TABLE"               => new Ast.Statement.DropExternalTable(One(removal)),
				"MESSAGE TYPE"                 => new Ast.Statement.DropMessageType(One(removal)),
				"QUEUE"                        => new Ast.Statement.DropQueue(One(removal)),
				"REMOTE SERVICE BINDING"       => new Ast.Statement.DropRemoteServiceBinding(One(removal)),
				"ROUTE"                        => new Ast.Statement.DropRoute(One(removal)),
				"SERVICE"                      => new Ast.Statement.DropService(One(removal)),
				"XML SCHEMA COLLECTION"        => new Ast.Statement.DropXmlSchemaCollection(One(removal)),
				"STATISTICS"                   => new Ast.Statement.DropStatistics(Names(removal)),
				"WORKLOAD CLASSIFIER"          => new Ast.Statement.DropWorkloadClassifier(Names(removal)),
				"APPLICATION ROLE"             => new Ast.Statement.DropApplicationRole(Part(removal)),
				"AVAILABILITY GROUP"           => new Ast.Statement.DropAvailabilityGroup(Part(removal)),
				"BROKER PRIORITY"              => new Ast.Statement.DropBrokerPriority(Part(removal)),
				"CERTIFICATE"                  => new Ast.Statement.DropCertificate(Part(removal)),
				"COLUMN ENCRYPTION KEY"        => new Ast.Statement.DropColumnEncryptionKey(Part(removal)),
				"COLUMN MASTER KEY"            => new Ast.Statement.DropColumnMasterKey(Part(removal)),
				"CREDENTIAL"                   => new Ast.Statement.DropCredential(Part(removal)),
				"CRYPTOGRAPHIC PROVIDER"       => new Ast.Statement.DropCryptographicProvider(Part(removal)),
				"DATABASE AUDIT SPECIFICATION" => new Ast.Statement.DropDatabaseAuditSpecification(Part(removal)),
				"DATABASE SCOPED CREDENTIAL"   => new Ast.Statement.DropDatabaseScopedCredential(Part(removal)),
				"ENDPOINT"                     => new Ast.Statement.DropEndpoint(Part(removal)),
				"EXTERNAL DATA SOURCE"         => new Ast.Statement.DropExternalDataSource(Part(removal)),
				"EXTERNAL FILE FORMAT"         => new Ast.Statement.DropExternalFileFormat(Part(removal)),
				"EXTERNAL MODEL"               => new Ast.Statement.DropExternalModel(Part(removal)),
				"EXTERNAL RESOURCE POOL"       => new Ast.Statement.DropExternalResourcePool(Part(removal)),
				"FULLTEXT CATALOG"             => new Ast.Statement.DropFullTextCatalog(Part(removal)),
				"FULLTEXT STOPLIST"            => new Ast.Statement.DropFullTextStopList(Part(removal)),
				"LOGIN"                        => new Ast.Statement.DropLogin(Part(removal)),
				"PARTITION FUNCTION"           => new Ast.Statement.DropPartitionFunction(Part(removal)),
				"PARTITION SCHEME"             => new Ast.Statement.DropPartitionScheme(Part(removal)),
				"RESOURCE POOL"                => new Ast.Statement.DropResourcePool(Part(removal)),
				"SEARCH PROPERTY LIST"         => new Ast.Statement.DropSearchPropertyList(Part(removal)),
				"SERVER AUDIT"                 => new Ast.Statement.DropServerAudit(Part(removal)),
				"SERVER AUDIT SPECIFICATION"   => new Ast.Statement.DropServerAuditSpecification(Part(removal)),
				"SERVER ROLE"                  => new Ast.Statement.DropServerRole(Part(removal)),
				"WORKLOAD GROUP"               => new Ast.Statement.DropWorkloadGroup(Part(removal)),
				"ASYMMETRIC KEY"               => new Ast.Statement.DropAsymmetricKey(Part(removal with { Tail = null })) { RemoveProviderKey = Said(removal, "REMOVE PROVIDER KEY") },
				"SYMMETRIC KEY"                => new Ast.Statement.DropSymmetricKey(Part(removal with { Tail = null })) { RemoveProviderKey = Said(removal, "REMOVE PROVIDER KEY") },
				"EXTERNAL LANGUAGE"            => new Ast.Statement.DropExternalLanguage(Part(removal with { Tail = null })) { Authorization = Owner(removal) },
				"DATABASE"                     => new Ast.Statement.DropDatabase(Each(Names(removal), one => Single(removal, one))) { IfExists = removal.IfExists },
				"ASSEMBLY"                     => new Ast.Statement.DropAssembly(Names(removal with { Tail = null }), removal.IfExists, Said(removal, "WITH NO DEPENDENTS")),
				"EVENT SESSION"                => new Ast.Statement.DropEventSession(Part(removal with { Tail = null }), EventScope(removal)),
				"EVENT NOTIFICATION"           => new Ast.Statement.DropEventNotification(Each(Names(removal with { Tail = null }), one => Single(removal, one)), NotificationScope(removal)),
				"EXTERNAL LIBRARY"             => new Ast.Statement.DropExternalLibrary(Part(removal with { Tail = null }), Owner(removal)),
				"FULLTEXT INDEX"               => new Ast.Statement.DropFullTextIndex(One(removal)),
				"SENSITIVITY CLASSIFICATION"   => new Ast.Statement.DropSensitivityClassification(Names(removal)),
				"MASTER KEY"                   => new Ast.Statement.DropMasterKey(),
				"DATABASE ENCRYPTION KEY"      => new Ast.Statement.DropDatabaseEncryptionKey(),
				"INDEX"                        => Index(removal),
				_ => throw new ArgumentOutOfRangeException(nameof(what), what, "The grammar reads this after DROP and the tree has no record for it."),
			};
		}

		/// <summary>The T-SQL tree's record for a kind, which the gaps of its statement are counted under.</summary>
		static string Record(string what)
		{
			return what switch
			{
				"AGGREGATE"                    => "Statement.DropAggregate",
				"APPLICATION ROLE"             => "Statement.DropApplicationRole",
				"AVAILABILITY GROUP"           => "Statement.DropAvailabilityGroup",
				"BROKER PRIORITY"              => "Statement.DropBrokerPriority",
				"CERTIFICATE"                  => "Statement.DropCertificate",
				"COLUMN ENCRYPTION KEY"        => "Statement.DropColumnEncryptionKey",
				"COLUMN MASTER KEY"            => "Statement.DropColumnMasterKey",
				"CONTRACT"                     => "Statement.DropContract",
				"CREDENTIAL"                   => "Statement.DropCredential",
				"CRYPTOGRAPHIC PROVIDER"       => "Statement.DropCryptographicProvider",
				"DATABASE AUDIT SPECIFICATION" => "Statement.DropDatabaseAuditSpecification",
				"DATABASE SCOPED CREDENTIAL"   => "Statement.DropDatabaseScopedCredential",
				"DATABASE"                     => "Statement.DropDatabase",
				"DEFAULT"                      => "Statement.DropDefault",
				"ENDPOINT"                     => "Statement.DropEndpoint",
				"EXTERNAL DATA SOURCE"         => "Statement.DropExternalDataSource",
				"EXTERNAL FILE FORMAT"         => "Statement.DropExternalFileFormat",
				"EXTERNAL LANGUAGE"            => "Statement.DropExternalLanguage",
				"EXTERNAL MODEL"               => "Statement.DropExternalModel",
				"EXTERNAL RESOURCE POOL"       => "Statement.DropExternalResourcePool",
				"EXTERNAL TABLE"               => "Statement.DropExternalTable",
				"FULLTEXT CATALOG"             => "Statement.DropFulltextCatalog",
				"FULLTEXT STOPLIST"            => "Statement.DropFulltextStoplist",
				"FUNCTION"                     => "Statement.DropFunction",
				"LOGIN"                        => "Statement.DropLogin",
				"MESSAGE TYPE"                 => "Statement.DropMessageType",
				"PARTITION FUNCTION"           => "Statement.DropPartitionFunction",
				"PARTITION SCHEME"             => "Statement.DropPartitionScheme",
				"PROCEDURE" or "PROC"          => "Statement.DropProcedure",
				"QUEUE"                        => "Statement.DropQueue",
				"REMOTE SERVICE BINDING"       => "Statement.DropRemoteServiceBinding",
				"RESOURCE POOL"                => "Statement.DropResourcePool",
				"ROLE"                         => "Statement.DropRole",
				"ROUTE"                        => "Statement.DropRoute",
				"RULE"                         => "Statement.DropRule",
				"SCHEMA"                       => "Statement.DropSchema",
				"SEARCH PROPERTY LIST"         => "Statement.DropSearchPropertyList",
				"SECURITY POLICY"              => "Statement.DropSecurityPolicy",
				"SEQUENCE"                     => "Statement.DropSequence",
				"SERVER AUDIT SPECIFICATION"   => "Statement.DropServerAuditSpecification",
				"SERVER AUDIT"                 => "Statement.DropServerAudit",
				"SERVER ROLE"                  => "Statement.DropServerRole",
				"SERVICE"                      => "Statement.DropService",
				"STATISTICS"                   => "Statement.DropStatistics",
				"SYNONYM"                      => "Statement.DropSynonym",
				"TABLE"                        => "Statement.DropTable",
				"TYPE"                         => "Statement.DropType",
				"USER"                         => "Statement.DropUser",
				"VIEW"                         => "Statement.DropView",
				"WORKLOAD CLASSIFIER"          => "Statement.DropWorkloadClassifier",
				"WORKLOAD GROUP"               => "Statement.DropWorkloadGroup",
				"XML SCHEMA COLLECTION"        => "Statement.DropXmlSchemaCollection",
				"ASYMMETRIC KEY"               => "Statement.DropAsymmetricKey",
				"SYMMETRIC KEY"                => "Statement.DropSymmetricKey",
				"ASSEMBLY"                     => "Statement.DropAssembly",
				"EXTERNAL LIBRARY"             => "Statement.DropExternalLibrary",
				"EVENT SESSION"                => "Statement.DropEventSession",
				"EVENT NOTIFICATION"           => "Statement.DropEventNotification",
				"FULLTEXT INDEX"               => "Statement.DropFulltextIndex",
				"INDEX"                        => "Statement.DropIndex",
				"SENSITIVITY CLASSIFICATION"   => "Statement.DropSensitivityClassification",
				"TRIGGER"                      => "Statement.DropTrigger",
				"MASTER KEY"                   => "Statement.DropMasterKey",
				"DATABASE ENCRYPTION KEY"      => "Statement.DropDatabaseEncryptionKey",
				_ => "Statement.Removal",
			};
		}

		/// <summary>
		/// The names dropped, which the old tree keeps as column references; and nothing after them.
		/// </summary>
		static SqlList<Ast.QualifiedName> Names(Dropping old)
		{
			if (old.Tail is not null)
				throw BuildGap.Open(old, $"inventory {NameOf(old)} (`{old.Tail}` after the names)");

			return Each(old.Names, one => Values.Wording(one) is { } text
				? Values.Name(old, text)
				: throw BuildGap.Open(old, $"inventory {NameOf(old)} (a name that is {NameOf(one)})"));
		}

		static Ast.Identifier Single(object old, Ast.QualifiedName name)
		{
			return name.Parts.Count == 1 ? name.Parts[0] : throw BuildGap.Open(old, "inventory Statement.DropRole (a qualified role name)");
		}

		/// <summary>
		/// <c>DROP FUNCTION</c> and <c>DROP PROCEDURE</c>, whose list is of designators, and whether <c>PROC</c> was written.
		/// </summary>
		static Ast.Statement Routines(Dropping old, Ast.RoutineKind kind, bool proc)
		{
			return new Ast.Statement.DropRoutine
			{
				Routines = Each(Names(old), one => new Ast.RoutineDesignator(kind, one)),
				IfExists = old.IfExists,
				Proc     = proc,
			};
		}

		/// <summary>
		/// <c>DROP TRIGGER</c>, and the scope of a DDL trigger, which the old tree keeps as its tail.
		/// </summary>
		static Ast.Statement Trigger(Dropping old)
		{
			if (old.Tail is not { } tail)
				return new Ast.Statement.DropTrigger { Names = Names(old), IfExists = old.IfExists };

			Ast.TriggerScope scope = Words(tail) switch
			{
				["ON", "DATABASE"]          => Ast.TriggerScope.Database,
				["ON", "ALL", "SERVER"]     => Ast.TriggerScope.AllServer,
				_                           => throw BuildGap.Open(old, $"inventory Statement.DropTrigger (`{tail}`)"),
			};

			// The old tree keeps the scope as written, which compares as the oracle does.
			if (!Same(old, "Statement.DropTrigger.Tail", scope == Ast.TriggerScope.Database ? "ON DATABASE" : "ON ALL SERVER", tail))
				throw BuildGap.Open(old, "Trees.FreeText (Statement.DropTrigger.Tail, a scope not in capitals)");

			return new Ast.Statement.DropTrigger { Names = Names(old with { Tail = null }), IfExists = old.IfExists, On = scope };
		}

		// ── T-SQL's drops ────────────────────────────────────────────────────────────────────────────────

		/// <summary>The one name a kind drops as a table's, and nothing after it.</summary>
		static Ast.QualifiedName One(Dropping old)
		{
			return Names(old) is [var one] ? one : throw BuildGap.Open(old, $"inventory {NameOf(old)} (other than one name)");
		}

		/// <summary>The one name of one part a kind drops, and nothing after it.</summary>
		static Ast.Identifier Part(Dropping old)
		{
			return Single(old, One(old));
		}

		/// <summary>
		/// Whether the words given are the tail, or there is none. The old tree keeps the tail as it was written and the
		/// round trip compares it as the oracle does, so a tail written otherwise than the writer prints it, beyond the case
		/// of its key words and its blanks where it is free text, is a gap.
		/// </summary>
		static bool Said(Dropping old, string words)
		{
			if (old.Tail is not { } tail)
				return false;

			if (string.Join(" ", Words(tail)) != words)
				throw BuildGap.Open(old, $"inventory {NameOf(old)} (`{tail}` after the names)");

			Written(old, NameOf(old) + ".Tail", tail, words);

			return true;
		}

		/// <summary>
		/// A text the old tree keeps as written, which the round trip compares as the oracle does (<see cref="Same"/>): where it
		/// is written otherwise than the writer prints it, the statement is a gap, never a difference.
		/// </summary>
		static void Written(object old, string member, string text, string written)
		{
			if (!Same(old, member, written, text))
				throw BuildGap.Open(old, $"inventory {NameOf(old)} (`{text}` kept as written, against `{written}` as the writer prints it, which the old tree compares exactly)");
		}

		/// <summary>A qualified name read from tokens: its parts and the dots between, as written.</summary>
		static string? Dotted(IEnumerable<SqlTokens.Token> tokens)
		{
			var text = new StringBuilder();
			var dot  = true;

			foreach (var token in tokens)
			{
				if (token is { Kind: SqlTokens.Kind.Punctuation, Text: "." })
					dot = true;
				else if (token.Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited && dot)
					dot = false;
				else
					return null;

				text.Append(token.Text);
			}

			return text.Length == 0 ? null : text.ToString();
		}

		/// <summary><c>AUTHORIZATION owner</c> after an external library's or language's name, or null.</summary>
		static Ast.Identifier? Owner(Dropping old)
		{
			if (old.Tail is not { } tail)
				return null;

			var tokens = SqlTokens.Of(tail);

			if (tokens is not [{ Kind: SqlTokens.Kind.Word, Text: var word }, { Kind: SqlTokens.Kind.Word or SqlTokens.Kind.Delimited } owner] || !word.Equals("AUTHORIZATION", StringComparison.OrdinalIgnoreCase))
				throw BuildGap.Open(old, $"inventory {NameOf(old)} (`{tail}` after the name)");

			Written(old, NameOf(old) + ".Tail", tail, "AUTHORIZATION " + owner.Text);

			return Values.Part(old, owner.Text);
		}

		/// <summary><c>ON SERVER</c> or <c>ON DATABASE</c> after an event session's name.</summary>
		static Ast.EventScope EventScope(Dropping old)
		{
			var scope = old.Tail is { } tail ? string.Join(" ", Words(tail)) : "";

			Ast.EventScope made = scope switch
			{
				"ON SERVER"   => Ast.EventScope.Server,
				"ON DATABASE" => Ast.EventScope.Database,
				_             => throw BuildGap.Open(old, $"inventory Statement.DropEventSession (`{old.Tail}`)"),
			};

			Written(old, NameOf(old) + ".Tail", old.Tail!, scope);

			return made;
		}

		/// <summary><c>ON SERVER</c>, <c>ON DATABASE</c> or <c>ON QUEUE q</c> after an event notification's names.</summary>
		static Ast.NotificationScope NotificationScope(Dropping old)
		{
			var tokens = SqlTokens.Of(old.Tail ?? "");
			var words  = Words(old.Tail ?? "");

			switch (words)
			{
				case ["ON", "SERVER"]:
					Written(old, NameOf(old) + ".Tail", old.Tail!, "ON SERVER");

					return new Ast.NotificationScope.Server();

				case ["ON", "DATABASE"]:
					Written(old, NameOf(old) + ".Tail", old.Tail!, "ON DATABASE");

					return new Ast.NotificationScope.Database();

				case ["ON", "QUEUE", ..] when Dotted(tokens.Skip(2)) is { } queue:
					Written(old, NameOf(old) + ".Tail", old.Tail!, "ON QUEUE " + queue);

					return new Ast.NotificationScope.Queue(Values.Name(old, queue));

				default:
					throw BuildGap.Open(old, $"inventory Statement.DropEventNotification (`{old.Tail}`)");
			}
		}

		/// <summary>
		/// DROP INDEX: each index the old tree keeps as one text — <c>i ON t WITH (…)</c> as written, or a qualified name —
		/// read again from its tokens.
		/// </summary>
		static Ast.Statement Index(Dropping old)
		{
			if (old.Tail is not null)
				throw BuildGap.Open(old, $"inventory Statement.DropIndex (`{old.Tail}` after the names)");

			return new Ast.Statement.DropIndex(Each(old.Names, one => Values.Wording(one) is { } text
				? DroppedIndex(old, text)
				: throw BuildGap.Open(old, $"inventory Statement.DropIndex (a name that is {NameOf(one)})")), old.IfExists);
		}

		static Ast.DroppedIndex DroppedIndex(object old, string text)
		{
			var tokens = SqlTokens.Of(text);

			if (tokens is not [{ Kind: SqlTokens.Kind.Word or SqlTokens.Kind.Delimited } name, { Kind: SqlTokens.Kind.Word, Text: var on }, ..] || !on.Equals("ON", StringComparison.OrdinalIgnoreCase))
				return new Ast.DroppedIndex.Qualified(Values.Name(old, text));

			var with  = tokens.FindIndex(2, static one => one is { Kind: SqlTokens.Kind.Word } && one.Text.Equals("WITH", StringComparison.OrdinalIgnoreCase));
			var table = Dotted(with < 0 ? tokens.Skip(2) : tokens.Skip(2).Take(with - 2))
				?? throw BuildGap.Open(old, $"inventory Statement.DropIndex (`{text}`)");

			var options = new List<Ast.Option>();
			var written = new List<string>();

			if (with >= 0)
			{
				if (tokens.Count < with + 3 || tokens[with + 1].Text != "(" || tokens[^1].Text != ")")
					throw BuildGap.Open(old, $"inventory Statement.DropIndex (`{text}`)");

				foreach (var option in Listed(tokens.Skip(with + 2).Take(tokens.Count - with - 3).ToList()))
				{
					var (made, spelled) = DropIndexOption(old, option);

					options.Add(made);
					written.Add(spelled);
				}
			}

			Written(old, BuildText.DropIndexName, text, $"{name.Text} ON {table}" + (with < 0 ? "" : $" WITH ({string.Join(", ", written)})"));

			return new Ast.DroppedIndex.Named(Values.Part(old, name.Text), Values.Name(old, table), options.ToSqlList());
		}

		/// <summary>A list of tokens cut at its commas outside brackets.</summary>
		static List<List<SqlTokens.Token>> Listed(List<SqlTokens.Token> tokens)
		{
			var pieces = new List<List<SqlTokens.Token>> { new() };
			var depth  = 0;

			foreach (var token in tokens)
			{
				if (token.Text == "(")
					depth++;
				else if (token.Text == ")")
					depth--;

				if (depth == 0 && token is { Kind: SqlTokens.Kind.Punctuation, Text: "," })
					pieces.Add([]);
				else
					pieces[^1].Add(token);
			}

			return pieces;
		}

		/// <summary>
		/// One option of DROP INDEX's <c>WITH (…)</c> (<c>DropIndexOption</c>), and how the writer prints it: the old tree
		/// keeps the option list as written.
		/// </summary>
		static (Ast.Option Made, string Written) DropIndexOption(object old, List<SqlTokens.Token> tokens)
		{
			var words = tokens.Select(static one => one.Kind == SqlTokens.Kind.Word ? one.Text.ToUpperInvariant() : one.Text).ToArray();
			var name  = new Ast.Identifier(tokens[0].Text);

			switch (words)
			{
				case ["MAXDOP", "=", _] when tokens[2].Kind == SqlTokens.Kind.Number:
					return (new Ast.Option(name, Values.Of(Values.Number(tokens[2].Text)), []), $"{tokens[0].Text} = {tokens[2].Text}");

				case ["ONLINE" or "DATA_COMPRESSION", "=", _] when tokens[2].Kind == SqlTokens.Kind.Word:
					return (new Ast.Option(name, Values.Of(Values.Reference(tokens[2].Text)), []), $"{tokens[0].Text} = {tokens[2].Text}");

				case ["DATA_COMPRESSION", "=", _, "ON", "PARTITIONS", "(", .., ")"] when tokens[2].Kind == SqlTokens.Kind.Word:
					var (ranges, spelled) = Ranges(old, tokens.Skip(6).Take(tokens.Count - 7).ToList());

					return (new Ast.Option(name, Values.Of(Values.Reference(tokens[2].Text)), []) { Partitions = ranges },
						$"{tokens[0].Text} = {tokens[2].Text} ON PARTITIONS ({spelled})");

				case ["MOVE", "TO", ..]:
					var (moved, to) = Place(old, Ast.PlacementKind.MoveTo, tokens.Skip(2).ToList());

					return (new Ast.Option(name, null, []) { Words = [new Ast.Identifier(tokens[1].Text)], Place = moved }, $"{tokens[0].Text} {tokens[1].Text} {to}");

				case ["FILESTREAM_ON", ..]:
					var (streamed, on) = Place(old, Ast.PlacementKind.FilestreamOn, tokens.Skip(1).ToList());

					return (new Ast.Option(name, null, []) { Place = streamed }, $"{tokens[0].Text} {on}");

				case ["WAIT_AT_LOW_PRIORITY", "(", "MAX_DURATION", "=", _, ..] when tokens[4].Kind == SqlTokens.Kind.Number:
					var minutes = words is [_, _, _, _, _, "MINUTES", ",", ..];
					var at      = minutes ? 7 : 6;

					if (words.Length != at + 4 || words[at - 1] != "," || words[at] != "ABORT_AFTER_WAIT" || words[at + 1] != "=" || words[^1] != ")")
						break;

					var number   = Values.Of(Values.Number(tokens[4].Text));
					var duration = new Ast.Option(new Ast.Identifier(tokens[2].Text), minutes ? new Ast.Expression.Measured(number, new Ast.Identifier(tokens[5].Text)) : number, []);
					var abort    = new Ast.Option(new Ast.Identifier(tokens[at].Text), Values.Of(Values.Reference(tokens[at + 2].Text)), []);

					return (new Ast.Option(name, null, [duration, abort]),
						$"{tokens[0].Text} ({tokens[2].Text} = {tokens[4].Text}{(minutes ? " " + tokens[5].Text : "")}, {tokens[at].Text} = {tokens[at + 2].Text})");
			}

			throw BuildGap.Open(old, $"inventory Statement.DropIndex (the option `{string.Join(" ", tokens.Select(static one => one.Text))}`)");
		}

		/// <summary>
		/// Where an index's rows or streams go (<c>PlacementTarget</c>): a name of one part or a string, with a scheme's column in
		/// brackets for MOVE TO, and how the writer prints it.
		/// </summary>
		static (Ast.Placement Made, string Written) Place(object old, Ast.PlacementKind kind, List<SqlTokens.Token> tokens)
		{
			switch (tokens)
			{
				case [{ Kind: SqlTokens.Kind.Word or SqlTokens.Kind.Delimited or SqlTokens.Kind.String } one]:
					return (new Ast.Placement(kind, Tables.PlaceTarget(old, one.Text), null), one.Text);

				case [{ Kind: SqlTokens.Kind.Word or SqlTokens.Kind.Delimited or SqlTokens.Kind.String } one, { Text: "(" }, { Kind: SqlTokens.Kind.Word or SqlTokens.Kind.Delimited } column, { Text: ")" }] when kind == Ast.PlacementKind.MoveTo:
					return (new Ast.Placement(kind, Tables.PlaceTarget(old, one.Text), [Values.Part(old, column.Text)]), $"{one.Text} ({column.Text})");
			}

			throw BuildGap.Open(old, $"inventory Statement.DropIndex (a place `{string.Join(" ", tokens.Select(static one => one.Text))}`, which no value of an option holds)");
		}

		/// <summary>
		/// The partitions of <c>ON PARTITIONS (1, 3 TO 5)</c>, numbers each or two with TO between, and how the writer
		/// prints them.
		/// </summary>
		static (SqlList<Ast.PartitionRange> Ranges, string Written) Ranges(object old, List<SqlTokens.Token> tokens)
		{
			var ranges  = new List<Ast.PartitionRange>();
			var written = new List<string>();

			foreach (var range in Listed(tokens))
			{
				switch (range)
				{
					case [{ Kind: SqlTokens.Kind.Number } from]:
						ranges.Add(new Ast.PartitionRange(Values.Of(Values.Number(from.Text)), null));
						written.Add(from.Text);
						break;

					case [{ Kind: SqlTokens.Kind.Number } from, { Kind: SqlTokens.Kind.Word, Text: var to }, { Kind: SqlTokens.Kind.Number } until] when to.Equals("TO", StringComparison.OrdinalIgnoreCase):
						ranges.Add(new Ast.PartitionRange(Values.Of(Values.Number(from.Text)), Values.Of(Values.Number(until.Text))));
						written.Add($"{from.Text} TO {until.Text}");
						break;

					default:
						throw BuildGap.Open(old, "inventory Statement.DropIndex (partitions other than numbers)");
				}
			}

			return (ranges.ToSqlList(), string.Join(", ", written));
		}

		/// <summary>
		/// DROP SIGNATURE: the module, which the old tree keeps as text with its class before <c>::</c>, and what signed it, kept as
		/// words; both are read as ADD SIGNATURE's are (<see cref="Security.SignedModule"/>).
		/// </summary>
		internal static Statement Signature(SqlList<Ast.Expression> names, string? from, bool counter)
		{
			var old = new Dropping("Statement.DropSignature", names, null, false);

			return Hold(old, (from, counter), static (old, state) =>
			{
				var (kind, module) = Security.SignedModule(old, state.from, "Statement.DropSignature.From");

				return new Ast.Statement.DropSignature(state.counter, module, Security.Signers(old, old.Names)) { Class = kind };
			});
		}
	}
}
