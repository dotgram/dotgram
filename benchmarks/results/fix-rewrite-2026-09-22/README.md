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

**Corrected the same day.** The `generated` reading of `Order44.strict` in these two runs was
`TryParseMessage` alone: the stand's binder for the schema check reaches the paired forms and not
this row, so the row read our parse against QuickFIX/n's parse *and* `DataDictionary.Validate`,
and the 2.09x and 2.55x it printed were never like against like. The commit that filed this
(8f206d4b) says "parse and check both sides" of that row, and that sentence is wrong; the numbers
of our side stand as what they are, the parse alone. The row is now two (aceedc78), and
`order44-vs-quickfix/` holds the reading of both, five runs, control 30.0 ns:

| row | work on both sides | ours ns | QuickFIX/n ns | theirs / ours | ours B | theirs B |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| Order44.parse | read, envelope checked, no schema | 940.4 | 1463.6 | 1.56x | 3112 | 5784 |
| Order44.strict | read, then the schema | 995.4 | 2420.1 | 2.43x | 3112 | 6808 |

Holding the order to the schema costs this package 55 ns and no bytes (a valid message makes no
finding, so nothing is allocated); it costs QuickFIX/n 956 ns and 1,024 bytes.

The spread of the `fixmsg/` rows rose from 2-5% to 6-9% between the two runs.

## The field reader, `fix/`

Its code did not change in the rewrite beyond the property removed from `FixField`. Its `.text`
rows read +2 to +7% against their hand base and the regex readings beside them, which read no
code of this package, moved +1 to +8% the same way; `.bytes` and `.stream` rows moved -4 to +3%.
Nothing there is read as an effect of the rewrite.

## Parse and schema on every reading: compiled in, loaded, and QuickFIX/n

`strict-default-loaded-quickfix/`: one order, five runs, control 30.5 ns. Every reading parses
and then holds the message to a schema; loading a schema is outside the reading on both sides
(a `Lazy` each). The loaded reading is `FixContext.Default.Load(FIX44.xml)`, the same file
QuickFIX/n validates with: its checks are written by the expression language at the load and
combined with the compiled-in ones, so this is what that road costs beside the compiled-in one.

| reading | ns | B | against ours compiled in |
| --- | ---: | ---: | ---: |
| ours, the compiled-in schema | 1020.5 | 3112 | — |
| ours, FIX44.xml loaded over it | 1039.2 | 3112 | +1.8% (5 of 5 runs above, +14 to +30 ns) |
| QuickFIX/n, FIX44.xml | 2442.9 | 6808 | 2.39x |

A loaded schema costs 19 ns on this order and no bytes: the loaded checks find nothing on a
valid message either, and a combined delegate is two calls where there was one.
