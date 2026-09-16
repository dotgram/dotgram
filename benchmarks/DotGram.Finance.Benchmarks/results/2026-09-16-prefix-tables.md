# Experimental literal-prefix tables, 2026-09-16

## Enabling the strategy

```csharp
[Gram("Grammar.gram", PrefixTables = true)]
```

The same property is available on `GramOptions` and `GramCompilerOptions`.
It defaults to false. The shipping Finance host keeps the default; the flag was
set only for the experimental assembly below, then removed.

## Implementation

`Machine.PrefixTables.cs` builds a trie from disjoint leading literal strings,
then emits a compact transition table and a scanner returning the alternative
index. This uses the lexer's table-driven approach, specialized for finite
literal prefixes; it does not invoke the full lexical split or produce tokens.
The scanner reads ahead without advancing the parser position. It jumps directly
to the existing alternative entry, which still checks its literal and parses the
value. Construction and capture code are shared with the old path, not duplicated.

The initial strategy accepts at least four alternatives, a case-sensitive prefix,
and a bounded alphabet/table (width <= 128, conservative limit 262144 cells).
It walks sequences, captures, deferred constructions and atomic groups. It does
not follow rule calls, skip guards or choose between overlapping prefixes. Bare
literal choices retain their existing specialization. Machines over token kinds
and other execution paths retain their own implementations.

Unrecognized or incomplete prefixes use the original choice code, preserving its
failure positions, expected items and starvation handling. A recognized disjoint
prefix can skip other alternatives: they cannot match this input. A failed value
can still return to an enclosing choice, including FIX's unknown-field fallback.
The old strategy is unchanged when the option is omitted or false.

For one FIX byte publication, the ordinary-field table has 12516 integer cells
(50064 bytes); the binary-pair table has 406 (1624 bytes). Other publications have
their own tables. The original failure path remains in the generated assembly.

## Measurements

Release, .NET 10.0.12, Windows 11, Ryzen 9 9950X3D. The existing `profile` harness
was run without a profiler: three fresh processes per variant/workload, alternating
variant order between rounds. Each operation fully enumerates returned fields;
stream construction is included, assembly loading and delegate binding are not.
Order and tag workloads use 500000 operations after 50000 warmups; Groups uses
500 operations after 100 warmups. No builds or test jobs ran alongside measurements.
These are diagnostic loop measurements, not BenchmarkDotNet confidence intervals.

Order has 15 fields; Groups has 1000 entries, 3011 fields. Tag inputs use the same
one-character string value. All durations below are microseconds per input; the
parentheses give the three-run range. Characters means TextReader, Bytes means
Stream. Allocations are managed bytes per operation after warmup.

| Input | Workload | Default median (range), us | Tables median (range), us | Speedup | B/op default -> tables |
| --- | --- | ---: | ---: | ---: | ---: |
| Bytes | Tag1 | 0.937 (0.920-0.949) | 0.910 (0.902-0.925) | 1.03x | 4552 -> 4552 |
| Bytes | Tag100 | 1.300 (1.258-1.325) | 0.915 (0.915-0.937) | 1.42x | 4552 -> 4552 |
| Bytes | Tag198 | 3.142 (3.113-3.511) | 0.935 (0.935-0.935) | 3.36x | 4880 -> 4552 |
| Bytes | Order | 8.518 (8.507-8.608) | 6.316 (6.312-6.369) | 1.35x | 6944 -> 5712 |
| Bytes | Groups | 6978.422 (6882.126-7012.074) | 1188.781 (1163.736-1198.698) | 5.87x | 702528 -> 173472 |
| Characters | Order | 12.631 (12.148-13.297) | 7.849 (7.604-9.103) | 1.61x | 10864 -> 9632 |
| String | Order | 16.194 (16.055-19.872) | 8.669 (8.603-10.067) | 1.87x | 1464 -> 1376 |

The late tag no longer pays for the preceding alternatives. Group input benefits
most, because its repeated tags were expensive to select. Removing unnecessary
choice entries also reduces allocation: byte orders by about 18%, groups by 75%.
The tiny difference on Tag1 should not be treated as a robust improvement.
Character/string timings vary more across runs; the table retains those ranges.

Materialization is not redesigned here. Its large helper remains a separate
optimization opportunity identified in the earlier profiling report.

## Cost and limitations

The default assembly is 15088128 bytes; tables is 15610880 bytes: +522752 bytes,
about 3.5%. Static table allocations happen during type initialization and are
excluded by warmup. Cold startup was not measured. Unknown/incomplete prefixes
pay for the table probe before the old path; this experiment does not establish
an improvement for invalid-input-heavy workloads or every grammar.

## Validation

- Six targeted tests compare values, error text and positions with the old path;
  include atomic groups, overlapping/duplicate prefixes, unknown-field fallback,
  truncated input, small method partitions, and byte/character reads of one item.
  Generated source is compiled under the test harness's C# 8 floor.
- All 3596 Finance tests pass with the final table-enabled assembly.
- Full core run: 8184 passed, with one unrelated failure for a missing existing
  heading in the specification's table of contents. The contents were corrected;
  both contents tests then passed. The six prefix tests passed after the final
  atomic-group change.
- The final default Finance builds succeed for net10.0 and netstandard2.0.
  The compatibility project builds successfully. Finance's pre-existing GRAM5003
  materializer-size warnings remain; this change does not address that method.

## Reproduction and evidence

Build Finance twice in Release, once with its current host attribute and once
with `PrefixTables = true`, preserving each net10.0 assembly separately. Return
the host to its default after collecting the second assembly. Run the existing
benchmark apphost, substituting either saved assembly:

```powershell
DotGram.Finance.Benchmarks.exe profile <assembly-path> Bytes Order 500000
DotGram.Finance.Benchmarks.exe profile <assembly-path> Bytes Groups 500
```

Other measured workloads are Tag1, Tag100, Tag198; other inputs are Characters
and String. The measured assemblies remain under T:/TEMP/fix-prefix/default
and T:/TEMP/fix-prefix/tables. SHA-256:

- default: `60D1078D1AC7E17DAF63B541E916915DB70E30C63980F6E945EA4518D4027C0F`
- tables: `7C35641208E952AC81152094E2063066C7AF7A59B4F8469F7AD9519B661DF221`

[Raw runs](2026-09-16-prefix-table-runs.txt) contain all 42 final measurements.
The first exploratory build only optimized binary pairs; its measurements are
not included in this comparison. Atomic-group support was added before the final runs.
