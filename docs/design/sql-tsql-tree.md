# T-SQL on the SQL:2023 tree

A proposal, not a description. T-SQL moves from the tree in `SqlSyntax.cs` onto the SQL:2023 tree in
`Standard/Sql2023Ast.cs` (`design/sql-parsers.md`, decided 2026-09-15). Every node of the old tree is held
against the new one in [`sql-tsql-tree-inventory.md`](sql-tsql-tree-inventory.md): 381 records and enums,
67 with the same concept in the new tree, 44 whose shape needs a decision, 270 that only T-SQL has.

What is decided is marked so. The numbered proposals below, principles and all, were accepted by Igor as
written on 2026-09-15, and are decided too; they stay numbered so the work can cite them.

## Decided

- T-SQL's own nodes are typed records in the standard's families, declared in a partial file,
  `Sql2023Ast.TransactSql.cs`.
- The writer first (done: `Sql2023Writer`), then `Sql92Parser` and T-SQL switch together; every commit
  keeps `--engine` with no defects and `--roundtrip` at 100%.
- `[x]` is `IdentifierStyle.Bracketed`; `@x` and `@@x` are T-SQL's `Expression.Variable`; `0xFF` is read by
  each grammar as its dialect has it; T-SQL's `EXECUTE` of a module is a node of its own; `DROP … IF EXISTS`
  is a flag, with a list of names and a behavior that may be left out; `OFFSET` and `FETCH` keep the
  standard's shape; the walker follows lists (done); `ISqlSpan` stands apart from the old tree (done).

## Principles proposed

- **P1. A T-SQL node says it is one.** Beside the `// BNF:` comments requirement 21 asks for, a T-SQL node
  or property carries `// T-SQL:` and the page of Microsoft's reference it comes from.
- **P2. No text and no `Extension` for what T-SQL reads.** The round trip holds the tree to the text; a
  tail of words the writer copies back checks nothing. Every construct gets a typed place, as the old tree
  had come to for everything but the DDL option tails.
- **P3. A missing word is a property, not a twin.** Where the standard's node has the concept and lacks a
  word T-SQL writes, it gains an optional property; a T-SQL record of its own only where the structure
  differs (requirement 9).

## Proposals

### Queries and data change statements

1. **`SELECT`.** T-SQL builds `Statement.Select` as the standard does, with T-SQL properties for what it
   adds: `Top` (a `TopClause`: value, brackets, `PERCENT`, `WITH TIES`), `IntoTable` (`INTO t [ON fg]`,
   not the standard's `IntoClause` of variables), `For` (a family: `Xml`, `Json`, `Browse`) and `Hints`
   (`OPTION (…)`). `WITH XMLNAMESPACES` is a list on `WithClause`.
2. **DML targets.** The target of `INSERT`, `UPDATE`, `DELETE` and `MERGE` becomes a `TableSource` in both
   dialects — the standard's `ONLY (t)` and a name are a `TableSource.Named` already — so that T-SQL's
   alias, variable, hints and `OPENQUERY` target fit one shape. Each statement gains `With`, `Top`,
   `Output` (`OutputClause`, with `INTO` and a second `OUTPUT`) and `Hints`; `UPDATE` and `DELETE` gain
   T-SQL's second `From`; `INSERT` and `MERGE` whether `INTO` was written.
3. **`MERGE`.** `WHEN NOT MATCHED BY SOURCE` is a member of its own, `MergeClause.NotMatchedBySource`, with
   a matched clause's action; `BY TARGET` a flag on `NotMatched`; `INSERT DEFAULT VALUES` a flag on
   `MergeInsertAction`.
4. **`SET` in `UPDATE`.** `Assignment` gains `Operator` (`=`, `+=`, and the other compound ones) and T-SQL's
   `Through` (`@v = col = expr`); a variable as a target is an `Expression.Variable` beside the column's
   `AssignmentTarget`; `.WRITE (…)` a member call on the target.
5. **`GROUP BY ALL` and `WITH CUBE`.** `ALL` there is not the standard's quantifier, so it is a T-SQL flag,
   `AllKeyword`; `WITH CUBE` and `WITH ROLLUP` an enum on `GroupByClause`.
6. **`APPLY`, `PIVOT`, `UNPIVOT`.** `CROSS APPLY` and `OUTER APPLY` are T-SQL values of `JoinKind`, and a
   join hint (`HASH`, `LOOP`, `MERGE`, `REMOTE`) an enum on `Join`. `PIVOT` and `UNPIVOT` are table sources
   that follow another, as `RowPatternRecognition` does.
7. **Rowset functions.** `OPENROWSET`, `OPENJSON`, `OPENXML`, `OPENQUERY`, `OPENDATASOURCE`, `CHANGETABLE`,
   `CONTAINSTABLE`, `FREETEXTTABLE` and `PREDICT` are typed T-SQL table sources, and a `WITH (…)` schema a
   list of `RowsetColumn`. A table-valued function whose arguments are values stays `TableSource.Function`.
8. **Composable DML.** `(MERGE … OUTPUT …) AS c` is `TableSource.DataChange` with `Option` left out.
9. **Hints.** Table hints, query hints and join hints are T-SQL families with a named member for the long
   tail (`Named(Identifier, arguments)`), not text.

### Values

10. **Names.** Every T-SQL name rule builds `Identifier` and `QualifiedName`. `#t` and `##t` are regular
    identifiers whose text holds the prefix; `a..b` holds an empty part; `$action` and `$IDENTITY` are
    identifiers whose text holds the `$`.
11. **Literals.** Money is a T-SQL value of `NumericLiteralKind`; T-SQL's `0x0A` is `LiteralValue.Binary` with
    a flag saying it was written so rather than `X'0A'`.
12. **Functions.** A built-in whose arguments are values is `Expression.Invocation`. Those that take a type
    or a key word get typed T-SQL nodes: `Convert` (type, value, style, and `TRY_`), `Parse` (value, type,
    culture, and `TRY_`), and a date part (`DATEADD (day, …)`) as an argument node of its own. `NEXT VALUE
    FOR … OVER` is `NextValue` with a window.
13. **Operators.** `%`, `&`, `|`, `^`, `<<`, `>>` are T-SQL values of `BinaryOperator`, `~` of `UnaryOperator`,
    `!=`, `!<`, `!>` of `ComparisonOperator`.
14. **Data types.** A type the standard spells is built as the standard's (`INT`, `VARCHAR (n)`,
    `DECIMAL (p, s)`, `TIME (n)`), and the length gains `MAX`. The rest — `tinyint`, `bit`, `money`,
    `datetime2 (7)`, `uniqueidentifier`, `geography`, `xml (schema)`, `[int]`, `int (10)` — is one T-SQL
    member, `DataType.Named` (a name and its arguments), with `Xml` the one of its own.
