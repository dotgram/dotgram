# DotGram Visual Studio tooling plan

This is the living implementation checklist for `DotGram.VisualStudio`. The broader
architecture and possible VS Code/LSP/metadata work remain in
`DotGram_Tooling_Agent_Handoff.md`.

## Completed

- Standalone `.gram` content type and syntax classification.
- Compiler diagnostics with exact editor spans.
- Embedded grammar discovery through the real `GramAttribute` symbol.
- Regular, verbatim, and raw C# string source maps.
- Embedded classification and diagnostics.
- Rule and local-symbol completion.
- Parameterized-rule signature help.
- Quick Info with expandable, classified referenced-rule definitions.
- Go To Definition, Find References, reference highlighting, and Rename for rules,
  parameters, and captures.
- Matching braces for grammar and embedded C# expressions.
- Folding for multiline rules, groups/blocks, and block comments.
- `namespace`, expression-scoped `with`, and publication-scoped `with` support.

## Current milestone: document structure

- [x] Editor-neutral hierarchical document symbols for namespaces, rules, and
  publications.
- [x] Visual Studio navigation UI for standalone `.gram` documents.
- [x] Equivalent navigation inside embedded grammar strings without replacing the C#
  navigation bar.
- [x] Keep selection synchronized with the caret and preserve namespace hierarchy.

## Next milestone: C# seam after `@`

- [x] Roslyn completion for C# names after `@` and in member-access chains.
- [x] Roslyn Quick Info for C# types and members after `@`.
- [x] Go To Definition from grammar C# references, including project declarations
  and Metadata-as-Source.
- [x] Preserve DotGram behavior while enabling Roslyn tooling inside `@(...)`
  expressions and grammar arguments.

## Completed: generated API navigation

- [x] Navigate from grammar publications to generated C# APIs.
- [x] Navigate from generated C# APIs back to standalone and embedded publications.

## Current milestone: VSIX packaging

- [x] Produce a Release VSIX for Visual Studio 18.
- [x] Add Extension Manager metadata and installation documentation.
- [x] Validate installation through VSIXInstaller.
- [x] Validate an update from 0.1.26 to 0.1.27 through VSIXInstaller.

## Current milestone: generated DSL discovery

- [x] Merge the generated author-facing tooling attributes.
- [x] Specify the first symbol-discovery and custom-attribute vertical slice.
- [x] Implement shape-based Roslyn discovery for parser hosts and attribute carriers.
- [x] Bind `GramClassify` rule/capture targets to grammar symbols.
- [x] Define a shared recognition trace for classifying arbitrary DSL input.
  - [x] Trace pure normalized recognition nodes with rule/capture extents, ordered
    backtracking, atomic groups, lookahead, repetition, calls, and whole-parse trivia.
  - [x] Define a non-executing contract for guard decisions and external-recognizer
    rule mappings, including an unsupported fallback.
  - [x] Add guarded/external recognition through a descriptor-backed non-executing
    tooling contract and prove parity against generated parsers on the bounded corpus.
- [x] Classify and diagnose string arguments routed through user-marked parameters in
  Visual Studio.
- [x] Cache language discovery per Roslyn compilation and normalized DSL preparation
  per editor buffer.
- [x] Report expected grammar elements at the furthest DSL recognition failure.
- [x] Show the language id and published entry rule in Quick Info for routed DSL strings.
- [x] Route string inputs of generated parse and try-parse publication methods in source
  projects without requiring a marker attribute.
- [x] Select the exact entry rule for generated APIs when one language publishes several
  parse methods.
- [x] Offer exact grammar literals through completion at a routed DSL failure position.
- [x] Decode character literals and offer punctuation/operator alternatives in DSL
  completion.

The detailed design and explicit deferrals are in
[`dsl-tooling-design.md`](dsl-tooling-design.md).

## Later milestones

- [x] Define and emit metadata format v1 with language id, SHA-256 grammar hash,
  complete composed grammar source, and publication entry bindings.
- [x] Discover metadata v1 through Roslyn in source and referenced assemblies without
  loading consumer code.
- [x] Route generated parser API string arguments from referenced assemblies through
  metadata v1, including exact entries for parse and try-parse methods.
- [x] Discover user-created DSLs through referenced marker attributes, both on library
  API parameters and on consumer-owned parameters.
- [x] Emit semantic classification mappings in metadata format v2, while retaining
  metadata v1 discovery compatibility.
- Reuse the editor-neutral service from an LSP/VS Code adapter.

## Validation rules

- Standalone and embedded grammar must use the same language intelligence.
- Tooling stays under `DotGram.VisualStudio` or editor-neutral files directly included
  by that project; no separate project until there is a demonstrated need.
- Do not change parser recognition semantics for an editor feature.
- Every source position crossing a C# string boundary uses `CSharpStringMap`.
- New behavior needs editor-neutral tests and embedded-source-map tests where relevant.
