# Transact-SQL syntax, as Microsoft publishes it

Every syntax block of the Transact-SQL reference in
[MicrosoftDocs/sql-docs](https://github.com/MicrosoftDocs/sql-docs), `docs/t-sql`, at
commit `8c0865af1c66c1deab003081210d03aed5d2719f` (2026-09-09), gathered by `--syntax`
(`benchmarks/DotGram.Benchmarks/SyntaxBlocks.cs`): the `syntaxsql` blocks, and a plain or
`sql` one under a heading that says Syntax.

© Microsoft Corporation. The documentation is licensed under
[Creative Commons Attribution 4.0 International](https://creativecommons.org/licenses/by/4.0/).
Changed from the original: only the syntax blocks are kept, each with its page's title and
path, the heading above it, the sentence before it, and the product range the page marks it
for; the documentation's includes are expanded in place, and trailing whitespace is
removed. Nothing inside a block is otherwise altered.

Microsoft publishes no grammar for T-SQL, and this is the nearest thing to one. It is what
`TransactSql.gram` is written from — and where a block and SQL Server disagree, the engine
is what the grammar follows.

## GetAncestor (Database Engine)

`docs/t-sql/data-types/getancestor-database-engine.md`

### Syntax

```syntaxsql
-- Transact-SQL syntax
child.GetAncestor ( n )
```

```syntaxsql
-- CLR syntax
SqlHierarchyId GetAncestor ( int n )
```

## GetDescendant (Database Engine)

`docs/t-sql/data-types/getdescendant-database-engine.md`

### Syntax

```syntaxsql
-- Transact-SQL syntax
parent.GetDescendant ( child1 , child2 )
```

```syntaxsql
-- CLR syntax
SqlHierarchyId GetDescendant ( SqlHierarchyId child1 , SqlHierarchyId child2 )
```

## GetLevel (Database Engine)

`docs/t-sql/data-types/getlevel-database-engine.md`

### Syntax

```syntaxsql
-- Transact-SQL syntax
node.GetLevel ( )
```

```syntaxsql
-- CLR syntax
SqlInt16 GetLevel ( )
```

## GetReparentedValue (Database Engine)

`docs/t-sql/data-types/getreparentedvalue-database-engine.md`

### Syntax

```syntaxsql
-- Transact-SQL syntax
node. GetReparentedValue ( oldRoot, newRoot )
```

```syntaxsql
-- CLR syntax
SqlHierarchyId GetReparentedValue ( SqlHierarchyId oldRoot , SqlHierarchyId newRoot )
```

## GetRoot (Database Engine)

`docs/t-sql/data-types/getroot-database-engine.md`

### Syntax

```syntaxsql
-- Transact-SQL syntax
hierarchyid::GetRoot ( )
```

```syntaxsql
-- CLR syntax
static SqlHierarchyId GetRoot ( )
```

## IsDescendantOf (Database Engine)

`docs/t-sql/data-types/isdescendantof-database-engine.md`

### Syntax

```syntaxsql
-- Transact-SQL syntax
child. IsDescendantOf ( parent )
```

```syntaxsql
-- CLR syntax
SqlHierarchyId IsDescendantOf (SqlHierarchyId parent )
```

## JSON Data Type

`docs/t-sql/data-types/json-data-type.md`

### Sample syntax

> The usage syntax for the **json** type is similar to all other SQL Server data types in a table.

```syntaxsql
column_name JSON [ NOT NULL | NULL ] [CHECK ( constraint_expression ) ] [ DEFAULT ( default_expression ) ]
```

## Parse (Database Engine)

`docs/t-sql/data-types/parse-database-engine.md`

### Syntax

```syntaxsql
-- Transact-SQL syntax
hierarchyid::Parse ( input )
-- This is functionally equivalent to the following syntax
-- which implicitly calls Parse():
CAST ( input AS hierarchyid )
```

## sql_variant (Transact-SQL)

`docs/t-sql/data-types/sql-variant-transact-sql.md`

### Syntax

```syntaxsql
sql_variant
```

## table (Transact-SQL)

`docs/t-sql/data-types/table-transact-sql.md`

### Syntax

```syntaxsql
table_type_definition ::=
    TABLE ( { <column_definition> | <table_constraint> } [ , ...n ] )

<column_definition> ::=
    column_name scalar_data_type
    [ COLLATE <collation_definition> ]
    [ [ DEFAULT constant_expression ] | IDENTITY [ ( seed , increment ) ] ]
    [ ROWGUIDCOL ]
    [ column_constraint ] [ ...n ]

 <column_constraint> ::=
    { [ NULL | NOT NULL ]
    | [ PRIMARY KEY | UNIQUE ]
    | CHECK ( logical_expression )
    }

<table_constraint> ::=
     { { PRIMARY KEY | UNIQUE } ( column_name [ , ...n ] )
     | CHECK ( logical_expression )
     }
```

## ToString (Database Engine)

`docs/t-sql/data-types/tostring-database-engine.md`

### Syntax

```syntaxsql
-- Transact-SQL syntax
node.ToString  ( )
-- This is functionally equivalent to the following syntax
-- which implicitly calls ToString():
CAST(node AS nvarchar(4000))
```

```syntaxsql
-- CLR syntax
string ToString  ( )
```

## Vector Data Type - Half Precision Float

`docs/t-sql/data-types/vector-data-type-half-precision-float.md`

### Sample syntax

> The usage syntax is similar to **vector** data type. However, to use `float16`, specify the base type explicitly.

```syntaxsql
column_name VECTOR ( <dimensions> [, <base_type>] ) [ NOT NULL | NULL ]
```

## Vector Data Type

`docs/t-sql/data-types/vector-data-type.md`

### Sample syntax

> The usage syntax for the **vector** type is similar to all other SQL Server data types in a table.

```syntaxsql
column_name VECTOR ( { <dimensions> } ) [ NOT NULL | NULL ]
```

> By default, the base type is `float32`. To use **half-precision**, you need to specify `float16` explicitly.

```syntaxsql
column_name VECTOR ( <dimensions> [ , <base_type> ] ) [ NOT NULL | NULL ]
```

## DBCC CHECKALLOC (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-checkalloc-transact-sql.md`

### Syntax

```syntaxsql
DBCC CHECKALLOC
[
    ( database_name | database_id | 0
      [ , NOINDEX
      | , { REPAIR_ALLOW_DATA_LOSS | REPAIR_FAST | REPAIR_REBUILD } ]
    )
    [ WITH
        {
          [ ALL_ERRORMSGS ]
          [ , NO_INFOMSGS ]
          [ , TABLOCK ]
          [ , ESTIMATEONLY ]
        }
    ]
]
```

## DBCC CHECKCATALOG (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-checkcatalog-transact-sql.md`

### Syntax

```syntaxsql
DBCC CHECKCATALOG
[
    (
    database_name | database_id | 0
    )
]
    [ WITH NO_INFOMSGS ]
```

## DBCC CHECKCONSTRAINTS (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-checkconstraints-transact-sql.md`

### Syntax

```syntaxsql
DBCC CHECKCONSTRAINTS
[
    (
    table_name | table_id | constraint_name | constraint_id
    )
]
    [ WITH
    [ { ALL_CONSTRAINTS | ALL_ERRORMSGS } ]
    [ , ] [ NO_INFOMSGS ]
    ]
```

## DBCC CHECKDB (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-checkdb-transact-sql.md`

### Syntax

```syntaxsql
DBCC CHECKDB
    [ [ ( database_name | database_id | 0
        [ , NOINDEX
        | , { REPAIR_ALLOW_DATA_LOSS | REPAIR_FAST | REPAIR_REBUILD } ]
    ) ]
    [ WITH
        {
            [ ALL_ERRORMSGS ]
            [ , EXTENDED_LOGICAL_CHECKS ]
            [ , NO_INFOMSGS ]
            [ , TABLOCK ]
            [ , ESTIMATEONLY ]
            [ , { PHYSICAL_ONLY | DATA_PURITY } ]
            [ , MAXDOP = number_of_processors ]
        }
    ]
]
```

## DBCC CHECKFILEGROUP (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-checkfilegroup-transact-sql.md`

### Syntax

```syntaxsql
DBCC CHECKFILEGROUP
[
    [ ( { filegroup_name | filegroup_id | 0 }
        [ , NOINDEX ]
  ) ]
    [ WITH
        {
            [ ALL_ERRORMSGS | NO_INFOMSGS ]
            [ , TABLOCK ]
            [ , ESTIMATEONLY ]
            [ , PHYSICAL_ONLY ]
            [ , MAXDOP  = number_of_processors ]
        }
    ]
]
```

## DBCC CHECKIDENT (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-checkident-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, Azure SQL Managed Instance, SQL database in Fabric:

```syntaxsql
DBCC CHECKIDENT
 (
    table_name
        [ , { NORESEED | { RESEED [ , new_reseed_value ] } } ]
)
[ WITH NO_INFOMSGS ]
```

> Syntax for Azure Synapse Analytics:

```syntaxsql
DBCC CHECKIDENT
 (
    table_name
        [ RESEED , new_reseed_value ]
)
[ WITH NO_INFOMSGS ]
```

```syntaxsql
DBCC CHECKIDENT ( 'table_name' [ , RESEED ] )
```

## DBCC CHECKTABLE (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-checktable-transact-sql.md`

### Syntax

```syntaxsql
DBCC CHECKTABLE
(
    table_name | view_name
    [ , { NOINDEX | index_id }
     | , { REPAIR_ALLOW_DATA_LOSS | REPAIR_FAST | REPAIR_REBUILD }
    ]
)
    [ WITH
        { [ ALL_ERRORMSGS ]
          [ , EXTENDED_LOGICAL_CHECKS ]
          [ , NO_INFOMSGS ]
          [ , TABLOCK ]
          [ , ESTIMATEONLY ]
          [ , { PHYSICAL_ONLY | DATA_PURITY } ]
          [ , MAXDOP = number_of_processors ]
        }
    ]
```

## DBCC CLEANTABLE (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-cleantable-transact-sql.md`

### Syntax

```syntaxsql
DBCC CLEANTABLE
(
    { database_name | database_id | 0 }
    , { table_name | table_id | view_name | view_id }
    [ , batch_size ]
)
[ WITH NO_INFOMSGS ]
```

## DBCC CLONEDATABASE (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-clonedatabase-transact-sql.md`

### Syntax

```syntaxsql
DBCC CLONEDATABASE
(
    source_database_name
    ,  target_database_name
)
    [ WITH { [ NO_STATISTICS ] [ , NO_QUERYSTORE ] [ , VERIFY_CLONEDB | SERVICEBROKER ] [ , BACKUP_CLONEDB ] } ]
```

## DBCC DBREINDEX (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-dbreindex-transact-sql.md`

### Syntax

```syntaxsql
DBCC DBREINDEX
(
    table_name
    [ , index_name [ , fillfactor ] ]
)
    [ WITH NO_INFOMSGS ]
```

## DBCC dllname (FREE) (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-dllname-free-transact-sql.md`

### Syntax

```syntaxsql
DBCC <dllname> ( FREE ) [ WITH NO_INFOMSGS ]
```

## DBCC DROPCLEANBUFFERS (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-dropcleanbuffers-transact-sql.md`

### Syntax

> Syntax for SQL Server,  Azure SQL Database, and serverless SQL pool in Azure Synapse Analytics:

```syntaxsql
DBCC DROPCLEANBUFFERS [ WITH NO_INFOMSGS ]
```

> Syntax for Azure Synapse Analytics and Analytics Platform System (PDW):

```syntaxsql
DBCC DROPCLEANBUFFERS ( COMPUTE | ALL ) [ WITH NO_INFOMSGS ]
```

## DBCC DROPRESULTSETCACHE (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-dropresultsetcache-transact-sql.md`

### Syntax

```syntaxsql
DBCC DROPRESULTSETCACHE
[;]
```

## DBCC FLUSHAUTHCACHE (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-flushauthcache-transact-sql.md`

### Syntax

```syntaxsql
DBCC FLUSHAUTHCACHE
[;]
```

## DBCC FREEPROCCACHE (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-freeproccache-transact-sql.md`

### Syntax

> Syntax for SQL Server and Azure SQL Database:

```sql
DBCC FREEPROCCACHE [ ( { plan_handle | sql_handle | pool_name } ) ] [ WITH NO_INFOMSGS ]
```

> Syntax for Azure Synapse Analytics and Analytics Platform System (PDW):

```sql
DBCC FREEPROCCACHE [ ( COMPUTE | ALL ) ]
     [ WITH NO_INFOMSGS ]
[;]
```

## DBCC FREESESSIONCACHE (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-freesessioncache-transact-sql.md`

### Syntax

```syntaxsql
DBCC FREESESSIONCACHE [ WITH NO_INFOMSGS ]
```

## DBCC FREESYSTEMCACHE (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-freesystemcache-transact-sql.md`

### Syntax

```syntaxsql
DBCC FREESYSTEMCACHE
    ( 'ALL' [ , pool_name ] )
    [ WITH
    { [ MARK_IN_USE_FOR_REMOVAL ] , [ NO_INFOMSGS ]  }
    ]
```

## DBCC HELP (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-help-transact-sql.md`

### Syntax

```syntaxsql
DBCC HELP ( 'dbcc_statement' | @dbcc_statement_var | '?' )
[ WITH NO_INFOMSGS ]
```

## DBCC INDEXDEFRAG (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-indexdefrag-transact-sql.md`

### Syntax

```syntaxsql
DBCC INDEXDEFRAG
(
    { database_name | database_id | 0 }
    , { table_name | table_id | view_name | view_id }
    [ , { index_name | index_id } [ , { partition_number | 0 } ] ]
)
    [ WITH NO_INFOMSGS ]
```

## DBCC INPUTBUFFER (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-inputbuffer-transact-sql.md`

### Syntax

```syntaxsql
DBCC INPUTBUFFER ( session_id [ , request_id ] )
[ WITH NO_INFOMSGS ]
```

## DBCC OPENTRAN (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-opentran-transact-sql.md`

### Syntax

```syntaxsql
DBCC OPENTRAN
[
    ( [ database_name | database_id | 0 ] )
    { [ WITH TABLERESULTS ]
      [ , [ NO_INFOMSGS ] ]
    }
]
```

## DBCC OUTPUTBUFFER (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-outputbuffer-transact-sql.md`

### Syntax

```syntaxsql
DBCC OUTPUTBUFFER ( session_id [ , request_id ] )
[ WITH NO_INFOMSGS ]
```

## DBCC PDW_SHOWEXECUTIONPLAN (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-pdw-showexecutionplan-transact-sql.md`

### Syntax

> Syntax for Azure Synapse Analytics:

```syntaxsql
DBCC PDW_SHOWEXECUTIONPLAN ( distribution_id , spid )
[;]
```

> Syntax for Analytics Platform System (PDW):

```syntaxsql
DBCC PDW_SHOWEXECUTIONPLAN ( pdw_node_id , spid )
[;]
```

## DBCC PDW_SHOWMATERIALIZEDVIEWOVERHEAD  (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-pdw-showmaterializedviewoverhead-transact-sql.md`

### Syntax

```syntaxsql
DBCC PDW_SHOWMATERIALIZEDVIEWOVERHEAD ( "[ schema_name . ] materialized_view_name" )
[;]
```

## DBCC PDW_SHOWPARTITIONSTATS (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-pdw-showpartitionstats-transact-sql.md`

### Syntax

```syntaxsql
--Show the partition stats for a table
DBCC PDW_SHOWPARTITIONSTATS ( "[ database_name . [ schema_name ] . ] | [ schema_name. ] table_name" )
[;]
```

## DBCC PDW_SHOWSPACEUSED (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-pdw-showspaceused-transact-sql.md`

### Syntax

```syntaxsql
-- Show the space used for all user tables and system tables in the current database
DBCC PDW_SHOWSPACEUSED [ WITH IGNORE_REPLICATED_TABLE_CACHE ]
[;]

-- Show the space used for a table
DBCC PDW_SHOWSPACEUSED ( "[ database_name . [ schema_name ] . ] | [ schema_name . ] table_name" ) [ WITH IGNORE_REPLICATED_TABLE_CACHE ]
[;]
```

## DBCC PROCCACHE (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-proccache-transact-sql.md`

### Syntax

```sql
DBCC PROCCACHE [ WITH NO_INFOMSGS ]
```

## DBCC SHOW_STATISTICS (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-show-statistics-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, and SQL database in Fabric:

```syntaxsql
DBCC SHOW_STATISTICS ( table_or_indexed_view_name , target )
[ WITH [ NO_INFOMSGS ] < option > [ , ...n ] ]
< option > ::=
    STAT_HEADER | DENSITY_VECTOR | HISTOGRAM | STATS_STREAM
[ ; ]
```

> Syntax for Azure Synapse Analytics, Analytics Platform System (PDW), and Warehouse in Microsoft Fabric:

```syntaxsql
DBCC SHOW_STATISTICS ( table_name , target )
    [ WITH { STAT_HEADER | DENSITY_VECTOR | HISTOGRAM } [ , ...n ] ]
[ ; ]
```

## DBCC SHOWCONTIG (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-showcontig-transact-sql.md`

### Syntax

```syntaxsql
DBCC SHOWCONTIG
[ (
    { table_name | table_id | view_name | view_id }
    [ , index_name | index_id ]
) ]
    [ WITH
        {
         [ , [ ALL_INDEXES ] ]
         [ , [ TABLERESULTS ] ]
         [ , [ FAST ] ]
         [ , [ ALL_LEVELS ] ]
         [ NO_INFOMSGS ]
         }
    ]
```

## DBCC SHOWRESULTCACHESPACEUSED (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-showresultcachespaceused-transact-sql.md`

### Syntax

```syntaxsql
DBCC SHOWRESULTCACHESPACEUSED
[;]
```

## DBCC SHRINKDATABASE (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-shrinkdatabase-transact-sql.md`

### Syntax

> Syntax for SQL Server:

```syntaxsql
DBCC SHRINKDATABASE
( database_name | database_id | 0
     [ , target_percent ]
     [ , { NOTRUNCATE | TRUNCATEONLY } ]
)
[ WITH
    {
         [ WAIT_AT_LOW_PRIORITY
            [ (
                  <wait_at_low_priority_option_list>
             ) ]
         ]
         [ , NO_INFOMSGS ]
    }
]

<wait_at_low_priority_option_list> ::=
    <wait_at_low_priority_option>
    | <wait_at_low_priority_option_list>
      , <wait_at_low_priority_option>

<wait_at_low_priority_option> ::=
  ABORT_AFTER_WAIT = { SELF | BLOCKERS }
```

> Syntax for Azure Synapse Analytics:

```syntaxsql
DBCC SHRINKDATABASE
( database_name
     [ , target_percent ]
)
[ WITH NO_INFOMSGS ]
```

## DBCC SHRINKFILE (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-shrinkfile-transact-sql.md`

### Syntax

```syntaxsql
DBCC SHRINKFILE
(
    { file_name | file_id }
    { [ , EMPTYFILE ]
    | [ [ , target_size ] [ , { NOTRUNCATE | TRUNCATEONLY } ] ]
    }
)
[ WITH
  {
      [ WAIT_AT_LOW_PRIORITY
        [ (
            <wait_at_low_priority_option_list>
        ) ]
      ]
      [ , NO_INFOMSGS ]
  }
]

<wait_at_low_priority_option_list> ::=
    <wait_at_low_priority_option>
    | <wait_at_low_priority_option_list> , <wait_at_low_priority_option>

<wait_at_low_priority_option> ::=
    ABORT_AFTER_WAIT = { SELF | BLOCKERS }
```

## DBCC SHRINKLOG - Analytics Platform System (PDW)

`docs/t-sql/database-console-commands/dbcc-shrinklog-azure-sql-data-warehouse.md`

### Syntax

```syntaxsql
DBCC SHRINKLOG
    [ ( SIZE = { target_size [ MB | GB | TB ]  } | DEFAULT ) ]
    [ WITH NO_INFOMSGS ]
[;]
```

## DBCC SQLPERF (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-sqlperf-transact-sql.md`

### Syntax

```syntaxsql
DBCC SQLPERF
(
     [ LOGSPACE ]
     | [ "sys.dm_os_latch_stats" , CLEAR ]
     | [ "sys.dm_os_wait_stats" , CLEAR ]
)
     [ WITH NO_INFOMSGS ]
```

## DBCC TRACEOFF (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-traceoff-transact-sql.md`

### Syntax

```syntaxsql
DBCC TRACEOFF ( trace# [ , ...n ] [ , -1 ] ) [ WITH NO_INFOMSGS ]
```

## DBCC TRACEON (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-traceon-transact-sql.md`

### Syntax

```syntaxsql
DBCC TRACEON ( trace# [ , ...n ] [ , -1 ] ) [ WITH NO_INFOMSGS ]
```

## DBCC TRACESTATUS (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-tracestatus-transact-sql.md`

### Syntax

```syntaxsql
DBCC TRACESTATUS ( [ [ trace# [ , ...n ] ] [ , ] [ -1 ] ] )
[ WITH NO_INFOMSGS ]
```

## DBCC UPDATEUSAGE (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-updateusage-transact-sql.md`

### Syntax

```syntaxsql
DBCC UPDATEUSAGE
(   { database_name | database_id | 0 }
    [ , { table_name | table_id | view_name | view_id }
    [ , { index_name | index_id } ] ]
) [ WITH [ NO_INFOMSGS ] [ , ] [ COUNT_ROWS ] ]
```

## DBCC USEROPTIONS (Transact-SQL)

`docs/t-sql/database-console-commands/dbcc-useroptions-transact-sql.md`

### Syntax

```syntaxsql
DBCC USEROPTIONS
[ WITH NO_INFOMSGS ]
```

## ABS (Transact-SQL)

`docs/t-sql/functions/abs-transact-sql.md`

### Syntax

```syntaxsql
ABS ( numeric_expression )
```

## ACOS (Transact-SQL)

`docs/t-sql/functions/acos-transact-sql.md`

### Syntax

```syntaxsql
ACOS ( float_expression )
```

## AI_ANALYZE_SENTIMENT (Transact-SQL)

`docs/t-sql/functions/ai-analyze-sentiment-transact-sql.md`

### Syntax

```syntaxsql
AI_ANALYZE_SENTIMENT ( text [ (NULL | ERROR | DEFAULT <value>) ON ERROR ] )
```

## AI_CLASSIFY (Transact-SQL)

`docs/t-sql/functions/ai-classify-transact-sql.md`

### Syntax

```syntaxsql
AI_CLASSIFY ( text, class1, class2 [ , ...n ] [ (NULL | ERROR | DEFAULT <value>) ON ERROR ] )
```

## AI_EXTRACT (Transact-SQL)

`docs/t-sql/functions/ai-extract-transact-sql.md`

### Syntax

```syntaxsql
AI_EXTRACT ( text, class1, class2 [ , ...n ] [ (NULL | ERROR | DEFAULT <value>) ON ERROR ] )
```

## AI_FIX_GRAMMAR (Transact-SQL)

`docs/t-sql/functions/ai-fix-grammar-transact-sql.md`

### Syntax

```syntaxsql
AI_FIX_GRAMMAR ( text [ (NULL | ERROR | DEFAULT <value>) ON ERROR ] )
```

## AI_GENERATE_CHUNKS (Transact-SQL)

`docs/t-sql/functions/ai-generate-chunks-transact-sql.md`

### Syntax

```syntaxsql
AI_GENERATE_CHUNKS (SOURCE = text_expression
                   , CHUNK_TYPE = FIXED
                   [ , CHUNK_SIZE = numeric_expression ]
                   [ , OVERLAP = numeric_expression ]
                   [ , ENABLE_CHUNK_SET_ID = numeric_expression ]
)
```

## AI_GENERATE_EMBEDDINGS (Transact-SQL)

`docs/t-sql/functions/ai-generate-embeddings-transact-sql.md`

### Syntax

```syntaxsql
AI_GENERATE_EMBEDDINGS ( source USE MODEL model_identifier [ PARAMETERS optional_json_request_body_parameters ] )
```

## AI_GENERATE_RESPONSE (Transact-SQL)

`docs/t-sql/functions/ai-generate-response-transact-sql.md`

### Syntax

```syntaxsql
AI_GENERATE_RESPONSE ( prompt [ , data ] [ (NULL | ERROR | DEFAULT <value>) ON ERROR ] )
```

## AI_SUMMARIZE (Transact-SQL)

`docs/t-sql/functions/ai-summarize-transact-sql.md`

### Syntax

```syntaxsql
AI_SUMMARIZE ( text [ (NULL | ERROR | DEFAULT <value>) ON ERROR ] )
```

## AI_TRANSLATE (Transact-SQL)

`docs/t-sql/functions/ai-translate-transact-sql.md`

### Syntax

```syntaxsql
AI_TRANSLATE ( text, lang_code [ (NULL | ERROR | DEFAULT <value>) ON ERROR ] )
```

## ANY_VALUE (Transact-SQL)

`docs/t-sql/functions/any-value-transact-sql.md`

### Syntax

> Aggregation function syntax:

```syntaxsql
ANY_VALUE ( [ ALL | DISTINCT ] expression )
```

> Analytic function syntax:

```syntaxsql
ANY_VALUE ( [ ALL | DISTINCT ] expression) OVER ( [ <partition_by_clause> ] [ <order_by_clause> ] )
```

## APP_NAME (Transact-SQL)

`docs/t-sql/functions/app-name-transact-sql.md`

### Syntax

```syntaxsql
APP_NAME  ( )
```

## APPLOCK_MODE (Transact-SQL)

`docs/t-sql/functions/applock-mode-transact-sql.md`

### Syntax

```syntaxsql
APPLOCK_MODE( 'database_principal' , 'resource_name' , 'lock_owner' )
```

## APPLOCK_TEST (Transact-SQL)

`docs/t-sql/functions/applock-test-transact-sql.md`

### Syntax

```syntaxsql
APPLOCK_TEST ( 'database_principal' , 'resource_name' , 'lock_mode' , 'lock_owner' )
```

## APPROX_COUNT_DISTINCT (Transact-SQL)

`docs/t-sql/functions/approx-count-distinct-transact-sql.md`

### Syntax

```syntaxsql
APPROX_COUNT_DISTINCT ( expression )
```

## APPROX_PERCENTILE_CONT (Transact-SQL)

`docs/t-sql/functions/approx-percentile-cont-transact-sql.md`

### Syntax

```syntaxsql
APPROX_PERCENTILE_CONT (numeric_literal)
WITHIN GROUP (ORDER BY order_by_expression [ASC|DESC])
```

## APPROX_PERCENTILE_DISC (Transact-SQL)

`docs/t-sql/functions/approx-percentile-disc-transact-sql.md`

### Syntax

```syntaxsql
APPROX_PERCENTILE_DISC (numeric_literal)
WITHIN GROUP (ORDER BY order_by_expression [ASC|DESC])
```

## ASCII (Transact-SQL)

`docs/t-sql/functions/ascii-transact-sql.md`

### Syntax

```syntaxsql
ASCII ( character_expression )
```

## ASIN (Transact-SQL)

`docs/t-sql/functions/asin-transact-sql.md`

### Syntax

```syntaxsql
ASIN ( float_expression )
```

## ASSEMBLYPROPERTY (Transact-SQL)

`docs/t-sql/functions/assemblyproperty-transact-sql.md`

### Syntax

```syntaxsql
ASSEMBLYPROPERTY('assembly_name', 'property_name')
```

## ASYMKEY_ID (Transact-SQL)

`docs/t-sql/functions/asymkey-id-transact-sql.md`

### Syntax

```syntaxsql
ASYMKEY_ID ( 'Asym_Key_Name' )
```

## ASYMKEYPROPERTY (Transact-SQL)

`docs/t-sql/functions/asymkeyproperty-transact-sql.md`

### Syntax

```syntaxsql
ASYMKEYPROPERTY (Key_ID , 'algorithm_desc' | 'string_sid' | 'sid')
```

## ATAN (Transact-SQL)

`docs/t-sql/functions/atan-transact-sql.md`

### Syntax

```syntaxsql
ATAN ( float_expression )
```

## ATN2 (Transact-SQL)

`docs/t-sql/functions/atn2-transact-sql.md`

### Syntax

```syntaxsql
ATN2 ( float_expression , float_expression )
```

## AVG (Transact-SQL)

`docs/t-sql/functions/avg-transact-sql.md`

### Syntax

```syntaxsql
AVG ( [ ALL | DISTINCT ] expression )
   [ OVER ( [ partition_by_clause ] order_by_clause ) ]
```

## BASE64_DECODE (Transact-SQL)

`docs/t-sql/functions/base64-decode-transact-sql.md`

### Syntax

```syntaxsql
BASE64_DECODE ( expression )
```

## BASE64_ENCODE (Transact-SQL)

`docs/t-sql/functions/base64-encode-transact-sql.md`

### Syntax

```syntaxsql
BASE64_ENCODE (expression [ , url_safe ] )
```

## BINARY_CHECKSUM (Transact-SQL)

`docs/t-sql/functions/binary-checksum-transact-sql.md`

### Syntax

```syntaxsql
BINARY_CHECKSUM ( * | expression [ , ...n ] )
```

## BIT_COUNT (Transact-SQL)

`docs/t-sql/functions/bit-count-transact-sql.md`

### Syntax

```syntaxsql
BIT_COUNT ( expression_value )
```

## CAST and CONVERT (Transact-SQL)

`docs/t-sql/functions/cast-and-convert-transact-sql.md`

### Syntax

> `CAST` syntax:

```syntaxsql
CAST ( expression AS data_type [ ( length ) ] )
```

> `CONVERT` syntax:

```syntaxsql
CONVERT ( data_type [ ( length ) ] , expression [ , style ] )
```

## CEILING (Transact-SQL)

`docs/t-sql/functions/ceiling-transact-sql.md`

### Syntax

```syntaxsql
CEILING ( numeric_expression )
```

## CERT_ID (Transact-SQL)

`docs/t-sql/functions/cert-id-transact-sql.md`

### Syntax

```syntaxsql
CERT_ID ( 'cert_name' )
```

## CERTENCODED (Transact-SQL)

`docs/t-sql/functions/certencoded-transact-sql.md`

### Syntax

```syntaxsql
CERTENCODED ( cert_id )
```

## CERTPRIVATEKEY (Transact-SQL)

`docs/t-sql/functions/certprivatekey-transact-sql.md`

### Syntax

```syntaxsql
CERTPRIVATEKEY
    (
          cert_ID
        , ' encryption_password '
      [ , ' decryption_password ' ]
    )
```

## CERTPROPERTY (Transact-SQL)

`docs/t-sql/functions/certproperty-transact-sql.md`

### Syntax

```syntaxsql
CertProperty ( Cert_ID , '<PropertyName>' )

<PropertyName> ::=
   Expiry_Date | Start_Date | Issuer_Name
   | Cert_Serial_Number | Subject | SID | String_SID
```

## CHAR (Transact-SQL)

`docs/t-sql/functions/char-transact-sql.md`

### Syntax

```syntaxsql
CHAR ( integer_expression )
```

## CHARINDEX (Transact-SQL)

`docs/t-sql/functions/charindex-transact-sql.md`

### Syntax

```syntaxsql
CHARINDEX ( expressionToFind , expressionToSearch [ , start_location ] )
```

## CHECKSUM_AGG (Transact-SQL)

`docs/t-sql/functions/checksum-agg-transact-sql.md`

### Syntax

```syntaxsql
CHECKSUM_AGG ( [ ALL | DISTINCT ] expression )
```

## CHECKSUM (Transact-SQL)

`docs/t-sql/functions/checksum-transact-sql.md`

### Syntax

```syntaxsql
CHECKSUM ( * | expression [ ,...n ] )
```

## COL_LENGTH (Transact-SQL)

`docs/t-sql/functions/col-length-transact-sql.md`

### Syntax

```syntaxsql
COL_LENGTH ( 'table' , 'column' )
```

## COL_NAME (Transact-SQL)

`docs/t-sql/functions/col-name-transact-sql.md`

### Syntax

```syntaxsql
COL_NAME ( table_id , column_id )
```

## COLLATIONPROPERTY (Transact-SQL)

`docs/t-sql/functions/collation-functions-collationproperty-transact-sql.md`

### Syntax

```syntaxsql
COLLATIONPROPERTY( collation_name , property )
```

## TERTIARY_WEIGHTS (Transact-SQL)

`docs/t-sql/functions/collation-functions-tertiary-weights-transact-sql.md`

### Syntax

```syntaxsql
TERTIARY_WEIGHTS( non_Unicode_character_string_expression )
```

## COLUMNPROPERTY (Transact-SQL)

`docs/t-sql/functions/columnproperty-transact-sql.md`

### Syntax

```syntaxsql
COLUMNPROPERTY ( id , column , property )
```

## COLUMNS_UPDATED (Transact-SQL)

`docs/t-sql/functions/columns-updated-transact-sql.md`

### Syntax

```syntaxsql
COLUMNS_UPDATED ( )
```

## COMPRESS (Transact-SQL)

`docs/t-sql/functions/compress-transact-sql.md`

### Syntax

```syntaxsql
COMPRESS ( expression )
```

## CONCAT (Transact-SQL)

`docs/t-sql/functions/concat-transact-sql.md`

### Syntax

```syntaxsql
CONCAT ( argument1 , argument2 [ , argumentN ] ... )
```

## CONCAT_WS (Transact-SQL)

`docs/t-sql/functions/concat-ws-transact-sql.md`

### Syntax

```syntaxsql
CONCAT_WS ( separator , argument1 , argument2 [ , argumentN ] ... )
```

## CONNECTIONPROPERTY (Transact-SQL)

`docs/t-sql/functions/connectionproperty-transact-sql.md`

### Syntax

```syntaxsql
CONNECTIONPROPERTY ( property )
```

## @@CONNECTIONS (Transact-SQL)

`docs/t-sql/functions/connections-transact-sql.md`

### Syntax

```syntaxsql
@@CONNECTIONS
```

## CONTEXT_INFO (Transact-SQL)

`docs/t-sql/functions/context-info-transact-sql.md`

### Syntax

```syntaxsql
CONTEXT_INFO()
```

## COS (Transact-SQL)

`docs/t-sql/functions/cos-transact-sql.md`

### Syntax

```syntaxsql
COS ( float_expression )
```

## COT (Transact-SQL)

`docs/t-sql/functions/cot-transact-sql.md`

### Syntax

```syntaxsql
COT ( float_expression )
```

## COUNT_BIG (Transact-SQL)

`docs/t-sql/functions/count-big-transact-sql.md`

### Syntax

> Aggregation function syntax:

```syntaxsql
COUNT_BIG ( { [ [ ALL | DISTINCT ] expression ] | * } )
```

> Analytic function syntax:

```syntaxsql
COUNT_BIG ( { [ ALL ] expression | * } ) OVER ( [ <partition_by_clause> ] )
```

## COUNT (Transact-SQL)

`docs/t-sql/functions/count-transact-sql.md`

### Syntax

> Aggregation function syntax:

```syntaxsql
COUNT ( { [ [ ALL | DISTINCT ] expression ] | * } )
```

> Analytic function syntax:

```syntaxsql
COUNT ( { [ ALL ] expression | * } ) OVER ( [ <partition_by_clause> ] )
```

## CPU_BUSY (Transact-SQL)

`docs/t-sql/functions/cpu-busy-transact-sql.md`

### Syntax

```syntaxsql
@@CPU_BUSY
```

## CRYPT_GEN_RANDOM (Transact-SQL)

`docs/t-sql/functions/crypt-gen-random-transact-sql.md`

### Syntax

```syntaxsql
CRYPT_GEN_RANDOM ( length [ , seed ] )
```

## CUME_DIST (Transact-SQL)

`docs/t-sql/functions/cume-dist-transact-sql.md`

### Syntax

```syntaxsql
CUME_DIST( )
    OVER ( [ partition_by_clause ] order_by_clause )
```

## CURRENT_DATE (Transact-SQL)

`docs/t-sql/functions/current-date-transact-sql.md`

### Syntax

```syntaxsql
CURRENT_DATE
```

## CURRENT_REQUEST_ID (Transact-SQL)

`docs/t-sql/functions/current-request-id-transact-sql.md`

### Syntax

```syntaxsql
CURRENT_REQUEST_ID()
```

## CURRENT_TIMESTAMP (Transact-SQL)

`docs/t-sql/functions/current-timestamp-transact-sql.md`

### Syntax

```syntaxsql
CURRENT_TIMESTAMP
```

## CURRENT_TIMEZONE_ID (Transact-SQL)

`docs/t-sql/functions/current-timezone-id-transact-sql.md`

### Syntax

```syntaxsql
CURRENT_TIMEZONE_ID ( )
```

## CURRENT_TIMEZONE (Transact-SQL)

`docs/t-sql/functions/current-timezone-transact-sql.md`

### Syntax

```syntaxsql
CURRENT_TIMEZONE ( )
```

## CURRENT_TRANSACTION_ID (Transact-SQL)

`docs/t-sql/functions/current-transaction-id-transact-sql.md`

### Syntax

```syntaxsql
CURRENT_TRANSACTION_ID( )
```

## CURRENT_USER (Transact-SQL)

`docs/t-sql/functions/current-user-transact-sql.md`

### Syntax

```syntaxsql
CURRENT_USER
```

## @@CURSOR_ROWS (Transact-SQL)

`docs/t-sql/functions/cursor-rows-transact-sql.md`

### Syntax

```syntaxsql
@@CURSOR_ROWS
```

## CURSOR_STATUS (Transact-SQL)

`docs/t-sql/functions/cursor-status-transact-sql.md`

### Syntax

```syntaxsql
CURSOR_STATUS
     (
          { 'local' , 'cursor_name' }
          | { 'global' , 'cursor_name' }
          | { 'variable' , 'cursor_variable' }
     )
```

## DATABASE_PRINCIPAL_ID (Transact-SQL)

`docs/t-sql/functions/database-principal-id-transact-sql.md`

### Syntax

```syntaxsql
DATABASE_PRINCIPAL_ID ( 'principal_name' )
```

## DATABASEPROPERTYEX (Transact-SQL)

`docs/t-sql/functions/databasepropertyex-transact-sql.md`

### Syntax

```syntaxsql
DATABASEPROPERTYEX ( database , property )
```

## DATALENGTH (Transact-SQL)

`docs/t-sql/functions/datalength-transact-sql.md`

### Syntax

```syntaxsql
DATALENGTH ( expression )
```

## DATE_BUCKET (Transact-SQL)

`docs/t-sql/functions/date-bucket-transact-sql.md`

### Syntax

```syntaxsql
DATE_BUCKET (datepart , number , date [ , origin ] )
```

## DATEADD (Transact-SQL)

`docs/t-sql/functions/dateadd-transact-sql.md`

### Syntax

```syntaxsql
DATEADD ( datepart , number , date )
```

## DATEDIFF_BIG (Transact-SQL)

`docs/t-sql/functions/datediff-big-transact-sql.md`

### Syntax

```syntaxsql
DATEDIFF_BIG ( datepart , startdate , enddate )
```

## DATEDIFF (Transact-SQL)

`docs/t-sql/functions/datediff-transact-sql.md`

### Syntax

```syntaxsql
DATEDIFF ( datepart , startdate , enddate )
```

## @@DATEFIRST (Transact-SQL)

`docs/t-sql/functions/datefirst-transact-sql.md`

### Syntax

```syntaxsql
@@DATEFIRST
```

## DATEFROMPARTS (Transact-SQL)

`docs/t-sql/functions/datefromparts-transact-sql.md`

### Syntax

```syntaxsql
DATEFROMPARTS ( year, month, day )
```

## DATENAME (Transact-SQL)

`docs/t-sql/functions/datename-transact-sql.md`

### Syntax

```syntaxsql
DATENAME ( datepart , date )
```

## DATEPART (Transact-SQL)

`docs/t-sql/functions/datepart-transact-sql.md`

### Syntax

```syntaxsql
DATEPART ( datepart , date )
```

## DATETIME2FROMPARTS (Transact-SQL)

`docs/t-sql/functions/datetime2fromparts-transact-sql.md`

### Syntax

```syntaxsql
DATETIME2FROMPARTS ( year, month, day, hour, minute, seconds, fractions, precision )
```

## DATETIMEFROMPARTS (Transact-SQL)

`docs/t-sql/functions/datetimefromparts-transact-sql.md`

### Syntax

```syntaxsql
DATETIMEFROMPARTS ( year , month , day , hour , minute , seconds , milliseconds )
```

## DATETIMEOFFSETFROMPARTS (Transact-SQL)

`docs/t-sql/functions/datetimeoffsetfromparts-transact-sql.md`

### Syntax

```syntaxsql
DATETIMEOFFSETFROMPARTS ( year, month, day, hour, minute, seconds, fractions, hour_offset, minute_offset, precision )
```

## DATETRUNC (Transact-SQL)

`docs/t-sql/functions/datetrunc-transact-sql.md`

### Syntax

```syntaxsql
DATETRUNC ( datepart , date )
```

## DAY (Transact-SQL)

`docs/t-sql/functions/day-transact-sql.md`

### Syntax

```syntaxsql
DAY ( date )
```

## DB_ID (Transact-SQL)

`docs/t-sql/functions/db-id-transact-sql.md`

### Syntax

```syntaxsql
DB_ID ( [ 'database_name' ] )
```

## DB_NAME (Transact-SQL)

`docs/t-sql/functions/db-name-transact-sql.md`

### Syntax

```syntaxsql
DB_NAME ( [ database_id ] )
```

## DBTS (Transact-SQL)

`docs/t-sql/functions/dbts-transact-sql.md`

### Syntax

```syntaxsql
@@DBTS
```

## DECOMPRESS (Transact-SQL)

`docs/t-sql/functions/decompress-transact-sql.md`

### Syntax

```syntaxsql
DECOMPRESS ( expression )
```

## DECRYPTBYASYMKEY (Transact-SQL)

`docs/t-sql/functions/decryptbyasymkey-transact-sql.md`

### Syntax

```syntaxsql
DecryptByAsymKey (Asym_Key_ID , { 'ciphertext' | @ciphertext }
    [ , 'Asym_Key_Password' ] )
```

## DECRYPTBYCERT (Transact-SQL)

`docs/t-sql/functions/decryptbycert-transact-sql.md`

### Syntax

```syntaxsql
DecryptByCert ( certificate_ID , { 'ciphertext' | @ciphertext }
    [ , { 'cert_password' | @cert_password } ] )
```

## DECRYPTBYKEY (Transact-SQL)

`docs/t-sql/functions/decryptbykey-transact-sql.md`

### Syntax

```syntaxsql
DECRYPTBYKEY ( { 'ciphertext' | @ciphertext }
    [ , add_authenticator , { authenticator | @authenticator } ] )
```

## DECRYPTBYKEYAUTOASYMKEY (Transact-SQL)

`docs/t-sql/functions/decryptbykeyautoasymkey-transact-sql.md`

### Syntax

```syntaxsql
DECRYPTBYKEYAUTOASYMKEY ( akey_ID , akey_password
    , { 'ciphertext' | @ciphertext }
  [ , { add_authenticator | @add_authenticator }
  [ , { authenticator | @authenticator } ] ] )
```

## DECRYPTBYKEYAUTOCERT (Transact-SQL)

`docs/t-sql/functions/decryptbykeyautocert-transact-sql.md`

### Syntax

```syntaxsql
DECRYPTBYKEYAUTOCERT ( cert_ID , cert_password
    , { 'ciphertext' | @ciphertext }
  [ , { add_authenticator | @add_authenticator }
  [ , { authenticator | @authenticator } ] ] )
```

## DECRYPTBYPASSPHRASE (Transact-SQL)

`docs/t-sql/functions/decryptbypassphrase-transact-sql.md`

### Syntax

```syntaxsql
DecryptByPassPhrase ( { 'passphrase' | @passphrase }
    , { 'ciphertext' | @ciphertext }
  [ , { add_authenticator | @add_authenticator }
    , { authenticator | @authenticator } ] )
```

## DEGREES (Transact-SQL)

`docs/t-sql/functions/degrees-transact-sql.md`

### Syntax

```syntaxsql
DEGREES ( numeric_expression )
```

## DENSE_RANK (Transact-SQL)

`docs/t-sql/functions/dense-rank-transact-sql.md`

### Syntax

```syntaxsql
DENSE_RANK ( ) OVER ( [ <partition_by_clause> ] < order_by_clause > )
```

## DIFFERENCE (Transact-SQL)

`docs/t-sql/functions/difference-transact-sql.md`

### Syntax

```syntaxsql
DIFFERENCE ( character_expression , character_expression )
```

## EDGE_ID_FROM_PARTS (Transact-SQL)

`docs/t-sql/functions/edge-id-from-parts-transact-sql.md`

### Syntax

```syntaxsql
EDGE_ID_FROM_PARTS ( object_id, graph_id )
```

## EDIT_DISTANCE_SIMILARITY (Transact-SQL)

`docs/t-sql/functions/edit-distance-similarity-transact-sql.md`

### Syntax

```syntaxsql
EDIT_DISTANCE_SIMILARITY (
    character_expression
    , character_expression
)
```

## EDIT_DISTANCE (Transact-SQL)

`docs/t-sql/functions/edit-distance-transact-sql.md`

### Syntax

```syntaxsql
EDIT_DISTANCE (
    character_expression
    , character_expression [ , maximum_distance ]
)
```

## ENCRYPTBYASYMKEY (Transact-SQL)

`docs/t-sql/functions/encryptbyasymkey-transact-sql.md`

### Syntax

```syntaxsql
EncryptByAsymKey ( Asym_Key_ID , { 'plaintext' | @plaintext } )
```

## ENCRYPTBYCERT (Transact-SQL)

`docs/t-sql/functions/encryptbycert-transact-sql.md`

### Syntax

```syntaxsql
EncryptByCert ( certificate_ID , { 'cleartext' | @cleartext } )
```

## ENCRYPTBYKEY (Transact-SQL)

`docs/t-sql/functions/encryptbykey-transact-sql.md`

### Syntax

```syntaxsql
EncryptByKey ( key_GUID , { 'cleartext' | @cleartext }
    [, { add_authenticator | @add_authenticator }
     , { authenticator | @authenticator } ] )
```

## ENCRYPTBYPASSPHRASE (Transact-SQL)

`docs/t-sql/functions/encryptbypassphrase-transact-sql.md`

### Syntax

```syntaxsql
EncryptByPassPhrase ( { 'passphrase' | @passphrase }
    , { 'cleartext' | @cleartext }
  [ , { add_authenticator | @add_authenticator }
    , { authenticator | @authenticator } ] )
```

## EOMONTH (Transact-SQL)

`docs/t-sql/functions/eomonth-transact-sql.md`

### Syntax

```syntaxsql
EOMONTH ( start_date [ , month_to_add ] )
```

## ERROR_LINE (Transact-SQL)

`docs/t-sql/functions/error-line-transact-sql.md`

### Syntax

```syntaxsql
ERROR_LINE ( )
```

## ERROR_MESSAGE (Transact-SQL)

`docs/t-sql/functions/error-message-transact-sql.md`

### Syntax

```syntaxsql
ERROR_MESSAGE ( )
```

## ERROR_NUMBER (Transact-SQL)

`docs/t-sql/functions/error-number-transact-sql.md`

### Syntax

```syntaxsql
ERROR_NUMBER ( )
```

## ERROR_PROCEDURE (Transact-SQL)

`docs/t-sql/functions/error-procedure-transact-sql.md`

### Syntax

```syntaxsql
ERROR_PROCEDURE ( )
```

## ERROR_SEVERITY (Transact-SQL)

`docs/t-sql/functions/error-severity-transact-sql.md`

### Syntax

```syntaxsql
ERROR_SEVERITY ( )
```

## ERROR_STATE (Transact-SQL)

`docs/t-sql/functions/error-state-transact-sql.md`

### Syntax

```syntaxsql
ERROR_STATE ( )
```

## @@ERROR (Transact-SQL)

`docs/t-sql/functions/error-transact-sql.md`

### Syntax

```
@@ERROR
```

## EVENTDATA (Transact-SQL)

`docs/t-sql/functions/eventdata-transact-sql.md`

### Syntax

```syntaxsql
EVENTDATA( )
```

## EXP (Transact-SQL)

`docs/t-sql/functions/exp-transact-sql.md`

### Syntax

```syntaxsql
EXP ( float_expression )
```

## FETCH_STATUS (Transact-SQL)

`docs/t-sql/functions/fetch-status-transact-sql.md`

### Syntax

```syntaxsql
@@FETCH_STATUS
```

## FILE_ID (Transact-SQL)

`docs/t-sql/functions/file-id-transact-sql.md`

### Syntax

```syntaxsql
FILE_ID ( file_name )
```

## FILE_IDEX (Transact-SQL)

`docs/t-sql/functions/file-idex-transact-sql.md`

### Syntax

```syntaxsql
FILE_IDEX ( file_name )
```

## FILE_NAME (Transact-SQL)

`docs/t-sql/functions/file-name-transact-sql.md`

### Syntax

```syntaxsql
FILE_NAME ( file_id )
```

## FILEGROUP_ID (Transact-SQL)

`docs/t-sql/functions/filegroup-id-transact-sql.md`

### Syntax

```syntaxsql
FILEGROUP_ID ( 'filegroup_name' )
```

## FILEGROUP_NAME (Transact-SQL)

`docs/t-sql/functions/filegroup-name-transact-sql.md`

### Syntax

```syntaxsql
FILEGROUP_NAME ( filegroup_id )
```

## FILEGROUPPROPERTY (Transact-SQL)

`docs/t-sql/functions/filegroupproperty-transact-sql.md`

### Syntax

```syntaxsql
FILEGROUPPROPERTY ( filegroup_name , property )
```

## FILEPROPERTY (Transact-SQL)

`docs/t-sql/functions/fileproperty-transact-sql.md`

### Syntax

```syntaxsql
FILEPROPERTY ( file_name , property )
```

## FILEPROPERTYEX (Transact-SQL)

`docs/t-sql/functions/filepropertyex-transact-sql.md`

### Syntax

```syntaxsql
FILEPROPERTYEX ( name , property )
```

## FIRST_VALUE (Transact-SQL)

`docs/t-sql/functions/first-value-transact-sql.md`

### Syntax

```syntaxsql
FIRST_VALUE ( [ scalar_expression ] ) [ IGNORE NULLS | RESPECT NULLS ]
    OVER ( [ partition_by_clause ] order_by_clause [ rows_range_clause ] )
```

## FLOOR (Transact-SQL)

`docs/t-sql/functions/floor-transact-sql.md`

### Syntax

```syntaxsql
FLOOR ( numeric_expression )
```

## FORMAT (Transact-SQL)

`docs/t-sql/functions/format-transact-sql.md`

### Syntax

```syntaxsql
FORMAT( value , format [ , culture ] )
```

### E. Format with comma separators for large numbers

> This example uses the `N` format specifier. The `N` specifier is used for numeric values, and the number of decimal places can be adjusted by changing the format string (for example, `N2` for two decimal places).

```syntaxsql
FORMAT ( value, format_string [, culture ] )
```

## FORMATMESSAGE (Transact-SQL)

`docs/t-sql/functions/formatmessage-transact-sql.md`

### Syntax

```syntaxsql
FORMATMESSAGE ( { msg_number  | ' msg_string ' | @msg_variable} , [ param_value [ ,...n ] ] )
```

## FULLTEXTCATALOGPROPERTY (Transact-SQL)

`docs/t-sql/functions/fulltextcatalogproperty-transact-sql.md`

### Syntax

```syntaxsql
FULLTEXTCATALOGPROPERTY ('catalog_name' ,'property')
```

## FULLTEXTSERVICEPROPERTY (Transact-SQL)

`docs/t-sql/functions/fulltextserviceproperty-transact-sql.md`

### Syntax

```syntaxsql
FULLTEXTSERVICEPROPERTY ('property')
```

## GENERATE_SERIES (Transact-SQL)

`docs/t-sql/functions/generate-series-transact-sql.md`

### Syntax

```syntaxsql
GENERATE_SERIES ( start , stop [ , step ] )
```

## GET_BIT (Transact-SQL)

`docs/t-sql/functions/get-bit-transact-sql.md`

### Syntax

```syntaxsql
GET_BIT ( expression_value , bit_offset )
```

## GET_FILESTREAM_TRANSACTION_CONTEXT (Transact-SQL)

`docs/t-sql/functions/get-filestream-transaction-context-transact-sql.md`

### Syntax

```syntaxsql
GET_FILESTREAM_TRANSACTION_CONTEXT()
```

## GETANSINULL (Transact-SQL)

`docs/t-sql/functions/getansinull-transact-sql.md`

### Syntax

```syntaxsql
GETANSINULL ( [ 'database' ] )
```

## GETDATE (Transact-SQL)

`docs/t-sql/functions/getdate-transact-sql.md`

### Syntax

```syntaxsql
GETDATE()
```

## GETUTCDATE (Transact-SQL)

`docs/t-sql/functions/getutcdate-transact-sql.md`

### Syntax

```syntaxsql
GETUTCDATE()
```

## GRAPH_ID_FROM_EDGE_ID (Transact-SQL)

`docs/t-sql/functions/graph-id-from-edge-id-transact-sql.md`

### Syntax

```syntaxsql
GRAPH_ID_FROM_EDGE_ID ( edge_id )
```

## GRAPH_ID_FROM_NODE_ID (Transact-SQL)

`docs/t-sql/functions/graph-id-from-node-id-transact-sql.md`

### Syntax

```syntaxsql
GRAPH_ID_FROM_NODE_ID ( node_id )
```

## GROUPING_ID (Transact-SQL)

`docs/t-sql/functions/grouping-id-transact-sql.md`

### Syntax

```syntaxsql
GROUPING_ID ( <column_expression> [ , ...n ] )
```

## GROUPING (Transact-SQL)

`docs/t-sql/functions/grouping-transact-sql.md`

### Syntax

```syntaxsql
GROUPING ( <column_expression> )
```

## HAS_DBACCESS (Transact-SQL)

`docs/t-sql/functions/has-dbaccess-transact-sql.md`

### Syntax

```syntaxsql
HAS_DBACCESS ( 'database_name' )
```

## HAS_PERMS_BY_NAME (Transact-SQL)

`docs/t-sql/functions/has-perms-by-name-transact-sql.md`

### Syntax

```syntaxsql
HAS_PERMS_BY_NAME ( securable , securable_class , permission
    [ , sub-securable ] [ , sub-securable_class ] )
```

## HASHBYTES (Transact-SQL)

`docs/t-sql/functions/hashbytes-transact-sql.md`

### Syntax

```syntaxsql
HASHBYTES ( '<algorithm>', { @input | 'input' } )

<algorithm>::= MD2 | MD4 | MD5 | SHA | SHA1 | SHA2_256 | SHA2_512
```

## HOST_ID (Transact-SQL)

`docs/t-sql/functions/host-id-transact-sql.md`

### Syntax

```syntaxsql
HOST_ID ()
```

## HOST_NAME (Transact-SQL)

`docs/t-sql/functions/host-name-transact-sql.md`

### Syntax

```syntaxsql
HOST_NAME ()
```

## IDENT_CURRENT (Transact-SQL)

`docs/t-sql/functions/ident-current-transact-sql.md`

### Syntax

```syntaxsql
IDENT_CURRENT( 'table_or_view' )
```

## IDENT_INCR (Transact-SQL)

`docs/t-sql/functions/ident-incr-transact-sql.md`

### Syntax

```syntaxsql
IDENT_INCR ( 'table_or_view' )
```

## IDENT_SEED (Transact-SQL)

`docs/t-sql/functions/ident-seed-transact-sql.md`

### Syntax

```syntaxsql
IDENT_SEED ( 'table_or_view' )
```

## IDENTITY (Function) (Transact-SQL)

`docs/t-sql/functions/identity-function-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, Azure SQL Managed Instance, SQL database in Fabric:

```syntaxsql
IDENTITY ( data_type [ , seed , increment ] ) AS column_name
```

> Syntax for Fabric Data Warehouse:

```syntaxsql
IDENTITY ( data_type ) AS column_name
```

## @@IDENTITY (Transact-SQL)

`docs/t-sql/functions/identity-transact-sql.md`

### Syntax

```syntaxsql
@@IDENTITY
```

## @@IDLE (Transact-SQL)

`docs/t-sql/functions/idle-transact-sql.md`

### Syntax

```syntaxsql
@@IDLE
```

## INDEX_COL (Transact-SQL)

`docs/t-sql/functions/index-col-transact-sql.md`

### Syntax

```syntaxsql
INDEX_COL ( '[ database_name . [ schema_name ] .| schema_name ]
    table_or_view_name', index_id , key_id )
```

## INDEXKEY_PROPERTY (Transact-SQL)

`docs/t-sql/functions/indexkey-property-transact-sql.md`

### Syntax

```syntaxsql
INDEXKEY_PROPERTY ( object_ID ,index_ID ,key_ID ,property )
```

## INDEXPROPERTY (Transact-SQL)

`docs/t-sql/functions/indexproperty-transact-sql.md`

### Syntax

```syntaxsql
INDEXPROPERTY ( object_ID , index_or_statistics_name , property )
```

## @@IO_BUSY (Transact-SQL)

`docs/t-sql/functions/io-busy-transact-sql.md`

### Syntax

```syntaxsql
@@IO_BUSY
```

## IS_MEMBER (Transact-SQL)

`docs/t-sql/functions/is-member-transact-sql.md`

### Syntax

```syntaxsql
IS_MEMBER ( { 'group' | 'role' } )
```

## IS_OBJECTSIGNED (Transact-SQL)

`docs/t-sql/functions/is-objectsigned-transact-sql.md`

### Syntax

```syntaxsql
IS_OBJECTSIGNED (
'OBJECT', @object_id, @class, @thumbprint
  )
```

## IS_ROLEMEMBER (Transact-SQL)

`docs/t-sql/functions/is-rolemember-transact-sql.md`

### Syntax

```syntaxsql
IS_ROLEMEMBER ( 'role' [ , 'database_principal' ] )
```

## IS_SRVROLEMEMBER (Transact-SQL)

`docs/t-sql/functions/is-srvrolemember-transact-sql.md`

### Syntax

```syntaxsql
IS_SRVROLEMEMBER ( 'role' [ , 'login' ] )
```

## ISDATE (Transact-SQL)

`docs/t-sql/functions/isdate-transact-sql.md`

### Syntax

```syntaxsql
ISDATE ( expression )
```

## ISJSON (Transact-SQL)

`docs/t-sql/functions/isjson-transact-sql.md`

### Syntax

```syntaxsql
ISJSON ( expression [, json_type_constraint] )
```

## ISNULL (Transact-SQL)

`docs/t-sql/functions/isnull-transact-sql.md`

### Syntax

```syntaxsql
ISNULL ( check_expression , replacement_value )
```

## ISNUMERIC (Transact-SQL)

`docs/t-sql/functions/isnumeric-transact-sql.md`

### Syntax

```syntaxsql
ISNUMERIC ( expression )
```

## JARO_WINKLER_DISTANCE (Transact-SQL)

`docs/t-sql/functions/jaro-winkler-distance-transact-sql.md`

### Syntax

```syntaxsql
JARO_WINKLER_DISTANCE (
    character_expression
    , character_expression
)
```

## JARO_WINKLER_SIMILARITY (Transact-SQL)

`docs/t-sql/functions/jaro-winkler-similarity-transact-sql.md`

### Syntax

```syntaxsql
JARO_WINKLER_SIMILARITY (
    character_expression
    , character_expression
)
```

## JSON_ARRAY (Transact-SQL)

`docs/t-sql/functions/json-array-transact-sql.md`

### Syntax

```syntaxsql
JSON_ARRAY ( [ <json_array_value> [ , ...n ] ] [ <json_null_clause> ] [ RETURNING json ] )

<json_array_value> ::= value_expression

<json_null_clause> ::=
      NULL ON NULL
    | ABSENT ON NULL
```

## JSON_ARRAYAGG (Transact-SQL)

`docs/t-sql/functions/json-arrayagg-transact-sql.md`

### Syntax

```syntaxsql
JSON_ARRAYAGG (value_expression [ order_by_clause ] [ json_null_clause ] [ RETURNING json ] )

json_null_clause ::=  NULL ON NULL | ABSENT ON NULL

order_by_clause ::= ORDER BY <column_list>
```

## JSON_CONTAINS (Transact-SQL)

`docs/t-sql/functions/json-contains-transact-sql.md`

### Syntax

```syntaxsql
JSON_CONTAINS( target_expression , search_value_expression [ , path_expression ]  [ , search_mode ] )
```

## JSON_MODIFY (Transact-SQL)

`docs/t-sql/functions/json-modify-transact-sql.md`

### Syntax

```syntaxsql
JSON_MODIFY ( expression , path , newValue )
```

## JSON_OBJECT (Transact-SQL)

`docs/t-sql/functions/json-object-transact-sql.md`

### Syntax

```syntaxsql
JSON_OBJECT ( [ <json_key_value> [ , ...n ] ] [ json_null_clause ] [ RETURNING json ] )

<json_key_value> ::= json_key_name : value_expression

<json_null_clause> ::=
      NULL ON NULL
    | ABSENT ON NULL
```

## JSON_OBJECTAGG (Transact-SQL)

`docs/t-sql/functions/json-objectagg-transact-sql.md`

### Syntax

```syntaxsql
JSON_OBJECTAGG ( json_key_value [ json_null_clause ] [ RETURNING JSON ] )

json_key_value ::= <json_name> : <value_expression>

json_null_clause ::= NULL ON NULL | ABSENT ON NULL
```

## JSON_PATH_EXISTS (Transact-SQL)

`docs/t-sql/functions/json-path-exists-transact-sql.md`

### Syntax

```syntaxsql
JSON_PATH_EXISTS( value_expression , sql_json_path )
```

## JSON_QUERY (Transact-SQL)

`docs/t-sql/functions/json-query-transact-sql.md`

### Syntax

```syntaxsql
JSON_QUERY ( expression [ , path ] [ WITH ARRAY WRAPPER ] )
```

## JSON_VALUE (Transact-SQL)

`docs/t-sql/functions/json-value-transact-sql.md`

### Syntax

Marked for `<=sql-server-linux-ver16 || <=sql-server-ver16`.

> Syntax for SQL Server 2022 (16.x) and earlier versions.

```syntaxsql
JSON_VALUE ( expression , path )
```

Marked for `>=sql-server-linux-ver17 || >=sql-server-ver17`.

> Syntax for SQL Server 2025 (17.x) and later versions.

```syntaxsql
JSON_VALUE ( expression , path [ RETURNING data_type ] )
```

## KEY_GUID (Transact-SQL)

`docs/t-sql/functions/key-guid-transact-sql.md`

### Syntax

```syntaxsql
Key_GUID( 'Key_Name' )
```

## KEY_ID (Transact-SQL)

`docs/t-sql/functions/key-id-transact-sql.md`

### Syntax

```syntaxsql
Key_ID ( 'Key_Name' )
```

## KEY_NAME (Transact-SQL)

`docs/t-sql/functions/key-name-transact-sql.md`

### Syntax

```syntaxsql
KEY_NAME ( ciphertext | key_guid )
```

## LAG (Transact-SQL)

`docs/t-sql/functions/lag-transact-sql.md`

### Syntax

```syntaxsql
LAG (scalar_expression [ , offset ] [ , default ] ) [ IGNORE NULLS | RESPECT NULLS ]
    OVER ( [ partition_by_clause ] order_by_clause )
```

## @@LANGID (Transact-SQL)

`docs/t-sql/functions/langid-transact-sql.md`

### Syntax

```syntaxsql
@@LANGID
```

## @@LANGUAGE (Transact-SQL)

`docs/t-sql/functions/language-transact-sql.md`

### Syntax

```syntaxsql
@@LANGUAGE
```

## LAST_VALUE (Transact-SQL)

`docs/t-sql/functions/last-value-transact-sql.md`

### Syntax

```syntaxsql
LAST_VALUE ( [ scalar_expression ] ) [ IGNORE NULLS | RESPECT NULLS ]
    OVER ( [ partition_by_clause ] order_by_clause [ rows_range_clause ] )
```

## LEAD (Transact-SQL)

`docs/t-sql/functions/lead-transact-sql.md`

### Syntax

```syntaxsql
LEAD ( scalar_expression [ , offset ] [ , default ] ) [ IGNORE NULLS | RESPECT NULLS ]
    OVER ( [ partition_by_clause ] order_by_clause )
```

## LEFT_SHIFT (Transact-SQL)

`docs/t-sql/functions/left-shift-transact-sql.md`

### Syntax

```syntaxsql
LEFT_SHIFT ( expression_value , shift_amount )
expression_value << shift_amount
```

## LEFT (Transact-SQL)

`docs/t-sql/functions/left-transact-sql.md`

### Syntax

```syntaxsql
LEFT ( character_expression , integer_expression )
```

## LEN (Transact-SQL)

`docs/t-sql/functions/len-transact-sql.md`

### Syntax

```syntaxsql
LEN ( string_expression )
```

## @@LOCK_TIMEOUT (Transact-SQL)

`docs/t-sql/functions/lock-timeout-transact-sql.md`

### Syntax

```syntaxsql
@@LOCK_TIMEOUT
```

## LOG (Transact-SQL)

`docs/t-sql/functions/log-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server, Azure SQL Database

LOG ( float_expression [, base ] )
```

```syntaxsql
-- Syntax for Azure Synapse SQL

LOG ( float_expression )
```

## LOG10 (Transact-SQL)

`docs/t-sql/functions/log10-transact-sql.md`

### Syntax

```syntaxsql
LOG10 ( float_expression )
```

## CHOOSE (Transact-SQL)

`docs/t-sql/functions/logical-functions-choose-transact-sql.md`

### Syntax

```syntaxsql
CHOOSE ( index, val_1, val_2 [, val_n ] )
```

## GREATEST (Transact-SQL)

`docs/t-sql/functions/logical-functions-greatest-transact-sql.md`

### Syntax

```syntaxsql
GREATEST ( expression1 [ , ...expressionN ] )
```

## IIF (Transact-SQL)

`docs/t-sql/functions/logical-functions-iif-transact-sql.md`

### Syntax

```syntaxsql
IIF( boolean_expression, true_value, false_value )
```

## LEAST (Transact-SQL)

`docs/t-sql/functions/logical-functions-least-transact-sql.md`

### Syntax

```syntaxsql
LEAST ( expression1 [ , ...expressionN ] )
```

## LOGINPROPERTY (Transact-SQL)

`docs/t-sql/functions/loginproperty-transact-sql.md`

### Syntax

```syntaxsql
LOGINPROPERTY ( 'login_name' , 'property_name' )
```

## LOWER (Transact-SQL)

`docs/t-sql/functions/lower-transact-sql.md`

### Syntax

```syntaxsql
LOWER ( character_expression )
```

## LTRIM (Transact-SQL)

`docs/t-sql/functions/ltrim-transact-sql.md`

### Syntax

> Syntax for SQL Server prior to SQL Server 2022 (16.x):

```syntaxsql
LTRIM ( character_expression )
```

> > You need your database compatibility level set to 160 to use the optional *characters* argument.

```syntaxsql
LTRIM ( character_expression , [ characters ] )
```

## @@MAX_CONNECTIONS (Transact-SQL)

`docs/t-sql/functions/max-connections-transact-sql.md`

### Syntax

```syntaxsql
@@MAX_CONNECTIONS
```

## @@MAX_PRECISION (Transact-SQL)

`docs/t-sql/functions/max-precision-transact-sql.md`

### Syntax

```syntaxsql
@@MAX_PRECISION
```

## MAX (Transact-SQL)

`docs/t-sql/functions/max-transact-sql.md`

### Syntax

> Aggregation function syntax:

```syntaxsql
MAX ( [ ALL | DISTINCT ] expression )
```

> Analytic function syntax:

```syntaxsql
MAX ( [ ALL ] expression) OVER ( [ <partition_by_clause> ] [ <order_by_clause> ] )
```

## MIN_ACTIVE_ROWVERSION (Transact-SQL)

`docs/t-sql/functions/min-active-rowversion-transact-sql.md`

### Syntax

```syntaxsql
MIN_ACTIVE_ROWVERSION ( )
```

## MIN (Transact-SQL)

`docs/t-sql/functions/min-transact-sql.md`

### Syntax

> Aggregation function syntax:

```syntaxsql
MIN ( [ ALL | DISTINCT ] expression )
```

> Analytic function syntax:

```syntaxsql
MIN ( [ ALL ] expression ) OVER ( [ <partition_by_clause> ] [ <order_by_clause> ] )
```

## MONTH (Transact-SQL)

`docs/t-sql/functions/month-transact-sql.md`

### Syntax

```syntaxsql
MONTH ( date )
```

## NCHAR (Transact-SQL)

`docs/t-sql/functions/nchar-transact-sql.md`

### Syntax

```syntaxsql
NCHAR ( integer_expression )
```

## @@NESTLEVEL (Transact-SQL)

`docs/t-sql/functions/nestlevel-transact-sql.md`

### Syntax

```syntaxsql
@@NESTLEVEL
```

## NEWID (Transact-SQL)

`docs/t-sql/functions/newid-transact-sql.md`

### Syntax

```syntaxsql
NEWID ( )
```

## NEWSEQUENTIALID (Transact-SQL)

`docs/t-sql/functions/newsequentialid-transact-sql.md`

### Syntax

```syntaxsql
NEWSEQUENTIALID ( )
```

## NEXT VALUE FOR (Transact-SQL)

`docs/t-sql/functions/next-value-for-transact-sql.md`

### Syntax

```syntaxsql
NEXT VALUE FOR [ database_name . ] [ schema_name . ]  sequence_name
   [ OVER (<over_order_by_clause>) ]
```

## NODE_ID_FROM_PARTS (Transact-SQL)

`docs/t-sql/functions/node-id-from-parts-transact-sql.md`

### Syntax

```syntaxsql
NODE_ID_FROM_PARTS ( object_id, graph_id )
```

## NTILE (Transact-SQL)

`docs/t-sql/functions/ntile-transact-sql.md`

### Syntax

```syntaxsql
NTILE (integer_expression) OVER ( [ <partition_by_clause> ] <order_by_clause> )
```

## OBJECT_DEFINITION (Transact-SQL)

`docs/t-sql/functions/object-definition-transact-sql.md`

### Syntax

```syntaxsql
OBJECT_DEFINITION ( object_id )
```

## OBJECT_ID_FROM_EDGE_ID (Transact-SQL)

`docs/t-sql/functions/object-id-from-edge-id-transact-sql.md`

### Syntax

```syntaxsql
OBJECT_ID_FROM_EDGE_ID ( edge_id )
```

## OBJECT_ID_FROM_NODE_ID (Transact-SQL)

`docs/t-sql/functions/object-id-from-node-id-transact-sql.md`

### Syntax

```syntaxsql
OBJECT_ID_FROM_NODE_ID ( node_id )
```

## OBJECT_ID (Transact-SQL)

`docs/t-sql/functions/object-id-transact-sql.md`

### Syntax

```syntaxsql
OBJECT_ID ( ' [ database_name . [ schema_name ] . | schema_name . ]
  object_name' [ , 'object_type' ] )
```

## OBJECT_NAME (Transact-SQL)

`docs/t-sql/functions/object-name-transact-sql.md`

### Syntax

```syntaxsql
OBJECT_NAME ( object_id [, database_id ] )
```

## OBJECT_SCHEMA_NAME (Transact-SQL)

`docs/t-sql/functions/object-schema-name-transact-sql.md`

### Syntax

```syntaxsql
OBJECT_SCHEMA_NAME ( object_id [, database_id ] )
```

## OBJECTPROPERTY (Transact-SQL)

`docs/t-sql/functions/objectproperty-transact-sql.md`

### Syntax

```syntaxsql
OBJECTPROPERTY ( ID , property )
```

## OBJECTPROPERTYEX (Transact-SQL)

`docs/t-sql/functions/objectpropertyex-transact-sql.md`

### Syntax

```syntaxsql
OBJECTPROPERTYEX ( id , property )
```

## ODBC Scalar Functions (Transact-SQL)

`docs/t-sql/functions/odbc-scalar-functions-transact-sql.md`

### Usage

```syntaxsql
SELECT {fn <function_name> [ (<argument>, ...n) ] }
```

## OPENDATASOURCE (Transact-SQL)

`docs/t-sql/functions/opendatasource-transact-sql.md`

### Syntax

```syntaxsql
OPENDATASOURCE ( 'provider_name', 'init_string' )
```

## OPENJSON (Transact-SQL)

`docs/t-sql/functions/openjson-transact-sql.md`

### Syntax

```syntaxsql
OPENJSON( jsonExpression [ , path ] )  [ <with_clause> ]

<with_clause> ::= WITH ( { colName type [ column_path ] [ AS JSON ] } [ ,...n ] )
```

## OPENQUERY (Transact-SQL)

`docs/t-sql/functions/openquery-transact-sql.md`

### Syntax

```syntaxsql
OPENQUERY ( linked_server ,'query' )
```

## OPENROWSET BULK (Transact-SQL)

`docs/t-sql/functions/openrowset-bulk-transact-sql.md`

### Syntax

Marked for `=azuresqldb-mi-current || =azuresqldb-current || >=sql-server-2017 || >=sql-server-linux-2017 || =fabric-sqldb`.

> For SQL Server, Azure SQL Database, SQL database in Fabric, and Azure SQL Managed Instance:

```syntaxsql
OPENROWSET( BULK 'data_file_path',
            <bulk_option> ( , <bulk_option> )*
)
[
    WITH (  ( <column_name> <sql_datatype> [ '<column_path>' | <column_ordinal> ] )+ )
]

<bulk_option> ::=
   DATA_SOURCE = 'data_source_name' |

   -- file format options
   CODEPAGE = { 'ACP' | 'OEM' | 'RAW' | 'code_page' } |
   DATAFILETYPE = { 'char' | 'widechar' } |
   FORMAT = <file_format> |

   FORMATFILE = 'format_file_path' |
   FORMATFILE_DATA_SOURCE = 'data_source_name' |

   SINGLE_BLOB |
   SINGLE_CLOB |
   SINGLE_NCLOB |

   -- Text/CSV options
   ROWTERMINATOR = 'row_terminator' |
   FIELDTERMINATOR =  'field_terminator' |
   FIELDQUOTE = 'quote_character' |

   -- Error handling options
   MAXERRORS = maximum_errors |
   ERRORFILE = 'file_name' |
   ERRORFILE_DATA_SOURCE = 'data_source_name' |

   -- Execution options
   FIRSTROW = first_row |
   LASTROW = last_row |

   ORDER ( { column [ ASC | DESC ] } [ , ...n ] ) [ UNIQUE ] ] |

   ROWS_PER_BATCH = rows_per_batch
```

### Syntax for Fabric Data Warehouse

Marked for `=fabric`.

```syntaxsql
OPENROWSET( BULK 'data_file_path',
            <bulk_option> ( , <bulk_option> )*
)
[
    WITH (  ( <column_name> <sql_datatype> [ '<column_path>' | <column_ordinal> ] )+ )
]

<bulk_option> ::=
   DATA_SOURCE = 'data_source_name' |

   -- file format options
   CODEPAGE = { 'ACP' | 'OEM' | 'RAW' | 'code_page' } |
   DATAFILETYPE = { 'char' | 'widechar' } |
   FORMAT = <file_format> |

   -- Text/CSV options
   ROWTERMINATOR = 'row_terminator' |
   FIELDTERMINATOR =  'field_terminator' |
   FIELDQUOTE = 'quote_character' |
   ESCAPECHAR = 'escape_char' |
   HEADER_ROW = [true|false] |
   PARSER_VERSION = 'parser_version' |

   -- Error handling options
   MAXERRORS = maximum_errors |
   ERRORFILE = 'file_name' |

   -- Execution options
   FIRSTROW = first_row |
   LASTROW = last_row |

   ROWS_PER_BATCH = rows_per_batch
```

## OPENROWSET (Transact-SQL)

`docs/t-sql/functions/openrowset-transact-sql.md`

### Syntax

> `OPENROWSET` syntax is used to query external data sources:

```syntaxsql
OPENROWSET
(  'provider_name'
    , { 'datasource' ; 'user_id' ; 'password' | 'provider_string' }
    , {  [ catalog. ] [ schema. ] object | 'query' }
)
```

## OPENXML (Transact-SQL)

`docs/t-sql/functions/openxml-transact-sql.md`

### Syntax

```syntaxsql
OPENXML ( idoc int [ in ]
    , rowpattern nvarchar [ in ]
    , [ flags byte [ in ] ] )
[ WITH ( SchemaDeclaration | TableName ) ]
```

## @@OPTIONS (Transact-SQL)

`docs/t-sql/functions/options-transact-sql.md`

### Syntax

```syntaxsql
@@OPTIONS
```

## ORIGINAL_DB_NAME (Transact-SQL)

`docs/t-sql/functions/original-db-name-transact-sql.md`

### Syntax

```syntaxsql
ORIGINAL_DB_NAME ()
```

## ORIGINAL_LOGIN (Transact-SQL)

`docs/t-sql/functions/original-login-transact-sql.md`

### Syntax

```syntaxsql
ORIGINAL_LOGIN( )
```

## @@PACK_RECEIVED (Transact-SQL)

`docs/t-sql/functions/pack-received-transact-sql.md`

### Syntax

```syntaxsql
@@PACK_RECEIVED
```

## @@PACK_SENT (Transact-SQL)

`docs/t-sql/functions/pack-sent-transact-sql.md`

### Syntax

```syntaxsql
@@PACK_SENT
```

## @@PACKET_ERRORS (Transact-SQL)

`docs/t-sql/functions/packet-errors-transact-sql.md`

### Syntax

```syntaxsql
@@PACKET_ERRORS
```

## PARSE (Transact-SQL)

`docs/t-sql/functions/parse-transact-sql.md`

### Syntax

```syntaxsql
PARSE ( string_value AS data_type [ USING culture ] )
```

## PARSENAME (Transact-SQL)

`docs/t-sql/functions/parsename-transact-sql.md`

### Syntax

```syntaxsql
PARSENAME ('object_name' , object_piece )
```

## $PARTITION (Transact-SQL)

`docs/t-sql/functions/partition-transact-sql.md`

### Syntax

```syntaxsql
[ database_name. ] $PARTITION.partition_function_name(expression)
```

## PATINDEX (Transact-SQL)

`docs/t-sql/functions/patindex-transact-sql.md`

### Syntax

```syntaxsql
PATINDEX ( '%pattern%' , expression )
```

## PERCENT_RANK (Transact-SQL)

`docs/t-sql/functions/percent-rank-transact-sql.md`

### Syntax

```syntaxsql
PERCENT_RANK( )
    OVER ( [ partition_by_clause ] order_by_clause )
```

## PERCENTILE_CONT (Transact-SQL)

`docs/t-sql/functions/percentile-cont-transact-sql.md`

### Syntax

```syntaxsql
PERCENTILE_CONT ( numeric_literal )
    WITHIN GROUP ( ORDER BY order_by_expression [ ASC | DESC ] )
    OVER ( [ <partition_by_clause> ] )
```

## PERCENTILE_DISC (Transact-SQL)

`docs/t-sql/functions/percentile-disc-transact-sql.md`

### Syntax

```syntaxsql
PERCENTILE_DISC ( numeric_literal ) WITHIN GROUP ( ORDER BY order_by_expression [ ASC | DESC ] )
    OVER ( [ <partition_by_clause> ] )
```

## PERMISSIONS (Transact-SQL)

`docs/t-sql/functions/permissions-transact-sql.md`

### Syntax

```syntaxsql
PERMISSIONS ( [ objectid [ , 'column' ] ] )
```

## PI (Transact-SQL)

`docs/t-sql/functions/pi-transact-sql.md`

### Syntax

```syntaxsql
PI ( )
```

## POWER (Transact-SQL)

`docs/t-sql/functions/power-transact-sql.md`

### Syntax

```syntaxsql
POWER ( float_expression , y )
```

## @@PROCID (Transact-SQL)

`docs/t-sql/functions/procid-transact-sql.md`

### Syntax

```syntaxsql
@@PROCID
```

## PRODUCT (Transact-SQL)

`docs/t-sql/functions/product-aggregate-transact-sql.md`

### Syntax

> Aggregate function syntax:

```syntaxsql
PRODUCT ( [ ALL | DISTINCT ] expression )
```

> Analytic function syntax:

```syntaxsql
PRODUCT ( [ ALL ] expression) OVER ( [ partition_by_clause ] [ order_by_clause ] )
```

## PWDCOMPARE (Transact-SQL)

`docs/t-sql/functions/pwdcompare-transact-sql.md`

### Syntax

```syntaxsql
PWDCOMPARE ( 'clear_text_password'
   , password_hash
   [ , version ] )
```

## PWDENCRYPT (Transact-SQL)

`docs/t-sql/functions/pwdencrypt-transact-sql.md`

### Syntax

```syntaxsql
PWDENCRYPT ( 'password' )
```

## QUOTENAME (Transact-SQL)

`docs/t-sql/functions/quotename-transact-sql.md`

### Syntax

```syntaxsql
QUOTENAME ( 'character_string' [ , 'quote_character' ] )
```

## RADIANS (Transact-SQL)

`docs/t-sql/functions/radians-transact-sql.md`

### Syntax

```syntaxsql
RADIANS ( numeric_expression )
```

## RAND (Transact-SQL)

`docs/t-sql/functions/rand-transact-sql.md`

### Syntax

```syntaxsql
RAND ( [ seed ] )
```

## RANK (Transact-SQL)

`docs/t-sql/functions/rank-transact-sql.md`

### Syntax

```syntaxsql
RANK ( ) OVER ( [ partition_by_clause ] order_by_clause )
```

## REGEXP_COUNT (Transact-SQL)

`docs/t-sql/functions/regexp-count-transact-sql.md`

### REGEXP_COUNT (Transact-SQL)

> Counts the number of times that a regular expression pattern is matched in a string.

```syntaxsql
REGEXP_COUNT
(
    string_expression,
    pattern_expression [ , start [ , flags ] ]
)
```

## REGEXP_INSTR (Transact-SQL)

`docs/t-sql/functions/regexp-instr-transact-sql.md`

### REGEXP_INSTR (Transact-SQL)

> Returns the starting or ending position of the matched substring, depending on the value of the `return_option` argument.

```syntaxsql
REGEXP_INSTR
(
    string_expression,
    pattern_expression [ , start [ , occurrence [ , return_option [ , flags [ , group ] ] ] ] ]
)
```

## REGEXP_LIKE (Transact-SQL)

`docs/t-sql/functions/regexp-like-transact-sql.md`

### REGEXP_LIKE (Transact-SQL)

> Indicates if the regular expression pattern matches in a string.

```syntaxsql
REGEXP_LIKE
(
    string_expression,
    pattern_expression [ , flags ]
)
```

## REGEXP_MATCHES (Transact-SQL)

`docs/t-sql/functions/regexp-matches-transact-sql.md`

### REGEXP_MATCHES (Transact-SQL)

> Returns a table of captured substrings that match a regular expression pattern to a string. If no match is found, the function returns no row.

```syntaxsql
REGEXP_MATCHES
(
    string_expression,
    pattern_expression [ , flags ]
)
```

## REGEXP_REPLACE (Transact-SQL)

`docs/t-sql/functions/regexp-replace-transact-sql.md`

### REGEXP_REPLACE (Transact-SQL)

> Returns a modified source string replaced by a replacement string, where the occurrence of the regular expression pattern found. If no matches are found, the function returns the original string.

```syntaxsql
REGEXP_REPLACE
(
    string_expression,
    pattern_expression [ , string_replacement [ , start [ , occurrence [ , flags ] ] ] ]
)
```

## REGEXP_SPLIT_TO_TABLE (Transact-SQL)

`docs/t-sql/functions/regexp-split-to-table-transact-sql.md`

### REGEXP_SPLIT_TO_TABLE

> Returns a table of strings split, delimited by the regex pattern. If there's no match to the pattern, the function returns the string.

```syntaxsql
REGEXP_SPLIT_TO_TABLE
(
    string_expression,
    pattern_expression [ , flags ]
)
```

## REGEXP_SUBSTR (Transact-SQL)

`docs/t-sql/functions/regexp-substr-transact-sql.md`

### REGEXP_SUBSTR (Transact-SQL)

> Returns one occurrence of a substring of a string that matches the regular expression pattern. If no match is found, it returns `NULL`.

```syntaxsql
REGEXP_SUBSTR
(
    string_expression,
    pattern_expression [ , start [ , occurrence [ , flags [ , group ] ] ] ]
)
```

## @@REMSERVER (Transact-SQL)

`docs/t-sql/functions/remserver-transact-sql.md`

### Syntax

```syntaxsql
@@REMSERVER
```

## REPLACE (Transact-SQL)

`docs/t-sql/functions/replace-transact-sql.md`

### Syntax

```syntaxsql
REPLACE ( string_expression , string_pattern , string_replacement )
```

## REPLICATE (Transact-SQL)

`docs/t-sql/functions/replicate-transact-sql.md`

### Syntax

```syntaxsql
REPLICATE ( string_expression , integer_expression )
```

## PUBLISHINGSERVERNAME (Transact-SQL)

`docs/t-sql/functions/replication-functions-publishingservername.md`

### Syntax

```syntaxsql
PUBLISHINGSERVERNAME()
```

## REVERSE (Transact-SQL)

`docs/t-sql/functions/reverse-transact-sql.md`

### Syntax

```syntaxsql
REVERSE ( string_expression )
```

## RIGHT_SHIFT (Transact-SQL)

`docs/t-sql/functions/right-shift-transact-sql.md`

### Syntax

```syntaxsql
RIGHT_SHIFT ( expression_value , shift_amount )
expression_value >> shift_amount
```

## RIGHT (Transact-SQL)

`docs/t-sql/functions/right-transact-sql.md`

### Syntax

```syntaxsql
RIGHT ( character_expression , integer_expression )
```

## ROUND (Transact-SQL)

`docs/t-sql/functions/round-transact-sql.md`

### Syntax

```syntaxsql
ROUND ( numeric_expression , length [ , function ] )
```

## ROW_NUMBER (Transact-SQL)

`docs/t-sql/functions/row-number-transact-sql.md`

### Syntax

```syntaxsql
ROW_NUMBER ( )
    OVER ( [ PARTITION BY value_expression , ... [ n ] ] order_by_clause )
```

## ROWCOUNT_BIG (Transact-SQL)

`docs/t-sql/functions/rowcount-big-transact-sql.md`

### Syntax

```syntaxsql
ROWCOUNT_BIG ( )
```

## @@ROWCOUNT (Transact-SQL)

`docs/t-sql/functions/rowcount-transact-sql.md`

### Syntax

```syntaxsql
@@ROWCOUNT
```

## RTRIM (Transact-SQL)

`docs/t-sql/functions/rtrim-transact-sql.md`

### Syntax

> Syntax for SQL Server prior to SQL Server 2022 (16.x):

```syntaxsql
RTRIM ( character_expression )
```

> > The database compatibility level must be set to 160 or higher to use the optional *characters* argument.

```syntaxsql
RTRIM ( character_expression , [ characters ] )
```

## SCHEMA_ID (Transact-SQL)

`docs/t-sql/functions/schema-id-transact-sql.md`

### Syntax

```syntaxsql
SCHEMA_ID ( [ schema_name ] )
```

## SCHEMA_NAME (Transact-SQL)

`docs/t-sql/functions/schema-name-transact-sql.md`

### Syntax

```syntaxsql
SCHEMA_NAME ( [ schema_id ] )
```

## SCOPE_IDENTITY (Transact-SQL)

`docs/t-sql/functions/scope-identity-transact-sql.md`

### Syntax

```syntaxsql
SCOPE_IDENTITY()
```

## @@SERVERNAME (Transact-SQL)

`docs/t-sql/functions/servername-transact-sql.md`

### Syntax

```syntaxsql
@@SERVERNAME
```

## SERVERPROPERTY (Transact-SQL)

`docs/t-sql/functions/serverproperty-transact-sql.md`

### Syntax

```syntaxsql
SERVERPROPERTY ( 'propertyname' )
```

## @@SERVICENAME (Transact-SQL)

`docs/t-sql/functions/servicename-transact-sql.md`

### Syntax

```syntaxsql
@@SERVICENAME
```

## SESSION_CONTEXT (Transact-SQL)

`docs/t-sql/functions/session-context-transact-sql.md`

### Syntax

```syntaxsql
SESSION_CONTEXT(N'key')
```

## SESSION_ID (Transact-SQL)

`docs/t-sql/functions/session-id-transact-sql.md`

### Syntax

```syntaxsql
-- Azure Synapse Analytics and Parallel Data Warehouse
SESSION_ID ( )
```

## SESSION_USER (Transact-SQL)

`docs/t-sql/functions/session-user-transact-sql.md`

### Syntax

```syntaxsql
SESSION_USER
```

## SESSIONPROPERTY (Transact-SQL)

`docs/t-sql/functions/sessionproperty-transact-sql.md`

### Syntax

```syntaxsql
SESSIONPROPERTY (option)
```

## SET_BIT (Transact-SQL)

`docs/t-sql/functions/set-bit-transact-sql.md`

### Syntax

```syntaxsql
SET_BIT ( expression_value , bit_offset )
SET_BIT ( expression_value , bit_offset , bit_value )
```

## SIGN (Transact-SQL)

`docs/t-sql/functions/sign-transact-sql.md`

### Syntax

```syntaxsql
SIGN ( numeric_expression )
```

## SIGNBYASYMKEY (Transact-SQL)

`docs/t-sql/functions/signbyasymkey-transact-sql.md`

### Syntax

```syntaxsql
SignByAsymKey( Asym_Key_ID , @plaintext [ , 'password' ] )
```

## SIGNBYCERT (Transact-SQL)

`docs/t-sql/functions/signbycert-transact-sql.md`

### Syntax

```syntaxsql
SignByCert ( certificate_ID , @cleartext [ , 'password' ] )
```

## SIN (Transact-SQL)

`docs/t-sql/functions/sin-transact-sql.md`

### Syntax

```syntaxsql
SIN ( float_expression )
```

## SMALLDATETIMEFROMPARTS (Transact-SQL)

`docs/t-sql/functions/smalldatetimefromparts-transact-sql.md`

### Syntax

```syntaxsql
SMALLDATETIMEFROMPARTS ( year, month, day, hour, minute )
```

## SOUNDEX (Transact-SQL)

`docs/t-sql/functions/soundex-transact-sql.md`

### Syntax

```syntaxsql
SOUNDEX ( character_expression )
```

## SPACE (Transact-SQL)

`docs/t-sql/functions/space-transact-sql.md`

### Syntax

```syntaxsql
SPACE ( integer_expression )
```

## @@SPID (Transact-SQL)

`docs/t-sql/functions/spid-transact-sql.md`

### Syntax

```syntaxsql
@@SPID
```

## SQL_VARIANT_PROPERTY (Transact-SQL)

`docs/t-sql/functions/sql-variant-property-transact-sql.md`

### Syntax

```syntaxsql
SQL_VARIANT_PROPERTY ( expression , property )
```

## SQRT (Transact-SQL)

`docs/t-sql/functions/sqrt-transact-sql.md`

### Syntax

```syntaxsql
SQRT ( float_expression )
```

## SQUARE (Transact-SQL)

`docs/t-sql/functions/square-transact-sql.md`

### Syntax

```syntaxsql
SQUARE ( float_expression )
```

## STATS_DATE (Transact-SQL)

`docs/t-sql/functions/stats-date-transact-sql.md`

### Syntax

```syntaxsql
STATS_DATE ( object_id , stats_id )
```

## STDEV (Transact-SQL)

`docs/t-sql/functions/stdev-transact-sql.md`

### Syntax

```syntaxsql
-- Aggregate Function Syntax
STDEV ( [ ALL | DISTINCT ] expression )

-- Analytic Function Syntax
STDEV ([ ALL ] expression) OVER ( [ partition_by_clause ] order_by_clause)
```

## STDEVP (Transact-SQL)

`docs/t-sql/functions/stdevp-transact-sql.md`

### Syntax

```syntaxsql
-- Aggregate Function Syntax
STDEVP ( [ ALL | DISTINCT ] expression )

-- Analytic Function Syntax
STDEVP ([ ALL ] expression) OVER ( [ partition_by_clause ] order_by_clause)
```

## STR (Transact-SQL)

`docs/t-sql/functions/str-transact-sql.md`

### Syntax

```syntaxsql
STR ( float_expression [ , length [ , decimal ] ] )
```

## STRING_AGG (Transact-SQL)

`docs/t-sql/functions/string-agg-transact-sql.md`

### Syntax

```syntaxsql
STRING_AGG ( expression , separator ) [ <order_clause> ]

<order_clause> ::=
    WITHIN GROUP ( ORDER BY <order_by_expression_list> [ ASC | DESC ] )
```

### <order_clause>

> Optionally specify order of concatenated results using `WITHIN GROUP` clause:

```syntaxsql
WITHIN GROUP ( ORDER BY <order_by_expression_list> [ ASC | DESC ] )
```

## STRING_ESCAPE (Transact-SQL)

`docs/t-sql/functions/string-escape-transact-sql.md`

### Syntax

```syntaxsql
STRING_ESCAPE( text , type )
```

## STRING_SPLIT (Transact-SQL)

`docs/t-sql/functions/string-split-transact-sql.md`

### Syntax

```syntaxsql
STRING_SPLIT ( string , separator [ , enable_ordinal ] )
```

## STUFF (Transact-SQL)

`docs/t-sql/functions/stuff-transact-sql.md`

### Syntax

```syntaxsql
STUFF ( character_expression , start , length , replace_with_expression )
```

## SUBSTRING (Transact-SQL)

`docs/t-sql/functions/substring-transact-sql.md`

### Syntax

Marked for `<=sql-server-ver16 || <=sql-server-linux-ver16`.

> Syntax for SQL Server 2022 (16.x) and earlier versions.

```syntaxsql
SUBSTRING ( expression , start , length )
```

Marked for `>=aps-pdw-2016 || =azuresqldb-current || =azure-sqldw-latest || >=sql-server-ver17 || >=sql-server-linux-ver17 || =azuresqldb-mi-current || =fabric || =fabric-sqldb`.

> Syntax for SQL Server 2025 (17.x) and later versions, Azure SQL Database, Azure SQL Managed Instance, Azure Synapse Analytics, Analytics Platform System (PDW), and Warehouse and SQL analytics endpoint in Microsoft Fabric.

```syntaxsql
SUBSTRING ( expression , start [ , length ] )
```

## SUM (Transact-SQL)

`docs/t-sql/functions/sum-transact-sql.md`

### Syntax

```syntaxsql
-- Aggregate Function Syntax
SUM ( [ ALL | DISTINCT ] expression )

-- Analytic Function Syntax
SUM ( [ ALL ] expression) OVER ( [ partition_by_clause ] [ order_by_clause ] )
```

## SUSER_ID (Transact-SQL)

`docs/t-sql/functions/suser-id-transact-sql.md`

### Syntax

```syntaxsql
SUSER_ID ( [ 'login' ] )
```

## SUSER_NAME (Transact-SQL)

`docs/t-sql/functions/suser-name-transact-sql.md`

### Syntax

```syntaxsql
SUSER_NAME ( [ server_user_id ] )
```

## SUSER_SID (Transact-SQL)

`docs/t-sql/functions/suser-sid-transact-sql.md`

### Syntax

```syntaxsql
SUSER_SID ( [ 'login' ] [ , Param2 ] )
```

## SUSER_SNAME (Transact-SQL)

`docs/t-sql/functions/suser-sname-transact-sql.md`

### Syntax

```syntaxsql
SUSER_SNAME ( [ server_user_sid ] )
```

## SWITCHOFFSET (Transact-SQL)

`docs/t-sql/functions/switchoffset-transact-sql.md`

### Syntax

```syntaxsql
SWITCHOFFSET ( datetimeoffset_expression , timezoneoffset_expression )
```

## SYMKEYPROPERTY (Transact-SQL)

`docs/t-sql/functions/symkeyproperty-transact-sql.md`

### Syntax

```syntaxsql
SYMKEYPROPERTY ( Key_ID , 'algorithm_desc' | 'string_sid' | 'sid' )
```

## SYSDATETIME (Transact-SQL)

`docs/t-sql/functions/sysdatetime-transact-sql.md`

### Syntax

```syntaxsql
SYSDATETIME ( )
```

## SYSDATETIMEOFFSET (Transact-SQL)

`docs/t-sql/functions/sysdatetimeoffset-transact-sql.md`

### Syntax

```syntaxsql
SYSDATETIMEOFFSET ( )
```

## SYSTEM_USER (Transact-SQL)

`docs/t-sql/functions/system-user-transact-sql.md`

### Syntax

```syntaxsql
SYSTEM_USER
```

## SYSUTCDATETIME (Transact-SQL)

`docs/t-sql/functions/sysutcdatetime-transact-sql.md`

### Syntax

```syntaxsql
SYSUTCDATETIME ( )
```

## TAN (Transact-SQL)

`docs/t-sql/functions/tan-transact-sql.md`

### Syntax

```syntaxsql
TAN ( float_expression )
```

## TEXTPTR (Transact-SQL)

`docs/t-sql/functions/text-and-image-functions-textptr-transact-sql.md`

### Syntax

```syntaxsql
TEXTPTR ( column )
```

## TEXTVALID (Transact-SQL)

`docs/t-sql/functions/text-and-image-functions-textvalid-transact-sql.md`

### Syntax

```syntaxsql
TEXTVALID ( 'table.column' ,text_ ptr )
```

## @@TEXTSIZE (Transact-SQL)

`docs/t-sql/functions/textsize-transact-sql.md`

### Syntax

```syntaxsql
@@TEXTSIZE
```

## TIMEFROMPARTS (Transact-SQL)

`docs/t-sql/functions/timefromparts-transact-sql.md`

### Syntax

```syntaxsql
TIMEFROMPARTS ( hour, minute, seconds, fractions, precision )
```

## @@TIMETICKS (Transact-SQL)

`docs/t-sql/functions/timeticks-transact-sql.md`

### Syntax

```syntaxsql
@@TIMETICKS
```

## TODATETIMEOFFSET (Transact-SQL)

`docs/t-sql/functions/todatetimeoffset-transact-sql.md`

### Syntax

```syntaxsql
TODATETIMEOFFSET ( datetime_expression , timezoneoffset_expression )
```

## @@TOTAL_ERRORS (Transact-SQL)

`docs/t-sql/functions/total-errors-transact-sql.md`

### Syntax

```syntaxsql
@@TOTAL_ERRORS
```

## @@TOTAL_READ (Transact-SQL)

`docs/t-sql/functions/total-read-transact-sql.md`

### Syntax

```syntaxsql
@@TOTAL_READ
```

## @@TOTAL_WRITE (Transact-SQL)

`docs/t-sql/functions/total-write-transact-sql.md`

### Syntax

```syntaxsql
@@TOTAL_WRITE
```

## @@TRANCOUNT (Transact-SQL)

`docs/t-sql/functions/trancount-transact-sql.md`

### Syntax

```syntaxsql
@@TRANCOUNT
```

## TRANSLATE (Transact-SQL)

`docs/t-sql/functions/translate-transact-sql.md`

### Syntax

```syntaxsql
TRANSLATE ( inputString, characters, translations )
```

## TRIGGER_NESTLEVEL (Transact-SQL)

`docs/t-sql/functions/trigger-nestlevel-transact-sql.md`

### Syntax

```syntaxsql
TRIGGER_NESTLEVEL ( [ object_id ] , [ 'trigger_type' ] , [ 'trigger_event_category' ] )
```

## TRIM (Transact-SQL)

`docs/t-sql/functions/trim-transact-sql.md`

### Syntax

> Syntax for SQL Server 2019 (15.x) and earlier versions, and Azure Synapse Analytics:

```syntaxsql
TRIM ( [ characters FROM ] string )
```

> > You need your database compatibility level set to `160` to use the `LEADING`, `TRAILING`, or `BOTH` keywords.

```syntaxsql
TRIM ( [ LEADING | TRAILING | BOTH ] [characters FROM ] string )
```

## TRY_CAST (Transact-SQL)

`docs/t-sql/functions/try-cast-transact-sql.md`

### Syntax

```syntaxsql
TRY_CAST ( expression AS data_type [ ( length ) ] )
```

## TRY_CONVERT (Transact-SQL)

`docs/t-sql/functions/try-convert-transact-sql.md`

### Syntax

```syntaxsql
TRY_CONVERT ( data_type [ ( length ) ] , expression [ , style ] )
```

## TRY_PARSE (Transact-SQL)

`docs/t-sql/functions/try-parse-transact-sql.md`

### Syntax

```syntaxsql
TRY_PARSE ( string_value AS data_type [ USING culture ] )
```

## TYPE_ID (Transact-SQL)

`docs/t-sql/functions/type-id-transact-sql.md`

### Syntax

```syntaxsql
TYPE_ID ( [ schema_name ] type_name )
```

## TYPE_NAME (Transact-SQL)

`docs/t-sql/functions/type-name-transact-sql.md`

### Syntax

```syntaxsql
TYPE_NAME ( type_id )
```

## TYPEPROPERTY (Transact-SQL)

`docs/t-sql/functions/typeproperty-transact-sql.md`

### Syntax

```syntaxsql
TYPEPROPERTY (type , property)
```

## UNICODE (Transact-SQL)

`docs/t-sql/functions/unicode-transact-sql.md`

### Syntax

```syntaxsql
UNICODE ( 'ncharacter_expression' )
```

## UNISTR (Transact-SQL)

`docs/t-sql/functions/unistr-transact-sql.md`

### Syntax

```syntaxsql
UNISTR ( 'character_expression' [ , 'unicode_escape_character' ] )
```

## UPDATE() (Transact-SQL)

`docs/t-sql/functions/update-trigger-functions-transact-sql.md`

### Syntax

```syntaxsql
UPDATE ( column )
```

## UPPER (Transact-SQL)

`docs/t-sql/functions/upper-transact-sql.md`

### Syntax

```syntaxsql
UPPER ( character_expression )
```

## USER_ID (Transact-SQL)

`docs/t-sql/functions/user-id-transact-sql.md`

### Syntax

```syntaxsql
USER_ID ( [ 'user' ] )
```

## USER_NAME (Transact-SQL)

`docs/t-sql/functions/user-name-transact-sql.md`

### Syntax

```syntaxsql
USER_NAME ( [ ID ] )
```

## USER (Transact-SQL)

`docs/t-sql/functions/user-transact-sql.md`

### Syntax

```syntaxsql
USER
```

## VAR (Transact-SQL)

`docs/t-sql/functions/var-transact-sql.md`

### Syntax

```syntaxsql
-- Aggregate Function Syntax
VAR ( [ ALL | DISTINCT ] expression )

-- Analytic Function Syntax
VAR ([ ALL ] expression) OVER ( [ partition_by_clause ] order_by_clause)
```

## VARP (Transact-SQL)

`docs/t-sql/functions/varp-transact-sql.md`

### Syntax

```syntaxsql
-- Aggregate Function Syntax
VARP ( [ ALL | DISTINCT ] expression )

-- Analytic Function Syntax
VARP ([ ALL ] expression) OVER ( [ partition_by_clause ] order_by_clause)
```

## VECTOR_DISTANCE (Transact-SQL)

`docs/t-sql/functions/vector-distance-transact-sql.md`

### Syntax

```syntaxsql
VECTOR_DISTANCE ( distance_metric , vector1 , vector2 )
```

## VECTOR_NORM (Transact-SQL)

`docs/t-sql/functions/vector-norm-transact-sql.md`

### Syntax

```syntaxsql
VECTOR_NORM ( vector , norm_type )
```

## VECTOR_NORMALIZE (Transact-SQL)

`docs/t-sql/functions/vector-normalize-transact-sql.md`

### Syntax

```syntaxsql
VECTOR_NORMALIZE ( vector , norm_type )
```

## VECTOR_SEARCH (Transact-SQL)

`docs/t-sql/functions/vector-search-transact-sql.md`

### Syntax

> > When querying tables that use the latest vector index version, approximate vector search must use the TOP (N) APPROXIMATE syntax. This syntax requirement indicates that the query is explicitly requesting approximate nearest-neighbor results.

```syntaxsql
SELECT TOP (N) WITH APPROXIMATE
    column_list
FROM VECTOR_SEARCH(
        TABLE = object [ AS source_table_alias ]
        , COLUMN = vector_column
        , SIMILAR_TO = query_vector
        , METRIC = { 'cosine' | 'dot' | 'euclidean' }
    ) [ AS result_table_alias ]
[ WHERE predicate ]
ORDER BY distance;
```

> **With earlier version Vector Indexes:**

```syntaxsql
VECTOR_SEARCH(
    TABLE = object [ AS source_table_alias ]
    , COLUMN = vector_column
    , SIMILAR_TO = query_vector
    , METRIC = { 'cosine' | 'dot' | 'euclidean' }
    , TOP_N = k
) [ AS result_table_alias ]
```

## VECTORPROPERTY (Transact-SQL)

`docs/t-sql/functions/vectorproperty-transact-sql.md`

### Syntax

```syntaxsql
VECTORPROPERTY(vector , property)
```

## VERIFYSIGNEDBYASYMKEY (Transact-SQL)

`docs/t-sql/functions/verifysignedbyasymkey-transact-sql.md`

### Syntax

```syntaxsql
VerifySignedByAsymKey( Asym_Key_ID , clear_text , signature )
```

## VERIFYSIGNEDBYCERT (Transact-SQL)

`docs/t-sql/functions/verifysignedbycert-transact-sql.md`

### Syntax

```syntaxsql
VerifySignedByCert( Cert_ID , signed_data , signature )
```

## @@VERSION (Transact-SQL)

`docs/t-sql/functions/version-transact-sql-configuration-functions.md`

### Syntax

```syntaxsql
@@VERSION
```

## VERSION (Transact-SQL)

`docs/t-sql/functions/version-transact-sql-metadata-functions.md`

### Syntax

```syntaxsql
-- Azure Synapse Analytics and Parallel Data Warehouse
VERSION ( )
```

## XACT_STATE (Transact-SQL)

`docs/t-sql/functions/xact-state-transact-sql.md`

### Syntax

```syntaxsql
XACT_STATE()
```

## YEAR (Transact-SQL)

`docs/t-sql/functions/year-transact-sql.md`

### Syntax

```syntaxsql
YEAR ( date )
```

## docs/t-sql/includes/alter-workload-group.md

`docs/t-sql/includes/alter-workload-group.md`

### Syntax

```syntaxsql
ALTER WORKLOAD GROUP { group_name | [ default ] }
[ WITH
    ( [ IMPORTANCE = { LOW | MEDIUM | HIGH } ]
      [ [ , ] REQUEST_MAX_MEMORY_GRANT_PERCENT = value ]
      [ [ , ] REQUEST_MAX_CPU_TIME_SEC = value ]
      [ [ , ] REQUEST_MEMORY_GRANT_TIMEOUT_SEC = value ]
      [ [ , ] MAX_DOP = value ]
      [ [ , ] GROUP_MAX_REQUESTS = value ]
      [ [ , ] GROUP_MAX_TEMPDB_DATA_MB = value ]
      [ [ , ] GROUP_MAX_TEMPDB_DATA_PERCENT = value ] )
]
[ USING { pool_name | [default] } ]
[ ; ]
```

## docs/t-sql/includes/create-workload-group.md

`docs/t-sql/includes/create-workload-group.md`

### Syntax

```syntaxsql
CREATE WORKLOAD GROUP group_name
[ WITH
    ( [ IMPORTANCE = { LOW | MEDIUM | HIGH } ]
      [ [ , ] REQUEST_MAX_MEMORY_GRANT_PERCENT = value ]
      [ [ , ] REQUEST_MAX_CPU_TIME_SEC = value ]
      [ [ , ] REQUEST_MEMORY_GRANT_TIMEOUT_SEC = value ]
      [ [ , ] MAX_DOP = value ]
      [ [ , ] GROUP_MAX_REQUESTS = value ]
      [ [ , ] GROUP_MAX_TEMPDB_DATA_MB = value ]
      [ [ , ] GROUP_MAX_TEMPDB_DATA_PERCENT = value ] )
]
[ USING {
    [ pool_name | [default] ]
    [ [ , ] EXTERNAL external_pool_name | [ default ] ]
    } ]
[ ; ]
```

## docs/t-sql/includes/drop-workload-group.md

`docs/t-sql/includes/drop-workload-group.md`

### Syntax

```syntaxsql
DROP WORKLOAD GROUP group_name
[;]
```

## += (Addition Assignment) (Transact-SQL)

`docs/t-sql/language-elements/add-equals-transact-sql.md`

### Syntax

```syntaxsql
expression += expression
```

## + (Addition) (Transact-SQL)

`docs/t-sql/language-elements/add-transact-sql.md`

### Syntax

```syntaxsql
expression + expression
```

## ALL (Transact-SQL)

`docs/t-sql/language-elements/all-transact-sql.md`

### Syntax

```syntaxsql
scalar_expression { = | <> | != | > | >= | !> | < | <= | !< } ALL ( subquery )
```

## AND (Transact-SQL)

`docs/t-sql/language-elements/and-transact-sql.md`

### Syntax

```syntaxsql
boolean_expression AND boolean_expression
```

## BEGIN DISTRIBUTED TRANSACTION (Transact-SQL)

`docs/t-sql/language-elements/begin-distributed-transaction-transact-sql.md`

### Syntax

```syntaxsql
BEGIN DISTRIBUTED { TRAN | TRANSACTION }
     [ transaction_name | @tran_name_variable ]
[ ; ]
```

## BEGIN...END (Transact-SQL)

`docs/t-sql/language-elements/begin-end-transact-sql.md`

### Syntax

```syntaxsql
BEGIN [ ; ]
    { sql_statement | statement_block }
END [ ; ]
```

## BEGIN TRANSACTION (Transact-SQL)

`docs/t-sql/language-elements/begin-transaction-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, Azure SQL Managed Instance, SQL database in Microsoft Fabric.

```syntaxsql
BEGIN { TRAN | TRANSACTION }
    [ { transaction_name | @tran_name_variable }
      [ WITH MARK [ 'description' ] ]
    ]
[ ; ]
```

> Syntax for Fabric Data Warehouse, Azure Synapse Analytics, and Analytics Platform System (PDW).

```syntaxsql
BEGIN { TRAN | TRANSACTION }
[ ; ]
```

## BETWEEN (Transact-SQL)

`docs/t-sql/language-elements/between-transact-sql.md`

### Syntax

```syntaxsql
test_expression [ NOT ] BETWEEN begin_expression AND end_expression
```

## &amp;= (Bitwise AND Assignment) (Transact-SQL)

`docs/t-sql/language-elements/bitwise-and-equals-transact-sql.md`

### Syntax

```syntaxsql
expression &= expression
```

## &amp; (Bitwise AND) (Transact-SQL)

`docs/t-sql/language-elements/bitwise-and-transact-sql.md`

### Syntax

```syntaxsql
expression & expression
```

## ^= (Bitwise Exclusive OR Assignment) (Transact-SQL)

`docs/t-sql/language-elements/bitwise-exclusive-or-equals-transact-sql.md`

### Syntax

```syntaxsql
expression ^= expression
```

## ^ (Bitwise Exclusive OR) (Transact-SQL)

`docs/t-sql/language-elements/bitwise-exclusive-or-transact-sql.md`

### Syntax

```syntaxsql
expression ^ expression
```

## ~ (Bitwise NOT) (Transact-SQL)

`docs/t-sql/language-elements/bitwise-not-transact-sql.md`

### Syntax

```syntaxsql
~ expression
```

## |= (Bitwise OR Assignment) (Transact-SQL)

`docs/t-sql/language-elements/bitwise-or-equals-transact-sql.md`

### Syntax

```syntaxsql
expression |= expression
```

## | (Bitwise OR) (Transact-SQL)

`docs/t-sql/language-elements/bitwise-or-transact-sql.md`

### Syntax

```syntaxsql
expression | expression
```

## CASE (Transact-SQL)

`docs/t-sql/language-elements/case-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, and Azure Synapse Analytics.

```syntaxsql
-- Simple CASE expression:
CASE input_expression
     WHEN when_expression THEN result_expression [ ...n ]
     [ ELSE else_result_expression ]
END

-- Searched CASE expression:
CASE
     WHEN Boolean_expression THEN result_expression [ ...n ]
     [ ELSE else_result_expression ]
END
```

> Syntax for Parallel Data Warehouse.

```syntaxsql
CASE
     WHEN when_expression THEN result_expression [ ...n ]
     [ ELSE else_result_expression ]
END
```

## CHECKPOINT (Transact-SQL)

`docs/t-sql/language-elements/checkpoint-transact-sql.md`

### Syntax

```syntaxsql
CHECKPOINT [ checkpoint_duration ]
```

## CLOSE (Transact-SQL)

`docs/t-sql/language-elements/close-transact-sql.md`

### Syntax

```syntaxsql
CLOSE { { [ GLOBAL ] cursor_name } | cursor_variable_name }
```

## COALESCE (Transact-SQL)

`docs/t-sql/language-elements/coalesce-transact-sql.md`

### Syntax

```syntaxsql
COALESCE ( expression [ , ...n ] )
```

## -- (Comment) (Transact-SQL)

`docs/t-sql/language-elements/comment-transact-sql.md`

### Syntax

```syntaxsql
-- text_of_comment
```

## COMMIT TRANSACTION (Transact-SQL)

`docs/t-sql/language-elements/commit-transaction-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, Azure SQL Managed Instance, SQL database in Microsoft Fabric.

```syntaxsql
COMMIT [ { TRAN | TRANSACTION }
    [ transaction_name | @tran_name_variable ] ]
    [ WITH ( DELAYED_DURABILITY = { OFF | ON } ) ]
[ ; ]
```

> Syntax for Fabric Data Warehouse, Azure Synapse Analytics, and Parallel Data Warehouse Database.

```syntaxsql
COMMIT [ TRAN | TRANSACTION ]
[ ; ]
```

## COMMIT WORK (Transact-SQL)

`docs/t-sql/language-elements/commit-work-transact-sql.md`

### Syntax

```syntaxsql
COMMIT [ WORK ]
[ ; ]
```

## Comparison Operators (Transact-SQL)

`docs/t-sql/language-elements/comparison-operators-transact-sql.md`

### Boolean Data Type

> Expressions with **Boolean** data types are used in the WHERE clause to filter the rows that qualify for the search conditions and in control-of-flow language statements such as IF and WHILE, for example:

```syntaxsql
-- Uses AdventureWorks

DECLARE @MyProduct INT;
SET @MyProduct = 750;
IF (@MyProduct <> 0)
   SELECT ProductID, Name, ProductNumber
   FROM Production.Product
   WHERE ProductID = @MyProduct;
```

## ||= (Compound assignment) (Transact-SQL)

`docs/t-sql/language-elements/compound-assignment-pipes-transact-sql.md`

### Syntax

```syntaxsql
variable ||= expression
```

## Compound Operators (Transact-SQL)

`docs/t-sql/language-elements/compound-operators-transact-sql.md`

### Syntax

```syntaxsql
expression <operator> expression
```

## CREATE DIAGNOSTICS SESSION (Transact-SQL)

`docs/t-sql/language-elements/create-diagnostics-session-transact-sql.md`

### Syntax

```syntaxsql
-- Creating a new diagnostics session:
CREATE DIAGNOSTICS SESSION diagnostics_name AS N' { <session_xml> } ';

<session_xml>::
<Session>
   [ <MaxItemCount>max_item_count_num</MaxItemCount> ]
   [ <Filter>
      { <Event Name = "event_name"/>
         [ <Where><filter_property_name Name = "value" ComparisonType = "comp_type"/></Where> ] [ , ...n ]
      } [ , ...n ]
   </Filter> ]
   <Capture>
      <Property Name = "property_name"/> [ , ...n ]
   </Capture>
<Session>

-- Retrieving results for a diagnostics session:
SELECT * FROM master.sysdiag.diagnostics_name ;

-- Removing results for a diagnostics session:
DROP DIAGNOSTICS SESSION diagnostics_name ;
```

## DEALLOCATE (Transact-SQL)

`docs/t-sql/language-elements/deallocate-transact-sql.md`

### Syntax

```syntaxsql
DEALLOCATE { { [ GLOBAL ] cursor_name } | @cursor_variable_name }
```

## DECLARE CURSOR (Transact-SQL)

`docs/t-sql/language-elements/declare-cursor-transact-sql.md`

### Syntax

> ISO syntax:

```syntaxsql
DECLARE cursor_name [ INSENSITIVE ] [ SCROLL ] CURSOR
    FOR select_statement
    [ FOR { READ_ONLY | UPDATE [ OF column_name [ , ...n ] ] } ]
[ ; ]
```

> Transact-SQL extended syntax:

```syntaxsql
DECLARE cursor_name CURSOR [ LOCAL | GLOBAL ]
    [ FORWARD_ONLY | SCROLL ]
    [ STATIC | KEYSET | DYNAMIC | FAST_FORWARD ]
    [ READ_ONLY | SCROLL_LOCKS | OPTIMISTIC ]
    [ TYPE_WARNING ]
    FOR select_statement
    [ FOR UPDATE [ OF column_name [ , ...n ] ] ]
[ ; ]
```

## DECLARE @local_variable (Transact-SQL)

`docs/t-sql/language-elements/declare-local-variable-transact-sql.md`

### Syntax

> The following syntax is for SQL Server and Azure SQL Database:

```syntaxsql
DECLARE
{
  { @local_variable [AS] data_type [ = value ] }
  | { @cursor_variable_name CURSOR }
| { @table_variable_name [AS] <table_type_definition> }
} [ , ...n ]

<table_type_definition> ::=
    TABLE ( { <column_definition> | <table_constraint> | <table_index> } } [ , ...n ] )

<column_definition> ::=
    column_name { scalar_data_type | AS computed_column_expression }
    [ COLLATE collation_name ]
    [ [ DEFAULT constant_expression ] | IDENTITY [ (seed, increment ) ] ]
    [ ROWGUIDCOL ]
    [ <column_constraint> ]
    [ <column_index> ]

<column_constraint> ::=
{
    [ NULL | NOT NULL ]
    { PRIMARY KEY | UNIQUE }
      [ CLUSTERED | NONCLUSTERED ]
      [ WITH FILLFACTOR = fillfactor
        | WITH ( < index_option > [ , ...n ] )
      [ ON { filegroup | "default" } ]
  | [ CHECK ( logical_expression ) ] [ , ...n ]
}

<column_index> ::=
    INDEX index_name [ CLUSTERED | NONCLUSTERED ]
    [ WITH ( <index_option> [ , ... n ] ) ]
    [ ON { partition_scheme_name (column_name )
         | filegroup_name
         | default
         }
    ]
    [ FILESTREAM_ON { filestream_filegroup_name | partition_scheme_name | "NULL" } ]

<table_constraint> ::=
{
    { PRIMARY KEY | UNIQUE }
      [ CLUSTERED | NONCLUSTERED ]
      ( column_name [ ASC | DESC ] [ , ...n ]
        [ WITH FILLFACTOR = fillfactor
        | WITH ( <index_option> [ , ...n ] )
  | [ CHECK ( logical_expression ) ] [ , ...n ]
}

<table_index> ::=
{
    {
      INDEX index_name  [ UNIQUE ] [ CLUSTERED | NONCLUSTERED ]
         (column_name [ ASC | DESC ] [ , ... n ] )
    | INDEX index_name CLUSTERED COLUMNSTORE
    | INDEX index_name [ NONCLUSTERED ] COLUMNSTORE ( column_name [ , ... n ] )
    }
    [ WITH ( <index_option> [ , ... n ] ) ]
    [ ON { partition_scheme_name ( column_name )
         | filegroup_name
         | default
         }
    ]
    [ FILESTREAM_ON { filestream_filegroup_name | partition_scheme_name | "NULL" } ]
}

<index_option> ::=
{
  PAD_INDEX = { ON | OFF }
  | FILLFACTOR = fillfactor
  | IGNORE_DUP_KEY = { ON | OFF }
  | STATISTICS_NORECOMPUTE = { ON | OFF }
  | STATISTICS_INCREMENTAL = { ON | OFF }
  | ALLOW_ROW_LOCKS = { ON | OFF }
  | ALLOW_PAGE_LOCKS = { ON | OFF }
  | OPTIMIZE_FOR_SEQUENTIAL_KEY = { ON | OFF }
  | COMPRESSION_DELAY = { 0 | delay [ Minutes ] }
  | DATA_COMPRESSION = { NONE | ROW | PAGE | COLUMNSTORE | COLUMNSTORE_ARCHIVE }
       [ ON PARTITIONS ( { partition_number_expression | <range> }
       [ , ...n ] ) ]
  | XML_COMPRESSION = { ON | OFF }
      [ ON PARTITIONS ( { <partition_number_expression> | <range> }
      [ , ...n ] ) ] ]
}
```

> The following syntax is for Azure Synapse Analytics and Parallel Data Warehouse and Microsoft Fabric:

```syntaxsql
DECLARE
{ { @local_variable [AS] data_type } [ = value [ COLLATE <collation_name> ] ] } [ , ...n ]
```

## /= (Division Assignment) (Transact-SQL)

`docs/t-sql/language-elements/divide-equals-transact-sql.md`

### Syntax

```syntaxsql
expression /= expression
```

## (Division) (Transact-SQL)

`docs/t-sql/language-elements/divide-transact-sql.md`

### Syntax

```syntaxsql
dividend / divisor
```

## ELSE (IF...ELSE) (Transact-SQL)

`docs/t-sql/language-elements/else-if-else-transact-sql.md`

### Syntax

```syntaxsql
IF boolean_expression
    { sql_statement | statement_block }
[ ELSE
    { sql_statement | statement_block } ]
```

## END (BEGIN...END) (Transact-SQL)

`docs/t-sql/language-elements/end-begin-end-transact-sql.md`

### Syntax

```syntaxsql
BEGIN
     { sql_statement | statement_block }
END
```

## = (Equals) (Transact-SQL)

`docs/t-sql/language-elements/equals-transact-sql.md`

### Syntax

```syntaxsql
expression = expression
```

## EXECUTE (Transact-SQL)

`docs/t-sql/language-elements/execute-transact-sql.md`

### Syntax

Marked for `>=sql-server-ver15 || >=sql-server-linux-ver15`.

> Syntax for SQL Server 2019 and later versions.

```syntaxsql
-- Execute a stored procedure or function
[ { EXEC | EXECUTE } ]
    {
      [ @return_status = ]
      { module_name [ ;number ] | @module_name_var }
        [ [ @parameter = ] { value
                           | @variable [ OUTPUT ]
                           | [ DEFAULT ]
                           }
        ]
      [ ,...n ]
      [ WITH <execute_option> [ ,...n ] ]
    }
[ ; ]

-- Execute a character string
{ EXEC | EXECUTE }
    ( { @string_variable | [ N ]'tsql_string' } [ + ...n ] )
    [ AS { LOGIN | USER } = ' name ' ]
[ ; ]

-- Execute a pass-through command against a linked server
{ EXEC | EXECUTE }
    ( { @string_variable | [ N ] 'command_string [ ? ]' } [ + ...n ]
        [ { , { value | @variable [ OUTPUT ] } } [ ...n ] ]
    )
    [ AS { LOGIN | USER } = ' name ' ]
    [ AT linked_server_name ]
    [ AT DATA_SOURCE data_source_name ]
[ ; ]

<execute_option>::=
{
        RECOMPILE
    | { RESULT SETS UNDEFINED }
    | { RESULT SETS NONE }
    | { RESULT SETS ( <result_sets_definition> [,...n ] ) }
}

<result_sets_definition> ::=
{
    (
         { column_name
           data_type
         [ COLLATE collation_name ]
         [ NULL | NOT NULL ] }
         [,...n ]
    )
    | AS OBJECT
        [ db_name . [ schema_name ] . | schema_name . ]
        {table_name | view_name | table_valued_function_name }
    | AS TYPE [ schema_name.]table_type_name
    | AS FOR XML
}
```

Marked for `=sql-server-2017 || =sql-server-linux-2017`.

> Syntax for SQL Server 2017 and earlier versions.

```syntaxsql
-- Execute a stored procedure or function
[ { EXEC | EXECUTE } ]
    {
      [ @return_status = ]
      { module_name [ ;number ] | @module_name_var }
        [ [ @parameter = ] { value
                           | @variable [ OUTPUT ]
                           | [ DEFAULT ]
                           }
        ]
      [ ,...n ]
      [ WITH <execute_option> [ ,...n ] ]
    }
[ ; ]

-- Execute a character string
{ EXEC | EXECUTE }
    ( { @string_variable | [ N ]'tsql_string' } [ + ...n ] )
    [ AS { LOGIN | USER } = ' name ' ]
[ ; ]

-- Execute a pass-through command against a linked server
{ EXEC | EXECUTE }
    ( { @string_variable | [ N ] 'command_string [ ? ]' } [ + ...n ]
        [ { , { value | @variable [ OUTPUT ] } } [ ...n ] ]
    )
    [ AS { LOGIN | USER } = ' name ' ]
    [ AT linked_server_name ]
[ ; ]

<execute_option>::=
{
        RECOMPILE
    | { RESULT SETS UNDEFINED }
    | { RESULT SETS NONE }
    | { RESULT SETS ( <result_sets_definition> [,...n ] ) }
}

<result_sets_definition> ::=
{
    (
         { column_name
           data_type
         [ COLLATE collation_name ]
         [ NULL | NOT NULL ] }
         [,...n ]
    )
    | AS OBJECT
        [ db_name . [ schema_name ] . | schema_name . ]
        {table_name | view_name | table_valued_function_name }
    | AS TYPE [ schema_name.]table_type_name
    | AS FOR XML
}
```

> Syntax for In-Memory OLTP.

```syntaxsql
-- Execute a natively compiled, scalar user-defined function
[ { EXEC | EXECUTE } ]
    {
      [ @return_status = ]
      { module_name | @module_name_var }
        [ [ @parameter = ] { value
                           | @variable
                           | [ DEFAULT ]
                           }
        ]
      [ ,...n ]
      [ WITH <execute_option> [ ,...n ] ]
    }
<execute_option>::=
{
    | { RESULT SETS UNDEFINED }
    | { RESULT SETS NONE }
    | { RESULT SETS ( <result_sets_definition> [,...n ] ) }
}
```

> Syntax for Azure SQL Database.

```syntaxsql
-- Execute a stored procedure or function
[ { EXEC | EXECUTE } ]
    {
      [ @return_status = ]
      { module_name  | @module_name_var }
        [ [ @parameter = ] { value
                           | @variable [ OUTPUT ]
                           | [ DEFAULT ]
                           }
        ]
      [ ,...n ]
      [ WITH RECOMPILE ]
    }
[ ; ]

-- Execute a character string
{ EXEC | EXECUTE }
    ( { @string_variable | [ N ]'tsql_string' } [ + ...n ] )
    [ AS {  USER } = ' name ' ]
[ ; ]

<execute_option>::=
{
        RECOMPILE
    | { RESULT SETS UNDEFINED }
    | { RESULT SETS NONE }
    | { RESULT SETS ( <result_sets_definition> [,...n ] ) }
}

<result_sets_definition> ::=
{
    (
         { column_name
           data_type
         [ COLLATE collation_name ]
         [ NULL | NOT NULL ] }
         [,...n ]
    )
    | AS OBJECT
        [ db_name . [ schema_name ] . | schema_name . ]
        {table_name | view_name | table_valued_function_name }
    | AS TYPE [ schema_name.]table_type_name
    | AS FOR XML
}
```

> Syntax for Azure Synapse Analytics and Parallel Data Warehouse.

```syntaxsql
-- Execute a stored procedure
[ { EXEC | EXECUTE } ]
    procedure_name
        [ { value | @variable [ OUT | OUTPUT ] } ] [ ,...n ]
[ ; ]

-- Execute a SQL string
{ EXEC | EXECUTE }
    ( { @string_variable | [ N ] 'tsql_string' } [ +...n ] )
[ ; ]
```

> Syntax for Microsoft Fabric.

```syntaxsql
-- Execute a stored procedure
[ { EXEC | EXECUTE } ]
    procedure_name
        [ { value | @variable [ OUT | OUTPUT ] } ] [ ,...n ]
        [ WITH <execute_option> [ ,...n ] ]  }
[ ; ]

-- Execute a SQL string
{ EXEC | EXECUTE }
    ( { @string_variable | [ N ] 'tsql_string' } [ +...n ] )
[ ; ]

<execute_option>::=
{
        RECOMPILE
    | { RESULT SETS UNDEFINED }
    | { RESULT SETS NONE }
    | { RESULT SETS ( <result_sets_definition> [,...n ] ) }
}
```

## EXISTS (Transact-SQL)

`docs/t-sql/language-elements/exists-transact-sql.md`

### Syntax

```syntaxsql
EXISTS ( subquery )
```

## Expressions (Transact-SQL)

`docs/t-sql/language-elements/expressions-transact-sql.md`

### Syntax

> Syntax for SQL Server and Azure SQL Database.

```syntaxsql
{ constant | scalar_function | [ table_name. ] column | variable
    | ( expression ) | ( scalar_subquery )
    | { unary_operator } expression
    | expression { binary_operator } expression
    | ranking_windowed_function | aggregate_windowed_function
}
```

> Syntax for Azure Synapse Analytics and Parallel Data Warehouse.

```syntaxsql
-- Expression in a SELECT statement
<expression> ::=
{
    constant
    | scalar_function
    | column
    | variable
    | ( expression )
    | { unary_operator } expression
    | expression { binary_operator } expression
}
[ COLLATE Windows_collation_name ]

-- Scalar Expression in a DECLARE , SET , IF...ELSE , or WHILE statement
<scalar_expression> ::=
{
    constant
    | scalar_function
    | variable
    | ( expression )
    | (scalar_subquery )
    | { unary_operator } expression
    | expression { binary_operator } expression
}
[ COLLATE [ Windows_collation_name ] ]
```

## FETCH (Transact-SQL)

`docs/t-sql/language-elements/fetch-transact-sql.md`

### Syntax

```syntaxsql
FETCH
          [ [ NEXT | PRIOR | FIRST | LAST
                    | ABSOLUTE { n | @nvar }
                    | RELATIVE { n | @nvar }
               ]
               FROM
          ]
{ { [ GLOBAL ] cursor_name } | @cursor_variable_name }
[ INTO @variable_name [ ,...n ] ]
```

## GOTO (Transact-SQL)

`docs/t-sql/language-elements/goto-transact-sql.md`

### Syntax

```
Define the label:
label:
Alter the execution:
GOTO label
```

## &gt;= (Greater Than or Equal To) (Transact-SQL)

`docs/t-sql/language-elements/greater-than-or-equal-to-transact-sql.md`

### Syntax

```syntaxsql
expression >= expression
```

## &gt; (Greater Than) (Transact-SQL)

`docs/t-sql/language-elements/greater-than-transact-sql.md`

### Syntax

```syntaxsql
expression > expression
```

## IF...ELSE (Transact-SQL)

`docs/t-sql/language-elements/if-else-transact-sql.md`

### Syntax

```syntaxsql
IF boolean_expression
    { sql_statement | statement_block }
[ ELSE
    { sql_statement | statement_block } ]
```

## IN (Transact-SQL)

`docs/t-sql/language-elements/in-transact-sql.md`

### Syntax

```syntaxsql
test_expression [ NOT ] IN
    ( subquery | expression [ ,...n ]
    )
```

## KILL QUERY NOTIFICATION SUBSCRIPTION

`docs/t-sql/language-elements/kill-query-notification-subscription-transact-sql.md`

### Syntax

```syntaxsql
KILL QUERY NOTIFICATION SUBSCRIPTION
   { ALL | subscription_id }
```

## KILL STATS JOB (Transact-SQL)

`docs/t-sql/language-elements/kill-stats-job-transact-sql.md`

### Syntax

```syntaxsql
KILL STATS JOB job_id
```

## KILL (Transact-SQL)

`docs/t-sql/language-elements/kill-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, and Azure SQL Managed Instance:

```syntaxsql
KILL { session_id [ WITH STATUSONLY ] | UOW [ WITH STATUSONLY | COMMIT | ROLLBACK ] }
[ ; ]
```

> Syntax for Azure Synapse Analytics, Analytics Platform System (PDW), and Microsoft Fabric:

```syntaxsql
KILL 'session_id'
[ ; ]
```

## <= (Less Than or Equal To) (Transact-SQL)

`docs/t-sql/language-elements/less-than-or-equal-to-transact-sql.md`

### Syntax

```syntaxsql
expression <= expression
```

## < (Less Than) (Transact-SQL)

`docs/t-sql/language-elements/less-than-transact-sql.md`

### Syntax

```syntaxsql
expression < expression
```

## LIKE (Transact-SQL)

`docs/t-sql/language-elements/like-transact-sql.md`

### Syntax

> Syntax for SQL Server and Azure SQL Database:

```syntaxsql
match_expression [ NOT ] LIKE pattern [ ESCAPE escape_character ]
```

> Syntax for Azure Synapse Analytics and Parallel Data Warehouse:

```syntaxsql
match_expression [ NOT ] LIKE pattern
```

## %= (Modulus assignment) (Transact-SQL)

`docs/t-sql/language-elements/modulo-equals-transact-sql.md`

### Syntax

```syntaxsql
expression %= expression
```

## % (Modulus) (Transact-SQL)

`docs/t-sql/language-elements/modulo-transact-sql.md`

### Syntax

```syntaxsql
dividend % divisor
```

## *= (Multiplication Assignment) (Transact-SQL)

`docs/t-sql/language-elements/multiply-equals-transact-sql.md`

### Syntax

```syntaxsql
expression *= expression
```

## * (Multiplication) (Transact-SQL)

`docs/t-sql/language-elements/multiply-transact-sql.md`

### Syntax

```syntaxsql
expression * expression
```

## &lt;&gt; (Not Equal To) (Transact-SQL)

`docs/t-sql/language-elements/not-equal-to-transact-sql-traditional.md`

### Syntax

```syntaxsql
expression <> expression
```

## !&gt; (Not Greater Than) (Transact-SQL)

`docs/t-sql/language-elements/not-greater-than-transact-sql.md`

### Syntax

```syntaxsql
expression !> expression
```

## !&lt; (Not Less Than) (Transact-SQL)

`docs/t-sql/language-elements/not-less-than-transact-sql.md`

### Syntax

```syntaxsql
expression !< expression
```

## NOT (Transact-SQL)

`docs/t-sql/language-elements/not-transact-sql.md`

### Syntax

```syntaxsql
[ NOT ] boolean_expression
```

## NULLIF (Transact-SQL)

`docs/t-sql/language-elements/nullif-transact-sql.md`

### Syntax

```syntaxsql
NULLIF ( expression , expression )
```

## OPEN (Transact-SQL)

`docs/t-sql/language-elements/open-transact-sql.md`

### Syntax

```syntaxsql
OPEN { { [ GLOBAL ] cursor_name } | cursor_variable_name }
```

## OR (Transact-SQL)

`docs/t-sql/language-elements/or-transact-sql.md`

### Syntax

```syntaxsql
boolean_expression OR boolean_expression
```

## PRINT (Transact-SQL)

`docs/t-sql/language-elements/print-transact-sql.md`

### Syntax

```syntaxsql
PRINT msg_str | @local_variable | string_expr
```

## RAISERROR (Transact-SQL)

`docs/t-sql/language-elements/raiserror-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, and Azure SQL Managed Instance:

```syntaxsql
RAISERROR ( { msg_id | msg_str | @local_variable }
    { , severity , state }
    [ , argument [ , ...n ] ] )
    [ WITH option [ , ...n ] ]
```

> Syntax for Azure Synapse Analytics and Parallel Data Warehouse:

```syntaxsql
RAISERROR ( { msg_str | @local_variable }
    { , severity , state }
    [ , argument [ , ...n ] ] )
    [ WITH option [ , ...n ] ]
```

## RECONFIGURE (Transact-SQL)

`docs/t-sql/language-elements/reconfigure-transact-sql.md`

### Syntax

```syntaxsql
RECONFIGURE [ WITH OVERRIDE ]
```

## RETURN (Transact-SQL)

`docs/t-sql/language-elements/return-transact-sql.md`

### Syntax

```syntaxsql
RETURN [ integer_expression ]
```

## ROLLBACK TRANSACTION (Transact-SQL)

`docs/t-sql/language-elements/rollback-transaction-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, Azure SQL Managed Instance, SQL database in Microsoft Fabric.

```syntaxsql
ROLLBACK { TRAN | TRANSACTION }
    [ transaction_name | @tran_name_variable
    | savepoint_name | @savepoint_variable ]
[ ; ]
```

> Syntax for Fabric Data Warehouse, Azure Synapse Analytics, and Parallel Data Warehouse Database.

```syntaxsql
ROLLBACK { TRAN | TRANSACTION }
[ ; ]
```

## ROLLBACK WORK (Transact-SQL)

`docs/t-sql/language-elements/rollback-work-transact-sql.md`

### Syntax

```syntaxsql
ROLLBACK [ WORK ]
[ ; ]
```

## SAVE TRANSACTION (Transact-SQL)

`docs/t-sql/language-elements/save-transaction-transact-sql.md`

### Syntax

```syntaxsql
SAVE { TRAN | TRANSACTION } { savepoint_name | @savepoint_variable }
[ ; ]
```

## SELECT @local_variable (Transact-SQL)

`docs/t-sql/language-elements/select-local-variable-transact-sql.md`

### Syntax

```syntaxsql
SELECT { @local_variable { = | += | -= | *= | /= | %= | &= | ^= | |= } expression }
    [ , ...n ] [ ; ]
```

### C. Antipattern use of recursive variable assignment

> Avoid the following pattern for recursive use of variables and expressions:

```syntaxsql
SELECT @Var = <expression containing @Var>
FROM
...
```

## SET @local_variable (Transact-SQL)

`docs/t-sql/language-elements/set-local-variable-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, and Azure SQL Managed Instance:

```syntaxsql
SET
{ @local_variable
    [ . { property_name | field_name } ] = { expression | udt_name { . | :: } method_name }
}
| { @SQLCLR_local_variable.mutator_method }
| { @local_variable
    { += | -= | *= | /= | %= | &= | ^= | |= } expression
}
| { @cursor_variable =
    { @cursor_variable | cursor_name
    | { CURSOR [ [ LOCAL | GLOBAL ] ]
        [ FORWARD_ONLY | SCROLL ]
        [ STATIC | KEYSET | DYNAMIC | FAST_FORWARD ]
        [ READ_ONLY | SCROLL_LOCKS | OPTIMISTIC ]
        [ TYPE_WARNING ]
    FOR select_statement
        [ FOR { READ ONLY | UPDATE [ OF column_name [ , ...n ] ] } ]
      }
    }
}
```

> Syntax for Azure Synapse Analytics and Parallel Data Warehouse and Microsoft Fabric:

```syntaxsql
SET @local_variable { = | += | -= | *= | /= | %= | &= | ^= | |= } expression
```

## EXCEPT and INTERSECT (Transact-SQL)

`docs/t-sql/language-elements/set-operators-except-and-intersect-transact-sql.md`

### Syntax

```syntaxsql
{ <query_specification> | ( <query_expression> ) }
{ EXCEPT | INTERSECT }
{ <query_specification> | ( <query_expression> ) }
```

## UNION (Transact-SQL)

`docs/t-sql/language-elements/set-operators-union-transact-sql.md`

### Syntax

```syntaxsql
{ <query_specification> | ( <query_expression> ) }
{ UNION [ ALL ]
  { <query_specification> | ( <query_expression> ) }
  [ ...n ] }
```

## SHUTDOWN (Transact-SQL)

`docs/t-sql/language-elements/shutdown-transact-sql.md`

### Syntax

```syntaxsql
SHUTDOWN [ WITH NOWAIT ]
```

## Slash Star (Block Comment) (Transact-SQL)

`docs/t-sql/language-elements/slash-star-comment-transact-sql.md`

### Syntax

```syntaxsql
/*
text_of_comment
*/
```

## SOME | ANY (Transact-SQL)

`docs/t-sql/language-elements/some-any-transact-sql.md`

### Syntax

```syntaxsql
scalar_expression { = | <> | != | > | >= | !> | < | <= | !< }
     { SOME | ANY } ( subquery )
```

## Backslash (Line Continuation) (Transact-SQL)

`docs/t-sql/language-elements/sql-server-utilities-statements-backslash.md`

### Syntax

```syntaxsql
<first section of string> \
<continued section of string>
```

## SQL Server Utilities Statements - GO

`docs/t-sql/language-elements/sql-server-utilities-statements-go.md`

### Syntax

```syntaxsql
GO [count]
```

## = (String comparison or assignment)

`docs/t-sql/language-elements/string-comparison-assignment.md`

### Syntax

```syntaxsql
expression = expression
```

## += String concatenation

`docs/t-sql/language-elements/string-concatenation-equal-transact-sql.md`

### Syntax

```syntaxsql
expression += expression
```

## || (String Concatenation) (Transact-SQL)

`docs/t-sql/language-elements/string-concatenation-pipes-transact-sql.md`

### Syntax

```syntaxsql
expression || expression
```

## + (String concatenation) (Transact-SQL)

`docs/t-sql/language-elements/string-concatenation-transact-sql.md`

### Syntax

```syntaxsql
expression + expression
```

## -= (Subtraction Assignment) (Transact-SQL)

`docs/t-sql/language-elements/subtract-equals-transact-sql.md`

### Syntax

```syntaxsql
expression -= expression
```

## - (Subtraction) (Transact-SQL)

`docs/t-sql/language-elements/subtract-transact-sql.md`

### Syntax

```syntaxsql
expression - expression
```

## THROW (Transact-SQL)

`docs/t-sql/language-elements/throw-transact-sql.md`

### Syntax

```syntaxsql
THROW [ { error_number | @local_variable }
    , { message | @local_variable }
    , { state | @local_variable } ]
[ ; ]
```

## Transactions (Azure Synapse Analytics)

`docs/t-sql/language-elements/transactions-sql-data-warehouse.md`

### Syntax

```syntaxsql
BEGIN TRANSACTION [;]
COMMIT [ TRAN | TRANSACTION | WORK ] [;]
ROLLBACK [ TRAN | TRANSACTION | WORK ] [;]
SET AUTOCOMMIT { ON | OFF } [;]
SET IMPLICIT_TRANSACTIONS { ON | OFF } [;]
```

## TRY...CATCH (Transact-SQL)

`docs/t-sql/language-elements/try-catch-transact-sql.md`

### Syntax

```syntaxsql
BEGIN TRY
    { sql_statement | statement_block }
END TRY
BEGIN CATCH
    [ { sql_statement | statement_block } ]
END CATCH
[ ; ]
```

## - (Unary Negative) (Transact-SQL)

`docs/t-sql/language-elements/unary-operators-negative.md`

### Syntax

```syntaxsql
- numeric_expression
```

## + (Unary Positive) (Transact-SQL)

`docs/t-sql/language-elements/unary-operators-positive.md`

### Syntax

```syntaxsql
+ numeric_expression
```

## USE (Transact-SQL)

`docs/t-sql/language-elements/use-transact-sql.md`

### Syntax

```syntaxsql
USE { database_name }
[ ; ]
```

## WAITFOR (Transact-SQL)

`docs/t-sql/language-elements/waitfor-transact-sql.md`

### Syntax

```syntaxsql
WAITFOR
{
    DELAY 'time_to_pass'
  | TIME 'time_to_execute'
  | [ ( receive_statement ) | ( get_conversation_group_statement ) ]
    [ , TIMEOUT timeout ]
}
```

## WHILE (Transact-SQL)

`docs/t-sql/language-elements/while-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, Azure SQL Managed Instance, and Microsoft Fabric.

```syntaxsql
WHILE boolean_expression
    { sql_statement | statement_block | BREAK | CONTINUE }
```

> Syntax for Azure Synapse Analytics and Analytics Platform System (PDW).

```syntaxsql
WHILE boolean_expression
    { sql_statement | statement_block | BREAK }
```

## Aliasing

`docs/t-sql/queries/aliasing-azure-sql-data-warehouse-parallel-data-warehouse.md`

### Syntax

```syntaxsql
object_source [ AS ] alias
```

## AT TIME ZONE (Transact-SQL)

`docs/t-sql/queries/at-time-zone-transact-sql.md`

### Syntax

```syntaxsql
inputdate AT TIME ZONE timezone
```

## CONTAINS (Transact-SQL)

`docs/t-sql/queries/contains-transact-sql.md`

### Syntax

```syntaxsql
CONTAINS (
     {
        column_name | ( column_list )
      | *
      | PROPERTY ( { column_name }, 'property_name' )
     }
     , '<contains_search_condition>'
     [ , LANGUAGE language_term ]
   )

<contains_search_condition> ::=
  {
      <simple_term>
    | <prefix_term>
    | <generation_term>
    | <generic_proximity_term>
    | <custom_proximity_term>
    | <weighted_term>
    }
  |
    { ( <contains_search_condition> )
        [ { <AND> | <AND NOT> | <OR> } ]
        <contains_search_condition> [ ...n ]
  }
<simple_term> ::=
     { word | "phrase" }

<prefix term> ::=
  { "word*" | "phrase*" }

<generation_term> ::=
  FORMSOF ( { INFLECTIONAL | THESAURUS } , <simple_term> [ ,...n ] )

<generic_proximity_term> ::=
  { <simple_term> | <prefix_term> } { { { NEAR | ~ }
     { <simple_term> | <prefix_term> } } [ ...n ] }

<custom_proximity_term> ::=
  NEAR (
     {
        { <simple_term> | <prefix_term> } [ ,...n ]
     |
        ( { <simple_term> | <prefix_term> } [ ,...n ] )
      [, <maximum_distance> [, <match_order> ] ]
     }
       )

      <maximum_distance> ::= { integer | MAX }
      <match_order> ::= { TRUE | FALSE }

<weighted_term> ::=
  ISABOUT
   ( {
        {
          <simple_term>
        | <prefix_term>
        | <generation_term>
        | <proximity_term>
        }
      [ WEIGHT ( weight_value ) ]
      } [ ,...n ]
   )

<AND> ::=
  { AND | & }

<AND NOT> ::=
  { AND NOT | &! }

<OR> ::=
  { OR | | }
```

## EXPLAIN (Transact-SQL)

`docs/t-sql/queries/explain-transact-sql.md`

### Syntax

```syntaxsql
EXPLAIN [WITH_RECOMMENDATIONS] SQL_statement
[;]
```

## FREETEXT (Transact-SQL)

`docs/t-sql/queries/freetext-transact-sql.md`

### Syntax

```syntaxsql
FREETEXT ( { column_name | (column_list) | * }
          , 'freetext_string' [ , LANGUAGE language_term ] )
```

## FROM clause plus JOIN, APPLY, PIVOT (T-SQL)

`docs/t-sql/queries/from-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, and SQL database in Fabric:

```syntaxsql
[ FROM { <table_source> } [ , ...n ] ]
<table_source> ::=
{
    table_or_view_name [ FOR SYSTEM_TIME <system_time> ] [ [ AS ] table_alias ]
        [ <tablesample_clause> ]
        [ WITH ( < table_hint > [ [ , ] ...n ] ) ]
    | rowset_function [ [ AS ] table_alias ]
        [ ( bulk_column_alias [ , ...n ] ) ]
    | user_defined_function [ [ AS ] table_alias ]
    | OPENXML <openxml_clause>
    | derived_table [ [ AS ] table_alias ] [ ( column_alias [ , ...n ] ) ]
    | <joined_table>
    | <pivoted_table>
    | <unpivoted_table>
    | @variable [ [ AS ] table_alias ]
    | @variable.function_call ( expression [ , ...n ] )
        [ [ AS ] table_alias ] [ (column_alias [ , ...n ] ) ]
}
<tablesample_clause> ::=
    TABLESAMPLE [ SYSTEM ] ( sample_number [ PERCENT | ROWS ] )
        [ REPEATABLE ( repeat_seed ) ]

<joined_table> ::=
{
    <table_source> <join_type> <table_source> ON <search_condition>
    | <table_source> CROSS JOIN <table_source>
    | left_table_source { CROSS | OUTER } APPLY right_table_source
    | [ ( ] <joined_table> [ ) ]
}
<join_type> ::=
    [ { INNER | { { LEFT | RIGHT | FULL } [ OUTER ] } } [ <join_hint> ] ]
    JOIN

<pivoted_table> ::=
    table_source PIVOT <pivot_clause> [ [ AS ] table_alias ]

<pivot_clause> ::=
        ( aggregate_function ( value_column [ [ , ] ...n ] )
        FOR pivot_column
        IN ( <column_list> )
    )

<unpivoted_table> ::=
    table_source UNPIVOT <unpivot_clause> [ [ AS ] table_alias ]

<unpivot_clause> ::=
    ( value_column FOR pivot_column IN ( <column_list> ) )

<column_list> ::=
    column_name [ , ...n ]

<system_time> ::=
{
      AS OF <date_time>
    | FROM <start_date_time> TO <end_date_time>
    | BETWEEN <start_date_time> AND <end_date_time>
    | CONTAINED IN (<start_date_time> , <end_date_time>)
    | ALL
}

    <date_time>::=
        <date_time_literal> | @date_time_variable

    <start_date_time>::=
        <date_time_literal> | @date_time_variable

    <end_date_time>::=
        <date_time_literal> | @date_time_variable
```

> Syntax for Parallel Data Warehouse, Azure Synapse Analytics:

```syntaxsql
FROM { <table_source> [ , ...n ] }

<table_source> ::=
{
    [ database_name . [ schema_name ] . | schema_name . ] table_or_view_name [ AS ] table_or_view_alias
    [ <tablesample_clause> ]
    | derived_table [ AS ] table_alias [ ( column_alias [ , ...n ] ) ]
    | <joined_table>
}

<tablesample_clause> ::=
    TABLESAMPLE ( sample_number [ PERCENT ] ) -- Azure Synapse Analytics Dedicated SQL pool only

<joined_table> ::=
{
    <table_source> <join_type> <table_source> ON search_condition
    | <table_source> CROSS JOIN <table_source>
    | left_table_source { CROSS | OUTER } APPLY right_table_source
    | [ ( ] <joined_table> [ ) ]
}

<join_type> ::=
    [ INNER ] [ <join_hint> ] JOIN
    | LEFT  [ OUTER ] JOIN
    | RIGHT [ OUTER ] JOIN
    | FULL  [ OUTER ] JOIN

<join_hint> ::=
    REDUCE
    | REPLICATE
    | REDISTRIBUTE
```

> Syntax for Microsoft Fabric Data Warehouse:

```syntaxsql
FROM { <table_source> [ , ...n ] }

<table_source> ::=
{
    [ database_name . [ schema_name ] . | schema_name . ] table_or_view_name [ AS ] table_or_view_alias
    | derived_table [ AS ] table_alias [ ( column_alias [ , ...n ] ) ]
    | <joined_table>
}

<joined_table> ::=
{
    <table_source> <join_type> <table_source> ON search_condition
    | <table_source> CROSS JOIN <table_source>
    | left_table_source { CROSS | OUTER } APPLY right_table_source
    | [ ( ] <joined_table> [ ) ]
}

<join_type> ::=
    [ INNER ] [ <join_hint> ] JOIN
    | LEFT  [ OUTER ] JOIN
    | RIGHT [ OUTER ] JOIN
    | FULL  [ OUTER ] JOIN

<join_hint> ::=
    REDUCE
    | REPLICATE
    | REDISTRIBUTE
```

## Using PIVOT and UNPIVOT

`docs/t-sql/queries/from-using-pivot-and-unpivot.md`

### Syntax

> Syntax for the `PIVOT` operator.

```syntaxsql
SELECT [ <non-pivoted column> [ AS <column name> ] , ]
    ...
    [ <first pivoted column> [ AS <column name> ] ,
    [ <second pivoted column> [ AS <column name> ] , ]
    ...
    [ <last pivoted column> [ AS <column name> ] ] ]
FROM
    ( <SELECT query that produces the data> )
    AS <alias for the source query>
PIVOT
(
    <aggregation function> ( <column being aggregated> )
FOR <column that contains the values that become column headers>
    IN ( <first pivoted column>
         , <second pivoted column>
         , ... <last pivoted column> )
) AS <alias for the pivot table>
[ <optional ORDER BY clause> ]
[ ; ]
```

> Syntax for the `UNPIVOT` operator.

```syntaxsql
SELECT [ <non-pivoted column> [ AS <column name> ] , ]
    ...
    [ <output column for names of the pivot columns> [ AS <column name> ] , ]
    [ <new output column created for values in result of the source query> [ AS <column name> ] ]
FROM
    ( <SELECT query that produces the data> )
    AS <alias for the source query>
UNPIVOT
(
    <new output column created for values in result of the source query>
FOR <output column for names of the pivot columns>
    IN ( <first pivoted column>
         , <second pivoted column>
         , ... <last pivoted column> )
)
[ <optional ORDER BY clause> ]
[ ; ]
```

## Join hints (Transact-SQL)

`docs/t-sql/queries/hints-transact-sql-join.md`

### Syntax

```syntaxsql
<join_hint> ::=
     { LOOP | HASH | MERGE | REMOTE | REDUCE | REPLICATE | REDISTRIBUTE [(columns count)]}
```

## Query Hints (Transact-SQL)

`docs/t-sql/queries/hints-transact-sql-query.md`

### Syntax

```syntaxsql
<query_hint> ::=
{ { HASH | ORDER } GROUP
  | { CONCAT | HASH | MERGE } UNION
  | { LOOP | MERGE | HASH } JOIN
  | DISABLE_OPTIMIZED_PLAN_FORCING
  | EXPAND VIEWS
  | FAST <integer_value>
  | FORCE ORDER
  | { FORCE | DISABLE } EXTERNALPUSHDOWN
  | { FORCE | DISABLE } SCALEOUTEXECUTION
  | IGNORE_NONCLUSTERED_COLUMNSTORE_INDEX
  | KEEP PLAN
  | KEEPFIXED PLAN
  | MAX_GRANT_PERCENT = <numeric_value>
  | MIN_GRANT_PERCENT = <numeric_value>
  | MAXDOP <integer_value>
  | MAXRECURSION <integer_value>
  | NO_PERFORMANCE_SPOOL
  | OPTIMIZE FOR ( @variable_name { UNKNOWN | = <literal_constant> } [ , ...n ] )
  | OPTIMIZE FOR UNKNOWN
  | PARAMETERIZATION { SIMPLE | FORCED }
  | QUERYTRACEON <integer_value>
  | RECOMPILE
  | ROBUST PLAN
  | USE HINT ( 'hint_name' [ , ...n ] )
  | USE PLAN N'<xml_plan>'
  | TABLE HINT ( <exposed_object_name> [ , <table_hint> [ [ , ] ...n ] ] )
  | FOR TIMESTAMP AS OF '<point_in_time>'
}

<table_hint> ::=
{ NOEXPAND [ , INDEX ( <index_value> [ , ...n ] ) | INDEX = ( <index_value> ) ]
  | INDEX ( <index_value> [ , ...n ] ) | INDEX = ( <index_value> )
  | FORCESEEK [ ( <index_value> ( <index_column_name> [ , ... ] ) ) ]
  | FORCESCAN
  | HOLDLOCK
  | NOLOCK
  | NOWAIT
  | PAGLOCK
  | READCOMMITTED
  | READCOMMITTEDLOCK
  | READPAST
  | READUNCOMMITTED
  | REPEATABLEREAD
  | ROWLOCK
  | SERIALIZABLE
  | SNAPSHOT
  | SPATIAL_WINDOW_MAX_CELLS = <integer_value>
  | TABLOCK
  | TABLOCKX
  | UPDLOCK
  | XLOCK
}
```

## Table Hints (Transact-SQL)

`docs/t-sql/queries/hints-transact-sql-table.md`

### Syntax

```syntaxsql
WITH  ( <table_hint> [ [ , ] ...n ] )

<table_hint> ::=
{ NOEXPAND
  | INDEX ( <index_value> [ , ...n ] ) | INDEX = ( <index_value> )
  | FORCE_ANN_ONLY
  | FORCESEEK [ ( <index_value> ( <index_column_name> [ , ... ] ) ) ]
  | FORCESCAN
  | HOLDLOCK
  | NOLOCK
  | NOWAIT
  | PAGLOCK
  | READCOMMITTED
  | READCOMMITTEDLOCK
  | READPAST
  | READUNCOMMITTED
  | REPEATABLEREAD
  | ROWLOCK
  | SERIALIZABLE
  | SNAPSHOT
  | SPATIAL_WINDOW_MAX_CELLS = <integer_value>
  | TABLOCK
  | TABLOCKX
  | UPDLOCK
  | XLOCK
}

<table_hint_limited> ::=
{
    KEEPIDENTITY
  | KEEPDEFAULTS
  | HOLDLOCK
  | IGNORE_CONSTRAINTS
  | IGNORE_TRIGGERS
  | NOLOCK
  | NOWAIT
  | PAGLOCK
  | READCOMMITTED
  | READCOMMITTEDLOCK
  | READPAST
  | REPEATABLEREAD
  | ROWLOCK
  | SERIALIZABLE
  | SNAPSHOT
  | TABLOCK
  | TABLOCKX
  | UPDLOCK
  | XLOCK
}
```

## IS [NOT] DISTINCT FROM (Transact-SQL)

`docs/t-sql/queries/is-distinct-from-transact-sql.md`

### Syntax

```syntaxsql
expression IS [NOT] DISTINCT FROM expression
```

## IS [NOT] NULL (Transact-SQL)

`docs/t-sql/queries/is-null-transact-sql.md`

### Syntax

```syntaxsql
expression IS [ NOT ] NULL
```

## MATCH (SQL Graph)

`docs/t-sql/queries/match-sql-graph.md`

### Syntax

```syntaxsql
MATCH (<graph_search_pattern>)

<graph_search_pattern>::=
  {
      <simple_match_pattern>
    | <arbitrary_length_match_pattern>
    | <arbitrary_length_match_last_node_predicate>
  }

<simple_match_pattern>::=
  {
      LAST_NODE(<node_alias>) | <node_alias>   {
          { <-( <edge_alias> )- }
        | { -( <edge_alias> )-> }
        <node_alias> | LAST_NODE(<node_alias>)
        }
  }
  [ { AND } { ( <simple_match_pattern> ) } ]
  [ , ...n ]

<node_alias> ::=
  node_table_name | node_table_alias

<edge_alias> ::=
  edge_table_name | edge_table_alias

<arbitrary_length_match_pattern>  ::=
  {
    SHORTEST_PATH(
      <arbitrary_length_pattern>
      [ { AND } { <arbitrary_length_pattern> } ]
      [ , ...n ]
    )
  }

<arbitrary_length_match_last_node_predicate> ::=
  {  LAST_NODE( <node_alias> ) = LAST_NODE( <node_alias> ) }

<arbitrary_length_pattern> ::=
    {  LAST_NODE( <node_alias> )   | <node_alias>
     ( <edge_first_al_pattern> [ <edge_first_al_pattern>... , n ] )
     <al_pattern_quantifier>
  }
     |  ( { <node_first_al_pattern> [ <node_first_al_pattern> ... , n ] )
            <al_pattern_quantifier>
        LAST_NODE( <node_alias> ) | <node_alias>
 }

<edge_first_al_pattern> ::=
  { (
        { -( <edge_alias> )->   }
      | { <-( <edge_alias> )- }
      <node_alias>
      )
  }

<node_first_al_pattern> ::=
  { (
      <node_alias>
        { <-( <edge_alias> )- }
      | { -( <edge_alias> )-> }
       )
  }

<al_pattern_quantifier> ::=
  {
        +
      | { 1 , n }
  }

n -  positive integer only.
```

## Nested Common Table Expression (CTE)

`docs/t-sql/queries/nested-common-table-expression.md`

### Syntax

```syntaxsql
WITH <NESTED_CTE_NAME_LEVEL1> [ (column_name , ...) ] AS
    (WITH <NESTED_CTE_NAME_LEVEL2> [ (column_name , ...) ] AS
        (
            ...
                WITH <NESTED_CTE_NAME_LEVELn-1> [ ( column_name , ...) ] AS
                (
                    WITH <NESTED_CTE_NAME_LEVELn> [ ( column_name , ...) ] AS
                    (
                        Standard_CTE_query_definition
                    )
                    <SELECT statement> -- Data source must include NESTED_CTE_NAME_LEVELn
                )
                <SELECT statement> -- Data source must include NESTED_CTE_NAME_LEVELn-1
            ...
        )
    <SELECT statement> -- Data source must include NESTED_CTE_NAME_LEVEL2
    )
```

## OPTION clause (Transact-SQL)

`docs/t-sql/queries/option-clause-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Managed Instance, and Azure SQL Database:

```syntaxsql
[ OPTION ( <query_hint> [ , ...n ] ) ]
```

> Syntax for Warehouse in Microsoft Fabric:

```syntaxsql
OPTION ( <query_option> [ , ...n ] )

<query_option> ::=
    LABEL = label_name |
    <query_hint>

<query_hint> ::=
    HASH JOIN
    | LOOP JOIN
    | MERGE JOIN
    | FORCE ORDER
    | { FORCE | DISABLE } EXTERNALPUSHDOWN
    | FOR TIMESTAMP AS OF '<point_in_time>'
```

> Syntax for Azure Synapse Analytics and Analytics Platform System (PDW) and SQL analytics endpoint in Microsoft Fabric:

```syntaxsql
OPTION ( <query_option> [ , ...n ] )

<query_option> ::=
    LABEL = label_name |
    <query_hint>

<query_hint> ::=
    HASH JOIN
    | LOOP JOIN
    | MERGE JOIN
    | FORCE ORDER
    | { FORCE | DISABLE } EXTERNALPUSHDOWN
```

> Syntax for serverless SQL pool in Azure Synapse Analytics:

```syntaxsql
OPTION ( <query_option> [ , ...n ] )

<query_option> ::=
    LABEL = label_name
```

## OUTPUT clause (Transact-SQL)

`docs/t-sql/queries/output-clause-transact-sql.md`

### Syntax

```syntaxsql
<OUTPUT_CLAUSE> ::=
{
    [ OUTPUT <dml_select_list> INTO { @table_variable | output_table } [ ( column_list ) ] ]
    [ OUTPUT <dml_select_list> ]
}
<dml_select_list> ::=
{ <column_name> | scalar_expression } [ [ AS ] column_alias_identifier ]
    [ , ...n ]

<column_name> ::=
{ DELETED | INSERTED | from_table_name } . { * | column_name }
    | $action
```

## PREDICT (Transact-SQL)

`docs/t-sql/queries/predict-transact-sql.md`

### Syntax

Marked for `>=sql-server-2017 || >=sql-server-linux-2017 || =azuresqldb-mi-current`.

```syntaxsql
PREDICT
(
  MODEL = @model | model_literal,
  DATA = object AS <table_alias>
)
WITH ( <result_set_definition> )

<result_set_definition> ::=
  {
    { column_name
      data_type
      [ COLLATE collation_name ]
      [ NULL | NOT NULL ]
    }
      [,...n ]
  }

MODEL = @model | model_literal
```

Marked for `>=azure-sqldw-latest`.

```syntaxsql
PREDICT
(
  MODEL = <model_object>,
  DATA = object AS <table_alias>
  [, RUNTIME = ONNX ]
)
WITH ( <result_set_definition> )

<result_set_definition> ::=
  {
    { column_name
      data_type
      [ COLLATE collation_name ]
      [ NULL | NOT NULL ]
    }
      [,...n ]
  }

<model_object> ::=
  {
    model_literal
    | model_variable
    | ( scalar_subquery )
  }
```

## READTEXT (Transact-SQL)

`docs/t-sql/queries/readtext-transact-sql.md`

### Syntax

```syntaxsql
READTEXT { table.column text_ptr offset size } [ HOLDLOCK ]
```

## Recursive Queries Using Common Table Expressions

`docs/t-sql/queries/recursive-common-table-expression-transact-sql.md`

### Pseudocode and semantics

> The recursive CTE structure must contain at least one anchor member and one recursive member. The following pseudocode shows the components of a simple recursive CTE that contains a single anchor member and single recursive member.

```syntaxsql
WITH cte_name ( column_name [ ,...n ] )
AS
(
    CTE_query_definition -- Anchor member is defined.
    UNION ALL
    CTE_query_definition -- Recursive member is defined referencing cte_name.
)

-- Statement using the CTE
SELECT *
FROM cte_name
```

## Search condition (Transact-SQL)

`docs/t-sql/queries/search-condition-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, and Azure SQL Managed Instance.

```syntaxsql
<search_condition> ::=
    MATCH (<graph_search_pattern>) | <search_condition_without_match> | <search_condition> AND <search_condition>

<search_condition_without_match> ::=
    { [ NOT ] <predicate> | ( <search_condition_without_match> ) }
    [ { AND | OR } [ NOT ] { <predicate> | ( <search_condition_without_match> ) } ]
[ ...n ]

<predicate> ::=
    { expression { = | <> | != | > | >= | !> | < | <= | !< } expression
    | string_expression [ NOT ] LIKE string_expression
  [ ESCAPE 'escape_character' ]
    | expression [ NOT ] BETWEEN expression AND expression
    | expression IS [ NOT ] NULL
    | expression IS [ NOT ] DISTINCT FROM
    | CONTAINS
  ( { column | * } , '<contains_search_condition>' )
    | FREETEXT ( { column | * } , 'freetext_string' )
    | expression [ NOT ] IN ( subquery | expression [ , ...n ] )
    | expression { = | < > | != | > | >= | ! > | < | <= | ! < }
  { ALL | SOME | ANY } ( subquery )
    | EXISTS ( subquery )     }

<graph_search_pattern> ::=
    { <node_alias> {
                    { <-( <edge_alias> )- }
                    | { -( <edge_alias> )-> }
                    <node_alias>
                   }
    }

<node_alias> ::=
    node_table_name | node_table_alias

<edge_alias> ::=
    edge_table_name | edge_table_alias
```

> Syntax for Azure Synapse Analytics and Parallel Data Warehouse.

```syntaxsql
< search_condition > ::=
    { [ NOT ] <predicate> | ( <search_condition> ) }
    [ { AND | OR } [ NOT ] { <predicate> | ( <search_condition> ) } ]
[ ...n ]

<predicate> ::=
    { expression { = | <> | != | > | >= | < | <= } expression
    | string_expression [ NOT ] LIKE string_expression
    | expression [ NOT ] BETWEEN expression AND expression
    | expression IS [ NOT ] NULL
    | expression [ NOT ] IN (subquery | expression [ , ...n ] )
    | expression [ NOT ] EXISTS (subquery)
    }
```

## SELECT Clause (Transact-SQL)

`docs/t-sql/queries/select-clause-transact-sql.md`

### Syntax

```syntaxsql
SELECT [ ALL | DISTINCT ]
[ TOP ( expression ) [ PERCENT ] [ WITH TIES ] ]
<select_list>
<select_list> ::=
    {
      *
      | { table_name | view_name | table_alias } .*
      | {
          [ { table_name | view_name | table_alias } . ]
               { column_name | $IDENTITY | $ROWGUID }
          | udt_column_name [ { . | :: } { { property_name | field_name }
            | method_name ( argument [ , ...n ] ) } ]
          | expression
         }
        [ [ AS ] column_alias ]
      | column_alias = expression
    } [ , ...n ]
```

## FOR Clause (Transact-SQL)

`docs/t-sql/queries/select-for-clause-transact-sql.md`

### Syntax

```syntaxsql
[ FOR { BROWSE | <XML> | <JSON> } ]

<XML> ::=
XML
{
    { RAW [ ( 'ElementName' ) ] | AUTO }
    [
        <CommonDirectivesForXML>
        [ , { XMLDATA | XMLSCHEMA [ ( 'TargetNameSpaceURI' ) ] } ]
        [ , ELEMENTS [ XSINIL | ABSENT ]
    ]
  | EXPLICIT
    [
        <CommonDirectivesForXML>
        [ , XMLDATA ]
    ]
  | PATH [ ( 'ElementName' ) ]
    [
        <CommonDirectivesForXML>
        [ , ELEMENTS [ XSINIL | ABSENT ] ]
    ]
}

<CommonDirectivesForXML> ::=
[ , BINARY BASE64 ]
[ , TYPE ]
[ , ROOT [ ( 'RootName' ) ] ]

<JSON> ::=
JSON
{
    { AUTO | PATH }
    [
        [ , ROOT [ ( 'RootName' ) ] ]
        [ , INCLUDE_NULL_VALUES ]
        [ , WITHOUT_ARRAY_WRAPPER ]
    ]

}
```

## GROUP BY (Transact-SQL)

`docs/t-sql/queries/select-group-by-transact-sql.md`

### Syntax

> ISO-compliant syntax for SQL Server and Azure SQL Database:

```syntaxsql
GROUP BY {
      column-expression
    | ROLLUP ( <group_by_expression> [ , ...n ] )
    | CUBE ( <group_by_expression> [ , ...n ] )
    | GROUPING SETS ( <grouping_set> [ , ...n ]  )
    | () --calculates the grand total
} [ , ...n ]

<group_by_expression> ::=
      column-expression
    | ( column-expression [ , ...n ] )

<grouping_set> ::=
      () --calculates the grand total
    | <grouping_set_item>
    | ( <grouping_set_item> [ , ...n ] )

<grouping_set_item> ::=
      <group_by_expression>
    | ROLLUP ( <group_by_expression> [ , ...n ] )
    | CUBE ( <group_by_expression> [ , ...n ] )
```

> Non-ISO-compliant syntax for SQL Server and Azure SQL Database (backward compatibility only):

```syntaxsql
GROUP BY {
       ALL column-expression [ , ...n ]
    | column-expression [ , ...n ]  WITH { CUBE | ROLLUP }
       }
```

> Syntax for Azure Synapse Analytics:

```syntaxsql
GROUP BY {
      column-name [ WITH (DISTRIBUTED_AGG) ]
    | column-expression
    | ROLLUP ( <group_by_expression> [ , ...n ] )
} [ , ...n ]
```

> Syntax for Analytics Platform System (PDW):

```syntaxsql
GROUP BY {
      column-name [ WITH (DISTRIBUTED_AGG) ]
    | column-expression
} [ , ...n ]
```

## HAVING (Transact-SQL)

`docs/t-sql/queries/select-having-transact-sql.md`

### Syntax

```syntaxsql
[ HAVING <search condition> ]
```

## INTO Clause (Transact-SQL)

`docs/t-sql/queries/select-into-clause-transact-sql.md`

### Syntax

```syntaxsql
[ INTO new_table ]
[ ON filegroup ]
```

## ORDER BY Clause (Transact-SQL)

`docs/t-sql/queries/select-order-by-clause-transact-sql.md`

### Syntax

> Syntax for SQL Server and Azure SQL Database.

```syntaxsql
ORDER BY order_by_expression
    [ COLLATE collation_name ]
    [ ASC | DESC ]
    [ , ...n ]
[ <offset_fetch> ]

<offset_fetch> ::=
{
    OFFSET { integer_constant | offset_row_count_expression } { ROW | ROWS }
    [
      FETCH { FIRST | NEXT } { integer_constant | fetch_row_count_expression } { ROW | ROWS } ONLY
    ]
}
```

> Syntax for Azure Synapse Analytics and Analytics Platform System (PDW):

```syntaxsql
[ ORDER BY
    {
    order_by_expression
    [ ASC | DESC ]
    } [ , ...n ]
]
```

## OVER Clause (Transact-SQL)

`docs/t-sql/queries/select-over-clause-transact-sql.md`

### Syntax

```syntaxsql
OVER (
       [ <PARTITION BY clause> ]
       [ <ORDER BY clause> ]
       [ <ROW or RANGE clause> ]
      )

<PARTITION BY clause> ::=
PARTITION BY value_expression , ... [ n ]

<ORDER BY clause> ::=
ORDER BY order_by_expression
    [ COLLATE collation_name ]
    [ ASC | DESC ]
    [ , ...n ]

<ROW or RANGE clause> ::=
{ ROWS | RANGE } <window frame extent>

<window frame extent> ::=
{   <window frame preceding>
  | <window frame between>
}
<window frame between> ::=
  BETWEEN <window frame bound> AND <window frame bound>

<window frame bound> ::=
{   <window frame preceding>
  | <window frame following>
}

<window frame preceding> ::=
{
    UNBOUNDED PRECEDING
  | <unsigned_value_specification> PRECEDING
  | CURRENT ROW
}

<window frame following> ::=
{
    UNBOUNDED FOLLOWING
  | <unsigned_value_specification> FOLLOWING
  | CURRENT ROW
}

<unsigned value specification> ::=
{  <unsigned integer literal> }
```

> Syntax only for Analytics Platform System (PDW):

```syntaxsql
OVER ( [ PARTITION BY value_expression ] [ order_by_clause ] )
```

### PARTITION BY

> Divides the query result set into partitions. The window function applies to each partition separately, and computation restarts for each partition.

```syntaxsql
PARTITION BY <value_expression>
```

### ORDER BY

```syntaxsql
ORDER BY <order_by_expression> [ COLLATE <collation_name> ] [ ASC | DESC ]
```

### BETWEEN AND

> **Applies to**: SQL Server 2012 (11.x) and later versions.

```syntaxsql
BETWEEN <window frame bound> AND <window frame bound>
```

## SELECT (Transact-SQL)

`docs/t-sql/queries/select-transact-sql.md`

### Syntax

> Syntax for SQL Server and Azure SQL Database:

```syntaxsql
<SELECT statement> ::=
    [ WITH { [ XMLNAMESPACES , ] [ <common_table_expression> [ , ...n ] ] } ]
    <query_expression>
    [ ORDER BY <order_by_expression> ]
    [ <FOR Clause> ]
    [ OPTION ( <query_hint> [ , ...n ] ) ]
<query_expression> ::=
    { <query_specification> | ( <query_expression> ) }
    [  { UNION [ ALL ] | EXCEPT | INTERSECT }
        <query_specification> | ( <query_expression> ) [ ...n ] ]
<query_specification> ::=
SELECT [ ALL | DISTINCT ]
    [ TOP ( expression ) [ PERCENT ] [ WITH TIES ] ]
    <select_list>
    [ INTO new_table ]
    [ FROM { <table_source> } [ , ...n ] ]
    [ WHERE <search_condition> ]
    [ <GROUP BY> ]
    [ HAVING <search_condition> ]
[ ; ]
```

> Syntax for Azure Synapse Analytics, Analytics Platform System (PDW), and Microsoft Fabric:

```syntaxsql
[ WITH <common_table_expression> [ , ...n ] ]
SELECT <select_criteria>
[ ; ]

<select_criteria> ::=
    [ TOP ( top_expression ) ]
    [ ALL | DISTINCT ]
    { * | column_name | expression } [ , ...n ]
    [ FROM { table_source } [ , ...n ] ]
    [ WHERE <search_condition> ]
    [ GROUP BY <group_by_clause> ]
    [ HAVING <search_condition> ]
    [ ORDER BY <order_by_expression> ]
    [ OPTION ( <query_option> [ , ...n ] ) ]
```

## WINDOW (Transact-SQL)

`docs/t-sql/queries/select-window-transact-sql.md`

### Syntax

```syntaxsql
WINDOW window_name AS (
       [ reference_window_name ]
       [ <PARTITION BY clause> ]
       [ <ORDER BY clause> ]
       [ <ROW or RANGE clause> ]
      )

<PARTITION BY clause> ::=
PARTITION BY value_expression , ... [ n ]

<ORDER BY clause> ::=
ORDER BY order_by_expression
    [ COLLATE collation_name ]
    [ ASC | DESC ]
    [ , ...n ]

<ROW or RANGE clause> ::=
{ ROWS | RANGE } <window frame extent>
```

## Table Value Constructor (Transact-SQL)

`docs/t-sql/queries/table-value-constructor-transact-sql.md`

### Syntax

```syntaxsql
VALUES ( <row value expression list> ) [ ,...n ]

<row value expression list> ::=
    {<row value expression> } [ ,...n ]

<row value expression> ::=
    { DEFAULT | NULL | expression }
```

## TOP (Transact-SQL)

`docs/t-sql/queries/top-transact-sql.md`

### Syntax

> Syntax for SQL Server and Azure SQL Database:

```syntaxsql
[
    TOP (expression) [ PERCENT ]
    [ WITH TIES ]
]
```

> Syntax for Azure Synapse Analytics and Analytics Platform System (PDW):

```syntaxsql
[
    TOP ( expression )
    [ WITH TIES ]
]
```

## UPDATE (Transact-SQL)

`docs/t-sql/queries/update-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server and Azure SQL Database

[ WITH <common_table_expression> [...n] ]
UPDATE
    [ TOP ( expression ) [ PERCENT ] ]
    { { table_alias | <object> | rowset_function_limited
         [ WITH ( <Table_Hint_Limited> [ ...n ] ) ]
      }
      | @table_variable
    }
    SET
        { column_name = { expression | DEFAULT | NULL }
          | { udt_column_name.{ { property_name = expression
                                | field_name = expression }
                                | method_name ( argument [ ,...n ] )
                              }
          }
          | column_name { .WRITE ( expression , @Offset , @Length ) }
          | @variable = expression
          | @variable = column = expression
          | column_name { += | -= | *= | /= | %= | &= | ^= | |= } expression
          | @variable { += | -= | *= | /= | %= | &= | ^= | |= } expression
          | @variable = column { += | -= | *= | /= | %= | &= | ^= | |= } expression
        } [ ,...n ]

    [ <OUTPUT Clause> ]
    [ FROM{ <table_source> } [ ,...n ] ]
    [ WHERE { <search_condition>
            | { [ CURRENT OF
                  { { [ GLOBAL ] cursor_name }
                      | cursor_variable_name
                  }
                ]
              }
            }
    ]
    [ OPTION ( <query_hint> [ ,...n ] ) ]
[ ; ]

<object> ::=
{
    [ server_name . database_name . schema_name .
    | database_name .[ schema_name ] .
    | schema_name .
    ]
    table_or_view_name}
```

```syntaxsql
-- Syntax for Azure Synapse Analytics and Microsoft Fabric

[ WITH <common_table_expression> [ ,...n ] ]
UPDATE [ database_name . [ schema_name ] . | schema_name . ] table_name
SET { column_name = { expression | NULL } } [ ,...n ]
FROM [ database_name . [ schema_name ] . | schema_name . ] table_name
JOIN {<join_table_source>}[ ,...n ]
ON <join_condition>
[ WHERE <search_condition> ]
[ OPTION ( LABEL = label_name ) ]
[;]

<join_table_source> ::=
{
    [ database_name . [ schema_name ] . | schema_name . ] table_or_view_name [ AS ] table_or_view_alias
    [ <tablesample_clause>]
    | derived_table [ AS ] table_alias [ ( column_alias [ ,...n ] ) ]
}
```

```syntaxsql
-- Syntax for Parallel Data Warehouse

UPDATE [ database_name . [ schema_name ] . | schema_name . ] table_name
SET { column_name = { expression | NULL } } [ ,...n ]
[ FROM from_clause ]
[ WHERE <search_condition> ]
[ OPTION ( LABEL = label_name ) ]
[;]
```

## UPDATETEXT (Transact-SQL)

`docs/t-sql/queries/updatetext-transact-sql.md`

### Syntax

```syntaxsql
UPDATETEXT [BULK] { table_name.dest_column_name dest_text_ptr }
  { NULL | insert_offset }
     { NULL | delete_length }
     [ WITH LOG ]
     [ inserted_data
    | { table_name.src_column_name src_text_ptr } ]
```

## WHERE (Transact-SQL)

`docs/t-sql/queries/where-transact-sql.md`

### Syntax

```syntaxsql
[ WHERE <search_condition> ]
```

## WITH common_table_expression (Transact-SQL)

`docs/t-sql/queries/with-common-table-expression-transact-sql.md`

### Syntax

```syntaxsql
[ WITH <common_table_expression> [ , ...n ] ]

<common_table_expression>::=
    expression_name [ ( column_name [ , ...n ] ) ]
    AS
    ( CTE_query_definition )
```

## WRITETEXT (Transact-SQL)

`docs/t-sql/queries/writetext-transact-sql.md`

### Syntax

```syntaxsql
WRITETEXT [BULK]
  { table.column text_ptr }
  [ WITH LOG ] { data }
```

## AsBinaryZM (geography Data Type)

`docs/t-sql/spatial-geography/asbinaryzm-geography-data-type.md`

### Syntax

```
.AsBinaryZM()
```

## AsGml (geography Data Type)

`docs/t-sql/spatial-geography/asgml-geography-data-type.md`

### Syntax

```
.AsGml ( )
```

## AsTextZM (geography Data Type)

`docs/t-sql/spatial-geography/astextzm-geography-data-type.md`

### Syntax

```sql
.AsTextZM ()
```

## BufferWithCurves (geography Data Type)

`docs/t-sql/spatial-geography/bufferwithcurves-geography-data-type.md`

### Syntax

```
.BufferWithCurves ( distance )
```

## BufferWithTolerance (geography Data Type)

`docs/t-sql/spatial-geography/bufferwithtolerance-geography-data-type.md`

### Syntax

```
.BufferWithTolerance ( distance, tolerance, relative )
```

## CollectionAggregate (geography Data Type)

`docs/t-sql/spatial-geography/collectionaggregate-geography-data-type.md`

### Syntax

```
CollectionAggregate ( geography_operand )
```

## ConvexHullAggregate (geography Data Type)

`docs/t-sql/spatial-geography/convexhullaggregate-geography-data-type.md`

### Syntax

```
ConvexHullAggregate ( geography_operand )
```

## CurveToLineWithTolerance (geography Data Type)

`docs/t-sql/spatial-geography/curvetolinewithtolerance-geography-data-type.md`

### Syntax

```
.CurveToLineWithTolerance( tolerance, relative )
```

## EnvelopeAggregate (geography Data Type)

`docs/t-sql/spatial-geography/envelopeaggregate-geography-data-type.md`

### Syntax

```
EnvelopeAggregate ( geography_operand )
```

## EnvelopeAngle (geography Data Type)

`docs/t-sql/spatial-geography/envelopeangle-geography-data-type.md`

### Syntax

```
EnvelopeAngle( )
```

## EnvelopeCenter (geography Data Type)

`docs/t-sql/spatial-geography/envelopecenter-geography-data-type.md`

### Syntax

```
EnvelopeCenter( )
```

## Filter (geography Data Type)

`docs/t-sql/spatial-geography/filter-geography-data-type.md`

### Syntax

```
.Filter ( other_geography )
```

## GeomFromGML (geography Data Type)

`docs/t-sql/spatial-geography/geomfromgml-geography-data-type.md`

### Syntax

```
GeomFromGml ( GML_input, SRID )
```

## HasM (geography Data Type)

`docs/t-sql/spatial-geography/hasm-geography-data-type.md`

### Syntax

```sql
.HasM
```

## HasZ (geography Data Type)

`docs/t-sql/spatial-geography/hasz-geography-data-type.md`

### Syntax

```
.HasZ
```

## InstanceOf (geography Data Type)

`docs/t-sql/spatial-geography/instanceof-geography-data-type.md`

### Syntax

```sql
.InstanceOf ( 'geography_type')
```

## IsNull (geography Data Type)

`docs/t-sql/spatial-geography/isnull-geography-data-type.md`

### Syntax

```
.IsNull
```

## IsValidDetailed (geography Data Type)

`docs/t-sql/spatial-geography/isvaliddetailed-geography-data-type.md`

### Syntax

```
.IsValidDetailed()
```

## Lat (geography Data Type)

`docs/t-sql/spatial-geography/lat-geography-data-type.md`

### Syntax

```
.Lat
```

## Long (geography Data Type)

`docs/t-sql/spatial-geography/long-geography-data-type.md`

### Syntax

```syntaxsql
.Long
```

## M (geography Data Type)

`docs/t-sql/spatial-geography/m-geography-data-type.md`

### Syntax

```
.M
```

## MakeValid (geography Data Type)

`docs/t-sql/spatial-geography/makevalid-geography-data-type.md`

### Syntax

```
.MakeValid ()
```

## MinDbCompatibilityLevel (geography Data Type)

`docs/t-sql/spatial-geography/mindbcompatibilitylevel-geography-data-type.md`

### Syntax

```
. MinDbCompatibilityLevel ( )
```

## Null (geography Data Type)

`docs/t-sql/spatial-geography/null-geography-data-type.md`

### Syntax

```
Null
```

## NumRings (geography Data Type)

`docs/t-sql/spatial-geography/numrings-geography-data-type.md`

### Syntax

```
.NumRings ()
```

## Parse (geography data type)

`docs/t-sql/spatial-geography/parse-geography-data-type.md`

### Syntax

```syntaxsql
Parse ( 'geography_tagged_text' )
```

## Point (geography Data Type)

`docs/t-sql/spatial-geography/point-geography-data-type.md`

### Syntax

```
Point ( Lat, Long, SRID )
```

## Reduce (geography Data Type)

`docs/t-sql/spatial-geography/reduce-geography-data-type.md`

### Syntax

```
.Reduce ( tolerance )
```

## ReorientObject (geography Data Type)

`docs/t-sql/spatial-geography/reorientobject-geography-data-type.md`

### Syntax

```syntaxsql
.ReorientObject (geography)
```

## RingN (geography Data Type)

`docs/t-sql/spatial-geography/ringn-geography-data-type.md`

### Syntax

```syntaxsql
.RingN (expression )
```

## ShortestLineTo (geography Data Type)

`docs/t-sql/spatial-geography/shortestlineto-geography-data-type.md`

### Syntax

```
.ShortestLineTo ( geography_other )
```

## STArea (geography Data Type)

`docs/t-sql/spatial-geography/starea-geography-data-type.md`

### Syntax

```
.STArea ( )
```

## STAsBinary (geography Data Type)

`docs/t-sql/spatial-geography/stasbinary-geography-data-type.md`

### Syntax

```
.STAsBinary ( )
```

## STAsText (geography Data Type)

`docs/t-sql/spatial-geography/stastext-geography-data-type.md`

### Syntax

```
.STAsText ( )
```

## STBuffer (geography Data Type)

`docs/t-sql/spatial-geography/stbuffer-geography-data-type.md`

### Syntax

```
.STBuffer ( distance )
```

## STContains  (geography Data Type)

`docs/t-sql/spatial-geography/stcontains-geography-data-type.md`

### Syntax

```
.STContains ( other_geography )
```

## STConvexHull (geography Data Type)

`docs/t-sql/spatial-geography/stconvexhull-geography-data-type.md`

### Syntax

```
.STConvexHull ( )
```

## STCurveN (geography Data Type)

`docs/t-sql/spatial-geography/stcurven-geography-data-type.md`

### Syntax

```
.STCurveN( n )
```

## STCurveToLine (geography Data Type)

`docs/t-sql/spatial-geography/stcurvetoline-geography-data-type.md`

### Syntax

```
.STCurveToLine()
```

## STDifference (geography Data Type)

`docs/t-sql/spatial-geography/stdifference-geography-data-type.md`

### Syntax

```
.STDifference ( other_geography )
```

## STDimension (geography Data Type)

`docs/t-sql/spatial-geography/stdimension-geography-data-type.md`

### Syntax

```
.STDimension ( )
```

## STDisjoint (geography Data Type)

`docs/t-sql/spatial-geography/stdisjoint-geography-data-type.md`

### Syntax

```
.STDisjoint ( other_geography )
```

## STDistance (geography Data Type)

`docs/t-sql/spatial-geography/stdistance-geography-data-type.md`

### Syntax

```
.STDistance ( other_geography )
```

## STEndPoint (geography Data Type)

`docs/t-sql/spatial-geography/stendpoint-geography-data-type.md`

### Syntax

```
.STEndPoint ( )
```

## STEquals (geography Data Type)

`docs/t-sql/spatial-geography/stequals-geography-data-type.md`

### Syntax

```
.STEquals ( other_geography )
```

## STGeomCollFromText (geography Data Type)

`docs/t-sql/spatial-geography/stgeomcollfromtext-geography-data-type.md`

### Syntax

```
STGeomCollFromText ( 'geometrycollection_tagged_text' , SRID )
```

## STGeomCollFromWKB (geography Data Type)

`docs/t-sql/spatial-geography/stgeomcollfromwkb-geography-data-type.md`

### Syntax

```
STGeomCollFromWKB ( 'WKB_geometrycollection' , SRID )
```

## STGeometryN (geography Data Type)

`docs/t-sql/spatial-geography/stgeometryn-geography-data-type.md`

### Syntax

```
.STGeometryN ( expression )
```

## STGeometryType (geography Data Type)

`docs/t-sql/spatial-geography/stgeometrytype-geography-data-type.md`

### Syntax

```
.STGeometryType ( )
```

## STGeomFromText (geography data type)

`docs/t-sql/spatial-geography/stgeomfromtext-geography-data-type.md`

### Syntax

```syntaxsql
STGeomFromText ( 'geography_tagged_text' , SRID )
```

## STGeomFromWKB (geography Data Type)

`docs/t-sql/spatial-geography/stgeomfromwkb-geography-data-type.md`

### Syntax

```
STGeomFromWKB ( 'WKB_geography' , SRID )
```

## STIntersection (geography Data Type)

`docs/t-sql/spatial-geography/stintersection-geography-data-type.md`

### Syntax

```
.STIntersection ( other_geography )
```

## STIntersects (geography Data Type)

`docs/t-sql/spatial-geography/stintersects-geography-data-type.md`

### Syntax

```syntaxsql
.STIntersects ( other_geography )
```

## STIsClosed (geography Data Type)

`docs/t-sql/spatial-geography/stisclosed-geography-data-type.md`

### Syntax

```
.STIsClosed ( )
```

## STIsEmpty (geography Data Type)

`docs/t-sql/spatial-geography/stisempty-geography-data-type.md`

### Syntax

```
.STIsEmpty ( )
```

## STIsValid (geography Data Type)

`docs/t-sql/spatial-geography/stisvalid-geography-data-type.md`

### Syntax

```
.STIsValid ( )
```

## STLength (geography Data Type)

`docs/t-sql/spatial-geography/stlength-geography-data-type.md`

### Syntax

```
.STLength ( )
```

## STLineFromText (geography Data Type)

`docs/t-sql/spatial-geography/stlinefromtext-geography-data-type.md`

### Syntax

```
STLineFromText ( 'linestring_tagged_text' , SRID )
```

## STLineFromWKB (geography Data Type)

`docs/t-sql/spatial-geography/stlinefromwkb-geography-data-type.md`

### Syntax

```
STLineFromWKB ( 'WKB_linestring' , SRID )
```

## STMLineFromText (geography Data Type)

`docs/t-sql/spatial-geography/stmlinefromtext-geography-data-type.md`

### Syntax

```
STMLineFromText ( 'multilinestring_tagged_text' , SRID )
```

## STMLineFromWKB (geography Data Type)

`docs/t-sql/spatial-geography/stmlinefromwkb-geography-data-type.md`

### Syntax

```
STMLineFromWKB ( 'WKB_multilinestring' , SRID )
```

## STMPointFromText (geography Data Type)

`docs/t-sql/spatial-geography/stmpointfromtext-geography-data-type.md`

### Syntax

```
STMPointFromText ( 'multipoint_tagged_text', SRID )
```

## STMPointFromWKB (geography Data Type)

`docs/t-sql/spatial-geography/stmpointfromwkb-geography-data-type.md`

### Syntax

```
STMPointFromWKB ( 'WKB_multipoint' , SRID )
```

## STMPolyFromText (geography Data Type)

`docs/t-sql/spatial-geography/stmpolyfromtext-geography-data-type.md`

### Syntax

```
STMPolyFromText ( 'multipolygon_tagged_text' , SRID )
```

## STMPolyFromWKB (geography Data Type)

`docs/t-sql/spatial-geography/stmpolyfromwkb-geography-data-type.md`

### Syntax

```
STMPolyFromWKB ( 'WKB_multipolygon' , SRID )
```

## STNumCurves (geography Data Type)

`docs/t-sql/spatial-geography/stnumcurves-geography-data-type.md`

### Syntax

```
.STNumCurves()
```

## STNumGeometries (geography Data Type)

`docs/t-sql/spatial-geography/stnumgeometries-geography-data-type.md`

### Syntax

```
.STNumGeometries ( )
```

## STNumPoints (geography Data Type)

`docs/t-sql/spatial-geography/stnumpoints-geography-data-type.md`

### Syntax

```
.STNumPoints ( )
```

## STOverlaps (geography Data Type)

`docs/t-sql/spatial-geography/stoverlaps-geography-data-type.md`

### Syntax

```
.STOverlaps ( other_geography )
```

## STPointFromText (geography Data Type)

`docs/t-sql/spatial-geography/stpointfromtext-geography-data-type.md`

### Syntax

```
STPointFromText ( 'point_tagged_text' , SRID )
```

## STPointFromWKB (geography Data Type)

`docs/t-sql/spatial-geography/stpointfromwkb-geography-data-type.md`

### Syntax

```
STPointFromWKB ( 'WKB_point' , SRID )
```

## STPointN (geography Data Type)

`docs/t-sql/spatial-geography/stpointn-geography-data-type.md`

### Syntax

```
.STPointN ( expression )
```

## STPolyFromText (geography Data Type)

`docs/t-sql/spatial-geography/stpolyfromtext-geography-data-type.md`

### Syntax

```
STPolyFromText ( 'polygon_tagged_text' , SRID )
```

## STPolyFromWKB (geography Data Type)

`docs/t-sql/spatial-geography/stpolyfromwkb-geography-data-type.md`

### Syntax

```
STPolyFromWKB ( 'WKB_polygon' , SRID )
```

## STSrid (geography Data Type)

`docs/t-sql/spatial-geography/stsrid-geography-data-type.md`

### Syntax

```
.STSrid
```

## STStartPoint (geography Data Type)

`docs/t-sql/spatial-geography/ststartpoint-geography-data-type.md`

### Syntax

```
.STStartPoint ( )
```

## STSymDifference (geography Data Type)

`docs/t-sql/spatial-geography/stsymdifference-geography-data-type.md`

### Syntax

```
.STSymDifference ( other_geography )
```

## STUnion (geography Data Type)

`docs/t-sql/spatial-geography/stunion-geography-data-type.md`

### Syntax

```
.STUnion ( other_geography )
```

## STWithin (geography Data Type)

`docs/t-sql/spatial-geography/stwithin-geography-data-type.md`

### Syntax

```
.STWithin ( other_geography )
```

## ToString (geography Data Type)

`docs/t-sql/spatial-geography/tostring-geography-data-type.md`

### Syntax

```
.ToString ()
```

## UnionAggregate (geography Data Type)

`docs/t-sql/spatial-geography/unionaggregate-geography-data-type.md`

### Syntax

```
UnionAggregate ( geography_operand )
```

## Z (geography Data Type)

`docs/t-sql/spatial-geography/z-geography-data-type.md`

### Syntax

```
.Z
```

## AsBinaryZM (geometry DataType)

`docs/t-sql/spatial-geometry/asbinaryzm-geometry-datatype.md`

### Syntax

```
.AsBinaryZM()
```

## AsGml (geometry Data Type)

`docs/t-sql/spatial-geometry/asgml-geometry-data-type.md`

### Syntax

```sql
.AsGml ( )
```

## AsTextZM (geometry Data Type)

`docs/t-sql/spatial-geometry/astextzm-geometry-data-type.md`

### Syntax

```sql
.AsTextZM ()
```

## BufferWithCurves (geometry Data Type)

`docs/t-sql/spatial-geometry/bufferwithcurves-geometry-data-type.md`

### Syntax

```syntaxsql
.BufferWithCurves ( distance )
```

## BufferWithTolerance (geometry Data Type)

`docs/t-sql/spatial-geometry/bufferwithtolerance-geometry-data-type.md`

### Syntax

```
.BufferWithTolerance ( distance, tolerance, relative )
```

## CollectionAggregate (geometry Data Type)

`docs/t-sql/spatial-geometry/collectionaggregate-geometry-data-type.md`

### Syntax

```
CollectionAggregate ( geometry_operand )
```

## ConvexHullAggregate (geometry Data Type)

`docs/t-sql/spatial-geometry/convexhullaggregate-geometry-data-type.md`

### Syntax

```
ConvexHullAggregate ( geometry_operand )
```

## CurveToLineWithTolerance (geometry Data Type)

`docs/t-sql/spatial-geometry/curvetolinewithtolerance-geometry-data-type.md`

### Syntax

```
.CurveToLineWithTolerance ( tolerance, relative )
```

## EnvelopeAggregate (geometry Data Type)

`docs/t-sql/spatial-geometry/envelopeaggregate-geometry-data-type.md`

### Syntax

```
EnvelopeAggregate ( geometry_operand )
```

## Filter (geometry Data Type)

`docs/t-sql/spatial-geometry/filter-geometry-data-type.md`

### Syntax

```
.Filter ( other_geometry )
```

## GeomFromGml (geometry Data Type)

`docs/t-sql/spatial-geometry/geomfromgml-geometry-data-type.md`

### Syntax

```
GeomFromGml ( GML_input, SRID )
```

## HasM (geometry DataType)

`docs/t-sql/spatial-geometry/hasm-geometry-datatype.md`

### Syntax

```
.HasM
```

## HasZ (geometry DataType)

`docs/t-sql/spatial-geometry/hasz-geometry-datatype.md`

### Syntax

```
.HasZ
```

## InstanceOf (geometry Data Type)

`docs/t-sql/spatial-geometry/instanceof-geometry-data-type.md`

### Syntax

```
.InstanceOf (geometry_type )
```

## IsNull (geometry Data Type)

`docs/t-sql/spatial-geometry/isnull-geometry-data-type.md`

### Syntax

```
.IsNull
```

## IsValidDetailed (geometry DataType)

`docs/t-sql/spatial-geometry/isvaliddetailed-geometry-datatype.md`

### Syntax

```
.IsValidDetailed()
```

## M (geometry Data Type)

`docs/t-sql/spatial-geometry/m-geometry-data-type.md`

### Syntax

```
.M
```

## MakeValid (geometry Data Type)

`docs/t-sql/spatial-geometry/makevalid-geometry-data-type.md`

### Syntax

```
.MakeValid ()
```

## MinDbCompatibilityLevel (geometry Data Type)

`docs/t-sql/spatial-geometry/mindbcompatibilitylevel-geometry-data-type.md`

### Syntax

```
.MinDbCompatibilityLevel ( )
```

## Null (geometry Data Type)

`docs/t-sql/spatial-geometry/null-geometry-data-type.md`

### Syntax

```
Null
```

## Parse (geometry Data Type)

`docs/t-sql/spatial-geometry/parse-geometry-data-type.md`

### Syntax

```
Parse ( 'geometry_tagged_text' )
```

## Point (geometry Data Type)

`docs/t-sql/spatial-geometry/point-geometry-data-type.md`

### Syntax

```
Point ( X, Y, SRID )
```

## Reduce (geometry Data Type)

`docs/t-sql/spatial-geometry/reduce-geometry-data-type.md`

### Syntax

```
.Reduce ( tolerance )
```

## ShortestLineTo (geometry Data Type)

`docs/t-sql/spatial-geometry/shortestlineto-geometry-data-type.md`

### Syntax

```
.ShortestLineTo ( geometry_other )
```

## STArea (geometry Data Type)

`docs/t-sql/spatial-geometry/starea-geometry-data-type.md`

### Syntax

```syntaxsql
.STArea ( )
```

## STAsBinary (geometry Data Type)

`docs/t-sql/spatial-geometry/stasbinary-geometry-data-type.md`

### Syntax

```
.STAsBinary ( )
```

## STAsText (geometry Data Type)

`docs/t-sql/spatial-geometry/stastext-geometry-data-type.md`

### Syntax

```
.STAsText ( )
```

## STBoundary (geometry Data Type)

`docs/t-sql/spatial-geometry/stboundary-geometry-data-type.md`

### Syntax

```
.STBoundary ( )
```

## STBuffer (geometry Data Type)

`docs/t-sql/spatial-geometry/stbuffer-geometry-data-type.md`

### Syntax

```
.STBuffer ( distance )
```

## STCentroid (geometry Data Type)

`docs/t-sql/spatial-geometry/stcentroid-geometry-data-type.md`

### Syntax

```
.STCentroid ( )
```

## STContains (geometry Data Type)

`docs/t-sql/spatial-geometry/stcontains-geometry-data-type.md`

### Syntax

```
.STContains ( other_geometry )
```

## STConvexHull (geometry Data Type)

`docs/t-sql/spatial-geometry/stconvexhull-geometry-data-type.md`

### Syntax

```
.STConvexHull ( )
```

## STCrosses (geometry Data Type)

`docs/t-sql/spatial-geometry/stcrosses-geometry-data-type.md`

### Syntax

```
.STCrosses ( other_geometry )
```

## STCurveN (geometry Data Type)

`docs/t-sql/spatial-geometry/stcurven-geometry-data-type.md`

### Syntax

```
.STCurveN ( curve_index )
```

## STCurveToLine (geometry Data Type)

`docs/t-sql/spatial-geometry/stcurvetoline-geometry-data-type.md`

### Syntax

```
.STCurveToLine ( )
```

## STDifference (geometry Data Type)

`docs/t-sql/spatial-geometry/stdifference-geometry-data-type.md`

### Syntax

```
.STDifference ( other_geometry )
```

## STDimension (geometry Data Type)

`docs/t-sql/spatial-geometry/stdimension-geometry-data-type.md`

### Syntax

```
.STDimension ( )
```

## STDisjoint (geometry Data Type)

`docs/t-sql/spatial-geometry/stdisjoint-geometry-data-type.md`

### Syntax

```
.STDisjoint ( other_geometry )
```

## STDistance (geometry Data Type)

`docs/t-sql/spatial-geometry/stdistance-geometry-data-type.md`

### Syntax

```
.STDistance ( other_geometry )
```

## STEndpoint (geometry Data Type)

`docs/t-sql/spatial-geometry/stendpoint-geometry-data-type.md`

### Syntax

```
.STEndPoint ( )
```

## STEnvelope (geometry Data Type)

`docs/t-sql/spatial-geometry/stenvelope-geometry-data-type.md`

### Syntax

```
STEnvelope ( )
```

## STEquals (geometry Data Type)

`docs/t-sql/spatial-geometry/stequals-geometry-data-type.md`

### Syntax

```
.STEquals ( other_geometry )
```

## STExteriorRing (geometry Data Type)

`docs/t-sql/spatial-geometry/stexteriorring-geometry-data-type.md`

### Syntax

```
.STExteriorRing ( )
```

## STGeomCollFromText (geometry Data Type)

`docs/t-sql/spatial-geometry/stgeomcollfromtext-geometry-data-type.md`

### Syntax

```
STGeomCollFromText ( 'geometrycollection_tagged_text' , SRID )
```

## STGeomCollFromWKB (geometry Data Type)

`docs/t-sql/spatial-geometry/stgeomcollfromwkb-geometry-data-type.md`

### Syntax

```
STGeomCollFromWKB ( 'WKB_geometrycollection' , SRID )
```

## STGeometryN (geometry Data Type)

`docs/t-sql/spatial-geometry/stgeometryn-geometry-data-type.md`

### Syntax

```
.STGeometryN ( expression )
```

## STGeometryType (geometry Data Type)

`docs/t-sql/spatial-geometry/stgeometrytype-geometry-data-type.md`

### Syntax

```
.STGeometryType ( )
```

## STGeomFromText (geometry Data Type)

`docs/t-sql/spatial-geometry/stgeomfromtext-geometry-data-type.md`

### Syntax

```
STGeomFromText ( 'geometry_tagged_text' , SRID )
```

## STGeomFromWKB (geometry Data Type)

`docs/t-sql/spatial-geometry/stgeomfromwkb-geometry-data-type.md`

### Syntax

```
STGeomFromWKB ( 'WKB_geometry' , SRID )
```

## STInteriorRingN (geometry Data Type)

`docs/t-sql/spatial-geometry/stinteriorringn-geometry-data-type.md`

### Syntax

```
.STInteriorRingN ( expression )
```

## STIntersection (geometry Data Type)

`docs/t-sql/spatial-geometry/stintersection-geometry-data-type.md`

### Syntax

```
.STIntersection ( other_geometry )
```

## STIntersects (geometry Data Type)

`docs/t-sql/spatial-geometry/stintersects-geometry-data-type.md`

### Syntax

```
.STIntersects ( other_geometry )
```

## STIsClosed (geometry Data Type)

`docs/t-sql/spatial-geometry/stisclosed-geometry-data-type.md`

### Syntax

```
.STIsClosed ( )
```

## STIsEmpty (geometry Data Type)

`docs/t-sql/spatial-geometry/stisempty-geometry-data-type.md`

### Syntax

```
.STIsEmpty ( )
```

## STIsRing (geometry Data Type)

`docs/t-sql/spatial-geometry/stisring-geometry-data-type.md`

### Syntax

```
.STIsRing ( )
```

## STIsSimple (geometry Data Type)

`docs/t-sql/spatial-geometry/stissimple-geometry-data-type.md`

### Syntax

```
.STIsSimple ( )
```

## STIsValid (geometry Data Type)

`docs/t-sql/spatial-geometry/stisvalid-geometry-data-type.md`

### Syntax

```
.STIsValid ( )
```

## STLength (geometry Data Type)

`docs/t-sql/spatial-geometry/stlength-geometry-data-type.md`

### Syntax

```
.STLength ( )
```

## STLineFromText (geometry Data Type)

`docs/t-sql/spatial-geometry/stlinefromtext-geometry-data-type.md`

### Syntax

```
STLineFromText ( 'linestring_tagged_text' , SRID )
```

## STLineFromWKB (geometry Data Type)

`docs/t-sql/spatial-geometry/stlinefromwkb-geometry-data-type.md`

### Syntax

```
STLineFromWKB ( 'WKB_linestring' , SRID )
```

## STMLineFromText (geometry Data Type)

`docs/t-sql/spatial-geometry/stmlinefromtext-geometry-data-type.md`

### Syntax

```
STMLineFromText ( 'multilinestring_tagged_text' , SRID )
```

## STMLineFromWKB (geometry Data Type)

`docs/t-sql/spatial-geometry/stmlinefromwkb-geometry-data-type.md`

### Syntax

```
STMLineFromWKB ( 'WKB_multilinestring' , SRID )
```

## STMPointFromText (geometry Data Type)

`docs/t-sql/spatial-geometry/stmpointfromtext-geometry-data-type.md`

### Syntax

```
STMPointFromText ( 'multipoint_tagged_text', SRID )
```

## STMPointFromWKB (geometry Data Type)

`docs/t-sql/spatial-geometry/stmpointfromwkb-geometry-data-type.md`

### Syntax

```
STMPointFromWKB ( 'WKB_multipoint' , SRID )
```

## STMPolyFromText (geometry Data Type)

`docs/t-sql/spatial-geometry/stmpolyfromtext-geometry-data-type.md`

### Syntax

```
STMPolyFromText ( 'multipolygon_tagged_text' , SRID )
```

## STMPolyFromWKB (geometry Data Type)

`docs/t-sql/spatial-geometry/stmpolyfromwkb-geometry-data-type.md`

### Syntax

```
STMPolyFromWKB ( 'WKB_multipolygon' , SRID )
```

## STNumCurves (geometry Data Type)

`docs/t-sql/spatial-geometry/stnumcurves-geometry-data-type.md`

### Syntax

```
.STNumCurves()
```

## STNumGeometries (geometry Data Type)

`docs/t-sql/spatial-geometry/stnumgeometries-geometry-data-type.md`

### Syntax

```
.STNumGeometries ( )
```

## STNumInteriorRing (geometry Data Type)

`docs/t-sql/spatial-geometry/stnuminteriorring-geometry-data-type.md`

### Syntax

```
.STNumInteriorRing ( )
```

## STNumPoints (geometry Data Type)

`docs/t-sql/spatial-geometry/stnumpoints-geometry-data-type.md`

### Syntax

```
.STNumPoints ( )
```

## STOverlaps (geometry Data Type)

`docs/t-sql/spatial-geometry/stoverlaps-geometry-data-type.md`

### Syntax

```
.STOverlaps ( other_geometry )
```

## STPointFromText (geometry Data Type)

`docs/t-sql/spatial-geometry/stpointfromtext-geometry-data-type.md`

### Syntax

```
STPointFromText ( 'point_tagged_text' , SRID )
```

## STPointFromWKB (geometry Data Type)

`docs/t-sql/spatial-geometry/stpointfromwkb-geometry-data-type.md`

### Syntax

```
STPointFromWKB ( 'WKB_point' , SRID )
```

## STPointN (geometry Data Type)

`docs/t-sql/spatial-geometry/stpointn-geometry-data-type.md`

### Syntax

```
.STPointN ( expression )
```

## STPointOnSurface (geometry Data Type)

`docs/t-sql/spatial-geometry/stpointonsurface-geometry-data-type.md`

### Syntax

```
.STPointOnSurface ( )
```

## STPolyFromText (geometry Data Type)

`docs/t-sql/spatial-geometry/stpolyfromtext-geometry-data-type.md`

### Syntax

```
STPolyFromText ( 'polygon_tagged_text' , SRID )
```

## STPolyFromWKB (geometry Data Type)

`docs/t-sql/spatial-geometry/stpolyfromwkb-geometry-data-type.md`

### Syntax

```
STPolyFromWKB ( 'WKB_polygon' , SRID )
```

## STRelate (geometry Data Type)

`docs/t-sql/spatial-geometry/strelate-geometry-data-type.md`

### Syntax

```
.STRelate ( other_geometry, intersection_pattern_matrix )
```

## STSrid (geometry Data Type)

`docs/t-sql/spatial-geometry/stsrid-geometry-data-type.md`

### Syntax

```
STSrid
```

## STStartPoint (geometry Data Type)

`docs/t-sql/spatial-geometry/ststartpoint-geometry-data-type.md`

### Syntax

```
.STStartPoint ( )
```

## STSymDifference (geometry Data Type)

`docs/t-sql/spatial-geometry/stsymdifference-geometry-data-type.md`

### Syntax

```
.STSymDifference ( other_geometry )
```

## STTouches (geometry Data Type)

`docs/t-sql/spatial-geometry/sttouches-geometry-data-type.md`

### Syntax

```
.STTouches ( other_geometry )
```

## STUnion (geometry Data Type)

`docs/t-sql/spatial-geometry/stunion-geometry-data-type.md`

### Syntax

```
.STUnion ( other_geometry )
```

## STWithin (geometry Data Type)

`docs/t-sql/spatial-geometry/stwithin-geometry-data-type.md`

### Syntax

```
.STWithin ( other_geometry )
```

## STX (geometry Data Type)

`docs/t-sql/spatial-geometry/stx-geometry-data-type.md`

### Syntax

```
.STX
```

## STY (geometry Data Type)

`docs/t-sql/spatial-geometry/sty-geometry-data-type.md`

### Syntax

```syntaxsql
.STY
```

## ToString (geometry Data Type)

`docs/t-sql/spatial-geometry/tostring-geometry-data-type.md`

### Syntax

```
.ToString ()
```

## UnionAggregate (geometry Data Type)

`docs/t-sql/spatial-geometry/unionaggregate-geometry-data-type.md`

### Syntax

```
UnionAggregate ( geometry_operand )
```

## Z (geometry Data Type)

`docs/t-sql/spatial-geometry/z-geometry-data-type.md`

### Syntax

```
.Z
```

## ADD SENSITIVITY CLASSIFICATION (Transact-SQL)

`docs/t-sql/statements/add-sensitivity-classification-transact-sql.md`

### Syntax

```syntaxsql
    ADD SENSITIVITY CLASSIFICATION TO
    <object_name> [ , ...n ]
    WITH ( <sensitivity_option> [ , ...n ] )

<object_name> ::=
{
    [ schema_name. ] table_name.column_name
}

<sensitivity_option> ::=
{
    LABEL = string |
    LABEL_ID = guidOrString |
    INFORMATION_TYPE = string |
    INFORMATION_TYPE_ID = guidOrString |
    RANK = NONE | LOW | MEDIUM | HIGH | CRITICAL
}
```

## ADD SIGNATURE (Transact-SQL)

`docs/t-sql/statements/add-signature-transact-sql.md`

### Syntax

```syntaxsql
ADD [ COUNTER ] SIGNATURE TO module_class::module_name
    BY <crypto_list> [ , ...n ]

<crypto_list> ::=
    CERTIFICATE cert_name
    | CERTIFICATE cert_name [ WITH PASSWORD = 'password' ]
    | CERTIFICATE cert_name WITH SIGNATURE = signed_blob
    | ASYMMETRIC KEY Asym_Key_Name
    | ASYMMETRIC KEY Asym_Key_Name [ WITH PASSWORD = 'password' ]
    | ASYMMETRIC KEY Asym_Key_Name WITH SIGNATURE = signed_blob
```

## ALTER APPLICATION ROLE (Transact-SQL)

`docs/t-sql/statements/alter-application-role-transact-sql.md`

### Syntax

```syntaxsql
ALTER APPLICATION ROLE application_role_name
    WITH <set_item> [ , ...n ]

<set_item> ::=
    NAME = new_application_role_name
    | PASSWORD = 'password'
    | DEFAULT_SCHEMA = schema_name
```

## ALTER ASSEMBLY (Transact-SQL)

`docs/t-sql/statements/alter-assembly-transact-sql.md`

### Syntax

```syntaxsql
ALTER ASSEMBLY assembly_name
    [ FROM <client_assembly_specifier> | <assembly_bits> ]
    [ WITH <assembly_option> [ , ...n ] ]
    [ DROP FILE { file_name [ , ...n ] | ALL } ]
    [ ADD FILE FROM
    {
        client_file_specifier [ AS file_name ]
      | file_bits AS file_name
    } [ , ...n ]
    ] [ ; ]
<client_assembly_specifier> ::=
    '\\computer_name\share-name\ [ path\ ] manifest_file_name '
  | '[ local_path\ ] manifest_file_name'

<assembly_bits> ::=
    { varbinary_literal | varbinary_expression }

<assembly_option> ::=
    PERMISSION_SET = { SAFE | EXTERNAL_ACCESS | UNSAFE }
  | VISIBILITY = { ON | OFF }
  | UNCHECKED DATA
```

## ALTER ASYMMETRIC KEY (Transact-SQL)

`docs/t-sql/statements/alter-asymmetric-key-transact-sql.md`

### Syntax

```syntaxsql
ALTER ASYMMETRIC KEY Asym_Key_Name <alter_option>

<alter_option> ::=
      <password_change_option>
    | REMOVE PRIVATE KEY

<password_change_option> ::=
    WITH PRIVATE KEY ( <password_option> [ , <password_option> ] )

<password_option> ::=
      ENCRYPTION BY PASSWORD = 'strongPassword'
    | DECRYPTION BY PASSWORD = 'oldPassword'
```

## ALTER AUTHORIZATION (Transact-SQL)

`docs/t-sql/statements/alter-authorization-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server
ALTER AUTHORIZATION
    ON [ <class_type>:: ] entity_name
    TO { principal_name | SCHEMA OWNER }
    [;]

<class_type> ::=
     {
      OBJECT | ASSEMBLY | ASYMMETRIC KEY | AVAILABILITY GROUP | CERTIFICATE
    | CONTRACT | TYPE | DATABASE | ENDPOINT | FULLTEXT CATALOG
    | FULLTEXT STOPLIST | MESSAGE TYPE | REMOTE SERVICE BINDING
    | ROLE | ROUTE | SCHEMA | SEARCH PROPERTY LIST | SERVER ROLE
    | SERVICE | SYMMETRIC KEY | XML SCHEMA COLLECTION
     }
```

```syntaxsql
-- Syntax for SQL Database

ALTER AUTHORIZATION
    ON [ <class_type>:: ] entity_name
    TO { principal_name | SCHEMA OWNER }
    [;]

<class_type> ::=
     {
    OBJECT | ASSEMBLY | ASYMMETRIC KEY | CERTIFICATE
     | TYPE | DATABASE | FULLTEXT CATALOG
     | FULLTEXT STOPLIST
     | ROLE | SCHEMA | SEARCH PROPERTY LIST
     | SYMMETRIC KEY | XML SCHEMA COLLECTION
     }
```

```syntaxsql
-- Syntax for Azure Synapse Analytics and Microsoft Fabric

ALTER AUTHORIZATION ON
     [ <class_type> :: ] <entity_name>
     TO { principal_name | SCHEMA OWNER }
    [;]

    <class_type> ::= {
    SCHEMA
     | OBJECT
    }

    <entity_name> ::=
    {
    schema_name
     | [ schema_name. ] object_name
    }
```

```syntaxsql
-- Syntax for Parallel Data Warehouse

ALTER AUTHORIZATION ON
     [ <class_type> :: ] <entity_name>
     TO { principal_name | SCHEMA OWNER }
    [;]

<class_type> ::= {
    DATABASE
     | SCHEMA
     | OBJECT
    }

<entity_name> ::=
    {
    database_name
     | schema_name
     | [ schema_name. ] object_name
    }
```

## ALTER AVAILABILITY GROUP (Transact-SQL)

`docs/t-sql/statements/alter-availability-group-transact-sql.md`

### Syntax

```syntaxsql
ALTER AVAILABILITY GROUP group_name
  {
     SET ( <set_option_spec> )
   | ADD DATABASE database_name
   | REMOVE DATABASE database_name
   | ADD REPLICA ON <add_replica_spec>
   | MODIFY REPLICA ON <modify_replica_spec>
   | REMOVE REPLICA ON <server_instance>
   | JOIN
   | JOIN AVAILABILITY GROUP ON <add_availability_group_spec> [ , ...2 ]
   | MODIFY AVAILABILITY GROUP ON <modify_availability_group_spec> [ , ...2 ]
   | GRANT CREATE ANY DATABASE
   | DENY CREATE ANY DATABASE
   | FAILOVER
   | FORCE_FAILOVER_ALLOW_DATA_LOSS
   | ADD LISTENER 'dns_name' ( <add_listener_option> )
   | MODIFY LISTENER 'dns_name' ( <modify_listener_option> )
   | RESTART LISTENER 'dns_name'
   | REMOVE LISTENER 'dns_name'
   | OFFLINE
  }
[ ; ]

<set_option_spec> ::=
    AUTOMATED_BACKUP_PREFERENCE = { PRIMARY | SECONDARY_ONLY | SECONDARY | NONE }
  | FAILURE_CONDITION_LEVEL  = { 1 | 2 | 3 | 4 | 5 }
  | HEALTH_CHECK_TIMEOUT = milliseconds
  | DB_FAILOVER  = { ON | OFF }
  | DTC_SUPPORT  = { PER_DB | NONE }
  | REQUIRED_SYNCHRONIZED_SECONDARIES_TO_COMMIT = { integer }
  | ROLE = SECONDARY
  | CLUSTER_CONNECTION_OPTIONS = 'key_value_pairs> [ ;... ] '

<server_instance> ::=
 { 'system_name [ \instance_name ] ' | 'FCI_network_name [ \instance_name ] ' }

<add_replica_spec>::=
  <server_instance> WITH
    (
       ENDPOINT_URL = 'TCP://system-address:port' ,
       AVAILABILITY_MODE = { SYNCHRONOUS_COMMIT | ASYNCHRONOUS_COMMIT | CONFIGURATION_ONLY } ,
       FAILOVER_MODE = { AUTOMATIC | MANUAL }
       [ , <add_replica_option> [ , ...n ] ]
    )

  <add_replica_option>::=
       SEEDING_MODE = { AUTOMATIC | MANUAL }
     | BACKUP_PRIORITY = n
     | SECONDARY_ROLE ( {
            [ ALLOW_CONNECTIONS = { NO | READ_ONLY | ALL } ]
        [ , ] [ READ_ONLY_ROUTING_URL = 'TCP://system-address:port' ]
     } )
     | PRIMARY_ROLE ( {
            [ ALLOW_CONNECTIONS = { READ_WRITE | ALL } ]
        [ , ] [ READ_ONLY_ROUTING_LIST = { ( '<server_instance>' [ , ...n ] ) | NONE } ]
        [ , ] [ READ_WRITE_ROUTING_URL = 'TCP://system-address:port' ]
     } )
     | SESSION_TIMEOUT = integer

<modify_replica_spec>::=
  <server_instance> WITH
    (
       ENDPOINT_URL = 'TCP://system-address:port'
     | AVAILABILITY_MODE = { SYNCHRONOUS_COMMIT | ASYNCHRONOUS_COMMIT }
     | FAILOVER_MODE = { AUTOMATIC | MANUAL }
     | SEEDING_MODE = { AUTOMATIC | MANUAL }
     | BACKUP_PRIORITY = n
     | SECONDARY_ROLE ( {
          [ ALLOW_CONNECTIONS = { NO | READ_ONLY | ALL }  ]
        | [ READ_ONLY_ROUTING_URL = { 'TCP://system-address:port' | NONE } ]
          } )
     | PRIMARY_ROLE ( {
          [ ALLOW_CONNECTIONS = { READ_WRITE | ALL }   ]
        | [ READ_ONLY_ROUTING_LIST = { ( '<server_instance>' [ , ...n ] ) | NONE } ]
        | [ READ_WRITE_ROUTING_URL = { 'TCP://system-address:port' | NONE }  ]
          } )
     | SESSION_TIMEOUT = seconds
    )

<add_availability_group_spec>::=
 <ag_name> WITH
    (
       LISTENER_URL = 'TCP://system-address:port' ,
       AVAILABILITY_MODE = { SYNCHRONOUS_COMMIT | ASYNCHRONOUS_COMMIT } ,
       FAILOVER_MODE = MANUAL ,
       SEEDING_MODE = { AUTOMATIC | MANUAL }
    )

<modify_availability_group_spec>::=
 <ag_name> WITH
    (
       LISTENER = 'TCP://system-address:port'
       | AVAILABILITY_MODE = { SYNCHRONOUS_COMMIT | ASYNCHRONOUS_COMMIT }
       | SEEDING_MODE = { AUTOMATIC | MANUAL }
    )

<add_listener_option> ::=
   {
      WITH DHCP [ ON ( <network_subnet_option> ) ]
    | WITH IP ( { ( <ip_address_option> ) } [ , ...n ] ) [ , PORT = listener_port ]
   }

  <network_subnet_option> ::=
     'ipv4_address' , 'ipv4_mask'

  <ip_address_option> ::=
     {
        'four_part_ipv4_address' , 'four_part_ipv4_mask'
      | 'ipv6_address'
     }

<modify_listener_option>::=
    {
       ADD IP ( <ip_address_option> )
     | PORT = listener_port
     | REMOVE IP ( 'ipv4_address' | 'ipv6_address')
    }
```

## ALTER BROKER PRIORITY (Transact-SQL)

`docs/t-sql/statements/alter-broker-priority-transact-sql.md`

### Syntax

```syntaxsql
ALTER BROKER PRIORITY ConversationPriorityName
FOR CONVERSATION
{ SET ( [ CONTRACT_NAME = {ContractName | ANY } ]
        [ [ , ] LOCAL_SERVICE_NAME = {LocalServiceName | ANY } ]
        [ [ , ] REMOTE_SERVICE_NAME = {'RemoteServiceName' | ANY } ]
        [ [ , ] PRIORITY_LEVEL = { PriorityValue | DEFAULT } ]
              )
}
[;]
```

## ALTER CERTIFICATE (Transact-SQL)

`docs/t-sql/statements/alter-certificate-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server and Azure SQL Database

ALTER CERTIFICATE certificate_name
      REMOVE PRIVATE KEY
    | WITH PRIVATE KEY ( <private_key_spec> )
    | WITH ACTIVE FOR BEGIN_DIALOG = { ON | OFF }

<private_key_spec> ::=
      {
        { FILE = 'path_to_private_key' | BINARY = private_key_bits }
         [ , DECRYPTION BY PASSWORD = 'current_password' ]
         [ , ENCRYPTION BY PASSWORD = 'new_password' ]
      }
    |
      {
         [ DECRYPTION BY PASSWORD = 'current_password' ]
         [ [ , ] ENCRYPTION BY PASSWORD = 'new_password' ]
      }
```

```syntaxsql
-- Syntax for Parallel Data Warehouse

ALTER CERTIFICATE certificate_name
{
      REMOVE PRIVATE KEY
    | WITH PRIVATE KEY (
        FILE = '<path_to_private_key>',
        DECRYPTION BY PASSWORD = '<key password>' )
}
```

## ALTER COLUMN ENCRYPTION KEY (Transact-SQL)

`docs/t-sql/statements/alter-column-encryption-key-transact-sql.md`

### Syntax

```syntaxsql
ALTER COLUMN ENCRYPTION KEY key_name
    [ ADD | DROP ] VALUE
    (
        COLUMN_MASTER_KEY = column_master_key_name
        [, ALGORITHM = 'algorithm_name' , ENCRYPTED_VALUE =  varbinary_literal ]
    ) [;]
```

## ALTER CREDENTIAL (Transact-SQL)

`docs/t-sql/statements/alter-credential-transact-sql.md`

### Syntax

```syntaxsql
ALTER CREDENTIAL credential_name WITH IDENTITY = 'identity_name'
    [ , SECRET = 'secret' ]
```

## ALTER CRYPTOGRAPHIC PROVIDER (Transact-SQL)

`docs/t-sql/statements/alter-cryptographic-provider-transact-sql.md`

### Syntax

```syntaxsql
ALTER CRYPTOGRAPHIC PROVIDER provider_name
    [ FROM FILE = path_of_DLL ]
    ENABLE | DISABLE
```

## ALTER DATABASE AUDIT SPECIFICATION (Transact-SQL)

`docs/t-sql/statements/alter-database-audit-specification-transact-sql.md`

### Syntax

```syntaxsql
ALTER DATABASE AUDIT SPECIFICATION audit_specification_name
{
    [ FOR SERVER AUDIT audit_name ]
    [ { { ADD | DROP } (
           { <audit_action_specification> | audit_action_group_name }
                )
      } [, ...n] ]
    [ WITH ( STATE = { ON | OFF } ) ]
}
[ ; ]
<audit_action_specification>::=
{
      <action_specification>[ ,...n ] ON [ class :: ] securable
     BY principal [ ,...n ]
}
```

## ALTER DATABASE ENCRYPTION KEY (Transact-SQL)

`docs/t-sql/statements/alter-database-encryption-key-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server

ALTER DATABASE ENCRYPTION KEY
      REGENERATE WITH ALGORITHM = { AES_128 | AES_192 | AES_256 | TRIPLE_DES_3KEY }
   |
   ENCRYPTION BY SERVER
    {
        CERTIFICATE Encryptor_Name |
        ASYMMETRIC KEY Encryptor_Name
    }
[ ; ]
```

```syntaxsql
-- Syntax for Parallel Data Warehouse

ALTER DATABASE ENCRYPTION KEY
    {
      {
        REGENERATE WITH ALGORITHM = { AES_128 | AES_192 | AES_256 | TRIPLE_DES_3KEY }
        [ ENCRYPTION BY SERVER CERTIFICATE Encryptor_Name ]
      }
      |
      ENCRYPTION BY SERVER   CERTIFICATE Encryptor_Name
    }
[ ; ]
```

## ALTER DATABASE SCOPED CONFIGURATION

`docs/t-sql/statements/alter-database-scoped-configuration-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, SQL database in Microsoft Fabric, and Azure SQL Managed Instance:

```syntaxsql
ALTER DATABASE SCOPED CONFIGURATION
{
    { [ FOR SECONDARY ] SET <set_options> }
}
| CLEAR PROCEDURE_CACHE [plan_handle]
| SET < set_options >
[;]

< set_options > ::=
{
      ACCELERATED_PLAN_FORCING = { ON | OFF }
    | ALLOW_BUILTIN_TVF_IN_ALL_COMPAT_LEVELS = { ON | OFF }
    | ALLOW_STALE_VECTOR_INDEX = { ON | OFF }
    | ASYNC_STATS_UPDATE_WAIT_AT_LOW_PRIORITY = { ON | OFF }
    | BATCH_MODE_ADAPTIVE_JOINS = { ON | OFF }
    | BATCH_MODE_MEMORY_GRANT_FEEDBACK = { ON | OFF }
    | BATCH_MODE_ON_ROWSTORE = { ON | OFF }
    | CE_FEEDBACK = { ON | OFF }
    | DEFERRED_COMPILATION_TV = { ON | OFF }
    | DOP_FEEDBACK = { ON | OFF }
    | ELEVATE_ONLINE = { OFF | WHEN_SUPPORTED | FAIL_UNSUPPORTED }
    | ELEVATE_RESUMABLE = { OFF | WHEN_SUPPORTED | FAIL_UNSUPPORTED }
    | EXEC_QUERY_STATS_FOR_SCALAR_FUNCTIONS = { ON | OFF }
    | FORCE_SHOWPLAN_RUNTIME_PARAMETER_COLLECTION = { ON | OFF }
    | FULLTEXT_INDEX_VERSION = <version>
    | IDENTITY_CACHE = { ON | OFF }
    | INTERLEAVED_EXECUTION_TVF = { ON | OFF }
    | ISOLATE_SECURITY_POLICY_CARDINALITY  = { ON | OFF }
    | GLOBAL_TEMPORARY_TABLE_AUTO_DROP = { ON | OFF }
    | LAST_QUERY_PLAN_STATS = { ON | OFF }
    | LEDGER_DIGEST_STORAGE_ENDPOINT = { <endpoint URL string> | OFF }
    | LEGACY_CARDINALITY_ESTIMATION = { ON | OFF | PRIMARY }
    | LIGHTWEIGHT_QUERY_PROFILING = { ON | OFF }
    | MAXDOP = { <value> | PRIMARY }
    | MEMORY_GRANT_FEEDBACK_PERCENTILE_GRANT = { ON | OFF }
    | MEMORY_GRANT_FEEDBACK_PERSISTENCE = { ON | OFF }
    | OPTIMIZE_FOR_AD_HOC_WORKLOADS = { ON | OFF }
    | OPTIMIZED_PLAN_FORCING = { ON | OFF }
    | OPTIMIZED_SP_EXECUTESQL = { ON | OFF }
    | OPTIONAL_PARAMETER_OPTIMIZATION = { ON | OFF }
    | PARAMETER_SENSITIVE_PLAN_OPTIMIZATION = { ON | OFF }
    | PARAMETER_SNIFFING = { ON | OFF | PRIMARY }
    | PAUSED_RESUMABLE_INDEX_ABORT_DURATION_MINUTES = <time>
    | PREVIEW_FEATURES = { ON | OFF }
    | QUERY_OPTIMIZER_HOTFIXES = { ON | OFF | PRIMARY }
    | READABLE_SECONDARY_TEMPORARY_STATS_AUTO_CREATE = { ON | OFF | PRIMARY }
    | READABLE_SECONDARY_TEMPORARY_STATS_AUTO_UPDATE = { ON | OFF | PRIMARY }
    | ROW_MODE_MEMORY_GRANT_FEEDBACK = { ON | OFF }
    | TSQL_SCALAR_UDF_INLINING = { ON | OFF }
    | VERBOSE_TRUNCATION_WARNINGS = { ON | OFF }
    | XTP_PROCEDURE_EXECUTION_STATISTICS = { ON | OFF }
    | XTP_QUERY_EXECUTION_STATISTICS = { ON | OFF }
}
```

> Syntax for Azure Synapse Analytics:

```syntaxsql
ALTER DATABASE SCOPED CONFIGURATION
{
    SET <set_options>
}
[;]

< set_options > ::=
{
    DW_COMPATIBILITY_LEVEL = { AUTO | 10 | 20 | 30 | 40 | 50 | 9000 }
}
```

## ALTER DATABASE SCOPED CREDENTIAL (Transact-SQL)

`docs/t-sql/statements/alter-database-scoped-credential-transact-sql.md`

### Syntax

```syntaxsql
ALTER DATABASE SCOPED CREDENTIAL credential_name WITH IDENTITY = 'identity_name'
    [ , SECRET = 'secret' ]
```

## ALTER DATABASE Compatibility Level (Transact-SQL)

`docs/t-sql/statements/alter-database-transact-sql-compatibility-level.md`

### Syntax

```syntaxsql
ALTER DATABASE database_name
SET COMPATIBILITY_LEVEL = { 170 | 160 | 150 | 140 | 130 | 120 | 110 | 100 | 90 | 80 }
```

## ALTER DATABASE Database Mirroring (Transact-SQL)

`docs/t-sql/statements/alter-database-transact-sql-database-mirroring.md`

### Syntax

```syntaxsql
ALTER DATABASE database_name
SET { <partner_option> | <witness_option> }
  <partner_option> ::=
    PARTNER { = 'partner_server'
            | FAILOVER
            | FORCE_SERVICE_ALLOW_DATA_LOSS
            | OFF
            | RESUME
            | SAFETY { FULL | OFF }
            | SUSPEND
            | TIMEOUT integer
            }
  <witness_option> ::=
    WITNESS { = 'witness_server'
            | OFF
            }
```

## ALTER DATABASE File and Filegroups

`docs/t-sql/statements/alter-database-transact-sql-file-and-filegroup-options.md`

### Syntax

```syntaxsql
ALTER DATABASE database_name
{
    <add_or_modify_files>
  | <add_or_modify_filegroups>
}

<add_or_modify_files>::=
{
    ADD FILE <filespec> [ ,...n ]
        [ TO FILEGROUP { filegroup_name } ]
  | ADD LOG FILE <filespec> [ ,...n ]
  | REMOVE FILE logical_file_name
  | MODIFY FILE <filespec>
}

<filespec>::=
(
    NAME = logical_file_name
    [ , NEWNAME = new_logical_name ]
    [ , FILENAME = {'os_file_name' | 'filestream_path' | 'memory_optimized_data_path' } ]
    [ , SIZE = size [ KB | MB | GB | TB ] ]
    [ , MAXSIZE = { max_size [ KB | MB | GB | TB ] | UNLIMITED } ]
    [ , FILEGROWTH = growth_increment [ KB | MB | GB | TB| % ] ]
    [ , OFFLINE ]
)

<add_or_modify_filegroups>::=
{
    | ADD FILEGROUP filegroup_name
        [ CONTAINS FILESTREAM | CONTAINS MEMORY_OPTIMIZED_DATA ]
    | REMOVE FILEGROUP filegroup_name
    | MODIFY FILEGROUP filegroup_name
        { <filegroup_updatability_option>
        | DEFAULT
        | NAME = new_filegroup_name
        | { AUTOGROW_SINGLE_FILE | AUTOGROW_ALL_FILES }
        }
}
<filegroup_updatability_option>::=
{
    { READONLY | READWRITE }
    | { READ_ONLY | READ_WRITE }
}
```

### Syntax for Azure SQL Managed Instance

```syntaxsql
ALTER DATABASE database_name
{
    <add_or_modify_files>
  | <add_or_modify_filegroups>
}
[;]

<add_or_modify_files>::=
{
    ADD FILE <filespec> [ ,...n ]
        [ TO FILEGROUP { filegroup_name } ]
  | REMOVE FILE logical_file_name
  | MODIFY FILE <filespec>
}

<filespec>::=
(
    NAME = logical_file_name
    [ , SIZE = size [ KB | MB | GB | TB ] ]
    [ , MAXSIZE = { max_size [ KB | MB | GB | TB ] | UNLIMITED } ]
    [ , FILEGROWTH = growth_increment [ KB | MB | GB | TB| % ] ]
)

<add_or_modify_filegroups>::=
{
    | ADD FILEGROUP filegroup_name
    | REMOVE FILEGROUP filegroup_name
    | MODIFY FILEGROUP filegroup_name
        { <filegroup_updatability_option>
        | DEFAULT
        | NAME = new_filegroup_name
        | { AUTOGROW_SINGLE_FILE | AUTOGROW_ALL_FILES }
        }
}
<filegroup_updatability_option>::=
{
    { READONLY | READWRITE }
    | { READ_ONLY | READ_WRITE }
}
```

## ALTER DATABASE SET HADR (Transact-SQL)

`docs/t-sql/statements/alter-database-transact-sql-set-hadr.md`

### Syntax

```syntaxsql
ALTER DATABASE database_name
   SET HADR
   {
        { AVAILABILITY GROUP = group_name | OFF }
   | { SUSPEND | RESUME }
   }
[;]
```

## ALTER DATABASE SET Options (Transact-SQL)

`docs/t-sql/statements/alter-database-transact-sql-set-options.md`

### Syntax

```syntaxsql
ALTER DATABASE { database_name | CURRENT }
SET
{
    <option_spec> [ ,...n ] [ WITH <termination> ]
}

{ [ FOR SECONDARY ] SET <set_options> }

<option_spec> ::=
{
    <accelerated_database_recovery>
  | <auto_option>
  | <automatic_tuning_option>
  | <change_tracking_option>
  | <containment_option>
  | <cursor_option>
  | <data_retention_policy>
  | <database_mirroring_option>
  | <date_correlation_optimization_option>
  | <db_encryption_option>
  | <db_state_option>
  | <db_update_option>
  | <db_user_access_option>
  | <delayed_durability_option>
  | <external_access_option>
  | FILESTREAM ( <FILESTREAM_option> )
  | <HADR_options>
  | <mixed_page_allocation_option>
  | <optimized_locking>
  | <parameterization_option>
  | <query_store_options>
  | <recovery_option>
  | <remote_data_archive_option>
  | <persistent_log_buffer_option>
  | <service_broker_option>
  | <snapshot_option>
  | <sql_option>
  | <suspend_for_snapshot_backup>
  | <target_recovery_time_option>
  | <termination>
  | <temporal_history_retention>
}
;

<accelerated_database_recovery> ::=
{
    ACCELERATED_DATABASE_RECOVERY = { ON | OFF }
     [ ( PERSISTENT_VERSION_STORE_FILEGROUP = { filegroup name } ) ]
}

<auto_option> ::=
{
    AUTO_CLOSE { ON | OFF }
  | AUTO_CREATE_STATISTICS { OFF | ON [ ( INCREMENTAL = { ON | OFF } ) ] }
  | AUTO_SHRINK { ON | OFF }
  | AUTO_UPDATE_STATISTICS { ON | OFF }
  | AUTO_UPDATE_STATISTICS_ASYNC { ON | OFF }
}

<automatic_tuning_option> ::=
{
    AUTOMATIC_TUNING ( FORCE_LAST_GOOD_PLAN = { DEFAULT | ON | OFF } )
}

<change_tracking_option> ::=
{
    CHANGE_TRACKING
   {
       = OFF
     | = ON [ ( <change_tracking_option_list > [,...n] ) ]
     | ( <change_tracking_option_list> [,...n] )
   }
}

<change_tracking_option_list> ::=
{
   AUTO_CLEANUP = { ON | OFF }
 | CHANGE_RETENTION = retention_period { DAYS | HOURS | MINUTES }
}

<containment_option> ::=
   CONTAINMENT = { NONE | PARTIAL }

<cursor_option> ::=
{
    CURSOR_CLOSE_ON_COMMIT { ON | OFF }
  | CURSOR_DEFAULT { LOCAL | GLOBAL }
}

<database_mirroring_option>
  ALTER DATABASE Database Mirroring

<date_correlation_optimization_option> ::=
    DATE_CORRELATION_OPTIMIZATION { ON | OFF }

<db_encryption_option> ::=
    ENCRYPTION { ON | OFF | SUSPEND | RESUME }

<db_state_option> ::=
    { ONLINE | OFFLINE | EMERGENCY }

<db_update_option> ::=
    { READ_ONLY | READ_WRITE }

<db_user_access_option> ::=
    { SINGLE_USER | RESTRICTED_USER | MULTI_USER }

<delayed_durability_option> ::=
    DELAYED_DURABILITY = { DISABLED | ALLOWED | FORCED }

<external_access_option> ::=
{
    DB_CHAINING { ON | OFF }
  | TRUSTWORTHY { ON | OFF }
  | DEFAULT_FULLTEXT_LANGUAGE = { <lcid> | <language name> | <language alias> }
  | DEFAULT_LANGUAGE = { <lcid> | <language name> | <language alias> }
  | NESTED_TRIGGERS = { OFF | ON }
  | TRANSFORM_NOISE_WORDS = { OFF | ON }
  | TWO_DIGIT_YEAR_CUTOFF = { 1753, ..., 2049, ..., 9999 }
}

<FILESTREAM_option> ::=
{
    NON_TRANSACTED_ACCESS = { OFF | READ_ONLY | FULL
  | DIRECTORY_NAME = <directory_name>
}

<HADR_options> ::=
    ALTER DATABASE SET HADR

<mixed_page_allocation_option> ::=
    MIXED_PAGE_ALLOCATION { OFF | ON }

<parameterization_option> ::=
    PARAMETERIZATION { SIMPLE | FORCED }

<query_store_options> ::=
{
    QUERY_STORE
    {
          = OFF [ ( FORCED ) ]
        | = ON [ ( <query_store_option_list> [,...n] ) ]
        | ( < query_store_option_list> [,...n] )
        | CLEAR [ ALL ]
    }
}

<query_store_option_list> ::=
{
      OPERATION_MODE = { READ_WRITE | READ_ONLY }
    | CLEANUP_POLICY = ( STALE_QUERY_THRESHOLD_DAYS = number )
    | DATA_FLUSH_INTERVAL_SECONDS = number
    | MAX_STORAGE_SIZE_MB = number
    | INTERVAL_LENGTH_MINUTES = number
    | SIZE_BASED_CLEANUP_MODE = { AUTO | OFF }
    | QUERY_CAPTURE_MODE = { ALL | AUTO | CUSTOM | NONE }
    | MAX_PLANS_PER_QUERY = number
    | WAIT_STATS_CAPTURE_MODE = { ON | OFF }
    | QUERY_CAPTURE_POLICY = ( <query_capture_policy_option_list> [,...n] )
}

<query_capture_policy_option_list> ::=
{
      STALE_CAPTURE_POLICY_THRESHOLD = number { DAYS | HOURS }
    | EXECUTION_COUNT = number
    | TOTAL_COMPILE_CPU_TIME_MS = number
    | TOTAL_EXECUTION_CPU_TIME_MS = number
}

<recovery_option> ::=
{
    RECOVERY { FULL | BULK_LOGGED | SIMPLE }
  | TORN_PAGE_DETECTION { ON | OFF }
  | PAGE_VERIFY { CHECKSUM | TORN_PAGE_DETECTION | NONE }
}

<remote_data_archive_option> ::=
{
    REMOTE_DATA_ARCHIVE =
    {
        ON ( SERVER = <server_name>,
             {
                  CREDENTIAL = <db_scoped_credential_name>
                  | FEDERATED_SERVICE_ACCOUNT = ON | OFF
             }
        )
        | OFF
    }
}

<persistent_log_buffer_option> ::=
{
    PERSISTENT_LOG_BUFFER
    {
          = ON (DIRECTORY_NAME= 'path-to-directory-on-a-DAX-volume')
        | = OFF
    }
}

<service_broker_option> ::=
{
    ENABLE_BROKER
  | DISABLE_BROKER
  | NEW_BROKER
  | ERROR_BROKER_CONVERSATIONS
  | HONOR_BROKER_PRIORITY { ON | OFF }
}

<snapshot_option> ::=
{
    ALLOW_SNAPSHOT_ISOLATION { ON | OFF }
  | READ_COMMITTED_SNAPSHOT { ON | OFF }
  | MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT = { ON | OFF }
}

<sql_option> ::=
{
    ANSI_NULL_DEFAULT { ON | OFF }
  | ANSI_NULLS { ON | OFF }
  | ANSI_PADDING { ON | OFF }
  | ANSI_WARNINGS { ON | OFF }
  | ARITHABORT { ON | OFF }
  | COMPATIBILITY_LEVEL = { 170 | 160 | 150 | 140 | 130 | 120 | 110 | 100 }
  | CONCAT_NULL_YIELDS_NULL { ON | OFF }
  | NUMERIC_ROUNDABORT { ON | OFF }
  | QUOTED_IDENTIFIER { ON | OFF }
  | RECURSIVE_TRIGGERS { ON | OFF }
}

<suspend_for_snapshot_backup> ::=
    SET SUSPEND_FOR_SNAPSHOT_BACKUP = { ON | OFF } [ ( MODE = COPY_ONLY ) ]

<target_recovery_time_option> ::=
    TARGET_RECOVERY_TIME = target_recovery_time { SECONDS | MINUTES }

<termination>::=
{
    ROLLBACK AFTER number [ SECONDS ]
  | ROLLBACK IMMEDIATE
  | NO_WAIT
}

<temporal_history_retention> ::=
    TEMPORAL_HISTORY_RETENTION { ON | OFF }

<data_retention_policy> ::=
    DATA_RETENTION { ON | OFF }

<optimized_locking> ::=
{
    OPTIMIZED_LOCKING = { ON | OFF }
}
```

```syntaxsql
ALTER DATABASE { database_name | CURRENT }
SET
{
    <option_spec> [ ,...n ] [ WITH <termination> ]
}
;

{ [ FOR SECONDARY ] SET <set_options> }

<option_spec> ::=
{
    <auto_option>
  | <automatic_tuning_option>
  | <change_tracking_option>
  | <cursor_option>
  | <db_encryption_option>
  | <db_update_option>
  | <db_user_access_option>
  | <delayed_durability_option>
  | <parameterization_option>
  | <query_store_options>
  | <snapshot_option>
  | <sql_option>
  | <termination>
  | <temporal_history_retention>
}
;

<auto_option> ::=
{
    AUTO_CREATE_STATISTICS { OFF | ON [ ( INCREMENTAL = { ON | OFF } ) ] }
  | AUTO_SHRINK { ON | OFF }
  | AUTO_UPDATE_STATISTICS { ON | OFF }
  | AUTO_UPDATE_STATISTICS_ASYNC { ON | OFF }
}

<automatic_tuning_option> ::=
{
    AUTOMATIC_TUNING = { AUTO | INHERIT | CUSTOM }
  | AUTOMATIC_TUNING ( CREATE_INDEX = { DEFAULT | ON | OFF } )
  | AUTOMATIC_TUNING ( DROP_INDEX = { DEFAULT | ON | OFF } )
  | AUTOMATIC_TUNING ( FORCE_LAST_GOOD_PLAN = { DEFAULT | ON | OFF } )
}

<change_tracking_option> ::=
{
    CHANGE_TRACKING
    {
        = OFF
      | = ON [ ( <change_tracking_option_list > [,...n] ) ]
      | ( <change_tracking_option_list> [,...n] )
    }
}

<change_tracking_option_list> ::=
   {
       AUTO_CLEANUP = { ON | OFF }
     | CHANGE_RETENTION = retention_period { DAYS | HOURS | MINUTES }
   }

<cursor_option> ::=
{
    CURSOR_CLOSE_ON_COMMIT { ON | OFF }
}

<db_encryption_option> ::=
  ENCRYPTION { ON | OFF }

<db_update_option> ::=
  { READ_ONLY | READ_WRITE }

<db_user_access_option> ::=
  { RESTRICTED_USER | MULTI_USER }

<delayed_durability_option> ::= DELAYED_DURABILITY = { DISABLED | ALLOWED | FORCED }

<parameterization_option> ::=
  PARAMETERIZATION { SIMPLE | FORCED }

<query_store_options> ::=
{
  QUERY_STORE
  {
      = OFF
    | = ON [ ( <query_store_option_list> [,... n] ) ]
    | ( < query_store_option_list> [,... n] )
    | CLEAR [ ALL ]
  }
}

<query_store_option_list> ::=
{
  OPERATION_MODE = { READ_WRITE | READ_ONLY }
  | CLEANUP_POLICY = ( STALE_QUERY_THRESHOLD_DAYS = number )
  | DATA_FLUSH_INTERVAL_SECONDS = number
  | MAX_STORAGE_SIZE_MB = number
  | INTERVAL_LENGTH_MINUTES = number
  | SIZE_BASED_CLEANUP_MODE = { AUTO | OFF }
  | QUERY_CAPTURE_MODE = { ALL | AUTO | CUSTOM | NONE }
  | MAX_PLANS_PER_QUERY = number
  | WAIT_STATS_CAPTURE_MODE = { ON | OFF }
  | QUERY_CAPTURE_POLICY = ( <query_capture_policy_option_list> [,...n] )
}

<query_capture_policy_option_list> ::=
{
    STALE_CAPTURE_POLICY_THRESHOLD = number { DAYS | HOURS }
    | EXECUTION_COUNT = number
    | TOTAL_COMPILE_CPU_TIME_MS = number
    | TOTAL_EXECUTION_CPU_TIME_MS = number
}

<snapshot_option> ::=
{
    ALLOW_SNAPSHOT_ISOLATION { ON | OFF }
  | READ_COMMITTED_SNAPSHOT { ON | OFF }
  | MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT { ON | OFF }
}
<sql_option> ::=
{
    ANSI_NULL_DEFAULT { ON | OFF }
  | ANSI_NULLS { ON | OFF }
  | ANSI_PADDING { ON | OFF }
  | ANSI_WARNINGS { ON | OFF }
  | ARITHABORT { ON | OFF }
  | COMPATIBILITY_LEVEL = { 170 | 160 | 150 | 140 | 130 | 120 | 110 | 100 }
  | CONCAT_NULL_YIELDS_NULL { ON | OFF }
  | NUMERIC_ROUNDABORT { ON | OFF }
  | QUOTED_IDENTIFIER { ON | OFF }
  | RECURSIVE_TRIGGERS { ON | OFF }
}

<termination>::=
{
    ROLLBACK AFTER integer [ SECONDS ]
  | ROLLBACK IMMEDIATE
  | NO_WAIT
}

<temporal_history_retention>::=TEMPORAL_HISTORY_RETENTION { ON | OFF }
```

```syntaxsql
ALTER DATABASE { database_name | CURRENT }
SET
{
    <option_spec> [ ,...n ] [ WITH <termination> ]
}
;

<option_spec> ::=
{
    <auto_option>
  | <automatic_tuning_option>
  | <change_tracking_option>
  | <cursor_option>
  | <db_update_option>
  | <db_user_access_option>
  | <delayed_durability_option>
  | <parameterization_option>
  | <query_store_options>
  | <snapshot_option>
  | <sql_option>
  | <termination>
  | <temporal_history_retention>
}
;

<auto_option> ::=
{
    AUTO_CREATE_STATISTICS { OFF | ON [ ( INCREMENTAL = { ON | OFF } ) ] }
  | AUTO_SHRINK { ON | OFF }
  | AUTO_UPDATE_STATISTICS { ON | OFF }
  | AUTO_UPDATE_STATISTICS_ASYNC { ON | OFF }
}

<automatic_tuning_option> ::=
{
    AUTOMATIC_TUNING = { AUTO | INHERIT | CUSTOM }
  | AUTOMATIC_TUNING ( CREATE_INDEX = { DEFAULT | ON | OFF } )
  | AUTOMATIC_TUNING ( DROP_INDEX = { DEFAULT | ON | OFF } )
  | AUTOMATIC_TUNING ( FORCE_LAST_GOOD_PLAN = { DEFAULT | ON | OFF } )
}

<change_tracking_option> ::=
{
    CHANGE_TRACKING
    {
        ( <change_tracking_option_list> [,...n] )
    }
}

<change_tracking_option_list> ::=
   {
       CHANGE_RETENTION = retention_period { DAYS | HOURS | MINUTES }
   }

<cursor_option> ::=
{
    CURSOR_CLOSE_ON_COMMIT { ON | OFF }
}

<db_update_option> ::=
  { READ_ONLY | READ_WRITE }

<db_user_access_option> ::=
  { RESTRICTED_USER | MULTI_USER }

<delayed_durability_option> ::= DELAYED_DURABILITY = { DISABLED | ALLOWED | FORCED }

<parameterization_option> ::=
  PARAMETERIZATION { SIMPLE | FORCED }

<query_store_options> ::=
{
  QUERY_STORE
  {
      = OFF
    | = ON [ ( <query_store_option_list> [,... n] ) ]
    | ( < query_store_option_list> [,... n] )
    | CLEAR [ ALL ]
  }
}

<query_store_option_list> ::=
{
  OPERATION_MODE = { READ_WRITE | READ_ONLY }
  | CLEANUP_POLICY = ( STALE_QUERY_THRESHOLD_DAYS = number )
  | DATA_FLUSH_INTERVAL_SECONDS = number
  | MAX_STORAGE_SIZE_MB = number
  | INTERVAL_LENGTH_MINUTES = number
  | SIZE_BASED_CLEANUP_MODE = { AUTO | OFF }
  | QUERY_CAPTURE_MODE = { ALL | AUTO | CUSTOM | NONE }
  | MAX_PLANS_PER_QUERY = number
  | WAIT_STATS_CAPTURE_MODE = { ON | OFF }
  | QUERY_CAPTURE_POLICY = ( <query_capture_policy_option_list> [,...n] )
}

<query_capture_policy_option_list> ::=
{
    STALE_CAPTURE_POLICY_THRESHOLD = number { DAYS | HOURS }
    | EXECUTION_COUNT = number
    | TOTAL_COMPILE_CPU_TIME_MS = number
    | TOTAL_EXECUTION_CPU_TIME_MS = number
}

<snapshot_option> ::=
{
    MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT { ON | OFF }
}
<sql_option> ::=
{
    ANSI_NULL_DEFAULT { ON | OFF }
  | ANSI_NULLS { ON | OFF }
  | ANSI_PADDING { ON | OFF }
  | ANSI_WARNINGS { ON | OFF }
  | ARITHABORT { ON | OFF }
  | COMPATIBILITY_LEVEL = { 170 | 160 | 150 | 140 | 130 | 120 | 110 | 100 }
  | CONCAT_NULL_YIELDS_NULL { ON | OFF }
  | NUMERIC_ROUNDABORT { ON | OFF }
  | QUOTED_IDENTIFIER { ON | OFF }
  | RECURSIVE_TRIGGERS { ON | OFF }
}

<termination>::=
{
    ROLLBACK AFTER integer [ SECONDS ]
  | ROLLBACK IMMEDIATE
  | NO_WAIT
}

<temporal_history_retention>::=TEMPORAL_HISTORY_RETENTION { ON | OFF }
```

```syntaxsql
ALTER DATABASE { database_name | CURRENT }
SET
{
    <optionspec> [ ,...n ]
}
;

{ [ FOR SECONDARY ] SET <set_options> }

<optionspec> ::=
{
    <auto_option>
  | <change_tracking_option>
  | <cursor_option>
  | <db_encryption_option>
  | <delayed_durability_option>
  | <parameterization_option>
  | <query_store_options>
  | <snapshot_option>
  | <sql_option>
  | <termination>
  | <temporal_history_retention>
}
;
<auto_option> ::=
{
    AUTO_CREATE_STATISTICS { OFF | ON [ ( INCREMENTAL = { ON | OFF } ) ] }
  | AUTO_SHRINK { ON | OFF }
  | AUTO_UPDATE_STATISTICS { ON | OFF }
  | AUTO_UPDATE_STATISTICS_ASYNC { ON | OFF }
}

<automatic_tuning_option> ::=
{
    AUTOMATIC_TUNING ( FORCE_LAST_GOOD_PLAN = { DEFAULT | ON | OFF } )
}

<change_tracking_option> ::=
{
    CHANGE_TRACKING
    {
       = OFF
     | = ON [ ( <change_tracking_option_list > [,...n] ) ]
     | ( <change_tracking_option_list> [,...n] )
    }
}

<change_tracking_option_list> ::=
   {
       AUTO_CLEANUP = { ON | OFF }
     | CHANGE_RETENTION = retention_period { DAYS | HOURS | MINUTES }
   }

<cursor_option> ::=
{
    CURSOR_CLOSE_ON_COMMIT { ON | OFF }
}

<db_encryption_option> ::=
  ENCRYPTION { ON | OFF }

<delayed_durability_option> ::=DELAYED_DURABILITY = { DISABLED | ALLOWED | FORCED }

<parameterization_option> ::=
  PARAMETERIZATION { SIMPLE | FORCED }

<query_store_options> ::=
{
  QUERY_STORE
  {
    = OFF
    | = ON [ ( <query_store_option_list> [,... n] ) ]
    | ( < query_store_option_list> [,... n] )
    | CLEAR [ ALL ]
  }
}

<query_store_option_list> ::=
{
  OPERATION_MODE = { READ_WRITE | READ_ONLY }
  | CLEANUP_POLICY = ( STALE_QUERY_THRESHOLD_DAYS = number )
  | DATA_FLUSH_INTERVAL_SECONDS = number
  | MAX_STORAGE_SIZE_MB = number
  | INTERVAL_LENGTH_MINUTES = number
  | SIZE_BASED_CLEANUP_MODE = { AUTO | OFF }
  | QUERY_CAPTURE_MODE = { ALL | AUTO | CUSTOM | NONE }
  | MAX_PLANS_PER_QUERY = number
  | WAIT_STATS_CAPTURE_MODE = { ON | OFF }
  | QUERY_CAPTURE_POLICY = ( <query_capture_policy_option_list> [,...n] )
}

<query_capture_policy_option_list> ::=
{
    STALE_CAPTURE_POLICY_THRESHOLD = number { DAYS | HOURS }
    | EXECUTION_COUNT = number
    | TOTAL_COMPILE_CPU_TIME_MS = number
    | TOTAL_EXECUTION_CPU_TIME_MS = number
}

<snapshot_option> ::=
{
    ALLOW_SNAPSHOT_ISOLATION { ON | OFF }
  | READ_COMMITTED_SNAPSHOT { ON | OFF }
  | MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT { ON | OFF }
}
<sql_option> ::=
{
    ANSI_NULL_DEFAULT { ON | OFF }
  | ANSI_NULLS { ON | OFF }
  | ANSI_PADDING { ON | OFF }
  | ANSI_WARNINGS { ON | OFF }
  | ARITHABORT { ON | OFF }
  | COMPATIBILITY_LEVEL = { 170 | 160 | 150 | 140 | 130 | 120 | 110 | 100 }
  | CONCAT_NULL_YIELDS_NULL { ON | OFF }
  | NUMERIC_ROUNDABORT { ON | OFF }
  | QUOTED_IDENTIFIER { ON | OFF }
  | RECURSIVE_TRIGGERS { ON | OFF }
}

<temporal_history_retention>::= TEMPORAL_HISTORY_RETENTION { ON | OFF }
```

```syntaxsql
ALTER DATABASE { database_name }
SET
{
    <optionspec> [ ,...n ]
}
;

<option_spec>::=
{
    <auto_option>
  | <db_encryption_option>
  | <query_store_options>
  | <result_set_caching>
  | <snapshot_option>
}
;

<auto_option> ::=
{
    AUTO_CREATE_STATISTICS { OFF | ON }
}

<db_encryption_option> ::=
{
    ENCRYPTION { ON | OFF }
}

<query_store_option> ::=
{
    QUERY_STORE { OFF | ON }
}

<result_set_caching_option> ::=
{
    RESULT_SET_CACHING { ON | OFF }
}

<snapshot_option> ::=
{
    READ_COMMITTED_SNAPSHOT { ON | OFF }
}
```

```syntaxsql
-- Microsoft Fabric Data Warehouse

ALTER DATABASE { warehouse_name | CURRENT }
SET
{
    <option_spec> [ ,...n ]
}

<option_spec> ::=
{
    <data_lake_log_publishing>
  | <vorder>
  | <timestamp>
  | <result_set_caching>
  | <proactive_statistics_refresh>
  | <data_retention_period>
}
;

<data_lake_log_publishing> ::=
{
    DATA_LAKE_LOG_PUBLISHING { PAUSED | AUTO }
}

<vorder> ::=
{
    VORDER = OFF
}

<timestamp> ::=
{
    TIMESTAMP = {CURRENT_TIMESTAMP | 'YYYY-MM-DDTHH:MM:SS.SS' }
}

<result_set_caching> ::=
{
    RESULT_SET_CACHING { ON | OFF }
}

<proactive_statistics_refresh> ::=
{
    PROACTIVE_STATISTICS_REFRESH = { ON | OFF }
}

<data_retention_period> ::=
{
    TIME_TRAVEL_RETENTION_PERIOD = { n } DAYS
}
```

## ALTER DATABASE (Transact-SQL)

`docs/t-sql/statements/alter-database-transact-sql.md`

### Syntax

```syntaxsql
-- SQL Server Syntax
ALTER DATABASE { database_name | CURRENT }
{
    MODIFY NAME = new_database_name
  | COLLATE collation_name
  | <file_and_filegroup_options>
  | SET <option_spec> [ ,...n ] [ WITH <termination> ]
}
[;]

<file_and_filegroup_options>::=
  <add_or_modify_files>::=
  <filespec>::=
  <add_or_modify_filegroups>::=
  <filegroup_updatability_option>::=

<option_spec>::=
{
  | <auto_option>
  | <change_tracking_option>
  | <cursor_option>
  | <database_mirroring_option>
  | <date_correlation_optimization_option>
  | <db_encryption_option>
  | <db_state_option>
  | <db_update_option>
  | <db_user_access_option>
  | <delayed_durability_option>
  | <external_access_option>
  | <FILESTREAM_options>
  | <HADR_options>
  | <parameterization_option>
  | <query_store_options>
  | <recovery_option>
  | <service_broker_option>
  | <snapshot_option>
  | <sql_option>
  | <termination>
  | <temporal_history_retention>
  | <data_retention_policy>
  | <compatibility_level>
      { 170 | 160 | 150 | 140 | 130 | 120 | 110 | 100 }
}
```

```syntaxsql
-- Azure SQL Database Syntax
ALTER DATABASE { database_name | CURRENT }
{
    MODIFY NAME = new_database_name
  | MODIFY ( <edition_options> [, ... n] ) [WITH MANUAL_CUTOVER]
  | MODIFY BACKUP_STORAGE_REDUNDANCY = { 'LOCAL' | 'ZONE' | 'GEO' }
  | SET { <option_spec> [ ,... n ] WITH <termination>}
  | ADD SECONDARY ON SERVER <partner_server_name>
    [WITH ( <add-secondary-option>::=[, ... n] ) ]
  | PERFORM_CUTOVER
  | REMOVE SECONDARY ON SERVER <partner_server_name>
  | FAILOVER
  | FORCE_FAILOVER_ALLOW_DATA_LOSS
}
[;]

<edition_options> ::=
{

  MAXSIZE = { 100 MB | 250 MB | 500 MB | 1 ... 1024 ... 4096 GB }
  | EDITION = { 'Basic' | 'Standard' | 'Premium' | 'GeneralPurpose' | 'BusinessCritical' | 'Hyperscale'}
  | SERVICE_OBJECTIVE =
       { <service-objective>
       | { ELASTIC_POOL (name = <elastic_pool_name>) }
       }
}

<add-secondary-option> ::=
   {
      ALLOW_CONNECTIONS = { ALL | NO }
     | BACKUP_STORAGE_REDUNDANCY = { 'LOCAL' | 'ZONE' | 'GEO' }
     | SERVICE_OBJECTIVE =
       { <service-objective>
       | { ELASTIC_POOL ( name = <elastic_pool_name>) }
       | DATABASE_NAME = <target_database_name>
       | SECONDARY_TYPE = { GEO | NAMED }
       }
   }

<service-objective> ::={ 'Basic' |'S0' | 'S1' | 'S2' | 'S3'| 'S4'| 'S6'| 'S7'| 'S9'| 'S12'
      | 'P1' | 'P2' | 'P4'| 'P6' | 'P11' | 'P15'
      | 'BC_DC_n'
      | 'BC_Gen5_n'
      | 'BC_M_n'
      | 'GP_DC_n'
      | 'GP_Gen5_n'
      | 'GP_S_Gen5_n'
      | 'HS_DC_n'
      | 'HS_Gen5_n'
      | 'HS_S_Gen5_n'
      | 'HS_MOPRMS_n'
      | 'HS_PRMS_n'
      | { ELASTIC_POOL(name = <elastic_pool_name>) }
      }

<option_spec> ::=
{
    <auto_option>
  | <change_tracking_option>
  | <cursor_option>
  | <db_encryption_option>
  | <db_update_option>
  | <db_user_access_option>
  | <delayed_durability_option>
  | <parameterization_option>
  | <query_store_options>
  | <snapshot_option>
  | <sql_option>
  | <target_recovery_time_option>
  | <termination>
  | <temporal_history_retention>
  | <compatibility_level>
    { 170 | 160 | 150 | 140 | 130 | 120 | 110 | 100 }

}
```

```syntaxsql
-- Azure SQL Managed Instance syntax
ALTER DATABASE { database_name | CURRENT }
{
    MODIFY NAME = new_database_name
  | COLLATE collation_name
  | <file_and_filegroup_options>
  | SET <option_spec> [ ,...n ]
}
[;]

<file_and_filegroup_options>::=
  <add_or_modify_files>::=
  <filespec>::=
  <add_or_modify_filegroups>::=
  <filegroup_updatability_option>::=

<option_spec> ::=
{
    <auto_option>
  | <change_tracking_option>
  | <cursor_option>
  | <db_encryption_option>
  | <db_update_option>
  | <db_user_access_option>
  | <delayed_durability_option>
  | <parameterization_option>
  | <query_store_options>
  | <snapshot_option>
  | <sql_option>
  | <target_recovery_time_option>
  | <temporal_history_retention>
  | <compatibility_level>
      { 170 | 160 | 150 | 140 | 130 | 120 | 110 | 100 }

}
```

### [Dedicated SQL pool](#tab/sqlpool)

```syntaxsql
ALTER DATABASE { database_name | CURRENT }
{
  MODIFY NAME = new_database_name
| MODIFY ( <edition_option> [, ... n] )
| SET <option_spec> [ ,...n ] [ WITH <termination> ]
}
[;]

<edition_option> ::=
      MAXSIZE = {
            250 | 500 | 750 | 1024 | 5120 | 10240 | 20480
          | 30720 | 40960 | 51200 | 61440 | 71680 | 81920
          | 92160 | 102400 | 153600 | 204800 | 245760
      } GB
      | SERVICE_OBJECTIVE = {
            'DW100' | 'DW200' | 'DW300' | 'DW400' | 'DW500'
          | 'DW600' | 'DW1000' | 'DW1200' | 'DW1500' | 'DW2000'
          | 'DW3000' | 'DW6000' | 'DW500c' | 'DW1000c' | 'DW1500c'
          | 'DW2000c' | 'DW2500c' | 'DW3000c' | 'DW5000c' | 'DW6000c'
          | 'DW7500c' | 'DW10000c' | 'DW15000c' | 'DW30000c'
      }
```

### [Serverless SQL pool](#tab/sqlod)

```syntaxsql
ALTER DATABASE { database_name | Current }
{
    COLLATE collation_name
  | SET { <optionspec> [ ,...n ] }
}
[;]

<optionspec> ::=
{
    <auto_option>
  | <sql_option>
}

<auto_option> ::=
{
    AUTO_CREATE_STATISTICS { OFF | ON [ ( INCREMENTAL = { ON | OFF } ) ] }
}

<sql_option> ::=
{
    ANSI_NULL_DEFAULT { ON | OFF }
  | ANSI_NULLS { ON | OFF }
  | ANSI_PADDING { ON | OFF }
  | ANSI_WARNINGS { ON | OFF }
  | ARITHABORT { ON | OFF }
  | COMPATIBILITY_LEVEL = { 170 | 160 | 150 | 140 | 130 | 120 | 110 | 100 }
  | CONCAT_NULL_YIELDS_NULL { ON | OFF }
  | NUMERIC_ROUNDABORT { ON | OFF }
  | QUOTED_IDENTIFIER { ON | OFF }
}
```

### Syntax

```syntaxsql
-- Analytics Platform System
ALTER DATABASE database_name
    SET ( <set_database_options> | <db_encryption_option> )
[;]

<set_database_options> ::=
{
    AUTOGROW = { ON | OFF }
    | REPLICATED_SIZE = size [GB]
    | DISTRIBUTED_SIZE = size [GB]
    | LOG_SIZE = size [GB]
    | SET AUTO_CREATE_STATISTICS { ON | OFF }
    | SET AUTO_UPDATE_STATISTICS { ON | OFF }
    | SET AUTO_UPDATE_STATISTICS_ASYNC { ON | OFF }
}

<db_encryption_option> ::=
    ENCRYPTION { ON | OFF }
```

## ALTER ENDPOINT (Transact-SQL)

`docs/t-sql/statements/alter-endpoint-transact-sql.md`

### Syntax

```syntaxsql
ALTER ENDPOINT endPointName [ AUTHORIZATION login ]
[ STATE = { STARTED | STOPPED | DISABLED } ]
[ AS { TCP } (
    <protocol_specific_arguments>
) ]
[ FOR { TSQL | SERVICE_BROKER | DATABASE_MIRRORING } (
    <language_specific_arguments>
) ]

<AS TCP_protocol_specific_arguments> ::=
AS TCP (
    LISTENER_PORT = listenerPort
    [ [ , ] LISTENER_IP = ALL | ( four_part_ipv4_address ) | ( 'ip_address_v6' ) ]
)

<FOR TSQL_language_specific_arguments> ::=
FOR TSQL (
    [ ENCRYPTION = { NEGOTIATED | STRICT } ]
)

<FOR SERVICE_BROKER_language_specific_arguments> ::=
FOR SERVICE_BROKER (
    [ AUTHENTICATION = {
          WINDOWS [ { NTLM | KERBEROS | NEGOTIATE } ]
          | CERTIFICATE certificate_name
          | WINDOWS [ { NTLM | KERBEROS | NEGOTIATE } ] CERTIFICATE certificate_name
          | CERTIFICATE certificate_name WINDOWS [ { NTLM | KERBEROS | NEGOTIATE } ]
    } ]
    [ [ , ] ENCRYPTION = {
          DISABLED
          | { SUPPORTED | REQUIRED }
            [ ALGORITHM { AES | RC4 | AES RC4 | RC4 AES } ]
    } ]
    [ [ , ] MESSAGE_FORWARDING = { ENABLED | DISABLED } ]
    [ [ , ] MESSAGE_FORWARD_SIZE = forward_size ]
)

<FOR DATABASE_MIRRORING_language_specific_arguments> ::=
FOR DATABASE_MIRRORING (
    [ AUTHENTICATION = {
          WINDOWS [ { NTLM | KERBEROS | NEGOTIATE } ]
          | CERTIFICATE certificate_name
          | WINDOWS [ { NTLM | KERBEROS | NEGOTIATE } ] CERTIFICATE certificate_name
          | CERTIFICATE certificate_name WINDOWS [ { NTLM | KERBEROS | NEGOTIATE } ]
    } ]
    [ [ , ] ENCRYPTION = {
          DISABLED
          | { SUPPORTED | REQUIRED }
            [ ALGORITHM { AES | RC4 | AES RC4 | RC4 AES } ]
    } ]
    [ , ] ROLE = { WITNESS | PARTNER | ALL }
)
```

## ALTER EVENT SESSION (Transact-SQL)

`docs/t-sql/statements/alter-event-session-transact-sql.md`

### Syntax

```syntaxsql
ALTER EVENT SESSION event_session_name
ON { SERVER | DATABASE }
{
    [ [ {  <add_drop_event> [ , ...n ] }
       | { <add_drop_event_target> [ , ...n ] } ]
    [ WITH ( <event_session_options> [ , ...n ] ) ]
    ]
    | [ STATE = { START | STOP } ]
}

<add_drop_event>::=
{
    [ ADD EVENT <event_specifier>
         [ ( {
                 [ SET { event_customizable_attribute = <value> [ , ...n ] } ]
                 [ ACTION ( { [event_module_guid].event_package_name.action_name [ , ...n ] } ) ]
                 [ WHERE <predicate_expression> ]
        } ) ]
   ]
   | DROP EVENT <event_specifier> }

<event_specifier> ::=
{
[event_module_guid].event_package_name.event_name
}
<predicate_expression> ::=
{
    [ NOT ] <predicate_factor> | { ( <predicate_expression> ) }
    [ { AND | OR } [ NOT ] { <predicate_factor> | ( <predicate_expression> ) } ]
    [ , ...n ]
}

<predicate_factor>::=
{
    <predicate_leaf> | ( <predicate_expression> )
}

<predicate_leaf>::=
{
      <predicate_source_declaration> { = | < > | != | > | >= | < | <= } <value>
    | [event_module_guid].event_package_name.predicate_compare_name ( <predicate_source_declaration> , <value> )
}

<predicate_source_declaration>::=
{
    event_field_name | ( [event_module_guid].event_package_name.predicate_source_name )
}

<value>::=
{
    number | 'string'
}

<add_drop_event_target>::=
{
    ADD TARGET <event_target_specifier>
        [ ( SET { target_parameter_name = <value> [ , ...n ] } ) ]
    | DROP TARGET <event_target_specifier>
}

<event_target_specifier>::=
{
    [event_module_guid].event_package_name.target_name
}

<event_session_options>::=
{
    [       MAX_MEMORY = size [ KB | MB ] ]
    [ [ , ] EVENT_RETENTION_MODE = { ALLOW_SINGLE_EVENT_LOSS | ALLOW_MULTIPLE_EVENT_LOSS | NO_EVENT_LOSS } ]
    [ [ , ] MAX_DISPATCH_LATENCY = { seconds SECONDS | INFINITE } ]
    [ [ , ] MAX_EVENT_SIZE = size [ KB | MB ] ]
    [ [ , ] MEMORY_PARTITION_MODE = { NONE | PER_NODE | PER_CPU } ]
    [ [ , ] TRACK_CAUSALITY = { ON | OFF } ]
    [ [ , ] STARTUP_STATE = { ON | OFF } ]
    [ [ , ] MAX_DURATION = { <time duration> { SECONDS | MINUTES | HOURS | DAYS } | UNLIMITED } ]
}
```

## ALTER EXTERNAL DATA SOURCE (Transact-SQL)

`docs/t-sql/statements/alter-external-data-source-transact-sql.md`

### Syntax

> Modify an external data source. Syntax for SQL Server (2016, 2017 and 2019) and Analytics Platform System (PDW).

```syntaxsql
-- Modify an external data source
-- Applies to: SQL Server (2016, 2017 and 2019) and APS
ALTER EXTERNAL DATA SOURCE data_source_name SET
    {
        LOCATION = '<prefix>://<path>[:<port>]' [,] |
        RESOURCE_MANAGER_LOCATION = <'IP address;Port'> [,] |
        CREDENTIAL = credential_name
    }
    [;]
```

> Modify an external data source pointing to Azure Blob storage. Syntax for SQL Server (2017 and 2019).

```syntaxsql
-- Modify an external data source pointing to Azure Blob storage
-- Applies to: SQL Server (2017 and 2019)
ALTER EXTERNAL DATA SOURCE data_source_name
    SET
        LOCATION = 'https://storage_account_name.blob.core.windows.net'
        [, CREDENTIAL = credential_name ]
```

> Modify an external data source pointing to Azure Blob storage. Syntax for SQL Server 2022 and later versions.

```syntaxsql
-- Modify an external data source pointing to Azure Blob storage
-- Applies to: SQL Server 2022 and later versions
ALTER EXTERNAL DATA SOURCE data_source_name
    SET
        LOCATION = 'abs://storage_account_name.blob.core.windows.net'
        [, CREDENTIAL = credential_name ]
```

> Modify an external data source pointing to Azure Data Lake Storage (ADLS) Gen2. Syntax for SQL Server 2022 and later versions.

```syntaxsql
-- Modify an external data source pointing to Azure Data Lake Storage Gen2
-- Applies to: SQL Server 2022 and later versions
ALTER EXTERNAL DATA SOURCE data_source_name
    SET
        LOCATION = 'adls://storage_account_name.dfs.core.windows.net'
        [, CREDENTIAL = credential_name ]
```

> Modify an external data source pointing to Azure Blob Storage or Azure Data Lake Storage. Syntax for Azure Synapse Analytics dedicated SQL pool only.

```syntaxsql
-- Modify an external data source pointing to Azure Blob storage or Azure Data Lake storage
-- Applies to: Azure Synapse Analytics dedicated SQL pool only
ALTER EXTERNAL DATA SOURCE data_source_name
    SET
        [LOCATION = '<location prefix>://<location path>']
        [, CREDENTIAL = credential_name ]
```

## ALTER EXTERNAL LANGUAGE (Transact-SQL) - SQL Server

`docs/t-sql/statements/alter-external-language-transact-sql.md`

### Syntax

```syntaxsql
ALTER EXTERNAL LANGUAGE language_name
[ AUTHORIZATION owner_name ]
{
    SET <file_spec>
    | ADD <file_spec>
    | REMOVE PLATFORM <platform>
}
[ ; ]

<file_spec> ::=
{
    ( CONTENT = {<external_lang_specifier> | <content_bits>,
    FILE_NAME = <external_lang_file_name>
    [, PLATFORM = <platform> ]
    [, PARAMETERS = <external_lang_parameters> ]
    [, ENVIRONMENT_VARIABLES = <external_lang_env_variables> ] )
}

<external_lang_specifier> :: =
{
    '[file_path\]os_file_name'
}

<content_bits> :: =
{
    varbinary_literal
   | varbinary_expression
}

<external_lang_file_name> :: =
'extension_file_name'

<platform> :: =
{
   WINDOWS
  | LINUX
}

< external_lang_parameters > :: =
'extension_specific_parameters'
```

## ALTER EXTERNAL LIBRARY (Transact-SQL)

`docs/t-sql/statements/alter-external-library-transact-sql.md`

### Syntax for SQL Server 2019

Marked for `>=sql-server-ver15 || =sql-server-linux-ver15`.

```syntaxsql
ALTER EXTERNAL LIBRARY library_name
[ AUTHORIZATION owner_name ]
SET <file_spec>
WITH ( LANGUAGE = <language> )
[ ; ]

<file_spec> ::=
{
    (CONTENT = { <client_library_specifier> | <library_bits> | NONE}
    [, PLATFORM = <platform> )
}

<client_library_specifier> :: =
{
      '[\\computer_name\]share_name\[path\]manifest_file_name'
    | '[local_path\]manifest_file_name'
    | '<relative_path_in_external_data_source>'
}

<library_bits> :: =
{
      varbinary_literal
    | varbinary_expression
}

<platform> :: =
{
      WINDOWS
    | LINUX
}

<language> :: =
{
      'R'
    | 'Python'
    | <external_language>
}
```

### Syntax for SQL Server 2017

Marked for `=sql-server-2017`.

```syntaxsql
ALTER EXTERNAL LIBRARY library_name
[ AUTHORIZATION owner_name ]
SET <file_spec>
WITH ( LANGUAGE = 'R' )
[ ; ]

<file_spec> ::=
{
    (CONTENT = { <client_library_specifier> | <library_bits> | NONE}
    [, PLATFORM = WINDOWS )
}

<client_library_specifier> :: =
{
      '[\\computer_name\]share_name\[path\]manifest_file_name'
    | '[local_path\]manifest_file_name'
    | '<relative_path_in_external_data_source>'
}

<library_bits> :: =
{
      varbinary_literal
    | varbinary_expression
}
```

### Syntax for Azure SQL Managed Instance

Marked for `=azuresqldb-mi-current`.

```syntaxsql
CREATE EXTERNAL LIBRARY library_name
[ AUTHORIZATION owner_name ]
FROM <file_spec> [ ,...2 ]
WITH ( LANGUAGE = <language> )
[ ; ]

<file_spec> ::=
{
    (CONTENT = <library_bits>)
}

<library_bits> :: =
{
      varbinary_literal
    | varbinary_expression
}

<language> :: =
{
      'R'
    | 'Python'
}
```

## ALTER EXTERNAL MODEL (Transact-SQL)

`docs/t-sql/statements/alter-external-model-transact-sql.md`

### Syntax

```syntaxsql
ALTER EXTERNAL MODEL external_model_object_name
SET
  (   LOCATION = '<prefix>://<path> [ :<port> ] '
    , API_FORMAT = '<OpenAI , Azure OpenAI , etc>'
    , MODEL_TYPE = EMBEDDINGS
    , MODEL = 'text-embedding-ada-002'
    [ , CREDENTIAL = <credential_name> ]
    [ , PARAMETERS = ' { "valid":"JSON" } ' ]
  );
```

## ALTER EXTERNAL RESOURCE POOL (Transact-SQL)

`docs/t-sql/statements/alter-external-resource-pool-transact-sql.md`

### Syntax

Marked for `>=sql-server-ver15 || >=sql-server-linux-ver15`.

```syntaxsql
ALTER EXTERNAL RESOURCE POOL { pool_name | "default" }
[ WITH (
    [ MAX_CPU_PERCENT = value ]
    [ [ , ] MAX_MEMORY_PERCENT = value ]
    [ [ , ] MAX_PROCESSES = value ]
    )
]
[ ; ]

<CPU_range_spec> ::=
{ CPU_ID | CPU_ID  TO CPU_ID } [ ,...n ]
```

Marked for `=sql-server-2017`.

```syntaxsql
ALTER EXTERNAL RESOURCE POOL { pool_name | "default" }
[ WITH (
   [ MAX_CPU_PERCENT = value ]
   [ [ , ] AFFINITY CPU =
           {
               AUTO
             | ( <cpu_range_spec> )
             | NUMANODE = (( <NUMA_node_id> )
           } ]
   [ [ , ] MAX_MEMORY_PERCENT = value ]
   [ [ , ] MAX_PROCESSES = value ]
   )
]
[ ; ]

<CPU_range_spec> ::=
{ CPU_ID | CPU_ID  TO CPU_ID } [ ,...n ]
```

## ALTER FULLTEXT CATALOG (Transact-SQL)

`docs/t-sql/statements/alter-fulltext-catalog-transact-sql.md`

### Syntax

```syntaxsql
ALTER FULLTEXT CATALOG catalog_name
{ REBUILD [ WITH ACCENT_SENSITIVITY = { ON | OFF } ]
| REORGANIZE
| AS DEFAULT
}
```

## ALTER FULLTEXT INDEX (Transact-SQL)

`docs/t-sql/statements/alter-fulltext-index-transact-sql.md`

### Syntax

```syntaxsql
ALTER FULLTEXT INDEX ON table_name
   { ENABLE
   | DISABLE
   | SET CHANGE_TRACKING [ = ] { MANUAL | AUTO | OFF }
   | ADD ( column_name
           [ TYPE COLUMN type_column_name ]
           [ LANGUAGE language_term ]
           [ STATISTICAL_SEMANTICS ]
           [ , ...n ]
         )
     [ WITH NO POPULATION ]
   | ALTER COLUMN column_name
     { ADD | DROP } STATISTICAL_SEMANTICS
     [ WITH NO POPULATION ]
   | DROP ( column_name [ , ...n ] )
     [ WITH NO POPULATION ]
   | START { FULL | INCREMENTAL | UPDATE } POPULATION
   | { STOP | PAUSE | RESUME } POPULATION
   | SET STOPLIST [ = ] { OFF | SYSTEM | stoplist_name }
     [ WITH NO POPULATION ]
   | SET SEARCH PROPERTY LIST [ = ] { OFF | property_list_name }
     [ WITH NO POPULATION ]
   }
[ ; ]
```

## ALTER FULLTEXT STOPLIST (Transact-SQL)

`docs/t-sql/statements/alter-fulltext-stoplist-transact-sql.md`

### Syntax

```syntaxsql
ALTER FULLTEXT STOPLIST stoplist_name
{
        ADD [N] 'stopword' LANGUAGE language_term
  | DROP
    {
        'stopword' LANGUAGE language_term
      | ALL LANGUAGE language_term
      | ALL
     }
;
```

## ALTER FUNCTION (Transact-SQL)

`docs/t-sql/statements/alter-function-transact-sql.md`

### Syntax

```syntaxsql
-- Transact-SQL Scalar Function Syntax
ALTER FUNCTION [ schema_name. ] function_name
( [ { @parameter_name [ AS ][ type_schema_name. ] parameter_data_type
    [ = default ] }
    [ ,...n ]
  ]
)
RETURNS return_data_type
    [ WITH <function_option> [ ,...n ] ]
    [ AS ]
    BEGIN
        function_body
        RETURN scalar_expression
    END
[ ; ]
```

```syntaxsql
-- Transact-SQL Inline Table-Valued Function Syntax
ALTER FUNCTION [ schema_name. ] function_name
( [ { @parameter_name [ AS ] [ type_schema_name. ] parameter_data_type
    [ = default ] }
    [ ,...n ]
  ]
)
RETURNS TABLE
    [ WITH <function_option> [ ,...n ] ]
    [ AS ]
    RETURN [ ( ] select_stmt [ ) ]
[ ; ]
```

```syntaxsql
-- Transact-SQL Multistatement Table-valued Function Syntax
ALTER FUNCTION [ schema_name. ] function_name
( [ { @parameter_name [ AS ] [ type_schema_name. ] parameter_data_type
    [ = default ] }
    [ ,...n ]
  ]
)
RETURNS @return_variable TABLE <table_type_definition>
    [ WITH <function_option> [ ,...n ] ]
    [ AS ]
    BEGIN
        function_body
        RETURN
    END
[ ; ]
```

```syntaxsql
-- Transact-SQL Function Clauses
<function_option>::=
{
    [ ENCRYPTION ]
  | [ SCHEMABINDING ]
  | [ RETURNS NULL ON NULL INPUT | CALLED ON NULL INPUT ]
  | [ EXECUTE_AS_Clause ]
}

<table_type_definition>:: =
( { <column_definition> <column_constraint>
  | <computed_column_definition> }
    [ <table_constraint> ] [ ,...n ]
)
<column_definition>::=
{
    { column_name data_type }
    [ [ DEFAULT constant_expression ]
      [ COLLATE collation_name ] | [ ROWGUIDCOL ]
    ]
    | [ IDENTITY [ (seed , increment ) ] ]
    [ <column_constraint> [ ...n ] ]
}

<column_constraint>::=
{
    [ NULL | NOT NULL ]
    { PRIMARY KEY | UNIQUE }
      [ CLUSTERED | NONCLUSTERED ]
        [ WITH FILLFACTOR = fillfactor
        | WITH ( < index_option > [ , ...n ] )
      [ ON { filegroup | "default" } ]
  | [ CHECK ( logical_expression ) ] [ ,...n ]
}

<computed_column_definition>::=
column_name AS computed_column_expression

<table_constraint>::=
{
    { PRIMARY KEY | UNIQUE }
      [ CLUSTERED | NONCLUSTERED ]
      ( column_name [ ASC | DESC ] [ ,...n ] )
        [ WITH FILLFACTOR = fillfactor
        | WITH ( <index_option> [ , ...n ] )
  | [ CHECK ( logical_expression ) ] [ ,...n ]
}

<index_option>::=
{
    PAD_INDEX = { ON | OFF }
  | FILLFACTOR = fillfactor
  | IGNORE_DUP_KEY = { ON | OFF }
  | STATISTICS_NORECOMPUTE = { ON | OFF }
  | ALLOW_ROW_LOCKS = { ON | OFF }
  | ALLOW_PAGE_LOCKS ={ ON | OFF }
}
```

```syntaxsql
-- CLR Scalar and Table-Valued Function Syntax
ALTER FUNCTION [ schema_name. ] function_name
( { @parameter_name [AS] [ type_schema_name. ] parameter_data_type
    [ = default ] }
    [ ,...n ]
)
RETURNS { return_data_type | TABLE <clr_table_type_definition> }
    [ WITH <clr_function_option> [ ,...n ] ]
    [ AS ] EXTERNAL NAME <method_specifier>
[ ; ]
```

```syntaxsql
-- CLR Function Clauses
<method_specifier>::=
    assembly_name.class_name.method_name

<clr_function_option>::=
}
    [ RETURNS NULL ON NULL INPUT | CALLED ON NULL INPUT ]
  | [ EXECUTE_AS_Clause ]
}

<clr_table_type_definition>::=
( { column_name data_type } [ ,...n ] )
```

```syntaxsql
-- Syntax for In-Memory OLTP: Natively compiled, scalar user-defined function
ALTER FUNCTION [ schema_name. ] function_name
 ( [ { @parameter_name [ AS ][ type_schema_name. ] parameter_data_type
    [ NULL | NOT NULL ] [ = default ] }
    [ ,...n ]
  ]
)
RETURNS return_data_type
    [ WITH <function_option> [ ,...n ] ]
    [ AS ]
    BEGIN ATOMIC WITH (set_option [ ,... n ])
        function_body
        RETURN scalar_expression
    END

<function_option>::=
{ |  NATIVE_COMPILATION
  |  SCHEMABINDING
  | [ EXECUTE_AS_Clause ]
  | [ RETURNS NULL ON NULL INPUT | CALLED ON NULL INPUT ]
}
```

## ALTER INDEX (Selective XML Indexes)

`docs/t-sql/statements/alter-index-selective-xml-indexes.md`

### Syntax

```syntaxsql
ALTER INDEX index_name
    ON <table_object>
    [WITH XMLNAMESPACES ( <xmlnamespace_list> )]
    FOR ( <promoted_node_path_action_list> )
    [WITH ( <index_options> )]

<table_object> ::=
{ database_name.schema_name.table_name | schema_name.table_name | table_name }
<promoted_node_path_action_list> ::=
<promoted_node_path_action_item> [, <promoted_node_path_action_list>]

<promoted_node_path_action_item>::=
<add_node_path_item_action> | <remove_node_path_item_action>

<add_node_path_item_action> ::=
ADD <path_name> = <promoted_node_path_item>

<promoted_node_path_item>::=
<xquery_node_path_item> | <sql_values_node_path_item>

<remove_node_path_item_action> ::= REMOVE <path_name>

<path_name_or_typed_node_path>::=
<path_name> | <typed_node_path>

<typed_node_path> ::=
<node_path> [[AS XQUERY <xsd_type_ext>] | [AS SQL <sql_type>]]

<xquery_node_path_item> ::=
<node_path> [AS XQUERY <xsd_type_or_node_hint>] [SINGLETON]

<xsd_type_or_node_hint> ::=
[<xsd_type>] [MAXLENGTH(x)] | 'node()'

<sql_values_node_path_item> ::=
<node_path> AS SQL <sql_type> [SINGLETON]

<node_path> ::=
character_string_literal

<xsd_type_ext> ::=
character_string_literal

<sql_type> ::=
identifier

<path_name> ::=
identifier

<xmlnamespace_list> ::=
<xmlnamespace_item> [, <xmlnamespace_list>]

<xmlnamespace_item> ::=
<xmlnamespace_uri> AS <xmlnamespace_prefix>

<xml_namespace_uri> ::= character_string_literal
<xml_namespace_prefix> ::= identifier

<index_options> ::=
(
  | PAD_INDEX  = { ON | OFF }
  | FILLFACTOR = fillfactor
  | SORT_IN_TEMPDB = { ON | OFF }
  | IGNORE_DUP_KEY =OFF
  | DROP_EXISTING = { ON | OFF }
  | ONLINE =OFF
  | ALLOW_ROW_LOCKS = { ON | OFF }
  | ALLOW_PAGE_LOCKS = { ON | OFF }
  | MAXDOP = max_degree_of_parallelism
)
```

## ALTER INDEX (Transact-SQL)

`docs/t-sql/statements/alter-index-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, and Azure SQL Managed Instance.

```syntaxsql
ALTER INDEX { index_name | ALL } ON <object>
{
      REBUILD [
            [ WITH ( <rebuild_index_option> [ , ...n ] ) ]
          | [ PARTITION = ALL [ WITH ( <rebuild_index_option> [ , ...n ] ) ] ]
          | [ PARTITION = partition_number [ WITH ( <single_partition_rebuild_index_option> [ , ...n ] ) ] ]
      ]
    | DISABLE
    | REORGANIZE  [ PARTITION = partition_number ] [ WITH ( <reorganize_option>  ) ]
    | SET ( <set_index_option> [ , ...n ] )
    | RESUME [ WITH (<resumable_index_option> [ , ...n ] ) ]
    | PAUSE
    | ABORT
}
[ ; ]

<object> ::=
{
    { database_name.schema_name.table_or_view_name | schema_name.table_or_view_name | table_or_view_name }
}

<rebuild_index_option> ::=
{
      PAD_INDEX = { ON | OFF }
    | FILLFACTOR = fillfactor
    | SORT_IN_TEMPDB = { ON | OFF }
    | IGNORE_DUP_KEY = { ON | OFF }
    | STATISTICS_NORECOMPUTE = { ON | OFF }
    | STATISTICS_INCREMENTAL = { ON | OFF }
    | ONLINE = { ON [ ( <low_priority_lock_wait> ) ] | OFF }
    | RESUMABLE = { ON | OFF }
    | MAX_DURATION = <time> [ MINUTES ]
    | ALLOW_ROW_LOCKS = { ON | OFF }
    | ALLOW_PAGE_LOCKS = { ON | OFF }
    | MAXDOP = max_degree_of_parallelism
    | DATA_COMPRESSION = { NONE | ROW | PAGE | COLUMNSTORE | COLUMNSTORE_ARCHIVE }
        [ ON PARTITIONS ( { <partition_number> [ TO <partition_number> ] } [ , ...n ] ) ]
    | XML_COMPRESSION = { ON | OFF }
        [ ON PARTITIONS ( { <partition_number> [ TO <partition_number> ] } [ , ...n ] ) ] }

<single_partition_rebuild_index_option> ::=
{
      SORT_IN_TEMPDB = { ON | OFF }
    | MAXDOP = max_degree_of_parallelism
    | RESUMABLE = { ON | OFF }
    | MAX_DURATION = <time> [ MINUTES ]
    | DATA_COMPRESSION = { NONE | ROW | PAGE | COLUMNSTORE | COLUMNSTORE_ARCHIVE }
    | XML_COMPRESSION = { ON | OFF }
    | ONLINE = { ON [ ( <low_priority_lock_wait> ) ] | OFF }
}

<reorganize_option> ::=
{
       LOB_COMPACTION = { ON | OFF }
    |  COMPRESS_ALL_ROW_GROUPS =  { ON | OFF }
}

<set_index_option> ::=
{
      ALLOW_ROW_LOCKS = { ON | OFF }
    | ALLOW_PAGE_LOCKS = { ON | OFF }
    | OPTIMIZE_FOR_SEQUENTIAL_KEY = { ON | OFF }
    | IGNORE_DUP_KEY = { ON | OFF }
    | STATISTICS_NORECOMPUTE = { ON | OFF }
    | COMPRESSION_DELAY = { 0 | delay [ Minutes ] }
}

<resumable_index_option> ::=
 {
    MAXDOP = max_degree_of_parallelism
    | MAX_DURATION = <time> [ MINUTES ]
    | <low_priority_lock_wait>
 }

<low_priority_lock_wait> ::=
{
    WAIT_AT_LOW_PRIORITY ( MAX_DURATION = <time> [ MINUTES ] ,
                          ABORT_AFTER_WAIT = { NONE | SELF | BLOCKERS } )
}
```

> Syntax for Azure Synapse Analytics and Analytics Platform System (PDW).

```syntaxsql
ALTER INDEX { index_name | ALL }
    ON [ schema_name. ] table_name
{
      REBUILD [
            [ WITH ( <rebuild_index_option> [ , ...n ] ) ]
          | [ PARTITION = ALL [ WITH ( <rebuild_index_option> ) ] ]
          | [ PARTITION = partition_number [ WITH ( <single_partition_rebuild_index_option> ) ] ]
      ]
    | DISABLE
    | REORGANIZE [ PARTITION = partition_number ]
}
[ ; ]

<rebuild_index_option> ::=
{
    DATA_COMPRESSION = { COLUMNSTORE | COLUMNSTORE_ARCHIVE }
        [ ON PARTITIONS ( { <partition_number> [ TO <partition_number> ] } [ , ...n ] ) ]
    | XML_COMPRESSION = { ON | OFF }
        [ ON PARTITIONS ( { <partition_number> [ TO <partition_number> ] } [ , ...n ] ) ]
}

<single_partition_rebuild_index_option> ::=
{
    DATA_COMPRESSION = { COLUMNSTORE | COLUMNSTORE_ARCHIVE }
    | XML_COMPRESSION = { ON | OFF }
}
```

## ALTER LOGIN (Transact-SQL)

`docs/t-sql/statements/alter-login-transact-sql.md`

### Syntax for SQL Server

```syntaxsql
ALTER LOGIN login_name
    {
    <status_option>
    | WITH <set_option> [ , ... ]
    | <cryptographic_credential_option>
    }
[;]

<status_option> ::=
        ENABLE | DISABLE

<set_option> ::=
    PASSWORD = 'password' | hashed_password HASHED
    [
      OLD_PASSWORD = 'oldpassword'
      | <password_option> [ <password_option> ]
    ]
    | DEFAULT_DATABASE = database
    | DEFAULT_LANGUAGE = language
    | NAME = login_name
    | CHECK_POLICY = { ON | OFF }
    | CHECK_EXPIRATION = { ON | OFF }
    | CREDENTIAL = credential_name
    | NO CREDENTIAL

<password_option> ::=
    MUST_CHANGE | UNLOCK

<cryptographic_credentials_option> ::=
    ADD CREDENTIAL credential_name
  | DROP CREDENTIAL credential_name
```

### Syntax for Azure SQL Database

```syntaxsql
ALTER LOGIN login_name
  {
      <status_option>
    | WITH <set_option> [ , .. .n ]
  }
[;]

<status_option> ::=
    ENABLE | DISABLE

<set_option> ::=
    PASSWORD = 'password'
    [
      OLD_PASSWORD = 'oldpassword'
    ]
    | NAME = login_name
```

### Syntax for SQL Server and Azure SQL Managed Instance

```syntaxsql
ALTER LOGIN login_name
    {
    <status_option>
    | WITH <set_option> [ , ... ]
    | <cryptographic_credential_option>
    }
[;]

<status_option> ::=
        ENABLE | DISABLE

<set_option> ::=
    PASSWORD = 'password' | hashed_password HASHED
    [
      OLD_PASSWORD = 'oldpassword'
      | <password_option> [ <password_option> ]
    ]
    | DEFAULT_DATABASE = database
    | DEFAULT_LANGUAGE = language
    | NAME = login_name
    | CHECK_POLICY = { ON | OFF }
    | CHECK_EXPIRATION = { ON | OFF }
    | CREDENTIAL = credential_name
    | NO CREDENTIAL

<password_option> ::=
    MUST_CHANGE | UNLOCK

<cryptographic_credentials_option> ::=
    ADD CREDENTIAL credential_name
  | DROP CREDENTIAL credential_name
```

```syntaxsql
-- Syntax for Azure SQL Managed Instance using Microsoft Entra logins

ALTER LOGIN login_name
  {
      <status_option>
    | WITH <set_option> [ , .. .n ]
  }
[;]

<status_option> ::=
    ENABLE | DISABLE

<set_option> ::=
     DEFAULT_DATABASE = database
   | DEFAULT_LANGUAGE = language
```

### Syntax for Azure Synapse

```syntaxsql
ALTER LOGIN login_name
  {
      <status_option>
    | WITH <set_option> [ , .. .n ]
  }
[;]

<status_option> ::=
    ENABLE | DISABLE

<set_option> ::=
    PASSWORD = 'password'
    [
      OLD_PASSWORD = 'oldpassword'
    ]
    | NAME = login_name
```

### Syntax for Analytics Platform System

```syntaxsql
ALTER LOGIN login_name
    {
    <status_option>
    | WITH <set_option> [ , ... ]
    }

<status_option> ::= ENABLE | DISABLE

<set_option> ::=
    PASSWORD = 'password'
    [
      OLD_PASSWORD = 'oldpassword'
      | <password_option> [ <password_option> ]
    ]
    | NAME = login_name
    | CHECK_POLICY = { ON | OFF }
    | CHECK_EXPIRATION = { ON | OFF }

<password_option> ::=
    MUST_CHANGE | UNLOCK
```

## ALTER MASTER KEY (Transact-SQL)

`docs/t-sql/statements/alter-master-key-transact-sql.md`

### Syntax

> Syntax for SQL Server

```syntaxsql
-- Syntax for SQL Server
ALTER MASTER KEY <alter_option>

<alter_option> ::=
    <regenerate_option> | <encryption_option>

<regenerate_option> ::=
    [ FORCE ] REGENERATE WITH ENCRYPTION BY PASSWORD = 'password'

<encryption_option> ::=
    ADD ENCRYPTION BY { SERVICE MASTER KEY | PASSWORD = 'password' }
    |
    DROP ENCRYPTION BY { SERVICE MASTER KEY | PASSWORD = 'password' }
```

> Syntax for Azure SQL Database and SQL database in Microsoft Fabric

```syntaxsql
-- Syntax for Azure SQL Database
-- Note: DROP ENCRYPTION BY SERVICE MASTER KEY is not supported on Azure SQL Database.

ALTER MASTER KEY <alter_option>

<alter_option> ::=
    <regenerate_option> | <encryption_option>

<regenerate_option> ::=
    [ FORCE ] REGENERATE WITH ENCRYPTION BY PASSWORD = 'password'

<encryption_option> ::=
    ADD ENCRYPTION BY { SERVICE MASTER KEY | PASSWORD = 'password' }
    |
    DROP ENCRYPTION BY { PASSWORD = 'password' }
```

> Syntax for Azure Synapse Analytics and Analytics Platform System (PDW)

```syntaxsql
-- Syntax for Azure Synapse Analytics and Analytics Platform System

ALTER MASTER KEY <alter_option>

<alter_option> ::=
    <regenerate_option> | <encryption_option>

<regenerate_option> ::=
    [ FORCE ] REGENERATE WITH ENCRYPTION BY PASSWORD ='password'

<encryption_option> ::=
    ADD ENCRYPTION BY SERVICE MASTER KEY
    |
    DROP ENCRYPTION BY SERVICE MASTER KEY
```

## ALTER MATERIALIZED VIEW (Transact-SQL)

`docs/t-sql/statements/alter-materialized-view-transact-sql.md`

### Syntax

```syntaxsql
ALTER MATERIALIZED VIEW [ schema_name . ] view_name
{
      REBUILD | DISABLE
}
[;]
```

## ALTER MESSAGE TYPE (Transact-SQL)

`docs/t-sql/statements/alter-message-type-transact-sql.md`

### Syntax

```syntaxsql
ALTER MESSAGE TYPE message_type_name
   VALIDATION =
    {  NONE
     | EMPTY
     | WELL_FORMED_XML
     | VALID_XML WITH SCHEMA COLLECTION schema_collection_name }
[ ; ]
```

## ALTER PARTITION FUNCTION (Transact-SQL)

`docs/t-sql/statements/alter-partition-function-transact-sql.md`

### Syntax

```syntaxsql
ALTER PARTITION FUNCTION partition_function_name()
{
    SPLIT RANGE ( boundary_value )
  | MERGE RANGE ( boundary_value )
} [ ; ]
```

## ALTER PARTITION SCHEME (Transact-SQL)

`docs/t-sql/statements/alter-partition-scheme-transact-sql.md`

### Syntax

```syntaxsql
ALTER PARTITION SCHEME partition_scheme_name
NEXT USED [ filegroup_name ] [ ; ]
```

## ALTER PROCEDURE (Transact-SQL)

`docs/t-sql/statements/alter-procedure-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server and Azure SQL Database

ALTER { PROC | PROCEDURE } [schema_name.] procedure_name [ ; number ]
    [ { @parameter_name [ type_schema_name. ] data_type }
        [ VARYING ] [ = default ] [ OUT | OUTPUT ] [READONLY]
    ] [ ,...n ]
[ WITH <procedure_option> [ ,...n ] ]
[ FOR REPLICATION ]
AS { [ BEGIN ] sql_statement [;] [ ...n ] [ END ] }
[;]

<procedure_option> ::=
    [ ENCRYPTION ]
    [ RECOMPILE ]
    [ EXECUTE AS Clause ]
```

```syntaxsql
-- Syntax for SQL Server CLR Stored Procedure

ALTER { PROC | PROCEDURE } [schema_name.] procedure_name [ ; number ]
    [ { @parameter_name [ type_schema_name. ] data_type }
        [ = default ] [ OUT | OUTPUT ] [READONLY]
    ] [ ,...n ]
[ WITH EXECUTE AS Clause ]
AS { EXTERNAL NAME assembly_name.class_name.method_name }
[;]
```

```syntaxsql
-- Syntax for Azure Synapse Analytics and Parallel Data Warehouse and Microsoft Fabric

ALTER { PROC | PROCEDURE } [schema_name.] procedure_name
    [ { @parameterdata_type } [= ] ] [ ,...n ]
AS { [ BEGIN ] sql_statement [ ; ] [ ,...n ] [ END ] }
[;]
```

## ALTER QUEUE (Transact-SQL)

`docs/t-sql/statements/alter-queue-transact-sql.md`

### Syntax

```syntaxsql
ALTER QUEUE <object>
   queue_settings
   | queue_action
[ ; ]

<object> : :=
{ database_name.schema_name.queue_name | schema_name.queue_name | queue_name }

<queue_settings> : :=
WITH
   [ STATUS = { ON | OFF } [ , ] ]
   [ RETENTION = { ON | OFF } [ , ] ]
   [ ACTIVATION (
       { [ STATUS = { ON | OFF } [ , ] ]
         [ PROCEDURE_NAME = <procedure> [ , ] ]
         [ MAX_QUEUE_READERS = max_readers [ , ] ]
         [ EXECUTE AS { SELF | 'user_name'  | OWNER } ]
       |  DROP }
          ) [ , ]]
         [ POISON_MESSAGE_HANDLING (
          STATUS = { ON | OFF } )
         ]

<queue_action> : :=
   REBUILD [ WITH <query_rebuild_options> ]
   | REORGANIZE [ WITH (LOB_COMPACTION = { ON | OFF } ) ]
   | MOVE TO { file_group | "default" }

<procedure> : :=
{ database_name.schema_name.stored_procedure_name | schema_name.stored_procedure_name | stored_procedure_name }

<queue_rebuild_options> : :=
{
   ( MAXDOP = max_degree_of_parallelism )
}
```

## ALTER REMOTE SERVICE BINDING (Transact-SQL)

`docs/t-sql/statements/alter-remote-service-binding-transact-sql.md`

### Syntax

```syntaxsql
ALTER REMOTE SERVICE BINDING binding_name
   WITH [ USER = <user_name> ] [ , ANONYMOUS = { ON | OFF } ]
[ ; ]
```

## ALTER RESOURCE GOVERNOR (Transact-SQL)

`docs/t-sql/statements/alter-resource-governor-transact-sql.md`

### Syntax

```syntaxsql
ALTER RESOURCE GOVERNOR
    { RECONFIGURE
      | DISABLE
      | RESET STATISTICS
      | WITH
              ( [ CLASSIFIER_FUNCTION = { schema_name.function_name | NULL } ]
                [ [ , ] MAX_OUTSTANDING_IO_PER_VOLUME = value ]
              )
    }
[ ; ]
```

## ALTER RESOURCE POOL (Transact-SQL)

`docs/t-sql/statements/alter-resource-pool-transact-sql.md`

### Syntax

```syntaxsql
ALTER RESOURCE POOL { pool_name | [default] }
[WITH
    ( [ MIN_CPU_PERCENT = value ]
    [ [ , ] MAX_CPU_PERCENT = value ]
    [ [ , ] CAP_CPU_PERCENT = value ]
    [ [ , ] AFFINITY {
                        SCHEDULER = AUTO
                      | ( <scheduler_range_spec> )
                      | NUMANODE = ( <NUMA_node_range_spec> )
                      }]
    [ [ , ] MIN_MEMORY_PERCENT = value ]
    [ [ , ] MAX_MEMORY_PERCENT = value ]
    [ [ , ] MIN_IOPS_PER_VOLUME = value ]
    [ [ , ] MAX_IOPS_PER_VOLUME = value ]
)]
[;]

<scheduler_range_spec> ::=
{SCHED_ID | SCHED_ID TO SCHED_ID}[,...n]

<NUMA_node_range_spec> ::=
{NUMA_node_ID | NUMA_node_ID TO NUMA_node_ID}[,...n]
```

## ALTER ROLE (Transact-SQL)

`docs/t-sql/statements/alter-role-transact-sql.md`

### Syntax

> Syntax for SQL Server (starting with 2012), Azure SQL Managed Instance, Azure SQL Database, and Microsoft Fabric.

```syntaxsql
ALTER ROLE  role_name
{
       ADD MEMBER database_principal
    |  DROP MEMBER database_principal
    |  WITH NAME = new_name
}
[;]
```

> Syntax for SQL Server prior to 2012.

```syntaxsql
-- Change the name of a user-defined database role
ALTER ROLE role_name
    WITH NAME = new_name
[;]
```

## ALTER ROUTE (Transact-SQL)

`docs/t-sql/statements/alter-route-transact-sql.md`

### Syntax

```syntaxsql
ALTER ROUTE route_name
WITH
  [ SERVICE_NAME = 'service_name' [ , ] ]
  [ BROKER_INSTANCE = 'broker_instance' [ , ] ]
  [ LIFETIME = route_lifetime [ , ] ]
  [ ADDRESS =  'next_hop_address' [ , ] ]
  [ MIRROR_ADDRESS = 'next_hop_mirror_address' ]
[ ; ]
```

## ALTER SCHEMA (Transact-SQL)

`docs/t-sql/statements/alter-schema-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server and Azure SQL Database

ALTER SCHEMA schema_name
   TRANSFER [ <entity_type> :: ] securable_name
[;]

<entity_type> ::=
    {
    Object | Type | XML Schema Collection
    }
```

```syntaxsql
-- Syntax for Azure Synapse Analytics and Parallel Data Warehouse and Microsoft Fabric

ALTER SCHEMA schema_name
   TRANSFER [ OBJECT :: ] securable_name
[;]
```

## ALTER SEARCH PROPERTY LIST (Transact-SQL)

`docs/t-sql/statements/alter-search-property-list-transact-sql.md`

### Syntax

```syntaxsql
ALTER SEARCH PROPERTY LIST list_name
{
   ADD 'property_name'
     WITH
      (
          PROPERTY_SET_GUID = 'property_set_guid'
        , PROPERTY_INT_ID = property_int_id
      [ , PROPERTY_DESCRIPTION = 'property_description' ]
      )
 | DROP 'property_name'
}
;
```

## ALTER SECURITY POLICY (Transact-SQL)

`docs/t-sql/statements/alter-security-policy-transact-sql.md`

### Syntax

```syntaxsql
ALTER SECURITY POLICY schema_name.security_policy_name
    [
        { ADD { FILTER | BLOCK } PREDICATE tvf_schema_name.security_predicate_function_name
           ( { column_name | arguments } [ , ...n ] ) ON table_schema_name.table_name
           [ <block_dml_operation> ]  }
        | { ALTER { FILTER | BLOCK } PREDICATE tvf_schema_name.new_security_predicate_function_name
             ( { column_name | arguments } [ , ...n ] ) ON table_schema_name.table_name
           [ <block_dml_operation> ] }
        | { DROP { FILTER | BLOCK } PREDICATE ON table_schema_name.table_name }
        | [ <additional_add_alter_drop_predicate_statements> [ , ...n ] ]
    ]    [ WITH ( STATE = { ON | OFF } ) ]
    [ NOT FOR REPLICATION ]
[;]

<block_dml_operation>
    [ { AFTER { INSERT | UPDATE } }
    | { BEFORE { UPDATE | DELETE } } ]
```

## ALTER SEQUENCE (Transact-SQL)

`docs/t-sql/statements/alter-sequence-transact-sql.md`

### Syntax

```syntaxsql
ALTER SEQUENCE [ schema_name. ] sequence_name
    [ RESTART [ WITH <constant> ] ]
    [ INCREMENT BY <constant> ]
    [ { MINVALUE <constant> } | { NO MINVALUE } ]
    [ { MAXVALUE <constant> } | { NO MAXVALUE } ]
    [ CYCLE | { NO CYCLE } ]
    [ { CACHE [ <constant> ] } | { NO CACHE } ]
[ ; ]
```

## ALTER SERVER AUDIT SPECIFICATION (Transact-SQL)

`docs/t-sql/statements/alter-server-audit-specification-transact-sql.md`

### Syntax

```syntaxsql
ALTER SERVER AUDIT SPECIFICATION audit_specification_name
{
    [ FOR SERVER AUDIT audit_name ]
    [ { { ADD | DROP } ( audit_action_group_name )
      } [, ...n] ]
    [ WITH ( STATE = { ON | OFF } ) ]
}
[ ; ]
```

## ALTER SERVER AUDIT (Transact-SQL)

`docs/t-sql/statements/alter-server-audit-transact-sql.md`

### Syntax

```syntaxsql
ALTER SERVER AUDIT audit_name
{
    [ TO { { FILE ( <file_options> [ , ...n ] ) } | APPLICATION_LOG | SECURITY_LOG } | URL ]
    [ WITH ( <audit_options> [ , ...n ] ) ]
    [ WHERE <predicate_expression> ]
}
| REMOVE WHERE
| MODIFY NAME = new_audit_name
[ ; ]

<file_options>::=
{
      FILEPATH = 'os_file_path'
    | MAXSIZE = { max_size { MB | GB | TB } | UNLIMITED }
    | MAX_ROLLOVER_FILES = { integer | UNLIMITED }
    | MAX_FILES = integer
    | RESERVE_DISK_SPACE = { ON | OFF }
}

<audit_options>::=
{
      QUEUE_DELAY = integer
    | ON_FAILURE = { CONTINUE | SHUTDOWN | FAIL_OPERATION }
    | STATE = = { ON | OFF }
}

<predicate_expression>::=
{
    [ NOT ] <predicate_factor>
    [ { AND | OR } [ NOT ] { <predicate_factor> } ]
    [ , ...n ]
}

<predicate_factor>::=
    event_field_name { = | < > | != | > | >= | < | <= } { number | 'string' }
```

## ALTER SERVER CONFIGURATION (Transact-SQL)

`docs/t-sql/statements/alter-server-configuration-transact-sql.md`

### Syntax

```syntaxsql
ALTER SERVER CONFIGURATION
SET <optionspec>
[;]

<optionspec> ::=
{
     <process_affinity>
   | <diagnostic_log>
   | <failover_cluster_property>
   | <hadr_cluster_context>
   | <buffer_pool_extension>
   | <soft_numa>
   | <memory_optimized>
   | <hardware_offload>
   | <suspend_for_snapshot_backup>
}

<process_affinity> ::=
   PROCESS AFFINITY
   {
     CPU = { AUTO | <CPU_range_spec> }
   | NUMANODE = <NUMA_node_range_spec>
   }
   <CPU_range_spec> ::=
      { CPU_ID | CPU_ID  TO CPU_ID } [ ,...n ]

   <NUMA_node_range_spec> ::=
      { NUMA_node_ID | NUMA_node_ID TO NUMA_node_ID } [ ,...n ]

<diagnostic_log> ::=
   DIAGNOSTICS LOG
   {
     ON
   | OFF
   | PATH = { 'os_file_path' | DEFAULT }
   | MAX_SIZE = { 'log_max_size' MB | DEFAULT }
   | MAX_FILES = { 'max_file_count' | DEFAULT }
   }

<failover_cluster_property> ::=
   FAILOVER CLUSTER PROPERTY <resource_property>
   <resource_property> ::=
      {
        VerboseLogging = { 'logging_detail' | DEFAULT }
      | SqlDumperDumpFlags = { 'dump_file_type' | DEFAULT }
      | SqlDumperDumpPath = { 'os_file_path'| DEFAULT }
      | SqlDumperDumpTimeOut = { 'dump_time-out' | DEFAULT }
      | FailureConditionLevel = { 'failure_condition_level' | DEFAULT }
      | HealthCheckTimeout = { 'health_check_time-out' | DEFAULT }
      | ClusterConnectionOptions = '<key_value_pairs>[;...]'
      }

<hadr_cluster_context> ::=
   HADR CLUSTER CONTEXT = { 'remote_windows_cluster' | LOCAL }

<buffer_pool_extension>::=
    BUFFER POOL EXTENSION
    { ON ( FILENAME = 'os_file_path_and_name' , SIZE = <size_spec> )
    | OFF }

    <size_spec> ::=
        { size [ KB | MB | GB ] }

<soft_numa> ::=
    SOFTNUMA
    { ON | OFF }

<memory-optimized> ::=
   MEMORY_OPTIMIZED
   {
     ON
   | OFF
   | [ TEMPDB_METADATA = { ON [(RESOURCE_POOL='resource_pool_name')] | OFF }
   | [ HYBRID_BUFFER_POOL = { ON | OFF }
   }

<hardware_offload> ::=
   HARDWARE_OFFLOAD
   {
     ON
   | OFF
   }

<suspend_for_snapshot_backup> ::=
    SET SUSPEND_FOR_SNAPSHOT_BACKUP = { ON | OFF } [ ( GROUP = ( <database>,...n) [ , MODE = COPY_ONLY ] ) ]
```

## ALTER SERVER ROLE (Transact-SQL)

`docs/t-sql/statements/alter-server-role-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server, Azure SQL Database, Azure SQL Managed Instance

ALTER SERVER ROLE server_role_name
{
    [ ADD MEMBER server_principal ]
  | [ DROP MEMBER server_principal ]
  | [ WITH NAME = new_server_role_name ]
} [ ; ]
```

```syntaxsql
-- Syntax for Parallel Data Warehouse

ALTER SERVER ROLE  server_role_name  ADD MEMBER login;

ALTER SERVER ROLE  server_role_name  DROP MEMBER login;
```

## ALTER SERVICE MASTER KEY (Transact-SQL)

`docs/t-sql/statements/alter-service-master-key-transact-sql.md`

### Syntax

```syntaxsql
ALTER SERVICE MASTER KEY
    [ { <regenerate_option> | <recover_option> } ] [;]

<regenerate_option> ::=
    [ FORCE ] REGENERATE

<recover_option> ::=
    { WITH OLD_ACCOUNT = 'account_name' , OLD_PASSWORD = 'password' }
    |
    { WITH NEW_ACCOUNT = 'account_name' , NEW_PASSWORD = 'password' }
```

## ALTER SERVICE (Transact-SQL)

`docs/t-sql/statements/alter-service-transact-sql.md`

### Syntax

```syntaxsql
ALTER SERVICE service_name
   [ ON QUEUE [ schema_name . ]queue_name ]
   [ ( < opt_arg > [ , ...n ] ) ]
[ ; ]

<opt_arg> ::=
   ADD CONTRACT contract_name | DROP CONTRACT contract_name
```

## ALTER SYMMETRIC KEY (Transact-SQL)

`docs/t-sql/statements/alter-symmetric-key-transact-sql.md`

### Syntax

```syntaxsql
ALTER SYMMETRIC KEY Key_name <alter_option>

<alter_option> ::=
   ADD ENCRYPTION BY <encrypting_mechanism> [ , ... n ]
   |
   DROP ENCRYPTION BY <encrypting_mechanism> [ , ... n ]
<encrypting_mechanism> ::=
   CERTIFICATE certificate_name
   |
   PASSWORD = 'password'
   |
   SYMMETRIC KEY Symmetric_Key_Name
   |
   ASYMMETRIC KEY Asym_Key_Name
```

## column_constraint (Transact-SQL)

`docs/t-sql/statements/alter-table-column-constraint-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, Azure SQL Managed Instance, SQL database in Microsoft Fabric

```syntaxsql
[ CONSTRAINT constraint_name ]
{
    [ NULL | NOT NULL ]
    { PRIMARY KEY | UNIQUE }
        [ CLUSTERED | NONCLUSTERED ]
        [ WITH FILLFACTOR = fillfactor ]
        [ WITH ( index_option [, ...n ] ) ]
        [ ON { partition_scheme_name (partition_column_name)
            | filegroup | "default" } ]
    | [ FOREIGN KEY ]
        REFERENCES [ schema_name . ] referenced_table_name
            [ ( ref_column ) ]
        [ ON DELETE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
        [ ON UPDATE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
        [ NOT FOR REPLICATION ]
    | CHECK [ NOT FOR REPLICATION ] ( logical_expression )
}
```

> Syntax for Microsoft Fabric Warehouse in Microsoft Fabric

```syntaxsql
[ CONSTRAINT constraint_name ]
{
    { PRIMARY KEY | UNIQUE }
       NONCLUSTERED
        (column [ ASC | DESC ] [ ,...n ] )
NOT ENFORCED
    | FOREIGN KEY
        ( column [ ,...n ] )
        REFERENCES referenced_table_name [ ( ref_column [ ,...n ] ) ]
NOT ENFORCED
}
```

## column_definition (Transact-SQL)

`docs/t-sql/statements/alter-table-column-definition-transact-sql.md`

### Syntax

```syntaxsql
column_name <data_type>
[ FILESTREAM ]
[ COLLATE collation_name ]
[ NULL | NOT NULL ]
[
    [ CONSTRAINT constraint_name ] DEFAULT constant_expression [ WITH VALUES ]
    | IDENTITY [ ( seed , increment ) ] [ NOT FOR REPLICATION ]
]
[ ROWGUIDCOL ]
[ SPARSE ]
[ ENCRYPTED WITH
  ( COLUMN_ENCRYPTION_KEY = key_name ,
      ENCRYPTION_TYPE = { DETERMINISTIC | RANDOMIZED } ,
      ALGORITHM =  'AEAD_AES_256_CBC_HMAC_SHA_256'
  ) ]
[ MASKED WITH ( FUNCTION = ' mask_function ') ]
[ <column_constraint> [ ...n ] ]

<data type> ::=
[ type_schema_name . ] type_name
    [ ( precision [ , scale ] | max |
        [ { CONTENT | DOCUMENT } ] xml_schema_collection ) ]

<column_constraint> ::=
[ CONSTRAINT constraint_name ]
{     { PRIMARY KEY | UNIQUE }
        [ CLUSTERED | NONCLUSTERED ]
        [
            WITH FILLFACTOR = fillfactor
          | WITH ( < index_option > [ , ...n ] )
        ]
        [ ON { partition_scheme_name ( partition_column_name )
            | filegroup | "default" } ]
  | [ FOREIGN KEY ]
        REFERENCES [ schema_name . ] referenced_table_name [ ( ref_column ) ]
        [ ON DELETE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
        [ ON UPDATE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
        [ NOT FOR REPLICATION ]
  | CHECK [ NOT FOR REPLICATION ] ( logical_expression )
}
```

## computed_column_definition (Transact-SQL)

`docs/t-sql/statements/alter-table-computed-column-definition-transact-sql.md`

### Syntax

```syntaxsql
column_name AS computed_column_expression
[ PERSISTED [ NOT NULL ] ]
[
    [ CONSTRAINT constraint_name ]
    { PRIMARY KEY | UNIQUE }
        [ CLUSTERED | NONCLUSTERED ]
        [ WITH FILLFACTOR = fillfactor ]
        [ WITH ( <index_option> [, ...n ] ) ]
        [ ON { partition_scheme_name ( partition_column_name ) | filegroup
            | "default" } ]
    | [ FOREIGN KEY ]
        REFERENCES ref_table [ ( ref_column ) ]
        [ ON DELETE { NO ACTION | CASCADE } ]
        [ ON UPDATE { NO ACTION } ]
        [ NOT FOR REPLICATION ]
    | CHECK [ NOT FOR REPLICATION ] ( logical_expression )
]
```

## ALTER TABLE index_option (Transact-SQL)

`docs/t-sql/statements/alter-table-index-option-transact-sql.md`

### Syntax

```syntaxsql
{
    PAD_INDEX = { ON | OFF }
  | FILLFACTOR = fillfactor
  | IGNORE_DUP_KEY = { ON | OFF }
  | STATISTICS_NORECOMPUTE = { ON | OFF }
  | ALLOW_ROW_LOCKS = { ON | OFF }
  | ALLOW_PAGE_LOCKS = { ON | OFF }
  | OPTIMIZE_FOR_SEQUENTIAL_KEY = { ON | OFF }
  | SORT_IN_TEMPDB = { ON | OFF }
  | MAXDOP = max_degree_of_parallelism
  | DATA_COMPRESSION = { NONE | ROW | PAGE | COLUMNSTORE | COLUMNSTORE_ARCHIVE }
      [ ON PARTITIONS ( { <partition_number_expression> | <range> }
      [ , ...n ] ) ]
  | XML_COMPRESSION = { ON | OFF }
      [ ON PARTITIONS ( { <partition_number_expression> | <range> }
      [ , ...n ] ) ]
  | ONLINE = { ON | OFF }
  | RESUMABLE = { ON | OFF }
  | MAX_DURATION = <time> [ MINUTES ]
}

<range> ::=
<partition_number_expression> TO <partition_number_expression>

<single_partition_rebuild__option> ::=
{
    SORT_IN_TEMPDB = { ON | OFF }
  | MAXDOP = max_degree_of_parallelism
  | DATA_COMPRESSION = { NONE | ROW | PAGE | COLUMNSTORE | COLUMNSTORE_ARCHIVE } }
  | ONLINE = { ON [ ( <low_priority_lock_wait> ) ] | OFF }
}

<low_priority_lock_wait>::=
{
    WAIT_AT_LOW_PRIORITY ( MAX_DURATION = <time> [ MINUTES ] ,
                           ABORT_AFTER_WAIT = { NONE | SELF | BLOCKERS } )
}
```

## table_constraint (Transact-SQL)

`docs/t-sql/statements/alter-table-table-constraint-transact-sql.md`

### Syntax

```syntaxsql
[ CONSTRAINT constraint_name ]
{
    { PRIMARY KEY | UNIQUE }
        [ CLUSTERED | NONCLUSTERED ]
        (column [ ASC | DESC ] [ ,...n ] )
        [ WITH FILLFACTOR = fillfactor ]
        [ WITH ( <index_option>[ , ...n ] ) ]
        [ ON { partition_scheme_name ( partition_column_name ... )
          | filegroup | "default" } ]
    | FOREIGN KEY
        ( column [ ,...n ] )
        REFERENCES referenced_table_name [ ( ref_column [ ,...n ] ) ]
        [ ON DELETE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
        [ ON UPDATE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
        [ NOT FOR REPLICATION ]
    | CONNECTION
        ( { node_table TO node_table }
          [ , {node_table TO node_table }]
          [ , ...n ]
        )
        [ ON DELETE { NO ACTION | CASCADE } ]
    | DEFAULT constant_expression FOR column [ WITH VALUES ]
    | CHECK [ NOT FOR REPLICATION ] ( logical_expression )
}
```

## ALTER TABLE (Transact-SQL)

`docs/t-sql/statements/alter-table-transact-sql.md`

### Syntax for disk-based tables

Marked for `=azuresqldb-current || >=sql-server-2017 || >=sql-server-linux-2017 || =azuresqldb-mi-current || =fabric-sqldb`.

```syntaxsql
ALTER TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
{
    ALTER COLUMN column_name
    {
        [ type_schema_name. ] type_name
            [ (
                {
                   precision [ , scale ]
                 | max
                 | xml_schema_collection
                }
            ) ]
        [ COLLATE collation_name ]
        [ NULL | NOT NULL ] [ SPARSE ]
      | { ADD | DROP }
          { ROWGUIDCOL | PERSISTED | NOT FOR REPLICATION | SPARSE | HIDDEN }
      | { ADD | DROP } MASKED [ WITH ( FUNCTION = ' mask_function ') ]
    }
    [ WITH ( ONLINE = ON | OFF ) ]
    | [ WITH { CHECK | NOCHECK } ]

    | ADD
    {
        <column_definition>
      | <computed_column_definition>
      | <table_constraint>
      | <column_set_definition>
    } [ ,...n ]
      | [ system_start_time_column_name datetime2 GENERATED ALWAYS AS ROW START
                [ HIDDEN ] [ NOT NULL ] [ CONSTRAINT constraint_name ]
            DEFAULT constant_expression [WITH VALUES] ,
                system_end_time_column_name datetime2 GENERATED ALWAYS AS ROW END
                   [ HIDDEN ] [ NOT NULL ][ CONSTRAINT constraint_name ]
            DEFAULT constant_expression [WITH VALUES] ,
                start_transaction_id_column_name bigint GENERATED ALWAYS AS TRANSACTION_ID START
                   [ HIDDEN ] NOT NULL [ CONSTRAINT constraint_name ]
            DEFAULT constant_expression [WITH VALUES],
                  end_transaction_id_column_name bigint GENERATED ALWAYS AS TRANSACTION_ID END
                   [ HIDDEN ] NULL [ CONSTRAINT constraint_name ]
            DEFAULT constant_expression [WITH VALUES],
                  start_sequence_number_column_name bigint GENERATED ALWAYS AS SEQUENCE_NUMBER START
                   [ HIDDEN ] NOT NULL [ CONSTRAINT constraint_name ]
            DEFAULT constant_expression [WITH VALUES],
                  end_sequence_number_column_name bigint GENERATED ALWAYS AS SEQUENCE_NUMBER END
                   [ HIDDEN ] NULL [ CONSTRAINT constraint_name ]
            DEFAULT constant_expression [WITH VALUES]
        ]
       PERIOD FOR SYSTEM_TIME ( system_start_time_column_name, system_end_time_column_name )
    | DROP
     [ {
         [ CONSTRAINT ][ IF EXISTS ]
         {
              constraint_name
              [ WITH
               ( <drop_clustered_constraint_option> [ ,...n ] )
              ]
          } [ ,...n ]
          | COLUMN [ IF EXISTS ]
          {
              column_name
          } [ ,...n ]
          | PERIOD FOR SYSTEM_TIME
     } [ ,...n ] ]
    | [ WITH { CHECK | NOCHECK } ] { CHECK | NOCHECK } CONSTRAINT
        { ALL | constraint_name [ ,...n ] }

    | { ENABLE | DISABLE } TRIGGER
        { ALL | trigger_name [ ,...n ] }

    | { ENABLE | DISABLE } CHANGE_TRACKING
        [ WITH ( TRACK_COLUMNS_UPDATED = { ON | OFF } ) ]

    | SWITCH [ PARTITION source_partition_number_expression ]
        TO target_table
        [ PARTITION target_partition_number_expression ]
        [ WITH ( <low_priority_lock_wait> ) ]

    | SET
        (
            [ FILESTREAM_ON =
                { partition_scheme_name | filegroup | "default" | "NULL" } ]
            | SYSTEM_VERSIONING =
                  {
                    OFF
                  | ON
                      [ ( HISTORY_TABLE = schema_name . history_table_name
                          [, DATA_CONSISTENCY_CHECK = { ON | OFF } ]
                          [, HISTORY_RETENTION_PERIOD =
                          {
                              INFINITE | number {DAY | DAYS | WEEK | WEEKS
                  | MONTH | MONTHS | YEAR | YEARS }
                          }
                          ]
                        )
                      ]
                  }
            | DATA_DELETION =
                {
                      OFF
                    | ON
                        [(  [ FILTER_COLUMN = column_name ]
                            [, RETENTION_PERIOD = { INFINITE | number { DAY | DAYS | WEEK | WEEKS
                                    | MONTH | MONTHS | YEAR | YEARS } } ]
                        )]
                    } )
    | REBUILD
      [ [PARTITION = ALL]
        [ WITH ( <rebuild_option> [ ,...n ] ) ]
      | [ PARTITION = partition_number
           [ WITH ( <single_partition_rebuild_option> [ ,...n ] ) ]
        ]
      ]

    | <table_option>
    | <filetable_option>
    | <stretch_configuration>
}
[ ; ]

-- ALTER TABLE options

<column_set_definition> ::=
    column_set_name XML COLUMN_SET FOR ALL_SPARSE_COLUMNS

<drop_clustered_constraint_option> ::=
    {
        MAXDOP = max_degree_of_parallelism
      | ONLINE = { ON | OFF }
      | MOVE TO
         { partition_scheme_name ( column_name ) | filegroup | "default" }
    }
<table_option> ::=
    {
        SET ( LOCK_ESCALATION = { AUTO | TABLE | DISABLE } )
    }

<filetable_option> ::=
    {
       [ { ENABLE | DISABLE } FILETABLE_NAMESPACE ]
       [ SET ( FILETABLE_DIRECTORY = directory_name ) ]
    }

<stretch_configuration> ::=
    {
      SET (
        REMOTE_DATA_ARCHIVE
        {
            = ON (<table_stretch_options>)
          | = OFF_WITHOUT_DATA_RECOVERY ( MIGRATION_STATE = PAUSED )
          | ( <table_stretch_options> [, ...n] )
        }
            )
    }

<table_stretch_options> ::=
    {
     [ FILTER_PREDICATE = { null | table_predicate_function } , ]
       MIGRATION_STATE = { OUTBOUND | INBOUND | PAUSED }
    }

<single_partition_rebuild__option> ::=
{
      SORT_IN_TEMPDB = { ON | OFF }
    | MAXDOP = max_degree_of_parallelism
    | DATA_COMPRESSION = { NONE | ROW | PAGE | COLUMNSTORE | COLUMNSTORE_ARCHIVE}
    | ONLINE = { ON [( <low_priority_lock_wait> ) ] | OFF }
}

<low_priority_lock_wait>::=
{
    WAIT_AT_LOW_PRIORITY ( MAX_DURATION = <time> [ MINUTES ],
        ABORT_AFTER_WAIT = { NONE | SELF | BLOCKERS } )
}
```

### Syntax for memory-optimized tables

Marked for `=azuresqldb-current || >=sql-server-2017 || >=sql-server-linux-2017 || =azuresqldb-mi-current || =fabric-sqldb`.

```syntaxsql
ALTER TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
{
    ALTER COLUMN column_name
    {
        [ type_schema_name. ] type_name
            [ (
                {
                   precision [ , scale ]
                }
            ) ]
        [ COLLATE collation_name ]
        [ NULL | NOT NULL ]
    }

    | ALTER INDEX index_name
    {
        [ type_schema_name. ] type_name
        REBUILD
        [ [ NONCLUSTERED ] WITH ( BUCKET_COUNT = bucket_count )
        ]
    }

    | ADD
    {
        <column_definition>
      | <computed_column_definition>
      | <table_constraint>
      | <table_index>
      | <column_index>
    } [ ,...n ]

    | DROP
     [ {
         CONSTRAINT [ IF EXISTS ]
         {
              constraint_name
          } [ ,...n ]
        | INDEX [ IF EXISTS ]
      {
         index_name
       } [ ,...n ]
          | COLUMN [ IF EXISTS ]
          {
              column_name
          } [ ,...n ]
          | PERIOD FOR SYSTEM_TIME
     } [ ,...n ] ]
    | [ WITH { CHECK | NOCHECK } ] { CHECK | NOCHECK } CONSTRAINT
        { ALL | constraint_name [ ,...n ] }

    | { ENABLE | DISABLE } TRIGGER
        { ALL | trigger_name [ ,...n ] }

    | SWITCH [ [ PARTITION ] source_partition_number_expression ]
        TO target_table
        [ PARTITION target_partition_number_expression ]
        [ WITH ( <low_priority_lock_wait> ) ]

}
[ ; ]

-- ALTER TABLE options

< table_constraint > ::=
 [ CONSTRAINT constraint_name ]
{
   {PRIMARY KEY | UNIQUE }
     {
       NONCLUSTERED (column [ ASC | DESC ] [ ,... n ])
       | NONCLUSTERED HASH (column [ ,... n ] ) WITH ( BUCKET_COUNT = bucket_count )
     }
    | FOREIGN KEY
        ( column [ ,...n ] )
        REFERENCES referenced_table_name [ ( ref_column [ ,...n ] ) ]
    | CHECK ( logical_expression )
}

<column_index> ::=
  INDEX index_name
{ [ NONCLUSTERED ] | [ NONCLUSTERED ] HASH WITH (BUCKET_COUNT = bucket_count) }

<table_index> ::=
  INDEX index_name
{ [ NONCLUSTERED ] HASH (column [ ,... n ] ) WITH (BUCKET_COUNT = bucket_count)
  | [ NONCLUSTERED ] (column [ ASC | DESC ] [ ,... n ] )
      [ ON filegroup_name | default ]
  | CLUSTERED COLUMNSTORE [ WITH ( COMPRESSION_DELAY = { 0 | delay [MINUTES] } ) ]
      [ ON filegroup_name | default ]
}
```

### Syntax for Azure Synapse Analytics and Parallel Data Warehouse

Marked for `>=aps-pdw-2016 || =azure-sqldw-latest`.

```syntaxsql
ALTER TABLE { database_name.schema_name.source_table_name | schema_name.source_table_name | source_table_name }
{
    ALTER COLUMN column_name
        {
            type_name [ ( precision [ , scale ] ) ]
            [ COLLATE Windows_collation_name ]
            [ NULL | NOT NULL ]
        }
    | ADD { <column_definition> | <column_constraint> FOR column_name} [ ,...n ]
    | DROP { COLUMN column_name | [CONSTRAINT] constraint_name } [ ,...n ]
    | REBUILD {
            [ PARTITION = ALL [ WITH ( <rebuild_option> ) ] ]
          | [ PARTITION = partition_number [ WITH ( <single_partition_rebuild_option> ] ]
      }
    | { SPLIT | MERGE } RANGE (boundary_value)
    | SWITCH [ PARTITION source_partition_number
        TO target_table_name [ PARTITION target_partition_number ] [ WITH ( TRUNCATE_TARGET = ON | OFF ) ] ]
}
[ ; ]

<column_definition>::=
{
    column_name
    type_name [ ( precision [ , scale ] ) ]
    [ <column_constraint> ]
    [ COLLATE Windows_collation_name ]
    [ NULL | NOT NULL ]
}

<column_constraint>::=
    [ CONSTRAINT constraint_name ]
    {
        DEFAULT constant_expression
        | PRIMARY KEY NONCLUSTERED (column_name [ ,... n ]) NOT ENFORCED -- Applies to Azure Synapse Analytics only
        | UNIQUE (column_name [ ,... n ]) NOT ENFORCED -- Applies to Azure Synapse Analytics only
    }
<rebuild_option > ::=
{
    DATA_COMPRESSION = { COLUMNSTORE | COLUMNSTORE_ARCHIVE }
        [ ON PARTITIONS ( {<partition_number> [ TO <partition_number>] } [ , ...n ] ) ]
    | XML_COMPRESSION = { ON | OFF }
        [ ON PARTITIONS ( {<partition_number> [ TO <partition_number>] } [ , ...n ] ) ]
}

<single_partition_rebuild_option > ::=
{
    DATA_COMPRESSION = { COLUMNSTORE | COLUMNSTORE_ARCHIVE }
}
```

### Syntax for Warehouse in Fabric

Marked for `=fabric`.

```syntaxsql
ALTER TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
{
    ALTER COLUMN column_name
    {
        [ type_schema_name. ] type_name
            [ (
                {
                    precision [ , scale ]
                }
            ) ]
        [ COLLATE collation_name ]
        [ NULL | NOT NULL ]
      | { ADD | DROP } MASKED [ WITH ( FUNCTION = ' mask_function ') ]
    }
}
{
  ADD  { column_name <data_type> [COLLATE collation_name] [ <column_options> ] } [ ,...n ]
| ADD { <column_constraint> FOR column_name} [ ,...n ]
| DROP { COLUMN column_name | [CONSTRAINT] constraint_name } [ ,...n ]
}
[ ; ]

<column_options> ::=
[ NULL ] -- default is NULL

<data type> ::= type_name [ ( precision [ , scale ] ) ]

<column_constraint>::=
    [ CONSTRAINT constraint_name ]
    {
       PRIMARY KEY NONCLUSTERED (column_name [ ,... n ]) NOT ENFORCED
        | UNIQUE NONCLUSTERED (column_name [ ,... n ]) NOT ENFORCED
    | FOREIGN KEY
        ( column [ ,...n ] )
        REFERENCES referenced_table_name [ ( ref_column [ ,...n ] ) ] NOT ENFORCED
    }
```

## ALTER TRIGGER (Transact-SQL)

`docs/t-sql/statements/alter-trigger-transact-sql.md`

### Syntax

```syntaxsql
-- SQL Server Syntax
-- Trigger on an INSERT, UPDATE, or DELETE statement to a table or view (DML Trigger)

ALTER TRIGGER schema_name.trigger_name
ON  ( table | view )
[ WITH <dml_trigger_option> [ ,...n ] ]
 ( FOR | AFTER | INSTEAD OF )
{ [ DELETE ] [ , ] [ INSERT ] [ , ] [ UPDATE ] }
[ NOT FOR REPLICATION ]
AS { sql_statement [ ; ] [ ...n ] | EXTERNAL NAME <method specifier>
[ ; ] }

<dml_trigger_option> ::=
    [ ENCRYPTION ]
    [ <EXECUTE AS Clause> ]

<method_specifier> ::=
    assembly_name.class_name.method_name

-- Trigger on an INSERT, UPDATE, or DELETE statement to a table
-- (DML Trigger on memory-optimized tables)

ALTER TRIGGER schema_name.trigger_name
ON  ( table  )
[ WITH <dml_trigger_option> [ ,...n ] ]
 ( FOR | AFTER )
{ [ DELETE ] [ , ] [ INSERT ] [ , ] [ UPDATE ] }
AS { sql_statement [ ; ] [ ...n ] }

<dml_trigger_option> ::=
    [ NATIVE_COMPILATION ]
    [ SCHEMABINDING ]
    [ <EXECUTE AS Clause> ]

-- Trigger on a CREATE, ALTER, DROP, GRANT, DENY, REVOKE,
-- or UPDATE statement (DDL Trigger)

ALTER TRIGGER trigger_name
ON { DATABASE | ALL SERVER }
[ WITH <ddl_trigger_option> [ ,...n ] ]
{ FOR | AFTER } { event_type [ ,...n ] | event_group }
AS { sql_statement [ ; ] | EXTERNAL NAME <method specifier>
[ ; ] }
}

<ddl_trigger_option> ::=
    [ ENCRYPTION ]
    [ <EXECUTE AS Clause> ]

<method_specifier> ::=
    assembly_name.class_name.method_name

-- Trigger on a LOGON event (Logon Trigger)

ALTER TRIGGER trigger_name
ON ALL SERVER
[ WITH <logon_trigger_option> [ ,...n ] ]
{ FOR| AFTER } LOGON
AS { sql_statement  [ ; ] [ ,...n ] | EXTERNAL NAME < method specifier >
  [ ; ] }

<logon_trigger_option> ::=
    [ ENCRYPTION ]
    [ EXECUTE AS Clause ]

<method_specifier> ::=
    assembly_name.class_name.method_name
```

```syntaxsql
-- Azure SQL Database Syntax
-- Trigger on an INSERT, UPDATE, or DELETE statement to a table or view (DML Trigger)

ALTER TRIGGER schema_name. trigger_name
ON (table | view )
 [ WITH <dml_trigger_option> [ ,...n ] ]
 ( FOR | AFTER | INSTEAD OF )
{ [ DELETE ] [ , ] [ INSERT ] [ , ] [ UPDATE ] }
AS { sql_statement [ ; ] [...n ] }

<dml_trigger_option> ::=
    [ <EXECUTE AS Clause> ]

-- Trigger on a CREATE, ALTER, DROP, GRANT, DENY, REVOKE, or UPDATE statement (DDL Trigger)

ALTER TRIGGER trigger_name
ON { DATABASE }
 [ WITH <ddl_trigger_option> [ ,...n ] ]
{ FOR | AFTER } { event_type [ ,...n ] | event_group }
AS { sql_statement
[ ; ] }
}

<ddl_trigger_option> ::=
    [ <EXECUTE AS Clause> ]
```

## ALTER USER (Transact-SQL)

`docs/t-sql/statements/alter-user-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server

ALTER USER user_name
 WITH <set_item> [ ,...n ]
[;]

<set_item> ::=
NAME = new_user_name
| DEFAULT_SCHEMA = { schema_name | NULL }
| LOGIN = login_name
| PASSWORD = 'password' [ OLD_PASSWORD = 'oldpassword' ]
| DEFAULT_LANGUAGE = { NONE | <lcid> | <language name> | <language alias> }
| ALLOW_ENCRYPTED_VALUE_MODIFICATIONS = [ ON | OFF ]
```

> **Syntax for Azure SQL Database**

```syntaxsql
-- Syntax for Azure SQL Database

ALTER USER user_name
 WITH <set_item> [ ,...n ]

<set_item> ::=
NAME = new_user_name
| DEFAULT_SCHEMA = schema_name
| LOGIN = login_name
| ALLOW_ENCRYPTED_VALUE_MODIFICATIONS = [ ON | OFF ]
[;]

-- Azure SQL Database Update Syntax
ALTER USER user_name
 WITH <set_item> [ ,...n ]
[;]

<set_item> ::=
NAME = new_user_name
| DEFAULT_SCHEMA = { schema_name | NULL }
| LOGIN = login_name
| PASSWORD = 'password' [ OLD_PASSWORD = 'oldpassword' ]
| ALLOW_ENCRYPTED_VALUE_MODIFICATIONS = [ ON | OFF ]

-- SQL Database syntax when connected to a federation member
ALTER USER user_name
 WITH <set_item> [ ,... n ]
[;]

<set_item> ::=
 NAME = new_user_name
```

> **Syntax for SQL database in Microsoft Fabric**

```syntaxsql
-- Syntax for SQL database in Fabric
ALTER USER
    {
    Microsoft_Entra_principal FROM EXTERNAL PROVIDER [WITH OBJECT_ID = 'objectid']
    }
 [ ; ]

-- Users that cannot authenticate
ALTER USER user_name
    {
         { FOR | FROM } CERTIFICATE cert_name
       | { FOR | FROM } ASYMMETRIC KEY asym_key_name
    }
 [ ; ]

<options_list> ::=
    DEFAULT_LANGUAGE = { NONE | lcid | language name | language alias }

-- SQL Database syntax when connected to a federation member
ALTER USER user_name
[;]
```

> **Syntax for Microsoft Fabric Warehouse in Microsoft Fabric**

```syntaxsql
-- Syntax for Fabric Data Warehouse

ALTER USER user_name
 WITH <set_item> [ ,...n ]

<set_item> ::=
 | DEFAULT_SCHEMA = schema_name
[;]
```

> > </br> </br> There is a new syntax extension that was added to help remap users in a database that was migrated to Azure SQL Managed Instance. The ALTER USER syntax helps map database users in a federated and synchronized domain with Microsoft Entra ID, to Microsoft Entra logins.

```syntaxsql
-- Syntax for SQL Managed Instance
ALTER USER user_name
 { WITH <set_item> [ ,...n ] | FROM EXTERNAL PROVIDER }
[;]

<set_item> ::=
NAME = new_user_name
| DEFAULT_SCHEMA = { schema_name | NULL }
| LOGIN = login_name
| PASSWORD = 'password' [ OLD_PASSWORD = 'oldpassword' ]
| DEFAULT_LANGUAGE = { NONE | <lcid> | <language name> | <language alias> }
| ALLOW_ENCRYPTED_VALUE_MODIFICATIONS = [ ON | OFF ]

-- Users or groups that are migrated as federated and synchronized with Azure AD have the following syntax:

/** Applies to Windows users that were migrated and have the following user names:
- Windows user <domain\user>
- Windows group <domain\MyWindowsGroup>
- Windows alias <MyWindowsAlias>
**/

ALTER USER user_name
 { WITH <set_item> [ ,...n ] | FROM EXTERNAL PROVIDER }
[;]

<set_item> ::=
 NAME = new_user_name
| DEFAULT_SCHEMA = { schema_name | NULL }
| LOGIN = login_name
| DEFAULT_LANGUAGE = { NONE | <lcid> | <language name> | <language alias> }
```

```syntaxsql
-- Syntax for Azure Synapse

ALTER USER user_name
 WITH <set_item> [ ,...n ]

<set_item> ::=
 NAME = new_user_name
 | LOGIN = login_name
 | DEFAULT_SCHEMA = schema_name
[;]
```

```syntaxsql
-- Syntax for Analytics Platform System

ALTER USER user_name
 WITH <set_item> [ ,...n ]

<set_item> ::=
 NAME = new_user_name
 | LOGIN = login_name
 | DEFAULT_SCHEMA = schema_name
[;]
```

## ALTER VIEW (Transact-SQL)

`docs/t-sql/statements/alter-view-transact-sql.md`

### Syntax

```syntaxsql
ALTER VIEW [ schema_name . ] view_name [ ( column [ ,...n ] ) ]
[ WITH <view_attribute> [ ,...n ] ]
AS select_statement
[ WITH CHECK OPTION ] [ ; ]

<view_attribute> ::=
{
    [ ENCRYPTION ]
    [ SCHEMABINDING ]
    [ VIEW_METADATA ]
}
```

```syntaxsql
-- Syntax for Azure Synapse Analytics and Parallel Data Warehouse and Microsoft Fabric

ALTER VIEW [ schema_name . ] view_name [  ( column_name [ ,...n ] ) ]
AS <select_statement>
[;]
```

## ALTER WORKLOAD GROUP (Transact-SQL)

`docs/t-sql/statements/alter-workload-group-transact-sql.md`

### Syntax

```syntaxsql
ALTER WORKLOAD GROUP { group_name | [ default ] }
[ WITH
    ( [ IMPORTANCE = { LOW | MEDIUM | HIGH } ]
      [ [ , ] REQUEST_MAX_MEMORY_GRANT_PERCENT = value ]
      [ [ , ] REQUEST_MAX_CPU_TIME_SEC = value ]
      [ [ , ] REQUEST_MEMORY_GRANT_TIMEOUT_SEC = value ]
      [ [ , ] MAX_DOP = value ]
      [ [ , ] GROUP_MAX_REQUESTS = value ]
      [ [ , ] GROUP_MAX_TEMPDB_DATA_MB = value ]
      [ [ , ] GROUP_MAX_TEMPDB_DATA_PERCENT = value ] )
]
[ USING { pool_name | [default] } ]
[ ; ]
```

```syntaxsql
ALTER WORKLOAD GROUP { group_name | [ default ] }
[ WITH
    ( [ IMPORTANCE = { LOW | MEDIUM | HIGH } ]
      [ [ , ] REQUEST_MAX_MEMORY_GRANT_PERCENT = value ]
      [ [ , ] REQUEST_MAX_CPU_TIME_SEC = value ]
      [ [ , ] REQUEST_MEMORY_GRANT_TIMEOUT_SEC = value ]
      [ [ , ] MAX_DOP = value ]
      [ [ , ] GROUP_MAX_REQUESTS = value ]
      [ [ , ] GROUP_MAX_TEMPDB_DATA_MB = value ]
      [ [ , ] GROUP_MAX_TEMPDB_DATA_PERCENT = value ] )
]
[ USING { pool_name | [default] } ]
[ ; ]
```

```syntaxsql
ALTER WORKLOAD GROUP group_name
WITH
([ MIN_PERCENTAGE_RESOURCE = value ]
  [ [ , ] CAP_PERCENTAGE_RESOURCE = value ]
  [ [ , ] REQUEST_MIN_RESOURCE_GRANT_PERCENT = value ]
  [ [ , ] REQUEST_MAX_RESOURCE_GRANT_PERCENT = value ]
  [ [ , ] IMPORTANCE = { LOW | BELOW_NORMAL | NORMAL | ABOVE_NORMAL | HIGH }]
  [ [ , ] QUERY_EXECUTION_TIMEOUT_SEC = value ] )
  [ ; ]
```

## ALTER XML SCHEMA COLLECTION (Transact-SQL)

`docs/t-sql/statements/alter-xml-schema-collection-transact-sql.md`

### Syntax

```syntaxsql
ALTER XML SCHEMA COLLECTION [ relational_schema. ]sql_identifier ADD 'Schema Component'
```

## BACKUP CERTIFICATE (Transact-SQL)

`docs/t-sql/statements/backup-certificate-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server

BACKUP CERTIFICATE certname TO FILE = 'path_to_file'
    [ WITH
      [FORMAT = 'PFX',]
      PRIVATE KEY
      (
        FILE = 'path_to_private_key_file' ,
        ENCRYPTION BY PASSWORD = 'encryption_password'
        [ , DECRYPTION BY PASSWORD = 'decryption_password' ]
      )
    ]
```

```syntaxsql
-- Syntax for Parallel Data Warehouse

BACKUP CERTIFICATE certname TO FILE ='path_to_file'
      WITH PRIVATE KEY
      (
        FILE ='path_to_private_key_file',
        ENCRYPTION BY PASSWORD ='encryption_password'
      )
```

## BACKUP MASTER KEY (Transact-SQL)

`docs/t-sql/statements/backup-master-key-transact-sql.md`

### Syntax

```syntaxsql
BACKUP MASTER KEY TO
  {
    FILE = 'path_to_file'
  | URL = 'Azure Blob storage URL'
  }
    ENCRYPTION BY PASSWORD = 'password'
```

## BACKUP SERVICE MASTER KEY (Transact-SQL)

`docs/t-sql/statements/backup-service-master-key-transact-sql.md`

### Syntax

```syntaxsql
BACKUP SERVICE MASTER KEY TO FILE = 'path_to_file'
    ENCRYPTION BY PASSWORD = 'password'
```

## BACKUP SYMMETRIC KEY (Transact-SQL)

`docs/t-sql/statements/backup-symmetric-key-transact-sql.md`

### Syntax

```syntaxsql
BACKUP SYMMETRIC KEY key_name TO
  {
    FILE = 'path_to_file'
  | URL = 'Azure Blob storage URL'
  }
      ENCRYPTION BY PASSWORD = 'password'
```

## BACKUP (Transact-SQL)

`docs/t-sql/statements/backup-transact-sql.md`

### Syntax

```syntaxsql
--Back up a whole database
BACKUP DATABASE { database_name | @database_name_var }
  TO <backup_device> [ , ...n ]
  [ <MIRROR TO clause> ] [ next-mirror-to ]
  [ WITH { DIFFERENTIAL
           | <general_WITH_options> [ , ...n ] } ]
[ ; ]

--Back up specific files or filegroups
BACKUP DATABASE { database_name | @database_name_var }
 <file_or_filegroup> [ , ...n ]
  TO <backup_device> [ , ...n ]
  [ <MIRROR TO clause> ] [ next-mirror-to ]
  [ WITH { DIFFERENTIAL | <general_WITH_options> [ , ...n ] } ]
[ ; ]

--Create a partial backup
BACKUP DATABASE { database_name | @database_name_var }
 READ_WRITE_FILEGROUPS [ , <read_only_filegroup> [ , ...n ] ]
  TO <backup_device> [ , ...n ]
  [ <MIRROR TO clause> ] [ next-mirror-to ]
  [ WITH { DIFFERENTIAL | <general_WITH_options> [ , ...n ] } ]
[ ; ]

--Back up the transaction log (full and bulk-logged recovery models)
BACKUP LOG
  { database_name | @database_name_var }
  TO <backup_device> [ , ...n ]
  [ <MIRROR TO clause> ] [ next-mirror-to ]
  [ WITH { <general_WITH_options> | <log_specific_options> } [ , ...n ] ]
[ ; ]

--Back up all the databases on an instance of SQL Server (a server)
ALTER SERVER CONFIGURATION
SET SUSPEND_FOR_SNAPSHOT_BACKUP ON
[ ; ]

BACKUP SERVER
  TO <backup_device> [ , ...n ]
  [ <MIRROR TO clause> ] [ next-mirror-to ]
  [ WITH { METADATA_ONLY
           | <general_WITH_options> [ , ...n ] } ]
[ ; ]

--Back up a group of databases
ALTER DATABASE <database>
SET SUSPEND_FOR_SNAPSHOT_BACKUP ON

ALTER DATABASE <...>
SET SUSPEND_FOR_SNAPSHOT_BACKUP ON
...

BACKUP GROUP { <database> [ , ... ] }
  TO <backup_device> [ , ...n ]
  [ <MIRROR TO clause> ] [ next-mirror-to ]
  [ WITH { METADATA_ONLY
           | <general_WITH_options> [ , ...n ] } ]
[ ; ]

<backup_device>::=
 {
  { logical_device_name | @logical_device_name_var }
 | {   DISK
     | TAPE
     | URL } =
     { 'physical_device_name' | @physical_device_name_var | 'NUL' }
 }

<MIRROR TO clause>::=
 MIRROR TO <backup_device> [ , ...n ]

<file_or_filegroup>::=
 {
   FILE = { logical_file_name | @logical_file_name_var }
 | FILEGROUP = { logical_filegroup_name | @logical_filegroup_name_var }
 }

<read_only_filegroup>::=
FILEGROUP = { logical_filegroup_name | @logical_filegroup_name_var }

<general_WITH_options> [ , ...n ] ::=
--Backup Set Options
   COPY_ONLY
 | [ COMPRESSION [ ( ALGORITHM = { MS_XPRESS | ZSTD | accelerator_algorithm } [ , LEVEL = { LOW | MEDIUM | HIGH } ] ) ] | NO_COMPRESSION ]
 | DESCRIPTION = { 'text' | @text_variable }
 | NAME = { backup_set_name | @backup_set_name_var }
 | CREDENTIAL
 | ENCRYPTION
 | FILE_SNAPSHOT
 | { EXPIREDATE = { 'date' | @date_var }
        | RETAINDAYS = { days | @days_var } }
 | { METADATA_ONLY | SNAPSHOT }

--Media set options
   { NOINIT | INIT }
 | { NOSKIP | SKIP }
 | { NOFORMAT | FORMAT }
 | MEDIADESCRIPTION = { 'text' | @text_variable }
 | MEDIANAME = { media_name | @media_name_variable }
 | BLOCKSIZE = { blocksize | @blocksize_variable }

--Data Transfer Options
   BUFFERCOUNT = { buffercount | @buffercount_variable }
 | MAXTRANSFERSIZE = { maxtransfersize | @maxtransfersize_variable }

--Error Management Options
   { NO_CHECKSUM | CHECKSUM }
 | { STOP_ON_ERROR | CONTINUE_AFTER_ERROR }

--Compatibility Options
   RESTART

--Monitoring Options
   STATS [ = percentage ]

--Tape Options
   { REWIND | NOREWIND }
 | { UNLOAD | NOUNLOAD }

--Encryption Options
 ENCRYPTION (ALGORITHM = { AES_128 | AES_192 | AES_256 | TRIPLE_DES_3KEY } , encryptor_options ) <encryptor_options> ::=
   SERVER CERTIFICATE = Encryptor_Name | SERVER ASYMMETRIC KEY = Encryptor_Name

<log_specific_options> [ , ...n ] ::=
--Log-specific Options
   { NORECOVERY | STANDBY = undo_file_name }
 | NO_TRUNCATE
```

```syntaxsql
BACKUP DATABASE { database_name | @database_name_var }
  TO URL = { 'physical_device_name' | @physical_device_name_var } [ , ...n ]
  WITH COPY_ONLY [ , { <general_WITH_options> } ]
[ ; ]

<general_WITH_options> [ , ...n ] ::=

--Media set options
   MEDIADESCRIPTION = { 'text' | @text_variable }
 | MEDIANAME = { media_name | @media_name_variable }
 | BLOCKSIZE = { blocksize | @blocksize_variable }

--Data Transfer Options
   BUFFERCOUNT = { buffercount | @buffercount_variable }
 | MAXTRANSFERSIZE = { maxtransfersize | @maxtransfersize_variable }

--Error Management Options
   { NO_CHECKSUM | CHECKSUM }
 | { STOP_ON_ERROR | CONTINUE_AFTER_ERROR }

--Compatibility Options
   RESTART

--Monitoring Options
   STATS [ = percentage ]

--Encryption Options
 ENCRYPTION (ALGORITHM = { AES_128 | AES_192 | AES_256 | TRIPLE_DES_3KEY } , encryptor_options ) <encryptor_options> ::=
   SERVER CERTIFICATE = Encryptor_Name | SERVER ASYMMETRIC KEY = Encryptor_Name
```

```syntaxsql
--Create a full backup of a user database or the master database.
BACKUP DATABASE database_name
    TO DISK = '\\UNC_path\backup_directory'
    [ WITH [ ( ] <with_options> [ , ...n ] [ ) ] ]
[ ; ]

--Create a differential backup of a user database.
BACKUP DATABASE database_name
    TO DISK = '\\UNC_path\backup_directory'
    WITH [ ( ] DIFFERENTIAL
    [ , <with_options> [ , ...n ] [ ) ] ]
[ ; ]

<with_options> ::=
    DESCRIPTION = 'text'
    | NAME = 'backup_name'
```

## BEGIN CONVERSATION TIMER (Transact-SQL)

`docs/t-sql/statements/begin-conversation-timer-transact-sql.md`

### Syntax

```syntaxsql
BEGIN CONVERSATION TIMER ( conversation_handle )
   TIMEOUT = timeout
[ ; ]
```

## BEGIN DIALOG CONVERSATION (Transact-SQL)

`docs/t-sql/statements/begin-dialog-conversation-transact-sql.md`

### Syntax

```syntaxsql
BEGIN DIALOG [ CONVERSATION ] @dialog_handle
   FROM SERVICE initiator_service_name
   TO SERVICE 'target_service_name'
       [ , { 'service_broker_guid' | 'CURRENT DATABASE' }]
   [ ON CONTRACT contract_name ]
   [ WITH
   [  { RELATED_CONVERSATION = related_conversation_handle
      | RELATED_CONVERSATION_GROUP = related_conversation_group_id } ]
   [ [ , ] LIFETIME = dialog_lifetime ]
   [ [ , ] ENCRYPTION = { ON | OFF }  ] ]
[ ; ]
```

## BULK INSERT (Transact-SQL)

`docs/t-sql/statements/bulk-insert-transact-sql.md`

### Syntax

Marked for `=azuresqldb-current || >=sql-server-2017 || >=sql-server-linux-2017 || =azuresqldb-mi-current`.

```syntaxsql
BULK INSERT
   { database_name.schema_name.table_or_view_name | schema_name.table_or_view_name | table_or_view_name }
      FROM 'data_file'
     [ WITH
    (
   [ [ , ] DATA_SOURCE = 'data_source_name' ]

   -- text formatting options
   [ [ , ] CODEPAGE = { 'RAW' | 'code_page' | 'ACP' | 'OEM' } ]
   [ [ , ] DATAFILETYPE = { 'char' | 'widechar' | 'native' | 'widenative' } ]
   [ [ , ] ROWTERMINATOR = 'row_terminator' ]
   [ [ , ] FIELDTERMINATOR = 'field_terminator' ]
   [ [ , ] FORMAT = 'CSV' ]
   [ [ , ] FIELDQUOTE = 'quote_characters' ]
   [ [ , ] FIRSTROW = first_row ]
   [ [ , ] LASTROW = last_row ]

   -- input file format options
   [ [ , ] FORMATFILE = 'format_file_path' ]
   [ [ , ] FORMATFILE_DATA_SOURCE = 'data_source_name' ]

   -- error handling options
   [ [ , ] MAXERRORS = max_errors ]
   [ [ , ] ERRORFILE = 'file_name' ]
   [ [ , ] ERRORFILE_DATA_SOURCE = 'errorfile_data_source_name' ]

   -- database options
   [ [ , ] KEEPIDENTITY ]
   [ [ , ] KEEPNULLS ]
   [ [ , ] FIRE_TRIGGERS ]
   [ [ , ] CHECK_CONSTRAINTS ]
   [ [ , ] TABLOCK ]

   -- source options
   [ [ , ] ORDER ( { column [ ASC | DESC ] } [ , ...n ] ) ]
   [ [ , ] ROWS_PER_BATCH = rows_per_batch ]
   [ [ , ] KILOBYTES_PER_BATCH = kilobytes_per_batch ]
   [ [ , ] BATCHSIZE = batch_size ]

    ) ]
```

Marked for `=fabric`.

```syntaxsql
BULK INSERT
   { database_name.schema_name.table_or_view_name | schema_name.table_or_view_name | table_or_view_name }
      FROM 'data_file'
     [ WITH
    (
   [ [ , ] DATA_SOURCE = 'data_source_name' ]

   -- text formatting options
   [ [ , ] CODEPAGE = { 'code_page' | 'ACP' } ]
   [ [ , ] DATAFILETYPE = { 'char' | 'widechar' } ]
   [ [ , ] ROWTERMINATOR = 'row_terminator' ]
   [ [ , ] FIELDTERMINATOR = 'field_terminator' ]
   [ [ , ] FORMAT = { 'CSV' | 'PARQUET' } ]
   [ [ , ] FIELDQUOTE = 'quote_characters' ]
   [ [ , ] FIRSTROW = first_row ]
   [ [ , ] LASTROW = last_row ]

   -- input file format options
   [ [ , ] FORMATFILE = 'format_file_path' ]
   [ [ , ] FORMATFILE_DATA_SOURCE = 'data_source_name' ]

   -- error handling options
   [ [ , ] MAXERRORS = max_errors ]
   [ [ , ] ERRORFILE = 'file_name' ]
   [ [ , ] ERRORFILE_DATA_SOURCE = 'errorfile_data_source_name' ]

    ) ]
```

## CLOSE MASTER KEY (Transact-SQL)

`docs/t-sql/statements/close-master-key-transact-sql.md`

### Syntax

```syntaxsql
CLOSE MASTER KEY
```

## CLOSE SYMMETRIC KEY (Transact-SQL)

`docs/t-sql/statements/close-symmetric-key-transact-sql.md`

### Syntax

```syntaxsql
CLOSE { SYMMETRIC KEY key_name | ALL SYMMETRIC KEYS }
```

## COLLATE (Transact-SQL)

`docs/t-sql/statements/collations.md`

### Syntax

```syntaxsql
COLLATE { <collation_name> | database_default }
<collation_name> ::=
    { Windows_collation_name } | { SQL_collation_name }
```

## COPY INTO (Transact-SQL)

`docs/t-sql/statements/copy-into-transact-sql.md`

### Syntax

Marked for `=azure-sqldw-latest`.

```syntaxsql
COPY INTO [ schema. ] table_name
[ (Column_list) ]
FROM '<external_location>' [ ,...n ]
WITH
 (
 [ FILE_TYPE = { 'CSV' | 'PARQUET' | 'ORC' } ]
 [ , FILE_FORMAT = EXTERNAL FILE FORMAT OBJECT ]
 [ , CREDENTIAL = (AZURE CREDENTIAL) ]
 [ , ERRORFILE = ' [ http(s)://storageaccount/container ] /errorfile_directory [ / ] ] '
 [ , ERRORFILE_CREDENTIAL = (AZURE CREDENTIAL) ]
 [ , MAXERRORS = max_errors ]
 [ , COMPRESSION = { 'Gzip' | 'DefaultCodec' | 'Snappy' } ]
 [ , FIELDQUOTE = 'string_delimiter' ]
 [ , FIELDTERMINATOR =  'field_terminator' ]
 [ , ROWTERMINATOR = 'row_terminator' ]
 [ , FIRSTROW = first_row ]
 [ , DATEFORMAT = 'date_format' ]
 [ , ENCODING = { 'UTF8' | 'UTF16' } ]
 [ , IDENTITY_INSERT = { 'ON' | 'OFF' } ]
 [ , AUTO_CREATE_TABLE = { 'ON' | 'OFF' } ]
)
```

Marked for `=fabric`.

```syntaxsql
COPY INTO [ warehouse_name. ] [ schema_name. ] table_name
[ (Column_list) ]
FROM '<external_location>' [ ,...n ]
WITH
 (
 [ FILE_TYPE = { 'CSV' | 'JSONL' | 'PARQUET' } ]
 [ , CREDENTIAL = (IDENTITY = '' , SECRET = '') ]
 [ , ERRORFILE = ' [ http(s)://storageaccount/container ] /errorfile_directory [ / ] ] '
 [ , ERRORFILE_CREDENTIAL = (AZURE CREDENTIAL) ]
 [ , MAXERRORS = max_errors ]
 [ , COMPRESSION = { 'Gzip' | 'Snappy' } ]
 [ , FIELDQUOTE = 'string_delimiter' ]
 [ , FIELDTERMINATOR =  'field_terminator' ]
 [ , ROWTERMINATOR = 'row_terminator' ]
 [ , FIRSTROW = first_row ]
 [ , DATEFORMAT = 'date_format' ]
 [ , ENCODING = { 'UTF8' | 'UTF16' } ]
 [ , PARSER_VERSION = { '1.0' | '2.0' } ]
 [ , MATCH_COLUMN_COUNT = { 'ON' | 'OFF' } ]
 [ , IDENTITY_INSERT = { 'ON' | 'OFF' } ]
)
```

## CREATE AGGREGATE (Transact-SQL)

`docs/t-sql/statements/create-aggregate-transact-sql.md`

### Syntax

```syntaxsql
CREATE AGGREGATE [ schema_name . ] aggregate_name
        (@param_name <input_sqltype>
        [ ,...n ] )
RETURNS <return_sqltype>
EXTERNAL NAME assembly_name [ .class_name ]

<input_sqltype> ::=
        system_scalar_type | { [ udt_schema_name. ] udt_type_name }

<return_sqltype> ::=
        system_scalar_type | { [ udt_schema_name. ] udt_type_name }
```

## CREATE APPLICATION ROLE (Transact-SQL)

`docs/t-sql/statements/create-application-role-transact-sql.md`

### Syntax

```syntaxsql
CREATE APPLICATION ROLE application_role_name
    WITH PASSWORD = 'password' [ , DEFAULT_SCHEMA = schema_name ]
```

## CREATE ASSEMBLY (Transact-SQL)

`docs/t-sql/statements/create-assembly-transact-sql.md`

### Syntax

```syntaxsql
CREATE ASSEMBLY assembly_name
[ AUTHORIZATION owner_name ]
FROM { <client_assembly_specifier> | <assembly_bits> [ , ...n ] }
[ WITH PERMISSION_SET = { SAFE | EXTERNAL_ACCESS | UNSAFE } ]
[ ; ]
<client_assembly_specifier> ::=
    '[ \\computer_name\ ] share_name\ [ path\ ] manifest_file_name'
    | '[ local_path\ ] manifest_file_name'

<assembly_bits> ::=
{ varbinary_literal | varbinary_expression }
```

## CREATE ASYMMETRIC KEY (Transact-SQL)

`docs/t-sql/statements/create-asymmetric-key-transact-sql.md`

### Syntax

```syntaxsql
CREATE ASYMMETRIC KEY asym_key_name
   [ AUTHORIZATION database_principal_name ]
   [ FROM <asym_key_source> ]
   [ WITH <key_option> ]
   [ ENCRYPTION BY <encrypting_mechanism> ]
   [ ; ]

<asym_key_source>::=
     FILE = 'path_to_strong-name_file'
   | EXECUTABLE FILE = 'path_to_executable_file'
   | ASSEMBLY assembly_name
   | PROVIDER provider_name

<key_option> ::=
   ALGORITHM = <algorithm>
      |
   PROVIDER_KEY_NAME = 'key_name_in_provider'
      |
      CREATION_DISPOSITION = { CREATE_NEW | OPEN_EXISTING }

<algorithm> ::=
      { RSA_4096 | RSA_3072 | RSA_2048 | RSA_1024 | RSA_512 }

<encrypting_mechanism> ::=
    PASSWORD = 'password'
```

## CREATE AVAILABILITY GROUP (Transact-SQL)

`docs/t-sql/statements/create-availability-group-transact-sql.md`

### Syntax

```syntaxsql
CREATE AVAILABILITY GROUP group_name
   WITH (<with_option_spec> [ ,...n ] )
   FOR [ DATABASE database_name [ ,...n ] ]
   REPLICA ON <add_replica_spec> [ ,...n ]
   AVAILABILITY GROUP ON <add_availability_group_spec> [ ,...2 ]
   [ LISTENER 'dns_name' ( <listener_option> ) ]
[ ; ]

<with_option_spec>::=
    AUTOMATED_BACKUP_PREFERENCE = { PRIMARY | SECONDARY_ONLY| SECONDARY | NONE }
  | FAILURE_CONDITION_LEVEL  = { 1 | 2 | 3 | 4 | 5 }
  | HEALTH_CHECK_TIMEOUT = milliseconds
  | DB_FAILOVER  = { ON | OFF }
  | DTC_SUPPORT  = { PER_DB | NONE }
  | [ BASIC | DISTRIBUTED | CONTAINED [ REUSE_SYSTEM_DATABASES | AUTOSEEDING_SYSTEM_DATABASES ] ]
  | REQUIRED_SYNCHRONIZED_SECONDARIES_TO_COMMIT = { integer }
  | CLUSTER_TYPE = { WSFC | EXTERNAL | NONE }
  | WRITE_LEASE_VALIDITY = { seconds }
  | CLUSTER_CONNECTION_OPTIONS = 'key_value_pairs>[;...]`

<add_replica_spec>::=
  <server_instance> WITH
    (
       ENDPOINT_URL = 'TCP://system-address:port',
       AVAILABILITY_MODE = { SYNCHRONOUS_COMMIT | ASYNCHRONOUS_COMMIT | CONFIGURATION_ONLY },
       FAILOVER_MODE = { AUTOMATIC | MANUAL | EXTERNAL }
       [ , <add_replica_option> [ ,...n ] ]
    )

  <add_replica_option>::=
       SEEDING_MODE = { AUTOMATIC | MANUAL }
     | BACKUP_PRIORITY = n
     | SECONDARY_ROLE ( {
            [ ALLOW_CONNECTIONS = { NO | READ_ONLY | ALL } ]
        [,] [ READ_ONLY_ROUTING_URL = 'TCP://system-address:port' ]
     } )
     | PRIMARY_ROLE ( {
            [ ALLOW_CONNECTIONS = { READ_WRITE | ALL } ]
        [,] [ READ_ONLY_ROUTING_LIST = { ( '<server_instance>' [ ,...n ] ) | NONE } ]
        [,] [ READ_WRITE_ROUTING_URL = 'TCP://system-address:port' ]
     } )
     | SESSION_TIMEOUT = integer

<add_availability_group_spec>::=
 <ag_name> WITH
    (
       LISTENER_URL = 'TCP://system-address:port',
       AVAILABILITY_MODE = { SYNCHRONOUS_COMMIT | ASYNCHRONOUS_COMMIT },
       FAILOVER_MODE = MANUAL,
       SEEDING_MODE = { AUTOMATIC | MANUAL }
    )

<listener_option> ::=
   {
      WITH DHCP [ ON ( <network_subnet_option> ) ]
    | WITH IP ( { ( <ip_address_option> ) } [ , ...n ] ) [ , PORT = listener_port ]
   }

  <network_subnet_option> ::=
     'ip4_address', 'four_part_ipv4_mask'

  <ip_address_option> ::=
     {
        'ip4_address', 'pv4_mask'
      | 'ipv6_address'
     }
```

## CREATE BROKER PRIORITY (Transact-SQL)

`docs/t-sql/statements/create-broker-priority-transact-sql.md`

### Syntax

```syntaxsql
CREATE BROKER PRIORITY ConversationPriorityName
FOR CONVERSATION
[ SET ( [ CONTRACT_NAME = {ContractName | ANY } ]
        [ [ , ] LOCAL_SERVICE_NAME = {LocalServiceName | ANY } ]
        [ [ , ] REMOTE_SERVICE_NAME = {'RemoteServiceName' | ANY } ]
        [ [ , ] PRIORITY_LEVEL = {PriorityValue | DEFAULT } ]
       )
]
[;]
```

## CREATE CERTIFICATE (Transact-SQL)

`docs/t-sql/statements/create-certificate-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server and Azure SQL Database

CREATE CERTIFICATE certificate_name [ AUTHORIZATION user_name ]
    { FROM <existing_keys> | <generate_new_keys> }
    [ ACTIVE FOR BEGIN_DIALOG = { ON | OFF } ]

<existing_keys> ::=
    ASSEMBLY assembly_name
    | {
        [ EXECUTABLE ] FILE = 'path_to_file'
        [ WITH [FORMAT = 'PFX',]
          PRIVATE KEY ( <private_key_options> ) ]
      }
    | {
        BINARY = asn_encoded_certificate
        [ WITH PRIVATE KEY ( <private_key_options> ) ]
      }
<generate_new_keys> ::=
    [ ENCRYPTION BY PASSWORD = 'password' ]
    WITH SUBJECT = 'certificate_subject_name'
    [ , <date_options> [ ,...n ] ]

<private_key_options> ::=
      {
        FILE = 'path_to_private_key'
         [ , DECRYPTION BY PASSWORD = 'password' ]
         [ , ENCRYPTION BY PASSWORD = 'password' ]
      }
    |
      {
        BINARY = private_key_bits
         [ , DECRYPTION BY PASSWORD = 'password' ]
         [ , ENCRYPTION BY PASSWORD = 'password' ]
      }

<date_options> ::=
    START_DATE = 'datetime' | EXPIRY_DATE = 'datetime'
```

```syntaxsql
-- Syntax for Parallel Data Warehouse

CREATE CERTIFICATE certificate_name
    { <generate_new_keys> | FROM <existing_keys> }
    [ ; ]

<generate_new_keys> ::=
    WITH SUBJECT = 'certificate_subject_name'
    [ , <date_options> [ ,...n ] ]

<existing_keys> ::=
    {
      FILE ='path_to_file'
      WITH PRIVATE KEY
         (
           FILE = 'path_to_private_key'
           , DECRYPTION BY PASSWORD ='password'
         )
    }

<date_options> ::=
    START_DATE ='datetime' | EXPIRY_DATE ='datetime'
```

## CREATE COLUMN ENCRYPTION KEY (Transact-SQL)

`docs/t-sql/statements/create-column-encryption-key-transact-sql.md`

### Syntax

```syntaxsql
CREATE COLUMN ENCRYPTION KEY key_name
WITH VALUES
  (
    COLUMN_MASTER_KEY = column_master_key_name,
    ALGORITHM = 'algorithm_name',
    ENCRYPTED_VALUE = varbinary_literal
  )
[, (
    COLUMN_MASTER_KEY = column_master_key_name,
    ALGORITHM = 'algorithm_name',
    ENCRYPTED_VALUE = varbinary_literal
  ) ]
[;]
```

## CREATE COLUMN MASTER KEY (Transact-SQL)

`docs/t-sql/statements/create-column-master-key-transact-sql.md`

### Syntax

```syntaxsql
CREATE COLUMN MASTER KEY key_name
    WITH (
        KEY_STORE_PROVIDER_NAME = 'key_store_provider_name',
        KEY_PATH = 'key_path'
        [,ENCLAVE_COMPUTATIONS (SIGNATURE = signature)]
         )
[;]
```

## CREATE COLUMNSTORE INDEX (Transact-SQL)

`docs/t-sql/statements/create-columnstore-index-transact-sql.md`

### Syntax

> Syntax for Azure SQL Database and Azure SQL Managed Instance<sup>[AUTD](/azure/azure-sql/managed-instance/update-policy#always-up-to-date-update-policy)</sup>:

```syntaxsql
-- Create a clustered columnstore index on disk-based table.
CREATE CLUSTERED COLUMNSTORE INDEX index_name
    ON { database_name.schema_name.table_name | schema_name.table_name | table_name }
    [ ORDER (column [ , ...n ] ) ]
    [ WITH ( <with_option> [ , ...n ] ) ]
    [ ON <on_option> ]
[ ; ]

-- Create a nonclustered columnstore index on a disk-based table.
CREATE [ NONCLUSTERED ]  COLUMNSTORE INDEX index_name
    ON { database_name.schema_name.table_name | schema_name.table_name | table_name }
        ( column  [ , ...n ] )
    [ ORDER (column [ , ...n ] ) ]
    [ WHERE <filter_expression> [ AND <filter_expression> ] ]
    [ WITH ( <with_option> [ , ...n ] ) ]
    [ ON <on_option> ]
[ ; ]

<with_option> ::=
      DROP_EXISTING = { ON | OFF } -- default is OFF
    | MAXDOP = max_degree_of_parallelism
    | ONLINE = { ON | OFF }
    | COMPRESSION_DELAY  = { 0 | delay [ MINUTES ] }
    | DATA_COMPRESSION = { COLUMNSTORE | COLUMNSTORE_ARCHIVE }
      [ ON PARTITIONS ( { partition_number_expression | range } [ , ...n ] ) ]

<on_option>::=
      partition_scheme_name ( column_name )
    | filegroup_name
    | "default"

<filter_expression> ::=
      column_name IN ( constant [ , ...n ]
    | column_name { IS | IS NOT | = | <> | != | > | >= | !> | < | <= | !< } constant )
```

> Syntax for SQL Server:

```syntaxsql
-- Create a clustered columnstore index on disk-based table.
CREATE CLUSTERED COLUMNSTORE INDEX index_name
    ON { database_name.schema_name.table_name | schema_name.table_name | table_name }
    [ WITH ( <with_option> [ , ...n ] ) ]
    [ ORDER (column [ , ...n ] ) ]
    [ ON <on_option> ]
[ ; ]

-- Create a nonclustered columnstore index on a disk-based table.
CREATE [ NONCLUSTERED ]  COLUMNSTORE INDEX index_name
    ON { database_name.schema_name.table_name | schema_name.table_name | table_name }
        ( column  [ , ...n ] )
    [ ORDER (column [ , ...n ] ) ]
    [ WHERE <filter_expression> [ AND <filter_expression> ] ]
    [ WITH ( <with_option> [ , ...n ] ) ]
    [ ON <on_option> ]
[ ; ]

<with_option> ::=
      DROP_EXISTING = { ON | OFF } -- default is OFF
    | MAXDOP = max_degree_of_parallelism
    | ONLINE = { ON | OFF }
    | COMPRESSION_DELAY  = { 0 | delay [ MINUTES ] }
    | DATA_COMPRESSION = { COLUMNSTORE | COLUMNSTORE_ARCHIVE }
      [ ON PARTITIONS ( { partition_number_expression | range } [ , ...n ] ) ]

<on_option>::=
      partition_scheme_name ( column_name )
    | filegroup_name
    | "default"

<filter_expression> ::=
      column_name IN ( constant [ , ...n ]
    | column_name { IS | IS NOT | = | <> | != | > | >= | !> | < | <= | !< } constant )
```

> Syntax for Azure Synapse Analytics and Analytics Platform System (PDW):

```syntaxsql
CREATE CLUSTERED COLUMNSTORE INDEX index_name
    ON { database_name.schema_name.table_name | schema_name.table_name | table_name }
    [ ORDER ( column [ , ...n ] ) ]
    [ WITH ( DROP_EXISTING = { ON | OFF } ) ] -- default is OFF
[;]
```

## CREATE CONTRACT (Transact-SQL)

`docs/t-sql/statements/create-contract-transact-sql.md`

### Syntax

```syntaxsql
CREATE CONTRACT contract_name
   [ AUTHORIZATION owner_name ]
      (  {   { message_type_name | [ DEFAULT ] }
          SENT BY { INITIATOR | TARGET | ANY }
       } [ ,...n] )
[ ; ]
```

## CREATE CREDENTIAL (Transact-SQL)

`docs/t-sql/statements/create-credential-transact-sql.md`

### Syntax

```syntaxsql
CREATE CREDENTIAL credential_name
WITH IDENTITY = 'identity_name'
    [ , SECRET = 'secret' ]
        [ FOR CRYPTOGRAPHIC PROVIDER cryptographic_provider_name ]
```

## CREATE CRYPTOGRAPHIC PROVIDER (Transact-SQL)

`docs/t-sql/statements/create-cryptographic-provider-transact-sql.md`

### Syntax

```syntaxsql
CREATE CRYPTOGRAPHIC PROVIDER provider_name
    FROM FILE = path_of_DLL
```

## CREATE DATABASE AUDIT SPECIFICATION

`docs/t-sql/statements/create-database-audit-specification-transact-sql.md`

### Syntax

```syntaxsql
CREATE DATABASE AUDIT SPECIFICATION audit_specification_name
{
    FOR SERVER AUDIT audit_name
        [ ADD (
            { <audit_action_specification> | audit_action_group_name }
            [ , ...n ] )
        ]
        [ WITH ( STATE = { ON | OFF } ) ]
}
[ ; ]
<audit_action_specification>::=
{
    action [ , ...n ] ON [class::]securable BY principal [ , ...n ]
}
```

## CREATE DATABASE ENCRYPTION KEY (Transact-SQL)

`docs/t-sql/statements/create-database-encryption-key-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server

CREATE DATABASE ENCRYPTION KEY
       WITH ALGORITHM = { AES_128 | AES_192 | AES_256 | TRIPLE_DES_3KEY }
   ENCRYPTION BY SERVER
    {
        CERTIFICATE Encryptor_Name |
        ASYMMETRIC KEY Encryptor_Name
    }
[ ; ]
```

```syntaxsql
-- Syntax for Parallel Data Warehouse

CREATE DATABASE ENCRYPTION KEY
       WITH ALGORITHM = { AES_128 | AES_192 | AES_256 | TRIPLE_DES_3KEY }
   ENCRYPTION BY SERVER CERTIFICATE Encryptor_Name
[ ; ]
```

## CREATE DATABASE SCOPED CREDENTIAL (Transact-SQL)

`docs/t-sql/statements/create-database-scoped-credential-transact-sql.md`

### Syntax

```syntaxsql
CREATE DATABASE SCOPED CREDENTIAL credential_name
WITH IDENTITY = 'identity_name'
    [ , SECRET = 'secret' ]
[ ; ]
```

## CREATE DATABASE (Transact-SQL)

`docs/t-sql/statements/create-database-transact-sql.md`

### Syntax

> For more information about the syntax conventions, see [Transact-SQL syntax conventions](../../t-sql/language-elements/transact-sql-syntax-conventions-transact-sql.md).

```syntaxsql
CREATE DATABASE database_name
[ CONTAINMENT = { NONE | PARTIAL } ]
[ ON
      [ PRIMARY ] <filespec> [ , ...n ]
      [ , <filegroup> [ , ...n ] ]
      [ LOG ON <filespec> [ , ...n ] ]
]
[ COLLATE collation_name ]
[ WITH <option> [ , ...n ] ]
[ ; ]

<option> ::=
{
      FILESTREAM ( <filestream_option> [ , ...n ] )
    | DEFAULT_FULLTEXT_LANGUAGE = { lcid | language_name | language_alias }
    | DEFAULT_LANGUAGE = { lcid | language_name | language_alias }
    | NESTED_TRIGGERS = { OFF | ON }
    | TRANSFORM_NOISE_WORDS = { OFF | ON }
    | TWO_DIGIT_YEAR_CUTOFF = <two_digit_year_cutoff>
    | DB_CHAINING { OFF | ON }
    | TRUSTWORTHY { OFF | ON }
    | PERSISTENT_LOG_BUFFER = ON ( DIRECTORY_NAME = 'path-to-directory-on-a-DAX-volume' )
    | LEDGER = { ON | OFF }
}

<filestream_option> ::=
{
      NON_TRANSACTED_ACCESS = { OFF | READ_ONLY | FULL }
    | DIRECTORY_NAME = 'directory_name'
}

<filespec> ::=
{
(
    NAME = logical_file_name ,
    FILENAME = { 'os_file_name' | 'filestream_path' }
    [ , SIZE = size [ KB | MB | GB | TB ] ]
    [ , MAXSIZE = { max_size [ KB | MB | GB | TB ] | UNLIMITED } ]
    [ , FILEGROWTH = growth_increment [ KB | MB | GB | TB | % ] ]
)
}

<filegroup> ::=
{
FILEGROUP filegroup name [ [ CONTAINS FILESTREAM ] [ DEFAULT ] | CONTAINS MEMORY_OPTIMIZED_DATA ]
    <filespec> [ , ...n ]
}
```

> Attach a database:

```syntaxsql
CREATE DATABASE database_name
    ON <filespec> [ , ...n ]
    FOR { { ATTACH [ WITH <attach_database_option> [ , ...n ] ] }
        | ATTACH_REBUILD_LOG }
[ ; ]

<attach_database_option> ::=
{
      <service_broker_option>
    | RESTRICTED_USER
    | FILESTREAM ( DIRECTORY_NAME = { 'directory_name' | NULL } )
}

<service_broker_option> ::=
{
    ENABLE_BROKER
  | NEW_BROKER
  | ERROR_BROKER_CONVERSATIONS
}
```

> Create a database snapshot:

```syntaxsql
CREATE DATABASE database_snapshot_name
    ON
    (
        NAME = logical_file_name ,
        FILENAME = 'os_file_name'
    ) [ , ...n ]
    AS SNAPSHOT OF
[ ; ]
```

### Create a database

> For more information about the syntax conventions, see [Transact-SQL syntax conventions](../../t-sql/language-elements/transact-sql-syntax-conventions-transact-sql.md).

```syntaxsql
CREATE DATABASE database_name [ COLLATE collation_name ]
{
  (<edition_options> [ , ...n ] )
}
[ WITH <with_options> [ , ..n ] ]
[ ; ]

<with_options> ::=
{
    CATALOG_COLLATION = { DATABASE_DEFAULT | SQL_Latin1_General_CP1_CI_AS }
  | BACKUP_STORAGE_REDUNDANCY = { 'LOCAL' | 'ZONE' | 'GEO' | 'GEOZONE' }
  | LEDGER = { ON | OFF }
}

<edition_options> ::=
{

  MAXSIZE = { 100 MB | 500 MB | 1 ... 1024 ... 4096 GB }
  | ( EDITION = { 'Basic' | 'Standard' | 'Premium' | 'GeneralPurpose' | 'BusinessCritical' | 'Hyperscale' }
  | SERVICE_OBJECTIVE =
    { 'Basic' | 'S0' | 'S1' | 'S2' | 'S3' | 'S4' | 'S6' | 'S7' | 'S9' | 'S12'
      | 'P1' | 'P2' | 'P4' | 'P6' | 'P11' | 'P15'
      | 'BC_DC_n'
      | 'BC_Gen5_n'
      | 'BC_M_n'
      | 'GP_DC_n'
      | 'GP_Gen5_n'
      | 'GP_S_Gen5_n'
      | 'HS_DC_n'
      | 'HS_Gen5_n'
      | 'HS_S_Gen5_n'
      | 'HS_MOPRMS_n'
      | 'HS_PRMS_n'
      | { ELASTIC_POOL(name = <elastic_pool_name>) } } )
}
```

### Copy a database

```syntaxsql
CREATE DATABASE database_name
    AS COPY OF [ source_server_name. ] source_database_name
    [ ( SERVICE_OBJECTIVE =
      { 'Basic' | 'S0' | 'S1' | 'S2' | 'S3' | 'S4' | 'S6' | 'S7' | 'S9' | 'S12'
      | 'P1' | 'P2' | 'P4' | 'P6' | 'P11' | 'P15'
      | 'GP_Gen5_n'
      | 'GP_S_Gen5_n'
      | 'BC_Gen5_n'
      | 'BC_M_n'
      | 'HS_Gen5_n'
      | 'HS_S_Gen5_n'
      | 'HS_PRMS_n'
      | 'HS_MOPRMS_n'
      | { ELASTIC_POOL(name = <elastic_pool_name>) } } )
   ]
   [ WITH ( BACKUP_STORAGE_REDUNDANCY = { 'LOCAL' | 'ZONE' | 'GEO' | 'GEOZONE' } ) ]
[ ; ]
```

### Syntax

> For more information about the syntax conventions, see [Transact-SQL syntax conventions](../../t-sql/language-elements/transact-sql-syntax-conventions-transact-sql.md).

```syntaxsql
CREATE DATABASE database_name [ COLLATE collation_name ]
[ WITH <with_options> [ , ..n ] ]
[ ; ]

<with_options> ::=
{
  LEDGER = { ON | OFF }
}
```

### [Dedicated SQL pool](#tab/sqlpool)

```syntaxsql
CREATE DATABASE database_name [ COLLATE collation_name ]
(
    [ MAXSIZE = {
          250 | 500 | 750 | 1024 | 5120 | 10240 | 20480 | 30720
        | 40960 | 51200 | 61440 | 71680 | 81920 | 92160 | 102400
        | 153600 | 204800 | 245760
      } GB ,
    ]
    EDITION = 'datawarehouse',
    SERVICE_OBJECTIVE = {
          'DW100c' | 'DW200c' | 'DW300c' | 'DW400c' | 'DW500c'
        | 'DW1000c' | 'DW1500c' | 'DW2000c' | 'DW2500c' | 'DW3000c' | 'DW5000c'
        | 'DW6000c' | 'DW7500c' | 'DW10000c' | 'DW15000c' | 'DW30000c'
    }
)
[ ; ]
```

### [Serverless SQL pool](#tab/sqlod)

```syntaxsql
CREATE DATABASE database_name [ COLLATE collation_name ]
[ ; ]
```

### Syntax

> For more information about the syntax conventions, see [Transact-SQL syntax conventions](../../t-sql/language-elements/transact-sql-syntax-conventions-transact-sql.md).

```syntaxsql
CREATE DATABASE database_name
WITH (
    [ AUTOGROW = ON | OFF , ]
    REPLICATED_SIZE = replicated_size [ GB ] ,
    DISTRIBUTED_SIZE = distributed_size [ GB ] ,
    LOG_SIZE = log_size [ GB ] )
[ ; ]
```

## CREATE DEFAULT (Transact-SQL)

`docs/t-sql/statements/create-default-transact-sql.md`

### Syntax

```syntaxsql
CREATE DEFAULT [ schema_name . ] default_name
AS constant_expression [ ; ]
```

## CREATE ENDPOINT (Transact-SQL)

`docs/t-sql/statements/create-endpoint-transact-sql.md`

### Syntax

```syntaxsql
CREATE ENDPOINT endPointName [ AUTHORIZATION login ]
[ STATE = { STARTED | STOPPED | DISABLED } ]
AS { TCP } (
    <protocol_specific_arguments>
)
FOR { TSQL | SERVICE_BROKER | DATABASE_MIRRORING } (
    <language_specific_arguments>
)

<AS TCP_protocol_specific_arguments> ::=
AS TCP (
    LISTENER_PORT = listenerPort
    [ [ , ] LISTENER_IP = ALL | ( four_part_ipv4_address ) | ( 'ip_address_v6' ) ]
)

<FOR TSQL_language_specific_arguments> ::=
FOR TSQL (
    [ ENCRYPTION = { NEGOTIATED | STRICT } ]
)

<FOR SERVICE_BROKER_language_specific_arguments> ::=
FOR SERVICE_BROKER (
    [ AUTHENTICATION = {
          WINDOWS [ { NTLM | KERBEROS | NEGOTIATE } ]
          | CERTIFICATE certificate_name
          | WINDOWS [ { NTLM | KERBEROS | NEGOTIATE } ] CERTIFICATE certificate_name
          | CERTIFICATE certificate_name WINDOWS [ { NTLM | KERBEROS | NEGOTIATE } ]
    } ]
    [ [ , ] ENCRYPTION = {
          DISABLED
          | { SUPPORTED | REQUIRED }
            [ ALGORITHM { AES | RC4 | AES RC4 | RC4 AES } ]
    } ]
    [ [ , ] MESSAGE_FORWARDING = { ENABLED | DISABLED } ]
    [ [ , ] MESSAGE_FORWARD_SIZE = forward_size ]
)

<FOR DATABASE_MIRRORING_language_specific_arguments> ::=
FOR DATABASE_MIRRORING (
    [ AUTHENTICATION = {
          WINDOWS [ { NTLM | KERBEROS | NEGOTIATE } ]
          | CERTIFICATE certificate_name
          | WINDOWS [ { NTLM | KERBEROS | NEGOTIATE } ] CERTIFICATE certificate_name
          | CERTIFICATE certificate_name WINDOWS [ { NTLM | KERBEROS | NEGOTIATE } ]
    } ]
    [ [ , ] ENCRYPTION = {
          DISABLED
          | { SUPPORTED | REQUIRED }
            [ ALGORITHM { AES | RC4 | AES RC4 | RC4 AES } ]
    } ]
    [ , ] ROLE = { WITNESS | PARTNER | ALL }
)
```

## CREATE EVENT NOTIFICATION (Transact-SQL)

`docs/t-sql/statements/create-event-notification-transact-sql.md`

### Syntax

```syntaxsql
CREATE EVENT NOTIFICATION event_notification_name
ON { SERVER | DATABASE | QUEUE queue_name }
[ WITH FAN_IN ]
FOR { event_type | event_group } [ , ...n ]
TO SERVICE 'broker_service' , { 'broker_instance_specifier' | 'current database' }
[ ; ]
```

## CREATE EVENT SESSION (Transact-SQL)

`docs/t-sql/statements/create-event-session-transact-sql.md`

### Syntax

```syntaxsql
CREATE EVENT SESSION event_session_name
ON { SERVER | DATABASE }
{
    <event_definition> [ , ...n ]
    [ <event_target_definition> [ , ...n ] ]
    [ WITH ( <event_session_options> [ , ...n ] ) ]
}
;

<event_definition>::=
{
    ADD EVENT [event_module_guid].event_package_name.event_name
         [ ( {
                 [ SET { event_customizable_attribute = <value> [ , ...n ] } ]
                 [ ACTION ( { [event_module_guid].event_package_name.action_name [ , ...n ] } ) ]
                 [ WHERE <predicate_expression> ]
        } ) ]
}

<predicate_expression> ::=
{
    [ NOT ] <predicate_factor> | { ( <predicate_expression> ) }
    [ { AND | OR } [ NOT ] { <predicate_factor> | ( <predicate_expression> ) } ]
    [ , ...n ]
}

<predicate_factor>::=
{
    <predicate_leaf> | ( <predicate_expression> )
}

<predicate_leaf>::=
{
      <predicate_source_declaration> { = | < > | != | > | >= | < | <= } <value>
    | [event_module_guid].event_package_name.predicate_compare_name ( <predicate_source_declaration> , <value> )
}

<predicate_source_declaration>::=
{
    event_field_name | ( [event_module_guid].event_package_name.predicate_source_name )
}

<value>::=
{
    number | 'string'
}

<event_target_definition>::=
{
    ADD TARGET [event_module_guid].event_package_name.target_name
        [ ( SET { target_parameter_name = <value> [ , ...n ] } ) ]
}

<event_session_options>::=
{
    [       MAX_MEMORY = size [ KB | MB ] ]
    [ [ , ] EVENT_RETENTION_MODE = { ALLOW_SINGLE_EVENT_LOSS | ALLOW_MULTIPLE_EVENT_LOSS | NO_EVENT_LOSS } ]
    [ [ , ] MAX_DISPATCH_LATENCY = { seconds SECONDS | INFINITE } ]
    [ [ , ] MAX_EVENT_SIZE = size [ KB | MB ] ]
    [ [ , ] MEMORY_PARTITION_MODE = { NONE | PER_NODE | PER_CPU } ]
    [ [ , ] TRACK_CAUSALITY = { ON | OFF } ]
    [ [ , ] STARTUP_STATE = { ON | OFF } ]
    [ [ , ] MAX_DURATION = { <time duration> { SECONDS | MINUTES | HOURS | DAYS } | UNLIMITED } ]
}
```

## CREATE EXTERNAL DATA SOURCE (Transact-SQL)

`docs/t-sql/statements/create-external-data-source-transact-sql.md`

### Syntax for SQL Server 2017

```syntaxsql
CREATE EXTERNAL DATA SOURCE <data_source_name>
WITH
  ( [ LOCATION = '<prefix>://<path>[:<port>]' ]
    [ [ , ] CREDENTIAL = <credential_name> ]
    [ [ , ] TYPE = { HADOOP | BLOB_STORAGE } ]
    [ [ , ] RESOURCE_MANAGER_LOCATION = '<resource_manager>[:<port>]' )
[ ; ]
```

### Syntax for SQL Server 2019

```syntaxsql
CREATE EXTERNAL DATA SOURCE <data_source_name>
WITH
  ( [ LOCATION = '<prefix>://<path>[:<port>]' ]
    [ [ , ] CONNECTION_OPTIONS = '<key_value_pairs>'[,...]]
    [ [ , ] CREDENTIAL = <credential_name> ]
    [ [ , ] PUSHDOWN = { ON | OFF } ]
    [ [ , ] TYPE = { HADOOP | BLOB_STORAGE } ]
    [ [ , ] RESOURCE_MANAGER_LOCATION = '<resource_manager>[:<port>]' ]
  )
[ ; ]
```

### Syntax for SQL Server 2022 and later versions

Marked for `=sql-server-ver16 || =sql-server-linux-ver16`.

```syntaxsql
CREATE EXTERNAL DATA SOURCE <data_source_name>
WITH
  ( [ LOCATION = '<prefix>://<path>[:<port>]' ]
    [ [ , ] CONNECTION_OPTIONS = '<key_value_pairs>'[,...]]
    [ [ , ] CREDENTIAL = <credential_name> ]
    [ [ , ] PUSHDOWN = { ON | OFF } ]
  )
[ ; ]
```

### Syntax for SQL Server 2025 and later versions

Marked for `>=sql-server-ver17 || >=sql-server-linux-ver17`.

> For more information about the syntax conventions, see [Transact-SQL syntax conventions](../../t-sql/language-elements/transact-sql-syntax-conventions-transact-sql.md).

```syntaxsql
CREATE EXTERNAL DATA SOURCE <data_source_name>
WITH
  ( [ LOCATION = '<prefix>://<path>[:<port>]' ]
    [ [ , ] CONNECTION_OPTIONS = '<key_value_pairs>'[,...]]
    [ [ , ] CREDENTIAL = <credential_name> ]
    [ [ , ] PUSHDOWN = { ON | OFF } ]
  )
[ ; ]
```

### Syntax

```syntaxsql
CREATE EXTERNAL DATA SOURCE <data_source_name>
WITH
  ( [ LOCATION = '<prefix>://<path>[:<port>]' ]
    [ [ , ] CREDENTIAL = <credential_name> ]
    [ [ , ] TYPE = { BLOB_STORAGE | RDBMS | SHARD_MAP_MANAGER } ]
    [ [ , ] DATABASE_NAME = '<database_name>' ]
    [ [ , ] SHARD_MAP_NAME = '<shard_map_manager>' ] )
[ ; ]
```

### [Dedicated SQL pool](#tab/dedicated)

```syntaxsql
CREATE EXTERNAL DATA SOURCE <data_source_name>
WITH
( [ LOCATION = '<prefix>://<path>[:<port>]' ]
  [ [ , ] CREDENTIAL = <credential_name> ]
  [ [ , ] TYPE = HADOOP ]
)
[ ; ]
```

### [Serverless SQL pool](#tab/serverless)

```syntaxsql
CREATE EXTERNAL DATA SOURCE <data_source_name>
WITH
(   LOCATION = '<prefix>://<path>'
)
[;]
```

### Syntax

```syntaxsql
CREATE EXTERNAL DATA SOURCE <data_source_name>
WITH
  ( [ LOCATION = '<prefix>://<path>[:<port>]' ]
    [ [ , ] CREDENTIAL = <credential_name> ]
    [ [ , ] TYPE = HADOOP ]
    [ [ , ] RESOURCE_MANAGER_LOCATION = '<resource_manager>[:<port>]' )
[ ; ]
```

```syntaxsql
CREATE EXTERNAL DATA SOURCE <data_source_name>
WITH
  ( [ LOCATION = '<prefix>://<path>[:<port>]' ]
    [ [ , ] CREDENTIAL = <credential_name> ]
  )
[ ; ]
```

```syntaxsql
CREATE EXTERNAL DATA SOURCE <data_source_name>
WITH
  ( LOCATION = '<prefix>://<path>[:<port>]' )
[ ; ]
```

```syntaxsql
CREATE EXTERNAL DATA SOURCE <data_source_name>
WITH
  ( [ LOCATION = '<prefix>://<path>[:<port>]' ]
[ ; ]
```

## CREATE EXTERNAL FILE FORMAT (Transact-SQL)

`docs/t-sql/statements/create-external-file-format-transact-sql.md`

### [Delimited text](#tab/delimited)

```syntaxsql
-- Create an external file format for DELIMITED (CSV/TSV) files.
CREATE EXTERNAL FILE FORMAT file_format_name
WITH (
        FORMAT_TYPE = DELIMITEDTEXT
    [ , FORMAT_OPTIONS ( <format_options> [ ,...n  ] ) ]
    [ , DATA_COMPRESSION = {
           'org.apache.hadoop.io.compress.GzipCodec'
        }
     ]);

<format_options> ::=
{
    FIELD_TERMINATOR = field_terminator
    | STRING_DELIMITER = string_delimiter
    | FIRST_ROW = integer -- Applies to: Azure Synapse Analytics and SQL Server 2022 and later versions
    | DATE_FORMAT = datetime_format
    | USE_TYPE_DEFAULT = { TRUE | FALSE }
    | ENCODING = {'UTF8' | 'UTF16'}
    | PARSER_VERSION = {'parser_version'}

}
```

### [RC](#tab/rc)

```syntaxsql
--Create an external file format for RC files.
CREATE EXTERNAL FILE FORMAT file_format_name
WITH (
    FORMAT_TYPE = RCFILE,
    SERDE_METHOD = {
        'org.apache.hadoop.hive.serde2.columnar.LazyBinaryColumnarSerDe'
      | 'org.apache.hadoop.hive.serde2.columnar.ColumnarSerDe'
    }
    [ , DATA_COMPRESSION = 'org.apache.hadoop.io.compress.DefaultCodec' ]);
```

### [ORC](#tab/orc)

```syntaxsql
--Create an external file format for ORC file.
CREATE EXTERNAL FILE FORMAT file_format_name
WITH (
         FORMAT_TYPE = ORC
     [ , DATA_COMPRESSION = {
        'org.apache.hadoop.io.compress.SnappyCodec'
      | 'org.apache.hadoop.io.compress.DefaultCodec' }
    ]);
```

### [Parquet](#tab/parquet)

```syntaxsql
--Create an external file format for PARQUET files.
CREATE EXTERNAL FILE FORMAT file_format_name
WITH (
         FORMAT_TYPE = PARQUET
     [ , DATA_COMPRESSION = {
        'org.apache.hadoop.io.compress.SnappyCodec'
      | 'org.apache.hadoop.io.compress.GzipCodec' }
    ]);
```

### [JSON](#tab/json)

```syntaxsql
-- Create an external file format for JSON files.
CREATE EXTERNAL FILE FORMAT file_format_name
WITH (
    FORMAT_TYPE = JSON
     [ , DATA_COMPRESSION = {
        'org.apache.hadoop.io.compress.SnappyCodec'
      | 'org.apache.hadoop.io.compress.GzipCodec'
      | 'org.apache.hadoop.io.compress.DefaultCodec' }
    ]);
```

### [Delta table](#tab/delta)

```syntaxsql
-- Create an external file format for delta table files
CREATE EXTERNAL FILE FORMAT file_format_name
WITH (
         FORMAT_TYPE = DELTA
      );
```

## CREATE EXTERNAL LANGUAGE (Transact-SQL) - SQL Server

`docs/t-sql/statements/create-external-language-transact-sql.md`

### Syntax

```syntaxsql
CREATE EXTERNAL LANGUAGE language_name
[ AUTHORIZATION owner_name ]
FROM <file_spec> [ ,...2 ]
[ ; ]

<file_spec> ::=
{
    ( CONTENT = { <external_lang_specifier> | <content_bits> },
    FILE_NAME = <external_lang_file_name>
    [ , PLATFORM = <platform> ]
    [ , PARAMETERS = <external_lang_parameters> ]
    [ , ENVIRONMENT_VARIABLES = <external_lang_env_variables> ] )
}

<external_lang_specifier> :: =
{
    '[file_path\]os_file_name'
}

<content_bits> :: =
{
    varbinary_literal
    | varbinary_expression
}

<external_lang_file_name> :: =
'extension_file_name'


<platform> :: =
{
    WINDOWS
  | LINUX
}

<external_lang_parameters> :: =
'extension_specific_parameters'
```

## CREATE EXTERNAL LIBRARY (Transact-SQL) - SQL Server

`docs/t-sql/statements/create-external-library-transact-sql.md`

### Syntax for SQL Server 2019

Marked for `>=sql-server-ver15 || >=sql-server-linux-ver15`.

```syntaxsql
CREATE EXTERNAL LIBRARY library_name
[ AUTHORIZATION owner_name ]
FROM <file_spec> [ ,...2 ]
WITH ( LANGUAGE = <language> )
[ ; ]

<file_spec> ::=
{
    (CONTENT = { <client_library_specifier> | <library_bits> }
    [, PLATFORM = <platform> ])
}

<client_library_specifier> :: =
{
    '[file_path\]manifest_file_name'
}

<library_bits> :: =
{
      varbinary_literal
    | varbinary_expression
}

<platform> :: =
{
      WINDOWS
    | LINUX
}

<language> :: =
{
      'R'
    | 'Python'
    | <external_language>
}
```

### Syntax for SQL Server 2017

Marked for `=sql-server-2017`.

```syntaxsql
CREATE EXTERNAL LIBRARY library_name
[ AUTHORIZATION owner_name ]
FROM <file_spec> [ ,...2 ]
WITH ( LANGUAGE = 'R' )
[ ; ]

<file_spec> ::=
{
    (CONTENT = { <client_library_specifier> | <library_bits> })
}

<client_library_specifier> :: =
{
    '[file_path\]manifest_file_name'
}

<library_bits> :: =
{
      varbinary_literal
    | varbinary_expression
}
```

### Syntax for Azure SQL Managed Instance

Marked for `=azuresqldb-mi-current`.

```syntaxsql
CREATE EXTERNAL LIBRARY library_name
[ AUTHORIZATION owner_name ]
FROM <file_spec> [ ,...2 ]
WITH ( LANGUAGE = <language> )
[ ; ]

<file_spec> ::=
{
    (CONTENT = <library_bits>)
}

<library_bits> :: =
{
      varbinary_literal
    | varbinary_expression
}

<language> :: =
{
      'R'
    | 'Python'
}
```

## CREATE EXTERNAL MODEL (Transact-SQL)

`docs/t-sql/statements/create-external-model-transact-sql.md`

### Syntax

```syntaxsql
CREATE EXTERNAL MODEL external_model_object_name
[ AUTHORIZATION owner_name ]
WITH
  ( LOCATION = '<prefix>://<path>[:<port>]'
    , API_FORMAT = '<OpenAI, Azure OpenAI, etc>'
    , MODEL_TYPE = EMBEDDINGS
    , MODEL = 'text-embedding-model-name'
    [ , CREDENTIAL = <credential_name> ]
    [ , PARAMETERS = '{"valid":"JSON"}' ]
    [ , LOCAL_RUNTIME_PATH = 'path to the ONNX Runtime files' ]
  );
```

## CREATE EXTERNAL RESOURCE POOL (Transact-SQL)

`docs/t-sql/statements/create-external-resource-pool-transact-sql.md`

### Syntax

Marked for `>=sql-server-ver15 || >=sql-server-linux-ver15`.

```syntaxsql
CREATE EXTERNAL RESOURCE POOL pool_name
[ WITH (
    [ MAX_CPU_PERCENT = value ]
    [ [ , ] MAX_MEMORY_PERCENT = value ]
    [ [ , ] MAX_PROCESSES = value ]
    )
]
[ ; ]

<CPU_range_spec> ::=
{ CPU_ID | CPU_ID  TO CPU_ID } [ ,...n ]
```

Marked for `=sql-server-2017`.

```syntaxsql
CREATE EXTERNAL RESOURCE POOL pool_name
[ WITH (
    [ MAX_CPU_PERCENT = value ]
    [ [ , ] AFFINITY CPU =
            {
                AUTO
              | ( <cpu_range_spec> )
              | NUMANODE = ( <NUMA_node_id> )
            } ]
    [ [ , ] MAX_MEMORY_PERCENT = value ]
    [ [ , ] MAX_PROCESSES = value ]
    )
]
[ ; ]

<CPU_range_spec> ::=
{ CPU_ID | CPU_ID  TO CPU_ID } [ ,...n ]
```

## CREATE EXTERNAL TABLE AS SELECT (CETAS) (Transact-SQL)

`docs/t-sql/statements/create-external-table-as-select-transact-sql.md`

### Syntax

```syntaxsql
CREATE EXTERNAL TABLE { [ [ database_name . [ schema_name ] . ] | schema_name . ] table_name }
    [ (column_name [ , ...n ] ) ]
    WITH (
        LOCATION = 'hdfs_folder' | '<prefix>://<path>[:<port>]' ,
        DATA_SOURCE = external_data_source_name ,
        FILE_FORMAT = external_file_format_name
        [ , <reject_options> [ , ...n ] ]
    )
    AS <select_statement>
[;]

<reject_options> ::=
{
    | REJECT_TYPE = value | percentage
    | REJECT_VALUE = reject_value
    | REJECT_SAMPLE_VALUE = reject_sample_value
}

<select_statement> ::=
    [ WITH <common_table_expression> [ , ...n ] ]
    SELECT <select_criteria>
```

```syntaxsql
CREATE EXTERNAL TABLE [ [database_name  . [ schema_name ] . ] | schema_name . ] table_name
    WITH (
        LOCATION = 'path_to_folder/',
        DATA_SOURCE = external_data_source_name,
        FILE_FORMAT = external_file_format_name
        [, PARTITION ( column_name [ , ...n ] ) ]
)
    AS <select_statement>
[;]

<select_statement> ::=
    [ WITH <common_table_expression> [ ,...n ] ]
    SELECT <select_criteria>
```

## CREATE EXTERNAL TABLE (Transact-SQL)

`docs/t-sql/statements/create-external-table-transact-sql.md`

### Syntax

```syntaxsql
-- Create a new external table
CREATE EXTERNAL TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
    ( <column_definition> [ , ...n ] )
    WITH (
        LOCATION = 'folder_or_filepath' ,
        DATA_SOURCE = external_data_source_name ,
        [ FILE_FORMAT = external_file_format_name ]
        [ , <reject_options> [ , ...n ] ]
    )
[ ; ]

<reject_options> ::=
{
    | REJECT_TYPE = { value | percentage }
    | REJECT_VALUE = reject_value
    | REJECT_SAMPLE_VALUE = reject_sample_value ,
    | REJECTED_ROW_LOCATION = '/REJECT_Directory'
}

<column_definition> ::=
column_name <data_type>
    [ COLLATE collation_name ]
    [ NULL | NOT NULL ]
```

> For use with [Data virtualization (preview)](/azure/azure-sql/database/data-virtualization-overview?view=azuresqldb-current&preserve-view=true)

```syntaxsql
CREATE EXTERNAL TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
    ( <column_definition> [ , ...n ] )
    WITH (
        LOCATION = 'filepath' ,
        DATA_SOURCE = external_data_source_name ,
        FILE_FORMAT = external_file_format_name
    )
[ ; ]

<column_definition> ::=
column_name <data_type>
    [ COLLATE collation_name ]
    [ NULL | NOT NULL ]
```

> For use with [Elastic queries (preview)](/azure/azure-sql/database/elastic-query-overview?view=azuresqldb-current&preserve-view=true):

```syntaxsql
-- Create a table for use with elastic query
CREATE EXTERNAL TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
    ( <column_definition> [ , ...n ] )
    WITH ( <sharded_external_table_options> )
[ ; ]

<column_definition> ::=
column_name <data_type>
    [ COLLATE collation_name ]
    [ NULL | NOT NULL ]

<sharded_external_table_options> ::=
        DATA_SOURCE = external_data_source_name ,
        SCHEMA_NAME = N'nonescaped_schema_name' ,
        OBJECT_NAME = N'nonescaped_object_name' ,
        [ DISTRIBUTION  = SHARDED(sharding_column_name) | REPLICATED | ROUND_ROBIN ] ]
    )
[ ; ]
```

### [Dedicated SQL pool](#tab/dedicated)

```syntaxsql
CREATE EXTERNAL TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
    ( <column_definition> [ ,...n ] )
    WITH (
        LOCATION = 'hdfs_folder_or_filepath',
        DATA_SOURCE = external_data_source_name,
        FILE_FORMAT = external_file_format_name
        [ , <reject_options> [ ,...n ] ]
    )
[ ; ]

<column_definition> ::=
column_name <data_type>
    [ COLLATE collation_name ]
    [ NULL | NOT NULL ]

<reject_options> ::=
{
    | REJECT_TYPE = { value | percentage },
    | REJECT_VALUE = reject_value,
    | REJECT_SAMPLE_VALUE = reject_sample_value,
    | REJECTED_ROW_LOCATION = '/REJECT_Directory'
}
```

### [Serverless SQL pool](#tab/serverless)

```syntaxsql
CREATE EXTERNAL TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
    ( <column_definition> [ ,...n ] )
    WITH (
        LOCATION = 'folder_or_filepath',
        DATA_SOURCE = external_data_source_name,
        FILE_FORMAT = external_file_format_name,
        [ , <reject_options> [ ,...n ] ]
        [, TABLE_OPTIONS = N'{"READ_OPTIONS":["ALLOW_INCONSISTENT_READS"]}' ]
    )
[ ; ]
<column_definition> ::=
column_name <data_type>
    [ COLLATE collation_name ]

<reject_options> ::=
{
    | REJECT_TYPE = value,
    | REJECT_VALUE = reject_value,
    | REJECT_SAMPLE_VALUE = reject_sample_value,
    | REJECTED_ROW_LOCATION = '/REJECT_Directory'
}
```

### Syntax

```syntaxsql
CREATE EXTERNAL TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
    ( <column_definition> [ , ...n ] )
    WITH (
        LOCATION = 'hdfs_folder_or_filepath' ,
        DATA_SOURCE = external_data_source_name ,
        FILE_FORMAT = external_file_format_name
        [ , <reject_options> [ , ...n ] ]
    )
[ ; ]

<column_definition> ::=
column_name <data_type>
    [ COLLATE collation_name ]
    [ NULL | NOT NULL ]

<reject_options> ::=
{
    | REJECT_TYPE = { value | percentage },
    | REJECT_VALUE = reject_value ,
    | REJECT_SAMPLE_VALUE = reject_sample_value ,

}
```

```syntaxsql
CREATE EXTERNAL TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
    ( <column_definition> [ , ...n ] )
    WITH (
        LOCATION = 'filepath' ,
        DATA_SOURCE = external_data_source_name ,
        FILE_FORMAT = external_file_format_name
    )
[ ; ]

<column_definition> ::=
column_name <data_type>
    [ COLLATE collation_name ]
    [ NULL | NOT NULL ]
```

```syntaxsql
CREATE EXTERNAL TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
    ( <column_definition> [ , ...n ] )
    WITH (
        LOCATION = 'filepath' ,
        DATA_SOURCE = external_data_source_name ,
        FILE_FORMAT = external_file_format_name
    )
[ ; ]

<column_definition> ::=
column_name <data_type>
    [ COLLATE collation_name ]
    [ NULL | NOT NULL ]
```

## CREATE FULLTEXT CATALOG (Transact-SQL)

`docs/t-sql/statements/create-fulltext-catalog-transact-sql.md`

### Syntax

```syntaxsql
CREATE FULLTEXT CATALOG catalog_name
     [ON FILEGROUP filegroup ]
     [IN PATH 'rootpath']
     [WITH <catalog_option>]
     [AS DEFAULT]
     [AUTHORIZATION owner_name ]

<catalog_option>::=
     ACCENT_SENSITIVITY = {ON|OFF}
```

## CREATE FULLTEXT INDEX (Transact-SQL)

`docs/t-sql/statements/create-fulltext-index-transact-sql.md`

### Syntax

```syntaxsql
CREATE FULLTEXT INDEX ON table_name
   [ ( { column_name
             [ TYPE COLUMN type_column_name ]
             [ LANGUAGE language_term ]
             [ STATISTICAL_SEMANTICS ]
        } [ , ...n ]
      ) ]
    KEY INDEX index_name
    [ ON <catalog_filegroup_option> ]
    [ WITH ( <with_option> [ , ...n ] ) ]
[;]

<catalog_filegroup_option>::=
 {
    fulltext_catalog_name
 | ( fulltext_catalog_name , FILEGROUP filegroup_name )
 | ( FILEGROUP filegroup_name , fulltext_catalog_name )
 | ( FILEGROUP filegroup_name )
 }

<with_option>::=
 {
   CHANGE_TRACKING [ = ] { MANUAL | AUTO | OFF [ , NO POPULATION ] }
 | STOPLIST [ = ] { OFF | SYSTEM | stoplist_name }
 | SEARCH PROPERTY LIST [ = ] property_list_name
 }
```

## CREATE FULLTEXT STOPLIST (Transact-SQL)

`docs/t-sql/statements/create-fulltext-stoplist-transact-sql.md`

### Syntax

```syntaxsql
CREATE FULLTEXT STOPLIST stoplist_name
[ FROM { [ database_name.]source_stoplist_name } | SYSTEM STOPLIST ]
[ AUTHORIZATION owner_name ]
;
```

## CREATE FUNCTION (Microsoft Fabric, Azure Synapse Analytics)

`docs/t-sql/statements/create-function-sql-data-warehouse.md`

### Scalar function syntax

```syntaxsql
CREATE FUNCTION [ schema_name. ] function_name
( [ { @parameter_name [ AS ] parameter_data_type
    [ = default ] }
    [ ,...n ]
  ]
)
RETURNS return_data_type
    [ WITH <function_option> [ ,...n ] ]
    [ AS ]
    BEGIN
        function_body
        RETURN scalar_expression
    END
[ ; ]

<function_option>::=
{
    [ SCHEMABINDING ]
  | [ RETURNS NULL ON NULL INPUT | CALLED ON NULL INPUT ]
}
```

### Inline table-valued function syntax

```syntaxsql
CREATE FUNCTION [ schema_name. ] function_name
( [ { @parameter_name [ AS ] parameter_data_type
    [ = default ] }
    [ ,...n ]
  ]
)
RETURNS TABLE
    [ WITH SCHEMABINDING ]
    [ AS ]
    RETURN [ ( ] select_stmt [ ) ]
[ ; ]
```

### Scalar function syntax

```syntaxsql
-- Transact-SQL Scalar Function Syntax (in dedicated pools in Azure Synapse Analytics and Parallel Data Warehouse)
-- Not available in the serverless SQL pools in Azure Synapse Analytics

CREATE FUNCTION [ schema_name. ] function_name
( [ { @parameter_name [ AS ] parameter_data_type
    [ = default ] }
    [ ,...n ]
  ]
)
RETURNS return_data_type
    [ WITH <function_option> [ ,...n ] ]
    [ AS ]
    BEGIN
        function_body
        RETURN scalar_expression
    END
[ ; ]

<function_option>::=
{
    [ SCHEMABINDING ]
  | [ RETURNS NULL ON NULL INPUT | CALLED ON NULL INPUT ]
}
```

### Inline table-valued function syntax

```syntaxsql
-- Transact-SQL Inline Table-Valued Function Syntax
-- Preview in dedicated SQL pools in Azure Synapse Analytics
-- Available in the serverless SQL pools in Azure Synapse Analytics
CREATE FUNCTION [ schema_name. ] function_name
( [ { @parameter_name [ AS ] parameter_data_type
    [ = default ] }
    [ ,...n ]
  ]
)
RETURNS TABLE
    [ WITH SCHEMABINDING ]
    [ AS ]
    RETURN [ ( ] select_stmt [ ) ]
[ ; ]
```

## CREATE FUNCTION (Transact-SQL)

`docs/t-sql/statements/create-function-transact-sql.md`

### Syntax

> Syntax for Transact-SQL scalar functions.

```syntaxsql
CREATE [ OR ALTER ] FUNCTION [ schema_name. ] function_name
( [ { @parameter_name [ AS ] [ type_schema_name. ] parameter_data_type [ NULL ]
 [ = default ] [ READONLY ] }
    [ , ...n ]
  ]
)
RETURNS return_data_type
    [ WITH <function_option> [ , ...n ] ]
    [ AS ]
    BEGIN
        function_body
        RETURN scalar_expression
    END
[ ; ]
```

> Syntax for Transact-SQL inline table-valued functions.

```syntaxsql
CREATE [ OR ALTER ] FUNCTION [ schema_name. ] function_name
( [ { @parameter_name [ AS ] [ type_schema_name. ] parameter_data_type [ NULL ]
    [ = default ] [ READONLY ] }
    [ , ...n ]
  ]
)
RETURNS TABLE
    [ WITH <function_option> [ , ...n ] ]
    [ AS ]
    RETURN [ ( ] select_stmt [ ) ]
[ ; ]
```

> Syntax for Transact-SQL multi-statement table-valued functions.

```syntaxsql
CREATE [ OR ALTER ] FUNCTION [ schema_name. ] function_name
( [ { @parameter_name [ AS ] [ type_schema_name. ] parameter_data_type [ NULL ]
    [ = default ] [ READONLY ] }
    [ , ...n ]
  ]
)
RETURNS @return_variable TABLE <table_type_definition>
    [ WITH <function_option> [ , ...n ] ]
    [ AS ]
    BEGIN
        function_body
        RETURN
    END
[ ; ]
```

> Syntax for Transact-SQL function clauses.

```syntaxsql
<function_option> ::=
{
    [ ENCRYPTION ]
  | [ SCHEMABINDING ]
  | [ RETURNS NULL ON NULL INPUT | CALLED ON NULL INPUT ]
  | [ EXECUTE_AS_Clause ]
  | [ INLINE = { ON | OFF } ]
}

<table_type_definition> ::=
( { <column_definition> <column_constraint>
  | <computed_column_definition> }
    [ <table_constraint> ] [ , ...n ]
)
<column_definition> ::=
{
    { column_name data_type }
    [ [ DEFAULT constant_expression ]
      [ COLLATE collation_name ] | [ ROWGUIDCOL ]
    ]
    | [ IDENTITY [ (seed , increment ) ] ]
    [ <column_constraint> [ ...n ] ]
}

<column_constraint> ::=
{
    [ NULL | NOT NULL ]
    { PRIMARY KEY | UNIQUE }
      [ CLUSTERED | NONCLUSTERED ]
      [ WITH FILLFACTOR = fillfactor
        | WITH ( <index_option> [ , ...n ] )
      [ ON { filegroup | "default" } ] ]
  | [ CHECK ( logical_expression ) ] [ , ...n ]
}

<computed_column_definition> ::=
column_name AS computed_column_expression

<table_constraint> ::=
{
    { PRIMARY KEY | UNIQUE }
      [ CLUSTERED | NONCLUSTERED ]
      ( column_name [ ASC | DESC ] [ , ...n ]
        [ WITH FILLFACTOR = fillfactor
        | WITH ( <index_option> [ , ...n ] )
  | [ CHECK ( logical_expression ) ] [ , ...n ]
}

<index_option> ::=
{
    PAD_INDEX = { ON | OFF }
  | FILLFACTOR = fillfactor
  | IGNORE_DUP_KEY = { ON | OFF }
  | STATISTICS_NORECOMPUTE = { ON | OFF }
  | ALLOW_ROW_LOCKS = { ON | OFF }
  | ALLOW_PAGE_LOCKS = { ON | OFF }
}
```

> Syntax for CLR scalar functions.

```syntaxsql
CREATE [ OR ALTER ] FUNCTION [ schema_name. ] function_name
( { @parameter_name [ AS ] [ type_schema_name. ] parameter_data_type [ NULL ]
    [ = default ] }
    [ , ...n ]
)
RETURNS { return_data_type }
    [ WITH <clr_function_option> [ , ...n ] ]
    [ AS ] EXTERNAL NAME <method_specifier>
[ ; ]
```

> Syntax for CLR table-valued functions.

```syntaxsql
CREATE [ OR ALTER ] FUNCTION [ schema_name. ] function_name
( { @parameter_name [ AS ] [ type_schema_name. ] parameter_data_type [ NULL ]
    [ = default ] }
    [ , ...n ]
)
RETURNS TABLE <clr_table_type_definition>
    [ WITH <clr_function_option> [ , ...n ] ]
    [ ORDER ( <order_clause> ) ]
    [ AS ] EXTERNAL NAME <method_specifier>
[ ; ]
```

> Syntax for CLR function clauses.

```syntaxsql
<order_clause> ::=
{
   <column_name_in_clr_table_type_definition>
   [ ASC | DESC ]
} [ , ...n ]

<method_specifier> ::=
    assembly_name.class_name.method_name

<clr_function_option> ::=
{
    [ RETURNS NULL ON NULL INPUT | CALLED ON NULL INPUT ]
  | [ EXECUTE_AS_Clause ]
}

<clr_table_type_definition> ::=
( { column_name data_type } [ , ...n ] )
```

> In-memory OLTP syntax for natively compiled, scalar user-defined functions.

```syntaxsql
CREATE [ OR ALTER ] FUNCTION [ schema_name. ] function_name
 ( [ { @parameter_name [ AS ] [ type_schema_name. ] parameter_data_type
    [ NULL | NOT NULL ] [ = default ] [ READONLY ] }
    [ , ...n ]
  ]
)
RETURNS return_data_type
     WITH <function_option> [ , ...n ]
    [ AS ]
    BEGIN ATOMIC WITH (set_option [ , ... n ] )
        function_body
        RETURN scalar_expression
    END

<function_option> ::=
{
  |  NATIVE_COMPILATION
  |  SCHEMABINDING
  | [ EXECUTE_AS_Clause ]
  | [ RETURNS NULL ON NULL INPUT | CALLED ON NULL INPUT ]
}
```

## CREATE INDEX (Transact-SQL)

`docs/t-sql/statements/create-index-transact-sql.md`

### Syntax for SQL Server, Azure SQL Database, SQL database in Fabric, Azure SQL Managed Instance

```syntaxsql
CREATE [ UNIQUE ] [ CLUSTERED | NONCLUSTERED ] INDEX index_name
    ON <object> ( column [ ASC | DESC ] [ ,...n ] )
    [ INCLUDE ( column_name [ ,...n ] ) ]
    [ WHERE <filter_predicate> ]
    [ WITH ( <relational_index_option> [ ,...n ] ) ]
    [ ON { partition_scheme_name ( column_name )
         | filegroup_name
         | default
         }
    ]
    [ FILESTREAM_ON { filestream_filegroup_name | partition_scheme_name | "NULL" } ]

[ ; ]

<object> ::=
{ database_name.schema_name.table_or_view_name | schema_name.table_or_view_name | table_or_view_name }

<relational_index_option> ::=
{
    PAD_INDEX = { ON | OFF }
  | FILLFACTOR = fillfactor
  | SORT_IN_TEMPDB = { ON | OFF }
  | IGNORE_DUP_KEY = { ON | OFF }
  | STATISTICS_NORECOMPUTE = { ON | OFF }
  | STATISTICS_INCREMENTAL = { ON | OFF }
  | DROP_EXISTING = { ON | OFF }
  | ONLINE = { ON [ ( <low_priority_lock_wait> ) ] | OFF }
  | RESUMABLE = { ON | OFF }
  | MAX_DURATION = <time> [MINUTES]
  | ALLOW_ROW_LOCKS = { ON | OFF }
  | ALLOW_PAGE_LOCKS = { ON | OFF }
  | OPTIMIZE_FOR_SEQUENTIAL_KEY = { ON | OFF }
  | MAXDOP = max_degree_of_parallelism
  | DATA_COMPRESSION = { NONE | ROW | PAGE }
     [ ON PARTITIONS ( { <partition_number_expression> | <range> }
     [ , ...n ] ) ]
  | XML_COMPRESSION = { ON | OFF }
     [ ON PARTITIONS ( { <partition_number_expression> | <range> }
     [ , ...n ] ) ]
}

<filter_predicate> ::=
    <conjunct> [ AND ] [ ...n ]

<conjunct> ::=
    <disjunct> | <comparison>

<disjunct> ::=
        column_name IN (constant ,...n)

<comparison> ::=
        column_name <comparison_op> constant

<comparison_op> ::=
    { IS | IS NOT | = | <> | != | > | >= | !> | < | <= | !< }

<low_priority_lock_wait>::=
{
    WAIT_AT_LOW_PRIORITY ( MAX_DURATION = <time> [ MINUTES ] ,
                          ABORT_AFTER_WAIT = { NONE | SELF | BLOCKERS } )
}

<range> ::=
<partition_number_expression> TO <partition_number_expression>
```

### Backward compatible relational index

> > Use the syntax structure specified in \<relational_index_option\> instead.

```syntaxsql
CREATE [ UNIQUE ] [ CLUSTERED | NONCLUSTERED ] INDEX index_name
    ON <object> ( column_name [ ASC | DESC ] [ ,...n ] )
    [ WITH <backward_compatible_index_option> [ ,...n ] ]
    [ ON { filegroup_name | "default" } ]

<object> ::=
{
    [ database_name. [ owner_name ] . | owner_name. ]
    table_or_view_name
}

<backward_compatible_index_option> ::=
{
    PAD_INDEX
  | FILLFACTOR = fillfactor
  | SORT_IN_TEMPDB
  | IGNORE_DUP_KEY
  | STATISTICS_NORECOMPUTE
  | DROP_EXISTING
}
```

### Syntax for Azure Synapse Analytics and Parallel Data Warehouse

```syntaxsql
CREATE CLUSTERED COLUMNSTORE INDEX index_name
    ON [ database_name . [ schema ] . | schema . ] table_name
    [ORDER (column[,...n])]
    [WITH ( DROP_EXISTING = { ON | OFF } )]
[;]


CREATE [ CLUSTERED | NONCLUSTERED ] INDEX index_name
    ON [ database_name . [ schema ] . | schema . ] table_name
        ( { column [ ASC | DESC ] } [ ,...n ] )
    WITH ( DROP_EXISTING = { ON | OFF } )
[;]
```

## CREATE JSON INDEX (Transact-SQL)

`docs/t-sql/statements/create-json-index-transact-sql.md`

### Syntax

```syntaxsql
CREATE JSON INDEX name ON table_name (json_column_name)
  [ FOR ( sql_json_path [ , ...n ] ) ]
  [ WITH ( <json_index_option> [ , ...n ] ) ]
  [ ON { filegroup_name | "default" } ]
[ ; ]

<object> ::=
    { database_name.schema_name.table_name | schema_name.table_name | table_name }

<sql_json_path> ::=
    { character_string_literal }

<json_index_option> ::=
{
    OPTIMIZE_FOR_ARRAY_SEARCH = { ON | OFF }
  | FILLFACTOR = fillfactor
  | DROP_EXISTING = { ON | OFF }
  | ONLINE = OFF
  | ALLOW_ROW_LOCKS = { ON | OFF }
  | ALLOW_PAGE_LOCKS = { ON | OFF }
  | MAXDOP = max_degree_of_parallelism
  | DATA_COMPRESSION = { NONE | ROW | PAGE }
}
```

## CREATE LOGIN (Transact-SQL)

`docs/t-sql/statements/create-login-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server
CREATE LOGIN login_name { WITH <option_list1> | FROM <sources> }

<option_list1> ::=
    PASSWORD = { 'password' | hashed_password HASHED } [ MUST_CHANGE ]
    [ , <option_list2> [ ,... ] ]

<option_list2> ::=
    SID = sid
    | DEFAULT_DATABASE = database
    | DEFAULT_LANGUAGE = language
    | CHECK_EXPIRATION = { ON | OFF}
    | CHECK_POLICY = { ON | OFF}
    | CREDENTIAL = credential_name

<sources> ::=
    WINDOWS [ WITH <windows_options>[ ,... ] ]
    | EXTERNAL PROVIDER [WITH OBJECT_ID = 'objectid']
    | CERTIFICATE certname
    | ASYMMETRIC KEY asym_key_name

<windows_options> ::=
    DEFAULT_DATABASE = database
    | DEFAULT_LANGUAGE = language
```

```syntaxsql
-- Syntax for Azure SQL Database
CREATE LOGIN login_name
  {
    FROM EXTERNAL PROVIDER [WITH OBJECT_ID = 'objectid']
    | WITH <option_list> [,..]
  }

<option_list> ::=
    PASSWORD = { 'password' }
    [ , SID = sid ]
```

```syntaxsql
-- Syntax for Azure SQL Managed Instance
CREATE LOGIN login_name [FROM EXTERNAL PROVIDER [WITH OBJECT_ID = 'objectid'] ] { WITH <option_list> [,..]}

<option_list> ::=
    PASSWORD = {'password'}
    | SID = sid
    | DEFAULT_DATABASE = database
    | DEFAULT_LANGUAGE = language
```

```syntaxsql
-- Syntax for Azure Synapse Analytics
CREATE LOGIN login_name
  {
    FROM EXTERNAL PROVIDER
    | WITH <option_list> [,..]
  }

<option_list> ::=
    PASSWORD = { 'password' }
    [ , SID = sid ]
```

```syntaxsql
-- Syntax for Analytics Platform System
CREATE LOGIN loginName { WITH <option_list1> | FROM WINDOWS }

<option_list1> ::=
    PASSWORD = { 'password' } [ MUST_CHANGE ]
    [ , <option_list> [ ,... ] ]

<option_list> ::=
      CHECK_EXPIRATION = { ON | OFF}
    | CHECK_POLICY = { ON | OFF}
```

## CREATE MASTER KEY (Transact-SQL)

`docs/t-sql/statements/create-master-key-transact-sql.md`

### Syntax

```syntaxsql
CREATE MASTER KEY [ ENCRYPTION BY PASSWORD ='password' ]
[ ; ]
```

## CREATE MATERIALIZED VIEW AS SELECT (Transact-SQL) creates a materialized view to persist the data returned from the view definition query and automatically gets updated as data changes in the underlying tables.

`docs/t-sql/statements/create-materialized-view-as-select-transact-sql.md`

### Syntax

```syntaxsql
CREATE MATERIALIZED VIEW [ schema_name. ] materialized_view_name
    WITH (
      <distribution_option>
    )
    AS <select_statement>
[;]

<distribution_option> ::=
    {
        DISTRIBUTION = HASH ( distribution_column_name )
      | DISTRIBUTION = HASH ( [distribution_column_name [, ...n]] )
      | DISTRIBUTION = ROUND_ROBIN
    }

<select_statement> ::=
    SELECT select_criteria
```

## CREATE MESSAGE TYPE (Transact-SQL)

`docs/t-sql/statements/create-message-type-transact-sql.md`

### Syntax

```syntaxsql
CREATE MESSAGE TYPE message_type_name
    [ AUTHORIZATION owner_name ]
    [ VALIDATION = {  NONE
                    | EMPTY
                    | WELL_FORMED_XML
                    | VALID_XML WITH SCHEMA COLLECTION schema_collection_name
                   } ]
[ ; ]
```

## CREATE PARTITION FUNCTION (Transact-SQL)

`docs/t-sql/statements/create-partition-function-transact-sql.md`

### Syntax

```syntaxsql
CREATE PARTITION FUNCTION partition_function_name ( input_parameter_type )
AS RANGE [ LEFT | RIGHT ]
FOR VALUES ( [ boundary_value [ ,...n ] ] )
[ ; ]
```

## CREATE PARTITION SCHEME (Transact-SQL)

`docs/t-sql/statements/create-partition-scheme-transact-sql.md`

### Syntax

```syntaxsql
CREATE PARTITION SCHEME partition_scheme_name
AS PARTITION partition_function_name
[ ALL ] TO ( { file_group_name | [ PRIMARY ] } [ , ...n ] )
[ ; ]
```

## CREATE PROCEDURE (Transact-SQL)

`docs/t-sql/statements/create-procedure-transact-sql.md`

### Syntax

> Transact-SQL syntax for stored procedures in SQL Server, Azure SQL Database, SQL database in Microsoft Fabric:

```syntaxsql
CREATE [ OR ALTER ] { PROC | PROCEDURE }
    [schema_name.] procedure_name [ ; number ]
    [ { @parameter_name [ type_schema_name. ] data_type }
        [ VARYING ] [ NULL ] [ = default ] [ OUT | OUTPUT | [READONLY]
    ] [ ,...n ]
[ WITH <procedure_option> [ ,...n ] ]
[ FOR REPLICATION ]
AS { [ BEGIN ] sql_statement [;] [ ...n ] [ END ] }
[;]

<procedure_option> ::=
    [ ENCRYPTION ]
    [ RECOMPILE ]
    [ EXECUTE AS Clause ]
```

> Transact-SQL syntax for CLR stored procedures:

```syntaxsql
CREATE [ OR ALTER ] { PROC | PROCEDURE }
    [schema_name.] procedure_name [ ; number ]
    [ { @parameter_name [ type_schema_name. ] data_type }
        [ = default ] [ OUT | OUTPUT ] [READONLY]
    ] [ ,...n ]
[ WITH EXECUTE AS Clause ]
AS { EXTERNAL NAME assembly_name.class_name.method_name }
[;]
```

> Transact-SQL syntax for natively compiled stored procedures:

```syntaxsql
CREATE [ OR ALTER ] { PROC | PROCEDURE } [schema_name.] procedure_name
    [ { @parameter data_type } [ NULL | NOT NULL ] [ = default ]
        [ OUT | OUTPUT ] [READONLY]
    ] [ ,... n ]
  WITH NATIVE_COMPILATION, SCHEMABINDING [ , EXECUTE AS clause ]
AS
{
  BEGIN ATOMIC WITH ( <set_option> [ ,... n ] )
sql_statement [;] [ ... n ]
[ END ]
}
[;]

<set_option> ::=
    LANGUAGE = [ N ] 'language'
  | TRANSACTION ISOLATION LEVEL = { SNAPSHOT | REPEATABLE READ | SERIALIZABLE }
  | [ DATEFIRST = number ]
  | [ DATEFORMAT = format ]
  | [ DELAYED_DURABILITY = { OFF | ON } ]
```

> Transact-SQL syntax for stored procedures in Azure Synapse Analytics and Parallel Data Warehouse:

```syntaxsql
CREATE { PROC | PROCEDURE } [ schema_name.] procedure_name
    [ { @parameter data_type } [ OUT | OUTPUT ] ] [ ,...n ]
AS
{
  [ BEGIN ] sql_statement [;][ ,...n ] [ END ]
}
[;]
```

> Transact-SQL syntax for stored procedures in Microsoft Fabric:

```syntaxsql
CREATE [ OR ALTER ] { PROC | PROCEDURE } [ schema_name.] procedure_name
    [ { @parameter data_type } [ OUT | OUTPUT ] ] [ ,...n ]
AS
{
  [ BEGIN ] sql_statement [;][ ,...n ] [ END ]
}
[;]
```

## CREATE QUEUE (Transact-SQL)

`docs/t-sql/statements/create-queue-transact-sql.md`

### Syntax

```syntaxsql
CREATE QUEUE <object>
   [ WITH
     [ STATUS = { ON | OFF } [ , ] ]
     [ RETENTION = { ON | OFF } [ , ] ]
     [ ACTIVATION (
         [ STATUS = { ON | OFF } , ]
           PROCEDURE_NAME = <procedure> ,
           MAX_QUEUE_READERS = max_readers ,
           EXECUTE AS { SELF | 'user_name' | OWNER }
            ) [ , ] ]
     [ POISON_MESSAGE_HANDLING (
         [ STATUS = { ON | OFF } ] ) ]
    ]
     [ ON { filegroup | [ DEFAULT ] } ]
[ ; ]

<object> ::=
{ database_name.schema_name.queue_name | schema_name.queue_name | queue_name }

<procedure> ::=
{ database_name.schema_name.stored_procedure_name | schema_name.stored_procedure_name | stored_procedure_name }
```

## CREATE REMOTE SERVICE BINDING (Transact-SQL)

`docs/t-sql/statements/create-remote-service-binding-transact-sql.md`

### Syntax

```syntaxsql
CREATE REMOTE SERVICE BINDING binding_name
   [ AUTHORIZATION owner_name ]
   TO SERVICE 'service_name'
   WITH  USER = user_name [ , ANONYMOUS = { ON | OFF } ]
[ ; ]
```

## CREATE REMOTE TABLE AS SELECT (Parallel Data Warehouse)

`docs/t-sql/statements/create-remote-table-as-select-parallel-data-warehouse.md`

### Syntax

```syntaxsql
CREATE REMOTE TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }  AT ('<connection_string>')
    [ WITH ( BATCH_SIZE = batch_size ) ]
    AS <select_statement>
[;]

<connection_string> ::=
    Data Source = { IP_address | hostname } [, port ]; User ID = user_name ;Password = strong_password;

<select_statement> ::=
    [ WITH <common_table_expression> [ ,...n ] ]
    SELECT <select_criteria>
```

## CREATE RESOURCE POOL (Transact-SQL)

`docs/t-sql/statements/create-resource-pool-transact-sql.md`

### Syntax

```syntaxsql
CREATE RESOURCE POOL pool_name
[ WITH
    (
        [ MIN_CPU_PERCENT = value ]
        [ [ , ] MAX_CPU_PERCENT = value ]
        [ [ , ] CAP_CPU_PERCENT = value ]
        [ [ , ] AFFINITY {SCHEDULER =
                  AUTO
                | ( <scheduler_range_spec> )
                | NUMANODE = ( <NUMA_node_range_spec> )
                } ]
        [ [ , ] MIN_MEMORY_PERCENT = value ]
        [ [ , ] MAX_MEMORY_PERCENT = value ]
        [ [ , ] MIN_IOPS_PER_VOLUME = value ]
        [ [ , ] MAX_IOPS_PER_VOLUME = value ]
    )
]
[;]

<scheduler_range_spec> ::=
{ SCHED_ID | SCHED_ID TO SCHED_ID }[,...n]

<NUMA_node_range_spec> ::=
{ NUMA_node_ID | NUMA_node_ID TO NUMA_node_ID }[,...n]
```

## CREATE ROLE (Transact-SQL)

`docs/t-sql/statements/create-role-transact-sql.md`

### Syntax

```syntaxsql
CREATE ROLE role_name [ AUTHORIZATION owner_name ]
```

## CREATE ROUTE (Transact-SQL)

`docs/t-sql/statements/create-route-transact-sql.md`

### Syntax

```syntaxsql
CREATE ROUTE route_name
[ AUTHORIZATION owner_name ]
WITH
   [ SERVICE_NAME = 'service_name', ]
   [ BROKER_INSTANCE = 'broker_instance_identifier' , ]
   [ LIFETIME = route_lifetime , ]
   ADDRESS =  'next_hop_address'
   [ , MIRROR_ADDRESS = 'next_hop_mirror_address' ]
[ ; ]
```

## CREATE RULE (Transact-SQL)

`docs/t-sql/statements/create-rule-transact-sql.md`

### Syntax

```syntaxsql
CREATE RULE [ schema_name . ] rule_name
AS condition_expression
[ ; ]
```

## CREATE SCHEMA (Transact-SQL)

`docs/t-sql/statements/create-schema-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, and SQL database in Microsoft Fabric.

```syntaxsql
CREATE SCHEMA schema_name_clause [ <schema_element> [ ...n ] ]

<schema_name_clause> ::=
    {
    schema_name
    | AUTHORIZATION owner_name
    | schema_name AUTHORIZATION owner_name
    }

<schema_element> ::=
    {
        table_definition | view_definition | grant_statement |
        revoke_statement | deny_statement
    }
```

> Syntax for Azure Synapse Analytics and Parallel Data Warehouse.

```syntaxsql
CREATE SCHEMA schema_name [ AUTHORIZATION owner_name ] [;]
```

## CREATE SEARCH PROPERTY LIST (Transact-SQL)

`docs/t-sql/statements/create-search-property-list-transact-sql.md`

### Syntax

```syntaxsql
CREATE SEARCH PROPERTY LIST new_list_name
   [ FROM [ database_name. ] source_list_name ]
   [ AUTHORIZATION owner_name ]
;
```

## CREATE SECURITY POLICY (Transact-SQL)

`docs/t-sql/statements/create-security-policy-transact-sql.md`

### Syntax

```syntaxsql
CREATE SECURITY POLICY [schema_name. ] security_policy_name
    { ADD [ FILTER | BLOCK ] } PREDICATE tvf_schema_name.security_predicate_function_name
      ( { column_name | expression } [ , ...n] ) ON table_schema_name. table_name
      [ <block_dml_operation> ] , [ , ...n]
    [ WITH ( STATE = { ON | OFF }  [,] [ SCHEMABINDING = { ON | OFF } ] ) ]
    [ NOT FOR REPLICATION ]
[;]

<block_dml_operation>
    [ { AFTER { INSERT | UPDATE } }
    | { BEFORE { UPDATE | DELETE } } ]
```

## CREATE SELECTIVE XML INDEX (Transact-SQL)

`docs/t-sql/statements/create-selective-xml-index-transact-sql.md`

### Syntax

```syntaxsql
CREATE SELECTIVE XML INDEX index_name
    ON <table_object> (xml_column_name)
    [WITH XMLNAMESPACES (<xmlnamespace_list>)]
    FOR (<promoted_node_path_list>)
    [WITH (<index_options>)]

<table_object> ::=
 { database_name.schema_name.table_name | schema_name.table_name | table_name }

<promoted_node_path_list> ::=
<named_promoted_node_path_item> [, <promoted_node_path_list>]

<named_promoted_node_path_item> ::=
<path_name> = <promoted_node_path_item>

<promoted_node_path_item>::=
<xquery_node_path_item> | <sql_values_node_path_item>

<xquery_node_path_item> ::=
<node_path> [AS XQUERY <xsd_type_or_node_hint>] [SINGLETON]

<xsd_type_or_node_hint> ::=
[<xsd_type>] [MAXLENGTH(x)] | node()

<sql_values_node_path_item> ::=
<node_path> AS SQL <sql_type> [SINGLETON]

<node_path> ::=
character_string_literal

<xsd_type> ::=
character_string_literal

<sql_type> ::=
identifier

<path_name> ::=
identifier

<xmlnamespace_list> ::=
<xmlnamespace_item> [, <xmlnamespace_list>]

<xmlnamespace_item> ::=
<xmlnamespace_uri> AS <xmlnamespace_prefix>

<xml_namespace_uri> ::=
character_string_literal

<xml_namespace_prefix> ::=
identifier

<index_options> ::=
(
  | PAD_INDEX  = { ON | OFF }
  | FILLFACTOR = fillfactor
  | SORT_IN_TEMPDB = { ON | OFF }
  | IGNORE_DUP_KEY = OFF
  | DROP_EXISTING = { ON | OFF }
  | ONLINE = OFF
  | ALLOW_ROW_LOCKS = { ON | OFF }
  | ALLOW_PAGE_LOCKS = { ON | OFF }
  | MAXDOP = max_degree_of_parallelism
)
```

## CREATE SEQUENCE (Transact-SQL)

`docs/t-sql/statements/create-sequence-transact-sql.md`

### Syntax

```syntaxsql
CREATE SEQUENCE [ schema_name . ] sequence_name
    [ AS [ built_in_integer_type | user-defined_integer_type ] ]
    [ START WITH <constant> ]
    [ INCREMENT BY <constant> ]
    [ { MINVALUE [ <constant> ] } | { NO MINVALUE } ]
    [ { MAXVALUE [ <constant> ] } | { NO MAXVALUE } ]
    [ CYCLE | { NO CYCLE } ]
    [ { CACHE [ <constant> ] } | { NO CACHE } ]
    [ ; ]
```

## CREATE SERVER AUDIT SPECIFICATION (Transact-SQL)

`docs/t-sql/statements/create-server-audit-specification-transact-sql.md`

### Syntax

```syntaxsql
CREATE SERVER AUDIT SPECIFICATION audit_specification_name
FOR SERVER AUDIT audit_name
{
    { ADD ( { audit_action_group_name } )
    } [, ...n]
    [ WITH ( STATE = { ON | OFF } ) ]
}
[ ; ]
```

## CREATE SERVER AUDIT (Transact-SQL)

`docs/t-sql/statements/create-server-audit-transact-sql.md`

### Syntax

```syntaxsql
CREATE SERVER AUDIT audit_name
{
    TO {
        [ FILE ( <file_options> [ ,... n ] ) ]
        | APPLICATION_LOG
        | SECURITY_LOG
        | URL
        | EXTERNAL_MONITOR
    }
    [ WITH ( <audit_options> [ ,... n ] ) ]
    [ WHERE <predicate_expression> ]
}
[ ; ]

<file_options> ::=
{
    FILEPATH = 'os_file_path'
    [ , MAXSIZE = { max_size { MB | GB | TB } | UNLIMITED } ]
    [ , { MAX_ROLLOVER_FILES = { integer | UNLIMITED } } | { MAX_FILES = integer } ]
    [ , RESERVE_DISK_SPACE = { ON | OFF } ]
}

<audit_options> ::=
{
    [ QUEUE_DELAY = integer ]
    [ , ON_FAILURE = { CONTINUE | SHUTDOWN | FAIL_OPERATION } ]
    [ , AUDIT_GUID = uniqueidentifier ]
    [ , OPERATOR_AUDIT = { ON | OFF } ]
    [ , RETENTION_DAYS = integer ]
}

<predicate_expression> ::=
    { [ NOT ] <predicate_factor>
    [ { AND | OR } [ NOT ] { <predicate_factor> } ] [ ,... n ] }

<predicate_factor> ::=
    event_field_name { = | < > | != | > | >= | < | <= | LIKE }
    { number | 'string' }
```

## CREATE SERVER ROLE (Transact-SQL)

`docs/t-sql/statements/create-server-role-transact-sql.md`

### Syntax

```syntaxsql
CREATE SERVER ROLE role_name [ AUTHORIZATION server_principal ]
```

## CREATE SERVICE (Transact-SQL)

`docs/t-sql/statements/create-service-transact-sql.md`

### Syntax

```syntaxsql
CREATE SERVICE service_name
   [ AUTHORIZATION owner_name ]
   ON QUEUE [ schema_name. ]queue_name
   [ ( contract_name | [DEFAULT][ ,...n ] ) ]
[ ; ]
```

## CREATE SPATIAL INDEX (Transact-SQL)

`docs/t-sql/statements/create-spatial-index-transact-sql.md`

### Syntax

```syntaxsql
CREATE SPATIAL INDEX index_name
  ON <object> ( spatial_column_name )
    {
       <geometry_tessellation> | <geography_tessellation>
    }
  [ ON { filegroup_name | "default" } ]
[;]

<object> ::=
    { database_name.schema_name.table_name | schema_name.table_name | table_name }

<geometry_tessellation> ::=
{
  <geometry_automatic_grid_tessellation>
| <geometry_manual_grid_tessellation>
}

<geometry_automatic_grid_tessellation> ::=
{
    [ USING GEOMETRY_AUTO_GRID ]
          WITH  (
        <bounding_box>
            [ [,] <tessellation_cells_per_object> [ ,...n] ]
            [ [,] <spatial_index_option> [ ,...n] ]
                 )
}

<geometry_manual_grid_tessellation> ::=
{
       [ USING GEOMETRY_GRID ]
         WITH (
                    <bounding_box>
                        [ [,]<tessellation_grid> [ ,...n] ]
                        [ [,]<tessellation_cells_per_object> [ ,...n] ]
                        [ [,]<spatial_index_option> [ ,...n] ]
   )
}

<geography_tessellation> ::=
{
      <geography_automatic_grid_tessellation> | <geography_manual_grid_tessellation>
}

<geography_automatic_grid_tessellation> ::=
{
    [ USING GEOGRAPHY_AUTO_GRID ]
    [ WITH (
        [ [,] <tessellation_cells_per_object> [ ,...n] ]
        [ [,] <spatial_index_option> ]
     ) ]
}

<geography_manual_grid_tessellation> ::=
{
    [ USING GEOGRAPHY_GRID ]
    [ WITH (
                [ <tessellation_grid> [ ,...n] ]
                [ [,] <tessellation_cells_per_object> [ ,...n] ]
                [ [,] <spatial_index_option> [ ,...n] ]
                ) ]
}

<bounding_box> ::=
{
      BOUNDING_BOX = ( {
       xmin, ymin, xmax, ymax
       | <named_bb_coordinate>, <named_bb_coordinate>, <named_bb_coordinate>, <named_bb_coordinate>
  } )
}

<named_bb_coordinate> ::= { XMIN = xmin | YMIN = ymin | XMAX = xmax | YMAX=ymax }

<tessellation_grid> ::=
{
    GRIDS = ( { <grid_level> [ ,...n ] | <grid_size>, <grid_size>, <grid_size>, <grid_size>  }
        )
}
<tessellation_cells_per_object> ::=
{
   CELLS_PER_OBJECT = n
}

<grid_level> ::=
{
     LEVEL_1 = <grid_size>
  |  LEVEL_2 = <grid_size>
  |  LEVEL_3 = <grid_size>
  |  LEVEL_4 = <grid_size>
}

<grid_size> ::= { LOW | MEDIUM | HIGH }

<spatial_index_option> ::=
{
    PAD_INDEX = { ON | OFF }
  | FILLFACTOR = fillfactor
  | SORT_IN_TEMPDB = { ON | OFF }
  | IGNORE_DUP_KEY = OFF
  | STATISTICS_NORECOMPUTE = { ON | OFF }
  | DROP_EXISTING = { ON | OFF }
  | ONLINE = OFF
  | ALLOW_ROW_LOCKS = { ON | OFF }
  | ALLOW_PAGE_LOCKS = { ON | OFF }
  | MAXDOP = max_degree_of_parallelism
    | DATA_COMPRESSION = { NONE | ROW | PAGE }
}
```

## CREATE STATISTICS (Transact-SQL)

`docs/t-sql/statements/create-statistics-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, and Azure SQL Managed Instance.

```syntaxsql
-- Create statistics on an external table
CREATE STATISTICS statistics_name
ON { table_or_indexed_view_name } ( column [ , ...n ] )
    [ WITH FULLSCAN ] ;

-- Create statistics on a regular table or indexed view
CREATE STATISTICS statistics_name
ON { table_or_indexed_view_name } ( column [ , ...n ] )
    [ WHERE <filter_predicate> ]
    [ WITH
        [ FULLSCAN
            [ [ , ] PERSIST_SAMPLE_PERCENT = { ON | OFF } ]
          | SAMPLE number { PERCENT | ROWS }
            [ [ , ] PERSIST_SAMPLE_PERCENT = { ON | OFF } ]
          | <update_stats_stream_option> [ , ...n ]
        [ [ , ] NORECOMPUTE ]
        [ [ , ] INCREMENTAL = { ON | OFF } ]
        [ [ , ] MAXDOP = max_degree_of_parallelism ]
        [ [ , ] AUTO_DROP = { ON | OFF } ]
        ]
    ] ;

<filter_predicate> ::=
    <conjunct> [ AND <conjunct> ]

<conjunct> ::=
    <disjunct> | <comparison>

<disjunct> ::=
        column_name IN (constant , ...)

<comparison> ::=
        column_name <comparison_op> constant

<comparison_op> ::=
    IS | IS NOT | = | <> | != | > | >= | !> | < | <= | !<

<update_stats_stream_option> ::=
    [ STATS_STREAM = stats_stream ]
    [ ROWCOUNT = numeric_constant ]
    [ PAGECOUNT = numeric_constant ]
```

> Syntax for Azure Synapse Analytics and Analytics Platform System (PDW).

```syntaxsql
CREATE STATISTICS statistics_name
    ON { database_name.schema_name.table_name | schema_name.table_name | table_name }
    ( column_name  [ , ...n ] )
    [ WHERE <filter_predicate> ]
    [ WITH {
           FULLSCAN
           | SAMPLE number PERCENT
      }
    ]
[ ; ]

<filter_predicate> ::=
    <conjunct> [ AND <conjunct> ]

<conjunct> ::=
    <disjunct> | <comparison>

<disjunct> ::=
        column_name IN (constant , ...)

<comparison> ::=
        column_name <comparison_op> constant

<comparison_op> ::=
    IS | IS NOT | = | <> | != | > | >= | !> | < | <= | !<
```

> Syntax for Microsoft Fabric.

```syntaxsql
CREATE STATISTICS statistics_name
    ON { database_name.schema_name.table_name | schema_name.table_name | table_name }
    ( column_name )
    [ WITH {
           FULLSCAN
           | SAMPLE number PERCENT
      }
    ]
[ ; ]
```

## CREATE SYMMETRIC KEY (Transact-SQL)

`docs/t-sql/statements/create-symmetric-key-transact-sql.md`

### Syntax

```syntaxsql
CREATE SYMMETRIC KEY key_name
    [ AUTHORIZATION owner_name ]
    [ FROM PROVIDER provider_name ]
    WITH
        [
            <key_options> [ , ... n ]
            | ENCRYPTION BY <encrypting_mechanism> [ , ... n ]
        ]

<key_options> ::=
    KEY_SOURCE = 'pass_phrase'
    | ALGORITHM = <algorithm>
    | IDENTITY_VALUE = 'identity_phrase'
    | PROVIDER_KEY_NAME = 'key_name_in_provider'
    | CREATION_DISPOSITION = { CREATE_NEW | OPEN_EXISTING }

<algorithm> ::=
    DES | TRIPLE_DES | TRIPLE_DES_3KEY | RC2 | RC4 | RC4_128
    | DESX | AES_128 | AES_192 | AES_256

<encrypting_mechanism> ::=
    CERTIFICATE certificate_name
    | PASSWORD = 'password'
    | SYMMETRIC KEY symmetric_key_name
    | ASYMMETRIC KEY asym_key_name
```

## CREATE SYNONYM (Transact-SQL)

`docs/t-sql/statements/create-synonym-transact-sql.md`

### Syntax

> SQL Server syntax:

```syntaxsql
CREATE SYNONYM [ schema_name_1. ] synonym_name FOR <object>

<object> ::=
{
    [
        server_name. [ database_name ] . [ schema_name_2 ] .
        | database_name. [ schema_name_2 ] .
        | schema_name_2.
    ]
    object_name
}
```

> Azure SQL Database and SQL database in Microsoft Fabric syntax:

```syntaxsql
CREATE SYNONYM [ schema_name_1. ] synonym_name FOR <object>

<object> ::=
{
    [ database_name. [ schema_name_2 ] . | schema_name_2. ] object_name
}
```

## CREATE TABLE AS CLONE OF

`docs/t-sql/statements/create-table-as-clone-of-transact-sql.md`

### Syntax

```syntaxsql
CREATE TABLE
    { database_name.schema_name.table_name | schema_name.table_name | table_name }
AS CLONE OF
    { database_name.schema_name.table_name | schema_name.table_name | table_name } [AT {point_in_time}]
```

## CREATE TABLE AS SELECT (Microsoft Fabric and Azure Synapse Analytics)

`docs/t-sql/statements/create-table-as-select-azure-sql-data-warehouse.md`

### Syntax

```syntaxsql
CREATE TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
    [ ( column_name [ ,...n ] ) ]
    WITH (
      <distribution_option> -- required
      [ , <table_option> [ ,...n ] ]
    )
    AS <select_statement>
    OPTION <query_hint>
[;]

<distribution_option> ::=
    {
        DISTRIBUTION = HASH ( distribution_column_name )
      | DISTRIBUTION = HASH ( [distribution_column_name [, ...n]] )
      | DISTRIBUTION = ROUND_ROBIN
      | DISTRIBUTION = REPLICATE
    }

<table_option> ::=
    {
        CLUSTERED COLUMNSTORE INDEX --default for Synapse Analytics
      | CLUSTERED COLUMNSTORE INDEX ORDER (column[,...n])
      | HEAP --default for Parallel Data Warehouse
      | CLUSTERED INDEX ( { index_column_name [ ASC | DESC ] } [ ,...n ] ) --default is ASC
    }
      | PARTITION ( partition_column_name RANGE [ LEFT | RIGHT ] --default is LEFT
        FOR VALUES ( [ boundary_value [,...n] ] ) )

<select_statement> ::=
    [ WITH <common_table_expression> [ ,...n ] ]
    SELECT select_criteria

<query_hint> ::=
    {
        MAXDOP
    }
```

```syntaxsql
CREATE TABLE { warehouse_name.schema_name.table_name | schema_name.table_name | table_name } (
) WITH (CLUSTER BY [ ,... n ])
AS <select_statement>
[;]

<select_statement> ::=
    SELECT select_criteria
```

## CREATE TABLE

`docs/t-sql/statements/create-table-azure-sql-data-warehouse.md`

### Syntax

```syntaxsql
-- Create a new table.
CREATE TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
    (
      { column_name <data_type>  [ <column_options> ] } [ ,...n ]
    )
    [ WITH ( <table_option> [ ,...n ] ) ]
[;]

<column_options> ::=
    [ COLLATE Windows_collation_name ]
    [ NULL | NOT NULL ] -- default is NULL
    [ IDENTITY [ ( seed, increment ) ] ]
    [ <column_constraint> ]

<column_constraint>::=
    {
        DEFAULT constant_expression
        | PRIMARY KEY NONCLUSTERED NOT ENFORCED -- Applies to Azure Synapse Analytics only
        | UNIQUE NOT ENFORCED -- Applies to Azure Synapse Analytics only
    }

<table_option> ::=
    {
       CLUSTERED COLUMNSTORE INDEX -- default for Azure Synapse Analytics
      | CLUSTERED COLUMNSTORE INDEX ORDER (column [,...n])
      | HEAP --default for Parallel Data Warehouse
      | CLUSTERED INDEX ( { index_column_name [ ASC | DESC ] } [ ,...n ] ) -- default is ASC
    }
    {
        DISTRIBUTION = HASH ( distribution_column_name )
      | DISTRIBUTION = HASH ( [distribution_column_name [, ...n]] )
      | DISTRIBUTION = ROUND_ROBIN -- default for Azure Synapse Analytics
      | DISTRIBUTION = REPLICATE -- default for Parallel Data Warehouse
    }
    | PARTITION ( partition_column_name RANGE [ LEFT | RIGHT ] -- default is LEFT
        FOR VALUES ( [ boundary_value [,...n] ] ) )

<data type> ::=
      datetimeoffset [ ( n ) ]
    | datetime2 [ ( n ) ]
    | datetime
    | smalldatetime
    | date
    | time [ ( n ) ]
    | float [ ( n ) ]
    | real [ ( n ) ]
    | decimal [ ( precision [ , scale ] ) ]
    | numeric [ ( precision [ , scale ] ) ]
    | money
    | smallmoney
    | bigint
    | int
    | smallint
    | tinyint
    | bit
    | nvarchar [ ( n | max ) ]  -- max applies only to Azure Synapse Analytics
    | nchar [ ( n ) ]
    | varchar [ ( n | max ) ] -- max applies only to Azure Synapse Analytics
    | char [ ( n ) ]
    | varbinary [ ( n | max ) ] -- max applies only to Azure Synapse Analytics
    | binary [ ( n ) ]
    | uniqueidentifier
```

```syntaxsql
-- Create a new table.
CREATE TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
    (
      { column_name <data_type>  [ <column_options> ] } [ ,...n ]
    )  WITH (CLUSTER BY [ ,... n ])
[;]

<column_options> ::=
    [ NULL | NOT NULL ] -- default is NULL
    [ COLLATE Windows_collation_name ]

<data type> ::=
      datetime2 ( n )
    | date
    | time ( n )
    | float [ ( n ) ]
    | real [ ( n ) ]
    | decimal [ ( precision [ , scale ] ) ]
    | numeric [ ( precision [ , scale ] ) ]
    | bigint
    | int
    | smallint
    | bit
    | varchar [ ( n | MAX ) ]
    | char [ ( n ) ]
    | varbinary [ ( n | MAX ) ]
    | uniqueidentifier
```

## CREATE TABLE (SQL Graph)

`docs/t-sql/statements/create-table-sql-graph.md`

### Syntax

```syntaxsql
CREATE TABLE
    { database_name.schema_name.table_name | schema_name.table_name | table_name }
    ( { <column_definition> }
       | <computed_column_definition>
       | <column_set_definition>
       | [ <table_constraint> ] [ ,... n ]
       | [ <table_index> ] }
          [ ,...n ]
    )
    AS [ NODE | EDGE ]
    [ ON { partition_scheme_name ( partition_column_name )
           | filegroup
           | "default" } ]
[ ; ]

< table_constraint > ::=
[ CONSTRAINT constraint_name ]
{
    { PRIMARY KEY | UNIQUE }
        [ CLUSTERED | NONCLUSTERED ]
        (column [ ASC | DESC ] [ ,...n ] )
        [
            WITH FILLFACTOR = fillfactor
           |WITH ( <index_option> [ , ...n ] )
        ]
        [ ON { partition_scheme_name (partition_column_name)
            | filegroup | "default" } ]
    | FOREIGN KEY
        ( column [ ,...n ] )
        REFERENCES referenced_table_name [ ( ref_column [ ,...n ] ) ]
        [ ON DELETE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
        [ ON UPDATE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
        [ NOT FOR REPLICATION ]
    | CONNECTION
        ( { node_table TO node_table }
          [ , {node_table TO node_table }]
          [ , ...n ]
        )
        [ ON DELETE { NO ACTION | CASCADE } ]
    | CHECK [ NOT FOR REPLICATION ] ( logical_expression )
```

## IDENTITY (Property) (Transact-SQL)

`docs/t-sql/statements/create-table-transact-sql-identity-property.md`

### Syntax

Marked for `=fabric`.

> Syntax for Fabric Data Warehouse:

```syntaxsql
IDENTITY
```

Marked for `=azuresqldb-current || =azure-sqldw-latest || >=sql-server-2017 || >=sql-server-linux-2017 || =azuresqldb-mi-current || =fabric-sqldb`.

```syntaxsql
IDENTITY [ (seed , increment) ]
```

## CREATE TABLE (Transact-SQL)

`docs/t-sql/statements/create-table-transact-sql.md`

### Common syntax

> Simple CREATE TABLE syntax (common if not using options):

```syntaxsql
CREATE TABLE
    { database_name.schema_name.table_name | schema_name.table_name | table_name }
    ( { <column_definition> } [ ,... n ] )
[ ; ]
```

### Full syntax

> Disk-based CREATE TABLE syntax:

```syntaxsql
CREATE TABLE
    { database_name.schema_name.table_name | schema_name.table_name | table_name }
    [ AS FileTable ]
    ( { <column_definition>
        | <computed_column_definition>
        | <column_set_definition>
        | [ <table_constraint> ] [ ,... n ]
        | [ <table_index> ] }
          [ ,... n ]
          [ PERIOD FOR SYSTEM_TIME ( system_start_time_column_name
             , system_end_time_column_name ) ]
      )
    [ ON { partition_scheme_name ( partition_column_name )
           | filegroup
           | "default" } ]
    [ TEXTIMAGE_ON { filegroup | "default" } ]
    [ FILESTREAM_ON { partition_scheme_name
           | filegroup
           | "default" } ]
    [ WITH ( <table_option> [ ,... n ] ) ]
[ ; ]

<column_definition> ::=
column_name <data_type>
    [ FILESTREAM ]
    [ COLLATE collation_name ]
    [ SPARSE ]
    [ MASKED WITH ( FUNCTION = 'mask_function' ) ]
    [ [ CONSTRAINT constraint_name ] DEFAULT constant_expression ]
    [ IDENTITY [ ( seed , increment ) ] ]
    [ NOT FOR REPLICATION ]
    [ GENERATED ALWAYS AS { ROW | TRANSACTION_ID | SEQUENCE_NUMBER } { START | END } [ HIDDEN ] ]
    [ [ CONSTRAINT constraint_name ] {NULL | NOT NULL} ]
    [ ROWGUIDCOL ]
    [ ENCRYPTED WITH
        ( COLUMN_ENCRYPTION_KEY = key_name ,
          ENCRYPTION_TYPE = { DETERMINISTIC | RANDOMIZED } ,
          ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256'
        ) ]
    [ <column_constraint> [ ,... n ] ]
    [ <column_index> ]

<data_type> ::=
[ type_schema_name. ] type_name
    [ ( precision [ , scale ] | max |
        [ { CONTENT | DOCUMENT } ] xml_schema_collection ) ]

<column_constraint> ::=
[ CONSTRAINT constraint_name ]
{
   { PRIMARY KEY | UNIQUE }
        [ CLUSTERED | NONCLUSTERED ]
        [ ( <column_name> [ ,... n ] ) ]
        [
            WITH FILLFACTOR = fillfactor
          | WITH ( <index_option> [ ,... n ] )
        ]
        [ ON { partition_scheme_name ( partition_column_name )
            | filegroup | "default" } ]

  | [ FOREIGN KEY ]
        REFERENCES [ schema_name. ] referenced_table_name [ ( ref_column ) ]
        [ ON DELETE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
        [ ON UPDATE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
        [ NOT FOR REPLICATION ]

  | CHECK [ NOT FOR REPLICATION ] ( logical_expression )
}

<column_index> ::=
 INDEX index_name [ CLUSTERED | NONCLUSTERED ]
    [ WITH ( <index_option> [ ,... n ] ) ]
    [ ON { partition_scheme_name ( column_name )
         | filegroup_name
         | default
         }
    ]
    [ FILESTREAM_ON { filestream_filegroup_name | partition_scheme_name | "NULL" } ]

<computed_column_definition> ::=
column_name AS computed_column_expression
[ PERSISTED [ NOT NULL ] ]
[
    [ CONSTRAINT constraint_name ]
    { PRIMARY KEY | UNIQUE }
        [ CLUSTERED | NONCLUSTERED ]
        [
            WITH FILLFACTOR = fillfactor
          | WITH ( <index_option> [ ,... n ] )
        ]
        [ ON { partition_scheme_name ( partition_column_name )
        | filegroup | "default" } ]

    | [ FOREIGN KEY ]
        REFERENCES referenced_table_name [ ( ref_column ) ]
        [ ON DELETE { NO ACTION | CASCADE } ]
        [ ON UPDATE { NO ACTION } ]
        [ NOT FOR REPLICATION ]

    | CHECK [ NOT FOR REPLICATION ] ( logical_expression )
]

<column_set_definition> ::=
column_set_name XML COLUMN_SET FOR ALL_SPARSE_COLUMNS

<table_constraint> ::=
[ CONSTRAINT constraint_name ]
{
    { PRIMARY KEY | UNIQUE }
        [ CLUSTERED | NONCLUSTERED ]
        ( column_name [ ASC | DESC ] [ ,... n ] )
        [
            WITH FILLFACTOR = fillfactor
           | WITH ( <index_option> [ ,... n ] )
        ]
        [ ON { partition_scheme_name (partition_column_name)
            | filegroup | "default" } ]
    | FOREIGN KEY
        ( column_name [ ,... n ] )
        REFERENCES referenced_table_name [ ( ref_column [ ,... n ] ) ]
        [ ON DELETE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
        [ ON UPDATE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
        [ NOT FOR REPLICATION ]
    | CHECK [ NOT FOR REPLICATION ] ( logical_expression )
}

<table_index> ::=
{
    {
      INDEX index_name [ UNIQUE ] [ CLUSTERED | NONCLUSTERED ]
         ( column_name [ ASC | DESC ] [ ,... n ] )
    | INDEX index_name CLUSTERED COLUMNSTORE [ ORDER (column_name [ , ...n ] ) ]
    | INDEX index_name [ NONCLUSTERED ] COLUMNSTORE ( column_name [ ,... n ] )
    }
    [ INCLUDE ( column_name [ ,... n ] ) ]
    [ WHERE <filter_predicate> ]
    [ WITH ( <index_option> [ ,... n ] ) ]
    [ ON { partition_scheme_name ( column_name )
         | filegroup_name
         | default
         }
    ]
    [ FILESTREAM_ON { filestream_filegroup_name | partition_scheme_name | "NULL" } ]
}

<table_option> ::=
{
    [ DATA_COMPRESSION = { NONE | ROW | PAGE }
      [ ON PARTITIONS ( { <partition_number_expression> | <range> }
      [ ,... n ] ) ] ]
    [ XML_COMPRESSION = { ON | OFF }
      [ ON PARTITIONS ( { <partition_number_expression> | <range> }
      [ ,... n ] ) ] ]
    [ FILETABLE_DIRECTORY = <directory_name> ]
    [ FILETABLE_COLLATE_FILENAME = { <collation_name> | database_default } ]
    [ FILETABLE_PRIMARY_KEY_CONSTRAINT_NAME = <constraint_name> ]
    [ FILETABLE_STREAMID_UNIQUE_CONSTRAINT_NAME = <constraint_name> ]
    [ FILETABLE_FULLPATH_UNIQUE_CONSTRAINT_NAME = <constraint_name> ]
    [ SYSTEM_VERSIONING = ON
        [ ( HISTORY_TABLE = schema_name.history_table_name
          [ , HISTORY_RETENTION_PERIOD = <history_retention_period> ]
          [ , DATA_CONSISTENCY_CHECK = { ON | OFF } ]
    ) ]
    ]
    [ REMOTE_DATA_ARCHIVE =
      {
        ON [ ( <table_stretch_options> [ ,... n] ) ]
        | OFF ( MIGRATION_STATE = PAUSED )
      }
    ]
    [ DATA_DELETION = ON
          { (
             FILTER_COLUMN = column_name,
             RETENTION_PERIOD = { INFINITE | number { DAY | DAYS | WEEK | WEEKS
                              | MONTH | MONTHS | YEAR | YEARS } }
        ) }
    ]
    [ LEDGER = ON [ ( <ledger_option> [ ,... n ] ) ]
    | OFF
    ]
}

<ledger_option>::=
{
    [ LEDGER_VIEW = schema_name.ledger_view_name [ ( <ledger_view_option> [ ,... n ] ) ] ]
    [ APPEND_ONLY = ON | OFF ]
}

<ledger_view_option>::=
{
    [ TRANSACTION_ID_COLUMN_NAME = transaction_id_column_name ]
    [ SEQUENCE_NUMBER_COLUMN_NAME = sequence_number_column_name ]
    [ OPERATION_TYPE_COLUMN_NAME = operation_type_id column_name ]
    [ OPERATION_TYPE_DESC_COLUMN_NAME = operation_type_desc_column_name ]
}

<table_stretch_options> ::=
{
    [ FILTER_PREDICATE = { NULL | table_predicate_function } , ]
      MIGRATION_STATE = { OUTBOUND | INBOUND | PAUSED }
}

<index_option> ::=
{
    PAD_INDEX = { ON | OFF }
  | FILLFACTOR = fillfactor
  | IGNORE_DUP_KEY = { ON | OFF }
  | STATISTICS_NORECOMPUTE = { ON | OFF }
  | STATISTICS_INCREMENTAL = { ON | OFF }
  | ALLOW_ROW_LOCKS = { ON | OFF }
  | ALLOW_PAGE_LOCKS = { ON | OFF }
  | OPTIMIZE_FOR_SEQUENTIAL_KEY = { ON | OFF }
  | COMPRESSION_DELAY = { 0 | delay [ Minutes ] }
  | DATA_COMPRESSION = { NONE | ROW | PAGE | COLUMNSTORE | COLUMNSTORE_ARCHIVE }
       [ ON PARTITIONS ( { partition_number_expression | <range> }
       [ ,... n ] ) ]
  | XML_COMPRESSION = { ON | OFF }
      [ ON PARTITIONS ( { <partition_number_expression> | <range> }
      [ ,... n ] ) ]
}

<range> ::=
<partition_number_expression> TO <partition_number_expression>
```

### Syntax for memory optimized tables

> Memory optimized CREATE TABLE syntax:

```syntaxsql
CREATE TABLE
    { database_name.schema_name.table_name | schema_name.table_name | table_name }
    ( { <column_definition>
    | [ <table_constraint> ] [ ,... n ]
    | [ <table_index> ]
      [ ,... n ] }
      [ PERIOD FOR SYSTEM_TIME ( system_start_time_column_name
        , system_end_time_column_name ) ]
)
    [ WITH ( <table_option> [ ,... n ] ) ]
 [ ; ]

<column_definition> ::=
column_name <data_type>
    [ COLLATE collation_name ]
    [ GENERATED ALWAYS AS ROW { START | END } [ HIDDEN ] ]
    [ NULL | NOT NULL ]
    [ [ CONSTRAINT constraint_name ] DEFAULT memory_optimized_constant_expression ]
    | [ IDENTITY [ ( 1, 1 ) ] ]
    [ <column_constraint> ]
    [ <column_index> ]

<data_type> ::=
 [type_schema_name. ] type_name [ (precision [ , scale ]) ]

<column_constraint> ::=
 [ CONSTRAINT constraint_name ]
{
  { PRIMARY KEY | UNIQUE }
      { NONCLUSTERED
        | NONCLUSTERED HASH WITH ( BUCKET_COUNT = bucket_count )
      }
  [ ( <column_name> [ ,... n ] ) ]
  | [ FOREIGN KEY ]
        REFERENCES [ schema_name. ] referenced_table_name [ ( ref_column ) ]
  | CHECK ( logical_expression )
}

<table_constraint> ::=
 [ CONSTRAINT constraint_name ]
{
   { PRIMARY KEY | UNIQUE }
     {
       NONCLUSTERED ( column_name [ ASC | DESC ] [ ,... n ])
       | NONCLUSTERED HASH ( column_name [ ,... n ] ) WITH ( BUCKET_COUNT = bucket_count )
                    }
    | FOREIGN KEY
        ( column_name [ ,... n ] )
        REFERENCES referenced_table_name [ ( ref_column [ ,... n ] ) ]
    | CHECK ( logical_expression )
}

<column_index> ::=
  INDEX index_name
{ [ NONCLUSTERED ] | [ NONCLUSTERED ] HASH WITH ( BUCKET_COUNT = bucket_count ) }

<table_index> ::=
  INDEX index_name
{   [ NONCLUSTERED ] HASH ( column_name [ ,... n ] ) WITH ( BUCKET_COUNT = bucket_count )
  | [ NONCLUSTERED ] ( column_name [ ASC | DESC ] [ ,... n ] )
      [ ON filegroup_name | default ]
  | CLUSTERED COLUMNSTORE [ WITH ( COMPRESSION_DELAY = { 0 | delay [ Minutes ] } ) ]
      [ ON filegroup_name | default ]
}

<table_option> ::=
{
    MEMORY_OPTIMIZED = ON
  | DURABILITY = { SCHEMA_ONLY | SCHEMA_AND_DATA }
  | SYSTEM_VERSIONING = ON [ ( HISTORY_TABLE = schema_name.history_table_name
        [ , DATA_CONSISTENCY_CHECK = { ON | OFF } ] ) ]
}
```

## CREATE TRIGGER (Transact-SQL)

`docs/t-sql/statements/create-trigger-transact-sql.md`

### SQL Server syntax

> Trigger on an `INSERT`, `UPDATE`, or `DELETE` statement to a table or view (DML trigger):

```syntaxsql
CREATE [ OR ALTER ] TRIGGER [ schema_name . ] trigger_name
ON { table | view }
[ WITH <dml_trigger_option> [ , ...n ] ]
{ FOR | AFTER | INSTEAD OF }
{ [ INSERT ] [ , ] [ UPDATE ] [ , ] [ DELETE ] }
[ WITH APPEND ]
[ NOT FOR REPLICATION ]
AS { sql_statement  [ ; ] [ , ...n ] | EXTERNAL NAME <method_specifier [ ; ] > }

<dml_trigger_option> ::=
    [ ENCRYPTION ]
    [ EXECUTE AS Clause ]

<method_specifier> ::=
    assembly_name.class_name.method_name
```

> Trigger on an `INSERT`, `UPDATE`, or `DELETE` statement to a table (DML trigger on memory-optimized tables):

```syntaxsql
CREATE [ OR ALTER ] TRIGGER [ schema_name . ] trigger_name
ON { table }
[ WITH <dml_trigger_option> [ , ...n ] ]
{ FOR | AFTER }
{ [ INSERT ] [ , ] [ UPDATE ] [ , ] [ DELETE ] }
AS { sql_statement  [ ; ] [ , ...n ] }

<dml_trigger_option> ::=
    [ NATIVE_COMPILATION ]
    [ SCHEMABINDING ]
    [ EXECUTE AS Clause ]
```

> Trigger on a `CREATE`, `ALTER`, `DROP`, `GRANT`, `DENY`, `REVOKE`, or `UPDATE` statement (DDL trigger):

```syntaxsql
CREATE [ OR ALTER ] TRIGGER trigger_name
ON { ALL SERVER | DATABASE }
[ WITH <ddl_trigger_option> [ , ...n ] ]
{ FOR | AFTER } { event_type | event_group } [ , ...n ]
AS { sql_statement  [ ; ] [ , ...n ] | EXTERNAL NAME < method specifier >  [ ; ] }

<ddl_trigger_option> ::=
    [ ENCRYPTION ]
    [ EXECUTE AS Clause ]
```

> Trigger on a `LOGON` event (Logon trigger):

```syntaxsql
CREATE [ OR ALTER ] TRIGGER trigger_name
ON ALL SERVER
[ WITH <logon_trigger_option> [ , ...n ] ]
{ FOR | AFTER } LOGON
AS { sql_statement  [ ; ] [ , ...n ] | EXTERNAL NAME < method specifier >  [ ; ] }

<logon_trigger_option> ::=
    [ ENCRYPTION ]
    [ EXECUTE AS Clause ]
```

### Azure SQL Database or SQL database in Fabric syntax

> Trigger on an `INSERT`, `UPDATE`, or `DELETE` statement to a table or view (DML trigger):

```syntaxsql
CREATE [ OR ALTER ] TRIGGER [ schema_name . ] trigger_name
ON { table | view }
 [ WITH <dml_trigger_option> [ , ...n ] ]
{ FOR | AFTER | INSTEAD OF }
{ [ INSERT ] [ , ] [ UPDATE ] [ , ] [ DELETE ] }
  AS { sql_statement  [ ; ] [ , ...n ] [ ; ] > }

<dml_trigger_option> ::=
        [ EXECUTE AS Clause ]
```

> Trigger on a `CREATE`, `ALTER`, `DROP`, `GRANT`, `DENY`, `REVOKE`, or `UPDATE STATISTICS` statement (DDL trigger):

```syntaxsql
CREATE [ OR ALTER ] TRIGGER trigger_name
ON { DATABASE }
 [ WITH <ddl_trigger_option> [ , ...n ] ]
{ FOR | AFTER } { event_type | event_group } [ , ...n ]
AS { sql_statement  [ ; ] [ , ...n ]  [ ; ] }

<ddl_trigger_option> ::=
    [ EXECUTE AS Clause ]
```

## CREATE TYPE (Transact-SQL)

`docs/t-sql/statements/create-type-transact-sql.md`

### Syntax

> User-defined data type syntax:

```syntaxsql
CREATE TYPE [ schema_name. ] type_name
{
      FROM base_type
      [ ( precision [ , scale ] ) ]
      [ NULL | NOT NULL ]
    | EXTERNAL NAME assembly_name [ .class_name ]
    | AS TABLE ( { <column_definition> | <computed_column_definition> [ , ...n ]
      [ <table_constraint> ] [ , ...n ]
      [ <table_index> ] [ , ...n ] } )
} [ ; ]

<column_definition> ::=
column_name <data_type>
    [ COLLATE collation_name ]
    [ NULL | NOT NULL ]
    [
        DEFAULT constant_expression ]
      | [ IDENTITY [ ( seed , increment ) ]
    ]
    [ ROWGUIDCOL ] [ <column_constraint> [ ...n ] ]

<data type> ::=
[ type_schema_name . ] type_name
    [ ( precision [ , scale ] | max |
                [ { CONTENT | DOCUMENT } ] xml_schema_collection ) ]

<column_constraint> ::=
{ { PRIMARY KEY | UNIQUE }
        [ CLUSTERED | NONCLUSTERED ]
        [
            WITH ( <index_option> [ , ...n ] )
        ]
  | CHECK ( logical_expression )
}

<computed_column_definition> ::=
column_name AS computed_column_expression
[ PERSISTED [ NOT NULL ] ]
[
    { PRIMARY KEY | UNIQUE }
        [ CLUSTERED | NONCLUSTERED ]
        [
            WITH ( <index_option> [ , ...n ] )
        ]
    | CHECK ( logical_expression )
]

<table_constraint> ::=
{
    { PRIMARY KEY | UNIQUE }
        [ CLUSTERED | NONCLUSTERED ]
    ( column [ ASC | DESC ] [ , ...n ] )
        [
    WITH ( <index_option> [ , ...n ] )
        ]
    | CHECK ( logical_expression )
}

<index_option> ::=
{
    IGNORE_DUP_KEY = { ON | OFF }
}

< table_index > ::=
  INDEX index_name
     [ CLUSTERED | NONCLUSTERED ] (column [ ASC | DESC ] [ , ...n ] )
     [INCLUDE (column, ...n)]
```

> User-defined memory optimized table types syntax:

```syntaxsql
CREATE TYPE [ schema_name. ] type_name
AS TABLE ( { <column_definition> [ , ...n ] }
    | [ <table_constraint> ] [ , ...n ]
    | [ <table_index> ] [ , ...n ] )
    [ WITH ( <table_option> [ , ...n ] ) ]
 [ ; ]

<column_definition> ::=
column_name <data_type>
    [ COLLATE collation_name ] [ NULL | NOT NULL ]
      [ IDENTITY [ (1 , 1) ]
    ]
    [ <column_constraint> [ , ...n ] ] [ <column_index> ]

<data type> ::=
 [ type_schema_name . ] type_name [ ( precision [ , scale ] ) ]

<column_constraint> ::=
{ PRIMARY KEY { NONCLUSTERED HASH WITH ( BUCKET_COUNT = bucket_count )
                | NONCLUSTERED }
}

< table_constraint > ::=
{ PRIMARY KEY { NONCLUSTERED HASH (column [ , ...n ] )
                   WITH ( BUCKET_COUNT = bucket_count )
               | NONCLUSTERED ( column [ ASC | DESC ] [ , ...n ] )
           }
}

<column_index> ::=
  INDEX index_name
{ [ NONCLUSTERED ] HASH (column [ , ...n ] ) WITH ( BUCKET_COUNT = bucket_count )
      | NONCLUSTERED ( column [ ASC | DESC ] [ , ...n ] )
}

< table_index > ::=
  INDEX index_name
{ [ NONCLUSTERED ] HASH (column [ , ...n ] ) WITH ( BUCKET_COUNT = bucket_count )
    | [ NONCLUSTERED ] ( column [ ASC | DESC ] [ , ...n ] )
}

<table_option> ::=
{
    [ MEMORY_OPTIMIZED = { ON | OFF } ]
}
```

## CREATE USER (Transact-SQL)

`docs/t-sql/statements/create-user-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, Azure SQL Managed Instance

```syntaxsql
-- Syntax Users based on logins in master
CREATE USER user_name
    [
        { FOR | FROM } LOGIN login_name
    ]
    [ WITH <limited_options_list> [ ,... ] ]
[ ; ]

-- Users that authenticate at the database
CREATE USER
    {
      windows_principal [ WITH <options_list> [ ,... ] ]

    | user_name WITH PASSWORD = 'password' [ , <options_list> [ ,... ]
    | Microsoft_Entra_principal FROM EXTERNAL PROVIDER [WITH OBJECT_ID = 'objectid']
    }

 [ ; ]

-- Users based on Windows principals that connect through Windows group logins
CREATE USER
    {
          windows_principal [ { FOR | FROM } LOGIN windows_principal ]
        | user_name { FOR | FROM } LOGIN windows_principal
}
    [ WITH <limited_options_list> [ ,... ] ]
[ ; ]

-- Users that cannot authenticate
CREATE USER user_name
    {
         WITHOUT LOGIN [ WITH <limited_options_list> [ ,... ] ]
       | { FOR | FROM } CERTIFICATE cert_name
       | { FOR | FROM } ASYMMETRIC KEY asym_key_name
    }
 [ ; ]

<options_list> ::=
      DEFAULT_SCHEMA = schema_name
    | DEFAULT_LANGUAGE = { NONE | lcid | language name | language alias }
    | SID = sid
    | ALLOW_ENCRYPTED_VALUE_MODIFICATIONS = [ ON | OFF ] ]

<limited_options_list> ::=
      DEFAULT_SCHEMA = schema_name ]
    | ALLOW_ENCRYPTED_VALUE_MODIFICATIONS = [ ON | OFF ] ]

-- SQL Database syntax when connected to a federation member
CREATE USER user_name
[;]

-- Syntax for users based on Microsoft Entra logins for Azure SQL Managed Instance
CREATE USER user_name
    [   { FOR | FROM } LOGIN login_name  ]
    | FROM EXTERNAL PROVIDER
    [ WITH <limited_options_list> [ ,... ] ]
[ ; ]


<limited_options_list> ::=
      DEFAULT_SCHEMA = schema_name
    | DEFAULT_LANGUAGE = { NONE | lcid | language name | language alias }
    | ALLOW_ENCRYPTED_VALUE_MODIFICATIONS = [ ON | OFF ] ]
```

> Syntax for Azure Synapse Analytics

```syntaxsql
CREATE USER user_name
    [ { { FOR | FROM } { LOGIN login_name }
      | WITHOUT LOGIN
    ]
    [ WITH DEFAULT_SCHEMA = schema_name ]
[;]

CREATE USER Microsoft_Entra_principal FROM EXTERNAL PROVIDER
    [ WITH DEFAULT_SCHEMA = schema_name ]
[;]
```

> Syntax for SQL database in Microsoft Fabric and Azure SQL Database

```syntaxsql
CREATE USER
    {
    Microsoft_Entra_principal FROM EXTERNAL PROVIDER [ WITH <limited_options_list> [ ,... ] ]
    | Microsoft_Entra_principal WITH <options_list> [ ,... ]
    }
 [ ; ]

-- Users that cannot authenticate
CREATE USER user_name
    {    WITHOUT LOGIN [ WITH DEFAULT_SCHEMA = schema_name ]
       | { FOR | FROM } CERTIFICATE cert_name
       | { FOR | FROM } ASYMMETRIC KEY asym_key_name
    }
 [ ; ]

<limited_options_list> ::=
      DEFAULT_SCHEMA = schema_name
    | OBJECT_ID = 'objectid'

<options_list> ::=
      DEFAULT_SCHEMA = schema_name
    | SID = sid
    | TYPE = { X | E }
```

> Syntax for Parallel Data Warehouse

```syntaxsql
CREATE USER user_name
    [ { { FOR | FROM }
      {
        LOGIN login_name
      }
      | WITHOUT LOGIN
    ]
    [ WITH DEFAULT_SCHEMA = schema_name ]
[;]
```

## CREATE VECTOR INDEX (Transact-SQL)

`docs/t-sql/statements/create-vector-index-transact-sql.md`

### Syntax

```syntaxsql
CREATE VECTOR INDEX index_name
ON object ( vector_column )
[ WITH (
    [ , ] METRIC = { 'cosine' | 'dot' | 'euclidean' }
    [ [ , ] TYPE = 'DiskANN' ]
    [ [ , ] MAXDOP = max_degree_of_parallelism ]
) ]
[ ON { filegroup_name | "default" } ]
[;]
```

## CREATE VIEW (Transact-SQL)

`docs/t-sql/statements/create-view-transact-sql.md`

### Syntax

> Syntax for SQL Server and Azure SQL Database.

```syntaxsql
CREATE [ OR ALTER ] VIEW [ schema_name . ] view_name [ (column [ ,...n ] ) ]
[ WITH <view_attribute> [ ,...n ] ]
AS select_statement
[ WITH CHECK OPTION ]
[ ; ]

<view_attribute> ::=
{
    [ ENCRYPTION ]
    [ SCHEMABINDING ]
    [ VIEW_METADATA ]
}
```

> Syntax for Azure Synapse Analytics and Parallel Data Warehouse.

```syntaxsql
CREATE VIEW [ schema_name . ] view_name [  ( column_name [ ,...n ] ) ]
AS <select_statement>
[;]

<select_statement> ::=
    [ WITH <common_table_expression> [ ,...n ] ]
    SELECT <select_criteria>
```

> Syntax for Microsoft Fabric Data Warehouse and SQL analytics endpoint.

```syntaxsql
CREATE [ OR ALTER ] VIEW [ schema_name . ] view_name [  ( column_name [ ,...n ] ) ]
[ WITH <view_attribute> [ ,...n ] ] AS <select_statement>
[;]

<view_attribute> ::=
{
    [ SCHEMABINDING ]
}

<select_statement> ::=
    [ WITH <common_table_expression> [ ,...n ] ]
    SELECT <select_criteria>
```

### Partitioned views

> Generally, a view is said to be a partitioned view if it is of the following form:

```syntaxsql
SELECT <select_list1>
FROM T1
UNION ALL
SELECT <select_list2>
FROM T2
UNION ALL
...
SELECT <select_listn>
FROM Tn;
```

### Conditions for creating partitioned views

> Constraint `C1` defined on table `T1` must be of the following form:

```syntaxsql
C1 ::= < simple_interval > [ OR < simple_interval > OR ...]
< simple_interval > :: =
< col > { < | > | \<= | >= | = < value >}
| < col > BETWEEN < value1 > AND < value2 >
| < col > IN ( value_list )
| < col > { > | >= } < value1 > AND
< col > { < | <= } < value2 >
```

> The following examples show valid sets of constraints:

```syntaxsql
{ [col < 10], [col between 11 and 20] , [col > 20] }
{ [col between 11 and 20], [col between 21 and 30], [col between 31 and 100] }
```

## CREATE WORKLOAD Classifier (Transact-SQL)

`docs/t-sql/statements/create-workload-classifier-transact-sql.md`

### Syntax

```syntaxsql
CREATE WORKLOAD CLASSIFIER classifier_name
WITH
    ( WORKLOAD_GROUP = 'name'
    , MEMBERNAME = 'security_account'
    [ [ , ] WLM_LABEL = 'label' ]
    [ [ , ] WLM_CONTEXT = 'context' ]
    [ [ , ] START_TIME = 'HH:MM' ]
    [ [ , ] END_TIME = 'HH:MM' ]
    [ [ , ] IMPORTANCE = { LOW | BELOW_NORMAL | NORMAL | ABOVE_NORMAL | HIGH } ] )
[ ; ]
```

## CREATE WORKLOAD GROUP (Transact-SQL)

`docs/t-sql/statements/create-workload-group-transact-sql.md`

### Syntax

```syntaxsql
CREATE WORKLOAD GROUP group_name
[ WITH
    ( [ IMPORTANCE = { LOW | MEDIUM | HIGH } ]
      [ [ , ] REQUEST_MAX_MEMORY_GRANT_PERCENT = value ]
      [ [ , ] REQUEST_MAX_CPU_TIME_SEC = value ]
      [ [ , ] REQUEST_MEMORY_GRANT_TIMEOUT_SEC = value ]
      [ [ , ] MAX_DOP = value ]
      [ [ , ] GROUP_MAX_REQUESTS = value ]
      [ [ , ] GROUP_MAX_TEMPDB_DATA_MB = value ]
      [ [ , ] GROUP_MAX_TEMPDB_DATA_PERCENT = value ] )
]
[ USING {
    [ pool_name | [default] ]
    [ [ , ] EXTERNAL external_pool_name | [ default ] ]
    } ]
[ ; ]
```

```syntaxsql
CREATE WORKLOAD GROUP group_name
[ WITH
    ( [ IMPORTANCE = { LOW | MEDIUM | HIGH } ]
      [ [ , ] REQUEST_MAX_MEMORY_GRANT_PERCENT = value ]
      [ [ , ] REQUEST_MAX_CPU_TIME_SEC = value ]
      [ [ , ] REQUEST_MEMORY_GRANT_TIMEOUT_SEC = value ]
      [ [ , ] MAX_DOP = value ]
      [ [ , ] GROUP_MAX_REQUESTS = value ]
      [ [ , ] GROUP_MAX_TEMPDB_DATA_MB = value ]
      [ [ , ] GROUP_MAX_TEMPDB_DATA_PERCENT = value ] )
]
[ USING {
    [ pool_name | [default] ]
    [ [ , ] EXTERNAL external_pool_name | [ default ] ]
    } ]
[ ; ]
```

### Azure Synapse Analytics

> Creates a workload group. Workload groups are containers for a set of requests and are the basis for how workload management is configured on a system. Workload groups provide the ability to reserve resources for workload isolation, contain resources, define resources per request, and adhere to execution rules. Once the statement completes, the settings are in effect.

```syntaxsql
CREATE WORKLOAD GROUP group_name
 WITH
 (   MIN_PERCENTAGE_RESOURCE = value
   , CAP_PERCENTAGE_RESOURCE = value
   , REQUEST_MIN_RESOURCE_GRANT_PERCENT = value
  [ [ , ] REQUEST_MAX_RESOURCE_GRANT_PERCENT = value ]
  [ [ , ] IMPORTANCE = { LOW | BELOW_NORMAL | NORMAL | ABOVE_NORMAL | HIGH } ]
  [ [ , ] QUERY_EXECUTION_TIMEOUT_SEC = value ] )
  [ ; ]
```

## CREATE XML INDEX (Selective XML Indexes)

`docs/t-sql/statements/create-xml-index-selective-xml-indexes.md`

### Syntax

```syntaxsql
CREATE XML INDEX index_name
    ON <table_object> ( xml_column_name )
    USING XML INDEX sxi_index_name
    FOR ( <xquery_or_sql_values_path> )
    [WITH ( <index_options> )]

<table_object> ::=
{ database_name.schema_name.table_name | schema_name.table_name | table_name }

<xquery_or_sql_values_path>::=
<path_name>

<path_name> ::=
character string literal

<xmlnamespace_list> ::=
<xmlnamespace_item> [, <xmlnamespace_list>]

<xmlnamespace_item> ::=
xmlnamespace_uri AS xmlnamespace_prefix

<index_options> ::=
(
  | PAD_INDEX  = { ON | OFF }
  | FILLFACTOR = fillfactor
  | SORT_IN_TEMPDB = { ON | OFF }
  | IGNORE_DUP_KEY = OFF
  | DROP_EXISTING = { ON | OFF }
  | ONLINE = OFF
  | ALLOW_ROW_LOCKS = { ON | OFF }
  | ALLOW_PAGE_LOCKS = { ON | OFF }
  | MAXDOP = max_degree_of_parallelism
)
```

## CREATE XML INDEX (Transact-SQL)

`docs/t-sql/statements/create-xml-index-transact-sql.md`

### Syntax

```syntaxsql
--Create XML Index
CREATE [ PRIMARY ] XML INDEX index_name
    ON <object> ( xml_column_name )
    [ USING XML INDEX xml_index_name
        [ FOR { VALUE | PATH | PROPERTY } ] ]
    [ WITH ( <xml_index_option> [ ,...n ] ) ]
[ ; ]

<object> ::=
{ database_name.schema_name.table_name | schema_name.table_name | table_name }

<xml_index_option> ::=
{
    PAD_INDEX  = { ON | OFF }
  | FILLFACTOR = fillfactor
  | SORT_IN_TEMPDB = { ON | OFF }
  | IGNORE_DUP_KEY = OFF
  | DROP_EXISTING = { ON | OFF }
  | ONLINE = OFF
  | ALLOW_ROW_LOCKS = { ON | OFF }
  | ALLOW_PAGE_LOCKS = { ON | OFF }
  | MAXDOP = max_degree_of_parallelism
  | XML_COMPRESSION = { ON | OFF }
}
```

## CREATE XML SCHEMA COLLECTION (Transact-SQL)

`docs/t-sql/statements/create-xml-schema-collection-transact-sql.md`

### Syntax

```syntaxsql
CREATE XML SCHEMA COLLECTION [ <relational_schema>. ] sql_identifier AS Expression
```

## DELETE (Transact-SQL)

`docs/t-sql/statements/delete-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server and Azure SQL Database

[ WITH <common_table_expression> [ ,...n ] ]
DELETE
    [ TOP ( expression ) [ PERCENT ] ]
    [ FROM ]
    { { table_alias
      | <object>
      | rowset_function_limited
      [ WITH ( table_hint_limited [ ...n ] ) ] }
      | @table_variable
    }
    [ <OUTPUT Clause> ]
    [ FROM table_source [ ,...n ] ]
    [ WHERE { <search_condition>
            | { [ CURRENT OF
                   { { [ GLOBAL ] cursor_name }
                       | cursor_variable_name
                   }
                ]
              }
            }
    ]
    [ OPTION ( <Query Hint> [ ,...n ] ) ]
[; ]

<object> ::=
{
    [ server_name.database_name.schema_name.
      | database_name. [ schema_name ] .
      | schema_name.
    ]
    table_or_view_name
}
```

```syntaxsql
-- Syntax for Azure Synapse Analytics and Microsoft Fabric

[ WITH <common_table_expression> [ ,...n ] ]
DELETE [database_name . [ schema ] . | schema. ] table_name
FROM [database_name . [ schema ] . | schema. ] table_name
JOIN {<join_table_source>}[ ,...n ]
ON <join_condition>
[ WHERE <search_condition> ]
[ OPTION ( <query_options> [ ,...n ]  ) ]
[; ]

<join_table_source> ::=
{
    [ database_name . [ schema_name ] . | schema_name . ] table_or_view_name [ AS ] table_or_view_alias
    [ <tablesample_clause>]
    | derived_table [ AS ] table_alias [ ( column_alias [ ,...n ] ) ]
}
```

```syntaxsql
-- Syntax for Parallel Data Warehouse

DELETE
    [ FROM [database_name . [ schema ] . | schema. ] table_name ]
    [ WHERE <search_condition> ]
    [ OPTION ( <query_options> [ ,...n ]  ) ]
[; ]
```

## DENY Assembly Permissions (Transact-SQL)

`docs/t-sql/statements/deny-assembly-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY { permission [ ,...n ] } ON ASSEMBLY :: assembly_name
    TO database_principal [ ,...n ]
        [ CASCADE ]
        [ AS denying_principal ]
```

## DENY Asymmetric Key Permissions (Transact-SQL)

`docs/t-sql/statements/deny-asymmetric-key-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY { permission  [ ,...n ] }
    ON ASYMMETRIC KEY :: asymmetric_key_name
        TO database_principal [ ,...n ]
    [ CASCADE ]
        [ AS denying_principal ]
```

## DENY Availability Group Permissions

`docs/t-sql/statements/deny-availability-group-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY permission  [ ,...n ] ON AVAILABILITY GROUP :: availability_group_name
        TO < server_principal >  [ ,...n ]
    [ CASCADE ]
    [ AS SQL_Server_login ]

<server_principal> ::=
        SQL_Server_login
    | SQL_Server_login_from_Windows_login
    | SQL_Server_login_from_certificate
    | SQL_Server_login_from_AsymKey
```

## DENY Certificate Permissions (Transact-SQL)

`docs/t-sql/statements/deny-certificate-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY permission  [ ,...n ]
    ON CERTIFICATE :: certificate_name
    TO principal [ ,...n ]
    [ CASCADE ]
    [ AS denying_principal ]
```

## DENY Database Permissions (Transact-SQL)

`docs/t-sql/statements/deny-database-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY <permission> [ ,...n ]
    TO <database_principal> [ ,...n ] [ CASCADE ]
    [ AS <database_principal> ]

<permission> ::=
    permission | ALL [ PRIVILEGES ]

<database_principal> ::=
    Database_user
  | Database_role
  | Application_role
  | Database_user_mapped_to_Windows_User
  | Database_user_mapped_to_Windows_Group
  | Database_user_mapped_to_certificate
  | Database_user_mapped_to_asymmetric_key
  | Database_user_with_no_login
```

## DENY Database Principal Permissions

`docs/t-sql/statements/deny-database-principal-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY permission [ ,...n ]
    ON
    {  [ USER :: database_user ]
     | [ ROLE :: database_role ]
     | [ APPLICATION ROLE :: application_role ]
    }
    TO <database_principal> [ ,...n ]
      [ CASCADE ]
      [ AS <database_principal> ]

<database_principal> ::=
    Database_user
  | Database_role
  | Application_role
  | Database_user_mapped_to_Windows_User
  | Database_user_mapped_to_Windows_Group
  | Database_user_mapped_to_certificate
  | Database_user_mapped_to_asymmetric_key
  | Database_user_with_no_login
```

## DENY Database Scoped Credential (Transact-SQL)

`docs/t-sql/statements/deny-database-scoped-credential-transact-sql.md`

### Syntax

```syntaxsql
DENY permission  [ ,...n ]
    ON DATABASE SCOPED CREDENTIAL :: credential_name
    TO principal [ ,...n ]
    [ CASCADE ]
    [ AS denying_principal ]
```

## DENY Endpoint Permissions (Transact-SQL)

`docs/t-sql/statements/deny-endpoint-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY permission  [ ,...n ] ON ENDPOINT :: endpoint_name
    TO < server_principal >  [ ,...n ]
    [ CASCADE ]
    [ AS SQL_Server_login ]

<server_principal> ::=
        SQL_Server_login
    | SQL_Server_login_from_Windows_login
    | SQL_Server_login_from_certificate
    | SQL_Server_login_from_AsymKey
```

## DENY Full-Text Permissions (Transact-SQL)

`docs/t-sql/statements/deny-full-text-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY permission [ ,...n ] ON
    FULLTEXT
        {
           CATALOG :: full-text_catalog_name
           |
           STOPLIST :: full-text_stoplist_name
        }
    TO database_principal [ ,...n ] [ CASCADE ]
        [ AS denying_principal ]
```

## DENY Object Permissions (Transact-SQL)

`docs/t-sql/statements/deny-object-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY <permission> [ ,...n ] ON
    [ OBJECT :: ][ schema_name ]. object_name [ ( column [ ,...n ] ) ]
        TO <database_principal> [ ,...n ]
    [ CASCADE ]
        [ AS <database_principal> ]

<permission> ::=
    ALL [ PRIVILEGES ] | permission [ ( column [ ,...n ] ) ]

<database_principal> ::=
        Database_user
    | Database_role
    | Application_role
    | Database_user_mapped_to_Windows_User
    | Database_user_mapped_to_Windows_Group
    | Database_user_mapped_to_certificate
    | Database_user_mapped_to_asymmetric_key
    | Database_user_with_no_login
```

## DENY Schema Permissions (Transact-SQL)

`docs/t-sql/statements/deny-schema-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY permission  [ ,...n ] } ON SCHEMA :: schema_name
    TO database_principal [ ,...n ]
    [ CASCADE ]
        [ AS denying_principal ]
```

## DENY Search Property List Permissions

`docs/t-sql/statements/deny-search-property-list-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY permission [ ,...n ] ON
        SEARCH PROPERTY LIST :: search_property_list_name
    TO database_principal [ ,...n ] [ CASCADE ]
    [ AS denying_principal ]
```

## DENY Server Permissions (Transact-SQL)

`docs/t-sql/statements/deny-server-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY permission [ ,...n ]
    TO <grantee_principal> [ ,...n ]
    [ CASCADE ]
    [ AS <grantor_principal> ]

<grantee_principal> ::= SQL_Server_login
    | SQL_Server_login_mapped_to_Windows_login
    | SQL_Server_login_mapped_to_Windows_group
    | SQL_Server_login_mapped_to_certificate
    | SQL_Server_login_mapped_to_asymmetric_key
    | server_role

<grantor_principal> ::= SQL_Server_login
    | SQL_Server_login_mapped_to_Windows_login
    | SQL_Server_login_mapped_to_Windows_group
    | SQL_Server_login_mapped_to_certificate
    | SQL_Server_login_mapped_to_asymmetric_key
    | server_role
```

## DENY Server Principal Permissions (Transact-SQL)

`docs/t-sql/statements/deny-server-principal-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY permission [ ,...n ] }
    ON
    { [ LOGIN :: SQL_Server_login ]
      | [ SERVER ROLE :: server_role ] }
    TO <server_principal> [ ,...n ]
    [ CASCADE ]
    [ AS SQL_Server_login ]

<server_principal> ::=
    SQL_Server_login
    | SQL_Server_login_from_Windows_login
    | SQL_Server_login_from_certificate
    | SQL_Server_login_from_AsymKey
    | server_role
```

## DENY Service Broker Permissions (Transact-SQL)

`docs/t-sql/statements/deny-service-broker-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY permission  [ ,...n ] ON
    {
       [ CONTRACT :: contract_name ]
       | [ MESSAGE TYPE :: message_type_name ]
       | [ REMOTE SERVICE BINDING :: remote_binding_name ]
       | [ ROUTE :: route_name ]
       | [ SERVICE :: service_name ]
        }
    TO database_principal [ ,...n ]
    [ CASCADE ]
        [ AS denying_principal ]
```

## DENY Symmetric Key Permissions (Transact-SQL)

`docs/t-sql/statements/deny-symmetric-key-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY permission [ ,...n ]
    ON SYMMETRIC KEY :: symmetric_key_name
        TO <database_principal> [ ,...n ] [ CASCADE ]
    [ AS <database_principal> ]

<database_principal> ::=
        Database_user
    | Database_role
    | Application_role
    | Database_user_mapped_to_Windows_User
    | Database_user_mapped_to_Windows_Group
    | Database_user_mapped_to_certificate
    | Database_user_mapped_to_asymmetric_key
    | Database_user_with_no_login
```

## DENY System Object Permissions (Transact-SQL)

`docs/t-sql/statements/deny-system-object-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY { SELECT | EXECUTE } ON [ sys. ] system_object TO principal
```

## DENY (Transact-SQL)

`docs/t-sql/statements/deny-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, and Fabric SQL database

```syntaxsql
-- Simplified syntax for DENY
DENY   { ALL [ PRIVILEGES ] }
     | <permission>  [ ( column [ ,...n ] ) ] [ ,...n ]
    [ ON [ <class> :: ] securable ]
    TO principal [ ,...n ]
    [ CASCADE] [ AS principal ]
[;]

<permission> ::=
{ see the tables below }

<class> ::=
{ see the tables below }
```

> Syntax for Azure Synapse Analytics and Parallel Data Warehouse and Microsoft Fabric warehouse

```syntaxsql
DENY
    <permission> [ ,...n ]
    [ ON [ <class_> :: ] securable ]
    TO principal [ ,...n ]
    [ CASCADE ]
[;]

<permission> ::=
{ see the tables below }

<class> ::=
{
      LOGIN
    | DATABASE
    | OBJECT
    | ROLE
    | SCHEMA
    | USER
}
```

## DENY Type Permissions (Transact-SQL)

`docs/t-sql/statements/deny-type-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY permission  [ ,...n ] ON TYPE :: [ schema_name . ] type_name
        TO <database_principal> [ ,...n ]
    [ CASCADE ]
    [ AS <database_principal> ]

<database_principal> ::=
        Database_user
    | Database_role
    | Application_role
    | Database_user_mapped_to_Windows_User
    | Database_user_mapped_to_Windows_Group
    | Database_user_mapped_to_certificate
    | Database_user_mapped_to_asymmetric_key
    | Database_user_with_no_login
```

## DENY XML Schema Collection Permissions

`docs/t-sql/statements/deny-xml-schema-collection-permissions-transact-sql.md`

### Syntax

```syntaxsql
DENY permission  [ ,...n ] ON
    XML SCHEMA COLLECTION :: [ schema_name . ]
    XML_schema_collection_name
    TO <database_principal> [ ,...n ]
        [ CASCADE ]
    [ AS <database_principal> ]

<database_principal> ::=
        Database_user
    | Database_role
    | Application_role
    | Database_user_mapped_to_Windows_User
    | Database_user_mapped_to_Windows_Group
    | Database_user_mapped_to_certificate
    | Database_user_mapped_to_asymmetric_key
    | Database_user_with_no_login
```

## DISABLE TRIGGER (Transact-SQL)

`docs/t-sql/statements/disable-trigger-transact-sql.md`

### Syntax

```syntaxsql
DISABLE TRIGGER { [ schema_name . ] trigger_name [ ,...n ] | ALL }
ON { object_name | DATABASE | ALL SERVER } [ ; ]
```

## DROP AGGREGATE (Transact-SQL)

`docs/t-sql/statements/drop-aggregate-transact-sql.md`

### Syntax

```syntaxsql
DROP AGGREGATE [ IF EXISTS ] [ schema_name . ] aggregate_name
```

## DROP APPLICATION ROLE (Transact-SQL)

`docs/t-sql/statements/drop-application-role-transact-sql.md`

### Syntax

```syntaxsql
DROP APPLICATION ROLE rolename
```

## DROP ASSEMBLY (Transact-SQL)

`docs/t-sql/statements/drop-assembly-transact-sql.md`

### Syntax

```syntaxsql
DROP ASSEMBLY [ IF EXISTS ] assembly_name [ , ...n ]
[ WITH NO DEPENDENTS ]
[ ; ]
```

## DROP ASYMMETRIC KEY (Transact-SQL)

`docs/t-sql/statements/drop-asymmetric-key-transact-sql.md`

### Syntax

```syntaxsql
DROP ASYMMETRIC KEY key_name [ REMOVE PROVIDER KEY ]
```

## DROP AVAILABILITY GROUP (Transact-SQL)

`docs/t-sql/statements/drop-availability-group-transact-sql.md`

### Syntax

```syntaxsql
DROP AVAILABILITY GROUP group_name
[ ; ]
```

## DROP BROKER PRIORITY (Transact-SQL)

`docs/t-sql/statements/drop-broker-priority-transact-sql.md`

### Syntax

```syntaxsql
DROP BROKER PRIORITY ConversationPriorityName
[;]
```

## DROP CERTIFICATE (Transact-SQL)

`docs/t-sql/statements/drop-certificate-transact-sql.md`

### Syntax

```syntaxsql
DROP CERTIFICATE certificate_name
```

## DROP COLUMN ENCRYPTION KEY (Transact-SQL)

`docs/t-sql/statements/drop-column-encryption-key-transact-sql.md`

### Syntax

```syntaxsql
DROP COLUMN ENCRYPTION KEY key_name [;]
```

## DROP COLUMN MASTER KEY (Transact-SQL)

`docs/t-sql/statements/drop-column-master-key-transact-sql.md`

### Syntax

```syntaxsql
DROP COLUMN MASTER KEY key_name;
```

## DROP CONTRACT (Transact-SQL)

`docs/t-sql/statements/drop-contract-transact-sql.md`

### Syntax

```syntaxsql
DROP CONTRACT contract_name
[ ; ]
```

## DROP CREDENTIAL (Transact-SQL)

`docs/t-sql/statements/drop-credential-transact-sql.md`

### Syntax

```syntaxsql
DROP CREDENTIAL credential_name
```

## DROP CRYPTOGRAPHIC PROVIDER (Transact-SQL)

`docs/t-sql/statements/drop-cryptographic-provider-transact-sql.md`

### Syntax

```syntaxsql
DROP CRYPTOGRAPHIC PROVIDER provider_name
```

## DROP DATABASE AUDIT SPECIFICATION (Transact-SQL)

`docs/t-sql/statements/drop-database-audit-specification-transact-sql.md`

### Syntax

```syntaxsql
DROP DATABASE AUDIT SPECIFICATION audit_specification_name
[ ; ]
```

## DROP DATABASE ENCRYPTION KEY (Transact-SQL)

`docs/t-sql/statements/drop-database-encryption-key-transact-sql.md`

### Syntax

```syntaxsql
DROP DATABASE ENCRYPTION KEY
```

## DROP DATABASE SCOPED CREDENTIAL (Transact-SQL)

`docs/t-sql/statements/drop-database-scoped-credential-transact-sql.md`

### Syntax

```syntaxsql
DROP DATABASE SCOPED CREDENTIAL credential_name
```

## DROP DATABASE (Transact-SQL)

`docs/t-sql/statements/drop-database-transact-sql.md`

### Syntax

> SQL Server syntax.

```syntaxsql
DROP DATABASE [ IF EXISTS ] { database_name | database_snapshot_name } [ ,...n ]
[ ; ]
```

> Azure SQL Database, Azure Synapse Analytics, and Analytics Platform System syntax.

```syntaxsql
DROP DATABASE database_name
[ ; ]
```

## DROP DEFAULT (Transact-SQL)

`docs/t-sql/statements/drop-default-transact-sql.md`

### Syntax

```syntaxsql
DROP DEFAULT [ IF EXISTS ] { [ schema_name . ] default_name } [ ,...n ] [ ; ]
```

## DROP ENDPOINT (Transact-SQL)

`docs/t-sql/statements/drop-endpoint-transact-sql.md`

### Syntax

```syntaxsql
DROP ENDPOINT endPointName
```

## DROP EVENT NOTIFICATION (Transact-SQL)

`docs/t-sql/statements/drop-event-notification-transact-sql.md`

### Syntax

```syntaxsql
DROP EVENT NOTIFICATION notification_name [ ,...n ]
ON { SERVER | DATABASE | QUEUE queue_name }
[ ; ]
```

## DROP EVENT SESSION (Transact-SQL)

`docs/t-sql/statements/drop-event-session-transact-sql.md`

### Syntax

```syntaxsql
DROP EVENT SESSION event_session_name
ON { SERVER | DATABASE }
```

## DROP EXTERNAL DATA SOURCE (Transact-SQL)

`docs/t-sql/statements/drop-external-data-source-transact-sql.md`

### Syntax

```syntaxsql
-- Drop an external data source
DROP EXTERNAL DATA SOURCE external_data_source_name
[;]
```

## DROP EXTERNAL FILE FORMAT (Transact-SQL)

`docs/t-sql/statements/drop-external-file-format-transact-sql.md`

### Syntax

```syntaxsql
-- Drop an external file format
DROP EXTERNAL FILE FORMAT external_file_format_name
[;]
```

## DROP EXTERNAL LANGUAGE (Transact-SQL) - SQL Server

`docs/t-sql/statements/drop-external-language-transact-sql.md`

### Syntax

```syntaxsql
DROP EXTERNAL LANGUAGE <language_name>
```

## DROP EXTERNAL LIBRARY (Transact-SQL)

`docs/t-sql/statements/drop-external-library-transact-sql.md`

### Syntax

```syntaxsql
DROP EXTERNAL LIBRARY library_name
[ AUTHORIZATION owner_name ];
```

## DROP EXTERNAL MODEL (Transact-SQL)

`docs/t-sql/statements/drop-external-model-transact-sql.md`

### Syntax

```syntaxsql
DROP EXTERNAL MODEL external_model_object_name
[ ; ]
```

## DROP EXTERNAL RESOURCE POOL (Transact-SQL)

`docs/t-sql/statements/drop-external-resource-pool-transact-sql.md`

### Syntax

```syntaxsql
DROP EXTERNAL RESOURCE POOL pool_name
```

## DROP EXTERNAL TABLE (Transact-SQL)

`docs/t-sql/statements/drop-external-table-transact-sql.md`

### Syntax

```syntaxsql
DROP EXTERNAL TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
[;]
```

## DROP FULLTEXT CATALOG (Transact-SQL)

`docs/t-sql/statements/drop-fulltext-catalog-transact-sql.md`

### Syntax

```syntaxsql
DROP FULLTEXT CATALOG catalog_name
```

## DROP FULLTEXT INDEX (Transact-SQL)

`docs/t-sql/statements/drop-fulltext-index-transact-sql.md`

### Syntax

```syntaxsql
DROP FULLTEXT INDEX ON table_name
```

## DROP FULLTEXT STOPLIST (Transact-SQL)

`docs/t-sql/statements/drop-fulltext-stoplist-transact-sql.md`

### Syntax

```syntaxsql
DROP FULLTEXT STOPLIST stoplist_name
;
```

## DROP FUNCTION (Transact-SQL)

`docs/t-sql/statements/drop-function-transact-sql.md`

### Syntax

```syntaxsql
 -- SQL Server, Azure SQL Database

DROP FUNCTION [ IF EXISTS ] { [ schema_name. ] function_name } [ ,...n ]
[;]
```

```syntaxsql
 -- Azure Synapse Analytics, Parallel Data Warehouse, Microsoft Fabric

DROP FUNCTION [IF EXISTS] [ schema_name. ] function_name
[;]
```

## DROP INDEX (Selective XML Indexes)

`docs/t-sql/statements/drop-index-selective-xml-indexes.md`

### Syntax

```syntaxsql
DROP INDEX index_name ON <object>
    [ WITH ( <drop_index_option> [ ,...n ] ) ]

<object> ::=
{ database_name.schema_name.table_or_view_name | schema_name.table_or_view_name | table_or_view_name }

<drop_index_option> ::=
{
    MAXDOP = max_degree_of_parallelism
    | ONLINE = { ON | OFF }
}
```

## DROP INDEX (Transact-SQL)

`docs/t-sql/statements/drop-index-transact-sql.md`

### Syntax

> Syntax for SQL Server (all options except filegroup and filestream apply to Azure SQL Database).

```syntaxsql
DROP INDEX [ IF EXISTS ]
{ <drop_relational_or_xml_or_spatial_index> [ , ...n ]
| <drop_backward_compatible_index> [ , ...n ]
}

<drop_relational_or_xml_or_spatial_index> ::=
    index_name ON <object>
    [ WITH ( <drop_clustered_index_option> [ , ...n ] ) ]

<drop_backward_compatible_index> ::=
    [ owner_name. ] table_or_view_name.index_name

<object> ::=
{ database_name.schema_name.table_or_view_name | schema_name.table_or_view_name | table_or_view_name }

<drop_clustered_index_option> ::=
{
    MAXDOP = max_degree_of_parallelism
  | ONLINE = { ON | OFF }
  | MOVE TO { partition_scheme_name ( column_name )
            | filegroup_name
            | "default"
            }
  [ FILESTREAM_ON { partition_scheme_name
            | filestream_filegroup_name
            | "default" } ]
}
```

> Syntax for Azure SQL Database.

```syntaxsql
DROP INDEX
{ <drop_relational_or_xml_or_spatial_index> [ , ...n ]
}

<drop_relational_or_xml_or_spatial_index> ::=
    index_name ON <object>

<object> ::=
{ database_name.schema_name.table_or_view_name | schema_name.table_or_view_name | table_or_view_name }
```

> Syntax for Azure Synapse Analytics and Analytics Platform System (PDW).

```syntaxsql
DROP INDEX index_name ON { database_name.schema_name.table_name | schema_name.table_name | table_name }
[ ; ]
```

## DROP LOGIN (Transact-SQL)

`docs/t-sql/statements/drop-login-transact-sql.md`

### Syntax

```syntaxsql
DROP LOGIN login_name
```

## DROP MASTER KEY (Transact-SQL)

`docs/t-sql/statements/drop-master-key-transact-sql.md`

### Syntax

```syntaxsql
DROP MASTER KEY
```

## DROP MESSAGE TYPE (Transact-SQL)

`docs/t-sql/statements/drop-message-type-transact-sql.md`

### Syntax

```syntaxsql
DROP MESSAGE TYPE message_type_name
[ ; ]
```

## DROP PARTITION FUNCTION (Transact-SQL)

`docs/t-sql/statements/drop-partition-function-transact-sql.md`

### Syntax

```syntaxsql
DROP PARTITION FUNCTION partition_function_name [ ; ]
```

## DROP PARTITION SCHEME (Transact-SQL)

`docs/t-sql/statements/drop-partition-scheme-transact-sql.md`

### Syntax

```syntaxsql
DROP PARTITION SCHEME partition_scheme_name
[ ; ]
```

## DROP PROCEDURE (Transact-SQL)

`docs/t-sql/statements/drop-procedure-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Managed Instance, and Azure SQL Database:

```syntaxsql
DROP { PROC | PROCEDURE } [ IF EXISTS ] { [ schema_name. ] procedure } [ , ...n ]
```

> Syntax for Azure Synapse Analytics, Analytics Platform System (PDW), and Microsoft Fabric:

```syntaxsql
DROP { PROC | PROCEDURE } { [ schema_name. ] procedure_name }
```

## DROP QUEUE (Transact-SQL)

`docs/t-sql/statements/drop-queue-transact-sql.md`

### Syntax

```syntaxsql
DROP QUEUE <object>
[ ; ]

<object> ::=
{ database_name.schema_name.queue_name | schema_name.queue_name | queue_name }
```

## DROP REMOTE SERVICE BINDING (Transact-SQL)

`docs/t-sql/statements/drop-remote-service-binding-transact-sql.md`

### Syntax

```syntaxsql
DROP REMOTE SERVICE BINDING binding_name
[ ; ]
```

## DROP RESOURCE POOL (Transact-SQL)

`docs/t-sql/statements/drop-resource-pool-transact-sql.md`

### Syntax

```syntaxsql
DROP RESOURCE POOL pool_name
[ ; ]
```

## DROP ROLE (Transact-SQL)

`docs/t-sql/statements/drop-role-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, Azure SQL Managed Instance, and Fabric SQL database

```syntaxsql
DROP ROLE [ IF EXISTS ] role_name
```

> Syntax for Azure Synapse Analytics and Parallel Data Warehouse

```syntaxsql
DROP ROLE role_name
```

## DROP ROUTE (Transact-SQL)

`docs/t-sql/statements/drop-route-transact-sql.md`

### Syntax

```syntaxsql
DROP ROUTE route_name
[ ; ]
```

## DROP RULE (Transact-SQL)

`docs/t-sql/statements/drop-rule-transact-sql.md`

### Syntax

```syntaxsql
DROP RULE [ IF EXISTS ] { [ schema_name . ] rule_name } [ , ...n ]
[ ; ]
```

## DROP SCHEMA (Transact-SQL)

`docs/t-sql/statements/drop-schema-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server and Azure SQL Database

DROP SCHEMA  [ IF EXISTS ] schema_name
```

```syntaxsql
-- Syntax for Azure Synapse Analytics and Parallel Data Warehouse

DROP SCHEMA schema_name
```

## DROP SEARCH PROPERTY LIST (Transact-SQL)

`docs/t-sql/statements/drop-search-property-list-transact-sql.md`

### Syntax

```syntaxsql
DROP SEARCH PROPERTY LIST property_list_name
;
```

## DROP SECURITY POLICY (Transact-SQL)

`docs/t-sql/statements/drop-security-policy-transact-sql.md`

### Syntax

```syntaxsql
DROP SECURITY POLICY [ IF EXISTS ] [schema_name. ] security_policy_name
[;]
```

## DROP SENSITIVITY CLASSIFICATION (Transact-SQL)

`docs/t-sql/statements/drop-sensitivity-classification-transact-sql.md`

### Syntax

```syntaxsql
DROP SENSITIVITY CLASSIFICATION FROM
    <object_name> [, ...n ]

<object_name> ::=
{
    [schema_name.]table_name.column_name
}
```

## DROP SEQUENCE (Transact-SQL)

`docs/t-sql/statements/drop-sequence-transact-sql.md`

### Syntax

```syntaxsql
DROP SEQUENCE [ IF EXISTS ] { database_name.schema_name.sequence_name | schema_name.sequence_name | sequence_name } [ ,...n ]
 [ ; ]
```

## DROP SERVER AUDIT SPECIFICATION (Transact-SQL)

`docs/t-sql/statements/drop-server-audit-specification-transact-sql.md`

### Syntax

```syntaxsql
DROP SERVER AUDIT SPECIFICATION audit_specification_name
[ ; ]
```

## DROP SERVER AUDIT (Transact-SQL)

`docs/t-sql/statements/drop-server-audit-transact-sql.md`

### Syntax

```syntaxsql
DROP SERVER AUDIT audit_name
[ ; ]
```

## DROP SERVER ROLE (Transact-SQL)

`docs/t-sql/statements/drop-server-role-transact-sql.md`

### Syntax

```syntaxsql
DROP SERVER ROLE role_name
[ ; ]
```

## DROP SERVICE (Transact-SQL)

`docs/t-sql/statements/drop-service-transact-sql.md`

### Syntax

```syntaxsql
DROP SERVICE service_name
[ ; ]
```

## DROP SIGNATURE (Transact-SQL)

`docs/t-sql/statements/drop-signature-transact-sql.md`

### Syntax

```syntaxsql
DROP [ COUNTER ] SIGNATURE FROM module_name
    BY <crypto_list> [ ,...n ]

<crypto_list> ::=
    CERTIFICATE cert_name
    | ASYMMETRIC KEY Asym_key_name
```

## DROP STATISTICS (Transact-SQL)

`docs/t-sql/statements/drop-statistics-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server and Azure SQL Database

DROP STATISTICS table.statistics_name | view.statistics_name [ ,...n ]
```

```syntaxsql
-- Syntax for Azure Synapse Analytics and Parallel Data Warehouse and Microsoft Fabric

DROP STATISTICS [ schema_name . ] table_name.statistics_name
[;]
```

## DROP SYMMETRIC KEY (Transact-SQL)

`docs/t-sql/statements/drop-symmetric-key-transact-sql.md`

### Syntax

```syntaxsql
DROP SYMMETRIC KEY symmetric_key_name [REMOVE PROVIDER KEY]
```

## DROP SYNONYM (Transact-SQL)

`docs/t-sql/statements/drop-synonym-transact-sql.md`

### Syntax

```syntaxsql
DROP SYNONYM [ IF EXISTS ] [ schema. ] synonym_name
```

## DROP TABLE (Transact-SQL)

`docs/t-sql/statements/drop-table-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server, Azure SQL Database, Warehouse in Microsoft Fabric

DROP TABLE [ IF EXISTS ] { database_name.schema_name.table_name | schema_name.table_name | table_name } [ ,...n ]
[ ; ]
```

```syntaxsql
-- Syntax for Azure Synapse Analytics and Parallel Data Warehouse

DROP TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
[;]
```

## DROP TRIGGER (Transact-SQL)

`docs/t-sql/statements/drop-trigger-transact-sql.md`

### Syntax

```syntaxsql
-- Trigger on an INSERT, UPDATE, or DELETE statement to a table or view (DML Trigger)

DROP TRIGGER [ IF EXISTS ] [schema_name.]trigger_name [ ,...n ] [ ; ]

-- Trigger on a CREATE, ALTER, DROP, GRANT, DENY, REVOKE or UPDATE statement (DDL Trigger)

DROP TRIGGER [ IF EXISTS ] trigger_name [ ,...n ]
ON { DATABASE | ALL SERVER }
[ ; ]

-- Trigger on a LOGON event (Logon Trigger)

DROP TRIGGER [ IF EXISTS ] trigger_name [ ,...n ]
ON ALL SERVER
```

## DROP TYPE (Transact-SQL)

`docs/t-sql/statements/drop-type-transact-sql.md`

### Syntax

```syntaxsql
DROP TYPE [ IF EXISTS ] [ schema_name. ] type_name [ ; ]
```

## DROP USER (Transact-SQL)

`docs/t-sql/statements/drop-user-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server and Azure SQL Database

DROP USER [ IF EXISTS ] user_name
```

```syntaxsql
-- Syntax for Azure Synapse Analytics and Parallel Data Warehouse

DROP USER user_name
```

## DROP VIEW (Transact-SQL)

`docs/t-sql/statements/drop-view-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server and Azure SQL Database

DROP VIEW [ IF EXISTS ] [ schema_name . ] view_name [ ...,n ] [ ; ]
```

```syntaxsql
-- Syntax for Azure Synapse Analytics and Microsoft Fabric

DROP VIEW [ IF EXISTS ] [ schema_name . ] view_name [ ; ]
```

```syntaxsql
-- Syntax for Parallel Data Warehouse

DROP VIEW [ schema_name . ] view_name [ ; ]
```

## DROP WORKLOAD Classifier (Transact-SQL)

`docs/t-sql/statements/drop-workload-classifier-transact-sql.md`

### Syntax

```syntaxsql
DROP WORKLOAD CLASSIFIER classifier_name;
```

## DROP WORKLOAD GROUP (Transact-SQL)

`docs/t-sql/statements/drop-workload-group-transact-sql.md`

### Syntax

```syntaxsql
DROP WORKLOAD GROUP group_name
[;]
```

```syntaxsql
DROP WORKLOAD GROUP group_name
[;]
```

```syntaxsql
DROP WORKLOAD GROUP group_name
```

## DROP XML SCHEMA COLLECTION (Transact-SQL)

`docs/t-sql/statements/drop-xml-schema-collection-transact-sql.md`

### Syntax

```syntaxsql
DROP XML SCHEMA COLLECTION [ relational_schema. ] sql_identifier
```

## ENABLE TRIGGER (Transact-SQL)

`docs/t-sql/statements/enable-trigger-transact-sql.md`

### Syntax

```syntaxsql
ENABLE TRIGGER { [ schema_name . ] trigger_name [ ,...n ] | ALL }
ON { object_name | DATABASE | ALL SERVER } [ ; ]
```

## END CONVERSATION (Transact-SQL)

`docs/t-sql/statements/end-conversation-transact-sql.md`

### Syntax

```syntaxsql
END CONVERSATION conversation_handle
   [   [ WITH ERROR = failure_code DESCRIPTION = 'failure_text' ]
     | [ WITH CLEANUP ]
    ]
[ ; ]
```

## EXECUTE AS Clause (Transact-SQL)

`docs/t-sql/statements/execute-as-clause-transact-sql.md`

### [SQL Server](#tab/sqlserver)

> Functions (except inline table-valued functions), stored procedures, and DML triggers:

```syntaxsql
{ EXEC | EXECUTE } AS { CALLER | SELF | OWNER | 'user_name' }
```

> DDL triggers with database scope:

```syntaxsql
{ EXEC | EXECUTE } AS { CALLER | SELF | 'user_name' }
```

> DDL triggers with server scope and logon triggers:

```syntaxsql
{ EXEC | EXECUTE } AS { CALLER | SELF | 'login_name' }
```

> Queues:

```syntaxsql
{ EXEC | EXECUTE } AS { SELF | OWNER | 'user_name' }
```

### [Azure SQL Database](#tab/sqldb)

> Functions (except inline table-valued functions), stored procedures, and DML triggers:

```syntaxsql
{ EXEC | EXECUTE } AS { CALLER | SELF | OWNER | 'user_name' }
```

> DDL triggers with database scope:

```syntaxsql
{ EXEC | EXECUTE } AS { CALLER | SELF | 'user_name' }
```

## EXECUTE AS (Transact-SQL)

`docs/t-sql/statements/execute-as-transact-sql.md`

### Syntax

```syntaxsql
{ EXEC | EXECUTE } AS <context_specification>
[;]

<context_specification>::=
{ LOGIN | USER } = 'name'
    [ WITH { NO REVERT | COOKIE INTO @varbinary_variable } ]
| CALLER
```

## GET CONVERSATION GROUP (Transact-SQL)

`docs/t-sql/statements/get-conversation-group-transact-sql.md`

### Syntax

```syntaxsql
[ WAITFOR ( ]
   GET CONVERSATION GROUP @conversation_group_id
      FROM <queue>
[ ) ] [ , TIMEOUT timeout ]
[ ; ]

<queue> ::=
{ database_name.schema_name.queue_name | schema_name.queue_name | queue_name }
```

## GET_TRANSMISSION_STATUS (Transact-SQL)

`docs/t-sql/statements/get-transmission-status-transact-sql.md`

### Syntax

```syntaxsql
GET_TRANSMISSION_STATUS ( conversation_handle )
```

## GRANT Assembly Permissions (Transact-SQL)

`docs/t-sql/statements/grant-assembly-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT { permission [ ,...n ] } ON ASSEMBLY :: assembly_name
    TO database_principal [ ,...n ]
    [ WITH GRANT OPTION ]
    [ AS granting_principal ]
```

## GRANT Asymmetric Key Permissions (Transact-SQL)

`docs/t-sql/statements/grant-asymmetric-key-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT { permission  [ ,...n ] }
    ON ASYMMETRIC KEY :: asymmetric_key_name
       TO database_principal [ ,...n ]
    [ WITH GRANT OPTION ]
    [ AS granting_principal ]
```

## GRANT Availability Group Permissions

`docs/t-sql/statements/grant-availability-group-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT permission  [ ,...n ] ON AVAILABILITY GROUP :: availability_group_name
        TO < server_principal >  [ ,...n ]
    [ WITH GRANT OPTION ]
    [ AS SQL_Server_login ]

<server_principal> ::=
        SQL_Server_login
    | SQL_Server_login_from_Windows_login
    | SQL_Server_login_from_certificate
    | SQL_Server_login_from_AsymKey
```

## GRANT Certificate Permissions (Transact-SQL)

`docs/t-sql/statements/grant-certificate-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT permission  [ ,...n ]
    ON CERTIFICATE :: certificate_name
    TO principal [ ,...n ] [ WITH GRANT OPTION ]
    [ AS granting_principal ]
```

## GRANT Database Permissions (Transact-SQL)

`docs/t-sql/statements/grant-database-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT <permission> [ ,...n ]
    TO <database_principal> [ ,...n ] [ WITH GRANT OPTION ]
    [ AS <database_principal> ]

<permission>::=
permission | ALL [ PRIVILEGES ]

<database_principal> ::=
    Database_user
  | Database_role
  | Application_role
  | Database_user_mapped_to_Windows_User
  | Database_user_mapped_to_Windows_Group
  | Database_user_mapped_to_certificate
  | Database_user_mapped_to_asymmetric_key
  | Database_user_with_no_login
```

## GRANT Database Principal Permissions

`docs/t-sql/statements/grant-database-principal-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT permission [ ,...n ]
    ON
    {  [ USER :: database_user ]
     | [ ROLE :: database_role ]
     | [ APPLICATION ROLE :: application_role ]
    }
    TO <database_principal> [ ,...n ]
       [ WITH GRANT OPTION ]
       [ AS <database_principal> ]

<database_principal> ::=
    Database_user
  | Database_role
  | Application_role
  | Database_user_mapped_to_Windows_User
  | Database_user_mapped_to_Windows_Group
  | Database_user_mapped_to_certificate
  | Database_user_mapped_to_asymmetric_key
  | Database_user_with_no_login
```

## GRANT Database Scoped Credential (Transact-SQL)

`docs/t-sql/statements/grant-database-scoped-credential-transact-sql.md`

### Syntax

```syntaxsql
GRANT permission  [ ,...n ]
    ON DATABASE SCOPED CREDENTIAL :: credential_name
    TO principal [ ,...n ] [ WITH GRANT OPTION ]
    [ AS granting_principal ]
```

## GRANT Endpoint Permissions (Transact-SQL)

`docs/t-sql/statements/grant-endpoint-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT permission  [ ,...n ] ON ENDPOINT :: endpoint_name
        TO < server_principal >  [ ,...n ]
    [ WITH GRANT OPTION ]
    [ AS SQL_Server_login ]

<server_principal> ::=
        SQL_Server_login
    | SQL_Server_login_from_Windows_login
    | SQL_Server_login_from_certificate
    | SQL_Server_login_from_AsymKey
```

## GRANT Full-Text Permissions (Transact-SQL)

`docs/t-sql/statements/grant-full-text-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT permission [ ,...n ] ON
    FULLTEXT
        {
           CATALOG :: full-text_catalog_name
           |
           STOPLIST :: full-text_stoplist_name
        }
    TO database_principal [ ,...n ]
    [ WITH GRANT OPTION ]
    [ AS granting_principal ]
```

## GRANT Object Permissions (Transact-SQL)

`docs/t-sql/statements/grant-object-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT <permission> [ ,...n ] ON
    [ OBJECT :: ][ schema_name ]. object_name [ ( column_name [ ,...n ] ) ]
    TO <database_principal> [ ,...n ]
    [ WITH GRANT OPTION ]
    [ AS <database_principal> ]

<permission> ::=
    ALL [ PRIVILEGES ] | permission [ ( column_name [ ,...n ] ) ]

<database_principal> ::=
        Database_user
    | Database_role
    | Application_role
    | Database_user_mapped_to_Windows_User
    | Database_user_mapped_to_Windows_Group
    | Database_user_mapped_to_certificate
    | Database_user_mapped_to_asymmetric_key
    | Database_user_with_no_login
```

## GRANT Schema Permissions (Transact-SQL)

`docs/t-sql/statements/grant-schema-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT permission  [ ,...n ] ON SCHEMA :: schema_name
    TO database_principal [ ,...n ]
    [ WITH GRANT OPTION ]
    [ AS granting_principal ]
```

## GRANT Search Property List Permissions

`docs/t-sql/statements/grant-search-property-list-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT permission [ ,...n ] ON
    SEARCH PROPERTY LIST :: search_property_list_name
    TO database_principal [ ,...n ]
    [ WITH GRANT OPTION ]
    [ AS granting_principal ]
```

## GRANT Server Permissions (Transact-SQL)

`docs/t-sql/statements/grant-server-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT permission [ ,...n ]
    TO <grantee_principal> [ ,...n ] [ WITH GRANT OPTION ]
    [ AS <grantor_principal> ]

<grantee_principal> ::= SQL_Server_login
    | SQL_Server_login_mapped_to_Windows_login
    | SQL_Server_login_mapped_to_Windows_group
    | SQL_Server_login_mapped_to_certificate
    | SQL_Server_login_mapped_to_asymmetric_key
    | server_role

<grantor_principal> ::= SQL_Server_login
    | SQL_Server_login_mapped_to_Windows_login
    | SQL_Server_login_mapped_to_Windows_group
    | SQL_Server_login_mapped_to_certificate
    | SQL_Server_login_mapped_to_asymmetric_key
    | server_role
```

## GRANT Server Principal Permissions (Transact-SQL)

`docs/t-sql/statements/grant-server-principal-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT permission [ ,...n ] }
    ON
    { [ LOGIN :: SQL_Server_login ]
      | [ SERVER ROLE :: server_role ] }
    TO <server_principal> [ ,...n ]
    [ WITH GRANT OPTION ]
    [ AS SQL_Server_login ]

<server_principal> ::=
    SQL_Server_login
    | SQL_Server_login_from_Windows_login
    | SQL_Server_login_from_certificate
    | SQL_Server_login_from_AsymKey
    | server_role
```

## GRANT Service Broker Permissions (Transact-SQL)

`docs/t-sql/statements/grant-service-broker-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT permission  [ ,...n ] ON
    {
              [ CONTRACT :: contract_name ]
       | [ MESSAGE TYPE :: message_type_name ]
       | [ REMOTE SERVICE BINDING :: remote_binding_name ]
       | [ ROUTE :: route_name ]
       | [ SERVICE :: service_name ]
    }
    TO database_principal [ ,...n ]
    [ WITH GRANT OPTION ]
        [ AS granting_principal ]
```

## GRANT Symmetric Key Permissions (Transact-SQL)

`docs/t-sql/statements/grant-symmetric-key-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT permission [ ,...n ]
    ON SYMMETRIC KEY :: symmetric_key_name
    TO <database_principal> [ ,...n ] [ WITH GRANT OPTION ]
        [ AS <database_principal> ]

<database_principal> ::=
        Database_user
    | Database_role
    | Application_role
    | Database_user_mapped_to_Windows_User
    | Database_user_mapped_to_Windows_Group
    | Database_user_mapped_to_certificate
    | Database_user_mapped_to_asymmetric_key
    | Database_user_with_no_login
```

## GRANT system object permissions (Transact-SQL)

`docs/t-sql/statements/grant-system-object-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT { SELECT | EXECUTE } ON [ sys. ] system_object TO principal
[ ; ]
```

## GRANT (Transact-SQL)

`docs/t-sql/statements/grant-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, and Fabric SQL database.

```syntaxsql
-- Simplified syntax for GRANT
GRANT { ALL [ PRIVILEGES ] }
      | permission [ ( column [ , ...n ] ) ] [ , ...n ]
      [ ON [ class :: ] securable ] TO principal [ , ...n ]
      [ WITH GRANT OPTION ] [ AS principal ]
```

> Syntax for Azure Synapse Analytics, Parallel Data Warehouse, and Microsoft Fabric warehouse.

```syntaxsql
GRANT
    <permission> [ , ...n ]
    [ ON [ <class_type> :: ] securable ]
    TO principal [ , ...n ]
    [ WITH GRANT OPTION ]
[;]

<permission> ::=
{ see the tables below }

<class_type> ::=
{
      LOGIN
    | DATABASE
    | OBJECT
    | ROLE
    | SCHEMA
    | USER
}
```

## GRANT Type Permissions (Transact-SQL)

`docs/t-sql/statements/grant-type-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT permission  [ ,...n ] ON TYPE :: [ schema_name . ] type_name
    TO <database_principal> [ ,...n ]
    [ WITH GRANT OPTION ]
    [ AS <database_principal> ]

<database_principal> ::=
        Database_user
    | Database_role
        | Application_role
    | Database_user_mapped_to_Windows_User
    | Database_user_mapped_to_Windows_Group
    | Database_user_mapped_to_certificate
    | Database_user_mapped_to_asymmetric_key
    | Database_user_with_no_login
```

## GRANT XML Schema Collection Permissions

`docs/t-sql/statements/grant-xml-schema-collection-permissions-transact-sql.md`

### Syntax

```syntaxsql
GRANT permission  [ ,...n ] ON
    XML SCHEMA COLLECTION :: [ schema_name . ]
    XML_schema_collection_name
    TO <database_principal> [ ,...n ]
    [ WITH GRANT OPTION ]
    [ AS <database_principal> ]

<database_principal> ::=
        Database_user
    | Database_role
    | Application_role
    | Database_user_mapped_to_Windows_User
    | Database_user_mapped_to_Windows_Group
    | Database_user_mapped_to_certificate
    | Database_user_mapped_to_asymmetric_key
    | Database_user_with_no_login
```

## INSERT (SQL Graph)

`docs/t-sql/statements/insert-sql-graph.md`

### INSERT Into node table syntax

> The syntax for inserting into a Node table is the same as for a regular table.

```syntaxsql
[ WITH <common_table_expression> [ ,...n ] ]
INSERT
{
        [ TOP ( expression ) [ PERCENT ] ]
        [ INTO ]
        { <object> | rowset_function_limited
          [ WITH ( <Table_Hint_Limited> [ ...n ] ) ]
        }
    {
        [ (column_list) ] | [(<edge_table_column_list>)]
        [ <OUTPUT Clause> ]
        { VALUES ( { DEFAULT | NULL | expression } [ ,...n ] ) [ ,...n     ]
        | derived_table
        | execute_statement
        | <dml_table_source>
        | DEFAULT VALUES
        }
    }
}
[;]

<object> ::=
{
    [ server_name . database_name . schema_name .
      | database_name .[ schema_name ] .
      | schema_name .
    ]
    node_table_name  | edge_table_name
}

<dml_table_source> ::=
    SELECT <select_list>
    FROM ( <dml_statement_with_output_clause> )
      [AS] table_alias [ ( column_alias [ ,...n ] ) ]
    [ WHERE <on_or_where_search_condition> ]
        [ OPTION ( <query_hint> [ ,...n ] ) ]

<on_or_where_search_condition> ::=
    {  <search_condition_with_match> | <search_condition> }

<search_condition_with_match> ::=
    { <graph_predicate> | [ NOT ] <predicate> | ( <search_condition> ) }
    [ AND { <graph_predicate> | [ NOT ] <predicate> | ( <search_condition> ) } ]
    [ ,...n ]

<search_condition> ::=
    { [ NOT ] <predicate> | ( <search_condition> ) }
    [ { AND | OR } [ NOT ] { <predicate> | ( <search_condition> ) } ]
    [ ,...n ]

<graph_predicate> ::=
    MATCH( <graph_search_pattern> [ AND <graph_search_pattern> ] [ , ...n] )

<graph_search_pattern>::=
    <node_alias> { { <-( <edge_alias> )- | -( <edge_alias> )-> } <node_alias> }

<edge_table_column_list> ::=
    ($from_id, $to_id, [column_list])
```

## INSERT (Transact-SQL)

`docs/t-sql/statements/insert-transact-sql.md`

### Syntax

> Syntax for SQL Server and Azure SQL Database and Fabric SQL database

```syntaxsql
-- Syntax for SQL Server and Azure SQL Database and Fabric SQL database

[ WITH <common_table_expression> [ ,...n ] ]
INSERT
{
        [ TOP ( expression ) [ PERCENT ] ]
        [ INTO ]
        { <object> | rowset_function_limited
          [ WITH ( <Table_Hint_Limited> [ ...n ] ) ]
        }
    {
        [ ( column_list ) ]
        [ <OUTPUT Clause> ]
        { VALUES ( { DEFAULT | NULL | expression } [ ,...n ] ) [ ,...n     ]
        | derived_table
        | execute_statement
        | <dml_table_source>
        | DEFAULT VALUES
        }
    }
}
[;]

<object> ::=
{
    [ server_name . database_name . schema_name .
      | database_name .[ schema_name ] .
      | schema_name .
    ]
  table_or_view_name
}

<dml_table_source> ::=
    SELECT <select_list>
    FROM ( <dml_statement_with_output_clause> )
      [AS] table_alias [ ( column_alias [ ,...n ] ) ]
    [ WHERE <search_condition> ]
        [ OPTION ( <query_hint> [ ,...n ] ) ]
```

```syntaxsql
-- External tool only syntax

INSERT
{
    [BULK]
    { database_name.schema_name.table_or_view_name | schema_name.table_or_view_name | table_or_view_name }
    ( <column_definition> )
    [ WITH (
        [ [ , ] CHECK_CONSTRAINTS ]
        [ [ , ] FIRE_TRIGGERS ]
        [ [ , ] KEEP_NULLS ]
        [ [ , ] KILOBYTES_PER_BATCH = kilobytes_per_batch ]
        [ [ , ] ROWS_PER_BATCH = rows_per_batch ]
        [ [ , ] ORDER ( { column [ ASC | DESC ] } [ ,...n ] ) ]
        [ [ , ] TABLOCK ]
    ) ]
}

[; ] <column_definition> ::=
 column_name <data_type>
    [ COLLATE collation_name ]
    [ NULL | NOT NULL ]

<data type> ::=
[ type_schema_name . ] type_name
    [ ( precision [ , scale ] | max ]
```

> Syntax for Azure Synapse Analytics and Parallel Data Warehouse and Microsoft Fabric Warehouse

```syntaxsql
-- Syntax for Azure Synapse Analytics and Parallel Data Warehouse and Microsoft Fabric

INSERT [INTO] { database_name.schema_name.table_name | schema_name.table_name | table_name }
    [ ( column_name [ ,...n ] ) ]
    {
      VALUES ( { NULL | expression } )
      | SELECT <select_criteria>
    }
    [ OPTION ( <query_option> [ ,...n ] ) ]
[;]
```

## MERGE (Transact-SQL)

`docs/t-sql/statements/merge-transact-sql.md`

### Syntax

Marked for `=azuresqldb-current || =azuresqldb-mi-current || >=sql-server-2017 || >=sql-server-linux-2017 || =fabric-sqldb`.

> Syntax for SQL Server, Azure SQL Database, and SQL database in Fabric:

```syntaxsql
[ WITH <common_table_expression> [,...n] ]
MERGE
    [ TOP ( expression ) [ PERCENT ] ]
    [ INTO ] <target_table> [ WITH ( <merge_hint> ) ] [ [ AS ] table_alias ]
    USING <table_source> [ [ AS ] table_alias ]
    ON <merge_search_condition>
    [ WHEN MATCHED [ AND <clause_search_condition> ]
        THEN <merge_matched> ] [ ...n ]
    [ WHEN NOT MATCHED [ BY TARGET ] [ AND <clause_search_condition> ]
        THEN <merge_not_matched> ]
    [ WHEN NOT MATCHED BY SOURCE [ AND <clause_search_condition> ]
        THEN <merge_matched> ] [ ...n ]
    [ <output_clause> ]
    [ OPTION ( <query_hint> [ ,...n ] ) ]
;

<target_table> ::=
{
    [ database_name . schema_name . | schema_name . ] [ [ AS ] target_table ]
    | @variable [ [ AS ] target_table ]
    | common_table_expression_name [ [ AS ] target_table ]
}

<merge_hint>::=
{
    { [ <table_hint_limited> [ ,...n ] ]
    [ [ , ] { INDEX ( index_val [ ,...n ] ) | INDEX = index_val }]
    }
}

<merge_search_condition> ::=
    <search_condition>

<merge_matched>::=
    { UPDATE SET <set_clause> | DELETE }

<merge_not_matched>::=
{
    INSERT [ ( column_list ) ]
        { VALUES ( values_list )
        | DEFAULT VALUES }
}

<clause_search_condition> ::=
    <search_condition>
```

Marked for `=azure-sqldw-latest || =fabric`.

> Syntax for Azure Synapse Analytics, Fabric Data Warehouse:

```syntaxsql
[ WITH <common_table_expression> [,...n] ]
MERGE
    [ INTO ] <target_table> [ [ AS ] table_alias ]
    USING <table_source> [ [ AS ] table_alias ]
    ON <merge_search_condition>
    [ WHEN MATCHED [ AND <clause_search_condition> ]
        THEN <merge_matched> ] [ ...n ]
    [ WHEN NOT MATCHED [ BY TARGET ] [ AND <clause_search_condition> ]
        THEN <merge_not_matched> ]
    [ WHEN NOT MATCHED BY SOURCE [ AND <clause_search_condition> ]
        THEN <merge_matched> ] [ ...n ]
    [ OPTION ( <query_hint> [ ,...n ] ) ]
;  -- The semi-colon is required, or the query will return a syntax error.

<target_table> ::=
{
    [ database_name . schema_name . | schema_name . ]
  target_table
}

<merge_search_condition> ::=
    <search_condition>

<merge_matched>::=
    { UPDATE SET <set_clause> | DELETE }

<merge_not_matched>::=
{
    INSERT [ ( column_list ) ]
        VALUES ( values_list )
}

<clause_search_condition> ::=
    <search_condition>
```

## MOVE CONVERSATION (Transact-SQL)

`docs/t-sql/statements/move-conversation-transact-sql.md`

### Syntax

```syntaxsql
MOVE CONVERSATION conversation_handle
   TO conversation_group_id
[ ; ]
```

## OPEN MASTER KEY (Transact-SQL)

`docs/t-sql/statements/open-master-key-transact-sql.md`

### Syntax

```syntaxsql
OPEN MASTER KEY DECRYPTION BY PASSWORD = 'password'
```

## OPEN SYMMETRIC KEY (Transact-SQL)

`docs/t-sql/statements/open-symmetric-key-transact-sql.md`

### Syntax

```syntaxsql
OPEN SYMMETRIC KEY Key_name DECRYPTION BY <decryption_mechanism>

<decryption_mechanism> ::=
    CERTIFICATE certificate_name [ WITH PASSWORD = 'password' ]
    |
    ASYMMETRIC KEY asym_key_name [ WITH PASSWORD = 'password' ]
    |
    SYMMETRIC KEY decrypting_Key_name
    |
    PASSWORD = 'decryption_password'
```

## GRANT-DENY-REVOKE permissions

`docs/t-sql/statements/permissions-grant-deny-revoke-azure-sql-data-warehouse-parallel-data-warehouse.md`

### Syntax

```syntaxsql
-- Azure Synapse Analytics and Parallel Data Warehouse and Microsoft Fabric
GRANT
    <permission> [ ,...n ]
    [ ON [ <class_type> :: ] securable ]
    TO principal [ ,...n ]
    [ WITH GRANT OPTION ]
[;]

DENY
    <permission> [ ,...n ]
    [ ON [ <class_type> :: ] securable ]
    TO principal [ ,...n ]
    [ CASCADE ]
[;]

REVOKE
    <permission> [ ,...n ]
    [ ON [ <class_type> :: ] securable ]
    [ FROM | TO ] principal [ ,...n ]
    [ CASCADE ]
[;]

<permission> ::=
{ see the tables below }

<class_type> ::=
{
      LOGIN
    | DATABASE
    | OBJECT
    | ROLE
    | SCHEMA
    | USER
}
```

## RECEIVE (Transact-SQL)

`docs/t-sql/statements/receive-transact-sql.md`

### Syntax

```syntaxsql
[ WAITFOR ( ]
    RECEIVE [ TOP ( n ) ]
        <column_specifier> [ ,...n ]
        FROM <queue>
        [ INTO table_variable ]
        [ WHERE {  conversation_handle = conversation_handle
                 | conversation_group_id = conversation_group_id } ]
[ ) ] [ , TIMEOUT timeout ]
[ ; ]

<column_specifier> ::=
{    *
  |  { column_name | [ ] expression } [ [ AS ] column_alias ]
}     [ ,...n ]

<queue> ::=
{ database_name.schema_name.queue_name | schema_name.queue_name | queue_name }
```

## RENAME (Transact-SQL)

`docs/t-sql/statements/rename-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for Azure Synapse Analytics

-- Rename a table.
RENAME OBJECT [::] [ [ database_name . [schema_name ] ] . ] | [schema_name . ] ] table_name TO new_table_name
[;]
```

```syntaxsql
-- Syntax for Analytics Platform System (PDW)

-- Rename a table
RENAME OBJECT [::] [ [ database_name . [ schema_name ] . ] | [ schema_name . ] ] table_name TO new_table_name
[;]

-- Rename a database
RENAME DATABASE [::] database_name TO new_database_name
[;]

-- Rename a column
RENAME OBJECT [::] [ [ database_name . [schema_name ] ] . ] | [schema_name . ] ] table_name COLUMN column_name TO new_column_name [;]
```

## RESTORE MASTER KEY (Transact-SQL)

`docs/t-sql/statements/restore-master-key-transact-sql.md`

### Syntax

```syntaxsql
RESTORE MASTER KEY FROM
  {
    FILE = 'path_to_file'
  | URL = 'Azure Blob storage URL'
  }
    DECRYPTION BY PASSWORD = 'password'
    ENCRYPTION BY PASSWORD = 'password'
    [ FORCE ]
```

## RESTORE SERVICE MASTER KEY (Transact-SQL)

`docs/t-sql/statements/restore-service-master-key-transact-sql.md`

### Syntax

```syntaxsql
RESTORE SERVICE MASTER KEY FROM FILE = 'path_to_file'
    DECRYPTION BY PASSWORD = 'password' [FORCE]
```

## RESTORE FILELISTONLY (Transact-SQL)

`docs/t-sql/statements/restore-statements-filelistonly-transact-sql.md`

### Syntax

```syntaxsql
RESTORE FILELISTONLY
FROM <backup_device>
[ WITH
 {
--Backup Set Options
   FILE = { backup_set_file_number | @backup_set_file_number }
 | PASSWORD = { password | @password_variable }
 | [ METADATA_ONLY | SNAPSHOT ] [ DBNAME = { database_name | @database_name_variable } ]

--Media Set Options
 | MEDIANAME = { media_name | @media_name_variable }
 | MEDIAPASSWORD = { mediapassword | @mediapassword_variable }

--Error Management Options
 | { CHECKSUM | NO_CHECKSUM }
 | { STOP_ON_ERROR | CONTINUE_AFTER_ERROR }

--Tape Options
 | { REWIND | NOREWIND }
 | { UNLOAD | NOUNLOAD }
 } [ ,...n ]
]
[;]

<backup_device> ::=
{
   { logical_backup_device_name |
      @logical_backup_device_name_var }
   | { DISK | TAPE | URL } = { 'physical_backup_device_name' |
       @physical_backup_device_name_var }
}
```

## RESTORE HEADERONLY (Transact-SQL)

`docs/t-sql/statements/restore-statements-headeronly-transact-sql.md`

### Syntax

```syntaxsql
RESTORE HEADERONLY
FROM <backup_device>
[ WITH
    {
    -- Backup set options
    FILE = { backup_set_file_number | @backup_set_file_number }
    | PASSWORD = { password | @password_variable }
    | [ METADATA_ONLY | SNAPSHOT ] [ DBNAME = { database_name | @database_name_variable } ]

    -- Media set options
    | MEDIANAME = { media_name | @media_name_variable }
    | MEDIAPASSWORD = { mediapassword | @mediapassword_variable }

    -- Error management options
    | { CHECKSUM | NO_CHECKSUM }
    | { STOP_ON_ERROR | CONTINUE_AFTER_ERROR }

    -- Tape options
    | { REWIND | NOREWIND }
    | { UNLOAD | NOUNLOAD }
    } [ , ...n ]
]
[ ; ]

<backup_device> ::=
{
   { logical_backup_device_name |
     @logical_backup_device_name_var }
   | { DISK | TAPE | URL } = { 'physical_backup_device_name' |
       @physical_backup_device_name_var }
}
```

## RESTORE LABELONLY (Transact-SQL)

`docs/t-sql/statements/restore-statements-labelonly-transact-sql.md`

### Syntax

```syntaxsql
RESTORE LABELONLY
FROM <backup_device>
[ WITH
 {
--Media Set Options
   MEDIANAME = { media_name | @media_name_variable }
 | MEDIAPASSWORD = { mediapassword | @mediapassword_variable }

--Error Management Options
 | { CHECKSUM | NO_CHECKSUM }
 | { STOP_ON_ERROR | CONTINUE_AFTER_ERROR }

--Tape Options
 | { REWIND | NOREWIND }
 | { UNLOAD | NOUNLOAD }
 } [ ,...n ]
]
[;]

<backup_device> ::=
{
   { logical_backup_device_name |
      @logical_backup_device_name_var }
   | { DISK | TAPE | URL } = { 'physical_backup_device_name' |
       @physical_backup_device_name_var }
}
```

## RESTORE REWINDONLY (Transact-SQL)

`docs/t-sql/statements/restore-statements-rewindonly-transact-sql.md`

### Syntax

```syntaxsql
RESTORE REWINDONLY
FROM <backup_device> [ ,...n ]
[ WITH {UNLOAD | NOUNLOAD}]
}
[;]

<backup_device> ::=
{
   { logical_backup_device_name |
      @logical_backup_device_name_var }
   | TAPE = { 'physical_backup_device_name' |
       @physical_backup_device_name_var }
}
```

## RESTORE (Transact-SQL)

`docs/t-sql/statements/restore-statements-transact-sql.md`

### Syntax

> - For more information about descriptions of the arguments, see [RESTORE Arguments](../../t-sql/statements/restore-statements-arguments-transact-sql.md).

```syntaxsql
--To Restore an Entire Database from a Full database backup (a Complete Restore):
RESTORE DATABASE { database_name | @database_name_var }
 [ FROM <backup_device> [ ,...n ] ]
 [ WITH
   {
    [ RECOVERY | NORECOVERY | STANDBY =
        {standby_file_name | @standby_file_name_var }
       ]
   | ,  <general_WITH_options> [ ,...n ]
   | , <replication_WITH_option>
   | , <change_data_capture_WITH_option>
   | , <FILESTREAM_WITH_option>
   | , <service_broker_WITH options>
   | , <point_in_time_WITH_options-RESTORE_DATABASE>
   } [ ,...n ]
 ]
[;]

--To perform the first step of the initial restore sequence of a piecemeal restore:
RESTORE DATABASE { database_name | @database_name_var }
   <files_or_filegroups> [ ,...n ]
 [ FROM <backup_device> [ ,...n ] ]
   WITH
      PARTIAL, NORECOVERY
      [  , <general_WITH_options> [ ,...n ]
       | , <point_in_time_WITH_options-RESTORE_DATABASE>
      ] [ ,...n ]
[;]

--To Restore Specific Files or Filegroups:
RESTORE DATABASE { database_name | @database_name_var }
   <file_or_filegroup> [ ,...n ]
 [ FROM <backup_device> [ ,...n ] ]
   WITH
   {
      [ RECOVERY | NORECOVERY ]
      [ , <general_WITH_options> [ ,...n ] ]
   } [ ,...n ]
[;]

--To Restore Specific Pages:
RESTORE DATABASE { database_name | @database_name_var }
   PAGE = 'file:page [ ,...n ]'
 [ , <file_or_filegroups> ] [ ,...n ]
 [ FROM <backup_device> [ ,...n ] ]
   WITH
       NORECOVERY
      [ , <general_WITH_options> [ ,...n ] ]
[;]

--To Restore a Transaction Log:
RESTORE LOG { database_name | @database_name_var }
 [ <file_or_filegroup_or_pages> [ ,...n ] ]
 [ FROM <backup_device> [ ,...n ] ]
 [ WITH
   {
     [ RECOVERY | NORECOVERY | STANDBY =
        {standby_file_name | @standby_file_name_var }
       ]
    | , <general_WITH_options> [ ,...n ]
    | , <replication_WITH_option>
    | , <point_in_time_WITH_options-RESTORE_LOG>
   } [ ,...n ]
 ]
[;]

--To Revert a Database to a Database Snapshot:
RESTORE DATABASE { database_name | @database_name_var }
FROM DATABASE_SNAPSHOT = database_snapshot_name

<backup_device>::=
{
   { logical_backup_device_name |
      @logical_backup_device_name_var }
 | { DISK
     | TAPE
     | URL
   } = { 'physical_backup_device_name' |
      @physical_backup_device_name_var }
}

<files_or_filegroups>::=
{
   FILE = { logical_file_name_in_backup | @logical_file_name_in_backup_var }
 | FILEGROUP = { logical_filegroup_name | @logical_filegroup_name_var }
 | READ_WRITE_FILEGROUPS
}

<general_WITH_options> [ ,...n ]::=
--Restore Operation Options
   MOVE 'logical_file_name_in_backup' TO 'operating_system_file_name'
          [ ,...n ]
 | REPLACE
 | RESTART
 | RESTRICTED_USER | CREDENTIAL

--Backup Set Options
 | FILE = { backup_set_file_number | @backup_set_file_number }
 | PASSWORD = { password | @password_variable }
 | [ METADATA_ONLY | SNAPSHOT ] [ DBNAME = { database_name | @database_name_variable } ]

--Media Set Options
 | MEDIANAME = { media_name | @media_name_variable }
 | MEDIAPASSWORD = { mediapassword | @mediapassword_variable }
 | BLOCKSIZE = { blocksize | @blocksize_variable }

--Data Transfer Options
 | BUFFERCOUNT = { buffercount | @buffercount_variable }
 | MAXTRANSFERSIZE = { maxtransfersize | @maxtransfersize_variable }

--Error Management Options
 | { CHECKSUM | NO_CHECKSUM }
 | { STOP_ON_ERROR | CONTINUE_AFTER_ERROR }

--Monitoring Options
 | STATS [ = percentage ]

--Tape Options.
 | { REWIND | NOREWIND }
 | { UNLOAD | NOUNLOAD }

<replication_WITH_option>::=
 | KEEP_REPLICATION

<change_data_capture_WITH_option>::=
 | KEEP_CDC

<FILESTREAM_WITH_option>::=
 | FILESTREAM ( DIRECTORY_NAME = directory_name )

<service_broker_WITH_options>::=
 | ENABLE_BROKER
 | ERROR_BROKER_CONVERSATIONS
 | NEW_BROKER

<point_in_time_WITH_options-RESTORE_DATABASE>::=
 | {
   STOPAT = { 'datetime'| @datetime_var }
 | STOPATMARK = 'lsn:lsn_number'
                 [ AFTER 'datetime']
 | STOPBEFOREMARK = 'lsn:lsn_number'
                 [ AFTER 'datetime']
   }

<point_in_time_WITH_options-RESTORE_LOG>::=
 | {
   STOPAT = { 'datetime'| @datetime_var }
 | STOPATMARK = { 'mark_name' | 'lsn:lsn_number' }
                 [ AFTER 'datetime']
 | STOPBEFOREMARK = { 'mark_name' | 'lsn:lsn_number' }
                 [ AFTER 'datetime']
   }
```

```syntaxsql
--To Restore an Entire Database from a Full database backup (a Complete Restore):
RESTORE DATABASE { database_name | @database_name_var }
 FROM URL = { 'physical_device_name' | @physical_device_name_var } [ ,...n ]
[;]
```

```syntaxsql
-- Restore the master database
-- Use the Configuration Manager tool.

Restore a full user database backup.
RESTORE DATABASE database_name
    FROM DISK = '\\UNC_path\full_backup_directory'
[;]

--Restore a full user database backup and then a differential backup.
RESTORE DATABASE database_name
    FROM DISK = '\\UNC_path\differential_backup_directory'
    WITH [ ( ] BASE = '\\UNC_path\full_backup_directory' [ ) ]
[;]

--Restore header information for a full or differential user database backup.
RESTORE HEADERONLY
    FROM DISK = '\\UNC_path\backup_directory'
[;]
```

### B. Restore a full and differential backup

> The full backup of the database is restored from the full backup which is stored in the `\\\xxx.xxx.xxx.xxx\backups\yearly\Invoices2013Full` directory. If the restore completes successfully, the differential backup is restored to the `SalesInvoices2013` database. The differential backup is stored in the `\\\xxx.xxx.xxx.xxx\backups\yearly\Invoices2013Diff` directory.

```syntaxsql
RESTORE DATABASE SalesInvoices2013
    FROM DISK = '\\xxx.xxx.xxx.xxx\backups\yearly\Invoices2013Diff'
    WITH BASE = '\\xxx.xxx.xxx.xxx\backups\yearly\Invoices2013Full'
[;]
```

### C. Restore the backup header

> This example restores the header information for database backup `\\\xxx.xxx.xxx.xxx\backups\yearly\Invoices2013Full`. The command results in one row of information for the `Invoices2013Full` backup.

```syntaxsql
RESTORE HEADERONLY
    FROM DISK = '\\xxx.xxx.xxx.xxx\backups\yearly\Invoices2013Full'
[;]
```

## RESTORE VERIFYONLY (Transact-SQL)

`docs/t-sql/statements/restore-statements-verifyonly-transact-sql.md`

### Syntax

```syntaxsql
RESTORE VERIFYONLY
FROM <backup_device> [ ,...n ]
[ WITH
 {
   LOADHISTORY

--Restore Operation Option
 | MOVE 'logical_file_name_in_backup' TO 'operating_system_file_name'
          [ ,...n ]

--Backup Set Options
 | FILE = { backup_set_file_number | @backup_set_file_number }
 | PASSWORD = { password | @password_variable }

--Media Set Options
 | MEDIANAME = { media_name | @media_name_variable }
 | MEDIAPASSWORD = { mediapassword | @mediapassword_variable }

--Error Management Options
 | { CHECKSUM | NO_CHECKSUM }
 | { STOP_ON_ERROR | CONTINUE_AFTER_ERROR }

--Monitoring Options
 | STATS [ = percentage ]

--Tape Options
 | { REWIND | NOREWIND }
 | { UNLOAD | NOUNLOAD }
 } [ ,...n ]
]
[;]

<backup_device> ::=
{
   { logical_backup_device_name |
      @logical_backup_device_name_var }
   | { DISK | TAPE | URL } = { 'physical_backup_device_name' |
       @physical_backup_device_name_var }
}
```

## RESTORE SYMMETRIC KEY (Transact-SQL)

`docs/t-sql/statements/restore-symmetric-key-transact-sql.md`

### Syntax

```syntaxsql
RESTORE SYMMETRIC KEY key_name FROM
  {
    FILE = 'path_to_file'
  | URL = 'Azure Blob storage URL'
  }
      DECRYPTION BY PASSWORD = 'password'
      ENCRYPTION BY PASSWORD = 'password'
```

## REVERT (Transact-SQL)

`docs/t-sql/statements/revert-transact-sql.md`

### Syntax

```syntaxsql
REVERT
    [ WITH COOKIE = @varbinary_variable ]
```

## REVOKE Assembly Permissions (Transact-SQL)

`docs/t-sql/statements/revoke-assembly-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] permission [ ,...n ]
    ON ASSEMBLY :: assembly_name
    { TO | FROM } database_principal [ ,...n ]
    [ CASCADE ]
    [ AS revoking_principal ]
```

## REVOKE Asymmetric Key Permissions (Transact-SQL)

`docs/t-sql/statements/revoke-asymmetric-key-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] { permission  [ ,...n ] }
    ON ASYMMETRIC KEY :: asymmetric_key_name
    { TO | FROM } database_principal [ ,...n ]
    [ CASCADE ]
    [ AS revoking_principal ]
```

## REVOKE Availability Group Permissions

`docs/t-sql/statements/revoke-availability-group-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] permission  [ ,...n ]
    ON AVAILABILITY GROUP :: availability_group_name
    { FROM | TO } < server_principal >  [ ,...n ]
    [ CASCADE ]
    [ AS SQL_Server_login ]

<server_principal> ::=
        SQL_Server_login
    | SQL_Server_login_from_Windows_login
    | SQL_Server_login_from_certificate
    | SQL_Server_login_from_AsymKey
```

## REVOKE Certificate Permissions (Transact-SQL)

`docs/t-sql/statements/revoke-certificate-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] permission  [ ,...n ]
    ON CERTIFICATE :: certificate_name
    { TO | FROM } database_principal [ ,...n ]
    [ CASCADE ]
    [ AS revoking_principal ]
```

## REVOKE Database Permissions (Transact-SQL)

`docs/t-sql/statements/revoke-database-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] <permission> [ ,...n ]
    { TO | FROM } <database_principal> [ ,...n ]
        [ CASCADE ]
    [ AS <database_principal> ]

<permission> ::=
permission | ALL [ PRIVILEGES ]

<database_principal> ::=
      Database_user
    | Database_role
    | Application_role
    | Database_user_mapped_to_Windows_User
    | Database_user_mapped_to_Windows_Group
    | Database_user_mapped_to_certificate
    | Database_user_mapped_to_asymmetric_key
    | Database_user_with_no_login
```

## REVOKE Database Principal Permissions

`docs/t-sql/statements/revoke-database-principal-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] permission [ ,...n ]
    ON
    {  [ USER :: database_user ]
       | [ ROLE :: database_role ]
       | [ APPLICATION ROLE :: application_role ]
    }
    { FROM | TO } <database_principal> [ ,...n ]
        [ CASCADE ]
    [ AS <database_principal> ]

<database_principal> ::=
        Database_user
    | Database_role
    | Application_role
    | Database_user_mapped_to_Windows_User
    | Database_user_mapped_to_Windows_Group
    | Database_user_mapped_to_certificate
    | Database_user_mapped_to_asymmetric_key
    | Database_user_with_no_login
```

## REVOKE Database Scoped Credential (Transact-SQL)

`docs/t-sql/statements/revoke-database-scoped-credential-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] permission  [ ,...n ]
    ON DATABASE SCOPED CREDENTIAL :: credential_name
    { TO | FROM } database_principal [ ,...n ]
    [ CASCADE ]
    [ AS revoking_principal ]
```

## REVOKE Endpoint Permissions (Transact-SQL)

`docs/t-sql/statements/revoke-endpoint-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] permission [ ,...n ]
    ON ENDPOINT :: endpoint_name
    { FROM | TO } <server_principal> [ ,...n ]
    [ CASCADE ]
    [ AS SQL_Server_login ]

<server_principal> ::=
        SQL_Server_login
    | SQL_Server_login_from_Windows_login
    | SQL_Server_login_from_certificate
    | SQL_Server_login_from_AsymKey
```

## REVOKE Full-Text Permissions (Transact-SQL)

`docs/t-sql/statements/revoke-full-text-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] permission [ ,...n ] ON
    FULLTEXT
        {
           CATALOG :: full-text_catalog_name
           |
           STOPLIST :: full-text_stoplist_name
        }
    { TO | FROM } database_principal [ ,...n ]
    [ CASCADE ]
    [ AS revoking_principal ]
```

## REVOKE Object Permissions (Transact-SQL)

`docs/t-sql/statements/revoke-object-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] <permission> [ ,...n ] ON
    [ OBJECT :: ][ schema_name ]. object_name [ ( column [ ,...n ] ) ]
        { FROM | TO } <database_principal> [ ,...n ]
    [ CASCADE ]
    [ AS <database_principal> ]

<permission> ::=
    ALL [ PRIVILEGES ] | permission [ ( column [ ,...n ] ) ]

<database_principal> ::=
        Database_user
    | Database_role
    | Application_role
    | Database_user_mapped_to_Windows_User
    | Database_user_mapped_to_Windows_Group
    | Database_user_mapped_to_certificate
    | Database_user_mapped_to_asymmetric_key
    | Database_user_with_no_login
```

## REVOKE Schema Permissions (Transact-SQL)

`docs/t-sql/statements/revoke-schema-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] permission  [ ,...n ]
    ON SCHEMA :: schema_name
    { TO | FROM } database_principal [ ,...n ]
    [ CASCADE ]
    [ AS revoking_principal ]
```

## REVOKE Search Property List Permissions

`docs/t-sql/statements/revoke-search-property-list-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] permission [ ,...n ] ON
        SEARCH PROPERTY LIST :: search_property_list_name
    { TO | FROM } database_principal [ ,...n ]
    [ CASCADE ]
    [ AS revoking_principal ]
```

## REVOKE Server Permissions (Transact-SQL)

`docs/t-sql/statements/revoke-server-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] permission  [ ,...n ]
    { TO | FROM } <grantee_principal> [ ,...n ]
        [ CASCADE ]
    [ AS <grantor_principal> ]

<grantee_principal> ::= SQL_Server_login
        | SQL_Server_login_mapped_to_Windows_login
    | SQL_Server_login_mapped_to_Windows_group
    | SQL_Server_login_mapped_to_certificate
    | SQL_Server_login_mapped_to_asymmetric_key
    | server_role

<grantor_principal> ::= SQL_Server_login
    | SQL_Server_login_mapped_to_Windows_login
    | SQL_Server_login_mapped_to_Windows_group
    | SQL_Server_login_mapped_to_certificate
    | SQL_Server_login_mapped_to_asymmetric_key
    | server_role
```

## REVOKE Server Principal Permissions

`docs/t-sql/statements/revoke-server-principal-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] permission [ ,...n ] }
    ON
    { [ LOGIN :: SQL_Server_login ]
      | [ SERVER ROLE :: server_role ] }
    { FROM | TO } <server_principal> [ ,...n ]
    [ CASCADE ]
    [ AS SQL_Server_login ]

<server_principal> ::=
    SQL_Server_login
    | SQL_Server_login_from_Windows_login
    | SQL_Server_login_from_certificate
    | SQL_Server_login_from_AsymKey
    | server_role
```

## REVOKE Service Broker Permissions (Transact-SQL)

`docs/t-sql/statements/revoke-service-broker-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] permission [ ,...n ] ON
    {
       [ CONTRACT :: contract_name ]
       | [ MESSAGE TYPE :: message_type_name ]
       | [ REMOTE SERVICE BINDING :: remote_binding_name ]
       | [ ROUTE :: route_name ]
       | [ SERVICE :: service_name ]
        }
    { TO | FROM } database_principal [ ,...n ]
    [ CASCADE ]
    [ AS revoking_principal ]
```

## REVOKE Symmetric Key Permissions (Transact-SQL)

`docs/t-sql/statements/revoke-symmetric-key-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] permission [ ,...n ]
    ON SYMMETRIC KEY :: symmetric_key_name
        { TO | FROM } <database_principal> [ ,...n ]
    [ CASCADE ]
    [ AS <database_principal> ]

<database_principal> ::=
        Database_user
    | Database_role
    | Application_role
    | Database_user_mapped_to_Windows_User
    | Database_user_mapped_to_Windows_Group
    | Database_user_mapped_to_certificate
    | Database_user_mapped_to_asymmetric_key
    | Database_user_with_no_login
```

## REVOKE System Object Permissions (Transact-SQL)

`docs/t-sql/statements/revoke-system-object-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE { SELECT | EXECUTE } ON [sys.]system_object FROM principal
```

## REVOKE (Transact-SQL)

`docs/t-sql/statements/revoke-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, and Fabric SQL database

```syntaxsql
-- Simplified syntax for REVOKE
REVOKE [ GRANT OPTION FOR ]
      {
        [ ALL [ PRIVILEGES ] ]
        |
                permission [ ( column [ ,...n ] ) ] [ ,...n ]
      }
      [ ON [ class :: ] securable ]
      { TO | FROM } principal [ ,...n ]
      [ CASCADE] [ AS principal ]
```

> Syntax for Azure Synapse Analytics, Parallel Data Warehouse, and Microsoft Fabric warehouse

```syntaxsql
REVOKE
    <permission> [ ,...n ]
    [ ON [ <class_type> :: ] securable ]
    [ FROM | TO ] principal [ ,...n ]
    [ CASCADE ]
[;]

<permission> ::=
{ see the tables below }

<class_type> ::=
{
      LOGIN
    | DATABASE
    | OBJECT
    | ROLE
    | SCHEMA
    | USER
}
```

## REVOKE Type Permissions (Transact-SQL)

`docs/t-sql/statements/revoke-type-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] permission [ ,...n ]
    ON TYPE :: [ schema_name ]. type_name
    { FROM | TO } <database_principal> [ ,...n ]
    [ CASCADE ]
    [ AS <database_principal> ]

<database_principal> ::=
      Database_user
    | Database_role
    | Application_role
    | Database_user_mapped_to_Windows_User
    | Database_user_mapped_to_Windows_Group
    | Database_user_mapped_to_certificate
    | Database_user_mapped_to_asymmetric_key
    | Database_user_with_no_login
```

## REVOKE XML Schema Collection Permissions

`docs/t-sql/statements/revoke-xml-schema-collection-permissions-transact-sql.md`

### Syntax

```syntaxsql
REVOKE [ GRANT OPTION FOR ] permission [ ,...n ] ON
    XML SCHEMA COLLECTION :: [ schema_name . ]
    XML_schema_collection_name
    { TO | FROM } <database_principal> [ ,...n ]
        [ CASCADE ]
    [ AS <database_principal> ]

<database_principal> ::=
        Database_user
    | Database_role
    | Application_role
    | Database_user_mapped_to_Windows_User
    | Database_user_mapped_to_Windows_Group
    | Database_user_mapped_to_certificate
    | Database_user_mapped_to_asymmetric_key
    | Database_user_with_no_login
```

## SEND (Transact-SQL)

`docs/t-sql/statements/send-transact-sql.md`

### Syntax

```syntaxsql
SEND
   ON CONVERSATION [(]conversation_handle [,.. @conversation_handle_n][)]
   [ MESSAGE TYPE message_type_name ]
   [ ( message_body_expression ) ]
[ ; ]
```

## SET ANSI_DEFAULTS (Transact-SQL)

`docs/t-sql/statements/set-ansi-defaults-transact-sql.md`

### Syntax for SQL Server, serverless SQL pool in Azure Synapse Analytics, Microsoft Fabric

```syntaxsql
SET ANSI_DEFAULTS { ON | OFF }
```

### Syntax for Azure Synapse Analytics and Analytics Platform System (PDW)

```syntaxsql
SET ANSI_DEFAULTS ON
```

## SET ANSI_NULL_DFLT_OFF (Transact-SQL)

`docs/t-sql/statements/set-ansi-null-dflt-off-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server and Azure SQL Database and Microsoft Fabric

SET ANSI_NULL_DFLT_OFF { ON | OFF }
```

```syntaxsql
-- Syntax for Azure Synapse Analytics and Parallel Data Warehouse

SET ANSI_NULL_DFLT_OFF OFF
```

## SET ANSI_NULL_DFLT_ON (Transact-SQL)

`docs/t-sql/statements/set-ansi-null-dflt-on-transact-sql.md`

### Syntax

```syntaxsql
-- Syntax for SQL Server and Azure SQL Database and Microsoft Fabric

SET ANSI_NULL_DFLT_ON {ON | OFF}
```

```syntaxsql
-- Syntax for Azure Synapse Analytics and Parallel Data Warehouse

SET ANSI_NULL_DFLT_ON ON
```

## SET ANSI_NULLS (Transact-SQL)

`docs/t-sql/statements/set-ansi-nulls-transact-sql.md`

### Syntax for SQL Server, serverless SQL pool in Azure Synapse Analytics, Microsoft Fabric

```syntaxsql
SET ANSI_NULLS { ON | OFF }
```

### Syntax for Azure Synapse Analytics and Analytics Platform System (PDW)

```syntaxsql
SET ANSI_NULLS ON
```

## SET ANSI_PADDING (Transact-SQL)

`docs/t-sql/statements/set-ansi-padding-transact-sql.md`

### Syntax

> Syntax for SQL Server, serverless SQL pool in Azure Synapse Analytics, Microsoft Fabric.

```syntaxsql
SET ANSI_PADDING { ON | OFF }
```

> Syntax for Azure Synapse Analytics and Analytics Platform System (PDW).

```syntaxsql
SET ANSI_PADDING ON
```

## SET ANSI_WARNINGS (Transact-SQL)

`docs/t-sql/statements/set-ansi-warnings-transact-sql.md`

### Syntax for SQL Server, serverless SQL pool in Azure Synapse Analytics, Microsoft Fabric

```syntaxsql
SET ANSI_WARNINGS { ON | OFF }
```

### Syntax for Azure Synapse Analytics and Analytics Platform System (PDW)

```syntaxsql
SET ANSI_WARNINGS ON
```

## SET ARITHABORT (Transact-SQL)

`docs/t-sql/statements/set-arithabort-transact-sql.md`

### Syntax for SQL Server, serverless SQL pool in Azure Synapse Analytics, Microsoft Fabric, SQL database in Microsoft Fabric

```syntaxsql
SET ARITHABORT { ON | OFF }
```

### Syntax for Azure Synapse Analytics and Analytics Platform System (PDW)

```syntaxsql
SET ARITHABORT ON
```

## SET ARITHIGNORE (Transact-SQL)

`docs/t-sql/statements/set-arithignore-transact-sql.md`

### Syntax for SQL Server, Azure SQL Database, Microsoft Fabric Data Warehouse, SQL database in Microsoft Fabric

```syntaxsql
SET ARITHIGNORE { ON | OFF }
```

### Syntax for Azure Synapse Analytics and Analytics Platform System (PDW)

```syntaxsql
SET ARITHIGNORE OFF
```

## SET CONCAT_NULL_YIELDS_NULL (Transact-SQL)

`docs/t-sql/statements/set-concat-null-yields-null-transact-sql.md`

### Syntax for SQL Server, serverless SQL pool in Azure Synapse Analytics, Microsoft Fabric

```syntaxsql
SET CONCAT_NULL_YIELDS_NULL { ON | OFF }
```

### Syntax for Azure Synapse Analytics and Analytics Platform System (PDW)

```syntaxsql
SET CONCAT_NULL_YIELDS_NULL ON
```

## SET CONTEXT_INFO (Transact-SQL)

`docs/t-sql/statements/set-context-info-transact-sql.md`

### Syntax

```syntaxsql
SET CONTEXT_INFO { binary_str | @binary_var }
```

## SET CURSOR_CLOSE_ON_COMMIT (Transact-SQL)

`docs/t-sql/statements/set-cursor-close-on-commit-transact-sql.md`

### Syntax

```syntaxsql
SET CURSOR_CLOSE_ON_COMMIT { ON | OFF }
```

## SET DATEFIRST (Transact-SQL)

`docs/t-sql/statements/set-datefirst-transact-sql.md`

### Syntax for SQL Server and Azure SQL Database

```syntaxsql
SET DATEFIRST { number | @number_var }
```

### Syntax for Azure Synapse Analytics and Parallel Data Warehouse

```syntaxsql
SET DATEFIRST 7 ;
```

## SET DATEFORMAT (Transact-SQL)

`docs/t-sql/statements/set-dateformat-transact-sql.md`

### Syntax

```syntaxsql
SET DATEFORMAT { format | @format_var }
```

## SET DEADLOCK_PRIORITY (Transact-SQL)

`docs/t-sql/statements/set-deadlock-priority-transact-sql.md`

### Syntax

```syntaxsql
SET DEADLOCK_PRIORITY { LOW | NORMAL | HIGH | <numeric-priority> | @deadlock_var | @deadlock_intvar }

<numeric-priority> ::= { -10 | -9 | -8 | ... | 0 | ... | 8 | 9 | 10 }
```

## SET FIPS_FLAGGER (Transact-SQL)

`docs/t-sql/statements/set-fips-flagger-transact-sql.md`

### Syntax

```syntaxsql
SET FIPS_FLAGGER ( 'level' |  OFF )
```

## SET FMTONLY (Transact-SQL)

`docs/t-sql/statements/set-fmtonly-transact-sql.md`

### Syntax

```syntaxsql
SET FMTONLY { ON | OFF }
```

## SET FORCEPLAN (Transact-SQL)

`docs/t-sql/statements/set-forceplan-transact-sql.md`

### Syntax

```syntaxsql
SET FORCEPLAN { ON | OFF }
```

## SET IDENTITY_INSERT (Transact-SQL)

`docs/t-sql/statements/set-identity-insert-transact-sql.md`

### Syntax

```syntaxsql
SET IDENTITY_INSERT [ [ database_name . ] schema_name . ] table_name { ON | OFF }
```

```syntaxsql
SET IDENTITY_INSERT [ schema_name. ] table_name { ON | OFF }
```

## SET IMPLICIT_TRANSACTIONS (Transact-SQL)

`docs/t-sql/statements/set-implicit-transactions-transact-sql.md`

### Syntax

```syntaxsql
SET IMPLICIT_TRANSACTIONS { ON | OFF }
```

## SET LANGUAGE (Transact-SQL)

`docs/t-sql/statements/set-language-transact-sql.md`

### Syntax

```syntaxsql
SET LANGUAGE { [ N ] 'language' | @language_var }
```

## SET LOCK_TIMEOUT (Transact-SQL)

`docs/t-sql/statements/set-lock-timeout-transact-sql.md`

### Syntax

```syntaxsql
SET LOCK_TIMEOUT timeout_period
```

## SET NOCOUNT (Transact-SQL)

`docs/t-sql/statements/set-nocount-transact-sql.md`

### Syntax

```syntaxsql
SET NOCOUNT { ON | OFF }
```

## SET NOEXEC (Transact-SQL)

`docs/t-sql/statements/set-noexec-transact-sql.md`

### Syntax

```syntaxsql
SET NOEXEC { ON | OFF }
```

## SET NUMERIC_ROUNDABORT (Transact-SQL)

`docs/t-sql/statements/set-numeric-roundabort-transact-sql.md`

### Syntax

```syntaxsql
SET NUMERIC_ROUNDABORT { ON | OFF }
```

## SET OFFSETS (Transact-SQL)

`docs/t-sql/statements/set-offsets-transact-sql.md`

### Syntax

```syntaxsql
SET OFFSETS keyword_list { ON | OFF }
```

## SET PARSEONLY (Transact-SQL)

`docs/t-sql/statements/set-parseonly-transact-sql.md`

### Syntax

```syntaxsql
SET PARSEONLY { ON | OFF }
[ ; ]
```

## SET QUERY_GOVERNOR_COST_LIMIT (Transact-SQL)

`docs/t-sql/statements/set-query-governor-cost-limit-transact-sql.md`

### Syntax

```syntaxsql
SET QUERY_GOVERNOR_COST_LIMIT value
```

## SET QUOTED_IDENTIFIER (Transact-SQL)

`docs/t-sql/statements/set-quoted-identifier-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, serverless SQL pool in Azure Synapse Analytics, and Microsoft Fabric.

```syntaxsql
SET QUOTED_IDENTIFIER { ON | OFF }
```

> Syntax for Azure Synapse Analytics and Parallel Data Warehouse.

```syntaxsql
SET QUOTED_IDENTIFIER ON
```

## SET RECOMMENDATIONS (Transact-SQL)

`docs/t-sql/statements/set-recommendations-sql.md`

### Syntax

```syntaxsql
SET RECOMMENDATIONS { ON | OFF };
```

## SET REMOTE_PROC_TRANSACTIONS (Transact-SQL)

`docs/t-sql/statements/set-remote-proc-transactions-transact-sql.md`

### Syntax

```syntaxsql
SET REMOTE_PROC_TRANSACTIONS { ON | OFF }
```

## SET RESULT_SET_CACHING (Transact-SQL)

`docs/t-sql/statements/set-result-set-caching-transact-sql.md`

### Syntax

```syntaxsql
SET RESULT_SET_CACHING { ON | OFF };
```

## SET ROWCOUNT (Transact-SQL)

`docs/t-sql/statements/set-rowcount-transact-sql.md`

### Syntax

```syntaxsql
SET ROWCOUNT { number | @number_var }
```

## SET SHOWPLAN_ALL (Transact-SQL)

`docs/t-sql/statements/set-showplan-all-transact-sql.md`

### Syntax

```syntaxsql
SET SHOWPLAN_ALL { ON | OFF }
```

## SET SHOWPLAN_TEXT (Transact-SQL)

`docs/t-sql/statements/set-showplan-text-transact-sql.md`

### Syntax

```syntaxsql
SET SHOWPLAN_TEXT { ON | OFF }
```

## SET SHOWPLAN_XML (Transact-SQL)

`docs/t-sql/statements/set-showplan-xml-transact-sql.md`

### Syntax

```syntaxsql
SET SHOWPLAN_XML { ON | OFF }
```

## SET STATISTICS IO (Transact-SQL)

`docs/t-sql/statements/set-statistics-io-transact-sql.md`

### Syntax

```syntaxsql
SET STATISTICS IO { ON | OFF }
```

## SET STATISTICS PROFILE (Transact-SQL)

`docs/t-sql/statements/set-statistics-profile-transact-sql.md`

### Syntax

```syntaxsql
SET STATISTICS PROFILE { ON | OFF }
```

## SET STATISTICS TIME (Transact-SQL)

`docs/t-sql/statements/set-statistics-time-transact-sql.md`

### Syntax

```syntaxsql
SET STATISTICS TIME { ON | OFF }
```

## SET STATISTICS XML (Transact-SQL)

`docs/t-sql/statements/set-statistics-xml-transact-sql.md`

### Syntax

```syntaxsql
SET STATISTICS XML { ON | OFF }
```

## SET TEXTSIZE (Transact-SQL)

`docs/t-sql/statements/set-textsize-transact-sql.md`

### Syntax

```syntaxsql
SET TEXTSIZE { number }
```

## SET TRANSACTION ISOLATION LEVEL (Transact-SQL)

`docs/t-sql/statements/set-transaction-isolation-level-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, and SQL database in Microsoft Fabric.

```syntaxsql
SET TRANSACTION ISOLATION LEVEL
    { READ UNCOMMITTED
    | READ COMMITTED
    | REPEATABLE READ
    | SNAPSHOT
    | SERIALIZABLE
    }
```

> Syntax for Azure Synapse Analytics and Parallel Data Warehouse.

```syntaxsql
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
```

## SET XACT_ABORT (Transact-SQL)

`docs/t-sql/statements/set-xact-abort-transact-sql.md`

### Syntax

```syntaxsql
SET XACT_ABORT { ON | OFF }
```

## SETUSER (Transact-SQL)

`docs/t-sql/statements/setuser-transact-sql.md`

### Syntax

```syntaxsql
SETUSER [ 'username' [ WITH NORESET ] ]
```

## SQL Server Collation Name (Transact-SQL)

`docs/t-sql/statements/sql-server-collation-name-transact-sql.md`

### Syntax

```syntaxsql
<SQL_collation_name> :: =
SQL_SortRules[_Pref]_CPCodepage_<ComparisonStyle>

<ComparisonStyle> ::=
_CaseSensitivity_AccentSensitivity | _BIN
```

## TRUNCATE TABLE (Transact-SQL)

`docs/t-sql/statements/truncate-table-transact-sql.md`

### Syntax

> Syntax for SQL Server, Azure SQL Database, Fabric SQL database

```syntaxsql
TRUNCATE TABLE
    { database_name.schema_name.table_name | schema_name.table_name | table_name }
    [ WITH ( PARTITIONS ( { <partition_number_expression> | <range> }
    [ , ...n ] ) ) ]
[ ; ]

<range> ::=
<partition_number_expression> TO <partition_number_expression>
```

> Syntax for Microsoft Fabric, Azure Synapse Analytics, and Parallel Data Warehouse.

```syntaxsql
TRUNCATE TABLE { database_name.schema_name.table_name | schema_name.table_name | table_name }
[ ; ]
```

## UPDATE STATISTICS (Transact-SQL)

`docs/t-sql/statements/update-statistics-transact-sql.md`

### Syntax

> Syntax for SQL Server and Azure SQL Database.

```syntaxsql
UPDATE STATISTICS table_or_indexed_view_name
    [
        {
            { index_or_statistics__name }
          | ( { index_or_statistics_name } [ , ...n ] )
                }
    ]
    [ WITH
        [
            FULLSCAN
              [ [ , ] PERSIST_SAMPLE_PERCENT = { ON | OFF } ]
            | SAMPLE number { PERCENT | ROWS }
              [ [ , ] PERSIST_SAMPLE_PERCENT = { ON | OFF } ]
            | RESAMPLE
              [ ON PARTITIONS ( { <partition_number> | <range> } [ , ...n ] ) ]
            | <update_stats_stream_option> [ , ...n ]
        ]
        [ [ , ] [ ALL | COLUMNS | INDEX ]
        [ [ , ] NORECOMPUTE ]
        [ [ , ] INCREMENTAL = { ON | OFF } ]
        [ [ , ] MAXDOP = max_degree_of_parallelism ]
        [ [ , ] AUTO_DROP = { ON | OFF } ]
    ] ;

<update_stats_stream_option> ::=
    [ STATS_STREAM = stats_stream ]
    [ ROWCOUNT = numeric_constant ]
    [ PAGECOUNT = numeric_constant ]
```

> Syntax for Azure Synapse Analytics and Parallel Data Warehouse.

```syntaxsql
UPDATE STATISTICS [ schema_name . ] table_name
    [ ( { statistics_name | index_name } ) ]
    [ WITH
       {
              FULLSCAN
            | SAMPLE number PERCENT
            | RESAMPLE
        }
    ]
[;]
```

> Syntax for Microsoft Fabric.

```syntaxsql
UPDATE STATISTICS [ schema_name . ] table_name
    [ ( { statistics_name } ) ]
    [ WITH
       {
              FULLSCAN
            | SAMPLE number PERCENT
        }
    ]
[;]
```

## Windows collation name (Transact-SQL)

`docs/t-sql/statements/windows-collation-name-transact-sql.md`

### Syntax

```syntaxsql
<Windows_collation_name> ::=
<CollationDesignator>_<ComparisonStyle>

<ComparisonStyle> ::=
{ <CaseSensitivity>_<AccentSensitivity> [ _<KanatypeSensitive> ] [ _<WidthSensitive> ] [ _<VariationSelectorSensitive> ]
}
| { _UTF8 }
| { _BIN | _BIN2 }
```

## delete (XML DML)

`docs/t-sql/xml/delete-xml-dml.md`

### Syntax

```syntaxsql
delete Expression
```

## exist() Method (xml Data Type)

`docs/t-sql/xml/exist-method-xml-data-type.md`

### Syntax

```syntaxsql
exist (XQuery)
```

## insert (XML DML)

`docs/t-sql/xml/insert-xml-dml.md`

### Syntax

```syntaxsql
insert Expression1 (
{as first | as last} into | after | before
Expression2
)
```

## modify() Method (xml Data Type)

`docs/t-sql/xml/modify-method-xml-data-type.md`

### Syntax

```syntaxsql
modify (XML_DML)
```

## nodes() Method (xml Data Type)

`docs/t-sql/xml/nodes-method-xml-data-type.md`

### Syntax

```syntaxsql
nodes (XQuery) as Table(Column)
```

## query() Method (xml Data Type)

`docs/t-sql/xml/query-method-xml-data-type.md`

### Syntax

```syntaxsql
query ('XQuery')
```

## replace value of (XML DML)

`docs/t-sql/xml/replace-value-of-xml-dml.md`

### Syntax

```syntaxsql
replace value of Expression1
with Expression2
```

## value() method (xml data type)

`docs/t-sql/xml/value-method-xml-data-type.md`

### Syntax

```syntaxsql
value ( XQuery , SQLType )
```

## WITH XMLNAMESPACES (Transact-SQL)

`docs/t-sql/xml/with-xmlnamespaces.md`

### Syntax

```syntaxsql
WITH XMLNAMESPACES ( <XML namespace declaration item>
[ { , <XML namespace declaration item> }...] )

<XML namespace declaration item> ::=
<xml_namespace_uri> AS <xml_namespace_prefix>
| <XML default namespace declaration item>
<xml_namespace_uri> ::= <character string literal>
```

```syntaxsql
<xml_namespace_prefix> ::= <identifier>
```

```syntaxsql
<XML default namespace declaration item> ::=
DEFAULT <xml_namespace_uri>
```

## xml_schema_namespace (Transact-SQL)

`docs/t-sql/xml/xml-schema-namespace.md`

### Syntax

```syntaxsql
xml_schema_namespace( Relational_schema , XML_schema_collection_name , [ Namespace ] )
```

## xml (Transact-SQL)

`docs/t-sql/xml/xml-transact-sql.md`

### Syntax

```syntaxsql
xml [ ( [ CONTENT | DOCUMENT ] xml_schema_collection ) ]
```
