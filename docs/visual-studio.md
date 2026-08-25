# Visual Studio extension

`DotGram.VisualStudio` provides language support for standalone `.gram` files and
DotGram grammars embedded in C# `GramAttribute` strings.

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
