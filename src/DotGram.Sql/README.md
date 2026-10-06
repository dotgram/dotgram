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
in `DotGram.Sql` — the records, [`SqlWriter`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Sql/SqlWriter.cs), which prints them back, and
[`SqlWalker`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Sql/SqlWalker.cs), which visits them. `SqlStandardParser` builds the standard's own tree,
and which parser answers with which is under the table.

| Parser | Namespace | What it reads |
| --- | --- | --- |
| [`SqlStandardParser`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Sql/Standard/SqlStandard.gram) | `DotGram.Sql.Standard` | ISO SQL:2023, in part: its lexical elements, names, scalar expressions, aggregates and window functions, the JSON functions, query expressions with row pattern recognition, predicates, the data change statements, the whole schema — tables, views, domains, sequences, privileges, routines, triggers, user-defined types, casts, orderings, transforms, character sets and collations — and the transaction, session, connection, diagnostics, dynamic and direct statements |
| [`Sql92Parser`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Sql/Standard/SqlStandard92.gram) | `DotGram.Sql.Standard` | SQL-92 as the standard writes it |
| [`TransactSqlParser`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Sql/TransactSql/TransactSql.gram) | `DotGram.Sql.TransactSql` | SQL Server's T-SQL, as the engine reads it |

The third names the second — `[GramInclude(typeof(Sql92Parser), As = "Sql92")]` — and
rebinds the rules where T-SQL differs, so what the two languages share is written once and the
dialect is the size of the difference.

`TransactSqlParser` builds the tree, and `Sql92Parser` the expressions in it that the two share.
`SqlStandardParser` builds a tree of its own, the standard's, laid out in
[`Sql2023Ast.cs`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Sql/Standard/Sql2023Ast.cs) in `DotGram.Sql.Ast` and printed back by
[`Sql2023Writer`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.Sql/Standard/Sql2023Writer.cs). Its records are named for the standard's
constructs, so `DotGram.Sql.Ast.Statement` and `DotGram.Sql.Statement` are two types.

`SqlStandardParser` reads at six levels: `ParseValue` (the standard's literal, a signed number among
them, or `NULL`), `ParseDataType`, `ParseExpression`, `ParseSearchCondition`, `ParseStatement` (one
statement of any kind, its `;` optional; a query is the `Statement.Select` among them) and `ParseSql`
(statements, each ended by a `;` or by the end of the text, where an empty statement is refused).
A production below them is read through the level that holds it, and a statement's kind is the type
of its node. `TransactSqlParser` publishes `ParseSelect`,
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

### Scripts

`GO` is not T-SQL: it is the line a client tool cuts a script at, and the server never sees it.
Nor are sqlcmd's `:setvar`, `:r` and `$(name)`. So which method to call is a question of where the
text came from: **text from a file or an editor is a script — read it with `ParseScript` or
`SqlScript.Read`; text a program sends the server in one command is not — read it with
`ParseSql`.** Read in one call, `SELECT 1` and then a line `GO` is a column called `GO`, because that
is what the server makes of it, and `ParseSql` does not guess otherwise.

`TransactSqlParser.ParseScript` cuts a script into batches as sqlcmd cuts it and reads each batch as
the one call it is, giving back a `Batch[]`: each with its statements, the `GO` line that ended it
(`GO 5`, a batch sent five times, too) and its `Source`, the batch as it was cut. The variables the
script sets with `:setvar` are substituted, a `:r` is refused — there is no file to read — and every
other command is the tool's and is passed over. `ParseScript100` to `ParseScript170` are its levels.

The cutting is `SqlScript.Read`, in `DotGram.Sql` and no dialect's: the batches a tool would send, the
commands it would act on, and what it would have said. It is held byte for byte to what sqlcmd
sends, and it runs nothing and reads no file itself — an include is a callback.

```csharp
using System;
using System.Collections.Generic;
using System.IO;

using DotGram.Sql;
using DotGram.Sql.TransactSql;

var text   = "CREATE TABLE $(db).dbo.t (a int)\nGO\nSELECT FROM t\n";
var script = SqlScript.Read(text, new ScriptOptions
{
	Profile        = ScriptProfile.SqlCmd(),                               // or ScriptProfile.Ssms(): GO lines only
	Variables      = new Dictionary<string, string> { ["db"] = "Sales" }, // sqlcmd's -v
	ResolveInclude = include => File.Exists(include.Path) ? new ScriptSource(include.Path, File.ReadAllText(include.Path)) : null,
	SourceName     = "deploy.sql",
});

foreach (var batch in script.Batches)
{
	var match = TransactSqlParser.Located.TryParseSql150(batch);

	if (!match.IsSuccess)
	{
		var where = batch.Locate((int)match.Position, 0);

		Console.WriteLine($"{where.Source}({where.Span.At}): {match.Error}");   // deploy.sql(43): …
	}
}
```

A batch written as one run of the script with nothing substituted is a window of the very string
given, so every position a reading reports is already a position in the script; in a batch where a
variable was substituted, `batch.Locate` takes it back to where it was written. `script.Directives`
lists the commands, none of them run, and `script.Diagnostics` what sqlcmd would have said — an
undefined variable, a file it could not include, a line it would stop at. And for text whose origin
is not known, `script.HasClientSyntax` says whether anything in it was the tool's: a script of one
batch ended by `GO` counts, which is why the question is not how many batches there are.

Where each node was written is there for whoever asks for it. `TransactSqlParser.Located` is the
same parser called with locations on — `TransactSqlParser.Located.TryParseStatement` — and every
node it builds carries in `Span` the range of text it was read from, and in `Span.GapStart` where
the text in front of it begins: the end of the token before it, so that what lies between is
spacing and comments and nothing else. The two are one parser: `TransactSqlParser` keeps no
positions and offers none, and pays a test per value it reads for sharing its reader.
A span takes no part in a node's equality: the located tree of a text equals the plain one.

A tree compares by what was written. The lists a node holds are `SqlList<T>`, which compare
element by element, so two readings of the same text are equal and hash alike, and a table of
nodes that should tell two equal subtrees apart is keyed by reference. A list is made with a
collection expression, `[a, b]` or `[.. items]`, or with `SqlList.From(items)`.

The tree they build is described in [`docs/ast.md`](https://github.com/dotgram/dotgram/blob/main/docs/ast.md).
What they read is held against SQL Server itself, against a corpus of somebody else's SQL and
against a round trip — parse, print, and compare the two readings — which catches a parser that
answers yes and builds the wrong thing.

All three of these parsers may be called from any number of threads at once; that is the
generated parser's own contract, not something this package adds
([`DotGram`'s README](https://github.com/dotgram/dotgram/blob/main/src/DotGram/README.md#threads)).

## Taking one

```xml
<PackageReference Include="DotGram.Sql" Version="0.2.0" />
```

There is no companion runtime package, and no generator to install alongside it: the parsers
were generated when this assembly was compiled.
