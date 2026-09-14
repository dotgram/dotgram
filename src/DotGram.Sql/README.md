# DotGram.Sql

SQL parsers written in `.gram`, and the one tree of records every one of them builds. Where an
example shows one feature, a parser here is written against a whole specification.

They are ordinary C# libraries. .Gram generates the parsers into this assembly at compile time,
so nothing here carries a parser runtime, and neither does anything that references it.

## The dialects

Each dialect is a directory, a namespace and a grammar of its own, and they meet only in the tree
in `DotGram.Sql` — the records, [`SqlWriter`](SqlWriter.cs), which prints them back, and
[`SqlWalker`](SqlWalker.cs), which visits them.

| Parser | Namespace | What it reads |
| --- | --- | --- |
| [`SqlStandardParser`](Standard/SqlStandard.gram) | `DotGram.Sql.Standard` | ISO SQL:2023, being written: so far its lexical elements, names, scalar expressions, aggregates and window functions, query expressions with row pattern recognition, and predicates |
| [`Sql92Parser`](Standard/SqlStandard92.gram) | `DotGram.Sql.Standard` | SQL-92 as the standard writes it, until SQL:2023 replaces it |
| [`TransactSqlParser`](TransactSql/TransactSql.gram) | `DotGram.Sql.TransactSql` | SQL Server's T-SQL, as the engine reads it |

For now the second names the first — `[GramInclude(typeof(Sql92Parser), As = "Sql92")]` —
and rebinds the rules where T-SQL differs, so what the two languages share is written once and the
dialect is the size of the difference.

The last two read through a lexical split (`Lexical = true`): a lexical half makes tokens, and the
syntactic half above it decides each choice by the token in front of it, which is what a parser
written by hand does.

```csharp
using DotGram.Sql;
using DotGram.Sql.TransactSql;

var match = TransactSqlParser.TryParseSelect("select name from Users where id > @id");

var select = (Statement.Select)match.Value;
var query  = (Query.Specification)select.Of;

query.From[0];   // TableReference.Named { Table = "Users" }
```

A database's compatibility level gates a small part of what SQL Server reads — the `WINDOW`
clause from 160, `OPENJSON`'s schema from 130. `TransactSqlParser.ParseStatement130` reads what
the engine reads at level 130, and so on from `100` to `170`; `ParseStatement` names no level and
reads them all. The levels are one grammar and one machine, told apart by a number, and what each
gates was measured against SQL Server rather than remembered.

`TransactSqlParser.ParseStatement` reads one statement. `TransactSqlParser.ParseSql` reads a text
of them — what a client sends the server in one call — and gives back a `Statement[]`: each ended
by a `;` or by nothing, except before a `WITH`, which needs the statement before it ended, as the
server does. `ParseSql100` to `ParseSql170` are its levels.

`GO` is not T-SQL: it is the line a client cuts a script at, and the server never sees it.
`TransactSqlParser.ParseScript` reads a script — batches cut apart at the lines that say `GO`, the
way ScriptDom and the management tools cut them — and gives back a `Batch[]`, each with its
statements and the `GO` line that ended it. `GO 5`, a batch sent five times, is read too.
`ParseSql` is one batch, and a `GO` in it is refused.

The tree they build is described in [`docs/ast.md`](https://github.com/dotgram/dotgram/blob/main/docs/ast.md).
What they read is held against SQL Server itself, against a corpus of somebody else's SQL and
against a round trip — parse, print, and compare the two readings — which catches a parser that
answers yes and builds the wrong thing.

## Taking one

```xml
<PackageReference Include="DotGram.Sql" Version="0.1.0" />
```

There is no companion runtime package, and no generator to install alongside it: the parsers
were generated when this assembly was compiled.
