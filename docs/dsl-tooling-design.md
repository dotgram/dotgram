# Generated DSL tooling design

This document narrows the arbitrary-DSL direction in
`DotGram_Tooling_Agent_Handoff.md` into an implementation plan for
`DotGram.VisualStudio`. It describes editor behavior only; tooling annotations must not
change what a grammar recognizes, emits, or returns.

## Available author contract

The generator emits these internal types into every consumer compilation:

- `GramLanguageAttribute` identifies a parser-host type and optionally claims file
  extensions.
- `GramClassifyAttribute` assigns a language-neutral semantic role to a rule or capture.
- `GramEmbeddedLanguageAttribute` on a parser host associates the language with a user
  attribute type that marks DSL-bearing string parameters.
- `GramClassification` contains semantic roles, never editor colors.

Tooling recognizes these types by namespace, metadata name, constructor shape, and
property shape. It does not compare CLR type identity and does not load a consumer or
referenced assembly.

## First supported scenario

The first vertical slice is a DSL parser and a custom attribute in the same solution:

```csharp
[Gram("Filter.gram")]
[GramLanguage("com.example.filter", Extensions = [".filter"])]
[GramClassify("Keyword", GramClassification.Keyword)]
[GramClassify("Identifier", GramClassification.Identifier)]
[GramClassify("Start.name", GramClassification.Variable)]
[GramEmbeddedLanguage(typeof(FilterAttribute))]
public static partial class FilterParser;

public sealed class FilterAttribute : Attribute;

public sealed class Query([Filter] string source);

var query = new Query("""
    Price > 10 AND Country = 'US'
    """);
```

Visual Studio should discover the language through symbols, decode the string with
`CSharpStringMap`, classify matched source spans by their rule/capture roles, and report
recognition diagnostics in C# coordinates.

The marker attribute is user-owned and carries no language data. Roslyn maps an argument
to the exact method or constructor parameter; the parameter's marker type then selects
the language. Ordinary strings and marker attributes used anywhere other than a string
parameter are not routed.

## Discovery pipeline

```text
C# string argument
    -> IArgumentOperation.Parameter
    -> marker attribute type on the exact parameter symbol
    -> parser host carrying GramEmbeddedLanguage(markerType)
    -> GramLanguage + Gram + GramClassify attributes
    -> grammar source and entry rule
    -> editor-neutral DSL document
    -> CSharpStringMap
    -> Visual Studio tags
```

Discovery is solution-scoped and compilation-backed:

1. Enumerate source types carrying a shape-valid `DotGram.GramLanguageAttribute`.
2. Read `DotGram.GramAttribute` from the same parser-host symbol.
3. Resolve embedded grammar text directly or a standalone grammar through the owning
   project's `AdditionalDocuments`.
4. Read every shape-valid `DotGram.GramClassifyAttribute` from the host type.
5. Read `DotGram.GramEmbeddedLanguageAttribute` from each parser host and resolve its
   `System.Type` constructor value to a user marker attribute type.
6. At a call or object creation, use `IArgumentOperation.Parameter` and compare the
   parameter's actual attribute symbols with the discovered marker types. Textual
   attribute names are never sufficient.

The cache key is the Roslyn `Compilation`, with per-document results invalidated when
the host syntax tree, grammar `AdditionalDocument`, or project references change. No
workspace-wide scan runs on a hover or classification request.

## Editor-neutral model

The Visual Studio adapter should consume a small model located with the existing shared
language sources and included directly by `DotGram.VisualStudio`:

```text
DslLanguage
    Id
    ParserType
    Extensions
    GrammarSource
    Publications
    Classifications

DslClassification
    Target
    Role
    AttributeLocation

DslEmbeddedSite
    Language
    EntryRule
    DecodedText
    SourceMap
```

`ParserType` and `AttributeLocation` are adapter data and must not leak into a future
serialized language contract. The editor-neutral result uses source offsets and stable
role names; the Visual Studio layer maps roles to existing classification types.

## Classification target binding

Targets use the first-cut syntax already emitted by the generator:

