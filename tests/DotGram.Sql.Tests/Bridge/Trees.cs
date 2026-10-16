using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotGram.Sql.Tests;

/// <summary>
/// What the bridge's oracle asks of two trees that are not simply equal, and the nodes an old one holds.
/// </summary>
/// <remarks>
/// <para>
/// <b>Key words, not names.</b> The old tree keeps some key words as the text they were written in —
/// <c>distinct</c>, <c>current_user</c>, a cast's <c>varchar (10)</c> — and the new tree keeps the meaning,
/// which <c>Sql2023Writer</c> prints in capitals. So a statement written in lower case comes back unequal by
/// record equality and equal in everything it says. The members that hold such text are a closed list,
/// <see cref="FreeText"/>, and are compared token by token (<see cref="SqlTokens"/>): blanks only between
/// tokens, a word without case only where both are in the closed list <see cref="KeyWords"/> the writer
/// capitalises, the <c>N</c> of a national string without case, and every other token — a name in brackets
/// or quotes, a string, a number, any other word — exactly. Every other member compares exactly. A statement
/// that passes only so is counted as converted and, per member, apart (<c>BridgeOracle.Outcome.ByCase</c>).
/// </para>
/// </remarks>
static class Trees
{
	/// <summary>
	/// The members of the old tree that hold text with key words in it, as <c>Record.Member</c>.
	/// </summary>
	public static readonly HashSet<string> FreeText = new(StringComparer.Ordinal)
	{
		"Query.Specification.Quantifier",
		"Expression.Quantified.Quantifier",
		"Expression.Match.Qualifier",
		"Expression.GraphMatch.Pattern",
		"Expression.RoutineInvocation.Word",
		"Expression.OdbcEscape.Kind",
		"Expression.RoutineInvocation.Name",
		"Expression.WindowFunction.Within",
		"Expression.Literal.Text",
		"Clause.Top.With",
		"Clause.For.Kind",
		"Clause.For.Options",
		"Clause.Hint.Text",
		"Clause.JsonColumn.Type",
		"Clause.ColumnDefinition.Type",
		"Expression.Hinted.Words",
		"Expression.NamedArgument.Name",
		"Expression.Prefixed.Word",
		"Clause.GroupBy.With",
		"Clause.SystemTime.Kind",
		"Clause.TableSample.Unit",
		"Clause.Option.Name",
		"Clause.Window.Frame",
		"Statement.Transaction.Kind",
		"Statement.Transaction.Word",
		"Statement.Transaction.Tail",
		"Statement.Insert.Into",
		"Statement.CursorAction.Verb",
		"Statement.Fetch.Orientation",
		"Statement.SequenceDefinition.Tail",
		"Statement.SchemaDefinition.Tail",
		"Statement.TableDefinition.Kind",
		"Statement.AlterTable.Tail",
		"Statement.StatisticsDefinition.Tail",
		"Statement.UpdateStatistics.Tail",
		"Statement.TypeDefinition.Tail",
		"Statement.SynonymDefinition.Tail",
		"Statement.FullTextIndexDefinition.Tail",
		"Statement.AlterFullTextIndex.Tail",
		"Statement.CreateIndex.Using",
		"Clause.Option.Partitions",
		"Clause.Dropped.Tail",
		"TableReference.Joined.Hint",
		"SetExpression.Transactions.IsolationLevel.Level",
		// The writer round's batch: types, routine and module tails, and the tails of the drops, written as the writer
		// prints them but for the case of their key words and their blanks.
		"Clause.VariableDeclaration.Type",
		"Clause.ParameterDeclaration.Type",
		"Statement.CreateFunction.Returns",
		"SetExpression.Statistics.Report.Kind",
		"Statement.RaiseError.Tail",
		"Statement.Execute.Tail",
		"Statement.AssemblyDefinition.Tail",
		"Statement.RuleDefinition.Tail",
		"Statement.DefaultDefinition.Tail",
		"Statement.AggregateDefinition.Tail",
		"Statement.DropAsymmetricKey.Tail",
		"Statement.DropSymmetricKey.Tail",
		"Statement.DropAssembly.Tail",
		"Statement.DropTrigger.Tail",
		"Statement.DropEventSession.Tail",
		"Statement.DropEventNotification.Tail",
		"Statement.DropSignature.From",
		// The Security family: a permission and the securable it is on, which the old tree keeps as the words written.
		"Statement.Grant.Privileges",
		"Statement.Grant.On",
		"Statement.Deny.Privileges",
		"Statement.Revoke.Privileges",
		// What follows the name of a role, a login, a user, a schema's transfer and an authorization, kept as written.
		"Statement.CreateRole.Tail",
		"Statement.AlterRole.Tail",
		"Statement.CreateServerRole.Tail",
		"Statement.AlterServerRole.Tail",
		"Statement.CreateLogin.Tail",
		"Statement.CreateUser.Tail",
		"Statement.AlterUser.Tail",
		"Statement.AlterSchema.Tail",
		// The keys, certificates, credentials and policies: what follows the name, and an added signature's module.
		"Statement.AsymmetricKeyDefinition.Tail",
		"Statement.AlterAsymmetricKey.Tail",
		"Statement.SymmetricKeyDefinition.Tail",
		"Statement.AlterSymmetricKey.Tail",
		"Statement.CertificateDefinition.Tail",
		"Statement.AlterCertificate.Tail",
		"Statement.MasterKeyDefinition.Tail",
		"Statement.AlterMasterKey.Tail",
		"Statement.AlterServiceMasterKey.Tail",
		"Statement.ColumnEncryptionKeyDefinition.Tail",
		"Statement.AlterColumnEncryptionKey.Tail",
		"Statement.ColumnMasterKeyDefinition.Tail",
		"Statement.CredentialDefinition.Tail",
		"Statement.OpenSymmetricKey.Tail",
		"Statement.AddSignature.To",
		// The External family: what follows the name of a data source, a file format, a library, a language, a model and a resource pool, kept as written.
		"Statement.ExternalDataSourceDefinition.Tail",
		"Statement.ExternalFileFormatDefinition.Tail",
		"Statement.ExternalLibraryDefinition.Tail",
		"Statement.ExternalLanguageDefinition.Tail",
		"Statement.ExternalModelDefinition.Tail",
		"Statement.ExternalResourcePoolDefinition.Tail",
		// The Broker family: what follows the name of a message type, a contract, a queue, a service, a route, a binding and a priority.
		"Statement.MessageTypeDefinition.Tail",
		"Statement.ContractDefinition.Tail",
		"Statement.QueueDefinition.Tail",
		"Statement.ServiceDefinition.Tail",
		"Statement.RouteDefinition.Tail",
		"Statement.RemoteServiceBindingDefinition.Tail",
		"Statement.BrokerPriorityDefinition.Tail",
		// The Monitoring family: what follows the name of an event notification, an audit and an audit specification, an event's
		// predicate and the state an event session is put in.
		"Statement.EventNotificationDefinition.Tail",
		"Statement.ServerAuditDefinition.Tail",
		"Statement.AuditSpecificationDefinition.Tail",
		"Statement.DatabaseAuditSpecificationDefinition.Tail",
		"Clause.EventPiece.Where",
		"Statement.EventSessionDefinition.State",
		// The Server family: what follows ALTER SERVER CONFIGURATION, ALTER RESOURCE GOVERNOR and the name of a resource pool, a
		// workload group, an availability group and an endpoint, kept as written.
		"Statement.AlterServerConfiguration.Tail",
		"Statement.AlterResourceGovernor.Tail",
		"Statement.ResourcePoolDefinition.Tail",
		"Statement.WorkloadGroupDefinition.Tail",
		"Statement.AvailabilityGroupDefinition.Tail",
		"Statement.EndpointDefinition.Protocol",
		"Statement.EndpointDefinition.Payload",
	};

