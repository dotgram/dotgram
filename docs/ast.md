# The tree

`src/DotGram.Sql/Ast` holds the one tree every SQL parser of the package builds, in the namespace
`DotGram.Sql.Ast`: `SqlStandardParser` reads SQL:2023 into it, `Sql92Parser` SQL-92, and
`TransactSqlParser` T-SQL as SQL Server reads it. `SqlAst.cs` is the standard's part, laid out by
`design/sql-ast.md`; `SqlAst.TransactSql*.cs` add T-SQL's own constructs, in the standard's families
where it has one and beside them where it does not. `SqlWriter` prints a tree back as SQL and
`TransactSqlWriter` as T-SQL reads it; `SqlWalker` visits it. This page is the map of that tree:
what is in it and where each node comes from, with T-SQL's nodes among the standard's.

## The rule

**A node is called what the production it comes from is called.** The standard's name where
the standard has the concept, the dialect's published name where it does not. So a reader
with the specification open can find the node, a reader with the node can find the
specification, and two dialects that read the same production build the same node rather than
one each. That is the point of having one tree at all: a text two parsers read is one tree.

Each record says where it comes from in its documentation: `BNF: <…>`, the standard's production,
or `T-SQL: <page> (Transact-SQL)`, the title of the page of Microsoft's reference. The tables below
say it for every node, from that comment, from the rule of `SqlStandard.gram` that builds the node
(whose names are the standard's productions), or from the family the node is in.

Three sources appear in the tables:

| Source | What it means |
| --- | --- |
| `SQL:2023` | ISO/IEC 9075:2023, by production. `SqlStandard.gram` is written from its BNF. |
| `SQL-92` | ISO/IEC 9075:1992, for what SQL-92 has and SQL:2003 took out: a string of bits. |
| `T-SQL` | Microsoft's reference, by the page's own title. Everything SQL Server has and the standard does not. |

## The shape

**A family per kind of thing, and no root above them.** `Statement`, `Query`, `Expression`,
`TableSource`, `DataType`, `SelectColumn` and a hundred and more beside them: each an abstract
record whose nodes are records nested in it, one level deep. A tree with a single root types
nothing: a field of it accepts a statement where a value belongs. So a field says which family it
holds — `Statement.Insert.Target` is a `TableSource`, `Expression.Subquery.Query` is a `Query` —
and a consumer switches over the nodes of the family it holds and has seen all of them. A literal
standing as a value is the `LiteralValue` itself, which is an `Expression`.

**A tree compares by what was written.** Every list a node holds is a `SqlList<T>`: one array,
read only, that compares element by element and hashes every element in order, with its length.
So the equality the compiler writes for each record compares the whole tree, and a tree equals its
own reparse; a record added later is right without anything written for it. `default` is the
empty list and equals `[]`; a list a record may leave out is a `SqlList<T>?`, where null and empty
stay different answers. A list is made from a collection expression or `SqlList.From`, and both
copy, so whoever made it holds no handle into the tree. Two equal subtrees written in different
places are equal too, so a table keyed by node is keyed by reference. A node's span takes no part
in its equality, so a located tree equals the plain tree of the same text. Equality and the hash
recurse; on a tree deeper than the stack has room for they throw
`InsufficientExecutionStackException`, which a caller can catch, rather than overflow the stack.

**A name is a value.** `QualifiedName` is its first `Identifier` and, where there are more, an
array of the others, so that a name of one part costs the node holding it nothing but its
identifier. A name keeps no span of its own: it spans its parts.

**The tree keeps the spelling beside the meaning.** One node stands for one meaning, and where
the language has two ways of writing it — `!=` and `<>`, `PROC` and `PROCEDURE`, `a AS x` and
`x = a`, `AS` written or left out — a flag beside it says which was written, so that a writer
gives the text back as it came. The writers print key words in capitals: the tree holds what a
key word means, not its case.

**What the tree has no place for yet is said, not dropped.** A construct a dialect reads and the
tree cannot hold yet comes back as an `Extension` — of a statement, an expression, a table source,
a data type or an action of `ALTER TABLE` — with the dialect, `T-SQL`, and a `Kind` that names
what it is. The writers refuse one; a reading never throws for one.

**And one record above them that is no node: `Batch`.** A script is a client's idea — the lines
that say `GO` are where a client cuts a file, and the server never sees them — so `ParseScript`
hands back a `Batch[]`, each holding the statements of one batch, the `GO` line that ended it and
in `Source` the batch as the script was cut (`ScriptBatch`, from `SqlScript.Read`). `Source` takes
no part in a batch's equality: two readings of one script give equal batches. A node's span in a
batch where a variable was substituted is a position in that batch's text, which
`Source.Locate` maps back to the script.

## How T-SQL reaches it

`TransactSql.gram` reads T-SQL in full and builds the tree in its actions. Where a construct is
one node, the rule builds the node: a value, a select list's item. Where a statement is assembled
from many parts, the rule hands what it read to a builder of `TransactSql/Building`, which makes
the statement's node at once, and where the text says more than the rule kept, reads the rest from
the text by the span the reading told it. A part with no place on the tree becomes the statement's
`Extension` when the statement is built, so a reading that meets one goes on.

**A node knows where it was written, where the reading asked.** `TransactSqlParser.Located` reads
with locations: each node is told the range of the rule that built it, and the last offer is kept.
A rule that only hands back a value another rule made offers nothing, so a clause's keyword is
never inside the value it introduces: the condition after `WHERE`, `HAVING` or `ON` and the
statement after `ELSE` each span their own text. An `OVER` is spanned whole, its keyword and the
name or the brackets after it, as the standard's reading spans it. The tree keeps no gap in front
of a node: `Span.GapStart` is `Span.At`. The plain reading tells each node the same range for its
own use and keeps none of them, so its nodes say nothing of where they were; a copy made with
`with` keeps its original's span.

**But the tree holds no text and no line numbers.** A span says where, in characters of the input
it was measured against; a consumer that wants the text cuts it from that input. What a span is
*for* is the thing no tree can hold: a comment falls between two spans, and the innermost node
containing it is the one it belongs to.

**Every statement says what it does.** `statement.Category()` (`StatementCategories`) gives a
`StatementCategory`: a query, rows changed (`Dml`), an object defined, changed or removed (`Ddl`),
a permission or the principal it names (`Dcl`), the flow of a batch (`Control`), a transaction,
the session, an `EXECUTE`, the server looked after (`Admin`), or a variable declared or set. It is
asked of the statement's type and kept by no record; the last column of the table of statements
says it, and a test reads that column against every record.

## Walking it

`SqlWalker.Walk(root, visit)` hands every node under `root` to `visit` — the root first, each node
before what it holds — until `visit` answers false, and says whether it went to the end. A node is
anything that is an `ISqlSpan`, which every node of the tree is; what a record holds of them, one or
a list, is found from its type once, so a record added here is walked without the walker being
told. A name hands out each of its parts, after the name.

It is how a question the parser does not answer gets asked. The parser reads what may be written;
whether a table somebody forbids was named is a check, and a check is a lambda that matches the
nodes it is about:

```csharp
var named = new List<QualifiedName>();

SqlWalker.Walk(statement, node =>
{
    if (node is TableSource.Named table)
        named.Add(table.Name);

    return true;
});
```

**Walk it all, not the branches where the thing is expected.** A subquery stands wherever a value
or a condition may stand — in a `WHERE`, in a column, under `OFFSET`/`FETCH`, in the arguments of a
table function — so the tables a statement reads are not only under its `FROM`, its joins and its
common table expressions. `SqlWalker.Walk` is what covers a statement whole.

## The nodes

Every record of every family that can be made, named by the family and the records it is nested in.
A family may be part of another — a `LiteralValue` is an `Expression` — and its records are named by
it. The families stand in alphabetical order, each family's records in the order they are declared.

### `Statement`

| Node | Source | Production or page | Category |
| --- | --- | --- | --- |
| `Statement.Select` | SQL:2023 | &lt;direct select statement: multiple rows&gt;, &lt;cursor specification&gt;, &lt;select statement: single row&gt;, &lt;dynamic single row select statement&gt; | Query |
| `Statement.Insert` | SQL:2023 | &lt;insert statement&gt; | Dml |
| `Statement.Update` | SQL:2023 | &lt;update statement: searched&gt;, &lt;update statement: positioned&gt;, dynamic/preparable positioned update | Dml |
| `Statement.Delete` | SQL:2023 | &lt;delete statement: searched&gt;, &lt;delete statement: positioned&gt;, dynamic/preparable positioned delete | Dml |
| `Statement.Merge` | SQL:2023 | &lt;merge statement&gt; | Dml |
| `Statement.TruncateTable` | SQL:2023 | &lt;truncate table statement&gt; | Ddl |
| `Statement.CreateSchema` | SQL:2023 | &lt;schema definition&gt; | Ddl |
| `Statement.CreateTable` | SQL:2023 | &lt;table definition&gt; | Ddl |
| `Statement.AlterTable` | SQL:2023 | &lt;alter table statement&gt; | Ddl |
| `Statement.DropSchema` | SQL:2023 | &lt;drop schema statement&gt; | Ddl |
| `Statement.DropTable` | SQL:2023 | &lt;drop table statement&gt; | Ddl |
| `Statement.CreateView` | SQL:2023 | &lt;view definition&gt; | Ddl |
| `Statement.DropView` | SQL:2023 | &lt;drop view statement&gt; | Ddl |
| `Statement.CreateDomain` | SQL:2023 | &lt;domain definition&gt;, &lt;alter domain statement&gt;, &lt;drop domain statement&gt; | Ddl |
| `Statement.AlterDomain` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.DropDomain` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.CreateCharacterSet` | SQL:2023 | character set/collation/translation/assertion statements | Ddl |
| `Statement.DropCharacterSet` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.CreateCollation` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.DropCollation` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.CreateTranslation` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.DropTranslation` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.CreateAssertion` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.DropAssertion` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.CreateTrigger` | SQL:2023 | trigger definition/drop trigger | Ddl |
| `Statement.DropTrigger` | T-SQL | DROP TRIGGER (Transact-SQL) | Ddl |
| `Statement.CreateType` | SQL:2023 | user-defined type/cast/ordering/transform statements | Ddl |
| `Statement.AlterType` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.DropType` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.CreateCast` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.DropCast` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.CreateOrdering` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.DropOrdering` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.CreateTransform` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.AlterTransform` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.DropTransform` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.CreateRoutine` | SQL:2023 | schema routine, alter/drop routine | Ddl |
| `Statement.AlterRoutine` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.DropRoutine` | T-SQL | DROP PROCEDURE (Transact-SQL) | Ddl |
| `Statement.CreateSequence` | SQL:2023 | sequence generator definition/alter/drop | Ddl |
| `Statement.AlterSequence` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.DropSequence` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Ddl |
| `Statement.Grant` | SQL:2023 | GRANT/REVOKE/ROLE | Dcl |
| `Statement.Revoke` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Dcl |
| `Statement.CreateRole` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Dcl |
| `Statement.DropRole` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Dcl |
| `Statement.DeclareCursor` | T-SQL | DECLARE CURSOR (Transact-SQL) | Declaration |
| `Statement.OpenCursor` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Control |
| `Statement.FetchCursor` | SQL:2023 | &lt;fetch statement&gt;, &lt;dynamic fetch statement&gt;: targets, or 'INTO [SQL] DESCRIPTOR d', and whether FROM was written | Control |
| `Statement.CloseCursor` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Control |
| `Statement.AllocateCursor` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Declaration |
| `Statement.DeclareLocalTemporaryTable` | SQL:2023 | &lt;temporary table declaration&gt; | Declaration |
| `Statement.GetDiagnostics` | SQL:2023 | &lt;get diagnostics statement&gt; | Control |
| `Statement.FreeLocator` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Session |
| `Statement.HoldLocator` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Session |
| `Statement.Call` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Execute |
| `Statement.Return` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Control |
| `Statement.StartTransaction` | SQL:2023 | transaction statements | Transaction |
| `Statement.SetTransaction` | T-SQL | SET TRANSACTION ISOLATION LEVEL (Transact-SQL) | Session |
| `Statement.SetConstraints` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Session |
| `Statement.Savepoint` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Transaction |
| `Statement.ReleaseSavepoint` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Transaction |
| `Statement.Commit` | T-SQL | COMMIT TRANSACTION, COMMIT WORK (Transact-SQL) | Transaction |
| `Statement.Rollback` | T-SQL | ROLLBACK TRANSACTION, ROLLBACK WORK (Transact-SQL) | Transaction |
| `Statement.Connect` | SQL:2023 | connection/session statements | Session |
| `Statement.SetConnection` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Session |
| `Statement.Disconnect` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Session |
| `Statement.SetSessionAuthorization` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Session |
| `Statement.SetRole` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Session |
| `Statement.SetTimeZone` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Session |
| `Statement.SetCatalog` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Session |
| `Statement.SetSchema` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Session |
| `Statement.SetNames` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Session |
| `Statement.SetPath` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Session |
| `Statement.SetTransformGroup` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Session |
| `Statement.SetCollation` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Session |
| `Statement.SetSessionCharacteristics` | SQL:2023 | &lt;session characteristic list&gt;: one list of modes per 'TRANSACTION ...' written | Session |
| `Statement.AllocateDescriptor` | SQL:2023 | diagnostics/descriptor/dynamic SQL | Declaration |
| `Statement.DeallocateDescriptor` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Declaration |
| `Statement.GetDescriptor` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Declaration |
| `Statement.SetDescriptor` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Declaration |
| `Statement.CopyDescriptor` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Declaration |
| `Statement.Prepare` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Execute |
| `Statement.DeallocatePrepare` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Execute |
| `Statement.Describe` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Execute |
| `Statement.Execute` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Execute |
| `Statement.ExecuteImmediate` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Execute |
| `Statement.PipeRow` | SQL:2023 | under top-level SQL statement containers and executable/schema/data/control/transaction/session/dynamic statement families | Control |
| `Statement.Whenever` | SQL:2023 | embedded WHENEVER and implementation-defined preparable statement escape hatch | Control |
| `Statement.Extension` | T-SQL | none: a construct a dialect reads and the tree has no place for yet, said by Dialect and Kind | Ddl |
| `Statement.CreateMessageType` | T-SQL | CREATE MESSAGE TYPE (Transact-SQL) | Ddl |
| `Statement.AlterMessageType` | T-SQL | ALTER MESSAGE TYPE (Transact-SQL) | Ddl |
| `Statement.CreateContract` | T-SQL | CREATE CONTRACT (Transact-SQL) | Ddl |
| `Statement.CreateQueue` | T-SQL | CREATE QUEUE (Transact-SQL) | Ddl |
| `Statement.AlterQueue` | T-SQL | ALTER QUEUE (Transact-SQL) | Ddl |
| `Statement.CreateService` | T-SQL | CREATE SERVICE (Transact-SQL) | Ddl |
| `Statement.AlterService` | T-SQL | ALTER SERVICE (Transact-SQL) | Ddl |
| `Statement.CreateRoute` | T-SQL | CREATE ROUTE (Transact-SQL) | Ddl |
| `Statement.AlterRoute` | T-SQL | ALTER ROUTE (Transact-SQL) | Ddl |
| `Statement.CreateRemoteServiceBinding` | T-SQL | CREATE REMOTE SERVICE BINDING (Transact-SQL) | Ddl |
| `Statement.AlterRemoteServiceBinding` | T-SQL | ALTER REMOTE SERVICE BINDING (Transact-SQL) | Ddl |
| `Statement.CreateBrokerPriority` | T-SQL | CREATE BROKER PRIORITY (Transact-SQL) | Ddl |
| `Statement.AlterBrokerPriority` | T-SQL | ALTER BROKER PRIORITY (Transact-SQL) | Ddl |
| `Statement.BeginDialog` | T-SQL | BEGIN DIALOG CONVERSATION (Transact-SQL) | Control |
| `Statement.BeginConversationTimer` | T-SQL | BEGIN CONVERSATION TIMER (Transact-SQL) | Control |
| `Statement.EndConversation` | T-SQL | END CONVERSATION (Transact-SQL) | Control |
| `Statement.MoveConversation` | T-SQL | MOVE CONVERSATION (Transact-SQL) | Control |
| `Statement.Send` | T-SQL | SEND (Transact-SQL) | Dml |
| `Statement.Receive` | T-SQL | RECEIVE (Transact-SQL) | Dml |
| `Statement.GetConversationGroup` | T-SQL | GET CONVERSATION GROUP (Transact-SQL) | Dml |
| `Statement.WaitForReceive` | T-SQL | WAITFOR (Transact-SQL) | Control |
| `Statement.WaitForGetConversationGroup` | T-SQL | WAITFOR (Transact-SQL) | Control |
| `Statement.DropAssembly` | T-SQL | DROP ASSEMBLY (Transact-SQL) | Ddl |
| `Statement.DropEventSession` | T-SQL | DROP EVENT SESSION (Transact-SQL) | Ddl |
| `Statement.DropEventNotification` | T-SQL | DROP EVENT NOTIFICATION (Transact-SQL) | Ddl |
| `Statement.DropExternalLibrary` | T-SQL | DROP EXTERNAL LIBRARY (Transact-SQL) | Ddl |
| `Statement.DropFullTextIndex` | T-SQL | DROP FULLTEXT INDEX (Transact-SQL) | Ddl |
| `Statement.DropSensitivityClassification` | T-SQL | DROP SENSITIVITY CLASSIFICATION (Transact-SQL) | Ddl |
| `Statement.DropMasterKey` | T-SQL | DROP MASTER KEY (Transact-SQL) | Ddl |
| `Statement.DropDatabaseEncryptionKey` | T-SQL | DROP DATABASE ENCRYPTION KEY (Transact-SQL) | Ddl |
| `Statement.DropIndex` | T-SQL | DROP INDEX (Transact-SQL) | Ddl |
| `Statement.DropSignature` | T-SQL | DROP SIGNATURE (Transact-SQL) | Ddl |
| `Statement.DeallocateCursor` | T-SQL | DEALLOCATE (Transact-SQL) | Control |
| `Statement.SetCursorVariable` | T-SQL | SET @local_variable (Transact-SQL) | Declaration |
| `Statement.BeginTransaction` | T-SQL | BEGIN TRANSACTION, BEGIN DISTRIBUTED TRANSACTION (Transact-SQL) | Transaction |
| `Statement.Deny` | T-SQL | DENY (Transact-SQL) | Dcl |
| `Statement.SaveTransaction` | T-SQL | SAVE TRANSACTION (Transact-SQL) | Transaction |
| `Statement.AlterDatabase` | T-SQL | ALTER DATABASE (Transact-SQL) | Ddl |
| `Statement.AlterDatabaseScopedConfiguration` | T-SQL | ALTER DATABASE SCOPED CONFIGURATION | Ddl |
| `Statement.CreateDatabase` | T-SQL | CREATE DATABASE (Transact-SQL) | Ddl |
| `Statement.BackupDatabase` | T-SQL | BACKUP (Transact-SQL) | Admin |
| `Statement.BackupLog` | T-SQL | BACKUP (Transact-SQL) | Admin |
| `Statement.BackupServer` | T-SQL | BACKUP (Transact-SQL) | Admin |
| `Statement.BackupGroup` | T-SQL | BACKUP (Transact-SQL) | Admin |
| `Statement.BackupCertificate` | T-SQL | BACKUP CERTIFICATE (Transact-SQL) | Admin |
| `Statement.BackupMasterKey` | T-SQL | BACKUP MASTER KEY (Transact-SQL) | Admin |
| `Statement.BackupServiceMasterKey` | T-SQL | BACKUP SERVICE MASTER KEY (Transact-SQL) | Admin |
| `Statement.BackupSymmetricKey` | T-SQL | BACKUP SYMMETRIC KEY (Transact-SQL) | Admin |
| `Statement.RestoreDatabase` | T-SQL | RESTORE (Transact-SQL) | Admin |
| `Statement.RestoreLog` | T-SQL | RESTORE (Transact-SQL) | Admin |
| `Statement.RestoreFileListOnly` | T-SQL | RESTORE FILELISTONLY (Transact-SQL) | Admin |
| `Statement.RestoreHeaderOnly` | T-SQL | RESTORE HEADERONLY (Transact-SQL) | Admin |
| `Statement.RestoreLabelOnly` | T-SQL | RESTORE LABELONLY (Transact-SQL) | Admin |
| `Statement.RestoreRewindOnly` | T-SQL | RESTORE REWINDONLY (Transact-SQL) | Admin |
| `Statement.RestoreVerifyOnly` | T-SQL | RESTORE VERIFYONLY (Transact-SQL) | Admin |
| `Statement.RestoreMasterKey` | T-SQL | RESTORE MASTER KEY (Transact-SQL) | Admin |
| `Statement.RestoreServiceMasterKey` | T-SQL | RESTORE SERVICE MASTER KEY (Transact-SQL) | Admin |
| `Statement.RestoreSymmetricKey` | T-SQL | RESTORE SYMMETRIC KEY (Transact-SQL) | Admin |
| `Statement.CreatePartitionFunction` | T-SQL | CREATE PARTITION FUNCTION (Transact-SQL) | Ddl |
| `Statement.AlterPartitionFunction` | T-SQL | ALTER PARTITION FUNCTION (Transact-SQL) | Ddl |
| `Statement.CreatePartitionScheme` | T-SQL | CREATE PARTITION SCHEME (Transact-SQL) | Ddl |
| `Statement.AlterPartitionScheme` | T-SQL | ALTER PARTITION SCHEME (Transact-SQL) | Ddl |
| `Statement.CreateFullTextCatalog` | T-SQL | CREATE FULLTEXT CATALOG (Transact-SQL) | Ddl |
| `Statement.AlterFullTextCatalog` | T-SQL | ALTER FULLTEXT CATALOG (Transact-SQL) | Ddl |
| `Statement.CreateFullTextStopList` | T-SQL | CREATE FULLTEXT STOPLIST (Transact-SQL) | Ddl |
| `Statement.AlterFullTextStopList` | T-SQL | ALTER FULLTEXT STOPLIST (Transact-SQL) | Ddl |
| `Statement.CreateSearchPropertyList` | T-SQL | CREATE SEARCH PROPERTY LIST (Transact-SQL) | Ddl |
| `Statement.AlterSearchPropertyList` | T-SQL | ALTER SEARCH PROPERTY LIST (Transact-SQL) | Ddl |
| `Statement.Checkpoint` | T-SQL | CHECKPOINT (Transact-SQL) | Admin |
| `Statement.BulkInsert` | T-SQL | BULK INSERT (Transact-SQL) | Dml |
| `Statement.InsertBulk` | T-SQL | INSERT BULK (no syntax block in syntax.md) | Dml |
| `Statement.ReadText` | T-SQL | READTEXT (Transact-SQL) | Dml |
| `Statement.WriteText` | T-SQL | WRITETEXT (Transact-SQL) | Dml |
| `Statement.UpdateText` | T-SQL | UPDATETEXT (Transact-SQL) | Dml |
| `Statement.DropAggregate` | T-SQL | DROP AGGREGATE (Transact-SQL) | Ddl |
| `Statement.DropDefault` | T-SQL | DROP DEFAULT (Transact-SQL) | Ddl |
| `Statement.DropRule` | T-SQL | DROP RULE (Transact-SQL) | Ddl |
| `Statement.DropSecurityPolicy` | T-SQL | DROP SECURITY POLICY (Transact-SQL) | Ddl |
| `Statement.DropSynonym` | T-SQL | DROP SYNONYM (Transact-SQL) | Ddl |
| `Statement.DropUser` | T-SQL | DROP USER (Transact-SQL) | Dcl |
| `Statement.DropContract` | T-SQL | DROP CONTRACT (Transact-SQL) | Ddl |
| `Statement.DropExternalTable` | T-SQL | DROP EXTERNAL TABLE (Transact-SQL) | Ddl |
| `Statement.DropMessageType` | T-SQL | DROP MESSAGE TYPE (Transact-SQL) | Ddl |
| `Statement.DropQueue` | T-SQL | DROP QUEUE (Transact-SQL) | Ddl |
| `Statement.DropRemoteServiceBinding` | T-SQL | DROP REMOTE SERVICE BINDING (Transact-SQL) | Ddl |
| `Statement.DropRoute` | T-SQL | DROP ROUTE (Transact-SQL) | Ddl |
| `Statement.DropService` | T-SQL | DROP SERVICE (Transact-SQL) | Ddl |
| `Statement.DropXmlSchemaCollection` | T-SQL | DROP XML SCHEMA COLLECTION (Transact-SQL) | Ddl |
| `Statement.DropStatistics` | T-SQL | DROP STATISTICS (Transact-SQL) | Ddl |
| `Statement.DropWorkloadClassifier` | T-SQL | DROP WORKLOAD Classifier (Transact-SQL) | Ddl |
| `Statement.DropApplicationRole` | T-SQL | DROP APPLICATION ROLE (Transact-SQL) | Dcl |
| `Statement.DropAvailabilityGroup` | T-SQL | DROP AVAILABILITY GROUP (Transact-SQL) | Ddl |
| `Statement.DropBrokerPriority` | T-SQL | DROP BROKER PRIORITY (Transact-SQL) | Ddl |
| `Statement.DropCertificate` | T-SQL | DROP CERTIFICATE (Transact-SQL) | Ddl |
| `Statement.DropColumnEncryptionKey` | T-SQL | DROP COLUMN ENCRYPTION KEY (Transact-SQL) | Ddl |
| `Statement.DropColumnMasterKey` | T-SQL | DROP COLUMN MASTER KEY (Transact-SQL) | Ddl |
| `Statement.DropCredential` | T-SQL | DROP CREDENTIAL (Transact-SQL) | Ddl |
| `Statement.DropCryptographicProvider` | T-SQL | DROP CRYPTOGRAPHIC PROVIDER (Transact-SQL) | Ddl |
| `Statement.DropDatabaseAuditSpecification` | T-SQL | DROP DATABASE AUDIT SPECIFICATION (Transact-SQL) | Ddl |
| `Statement.DropDatabaseScopedCredential` | T-SQL | DROP DATABASE SCOPED CREDENTIAL (Transact-SQL) | Ddl |
| `Statement.DropEndpoint` | T-SQL | DROP ENDPOINT (Transact-SQL) | Ddl |
| `Statement.DropExternalDataSource` | T-SQL | DROP EXTERNAL DATA SOURCE (Transact-SQL) | Ddl |
| `Statement.DropExternalFileFormat` | T-SQL | DROP EXTERNAL FILE FORMAT (Transact-SQL) | Ddl |
| `Statement.DropExternalModel` | T-SQL | DROP EXTERNAL MODEL (Transact-SQL) | Ddl |
| `Statement.DropExternalResourcePool` | T-SQL | DROP EXTERNAL RESOURCE POOL (Transact-SQL) | Ddl |
| `Statement.DropFullTextCatalog` | T-SQL | DROP FULLTEXT CATALOG (Transact-SQL) | Ddl |
| `Statement.DropFullTextStopList` | T-SQL | DROP FULLTEXT STOPLIST (Transact-SQL) | Ddl |
| `Statement.DropLogin` | T-SQL | DROP LOGIN (Transact-SQL) | Dcl |
| `Statement.DropPartitionFunction` | T-SQL | DROP PARTITION FUNCTION (Transact-SQL) | Ddl |
| `Statement.DropPartitionScheme` | T-SQL | DROP PARTITION SCHEME (Transact-SQL) | Ddl |
| `Statement.DropResourcePool` | T-SQL | DROP RESOURCE POOL (Transact-SQL) | Ddl |
| `Statement.DropSearchPropertyList` | T-SQL | DROP SEARCH PROPERTY LIST (Transact-SQL) | Ddl |
| `Statement.DropServerAudit` | T-SQL | DROP SERVER AUDIT (Transact-SQL) | Ddl |
| `Statement.DropServerAuditSpecification` | T-SQL | DROP SERVER AUDIT SPECIFICATION (Transact-SQL) | Ddl |
| `Statement.DropServerRole` | T-SQL | DROP SERVER ROLE (Transact-SQL) | Dcl |
| `Statement.DropWorkloadGroup` | T-SQL | DROP WORKLOAD GROUP (Transact-SQL) | Ddl |
| `Statement.DropAsymmetricKey` | T-SQL | DROP ASYMMETRIC KEY (Transact-SQL) | Ddl |
| `Statement.DropSymmetricKey` | T-SQL | DROP SYMMETRIC KEY (Transact-SQL) | Ddl |
| `Statement.DropExternalLanguage` | T-SQL | DROP EXTERNAL LANGUAGE (Transact-SQL) | Ddl |
| `Statement.DropDatabase` | T-SQL | DROP DATABASE (Transact-SQL) | Ddl |
| `Statement.CreateExternalDataSource` | T-SQL | CREATE EXTERNAL DATA SOURCE (Transact-SQL) | Ddl |
| `Statement.AlterExternalDataSource` | T-SQL | ALTER EXTERNAL DATA SOURCE (Transact-SQL) | Ddl |
| `Statement.CreateExternalFileFormat` | T-SQL | CREATE EXTERNAL FILE FORMAT (Transact-SQL) | Ddl |
| `Statement.CreateExternalTable` | T-SQL | CREATE EXTERNAL TABLE (Transact-SQL) | Ddl |
| `Statement.CreateExternalLibrary` | T-SQL | CREATE EXTERNAL LIBRARY (Transact-SQL) | Ddl |
| `Statement.AlterExternalLibrary` | T-SQL | ALTER EXTERNAL LIBRARY (Transact-SQL) | Ddl |
| `Statement.CreateExternalLanguage` | T-SQL | CREATE EXTERNAL LANGUAGE (Transact-SQL) | Ddl |
| `Statement.AlterExternalLanguage` | T-SQL | ALTER EXTERNAL LANGUAGE (Transact-SQL) | Ddl |
| `Statement.CreateExternalModel` | T-SQL | CREATE EXTERNAL MODEL (Transact-SQL) | Ddl |
| `Statement.AlterExternalModel` | T-SQL | ALTER EXTERNAL MODEL (Transact-SQL) | Ddl |
| `Statement.CreateExternalResourcePool` | T-SQL | CREATE EXTERNAL RESOURCE POOL (Transact-SQL) | Ddl |
| `Statement.AlterExternalResourcePool` | T-SQL | ALTER EXTERNAL RESOURCE POOL (Transact-SQL) | Ddl |
| `Statement.CreateEventNotification` | T-SQL | CREATE EVENT NOTIFICATION (Transact-SQL) | Ddl |
| `Statement.CreateServerAudit` | T-SQL | CREATE SERVER AUDIT (Transact-SQL) | Ddl |
| `Statement.AlterServerAudit` | T-SQL | ALTER SERVER AUDIT (Transact-SQL) | Ddl |
| `Statement.CreateServerAuditSpecification` | T-SQL | CREATE SERVER AUDIT SPECIFICATION (Transact-SQL) | Ddl |
| `Statement.AlterServerAuditSpecification` | T-SQL | ALTER SERVER AUDIT SPECIFICATION (Transact-SQL) | Ddl |
| `Statement.CreateDatabaseAuditSpecification` | T-SQL | CREATE DATABASE AUDIT SPECIFICATION | Ddl |
| `Statement.AlterDatabaseAuditSpecification` | T-SQL | ALTER DATABASE AUDIT SPECIFICATION (Transact-SQL) | Ddl |
| `Statement.CreateEventSession` | T-SQL | CREATE EVENT SESSION (Transact-SQL) | Ddl |
| `Statement.AlterEventSession` | T-SQL | ALTER EVENT SESSION (Transact-SQL) | Ddl |
| `Statement.Compound` | T-SQL | BEGIN...END (Transact-SQL) | Control |
| `Statement.If` | T-SQL | IF...ELSE (Transact-SQL) | Control |
| `Statement.While` | T-SQL | WHILE (Transact-SQL) | Control |
| `Statement.TryCatch` | T-SQL | TRY...CATCH (Transact-SQL) | Control |
| `Statement.Break` | T-SQL | BREAK (no syntax block in syntax.md) | Control |
| `Statement.Continue` | T-SQL | CONTINUE (no syntax block in syntax.md) | Control |
| `Statement.GoTo` | T-SQL | GOTO (Transact-SQL) | Control |
| `Statement.Label` | T-SQL | GOTO (Transact-SQL) | Control |
| `Statement.DeclareVariables` | T-SQL | DECLARE @local_variable (Transact-SQL) | Declaration |
| `Statement.SetVariable` | T-SQL | SET @local_variable (Transact-SQL) | Declaration |
| `Statement.SetOptions` | T-SQL | SET Statements (Transact-SQL) | Session |
| `Statement.Print` | T-SQL | PRINT (Transact-SQL) | Control |
| `Statement.RaiseError` | T-SQL | RAISERROR (Transact-SQL) | Control |
| `Statement.Throw` | T-SQL | THROW (Transact-SQL) | Control |
| `Statement.WaitFor` | T-SQL | WAITFOR (Transact-SQL) | Control |
| `Statement.ExecuteAs` | T-SQL | EXECUTE AS (Transact-SQL) | Session |
| `Statement.Revert` | T-SQL | REVERT (Transact-SQL) | Session |
| `Statement.SetUser` | T-SQL | SETUSER (Transact-SQL) | Session |
| `Statement.Use` | T-SQL | USE (Transact-SQL) | Session |
| `Statement.LineNumber` | T-SQL | LINENO (no syntax block in syntax.md) | Session |
| `Statement.ExecuteModule` | T-SQL | EXECUTE (Transact-SQL) | Execute |
| `Statement.ExecuteString` | T-SQL | EXECUTE (Transact-SQL) | Execute |
| `Statement.EnableTrigger` | T-SQL | ENABLE TRIGGER (Transact-SQL) | Ddl |
| `Statement.DisableTrigger` | T-SQL | DISABLE TRIGGER (Transact-SQL) | Ddl |
| `Statement.CreateAssembly` | T-SQL | CREATE ASSEMBLY (Transact-SQL) | Ddl |
| `Statement.AlterAssembly` | T-SQL | ALTER ASSEMBLY (Transact-SQL) | Ddl |
| `Statement.CreateRule` | T-SQL | CREATE RULE (Transact-SQL) | Ddl |
| `Statement.CreateDefault` | T-SQL | CREATE DEFAULT (Transact-SQL) | Ddl |
| `Statement.CreateAggregate` | T-SQL | CREATE AGGREGATE (Transact-SQL) | Ddl |
| `Statement.CreateXmlSchemaCollection` | T-SQL | CREATE XML SCHEMA COLLECTION (Transact-SQL) | Ddl |
| `Statement.AlterXmlSchemaCollection` | T-SQL | ALTER XML SCHEMA COLLECTION (Transact-SQL) | Ddl |
| `Statement.CreateClrType` | T-SQL | CREATE TYPE (Transact-SQL) | Ddl |
| `Statement.AlterRole` | T-SQL | ALTER ROLE (Transact-SQL) | Dcl |
| `Statement.CreateServerRole` | T-SQL | CREATE SERVER ROLE (Transact-SQL) | Dcl |
| `Statement.AlterServerRole` | T-SQL | ALTER SERVER ROLE (Transact-SQL) | Dcl |
| `Statement.CreateApplicationRole` | T-SQL | CREATE APPLICATION ROLE (Transact-SQL) | Dcl |
| `Statement.AlterApplicationRole` | T-SQL | ALTER APPLICATION ROLE (Transact-SQL) | Dcl |
| `Statement.CreateLogin` | T-SQL | CREATE LOGIN (Transact-SQL) | Dcl |
| `Statement.AlterLogin` | T-SQL | ALTER LOGIN (Transact-SQL) | Dcl |
| `Statement.CreateUser` | T-SQL | CREATE USER (Transact-SQL) | Dcl |
| `Statement.AlterUser` | T-SQL | ALTER USER (Transact-SQL) | Dcl |
| `Statement.AlterSchema` | T-SQL | ALTER SCHEMA (Transact-SQL) | Ddl |
| `Statement.AlterAuthorization` | T-SQL | ALTER AUTHORIZATION (Transact-SQL) | Dcl |
| `Statement.CreateAsymmetricKey` | T-SQL | CREATE ASYMMETRIC KEY (Transact-SQL) | Ddl |
| `Statement.AlterAsymmetricKey` | T-SQL | ALTER ASYMMETRIC KEY (Transact-SQL) | Ddl |
| `Statement.CreateSymmetricKey` | T-SQL | CREATE SYMMETRIC KEY (Transact-SQL) | Ddl |
| `Statement.AlterSymmetricKey` | T-SQL | ALTER SYMMETRIC KEY (Transact-SQL) | Ddl |
| `Statement.CreateCertificate` | T-SQL | CREATE CERTIFICATE (Transact-SQL) | Ddl |
| `Statement.AlterCertificate` | T-SQL | ALTER CERTIFICATE (Transact-SQL) | Ddl |
| `Statement.CreateMasterKey` | T-SQL | CREATE MASTER KEY (Transact-SQL) | Ddl |
| `Statement.AlterMasterKey` | T-SQL | ALTER MASTER KEY (Transact-SQL) | Ddl |
| `Statement.AlterServiceMasterKey` | T-SQL | ALTER SERVICE MASTER KEY (Transact-SQL) | Ddl |
| `Statement.CreateDatabaseEncryptionKey` | T-SQL | CREATE DATABASE ENCRYPTION KEY (Transact-SQL) | Ddl |
| `Statement.AlterDatabaseEncryptionKey` | T-SQL | ALTER DATABASE ENCRYPTION KEY (Transact-SQL) | Ddl |
| `Statement.CreateColumnEncryptionKey` | T-SQL | CREATE COLUMN ENCRYPTION KEY (Transact-SQL) | Ddl |
| `Statement.AlterColumnEncryptionKey` | T-SQL | ALTER COLUMN ENCRYPTION KEY (Transact-SQL) | Ddl |
| `Statement.CreateColumnMasterKey` | T-SQL | CREATE COLUMN MASTER KEY (Transact-SQL) | Ddl |
| `Statement.CreateCredential` | T-SQL | CREATE CREDENTIAL (Transact-SQL) | Ddl |
| `Statement.AlterCredential` | T-SQL | ALTER CREDENTIAL (Transact-SQL) | Ddl |
| `Statement.CreateDatabaseScopedCredential` | T-SQL | CREATE DATABASE SCOPED CREDENTIAL (Transact-SQL) | Ddl |
| `Statement.AlterDatabaseScopedCredential` | T-SQL | ALTER DATABASE SCOPED CREDENTIAL (Transact-SQL) | Ddl |
| `Statement.CreateSecurityPolicy` | T-SQL | CREATE SECURITY POLICY (Transact-SQL) | Ddl |
| `Statement.AlterSecurityPolicy` | T-SQL | ALTER SECURITY POLICY (Transact-SQL) | Ddl |
| `Statement.CreateCryptographicProvider` | T-SQL | CREATE CRYPTOGRAPHIC PROVIDER (Transact-SQL) | Ddl |
| `Statement.AlterCryptographicProvider` | T-SQL | ALTER CRYPTOGRAPHIC PROVIDER (Transact-SQL) | Ddl |
| `Statement.OpenSymmetricKey` | T-SQL | OPEN SYMMETRIC KEY (Transact-SQL) | Session |
| `Statement.OpenMasterKey` | T-SQL | OPEN MASTER KEY (Transact-SQL) | Session |
| `Statement.CloseSymmetricKey` | T-SQL | CLOSE SYMMETRIC KEY (Transact-SQL) | Session |
| `Statement.CloseAllSymmetricKeys` | T-SQL | CLOSE SYMMETRIC KEY (Transact-SQL) | Session |
| `Statement.CloseMasterKey` | T-SQL | CLOSE MASTER KEY (Transact-SQL) | Session |
| `Statement.AddSensitivityClassification` | T-SQL | ADD SENSITIVITY CLASSIFICATION (Transact-SQL) | Ddl |
| `Statement.AddSignature` | T-SQL | ADD SIGNATURE (Transact-SQL) | Ddl |
| `Statement.Kill` | T-SQL | KILL (Transact-SQL) | Admin |
| `Statement.KillStatsJob` | T-SQL | KILL STATS JOB (Transact-SQL) | Admin |
| `Statement.KillQueryNotificationSubscription` | T-SQL | KILL QUERY NOTIFICATION SUBSCRIPTION | Admin |
| `Statement.Reconfigure` | T-SQL | RECONFIGURE (Transact-SQL) | Admin |
| `Statement.Shutdown` | T-SQL | SHUTDOWN (Transact-SQL) | Admin |
| `Statement.Dbcc` | T-SQL | DBCC (Transact-SQL) | Admin |
| `Statement.AlterServerConfiguration` | T-SQL | ALTER SERVER CONFIGURATION (Transact-SQL) | Admin |
| `Statement.AlterResourceGovernor` | T-SQL | ALTER RESOURCE GOVERNOR (Transact-SQL) | Admin |
| `Statement.CreateResourcePool` | T-SQL | CREATE RESOURCE POOL (Transact-SQL) | Ddl |
| `Statement.AlterResourcePool` | T-SQL | ALTER RESOURCE POOL (Transact-SQL) | Ddl |
| `Statement.CreateWorkloadGroup` | T-SQL | CREATE WORKLOAD GROUP (Transact-SQL) | Ddl |
| `Statement.AlterWorkloadGroup` | T-SQL | ALTER WORKLOAD GROUP (Transact-SQL) | Ddl |
| `Statement.CreateAvailabilityGroup` | T-SQL | CREATE AVAILABILITY GROUP (Transact-SQL) | Ddl |
| `Statement.AlterAvailabilityGroup` | T-SQL | ALTER AVAILABILITY GROUP (Transact-SQL) | Ddl |
| `Statement.CreateEndpoint` | T-SQL | CREATE ENDPOINT (Transact-SQL) | Ddl |
| `Statement.AlterEndpoint` | T-SQL | ALTER ENDPOINT (Transact-SQL) | Ddl |
| `Statement.CreateIndex` | T-SQL | CREATE INDEX (Transact-SQL) | Ddl |
| `Statement.CreateXmlIndex` | T-SQL | CREATE XML INDEX (Transact-SQL) | Ddl |
| `Statement.CreateMaterializedView` | T-SQL | CREATE MATERIALIZED VIEW AS SELECT (Transact-SQL) | Ddl |
| `Statement.CreateJsonIndex` | T-SQL | CREATE JSON INDEX (Transact-SQL) | Ddl |
| `Statement.CreateSelectiveXmlIndex` | T-SQL | CREATE SELECTIVE XML INDEX (Transact-SQL) | Ddl |
| `Statement.CreateSpatialIndex` | T-SQL | CREATE SPATIAL INDEX (Transact-SQL) | Ddl |
| `Statement.AlterIndex` | T-SQL | ALTER INDEX (Transact-SQL) | Ddl |
| `Statement.CreateFullTextIndex` | T-SQL | CREATE FULLTEXT INDEX (Transact-SQL) | Ddl |
| `Statement.AlterFullTextIndex` | T-SQL | ALTER FULLTEXT INDEX (Transact-SQL) | Ddl |
| `Statement.CreateStatistics` | T-SQL | CREATE STATISTICS (Transact-SQL) | Ddl |
| `Statement.UpdateStatistics` | T-SQL | UPDATE STATISTICS (Transact-SQL) | Admin |
| `Statement.CreateTableType` | T-SQL | CREATE TYPE (Transact-SQL) | Ddl |
| `Statement.CreateAliasType` | T-SQL | CREATE TYPE (Transact-SQL) | Ddl |
| `Statement.CreateSynonym` | T-SQL | CREATE SYNONYM (Transact-SQL) | Ddl |

### Everything else

| Node | Source | Production or page |
| --- | --- | --- |
| `ActivationSetting.Status` | T-SQL | STATUS = ON or OFF — under CREATE QUEUE (Transact-SQL) |
| `ActivationSetting.ProcedureName` | T-SQL | PROCEDURE_NAME = procedure — under CREATE QUEUE (Transact-SQL) |
| `ActivationSetting.MaxQueueReaders` | T-SQL | MAX_QUEUE_READERS = n — under CREATE QUEUE (Transact-SQL) |
| `ActivationSetting.ExecuteAs` | T-SQL | EXECUTE AS SELF, EXECUTE AS OWNER (the name null), or EXECUTE AS 'user' (kind ExecutionContextKind.User and the name a string) — under CREATE QUEUE (Transact-SQL) |
| `AlterColumnAction.SetDefault` | SQL:2023 | &lt;alter column action&gt; |
| `AlterColumnAction.DropDefault` | SQL:2023 | &lt;alter column action&gt; |
| `AlterColumnAction.SetNotNull` | SQL:2023 | &lt;alter column action&gt; |
| `AlterColumnAction.DropNotNull` | SQL:2023 | &lt;alter column action&gt; |
| `AlterColumnAction.AddScope` | SQL:2023 | &lt;alter column action&gt; |
| `AlterColumnAction.DropScope` | SQL:2023 | &lt;alter column action&gt; |
| `AlterColumnAction.SetDataType` | SQL:2023 | &lt;alter column action&gt; |
| `AlterColumnAction.SetIdentityGeneration` | SQL:2023 | &lt;alter column action&gt; |
| `AlterColumnAction.IdentityOptions` | SQL:2023 | &lt;alter column action&gt; |
| `AlterColumnAction.DropIdentity` | SQL:2023 | &lt;alter column action&gt; |
| `AlterColumnAction.DropExpression` | SQL:2023 | &lt;alter column action&gt; |
| `AlterColumnAction.Property` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterDatabaseAction.Collate` | T-SQL | COLLATE collation_name — under ALTER DATABASE (Transact-SQL) |
| `AlterDatabaseAction.ModifyName` | T-SQL | MODIFY NAME = new_database_name — under ALTER DATABASE (Transact-SQL) |
| `AlterDatabaseAction.Modify` | T-SQL | ALTER DATABASE (Transact-SQL) |
| `AlterDatabaseAction.ModifyBackupStorageRedundancy` | T-SQL | MODIFY BACKUP_STORAGE_REDUNDANCY = one of three quoted words — under ALTER DATABASE (Transact-SQL) |
| `AlterDatabaseAction.PerformCutover` | T-SQL | PERFORM_CUTOVER, which takes nothing — under ALTER DATABASE (Transact-SQL) |
| `AlterDatabaseAction.RebuildLog` | T-SQL | REBUILD LOG [ON (filespec)] — under ALTER DATABASE (Transact-SQL) |
| `AlterDatabaseAction.AddFile` | T-SQL | ALTER DATABASE File and Filegroups |
| `AlterDatabaseAction.AddLogFile` | T-SQL | ALTER DATABASE File and Filegroups |
| `AlterDatabaseAction.ModifyFile` | T-SQL | ALTER DATABASE File and Filegroups |
| `AlterDatabaseAction.RemoveFile` | T-SQL | ALTER DATABASE File and Filegroups |
| `AlterDatabaseAction.AddFileGroup` | T-SQL | ALTER DATABASE File and Filegroups |
| `AlterDatabaseAction.ModifyFileGroup` | T-SQL | ALTER DATABASE File and Filegroups |
| `AlterDatabaseAction.RemoveFileGroup` | T-SQL | ALTER DATABASE File and Filegroups |
| `AlterDatabaseAction.Set` | T-SQL | ALTER DATABASE SET Options (Transact-SQL) |
| `AlterDomainAction.SetDefault` | SQL:2023 | &lt;alter domain action&gt; |
| `AlterDomainAction.DropDefault` | SQL:2023 | &lt;alter domain action&gt; |
| `AlterDomainAction.AddConstraint` | SQL:2023 | &lt;alter domain action&gt; |
| `AlterDomainAction.DropConstraint` | SQL:2023 | &lt;alter domain action&gt; |
| `AlterLoginAction.Enable` | T-SQL | ENABLE — under ALTER LOGIN (Transact-SQL) |
| `AlterLoginAction.Disable` | T-SQL | DISABLE — under ALTER LOGIN (Transact-SQL) |
| `AlterLoginAction.With` | T-SQL | WITH setting, … — under ALTER LOGIN (Transact-SQL) |
| `AlterLoginAction.AddCredential` | T-SQL | ADD CREDENTIAL credential_name — under ALTER LOGIN (Transact-SQL) |
| `AlterLoginAction.DropCredential` | T-SQL | DROP CREDENTIAL credential_name — under ALTER LOGIN (Transact-SQL) |
| `AlterTableAction.AddColumn` | SQL:2023 | &lt;alter table action&gt; |
| `AlterTableAction.AlterColumn` | SQL:2023 | &lt;alter table action&gt; |
| `AlterTableAction.DropColumn` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterTableAction.AddConstraint` | SQL:2023 | &lt;alter table action&gt; |
| `AlterTableAction.AlterConstraint` | SQL:2023 | &lt;alter table action&gt; |
| `AlterTableAction.DropConstraint` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterTableAction.AddPeriod` | SQL:2023 | under &lt;alter table action&gt; |
| `AlterTableAction.DropPeriod` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterTableAction.AddSystemVersioning` | SQL:2023 | &lt;alter table action&gt; |
| `AlterTableAction.DropSystemVersioning` | SQL:2023 | &lt;alter table action&gt; |
| `AlterTableAction.Extension` | T-SQL | none: a construct a dialect reads and the tree has no place for yet, said by Dialect and Kind |
| `AlterTableAction.Add` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterTableAction.AlterIndex` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterTableAction.DropIndex` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterTableAction.Drop` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterTableAction.Switch` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterTableAction.SetOptions` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterTableAction.Rebuild` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterTableAction.CheckConstraints` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterTableAction.Triggers` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterTableAction.ChangeTracking` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterTableAction.SplitRange` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterTableAction.MergeRange` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterTableAction.FileTableNamespace` | T-SQL | ALTER TABLE (Transact-SQL) |
| `AlterTypeAction.AddAttribute` | SQL:2023 | &lt;alter type action&gt; |
| `AlterTypeAction.DropAttribute` | SQL:2023 | &lt;alter type action&gt; |
| `AlterTypeAction.AddMethod` | SQL:2023 | &lt;alter type action&gt; |
| `AlterTypeAction.DropMethod` | SQL:2023 | &lt;alter type action&gt; |
| `AsymmetricKeyChange.RemovePrivateKey` | T-SQL | REMOVE PRIVATE KEY — under ALTER ASYMMETRIC KEY (Transact-SQL) |
| `AsymmetricKeyChange.WithPrivateKey` | T-SQL | WITH PRIVATE KEY (DECRYPTION BY PASSWORD = 'p', ENCRYPTION BY PASSWORD = 'q') — under ALTER ASYMMETRIC KEY (Transact-SQL) |
| `AsymmetricKeyChange.AttestedBy` | T-SQL | ATTESTED BY 'a' — under ALTER ASYMMETRIC KEY (Transact-SQL) |
| `AsymmetricKeyChange.RemoveAttestedOption` | T-SQL | REMOVE ATTESTED OPTION — under ALTER ASYMMETRIC KEY (Transact-SQL) |
| `AsymmetricKeySource.Provider` | T-SQL | FROM PROVIDER p — under CREATE ASYMMETRIC KEY (Transact-SQL) |
| `AsymmetricKeySource.File` | T-SQL | FROM [EXECUTABLE] FILE = 'path' — under CREATE ASYMMETRIC KEY (Transact-SQL) |
| `AsymmetricKeySource.Assembly` | T-SQL | FROM ASSEMBLY name — under CREATE ASYMMETRIC KEY (Transact-SQL) |
| `AtomicOption.Isolation` | T-SQL | CREATE PROCEDURE (Transact-SQL) |
| `AtomicOption.Setting` | T-SQL | CREATE PROCEDURE (Transact-SQL) |
| `AuditChange.RemoveWhere` | T-SQL | REMOVE WHERE — under ALTER SERVER AUDIT (Transact-SQL) |
| `AuditChange.ModifyName` | T-SQL | MODIFY NAME = name — under ALTER SERVER AUDIT (Transact-SQL) |
| `AuditChange.Settings` | T-SQL | [TO target] [WITH (setting, …)] [WHERE condition] — under ALTER SERVER AUDIT (Transact-SQL) |
| `AuditTarget.File` | T-SQL | FILE (setting, …) — under CREATE SERVER AUDIT (Transact-SQL) |
| `AuditTarget.ApplicationLog` | T-SQL | APPLICATION_LOG — under CREATE SERVER AUDIT (Transact-SQL) |
| `AuditTarget.SecurityLog` | T-SQL | SECURITY_LOG — under CREATE SERVER AUDIT (Transact-SQL) |
| `AuditTarget.Url` | T-SQL | URL, Managed Instance's, which the grammar reads by its published syntax — under CREATE SERVER AUDIT (Transact-SQL) |
| `AuditTarget.ExternalMonitor` | T-SQL | EXTERNAL_MONITOR, Azure's — under CREATE SERVER AUDIT (Transact-SQL) |
| `AuthorizationTo.Principal` | T-SQL | TO principal_name — under ALTER AUTHORIZATION (Transact-SQL) |
| `AuthorizationTo.SchemaOwner` | T-SQL | TO SCHEMA OWNER — under ALTER AUTHORIZATION (Transact-SQL) |
| `AvailabilityAction.Set` | T-SQL | SET (setting) — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityAction.AddDatabase` | T-SQL | ADD DATABASE db — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityAction.RemoveDatabase` | T-SQL | REMOVE DATABASE db — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityAction.AddReplica` | T-SQL | ADD REPLICA ON 'server' WITH (setting, …) — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityAction.ModifyReplica` | T-SQL | MODIFY REPLICA ON 'server' WITH (setting, …) — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityAction.RemoveReplica` | T-SQL | REMOVE REPLICA ON 'server' — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityAction.JoinAvailabilityGroup` | T-SQL | JOIN AVAILABILITY GROUP ON 'group' WITH (…) [, 'group' WITH (…)] — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityAction.ModifyAvailabilityGroup` | T-SQL | MODIFY AVAILABILITY GROUP ON 'group' WITH (…) [, 'group' WITH (…)] — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityAction.Join` | T-SQL | JOIN — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityAction.CreateAnyDatabase` | T-SQL | GRANT CREATE ANY DATABASE or DENY CREATE ANY DATABASE — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityAction.Failover` | T-SQL | FAILOVER — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityAction.ForceFailoverAllowDataLoss` | T-SQL | FORCE_FAILOVER_ALLOW_DATA_LOSS — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityAction.Offline` | T-SQL | OFFLINE — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityAction.AddListener` | T-SQL | ADD LISTENER 'name' (listener) — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityAction.ModifyListener` | T-SQL | MODIFY LISTENER 'name' (change) — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityAction.RestartListener` | T-SQL | RESTART LISTENER 'name' — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityAction.RemoveListener` | T-SQL | REMOVE LISTENER 'name' — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityListener.Dhcp` | T-SQL | WITH DHCP [ON ('address', 'mask')] — under CREATE AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilityListener.Ip` | T-SQL | WITH IP (('address', 'mask'), …) [, PORT = n] — under CREATE AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilitySource.Replicas` | T-SQL | FOR [DATABASE db, …] REPLICA ON 'server' WITH (setting, …), … — under CREATE AVAILABILITY GROUP (Transact-SQL) |
| `AvailabilitySource.Groups` | T-SQL | AVAILABILITY GROUP ON 'group' WITH (setting, …) [, 'group' WITH (…)], with no FOR — under CREATE AVAILABILITY GROUP (Transact-SQL) |
| `BackupDevice.Logical` | T-SQL | BACKUP (Transact-SQL) |
| `BackupDevice.Physical` | T-SQL | BACKUP (Transact-SQL) |
| `BackupFile.File` | T-SQL | BACKUP (Transact-SQL) |
| `BackupFile.FileGroup` | T-SQL | BACKUP (Transact-SQL) |
| `BackupFile.ReadWriteFileGroups` | T-SQL | BACKUP (Transact-SQL) |
| `BackupFile.Page` | T-SQL | RESTORE (Transact-SQL) |
| `CertificateChange.RemovePrivateKey` | T-SQL | REMOVE PRIVATE KEY — under ALTER CERTIFICATE (Transact-SQL) |
| `CertificateChange.WithPrivateKey` | T-SQL | WITH PRIVATE KEY (FILE = 'k', DECRYPTION BY PASSWORD = 'p', …) — under ALTER CERTIFICATE (Transact-SQL) |
| `CertificateChange.ActiveForBeginDialog` | T-SQL | WITH ACTIVE FOR BEGIN_DIALOG = ON (true) or OFF (false) — under ALTER CERTIFICATE (Transact-SQL) |
| `CertificateChange.AttestedBy` | T-SQL | ATTESTED BY 'a' — under ALTER CERTIFICATE (Transact-SQL) |
| `CertificateChange.RemoveAttestedOption` | T-SQL | REMOVE ATTESTED OPTION — under ALTER CERTIFICATE (Transact-SQL) |
| `CertificateSource.Assembly` | T-SQL | FROM ASSEMBLY name — under CREATE CERTIFICATE (Transact-SQL) |
| `CertificateSource.File` | T-SQL | FROM [EXECUTABLE] FILE = 'path' [WITH [FORMAT = 'f',] PRIVATE KEY (…)] — under CREATE CERTIFICATE (Transact-SQL) |
| `CertificateSource.Binary` | T-SQL | FROM BINARY = 0x… [WITH PRIVATE KEY (…)] — under CREATE CERTIFICATE (Transact-SQL) |
| `ColumnGeneration.Default` | SQL:2023 | &lt;column value source&gt; |
| `ColumnGeneration.Identity` | SQL:2023 | &lt;column value source&gt; |
| `ColumnGeneration.Generated` | SQL:2023 | &lt;column value source&gt; |
| `ColumnGeneration.RowStart` | SQL:2023 | &lt;column value source&gt; |
| `ColumnGeneration.RowEnd` | SQL:2023 | &lt;column value source&gt; |
| `ColumnKeyChange.AddValue` | T-SQL | ADD VALUE (…) — under ALTER COLUMN ENCRYPTION KEY (Transact-SQL) |
| `ColumnKeyChange.DropValue` | T-SQL | DROP VALUE (COLUMN_MASTER_KEY = m) — under ALTER COLUMN ENCRYPTION KEY (Transact-SQL) |
| `ColumnOption.Default` | T-SQL | column_definition (Transact-SQL) |
| `ColumnOption.Identity` | T-SQL | column_definition (Transact-SQL) |
| `ColumnOption.NotForReplication` | T-SQL | column_definition (Transact-SQL) |
| `ColumnOption.Collation` | T-SQL | column_definition (Transact-SQL) |
| `ColumnOption.Constraint` | T-SQL | column_constraint (Transact-SQL) |
| `ColumnOption.Null` | T-SQL | column_definition (Transact-SQL) |
| `ColumnOption.Computed` | T-SQL | computed_column_definition (Transact-SQL) |
| `ColumnOption.Persisted` | T-SQL | computed_column_definition (Transact-SQL) |
| `ColumnOption.Sparse` | T-SQL | column_definition (Transact-SQL) |
| `ColumnOption.Filestream` | T-SQL | column_definition (Transact-SQL) |
| `ColumnOption.RowGuidCol` | T-SQL | column_definition (Transact-SQL) |
| `ColumnOption.Hidden` | T-SQL | column_definition (Transact-SQL) |
| `ColumnOption.ColumnSet` | T-SQL | CREATE TABLE (Transact-SQL) |
| `ColumnOption.Masked` | T-SQL | column_definition (Transact-SQL) |
| `ColumnOption.Encrypted` | T-SQL | column_definition (Transact-SQL) |
| `ColumnOption.GeneratedAlways` | T-SQL | column_definition (Transact-SQL) |
| `ColumnOption.Index` | T-SQL | CREATE TABLE (Transact-SQL) |
| `ConditionAction.Continue` | SQL:2023 | under &lt;condition action&gt; |
| `ConditionAction.GoTo` | SQL:2023 | under &lt;condition action&gt; |
| `ConnectionObject.Default` | SQL:2023 | &lt;connection object&gt; |
| `ConnectionObject.Named` | SQL:2023 | &lt;connection object&gt; |
| `Constraint.NotNull` | SQL:2023 | &lt;column constraint&gt; |
| `Constraint.Unique` | SQL:2023 | &lt;column constraint&gt; |
| `Constraint.ForeignKey` | SQL:2023 | &lt;column constraint&gt; |
| `Constraint.Check` | SQL:2023 | &lt;check constraint definition&gt; |
| `Constraint.Default` | T-SQL | table_constraint (Transact-SQL) |
| `Constraint.Connection` | T-SQL | CREATE TABLE (SQL Graph) |
| `ConstraintTarget.All` | SQL:2023 | &lt;constraint name list&gt; |
| `ConstraintTarget.Names` | SQL:2023 | &lt;constraint name list&gt; |
| `CryptographicProviderChange.Enable` | T-SQL | ENABLE — under ALTER CRYPTOGRAPHIC PROVIDER (Transact-SQL) |
| `CryptographicProviderChange.Disable` | T-SQL | DISABLE — under ALTER CRYPTOGRAPHIC PROVIDER (Transact-SQL) |
| `CryptographicProviderChange.FromFile` | T-SQL | FROM FILE = 'path' — under ALTER CRYPTOGRAPHIC PROVIDER (Transact-SQL) |
| `CursorAllocationSource.Prepared` | SQL:2023 | &lt;allocate extended dynamic cursor statement&gt; |
| `CursorAllocationSource.Routine` | SQL:2023 | &lt;allocate received cursor statement&gt; |
| `CursorSource.Query` | T-SQL | under DECLARE CURSOR (Transact-SQL) |
| `CursorSource.Prepared` | T-SQL | under DECLARE CURSOR (Transact-SQL) |
| `DataType.Character` | T-SQL | char and varchar, nchar and nvarchar (Transact-SQL) |
| `DataType.Binary` | T-SQL | binary and varbinary (Transact-SQL) |
| `DataType.Numeric` | SQL:2023 | under &lt;data type&gt; and all predefined/row/reference/collection/user-defined type rules |
| `DataType.Boolean` | SQL:2023 | under &lt;data type&gt; and all predefined/row/reference/collection/user-defined type rules |
| `DataType.DateTime` | SQL:2023 | under &lt;data type&gt; and all predefined/row/reference/collection/user-defined type rules |
| `DataType.Interval` | SQL:2023 | under &lt;data type&gt; and all predefined/row/reference/collection/user-defined type rules |
| `DataType.Row` | SQL:2023 | under &lt;data type&gt; and all predefined/row/reference/collection/user-defined type rules |
| `DataType.Reference` | SQL:2023 | under &lt;data type&gt; and all predefined/row/reference/collection/user-defined type rules |
| `DataType.Array` | SQL:2023 | under &lt;data type&gt; and all predefined/row/reference/collection/user-defined type rules |
| `DataType.Multiset` | SQL:2023 | under &lt;data type&gt; and all predefined/row/reference/collection/user-defined type rules |
| `DataType.UserDefined` | SQL:2023 | under &lt;data type&gt; and all predefined/row/reference/collection/user-defined type rules |
| `DataType.Json` | SQL:2023 | under &lt;data type&gt; and all predefined/row/reference/collection/user-defined type rules |
| `DataType.Domain` | SQL:2023 | under &lt;data type&gt; and all predefined/row/reference/collection/user-defined type rules |
| `DataType.Descriptor` | SQL:2023 | under &lt;data type&gt; and all predefined/row/reference/collection/user-defined type rules |
| `DataType.GenericTable` | SQL:2023 | under &lt;data type&gt; and all predefined/row/reference/collection/user-defined type rules |
| `DataType.Extension` | T-SQL | none: a construct a dialect reads and the tree has no place for yet, said by Dialect and Kind |
| `DataType.BitString` | SQL-92 | §6.1 &lt;data type&gt;, &lt;bit string type&gt; |
| `DataType.Named` | T-SQL | Data types (Transact-SQL) |
| `DataType.Vector` | T-SQL | Vector Data Type |
| `DataType.Xml` | T-SQL | xml (Transact-SQL) |
| `DatabaseEncryptionKeyChange.Regenerate` | T-SQL | REGENERATE WITH ALGORITHM = a — under ALTER DATABASE ENCRYPTION KEY (Transact-SQL) |
| `DatabaseEncryptionKeyChange.EncryptedBy` | T-SQL | ENCRYPTION BY SERVER { CERTIFICATE / ASYMMETRIC KEY } name — under ALTER DATABASE ENCRYPTION KEY (Transact-SQL) |
| `DatabaseSource.ForAttach` | T-SQL | CREATE DATABASE (Transact-SQL) |
| `DatabaseSource.AsSnapshotOf` | T-SQL | CREATE DATABASE (Transact-SQL) |
| `DatabaseSource.AsCopyOf` | T-SQL | CREATE DATABASE (Transact-SQL) |
| `DescribeBody.Input` | SQL:2023 | &lt;describe statement&gt; |
| `DescribeBody.Output` | SQL:2023 | &lt;describe statement&gt; |
| `DescribeObject.Statement` | SQL:2023 | &lt;described object&gt; |
| `DescribeObject.Cursor` | SQL:2023 | &lt;described object&gt; |
| `DescriptorCopy.Whole` | SQL:2023 | under &lt;descriptor copy&gt; |
| `DescriptorCopy.Item` | SQL:2023 | under &lt;descriptor copy&gt; |
| `DescriptorGet.Header` | SQL:2023 | &lt;get descriptor information&gt; |
| `DescriptorGet.Value` | SQL:2023 | &lt;get descriptor information&gt; |
| `DescriptorSet.Header` | SQL:2023 | &lt;set descriptor information&gt; |
| `DescriptorSet.Value` | SQL:2023 | &lt;set descriptor information&gt; |
| `DiagnosticsInformation.Statement` | SQL:2023 | under &lt;SQL diagnostics information&gt;: a statement's items, a condition's items, or all of either |
| `DiagnosticsInformation.Condition` | SQL:2023 | under &lt;SQL diagnostics information&gt;: a statement's items, a condition's items, or all of either |
| `DiagnosticsInformation.All` | SQL:2023 | &lt;all information&gt;, ':t = ALL CONDITION 1'; Qualifier null where none was written |
| `DisconnectObject.Connection` | SQL:2023 | &lt;disconnect object&gt; |
| `DisconnectObject.All` | SQL:2023 | &lt;disconnect object&gt; |
| `DisconnectObject.Current` | SQL:2023 | &lt;disconnect object&gt; |
| `DroppedIndex.Named` | T-SQL | index_name ON object [ WITH ( … ) ] — under DROP INDEX (Transact-SQL) |
| `DroppedIndex.Qualified` | T-SQL | [ owner — under DROP INDEX (Transact-SQL) |
| `DynamicArguments.Values` | SQL:2023 | &lt;fetch statement&gt; |
| `DynamicArguments.Descriptor` | SQL:2023 | &lt;input using clause&gt; |
| `EmbeddedSql.Statement` | SQL:2023 | under embedded SQL host-program sections |
| `EmbeddedSql.DeclareSection` | SQL:2023 | under embedded SQL host-program sections |
| `EmbeddedSql.Authorization` | SQL:2023 | under embedded SQL host-program sections |
| `EmbeddedSql.Path` | SQL:2023 | under embedded SQL host-program sections |
| `EmbeddedSql.Transform` | SQL:2023 | under embedded SQL host-program sections |
| `EmbeddedSql.Collation` | SQL:2023 | under embedded SQL host-program sections |
| `EndpointPayload.Tsql` | T-SQL | FOR TSQL (setting, …) — under CREATE ENDPOINT (Transact-SQL) |
| `EndpointPayload.ServiceBroker` | T-SQL | FOR SERVICE_BROKER (setting, …) — under CREATE ENDPOINT (Transact-SQL) |
| `EndpointPayload.DatabaseMirroring` | T-SQL | FOR DATABASE_MIRRORING (setting, …) — under CREATE ENDPOINT (Transact-SQL) |
| `EndpointPayload.Soap` | T-SQL | FOR SOAP (item, …), SQL Server 2005's web services, removed in 2012 and still read — under CREATE ENDPOINT (Transact-SQL) |
| `EndpointTransport.Tcp` | T-SQL | AS TCP (setting, …) — under CREATE ENDPOINT (Transact-SQL) |
| `EndpointTransport.Http` | T-SQL | AS HTTP (setting, …), which SQL Server 2005 published and 2012 removed and the engine still reads — under CREATE ENDPOINT (Transact-SQL) |
| `EventPredicate.Join` | T-SQL | left AND right, left OR right — under CREATE EVENT SESSION (Transact-SQL) |
| `EventPredicate.Not` | T-SQL | NOT term — under CREATE EVENT SESSION (Transact-SQL) |
| `EventPredicate.Parenthesized` | T-SQL | (predicate) — under CREATE EVENT SESSION (Transact-SQL) |
| `EventPredicate.Call` | T-SQL | package.compare (field, value) — under CREATE EVENT SESSION (Transact-SQL) |
| `EventPredicate.Compare` | T-SQL | field = value (EventCompare) — under CREATE EVENT SESSION (Transact-SQL) |
| `EventPredicate.Like` | T-SQL | field LIKE value, field NOT LIKE value (EventCompare) — under CREATE EVENT SESSION (Transact-SQL) |
| `EventSessionPiece.AddEvent` | T-SQL | ADD EVENT name [(SET setting, … ACTION (name, …) WHERE predicate)] — under CREATE EVENT SESSION (Transact-SQL) |
| `EventSessionPiece.DropEvent` | T-SQL | DROP EVENT name — under CREATE EVENT SESSION (Transact-SQL) |
| `EventSessionPiece.AddTarget` | T-SQL | ADD TARGET name [(SET setting = value, …)] — under CREATE EVENT SESSION (Transact-SQL) |
| `EventSessionPiece.DropTarget` | T-SQL | DROP TARGET name — under CREATE EVENT SESSION (Transact-SQL) |
| `ExecuteOption.Named` | T-SQL | EXECUTE (Transact-SQL) |
| `ExecuteOption.ResultSets` | T-SQL | EXECUTE (Transact-SQL) |
| `Expression.Reference` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Parameter` | SQL:2023 | &lt;host parameter specification&gt;, &lt;embedded variable specification&gt;, &lt;dynamic parameter specification&gt; |
| `Expression.Current` | SQL:2023 | &lt;general value specification&gt;'s key words, &lt;datetime value function&gt; |
| `Expression.Unary` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Binary` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.MultisetOperation` | SQL:2023 | &lt;multiset value expression&gt;, &lt;multiset term&gt; — 'MULTISET UNION ALL', which carries a quantifier |
| `Expression.Row` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Parenthesized` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Subquery` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Case` | SQL:2023 | &lt;case expression&gt; |
| `Expression.CaseOperand` | SQL:2023 | &lt;case operand&gt; standing where a &lt;when operand&gt; leaves it out |
| `Expression.Cast` | T-SQL | TRY_CAST (Transact-SQL) |
| `Expression.Invocation` | SQL:2023 | &lt;routine invocation&gt;, &lt;aggregate function&gt;, &lt;window function&gt;, &lt;row pattern navigation operation&gt;, and the functions whose arguments are a plain comma list |
| `Expression.Asterisk` | SQL:2023 | 'COUNT (*)''s &lt;asterisk&gt;, as the one argument |
| `Expression.Member` | SQL:2023 | &lt;method invocation&gt;, &lt;attribute or method reference&gt;, &lt;static method invocation&gt; ('T::m'), &lt;dereference operation&gt; |
| `Expression.Generalized` | SQL:2023 | &lt;generalized expression&gt;, '(a AS t).m ()' |
| `Expression.Treat` | SQL:2023 | &lt;subtype treatment&gt;, &lt;new specification&gt;, &lt;reference resolution&gt; |
| `Expression.New` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Dereference` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Array` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Multiset` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.CollectionQuery` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Element` | SQL:2023 | &lt;array element reference&gt;, and a JSON simplified accessor's '[1 TO 3]' |
| `Expression.Wildcard` | T-SQL | 't.*' as an argument (CHECKSUM, BINARY_CHECKSUM, CONTAINS, FREETEXT): all columns of 't'; the target is the table's reference, as in 'SelectColumn.QualifiedAll' |
| `Expression.JsonAccessor` | SQL:2023 | a JSON simplified accessor's &lt;JSON array accessor&gt;, whose subscripts are in the path language: 'a[$ to last]' |
| `Expression.NextValue` | T-SQL | NEXT VALUE FOR (Transact-SQL) |
| `Expression.Collate` | SQL:2023 | &lt;collate clause&gt; on a character factor |
| `Expression.AtTimeZone` | SQL:2023 | &lt;time zone&gt;, 'a AT TIME ZONE z' and 'a AT LOCAL', where Zone is null |
| `Expression.IntervalQualified` | SQL:2023 | &lt;interval value expression&gt;'s '(a - b) DAY TO SECOND' |
| `Expression.Substring` | SQL:2023 | &lt;string value function&gt;s whose arguments are key words and not a comma list |
| `Expression.SubstringSimilar` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Trim` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Overlay` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Position` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Length` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Extract` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Normalize` | SQL:2023 | &lt;normalize function&gt;; a result length may carry a multiplier and its units, '10 K CHARACTERS' |
| `Expression.TranslateUsing` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Regex` | SQL:2023 | &lt;regex occurrences function&gt;, &lt;regex position expression&gt;, &lt;regex substring function&gt;, &lt;regex transliteration&gt; |
| `Expression.Comparison` | T-SQL | Comparison Operators (Transact-SQL) |
| `Expression.Between` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.In` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Like` | SQL:2023 | &lt;like predicate&gt;, &lt;similar predicate&gt;, &lt;regex like predicate&gt; — whose FLAG is Flag |
| `Expression.IsNull` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.IsTruth` | SQL:2023 | &lt;boolean test&gt;, 'a IS NOT UNKNOWN' |
| `Expression.QuantifiedComparison` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Exists` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Unique` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Match` | SQL:2023 | &lt;match predicate&gt; |
| `Expression.Overlaps` | SQL:2023 | &lt;overlaps predicate&gt;, whose two sides are rows |
| `Expression.IsDistinct` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.IsNormalized` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.MemberOf` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.SubmultisetOf` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.IsSet` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.IsOf` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.PeriodPredicate` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.JsonPredicate` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.JsonExists` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.JsonValue` | SQL:2023 | &lt;JSON value function&gt;, &lt;JSON query&gt;, the &lt;JSON value constructor&gt;s and &lt;JSON aggregate function&gt;s |
| `Expression.JsonQuery` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.JsonObject` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.JsonArray` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.JsonArrayQuery` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.JsonObjectAggregate` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.JsonArrayAggregate` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.JsonParse` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.JsonScalar` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.JsonSerialize` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.TableArgument` | SQL:2023 | &lt;table argument&gt;: 'TABLE (t)', 'TABLE (SELECT ...)' or a call, with its name, partitioning, pruning and order |
| `Expression.DescriptorConstructor` | SQL:2023 | &lt;descriptor value constructor&gt;, 'DESCRIPTOR (a INT, b)' |
| `Expression.CollationFor` | SQL:2023 | &lt;current collation specification&gt;, 'COLLATION FOR (x)' |
| `Expression.Default` | SQL:2023 | &lt;default specification&gt;, 'DEFAULT' where a value stands |
| `Expression.RowMarker` | SQL:2023 | &lt;row marker expression&gt;, 'END_FRAME - 1', and &lt;value_of expression at row&gt;, 'VALUE_OF (x AT BEGIN_FRAME, d)' |
| `Expression.ValueOf` | SQL:2023 | under &lt;value expression&gt; and all numeric/string/datetime/interval/boolean/predicate/value-primary subgrammars |
| `Expression.Extension` | T-SQL | none: a construct a dialect reads and the tree has no place for yet, said by Dialect and Kind |
| `Expression.FullTextPredicate` | T-SQL | CONTAINS (Transact-SQL) |
| `Expression.AiGenerateEmbeddings` | T-SQL | AI_GENERATE_EMBEDDINGS (Transact-SQL) |
| `Expression.Identity` | T-SQL | IDENTITY (Function) (Transact-SQL) |
| `Expression.Measured` | T-SQL | the WITH clause of the CREATE and ALTER statements |
| `Expression.OdbcEscape` | T-SQL | ODBC Scalar Functions (Transact-SQL) |
| `Expression.Variable` | T-SQL | DECLARE @local_variable (Transact-SQL) |
| `Expression.Convert` | T-SQL | CAST and CONVERT (Transact-SQL) |
| `Expression.Parse` | T-SQL | PARSE (Transact-SQL) |
| `Expression.DatePart` | T-SQL | DATEADD (Transact-SQL) |
| `Expression.PoolRanges` | T-SQL | CREATE EXTERNAL RESOURCE POOL (Transact-SQL) |
| `Expression.GraphMatch` | T-SQL | MATCH (SQL Graph) |
| `Expression.EndpointEncryption` | T-SQL | CREATE ENDPOINT (Transact-SQL) |
| `Expression.EndpointAuthentication` | T-SQL | CREATE ENDPOINT (Transact-SQL) |
| `Expression.EndpointAddress` | T-SQL | CREATE ENDPOINT (Transact-SQL) |
| `Expression.ServerRanges` | T-SQL | ALTER SERVER CONFIGURATION (Transact-SQL) |
| `ExternalLanguageChange.Set` | T-SQL | SET (file) — under ALTER EXTERNAL LANGUAGE (Transact-SQL) |
| `ExternalLanguageChange.Add` | T-SQL | ADD (file) — under ALTER EXTERNAL LANGUAGE (Transact-SQL) |
| `ExternalLanguageChange.RemovePlatform` | T-SQL | REMOVE PLATFORM platform — under ALTER EXTERNAL LANGUAGE (Transact-SQL) |
| `ExternalLibraryChange.Set` | T-SQL | SET (file), … — under ALTER EXTERNAL LIBRARY (Transact-SQL) |
| `ExternalLibraryChange.Add` | T-SQL | ADD (file), … — under ALTER EXTERNAL LIBRARY (Transact-SQL) |
| `FileGroupChange.Updatability` | T-SQL | Whether the filegroup may be written to — under ALTER DATABASE File and Filegroups |
| `FileGroupChange.Default` | T-SQL | DEFAULT, which takes nothing — under ALTER DATABASE File and Filegroups |
| `FileGroupChange.Rename` | T-SQL | NAME = new_filegroup_name — under ALTER DATABASE File and Filegroups |
| `FileGroupChange.AutoGrow` | T-SQL | AUTOGROW_SINGLE_FILE or AUTOGROW_ALL_FILES — under ALTER DATABASE File and Filegroups |
| `ForClause.Xml` | T-SQL | FOR XML RAW, AUTO, EXPLICIT or PATH, the element each row is written as (RAW ('row'), PATH ('')), which the grammar reads after every mode, and the directives i — under FOR Clause (Transact-SQL) |
| `ForClause.Json` | T-SQL | FOR JSON AUTO or PATH, and the directives in the order written — under FOR Clause (Transact-SQL) |
| `ForClause.Browse` | T-SQL | FOR BROWSE, which takes nothing — under FOR Clause (Transact-SQL) |
| `FullTextAlteration.Enable` | T-SQL | ALTER FULLTEXT INDEX (Transact-SQL) |
| `FullTextAlteration.Disable` | T-SQL | ALTER FULLTEXT INDEX (Transact-SQL) |
| `FullTextAlteration.SetChangeTracking` | T-SQL | ALTER FULLTEXT INDEX (Transact-SQL) |
| `FullTextAlteration.AddColumns` | T-SQL | ALTER FULLTEXT INDEX (Transact-SQL) |
| `FullTextAlteration.AlterColumn` | T-SQL | ALTER FULLTEXT INDEX (Transact-SQL) |
| `FullTextAlteration.DropColumns` | T-SQL | ALTER FULLTEXT INDEX (Transact-SQL) |
| `FullTextAlteration.Population` | T-SQL | ALTER FULLTEXT INDEX (Transact-SQL) |
| `FullTextAlteration.SetStopList` | T-SQL | ALTER FULLTEXT INDEX (Transact-SQL) |
| `FullTextAlteration.SetSearchPropertyList` | T-SQL | ALTER FULLTEXT INDEX (Transact-SQL) |
| `FullTextCatalogAction.Rebuild` | T-SQL | ALTER FULLTEXT CATALOG (Transact-SQL) |
| `FullTextCatalogAction.Reorganize` | T-SQL | ALTER FULLTEXT CATALOG (Transact-SQL) |
| `FullTextCatalogAction.AsDefault` | T-SQL | ALTER FULLTEXT CATALOG (Transact-SQL) |
| `GrantBody.Privileges` | T-SQL | GRANT (Transact-SQL) |
| `GrantBody.Roles` | SQL:2023 | &lt;grant statement&gt; |
| `Grantee.Public` | T-SQL | under GRANT (Transact-SQL) |
| `Grantee.Identifier` | T-SQL | under GRANT (Transact-SQL) |
| `Grantee.Null` | T-SQL | under GRANT (Transact-SQL) |
| `GraphPattern.Chain` | T-SQL | a-(e)-&gt;b&lt;-(f)-c — under MATCH (SQL Graph) |
| `GraphPattern.LastNodes` | T-SQL | LAST_NODE (a) = LAST_NODE (b) — under MATCH (SQL Graph) |
| `GraphPattern.ShortestPath` | T-SQL | SHORTEST_PATH (n(-(e)-&gt;m)+), the anchor first and each step written edge then node — under MATCH (SQL Graph) |
| `GroupingElement.Ordinary` | SQL:2023 | under grouping element families |
| `GroupingElement.Rollup` | SQL:2023 | under grouping element families |
| `GroupingElement.Cube` | SQL:2023 | under grouping element families |
| `GroupingElement.Sets` | SQL:2023 | under grouping element families |
| `GroupingElement.Empty` | SQL:2023 | under grouping element families |
| `HostDeclaration.Ada` | SQL:2023 | under &lt;host declaration&gt; |
| `HostDeclaration.C` | SQL:2023 | under &lt;host declaration&gt; |
| `HostDeclaration.Cobol` | SQL:2023 | under &lt;host declaration&gt; |
| `HostDeclaration.Fortran` | SQL:2023 | under &lt;host declaration&gt; |
| `HostDeclaration.Mumps` | SQL:2023 | under &lt;host declaration&gt; |
| `HostDeclaration.Pascal` | SQL:2023 | under &lt;host declaration&gt; |
| `HostDeclaration.Pli` | SQL:2023 | under &lt;host declaration&gt; |
| `HostParameterDeclaration.Parameter` | SQL:2023 | under &lt;host parameter declaration&gt;: a host parameter with its type, or the status parameter SQLSTATE |
| `HostParameterDeclaration.Status` | SQL:2023 | under &lt;host parameter declaration&gt;: a host parameter with its type, or the status parameter SQLSTATE |
| `InSource.Subquery` | SQL:2023 | &lt;negatable predicate part2&gt; |
| `InSource.Values` | SQL:2023 | &lt;negatable predicate part2&gt; |
| `IndexAlteration.Rebuild` | T-SQL | ALTER INDEX (Transact-SQL) |
| `IndexAlteration.Reorganize` | T-SQL | ALTER INDEX (Transact-SQL) |
| `IndexAlteration.Disable` | T-SQL | ALTER INDEX (Transact-SQL) |
| `IndexAlteration.SetOptions` | T-SQL | ALTER INDEX (Transact-SQL) |
| `IndexAlteration.Paths` | T-SQL | ALTER INDEX (Selective XML Indexes) |
| `IndexAlteration.Resume` | T-SQL | ALTER INDEX (Transact-SQL) |
| `IndexAlteration.Pause` | T-SQL | ALTER INDEX (Transact-SQL) |
| `IndexAlteration.Abort` | T-SQL | ALTER INDEX (Transact-SQL) |
| `InsertSource.Subquery` | SQL:2023 | &lt;insert values&gt; |
| `InsertSource.Values` | SQL:2023 | &lt;insert values&gt; |
| `InsertSource.DefaultValues` | SQL:2023 | &lt;insert columns and source&gt; |
| `InsertSource.ExecuteModule` | T-SQL | INSERT (Transact-SQL) |
| `InsertSource.ExecuteString` | T-SQL | INSERT (Transact-SQL) |
| `JoinSpecification.On` | SQL:2023 | &lt;join specification&gt; |
| `JoinSpecification.Using` | SQL:2023 | &lt;join specification&gt; |
| `JsonPathAccessor.Member` | SQL:2023 | under &lt;json path accessor&gt; |
| `JsonPathAccessor.WildcardMember` | SQL:2023 | &lt;json path accessor op&gt; |
| `JsonPathAccessor.Array` | SQL:2023 | &lt;primary step&gt; |
| `JsonPathAccessor.WildcardArray` | SQL:2023 | &lt;json path accessor op&gt; |
| `JsonPathAccessor.Filter` | SQL:2023 | &lt;json path accessor op&gt; |
| `JsonPathAccessor.Method` | SQL:2023 | &lt;json path accessor op&gt; |
| `JsonPathExpression.Literal` | SQL:2023 | under SQL/JSON path language |
| `JsonPathExpression.Variable` | SQL:2023 | under SQL/JSON path language |
| `JsonPathExpression.Unary` | SQL:2023 | under SQL/JSON path language |
| `JsonPathExpression.Binary` | SQL:2023 | under SQL/JSON path language |
| `JsonPathExpression.Access` | SQL:2023 | under SQL/JSON path language |
| `JsonPathExpression.Predicate` | SQL:2023 | under SQL/JSON path language |
| `JsonPathExpression.Parenthesized` | SQL:2023 | under SQL/JSON path language |
| `JsonPathPredicate.Exists` | SQL:2023 | &lt;json predicate primary&gt; |
| `JsonPathPredicate.Comparison` | SQL:2023 | &lt;json predicate primary&gt; |
| `JsonPathPredicate.LikeRegex` | SQL:2023 | under &lt;json path predicate&gt; |
| `JsonPathPredicate.StartsWith` | SQL:2023 | under &lt;json path predicate&gt; |
| `JsonPathPredicate.IsUnknown` | SQL:2023 | &lt;json predicate primary&gt; |
| `JsonPathPredicate.Not` | SQL:2023 | under &lt;json path predicate&gt; |
| `JsonPathPredicate.And` | SQL:2023 | under &lt;json path predicate&gt; |
| `JsonPathPredicate.Or` | SQL:2023 | under &lt;json path predicate&gt; |
| `JsonPathPredicate.Parenthesized` | SQL:2023 | &lt;json predicate primary&gt; |
| `JsonTableColumn.Ordinality` | SQL:2023 | &lt;json table column definition&gt; |
| `JsonTableColumn.Regular` | SQL:2023 | under &lt;json table column&gt; |
| `JsonTableColumn.Formatted` | SQL:2023 | under &lt;json table column&gt; |
| `JsonTableColumn.Nested` | SQL:2023 | &lt;json table column definition&gt; |
| `JsonTableColumn.Chaining` | SQL:2023 | &lt;json table primitive column&gt; |
| `JsonTablePlan.Name` | SQL:2023 | &lt;json table plan primary&gt; |
| `JsonTablePlan.Outer` | SQL:2023 | under &lt;json table plan&gt; |
| `JsonTablePlan.Inner` | SQL:2023 | under &lt;json table plan&gt; |
| `JsonTablePlan.Union` | SQL:2023 | under &lt;json table plan&gt; |
| `JsonTablePlan.Cross` | SQL:2023 | under &lt;json table plan&gt; |
| `JsonTablePlan.Default` | SQL:2023 | under &lt;json table plan&gt; |
| `JsonTablePlan.Parenthesized` | SQL:2023 | &lt;json table plan primary&gt; |
| `JsonValueBehavior.Error` | SQL:2023 | &lt;json value behavior&gt; |
| `JsonValueBehavior.Null` | SQL:2023 | &lt;json value behavior&gt; |
| `JsonValueBehavior.Default` | SQL:2023 | &lt;json value behavior&gt; |
| `KeyEncryptor.Password` | T-SQL | PASSWORD = 'p' — under CREATE SYMMETRIC KEY (Transact-SQL) |
| `KeyEncryptor.Certificate` | T-SQL | CERTIFICATE c — under CREATE SYMMETRIC KEY (Transact-SQL) |
| `KeyEncryptor.SymmetricKey` | T-SQL | SYMMETRIC KEY k — under CREATE SYMMETRIC KEY (Transact-SQL) |
| `KeyEncryptor.AsymmetricKey` | T-SQL | ASYMMETRIC KEY k — under CREATE SYMMETRIC KEY (Transact-SQL) |
| `KeyEncryptor.ServiceMasterKey` | T-SQL | ALTER MASTER KEY (Transact-SQL) |
| `ListenerChange.AddIp` | T-SQL | ADD IP ('address' [, 'mask']) — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `ListenerChange.Port` | T-SQL | PORT = n, whole digits — under ALTER AVAILABILITY GROUP (Transact-SQL) |
| `LiteralValue.Numeric` | SQL:2023 | under &lt;literal value&gt; |
| `LiteralValue.String` | SQL:2023 | Text is the literal's spelling as written, from its first quote to its last — under &lt;literal value&gt; |
| `LiteralValue.Binary` | T-SQL | Constants (Transact-SQL) |
| `LiteralValue.Boolean` | SQL:2023 | under &lt;literal value&gt; |
| `LiteralValue.DateTime` | SQL:2023 | &lt;date literal&gt; |
| `LiteralValue.Interval` | SQL:2023 | the sign written after INTERVAL, null where none was — &lt;interval literal&gt; |
| `LiteralValue.Null` | SQL:2023 | &lt;value&gt; |
| `LiteralValue.BitString` | SQL-92 | §5.3 &lt;bit string literal&gt; |
| `LoginSource.Password` | T-SQL | WITH PASSWORD = 'p' [HASHED] [MUST_CHANGE] — under CREATE LOGIN (Transact-SQL) |
| `LoginSource.Windows` | T-SQL | FROM WINDOWS — under CREATE LOGIN (Transact-SQL) |
| `LoginSource.ExternalProvider` | T-SQL | FROM EXTERNAL PROVIDER — under CREATE LOGIN (Transact-SQL) |
| `LoginSource.Certificate` | T-SQL | FROM CERTIFICATE c [WITH CREDENTIAL = cr] — under CREATE LOGIN (Transact-SQL) |
| `LoginSource.AsymmetricKey` | T-SQL | FROM ASYMMETRIC KEY k [WITH CREDENTIAL = cr] — under CREATE LOGIN (Transact-SQL) |
| `MasterKeyChange.Regenerate` | T-SQL | [FORCE] REGENERATE WITH ENCRYPTION BY PASSWORD = 'p' — under ALTER MASTER KEY (Transact-SQL) |
| `MasterKeyChange.AddEncryption` | T-SQL | ADD ENCRYPTION BY { SERVICE MASTER KEY / PASSWORD = 'p' } — under ALTER MASTER KEY (Transact-SQL) |
| `MasterKeyChange.DropEncryption` | T-SQL | DROP ENCRYPTION BY { SERVICE MASTER KEY / PASSWORD = 'p' } — under ALTER MASTER KEY (Transact-SQL) |
| `MergeClause.Matched` | SQL:2023 | &lt;merge when clause&gt; |
| `MergeClause.NotMatched` | SQL:2023 | &lt;merge when clause&gt; |
| `MergeClause.NotMatchedBySource` | T-SQL | MERGE (Transact-SQL) |
| `MergeMatchedAction.Update` | SQL:2023 | &lt;merge matched then&gt; |
| `MergeMatchedAction.Delete` | SQL:2023 | &lt;merge matched then&gt; |
| `MessageValidation.Named` | T-SQL | VALIDATION = word — under CREATE MESSAGE TYPE (Transact-SQL) |
| `MessageValidation.ValidXml` | T-SQL | VALIDATION = VALID_XML WITH SCHEMA COLLECTION name — under CREATE MESSAGE TYPE (Transact-SQL) |
| `ModuleContent.Cursor` | SQL:2023 | under &lt;module contents&gt;: a cursor declared, statically or dynamically, or an &lt;externally-invoked procedure&gt; |
| `ModuleContent.Procedure` | SQL:2023 | under &lt;module contents&gt;: a cursor declared, statically or dynamically, or an &lt;externally-invoked procedure&gt; |
| `ModuleReference.Named` | T-SQL | EXECUTE (Transact-SQL) |
| `ModuleReference.Variable` | T-SQL | EXECUTE (Transact-SQL) |
| `NotificationScope.Server` | T-SQL | ON SERVER — under DROP EVENT NOTIFICATION (Transact-SQL) |
| `NotificationScope.Database` | T-SQL | ON DATABASE — under DROP EVENT NOTIFICATION (Transact-SQL) |
| `NotificationScope.Queue` | T-SQL | ON QUEUE queue_name — under DROP EVENT NOTIFICATION (Transact-SQL) |
| `OrderingCategory.Relative` | SQL:2023 | &lt;ordering category&gt; |
| `OrderingCategory.Map` | SQL:2023 | &lt;ordering category&gt; |
| `OrderingCategory.State` | SQL:2023 | &lt;ordering category&gt; |
| `PartitionFunctionAction.Split` | T-SQL | ALTER PARTITION FUNCTION (Transact-SQL) |
| `PartitionFunctionAction.Merge` | T-SQL | ALTER PARTITION FUNCTION (Transact-SQL) |
| `PathAction.Add` | T-SQL | ALTER INDEX (Selective XML Indexes) |
| `PathAction.Alter` | T-SQL | ALTER INDEX (Selective XML Indexes) |
| `PathAction.Remove` | T-SQL | ALTER INDEX (Selective XML Indexes) |
| `PathHint.XQuery` | T-SQL | CREATE SELECTIVE XML INDEX (Transact-SQL) |
| `PathHint.Sql` | T-SQL | CREATE SELECTIVE XML INDEX (Transact-SQL) |
| `PathHint.Singleton` | T-SQL | CREATE SELECTIVE XML INDEX (Transact-SQL) |
| `PeriodRight.Period` | SQL:2023 | &lt;period predicate part2&gt; |
| `PeriodRight.Point` | SQL:2023 | &lt;period contained&gt; |
| `PeriodValue.Reference` | SQL:2023 | &lt;period predicand&gt; |
| `PeriodValue.Range` | SQL:2023 | &lt;period constructor&gt; |
| `PolicyPredicate.AddFilter` | T-SQL | ADD FILTER PREDICATE f(c) ON t — under CREATE SECURITY POLICY (Transact-SQL) |
| `PolicyPredicate.AlterFilter` | T-SQL | ALTER FILTER PREDICATE f(c) ON t — under CREATE SECURITY POLICY (Transact-SQL) |
| `PolicyPredicate.DropFilter` | T-SQL | DROP FILTER PREDICATE ON t — under CREATE SECURITY POLICY (Transact-SQL) |
| `PolicyPredicate.AddBlock` | T-SQL | ADD BLOCK PREDICATE f(c) ON t [AFTER INSERT] — under CREATE SECURITY POLICY (Transact-SQL) |
| `PolicyPredicate.AlterBlock` | T-SQL | ALTER BLOCK PREDICATE f(c) ON t [AFTER INSERT] — under CREATE SECURITY POLICY (Transact-SQL) |
| `PolicyPredicate.DropBlock` | T-SQL | DROP BLOCK PREDICATE ON t [AFTER INSERT] — under CREATE SECURITY POLICY (Transact-SQL) |
| `PrivilegeObject.Named` | T-SQL | GRANT Object Permissions (Transact-SQL) |
| `PrivilegeObject.Classed` | T-SQL | GRANT (Transact-SQL) |
| `Query.Select` | SQL:2023 | &lt;query specification&gt;, &lt;table value constructor&gt;, &lt;explicit table&gt; |
| `Query.Parenthesized` | SQL:2023 | A pair of brackets written around a query — under &lt;query expression&gt;, &lt;query expression body&gt;, &lt;query term&gt;, &lt;query primary&gt; |
| `QueryHint.Named` | T-SQL | A hint of one word, with or without a value — under Query Hints (Transact-SQL) |
| `QueryHint.Phrase` | T-SQL | A hint of several fixed words — under Query Hints (Transact-SQL) |
| `QueryHint.UseHint` | T-SQL | USE HINT ('a', N'b') — under Query Hints (Transact-SQL) |
| `QueryHint.UsePlan` | T-SQL | USE PLAN N'…' — under Query Hints (Transact-SQL) |
| `QueryHint.OptimizeFor` | T-SQL | OPTIMIZE FOR (@a = 1, @b UNKNOWN, @c) — under Query Hints (Transact-SQL) |
| `QueryHint.OptimizeForUnknown` | T-SQL | OPTIMIZE FOR UNKNOWN — under Query Hints (Transact-SQL) |
| `QueryHint.ForTable` | T-SQL | TABLE HINT (t, NOLOCK, INDEX (i)) — under Query Hints (Transact-SQL) |
| `QueueChange.With` | T-SQL | WITH option, … — under ALTER QUEUE (Transact-SQL) |
| `QueueChange.Rebuild` | T-SQL | REBUILD [WITH (MAXDOP = n)] — under ALTER QUEUE (Transact-SQL) |
| `QueueChange.Reorganize` | T-SQL | REORGANIZE [WITH (LOB_COMPACTION = ON / OFF)] — under ALTER QUEUE (Transact-SQL) |
| `QueueChange.MoveTo` | T-SQL | MOVE TO filegroup — under ALTER QUEUE (Transact-SQL) |
| `QueueOption.Status` | T-SQL | STATUS = ON or OFF — under CREATE QUEUE (Transact-SQL) |
| `QueueOption.Retention` | T-SQL | RETENTION = ON or OFF — under CREATE QUEUE (Transact-SQL) |
| `QueueOption.PoisonMessageHandling` | T-SQL | POISON_MESSAGE_HANDLING (STATUS = ON) or OFF — under CREATE QUEUE (Transact-SQL) |
| `QueueOption.DropActivation` | T-SQL | ACTIVATION (DROP) — under CREATE QUEUE (Transact-SQL) |
| `QueueOption.Activation` | T-SQL | ACTIVATION (setting, …) — under CREATE QUEUE (Transact-SQL) |
| `ResourceGovernorAction.Reconfigure` | T-SQL | RECONFIGURE — under ALTER RESOURCE GOVERNOR (Transact-SQL) |
| `ResourceGovernorAction.Disable` | T-SQL | DISABLE — under ALTER RESOURCE GOVERNOR (Transact-SQL) |
| `ResourceGovernorAction.ResetStatistics` | T-SQL | RESET STATISTICS — under ALTER RESOURCE GOVERNOR (Transact-SQL) |
| `ResourceGovernorAction.Settings` | T-SQL | WITH (setting) — under ALTER RESOURCE GOVERNOR (Transact-SQL) |
| `ResultSet.Columns` | T-SQL | EXECUTE (Transact-SQL) |
| `ResultSet.AsObject` | T-SQL | EXECUTE (Transact-SQL) |
| `ResultSet.AsType` | T-SQL | EXECUTE (Transact-SQL) |
| `ResultSet.AsForXml` | T-SQL | EXECUTE (Transact-SQL) |
| `RevokeBody.Privileges` | T-SQL | REVOKE (Transact-SQL) |
| `RevokeBody.Roles` | SQL:2023 | &lt;revoke statement&gt; |
| `RoleAction.AddMember` | T-SQL | ADD MEMBER principal — under ALTER ROLE (Transact-SQL) |
| `RoleAction.DropMember` | T-SQL | DROP MEMBER principal — under ALTER ROLE (Transact-SQL) |
| `RoleAction.WithName` | T-SQL | WITH NAME = new_name — under ALTER ROLE (Transact-SQL) |
| `RoutineBody.Sql` | SQL:2023 | &lt;SQL routine spec&gt;: the rights it runs with and one &lt;SQL procedure statement&gt; |
| `RoutineBody.External` | SQL:2023 | &lt;external body reference&gt; |
| `RoutineBody.PolymorphicTableFunction` | SQL:2023 | &lt;routine body&gt; |
| `RoutineBody.Statements` | T-SQL | CREATE PROCEDURE (Transact-SQL) |
| `RoutineBody.ExternalName` | T-SQL | CREATE PROCEDURE (Transact-SQL) |
| `RoutineBody.Inline` | T-SQL | CREATE FUNCTION (Transact-SQL) |
| `RoutineCharacteristic.Language` | SQL:2023 | &lt;language clause&gt; |
| `RoutineCharacteristic.ParameterStyle` | SQL:2023 | &lt;routine characteristic&gt; |
| `RoutineCharacteristic.Specific` | SQL:2023 | &lt;routine characteristic&gt; |
| `RoutineCharacteristic.Deterministic` | SQL:2023 | &lt;routine characteristic&gt; |
| `RoutineCharacteristic.DataAccess` | SQL:2023 | &lt;routine characteristic&gt; |
| `RoutineCharacteristic.NullCall` | SQL:2023 | &lt;routine characteristic&gt; |
| `RoutineCharacteristic.DynamicResultSets` | SQL:2023 | &lt;routine characteristic&gt; |
| `RoutineCharacteristic.SavepointLevel` | SQL:2023 | &lt;routine characteristic&gt; |
| `RoutineCharacteristic.ExternalName` | SQL:2023 | 'NAME &lt;external routine name&gt;' |
| `RoutineOption.Setting` | T-SQL | CREATE PROCEDURE (Transact-SQL) |
| `RoutineOption.ExecuteAs` | T-SQL | EXECUTE AS Clause (Transact-SQL) |
| `RoutineOption.NullCall` | T-SQL | CREATE FUNCTION (Transact-SQL) |
| `RowPattern.Variable` | SQL:2023 | under MATCH_RECOGNIZE and row-pattern grammar |
| `RowPattern.StartAnchor` | SQL:2023 | under MATCH_RECOGNIZE and row-pattern grammar |
| `RowPattern.EndAnchor` | SQL:2023 | under MATCH_RECOGNIZE and row-pattern grammar |
| `RowPattern.Sequence` | SQL:2023 | under MATCH_RECOGNIZE and row-pattern grammar |
| `RowPattern.Alternation` | SQL:2023 | under MATCH_RECOGNIZE and row-pattern grammar |
| `RowPattern.Quantified` | SQL:2023 | under MATCH_RECOGNIZE and row-pattern grammar |
| `RowPattern.Parenthesized` | SQL:2023 | under MATCH_RECOGNIZE and row-pattern grammar |
| `RowPattern.Excluded` | SQL:2023 | under MATCH_RECOGNIZE and row-pattern grammar |
| `RowPattern.Permute` | SQL:2023 | under MATCH_RECOGNIZE and row-pattern grammar |
| `ScopedConfigurationAction.Set` | T-SQL | ALTER DATABASE SCOPED CONFIGURATION |
| `ScopedConfigurationAction.ClearProcedureCache` | T-SQL | ALTER DATABASE SCOPED CONFIGURATION |
| `SearchPropertyListAction.Add` | T-SQL | ALTER SEARCH PROPERTY LIST (Transact-SQL) |
| `SearchPropertyListAction.Drop` | T-SQL | ALTER SEARCH PROPERTY LIST (Transact-SQL) |
| `SecurityPolicyChange.Predicates` | T-SQL | The predicates, ADD FILTER PREDICATE f(c) ON t, DROP BLOCK PREDICATE ON u — under ALTER SECURITY POLICY (Transact-SQL) |
| `SecurityPolicyChange.State` | T-SQL | WITH (STATE = ON) (true) or OFF (false) — under ALTER SECURITY POLICY (Transact-SQL) |
| `SecurityPolicyChange.AddNotForReplication` | T-SQL | ADD NOT FOR REPLICATION — under ALTER SECURITY POLICY (Transact-SQL) |
| `SecurityPolicyChange.DropNotForReplication` | T-SQL | DROP NOT FOR REPLICATION — under ALTER SECURITY POLICY (Transact-SQL) |
| `SelectColumn.All` | SQL:2023 | under &lt;select list&gt;, &lt;select sublist&gt;, &lt;derived column&gt;, &lt;qualified asterisk&gt;, &lt;all fields reference&gt; |
| `SelectColumn.ExpressionItem` | SQL:2023 | under &lt;select list&gt;, &lt;select sublist&gt;, &lt;derived column&gt;, &lt;qualified asterisk&gt;, &lt;all fields reference&gt; |
| `SelectColumn.QualifiedAll` | SQL:2023 | under &lt;select list&gt;, &lt;select sublist&gt;, &lt;derived column&gt;, &lt;qualified asterisk&gt;, &lt;all fields reference&gt; |
| `SelectColumn.VariableAssignment` | T-SQL | SELECT @local_variable (Transact-SQL) |
| `SequenceOption.DataTypeOption` | SQL:2023 | &lt;sequence generator option&gt; |
| `SequenceOption.Start` | SQL:2023 | &lt;common sequence generator option&gt; |
| `SequenceOption.Increment` | SQL:2023 | &lt;basic sequence generator option&gt; |
| `SequenceOption.Max` | SQL:2023 | &lt;basic sequence generator option&gt; |
| `SequenceOption.Min` | SQL:2023 | &lt;basic sequence generator option&gt; |
| `SequenceOption.Cycle` | SQL:2023 | &lt;basic sequence generator option&gt; |
| `SequenceOption.Restart` | SQL:2023 | &lt;alter identity column option&gt; |
| `SequenceOption.Cache` | T-SQL | CREATE SEQUENCE (Transact-SQL) |
| `SequenceOption.NoCache` | T-SQL | CREATE SEQUENCE (Transact-SQL) |
| `ServiceMasterKeyChange.Regenerate` | T-SQL | [FORCE] REGENERATE — under ALTER SERVICE MASTER KEY (Transact-SQL) |
| `ServiceMasterKeyChange.OldAccount` | T-SQL | WITH OLD_ACCOUNT = 'a', OLD_PASSWORD = 'p' — under ALTER SERVICE MASTER KEY (Transact-SQL) |
| `ServiceMasterKeyChange.NewAccount` | T-SQL | WITH NEW_ACCOUNT = 'a', NEW_PASSWORD = 'p' — under ALTER SERVICE MASTER KEY (Transact-SQL) |
| `SetOption.Switch` | T-SQL | SET Statements (Transact-SQL) |
| `SetOption.Value` | T-SQL | SET Statements (Transact-SQL) |
| `SetOption.Statistics` | T-SQL | SET STATISTICS IO, PROFILE, TIME, XML (Transact-SQL) |
| `SetOption.IdentityInsert` | T-SQL | SET IDENTITY_INSERT (Transact-SQL) |
| `SetOption.Offsets` | T-SQL | SET OFFSETS (Transact-SQL) |
| `SignatureProof.Password` | T-SQL | WITH PASSWORD = 'p' — under ADD SIGNATURE (Transact-SQL) |
| `SignatureProof.Signature` | T-SQL | WITH SIGNATURE = 0x… — under ADD SIGNATURE (Transact-SQL) |
| `SignedBy.Certificate` | T-SQL | CERTIFICATE cert_name [WITH PASSWORD = 'p' / WITH SIGNATURE = 0x…] — under DROP SIGNATURE (Transact-SQL) |
| `SignedBy.AsymmetricKey` | T-SQL | ASYMMETRIC KEY key_name [WITH PASSWORD = 'p' / WITH SIGNATURE = 0x…] — under DROP SIGNATURE (Transact-SQL) |
| `SignedBy.SymmetricKey` | T-SQL | SYMMETRIC KEY key_name — under DROP SIGNATURE (Transact-SQL) |
| `SignedBy.Password` | T-SQL | PASSWORD = 'p' — under DROP SIGNATURE (Transact-SQL) |
| `SoapItem.Setting` | T-SQL | A setting — under CREATE ENDPOINT (Transact-SQL) |
| `SoapItem.WebMethod` | T-SQL | [ADD / ALTER / DROP] WEBMETHOD ['namespace'.]'method' [(NAME = 'object', SCHEMA = …, FORMAT = …)] — under CREATE ENDPOINT (Transact-SQL) |
| `SqlCondition.Major` | SQL:2023 | under &lt;sql condition&gt; |
| `SqlCondition.State` | SQL:2023 | under &lt;sql condition&gt; |
| `SqlCondition.Constraint` | SQL:2023 | under &lt;sql condition&gt; |
| `StopListAction.Add` | T-SQL | ALTER FULLTEXT STOPLIST (Transact-SQL) |
| `StopListAction.Drop` | T-SQL | ALTER FULLTEXT STOPLIST (Transact-SQL) |
| `StopListAction.DropAll` | T-SQL | ALTER FULLTEXT STOPLIST (Transact-SQL) |
| `StopListSource.SystemStopList` | T-SQL | CREATE FULLTEXT STOPLIST (Transact-SQL) |
| `StopListSource.Named` | T-SQL | CREATE FULLTEXT STOPLIST (Transact-SQL) |
| `SymmetricKeyChange.AddEncryption` | T-SQL | ADD ENCRYPTION BY encryptor, … — under ALTER SYMMETRIC KEY (Transact-SQL) |
| `SymmetricKeyChange.DropEncryption` | T-SQL | DROP ENCRYPTION BY encryptor, … — under ALTER SYMMETRIC KEY (Transact-SQL) |
| `SystemTimeSpecification.AsOf` | SQL:2023 | &lt;query system time period specification&gt; |
| `SystemTimeSpecification.Between` | SQL:2023 | &lt;query system time period specification&gt; |
| `SystemTimeSpecification.FromTo` | SQL:2023 | &lt;query system time period specification&gt; |
| `SystemTimeSpecification.ContainedIn` | T-SQL | FROM clause plus JOIN, APPLY, PIVOT (T-SQL) |
| `SystemTimeSpecification.All` | T-SQL | FROM clause plus JOIN, APPLY, PIVOT (T-SQL) |
| `TableContents.Elements` | SQL:2023 | under table element/column/period/constraint/LIKE/typed-table/as-subquery structures |
| `TableContents.Typed` | SQL:2023 | under table element/column/period/constraint/LIKE/typed-table/as-subquery structures |
| `TableContents.AsQuery` | T-SQL | CREATE TABLE AS SELECT (Microsoft Fabric and Azure Synapse Analytics) writes no 'WITH [NO] DATA': Data is null there |
| `TableElement.Column` | SQL:2023 | &lt;table element&gt; |
| `TableElement.Period` | SQL:2023 | &lt;table element&gt; |
| `TableElement.TableConstraint` | SQL:2023 | &lt;typed table element&gt; |
| `TableElement.Like` | SQL:2023 | &lt;like clause&gt; |
| `TableElement.SelfReference` | SQL:2023 | &lt;self referencing column specification&gt; |
| `TableElement.ColumnOptions` | SQL:2023 | &lt;column options&gt; |
| `TableElement.Index` | T-SQL | CREATE TABLE (Transact-SQL) |
| `TableHint.Named` | T-SQL | A hint the grammar reads by name, with its values — under Table Hints (Transact-SQL) |
| `TableHint.ForceSeek` | T-SQL | FORCESEEK (ix (a, b)) — under Table Hints (Transact-SQL) |
| `TableSource.Named` | SQL:2023 | under &lt;table reference&gt;, &lt;table factor&gt;, &lt;table primary&gt;, &lt;joined table&gt;, derived/collection/function/JSON tables |
| `TableSource.Subquery` | SQL:2023 | under &lt;table reference&gt;, &lt;table factor&gt;, &lt;table primary&gt;, &lt;joined table&gt;, derived/collection/function/JSON tables |
| `TableSource.Join` | SQL:2023 | under &lt;table reference&gt;, &lt;table factor&gt;, &lt;table primary&gt;, &lt;joined table&gt;, derived/collection/function/JSON tables |
| `TableSource.Unnest` | SQL:2023 | under &lt;table reference&gt;, &lt;table factor&gt;, &lt;table primary&gt;, &lt;joined table&gt;, derived/collection/function/JSON tables |
| `TableSource.Function` | SQL:2023 | under &lt;table reference&gt;, &lt;table factor&gt;, &lt;table primary&gt;, &lt;joined table&gt;, derived/collection/function/JSON tables |
| `TableSource.TableFunction` | SQL:2023 | &lt;table function derived table&gt;, 'TABLE (f (a))' |
| `TableSource.RowPatternRecognition` | SQL:2023 | &lt;row pattern recognition clause&gt; after a table primary, 't MATCH_RECOGNIZE (…) AS m' |
| `TableSource.JsonTable` | SQL:2023 | under &lt;table reference&gt;, &lt;table factor&gt;, &lt;table primary&gt;, &lt;joined table&gt;, derived/collection/function/JSON tables |
| `TableSource.DataChange` | T-SQL | INSERT (Transact-SQL) |
| `TableSource.Parenthesized` | SQL:2023 | under &lt;table reference&gt;, &lt;table factor&gt;, &lt;table primary&gt;, &lt;joined table&gt;, derived/collection/function/JSON tables |
| `TableSource.Extension` | T-SQL | none: a construct a dialect reads and the tree has no place for yet, said by Dialect and Kind |
| `TableSource.Pivot` | T-SQL | FROM clause plus JOIN, APPLY, PIVOT (T-SQL) |
| `TableSource.Unpivot` | T-SQL | FROM clause plus JOIN, APPLY, PIVOT (T-SQL) |
| `TableSource.OdbcJoin` | T-SQL | the ODBC outer join escape { oj … } (no syntax block in syntax.md) |
| `TableSource.OpenJson` | T-SQL | OPENJSON (Transact-SQL) |
| `TableSource.OpenXml` | T-SQL | OPENXML (Transact-SQL) |
| `TableSource.OpenQuery` | T-SQL | OPENQUERY (Transact-SQL) |
| `TableSource.OpenRowset` | T-SQL | OPENROWSET (Transact-SQL) |
| `TableSource.OpenRowsetBulk` | T-SQL | OPENROWSET BULK (Transact-SQL) |
| `TableSource.AiGenerateChunks` | T-SQL | PREDICT (Transact-SQL) |
| `TableSource.Predict` | SQL:2023 | under &lt;table reference&gt;, &lt;table factor&gt;, &lt;table primary&gt;, &lt;joined table&gt;, derived/collection/function/JSON tables |
| `TableSource.ChangeTableChanges` | T-SQL | CHANGETABLE (Transact-SQL) |
| `TableSource.ChangeTableVersion` | T-SQL | CHANGETABLE (Transact-SQL) |
| `TableSource.ContainsTable` | T-SQL | CONTAINSTABLE (Transact-SQL) |
| `TableSource.FreetextTable` | T-SQL | FREETEXTTABLE (Transact-SQL) |
| `TableSource.Variable` | T-SQL | FROM clause plus JOIN, APPLY, PIVOT (T-SQL) |
| `Termination.RollbackAfter` | T-SQL | ALTER DATABASE SET Options (Transact-SQL) |
| `Termination.RollbackImmediate` | T-SQL | ALTER DATABASE SET Options (Transact-SQL) |
| `Termination.NoWait` | T-SQL | ALTER DATABASE SET Options (Transact-SQL) |
| `TransactionMode.Isolation` | SQL:2023 | &lt;transaction mode&gt; |
| `TransactionMode.Access` | SQL:2023 | &lt;transaction mode&gt; |
| `TransactionMode.DiagnosticsSize` | SQL:2023 | &lt;transaction mode&gt; |
| `TransformDropTarget.All` | SQL:2023 | &lt;transform drop target&gt; |
| `TransformDropTarget.Group` | SQL:2023 | &lt;transform drop target&gt; |
| `TypeRepresentation.Type` | SQL:2023 | &lt;representation&gt; |
| `TypeRepresentation.Members` | SQL:2023 | &lt;representation&gt; |
| `TypeRepresentation.Table` | SQL:2023 | under &lt;type representation&gt; |
| `UserSource.Login` | T-SQL | { FOR / FROM } LOGIN login_name — under CREATE USER (Transact-SQL) |
| `UserSource.Certificate` | T-SQL | { FOR / FROM } CERTIFICATE cert_name — under CREATE USER (Transact-SQL) |
| `UserSource.AsymmetricKey` | T-SQL | { FOR / FROM } ASYMMETRIC KEY key_name — under CREATE USER (Transact-SQL) |
| `UserSource.ExternalProvider` | T-SQL | { FOR / FROM } EXTERNAL PROVIDER — under CREATE USER (Transact-SQL) |
| `UserSource.WithoutLogin` | T-SQL | WITHOUT LOGIN — under CREATE USER (Transact-SQL) |
| `UserTypeOption.Instantiable` | SQL:2023 | &lt;user defined type option&gt; |
| `UserTypeOption.Final` | SQL:2023 | &lt;user defined type option&gt; |
| `UserTypeOption.Reference` | SQL:2023 | &lt;user defined type option&gt; |
| `UserTypeOption.Cast` | SQL:2023 | &lt;user defined type option&gt; |
| `UserTypeReference.Using` | SQL:2023 | &lt;user defined type option&gt; |
| `UserTypeReference.FromAttributes` | SQL:2023 | &lt;user defined type option&gt; |
| `UserTypeReference.SystemGenerated` | SQL:2023 | &lt;user defined type option&gt; |
| `VariableDeclaration.Typed` | T-SQL | DECLARE @local_variable (Transact-SQL) |
| `VariableDeclaration.Table` | T-SQL | DECLARE @local_variable (Transact-SQL) |
| `ViewElement.SelfReference` | SQL:2023 | &lt;view element&gt; |
| `ViewElement.ColumnOption` | SQL:2023 | &lt;view element&gt; |
| `ViewSpecification.Regular` | SQL:2023 | &lt;view specification&gt; |
| `ViewSpecification.Referenceable` | SQL:2023 | &lt;view specification&gt; |
| `WindowFrameExtent.Single` | SQL:2023 | &lt;window frame extent&gt; |
| `WindowFrameExtent.Between` | SQL:2023 | &lt;window frame extent&gt; |
| `WindowReference.NameRef` | SQL:2023 | &lt;window name or specification&gt; |
| `WindowReference.Specification` | SQL:2023 | &lt;window name or specification&gt; |
