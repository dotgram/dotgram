# Which carrier each grammar is read with, and why

Every grammar of the solution, as the last build with `-p:DotGramReportGeneration=true` compiled
it: the carrier `Auto` took (GRAM5012), and for a grammar kept on the tape, the gate that kept it
and each rule held there. Written by `--carriers` (`benchmarks/DotGram.Benchmarks/Carriers.cs`)
from the reports that build left; run again rather than edited.

**Carrier** is what `Auto` took: `immediate`, `tape`, or the author's own choice. **Gate** is what
kept a grammar on the tape: `replay` — a building rule read where the reading may not stand
(`Replay`) — or `read again` — a rule the reader can be asked again after it answered, which is
asked only where the first gate let everything through. **Direct** is how many of the replayed
rules have a cause of their own; the rest are under one of them.

| Grammar | Carrier | Gate | Building | Replayed | Direct | Read again |
| --- | --- | --- | ---: | ---: | ---: | ---: |
| DotGram.Benchmarks.CallCost.Called | immediate | none |  |  |  |  |
| DotGram.Benchmarks.CallCost.Inlined | nothing to choose | none |  |  |  |  |
| DotGram.Benchmarks.CallCost.Valued | immediate | none |  |  |  |  |
| DotGram.Benchmarks.Climbing | tape | read again | 1 | 0 | 0 | 1 |
| DotGram.Benchmarks.Config | tape | read again | 4 | 0 | 0 | 5 |
| DotGram.Benchmarks.Extents | immediate | none |  |  |  |  |
| DotGram.Benchmarks.Feed | nothing to choose | none |  |  |  |  |
| DotGram.Benchmarks.Flat.Lowered | nothing to choose | none |  |  |  |  |
| DotGram.Benchmarks.Flat.NotLowered | immediate | none |  |  |  |  |
| DotGram.Benchmarks.ImmediateSql | immediate (author) |  |  |  |  |  |
| DotGram.Benchmarks.Levels | tape | read again | 4 | 0 | 0 | 4 |
| DotGram.Benchmarks.MaterializationCost.NoCaptures | tape | read again | 1 | 0 | 0 | 2 |
| DotGram.Benchmarks.MaterializationCost.SpanCaptures | tape | replay | 7 | 1 | 1 | 0 |
| DotGram.Benchmarks.MaterializationCost.WithCaptures | tape | read again | 1 | 0 | 0 | 2 |
| DotGram.Benchmarks.Nesting | nothing to choose | none |  |  |  |  |
| DotGram.Benchmarks.Numbers | tape | read again | 2 | 0 | 0 | 2 |
| DotGram.Benchmarks.Possession.Open | nothing to choose | none |  |  |  |  |
| DotGram.Benchmarks.Possession.Settled | nothing to choose | none |  |  |  |  |
| DotGram.Benchmarks.Settlements | nothing to choose | none |  |  |  |  |
| DotGram.Benchmarks.TinyScalar | immediate | none |  |  |  |  |
| DotGram.Benchmarks.Urls | tape | read again | 2 | 0 | 0 | 3 |
| DotGram.Examples.Expressions.ArithmeticTree | tape | read again | 5 | 0 | 0 | 5 |
| DotGram.Examples.Expressions.Calculator | tape | read again | 6 | 0 | 0 | 8 |
| DotGram.Examples.Expressions.ClampedExample | tape | read again | 3 | 0 | 0 | 3 |
| DotGram.Examples.Expressions.LocaleNumber | tape | read again | 2 | 0 | 0 | 2 |
| DotGram.Examples.Feeds.FeedReader | tape | read again | 5 | 0 | 0 | 5 |
| DotGram.Examples.Feeds.LoggingFeedReader | nothing to choose | none |  |  |  |  |
| DotGram.Examples.Feeds.RecoveringFeedReader | nothing to choose | none |  |  |  |  |
| DotGram.Examples.Feeds.StockCountReader | nothing to choose | none |  |  |  |  |
| DotGram.Examples.Feeds.StreamingFeedReader | nothing to choose | none |  |  |  |  |
| DotGram.Examples.Formats.Config | tape | read again | 4 | 0 | 0 | 6 |
| DotGram.Examples.Formats.Config.Located | tape | read again | 4 | 0 | 0 | 6 |
| DotGram.Examples.Formats.FileNames | tape | read again | 2 | 0 | 0 | 2 |
| DotGram.Examples.Formats.FixParser | immediate | none |  |  |  |  |
| DotGram.Examples.Formats.FixedWidth | immediate | none |  |  |  |  |
| DotGram.Examples.Formats.HttpParser | tape | read again | 5 | 0 | 0 | 6 |
| DotGram.Examples.Formats.IniParser | tape | read again | 7 | 0 | 0 | 11 |
| DotGram.Examples.Formats.JsonParser | tape | read again | 9 | 0 | 0 | 11 |
| DotGram.Examples.Formats.Links | nothing to choose | none |  |  |  |  |
| DotGram.Examples.Formats.MarkdownParser | tape | read again | 9 | 0 | 0 | 10 |
| DotGram.Examples.Formats.MetricsLine | tape | read again | 5 | 0 | 0 | 7 |
| DotGram.Examples.Formats.Netstrings | tape | read again | 2 | 0 | 0 | 1 |
| DotGram.Examples.Formats.TypedCsv | nothing to choose | none |  |  |  |  |
| DotGram.Examples.Formats.XmlParser | tape | replay | 7 | 6 | 3 | 0 |
| DotGram.Examples.Formats.YamlLite | tape | read again | 6 | 0 | 0 | 7 |
| DotGram.Examples.Languages.Filter | tape | replay | 9 | 6 | 2 | 0 |
| DotGram.Examples.Languages.FilterFile | tape | read again | 2 | 0 | 0 | 3 |
| DotGram.Examples.Languages.Filters | tape | replay | 2 | 1 | 1 | 0 |
| DotGram.Examples.Languages.GramGrammar | tape | replay | 35 | 28 | 2 | 0 |
| DotGram.Examples.Languages.Lexemes | nothing to choose | none |  |  |  |  |
| DotGram.Examples.Languages.Scoped | tape | read again | 4 | 0 | 0 | 5 |
| DotGram.Examples.Languages.Selectors | tape | read again | 6 | 0 | 0 | 2 |
| DotGram.Examples.Languages.SettingsFile | tape | read again | 2 | 0 | 0 | 3 |
| DotGram.Examples.Languages.SqlDialect | immediate | none |  |  |  |  |
| DotGram.Examples.Languages.SqlReadOnly | nothing to choose | none |  |  |  |  |
| DotGram.Examples.Languages.TokenizedQuery | immediate | none |  |  |  |  |
| DotGram.ExpressionLanguage.ExpressionParser | tape | replay | 137 | 131 | 20 | 0 |
| DotGram.ExpressionLanguage.ExpressionParser.Immediate | immediate (author) |  |  |  |  |  |
| DotGram.Finance.Fix.FixGrammar | nothing to choose | none |  |  |  |  |
| DotGram.Finance.Fix44.Fix44Grammar | nothing to choose | none |  |  |  |  |
| DotGram.Finance.Fix44.FixFieldGrammar | nothing to choose | none |  |  |  |  |
| DotGram.Sql.Standard.Sql92Parser | tape | replay | 48 | 44 | 4 | 0 |
| DotGram.Sql.Standard.SqlStandardParser | tape | replay | 556 | 308 | 26 | 0 |
| DotGram.Sql.TransactSql.TransactSqlParser | tape | replay | 658 | 643 | 84 | 0 |
| DotGram.Sql.TransactSql.TransactSqlParser.Located | tape | replay | 658 | 643 | 84 | 0 |
| DotGram.Tests.Calculators.DecimalCalculator | tape | read again | 5 | 0 | 0 | 5 |
| DotGram.Tests.Calculators.OneRuleParser | tape | read again | 1 | 0 | 0 | 1 |
| DotGram.Tests.Calculators.StrengthCalculator | tape | read again | 1 | 0 | 0 | 1 |
| DotGram.Tests.Calculators.TwoCalculators | tape | read again | 10 | 0 | 0 | 8 |
| DotGram.Tests.Extents | immediate | none |  |  |  |  |
| DotGram.Tests.Generated.UrlGrammar | nothing to choose | none |  |  |  |  |
| DotGram.Web.Rfc3339 | immediate | none |  |  |  |  |
| DotGram.Web.Rfc3986 | tape | read again | 6 | 0 | 0 | 12 |
| DotGram.Web.Rfc5322 | tape | read again | 48 | 0 | 0 | 114 |
| DotGram.Web.Rfc5646 | tape | read again | 9 | 0 | 0 | 7 |
| DotGram.Web.Rfc6265 | tape | read again | 6 | 0 | 0 | 6 |
| DotGram.Web.Rfc6266 | tape | read again | 3 | 0 | 0 | 5 |
| DotGram.Web.Rfc6570 | tape | read again | 5 | 0 | 0 | 3 |
| DotGram.Web.Rfc6901 | tape | read again | 2 | 0 | 0 | 3 |
| DotGram.Web.Rfc7239 | tape | read again | 7 | 0 | 0 | 14 |
| DotGram.Web.Rfc8259 | tape | read again | 10 | 0 | 0 | 11 |
| DotGram.Web.Rfc8288 | tape | read again | 5 | 0 | 0 | 8 |
| DotGram.Web.Rfc9110 | tape | read again | 4 | 0 | 0 | 8 |
| DotGram.Web.Rfc9651 | tape | read again | 15 | 0 | 0 | 17 |

