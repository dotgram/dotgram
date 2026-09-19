# What the stand does not read: the published forms with no row

**Taken on the evening of 2026-09-19, from the stand's tree `2fcd156d` and the net10.0 builds of the libraries at `5d1f26d8` (the surface), and it goes out of date by construction: as soon as expr's bool `Try...(out)` forms land on main the publications of every parser double, and each row added or removed after this date changes a line of it. Regenerate before believing a line older than the day it was taken.** The architect's order of additions (2026-09-19): (1) the positional `(string, at)` form of SQL:2023 and T-SQL and one window row; (2) `Scan` for SQL:2023 and EL; (3) `FixMessages` stream, `TextReader` and lazy reading; (4) the Web lists as `linearity` series; (5) one span row per library; (6) `StreamingFeedReader`. The throwing `Parse` next to the `Try` and the twenty-one unmeasured examples are not wanted. The architect's rule from window 61: a form has a row before it is optimized.
The log form of FIX had none, and a way was removed from it that nobody had measured. This is the list of the holes, by
library, by input (a string, a span, bytes in memory, a stream, a `TextReader`) and by kind of publication (whole, lazy,
search, positional, window). The publications are read off the built assemblies (`DotGram.Finance`, `.Web`, `.Sql`,
`.ExpressionLanguage`, `.Examples`, net10.0 builds of 5d1f26d8: the bool `Try...(out)` forms of expr's dba87a9e are in none of
them, and are covered only by the five `.bool` rows); what a row reads is read off `Stand*.cs`. "Row" is a row of the
paired stand (`--stand-paired`) or of the plain one (`--stand`); most of what was added today is in the paired stand only,
which has no baseline, so a row that exists only there is marked *paired*.

## FIX

| publication | string | span | bytes in memory | stream | `TextReader` |
| --- | --- | --- | --- | --- | --- |
| `FixParser.Parse` (whole, an array) | row | **none** | row | row (yield form) | row, *paired* (`.yield-reader`) |
| `FixParser.ParseLog` | row, *paired* | **none** | row, *paired* | row, *paired* (yield form) | **none** |
| whole-stream (`FixGrammar.ParseFields`, internal, by reflection) | | | | row, *paired* (`.whole-stream`) | row, *paired* (`.whole-reader`) |
| `FixMessages.Parse` | one row (the order, by string) | **none** | | **none** | **none** |
| `FixMessages.TryParse` | called once in a row's check, not timed | **none** | | **none** | **none** |
| `FixMessages.ParseLog` | **none** | | | | |
| `FixMessages.ReadMessages` (lazy) | | | | **none** | **none** |

The span forms of `FixParser` convert to a string first (`ParseLog(ReadOnlySpan<char>)` is `ParseLog(input.ToString())`), so their
cost is the conversion, and it has no row. Of `FixMessages`' 21 parse methods one is timed (`Parse` of a strict wire, by string, plus `Build`, which is not a parse); `TryParse(string)` is called once as the row's agreement check and is not timed.

## Web (34 publications over 22 types; every one takes a string, and none has a span, positional, window or stream form)

Rows: `UriReference.TryParse`, `JsonValue.TryParse`, `Timestamp.TryParse`, `AddrSpec.TryParse` and `TryParseStrict`,
`MediaType.TryParse`, `StructuredField.TryParseItem`, `TryParseList`, `TryParseDictionary`, `SetCookie.TryParse`,
`JsonPointer.TryParse`, `LanguageTag.TryParse`: 12 of 34, all the `Try` form. The `Parse` (throwing) form of none.

**No row at all**: `ContentDisposition`, `CookieDate`, `CookiePair.ParseField`, `EmailAddress` (the four list forms: `ParseList`,
`ParseStrictList`, `ParseMailboxList`, `ParseStrictMailboxList`) and `EmailAddress.Mailbox` (two), `ForwardedElement.ParseField`,
`ForwardedNode`, `FullDate`, `FullTime`, `JsonPatch` (`Parse` and `Read(JsonValue)`), `JsonPointer.TryParseFragment`,
`MediaRange.ParseAccept`, `UriReference.TryParseUri`, `UriTemplate`, `WebLink.ParseField`: 22 publications. Five of them are
lists or fields (`EmailAddress` lists, `MediaRange`, `ForwardedElement`, `WebLink`, `CookiePair`), the shape whose curve
matters, and `linearity` has a series only for the media type's parameters, the structured list and the URL path.

