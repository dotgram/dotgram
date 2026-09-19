Preparing worktree (detached HEAD 584a7c1f)
Preparing worktree (detached HEAD c29deeb0)
round 1 base 584a7c1f 05:35:52
round 1 head c29deeb0 05:37:21
round 2 base 584a7c1f 05:38:49
round 2 head c29deeb0 05:40:16
round 3 base 584a7c1f 05:41:44
round 3 head c29deeb0 05:43:12

Generator time, head c29deeb0 against base 584a7c1f, median of 3 alternating rounds (same cores):

| host | base ms (min-max) | head ms (min-max) | head / base |
| --- | ---: | ---: | ---: |
| DotGram.Examples.Expressions.ArithmeticTree | 11 (11-11) | 11 (11-11) | 1.01x |
| DotGram.Examples.Expressions.Calculator | 183 (181-185) | 182 (180-185) | 0.99x |
| DotGram.Examples.Expressions.ClampedExample | 6 (6-7) | 6 (6-6) | 0.97x |
| DotGram.Examples.Expressions.LocaleNumber | 4 (4-4) | 4 (4-4) | 0.99x |
| DotGram.Examples.Feeds.FeedReader | 29 (28-30) | 28 (28-29) | 0.97x |
| DotGram.Examples.Feeds.LoggingFeedReader | 7 (7-7) | 7 (7-7) | 0.97x |
| DotGram.Examples.Feeds.RecoveringFeedReader | 8 (7-8) | 7 (7-7) | 0.97x |
| DotGram.Examples.Feeds.StockCountReader | 5 (5-5) | 5 (5-5) | 0.99x |
| DotGram.Examples.Feeds.StreamingFeedReader | 10 (10-10) | 10 (10-10) | 0.97x |
| DotGram.Examples.Formats.Config | 15 (15-15) | 15 (15-15) | 0.98x |
| DotGram.Examples.Formats.Config.Located | 12 (12-13) | 12 (12-12) | 0.99x |
| DotGram.Examples.Formats.FileNames | 5 (5-5) | 5 (5-5) | 0.99x |
| DotGram.Examples.Formats.FixedWidth | 15 (15-15) | 15 (14-15) | 0.99x |
| DotGram.Examples.Formats.FixParser | 3 (3-3) | 3 (3-3) | 0.99x |
| DotGram.Examples.Formats.HttpParser | 4 (4-14) | 4 (3-4) | 0.96x |
| DotGram.Examples.Formats.IniParser | 6 (5-7) | 6 (6-16) | 0.99x |
| DotGram.Examples.Formats.JsonParser | 8 (8-8) | 8 (8-8) | 0.97x |
| DotGram.Examples.Formats.Links | 9 (9-9) | 9 (9-9) | 0.99x |
| DotGram.Examples.Formats.MarkdownParser | 7 (7-7) | 6 (6-6) | 0.94x |
| DotGram.Examples.Formats.MetricsLine | 14 (14-14) | 14 (14-14) | 0.98x |
| DotGram.Examples.Formats.Netstrings | 3 (3-3) | 3 (3-3) | 0.97x |
| DotGram.Examples.Formats.TypedCsv | 10 (10-10) | 10 (9-10) | 0.97x |
| DotGram.Examples.Formats.XmlParser | 6 (6-7) | 6 (6-6) | 0.98x |
| DotGram.Examples.Formats.YamlLite | 4 (4-4) | 4 (4-4) | 0.98x |
| DotGram.Examples.Languages.Filter | 10 (10-10) | 10 (10-10) | 0.98x |
| DotGram.Examples.Languages.FilterFile | 8 (8-9) | 8 (8-11) | 0.99x |
| DotGram.Examples.Languages.Filters | 15 (12-16) | 14 (12-15) | 0.96x |
| DotGram.Examples.Languages.GramGrammar | 128 (125-130) | 126 (125-128) | 0.98x |
| DotGram.Examples.Languages.Lexemes | 14 (5-14) | 5 (5-23) | 0.35x |
| DotGram.Examples.Languages.Scoped | 11 (11-13) | 11 (11-11) | 1.00x |
| DotGram.Examples.Languages.Selectors | 9 (9-10) | 9 (9-9) | 0.99x |
| DotGram.Examples.Languages.SettingsFile | 12 (11-14) | 11 (11-11) | 0.92x |
| DotGram.Examples.Languages.SqlDialect | 7 (7-7) | 7 (7-7) | 0.98x |
| DotGram.Examples.Languages.SqlReadOnly | 18 (18-19) | 18 (18-18) | 0.99x |
| DotGram.Examples.Languages.TokenizedQuery | 91 (91-95) | 89 (89-91) | 0.98x |
| DotGram.Sql.Standard.Sql92Parser | 761 (758-781) | 761 (732-762) | 1.00x |
| DotGram.Sql.Standard.SqlStandardParser | 3,653 (3,588-3,671) | 3,729 (3,702-3,777) | 1.02x |
| DotGram.Sql.TransactSql.TransactSqlParser | 4,752 (4,704-4,753) | 4,726 (4,618-4,851) | 0.99x |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | 2,280 (2,269-2,378) | 2,363 (2,349-2,374) | 1.04x |

No host moved by more than the tolerance.
