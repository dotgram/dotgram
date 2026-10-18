using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DotGram.Sql.Ast;

/// <summary>
/// What a statement does, which a reader filters on without naming every record: asked of the statement's type, and
/// kept by no record.
/// </summary>
/// <remarks>
/// <para>
/// The groups are T-SQL's reference's, as <see cref="StatementCategory"/> says them. A statement of the standard that
/// T-SQL does not have is put where it would be in T-SQL's terms: a <c>CONNECT</c> and the session's <c>SET</c>s in
/// <see cref="StatementCategory.Session"/>, a prepared statement in <see cref="StatementCategory.Execute"/>, a descriptor
/// or a cursor allocated in <see cref="StatementCategory.Declaration"/>, a <c>WHENEVER</c>, a <c>GET DIAGNOSTICS</c> and
/// a <c>PIPE ROW</c> in <see cref="StatementCategory.Control"/>.
/// </para>
/// <para>
/// A record named in no list below is a definition: <c>CREATE</c>, <c>ALTER</c> and <c>DROP</c> are data definition, and
/// a <c>BACKUP</c> or a <c>RESTORE</c> the server looked after, except where the definition is of a principal — a login,
/// a user, a role — which is who may do what.
/// </para>
/// </remarks>
public static class StatementCategories
{
	/// <summary>What <paramref name="statement"/> does.</summary>
	public static StatementCategory Category(this Statement statement)
	{
		if (statement is null)
			throw new ArgumentNullException(nameof(statement));

		return Known.GetOrAdd(statement.GetType(), Of);
	}

	/// <summary>Each type's group, worked out once.</summary>
	static readonly ConcurrentDictionary<Type, StatementCategory> Known = new();

