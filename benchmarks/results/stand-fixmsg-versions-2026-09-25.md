# The stand's FIX rows for 4.2 and 5.0 SP2, 2026-09-25

The rows added in `93a521d6`, timed the day they were written. Internal, dated, and about this
machine on this afternoon: nothing here is a page a consumer reads.

    DotGram.Benchmarks.exe --stand <out> --only fixmsg/ --repeat 5

Tree `93a521d6` (the generated parsers are `5ffbe071`'s, which is what the run's own header says),
IGOR-DESKTOP, .NET 10.0.12, pinned to logical processors 0-15 at high priority, five runs each in a
process of its own, medians of the five. `--stand-check`: 128 rows, every reading agreeing.

**This run was taken on a machine that is not quiet, and could not be made quiet.** Over three
seconds before it started, 13.7% of 32 logical processors (4.4 cores) were in use outside it, and
**3.75 of those were one process**: `DevHub.exe`, Visual Studio 18's extensibility host, running
since 2026-09-22 with 2,029 minutes of processor time and an affinity mask of all 32 processors, so
it sits on the stand's half as readily as on the other. The stand's own rule refuses a window above
6%. What stands in for quiet here is the control and the spread:

- the control read **31.8 ns**, and the five runs' controls were 32.1, 31.7, 31.9, 31.8, 31.7 — a
  spread of 1.3%, and no run was dropped by the 5% gate;
- the spread column below is the base reading's spread between the five runs. **A difference
  smaller than the spread of its own row is not an effect**, and the two build rows (8.6-8.7%) are
  the loosest here.

## What the rows read

| row | median ns | bytes | fields | ns a field | spread |
| --- | ---: | ---: | ---: | ---: | ---: |
| `Order42.parse-string` | 1,084.2 | 2,648 | 21 | 51.6 | 4.1% |
| `Order42.strict-string` | 1,191.9 | 2,680 | 21 | 56.8 | 8.3% |
| `Order42.build-string` | 1,195.1 | 2,840 | 21 | 56.9 | 8.7% |
| `Order50.parse-string` | 1,716.9 | 4,424 | 25 | 68.7 | 3.5% |
| `Order50.strict-string` | 1,933.6 | 4,456 | 25 | 77.3 | 3.5% |
| `Order50.build-string` | 1,934.3 | 4,648 | 25 | 77.4 | 8.6% |
| `Report50.parse-string` | 4,363.7 | 7,976 | 45 | 97.0 | 2.0% |
| `Report50.strict-string` | 4,861.7 | 8,008 | 45 | 108.0 | 1.7% |

The 4.4 rows of the same run, for the scale rather than for a comparison:

| row | reading | median ns | bytes | fields | spread |
| --- | --- | ---: | ---: | ---: | ---: |
| `Order44.parse` | generated | 1,082.3 | 2,920 | 17 | 2.2% |
| `Order44.parse` | reference-QuickFIXn | 1,817.1 | 5,784 | 17 | 2.2% |
| `Order44.strict` | generated | 1,177.2 | 2,952 | 17 | 3.2% |
| `Order44.strict` | generated-loaded | 1,309.4 | 2,952 | 17 | 3.2% |
| `Order44.strict` | reference-QuickFIXn | 2,855.9 | 6,808 | 17 | 3.2% |
| `Order.parse` | generated | 963.8 | 2,864 | 16 | 5.1% |
| `Order.build` | generated | 1,116.9 | 3,016 | 16 | 3.9% |

## What may be said from this

**Not that one version is faster than another.** Every row reads a different message: the 4.2 order
carries its allocations as a group written inline, the 5.0 order carries the application's version,
a maturity date and time at an offset and two parties, and the 4.4 rows read two different orders
again. A version comparison would need one wire every version accepts, and the versions do not
place the same fields in an order, which is why there is no such row.

**Holding a message to the schema costs 9-11% of reading it, and nothing measurable in bytes.**
1,084 to 1,192 ns on the 4.2 order, 1,717 to 1,934 on the 5.0 order, 4,364 to 4,862 on the report,
each larger than its row's spread.

**The 32 bytes this file first read as validation's are the instrument's floor**, and the comparison
with QuickFIX/n taken the same evening settles it (its results are in
`DotGram.Finance.Benchmarks/results/quickfix-2026-09-25.md`): one 32-byte object of our parse path
is heap-allocated in tier-0 code and elided in
promoted code, so the same method under two names reads 2,648 and 2,680 in BenchmarkDotNet's own A/A
rows, on all three version shapes, with the copy always lower. Where two of our byte figures differ
by 32, that is the floor and not a cost; in that harness three of the five 4.4 shapes read the same
bytes with validation as without.

**Building over fields already read costs what parsing them costs.** `Order42.build-string` reads
1,195.1 against `Order42.parse-string`'s 1,084.2 and `Order50.build-string` 1,934.3 against
1,716.9 — 10% and 13%, against spreads of 8.7% and 8.6%. Those two differences are barely outside
their own resolution and are not quoted as a price; what the rows are for is that a change in
`BuildMessage` moves them.

**The cost of a field rises with the depth of what carries it**: 51.6 ns a field on the 4.2 order,
68.7 on the 5.0 order, 97.0 on the report whose sides carry parties and whose parties carry
sub-parties. The three levels of the report are the point of that row. This is one reading of three
shapes, not a curve: what it can say is that the per-field cost is not a constant of the layer.

## The loaded dictionary, and where its compiled checks are paid for

`e2d8204e` (2026-09-25) made a dictionary's checks compile **when one is first asked** rather than
when the dictionary is applied. One row of this family loads a dictionary — `Order44.strict`'s
`generated-loaded` reading, over QuickFIX/n's `FIX44.xml` and this repository's errata beside it —
so its first `Validate` now compiles what it asks for. **The row's warm-up absorbs it, and the
rounds say so rather than the reasoning:**

| run | warm-up iterations | round 1 | median of the timed rounds |
| --- | ---: | ---: | ---: |
| run-1 | 2 | 1,363.3 ns | 1,332.5 ns |
| run-3 | 4 | 1,266.9 ns | 1,297.5 ns |

A compile of those checks is milliseconds; the first timed round is within 3% of the median of the
twelve. So the compile happened inside the warm-up — in its first call — and no timed round carries
it. It cannot return later either: the context is one process-wide `Lazy`, and the row validates the
same message type every call, so everything it asks for is compiled by the end of the first call.

The number the row then reports, 1,309.4 ns against 1,177.2 for the compiled-in schema, is the
per-message cost of a loaded dictionary's checks and not of loading it. **What the load itself now
costs was not measured here**; the architect measured it with the change, outside a window, over the
same file: 20-90 ms and 9-16 MB, against 665-1000 ms and 206-278 MB before it (e2d8204e's message).
The 698 ms filed on 2026-09-24 was taken before the change and sits inside that older range, which
is two instruments agreeing about the same call; that file now carries both numbers. Unmeasured by
either: a session that validates many message types off one loaded dictionary, where the first
message of each type pays for its own checks.
