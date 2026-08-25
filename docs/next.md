# Unified parser handoff

This is an internal engineering handoff, not public product documentation. It records
the current implementation, the decisions behind it, known gaps, and a safe order for
continuing the work on another machine. Public guarantees belong in `syntax.md` and
`status.md` only after the old and new generators have the same semantics.

## Read this first

Work is on `main`. The current direction is a private nested parser object executing one
shared generated automaton. Recognition records an all-integer derivation arena;
user-visible values and `=>` calls are materialized only after the parse has been
accepted. Recursive rules use explicit frames rather than the C# call stack.

The checkout was clean before this handoff was written. Which commit that was is not
recorded here: it went stale within the day and said nothing `git log` does not say
better.

## Repository conventions and verification

- C# files are UTF-8 with BOM and CRLF.
- Markdown files are UTF-8 without BOM and CRLF.
- Do not let a build or test invocation run for minutes. The complete suite takes about
  14-26 seconds on the machines used so far; stop it at 30 seconds and investigate.
- Build with:

  ```powershell
  dotnet build --no-restore -nodeReuse:false -p:UseSharedCompilation=false -v:minimal
  ```

- Run the already-built test executable directly:

  ```powershell
  .\tests\DotGram.Tests\bin\Debug\net10.0\DotGram.Tests.exe -noColor
  ```

- Before committing, run `git diff --check`, verify line endings and BOM policy, and
  inspect generated snapshot changes rather than accepting them mechanically.

Current baseline: the build succeeds with no warnings or errors. The runner discovers
887 tests and all 887 pass. The stray-character recovery regression is fixed: a broken
expression now produces the lexer error and one parser synchronization diagnostic, while
rules following the broken declaration are still bound and checked.

## Language decisions already made

### C# roles are syntax-directed

Grammar syntax determines the role of a referenced C# symbol. The generator must not
inspect overloads and infer grammar meaning from their signatures:

```dotgram
[@IsLetter]       // one-input-element predicate
@ReadFrame        // arbitrary-consuming external recognizer
when @IsValid(x)  // semantic guard
=> @Create(x)     // construction/factory
Value : @decimal  // type reference
```

The obsolete method-resolution and generated partial-declaration mechanism was removed.
Generated code should call the symbol the grammar names; missing or incompatible C# is
then reported normally by the C# compiler. In particular, a user method with a similar
name must not hide a misspelling in the grammar.

### Framework compatibility

Do not degrade generated hot code merely to support old TFMs. Public documentation should
eventually list the APIs/language features emitted for performance and recommend Polyfill
or an equivalent compatibility package where necessary. Test representative older TFMs
before making compatibility claims.

### Guards

The keyword is `when`, not `where`. Its meaning is a semantic guard on the current
recognition path, analogous to a C# pattern guard.

### Atomicity

Ordinary rule calls are intended to be transparent to backtracking. Explicit `{ ... }`
is a commit-on-success atomic group. For example, an alternative inside an ordinary rule
may be retried after later input fails; wrapping it in `{ ... }` commits once that group
succeeds.

This is the behavior of the generator. The former per-rule machine and its implicit rule
boundaries have been removed; public documentation now describes only transparent calls
and explicit atomic groups.

The URL test for `https://1.2.3.4.5` now succeeds intentionally: after the IPv4 path fails
later, `Host` can retry the `RegName` alternative.

### Deferred construction

`=>` must not execute during speculative recognition. User construction code runs only
for the finally accepted derivation. This rules out side effects from abandoned paths and
is the reason the arena records construction work instead of storing typed values during
matching.

## Implemented architecture

### Generated parser object

The generated host contains a private nested `Parser`. This avoids passing a growing set
of execution buffers through every generated method and gives reusable storage a clear
lifetime. Classic partial hooks let an application supply pooling:

```csharp
static partial void RentParser(ref Parser parser);
static partial void ReturnParser(Parser parser);
```

The default path no longer creates a parser each time: it keeps the last one this thread
used, in a single slot taken out of the field while it is in use, and lets go of a parser
whose arena has grown past what is worth holding. A caller implementing the hooks gets its
own parser back through them and never meets that slot. Internal storage now is
parameterized by grammar value types — one table for each type a rule can produce, so that
a struct value is not boxed on its way into it.

### One shared recognition arena

Recognition uses one array-backed `ParserArena` for the whole parse. `ParserEntry` is a
readonly internal all-integer record. It currently represents choices, calls, atomic boundaries,
repetitions, lookahead, captures, pending constructions, completed invocations, and rule
captures.

Successful value-producing calls are rewritten in place into `Completed` entries. A
completed invocation retains its rule id, matched extent, parent/frame links, and return
continuation. `RuleIndex` is stored separately: an earlier attempt to reuse the
continuation field broke EOF backtracking.

### Shared automaton and recursion

A supported set of parse and find publications is emitted as one automaton with shared label blocks.
Static transitions use direct `goto`; dynamic returns and backtracking use dispatchers.
Frequently called rules therefore remain shared instead of being expanded at every call
site, while thin typed wrappers select each publication's entry label and result id.
Recursive calls are explicit arena frames and do not consume the C# stack.

Verified stress cases include roughly 100,000 levels of direct recognition recursion,
50,000 mutual-recursion steps, 20,000 recursive repetition steps, deep lookahead, and
20,000 levels of typed recursive materialization.

### Post-acceptance materialization

After acceptance, the parser materializes only the accepted derivation:

- one reusable `object?[]` stores constructed values and is cleared by `Reset`;
- one reusable `int[]` of `2 * arena.Count` stores invocation heads in its first half and
  next links in its second half;
- the owner index is built once, keeping materialization linear.

The first implementation repeatedly scanned the arena and was quadratic; a full test run
rose to about 25.5 seconds. The owner index brought it back to about 14 seconds.

Text captures support required and optional values, alternatives, repetitions, empty `*`
as an empty string, and rollback. Repeated text is represented by the source extent.

Rule-value captures point to child `Completed` entries. Required, optional, recursive,
and rollback cases work. An absent nullable value must be emitted as `default(T?)`, not as
`default(T)`, or absence becomes a real zero/default value. A globally optional member
used as a required factory argument needs an explicit cast; null-forgiving `!` does not
unwrap `Nullable<T>`.

Rule sequences do not allocate `List<T>`. Materialization counts matching rule captures,
allocates an exact `T[]`, and fills it from the end because owner links are newest-first.
Empty and rollback cases are covered.

Declared `@T[]` results and `Construction.Sequence` use the machine
non-streaming publications. Their generated factory counts scalar, optional, and sequence
members, allocates one exact `T[]`, and fills it in grammar order; it no longer builds a
typed `List<T>`. Materialization first marks the completed invocations reachable from the
accepted root, so construction code belonging only to an abandoned derivation is never
called.

Ordinary left-recursive folds use the machine. Recognition records each base or
step `Construct` in arena order. After acceptance, materialization builds the base once,
then walks the step markers from left to right and applies each factory to one accumulator.
Captures are read only from the arena segment belonging to that marker, so no per-fold
typed list is needed and a chain of 20,000 terms materializes iteratively. Binding-power
climbing now uses the same path: the current power is carried by explicit call frames,
alternatives test their normalized level before recognition, and `<<`/`>>` calls enter
the operand at the normalized requested power. There is still no C# recursion.

Whole unified publications also compile the root rule's external leading and trailing
`trivia`; EOF is still checked at `Accept`. This is distinct from the trivia already
inserted between sequence operands and fixes published folds beginning with whitespace.

