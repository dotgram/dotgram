# DotGram Code Snippets

Quick-start code snippets for writing `.gram` grammars and their C# host classes.

## VS Code

The VS Code snippets support both `.gram` files and C# files where grammars appear.

### Installation

1. **User Snippets** (applies to all workspaces):
   - Open the VS Code command palette: `Ctrl+Shift+P` (or `Cmd+Shift+P` on Mac)
   - Type **"Configure User Snippets"** and select **"DotGram"** (if prompted to choose a language)
   - If "DotGram" is not listed, select **"New Global Snippets file..."** and name it `gram`
   - Copy the contents of `vscode/gram.code-snippets` into the file
   - Save and restart VS Code

2. **Workspace Snippets** (applies only to this project):
   - Create a `.vscode` directory in your project root if it doesn't exist
   - Copy `vscode/gram.code-snippets` to `.vscode/gram.code-snippets`
   - Snippets are available immediately in that workspace

### Available Snippets

| Shortcut | Scope | Description |
|----------|-------|-------------|
| `gram` | .gram, C# | Basic grammar skeleton with trivia and parse publication |
| `typed-rule` | .gram | Rule that produces a typed result through a factory method |
| `rule-construct` | .gram | Rule that constructs a value from captured pieces |
| `choice` | .gram | Choice between multiple alternatives |
| `delimited-list` | .gram | Idiomatic delimited list using first & rest pattern |
| `recover` | .gram | Error recovery for a stream of elements |
| `guard` | .gram | Rule with a guard condition checking captured values |
| `on-fail` | .gram | Provide a custom error message when recognition fails |
| `namespace-notrivia` | .gram | Grammar namespace with no trivia for tight token recognition |
| `gram-host-inline` | C# | Host class with an inline grammar |
| `gram-host-file` | C# | Host class using a separate .gram file |

## Visual Studio

The Visual Studio snippets are distributed as `.snippet` XML files. Currently, only C# snippets are available; Visual Studio does not have built-in support for custom `.gram` files without an extension language service.

### Installation

1. Open the **Code Snippets Manager**:
   - Go to **Tools → Code Snippets Manager** (or press `Ctrl+K, Ctrl+B`)

2. Choose the language: Select **C#** from the dropdown

3. Click **Import** and select all `.snippet` files from `visualstudio/`:
   - `GramHostInline.snippet`
   - `GramHostFile.snippet`
   - `TypedRuleWithFactory.snippet`
   - `DelimitedList.snippet`

4. The snippets are now available in the C# editor. Trigger them with `Ctrl+K, Ctrl+X` and search by shortcut name

### Available Snippets

| Shortcut | Description |
|----------|-------------|
| `gram-host-inline` | Host class with an inline grammar |
| `gram-host-file` | Host class using a separate .gram file |
| `typed-rule` | Rule producing a typed result through a factory |
| `delimited-list` | Idiomatic delimited list using first & rest |

## Snippet Templates

All snippets use tab stops and placeholders to guide you through the common patterns:

- **Tab stops** (`${1:placeholder}`) let you quickly move between fields with `Tab`
- **Choices** (`${1|option1,option2|}`) offer alternatives you can select
- **Syntax highlighting** in the snippets reflects actual `.gram` or C# syntax

## Grammar Resources

For more information about writing grammars, see:
- [DotGram syntax documentation](https://github.com/dotgram/dotgram/blob/main/docs/syntax.md)
- [DotGram diagnostic reference](https://github.com/dotgram/dotgram/blob/main/docs/diagnostics.md)
- [Example grammars](https://github.com/dotgram/dotgram/tree/main/examples/DotGram.Examples)
- [SKILL.md](https://github.com/dotgram/dotgram/blob/main/src/DotGram/SKILL.md) in the DotGram package
