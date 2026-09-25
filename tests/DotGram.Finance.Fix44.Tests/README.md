# DotGram.Finance.Fix44.Tests

The FIX field parsers held to the oracle. `tests/DotGram.Finance.Fix44` is a second FIX
4.4 grammar, written as one literal alternative per standard tag, and nothing in it is
shared with `FixGrammar` but the field model. Two readings of the same wire agreeing is
the evidence this project gives.

- `FixOracleTests` reads every standard message with `FixParser`, `HandFixParser` and
  `Fix44Parser`, from a string, a byte array, a `TextReader` and a `Stream`, and asks
  for the same fields: types, values and locations.
- `OracleFieldReaderTests` runs the shared `FixFieldReaderTests` over `Fix44Parser`. The
  same tests run over the generated and the hand parser in `DotGram.Finance.Tests`,
  which is where the file lives. An oracle that behaves differently from the product
  would make the comparison above meaningless.

It is a project of its own because the oracle is slow to build: the Fix44 grammar
compiles into tens of megabytes, about a hundred seconds, and `DotGram.Finance.Tests`
would pay that on every build if it referenced it.

## When to run it

CI runs it on every build, as part of the solution.

Locally, run it before merging a change to any of these:

- `src/DotGram.Finance/Fix/FixGrammar.cs`, `FixField.cs`, `FixFieldBuilder.cs` or
  `FixConvert.cs`, or `src/DotGram.Finance/Fix/Fix44/FixParser.cs`;
- `examples/DotGram.Handwritten/Fix/HandFixParser.cs`;
- the generator, when the change alters what `FixGrammar` compiles into;
- the shared tests, `tests/DotGram.Finance.Tests/FixFieldReaderTests.cs`,
  `FieldParser.cs` or `FixFixtures.cs`.

Otherwise run it when you want the evidence, not by habit.

```powershell
dotnet build tests/DotGram.Finance.Fix44.Tests -c Debug
dotnet tests/DotGram.Finance.Fix44.Tests/bin/Debug/net10.0/DotGram.Finance.Fix44.Tests.dll
```
