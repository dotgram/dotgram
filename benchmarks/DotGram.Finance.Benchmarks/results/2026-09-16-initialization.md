# FIX short-input regression: initialization and materialization

## Result

The regression is per field, rather than a fixed parser startup cost. The generated
materialization helper is the main suspect: flattening 912 field alternatives into
one rule produces a much larger stack frame, cleared on every invocation. This is
stack initialization during field construction, not initialization of the parser
object. The internal parser is reused through its thread-local pool.

The grammar should remain simple. The compiler should partition construction
alternatives inside a large rule; currently `Machine.Materialization.cs` partitions
by rules, leaving a single large rule indivisible. This investigation does not
change parser generation or claim that the regression has been fixed.

## Warm benchmark

Compared the baseline from commit `910c40f` with the simplified grammar in
`b18b78b`, using the same assemblies as the [grammar comparison](2026-09-16-flat-fields.md).
Both versions run in the same process through compiled delegates. Inputs are
prepared outside timing. Each operation creates a MemoryStream and obtains the
lazy field enumerable; all phases except Create enumerate it fully. Setup checks
that both versions return the same number of fields. No semantic message assembly
or validation is included.

BenchmarkDotNet 0.15.8, .NET 10.0.12, SDK 10.0.400, Windows 11,
AMD Ryzen 9 9950X3D. ShortRun, InProcessEmitToolchain, three warmup and three
measurement iterations. Error is the 99.9% confidence interval half-width.

| Phase | Previous mean | Simplified mean | Previous error | Simplified error | Allocated, previous/new |
| --- | ---: | ---: | ---: | ---: | ---: |
| Create enumerable, no enumeration | 12.08 ns | 14.79 ns | 1.252 ns | 2.016 ns | 152 / 152 B |
| Empty input, fully enumerate | 83.19 ns | 82.04 ns | 12.318 ns | 20.044 ns | 4472 / 4472 B |
| One field | 466.14 ns | 584.55 ns | 10.690 ns | 163.924 ns | 4680 / 4680 B |
| One order, 15 fields | 6425.59 ns | 8287.93 ns | 225.693 ns | 3883.655 ns | 6968 / 6968 B |
| 16 orders, 240 fields | 100725.25 ns | 125310.72 ns | 14371.676 ns | 14956.452 ns | 44408 / 44409 B |

The 2.71 ns creation difference cannot explain the 1862 ns order difference.
Empty enumeration is effectively unchanged. The extra cost is approximately
118 ns for one field, 124 ns/field for one order and 102 ns/field for 16 orders.
These are short-run estimates, not precise attribution of time to individual
instructions. Managed allocation is essentially unchanged.

## Generated native code

Separate diagnostic processes ran the order workload 20,000 times with
`DOTNET_TieredCompilation=0` and `DOTNET_JitDisasm=*ReadFields_Bytes*`.
Tiering was disabled only for disassembly, not for the benchmark above.
Both versions compiled with **FullOpts**, explicitly marked optimized code.
The GRAM5003 warning is therefore not evidence that JIT optimization was disabled.

| Observed helper | Previous Part10 | Simplified Part1 |
| --- | ---: | ---: |
| Stack frame reservation | 7456 B (0x1D20) | 28168 B (0x6E08) |
| Main stack-zeroing loop | 6144 B (0x1800) | 28080 B (0x6DB0) |
| Native code size | 35932 B | 155191 B |

These are the materialization helpers compiled while processing this workload,
not totals for the entire parser. Other helpers also execute. The simplified
helper contains the construction dispatch for all 912 known-field alternatives.
Its prologue runs when materializing a known field, even for a tiny value.

Previous prologue excerpt:

```asm
lea      r11, [rsp-0x1D20]
call     CORINFO_HELP_STACK_PROBE
mov      rsp, r11
; ...
mov      rax, -0x1800
; loop: three 16-byte zero stores, add rax, 48, branch
```

Simplified prologue excerpt:

```asm
lea      r11, [rsp-0x6E08]
call     CORINFO_HELP_STACK_PROBE
mov      rsp, r11
; ...
mov      rax, -0x6DB0
; loop: three 16-byte zero stores, add rax, 48, branch
```

The previous helper reports 122 single-block inlinees; the simplified helper
retains calls to accessors such as ParserArena.get_Item and ParserEntry.get_RuleIndex.
The larger native method and changed inlining are additional plausible costs.
The root byte recognizer IL actually shrank from 8093 to 4038 bytes.

This establishes a concrete per-field overhead consistent with the measurements.
It does not isolate the exact fraction caused by stack clearing versus inlining,
dispatch, or instruction-cache effects. A follow-up compiler partitioning change
and same-run comparison are needed to establish the recoverable performance.
Cold first-use latency was not statistically benchmarked here.

## Reproduction

Build Release before measuring. Set DOTGRAM_FIX_BASELINE to the saved net10.0
assembly from commit 910c40f, then run:

```powershell
$env:DOTGRAM_FIX_BASELINE = 'T:/TEMP/fix-grammar-baseline/DotGram.Finance.dll'
dotnet run --project benchmarks/DotGram.Finance.Benchmarks -c Release --no-build -- --filter '*FixInitializationBenchmarks*' --job short --inProcess
```

For disassembly, use a separate shell, repeat for previous and simplified, and
change the output file for each variant:

```powershell
$env:DOTGRAM_FIX_BASELINE = 'T:/TEMP/fix-grammar-baseline/DotGram.Finance.dll'
$env:DOTNET_TieredCompilation = '0'
$env:DOTNET_JitDisasm = '*ReadFields_Bytes*'
$env:DOTNET_JitStdOutFile = 'T:/TEMP/fix-jit-simplified.asm'
dotnet run --project benchmarks/DotGram.Finance.Benchmarks -c Release --no-build -- --fix-jit-probe simplified
```

The probe also prints method IL sizes and one class-initialization observation;
the latter includes cold runtime effects and is not a startup benchmark.
