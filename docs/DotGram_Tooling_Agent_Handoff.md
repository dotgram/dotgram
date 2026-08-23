# .Gram Tooling Project — Agent Handoff Specification

## Purpose

This document consolidates the design discussion for future `.Gram` tooling:

- Visual Studio support;
- VS Code support;
- tooling for standalone `.gram` files;
- tooling for grammar text embedded in C#;
- tooling for arbitrary DSLs created with `.Gram`;
- generated grammar/language metadata;
- semantic classification;
- nested embedded languages;
- source mapping;
- agent-facing metadata;
- XML documentation;
- likely project structure and implementation phases.

The central design constraint is that **tooling metadata must remain orthogonal to grammar recognition semantics**.

A grammar must continue to mean the same thing with or without tooling annotations.

---

# 1. Core product idea

`.Gram` is not only a parser generator.

A generated parser can become a **self-describing language artifact**.

One `.gram` source can eventually be the source of truth for:

- parser generation;
- runtime parsing;
- syntax highlighting;
- semantic highlighting;
- completion;
- diagnostics;
- navigation;
- Find References;
- Rename;
- folding;
- Document Symbols / Outline;
- embedded-language recognition;
- XML documentation;
- coding agents;
- API understanding.

The intended architecture is:

```text
.gram
    language / recognition semantics
        ↓
source generator
        ↓
generated parser
        +
generated language metadata
        +
optional XML documentation
        ↓
VS / VS Code / LSP / agents / other tools
```

Tooling must consume declarative metadata and compiler-produced structures rather than reconstructing the language heuristically.

---

# 2. Fundamental invariant

The strongest invariant for the whole tooling design is:

> **Tooling annotations may describe a grammar, but must never change what the grammar recognizes or produces.**

For example, a parser may be annotated conceptually as:

```csharp
[Gram("Rules.gram")]
[GramClassify("Identifier", GramClassification.Identifier)]
[GramClassify("String",     GramClassification.String)]
[GramClassify("Keyword",    GramClassification.Keyword)]
public partial class RulesParser;
```

If all `GramClassify` attributes are removed, parser generation and runtime behavior must remain identical.

This gives the system a clean separation:

```text
.gram
    says what text belongs to the language

tooling attributes
    say what grammar constructs mean to tools

editor/theme
    says how those semantic roles look
```

This should be treated as an architectural rule, not merely a convention.

---

# 3. Pay-as-you-go tooling

Tooling must be usable incrementally.

A user should be able to start with:

```csharp
[Gram("Rules.gram")]
public partial class RulesParser;
```

and get a parser.

Later the same user may add:

```csharp
[GramClassify("Identifier", GramClassification.Identifier)]
[GramClassify("String", GramClassification.String)]
```

and gain richer editor behavior.

Later still:

```csharp
[GramClassify("Declaration.type", GramClassification.Type)]
[GramClassify("Declaration.name", GramClassification.Variable)]
```

may enable semantic highlighting.

Later:

```csharp
[GramLanguage("Query.sql", typeof(SqlParser))]
```

may identify an embedded language.

The grammar itself should not need to be rewritten for these tooling features.

---

# 4. Recognition semantics versus tooling semantics

The grammar alone usually cannot tell the IDE whether a recognized fragment is a keyword, identifier, type, variable, string, comment, or operator.

Example:

```dotgram
Select = ^"select"
Open   = '('
Name   = ['a'..'z']+
Text   = '"' & [^ '"']* & '"'
```

Recognition structure tells us:

```text
Select → literal
Open   → literal
Name   → repeated element set
Text   → sequence
```

It does not reliably imply:

```text
Select → keyword
Open   → punctuation
Name   → identifier
Text   → string
```

A literal might be:

- a keyword;
- an enum value;
- protocol constant;
- delimiter;
- unit suffix;
- operator;
- magic marker.

Therefore tooling must not infer semantic roles from grammar shape or rule names.

Do not use heuristics such as:

```text
rule named "Identifier" → identifier
rule named "Keyword"    → keyword
```

Those names are user-defined and have no stable semantic contract.

---

# 5. Semantic roles

Tooling should use semantic roles rather than concrete colors.

Conceptual vocabulary:

```text
SemanticRole
    Keyword
    Identifier
    Type
    Variable
    Function
    Method
    Property
    Number
    String
    Comment
    Operator
    Punctuation
    Namespace
    Parameter
    Label
    ...
```

Exact names should not be frozen prematurely.

Important constraint: VS, VS Code, and LSP semantic tokens each have their own token taxonomies. `.Gram` should ideally expose a stable language-neutral role schema and let adapters map it to editor-specific categories.

Never make the metadata mean:

```text
Identifier → blue
```

It should mean:

