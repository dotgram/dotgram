# .Gram language support

First-class Visual Studio tooling for [.Gram](https://github.com/dotgram/dotgram),
a source generator that compiles grammars into strongly typed C# parsers with no runtime
parser engine.

![A .Gram grammar embedded in C# with syntax highlighting](images/dotgram-tooling.png)

## Write grammars where they belong

Use standalone `.gram` files or embed a grammar directly in a C# `[Gram]` attribute.
The extension understands both forms and keeps navigation connected to the generated C# API.

```csharp
[Gram("""
    Number : @int = digits: ['0'..'9']+ => @(int.Parse(digits))
    parse Number as ParseNumber
    """)]
public static partial class Numbers;
```

## Editor features

- Theme-aware syntax highlighting and live diagnostics
- Completion, signature help, and Quick Info
- Brace matching, folding, and an alphabetical rule navigation list
- Go To Definition, Find All References, reference highlighting, and Rename
- Navigation between grammar declarations and generated C# symbols
- Embedded DSL highlighting and diagnostics through `StringSyntax`
- Responsive background analysis for large grammar files

## Generated parsers, ordinary C#

.Gram performs grammar analysis at build time and emits the parser into your assembly.
The generated code can expose `Parse`, `TryParse`, and `Find` APIs, produce strongly typed
results, specialize one grammar into several parsers, read from `TextReader`, and recover
from malformed record-oriented input.

## Learn more

- [Project and installation](https://github.com/dotgram/dotgram)
- [Language syntax](https://github.com/dotgram/dotgram/blob/main/docs/syntax.md)
- [Documentation index](https://github.com/dotgram/dotgram/blob/main/docs/README.md)
- [Report an issue](https://github.com/dotgram/dotgram/issues)

The extension does not collect telemetry and does not communicate with external services.