15. **ODBC escapes and graph `MATCH`.** `{fn …}`, `{d '…'}` and `{ oj … }` are T-SQL nodes; a graph pattern
    is a family of its own, as requirement 19 allows.

### Statements

16. **`CREATE TABLE`.** T-SQL writes a column's options in any order, so a T-SQL column keeps them in the
    order written, in a `ColumnOption` family (default, `IDENTITY (1, 1)`, collation, constraint, `SPARSE`,
    `FILESTREAM`, `ROWGUIDCOL`, `MASKED`, `ENCRYPTED`, `PERSISTED`, `HIDDEN`, `NULL`); a standard column
    keeps its properties. The table gains placements (`ON`, `TEXTIMAGE_ON`, `FILESTREAM_ON`), options and its
    kind (`FILETABLE`, `NODE`, `EDGE`); an inline index is a `TableElement` member; a constraint gains
    clustering, `INCLUDE`, a filter and options.
17. **`ALTER TABLE`.** `Action` becomes a list in both dialects — the standard's is a list of one — and the
    T-SQL actions (`SWITCH`, `SET (…)`, `REBUILD`, `CHECK CONSTRAINT`, `ENABLE TRIGGER`, several columns
    added or dropped) are T-SQL members of `AlterTableAction`.
18. **Other drops.** The 56 T-SQL drops that differ only by what they drop are one record,
    `Statement.DropObject` (the kind as an enum, names, `IF EXISTS`); drops with parts of their own
    (`DROP INDEX … ON`, `DROP SIGNATURE`, `DROP TRIGGER … ON DATABASE`) have their own records.
19. **Routines and triggers.** A T-SQL body `AS s1; s2` is a list, `RoutineBody.Statements`; `BEGIN … END` is
    SQL/PSM's compound statement. `ParameterDefinition` gains `OUTPUT`, `READONLY`, `VARYING` and
    nullability; a routine gains its options, `CREATE`/`ALTER`/`CREATE OR ALTER` as an enum, and `;n`. A
    trigger's event becomes a list, with `FOR` as a spelling of `AFTER`, and a DDL trigger's target
    (`ON DATABASE`, `ON ALL SERVER`) with event groups.
20. **The DDL option catalogue.** One T-SQL `Option` record — a name and a value from a small family: a
    value, a list of options, a name — for every `WITH (…)` of the ~140 T-SQL DDL statements, which become
    typed records whose own parts are properties. Nothing stays as a tail of text.
21. **Transactions.** `BEGIN TRAN name WITH MARK` and `SAVE TRAN` are T-SQL statements; `Commit` and `Rollback`
    gain the word written (`WORK`, `TRAN`, `TRANSACTION`), a name and delayed durability; `IsolationLevel`
    gains `Snapshot`.
22. **Cursors.** T-SQL's options after `CURSOR` (`LOCAL`, `FORWARD_ONLY`, `STATIC`, `KEYSET`, `DYNAMIC`,
    `FAST_FORWARD`, `READ_ONLY`, `SCROLL_LOCKS`, `OPTIMISTIC`, `TYPE_WARNING`) are properties on
    `CursorProperties`; `DEALLOCATE` is a statement.
23. **Permissions.** T-SQL's permissions are a name of words (`VIEW DEFINITION`, `CREATE TABLE`) rather than
    the standard's enum; a securable gains its class (`OBJECT::`, `SCHEMA::`); `DENY` is a statement sharing
    the shape; `AS principal` is a name.
24. **`SET` options.** One flat `SetOption` family on `Statement.SetOptions`, the group an enum rather than a
    level.
25. **Procedural statements.** SQL/PSM's names where SQL/PSM has the statement (compound, `IF`, `WHILE`,
    variable declaration, assignment); T-SQL's own for the rest (`TRY … CATCH`, `PRINT`, `RAISERROR`,
    `THROW`, `GOTO`, labels, `BREAK`, `CONTINUE`, `WAITFOR`).
26. **Batches.** `Batch` and `GO` move into `DotGram.Sql.Ast` beside the statements.
27. **`StatementCategory`.** Leaves the tree; what reads it asks an extension method outside.
28. **The node map.** The standard's nodes answer to `Sql2023Ast.BnfMap.csv`; T-SQL's to a map of their own
    against the reference's pages, which takes over from `docs/ast.md`'s tables and `AstReferenceTests`.