```text
Identifier → semantic role: identifier
```

Theme/editor decides presentation.

---

# 6. Rule-level classification and use-site overrides

Rule-level classification is useful, but insufficient.

Example:

```dotgram
Declaration =
    type: Identifier
    & name: Identifier
```

Both captures use the same lexical rule:

```text
Identifier
```

but their semantic meanings differ:

```text
Declaration.type → type
Declaration.name → variable
```

Therefore classification should support at least two levels:

```text
rule classification
        ↓ default
capture / use-site classification
        ↓ override
```

Conceptually:

```csharp
[GramClassify("Identifier",        GramClassification.Identifier)]
[GramClassify("Declaration.type", GramClassification.Type)]
[GramClassify("Declaration.name", GramClassification.Variable)]
```

The more specific use-site classification overrides the general rule classification.

This distinction is what separates simple lexical coloring from true semantic highlighting.

---

# 7. Keywords

If a grammar already defines a keyword rule:

```dotgram
Keyword =
      ^"select"
    | ^"from"
    | ^"where"
```

then tooling should be able to classify the rule once:

```csharp
[GramClassify("Keyword", GramClassification.Keyword)]
```

There should be no need to duplicate every individual keyword in tooling metadata.

The grammar remains the source of truth for which strings belong to the keyword category.

---

# 8. Embedded languages

Tooling metadata should be able to mark a captured/source region as another language.

Example:

```dotgram
Query =
    "sql" & body: SqlText
```

Metadata may say:

```text
Query.body
    embedded language = SQL
```

The IDE then knows:

```text
host DSL
    ↓
Query.body source span
    ↓
SQL language service
```

The important abstraction is not:

```text
Rule → Color
```

but:

```text
Rule/Capture → SemanticRole
Rule/Capture → EmbeddedLanguage
```

The latter generalizes to arbitrary nested-language composition.

---

# 9. Nested embedded languages

The design must allow recursive language embedding.

Example:

```text
Rules DSL
    └── expression
            └── SQL fragment
                    └── regex literal
```

Useful terminology:

- **embedded language**
- **nested embedded languages**
- **recursive language embedding**
- **language region**
- **language composition**

Conceptual runtime/tooling structure:

```csharp
LanguageRegion
{
    LanguageId
    SourceSpan
    Children[]
}
```

The model must support arbitrary nesting:

```text
host language
    ↓
embedded language A
    ↓
embedded language B
    ↓
embedded language C
```

Tooling must not infer the embedded language from names such as `Sql`, `Regex`, or `Json`.

The relationship must be explicit:

> this captured/source region is language X.

---

# 10. Source mapping is a first-class abstraction

Embedded tooling inside C# requires a correct mapping between:

```text
decoded embedded text
```

and:

```text
original host source positions
```

This is non-trivial because C# string literals may be:

- regular strings;
- verbatim strings;
- raw strings;
- interpolated strings, if eventually supported;
- indented raw strings;
- escaped strings.

The central abstraction should look conceptually like:

```csharp
interface IEmbeddedTextMap
{
    TextSpan SourceSpan(int decodedStart, int decodedLength);
}
```

Potential implementations:

```text
RegularStringMap
VerbatimStringMap
RawStringMap
```

The exact interface may evolve, but the abstraction itself should be explicit and reusable.

Do not scatter one-off offset calculations throughout the VS extension, diagnostics, generator, and LSP.

The same source-map abstraction should ideally serve:

- source-generator diagnostics;
- syntax highlighting;
- semantic highlighting;
- completion;
- navigation;
- LSP virtual documents;
- nested embedded-language composition.

---

# 11. Embedded grammar inside C#

A high-value first tooling scenario is grammar text written directly in C#:

```csharp
[Gram("""
    Expr = ...
""")]
public partial class Parser;
```

Visual Studio should eventually understand the string as `.Gram`, not as plain C# string content.

Basic pipeline:

```text
C# ITextSnapshot
        ↓
Roslyn SyntaxTree
        ↓
find AttributeSyntax
        ↓
resolve attribute symbol
        ↓
is this DotGram.GramAttribute?
        ↓
extract decoded string
        ↓
.Gram lexer/parser/compiler front-end
        ↓
grammar tokens + diagnostics + semantic info
        ↓
map decoded spans back to C# source spans
        ↓
editor classifications / squiggles / completion
```

The extension must use Roslyn semantic symbol resolution for rich tooling.

Do not rely on text matching:

```text
[Gram(...)]
```

because valid forms may include:

```csharp
[DotGram.Gram(...)]
```

or:

```csharp
using DG = DotGram;

[DG.Gram(...)]
```

or other aliasing scenarios.

Text-based matching may still be acceptable for limited TextMate injection on VS Code, but not for semantic tooling.