	/// <summary>The records whose group their name does not say.</summary>
	static readonly Dictionary<string, StatementCategory> Named = new(StringComparer.Ordinal)
	{
		["Select"] = StatementCategory.Query,

		["Insert"] = StatementCategory.Dml,
		["InsertBulk"] = StatementCategory.Dml,
		["BulkInsert"] = StatementCategory.Dml,
		["Update"] = StatementCategory.Dml,
		["Delete"] = StatementCategory.Dml,
		["Merge"] = StatementCategory.Dml,
		["Send"] = StatementCategory.Dml,
		["Receive"] = StatementCategory.Dml,
		["GetConversationGroup"] = StatementCategory.Dml,
		["ReadText"] = StatementCategory.Dml,
		["WriteText"] = StatementCategory.Dml,
		["UpdateText"] = StatementCategory.Dml,

		["TruncateTable"] = StatementCategory.Ddl,
		["AddSignature"] = StatementCategory.Ddl,
		["AddSensitivityClassification"] = StatementCategory.Ddl,
		["EnableTrigger"] = StatementCategory.Ddl,
		["DisableTrigger"] = StatementCategory.Ddl,
		["Extension"] = StatementCategory.Ddl,

		["Grant"] = StatementCategory.Dcl,
		["Deny"] = StatementCategory.Dcl,
		["Revoke"] = StatementCategory.Dcl,
		["AlterAuthorization"] = StatementCategory.Dcl,

		["Compound"] = StatementCategory.Control,
		["If"] = StatementCategory.Control,
		["While"] = StatementCategory.Control,
		["TryCatch"] = StatementCategory.Control,
		["BeginDialog"] = StatementCategory.Control,
		["BeginConversationTimer"] = StatementCategory.Control,
		["EndConversation"] = StatementCategory.Control,
		["MoveConversation"] = StatementCategory.Control,
		["WaitFor"] = StatementCategory.Control,
		["WaitForReceive"] = StatementCategory.Control,
		["WaitForGetConversationGroup"] = StatementCategory.Control,
		["OpenCursor"] = StatementCategory.Control,
		["CloseCursor"] = StatementCategory.Control,
		["FetchCursor"] = StatementCategory.Control,
		["DeallocateCursor"] = StatementCategory.Control,
		["Print"] = StatementCategory.Control,
		["Return"] = StatementCategory.Control,
		["Throw"] = StatementCategory.Control,
		["RaiseError"] = StatementCategory.Control,
		["GoTo"] = StatementCategory.Control,
		["Label"] = StatementCategory.Control,
		["Break"] = StatementCategory.Control,
		["Continue"] = StatementCategory.Control,
		["Whenever"] = StatementCategory.Control,
		["GetDiagnostics"] = StatementCategory.Control,
		["PipeRow"] = StatementCategory.Control,

		["DeclareVariables"] = StatementCategory.Declaration,
		["DeclareCursor"] = StatementCategory.Declaration,
		["DeclareLocalTemporaryTable"] = StatementCategory.Declaration,
		["SetVariable"] = StatementCategory.Declaration,
		["SetCursorVariable"] = StatementCategory.Declaration,
		["AllocateCursor"] = StatementCategory.Declaration,
		["AllocateDescriptor"] = StatementCategory.Declaration,
		["DeallocateDescriptor"] = StatementCategory.Declaration,
		["GetDescriptor"] = StatementCategory.Declaration,
		["SetDescriptor"] = StatementCategory.Declaration,
		["CopyDescriptor"] = StatementCategory.Declaration,

		["BeginTransaction"] = StatementCategory.Transaction,
		["SaveTransaction"] = StatementCategory.Transaction,
		["StartTransaction"] = StatementCategory.Transaction,
		["Commit"] = StatementCategory.Transaction,
		["Rollback"] = StatementCategory.Transaction,
		["Savepoint"] = StatementCategory.Transaction,
		["ReleaseSavepoint"] = StatementCategory.Transaction,

		["Execute"] = StatementCategory.Execute,
		["ExecuteModule"] = StatementCategory.Execute,
		["ExecuteString"] = StatementCategory.Execute,
		["ExecuteImmediate"] = StatementCategory.Execute,
		["Call"] = StatementCategory.Execute,
		["Prepare"] = StatementCategory.Execute,
		["Describe"] = StatementCategory.Execute,
		["DeallocatePrepare"] = StatementCategory.Execute,

		["UpdateStatistics"] = StatementCategory.Admin,
		["Kill"] = StatementCategory.Admin,
		["KillQueryNotificationSubscription"] = StatementCategory.Admin,
		["KillStatsJob"] = StatementCategory.Admin,
		["Reconfigure"] = StatementCategory.Admin,
		["Shutdown"] = StatementCategory.Admin,
		["Dbcc"] = StatementCategory.Admin,
		["Checkpoint"] = StatementCategory.Admin,
		["AlterResourceGovernor"] = StatementCategory.Admin,
		["AlterServerConfiguration"] = StatementCategory.Admin,

		["LineNumber"] = StatementCategory.Session,
		["Use"] = StatementCategory.Session,
		["ExecuteAs"] = StatementCategory.Session,
		["Revert"] = StatementCategory.Session,
		["SetUser"] = StatementCategory.Session,
		["SetOptions"] = StatementCategory.Session,
		["OpenSymmetricKey"] = StatementCategory.Session,
		["OpenMasterKey"] = StatementCategory.Session,
		["CloseSymmetricKey"] = StatementCategory.Session,
		["CloseAllSymmetricKeys"] = StatementCategory.Session,
		["CloseMasterKey"] = StatementCategory.Session,
		["Connect"] = StatementCategory.Session,
		["Disconnect"] = StatementCategory.Session,
		["FreeLocator"] = StatementCategory.Session,
		["HoldLocator"] = StatementCategory.Session,
	};

	/// <summary>What a principal is called in a definition's name: who may do what.</summary>
	static readonly string[] Principals = ["Login", "User", "Role", "ServerRole", "ApplicationRole"];

	static StatementCategory Of(Type type)
	{
		var name = type.Name;

		if (Named.TryGetValue(name, out var named))
			return named;

		if (name.StartsWith("Backup", StringComparison.Ordinal) || name.StartsWith("Restore", StringComparison.Ordinal))
			return StatementCategory.Admin;

		foreach (var verb in (string[])["Create", "Alter", "Drop"])
			if (name.StartsWith(verb, StringComparison.Ordinal))
				return Array.IndexOf(Principals, name.Substring(verb.Length)) >= 0 ? StatementCategory.Dcl : StatementCategory.Ddl;

		// The standard's SET statements are the session's, a SET of a variable apart.
		if (name.StartsWith("Set", StringComparison.Ordinal))
			return StatementCategory.Session;

		throw new NotSupportedException($"Statement.{name} has no category.");
	}
}
