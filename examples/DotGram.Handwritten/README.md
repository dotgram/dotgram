# Handwritten parsers

A shared library of manually implemented parsers for differential testing and
performance comparisons with DotGram parsers. The project is built by the solution
and is not published as a package. Test and benchmark projects reference this
library; test frameworks and benchmark runners stay outside it.

## Existing parsers

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

## Next parser: FIX

The handwritten FIX parser will live here and reuse the public Finance field model.
Its comparison contract must cover typed fields, binary length/data pairs, source
locations, SOH and log separators, recovery, character and byte input, and streaming
publication. Compare output and failures before measuring speed. This parser has
not been implemented yet.

`benchmarks/DotGram.HandDeferred` remains a separate experiment comparing deferred
construction strategies, with its own benchmark runner.
