using System;

using DotGram;
using DotGram.Sql.Standard;

namespace DotGram.Sql.TransactSql;

/// <summary>
/// A first slice of Microsoft's T-SQL, written as a dialect of <see cref="Sql92Parser"/>
/// rather than as a grammar of its own.
/// </summary>
/// <remarks>
/// <c>ParseSql</c> reads what a program sends the server in one call, and reads it as the server
/// does — so a <c>GO</c> there is a word, and <c>SELECT 1</c> and then a line <c>GO</c> is a column
/// called <c>GO</c>. Text from a file or an editor is a script, whose <c>GO</c> lines and sqlcmd
/// commands are the client tool's: read it with <c>ParseScript</c>, or cut it with
/// <see cref="SqlScript.Read"/> and read each batch with <c>TryParseSql(ScriptBatch)</c>.
/// <see cref="SqlScript.HasClientSyntax"/> says whether text of unknown origin is a script.
/// </remarks>
// One parser, read through two doors: the methods below, and the same methods in `Located`,
// which ask the same reading to tell every value where it was written and where the text in
// front of it began. Locations are not free for a recognizer, so whoever needs them asks for
// them by name, `TransactSqlParser.Located.TryParseStatement`; a reading that does not ask
// still keeps on its tape where each value was read and tests whether to offer it, a few per
// cent of a parse, and the grammar is compiled once rather than twice.
// The standard this dialect is written on top of, named rather than inherited: a class has
// one base and as many attributes as it likes, and what a grammar is called inside this one
// — `Sql92.ValueExpression` — is this grammar's business rather than the standard's.
[GramInclude(typeof(Sql92Parser), As = "Sql92")]
[Gram("TransactSql.gram", Lexical = true)]
[GramOptions(LocationType = typeof(ISqlLocatable), Suffix = "Located", PerCall = true)]
public abstract partial class TransactSqlParser
{
}