---

# 12. Standalone `.gram` files

Standalone `.gram` files should be first-class tooling targets.

Expected eventual features:

- syntax highlighting;
- diagnostics;
- completion;
- Go To Definition;
- Find References;
- Rename;
- Quick Info / hover;
- brace/group matching;
- folding;
- Document Symbols / Outline;
- navigation between grammar and generated API;
- semantic classification;
- embedded-language regions.

The `.Gram` compiler front-end must remain reusable outside the source generator so tooling does not need to duplicate grammar parsing/binding logic.

The existing design of a Roslyn-free grammar compiler is a good foundation for this.

---

# 13. Visual Studio extension

## 13.1 Initial scope

A practical first Visual Studio extension should support:

1. `.gram` syntax highlighting;
2. `.gram` diagnostics;
3. embedded grammar highlighting inside `[Gram("""...""")]`;
4. embedded diagnostics inside the C# string;
5. basic navigation where feasible.

Avoid trying to build the entire language service in the first version.

## 13.2 Architecture

Conceptual project:

```text
DotGram.VisualStudio
    VSIX
        ↓
Roslyn APIs
Visual Studio text/classification APIs
        ↓
DotGram.Language
        ↓
shared grammar compiler / semantic model
```

Do not put grammar parsing logic directly into the VSIX.

## 13.3 Classification

For embedded grammar:

```text
ITextSnapshot
    ↓
Roslyn syntax/semantic model
    ↓
embedded grammar region
    ↓
DotGram lexer / semantic model
    ↓
classification spans
    ↓
VS ClassificationTag / equivalent editor classification
```

The editor integration layer should be thin.

## 13.4 Diagnostics

Compiler diagnostics should point to real host source locations.

For embedded strings, diagnostics must use the source map rather than approximating line/column manually.

When a precise map cannot be produced safely, prefer a broader but correct host span over a wrong squiggle.

## 13.5 Completion

Potential completion categories:

- visible rule names;
- builtin rules;
- parameters;
- scopes/contexts;
- C# symbols after `@`, if semantic host integration is available;
- capture names where valid;
- syntax operators/keywords;
- publication targets;
- tooling metadata target names.

## 13.6 Navigation

Potential navigation:

- rule reference → rule declaration;
- parameter use → parameter declaration;
- capture/tooling target → grammar source;
- generated parser API → originating rule;
- tooling metadata target string → grammar rule/capture.

---

# 14. VS Code extension

VS Code should use a different integration strategy from Visual Studio.

Do not try to mechanically reproduce the VS implementation.

## 14.1 Standalone `.gram`

For basic syntax coloring:

```text
VS Code extension
    ↓
register language id
    ↓
TextMate grammar
```

For richer features:

```text
DotGram.VSCode
    TypeScript extension
        ↓
LSP
        ↓
DotGram.LanguageServer (.NET)
        ↓
DotGram.Language
```

The language server should reuse the same grammar front-end and semantic model as Visual Studio tooling.

## 14.2 Embedded grammar in C#

Basic coloring can use a TextMate injection grammar into C#.

This can recognize common forms such as:

```csharp
[Gram("...")]
[Gram(@"...")]
[Gram("""...""")]
[DotGram.Gram("""...""")]
```

However, TextMate is syntactic and cannot reliably resolve:

```csharp
using DG = DotGram;

[DG.Gram("""...""")]
```

or distinguish an unrelated user attribute also named `Gram`.

Therefore:

```text
TextMate injection
    good for basic embedded coloring

semantic/LSP tooling
    must resolve actual symbols
```

## 14.3 Semantic tokens warning

Do not casually register a second semantic-token provider for all `csharp` documents.

Semantic token providers do not necessarily compose cleanly with the existing C# extension.

For embedded C# regions, safer options are:

- TextMate injection for basic coloring;
- targeted language-region routing;
- virtual documents for rich language features;
- explicit coordination with the host language service if required.

Avoid a design that competes globally with the C# semantic token provider.

---

# 15. LSP and virtual documents

Rich embedded-language tooling in VS Code may require virtual documents.

Conceptual flow:

```text
host C# document
    ↓
discover embedded DSL region
    ↓
decode DSL text
    ↓
create virtual document
    ↓
DotGram Language Server
    ↓
completion / diagnostics / navigation
    ↓
map positions back to host source
```

For nested languages:

```text
C#
    ↓
DSL virtual document
    ↓
SQL child region
    ↓
SQL virtual document
```

Source-map composition therefore needs to work recursively.

Do not assume every embedded language consumes the exact raw host slice. Some languages may operate on decoded/transformed text.

---

# 16. Tooling arbitrary DSLs created with `.Gram`

The long-term tooling goal is broader than editing `.gram`.

