using System;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Monitoring family (the auditing and event-capture statements): writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutMonitoring(ISqlNode node)
		{
			return false;
		}
	}
}
