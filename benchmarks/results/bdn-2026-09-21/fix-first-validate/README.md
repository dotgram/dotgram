# The first call of validation: the walk against the generated rule

2026-09-21, window `fix-first-validate` 13:19:18–13:34:18, HEAD `402055eb` with no modified tracked
files, quiet check 1.7% of the machine, mask `0xFFFF` inherited by every process. `run.txt` holds
the announcement, the artifact hashes and the state; `output.txt` holds the table as it was
printed.

## What "the first call" means here

A process, and one act inside it. `fixfirst` loads the library, runs the type initializers, parses
one message, and only then validates it once. The figure below is **the wall time of that one
call**, in a process where nothing has validated anything before. It is not the whole path from
process start: the load, the initializers and the parse take about 14 ms before it and are timed
separately. The second call in the same process is 0.3–0.5 ms, nearly all of it the harness's own
reflection.

The `compiling` column is the part of that call the runtime spent in the JIT, read from
`JitInfo.GetCompilationTime()`. It is 92–99% of the wall time in every cell, which is the whole
story of this measurement: what is being paid for is compiling a method, not running it.

## The row

| message | road | wall ms (min/med/max) | compiling ms | methods | IL bytes |
|---|---|---|---|---|---|
| Heartbeat | the walk | 6.31 / **6.53** / 6.76 | 5.81 / 6.02 / 6.21 | 57 | 11 009 |
| Heartbeat | generated | 7.79 / **8.04** / 8.64 | 7.41 / 7.67 / 8.21 | 28 | 24 994 |
| Heartbeat | generated A/A | 7.72 / **8.12** / 8.39 | 7.38 / 7.74 / 7.94 | 28 | 24 994 |
| NewOrderSingle | the walk | 6.63 / **6.81** / 7.12 | 6.08 / 6.24 / 6.51 | 64 | 11 520 |
| NewOrderSingle | generated | 22.56 / **22.69** / 23.63 | 22.04 / 22.23 / 23.10 | 35 | 60 919 |
| NewOrderSingle | generated A/A | 22.48 / **22.82** / 23.41 | 22.03 / 22.33 / 22.88 | 35 | 60 919 |
| ExecutionReport | the walk | 6.60 / **6.91** / 7.12 | 6.04 / 6.34 / 6.51 | 64 | 11 520 |
| ExecutionReport | generated | 31.76 / **32.56** / 34.15 | 31.27 / 32.03 / 33.45 | 35 | 79 562 |
| ExecutionReport | generated A/A | 32.06 / **32.29** / 33.57 | 31.55 / 31.81 / 33.16 | 35 | 79 562 |
| TradeCaptureReport | the walk | 6.67 / **6.80** / 7.12 | 6.11 / 6.24 / 6.56 | 64 | 11 520 |
| TradeCaptureReport | generated | 34.99 / **35.65** / 36.62 | 34.48 / 35.17 / 36.07 | 35 | 90 064 |
| TradeCaptureReport | generated A/A | 35.66 / **35.98** / 36.32 | 35.17 / 35.47 / 35.84 | 35 | 90 064 |

Eleven launches a cell, one fresh process each, the three roads rotating so that none of them is
always the one that goes first.

## The resolution, read first

The two `generated` cells of a message are the same road twice. They differ by 0.08, 0.13, 0.27 and
0.33 ms. Every difference below is between 1.5 and 29 ms, which is five to ninety times that, so
the differences are the run's and not the hour's.

## What the row says

**The walk is flat and the generated road is not.** The walk costs 6.5–6.9 ms whatever the message
is, and compiles the same 11.5 KB of IL every time: it is one walk over tables, and the tables are
data. The generated road costs 8 ms for a Heartbeat and 35.7 ms for a TradeCaptureReport, and what
grows is the IL: 25 KB, 61 KB, 79.5 KB, 90 KB.

**The generated road has a shared part and a per-type part, and the numbers separate them.**
Heartbeat compiles 24 994 bytes although its own rule is a few lines; TradeCaptureReport compiles
90 064, and its own `ValidateDefault` is 65 131 of them. 24 994 + 65 131 = 90 125, which is the
whole of TradeCaptureReport's figure. So the shape is: about 25 KB paid once, for the standard
header and trailer and the code sets, plus that type's own method.