## Where the second gate's ways are opened

Each place a rule's own reading opens a way, by its shape — a choice over characters, a run of
one character, the turns of a repetition — and why the way could not be left out. **Places**
counts each once per grammar; **captured** is how many of them capture inside the turn, and
**sealed** how many are in a rule every call of which is inside an atomic group or a lookahead,
or which nothing calls, so that no caller asks it again.

| Shape | Why the way stays | Grammars | Rules | Places | Captured | Sealed | For example |
| --- | --- | ---: | ---: | ---: | ---: | ---: | --- |
| choice | alternatives begin alike | 14 | 23 | 38 | 0 | 8 | Config.trivia: `(LineComment \| BlockComment)` |
| run | what follows begins alike | 18 | 34 | 34 | 0 | 7 | Config.LineComment: `[^ '\n' \| '\r']*` |
| turns | a turn led by what may read nothing | 8 | 16 | 34 | 8 | 2 | IniParser.Entries: `(item0: Entry \| Blank)*` |
| choice | alternatives begin apart | 12 | 20 | 33 | 0 | 1 | JsonParser.Body: `(Plain \| Escape)` |
| optional | what follows begins alike | 12 | 14 | 20 | 7 | 3 | NoCaptures.Url: `(UserInfo & '@')?` |
| choice | every alternative led by what may read nothing | 4 | 8 | 17 | 0 | 1 | IniParser.Entries: `(item0: Entry \| Blank)` |
| turns | the seam leads every alternative of the turn | 9 | 11 | 15 | 15 | 0 | Climbing.Expr: `(trivia & '+' & trivia & r: Expr => (l + r) \| trivia & '-' & trivia & …` |
| choice | an alternative that may read nothing | 7 | 14 | 15 | 0 | 1 | HttpParser.Field: `(eol \| ?=eof)` |
| turns | what follows begins alike | 10 | 11 | 12 | 6 | 2 | JsonParser.Body: `(Plain \| Escape)*` |
| choice | the seam leads every alternative | 7 | 9 | 11 | 0 | 0 | Climbing.Expr: `(trivia & '+' & trivia & r: Expr => (l + r) \| trivia & '-' & trivia & …` |
| counted | what follows begins alike | 3 | 3 | 11 | 1 | 0 | Rfc3986.IPv6Address: `(H16 & ':'){0,2}` |
| optional | a turn led by what may read nothing | 2 | 5 | 7 | 2 | 0 | Rfc3986.Authority: `(user: UserInfoText & '@')?` |
| choice | literals, a shorter one wanted | 4 | 4 | 4 | 0 | 0 | HttpParser.eol: `("\r\n" \| '\r')` |
| choice | an ignore-case literal | 1 | 1 | 4 | 0 | 0 | Rfc5646.Grandfathered: `("i-ami"i \| "i-bnn"i \| "i-default"i \| "i-enochian"i \| "i-hak"i \| "i-kl…` |
| turns | a turn led by a lookahead | 1 | 1 | 1 | 0 | 1 | Config.BlockComment: `(?!"*/" & any)*` |
| choice | literals, follow unknown | 1 | 1 | 1 | 0 | 0 | FeedReader.eol: `("\r\n" \| '\r')` |
| turns | seam first, what follows begins alike past it | 1 | 1 | 1 | 0 | 1 | Scoped.Program: `(trivia & Let)*` |
| choice | literals under a capture or construction | 1 | 1 | 1 | 0 | 0 | Rfc9651.SfBareItem: `("?1" => (BareItem.Boolean.True) \| "?0" => (BareItem.Boolean.False))` |

## DotGram.Benchmarks.Climbing

