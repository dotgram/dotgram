using System;

namespace DotGram.Sql.Ast;

// T-SQL's Server family: what runs beside the databases and the commands that govern it — KILL, RECONFIGURE, SHUTDOWN and DBCC, the
// server's configuration, the resource governor, availability groups and endpoints. A setting that is a name and a value is an
// Option (proposal 20); a place that is one of several things is a small family named after the grammar's words.

public abstract partial record Statement
{
	/// <summary>
	/// T-SQL: KILL (Transact-SQL). <c>KILL session [WITH STATUSONLY | COMMIT | ROLLBACK]</c>: a session by its number, which a minus
	/// may precede, or a unit of work by its string.
	/// </summary>
	public sealed record Kill(Expression Target) : Statement
	{
		/// <summary>T-SQL: KILL (Transact-SQL). <c>WITH STATUSONLY</c>, <c>COMMIT</c> or <c>ROLLBACK</c>; null where none is written.</summary>
		public KillOption? With { get; init; }
	}

	/// <summary>T-SQL: KILL STATS JOB (Transact-SQL). <c>KILL STATS JOB job</c>: a number, which a minus may precede.</summary>
	public sealed record KillStatsJob(LiteralValue.Numeric Job) : Statement;

	/// <summary>
	/// T-SQL: KILL QUERY NOTIFICATION SUBSCRIPTION. <c>KILL QUERY NOTIFICATION SUBSCRIPTION { ALL | subscription }</c>: every
	/// subscription, or the one a number names, which a minus may precede; exactly one of the two.
	/// </summary>
	public sealed record KillQueryNotificationSubscription : Statement
	{
		/// <summary>T-SQL: KILL QUERY NOTIFICATION SUBSCRIPTION. <c>ALL</c>.</summary>
		public bool All { get; init; }

		/// <summary>T-SQL: KILL QUERY NOTIFICATION SUBSCRIPTION. The subscription's number; null where <see cref="All"/> is said.</summary>
		public LiteralValue.Numeric? Subscription { get; init; }
	}

	/// <summary>T-SQL: RECONFIGURE (Transact-SQL). <c>RECONFIGURE [WITH OVERRIDE]</c>.</summary>
	public sealed record Reconfigure(bool Override) : Statement;

	/// <summary>T-SQL: SHUTDOWN (Transact-SQL). <c>SHUTDOWN [WITH NOWAIT]</c>.</summary>
	public sealed record Shutdown(bool NoWait) : Statement;

	/// <summary>
	/// T-SQL: DBCC (Transact-SQL). <c>DBCC command [(argument, …)] [WITH option, …]</c>: one record for every command, the documented
	/// ones and the rest, as the engine reads them all one way. The command is a plain word; an argument is a value or a name and a
	/// value, as an EXECUTE's is.
	/// </summary>
	public sealed record Dbcc(Identifier Command) : Statement
	{
		/// <summary>
		/// T-SQL: DBCC (Transact-SQL). The arguments in brackets, each a value the grammar's <c>DbccValue</c> reads or a name and a
		/// value, <c>SIZE = 10</c>; null where no brackets are written, and empty where they hold nothing.
		/// </summary>
		public SqlList<ExecuteArgument>? Arguments { get; init; }

		/// <summary>
		/// T-SQL: DBCC (Transact-SQL). <c>WITH option, …</c>: words, MAXDOP and WAIT_AT_LOW_PRIORITY with their own values; the
		/// first may be a run of words joined by <c>JOIN</c>; none where no <c>WITH</c> is written.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: ALTER SERVER CONFIGURATION (Transact-SQL). <c>ALTER SERVER CONFIGURATION SET setting</c>: one setting of the server's, a
	/// name and a value held to the catalogue of the statement. The name keeps its first word in <see cref="Option.Name"/> and the
	/// rest in <see cref="Option.Words"/> (<c>PROCESS AFFINITY CPU</c>, <c>FAILOVER CLUSTER PROPERTY VerboseLogging</c>); a value
	/// written with no <c>=</c> is the value of an option whose <see cref="Option.EqualsOmitted"/> stays false, since the setting's own
	/// rule fixes the form.
	/// </summary>
	public sealed record AlterServerConfiguration(Option Setting) : Statement;

	/// <summary>
	/// T-SQL: ALTER RESOURCE GOVERNOR (Transact-SQL). <c>ALTER RESOURCE GOVERNOR action [RECONFIGURE]</c>: the governor reconfigured,
	/// disabled, its statistics reset, or given one setting, and any of those followed by <c>RECONFIGURE</c>.
	/// </summary>
	public sealed record AlterResourceGovernor(ResourceGovernorAction Action) : Statement
	{
		/// <summary>T-SQL: ALTER RESOURCE GOVERNOR (Transact-SQL). A <c>RECONFIGURE</c> written after the action.</summary>
		public bool ReconfigureAfter { get; init; }
	}

