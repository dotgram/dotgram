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

**The lever was pulled, and it held** — see "Built: the materializer is a method" below.
Taking the materialization walk out of the recognizer and calling it instead, the same
code either way, was worth 7% on a capture-heavy parse of the URL grammar and nothing at
all on a grammar a fifth the size. Read that as the standing recommendation this section
was making, now with a number and with the shape of when it applies.

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

## Built: one walk of a call's captures, not one per member

`MaterializationCost.cs` had said captures cost 2.9× what recognizing the same shape
does, and left three candidates inside that without telling them apart. A third variant
was meant to tell them apart: the same seven captures kept as `SourceSpan`, with not one
string built for them.

```text
                                    before      after
nothing captured                    96.1 ns    97.7 ns      0 B
captured as spans, no strings      279.8 ns   266.8 ns     88 B
captured as strings                306.1 ns   247.0 ns    328 B
```

**That variant is not the clean isolation it was meant to be**, and the after column is
what showed it: the spans are now *slower* than the strings they were supposed to be
cheaper than. Declaring seven rules `: @SourceSpan` gives each of them a value, and a
rule with a value gets a boundary of its own — so that grammar pays for seven rule
frames the string one does not, and reads each captured value back through another arena
entry. It was never measuring strings alone; the walk simply used to dominate both.

What it did establish, and what the fix then confirmed, is that most of what captures
cost was not the strings. The 26 ns the before column seemed to attribute to them is a
floor and not a figure, and nothing here should be quoted as the cost of building a
capture's text.

What the machinery was doing, in the generated materializer: `Url` has five members and
walked the same linked list of its call's captures five times, once per member, filtering
each time by kind and slot. `Authority` has three and walked it three. The list is chained
through `linkNexts`, so every step is a load from somewhere else in the arena again — five
walks is five times the pointer-chasing to read what one pass could have collected.

Now one walk, with a `switch` on the capture's slot: a slot belongs to one member, so what
was a run of comparisons per entry per member is one jump per entry. The kind is still
tested inside each case — a `Recovery` entry's own state numbering is not a capture slot's,
and nothing says the two cannot land on the same number. A sequence member keeps its own
two walks; it has to count before it can fill, and merging that in would mean counting for
members that are not sequences.

Worth **19.3%** where the members are all on one rule (306.1 ns against 247.0, with the
no-capture control unmoved at 96.1 against 97.7, so the machine was the same for both),
**7.9%** on the capture-heavy URL of `benchmarks/Urls.cs`, whose five members and three
split across `Url` and `Authority` (13.44M parses in five seconds against 14.50M, medians
of five, ranges barely touching), and 3.7% on the mixed hot loop — the cross-check, since
half of that loop is a refusal that never materializes anything at all. More members on
one rule is more walks collapsed, which is the shape of the saving.

## Built: the materializer is a method, and the gate's own lever was real

"Future optimization gate" above says the one thing left worth moving is the size of the
method the automaton is compiled into, and that splitting it is the change that would move
it. That had never been tried. The walk that turns an accepted derivation into values was
written out at `Accept:`, inside the one method that is also the entire recognizer —
about 160 lines of it for the URL grammar — and was only ever a method of its own for a
grammar whose `when` guards read a value mid-parse.

It is now always one. `EnsureMaterializer` emits it for any grammar that builds anything,
with `cached` following what the guards need rather than deciding whether the method
exists at all, and `Accept:` declares the tables the root value is read from and calls it.

**7.0%** on the capture-heavy URL — 14.80M parses in five seconds against 15.84M, medians
of five, taken back to back with a rebuild between them and no overlap between the two
sets. Compared instead against a measurement from earlier in the sitting it looked like
10.1%, which is the drift this file already has an entry about; the back-to-back number is
the one to quote.

Not on the flattened grammar of `MaterializationCost.cs`, though: 247.0 ns against 246.3,
which is nothing. That grammar's recognizer is 3,772 lines of generated C# and the URL
one's is 21,500 — the saving is in how big the method was, so a small one has nothing to
give back. Both readings are the same claim, which is why neither is a surprise.

The refusal half of the mixed loop materializes nothing and was unchanged — 228 ns against
230 by difference — which is the shape to expect: what moved is the method the recognizer
runs in, and what moved is the input that used to run the walk inline inside it.

So the gate was right, and it was right for a reason that has nothing to do with the
walk being faster: it is the same code, called rather than pasted. Nothing else about
this project's generated output has been shown to move for that reason, and it is the
first thing to try on the next hot method rather than the last.

## Built: the accepted value tree is reached, not scanned for

With the materializer visible to a profiler at last, it turned out to cost about what the
entire recognizer does — 3,427 against 3,585 in own samples on a capture-heavy URL, with
the strings it ends in a third of that at 992. So the walk itself, not what it builds.

It was three passes over every arena entry to act on two. Building the links needs them
all and still does. The other two did not: one scanned forward for entries already marked
as reachable, marking their rule-captured children as it went, and the other scanned
backward for the marked ones that were `Completed`. The marking is what says which entries
those are — so the walk that does the marking now keeps them, in a reused `int[]` on the
parser, and the build walks that list back to front instead of the arena.

Two things had to be got right, and one of them was got wrong first.

**Back to front is every child before its parent**, which is what a parent's `=>` needs,
and it is true because the list was built outwards from the root. Descending index said
the same only because a child's call is written after its parent's — true, but an
invariant a walk over a list no longer has to lean on.

**The mark also has to stop a call being kept twice.** A scan visits an index once however
many ways it was reached; a list would otherwise hold it once per way and run its `=>`
once per copy, which is the deferred-construction guarantee broken in a new place.

**And the guard path is not this shape at all.** With caching, a `when` guard calls this
mid-parse having marked whichever values its own condition asks for — a set that is not
reachable from the root and not knowable without looking. Seeding a walk from the root
there quietly builds a different set from the one the guard asked about. The first version
did exactly that; `GeneratorDriverTests`'s guard tests and two `ExampleTests` said so
immediately, which is what they are for. The scan stays for `cached`, and the two shapes
sit side by side under the flag that already told them apart.

**11.9%** on the capture-heavy URL — 16.06M parses in five seconds against 17.97M, medians
of five, back to back, with no overlap at all between the two sets. On the mixed hot loop
the running total, measured the same way at each step:

```text
after the prefix-literal change             20.21M
after one walk per call                     20.96M
after the materializer became a method      22.11M
after the value tree is reached not scanned 23.62M
```

### Where materialization stands, and why to stop here

Over the three changes, seven captures went from costing **2.19× what recognizing the
same shape costs to 1.41×** — 306.1 ns against 90.8 to 219.2 against 96.1, read as ratios
because the control moved between runs too. On the real URL grammar the profile agrees:
`Materialize_DotGram` was 13.9% of samples and is 10.4%, while doing more parses per
second, and recognition at 16.9% is now the larger of the two.

What is left inside it does not look like more of the same.

- **Building the links is the one pass over every entry still there, and it needs to be.**
  Doing it while matching instead means unpicking a chain on every step back — which is
  what `Truncate` already does for the guard path, and what its own comment describes as
  the delicate part. One clean pass at the end is cheaper than paying on every backtrack.
- **The strings are what the caller asked for.** Building them lazily would mean holding
  the input to slice later, which the `ReadOnlySpan<char>` entry point has nothing to hold.
- **`Reset` clears three arrays per parse**, about 2% of samples. Half of one of them is
  provably unnecessary — nothing ever reads a `linkNexts` slot that was not written this
  parse, since every index reachable from a valid head was linked this parse and linking
  writes it. That is a fraction of a per cent, which is the noise floor this file has an
  entry about. The `_values` clear is the larger half and cannot go without moving the
  mark to a generation stamp, which trades an O(n) clear for holding a parse's objects
  alive in a pooled parser until the next one overwrites them. Worth knowing about; not
  obviously worth doing.

The mass that is left is recognition, and 59% of samples are in native code the sampler
cannot attribute at all — most likely the character loops the JIT has inlined flat. That
is a different kind of problem from the one this section solved, and wants its own
instrument before anyone starts guessing at it.

## Built: a refusal says nothing until it is asked

Asking what `Regex` does that this does not turned up one answer worth taking, and it was
not where it was looked for.

`Capture.Value` in `System.Text.RegularExpressions` is

```csharp
public string Value => Text is string text ? text.Substring(Index, Length) : string.Empty;
```

— the match records *where*, and the string is cut when somebody asks for it. Both engines
record positions while matching; only one of them then builds every string whether or not
the caller wanted it.

That looked like it explained the URL benchmark's remaining loss on the input with every
part present, where the pattern is asked for one group and this is asked for a record of
seven. It did not: the materialization work above had already turned that input round to
1.08× while still building all seven, and the loss that was left was the refusal — where
no captures are built at all.

The refusal was allocating **344 bytes to say no**, against the pattern's zero. All of it
was the message: merging the furthest-failure arrays, joining them, and wording the
sentence — at the one moment nobody had yet asked what went wrong. So `Match<T>` now keeps
what the failure recorded, and `Error` merges and words it on access, the same bargain
`Group.Value` makes. Two references wider on the struct, and the wrapper's failure path
chooses between two literals and returns.

**344 B down to 88 B**, the remainder being the `List<string[]>` that accumulates ties
during the parse itself and cannot be deferred with the rest. On the hot loop the refusal
went from 28.97M parses in five seconds to **48.56M**, medians of five, back to back —
172.6 ns to 103.0, or **+67.6%**.

In the `Regex` comparison it takes the refusal from 0.85× to **1.36×**, which was the last
input where the compiled pattern was ahead. All five are now this side of parity: 1.86,
1.81, 1.36, 1.88, 1.03.

Worth naming what this does not change. The seven eager strings are still seven eager
strings, and a caller who wants one of them still pays for all seven; `Group.Value` would
not. Left undone deliberately, with the asymmetry stated.

**The reason first given here for leaving it undone was wrong**, and is struck rather than
quietly edited. It said making them lazy "needs an input to slice later, and the
`ReadOnlySpan<char>` entry point has nothing to hold". There is no `ReadOnlySpan<char>`
entry point: every published one takes a `string` (`ParseX`, `TryParseX`, `FindX`, and
`TextReader`/`IEnumerable<string>` for the streaming finds), the span exists only inside
the recognizer, and `TryParseX` has the string in hand at the moment it hands back a value.
The emitted value types are ours too — a `sealed class` of auto-properties this generator
writes — so their shape is not a constraint either. What the entry above should have said
is in "Deferred materialization: what actually stands in the way" below.

## Measured: what the seven eager strings actually cost, against a pattern's one lazy one

The entry above left an asymmetry standing and named it: seven parts are built whether or
not the caller wants seven, and `Group.Value` builds one. What it did not say is that
`UrlBenchmarks` was itself built on the pattern's side of that asymmetry. Its own comment
claimed both sides were asked for the parts rather than for a yes — and the code read one
named group from the regex and one property from the record. One string against seven,
timed as though it were like against like, for every number this project has published.

