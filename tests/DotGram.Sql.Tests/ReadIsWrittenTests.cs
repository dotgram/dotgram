using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// What T-SQL reads it can write: a text the parser takes as a statement of the common tree is printed by
/// <c>TransactSqlWriter</c> and reads back as the same tree. A text the tree has no place for is refused or an
/// extension, never a tree the writer then refuses.
/// </summary>
/// <remarks>
/// Each text here is read today, as a tree with a name part or a literal the writer refuses
/// (<see cref="System.ArgumentException"/>): the qualifier of a star with an empty part, a table named as a pseudo
/// column, a static member called on nothing, and <c>x'01'</c>, which the grammar reads as the standard's binary
/// string although the writer says T-SQL has none (in T-SQL it is the column <c>x</c> with the alias <c>'01'</c>).
/// </remarks>
public sealed class ReadIsWrittenTests
{
	[Theory]
	[InlineData("SELECT a. .*")]
	[InlineData("SELECT s..*")]
	[InlineData("SELECT [s]. .* FROM t")]
	[InlineData("DELETE FROM t OUTPUT deleted. .*")]
	[InlineData("SELECT 1 FROM $node_id")]
	[InlineData("SELECT ::ba")]
	[InlineData("SELECT x'01'")]
	public void A_statement_read_is_written_and_read_back(string text)
	{
		var match = TransactSqlParser.Located.TryParseStatement(text);

		if (!match.IsSuccess || match.Value is Ast.Statement.Extension)
			return;

		ReadBack.Holds(text);
	}
}
