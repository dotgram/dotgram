Preparing worktree (detached HEAD 584a7c1f)
Preparing worktree (detached HEAD e4ad4638)
round 1 base 584a7c1f 07:05:27
round 1 head e4ad4638 07:06:56
round 2 base 584a7c1f 07:08:22
round 2 head e4ad4638 07:09:48
round 3 base 584a7c1f 07:11:17
round 3 head e4ad4638 07:12:46

Generator time, head e4ad4638 against base 584a7c1f, median of 3 alternating rounds (same cores):

| host | base ms (min-max) | head ms (min-max) | head / base |
| --- | ---: | ---: | ---: |
| DotGram.Examples.Expressions.ArithmeticTree | 11 (11-11) | 11 (11-11) | 0.99x |
| DotGram.Examples.Expressions.Calculator | 180 (180-186) | 181 (180-186) | 1.00x |
| DotGram.Examples.Expressions.ClampedExample | 6 (6-6) | 6 (6-6) | 1.01x |
| DotGram.Examples.Expressions.LocaleNumber | 4 (4-4) | 4 (4-4) | 0.97x |
| DotGram.Examples.Feeds.FeedReader | 28 (28-28) | 28 (28-29) | 1.01x |
| DotGram.Examples.Feeds.LoggingFeedReader | 7 (7-7) | 7 (7-7) | 1.00x |
| DotGram.Examples.Feeds.RecoveringFeedReader | 7 (7-7) | 7 (7-8) | 1.01x |
| DotGram.Examples.Feeds.StockCountReader | 5 (5-6) | 5 (5-5) | 1.01x |
| DotGram.Examples.Feeds.StreamingFeedReader | 10 (10-10) | 10 (10-10) | 1.00x |
| DotGram.Examples.Formats.Config | 15 (15-15) | 15 (14-15) | 1.00x |
| DotGram.Examples.Formats.Config.Located | 12 (12-12) | 12 (12-12) | 1.00x |
| DotGram.Examples.Formats.FileNames | 5 (5-5) | 5 (5-5) | 1.00x |
| DotGram.Examples.Formats.FixedWidth | 14 (14-15) | 15 (15-16) | 1.03x |
| DotGram.Examples.Formats.FixParser | 3 (3-3) | 3 (3-3) | 1.00x |
| DotGram.Examples.Formats.HttpParser | 13 (3-15) | 3 (3-4) | 0.23x |
| DotGram.Examples.Formats.IniParser | 5 (5-15) | 6 (6-15) | 1.24x |
| DotGram.Examples.Formats.JsonParser | 8 (8-8) | 8 (8-9) | 0.99x |
| DotGram.Examples.Formats.Links | 8 (8-9) | 8 (8-9) | 1.00x |
| DotGram.Examples.Formats.MarkdownParser | 6 (6-6) | 6 (6-6) | 0.99x |
| DotGram.Examples.Formats.MetricsLine | 14 (14-14) | 14 (14-14) | 1.00x |
| DotGram.Examples.Formats.Netstrings | 3 (3-3) | 3 (3-3) | 0.99x |
| DotGram.Examples.Formats.TypedCsv | 9 (9-10) | 9 (9-9) | 1.00x |
| DotGram.Examples.Formats.XmlParser | 6 (6-6) | 6 (6-6) | 0.99x |
| DotGram.Examples.Formats.YamlLite | 4 (4-4) | 4 (4-4) | 0.99x |
| DotGram.Examples.Languages.Filter | 10 (10-10) | 10 (10-10) | 1.00x |
| DotGram.Examples.Languages.FilterFile | 8 (8-11) | 8 (8-11) | 1.00x |
| DotGram.Examples.Languages.Filters | 12 (12-12) | 14 (12-15) | 1.23x |
| DotGram.Examples.Languages.GramGrammar | 129 (128-129) | 124 (123-128) | 0.96x |
| DotGram.Examples.Languages.Lexemes | 25 (13-25) | 13 (5-16) | 0.54x |
| DotGram.Examples.Languages.Scoped | 11 (11-11) | 11 (11-11) | 1.00x |
| DotGram.Examples.Languages.Selectors | 9 (9-9) | 9 (9-9) | 1.00x |
| DotGram.Examples.Languages.SettingsFile | 14 (11-14) | 11 (11-11) | 0.75x |
| DotGram.Examples.Languages.SqlDialect | 7 (7-7) | 7 (7-7) | 0.99x |
| DotGram.Examples.Languages.SqlReadOnly | 18 (18-18) | 18 (18-18) | 1.00x |
| DotGram.Examples.Languages.TokenizedQuery | 89 (89-91) | 89 (89-89) | 1.00x |
| DotGram.Sql.Standard.Sql92Parser | 756 (756-756) | 726 (718-748) | 0.96x |
| DotGram.Sql.Standard.SqlStandardParser | 3,539 (3,534-3,652) | 3,639 (3,550-3,725) | 1.03x |
| DotGram.Sql.TransactSql.TransactSqlParser | 4,628 (4,573-4,772) | 4,857 (4,611-4,894) | 1.05x |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | 2,363 (2,289-2,465) | 2,417 (2,228-2,498) | 1.02x |

No host moved by more than the tolerance.
