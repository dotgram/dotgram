# Benchmarks

Reusable handwritten parsers live in
[DotGram.Handwritten](../examples/DotGram.Handwritten/README.md).
The comparison and timing harnesses remain in this directory.

FIX message workloads and measured results are in
[`DotGram.Finance.Benchmarks`](DotGram.Finance.Benchmarks/README.md).

```console
dotnet run -c Release --project benchmarks/DotGram.Benchmarks
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --filter "*Url*"
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --against 9 200000
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --feed
```

Not run by CI. A number from a shared runner is a number about the runner, and a test
suite that fails when a machine is busy is a test suite people learn to ignore. The
project is in the solution so that it has to keep compiling.

## The stand: every generated parser against its hand-written one

```console
dotnet benchmarks/DotGram.Benchmarks/bin/Release/net10.0/DotGram.Benchmarks.dll --stand
dotnet benchmarks/DotGram.Benchmarks/bin/Release/net10.0/DotGram.Benchmarks.dll --stand --rebuild
dotnet benchmarks/DotGram.Benchmarks/bin/Release/net10.0/DotGram.Benchmarks.dll --stand-compare before.json after.json
dotnet benchmarks/DotGram.Benchmarks/bin/Release/net10.0/DotGram.Benchmarks.dll --stand-check
```

One run over FIX (a string, bytes in memory and a stream), the web's formats, the expression
language (by hand, on the tape and by the immediate carrier) and SQL:2023: time and allocation
per parse against the hand-written parser, the first call in a fresh process, what a lazily
streamed FIX parse holds, and what the generator took to write each parser, from the last
build's reports (`Stand.cs`, `StandWeb.cs`). `--stand-check` holds every row's readings to one
another and times nothing: run it after writing a row, before asking anyone for a quiet machine. Every row checks that its readings answer the same before it is timed, and a
round a generation 1 or 2 collection fell inside is redone rather than kept. The process pins
itself to logical processors 0 to 15 at high priority, and times a row of plain arithmetic in
every round, so that two runs taken on different days can be told apart from two parsers that
differ. Results go to `T:\TEMP\dotgram-stand\<time>` (`stand.md`, `stand.json`), or to a
directory named after `--stand`.

It times; so, under the rule every session here keeps, it runs in an announced window with
nothing else building. Reports of the generator appear only for projects the last build
actually compiled: `--rebuild` rebuilds every grammar-hosting project first (with `-t:Rebuild`,
node reuse and the compiler server off) so the table is complete; without it, a project whose
report is missing is named in its own section, with why.

### Medians, not runs

```console
dotnet benchmarks/DotGram.Benchmarks/bin/Release/net10.0/DotGram.Benchmarks.dll --stand --repeat 5
dotnet benchmarks/DotGram.Benchmarks/bin/Release/net10.0/DotGram.Benchmarks.dll --stand --only fix/Order.text,el/ladder --repeat 5
dotnet benchmarks/DotGram.Benchmarks/bin/Release/net10.0/DotGram.Benchmarks.dll --stand-paired beforeDir afterDir --repeat 5
dotnet benchmarks/DotGram.Benchmarks/bin/Release/net10.0/DotGram.Benchmarks.dll --stand-paired beforeDir afterDir --first --only sql/
```

A before and an after are quoted as medians of at least five runs, even on a quiet machine:
one run of the stand spreads 4-16% from the next there, and on a machine other people build on
a single run has been 85% off (2026-09-18: 56 runs under a build, medians within +-4% of the
quiet ones, single runs to +85%). `--repeat N` takes the run N times, each in a process of its
own, and reports each reading's median. A run whose control is more than 5% off the median of
the controls is dropped and named; fewer than three kept and it refuses to say a number. The
control sees a machine that is busy, not every kind of interference: a run can keep a clean
control and still have one row 50% off, which is what the other runs are for. The spread
column of such a report is the base reading's spread between runs, which is the one to hold a
change against.

`--only a,b` keeps the rows whose id contains any of the pieces, and leaves out the first
calls, the streamed run and the generator's reports, so that some rows can be looked at in a
minute. Its absolute numbers are not a full run's: the hand FIX parser reads `fix/Order.text`
at 940 ns in a first short run and at 670 in every other, so a short run is compared with
another short run and never with a full one. `--stand-check` times nothing and holds every
row's readings to one another. `--stand-paired --first` takes the first call of each reading of
each row in a fresh process each, median of five, before and after.

### Rows of a run share a profile

All the rows of one `--stand-paired` run are read in one process, so the runtime's dynamic PGO
has seen every one of them when it optimizes the code they share. A change that touches only some
forms of a parser can therefore look like a regression, or a gain, on the forms it does not touch:
the byte forms of FIX read +2.5..+6.5% slower in a run of 28 rows and -2.2..-5.8% faster in a run
of those byte forms alone, with the generated code of the byte path identical on both sides, and
flat (-1.3..+2.9%) under `DOTNET_TieredPGO=0` (2026-09-19). Read a change outside the forms it
touches against a run of just those forms and against the twin without PGO before believing it.

### The generator's time is a gate

Every full run holds the time the generator took per host to the previous base (`--against
previous.json`, or the newest `benchmarks/results/stand-*.json`) and names every host that
moved by more than 20% and by at least 100 ms, on a line of its own, `DEVIATION`, at the end of
`stand.md`. A commit that took T-SQL from 4 s to 86 s (2026-09-18) went unnoticed for an
afternoon; this is the line that would have caught it in the first run after it.
`--stand-gate now.json previous.json` prints it for two results already taken. It needs the
reports of a complete build, so a run for the record is `--rebuild`.

That comparison is history, not the gate to quote: the generator's milliseconds move with the
machine — the morning's base commit, rebuilt that evening, read 22-39% above its own report — so
a head is held to a base rebuilt in the same run. `benchmarks/Gate-Generation.ps1 -Base <commit>
[-Head <commit>]` rebuilds DotGram.Sql and DotGram.Examples in a worktree of each, base, head, base,
head, on the same cores, and names every host whose ratio of medians is more than 20% from 1 by at
least 100 ms. It is a timing run: take it in a window.

### The base of a row, and the regular expression

Every ratio in the report is taken against the row's first reading, which is the hand-written
parser wherever there is one. The web's formats have none — no one wrote RFC 3339 by hand —
so their rows (`web/*`) name the generated reading first and take their ratios against it; the
report says so above each table.

`tsql/*` (`StandTsql.cs`) is the other kind of row with no hand-written reading: Microsoft's
ScriptDom is the first reading and the base, a third party's parser, asked for the whole tree
of one statement, against the generated T-SQL parser without positions and with them
(`located`, the one to hold against ScriptDom, which always carries them). Five statements of
different shapes; agreement is that all three accept, and whether the trees say the same is
what `--roundtrip` and `--kinds` answer over the corpus. The long select is written in the
corpus's shape and not cut from it, since every fresh first-call process rebuilds the rows.