Not fixed by changing the pair, which would only have moved the thumb to the other side of
the scale. A second pair was added beside it, reading all seven from both, and the class
comment now says plainly that "the parts" is two questions: one part read, which is the
shape of question a lazy match object is designed for, and every part read, which is the
shape a typed record is. Both are timed. `CheckTheyAgree` gained a total over every part,
so a run where one side reads six and the other seven is refused rather than measured —
the same rule the benchmark already applied to the parts themselves.

`DefaultJob`, 2026-08-25, against `RegexOptions.Compiled`:

| input | one part | every part |
| --- | --: | --: |
| `http://example.com` | 1.86× | **2.71×** |
| `https://192.168.0.1/` | 1.82× | **2.62×** |
| `https://exa mple.com/` — no match | 1.34× | **1.39×** |
| a 47-character URL with every part | 1.12× | **1.65×** |
| an 84-character path of eight segments | 1.91× | **2.52×** |

Reading all seven costs this **nothing**: every row moved by less than 4% from its own
one-part row, in both directions, and allocation is identical to the byte. There is nothing
for it to cost — the strings existed before the call returned, and a property is a field.

It costs the compiled pattern **32% to 49%** and 32 to 208 bytes: 263→378, 248→370, 412→544
and 253→376 ns. Only the refusal is flat, having no parts to cut.

So the asymmetry is real in both directions, and neither table is the honest one alone.
Deferral wins a caller who wants one part out of seven, by roughly the margin the first
table shows; it loses the caller who wants the parse, by the margin the second does. The
design question it was raised against — whether to make these lazy — is not settled by
this and was not meant to be. What is settled is that the instrument no longer answers only
one of the two questions while claiming to answer both.

One thing to carry forward about the instrument itself: the 47-character URL, which this
repository's own guidance called stable to within 2%, moved 6.6% (242.5 → 226.6 ns) between
the two runs `benchmarks/README.md` has carried, on parsing code neither run touched. Only
the 84-character path has earned that 2%. The guidance in that file is corrected to say so.

## Deferred materialization: what actually stands in the way

Raised again after the every-part measurement above, and the honest answer is that the
obstacles named earlier in this file were not obstacles. A capture of a contiguous extent
is the input string and two integers; the string is in `TryParseX`'s hand, the two integers
are in the arena at the moment the value is built, and the value type is one this generator
writes. None of the three is a constraint.

What is real, in order of how much it costs to answer:

**The arena is recycled.** `ReturnParser` puts the parser back before the wrapper returns,
so a value must copy `(from, to)` out rather than point into `entries`. Two integers copied
where a reference is stored today, which is not a cost worth discussing.

**The value object grows.** A lazy string member is `string? _cache` plus two integers
where an eager one is a single reference, and the object needs one reference to the input
besides. `UrlValue` and `Authority` together would gain roughly 64 bytes, and the seven
strings — 30 to 60 bytes each — would not exist until read. A caller who reads one part
wins; a caller who reads all seven pays the object growth and a branch per read for
nothing. That is the same shape of trade as the two tables in `benchmarks/README.md`, which
is why it is a real decision rather than an obvious win.

**Only a flat contiguous string capture qualifies.** A `=> @(...)` factory cannot be
deferred without deferring the consumer's own C#, moving when its side effects happen and
when it throws — that is a change of meaning, not an optimization. A typed conversion
(`@int` and its kind) is deferrable in principle and raises the same question from the
other end: a malformed number fails the parse today and would fail a property read instead.
A repetition member is a list, not two integers. `@SourceSpan` already costs nothing and
has nothing to defer. What is left is the leaf string capture — which happens to be the
common case, and is exactly what the seven parts of the URL benchmark are.

**The streaming finds cannot have it at all.** `FindX(TextReader)` and
`FindX(IEnumerable<string>)` read through `Window`, which advances over chunks. A value
handed out of that iterator would hold two integers into a chunk that has moved on. Those
entry points stay eager, which is a split between entry points — small and contained, but
it has to be said in the documentation rather than discovered.

**And the input outlives the value.** This is the one that decides the design. Today a
value is self-contained and the input can be collected the moment the caller drops it.
Lazily, any surviving part keeps the whole input alive: three names lifted out of a
ten-megabyte document are three short strings now and ten megabytes then. `Regex` takes
that bet — a `Match` holds its input — and it is not free; it is simply invisible until
somebody parses something large.

So the question is not whether it can be done but what the default should be, and the
grammar already answers questions of that shape by declaration rather than by switch: a
rule says `@SourceSpan` when the caller wants the extent instead of the string. A third
form — the string, cut when asked — belongs in the same place, chosen by the author of the
grammar, who is the only one who knows whether the input outlives the value. A global
option would be the wrong instrument, and so would a change of default.

Not started. Written down so the next attempt argues about the default rather than about
whether the two integers are reachable.

## Built: a predicted choice steps over the terminal it just tested

Recognition was the next target and there was no instrument for it — a profiler attributes
everything to one 21,500-line method, which is a way of saying nothing. So the instrument
came first, and it cost less than the profiling had: `tests/Snapshots/Url.gram.g.cs` is a
complete generated parser checked into the repository, so a script can put `Visits[N]++`
after each of its 1,409 state labels and a run says exactly where the automaton went.
Exactly, not approximately — the counts are deterministic, so one parse per input is the
whole measurement and there is nothing to sample.

The first thing it said was that **3% of the automaton runs**: 48 to 79 states of 1,433,
and four to seven state visits per input character.

The second thing was worth more. Three states carried 55% of the visits on the long path,
and they were doing this:

```csharp
S75: c = text[p];
     if (Unreserved(c)) goto S69;   // the class is tested
S69: if (p >= text.Length) …        // p has not moved
     c = text[p];                   // the same character, read again
     if (!Unreserved(c)) fail;      // the same class, tested again
     p++;
```

A predicted dispatch reads the character and tests it against each alternative's first set
to choose a branch. The alternative it chooses then begins with its own terminal — and asks
all three questions over again about a character nothing had moved past. Two bounds checks,
two loads and two class tests per character, in the loops a grammar spends its life in.

It comes out because the dispatch's answer is a fact about the position, and nothing can
arrive between the two: a predicted choice writes no way back, and of 1,409 states only 105
are ever stored in an arena entry as somewhere to resume — none of these. The `switch
(state)` table lists every state, which made it look as though anything could be re-entered,
but most of those cases are unreachable.

So `CompilePredictedChoice` now emits `if (test) { p++; goto rest; }` and never compiles the
alternative's leading terminal at all. `BeginsWith` has to look through an inlined call to
find it: `Unreserved = [Digit | 'a'..'z' | …]` is a character class wearing a name, and an
alternative that calls it begins with its element as surely as if the class had been written
in place. Without that the change fired in four places instead of twenty-five. The two tests
are compared as text, and the one wording difference bridged is the bracket
`CSharpEmitter.Test` adds and `RangesTest` does not.

The URL grammar's hot loop is now one block, and three of its four alternatives collapse to
the same two instructions:

```csharp
if (Unreserved(c))        { p++; goto S60; }
if (SubDelim(c))          { p++; goto S60; }
if (c == '%')             goto S65;
if (c == ':' || c == '@') { p++; goto S60; }
```

21,574 lines to 21,111, and 1,409 states to 1,388.

## What that change actually measured: dynamic PGO, not work

The first run of it disagreed with itself, and chasing that down is worth more than the
optimization.

An A/B on the snapshot grammar said every input got faster. `DefaultJob` on the benchmark
said one input — the 47-character URL with every part — got 11% slower. The first confound
was mine: **the benchmark's grammar is not the snapshot's.** `benchmarks/Urls.cs` leaves out
`IPLiteral` and the nine-alternative `IPv6` rule, so the two are different parsers and an
A/B on one predicts nothing about the other. Measured again on the benchmark's own binaries,
in separate processes and alternating: the regression was real, −9.5%.

Then the state counter said something that ruled out the obvious explanation: **the visit
counts are identical, input for input, before and after.** Nothing was added. And 15.5% of
that input's visits were to states whose redundant test had just been removed. Less work,
same steps, slower.

`DOTNET_TieredPGO=0` settled it. With dynamic PGO switched off the same pair goes from
−8.5% to **+5.7%**, and the long path from +14.3% to **+24.3%**. The loss is the
profile-guided block layout making a different guess about a method this size, and the
guess it made about the old code happened to be better for that one input.

The clincher is which input loses. On the benchmark's grammar it is the 47-character URL; on
the snapshot's larger one, the same input **gains 10%** and the refusal loses 5% instead. A
change that were bad for a kind of input would lose on the same input both times. This one
does not, because it is not about the input.

What the shipped form measures, medians of five, each variant in its own process and the
two alternating so that machine drift lands on both:

| input | benchmark grammar | snapshot grammar |
| --- | --: | --: |
| `http://example.com` | +10.3% | +15.2% |
| `https://192.168.0.1/` | +1.6% | +7.0% |
| `https://exa mple.com/` — no match | +3.6% | −5.1% |
| a 47-character URL with every part | −12.6% | +10.0% |
| an 84-character path of eight segments | +17.2% | +17.5% |

With `DOTNET_TieredPGO=0`, the benchmark grammar's two extremes are +5.7% and +24.3%.

Three things follow, and only the first is about this change.

**It ships.** It removes work unconditionally, adds no step, and wins on four of five inputs
of each grammar and on all of them once the layout lottery is taken out.

**Two instruments were wrong and are now right.** An A/B must be built from the artifact
under test, not from a similar one; and the URL benchmark's numbers can move by a tenth on
one input for reasons that have nothing to do with the code under test. `benchmarks/README.md`
already said the shortest two inputs cannot be compared between runs. This says something
worse: on a method this size, any input can move that far on any change that shifts a block,
and the direction means nothing.

**It is the strongest argument yet for splitting the automaton.** The standing plan was to
split it for instruction-cache locality, on the evidence that extracting the materializer
was worth 7% on the big grammar and nothing on the small one. This is a better reason: a
21,500-line method is large enough that where PGO puts the blocks matters more than what the
blocks do, and neither the generator nor the consumer has any say in it. Smaller methods are
not merely faster — they are measurable.

### Built: `--against`, because the ratio needed an instrument of its own

The published tables could not be refreshed for this change, and the third attempt is what
said why rather than how to wait. Three `DefaultJob` runs went in the bin: the benchmark's
own two rows that must agree — `.Gram` and `.Gram, every part`, which do the same work —
came out 21% and 28% apart, and in the third only three of the five input blocks were
usable, because BenchmarkDotNet runs blocks in sequence and interference is local in time.

That check is worth naming on its own. `UrlBenchmarks` grew a second pair of measurements
to answer a question about laziness, and the pair turns out to double as a validity gate:
two timings of the same work, in the same run, that a reader can compare. A benchmark that
can tell you its own run was no good is worth more than one that cannot.

