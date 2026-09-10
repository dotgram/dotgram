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
- **The specifications are kept beside the parsers that read them**, so that the answer
  to "why is it written so" is a file in the repository.

## To do

- **Convert a BNF into `.gram`.** A tool that reads the ISO BNF (and any BNF of the same
  shape) and writes a `.gram` skeleton: rule names kept from the BNF, `[ … ]` and `{ … }…`
  turned into `?` and `*`, the lexical part separated. The standard's grammar starts from
  its output, so that each rule is traceable to the BNF by name and a test can say which
  BNF rules a grammar has not written yet.
- **Split `DotGram.Parsers` into directories**, one per family and one per SQL dialect,
  with room for other databases.
- ~~**T-SQL's published syntax as a file.**~~ Done: Microsoft publishes no BNF for T-SQL,
  and `--syntax` gathers the reference's syntax blocks into
  `src/DotGram.Parsers/Sql/TransactSql/Specification/syntax.md` from a clone of
  MicrosoftDocs/sql-docs. Run again, it shows by diff what the documentation changed.

## Open

- The directory layout and whether the namespaces follow it.
- Whether `SqlStandard92` is replaced by the SQL:2023 grammar with 1992 as one edition, or
  kept apart.