- `Rule` assigns a default role to every source span recognized by that rule.
- `Rule.capture` assigns a role to the source span captured at that use site and overrides
  the called rule's default role.

Target binding uses the same grammar document symbol model as navigation. It must report
unknown rules, unknown captures, duplicate targets, and malformed targets at the
attribute argument. Namespace-qualified targets are deferred until their unambiguous
surface syntax is specified.

The binder produces symbol identities, not a dictionary consulted by source spelling.
This keeps same-named rules in different grammar namespaces from being conflated later.

## Recognition requirement

Discovery and annotations are not enough to classify an input document. Tooling needs to
know which rules and captures recognized each accepted source span.

The extension must not:

- execute the user's generated assembly;
- compile and load transient consumer code;
- approximate the grammar with regular expressions;
- maintain a second parser whose acceptance semantics can drift from DotGram.

The required shared component is therefore a tooling trace over the compiler's normalized
recognition model. It should reuse the same normalized alternatives, calls, lookahead,
repetition, backtracking, `with`, and namespace specialization that feed code emission.
Its output is recognition success/failure plus rule/capture source extents. Construction
C#, guards that require user execution, recovery factories, and generated return values
are outside the first slice.

Before implementation, the parser work on `main` must expose a stable input to this trace
without making `Grammar/` depend on Roslyn or Visual Studio. If reuse would require
duplicating the emitted automaton, stop and define the versioned language contract first.

## Parser API arguments

Calls such as `FilterParser.ParseFilter(text)` are the second routing source. In the same
solution they can be related to a publication by the exact `IMethodSymbol`, parser-host
type, and the existing publication index. This is sufficient for an initial source-based
implementation.

A referenced parser assembly is not yet sufficient: the new attributes expose language
identity and roles, but not grammar payload, publication-to-parameter bindings, or a
normalized recognition contract. Referenced-assembly support therefore waits for a
versioned generated descriptor. Tooling must not infer it from generated method names.

## Implementation phases

### DSL-1: symbol discovery

- Add shape readers for the four generated types.
- Discover parser hosts and custom attribute carriers in the current compilation.
- Resolve embedded and `AdditionalDocument` grammar sources.
- Add tests using aliases, qualification, unrelated same-named attributes, malformed
  shapes, and multiple projects.

### DSL-2: annotation binding

- Bind rule and capture targets to `GramDocument` symbols.
- Apply capture roles over rule defaults.
- Surface target diagnostics at C# attribute arguments.
- Add an annotated DSL playground beside the current tooling playground.

### DSL-3: tooling recognition trace

- Produce rule/capture extents for a selected publication without executing user code.
- Preserve ordered-choice and backtracking behavior.
- Return stable failure spans suitable for editor diagnostics.
- Prove recognition parity against generated parsers on a bounded grammar corpus.

### DSL-4: Visual Studio integration

- Discover string arguments whose exact parameters carry a registered user marker.
- Map classification and diagnostics through `CSharpStringMap`.
- Reuse standard Visual Studio semantic classification categories.
- Keep classification incremental and cancel stale analyses.

### DSL-5: generated API calls

- Route string arguments of generated publication methods in source projects.
- Add Quick Info identifying the language and entry rule.
- Add referenced-assembly support only after the versioned descriptor exists.

## Deferred

- Dynamic registration of arbitrary file extensions.
- Referenced DSL libraries without source.
- Non-literal DSL arguments whose value cannot be mapped to one source span.
- Nested embedded languages.
- Symbol declaration/reference semantics, Rename, and Find References inside a user DSL.
- Running user construction code, guards, or recovery callbacks in the editor.

## Acceptance criteria for the first slice

1. Discovery uses actual Roslyn symbols and never attribute spelling.
2. Removing tooling attributes does not change generated parser output except for the
   attributes themselves.
3. Rule roles and capture overrides map to exact raw, regular, and verbatim C# string
   spans.
4. Invalid targets produce diagnostics at their attribute arguments.
5. Recognition results agree with generated parsers for the supported grammar subset.
6. No consumer assembly is loaded or executed.
7. Ordinary C# strings and unrelated same-named attributes receive no DotGram tags.
