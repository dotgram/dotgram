using System;
using System.Diagnostics.CodeAnalysis;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// The server and what runs beside the databases: resource governance, server configuration, endpoints, availability groups, and the administrative commands.
	/// </summary>
	/// <remarks>No arm yet: every record here throws the gap its inventory row names.</remarks>
	internal static class Server
	{
		public static readonly string[] Owns =
		[
			"Statement.ResourcePoolDefinition", "Statement.WorkloadGroupDefinition", "Statement.AlterResourceGovernor",
			"Statement.AlterServerConfiguration", "Statement.AvailabilityGroupDefinition", "Statement.EndpointDefinition",
			"Statement.Kill", "Statement.Reconfigure", "Statement.Shutdown",
			"Statement.Dbcc",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = null;

			return Owned(Owns, old) ? throw Inventory.Unconverted(old) : false;
		}
	}
}