A user may create:

```csharp
[Gram("Filter.gram")]
public partial class FilterParser;
```

and later write:

```csharp
FilterParser.Parse("""
    Price > 10 AND Country = 'US'
""");
```

The editor should be able to recognize that this string contains the Filter DSL.

That requires a stable generated **Language Contract Metadata**.

---

# 17. API-to-language bindings

A generated parser method can describe the language expected by a string parameter.

Conceptual metadata:

```text
Method:
    FilterParser.ParseFilter

Parameter:
    input

Language:
    filter

EntryRule:
    Filter
```

Possible serialized descriptor:

```json
{
  "language": "filter",
  "entries": {
    "FilterParser.ParseFilter(System.String)": "Filter"
  }
}
```

Then:

```csharp
FilterParser.Parse("""
    Price > 10
""");
```

can be recognized by tooling through normal symbol resolution:

```text
invocation symbol
    ↓
generated API metadata
    ↓
parameter 0 = Filter DSL / entry Filter
    ↓
apply Filter tooling
```

This is much stronger than searching C# source for string patterns.

---

# 18. User-defined attributes that carry a DSL

Tooling should also support user attributes whose string arguments contain a generated language.

Conceptual API:

```csharp
[Gram("Filter.gram")]
[GramLanguage("filter")]
public partial class FilterParser;
```

and:

```csharp
[GramEmbeddedLanguage(typeof(FilterParser))]
public sealed class FilterAttribute : Attribute
{
}
```

Then:

```csharp
[Filter("""
    Price > 100 AND Country = "US"
""")]
```

can be identified as Filter DSL.

The important relationship is explicit:

```text
FilterAttribute
    ↓
FilterParser
    ↓
Filter.gram / generated language descriptor
```

Do not infer that arbitrary attributes transitively use `.Gram`.

---

# 19. Generated language metadata

A generated parser should be able to carry enough declarative metadata for tools and agents to understand its language without the original project source.

Potential metadata:

```text
MetadataFormatVersion
compiler version
grammar hash
language id
file extensions
original grammar source
root/public rules
rule names
rule kinds
source spans
literals
keywords
captures
references
scopes/contexts
normalized recognition structure
semantic classifications
embedded-language bindings
API bindings
capabilities / feature flags
```

The exact schema should be intentionally versioned.

Do not serialize raw internal compiler object graphs as the public contract.

Internal types change too easily.

Define a stable descriptor model.

Possible names:

- `LanguageDescriptor`
- `GrammarDescriptor`
- `LanguageContract`
- `GrammarMetadata`

`ToolingDescriptor` is probably too narrow because IDEs are not the only consumers.

---

# 20. Metadata placement

Do not assume all metadata belongs directly in custom attribute constructor arguments.

Attributes are good for:

- discovery;
- small identifiers;
- version markers;
- hashes;
- relationships.

Large payloads should likely live in:

- generated static fields;
- embedded resources;
- assembly metadata/resources;
- generated compact tables.

Conceptual pattern:

```csharp
[GeneratedGramMetadata("A17F...")]
public partial class RulesParser
{
    private const string __GramMetadata = "...";
}
```

or:

```text
attribute contains descriptor key/hash
assembly resource contains payload
```

The goal is:

```text
small discovery anchor
    +
large declarative payload elsewhere
```

---

# 21. Assembly discovery

A major benefit of embedding metadata in the generated assembly is that tooling can understand referenced DSL libraries.

Example:

```text
NuGet package
    MyCompany.Rules.dll
        ├── generated parser
        └── .Gram language metadata
                    ↓
            IDE extension / LSP / agent
```

This enables tooling even when the original `.gram` file is not present in the consuming project.

Important safety constraint:

> tooling should inspect declarative metadata without executing arbitrary referenced assemblies.

Prefer:

- PE/metadata/resource inspection;
- Roslyn symbol metadata;
- declarative resource loading.

Avoid loading arbitrary assemblies into the IDE process merely to ask them what language they contain.

---

# 22. Current-project versus referenced-assembly discovery

For a project currently being edited, the compiled assembly may be stale or unavailable because the project does not build.

Therefore discovery should be layered.

Suggested priority:

```text
1. current source / generated Roslyn model
2. current generator output / compilation symbols
3. referenced assembly metadata/resources
```

Do not rely exclusively on the last successful build.

---

# 23. Agent-facing metadata

The same generated metadata useful to IDE tooling is useful to coding agents.

A generated parser can expose:

- syntax;
- valid keywords;
- operators;
- precedence;
- associativity;
- rules;
- captures;
- public entry points;
- embedded languages;
- semantic classifications.

This means an agent can understand:

```text
what strings are valid here
what constructs exist
what parser entry point expects
what a generated result contains
```

