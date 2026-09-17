# Splitting generated C# files: controlled FIX experiment

## Result

On baseline f7249e8, splitting the generated Fix44Grammar into partial files reduced
compiler time by approximately **37%** in two pairs with reversed order. This is a
positive compiler experiment, not yet a production generator change or a measured
full-solution improvement. No reliable peak-memory reduction was established.

## What changed

Preparation generated a fresh set of sources for DotGram.Examples, including the
legacy FIX grammar recently moved out of Finance. The main generated file contains
71,779,597 UTF-8 bytes. It became one file retaining fields, constructors and nested
types plus **28 files containing methods**. The total compilation grew from 74 to
102 syntax trees. The source is not reduced or algorithmically refactored.

Only direct method members move. All 5,088 moved methods retain their full original
text, verified by sorted SHA-256 multisets. Field and constructor order stays unchanged.
No method is divided. Groups target two million characters; a larger method stays
alone. The largest recognition methods contain about 5.1-5.3 million characters each.
The original partial class declaration is repeated with its namespace/usings context;
no attributes or base lists are duplicated in this fixture.

## Methodology

- Roslyn 5.9.0-1.26379.115 from SDK 10.0.400, hosted on .NET 10.0.12, Windows x64.
- Debug optimization, embedded PDB, concurrent compilation enabled for both variants.
- References: runtime platform assemblies plus the same built Finance assembly.
- Each observation runs in a fresh process. Order: original, split, split, original.
- Source text loading precedes the timer. Syntax trees are parsed in parallel; the
  timer then includes creation and emission of the CSharpCompilation. The generator
  and the preparatory splitting work are outside the timer.
- No builds, tests, profilers or other benchmark runs overlapped the timed compilations.
- These are two observations per variant, not confidence intervals. Compiler settings,
  reference selection and the runtime host differ from the earlier full-solution audit;
  its timings must not be compared directly to this table.

## Measurements

| Variant / run | Syntax parse, s | Total compilation, s | Allocated, bytes | Peak working set, bytes |
|---|---:|---:|---:|---:|
| Original 1 | 2.631 | 199.482 | 31,240,146,448 | 5,899,476,992 |
| Split 1 | 1.212 | 123.540 | 31,659,224,696 | 5,493,956,608 |
| Split 2 | 1.067 | 120.143 | 31,606,502,288 | 5,424,103,424 |
| Original 2 | 2.476 | 187.638 | 31,223,476,304 | 5,416,132,608 |

Median total time: **193.560 -> 121.842 seconds**, a **37.05% reduction**.
Both pairings show a large time decrease. Syntax parsing saves only about 1.4 seconds,
so most of the observed improvement occurs after syntax parsing. This does not by
itself identify the responsible Roslyn phase or prove a particular scheduling mechanism.

Total allocation is approximately 1.3% higher for split output. Peak working-set ranges
overlap (original approximately 5.04-5.49 GiB; split 5.05-5.12 GiB). The lower first-run
peak did not establish a repeatable memory improvement.

Assembly sizes including embedded PDB: 30,517,248 bytes original and 30,518,784 split
(+1,536 bytes). No IL identity or parser-throughput improvement is claimed: declaration
metadata order and debug documents can change even though method text is identical.

## Validation and reproducibility

Both experimental assemblies passed all **3,808 Finance tests**, including comparisons
between the legacy grammar and the production FIX parser, binary/character inputs,
locations, large tags and recovery. Each variant was tested by replacing the example
assembly in an isolated copy of the test host, without changing normal project outputs.
The method-text hash check passed before compilation.

The first preparation attempt detected a trivia-boundary mistake in the splitter and
threw an unhandled exception, producing a Windows dotnet.exe error dialog. The splitter
was corrected to preserve the opening-brace trailing trivia. The probe now catches
failures and logs them with a nonzero exit code. No measurements or test results from
that failed preparation are included.

[Raw observations and method inventory](../../benchmarks/results/file-split-2026-09-16.json).
The [standalone probe](../../benchmarks/CompilationSplitExperiment/README.md) preserves
the experiment. Its local outputs and logs are in `.work/file-split/`.

## Production follow-up

File splitting is worth implementing as a measured generator feature for large output.
Use boundaries already known by the emitter; do not add a second Roslyn parse of a
70-MB generated string to every generation just to discover method boundaries.

Keep small grammars in one file. Preserve field/initializer order, declaration-level
attributes, nested/generic type context, nullable/preprocessor state, source locations,
and stable unique hint names. Also account for CallerFilePath/CallerLineNumber effects
where applicable. Verify end-to-end generation plus compilation, SQL and ExpressionLanguage,
release/debug configurations, source/IL size and parser correctness before enabling it
broadly. The two-million-character group size is an experimental choice, not a tuned default.

## Production implementation

