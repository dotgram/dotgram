# PostgreSQL — the published grammar

PostgreSQL publishes no BNF; its syntax is the Bison grammar and the flex lexer of the server
itself, which `PostgreSql.gram` is written from, nonterminal by nonterminal:

| Source | Where |
| --- | --- |
| `gram.y`, the grammar | <https://github.com/postgres/postgres/blob/REL_18_STABLE/src/backend/parser/gram.y> |
| `scan.l`, the lexer | <https://github.com/postgres/postgres/blob/REL_18_STABLE/src/backend/parser/scan.l> |
| `kwlist.h`, the key words and their classes | <https://github.com/postgres/postgres/blob/REL_18_STABLE/src/include/parser/kwlist.h> |
| The manual's lexical structure and keyword appendix | <https://www.postgresql.org/docs/18/sql-syntax-lexical.html>, <https://www.postgresql.org/docs/18/sql-keywords-appendix.html> |

Read at branch `REL_18_STABLE`, commit `ea24eadda01088e0fa43d7961244db4ffd9961e9` (PostgreSQL 18).
They are under the PostgreSQL Licence — use, copy, modify and distribute for any purpose, provided
the copyright notice and its two disclaimer paragraphs travel with any copy — and none of them is
copied here: what this directory holds is names and facts read from them.

- `gram-map.csv` — every nonterminal of `gram.y`, how many alternatives it has, and where it went:
  a rule of `PostgreSql.gram`, `token` for a fixed spelling its parent's node already says, `erased`
  for a rule that carries no information of its own, `refused` for a construct read by the server's
  grammar and not by this one (with the reason), or `unwritten` while it waits. Written by a script
  over `gram.y` so that the inventory starts complete, and held by `PostgreSqlGramMapTests`.
- `../KeywordCatalogue.cs` — the 494 key words with their classes (reserved, type or function
  name, column name, unreserved) and whether each may be a column label without `AS`, as
  `kwlist.h` lists them; `KeywordCatalogueTests` holds the grammar's word lists to it.

**The authority is the server's parser itself.** `libpg_query` is `gram.y` and `scan.l` compiled apart
from the server: it answers whether a text parses, with no catalogue and no connection, and what
its actions refuse it refuses too. Its verdicts over the tests' texts are recorded in
`tests/DotGram.Sql.Tests/Scripts/pg-oracle.json`, with the version that gave them, and the grammar is
held to them both ways: what PostgreSQL reads and this refuses is the work list, what this reads and
PostgreSQL refuses is a defect. A refusal that parse analysis makes after the grammar — a column
that is no column, `DEFAULT` where no default is taken, `FOR UPDATE OF` a qualified name — is not
syntax, and the grammar reads what the server's grammar reads.
