# Which carrier each grammar is read with, and why

Every grammar the last build with `-p:DotGramReportGeneration=full` compiled: the carrier
`Auto` took (GRAM5012), and for a grammar kept on the tape, the gate that kept it and each rule
held there. Written by `--carriers` (`benchmarks/DotGram.Benchmarks/Carriers.cs`) from the reports
that build left; run again rather than edited.

Read from 10 projects, and written when each one was last compiled with the
report on. A project built below that level leaves no report and is absent here rather than
empty, so a short table is a short build and not a grammar with nothing to say; a project whose
time is older than the rest was not in the last build, and its rows are that build's answer.

| Read from | Report written |
| --- | --- |
| DotGram.Benchmarks | 2026-10-06 19:28 |
| DotGram.Examples | 2026-10-06 19:27 |
| DotGram.ExpressionLanguage | 2026-10-06 19:27 |
| DotGram.ExpressionLanguage.Immediate | 2026-10-06 19:27 |
| DotGram.Finance | 2026-10-06 19:27 |
| DotGram.Finance.Fix44 | 2026-10-06 19:27 |
| DotGram.Sql | 2026-10-06 19:27 |
| DotGram.Sql.Productions | 2026-10-06 19:27 |
| DotGram.Tests | 2026-10-06 19:28 |
| DotGram.Web | 2026-10-06 19:27 |

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
| DotGram.Benchmarks.Levels | 1 | 0 | immediate | none |  |  |  |  |  |  | 19/19 |
| DotGram.Benchmarks.MaterializationCost.NoCaptures | 1 | 1 | tape | read again | 1 | 0 | 0 | 2 | 0 | 0 | 1/1 |
| DotGram.Benchmarks.MaterializationCost.SpanCaptures | 1 | 1 | tape | replay | 7 | 1 | 1 | 0 | 0 | 0 | 17/17 |
| DotGram.Benchmarks.MaterializationCost.WithCaptures | 1 | 1 | tape | read again | 1 | 0 | 0 | 2 | 0 | 0 | 1/1 |
| DotGram.Benchmarks.Nesting | 1 | 1 | nothing to choose | none |  |  |  |  |  |  | 0/0 |
| DotGram.Benchmarks.Numbers | 1 | 0 | immediate | none |  |  |  |  |  |  | 3/3 |
| DotGram.Benchmarks.Possession.Open | 1 | 1 | nothing to choose | none |  |  |  |  |  |  | 0/0 |
| DotGram.Benchmarks.Possession.Settled | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 0/0 |
| DotGram.Benchmarks.Settlements | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 7/7 |
| DotGram.Benchmarks.TapeSql |  |  | tape (author) |  |  |  |  |  |  |  | 12/244 |
| DotGram.Benchmarks.TapeSqlStandard |  |  | tape (author) |  |  |  |  |  |  |  | 1248/2605 |
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
| DotGram.ExpressionLanguage.ExpressionParser | 1 | 1 | tape | replay | 103 | 97 | 26 | 0 | 0 | 0 | 23/505 |
| DotGram.ExpressionLanguage.ExpressionParser.Immediate |  |  | immediate (author) |  |  |  |  |  |  |  | 23/505 |
| DotGram.Finance.Fix.Fix44.Fix44Grammar | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 1830/1830 |
| DotGram.Finance.Fix.Fix44.FixFieldGrammar | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 0/0 |
| DotGram.Finance.Fix.FixGrammar |  |  | immediate (author) |  |  |  |  |  |  |  | 14/14 |
| DotGram.Sql.Productions.SqlStandardProductions |  |  | tape (author) |  |  |  |  |  |  |  | 1299/2659 |
| DotGram.Sql.Standard.Sql92Parser |  |  | immediate (author) |  |  |  |  |  |  |  | 12/244 |
| DotGram.Sql.Standard.SqlStandardParser |  |  | immediate (author) |  |  |  |  |  |  |  | 1248/2605 |
| DotGram.Sql.TransactSql.TransactSqlParser |  |  | immediate (author) |  |  |  |  |  |  |  | 2352/3138 |
| DotGram.Tests.Calculators.DecimalCalculator | 1 | 0 | immediate | none |  |  |  |  |  |  | 18/18 |
| DotGram.Tests.Calculators.OneRuleParser | 1 | 1 | tape | read again | 1 | 0 | 0 | 1 | 0 | 0 | 15/15 |
| DotGram.Tests.Calculators.StrengthCalculator | 1 | 1 | tape | read again | 1 | 0 | 0 | 1 | 0 | 0 | 15/15 |
| DotGram.Tests.Calculators.TwoCalculators | 2 | 0 | immediate | none |  |  |  |  |  |  | 34/34 |
| DotGram.Tests.Extents | 1 | 0 | immediate | none |  |  |  |  |  |  | 1/1 |
| DotGram.Tests.Generated.UrlGrammar | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 1/1 |
| DotGram.Tests.RereadUnderSubstitutionTests.Reread | 0 | 0 | nothing to choose | none |  |  |  |  |  |  | 16/16 |
| DotGram.Web.Rfc3339 | 1 | 0 | immediate | none |  |  |  |  |  |  | 5/5 |
| DotGram.Web.Rfc3986 |  |  | immediate (author) |  |  |  |  |  |  |  | 19/19 |
| DotGram.Web.Rfc5322 |  |  | immediate (author) |  |  |  |  |  |  |  | 129/129 |
| DotGram.Web.Rfc5646 |  |  | immediate (author) |  |  |  |  |  |  |  | 22/22 |
| DotGram.Web.Rfc6265 |  |  | immediate (author) |  |  |  |  |  |  |  | 29/29 |
| DotGram.Web.Rfc6266 |  |  | immediate (author) |  |  |  |  |  |  |  | 5/5 |
| DotGram.Web.Rfc6570 | 1 | 0 | immediate | none |  |  |  |  |  |  | 10/10 |
| DotGram.Web.Rfc6901 | 2 | 0 | immediate | none |  |  |  |  |  |  | 4/4 |
| DotGram.Web.Rfc7239 |  |  | immediate (author) |  |  |  |  |  |  |  | 16/16 |
| DotGram.Web.Rfc8259 | 1 | 0 | immediate | none |  |  |  |  |  |  | 29/29 |
| DotGram.Web.Rfc8288 |  |  | immediate (author) |  |  |  |  |  |  |  | 9/9 |
| DotGram.Web.Rfc9110 |  |  | immediate (author) |  |  |  |  |  |  |  | 13/13 |
| DotGram.Web.Rfc9651 |  |  | immediate (author) |  |  |  |  |  |  |  | 44/44 |

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
| DotGram.ExpressionLanguage.ExpressionParser | ParseLambda, ParseHole, ParseBody | whole | tape | replay | 103 | 97 | 0 | 0 | 23/505 |
| DotGram.Tests.Calculators.DecimalCalculator | Evaluate | whole | immediate | none | 0 | 0 | 0 | 0 | 18/18 |
| DotGram.Tests.Calculators.OneRuleParser | Read | whole | tape | read again | 1 | 0 | 1 | 0 | 15/15 |
| DotGram.Tests.Calculators.StrengthCalculator | Evaluate | whole | tape | read again | 1 | 0 | 1 | 0 | 15/15 |
| DotGram.Tests.Calculators.TwoCalculators | EvaluateInt | whole | immediate | none | 0 | 0 | 0 | 0 | 17/17 |
| DotGram.Tests.Calculators.TwoCalculators | EvaluateDouble | whole | immediate | none | 0 | 0 | 0 | 0 | 17/17 |
| DotGram.Tests.Extents | ParseExtent | whole | immediate | none | 0 | 0 | 0 | 0 | 1/1 |
| DotGram.Web.Rfc3339 | ParseTimestamp, ParseFullDate, ParseFullTime | whole | immediate | none | 0 | 0 | 0 | 0 | 5/5 |
| DotGram.Web.Rfc6570 | ParseTemplate | whole | immediate | none | 0 | 0 | 0 | 0 | 10/10 |
| DotGram.Web.Rfc6901 | ParsePointer | whole | immediate | none | 0 | 0 | 0 | 0 | 3/3 |
| DotGram.Web.Rfc6901 | ParseFragment | whole | immediate | none | 0 | 0 | 0 | 0 | 1/1 |
| DotGram.Web.Rfc8259 | ParseJson | whole | immediate | none | 0 | 0 | 0 | 0 | 29/29 |

