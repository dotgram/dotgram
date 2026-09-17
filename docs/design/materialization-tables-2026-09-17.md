# Materialization table scope — 2026-09-17

## Finding and change

Each generated machine initially collects the value types of the rules it reaches.
`ShareValueTables` then replaces that list with the union used by sibling machines,
so that all of them agree on table numbers. Materializer local declarations used
that union too, including types the particular machine never reads or writes.

Keep the original subset when it is smaller than the union, and use it for the
engine and direct-reader materializer's local table bindings. The global numbering,
parser storage declarations and strategy-selection thresholds remain unchanged.
Single-machine parsers do not require an additional subset allocation. The engine
also avoids calls to `parser.MaterializationN()` for unrelated types; direct readers
avoid the corresponding unused table loads.

This does not merge user factories or materializers with different rule layouts.
An initial exact-body inventory found repeated short guards/readers, while the large
materializers differed. Combining those requires a separate equivalence analysis.

## Generated output

Compared the single-file baseline sources from the preceding production probe with
fresh generator output after this change. Source splitting remains disabled.
The baseline output predates this change and matches the layout retained by `c9ddc2f`.

| Project | Before, characters | After, characters | Removed table bindings |
|---|---:|---:|---:|
| Examples, including legacy FIX and ExpressionLanguage | 74,768,188 | 74,766,907 | 32 |
| SQL | 120,188,452 | 120,103,615 | 2,539 |

Removing the local table-declaration lines from both versions makes every generated
file textually identical. SQL source shrinks by 84,837 characters, approximately
0.07%; this is not a substantial generated-code-size reduction. No end-to-end build
speedup, parser-throughput gain or measured allocation reduction is claimed.
Generation-only exploratory timings were not collected as a controlled A/B benchmark.

[Source comparison](../../benchmarks/results/materialization-tables-2026-09-17.json).

## Validation

A regression grammar publishes independent recursive `int` and `string` rules.
Both the engine and tape reader materializers bind one table each, retain shared
numbering, and produce the expected values.

- Core: 8,296 tests; the initial run had only three expected snapshot mismatches.
  Reviewed Feed, Minimal and Notation diffs (table bindings only), updated them,
  and reran all five snapshot cases successfully.
- SQL: all 14,701 tests pass against the rebuilt Release assembly.
- Finance: all 3,808 tests pass against the rebuilt Release Examples assembly.
- C# 8 compatibility builds pass for net8.0, netstandard2.0 and net472.
- Generator, core/example and SQL builds succeed with zero warnings/errors.
