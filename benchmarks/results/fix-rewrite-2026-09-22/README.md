# The FIX message model rewritten: 49ecdaf9 against b9985c36

Two full `--stand` runs, `--only fix/,fixmsg/ --repeat 5`, each in an announced window of its
own (`run.txt` beside each: quiet check, artifacts, mask), and `--stand-compare` between them
(`compare.md`). Not a paired run: the paired stand's preflight holds every side's fields to the
hand parser of the running process by serialisation, and `FixField` lost `FieldType` in the
rewrite, so the older side disagrees on every field before anything is timed. That is the case
the stand's README names (a type both sides construct), and two runs with `--stand-compare` is
what it prescribes for it. Two processes, so the resolution is coarser than a pair's: the
`regex-lesser` readings, which touch none of this code, moved +1 to +8% between the runs, and
that is the width nothing below is read finer than.

`before` is 49ecdaf9, the last commit before the rewrite, read as parse then
`FixMessage.Validate()` (form 4). `after` is `DotGram.Finance` at b9985c36 in a tree at 0e948b69
(b9985c36 plus the stand's binder for form 5), read as parse then `Validate(FixContext.Default)`.
Both are parse-and-check.

## The message layer, `fixmsg/` (no hand base; absolute ns and bytes a call)

| row | before ns | after ns | change | before B | after B |
| --- | ---: | ---: | ---: | ---: | ---: |
| slope-8 | 544.9 | 412.9 | -24% | 1984 | 1248 |
| slope-12 | 765.0 | 577.8 | -24% | 2968 | 1600 |
| slope-13 | 838.2 | 620.8 | -26% | 3792 | 1688 |
| slope-20 | 1222.2 | 957.8 | -22% | 4960 | 2352 |
| slope-21 | 1304.8 | 995.3 | -24% | 6432 | 2448 |
| slope-36 | 2082.4 | 1655.8 | -20% | 8952 | 3888 |
| slope-37 | 2198.5 | 1692.4 | -23% | 11704 | 3984 |
| slope-68 | 3824.0 | 3085.5 | -19% | 16912 | 6960 |
| slope-69 | 3974.6 | 3125.8 | -21% | 22224 | 7056 |
| slope-132 | 7283.9 | 5970.7 | -18% | 32808 | 13104 |
| Order44.strict | 1146.1 | 949.6 | -17% | 4256 | 3112 |
| Order.parse | 1052.5 | 909.2 | -14% | 3456 | 3048 |
| Order.build | 1271.3 | 1119.5 | -12% | 3608 | 3200 |

`reference-QuickFIXn` on `Order44.strict` (their parse and `DataDictionary.Validate`, the same
library on both sides): 2398.6 and 2426.3 ns, so the ratio to it went from 2.09x to 2.55x.

The spread of the `fixmsg/` rows rose from 2-5% to 6-9% between the two runs.

## The field reader, `fix/`

Its code did not change in the rewrite beyond the property removed from `FixField`. Its `.text`
rows read +2 to +7% against their hand base and the regex readings beside them, which read no
code of this package, moved +1 to +8% the same way; `.bytes` and `.stream` rows moved -4 to +3%.
Nothing there is read as an effect of the rewrite.
