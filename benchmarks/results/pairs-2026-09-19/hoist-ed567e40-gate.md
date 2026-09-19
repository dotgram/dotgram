Preparing worktree (detached HEAD 7e13fe68)
Preparing worktree (detached HEAD ed567e40)
round 1 base 7e13fe68 06:05:47
round 1 head ed567e40 06:07:13
round 2 base 7e13fe68 06:08:43
round 2 head ed567e40 06:10:09
round 3 base 7e13fe68 06:11:35
round 3 head ed567e40 06:13:02

Generator time, head ed567e40 against base 7e13fe68, median of 3 alternating rounds (same cores):

| host | base ms (min-max) | head ms (min-max) | head / base |
| --- | ---: | ---: | ---: |
| DotGram.Examples.Expressions.ArithmeticTree | 12 (12-12) | 13 (13-13) | 1.08x |
| DotGram.Examples.Expressions.Calculator | 182 (181-182) | 184 (182-185) | 1.01x |
| DotGram.Examples.Expressions.ClampedExample | 6 (6-6) | 7 (7-7) | 1.07x |
| DotGram.Examples.Expressions.LocaleNumber | 4 (4-4) | 4 (4-4) | 1.08x |
| DotGram.Examples.Feeds.FeedReader | 28 (28-29) | 29 (29-29) | 1.01x |
| DotGram.Examples.Feeds.LoggingFeedReader | 6 (6-7) | 7 (7-7) | 1.06x |
| DotGram.Examples.Feeds.RecoveringFeedReader | 7 (7-8) | 8 (8-8) | 1.05x |
| DotGram.Examples.Feeds.StockCountReader | 5 (5-5) | 5 (5-5) | 1.06x |
| DotGram.Examples.Feeds.StreamingFeedReader | 10 (10-10) | 10 (10-11) | 1.05x |
| DotGram.Examples.Formats.Config | 15 (15-15) | 16 (16-16) | 1.07x |
| DotGram.Examples.Formats.Config.Located | 12 (12-12) | 14 (14-14) | 1.16x |
| DotGram.Examples.Formats.FileNames | 5 (5-5) | 5 (5-5) | 1.06x |
| DotGram.Examples.Formats.FixedWidth | 15 (14-15) | 15 (15-15) | 1.02x |
| DotGram.Examples.Formats.FixParser | 3 (3-3) | 4 (4-4) | 1.10x |
| DotGram.Examples.Formats.HttpParser | 13 (13-14) | 14 (4-14) | 1.02x |
| DotGram.Examples.Formats.IniParser | 5 (5-5) | 5 (5-7) | 1.09x |
| DotGram.Examples.Formats.JsonParser | 8 (7-8) | 8 (8-8) | 1.07x |
| DotGram.Examples.Formats.Links | 9 (9-9) | 9 (9-9) | 1.05x |
| DotGram.Examples.Formats.MarkdownParser | 6 (6-7) | 6 (6-7) | 0.99x |
| DotGram.Examples.Formats.MetricsLine | 14 (14-14) | 15 (15-15) | 1.07x |
| DotGram.Examples.Formats.Netstrings | 3 (3-3) | 3 (3-3) | 1.09x |
| DotGram.Examples.Formats.TypedCsv | 9 (9-10) | 10 (10-10) | 1.04x |
| DotGram.Examples.Formats.XmlParser | 6 (6-6) | 7 (7-7) | 1.07x |
| DotGram.Examples.Formats.YamlLite | 4 (4-4) | 4 (4-5) | 1.09x |
| DotGram.Examples.Languages.Filter | 10 (10-10) | 11 (11-11) | 1.06x |
| DotGram.Examples.Languages.FilterFile | 8 (8-8) | 9 (9-9) | 1.14x |
| DotGram.Examples.Languages.Filters | 12 (12-12) | 13 (13-16) | 1.08x |
| DotGram.Examples.Languages.GramGrammar | 128 (128-129) | 160 (157-160) | 1.25x |
| DotGram.Examples.Languages.Lexemes | 14 (11-18) | 10 (6-10) | 0.70x |
| DotGram.Examples.Languages.Scoped | 11 (11-11) | 13 (13-13) | 1.16x |
| DotGram.Examples.Languages.Selectors | 9 (9-9) | 11 (10-11) | 1.18x |
| DotGram.Examples.Languages.SettingsFile | 14 (14-14) | 13 (13-13) | 0.90x |
| DotGram.Examples.Languages.SqlDialect | 7 (7-7) | 7 (7-7) | 1.06x |
| DotGram.Examples.Languages.SqlReadOnly | 18 (18-18) | 19 (19-19) | 1.03x |
| DotGram.Examples.Languages.TokenizedQuery | 89 (89-89) | 91 (91-91) | 1.02x |
| DotGram.Sql.Standard.Sql92Parser | 755 (716-759) | 811 (771-814) | 1.07x |
| DotGram.Sql.Standard.SqlStandardParser | 3,625 (3,575-3,678) | 3,627 (3,552-3,715) | 1.00x |
| DotGram.Sql.TransactSql.TransactSqlParser | 4,724 (4,693-4,765) | 4,717 (4,577-4,760) | 1.00x |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | 2,346 (2,331-2,375) | 2,426 (2,355-2,515) | 1.03x |

No host moved by more than the tolerance.
