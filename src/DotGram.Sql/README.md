<!--
  Agents: the skill for this package is SKILL.md, beside this file in the package
  directory — which parser to take, the contract they share, and what is easy to get
  wrong. Read it before writing code against the package. In a restored package that is
  ~/.nuget/packages/dotgram.sql/<version>/SKILL.md.
-->
# DotGram.Sql

SQL parsers written in `.gram`, and the trees of records they build. Where an
example shows one feature, a parser here is written against a whole specification.

They are ordinary C# libraries. .Gram generates the parsers into this assembly at compile time,
so nothing here carries a parser runtime, and neither does anything that references it.

## The dialects

Each dialect is a directory, a namespace and a grammar of its own. SQL-92 and T-SQL meet in the tree
in `DotGram.Sql` — the records, [`SqlWriter`](SqlWriter.cs), which prints them back, and
[`SqlWalker`](SqlWalker.cs), which visits them. `SqlStandardParser` builds the standard's own tree,
and which parser answers with which is under the table.

| Parser | Namespace | What it reads |
| --- | --- | --- |
| [`SqlStandardParser`](Standard/SqlStandard.gram) | `DotGram.Sql.Standard` | ISO SQL:2023, in part: its lexical elements, names, scalar expressions, aggregates and window functions, the JSON functions, query expressions with row pattern recognition, predicates, the data change statements, the whole schema — tables, views, domains, sequences, privileges, routines, triggers, user-defined types, casts, orderings, transforms, character sets and collations — and the transaction, session, connection, diagnostics, dynamic and direct statements |
| [`Sql92Parser`](Standard/SqlStandard92.gram) | `DotGram.Sql.Standard` | SQL-92 as the standard writes it |
| [`TransactSqlParser`](TransactSql/TransactSql.gram) | `DotGram.Sql.TransactSql` | SQL Server's T-SQL, as the engine reads it |

The third names the second — `[GramInclude(typeof(Sql92Parser), As = "Sql92")]` — and
rebinds the rules where T-SQL differs, so what the two languages share is written once and the
dialect is the size of the difference.

`TransactSqlParser` builds the tree, and `Sql92Parser` the expressions in it that the two share.
`SqlStandardParser` builds a tree of its own, the standard's, laid out in
[`Sql2023Ast.cs`](Standard/Sql2023Ast.cs) in `DotGram.Sql.Ast` and printed back by
[`Sql2023Writer`](Standard/Sql2023Writer.cs). Its records are named for the standard's
constructs, so `DotGram.Sql.Ast.Statement` and `DotGram.Sql.Statement` are two types.

`SqlStandardParser` publishes the standard's productions under their own names —
`ParseValueExpression`, `ParseSearchCondition`, `ParseQueryExpression`, `ParseSQLSchemaStatement`
and the rest listed at the end of its grammar. `TransactSqlParser` publishes `ParseSelect`,
`ParseQuery`, `ParseSearchCondition`, `ParseValueExpression`, `ParseStatement`, `ParseSql` and
`ParseScript`, each with two `TryParse…`: one giving a `Match<T>` with the refusal's message and
position, and a `bool TryParse…(string input, out T value)` for where only the answer is wanted.

All three read through a lexical split (`Lexical = true`): a lexical half makes tokens, and the
syntactic half above it decides each choice by the token in front of it, which is what a parser
written by hand does. `SqlStandardParser` reads two things less than the standard does because of
it: a comment holds others six deep, and a `/*` inside a comment always opens one.

```csharp
using DotGram.Sql;
using DotGram.Sql.TransactSql;

var match = TransactSqlParser.TryParseSelect("select name from Users where id > @id");

var select = (Statement.Select)match.Value;
var query  = (Query.Specification)select.Of;

var from = (TableReference.Named)query.From[0];   // from.Table is "Users"
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
