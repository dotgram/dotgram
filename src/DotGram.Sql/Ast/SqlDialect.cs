using System;

namespace DotGram.Sql.Ast;

/// <summary>
/// Which reading a writer's text is for: the standard's, which <see cref="SqlWriter.Write(ISqlNode)"/> prints, or a
/// dialect's, which refuses besides the trees that dialect would read as others and keeps its own spellings.
/// </summary>
enum SqlDialect
{
	Standard,
	TransactSql,
	PostgreSql,
}
