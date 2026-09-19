Own and Total in ms over 1520 rounds (20 warm-up + timed), dotTrace Sampling / ThreadTime (15.6 ms samples);
a group's Total adds nested calls twice, so read Own to compare, Total for the biggest single call.

| group | plain Own | located Own | delta Own | plain Own/round | located Own/round | plain Total | located Total |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| a  Ways.* (Begin/Push/End/Open/Retry/Next/Put ...) | 250 | 283 | +33 | 0.16 | 0.19 | 250 | 283 |
| b1 Materialize_* | 18284 | 24330 | +6046 | 12.03 | 16.01 | 42048 | 54283 |
| b2 Construct_* | 1889 | 2026 | +137 | 1.24 | 1.33 | 2910 | 3175 |
| c  Locate / SqlSpan / ISqlSpan | 0 | 32 | +32 | 0.00 | 0.02 | 0 | 32 |
| d  Read_* (the reader) | 8207 | 8767 | +560 | 5.40 | 5.77 | 119536 | 147148 |
| f  Recognize_* / Scan* / Text_* (the recognizer and the lexer) | 2142 | 2579 | +437 | 1.41 | 1.70 | 5283 | 6472 |
| g  DirectValues / ValueTable (value storage) | 2658 | 2501 | -157 | 1.75 | 1.65 | 5502 | 5846 |
| h  TryParseStatement itself | 6000 | 6422 | +422 | 3.95 | 4.22 | 43266 | 51813 |
| all functions of TransactSqlParser (Own) | 39478 | 47472 | +7994 | 25.97 | 31.23 | | |
| everything else (Own: the corpus setup, the framework, [Native or optimized code]) | 10319 | 11363 | +1044 | | | | |
| TryParseStatement Total | | | | | | 43266 | 51344 |

-- a  Ways.* (Begin/Push/End/Open/Retry/Next/Put ...): top functions by Own (plain | located)
   plain        78     78  DotGram.Sql.TransactSql.TransactSqlParser+Ways.Rent
   plain        47     47  DotGram.Sql.TransactSql.TransactSqlParser+Ways.Put
   plain        47     47  DotGram.Sql.TransactSql.TransactSqlParser+Ways.Collect
   plain        31     31  DotGram.Sql.TransactSql.TransactSqlParser+Ways.Return
   plain        31     31  DotGram.Sql.TransactSql.TransactSqlParser+Ways.End
   located      78     78  DotGram.Sql.TransactSql.TransactSqlParser+Located+Ways.Return
   located      63     63  DotGram.Sql.TransactSql.TransactSqlParser+Located+Ways.Put
   located      63     63  DotGram.Sql.TransactSql.TransactSqlParser+Located+Ways.Begin
   located      47     47  DotGram.Sql.TransactSql.TransactSqlParser+Located+Ways.Rent
   located      16     16  DotGram.Sql.TransactSql.TransactSqlParser+Located+Ways.Push
-- b1 Materialize_*: top functions by Own (plain | located)
   plain      6156   6656  DotGram.Sql.TransactSql.TransactSqlParser.<Materialize_DotGram_Dialect_Statement_Dialect_Direct>g__Materialize_DotGram_Dialect_Statement_Dialect_Direc
   plain      2844  22844  DotGram.Sql.TransactSql.TransactSqlParser.Materialize_DotGram_Dialect_Statement_Dialect_Direct
   plain      2219   2984  DotGram.Sql.TransactSql.TransactSqlParser.<Materialize_DotGram_Dialect_Statement_Dialect_Direct>g__Materialize_DotGram_Dialect_Statement_Dialect_Direc
   plain      1313   1641  DotGram.Sql.TransactSql.TransactSqlParser.<Materialize_DotGram_Dialect_Statement_Dialect_Direct>g__Materialize_DotGram_Dialect_Statement_Dialect_Direc
   plain      1047   1359  DotGram.Sql.TransactSql.TransactSqlParser.<Materialize_DotGram_Dialect_Statement_Dialect_Direct>g__Materialize_DotGram_Dialect_Statement_Dialect_Direc
   located    8578   9391  DotGram.Sql.TransactSql.TransactSqlParser+Located.<Materialize_DotGram_Dialect_Statement_Dialect_Direct>g__Materialize_DotGram_Dialect_Statement_Diale
   located    3594  29094  DotGram.Sql.TransactSql.TransactSqlParser+Located.Materialize_DotGram_Dialect_Statement_Dialect_Direct
   located    2953   3625  DotGram.Sql.TransactSql.TransactSqlParser+Located.<Materialize_DotGram_Dialect_Statement_Dialect_Direct>g__Materialize_DotGram_Dialect_Statement_Diale
   located    1641   1906  DotGram.Sql.TransactSql.TransactSqlParser+Located.<Materialize_DotGram_Dialect_Statement_Dialect_Direct>g__Materialize_DotGram_Dialect_Statement_Diale
   located    1219   1703  DotGram.Sql.TransactSql.TransactSqlParser+Located.<Materialize_DotGram_Dialect_Statement_Dialect_Direct>g__Materialize_DotGram_Dialect_Statement_Diale