	/// <summary>
	/// T-SQL: CREATE RESOURCE POOL (Transact-SQL). <c>CREATE RESOURCE POOL pool [WITH (setting, …)]</c>: the pool is named by a name
	/// or by a string, which an identifier of style <see cref="IdentifierStyle.String"/> holds.
	/// </summary>
	public sealed record CreateResourcePool(Identifier Name) : Statement
	{
		/// <summary>
		/// T-SQL: CREATE RESOURCE POOL (Transact-SQL). <c>WITH (setting, …)</c>: the percentages and the I/O rates as whole numbers, and
		/// <c>AFFINITY SCHEDULER</c> and <c>AFFINITY NUMANODE</c>, in any order and as often as wanted; none where no <c>WITH</c> is written.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: ALTER RESOURCE POOL (Transact-SQL). <c>ALTER RESOURCE POOL pool WITH (setting, …)</c>: the settings of the creating
	/// statement, one at least.
	/// </summary>
	public sealed record AlterResourcePool(Identifier Name, SqlList<Option> Options) : Statement;

	/// <summary>
	/// T-SQL: CREATE WORKLOAD GROUP (Transact-SQL). <c>CREATE WORKLOAD GROUP group [WITH (setting, …)] [USING pool, …]</c>.
	/// </summary>
	public sealed record CreateWorkloadGroup(Identifier Name) : Statement
	{
		/// <summary>
		/// T-SQL: CREATE WORKLOAD GROUP (Transact-SQL). <c>WITH (setting, …)</c>: IMPORTANCE and the limits of a request and of the
		/// group, in any order and as often as wanted; none where no <c>WITH</c> is written.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];

		/// <summary>T-SQL: CREATE WORKLOAD GROUP (Transact-SQL). <c>USING pool, EXTERNAL pool</c>; none where no <c>USING</c> is written.</summary>
		public SqlList<WorkloadPool> Pools { get; init; } = [];
	}

	/// <summary>T-SQL: ALTER WORKLOAD GROUP (Transact-SQL). The same parts as the creating statement, each optional.</summary>
	public sealed record AlterWorkloadGroup(Identifier Name) : Statement
	{
		/// <summary>T-SQL: ALTER WORKLOAD GROUP (Transact-SQL). <c>WITH (setting, …)</c>; none where no <c>WITH</c> is written.</summary>
		public SqlList<Option> Options { get; init; } = [];

