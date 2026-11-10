# PostgreSQL's regression suite

`sql/` is `src/test/regress/sql` of PostgreSQL, branch `REL_18_STABLE`, commit
`d6694976c88c038039c2b74d0aa4eaa4e083519c` (2026-11-04), copied byte for byte: 233
files, 4.6 MB. The licence is `COPYRIGHT` beside them, as it stands in the
PostgreSQL tree.

The files are exempt from the repository's line-ending normalization
(`.gitattributes`), as `../ScriptDom` is: one of them (`collate.windows.win1252.sql`)
is not UTF-8, and the text is theirs.

What it is for, and how it is cut and read, is in `../README.md` and in
`benchmarks/DotGram.Benchmarks/PgCorpus.cs`:

```
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --pg-corpus tests/Corpus/PostgreSql/sql
```

# A dump beside it

The suite tests the server and holds next to none of what a schema is written in when it is
kept or moved: `dump/pagila.sql` is the schema of Pagila — a sample database in the
PostgreSQL Licence, `https://github.com/devrimgunduz/pagila`, commit
`6ea53f812254c91a602f61c0ef62beca3fa676b1` — loaded from its `pagila-schema.sql` into
PostgreSQL 18.6 and written out again by its `pg_dump --schema-only`, as pg_dump 18 writes
it, `\restrict` lines and all. Pagila's licence is `dump/COPYRIGHT`. The dump leaves out the
one table that needs the `vector` extension, which the server it was loaded into does not have.

It is read by the same harness, with its own lines in the baseline under `dump/`:

```
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --pg-corpus tests/Corpus/PostgreSql/dump
```
