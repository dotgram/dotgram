# Which carrier each grammar is read with, and why

Every grammar of the solution, as the last build with `-p:DotGramReportGeneration=true` compiled
it: the carrier `Auto` took (GRAM5012), and for a grammar kept on the tape, the gate that kept it
and each rule held there. Written by `--carriers` (`benchmarks/DotGram.Benchmarks/Carriers.cs`)
from the reports that build left; run again rather than edited.

**Carrier** is what `Auto` took: `immediate`, `tape`, or the author's own choice. **Gate** is what
kept a grammar on the tape: `replay` — a building rule read where the reading may not stand
(`Replay`) — or `read again` — a rule the reader can be asked again after it answered, which is
asked only where the first gate let everything through. **Direct** is how many of the replayed
rules have a cause of their own; the rest are under one of them. **Points** is how many of the
sites that build — a call whose value is built, a construction — have a point past which what they
read is settled (`Commit`), of how many there are: what building at that point could take off the
tape.

| Grammar | Carrier | Gate | Building | Replayed | Direct | Read again | Points |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: |
| DotGram.Benchmarks.CallCost.Called | immediate | none |  |  |  |  | 1/1 |
| DotGram.Benchmarks.CallCost.Inlined | nothing to choose | none |  |  |  |  | 1/1 |
| DotGram.Benchmarks.CallCost.Valued | immediate | none |  |  |  |  | 5/5 |
| DotGram.Benchmarks.Climbing | tape | read again | 1 | 0 | 0 | 1 | 13/13 |
| DotGram.Benchmarks.Config | immediate | none |  |  |  |  | 7/7 |
| DotGram.Benchmarks.Extents | immediate | none |  |  |  |  | 1/1 |
| DotGram.Benchmarks.Feed | nothing to choose | none |  |  |  |  | 7/7 |
| DotGram.Benchmarks.Flat.Lowered | nothing to choose | none |  |  |  |  | 0/0 |
| DotGram.Benchmarks.Flat.NotLowered | immediate | none |  |  |  |  | 0/0 |
| DotGram.Benchmarks.ImmediateSql | immediate (author) |  |  |  |  |  | 12/242 |
| DotGram.Benchmarks.Levels | tape | read again | 4 | 0 | 0 | 4 | 19/19 |
| DotGram.Benchmarks.MaterializationCost.NoCaptures | tape | read again | 1 | 0 | 0 | 2 | 1/1 |
| DotGram.Benchmarks.MaterializationCost.SpanCaptures | tape | replay | 7 | 1 | 1 | 0 | 17/17 |
| DotGram.Benchmarks.MaterializationCost.WithCaptures | tape | read again | 1 | 0 | 0 | 2 | 1/1 |
| DotGram.Benchmarks.Nesting | nothing to choose | none |  |  |  |  | 0/0 |
| DotGram.Benchmarks.Numbers | tape | read again | 2 | 0 | 0 | 2 | 3/3 |
| DotGram.Benchmarks.Possession.Open | nothing to choose | none |  |  |  |  | 0/0 |
| DotGram.Benchmarks.Possession.Settled | nothing to choose | none |  |  |  |  | 0/0 |
| DotGram.Benchmarks.Settlements | nothing to choose | none |  |  |  |  | 7/7 |
| DotGram.Benchmarks.TinyScalar | immediate | none |  |  |  |  | 3/3 |
| DotGram.Benchmarks.Urls | tape | read again | 2 | 0 | 0 | 3 | 1/1 |
| DotGram.Examples.Expressions.ArithmeticTree | tape | read again | 5 | 0 | 0 | 5 | 22/22 |
| DotGram.Examples.Expressions.Calculator | tape | read again | 6 | 0 | 0 | 8 | 51/51 |
| DotGram.Examples.Expressions.ClampedExample | tape | read again | 3 | 0 | 0 | 3 | 14/14 |
| DotGram.Examples.Expressions.LocaleNumber | tape | read again | 2 | 0 | 0 | 2 | 4/4 |
| DotGram.Examples.Feeds.FeedReader | tape | read again | 5 | 0 | 0 | 5 | 5/5 |
| DotGram.Examples.Feeds.LoggingFeedReader | nothing to choose | none |  |  |  |  | 5/5 |
| DotGram.Examples.Feeds.RecoveringFeedReader | nothing to choose | none |  |  |  |  | 6/6 |
| DotGram.Examples.Feeds.StockCountReader | nothing to choose | none |  |  |  |  | 5/5 |
| DotGram.Examples.Feeds.StreamingFeedReader | nothing to choose | none |  |  |  |  | 9/9 |
| DotGram.Examples.Formats.Config | tape | read again | 3 | 0 | 0 | 3 | 6/6 |
| DotGram.Examples.Formats.Config.Located | tape | read again | 3 | 0 | 0 | 3 | 6/6 |
| DotGram.Examples.Formats.FileNames | tape | read again | 2 | 0 | 0 | 2 | 4/4 |
| DotGram.Examples.Formats.FixParser | immediate | none |  |  |  |  | 9/9 |
| DotGram.Examples.Formats.FixedWidth | immediate | none |  |  |  |  | 22/22 |
| DotGram.Examples.Formats.HttpParser | tape | read again | 5 | 0 | 0 | 6 | 10/10 |
| DotGram.Examples.Formats.IniParser | tape | read again | 7 | 0 | 0 | 11 | 14/14 |
| DotGram.Examples.Formats.JsonParser | tape | read again | 9 | 0 | 0 | 11 | 30/30 |
| DotGram.Examples.Formats.Links | nothing to choose | none |  |  |  |  | 1/1 |
| DotGram.Examples.Formats.MarkdownParser | tape | read again | 9 | 0 | 0 | 10 | 24/24 |
| DotGram.Examples.Formats.MetricsLine | tape | read again | 5 | 0 | 0 | 4 | 11/11 |
| DotGram.Examples.Formats.Netstrings | tape | read again | 2 | 0 | 0 | 1 | 3/3 |
| DotGram.Examples.Formats.TypedCsv | nothing to choose | none |  |  |  |  | 12/12 |
| DotGram.Examples.Formats.XmlParser | tape | replay | 7 | 6 | 3 | 0 | 21/21 |
| DotGram.Examples.Formats.YamlLite | tape | read again | 6 | 0 | 0 | 7 | 11/11 |
| DotGram.Examples.Languages.Filter | tape | replay | 9 | 6 | 2 | 0 | 33/33 |
| DotGram.Examples.Languages.FilterFile | tape | read again | 2 | 0 | 0 | 3 | 4/4 |
| DotGram.Examples.Languages.Filters | tape | replay | 2 | 1 | 1 | 0 | 8/8 |
| DotGram.Examples.Languages.GramGrammar | tape | replay | 35 | 28 | 2 | 0 | 108/108 |
| DotGram.Examples.Languages.Lexemes | nothing to choose | none |  |  |  |  | 0/0 |
| DotGram.Examples.Languages.Scoped | tape | read again | 4 | 0 | 0 | 5 | 13/13 |
| DotGram.Examples.Languages.Selectors | tape | read again | 6 | 0 | 0 | 2 | 15/15 |
| DotGram.Examples.Languages.SettingsFile | tape | read again | 2 | 0 | 0 | 3 | 3/3 |
| DotGram.Examples.Languages.SqlDialect | immediate | none |  |  |  |  | 3/3 |
| DotGram.Examples.Languages.SqlReadOnly | nothing to choose | none |  |  |  |  | 0/0 |
| DotGram.Examples.Languages.TokenizedQuery | immediate | none |  |  |  |  | 12/12 |
| DotGram.ExpressionLanguage.ExpressionParser | tape | replay | 137 | 131 | 20 | 0 | 18/854 |
| DotGram.ExpressionLanguage.ExpressionParser.Immediate | immediate (author) |  |  |  |  |  | 18/854 |
| DotGram.Finance.Fix.FixGrammar | nothing to choose | none |  |  |  |  | 18/18 |
| DotGram.Finance.Fix44.Fix44Grammar | nothing to choose | none |  |  |  |  | 1910/1910 |
| DotGram.Finance.Fix44.FixFieldGrammar | nothing to choose | none |  |  |  |  | 0/0 |
| DotGram.Sql.Standard.Sql92Parser | tape | replay | 48 | 44 | 4 | 0 | 12/242 |
| DotGram.Sql.Standard.SqlStandardParser | tape | replay | 17 | 15 | 2 | 0 | 1284/2641 |
| DotGram.Sql.TransactSql.TransactSqlParser | tape | replay | 178 | 173 | 34 | 0 | 2362/3146 |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | tape | replay | 178 | 173 | 34 | 0 | 2362/3146 |
| DotGram.Tests.Calculators.DecimalCalculator | tape | read again | 5 | 0 | 0 | 5 | 18/18 |
| DotGram.Tests.Calculators.OneRuleParser | tape | read again | 1 | 0 | 0 | 1 | 15/15 |
| DotGram.Tests.Calculators.StrengthCalculator | tape | read again | 1 | 0 | 0 | 1 | 15/15 |
| DotGram.Tests.Calculators.TwoCalculators | tape | read again | 10 | 0 | 0 | 8 | 34/34 |
| DotGram.Tests.Extents | immediate | none |  |  |  |  | 1/1 |
| DotGram.Tests.Generated.UrlGrammar | nothing to choose | none |  |  |  |  | 1/1 |
| DotGram.Web.Rfc3339 | immediate | none |  |  |  |  | 5/5 |
| DotGram.Web.Rfc3986 | tape | read again | 6 | 0 | 0 | 12 | 19/19 |
| DotGram.Web.Rfc5322 | tape | read again | 48 | 0 | 0 | 114 | 129/129 |
| DotGram.Web.Rfc5646 | tape | read again | 9 | 0 | 0 | 6 | 22/22 |
| DotGram.Web.Rfc6265 | tape | read again | 6 | 0 | 0 | 6 | 29/29 |
| DotGram.Web.Rfc6266 | tape | read again | 3 | 0 | 0 | 2 | 5/5 |
| DotGram.Web.Rfc6570 | tape | read again | 5 | 0 | 0 | 2 | 10/10 |
| DotGram.Web.Rfc6901 | immediate | none |  |  |  |  | 4/4 |
| DotGram.Web.Rfc7239 | tape | read again | 7 | 0 | 0 | 9 | 16/16 |
| DotGram.Web.Rfc8259 | immediate | none |  |  |  |  | 29/29 |
| DotGram.Web.Rfc8288 | tape | read again | 5 | 0 | 0 | 7 | 9/9 |
| DotGram.Web.Rfc9110 | tape | read again | 4 | 0 | 0 | 7 | 13/13 |
| DotGram.Web.Rfc9651 | tape | read again | 15 | 0 | 0 | 17 | 44/44 |

