round 1 base d194c138 15:57:38
round 1 head b0547c87 15:59:08
round 2 base d194c138 16:00:38
round 2 head b0547c87 16:02:06
round 3 base d194c138 16:03:36
round 3 head b0547c87 16:05:05

Generator time, head b0547c87 against base d194c138, median of 3 alternating rounds (same cores):

| host | base ms (min-max) | head ms (min-max) | head / base |
| --- | ---: | ---: | ---: |
| DotGram.Examples.Expressions.ArithmeticTree | 18 (17-18) | 18 (18-19) | 1.01x |
| DotGram.Examples.Expressions.Calculator | 197 (195-208) | 208 (199-210) | 1.06x |
| DotGram.Examples.Expressions.ClampedExample | 6 (6-6) | 6 (6-7) | 1.02x |
| DotGram.Examples.Expressions.LocaleNumber | 4 (4-4) | 4 (4-5) | 1.02x |
| DotGram.Examples.Feeds.FeedReader | 33 (32-33) | 33 (33-34) | 1.03x |
| DotGram.Examples.Feeds.LoggingFeedReader | 6 (6-7) | 7 (7-7) | 1.04x |
| DotGram.Examples.Feeds.RecoveringFeedReader | 9 (9-9) | 9 (9-9) | 1.01x |
| DotGram.Examples.Feeds.StockCountReader | 5 (5-5) | 5 (5-6) | 1.00x |
| DotGram.Examples.Feeds.StreamingFeedReader | 10 (10-10) | 10 (10-11) | 1.01x |
| DotGram.Examples.Formats.Config | 15 (15-15) | 15 (15-15) | 1.00x |
| DotGram.Examples.Formats.Config.Located | 13 (13-13) | 13 (13-13) | 1.00x |
| DotGram.Examples.Formats.FileNames | 5 (5-5) | 5 (5-5) | 1.02x |
| DotGram.Examples.Formats.FixedWidth | 8 (8-8) | 8 (8-9) | 1.01x |
| DotGram.Examples.Formats.FixParser | 3 (3-3) | 4 (3-4) | 1.05x |
| DotGram.Examples.Formats.HttpParser | 3 (3-3) | 3 (3-4) | 1.02x |
| DotGram.Examples.Formats.IniParser | 6 (6-6) | 16 (6-17) | 2.72x |
| DotGram.Examples.Formats.JsonParser | 9 (9-9) | 8 (8-9) | 0.95x |
| DotGram.Examples.Formats.Links | 9 (9-9) | 9 (9-9) | 1.01x |
| DotGram.Examples.Formats.MarkdownParser | 6 (6-6) | 7 (7-7) | 1.07x |
| DotGram.Examples.Formats.MetricsLine | 15 (15-15) | 15 (15-16) | 1.01x |
| DotGram.Examples.Formats.Netstrings | 3 (3-3) | 3 (3-3) | 1.01x |
| DotGram.Examples.Formats.TypedCsv | 10 (10-10) | 10 (10-10) | 1.01x |
| DotGram.Examples.Formats.XmlParser | 6 (6-7) | 7 (7-7) | 1.02x |
| DotGram.Examples.Formats.YamlLite | 4 (4-4) | 4 (4-5) | 1.03x |
| DotGram.Examples.Languages.Filter | 10 (10-11) | 11 (11-13) | 1.03x |
| DotGram.Examples.Languages.FilterFile | 9 (8-9) | 9 (9-9) | 1.03x |
| DotGram.Examples.Languages.Filters | 20 (20-20) | 23 (22-26) | 1.16x |
| DotGram.Examples.Languages.GramGrammar | 175 (170-177) | 209 (205-212) | 1.19x |
| DotGram.Examples.Languages.Lexemes | 23 (17-24) | 6 (5-20) | 0.26x |
| DotGram.Examples.Languages.Scoped | 16 (15-16) | 16 (16-16) | 1.02x |
| DotGram.Examples.Languages.Selectors | 14 (14-15) | 17 (17-17) | 1.19x |
| DotGram.Examples.Languages.SettingsFile | 17 (17-17) | 17 (17-18) | 1.04x |
| DotGram.Examples.Languages.SqlDialect | 7 (7-7) | 7 (7-7) | 1.04x |
| DotGram.Examples.Languages.SqlReadOnly | 19 (18-19) | 19 (19-19) | 1.02x |
| DotGram.Examples.Languages.TokenizedQuery | 89 (88-92) | 91 (90-93) | 1.03x |
| DotGram.Sql.Standard.Sql92Parser | 762 (749-781) | 749 (741-751) | 0.98x |
| DotGram.Sql.Standard.SqlStandardParser | 3,850 (3,808-3,887) | 3,827 (3,786-3,925) | 0.99x |
| DotGram.Sql.TransactSql.TransactSqlParser | 4,776 (4,548-4,780) | 4,661 (4,658-4,794) | 0.98x |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | 2,401 (2,385-2,429) | 2,416 (2,389-2,446) | 1.01x |

No host moved by more than the tolerance.