	/// <summary>
	/// What DROP INDEX's index text counts as among the members compared without the case of key words: the old tree keeps
	/// <c>i ON t WITH (…)</c> as one column reference's text, which compares so under <c>Statement.DropIndex</c> alone.
	/// </summary>
	public const string DropIndexName = "Expression.ColumnReference.Text under Statement.DropIndex";

	/// <summary>
	/// What a signer's text counts as: the old tree keeps <c>CERTIFICATE c</c> as one column reference's text, which compares
	/// so under <c>Statement.DropSignature</c> and <c>Statement.AddSignature</c> alone.
	/// </summary>
	public const string SignerName = "Expression.ColumnReference.Text under Statement.DropSignature";

	/// <summary>Whether a member, or a scoped member above, compares token by token.</summary>
	public static bool Free(string member)
	{
		return FreeText.Contains(member) || member is DropIndexName or SignerName;
	}

	/// <summary>
	/// The key words <c>Sql2023Writer</c> prints in capitals where the old tree keeps what was written: the
	/// only words <see cref="FreeText"/> compares without case.
	/// </summary>
	public static readonly HashSet<string> KeyWords = new(StringComparer.OrdinalIgnoreCase)
	{
		"ABSENT", "ABSOLUTE", "ALL", "AND", "ANY", "APPROXIMATE", "ARRAY", "AS", "ASC", "AT", "AUTHORIZATION", "AUTO",
		"BASE64", "BEGIN", "BETWEEN", "BIGINT", "BINARY", "BOTH", "BULK", "BY", "CACHE", "CAST", "CHANGES", "CHANGETABLE",
		"CHANGE_TRACKING", "CHAR", "CHARACTER", "CHECK", "CLOSE", "COLLATE", "COMMIT", "COMMITTED", "CONCAT",
		"CONSTRAINT", "CONTAINED", "CONTAINSTABLE", "CONTENT", "CONVERT", "CUBE", "CURRENT", "CURRENT_DATE",
		"CURRENT_TIME", "CURRENT_TIMESTAMP", "CURRENT_USER", "CYCLE", "D", "DATA", "DATE", "DAY", "DAYOFYEAR", "DAYS",
		"DD", "DEALLOCATE", "DEC", "DECIMAL", "DEFAULT", "DELAYED_DURABILITY", "DESC", "DISABLE", "DISTINCT", "DOCUMENT",
		"DOUBLE", "DW", "DY", "EDGE", "ELEMENTS", "ENABLE", "ESCAPE", "EXPAND", "EXPLICIT", "FETCH", "FILETABLE", "FIRST",
		"FLOAT", "FOLLOWING", "FOR", "FORCE", "FORCESEEK", "FREETEXTTABLE", "FROM", "FULL", "FUNCTION", "GB", "GRAPH",
		"GROUP", "HASH", "HH", "HINT", "HOUR", "HOURS", "IGNORE", "IN", "INCLUDE_NULL_VALUES", "INCREMENT", "INT",
		"INTEGER", "INTO", "ISOWK", "ISOWW", "ISO_WEEK", "JOIN", "JSON", "JSON_ARRAY", "JSON_ARRAYAGG", "JSON_OBJECT",
		"JSON_OBJECTAGG", "JSON_QUERY", "JSON_VALUE", "KB", "KEEP", "KEEPFIXED", "LANGUAGE", "LAST", "LAST_NODE",
		"LEADING", "LOOP", "M", "MARK", "MAX", "MAXVALUE", "MB", "MCS", "MERGE", "MI", "MICROSECOND", "MILLISECOND",
		"MINUTE", "MINUTES", "MINVALUE", "MM", "MODEL", "MONTH", "MONTHS", "MS", "N", "NANOSECOND", "NATIONAL", "NCHAR",
		"NEXT", "NO", "NOCHECK", "NODE", "NOT", "NS", "NULL", "NULLS", "NUMERIC", "NVARCHAR", "OF", "OFF", "ON", "OPEN",
		"OPENDATASOURCE", "OPENJSON", "OPENQUERY", "OPENROWSET", "OPENXML", "OPTIMIZE", "ORDER", "OVER", "PARSE",
		"PARTIAL", "PARTITION", "PATH", "PB", "PERCENT", "PLAN", "PRECEDING", "PRECISION", "PREDICT", "PRIOR", "Q", "QQ",
		"QUARTER", "RANGE", "RAW", "READ", "REAL", "REBUILD", "RELATIVE", "REMOTE", "REPEATABLE", "RESPECT", "RESTART",
		"RETURNING", "ROBUST", "ROLLBACK", "ROLLUP", "ROOT", "ROW", "ROWS", "RUNTIME", "S", "SAVE", "SCALEOUTEXECUTION",
		"SECOND", "SECONDS", "SERIALIZABLE", "SESSION_USER", "SET", "SHORTEST_PATH", "SIMPLE", "SMALLINT", "SNAPSHOT",
		"SOME", "SS", "START", "SWITCH", "SYSTEM_USER", "TABLE", "TB", "TIES", "TIME", "TO", "TRAILING", "TRAN",
		"TRANSACTION", "TRIGGER", "TRIM", "TRY_CAST", "TRY_CONVERT", "TRY_PARSE", "TYPE", "TZ", "TZOFFSET", "UNBOUNDED",
		"UNCOMMITTED", "UNION", "UNIQUE", "UNKNOWN", "USE", "USER", "VALUE", "VARBINARY", "VARCHAR", "VARYING", "VERSION",
		"VIEWS", "W", "WEEK", "WEEKDAY", "WEEKS", "WITH", "WITHIN", "WITHOUT_ARRAY_WRAPPER", "WK", "WORK", "WRAPPER",
		"WW", "XML", "XMLDATA", "XMLNAMESPACES", "XMLSCHEMA", "XSINIL", "Y", "YEAR", "YEARS", "YY", "YYYY", "ZONE",
		// The words the definitions of indexes, statistics, types and full-text indexes write in their tails.
		"ACTION", "ADD", "ALTER", "ALWAYS", "CASCADE", "CLUSTERED", "COLUMN", "COLUMNSTORE", "DELETE", "DROP",
		"ENCRYPTED", "EXISTS", "FILEGROUP", "FILESTREAM", "FOREIGN", "GENERATED", "GEOGRAPHY_AUTO_GRID",
		"GEOGRAPHY_GRID", "GEOMETRY_AUTO_GRID", "GEOMETRY_GRID", "HIDDEN", "IDENTITY", "INCLUDE", "INCREMENTAL", "INDEX",
		"IS", "KEY", "LIKE", "LIST", "MANUAL", "MASKED", "MOVE", "NONCLUSTERED", "OR", "PARTITIONS", "PAUSE", "PERSISTED",
		"POPULATION", "PRIMARY", "PROPERTY", "REFERENCES", "REPLICATION", "RESUME", "ROWGUIDCOL", "SEARCH", "SPARSE",
		"STATISTICAL_SEMANTICS", "STOP", "STOPLIST", "SYSTEM", "UPDATE", "USING", "WHERE",
		// The words of the writer round's batch: routine and module tails, the drops' tails, a parameter's OUTPUT.
		"ASYMMETRIC", "CERTIFICATE", "DATABASE", "DEPENDENTS", "IO", "LOG", "NONE", "NOWAIT", "OBJECT", "OUTPUT", "PROFILE",
		"PROVIDER", "RECOMPILE", "REMOVE", "RESULT", "SERVER", "SETERROR", "SETS", "UNDEFINED",
		// The Security family: the words of permissions and of securables' classes the old tree keeps as written.
		"ACTIVE", "AFTER", "ALGORITHM", "ASSEMBLY", "ATTESTED", "BEFORE", "BEGIN_DIALOG", "BLOCK", "CLASSIFICATION", "COLLECTION",
		"COUNTER", "CREATE", "CREDENTIAL", "CRYPTOGRAPHIC", "DECRYPTION", "ENCRYPTION", "EXECUTABLE", "EXTERNAL", "FILE", "FILTER",
		"FORMAT", "GRANT", "HASHED", "INSERT", "LOGIN", "MASTER", "MEMBER", "MUST_CHANGE", "NAME", "NEW_ACCOUNT", "NEW_PASSWORD",
		"OLD_ACCOUNT", "OLD_PASSWORD", "OPTION", "OWNER", "PASSWORD", "POLICY", "PREDICATE", "PRIVATE", "PRIVILEGES", "PROCEDURE", "PUBLIC",
		"REGENERATE", "REVOKE", "ROLE", "SCHEMA", "SCOPED", "SECRET", "SECURITY", "SELECT", "SENSITIVITY", "SERVICE", "SIGNATURE",
		"STATE", "SYMMETRIC", "TRANSFER", "UNLOCK", "VALUES", "VIEW", "WINDOWS", "WITHOUT",
		// The External family: the words of an external language's and library's files that the writer prints in capitals.
		"LINUX", "PLATFORM",
		// The Broker family: the words of its objects and statements that the writer prints in capitals.
		"ACTIVATION", "ADD", "BINDING", "BROKER", "BY", "CLEANUP", "CONTRACT", "CONVERSATION", "DESCRIPTION", "DIALOG", "ERROR", "EXECUTE", "GET", "INITIATOR", "LOB_COMPACTION", "MAXDOP", "MAX_QUEUE_READERS", "MESSAGE", "POISON_MESSAGE_HANDLING", "PRIORITY", "PROCEDURE_NAME", "QUEUE", "RECEIVE", "REORGANIZE", "RETENTION", "SELF", "SEND", "SENT", "STATUS", "TARGET", "TIMEOUT", "TIMER", "TOP", "VALIDATION", "VALID_XML", "WAITFOR",
		// The Monitoring family: the words of its statements that the writer prints in capitals.
		"APPLICATION_LOG", "AUDIT", "DROP", "EVENT", "EXTERNAL_MONITOR", "FAN_IN", "FILEPATH", "FOR", "MODIFY", "NOT", "NOTIFICATION", "NULL", "ON", "OR", "PUBLIC", "SECURITY_LOG", "SESSION", "SPECIFICATION", "START", "STOP", "URL",
		// The Server family: the words of its statements that the writer prints in capitals.
		"AVAILABILITY", "CONFIGURATION", "DATABASE_MIRRORING", "DATA_MIRRORING", "DENY", "DHCP", "FAILOVER", "FORCE_FAILOVER_ALLOW_DATA_LOSS", "GOVERNOR", "HTTP", "IP", "LISTENER", "OFFLINE", "POOL", "PORT", "RECONFIGURE", "REPLICA", "RESET", "RESOURCE", "RESTART", "SERVICE_BROKER", "SOAP", "STATISTICS", "TCP", "TSQL", "WORKLOAD",
	};

