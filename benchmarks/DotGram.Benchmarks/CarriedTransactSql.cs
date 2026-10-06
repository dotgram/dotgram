using System;

using DotGram.Sql;
using DotGram.Sql.Standard;

namespace DotGram.Benchmarks;

// Lab only (not for landing): T-SQL with the rule that gathered two members onto one stack
// (SetExpressions) split so that each alternative's gathered list is a rule of its own, compiled
// immediately and, for the same grammar text, on the tape. Both read through a located door too,
// as the shipped parser does.
[GramInclude(typeof(Sql92Parser), As = "Sql92")]
[Gram("TransactSqlCarried.gram", Lexical = true, Carrier = GramCarrier.Immediate)]
[GramOptions(LocationType = typeof(ISqlLocatable), Suffix = "Located", PerCall = true)]
public abstract partial class CarriedTransactSql
{
}

[GramInclude(typeof(Sql92Parser), As = "Sql92")]
[Gram("TransactSqlCarried.gram", Lexical = true, Carrier = GramCarrier.Tape)]
[GramOptions(LocationType = typeof(ISqlLocatable), Suffix = "Located", PerCall = true)]
public abstract partial class CarriedTransactSqlTape
{
}
