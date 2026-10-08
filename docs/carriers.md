# Which carrier each grammar is read with, and why

Every grammar the last build with `-p:DotGramReportGeneration=full` compiled: the carrier each
is read with, and for a grammar left to the generator (`Auto`), which carries it as `Immediate`
wherever that carrier does not refuse a machine, what the tape would have held back and what the
build is told of it. Written by `--carriers` (`benchmarks/DotGram.Benchmarks/Carriers.cs`) from the
reports that build left; run again rather than edited.

Read from 10 projects, and written when each one was last compiled with the
report on. A project built below that level leaves no report and is absent here rather than
empty, so a short table is a short build and not a grammar with nothing to say; a project whose
time is older than the rest was not in the last build, and its rows are that build's answer.

| Read from | Report written |
| --- | --- |
| DotGram.Benchmarks | 2026-10-08 16:31 |
| DotGram.Examples | 2026-10-08 16:30 |
| DotGram.ExpressionLanguage | 2026-10-08 16:30 |
| DotGram.ExpressionLanguage.Immediate | 2026-10-08 16:30 |
| DotGram.Finance | 2026-10-08 16:30 |
| DotGram.Finance.Fix44 | 2026-10-08 16:30 |
| DotGram.Sql | 2026-10-08 16:30 |
| DotGram.Sql.Productions | 2026-10-08 16:30 |
| DotGram.Tests | 2026-10-08 16:31 |
| DotGram.Web | 2026-10-08 16:30 |

**Carrier** is what a grammar left to the generator is read with — `immediate`, or `tape` where the
immediate carrier refused a machine — or the author's own choice. **Gate** is what the tape would
hold back that the immediate carrier runs where it is read: `replay` — a building rule read where
the reading may not stand (`Replay`) — or `read again` — a rule the reader can be asked again after
it answered, which is asked only where the first gate holds nothing. **Reasons** are what the build
is told of a machine carried immediately: `shares context` (constructions and the hooks run during
recognition share `context`) and `rebuilds lists` (a refused input can rebuild a list on every turn
given back) are a warning, **Told** `GRAM5016`; `writes context`, `replay` and `read again` are
information, `GRAM5012`. **Direct** is how many of the replayed
rules have a cause of their own; the rest are under one of them. **Points** is how many of the
sites that build — a call whose value is built, a construction — have a point past which what they
read is settled (`Commit`), of how many there are: what building at that point could take off the
tape. **Refused** is how many of a grammar's machines the immediate carrier refuses, which are read on
the tape in silence (gate `refused` where nothing else would be held back), each named under the
grammar with its reason; their building rules count in **Building**. **Alone** is how many of those
neither gate holds anything back of: what lifting the refusal would move without a word said.

A carrier is settled per machine, and a grammar has one machine per publication group and input
form, so the grammar's row is a **summary** and shows the worst of its machines. What a machine
answers is in the table below it, and that is the row to read before expecting anything of a
change: a grammar on the tape may have a machine that is not.