Capture-aware `when` works for text, typed, and sequence captures. Text is
read directly from `Capture` extents. A typed value requested by the guard is materialized
once, cached by its `Completed` entry, and reused after acceptance. A sequence marks all
of its accepted and recovered elements, materializes them in one pass, and hands the guard
one exact array in grammar order. Truncating the arena clears the parallel cache, so an
index reused after backtracking cannot observe a value from the abandoned derivation.
Grammars without typed guards emit neither this cache nor its invalidation operations.

Positive lookahead records the furthest seen position before restoring the input cursor.
Negative lookahead stores its capture slot in the frame and creates an empty successful
capture only on the inner-failure path. A capture around lookahead works. A capture nested
inside lookahead is rejected by language validation (`GRAM4006`), not routed to legacy
generation.

### Diagnostics and tracing

Generated development checks use `Debug.Assert`. Detailed tracing is emitted through a
method marked `[Conditional("DOTGRAM_TRACE")]` and writes to `Debug.WriteLine`, so release
calls and argument evaluation disappear when the symbol is absent.

## Fixed: an atomic group kept the ways back and dropped the derivation

It used to lose what it recognised. A text capture written inside `{ … }` vanished
silently, and a typed one threw, because commit was one line:

```csharp
entries.RemoveRange(atomic, entries.Count - atomic);
```

The arena holds two unlike things. The ways back into the group — `Choice`, `Run`,
`Lookahead` — are what commit exists to close. The derivation — `Capture`, `RuleCapture`,
`Construct`, `Completed` — is what the value is built from after acceptance. Taking the
length off the end took both.

The ways back are now put out where they lie, as `Dead`, which the failure path passes
over. In place rather than removed, because an entry's index is its name: a capture of a
rule's value names the entry its call completed into, and closing the gaps would rename
every record either side.

An earlier attempt at the same thing failed and is worth knowing about: it marked the
resume points and left them, but `Dead` was only passed over by grammars that also used
`recover`, so everywhere else the unwinder fell through to the branch below and treated
them as something else. The mark has to be understood by every parser that can make one.

Where the group recognised nothing worth keeping — no capture, no construction, no call
to a rule with a value — the length still comes off the end, because nothing above the
boundary is named from below it. `Machine.KeepsRecords` decides which of the two is
written, and it matters under a repetition: otherwise a group inside one would leave its
entries behind on every turn, and an arena the grammar bounds would become one the input
does.

## Next: regions, and what each one needs

`ExecutionPlan` holds the decisions that are about a rule and nothing else. The rest depend
on what follows, and what follows depends on the caller — so they are asked during
compilation, from a context threaded down the tree by hand. A region is what carries that
context so they can be asked once and answered in one place.

### What the arena is standing in for

It holds three unlike things at once, and a grammar rarely needs all three:

```text
frames          recursion
ways back       resumability
derivation      construction deferred until the parse is accepted
```

A repetition of characters needs none of them. A rule that calls itself needs the first and
not the second. `X = "ab" | "a"` in `X & 'b'` needs the second and not the first. The
present engine gives every grammar all three because one machine is easier to be right
about than three, and being right came first.

**The arena should be what a grammar gets where resumability is required, not what every
grammar pays for the language having it.**

### What identifies a region

Three answers, and the third is the one to build:

```text
(node, following)        as many regions as there are distinct first sets
(node, call site)        exact, and multiplies along every path
(node, decision class)   regions merge when the context does not change the answer
```

The third makes today's engine the degenerate case — one class per node — so the size grows
only where it buys something. `Weight` is already the budget for saying how far that may go.

### The fourth need

Three needs are about storage. The fourth is about time, and nothing computes it today:

> is this region inside a continuation that has already been committed?

Without it, proving the arena unnecessary does not permit running `=>` immediately.
`A = X => @Build(...)` in `Start = A & "suffix"` is deterministic throughout, and `suffix`
may still fail — so building at the end of `A` would run construction for a derivation that
did not survive, which §3 exists to prevent. Design it with the regions or it will not fit
afterwards.

Computed now, as `DecisionClass.Committed` — `Grammar/Model/Region.cs`'s remarks on
`Regions.Of` say how it threads: forward rather than backward, reset to `true` only past an
atomic group's close, and everywhere else what came in narrowed by whether the node just
passed had one way to go. Not wired into `Compile`; eager construction itself is still
future work, waiting on where regions land in codegen.

### Eager construction: where it would hook in, and why it does not yet