	/// <summary>
	/// One step from a node to a member of it, and the item where the member is a list.
	/// </summary>
	public readonly record struct Step(object Node, string Member, int? Index);

	/// <summary>
	/// One place two trees differ: the steps from the root of the left tree down to it, and what differs.
	/// </summary>
	public sealed record Diff(IReadOnlyList<Step> Path, string Text, object? Left = null, object? Right = null)
	{
		public override string ToString()
		{
			return string.Concat(Path.Select(static one => $"/{Bridge.NameOf(one.Node)}.{one.Member}{(one.Index is { } at ? $"[{at}]" : "")}")) + ": " + Text;
		}
	}

	/// <summary>
	/// Every place two trees differ, or none where they do not. For two old trees the members of
	/// <see cref="FreeText"/> compare token by token, and each that passed only so is added to
	/// <paramref name="byCase"/>.
	/// </summary>
	public static List<Diff> Differences(object? left, object? right, bool old = true, List<string>? byCase = null)
	{
		var found = new List<Diff>();

		Compare(left, right, [], old, byCase, found);

		return found;
	}

	/// <summary>The first place two trees differ, as text, or null.</summary>
	public static string? Difference(object? left, object? right, bool old = true, List<string>? byCase = null)
	{
		return Differences(left, right, old, byCase).FirstOrDefault()?.ToString();
	}

