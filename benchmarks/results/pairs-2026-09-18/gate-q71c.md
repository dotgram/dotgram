Preparing worktree (detached HEAD b35eb2ee)
Preparing worktree (detached HEAD 853b1665)
round 1 base b35eb2ee 22:48:07
round 1 head 853b1665 22:49:32
round 2 base b35eb2ee 22:50:55
round 2 head 853b1665 22:52:19
round 3 base b35eb2ee 22:53:45
round 3 head 853b1665 22:55:10

Generator time, head 853b1665 against base b35eb2ee, median of 3 alternating rounds (same cores):

| host | base ms | head ms | head / base |
| --- | ---: | ---: | ---: |
| DotGram.Examples.Expressions.ArithmeticTree | 11 | 11 | 1.04x |
| DotGram.Examples.Expressions.Calculator | 173 | 182 | 1.05x |
| DotGram.Examples.Expressions.ClampedExample | 6 | 6 | 1.04x |
| DotGram.Examples.Expressions.LocaleNumber | 4 | 4 | 1.03x |
| DotGram.Examples.Feeds.FeedReader | 28 | 29 | 1.04x |
| DotGram.Examples.Feeds.LoggingFeedReader | 6 | 6 | 1.04x |
| DotGram.Examples.Feeds.RecoveringFeedReader | 4 | 4 | 1.03x |
| DotGram.Examples.Feeds.StockCountReader | 5 | 5 | 1.03x |
| DotGram.Examples.Feeds.StreamingFeedReader | 10 | 10 | 1.05x |
| DotGram.Examples.Formats.Config | 14 | 15 | 1.03x |
| DotGram.Examples.Formats.Config.Located | 12 | 12 | 1.04x |
| DotGram.Examples.Formats.FileNames | 5 | 5 | 1.03x |
| DotGram.Examples.Formats.FixedWidth | 9 | 9 | 1.02x |
| DotGram.Examples.Formats.FixParser | 10 | 10 | 1.01x |
| DotGram.Examples.Formats.HttpParser | 4 | 15 | 3.70x |
| DotGram.Examples.Formats.IniParser | 5 | 5 | 1.02x |
| DotGram.Examples.Formats.JsonParser | 7 | 8 | 1.04x |
| DotGram.Examples.Formats.Links | 9 | 9 | 1.03x |
| DotGram.Examples.Formats.MarkdownParser | 6 | 7 | 1.04x |
| DotGram.Examples.Formats.MetricsLine | 14 | 14 | 1.04x |
| DotGram.Examples.Formats.Netstrings | 3 | 3 | 1.03x |
| DotGram.Examples.Formats.TypedCsv | 9 | 10 | 1.06x |
| DotGram.Examples.Formats.XmlParser | 6 | 6 | 1.01x |
| DotGram.Examples.Formats.YamlLite | 6 | 4 | 0.65x |
| DotGram.Examples.Languages.Filter | 10 | 10 | 1.01x |
| DotGram.Examples.Languages.FilterFile | 8 | 11 | 1.34x |
| DotGram.Examples.Languages.Filters | 12 | 12 | 1.01x |
| DotGram.Examples.Languages.GramGrammar | 119 | 126 | 1.06x |
| DotGram.Examples.Languages.Lexemes | 5 | 17 | 3.43x |
| DotGram.Examples.Languages.Scoped | 11 | 11 | 1.03x |
| DotGram.Examples.Languages.Selectors | 9 | 9 | 1.01x |
| DotGram.Examples.Languages.SettingsFile | 11 | 11 | 1.03x |
| DotGram.Examples.Languages.SqlDialect | 7 | 7 | 1.01x |
| DotGram.Examples.Languages.SqlReadOnly | 18 | 18 | 1.01x |
| DotGram.Examples.Languages.TokenizedQuery | 89 | 92 | 1.03x |
| DotGram.Sql.Standard.Sql92Parser | 723 | 766 | 1.06x |
| DotGram.Sql.Standard.SqlStandardParser | 3,480 | 3,475 | 1.00x |
| DotGram.Sql.TransactSql.TransactSqlParser | 4,398 | 4,553 | 1.04x |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | 2,139 | 2,205 | 1.03x |

No host moved by more than the tolerance.
