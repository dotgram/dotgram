# Examples

Whole, working parsers, meant to be copied. No test framework and no scaffolding —
each file is a grammar, the class it attaches to, and the code somebody would write
against it. `DotGram.Tests` runs them; nothing here knows that.

| | |
| --- | --- |
| [`UrlExample.cs`](DotGram.Examples/UrlExample.cs) | a URL, after RFC 3986 — captures, optional parts, `find all` |
| [`FeedExample.cs`](DotGram.Examples/FeedExample.cs) | a line-oriented feed — nested rule values, an envelope checked as a whole |

## Taking one

Two things, and nothing else:

```xml
<PackageReference Include="DotGram" Version="0.1.0-alpha"
                  PrivateAssets="all" ExcludeAssets="runtime" />
```

```csharp
[Gram("""
    Digits = ['0'..'9']+
    parse Digits
    """)]
public static partial class Numbers
{
    public static int Sum(string text) => ParseDigits(text).Length;   // ParseDigits is generated here
}
```

The attribute goes on the class you want the parser in, and the methods and types
appear in it — there is nothing to wire up and nothing to name twice. The class must
be `partial`, and every class around it too; `static` is fine. No runtime assembly is
referenced, because there is none: everything the parser needs is generated into your
own compilation.

Give the grammar a class of its own when the generated methods should not be part of
your API — an `internal partial class` beside your public one, which is also the only
way to keep `ParseX` and `TryParseX` out of it.

A grammar long enough to want its own place goes in a `.gram` file instead, named
after the class or given to the attribute, and listed so the generator can see it:

```xml
<AdditionalFiles Include="Numbers.gram" />
```

## Seeing what was generated

Both example projects write it to disk:

```xml
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
<CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)GeneratedFiles</CompilerGeneratedFilesOutputPath>
```

After a build it is under `obj/GeneratedFiles/DotGram/DotGram.Generation.GramGenerator`.

## What these deliberately do not show

Things the specification describes and the compiler does not do yet — a rule
converting its own fields with `: @int` and `=>`, a bad record reported and stepped
over with `recover`, a feed read as one typed sequence. [`docs/status.md`](../docs/status.md)
is the list; where an example works around a gap, it says so at that line.
