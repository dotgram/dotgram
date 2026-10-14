using System;
using System.Diagnostics.CodeAnalysis;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// Auditing and event capture: server audits, audit specifications, event sessions and event notifications.
	/// </summary>
	/// <remarks>No arm yet: every record here throws the gap its inventory row names.</remarks>
	internal static class Monitoring
	{
		public static readonly string[] Owns =
		[
			"Statement.EventSessionDefinition", "Statement.EventNotificationDefinition", "Statement.ServerAuditDefinition",
			"Statement.AuditSpecificationDefinition", "Statement.DatabaseAuditSpecificationDefinition", "Clause.EventPiece",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = null;

			return Owned(Owns, old) ? throw Inventory.Unconverted(old) : false;
		}
	}
}
