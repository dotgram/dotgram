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

Windows, .NET 10, `--job medium`. Indicative, not stable CI thresholds:

| input | .Gram | allocated |
| --- | --: | --: |
| `http://example.com` | 362 ns | 176 B |
| `https://192.168.0.1/` | 294 ns | 200 B |
| `https://exa mple.com/` — no match | 274 ns | 0 B |
| a 47-character URL with every part | 525 ns | 352 B |
| an 84-character path of eight segments | 599 ns | 328 B |

Against `RegexOptions.Compiled` on the same inputs that is between 1.2 and 2.6 times
slower, and against the interpreted pattern between 1.1 and 1.9 times faster. What is not
close is the allocation: the pattern takes about 1032 bytes for the short URL against
176, and what `.Gram` takes is the result and nothing else.

The earlier numbers this table used to carry — 774 ns for the short URL, 1.84 us for the
long path — were measured before possessive repetitions, predictive choices, the parser
being kept between parses and the value tables. `docs/next.md` keeps what each of those
was worth, and `Membership.cs` and `Scanning.cs` beside this file keep the experiments
that were measured and rejected.

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