without reverse-engineering generated C#.

This is a strategic benefit of making the parser self-describing.

---

# 24. XML documentation

The source generator should eventually generate useful XML documentation for generated APIs.

Example:

```csharp
/// <summary>
/// Parses an <c>Expr</c> from the complete input.
/// </summary>
/// <remarks>
/// Generated from the <c>Expr</c> rule of the embedded .Gram grammar.
/// </remarks>
public static Expression ParseExpr(string input);
```

Generated capture/result members:

```csharp
/// <summary>
/// Value captured by <c>name: Identifier</c>.
/// </summary>
public string Name { get; }
```

Avoid duplicating the full grammar into every member comment.

The full grammar belongs in authoritative language metadata.

XML documentation should explain:

- role;
- source rule;
- capture source;
- entry rule;
- generated relationship.

Remember that compiled .NET XML documentation is usually a separate `Foo.xml` file beside `Foo.dll`; it is not normal CLR reflection metadata.

Therefore:

```text
machine-readable assembly metadata
    = authoritative tooling/agent contract

XML documentation
    = human/IDE/API documentation
```

They are complementary, not interchangeable.

---

# 25. Tooling metadata via attributes

Conceptual attributes discussed so far include:

```csharp
[GramClassify(...)]
[GramLanguage(...)]
[GramEmbeddedLanguage(...)]
```

These names are proposals, not frozen public API.

The important semantics are:

```text
classification:
    grammar symbol/use site → semantic role

embedded language:
    grammar symbol/use site → language descriptor

language identity:
    parser → stable language id / extensions / metadata

API embedding:
    parameter or attribute argument → generated language + entry rule
```

Do not implement names merely because they appear in this document; first align them with existing `.Gram` attribute-generation conventions.

---

# 26. Generated attributes and no-runtime architecture

`.Gram` intentionally ships no runtime assembly.

Tooling-related attributes should preserve that model if possible.

If current architecture emits `[Gram]` support types into the consumer compilation, tooling annotations may follow the same pattern where practical.

However, tooling contracts that must be shared across independently compiled assemblies may need a different strategy.

Do not accidentally introduce a required runtime package just to share an enum such as `GramClassification`.

Potential solutions include:

- generated local attribute types with string/stable numeric values;
- compiler-recognized attribute shape;
- metadata resources independent of CLR type identity;
- a deliberately tiny tooling contract package only if absolutely necessary.

The default preference remains: no runtime dependency.

---

# 27. Tooling feature set for user-created DSLs

Given grammar metadata plus optional semantic annotations, generic tooling can potentially provide:

- syntax highlighting;
- semantic highlighting;
- diagnostics;
- completion;
- brace matching;
- Go To Definition;
- Find References;
- Rename;
- Document Symbols / Outline;
- folding;
- hover / Quick Info;
- embedded-language dispatch.

But do not claim all semantic features are automatic.

For example, grammar structure alone usually cannot determine:

```text
this identifier declares a symbol
this one references it
this capture is a type name
this one is a local variable
```

Those require semantic metadata or user hooks.

Distinguish:

```text
grammar semantics
    what text is syntactically recognized

tooling semantics
    what recognized constructs mean to an IDE
```

---

# 28. Name binding and semantic tooling

Future semantic tooling may require metadata such as:

```text
Declaration.name
    declares SymbolKind.Variable

Reference.name
    references SymbolKind.Variable

TypeReference.name
    references SymbolKind.Type
```

This is beyond simple coloring.

The metadata model should leave room for:

- declaration sites;
- reference sites;
- scope boundaries;
- symbol kinds;
- rename groups;
- navigation targets.

Do not bake this into grammar recognition semantics.

---

# 29. Grammar tooling as dogfooding

`.Gram` should eventually describe enough of its own language that the same generic tooling infrastructure used for user DSLs can help edit `.gram` itself.

Target direction:

```text
DotGram grammar
    ↓
DotGram language metadata
    ↓
generic DotGram tooling engine
    ↓
support for .gram
```

Some compiler-specific features will still need custom logic, but generic highlighting/navigation infrastructure should be reusable.

This is a useful architecture test: tooling should not be hardcoded exclusively for `.gram`.

---

# 30. Grammar Explorer / Language Explorer

A high-value optional tool is an explorer inspired by parser workbenches.

For `.Gram`, such a tool could expose more than a parse tree.

Potential panels:

```text
grammar source

bound grammar

RecognitionGraph

FIRST sets
FOLLOW sets
nullable rules
recursive SCCs

predictive choices
possessive repetitions
deterministic regions

ExecutionPlan / regions
    Direct
    Frame-only
    Resumable
    Derivation-backed

generated C#

input
    ↓
recognition trace
    ↓
accepted derivation
    ↓
typed result

language metadata

embedded-language regions
```

