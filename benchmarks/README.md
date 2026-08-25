# Benchmarks

```console
dotnet run -c Release --project benchmarks/DotGram.Benchmarks
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --filter "*Url*"
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

One of the five inputs does not match. A parser that is quick to say yes and slow to say
no is quick on the input nobody sends: refusal is where a backtracking engine does its
worst work.

### Current result

Windows, .NET 10, `DefaultJob`, 2026-08-24. Indicative, not stable CI thresholds:

| input | .Gram | Regex | Regex, compiled |
| --- | --: | --: | --: |
| `http://example.com` | 280.5 ns | 778.5 ns (2.78×) | 365.5 ns (1.30×) |
| `https://192.168.0.1/` | 252.8 ns | 701.8 ns (2.78×) | 328.2 ns (1.30×) |
| `https://exa mple.com/` — no match | 190.9 ns | 582.4 ns (3.05×) | 138.9 ns (0.73×) |
| a 47-character URL with every part | 441.4 ns | 726.5 ns (1.65×) | 332.8 ns (0.75×) |
| an 84-character path of eight segments | 381.7 ns | 1420.5 ns (3.73×) | 570.6 ns (1.50×) |

Beats `RegexOptions.Compiled` on three of the five — the short URL, the IP-host form, the
long path — and loses on two: the refusal, expected (refusal is where a backtracking
engine does its worst work), and the one input exercising every named part, which is also
the one materializing the most values. Against interpreted `Regex`, faster on every input.

**The timings above predate the deferred-`Expected` change** (`docs/next.md`, "Fixed: the
furthest-failure set was rebuilt on every step back"); the allocation figures below are
after it. Re-measure the timings before comparing the two columns against each other.

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
| url, no match | 440 B | 344 B |
| forty letters, one string | 168 B | 104 B |
| a hundred letters, one string | 288 B | 224 B |
| forty letters, kept as a span | 0 B | 0 B |
| twenty numbers, each a struct value | 2016 B | 784 B |

**A rejected URL was never free.** This file and `docs/status.md` both used to say it
allocated nothing; it allocated 440 B, and the furthest-failure set was what it spent them
on. What is genuinely zero is a recognition whose value is its own extent — the two
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
of the time is recognition and how much is capturing and building the typed result? Two
copies of the same grammar, same rule boundaries, same character tests, differing only in
whether anything is captured — `WithCaptures` publishes seven named parts into a record,
`NoCaptures` publishes the same shape as `@SourceSpan` and captures nothing. `--job short`,
2026-08-24, first numbers this file has carried:

| | mean | allocated |
| --- | --: | --: |
| materialized, 7 captures | 336.7 ns | 456 B |
| recognized only, nothing captured | 115.8 ns | 64 B |

**Capturing and materializing costs about 2.9× what bare recognition of the identical
shape costs** — 221 ns of the 337, against `CallCost.cs`'s ~25% for the arena call
boundary alone. This does not separate "writing a capture entry to the arena as it
matches" from "walking the arena at `Accept:` and building the record" from "capturing
disqualifying a repetition that would otherwise have been possessive and arena-free" —
all three are real candidates and this benchmark does not tell them apart. What it does
say plainly: on a capture-heavy input, this is where the time is going, not the
dispatch overhead `CallCost.cs` measures. That is the more promising place to look next,
and the reason the URL benchmark's own worst case is the input with every part present
rather than the longest one.
