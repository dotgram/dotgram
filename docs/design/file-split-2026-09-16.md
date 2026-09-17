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
