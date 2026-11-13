# Setting up the generator

.Gram is a source generator: it runs inside the C# compiler, reads the grammars a project
attaches to its classes, and adds the parsers to the same compilation. This page covers what the
`DotGram` package is, how a project takes it, and what older frameworks need beside it.

## What the package contains

| Path in the package | What it is |
| --- | --- |
| `analyzers/dotnet/cs/DotGram.dll` | the generator, loaded by the compiler as an analyzer |
| `build/DotGram.targets` | the MSBuild properties the generator reads ([MSBuild properties](04-msbuild.md)) |
| `README.md`, `SKILL.md` | the package's front page, and the guide to writing a grammar for a coding agent |

There is no `lib/` folder: nothing of the package is referenced by your code or deployed with
your application. Everything a parser needs is written into your own assembly as C#. The package
has no dependencies.

## Adding it to a project

```xml
<ItemGroup>
  <PackageReference Include="DotGram" Version="0.3.0" PrivateAssets="all" />
</ItemGroup>
```

`PrivateAssets="all"` keeps the generator to this project: a project that references yours gets
your parsers as compiled code, and has no use for the generator that wrote them.

Reference the package in **every project that declares a grammar**. A project that only calls
parsers another project generated needs nothing.

## A first parser

A grammar is attached to a `partial` class with `[Gram]`, and every rule the grammar publishes
becomes a static method of that class:

```csharp example=first
using DotGram;

namespace Demo;

[Gram("""
	Digits = ['0'..'9']+

	parse Digits
	""")]
public static partial class Numbers;
```

```text run example=first
Numbers.ParseDigits("2026") → "2026"
Numbers.TryParseDigits("20x6").IsSuccess → false
```

`ParseDigits` throws `FormatException` on input it refuses; `TryParseDigits` answers a match that
says whether it succeeded, and why not. Nothing else is needed: the attribute, the parser and its
support types are all generated into the project.

Where the class can live, what `[Gram]` and its options say, and grammars kept in files of their
own are the next two pages: [the host and its attributes](02-hosts-and-attributes.md) and
[grammar files](03-grammar-files.md). The notation itself is the
[language guide](../language/README.md).

## Language version and older frameworks

The generated parser is C# 8 and compiles for whatever the project targets: `net8.0` and later,
`netstandard2.0`, `net472`. Three things depend on the target.

**The language version.** A grammar written inside `[Gram("""…""")]` is a raw string literal,
which is C# 11. A grammar in a `.gram` file needs only what the generated code needs, C# 8.
`netstandard2.0` and .NET Framework projects default to C# 7.3, so they set `<LangVersion>`. Below
C# 8 the generator writes no parser and says why once, at the first `[Gram]`:

```csharp example=too-old LangVersion=7.3 GRAM0010
using DotGram;

[Gram]
public static partial class Feed { }
```

```dotgram file=Feed.gram example=too-old
Digits = ['0'..'9']+

parse Digits
```

The error reads *The generated parser needs C# 8 or later; this project compiles C# 7.3. Set
`<LangVersion>8.0</LangVersion>` or later.* A project with no `[Gram]` is not affected: the
attributes the generator adds to every compilation are C# 7.3 themselves.

**`System.Memory`.** The generated methods take `ReadOnlySpan<char>`. On .NET it is part of the
framework; on `netstandard2.0` and .NET Framework it comes from the `System.Memory` package, which
the project references itself:

```xml
<PropertyGroup>
  <TargetFramework>netstandard2.0</TargetFramework>
  <LangVersion>latest</LangVersion>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="DotGram" Version="0.3.0" PrivateAssets="all" />
  <PackageReference Include="System.Memory" Version="4.6.3" />
</ItemGroup>
```

The `DotGram` package cannot bring it along for you. It is private to the project, so a dependency
it declared would reach this project and stop there: a library built that way would not pass
`System.Memory` on to its own consumers, and they would fail at run time on .NET Framework. A
reference the project makes itself travels with the library like any other.

**`record` on old frameworks.** A grammar that builds `record` values needs
`System.Runtime.CompilerServices.IsExternalInit`, a type `netstandard2.0` and .NET Framework do
not have (CS0518). Declare it once in the project:

```csharp fragment
namespace System.Runtime.CompilerServices
{
	internal static class IsExternalInit { }
}
```

## What the generator adds to every compilation

Before it looks at any grammar, the generator adds one file to every project that references it,
`DotGram.Attributes.g.cs`: `[Gram]` and the other attributes of the next page, all `internal`.
Being internal, two assemblies that both use .Gram never see each other's copies, and there is no
runtime library whose version could disagree with the generator's. The attributes are marked
`[Microsoft.CodeAnalysis.Embedded]`, which keeps them apart even where one assembly grants another
`InternalsVisibleTo`.

Then, for each class with a `[Gram]`, it adds that class's parser, named after the class:
`Demo.Numbers.g.cs` for the first example. Visual Studio lists every generated file under
**Dependencies → Analyzers → DotGram** of the project, and opens it like any other source. To have
them written to disk as well, set the compiler's own property:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</PropertyGroup>
```

The files then land under `obj/`, in `generated/DotGram/DotGram.Generation.GramGenerator/`.

## Generating outside the build

A large grammar takes seconds to generate, and every cold build pays for it. The same generator is
also a .NET tool, `dotnet dotgram`, which writes the files a build would generate into a folder
that the project then compiles without referencing the package. A project does one or the other,
never both: the build would generate every parser a second time. [The command-line tool](05-cli.md)
has the details.

## Visual Studio

Nothing in Visual Studio has to be set for the generator: a build inside it and a build from the
command line run the same generator from the same package. The optional DotGram extension adds
language support for grammars in the editor — colouring, diagnostics as you type, navigation —
and has no settings that change what is generated ([`visual-studio.md`](../../visual-studio.md)).

