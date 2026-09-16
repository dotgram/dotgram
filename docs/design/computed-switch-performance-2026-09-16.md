# Computed switch: performance assessment, 2026-09-16

## Scope and conclusion

Main at `88ffd5a` was merged into the performance branch as `f2ed357`. The merge includes computed switch (`e75e5fe`), buffered-input pooling and final FIX fields ending at EOF. This assessment changes no production grammar or switch implementation.

Computed switch is useful for tagged or context-directed data: a cheap, authoritative discriminator selects one of a few actual syntactic shapes. FIX demonstrates this well. It is not currently a safe blanket performance replacement for ordinary alternatives in SQL or ExpressionLanguage. The present implementation rejects direct reader emission for a reachable computed choice and conservatively reports unknown FIRST information.

## What the implementation does

- Parsing/lowering represents the construct as a Choice with Selection metadata. The selector is bound through the same machinery as a guard.
- The machine compiles every branch, emits a helper whose C# switch maps a label to a branch index, then dispatches that index to the selected state. The selector is evaluated once per encounter. It can run again after enclosing backtracking.
- Failure of the selected branch does not select another case or default. Ordinary alternatives inside a case and enclosing alternatives retain their own behavior. Thus replacing overlapping ordered alternatives by cases is a semantic change unless the discriminator proves which branch applies.
- Selector captures have guard costs: locating captured entries and, for typed values, potentially materializing them during recognition. A string selector is ordinal/case-sensitive; it is not a free case-insensitive SQL keyword classifier. A large integer/string switch does not guarantee a constant-time JIT jump table.
- Character and byte buffered inputs are supported, including span captures. No byte-to-character conversion is required by the construct.

Implementation anchors: `GrammarNormalizer.Lowering.cs` (Expr.Switch), `Machine.cs` (selection compilation and guard dispatch), `Machine.Direct.cs` (DirectReachable), and `FirstSets.cs` (computed Choice FIRST result).

## Current constraints for large language grammars

1. **Direct-reader fallback.** Machine.DirectReachable refuses any reachable Choice with Selection. The entire affected publication group falls back to the engine, not just the switch node. SQL and ExpressionLanguage request lexical parsing; adding a computed selector can therefore give up an existing fast emission path. Lexical syntax support does not mean direct-reader support: the switch test explicitly filters the fallback warning for its lexical case.
2. **Conservative FIRST information.** FirstSets returns First.All for a computed choice. This can weaken prediction and follow/disjointness proofs around the node. Replacing it unconditionally by the union of branch FIRST sets would need care: selectors and capture materialization may have observable side effects or exceptions. Pruning a selector invocation is not automatically semantics-preserving. Start with compiler-owned, demonstrably pure discriminators if exploiting this bound.
3. **All branch code remains.** A switch with hundreds of distinct, unchanged bodies still emits those bodies. Large switches/helpers may themselves hit JIT size limits. The major size win comes from sharing syntactic shapes and moving tag-specific value construction into bounded helpers.
4. **Existing alternatives already have dispatch.** Ordinary choices use predictive selection and, where applicable, literal prefix tables. A new manual selector can duplicate lexing or classify input that the token stream already classified.

## FIX evidence

The checked-in runtime comparison (`benchmarks/DotGram.Finance.Benchmarks/results/2026-09-16-dispatch.md`) reports medians from three processes per implementation/workload, alternating order. These historical runtime numbers were read and assessed here, not rerun in this assessment:

| Workload | Fix44 | FixDispatch | Observed ratio |
|---|---:|---:|---:|
| Byte stream, 15-field order | 3.110 us | 1.501 us | 2.07x |
| Character stream, order | 4.368 us | 1.733 us | 2.52x |
| String, order | 5.445 us | 1.344 us | 4.05x |
| Byte stream, 3,011-field groups | 611.618 us | 370.574 us | 1.65x |
| Byte stream, binary payload | 49.193 us | 36.767 us | 1.34x |

These compare complete implementations, not isolated switch instructions. FixDispatch reads a general numeric tag, classifies ordinary text versus a length/data pair, and uses a separate typed-value factory. Ordinary streaming allocation is nearly unchanged in that report (+8 bytes per enumeration). Startup, malformed-input timing and network I/O were not measured there.