		/// <summary>T-SQL: ALTER WORKLOAD GROUP (Transact-SQL). <c>USING pool, EXTERNAL pool</c>; none where no <c>USING</c> is written.</summary>
		public SqlList<WorkloadPool> Pools { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: CREATE AVAILABILITY GROUP (Transact-SQL). <c>CREATE AVAILABILITY GROUP group [WITH (setting, …)] source [LISTENER
	/// 'name' (listener)]</c>: a group is made of its replicas or of other groups, and says so.
	/// </summary>
	public sealed record CreateAvailabilityGroup(Identifier Name, AvailabilitySource Source) : Statement
	{
		/// <summary>
		/// T-SQL: CREATE AVAILABILITY GROUP (Transact-SQL). <c>WITH (setting, …)</c>: AUTOMATED_BACKUP_PREFERENCE,
		/// FAILURE_CONDITION_LEVEL, HEALTH_CHECK_TIMEOUT, DB_FAILOVER, DTC_SUPPORT, REQUIRED_SYNCHRONIZED_SECONDARIES_TO_COMMIT and
		/// CLUSTER_TYPE with their values, and BASIC, DISTRIBUTED and CONTAINED alone; none where no <c>WITH</c> is written.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];

		/// <summary>T-SQL: CREATE AVAILABILITY GROUP (Transact-SQL). <c>LISTENER 'name'</c>: the listener's name, a string; null where no listener is written.</summary>
		public LiteralValue.String? ListenerName { get; init; }

		/// <summary>T-SQL: CREATE AVAILABILITY GROUP (Transact-SQL). <c>LISTENER 'name' (listener)</c>: how the listener is addressed, with <see cref="ListenerName"/> or not at all.</summary>
		public AvailabilityListener? Listener { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER AVAILABILITY GROUP (Transact-SQL). <c>ALTER AVAILABILITY GROUP group action</c>: an ALTER does one thing.
	/// </summary>
	public sealed record AlterAvailabilityGroup(Identifier Name, AvailabilityAction Action) : Statement;
}

/// <summary>T-SQL: CREATE AVAILABILITY GROUP (Transact-SQL). What an availability group is made of.</summary>
public abstract record AvailabilitySource : ISqlNode
{
	/// <summary>Where in the text this source was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this source was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// <c>FOR [DATABASE db, …] REPLICA ON 'server' WITH (setting, …), …</c>: the databases, none where no <c>DATABASE</c> is written,
	/// and the replicas, one at least.
	/// </summary>
	public sealed record Replicas(SqlList<Identifier> Databases, SqlList<AvailabilityReplica> Servers) : AvailabilitySource;

	/// <summary><c>AVAILABILITY GROUP ON 'group' WITH (setting, …) [, 'group' WITH (…)]</c>, with no <c>FOR</c>: a distributed group, of one group or of two.</summary>
	public sealed record Groups(SqlList<AvailabilityGroupSpec> Specs) : AvailabilitySource;
}

/// <summary>
/// T-SQL: CREATE AVAILABILITY GROUP (Transact-SQL), ALTER AVAILABILITY GROUP (Transact-SQL). A replica: a server instance, and the
/// bracket that says how it takes part.
/// </summary>
public sealed record AvailabilityReplica(LiteralValue.String Server, SqlList<Option> Options) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>
/// T-SQL: CREATE AVAILABILITY GROUP (Transact-SQL), ALTER AVAILABILITY GROUP (Transact-SQL). A group joined to another: named, and told
/// where to listen.
/// </summary>
public sealed record AvailabilityGroupSpec(LiteralValue.String Group, SqlList<Option> Options) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: CREATE AVAILABILITY GROUP (Transact-SQL), ALTER AVAILABILITY GROUP (Transact-SQL). How a listener takes its address.</summary>
public abstract record AvailabilityListener : ISqlNode
{
	/// <summary>Where in the text this listener was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this listener was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>WITH DHCP [ON ('address', 'mask')]</c>: the address from the network, and, where it is told, the network it is on; both strings.</summary>
	public sealed record Dhcp(ListenerAddress? On) : AvailabilityListener;

	/// <summary><c>WITH IP (('address', 'mask'), …) [, PORT = n]</c>: given addresses, one at least, and a port.</summary>
	public sealed record Ip(SqlList<ListenerAddress> Addresses) : AvailabilityListener
	{
		/// <summary>T-SQL: CREATE AVAILABILITY GROUP (Transact-SQL). <c>, PORT = n</c>, whole digits; null where none is written.</summary>
		public LiteralValue.Numeric? Port { get; init; }
	}
}

/// <summary>
/// T-SQL: CREATE AVAILABILITY GROUP (Transact-SQL), ALTER AVAILABILITY GROUP (Transact-SQL). An address of a listener, a string, and
/// its subnet mask, a string, where the address has one.
/// </summary>
public sealed record ListenerAddress(LiteralValue.String Address, LiteralValue.String? Mask) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: ALTER AVAILABILITY GROUP (Transact-SQL). What an ALTER AVAILABILITY GROUP does, one thing at a time.</summary>
public abstract record AvailabilityAction : ISqlNode
{
	/// <summary>Where in the text this action was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this action was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>SET (setting)</c>: one of the settings of the group's <c>WITH</c>, or <c>ROLE = SECONDARY</c>.</summary>
	public sealed record Set(Option Setting) : AvailabilityAction;

	/// <summary><c>ADD DATABASE db</c>.</summary>
	public sealed record AddDatabase(Identifier Database) : AvailabilityAction;

	/// <summary><c>REMOVE DATABASE db</c>.</summary>
	public sealed record RemoveDatabase(Identifier Database) : AvailabilityAction;

	/// <summary><c>ADD REPLICA ON 'server' WITH (setting, …)</c>.</summary>
	public sealed record AddReplica(AvailabilityReplica Replica) : AvailabilityAction;

	/// <summary><c>MODIFY REPLICA ON 'server' WITH (setting, …)</c>.</summary>
	public sealed record ModifyReplica(AvailabilityReplica Replica) : AvailabilityAction;

	/// <summary><c>REMOVE REPLICA ON 'server'</c>.</summary>
	public sealed record RemoveReplica(LiteralValue.String Server) : AvailabilityAction;

	/// <summary><c>JOIN AVAILABILITY GROUP ON 'group' WITH (…) [, 'group' WITH (…)]</c>: one group or two.</summary>
	public sealed record JoinAvailabilityGroup(SqlList<AvailabilityGroupSpec> Specs) : AvailabilityAction;

	/// <summary><c>MODIFY AVAILABILITY GROUP ON 'group' WITH (…) [, 'group' WITH (…)]</c>: one group or two.</summary>
	public sealed record ModifyAvailabilityGroup(SqlList<AvailabilityGroupSpec> Specs) : AvailabilityAction;

	/// <summary><c>JOIN</c>.</summary>
	public sealed record Join : AvailabilityAction;

	/// <summary><c>GRANT CREATE ANY DATABASE</c> or <c>DENY CREATE ANY DATABASE</c>.</summary>
	public sealed record CreateAnyDatabase(AvailabilityPermission Permission) : AvailabilityAction;

	/// <summary><c>FAILOVER</c>.</summary>
	public sealed record Failover : AvailabilityAction;

	/// <summary><c>FORCE_FAILOVER_ALLOW_DATA_LOSS</c>.</summary>
	public sealed record ForceFailoverAllowDataLoss : AvailabilityAction;

	/// <summary><c>OFFLINE</c>.</summary>
	public sealed record Offline : AvailabilityAction;

	/// <summary><c>ADD LISTENER 'name' (listener)</c>.</summary>
	public sealed record AddListener(LiteralValue.String Name, AvailabilityListener Listener) : AvailabilityAction;

	/// <summary><c>MODIFY LISTENER 'name' (change)</c>.</summary>
	public sealed record ModifyListener(LiteralValue.String Name, ListenerChange Change) : AvailabilityAction;

	/// <summary><c>RESTART LISTENER 'name'</c>.</summary>
	public sealed record RestartListener(LiteralValue.String Name) : AvailabilityAction;

	/// <summary><c>REMOVE LISTENER 'name'</c>.</summary>
	public sealed record RemoveListener(LiteralValue.String Name) : AvailabilityAction;
}

/// <summary>T-SQL: ALTER AVAILABILITY GROUP (Transact-SQL). What <c>MODIFY LISTENER</c> changes.</summary>
public abstract record ListenerChange : ISqlNode
{
	/// <summary>Where in the text this change was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this change was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>ADD IP ('address' [, 'mask'])</c>.</summary>
	public sealed record AddIp(ListenerAddress Address) : ListenerChange;

	/// <summary><c>PORT = n</c>, whole digits.</summary>
	public sealed record Port(LiteralValue.Numeric Number) : ListenerChange;
}

/// <summary>T-SQL: ALTER AVAILABILITY GROUP (Transact-SQL). <c>GRANT</c> or <c>DENY</c> of the permission to create any database.</summary>
public enum AvailabilityPermission { Grant, Deny }

public abstract partial record Expression
{
	/// <summary>
	/// T-SQL: ALTER SERVER CONFIGURATION (Transact-SQL). The processors or NUMA nodes of a process's affinity (the grammar's
	/// <c>ServerRanges</c>): whole numbers and ranges of them joined by commas with no brackets, <c>1, 3 TO 5, 8</c>.
	/// </summary>
	public sealed record ServerRanges(SqlList<PartitionRange> Ranges) : Expression;
}

/// <summary>T-SQL: KILL (Transact-SQL). What a KILL does to the work it ends: <c>STATUSONLY</c>, <c>COMMIT</c> or <c>ROLLBACK</c>.</summary>
public enum KillOption { StatusOnly, Commit, Rollback }

/// <summary>T-SQL: ALTER RESOURCE GOVERNOR (Transact-SQL). What an ALTER RESOURCE GOVERNOR does.</summary>
public abstract record ResourceGovernorAction : ISqlNode
{
	/// <summary>Where in the text this action was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this action was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>RECONFIGURE</c>.</summary>
	public sealed record Reconfigure : ResourceGovernorAction;

	/// <summary><c>DISABLE</c>.</summary>
	public sealed record Disable : ResourceGovernorAction;

	/// <summary><c>RESET STATISTICS</c>.</summary>
	public sealed record ResetStatistics : ResourceGovernorAction;

	/// <summary>
	/// <c>WITH (setting)</c>: one setting in brackets — <c>CLASSIFIER_FUNCTION</c>, a function named by two parts exactly or NULL, or
	/// <c>MAX_OUTSTANDING_IO_PER_VOLUME</c>, a number with no exponent or DEFAULT.
	/// </summary>
	public sealed record Settings(Option Setting) : ResourceGovernorAction;
}

/// <summary>
/// T-SQL: CREATE WORKLOAD GROUP (Transact-SQL), ALTER WORKLOAD GROUP (Transact-SQL). One pool a workload group is used in: a
/// resource pool by its name, or <c>EXTERNAL</c> and an external resource pool by its name.
/// </summary>
public sealed record WorkloadPool(Identifier Name, bool External) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