The cause is in BenchmarkDotNet's design and is the right design for what it is for. It runs
each case in a process of its own, one after another, which is how an absolute number should
be taken — and it means `.Gram` is measured at one minute and `Regex` at another. A ratio
between them assumes nothing about the machine changed in between. On an idle machine
nothing does. This one was not idle.

So `--against` (`benchmarks/DotGram.Benchmarks/Against.cs`): the same six methods, called
through `UrlBenchmarks` itself so the work is the same work, measured round-robin — every
method once per round, rounds repeating, one process. Whatever the machine does to one
measurement it does to the five beside it. It subtracts the loop and the indirect call
(1.5 ns here) because a constant added to both sides of a ratio drags the ratio towards one,
which flatters whichever engine is slower. It warms at full size, twice, because a short
warmup leaves a method at tier zero and the first round then measures the tiering — the
first version of this reported an 785% spread and that was what it was.

Through the same conditions that ruined three `DefaultJob` runs: every method's spread
between 0.3% and 6%, and two independent runs agreeing to within 0.05 on every ratio.
`benchmarks/README.md` and `docs/status.md` now carry those numbers, and say which
instrument to reach for when.

## Built: `with` proved end to end, on the question a consumer actually asks

`with` had thorough coverage against the normalizer's own output — that a rebinding clones
what it reaches, composes with an enclosing one, does not leak into a sibling publication,
and reports what it should. All of it stops at the model. None of it asked whether the
generated code works.

The case that matters is one rule published several ways:

```dotgram
Port       = Digit+
PortAsInt  : @int = Digit+ => @Number(parserText)
PortAsSpan : @SourceSpan = Digit+
HostAsSpan : @SourceSpan = (Letter | Digit | '.' | '-')+

parse Url as UrlWithStrings
parse Url with (Port = PortAsInt) as UrlWithInt
parse Url with (Port = PortAsSpan, Host = HostAsSpan) as UrlWithSpans
```

Three publications of one grammar, where the rebinding changes a member's *type* — so the
three cannot share a value type, and the generator has to emit three. It does, and they
parse: `Port` comes back `string`, `int?` and a `SourceSpan`, `Host` is the same string in
the first two because the binding does not reach it, and an absent port is null in all of
them. Four tests in `GeneratorDriverTests`, and they went green without a change to the
generator — which is the result, since the model-level tests could not have told us.

Worth saying what this settles. The open question under deferred materialization was what a
capture should cost and who decides — a string, an extent, or something lazy. It is already
decided, and not by the generator: a rule declares its type, `with` republishes the same
grammar with a different one, and the consumer picks per publication rather than per
compilation. The one thing missing for a consumer's own lazy type is that nothing supplies
the input to a construction — `parserText` is a string already cut and `parserSpan` is two
integers — so a user's lazy struct can hold the extent but must be handed the text to cut
it. A supplied `parserInput` beside the three §8.2 already has would close that, and is not
built.

## Built: `parserInput`, the one thing a consumer's own lazy value was missing

The `with` work above settled who decides what a capture costs — the grammar author, per
publication. One thing was missing to make the third option buildable at all. A value that
means to keep where it matched and cut its string later needs two things: the extent, which
`parserSpan` already gave, and the input to cut it out of, which nothing did. `parserText`
is a string already cut, which is the very allocation such a value exists to avoid.

So `parserInput`, beside the names §8.2 already supplies:

```dotgram
Host : @Text = (Unreserved | SubDelim)+ => @(new Text(parserInput, parserSpan))
```

Threaded rather than reconstructed. The engine, the wrapper and the materializer take a
`string` parameter, and only when something in the grammar names it — a grammar that does
not is compiled exactly as before, which the snapshots confirmed by not moving.

**A rule that asks for it gets no reader overload**, reported as `GRAM5001` at `Info` like
every other refusal of §6.3, and next to the one for `parserSpan` that it is the twin of. A
stream is what having no whole input is called: a window holds the part being read, and
handing that over under this name would be one name meaning two things and the wrong one
silently. That refusal is also what makes the threading safe — probes and streamed wrappers
pass `null!`, and a probe only exists for a publication whose rules provably never name it.

Two things this does not do, both deliberate and both worth writing down.

**It does not choose a default.** `: @string` is eager, `: @SourceSpan` is the extent with
no string, and this is the middle one. Which to use is said in the grammar. The cost is
real and belongs to whoever writes it: a value built this way keeps the whole input alive
for as long as any part of the result lives, which for three names lifted out of a large
document is the document. `Regex` takes the same bet with `Match`.

**It works in `=> @(...)` and not in `=> @Method(...)`** — which turned out to be wrong,
and is corrected in the entry below.

## Fixed: seven of the eight supplied names were undefined in an argument list

`=> @Hold(parserSpan, parserInput)` was refused with `GRAM3002: No rule, parameter or
capture named 'parserSpan'`, while `=> @(new Held(parserInput, parserSpan))` was accepted.
The entry above recorded that as a property of the language. It was a missing line.

§2 has a rule about this and it is deliberate: a bare name in an argument list is a
**grammar** name, looked up among rules and captures, and every C# name needs its own `@` —
which is why `@int.Parse(d, CultureInfo.InvariantCulture)` is refused and
`@int.Parse(d, @CultureInfo.InvariantCulture)` is not. Two tests in `SemanticTests` hold
that, and it is worth holding: it is what lets the grammar compiler catch a mistyped capture
instead of the consumer's C# compiler catching it in a file nobody wrote.

The names the parser supplies are among the names a rule has, exactly like a capture — which
is why they resolve at all. The binder registered one of them:

```csharp
_captures.Clear();
_captures.Add("parserText");
```

So `@int.Parse(parserText)` worked and the other seven did not. Nothing downstream cared:
a factory's parameters come from the rule's captures (`graph.Results`) and the supplied ones
from `Asks`, which reads the lowered text — and both forms lower to the same
`Construction.Expression`. The two notations were only ever different at the binder.

Registering all eight needed the list to be somewhere both halves can see it. It lived on
`Recovery` in `Grammar.Model`, and `Grammar.Binding` has no dependency on `Model` and should
not acquire one — the dependency runs the other way. Moved to `Binding.SuppliedNames.All`,
with `Recovery.Supplied` now pointing at it, so there is one list rather than two that have
to agree.

**And then the decision went the other way, which is the entry below.** Registering all
eight names made the call form work, but left the two spellings of one construction
accepting different things — and that, not the missing names, was the actual defect.

## Decided: nothing under an `@` is resolved, so the two spellings are one thing

`=> @M(a, b)` and `=> @(M(a, b))` did not accept the same things. The parenthesized form
went across as text; the call form had its arguments looked up as grammar names, so a C#
name needed its own `@` in one and not the other, and a name the parser supplies resolved
in one and was undefined in the other. The entry above fixed the second half of that by
registering all eight supplied names. This removes the cause.

The rule that stood was deliberate and written down: §2 with no exception for argument
lists — a bare name there is a capture, a rule or a parameter, and C# is reached with `@`,
which is why `@int.Parse(d, CultureInfo.InvariantCulture)` was refused and
`@int.Parse(d, @CultureInfo.InvariantCulture)` was not. What it bought is real: the grammar
compiler catching a mistyped capture in that position, rather than the consumer's C#
compiler catching it in a file nobody wrote.

It is given up anyway, and the argument that settles it is not about this position at all.
**Resolving names inside a consumer's C# is a commitment to keep up with C#.** This compiler
is not a C# compiler and will not become one, so every construct it has not learnt is a
construct the language forbids for no reason of its own — a limit that grows as C# does and
that nothing here can pay down. Catching one class of typo a little earlier is not worth
standing in front of the language the consumer actually writes in.

So `ResolveExpression` stops at a call whose target is C#, in a `=>` or a `when` value.
Outside an `@` nothing changes: a call to a rule of the grammar still takes grammar names,
and one that names nothing is still found here. The line is the `@`, not the bracket.

Two tests went with it, and their replacements say the new rule with the old one's reasoning
kept: `Everything_under_an_at_sign_is_the_consumer_s_own_C_sharp` runs three spellings of one
construction through the same assertion, and `And_a_name_in_a_grammar_argument_list_still_is_one`
holds the half that stayed. §7.1's table entry is rewritten to match.

## Later: something other than a name on the right of a rebinding

`with (A = B)` takes a name on each side and nothing else — `ParseRebindings` is two
`ExpectName()` calls around an `=`. So a substitution used in one place still has to invent
a name for what it substitutes, and a rebinding that would rather be written in place is
refused by the parser:

```dotgram
parse Start with (A = a : int "aa" => A(a))
// GRAM2001: Expected ')'.  GRAM2001: Expected '='.
```

Written today in two steps, which is the workaround and not a bad one:

```dotgram
B : @int = a: "aa" => @Make(a)
parse Start with (A = B)
```

**Two tiers, and they are not the same feature.** Worth keeping apart when this is picked
up, because the first is much cheaper than the second.

*An expression on the right* — `with (Sep = ',' | ';')`. No type, no captures, no factory,
no value: the same thing `A = B` is, with a body instead of a name. `with (Point = Comma)`
already means exactly this and only spells it with a name.

*A whole rule on the right* — the example above, with a type, captures and a `=>`. This is
the one that costs, and what it costs is a property worth naming: today a rebinding is
**fully checkable before anything is generated**, because both sides are symbols. That is
where GRAM4014's assignability check lives, and what lets specialization clone exactly what
the binding reaches — `Number_With1` in the normalizer's own output. An inline rule gives
the binding a body, and the body has to go through specialization, which works on symbols
rather than trees.

**And a scoping question that does not arise for a name.** A rule resolves where it is
declared. An inline rule inside a `with` on a publication is written in one place and
substituted into another — so which namespace's `trivia`, imports and rebindings does it
see? The answer has to be chosen rather than inherited, and choosing it wrongly is the kind
of thing §5.1 exists to keep unambiguous.

Not started. Recorded so the next attempt begins at the expression tier and argues about
scope before syntax.

## Built: reading the minimal snapshot, and taking out what it showed

`tests/Snapshots/Minimal.gram` — `A = "a"`, one publication, nothing to backtrack into —
exists so that every step up from it shows what that step cost. Read for its own sake it
shows something else: what the generator writes when there is nothing to write.

Four lines went, all of them in the generated file rather than in what it does.

**The jump into the first state, and its label.** `goto S5;` stood immediately above `S5:`.
`RenderStates` already strips a trailing jump where its target is the state written next —
that peephole simply was not applied to the jump in. The label goes with it where nothing
else names the state, because C# warns on a label nobody jumps to and this file is compiled
in somebody else's build, possibly with warnings as errors.

**`text[p + 0]`, and `p + 1 > text.Length`.** The general form of an index and of a room
check, written where the general form is not the question. `At`, `Short` and `Room` say them
the short way at one.