This would serve:

- language authors;
- compiler developers;
- performance investigations;
- documentation;
- teaching;
- agent debugging.

Do not make this a prerequisite for the first VS/VS Code extension.

---

# 31. Proposed shared project structure

A useful eventual layout:

```text
src/
    DotGram/
        source generator / compiler

    DotGram.Language/
        grammar lexer/parser facade
        semantic model
        source mapping
        language metadata model
        generic tooling operations

    DotGram.LanguageServer/
        LSP facade over DotGram.Language

tooling/
    DotGram.VisualStudio/
        VSIX

    DotGram.VSCode/
        TypeScript extension
        TextMate grammars
        LSP client

    DotGram.Explorer/
        optional future explorer
```

Do not force this exact structure if the repository conventions suggest a better layout.

The architectural requirement is separation:

```text
language intelligence
    shared

editor adapters
    thin
```

---

# 32. Shared language-service operations

`DotGram.Language` should ideally expose editor-neutral operations such as:

```text
ParseDocument
GetDiagnostics
GetClassifications
GetCompletions
FindDefinition
FindReferences
Rename
GetDocumentSymbols
GetFoldingRanges
GetEmbeddedRegions
GetQuickInfo
```

VS and LSP adapters translate editor protocols to these operations.

Avoid implementing semantic logic twice.

---

# 33. TextMate grammar role

TextMate is useful but limited.

Use it for:

- immediate `.gram` syntax coloring;
- VS Code embedded grammar coloring;
- low-cost first experience.

Do not rely on it for:

- semantic alias resolution;
- exact embedded attribute identity;
- semantic diagnostics;
- rename;
- references;
- cross-file navigation;
- generated language metadata.

Treat TextMate as a presentation bootstrap layer.

---

# 34. Semantic Tokens role

For standalone `.gram`, semantic tokens are appropriate once a semantic model exists.

For embedded DSLs in C#, integration is more delicate because the host C# extension already owns semantic tokens for the document.

Do not design the whole system around overriding the host semantic token provider.

Prefer region-aware or virtual-document strategies.

---

# 35. File extensions and language identity

Generated metadata should be able to identify a language independently of the parser class name.

Conceptual:

```csharp
[GramLanguage(
    "my-filter",
    Extensions = new[] { ".filter" })]
```

Exact API not fixed.

Language identity should be stable enough for:

- VS Code language ids;
- document selectors;
- VS content types;
- metadata discovery;
- nested-language routing;
- agents.

Do not use generated CLR type full name as the only language identity.

---

# 36. External grammar files versus embedded grammar source

A parser may be generated from:

```csharp
[Gram("Rules.gram")]
```

or from grammar text embedded in C#.

Tooling metadata should normalize both cases to the same logical language descriptor.

The origin may differ:

```text
external grammar source
embedded C# source
referenced assembly metadata
```

but consumers should not need separate semantic models.

---

# 37. Source-of-truth hierarchy

For current project editing:

```text
source grammar
    authoritative for text and current diagnostics

generated metadata
    authoritative for compiled/public language contract

referenced assembly descriptor
    authoritative when source is absent
```

Do not silently prefer stale compiled metadata over current source.

---

# 38. Nested source-map composition

For nested embedded languages, source maps may need to compose.

Example:

```text
C# raw string
    ↓ decode/indent map
Filter DSL
    ↓ captured SQL region
SQL
    ↓ regex literal region
Regex
```

Tooling needs:

```text
Regex decoded position
    → SQL source position
    → Filter DSL source position
    → C# host source position
```

The source-map abstraction should therefore support composition.

Do not assume only one embedding level.

---

# 39. Embedded text may be transformed

An embedded language region may not always be an exact host slice.

Possible future cases:

- escaped string contents;
- decoded entities;
- indentation normalization;
- template substitutions;
- string concatenation, if ever supported.

The initial implementation can restrict supported hosts to exact/tractable mappings, but the architecture should not hardcode identity mapping.

If a transformation cannot be mapped precisely, rich tooling should refuse or degrade safely.

---

# 40. Visual Studio first implementation plan

Suggested phases:

## Phase VS-1 — standalone syntax

- register `.gram` content type / extension;
- basic syntax classification;
- matching braces/groups if easy;
- no semantic model required beyond lexer.

## Phase VS-2 — diagnostics

- invoke shared `.Gram` compiler front-end;
- surface syntax/semantic diagnostics;
- maintain correct spans.

## Phase VS-3 — embedded `[Gram]` highlighting

- use Roslyn to locate actual `GramAttribute`;
- support raw strings first;
- build `IEmbeddedTextMap`;
- classify grammar tokens inside the string.

