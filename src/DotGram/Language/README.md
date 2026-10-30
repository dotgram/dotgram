# The shared language core

This folder and `../Grammar/` are where the DotGram language is understood: how a grammar is tokenized, parsed, bound
and checked, and — for an editor — classified, navigated and described. The source generator compiles them to write
parsers; the Visual Studio extension compiles them to show the same language in an editor, and a language server for
other editors is to compile them the same way. Language knowledge belongs here, as functions over a `GramDocument`, a
`GrammarModel` or a `RecognitionGraph` returning plain data; an editor adapter maps that data onto its own protocol
and adds nothing the other editors would then lack.

## Who compiles it, and why sources rather than an assembly

The file list is `../Language.projitems`; every host imports it and compiles the same sources into its own assembly:
the Visual Studio extension (net472), the tests in `tests/DotGram.Language.Tests` and `tests/DotGram.Language.Tests.Slow`,
and the guard below. The generator compiles `Grammar/**` and three Roslyn seams in this folder — the scanner of `@(...)`
(`RoslynCSharpScanner`), the walk of included grammars (`GramIncludes`) and the reading of a `[Gram]`'s options
(`GramHostOptions`) — and leaves the rest of the folder out: an analyzer must not carry editor code or Roslyn's
Workspaces.

Nothing references the generator's assembly. Visual Studio already holds a copy of it — whichever version the open
solution uses — loaded by Roslyn as an analyzer, and a second copy of the same name in the same process is a hazard
with no gain. Nothing is lost by compiling instead of referencing: the generator and an editor never exchange objects,
only text (a grammar) and attributes read by name, so no type has to be the same type on both sides; and `internal`
stays internal, with no `InternalsVisibleTo` and nothing made public to be shared. A test project cannot both
reference the generator and compile these sources (the two copies of `Grammar/**` collide, CS0436), which is why the
tests of this folder have projects of their own.

## What may be referenced from here

- `Grammar/**`: yes. `Grammar/**` itself references nothing — not Roslyn, not this folder.
- Roslyn, including Workspaces (`Compilation`, `SemanticModel`, `Project`, `Solution`, `Document`): yes, for reading
  C#. A host hands these in; where they came from — a Visual Studio workspace, an MSBuild design-time pass, an ad-hoc
  workspace in a server — is the host's business.
- `Generation/**`: no. That is the generator's shell: Roslyn's incremental pipeline, its caches and reports. The files
  both sides need live here, not there, so that the rule has no exceptions.
- Visual Studio, LSP, WPF, MEF, the file system as policy: no. Those are the adapters'. `IncludedGrammarView`, which
  writes read-only copies of included grammars to a temporary folder for Visual Studio to open, is the one file here
  that still decides something about the file system, and is to move behind a seam the host provides.

## How this is enforced, and what is not enforced

Two projects compile these folders alone and are referenced by nothing; their output is the build succeeding.
`src/DotGram.Grammar.Guard` compiles `Grammar/**` for netstandard2.0, as the generator does, with no reference but
the polyfills and `System.Memory`, so a Roslyn type there does not build, nor a `using` of this folder or of
`Generation/**`. (A bare `using Microsoft.CodeAnalysis;` does: the polyfills declare their `EmbeddedAttribute` in that
namespace. Anything taken from it does not.)
`src/DotGram.Language.Guard` compiles `Grammar/**` and this folder for net472, the oldest host, with a reference to
`Microsoft.CodeAnalysis.CSharp.Workspaces` only, so a reach into `Generation/**` or into an editor does not build;
MEF, which Workspaces brings with it and .NET Framework carries, is taken out of what it compiles against, so an
`[Export]` does not build either.
Both are in the Linux solution configuration.

The guards hold the dependency line. They cannot see a feature re-implemented inside an adapter from knowledge that
exists here; that is held by the tests — the words the editor describes are the words the parser takes
(`EditorVocabularyTests`), every diagnostic the generator can report is shown or listed with the reason it cannot be
(`EditorDiagnosticsTests`), every grammar in the repository classifies without an unrecognized token
(`EditorCorpusTests`) and with the compiler's diagnostics under equal options (`EditorParityTests`) — and by review
against the rule above. A list written by hand from a table that exists elsewhere, a reading of `[Gram]` that is not
`GramHostOptions.Read`, and a tooling attribute's name spelled anywhere but `GramMetadataNames` are the defects they
exist to catch.

## Adding a feature

Write the editor-independent question here, with a test beside it in `tests/DotGram.Language.Tests`. Then teach each
adapter to show the answer: the Visual Studio project maps it to a tagger, a completion source or a command handler; a
language server maps it to the LSP request it answers. If a feature seems to need an editor type to be computed at all,
the boundary is in the wrong place: find the editor-independent part and put that here.

## Known models, not truths

The generated parser is the truth about what a grammar accepts. `DslRecognitionTrace` is a model of it that classifies
a user's DSL text without running the user's code; it is held to generated parsers by a parity test and may answer
"unsupported". Diagnostics about a project rather than a grammar — a host that is not partial, a grammar file that is
not an additional file — are the generator shell's, and an editor shows them only through the build. The editor reads
of a grammar's host only what changes the analysis it shows (`Lexical`, and the grammars it includes), and it resolves
no C# type a grammar names; diagnostics that depend on the rest are the build's. Analysis in an editor runs the whole
compiler, emission included, so that the warnings computed over the generated text are the build's; if that ever
becomes too slow for a keystroke, the fast tier that replaces it must say which diagnostics it does not promise.
