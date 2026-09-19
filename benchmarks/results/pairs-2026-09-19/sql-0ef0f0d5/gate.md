round 1 base b0547c87 16:29:37
round 1 head 0ef0f0d5 16:31:07
round 2 base b0547c87 16:32:38
round 2 head 0ef0f0d5 16:34:07
round 3 base b0547c87 16:35:38
round 3 head 0ef0f0d5 16:37:07

Generator time, head 0ef0f0d5 against base b0547c87, median of 3 alternating rounds (same cores):

| host | base ms (min-max) | head ms (min-max) | head / base |
| --- | ---: | ---: | ---: |
| DotGram.Examples.Expressions.ArithmeticTree | 18 (18-18) | 18 (17-18) | 0.97x |
| DotGram.Examples.Expressions.Calculator | 202 (202-205) | 198 (197-209) | 0.98x |
| DotGram.Examples.Expressions.ClampedExample | 6 (6-6) | 6 (6-6) | 1.00x |
| DotGram.Examples.Expressions.LocaleNumber | 4 (4-4) | 4 (4-4) | 1.00x |
| DotGram.Examples.Feeds.FeedReader | 33 (33-33) | 33 (32-33) | 0.99x |
| DotGram.Examples.Feeds.LoggingFeedReader | 7 (6-7) | 6 (6-7) | 0.99x |
| DotGram.Examples.Feeds.RecoveringFeedReader | 9 (9-10) | 9 (9-9) | 0.98x |
| DotGram.Examples.Feeds.StockCountReader | 5 (5-5) | 5 (5-5) | 1.00x |
| DotGram.Examples.Feeds.StreamingFeedReader | 10 (10-10) | 10 (10-10) | 1.01x |
| DotGram.Examples.Formats.Config | 15 (15-15) | 15 (15-16) | 1.01x |
| DotGram.Examples.Formats.Config.Located | 13 (13-13) | 13 (13-13) | 1.01x |
| DotGram.Examples.Formats.FileNames | 5 (5-5) | 5 (5-5) | 0.99x |
| DotGram.Examples.Formats.FixedWidth | 8 (8-8) | 9 (8-9) | 1.05x |
| DotGram.Examples.Formats.FixParser | 3 (3-4) | 4 (3-4) | 1.02x |
| DotGram.Examples.Formats.HttpParser | 3 (3-4) | 3 (3-3) | 1.02x |
| DotGram.Examples.Formats.IniParser | 16 (7-17) | 6 (6-16) | 0.39x |
| DotGram.Examples.Formats.JsonParser | 9 (9-9) | 9 (8-9) | 1.05x |
| DotGram.Examples.Formats.Links | 9 (9-9) | 9 (9-9) | 1.00x |
| DotGram.Examples.Formats.MarkdownParser | 7 (6-7) | 7 (6-7) | 1.01x |
| DotGram.Examples.Formats.MetricsLine | 15 (15-15) | 15 (15-16) | 1.02x |
| DotGram.Examples.Formats.Netstrings | 3 (3-3) | 3 (3-3) | 1.00x |
| DotGram.Examples.Formats.TypedCsv | 10 (10-10) | 10 (10-10) | 0.99x |
| DotGram.Examples.Formats.XmlParser | 7 (6-7) | 6 (6-7) | 0.99x |
| DotGram.Examples.Formats.YamlLite | 4 (4-4) | 4 (4-5) | 1.00x |
| DotGram.Examples.Languages.Filter | 11 (10-11) | 11 (10-11) | 1.00x |
| DotGram.Examples.Languages.FilterFile | 9 (9-9) | 9 (9-9) | 1.01x |
| DotGram.Examples.Languages.Filters | 22 (22-26) | 25 (22-25) | 1.12x |
| DotGram.Examples.Languages.GramGrammar | 210 (198-211) | 206 (197-206) | 0.98x |
| DotGram.Examples.Languages.Lexemes | 6 (5-21) | 15 (5-17) | 2.54x |
| DotGram.Examples.Languages.Scoped | 16 (16-16) | 16 (15-16) | 1.01x |
| DotGram.Examples.Languages.Selectors | 17 (17-17) | 17 (16-17) | 0.99x |
| DotGram.Examples.Languages.SettingsFile | 17 (17-17) | 17 (17-18) | 0.99x |
| DotGram.Examples.Languages.SqlDialect | 7 (7-7) | 7 (7-7) | 0.99x |
| DotGram.Examples.Languages.SqlReadOnly | 19 (19-19) | 19 (19-19) | 0.98x |
| DotGram.Examples.Languages.TokenizedQuery | 90 (90-92) | 90 (89-93) | 0.99x |
| DotGram.Sql.Standard.Sql92Parser | 776 (739-781) | 797 (792-799) | 1.03x |
| DotGram.Sql.Standard.SqlStandardParser | 3,831 (3,780-4,223) | 3,815 (3,737-3,866) | 1.00x |
| DotGram.Sql.TransactSql.TransactSqlParser | 4,878 (4,762-5,025) | 4,882 (4,785-5,115) | 1.00x |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | 2,369 (2,368-2,451) | 2,406 (2,385-2,432) | 1.02x |

No host moved by more than the tolerance.