Not at `Node.Construct`'s own `atClose` — at that point `entries[call]` is still `Kind ==
Call`; it only becomes `Completed` later, at the one shared `Return:` label every rule
returns through. Eager materialization has to happen after that rewrite, keyed on
`returned.RuleIndex`.

Eligibility needs no new field: a rule `R` is safe to materialize the moment it returns
exactly when every region on `graph.Bodies[R]` has both `Committed` and `Deterministic` —
`Deterministic` because a rule that can still be retried with a different alternative is not
safe to have materialized once and forgotten, `Committed` because nothing upstream may still
discard this call for a sibling one. Both are already there to read.

A mid-recognition materializer already exists: typed `when` guards mark `guardValues[at] =
parser` for whatever they need and call the generated `Materialize_DotGram(text, parser,
entries)`, the same routine `Accept:` uses, guarded by a parallel `built[]` array so a value
is never built twice. Eager construction at `Return:` would look like nothing more than `if
(!built[call]) values[call] = parser;` followed by the same call.

That reuse is the wrong move as it stands. `Materialize_DotGram` walks the *entire* arena —
three passes over `entries.Count` — every time it runs. Once per accepted parse, or a few
times for a handful of textual guards, is cheap. At *every* return of an eager-eligible rule
it is not: a repeated-record grammar (`FeedExample`, `LoggingFeedExample`,
`WideFeedBenchmarks`, the CSV-shaped examples generally — most of what this engine's own
examples are) would re-scan a growing arena on every record, turning an O(n) parse into
O(n²). That is exactly the cost the rest of the regions work exists to avoid, so shipping it
this way would be a regression wearing a feature's name.

What it actually needs is a materializer that walks only what changed since it last ran,
bounded by work done since the last eager trigger rather than by total arena size —
something closer to incremental arena bookkeeping than to `Materialize_DotGram`'s sweep. That
is a separate, sizeable design and does not belong to this pass.

### Incremental materializer: the supporting mechanism, built

The piece the previous section said was missing — a materializer bounded by what changed
rather than by arena size — is now in `Parser` and `Machine.Materialization.cs`. Not eager
construction itself yet; the shared infrastructure any eager trigger and every existing
caller (`Accept:`'s one-shot sweep, a guard's speculative one) now run through alike.

The linking pass (`Machine.Materialization.cs`'s `MaterializeRange`, formerly `Materialize`)
used to rebuild the whole `links[]` array from index `0` on every call — the actual O(n²)
source, since a full relink on every eager trigger would have defeated the point as surely as
`Materialize_DotGram`'s full walk did. It is incremental now: `Parser.LinkedUpTo` remembers
how far the linking loop has already gone, and the loop starts there instead of at `0`. The
owner-marking and build sweeps take a `fromExpr` bound the same way — `"0"` for every caller
today, a rule's own call index for the eager trigger once one exists.

Three bugs came out of building this, each found by a full test-suite hang rather than by
reasoning about the design beforehand — the kind of thing "checked by hand against every
snapshot" (see step 2 above) does not catch, because none of them change what a single parse
computes:

- **The link table is per-node, not per-parse.** `Parser` is pooled (`Recycled`/`Recycle`,
  keyed off `[ThreadStatic]`), so the same arrays outlive many parses. The original code's
  full re-zero on every call hid this; growing-not-rebuilding does not. `Reset()` now clears
  `_linkHeads`/`_linkNexts` back to `-1` over `[0, _valuesUsed)` — the same range `_values`
  and `_built` were already cleared over — so a rule call that captures nothing this parse
  does not fall through to a stale chain a previous parse left in the same pooled slot.
- **Growing the value table and growing the link tables are one operation, not two.** A typed
  `when` guard calls `parser.Materialization(entries.Count)` directly to size the value table
  before deciding whether it needs `Materialize_DotGram` at all — and skips the call entirely
  when everything it needs is already `built[]`. That left `_valuesUsed` ahead of
  `_linkHeads.Length`/`_linkNexts.Length` whenever a second guard in the same parse found the
  first guard's values already there, and `Reset()` read `_valuesUsed` off the end of the
  shorter arrays. Fixed by moving the link tables' growth into `Materialization(count)`
  itself — `MaterializationHeads`/`MaterializationNexts` are now plain accessors, sized
  wherever the value table is, so the two cannot fall out of step by construction.
- **A discarded derivation's own slots being cleared is not enough — the parent's head
  pointer into them has to be too.** `Truncate` already clears `_values`/`_built` over the
  range being discarded; extending that to `_linkHeads`/`_linkNexts` over the same range
  fixed the previous bug but not this one. The list a call's captures thread through is
  built by prepending — `linkNexts[new] = linkHeads[call]; linkHeads[call] = new` — and
  discarding the *most recently* prepended entry leaves `linkHeads[call]` still pointing at
  it unless something undoes that prepend. The surviving call is not itself in the truncated
  range, so nothing in it was going to be cleared. `Truncate` now takes `entries` and walks
  the discarded range descending, popping each index off the head of its own call's list —
  `entries[i].CallIndex`, checked, reverted, then cleared — the same order those entries were
  pushed in, undone. Missing this produced a *reachable* answer, not a crash: a stale head
  splices one call's list onto a slot a later, unrelated derivation has since reused, and
  `GeneratorDriverTests.A_cached_guard_value_is_discarded_with_its_derivation` — written for
  exactly this shape, a guard whose first alternative's cached value must not survive into
  the second — hung rather than failed once it existed to find it.

`Parser.Truncate` gained a second parameter (`ParserArena entries`) for the third fix; every
call site in `Machine.cs` was updated alongside it. `MaterializationHeads`/`Nexts` dropped
their `count` parameter for the second fix. Both are internal-only signature changes with no
effect on a generated parser's public surface.

Verification for this step was the existing suite, not new tests written for it — `ExampleTests`
(pooled reuse across ~90 parses per run) and `GeneratorDriverTests`'
`A_cached_guard_value_is_discarded_with_its_derivation` (guard-cache-discard-on-backtrack,
almost exactly the third bug's shape) already covered the shapes that broke, once they were
run for long enough and without something else masking the hang.

### Eager construction: built, wired in, and caught its own bug

The wiring the previous section left open is in now. `Machine.Regions.cs`'s
`ComputeEagerRules` walks `_regions` once and keeps a rule when *every* region on its own
body — every call site, since one compiled body serves all of them — is both `Committed`
and `Deterministic`; one region reached either false disqualifies the rule everywhere,
because the automaton cannot tell at `Return:` which call site a given return came from.
A rule with nothing declared to build (`ValueRule(rule) < 0`) is filtered out too — nothing
for an eager trigger to do early. The whole set is empty whenever `graph.Recoveries.Count >
0`, the same conservative, grammar-wide guard `CanLower` already uses, because the recovery
pass inside materialization has not been checked against a bounded range.

`EnsureEagerMaterializer` builds `Materialize_DotGram_Eager(text, parser, entries, int
from)` — `MaterializeRange` bounded from `from` instead of `"0"`, cached the same way the
guard materializer is. The shared `Return:` label calls it, switching on
`returned.RuleIndex` over the eager set, right after the `entries[call] = Completed`
rewrite and never before it — `MaterializeRule` reads a rule's value off its `Completed`
entry, so materializing a moment earlier would read a call that has not become one yet.
Every place that decided whether to emit `Truncate`, the cached `Accept:` path, or the
value-cache fields at all on `_guardValues` alone now decides on `Caches` — `_guardValues ||
_eagerRules.Count > 0` — since an eager rule needs exactly the same infrastructure a typed
guard does, for the same reason: a value that might be read again before the arena forgets
it needs `built[]` to say whether it already was.

Verification followed the plan's own list. `GeneratorDriverTests` gained three tests:
`An_eager_eligible_rule_is_constructed_even_when_the_parse_later_fails` proves eager fired
at all — the factory runs and the parse still fails afterward with nothing else in the
grammar that could have run it, since `Accept:` is never reached.
`Eager_construction_survives_a_repetition_giving_back_its_last_turn` checks a repeat whose
last turn fails and is given back, with an explicit source-text assertion confirming the
grammar it uses actually qualifies (a comment claiming eager fired is worth exactly what the
sub-string search behind it proves, and finding the right shape by hand — a repeat followed
by anything else in the same sequence is enough on its own to disqualify the rule —
took several wrong grammars before the right one, which is why the test checks rather than
asserts by comment). `Recovery_anywhere_turns_eager_construction_off_for_the_whole_grammar`
checks generated source for `Materialize_DotGram_Eager`'s presence and absence across a
minimal pair.

The benchmark the plan asked for (`benchmarks/DotGram.Benchmarks/EagerConstruction.cs`,
`Feed : @int[] = items: Item* => @(items)`, nothing following the repeat so `Feed` itself
qualifies) is what actually found a fourth bug, one the correctness tests above had no way
to catch because it does not change what a parse computes — only how much it costs.
`Materialization(count)`, and the same shape in `_built`'s resize, each typed value table's
resize, and `_linkHeads`/`_linkNexts`' `Grow`, all resized to *exactly* `count` rather than
growing with headroom. Harmless when called once or a handful of times per parse, which is
all any caller before eager construction ever did. An eager rule inside a repeat calls it
once per turn, and a resize to exactly `count` on every call is an O(current-size) copy —
O(n²) over a repeated-record grammar, the identical cost class the incremental linking pass
was built to remove, reintroduced one array over. First run, 10,000 records: 710 ms and 8.1
GB allocated; 100,000 records: 98.3 *seconds* and 810 GB. Fixed by growing each of the four
to `Math.Max(count, length * 2)` instead — the doubling `ParserArena.Add` already used, just
missing from these. Same grammar afterward: 10,000 records in 2.6 ms allocating 11 MB;
100,000 in 22 ms allocating 125 MB — an eightfold-to-elevenfold cost for a tenfold input,
not the roughly hundredfold time and hundredfold-of-already-huge allocation the exact-sized
version paid. `dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --filter
"*EagerConstruction*" --job short` reproduces both.

### What must not happen

Regions must **reference** nodes, not clone them. `_captureSlots`, `_owners` and
`_constructs` are keyed by node identity, and `Orphans()` and `GraphIntegrityTests` exist
because that has bitten before. A plan keyed by region and pointing at shared nodes has no
such problem; cloning nodes per context reintroduces all of it.

### Order

1. ~~The region type and the walk from publications, with one decision class — output
   byte-for-byte unchanged, which is what makes this step safe to take blind.~~ Done:
   `Region`/`DecisionClass` and the walk are `Grammar/Model/Region.cs`.
2. ~~Real decision classes. The snapshots then show exactly where context opened
   something.~~ Done: the walk takes a `classify` function now instead of a fixed answer,
   so it stayed in Model while `Machine.Regions.cs` supplies the real one — `Silent` and
   `Possessive`, called directly rather than duplicated. What that found, checked by hand
   against every snapshot, example and benchmark grammar: nothing loops, and several split
   a rule into more than one class where today's engine could not tell — Url 16 ways,
   Json 2, Xml 3, Sql 1. Not read by `Compile` yet, so nothing generated changed.
3. ~~The fourth need~~, and eager construction where it is proved. `Committed` is done —
   see "The fourth need" above for how it threads. Checked by hand the same way: nothing
   loops, and which construct-node regions come out committed is never all-or-nothing
   (Json 9 of 19, Filter 1 of 19). ~~Eager construction itself~~ — actually running `=>`
   early where this says it is safe — is done: the incremental materializer it needed to
   avoid `Materialize_DotGram`'s O(n²) is built, and eager construction is wired to it; see
   "Incremental materializer: the supporting mechanism, built" and "Eager construction:
   built, wired in, and caught its own bug" above.
4. Lowering: a region needing none of the three becomes an ordinary method, and splitting
   the automaton across methods falls out of that rather than being done for its own sake.
   ~~Whole-grammar case done~~: when *every* publication qualifies, the grammar compiles
   without the shared engine at all — see "Whole-grammar lowering" below. Splitting a mixed
   grammar, where one rule lowers and a sibling does not, is not built, and the specific
   shape investigated for it — a silent rule that still keeps a useful type — turned out to
   have no target at all; see "Mixed lowering: investigated, has no target" below before
   attempting it again.

### Whole-grammar lowering

`Machine.Silent(node, following)` already *is* the eligibility test for lowering — its own
recursive definition requires every reachable call to be inlinable, which already excludes a
rule that can reach itself, and defaults every node kind it has no case for — a capture, a
construction, a guard, an external recognizer, a lookahead, an atomic group — to not silent.
Asking it once at a publication's root (`Machine.CanLower`) asks it of everything reachable,
with no separate structural scan needed.

Scoped to whole grammars: lowering fires only when *every* publication qualifies (also
requiring no `recover`, no binding powers, no streaming — each drives machinery of its own).
One disqualifying rule anywhere and the output is exactly today's, unchanged. This sidesteps
the harder problem entirely — a lowered rule calling into, or being called from, the shared
automaton — by ensuring the two never coexist in one file.

The recognizer itself reuses `Compile` and `PlanLayout` completely unchanged: it is a
different rendering of the same states (`Machine.RenderFlat` in `Machine.Flat.cs`), not a
second compiler. `PlanLayout`'s reachability and signpost-collapsing already work from any
`_roots`, by regex over already-generated text, with no idea what becomes of the states it
orders — reusing it for a standalone entry needed no changes at all.

`Start = "h"` (`parse Start`) now compiles to:

```csharp
static int Recognize_Start_Whole_Flat(ReadOnlySpan<char> text, int pos, ref Failure failure)
{
	var p = pos;
	if (p + 1 > text.Length) goto Fail;
	if (text[p + 0] != 'h') goto Fail;
	p += 1;
	if (p != text.Length) goto Fail;
	return p;

	Fail:
	failure.Position = p;
	return -1;
}
```

No `Parser`, no `ParserArena`, no dispatch, no pooling — none of it is emitted at all, not
just unused. Measured on a repeated-record grammar structurally identical to
`Possession.Settled` (`benchmarks/Flat.cs`, one added capture the only difference): 119 ns
and zero allocation lowered, against 691 ns and 952 B through the shared engine — the arena
and dispatch overhead this section exists to name.

### Mixed lowering: investigated, has no target

The obvious next step — let one silent rule inside a larger, non-silent grammar compile
flat and be called with a plain method call from the shared automaton — was designed and
partly built (`Machine.CanLowerRule`, `Machine.CompileFlatCall`) before turning out to have
no reachable case, and was reverted rather than left as dead code.

The reasoning: a silent rule that declares no type is already inlined by
`ExecutionPlan.CompiledInPlace`, so the only case worth a separate method is one
`CanInline` refuses only because it declares `: @T` — and `Silent` already excludes every
`Capture`/`Construct`, so such a rule can only be §4.1 case 4 (builds nothing, captures
nothing). Checked against the actual normalizer (`GrammarNormalizer.Results.cs`,
`ExtentValues`) rather than assumed: `: @string` on such a rule has its declared type
erased during normalization — "the type is recorded as absent so the machine goes on doing
what it did" — so it is already `CanInline`-eligible, same as writing no type at all.
`: @SourceSpan` keeps its type, but normalization gives it a `Construct` (a factory taking
`parserSpan`) to name the bounds explicitly — and a `Construct` anywhere is exactly what
`Silent` excludes. Every other case (§4.1 cases 2 and 3) already requires a capture or an
injected `Construct` to build its value. There is no grammar `: @T` and silent at once can
describe.

If this is ever revisited, it needs a different premise than "a silent rule can still be
usefully typed" — that one is closed. `Machine.RenderFlat`/`Machine.PlanLayout` reuse and
the `_fail` save/restore trap found along the way (a nested `Compile` call, from inside
another `Compile` call, must not inherit a redirected `_fail`) both still apply to whatever
premise replaces it.

### `Node.Guard` does not have the same gap `Node.External` did

Checked, not assumed, after `External` widened cleanly: `Guard`'s compiled shape
(`Machine.cs:1034-1243`) unconditionally emits `var ruleStart = entries[call].Position;`
before it even asks whether the guard's own text names `parserText` — `entries`/`call`
referenced regardless. `External` was safe to add because its compiled shape names neither;
`Silent` only asks "does this write to the arena" and happened to coincide with "does this
reference the arena at all" for `External`, not because the two questions are the same one.
A flat method's signature carries no `entries`/`call` at all, so marking `Guard` silent
without first making that line conditional would compile-error the moment a silent-marked
guard landed inside `RenderFlat`.

The narrow case that would open — a guard naming neither `parserText` nor any capture —
is real but vanishingly rare (a `when` with no relation to what was matched) and would need
that line made conditional first, for a case unlikely to occur in a written grammar. Not
worth it on its own; if `Guard` is reconsidered, start from making `ruleStart` conditional,
not from `Silent`'s switch.

## What the machine supports now

`Machine` handles every normalized node form:

- empty nodes, literals, and element predicates;
- sequences and choices;
- transparent rule calls and explicit atomic groups;
- repetitions and lookahead;
- external recognizers;
- capture-free and capture-aware guards;
- text/rule captures and supported construction;
- recovery, including continuation-first boundaries, synchronization, deferred recovery
  factories, the optional reporting hook, and recognition-only continuation probes for
  streamed repetitions;
- binding-power climbing with power-aware arena call frames;
- direct and mutual recursion through explicit frames.

String and reader `find`, whole parse, and every stage of a streamed parse now call the
same engine. Reader drivers own only window extension, iteration, yielding, and
recovery scanning. Recognition-only entries test the complete continuation before each
streamed element without invoking `=>`, a recovery factory, or `OnRecovered`. Feed and
URL no longer carry a second legacy recognizer graph merely because they publish `find`.

## Typed guard materialization

Text captures remain allocation-free extents until handed to C#. A typed captured rule
result normally does not exist until deferred materialization, but a `when` that names it
sets that completed invocation as a materialization root. All roots needed by one guard
are materialized together. A `bool[]` parallel to the existing object cache distinguishes
an unbuilt value from a factory that legitimately returned null. Both arrays are cleared
when backtracking truncates the arena. Final materialization skips cached entries, so user
construction is invoked once for a surviving derivation. Work on a derivation later
rejected by the guard or suffix is the consequence of the author's decision to inspect
that computed value.

## Future optimization gate

Inlining is no longer bounded to single-literal rules: it takes any rule outside every
call cycle that produces no value, because such a rule's call buys nothing and its
expansion terminates. That was measured, and what it bought was not the call — it was
letting the analyses see the body in place, where a repetition can be shown possessive
and a choice decidable.

What replaced this gate is a harder one. A change to the method the automaton is compiled
into moves the time by several per cent **whether or not the changed code runs**: a
character-class window that was never reached on the input measured cost the URL grammar
7%. Everything below that is unmeasurable there — the window is 1.6× faster in isolation,
and reordering a chain of ranges is worth 3×, and neither shows in a parser.

So: do not tune the shape of anything the emitter writes without measuring it **in a
parser**, and expect the answer to be noise. The lever that is left is the size of the
method, and splitting the automaton across methods is the change that would move it.
`benchmarks/Scanning.cs` and `benchmarks/Membership.cs` hold the numbers and the reasoning
behind that conclusion, including the explanations that were tried and measured wrong.

## First single-machine performance pass

The URL benchmark initially exposed parser-storage allocation rather than automaton
throughput: without a consumer cache it allocated 2.3--21.6 KB per parse. Its existing
`RentParser`/`ReturnParser` hooks now back a one-item thread-local cache in the benchmark;
accepted outputs still allocate normally, while a rejected URL allocates nothing.

The inlining rule at the time was structural and narrow: an untyped, capture-free rule
whose entire normalized body was one literal or element set was compiled directly at its
call sites, and every larger rule remained a shared block. It has since been widened to
any rule outside every call cycle that produces no value — see the gate above, and the
reason, which was not the call it saves. On the short run this reduced the 84-character URL from 3.27 us to 1.86 us
and the full 47-character URL from 1.51 us to 1.17 us. Generated source also shrank from
56,749 to 56,325 bytes for URL and from 127,292 to 124,288 bytes for Settlements. Results
on the shortest cases were mixed, so broader inlining is not currently warranted.

`ParserArena` replaces `List<ParserEntry>` with the five operations the generated engine
actually needs. Because entries contain only integers, removal and reset only move the
live suffix and adjust `Count`; clearing dead slots would release no references. With
parser reuse, this reduced the short URL from 837 ns to 774 ns and the full URL from
1.17 us to 1.05 us. It adds about 1 KB of support source per generated class. Combined
with atom inlining, URL is 57,370 bytes versus the pre-optimization 56,749, while the
larger Settlements parser remains smaller at 125,333 versus 127,292 bytes.

Recognition-only measurements put successful URL materialization at roughly 110--470 ns
and all 176--352 bytes allocated by those successful parses. The allocations are the
accepted strings, exact capture arrays and result objects; a rejected URL still allocates
nothing. `Materialized()` no longer repeats the array preparation already performed by
`Materialization(count)`.

An experiment that initialized only apparent owner heads instead of resetting the full
first half of the reusable links array was reverted. Materialization may run from a
typed `when` while recognition still has incomplete calls, so the owner graph has more
live intermediate states than final-result materialization alone exposes. Removing that
linear reset safely would require maintaining rollback-aware links as arena entries are
added and truncated. That is an architectural change, not a local hot-path cleanup, and
the measured remaining cost does not justify it now.

## Implementation map

- `src/DotGram/Grammar/Emit/Machine.cs`: arena model,
  automaton emission, backtracking, and materialization.
- `src/DotGram/Grammar/Emit/Support.cs`: generated support types including the nested
  parser infrastructure.
- `src/DotGram/Grammar/Emit/CSharpEmitter.cs`: publication, staging and streaming routing.
- `src/DotGram/Grammar/Model/CaptureLayout.cs`: capture layout; recovery/fold structures remain
  here for historical reasons.
- `src/DotGram/Grammar/Model/RecognitionGraph.cs` and
  `src/DotGram/Grammar/Model/Retention.cs`: graph and retention/streaming analyses that
  constrain eligibility.
- `tests/DotGram.Tests/CSharpEmitterTests.cs`: generated-code shape and snapshot coverage.
- `tests/DotGram.Tests/SemanticTests.cs`: language semantics, including recovery without
  diagnostic cascades.
- `tests/DotGram.Tests/UrlTests.cs`: transparent rule backtracking regression coverage.

## Commit sequence for the current work

The implementation can be reviewed in this order:

- `514d6cd` Add explicit atomic groups
- `fa04051` Count rule call sites for automaton sharing
- `355336b` Generate shared automaton for simple grammars
- `57338f3` Run repetitions on the shared parser arena
- `fc46cec` Run lookahead on the shared parser arena
- `e17be62` Run external recognizers in the shared automaton
- `18c4745` Run capture-free guards in the shared automaton
- `3f410bc` Record text captures in the shared arena
- `80fb32c` Defer root constructions until acceptance
- `f52ae74` Record completed value invocations
- `0ea1133` Materialize captured rule values after acceptance
- `75caaa5` Materialize captured rule sequences into arrays
- `0f83983` Materialize optional rule values in linear time
- `77b4561` Capture positive lookahead extents in the arena
- `d074b2d` Capture negative lookahead success in the arena

Earlier recursion groundwork is in `9f95cfa`, `6076080`, `61ff9c8`, `a4deb7b`, and
`4776d81`.

## Completion criterion

The semantic restructuring and first performance pass are complete: every publication
kind uses transparent-rule semantics and explicit atomic groups, recursive
parsing/materialization is iterative, and one generator implements the language.

Since then a second pass has been made and is also complete. A rule outside every call
cycle is compiled into its callers; a choice its first character decides writes no resume
point, and one whose later alternatives that character rules out writes none either; a
repetition whose body matches one way and is followed by something it cannot begin with is
run to its end and never asked to give any of it back, and where its body writes nothing to
the arena it is a plain loop; text alternatives none of which begins another are decided
where they differ. The parser is kept between parses, values are held in a table for each
type rather than one of `object?`, and an extent is read from the entry the rule left
rather than stored. What a parse allocates is the result and nothing else.

Two defects were closed on the way, both of them the same mistake in three places: an
entry's index is its name, so what commits or unwinds cannot renumber the entries around
it. An atomic group discarded the derivation with the resume points; a repetition written
as a loop kept the position of a turn that broke halfway.

Further performance work is optional and now has a stated obstacle rather than a target —
see the gate above. The full suite remains below the 30-second ceiling.

## Built: a warning for accidental shadowing inside a nested namespace

`docs/syntax.md` §5.1 names the footgun: `namespace (A = B) { ... }` and
`namespace { A = B }` are one pair of parentheses apart and mean different things — a
substitution reaching the whole call graph, against an ordinary declaration that
shadows only what is lexically inside the block. (Named `context` at the time this
shipped; renamed to `namespace` afterward — see the entry below.) A missing header
entry used to compile either way with nothing to say so.

`GrammarBinder.ShadowsEnclosingRule` (`GRAM3012`) now does, at exactly the narrow scope
settled on after going back and forth over it:

- **A rule declared inside a nested `namespace { ... }` block, whose name also resolves
  in an enclosing *grammar* scope**, gets an `Info`-level diagnostic (docs/status.md's
  own convention for "the grammar is correct and there is nothing to fix" pointers, e.g.
  `GRAM5001`) — not a refusal. Fires in `GrammarBinder.Declare`, right after a successful
  `TryDeclare`, by looking the name up starting from the declaring namespace's *parent* —
  found and not in `StandardLibrary` means an enclosing grammar rule was shadowed. Applies
  whether or not that namespace already carries a header for something else — the risk is
  "was a header entry meant here," not "does this specific block already use one."
- **Shadowing the standard library** (`trivia`, `wordboundary`, `any`, `none`, `eol`,
  `eof`), at any nesting depth and any number of times over, stays completely silent —
  excluded by name, not by whether the symbol found is literally the original built-in
  (an already-shadowed `trivia` re-shadowed again is still `trivia`). The language's
  normal, intentionally frictionless mechanism (§3.1.1: "no directive, no mode, nothing
  declared specially to make it possible"), used throughout the examples, and not what
  this warning is for.
- **Top-level shadowing** (declaring `trivia = none` etc. at the top of a file, not inside
  any `namespace {}`) stays silent too — there is no `namespace (...)` header syntax
  anywhere nearby to have meant instead, so there is nothing to be ambiguous about.
  (Provably redundant with the standard-library exclusion above, since the top level's
  only possible parent is the standard-library namespace itself — kept as its own
  explicit condition anyway, for a reader rather than for correctness.)

**Known first-cut gap, accepted rather than chased**: a name shadowed only by way of an
import (`using Lib;` bringing in a name that collides with an enclosing scope's) is not
caught. `Declare` (pass one, where the check runs) executes before `ResolveImports`, so
the import is not wired up yet at the point this asks — reaching it would mean moving the
check to pass two. Under-reports; never mis-attributes.

## Built: `Expression with (A = B, ...)`

The idea raised alongside the warning above, now designed and shipped: an expression-
extent counterpart to `namespace (A = B) { ... }` — the same substitution, applied to
one operand instead of a whole block, so a single override does not need a block
wrapped around it. `docs/syntax.md` §5.1 has the notation and the "which of the two to
reach for" guidance; §3.8 and §10 have the precedence and the grammar.

Postfix, and settled as such rather than reconsidered: `Number with (Point = Comma)`,
binding at the same tightness as a quantifier or `recover` — outermost of the three,
checked last in `GramParser.ParseQuantified` (split into `ParseQuantifiedCore` plus a
new `ParseWith`). A capture written before the wrapped operand ends up *inside* the
`with`, not beside it — `c: Number with (...)` parses as `(c: Number) with (...)`, since
`with` wraps whatever `ParseQuantifiedCore` already built. That single fact is what
shaped the implementation: an earlier sketch that lowered the operand into a synthesized
rule (mirroring the external-recognizer-value feature's `ExternalRuleFor`) would have
isolated that capture inside a private, unreachable rule and silently dropped it from
the enclosing rule's own result. Caught by a Plan agent explicitly asked to verify
rather than accept the sketch, before any of it was written.

What shipped instead: the operand lowers exactly as if `with` were not there — no
wrapper node — and the pending site is recorded by the *node identity* of its own
lowered root (`GrammarNormalizer.Lowering.cs`'s new `LowerWith`, tracking which rule is
currently being lowered via a new `_currentRule` field). A new pass,
`GrammarNormalizer.With.cs`'s `SpecializeWithSites`, runs after `LowerAll()` and before
`SpecializeNamespaces()` — `with` mutates a rule's body in place, and an enclosing
`namespace (...)` clone of that rule has to see the mutation already applied. It
computes each site's affected set exactly as a `namespace (...)` block does (`Seed`
replaced by a new `DirectCalls`, since a `with` names what it calls directly rather
than what a block declares; `ReachableFromSeed`/`AffectedSet`/`CloneAndRewrite` reused
unmodified), then splices the rewritten root back into the enclosing rule's body with a
new identity-keyed rebuild pass, `SpliceWithSites` — needed because nodes are immutable
records, so replacing one descendant means reconstructing every ancestor up to the
rule's own root.

One case needed more than a straight port of the `namespace (...)` machinery: `Group`
is transparent at lowering, so `(X with (A=B)) with (C=D)` has both `with`s' operand
lower to the *exact same node*. Cloning each site independently and applying both
rewrites in sequence does not compose — the second pass, built against the pre-splice
call graph, cannot see inside the clone the first pass already made, since that clone
is a new rule referenced only by symbol. Fixed by detecting the shared root and merging
the two sites' rebindings into one combined set (later overriding earlier for the same
key — the same child-overrides-parent layering nested `namespace (...)` headers already
use) before cloning once. `SpecializeSite`'s clone-building tail was extracted into a
reusable `CloneAffected`, and `NameFor` generalized to take a bare site name instead of
a `GrammarNamespace`, so both features share the one implementation.

`GrammarBinder.cs`: `ResolveNamespaceRebindings`'s per-entry validation extracted into
`ValidateRebinding`, reusing the header form's own diagnostic IDs
(`UnknownRebindingTarget`/`UnknownRebindingReplacement`/`ParameterizedRebinding`/
`DuplicateRebinding`) rather than minting new ones — same failure, different syntactic
position. `NamespaceBoundNameRedeclared` does not port: `with` declares nothing, so
there is nothing to check a redeclaration against.

## Built: `with (...)` on a publication directly

Asked in conversation: why can't `parse Sum with (trivia = none) as Evaluate` be
written directly, instead of wrapping the directive in a single-purpose
`namespace (trivia = none) { parse Sum as Evaluate }`? Answered and shipped —
`Publication` gained `Rebindings` (`GrammarModel.cs`), `Decl.Publish` gained the same
(`Tree.cs`), and `GramParser.ParsePublication` parses an optional `with (...)` between
the rule name and `as`, reusing `ParseRebindings()` verbatim.

Simpler than either other extent: a publication has no node tree to splice into and no
block to specialize — it names one rule directly. `GrammarNormalizer.With.cs`'s new
`SpecializePublicationWith` seeds straight from `Publication.Rule` (no `DirectCalls`
needed, since there is no operand to walk), computes the affected set exactly like the
other two, and either remaps `Publication.Rule` to the clone or leaves the publication
untouched — no splice, no rewrite pass of its own. Runs *after* `SpecializeNamespaces`,
not before like `SpecializeWithSites` does: a publication's own `with` is the more
locally written of the two extents, so it composes on top of whatever an enclosing
`namespace (...)` already did to the rule it publishes, rather than the reverse.

Caught along the way, not by reasoning but by a crash on the very first grammar that
exercised it: `CloneAndRewrite`'s `Node.Call` case rewrote a call's target via
`RewriteTarget` and built the new node directly (`new Node.Call(...)`), bypassing
`CallTo`'s on-demand built-in registration (§3.1 — "a grammar that never says `eol`
carries no `eol`"). A rebinding's replacement can easily be a built-in nothing else in
the grammar calls yet (`with (trivia = none)` when `none` is otherwise unused) — the
rewritten call reached `none` correctly, but nothing had ever registered it into
`_rules`/`_bodies`, and `Machine.ValueRule` threw `KeyNotFoundException` reading its
results at emission. **Latent in `namespace (...)` too** — the existing test that
exercises `namespace (trivia = none)` happened to also declare `trivia = none` as an
ordinary shadowing rule elsewhere in the same grammar, which registered `none` as a
side effect and masked the gap. Fixed at the one shared spot, `CloneAndRewrite` calling
`CallTo(RewriteTarget(...), ...)` instead of constructing the node directly — every
caller (namespace blocks, expression `with`, publication `with`) gets the fix for free.

## Built: renamed `context` to `namespace`, everywhere

`context Name { ... }` grouped and hid rules, imported via `using Name;`, and compiled
to a nested `static class` — a namespace in everything but name, and the word had
already been retired from the *rebinding* mechanism (now `with`, at all three extents)
earlier this session. Raised in conversation, including a real collision found and
resolved before it was approved to proceed: `docs/syntax.md` §2, then titled *"Two
namespaces: `@` and its absence"*, already used "namespace" for the distinction between
grammar-rule names and C# names — a third sense of the same word right where a reader
first meets the term. Resolved by retitling §2 to *"Two vocabularies: `@` and its
absence"* and rewording its own internal uses, freeing the word for the renamed
construct. (`@using System.Text; // import a C# namespace` was untouched — that one
already named a real C# namespace, and `GramCompilerOptions.Namespace` /
`CSharpEmitter.Emit`'s `@namespace` parameter — the generated code's own target C#
namespace — stayed distinguishable by the existing `Grammar`-prefixed convention:
`GrammarNamespace`, never bare `Namespace`.)

