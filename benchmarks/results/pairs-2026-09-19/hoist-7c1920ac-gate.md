Preparing worktree (detached HEAD e35bc176)
Preparing worktree (detached HEAD 7c1920ac)
round 1 base e35bc176 07:16:34
round 1 head 7c1920ac 07:18:03
round 2 base e35bc176 07:19:33
round 2 head 7c1920ac 07:20:59
round 3 base e35bc176 07:22:25
round 3 head 7c1920ac 07:23:53

Generator time, head 7c1920ac against base e35bc176, median of 3 alternating rounds (same cores):

| host | base ms (min-max) | head ms (min-max) | head / base |
| --- | ---: | ---: | ---: |
| DotGram.Examples.Expressions.ArithmeticTree | 13 (12-13) | 13 (13-13) | 1.07x |
| DotGram.Examples.Expressions.Calculator | 185 (184-185) | 189 (188-191) | 1.02x |
| DotGram.Examples.Expressions.ClampedExample | 6 (6-6) | 7 (7-7) | 1.03x |
| DotGram.Examples.Expressions.LocaleNumber | 4 (4-4) | 4 (4-4) | 1.01x |
| DotGram.Examples.Feeds.FeedReader | 29 (28-30) | 29 (29-30) | 1.03x |
| DotGram.Examples.Feeds.LoggingFeedReader | 6 (6-7) | 7 (7-7) | 1.04x |
| DotGram.Examples.Feeds.RecoveringFeedReader | 7 (7-7) | 7 (7-8) | 1.01x |
| DotGram.Examples.Feeds.StockCountReader | 5 (5-5) | 5 (5-5) | 1.05x |
| DotGram.Examples.Feeds.StreamingFeedReader | 10 (10-10) | 10 (10-10) | 1.01x |
| DotGram.Examples.Formats.Config | 15 (15-15) | 15 (15-15) | 1.03x |
| DotGram.Examples.Formats.Config.Located | 12 (12-12) | 12 (12-13) | 1.01x |
| DotGram.Examples.Formats.FileNames | 5 (5-5) | 5 (5-5) | 1.04x |
| DotGram.Examples.Formats.FixedWidth | 15 (14-15) | 15 (15-15) | 1.03x |
| DotGram.Examples.Formats.FixParser | 3 (3-3) | 3 (3-4) | 1.04x |
| DotGram.Examples.Formats.HttpParser | 4 (3-14) | 4 (3-14) | 1.02x |
| DotGram.Examples.Formats.IniParser | 6 (5-17) | 6 (5-6) | 0.97x |
| DotGram.Examples.Formats.JsonParser | 8 (8-8) | 9 (8-17) | 1.17x |
| DotGram.Examples.Formats.Links | 9 (9-9) | 9 (9-9) | 1.03x |
| DotGram.Examples.Formats.MarkdownParser | 6 (6-7) | 6 (6-6) | 1.01x |
| DotGram.Examples.Formats.MetricsLine | 14 (14-14) | 14 (14-15) | 1.01x |
| DotGram.Examples.Formats.Netstrings | 3 (3-3) | 3 (3-3) | 1.01x |
| DotGram.Examples.Formats.TypedCsv | 10 (9-10) | 10 (9-10) | 1.00x |
| DotGram.Examples.Formats.XmlParser | 6 (6-6) | 6 (6-7) | 1.03x |
| DotGram.Examples.Formats.YamlLite | 4 (4-4) | 4 (4-5) | 1.03x |
| DotGram.Examples.Languages.Filter | 10 (10-10) | 10 (10-10) | 1.02x |
| DotGram.Examples.Languages.FilterFile | 8 (8-11) | 9 (9-9) | 1.05x |
| DotGram.Examples.Languages.Filters | 12 (12-15) | 16 (12-16) | 1.31x |
| DotGram.Examples.Languages.GramGrammar | 128 (125-130) | 129 (126-132) | 1.01x |
| DotGram.Examples.Languages.Lexemes | 15 (5-19) | 16 (5-21) | 1.06x |
| DotGram.Examples.Languages.Scoped | 11 (11-11) | 12 (12-12) | 1.06x |
| DotGram.Examples.Languages.Selectors | 9 (9-9) | 9 (9-9) | 1.04x |
| DotGram.Examples.Languages.SettingsFile | 11 (11-14) | 12 (12-16) | 1.16x |
| DotGram.Examples.Languages.SqlDialect | 7 (7-7) | 7 (7-8) | 1.02x |
| DotGram.Examples.Languages.SqlReadOnly | 18 (18-18) | 19 (18-19) | 1.02x |
| DotGram.Examples.Languages.TokenizedQuery | 90 (88-90) | 92 (91-93) | 1.02x |
| DotGram.Sql.Standard.Sql92Parser | 761 (721-765) | 738 (712-739) | 0.97x |
| DotGram.Sql.Standard.SqlStandardParser | 3,715 (3,666-3,733) | 3,739 (3,567-3,871) | 1.01x |
| DotGram.Sql.TransactSql.TransactSqlParser | 4,733 (4,696-4,998) | 4,773 (4,631-4,891) | 1.01x |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | 2,355 (2,324-2,394) | 2,379 (2,378-2,497) | 1.01x |

No host moved by more than the tolerance.
