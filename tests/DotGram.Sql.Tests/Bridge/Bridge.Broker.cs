using System;
using System.Diagnostics.CodeAnalysis;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// Service Broker: message types, contracts, queues, services, routes, remote service bindings, broker priorities, and the dialog and conversation statements (BEGIN DIALOG, SEND, RECEIVE, WAITFOR).
	/// </summary>
	/// <remarks>No arm yet: every record here throws the gap its inventory row names.</remarks>
	internal static class Broker
	{
		public static readonly string[] Owns =
		[
			"Statement.MessageTypeDefinition", "Statement.ContractDefinition", "Statement.QueueDefinition",
			"Statement.ServiceDefinition", "Statement.RouteDefinition", "Statement.RemoteServiceBindingDefinition",
			"Statement.BrokerPriorityDefinition", "Statement.BeginDialog", "Statement.ConversationTimer",
			"Statement.EndConversation", "Statement.MoveConversation", "Statement.Send",
			"Statement.Receive", "Statement.GetConversationGroup", "Statement.WaitForStatement",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = null;

			return Owned(Owns, old) ? throw Inventory.Unconverted(old) : false;
		}
	}
}