## Where the second gate's ways are opened

Each place a rule's own reading opens a way, by its shape — a choice over characters, a run of
one character, the turns of a repetition — and why the way could not be left out. **Places**
counts each once per grammar; **captured** is how many of them capture inside the turn, and
**sealed** how many are in a rule every call of which is inside an atomic group or a lookahead,
or which nothing calls, so that no caller asks it again.

| Shape | Why the way stays | Grammars | Rules | Places | Captured | Sealed | For example |
| --- | --- | ---: | ---: | ---: | ---: | ---: | --- |
| run | what follows begins alike | 9 | 14 | 14 | 0 | 0 | Calculator.Spacing: `Whitespace+` |
| choice | alternatives begin alike | 8 | 8 | 9 | 0 | 3 | NoCaptures.Host: `(IPv4 \| RegName)` |
| turns | the seam leads every alternative of the turn | 5 | 5 | 7 | 7 | 0 | Climbing.Expr: `(trivia & '+' & trivia & r: Expr => (l + r) \| trivia & '-' & trivia & …` |
| optional | what follows begins alike | 6 | 6 | 6 | 2 | 2 | NoCaptures.Url: `(UserInfo & '@')?` |
| choice | the seam leads every alternative | 2 | 2 | 4 | 0 | 0 | Calculator.Expr: `(trivia & '+' & trivia & right: Expr_With1 => (left + right) \| trivia …` |
| choice | an alternative that may read nothing | 3 | 4 | 4 | 0 | 0 | HttpParser.Field: `(eol \| ?=eof)` |
| choice | literals, a shorter one wanted | 4 | 4 | 4 | 0 | 0 | HttpParser.eol: `("\r\n" \| '\r')` |
| turns | a turn led by what may read nothing | 2 | 2 | 2 | 2 | 0 | IniParser.Entries: `(item0: Entry \| Blank)*` |
| choice | every alternative led by what may read nothing | 2 | 2 | 2 | 0 | 0 | IniParser.Entries: `(item0: Entry \| Blank)` |
| turns | what follows begins alike | 2 | 2 | 2 | 1 | 1 | JsonParser.Body: `(Plain \| Escape)*` |
| choice | literals, follow unknown | 1 | 1 | 1 | 0 | 0 | FeedReader.eol: `("\r\n" \| '\r')` |
| turns | seam first, what follows begins alike past it | 1 | 1 | 1 | 0 | 1 | Scoped.Program: `(trivia & Let)*` |

