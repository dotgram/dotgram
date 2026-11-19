# A map of the code (contributors' guide)

[`implementation.md`](implementation.md) explains how the engine works and why. This page says
where each part of it lives: the stages a grammar goes through from a `[Gram]` attribute or a
`.gram` file to an emitted `.g.cs`, the folders and types of each, what goes in and comes out,
and the tests that hold it. Read it first, then the section of `implementation.md` it points to.
Every path is relative to the repository root.

## The two halves

`src/DotGram` is one analyzer assembly: two halves and the core it shares with the editors. The
lines between them are enforced by build guards:

- **`Grammar/`** is pure: a function of grammar text that references no Roslyn. What it needs
  to know about C# comes through two seams, `ISymbolResolver` (what a `@Name` or a C# type
  refers to) and `ICSharpScanner` (where an inline `@(...)` ends). `src/DotGram.Grammar.Guard`
  compiles it alone, so a Roslyn type there does not build. Inside it, the stages that read a
  grammar ask `ITargetLanguage` about the language the parser is generated in and know none
  themselves: C# is `Grammar/CSharp/` and `Grammar/Emit/`, and `src/DotGram.Grammar.Front.Guard`
  compiles the rest without them. The rules:
  [`.claude/rules/grammar-half.md`](../../.claude/rules/grammar-half.md).
- **`Generation/`** is the Roslyn shell: the incremental generator, its caches, the answers to
  the grammar's questions and the conversion of diagnostics.
- **`Language/`** is the shared language core that the editors compile from
  `src/DotGram/Language.projitems` together with `Grammar/`. The generator compiles only four
  of its files (`RoslynCSharpScanner`, `GramIncludes`, `GramHostOptions`, `CSharpStringMap`).
  What may go where: [`src/DotGram/Language/README.md`](../../src/DotGram/Language/README.md).

`GramCompiler.Compile` (`Grammar/GramCompiler.cs`) is the whole grammar half in one call, and
reading it is the fastest way into the pipeline below.

## The pipeline

Stages 1 to 3 are the Roslyn shell; 4 to 10 are `GramCompiler.Compile`; 11 is the shell again.
Each stage reports `GramDiagnostic`s (`Grammar/GramCompilation.cs`) and the next one runs
anyway; only emission is skipped after an error (`implementation.md` §0, §1).

**1. Finding hosts and grammars.** `Generation/GramGenerator.cs`, `GramGenerator.Initialize`.
In: the compilation's classes carrying `[Gram]` (and `[GramParserVariant]`), found with
`ForAttributeWithMetadataName`, and the `.gram` files among the additional files. A host's
options are read by `GramHostOptions.Read` (`Language/GramHostOptions.cs`), its includes walked
by `GramIncludes`. Before anything else `SupportEmitter` (`Grammar/Emit/SupportEmitter.cs`)
writes `DotGram.Attributes.g.cs`. Out: a host and the grammar texts it may read.
Tests: `tests/DotGram.Tests` `GeneratorDriverTests`, `ImportAmbiguityTests`,
`ReferencedGrammarTests`. Why: `implementation.md` §1 "The Roslyn shell", "What is written".

**2. Asked.** `GramGenerator`'s `Asked` reads the grammar with `GramCompiler.Read` (stages 4
and 5) and collects, in `Generation/Questions.cs` (`Questions`), the questions its C# names
raise. In: the grammar text and host. Out: the questions, worked out from the text alone and
cached on both. Tests: `IncrementalCachingTests`. Why: §1 "The Roslyn shell".

**3. Answered.** `Generation/RoslynSymbolResolver.cs`. In: the questions and the
`Compilation`. Out: the answers as plain values, which `AnsweredSymbolResolver`
(`Questions.cs`) hands to the grammar half as its `ISymbolResolver`. This is the only stage that
re-runs on an ordinary C# edit. Tests: `RoslynSymbolResolverTests`, `IncrementalCachingTests`.

**4. Lexing.** `Grammar/Parsing/GramLexer.cs`, `GramLexer.Tokenize`; `Token.cs`, `TokenKind.cs`.
In: the grammar text with the standard library (`Grammar/Std.gram`, `StandardLibrary.SplicedOnto`)
spliced on the end and included grammars joined by `GrammarSplice`. An inline `@(...)` is
measured through `ICSharpScanner` (over Roslyn: `Language/RoslynCSharpScanner.cs`). Out: a
`TokenList`. Tests: `GramLexerTests`, `CSharpScannerTests`, `SpliceTests`,
`StandardLibraryTests`, `UnicodeCategoryTests`. Diagnostics: `GRAM1xxx`.