| Grammar (summary) | Machines | On the tape | Carrier | Gate | Reasons | Told | Building | Replayed | Direct | Read again | Refused | Alone | Points |
| --- | ---: | ---: | --- | --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| DotGram.Benchmarks.CallCost.Called | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 1/1 |
| DotGram.Benchmarks.CallCost.Inlined | 0 | 0 | nothing to choose | none | none | none |  |  |  |  |  |  | 1/1 |
| DotGram.Benchmarks.CallCost.Valued | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 5/5 |
| DotGram.Benchmarks.Climbing | 1 | 0 | immediate | read again | read again | GRAM5012 | 1 | 0 | 0 | 1 | 0 | 0 | 13/13 |
| DotGram.Benchmarks.Config | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 7/7 |
| DotGram.Benchmarks.Extents | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 1/1 |
| DotGram.Benchmarks.Feed | 0 | 0 | nothing to choose | none | none | none |  |  |  |  |  |  | 7/7 |
| DotGram.Benchmarks.Flat.Lowered | 0 | 0 | nothing to choose | none | none | none |  |  |  |  |  |  | 0/0 |
| DotGram.Benchmarks.Flat.NotLowered | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 0/0 |
| DotGram.Benchmarks.Levels | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 19/19 |
| DotGram.Benchmarks.MaterializationCost.NoCaptures | 1 | 0 | immediate | read again | read again | GRAM5012 | 1 | 0 | 0 | 2 | 0 | 0 | 1/1 |
| DotGram.Benchmarks.MaterializationCost.SpanCaptures | 1 | 0 | immediate | replay | replay | GRAM5012 | 7 | 1 | 1 | 0 | 0 | 0 | 17/17 |
| DotGram.Benchmarks.MaterializationCost.WithCaptures | 1 | 0 | immediate | read again | read again | GRAM5012 | 1 | 0 | 0 | 2 | 0 | 0 | 1/1 |
| DotGram.Benchmarks.Nesting | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 0/0 |
| DotGram.Benchmarks.Numbers | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 3/3 |
| DotGram.Benchmarks.Possession.Open | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 0/0 |
| DotGram.Benchmarks.Possession.Settled | 0 | 0 | nothing to choose | none | none | none |  |  |  |  |  |  | 0/0 |
| DotGram.Benchmarks.Settlements | 0 | 0 | nothing to choose | none | none | none |  |  |  |  |  |  | 7/7 |
| DotGram.Benchmarks.TapeSql |  |  | tape (author) |  |  |  |  |  |  |  |  |  | 12/244 |
| DotGram.Benchmarks.TapeSqlStandard |  |  | tape (author) |  |  |  |  |  |  |  |  |  | 1248/2605 |
| DotGram.Benchmarks.TinyScalar | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 3/3 |
| DotGram.Benchmarks.Urls | 1 | 0 | immediate | read again | read again | GRAM5012 | 2 | 0 | 0 | 3 | 0 | 0 | 1/1 |
| DotGram.Examples.Expressions.ArithmeticTree | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 22/22 |
| DotGram.Examples.Expressions.Calculator | 3 | 0 | immediate | read again | read again | GRAM5012 | 6 | 0 | 0 | 8 | 0 | 0 | 51/51 |
| DotGram.Examples.Expressions.ClampedExample | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 14/14 |
| DotGram.Examples.Expressions.LocaleNumber | 2 | 0 | immediate | read again | read again | GRAM5012 | 2 | 0 | 0 | 2 | 0 | 0 | 4/4 |
| DotGram.Examples.Feeds.FeedReader | 1 | 0 | immediate | read again | read again | GRAM5012 | 5 | 0 | 0 | 5 | 0 | 0 | 5/5 |
| DotGram.Examples.Feeds.LoggingFeedReader | 0 | 0 | nothing to choose | none | none | none |  |  |  |  |  |  | 5/5 |
| DotGram.Examples.Feeds.RecoveringFeedReader | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 6/6 |
| DotGram.Examples.Feeds.StockCountReader | 2 | 0 | immediate | none | none | none |  |  |  |  |  |  | 5/5 |
| DotGram.Examples.Feeds.StreamingFeedReader | 0 | 0 | nothing to choose | none | none | none |  |  |  |  |  |  | 9/9 |
| DotGram.Examples.Formats.Config | 2 | 0 | immediate | read again | read again | GRAM5012 | 3 | 0 | 0 | 3 | 0 | 0 | 6/6 |
| DotGram.Examples.Formats.Config.Located | 2 | 0 | immediate | read again | read again | GRAM5012 | 3 | 0 | 0 | 3 | 0 | 0 | 6/6 |
| DotGram.Examples.Formats.FileNames | 1 | 0 | immediate | read again | read again | GRAM5012 | 2 | 0 | 0 | 2 | 0 | 0 | 4/4 |
| DotGram.Examples.Formats.FixParser | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 9/9 |
| DotGram.Examples.Formats.FixedWidth | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 22/22 |
| DotGram.Examples.Formats.HttpParser | 1 | 0 | immediate | read again | read again | GRAM5012 | 5 | 0 | 0 | 6 | 0 | 0 | 10/10 |
| DotGram.Examples.Formats.IniParser | 1 | 0 | immediate | read again | read again | GRAM5012 | 7 | 0 | 0 | 11 | 0 | 0 | 14/14 |
| DotGram.Examples.Formats.JsonParser | 1 | 0 | immediate | read again | read again | GRAM5012 | 9 | 0 | 0 | 11 | 0 | 0 | 30/30 |
| DotGram.Examples.Formats.Links | 0 | 0 | nothing to choose | none | none | none |  |  |  |  |  |  | 1/1 |
| DotGram.Examples.Formats.MarkdownParser | 1 | 0 | immediate | read again | read again | GRAM5012 | 9 | 0 | 0 | 8 | 0 | 0 | 24/24 |
| DotGram.Examples.Formats.MetricsLine | 1 | 0 | immediate | read again | read again | GRAM5012 | 5 | 0 | 0 | 3 | 0 | 0 | 11/11 |
| DotGram.Examples.Formats.Netstrings | 1 | 0 | immediate | read again | read again | GRAM5012 | 2 | 0 | 0 | 1 | 0 | 0 | 3/3 |
| DotGram.Examples.Formats.TypedCsv | 0 | 0 | nothing to choose | none | none | none |  |  |  |  |  |  | 12/12 |
| DotGram.Examples.Formats.XmlParser | 1 | 0 | immediate | replay | replay | GRAM5012 | 7 | 6 | 3 | 0 | 0 | 0 | 21/21 |
| DotGram.Examples.Formats.YamlLite | 1 | 0 | immediate | read again | read again | GRAM5012 | 6 | 0 | 0 | 7 | 0 | 0 | 11/11 |
| DotGram.Examples.Languages.Filter | 1 | 0 | immediate | replay | replay | GRAM5012 | 9 | 6 | 2 | 0 | 0 | 0 | 33/33 |
| DotGram.Examples.Languages.FilterFile | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 4/4 |
| DotGram.Examples.Languages.Filters | 1 | 0 | immediate | replay | replay | GRAM5012 | 2 | 1 | 1 | 0 | 0 | 0 | 8/8 |
| DotGram.Examples.Languages.GramGrammar | 1 | 0 | immediate | replay | replay | GRAM5012 | 35 | 28 | 2 | 0 | 0 | 0 | 108/108 |
| DotGram.Examples.Languages.Lexemes | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 0/0 |
| DotGram.Examples.Languages.Scoped |  |  | tape (author) |  |  |  |  |  |  |  |  |  | 13/13 |
| DotGram.Examples.Languages.Selectors | 1 | 0 | immediate | read again | read again | GRAM5012 | 6 | 0 | 0 | 1 | 0 | 0 | 15/15 |
| DotGram.Examples.Languages.SettingsFile | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 3/3 |
| DotGram.Examples.Languages.SqlDialect | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 3/3 |
| DotGram.Examples.Languages.SqlReadOnly | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 0/0 |
| DotGram.Examples.Languages.TokenizedQuery | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 12/12 |
| DotGram.ExpressionLanguage.ExpressionParser |  |  | tape (author) |  |  |  |  |  |  |  |  |  | 23/505 |
| DotGram.ExpressionLanguage.ExpressionParser.Immediate |  |  | immediate (author) |  |  |  |  |  |  |  |  |  | 23/505 |
| DotGram.Finance.Fix.Fix44.Fix44Grammar | 0 | 0 | nothing to choose | none | none | none |  |  |  |  |  |  | 1830/1830 |
| DotGram.Finance.Fix.Fix44.FixFieldGrammar | 0 | 0 | nothing to choose | none | none | none |  |  |  |  |  |  | 0/0 |
| DotGram.Finance.Fix.FixGrammar |  |  | immediate (author) |  |  |  |  |  |  |  |  |  | 14/14 |
| DotGram.Sql.Productions.SqlStandardProductions |  |  | tape (author) |  |  |  |  |  |  |  |  |  | 1299/2659 |
| DotGram.Sql.Standard.Sql92Parser |  |  | immediate (author) |  |  |  |  |  |  |  |  |  | 12/244 |
| DotGram.Sql.Standard.SqlStandardParser |  |  | immediate (author) |  |  |  |  |  |  |  |  |  | 1248/2605 |
| DotGram.Sql.TransactSql.TransactSqlParser |  |  | immediate (author) |  |  |  |  |  |  |  |  |  | 2352/3138 |
| DotGram.Tests.Calculators.DecimalCalculator | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 18/18 |
| DotGram.Tests.Calculators.OneRuleParser | 1 | 0 | immediate | read again | read again | GRAM5012 | 1 | 0 | 0 | 1 | 0 | 0 | 15/15 |
| DotGram.Tests.Calculators.StrengthCalculator | 1 | 0 | immediate | read again | read again | GRAM5012 | 1 | 0 | 0 | 1 | 0 | 0 | 15/15 |
| DotGram.Tests.Calculators.TwoCalculators | 2 | 0 | immediate | none | none | none |  |  |  |  |  |  | 34/34 |
| DotGram.Tests.Extents | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 1/1 |
| DotGram.Tests.Generated.UrlGrammar | 0 | 0 | nothing to choose | none | none | none |  |  |  |  |  |  | 1/1 |
| DotGram.Tests.RereadUnderSubstitutionTests.Reread | 0 | 0 | nothing to choose | none | none | none |  |  |  |  |  |  | 16/16 |
| DotGram.Web.Rfc3339 | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 5/5 |
| DotGram.Web.Rfc3986 |  |  | immediate (author) |  |  |  |  |  |  |  |  |  | 19/19 |
| DotGram.Web.Rfc5322 | 5 | 0 | immediate | read again | read again | GRAM5012 | 48 | 0 | 0 | 99 | 0 | 0 | 129/129 |
| DotGram.Web.Rfc5646 | 1 | 0 | immediate | read again | read again | GRAM5012 | 9 | 0 | 0 | 6 | 0 | 0 | 22/22 |
| DotGram.Web.Rfc6265 |  |  | immediate (author) |  |  |  |  |  |  |  |  |  | 29/29 |
| DotGram.Web.Rfc6266 |  |  | immediate (author) |  |  |  |  |  |  |  |  |  | 5/5 |
| DotGram.Web.Rfc6570 | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 10/10 |
| DotGram.Web.Rfc6901 | 2 | 0 | immediate | none | none | none |  |  |  |  |  |  | 4/4 |
| DotGram.Web.Rfc7239 |  |  | immediate (author) |  |  |  |  |  |  |  |  |  | 16/16 |
| DotGram.Web.Rfc8259 | 1 | 0 | immediate | none | none | none |  |  |  |  |  |  | 29/29 |
| DotGram.Web.Rfc8288 |  |  | immediate (author) |  |  |  |  |  |  |  |  |  | 9/9 |
| DotGram.Web.Rfc9110 | 1 | 0 | immediate | read again | read again | GRAM5012 | 4 | 0 | 0 | 6 | 0 | 0 | 13/13 |
| DotGram.Web.Rfc9651 | 3 | 0 | immediate | read again | read again | GRAM5012 | 15 | 0 | 0 | 17 | 0 | 0 | 44/44 |