**Two `if`s with the same body, one after the other.** The room check and the first
character's test fail identically, so they are one question — `if (p >= text.Length ||
text[p] != 'a')`. Only where nothing is written between them, which means not under
`_starves`: starvation marks the failure before reporting it, and belongs to the length
alone. `Url` and `Feed` are unchanged for exactly that reason.

None of it is faster, and it was not meant to be. The JIT's own output says so.

## Measured: two things the JIT already does, so we do not

Both raised while reading that file, and both answered with `DOTNET_JitDisasm` rather than
with an opinion. Recorded because the next reader will have the same two thoughts.

**A jump to the label below it costs nothing.** `Recognize_A_Whole_Flat` at FullOpts goes
from its prologue straight into the bounds check — the body of `S5` — with no `jmp` between.
That is not a property of this method: folding a jump to the block that follows is what
basic-block layout is. It came out of the generated file for readability, not for speed.

**`text.Length` needs no local of its own, even in the 21,500-line method.** The obvious
worry is that a span field read 183 times is 183 dereferences. It is not:

```asm
mov  r12, bword ptr [rcx]        ; text._pointer  -> r12, callee-saved, kept
mov  eax, dword ptr [rcx+0x08]   ; text._length, read once
mov  dword ptr [rbp-0xC4], eax   ; and spilled
```

One write to that slot, 183 reads of it, and the span's own byref is never read again. A
hand-written `var length = text.Length;` would compile to the same thing — one load, one
spill, a stack read per use — because a method this size cannot hold it in a register and
neither can we. The `[reg+0x08]` reads elsewhere in the listing are `ParserEntry` fields and
list counts, not the span.

## Built: one array per distinct set, and only where something names it

`DeclareExpected` wrote a `static readonly string[]` for every call and never looked at what
it had written before. Two faults compounded, and the URL grammar carried **1,137 arrays**
because of them.

**The same set was written out again for every site that wanted it.** A rule called from two
places, a character class appearing in two alternatives — each got a field of its own with
identical contents. Now one name per distinct list.

**An array was written whether or not anything named it.** A site declares one before it
knows whether it will fail that way, and some do not: a shared-prefix run that turns out
`settled` writes neither the later texts nor the catch-all, so the name it reserved reached
nothing. `EmitTerminalFailure` is the only thing that ever writes such a name into a state,
so marking there is exact — `Extra` now yields only what was marked.

| grammar | arrays | generated lines |
| --- | --: | --: |
| `Url` | 1137 → **22** | 21,111 → 18,881 |
| `Feed` | 55 → **12** | 2,261 → 2,175 |
| `Csv` | 7 → **5** | 1,253 → 1,249 |
| `Minimal` | 2 → **1** | 211 → 209 |

**It is not faster, and it was not going to be.** Two binaries, separate processes,
alternating, medians of five: −1.2, −1.5, −1.8, +4.0, +1.0 per cent, which is this machine's
noise and nothing else. What it removes is 1,115 static fields, each an array allocated when
the type is first touched and held for the life of the program, and a tenth of the file the
consumer compiles. Recorded so that nobody re-runs the benchmark hoping.

## Asked and answered: the `expected` local stays

Raised while reading the flat recognizer — `string[]? expected = null;` and a write to it
before every `goto Fail`.

**In the arena engine it is load-bearing.** `Fail:` is not the end of a parse; it is every
local dead end backtracking walks through, and it decides rather than records:

```csharp
if (lookahead < 0 && p > failure.Position) { …; failure.Expected = expected; … }
else if (lookahead < 0 && p == failure.Position && expected is not null) …
```

Writing `failure.Expected` at the terminal instead would let every dead end overwrite the
furthest-failure record, which is the bug "the furthest-failure set was rebuilt on every step
back" fixed. Inside a lookahead nothing is recorded at all, so the value is often carried and
then dropped — which is what a local is for.

**In the flat path it could go**, since `Fail:` there is unconditional and the path is
deterministic: one attempt, one write either way. It stays anyway. `EmitTerminalFailure` is
shared by both paths, and splitting it to save one line and one local in the simpler of the
two is a worse trade than the line.

## Built: the flat recognizer takes the wrapper's name instead of being forwarded to

`Minimal.gram.g.cs` ended with this, and every lowered publication with something like it:

```csharp
static int Recognize_A_Whole(ReadOnlySpan<char> text, int pos, ref Failure failure)
{
    var end = Recognize_A_Whole_Flat(text, pos, ref failure);
    return end;
}
```

`RenderFlatWrapper` exists to give the lowered path the signature `RenderWrapper` would have
produced, "so the caller cannot tell which one it got". What it actually adds is the `out`
parameter a rule with a value needs, and the line that fills it. A rule without one asks the
recognizer exactly what the wrapper would have asked and hands back exactly what it answered
— the two signatures are one signature, and what stands between them is a call and a name.
So the recognizer takes the name.

## And that leaves `RenderFlatWrapper` with no reachable case

Worth writing down rather than acting on, because the conclusion is stronger than it looks.

The wrapper is now emitted only where `results.QualifiedOf(rule)` is not null — a rule with
a value of its own. A value comes from a `Node.Construct`; the normalizer makes one even for
`: @SourceSpan`, rewriting it to `Construct(…, Expression("parserSpan"))`. And `CanLower` is
`Silent`, which has no case for a construction and defaults to not silent. So a rule with a
value is never lowered, and a lowered rule never has one: the two conditions cannot both
hold.

Checked rather than reasoned at: `A : @SourceSpan = "a"` compiles to the arena engine, with
`Recognize_A_Whole` coming from `RenderWrapper` and no flat method at all.

The method's own `IsExtent` branch was written for exactly the case that cannot arise, and
one of its arms was already commented "Not reachable". Removed, which is this repository's
practice with an analysis that has no reachable target — mixed lowering and eager construction
went the same way. The lowered recognizer is emitted under the name the caller uses and there
is nothing between them.

The argument for keeping it was that widening `CanLower` to admit a construction whose value
is its own extent — the one construction that needs no arena — would want it back. That is a
reason to write it then, against whatever that widening turns out to need, rather than to
keep a method no call reaches in the hope that a future one will. `IsExtent` stays: the arena
path uses it in four places.

## Built: a literal is one comparison, not one per character

`Minimal.gram` grew a second rule — `A = "a"` beside `B = "abcd"` — and the four-character
one showed what a multi-character literal had been costing.

The old shape was a room check and then one `if` per character. The disassembly says what
that really was:

```asm
cmp  esi, ecx                    ; bounds check for p+0
jae  → throw
cmp  word ptr [rax+2*r10], 97    ; 'a'
lea  esi, [rdx+0x01]
cmp  esi, ecx                    ; bounds check for p+1, AGAIN
…                                ; and for p+2, and p+3
```

**Four bounds checks the room check above had already made unnecessary.** `p + 4 <= Length`
does not tell the range-check eliminator that `p + 1 < Length` without also knowing `p`
cannot overflow, so it kept every one. And it could not widen the four comparisons either:
they are short-circuiting branches with an order that is observable.

`SequenceEqual` against a constant is a comparison the JIT recognizes, and it unrolls that
one itself:

```asm
mov  rcx, 0x64006300620061       ; "abcd", four characters in one constant
cmp  qword ptr [rax], rcx        ; one compare
```

So a literal of two or more characters is now `text.Slice(p, n)` against the constant, and
the position of the character that did not fit is worked out **afterwards**, inside the
branch that comparison already failed. Nothing reaches it unless the parse is failing, and
the last character needs no test of its own: if every earlier one matched and the whole did
not, it is the one. `CompileLiterals` does the same for each alternative's remainder — which
is where `"https" | "http" | "ftp"` lives.

Case-insensitive literals stay per character. What they compare is each character folded,
which is not the comparison any span method makes.

**Measured, two binaries alternating, medians of five:** +6.5% and +7.0% on the two inputs
whose parse is mostly scheme and host, +2.0% on the refusal, and −0.4% and −4.5% on the two
where a literal is a small part of the work. The last is the layout lottery again and says
so under the usual test: with `DOTNET_TieredPGO=0` the long path's −6.5% becomes −0.1%,
flat, while nothing else moves.

**`AsSpan` is written out rather than left to the implicit conversion**, which arrived with
.NET Core 2.1. `DotGram.Compatibility` builds the emitted code for `netstandard2.0`, where
`string` does not convert to `ReadOnlySpan<char>` on its own — it caught this as a `CS1503`
in a file nobody wrote, which is exactly what that project is for.

## Measured: what a list pattern does, and why it is not this

Raised as the other way to write it — `text.Slice(p) is ['a', 'b', 'c', 'd', ..]`, or the
same thing as a `switch`. Both lower identically, and the answer is interesting enough to
keep:

```asm
cmp  ecx, 4                      ; the length, once
jl   → fail
cmp  word ptr [rax], 97          ; 'a'
cmp  word ptr [rax+0x02], 98     ; 'b'
cmp  word ptr [rax+0x04], 99     ; 'c'
cmp  word ptr [rax+0x06], 100    ; 'd'
```

**Every bounds check is gone** — the indices became constant offsets from the slice's base
with its length tested once, which is exactly what the `text[p + i]` chain could not manage.
But there is no widening: four narrow compares where `SequenceEqual` makes one.

| | bounds checks | comparisons |
| --- | --: | --- |
| one `if` per character | 4 | 4 × 16-bit |
| list pattern | **0** | 4 × 16-bit |
| `SequenceEqual` | 1 | **1 × 64-bit** |

So for a literal, `SequenceEqual` wins. Where a list pattern would win is the thing
`SequenceEqual` cannot express at all: a fixed-length run of character *classes*
(`[>= '0' and <= '9', …]`) — `Octet`, `H16`, and every `Class{n}` in any grammar, all of
which pay a bounds check per character today for the same reason the literal chain did. Not
started, and this is where to start it.

## Built: the emitted language-version floor, which was never checked

`emitted-code.md` said "assume nothing about the consumer's language version or TFM", and
the TFM half was tested — `DotGram.Compatibility` builds the generated code for
`netstandard2.0`, and it caught a `string`-to-span conversion an hour before this. The
language half was tested by nothing: that project pinned `LangVersion 12`, so anything the
emitter wrote up to C# 12 passed in silence.

What the floor actually is, once looked at: **C# 8**, and not by choice. Every emitted file
opens with `#nullable enable`, so nothing lower can ever compile. And exactly one construct
in all of the generated code exceeded it — `expected is not null` in the arena's `Fail:`,
C# 9 — now written `!= null`. (A grep for `record` also hit, in a comment.)

So the floor is declared C# 8 and the compatibility project's `netstandard2.0` target
compiles at it. Two things had to move for that to mean anything: `ImplicitUsings` is off
there, because the file it generates is C# 10 and would fail before ours was judged, and
`Consumer.cs` is rewritten in C# 8 — block namespace, concatenated strings, an ordinary
constructor. That last one is the lesson: the first attempt at this measurement reported
zero errors in the generated file, which looked like a pass and was not. `Consumer.cs`
failed to *parse* at the floor, so `[Gram]` went unrecognized, the generator produced
nothing, and the file being grepped was left over from the previous build.