## Phase VS-4 — embedded diagnostics

- map grammar diagnostics back into C# source;
- ensure no incorrect squiggles.

## Phase VS-5 — navigation/completion

- rules;
- captures;
- builtins;
- C# seam after `@` where practical.

## Phase VS-6 — generated-language metadata

- discover user DSLs;
- recognize parser API string arguments;
- semantic classification.

---

# 41. VS Code first implementation plan

Suggested phases:

## Phase VSC-1 — `.gram` TextMate

- language registration;
- syntax coloring;
- brackets/comments/basic folding.

## Phase VSC-2 — C# injection grammar

- basic `[Gram(...)]` embedded coloring;
- common qualified attribute forms where syntax permits.

## Phase VSC-3 — LSP for standalone `.gram`

- diagnostics;
- completion;
- definitions;
- references;
- rename;
- symbols;
- folding.

## Phase VSC-4 — embedded virtual documents

- detect grammar strings;
- decode;
- source-map;
- route LSP requests.

## Phase VSC-5 — generated DSL discovery

- load Language Contract Metadata;
- identify parser API arguments and custom attribute arguments;
- route strings to proper language server.

## Phase VSC-6 — nested language routing

- language regions;
- child virtual documents;
- composed source maps.

---

# 42. Generated metadata implementation phases

Suggested order:

## Metadata-1 — discovery descriptor

Include only:

```text
format version
language id
grammar hash
entry rules
source grammar or source reference
```

## Metadata-2 — grammar symbol table

Add:

```text
rules
captures
source spans
publications
literals
keywords if explicitly classified
```

## Metadata-3 — classifications

Add:

```text
Rule/Capture → SemanticRole
```

with use-site override precedence.

## Metadata-4 — API bindings

Add:

```text
method parameter → language + entry rule
attribute argument → language + entry rule
```

## Metadata-5 — embedded language regions

Add:

```text
capture/use site → child language
```

## Metadata-6 — semantic symbol metadata

Optional future:

```text
declarations
references
symbol kinds
scope relationships
```

---

# 43. Validation of tooling attributes

Any attribute that references a grammar rule or capture should be validated at compile time.

Example:

```csharp
[GramClassify("Identifer", ...)]
```

should not silently do nothing if the actual rule is:

```text
Identifier
```

The generator should report a diagnostic at the attribute argument.

Likewise:

```csharp
[GramClassify("Declaration.typo", ...)]
```

must fail if the capture path does not exist.

Tooling metadata must not become a stringly-typed configuration swamp.

---

# 44. Metadata target syntax

Conceptual target strings currently discussed:

```text
Identifier
Declaration.type
Declaration.name
Query.sql
```

The exact target syntax should be specified before public release.

Requirements:

- deterministic;
- validateable;
- stable under grammar structure rules;
- supports rule-level and capture/use-site targets;
- can eventually address embedded-language regions;
- no ambiguity between same-named symbols in nested contexts/scopes.

Do not overcomplicate this in the first implementation.

---

# 45. Relationship with `context`

The grammar construct currently being designed as:

```dotgram
context (...)
{
    ...
}
```

is a grammar recognition/binding construct, not tooling metadata.

Tooling should understand it for:

- name resolution;
- navigation;
- semantic model;
- source structure.

But editor annotations must not alter contextual rule binding semantics.

The same orthogonality rule applies.

---

# 46. Diagnostics philosophy

For tooling, prefer:

```text
correct broader location
```

over:

```text
precise-looking wrong location
```

Prefer:

```text
declarative semantic metadata
```

over:

```text
heuristics based on names
```

Prefer:

```text
compile-time validation
```

over:

```text
silent metadata that has no effect
```

These principles match the broader `.Gram` compiler philosophy.

---

# 47. Performance expectations

The tooling architecture should avoid reparsing/rebinding the whole grammar on every editor keystroke if incremental editor APIs make narrower updates possible.

However, correctness and shared compiler reuse come first.

Do not prematurely fork a second lightweight parser for tooling.

The first implementation may parse the current embedded/standalone grammar afresh if latency is acceptable.

Measure before introducing a separate incremental parser.

---

# 48. Threading and editor process safety

VS and VS Code tooling runs in long-lived editor processes.

Avoid:

- arbitrary assembly execution;
- unbounded caches keyed by documents that never release;
- static global mutable compiler state;
- blocking UI thread on full project analysis;
- synchronous disk/assembly scans where avoidable.

Prefer immutable language descriptors and explicit caches with document/project lifetime.

---

# 49. Cross-editor consistency

A `.gram` file should produce the same:

```text
diagnostics
symbol resolution
semantic classifications
embedded regions
```

whether consumed by:

