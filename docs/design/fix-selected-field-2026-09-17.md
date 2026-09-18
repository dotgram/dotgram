# Selected field reader experiment — 2026-09-17

## Decision

Do not retain the new emitter path yet. Preserve the previous scalar readers and
scalar guard optimization. Moving a whole selected field into a method while
retaining its rollback records did not produce a sufficiently stable gain to
justify another emitter path and larger generated code.

## Experiment

The baseline is the working-tree implementation described in
[scalar guards](fix-scalar-guards-2026-09-17.md), not main before that optimization.
Two implementations were tried:

- A manually edited generated Fix wire reader: read Tag and `=`, execute the
  selector once, scan a text value in the method, then return to the machine.
  Binary and empty text arms resume their original states after selection.
  The prototype omits the text Run and outer CaptureOpen records.
- A generic emitter recognizing that structure. It preserves those records when
  the follow set cannot prove that returning characters is unnecessary. In Fix,
  the computed follow set is `Anything`, so this conservative path retains them.
  Pinpointing and refining that approximation remains work for a subsequent change.

Both keep the scalar value cache and defer the outer field factory. Six additional
regression cases exercised success, an unoptimized selected arm, an empty value,
missing selection, prefix failure, and rollback inside the selected text arm.
The experimental generic emitter passed 77 switch/recovery tests, including these
six cases; Finance passed 6417 tests. Generated C# compiled under the core test
harness. This is experimental coverage, not a proof of full semantic equivalence.

## Results

Nine alternating batches per case, median timing, 1.5-second warmup, independent
assembly load contexts. Fresh forward/reverse processes, with no concurrent builds
or tests. The generic implementation was repeated for four processes. These are
exploratory measurements without confidence intervals.

| String workload | Prototype time reduction, two runs | Generic time reduction, four runs | Bytes/call, both |
|---|---:|---:|---:|
| One field, 7 chars | 9.3–17.0% | -2.2–6.0% | 184 |
| Order, 127 chars | 6.9–10.1% | 0.3–6.9% | 1448 |
| BinaryMany, 768 chars | 0.4–1.5% | -1.3–1.9% | 6744 |

Negative reduction means a regression. The generic source grew from 575,143 to
579,421 bytes (+0.74%). Managed allocation totals did not improve; fewer logical
entries in the prototype fit the same warmed retained arena. No working-set or
cold-start improvement is claimed. The optimization targeted string wire parsing;
these measurements establish no stream or Fix44 speedup.

## Next step

The useful target is eliminating proven-unnecessary arena entries and their later
walks, not method extraction alone. Refine the proof around delimiter/end-of-input
continuations, including lookahead, recovery and shared call sites, before enabling
whole-field readers. Keep the machine for ambiguous continuations and recursive
paths; the trial adds no unbounded recursive calls.

[Raw samples and hashes](../../benchmarks/results/fix-selected-field-2026-09-17.json).
[Rejected generic emitter and six regression cases](../../benchmarks/results/fix-selected-field-experiment.patch).
The patch applies on top of the current scalar-reader/guard work. Scratch prototype,
assemblies, harness output and test logs remain in `.work/fix-field-methods`.
