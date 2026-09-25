# Which carrier each grammar is read with, and why

Every grammar the last build with `-p:DotGramReportGeneration=full` compiled: the carrier
`Auto` took (GRAM5012), and for a grammar kept on the tape, the gate that kept it and each rule
held there. Written by `--carriers` (`benchmarks/DotGram.Benchmarks/Carriers.cs`) from the reports
that build left; run again rather than edited.

Read from 8 projects, and written when each one was last compiled with the
report on. A project built below that level leaves no report and is absent here rather than
empty, so a short table is a short build and not a grammar with nothing to say; a project whose
time is older than the rest was not in the last build, and its rows are that build's answer.

| Read from | Report written |
| --- | --- |
| DotGram.Benchmarks | 2026-09-24 21:11 |
| DotGram.Examples | 2026-09-24 21:10 |
| DotGram.ExpressionLanguage | 2026-09-24 21:10 |
| DotGram.Finance | 2026-09-24 21:10 |
| DotGram.Finance.Fix44 | 2026-09-24 21:10 |
| DotGram.Sql | 2026-09-24 21:10 |
| DotGram.Tests | 2026-09-24 21:11 |
| DotGram.Web | 2026-09-24 21:10 |

**Carrier** is what `Auto` took: `immediate`, `tape`, or the author's own choice. **Gate** is what
kept a grammar on the tape: `replay` — a building rule read where the reading may not stand
(`Replay`) — or `read again` — a rule the reader can be asked again after it answered, which is
asked only where the first gate let everything through. **Direct** is how many of the replayed
rules have a cause of their own; the rest are under one of them. **Points** is how many of the
sites that build — a call whose value is built, a construction — have a point past which what they
read is settled (`Commit`), of how many there are: what building at that point could take off the
tape. **Refused** is how many of a grammar's machines the immediate carrier refuses before any gate
is asked (gate `refused` where nothing else kept it), each named under the grammar with its
reason; their building rules count in **Building**. **Alone** is how many of those neither gate
would keep on the tape: what lifting the refusal would move.

A carrier is chosen per machine, and a grammar has one machine per publication group and input
form, so the grammar's row is a **summary** and shows the worst of its machines. What a machine
answers is in the table below it, and that is the row to read before expecting anything of a
change: a grammar on the tape may have a machine that is not.

| Grammar (summary) | Machines | On the tape | Carrier | Gate | Building | Replayed | Direct | Read again | Refused | Alone | Points |
| --- | ---: | ---: | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| DotGram.Benchmarks.CallCost.Called | 1 | 0 | immediate | none |  |  |  |  |  |  | 1/1 |
| DotGram.Benchmarks.CallCost.Inlined | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 1/1 |
| DotGram.Benchmarks.CallCost.Valued | 1 | 0 | immediate | none |  |  |  |  |  |  | 5/5 |
| DotGram.Benchmarks.Climbing | 1 | 1 | tape | read again | 1 | 0 | 0 | 1 | 0 | 0 | 13/13 |
| DotGram.Benchmarks.Config | 1 | 0 | immediate | none |  |  |  |  |  |  | 7/7 |
| DotGram.Benchmarks.Extents | 1 | 0 | immediate | none |  |  |  |  |  |  | 1/1 |
| DotGram.Benchmarks.Feed | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 7/7 |
| DotGram.Benchmarks.Flat.Lowered | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 0/0 |
| DotGram.Benchmarks.Flat.NotLowered | 1 | 0 | immediate | none |  |  |  |  |  |  | 0/0 |
| DotGram.Benchmarks.ImmediateSql |  |  | immediate (author) |  |  |  |  |  |  |  | 12/242 |
| DotGram.Benchmarks.Levels | 1 | 0 | immediate | none |  |  |  |  |  |  | 19/19 |
| DotGram.Benchmarks.MaterializationCost.NoCaptures | 1 | 1 | tape | read again | 1 | 0 | 0 | 2 | 0 | 0 | 1/1 |
| DotGram.Benchmarks.MaterializationCost.SpanCaptures | 1 | 1 | tape | replay | 7 | 1 | 1 | 0 | 0 | 0 | 17/17 |
| DotGram.Benchmarks.MaterializationCost.WithCaptures | 1 | 1 | tape | read again | 1 | 0 | 0 | 2 | 0 | 0 | 1/1 |
| DotGram.Benchmarks.Nesting | 1 | 1 | nothing to choose | none |  |  |  |  |  |  | 0/0 |
| DotGram.Benchmarks.Numbers | 1 | 0 | immediate | none |  |  |  |  |  |  | 3/3 |
| DotGram.Benchmarks.Possession.Open | 1 | 1 | nothing to choose | none |  |  |  |  |  |  | 0/0 |
| DotGram.Benchmarks.Possession.Settled | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 0/0 |
| DotGram.Benchmarks.Settlements | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 7/7 |
| DotGram.Benchmarks.TinyScalar | 1 | 0 | immediate | none |  |  |  |  |  |  | 3/3 |
| DotGram.Benchmarks.Urls | 1 | 1 | tape | read again | 2 | 0 | 0 | 3 | 0 | 0 | 1/1 |
| DotGram.Examples.Expressions.ArithmeticTree | 1 | 0 | immediate | none |  |  |  |  |  |  | 22/22 |
| DotGram.Examples.Expressions.Calculator | 3 | 3 | tape | read again | 6 | 0 | 0 | 8 | 0 | 0 | 51/51 |
| DotGram.Examples.Expressions.ClampedExample | 1 | 0 | immediate | none |  |  |  |  |  |  | 14/14 |
| DotGram.Examples.Expressions.LocaleNumber | 2 | 2 | tape | read again | 2 | 0 | 0 | 2 | 0 | 0 | 4/4 |
| DotGram.Examples.Feeds.FeedReader | 1 | 1 | tape | read again | 5 | 0 | 0 | 5 | 0 | 0 | 5/5 |
| DotGram.Examples.Feeds.LoggingFeedReader | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 5/5 |
| DotGram.Examples.Feeds.RecoveringFeedReader | 1 | 0 | immediate | none |  |  |  |  |  |  | 6/6 |
| DotGram.Examples.Feeds.StockCountReader | 2 | 0 | immediate | none |  |  |  |  |  |  | 5/5 |
| DotGram.Examples.Feeds.StreamingFeedReader | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 9/9 |
| DotGram.Examples.Formats.Config | 2 | 1 | tape | read again | 3 | 0 | 0 | 3 | 0 | 0 | 6/6 |
| DotGram.Examples.Formats.Config.Located | 2 | 1 | tape | read again | 3 | 0 | 0 | 3 | 0 | 0 | 6/6 |
| DotGram.Examples.Formats.FileNames | 1 | 1 | tape | read again | 2 | 0 | 0 | 2 | 0 | 0 | 4/4 |
| DotGram.Examples.Formats.FixParser | 1 | 0 | immediate | none |  |  |  |  |  |  | 9/9 |
| DotGram.Examples.Formats.FixedWidth | 1 | 0 | immediate | none |  |  |  |  |  |  | 22/22 |
| DotGram.Examples.Formats.HttpParser | 1 | 1 | tape | read again | 5 | 0 | 0 | 6 | 0 | 0 | 10/10 |
| DotGram.Examples.Formats.IniParser | 1 | 1 | tape | read again | 7 | 0 | 0 | 11 | 0 | 0 | 14/14 |
| DotGram.Examples.Formats.JsonParser | 1 | 1 | tape | read again | 9 | 0 | 0 | 11 | 0 | 0 | 30/30 |
| DotGram.Examples.Formats.Links | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 1/1 |
| DotGram.Examples.Formats.MarkdownParser | 1 | 1 | tape | read again | 9 | 0 | 0 | 8 | 0 | 0 | 24/24 |
| DotGram.Examples.Formats.MetricsLine | 1 | 1 | tape | read again | 5 | 0 | 0 | 3 | 0 | 0 | 11/11 |
| DotGram.Examples.Formats.Netstrings | 1 | 1 | tape | read again | 2 | 0 | 0 | 1 | 0 | 0 | 3/3 |
| DotGram.Examples.Formats.TypedCsv | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 12/12 |
| DotGram.Examples.Formats.XmlParser | 1 | 1 | tape | replay | 7 | 6 | 3 | 0 | 0 | 0 | 21/21 |
| DotGram.Examples.Formats.YamlLite | 1 | 1 | tape | read again | 6 | 0 | 0 | 7 | 0 | 0 | 11/11 |
| DotGram.Examples.Languages.Filter | 1 | 1 | tape | replay | 9 | 6 | 2 | 0 | 0 | 0 | 33/33 |
| DotGram.Examples.Languages.FilterFile | 1 | 0 | immediate | none |  |  |  |  |  |  | 4/4 |
| DotGram.Examples.Languages.Filters | 1 | 1 | tape | replay | 2 | 1 | 1 | 0 | 0 | 0 | 8/8 |
| DotGram.Examples.Languages.GramGrammar | 1 | 1 | tape | replay | 35 | 28 | 2 | 0 | 0 | 0 | 108/108 |
| DotGram.Examples.Languages.Lexemes | 1 | 1 | nothing to choose | none |  |  |  |  |  |  | 0/0 |
| DotGram.Examples.Languages.Scoped | 1 | 1 | tape | read again | 4 | 0 | 0 | 5 | 0 | 0 | 13/13 |
| DotGram.Examples.Languages.Selectors | 1 | 1 | tape | read again | 6 | 0 | 0 | 1 | 0 | 0 | 15/15 |
| DotGram.Examples.Languages.SettingsFile | 1 | 0 | immediate | none |  |  |  |  |  |  | 3/3 |
| DotGram.Examples.Languages.SqlDialect | 1 | 0 | immediate | none |  |  |  |  |  |  | 3/3 |
| DotGram.Examples.Languages.SqlReadOnly | 1 | 1 | nothing to choose | none |  |  |  |  |  |  | 0/0 |
| DotGram.Examples.Languages.TokenizedQuery | 1 | 0 | immediate | none |  |  |  |  |  |  | 12/12 |
| DotGram.ExpressionLanguage.ExpressionParser | 2 | 2 | tape | replay | 149 | 143 | 24 | 0 | 2 | 0 | 28/906 |
| DotGram.ExpressionLanguage.ExpressionParser.Immediate |  |  | immediate (author) |  |  |  |  |  |  |  | 28/906 |
| DotGram.Finance.Fix.Fix44.Fix44Grammar | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 1830/1830 |
| DotGram.Finance.Fix.Fix44.FixFieldGrammar | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 0/0 |
| DotGram.Finance.Fix.FixGrammar | 10 | 0 | immediate | none |  |  |  |  |  |  | 14/14 |
| DotGram.Sql.Standard.Sql92Parser | 1 | 1 | tape | replay | 48 | 44 | 4 | 0 | 0 | 0 | 12/242 |
| DotGram.Sql.Standard.SqlStandardParser | 4 | 4 | tape | replay | 556 | 308 | 25 | 0 | 2 | 0 | 1284/2641 |
| DotGram.Sql.TransactSql.TransactSqlParser | 2 | 2 | tape | replay | 659 | 322 | 80 | 0 | 1 | 0 | 2362/3149 |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | 2 | 2 | tape | replay | 659 | 322 | 80 | 0 | 1 | 0 | 2362/3149 |
| DotGram.Tests.Calculators.DecimalCalculator | 1 | 0 | immediate | none |  |  |  |  |  |  | 18/18 |
| DotGram.Tests.Calculators.OneRuleParser | 1 | 1 | tape | read again | 1 | 0 | 0 | 1 | 0 | 0 | 15/15 |
| DotGram.Tests.Calculators.StrengthCalculator | 1 | 1 | tape | read again | 1 | 0 | 0 | 1 | 0 | 0 | 15/15 |
| DotGram.Tests.Calculators.TwoCalculators | 2 | 0 | immediate | none |  |  |  |  |  |  | 34/34 |
| DotGram.Tests.Extents | 1 | 0 | immediate | none |  |  |  |  |  |  | 1/1 |
| DotGram.Tests.Generated.UrlGrammar | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 1/1 |
| DotGram.Web.Rfc3339 | 1 | 0 | immediate | none |  |  |  |  |  |  | 5/5 |
| DotGram.Web.Rfc3986 | 1 | 1 | tape | read again | 6 | 0 | 0 | 12 | 0 | 0 | 19/19 |
| DotGram.Web.Rfc5322 | 5 | 5 | tape | read again | 48 | 0 | 0 | 107 | 0 | 0 | 129/129 |
| DotGram.Web.Rfc5646 | 1 | 1 | tape | read again | 9 | 0 | 0 | 6 | 0 | 0 | 22/22 |
| DotGram.Web.Rfc6265 | 6 | 5 | tape | read again | 6 | 0 | 0 | 6 | 0 | 0 | 29/29 |
| DotGram.Web.Rfc6266 | 1 | 1 | tape | read again | 3 | 0 | 0 | 2 | 0 | 0 | 5/5 |
| DotGram.Web.Rfc6570 | 1 | 0 | immediate | none |  |  |  |  |  |  | 10/10 |
| DotGram.Web.Rfc6901 | 2 | 0 | immediate | none |  |  |  |  |  |  | 4/4 |
| DotGram.Web.Rfc7239 | 2 | 2 | tape | read again | 7 | 0 | 0 | 9 | 0 | 0 | 16/16 |
| DotGram.Web.Rfc8259 | 1 | 0 | immediate | none |  |  |  |  |  |  | 29/29 |
| DotGram.Web.Rfc8288 | 1 | 1 | tape | read again | 5 | 0 | 0 | 7 | 0 | 0 | 9/9 |
| DotGram.Web.Rfc9110 | 1 | 1 | tape | read again | 4 | 0 | 0 | 7 | 0 | 0 | 13/13 |
| DotGram.Web.Rfc9651 | 3 | 3 | tape | read again | 15 | 0 | 0 | 17 | 0 | 0 | 44/44 |

## Machine by machine

One row a machine: what it publishes, the form it reads (`whole` for a text held whole, `buffered`
for a reader, `bytes` for a byte stream), and its own carrier, gate and counts. The counts are the
machine's own, and so are the points: a site belongs to the machine that reads the rule it stands in.

