# One order, parse and schema, at 237b11bf

One announced window (`run.txt`: quiet check 2.4 %, artifacts, mask), `--only fixmsg/Order44.parse,fixmsg/Order44.strict --repeat 5`,
control 30.2 ns. Read beside the morning's `fix-rewrite-2026-09-22/order44-vs-quickfix` (parse, 38ac14af) and
`strict-default-loaded-quickfix` (strict, 38ac14af): the same rows, the same machine, the same day.

What changed between the two commits, in the order it was committed: the loaded schema replaces a slot rather than
adding to it (cfbdc324); the entry of a group is held to the schema (76bce7fc); a header field met after the body is
out of order and a field met twice is a duplicate, which is a null test in every arm of the field switch (in the same
two); the loaded field checks are written as `x switch { … }` over the codes (cb0bc30e); a constant is folded through
an implicit operator (7cc7d1e9); a whole-number field is a `long` and its codes are a pattern (2fcce891, e99be3a7); a
date, a time and a timestamp are `DateOnly`, `TimeOnly` and `DateTimeOffset` (817ff9a1).

| reading | 38ac14af | 237b11bf | change |
| --- | ---: | ---: | ---: |
| `Order44.parse`, generated | 940.4 | 1024.5 | +9 % |
| `Order44.parse`, QuickFIX/n | 1463.6 | 1459.0 | 0 |
| `Order44.strict`, compiled-in schema | 1020.5 | 1121.0 | +10 % |
| `Order44.strict`, FIX44.xml loaded over it | 1039.2 | 1236.0 | +19 % |
| `Order44.strict`, QuickFIX/n | 2442.9 | 2436.8 | 0 |
| bytes a call, generated | 3112 | 3048 | -64 |

QuickFIX/n read the same on both days, so the change is ours. What it is made of is not separated here: a paired stand
of the two commits would do that, and was not run. The candidates, by what each does on every message: the duplicate
test in every field arm and the header-order walk over the fields (`parse` and `strict` alike, +84 ns on `parse`), and
the loaded check being a whole message check written by the expression language rather than the compiled-in one plus
additions (the loaded reading alone, +19 ns over the compiled-in schema in the morning, +115 ns now). The `long` and
the date types took 64 bytes off a call and are not expected to have cost time on this order, which has no
whole-number field with codes and one timestamp.

**`Order44.parse` is bimodal by process.** Two of the five runs read 1806 and 1841 ns where the other three read
1013-1024, and in the same processes `Order44.strict`, which contains the same parse, read 1113 and 1133 like the
others, and QuickFIX/n's parse read as always. The same shape stood in the probe run before this one (one run at 1977).
The stand takes the median, so the row's figure is the 1024 the three agree on, and the spread column (81 %) says the
rest. Not chased: a reading that is 1.8x in some processes and not in others, on a row whose result is dropped where
`strict`'s is validated, looks like the JIT's choice (tiering, PGO of a call whose result is dead) and not the
parser's, and a paired stand or a run with tiering pinned would say.
