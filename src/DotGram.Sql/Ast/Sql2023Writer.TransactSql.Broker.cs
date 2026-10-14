using System;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Broker family (Service Broker's objects and conversation statements): writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutBroker(ISqlNode node)
		{
			return false;
		}
	}
}
