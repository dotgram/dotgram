# .Gram language support

Editing support for [DotGram](https://github.com/dotgram/dotgram) grammars (`.gram` files) in VS Code, from the
same language core the DotGram source generator and the Visual Studio extension compile: what you see is what the
build reads.

## What works

- **Diagnostics** as the generator reports them, with their `GRAM` ids.
- **Colours**: a first paint from a TextMate grammar, then the language server's classification of every token.
- **Completion** of rules, keywords, built-in rules, captures, the standard library after `Std.`, and C# inside
  `@(...)` (from Roslyn).
- **Hover** on a rule (its definition and the rules it references), a keyword, a symbol, or C#.
- **Signature help** for rules with parameters.
- **Go to Definition** (`F12`), including into an included grammar — its file, or a read-only copy where only a
  referenced assembly carries it — and into the C# host for a C# name.
- **Find All References**, **highlights**, **Rename** (`F2`) within the file, **folding** and the **outline**.
- **Snippets** for grammars and their C# hosts.

A grammar's host — the C# class whose `[Gram("….gram")]` names the file — is looked for in the workspace folder's
C# without a build, so its `Lexical` option and the grammars it includes are read as the build reads them. A folder
with more than 2,000 C# files, and a file outside any folder, are read as they stand.

## What does not

- Grammars embedded in C# (`[Gram("""…""")]` in a `.cs` file) are not analysed: this extension reads `.gram` files.
- Diagnostics about the project rather than the grammar (a host that is not `partial`, a grammar that is not an
  additional file) are the build's.

## Requirements

The language server runs on .NET 10. The extension asks the
[.NET Install Tool](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.vscode-dotnet-runtime) for
the runtime, which finds an installed one or downloads it; `dotgram.dotnetPath` names a `dotnet` to use instead.

## Settings

| Setting | |
|---|---|
| `dotgram.server.path` | A language server to run instead of the one the extension carries. |
| `dotgram.dotnetPath` | The `dotnet` that runs the server. |
| `dotgram.trace.server` | `messages` or `verbose` traces the protocol in the **.Gram** output panel. |

Building from source, and troubleshooting: [docs/vscode.md](https://github.com/dotgram/dotgram/blob/main/docs/vscode.md).