Full scope, not just the keyword: `Decl.Context` → `Decl.Namespace`, the `GrammarContext`
class → `GrammarNamespace`, `RuleSymbol.Context` → `RuleSymbol.Namespace`,
`SpecializeContexts` → `SpecializeNamespaces`, diagnostic constant names and message
text, every test, every example, every doc mention. A second, separate rename went the
other way: `ContextRebinding`/`OwnBindings`/`ContextBindings` and five of the `GRAM30xx`
diagnostics (`UnknownContextTarget`, `UnknownContextReplacement`,
`DuplicateContextBinding`, `ParameterizedContextBinding`, `CircularContextBinding`) had
never been about the namespace construct at all — they are the rebinding mechanism's
own validation, firing identically for a namespace header, an expression `with`, or a
publication `with`, so they moved to `Rebinding` vocabulary
(`ResolvedRebinding`/`OwnRebindings`/`Rebindings`, `UnknownRebindingTarget`,
`UnknownRebindingReplacement`, `DuplicateRebinding`, `ParameterizedRebinding`,
`CircularRebinding`) instead of the namespace's. `ContextRebinding` could not become
bare `Rebinding`: `GrammarBinder.cs` already has a *different*, syntax-level `Rebinding`
in scope via `using DotGram.Grammar.Parsing;`, so the resolved form became
`ResolvedRebinding` — the same syntax-vs-bound naming split this codebase already uses
for `Decl.Rule` vs. `RuleSymbol`. `GRAM####` values did not change, only the C# constant
names and message text. `GrammarNormalizer.Contexts.cs` was renamed to
`GrammarNormalizer.Namespaces.cs` to match.