-- b2 Construct_*: top functions by Own (plain | located)
   plain        78    141  DotGram.Sql.TransactSql.TransactSqlParser.Construct_FunctionCall_Dialect
   plain        31     94  DotGram.Sql.TransactSql.TransactSqlParser.Construct_IndexOrder
   plain        31     63  DotGram.Sql.TransactSql.TransactSqlParser.Construct_PrincipalStatement_Dialect_9
   plain        31     47  DotGram.Sql.TransactSql.TransactSqlParser.Construct_ColumnTrait_Dialect_6
   plain        31     47  DotGram.Sql.TransactSql.TransactSqlParser.Construct_AlterDatabaseStatement_Dialect_4
   located      94    109  DotGram.Sql.TransactSql.TransactSqlParser+Located.Construct_Sql92_ColumnReference_Dialect
   located      78    109  DotGram.Sql.TransactSql.TransactSqlParser+Located.Construct_TSqlTablePrimary_Dialect_5
   located      78     94  DotGram.Sql.TransactSql.TransactSqlParser+Located.Construct_OptionSetting_Dialect_6
   located      63    109  DotGram.Sql.TransactSql.TransactSqlParser+Located.Construct_FunctionCall_Dialect
   located      47     63  DotGram.Sql.TransactSql.TransactSqlParser+Located.Construct_TSqlValuePrimary_Dialect
-- c  Locate / SqlSpan / ISqlSpan: top functions by Own (plain | located)
   located      16     16  DotGram.Sql.Expression.Locate
   located      16     16  DotGram.Sql.Clause.Locate
   located       0      0  DotGram.Sql.Statement.Locate
-- d  Read_* (the reader): top functions by Own (plain | located)
   plain       594   2656  DotGram.Sql.TransactSql.TransactSqlParser+Reader_DotGram_Dialect_Statement_Dialect.Read_TSqlPrimaryCore_Dialect_Dialect_Statement_Dialect
   plain       484  22516  DotGram.Sql.TransactSql.TransactSqlParser+Reader_DotGram_Dialect_Statement_Dialect.Read_TSqlStatement_Dialect_Dialect_Statement_Dialect_Part0
   plain       375   2625  DotGram.Sql.TransactSql.TransactSqlParser+Reader_DotGram_Dialect_Statement_Dialect.Read_TSqlValueExpression_Dialect_Dialect_Statement_Dialect
   plain       297    859  DotGram.Sql.TransactSql.TransactSqlParser+Reader_DotGram_Dialect_Statement_Dialect.Read_FunctionCall_Dialect_Dialect_Statement_Dialect
   plain       250  10719  DotGram.Sql.TransactSql.TransactSqlParser+Reader_DotGram_Dialect_Statement_Dialect.Read_DefinitionStatement_Dialect_Dialect_Statement_Dialect
   located     703   3000  DotGram.Sql.TransactSql.TransactSqlParser+Located+Reader_DotGram_Dialect_Statement_Dialect.Read_TSqlPrimaryCore_Dialect_Dialect_Statement_Dialect
   located     422  26391  DotGram.Sql.TransactSql.TransactSqlParser+Located+Reader_DotGram_Dialect_Statement_Dialect.Read_TSqlStatement_Dialect_Dialect_Statement_Dialect_Part0
   located     328    813  DotGram.Sql.TransactSql.TransactSqlParser+Located+Reader_DotGram_Dialect_Statement_Dialect.Read_FunctionCall_Dialect_Dialect_Statement_Dialect
   located     281   2844  DotGram.Sql.TransactSql.TransactSqlParser+Located+Reader_DotGram_Dialect_Statement_Dialect.Read_TSqlValueExpression_Dialect_Dialect_Statement_Dialect
   located     219  12266  DotGram.Sql.TransactSql.TransactSqlParser+Located+Reader_DotGram_Dialect_Statement_Dialect.Read_DefinitionStatement_Dialect_Dialect_Statement_Dialect
