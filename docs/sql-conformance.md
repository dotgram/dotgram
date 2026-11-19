# SQL conformance

How much SQL that nobody here wrote the T-SQL parser of `DotGram.Sql` reads, held against two
outside judges: Microsoft's own T-SQL parser, ScriptDom, over its test corpus, and SQL Server
itself, over every example of the T-SQL reference; and how much PostgreSQL its PostgreSQL parser
reads, held against the server's own parser and its regression suite. Every number below is a count
a test or a committed report holds, and each section says which.

What is measured is syntax. SQL Server is asked under `SET PARSEONLY ON`, which answers whether a
statement is well formed and resolves no name: a statement read here may still name a table, a
column or a function that does not exist.

## Against SQL Server: the T-SQL reference

Every example of every page of the
[Transact-SQL reference](https://github.com/MicrosoftDocs/sql-docs/tree/0a5380c14e9b504e320cca4f758ce4f46886f087/docs/t-sql)
(MicrosoftDocs/sql-docs at `0a5380c`, 2025-05-08), cut into statements and asked of SQL Server 2025
under `SET PARSEONLY ON` at every compatibility level from 100 to 170, and of `TransactSqlParser` at
the same level. The latest level decides a statement's column.

| | statements |
| --- | ---: |
| pages of the reference / with examples | 1,163 / 996 |
| statements asked | 8,338 |
| read by both | 7,997 |
| read by the engine and refused here | 0 |
| read here and refused by the engine | 0 |
| read by both at 170, and at an older level by one only | 0 |
| read here, and another product's (Azure Synapse, Fabric, …) by the page's marking or the engine's answer | 81 |
| read by neither | 260 |
| left out: they name `PARSEONLY` themselves | 0 |

So of the 7,997 statements SQL Server reads, the parser reads every one, at the level the engine reads
it, and reads nothing the engine refuses, apart from the 81 statements of other products. The 260 that
both refuse are counted and not listed: an example cut out of its page is not always a statement SQL
Server reads, and a refusal both agree on is not a difference between them.

Source: [`docs/internal/coverage.md`](https://github.com/dotgram/dotgram/blob/main/docs/internal/coverage.md),
written by `--coverage` (`benchmarks/DotGram.Benchmarks/Coverage.cs`) against a live server and
committed, page by page. What each level gates is held by `TransactSqlTests`
(`Each_level_reads_from_where_the_engine_does`, `Each_level_reads_until_where_the_engine_stops`), every
case of which was asked of the engine.

## Against ScriptDom: its test corpus

The test corpus of [SqlScriptDOM](https://github.com/microsoft/SqlScriptDOM), Microsoft's T-SQL parser,
copied byte for byte into `tests/Corpus/ScriptDom` (commit `b583737`, release 180.102.0): 1,086 files.
ScriptDom cuts each file into statements, reading it at the parser version its `Baselines<n>` directory
names, capped at 180; each statement ScriptDom reads is then read by `TransactSqlParser`, which knows
no version below 100 and reads every level at once.

| | files | ScriptDom reads | read here as well | refused here |
| --- | ---: | ---: | ---: | ---: |
| `TestScripts` | 488 | 4,166 | 3,796 | 370 |
| `Baselines80` … `Baselines180` | 411 | 3,495 | 3,169 | 326 |
| `BaselinesCommon` | 70 | 693 | 666 | 27 |
| `BaselinesFabricDW` | 21 | 85 | 85 | 0 |
| `PhaseOneTestScripts` | 96 | 0 | 0 | 0 |
| **all** | **1,086** | **8,439** | **7,716** (91.4%) | **723** |

**Round trip.** Each of the 7,716 is printed back by `TransactSqlWriter`, read again by ScriptDom, and
ScriptDom's printing of that is compared with its printing of the original — so what is compared is
what the tree holds, not the formatting. 7,697 come back as the same statement; 19 do not, because the
writer refuses to print a construct the tree holds as an extension (a type of several words such as
`national text`, `WITH (0)`, a qualified column in an `INSERT` list, `JSON_OBJECTAGG()` of no pair,
…); none comes back as a different statement. The 19 are listed one by one, with the reason each fails,
in `CorpusRoundTripTests.Known`.

Source: `tests/DotGram.Tests.Slow/CorpusBaseline.txt`, the four counts of every file, which
`CorpusRoundTripTests` holds each file to: a fall in what is read or comes back, or a rise in what the
writer refuses, fails it. The round trip is printed by
`dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --roundtrip tests/Corpus/ScriptDom`.

### What is left out, and why

- **`PhaseOneTestScripts`** hold no statement ScriptDom reads: they are its tests of refusals
  (`This is not a valid TSql Statement.` heads the first), so they count nothing on either side.
- **What only this parser reads** is not counted: a statement is cut out of the corpus by ScriptDom's
  reading, so one ScriptDom refuses never reaches the comparison. The reference above is where that
  direction is measured, against the engine, and it is 0.
- **The 723 refused here** are not broken down by cause in any test. The corpus holds syntax for
  ScriptDom's own sake as well as SQL Server's: forms no level of a current engine reads
  (`COMPUTE`, the `*=` join), which ScriptDom reads at the old version a file is written for; forms of other products (Synapse's `OPENROWSET` over Cosmos DB, `PREDICT`); forms
  ScriptDom reads and the engine refuses (`INNER LOCAL MERGE JOIN`, answered by SQL Server 2025 with
  "'LOCAL' is not a recognized join option"); and forms of the newest releases this grammar does not
  read yet (`VECTOR_SEARCH`, `TOP n WITH APPROXIMATE`, `ORDER BY ALL`). The benchmarks' `--corpus`
  groups the query-shaped ones by where the reading stopped.

## PostgreSQL: the server's parser and its regression suite

`PostgreSqlParser` is held against two outside judges of its own.

- **The server's parser.** Every row of `tests/DotGram.Sql.Tests/Scripts/pg-oracle.json` is a text
  and the verdict PostgreSQL's grammar gave it: libpg_query 18.6, which is the server's `gram.y` and
  `scan.l` compiled apart from the server, syntax alone; where the live server answers otherwise,
  the row carries the server's verdict, and the server wins (20 rows). There are 550 rows, 361 read
  and 189 refused, and the parser answers each as recorded, both ways: what PostgreSQL reads and this
  refuses is the work list, what this reads and PostgreSQL refuses is a defect. 35 rows are known
  deviations, each with its reason in the fixture, 32 of them texts PostgreSQL reads that are not
  read here yet; `PostgreSqlOracleTests` holds every row, and fails a known deviation that is no
  longer one.
- **The regression suite.** `tests/Corpus/PostgreSql/sql` is `src/test/regress/sql` of
  PostgreSQL 18, 233 files, cut into statements as psql cuts them. Of the statements this grammar
  reads, every one is written back by `PostgreSqlWriter` and read again as the same tree. A schema
  as pg_dump writes it, `tests/Corpus/PostgreSql/dump`, is counted beside it.

| Corpus | Read | Refused | Read, written back, the same tree |
| --- | ---: | ---: | ---: |
| Regression suite | 40,633 | 9,733 | 40,633 |
| pg_dump of a schema | 83 | 519 | 83 |

`PgCorpusRoundTripTests` (in `DotGram.Tests.Slow`) holds every file to `PgCorpusBaseline.txt`: a file
may not read fewer statements, nor write back fewer, and none may throw. A statement that uses a psql
variable is read by nobody and not counted. The refused ones are not broken down by cause in any
test; among them are the statements this grammar does not read yet (`ALTER`, `CREATE INDEX`,
views, routines, triggers, `GRANT`), and those are most of the dump: of its statements, 341 are
`ALTER TABLE` or `ALTER INDEX` and 92 `CREATE INDEX`.

## The standard's parsers

`SqlStandardParser` (ISO SQL:2023, in part) and `Sql92Parser` are written from the standard's BNF, and
[`ast.md`](ast.md) maps every node of the tree to the production it stands for. There is no outside
corpus of standard SQL to hold them to, and no table by the standard's feature identifiers yet: what
they read is held by their own tests, not counted here.
