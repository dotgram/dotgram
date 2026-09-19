Own and Total in ms over 1020 rounds (20 warm-up + timed), dotTrace Sampling / ThreadTime (15.6 ms samples);
a group's Total adds nested calls twice, so read Own to compare, Total for the biggest single call.

| group | plain Own | located Own | delta Own | plain Own/round | located Own/round | plain Total | located Total |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| a  Ways.* (Begin/Push/End/Open/Retry/Next/Put ...) | 2204 | 1955 | -249 | 2.16 | 1.92 | 2360 | 2080 |
| b1 Materialize_* | 17907 | 21052 | +3145 | 17.56 | 20.64 | 56566 | 65203 |
| b2 Construct_* | 1095 | 1237 | +142 | 1.07 | 1.21 | 6470 | 8185 |
| c  Locate / SqlSpan / ISqlSpan | 0 | 1062 | +1062 | 0.00 | 1.04 | 0 | 1311 |
| d  Read_* (the reader) | 11055 | 10283 | -772 | 10.84 | 10.08 | 341004 | 357505 |
| f  Recognize_* / Scan* / Text_* (the recognizer and the lexer) | 12002 | 11359 | -643 | 11.77 | 11.14 | 117284 | 118270 |
| g  DirectValues / ValueTable (value storage) | 5955 | 4986 | -969 | 5.84 | 4.89 | 17955 | 15579 |
| h  TryParseStatement itself | 188 | 94 | -94 | 0.18 | 0.09 | 68906 | 69828 |
| all functions of TransactSqlParser (Own) | 50594 | 52498 | +1904 | 49.60 | 51.47 | | |
| everything else (Own: the corpus setup, the framework, [Native or optimized code]) | 22608 | 21722 | -886 | | | | |
| TryParseStatement Total | | | | | | 68906 | 69109 |

-- a  Ways.* (Begin/Push/End/Open/Retry/Next/Put ...): top functions by Own (plain | located)
   plain       703    703  DotGram.Sql.TransactSql.TransactSqlParser+Ways.Put
   plain       516    516  DotGram.Sql.TransactSql.TransactSqlParser+Ways.Begin
   plain       297    297  DotGram.Sql.TransactSql.TransactSqlParser+Ways.End
   plain       281    281  DotGram.Sql.TransactSql.TransactSqlParser+Ways.Put
   plain       219    359  DotGram.Sql.TransactSql.TransactSqlParser+Ways.Collect
   located     672    672  DotGram.Sql.TransactSql.TransactSqlParser+Located+Ways.Begin
   located     531    531  DotGram.Sql.TransactSql.TransactSqlParser+Located+Ways.Put
   located     266    359  DotGram.Sql.TransactSql.TransactSqlParser+Located+Ways.Collect
   located     188    188  DotGram.Sql.TransactSql.TransactSqlParser+Located+Ways.Put
   located     172    172  DotGram.Sql.TransactSql.TransactSqlParser+Located+Ways.End
-- b1 Materialize_*: top functions by Own (plain | located)
   plain      5859   7250  DotGram.Sql.TransactSql.TransactSqlParser.<Materialize_DotGram_Dialect_Statement_Dialect_Direct>g__Materialize_DotGram_Dialect_Statement_Dialect_Direc
   plain      3828  32094  DotGram.Sql.TransactSql.TransactSqlParser.Materialize_DotGram_Dialect_Statement_Dialect_Direct
   plain      2250   3641  DotGram.Sql.TransactSql.TransactSqlParser.<Materialize_DotGram_Dialect_Statement_Dialect_Direct>g__Materialize_DotGram_Dialect_Statement_Dialect_Direc
   plain      1078   2313  DotGram.Sql.TransactSql.TransactSqlParser.<Materialize_DotGram_Dialect_Statement_Dialect_Direct>g__Materialize_DotGram_Dialect_Statement_Dialect_Direc
   plain       750    750  DotGram.Sql.TransactSql.TransactSqlParser.<Materialize_DotGram_Dialect_Statement_Dialect_Direct>g__Materialize_DotGram_Dialect_Statement_Dialect_Direc
   located    7688  10031  DotGram.Sql.TransactSql.TransactSqlParser+Located.<Materialize_DotGram_Dialect_Statement_Dialect_Direct>g__Materialize_DotGram_Dialect_Statement_Diale
   located    3563  35891  DotGram.Sql.TransactSql.TransactSqlParser+Located.Materialize_DotGram_Dialect_Statement_Dialect_Direct
   located    2188   4234  DotGram.Sql.TransactSql.TransactSqlParser+Located.<Materialize_DotGram_Dialect_Statement_Dialect_Direct>g__Materialize_DotGram_Dialect_Statement_Diale
   located    1203   1719  DotGram.Sql.TransactSql.TransactSqlParser+Located.<Materialize_DotGram_Dialect_Statement_Dialect_Direct>g__Materialize_DotGram_Dialect_Statement_Diale
   located    1125   1859  DotGram.Sql.TransactSql.TransactSqlParser+Located.<Materialize_DotGram_Dialect_Statement_Dialect_Direct>g__Materialize_DotGram_Dialect_Statement_Diale
