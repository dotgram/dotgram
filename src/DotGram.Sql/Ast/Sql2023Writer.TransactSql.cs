using System;

namespace DotGram.Sql.Ast;

// T-SQL's nodes are written by one file a family, as the bridge carries them (tests/DotGram.Sql.Tests/Bridge):
// Sql2023Writer.TransactSql.<Family>.cs, each with a TryPut<Family> that writes the nodes it owns and says no
// to the rest. The central switches of the standard's writer — a literal, a data type, an expression, a table
// source, a statement — ask them in turn before they refuse, so that a family is filled in its own file without
// touching a shared switch. The refusal stays the last step.
public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// A node the standard's switches have no arm for, offered to each family in turn; false where none
		/// writes it, and the caller then refuses it.
		/// </summary>
		bool TryPutTransactSql(ISqlNode node)
		{
			return TryPutValues(node)
				|| TryPutQueries(node)
				|| TryPutDml(node)
				|| TryPutTables(node)
				|| TryPutRoutines(node)
				|| TryPutProcedural(node)
				|| TryPutSecurity(node)
				|| TryPutDatabase(node)
				|| TryPutServer(node)
				|| TryPutBroker(node)
				|| TryPutMonitoring(node)
				|| TryPutExternal(node)
				|| TryPutDrop(node);
		}
	}
}