| Grammar | Publishes | Form | Carrier | Gate | Building | Replayed | Read again | Refused | Points |
| --- | --- | --- | --- | --- | ---: | ---: | ---: | ---: | ---: |
| DotGram.Benchmarks.CallCost.Called | ParseStart | whole | immediate | none | 0 | 0 | 0 | 0 | 1/1 |
| DotGram.Benchmarks.CallCost.Valued | ParseStart | whole | immediate | none | 0 | 0 | 0 | 0 | 5/5 |
| DotGram.Benchmarks.Climbing | Climbed | whole | tape | read again | 1 | 0 | 1 | 0 | 13/13 |
| DotGram.Benchmarks.Config | ParseFile | whole | immediate | none | 0 | 0 | 0 | 0 | 7/7 |
| DotGram.Benchmarks.Extents | ParseLetters | whole | immediate | none | 0 | 0 | 0 | 0 | 1/1 |
| DotGram.Benchmarks.Flat.NotLowered | ParseDoc | whole | immediate | none | 0 | 0 | 0 | 0 | 0/0 |
| DotGram.Benchmarks.Levels | Levelled | whole | immediate | none | 0 | 0 | 0 | 0 | 19/19 |
| DotGram.Benchmarks.MaterializationCost.NoCaptures | ParseUrl | whole | tape | read again | 1 | 0 | 2 | 0 | 1/1 |
| DotGram.Benchmarks.MaterializationCost.SpanCaptures | ParseUrl | whole | tape | replay | 7 | 1 | 0 | 0 | 17/17 |
| DotGram.Benchmarks.MaterializationCost.WithCaptures | ParseUrl | whole | tape | read again | 1 | 0 | 2 | 0 | 1/1 |
| DotGram.Benchmarks.Nesting | ParseExpr | whole | tape | none | 0 | 0 | 0 | 0 | 0/0 |
| DotGram.Benchmarks.Numbers | ParseSum | whole | immediate | none | 0 | 0 | 0 | 0 | 3/3 |
| DotGram.Benchmarks.Possession.Open | ParseDoc | whole | tape | none | 0 | 0 | 0 | 0 | 0/0 |
| DotGram.Benchmarks.TinyScalar | ParseDepth | whole | immediate | none | 0 | 0 | 0 | 0 | 3/3 |
| DotGram.Benchmarks.Urls | ParseUrl | whole | tape | read again | 2 | 0 | 3 | 0 | 1/1 |
| DotGram.Examples.Expressions.ArithmeticTree | Read | whole | immediate | none | 0 | 0 | 0 | 0 | 22/22 |
| DotGram.Examples.Expressions.Calculator | EvaluateInt | whole | tape | read again | 2 | 0 | 3 | 0 | 17/17 |
| DotGram.Examples.Expressions.Calculator | EvaluateDecimal | whole | tape | read again | 2 | 0 | 5 | 0 | 17/17 |
| DotGram.Examples.Expressions.Calculator | BuildTree | whole | tape | read again | 2 | 0 | 5 | 0 | 17/17 |
| DotGram.Examples.Expressions.ClampedExample | Read | whole | immediate | none | 0 | 0 | 0 | 0 | 14/14 |
| DotGram.Examples.Expressions.LocaleNumber | ParseNumber | whole | tape | read again | 1 | 0 | 1 | 0 | 2/2 |
| DotGram.Examples.Expressions.LocaleNumber | ParseEuropeanNumber | whole | tape | read again | 1 | 0 | 1 | 0 | 2/2 |
| DotGram.Examples.Feeds.FeedReader | ParseFeed | whole | tape | read again | 5 | 0 | 5 | 0 | 5/5 |
| DotGram.Examples.Feeds.RecoveringFeedReader | ParseFeed | whole | immediate | none | 0 | 0 | 0 | 0 | 6/6 |
| DotGram.Examples.Feeds.StockCountReader | ParseCount | whole | immediate | none | 0 | 0 | 0 | 0 | 5/5 |
| DotGram.Examples.Feeds.StockCountReader | ParseCount | buffered | immediate | none | 0 | 0 | 0 | 0 | 5/5 |
| DotGram.Examples.Formats.Config | ParseFile | whole | tape | read again | 3 | 0 | 3 | 0 | 5/5 |
| DotGram.Examples.Formats.Config | ParseKeySpan | whole | immediate | none | 0 | 0 | 0 | 0 | 1/1 |
| DotGram.Examples.Formats.Config.Located | ParseFile | whole | tape | read again | 3 | 0 | 3 | 0 | 5/5 |
| DotGram.Examples.Formats.Config.Located | ParseKeySpan | whole | immediate | none | 0 | 0 | 0 | 0 | 1/1 |
| DotGram.Examples.Formats.FileNames | ParseRoute, ParseSegment | whole | tape | read again | 2 | 0 | 2 | 0 | 4/4 |
| DotGram.Examples.Formats.FixParser | ParseMessage | whole | immediate | none | 0 | 0 | 0 | 0 | 9/9 |
| DotGram.Examples.Formats.FixedWidth | ParseFeed | whole | immediate | none | 0 | 0 | 0 | 0 | 22/22 |
| DotGram.Examples.Formats.HttpParser | ParseHeaders | whole | tape | read again | 5 | 0 | 6 | 0 | 10/10 |
| DotGram.Examples.Formats.IniParser | ParseIni | whole | tape | read again | 7 | 0 | 11 | 0 | 14/14 |
| DotGram.Examples.Formats.JsonParser | ParseJson | whole | tape | read again | 9 | 0 | 11 | 0 | 30/30 |
| DotGram.Examples.Formats.MarkdownParser | ParseDoc | whole | tape | read again | 9 | 0 | 8 | 0 | 24/24 |
| DotGram.Examples.Formats.MetricsLine | ParseLine | whole | tape | read again | 5 | 0 | 3 | 0 | 11/11 |
| DotGram.Examples.Formats.Netstrings | ParseStream | whole | tape | read again | 2 | 0 | 1 | 0 | 3/3 |
| DotGram.Examples.Formats.XmlParser | ParseXml | whole | tape | replay | 7 | 6 | 0 | 0 | 21/21 |
| DotGram.Examples.Formats.YamlLite | ParseDoc | whole | tape | read again | 6 | 0 | 7 | 0 | 11/11 |
| DotGram.Examples.Languages.Filter | ParseFilter | whole | tape | replay | 9 | 6 | 0 | 0 | 33/33 |
| DotGram.Examples.Languages.FilterFile | ParseFilter | whole | immediate | none | 0 | 0 | 0 | 0 | 4/4 |
| DotGram.Examples.Languages.Filters | ParseFilter | whole | tape | replay | 2 | 1 | 0 | 0 | 8/8 |
| DotGram.Examples.Languages.GramGrammar | ParseFile | whole | tape | replay | 35 | 28 | 0 | 0 | 108/108 |
| DotGram.Examples.Languages.Lexemes | ParseQuoted | whole | tape | none | 0 | 0 | 0 | 0 | 0/0 |
| DotGram.Examples.Languages.Scoped | ParseProgram | whole | tape | read again | 4 | 0 | 5 | 0 | 13/13 |
| DotGram.Examples.Languages.Selectors | ParseSelector | whole | tape | read again | 6 | 0 | 1 | 0 | 13/13 |
| DotGram.Examples.Languages.SettingsFile | ParseSettings | whole | immediate | none | 0 | 0 | 0 | 0 | 3/3 |
| DotGram.Examples.Languages.SqlDialect | ParseOld, ParseNew, ParseAny | whole | immediate | none | 0 | 0 | 0 | 0 | 3/3 |
| DotGram.Examples.Languages.SqlReadOnly | ParseQuery | whole | tape | none | 0 | 0 | 0 | 0 | 0/0 |
| DotGram.Examples.Languages.TokenizedQuery | ParseQuery | whole | immediate | none | 0 | 0 | 0 | 0 | 12/12 |
| DotGram.ExpressionLanguage.ExpressionParser | ParseLambda, ParseHole, ParseBody | whole | tape | replay | 90 | 87 | 0 | 1 | 14/453 |
| DotGram.ExpressionLanguage.ExpressionParser | ParseAsciiLambda, ParseHole_With2, ParseBody_With3 | whole | tape | replay | 90 | 87 | 0 | 1 | 14/453 |
| DotGram.Finance.Fix.FixGrammar | ParseFields | whole | immediate | none | 0 | 0 | 0 | 0 | 6/6 |
| DotGram.Finance.Fix.FixGrammar | ParseLogFields | whole | immediate | none | 0 | 0 | 0 | 0 | 6/6 |
| DotGram.Finance.Fix.FixGrammar | ParseFields | buffered | immediate | none | 0 | 0 | 0 | 0 | 6/6 |
| DotGram.Finance.Fix.FixGrammar | ParseFields | bytes | immediate | none | 0 | 0 | 0 | 0 | 6/6 |
| DotGram.Finance.Fix.FixGrammar | ReadFields | buffered | immediate | none | 0 | 0 | 0 | 0 | 6/6 |
| DotGram.Finance.Fix.FixGrammar | ReadFields | bytes | immediate | none | 0 | 0 | 0 | 0 | 6/6 |
| DotGram.Finance.Fix.FixGrammar | ParseLogFields | buffered | immediate | none | 0 | 0 | 0 | 0 | 6/6 |
| DotGram.Finance.Fix.FixGrammar | ParseLogFields | bytes | immediate | none | 0 | 0 | 0 | 0 | 6/6 |
| DotGram.Finance.Fix.FixGrammar | ReadLogFields | buffered | immediate | none | 0 | 0 | 0 | 0 | 6/6 |
| DotGram.Finance.Fix.FixGrammar | ReadLogFields | bytes | immediate | none | 0 | 0 | 0 | 0 | 6/6 |
| DotGram.Sql.Standard.Sql92Parser | ParseSelect, ParseQuery, ParseSearchCondition and 1 more | whole | tape | replay | 48 | 44 | 0 | 0 | 12/242 |
| DotGram.Sql.Standard.SqlStandardParser | ParseLiteral | whole | tape | replay | 13 | 12 | 0 | 0 | 4/35 |
| DotGram.Sql.Standard.SqlStandardParser | ParseTableName | whole | tape | replay | 4 | 3 | 0 | 0 | 2/12 |
| DotGram.Sql.Standard.SqlStandardParser | ParseValueExpression, ParseUnsignedLiteral, ParseColumnReference and 21 more | whole | tape | replay | 306 | 290 | 0 | 1 | 32/1363 |
| DotGram.Sql.Standard.SqlStandardParser | ParseDirectSQLStatement, ParseSQLSchemaStatement, ParseUpdateStatementPositioned and 11 more | whole | tape | replay | 539 | 308 | 0 | 1 | 1248/2605 |
| DotGram.Sql.TransactSql.TransactSqlParser | ParseSelect, ParseQuery, ParseSearchCondition and 1 more | whole | tape | replay | 179 | 174 | 0 | 0 | 37/812 |
| DotGram.Sql.TransactSql.TransactSqlParser | ParseStatement, ParseStatement100, ParseStatement110 and 24 more | whole | tape | replay | 655 | 322 | 0 | 1 | 2354/3141 |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | ParseSelect, ParseQuery, ParseSearchCondition and 1 more | whole | tape | replay | 179 | 174 | 0 | 0 | 37/812 |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | ParseStatement, ParseStatement100, ParseStatement110 and 24 more | whole | tape | replay | 655 | 322 | 0 | 1 | 2354/3141 |
| DotGram.Tests.Calculators.DecimalCalculator | Evaluate | whole | immediate | none | 0 | 0 | 0 | 0 | 18/18 |
| DotGram.Tests.Calculators.OneRuleParser | Read | whole | tape | read again | 1 | 0 | 1 | 0 | 15/15 |
| DotGram.Tests.Calculators.StrengthCalculator | Evaluate | whole | tape | read again | 1 | 0 | 1 | 0 | 15/15 |
| DotGram.Tests.Calculators.TwoCalculators | EvaluateInt | whole | immediate | none | 0 | 0 | 0 | 0 | 17/17 |
| DotGram.Tests.Calculators.TwoCalculators | EvaluateDouble | whole | immediate | none | 0 | 0 | 0 | 0 | 17/17 |
| DotGram.Tests.Extents | ParseExtent | whole | immediate | none | 0 | 0 | 0 | 0 | 1/1 |
| DotGram.Web.Rfc3339 | ParseTimestamp, ParseFullDate, ParseFullTime | whole | immediate | none | 0 | 0 | 0 | 0 | 5/5 |
| DotGram.Web.Rfc3986 | ParseReference, ParseUri | whole | tape | read again | 6 | 0 | 12 | 0 | 19/19 |
| DotGram.Web.Rfc5322 | ParseAddressList, ParseMailboxList, ParseMailbox and 1 more | whole | tape | read again | 17 | 0 | 32 | 0 | 47/47 |
| DotGram.Web.Rfc5322 | ParseStrictAddrSpec | whole | tape | read again | 4 | 0 | 15 | 0 | 9/9 |
| DotGram.Web.Rfc5322 | ParseStrictMailbox | whole | tape | read again | 6 | 0 | 18 | 0 | 15/15 |
| DotGram.Web.Rfc5322 | ParseStrictMailboxList | whole | tape | read again | 8 | 0 | 20 | 0 | 21/21 |
| DotGram.Web.Rfc5322 | ParseStrictAddressList | whole | tape | read again | 13 | 0 | 25 | 0 | 37/37 |
| DotGram.Web.Rfc5646 | ParseTag | whole | tape | read again | 9 | 0 | 6 | 0 | 22/22 |
| DotGram.Web.Rfc6265 | ParseSetCookie | whole | immediate | none | 0 | 0 | 0 | 0 | 5/5 |
| DotGram.Web.Rfc6265 | ReadDateTokens | whole | tape | read again | 2 | 0 | 1 | 0 | 3/3 |
| DotGram.Web.Rfc6265 | ReadTime | whole | tape | read again | 1 | 0 | 2 | 0 | 1/1 |
| DotGram.Web.Rfc6265 | ReadDay | whole | tape | read again | 1 | 0 | 2 | 0 | 1/1 |
| DotGram.Web.Rfc6265 | ReadMonth | whole | tape | read again | 1 | 0 | 1 | 0 | 12/12 |
| DotGram.Web.Rfc6265 | ReadYear | whole | tape | read again | 1 | 0 | 2 | 0 | 1/1 |
| DotGram.Web.Rfc6266 | ParseContentDisposition | whole | tape | read again | 3 | 0 | 2 | 0 | 5/5 |
| DotGram.Web.Rfc6570 | ParseTemplate | whole | immediate | none | 0 | 0 | 0 | 0 | 10/10 |
| DotGram.Web.Rfc6901 | ParsePointer | whole | immediate | none | 0 | 0 | 0 | 0 | 3/3 |
| DotGram.Web.Rfc6901 | ParseFragment | whole | immediate | none | 0 | 0 | 0 | 0 | 1/1 |
| DotGram.Web.Rfc7239 | ParseForwarded | whole | tape | read again | 6 | 0 | 4 | 0 | 12/12 |
| DotGram.Web.Rfc7239 | ParseNode | whole | tape | read again | 1 | 0 | 5 | 0 | 4/4 |
| DotGram.Web.Rfc8259 | ParseJson | whole | immediate | none | 0 | 0 | 0 | 0 | 29/29 |
| DotGram.Web.Rfc8288 | ParseLinks | whole | tape | read again | 5 | 0 | 7 | 0 | 9/9 |
| DotGram.Web.Rfc9110 | ParseContentType | whole | tape | read again | 4 | 0 | 7 | 0 | 7/7 |
| DotGram.Web.Rfc9651 | ParseItem | whole | tape | read again | 7 | 0 | 9 | 0 | 22/22 |
| DotGram.Web.Rfc9651 | ParseList | whole | tape | read again | 11 | 0 | 13 | 0 | 34/34 |
| DotGram.Web.Rfc9651 | ParseDictionary | whole | tape | read again | 12 | 0 | 14 | 0 | 37/37 |

## Where the second gate's ways are opened

Each place a rule's own reading opens a way, by its shape — a choice over characters, a run of
one character, the turns of a repetition — and why the way could not be left out. **Places**
counts each once per grammar; **captured** is how many of them capture inside the turn, and
**sealed** how many are in a rule every call of which is inside an atomic group or a lookahead,
or which nothing calls, so that no caller asks it again.

| Shape | Why the way stays | Grammars | Rules | Places | Captured | Sealed | For example |
| --- | --- | ---: | ---: | ---: | ---: | ---: | --- |
| choice | alternatives begin alike | 13 | 22 | 37 | 0 | 8 | NoCaptures.Host: `(IPv4 \| RegName)` |
| turns | a turn led by what may read nothing | 8 | 13 | 25 | 8 | 2 | IniParser.Entries: `(item0: Entry \| Blank)*` |
| run | what follows begins alike | 14 | 24 | 24 | 0 | 0 | Calculator.Spacing: `Whitespace+` |
| choice | alternatives begin apart | 1 | 6 | 19 | 0 | 0 | Rfc5322.Ctext: `(['!'..'\'' \| '*'..'[' \| ']'..'~'] \| Never)` |
| optional | what follows begins alike | 12 | 13 | 17 | 8 | 3 | NoCaptures.Url: `(UserInfo & '@')?` |
| choice | every alternative led by what may read nothing | 4 | 8 | 17 | 0 | 1 | IniParser.Entries: `(item0: Entry \| Blank)` |
| choice | an alternative that may read nothing | 7 | 13 | 14 | 0 | 1 | HttpParser.Field: `(eol \| ?=eof)` |
| counted | what follows begins alike | 3 | 3 | 11 | 1 | 0 | Rfc3986.IPv6Address: `(H16 & ':'){0,2}` |
| turns | what follows begins alike | 5 | 7 | 10 | 5 | 2 | JsonParser.Body: `(Plain \| Escape)*` |
| turns | the seam leads every alternative of the turn | 5 | 5 | 7 | 7 | 0 | Climbing.Expr: `(trivia & '+' & trivia & r: Expr => (l + r) \| trivia & '-' & trivia & …` |
| optional | a turn led by what may read nothing | 2 | 4 | 6 | 1 | 0 | Rfc3986.Authority: `(user: UserInfoText & '@')?` |
| choice | the seam leads every alternative | 2 | 2 | 4 | 0 | 0 | Calculator.Expr: `(trivia & '+' & trivia & right: Expr_With1 => (left + right) \| trivia …` |
| choice | literals, a shorter one wanted | 4 | 4 | 4 | 0 | 0 | HttpParser.eol: `("\r\n" \| '\r')` |
| choice | literals, follow unknown | 1 | 1 | 1 | 0 | 0 | FeedReader.eol: `("\r\n" \| '\r')` |
| turns | seam first, what follows begins alike past it | 1 | 1 | 1 | 0 | 1 | Scoped.Program: `(trivia & Let)*` |

## DotGram.Benchmarks.CallCost.Called

