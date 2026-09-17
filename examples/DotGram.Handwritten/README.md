# Handwritten parsers

A shared library of manually implemented parsers for differential testing and
performance comparisons with DotGram parsers. The project is built by the solution
and is not published as a package. Test and benchmark projects reference this
library; test frameworks and benchmark runners stay outside it.

## Existing parsers

- `Fix.HandFixParser`: independent FIX field parser with eager and lazy input APIs.
- `HandExpression`: expression-language lexer and recursive-descent parser. Uses
  the production expression factories and state so comparisons build the same values.
- `HandSqlTokens`: SQL search-condition lexer and parser. Builds the production SQL
  tree and is checked by the SQL comparison harness before timing.
- `HandSqlOriginal`: historical, deliberately limited SQL baseline. It does not
  implement the full language and must not be used as a correctness oracle.

The public entry points retain their existing names: `Parse`, `Build` and
`LexOnly`, where supported. Their namespace is `DotGram.Handwritten`.

The library has no generator analyzer reference. References to production libraries
provide shared result models and semantic factories, not generated recognition.

## Validation

The existing comparison harnesses remain in `DotGram.Benchmarks`:

```shell
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --hand 1 1
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --el 1 1
```

These short runs exercise the existing agreement checks; they are not performance
measurements. Use normal benchmark settings for measurements.

## FIX

`DotGram.Handwritten.Fix.HandFixParser` mirrors `FixParser`:

- `Parse` reads SOH-separated fields; `ParseLog` accepts pipes with optional spaces.
- Strings, `ReadOnlySpan<char>` and byte arrays return `FixField[]`.
- `TextReader` and `Stream` return lazy `IEnumerable<FixField>` and leave input open.
- Standard and custom `FixOptions` length/data pairs return a single binary field.
- Locations, an optional final separator, invalid primitive values and recovery as
  `FixField.Invalid` follow the production field contract.
- Stream buffers grow as needed up to `maxRetained` input units, retain the current
  field and lookahead, and reuse storage across fields. Exceeding the limit throws
  `InvalidOperationException`. Positions use `int`, as in the generated parser.

Recognition is handwritten and does not call `FixParser` or the generated grammar.
The implementation shares `FixFactory`, `FixConvert`, `FixOptions` and `FixField`
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

`benchmarks/DotGram.HandDeferred` remains a separate experiment comparing deferred
construction strategies, with its own benchmark runner.