	static void Compare(object? left, object? right, List<Step> path, bool old, List<string>? byCase, List<Diff> found)
	{
		if (ReferenceEquals(left, right))
			return;

		if (left is null || right is null)
		{
			found.Add(new Diff([.. path], $"{Show(left)} against {Show(right)}", left, right));

			return;
		}

		if (left.GetType() != right.GetType())
		{
			found.Add(new Diff([.. path], $"{Show(left)} against {Show(right)}", left, right));

			return;
		}

		if (left is string || left.GetType().IsPrimitive || left.GetType().IsEnum || left is not ISqlSpan && left is not IEnumerable)
		{
			if (!left.Equals(right))
				found.Add(new Diff([.. path], $"{Show(left)} against {Show(right)}", left, right));

			return;
		}

		if (left is not ISqlSpan)
			return;

		var record = Bridge.NameOf(left);

		foreach (var property in Properties(left.GetType()))
		{
			var a      = property.GetValue(left);
			var b      = property.GetValue(right);
			var member = record + "." + property.Name;

			if (a is IEnumerable one && b is IEnumerable two && a is not string && a.GetType().Name.StartsWith("SqlList", StringComparison.Ordinal))
			{
				var x = one.Cast<object?>().ToArray();
				var y = two.Cast<object?>().ToArray();

				if (x.Length != y.Length)
				{
					found.Add(new Diff([.. path, new Step(left, property.Name, null)], $"{x.Length} items against {y.Length}"));

					continue;
				}

				for (var at = 0; at < x.Length; at++)
				{
					// A list of texts in the closed list compares item by item as one text does.
					if (old && x[at] is string text && y[at] is string other && text != other && FreeText.Contains(member) && SameTokens(left, member, text, other))
					{
						byCase?.Add(member);

						continue;
					}

					Compare(x[at], y[at], [.. path, new Step(left, property.Name, at)], old, byCase, found);
				}

				continue;
			}

			if (old && a is string first && b is string second && first != second)
			{
				var scoped = FreeText.Contains(member) ? member : DatePart(left, path) ? DatePartWord : Dropped(left, path);

				if (scoped is not null && SameTokens(left, scoped, first, second))
				{
					byCase?.Add(scoped);

					continue;
				}
			}

			Compare(a, b, [.. path, new Step(left, property.Name, null)], old, byCase, found);
		}
	}

