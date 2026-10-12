using System;
using System.Diagnostics.CodeAnalysis;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// The server and what runs beside the databases: external sources, resource governance, audits, event sessions, endpoints, availability groups, Service Broker, and the administrative commands.
	/// </summary>
	/// <remarks>No arm yet: every record here throws the gap its inventory row names.</remarks>
	internal static class Server
	{
		public static readonly string[] Owns =
		[
			"Statement.ExternalDataSourceDefinition", "Statement.ExternalFileFormatDefinition", "Statement.ExternalLibraryDefinition",
			"Statement.ExternalResourcePoolDefinition", "Statement.ExternalLanguageDefinition", "Statement.ExternalModelDefinition",
			"Statement.ResourcePoolDefinition", "Statement.WorkloadGroupDefinition", "Statement.AlterResourceGovernor",
			"Statement.AlterServerConfiguration", "Statement.ServerAuditDefinition", "Statement.AuditSpecificationDefinition",
			"Statement.EventSessionDefinition", "Statement.EventNotificationDefinition", "Statement.AvailabilityGroupDefinition",
			"Statement.EndpointDefinition", "Statement.MessageTypeDefinition", "Statement.ContractDefinition",
			"Statement.QueueDefinition", "Statement.ServiceDefinition", "Statement.RouteDefinition",
			"Statement.RemoteServiceBindingDefinition", "Statement.BrokerPriorityDefinition", "Statement.BeginDialog",
			"Statement.ConversationTimer", "Statement.EndConversation", "Statement.MoveConversation",
			"Statement.Send", "Statement.Receive", "Statement.GetConversationGroup",
			"Statement.Kill", "Statement.Reconfigure", "Statement.Shutdown",
			"Statement.Dbcc", "Clause.EventPiece",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = null;

			return Owned(Owns, old) ? throw Inventory.Unconverted(old) : false;
		}
	}
}
