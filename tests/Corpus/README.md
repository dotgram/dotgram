# Somebody else's corpus

Every other test in this repository was written here, which is the one thing wrong
with all of them: a grammar and its tests written by the same hand agree about what
the language is. This directory is the exception.

## What is here

`ScriptDom/` is the test corpus of Microsoft's **SqlScriptDOM** — the T-SQL parser
that ships as `Microsoft.SqlServer.TransactSql.ScriptDom` — copied byte for byte:

| from | files |
| --- | --: |
| `Test/SqlDom/TestScripts` | 488 |
| `Test/SqlDom/PhaseOneTestScripts` | 96 |
| `Test/SqlDom/BaselinesCommon` | 70 |
| `Test/SqlDom/Baselines80` … `Baselines180` | 411 |
| `Test/SqlDom/BaselinesFabricDW` | 21 |
| | **1086** |

Source: <https://github.com/microsoft/SqlScriptDOM>, commit `b583737` (release
180.102.0, 2026-08-28). Licensed under the MIT License, Copyright (c) Microsoft
Corporation — the notice is kept beside the files it covers, in `ScriptDom/LICENSE`.

The files are exempt from the repository's line-ending normalization
(`.gitattributes`): fifteen of them are UTF-16, and the point of a corpus written
elsewhere is that it is their text and not ours.

## Why it is here

Two things it answers that nothing written here can.

**Whether the grammar reads what people write.** `SqlStandard92.gram` and
`TransactSql.gram` are read against nineteen hundred statements nobody here composed,
and what they refuse is grouped by *what stood where the reading stopped* — which
names the feature rather than the message.

**Which version of T-SQL a feature belongs to.** The `Baselines<n>` directories are
Microsoft's own partition of the language by parser version: `Baselines130` is what
`TSql130Parser` reads and its predecessors do not. A dialect written per version has
its target set for it.

## Running it

```
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --corpus tests/Corpus/ScriptDom
```

It is not a benchmark and not a test: it prints what each parser read and where the
rest of it stopped. Nothing in CI depends on the number.

**A low percentage is not a coverage number.** The corpus is a T-SQL parser's own
test suite — it is made of the things T-SQL has, most of them things no SQL-92 parser
should accept, and a good many of them statements rather than queries. What the
number is for is the difference between two readings of the same text: the standard
against the dialect, and one version of the dialect against the next.