`GramCompiler` now returns additional sources for complete engine/reader groups of
at least 2,000,000 characters, and `GramGenerator` delivers them as separate syntax
trees. The emitter uses its own rendering boundaries; it does not parse its output
with Roslyn. Host fields (including dispatch tables), initializer order and attributes
stay in the main file. Nested reader types move whole. Small output stays identical.
`GramCompilerOptions.SourceFileSize = 0` retains one source; direct consumers must
compile every entry in `GramCompilation.Sources`.

This first implementation separates fewer members than the exploratory rewriter:
legacy FIX gets 12 additional engine files and a 9,769,989-byte main file, versus
28 additional files and a 1,569,888-byte main file in the experiment. It does not
pack all the smaller helper methods into separate files. The original 37% result
is therefore not a production performance claim.

### End-to-end observations

The production probe prepares handwritten trees and references, then times
`RunGeneratorsAndUpdateCompilation` and Debug DLL emission with embedded PDB.
It dumps generated sources after timing. Generator assembly, generated-tree parsing
and compilation run in one fresh process; the earlier exploratory probe compiled
previously dumped text and did not run the generator. These timings must not be
compared directly to the earlier table. No tests or builds overlapped the timed runs.

The baseline generator is from `8c34352` (generator code unchanged from `f7249e8`).
Both variants use the same project inputs, references and Roslyn/runtime binaries.
Run order: baseline FIX, candidate FIX, candidate FIX, baseline FIX, baseline SQL,
candidate SQL. FIX refers to the whole Examples project containing legacy FIX and
ExpressionLanguage, not solely one generated file.

| Variant | Generation, s | Generation + compilation, s | Allocated bytes | Peak working set, bytes |
|---|---:|---:|---:|---:|
| FIX baseline 1 | 5.890 | 133.978 | 36,424,893,392 | 5,473,533,952 |
| FIX candidate 1 | 6.312 | 130.622 | 36,894,603,376 | 5,783,588,864 |
| FIX candidate 2 | 5.521 | 123.509 | 36,916,462,832 | 5,765,419,008 |
| FIX baseline 2 | 5.961 | 135.963 | 36,405,033,256 | 5,495,615,488 |

The FIX two-run mean is **134.970 -> 127.065 seconds (-5.86%)**. Mean allocation
rises approximately 1.35%, and mean peak working set approximately 5.29%. Generator
time is effectively unchanged at this sample size. The observations support a modest
build-time improvement for this workload, not the exploratory 37%, a memory saving,
a confidence interval, or a measured improvement in Visual Studio solution build time.

Generated Examples sources: **38 -> 50 files**, **74,768,188 -> 74,772,016 characters**
(+3,828 characters for the repeated partial-file context). Parser method bodies and
public entry points are preserved. No parser-throughput improvement is claimed.

| SQL variant | Generation, s | Generation + compilation, s | Allocated bytes | Peak working set, bytes |
|---|---:|---:|---:|---:|
| Baseline | 27.683 | 73.066 | 38,491,527,264 | 5,680,173,056 |
| Candidate | 28.354 | 72.589 | 38,829,352,536 | 5,165,936,640 |

For SQL, **73.066 -> 72.589 seconds (-0.65%)** is effectively unchanged given
one observation per variant. Allocation rises 0.88%; the lower observed peak is
not sufficient to establish a repeatable reduction. SQL goes from 6 to 27 sources,
120,188,452 to 120,193,822 characters (+5,370).

Debug DLL sizes including embedded PDB: Examples 30,513,664 -> 30,514,688 bytes;
SQL 53,130,240 -> 53,095,424 bytes. Source paths and metadata ordering differ, so
neither DLL identity nor unchanged instruction layout is asserted.

### Production validation

- All 8,294 core tests pass, including forced separation for Flat/Adaptive/Paged
  storage, char/byte streams, typed guards, backtracking, generic/nested hosts,
  suffixes, imports, stable names and identical method bodies. Small-output snapshots
  remain unchanged. The large dispatch test compiles all generated parts at C# 8.
- The measured candidate Debug DLLs pass all 3,808 Finance and 14,701 SQL tests
  in isolated test-output copies; normal project DLLs are not replaced.
- The compatibility project builds at C# 8 for net8.0, netstandard2.0 and net472,
  with zero warnings/errors. The production measurement probe builds cleanly.
- Release builds of SQL and Examples/ExpressionLanguage succeed. The 14,701 SQL
  and 3,808 Finance tests also pass against Release output.

[Raw production observations](../../benchmarks/results/source-parts-production-2026-09-16.json)
include both generator hashes. The `generate` mode of
[CompilationSplitExperiment](../../benchmarks/CompilationSplitExperiment/README.md)
preserves the end-to-end procedure. Measurements here used the same procedure in
an isolated scratch host before promoting it to that checked-in mode.