## Where the second gate's ways are opened

Each place a rule's own reading opens a way, by its shape — a choice over characters, a run of
one character, the turns of a repetition — and why the way could not be left out. **Places**
counts each once per grammar; **captured** is how many of them capture inside the turn, and
**sealed** how many are in a rule every call of which is inside an atomic group or a lookahead,
or which nothing calls, so that no caller asks it again.

| Shape | Why the way stays | Grammars | Rules | Places | Captured | Sealed | For example |
| --- | --- | ---: | ---: | ---: | ---: | ---: | --- |
| choice | alternatives begin alike | 13 | 22 | 37 | 0 | 8 | NoCaptures.Host: `(IPv4 \| RegName)` |
| turns | a turn led by what may read nothing | 8 | 16 | 34 | 8 | 2 | IniParser.Entries: `(item0: Entry \| Blank)*` |
| run | what follows begins alike | 16 | 27 | 27 | 0 | 0 | Numbers.Number: `['0'..'9']+` |
| optional | what follows begins alike | 12 | 14 | 20 | 7 | 3 | NoCaptures.Url: `(UserInfo & '@')?` |
| choice | alternatives begin apart | 1 | 6 | 19 | 0 | 0 | Rfc5322.Ctext: `(['!'..'\'' \| '*'..'[' \| ']'..'~'] \| Never)` |
| choice | every alternative led by what may read nothing | 4 | 8 | 17 | 0 | 1 | IniParser.Entries: `(item0: Entry \| Blank)` |
| turns | the seam leads every alternative of the turn | 9 | 11 | 15 | 15 | 0 | Climbing.Expr: `(trivia & '+' & trivia & r: Expr => (l + r) \| trivia & '-' & trivia & …` |
| choice | an alternative that may read nothing | 7 | 13 | 14 | 0 | 1 | HttpParser.Field: `(eol \| ?=eof)` |
| turns | what follows begins alike | 10 | 11 | 12 | 6 | 2 | JsonParser.Body: `(Plain \| Escape)*` |
| choice | the seam leads every alternative | 7 | 9 | 11 | 0 | 0 | Climbing.Expr: `(trivia & '+' & trivia & r: Expr => (l + r) \| trivia & '-' & trivia & …` |
| counted | what follows begins alike | 3 | 3 | 11 | 1 | 0 | Rfc3986.IPv6Address: `(H16 & ':'){0,2}` |
| optional | a turn led by what may read nothing | 2 | 5 | 7 | 2 | 0 | Rfc3986.Authority: `(user: UserInfoText & '@')?` |
| choice | literals, a shorter one wanted | 4 | 4 | 4 | 0 | 0 | HttpParser.eol: `("\r\n" \| '\r')` |
| choice | literals, follow unknown | 1 | 1 | 1 | 0 | 0 | FeedReader.eol: `("\r\n" \| '\r')` |
| turns | seam first, what follows begins alike past it | 1 | 1 | 1 | 0 | 1 | Scoped.Program: `(trivia & Let)*` |

