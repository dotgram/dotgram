# el/try and el/overloads, before/after 5b60607c → f84e3f4a, one paired process

For architect/expr, closing the two generated-side EL regressions the 2026-09-26 yardstick
diff.md flagged (`el/try` +35.2%, `el/overloads` +57.2%, over the much wider 46dec223→dbdb75ac
range). This is a different, narrower commit pair, chosen by expr for the specific fixes:
`f84e3f4a` ("what a name means inside a namespace is kept, and a repeated question builds
nothing" — the `Rooted` rule for overloads, `Qualified` kept for try).

`--stand-paired elbefore(5b60607c) elafter(f84e3f4a) --only el/ --repeat 5`, sides built and
frozen by expr (`side/elbefore`, `side/elafter`, emitted hashes differ: a real pair). Window
04:51:08-05:22:08, control 31.5 ns (31.7/31.4/31.5/31.6/31.5 across the five runs), **no run
dropped**. A/A of the parent taken automatically (`--only` implies `--aa-only` unless declined),
so every row's own change is read against what the same build shows against itself in the same
slot.

## Both rows close cleanly — real, large improvements, well outside the A/A noise floor

| row | reading | before/hand | after/hand | change | range over 5 runs | A/A of the parent |
| --- | --- | ---: | ---: | ---: | --- | --- |
| el/try | generated | 1.63x | 1.37x | -15.9% | [-17.1%..-14.7%], 0/5 positive | -3.2% [-5.2%..-2.0%], 0/5 positive |
| el/try | immediate | 1.63x | 1.37x | -16.0% | [-17.5%..-14.6%], 0/5 positive | -3.3% [-3.6%..-1.5%], 0/5 positive |
| el/overloads | generated | 2.33x | 1.53x | -34.4% | [-36.8%..-33.0%], 0/5 positive | -3.6% [-4.9%..-1.1%], 0/5 positive |
| el/overloads | immediate | 2.33x | 1.50x | -35.7% | [-36.5%..-34.2%], 0/5 positive | -2.3% [-4.1%..-1.5%], 0/5 positive |

Both rows' five-run ranges are entirely negative and sit well past the A/A of the parent's own
range (the control's noise floor, itself negative here by 1-5%, i.e. this slot ran a touch fast
throughout — the *excess* over that, not the raw change, is the effect, per D50): median change
minus median A/A is about -13pp for `el/try` (both carriers) and about -31pp/-33pp for
`el/overloads` (generated/immediate) — an order of magnitude past the parent's own noise, not a
few points of drift. Both carriers (generated/tape and immediate) agree closely on each row,
which is expected since neither fix is carrier-specific.

**Verdict: yes, both regressions the wider diff.md flagged are closed by this narrower range** —
consistent with (not yet independently bisected beyond) the two commit messages expr named.

## el/untyped, for the record — not part of this task, flag before reading it

`el/untyped generated`: change +6.4%, range [+0.2%..+9.3%] (5/5 positive) — but the A/A of the
parent in this same slot is +7.0% [-0.3%..+7.4%] (4/5 positive), which **overlaps almost entirely**
with el/untyped's own range. Unlike el/try and el/overloads, this one is not distinguishable from
this slot's own control noise and should not be read as a finding without a re-run — raised here
only because it's the adjacent row in the same table architect asked about separately in the
byte-column thread.

Raw `paired.md`/`paired.json` here; the A/A's own five runs (`aa-run-1`…`5`) and the pair's
(`run-1`…`5`) are on `T:\TEMP\dotgram-stand\el-paired`, not copied.