- machine ParseStart [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 1/1

## DotGram.Benchmarks.CallCost.Valued

- machine ParseStart [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 5/5

## DotGram.Benchmarks.Climbing

- machine Climbed [whole]: carrier: tape; gate: read again; building: 1; replayed: 0; read again: 1; refused: 0; points: 13/13
- again Expr: opens a way
- open Expr: turns, captured; the seam leads every alternative of the turn; open; (trivia & '+' & trivia & r: Expr => (l + r) | trivia & '-' & trivia & …

## DotGram.Benchmarks.Config

- machine ParseFile [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 7/7

## DotGram.Benchmarks.Extents

- machine ParseLetters [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 1/1

## DotGram.Benchmarks.Flat.NotLowered

- machine ParseDoc [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 0/0

## DotGram.Benchmarks.Levels

- machine Levelled [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 19/19

## DotGram.Benchmarks.MaterializationCost.NoCaptures

- machine ParseUrl [whole]: carrier: tape; gate: read again; building: 1; replayed: 0; read again: 2; refused: 0; points: 1/1
- again Host: opens a way
- open Host: choice; alternatives begin alike; open; (IPv4 | RegName)
- again Url: opens a way
- open Url: optional; what follows begins alike; entry; (UserInfo & '@')?

## DotGram.Benchmarks.MaterializationCost.SpanCaptures

- machine ParseUrl [whole]: carrier: tape; gate: replay; building: 7; replayed: 1; read again: 0; refused: 0; points: 17/17
- replay UserInfo: Follows in Url [turn], then '@'

## DotGram.Benchmarks.MaterializationCost.WithCaptures

- machine ParseUrl [whole]: carrier: tape; gate: read again; building: 1; replayed: 0; read again: 2; refused: 0; points: 1/1
- again Host: opens a way
- open Host: choice; alternatives begin alike; open; (IPv4 | RegName)
- again Url: opens a way
- open Url: optional, captured; what follows begins alike; entry; (user: UserInfo & '@')?

## DotGram.Benchmarks.Nesting

- machine ParseExpr [whole]: carrier: tape; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 0/0

## DotGram.Benchmarks.Numbers

- machine ParseSum [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 3/3

## DotGram.Benchmarks.Possession.Open

- machine ParseDoc [whole]: carrier: tape; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 0/0

## DotGram.Benchmarks.TinyScalar

- machine ParseDepth [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 3/3

## DotGram.Benchmarks.Urls

- machine ParseUrl [whole]: carrier: tape; gate: read again; building: 2; replayed: 0; read again: 3; refused: 0; points: 1/1
- again Authority: opens a way
- open Authority: optional, captured; what follows begins alike; open; (user: UserInfo & '@')?
- again Host: opens a way
- open Host: choice; alternatives begin alike; open; (IPv4 | RegName)
- again Url: through Authority

## DotGram.Examples.Expressions.ArithmeticTree

- machine Read [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 22/22

## DotGram.Examples.Expressions.Calculator

- machine EvaluateInt [whole]: carrier: tape; gate: read again; building: 2; replayed: 0; read again: 3; refused: 0; points: 17/17
- machine EvaluateDecimal [whole]: carrier: tape; gate: read again; building: 2; replayed: 0; read again: 5; refused: 0; points: 17/17
- machine BuildTree [whole]: carrier: tape; gate: read again; building: 2; replayed: 0; read again: 5; refused: 0; points: 17/17
- again DecimalNumber: through Point
- again Expr: opens a way
- open Expr: turns, captured; the seam leads every alternative of the turn; open; (trivia & '+' & trivia & right: Expr_With1 => (left + right) | trivia …
- open Expr: choice; the seam leads every alternative; open; (trivia & '+' & trivia & right: Expr_With1 => (left + right) | trivia …
- again Expr: opens a way
- open Expr: turns, captured; the seam leads every alternative of the turn; open; (trivia & '+' & trivia & right: Expr_With2 => (left + right) | trivia …
- open Expr: choice; the seam leads every alternative; open; (trivia & '+' & trivia & right: Expr_With2 => (left + right) | trivia …
- again Expr: opens a way
- open Expr: turns, captured; the seam leads every alternative of the turn; open; (trivia & '+' & trivia & right: Expr_With3 => (left + right) | trivia …
- open Expr: choice; the seam leads every alternative; open; (trivia & '+' & trivia & right: Expr_With3 => (left + right) | trivia …
- again NodeNumber: through Point
- again Point: through trivia
- again Spacing: opens a way
- open Spacing: run; what follows begins alike; open; Whitespace+
- again trivia: opens a way
- open trivia: optional; what follows begins alike; open; Spacing?

## DotGram.Examples.Expressions.ClampedExample

- machine Read [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 14/14

## DotGram.Examples.Expressions.LocaleNumber

- machine ParseNumber [whole]: carrier: tape; gate: read again; building: 1; replayed: 0; read again: 1; refused: 0; points: 2/2
- machine ParseEuropeanNumber [whole]: carrier: tape; gate: read again; building: 1; replayed: 0; read again: 1; refused: 0; points: 2/2
- again Number: opens a way
- open Number: choice; alternatives begin alike; entry; (whole: Digit+ & Point & frac: Digit+ => (Whole(whole) + Fraction(frac…
- again Number: opens a way
- open Number: choice; alternatives begin alike; entry; (whole: Digit+ & Comma & frac: Digit+ => (Whole(whole) + Fraction(frac…

## DotGram.Examples.Feeds.FeedReader

- machine ParseFeed [whole]: carrier: tape; gate: read again; building: 5; replayed: 0; read again: 5; refused: 0; points: 5/5
- again Feed: through Header
- again Header: through eol
- again Row: through eol
- again Trailer: through eol
- again eol: opens a way
- open eol: choice; literals, follow unknown; open; ("\r\n" | '\r')

## DotGram.Examples.Feeds.RecoveringFeedReader

- machine ParseFeed [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 6/6

## DotGram.Examples.Feeds.StockCountReader

- machine ParseCount [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 5/5
- machine ParseCount [buffered]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 5/5

## DotGram.Examples.Formats.Config

- machine ParseFile [whole]: carrier: tape; gate: read again; building: 3; replayed: 0; read again: 3; refused: 0; points: 5/5
- machine ParseKeySpan [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 1/1
- again Entry: through Value
- again File: through Entry
- again Value: opens a way
- open Value: run; what follows begins alike; open; [^ '\n' | '\r']*

## DotGram.Examples.Formats.Config.Located

- machine ParseFile [whole]: carrier: tape; gate: read again; building: 3; replayed: 0; read again: 3; refused: 0; points: 5/5
- machine ParseKeySpan [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 1/1
- again Entry: through Value
- again File: through Entry
- again Value: opens a way
- open Value: run; what follows begins alike; open; [^ '\n' | '\r']*

## DotGram.Examples.Formats.FileNames

- machine ParseRoute, ParseSegment [whole]: carrier: tape; gate: read again; building: 2; replayed: 0; read again: 2; refused: 0; points: 4/4
- again Route: through Segment
- again Segment: opens a way
- open Segment: run; what follows begins alike; open; [IsAllowed]+

## DotGram.Examples.Formats.FixParser

- machine ParseMessage [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 9/9

## DotGram.Examples.Formats.FixedWidth

- machine ParseFeed [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 22/22

## DotGram.Examples.Formats.HttpParser

- machine ParseHeaders [whole]: carrier: tape; gate: read again; building: 5; replayed: 0; read again: 6; refused: 0; points: 10/10
- again Field: opens a way
- open Field: choice; an alternative that may read nothing; open; (eol | ?=eof)
- again Fold: opens a way
- open Fold: choice; an alternative that may read nothing; open; (eol | ?=eof)
- again Headers: through Field
- again Line: opens a way
- open Line: run; what follows begins alike; open; [^ '\n' | '\r']*
- again Space: opens a way
- open Space: run; what follows begins alike; open; ['\t' | ' ']*
- again eol: opens a way
- open eol: choice; literals, a shorter one wanted; open; ("\r\n" | '\r')

## DotGram.Examples.Formats.IniParser

- machine ParseIni [whole]: carrier: tape; gate: read again; building: 7; replayed: 0; read again: 11; refused: 0; points: 14/14
- again Blank: through Space
- again Comment: opens a way
- open Comment: run; what follows begins alike; open; [^ '\n' | '\r']*
- again Entries: opens a way
- open Entries: turns, captured; a turn led by what may read nothing; open; (item0: Entry | Blank)*
- open Entries: choice; every alternative led by what may read nothing; open; (item0: Entry | Blank)
- open Entries: optional; what follows begins alike; open; Tail?
- again Entry: opens a way
- open Entry: choice; an alternative that may read nothing; open; (eol | ?=eof)
- again Ini: through Entries
- again Key: opens a way
- open Key: run; what follows begins alike; open; [^ '\n' | '\r' | '#' | ';' | '=' | '[' | ']']+
- again Section: through Entries
- again Space: opens a way
- open Space: run; what follows begins alike; open; ['\t' | ' ']*
- again Tail: opens a way
- open Tail: choice; alternatives begin alike; open; (Space & Comment | Comment)
- again Value: opens a way
- open Value: run; what follows begins alike; open; [^ '\n' | '\r']*
- again eol: opens a way
- open eol: choice; literals, a shorter one wanted; open; ("\r\n" | '\r')

## DotGram.Examples.Formats.JsonParser

- machine ParseJson [whole]: carrier: tape; gate: read again; building: 9; replayed: 0; read again: 11; refused: 0; points: 30/30
- again Array: through trivia
- again Body: opens a way
- open Body: turns; what follows begins alike; open; (Plain | Escape)*
- again Json: through Value
- again List: through trivia
- again List: through Value
- again Member: through Value
- again Number: through trivia
- again Object: through trivia
- again Text: through trivia
- again Value: through Object
- again trivia: opens a way
- open trivia: run; what follows begins alike; open; ['\t'..'\n' | '\r' | ' ']*

## DotGram.Examples.Formats.MarkdownParser

- machine ParseDoc [whole]: carrier: tape; gate: read again; building: 9; replayed: 0; read again: 8; refused: 0; points: 24/24
- again Blank: through eol
- again Block: opens a way
- open Block: choice; alternatives begin alike; open; (block: Heading => (block) | block: Bullets => (block) | block: Code =…
- again Code: opens a way
- open Code: turns, captured; a turn led by what may read nothing; open; lines: CodeLine*
- again CodeLine: through eol
- again Doc: through Block
- again Heading: through eol
- again Paragraph: through eol
- again eol: opens a way
- open eol: choice; literals, a shorter one wanted; open; ("\r\n" | '\r')

## DotGram.Examples.Formats.MetricsLine

- machine ParseLine [whole]: carrier: tape; gate: read again; building: 5; replayed: 0; read again: 3; refused: 0; points: 11/11
- again Line: through Reading
- again Reading: through Value
- again Value: opens a way
- open Value: choice; alternatives begin alike; open; (?=Digits & trivia & '.' & trivia & d: Decimal => (d) | n: Long => (n)…

## DotGram.Examples.Formats.Netstrings

- machine ParseStream [whole]: carrier: tape; gate: read again; building: 2; replayed: 0; read again: 1; refused: 0; points: 3/3
- again Stream: opens a way
- open Stream: turns, captured; what follows begins alike; entry; item0: Frame*

## DotGram.Examples.Formats.XmlParser

- machine ParseXml [whole]: carrier: tape; gate: replay; building: 7; replayed: 6; read again: 0; refused: 0; points: 21/21
- replay Attribute: Follows in Element [choice], then '>'
- replay Content: Follows in Element [choice], then "</"
- replay Name: Follows in Element [choice], then '>'
- replay Element: under Content
- replay Text: under Content
- replay Value: under Attribute

## DotGram.Examples.Formats.YamlLite

- machine ParseDoc [whole]: carrier: tape; gate: read again; building: 6; replayed: 0; read again: 7; refused: 0; points: 11/11
- again Blank: through Space
- again Doc: through Lines
- again Line: opens a way
- open Line: choice; an alternative that may read nothing; open; (eol | ?=eof)
- again Lines: opens a way
- open Lines: choice; every alternative led by what may read nothing; open; (item0: Line | Blank)
- again Rest: opens a way
- open Rest: run; what follows begins alike; open; [^ '\n' | '\r']*
- again Space: opens a way
- open Space: run; what follows begins alike; open; ' '*
- again eol: opens a way
- open eol: choice; literals, a shorter one wanted; open; ("\r\n" | '\r')

## DotGram.Examples.Languages.Filter

- machine ParseFilter [whole]: carrier: tape; gate: replay; building: 9; replayed: 6; read again: 0; refused: 0; points: 33/33
- replay List: Follows in Expr [choice], then ')'
- replay Op: Follows in Expr [choice], then Value
- replay Body: under Text
- replay Number: under Value
- replay Text: under Value
- replay Value: under List

## DotGram.Examples.Languages.FilterFile

- machine ParseFilter [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 4/4

## DotGram.Examples.Languages.Filters

- machine ParseFilter [whole]: carrier: tape; gate: replay; building: 2; replayed: 1; read again: 0; refused: 0; points: 8/8
- replay Test: Follows in Test [choice], then ')'

## DotGram.Examples.Languages.GramGrammar

- machine ParseFile [whole]: carrier: tape; gate: replay; building: 35; replayed: 28; read again: 0; refused: 0; points: 108/108
- replay AnyTest: Follows in OneTest [choice], then ')'
- replay Reference: Follows in PublicationTarget [choice], then ?=':'
- replay AllTest: under AnyTest
- replay Alternative: under Argument
- replay Argument: under RefOrCall
- replay Body: under Primary
- replay Captured: under Prefixed
- replay CsExpr: under Primary
- replay ElemAlt: under ElementSet
- replay ElementSet: under Primary
- replay Glued: under Sequence
- replay Guard: under Operand
- replay GuardBody: under Guard
- replay Marking: under Quantified
- replay OneTest: under AllTest
- replay Operand: under Glued
- replay Prefixed: under QuantifiedCore
- replay Primary: under Captured
- replay Quantified: under Operand
- replay QuantifiedCore: under Quantified
- replay Quantifier: under QuantifiedCore
- replay Recovery: under QuantifiedCore
- replay RefOrCall: under Primary
- replay Sequence: under Alternative
- replay TestLeft: under OneTest
- replay TestRight: under OneTest
- replay Type: under ?
- replay Value: under Alternative

## DotGram.Examples.Languages.Lexemes

- machine ParseQuoted [whole]: carrier: tape; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 0/0

## DotGram.Examples.Languages.Scoped

- machine ParseProgram [whole]: carrier: tape; gate: read again; building: 4; replayed: 0; read again: 5; refused: 0; points: 13/13
- again Blank: opens a way
- open Blank: run; what follows begins alike; open; Space+
- again Expr: opens a way
- open Expr: turns, captured; the seam leads every alternative of the turn; open; (trivia & '+' & trivia & right: Expr => (left + right) | trivia & '*' …
- open Expr: choice; the seam leads every alternative; open; (trivia & '+' & trivia & right: Expr => (left + right) | trivia & '*' …
- again Let: through trivia
- again Program: opens a way
- open Program: turns; seam first, what follows begins alike past it; entry; (trivia & Let)*
- again trivia: opens a way
- open trivia: optional; what follows begins alike; open; Blank?

## DotGram.Examples.Languages.Selectors

- machine ParseSelector [whole]: carrier: tape; gate: read again; building: 6; replayed: 0; read again: 1; refused: 0; points: 13/13
- again Selector: opens a way
- open Selector: choice; alternatives begin alike; entry; (s: Applied => (s) | s: Root => (s))

## DotGram.Examples.Languages.SettingsFile

- machine ParseSettings [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 3/3

## DotGram.Examples.Languages.SqlDialect

- machine ParseOld, ParseNew, ParseAny [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 3/3

## DotGram.Examples.Languages.SqlReadOnly

- machine ParseQuery [whole]: carrier: tape; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 0/0

## DotGram.Examples.Languages.TokenizedQuery

- machine ParseQuery [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 12/12

## DotGram.ExpressionLanguage.ExpressionParser

- machine ParseLambda, ParseHole, ParseBody [whole]: carrier: tape; gate: replay; building: 90; replayed: 87; read again: 0; refused: 1; points: 14/453
- machine ParseAsciiLambda, ParseHole_With2, ParseBody_With3 [whole]: carrier: tape; gate: replay; building: 90; replayed: 87; read again: 0; refused: 1; points: 14/453
- refused: 'Primary' gathers two members onto one stack (string); otherwise replay 87
- refused: 'Primary_With1' gathers two members onto one stack (string); otherwise replay 87
- replay Arm: Follows in Binary [turn], then '}'
- replay Arm: Follows in Binary [turn], then '}'
- replay Assignment: Follows in Primary [choice], then '}'
- replay Assignment: Follows in Primary [choice], then '}'
- replay Conditional: Follows in Conditional [turn], then ':'
- replay Conditional: Follows in Conditional [turn], then ':'
- replay Core: Follows in Primary [choice], then '.'
- replay Core: Follows in Primary [choice], then '.'
- replay Discard: Follows in Arm [choice], then "=>"
- replay Discard: Follows in Arm [choice], then "=>"
- replay Elements: Follows in Primary [turn], then '}'
- replay Elements: Follows in Primary [turn], then '}'
- replay Identifier: Lookahead in Constant [choice]
- replay Identifier: Lookahead in Constant [choice]
- replay Indices: Follows in Assignment [choice], then '='
- replay Indices: Follows in Assignment [choice], then '='
- replay Name: Follows in Constant [choice], then ?="=>" or ':' or Identifier
- replay Name: Follows in Constant [choice], then ?="=>" or ':' or Identifier_With1
- replay Parameter: Follows in Function [choice], then ')'
- replay Parameter: Follows in Function [choice], then ')'
- replay Target: Follows in Assignment [choice], then '=' & value: Assignment => (ExpressionParser.AddAssign(target, value, …
- replay Target: Follows in Assignment [choice], then '=' & value: Assignment_With1 => (ExpressionParser.AddAssign(target, v…
- replay Type: Follows in NamedType [turn], then '>'
- replay Type: Follows in NamedType [turn], then '>'
- replay Arguments: under Postfix
- replay Arguments: under Postfix
- replay Awaiting: under Untyped
- replay Awaiting: under Untyped
- replay Bin: under Primary
- replay Binary: under Coalesce
- replay Binary: under Coalesce
- replay Binding: under Bindings
- replay Binding: under Bindings
- replay Bindings: under Primary
- replay Bindings: under Primary
- replay Block: under Body
- replay Block: under Body
- replay Body: under Inner
- replay Body: under Inner
- replay Case: under Switch
- replay Case: under Switch
- replay Catch: under Try
- replay Catch: under Try
- replay Char: under Primary
- replay Coalesce: under Conditional
- replay Coalesce: under Conditional
- replay Constant: under Pattern
- replay Constant: under Pattern
- replay Control: under Body
- replay Control: under Body
- replay Dec: under Primary
- replay Decimals: under Primary
- replay DoWhile: under Control
- replay DoWhile: under Control
- replay Doubles: under Primary
- replay Element: under Elements
- replay Element: under Elements
- replay Fallback: under Switch
- replay Fallback: under Switch
- replay Floats: under Primary
- replay For: under Control
- replay ForLoop: under For
- replay ForLoop: under For
- replay For: under Control
- replay Foreach: under Control
- replay ForeachInferred: under Control
- replay ForeachInferred: under Control
- replay ForeachUnsettled: under Control
- replay ForeachUnsettled: under Control
- replay Foreach: under Control
- replay Guarded: under Postfix
- replay Guarded: under Postfix
- replay Held: under Untyped
- replay HeldBody: under Held
- replay HeldBody: under Held
- replay Held: under Untyped
- replay Hex: under Primary
- replay If: under Control
- replay IfValue: under Body
- replay IfValue: under Body
- replay If: under Control
- replay Inferred: under Statement
- replay InferredUnsettled: under Statement
- replay InferredUnsettled: under Statement
- replay Inferred: under Statement
- replay Inner: under Primary
- replay Inner: under Primary
- replay Interpolated: under Primary
- replay Jump: under Statement
- replay Jump: under Statement
- replay Label: under Case
- replay Label: under Case
- replay Local: under Statement
- replay Local: under Statement
- replay NamedType: under Core
- replay NamedType: under Core
- replay Or: under Pattern
- replay Or: under Pattern
- replay Pattern: under Arm
- replay Pattern: under Arm
- replay Postfix: under Unary
- replay Postfix: under Unary
- replay Primary: under Postfix
- replay Primary: under Postfix
- replay RawByHand: under Primary
- replay RawDoubled3: under Primary
- replay RawDoubled4: under Primary
- replay RawDoubled5: under Primary
- replay RawInterpolated3: under Primary
- replay RawInterpolated4: under Primary
- replay RawInterpolated5: under Primary
- replay RawText3: under Primary
- replay RawText4: under Primary
- replay RawText5: under Primary
- replay Real: under Primary
- replay Return: under Statement
- replay Return: under Statement
- replay SignedLong: under Primary
- replay SignedLong: under Primary
- replay SignedLong: under Primary
- replay Statement: under Block
- replay Statement: under Block
- replay Step: under Guarded
- replay Step: under Guarded
- replay Switch: under Control
- replay Switch: under Control
- replay Text: under Primary
- replay Try: under Control
- replay Try: under Control
- replay Unary: under Binary
- replay Unary: under Binary
- replay UnsignedLong: under Primary
- replay UnsignedLong: under Primary
- replay UnsignedLong: under Primary
- replay Unsigned: under Primary
- replay Unsigned: under Primary
- replay Unsigned: under Primary
- replay Untyped: under Primary
- replay Untyped: under Primary
- replay Verbatim: under Primary
- replay VerbatimInterpolated: under Primary
- replay While: under Control
- replay While: under Control

## DotGram.Finance.Fix.FixGrammar

- machine ParseFields [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 6/6
- machine ParseLogFields [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 6/6
- machine ParseFields [buffered]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 6/6
- machine ParseFields [bytes]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 6/6
- machine ReadFields [buffered]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 6/6
- machine ReadFields [bytes]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 6/6
- machine ParseLogFields [buffered]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 6/6
- machine ParseLogFields [bytes]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 6/6
- machine ReadLogFields [buffered]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 6/6
- machine ReadLogFields [bytes]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 6/6

## DotGram.Sql.Standard.Sql92Parser

- machine ParseSelect, ParseQuery, ParseSearchCondition, ParseValueExpression [whole]: carrier: tape; gate: replay; building: 48; replayed: 44; read again: 0; refused: 0; points: 12/242
- replay ColumnList: Follows in TableReference [turn], then ')'
- replay RowValueConstructorElement: Follows in RowValueConstructor [choice], then ',' …
- replay Subquery: Follows in TablePrimary [choice], then Identifier
- replay ValueExpression: Follows in ValueExpressionPrimary [choice], then ')'
- replay BooleanFactor: under BooleanTerm
- replay BooleanPrimary: under BooleanTest
- replay BooleanTerm: under SearchCondition
- replay BooleanTest: under BooleanFactor
- replay CaseExpression: under ValueExpressionPrimary
- replay CastOperand: under CastSpecification
- replay CastSpecification: under ValueExpressionPrimary
- replay Collate: under ValueExpression
- replay ColumnName: under ColumnList
- replay ColumnReference: under ValueExpressionPrimary
- replay EscapeClause: under NegatablePredicate
- replay FromClause: under QuerySpecification
- replay GeneralValueSpecification: under ValueExpressionPrimary
- replay GroupByClause: under QuerySpecification
- replay GroupingColumn: under GroupByClause
- replay HavingClause: under QuerySpecification
- replay InPredicateValue: under NegatablePredicate
- replay NegatablePredicate: under PredicateTail
- replay Predicate: under BooleanPrimary
- replay PredicateTail: under Predicate
- replay QueryExpression: under Subquery
- replay QueryPrimary: under QueryExpression
- replay QuerySpecification: under QueryPrimary
- replay Reserved: under ?
- replay Result: under CaseExpression
- replay RowValueConstructor: under Predicate
- replay ScalarSubquery: under ValueExpressionPrimary
- replay SearchCondition: under SearchedWhen
- replay SearchedWhen: under CaseExpression
- replay SelectList: under QuerySpecification
- replay SelectSublist: under SelectList
- replay SetFunctionSpecification: under ValueExpressionPrimary
- replay SimpleWhen: under CaseExpression
- replay TablePrimary: under TableReference
- replay TableReference: under FromClause
- replay TableValueConstructor: under QueryPrimary
- replay UnsignedLiteral: under ValueExpressionPrimary
- replay ValueExpressionPrimary: under ValueExpression
- replay ValueFunction: under ValueExpressionPrimary
- replay WhereClause: under QuerySpecification

## DotGram.Sql.Standard.SqlStandardParser

- machine ParseLiteral [whole]: carrier: tape; gate: replay; building: 13; replayed: 12; read again: 0; refused: 0; points: 4/35
- machine ParseTableName [whole]: carrier: tape; gate: replay; building: 4; replayed: 3; read again: 0; refused: 0; points: 2/12
- machine ParseValueExpression, ParseUnsignedLiteral, ParseColumnReference, ParseIdentifierChain, ParseDataType, ParseQueryExpression, ParseQuerySpecification, ParseTableReference, ParseInsertStatement, ParseUpdateStatementSearched, ParseDeleteStatementSearched, ParseMergeStatement, ParseCommonValueExpression, ParseNumericValueExpression, ParseStringValueExpression, ParseCharacterValueExpression, ParseBinaryValueExpression, ParseDatetimeValueExpression, ParseIntervalValueExpression, ParseBooleanValueExpression, ParseSearchCondition, ParsePredicate, ParseRowValuePredicand, ParseValueExpressionPrimary [whole]: carrier: tape; gate: replay; building: 306; replayed: 290; read again: 0; refused: 1; points: 32/1363
- machine ParseDirectSQLStatement, ParseSQLSchemaStatement, ParseUpdateStatementPositioned, ParseDeleteStatementPositioned, ParseTruncateTableStatement, ParseSQLTransactionStatement, ParseSQLConnectionStatement, ParseSQLSessionStatement, ParseSQLDiagnosticsStatement, ParseDirectSQLDataStatement, ParseSQLControlStatement, ParseSQLDataStatement, ParseSQLDynamicStatement, ParseSQLProcedureStatement [whole]: carrier: tape; gate: replay; building: 539; replayed: 308; read again: 0; refused: 1; points: 1248/2605
- refused: 'JSONTablePlanTail' gathers two members onto one stack (JsonTablePlan); otherwise replay 290
- refused: 'JSONTablePlanTail' gathers two members onto one stack (JsonTablePlan); otherwise replay 308
- replay CharacterLargeObjectLength: Follows in CharacterStringType [choice], then ')'
- replay CharacterLength: Follows in CharacterStringType [choice], then ')'
- replay CharacterNode: Follows in JSONNameAndValue [choice], then "VALUE"i
- replay ColumnNameList: Follows in Correlated [choice], then ')'
- replay CommonSequenceGeneratorOptions: Follows in ColumnValueSource [choice], then ')'
- replay ContextuallyTypedElement: Follows in ContextuallyTypedRowValueExpression [choice], then ',' …
- replay ContextuallyTypedTableValueConstructor: Follows in InsertValues [choice], then ?!"UNION"i or "EXCEPT"i or "INTERSECT"i or "ORDER"i or "OFFSET"i or "F…
- replay ContextuallyTypedValueSpecification: Follows in ContextuallyTypedRowValueExpression [choice], then ')'
- replay DataType: Follows in Correlated [choice], then Identifier
- replay FetchOrientation: Follows in FetchStatement [turn], then "FROM"i
- replay GrantedBy: Follows in RevokeStatement [choice], then DropBehavior
- replay Grantees: Follows in RevokeStatement [choice], then DropBehavior
- replay Identifier: Follows in CharacterSetSpecification [turn], then '.'
- replay IdentityGeneration: Follows in ColumnValueSource [choice], then "AS"i
- replay JSONInputExpression: Follows in JSONArrayConstructor [choice], then ')'
- replay JSONOutputClause: Follows in JSONArrayConstructor [choice], then ')'
- replay JSONPathPredicate: Follows in JSONPredicatePrimary [choice], then ')'
- replay Privileges: Follows in GrantStatement [choice], then "TO"i
- replay RowPattern: Follows in RowPatternPrimary [choice], then ')'
- replay SQLStatementName: Follows in DescribeStatement [choice], then UsingDescriptor
- replay SchemaName: Follows in SchemaNameClause [choice], then "AUTHORIZATION"i
- replay SimpleTargetSpecification: Follows in SQLDiagnosticsInformation [choice], then '='
- replay StartField: Follows in IntervalQualifier [choice], then "TO"i
- replay Subquery: Follows in TablePrimary [choice], then CorrelationOrRecognition
- replay ValueNode: Follows in CollectionValueConstructor [choice], then "??)" or ']'
- replay AbsoluteValue: under ValueFunction
- replay ActualIdentifier: under Identifier
- replay AggregateCall: under AggregateFunction
- replay AggregateFunction: under WindowedFunction
- replay AllFields: under SelectSublist
- replay AndOperand: under Conjunction
- replay ArrayElementStep: under FunctionSubscript
- replay ArrayValueExpression: under ValueFunction
- replay AsClause: under AllFields
- replay BasicSequenceGeneratorOption: under CommonSequenceGeneratorOption
- replay BinaryStringType: under PredefinedType
- replay BooleanFactor: under Conjunction
- replay BooleanLiteral: under GeneralLiteral
- replay BooleanPrimary: under BooleanTest
- replay BooleanTest: under BooleanFactor
- replay BooleanValueExpression: under SearchedWhenClause
- replay BracketTail: under Bracketed
- replay Bracketed: under PrimaryReading
- replay CaseExpression: under PrimaryBase
- replay CastSpecification: under PrimaryBase
- replay ChainOrMeasure: under PrimaryBase
- replay CharacterSetSpecification: under PredefinedType
- replay CharacterStringType: under PredefinedType
- replay CharacterValueExpression: under CharacterNode
- replay CollateClause: under PredefinedType
- replay CollectionNode: under TablePrimary
- replay CollectionTypeSuffix: under DataType
- replay CollectionValueConstructor: under PrimaryBase
- replay CollectionValueExpression: under TablePrimary
- replay ColumnReference: under WindowedFunction
- replay CommonSequenceGeneratorOption: under CommonSequenceGeneratorOptions
- replay CommonValueExpression: under DatetimeValueExpression
- replay CommonValueExpressionOrRow: under BooleanPrimary
- replay ComparisonTail: under PredicatePart2
- replay Conjunction: under Disjunction
- replay ContextuallyTypedRowValueExpression: under ContextuallyTypedTableValueConstructor
- replay Correlated: under TablePrimary
- replay CorrelationOrRecognition: under TablePrimary
- replay CorrespondingSpec: under Intersected
- replay CycleClause: under SearchOrCycleClause
- replay DataChangeDeltaTable: under TablePrimary
- replay DataChangeStatement: under DataChangeDeltaTable
- replay DataTypeBase: under DataType
- replay DateLiteral: under GeneralLiteral
- replay DatetimeType: under PredefinedType
- replay DatetimeValueExpression: under ForPortionOf
- replay DatetimeValueFunction: under ValueFunction
- replay DeleteStatementSearched: under DataChangeStatement
- replay Disjunction: under ValueExpression
- replay ElseClause: under CaseExpression
- replay EndField: under IntervalQualifier
- replay ExistingWindowName: under WindowSpecificationDetails
- replay ExtractExpression: under NumericValueFunction
- replay FetchFirstClause: under QueryExpression
- replay FetchFirstQuantity: under FetchFirstClause
- replay FieldDefinition: under RowType
- replay FilterClause: under AggregateFunction
- replay ForPortionOf: under DeleteStatementSearched
- replay FromClause: under TableExpression
- replay FunctionSubscript: under Primary
- replay GeneralLiteral: under UnsignedLiteral
- replay GeneralValueSpecification: under PrimaryBase
- replay GranteeItem: under Grantees
- replay Grantor: under GrantedBy
- replay GreatestOrLeastFunction: under PrimaryBase
- replay GroupByClause: under TableExpression
- replay GroupingColumnReference: under OrdinaryGroupingSet
- replay GroupingElement: under GroupByClause
- replay HavingClause: under TableExpression
- replay IdentifierChain: under PeriodPredicand
- replay ImplicitlyTypedValueSpecification: under ContextuallyTypedValueSpecification
- replay InsertColumnsAndSource: under InsertStatement
- replay InsertStatement: under DataChangeStatement
- replay InsertValues: under InsertColumnsAndSource
- replay Intersected: under QueryTerm
- replay IntervalLiteral: under GeneralLiteral
- replay IntervalPrimary: under TimeZoneSpecifier
- replay IntervalQualifier: under IntervalLiteral
- replay IntroducedStringLiteral: under GeneralLiteral
- replay IsPredicatePart2: under PredicatePart2
- replay JSONAPICommonSyntax: under JSONValueFunction
- replay JSONAggregateFunction: under AggregateCall
- replay JSONArgument: under JSONAPICommonSyntax
- replay JSONArrayConstructor: under PrimaryBase
- replay JSONColumnBehavior: under JSONTableTypedColumn
- replay JSONColumnFormat: under JSONTableTypedColumn
- replay JSONColumnQuotes: under JSONColumnWrapper
- replay JSONColumnWrapper: under JSONTableTypedColumn
- replay JSONExistsPredicate: under BooleanPrimary
- replay JSONInputClause: under JSONInputExpression
- replay JSONMethod: under PrimaryStep
- replay JSONNameAndValue: under JSONAggregateFunction
- replay JSONObjectConstructor: under PrimaryBase
- replay JSONPathAccessor: under JSONPathUnary
- replay JSONPathAccessorOp: under JSONPathAccessor
- replay JSONPathAdded: under JSONPathWff
- replay JSONPathMultiplicative: under JSONPathWff
- replay JSONPathMultiplied: under JSONPathMultiplicative
- replay JSONPathPrimary: under JSONPathAccessor
- replay JSONPathUnary: under JSONPathMultiplicative
- replay JSONPathWff: under JSONSubscript
- replay JSONPredicatePrimary: under JSONPathPredicate
- replay JSONQuery: under PrimaryBase
- replay JSONQueryQuotes: under JSONQuery
- replay JSONRepresentation: under JSONOutputClause
- replay JSONSerialize: under StringValueFunction
- replay JSONSubscript: under PrimaryStep
- replay JSONTable: under TablePrimary
- replay JSONTableColumnDefinition: under JSONTableColumnsClause
- replay JSONTableColumnsClause: under JSONTable
- replay JSONTableDefaultPlanChoices: under JSONTablePlanClause
- replay JSONTablePlan: under JSONTablePlanClause
- replay JSONTablePlanClause: under JSONTable
- replay JSONTablePlanPrimary: under JSONTablePlan
- replay JSONTablePlanTail: under JSONTablePlan
- replay JSONTablePrimitive: under TablePrimary
- replay JSONTablePrimitiveColumn: under JSONTablePrimitive
- replay JSONTableTypedColumn: under JSONTableColumnDefinition
- replay JSONTypedValueFunction: under ValueFunction
- replay JSONValueBehavior: under JSONValueFunction
- replay JSONValueExpression: under JSONSerialize
- replay JSONValueFunction: under PrimaryBase
- replay JoinOperand: under PartitionedJoin
- replay JoinOperandTail: under JoinOperand
- replay JoinPartitioning: under Joins
- replay JoinSpecification: under PartitionedJoin
- replay JoinStep: under Joins
- replay Joins: under TableReference
- replay LargeObjectLength: under CharacterLargeObjectLength
- replay LikeEscape: under NegatablePredicatePart2
- replay ListaggOverflowClause: under AggregateCall
- replay LocalOrSchemaQualifiedName: under SimpleTable
- replay MergeCondition: under MergeWhenClause
- replay MergeInsertSpecification: under MergeWhenClause
- replay MergeMatchedThen: under MergeWhenClause
- replay MergeStatement: under DataChangeStatement
- replay MergeWhenClause: under MergeStatement
- replay MultisetElementReference: under PrimaryBase
- replay MultisetValueExpression: under ValueFunction
- replay NationalCharacterStringType: under PredefinedType
- replay NegatablePredicatePart2: under PredicatePart2
- replay NewSpecification: under PrimaryBase
- replay NextValueExpression: under PrimaryBase
- replay NumericNode: under NumericValueFunction
- replay NumericType: under PredefinedType
- replay NumericValueExpression: under ValueFunction
- replay NumericValueFunction: under ValueFunction
- replay ObjectName: under Privileges
- replay Operand: under CommonValueExpressionOrRow
- replay Operated: under CommonValueExpressionOrRow
- replay Operator: under Operated
- replay OrOperand: under Disjunction
- replay OrderByClause: under QueryExpression
- replay OrdinaryGroupingSet: under GroupingElement
- replay OverflowBehavior: under ListaggOverflowClause
- replay OverlayFunction: under StringValueFunction
- replay OverrideClause: under InsertColumnsAndSource
- replay ParenthesizedJoinedTable: under TablePrimary
- replay PartitionColumn: under WindowPartitionClause
- replay PartitionedJoin: under Joins
- replay PathResolvedUserDefinedTypeName: under DataTypeBase
- replay PeriodConstructor: under BooleanPrimary
- replay PeriodContained: under PeriodPredicatePart2
- replay PeriodPredicand: under PeriodPredicatePart2
- replay PeriodPredicatePart2: under BooleanPrimary
- replay PositionExpression: under NumericValueFunction
- replay Postfix: under Operand
- replay PredefinedType: under DataTypeBase
- replay PredicatePart2: under WhenOperand
- replay PredicatePart2Mark: under BooleanPrimary
- replay Primary: under Operand
- replay PrimaryBase: under PrimaryReading
- replay PrimaryReading: under Primary
- replay PrimaryStep: under FunctionSubscript
- replay PrimarySteps: under PrimaryReading
- replay PrivilegeAction: under PrivilegeActions
- replay PrivilegeActions: under Privileges
- replay QueryExpression: under Subquery
- replay QueryExpressionBody: under QueryExpression
- replay QueryPrimary: under QueryTerm
- replay QuerySpecification: under SimpleTable
- replay QuerySystemTimePeriodSpecification: under TablePrimary
- replay QueryTerm: under QueryExpressionBody
- replay Recognized: under TablePrimary
- replay ReferenceResolution: under PrimaryBase
- replay ReferenceType: under DataTypeBase
- replay RegexSearch: under NumericValueFunction
- replay Result: under SimpleWhenClause
- replay ResultOffsetClause: under QueryExpression
- replay RoutineInvocation: under PrimaryBase
- replay RoutineName: under RoutineInvocation
- replay RoutineType: under SpecificRoutineDesignator
- replay RowPatternCommonSyntax: under WindowFrameClause
- replay RowPatternDefinition: under RowPatternCommonSyntax
- replay RowPatternFactor: under RowPatternTerm
- replay RowPatternMeasure: under RowPatternMeasures
- replay RowPatternMeasures: under WindowFrameClause
- replay RowPatternNavigationOperation: under PrimaryBase
- replay RowPatternPartitionBy: under RowPatternRecognitionClause
- replay RowPatternPrimary: under RowPatternFactor
- replay RowPatternQuantifier: under RowPatternFactor
- replay RowPatternRecognitionClause: under Recognized
- replay RowPatternRowsPerMatch: under RowPatternRecognitionClause
- replay RowPatternSkipTo: under RowPatternCommonSyntax
- replay RowPatternSubsetClause: under RowPatternCommonSyntax
- replay RowPatternSubsetItem: under RowPatternSubsetClause
- replay RowPatternTerm: under RowPattern
- replay RowType: under DataTypeBase
- replay RowValueExpression: under NegatablePredicatePart2
- replay RowValuePredicand: under CaseExpression
- replay SQLArgument: under SQLArgumentList
- replay SQLArgumentList: under PrimaryStep
- replay SampleClause: under TableFactor
- replay SchemaQualifiedName: under StringValueFunction
- replay SearchClause: under SearchOrCycleClause
- replay SearchOrCycleClause: under WithListElement
- replay SearchedWhenClause: under CaseExpression
- replay SelectList: under QuerySpecification
- replay SelectSublist: under SelectList
- replay SetClause: under SetClauseList
- replay SetClauseList: under MergeMatchedThen
- replay SetTarget: under SetClause
- replay SetTargetTail: under SetTarget
- replay SignedNumericLiteral: under SimpleValueSpecification
- replay SimpleOrDynamicValue: under WindowedFunction
- replay SimpleTable: under QueryPrimary
- replay SimpleValueSpecification: under SimpleOrDynamicValue
- replay SimpleWhenClause: under CaseExpression
- replay SingleDatetimeField: under IntervalQualifier
- replay SortSpecification: under SortSpecificationList
- replay SortSpecificationList: under AggregateCall
- replay SpecificRoutineDesignator: under PrivilegeAction
- replay StaticMethodInvocation: under PrimaryBase
- replay StringValueExpression: under NumericValueFunction
- replay StringValueFunction: under ValueFunction
- replay SubstringFunction: under StringValueFunction
- replay SubstringTail: under SubstringFunction
- replay SubtypeTreatment: under PrimaryBase
- replay TableExpression: under QuerySpecification
- replay TableFactor: under TableReference
- replay TablePrimary: under TableFactor
- replay TableReference: under FromClause
- replay TableValueConstructor: under SimpleTable
- replay TargetCorrelation: under DeleteStatementSearched
- replay TargetSubtype: under SubtypeTreatment
- replay TargetTable: under DeleteStatementSearched
- replay TimeLiteral: under GeneralLiteral
- replay TimeZone: under Postfix
- replay TimeZoneSpecifier: under TimeZone
- replay TimestampLiteral: under GeneralLiteral
- replay TrimAfterSpecification: under TrimOperands
- replay TrimFrom: under TrimOperands
- replay TrimFunction: under StringValueFunction
- replay TrimOperands: under TrimFunction
- replay TruthTest: under BooleanTest
- replay UnionOrExcept: under QueryExpressionBody
- replay UnsignedLiteral: under PrimaryBase
- replay UnsignedValueSpecification: under WindowFrameBound
- replay UpdateStatementSearched: under DataChangeStatement
- replay UserDefinedTypeSpecification: under IsPredicatePart2
- replay UsingUnits: under PositionExpression
- replay ValueExpression: under ValueNode
- replay ValueExpressionPrimary: under ReferenceResolution
- replay ValueFunction: under Primary
- replay WhenOperand: under SimpleWhenClause
- replay WhereClause: under TableExpression
- replay WindowClause: under TableExpression
- replay WindowDefinition: under WindowClause
- replay WindowFrameBound: under WindowFrameExtent
- replay WindowFrameClause: under WindowSpecificationDetails
- replay WindowFrameExclusion: under WindowFrameClause
- replay WindowFrameExtent: under WindowFrameClause
- replay WindowFrameStart: under WindowFrameExtent
- replay WindowNameOrSpecification: under WindowOver
- replay WindowOrderClause: under WindowSpecificationDetails
- replay WindowOver: under WindowedFunction
- replay WindowPartitionClause: under WindowSpecificationDetails
- replay WindowSpecification: under WindowNameOrSpecification
- replay WindowSpecificationDetails: under WindowSpecification
- replay WindowedFunction: under PrimaryBase
- replay WithClause: under QueryExpression
- replay WithListElement: under WithClause
- replay WithinGroupSpecification: under AggregateCall

## DotGram.Sql.TransactSql.TransactSqlParser

- machine ParseSelect, ParseQuery, ParseSearchCondition, ParseValueExpression [whole]: carrier: tape; gate: replay; building: 179; replayed: 174; read again: 0; refused: 0; points: 37/812
- machine ParseStatement, ParseStatement100, ParseStatement110, ParseStatement120, ParseStatement130, ParseStatement140, ParseStatement150, ParseStatement160, ParseStatement170, ParseSql, ParseSql100, ParseSql110, ParseSql120, ParseSql130, ParseSql140, ParseSql150, ParseSql160, ParseSql170, ParseScript, ParseScript100, ParseScript110, ParseScript120, ParseScript130, ParseScript140, ParseScript150, ParseScript160, ParseScript170 [whole]: carrier: tape; gate: replay; building: 655; replayed: 322; read again: 0; refused: 1; points: 2354/3141
- refused: 'SetExpressions_Dialect' gathers two members onto one stack (string); otherwise replay 322
- replay AlterColumnWord: Follows in AlterTableAction [choice], then when (Syntax.FlagsOnline(flag, options))
- replay Arguments: Follows in Member [turn], then ')'
- replay AssemblyOptions: Follows in CodeStatement [choice], then when (Syntax.Tail(tail) is not null)
- replay AssignOp: Follows in TSqlSelectSublist [choice], then TSqlValueExpression
- replay BackupRedundancy: Follows in DatabaseTail [turn], then ')'
- replay BindingOptions: Follows in BrokerStatement [choice], then when (Syntax.NamedOnce(null, options) && Syntax.HasOption(options, "US…
- replay BrokerString: Follows in AvailabilityMade [turn], then '('
- replay ColumnDefinition: Follows in AlterTableAction [choice], then when (Syntax.AlteredColumn(column))
- replay ColumnKeySetting: Follows in KeyStatement [choice], then ')'
- replay ColumnList: Follows in VariableSource [turn], then ')'
- replay ColumnOnline: Follows in AlterTableAction [choice], then when (Syntax.AlteredColumn(column))
- replay ConstraintBody: Follows in AlterTableAction [choice], then "FOR"i
- replay CreateOrAlter: Follows in KeyStatement [choice], then 'Ĝ' & name: TSqlIdentifier & tail: CredentialWith & ('2' & '̲' & '̳' &…
- replay CursorFor: Follows in CursorQuery [choice], then QueryHints
- replay CursorSelect: Follows in CursorQuery [choice], then CursorFor
- replay DataAlias: Follows in RowsetArgument [choice], then when (alias is null || string.Equals(name, "DATA", System.StringCompar…
- replay DataSourceBody: Follows in ExternalStatement [choice], then when (Syntax.NamedOnce(null, options))
- replay DatabaseTarget: Follows in AlterDatabaseStatement [choice], then "SET"i
- replay DatePart: Follows in DatePart [choice], then ')'
- replay DbccWait: Follows in DbccOption [choice], then ')'
- replay DbccWord: Follows in DbccArgument [choice], then '='
- replay DeclaredBody: Follows in CreateTableStatement [choice], then ')'
- replay DmlTarget: Follows in TSqlInsert [choice], then InsertRows
- replay EventBody: Follows in EventAdd [turn], then ')'
- replay EventObject: Follows in EventTerm [choice], then ','
- replay EventSet: Follows in TargetAdd [turn], then ')'
- replay ExecValue: Follows in ExecArgument [choice], then when (Syntax.Passes(v, back))
- replay ExternalTableWith: Follows in ExternalStatement [choice], then "AS"i
- replay FetchRow: Follows in CursorStatement [choice], then "FROM"i
- replay FullTextAll: Follows in FullTextColumns [choice], then ')'
- replay GroupingSetItem: Follows in GroupingSet [choice], then ')'
- replay GroupingSet: Follows in TSqlGrouping [choice], then ')'
- replay IncludeColumns: Follows in TableIndex [turn], then ')'
- replay IndexOrder: Follows in CreateIndexStatement [choice], then "INDEX"i
- replay InsertColumns: Follows in TSqlInsert [choice], then InsertRows
- replay InsertRows: Follows in TSqlInsert [choice], then when (options is null || rows is not Query.FromExecute)
- replay JoinHint: Follows in TSqlTableReference [turn], then "JOIN"i
- replay JoinedRight: Follows in TSqlTableReference [turn], then "ON"i
- replay LanguageFile: Follows in CodeStatement [choice], then when (Syntax.LanguageFiles(file, other))
- replay MasterKeySetting: Follows in KeyStatement [choice], then ')'
- replay MemberName: Follows in SetStatement [choice], then MemberTail
- replay MemberTail: Follows in SetStatement [choice], then when (tail is not { Operator: "" } || members is { Length: 1 })
- replay ModelOptions: Follows in CodeStatement [choice], then when (Syntax.NamedOnce(null, options))
- replay Nulls: Follows in CallTail [choice], then Over
- replay OdbcLiteralKind: Follows in OdbcEscape [choice], then NationalCharacterStringLiteral
- replay OdbcType: Follows in OdbcFunction [choice], then ')'
- replay OnOff: Follows in TypeStatement [choice], then ')'
- replay OptionList: Follows in SwitchTail [choice], then ')'
- replay OptionsWith: Follows in CreateTableStatement [choice], then "AS"i
- replay OrderByClause: Follows in WithinGroup [choice], then ?reading(0x1FDFDFD)
- replay OutputClause: Follows in TSqlInsert [choice], then InsertRows
- replay OutputList: Follows in OutputClause [choice], then "INTO"i
- replay PlacementTarget: Lookahead in SwitchTail [lookahead]
- replay PoolName: Follows in ExternalStatement [choice], then ExternalPoolWith
- replay PredictCall: Follows in TSqlTablePrimary [choice], then PredictSchema
- replay PriorityOptions: Follows in BrokerStatement [turn], then ')'
- replay QueryHints: Follows in TSqlInsert [choice], then when (options is null || rows is not Query.FromExecute)
- replay QueueWith: Follows in BrokerStatement [choice], then when (Syntax.NamedOnce(null, options) && Syntax.Activates(options))
- replay RoleWord: Follows in PrincipalStatement [choice], then TSqlIdentifier
- replay RouteOptions: Follows in BrokerStatement [choice], then when (Syntax.NamedOnce(null, options) && Syntax.HasOption(options, "AD…
- replay RowValueConstructorElement: Follows in RowValueConstructor [choice], then ',' …
- replay RowValueConstructor: Follows in TSqlPredicate [choice], then PredicateTail
- replay RowsetArgument: Follows in RowsetArgument [choice], then ')'
- replay RowsetFunction: Follows in TSqlTablePrimary [choice], then when (Syntax.Schemas(f, schema))
- replay RowsetSchema: Follows in TSqlTablePrimary [choice], then when (Syntax.Schemas(f, schema))
- replay RowsetValue: Follows in RowsetArgument [choice], then ',' …
- replay SearchCondition: Follows in MergeArm [choice], then "THEN"i
- replay ServerOrDatabase: Follows in AuditStatement [choice], then "AUDIT"i
- replay SetValue: Follows in CodeStatement [choice], then when (Syntax.Tail(tail) is not null)
- replay SpatialSetting: Follows in SpatialSet [choice], then ')'
- replay TSqlAlias: Follows in TSqlSelectSublist [choice], then '='
- replay TSqlJoinType: Follows in TSqlTableReference [turn], then "JOIN"i
- replay TSqlSubquery: Follows in TSqlTablePrimary [choice], then CorrelationName
- replay TSqlValueExpression: Follows in TSqlPrimaryCore [choice], then ')'
- replay TableAs: Follows in CreateTableStatement [choice], then '('
- replay TableBody: Follows in ExternalStatement [choice], then ')'
- replay TextColumn: Follows in TextStatement [turn], then TextPointer
- replay Top: Follows in TSqlInsert [choice], then DmlTarget
- replay WithClause: Follows in InlineReturn [choice], then TSqlQueryExpression
- replay WithinGroup: Follows in CallTail [choice], then Over
- replay Activation: under QueueOption
- replay AdHocObject: under TSqlTablePrimary
- replay AdHocServer: under TSqlTablePrimary
- replay AffinityValue: under OptionSetting
- replay Argument: under Arguments
- replay AssemblyOption: under AssemblyOptions
- replay AssignTail: under MemberTail
- replay AssignedValue: under VariableChainTail
- replay Assignment: under Assignments
- replay Assignments: under TSqlUpdate
- replay AtTimeZone: under TSqlValuePrimary
- replay BareKeyOption: under BareKeyOptions
- replay BareKeyOptions: under ConstraintWith
- replay BindingOption: under BindingOptions
- replay BooleanPrimary: under BooleanTest
- replay BooleanTerm: under SearchCondition
- replay BooleanTest: under TSqlBooleanFactor
- replay CallTail: under TSqlPrimaryCore
- replay CaseExpression: under TSqlPrimaryCore
- replay CastOperand: under TSqlCast
- replay ChainTail: under Assignment
- replay ChangeTrackingContext: under WithClause
- replay ChangedSource: under InsertRows
- replay ChangedStatement: under ChangedTable
- replay ChangedTable: under ChangedSource
- replay ChunksFunction: under TSqlTablePrimary
- replay ChunksOverlap: under ChunksFunction
- replay ChunksSet: under ChunksFunction
- replay ChunksSize: under ChunksFunction
- replay ChunksSource: under ChunksFunction
- replay ChunksType: under ChunksFunction
- replay ChunksValue: under ChunksSource
- replay Collate: under TSqlValueExpression
- replay ColumnBody: under ColumnDefinition
- replay ColumnName: under ColumnList
- replay ColumnReference: under TSqlPrimaryCore
- replay ColumnTail: under TableColumnBody
- replay ColumnTrait: under ColumnTail
- replay ConstraintOption: under ConstraintWith
- replay ConstraintWith: under ConstraintBody
- replay CountStar: under TSqlPrimaryCore
- replay CteBody: under CteDefinition
- replay CteDefinition: under WithClause
- replay CursorColumn: under CursorColumns
- replay CursorColumns: under CursorFor
- replay DataSourceChange: under DataSourceBody
- replay DataSourceItem: under DataSourceBody
- replay DataSourceOption: under DataSourceItem
- replay DatePartCall: under TSqlPrimaryCore
- replay DatePartFunction: under DatePartCall
- replay DbccValue: under ExecValue
- replay DistinctTail: under TSqlPredicate
- replay DmlCall: under TSqlInsert
- replay DmlWhere: under TSqlDelete
- replay DottedType: under ?
- replay EdgePair: under ConstraintBody
- replay EncryptionOption: under ColumnTrait
- replay Enforced: under ConstraintBody
- replay EventActions: under EventBody
- replay EventPredicate: under EventBody
- replay EventSetting: under EventSet
- replay ExactNumber: under ?
- replay ExecArgument: under ExecArguments
- replay ExecArguments: under ExecuteBody
- replay ExecAt: under ExecuteBody
- replay ExecContext: under ExecuteBody
- replay ExecContextKind: under ExecContext
- replay ExecContextName: under ExecContext
- replay ExecDataSource: under ExecuteBody
- replay ExecName: under ExecTarget
- replay ExecNumber: under ExecTarget
- replay ExecReturn: under ExecuteBody
- replay ExecTarget: under ExecuteBody
- replay ExecuteBody: under ExecuteStatement
- replay ExecuteStatement: under InsertRows
- replay ExternalTableItem: under ExternalTableWith
- replay ExternalTableOption: under ExternalTableItem
- replay FetchClause: under OffsetFetch
- replay ForClause: under TSqlSubquery
- replay FromClause: under TSqlQuerySpecification
- replay FullTextColumns: under FullTextPredicate
- replay FullTextPredicate: under TSqlPredicate
- replay FullTextSearch: under RowsetFunction
- replay FunctionCall: under TSqlPrimaryCore
- replay GraphMatch: under TSqlPredicate
- replay GroupByExpression: under GroupingSet
- replay GroupByItem: under TSqlGrouping
- replay GroupList: under GroupingSet
- replay GroupWith: under TSqlGroupByClause
- replay HavingClause: under TSqlQuerySpecification
- replay IdentityFunction: under TSqlSelectSublist
- replay IdentityNumber: under IdentityFunction
- replay InPredicateValue: under NegatablePredicate
- replay IndexColumn: under IndexColumns
- replay IndexColumns: under ConstraintBody
- replay IndexName: under IncludeColumns
- replay IndexOn: under ConstraintBody
- replay InsertColumn: under InsertColumns
- replay Into: under TSqlQuerySpecification
- replay JoinedTail: under JoinedRight
- replay JsonArrayBody: under TSqlValueFunction
- replay JsonDirective: under ForClause
- replay JsonFunction: under TSqlTablePrimary
- replay JsonKeyValue: under JsonKeyValues
- replay JsonKeyValues: under JsonObjectBody
- replay JsonMode: under ForClause
- replay JsonNullClause: under TSqlValueFunction
- replay JsonObjectBody: under TSqlValueFunction
- replay JsonOrder: under TSqlValueFunction
- replay JsonReturningJson: under TSqlValueFunction
- replay JsonSchemaColumn: under JsonSchema
- replay JsonSchema: under TSqlTablePrimary
- replay JsonValueReturning: under TSqlValueFunction
- replay JsonValue: under JsonKeyValue
- replay JsonWrapper: under TSqlValueFunction
- replay LanguageOption: under LanguageFile
- replay LanguagePlatform: under LanguageOption
- replay LeftRightCall: under TSqlPrimaryCore
- replay LevelOrDefault: under PriorityOption
- replay LocalVariable: under FetchRow
- replay MaskOption: under ColumnTrait
- replay Member: under TSqlValuePrimary
- replay MergeArm: under TSqlMerge
- replay MergeChange: under MergeArm
- replay MergeColumn: under MergeColumns
- replay MergeColumns: under MergeInsert
- replay MergeInsertRows: under MergeInsert
- replay MergeInsert: under MergeArm
- replay MethodCallTail: under Assignment
- replay ModelOption: under ModelOptions
- replay NameOrAny: under PriorityOption
- replay NamedConstraint: under ColumnTrait
- replay NegatablePredicate: under PredicateTail
- replay NextValue: under TSqlPrimaryCore
- replay NullSpec: under PredictColumn
- replay OdbcEscape: under TSqlPrimaryCore
- replay OdbcFunction: under OdbcEscape
- replay OffsetFetch: under TSqlSubquery
- replay OnPartitions: under OptionSetting
- replay OpenQueryCall: under RowsetFunction
- replay OptionNest: under OptionTail
- replay OptionSetting: under OptionList
- replay OptionTail: under OptionSetting
- replay OptionUnit: under OptionValue
- replay OptionValue: under OptionSetting
- replay OutputItem: under OutputList
- replay Over: under CallTail
- replay PivotName: under PivotNames
- replay PivotNames: under Pivot
- replay PivotSuffix: under TSqlTableReference
- replay Pivot: under PivotSuffix
- replay PredicateTail: under TSqlPredicate
- replay PredictColumn: under PredictSchema
- replay PredictSchema: under TSqlTablePrimary
- replay PriorityOption: under PriorityOptions
- replay QueryHint: under QueryHints
- replay QueryPrimary: under TSqlQueryExpression
- replay QueueOption: under QueueWith
- replay ReferenceAction: under ConstraintBody
- replay ReferenceOn: under References
- replay References: under ConstraintBody
- replay ResultColumns: under ?
- replay Result: under CaseExpression
- replay RouteOption: under RouteOptions
- replay RowsetArguments: under PredictCall
- replay RowsetOrder: under RowsetArgument
- replay SampleUnit: under TableSample
- replay SampledHint: under TSqlTablePrimary
- replay ScalarSubquery: under TSqlPrimaryCore
- replay SchemaCollate: under JsonSchemaColumn
- replay SchemaColumn: under RowsetSchema
- replay SchemaOrdinal: under SchemaColumn
- replay SchemaPath: under JsonSchemaColumn
- replay SearchLanguage: under RowsetFunction
- replay SearchTop: under RowsetFunction
- replay SearchedColumn: under FullTextColumns
- replay SearchedWhen: under CaseExpression
- replay SemanticArgument: under SemanticArguments
- replay SemanticArguments: under RowsetFunction
- replay SetFunctionSpecification: under TSqlPrimaryCore
- replay SortSpecification: under OrderByClause
- replay SortedData: under BareKeyOption
- replay SourceHints: under TSqlTablePrimary
- replay SpatialItem: under SpatialSetting
- replay StringOrAny: under PriorityOption
- replay SwitchTail: under OptionTail
- replay SystemTimeWhen: under SystemTime
- replay SystemTime: under TSqlTablePrimary
- replay TSqlAsClause: under TSqlSelectSublist
- replay TSqlBooleanFactor: under BooleanTerm
- replay TSqlCast: under TSqlPrimaryCore
- replay TSqlDelete: under ChangedStatement
- replay TSqlEscapeClause: under NegatablePredicate
- replay TSqlGroupByClause: under TSqlQuerySpecification
- replay TSqlGroupingColumn: under GroupByExpression
- replay TSqlGrouping: under TSqlGroupByClause
- replay TSqlInsert: under ChangedStatement
- replay TSqlMerge: under ChangedStatement
- replay TSqlPredicate: under BooleanPrimary
- replay TSqlPrimaryCore: under TSqlValuePrimary
- replay TSqlQueryExpression: under TSqlSubquery
- replay TSqlQuerySpecification: under QueryPrimary
- replay TSqlRow: under TSqlTableValueConstructor
- replay TSqlSelectList: under TSqlQuerySpecification
- replay TSqlSelectSublist: under TSqlSelectList
- replay TSqlSimpleWhen: under CaseExpression
- replay TSqlTablePrimary: under JoinedRight
- replay TSqlTableReference: under FromClause
- replay TSqlTableValueConstructor: under QueryPrimary
- replay TSqlUpdate: under ChangedStatement
- replay TSqlValueFunction: under TSqlPrimaryCore
- replay TSqlValuePrimary: under TSqlValueExpression
- replay TSqlValueSpecification: under TSqlPrimaryCore
- replay TableColumnBody: under TableColumn
- replay TableColumn: under TableElement
- replay TableElement: under TableBody
- replay TableHint: under SampledHint
- replay TableHints: under SourceHints
- replay TableIndexOption: under TableIndexWith
- replay TableIndexWith: under TableIndex
- replay TableIndex: under TableElement
- replay TableRepeatable: under TableSample
- replay TableSample: under TSqlTablePrimary
- replay TableSearchColumn: under TableSearchColumns
- replay TableSearchColumns: under RowsetFunction
- replay TimeZone: under AtTimeZone
- replay TopSuffix: under Top
- replay Unpivot: under PivotSuffix
- replay UnsignedLiteral: under TSqlPrimaryCore
- replay UpdateVariableTail: under Assignment
- replay UseModel: under FunctionCall
- replay ValueFunction: under TSqlPrimaryCore
- replay VariableChainTail: under UpdateVariableTail
- replay VariableSource: under TSqlTablePrimary
- replay WhereClause: under TSqlQuerySpecification
- replay WindowDef: under Window
- replay WindowFrame: under WindowSpecification
- replay WindowSpecification: under WindowDef
- replay Window: under TSqlQuerySpecification
- replay XmlDirective: under ForClause
- replay XmlMode: under ForClause
- replay XmlNamespaces: under WithClause

## DotGram.Sql.TransactSql.TransactSqlParser.Located

- machine ParseSelect, ParseQuery, ParseSearchCondition, ParseValueExpression [whole]: carrier: tape; gate: replay; building: 179; replayed: 174; read again: 0; refused: 0; points: 37/812
- machine ParseStatement, ParseStatement100, ParseStatement110, ParseStatement120, ParseStatement130, ParseStatement140, ParseStatement150, ParseStatement160, ParseStatement170, ParseSql, ParseSql100, ParseSql110, ParseSql120, ParseSql130, ParseSql140, ParseSql150, ParseSql160, ParseSql170, ParseScript, ParseScript100, ParseScript110, ParseScript120, ParseScript130, ParseScript140, ParseScript150, ParseScript160, ParseScript170 [whole]: carrier: tape; gate: replay; building: 655; replayed: 322; read again: 0; refused: 1; points: 2354/3141
- refused: 'SetExpressions_Dialect' gathers two members onto one stack (string); otherwise replay 322
- replay AlterColumnWord: Follows in AlterTableAction [choice], then when (Syntax.FlagsOnline(flag, options))
- replay Arguments: Follows in Member [turn], then ')'
- replay AssemblyOptions: Follows in CodeStatement [choice], then when (Syntax.Tail(tail) is not null)
- replay AssignOp: Follows in TSqlSelectSublist [choice], then TSqlValueExpression
- replay BackupRedundancy: Follows in DatabaseTail [turn], then ')'
- replay BindingOptions: Follows in BrokerStatement [choice], then when (Syntax.NamedOnce(null, options) && Syntax.HasOption(options, "US…
- replay BrokerString: Follows in AvailabilityMade [turn], then '('
- replay ColumnDefinition: Follows in AlterTableAction [choice], then when (Syntax.AlteredColumn(column))
- replay ColumnKeySetting: Follows in KeyStatement [choice], then ')'
- replay ColumnList: Follows in VariableSource [turn], then ')'
- replay ColumnOnline: Follows in AlterTableAction [choice], then when (Syntax.AlteredColumn(column))
- replay ConstraintBody: Follows in AlterTableAction [choice], then "FOR"i
- replay CreateOrAlter: Follows in KeyStatement [choice], then 'Ĝ' & name: TSqlIdentifier & tail: CredentialWith & ('2' & '̲' & '̳' &…
- replay CursorFor: Follows in CursorQuery [choice], then QueryHints
- replay CursorSelect: Follows in CursorQuery [choice], then CursorFor
- replay DataAlias: Follows in RowsetArgument [choice], then when (alias is null || string.Equals(name, "DATA", System.StringCompar…
- replay DataSourceBody: Follows in ExternalStatement [choice], then when (Syntax.NamedOnce(null, options))
- replay DatabaseTarget: Follows in AlterDatabaseStatement [choice], then "SET"i
- replay DatePart: Follows in DatePart [choice], then ')'
- replay DbccWait: Follows in DbccOption [choice], then ')'
- replay DbccWord: Follows in DbccArgument [choice], then '='
- replay DeclaredBody: Follows in CreateTableStatement [choice], then ')'
- replay DmlTarget: Follows in TSqlInsert [choice], then InsertRows
- replay EventBody: Follows in EventAdd [turn], then ')'
- replay EventObject: Follows in EventTerm [choice], then ','
- replay EventSet: Follows in TargetAdd [turn], then ')'
- replay ExecValue: Follows in ExecArgument [choice], then when (Syntax.Passes(v, back))
- replay ExternalTableWith: Follows in ExternalStatement [choice], then "AS"i
- replay FetchRow: Follows in CursorStatement [choice], then "FROM"i
- replay FullTextAll: Follows in FullTextColumns [choice], then ')'
- replay GroupingSetItem: Follows in GroupingSet [choice], then ')'
- replay GroupingSet: Follows in TSqlGrouping [choice], then ')'
- replay IncludeColumns: Follows in TableIndex [turn], then ')'
- replay IndexOrder: Follows in CreateIndexStatement [choice], then "INDEX"i
- replay InsertColumns: Follows in TSqlInsert [choice], then InsertRows
- replay InsertRows: Follows in TSqlInsert [choice], then when (options is null || rows is not Query.FromExecute)
- replay JoinHint: Follows in TSqlTableReference [turn], then "JOIN"i
- replay JoinedRight: Follows in TSqlTableReference [turn], then "ON"i
- replay LanguageFile: Follows in CodeStatement [choice], then when (Syntax.LanguageFiles(file, other))
- replay MasterKeySetting: Follows in KeyStatement [choice], then ')'
- replay MemberName: Follows in SetStatement [choice], then MemberTail
- replay MemberTail: Follows in SetStatement [choice], then when (tail is not { Operator: "" } || members is { Length: 1 })
- replay ModelOptions: Follows in CodeStatement [choice], then when (Syntax.NamedOnce(null, options))
- replay Nulls: Follows in CallTail [choice], then Over
- replay OdbcLiteralKind: Follows in OdbcEscape [choice], then NationalCharacterStringLiteral
- replay OdbcType: Follows in OdbcFunction [choice], then ')'
- replay OnOff: Follows in TypeStatement [choice], then ')'
- replay OptionList: Follows in SwitchTail [choice], then ')'
- replay OptionsWith: Follows in CreateTableStatement [choice], then "AS"i
- replay OrderByClause: Follows in WithinGroup [choice], then ?reading(0x1FDFDFD)
- replay OutputClause: Follows in TSqlInsert [choice], then InsertRows
- replay OutputList: Follows in OutputClause [choice], then "INTO"i
- replay PlacementTarget: Lookahead in SwitchTail [lookahead]
- replay PoolName: Follows in ExternalStatement [choice], then ExternalPoolWith
- replay PredictCall: Follows in TSqlTablePrimary [choice], then PredictSchema
- replay PriorityOptions: Follows in BrokerStatement [turn], then ')'
- replay QueryHints: Follows in TSqlInsert [choice], then when (options is null || rows is not Query.FromExecute)
- replay QueueWith: Follows in BrokerStatement [choice], then when (Syntax.NamedOnce(null, options) && Syntax.Activates(options))
- replay RoleWord: Follows in PrincipalStatement [choice], then TSqlIdentifier
- replay RouteOptions: Follows in BrokerStatement [choice], then when (Syntax.NamedOnce(null, options) && Syntax.HasOption(options, "AD…
- replay RowValueConstructorElement: Follows in RowValueConstructor [choice], then ',' …
- replay RowValueConstructor: Follows in TSqlPredicate [choice], then PredicateTail
- replay RowsetArgument: Follows in RowsetArgument [choice], then ')'
- replay RowsetFunction: Follows in TSqlTablePrimary [choice], then when (Syntax.Schemas(f, schema))
- replay RowsetSchema: Follows in TSqlTablePrimary [choice], then when (Syntax.Schemas(f, schema))
- replay RowsetValue: Follows in RowsetArgument [choice], then ',' …
- replay SearchCondition: Follows in MergeArm [choice], then "THEN"i
- replay ServerOrDatabase: Follows in AuditStatement [choice], then "AUDIT"i
- replay SetValue: Follows in CodeStatement [choice], then when (Syntax.Tail(tail) is not null)
- replay SpatialSetting: Follows in SpatialSet [choice], then ')'
- replay TSqlAlias: Follows in TSqlSelectSublist [choice], then '='
- replay TSqlJoinType: Follows in TSqlTableReference [turn], then "JOIN"i
- replay TSqlSubquery: Follows in TSqlTablePrimary [choice], then CorrelationName
- replay TSqlValueExpression: Follows in TSqlPrimaryCore [choice], then ')'
- replay TableAs: Follows in CreateTableStatement [choice], then '('
- replay TableBody: Follows in ExternalStatement [choice], then ')'
- replay TextColumn: Follows in TextStatement [turn], then TextPointer
- replay Top: Follows in TSqlInsert [choice], then DmlTarget
- replay WithClause: Follows in InlineReturn [choice], then TSqlQueryExpression
- replay WithinGroup: Follows in CallTail [choice], then Over
- replay Activation: under QueueOption
- replay AdHocObject: under TSqlTablePrimary
- replay AdHocServer: under TSqlTablePrimary
- replay AffinityValue: under OptionSetting
- replay Argument: under Arguments
- replay AssemblyOption: under AssemblyOptions
- replay AssignTail: under MemberTail
- replay AssignedValue: under VariableChainTail
- replay Assignment: under Assignments
- replay Assignments: under TSqlUpdate
- replay AtTimeZone: under TSqlValuePrimary
- replay BareKeyOption: under BareKeyOptions
- replay BareKeyOptions: under ConstraintWith
- replay BindingOption: under BindingOptions
- replay BooleanPrimary: under BooleanTest
- replay BooleanTerm: under SearchCondition
- replay BooleanTest: under TSqlBooleanFactor
- replay CallTail: under TSqlPrimaryCore
- replay CaseExpression: under TSqlPrimaryCore
- replay CastOperand: under TSqlCast
- replay ChainTail: under Assignment
- replay ChangeTrackingContext: under WithClause
- replay ChangedSource: under InsertRows
- replay ChangedStatement: under ChangedTable
- replay ChangedTable: under ChangedSource
- replay ChunksFunction: under TSqlTablePrimary
- replay ChunksOverlap: under ChunksFunction
- replay ChunksSet: under ChunksFunction
- replay ChunksSize: under ChunksFunction
- replay ChunksSource: under ChunksFunction
- replay ChunksType: under ChunksFunction
- replay ChunksValue: under ChunksSource
- replay Collate: under TSqlValueExpression
- replay ColumnBody: under ColumnDefinition
- replay ColumnName: under ColumnList
- replay ColumnReference: under TSqlPrimaryCore
- replay ColumnTail: under TableColumnBody
- replay ColumnTrait: under ColumnTail
- replay ConstraintOption: under ConstraintWith
- replay ConstraintWith: under ConstraintBody
- replay CountStar: under TSqlPrimaryCore
- replay CteBody: under CteDefinition
- replay CteDefinition: under WithClause
- replay CursorColumn: under CursorColumns
- replay CursorColumns: under CursorFor
- replay DataSourceChange: under DataSourceBody
- replay DataSourceItem: under DataSourceBody
- replay DataSourceOption: under DataSourceItem
- replay DatePartCall: under TSqlPrimaryCore
- replay DatePartFunction: under DatePartCall
- replay DbccValue: under ExecValue
- replay DistinctTail: under TSqlPredicate
- replay DmlCall: under TSqlInsert
- replay DmlWhere: under TSqlDelete
- replay DottedType: under ?
- replay EdgePair: under ConstraintBody
- replay EncryptionOption: under ColumnTrait
- replay Enforced: under ConstraintBody
- replay EventActions: under EventBody
- replay EventPredicate: under EventBody
- replay EventSetting: under EventSet
- replay ExactNumber: under ?
- replay ExecArgument: under ExecArguments
- replay ExecArguments: under ExecuteBody
- replay ExecAt: under ExecuteBody
- replay ExecContext: under ExecuteBody
- replay ExecContextKind: under ExecContext
- replay ExecContextName: under ExecContext
- replay ExecDataSource: under ExecuteBody
- replay ExecName: under ExecTarget
- replay ExecNumber: under ExecTarget
- replay ExecReturn: under ExecuteBody
- replay ExecTarget: under ExecuteBody
- replay ExecuteBody: under ExecuteStatement
- replay ExecuteStatement: under InsertRows
- replay ExternalTableItem: under ExternalTableWith
- replay ExternalTableOption: under ExternalTableItem
- replay FetchClause: under OffsetFetch
- replay ForClause: under TSqlSubquery
- replay FromClause: under TSqlQuerySpecification
- replay FullTextColumns: under FullTextPredicate
- replay FullTextPredicate: under TSqlPredicate
- replay FullTextSearch: under RowsetFunction
- replay FunctionCall: under TSqlPrimaryCore
- replay GraphMatch: under TSqlPredicate
- replay GroupByExpression: under GroupingSet
- replay GroupByItem: under TSqlGrouping
- replay GroupList: under GroupingSet
- replay GroupWith: under TSqlGroupByClause
- replay HavingClause: under TSqlQuerySpecification
- replay IdentityFunction: under TSqlSelectSublist
- replay IdentityNumber: under IdentityFunction
- replay InPredicateValue: under NegatablePredicate
- replay IndexColumn: under IndexColumns
- replay IndexColumns: under ConstraintBody
- replay IndexName: under IncludeColumns
- replay IndexOn: under ConstraintBody
- replay InsertColumn: under InsertColumns
- replay Into: under TSqlQuerySpecification
- replay JoinedTail: under JoinedRight
- replay JsonArrayBody: under TSqlValueFunction
- replay JsonDirective: under ForClause
- replay JsonFunction: under TSqlTablePrimary
- replay JsonKeyValue: under JsonKeyValues
- replay JsonKeyValues: under JsonObjectBody
- replay JsonMode: under ForClause
- replay JsonNullClause: under TSqlValueFunction
- replay JsonObjectBody: under TSqlValueFunction
- replay JsonOrder: under TSqlValueFunction
- replay JsonReturningJson: under TSqlValueFunction
- replay JsonSchemaColumn: under JsonSchema
- replay JsonSchema: under TSqlTablePrimary
- replay JsonValueReturning: under TSqlValueFunction
- replay JsonValue: under JsonKeyValue
- replay JsonWrapper: under TSqlValueFunction
- replay LanguageOption: under LanguageFile
- replay LanguagePlatform: under LanguageOption
- replay LeftRightCall: under TSqlPrimaryCore
- replay LevelOrDefault: under PriorityOption
- replay LocalVariable: under FetchRow
- replay MaskOption: under ColumnTrait
- replay Member: under TSqlValuePrimary
- replay MergeArm: under TSqlMerge
- replay MergeChange: under MergeArm
- replay MergeColumn: under MergeColumns
- replay MergeColumns: under MergeInsert
- replay MergeInsertRows: under MergeInsert
- replay MergeInsert: under MergeArm
- replay MethodCallTail: under Assignment
- replay ModelOption: under ModelOptions
- replay NameOrAny: under PriorityOption
- replay NamedConstraint: under ColumnTrait
- replay NegatablePredicate: under PredicateTail
- replay NextValue: under TSqlPrimaryCore
- replay NullSpec: under PredictColumn
- replay OdbcEscape: under TSqlPrimaryCore
- replay OdbcFunction: under OdbcEscape
- replay OffsetFetch: under TSqlSubquery
- replay OnPartitions: under OptionSetting
- replay OpenQueryCall: under RowsetFunction
- replay OptionNest: under OptionTail
- replay OptionSetting: under OptionList
- replay OptionTail: under OptionSetting
- replay OptionUnit: under OptionValue
- replay OptionValue: under OptionSetting
- replay OutputItem: under OutputList
- replay Over: under CallTail
- replay PivotName: under PivotNames
- replay PivotNames: under Pivot
- replay PivotSuffix: under TSqlTableReference
- replay Pivot: under PivotSuffix
- replay PredicateTail: under TSqlPredicate
- replay PredictColumn: under PredictSchema
- replay PredictSchema: under TSqlTablePrimary
- replay PriorityOption: under PriorityOptions
- replay QueryHint: under QueryHints
- replay QueryPrimary: under TSqlQueryExpression
- replay QueueOption: under QueueWith
- replay ReferenceAction: under ConstraintBody
- replay ReferenceOn: under References
- replay References: under ConstraintBody
- replay ResultColumns: under ?
- replay Result: under CaseExpression
- replay RouteOption: under RouteOptions
- replay RowsetArguments: under PredictCall
- replay RowsetOrder: under RowsetArgument
- replay SampleUnit: under TableSample
- replay SampledHint: under TSqlTablePrimary
- replay ScalarSubquery: under TSqlPrimaryCore
- replay SchemaCollate: under JsonSchemaColumn
- replay SchemaColumn: under RowsetSchema
- replay SchemaOrdinal: under SchemaColumn
- replay SchemaPath: under JsonSchemaColumn
- replay SearchLanguage: under RowsetFunction
- replay SearchTop: under RowsetFunction
- replay SearchedColumn: under FullTextColumns
- replay SearchedWhen: under CaseExpression
- replay SemanticArgument: under SemanticArguments
- replay SemanticArguments: under RowsetFunction
- replay SetFunctionSpecification: under TSqlPrimaryCore
- replay SortSpecification: under OrderByClause
- replay SortedData: under BareKeyOption
- replay SourceHints: under TSqlTablePrimary
- replay SpatialItem: under SpatialSetting
- replay StringOrAny: under PriorityOption
- replay SwitchTail: under OptionTail
- replay SystemTimeWhen: under SystemTime
- replay SystemTime: under TSqlTablePrimary
- replay TSqlAsClause: under TSqlSelectSublist
- replay TSqlBooleanFactor: under BooleanTerm
- replay TSqlCast: under TSqlPrimaryCore
- replay TSqlDelete: under ChangedStatement
- replay TSqlEscapeClause: under NegatablePredicate
- replay TSqlGroupByClause: under TSqlQuerySpecification
- replay TSqlGroupingColumn: under GroupByExpression
- replay TSqlGrouping: under TSqlGroupByClause
- replay TSqlInsert: under ChangedStatement
- replay TSqlMerge: under ChangedStatement
- replay TSqlPredicate: under BooleanPrimary
- replay TSqlPrimaryCore: under TSqlValuePrimary
- replay TSqlQueryExpression: under TSqlSubquery
- replay TSqlQuerySpecification: under QueryPrimary
- replay TSqlRow: under TSqlTableValueConstructor
- replay TSqlSelectList: under TSqlQuerySpecification
- replay TSqlSelectSublist: under TSqlSelectList
- replay TSqlSimpleWhen: under CaseExpression
- replay TSqlTablePrimary: under JoinedRight
- replay TSqlTableReference: under FromClause
- replay TSqlTableValueConstructor: under QueryPrimary
- replay TSqlUpdate: under ChangedStatement
- replay TSqlValueFunction: under TSqlPrimaryCore
- replay TSqlValuePrimary: under TSqlValueExpression
- replay TSqlValueSpecification: under TSqlPrimaryCore
- replay TableColumnBody: under TableColumn
- replay TableColumn: under TableElement
- replay TableElement: under TableBody
- replay TableHint: under SampledHint
- replay TableHints: under SourceHints
- replay TableIndexOption: under TableIndexWith
- replay TableIndexWith: under TableIndex
- replay TableIndex: under TableElement
- replay TableRepeatable: under TableSample
- replay TableSample: under TSqlTablePrimary
- replay TableSearchColumn: under TableSearchColumns
- replay TableSearchColumns: under RowsetFunction
- replay TimeZone: under AtTimeZone
- replay TopSuffix: under Top
- replay Unpivot: under PivotSuffix
- replay UnsignedLiteral: under TSqlPrimaryCore
- replay UpdateVariableTail: under Assignment
- replay UseModel: under FunctionCall
- replay ValueFunction: under TSqlPrimaryCore
- replay VariableChainTail: under UpdateVariableTail
- replay VariableSource: under TSqlTablePrimary
- replay WhereClause: under TSqlQuerySpecification
- replay WindowDef: under Window
- replay WindowFrame: under WindowSpecification
- replay WindowSpecification: under WindowDef
- replay Window: under TSqlQuerySpecification
- replay XmlDirective: under ForClause
- replay XmlMode: under ForClause
- replay XmlNamespaces: under WithClause

## DotGram.Tests.Calculators.DecimalCalculator

- machine Evaluate [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 18/18

## DotGram.Tests.Calculators.OneRuleParser

- machine Read [whole]: carrier: tape; gate: read again; building: 1; replayed: 0; read again: 1; refused: 0; points: 15/15
- again Expr: opens a way
- open Expr: turns, captured; the seam leads every alternative of the turn; open; (trivia & '+' & trivia & right: Expr => (new Add(left, right)) | trivi…

## DotGram.Tests.Calculators.StrengthCalculator

- machine Evaluate [whole]: carrier: tape; gate: read again; building: 1; replayed: 0; read again: 1; refused: 0; points: 15/15
- again Expr: opens a way
- open Expr: turns, captured; the seam leads every alternative of the turn; open; (trivia & '+' & trivia & right: Expr => (left + right) | trivia & '-' …

## DotGram.Tests.Calculators.TwoCalculators

- machine EvaluateInt [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 17/17
- machine EvaluateDouble [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 17/17

## DotGram.Tests.Extents

- machine ParseExtent [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 1/1

## DotGram.Web.Rfc3339

- machine ParseTimestamp, ParseFullDate, ParseFullTime [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 5/5

## DotGram.Web.Rfc3986

- machine ParseReference, ParseUri [whole]: carrier: tape; gate: read again; building: 6; replayed: 0; read again: 12; refused: 0; points: 19/19
- again Authority: opens a way
- open Authority: optional, captured; a turn led by what may read nothing; open; (user: UserInfoText & '@')?
- again DecOctet: opens a way
- open DecOctet: choice; alternatives begin alike; open; ('1' & Digit & Digit | ['1'..'9'] & Digit | Digit)
- open DecOctet: choice; alternatives begin alike; open; ("25" & ['0'..'5'] | '2' & ['0'..'4'] & Digit | ['1'..'9'] & Digit | D…
- open DecOctet: choice; alternatives begin alike; open; (['1'..'9'] & Digit | Digit)
- again HierPart: opens a way
- open HierPart: choice; an alternative that may read nothing; open; ("//" & a: Authority & path: PathAbEmpty => (a with { Path = path }) |…
- again HostText: opens a way
- open HostText: choice; an alternative that may read nothing; open; (IPLiteral | IPv4Address | RegName)
- again IPLiteral: through IPv6Address
- again IPv4Address: through DecOctet
- again IPv6Address: opens a way
- open IPv6Address: choice; alternatives begin alike; open; ((H16 & ':'){6} & Ls32 | H16? & "::" & (H16 & ':'){4} & Ls32 | ((H16 &…
- open IPv6Address: optional; what follows begins alike; open; (H16 & ':')?
- open IPv6Address: counted; what follows begins alike; open; (H16 & ':'){0,2}
- open IPv6Address: counted; what follows begins alike; open; (H16 & ':'){0,3}
- open IPv6Address: counted; what follows begins alike; open; (H16 & ':'){0,4}
- open IPv6Address: counted; what follows begins alike; open; (H16 & ':'){0,5}
- open IPv6Address: counted; what follows begins alike; open; (H16 & ':'){0,6}
- open IPv6Address: choice; alternatives begin alike; open; ("::" & (H16 & ':'){5} & Ls32 | H16? & "::" & (H16 & ':'){4} & Ls32 | …
- again Ls32: opens a way
- open Ls32: choice; alternatives begin alike; open; (H16 & ':' & H16 | IPv4Address)
- again Reference: opens a way
- open Reference: choice; an alternative that may read nothing; entry; (u: Uri => (u) | ?!SchemeMark & r: RelativeRef => (r))
- again RelativePart: opens a way
- open RelativePart: choice; an alternative that may read nothing; open; ("//" & a: Authority & path: PathAbEmpty => (a with { Path = path }) |…
- again RelativeRef: through RelativePart
- again Uri: through HierPart

## DotGram.Web.Rfc5322

- machine ParseAddressList, ParseMailboxList, ParseMailbox, ParseAddrSpec [whole]: carrier: tape; gate: read again; building: 17; replayed: 0; read again: 32; refused: 0; points: 47/47
- machine ParseStrictAddrSpec [whole]: carrier: tape; gate: read again; building: 4; replayed: 0; read again: 15; refused: 0; points: 9/9
- machine ParseStrictMailbox [whole]: carrier: tape; gate: read again; building: 6; replayed: 0; read again: 18; refused: 0; points: 15/15
- machine ParseStrictMailboxList [whole]: carrier: tape; gate: read again; building: 8; replayed: 0; read again: 20; refused: 0; points: 21/21
- machine ParseStrictAddressList [whole]: carrier: tape; gate: read again; building: 13; replayed: 0; read again: 25; refused: 0; points: 37/37
- again AddrSpecRule: through Domain
- again AddrSpecRule: through CurrentLocalPart
- again AddrSpecRule: through CurrentLocalPart
- again AddrSpecRule: through CurrentLocalPart
- again AddrSpecRule: through CurrentLocalPart
- again Address: opens a way
- open Address: choice; alternatives begin alike; open; (mailbox: Mailbox => (mailbox) | group: Group => (group))
- again AddressList: through NullMembers
- again AddressList: through Address
- again Address: opens a way
- open Address: choice; alternatives begin alike; open; (mailbox: Mailbox_With4 => (mailbox) | group: Group_With4 => (group))
- again AngleAddr: opens a way
- open AngleAddr: optional; a turn led by what may read nothing; open; ObsRoute?
- again AngleAddr: through Cfws
- again AngleAddr: through Cfws
- again AngleAddr: through Cfws
- again Atom: through Cfws
- again Ccontent: through Comment
- again Ccontent: through Comment
- again Ccontent: through Comment
- again Ccontent: through Comment
- again Ccontent: through Comment
- again Cfws: opens a way
- open Cfws: choice; alternatives begin alike; open; ({ (Fws? & Comment)+ } & Fws? | Fws)
- open Cfws: optional; what follows begins alike; open; Fws?
- again Cfws: opens a way
- open Cfws: choice; alternatives begin alike; open; ({ (CurrentFws? & Comment_With1)+ } & CurrentFws? | CurrentFws)
- again Cfws: opens a way
- open Cfws: choice; alternatives begin alike; open; ({ (CurrentFws? & Comment_With2)+ } & CurrentFws? | CurrentFws)
- open Cfws: optional; a turn led by what may read nothing; open; CurrentFws?
- again Cfws: opens a way
- open Cfws: choice; alternatives begin alike; open; ({ (CurrentFws? & Comment_With3)+ } & CurrentFws? | CurrentFws)
- open Cfws: optional; a turn led by what may read nothing; open; CurrentFws?
- again Cfws: opens a way
- open Cfws: choice; alternatives begin alike; open; ({ (CurrentFws? & Comment_With4)+ } & CurrentFws? | CurrentFws)
- open Cfws: optional; a turn led by what may read nothing; open; CurrentFws?
- again Comment: opens a way
- open Comment: turns; a turn led by what may read nothing; open; (Fws? & Ccontent)*
- again Comment: opens a way
- open Comment: turns; a turn led by what may read nothing; open; (CurrentFws? & Ccontent_With1)*
- again Comment: opens a way
- open Comment: turns; a turn led by what may read nothing; open; (CurrentFws? & Ccontent_With2)*
- again Comment: opens a way
- open Comment: turns; a turn led by what may read nothing; open; (CurrentFws? & Ccontent_With3)*
- again Comment: opens a way
- open Comment: turns; a turn led by what may read nothing; open; (CurrentFws? & Ccontent_With4)*
- again Ctext: opens a way
- open Ctext: choice; alternatives begin apart; open; (['!'..'\'' | '*'..'[' | ']'..'~'] | Never)
- again Ctext: opens a way
- open Ctext: choice; alternatives begin apart; open; (['!'..'\'' | '*'..'[' | ']'..'~'] | Never)
- again Ctext: opens a way
- open Ctext: choice; alternatives begin apart; open; (['!'..'\'' | '*'..'[' | ']'..'~'] | Never)
- again Ctext: opens a way
- open Ctext: choice; alternatives begin apart; open; (['!'..'\'' | '*'..'[' | ']'..'~'] | Never)
- again CurrentDomain: opens a way
- open CurrentDomain: choice; every alternative led by what may read nothing; open; (literal: DomainLiteral_With1 => (literal) | Cfws_With1? & text: DotAt…
- again CurrentDomain: opens a way
- open CurrentDomain: choice; every alternative led by what may read nothing; open; (literal: DomainLiteral_With2 => (literal) | Cfws_With2? & text: DotAt…
- again CurrentDomain: opens a way
- open CurrentDomain: choice; every alternative led by what may read nothing; open; (literal: DomainLiteral_With3 => (literal) | Cfws_With3? & text: DotAt…
- again CurrentDomain: opens a way
- open CurrentDomain: choice; every alternative led by what may read nothing; open; (literal: DomainLiteral_With4 => (literal) | Cfws_With4? & text: DotAt…
- again CurrentFws: opens a way
- open CurrentFws: optional; a turn led by what may read nothing; open; (Wsp* & Crlf)?
- again CurrentLocalPart: opens a way
- open CurrentLocalPart: choice; every alternative led by what may read nothing; open; (Cfws_With1? & text: DotAtomText & Cfws_With1? => (text) | Cfws_With1?…
- again CurrentLocalPart: opens a way
- open CurrentLocalPart: choice; every alternative led by what may read nothing; open; (Cfws_With2? & text: DotAtomText & Cfws_With2? => (text) | Cfws_With2?…
- again CurrentLocalPart: opens a way
- open CurrentLocalPart: choice; every alternative led by what may read nothing; open; (Cfws_With3? & text: DotAtomText & Cfws_With3? => (text) | Cfws_With3?…
- again CurrentLocalPart: opens a way
- open CurrentLocalPart: choice; every alternative led by what may read nothing; open; (Cfws_With4? & text: DotAtomText & Cfws_With4? => (text) | Cfws_With4?…
- again CurrentPhrase: opens a way
- open CurrentPhrase: turns; what follows begins alike; open; WordText_With2+
- again CurrentPhrase: opens a way
- open CurrentPhrase: turns; what follows begins alike; open; WordText_With3+
- again CurrentPhrase: opens a way
- open CurrentPhrase: turns; what follows begins alike; open; WordText_With4+
- again Domain: opens a way
- open Domain: choice; every alternative led by what may read nothing; open; (literal: DomainLiteral => (literal) | first: Atom & rest: DotAtom* =>…
- again DomainLiteral: through Cfws
- again DomainLiteralBody: opens a way
- open DomainLiteralBody: turns; a turn led by what may read nothing; open; (Fws? & Dtext)*
- again DomainLiteralBody: opens a way
- open DomainLiteralBody: turns; a turn led by what may read nothing; open; (CurrentFws? & Dtext_With1)*
- again DomainLiteralBody: opens a way
- open DomainLiteralBody: turns; a turn led by what may read nothing; open; (CurrentFws? & Dtext_With2)*
- again DomainLiteralBody: opens a way
- open DomainLiteralBody: turns; a turn led by what may read nothing; open; (CurrentFws? & Dtext_With3)*
- again DomainLiteralBody: opens a way
- open DomainLiteralBody: turns; a turn led by what may read nothing; open; (CurrentFws? & Dtext_With4)*
- again DomainLiteral: through Cfws
- again DomainLiteral: through Cfws
- again DomainLiteral: through Cfws
- again DomainLiteral: through Cfws
- again DotAtom: through Atom
- again DotWord: through Word
- again Dtext: through ObsDtext
- again Dtext: opens a way
- open Dtext: choice; alternatives begin apart; open; (['!'..'Z' | '^'..'~'] | Never)
- again Dtext: opens a way
- open Dtext: choice; alternatives begin apart; open; (['!'..'Z' | '^'..'~'] | Never)
- again Dtext: opens a way
- open Dtext: choice; alternatives begin apart; open; (['!'..'Z' | '^'..'~'] | Never)
- again Dtext: opens a way
- open Dtext: choice; alternatives begin apart; open; (['!'..'Z' | '^'..'~'] | Never)
- again Group: through Cfws
- again GroupList: opens a way
- open GroupList: choice; an alternative that may read nothing; open; (list: MailboxList => (list) | ObsGroupList => (Array.Empty<EmailAddre…
- again GroupList: opens a way
- open GroupList: choice; an alternative that may read nothing; open; (list: MailboxList_With4 => (list) | Never => (Array.Empty<EmailAddres…
- again Group: through CurrentPhrase
- again LocalPart: through Word
- again Mailbox: opens a way
- open Mailbox: choice; every alternative led by what may read nothing; open; (name: Phrase? & address: AngleAddr => (new EmailAddress.Mailbox(Rfc53…
- open Mailbox: optional, captured; what follows begins alike; open; name: Phrase?
- again MailboxList: through NullMembers
- again MailboxList: through Mailbox
- again MailboxList: through Mailbox
- again Mailbox: opens a way
- open Mailbox: choice; every alternative led by what may read nothing; entry; (name: CurrentPhrase_With2? & address: AngleAddr_With2 => (new EmailAd…
- open Mailbox: optional, captured; what follows begins alike; entry; name: CurrentPhrase_With2?
- again Mailbox: opens a way
- open Mailbox: choice; every alternative led by what may read nothing; open; (name: CurrentPhrase_With3? & address: AngleAddr_With3 => (new EmailAd…
- open Mailbox: optional, captured; what follows begins alike; open; name: CurrentPhrase_With3?
- again Mailbox: opens a way
- open Mailbox: choice; every alternative led by what may read nothing; open; (name: CurrentPhrase_With4? & address: AngleAddr_With4 => (new EmailAd…
- open Mailbox: optional, captured; what follows begins alike; open; name: CurrentPhrase_With4?
- again NextAddress: opens a way
- open NextAddress: choice; an alternative that may read nothing; open; (address: Address => (new[] { address }) | NullMember => (Array.Empty<…
- again NextAddress: opens a way
- open NextAddress: choice; alternatives begin apart; open; (address: Address_With4 => (new[] { address }) | Never => (Array.Empty…
- again NextMailbox: opens a way
- open NextMailbox: choice; an alternative that may read nothing; open; (mailbox: Mailbox => (new[] { mailbox }) | NullMember => (Array.Empty<…
- again NextMailbox: opens a way
- open NextMailbox: choice; alternatives begin apart; open; (mailbox: Mailbox_With3 => (new[] { mailbox }) | Never => (Array.Empty…
- again NextMailbox: opens a way
- open NextMailbox: choice; alternatives begin apart; open; (mailbox: Mailbox_With4 => (new[] { mailbox }) | Never => (Array.Empty…
- again NullMember: through Cfws
- again NullMembers: opens a way
- open NullMembers: turns; a turn led by what may read nothing; open; (Cfws? & ',')*
- again ObsDtext: through QuotedPair
- again ObsGroupList: opens a way
- open ObsGroupList: turns; a turn led by what may read nothing; open; (Cfws? & ',')+
- again ObsPhrase: opens a way
- open ObsPhrase: turns; what follows begins alike; open; (WordText | '.' | Cfws)*
- open ObsPhrase: choice; alternatives begin alike; open; (WordText | Cfws)
- again ObsRoute: through Cfws
- again Phrase: through ObsPhrase
- again Qcontent: through QuotedPair
- again Qcontent: through QuotedPair
- again Qcontent: through QuotedPair
- again Qcontent: through QuotedPair
- again Qcontent: through QuotedPair
- again Qtext: opens a way
- open Qtext: choice; alternatives begin apart; open; (['!' | '#'..'[' | ']'..'~'] | Never)
- again Qtext: opens a way
- open Qtext: choice; alternatives begin apart; open; (['!' | '#'..'[' | ']'..'~'] | Never)
- again Qtext: opens a way
- open Qtext: choice; alternatives begin apart; open; (['!' | '#'..'[' | ']'..'~'] | Never)
- again Qtext: opens a way
- open Qtext: choice; alternatives begin apart; open; (['!' | '#'..'[' | ']'..'~'] | Never)
- again QuotedBody: opens a way
- open QuotedBody: turns; a turn led by what may read nothing; open; (Fws? & Qcontent)*
- again QuotedBody: opens a way
- open QuotedBody: turns; a turn led by what may read nothing; open; (CurrentFws? & Qcontent_With1)*
- again QuotedBody: opens a way
- open QuotedBody: turns; a turn led by what may read nothing; open; (CurrentFws? & Qcontent_With2)*
- again QuotedBody: opens a way
- open QuotedBody: turns; a turn led by what may read nothing; open; (CurrentFws? & Qcontent_With3)*
- again QuotedBody: opens a way
- open QuotedBody: turns; a turn led by what may read nothing; open; (CurrentFws? & Qcontent_With4)*
- again QuotedPair: opens a way
- open QuotedPair: choice; alternatives begin alike; open; ('\\' & ['\t' | ' '..'~'] | ObsQp)
- again QuotedPair: opens a way
- open QuotedPair: choice; alternatives begin apart; open; ('\\' & ['\t' | ' '..'~'] | Never)
- again QuotedPair: opens a way
- open QuotedPair: choice; alternatives begin apart; open; ('\\' & ['\t' | ' '..'~'] | Never)
- again QuotedPair: opens a way
- open QuotedPair: choice; alternatives begin apart; open; ('\\' & ['\t' | ' '..'~'] | Never)
- again QuotedPair: opens a way
- open QuotedPair: choice; alternatives begin apart; open; ('\\' & ['\t' | ' '..'~'] | Never)
- again Word: opens a way
- open Word: choice; every alternative led by what may read nothing; open; (Cfws? & text: AtomText & Cfws? => (text) | Cfws? & body: QuotedBody &…

## DotGram.Web.Rfc5646

- machine ParseTag [whole]: carrier: tape; gate: read again; building: 9; replayed: 0; read again: 6; refused: 0; points: 22/22
- again Extension: opens a way
- open Extension: turns, captured; what follows begins alike; open; subtags: ExtensionSubtag+
- again LangTag: opens a way
- open LangTag: choice; alternatives begin alike; open; (language: ShortLanguage & extended: ExtLang{0,3} & tail: Tail => (tai…
- open LangTag: counted, captured; what follows begins alike; open; extended: ExtLang{0,3}
- again LanguageTag: opens a way
- open LanguageTag: choice; alternatives begin alike; entry; (registered: Grandfathered & eof => (Rfc5646.Registered(registered)) |…
- open LanguageTag: choice; alternatives begin alike; entry; (tag: LangTag => (tag) | subtags: PrivateUse => (new LanguageTag(null,…
- again Tail: opens a way
- open Tail: optional, captured; what follows begins alike; open; ('-' & script: Script)?
- open Tail: optional, captured; what follows begins alike; open; ('-' & region: Region)?
- open Tail: turns, captured; what follows begins alike; open; variants: Variant*
- open Tail: turns, captured; what follows begins alike; open; extensions: Extension*
- again Variant: through VariantText
- again VariantText: opens a way
- open VariantText: choice; alternatives begin alike; open; (Alphanum{5,8} | Digit & Alphanum{3})

## DotGram.Web.Rfc6265

- machine ParseSetCookie [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 5/5
- machine ReadDateTokens [whole]: carrier: tape; gate: read again; building: 2; replayed: 0; read again: 1; refused: 0; points: 3/3
- machine ReadTime [whole]: carrier: tape; gate: read again; building: 1; replayed: 0; read again: 2; refused: 0; points: 1/1
- machine ReadDay [whole]: carrier: tape; gate: read again; building: 1; replayed: 0; read again: 2; refused: 0; points: 1/1
- machine ReadMonth [whole]: carrier: tape; gate: read again; building: 1; replayed: 0; read again: 1; refused: 0; points: 12/12
- machine ReadYear [whole]: carrier: tape; gate: read again; building: 1; replayed: 0; read again: 2; refused: 0; points: 1/1
- again CookieDate: opens a way
- open CookieDate: turns, captured; what follows begins alike; entry; rest: NextDateToken*
- again DayToken: through Tail
- again MonthToken: opens a way
- open MonthToken: choice; alternatives begin alike; entry; ("apr"i & any* => (4) | "aug"i & any* => (8))
- open MonthToken: choice; alternatives begin alike; entry; ("jan"i & any* => (1) | "jun"i & any* => (6) | "jul"i & any* => (7))
- open MonthToken: choice; alternatives begin alike; entry; ("mar"i & any* => (3) | "may"i & any* => (5))
- again Tail: opens a way
- open Tail: run; what follows begins alike; open; any*
- again TimeToken: through Tail
- again YearToken: through Tail

## DotGram.Web.Rfc6266

- machine ParseContentDisposition [whole]: carrier: tape; gate: read again; building: 3; replayed: 0; read again: 2; refused: 0; points: 5/5
- again DispositionField: through DispositionParms
- again DispositionParms: opens a way
- open DispositionParms: turns, captured; a turn led by what may read nothing; open; items: DispositionParm*

## DotGram.Web.Rfc6570

- machine ParseTemplate [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 10/10

## DotGram.Web.Rfc6901

- machine ParsePointer [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 3/3
- machine ParseFragment [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 1/1

## DotGram.Web.Rfc7239

- machine ParseForwarded [whole]: carrier: tape; gate: read again; building: 6; replayed: 0; read again: 4; refused: 0; points: 12/12
- machine ParseNode [whole]: carrier: tape; gate: read again; building: 1; replayed: 0; read again: 5; refused: 0; points: 4/4
- again DecOctet: opens a way
- open DecOctet: choice; alternatives begin alike; open; ('1' & Digit & Digit | ['1'..'9'] & Digit | Digit)
- open DecOctet: choice; alternatives begin alike; open; ("25" & ['0'..'5'] | '2' & ['0'..'4'] & Digit | ['1'..'9'] & Digit | D…
- open DecOctet: choice; alternatives begin alike; open; (['1'..'9'] & Digit | Digit)
- again ElementList: opens a way
- open ElementList: turns, captured; a turn led by what may read nothing; open; rest: NextElement*
- again ForwardedField: through Ows
- again IPv4Address: through DecOctet
- again IPv6Address: opens a way
- open IPv6Address: choice; alternatives begin alike; open; ((H16 & ':'){6} & Ls32 | H16? & "::" & (H16 & ':'){4} & Ls32 | ((H16 &…
- open IPv6Address: optional; what follows begins alike; open; (H16 & ':')?
- open IPv6Address: counted; what follows begins alike; open; (H16 & ':'){0,2}
- open IPv6Address: counted; what follows begins alike; open; (H16 & ':'){0,3}
- open IPv6Address: counted; what follows begins alike; open; (H16 & ':'){0,4}
- open IPv6Address: counted; what follows begins alike; open; (H16 & ':'){0,5}
- open IPv6Address: counted; what follows begins alike; open; (H16 & ':'){0,6}
- open IPv6Address: choice; alternatives begin alike; open; ("::" & (H16 & ':'){5} & Ls32 | H16? & "::" & (H16 & ':'){4} & Ls32 | …
- again Ls32: opens a way
- open Ls32: choice; alternatives begin alike; open; (H16 & ':' & H16 | IPv4Address)
- again NextElement: through Ows
- again Node: through IPv4Address
- again Ows: opens a way
- open Ows: run; what follows begins alike; open; ['\t' | ' ']*

## DotGram.Web.Rfc8259

- machine ParseJson [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 29/29

## DotGram.Web.Rfc8288

- machine ParseLinks [whole]: carrier: tape; gate: read again; building: 5; replayed: 0; read again: 7; refused: 0; points: 9/9
- again Assignment: opens a way
- open Assignment: optional; what follows begins alike; open; ('=' & Ows & (Token | QuotedString))?
- again Element: opens a way
- open Element: choice; an alternative that may read nothing; open; ((',' & Ows)+ | ?!any)
- again Field: through Ows
- again LinkParam: through Ows
- again LinkValue: opens a way
- open LinkValue: turns, captured; a turn led by what may read nothing; open; parameters: LinkParam*
- again Ows: opens a way
- open Ows: run; what follows begins alike; open; ['\t' | ' ']*
- again Token: opens a way
- open Token: run; what follows begins alike; open; Tchar+

## DotGram.Web.Rfc9110

- machine ParseContentType [whole]: carrier: tape; gate: read again; building: 4; replayed: 0; read again: 7; refused: 0; points: 7/7
- again ContentTypeField: through Ows
- again Media: through Token
- again Ows: opens a way
- open Ows: run; what follows begins alike; open; ['\t' | ' ']*
- again ParameterSlot: through Ows
- again ParameterText: opens a way
- open ParameterText: optional; what follows begins alike; open; (Token & '=' & (Token | QuotedString))?
- again Parameters: opens a way
- open Parameters: turns, captured; a turn led by what may read nothing; open; slots: ParameterSlot*
- again Token: opens a way
- open Token: run; what follows begins alike; open; Tchar+

## DotGram.Web.Rfc9651

- machine ParseItem [whole]: carrier: tape; gate: read again; building: 7; replayed: 0; read again: 9; refused: 0; points: 22/22
- machine ParseList [whole]: carrier: tape; gate: read again; building: 11; replayed: 0; read again: 13; refused: 0; points: 34/34
- machine ParseDictionary [whole]: carrier: tape; gate: read again; building: 12; replayed: 0; read again: 14; refused: 0; points: 37/37
- again DictMember: opens a way
- open DictMember: choice; an alternative that may read nothing; open; ('=' & value: ListMember | parameters: SfParameters)
- again DictRest: through DictMember
- again DictionaryField: opens a way
- open DictionaryField: turns, captured; a turn led by what may read nothing; entry; rest: DictRest*
- again InnerMember: through SfItem
- again ItemField: through SfItem
- again Key: opens a way
- open Key: run; what follows begins alike; open; ['*' | '-'..'.' | '0'..'9' | '_' | 'a'..'z']*
- again ListField: opens a way
- open ListField: turns, captured; a turn led by what may read nothing; entry; rest: ListRest*
- again ListMember: through SfItem
- again ListRest: through ListMember
- again Parameter: through SfBareItem
- again SfBareItem: opens a way
- open SfBareItem: choice; every alternative led by what may read nothing; open; (text: SfDecimal => (new BareItem.Decimal(Rfc9651.Decimal(text))) | te…
- again SfDecimal: opens a way
- open SfDecimal: run; what follows begins alike; open; Digit{1,3}
- again SfInnerList: through SfParameters
- again SfInteger: opens a way
- open SfInteger: run; what follows begins alike; open; Digit{1,15}
- again SfItem: through SfBareItem
- again SfParameters: through Parameter
- again SfToken: opens a way
- open SfToken: run; what follows begins alike; open; ['!' | '#'..'\'' | '*'..'+' | '-'..':' | 'A'..'Z' | '^'..'z' | '|' | '…
