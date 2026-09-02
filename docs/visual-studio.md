# Visual Studio extension

`DotGram.VisualStudio` provides language support for standalone `.gram` files and
DotGram grammars embedded in C# `GramAttribute` strings.

An API that accepts DotGram source text directly can opt a string parameter into the
same tooling with the standard annotation:

```csharp
using System.Diagnostics.CodeAnalysis;

void Inspect([StringSyntax("DotGram")] string grammar);
void InspectFileSyntax([StringSyntax(".gram")] string grammar);
```

Both syntax names are equivalent. String literals passed to either parameter receive DotGram classification, diagnostics,
Quick Info, navigation, completion, brace matching, and folding. This annotation marks
the DotGram grammar notation itself.

For a generated DSL, use the identifier declared by its parser host:

```csharp
[Gram("Filter.gram")]
[GramLanguage("filter")]
public static partial class FilterLanguage;

void Execute([StringSyntax("filter")] string query);
```

The extension resolves the `StringSyntax` value to the matching `GramLanguage`
descriptor. Calls to generated parser publication methods are recognized directly from
their parser host and publication metadata.

The same annotation works on the receiver parameter of an extension method and on a
field or property initializer:

```csharp
static string AsFilter([StringSyntax("filter")] this string text) => text;

var filter = "status = active".AsFilter();

[StringSyntax("filter")]
const string DefaultFilter = "status = active";
```

Only the immediate literal receiver or initializer is classified. Values are not
propagated through variables, and later assignments to an annotated field are ignored.

## Build the VSIX

From the repository root:

```powershell
dotnet build src/DotGram.VisualStudio/DotGram.VisualStudio.csproj -c Release
```

The installable package is written to:

```text
src/DotGram.VisualStudio/bin/Release/net472/DotGram.VisualStudio.vsix
```

## Install

Close Visual Studio, then open `DotGram.VisualStudio.vsix` in File Explorer or run:

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Enterprise\Common7\IDE\VSIXInstaller.exe" `
  "src\DotGram.VisualStudio\bin\Release\net472\DotGram.VisualStudio.vsix"
```

Select the Visual Studio 18 instance in the installer and restart Visual Studio after
installation. The package intentionally does not target Visual Studio 2022.

To update a local installation, build a package with a higher `Version` in
`DotGram.VisualStudio.csproj`, then run the new VSIX. Visual Studio identifies updates by
the stable `DotGram.VisualStudio` extension ID.

## Verify

Open `examples/DotGram.Examples/VisualStudioToolingPlayground.cs` and
`examples/DotGram.Examples/VisualStudioToolingPlayground.gram`. The comments beside each
grammar rule describe the manual checks and their expected results.
