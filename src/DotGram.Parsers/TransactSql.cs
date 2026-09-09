using System;

using DotGram;

namespace DotGram.Parsers;

/// <summary>
/// A first slice of Microsoft's T-SQL, written as a dialect of <see cref="SqlStandard92"/>
/// rather than as a grammar of its own.
/// </summary>
// Two parsers of one grammar: the reading below, and the same reading told where every
// value was written. Locations cost fourteen per cent of the parse and nothing in memory,
// which is cheap for a tool and not free for a recognizer — so whoever needs them asks for
// them by name, `TransactSql.Located.TryParseStatement`, and everybody else does not pay.
// The standard this dialect is written on top of, named rather than inherited: a class has
// one base and as many attributes as it likes, and what a grammar is called inside this one
// — `Sql92.ValueExpression` — is this grammar's business rather than the standard's.
[GramInclude(typeof(SqlStandard92), As = "Sql92")]
[Gram("TransactSql.gram", Lexical = true)]
[Gram(LocationType = typeof(Sql.ISqlSpan), Suffix = "Located")]
public abstract partial class TransactSql
{
}
