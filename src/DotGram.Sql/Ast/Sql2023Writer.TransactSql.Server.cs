using System;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Server family: writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutServer(ISqlNode node)
		{
			return false;
		}
	}
}