	/// <summary>
	/// Whether two texts of a free-text member say the same: token by token, a word without case only where
	/// both are key words, a national string's <c>N</c> without case, everything else exactly.
	/// </summary>
	public static bool SameTokens(object node, string member, string one, string two)
	{
		// A binary literal's introducer is a key word whose case the tree does not keep: `0XFF` is `0xFF`.
		if (member == "Expression.Literal.Text" && node is Expression.Literal { Kind: SqlLiteralKind.Hex })
			return one.Length == two.Length && one.Length > 2 && one[0] == '0' && two[0] == '0' && one[1] is 'x' or 'X' && two[1] is 'x' or 'X'
				&& string.Equals(one[2..], two[2..], StringComparison.Ordinal);

		// A literal's text is free only where it is a key word (`current_user`) or a national string.
		if (member == "Expression.Literal.Text" && node is Expression.Literal { Kind: not (SqlLiteralKind.Special or SqlLiteralKind.National) })
			return false;

		var x = SqlTokens.Of(one);
		var y = SqlTokens.Of(two);

		if (x.Count != y.Count)
			return false;

		// A routine's name of several parts is a name, whose case a collation may tell apart: `dbo.max`.
		var named = member == "Expression.RoutineInvocation.Name" && x.Any(static one => one.Text == ".");

		for (var at = 0; at < x.Count; at++)
		{
			var a = x[at];
			var b = y[at];

			if (a.Kind != b.Kind)
				return false;

			if (a.Text == b.Text)
				continue;

			var same = a.Kind switch
			{
				SqlTokens.Kind.Word   => !named && !Named(x, at) && Placed(member, x, at) && KeyWords.Contains(a.Text) && a.Text.Equals(b.Text, StringComparison.OrdinalIgnoreCase),
				SqlTokens.Kind.String => a.Text.Length == b.Text.Length && a.Text[0] is 'N' or 'n' && b.Text[0] is 'N' or 'n' && a.Text[1..] == b.Text[1..],
				_                     => false,
			};

			if (!same)
				return false;
		}

		return true;
	}