## And the floor is a floor, not a ceiling

Raised while the above was being written, and it reframes it: **a generator runs on every
compilation.** Nothing generated outlives the compilation that produced it, so there is no
reason all consumers must get the same C#. `context.ParseOptionsProvider` gives the
effective `LanguageVersion`, and emitting the better form where it is available costs
nothing at the floor.

Effective is the word that matters. It is what the consumer's `<LangVersion>` resolves to,
so an explicit `<LangVersion>8</LangVersion>` on a `net8.0` project reads as 8 — while
`#if NET8_0_OR_GREATER` would read that same project as new enough and hand it code it
cannot compile. There is no preprocessor symbol for the language version, and the TFM is
not one.

The cost is that every such site needs both forms written and both kept working, which is
affordable only because the floor build now exists to check the second.

**It was first written down here as a second cost — that one grammar would stop producing
one parser — and that was wrong.** What makes two parsers different is what a consumer can
observe: which inputs they accept, which values they build, what they say when they refuse.
A `switch` over list patterns and an `else if` chain computing the same `p` differ in none
of those. By that reasoning Debug and Release are two parsers, and so is one JIT version and
the next — and so, measured twice today, is one run and the next, since profile-guided
layout moved an input by 6.5% with nothing else changed.

The line is real but narrower: **method bodies only.** A body is nobody's business but the
compiler's. The shape of an emitted type is the consumer's — a `record` and a class differ
in equality and `ToString` — and so is a signature, and so is the text of a diagnostic. Vary
one of those by language version and the same grammar does mean two different things.

