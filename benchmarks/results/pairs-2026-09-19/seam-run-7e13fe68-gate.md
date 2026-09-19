Preparing worktree (detached HEAD 164967f8)
Preparing worktree (detached HEAD 7e13fe68)
round 1 base 164967f8 05:56:52
round 1 head 7e13fe68 05:58:22
round 2 base 164967f8 05:59:50
round 2 head 7e13fe68 06:01:19
round 3 base 164967f8 06:02:49
round 3 head 7e13fe68 06:04:17

Generator time, head 7e13fe68 against base 164967f8, median of 3 alternating rounds (same cores):

| host | base ms (min-max) | head ms (min-max) | head / base |
| --- | ---: | ---: | ---: |
| DotGram.Examples.Expressions.ArithmeticTree | 11 (11-11) | 13 (12-13) | 1.12x |
| DotGram.Examples.Expressions.Calculator | 183 (180-185) | 183 (181-184) | 1.00x |
| DotGram.Examples.Expressions.ClampedExample | 6 (6-6) | 6 (6-7) | 0.99x |
| DotGram.Examples.Expressions.LocaleNumber | 4 (4-4) | 4 (4-4) | 0.99x |
| DotGram.Examples.Feeds.FeedReader | 29 (28-29) | 28 (28-29) | 0.97x |
| DotGram.Examples.Feeds.LoggingFeedReader | 7 (6-7) | 6 (6-7) | 0.98x |
| DotGram.Examples.Feeds.RecoveringFeedReader | 8 (7-8) | 7 (7-8) | 0.99x |
| DotGram.Examples.Feeds.StockCountReader | 5 (5-5) | 5 (5-5) | 0.98x |
| DotGram.Examples.Feeds.StreamingFeedReader | 10 (10-10) | 10 (10-10) | 1.00x |
| DotGram.Examples.Formats.Config | 15 (15-15) | 15 (15-15) | 0.98x |
| DotGram.Examples.Formats.Config.Located | 12 (12-13) | 12 (12-12) | 0.99x |
| DotGram.Examples.Formats.FileNames | 5 (5-5) | 5 (5-5) | 0.99x |
| DotGram.Examples.Formats.FixedWidth | 15 (15-15) | 15 (14-15) | 0.99x |
| DotGram.Examples.Formats.FixParser | 3 (3-3) | 3 (3-3) | 1.00x |
| DotGram.Examples.Formats.HttpParser | 14 (3-14) | 14 (3-14) | 1.01x |
| DotGram.Examples.Formats.IniParser | 5 (5-6) | 5 (5-6) | 0.98x |
| DotGram.Examples.Formats.JsonParser | 8 (8-9) | 8 (7-8) | 0.94x |
| DotGram.Examples.Formats.Links | 9 (9-9) | 9 (9-9) | 0.99x |
| DotGram.Examples.Formats.MarkdownParser | 6 (6-7) | 6 (6-6) | 0.97x |
| DotGram.Examples.Formats.MetricsLine | 14 (14-14) | 14 (14-14) | 0.99x |
| DotGram.Examples.Formats.Netstrings | 3 (3-3) | 3 (3-3) | 1.00x |
| DotGram.Examples.Formats.TypedCsv | 10 (9-10) | 10 (9-10) | 1.00x |
| DotGram.Examples.Formats.XmlParser | 6 (6-6) | 6 (6-6) | 1.01x |
| DotGram.Examples.Formats.YamlLite | 4 (4-4) | 4 (4-4) | 0.98x |
| DotGram.Examples.Languages.Filter | 10 (10-10) | 10 (10-10) | 1.00x |
| DotGram.Examples.Languages.FilterFile | 8 (8-8) | 8 (8-8) | 0.99x |
| DotGram.Examples.Languages.Filters | 12 (12-15) | 12 (12-15) | 0.96x |
| DotGram.Examples.Languages.GramGrammar | 128 (127-134) | 129 (125-129) | 1.01x |
| DotGram.Examples.Languages.Lexemes | 20 (13-21) | 14 (5-25) | 0.67x |
| DotGram.Examples.Languages.Scoped | 12 (11-12) | 11 (11-11) | 0.99x |
| DotGram.Examples.Languages.Selectors | 9 (9-9) | 9 (9-9) | 1.00x |
| DotGram.Examples.Languages.SettingsFile | 14 (11-14) | 14 (11-14) | 1.01x |
| DotGram.Examples.Languages.SqlDialect | 7 (7-8) | 7 (7-7) | 0.97x |
| DotGram.Examples.Languages.SqlReadOnly | 18 (18-18) | 18 (18-19) | 0.99x |
| DotGram.Examples.Languages.TokenizedQuery | 91 (89-93) | 89 (88-90) | 0.98x |
| DotGram.Sql.Standard.Sql92Parser | 720 (715-764) | 760 (737-790) | 1.06x |
| DotGram.Sql.Standard.SqlStandardParser | 3,838 (3,802-3,847) | 3,809 (3,753-3,823) | 0.99x |
| DotGram.Sql.TransactSql.TransactSqlParser | 4,893 (4,881-4,906) | 5,020 (4,860-5,048) | 1.03x |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | 2,276 (2,241-2,359) | 2,408 (2,354-2,444) | 1.06x |

No host moved by more than the tolerance.
