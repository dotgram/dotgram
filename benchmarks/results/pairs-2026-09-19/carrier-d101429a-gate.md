round 1 base 312d5b98 08:57:19
round 1 head d101429a 08:58:49
round 2 base 312d5b98 09:00:18
round 2 head d101429a 09:01:48
round 3 base 312d5b98 09:03:18
round 3 head d101429a 09:04:47

Generator time, head d101429a against base 312d5b98, median of 3 alternating rounds (same cores):

| host | base ms (min-max) | head ms (min-max) | head / base |
| --- | ---: | ---: | ---: |
| DotGram.Examples.Expressions.ArithmeticTree | 13 (12-13) | 13 (12-13) | 1.01x |
| DotGram.Examples.Expressions.Calculator | 187 (182-189) | 185 (185-194) | 0.99x |
| DotGram.Examples.Expressions.ClampedExample | 6 (6-7) | 6 (6-7) | 1.00x |
| DotGram.Examples.Expressions.LocaleNumber | 4 (4-4) | 4 (4-4) | 1.00x |
| DotGram.Examples.Feeds.FeedReader | 29 (28-29) | 29 (28-29) | 0.99x |
| DotGram.Examples.Feeds.LoggingFeedReader | 7 (6-7) | 6 (6-7) | 0.99x |
| DotGram.Examples.Feeds.RecoveringFeedReader | 7 (7-8) | 7 (7-8) | 0.98x |
| DotGram.Examples.Feeds.StockCountReader | 5 (5-5) | 5 (5-5) | 0.98x |
| DotGram.Examples.Feeds.StreamingFeedReader | 10 (10-10) | 10 (10-10) | 0.99x |
| DotGram.Examples.Formats.Config | 15 (14-15) | 15 (15-15) | 1.01x |
| DotGram.Examples.Formats.Config.Located | 12 (12-13) | 12 (12-13) | 1.00x |
| DotGram.Examples.Formats.FileNames | 5 (5-5) | 5 (5-5) | 0.99x |
| DotGram.Examples.Formats.FixedWidth | 15 (14-15) | 15 (15-15) | 0.99x |
| DotGram.Examples.Formats.FixParser | 3 (3-3) | 3 (3-3) | 0.99x |
| DotGram.Examples.Formats.HttpParser | 4 (3-4) | 3 (3-4) | 0.90x |
| DotGram.Examples.Formats.IniParser | 6 (6-17) | 6 (6-6) | 0.91x |
| DotGram.Examples.Formats.JsonParser | 8 (7-8) | 9 (9-14) | 1.14x |
| DotGram.Examples.Formats.Links | 9 (9-9) | 9 (8-9) | 1.00x |
| DotGram.Examples.Formats.MarkdownParser | 6 (6-7) | 6 (6-7) | 1.00x |
| DotGram.Examples.Formats.MetricsLine | 14 (14-14) | 14 (14-14) | 1.00x |
| DotGram.Examples.Formats.Netstrings | 3 (3-3) | 3 (3-3) | 1.00x |
| DotGram.Examples.Formats.TypedCsv | 10 (9-10) | 10 (10-10) | 1.01x |
| DotGram.Examples.Formats.XmlParser | 7 (6-7) | 6 (6-7) | 0.94x |
| DotGram.Examples.Formats.YamlLite | 4 (4-4) | 4 (4-4) | 1.00x |
| DotGram.Examples.Languages.Filter | 10 (10-11) | 10 (10-11) | 1.04x |
| DotGram.Examples.Languages.FilterFile | 9 (8-11) | 8 (8-9) | 0.96x |
| DotGram.Examples.Languages.Filters | 15 (12-15) | 15 (15-15) | 0.99x |
| DotGram.Examples.Languages.GramGrammar | 129 (124-130) | 125 (125-130) | 0.97x |
| DotGram.Examples.Languages.Lexemes | 16 (4-22) | 5 (5-23) | 0.33x |
| DotGram.Examples.Languages.Scoped | 12 (11-12) | 12 (11-13) | 0.99x |
| DotGram.Examples.Languages.Selectors | 9 (9-9) | 9 (9-9) | 1.02x |
| DotGram.Examples.Languages.SettingsFile | 11 (11-11) | 11 (11-11) | 1.00x |
| DotGram.Examples.Languages.SqlDialect | 7 (7-7) | 7 (7-7) | 0.97x |
| DotGram.Examples.Languages.SqlReadOnly | 19 (18-19) | 18 (18-19) | 0.98x |
| DotGram.Examples.Languages.TokenizedQuery | 93 (89-104) | 89 (89-92) | 0.96x |
| DotGram.Sql.Standard.Sql92Parser | 780 (734-780) | 730 (727-790) | 0.94x |
| DotGram.Sql.Standard.SqlStandardParser | 3,661 (3,592-3,885) | 3,694 (3,619-3,813) | 1.01x |
| DotGram.Sql.TransactSql.TransactSqlParser | 4,752 (4,697-4,991) | 4,748 (4,744-4,875) | 1.00x |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | 2,388 (2,358-2,398) | 2,347 (2,338-2,530) | 0.98x |

No host moved by more than the tolerance.
