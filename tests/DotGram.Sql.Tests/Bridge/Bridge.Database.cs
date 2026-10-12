using System;
using System.Diagnostics.CodeAnalysis;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// A database and what it is made of: files, filegroups, its settings, backups and restores, partitions, full-text catalogues.
	/// </summary>
	/// <remarks>No arm yet: every record here throws the gap its inventory row names.</remarks>
	internal static class Database
	{
		public static readonly string[] Owns =
		[
			"Statement.CreateDatabase", "Statement.AlterDatabaseSet", "Statement.AlterDatabaseScopedConfiguration",
			"Statement.AlterDatabaseCollate", "Statement.AlterDatabaseModifyName", "Statement.AlterDatabaseModifyFileGroup",
			"Statement.AlterDatabaseModifyFile", "Statement.AlterDatabaseModify", "Statement.AlterDatabaseAddFileGroup",
			"Statement.AlterDatabaseAddLogFile", "Statement.AlterDatabaseAddFile", "Statement.AlterDatabaseRemoveFileGroup",
			"Statement.AlterDatabaseRemoveFile", "Statement.AlterDatabaseRebuildLog", "Statement.AlterDatabasePerformCutover",
			"Statement.AlterDatabaseModifyBackupStorageRedundancy", "Statement.BackupDatabase", "Statement.BackupTransactionLog",
			"Statement.BackupServer", "Statement.BackupGroup", "Statement.BackupCertificate",
			"Statement.BackupMasterKey", "Statement.BackupServiceMasterKey", "Statement.BackupSymmetricKey",
			"Statement.RestoreDatabase", "Statement.RestoreLog", "Statement.RestoreFileListOnly",
			"Statement.RestoreHeaderOnly", "Statement.RestoreLabelOnly", "Statement.RestoreRewindOnly",
			"Statement.RestoreVerifyOnly", "Statement.RestoreMasterKey", "Statement.RestoreServiceMasterKey",
			"Statement.RestoreSymmetricKey", "Statement.Checkpoint", "Statement.PartitionFunctionDefinition",
			"Statement.AlterPartitionFunction", "Statement.PartitionSchemeDefinition", "Statement.AlterPartitionScheme",
			"Statement.FullTextCatalogDefinition", "Statement.AlterFullTextCatalog", "Statement.FullTextStopListDefinition",
			"Statement.AlterFullTextStopList", "Statement.SearchPropertyListDefinition", "Statement.AlterSearchPropertyList",
			"Statement.DatabaseAuditSpecificationDefinition", "Clause.DatabaseFile", "Clause.FileGroup",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = null;

			return Owned(Owns, old) ? throw Inventory.Unconverted(old) : false;
		}
	}
}
