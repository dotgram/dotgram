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

- [ ] Roslyn completion for C# names.
- [ ] Roslyn Quick Info for C# symbols.
- [ ] Go To Definition from grammar C# references.
- [ ] Preserve DotGram behavior for `@(...)` expressions and grammar arguments.

## Later milestones

- Navigate between grammar rules and generated C# APIs.
- Define and emit versioned generated-language discovery metadata.
- Discover user-created DSLs in parser API and attribute string arguments.
- Reuse the editor-neutral service from an LSP/VS Code adapter.

## Validation rules

- Standalone and embedded grammar must use the same language intelligence.
- Tooling stays under `DotGram.VisualStudio` or editor-neutral files directly included
  by that project; no separate project until there is a demonstrated need.
- Do not change parser recognition semantics for an editor feature.
- Every source position crossing a C# string boundary uses `CSharpStringMap`.
- New behavior needs editor-neutral tests and embedded-source-map tests where relevant.
