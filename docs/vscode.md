# VS Code extension

`editors/vscode` is a VS Code extension for `.gram` files and for the grammars written into C# strings. It starts
the DotGram language server, `src/DotGram.LanguageServer`, which compiles the same language core as the source
generator and the Visual Studio extension (`src/DotGram/Language/README.md`) and answers the Language Server
Protocol over standard input and output. What a `.gram` file shows in VS Code is what it shows in Visual Studio:

- diagnostics, with the generator's `GRAM` ids;
- colours: a TextMate grammar paints the file at once, and the server's semantic tokens replace it once the file is
  analysed;
- completion of rules, keywords, built-in rules, captures and parameters, the standard library after `Std.`, and C#
  inside `@(...)`;
- hover on a rule (its definition, then the rules it references), a word or symbol of the notation, or C#;
- signature help for a rule with parameters;
- Go to Definition: a rule in the file, a rule of the standard library or of an included grammar, a C# name in the
  grammar's host (not the methods a publication generates: no generator runs without a build);
- Find All References (into included grammars too), highlights, Rename within the file, folding and the outline;
- the snippets of `editors/vscode/snippets`.

A grammar in a C# document has its diagnostics, colours, hover, Go to Definition, Find All References, highlights
and Rename (see [Grammars in C#](#grammars-in-c)).

The symbols of the notation — `|`, `&`, `~`, `?`, `*`, `+`, `^`, `..`, `<`, `>`, `?=`, `?!` — are painted as
`keyword.control`, the keywords as `keyword.other`, as Visual Studio paints the first like a regular expression's
quantifiers and the second as its keywords: in the default themes (Light+, Dark+, the Modern ones and both High
Contrast ones) the symbols stand apart from the names and from the keywords, where `keyword.operator`, which they
were before, is the colour of plain text. An operator — `=`, `=>`, `-` — stays `keyword.operator`, as in Visual
Studio.

## Building

```bash
editors/vscode/build.sh            # Linux, macOS
pwsh editors/vscode/build.ps1      # Windows
```

The script publishes the server into `editors/vscode/server`, installs the client's packages (`npm ci`), bundles the
client with esbuild, writes `ThirdPartyNotices.txt` — the licence of every npm package the bundle holds and of every
NuGet package the server loads, the runtime's where it carries one, each with the notices it ships, and the text of a
licence a package only names from `scripts/licenses` (SPDX's); a package whose licence text is found nowhere fails
the build — and packages the whole as `artifacts/dotgram-<version>.vsix` at the repository root with
`vsce package`. It needs the .NET SDK `global.json` names and Node 22. Nothing is published to the Marketplace.

CI does the same in the `vscode` job of `.github/workflows/build.yml` and keeps the `.vsix` as the `vscode-vsix`
artifact. The workflow runs on `main`; a branch is built with `gh workflow run build.yml --ref <branch>`.

## Installing

```bash
code --install-extension artifacts/dotgram-0.3.0.vsix
```

The extension depends on the [.NET Install Tool](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.vscode-dotnet-runtime)
(`ms-dotnettools.vscode-dotnet-runtime`), which VS Code installs with it from the Marketplace; on a machine without
access to the Marketplace, install that extension first.

## How the server is shipped

The server is **framework-dependent**: the `.vsix` carries its assemblies (about 35 MB on disk, 12 MB packed, most of
it Roslyn, which reads the C# in a grammar and finds its host), and the .NET 10 runtime they run on is acquired when
the extension starts, through the .NET Install Tool's `dotnet.acquire` — an installed runtime is found, a missing one
is downloaded once. This is what the C# extension does, and one package serves every platform.

A **self-contained** server would need no runtime from anywhere, at the price of one `.vsix` per platform
(`vsce package --target`), each about 70-100 MB, six of them built in a CI matrix, and Roslyn's caveats about
trimming, which a server that loads Roslyn's Features by MEF composition would have to work through. The build
scripts take the switch already (`build.sh --self-contained linux-x64`, `build.ps1 -SelfContained win-x64`), and
the extension runs the executable such a build carries instead of acquiring a runtime, so that a change of mind is
a flag and a CI matrix, not a rewrite.

## Settings

| Setting | Default | |
|---|---|---|
| `dotgram.server.path` | empty | A server to run instead of the one in the extension: a `DotGram.LanguageServer.dll`, run with `dotnet`, or an executable. |
| `dotgram.dotnetPath` | empty | The `dotnet` that runs the server; empty asks the .NET Install Tool for .NET 10. |
| `dotgram.trace.server` | `off` | `messages` or `verbose`: the protocol traced in the **.Gram** output panel. |

The two settings that choose a program to run are the machine's: a workspace's `.vscode/settings.json` cannot set
them, so a folder cloned from somewhere cannot make the extension run a program of its choosing.

The server itself takes `--version`, and `--log <path>` to append every message it reads and writes to a file.

## A grammar's host without a build

Visual Studio finds a grammar's host — the C# class whose `[Gram("….gram")]` names the file — in the solution it has
loaded. VS Code has no solution, so the server makes one from the workspace folders: a project for every `.csproj`.
In a workspace VS Code trusts, on a machine with a .NET SDK, MSBuild evaluates the projects — once, all of them in
one run of `dotnet msbuild` in each workspace folder, where its `global.json` chooses the SDK — and a project holds
what the build gives its compiler: its `Compile` items, its `AdditionalFiles` grammars, the projects it references
and its `Using` items, every glob, `Remove`, condition and import applied, `Directory.Build.props` and a restored
package's build files among them. A file below a nested project is in both where both glob it, and a grammar class
of a referenced project is included from that project's file, as the build reads it from the referenced assembly.
The evaluation runs only the projects' `InitialTargets` and a target of the server's that returns those items;
nothing is built or restored. On this repository, 54 projects, the folders are read and evaluated in about 2 s;
a project file changed has its project evaluated again in about 1 s, a `.props` or `.targets` file every project
in about 2 s.

A project MSBuild cannot evaluate (the server says which in the output panel), every project of a workspace VS
Code does not trust — evaluating a project runs its own code — and every project on a machine without a .NET SDK
are read by their folders instead, the fallback: a project holds every C# and `.gram` file below it that no nearer
project file claims, the project references and `Using` items its file lists (`ImplicitUsings` as the SDK's
defaults), and nothing of its conditions, imports or packages. Trust granted later starts the server again,
evaluating. The files no project holds are a project of each folder's own.

Each project is compiled against the .NET 10 reference assemblies and the generator's own attribute declarations,
with its grammars as additional files. A grammar's host is looked for in the project that holds the grammar, as the
build compiles it there: two projects that each have a `Parser.gram` and a host for it are two hosts. A host found
there gives the grammar its `Lexical` option and the grammars it includes, read as the build reads them, so a rule
from an included grammar is not reported missing and Go to Definition goes into its file — or, where only a class's
`[GramSource]` carries it, into a read-only copy under the temporary directory, as in Visual Studio. The projects
follow the folders: a `.cs` or `.gram` file created, changed or deleted is read again (one created, or a grammar
opened before it was on disk and then saved, below an evaluated project has that project evaluated again, so that
its glob can take it in), and a C# file saved has every
grammar of its project look for its host again (a `global using` may be what binds it); a project file, `.props` or
`.targets` file changed, or a folder added to or removed from the workspace, has the projects read again — a file
saved while that runs keeps what it was saved as. A grammar open in the editor is its editor's text, unsaved edits
included, for every grammar that includes it, until it is closed.

`bin`, `obj`, `node_modules`, folders whose name starts with a dot, and links (symbolic links, junctions) are not
read. A workspace with more than 2,000 C# files (counting those its projects compile from elsewhere), more than 200,000 files and folders in all, or one that takes longer
than ten seconds to look through, is not read at all — the server says so once in the output panel — and its
grammars, like a `.gram` file
opened outside any folder, are read as they stand and quiet about what depends on a host: syntax errors are shown,
a name that may come from an included grammar is not reported.

A project's package and assembly references are not compiled against, so what a host's code takes from them is not
known; the diagnostics that depend on it are the build's, as they are in Visual Studio for any C# type a grammar
names.

## Grammars in C#

The extension starts with the first `.gram` or C# document opened. It hands the server every C# document, as Visual
Studio's extension reads every C# buffer, and the server analyses the grammars written into it with the shared core's
embedded session — the analysis Visual Studio runs: the string of a `[Gram("…")]`, plain, verbatim or raw, however the attribute is spelled (an alias, a
qualified name); a string passed to a parameter annotated `[StringSyntax("DotGram")]` or `[StringSyntax(".gram")]`;
and a string written in the DSL of a `[GramLanguage]` (docs/visual-studio.md). The document is read in the project the
workspace reads its file in (see [A grammar's host without a build](#a-grammars-host-without-a-build)), with the
editor's text, so a grammar's host gives its options and what it includes, a grammar of a referenced project
among them. A C# file outside the workspace folders, or one not saved to disk yet, has nothing analysed.

What a grammar in C# shows:

- its diagnostics, published on the C# document with the source `dotgram` and their `GRAM` ids, as the build's
  `#pragma`s and severities let them through;
- its colours, painted over the C# document's (below);
- hover, Go to Definition, Find All References, highlights and Rename inside it; Go to Definition on the name of a
  `.gram` file a `[Gram("….gram")]` gives opens that file.

Everywhere else in a C# document the extension answers nothing and asks the server nothing: that is the C#
extension's. Where the build itself runs the generator — a project that references the DotGram package, open with
the C# extension or C# Dev Kit — a grammar's diagnostics may be shown twice, once by each.

### How the colours are painted

VS Code lets one provider of semantic tokens colour a document; a C# document's is the C# extension's, and the two
cannot be merged. The server therefore does not offer semantic tokens for C# documents (its capability names `.gram`
documents only). It tells the colours as a notification of its own, `dotgram/embeddedColors` — the spans of one
version of the document, each with its kind, the same kinds a `.gram` file's semantic tokens have — and the extension
paints them as text decorations, which VS Code draws over any provider's colours. So:

- **without the C# extension**, VS Code's own TextMate grammar colours the C# and the grammar is painted over its
  string colour;
- **with the C# extension** (or C# Dev Kit), its semantic tokens colour the C# — references, the unused, everything
  it does — and the grammar is painted over the string as before; nothing of the C# extension's is replaced.

A decoration's colour is not a theme's token colour. Each kind is a colour of the extension's own,
`dotgram.embedded.<kind>` (`dotgram.embedded.keyword`, `dotgram.embedded.gramSpecialSymbol`, …), whose defaults
are the colours that family has in the default themes, for dark, light, High Contrast and High Contrast Light: a
grammar in C# looks as it does in a `.gram` file under the default themes, and under another theme keeps those
colours until `workbench.colorCustomizations` sets them. An edit moves what is painted with the text, and the server
paints the version again once it has analysed it.

## What is not there

- Completion, signature help, folding and the outline inside a grammar in C#, which the shared core and Visual Studio
  have: the server does not answer them for C# documents yet.
- Code actions, formatting and inlay hints, which the Visual Studio extension does not have either.
- The Marketplace: the extension is built and installed from a `.vsix`.

## Troubleshooting

- **No colours beyond the first paint, no diagnostics.** The server did not start: the **.Gram** output panel says
  why. Most often the .NET 10 runtime could not be acquired; set `dotgram.dotnetPath` to a `dotnet` that has it
  (`dotnet --list-runtimes`).
- **A rule from an included grammar is reported missing.** The host was not found: is its `.cs` file inside the
  workspace folder, and does its `[Gram]` name the file? With more than 2,000 C# files in the workspace, open a
  smaller folder.
- **What did the server say?** Set `dotgram.trace.server` to `verbose` and read the output panel, or start the
  server with `--log` through `dotgram.server.path` pointing at a script that passes it.

## Testing

The server's tests, `tests/DotGram.LanguageServer.Tests`, run with the solution on every platform: the protocol, the
T-SQL grammar's diagnostics, every repository grammar's semantic tokens against its classifications, the
playground recordings the Visual Studio extension's answers were recorded in, asked through the protocol, hosts found
in a folder, the TextMate grammar's words against the core's vocabulary and its symbols against the core's
classification, and the grammars in C# documents (`EmbeddedTests`): raw, verbatim and plain strings, an alias of the
attribute, a `StringSyntax` parameter, a grammar that includes one of a referenced project, edits, and the answers
inside a grammar and outside it.

`npm test` in `editors/vscode`, after a build, installs the packaged `.vsix` into an extensions folder of its own,
beside the .NET Install Tool, in a downloaded VS Code, and checks that the extension running is that package and
carries its third-party notices; then it opens `test-fixture/` and waits for a grammar's diagnostics and semantic
tokens, and for a C# document's grammar's diagnostic, its colours, hover and Go to Definition, and checks that the C#
document's semantic tokens are not the extension's. VS Code and the .NET Install Tool are pinned: VS Code 1.90.0, the
oldest the manifest accepts, and the newest .NET Install Tool it runs (2.3.2); `DOTGRAM_SMOKE_VSCODE` and
`DOTGRAM_SMOKE_RUNTIME_TOOL` (`ms-dotnettools.vscode-dotnet-runtime@<version>`) choose others for a run, and
`DOTGRAM_VSIX` another package. `DOTGRAM_SMOKE_CSHARP` (`ms-dotnettools.csharp@<version>`; 2.50.27 is the newest that
runs on VS Code 1.90) installs the C# extension beside them, and the C# document's semantic tokens are then waited
for and must be the C# extension's, with the grammar painted all the same. It
needs the network and a display (`xvfb-run -a npm test` on a Linux machine without one); the `vscode` CI job runs it
after packaging.
