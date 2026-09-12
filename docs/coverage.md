# How much of the T-SQL reference the grammar reads

Every example of every page of the Transact-SQL reference in
[MicrosoftDocs/sql-docs](https://github.com/MicrosoftDocs/sql-docs), `docs/t-sql`, at
commit `0a5380c14e9b504e320cca4f758ce4f46886f087` (2025-05-08), cut into statements and asked of SQL Server 2025 under
`SET PARSEONLY ON` at every compatibility level from 100 to 170, and of
`TransactSql.gram`'s parser for the same level. Written by `--coverage`
(`benchmarks/DotGram.Benchmarks/Coverage.cs`); run again rather than edited.

© Microsoft Corporation. The documentation is licensed under
[Creative Commons Attribution 4.0 International](https://creativecommons.org/licenses/by/4.0/).
Changed from the original: of each page only its title, its path and one statement of its
examples, flattened to a line, are kept.

The latest level decides a statement's column. **Both** read it. **Work**: the engine
reads it and the grammar does not. **Defects**: the grammar reads what the engine refuses.
**Levels**: both read it at 170 and part at an older level. **Other**: the grammar reads
it, and it is another product's, by the range the page or the example is marked for or
by the engine's answer. **Neither** reads it. **Read** is both over both, work and
defects. What names `PARSEONLY` is left out, since `SET PARSEONLY OFF` would have the
rest run rather than read.

996 of 1163 pages have examples. 442 statements set an option of the session, and the connections were opened again after each.

| Section | pages | statements | read | both | work | defects | levels | other | neither | left out |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| **all** | 996 | 8338 | 99.8% | 7984 | 19 | 0 | 4 | 85 | 250 | 0 |
| [data-types](#data-types) | 29 | 304 | 100.0% | 291 | 0 | 0 | 0 | 0 | 13 | 0 |
| [database-console-commands](#database-console-commands) | 36 | 242 | 99.2% | 234 | 2 | 0 | 0 | 1 | 5 | 0 |
| [functions](#functions) | 324 | 2101 | 100.0% | 2082 | 0 | 0 | 3 | 2 | 17 | 0 |
| [includes](#includes) | 3 | 7 | 100.0% | 7 | 0 | 0 | 0 | 0 | 0 | 0 |
| [language-elements](#language-elements) | 86 | 868 | 99.5% | 850 | 4 | 0 | 0 | 0 | 14 | 0 |
| [queries](#queries) | 37 | 747 | 99.2% | 718 | 6 | 0 | 0 | 1 | 22 | 0 |
| [reference](#reference) | 3 | 32 | 100.0% | 32 | 0 | 0 | 0 | 0 | 0 | 0 |
| [spatial-geography](#spatial-geography) | 72 | 312 | 100.0% | 311 | 0 | 0 | 0 | 0 | 1 | 0 |
| [spatial-geometry](#spatial-geometry) | 74 | 357 | 100.0% | 357 | 0 | 0 | 0 | 0 | 0 | 0 |
| [statements](#statements) | 320 | 3163 | 99.8% | 2901 | 7 | 0 | 1 | 81 | 174 | 0 |
| [xml](#xml) | 12 | 205 | 100.0% | 201 | 0 | 0 | 0 | 0 | 4 | 0 |

## The work list, by what a statement begins with

| Statement | statements | pages | for example |
| --- | ---: | ---: | --- |
| IF | 4 | 1 | stops at ''' — `IF OBJECT_ID ('dbo.Table1', 'U') isn't NULL DROP TABLE dbo.Table1;` |
| ALTER AVAILABILITY GROUP | 3 | 2 | stops at AVAILABILITY — `ALTER AVAILABILITY GROUP AccountsAG JOIN;` |
| SET | 3 | 1 | stops at AUTOCOMMIT — `SET AUTOCOMMIT ON;` |
| USE | 3 | 2 | stops at COMPUTE — `USE UserDbSales; DBCC FREEPROCCACHE (COMPUTE) WITH NO_INFOMSGS;` |
| SELECT | 2 | 2 | stops at DIAGNOSTICS — `-- Determine the session_id of your current session SELECT TOP 1 session_id();  -- ...` |
| ALTER TABLE | 1 | 1 | stops at WITH — `ALTER TABLE dbo.doc_exf ADD AddDate smalldatetime NULL CONSTRAINT AddDateDflt DEFAU...` |
| CREATE DATABASE SCOPED | 1 | 1 | stops at ')' — `CREATE DATABASE SCOPED CREDENTIAL AccessAzureInvoices WITH IDENTITY = 'SHARED ACCES...` |
| CREATE TABLE | 1 | 1 | stops at ''' — `CREATE TABLE #temptable (col1 int);  INSERT INTO #temptable VALUES (10);  SELECT co...` |
| INSERT | 1 | 1 | stops at WITH — `INSERT INTO loan_applications (c1, c2, c3, c4, score) SELECT d.c1, d.c2, d.c3, d.c4...` |

## What the engine answered the defects

None: the engine reads every statement this grammar reads.

## data-types

| Page | statements | read | work | defects | levels | other | first thing to do |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| binary and varbinary (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| char and varchar (Transact-SQL) | 15 | 100.0% | 0 | 0 | 0 | 0 |  |
| Data type conversion (Database Engine) | 10 | 100.0% | 0 | 0 | 0 | 0 |  |
| Data type synonyms (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| date (Transact-SQL) | 18 | 100.0% | 0 | 0 | 0 | 0 |  |
| datetime (Transact-SQL) | 21 | 100.0% | 0 | 0 | 0 | 0 |  |
| datetime2 (Transact-SQL) | 16 | 100.0% | 0 | 0 | 0 | 0 |  |
| datetimeoffset (Transact-SQL) | 27 | 100.0% | 0 | 0 | 0 | 0 |  |
| decimal and numeric (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| GetAncestor (Database Engine) | 15 | 100.0% | 0 | 0 | 0 | 0 |  |
| GetDescendant (Database Engine) | 24 | 100.0% | 0 | 0 | 0 | 0 |  |
| GetLevel (Database Engine) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| GetReparentedValue (Database Engine) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| GetRoot (Database Engine) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| int, bigint, smallint, and tinyint (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| IsDescendantOf (Database Engine) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| JSON data type (preview) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| money and smallmoney (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| Nondeterministic conversion of date literals | 14 | 100.0% | 0 | 0 | 0 | 0 |  |
| Parse (Database Engine) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| Precision, scale, and length (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| rowversion (Transact-SQL) | 10 | 100.0% | 0 | 0 | 0 | 0 |  |
| smalldatetime (Transact-SQL) | 17 | 100.0% | 0 | 0 | 0 | 0 |  |
| sql_variant (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| table (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| time (Transact-SQL) | 19 | 100.0% | 0 | 0 | 0 | 0 |  |
| ToString (Database Engine) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| uniqueidentifier (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| Vector data type (preview) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |

## database-console-commands

| Page | statements | read | work | defects | levels | other | first thing to do |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| DBCC FREEPROCCACHE (Transact-SQL) | 10 | 80.0% | 2 | 0 | 0 | 0 | stops at COMPUTE — `USE UserDbSales; DBCC FREEPROCCACHE (COMPUTE) WITH NO_INFOMSGS;` |
| DBCC CHECKALLOC (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC CHECKCATALOG (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC CHECKCONSTRAINTS (Transact-SQL) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC CHECKDB (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC CHECKFILEGROUP (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC CHECKIDENT (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC CHECKTABLE (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC CLEANTABLE (Transact-SQL) | 22 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC CLONEDATABASE (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC DBREINDEX (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC dllname (FREE) (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC FLUSHAUTHCACHE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC FREESESSIONCACHE (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC FREESYSTEMCACHE (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC HELP (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC INDEXDEFRAG (Transact-SQL) | 23 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC INPUTBUFFER (Transact-SQL) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC OPENTRAN (Transact-SQL) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC OUTPUTBUFFER (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC PDW_SHOWEXECUTIONPLAN (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC PDW_SHOWMATERIALIZEDVIEWOVERHEAD  (Transact-SQL) | 14 | 100.0% | 0 | 0 | 0 | 1 |  |
| DBCC PDW_SHOWPARTITIONSTATS (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC PDW_SHOWSPACEUSED (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC SHOW_STATISTICS (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC SHOWCONTIG (Transact-SQL) | 33 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC SHRINKDATABASE (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC SHRINKFILE (Transact-SQL) | 17 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC SHRINKLOG - Analytics Platform System (PDW) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC SQLPERF (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC TRACEOFF (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| Trace flags (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC TRACEON (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC TRACESTATUS (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC UPDATEUSAGE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBCC USEROPTIONS (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |

## functions

| Page | statements | read | work | defects | levels | other | first thing to do |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| OPENJSON (Transact-SQL) | 23 | 100.0% | 0 | 0 | 3 | 0 | levels 100 engine, 110 engine, 120 engine — `SELECT * FROM OPENJSON(@array) WITH (  month VARCHAR(3), temp int, month_id tinyint...` |
| ABS (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| ACOS (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| APP_NAME (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| APPLOCK_MODE (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| APPLOCK_TEST (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| APPROX_COUNT_DISTINCT (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| APPROX_PERCENTILE_CONT (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| APPROX_PERCENTILE_DISC (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| ASCII (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| ASIN (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| ASSEMBLYPROPERTY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| ASYMKEY_ID (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| ASYMKEYPROPERTY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| ATAN (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| ATN2 (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| AVG (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| BASE64_DECODE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| BASE64_ENCODE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| BINARY_CHECKSUM  (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| BIT_COUNT (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| CAST and CONVERT (Transact-SQL) | 60 | 100.0% | 0 | 0 | 0 | 0 |  |
| CEILING (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| CERT_ID (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| CERTENCODED (Transact-SQL) | 33 | 100.0% | 0 | 0 | 0 | 0 |  |
| CERTPRIVATEKEY (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| CERTPROPERTY (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| CHAR (Transact-SQL) | 14 | 100.0% | 0 | 0 | 0 | 0 |  |
| CHARINDEX (Transact-SQL) | 18 | 100.0% | 0 | 0 | 0 | 0 |  |
| CHECKSUM_AGG (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| CHECKSUM (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| COL_LENGTH (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| COL_NAME (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| COLLATIONPROPERTY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| TERTIARY_WEIGHTS (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| COLUMNPROPERTY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| COLUMNS_UPDATED (Transact-SQL) | 16 | 100.0% | 0 | 0 | 0 | 0 |  |
| COMPRESS (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| CONCAT (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| CONCAT_WS (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| CONNECTIONPROPERTY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@CONNECTIONS (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| CONTEXT_INFO (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| COS (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| COT (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| COUNT (Transact-SQL) | 14 | 100.0% | 0 | 0 | 0 | 0 |  |
| CPU_BUSY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| CRYPT_GEN_RANDOM (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| CUME_DIST (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| CURRENT_DATE (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| CURRENT_TIMESTAMP (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| CURRENT_TIMEZONE_ID (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| CURRENT_TIMEZONE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| CURRENT_TRANSACTION_ID (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| CURRENT_USER (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@CURSOR_ROWS (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| CURSOR_STATUS (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| DATABASE_PRINCIPAL_ID (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DATABASEPROPERTYEX (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| DATALENGTH (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DATE_BUCKET (Transact-SQL) | 27 | 100.0% | 0 | 0 | 0 | 0 |  |
| DATEADD (Transact-SQL) | 30 | 100.0% | 0 | 0 | 0 | 0 |  |
| DATEDIFF_BIG (Transact-SQL) | 38 | 100.0% | 0 | 0 | 0 | 0 |  |
| DATEDIFF (Transact-SQL) | 62 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@DATEFIRST (Transact-SQL) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| DATEFROMPARTS (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DATENAME (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DATEPART (Transact-SQL) | 10 | 100.0% | 0 | 0 | 0 | 0 |  |
| DATETIME2FROMPARTS (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DATETIMEFROMPARTS (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DATETIMEOFFSETFROMPARTS (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DATETRUNC (Transact-SQL) | 52 | 100.0% | 0 | 0 | 0 | 0 |  |
| DAY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DB_ID (Transact-SQL) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| DB_NAME (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| DBTS (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DECOMPRESS (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DECRYPTBYASYMKEY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DECRYPTBYCERT (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DECRYPTBYKEY (Transact-SQL) | 26 | 100.0% | 0 | 0 | 0 | 0 |  |
| DECRYPTBYKEYAUTOASYMKEY (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| DECRYPTBYKEYAUTOCERT (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| DECRYPTBYPASSPHRASE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DEGREES (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DENSE_RANK (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| DIFFERENCE (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| EDGE_ID_FROM_PARTS (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| EDIT_DISTANCE_SIMILARITY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| EDIT_DISTANCE (Transact-SQL) | 1 | — | 0 | 0 | 0 | 0 |  |
| ENCRYPTBYASYMKEY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| ENCRYPTBYCERT (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| ENCRYPTBYKEY (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| ENCRYPTBYPASSPHRASE (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| EOMONTH (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| ERROR_LINE (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| ERROR_MESSAGE (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| ERROR_NUMBER (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| ERROR_PROCEDURE (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| ERROR_SEVERITY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| ERROR_STATE (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@ERROR (Transact-SQL) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| EVENTDATA (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| EXP (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| FETCH_STATUS (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| FILE_ID (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| FILE_IDEX (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| FILE_NAME (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| FILEGROUP_ID (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| FILEGROUP_NAME (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| FILEGROUPPROPERTY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| FILEPROPERTY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| FILEPROPERTYEX (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| FIRST_VALUE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| FLOOR (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| FORMAT (Transact-SQL) | 20 | 100.0% | 0 | 0 | 0 | 0 |  |
| FORMATMESSAGE (Transact-SQL) | 18 | 100.0% | 0 | 0 | 0 | 0 |  |
| FULLTEXTCATALOGPROPERTY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| FULLTEXTSERVICEPROPERTY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| GENERATE_SERIES (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| GET_BIT (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| GETANSINULL (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| GETDATE (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| GETUTCDATE (Transact-SQL) | 18 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRAPH_ID_FROM_EDGE_ID (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRAPH_ID_FROM_NODE_ID (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| GROUPING_ID (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| GROUPING (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| HAS_DBACCESS (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| HAS_PERMS_BY_NAME (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| HASHBYTES (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| HOST_ID (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| HOST_NAME (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| IDENT_CURRENT (Transact-SQL) | 18 | 100.0% | 0 | 0 | 0 | 0 |  |
| IDENT_INCR (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| IDENT_SEED (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| IDENTITY (Function) (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@IDENTITY (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@IDLE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| INDEX_COL (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| INDEXKEY_PROPERTY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| INDEXPROPERTY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@IO_BUSY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| IS_MEMBER (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| IS_OBJECTSIGNED (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| IS_ROLEMEMBER (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| IS_SRVROLEMEMBER (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| ISDATE (Transact-SQL) | 34 | 100.0% | 0 | 0 | 0 | 0 |  |
| ISJSON (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| ISNULL (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| ISNUMERIC (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| JARO_WINKLER_DISTANCE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| JARO_WINKLER_SIMILARITY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| JSON_ARRAY (Transact-SQL) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| JSON_ARRAYAGG (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| JSON_MODIFY (Transact-SQL) | 34 | 100.0% | 0 | 0 | 0 | 0 |  |
| JSON_OBJECT (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| JSON_OBJECTAGG (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| JSON_PATH_EXISTS (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| JSON_QUERY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| JSON_VALUE (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| KEY_GUID (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| KEY_ID (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| KEY_NAME (Transact-SQL) | 15 | 100.0% | 0 | 0 | 0 | 0 |  |
| LAG (Transact-SQL) | 19 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@LANGID (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@LANGUAGE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| LAST_VALUE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| LEAD (Transact-SQL) | 19 | 100.0% | 0 | 0 | 0 | 0 |  |
| LEFT_SHIFT (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| LEFT (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| LEN (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@LOCK_TIMEOUT (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| LOG (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| LOG10 (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| CHOOSE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| GREATEST (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| IIF (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| LEAST (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| LOGINPROPERTY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| LOWER (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| LTRIM (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@MAX_CONNECTIONS (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@MAX_PRECISION (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| MAX (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| MIN_ACTIVE_ROWVERSION (Transact-SQL) | 27 | 100.0% | 0 | 0 | 0 | 0 |  |
| MIN (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| MONTH (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| NCHAR (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@NESTLEVEL (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| NEWID (Transact-SQL) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| NEWSEQUENTIALID (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| NEXT VALUE FOR (Transact-SQL) | 25 | 100.0% | 0 | 0 | 0 | 0 |  |
| NODE_ID_FROM_PARTS (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| NTILE (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| OBJECT_DEFINITION (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| OBJECT_ID_FROM_EDGE_ID (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| OBJECT_ID_FROM_NODE_ID (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| OBJECT_ID (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| OBJECT_NAME (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| OBJECT_SCHEMA_NAME (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| OBJECTPROPERTY (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| OBJECTPROPERTYEX (Transact-SQL) | 10 | 100.0% | 0 | 0 | 0 | 0 |  |
| ODBC Scalar Functions (Transact-SQL) | 38 | 100.0% | 0 | 0 | 0 | 0 |  |
| OPENDATASOURCE (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| OPENQUERY (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| OPENROWSET (Transact-SQL) | 37 | 100.0% | 0 | 0 | 0 | 2 |  |
| OPENXML (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@OPTIONS (Transact-SQL) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| ORIGINAL_LOGIN (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@PACK_RECEIVED (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@PACK_SENT (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@PACKET_ERRORS (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| PARSE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| PARSENAME (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| $PARTITION (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| PATINDEX (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| PERCENT_RANK (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| PERCENTILE_CONT (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| PERCENTILE_DISC (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| PERMISSIONS (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| PI (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| POWER (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@PROCID (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| PWDCOMPARE (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| QUOTENAME (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| RADIANS (Transact-SQL) | 16 | 100.0% | 0 | 0 | 0 | 0 |  |
| RAND (Transact-SQL) | 18 | 100.0% | 0 | 0 | 0 | 0 |  |
| RANK (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| Ranking Functions (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| REGEXP_COUNT (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| REGEXP_INSTR (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| REGEXP_LIKE (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| REGEXP_REPLACE (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| REGEXP_SUBSTR (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@REMSERVER (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| REPLACE (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| REPLICATE (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| REVERSE (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| RIGHT_SHIFT (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| RIGHT (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| ROUND (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| ROW_NUMBER (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@ROWCOUNT (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| RTRIM (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| SCHEMA_ID (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| SCHEMA_NAME (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| SCOPE_IDENTITY (Transact-SQL) | 18 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@SERVERNAME (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| SERVERPROPERTY (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@SERVICENAME (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| SESSION_CONTEXT (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| SESSION_ID (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| SESSION_USER (Transact-SQL) | 18 | 100.0% | 0 | 0 | 0 | 0 |  |
| SESSIONPROPERTY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET_BIT (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| SIGN (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| SIGNBYASYMKEY (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| SIGNBYCERT (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| SIN (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| SMALLDATETIMEFROMPARTS (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| SOUNDEX (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| SPACE (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@SPID (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| SQL_VARIANT_PROPERTY (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| SQRT (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| SQUARE (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| STATS_DATE (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| STDEV (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STDEVP (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| STR (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STRING_AGG (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| STRING_ESCAPE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| STRING_SPLIT (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| STUFF (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| SUBSTRING (Transact-SQL) | 17 | 100.0% | 0 | 0 | 0 | 0 |  |
| SUM (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| SUSER_ID (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| SUSER_NAME (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| SUSER_SID (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| SUSER_SNAME (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| SWITCHOFFSET (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| SYMKEYPROPERTY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| SYSDATETIME (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| SYSDATETIMEOFFSET (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| SYSTEM_USER (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| SYSUTCDATETIME (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| TAN (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| TEXTPTR (Transact-SQL) | 20 | 100.0% | 0 | 0 | 0 | 0 |  |
| TEXTVALID (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@TEXTSIZE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| TIMEFROMPARTS (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@TIMETICKS (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| TODATETIMEOFFSET (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@TOTAL_ERRORS (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@TOTAL_READ (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@TOTAL_WRITE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@TRANCOUNT (Transact-SQL) | 16 | 100.0% | 0 | 0 | 0 | 0 |  |
| TRANSLATE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| TRIGGER_NESTLEVEL (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| TRIM (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| TRY_CAST (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| TRY_CONVERT (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| TRY_PARSE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| TYPE_ID (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| TYPE_NAME (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| TYPEPROPERTY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| UNICODE (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| UNISTR (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| UPDATE() (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| UPPER (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| USER_ID (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| USER_NAME (Transact-SQL) | 10 | 100.0% | 0 | 0 | 0 | 0 |  |
| USER (Transact-SQL) | 16 | 100.0% | 0 | 0 | 0 | 0 |  |
| VAR (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| VARP (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| VECTOR_DISTANCE (Transact-SQL) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| VECTOR_NORM (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| VECTOR_NORMALIZE (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| VERIFYSIGNEDBYASYMKEY (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| VERIFYSIGNEDBYCERT (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| @@VERSION (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| VERSION (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| XACT_STATE (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| YEAR (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |

## includes

| Page | statements | read | work | defects | levels | other | first thing to do |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| docs/t-sql/includes/alter-workload-group.md | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| docs/t-sql/includes/create-workload-group.md | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| docs/t-sql/includes/drop-workload-group.md | 2 | 100.0% | 0 | 0 | 0 | 0 |  |

## language-elements

| Page | statements | read | work | defects | levels | other | first thing to do |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| Transactions (Azure Synapse Analytics and Microsoft Fabric) | 11 | 72.7% | 3 | 0 | 0 | 0 | stops at AUTOCOMMIT — `SET AUTOCOMMIT ON;` |
| CREATE DIAGNOSTICS SESSION (Transact-SQL) | 9 | 83.3% | 1 | 0 | 0 | 0 | stops at DIAGNOSTICS — `-- Determine the session_id of your current session SELECT TOP 1 session_id();  -- ...` |
| + (Addition) (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALL (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| AND (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| = (Assignment Operator) (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| BEGIN DISTRIBUTED TRANSACTION (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| BEGIN...END (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| BEGIN TRANSACTION (Transact-SQL) | 19 | 100.0% | 0 | 0 | 0 | 0 |  |
| BETWEEN (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| &amp;= (Bitwise AND assignment) (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| &amp; (Bitwise AND) (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| ^ (Bitwise Exclusive OR) (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| ~ (Bitwise NOT) (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| \| (Bitwise OR) (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| BREAK (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| CASE (Transact-SQL) | 17 | 100.0% | 0 | 0 | 0 | 0 |  |
| CLOSE (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| COALESCE (Transact-SQL) | 14 | 100.0% | 0 | 0 | 0 | 0 |  |
| -- (Comment) (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| COMMIT TRANSACTION (Transact-SQL) | 20 | 100.0% | 0 | 0 | 0 | 0 |  |
| \|\|= (Compound assignment) (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| Compound Operators (Transact-SQL) | 24 | 100.0% | 0 | 0 | 0 | 0 |  |
| DEALLOCATE (Transact-SQL) | 24 | 100.0% | 0 | 0 | 0 | 0 |  |
| DECLARE CURSOR (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| DECLARE @local_variable (Transact-SQL) | 26 | 100.0% | 0 | 0 | 0 | 0 |  |
| (Division Assignment) (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| (Division) (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| ELSE (IF...ELSE) (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| END (BEGIN...END) (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| = (Equals) (Transact-SQL) | 24 | 100.0% | 0 | 0 | 0 | 0 |  |
| EXECUTE (Transact-SQL) | 69 | 100.0% | 0 | 0 | 0 | 0 |  |
| EXISTS (Transact-SQL) | 10 | 100.0% | 0 | 0 | 0 | 0 |  |
| Expressions (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| FETCH (Transact-SQL) | 26 | 100.0% | 0 | 0 | 0 | 0 |  |
| &gt;= (Greater Than or Equal To) (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| &gt; (Greater Than) (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| IF...ELSE (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| IN (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| KILL QUERY NOTIFICATION SUBSCRIPTION | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| KILL STATS JOB (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| KILL (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| <= (Less Than or Equal To) (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| < (Less Than) (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| LIKE (Transact-SQL) | 27 | 100.0% | 0 | 0 | 0 | 0 |  |
| % (Modulus) (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| * (Multiplication) (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| &lt;&gt; (Not Equal To) (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| NOT (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| NULLIF (Transact-SQL) | 19 | 100.0% | 0 | 0 | 0 | 0 |  |
| OPEN (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| Operator Precedence (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| OR (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| Wildcard search (%) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| PRINT (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| RAISERROR (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| RECONFIGURE (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| RETURN (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| ROLLBACK TRANSACTION (Transact-SQL) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| SAVE TRANSACTION (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| :: (Scope Resolution) (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| SELECT @local_variable (Transact-SQL) | 14 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET @local_variable (Transact-SQL) | 54 | 100.0% | 0 | 0 | 0 | 0 |  |
| EXCEPT and INTERSECT (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| UNION (Transact-SQL) | 27 | 100.0% | 0 | 0 | 0 | 0 |  |
| Slash Star (Block Comment) (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| SOME \| ANY (Transact-SQL) | 10 | 100.0% | 0 | 0 | 0 | 0 |  |
| Backslash (Line Continuation) (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| SQL Server Utilities Statements - GO | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| = (String comparison or assignment) | 17 | 100.0% | 0 | 0 | 0 | 0 |  |
| += String concatenation | 20 | 100.0% | 0 | 0 | 0 | 0 |  |
| \|\| (String Concatenation) (Transact-SQL) | 17 | 100.0% | 0 | 0 | 0 | 0 |  |
| + (String concatenation) (Transact-SQL) | 18 | 100.0% | 0 | 0 | 0 | 0 |  |
| - (Subtraction) (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| THROW (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| Transact-SQL Syntax Conventions (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| TRY...CATCH (Transact-SQL) | 15 | 100.0% | 0 | 0 | 0 | 0 |  |
| - (Unary negative) (Transact-SQL) | 16 | 100.0% | 0 | 0 | 0 | 0 |  |
| + (Unary positive) (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| USE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| Variables (Transact-SQL) | 30 | 100.0% | 0 | 0 | 0 | 0 |  |
| WAITFOR (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| WHILE (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| [^] Wildcard to exclude characters | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| [ ] Wildcard to match characters | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| _ (Wildcard - Match One Character) (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |

## queries

| Page | statements | read | work | defects | levels | other | first thing to do |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| UPDATE (Transact-SQL) | 136 | 97.1% | 4 | 0 | 0 | 0 | stops at ''' — `IF OBJECT_ID ('dbo.Table1', 'U') isn't NULL DROP TABLE dbo.Table1;` |
| PREDICT (Transact-SQL) | 8 | 71.4% | 2 | 0 | 0 | 0 | stops at WITH — `SELECT d.*, p.Score FROM PREDICT(MODEL = @model, DATA = dbo.mytable AS d, RUNTIME =...` |
| Aliasing | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| AT TIME ZONE (Transact-SQL) | 17 | 100.0% | 0 | 0 | 0 | 0 |  |
| CONTAINS (Transact-SQL) | 40 | 100.0% | 0 | 0 | 0 | 0 |  |
| EXPLAIN (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| FREETEXT (Transact-SQL) | 14 | 100.0% | 0 | 0 | 0 | 0 |  |
| FROM clause plus JOIN, APPLY, PIVOT (T-SQL) | 38 | 100.0% | 0 | 0 | 0 | 0 |  |
| Using PIVOT and UNPIVOT | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| Join hints (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| Query hints (Transact-SQL) | 22 | 100.0% | 0 | 0 | 0 | 0 |  |
| Table Hints (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| IS [NOT] DISTINCT FROM (Transact-SQL) | 36 | 100.0% | 0 | 0 | 0 | 0 |  |
| IS NULL (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| MATCH (SQL Graph) | 14 | 100.0% | 0 | 0 | 0 | 0 |  |
| Nested Common Table Expression (CTE) in Fabric data warehousing | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| OPTION clause (Transact-SQL) | 14 | 100.0% | 0 | 0 | 0 | 0 |  |
| OUTPUT clause (Transact-SQL) | 67 | 100.0% | 0 | 0 | 0 | 0 |  |
| READTEXT (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| Search condition (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| SELECT Clause (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| SELECT examples (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| FOR Clause (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| GROUP BY (Transact-SQL) | 33 | 100.0% | 0 | 0 | 0 | 0 |  |
| HAVING (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| INTO Clause (Transact-SQL) | 17 | 100.0% | 0 | 0 | 0 | 0 |  |
| ORDER BY clause (Transact-SQL) | 53 | 100.0% | 0 | 0 | 0 | 0 |  |
| OVER Clause (Transact-SQL) | 18 | 100.0% | 0 | 0 | 0 | 0 |  |
| SELECT (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| WINDOW (Transact-SQL) | 21 | 100.0% | 0 | 0 | 0 | 0 |  |
| Subqueries | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| Table Value Constructor (Transact-SQL) | 20 | 100.0% | 0 | 0 | 0 | 0 |  |
| TOP (Transact-SQL) | 28 | 100.0% | 0 | 0 | 0 | 0 |  |
| UPDATETEXT (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| WHERE (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| WITH common_table_expression (Transact-SQL) | 28 | 100.0% | 0 | 0 | 0 | 1 |  |
| WRITETEXT (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |

## reference

| Page | statements | read | work | defects | levels | other | first thing to do |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| T-SQL Tutorial: Create and query database objects | 18 | 100.0% | 0 | 0 | 0 | 0 |  |
| T-SQL Tutorial: Configure permissions on db objects | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| T-SQL Tutorial: Delete database objects | 10 | 100.0% | 0 | 0 | 0 | 0 |  |

## spatial-geography

| Page | statements | read | work | defects | levels | other | first thing to do |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| AsBinaryZM (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| AsGml (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| AsTextZM (geography Data Type) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| BufferWithCurves (geography Data Type) | 17 | 100.0% | 0 | 0 | 0 | 0 |  |
| BufferWithTolerance (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| CollectionAggregate (geography Data Type) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| ConvexHullAggregate (geography Data Type) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| CurveToLineWithTolerance (geography Data Type) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| EnvelopeAggregate (geography Data Type) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| EnvelopeAngle (geography Data Type) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| EnvelopeCenter (geography Data Type) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| Filter (geography Data Type) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| GeomFromGML (geography Data Type) | 10 | 100.0% | 0 | 0 | 0 | 0 |  |
| HasM (geography Data Type) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| HasZ (geography Data Type) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| InstanceOf (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| IsValidDetailed (geography Data Type) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| Lat (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| Long (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| MakeValid (geography Data Type) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| MinDbCompatibilityLevel (geography Data Type) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| Null (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| NumRings (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| Parse (geography data type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| Point (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| ReorientObject (geography Data Type) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| RingN (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| ShortestLineTo (geography Data Type) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| geography (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| STArea (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STAsBinary (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STAsText (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STConvexHull (geography Data Type) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| STCurveN (geography Data Type) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| STCurveToLine (geography Data Type) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| STDifference (geography Data Type) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| STDistance (geography Data Type) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| STEndPoint (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STGeomCollFromText (geography Data Type) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| STGeomCollFromWKB (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STGeometryN (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STGeometryType (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STGeomFromText (geography data type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STGeomFromWKB (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STIntersection (geography Data Type) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| STIsClosed (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STIsEmpty (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STIsValid (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STLength (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STLineFromText (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STLineFromWKB (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STMLineFromText (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STMLineFromWKB (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STMPointFromText (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STMPointFromWKB (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STMPolyFromText (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STMPolyFromWKB (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STNumCurves (geography Data Type) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| STNumGeometries (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STNumPoints (geography Data Type) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| STPointFromText (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STPointFromWKB (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STPointN (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STPolyFromText (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STPolyFromWKB (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STSrid (geography Data Type) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| STStartPoint (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STSymDifference (geography Data Type) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| STUnion (geography Data Type) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| ToString (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| UnionAggregate (geography Data Type) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| Z (geography Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |

## spatial-geometry

| Page | statements | read | work | defects | levels | other | first thing to do |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| AsBinaryZM (geometry DataType) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| AsGml (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| AsTextZM (geometry Data Type) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| BufferWithCurves (geometry Data Type) | 21 | 100.0% | 0 | 0 | 0 | 0 |  |
| BufferWithTolerance (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| CollectionAggregate (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| ConvexHullAggregate (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| CurveToLineWithTolerance (geometry Data Type) | 14 | 100.0% | 0 | 0 | 0 | 0 |  |
| EnvelopeAggregate (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| Filter (geometry Data Type) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| GeomFromGml (geometry Data Type) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| HasM (geometry DataType) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| HasZ (geometry DataType) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| InstanceOf (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| IsValidDetailed (geometry DataType) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| M (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| MakeValid (geometry Data Type) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| MinDbCompatibilityLevel (geometry Data Type) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| Null (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| Parse (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| Point (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| Reduce (geometry Data Type) | 14 | 100.0% | 0 | 0 | 0 | 0 |  |
| ShortestLineTo (geometry Data Type) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| geometry (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| STArea (geometry Data Type) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| STAsBinary (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STAsText (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STBoundary (geometry Data Type) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| STConvexHull (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STCrosses (geometry Data Type) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| STCurveN (geometry Data Type) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| STCurveToLine (geometry Data Type) | 18 | 100.0% | 0 | 0 | 0 | 0 |  |
| STDifference (geometry Data Type) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| STExteriorRing (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STGeomCollFromText (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STGeomCollFromWKB (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STGeometryN (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STGeometryType (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STGeomFromText (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STGeomFromWKB (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STInteriorRingN (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STIntersection (geometry Data Type) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| STIsClosed (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STIsEmpty (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STIsRing (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STIsSimple (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STIsValid (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STLength (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STLineFromText (geometry Data Type) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| STLineFromWKB (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STMLineFromText (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STMLineFromWKB (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STMPointFromText (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STMPointFromWKB (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STMPolyFromText (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STMPolyFromWKB (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STNumCurves (geometry Data Type) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| STNumPoints (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STPointFromText (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STPointFromWKB (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STPointN (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STPointOnSurface (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STPolyFromText (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STPolyFromWKB (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STRelate (geometry Data Type) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| STSrid (geometry Data Type) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| STStartPoint (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STSymDifference (geometry Data Type) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| STTouches (geometry Data Type) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| STUnion (geometry Data Type) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| STX (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| STY (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| ToString (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| UnionAggregate (geometry Data Type) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |

## statements

| Page | statements | read | work | defects | levels | other | first thing to do |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| ALTER AVAILABILITY GROUP (Transact-SQL) | 3 | 0.0% | 2 | 0 | 0 | 0 | stops at AVAILABILITY — `ALTER AVAILABILITY GROUP AccountsAG JOIN;` |
| ALTER TABLE (Transact-SQL) | 141 | 99.2% | 1 | 0 | 0 | 4 | stops at WITH — `ALTER TABLE dbo.doc_exf ADD AddDate smalldatetime NULL CONSTRAINT AddDateDflt DEFAU...` |
| CREATE AVAILABILITY GROUP (Transact-SQL) | 2 | 0.0% | 1 | 0 | 0 | 0 | stops at AVAILABILITY — `ALTER AVAILABILITY GROUP [MyAg] ADD LISTENER 'MyAgListenerIvP6' ( WITH IP ( ('2001:...` |
| CREATE EXTERNAL DATA SOURCE (Transact-SQL) | 115 | 98.9% | 1 | 0 | 0 | 23 | stops at ')' — `CREATE DATABASE SCOPED CREDENTIAL AccessAzureInvoices WITH IDENTITY = 'SHARED ACCES...` |
| CREATE REMOTE TABLE AS SELECT (Parallel Data Warehouse) | 3 | 50.0% | 1 | 0 | 0 | 0 | stops at TABLE — `USE ssawPDW; CREATE REMOTE TABLE OrderReporting.Orders.MyOrdersTable AT ( 'Data Sou...` |
| CREATE TABLE AS SELECT (Azure Synapse Analytics and Microsoft Fabric) | 53 | 100.0% | 0 | 0 | 1 | 12 | levels 100 engine, 110 engine, 120 engine — `INSERT INTO Users (id, name, age, street, city) SELECT id, name, age, JSON_VALUE(ad...` |
| DROP TABLE (Transact-SQL) | 6 | 83.3% | 1 | 0 | 0 | 0 | stops at ''' — `CREATE TABLE #temptable (col1 int);  INSERT INTO #temptable VALUES (10);  SELECT co...` |
| ADD SENSITIVITY CLASSIFICATION (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| ADD SIGNATURE (Transact-SQL) | 41 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER APPLICATION ROLE (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER ASSEMBLY (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER ASYMMETRIC KEY (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER AUTHORIZATION (Transact-SQL) | 19 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER BROKER PRIORITY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER CERTIFICATE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER COLUMN ENCRYPTION KEY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER CREDENTIAL (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER CRYPTOGRAPHIC PROVIDER (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER DATABASE AUDIT SPECIFICATION (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER DATABASE ENCRYPTION KEY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER DATABASE SCOPED CONFIGURATION | 20 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER DATABASE SCOPED CREDENTIAL (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER DATABASE Compatibility Level (Transact-SQL) | 21 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER DATABASE Database Mirroring (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER DATABASE File and Filegroups | 69 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER DATABASE SET HADR (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER DATABASE SET Options (Transact-SQL) | 48 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER DATABASE (Transact-SQL) | 35 | 100.0% | 0 | 0 | 0 | 9 |  |
| ALTER EVENT SESSION (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER EXTERNAL DATA SOURCE (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 1 |  |
| ALTER EXTERNAL LANGUAGE (Transact-SQL) - SQL Server | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER EXTERNAL LIBRARY (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER EXTERNAL RESOURCE POOL (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER FULLTEXT CATALOG (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER FULLTEXT INDEX (Transact-SQL) | 14 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER FULLTEXT STOPLIST (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER INDEX (Selective XML Indexes) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER INDEX (Transact-SQL) | 58 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER LOGIN (Transact-SQL) | 42 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER MASTER KEY (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER MATERIALIZED VIEW (Transact-SQL) | 2 | — | 0 | 0 | 0 | 0 |  |
| ALTER MESSAGE TYPE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER PARTITION FUNCTION (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER PARTITION SCHEME (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER PROCEDURE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER QUEUE (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER REMOTE SERVICE BINDING (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER RESOURCE GOVERNOR (Transact-SQL) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER RESOURCE POOL (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER ROLE (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER ROUTE (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER SCHEMA (Transact-SQL) | 10 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER SEARCH PROPERTY LIST (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER SECURITY POLICY (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER SEQUENCE (Transact-SQL) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER SERVER AUDIT SPECIFICATION (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER SERVER AUDIT (Transact-SQL) | 17 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER SERVER CONFIGURATION (Transact-SQL) | 20 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER SERVER ROLE (Transact-SQL) | 10 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER SERVICE MASTER KEY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER SERVICE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER SYMMETRIC KEY (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER TABLE index_option (Transact-SQL) | 1 | — | 0 | 0 | 0 | 0 |  |
| ALTER TRIGGER (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER USER (Transact-SQL) | 19 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER VIEW (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| ALTER WORKLOAD GROUP (Transact-SQL) | 10 | 100.0% | 0 | 0 | 0 | 1 |  |
| ALTER XML SCHEMA COLLECTION (Transact-SQL) | 19 | 100.0% | 0 | 0 | 0 | 0 |  |
| BACKUP CERTIFICATE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| BACKUP MASTER KEY (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| BACKUP SERVICE MASTER KEY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| BACKUP SYMMETRIC KEY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| BACKUP (Transact-SQL) | 29 | 100.0% | 0 | 0 | 0 | 0 |  |
| BEGIN CONVERSATION TIMER (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| BEGIN DIALOG CONVERSATION (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| BULK INSERT (Transact-SQL) | 29 | 100.0% | 0 | 0 | 0 | 0 |  |
| CLOSE MASTER KEY (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| CLOSE SYMMETRIC KEY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| Collation precedence | 10 | 100.0% | 0 | 0 | 0 | 0 |  |
| COLLATE (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| COPY INTO (Transact-SQL) | 18 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE AGGREGATE (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE APPLICATION ROLE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE ASSEMBLY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE ASYMMETRIC KEY (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE BROKER PRIORITY (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE CERTIFICATE (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE COLUMN ENCRYPTION KEY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE COLUMN MASTER KEY (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE COLUMNSTORE INDEX (Transact-SQL) | 52 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE CONTRACT (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE CREDENTIAL (Transact-SQL) | 26 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE CRYPTOGRAPHIC PROVIDER (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE DATABASE AUDIT SPECIFICATION | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE DATABASE ENCRYPTION KEY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE DATABASE SCOPED CREDENTIAL (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE DATABASE (Transact-SQL) | 54 | 100.0% | 0 | 0 | 0 | 6 |  |
| CREATE DEFAULT (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE ENDPOINT (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE EVENT NOTIFICATION (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE EVENT SESSION (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE EXTERNAL FILE FORMAT (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 2 |  |
| CREATE EXTERNAL LANGUAGE (Transact-SQL) - SQL Server | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE EXTERNAL LIBRARY (Transact-SQL) - SQL Server | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE EXTERNAL RESOURCE POOL (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE EXTERNAL TABLE AS SELECT (CETAS) (Transact-SQL) | 49 | 100.0% | 0 | 0 | 0 | 10 |  |
| CREATE EXTERNAL TABLE (Transact-SQL) | 43 | 100.0% | 0 | 0 | 0 | 7 |  |
| CREATE FULLTEXT CATALOG (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE FULLTEXT INDEX (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE FULLTEXT STOPLIST (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE FUNCTION (Azure Synapse Analytics and Microsoft Fabric) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE FUNCTION (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE INDEX (Transact-SQL) | 66 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE LOGIN (Transact-SQL) | 48 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE MASTER KEY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE MATERIALIZED VIEW AS SELECT (Transact-SQL) creates a materialized view to persist the data returned from the view definition query and automatically gets updated as data changes in the underlying tables. | 18 | 100.0% | 0 | 0 | 0 | 2 |  |
| CREATE MESSAGE TYPE (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE PARTITION FUNCTION (Transact-SQL) | 14 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE PARTITION SCHEME (Transact-SQL) | 10 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE PROCEDURE (Transact-SQL) | 61 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE QUEUE (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE REMOTE SERVICE BINDING (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE RESOURCE POOL (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE ROLE (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE ROUTE (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE RULE (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE SCHEMA (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE SEARCH PROPERTY LIST (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE SECURITY POLICY (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE SELECTIVE XML INDEX (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE SEQUENCE (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE SERVER AUDIT SPECIFICATION (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE SERVER AUDIT (Transact-SQL) | 15 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE SERVER ROLE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE SERVICE (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE SPATIAL INDEX (Transact-SQL) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE STATISTICS (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE SYMMETRIC KEY (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE SYNONYM (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE TABLE AS CLONE OF | 4 | — | 0 | 0 | 0 | 0 |  |
| CREATE TABLE | 12 | — | 0 | 0 | 0 | 1 |  |
| CREATE TABLE (SQL Graph) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| IDENTITY (Property) (Transact-SQL) | 22 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE TABLE (Transact-SQL) | 69 | 100.0% | 0 | 0 | 0 | 1 |  |
| CREATE TRIGGER (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE TYPE (Transact-SQL) | 10 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE USER (Transact-SQL) | 42 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE VIEW (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE WORKLOAD Classifier (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE WORKLOAD GROUP (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 1 |  |
| CREATE XML INDEX (Selective XML Indexes) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE XML INDEX (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| CREATE XML SCHEMA COLLECTION (Transact-SQL) | 18 | 100.0% | 0 | 0 | 0 | 0 |  |
| DELETE (Transact-SQL) | 35 | 100.0% | 0 | 0 | 0 | 0 |  |
| DENY Availability Group Permissions | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DENY Database Permissions (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| DENY Database Principal Permissions | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| DENY Endpoint Permissions (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DENY Object Permissions (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| DENY Server Permissions (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DENY Server Principal Permissions (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DENY Symmetric Key Permissions (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DENY System Object Permissions (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DENY Type Permissions (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DENY XML Schema Collection Permissions | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DISABLE TRIGGER (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP AGGREGATE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP APPLICATION ROLE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP ASSEMBLY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP ASYMMETRIC KEY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP AVAILABILITY GROUP (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP BROKER PRIORITY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP CERTIFICATE (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP COLUMN ENCRYPTION KEY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP COLUMN MASTER KEY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP CONTRACT (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP CREDENTIAL (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP CRYPTOGRAPHIC PROVIDER (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP DATABASE AUDIT SPECIFICATION (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP DATABASE ENCRYPTION KEY (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP DATABASE SCOPED CREDENTIAL (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP DATABASE (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP DEFAULT (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP ENDPOINT (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP EVENT NOTIFICATION (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP EVENT SESSION (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP EXTERNAL DATA SOURCE (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP EXTERNAL FILE FORMAT (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP EXTERNAL LANGUAGE (Transact-SQL) - SQL Server | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP EXTERNAL LIBRARY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP EXTERNAL RESOURCE POOL (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP EXTERNAL TABLE (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP FULLTEXT INDEX (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP FULLTEXT STOPLIST (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP FUNCTION (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP INDEX (Selective XML Indexes) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP INDEX (Transact-SQL) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP LOGIN (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP MASTER KEY (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP MESSAGE TYPE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP PARTITION FUNCTION (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP PARTITION SCHEME (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP PROCEDURE (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP QUEUE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP REMOTE SERVICE BINDING (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP RESOURCE POOL (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP ROLE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP ROUTE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP RULE (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP SCHEMA (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP SEARCH PROPERTY LIST (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP SECURITY POLICY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP SENSITIVITY CLASSIFICATION (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP SEQUENCE (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP SERVER AUDIT SPECIFICATION (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP SERVER AUDIT (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP SERVER ROLE (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP SERVICE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP SIGNATURE (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP STATISTICS (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP SYMMETRIC KEY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP SYNONYM (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP TRIGGER (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP TYPE (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP USER (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP VIEW (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP WORKLOAD Classifier (Transact-SQL) | 1 | — | 0 | 0 | 0 | 1 |  |
| DROP WORKLOAD GROUP (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| DROP XML SCHEMA COLLECTION (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| ENABLE TRIGGER (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| END CONVERSATION (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| EXECUTE AS Clause (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| EXECUTE AS (Transact-SQL) | 21 | 100.0% | 0 | 0 | 0 | 0 |  |
| GET CONVERSATION GROUP (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| GET_TRANSMISSION_STATUS (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRANT Availability Group Permissions | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRANT Database Permissions (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRANT Database Principal Permissions | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRANT Endpoint Permissions (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRANT Full-Text Permissions (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRANT Object Permissions (Transact-SQL) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRANT Schema Permissions (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRANT Search Property List Permissions | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRANT Server Permissions (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRANT Server Principal Permissions (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRANT Symmetric Key Permissions (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRANT system object permissions (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRANT (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRANT Type Permissions (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRANT XML Schema Collection Permissions | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| INSERT (SQL Graph) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| INSERT (Transact-SQL) | 73 | 100.0% | 0 | 0 | 0 | 0 |  |
| MERGE (Transact-SQL) | 54 | 100.0% | 0 | 0 | 0 | 0 |  |
| MOVE CONVERSATION (Transact-SQL) | 1 | — | 0 | 0 | 0 | 0 |  |
| OPEN MASTER KEY (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| OPEN SYMMETRIC KEY (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| GRANT-DENY-REVOKE permissions | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| RECEIVE (Transact-SQL) | 10 | 100.0% | 0 | 0 | 0 | 0 |  |
| RENAME (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| RESTORE MASTER KEY (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| RESTORE SERVICE MASTER KEY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| RESTORE FILELISTONLY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| RESTORE HEADERONLY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| RESTORE (Transact-SQL) | 44 | 100.0% | 0 | 0 | 0 | 0 |  |
| RESTORE VERIFYONLY (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| RESTORE SYMMETRIC KEY (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| REVERT (Transact-SQL) | 24 | 100.0% | 0 | 0 | 0 | 0 |  |
| REVOKE Availability Group Permissions | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| REVOKE Database Permissions (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| REVOKE Database Principal Permissions | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| REVOKE Endpoint Permissions (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| REVOKE Object Permissions (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| REVOKE Server Permissions (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| REVOKE Server Principal Permissions | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| REVOKE Symmetric Key Permissions (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| REVOKE System Object Permissions (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| REVOKE (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| REVOKE Type Permissions (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| REVOKE XML Schema Collection Permissions | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| SEND (Transact-SQL) | 7 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET ANSI_DEFAULTS (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET ANSI_NULL_DFLT_OFF (Transact-SQL) | 23 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET ANSI_NULL_DFLT_ON (Transact-SQL) | 24 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET ANSI_NULLS (Transact-SQL) | 29 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET ANSI_PADDING (Transact-SQL) | 17 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET ANSI_WARNINGS (Transact-SQL) | 25 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET ARITHABORT (Transact-SQL) | 30 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET ARITHIGNORE (Transact-SQL) | 16 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET CONCAT_NULL_YIELDS_NULL (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET CONTEXT_INFO (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET CURSOR_CLOSE_ON_COMMIT (Transact-SQL) | 33 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET DATEFIRST (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET DATEFORMAT (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET DEADLOCK_PRIORITY (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET FMTONLY (Transact-SQL) | 16 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET FORCEPLAN (Transact-SQL) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET IDENTITY_INSERT (Transact-SQL) | 10 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET IMPLICIT_TRANSACTIONS (Transact-SQL) | 54 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET LANGUAGE (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET LOCK_TIMEOUT (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET NOCOUNT (Transact-SQL) | 9 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET NOEXEC (Transact-SQL) | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET NUMERIC_ROUNDABORT (Transact-SQL) | 40 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET QUOTED_IDENTIFIER (Transact-SQL) | 26 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET RECOMMENDATIONS (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET RESULT_SET_CACHING (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET ROWCOUNT (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET SHOWPLAN_ALL (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET SHOWPLAN_TEXT (Transact-SQL) | 8 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET SHOWPLAN_XML (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET STATISTICS IO (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET STATISTICS TIME (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET STATISTICS XML (Transact-SQL) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET TRANSACTION ISOLATION LEVEL (Transact-SQL) | 6 | 100.0% | 0 | 0 | 0 | 0 |  |
| SET XACT_ABORT (Transact-SQL) | 24 | 100.0% | 0 | 0 | 0 | 0 |  |
| SETUSER (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| SQL Server Collation Name (Transact-SQL) | 1 | 100.0% | 0 | 0 | 0 | 0 |  |
| TRUNCATE TABLE (Transact-SQL) | 14 | 100.0% | 0 | 0 | 0 | 0 |  |
| UPDATE STATISTICS (Transact-SQL) | 19 | 100.0% | 0 | 0 | 0 | 0 |  |
| Windows collation name (Transact-SQL) | 3 | 100.0% | 0 | 0 | 0 | 0 |  |

## xml

| Page | statements | read | work | defects | levels | other | first thing to do |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| Binding Relational Data Inside XML Data | 3 | 100.0% | 0 | 0 | 0 | 0 |  |
| delete (XML DML) | 25 | 100.0% | 0 | 0 | 0 | 0 |  |
| exist() Method (xml Data Type) | 20 | 100.0% | 0 | 0 | 0 | 0 |  |
| Guidelines for Using xml Data Type Methods | 12 | 100.0% | 0 | 0 | 0 | 0 |  |
| insert (XML DML) | 84 | 100.0% | 0 | 0 | 0 | 0 |  |
| nodes() Method (xml Data Type) | 13 | 100.0% | 0 | 0 | 0 | 0 |  |
| query() Method (xml Data Type) | 5 | 100.0% | 0 | 0 | 0 | 0 |  |
| replace value of (XML DML) | 25 | 100.0% | 0 | 0 | 0 | 0 |  |
| value() method (xml data type) | 11 | 100.0% | 0 | 0 | 0 | 0 |  |
| xml Data Type Methods | 1 | — | 0 | 0 | 0 | 0 |  |
| xml_schema_namespace (Transact-SQL) | 2 | 100.0% | 0 | 0 | 0 | 0 |  |
| xml (Transact-SQL) | 4 | 100.0% | 0 | 0 | 0 | 0 |  |