## DotGram.Benchmarks.CallCost.Called

- machine ParseStart [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 1/1
- memo ParseStart [whole]: remembered 0; not 1: Letter (characters)

## DotGram.Benchmarks.CallCost.Valued

- machine ParseStart [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 5/5
- memo ParseStart [whole]: remembered 0; not 1: Letter (characters)

## DotGram.Benchmarks.Climbing

- machine Climbed [whole]: carrier: tape; gate: read again; building: 1; replayed: 0; read again: 1; refused: 0; points: 13/13
- memo Climbed [whole]: remembered 0; not 1: Expr (characters)
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
- memo Levelled [whole]: remembered 0; not 2: Sum (characters), Unary (characters)

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
- memo ParseExpr [whole]: remembered 0; not 1: Expr (characters)

## DotGram.Benchmarks.Numbers

- machine ParseSum [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 3/3

## DotGram.Benchmarks.Possession.Open

- machine ParseDoc [whole]: carrier: tape; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 0/0

## DotGram.Benchmarks.TapeSql

- memo ParseSelect, ParseQuery, ParseSearchCondition, ParseValueExpression [whole]: remembered 4: QueryExpression, ValueExpression, SearchCondition, TableReference; not 0

## DotGram.Benchmarks.TapeSqlStandard

- memo ParseExpression, ParseDataType, ParseSearchCondition [whole]: remembered 19: ValueExpression, Disjunction, Subquery, QueryExpression, QueryExpressionBody, TableReference, TableFactor, DatetimeValueExpression, CommonValueExpression, CommonValueExpressionOrRow, DataType, JSONSubscript, JSONPathWff, JSONPathPredicate, RowPattern, JSONTableColumnsClause, JSONTablePlan, PartitionedJoin, GroupingElement; not 0
- memo ParseSql, ParseStatement [whole]: remembered 20: QueryExpression, ValueNode, Disjunction, PeriodConstructor, DatetimeValueExpression, CommonValueExpression, CommonValueExpressionOrRow, DataType, JSONSubscript, JSONPathWff, JSONPathPredicate, RowPattern, QueryExpressionBody, TableReference, TableFactor, JSONTableColumnsClause, JSONTablePlan, PartitionedJoin, GroupingElement, SQLExecutableStatement; not 0

## DotGram.Benchmarks.TinyScalar

- machine ParseDepth [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 3/3
- memo ParseDepth [whole]: remembered 0; not 1: Depth (characters)

## DotGram.Benchmarks.Urls

- machine ParseUrl [whole]: carrier: tape; gate: read again; building: 2; replayed: 0; read again: 3; refused: 0; points: 1/1
- again Authority: opens a way
- open Authority: optional, captured; what follows begins alike; open; (user: UserInfo & '@')?
- again Host: opens a way
- open Host: choice; alternatives begin alike; open; (IPv4 | RegName)
- again Url: through Authority

## DotGram.Examples.Expressions.ArithmeticTree

- machine Read [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 22/22
- memo Read [whole]: remembered 0; not 2: Sum (characters), Unary (characters)

## DotGram.Examples.Expressions.Calculator

- machine EvaluateInt [whole]: carrier: tape; gate: read again; building: 2; replayed: 0; read again: 3; refused: 0; points: 17/17
- machine EvaluateDecimal [whole]: carrier: tape; gate: read again; building: 2; replayed: 0; read again: 5; refused: 0; points: 17/17
- machine BuildTree [whole]: carrier: tape; gate: read again; building: 2; replayed: 0; read again: 5; refused: 0; points: 17/17
- memo EvaluateInt [whole]: remembered 0; not 1: Expr (characters)
- memo EvaluateDecimal [whole]: remembered 0; not 1: Expr (characters)
- memo BuildTree [whole]: remembered 0; not 1: Expr (characters)
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
- memo Read [whole]: remembered 0; not 1: Sum (characters)

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
- memo ParseJson [whole]: remembered 0; not 1: Value (characters)
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
- memo ParseXml [whole]: remembered 0; not 1: Element (characters)
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
- memo ParseFilter [whole]: remembered 0; not 1: Expr (characters)
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
- memo ParseFilter [whole]: remembered 0; not 1: Test (characters)
- replay Test: Follows in Test [choice], then ')'

## DotGram.Examples.Languages.GramGrammar

- machine ParseFile [whole]: carrier: tape; gate: replay; building: 35; replayed: 28; read again: 0; refused: 0; points: 108/108
- memo ParseFile [whole]: remembered 0; not 7: Type (characters), Namespace (characters), With (characters), QuantifiedCore (characters), Primary (characters), RefOrCall (characters), AnyTest (characters)
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
- memo ParseProgram [whole]: remembered 0; not 1: Expr (characters)
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

- machine ParseLambda, ParseHole, ParseBody [whole]: carrier: tape; gate: replay; building: 103; replayed: 97; read again: 0; refused: 0; points: 23/505
- memo ParseLambda, ParseHole, ParseBody [whole]: remembered 0; not 11: Type (context), Body (context), Block (context), Statement (context), IfValue (context), Assignment (context), Conditional (context), Coalesce (context), Binary (context), Unary (context), Bindings (context)
- replay Arm: Follows in Binary [turn], then '}'
- replay Assignment: Follows in Primary [choice], then ']'
- replay Char: Lookahead in TestedMarked [lookahead]
- replay Conditional: Follows in Conditional [turn], then ':'
- replay Core: Follows in Primary [choice], then '.'
- replay Discard: Follows in Arm [choice], then "=>"
- replay Elements: Follows in TargetNew [turn], then '}'
- replay Identifier: Lookahead in Constant [choice]
- replay Interpolated: Lookahead in TestedMarked [lookahead]
- replay Name: Follows in Constant [choice], then ?="=>" or ':' or Identifier
- replay Parameter: Follows in Function [choice], then ')'
- replay RawByHand: Lookahead in TestedMarked [lookahead]
- replay RawDoubled3: Lookahead in TestedMarked [lookahead]
- replay RawDoubled4: Lookahead in TestedMarked [lookahead]
- replay RawDoubled5: Lookahead in TestedMarked [lookahead]
- replay RawInterpolated3: Lookahead in TestedMarked [lookahead]
- replay RawInterpolated4: Lookahead in TestedMarked [lookahead]
- replay RawInterpolated5: Lookahead in TestedMarked [lookahead]
- replay RawText3: Lookahead in TestedMarked [lookahead]
- replay RawText4: Lookahead in TestedMarked [lookahead]
- replay RawText5: Lookahead in TestedMarked [lookahead]
- replay Target: Follows in Assignment [choice], then '/' & value: Assignment => (ExpressionParser.AddAssign(target, value, …
- replay Text: Lookahead in TestedMarked [lookahead]
- replay Type: Follows in NamedType [turn], then '>'
- replay Verbatim: Lookahead in TestedMarked [lookahead]
- replay VerbatimInterpolated: Lookahead in TestedMarked [lookahead]
- replay Arguments: under Postfix
- replay Awaiting: under Untyped
- replay Bin: under Primary
- replay Binary: under Coalesce
- replay Binding: under Bindings
- replay Bindings: under Instanced
- replay Block: under Body
- replay Body: under Inner
- replay Case: under Switch
- replay Catch: under Try
- replay Change: under Binary
- replay Coalesce: under Conditional
- replay Constant: under Pattern
- replay Control: under Body
- replay Dec: under Primary
- replay Decimals: under Primary
- replay DoWhile: under Control
- replay Doubles: under Primary
- replay Element: under Elements
- replay Fallback: under Switch
- replay Floats: under Primary
- replay For: under Control
- replay ForLoop: under For
- replay Foreach: under Control
- replay ForeachInferred: under Control
- replay ForeachUnsettled: under Control
- replay Guarded: under Postfix
- replay Held: under Untyped
- replay HeldBody: under Held
- replay Hex: under Primary
- replay If: under Control
- replay IfValue: under Body
- replay ImplicitArray: under Primary
- replay Indices: under Postfix
- replay Inferred: under Statement
- replay InferredUnsettled: under Statement
- replay Initial: under Binding
- replay Inner: under Primary
- replay Instanced: under Primary
- replay Jump: under Statement
- replay Label: under Case
- replay Local: under Statement
- replay Marked: under Type
- replay NamedType: under Core
- replay Or: under Pattern
- replay Parenthesised: under Primary
- replay Pattern: under Arm
- replay Postfix: under Unary
- replay Primary: under Postfix
- replay Real: under Primary
- replay Return: under Statement
- replay SignedLong: under Primary
- replay SignedLong: under Primary
- replay SignedLong: under Primary
- replay Statement: under Block
- replay Step: under Guarded
- replay Strict: under Instanced
- replay Switch: under Control
- replay TargetNew: under Primary
- replay Tested: under Binary
- replay TestedMarked: under Tested
- replay Try: under Control
- replay Unary: under Binary
- replay UnsignedLong: under Primary
- replay UnsignedLong: under Primary
- replay UnsignedLong: under Primary
- replay Unsigned: under Primary
- replay Unsigned: under Primary
- replay Unsigned: under Primary
- replay Untyped: under Primary
- replay While: under Control

## DotGram.ExpressionLanguage.ExpressionParser.Immediate

- memo ParseLambda, ParseHole, ParseBody [whole]: remembered 0; not 11: Type (context), Body (context), Block (context), Statement (context), IfValue (context), Assignment (context), Conditional (context), Coalesce (context), Binary (context), Unary (context), Bindings (context)

## DotGram.Sql.Productions.SqlStandardProductions

- memo ParseExpression, ParseValueExpression, ParseDataType, ParseUnsignedLiteral, ParseIdentifierChain, ParseColumnReference, ParseQueryExpression, ParseQuerySpecification, ParseTableReference, ParseInsertStatement, ParseUpdateStatementSearched, ParseDeleteStatementSearched, ParseMergeStatement, ParseSearchCondition, ParseCommonValueExpression, ParseNumericValueExpression, ParseStringValueExpression, ParseCharacterValueExpression, ParseBinaryValueExpression, ParseDatetimeValueExpression, ParseIntervalValueExpression, ParseBooleanValueExpression, ParsePredicate, ParseRowValuePredicand, ParseValueExpressionPrimary [whole]: remembered 19: ValueExpression, Disjunction, Subquery, QueryExpression, QueryExpressionBody, TableReference, TableFactor, DatetimeValueExpression, CommonValueExpression, CommonValueExpressionOrRow, DataType, JSONSubscript, JSONPathWff, JSONPathPredicate, RowPattern, JSONTableColumnsClause, JSONTablePlan, PartitionedJoin, GroupingElement; not 0
- memo ParseSql, ParseStatement, ParseUpdateStatementPositioned, ParseDeleteStatementPositioned, ParseTruncateTableStatement, ParseSQLSchemaStatement, ParseSQLTransactionStatement, ParseSQLConnectionStatement, ParseSQLSessionStatement, ParseSQLDiagnosticsStatement, ParseSQLControlStatement, ParseSQLDataStatement, ParseSQLDynamicStatement, ParseDirectSQLStatement, ParseDirectSQLDataStatement, ParseSQLProcedureStatement [whole]: remembered 20: QueryExpression, ValueNode, Disjunction, PeriodConstructor, DatetimeValueExpression, CommonValueExpression, CommonValueExpressionOrRow, DataType, JSONSubscript, JSONPathWff, JSONPathPredicate, RowPattern, QueryExpressionBody, TableReference, TableFactor, JSONTableColumnsClause, JSONTablePlan, PartitionedJoin, GroupingElement, SQLExecutableStatement; not 0

## DotGram.Sql.Standard.Sql92Parser

- memo ParseSelect, ParseQuery, ParseSearchCondition, ParseValueExpression [whole]: remembered 4: QueryExpression, ValueExpression, SearchCondition, TableReference; not 0

## DotGram.Sql.Standard.SqlStandardParser

- memo ParseExpression, ParseDataType, ParseSearchCondition [whole]: remembered 19: ValueExpression, Disjunction, Subquery, QueryExpression, QueryExpressionBody, TableReference, TableFactor, DatetimeValueExpression, CommonValueExpression, CommonValueExpressionOrRow, DataType, JSONSubscript, JSONPathWff, JSONPathPredicate, RowPattern, JSONTableColumnsClause, JSONTablePlan, PartitionedJoin, GroupingElement; not 0
- memo ParseSql, ParseStatement [whole]: remembered 20: QueryExpression, ValueNode, Disjunction, PeriodConstructor, DatetimeValueExpression, CommonValueExpression, CommonValueExpressionOrRow, DataType, JSONSubscript, JSONPathWff, JSONPathPredicate, RowPattern, QueryExpressionBody, TableReference, TableFactor, JSONTableColumnsClause, JSONTablePlan, PartitionedJoin, GroupingElement, SQLExecutableStatement; not 0

## DotGram.Sql.TransactSql.TransactSqlParser

- memo ParseSelect, ParseQuery, ParseSearchCondition, ParseValueExpression [whole]: remembered 10: TSqlValueExpression, SearchCondition, TSqlBooleanFactor, TSqlSubquery, TSqlQueryExpression, TSqlTableReference, RowsetArgument, JoinedRight, DatePart, TimeZone; not 0
- memo ParseStatement, ParseStatement100, ParseStatement110, ParseStatement120, ParseStatement130, ParseStatement140, ParseStatement150, ParseStatement160, ParseStatement170, ParseSql, ParseSql100, ParseSql110, ParseSql120, ParseSql130, ParseSql140, ParseSql150, ParseSql160, ParseSql170 [whole]: remembered 20: TSqlValueExpression, SearchCondition, TSqlBooleanFactor, TSqlSubquery, TSqlQueryExpression, TSqlTableReference, RowsetArgument, JoinedRight, DatePart, TimeZone, TSqlInsert, OutputClause, OptionSetting, TryCatchStatement, StatementList, ConditionalStatement, Branch, EventValue, EventPredicate, BackupName; not 0

## DotGram.Tests.Calculators.DecimalCalculator

- machine Evaluate [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 18/18
- memo Evaluate [whole]: remembered 0; not 2: Sum (characters), Unary (characters)

## DotGram.Tests.Calculators.OneRuleParser

- machine Read [whole]: carrier: tape; gate: read again; building: 1; replayed: 0; read again: 1; refused: 0; points: 15/15
- memo Read [whole]: remembered 0; not 1: Expr (characters)
- again Expr: opens a way
- open Expr: turns, captured; the seam leads every alternative of the turn; open; (trivia & '+' & trivia & right: Expr => (new Add(left, right)) | trivi…

## DotGram.Tests.Calculators.StrengthCalculator

- machine Evaluate [whole]: carrier: tape; gate: read again; building: 1; replayed: 0; read again: 1; refused: 0; points: 15/15
- memo Evaluate [whole]: remembered 0; not 1: Expr (characters)
- again Expr: opens a way
- open Expr: turns, captured; the seam leads every alternative of the turn; open; (trivia & '+' & trivia & right: Expr => (left + right) | trivia & '-' …

## DotGram.Tests.Calculators.TwoCalculators

- machine EvaluateInt [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 17/17
- machine EvaluateDouble [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 17/17
- memo EvaluateInt [whole]: remembered 0; not 2: Sum (characters), Unary (characters)
- memo EvaluateDouble [whole]: remembered 0; not 2: Sum (characters), Unary (characters)

## DotGram.Tests.Extents

- machine ParseExtent [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 1/1

## DotGram.Web.Rfc3339

- machine ParseTimestamp, ParseFullDate, ParseFullTime [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 5/5

## DotGram.Web.Rfc5322

- memo ParseAddressList, ParseMailboxList, ParseMailbox, ParseAddrSpec [whole]: remembered 0; not 1: Comment (characters)
- memo ParseStrictAddrSpec [whole]: remembered 0; not 1: Comment (characters)
- memo ParseStrictMailbox [whole]: remembered 0; not 1: Comment (characters)
- memo ParseStrictMailboxList [whole]: remembered 0; not 1: Comment (characters)
- memo ParseStrictAddressList [whole]: remembered 0; not 1: Comment (characters)

## DotGram.Web.Rfc6570

- machine ParseTemplate [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 10/10

## DotGram.Web.Rfc6901

- machine ParsePointer [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 3/3
- machine ParseFragment [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 1/1

## DotGram.Web.Rfc8259

- machine ParseJson [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 29/29
- memo ParseJson [whole]: remembered 0; not 1: Value (characters)