-- b2 Construct_*: top functions by Own (plain | located)
   plain        78    109  DotGram.Sql.TransactSql.TransactSqlParser.Construct_TSqlSelectSublist_Dialect_6
   plain        47    109  DotGram.Sql.TransactSql.TransactSqlParser.Construct_TSqlQuerySpecification_Dialect
   plain        31     63  DotGram.Sql.TransactSql.TransactSqlParser.Construct_TSqlDirectSelect_Dialect
   plain        31     47  DotGram.Sql.TransactSql.TransactSqlParser.Construct_Sql92_UnsignedLiteral_1
   plain        31     31  DotGram.Sql.TransactSql.TransactSqlParser.Construct_TSqlValueExpression_Dialect_1
   located      47    172  DotGram.Sql.TransactSql.TransactSqlParser+Located.Construct_TSqlStatement_Dialect
   located      47    141  DotGram.Sql.TransactSql.TransactSqlParser+Located.Construct_ColumnBody_Dialect_1
   located      47    109  DotGram.Sql.TransactSql.TransactSqlParser+Located.Construct_TSqlValuePrimary_Dialect
   located      31    156  DotGram.Sql.TransactSql.TransactSqlParser+Located.Construct_TSqlValueExpression_Dialect_1
   located      31    141  DotGram.Sql.TransactSql.TransactSqlParser+Located.Construct_TSqlQuerySpecification_Dialect
-- c  Locate / SqlSpan / ISqlSpan: top functions by Own (plain | located)
   located     297    359  DotGram.Sql.Expression.Locate
   located     281    359  DotGram.Sql.Clause.Locate
   located     234    281  DotGram.Sql.Statement.Locate
   located     156    156  DotGram.Sql.SqlSpan..ctor
   located      94    125  DotGram.Sql.Query.Locate
-- d  Read_* (the reader): top functions by Own (plain | located)
   plain       656   4219  DotGram.Sql.TransactSql.TransactSqlParser+Reader_DotGram_Dialect_Statement_Dialect.Read_TSqlPrimaryCore_Dialect_Dialect_Statement_Dialect
   plain       516  32328  DotGram.Sql.TransactSql.TransactSqlParser+Reader_DotGram_Dialect_Statement_Dialect.Read_TSqlStatement_Dialect_Dialect_Statement_Dialect_Part0
   plain       313  16375  DotGram.Sql.TransactSql.TransactSqlParser+Reader_DotGram_Dialect_Statement_Dialect.Read_DefinitionStatement_Dialect_Dialect_Statement_Dialect
   plain       250    625  DotGram.Sql.TransactSql.TransactSqlParser+Reader_DotGram_Dialect_Statement_Dialect.Read_TSqlIdentifier_Dialect_Statement_Dialect
   plain       188    688  DotGram.Sql.TransactSql.TransactSqlParser+Reader_DotGram_Dialect_Statement_Dialect.Read_TSqlQualifiedName_Dialect_Dialect_Statement_Dialect
   located     469  32859  DotGram.Sql.TransactSql.TransactSqlParser+Located+Reader_DotGram_Dialect_Statement_Dialect.Read_TSqlStatement_Dialect_Dialect_Statement_Dialect_Part0
   located     453   3828  DotGram.Sql.TransactSql.TransactSqlParser+Located+Reader_DotGram_Dialect_Statement_Dialect.Read_TSqlPrimaryCore_Dialect_Dialect_Statement_Dialect
   located     313    750  DotGram.Sql.TransactSql.TransactSqlParser+Located+Reader_DotGram_Dialect_Statement_Dialect.Read_TSqlIdentifier_Dialect_Statement_Dialect
   located     250    438  DotGram.Sql.TransactSql.TransactSqlParser+Located+Reader_DotGram_Dialect_Statement_Dialect.Read_TSqlIdentifier_Dialect_Statement_Dialect_Part0
   located     234    656  DotGram.Sql.TransactSql.TransactSqlParser+Located+Reader_DotGram_Dialect_Statement_Dialect.Read_TSqlQualifiedName_Dialect_Dialect_Statement_Dialect