	/// <summary>
	/// What a date part's word counts as among the members compared without the case of key words.
	/// </summary>
	public const string DatePartWord = "Expression.ColumnReference.Text of a date part";

	/// <summary>
	/// Whether an old node is the column a date part function's first argument is read as, <c>DATEADD (dd, …)</c>,
	/// whose word the new tree keeps without its case.
	/// </summary>
	static bool DatePart(object node, List<Step> path)
	{
		return node is Expression.ColumnReference
			&& path is [.., { Node: Expression.RoutineInvocation call, Member: "Arguments", Index: 0 }]
			&& Syntax.IsDatePartFunction(call.Name.ToUpperInvariant());
	}

	/// <summary>
	/// The scoped member an old column reference's text counts as where it is an index DROP INDEX names or a signer DROP
	/// SIGNATURE names, or null: everywhere else a column reference's text compares exactly.
	/// </summary>
	static string? Dropped(object node, List<Step> path)
	{
		return node is Expression.ColumnReference && path is [.., { Member: "Names" or "By", Index: not null } step]
			? step.Node switch
			{
				Statement.DropIndex     => DropIndexName,
				Statement.DropSignature => SignerName,
				Statement.AddSignature  => SignerName,
				_                       => null,
			}
			: null;
	}

	/// <summary>
	/// Whether the word at a place in a free text stands where a key word does, and not a name spelled like one:
	/// never a part of a dotted name or the name after COLLATE; in a graph pattern only AND, and LAST_NODE or
	/// SHORTEST_PATH before their bracket, every other word an alias; in a hint, not an index's name, the
	/// columns FORCESEEK names, or the table TABLE HINT names first; in WITHIN GROUP and in a bulk option's ORDER,
	/// not a sort's column, which follows BY, ORDER's bracket or a comma.
	/// </summary>
	static bool Placed(string member, List<SqlTokens.Token> tokens, int at)
	{
		string? Text(int place)
		{
			return place >= 0 && place < tokens.Count ? tokens[place].Text.ToUpperInvariant() : null;
		}

		// The word a definition's tail opens with is its key word, a name after it with a hole in front or not: `FOR .t`.
		if (at == 0 && member is "Statement.SynonymDefinition.Tail" or "Statement.StatisticsDefinition.Tail")
			return true;

		if (Text(at - 1) is "." or "COLLATE" || Text(at + 1) == ".")
			return false;

		switch (member)
		{
			case "Expression.GraphMatch.Pattern":
				return Text(at) == "AND" || Text(at) is "LAST_NODE" or "SHORTEST_PATH" && Text(at + 1) == "(";

			case "Expression.WindowFunction.Within":
				return Text(at - 1) is not ("BY" or ",") || Text(at) == "GRAPH";

			case "Clause.Option.Name" when Text(0) == "ORDER":
				return Text(at - 1) is not ("," or "(");

			case "Clause.Hint.Text":
				// The words before each open bracket, innermost last: a name stands inside INDEX's or FORCESEEK's
				// brackets, and first inside TABLE HINT's.
				var openers = new List<(string? Word, int At)>();

				for (var place = 0; place < at; place++)
				{
					if (tokens[place].Text == "(")
						openers.Add((Text(place - 1), place));
					else if (tokens[place].Text == ")" && openers.Count > 0)
						openers.RemoveAt(openers.Count - 1);
				}

				return !openers.Any(static one => one.Word is "INDEX" or "FORCESEEK")
					&& !(openers.Count > 0 && openers[^1] is { Word: "HINT" } table && table.At == at - 1);

			default:
				return true;
		}
	}