## DotGram.Benchmarks.Climbing

- again Expr: opens a way
- open Expr: turns, captured; the seam leads every alternative of the turn; open; (trivia & '+' & trivia & r: Expr => (l + r) | trivia & '-' & trivia & …
- open Expr: choice; the seam leads every alternative; open; (trivia & '+' & trivia & r: Expr => (l + r) | trivia & '-' & trivia & …

## DotGram.Benchmarks.Levels

- again Primary: through Sum
- again Product: opens a way
- open Product: choice; the seam leads every alternative; open; (trivia & '*' & trivia & r: Unary => (l * r) | trivia & '/' & trivia &…
- again Sum: opens a way
- open Sum: choice; the seam leads every alternative; open; (trivia & '+' & trivia & r: Product => (l + r) | trivia & '-' & trivia…
- again Unary: through Primary

## DotGram.Benchmarks.MaterializationCost.NoCaptures

- again Host: opens a way
- open Host: choice; alternatives begin alike; open; (IPv4 | RegName)
- again Url: opens a way
- open Url: optional; what follows begins alike; entry; (UserInfo & '@')?

## DotGram.Benchmarks.MaterializationCost.SpanCaptures

- replay UserInfo: Follows in Url [turn], then '@'

## DotGram.Benchmarks.MaterializationCost.WithCaptures

- again Host: opens a way
- open Host: choice; alternatives begin alike; open; (IPv4 | RegName)
- again Url: opens a way
- open Url: optional, captured; what follows begins alike; entry; (user: UserInfo & '@')?

## DotGram.Benchmarks.Numbers

- again Number: opens a way
- open Number: run; what follows begins alike; open; ['0'..'9']+
- again Sum: through Number

## DotGram.Benchmarks.Urls

- again Authority: opens a way
- open Authority: optional, captured; what follows begins alike; open; (user: UserInfo & '@')?
- again Host: opens a way
- open Host: choice; alternatives begin alike; open; (IPv4 | RegName)
- again Url: through Authority

## DotGram.Examples.Expressions.ArithmeticTree

- again Power: through Unary
- again Primary: through Sum
- again Product: opens a way
- open Product: choice; the seam leads every alternative; open; (trivia & '*' & trivia & right: Unary => (new Mul(left, right)) | triv…
- again Sum: opens a way
- open Sum: choice; the seam leads every alternative; open; (trivia & '+' & trivia & right: Product => (new Add(left, right)) | tr…
- again Unary: through Power

## DotGram.Examples.Expressions.Calculator

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

- again Body: through Sum
- again Sum: opens a way
- open Sum: turns, captured; the seam leads every alternative of the turn; open; trivia & '+' & trivia & right: Term => (System.Linq.Expressions.Expres…
- again Term: through Sum

## DotGram.Examples.Expressions.LocaleNumber

- again Number: opens a way
- open Number: choice; alternatives begin alike; entry; (whole: Digit+ & Point & frac: Digit+ => (Whole(whole) + Fraction(frac…
- again Number: opens a way
- open Number: choice; alternatives begin alike; entry; (whole: Digit+ & Comma & frac: Digit+ => (Whole(whole) + Fraction(frac…

## DotGram.Examples.Feeds.FeedReader

- again Feed: through Header
- again Header: through eol
- again Row: through eol
- again Trailer: through eol
- again eol: opens a way
- open eol: choice; literals, follow unknown; open; ("\r\n" | '\r')

## DotGram.Examples.Formats.Config

- again Entry: through Value
- again File: through Entry
- again Value: opens a way
- open Value: run; what follows begins alike; open; [^ '\n' | '\r']*

## DotGram.Examples.Formats.Config.Located

- again Entry: through Value
- again File: through Entry
- again Value: opens a way
- open Value: run; what follows begins alike; open; [^ '\n' | '\r']*

## DotGram.Examples.Formats.FileNames

- again Route: through Segment
- again Segment: opens a way
- open Segment: run; what follows begins alike; open; [IsAllowed]+

## DotGram.Examples.Formats.HttpParser

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

- again Blank: through eol
- again Block: opens a way
- open Block: choice; alternatives begin alike; open; (block: Heading => (block) | block: Bullets => (block) | block: Code =…
- again Bullet: through eol
- again Bullets: opens a way
- open Bullets: turns, captured; what follows begins alike; open; items: Bullet+
- again Code: opens a way
- open Code: turns, captured; a turn led by what may read nothing; open; lines: CodeLine*
- again CodeLine: through eol
- again Doc: through Block
- again Heading: through eol
- again Paragraph: through eol
- again eol: opens a way
- open eol: choice; literals, a shorter one wanted; open; ("\r\n" | '\r')

## DotGram.Examples.Formats.MetricsLine

- again Line: through Reading
- again Quoted: opens a way
- open Quoted: turns; what follows begins alike; open; ("""" & (?!'"' & any)*)*
- again Reading: through Value
- again Value: opens a way
- open Value: choice; alternatives begin alike; open; (?=Digits & trivia & '.' & trivia & d: Decimal => (d) | n: Long => (n)…

## DotGram.Examples.Formats.Netstrings

- again Stream: opens a way
- open Stream: turns, captured; what follows begins alike; entry; item0: Frame*

## DotGram.Examples.Formats.XmlParser

- replay Attribute: Follows in Element [choice], then '>'
- replay Content: Follows in Element [choice], then "</"
- replay Name: Follows in Element [choice], then '>'
- replay Element: under Content
- replay Text: under Content
- replay Value: under Attribute

## DotGram.Examples.Formats.YamlLite

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

- replay List: Follows in Expr [choice], then ')'
- replay Op: Follows in Expr [choice], then Value
- replay Body: under Text
- replay Number: under Value
- replay Text: under Value
- replay Value: under List

## DotGram.Examples.Languages.FilterFile

- again Filter: through Test
- again Quoted: opens a way
- open Quoted: turns; what follows begins alike; open; ("""" | [^ '"'])*
- again Test: through Quoted

## DotGram.Examples.Languages.Filters

- replay Test: Follows in Test [choice], then ')'

## DotGram.Examples.Languages.GramGrammar

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

## DotGram.Examples.Languages.Scoped

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

- again Applied: opens a way
- open Applied: turns, captured; the seam leads every alternative of the turn; open; trivia & step: Step => (new Step(target, step))*
- again Selector: opens a way
- open Selector: choice; alternatives begin alike; entry; (s: Applied => (s) | s: Root => (s))

## DotGram.Examples.Languages.SettingsFile

- again File: through Setting
- again Quoted: opens a way
- open Quoted: turns; what follows begins alike; open; ("""" | [^ '"'])*
- again Setting: through Quoted

## DotGram.ExpressionLanguage.ExpressionParser

- replay Assignment: Follows in Primary [choice], then ']'
- replay Assignment: Follows in Primary [choice], then ']'
- replay Catch: Follows in Try [choice], then "finally"
- replay Catch: Follows in Try [choice], then "finally"
- replay Conditional: Follows in Conditional [turn], then ':'
- replay Conditional: Follows in Conditional [turn], then ':'
- replay Core: Follows in Primary [choice], then '.'
- replay Core: Follows in Primary [choice], then '.'
- replay Elements: Follows in Primary [turn], then '}'
- replay Elements: Follows in Primary [turn], then '}'
- replay Identifier: Follows in Untyped [lookahead], then "=>"
- replay Identifier: Follows in Untyped [lookahead], then "=>"
- replay Indices: Follows in Assignment [choice], then '='
- replay Indices: Follows in Assignment [choice], then '='
- replay Name: Follows in Assignment [choice], then Indices
- replay Name: Follows in Postfix [choice], then args: Arguments_With1 => (ExpressionParser.Invoked(target, args)) or '…
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
- replay Block: under Catch
- replay Block: under Catch
- replay Body: under Inner
- replay Body: under Inner
- replay Case: under Switch
- replay Case: under Switch
- replay Char: under Primary
- replay Coalesce: under Conditional
- replay Coalesce: under Conditional
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
- replay Local: under Statement
- replay Local: under Statement
- replay NamedType: under Core
- replay NamedType: under Core
- replay Parameter: under Inner
- replay Parameter: under Inner
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

## DotGram.Sql.Standard.Sql92Parser

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

- replay Identifier: Follows in CharacterSetSpecification [turn], then '.'
- replay StartField: Follows in IntervalQualifier [choice], then "TO"i
- replay ActualIdentifier: under Identifier
- replay BooleanLiteral: under GeneralLiteral
- replay DateLiteral: under GeneralLiteral
- replay EndField: under IntervalQualifier
- replay GeneralLiteral: under ?
- replay IntervalLiteral: under GeneralLiteral
- replay IntervalQualifier: under IntervalLiteral
- replay IntroducedStringLiteral: under GeneralLiteral
- replay LocalOrSchemaQualifiedName: under ?
- replay SignedNumericLiteral: under ?
- replay SingleDatetimeField: under IntervalQualifier
- replay TimeLiteral: under GeneralLiteral
- replay TimestampLiteral: under GeneralLiteral

## DotGram.Sql.TransactSql.TransactSqlParser

- replay Arguments: Follows in Member [turn], then ')'
- replay AssignOp: Follows in TSqlSelectSublist [choice], then TSqlValueExpression
- replay BrokerString: Follows in AvailabilityMade [turn], then '('
- replay ColumnList: Follows in VariableSource [turn], then ')'
- replay DataAlias: Follows in RowsetArgument [choice], then when (alias is null || string.Equals(name, "DATA", System.StringCompar…
- replay DatePart: Follows in DatePart [choice], then ')'
- replay FullTextAll: Follows in FullTextColumns [choice], then ')'
- replay GroupingSetItem: Follows in GroupingSet [choice], then ')'
- replay GroupingSet: Follows in TSqlGrouping [choice], then ')'
- replay JoinHint: Follows in TSqlTableReference [turn], then "JOIN"i
- replay JoinedRight: Follows in TSqlTableReference [turn], then "ON"i
- replay Nulls: Follows in CallTail [choice], then Over
- replay OdbcLiteralKind: Follows in OdbcEscape [choice], then NationalCharacterStringLiteral
- replay OdbcType: Follows in OdbcFunction [choice], then ')'
- replay OffsetFetch: Follows in InlineReturn [choice], then ')'
- replay OrderByClause: Follows in InlineReturn [choice], then ')'
- replay PredictCall: Follows in TSqlTablePrimary [choice], then PredictSchema
- replay QueryHints: Follows in TSqlInsert [choice], then when (options is null || rows is not Query.FromExecute)
- replay RowValueConstructorElement: Follows in RowValueConstructor [choice], then ',' …
- replay RowValueConstructor: Follows in TSqlPredicate [choice], then PredicateTail
- replay RowsetArgument: Follows in RowsetArgument [choice], then ')'
- replay RowsetFunction: Follows in TSqlTablePrimary [choice], then when (Syntax.Schemas(f, schema))
- replay RowsetSchema: Follows in TSqlTablePrimary [choice], then when (Syntax.Schemas(f, schema))
- replay RowsetValue: Follows in RowsetArgument [choice], then ',' …
- replay SearchCondition: Follows in MergeArm [choice], then "THEN"i
- replay TSqlAlias: Follows in TSqlSelectSublist [choice], then '='
- replay TSqlGroupingColumn: Follows in GroupByExpression [choice], then ')'
- replay TSqlJoinType: Follows in TSqlTableReference [turn], then "JOIN"i
- replay TSqlQueryExpression: Follows in InlineReturn [choice], then ')'
- replay TSqlSubquery: Follows in TSqlTablePrimary [choice], then CorrelationName
- replay TSqlValueExpression: Follows in TSqlPrimaryCore [choice], then ')'
- replay Top: Follows in TSqlInsert [choice], then DmlTarget
- replay WithClause: Follows in InlineReturn [choice], then TSqlQueryExpression
- replay WithinGroup: Follows in CallTail [choice], then Over
- replay AdHocObject: under TSqlTablePrimary
- replay AdHocServer: under TSqlTablePrimary
- replay Argument: under Arguments
- replay AtTimeZone: under TSqlValuePrimary
- replay BooleanPrimary: under BooleanTest
- replay BooleanTerm: under SearchCondition
- replay BooleanTest: under TSqlBooleanFactor
- replay CallTail: under TSqlPrimaryCore
- replay CaseExpression: under TSqlPrimaryCore
- replay CastOperand: under TSqlCast
- replay ChangeTrackingContext: under WithClause
- replay ChunksFunction: under TSqlTablePrimary
- replay ChunksOverlap: under ChunksFunction
- replay ChunksSet: under ChunksFunction
- replay ChunksSize: under ChunksFunction
- replay ChunksSource: under ChunksFunction
- replay ChunksType: under ChunksFunction
- replay ChunksValue: under ChunksSource
- replay Collate: under TSqlValueExpression
- replay ColumnName: under ColumnList
- replay ColumnReference: under TSqlPrimaryCore
- replay CountStar: under TSqlPrimaryCore
- replay CteBody: under CteDefinition
- replay CteDefinition: under WithClause
- replay DatePartCall: under TSqlPrimaryCore
- replay DatePartFunction: under DatePartCall
- replay DistinctTail: under TSqlPredicate
- replay DottedType: under ?
- replay FetchClause: under OffsetFetch
- replay ForClause: under TSqlSubquery
- replay FromClause: under TSqlQuerySpecification
- replay FullTextColumns: under FullTextPredicate
- replay FullTextPredicate: under TSqlPredicate
- replay FullTextSearch: under RowsetFunction
- replay FunctionCall: under TSqlPrimaryCore
- replay GraphMatch: under TSqlPredicate
- replay GroupByExpression: under GroupingSet
- replay GroupList: under GroupingSet
- replay GroupWith: under TSqlGroupByClause
- replay HavingClause: under TSqlQuerySpecification
- replay IdentityFunction: under TSqlSelectSublist
- replay IdentityNumber: under IdentityFunction
- replay InPredicateValue: under NegatablePredicate
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
- replay LeftRightCall: under TSqlPrimaryCore
- replay Member: under TSqlValuePrimary
- replay NegatablePredicate: under PredicateTail
- replay NextValue: under TSqlPrimaryCore
- replay NullSpec: under PredictColumn
- replay OdbcEscape: under TSqlPrimaryCore
- replay OdbcFunction: under OdbcEscape
- replay OpenQueryCall: under RowsetFunction
- replay Over: under CallTail
- replay PivotName: under PivotNames
- replay PivotNames: under Pivot
- replay PivotSuffix: under TSqlTableReference
- replay Pivot: under PivotSuffix
- replay PredicateTail: under TSqlPredicate
- replay PredictColumn: under PredictSchema
- replay PredictSchema: under TSqlTablePrimary
- replay QueryHint: under QueryHints
- replay QueryPrimary: under TSqlQueryExpression
- replay Result: under CaseExpression
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
- replay SourceHints: under TSqlTablePrimary
- replay SystemTimeWhen: under SystemTime
- replay SystemTime: under TSqlTablePrimary
- replay TSqlAsClause: under TSqlSelectSublist
- replay TSqlBooleanFactor: under BooleanTerm
- replay TSqlCast: under TSqlPrimaryCore
- replay TSqlEscapeClause: under NegatablePredicate
- replay TSqlGroupByClause: under TSqlQuerySpecification
- replay TSqlGrouping: under TSqlGroupByClause
- replay TSqlPredicate: under BooleanPrimary
- replay TSqlPrimaryCore: under TSqlValuePrimary
- replay TSqlQuerySpecification: under QueryPrimary
- replay TSqlRow: under TSqlTableValueConstructor
- replay TSqlSelectList: under TSqlQuerySpecification
- replay TSqlSelectSublist: under TSqlSelectList
- replay TSqlSimpleWhen: under CaseExpression
- replay TSqlTablePrimary: under JoinedRight
- replay TSqlTableReference: under FromClause
- replay TSqlTableValueConstructor: under QueryPrimary
- replay TSqlValueFunction: under TSqlPrimaryCore
- replay TSqlValuePrimary: under TSqlValueExpression
- replay TSqlValueSpecification: under TSqlPrimaryCore
- replay TableHint: under SampledHint
- replay TableHints: under SourceHints
- replay TableRepeatable: under TableSample
- replay TableSample: under TSqlTablePrimary
- replay TableSearchColumn: under TableSearchColumns
- replay TableSearchColumns: under RowsetFunction
- replay TimeZone: under AtTimeZone
- replay TopSuffix: under Top
- replay Unpivot: under PivotSuffix
- replay UnsignedLiteral: under TSqlPrimaryCore
- replay UseModel: under FunctionCall
- replay ValueFunction: under TSqlPrimaryCore
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

- replay Arguments: Follows in Member [turn], then ')'
- replay AssignOp: Follows in TSqlSelectSublist [choice], then TSqlValueExpression
- replay BrokerString: Follows in AvailabilityMade [turn], then '('
- replay ColumnList: Follows in VariableSource [turn], then ')'
- replay DataAlias: Follows in RowsetArgument [choice], then when (alias is null || string.Equals(name, "DATA", System.StringCompar…
- replay DatePart: Follows in DatePart [choice], then ')'
- replay FullTextAll: Follows in FullTextColumns [choice], then ')'
- replay GroupingSetItem: Follows in GroupingSet [choice], then ')'
- replay GroupingSet: Follows in TSqlGrouping [choice], then ')'
- replay JoinHint: Follows in TSqlTableReference [turn], then "JOIN"i
- replay JoinedRight: Follows in TSqlTableReference [turn], then "ON"i
- replay Nulls: Follows in CallTail [choice], then Over
- replay OdbcLiteralKind: Follows in OdbcEscape [choice], then NationalCharacterStringLiteral
- replay OdbcType: Follows in OdbcFunction [choice], then ')'
- replay OffsetFetch: Follows in InlineReturn [choice], then ')'
- replay OrderByClause: Follows in InlineReturn [choice], then ')'
- replay PredictCall: Follows in TSqlTablePrimary [choice], then PredictSchema
- replay QueryHints: Follows in TSqlInsert [choice], then when (options is null || rows is not Query.FromExecute)
- replay RowValueConstructorElement: Follows in RowValueConstructor [choice], then ',' …
- replay RowValueConstructor: Follows in TSqlPredicate [choice], then PredicateTail
- replay RowsetArgument: Follows in RowsetArgument [choice], then ')'
- replay RowsetFunction: Follows in TSqlTablePrimary [choice], then when (Syntax.Schemas(f, schema))
- replay RowsetSchema: Follows in TSqlTablePrimary [choice], then when (Syntax.Schemas(f, schema))
- replay RowsetValue: Follows in RowsetArgument [choice], then ',' …
- replay SearchCondition: Follows in MergeArm [choice], then "THEN"i
- replay TSqlAlias: Follows in TSqlSelectSublist [choice], then '='
- replay TSqlGroupingColumn: Follows in GroupByExpression [choice], then ')'
- replay TSqlJoinType: Follows in TSqlTableReference [turn], then "JOIN"i
- replay TSqlQueryExpression: Follows in InlineReturn [choice], then ')'
- replay TSqlSubquery: Follows in TSqlTablePrimary [choice], then CorrelationName
- replay TSqlValueExpression: Follows in TSqlPrimaryCore [choice], then ')'
- replay Top: Follows in TSqlInsert [choice], then DmlTarget
- replay WithClause: Follows in InlineReturn [choice], then TSqlQueryExpression
- replay WithinGroup: Follows in CallTail [choice], then Over
- replay AdHocObject: under TSqlTablePrimary
- replay AdHocServer: under TSqlTablePrimary
- replay Argument: under Arguments
- replay AtTimeZone: under TSqlValuePrimary
- replay BooleanPrimary: under BooleanTest
- replay BooleanTerm: under SearchCondition
- replay BooleanTest: under TSqlBooleanFactor
- replay CallTail: under TSqlPrimaryCore
- replay CaseExpression: under TSqlPrimaryCore
- replay CastOperand: under TSqlCast
- replay ChangeTrackingContext: under WithClause
- replay ChunksFunction: under TSqlTablePrimary
- replay ChunksOverlap: under ChunksFunction
- replay ChunksSet: under ChunksFunction
- replay ChunksSize: under ChunksFunction
- replay ChunksSource: under ChunksFunction
- replay ChunksType: under ChunksFunction
- replay ChunksValue: under ChunksSource
- replay Collate: under TSqlValueExpression
- replay ColumnName: under ColumnList
- replay ColumnReference: under TSqlPrimaryCore
- replay CountStar: under TSqlPrimaryCore
- replay CteBody: under CteDefinition
- replay CteDefinition: under WithClause
- replay DatePartCall: under TSqlPrimaryCore
- replay DatePartFunction: under DatePartCall
- replay DistinctTail: under TSqlPredicate
- replay DottedType: under ?
- replay FetchClause: under OffsetFetch
- replay ForClause: under TSqlSubquery
- replay FromClause: under TSqlQuerySpecification
- replay FullTextColumns: under FullTextPredicate
- replay FullTextPredicate: under TSqlPredicate
- replay FullTextSearch: under RowsetFunction
- replay FunctionCall: under TSqlPrimaryCore
- replay GraphMatch: under TSqlPredicate
- replay GroupByExpression: under GroupingSet
- replay GroupList: under GroupingSet
- replay GroupWith: under TSqlGroupByClause
- replay HavingClause: under TSqlQuerySpecification
- replay IdentityFunction: under TSqlSelectSublist
- replay IdentityNumber: under IdentityFunction
- replay InPredicateValue: under NegatablePredicate
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
- replay LeftRightCall: under TSqlPrimaryCore
- replay Member: under TSqlValuePrimary
- replay NegatablePredicate: under PredicateTail
- replay NextValue: under TSqlPrimaryCore
- replay NullSpec: under PredictColumn
- replay OdbcEscape: under TSqlPrimaryCore
- replay OdbcFunction: under OdbcEscape
- replay OpenQueryCall: under RowsetFunction
- replay Over: under CallTail
- replay PivotName: under PivotNames
- replay PivotNames: under Pivot
- replay PivotSuffix: under TSqlTableReference
- replay Pivot: under PivotSuffix
- replay PredicateTail: under TSqlPredicate
- replay PredictColumn: under PredictSchema
- replay PredictSchema: under TSqlTablePrimary
- replay QueryHint: under QueryHints
- replay QueryPrimary: under TSqlQueryExpression
- replay Result: under CaseExpression
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
- replay SourceHints: under TSqlTablePrimary
- replay SystemTimeWhen: under SystemTime
- replay SystemTime: under TSqlTablePrimary
- replay TSqlAsClause: under TSqlSelectSublist
- replay TSqlBooleanFactor: under BooleanTerm
- replay TSqlCast: under TSqlPrimaryCore
- replay TSqlEscapeClause: under NegatablePredicate
- replay TSqlGroupByClause: under TSqlQuerySpecification
- replay TSqlGrouping: under TSqlGroupByClause
- replay TSqlPredicate: under BooleanPrimary
- replay TSqlPrimaryCore: under TSqlValuePrimary
- replay TSqlQuerySpecification: under QueryPrimary
- replay TSqlRow: under TSqlTableValueConstructor
- replay TSqlSelectList: under TSqlQuerySpecification
- replay TSqlSelectSublist: under TSqlSelectList
- replay TSqlSimpleWhen: under CaseExpression
- replay TSqlTablePrimary: under JoinedRight
- replay TSqlTableReference: under FromClause
- replay TSqlTableValueConstructor: under QueryPrimary
- replay TSqlValueFunction: under TSqlPrimaryCore
- replay TSqlValuePrimary: under TSqlValueExpression
- replay TSqlValueSpecification: under TSqlPrimaryCore
- replay TableHint: under SampledHint
- replay TableHints: under SourceHints
- replay TableRepeatable: under TableSample
- replay TableSample: under TSqlTablePrimary
- replay TableSearchColumn: under TableSearchColumns
- replay TableSearchColumns: under RowsetFunction
- replay TimeZone: under AtTimeZone
- replay TopSuffix: under Top
- replay Unpivot: under PivotSuffix
- replay UnsignedLiteral: under TSqlPrimaryCore
- replay UseModel: under FunctionCall
- replay ValueFunction: under TSqlPrimaryCore
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

- again Power: through Unary
- again Primary: through Sum
- again Product: opens a way
- open Product: turns, captured; the seam leads every alternative of the turn; open; trivia & op: ['*' | '/'] & trivia & right: Unary => (op == "*" ? left …
- again Sum: opens a way
- open Sum: turns, captured; the seam leads every alternative of the turn; open; trivia & op: ['+' | '-'] & trivia & right: Product => (op == "+" ? lef…
- again Unary: through Power

## DotGram.Tests.Calculators.OneRuleParser

- again Expr: opens a way
- open Expr: turns, captured; the seam leads every alternative of the turn; open; (trivia & '+' & trivia & right: Expr => (new Add(left, right)) | trivi…
- open Expr: choice; the seam leads every alternative; open; (trivia & '+' & trivia & right: Expr => (new Add(left, right)) | trivi…

## DotGram.Tests.Calculators.StrengthCalculator

- again Expr: opens a way
- open Expr: turns, captured; the seam leads every alternative of the turn; open; (trivia & '+' & trivia & right: Expr => (left + right) | trivia & '-' …
- open Expr: choice; the seam leads every alternative; open; (trivia & '+' & trivia & right: Expr => (left + right) | trivia & '-' …

## DotGram.Tests.Calculators.TwoCalculators

- again Primary: through Sum
- again Primary: through Sum
- again Product: opens a way
- open Product: turns, captured; the seam leads every alternative of the turn; open; trivia & op: ['*' | '/'] & trivia & right: Unary_With1 => (op == "*" ?…
- again Product: opens a way
- open Product: turns, captured; the seam leads every alternative of the turn; open; trivia & op: ['*' | '/'] & trivia & right: Unary_With2 => (op == "*" ?…
- again Sum: opens a way
- open Sum: turns, captured; the seam leads every alternative of the turn; open; trivia & op: ['+' | '-'] & trivia & right: Product_With1 => (op == "+"…
- again Sum: opens a way
- open Sum: turns, captured; the seam leads every alternative of the turn; open; trivia & op: ['+' | '-'] & trivia & right: Product_With2 => (op == "+"…
- again Unary: through Primary
- again Unary: through Primary

## DotGram.Web.Rfc3986

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
- open Reference: choice; an alternative that may read nothing; entry; (u: Uri => (u) | r: RelativeRef => (r))
- again RelativePart: opens a way
- open RelativePart: choice; an alternative that may read nothing; open; ("//" & a: Authority & path: PathAbEmpty => (a with { Path = path }) |…
- again RelativeRef: through RelativePart
- again Uri: through HierPart

## DotGram.Web.Rfc5322

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
- again AtomText: opens a way
- open AtomText: run; what follows begins alike; open; Atext+
- again Ccontent: through Comment
- again Ccontent: through Comment
- again Ccontent: through Comment
- again Ccontent: through Comment
- again Ccontent: through Comment
- again Cfws: opens a way
- open Cfws: choice; alternatives begin alike; open; ((Fws? & Comment)+ & Fws? | Fws)
- open Cfws: turns; a turn led by what may read nothing; open; (Fws? & Comment)+
- open Cfws: optional; what follows begins alike; open; Fws?
- again Cfws: opens a way
- open Cfws: choice; alternatives begin alike; open; ((CurrentFws? & Comment_With1)+ & CurrentFws? | CurrentFws)
- open Cfws: turns; a turn led by what may read nothing; open; (CurrentFws? & Comment_With1)+
- again Cfws: opens a way
- open Cfws: choice; alternatives begin alike; open; ((CurrentFws? & Comment_With2)+ & CurrentFws? | CurrentFws)
- open Cfws: turns; a turn led by what may read nothing; open; (CurrentFws? & Comment_With2)+
- open Cfws: optional; a turn led by what may read nothing; open; CurrentFws?
- again Cfws: opens a way
- open Cfws: choice; alternatives begin alike; open; ((CurrentFws? & Comment_With3)+ & CurrentFws? | CurrentFws)
- open Cfws: turns; a turn led by what may read nothing; open; (CurrentFws? & Comment_With3)+
- open Cfws: optional; a turn led by what may read nothing; open; CurrentFws?
- again Cfws: opens a way
- open Cfws: choice; alternatives begin alike; open; ((CurrentFws? & Comment_With4)+ & CurrentFws? | CurrentFws)
- open Cfws: turns; a turn led by what may read nothing; open; (CurrentFws? & Comment_With4)+
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
- open CurrentFws: run; what follows begins alike; open; Wsp+
- again CurrentLocalPart: opens a way
- open CurrentLocalPart: choice; every alternative led by what may read nothing; open; (Cfws_With1? & text: DotAtomText & Cfws_With1? => (text) | Cfws_With1?…
- again CurrentLocalPart: opens a way
- open CurrentLocalPart: choice; every alternative led by what may read nothing; open; (Cfws_With2? & text: DotAtomText & Cfws_With2? => (text) | Cfws_With2?…
- again CurrentLocalPart: opens a way
- open CurrentLocalPart: choice; every alternative led by what may read nothing; open; (Cfws_With3? & text: DotAtomText & Cfws_With3? => (text) | Cfws_With3?…
- again CurrentLocalPart: opens a way
- open CurrentLocalPart: choice; every alternative led by what may read nothing; open; (Cfws_With4? & text: DotAtomText & Cfws_With4? => (text) | Cfws_With4?…
- again CurrentPhrase: opens a way
- open CurrentPhrase: turns; a turn led by what may read nothing; open; WordText_With2+
- again CurrentPhrase: opens a way
- open CurrentPhrase: turns; a turn led by what may read nothing; open; WordText_With3+
- again CurrentPhrase: opens a way
- open CurrentPhrase: turns; a turn led by what may read nothing; open; WordText_With4+
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
- again Fws: through ObsFws
- again Group: through Cfws
- again GroupList: opens a way
- open GroupList: choice; an alternative that may read nothing; open; (list: MailboxList => (list) | ObsGroupList => (Array.Empty<EmailAddre…
- again GroupList: opens a way
- open GroupList: choice; an alternative that may read nothing; open; (list: MailboxList_With4 => (list) | Never => (Array.Empty<EmailAddres…
- again Group: through CurrentPhrase
- again LocalPart: through Word
- again Mailbox: opens a way
- open Mailbox: choice; every alternative led by what may read nothing; open; (name: Phrase? & address: AngleAddr => (new EmailAddress.Mailbox(Rfc53…
- open Mailbox: optional, captured; a turn led by what may read nothing; open; name: Phrase?
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
- again ObsFws: opens a way
- open ObsFws: turns; a turn led by what may read nothing; open; (Crlf? & Wsp)+
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
- again WordText: opens a way
- open WordText: optional; what follows begins alike; open; Cfws?
- again WordText: opens a way
- open WordText: optional; what follows begins alike; open; Cfws_With2?
- again WordText: opens a way
- open WordText: optional; what follows begins alike; open; Cfws_With3?
- again WordText: opens a way
- open WordText: optional; what follows begins alike; open; Cfws_With4?

## DotGram.Web.Rfc5646

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

- again DispositionField: through DispositionParms
- again DispositionParms: opens a way
- open DispositionParms: turns, captured; a turn led by what may read nothing; open; items: DispositionParm*

## DotGram.Web.Rfc6570

- again Literals: opens a way
- open Literals: turns; what follows begins alike; open; (LiteralChar | PctEncoded)+
- again Template: through Literals

## DotGram.Web.Rfc7239

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

## DotGram.Web.Rfc8288

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
