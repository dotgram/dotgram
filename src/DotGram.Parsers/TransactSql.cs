using System;

using DotGram;

namespace DotGram.Parsers;

/// <summary>
/// A first slice of Microsoft's T-SQL, written as a dialect of <see cref="SqlStandard92"/>
/// rather than as a grammar of its own.
/// </summary>
[Gram("TransactSql.gram", Lexical = true)]
public abstract partial class TransactSql : SqlStandard92
{
}