- Visual Studio;
- VS Code/LSP;
- CLI tooling;
- Explorer;
- agent metadata reader.

Editor adapters may present information differently, but semantic answers should come from the same shared language layer.

---

# 50. Non-goals for first tooling milestone

Do not attempt all of the following immediately:

- full semantic rename across C# and DSL boundaries;
- arbitrary nested third-party language servers;
- dynamic registration of every referenced DSL file extension;
- incremental parsing engine;
- complete AST editor/refactoring framework;
- live visualization of execution regions;
- debugger integration;
- syntax-aware formatting for every DSL;
- arbitrary string concatenation reconstruction in C#;
- host-language semantic analysis beyond what is necessary.

The first milestone should prove the architecture with a narrow vertical slice.

---

# 51. Recommended first vertical slice

The strongest first end-to-end scenario is:

```csharp
[Gram("""
    Identifier = ['a'..'z']+
    Keyword    = "let"
    Start      = Keyword & name: Identifier
""")]
[GramClassify("Keyword", GramClassification.Keyword)]
[GramClassify("Identifier", GramClassification.Identifier)]
[GramClassify("Start.name", GramClassification.Variable)]
public partial class Parser;
```

Visual Studio should:

1. identify the actual `GramAttribute`;
2. decode the raw string;
3. parse it using shared `.Gram` language services;
4. syntax-highlight the grammar;
5. report grammar diagnostics inside the C# string;
6. apply semantic classification;
7. map all spans correctly back to C# source.

The same grammar saved as `.gram` should produce equivalent classifications and diagnostics.

This proves:

```text
shared grammar intelligence
source mapping
embedded-language handling
semantic metadata
editor integration
```

without requiring the whole future system.

---

# 52. Recommended second vertical slice

A user-generated DSL:

```csharp
[Gram("Filter.gram")]
[GramLanguage("filter")]
[GramClassify("Keyword", GramClassification.Keyword)]
[GramClassify("Identifier", GramClassification.Identifier)]
public partial class FilterParser;
```

Consumer:

```csharp
var filter = FilterParser.Parse("""
    Price > 10 AND Country = 'US'
""");
```

Tooling should:

1. resolve `FilterParser.Parse`;
2. discover generated language metadata;
3. determine that the string parameter expects `filter`;
4. parse/classify the string with the Filter grammar;
5. surface diagnostics inside the string.

This proves the central long-term proposition:

> `.Gram` can generate not only parsers, but languages understood by tools.

---

# 53. Recommended third vertical slice

Nested embedded language:

```text
Host DSL
    contains SQL region
```

Metadata:

```text
Host.Query.sql
    embedded language = sql
```

Tooling should:

1. parse Host DSL;
2. discover SQL source region;
3. create child language region;
4. route SQL text to SQL tooling or a generated `.Gram` SQL parser;
5. compose source maps back to the original host.

Do not start here; use it to validate that earlier abstractions were not designed too narrowly.

---

# 54. Acceptance criteria for initial tooling architecture

The architecture should be considered sound when:

1. Standalone `.gram` and embedded `[Gram]` use the same grammar intelligence.
2. Embedded grammar spans map correctly for raw/verbatim/regular strings supported by the implementation.
3. Rich tooling resolves the real `GramAttribute` symbol rather than matching its text.
4. Tooling classifications are independent of parser recognition semantics.
5. Rule-level classifications can be overridden at capture/use-site level.
6. Invalid metadata targets produce compiler diagnostics.
7. Visual Studio integration contains minimal grammar logic.
8. VS Code LSP integration contains minimal grammar logic.
9. Language metadata has an explicit format version.
10. Referenced generated parsers can be discovered without executing their assemblies.
11. The metadata model can represent embedded child languages.
12. No editor-specific color concepts leak into the grammar or stable metadata contract.
13. The design leaves room for user-defined DSLs and nested language regions.
14. Removing all tooling attributes leaves generated parser behavior unchanged.

---

# 55. Key architectural statement

The project should preserve this model:

```text
.gram
    defines recognition semantics

tooling metadata
    describes semantic/editor meaning

generated language descriptor
    bridges compiler and consumers

shared language service
    interprets grammar + metadata

Visual Studio / VS Code / LSP / agents
    consume the shared model
```

The tooling project should not become a second parser implementation, and the grammar should not become polluted with editor-specific concerns.

The long-term objective is a self-describing generated language:

```text
generated parser assembly
    ├── executable parser
    ├── original or normalized grammar metadata
    ├── language contract
    ├── semantic classifications
    ├── embedded-language relationships
    ├── API language bindings
    └── XML documentation
```

That makes the parser useful not only to application code, but also to IDEs, language servers, agents, documentation generators, and future tooling without changing the grammar's recognition semantics.