**5. Parsing.** `Grammar/Parsing/GramParser.cs`, `GramParser.Parse`. Hand-written and recovering
to a declaration boundary. In: tokens. Out: a `ParseResult` whose `GrammarFile` is the syntax
tree, records in `Grammar/Parsing/Tree.cs` (`Decl`, `Using`, `Param`, …). Tests: `GramParserTests`.
Diagnostics: `GRAM2xxx`. Why: §0 ("one error, one message").

**6. Binding.** `Grammar/Binding/GrammarBinder.cs`, `GrammarBinder.Bind`; the model in
`GrammarModel.cs` (`Symbol`, `RuleSymbol`), the names a construction is handed in
`SuppliedNames.cs`. In: the tree and an `ISymbolResolver`. Out: a `GrammarModel`, every name
resolved to a rule, a capture or a C# member. Tests: `GrammarBinderTests`, `SuppliedNameTests`,
`ExternalNameTests`, `FreeNameTests`. Diagnostics: `GRAM3xxx`.

**7. Normalization.** `Grammar/Model/GrammarNormalizer.cs` and its partial files
(`GrammarNormalizer.With.cs`, `.Namespaces.cs`, `.Recursion.cs`, `.Trivia.cs`, `.Lexemes.cs`,
`.Transparent.cs`, `.Results.cs`, `.Checks.cs`, `.Factoring.cs`, …), `GrammarNormalizer.Normalize`.
In: the model. Out: a `RecognitionGraph` (`Grammar/Model/RecognitionGraph.cs`), the one structure
every later stage reads: `with` and namespaces specialized, left recursion rewritten into folds,
trivia and keyword boundaries woven in, types and results computed, checks, then factoring of
shared beginnings and pruning. Tests: `GrammarNormalizerTests`, `GraphIntegrityTests` (asks
`RecognitionGraph.Orphans()` of every grammar), `SemanticTests`,
`ConditionAndParameterTests`, `CallGraphTests`. Diagnostics: mostly `GRAM4xxx`. Why: §1, §5
(factoring), §9 (precedence), §10 (trivia and keywords), §11 (locations).