### Fresh generated-code measurement after the merge

The current Release generator harness emitted Finance sources at `f2ed357`. The generator reported no errors. Sizes below are actual UTF-8 file bytes, not assembly sizes, JIT code sizes or source-character counts:

| Artifact | Bytes | MiB |
|---|---:|---:|
| FixGrammar generated parser | 59,724,842 | 56.958 |
| FixDispatchGrammar generated parser | 488,683 | 0.466 |
| Additional checked-in FixDispatchFactory.Generated.cs | 125,370 | 0.120 |

The parser-source ratio is approximately 122x; counting the additional generated factory reduces it to approximately 97x. This is not a total-library footprint comparison: common field definitions and support code, as well as the handwritten dispatcher/context/options, are excluded. The smaller grammar is expected to reduce generation and C# compilation work, but no controlled build-time ratio was measured here. A single dump run is not a generator throughput benchmark.

Artifacts: `.work/generator-analysis/switch-finance-source/` and `switch-finance-generation.jsonl`. The report includes a fresh source-size measurement; the runtime table above remains the separately published comparison.

## Where to apply it

| Grammar / area | Assessment |
|---|---|
| FIX and similar tagged binary/text protocols | Apply now where the consumed tag/type/version uniquely determines the wire shape. Keep span-based integer classification cheap, share common text/numeric payload syntax and bound typed construction helpers. |
| SQL statement families | A possible experiment after direct-reader support: dispatch by an already available token kind into INSERT/UPDATE/DELETE/SELECT/procedural groups. Keep ambiguous WITH-prefixed forms grouped. Do not rescan raw text or allocate keyword strings merely to dispatch. |
| SQL CREATE/ALTER/DROP families | Select a proven group after a common prefix or a bounded token lookahead; retain ordinary alternatives inside ambiguous groups. These are candidates for measurement, not demonstrated wins. |
| ExpressionLanguage control statements | Potentially dispatch clear keyword families. However existing lexical prediction may already remove the same work, so expect less than the FIX restructuring benefit. |
| ExpressionLanguage declaration vs expression, casts, generic syntax, two forms of if | Do not commit based on the first word/token. The current If alternatives share the whole condition prefix and differ later; an if label cannot decide whether an else will follow. Preserve the inner ordered choice/common-prefix factoring. |
| Small parsers | Avoid introducing a classifier and engine fallback for a handful of cheap alternatives. Require a measured benefit and include cold/repeated-call costs. |

## Recommended implementation order

1. Add native computed selection to ReaderWriter, including provisional observation, captures, nested alternatives, replay and selected-branch failure. Remove the global direct-reader refusal only once those semantics are covered.
2. Add an internal decision representation that can use existing token kinds or proven prefix groups. Share it with ordinary predictive choices so automatic optimization works per grammar node without requiring wholesale grammar rewrites.
3. Preserve FIRST information only where selector purity and invocation semantics permit it; retain conservative behavior for arbitrary user C#.
4. Trial one SQL family and one ExpressionLanguage control group. Compare against the existing predictive parser, not a deliberately linear reference. Measure generation CPU/allocation, generated source bytes, C# build/JIT time, parser time/allocation, short and long input, invalid/truncated input and chunked streams. Keep AST, locations and failure behavior equivalent.

The immediate priority is direct-reader support. Mass conversion of the large grammars before that would mix a promising selection mechanism with a potentially much larger backend regression.
## Validation after the merge

- Release builds of DotGram.Tests and DotGram.Finance.Tests completed with zero warnings and errors.
- DotGram.Tests: 8,257 passed; DotGram.Finance.Tests: 3,801 passed; no failures or skips.
- Fresh Finance source generation reported no errors.
- A minimal lexical grammar with trivia and a computed switch reproduced GRAM5005. The equivalent structural probe using an ordinary choice emitted no diagnostic. This is a backend-selection check, not a performance or semantic-equivalence benchmark: the computed probe used a constant selector.

GRAM5005 also makes the semantic limitation explicit: fallback runs ordered character choices instead of the committed token-kind reading promised by lexical notation. This is another reason to implement direct-reader support before migrating lexical language grammars.