-- f  Recognize_* / Scan* / Text_* (the recognizer and the lexer): top functions by Own (plain | located)
   plain       766    766  DotGram.Sql.TransactSql.TransactSqlParser.Scan_trivia_Seam
   plain       719    719  DotGram.Sql.TransactSql.TransactSqlParser.Recognize_DotGram_Dialect_Statement_Dialect_In
   plain       516    625  DotGram.Sql.TransactSql.TransactSqlParser.Text_DotGram_Dialect_Statement_Dialect
   plain       125    125  DotGram.Sql.TransactSql.TransactSqlParser.Scan
   plain        16   1109  DotGram.Sql.TransactSql.TransactSqlParser+Reader_DotGram_Dialect_Statement_Dialect.Recognize_Dialect_Statement_Dialect_Whole_Read
   located    1016   1016  DotGram.Sql.TransactSql.TransactSqlParser+Located.Scan_trivia_Seam
   located     984    984  DotGram.Sql.TransactSql.TransactSqlParser+Located.Recognize_DotGram_Dialect_Statement_Dialect_In
   located     406    641  DotGram.Sql.TransactSql.TransactSqlParser+Located.Text_DotGram_Dialect_Statement_Dialect
   located     109    109  DotGram.Sql.TransactSql.TransactSqlParser+Located.Scan
   located      16    188  DotGram.Sql.TransactSql.TransactSqlParser+Located.Tokenize_DotGram
-- g  DirectValues / ValueTable (value storage): top functions by Own (plain | located)
   plain      1359   3547  DotGram.Sql.TransactSql.TransactSqlParser+DirectValues.Return
   plain       750   1375  DotGram.Sql.TransactSql.TransactSqlParser+ValueTable`1.Clear
   plain       469    500  DotGram.Sql.TransactSql.TransactSqlParser+DirectValues.Room
   plain        16     16  DotGram.Sql.TransactSql.TransactSqlParser+ValueTable`1.get_Length
   plain        16     16  DotGram.Sql.TransactSql.TransactSqlParser+ValueTable`1.get_Item
   located    1391   4063  DotGram.Sql.TransactSql.TransactSqlParser+Located+DirectValues.Return
   located     734   1328  DotGram.Sql.TransactSql.TransactSqlParser+Located+ValueTable`1.Clear
   located     266    313  DotGram.Sql.TransactSql.TransactSqlParser+Located+DirectValues.Room
   located      31     31  DotGram.Sql.TransactSql.TransactSqlParser+Located+ValueTable`1.get_Item
   located      31     31  DotGram.Sql.TransactSql.TransactSqlParser+Located+DirectValues.Rent
-- h  TryParseStatement itself: top functions by Own (plain | located)
   plain      6000  43266  DotGram.Sql.TransactSql.TransactSqlParser.TryParseStatement
   located    6422  51344  DotGram.Sql.TransactSql.TransactSqlParser+Located.TryParseStatement
   located       0    469  DotGram.Sql.TransactSql.TransactSqlParser.TryParseStatement
