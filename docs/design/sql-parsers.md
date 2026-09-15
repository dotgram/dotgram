# The SQL parsers: one tree, independent grammars

A proposal and a list of work, not a description. What is decided is marked so; the rest
is open until it is built, and `status.md` is what will say when it has been.

## Decided

- **Every SQL grammar stands alone, and they meet only in the tree.** The standard's
  grammar is written from the ISO BNF and cites it rule by rule; T-SQL's from Microsoft's
  published syntax and the engine. Neither includes the other: a T-SQL rule inherited from
  the standard is explained by the wrong authority, and the dialect implements the
  standard partly and differently — no `RECURSIVE`, no `INTERSECT ALL`, no `FETCH` without
  `OFFSET`, no `WITH` inside a subquery — which rebinding can only express as subtraction.
  What is shared is `SqlSyntax.cs`, the writer and the walker; a statement both grammars
  read must build the same tree from either, and a test will hold them to it.
- **Versions are the one axis inside a grammar.** The standard's editions, `1992` to
  `2023`, are readings of one grammar (`parse … with (Edition = …)`), as T-SQL's
  compatibility levels are readings of T-SQL's. No grammar is both a dialect of another
  and a version of itself.
- **The tree follows the latest standard.** ISO/IEC 9075-2:2023 (edition 6) is the
  reference a node's name and shape answer to: `<query expression>` holds its `<with
  clause>`, its body and its `ORDER BY`, `OFFSET` and `FETCH`; `WITH` is one clause with
  `RECURSIVE` and a list, not a list of clauses. T-SQL's own nodes are marked as T-SQL's.
- **Order of work:** T-SQL first, to the end of its programme; then the standard's grammar
  on SQL:2023, and the tree reshaped to it.
- **The standard is written from the newest edition down** (Igor, 2026-09-13). SQL:2023 first,
  then each earlier edition as a reading of the same grammar. The goal is the standard as a
  whole, every edition; SQL-92 is kept only while something still leans on it, and is no fixed
  point to preserve: `SqlStandard92.gram` moves to a temporary `Sql92Parser`, which T-SQL
  includes and the benchmarks hold against a hand-written parser, and goes when T-SQL restates
  what it takes from it and 1992 is an edition of the new grammar.
- **Where the standard and T-SQL disagree about the tree, the standard wins.** It is formal and
  T-SQL is not; the reshaping is the test of the decisions taken while T-SQL alone shaped the
  tree, and T-SQL's measurements have to stay where they were through each of them.
- **The authority is the BNF itself, read by an Earley recognizer** (Igor, 2026-09-13). The
  standard has no engine to ask; a recognizer that reads `ISO_IEC_9075-2(E)_Foundation.bnf.txt`
  as a context-free grammar answers exactly what its productions allow, and the grammar is held
  to it the way T-SQL is held to SQL Server. What the Syntax Rules of the text narrow is written
  by hand, and says so.
- **The grammar starts from a converter's output** (Igor, 2026-09-13): a skeleton with the BNF's
  names, finished by hand, and a test that says which productions are not written yet.
- **One project, a directory and a namespace per dialect** (2026-09-13). The parsers are
  named for what they are, `SqlStandardParser` and `TransactSqlParser`, so that no namespace
  shares a name with a type in it. For now T-SQL still includes the standard's grammar; it
  stops when the SQL:2023 grammar replaces `SqlStandard92.gram`.
- **The reshaped tree has requirements of its own** (Igor, 2026-09-15): lossless, one-level
  hierarchies, enums and properties rather than the BNF's structure, validation outside the tree.
  They are in `sql-ast.md`, with the SQL:2023 blank `Standard/Sql2023Ast.cs` they start from and each
  adaptation made to it.
- **The specifications are kept beside the parsers that read them**, so that the answer
  to "why is it written so" is a file in the repository.

## To do

- ~~**An Earley recognizer over a BNF**~~ Done, 2026-09-13: `--standard` in DotGram.Benchmarks,
  which since 2026-09-14 also asks `SqlStandardParser`'s rule of the production's name and
  marks where the two differ.
- ~~**Convert a BNF into `.gram`.**~~ Done, 2026-09-13: `--bnf-gram` writes the skeleton.
- **The SQL:2023 grammar, chapter by chapter** (`Standard/SqlStandard.gram`). §5 is written —
  tokens, separators, literals, names and the reserved words — §6 with window functions, §7's query
  expression with the subqueries §6 and §8 held and row pattern recognition, §8, and §10.9's
  aggregates, the JSON functions as far as the BNF spells them, §14's data change statements, and the
  whole schema of §11 and §12 — routines, triggers and user-defined types among it — and the control,
  transaction, connection, session, diagnostics, dynamic and direct statements: every row tried and
  152,000 random verdicts agree with the BNF. Then the tree: begun 2026-09-14 in `sql-ast.md` and
  `Standard/Sql2023Ast.cs`, and `SqlStandardParser` builds none yet.
- **A test that says which productions are not written yet.**
- **Whether the standard reads through a lexical split.** Not for now: its tokens overlap — a
  date string is a character string too, and which one a token is depends on the key word before
  it — and a choice over characters can go back where one over tokens cannot. Asked again when
  the grammar is whole and fast enough matters.
- ~~**Split `DotGram.Parsers` into directories**~~ Done, 2026-09-13: SQL is a project and a
  package of its own, `DotGram.Sql` — the tree, its writer and walker in `DotGram.Sql`, and a
  directory and a namespace per dialect, `DotGram.Sql.Standard` and `DotGram.Sql.TransactSql`,
  with room for other databases beside them. `DotGram.Parsers` keeps the URI.
- ~~**T-SQL's published syntax as a file.**~~ Done: Microsoft publishes no BNF for T-SQL,
  and `--syntax` gathers the reference's syntax blocks into
  `src/DotGram.Sql/TransactSql/Specification/syntax.md` from a clone of
  MicrosoftDocs/sql-docs. Run again, it shows by diff what the documentation changed.