## Machine by machine

One row a machine: what it publishes, the form it reads (`whole` for a text held whole, `buffered`
for a reader, `bytes` for a byte stream), and its own carrier, gate and counts. The counts are the
machine's own, and so are the points: a site belongs to the machine that reads the rule it stands in.

| Grammar | Publishes | Form | Carrier | Gate | Building | Replayed | Read again | Refused | Points |
| --- | --- | --- | --- | --- | ---: | ---: | ---: | ---: | ---: |
| DotGram.Benchmarks.CallCost.Called | ParseStart | whole | immediate | none | 0 | 0 | 0 | 0 | 1/1 |
| DotGram.Benchmarks.CallCost.Valued | ParseStart | whole | immediate | none | 0 | 0 | 0 | 0 | 5/5 |
| DotGram.Benchmarks.Climbing | Climbed | whole | immediate | read again | 1 | 0 | 1 | 0 | 13/13 |
| DotGram.Benchmarks.Config | ParseFile | whole | immediate | none | 0 | 0 | 0 | 0 | 7/7 |
| DotGram.Benchmarks.Extents | ParseLetters | whole | immediate | none | 0 | 0 | 0 | 0 | 1/1 |
| DotGram.Benchmarks.Flat.NotLowered | ParseDoc | whole | immediate | none | 0 | 0 | 0 | 0 | 0/0 |
| DotGram.Benchmarks.Levels | Levelled | whole | immediate | none | 0 | 0 | 0 | 0 | 19/19 |
| DotGram.Benchmarks.MaterializationCost.NoCaptures | ParseUrl | whole | immediate | read again | 1 | 0 | 2 | 0 | 1/1 |
| DotGram.Benchmarks.MaterializationCost.SpanCaptures | ParseUrl | whole | immediate | replay | 7 | 1 | 0 | 0 | 17/17 |
| DotGram.Benchmarks.MaterializationCost.WithCaptures | ParseUrl | whole | immediate | read again | 1 | 0 | 2 | 0 | 1/1 |
| DotGram.Benchmarks.Nesting | ParseExpr | whole | immediate | none | 0 | 0 | 0 | 0 | 0/0 |
| DotGram.Benchmarks.Numbers | ParseSum | whole | immediate | none | 0 | 0 | 0 | 0 | 3/3 |
| DotGram.Benchmarks.Possession.Open | ParseDoc | whole | immediate | none | 0 | 0 | 0 | 0 | 0/0 |
| DotGram.Benchmarks.TinyScalar | ParseDepth | whole | immediate | none | 0 | 0 | 0 | 0 | 3/3 |
| DotGram.Benchmarks.Urls | ParseUrl | whole | immediate | read again | 2 | 0 | 3 | 0 | 1/1 |
| DotGram.Examples.Expressions.ArithmeticTree | Read | whole | immediate | none | 0 | 0 | 0 | 0 | 22/22 |
| DotGram.Examples.Expressions.Calculator | EvaluateInt | whole | immediate | read again | 2 | 0 | 3 | 0 | 17/17 |
| DotGram.Examples.Expressions.Calculator | EvaluateDecimal | whole | immediate | read again | 2 | 0 | 5 | 0 | 17/17 |
| DotGram.Examples.Expressions.Calculator | BuildTree | whole | immediate | read again | 2 | 0 | 5 | 0 | 17/17 |
| DotGram.Examples.Expressions.ClampedExample | Read | whole | immediate | none | 0 | 0 | 0 | 0 | 14/14 |
| DotGram.Examples.Expressions.LocaleNumber | ParseNumber | whole | immediate | read again | 1 | 0 | 1 | 0 | 2/2 |
| DotGram.Examples.Expressions.LocaleNumber | ParseEuropeanNumber | whole | immediate | read again | 1 | 0 | 1 | 0 | 2/2 |
| DotGram.Examples.Feeds.FeedReader | ParseFeed | whole | immediate | read again | 5 | 0 | 5 | 0 | 5/5 |
| DotGram.Examples.Feeds.RecoveringFeedReader | ParseFeed | whole | immediate | none | 0 | 0 | 0 | 0 | 6/6 |
| DotGram.Examples.Feeds.StockCountReader | ParseCount | whole | immediate | none | 0 | 0 | 0 | 0 | 5/5 |
| DotGram.Examples.Feeds.StockCountReader | ParseCount | buffered | immediate | none | 0 | 0 | 0 | 0 | 5/5 |
| DotGram.Examples.Formats.Config | ParseFile | whole | immediate | read again | 3 | 0 | 3 | 0 | 5/5 |
| DotGram.Examples.Formats.Config | ParseKeySpan | whole | immediate | none | 0 | 0 | 0 | 0 | 1/1 |
| DotGram.Examples.Formats.Config.Located | ParseFile | whole | immediate | read again | 3 | 0 | 3 | 0 | 5/5 |
| DotGram.Examples.Formats.Config.Located | ParseKeySpan | whole | immediate | none | 0 | 0 | 0 | 0 | 1/1 |
| DotGram.Examples.Formats.FileNames | ParseRoute, ParseSegment | whole | immediate | read again | 2 | 0 | 2 | 0 | 4/4 |
| DotGram.Examples.Formats.FixParser | ParseMessage | whole | immediate | none | 0 | 0 | 0 | 0 | 9/9 |
| DotGram.Examples.Formats.FixedWidth | ParseFeed | whole | immediate | none | 0 | 0 | 0 | 0 | 22/22 |
| DotGram.Examples.Formats.HttpParser | ParseHeaders | whole | immediate | read again | 5 | 0 | 6 | 0 | 10/10 |
| DotGram.Examples.Formats.IniParser | ParseIni | whole | immediate | read again | 7 | 0 | 11 | 0 | 14/14 |
| DotGram.Examples.Formats.JsonParser | ParseJson | whole | immediate | read again | 9 | 0 | 11 | 0 | 30/30 |
| DotGram.Examples.Formats.MarkdownParser | ParseDoc | whole | immediate | read again | 9 | 0 | 8 | 0 | 24/24 |
| DotGram.Examples.Formats.MetricsLine | ParseLine | whole | immediate | read again | 5 | 0 | 3 | 0 | 11/11 |
| DotGram.Examples.Formats.Netstrings | ParseStream | whole | immediate | read again | 2 | 0 | 1 | 0 | 3/3 |
| DotGram.Examples.Formats.XmlParser | ParseXml | whole | immediate | replay | 7 | 6 | 0 | 0 | 21/21 |
| DotGram.Examples.Formats.YamlLite | ParseDoc | whole | immediate | read again | 6 | 0 | 7 | 0 | 11/11 |
| DotGram.Examples.Languages.Filter | ParseFilter | whole | immediate | replay | 9 | 6 | 0 | 0 | 33/33 |
| DotGram.Examples.Languages.FilterFile | ParseFilter | whole | immediate | none | 0 | 0 | 0 | 0 | 4/4 |
| DotGram.Examples.Languages.Filters | ParseFilter | whole | immediate | replay | 2 | 1 | 0 | 0 | 8/8 |
| DotGram.Examples.Languages.GramGrammar | ParseFile | whole | immediate | replay | 35 | 28 | 0 | 0 | 108/108 |
| DotGram.Examples.Languages.Lexemes | ParseQuoted | whole | immediate | none | 0 | 0 | 0 | 0 | 0/0 |
| DotGram.Examples.Languages.Selectors | ParseSelector | whole | immediate | read again | 6 | 0 | 1 | 0 | 13/13 |
| DotGram.Examples.Languages.SettingsFile | ParseSettings | whole | immediate | none | 0 | 0 | 0 | 0 | 3/3 |
| DotGram.Examples.Languages.SqlDialect | ParseOld, ParseNew, ParseAny | whole | immediate | none | 0 | 0 | 0 | 0 | 3/3 |
| DotGram.Examples.Languages.SqlReadOnly | ParseQuery | whole | immediate | none | 0 | 0 | 0 | 0 | 0/0 |
| DotGram.Examples.Languages.TokenizedQuery | ParseQuery | whole | immediate | none | 0 | 0 | 0 | 0 | 12/12 |
| DotGram.Tests.Calculators.DecimalCalculator | Evaluate | whole | immediate | none | 0 | 0 | 0 | 0 | 18/18 |
| DotGram.Tests.Calculators.OneRuleParser | Read | whole | immediate | read again | 1 | 0 | 1 | 0 | 15/15 |
| DotGram.Tests.Calculators.StrengthCalculator | Evaluate | whole | immediate | read again | 1 | 0 | 1 | 0 | 15/15 |
| DotGram.Tests.Calculators.TwoCalculators | EvaluateInt | whole | immediate | none | 0 | 0 | 0 | 0 | 17/17 |
| DotGram.Tests.Calculators.TwoCalculators | EvaluateDouble | whole | immediate | none | 0 | 0 | 0 | 0 | 17/17 |
| DotGram.Tests.Extents | ParseExtent | whole | immediate | none | 0 | 0 | 0 | 0 | 1/1 |
| DotGram.Web.Rfc3339 | ParseTimestamp, ParseFullDate, ParseFullTime | whole | immediate | none | 0 | 0 | 0 | 0 | 5/5 |
| DotGram.Web.Rfc5322 | ParseAddressList, ParseMailboxList, ParseMailbox and 1 more | whole | immediate | read again | 17 | 0 | 27 | 0 | 47/47 |
| DotGram.Web.Rfc5322 | ParseStrictAddrSpec | whole | immediate | read again | 4 | 0 | 15 | 0 | 9/9 |
| DotGram.Web.Rfc5322 | ParseStrictMailbox | whole | immediate | read again | 6 | 0 | 17 | 0 | 15/15 |
| DotGram.Web.Rfc5322 | ParseStrictMailboxList | whole | immediate | read again | 8 | 0 | 19 | 0 | 21/21 |
| DotGram.Web.Rfc5322 | ParseStrictAddressList | whole | immediate | read again | 13 | 0 | 24 | 0 | 37/37 |
| DotGram.Web.Rfc5646 | ParseTag | whole | immediate | read again | 9 | 0 | 6 | 0 | 22/22 |
| DotGram.Web.Rfc6570 | ParseTemplate | whole | immediate | none | 0 | 0 | 0 | 0 | 10/10 |
| DotGram.Web.Rfc6901 | ParsePointer | whole | immediate | none | 0 | 0 | 0 | 0 | 3/3 |
| DotGram.Web.Rfc6901 | ParseFragment | whole | immediate | none | 0 | 0 | 0 | 0 | 1/1 |
| DotGram.Web.Rfc8259 | ParseJson | whole | immediate | none | 0 | 0 | 0 | 0 | 29/29 |
| DotGram.Web.Rfc9110 | ParseContentType | whole | immediate | read again | 4 | 0 | 6 | 0 | 7/7 |
| DotGram.Web.Rfc9651 | ParseItem | whole | immediate | read again | 7 | 0 | 9 | 0 | 22/22 |
| DotGram.Web.Rfc9651 | ParseList | whole | immediate | read again | 11 | 0 | 13 | 0 | 34/34 |
| DotGram.Web.Rfc9651 | ParseDictionary | whole | immediate | read again | 12 | 0 | 14 | 0 | 37/37 |

