# What `MethodImplOptions.AggressiveOptimization` on the recursive cycle buys and costs, 2026-09-26

Igor approved building this on a proposal that measured **−29% stack a level**, and then declined it
on these figures. **It is not in the tree**; the patch that implemented it has been deleted, and this
page exists so the answer is not rediscovered. It was built, it worked, and both halves of what it
does were measured.

## The benefit does not reproduce, and is zero where the budget is derived

Stack a level, three readings each, stable to 0.01 KiB:

| grammar | configuration | before | after | |
| --- | --- | ---: | ---: | ---: |
| SQL:2023 | Release | 3.410 KiB | 3.270 KiB | −4.1% |
| expression language | Release | 2.633 KiB | 2.360 KiB | −10.4% |
| SQL:2023 | Debug | 4.240 KiB | 4.240 KiB | 0 |
| T-SQL | Debug | 2.580 KiB | 2.580 KiB | 0 |
| expression language | Debug | 5.055 KiB | 5.055 KiB | 0 |

−4.1% and −10.4% against the −29% the proposal rested on. **Debug is zero**, and that is the half
that settles the benefit: a Debug assembly carries `DebuggableAttribute` disabling optimization, so
the attribute has nothing to act on — and Debug is the configuration the stack budget and the probe
interval are DERIVED from (`StackFrameBudgetTests`: "a consumer debugs their application"). So the
change buys the budget nothing at all.

## The cost, and it is the half that settles the whole question

The expression language is substantially slower — four rows, two carriers, every one the same
direction:

| row | before/control | after/control |
| --- | ---: | ---: |
| `el/nested-100.match` | 1.08x | 1.24x |
| `el/nested-refused-100.match` | 1.14x | 1.33x |
| `el/nested-400.match` | 1.10x | 1.25x |
| `el/nested-refused-400.match` | 1.12x | 1.33x |
| the same four, immediate carrier | 0.96–1.00x | 1.19–1.29x |

**14% to 33% slower.** These are `.match` rows with the same form on both sides, so not the
Match-against-bool artifact. It is the dynamic-PGO loss that `AggressiveOptimization` buys by
skipping tier 0, and the immediate carrier moving most is exactly where the attribute lands:
`!tape` readers are the immediate ones.

T-SQL is flat, every row within 0.01 of its ScriptDom ratio. SQL moves −6.2% to +5.9% with mixed
signs. Not `sql/refused-late.bool` at −51.5%, which is the Match-against-bool artifact and not a
result. First-call cost was deliberately not measured: it can only make the case worse.

## What the patch contained, since it worked

- the attribute on exactly the rules that carry the stack probe, `AggressiveOptimization && !tape &&
  Deepens(rule)` — the same predicate, because a rule that can reach itself is the one the recursion
  runs through. 42 sites in `SqlStandardParser`, 22 in each expression-language rendering;
- the capability asked of Roslyn at the transform that holds the compilation, of the ENUM and not of
  the framework's name, carried on `Host` → `GramCompilerOptions` → `CSharpEmitter` → `Machine`.

**The gate is `DotGram.Sql`, not `DotGram.Compatibility`.** Compatibility's grammars have no
`!tape && Deepens` rule, so the attribute is never emitted there whatever the flag says — it cannot
catch a broken check, and I nearly concluded the check was broken when net8.0 read zero.
`DotGram.Sql` targets `netstandard2.0;net10.0`: with the check both build and only net10.0 carries
the attribute; with the check bypassed netstandard2.0 fails with

    error CS0117: 'MethodImplOptions' does not contain a definition for 'AggressiveOptimization'

which was run deliberately, to prove the gate can fail.

## One thing about how these numbers were read

An earlier draft of this record said "steady state is within noise". That was read off ONE of the
three passes of the paired run: the `el/` and `tsql/` passes had not started, and the background task
had already reported "completed" with an empty stderr. Neither is evidence that a run finished.
**Count the output files against the passes asked for** — the cost above is what the missing passes
held.