**Per byte of IL the generated code is cheaper to compile, and there is eight times more of it.**
The walk: 6.24 ms for 11 520 bytes, 0.54 ms/KB. The generated road at its largest: 35.17 ms for
90 064 bytes, 0.39 ms/KB. Nothing here is pathological about the generated code; it is simply
large.

**And the costs are paid differently.** The walk's 11.5 KB is the same 64 methods for every message
type: a process that validates one type has paid for all ninety-three. The generated road's methods
are disjoint — the IL differs per type in the table above — so a process pays again for each type
it touches. A session handling ten message types pays about 25 KB once and ten methods after it.

## All ninety-three, counted rather than extrapolated

Four methods of ninety-three is the wrong sample to multiply by ninety-three, so the IL was read
off the built assembly instead of guessed at: every `ValidateDefault` in
`DotGram.Finance.dll`, through `MethodBody.GetILAsByteArray`.

| | |
|---|---|
| generated methods | 93 |
| their IL, in total | **2 332 491 bytes** |
| mean | 25 081 |
| median | 29 786 |
| largest (`TradeCaptureReport`) | 65 131 |
| smallest (`Heartbeat`) | 332 |

They are not one size: 10 are under a kilobyte, 23 between one and ten, 17 between ten and thirty,
34 between thirty and fifty, and 9 above fifty. The shared part beside them is `FixStandard`
10 344 bytes and `FixCodes` 16 952, which agrees with the 24 994 a Heartbeat was measured
compiling.

**What a process that touches all ninety-three would pay, and what that figure rests on.** The four
cells give a marginal rate of about 0.43 ms per kilobyte of IL (a straight line through the
extremes reproduces the two middle cells to within 1.3 ms). At that rate 2 332 491 bytes is about
**one second** of compiling, against the walk's 6.8 ms for the same ninety-three types — which the
walk pays for whichever single type it is first asked about.

That second is an extrapolation of the RATE, and the rate was measured between 25 and 90 KB while
33 of the 93 methods are smaller than its floor, where a method's fixed cost weighs more. The IL is
counted; the time per byte is not. A harness mode that validates one message of every type in one
process would replace the product with a reading, and would take one short window.

**And it is a JIT cost, which is not the only way this ships.** What the package carries is IL;
what compiles it is the consumer's build. A consumer publishing with ReadyToRun or NativeAOT
compiles ahead of time and pays none of this. That is not a mitigation offered in the package's
favour -- it is part of the mechanism, and the figures above are the cost for a consumer who runs
the IL as we ship it.

**A second cost, which this window did not set out to measure.** The assembly is 3 389 440 bytes,
and 2 332 491 of that is the ninety-three method bodies. Built from the commit before the generated
file and from the one after it, both sides of every artifact the package ships:

| | before (`82b68221`) | after (`80ba6733`) | |
|---|---|---|---|
| `DotGram.Finance.dll`, net10.0 | 855 552 | 3 389 440 | ×3.96 |
| `DotGram.Finance.dll`, netstandard2.0 | 1 739 264 | 4 292 096 | ×2.47 |
| `DotGram.Finance.0.1.0.nupkg` | 1 833 756 | 3 018 474 | ×1.65 |

The download grows least because generated code compresses well: the nupkg is 65% larger while the
net10.0 assembly is nearly four times the size. Which of the three matters depends on what a
consumer is short of -- bandwidth once, or the bytes that are mapped and read at load, every
process, every time.

This is paid by everyone, at download and at load, whether or not they ever validate anything. The
first-call cost above is paid only by a consumer who validates, and disappears entirely under
ReadyToRun or NativeAOT; this one does not.

## What this does not measure

Steady state. The second call is 0.3–0.5 ms here, and it is the harness's reflection rather than
either road; what a warm validation costs per message is the BenchmarkDotNet work of the same day
(`fix-frozen-tree`), not this. This measures the price of the first message a fresh process sees,
which is the price a FIX session pays at start-up and never again.
