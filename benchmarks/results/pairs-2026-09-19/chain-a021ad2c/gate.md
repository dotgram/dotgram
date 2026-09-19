round 1 base d032591f 09:55:50
round 1 head a021ad2c 09:57:22
round 2 base d032591f 09:58:49
round 2 head a021ad2c 10:00:21
round 3 base d032591f 10:01:48
round 3 head a021ad2c 10:03:16

Generator time, head a021ad2c against base d032591f, median of 3 alternating rounds (same cores):

| host | base ms (min-max) | head ms (min-max) | head / base |
| --- | ---: | ---: | ---: |
| DotGram.Examples.Expressions.ArithmeticTree | 13 (13-14) | 14 (13-14) | 1.02x |
| DotGram.Examples.Expressions.Calculator | 196 (193-201) | 205 (195-205) | 1.04x |
| DotGram.Examples.Expressions.ClampedExample | 7 (6-7) | 7 (7-7) | 1.03x |
| DotGram.Examples.Expressions.LocaleNumber | 5 (5-5) | 5 (5-5) | 1.04x |
| DotGram.Examples.Feeds.FeedReader | 29 (28-29) | 30 (30-31) | 1.04x |
| DotGram.Examples.Feeds.LoggingFeedReader | 6 (6-6) | 7 (7-7) | 1.05x |
| DotGram.Examples.Feeds.RecoveringFeedReader | 8 (7-8) | 8 (8-8) | 1.06x |
| DotGram.Examples.Feeds.StockCountReader | 5 (5-5) | 5 (5-5) | 1.06x |
| DotGram.Examples.Feeds.StreamingFeedReader | 10 (10-10) | 11 (10-11) | 1.06x |
| DotGram.Examples.Formats.Config | 15 (15-16) | 16 (15-16) | 1.04x |
| DotGram.Examples.Formats.Config.Located | 13 (12-13) | 13 (13-13) | 1.03x |
| DotGram.Examples.Formats.FileNames | 5 (5-5) | 5 (5-5) | 1.06x |
| DotGram.Examples.Formats.FixedWidth | 13 (12-13) | 13 (13-13) | 1.05x |
| DotGram.Examples.Formats.FixParser | 3 (3-3) | 4 (4-4) | 1.04x |
| DotGram.Examples.Formats.HttpParser | 3 (3-14) | 4 (4-4) | 1.18x |
| DotGram.Examples.Formats.IniParser | 6 (5-6) | 6 (6-7) | 1.05x |
| DotGram.Examples.Formats.JsonParser | 12 (11-32) | 12 (11-31) | 0.97x |
| DotGram.Examples.Formats.Links | 9 (9-9) | 9 (9-9) | 1.03x |
| DotGram.Examples.Formats.MarkdownParser | 6 (6-7) | 7 (6-7) | 1.06x |
| DotGram.Examples.Formats.MetricsLine | 15 (14-15) | 15 (15-15) | 1.03x |
| DotGram.Examples.Formats.Netstrings | 3 (3-3) | 3 (3-3) | 1.01x |
| DotGram.Examples.Formats.TypedCsv | 10 (10-11) | 10 (10-11) | 1.02x |
| DotGram.Examples.Formats.XmlParser | 6 (6-7) | 7 (7-7) | 1.04x |
| DotGram.Examples.Formats.YamlLite | 4 (4-4) | 4 (4-8) | 1.02x |
| DotGram.Examples.Languages.Filter | 10 (10-10) | 11 (10-11) | 1.01x |
| DotGram.Examples.Languages.FilterFile | 9 (9-9) | 9 (9-9) | 1.01x |
| DotGram.Examples.Languages.Filters | 15 (13-16) | 15 (13-15) | 0.99x |
| DotGram.Examples.Languages.GramGrammar | 137 (133-138) | 137 (136-140) | 1.00x |
| DotGram.Examples.Languages.Lexemes | 16 (5-24) | 15 (11-26) | 0.92x |
| DotGram.Examples.Languages.Scoped | 12 (12-12) | 12 (12-13) | 1.02x |
| DotGram.Examples.Languages.Selectors | 9 (9-10) | 10 (9-10) | 1.03x |
| DotGram.Examples.Languages.SettingsFile | 12 (12-15) | 12 (12-12) | 0.98x |
| DotGram.Examples.Languages.SqlDialect | 7 (7-7) | 7 (7-7) | 1.00x |
| DotGram.Examples.Languages.SqlReadOnly | 19 (18-19) | 19 (19-20) | 1.00x |
| DotGram.Examples.Languages.TokenizedQuery | 91 (91-94) | 92 (91-94) | 1.01x |
| DotGram.Sql.Standard.Sql92Parser | 761 (733-775) | 776 (765-785) | 1.02x |
| DotGram.Sql.Standard.SqlStandardParser | 3,862 (3,718-3,985) | 3,743 (3,741-3,882) | 0.97x |
| DotGram.Sql.TransactSql.TransactSqlParser | 4,923 (4,719-4,966) | 4,623 (4,577-4,636) | 0.94x |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | 2,404 (2,355-2,423) | 2,404 (2,365-2,443) | 1.00x |

No host moved by more than the tolerance.