The case that raised it: the position-sharpening chain inside a failed literal comparison
reads better as a `switch` over list patterns (C# 11), and it was measured — bounds checks
all gone, four narrow compares. It is cold code, so the gain is readability. Not built, and
now the decision is about whether one grammar producing two parsers is worth a nicer cold
block, rather than about whether it is allowed.

## Measured and rejected: a list pattern for a bounded character-class run

The place a list pattern was supposed to win. `Octet = Digit{1,3}` compiles to a loop with a
count check, a room check, a load and a class test per character; the same thing as one
decision reads better and, going by what the literal case showed, ought to lose the bounds
checks:

```csharp
var taken = text.Slice(p) switch
{
    [Digit, Digit, Digit, ..] => 3,
    [Digit, Digit, ..]        => 2,
    [Digit, ..]               => 1,
    _                         => 0,
};
```

**It is twice the code.** 57 instructions against the loop's 28, 16 compares against 7. The
element tests themselves are ideal — `movzx; sub 48; cmp 9; jbe`, the unsigned-range trick,
three instructions a digit — but Roslyn's decision DAG splits on **length first**, so the
`len < 2` branch and the `len >= 2` branch each test element zero over again. The loop is
compact precisely because it interleaves the length test and the element test once per
position instead of enumerating the lengths.

Rejected on the measurement. Which leaves the loop, and looking at *its* disassembly is what
found the next entry.

## Built: the room check written the way the bounds check is written

The run loop had two identical comparisons back to back:

```asm
cmp  edx, ecx      ; our `p >= text.Length`
jge  → break
cmp  edx, ecx      ; the indexer's own bounds check, same registers
jae  → throw
movzx r10, word ptr [rax+2*rdx]
```

Signed and unsigned are different predicates, and the range-check eliminator will not treat
one as the other without proving `p >= 0`. So it kept both — one extra compare and branch per
character, in the hottest loop any grammar has.

Written `(uint)p >= (uint)text.Length`, they are one comparison and the throw path goes with
them. Every guard before a `text[p]` is written that way now: the element, the predicted
dispatch, the run loop, recovery's scan. It is the same idiom the BCL's own indexers use —
one comparison catching `i < 0` and `i >= length` together — and it is unfamiliar mostly
because ordinary code never needs it: `for (int i = 0; i < a.Length; i++)` is an induction
variable RyuJIT recognizes and drops the check entirely. `p` is not one: it lives across
`goto`s and is written in a dozen places.

Nothing is given up. `p` is never negative, and were it ever to be, the unsigned form refuses
where the signed one would have read out of bounds.

**And it is worth nothing measurable:** −1.5, −0.1, +0.3, −0.8, −0.0 per cent, medians of
five. The comparison it removes is perfectly predicted and off the dependency path, so an
out-of-order core absorbs it whole. Instruction count and time are different quantities, and
this is the second time today they have said different things.

Kept anyway, and the reason is not the compare: it is one fewer cold throw block and a
smaller method, in a method whose size was measured this same day to matter more through
profile-guided layout than the work inside it. That argument is not demonstrated by this
benchmark and is not pretended to be.

## How generation under more than one language version would be tested

Asked before anything conditional exists, which is the right time. Four layers, and the
first was a hole.

**The floor, everywhere.** `EmittedCode.Compile` is what every snapshot and every emitter
test compiles through, and it parsed with `CSharpParseOptions.Default` — the newest version
Roslyn offers. So a feature the emitter started using would have passed all of them, and the
floor was checked by `DotGram.Compatibility` on one framework and by nothing else. It parses
at C# 8 now. The whole suite still passes, which is the first evidence that the floor claim
was true rather than merely stated.

**One configuration per emitted form, not per language version.** If emission branches once —
"C# 11 or later, otherwise this" — that is two forms, and two is the size of the matrix
however many versions exist between them. A `[Theory]` over the branch points, driving the
generator with `CSharpParseOptions` at each, is the shape; `GeneratorDriverTests` already
builds and runs a parser exactly this way and only hardcodes `Preview`.

**The assertion is behaviour, not text.** The rule those forms live under is method bodies
only — nothing a consumer observes may vary — so the test that matters runs the same inputs
through both compilations and compares what comes back: accepted or refused, the same value,
the same message. Asserting the shape of the emitted text would check the wrong thing and
break on every rewording.

**`DotGram.Compatibility` unchanged.** A real SDK build with a real restore, the floor form
end to end, on the framework where `System.Memory` has to be added by hand. Unit tests can be
wrong about what a build does; that project cannot.

**Snapshots stay single**, at the floor. One file per grammar. Doubling them per form would
double the diff every codegen change produces, to check something the layer above checks
better.

## Found: one publication that cannot lower costs every sibling its flat path

`Minimal.gram` gained `C = "http" | "https" | "ftp"` — a choice of literals, written
shortest-prefix-first on purpose. It compiles the way it should: the two texts beginning `h`
are not told apart by one character, so the choice is not predictive, so it is not silent,
so it needs the arena and a way back. On input `https` the run matches `"http"`, the
whole-input check refuses, and the parse resumes at the `Choice` entry and takes `"https"`.

**What was not expected is that `A` and `B` went with it.** Lowering is decided for the
whole grammar — `graph.Publications.All(machine.CanLower(...))` — so one publication that
cannot lower puts every other one through the shared engine. `A = "a"` and `B = "abcd"` are
each perfectly lowerable and nothing calls them from the automaton; they are separate entry
points that never meet `C`. They are compiled as arena states anyway, and each pays what
`CallCost.cs` measures: about 25% for going through the arena at all, and the whole engine's
machinery emitted into the file besides.

This is not the case that was investigated and rejected. "Mixed lowering: investigated, has
no target" above is about a *rule* inside a grammar compiling flat and being **called from**
the shared automaton — the two coexisting, with a plain call across the seam. What this is
about is *sibling publications* that never call each other at all. Nothing has to cross: `A`
gets a flat method, `C` gets the engine, and neither knows the other exists.

The condition that makes it safe is narrower than "silent", and it is checkable: a
publication may lower on its own when its rule is not reachable from any publication that
cannot. Where it is reachable from one, the rule is in the engine anyway and a flat copy
would be a second copy — which is the trade the earlier investigation was about, and can
stay rejected.

Not built. Worth doing when a grammar with several entry points, most of them simple, is a
shape anyone has — a DSL with `parse Document` beside `parse Identifier` is exactly that,
and `Minimal.gram` is now a small one.

## Decided: one machine per published rule

`parse A` and `find A` share one — the same rule, two entry states. `parse A` and `parse B`
do not, even where both call `Shared`: `Shared` is compiled into each. And `parse A with (…)`
twice is two, which needs no separate rule because `with` clones what it reaches, so what is
published is `A` and `A_With1` — two rules by the time the emitter sees them.

Today there is one machine per *file*, and no recorded reason for it. The rationale under
"Shared automaton and recursion" is entirely about sharing — "frequently called rules remain
shared instead of being expanded at every call site" — and that describes a minority of
rules: `CanInline`'s own comment says a rule with no value, no capture inside it and no
recursion is "a rule only in the source text", and it is expanded at every call site
already. What stays shared is only what keeps an arena frame.

So a calculator published twice, `parse Expr as English` beside `parse Expr with (Point =
Comma) as European`, compiles to six rule entries — `Expr`, `Term`, `Number` and
`Expr_With1`, `Term_With1`, `Number_With1` — in **one** 56-state automaton. Two parsers with
nothing in common, fused because the emitter's unit is the file.

**What it costs, measured on this repository's own grammars.** `Url.gram` does not change:
`parse Url` and `find Url as AllUrls` are one rule. `Minimal.gram` gains — `A` and `B`
become flat methods and only `C` needs an engine. `Feed.gram` pays: `parse Feed`, `find
Name` and `find Row as AllRows` are three rules, and `Row` reaches nearly everything `Feed`
does, so 64 states become 61 + 2 + 63 = 126. Roughly double, and the largest method barely
shrinks.

That is consistent with what this project already spends — `CanInline` calls code size "what
this project spends freely" — but it is real, and the entry below is what would earn it back.

## Later: cutting a large rule out into a machine of its own

Raised against exactly the `Feed` case above. `Feed = Header & Row* & Trailer` splits into
parts that do not overlap, and each could be its own machine with `Feed`'s reduced to a
coordinator that calls them.

This is the shape "Mixed lowering: investigated, has no target" rejected, and it was rejected
for having no case. `Feed` under the split above is a case.

**The condition that makes a seam safe is one this repository already computes.** Backtracking
cannot cross it: once `Row`'s machine has answered, its arena is gone, so a later failure in
`Trailer` has nothing to resume into. What has to hold is that nothing handed over would ever
be given back — which is what `Retention.StreamedParse` decides for a window, and decides by
construction rather than by promise, since §8.2 makes a `recover`-marked repetition possessive
and "an element it took was either read or explicitly rejected and there is no shorter reading
to come back for".

So the same predicate, asked of a seam between machines instead of a seam between the parse
and a window. A rule that qualifies can be cut out; one that does not stays inside.

Not started. Worth doing after the per-publication split, because it is what makes that split
pay on a grammar like `Feed` rather than only on one like `Minimal`.

## Built: one machine per published rule

`CSharpEmitter` built one `Machine` over the whole file. It builds one per published rule
now — `parse R` and `find R` share theirs, two publications of different rules get one each,
and `parse R with (…)` twice needs no case of its own since what is published is two rules
by then.

What it changes, on this repository's own snapshots:

| grammar | before | after |
| --- | --- | --- |
| `Url` | one engine | **unchanged** — `parse Url` and `find Url` are one rule |
| `Csv` | one engine | unchanged |
| `Minimal` | one engine, three publications | **`A` and `B` are flat methods; only `C` has an engine** |
| `Feed` | one engine, 2,175 lines | three engines, 3,014 lines |

`Minimal` is the point: `A = "a"` and `B = "abcd"` are each lowerable and were paying for the
arena because `C = "http" | "https" | "ftp"` in the same file is not. `Feed` is the price,
and the entry above about cutting a large rule out into a machine of its own is what would
earn it back.

**Seven couplings had to come apart, and each was the same shape** — something written for a
grammar's one machine that is now several. Recorded because the next person to split
something in this emitter will meet them again.

**The rules a machine compiles** are what its published rule reaches, and reachability has to
include the rule's `trivia`: a `parse` compiles the body wrapped in it, so what the trivia
calls belongs to that machine as much as the body does. `Retention`'s own `Reachable` does
not do this and could not be reused.

**Four names carry a tag** — the engine, the guard methods, the expected arrays and the
materializer — and the tag is empty where there is only one machine, so a single-publication
grammar is compiled to exactly the names it always was.

**`_ruleIds` and the state numbering are per machine**, so anything asking a machine about a
rule it does not have throws. Three loops over `_graph.Rules` inside `Machine` had to become
loops over its own: `RenderEngine`'s `hasValues`, and two in the materializer.

**The value tables are the file's, not a machine's.** A machine names a type by where it sits
in a list and the parser holds one table per entry — one parser for the file. So the order is
the union, computed once every machine exists and handed back to each before a line is
rendered (`Machine.ShareValueTables`).

**The streamed-rule set is a machine's, not the file's.** Shared, a machine tried to write a
wrapper for a rule that belongs to another and does not exist in it. The probe dictionaries
are the opposite: they stay the file's, because the streaming entry points read them later —
but each machine may only *emit* the probes it added, which is why it keeps its own list
beside them.

**The failure struct, the recovery factories, the parser runtime and the extras are the
file's.** Three of those were written `if (machine is not null)`, which silently became "if
there is exactly one" and emitted nothing at all for a grammar with two.

**`CanLower` is asked per machine**, which is the change that makes `Minimal` lower at all.
`HasTypedGuards` moved with it: the incremental materializer it turns on is machinery a
machine whose rules never read a value mid-parse has no use for.

## Measured: what a literal of each length actually compiles to

The entry above said `SequenceEqual` against a constant becomes "a single 64-bit `cmp`",
which is true of `"abcd"` and is not the reason it wins. Four lengths, disassembled at
FullOpts, say what the reason is.

**Four characters** — eight bytes, one word:

```asm
mov  rcx, 0x64006300620061       ; "abcd"
cmp  qword ptr [rax], rcx
jne  → fail
```

**Five** — ten bytes, so a word and a half, and the halves are not compared one after the
other:

```asm
mov   rcx, 0x70007400740068      ; "http"
xor   rcx, qword ptr [rax]       ; xor, not cmp — it accumulates the difference
movzx rax, word ptr [rax+0x08]
xor   eax, 115                   ; 's'
or    rax, rcx                   ; both differences in one value
jne   → fail                     ; one branch for the whole literal
```

**Three** — the same shape a size down: a 32-bit load for `"ft"`, a 16-bit one for `'p'`,
`xor`, `or`, one branch.

**Seven** — fourteen bytes, and this is the one worth seeing:

```asm
mov rcx, 0x64006300620061        ; "abcd", characters 0..3
xor rcx, qword ptr [rax]
mov rdx, 0x67006600650064        ; "defg", characters 3..6
xor rdx, qword ptr [rax+0x06]    ; offset 6 bytes — character 3 again
or / jne
```

**Overlapping loads.** `'d'` is compared twice, because two eight-byte reads that overlap are
cheaper than eight plus four plus two, and cheaper than the branch that choosing between them
would need.

So the win is not that the comparison is one instruction. It is that **the number of branches
does not grow with the length**: one, whatever the literal is. The chain this replaced had a
load, a comparison and a branch per character — every one of them a place the predictor can
be wrong — and, as the entry above found, a bounds check per character on top, where the span
form has one for the whole slice.

Worth knowing where this stops. `SequenceEqual` is folded this way only against a constant the
JIT can see, which is why the emitted call passes the literal directly rather than through
anything of ours, and why an ignore-case literal — which compares each character folded — has
to stay a chain.

## Found: `Trace` is load-bearing, and what it is holding up is a bug

`Trace` is a nice-to-have. It may sit beside the generated code; it may not shape it. No jump
exists for its sake and no logic is complicated for it, and if it ever gets in the way it goes
rather than the thing it is in the way of. Stating that plainly, because the emitter had
quietly stopped obeying it.

Every rule's entry state is two lines:

```csharp
S5: { Trace("enter Trailer", 5, p, entries.Count); goto S42; }
```

`Machine.Layout`'s `JumpOnly` collapses a state whose body is **one** statement and that
statement a jump — so this one is not collapsed, and every rule pays a state, a dispatch case
and a block for it. The trace is the only reason. It is `[Conditional]`, so it costs nothing
at run time and everything in structure, which is exactly backwards.

It is redundant besides. The call site already says it, one line earlier:

```csharp
Trace("call Trailer", 5, p, entries.Count);
goto S5;
```

The only entry a call site does not announce is the root, which is not reached from one — and
that is now traced once at the top of the method instead, which the commit before this one
did while taking out the `goto Dispatch;` that stood above `Dispatch:`.

**So the trace came out, and 191 tests failed.** Four of them were assertions that used
`"enter R"` as a marker for "this rule kept a shared block", which is fair and would have been
rewritten to use `"call R"` — that marker is more accurate anyway, since it appears exactly
when the rule is not inlined.

The other 187 were parses going wrong. `ArgumentNullException` inside a construction, from a
capture that came back null, in `Materialize` — recognition succeeded and the arena did not
hold what materialization expected. The generated code reads correctly: the dispatch points
past the collapsed stubs (`case 3: goto S29;`), call sites jump straight to the bodies, and
`_roots` is walked through `Resolved` so the layout pass knows about the collapse.

**So collapsing a rule's entry state is unsafe for a reason not yet found, and the trace has
been hiding it.** Not "the trace is needed" — the collapse is wrong, and until it is right the
trace is the only thing making the emitter accidentally correct.

Reverted, and left here rather than in a branch because the next step is to find that reason
and not to try the removal again. Two things worth knowing before starting: it shows up on
grammars with binding powers, where `Register` compiles a second body for the whole-input
entry after the constructor has run; and the failure is in materialization rather than in
recognition, so what to look at is what the arena records against a state, not what the
automaton does with one.

## Built: states written in the order the parse runs them

Raised from reading the generated file: it looks as though rules were added to the front of
the machine rather than the back, so the rule everything starts at sits at the end and the
parse jumps to the far end and walks backwards.

The mechanism is not an insertion at the front, but the effect is exactly that.
`_order` was ascending index order, and indices come from `Reserve` in the order things are
*compiled* — which is continuation-passing: what a state jumps to is compiled before the
state that jumps to it. So a sequence's last part is numbered before its first, and ascending
index order is very nearly reverse execution order.

Two things follow from it, and the second is the one that costs. It reads backwards. And a
state's trailing jump almost never names the state written next — which is the one case
`RenderStates` can drop the jump for, so the peephole that has been there all along almost
never fired.

`_order` is now built by following each chain to its end before starting another: the state a
body ends by jumping to is where the chain goes on, and everything else it can reach waits.
**Url loses 526 of its 1,826 jumps** — 1,300 left — and 526 lines with them.

The labels had to follow. The engine names every written state from its dispatch, so nothing
there can be orphaned; a lowered recognizer has no dispatch, and a state now reached only by
falling into it from the one above is a label C# warns about and a consumer's build may
refuse. `RenderStates` works out which labels survive the jumps it is about to drop, and
writes only those.

**It is not faster.** Two runs of five medians: +1.8/−0.9, +2.2/−0.0, −0.2/−1.1, +1.0/+0.2,
+2.7/+1.5 per cent. Only the long path is positive twice and even that is inside what this
machine moves by. Kept for the 526 jumps and the 526 lines, and because the file now reads in
the order it runs — which is the third criterion and would not have been enough on its own,
except that the first two are untouched.

**The state numbers still count backwards**, since the numbering is the compilation order and
only the layout changed. Renumbering them by position would make the file read `S1, S2, S3`
and is a separate change: it has to rewrite the dispatch's cases, every `goto` and every state
literal inside a `ParserEntry` — which is the field the entry above found being rewritten
where it should not have been.

## Built: the second test of a choice reads what the first one settled

Two states in the smallest grammar that has a choice of literals looked wrong, and were:

```csharp
S8: if (!(c == 'h')) goto S6;              // past here, c is 'h'
    if (!(c == 'h' || c == 'f')) goto S7;  // which makes this one unable to fire

S6: if (!(c == 'h')) goto S4;              // past here, c is 'h'
    if (!(c == 'f')) goto S5;              // which makes this one always fire
```

A choice writes two tests and they are about different things: the first says this
alternative cannot begin here, so skip it; the second says none of the ones after it can, so
no way back is needed. Both were measured and both earn their place. What neither did was
read the other: the second is evaluated knowing the first did not fire, and its set was not
narrowed by that.

Narrowed now, with the two operations `FirstSets.First` already had rather than a set
difference it does not:

- **the later alternatives cover this one** (`after.Covers(mine)`) — the test cannot fire,
  and is not written;
- **they share nothing** (`!mine.Overlaps(after)`) — it always fires, so the jump is written
  flat;
- **some of each** — asked as it stands, which is not minimal and is correct.

The URL grammar is where this was costing: `IPv6`'s nine alternatives each had one of these,
and each was a disjunction of forty-odd ranges that **was evaluated at run time and could
never be true**. 225,120 characters to 218,727 for nine fewer lines, which is what taking out
six tests of a thousand characters each looks like.

## Built: a choice's skip lands past the links that cannot say anything new

`S6` repeats `S8` — a double jump, in the smallest grammar that has a choice of literals:

```csharp
S8: if (!(c == 'h')) goto S6;   // reached S6 only because c is not 'h'
S6: if (!(c == 'h')) goto S4;   // and asks it again, with the answer already known
```

A choice is a chain and each link opens by asking whether its own alternative can begin
here. Arriving because the link before said no is arriving with that question answered,
wherever this link's set sits inside the previous one's — and `"http"` and `"https"` both
begin with `h`, so the second link asked about `h` having been reached only when there was
none. Two states and two reads of the same character to arrive where one jump goes now.

The skip is followed to the first link that could say something new (`Skipped`, walking a
map of each link's set and where it goes). The way back written beside it is untouched: a
resume point is reached with nothing known, so the alternative it names has to ask.

+1.3, +1.1, +1.7, +0.8 and +0.9 per cent on the RFC grammar — small, and the same sign on
all five, which at this size is what makes it a number rather than noise.

## Measured: what the whole of this series was worth

The five changes since the emitter's structure was first looked at, against the snapshot
grammar rather than the benchmark's — the benchmark's is 119 states and has no `IPv6`, so
most of this could not show there.

| | states | jumps | lines |
| --- | --: | --: | --: |
| before | 1,388 | 3,501 | 18,812 |
| after | **725** | **1,291** | **9,865** |

Two runs, alternating, medians of five: **+10.2/+10.1, +16.6/+14.5, +7.7/+8.6, +11.0/+9.6,
+6.0/+5.6 per cent.** Around a tenth, on every input.

**Where it comes from, isolated.** The layout change alone — same 725 states, 526 fewer jumps
— is +1.1, +2.2, +4.7, +3.9, −0.1: about a third of the total, and worth naming because the
same change measured on the benchmark's small grammar was flat. Execution order helps when
there is enough method for it to matter. The rest is the method being half the size, which is
the collapse of the rule entry states, and the dead tests that were being evaluated.

**And the benchmark barely moved**, which is the honest other half: 2.23 → 2.26/2.32 against
the compiled pattern, inside the run-to-run spread. Its grammar has one publication, 119
states and no nine-alternative rule. A change that halves a large method does nothing
visible to a small one.

## Built: a choice of literals is silent, and lowers

From reading `P:\OldProjects\Roc` — an earlier attempt at these ideas, whose macro
(`Macros/BnfMacro.n`, `OptimizeRules`) folded rules before generating: it inlined references,
flattened groups, concatenated adjacent literals, multiplied quantifiers, and merged the
character ranges of an alternation into single ranges. Asked whether the folding of
`http | https` into `http{s}` was ever here.

**Most of that list is here already.** Inlining is `CanInline`; the flattening and quantifier
work is the normalizer's; and range merging is done — `A = 'a' | 'b' | 'c'` compiles to no
states at all and one character test. The prefix folding is not in `Roc` either, as far as
its source shows: what it merges is adjacent literals in a *sequence* and ranges in an
alternation, not the common prefix of two strings.

**But the question found a real gap.** A shared prefix is handled here, by
`CompileLiterals` and `PrefixSettled` — which is what `http{s}` would have bought: the
prefix is tested once and the texts part where they differ. And it writes nothing to the
arena. `Silent` did not know: its case for a choice asked `Predictive`, and two literals
beginning with the same character are never predictive, so a construction that needs no
arena took the whole grammar into one.

`Silent` asks `LiteralRun` now — the same test `CompileChoice` uses to decide whether to
compile a run at all, which admits one only where every pair in it is `PrefixSettled`. So:

| | |
| --- | --- |
| `"https" \| "http" \| "ftp"` | **lowered** — 211 lines where it was 664 |
| `"ab" \| "ac"` | **lowered** |
| `"http" \| "https" \| "ftp"` | the arena, and rightly: shorter first, so the shorter takes the position wherever the longer would and there is no telling which was meant |

The order matters and that is not an accident of the implementation — it is the difference
`PrefixSettled` exists to decide, and the grammar author chooses it by writing the
alternatives in an order.

Two tests used `"ab" | "a"` as their example of a choice needing the arena. It no longer is
one, which is the improvement; they say `"a" | "ab"` now, and their comments say what
actually makes the difference.

## Later: the resume edge of a choice knows something too, and one thing stops it

The skip edge of a choice link was taught to land past the links whose question it had
already answered. The comment written beside it said the way back needed no such thing
because "it is a resume point, reached with nothing known". That is false, and the code says
so if read:

```csharp
S8: if (p < len) { c = text[p]; if (!(c == 'h')) goto S4; }   // reached below only when c is 'h'
    entries.Add(Choice, 6, …);                                 // or when there is no character
S6: if (p < len) { c = text[p]; if (!(c == 'h')) goto S4;      // which can never fire
                                goto S5; }
```

`S6` is reached only by resuming the entry `S8` pushed, and `S8` pushes it only on the path
its own test let through. So `c` is in `S8`'s set there, and `S6` asks a question with a known
answer — `goto S4` inside it cannot be taken.

The symmetric fix would be to push the way back at `S5` rather than `S6`, past the test,
which would leave `S6` unreachable for the layout pass to drop.

**What stops it is the other way in.** The entry is pushed at the end of the input as well,
where no test ran — and there the link behaves differently: it pushes its own way back and
falls through to its first alternative, both of which fail on length and both of which name
what they expected. Skipping the link would take that path out. Nothing about what the parse
*accepts* would change; what `Match.Error` says would.

So it is a real optimization with a diagnostic cost, and the two have to be weighed rather
than one assumed. Not built. Whoever picks it up should start by deciding whether the
end-of-input path through a choice link contributes anything to `expected` that the links it
skips do not already say.

## Later: a written "longest" on a choice of literals

Ordered choice stays, and the reasoning is §11's own: the first alternative that succeeds is
a decision the author made and wrote down, while the longest is one the machine makes and the
author learns about from the result. For a language whose posture is that the grammar says
what it means and the generator does not guess, the longest would be the generator guessing.
It is also local — what `A | B` means does not depend on how far `B` could have gone — and
locality is what makes a grammar something anyone can reason about a piece at a time.

But one case is frequent, honest, and expressed today by a convention rather than by
syntax: a keyword against an identifier, `if` inside `iffy`. The author has to remember to
write the alternatives longest-first, and forgetting is a parse that silently reads less than
it should. Making people remember is exactly what a notation should take off them.

So: a marker that says **longest here**, and only on a choice of literals.

That restriction is not a compromise, it is the case. A set of literal alternatives asked for
the longest is decided by comparing from the longest down — the first that matches is the
longest, and nothing after it is looked at. No way back, nothing written to the arena, and
`PrefixSettled` stops being needed at all: pairwise settledness exists to decide whether the
order the author wrote can be trusted, and under the marker the order is not what is being
trusted. It is `CompileLiterals` with one condition dropped.

For alternatives in general it is a different feature and a much larger one. The longest
means running every alternative and comparing, not stopping at the first that works, which
in the arena is a control shape that does not exist yet: run, remember how far, unwind, run
the next, come back to the best. And it needs a decision the literal case does not raise —
whether "longest" means longest here and then committed, which is an atomic group and changes
what a failure reports, or longest among the readings that let the whole parse succeed, which
is not expressible without enumerating them. Out of scope, and the marker's documentation
should say the restriction rather than leave it to be discovered.

**This is also what closes the `http{s}` question.** Folding `X | Xy` into `X y?` is safe
exactly where the following cannot begin with `y` — which is `PrefixSettled`, so where the
fold is safe the result is already had, and where it is not the fold changes what the grammar
means. Under the marker the fold is unnecessary from the other side: order is not significant
there, and comparing longest-down gives the same answer without rewriting anything.

## Built: a choice of literals compares each character once, backtracking included

The waste, visible in the smallest grammar that has one: `"http"` matched, the parse went on,
the input ended before the rule did, and the way back compared `"https"` **from the first
character** — four characters compared a second time to discover a fifth.

`CompileLiterals` already knew how to avoid that within a run: share the prefix, test only
the tails. It never reached this case, because `LiteralRun` refuses a run holding a pair
where one text begins another, and the choice fell to the general machinery where each
alternative is compared whole and independently. The mechanism existed; the case did not get
to it.

**Two things were needed and both are small.**

`LiteralGroup` is the wider question the compiler needs — whether the texts can be compared
together — against `LiteralRun`, which asks whether they need no way back at all and is what
`Silent` reads. A later alternative continuing an earlier one is admitted by the first and
refused by the second, which is exactly right: it is compiled *with* a way back. The other
direction stays with `PrefixSettled`, because whether an earlier, longer alternative needs
coming back to a shorter one written after it depends on what follows the choice.

And the way back is pushed **after the position has moved**, which is the whole of it. An
arena entry records `p` as it stands, so what resumes there resumes past the characters the
shorter alternative matched, and the continuation compares only its own remainder.

`C = "http" | "https" | "ftp"` was five states with two dispatchers re-reading the same
character; it is two:

```csharp
S4: if (SequenceEqual(text.Slice(p, 4), "http")) { p += 4; push Choice → S5; take it; }
    if (SequenceEqual(text.Slice(p, 3), "ftp"))  { p += 3; take it; }
    fail

S5: if (text[p] == 's') { p += 1; take "https"; }
    fail
```

`"https"` no longer appears in the falling-through chain either: it begins with a text tested
above it, so it cannot match where that one did not, and it is reached by the way back
instead.

Checked against what it must not change: `parse` of `https` still answers `https`, `find`
still answers `http` (§11), and `("http" | "https") & "s"` still matches `httpss` — which is
the case that exercises the way back, since `"http"` and `"s"` leave a character over and the
parse has to come back for `"https"` to spend it.

**Not a speed change on any grammar here.** `Url`, `Feed` and `Csv` are byte for byte what
they were: their literal choices were already settled runs, which is where
`CompileLiterals` was already doing this. It shows only where a text begins another, and
what it saves there is the second reading of the shared characters — which no benchmark in
this repository has, and which any keyword set does.

## Measured: a trie over a choice of literals is slower, and why

The obvious next step from the previous entry was to stop settling for pairs and build the
whole run into a trie — read one character, jump to the alternatives that begin with it,
and never read a character twice. `"http" | "https"` folds to `http{s}`; the operator set
in `FilterExample`,

```
Op : @string = text: (">=" | "<=" | "<>" | "!=" | "=" | ">" | "<") => @(text)
```

folds to one question about the first character and at most one about the second, where
today it is four two-character comparisons in written order and then a range test for the
three one-character alternatives.

Measured before writing any of it, against what the generator emits today, 100,000
operators, medians of nine, alternating:

| input | chain (today) | trie, `switch` | trie, `if`/`else` |
|---|---|---|---|
| the seven mixed at random | **5.4 ns** | 9.6 ns (−78%) | 8.7 ns (−59%) |
| 90% `=` — the chain's *worst* path | **3.1 ns** | 3.9 ns (−26%) | 3.2 ns (−3%) |
| 90% `>=` — the chain's *best* path | 3.9 ns | **3.2 ns** (+18%) | 3.4 ns (+12%) |

The trie does strictly less work and loses badly. What it does is convert a column of
independent, almost-always-not-taken comparisons — which the processor predicts perfectly
and issues in parallel — into a branch that *is* the decision, and therefore cannot be
predicted at all. One mispredict costs more than the four comparisons it removed. The
`switch` form is worse again because a sparse jump table (`!` is 33, `<>=` are 60-62) is an
indirect branch, but the `if`/`else` form loses too, so the jump table is not the cause.

The one row the trie wins is the one where the input is predictable *and* the chain is
already answering on its first test — which is to say, where nothing was wrong.

Left-factoring a single pair — the narrow version, which is all `PrefixSettled` would need
to also admit "longer written first" — was measured the same way on `"https" | "http"`:

| input | today | left-factored |
|---|---|---|
| the two evenly split | 5.7 ns | 5.8 ns (−2%) |
| 90% `https` | 2.80 ns | 2.77 ns (+1%) |
| 90% `http` | 3.14 ns | **2.68 ns** (+15%) |

A wash, except where the shorter reading dominates and today's code pays a failed
five-character comparison before it. That is a few percent of a construct that fires once
per input, on grammars that mostly do not have it.

**Neither is worth building.** The measurement is of the recognition shape alone and does
not include arena traffic, but the arena is a wash between the two shapes as well: both
push exactly one way back on the branch that has one.

Worth keeping as a general result rather than a fact about literals: **an optimization
that removes predictable work and adds an unpredictable branch is a loss**, and this
engine's straight-line chains are full of predictable work. The same caution applies to
anything else here shaped like a dispatch table.

## Built: the dispatch has a case only where the parse can arrive

Every generated machine was swept for redundancy — a `goto` at the state written next, a
state whose whole body is one jump, a label nothing names, two states with the same body.
Across all 42 generated parsers the first three came back **zero**, which is what the
layout pass was built to do and evidence that it still does it.

The fourth turned up something else. The dispatch was written with a case for every state
in the table:

```csharp
for (var i = 0; i < _states.Count; i++)
    if (Written(Resolved(i + First)))
        file.Line($"case {i + First}: goto {Label(Resolved(i + First))};");
```

But a state is *arrived at* through the dispatch only when an arena entry names it, and the
only other way in is from outside the table. Every other state is reached by falling into it
or by a `goto` written inside the method, and a case for one of those is a slot in the jump
table and a jump stub that nothing can execute. Measured on the output rather than reasoned
about: **4,612 of 5,591 cases — 82% — could not be reached by any parse.** `Url` had 735 and
needed 58.

Two things were needed to say which:

- the reachability walk read the second field of *every* `ParserEntry` as a state, though in
  `Capture`, `RuleCapture`, `Construct` and `Recovery` it is a slot or a factory. It is the
  same confusion the `Resumable` whitelist was written for, in the one place that had not
  been taught it, and it was keeping alive whatever state happened to share a slot's number;
- the labels. The engine wrote every one of them, excused by "its dispatch has a case for
  every written state" — which is exactly the premise being removed. It now works out which
  labels are named the same way a lowered recognizer always has, plus the ones the cases name.

`Repeat` and `Atomic` carry a nesting count in that field rather than a state, always 0, and
0 is `Return`'s case. A grammar that publishes nothing has no roots at all — the emitter
writes a recognizer for each of its rules instead — and there the dispatch keeps every state,
which is what `PlanLayout` already does for the same reason and for the same grammar.

| | before | after |
|---|---|---|
| dispatch cases, all 42 parsers | 5,591 | 1,084 |
| of those, unreachable | 4,612 | 105 |
| `Url` snapshot | 9,865 lines, 747 cases | 8,838 lines, 70 cases |
| `Feed` snapshot | 2,824 lines, 118 cases | 2,685 lines, 36 cases |

Checked the other way as well, which is the direction that matters: every state named by an
arena entry, and every state a wrapper or probe passes in, still has a case — verified over
the emitted text of all 42, not over the analysis that produced it.

**Not a speed change.** Measured against `HEAD` round-robin, with and without `TieredPGO`,
the five URL inputs moved both ways and the runs did not agree with each other — the layout
lottery this repository has already been bitten by. No number is published because none
held. What it buys is 1,173 lines out of the checked-in snapshots and a smaller method
handed to the JIT, whose budgets `PlanLayout`'s own remarks are about.

### Still open: states with the same body

The sweep also found 253 groups of states whose bodies are textually identical — 144 of them
in `Url` alone, every one of them `turn0 = p;`. They are not the same state: a body with no
trailing jump falls into whatever is written next, so two identical bodies with different
successors do different things. Whether any of them are *actually* the same — same body and
same successor — was not worked out, and merging those that are would be a size change of
the same kind as this one. Not started.

## Built: the notation reads itself, and builds a tree doing it

`examples/DotGram.Examples/GramExample.cs` is the grammar of `.gram`, written in `.gram`,
producing a tree. `ExampleTests.The_notation_reads_its_own_corpus` runs it over every
grammar in this repository — the four snapshots on disk and the text of every `[Gram]` in
the examples assembly, 28 of them — and a grammar added anywhere joins that corpus without
anyone remembering to add it.

It replaces §10's printed sketch, which had never been compiled and showed it: seven names
it never defined, no comments, `@(...)` missing from `Primary`, and separated lists written
the way the entry above this one is about.

The tree is faithful about structure — what nests inside what, in the order written — and
deliberately shallow about leaves. A literal keeps the text between its quotes rather than
a decoded value and an element set keeps its items as written, because decoding them is
`GramLexer`'s job and doing it twice would say nothing about the notation.

Two things in it are the notation being asked to carry its own weight rather than
demonstrations:

- **`wordboundary`** (§4.6) is the only reason `parse` does not match the start of a rule
  named `parseHeader`, and the only reason the order of `Declaration`'s alternatives does
  not matter.
- **`@(...)` is not recognized by the grammar at all.** Finding the parenthesis that closes
  it means knowing C#'s own strings and comments, which no grammar can do. `@CSharp` is an
  ordinary external recognizer (§7.1's second row) that reads the input itself — the seam
  that exists so that nothing ships at run time.

### Fixed: a text capture kept its start where backtracking could not reach it

Three of the twenty-eight did not survive being read, and what stopped them was not a
parse failure. `Materialize_DotGram` threw `ArgumentOutOfRangeException`: the span of a
text capture arrived with its end before its start, and `text.Slice` refused it. All three
write a binding power, and shrunk against the grammar next door the whole of it was
**eleven characters** — `A=x<<1=>@()`.

Three guesses at a small grammar with the same shape all failed to reproduce it, which is
what it cost to go on guessing. Made to say it instead — a temporary probe in the emitter,
printing the link list at the point the slice is taken — it said it in one line:

```text
slice inside out: member 0 rule Reference from=4 to=3 call=14
   at=15 kind=Capture state=50 pos=4 value=3
```

**One** entry, already inside out. Not two entries mixed up, which is what the walk above it
had been suspected of: a single capture that began at 4 and ended at 3.

A capture recorded where it began in a **method variable** and where it ended in the arena:

```csharp
writer.Line($"capture{slot} = p;");                                    // the opening
atClose.Line($"entries.Add(new ParserEntry(ParserEntry.Capture, {slot}, capture{slot}, …, p));");
```

A variable is right for exactly as long as nothing opens the same capture between the two.
A rule that can reach itself does, and the half a variable can never get right is the
*failed* inner attempt: backtracking restores the arena and nothing else, so the start
written at position 4 outlives the parse giving 4 back. `Reference` reaches itself through
`TypeArgs` → `Type` → `Reference`, the inner opening wrote 4, its `Name` failed on `<`, and
the outer closed at 3 holding the inner's start.

Now the start goes where backtracking can reach it — the arena — but only where it has to.
The opening writes the entry the close will need and marks it unfinished with `-1`; the
close finds the innermost unfinished one for its slot and fills the end in. They nest and
unwind in the same order, so the innermost is always the close's own. Everything else keeps
the variable, and `graph.Recursive` is what tells the two apart — an over-approximation, in
that the recursion need not pass through this particular capture, and the wrong way round
would be a wrong answer rather than a slower one.

**What it cost to find.** Nothing in the corpus of hand-written tests had a text capture
over a group in a rule that reaches itself, because nobody writes one on purpose. A grammar
of the notation itself does, three times over, without trying.

## Found: the exponential is a repetition giving back turns it need not

`status.md` has said for a long time that "pathological backtracking remains possible".
Measured, on the grammar of the notation itself reading a `.gram` that does not parse:

| operands in the bad rule | time to say no |
|---|---|
| 3 | 22 ms |
| 4 | 287 ms |
| 5 | 2.3 s |
| 6 | 65 s |

About twelve times per operand, and success is instant either way. A parser for a language
that takes a minute to report a syntax error is not one anybody can use, so this is not an
academic worst case.

**Where it is.** Counting what the trace reports, the hottest thing in the parse by a
factor of two is *leaving a repetition* — 70,680 of them at four operands, against 28,272
for the next thing down. Not calls, not literals: a repetition handing a turn back, the
suffix being re-read, and the turns of the repetitions above it multiplying that.

**Proved rather than argued.** Three atomic groups written into the grammar by hand —
`{ ('|' & rest: Alternative)* }` and two like it — and the table above becomes 0.0 ms at
every size, up to twelve operands. The exponential is *entirely* the give-back.

### What did not work, and why it is worth knowing

Memoizing failed calls — a rule that failed at a position cannot match there, so refuse the
second arrival without running it — was built and measured: **2.6×**, and the exponential
untouched. Two reasons, and the second is the one that matters.

- Most of the search never crosses a call. The give-back happens inside one activation.
- **This is not PEG.** In a PEG `*` is possessive and a rule has one result at a position,
  which is what makes a memo table equivalent to the parse. Here a repetition gives turns
  back, so a rule *succeeds in several ways* and is re-entered for the next one. "It failed
  here" is sound; "it matched here, and this is how" is not a single fact, and a table of
  failures alone cannot collapse a search over lengths.

It was also not sound as written: a `Call` entry inside an atomic group whose commit has
put out its ways back is taken off the arena without having completed, and reads as
exhausted when it was cut short. Reverted rather than gated, because 2.6× on a pathological
case does not pay for a branch on every call plus a condition nobody would remember.

### The cure, and the one thing in the way

`Possessive` already decides exactly this question — a repetition need never give a turn
back when the body cannot be empty, its first set is disjoint from what follows, and it
matches one way only — and it is *right* about these repetitions. It is simply never asked:
its only two callers are `SilentRepeat`, which decides whether a repetition can be lowered
to a plain loop, and `Deterministic`. A repetition with a capture in it can never be
silent, so `('|' & rest: Alternative)*` is never even a candidate, and the general
machinery writes a way back per turn regardless of what could have been proved.

So `CompileRepeat` should ask, and where the answer is yes write **one** way back instead of
one per turn: pushed with the `Repeat` entry so its index is always `repeat + 1`, its
position rewritten at the end of every turn, and put out at the exit. O(1) per turn, no new
field, no analysis that does not already exist — the same thing the atomic groups did in the
experiment, except proved instead of asserted.

Not built yet.