**8. Soundness and cost checks**, only on a grammar nothing above refused, each a type of its own
in `Grammar/Model/`: `Retention.Check` (which publications may stream, `Retention.cs`),
`FirstSets.Check` and `FollowSets` (where a repetition's end cannot be told), `TriviaOverlap.Check`,
`ExponentialRepetition.Report`, `SharedRecursion.Check`, `Determinism`. Tests: `RetentionTests`,
`FirstSetsTests`, `FirstSetsSettleTests`, `TriviaOverlapTests`, `ExponentialRepetitionTests`,
`DeterminismTests`; at scale `tests/DotGram.Tests.Slow` (`SharedRecursionTests`,
`ExponentialRepetitionScalingTests`). Diagnostics: `GRAM4xxx`, `GRAM5xxx`. Why: §5, §7.

**9. The lexical split** (only for `Lexical = true`). `GramCompiler.Cut`,
`Grammar/Model/LexicalSplit.cs` (`LexicalSplit.Of`), `TerminalInventory.cs` (the kinds),
`LexicalAutomaton.cs` (one DFA over every terminal). In: the graph. Out: a lexer and a second
`RecognitionGraph` over kinds, which every later stage reads unchanged. Tests:
`LexicalSplitTests`, `TerminalInventoryTests`, `LexicalBoundaryTests`, `LexicalContextTests`.
Diagnostics: `GRAM5004`, `GRAM5005`, `GRAM5011`. Why: §8.

**10. Emission.** `Grammar/Emit/`, entered at `CSharpEmitter.Emit` (`CSharpEmitter.cs`), handed
an `EmitContext`. In: the graph (or the syntax half of a split). Out: the C# text of one file per
host reading, and `.part-NNNN` files where it is large. The grammar is compiled as **machines**,
one per published rule and what it reaches (`CSharpEmitter.Published`, `Joined`); a `Machine`
(`Machine.cs` and its `Machine.*.cs` partial files, settings in `MachineSettings.cs`) is written
in the cheapest rendering it allows:

| Rendering | Where | `implementation.md` |
| --- | --- | --- |
| flat | `Machine.Flat.cs` | §2 |
| reader, a method per rule | `Machine.Reader.cs`, `Machine.Direct*.cs` | §2, §6 |
| buffered reader, over a stream | `BufferedEmitter.cs`, `StreamingEmitter.cs` | §2, §7 |
| engine, the automaton over an arena | `Machine.cs`, `Machine.Layout.cs`, `Machine.Graph.cs` | §4 |

Around them: carriers (`Machine.Carrier.cs`, `Machine.Immediate.cs`, `Machine.Materialization.cs`,
§3), recovery (`Machine.Recovery.cs`, §0), memo tables (`Machine.Memo.cs`), first-element dispatch
(`Machine.Scan.cs`, `Machine.PrefixTables.cs`, `ExpectedSets.cs`, §5), the lexer
(`LexerEmitter.cs`, §8), segmented input (`SegmentEmitter.cs`), streamed `yield`
(`YieldEmitter.cs`), the support types every parser carries as text, since no runtime
assembly ships (`Support.cs`, `syntax.md` §6.2), trace builds (`CSharpEmitter.Trace.cs`,
`Support.Trace.cs`), and the generated names (`GeneratedNames.cs`). Text is written through `Writer`. Tests: `CSharpEmitterTests`,
`SnapshotTests` (the grammars and their checked-in output in `tests/Snapshots`), `ReaderTests`,
`FlatLoweringTests`, `CarrierTests`, `BufferedInputTests`, `StreamedParseTests`,
`RecoveringReaderTests`, `LexerEmitterTests`, `GeneratedApiTests`, `GeneratedCompileTests`,
`RefusalTests` with the refusal corpus (`RefusalCorpus.cs`, `RefusalTests.txt`; the whole record
in `tests/DotGram.Tests.Slow`), `ReferenceDifferentialTests`; and outside `DotGram.Tests`,
`tests/DotGram.Compatibility` (the output built at C# 8 for netstandard2.0, net472, net8.0) and
`tests/DotGram.Determinism.Tests` (the output on .NET 8 equal to the snapshots). What may be
emitted into a consumer's assembly: [`.claude/rules/emitted-code.md`](../../.claude/rules/emitted-code.md).

**11. Compiled and Placed.** Back in `Generation/GramGenerator.cs`: the `Compiled` stage calls
`GramCompiler.Compile` with the answered resolver (kept across compilations by
`Generation/CompileCache.cs`); the `Placed` stage writes the host's line numbers into every
`#line` (`CSharpEmitter.Placed`, line maps in `Grammar/GrammarLineMap.cs` and
`Generation/InlineLineMap.cs`); delivery turns each `GramDiagnostic` into a Roslyn `Diagnostic`
(`Generation/Diagnostics.cs`, locations in `Generation/Report.cs`). An exception in any stage
becomes `GRAM0001` rather than a disabled generator. Tests: `IncrementalCachingTests`,
`CompileCacheTests`, `LineDirectiveTests`, `DiagnosticPlaceTests`, `GenerationReportTests`.

`tests/DotGram.Tests` reaches these at three levels: direct calls to a stage, the in-memory
`CSharpGeneratorDriver`, and the generator attached as an analyzer to a real project build (the
examples under `examples/DotGram.Examples`, held by `ExampleTests`).

## Around the generator

- **The editor core and the VSIX.** `src/DotGram/Language` turns a `.gram` document or a C#
  string literal into plain data with the generator's own grammar half (`GramDocument` calls
  `GramCompiler.Compile` for the diagnostics). `GramAnalysisSession` keeps one open `.gram` file
  analysed, `EmbeddedGrammarSession` and `EmbeddedGrammarFinder` the grammars in C# strings;
  `GramVocabulary` and providers such as `GramCompletionProvider` and `GramQuickInfoProvider`
  answer from that analysis. Tests: `tests/DotGram.Language.Tests`;
  every grammar of the repository in `tests/DotGram.Language.Tests.Slow` (`EditorCorpusTests`,
  `EditorParityTests`). `src/DotGram.VisualStudio` only adapts that data to Visual Studio
  (`GramClassifier.cs`, `GramCompletion.cs`, `GramQuickInfo.cs`, `EmbeddedGrammarTagger.cs`, …);
  `tests/DotGram.VisualStudio.Tests` runs on Windows only. It never references the generator
  assembly. Its own diagnostics are `GRAM61xx`.
- **The CLI.** `src/DotGram.Cli` (`dotnet dotgram`, [`cli.md`](../cli.md)) loads a project's
  compilation (`MSBuild.cs`, `Compilation.cs`) and runs `GramGenerator` in a
  `CSharpGeneratorDriver` (`Generator.cs`) beside the project's other generators
  (`AnalyzerGenerators.cs`), then writes the files. It adds no stage of its own. Tests:
  `tests/DotGram.Cli.Tests`, held to what a build writes (`BuildEqualTests`).
- **The packages built with it.** `src/DotGram.Sql` (`SqlStandardParser`, `Sql92Parser`,
  `TransactSqlParser`, grammars in `.gram` files), `src/DotGram.Web` (grammars in `[Gram]`
  attributes, one file per RFC), `src/DotGram.Finance` (`FixParser`, versions generated by
  `Fix/generate.cs`) and `src/DotGram.ExpressionLanguage` (`ExpressionParser`) are ordinary
  consumers: each references `src/DotGram/DotGram.csproj` as an analyzer, so they exercise the
  generator on real grammars. Their tests are `tests/DotGram.Sql.Tests`,
  `tests/DotGram.Finance.Tests`, `tests/DotGram.Finance.Fix44.Tests`, and the `Web` and
  `ExpressionLanguage` folders of `tests/DotGram.Tests`; the trace build of three of them is
  `tests/DotGram.Traced` with `tests/DotGram.Trace.Tests`. A change to the emitter shows up in
  their output, which is why the emission gates below rebuild them.

## How to add a diagnostic

1. Pick the stage that can see the problem; its range is a heading of `docs/diagnostics.md`.
   Take the next number in that range that is neither in use nor listed as retired.
2. Declare the id as a constant in the stage that reports it (as `GrammarNormalizer.FirstSetUnsettled`
   is), report it as a `GramDiagnostic` with a position and a length, and add its title and
   severity to `Grammar/DiagnosticCatalog.cs`. A shell diagnostic gets a `DiagnosticDescriptor`
   in `Generation/Diagnostics.cs` as well.
3. Add its row to [`docs/diagnostics.md`](../diagnostics.md): title, severity, what to do.
   `DiagnosticsReferenceTests` holds the code, the catalog and the page to one another.
4. Add a sample grammar, or the reason the editor cannot reach it, to
   `tests/DotGram.Language.Tests/EditorDiagnostics.txt`; `EditorDiagnosticsTests` fails until
   you do.
5. Test, in the test class of the stage, that the grammar is refused by this id at this place. A refusal of a construct also owes its row in
   [`status.md`](../status.md), held by `SemanticTests.Still_refused`.

## How to add a grammar construct

1. The language first: [`syntax.md`](../syntax.md) is the specification; a construct is
   described there before or with the code.
2. Front end: a token in `TokenKind`/`GramLexer` if it needs one, the syntax in `GramParser` and
   a record in `Tree.cs`, the words in `Language/GramVocabulary.cs` (`EditorVocabularyTests`
   holds the editor's words to the parser's).
3. Meaning: binding in `GrammarBinder`, then a lowering into the `RecognitionGraph` in the
   `GrammarNormalizer` partial file it belongs to. Lowering it onto nodes the graph already
   has usually means no emitter change. If a rendering cannot carry it, refuse it there with a
   diagnostic (above) rather than emit something that means nothing.
4. Tests at each stage it touches, a row in `status.md` in the column it reaches, and, if it
   changes what any grammar of the repository emits, the emission gates below.

## How to change what is emitted

The steps and the reasons for each are in [`development.md`](development.md) ("The snapshot
baseline", "Analysing the generator's own output", "What a change owes"). In short:

1. **Snapshot baseline.** `SnapshotTests` rewrites `tests/Snapshots/*.gram.g.cs` and fails once;
   read the diff, which is the review of the generated file. A change meant to be structural
   leaves `git diff --stat -- tests/Snapshots examples/` empty.
2. **The whole emission.** For anything in `Grammar/Emit`, compare every generated file of every
   project before and after (`-p:EmitCompilerGeneratedFiles=true`, both sides `-t:Rebuild`).
3. **Builds that assert.** `tests/DotGram.Compatibility` (C# 8, old frameworks) and the
   solution with warnings as errors: a warning in emitted code is an error at the consumer.
4. **Generated analysis.** `tools/generated-analysis/run-all.sh`, then `SarifGate check`
   against `tools/generated-analysis/counts.txt` and `pragma-inventory.sh check`; CI's
   `generated-analysis` job fails on any difference in either direction.
5. **Speed and generator time.** `--stand-paired` against the parent over the families whose
   output changed, and `benchmarks/Gate-Generation.ps1` for any change to the generator
   ([`benchmarks/README.md`](../../benchmarks/README.md)); `--carriers` before and after a change
   meant to move a grammar off the tape.
