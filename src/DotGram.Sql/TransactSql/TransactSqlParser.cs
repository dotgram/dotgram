using System;

using DotGram;
using DotGram.Sql.Standard;

namespace DotGram.Sql.TransactSql;

/// <summary>
/// A first slice of Microsoft's T-SQL, written as a dialect of <see cref="Sql92Parser"/>
/// rather than as a grammar of its own.
/// </summary>
// Two parsers of one grammar: the reading below, and the same reading told where every
// value was written. Locations cost fourteen per cent of the parse and nothing in memory,
// which is cheap for a tool and not free for a recognizer — so whoever needs them asks for
// them by name, `TransactSqlParser.Located.TryParseStatement`, and everybody else does not pay.
// The standard this dialect is written on top of, named rather than inherited: a class has
// one base and as many attributes as it likes, and what a grammar is called inside this one
// — `Sql92.ValueExpression` — is this grammar's business rather than the standard's.
[GramInclude(typeof(Sql92Parser), As = "Sql92")]
[Gram("TransactSql.gram", Lexical = true)]
[GramOptions(LocationType = typeof(ISqlSpan), Suffix = "Located")]
public abstract partial class TransactSqlParser
{
}
