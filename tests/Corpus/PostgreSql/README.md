# PostgreSQL's regression suite

`sql/` is `src/test/regress/sql` of PostgreSQL, branch `REL_18_STABLE`, commit
`d6694976c88c038039c2b74d0aa4eaa4e083519c` (2026-11-04), copied byte for byte: 233
files, 4.9 MB. The licence is `COPYRIGHT` beside them, as it stands in the
PostgreSQL tree.

The files are exempt from the repository's line-ending normalization
(`.gitattributes`), as `../ScriptDom` is: one of them (`collate.windows.win1252.sql`)
is not UTF-8, and the text is theirs.

What it is for, and how it is cut and read, is in `../README.md` and in
`benchmarks/DotGram.Benchmarks/PgCorpus.cs`:

```
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --pg-corpus tests/Corpus/PostgreSql/sql
```
