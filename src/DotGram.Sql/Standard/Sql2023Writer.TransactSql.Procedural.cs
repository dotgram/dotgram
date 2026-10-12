using System;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Procedural family: writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutProcedural(ISqlNode node)
		{
			return false;
		}
	}
}
