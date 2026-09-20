round 1 base ce8102ec 19:48:23
round 1 head 1295b475 19:49:57
round 2 base ce8102ec 19:51:30
round 2 head 1295b475 19:53:03
round 3 base ce8102ec 19:54:34
round 3 head 1295b475 19:56:08

Generator time, head 1295b475 against base ce8102ec, median of 3 alternating rounds (same cores):

| host | base ms (min-max) | head ms (min-max) | head / base |
| --- | ---: | ---: | ---: |
| DotGram.Examples.Expressions.ArithmeticTree | 18 (17-18) | 18 (18-19) | 1.04x |
| DotGram.Examples.Expressions.Calculator | 198 (196-201) | 208 (206-210) | 1.06x |
| DotGram.Examples.Expressions.ClampedExample | 6 (6-7) | 7 (6-7) | 1.04x |
| DotGram.Examples.Expressions.LocaleNumber | 4 (4-4) | 5 (4-5) | 1.09x |
| DotGram.Examples.Feeds.FeedReader | 33 (32-33) | 34 (33-34) | 1.03x |
| DotGram.Examples.Feeds.LoggingFeedReader | 7 (6-7) | 7 (6-7) | 1.03x |
| DotGram.Examples.Feeds.RecoveringFeedReader | 9 (9-9) | 10 (9-10) | 1.05x |
| DotGram.Examples.Feeds.StockCountReader | 5 (5-5) | 6 (5-6) | 1.04x |
| DotGram.Examples.Feeds.StreamingFeedReader | 10 (10-11) | 10 (10-11) | 1.03x |
| DotGram.Examples.Formats.Config | 15 (15-16) | 16 (15-16) | 1.03x |
| DotGram.Examples.Formats.Config.Located | 13 (12-13) | 13 (13-13) | 1.02x |
| DotGram.Examples.Formats.FileNames | 5 (5-6) | 5 (5-5) | 1.04x |
| DotGram.Examples.Formats.FixedWidth | 8 (8-8) | 9 (9-9) | 1.06x |
| DotGram.Examples.Formats.FixParser | 4 (3-4) | 4 (4-4) | 0.99x |
| DotGram.Examples.Formats.HttpParser | 14 (4-14) | 5 (4-14) | 0.33x |
| DotGram.Examples.Formats.IniParser | 6 (5-6) | 6 (5-7) | 1.12x |
| DotGram.Examples.Formats.JsonParser | 8 (8-16) | 9 (9-27) | 1.11x |
| DotGram.Examples.Formats.Links | 9 (9-9) | 9 (9-9) | 1.02x |
| DotGram.Examples.Formats.MarkdownParser | 7 (6-7) | 7 (7-7) | 1.01x |
| DotGram.Examples.Formats.MetricsLine | 15 (15-15) | 15 (15-16) | 1.01x |
| DotGram.Examples.Formats.Netstrings | 3 (3-3) | 3 (3-3) | 1.02x |
| DotGram.Examples.Formats.TypedCsv | 10 (10-10) | 10 (10-10) | 1.03x |
| DotGram.Examples.Formats.XmlParser | 7 (6-7) | 7 (7-7) | 1.04x |
| DotGram.Examples.Formats.YamlLite | 4 (4-4) | 4 (4-5) | 1.02x |
| DotGram.Examples.Languages.Filter | 10 (10-10) | 11 (11-11) | 1.03x |
| DotGram.Examples.Languages.FilterFile | 9 (8-9) | 9 (9-9) | 1.04x |
| DotGram.Examples.Languages.Filters | 22 (22-25) | 20 (17-21) | 0.92x |
| DotGram.Examples.Languages.GramGrammar | 207 (198-208) | 181 (178-188) | 0.88x |
| DotGram.Examples.Languages.Lexemes | 5 (5-21) | 19 (5-22) | 3.73x |
| DotGram.Examples.Languages.Scoped | 16 (15-16) | 16 (16-17) | 1.04x |
| DotGram.Examples.Languages.Selectors | 16 (16-17) | 15 (15-15) | 0.90x |
| DotGram.Examples.Languages.SettingsFile | 17 (17-17) | 18 (17-18) | 1.05x |
| DotGram.Examples.Languages.SqlDialect | 7 (7-7) | 7 (7-7) | 1.09x |
| DotGram.Examples.Languages.SqlReadOnly | 19 (18-19) | 19 (19-19) | 1.04x |
| DotGram.Examples.Languages.TokenizedQuery | 88 (87-91) | 94 (94-96) | 1.07x |
| DotGram.Sql.Standard.Sql92Parser | 773 (757-789) | 794 (735-814) | 1.03x |
| DotGram.Sql.Standard.SqlStandardParser | 3,605 (3,569-3,777) | 3,589 (3,578-3,599) | 1.00x |
| DotGram.Sql.TransactSql.TransactSqlParser | 4,549 (4,505-4,624) | 4,537 (4,363-4,716) | 1.00x |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | 2,308 (2,275-2,417) | 2,276 (2,264-2,328) | 0.99x |

No host moved by more than the tolerance.