-- f  Recognize_* / Scan* / Text_* (the recognizer and the lexer): top functions by Own (plain | located)
   plain      7813   7813  DotGram.Sql.TransactSql.TransactSqlParser.Scan
   plain      2391   4234  DotGram.Sql.TransactSql.TransactSqlParser.Scan_trivia_Seam
   plain       703    703  DotGram.Sql.TransactSql.TransactSqlParser.Recognize_DotGram_Dialect_Statement_Dialect_In
   plain       531    609  DotGram.Sql.TransactSql.TransactSqlParser.Text_DotGram_Dialect_Statement_Dialect
   plain       250  12375  DotGram.Sql.TransactSql.TransactSqlParser.Tokenize_DotGram
   located    6906   6906  DotGram.Sql.TransactSql.TransactSqlParser+Located.Scan
   located    2688   4063  DotGram.Sql.TransactSql.TransactSqlParser+Located.Scan_trivia_Seam
   located     734    734  DotGram.Sql.TransactSql.TransactSqlParser+Located.Recognize_DotGram_Dialect_Statement_Dialect_In
   located     359  11375  DotGram.Sql.TransactSql.TransactSqlParser+Located.Tokenize_DotGram
   located     234  57391  DotGram.Sql.TransactSql.TransactSqlParser+Located.Recognize_Dialect_Statement_Dialect_Whole
-- g  DirectValues / ValueTable (value storage): top functions by Own (plain | located)
   plain      2125   6344  DotGram.Sql.TransactSql.TransactSqlParser+ValueTable`1.Clear
   plain      1594   1594  DotGram.Sql.TransactSql.TransactSqlParser+ValueTable`1.get_First
   plain      1094   1953  DotGram.Sql.TransactSql.TransactSqlParser+DirectValues.Room
   plain       625   7516  DotGram.Sql.TransactSql.TransactSqlParser+DirectValues.Return
   plain       438    438  DotGram.Sql.TransactSql.TransactSqlParser+ValueTable`1.get_Length
   located    1641   5547  DotGram.Sql.TransactSql.TransactSqlParser+Located+ValueTable`1.Clear
   located    1188   1922  DotGram.Sql.TransactSql.TransactSqlParser+Located+DirectValues.Room
   located    1156   1156  DotGram.Sql.TransactSql.TransactSqlParser+Located+ValueTable`1.get_First
   located     625   6547  DotGram.Sql.TransactSql.TransactSqlParser+Located+DirectValues.Return
   located     313    313  DotGram.Sql.TransactSql.TransactSqlParser+Located+ValueTable`1.get_Length
-- h  TryParseStatement itself: top functions by Own (plain | located)
   plain       188  68906  DotGram.Sql.TransactSql.TransactSqlParser.TryParseStatement
   located      94  69109  DotGram.Sql.TransactSql.TransactSqlParser+Located.TryParseStatement
   located       0    719  DotGram.Sql.TransactSql.TransactSqlParser.TryParseStatement