`feeds/stock-count.{small,good,broken}.{text,reader,reader64}` (`StandFeeds.cs`) is the stock
count of `DotGram.Examples` against `HandStockCount`: the string, and a `TextReader` at the
default buffer and at 64 characters, over the four-line count of the example's header, a
thousand good lines, and the same with every tenth broken. The stand references
`DotGram.Examples` for it, unlike the URL benchmark, which copies a grammar: a copy of an
example is a second parser to keep in step with the shipped one, and the hand parser
references the same example for the same value types.

Beside the parsers, a regular expression, where one can be written honestly:

| family | regex reading | what the pattern does less |
| --- | --- | --- |
| `fixmsg/Order.{parse,build}` | N/A | the schema-checked message layer, generated alone (there is no hand layer; the paired stand holds this tree's own layer as the control) |
| `fix/*.text` (plain rows and `slope-N`) | `regex-lesser`, `regex-compiled-lesser`: `(\d+)=([^\x01]*)\x01` | only the split into tag and value: no typed value, no length/data pair, no recovery. Held to the hand parser by field count, tag and where each value sits; a row with a binary pair or a malformed field has none |
| `web/url.*` | `regex`, `regex-compiled`: the pattern of `UrlBenchmarks` | three schemes, no relative references; held to RFC 3986's parser part by part |
| `web/date-time.*` | the ABNF of RFC 3339 §5.6 | no calendar and no leap-second rule (§5.7): it says yes to the thirtieth of February |
| `web/addr-spec.*` | the dot-atom form of RFC 5322 §3.4.1 | no comments, folding or quoted local part; held to `TryParseStrict` |
| `web/media-type.*` | RFC 9110 §8.3.1 | no case-folding, no unescaping of a quoted pair |
| `web/json.*`, `web/sf.*` | N/A | recursive languages: a regular expression cannot read them, so the row has the generated reading alone |
| `sql/*`, `el/*` | N/A | the same: a grammar of this size is not a pattern |

A pattern is built inside the row's first call, not when the rows are made, so the first-call
table charges it for its construction the way a user's first call would; and each is timed
interpreted and compiled. "Lesser" is in the reading's name where it does less work, so that
nobody reads `regex-lesser` faster than a parser as a win.

### The first call, by phase

`benchmarks/FirstCall` holds two small programs that take the first call of a parser apart in a
fresh process (sql-39's, adopted 2026-09-18): `fixfirst <directory> generated|hand|parse|build`
for FIX — the field parser, generated or by hand, and the message layer (`FixMessages.Parse`,
and `Build` over the fields already read), with `FixSchema`'s type initializer on its own — and
`sqlfirst <directory> <TryParse method> <input>` for SQL:2023. Each prints the phases (load, the
type initializers, the first parse, the second) with their time, how many methods the runtime
compiled in them, and the time spent compiling; `fixfirst` also lists the methods with their IL,
from the runtime's events, which cost time of their own — the anatomy, not the time, which
`--stand-paired --first` gives. The directory holds the DLLs of the build to look at.

### Linearity

`linearity` (`StandLinearity.cs`) times every parser that reads a long input at three sizes ten
times apart — the stock count, FIX orders and fields, JSON, a URL path, media-type parameters, a
structured list, a feed, SQL conditions, rows and columns, an expression — and prints the exponent
of each step, `log(t2/t1) / log(n2/n1)`, flagging a series above 1.2. Rough: one process, no
window, a few hundred milliseconds a cell. It exists because the stock count's generated parser
counted newlines from the start of the input for every rejected line and no row noticed until one
held a thousand of them. Beside each exponent it prints the KB a call allocates at each size and the
generation 2 collections a call causes at the largest, because a growing list on the large object heap
raises the exponent without the algorithm being wrong: a series above 1.2 is `GC` when its allocation is
linear and generation 2 collected, `ALGORITHM (allocation)` when the allocation grows faster than the
input, and `ALGORITHM` when the time does with neither.

### Tiered PGO

The stand's agreement check runs every row's readings on every row's input before anything is
timed, so the runtime's dynamic profile of a parser is taken over all of them, broken and
binary inputs among them, and then the timed rows run on that profile. A harness that checked
agreement on other inputs than it timed measured the generated FIX parser at 185 ns a field
where the stand had 140, for that reason (performance-ff, 2026-09-18). So a baseline is taken
twice, as is and under `DOTNET_TieredPGO=0`, and `--stand-compare` between the two shows the
rows the profile moves.

### `--stand-paired`: two builds, one process

```console
dotnet benchmarks/DotGram.Benchmarks/bin/Release/net10.0/DotGram.Benchmarks.dll --stand-paired beforeDir afterDir
```

`--stand-compare` runs `--stand` twice, in two separate processes, and compares the two
`stand.json` files afterward — which measures the two builds fairly only if nothing else
about the two runs differed. It caught a real regression as noise once (Q7.3, 2026-09-18:
Fix44's stream form, +10-13% two-process, +4% and every one of 16 alternating rounds
agreeing once measured properly) before `--stand-paired` closed that gap.

`beforeDir` and `afterDir` are two Release build output directories — each holding its own
`DotGram.Sql.dll`, `DotGram.ExpressionLanguage.dll` and `DotGram.Finance.dll` — loaded into
two isolated `AssemblyLoadContext`s in the one process running `--stand-paired`, so the same
process can call both builds' same-named types without collision. Every row `Workloads()`
times is timed here too, alternating round-robin between `before` and `after` exactly as
`--stand` alternates hand and generated; a row where either side disagrees with this process's
own hand parser is refused before anything is timed, the same rule as `--stand`. SQL's
agreement drops the tree comparison `--stand` makes — an AST type loaded into one
`AssemblyLoadContext` is not the AST type loaded into another — for accept/refuse against
hand alone, the same trade `--stand` already makes for a row both sides must refuse.

Reflection reaches only the generated side; the hand-written parsers stay this process's own,
since a paired compare targets a generator change and the hand parsers do not move with it
(D1). **That does not hold for a change to a type both sides construct** — FIX's own field
class, for one (2026-09-18: FixField's compact header). There, hand needs its own before/after
build too, so use two full `--stand` runs and `--stand-compare` instead of `--stand-paired`.
The reflection a paired run does use is a real, if fixed, per-call cost quoted nowhere against
`--stand`'s own numbers, held constant by construction on both the `before` and `after` reading
of a row.
The comparison the ratio between them carries is exact; the absolute nanoseconds a paired run
prints are not `--stand`'s and should not be pasted beside them. Results go to `paired.md`,
same directory rule as `--stand`; it needs the same announced window.

The three projects build to three different `bin` folders, so `beforeDir` and `afterDir` want
their `.dll`s copied together first — there is no build option that puts them in one place.
`--only substring` keeps rows whose id contains it, for a cheap rerun of one row a full run
flagged, without paying for the other rows again; useful when a row's own spread is wide
enough that one run's number is not worth trusting on its own (2026-09-18: `el/refused-late`
read +16% in a full run and settled to noise, -4.5% to +0.7%, over four `--only` reruns).

## Parser resource baselines

`ParserResourceBenchmarks` measures repeated SQL92 conditions at 1, 1000, 10000,
and 50000 predicates. The larger inputs exercise the capacity limit on retained
stores; `MemoryDiagnoser` reports allocations, not the memory left in those pools.
`TinyParserBenchmarks` measures a scalar recursive reader with space trivia,
including first-character and final-character refusals. Setup verifies the expected
success and scalar value before any timing. These benchmarks measure warm calls;
they do not measure cold-process startup or fresh-storage cost.

For a quick baseline after a Release build, run the following in a separate
PowerShell process, without concurrent tests or builds:

```powershell
$env:DOTNET_TieredCompilation = '0'
dotnet benchmarks/DotGram.Benchmarks/bin/Release/net10.0/DotGram.Benchmarks.dll --filter '*ParserResourceBenchmarks*' '*TinyParserBenchmarks*' '*ExpressionBenchmarks*' '*UrlBenchmarks.Grammar' --inProcess --launchCount 1 --warmupCount 3 --iterationCount 5 --iterationTime 100 --artifacts .work/strategy-audit/bdn-baseline
```

This short in-process run is a baseline for investigation, not a precise speedup
claim. Preserve its reports and compare candidates under the same runtime settings.
Use longer isolated-process runs to confirm small timing differences. Check the
ExpressionLanguage readings against each other with `--elbytes`, which verifies its
comparison corpus before printing allocations, and SQL:2023 with `--standard`, which
checks agreement line by line before it times anything.

## Why these exist

The architecture makes performance claims — `ReadOnlySpan<char>`, a state machine rather
than nested calls, `goto case`, no closures, no runtime parser graph, construction
deferred until a match is certain — and none of them were measured. A design rationale
that cannot be checked is a design rationale that drifts.

## Parser reuse

A parser on the engine owns reusable arena storage and exposes `RentParser`/`ReturnParser`
partial hooks, and the benchmarks here used to fill them in with a one-item thread-local
cache — kept in consumer code rather than hidden in the timing method, and safe under
reentrance, because renting clears the slot. Without reuse those benchmarks mostly
measured allocating and growing a new arena on every call.

Since 2026-09-03 none of them can: every grammar in this project is rendered by methods
throughout, and such a file has no arena, no `Parser` and no hooks to fill in — the tape
those methods keep is rented per thread by the generated code itself. The hooks remain
declared where the engine remains, and a consumer that had filled them in over a file
that has since gone direct loses them with the class, because what they rented, nothing
rents.

## The URL benchmark

`examples/DotGram.Examples/Formats/UrlExample.cs`'s grammar against the same language
written as a regular
expression, interpreted and compiled. The pattern is not a loose URL-shaped regex: it is
that grammar transcribed rule by rule, with the same character classes and the same named
groups, and **the run refuses to start until both sides agree on every input** — scheme,
user, host, port, path and query, part by part. A benchmark of two things that do not do
the same work is a number about nothing.

That check earned its place immediately. The first run failed on
`https://user@example.com:8080/a/b/c?q=1&r=2#top` because the transcription had `+`
outside the named group — `(?<user>…)+` captures the last repetition, so `user` was `r`
rather than `user`. Timing would have been perfectly happy to compare a correct parser
against a wrong pattern.

Both sides are asked for the parts, not for a yes. "Is this a URL" is a different and much
cheaper question than "what are its scheme, host, port and path", and the second is what a
parser is for.

Asked twice, because "the parts" turns out to be two questions and the two engines answer
them differently — one part read, and every part read. Both are timed, and the two tables
below are what each costs whom.

One of the five inputs does not match. A parser that is quick to say yes and slow to say
no is quick on the input nobody sends: refusal is where a backtracking engine does its
worst work.

### Current result

Windows, .NET 10, 2026-08-27, after the analysis work that closed the performance program
(`docs/next.md`). Measured with `--against` rather than `DefaultJob`, for the reason under
"Which instrument, and why" below. Two independent runs agreed to within 0.1 on every
ratio. Indicative, not stable CI thresholds:

| input | .Gram | Regex | Regex, compiled |
| --- | --: | --: | --: |
| `http://example.com` | 104.4 ns | 636.7 ns (6.10×) | 315.0 ns (3.02×) |
| `https://192.168.0.1/` | 110.1 ns | 613.3 ns (5.57×) | 304.0 ns (2.76×) |
| `https://exa mple.com/` — no match | 54.1 ns | 469.2 ns (8.67×) | 121.1 ns (2.24×) |
| a 47-character URL with every part | 277.9 ns | 653.0 ns (2.35×) | 306.0 ns (1.10×) |
| an 84-character path of eight segments | 170.9 ns | 1312.7 ns (7.68×) | 479.9 ns (2.81×) |

**Faster than `RegexOptions.Compiled` on all five**, and faster than the interpreted pattern
by 2.4× to 8.7×.

**The 47-character URL is the one to watch, and it has been both sides of parity.** It was
1.12× before the predicted-dispatch change, 0.99× after — that change removes work on every
input and lost this one to profile-guided block layout anyway, which `docs/next.md` records
under "What that change actually measured". Comparing a literal as one span put it back at
1.01×, and it stands at 1.10× today. A margin of a few per cent on this input is layout as
often as it is work, and is worth nothing without the `DOTNET_TieredPGO=0` check beside it.

### Asked for every part instead of one

The table above asks each side for one part, the host, and that is the pattern's shape of
question rather than this project's. `Group.Value` records where a capture was and cuts the
string when somebody reads it, so one group asked for is one string built. A publication
hands back a record with all seven parts already inside it, so one part asked for is seven
parts built. The table above is a comparison of seven strings against one, and it says so
in this file only because somebody thought to check.

The same five inputs, with every part read on both sides:

| input | .Gram | Regex | Regex, compiled |
| --- | --: | --: | --: |
| `http://example.com` | 105.6 ns | 757.4 ns (7.25×) | 445.1 ns (4.26×) |
| `https://192.168.0.1/` | 106.8 ns | 725.7 ns (6.59×) | 443.9 ns (4.03×) |
| `https://exa mple.com/` — no match | 49.0 ns | 471.9 ns (8.72×) | 121.6 ns (2.25×) |
| a 47-character URL with every part | 286.0 ns | 800.4 ns (2.88×) | 463.5 ns (1.67×) |
| an 84-character path of eight segments | 168.2 ns | 1438.6 ns (8.42×) | 634.4 ns (3.71×) |

**Asking for all seven costs this nothing.** Every row is within 5% of its own row above,
in both directions, and the allocation figures are identical to the byte. They were built
before the call returned; reading them reads fields. It is also the check that says whether
a run is worth reading at all — two measurements that must agree, and a run where they come
out 20% apart is a run something else was happening during.

**It costs the compiled pattern 32% to 52%** on the four inputs that match — 315→445,
304→444, 480→634 and 306→464 ns, and 32 to 208 bytes more each. Only the refusal is
unchanged, because a refusal has no parts to cut.

**The input that is level on the first table is 1.62× ahead on this one.** That is the whole
of what the two tables are for: the question the pattern is built for and the question this
is.

Neither table is the honest one on its own. The first flatters the pattern by asking for
the one thing it defers; the second flatters this by asking for everything it built anyway.
Together they say the deferral is real and worth something to a caller who wants one part,
and is a cost the moment the caller wants the parse.

### Which instrument, and why

`--against` measures the same six methods `UrlBenchmarks` does — through the benchmark class
itself, so the work is the same work — but round-robin: every method once per round, rounds
repeating, all in one process. `DefaultJob` is the better instrument for an absolute number
and cannot be the better one for a ratio, because it runs each case in a process of its own,
one after another: `.Gram` is measured at one minute and `Regex` at another, and a ratio
between them assumes nothing about the machine changed in between.

On an idle machine nothing does, and the two agree. On this one, three `DefaultJob` runs in
a row had to be thrown away — `.Gram` and `.Gram, every part`, which do the same work, came
out 21% and 28% apart, and in the third only three of the five input blocks were usable,
because BenchmarkDotNet runs blocks in sequence and interference is local in time.
`--against` came through the same conditions with every method's own spread between 0.3% and
6%, and two independent runs agreeing to within 0.05 on every ratio.

It also subtracts what the loop and the indirect call cost — 1.5 ns here — for a reason
worth stating: a constant added to both sides of a ratio drags the ratio towards one, so
leaving it in flatters whichever engine is slower.

Use `DefaultJob` for absolute nanoseconds and allocation on a quiet machine. Use `--against`
when what is wanted is the comparison, or when the machine is not quiet.

### Reading these numbers between runs

**Two of these five inputs are too short to compare between runs.** Run the identical
binary twice and `http://example.com` and `https://192.168.0.1/` — both around 140 ns —
move by 9% and 14%. The other three were once described here as holding to within 2%; that
was optimistic. The 47-character URL moved 6.6% (242.5 → 226.6 ns) between the two runs
this file has carried, on parsing code neither run changed. Two per cent is the floor for
the 84-character path and nothing else. A difference smaller than an input's own movement
is not a difference, whatever the compiled pattern beside it did. The one time this was
ignored, a 17% "regression" on the IP-host form survived a stable control
and three repetitions of a second instrument before five repetitions said it had never
been there (`docs/next.md`, "Three measurements said this was a regression").

**Compare the ratios between runs, not the nanoseconds**, and only for the inputs stable
enough to compare at all. Two runs back `Regex, compiled` sat at
365.5/328.2/138.9/332.8/570.6 ns on these inputs — the BCL got no faster in between, the
machine was quieter. Against that control the deferred-`Expected` change moved every
ratio (1.30→1.57, 1.30→1.64, 0.73→0.79, 0.75→0.78, 1.50→1.73), and the prefix-literal
change after it moved the refusal 0.79→0.83 and the long path 1.73→1.78.

**This table used to say uniformly 1.2×–2.6× slower.** That was true once — the numbers
below are what it was measured against — but nobody had re-run the benchmark since enough
of this project's own accumulated optimizations (possessive repetitions, predictive
choices, the parser kept between parses, typed value tables) landed to close most of the
gap. Re-measure before trusting either table; `docs/status.md`, "What has been measured"
carries whichever numbers were most recently refreshed.

The earlier numbers this table used to carry before that — 774 ns for the short URL,
1.84 us for the long path — were measured before those same optimizations landed.
`docs/next.md` keeps what each of those was worth, and `Membership.cs` and `Scanning.cs`
beside this file keep the experiments that were measured and rejected.

### Historical per-rule result

Windows, .NET 10, `--job short`, so these are indicative rather than publishable — ratios
against `.Gram` as the baseline:

| input | .Gram | Regex | Regex, compiled |
| --- | --: | --: | --: |
| `http://example.com` | 190 ns | 601 ns (3.2×) | 262 ns (1.4×) |
| `https://192.168.0.1/` | 137 ns | 527 ns (3.8×) | 254 ns (1.9×) |
| `https://exa mple.com/` — no match | 71 ns | 449 ns (6.3×) | 108 ns (1.5×) |
| a 47-character URL with every part | 238 ns | 548 ns (2.3×) | 261 ns (1.1×) |
| an 84-character path of eight segments | 408 ns | 1144 ns (2.8×) | 446 ns (1.1×) |

Read out of that:

- **Against interpreted `Regex`, 2.3× to 6.3×.** Expected: one side is generated straight-
  line C# and the other is walking a pattern at run time.
- **Against `RegexOptions.Compiled`, between 1.1× and 1.9× — the same order.** Also
  expected, and the honest reading is that the generated parser is competitive with the
  best the BCL does rather than in a different class. What it adds over that is typed
  parts, rules that compose, and a grammar somebody can read.
- **The gap is widest on refusal and on the short inputs**, and narrowest on the long
  path, where both engines spend their time in the same character-class loops.
- **Allocation is at parity** — 608–1144 B against regex's 1032–1240 B. Both materialize
  the parts as strings; neither is free. `Regex` allocates nothing when it fails, and
  `.Gram` allocates 72 B, which is small, real, and not yet explained.

### Worth knowing before reading too much into it

`Compiled` pays a large one-off cost that this does not measure — the regex is built once
in a static field, outside the timed region, which flatters it against a parser that has
no build step at all because the build happened at compile time. A benchmark that included
first-call cost would say something quite different, and neither number is the whole
truth on its own.

## The Documents benchmark

`Documents.cs`: the other everyday shape, and the one most grammars actually are — a
file of records with spacing and comments between every operand, values that are spans
of the input, a collection collected in reading order. The URL grammar cannot see any
of this: it has no trivia, so the seam machinery never runs, and its values were on the
engine before any of the value work landed.

Three inputs of the same four hundred entries tell the costs apart: dense (no seam
finds anything — the commonest call, and the fastest to get wrong), spaced (a seam at
every operand), and commented (line and block comments between records, exercising the
scanner's `IndexOf` path).

### Current result

Measured across the 2026-08 generator series (flat lowering scoped per machine, capture
hoisting, valued-flat and sited calls, scanner front tests and delimiter search, CFG
threading), against the state before it:

| | before | after |
|---|---:|---:|
| dense | 287.5 us | 18.9 us |
| spaced | 279.8 us | 19.7 us |
| commented | 276.9 us | 19.9 us |
| allocated per parse | 3.14 MB | 46 KB |

Re-measured 2026-08-27, after the analysis work that closed the series; the numbers
above are that run (BenchmarkDotNet, `--filter *Documents*`).

Fifteenfold in time and sixty-eightfold in allocation, and the 46 KB that remain are
the result itself: four hundred `Setting` objects and their strings. The before-column
allocation is what the review that started the series predicted — the arena wrote per
character, and the value tables grew with it.

The seam costs what it should: spaced runs within a few percent of dense, comments a
few more. The grammar writes its list the natural way — `entries: Entry*` — and §4.5
spaces it, because a repetition of a valued rule is a collection and collections are
separated the way operands are. A valueless repetition (`['0'..'9']+`, `Letter+`) stays
a lexeme; that line has its own semantic tests.

## What a parse costs before it reads anything

Every other comparison here times an input worth parsing, which answers what the engine
costs per character and hides what it costs per call. On `TransactSql.gram` that hardly
matters — the statement is long and the fixed part is lost in it. On a small grammar it is
the question: a parser called once per line of a log pays whatever is fixed on every line.

### A warm call, twice per grammar

`--filter *PreparationBenchmarks*`. `least` is the shortest input the grammar's own rules
accept; `real` is something somebody would hand it.

| grammar | shape | least | real | fixed share | least alloc |
| --- | --- | --: | --: | --: | --: |
| Levels | characters | 39.9 ns | 4,347.5 ns | 0.9% | 24 B |
| Config | trivia | 107.1 ns | 30,149.0 ns | 0.4% | 112 B |
| Url | characters | 99.1 ns | 199.6 ns | **50%** | 240 B |
| Sql-92 | kinds | 197.5 ns | 1,275.7 ns | 16% | 168 B |
| TransactSql | kinds | 1,394.7 ns | 14,123.7 ns | 10% | 928 B |

**Half of a real URL parse is the cost of making the call.** `least` is not zero-length —
`http://a` is eight characters against the real input's forty-seven — so the fixed part is
somewhat under half rather than exactly half, but the shape of the answer does not change:
this is a grammar whose whole job is short inputs, and a benchmark reporting 200 ns has
been reporting two things.

**Over kinds the floor is ten times higher.** `SELECT 1` costs 1.4 µs where `http://a`
costs 99 ns, and the reason is structural rather than a defect: a reading over kinds cuts
the input into tokens before the first rule runs, and on the shortest input that cut is
most of what happens. It is invisible in the SQL numbers everywhere else here because a
statement is long.

`Config` is the control: seven rules over four characters and over four hundred entries,
nothing but the input changed, and the fixed part is four tenths of one per cent.

### The first call of all

`--prepare [name]`. One call each, in the order printed, with nothing having touched that
grammar before — its statics built, its methods jitted at tier zero. No loop and no median,
because warming is the thing being measured, which is also why BenchmarkDotNet cannot ask
this and why the harness is eleven lines of `Stopwatch`.

| grammar | rules | first call | warm | ratio |
| --- | --: | --: | --: | --: |
| Levels | 4 | 7.9 ms | 39.9 ns | — |
| Url | 14 | 3.3 ms | 99.1 ns | 33,000× |
| Config | 9 | 2.1 ms | 107.1 ns | 20,000× |
| Sql-92 | ~130 | 9.6 ms | 197.5 ns | 49,000× |
| TransactSql | ~640 | 44.1 ms | 1,394.7 ns | 32,000× |

**The first row is not comparable**: whichever grammar goes first pays for runtime warm-up
the rest do not. Run `--prepare Url` and Url reads 3.2 ms while Levels, now second, reads
1.8 ms rather than 7.9. Read the rest.

**Forty-four milliseconds for the first T-SQL statement**, against fourteen microseconds
for the next one. For anything that parses a handful of documents and exits — a
command-line tool, a source generator, an editor extension opening a file — that is
essentially the whole bill, and none of the other instruments here can see it. It is the
first thing to look at in the generator, which is measured next.

## What a parse allocates

`--alloc` (`Allocation.cs`) asks the runtime what the thread allocated between two points
and divides by how many parses happened in between — exact, where `MemoryDiagnoser` gives
a rounded per-operation figure. 2026-08-25, before and after the deferred-`Expected`
change (`docs/next.md`):

| parse | before | after |
| --- | --: | --: |
| url, whole value | 400 B | 264 B |
| url, every part | 480 B | 352 B |
| url, host and path | 424 B | 392 B |
| url, no match | 440 B | 88 B |
| forty letters, one string | 168 B | 104 B |
| a hundred letters, one string | 288 B | 224 B |
| forty letters, kept as a span | 0 B | 0 B |
| twenty numbers, each a struct value | 2016 B | 784 B |

**A rejected URL was never free.** This file and `docs/status.md` both used to say it
allocated nothing; it allocated 440 B, and the furthest-failure set was what it spent them
on. Two changes took that to 88: not rebuilding the set on every step back, and then not
wording the message until somebody asks for it (`docs/next.md`, "a refusal says nothing
until it is asked"). What is left is the list that accumulates tied terminals during the
parse itself, which nothing can defer.

What is genuinely zero is a recognition whose value is its own extent — the two
`kept as a span` rows, where nothing is stored because the entry the rule completed into
already holds where it began and where it reached.

## What a rule boundary costs

`CallCost.cs` isolates the two things "one automaton instead of methods" (`next.md`) could
plausibly cost — going through the arena, and not reusing the parser — measured separately
so neither is blamed for the other's share. `--job short`, 2026-08-24, first numbers this
file has carried:

| | mean | allocated |
| --- | --: | --: |
| compiled in place, no arena | 568.5 ns | 168 B |
| called as an arena rule, default pooling | 711.2 ns | 168 B |
| called, an explicit one-slot pool | 698.3 ns | 168 B |
| called, a fresh `Parser` every call | 1276.8 ns | 11064 B |

**Going through the arena instead of being compiled in place costs about 25%** (568 ns
against 711 ns), with the parser pooled either way and allocation identical — this is the
one piece of "one automaton" overhead that shows up on every call regardless of allocation,
and the reason a silent, arena-free subtree of an otherwise arena-using grammar is worth
pulling out into its own method rather than leaving as a state in the shared one.

**The parser is pooled by default, without the consumer doing anything.** `Called as an
arena rule` uses no `RentParser`/`ReturnParser` override at all — the generated code's own
fallback (`Recycled()`, a thread-static one-slot cache) is what ran, and it already lands
within 2% of an explicit consumer-supplied pool. What actually costs — 2.25× the time and
66× the allocation — is a consumer explicitly forcing a fresh `Parser` per call
(`Called_without_pooling`), which nothing does by default and no reasonable consumer would
opt into. "Heavy initialization" is not a default-path problem; it is what happens if
pooling is deliberately turned off.

**2026-09-03.** These grammars are read by methods now, and a file rendered that way has
no `Parser` to pool, so the two pooling rows are gone from `CallCost.cs`. The table above
is what the engine cost when the engine was what ran; the three rows that remain — in
place, called, called and valued — ask the same question of the methods.

## What captures cost

`MaterializationCost.cs` asks a narrower question than the URL benchmark above: on the
input that materializes the most values (the 47-character URL with every part), how much
of the time is recognition and how much is capturing and building the typed result? Three
copies of one grammar, same character tests throughout — `WithCaptures` keeps seven named
parts as strings, `SpanCaptures` keeps the same seven as extents with no string built,
`NoCaptures` captures nothing at all. `DefaultJob`, 2026-08-25, before and after the
single-walk materializer (`docs/next.md`):

| | before | after | allocated |
| --- | --: | --: | --: |
| captured as strings, 7 members | 306.1 ns | 219.2 ns | 328 B |
| captured as spans, no strings built | 279.8 ns | 239.3 ns | 88 B |
| nothing captured | 96.1 ns | 90.8 ns | 0 B |

**Capturing costs a multiple of recognizing the same shape**, and the third row is the
control that says so: 90.8 ns to recognize this URL against 219.2 to recognize it and keep
seven parts. Read it as a ratio rather than a subtraction, since the control moved too:
the seven parts cost **2.19× recognition before this work and 1.41× after**, over the three
changes `docs/next.md` records under materialization.

Note what this grammar does *not* show. Making the materializer a method of its own was
worth 7% on `benchmarks/DotGram.Benchmarks/Urls.cs` and nothing measurable here — the recognizer this
grammar compiles to is 3,772 lines of generated C# against the URL one's 21,500, and that
saving is in how large the method was. A benchmark small enough to be readable is
sometimes small enough to miss what it is measuring.

`HotLoop.cs` is the other instrument, and for changes to materialization it is the one to
read: `--hot 5 everypart` runs the real URL grammar on the input that keeps the most, and
the three materialization changes together took it from 13.44M parses in five seconds to
17.97M. Medians of five, each measured against its own immediate predecessor rather than
against a number from earlier in the day — `docs/next.md` has what believing the second
kind cost.

**The span row does not isolate what strings cost, though it was written to.** After the
walk stopped dominating, it came out *slower* than the strings it was meant to be cheaper
than — declaring seven rules `: @SourceSpan` gives each a value, a rule with a value gets
a boundary, and that grammar pays for seven rule frames the string one does not. Read the
two capture rows as two grammars, not as one grammar with and without strings.

## T-SQL against ScriptDom

`--kinds` asks the two parsers what they read; this asks how long they take about it, over
Microsoft's own corpus of eleven hundred `.sql` files. There are two harnesses because
there are two questions.

### The stand's rows, 2026-09-18 (default tiered PGO)

The stand times five statements of different shapes (`tsql/*`, `StandTsql.cs`) against ScriptDom
in the same process, the tree of one statement each, median of five runs: the generated parser
takes **0.28-0.55x** of ScriptDom's time (1.8-3.6x faster), 0.42-0.67x with positions
(`located`, the one to hold against it), and allocates **2.4-13.2 KB a statement against 48.5-138
KB**. Under `DOTNET_TieredPGO=0` ScriptDom is 60-85% slower and the generated parser 8-20%, so
the ratio widens to 0.21-0.32x; every figure here is with the default. The rows, the spreads
and the twin are in [docs/design/stand-2026-09-18b.md](../docs/design/stand-2026-09-18b.md).

The corpus-wide measurements, both taken on 2026-09-18 at 23:25 on a quiet machine, on logical
processors 0-15 at high priority, with the runtime's defaults, on main at `68c60c58` (the silent
reading of a refusal, the lookahead fix, the SQL:2023 split and the rest of the week in):

### The number to quote

`ScriptDomBenchmarks` is BenchmarkDotNet: one case per process, warmed and iterated until
the distribution settles, confidence intervals printed beside it.

```console
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --filter '*ScriptDomBenchmarks*'
```

```text
| Method  | Mean      | Error    | StdDev    | Median    | Ratio | RatioSD | Gen0      | Gen1     | Allocated |
|-------- |----------:|---------:|----------:|----------:|------:|--------:|----------:|---------:|----------:|
| Tokens  |  66.48 ms | 1.272 ms |  2.683 ms |  65.34 ms |  0.50 |    0.10 | 6000.0000 | 500.0000 | 288.97 MB |
| Tree    | 137.81 ms | 8.452 ms | 24.922 ms | 148.47 ms |  1.04 |    0.27 | 6000.0000 |        - | 312.83 MB |
| Located |  31.25 ms | 0.498 ms |  0.573 ms |  31.04 ms |  0.23 |    0.05 |  187.5000 |        - |   9.08 MB |
| Grammar |  26.21 ms | 0.508 ms |  0.679 ms |  26.09 ms |  0.20 |    0.04 |  187.5000 |        - |   9.08 MB |
```

One operation is the whole corpus — 7,716 statements, the ones both parsers read, out of
the 8,397 ScriptDom finds (6,861 the last time this was taken: the grammar reads more of the
corpus now). Per statement that is 17.9 µs for ScriptDom's tree, 8.6 µs for its lexer alone,
4.1 µs here with positions and 3.4 µs without.

**Both build a tree of the whole statement.** That has to be said because it was not true
until recently, and because saying it is cheap: what makes it true is the section below —
every one of these statements, printed back out from the tree and handed to ScriptDom,
comes back as the statement it was read from. Hints, options, output clauses, the words a
catalogue statement was given: all kept.

**So the row to hold against ScriptDom's tree is `Located`**, which carries where each part
of the statement was, as ScriptDom always does. That is **4.4 times** (`Grammar`, without
positions, 5.3), and the difference between the two is about a sixth, for nothing in
allocation, since a span is two numbers written into a record that exists either way.

**And 34 times less garbage** — 42.5 KB a statement against 1.2 KB — which is where ScriptDom's
spread comes from: six thousand Gen0 collections per thousand operations against a hundred and
eighty-seven, and `Tree` is the only row here whose standard deviation is in double figures (18%
of its mean, its median 148 ms above its mean of 138) while `Located` beside it holds to under two
per cent.

### The number for the day

`--speed [path] [version] [rounds]` (`Speed.cs`) measures the same four round-robin: every
method once per round, adjacent in time and in one process, and the rounds repeat. What
the machine does to one measurement it does to the four beside it, so the ratio survives a
machine that is not idle — which a developer's machine is not, and which is why this is the
one to run while working. It is for movement, not for a number to quote: the two runs of 31
rounds taken beside the table above spread 25-40% on the lexer and the located reading and
144-168% on `.Gram`, whose rounds fall into a cheap and an expensive kind.

```console
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --speed 170 31
```

```text
against ScriptDom, TSql170Parser, 31 rounds

  7716 of 8397 statements — the ones both parsers read, which is what may be timed
  770 KB of T-SQL a round

                        per statement     MB/s   ratio   spread    allocated
  --------------------------------------------------------------------------
  ScriptDom, tokens          15684 ns      6.2    1.41   36.9%     39251 B
  ScriptDom, tree            22091 ns      4.4    1.00    9.5%     42477 B
  .Gram, located             15681 ns      6.2    1.41   38.8%      1237 B
  .Gram                       9852 ns      9.9    2.24  144.1%      1235 B
```

### The two disagree, and the reason is worth knowing

1.4 against 4.4 is not rounding. **Running everything in one process, which is what makes
the round-robin fair to machine noise, makes it unfair to the parser that allocates less.**
ScriptDom leaves 40 KB a statement on a heap this grammar shares; those collections happen
whenever the runtime decides, which is to say during the rows that did not cause them. Each
BenchmarkDotNet case has a process of its own and pays for its own garbage.

So: `--speed` for whether today's change made something slower, where both sides are
measured under the same conditions and only the movement matters. `ScriptDomBenchmarks` on
a quiet machine for what the two parsers actually cost.

### What is not measured here

ScriptDom's parsers are twelve, one per version, and this is one grammar; the version chain
is not written yet. Both harnesses read each corpus file with the parser its `Baselines<n>`
directory names, so a statement whose syntax was taken out of the language is timed by a
parser that still has it rather than by one recovering from an error — but the grammar
being timed against all twelve is still one grammar and not a version of anything.

## Whether the tree says what the text said

`--roundtrip [path] [version]` (`RoundTrip.cs`) asks the question a refusal count cannot:
the parser read the statement and answered yes — did it build the right thing?

Nothing in this repository could answer that on its own, because the only thing that knows
what the tree should hold is the tree. So ScriptDom is asked, twice, and its own generator
is used as the normal form on both sides:

1. ScriptDom parses the original and prints it — **A**;
2. this grammar parses the original, `SqlWriter` prints it, ScriptDom parses *that* and
   prints it — **B**.

Keyword casing, line breaks, redundant brackets, `INNER` written or left out, every other
way of writing the same statement — all of it is erased before the comparison, because both
sides come out of the same printer. What survives a difference between A and B is a
difference in meaning.

```console
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --roundtrip 180
```

```text
  6830 statements read by both, printed back and put to ScriptDom again

    1591  the same statement             23.3%
    2792  read, printed, and different    40.9%
    2447  printed into something ScriptDom will not read  35.8%

  kind                                            count    same    share
  SelectStatement                                  1792     707    39.5%
  CreateTableStatement                              584      37     6.3%
  AlterDatabaseSetStatement                         254      74    29.1%
  …
  and 48 kinds that come back whole
```

**The number is completeness, not correctness alone.** Everything the tree reads and drops —
`TOP`, `OVER`, the hints, `OUTPUT`, a named query's `WITH`, the option lists of the DDL —
cannot be printed back, so it lands here as a difference. That is what makes the table
useful: it is the first measurement of how much the tree throws away, ordered by how often
the corpus needs it, and it is the work list for making the tree lossless.

The third row is the sharpest one. A statement that prints into something ScriptDom will not
read is a tree that has lost something *structural* rather than decorative — an `ALTER TABLE`
that kept the word and not the thing it was done to, a `CREATE STATISTICS` with no columns.
Those are nodes to add, not fields.

## What the standard reads

`--standard production file` (`Standard.cs`) puts each line of a file to the BNF of ISO/IEC
9075-2:2023 — `src/DotGram.Sql/Standard/Specification/ISO_IEC_9075-2(E)_Foundation.bnf.txt`,
read as ISO publishes it (`Bnf.cs`) — and prints whether the line is the production named, how
many tokens it is, and the token the reading could not go past. The standard has no engine to
ask, so this is its authority, as `--engine` is T-SQL's: an Earley recognizer
(`StandardOracle.cs`) that cuts the text into the standard's own tokens and recognizes them
against its productions, with the one Syntax Rule it cannot do without — a regular identifier
is no reserved word — written in by hand.

```
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --standard "direct SQL statement" lines.sql
```

A line that begins `--` is skipped. `--standard ? file` says which lexical productions derive
each word of a line, and `--standard ! production` which pieces of the BNF read as empty and
which productions the one named reaches that derive nothing.

Where `SqlStandardParser` publishes a rule of the production's name — `<identifier chain>` as
`TryParseIdentifierChain` — each line is put to it as well, the two verdicts are printed side by
side with `≠` where they differ, and the count of both closes the run. That is how the grammar is
held to the standard: a row goes into `SqlStandardParserTests` once the two agree on it. A line's
milliseconds are mostly the recognizer's; the grammar's own time, and its slowest line, are said
after the count, timed on a second reading of each line so that compiling the parser is not counted.
`--standard =production file` leaves the recognizer out and prints the grammar's verdict and time
alone, line by line — what to use when looking for a line the grammar is slow on.

`--bnf-gram [file]` (`BnfGram.cs`) writes the same BNF as a `.gram` skeleton — every production
under a rule named after it, the lexical ones in a namespace of their own — by default to
`.work/SqlStandard.skeleton.gram`, for the standard's grammar to be written from by hand.

## How an alternative that begins like its siblings is written

`AlternativeShape.cs`. Over kinds a choice is usually decided by the first token, and then
nothing is tried in order at all. What is left is the minority the normalizer factors:
alternatives that begin alike, whose shared head is read — and, if it is captured,
captured — before the choice. Each of them then has to be able to say "not me, try the
next" from halfway through, and there are three ways to write that without a jump: a
method of its own that says it by returning a number, a local function that says the same
and reaches the head by capturing it, or a staircase of nested `if`s written in place,
where saying it is falling off the end of the staircase.

Three alternatives of twelve tokens, two of which run to their last token and fail there,
over a head of two or ten captured positions. `--filter *AlternativeShape*`, 2026-09-03:

| | head of 2 | head of 10 |
| --- | --: | --: |
| a method of its own | 20.09 us | 28.11 us |
| a local function | 20.07 us | 28.07 us |
| written in place, a staircase | 24.95 us | 33.80 us |
| a method the JIT may not compile in | 33.02 us | 44.33 us |

**Writing it in place is the slowest of the three that anyone would write**, by 20–24%,
and it is slower at both head widths. That is the opposite of what "a call costs
something" suggests, and the last row says why: the call costs nothing because the JIT
compiles the part into its caller, and a part it is forbidden to compile in costs 58–64%.
So the question is not whether to extract but whether what is extracted stays small
enough to be put back — which is the same fact `Machine.Sizes.cs` was built around,
arriving from the other end.

**How wide the shared head is turns out not to be the axis.** Ten captured positions cost
more than two, but they cost the same more in all three shapes, so passing them as
arguments is not what it costs. That is the thing this was built to find out, because it
is the one argument against extracting: the parameters. There is no case against them.

**A local function is not a third option.** Roslyn compiles it to an ordinary static
method taking a struct closure by reference — it is the first shape with every captured
local passed by reference rather than the read-only ones by value, and it cannot capture
the input at all, because a `ReadOnlySpan` may not go into a closure (CS9108). It measures
the same because it is the same, and it is kept in the table so that nobody has to ask
again.

`AlternativeLength.cs` below measures the boundary this does not: how long the alternative
may be before the JIT stops compiling it in.

## How long an alternative may be before a method of its own stops being free

The table above leaves one thing open, and it is the thing that could have made extracting
wrong. A method costs nothing while the JIT compiles it into its caller, and 58-64% where
it may not. So: how long may the alternative be before it stops? Above that length the
reader would be paying, and nothing would say so.

The same alternative at five lengths, each as an ordinary method and as one the JIT is
forbidden to compile in. While the two differ, the ordinary one is being compiled in.
`--filter *AlternativeLength*`, 2026-09-03:

| tokens in the alternative | a method of its own | one that may not be compiled in | ratio |
| --: | --: | --: | --: |
| 4 | 6.78 us | 15.81 us | 2.33 |
| 8 | 10.24 us | 19.52 us | 1.91 |
| 16 | 18.35 us | 26.72 us | 1.46 |
| 32 | 37.44 us | 44.11 us | 1.18 |
| 64 | 90.72 us | 90.74 us | 1.00 |

**The line is between 32 and 64 tokens**, and at 64 the two are the same to within a
fiftieth of a percent — the JIT has stopped, and there is nothing left to lose.

**And the penalty for being past it is nothing, because it arrives already spent.** The
call costs a constant — about 1 ns, the same at every length — while the alternative's own
work grows with it. So the 58-64% of the first table is what a *short* alternative would
pay if it were not compiled in, and a short alternative always is. By the time the JIT
gives up, the call it will not remove has become a rounding error.

Which closes the question the first table opened, and closes it by saying there is nothing
to guard. There is no length at which writing the alternative in place becomes the better
choice: below the line the call is free, above it the call is negligible, and the staircase
is 20-24% worse throughout. The emitter needs no size check here, and this is the reason it
has none.

For scale: the longest alternative in the SQL grammar is around a dozen elements, so a real
grammar is not near this at all.

One thing neither table measures: a working set larger than a cache. Everything here is 48
KB of tokens read through a log that wraps, so what is being compared is register
allocation and branch layout. On a megabyte of input, a shape that is three times the code
for the same reading may pay for it in the instruction cache, and that is a different
question — `--big` is where it would be asked, once the reader can write the SQL parser.

## A settlement feed, whole against a part at a time

`WideFeedBenchmarks.cs`. Forty-seven fields a record, converted as they are read — `long`,
`int`, `decimal`, `DateOnly`, `DateTime`, an enum and a good deal of text — so what is
measured is the whole job and not a recognizer handing back substrings for somebody else to
parse. Three doors: the whole file as one string, a `TextReader`, and `File.ReadLines`.
`--filter *WideFeed* --job short`, 2026-09-03:

| records | | mean | allocated |
| --: | --- | --: | --: |
| 100,000 | string | 265.1 ms | 592.76 MB |
| | TextReader | 183.2 ms | 109.19 MB |
| | File.ReadLines | 198.4 ms | 214.83 MB |
| 1,000,000 | string | 3,161.5 ms | 5376.69 MB |
| | TextReader | 1,878.2 ms | 1105.66 MB |
| | File.ReadLines | 1,970.5 ms | 2174.62 MB |

**Streaming is not the slower door.** It is 0.59-0.69× the time and a fifth of the
allocation, and the allocation is the reason for the time: the whole-string parse collects
in the second generation at both sizes (2,000 and 8,000 gen-2 collections) and the streamed
one never leaves the first. Reading a feed a record at a time is what the window is for,
and here it costs nothing to use — it pays.

**These rows could not be produced until 2026-09-03**, which is the thing worth remembering
about them. Both sizes failed in setup: a scanner that matched threw away how far it had
followed the input, so a record the window cut in half looked like a record that did not
match, and the stream closed a repetition that had not ended after a hundred and fourteen
records of a hundred thousand (docs/next.md, "a scanner that matched threw away how far it
had looked"). Nothing caught it until every benchmark in the repository was run at once.

## What a streamed parse holds

`--feed`, over `StreamingBenchmarks.cs`'s grammar. The input is made a line at a time by
`MadeFeed.cs` and never held, so what is measured is the parse rather than a disk — and so
that a claim about twenty gigabytes does not need twenty gigabytes of disk to check.

Two sizes, because the claim is about the difference between them rather than about either
number. Windows, .NET 10, one process each:

| rows | read | seconds | managed peak | working set |
| --: | --: | --: | --: | --: |
| 40,000,002 | 1.41 GiB | 12.9 s | 37.8 MiB | 83.0 MiB |
| 600,000,002 | 21.17 GiB | 198.1 s | 48.3 MiB | 78.2 MiB |

**Fifteen times the input, and the working set went down.** What a streamed parse holds is
the window and the record in hand, so the file's size is not in the figure: 19.8 GiB more
input left the process 4.8 MiB smaller.

The managed peak is sampled once every million records, so the longer run takes fifteen
times as many samples and finds a higher point in the collection cycle — 48.3 against 37.8
MiB is where the samples landed, not a heap that grew. What says it did not grow is that
the working set, which is not sampled, did not.

The row count is what the parse handed back: one part per line, plus the header and the
trailer. Nothing keeps them — the loop counts and drops, which is what a caller writing to
a database or adding up a column does, and what makes the figure the parser's rather than
the caller's.

## Generated source and compiled code size

After building baseline and candidate parser projects for Release net10.0, run:

```console
dotnet run --project benchmarks/DotGram.CodeSize -c Release -- <before-repository> <after-repository>
```

The tool compares SQL and ExpressionLanguage generated source files, byte counts with
repository paths normalized, line counts, DLL sizes, and method IL instruction bytes.
It separates Located and Immediate variants in the IL totals. Both trees must use the
same grammar revision and build settings; existing files under obj/GeneratedFiles must
belong to those builds. It does not build or clean either tree.

Raw source bytes include absolute #line paths, and DLLs may contain embedded debug
information. Use normalized source bytes and IL totals for code-size conclusions.
IL totals include handwritten partial members and nested types; they exclude metadata,
method headers, exception tables and native JIT code. The JSON-lines output can be kept
beside throughput and allocation reports for each optimization.

See [the September 15 comparison](../docs/design/parser-comparison-2026-09-15.md#generated-code-size).
