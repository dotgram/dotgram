# Handwritten parsers

A shared library of manually implemented parsers for differential testing and
performance comparisons with DotGram parsers. The project is built by the solution
and is not published as a package. Test and benchmark projects reference this
library; test frameworks and benchmark runners stay outside it.

## Existing parsers

- `Fix.HandFixParser`: independent FIX field parser with eager and lazy input APIs.
- `HandExpression`: expression-language lexer and recursive-descent parser. Uses
  the production expression factories and state so comparisons build the same values.
- `HandSqlStandard`: ISO/IEC 9075-2:2023 read by hand, in `Sql/`. Builds the production
  `DotGram.Sql.Ast` tree and is held to `SqlStandardParser` answer for answer.

The public entry points retain their existing names: `Parse`, `Build` and
`LexOnly`, where supported. Their namespace is `DotGram.Handwritten`.

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
  fuzz corpus of the standard's grammar has been put to it — 113,000 lines, clean and
  mutated, over every publication — with nothing differing.

### Two places it mirrors the generated parser rather than the BNF

Both are how the generated parser's construction finds the parts of an introduced string
literal, and both need a delimited name inside the introducer to show at all:

- `_u&".s".x'a'` — the character set is split at every period, the one inside the delimited
  name included, so it is three parts and not two.
- `_u&"'s".x'a'` — the literal's text is taken from the first quote in the token, which is
  the one inside the delimited name.

Where the generated parser is corrected, these go with it.
