# VS Code extension

`editors/vscode` is a VS Code extension for standalone `.gram` files. It starts the DotGram language server,
`src/DotGram.LanguageServer`, which compiles the same language core as the source generator and the Visual Studio
extension (`src/DotGram/Language/README.md`) and answers the Language Server Protocol over standard input and output.
What a `.gram` file shows in VS Code is what it shows in Visual Studio:

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

## Building

```bash
editors/vscode/build.sh            # Linux, macOS
pwsh editors/vscode/build.ps1      # Windows
```

The script publishes the server into `editors/vscode/server`, installs the client's packages (`npm ci`), bundles the
client with esbuild, and packages the whole as `artifacts/dotgram-<version>.vsix` at the repository root with
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

The two settings that choose a program to run are the machine's: a workspace's `.vscode/settings.json` cannot set
them, so a folder cloned from somewhere cannot make the extension run a program of its choosing.
| `dotgram.trace.server` | `off` | `messages` or `verbose`: the protocol traced in the **.Gram** output panel. |

The server itself takes `--version`, and `--log <path>` to append every message it reads and writes to a file.

## A grammar's host without a build

Visual Studio finds a grammar's host — the C# class whose `[Gram("….gram")]` names the file — in the solution it has
loaded. VS Code has no solution, so the server makes one: an ad-hoc project of the workspace folders' C# files that
declare a grammar attribute (`[Gram]`, `[GramInclude]`, `[GramSource]`, found by their syntax), compiled against the
.NET 10 reference assemblies and the generator's own attribute declarations, with every `.gram` file of the folders as
an additional file. A host found there gives the grammar its `Lexical` option and the grammars it includes, read as
the build reads them, so a rule from an included grammar is not reported missing and Go to Definition goes into its
file — or, where only a class's `[GramSource]` carries it, into a read-only copy under the temporary directory, as in
Visual Studio. The project follows the folder: a `.cs` or `.gram` file created, changed or deleted is read again, and
the grammars whose host it can change are analysed again. A grammar open in the editor is its editor's text, unsaved
edits included, for every grammar that includes it, until it is closed.

`bin`, `obj`, `node_modules`, folders whose name starts with a dot, and links (symbolic links, junctions) are not
read. A workspace with more than 2,000 C# files, more than 200,000 files and folders in all, or one that takes longer
than ten seconds to look through, is not read at all — the server says so once in the output panel — and its
grammars, like a `.gram` file
opened outside any folder, are read as they stand and quiet about what depends on a host: syntax errors are shown,
a name that may come from an included grammar is not reported.

Only the C# files that declare a grammar are compiled, so what the host's other code depends on is not known; the
diagnostics that depend on it are the build's, as they are in Visual Studio for any C# type a grammar names.

## What is not there

- Grammars embedded in C# (`[Gram("""…""")]` in a `.cs` file): VS Code shows C# files with its own C# extension,
  and this extension adds nothing to them.
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
in a folder, and the TextMate grammar's words against the core's vocabulary.

`npm test` in `editors/vscode`, after a build, runs the packaged extension in a downloaded VS Code against
`test-fixture/` and waits for a grammar's diagnostics and semantic tokens. It needs the network and a display
(`xvfb-run -a npm test` on a Linux machine without one); the `vscode` CI job runs it after packaging.
