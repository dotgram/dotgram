# el/untyped, time only, 46dec223 → f84e3f4a, one paired process

For expr, the time half of `el/untyped` (the byte half is already settled: flat in my one-binary
read and expr's own domain across five commits — see commit `cc6b5abe`, the `Allocated()` fix,
and the diff.md correction it made). This covers the range the original yardstick's +33.8%
figure did not: that was `46dec223 → dbdb75ac` in a full run; this is `46dec223 → f84e3f4a` in an
`el/`-only run, so the two numbers below are not comparable to it, or to each other by
subtraction — only the ratio taken *inside this one run* is (rows share a PGO profile within a
run, not across runs, per expr).

Two false starts before this, both explained, neither a real result:
1. `--only el/untyped` alone (single row): the control itself read 87-178 ns across 5 runs — a
   real disturbance (sql's `DotGram.Sql.Tests` build started ~30s into the window; confirmed by
   sql-47). Harness correctly kept only 2 of 5 controls and refused to quote a median.
2. `--only el/untyped` alone, retried on a clean machine: the control now read a *consistent*
   142-150 ns across all 5 runs — not disturbance, but README's documented "a short probe in a
   cold process overstates its first rows" (a single-row run doesn't warm the process enough for
   even the control to be reliable). Harness refused again, correctly, for a different reason.

Fixed by widening to `--only el/` (the whole family — same as the el/try/el/overloads window,
which had no such trouble) and reading just the `el/untyped` row out of the result.

## The result

Window 06:16:30-06:31:30, control 31.5 ns steady (31.5/31.5/31.5/31.5/31.4 across the five runs),
**no run dropped**.

| row | reading | before/hand | after/hand | change | range over 5 runs | A/A of the parent |
| --- | --- | ---: | ---: | ---: | --- | --- |
| el/untyped | generated | 1.05x | 1.12x | +6.0% | [+3.1%..+12.2%], 5/5 positive | -0.7% [-0.7%..+10.0%], 2/5 positive |

**The two ranges overlap** ([+3.1%..+10.0%] is shared by both), so this does not close as cleanly
as `el/try`/`el/overloads` did. It also doesn't fully disappear into the A/A the way the untyped
row did in *that* window (where the A/A range there was [-0.3%..+7.4%], nearly identical to the
pair's own [+0.2%..+9.3%]) — here the pair's median (+6.0%) sits 6.7pp above the A/A's median
(-0.7%), and the pair's low end (+3.1%) doesn't reach the A/A's low end (-0.7%) at all. Read
plainly: a small, borderline-positive effect over this range that this one run can't cleanly
confirm or rule out — narrower than the yardstick's +33.8% by a lot either way, but not flatly
zero either. Byte columns for the record, not the question asked: 24,832/24,440 before/after,
-1.6%, well inside ordinary noise now that `Allocated()` is fixed.

Your call on whether this is worth a bisect over the 21 commits, or whether "not a clean
regression at this resolution, and nowhere near the yardstick's +33.8%" is enough to close it.
