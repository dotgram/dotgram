# What share of the nodes a parse builds does a consumer touch, 2026-09-20

The question, as the architect put it: of the nodes a parse builds, how many does a real consumer get hold of at all, so that it is known whether a lazily built tree (Roslyn's green and red) has anything to gain. It answers this and no other: it is a count of nodes fetched, not a size, a number of factories or a statistic of the tree.

**How.** `tool/` holds the rewriter and the harness (scratch, not part of the solution, named `.txt` so nothing compiles them). The rewriter takes a built `DotGram.Sql.dll` (here the one of 39d7d356, main at that time) and, with Mono.Cecil, makes every property getter of a node type that returns a reference type report its value to a static counter, and every constructor of a root of a tree count one node built. Three numbers per parse:

- **built**: nodes constructed while parsing, the ones a failed alternative built and threw away included;
- **reachable**: nodes in the returned tree (a walk over its properties, by reflection, with the counter off);
- **fetched**: nodes a consumer got hold of through a getter, with the counter on only around the consumer, plus the node the caller handed to it. **A getter that returns an array or a list counts every element in it as fetched**: for a consumer that takes an array and reads only some elements the true figure is lower, so fetched is an upper bound for such a consumer.

**Consumers.** (1) The writer, `SqlWriter.Write` (`Sql2023Writer.Write` for SQL:2023), what `--roundtrip` runs; it reads the whole tree by definition, so it is the control that the counter counts. (2) A host that asks a script the kind of each statement and does not go inside: the statement is touched and nothing under it. (2) is a count by construction (one node a statement), not a rewritten reading.

**Corpus.** ScriptDom's own (`tests/Corpus/ScriptDom`, 1,086 files), cut into statements by ScriptDom the way `--roundtrip` cuts them, each read by `TransactSqlParser.TryParseStatement`. 7,694 statements read by this grammar and written by the writer (665 refused, none threw). See `output.txt`.

## What it says

- **Built against reachable.** 61,171 nodes built for 48,248 in the returned trees: 78.9%. A fifth of what the T-SQL parser builds is built and thrown away. In SQL:2023 the same fraction is 98% on `select20` (93 built, 91 reachable) and 78% on 100 conditions (908, 708).
- **The control holds.** The writer fetches 47,471 of the 48,248 reachable nodes, 98.4%; in 165 statements it fetches fewer than are there, in none more. What it does not fetch are nodes it does not print.
- **The kinds host touches one node a statement.** On this corpus that is 15.95% of the reachable nodes and 12.58% of the built, but the corpus statements are small (median 4 nodes; 7,419 of the 7,694 have 20 nodes or fewer), and a one-node reading of a small statement is a large share by arithmetic. For the 275 statements with more than 20 nodes it is 3.00% of reachable and 2.30% of built. On the stand's own statements: `select20` in SQL:2023 has 91 nodes reachable, so 1.1%; `select20` in T-SQL 46 nodes, 2.2%; the T-SQL join row 37 nodes, 2.7%; 100 conditions in SQL:2023 708 nodes, 0.1%; 1,000 columns in T-SQL 2,003 nodes, 0.05%.
- **So** for a consumer that does not go inside a statement, a lazily built tree leaves 97-99% of a statement's nodes unbuilt on any statement of the size a script has; for a consumer that goes everywhere there is nothing to leave. The number in between, a consumer that reads some parts of each statement, is not measured here: it needs a consumer that reads some parts, and the two extremes only bound it.

## The middle consumer: a dependency scanner (the architect's second order)

For each statement, the names of the tables it refers to and nothing else (`tool/Harness.Scan.cs.txt`, output in `scan-output.txt`). Three readings of the same 2,250 statements the scanner has a case for (DML and the blocks that hold it; 1,202 of them refer to a table), against an oracle that reads every node:

- **Shallow**: only where a table reference can stand by the tree's shape: the sources of a query, the targets, joins (not their conditions), derived tables, the common table expressions in front. No select list, no condition, no value. It fetches **32.5%** of those statements' nodes (7,267 of 22,379) and **misses 92 table names in 78 statements** (of 1,654 names found): the tables inside a subquery that stands in a condition (`IS DISTINCT FROM ANY (SELECT ...)`), in an `OFFSET`/`FETCH`, in a `USING (SELECT ...)`. It adds no name that is not there.
- **Deep**: the same, and every clause and expression too, because a subquery can stand in any of them. It finds all 1,654 names, none missed. It fetches **97.6%** of those statements' nodes (21,834 of 22,379).
- Getting the deep scanner to miss nothing took five rounds of adding the places a subquery had turned out to stand: the columns, the conditions, the `TOP`, `OUTPUT` and `OPTION` clauses, `OFFSET`/`FETCH` under `ORDER BY`, the arguments of a table function. Each round was a place the tree's shape does not say a subquery may be.

**So the architect's "only those branches" is not executable on this tree without reading nearly everything**, if the scanner is to be right: nothing in the tree says where a subquery may stand, so a correct scan reads every clause and expression; the 2.4% of nodes it does not fetch I did not classify. A cheap partial walk exists only if the parser records the subqueries and table references of a statement as it builds it (a list on the statement), and then it is the parser's work moved, not the tree's shape. Counted by the instrumented getters over 7,694 statements of ScriptDom's corpus; other statement kinds (CREATE PROCEDURE and the rest, 5,444 of them) are outside both scanners.