## Where the second gate's ways are opened

Each place a rule's own reading opens a way, by its shape — a choice over characters, a run of
one character, the turns of a repetition — and why the way could not be left out. **Places**
counts each once per grammar; **captured** is how many of them capture inside the turn, and
**sealed** how many are in a rule every call of which is inside an atomic group or a lookahead,
or which nothing calls, so that no caller asks it again.

| Shape | Why the way stays | Grammars | Rules | Places | Captured | Sealed | For example |
| --- | --- | ---: | ---: | ---: | ---: | ---: | --- |
| choice | alternatives begin alike | 10 | 16 | 23 | 0 | 5 | NoCaptures.Host: `(IPv4 \| RegName)` |
| turns | a turn led by what may read nothing | 5 | 10 | 22 | 5 | 2 | IniParser.Entries: `(item0: Entry \| Blank)*` |
| choice | alternatives begin apart | 1 | 6 | 19 | 0 | 0 | Rfc5322.Ctext: `(['!'..'\'' \| '*'..'[' \| ']'..'~'] \| Never)` |
| run | what follows begins alike | 10 | 18 | 18 | 0 | 0 | Calculator.Spacing: `Whitespace+` |
| choice | every alternative led by what may read nothing | 4 | 7 | 15 | 0 | 1 | IniParser.Entries: `(item0: Entry \| Blank)` |
| optional | what follows begins alike | 8 | 8 | 12 | 8 | 3 | NoCaptures.Url: `(UserInfo & '@')?` |
| choice | an alternative that may read nothing | 5 | 8 | 9 | 0 | 0 | HttpParser.Field: `(eol \| ?=eof)` |
| turns | the seam leads every alternative of the turn | 4 | 4 | 6 | 6 | 0 | Climbing.Expr: `(trivia & '+' & trivia & r: Expr => (l + r) \| trivia & '-' & trivia & …` |
| turns | what follows begins alike | 3 | 4 | 5 | 4 | 1 | JsonParser.Body: `(Plain \| Escape)*` |
| turns | follow unknown | 1 | 1 | 5 | 0 | 0 | Rfc5322.Cfws: `(Fws? & Comment)+` |
| choice | literals, a shorter one wanted | 4 | 4 | 4 | 0 | 0 | HttpParser.eol: `("\r\n" \| '\r')` |
| choice | the seam leads every alternative | 1 | 1 | 3 | 0 | 0 | Calculator.Expr: `(trivia & '+' & trivia & right: Expr_With1 => (left + right) \| trivia …` |
| optional | a turn led by what may read nothing | 1 | 2 | 2 | 0 | 0 | Rfc5322.AngleAddr: `ObsRoute?` |
| choice | literals, follow unknown | 1 | 1 | 1 | 0 | 0 | FeedReader.eol: `("\r\n" \| '\r')` |
| counted | what follows begins alike | 1 | 1 | 1 | 1 | 0 | Rfc5646.LangTag: `extended: ExtLang{0,3}` |

