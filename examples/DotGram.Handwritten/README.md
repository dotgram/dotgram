# Handwritten parsers

A shared library of manually implemented parsers for differential testing and
performance comparisons with DotGram parsers. The project is built by the solution
and is not published as a package. Test and benchmark projects reference this
library; test frameworks and benchmark runners stay outside it.

## Existing parsers

- `Fix.HandFixParser`: independent FIX field parser with eager and lazy input APIs.
- `HandExpression`: the expression language read by hand, a lexer and a recursive
  descent. Calls the production factories and hands the production state the same
  spans, so the two build the same trees; held to `ExpressionParser` answer for answer.
- `HandSqlStandard`: ISO/IEC 9075-2:2023 read by hand, in `Sql/`. Builds the production
  `DotGram.Sql.Ast` tree and is held to `SqlStandardParser` answer for answer.

A hand parser's entry points are named after the generated parser's publications
(`TryParseLambda`, `TryParseLiteral`, …). Their namespace is `DotGram.Handwritten`. Where
a publication takes the language's internal state, as the expression language's do, it is
internal here too and visible to `DotGram.Tests` and `DotGram.Benchmarks`.

The library has no generator analyzer reference. References to production libraries
provide shared result models and semantic factories, not generated recognition.

## Validation

The existing comparison harnesses remain in `DotGram.Benchmarks`:

```shell
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --standard "^query expression" file.sql
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --el 1 1
```

These short runs exercise the existing agreement checks; they are not performance
measurements. Use normal benchmark settings for measurements.

## FIX

`DotGram.Handwritten.Fix.HandFixParser` mirrors `FixParser`:

- `Parse` reads SOH-separated fields; `ParseLog` accepts pipes with optional spaces.
- Strings, `ReadOnlySpan<char>` and byte arrays return `FixField[]`.
- `TextReader` and `Stream` return lazy `IEnumerable<FixField>` and leave input open.
- Standard and custom `FixFieldOptions` length/data pairs return a single binary field.
- Locations, an optional final separator, invalid primitive values and recovery as
  `FixField.Invalid` follow the production field contract.
- Stream buffers grow as needed up to `maxRetained` input units, retain the current
  field and lookahead, and reuse storage across fields. Exceeding the limit throws
  `InvalidOperationException`. Positions use `int`, as in the generated parser.

Recognition is handwritten and does not call `FixParser` or the generated grammar.
The implementation shares `FixFieldFactory`, `FixConvert`, `FixFieldOptions` and `FixField`
through a friend assembly reference. This keeps value construction identical while
comparing recognition independently; it is not an independent test of conversions.
Recovery diagnostics explain the handwritten parser's failure and need not have the
same wording as generator diagnostics. Tests compare error locations and raw data.

Use the existing, explicitly invoked semantic API for message validation and groups:

```csharp
var fields = HandFixParser.Parse(wire);
var message = FixMessages.Build(wire, fields);
```

`HandFixTests` compares fixtures, code values, malformed inputs, binary payloads,
custom dictionaries and buffer boundaries across all input forms. It also checks
lazy consumption, independent enumerations and bounded retention.
`HandFixBenchmarks` compares the same work in both implementations and validates
field output before timing. See the Finance benchmark README for commands.

## SQL:2023

`DotGram.Handwritten.HandSqlStandard` is the yardstick `SqlStandardParser` is measured
against, and the target it is optimized towards. It reads the same language, builds the
same tree and refuses the same input; a ratio against a parser that quietly read less
would say nothing about the generator.

- **Publications keep the generated names**: `ParseLiteral`, `TryParseDataType` and the
  rest, one pair per publication of the grammar. `TryParseX(string, out T)` answers rather
  than throwing, and `ParseX` throws `FormatException`.
- **The whole language**: all forty-two publications of the grammar, from §5's tokens to
  §23's diagnostics, with §6's value expressions and their functions, §7's queries, §8's
  predicates, §11 and §12's schema statements, §14's data change statements and cursors,
  and §16 to §23's control, transaction, connection, session, dynamic and direct
  statements. Written chapter by chapter, in the order the grammar was.