- again Expr: opens a way
- open Expr: turns, captured; the seam leads every alternative of the turn; open; (trivia & '+' & trivia & r: Expr => (l + r) | trivia & '-' & trivia & …
- open Expr: choice; the seam leads every alternative; open; (trivia & '+' & trivia & r: Expr => (l + r) | trivia & '-' & trivia & …

## DotGram.Benchmarks.Config

- again BlockComment: opens a way
- open BlockComment: turns; a turn led by a lookahead; sealed; (?!"*/" & any)*
- again Entry: through trivia
- again File: through trivia
- again LineComment: opens a way
- open LineComment: run; what follows begins alike; sealed; [^ '\n' | '\r']*
- again trivia: opens a way
- open trivia: choice; alternatives begin alike; open; (LineComment | BlockComment)

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

- again Entry: through trivia
- again File: through trivia
- again LineComment: opens a way
- open LineComment: run; what follows begins alike; sealed; [^ '\n' | '\r']*
- again Spacing: opens a way
- open Spacing: run; what follows begins alike; sealed; Whitespace+
- again Value: opens a way
- open Value: run; what follows begins alike; open; [^ '\n' | '\r']*
- again trivia: through Spacing

## DotGram.Examples.Formats.Config.Located

- again Entry: through trivia
- again File: through trivia
- again LineComment: opens a way
- open LineComment: run; what follows begins alike; sealed; [^ '\n' | '\r']*
- again Spacing: opens a way
- open Spacing: run; what follows begins alike; sealed; Whitespace+
- again Value: opens a way
- open Value: run; what follows begins alike; open; [^ '\n' | '\r']*
- again trivia: through Spacing

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
- open Body: choice; alternatives begin apart; open; (Plain | Escape)
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

- again Line: through trivia
- again LineComment: opens a way
- open LineComment: run; what follows begins alike; sealed; [^ '\n' | '\r']*
- again Quoted: opens a way
- open Quoted: turns; what follows begins alike; open; ("""" & (?!'"' & any)*)*
- again Reading: through trivia
- again Spacing: opens a way
- open Spacing: run; what follows begins alike; sealed; Whitespace+
- again Value: opens a way
- open Value: choice; alternatives begin alike; open; (?=Digits & trivia & '.' & trivia & d: Decimal => (d) | n: Long => (n)…
- again trivia: through Spacing

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
- open Quoted: choice; alternatives begin apart; open; ("""" | [^ '"'])
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
- open Expr: choice; alternatives begin apart; open; ('(' & trivia & inner: Expr & trivia & ')' => (inner) | n: Integer => …
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
- open Quoted: choice; alternatives begin apart; open; ("""" | [^ '"'])
- again Setting: opens a way
- open Setting: choice; alternatives begin apart; open; (Number | Quoted | Word)

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
- replay LargeObjectLength: Follows in BinaryStringType [choice], then ')'
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

- replay AlterColumnWord: Follows in AlterTableAction [choice], then when (Syntax.FlagsOnline(flag, options))
- replay Arguments: Follows in Member [turn], then ')'
- replay AssemblyOptions: Follows in CodeStatement [choice], then when (Syntax.Tail(tail) is not null)
- replay AssignOp: Follows in TSqlSelectSublist [choice], then TSqlValueExpression
- replay BackupRedundancy: Follows in DatabaseTail [turn], then ')'
- replay BindingOptions: Follows in BrokerStatement [choice], then when (Syntax.NamedOnce(null, options) && Syntax.HasOption(options, "US…
- replay BrokerString: Follows in AvailabilityMade [turn], then '('
- replay ColumnDefinition: Follows in AlterTableAction [choice], then when (Syntax.AlteredColumn(column))
- replay ColumnKeySetting: Follows in KeyStatement [choice], then ')'
- replay ColumnList: Follows in VariableSource [choice], then ')'
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
- replay OffsetFetch: Follows in InlineReturn [choice], then ')'
- replay OnOff: Follows in TypeStatement [choice], then ')'
- replay OptionList: Follows in SwitchTail [choice], then ')'
- replay OptionsWith: Follows in CreateTableStatement [choice], then "AS"i
- replay OrderByClause: Follows in InlineReturn [choice], then ')'
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
- replay SqlPiece: Lookahead in AtomicBody [lookahead]
- replay TSqlAlias: Follows in TSqlSelectSublist [choice], then '='
- replay TSqlGroupingColumn: Follows in GroupByExpression [choice], then ')'
- replay TSqlJoinType: Follows in TSqlTableReference [turn], then "JOIN"i
- replay TSqlQueryExpression: Follows in InlineReturn [choice], then ')'
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
- replay AlgorithmSetting: under AsymmetricBody
- replay AlterConstraintOption: under AlterConstraintWith
- replay AlterConstraintWith: under ConstraintBody
- replay AlterDatabaseAction: under AlterDatabaseStatement
- replay AlterDatabaseStatement: under DefinitionStatement
- replay AlterIndexAction: under AlterIndexStatement
- replay AlterIndexStatement: under DefinitionStatement
- replay AlterTableAction: under AlterTableStatement
- replay AlterTableStatement: under DefinitionStatement
- replay ArchiveNest: under DatabaseSetting
- replay ArchiveOption: under ArchiveNest
- replay Argument: under Arguments
- replay AssemblyOption: under AssemblyOptions
- replay AssignTail: under MemberTail
- replay AssignedValue: under VariableChainTail
- replay Assignment: under Assignments
- replay Assignments: under TSqlUpdate
- replay AsymmetricBody: under KeyStatement
- replay AsymmetricChange: under KeyStatement
- replay AtTimeZone: under TSqlValuePrimary
- replay AtomicBody: under ProcedureStatement
- replay AtomicOption: under AtomicOptions
- replay AtomicOptions: under AtomicBody
- replay AttachOption: under DatabaseTail
- replay AuditStatement: under DefinitionStatement
- replay AvailabilityStatement: under DefinitionStatement
- replay BackupGroup: under BackupStatement
- replay BackupName: under BackupGroup
- replay BackupStatement: under DefinitionStatement
- replay BackupTarget: under BackupStatement
- replay BackupWhat: under BackupStatement
- replay BareIndexWord: under OldIndexOption
- replay BareKeyOption: under BareKeyOptions
- replay BareKeyOptions: under ConstraintWith
- replay BindingOption: under BindingOptions
- replay BooleanPrimary: under BooleanTest
- replay BooleanTerm: under SearchCondition
- replay BooleanTest: under TSqlBooleanFactor
- replay Branch: under ConditionalStatement
- replay BrokerStatement: under DefinitionStatement
- replay BrokerTarget: under ReceiveWhere
- replay BulkColumnTail: under BulkColumn
- replay BulkColumn: under BulkColumns
- replay BulkColumns: under BulkInsertStatement
- replay BulkDigits: under BulkOption
- replay BulkFileType: under BulkOption
- replay BulkFile: under BulkInsertStatement
- replay BulkFormat: under BulkOption
- replay BulkInsertStatement: under DefinitionStatement
- replay BulkNumber: under BulkOption
- replay BulkOption: under BulkWith
- replay BulkOrder: under BulkOption
- replay BulkStreamOption: under BulkStreamWith
- replay BulkStreamWith: under BulkInsertStatement
- replay BulkString: under BulkOption
- replay BulkTarget: under BulkInsertStatement
- replay BulkWith: under BulkInsertStatement
- replay CallTail: under TSqlPrimaryCore
- replay CaseExpression: under TSqlPrimaryCore
- replay CastOperand: under TSqlCast
- replay CertificateBody: under KeyStatement
- replay CertificateChange: under KeyStatement
- replay CertificateKeyItem: under CertificateKeyItems
- replay CertificateKeyItems: under ?
- replay CertificateSetting: under CertificateBody
- replay CertificateSource: under CertificateBody
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
- replay ClassificationOption: under MaintenanceStatement
- replay ClassifiedColumn: under MaintenanceStatement
- replay CodeStatement: under DefinitionStatement
- replay Collate: under TSqlValueExpression
- replay ColumnBody: under ColumnDefinition
- replay ColumnBody: under TableColumnBody
- replay ColumnBody: under TableColumnBody
- replay ColumnKeyValue: under KeyStatement
- replay ColumnName: under ColumnList
- replay ColumnReference: under TSqlPrimaryCore
- replay ColumnTail: under TableColumnBody
- replay ColumnTail: under TableColumnBody
- replay ColumnTail: under TableColumnBody
- replay ColumnTrait: under ColumnTail
- replay ColumnTrait: under ColumnTail
- replay ColumnTrait: under ColumnTail
- replay ColumnstoreOption: under ColumnstoreWith
- replay ColumnstoreWith: under CreateIndexStatement
- replay ConditionalStatement: under SqlPiece
- replay ConstraintBody: under TableElement
- replay ConstraintBody: under TableElement
- replay ConstraintOption: under ConstraintWith
- replay ConstraintWith: under ConstraintBody
- replay ConversationError: under EndConversationWith
- replay ConversationStatement: under SqlPiece
- replay CountStar: under TSqlPrimaryCore
- replay CreateDatabaseOption: under CreateDatabaseStatement
- replay CreateDatabaseStatement: under DefinitionStatement
- replay CreateIndexStatement: under DefinitionStatement
- replay CreateRoutine: under DefinitionStatement
- replay CreateStatisticsWith: under ExternalStatement
- replay CreateTableStatement: under DefinitionStatement
- replay CteBody: under CteDefinition
- replay CteDefinition: under WithClause
- replay CursorColumn: under CursorColumns
- replay CursorColumns: under CursorFor
- replay CursorDefinition: under DeclareStatement
- replay CursorInPlace: under SetStatement
- replay CursorIsoWord: under CursorDefinition
- replay CursorName: under CursorStatement
- replay CursorOption: under CursorDefinition
- replay CursorQuery: under CursorDefinition
- replay CursorStatement: under SqlPiece
- replay DataSourceChange: under DataSourceBody
- replay DataSourceItem: under DataSourceBody
- replay DataSourceOption: under DataSourceItem
- replay DatabaseAdd: under AlterDatabaseAction
- replay DatabaseDecimal: under DatabaseSetting
- replay DatabaseFiles: under CreateDatabaseStatement
- replay DatabaseInteger: under DatabaseSetting
- replay DatabaseLogFiles: under DatabaseFiles
- replay DatabaseModify: under AlterDatabaseAction
- replay DatabaseRemove: under AlterDatabaseAction
- replay DatabaseSetting: under CreateDatabaseOption
- replay DatabaseTail: under CreateDatabaseStatement
- replay DatabaseTermination: under AlterDatabaseStatement
- replay DatabaseText: under DatabaseSetting
- replay DatePartCall: under TSqlPrimaryCore
- replay DatePartFunction: under DatePartCall
- replay DbccArgument: under DbccArguments
- replay DbccArguments: under DbccStatement
- replay DbccFirst: under DbccOptions
- replay DbccOption: under DbccOptions
- replay DbccOptions: under DbccStatement
- replay DbccStatement: under SqlPiece
- replay DbccValue: under ExecValue
- replay Declaration: under DeclareStatement
- replay DeclareStatement: under DefinitionStatement
- replay DeclaredAs: under Declaration
- replay DefinitionStatement: under SqlPiece
- replay DialogOption: under DialogWith
- replay DialogWith: under ConversationStatement
- replay DistinctTail: under TSqlPredicate
- replay DmlCall: under TSqlInsert
- replay DmlWhere: under TSqlDelete
- replay DottedType: under ?
- replay DropBody: under DropStatement
- replay DropIndexName: under DropBody
- replay DropIndexOption: under DropIndexOptions
- replay DropIndexOptions: under ?
- replay DropLedName: under DropBody
- replay DropLedNames: under DropBody
- replay DropName: under DropBody
- replay DropNames: under DropBody
- replay DropOnePart: under DropBody
- replay DropOneParts: under DropBody
- replay DropOption: under DropOptions
- replay DropOptions: under ?
- replay DropStatement: under DefinitionStatement
- replay DropTarget: under AlterTableAction
- replay EdgePair: under ConstraintBody
- replay EditionOption: under EditionOptions
- replay EditionOptions: under CreateDatabaseStatement
- replay ElseBranch: under ConditionalStatement
- replay EncryptionOption: under ColumnTrait
- replay EndConversationWith: under ConversationStatement
- replay EndpointAffinity: under EndpointState
- replay EndpointAs: under AuditStatement
- replay EndpointAuthentication: under EndpointBrokerOption
- replay EndpointBrokerOption: under EndpointBrokerOptions
- replay EndpointBrokerOptions: under EndpointFor
- replay EndpointEncryption: under EndpointTsqlOptions
- replay EndpointFor: under AuditStatement
- replay EndpointHttpOption: under EndpointAs
- replay EndpointMirroringOption: under EndpointMirroringOptions
- replay EndpointMirroringOptions: under EndpointFor
- replay EndpointSetting: under EndpointState
- replay EndpointSoapOption: under EndpointSoapOptions
- replay EndpointSoapOptions: under EndpointFor
- replay EndpointState: under AuditStatement
- replay EndpointTcpOption: under EndpointAs
- replay EndpointTsqlOptions: under EndpointFor
- replay Enforced: under ConstraintBody
- replay EventActions: under EventBody
- replay EventAdd: under EventAdds
- replay EventAdds: under AuditStatement
- replay EventAlterations: under AuditStatement
- replay EventDrop: under EventAlterations
- replay EventPredicate: under EventBody
- replay EventSessionOption: under EventSessionWith
- replay EventSessionState: under AuditStatement
- replay EventSessionWith: under AuditStatement
- replay EventSetting: under EventSet
- replay ExactNumber: under TranName
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
- replay ExecuteAsWho: under RoutineOption
- replay ExecuteBody: under ExecuteStatement
- replay ExecuteStatement: under InsertRows
- replay ExternalStatement: under DefinitionStatement
- replay ExternalTableItem: under ExternalTableWith
- replay ExternalTableOption: under ExternalTableItem
- replay FetchClause: under OffsetFetch
- replay FetchInto: under CursorStatement
- replay FileGroupSpec: under DatabaseFiles
- replay FileOption: under FileSpec
- replay FileSpec: under DatabaseFiles
- replay FilestreamOption: under DatabaseSetting
- replay ForClause: under TSqlSubquery
- replay ForcedNest: under DatabaseSetting
- replay ForcedOption: under ForcedNest
- replay FromClause: under TSqlQuerySpecification
- replay FullTextColumns: under FullTextPredicate
- replay FullTextOption: under FullTextOptions
- replay FullTextOptions: under ?
- replay FullTextPredicate: under TSqlPredicate
- replay FullTextSearch: under RowsetFunction
- replay FullTextStatement: under DefinitionStatement
- replay FunctionCall: under TSqlPrimaryCore
- replay FunctionReturns: under FunctionStatement
- replay FunctionStatement: under CreateRoutine
- replay GetConversationGroup: under SimpleCommand
- replay Grantee: under Grantees
- replay Grantees: under PermissionStatement
- replay GraphMatch: under TSqlPredicate
- replay GroupByExpression: under GroupingSet
- replay GroupList: under GroupingSet
- replay GroupWith: under TSqlGroupByClause
- replay HavingClause: under TSqlQuerySpecification
- replay IdentityFunction: under TSqlSelectSublist
- replay IdentityNumber: under IdentityFunction
- replay InPredicateValue: under NegatablePredicate
- replay IncrementalNest: under DatabaseSetting
- replay IndexColumn: under IndexColumns
- replay IndexColumns: under ConstraintBody
- replay IndexName: under IncludeColumns
- replay IndexOn: under ConstraintBody
- replay IndexOption: under IndexWith
- replay IndexTarget: under AlterIndexStatement
- replay IndexWith: under CreateIndexStatement
- replay InlineReturn: under FunctionReturns
- replay InsertColumn: under InsertColumns
- replay Into: under TSqlQuerySpecification
- replay JoinedTail: under JoinedRight
- replay JsonArrayBody: under TSqlValueFunction
- replay JsonDirective: under ForClause
- replay JsonFunction: under TSqlTablePrimary
- replay JsonIndexOption: under JsonIndexWith
- replay JsonIndexWith: under CreateIndexStatement
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
- replay KeyPasswordSetting: under AsymmetricChange
- replay KeyStatement: under DefinitionStatement
- replay KillNumber: under MaintenanceStatement
- replay KillSubscription: under MaintenanceStatement
- replay KillWith: under MaintenanceStatement
- replay LanguageOption: under LanguageFile
- replay LanguagePlatform: under CodeStatement
- replay LanguageValue: under DatabaseSetting
- replay LeftRightCall: under TSqlPrimaryCore
- replay LevelOrDefault: under PriorityOption
- replay LocalVariable: under FetchRow
- replay LogBufferNest: under DatabaseSetting
- replay LoopStatement: under SqlPiece
- replay MaintenanceStatement: under DefinitionStatement
- replay ManualCutover: under AlterDatabaseStatement
- replay MaskOption: under ColumnTrait
- replay Member: under TSqlValuePrimary
- replay MergeArm: under TSqlMerge
- replay MergeChange: under MergeArm
- replay MergeColumn: under MergeColumns
- replay MergeColumns: under MergeInsert
- replay MergeInsertRows: under MergeInsert
- replay MergeInsert: under MergeArm
- replay MessageTypeName: under ConversationStatement
- replay MethodCallTail: under Assignment
- replay ModelOption: under ModelOptions
- replay NameOrAny: under PriorityOption
- replay NamedConstraint: under ColumnTrait
- replay NamedConstraint: under ColumnTrait
- replay NamedConstraint: under ColumnTrait
- replay NegatablePredicate: under PredicateTail
- replay NextValue: under TSqlPrimaryCore
- replay NullSpec: under PredictColumn
- replay OdbcEscape: under TSqlPrimaryCore
- replay OdbcFunction: under OdbcEscape
- replay OldIndexOption: under IndexWith
- replay OnPartitions: under OptionSetting
- replay OpenQueryCall: under RowsetFunction
- replay OptionNest: under OptionTail
- replay OptionSetting: under OptionList
- replay OptionTail: under OptionSetting
- replay OptionUnit: under OptionValue
- replay OptionValue: under OptionSetting
- replay OutputItem: under OutputList
- replay Over: under CallTail
- replay ParameterTail: under RoutineParameter
- replay ParameterWay: under ParameterTail
- replay PartitionRebuildOption: under PartitionRebuildWith
- replay PartitionRebuildWith: under AlterIndexAction
- replay PartitionSpan: under TruncatePartitions
- replay PartitionStatement: under DefinitionStatement
- replay PartitionWhich: under AlterTableAction
- replay PermissionAs: under PermissionStatement
- replay PermissionOn: under PermissionStatement
- replay PermissionStatement: under DefinitionStatement
- replay Permission: under Permissions
- replay Permissions: under PermissionStatement
- replay PivotName: under PivotNames
- replay PivotNames: under Pivot
- replay PivotSuffix: under TSqlTableReference
- replay Pivot: under PivotSuffix
- replay PlanHandle: under ScopedConfiguration
- replay PredicateTail: under TSqlPredicate
- replay PredictColumn: under PredictSchema
- replay PredictSchema: under TSqlTablePrimary
- replay PrincipalName: under Grantee
- replay PrincipalStatement: under DefinitionStatement
- replay PriorityOption: under PriorityOptions
- replay PrivateKeyList: under CertificateSource
- replay PrivateKeySetting: under PrivateKeyList
- replay ProcedureStatement: under CreateRoutine
- replay PromotedPath: under SelectivePath
- replay ProviderKeySetting: under AsymmetricBody
- replay ProviderSetting: under SymmetricBody
- replay QueryHint: under QueryHints
- replay QueryPrimary: under TSqlQueryExpression
- replay QueueOption: under QueueWith
- replay RebuildLogOption: under DatabaseTail
- replay RebuildOption: under RebuildWith
- replay RebuildWith: under AlterTableAction
- replay ReceiveColumn: under ReceiveStatement
- replay ReceiveStatement: under SimpleCommand
- replay ReceiveTop: under ReceiveStatement
- replay ReceiveWhere: under ReceiveStatement
- replay RecoveryUnit: under DatabaseSetting
- replay ReferenceAction: under ConstraintBody
- replay ReferenceOn: under References
- replay References: under ConstraintBody
- replay ReorganizeOption: under ReorganizeWith
- replay ReorganizeWith: under AlterIndexAction
- replay RestoreStatement: under DefinitionStatement
- replay RestoreWhat: under RestoreStatement
- replay ResultColumns: under ?
- replay Result: under CaseExpression
- replay ResumeOption: under ResumeWith
- replay ResumeWith: under AlterIndexAction
- replay RevertCookie: under SessionStatement
- replay RouteOption: under RouteOptions
- replay RoutineOption: under RoutineOptions
- replay RoutineOptions: under ProcedureStatement
- replay RoutineParameter: under RoutineParameters
- replay RoutineParameters: under ProcedureStatement
- replay RowsetArguments: under PredictCall
- replay RowsetOrder: under RowsetArgument
- replay SampleUnit: under TableSample
- replay SampledHint: under TSqlTablePrimary
- replay ScalarSubquery: under TSqlPrimaryCore
- replay SchemaCollate: under JsonSchemaColumn
- replay SchemaColumn: under RowsetSchema
- replay SchemaElement: under PrincipalStatement
- replay SchemaOrdinal: under SchemaColumn
- replay SchemaPath: under JsonSchemaColumn
- replay ScopedConfiguration: under AlterDatabaseStatement
- replay ScopedNumber: under ScopedSetting
- replay ScopedSet: under AlterDatabaseStatement
- replay ScopedSetting: under ScopedSet
- replay ScopedSwitchValue: under ScopedSetting
- replay SearchLanguage: under RowsetFunction
- replay SearchTop: under RowsetFunction
- replay SearchedColumn: under FullTextColumns
- replay SearchedWhen: under CaseExpression
- replay SelectivePath: under SelectivePaths
- replay SelectivePaths: under CreateIndexStatement
- replay SemanticArgument: under SemanticArguments
- replay SemanticArguments: under RowsetFunction
- replay SendTargets: under ConversationStatement
- replay SequenceName: under SequenceStatement
- replay SequenceOption: under SequenceStatement
- replay SequenceStatement: under DefinitionStatement
- replay SequenceValue: under SequenceOption
- replay ServerStatement: under DefinitionStatement
- replay SessionKind: under SessionWho
- replay SessionStatement: under DefinitionStatement
- replay SessionUser: under SessionStatement
- replay SessionWho: under SessionStatement
- replay SessionWith: under SessionStatement
- replay SetConstant: under SettingValue
- replay SetExpressions: under SetStatement
- replay SetFipsLevel: under SettingValue
- replay SetFunctionSpecification: under TSqlPrimaryCore
- replay SetIndexOption: under SetIndexOptions
- replay SetIndexOptions: under AlterIndexAction
- replay SetInteger: under SetExpressions
- replay SetNumber: under SettingValue
- replay SetOffset: under SetExpressions
- replay SetReport: under SetExpressions
- replay SetRowCount: under SetExpressions
- replay SetStatement: under DefinitionStatement
- replay SetSwitch: under SetExpressions
- replay SettingValue: under SetExpressions
- replay SignatureCrypto: under DropBody
- replay SignedModule: under DropBody
- replay SimpleCommand: under SqlPiece
- replay SortSpecification: under OrderByClause
- replay SortedData: under BareKeyOption
- replay SourceHints: under TSqlTablePrimary
- replay SpatialColumn: under CreateIndexStatement
- replay SpatialItem: under SpatialSetting
- replay SpatialOption: under SpatialWith
- replay SpatialSet: under SpatialOption
- replay SpatialValue: under SpatialSet
- replay SpatialWith: under CreateIndexStatement
- replay StatementBlock: under SqlPiece
- replay StatementList: under TryCatchStatement
- replay StoreNest: under DatabaseSetting
- replay StoreOption: under StoreNest
- replay StringOrAny: under PriorityOption
- replay SuspendNest: under DatabaseSetting
- replay SwitchTail: under OptionTail
- replay SymmetricBody: under KeyStatement
- replay SymmetricSetting: under SymmetricBody
- replay SynonymName: under TypeStatement
- replay SystemTimeWhen: under SystemTime
- replay SystemTime: under TSqlTablePrimary
- replay TSqlAsClause: under TSqlSelectSublist
- replay TSqlBooleanFactor: under BooleanTerm
- replay TSqlCast: under TSqlPrimaryCore
- replay TSqlDelete: under SqlPiece
- replay TSqlDirectSelect: under SqlPiece
- replay TSqlEscapeClause: under NegatablePredicate
- replay TSqlGroupByClause: under TSqlQuerySpecification
- replay TSqlGrouping: under TSqlGroupByClause
- replay TSqlInsert: under SqlPiece
- replay TSqlMerge: under SqlPiece
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
- replay TSqlUpdate: under SqlPiece
- replay TSqlValueFunction: under TSqlPrimaryCore
- replay TSqlValuePrimary: under TSqlValueExpression
- replay TSqlValueSpecification: under TSqlPrimaryCore
- replay TableBody: under AlterTableAction
- replay TableBody: under AlterTableAction
- replay TableColumnBody: under TableColumn
- replay TableColumnBody: under TableColumn
- replay TableColumnBody: under TableColumn
- replay TableColumn: under TableElement
- replay TableColumn: under TableElement
- replay TableColumn: under TableElement
- replay TableElement: under TableBody
- replay TableElement: under TableBody
- replay TableElement: under TableBody
- replay TableHint: under SampledHint
- replay TableHints: under SourceHints
- replay TableIndexOption: under TableIndexWith
- replay TableIndexWith: under TableIndex
- replay TableIndex: under TableElement
- replay TableOption: under TableWith
- replay TablePlacement: under CreateTableStatement
- replay TableRepeatable: under TableSample
- replay TableSample: under TSqlTablePrimary
- replay TableSearchColumn: under TableSearchColumns
- replay TableSearchColumns: under RowsetFunction
- replay TableTypeBody: under TypeStatement
- replay TableWith: under CreateTableStatement
- replay TargetAdd: under TargetAdds
- replay TargetAdds: under AuditStatement
- replay TargetDrop: under EventAlterations
- replay TextAmount: under TextStatement
- replay TextData: under TextStatement
- replay TextPlace: under TextStatement
- replay TextPointer: under TextStatement
- replay TextStamp: under TextStatement
- replay TextStatement: under DefinitionStatement
- replay TimeZone: under AtTimeZone
- replay ToOrFrom: under PermissionStatement
- replay TopSuffix: under Top
- replay TrackingNest: under DatabaseSetting
- replay TrackingOption: under TrackingNest
- replay TranName: under TransactionStatement
- replay TranWord: under TransactionStatement
- replay TransactionStatement: under SqlPiece
- replay TriggerEvent: under TriggerEvents
- replay TriggerEvents: under TriggerStatement
- replay TriggerName: under TriggerNames
- replay TriggerNames: under MaintenanceStatement
- replay TriggerOn: under TriggerStatement
- replay TriggerScope: under MaintenanceStatement
- replay TriggerStatement: under CreateRoutine
- replay TriggerWhen: under TriggerStatement
- replay TruncatePartitions: under MaintenanceStatement
- replay TryCatchStatement: under SqlPiece
- replay TuningOption: under DatabaseSetting
- replay TypeStatement: under DefinitionStatement
- replay Unpivot: under PivotSuffix
- replay UnsignedLiteral: under TSqlPrimaryCore
- replay UpdateStatisticsWith: under ExternalStatement
- replay UpdateVariableTail: under Assignment
- replay UseModel: under FunctionCall
- replay ValueFunction: under TSqlPrimaryCore
- replay VariableChainTail: under UpdateVariableTail
- replay VariableSource: under TSqlTablePrimary
- replay VersionStoreNest: under DatabaseSetting
- replay ViewStatement: under CreateRoutine
- replay WaitTimeout: under SimpleCommand
- replay WaitValue: under SimpleCommand
- replay WebMethodName: under EndpointSoapOption
- replay WebMethodOption: under WebMethodOptions
- replay WebMethodOptions: under EndpointSoapOption
- replay WhereClause: under TSqlQuerySpecification
- replay WindowDef: under Window
- replay WindowFrame: under WindowSpecification
- replay WindowSpecification: under WindowDef
- replay Window: under TSqlQuerySpecification
- replay WorkWord: under TransactionStatement
- replay XmlDirective: under ForClause
- replay XmlIndexOption: under XmlIndexWith
- replay XmlIndexWith: under CreateIndexStatement
- replay XmlMode: under ForClause
- replay XmlNamespaces: under WithClause

## DotGram.Sql.TransactSql.TransactSqlParser.Located

- replay AlterColumnWord: Follows in AlterTableAction [choice], then when (Syntax.FlagsOnline(flag, options))
- replay Arguments: Follows in Member [turn], then ')'
- replay AssemblyOptions: Follows in CodeStatement [choice], then when (Syntax.Tail(tail) is not null)
- replay AssignOp: Follows in TSqlSelectSublist [choice], then TSqlValueExpression
- replay BackupRedundancy: Follows in DatabaseTail [turn], then ')'
- replay BindingOptions: Follows in BrokerStatement [choice], then when (Syntax.NamedOnce(null, options) && Syntax.HasOption(options, "US…
- replay BrokerString: Follows in AvailabilityMade [turn], then '('
- replay ColumnDefinition: Follows in AlterTableAction [choice], then when (Syntax.AlteredColumn(column))
- replay ColumnKeySetting: Follows in KeyStatement [choice], then ')'
- replay ColumnList: Follows in VariableSource [choice], then ')'
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
- replay OffsetFetch: Follows in InlineReturn [choice], then ')'
- replay OnOff: Follows in TypeStatement [choice], then ')'
- replay OptionList: Follows in SwitchTail [choice], then ')'
- replay OptionsWith: Follows in CreateTableStatement [choice], then "AS"i
- replay OrderByClause: Follows in InlineReturn [choice], then ')'
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
- replay SqlPiece: Lookahead in AtomicBody [lookahead]
- replay TSqlAlias: Follows in TSqlSelectSublist [choice], then '='
- replay TSqlGroupingColumn: Follows in GroupByExpression [choice], then ')'
- replay TSqlJoinType: Follows in TSqlTableReference [turn], then "JOIN"i
- replay TSqlQueryExpression: Follows in InlineReturn [choice], then ')'
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
- replay AlgorithmSetting: under AsymmetricBody
- replay AlterConstraintOption: under AlterConstraintWith
- replay AlterConstraintWith: under ConstraintBody
- replay AlterDatabaseAction: under AlterDatabaseStatement
- replay AlterDatabaseStatement: under DefinitionStatement
- replay AlterIndexAction: under AlterIndexStatement
- replay AlterIndexStatement: under DefinitionStatement
- replay AlterTableAction: under AlterTableStatement
- replay AlterTableStatement: under DefinitionStatement
- replay ArchiveNest: under DatabaseSetting
- replay ArchiveOption: under ArchiveNest
- replay Argument: under Arguments
- replay AssemblyOption: under AssemblyOptions
- replay AssignTail: under MemberTail
- replay AssignedValue: under VariableChainTail
- replay Assignment: under Assignments
- replay Assignments: under TSqlUpdate
- replay AsymmetricBody: under KeyStatement
- replay AsymmetricChange: under KeyStatement
- replay AtTimeZone: under TSqlValuePrimary
- replay AtomicBody: under ProcedureStatement
- replay AtomicOption: under AtomicOptions
- replay AtomicOptions: under AtomicBody
- replay AttachOption: under DatabaseTail
- replay AuditStatement: under DefinitionStatement
- replay AvailabilityStatement: under DefinitionStatement
- replay BackupGroup: under BackupStatement
- replay BackupName: under BackupGroup
- replay BackupStatement: under DefinitionStatement
- replay BackupTarget: under BackupStatement
- replay BackupWhat: under BackupStatement
- replay BareIndexWord: under OldIndexOption
- replay BareKeyOption: under BareKeyOptions
- replay BareKeyOptions: under ConstraintWith
- replay BindingOption: under BindingOptions
- replay BooleanPrimary: under BooleanTest
- replay BooleanTerm: under SearchCondition
- replay BooleanTest: under TSqlBooleanFactor
- replay Branch: under ConditionalStatement
- replay BrokerStatement: under DefinitionStatement
- replay BrokerTarget: under ReceiveWhere
- replay BulkColumnTail: under BulkColumn
- replay BulkColumn: under BulkColumns
- replay BulkColumns: under BulkInsertStatement
- replay BulkDigits: under BulkOption
- replay BulkFileType: under BulkOption
- replay BulkFile: under BulkInsertStatement
- replay BulkFormat: under BulkOption
- replay BulkInsertStatement: under DefinitionStatement
- replay BulkNumber: under BulkOption
- replay BulkOption: under BulkWith
- replay BulkOrder: under BulkOption
- replay BulkStreamOption: under BulkStreamWith
- replay BulkStreamWith: under BulkInsertStatement
- replay BulkString: under BulkOption
- replay BulkTarget: under BulkInsertStatement
- replay BulkWith: under BulkInsertStatement
- replay CallTail: under TSqlPrimaryCore
- replay CaseExpression: under TSqlPrimaryCore
- replay CastOperand: under TSqlCast
- replay CertificateBody: under KeyStatement
- replay CertificateChange: under KeyStatement
- replay CertificateKeyItem: under CertificateKeyItems
- replay CertificateKeyItems: under ?
- replay CertificateSetting: under CertificateBody
- replay CertificateSource: under CertificateBody
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
- replay ClassificationOption: under MaintenanceStatement
- replay ClassifiedColumn: under MaintenanceStatement
- replay CodeStatement: under DefinitionStatement
- replay Collate: under TSqlValueExpression
- replay ColumnBody: under ColumnDefinition
- replay ColumnBody: under TableColumnBody
- replay ColumnBody: under TableColumnBody
- replay ColumnKeyValue: under KeyStatement
- replay ColumnName: under ColumnList
- replay ColumnReference: under TSqlPrimaryCore
- replay ColumnTail: under TableColumnBody
- replay ColumnTail: under TableColumnBody
- replay ColumnTail: under TableColumnBody
- replay ColumnTrait: under ColumnTail
- replay ColumnTrait: under ColumnTail
- replay ColumnTrait: under ColumnTail
- replay ColumnstoreOption: under ColumnstoreWith
- replay ColumnstoreWith: under CreateIndexStatement
- replay ConditionalStatement: under SqlPiece
- replay ConstraintBody: under TableElement
- replay ConstraintBody: under TableElement
- replay ConstraintOption: under ConstraintWith
- replay ConstraintWith: under ConstraintBody
- replay ConversationError: under EndConversationWith
- replay ConversationStatement: under SqlPiece
- replay CountStar: under TSqlPrimaryCore
- replay CreateDatabaseOption: under CreateDatabaseStatement
- replay CreateDatabaseStatement: under DefinitionStatement
- replay CreateIndexStatement: under DefinitionStatement
- replay CreateRoutine: under DefinitionStatement
- replay CreateStatisticsWith: under ExternalStatement
- replay CreateTableStatement: under DefinitionStatement
- replay CteBody: under CteDefinition
- replay CteDefinition: under WithClause
- replay CursorColumn: under CursorColumns
- replay CursorColumns: under CursorFor
- replay CursorDefinition: under DeclareStatement
- replay CursorInPlace: under SetStatement
- replay CursorIsoWord: under CursorDefinition
- replay CursorName: under CursorStatement
- replay CursorOption: under CursorDefinition
- replay CursorQuery: under CursorDefinition
- replay CursorStatement: under SqlPiece
- replay DataSourceChange: under DataSourceBody
- replay DataSourceItem: under DataSourceBody
- replay DataSourceOption: under DataSourceItem
- replay DatabaseAdd: under AlterDatabaseAction
- replay DatabaseDecimal: under DatabaseSetting
- replay DatabaseFiles: under CreateDatabaseStatement
- replay DatabaseInteger: under DatabaseSetting
- replay DatabaseLogFiles: under DatabaseFiles
- replay DatabaseModify: under AlterDatabaseAction
- replay DatabaseRemove: under AlterDatabaseAction
- replay DatabaseSetting: under CreateDatabaseOption
- replay DatabaseTail: under CreateDatabaseStatement
- replay DatabaseTermination: under AlterDatabaseStatement
- replay DatabaseText: under DatabaseSetting
- replay DatePartCall: under TSqlPrimaryCore
- replay DatePartFunction: under DatePartCall
- replay DbccArgument: under DbccArguments
- replay DbccArguments: under DbccStatement
- replay DbccFirst: under DbccOptions
- replay DbccOption: under DbccOptions
- replay DbccOptions: under DbccStatement
- replay DbccStatement: under SqlPiece
- replay DbccValue: under ExecValue
- replay Declaration: under DeclareStatement
- replay DeclareStatement: under DefinitionStatement
- replay DeclaredAs: under Declaration
- replay DefinitionStatement: under SqlPiece
- replay DialogOption: under DialogWith
- replay DialogWith: under ConversationStatement
- replay DistinctTail: under TSqlPredicate
- replay DmlCall: under TSqlInsert
- replay DmlWhere: under TSqlDelete
- replay DottedType: under ?
- replay DropBody: under DropStatement
- replay DropIndexName: under DropBody
- replay DropIndexOption: under DropIndexOptions
- replay DropIndexOptions: under ?
- replay DropLedName: under DropBody
- replay DropLedNames: under DropBody
- replay DropName: under DropBody
- replay DropNames: under DropBody
- replay DropOnePart: under DropBody
- replay DropOneParts: under DropBody
- replay DropOption: under DropOptions
- replay DropOptions: under ?
- replay DropStatement: under DefinitionStatement
- replay DropTarget: under AlterTableAction
- replay EdgePair: under ConstraintBody
- replay EditionOption: under EditionOptions
- replay EditionOptions: under CreateDatabaseStatement
- replay ElseBranch: under ConditionalStatement
- replay EncryptionOption: under ColumnTrait
- replay EndConversationWith: under ConversationStatement
- replay EndpointAffinity: under EndpointState
- replay EndpointAs: under AuditStatement
- replay EndpointAuthentication: under EndpointBrokerOption
- replay EndpointBrokerOption: under EndpointBrokerOptions
- replay EndpointBrokerOptions: under EndpointFor
- replay EndpointEncryption: under EndpointTsqlOptions
- replay EndpointFor: under AuditStatement
- replay EndpointHttpOption: under EndpointAs
- replay EndpointMirroringOption: under EndpointMirroringOptions
- replay EndpointMirroringOptions: under EndpointFor
- replay EndpointSetting: under EndpointState
- replay EndpointSoapOption: under EndpointSoapOptions
- replay EndpointSoapOptions: under EndpointFor
- replay EndpointState: under AuditStatement
- replay EndpointTcpOption: under EndpointAs
- replay EndpointTsqlOptions: under EndpointFor
- replay Enforced: under ConstraintBody
- replay EventActions: under EventBody
- replay EventAdd: under EventAdds
- replay EventAdds: under AuditStatement
- replay EventAlterations: under AuditStatement
- replay EventDrop: under EventAlterations
- replay EventPredicate: under EventBody
- replay EventSessionOption: under EventSessionWith
- replay EventSessionState: under AuditStatement
- replay EventSessionWith: under AuditStatement
- replay EventSetting: under EventSet
- replay ExactNumber: under TranName
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
- replay ExecuteAsWho: under RoutineOption
- replay ExecuteBody: under ExecuteStatement
- replay ExecuteStatement: under InsertRows
- replay ExternalStatement: under DefinitionStatement
- replay ExternalTableItem: under ExternalTableWith
- replay ExternalTableOption: under ExternalTableItem
- replay FetchClause: under OffsetFetch
- replay FetchInto: under CursorStatement
- replay FileGroupSpec: under DatabaseFiles
- replay FileOption: under FileSpec
- replay FileSpec: under DatabaseFiles
- replay FilestreamOption: under DatabaseSetting
- replay ForClause: under TSqlSubquery
- replay ForcedNest: under DatabaseSetting
- replay ForcedOption: under ForcedNest
- replay FromClause: under TSqlQuerySpecification
- replay FullTextColumns: under FullTextPredicate
- replay FullTextOption: under FullTextOptions
- replay FullTextOptions: under ?
- replay FullTextPredicate: under TSqlPredicate
- replay FullTextSearch: under RowsetFunction
- replay FullTextStatement: under DefinitionStatement
- replay FunctionCall: under TSqlPrimaryCore
- replay FunctionReturns: under FunctionStatement
- replay FunctionStatement: under CreateRoutine
- replay GetConversationGroup: under SimpleCommand
- replay Grantee: under Grantees
- replay Grantees: under PermissionStatement
- replay GraphMatch: under TSqlPredicate
- replay GroupByExpression: under GroupingSet
- replay GroupList: under GroupingSet
- replay GroupWith: under TSqlGroupByClause
- replay HavingClause: under TSqlQuerySpecification
- replay IdentityFunction: under TSqlSelectSublist
- replay IdentityNumber: under IdentityFunction
- replay InPredicateValue: under NegatablePredicate
- replay IncrementalNest: under DatabaseSetting
- replay IndexColumn: under IndexColumns
- replay IndexColumns: under ConstraintBody
- replay IndexName: under IncludeColumns
- replay IndexOn: under ConstraintBody
- replay IndexOption: under IndexWith
- replay IndexTarget: under AlterIndexStatement
- replay IndexWith: under CreateIndexStatement
- replay InlineReturn: under FunctionReturns
- replay InsertColumn: under InsertColumns
- replay Into: under TSqlQuerySpecification
- replay JoinedTail: under JoinedRight
- replay JsonArrayBody: under TSqlValueFunction
- replay JsonDirective: under ForClause
- replay JsonFunction: under TSqlTablePrimary
- replay JsonIndexOption: under JsonIndexWith
- replay JsonIndexWith: under CreateIndexStatement
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
- replay KeyPasswordSetting: under AsymmetricChange
- replay KeyStatement: under DefinitionStatement
- replay KillNumber: under MaintenanceStatement
- replay KillSubscription: under MaintenanceStatement
- replay KillWith: under MaintenanceStatement
- replay LanguageOption: under LanguageFile
- replay LanguagePlatform: under CodeStatement
- replay LanguageValue: under DatabaseSetting
- replay LeftRightCall: under TSqlPrimaryCore
- replay LevelOrDefault: under PriorityOption
- replay LocalVariable: under FetchRow
- replay LogBufferNest: under DatabaseSetting
- replay LoopStatement: under SqlPiece
- replay MaintenanceStatement: under DefinitionStatement
- replay ManualCutover: under AlterDatabaseStatement
- replay MaskOption: under ColumnTrait
- replay Member: under TSqlValuePrimary
- replay MergeArm: under TSqlMerge
- replay MergeChange: under MergeArm
- replay MergeColumn: under MergeColumns
- replay MergeColumns: under MergeInsert
- replay MergeInsertRows: under MergeInsert
- replay MergeInsert: under MergeArm
- replay MessageTypeName: under ConversationStatement
- replay MethodCallTail: under Assignment
- replay ModelOption: under ModelOptions
- replay NameOrAny: under PriorityOption
- replay NamedConstraint: under ColumnTrait
- replay NamedConstraint: under ColumnTrait
- replay NamedConstraint: under ColumnTrait
- replay NegatablePredicate: under PredicateTail
- replay NextValue: under TSqlPrimaryCore
- replay NullSpec: under PredictColumn
- replay OdbcEscape: under TSqlPrimaryCore
- replay OdbcFunction: under OdbcEscape
- replay OldIndexOption: under IndexWith
- replay OnPartitions: under OptionSetting
- replay OpenQueryCall: under RowsetFunction
- replay OptionNest: under OptionTail
- replay OptionSetting: under OptionList
- replay OptionTail: under OptionSetting
- replay OptionUnit: under OptionValue
- replay OptionValue: under OptionSetting
- replay OutputItem: under OutputList
- replay Over: under CallTail
- replay ParameterTail: under RoutineParameter
- replay ParameterWay: under ParameterTail
- replay PartitionRebuildOption: under PartitionRebuildWith
- replay PartitionRebuildWith: under AlterIndexAction
- replay PartitionSpan: under TruncatePartitions
- replay PartitionStatement: under DefinitionStatement
- replay PartitionWhich: under AlterTableAction
- replay PermissionAs: under PermissionStatement
- replay PermissionOn: under PermissionStatement
- replay PermissionStatement: under DefinitionStatement
- replay Permission: under Permissions
- replay Permissions: under PermissionStatement
- replay PivotName: under PivotNames
- replay PivotNames: under Pivot
- replay PivotSuffix: under TSqlTableReference
- replay Pivot: under PivotSuffix
- replay PlanHandle: under ScopedConfiguration
- replay PredicateTail: under TSqlPredicate
- replay PredictColumn: under PredictSchema
- replay PredictSchema: under TSqlTablePrimary
- replay PrincipalName: under Grantee
- replay PrincipalStatement: under DefinitionStatement
- replay PriorityOption: under PriorityOptions
- replay PrivateKeyList: under CertificateSource
- replay PrivateKeySetting: under PrivateKeyList
- replay ProcedureStatement: under CreateRoutine
- replay PromotedPath: under SelectivePath
- replay ProviderKeySetting: under AsymmetricBody
- replay ProviderSetting: under SymmetricBody
- replay QueryHint: under QueryHints
- replay QueryPrimary: under TSqlQueryExpression
- replay QueueOption: under QueueWith
- replay RebuildLogOption: under DatabaseTail
- replay RebuildOption: under RebuildWith
- replay RebuildWith: under AlterTableAction
- replay ReceiveColumn: under ReceiveStatement
- replay ReceiveStatement: under SimpleCommand
- replay ReceiveTop: under ReceiveStatement
- replay ReceiveWhere: under ReceiveStatement
- replay RecoveryUnit: under DatabaseSetting
- replay ReferenceAction: under ConstraintBody
- replay ReferenceOn: under References
- replay References: under ConstraintBody
- replay ReorganizeOption: under ReorganizeWith
- replay ReorganizeWith: under AlterIndexAction
- replay RestoreStatement: under DefinitionStatement
- replay RestoreWhat: under RestoreStatement
- replay ResultColumns: under ?
- replay Result: under CaseExpression
- replay ResumeOption: under ResumeWith
- replay ResumeWith: under AlterIndexAction
- replay RevertCookie: under SessionStatement
- replay RouteOption: under RouteOptions
- replay RoutineOption: under RoutineOptions
- replay RoutineOptions: under ProcedureStatement
- replay RoutineParameter: under RoutineParameters
- replay RoutineParameters: under ProcedureStatement
- replay RowsetArguments: under PredictCall
- replay RowsetOrder: under RowsetArgument
- replay SampleUnit: under TableSample
- replay SampledHint: under TSqlTablePrimary
- replay ScalarSubquery: under TSqlPrimaryCore
- replay SchemaCollate: under JsonSchemaColumn
- replay SchemaColumn: under RowsetSchema
- replay SchemaElement: under PrincipalStatement
- replay SchemaOrdinal: under SchemaColumn
- replay SchemaPath: under JsonSchemaColumn
- replay ScopedConfiguration: under AlterDatabaseStatement
- replay ScopedNumber: under ScopedSetting
- replay ScopedSet: under AlterDatabaseStatement
- replay ScopedSetting: under ScopedSet
- replay ScopedSwitchValue: under ScopedSetting
- replay SearchLanguage: under RowsetFunction
- replay SearchTop: under RowsetFunction
- replay SearchedColumn: under FullTextColumns
- replay SearchedWhen: under CaseExpression
- replay SelectivePath: under SelectivePaths
- replay SelectivePaths: under CreateIndexStatement
- replay SemanticArgument: under SemanticArguments
- replay SemanticArguments: under RowsetFunction
- replay SendTargets: under ConversationStatement
- replay SequenceName: under SequenceStatement
- replay SequenceOption: under SequenceStatement
- replay SequenceStatement: under DefinitionStatement
- replay SequenceValue: under SequenceOption
- replay ServerStatement: under DefinitionStatement
- replay SessionKind: under SessionWho
- replay SessionStatement: under DefinitionStatement
- replay SessionUser: under SessionStatement
- replay SessionWho: under SessionStatement
- replay SessionWith: under SessionStatement
- replay SetConstant: under SettingValue
- replay SetExpressions: under SetStatement
- replay SetFipsLevel: under SettingValue
- replay SetFunctionSpecification: under TSqlPrimaryCore
- replay SetIndexOption: under SetIndexOptions
- replay SetIndexOptions: under AlterIndexAction
- replay SetInteger: under SetExpressions
- replay SetNumber: under SettingValue
- replay SetOffset: under SetExpressions
- replay SetReport: under SetExpressions
- replay SetRowCount: under SetExpressions
- replay SetStatement: under DefinitionStatement
- replay SetSwitch: under SetExpressions
- replay SettingValue: under SetExpressions
- replay SignatureCrypto: under DropBody
- replay SignedModule: under DropBody
- replay SimpleCommand: under SqlPiece
- replay SortSpecification: under OrderByClause
- replay SortedData: under BareKeyOption
- replay SourceHints: under TSqlTablePrimary
- replay SpatialColumn: under CreateIndexStatement
- replay SpatialItem: under SpatialSetting
- replay SpatialOption: under SpatialWith
- replay SpatialSet: under SpatialOption
- replay SpatialValue: under SpatialSet
- replay SpatialWith: under CreateIndexStatement
- replay StatementBlock: under SqlPiece
- replay StatementList: under TryCatchStatement
- replay StoreNest: under DatabaseSetting
- replay StoreOption: under StoreNest
- replay StringOrAny: under PriorityOption
- replay SuspendNest: under DatabaseSetting
- replay SwitchTail: under OptionTail
- replay SymmetricBody: under KeyStatement
- replay SymmetricSetting: under SymmetricBody
- replay SynonymName: under TypeStatement
- replay SystemTimeWhen: under SystemTime
- replay SystemTime: under TSqlTablePrimary
- replay TSqlAsClause: under TSqlSelectSublist
- replay TSqlBooleanFactor: under BooleanTerm
- replay TSqlCast: under TSqlPrimaryCore
- replay TSqlDelete: under SqlPiece
- replay TSqlDirectSelect: under SqlPiece
- replay TSqlEscapeClause: under NegatablePredicate
- replay TSqlGroupByClause: under TSqlQuerySpecification
- replay TSqlGrouping: under TSqlGroupByClause
- replay TSqlInsert: under SqlPiece
- replay TSqlMerge: under SqlPiece
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
- replay TSqlUpdate: under SqlPiece
- replay TSqlValueFunction: under TSqlPrimaryCore
- replay TSqlValuePrimary: under TSqlValueExpression
- replay TSqlValueSpecification: under TSqlPrimaryCore
- replay TableBody: under AlterTableAction
- replay TableBody: under AlterTableAction
- replay TableColumnBody: under TableColumn
- replay TableColumnBody: under TableColumn
- replay TableColumnBody: under TableColumn
- replay TableColumn: under TableElement
- replay TableColumn: under TableElement
- replay TableColumn: under TableElement
- replay TableElement: under TableBody
- replay TableElement: under TableBody
- replay TableElement: under TableBody
- replay TableHint: under SampledHint
- replay TableHints: under SourceHints
- replay TableIndexOption: under TableIndexWith
- replay TableIndexWith: under TableIndex
- replay TableIndex: under TableElement
- replay TableOption: under TableWith
- replay TablePlacement: under CreateTableStatement
- replay TableRepeatable: under TableSample
- replay TableSample: under TSqlTablePrimary
- replay TableSearchColumn: under TableSearchColumns
- replay TableSearchColumns: under RowsetFunction
- replay TableTypeBody: under TypeStatement
- replay TableWith: under CreateTableStatement
- replay TargetAdd: under TargetAdds
- replay TargetAdds: under AuditStatement
- replay TargetDrop: under EventAlterations
- replay TextAmount: under TextStatement
- replay TextData: under TextStatement
- replay TextPlace: under TextStatement
- replay TextPointer: under TextStatement
- replay TextStamp: under TextStatement
- replay TextStatement: under DefinitionStatement
- replay TimeZone: under AtTimeZone
- replay ToOrFrom: under PermissionStatement
- replay TopSuffix: under Top
- replay TrackingNest: under DatabaseSetting
- replay TrackingOption: under TrackingNest
- replay TranName: under TransactionStatement
- replay TranWord: under TransactionStatement
- replay TransactionStatement: under SqlPiece
- replay TriggerEvent: under TriggerEvents
- replay TriggerEvents: under TriggerStatement
- replay TriggerName: under TriggerNames
- replay TriggerNames: under MaintenanceStatement
- replay TriggerOn: under TriggerStatement
- replay TriggerScope: under MaintenanceStatement
- replay TriggerStatement: under CreateRoutine
- replay TriggerWhen: under TriggerStatement
- replay TruncatePartitions: under MaintenanceStatement
- replay TryCatchStatement: under SqlPiece
- replay TuningOption: under DatabaseSetting
- replay TypeStatement: under DefinitionStatement
- replay Unpivot: under PivotSuffix
- replay UnsignedLiteral: under TSqlPrimaryCore
- replay UpdateStatisticsWith: under ExternalStatement
- replay UpdateVariableTail: under Assignment
- replay UseModel: under FunctionCall
- replay ValueFunction: under TSqlPrimaryCore
- replay VariableChainTail: under UpdateVariableTail
- replay VariableSource: under TSqlTablePrimary
- replay VersionStoreNest: under DatabaseSetting
- replay ViewStatement: under CreateRoutine
- replay WaitTimeout: under SimpleCommand
- replay WaitValue: under SimpleCommand
- replay WebMethodName: under EndpointSoapOption
- replay WebMethodOption: under WebMethodOptions
- replay WebMethodOptions: under EndpointSoapOption
- replay WhereClause: under TSqlQuerySpecification
- replay WindowDef: under Window
- replay WindowFrame: under WindowSpecification
- replay WindowSpecification: under WindowDef
- replay Window: under TSqlQuerySpecification
- replay WorkWord: under TransactionStatement
- replay XmlDirective: under ForClause
- replay XmlIndexOption: under XmlIndexWith
- replay XmlIndexWith: under CreateIndexStatement
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
- again Grandfathered: opens a way
- open Grandfathered: choice; an ignore-case literal; open; ("i-ami"i | "i-bnn"i | "i-default"i | "i-enochian"i | "i-hak"i | "i-kl…
- open Grandfathered: choice; an ignore-case literal; open; ("no-bok"i | "no-nyn"i)
- open Grandfathered: choice; an ignore-case literal; open; ("sgn-BE-FR"i | "sgn-BE-NL"i | "sgn-CH-DE"i)
- open Grandfathered: choice; an ignore-case literal; open; ("zh-guoyu"i | "zh-hakka"i | "zh-min-nan"i | "zh-min"i | "zh-xiang"i)
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
- again DispositionParm: through ParmValue
- again DispositionParms: opens a way
- open DispositionParms: turns, captured; a turn led by what may read nothing; open; items: DispositionParm*
- again ParmValue: through QuotedString
- again QuotedString: opens a way
- open QuotedString: choice; alternatives begin apart; open; (['\t' | ' '..'!' | '#'..'[' | ']'..'~' | ''..'ÿ'] | '\\' & ['\t' | '…

## DotGram.Web.Rfc6570

- again LiteralChar: opens a way
- open LiteralChar: choice; alternatives begin apart; open; (['!' | '#'..'$' | '&'..';' | '=' | '?'..'[' | ']' | '_' | 'a'..'z' | …
- again Literals: opens a way
- open Literals: turns; what follows begins alike; open; (LiteralChar | PctEncoded)+
- open Literals: choice; alternatives begin apart; open; (LiteralChar | PctEncoded)
- again Template: opens a way
- open Template: choice; alternatives begin apart; entry; (parts: Expression | parts: Literals)

## DotGram.Web.Rfc6901

- again Pointer: through ReferenceToken
- again ReferenceToken: through TokenText
- again TokenText: opens a way
- open TokenText: choice; alternatives begin apart; open; ([^ '/' | '~'] | '~' & ['0'..'1'])

## DotGram.Web.Rfc7239

- again DecOctet: opens a way
- open DecOctet: choice; alternatives begin alike; open; ('1' & Digit & Digit | ['1'..'9'] & Digit | Digit)
- open DecOctet: choice; alternatives begin alike; open; ("25" & ['0'..'5'] | '2' & ['0'..'4'] & Digit | ['1'..'9'] & Digit | D…
- open DecOctet: choice; alternatives begin alike; open; (['1'..'9'] & Digit | Digit)
- again Element: through PairList
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
- again NextPairSlot: through PairSlot
- again Node: through IPv4Address
- again Ows: opens a way
- open Ows: run; what follows begins alike; open; ['\t' | ' ']*
- again PairList: through PairSlot
- again PairSlot: through QuotedString
- again QuotedString: opens a way
- open QuotedString: choice; alternatives begin apart; open; (['\t' | ' '..'!' | '#'..'[' | ']'..'~' | ''..'ÿ'] | '\\' & ['\t' | '…

## DotGram.Web.Rfc8259

- again ArrayBody: through Items
- again Element: through Value
- again Items: through Element
- again JsonText: through Value
- again Member: through Value
- again Members: through Member
- again NextElement: through Element
- again NextMember: through Member
- again ObjectBody: through Members
- again StringText: opens a way
- open StringText: choice; alternatives begin apart; open; ([' '..'!' | '#'..'[' | ']'..'￿'] | '\\' & (['"' | '/' | '\\' | 'b' | …
- again Value: through ObjectBody

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
- again QuotedString: opens a way
- open QuotedString: choice; alternatives begin apart; open; (['\t' | ' '..'!' | '#'..'[' | ']'..'~' | ''..'ÿ'] | '\\' & ['\t' | '…
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
- again QuotedString: opens a way
- open QuotedString: choice; alternatives begin apart; open; (['\t' | ' '..'!' | '#'..'[' | ']'..'~' | ''..'ÿ'] | '\\' & ['\t' | '…
- again Token: opens a way
- open Token: run; what follows begins alike; open; Tchar+

## DotGram.Web.Rfc9651

- again DictMember: opens a way
- open DictMember: choice; an alternative that may read nothing; open; ('=' & value: ListMember | parameters: SfParameters)
- again DictRest: through DictMember
- again DictionaryField: opens a way
- open DictionaryField: turns, captured; a turn led by what may read nothing; entry; rest: DictRest*
- again InnerMember: opens a way
- open InnerMember: choice; an alternative that may read nothing; open; (' '+ | ?=')')
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
- open SfBareItem: choice; literals under a capture or construction; open; ("?1" => (BareItem.Boolean.True) | "?0" => (BareItem.Boolean.False))
- again SfDecimal: opens a way
- open SfDecimal: run; what follows begins alike; open; Digit{1,3}
- again SfInnerList: through SfParameters
- again SfInteger: opens a way
- open SfInteger: run; what follows begins alike; open; Digit{1,15}
- again SfItem: through SfBareItem
- again SfParameters: through Parameter
- again SfToken: opens a way
- open SfToken: run; what follows begins alike; open; ['!' | '#'..'\'' | '*'..'+' | '-'..':' | 'A'..'Z' | '^'..'z' | '|' | '…