	/// <summary>
	/// Whether the token at a position is a name by where it stands: the owner after <c>AUTHORIZATION</c>, the mark
	/// after <c>MARK</c>, the transaction or savepoint after <c>TRAN</c> or <c>TRANSACTION</c>.
	/// </summary>
	static bool Named(List<SqlTokens.Token> tokens, int at)
	{
		return at > 0 && tokens[at - 1] is { Kind: SqlTokens.Kind.Word } before
			&& before.Text.ToUpperInvariant() is "AUTHORIZATION" or "MARK" or "TRAN" or "TRANSACTION";
	}

	static string Show(object? value)
	{
		return value switch
		{
			null      => "null",
			string s  => $"\"{s}\"",
			ISqlSpan  => Bridge.NameOf(value),
			_         => value.ToString() ?? "",
		};
	}

	static readonly Dictionary<Type, PropertyInfo[]> Known = [];

	/// <summary>
	/// A record's members that say what was written: not its span, nor what it derives.
	/// </summary>
	static PropertyInfo[] Properties(Type type)
	{
		lock (Known)
		{
			if (!Known.TryGetValue(type, out var properties))
				Known[type] = properties = [.. type
					.GetProperties(BindingFlags.Public | BindingFlags.Instance)
					.Where(static one => one.GetIndexParameters().Length == 0 && one.Name is not "Span" and not "Category" and not "EqualityContract")];

			return properties;
		}
	}

	/// <summary>
	/// Every node of an old tree, the root among them.
	/// </summary>
	public static IEnumerable<object> Nodes(object root)
	{
		var pending = new Stack<object>();

		pending.Push(root);

		while (pending.Count > 0)
		{
			var node = pending.Pop();

			yield return node;

			foreach (var property in Properties(node.GetType()))
			{
				var value = property.GetValue(node);

				if (value is ISqlSpan child)
					pending.Push(child);
				else if (value is IEnumerable list and not string)
					foreach (var item in list)
						if (item is ISqlSpan one)
							pending.Push(one);
			}
		}
	}
}