- **No array of tokens.** `SqlCursor` makes one token at a time as the parser asks for it,
  because the generated parser reads characters where it stands and keeps nothing of what
  it passed. A parser that built an array would be paying a cost on one side of the
  comparison only. Going back is a copy of the cursor struct.
- **A reserved word is an integer.** `SqlWord` and `SqlWords.Of` turn a word into one of
  §5.2's 376 reserved words, refusing it by length and by initial letter before anything is
  compared, so a name costs two tests and no comparison. Where the BNF writes a word §5.2
  does not reserve — `FINAL`, `PERMUTE`, `OCTETS`, `WRAPPER` — the comparison is by
  spelling, because that word is a name wherever it is not that key word.
- **The towers are carried, not tried.** The BNF types its value expressions as towers that
  meet only in a primary, so the shape they share is read once and `SqlTowers` says which
  towers the whole still belongs to. That is the same reasoning the generated parser
  carries, because it is a property of the BNF and not of either parser.

### How it is held to the generated parser

- `Both` in `tests/DotGram.Sql.Tests` calls both parsers and asserts the same verdict and
  the same tree, property by property. Every row of `SqlStandardParserTests` and
  `SqlStandardTreeTests` for a production it reads goes through it, so `dotnet test` is the
  check.
- `--standard "^production" file` in DotGram.Benchmarks puts every line of a corpus to both
  and prints what tells them apart; when nothing does, it times the two round-robin. Every
  fuzz corpus of the standard's grammar has been put to it — 143,000 lines, clean and
  mutated, over every publication — with nothing differing.

### Two places it reads less than the BNF

Both are comments, and both are where the generated parser skips a comment with a scanner, which
neither nests without limit nor goes back. This parser reads what that one reads, so that a ratio
is between two readings of one language and not between two languages:

- A comment holds others six deep. A seventh `/*` inside the sixth leaves it unclosed.
- A `/*` inside a comment always opens one. The BNF would also let it be two characters of the
  comment around it, so that `/* a /* b */` is one comment; here it is unclosed.

## The expression language

`DotGram.Handwritten.HandExpression` reads the language `ExpressionParser` reads, as its
grammar stands (docs/design/architecture-decisions.md, D1 and D8).

- **The same publications**: `TryParseLambda` and `TryParseAsciiLambda`, answering with the
  generated parser's own `Match`, and the two it reads again over a window of the text — a
  hole of an interpolated string and the body of a lambda that says no types.
- **The same answers**: the tree, down to what the API prints for a debugger; a refusal at the
  same position, with the same outcome and the same name refused in the state; the same
  exception where the API refuses what the text asked for. The wording of a message is no
  part of it.
- **Built where it is read.** The grammar reads a text whole and builds afterwards, so where
  building throws, the hand parser reads the text again building nothing, from the state
  rolled back to where it began, and a text that does not read is answered as refused. That
  costs a second pass on a text that throws and nothing on one that reads.
- **The body of a lambda that says no types is read without building** the first time,
  because its types are not known yet, and read again by the call it is handed to.

### How it is held to the generated parser

- `Both` in `tests/DotGram.Tests/ExpressionLanguage` puts every text `ExpressionParserTests`
  reads to both parsers, so `dotnet test` is the check.
- `ExpressionHandTests` reads `ExpressionCorpus.Shapes` — every construct of the language
  and its refusals, one list shared with the benchmark — in both spellings of a name, and
  every text one character short of a shape.
- `--el` agrees the two over the corpus before it times anything.

### Where it follows how the generated parser counts, not the language

Three things decide where a refusal is said, and each is the generator's accounting:

- The whole text is cut into tokens before anything is read, so a character no token
  begins with is refused wherever it stands.
- A negative lookahead records nothing, and neither does a turn of a repetition that fails
  at its first token: `case 1:` with no statement after it is refused at the colon.
- A positive lookahead does record: `(int x) => y` is refused at the end, where the look
  for a lambda's `=>` stopped.

If a lexer that makes tokens as they are asked for replaces the first (Q1, D5), where a
refusal is said is decided in `docs/syntax.md` and not by whichever parser is written second.
