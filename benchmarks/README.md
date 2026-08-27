# Benchmarks

```console
dotnet run -c Release --project benchmarks/DotGram.Benchmarks
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --filter "*Url*"
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --against 9 200000
```

Not run by CI. A number from a shared runner is a number about the runner, and a test
suite that fails when a machine is busy is a test suite people learn to ignore. The
project is in the solution so that it has to keep compiling.

## Why these exist

The architecture makes performance claims — `ReadOnlySpan<char>`, a state machine rather
than nested calls, `goto case`, no closures, no runtime parser graph, construction
deferred until a match is certain — and none of them were measured. A design rationale
that cannot be checked is a design rationale that drifts.

## Parser reuse

The generated parser owns reusable arena storage and exposes `RentParser`/`ReturnParser`
partial hooks. The URL benchmark implements a one-item thread-local cache through those
hooks. This keeps the benchmark focused on recognition and accepted-value construction;
without reuse it mostly measures allocating and growing a new arena on every call.

The cache is deliberately in benchmark consumer code rather than hidden in the timing
method. Reentrant parsing remains safe: renting clears the slot, so a nested parse creates
another parser and only returned instances enter the cache.

## The URL benchmark

`examples/UrlExample.cs`'s grammar against the same language written as a regular
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
worth 7% on `benchmarks/Urls.cs` and nothing measurable here — the recognizer this
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
