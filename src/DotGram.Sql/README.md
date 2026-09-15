# DotGram.Sql

SQL parsers written in `.gram`, with typed syntax trees and SQL writers. Where an
example shows one feature, a parser here is written against a whole specification.

They are ordinary C# libraries. .Gram generates the parsers into this assembly at compile time,
so nothing here carries a parser runtime, and neither does anything that references it.

## The dialects

Each dialect has a directory, a namespace and a grammar of its own. SQL-92 and T-SQL use
the records in `DotGram.Sql`, with [`SqlWriter`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Sql/SqlWriter.cs)
to print them back and [`SqlWalker`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Sql/SqlWalker.cs)
to visit them. SQL:2023 uses the separate tree in `DotGram.Sql.Ast` and `Sql2023Writer`.

| Parser | Namespace | What it reads |
| --- | --- | --- |
| [`SqlStandardParser`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Sql/Standard/SqlStandard.gram) | `DotGram.Sql.Standard` | ISO SQL:2023, in part: its lexical elements, names, scalar expressions, aggregates and window functions, the JSON functions, query expressions with row pattern recognition, predicates, the data change statements, the whole schema — tables, views, domains, sequences, privileges, routines, triggers, user-defined types, casts, orderings, transforms, character sets and collations — and the transaction, session, connection, diagnostics, dynamic and direct statements |
| [`Sql92Parser`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Sql/Standard/SqlStandard92.gram) | `DotGram.Sql.Standard` | SQL-92 as the standard writes it |
| [`TransactSqlParser`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Sql/TransactSql/TransactSql.gram) | `DotGram.Sql.TransactSql` | SQL Server's T-SQL, as the engine reads it |

The third names the second — `[GramInclude(typeof(Sql92Parser), As = "Sql92")]` — and
rebinds the rules where T-SQL differs, so what the two languages share is written once and the
dialect is the size of the difference.

`TransactSqlParser` builds the T-SQL tree, and `Sql92Parser` the expressions in it that the two share.
`SqlStandardParser` builds the SQL:2023 tree in
[`Sql2023Ast.cs`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Sql/Standard/Sql2023Ast.cs):
names, literals, data types, expressions, queries, data change statements, schema definitions,
and the other statements listed above. Its published productions return typed values.
[`Sql2023Writer`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Sql/Standard/Sql2023Writer.cs)
writes the tree back as SQL; tests parse that text again and compare the trees.

`SqlStandardParser` publishes the standard's productions under their own names —
`ParseValueExpression`, `ParseSearchCondition`, `ParseQueryExpression`, `ParseSQLSchemaStatement`
and the rest listed at the end of its grammar. `TransactSqlParser` publishes `ParseSelect`,
`ParseQuery`, `ParseSearchCondition`, `ParseValueExpression`, `ParseStatement`, `ParseSql` and
`ParseScript`, each with its `TryParse…`.

`Sql92Parser` and `TransactSqlParser` read through a lexical split (`Lexical = true`): a lexical
half makes tokens, and the syntactic half above it decides each choice by the token in front of
it, which is what a parser written by hand does.

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
gates is held against SQL Server.

`TransactSqlParser.ParseStatement` reads one statement. `TransactSqlParser.ParseSql` reads a text
of them — what a client sends the server in one call — and gives back a `Statement[]`: each ended
by a `;` or by nothing, except before a `WITH`, which needs the statement before it ended, as the
server does. `ParseSql100` to `ParseSql170` are its levels.

`GO` is not T-SQL: it is the line a client cuts a script at, and the server never sees it.
`TransactSqlParser.ParseScript` reads a script — batches cut apart at the lines that say `GO`, the
way ScriptDom and the management tools cut them — and gives back a `Batch[]`, each with its
statements and the `GO` line that ended it. `GO 5`, a batch sent five times, is read too.
`ParseScript100` to `ParseScript170` are its levels. `ParseSql` is one batch, and a `GO` in it is
refused.

Where each node was written is there for whoever asks for it. `TransactSqlParser.Located` is the
same grammar compiled with `LocationType = typeof(ISqlSpan)` —
`TransactSqlParser.Located.TryParseStatement` — and every node it builds carries in `Span` the
range of text it was read from. `TransactSqlParser` itself pays nothing for them.

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

The package targets `netstandard2.0` and `net10.0`. On `netstandard2.0` it depends on
`System.Memory`; on `net10.0` it has no package dependencies.