## Reverted: eager construction violated deferred-construction semantics

A review of `main` found two problems in "Eager construction: built, wired in, and
caught its own bug" above, confirmed directly against the code rather than assumed, and
the whole feature was removed rather than patched.

**The concept itself is unsound**, independent of any bug in it. `Committed` proves only
that no alternative derivation could still replace this one through backtracking — it
says nothing about whether the suffix that follows will go on to succeed. The project's
own test already proved this observably wrong:
`GeneratorDriverTests.An_eager_eligible_rule_is_constructed_even_when_the_parse_later_fails`
asserted that `Built()` **is** called on input `"1"` for `Start : @int = value: Inner &
'x' => @(value)` / `Inner : @int = '1' => @Built()`, even though there is no trailing
`'x'`, the whole parse fails, and `Accept:` — the only other place a value is ever
built — is never reached. That directly contradicts §3 of `docs/implementation.md`
("nothing is built while matching") and the deferred-construction guarantee
`README.md` advertises. No atomic group is even involved in that example — `Inner` is
`Committed` simply because nothing precedes it to backtrack into.

**A separate, compounding bug** made `Committed` wrong more often than intended. The
runtime commit for an atomic group (the `atCommit` writer in `Machine.cs`) only marks
arena entries created *inside* the group `Dead` — entries from before the group, such as
an outer rule's own still-live alternative, are never touched; the code's own comment
said as much ("committing is about the first only... the ways back are put out and
everything stays where it is"). But the region walk in the now-deleted `Region.cs`
returned fully committed unconditionally after any successful atomic group, regardless
of the incoming committed state — narrower in the runtime than in the analysis that was
supposed to describe it.

Removed rather than patched: even with the atomic-group propagation corrected, the first
problem stands on its own — local commitment never implies eventual acceptance.
`Region`/`DecisionClass`/`ComputeRegions()` had exactly one consumer
(`ComputeEagerRules`, confirmed by grep — nothing else in `Machine`/`Compile` ever read
`_regions`), so removing eager construction left the whole region-analysis subsystem
unused, and it went with it: `Region.cs`, `Machine.Regions.cs`,
`EnsureEagerMaterializer` and the `Return:`-time trigger in `Machine.cs`,
`benchmarks/DotGram.Benchmarks/EagerConstruction.cs`, and `RegionTests.cs`. If a future
storage or lifetime optimization wants a "can this resume point be dropped" concept, it
should be built with correct commit semantics from scratch rather than inherit this
one's bug.

`MaterializeRange`'s `fromExpr` parameter — the bounded-start capability eager
construction was the only caller of with anything other than `"0"` — went with it too,
collapsed back into a plain `Materialize(file, cached)` that always walks from the start
of what changed since the last call.

## Fixed: `with` sites in different rules could not see each other's own splice

`SpecializeWithSites` built the call graph once, before iterating its rule-groups, and
walked the groups in whatever order `_pendingWith` happened to hold them — encounter
order, not dependency order. `R2 = R1 with (C = D)`, where `R1` is itself `R1 = A with
(B = C)`, needs `R2`'s own affected-set computation to see the call `R1`'s own splice
introduced (from `R1` into `A`'s with-clone); the graph built before either site ran
never had that call in it, so `R2`'s rebinding silently became a no-op wherever it only
reached its target through a call the earlier splice itself introduced. Fixed by
ordering rule-groups so a rule runs only after every other with-bearing rule its own
sites can reach, and rebuilding the call graph fresh before each group.

## Fixed: a `with` extent's own type-compatibility went unchecked

GRAM4014 (a rebinding's replacement must be assignable to what it replaces, §14) was
checked only against `GrammarNamespace.OwnRebindings` — a namespace header. An
expression `with (A = B)` or a publication's own `with (A = B)` could rebind onto an
incompatible type with nothing to say so, because `Publication.Rebindings` and
`GrammarModel.WithBindings` were already flattened to a chain-resolved `RuleSymbol ->
RuleSymbol` dict in the binder, and each pair's own position went with the flattening.
Fixed by giving both the same `OwnRebindings`/`Rebindings` split `GrammarNamespace`
already has: an entry-by-entry, positioned list for the check, the chain-resolved dict
still for specialization. `CheckNamespaceReplacements` is now
`CheckRebindingReplacements`, run over all three extents through one shared
`CheckReplacement`.

## Fixed: `GRAM3012` promoted to `Error`, and its own known gap closed

"Built: a warning for accidental shadowing inside a nested namespace" above shipped
`ShadowsEnclosingRule` at `Info` — a pointer, not a refusal, while the `with`/`namespace`
rebinding model was still settling. It has settled: a declaration always means a new
rule, and a rebinding is the only way to replace one, so silently landing a declaration
on a name that already resolves to something else is now `Error`, not a pointer.

The entry above also named its own known first-cut gap: a name shadowed only by way of
an import (`using Lib;` bringing in a name a namespace then redeclares itself) went
uncaught, because `Declare` — pass one, where the enclosing-namespace half of this check
runs — executes before `ResolveImports`, so the import is not wired up yet at the point
`Declare` asks. Closed by checking the import half separately: `CheckImportShadowing`
runs in `Resolve` (pass two), right after each namespace's own imports are resolved, so
it asks the same question — does a name this namespace just declared already resolve to
something else? — once there is something to find. Never runs for the global namespace,
which has no header syntax to suggest instead, the same exclusion the enclosing-
namespace check already made.

## Fixed: the furthest-failure set was rebuilt on every step back

Found with a profiler rather than by reading: `HotLoop.cs` runs the URL grammar's own two
losing cases against `RegexOptions.Compiled` in a tight loop, and dotTrace's CLI
(`dottrace start`, then `Reporter.exe report` for a readable XML) put
`List<string>.AddRange` and `List<string>..ctor` together at the top — 9.8M constructions
against 6.5M parses, more than one per parse, for a diagnostic nothing reads unless the
whole parse fails. Three profiling modes (Line-by-Line, Tracing, Sampling) disagreed
wildly about everything else and agreed about this.

`Fail:` is not the end of a parse; it is every local dead end backtracking walks through.
Each time the furthest position advanced, it copied a static `string[]` — one the
generator had already declared — into a fresh `List<string>`, and every one of those was
thrown away by any parse that went on to succeed.

`Failure.Expected` is now the `string[]` itself, assigned by reference, and
`Failure.ExpectedMore` (a `List<string[]>`) stays null until terminals genuinely tie for
the furthest position. The merge into one exactly-sized `string[]` happens once, in the
`TryParseX` wrapper, and only on a failure that actually reached the caller. `GetRange` in
the message builder went too — `string.Join`'s range overload says the same thing without
a second list. A flat, arena-free grammar never reaches a tie at all, so it does not
declare `ExpectedMore` and skips the merge entirely.

Measured exactly (`--alloc`): the short URL 400 → 264 B, every-part 480 → 352, host-and-
path 424 → 392, the refusal 440 → 344, forty letters 168 → 104, twenty struct-valued
numbers 2016 → 784. The refusal number also corrected a claim in `benchmarks/README.md`
and `docs/status.md` that a rejected URL allocated nothing: it allocated 440 B, and this
is what it was spending them on.

On time, the ratio against `RegexOptions.Compiled` improved on all five URL inputs —
1.30→1.57, 1.30→1.64, 0.73→0.79, 0.75→0.78, 1.50→1.73. Ratios rather than nanoseconds,
deliberately: the BCL's own numbers moved by up to a third between the two runs on code
neither change touched, so the absolute figures were measuring the machine. Two inputs
still lose to the compiled pattern, both by less than before — the refusal, and the one
that materializes every named part, which is where `MaterializationCost.cs` already
pointed.

## Built: a literal that begins another can still be decided where they differ

With the furthest-failure set no longer rebuilt on every step back, `ParserArena.Add` was
what the profiler put on top — 72M calls against 6.5M parses. The first thing to establish
was where those come from, and probing the emitter with small grammars answered it in a
way that settled most of the question by ruling things out. All of these already write
nothing at all:

```text
(':' & D+)? & '/'            an optional whose branches are told apart
U & '@'      U = ['a'..'z']+ a repetition and what cannot follow it
(U & '@')? & H               U = ['a'..'z']+, H = ['0'..'9']+
'a' | 'b'                    a choice one character decides
"aax" | "aay"                literals decided where they differ
```

So `Possessive` and `Predictive` were doing better than the counts suggested, and what is
left in the URL grammar is mostly the grammar being genuinely ambiguous — `(UserInfo &
'@')? & Host` cannot know which of the two it is reading until an `@` arrives or does not,
and `Host = IPv4 | RegName` can begin either way with a digit. Neither is an analysis
failure and neither is fixable without unbounded lookahead.

One case was not that. `LiteralRun` refused any run holding a pair where one literal
begins another, because the shorter is a second reading the parse may have to come back
for. That is true of `"ab" | "abc"` and false of `"https" | "http" | "ftp"` before
`"://"`: the longer is written first, so taking it and failing later would leave the
shorter standing at the `'s'` the longer went on with — and `"://"` does not begin with
one. The shorter reading fails wherever it is tried, and an entry leading only to a
failure is one nothing needs.

`PrefixSettled` decides exactly that, and only that. Written shorter-first the entry is
what makes the longer reachable at all and is kept; where what follows can begin with the
character the longer carried on with, it is kept; where the following set says nothing
that can be held to, it is kept. `docs/syntax.md` §11 promises alternatives are never
reordered, so the order a grammar was written in is a fact to read and not one to
normalize away.

`CompileLiterals` needed one repair to go with it: a run may now hold a literal that *is*
the shared prefix, whose own test is empty, so it takes the position unconditionally —
and everything written after it, the catch-all failure included, is unreachable. Emitting
it anyway is a `CS0162` in somebody else's build, which the test harness rightly counts
as a failure.

Worth, measured on the hot loop: **4.7%** on the URL grammar — 19.31M parses in six
seconds against 20.21M, medians of five, and the ranges barely touch (19.86M was the best
of the five before, 19.82M the worst of the five after). Four choice sites gone out of 85,
and 236 fewer lines of generated C#. The wider case is `eol` — `"\r\n" | "\n" | "\r"`,
where `"\r"` begins `"\r\n"` — which every line-oriented grammar reaches for and which now
compiles to one entry-less run wherever a line cannot be followed by a bare newline.

### Three measurements said this was a regression, and none of them was

The `Regex` comparison first reported the IP-host URL 17% *slower* — 148.2 ns against
173.9, while the compiled pattern beside it moved less than a nanosecond, which is exactly
the control that normally says an effect is real. Running the identical binary again put
it at 152.0 ns. Two of the five inputs — the two shortest, around 150 ns — swing 9% to 14%
between runs of the same build; the other three hold to within 2%.

The hot loop then said it too, at three repetitions: 23.16M against 23.87M. At five it
said the opposite, 23.84M against 23.57M, which is to say it said nothing.

So three repetitions is not enough for this machine, and a stable control does not make a
difference real when the thing being controlled is itself the noisy one. What survives at
five repetitions is the aggregate above and the two inputs that were stable all along —
the refusal and the 84-character path. `docs/next.md`'s own "Future optimization gate"
already said measure in a parser and expect noise; this is what that costs in practice,
and the answer is repetitions and medians rather than a better single run.