## SQL

| publication | `string` | `(string, at)` | `(string, at, length)` | `Scan(ReadOnlySpan<char>, ...)` |
| --- | --- | --- | --- | --- |
| `SqlStandardParser`, 42 rules | 6 rules have a row (`Literal`, `ColumnReference`, `ValueExpression`, `SearchCondition`, `QueryExpression`, `SQLSchemaStatement`); **36 none** | **none** | **none** | **none** |
| `Sql92Parser`, 4 rules | **none** (generation only, by the gate) | **none** | **none** | **none** |
| `TransactSqlParser`, 31 rules | 1 rule: `Statement`; `SearchCondition` only in `linearity`; **29 none** | `Statement`, the script rows, *paired* | **none** | **none** |
| `TransactSqlParser.Located`, 31 rules | `Statement` (the plain stand's located reading); **30 none** | **none** | **none** | **none** |

The `Parse` (throwing) form of none. `Scan`, the search publication (`find`), has no row for any of the five parsers that publish
it (Sql92, SqlStandard, T-SQL, Located, ExpressionLanguage). The bool forms: two rows (`sql/refused-late.bool`,
`sql/select20.bool`), *paired*. The `Syntax.Parse(string, string)` helper: none.

## The expression language

Rows read the grammar's own entry, `TryParseLambda(string, State)`, on the tape and on the immediate carrier. **The public
`ExpressionParser.Parse(string)`, `Parse(string, Assembly)`, `TryParse(string)`, `TryParse(string, Assembly)` have no row**, nor
does `Scan`; the bool form has three rows (*paired*).

## The examples (36 classes, 97 publications)

Rows: `StockCountReader` (a string, `TextReader`, `TextReader` with a 64-character buffer), `RecoveringFeedReader` (*paired*),
`Config` (*paired*, the benchmarks' own). Read only by the rough tools, never by a stand row: `MetricsLine`, `FilterFile`, `Filters`,
`Lexemes`, `SettingsFile`, `ArithmeticTree`, `ClampedExample`, `Calculator`, `JsonParser`, `GramGrammar`, `Levels`, `FeedReader`
(`--rough-examples`, `linearity`). **No measurement of any kind**: `LocaleNumber`, `LoggingFeedReader`, `StreamingFeedReader` (a lazy
form over a `TextReader`), `FileNames`, `FixedWidth`, `HttpParser`, `IniParser`, `Links`, `MarkdownParser`, `Netstrings`,
`TypedCsv`, `XmlParser`, `YamlLite`, `Filter`, `Scoped`, `Selectors`, `SqlDialect`, `SqlReadOnly`, `TokenizedQuery`,
`Config.Located`, the examples' own `FixParser`: 21 classes.

## By input and by kind, across the libraries

- **A span**: no row anywhere: FIX `Parse` and `ParseLog` (2), `FixMessages` (4), the five `Scan` forms.
- **Bytes in memory**: covered for FIX (`Parse`, `ParseLog`); no other library publishes it.
- **A stream**: FIX yield forms covered; `FixMessages` (5 methods) and `ReadMessages` none.
- **A `TextReader`**: `FixParser.Parse` and the stock count covered (paired for the first); `FixParser.ParseLog(TextReader)`,
  `FixMessages` (5), `StreamingFeedReader` none.
- **Lazy** (`yield`): the FIX forms covered; `ReadMessages` and `StreamingFeedReader` none.
- **Search** (`Scan`): none, in five parsers.
- **Positional** `(string, at)`: one rule of T-SQL (paired); 40 of SQL:2023's 42 rules, all 4 of Sql92's and 30 more of T-SQL's 31 publish it with no row.
- **Window** `(string, at, length)`: none (40 SQL:2023 rules, 4 Sql92, 31 T-SQL and 31 Located publish it).

## Which of these matter first

Not all at once. The forms about to change: the positional and window forms (lazy tokens, expr's next commits: the script rows
are the only reading of them), the `Scan` search (five parsers publish one and every reader of it is invisible),
the span forms (`FixMessages` in particular, and the FIX ones that copy), the lists and fields of the Web (loops, so a curve), the
throwing `Parse` next to the `Try`, and `FixMessages`' stream and reader forms. The 36 SQL:2023 and 29 T-SQL rules with no row
are rules of the grammar rather than forms of a publication: a change to the reader that touches them is read today only
through the statements the T-SQL rows and the six SQL:2023 rules happen to contain.