## DotGram.Benchmarks.CallCost.Called

- machine ParseStart [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 1/1
- memo ParseStart [whole]: remembered 0; not 1: Letter (characters)

## DotGram.Benchmarks.CallCost.Valued

- machine ParseStart [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 5/5
- memo ParseStart [whole]: remembered 0; not 1: Letter (characters)

## DotGram.Benchmarks.Climbing

- machine Climbed [whole]: carrier: immediate; gate: read again; building: 1; replayed: 0; read again: 1; refused: 0; points: 13/13
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

- machine ParseUrl [whole]: carrier: immediate; gate: read again; building: 1; replayed: 0; read again: 2; refused: 0; points: 1/1
- again Host: opens a way
- open Host: choice; alternatives begin alike; open; (IPv4 | RegName)
- again Url: opens a way
- open Url: optional; what follows begins alike; entry; (UserInfo & '@')?

## DotGram.Benchmarks.MaterializationCost.SpanCaptures

- machine ParseUrl [whole]: carrier: immediate; gate: replay; building: 7; replayed: 1; read again: 0; refused: 0; points: 17/17
- replay UserInfo: Follows in Url [turn], then '@'

## DotGram.Benchmarks.MaterializationCost.WithCaptures

- machine ParseUrl [whole]: carrier: immediate; gate: read again; building: 1; replayed: 0; read again: 2; refused: 0; points: 1/1
- again Host: opens a way
- open Host: choice; alternatives begin alike; open; (IPv4 | RegName)
- again Url: opens a way
- open Url: optional, captured; what follows begins alike; entry; (user: UserInfo & '@')?

## DotGram.Benchmarks.Nesting

- machine ParseExpr [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 0/0
- memo ParseExpr [whole]: remembered 0; not 1: Expr (characters)

## DotGram.Benchmarks.Numbers

- machine ParseSum [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 3/3

## DotGram.Benchmarks.Possession.Open

- machine ParseDoc [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 0/0

## DotGram.Benchmarks.TapeSql

- memo ParseSelect, ParseQuery, ParseSearchCondition, ParseValueExpression [whole]: remembered 4: QueryExpression, ValueExpression, SearchCondition, TableReference; not 0

## DotGram.Benchmarks.TapeSqlStandard

- memo ParseExpression, ParseDataType, ParseSearchCondition [whole]: remembered 19: ValueExpression, Disjunction, Subquery, QueryExpression, QueryExpressionBody, TableReference, TableFactor, DatetimeValueExpression, CommonValueExpression, CommonValueExpressionOrRow, DataType, JSONSubscript, JSONPathWff, JSONPathPredicate, RowPattern, JSONTableColumnsClause, JSONTablePlan, PartitionedJoin, GroupingElement; not 0
- memo ParseSql, ParseStatement [whole]: remembered 20: QueryExpression, ValueNode, Disjunction, PeriodConstructor, DatetimeValueExpression, CommonValueExpression, CommonValueExpressionOrRow, DataType, JSONSubscript, JSONPathWff, JSONPathPredicate, RowPattern, QueryExpressionBody, TableReference, TableFactor, JSONTableColumnsClause, JSONTablePlan, PartitionedJoin, GroupingElement, SQLExecutableStatement; not 0

## DotGram.Benchmarks.TinyScalar

- machine ParseDepth [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 3/3
- memo ParseDepth [whole]: remembered 0; not 1: Depth (characters)

## DotGram.Benchmarks.Urls

- machine ParseUrl [whole]: carrier: immediate; gate: read again; building: 2; replayed: 0; read again: 3; refused: 0; points: 1/1
- again Authority: opens a way
- open Authority: optional, captured; what follows begins alike; open; (user: UserInfo & '@')?
- again Host: opens a way
- open Host: choice; alternatives begin alike; open; (IPv4 | RegName)
- again Url: through Authority

## DotGram.Examples.Expressions.ArithmeticTree

- machine Read [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 22/22
- memo Read [whole]: remembered 0; not 2: Sum (characters), Unary (characters)

## DotGram.Examples.Expressions.Calculator

- machine EvaluateInt [whole]: carrier: immediate; gate: read again; building: 2; replayed: 0; read again: 3; refused: 0; points: 17/17
- machine EvaluateDecimal [whole]: carrier: immediate; gate: read again; building: 2; replayed: 0; read again: 5; refused: 0; points: 17/17
- machine BuildTree [whole]: carrier: immediate; gate: read again; building: 2; replayed: 0; read again: 5; refused: 0; points: 17/17
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

- machine ParseNumber [whole]: carrier: immediate; gate: read again; building: 1; replayed: 0; read again: 1; refused: 0; points: 2/2
- machine ParseEuropeanNumber [whole]: carrier: immediate; gate: read again; building: 1; replayed: 0; read again: 1; refused: 0; points: 2/2
- again Number: opens a way
- open Number: choice; alternatives begin alike; entry; (whole: Digit+ & Point & frac: Digit+ => (Whole(whole) + Fraction(frac…
- again Number: opens a way
- open Number: choice; alternatives begin alike; entry; (whole: Digit+ & Comma & frac: Digit+ => (Whole(whole) + Fraction(frac…

## DotGram.Examples.Feeds.FeedReader

- machine ParseFeed [whole]: carrier: immediate; gate: read again; building: 5; replayed: 0; read again: 5; refused: 0; points: 5/5
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

- machine ParseFile [whole]: carrier: immediate; gate: read again; building: 3; replayed: 0; read again: 3; refused: 0; points: 5/5
- machine ParseKeySpan [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 1/1
- again Entry: through Value
- again File: through Entry
- again Value: opens a way
- open Value: run; what follows begins alike; open; [^ '\n' | '\r']*

## DotGram.Examples.Formats.Config.Located

- machine ParseFile [whole]: carrier: immediate; gate: read again; building: 3; replayed: 0; read again: 3; refused: 0; points: 5/5
- machine ParseKeySpan [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 1/1
- again Entry: through Value
- again File: through Entry
- again Value: opens a way
- open Value: run; what follows begins alike; open; [^ '\n' | '\r']*

## DotGram.Examples.Formats.FileNames

- machine ParseRoute, ParseSegment [whole]: carrier: immediate; gate: read again; building: 2; replayed: 0; read again: 2; refused: 0; points: 4/4
- again Route: through Segment
- again Segment: opens a way
- open Segment: run; what follows begins alike; open; [IsAllowed]+

## DotGram.Examples.Formats.FixParser

- machine ParseMessage [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 9/9

## DotGram.Examples.Formats.FixedWidth

- machine ParseFeed [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 22/22

## DotGram.Examples.Formats.HttpParser

- machine ParseHeaders [whole]: carrier: immediate; gate: read again; building: 5; replayed: 0; read again: 6; refused: 0; points: 10/10
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

- machine ParseIni [whole]: carrier: immediate; gate: read again; building: 7; replayed: 0; read again: 11; refused: 0; points: 14/14
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

- machine ParseJson [whole]: carrier: immediate; gate: read again; building: 9; replayed: 0; read again: 11; refused: 0; points: 30/30
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

- machine ParseDoc [whole]: carrier: immediate; gate: read again; building: 9; replayed: 0; read again: 8; refused: 0; points: 24/24
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

- machine ParseLine [whole]: carrier: immediate; gate: read again; building: 5; replayed: 0; read again: 3; refused: 0; points: 11/11
- again Line: through Reading
- again Reading: through Value
- again Value: opens a way
- open Value: choice; alternatives begin alike; open; (?=Digits & trivia & '.' & trivia & d: Decimal => (d) | n: Long => (n)…

## DotGram.Examples.Formats.Netstrings

- machine ParseStream [whole]: carrier: immediate; gate: read again; building: 2; replayed: 0; read again: 1; refused: 0; points: 3/3
- again Stream: opens a way
- open Stream: turns, captured; what follows begins alike; entry; item0: Frame*

## DotGram.Examples.Formats.XmlParser

- machine ParseXml [whole]: carrier: immediate; gate: replay; building: 7; replayed: 6; read again: 0; refused: 0; points: 21/21
- memo ParseXml [whole]: remembered 0; not 1: Element (characters)
- replay Attribute: Follows in Element [choice], then '>'
- replay Content: Follows in Element [choice], then "</"
- replay Name: Follows in Element [choice], then '>'
- replay Element: under Content
- replay Text: under Content
- replay Value: under Attribute

## DotGram.Examples.Formats.YamlLite

- machine ParseDoc [whole]: carrier: immediate; gate: read again; building: 6; replayed: 0; read again: 7; refused: 0; points: 11/11
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

- machine ParseFilter [whole]: carrier: immediate; gate: replay; building: 9; replayed: 6; read again: 0; refused: 0; points: 33/33
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

- machine ParseFilter [whole]: carrier: immediate; gate: replay; building: 2; replayed: 1; read again: 0; refused: 0; points: 8/8
- memo ParseFilter [whole]: remembered 0; not 1: Test (characters)
- replay Test: Follows in Test [choice], then ')'

## DotGram.Examples.Languages.GramGrammar

- machine ParseFile [whole]: carrier: immediate; gate: replay; building: 35; replayed: 28; read again: 0; refused: 0; points: 108/108
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

- machine ParseQuoted [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 0/0

## DotGram.Examples.Languages.Scoped

- memo ParseProgram [whole]: remembered 0; not 1: Expr (characters)

## DotGram.Examples.Languages.Selectors

- machine ParseSelector [whole]: carrier: immediate; gate: read again; building: 6; replayed: 0; read again: 1; refused: 0; points: 13/13
- again Selector: opens a way
- open Selector: choice; alternatives begin alike; entry; (s: Applied => (s) | s: Root => (s))

## DotGram.Examples.Languages.SettingsFile

- machine ParseSettings [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 3/3

## DotGram.Examples.Languages.SqlDialect

- machine ParseOld, ParseNew, ParseAny [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 3/3

## DotGram.Examples.Languages.SqlReadOnly

- machine ParseQuery [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 0/0

## DotGram.Examples.Languages.TokenizedQuery

- machine ParseQuery [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 12/12

## DotGram.ExpressionLanguage.ExpressionParser

- memo ParseLambda, ParseHole, ParseBody [whole]: remembered 0; not 11: Type (context), Body (context), Block (context), Statement (context), IfValue (context), Assignment (context), Conditional (context), Coalesce (context), Binary (context), Unary (context), Bindings (context)

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

- machine Read [whole]: carrier: immediate; gate: read again; building: 1; replayed: 0; read again: 1; refused: 0; points: 15/15
- memo Read [whole]: remembered 0; not 1: Expr (characters)
- again Expr: opens a way
- open Expr: turns, captured; the seam leads every alternative of the turn; open; (trivia & '+' & trivia & right: Expr => (new Add(left, right)) | trivi…

## DotGram.Tests.Calculators.StrengthCalculator

- machine Evaluate [whole]: carrier: immediate; gate: read again; building: 1; replayed: 0; read again: 1; refused: 0; points: 15/15
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

- machine ParseAddressList, ParseMailboxList, ParseMailbox, ParseAddrSpec [whole]: carrier: immediate; gate: read again; building: 17; replayed: 0; read again: 27; refused: 0; points: 47/47
- machine ParseStrictAddrSpec [whole]: carrier: immediate; gate: read again; building: 4; replayed: 0; read again: 15; refused: 0; points: 9/9
- machine ParseStrictMailbox [whole]: carrier: immediate; gate: read again; building: 6; replayed: 0; read again: 17; refused: 0; points: 15/15
- machine ParseStrictMailboxList [whole]: carrier: immediate; gate: read again; building: 8; replayed: 0; read again: 19; refused: 0; points: 21/21
- machine ParseStrictAddressList [whole]: carrier: immediate; gate: read again; building: 13; replayed: 0; read again: 24; refused: 0; points: 37/37
- memo ParseAddressList, ParseMailboxList, ParseMailbox, ParseAddrSpec [whole]: remembered 0; not 1: Comment (characters)
- memo ParseStrictAddrSpec [whole]: remembered 0; not 1: Comment (characters)
- memo ParseStrictMailbox [whole]: remembered 0; not 1: Comment (characters)
- memo ParseStrictMailboxList [whole]: remembered 0; not 1: Comment (characters)
- memo ParseStrictAddressList [whole]: remembered 0; not 1: Comment (characters)
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
- open Cfws: choice; alternatives begin alike; open; ({ (Fws? & Comment)+ & Fws? } | Fws)
- open Cfws: turns; follow unknown; open; (Fws? & Comment)+
- again Cfws: opens a way
- open Cfws: choice; alternatives begin alike; open; ({ (CurrentFws? & Comment_With1)+ & CurrentFws? } | CurrentFws)
- open Cfws: turns; follow unknown; open; (CurrentFws? & Comment_With1)+
- again Cfws: opens a way
- open Cfws: choice; alternatives begin alike; open; ({ (CurrentFws? & Comment_With2)+ & CurrentFws? } | CurrentFws)
- open Cfws: turns; follow unknown; open; (CurrentFws? & Comment_With2)+
- again Cfws: opens a way
- open Cfws: choice; alternatives begin alike; open; ({ (CurrentFws? & Comment_With3)+ & CurrentFws? } | CurrentFws)
- open Cfws: turns; follow unknown; open; (CurrentFws? & Comment_With3)+
- again Cfws: opens a way
- open Cfws: choice; alternatives begin alike; open; ({ (CurrentFws? & Comment_With4)+ & CurrentFws? } | CurrentFws)
- open Cfws: turns; follow unknown; open; (CurrentFws? & Comment_With4)+
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
- again Domain: opens a way
- open Domain: choice; alternatives begin alike; open; (literal: DomainLiteral => (literal) | { first: Atom & rest: DotAtom* …
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
- again Group: through Cfws
- again Mailbox: opens a way
- open Mailbox: choice; alternatives begin alike; open; (name: Phrase? & address: AngleAddr => (new EmailAddress.Mailbox(Rfc53…
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
- again ObsRoute: through Cfws
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

- machine ParseTag [whole]: carrier: immediate; gate: read again; building: 9; replayed: 0; read again: 6; refused: 0; points: 22/22
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

## DotGram.Web.Rfc6570

- machine ParseTemplate [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 10/10

## DotGram.Web.Rfc6901

- machine ParsePointer [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 3/3
- machine ParseFragment [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 1/1

## DotGram.Web.Rfc8259

- machine ParseJson [whole]: carrier: immediate; gate: none; building: 0; replayed: 0; read again: 0; refused: 0; points: 29/29
- memo ParseJson [whole]: remembered 0; not 1: Value (characters)

## DotGram.Web.Rfc9110

- machine ParseContentType [whole]: carrier: immediate; gate: read again; building: 4; replayed: 0; read again: 6; refused: 0; points: 7/7
- again ContentTypeField: through Ows
- again Media: through Parameters
- again Ows: opens a way
- open Ows: run; what follows begins alike; open; ['\t' | ' ']*
- again ParameterSlot: through Ows
- again ParameterText: opens a way
- open ParameterText: optional; what follows begins alike; open; (Token & '=' & (Token | QuotedString))?
- again Parameters: opens a way
- open Parameters: turns, captured; a turn led by what may read nothing; open; slots: ParameterSlot*

## DotGram.Web.Rfc9651

- machine ParseItem [whole]: carrier: immediate; gate: read again; building: 7; replayed: 0; read again: 9; refused: 0; points: 22/22
- machine ParseList [whole]: carrier: immediate; gate: read again; building: 11; replayed: 0; read again: 13; refused: 0; points: 34/34
- machine ParseDictionary [whole]: carrier: immediate; gate: read again; building: 12; replayed: 0; read again: 14; refused: 0; points: 37/37
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
