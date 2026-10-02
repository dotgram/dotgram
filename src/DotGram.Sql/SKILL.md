---
name: dotgram-sql
description: Parse SQL text into a tree of records with DotGram.Sql — T-SQL as SQL Server reads it (statements, whole texts, scripts cut into batches as sqlcmd cuts them, a database's compatibility level, source locations), ISO SQL:2023 as the standard writes it, and SQL-92. Use when a project references the DotGram.Sql package, or when SQL text has to be read, checked, walked or printed back without a database. Not a database client, a query builder or a formatter: it reads syntax, and a statement it reads may still name tables and columns that do not exist.
---

# DotGram.Sql

Three parsers, each a grammar of its own, and the trees they build. There is no runtime behind
them and nothing to configure: the parsers were generated when the package was compiled.

The [README][readme] beside this file lists what each parser reads and what it publishes. This
is what to decide before using one, the contract they share, and what is easy to get wrong.

[readme]: https://github.com/dotgram/dotgram/tree/main/src/DotGram.Sql

## Which parser

- **`TransactSqlParser`** (`DotGram.Sql.TransactSql`) — code written for SQL Server. It reads
  what the engine reads, held against SQL Server itself, and builds the tree in `DotGram.Sql`.
- **`SqlStandardParser`** (`DotGram.Sql.Standard`) — the ISO SQL:2023 language, when the
  question is whether a text is standard SQL, or when a tree of the standard's own constructs is
  wanted. It builds the tree in `DotGram.Sql.Ast`. It refuses what the standard does not have:
  `SELECT TOP 1 a FROM t` is T-SQL, and here it is refused.
- **`Sql92Parser`** (`DotGram.Sql.Standard`) — SQL-92, the base T-SQL is written on.

## The contract they share

- **Input is a `string`.** Every published production has `ParseX` and two `TryParseX`, and each
  reads the *whole* input: text left over is a refusal, not a shorter answer.
- **`ParseX` throws `FormatException`**, its message saying what was expected and ending
  with the offset where the text stopped fitting. **`TryParseX` returns a `Match<T>`**:
  `IsSuccess`, `Value` (read it only after `IsSuccess`), `Error` and `Position`.
- **`bool TryParseX(string input, out T value)`** answers only whether the text reads, and says
  nothing about what was expected where it does not. Where the message is not wanted, ask this
  one: a refusal costs it about half, since nothing is read a second time to record what failed.
- **A node is a record.** Two that mean the same are equal: a tree equals its own reparse,
  located or not. Every list a node holds is a `SqlList<T>`, read only and compared element by
  element; make one with `[a, b]`, `[.. items]` or `SqlList.From(items)` — an array does not
  convert. A list a record may leave out is a `SqlList<T>?`. Equality and the hash throw
  `InsufficientExecutionStackException` on a tree deeper than the stack. `SqlWriter.Write`
  prints a T-SQL tree back as text; `Sql2023Writer.Write` (in `DotGram.Sql.Ast`) prints the
  standard's.
- **`SqlWalker.Walk(root, visit)`** hands every node under `root` to `visit`, parent first,
  until `visit` answers false. It walks either tree; a check is a pattern match in the lambda.

```csharp
using System.Collections.Generic;

using DotGram.Sql;
using DotGram.Sql.TransactSql;

var match = TransactSqlParser.TryParseStatement("select name from Users where id > @id");

if (!match.IsSuccess)
    return;                                          // match.Error, match.Position

var select = (Statement.Select)match.Value;
string text = SqlWriter.Write(select);              // SELECT name FROM Users WHERE id > @id

var tables = new List<string>();
SqlWalker.Walk(select, node =>
{
    if (node is TableReference.Named named)
        tables.Add(named.Table);                    // Users
    return true;                                    // false stops the walk
});
```

## What is easy to get wrong

**Syntax is all it checks.** `SELECT nosuch FROM nowhere` reads: names are not resolved, types
are not checked, and nothing is looked up in a database. What it answers is what SQL Server's
`SET PARSEONLY ON` answers. A statement that parses is not a statement that is safe to run; a
value from outside still goes in as a parameter.

**One statement, a text, or a script.**

- `ParseStatement` reads exactly one statement. `SELECT 1; SELECT 2` is refused there.
- `ParseSql` reads a text of statements — what a client sends the server in one call — and
  gives back a `Statement[]`.
- `GO` is not T-SQL. It is the line a client tool cuts a script at, and the server never sees
  it — so in `ParseSql` it is a word: `SELECT 1` and then a line `GO` reads as `SELECT 1 AS GO`,
  because that is what the server does with that text in one call. **Text from a file or an
  editor is a script: `ParseScript`. Text a program sends in one command: `ParseSql`.** Pasting
  a script into `ParseSql` is the mistake to avoid; nothing in `ParseSql` guesses.
- `ParseScript` cuts a script as sqlcmd cuts it and gives back a `Batch[]`, one for each batch
  sqlcmd would send: a `GO` with nothing before it sends nothing, a batch of only spacing or a
  comment is sent and reads as no statements, `GO;` is text. `:setvar` and `$(name)` are
  substituted, a `:r` is refused, other commands are passed over.
- `SqlScript.Read(text, options)` is the cutting on its own, for the batches one at a time, the
  commands (`Directives`), what sqlcmd would have said (`Diagnostics`), variables from outside
  and includes through a callback; `TransactSqlParser.TryParseSql(batch)` reads one of its
  batches. `SqlScript.Read(text).HasClientSyntax` says whether text of unknown origin holds
  anything of the tool's.
- In a batch where a variable was substituted, positions are in that batch's own text
  (`batch.Source.Locate` maps them back); everywhere else they are positions in the script.

**The compatibility level.** `ParseStatement`, `ParseSql` and `ParseScript` name no level and
read everything any level reads. A database at level 150 refuses a `WINDOW` clause that
`ParseStatement` accepts. To read what one database reads, use the level's own form:
`ParseStatement150`, `ParseSql150`, `ParseScript150`, from `100` to `170`.

**Locations are paid for, and only where asked.** A node from `TransactSqlParser` has a `Span`
that says nothing (`Known` is false). `TransactSqlParser.Located` is the same parser with
spans filled in: `TransactSqlParser.Located.ParseStatement(text).Span`. `Span.GapStart` is where
the trivia in front of the node begins, so `text[GapStart..At]` holds its leading comments and
nothing else; a match's `ReadingEnd` is where the reading stopped looking, so the comments after
the last statement lie between its end and that. Spans take no part in equality: key a table of
nodes by reference. A copy made with `with` keeps the span and says `IsStale`.

**Two trees with the same names.** `DotGram.Sql` and `DotGram.Sql.Ast` both have a `Statement`
and an `Expression`, and they are different types. Code that uses both parsers imports one
namespace and aliases the other: `using Ast = DotGram.Sql.Ast;`.

**A refusal's message can be long.** Where a statement stops at a position that could begin
almost anything, `Error` lists everything that could have stood there, hundreds of keywords
included. `Position` is the part to show a user; the list is for a tool.

**Where `SqlStandardParser` reads less than the standard.** A comment holds others six deep,
and a `/*` inside a comment always opens one: `/* a /* b */ 1` is refused, where the BNF would
also let that `/*` be two characters of the outer comment. And it reads SQL:2023 in part: the
README lists what it covers, and what it does not cover it refuses.

```csharp
using Ast = DotGram.Sql.Ast;
using DotGram.Sql.Standard;

Ast.Expression e = SqlStandardParser.ParseValueExpression("a + b * 2");
string again     = Ast.Sql2023Writer.Write(e);     // a + b * 2

bool standard = SqlStandardParser.TryParseQueryExpression("SELECT TOP 1 a FROM t").IsSuccess;  // false
```
