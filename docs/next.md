# Engineering diary

**This file is a diary, not a description of the compiler.** It is written newest last,
each entry recording what was built or found and — the part worth keeping — *why the
alternatives were not taken*. An entry is true of the day it was written and is left alone
afterwards, because a design note that gets edited to match the code stops being a record
of the decision and becomes a worse copy of the code.

So: **nothing here is authoritative about the present.**

| For | Read |
| --- | --- |
| what the language is | [`syntax.md`](syntax.md) — a specification, present tense |
| what the compiler does today | [`status.md`](status.md) — the report |
| why it is that way | this file, and `git log` |

The sections between here and the first `## Built:` entry are the oldest of all — they
were the handoff this file began as, and they describe an engine of some ninety commits
ago. They are kept because the reasoning in them is still the reasoning, and read as
history. Where one of them names a file (`Region.cs`) or a number (887 tests) that no
longer exists, that is the point: the entry below it says what happened to it.

## Read this first

The conventions below are the one part of the old handoff that is still operational.

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

Baseline **as of the day this was written**, kept for the shape of the report rather than
for the number — `status.md` is where the current one lives: the build succeeded with no
warnings or errors, and the runner discovered 887 tests, all passing. The stray-character recovery regression is fixed: a broken
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

So `CompileRepeat` should ask, and where the answer is yes keep **one** way back alive
instead of one per turn.

### Built, measured, and taken out again — and what it ran into

Built twice, because the first shape was wrong in a way worth writing down. One way back
for the whole run, written straight after the `Repeat` entry so that its place is always
`repeat + 1`: 104 tests went red and every capture inside a repetition came back empty. A
way back **below** the turns is a way back that throws them away — coming back to it pops
everything above it, which is precisely the entries those turns recorded. It has to stand
where the last turn left off, above them.

So: written per turn as before, and the one the turn before left put out, its place kept in
the `Repeat` entry's own second field — which means nothing for that kind, and unlike a
variable survives the backtracking this is about. That is correct: 1,020 tests green, and
the emitted code does what it says.

**It buys nothing yet, and costs 4-9%.** Measured against `HEAD` on all five URL inputs,
three rounds, every one slower and all in the same direction — two extra arena writes per
turn. And the sixty-five seconds are untouched, because the repetitions that matter are not
found settled.

**The wall is `following.IsKnown`.** Asked of every repetition in the grammar of the
notation, the verdict is the same for all of the ones that cost anything:

```text
settled=False known=False nullable=False overlaps=True :: trivia & '|' & trivia & rest: Alternative
settled=False known=False nullable=False overlaps=True :: trivia & '&' & trivia & rest: Operand
settled=False known=False nullable=False overlaps=True :: trivia & ',' & trivia & rest: Argument
```

`overlaps=True` is not a finding there — an unknown continuation is "anything", which
overlaps by definition. The one fact is `known=False`. A published rule's body is compiled
with `FirstSets.First.All`, and unknown cascades down every rule it reaches, so the
structural repetitions — exactly the ones that multiply — are never even candidates. What
is settled today is the lexical layer, where the follow is known because the rule is
reached from somewhere that fixed it.

So the order is: **follow sets first**, then this. Reverted rather than left in, because a
mechanism that costs five per cent and fires nowhere is a mechanism that will be measured
once, wrongly, by whoever comes next. The shape is written down here and is half an hour to
put back once the analysis can feed it.

## Built: the exponential is dead

Sixty-five seconds to refuse a six-operand rule; now 0.0 ms, flat through fourteen. Not a
patch — five layers, each of independent worth, each proved before the next was started:

1. **First sets got real characters** — a Unicode category is its ranges, a negation its
   complement, a folded literal its foldings; ranges normalized so `Covers` is exact and
   the follow fixed point stops on content rather than spelling.
2. **Two one-line over-approximations fixed** — an optional told its body another turn
   might follow (poisoning everything upstream of `@(...)`'s honest "anything"), and a
   whole parse's body was compiled as though anything could follow it instead of the end
   of input. After both, 44 of 52 rules in the self-hosting grammar know their follow.
3. **The seam is withheld where the author's own trivia stands** — `trivia & trivia &
   trivia` was one seam whose every split the search walked.
4. **What follows is threaded as a pair** — as it stands, and past the seam. §4.5 heads
   every spaced seam with the same trivia, so a turn and the continuation behind it are
   told apart by what each reads *next*; compared plainly they overlap on the trivia
   itself and the comparison says nothing. One composition (`FollowSets.Precedes`), used
   by the fixed point and the compiler both, crossing namespaces the same way in both.
5. **A settled repetition keeps one way back** — a `LoopExit` entry valid only while it is
   the loop's latest (the `Repeat` entry's rule-index field holds where the last completed
   turn ended); stale ones are popped past, never resumed. Soundness needs only the first
   sets: an exit at a completed turn's start would have the continuation begin where the
   turn began, on a character the turn read, and disjointness says it cannot. No
   determinism demanded — that is `Possessive`'s stronger licence for silent lowering, and
   the body's own machinery stays recorded here.

The boundary found on the way matters as much as the mechanism. Trivia that can swallow
multi-character units — comments — genuinely admits readings where a failing parse re-opens
a comment's interior as syntax, one give-back at a time; §11 requires them, `Contained`
detects them, and the proof honestly declines. The author's answer is one brace pair —
`trivia = { … }` — which says "what a comment swallowed stays swallowed" and hands the
proof back. §4.5 now recommends it; the self-hosting grammar wears it.

Measured: the failure table flat at 0.0 ms through fourteen operands; the whole corpus
still reads as trees (Url.gram in 2.8 ms warm); the URL benchmark's ratios to compiled
Regex unchanged within noise, with the two-row validity gate agreeing. 1,031 tests green.

## Built: a trace worth reading, and an invariant that names its rule

`DOTGRAM_TRACE` now buys lines like these, on standard error, with nothing else to
configure:

```text
.Gram call Using in File state=6 at 0 "^A = 'x' | 'y'\npa" arena=4
.Gram stand exit in Using state=215 at 0 "^A = 'x' | 'y'\npa" arena=7
.Gram resume exit state=215 at 0 "^A = 'x' | 'y'\npa" arena=6
```

The rule name is written into each emitted `Trace` call as a literal at generation time —
knowable there and not at run time, where an engine's state numbers are shared by every
rule it inlined; an inlined body traces as the rule it lives in. The input window rides
along as a span, the caret marks the position, and `[Conditional]` removes calls and
arguments alike when the symbol is off, so the parser's structure and its speed owe the
trace nothing.

The materializer's one load-bearing assumption — a text capture's span runs forward — is
now a named check in debug builds: `capture 'text' of rule 'Name' has its end before its
start (4..3)`, instead of `ArgumentOutOfRangeException` out of generated line 34853, which
is what finding the reopened-capture bug actually cost.

## Analyzed: the notation's own grammar, by its own analyses

With first sets real and follow sets paired, the self-hosting grammar reads as a decision
structure. 25 of its 38 repetitions prove settled; each of the 13 that do not is one of
two honest things. Behind `@(...)`: the one unknowable first set, contained by design.
Or a genuine ambiguity the language means: `TypeArgs?` against `<<` (`A << 1` must fail
`<` as type arguments and come back — the hand parser spells the same fact LL(2)); the
`?=`/`?!` prefix against the `?` quantifier; a capture name against a bare reference;
keywords not being reserved, so `using` may open a rule as well as an import and only
ordered choice plus `wordboundary` decides. §10's old "two tokens of lookahead" claim
survives translation: what the hand parser buys with a second token, the notation buys
with a way back the analysis correctly refuses to remove.

Of the choices, the dispatchable ones dispatch and the rest are cheap trials in written
order. One gap worth remembering: `Int | Identifier` is disjoint — digits against letters
— but a category's six hundred ranges exceed the rendering budget, so no dispatcher is
written even though the *other* alternative's test is one range. A dispatcher that tests
the small set and falls to the large one would cover it; not built.

## Built: an executable §11, and what it caught in its first minute

`ReferenceInterpreter` is the semantics with no optimization at all: thirty lines of
recursion per construct, every reading enumerated lazily in preference order. Its worth is
being obviously right where the automaton is subtly right. `ReferenceDifferentialTests`
compiles random grammars — seeded, guard-free, recursion-free, half of them spaced — and
holds the automaton to agreement on random inputs; `Shrink` (also new, and kept this time)
cuts any disagreement to its essence. The shrinker's own test caught the classic predicate
mistake on its first run — GRAM2005 has five sites, and a predicate that names only the id
converges on whichever is cheapest to reach — which is now the example its documentation
teaches with.

The fuzzer paid for itself in its first minute, twice:

**A counted repetition counted a re-matched turn twice.** The count lives in the `Repeat`
entry and is rewritten in place, and an in-place rewrite survives backtracking that the
turn it counted does not: resume an alternative inside a completed turn, the body
re-completes, the same turn counts again, and `X{2}` reads two of a thing the input held
one of. As old as the engine — the commit this week started from accepts it — and invisible
to every hand-written test, the regex differential, the corpus, and the mutation fuzzer,
because none of them pair a bounded count with a body that can re-match. The fix follows
the arena's own philosophy: a completed turn leaves a `TurnDone` entry, and popping it is
what un-counts the turn, at the exact moment the parse abandons it.

**A parser that compiles with a warning.** `var c` was declared on the compile-time flag,
and the layout can drop every state that read it; the fuzzer found a grammar where it did,
and `CS0219` in somebody else's build is a defect here. The declaration now asks the
written bodies.

## Built: the self-hosting differential, and the honest number it produced

`SelfHostingTests` holds the two implementations of the notation to agreement — the
hand-written `GramParser` and the generated `GramGrammar`, over every `.gram` in the
snapshots — and they agree. That is the differential the self-hosting work was started
for, and it lives in the tests because the benchmarks project deliberately references the
generator as an analyzer and nothing else: the compiler's own front end is only
measurable where both sides meet.

The same test carries the cost of each side, as medians of individual parses:

| grammar | hand-written | generated | ratio |
|---|---|---|---|
| Minimal.gram | 0.012 ms | 0.068 ms | 5.5 |
| Csv.gram | 0.021 ms | 0.081 ms | 4.0 |
| Feed.gram | 0.018 ms | 0.257 ms | 14 |
| Url.gram | 0.038 ms | 2.18 ms | **57** |

The work differs in kind — the hand parser builds the compiler's tree with positions and
diagnostics, the generated one the example's records — so the ratio is a scale, not a
verdict. But fifty-seven is not a scale disagreement; it is the next hunt, on the first
genuinely realistic input the benchmarks have had: recursive, backtracking, 87 KB
allocated per parse where the URL machine allocates 264 bytes.

**The instrument itself had to be fixed first, and the lesson is worth the table.** The
first version averaged fifty parses per round and reported the generated side at 140
times the hand-written one. Individual parses told another story: 0.08 ms each, with two
spikes of 79 ms — tiered compilation re-jitting the thirty-thousand-line engine method
tens of calls in. A mean over a window holding that spike is the layout lottery again,
relearned against the JIT; medians of individual parses are what survive it. The
shrinker met the same enemy from the other side: a cost predicate sampled once let the
shrink walk out through a single lucky-fast measurement, and "slow" had to become "even
the fastest of three is slow".

## Hunted: fifty-seven times became five, and where the rest lives

The 57× on `Url.gram` fell to the instruments in an afternoon, and the kills are worth
listing in order, because the order is the method.

**What it was not.** Steps said 32,592 for the whole file at 2.5 ms — 76 ns a step against
33 on a small prefix, so half the time was not in steps at all. Individual parses said
0.08 ms with two 79 ms spikes — tiered compilation, not parsing — which killed the first
instrument. And a minified input — comments stripped, 43% of the characters — cost the
same 2.09 ms as the original, which killed the best hypothesis: the atomic trivia and its
2,444 arena entries per parse were not the bill.

**What it was: `Call | Reference`.** Every bare reference parsed `Reference` twice — once
inside the failing `Call`, once as itself — and `Reference` contains `TypeArgs?`, which
contains `Type`, which contains `Reference`. The double parse compounded through that
nesting, and references are most of what a grammar is made of: 5,974 of 32,592 steps sat
in `Reference` alone. Left-factoring one rule —

```dotgram
RefOrCall = target: Reference & (open: '(' & (Argument & (',' & Argument)*)? & ')')?
          => @(open is null ? target : Call(target, first, rest))
```

— took the parse from 2.09 ms to 0.35 ms. Sixfold, from spelling one choice the way the
hand-written parser always had (`ParseReferenceOrCall`). §11's ordered choice is not
obliged to be written with the prefix shared, and the self-hosting grammar now teaches
that too.

A settled optional whose body one character decides also stopped paying the arena — a
`Repeat` entry, a standing exit, a count and their unwinding became one comparison — which
is worth a few percent here and applies wherever the proofs reach.

| grammar | hand-written | generated | was | now |
|---|---|---|---|---|
| Csv.gram | 0.019 ms | 0.051 ms | 4.0 | **2.6** |
| Feed.gram | 0.022 ms | 0.105 ms | 14 | **4.7** |
| Minimal.gram | 0.014 ms | 0.061 ms | 5.5 | **4.3** |
| Url.gram | 0.062 ms | 0.334 ms | 57 | **5.4** |

**Where the rest lives.** 20,632 steps remain for `Url.gram`, and the shape of the residue
is arena traffic per construct — enter/leave repeat, atomic enter/commit on every seam —
where the hand parser pays a method call. Two directions are recorded rather than taken:
teaching the normalizer to left-factor shared call prefixes itself, which constructions
make hard in general and the measurement makes tempting; and a no-arena compilation for
atomic, capture-free bodies — a lexeme-scanner mode, the flat path's little sibling —
which would take the seam's cost to a hand-written skip loop's. Both are engine work with
a proved instrument to hold them to.

## Built: the scanner — no arena where nothing is remembered

A rule that wears atomic braces and keeps no records now compiles as a plain method —
checkpoints in locals, greedy loops, one `return` — and every call to it as a call:

```csharp
static int Scan_trivia(global::System.ReadOnlySpan<char> text, int pos)
```

The braces are the licence, not a hint. An atomic group commits its first reading, so a
compilation that finds the first reading and nothing else — each choice committing the
first alternative that matches, each repetition greedy — is §11 inside the braces, not an
approximation of it. `Scannable` draws the fence: choices must be mutually exclusive
(disjoint firsts, or leading literals neither of which begins the other), repetitions must
sit at the tail where greed is final, or be settled against what follows, or be the
guarded scan `(?!X & …)* & X`, which stops at the first `X` by construction. Captures
never pass — a scanner has nowhere to put one.

What it buys is the seam. Atomic trivia — §4.5's own recommendation — was applied 2,444
times per parse of `Url.gram`, each application an atomic entry, a repeat entry, their
unwinding and a commit walk. Now each is one call to a method that reads like the
`SkipWhitespaceAndComments` anyone would write by hand. Steps fell from 20,632 to 9,440.

| grammar | was (start of hunt) | after left-factoring | now |
|---|---|---|---|
| Csv.gram | 4.0 | 2.6 | **2.0** |
| Feed.gram | 14 | 4.7 | **2.9** |
| Minimal.gram | 5.5 | 4.3 | **2.7** |
| Url.gram | **57** | 5.4 | **3.1** |

The differential fuzzer now generates atomic comment-bearing trivia in a quarter of its
grammars, so the scanner compilation sits under the same oracle as everything else. The
snapshots did not change: no grammar in them has an atomic record-free rule, which is
itself the honest note — this optimization is §4.5's advice paying for itself, and a
grammar that ignores the advice keeps the machinery it asked for.

## Design: the lexical layer — tokens without a token in sight

Goal, per the standing target: a generated parser's decisions should cost what a
hand-written parser's cost, or less. The hand-written one owes most of its speed to a
lexer — the input linearized once, ~5x fewer items to decide over, decisions by a dense
switch on a token kind. The generated engine re-derives "what stands here" structurally
at every decision point: 27 recorded steps per token where the hand parser spends 3-5.
This design closes that gap. It is a design, not work done.

### Where tokens come from: the boundary that is already written

No new syntax. The language already draws the line §4.5 recommends drawing: lexical
rules live in a namespace with `trivia = none`, syntactic rules live where trivia is
non-empty. So:

- a **token rule** is a rule in a no-trivia namespace referenced from a spaced one —
  `Identifier`, `Int`, `Char`, `String` in the grammar of the notation;
- an **anonymous token** is a literal written directly in a spaced rule — `'&'`, `"=>"`,
  `"namespace"`;
- **trivia** is already the skip, and already compiles as a scanner.

A grammar with no such boundary — `Url.gram`, every regex-like grammar, everything with
`trivia = none` throughout — has no lexical layer, generates exactly what it generates
today, and pays exactly nothing. That is the answer to the first question asked of this
design: no memory appears anywhere, because the layer does not exist where it buys
nothing.

### What a token is at run time: three locals

The hand-written lexer builds a `List<Token>` for the whole input up front. This design
deliberately does not. A token at run time is a *kind* and an *end position* in locals —
no array, no buffer, no runtime type, no allocation for any grammar, with or without the
layer.

Token-at-a-time works because of what this week built. A decision point that qualifies
compiles as a **fused dispatch**: one call to `Scan_trivia`, one read of the token at
`p` (a switch on the first character routing to the candidate token scanners), the kind
into a local, then a switch over dense small integers — the jump table that
character-level dispatch can never have. On the straight-line path every token is
scanned exactly once, which is all the hand lexer's array achieves; the array would only
pay under heavy re-visiting, and the settled-repetition proofs have already made
re-visiting rare. Where backtracking does revisit, the token is re-scanned — positions
are the only state, which is the arena's own philosophy. The hand lexer's list is the
part we skip: «одинаков или лучше» — this is the «лучше».

Bufferless is also what makes degradation graceful. There is no token stream to fall out
of sync with, so a single grammar may freely mix token dispatch where the proofs hold
and today's character machinery where they do not — down to individual decision points.
And it is what keeps `find` and streaming untouched: nothing is tokenized ahead of the
position.

### Semantics: proof, not mode

Tokenization classically changes a language — maximal munch, priority order. This design
changes nothing: a decision point compiles as fused dispatch only where the analyses
prove the dispatch equal to §11, and stays as today everywhere else. The proofs are this
week's, composed: token rules must be `Scannable`-grade (atomic or deterministic — the
commit licence), candidates at a decision point must be pairwise `Exclusive`, and
keyword-versus-identifier exclusivity is exactly what §4.6's `wordboundary` already
supplies — the boundary lookahead baked into a keyword's scanner is the proof that
`"using"` and `usingFoo` cannot both match. The unsettled thirteen — `TypeArgs?` against
`<<`, prefix against quantifier — stay character-level in v1 and become the two-token
fusion of v2.

### What gets better beyond speed

Expected-sets become token names: "expected an identifier or '('" instead of a character
class. The failure path reports over the same inventory the dispatch uses.

### Staged, each stage measured

1. **Inventory as a probe**: the boundary-crossing analysis alone, reported as emitted
   comments — how many decision points of the notation's grammar qualify, before any
   emission changes.
2. **Fused dispatch** at qualifying choice points; oracle, corpus, self-hosting table
   after each.
3. **Keyword kinds**: anonymous-token literals interned, boundary baked.
4. Targets for calling it done: Feed and Csv at or under 1.5x the hand-written parser,
   Url.gram under 2x, URL benchmark and snapshots byte-identical for layerless grammars.
5. **v2, separately argued**: two-token fusion for the LL(2) residue; the normalizer's
   auto-left-factoring feeding the same dispatch.

Rejected: the upfront token array with pooling. It allocates proportionally to input,
complicates `find` and streaming, adds a runtime type — and buys only what bufferless
already has on the straight-line path.

## Stage 1 of the lexical layer: the inventory, and what it turned up

The probe classified every choice point of the notation's grammar for token dispatch,
before touching emission. Of twelve spaced choices: three route on exclusive tokens
already (`"<<" | ">>"`, `"?=" | "?!"`, `"parse" | "find"`); one routes with a shared
pair (the quantifier's class against `'{'`); four lead through spaced rules and need the
look-through that stage 2 builds (`Declaration`'s three keywords against `Rule`'s
identifier among them); four are blocked because a token rule cannot be committed against
its follow — and the last group is not a weakness of the analysis. It is the finding.

**The two implementations accept different languages, and the inventory found it.**
`Identifier` refuses to commit because letters sit in its follow sets, and they sit there
because §11 genuinely permits what a lexer never does: on `parse Xas y`, the identifier
hands two characters back and the keyword `as` matches mid-word — §4.6's boundary guards
only what follows a keyword, not what precedes it. Verified live: the generated parser
accepts `parse Xas y` as `parse X as y`; the hand-written one refuses, because `Xas` is
one token and always was. Pinned in `SelfHostingTests` so the resolution flips a test
consciously.

So "the same process" is not reachable by proofs alone — the processes implement
different languages at exactly the points the proofs refuse. The v2 decision this forces
is now concrete rather than speculative: whether the notation's semantics at the trivia
boundary should become token semantics — maximal munch, no mid-word keywords — which is
almost certainly what every author already believes it is. That is a language decision,
§11-adjacent, and it is not taken here.

The probe itself was temporary and is gone; what stage 2 needs of it — leading-token
computation, exclusivity over tokens, scannability against a rule's follow — is specified
by what the probe measured.

## Built: §4.6 made symmetric — and made real

The ruling: `Xas` is one lexeme. §11's backtracking could read `parse Xas y` as
`parse X as y`, because the woven boundary guarded only what follows a keyword; a new
`Node.Behind` — one comparison against `text[p - 1]`, woven by the normalizer, never
written by an author — guards what precedes. The pinned divergence test flipped to an
agreement test: both implementations now refuse, for the same reason a lexer always did.

Two findings on the way, both worth their own lines:

**§4.6 had never fired for the grammar shape its own example recommends.** `Continues`
required the boundary's element to stand directly in the `wordboundary` body, and
`wordboundary = WordOrDigit` names it through a rule — so the right-edge guard had been
silently inert for the self-hosting grammar all along, and nothing said so. The decision
now looks through reference chains and answers category membership at build time with the
first-set machinery. The weave also ran eagerly against a body that might not be lowered
yet; it lowers on demand now.

**Symmetry exposed the scope.** The instant the guards actually fired, they fired inside
lexical namespaces too, and the `'u'` of `'\uFFFF'` was told it cannot precede a hex
digit. The rule that survives: a lexical namespace — one whose own trivia is empty —
shields a `wordboundary` inherited from outside, because its literals are the parts of
one lexeme; a namespace that declares boundary and empty trivia together keeps both,
which is the scannerless keyword grammar (`SqlReadOnly`), and its `into_stock` tests are
what caught the first, too-broad scoping.

And the second ruling: dots are punctuation. `Name` moved from the lexical namespace to
the spaced layer of the self-hosting grammar, so `using A . B;` reads as the hand-written
parser always read it. `A.B:C` tokenizes as five things, not three.

Symbols stay per-context — no maximal munch for them, by design: C's `a+++++b` is the
cautionary tale, where a global lexer's greed commits `a ++ ++ + b` and the parser can
never recover the valid `a++ + ++b`. Here the candidate set at each decision point is the
context, and §11's give-back remains available exactly where word-lexeme rules do not
apply. Self-hosting ratios held: 2.1/3.0/3.0/3.2.

## Built: a rule that only forwards costs nothing

Stage 2 opened with the trace, and the trace moved the target. The choice points the
inventory catalogued were not the top of the bill — the top was the pass-through tower:
`Operand : @T = o: Guard => @(o) | o: Quantified => @(o)`, a floor of the layered grammar
that does nothing but forward, and cost a call frame, a completion, a rule capture, a
pass-through construction and a return per operand. Work a hand-written parser does not
do, which under the standing rule makes it a proof obligation.

The proof is by identity, and the normalizer now discharges it: `CollapseTransparent`
inlines every call to such a rule as the choice of its sources, distributing the capture
over the branches — `e: Operand` becomes `(e: Guard | e: Quantified)` — ordered as
written, values flowing from the producers they always flowed from. The rule stays in the
graph for whatever reaches it by name; unreachable states are already the layout's to
drop. One lesson cost an hour: the rewriter must preserve the identity of untouched
subtrees, because everything before it keys facts by node reference — binding powers,
sequence captures — and a wholesale clone orphaned the calculator's operators before the
identity-preserving walk fixed it.

Steps on `Url.gram`: 9,449 → 8,844. Modest — only `Operand` and `Declaration` are fully
transparent in the notation's grammar; `Quantified` and `Prefixed` carry real factories
and remain — but the feature is general, and every layered grammar stops paying rent on
its transparent floors. Ratios: 2.1 / 2.9 / 2.8 / 3.1.

Alongside it, the `a+++++b` pair went in as tests: the guarded grammar dies the death the
C standard prescribes, the unguarded one reads `a++ + ++b` — both languages three lines
apart, neither imposed.

## Where the remaining 3x lives, measured to the entry

After the collapse, the trace of `Url.gram` reads 8,844 steps, and the shape is no longer
scattered: 907 valued-rule completions each pay the call ceremony — a `Call` entry, its
`Completed` rewrite, a `RuleCapture`, a pass through `Return`, a `Construct` — about five
arena entries apiece, roughly half of everything. That is the factory tower
(`Quantified → Prefixed → Captured → Primary → RefOrCall → Reference`, ~six valued calls
per operand), and its factories are real: they build the tree. The hand-written parser
makes the same six calls and builds the same tree; what it does not do is write five
records per call so that backtracking could unwind a construction it almost never
unwinds. The rest of the residue is honest §11: `recover` against a rule named
`recover` is ambiguous beyond any fixed lookahead, and the ways back that remain are the
ones the semantics require.

Two directions are designed, not started, in order:

**Sound eager construction.** The unsound version was removed this week for keying on a
static fact that did not imply acceptance. The sound key is dynamic and exact: at a
call's completion, if no resumable entry lives above the call's own — an O(1) question
for a counter of live ways back — then nothing can ever resume into its span, and the
value can be built on the spot and the five records collapsed to none. A way back
*below* the call may still rewind past it wholesale; that discards the built value
(`Truncate` already knows how) and re-parses, paying only on the path that was already
losing. The surgery is in the materialization protocol: a caller's capture must be able
to hold a value directly, not only a completed call's index.

**Two-token settledness.** `name: Identifier & ':'` against a bare reference is decided
by the token after the word — ':' is provably not in `Primary`'s follow — and the same
shape decides several of the remaining optionals. The fused test is a word-run scan, a
seam skip, and one character. The generic analysis is the two-token fusion the lexical
design already names as v2; the shapes are now enumerable from the trace.

## Fixed: three facts asked at the wrong scope, and a capture asked per turn

An outside review of the Minimal catalog — one snapshot holding every shape the
generator compiles — found flat rules renting parsers: `Recognize_A_Whole`, a method
whose own comment promises "no engine anywhere near it", opened with `RentParser`.
Bisecting with minimal grammars found three separate places where a fact about one
machine was asked of the whole graph:

- **The flat gate in `CSharpEmitter`** required `graph.Recoveries.Count == 0 &&
  graph.Climbing.Count == 0 && !Streaming(graph)` — graph-wide, so `Sheet`'s
  `recover` and `Sum`'s `<<` cost `A = "a"` its flat path from three sections away.
  Now `RecoversWithin`/`ClimbsWithin` ask the group's reachable subgraph, and
  streaming is asked of the group's own publications.
- **`Silent` in `Machine.Analysis`** opened with `_graph.Climbing.Count == 0 ||
  !_owners.ContainsKey(node)` — one `<<` anywhere and every node of every rule lost
  every proof, which is how `Maybe`'s settled optional two rules from the climb was
  found compiling as a `Run` loop with a give-back entry. The refusal is now the
  owning rule's: a climbing rule keeps the general machinery, everything else keeps
  its proofs.
- **A capture repeated recorded per turn.** §10 makes `t: ['a'..'z']+` one capture
  repeated, and the machine compiled it as written: `Capture` + `TurnDone` + two
  `Repeat` rewrites + a `LoopExit` refresh per character — O(n) arena for a value
  defined to be the text joined, which for contiguous turns is the extent of the
  loop. `HoistTextCaptures` in the normalizer now rewrites `(t: X)+` to `t: (X+)`
  wherever the body is pure text and at least one turn is required (an optional keeps
  the §10 null-versus-empty distinction and stays distributed; recovering repeats and
  fold rules keep their node-keyed facts and are skipped). The freed loop then
  compiles silent: `Text`'s engine body is now a capture local, a char loop with no
  entries, and one `Capture` record.

Csv arena writes 24 → 18 statically; the catalog's `A`–`F` are flat methods again;
ratios (medians, same discipline): Csv 2.1, Feed 3.7, Minimal 2.4, Url 4.0.

The review's larger direction stands on its own: the arena as fallback rather than
default form, with lowering classes (direct / scanner / predicted / checkpoint /
precedence / general) and captures decoupled from backtracking state — `[start, end)`
locals unless a proof says backtracking can change the capture's identity. That is
the valued-flat stage: `CanLower`'s `Silent` has no case for `Capture`/`Construct`,
so every valued rule still enters the engine only to run a loop the proofs already
made silent. The next stage teaches the flat writer captures-as-locals and direct
construction, which by the review's own table takes the arena out of 21 of the
catalog's 23 rules.

## Built: valued flat lowering - captures in locals, construction at Accept

The review's central principle - the arena is a fallback, not the default form of
parser state - split into two independent questions: does parsing need backtracking
state, and does the value need persistent derivation state. For most valued rules the
answer to both is no, and the machinery to prove the first half already existed. What
was missing was the second half: `CanLower` is `Silent`, and `Silent` had no case for
a capture or a construction, so every valued rule entered the engine only to run a
loop the proofs had already made silent, keep one capture, and write four arena
entries of ceremony around it.

`CanLowerValued` admits a valued publication to the flat path when the value adds
nothing the arena is for: captures are spans of the input (no rule values, whose
per-turn records are the point), none sits under a repetition of more than one turn
(a local would keep only the last), and the construction is single and at the top, so
Accept knows the factory without a record. The silence question is then `Silent`'s
own, asked with `_valuesInLocals` on - the same flag the rendering compiles under, so
analysis and compilation cannot disagree.

Under the flag, a capture is two locals - `flat{slot}Start`/`End`, sentinel start for
the §10 null-versus-empty distinction - and the construction compiles to nothing: the
factory runs once, at Accept, after the whole-input check. Deferred construction is
kept exactly, without an arena to defer into, and a new test pins the factory call
textually after the length check. A give-back door now unsets the capture locals the
abandoned turn set - the one thing arena unwinding used to do for them - which the
optional-capture test caught on its first run ("" where null was meant).

Silence itself grew three honest cases, engine-wide, not flat-specific: `Behind` (one
comparison, already routed through `_fail`), a scanner call (one method call), and a
lookahead over a silent body - compiled as a checkpoint local and a rewind through
the same `GiveBack` door a possessive turn leaves by, in both directions, replacing
the Lookahead entry and its RemoveRange wherever the body is silent. The rewind on
the failing side is what keeps "a lookahead does not report how far it looked" true,
and that test caught the first version leaving `p` mid-body.

On the Minimal catalog this makes 12 valued rules plain methods with zero arena -
Text, Number, Predicted, List, Counted, Maybe, Ahead, Not, Ci, Upper, Spaced.Pair,
and the valueless A-F alongside - `Recognize_Text_Whole` now being a char loop, one
`string` allocation, and a factory call, the review's target form for it verbatim.
The engine remains for exactly what the review's own table kept it for: C/E/F (a way
back - the checkpoint class, deferred), Committed (atomic commit), Alias, Either,
Wrapped (rule values across calls - the next stage), Sum (climbing; direct recursion
was considered and declined - a hand parser overflows the stack where the engine
refuses cleanly), Sheet (recovery), AnyItem (a find is a prefix parse, unsettled by
definition). The catalog file: 10,973 lines before the scoping fixes, 7,155 now.

Not taken up yet, in order: rule values across calls (Alias's direct call of a flat
callee, Either/Wrapped's predicted dispatch with the factory choice as a local
tag); the checkpoint class for C/E/F/Committed; Sheet's collection materialization
(the review's copy-on-return point) and pool retention.

## Built: rule values across calls - sites, instances, and a tag for the choice

The second half of valued flat lowering: a capture of another flat-valued rule's
value. `FlatValued` is the structural predicate, memoized, and it serves root and
callee alike: constructions that are the whole of the body - one at the top, or one
per alternative - over captures that are spans of the input or, now, sites of
further such rules. A site compiles the callee's body in place under an instance of
its own (`flat{instance}_{slot}` - the same rule inlined twice may not share
locals), and the capturing slot's sentinel doubles as "did this site run". The
callee must share the caller's seam: a namespace crossing degrades continuations to
"anything", which the silence proofs cannot survive, so the predicate refuses it.

At Accept the factories run inner-sites-first - a child's instance id is above its
parent's, so reverse order is dependency order - each guarded by its parent slot's
sentinel, the root last. Where a rule is a choice of constructions, which
alternative matched is a tag local (`flatWhich{instance}`) written as the
alternative closes, and Accept switches on it; the switch sections are braced,
because they share one declaration scope and every case names its captures the
same. A member captured in more than one alternative reads the slot that ran,
first written first - the chain the arena materializer resolves by entry presence.

Deferred construction holds exactly as before: nothing runs until the whole parse
is decided, and a site whose branch was not taken never constructs - the engine's
per-need materialization, kept by a sentinel instead of a link walk.

Alias, Either and Wrapped - the three catalog rules this stage was for - now
compile to the review's target forms: Alias is '#', a digit loop, and
`Construct_Number` feeding `Construct_Alias` at Accept; Either is a first-char
switch, a tag, and a factory switch; Wrapped picks `value1` or `value2` by
sentinel. The catalog file is 6,096 lines (10,973 three commits ago), and seven
publications still rent a parser: C/E/F (a way back - the checkpoint class),
Committed (atomic commit), Sum (climbing), Sheet (recovery), AnyItem (a find). The
engine test that pinned "a valued rule has one shared block" keeps pinning it, on a
grammar the flat path refuses; a new sibling pins the inline: three
`Construct_Name` sites, no call, no parser.

The notation's own grammar is unchanged by this - its machines are recursive and
stay on the engine. Its path to the same benefit is the recorded direction: an
engine machine calling a flat-valued rule as a method instead of a state, which is
the sound remainder of the eager-construction idea.

## Built: the atomic group joined the silent shapes

`{ "ab" | "a" }` is first-match-commits, and that is a shape locals hold: try each
alternative in order through the give-back door, and the first that matches is final
- nothing ever comes back, which is what "atomic" says. No `Atomic` entry, no commit
sweep putting out ways back, because none were written. The alternatives may share
prefixes freely - prediction is what this shape never needed - and each one's
captures are unset on the way through the door, the same discipline as a given-back
turn. `Committed` compiles to the review's local-checkpoint form for it, verbatim.

One refusal, caught by the semantic suite on first run: a machine that recovers
keeps the engine's atomic commit, because §8.2's discriminator rests on the commit
marking the element owned, and that mark is the engine's. The silence test and the
compile branch ask the same predicate, recoveries included, so they cannot disagree.

The catalog stands at 5,815 lines; six publications still rent a parser: C, E, F (a
way back past the construct's edge - the deferred normalization family), Sum
(climbing), Sheet (recovery), AnyItem (a find). The catalog's own section comments
now describe the shapes that are actually emitted.

## Fixed: two answers nobody needed to ask for

Two of the review's P1 points, both of the same kind - work spent answering a
question with one answer.

A rule with one factory wrote a `Construct` entry per completion, and the
materializer walked the link chain to find it - to learn which construction ran,
when only one could have. The entry is now written only where the body is a choice
of constructions, and the single-factory materializer calls its factory without
looking. A fold keeps its entries: there each one is an iteration, not a choice.
This is a quarter of the completion ceremony the factory-tower measurement charged
half of everything to, and the ratios moved with it: Csv 1.9, Feed 3.3, Minimal
2.2, Url 3.6 (from 2.0 / 3.6 / 2.4 / 3.9; two runs agreeing).

`Construct_Sheet(string[] item0)` counted its one argument into a copy of itself.
A §4.1 case 2 factory over exactly one repetition and nothing else now hands the
array back - it was built fresh by the materializer for this construction and is
shared with nobody.

The pool retention threshold (the review's third P1 point) stays as it is until
measured: with captures out of the arena the entry counts no longer scale with the
input, and 4096 covers far more than it did when the number was picked.

## Tried and declined: sound eager construction, measured to a net loss

The dynamic version of eager construction - the one the earlier diary entry designed
- was built in full and worked: a `wayBack` high-water mark over resumable entries
(Choice, Run, LoopExit, Lookahead), maintained O(1) at every push, repaired
conservatively at every resume ("the mark stands at the resume point"); at a valued
completion with `wayBack <= call`, the span above the call was materialized through
a bounded `Materialize_DotGram_Eager` and truncated to the Completed entry that owns
the value, with a DEBUG invariant recomputing the precondition by scan. All 1,068
tests and the differential fuzzer stayed green, the deferred-construction pins were
flipped to the agreed weaker contract, and correctness of values held by
construction: nothing from an abandoned derivation can survive a truncation.

Measured Release-to-Release against its own baseline, medians, two runs agreeing:

  Csv  1.93 -> 2.15    Feed 2.69 -> 2.94    Minimal 2.07 -> 2.40    Url 2.94 -> 3.40

A ten-to-fifteen percent net loss, and the reason teaches something the step-count
measurement hid: the factory tower's cost is the WRITING of Call/Completed/
RuleCapture and the Return dispatch, not the existence of the records afterwards.
Runtime collapse erases records that were already paid for, and charges for the
erasure: a materializer invocation per completion, `Truncate` link bookkeeping on
every unwind pop (the caches machinery, now on for every valued machine), and the
cached Accept sweep scanning the whole arena where the owners-list walk had not.
The only way to remove the ceremony's cost is to never write it - a compile-time
decision, which is exactly what the flat-value sites already do outside the engine.

Reverted per this project's own discipline (trie, left-factoring, mixed lowering:
built, measured, declined). The revert keeps nothing dead. What stands after it:
the deferred-construction contract is UNCHANGED - factories still run only at
Accept - since the weakening was only ever the price of a win that did not appear.

The direction that replaces it: compile-time inlining of flat-valued callees at
engine call sites whose site continuation passes the same proofs the flat path
asks (`FlatValued` callee, silent under the site's continuation) - writing no Call,
no Completed, no RuleCapture, holding the span in locals and deferring the factory
to Accept through the value-capture protocol. That needs the engine's capture
protocol to accept a value that has no Completed entry, which is the same seam the
eager build touched; the difference is that the decision is made once, at
generation time, and costs the runtime nothing.

## Built: sited calls - a captured call compiled as its callee's body

The compile-time successor to the declined eager experiment. A captured call whose
callee is the flat-value shape - one factory over captures that are spans of the
input, with a required span to witness the site ran - compiles as the callee's body
in place: its captures record into a run of slots the site owns (the same rule
inlined twice may not share records), and the materializer builds the member by
calling the callee's factory over those spans directly. No Call entry, no Completed
rewrite, no RuleCapture, no dispatch - nothing written that was not already paid
for, which is what the eager measurement demanded.

No silence is asked. The site's captures are ordinary arena records and unwind like
any others, so every call site of a qualifying rule qualifies, settled or not; and
construction is untouched - the factory still runs at Accept, off the accepted
derivation. Two exclusions carry the protocol they replace: a rule with a guard
reads members mid-parse through the completed-call protocol, and a machine that
recovers reads elements off it, so both keep the ceremony.

Two bugs found by the example suite on first run, both worth remembering. Node is a
record, and record equality is by value: `t: Line` written in two rules is one
dictionary key unless the map is built over `NodeIdentity` - the same lesson
CaptureLayout.cs already carries. And a rule every call of which became a site
leaves its own states unreachable, whose capture local then trips CS0219 in the
consumer's build - `UsesCapture` now checks the written states the way `UsesChar`
does.

Measured Release-to-Release: the corpus is flat (Csv 1.91, Feed 2.7, Minimal 2.10,
Url 2.87 against 1.93 / 2.69 / 2.07 / 2.94) - and the reason is the notation's own
shape, not the mechanism. Its valued leaf captures are collections: `usings:
Using`, `declarations: Declaration`, `alternatives: Alternative` - sequence
members, which V1 refuses because per-turn records need element boundaries the
scalar walk does not have. The scalar case pays off where real grammars capture a
lexeme-like rule once - Markdown's `text: Line` in Heading is the shape - and it is
the foundation the sequence extension stands on.

Next, in order: sited sequence members - wrap each element in one synthetic extent
capture as the boundary (net minus two entries per element and no dispatch, against
the three-plus-dispatch it replaces), walk the chain grouping parts between
boundaries; then transparent-collapsed multi-slot members if the counts say so.

## Built: sited collections - one boundary capture per element

The sequence half of sited calls. A collection member whose element rule is the
sited shape now sites too: each element is the callee's body compiled in place,
wrapped in one synthetic boundary capture whose entries are what tell one element's
spans from the next's. The materializer counts boundaries for the array's length,
then walks the chain once - reverse write order, so a boundary arrives after the
spans of its own element - closing each element as the next boundary appears and
the last one after the walk.

`Extent` also learned that a call to a valueless rule is text: `name: Name` in the
notation is that shape, and refusing it kept several rules off the sited path for
no reason. The flat renderer still gates such captures through `Silent`, which is
what refuses the calls it cannot compile without an arena; a sited capture needs no
such gate, since its records unwind like any other.

Per element this trades three entries and two dispatcher passes (Call, its
Completed rewrite, RuleCapture, the jump in and the return) for one - the boundary
capture - plus whatever spans the element itself records, which it recorded before.

Measured Release-to-Release, two runs agreeing: Csv 1.93 -> 1.88, Feed 2.69 ->
2.62, Minimal 2.07 -> 2.08, Url 2.94 -> 2.88. Small, and honestly so: the corpus's
sited collections are `usings` and `parts`, which the sample texts have few of. The
shape it is for is a document of many small records - Markdown's blocks, a feed's
rows - where the ceremony was three entries per record.

## Built: the scanner stopped being the grammar written out

A review of `Scan_trivia` named two things in it that were the automaton showing
through rather than anything the shape required, and both had general answers.

**One character for the whole choice.** Every alternative of a scan choice that must
begin with a character is refused by one test over the union of their first sets,
written before the chain. The scanner's commonest answer is "no trivia here" - it is
called at every seam - and that answer used to cost a test per alternative: for the
notation, a bounds check and a two-character span compare for `//`, then the same
again for `/*`, after the whitespace class had already said no. Now the character
says no to all of them at once, and the end of the input is the same test rather
than a cascade through every alternative's own EOF check.

**A guarded scan is a search.** `(?!L & any)* & L` was compiled as written: a turn
that tests the delimiter, refuses, rewinds and consumes one character, and then, on
the way out, the trailing literal testing the same delimiter the guard just proved.
Two marks, a rewind and a double read per character of every comment. It is one
search - `IndexOf` - and the runtime's is vectorized where that loop could not be.
The pair was already judged as a pair by `Scannable`, so the fusion is the same
judgement carried into emission: found is where the guard would first have held, not
found is the literal failing after a scan to the end, which is the pair failing.

Narrower than the general shape on purpose: only where the turn consumes `any`. A
turn like `?!X & [^ '
']` can also stop because its own test refused, which a search
for X would run straight past, so that shape keeps the loop. `any` is a rule of the
standard library rather than an element written in place, so the predicate follows
calls - which is what made the first attempt silently not fire.

Ratios, Release, two runs agreeing: Csv 1.88 -> 1.76, Feed 2.62 -> 2.40, Minimal
2.08 -> 1.86, Url 2.88 -> 2.62. The largest single step since the exponential fix,
and it is all in the seam: Url has no trivia at all and still moved, because the
notation grammar that parses these files is what the corpus measures.

Not taken: prefix dispatch merging `//` and `/*` under one `'/'` test. The front
test already removed the negative path both were on, and what is left is one span
compare per comment start. Same reason the trie was declined: measure a shape that
exists first.

## Built: the CFG stopped photographing the automaton

The review's diagnosis - the generator optimizes the parsing operation but not the
control-flow graph it leaves behind - taken in five pieces, each general.

**A door for nothing.** The give-back door restores a position the failing body
never moved: a repetition or optional whose body is one character (an element, a
one-character literal, `Behind`) fails before `p++`, so `FailsWhereItBegan` routes
its failure straight to the continuation - no `turn` local, no restore state, no
trampoline. The identifier tail, the optional marker, and the silent atomic's
alternatives all lose their `p = turn0; goto` blocks. A literal longer than one
keeps the door, and not only for the obvious reason: its failure branch moves `p`
to the character that did not fit, for the diagnostic.

**A lookahead one character decides is its test and nothing else.** No local, no
consuming, no rewinding: `?!W` is `if (p < length && W(text[p])) fail`, and `?=` the
mirror image. §4.6 weaves one of these around every word literal, so every keyword
boundary in every grammar was paying the checkpoint ceremony for one comparison.

**Jump threading over the scanner.** Emission is compositional - a choice cannot
know its taken-exit falls into the loop's back-edge - so the seams are threaded over
the finished text: a jump to a label whose block is another jump goes where that one
goes, a jump to the label it falls into disappears, an unreferenced label goes, and
a jump left unreachable by a removed label goes with it (a branch of a two-line `if`
recognized as conditional, whatever its own line says). Two passes to a fixpoint.
`Scan_trivia`'s whitespace turn is one back-edge instead of two jumps; the block
comment's success path is one jump; the EOF cascade is shorter by every label that
did nothing.

**One classification instead of five.** A multi-category element test called
`GetUnicodeCategory(c)` once per category; the enum fits an int, so it is one call
and one mask: `((1 << (int)GetUnicodeCategory(c)) & 0x...) != 0`. The notation's
generated file holds 90 textual calls where it held about five hundred.

**The pool held the previous document alive.** `Reset()` cleared the object table
but not the typed ones, so a pooled parser kept references into the last parse's
tree from a thread-static field until the next parse happened to overwrite them.
The typed tables are now cleared with the rest, in the same `finally`.

Ratios, Release, two runs agreeing: Csv 1.78, Feed ~2.3, Minimal 1.86, Url 2.52
(from 1.76 / 2.40 / 1.86 / 2.62) - the gain is Url's, the rest is hygiene the
corpus cannot see: fewer states, fewer jumps, no retention.

Left open from the review, in order: the redundant bounds-and-reload at a choice's
first alternative after the front test (the emission would have to carry "c holds
text[p]" across the seam); partial FIRST dispatch for choices that are not fully
disjoint - Primary's `'@'` deciding CsExpr against RefOrCall one character early;
and the Return/Dispatch architecture itself, deliberately last.

## Measured: the whole series, on the shape it was for

The corpus's flat ratios kept saying the same thing: the notation grammar is a tower
of recursive valued rules, and most of the series - hoisted captures, valued flat,
sited calls, scanner work - lives elsewhere. So the benchmark that was missing got
written: `Documents.cs`, four hundred key-value records with trivia at every seam,
values that are spans, a collection collected in order. Three inputs of one length -
dense, spaced, commented - tell the seam's cost from the record's.

Against the state before the series (one worktree per side, same machine, same run):

  dense      287.5 us -> 19.3 us      allocated  3.14 MB -> 46 KB
  spaced     279.8 us -> 20.5 us
  commented  276.9 us -> 21.2 us

Fifteenfold in time, sixty-eight-fold in allocation, and the remaining 46 KB is the
result itself. The seam now costs seven percent over dense and comments ten. The
numbers went into benchmarks/README.md next to the URL ones, which measure the dense
regex-shaped case this benchmark deliberately is not.

Writing it also walked into a sharp edge worth remembering: `entries: Entry*` in a
spaced grammar parses `a;b;` and silently refuses `a; b;` - §4.5 puts trivia between
the operands of a sequence and not between the turns of a repetition, because
`Word*` cannot be told from a list by looking, and the semantic tests pin exactly
that. An attempt to widen the turn seam to called turns broke those pins and was
reverted the same hour; the list spells its seam itself - `(trivia & entries:
Entry)*` - the way the notation grammar always has. Whether the language should say
something louder at that edge (a diagnostic for a called repetition in a spaced
namespace with no seam in the turn?) is a language question, noted here and not
decided.

## Fixed: a collection of a valued rule is spaced

The benchmark's natural grammar - `entries: Entry*` in a spaced namespace - parsed
`a;b;` and silently refused `a; b;`, and the ruling was that a grammar must work the
way it reads. The line that makes both halves keep their meaning is valuedness: a
repetition of a rule that builds a value is the collection §4.1 case 2 gathers, and a
grammar that separates its operands separates its collections the same way; a
valueless operand is a fragment of text - `Digits = ['0'..'9']+`, `Name = Letter+` -
and spacing its turns would make `1 2` one number. The first attempt drew the line at
"any called turn" and broke exactly that: §4.5's own `Name = Letter+` example.

Valuedness needs the types, so the seam is not woven in lowering but by `SpaceLists`,
a normalizer pass after `ComputeTypes` and after `CollectSequences` (whose implicit
capture must sit inside the seam, not around it), re-keying `recover` like every
rewrite that replaces a repetition node. The old pins survive untouched - `Word*` and
`W*` are valueless and stay lexeme-shaped - and a new pin runs the same four inputs
through the captured, bare and hand-seamed spellings of one list.

Two analyses had to learn what a seam is. `Undecided` (GRAM5002) saw the woven turn
overlap the woven continuation on every space - one seam split two ways, not a choice
the input decides - and now discounts the seam on both sides. And `StreamedParse`
told the author to "give the repeated part its own rule" about a part that had one;
it now says the true thing: a streamed parse does not yet skip trivia between the
elements it hands over. Building that driver - the seam is already a stage shape the
streaming emitter knows - is recorded as the follow-up.

§4.5 in docs/syntax.md now states the rule as the language means it. The Documents
benchmark reads the natural spelling and its numbers stand: dense 20.0 us, spaced
19.8, commented 20.7, 46 KB - within the run-to-run spread of the hand-seamed form.

## Built: a spaced collection streams, seams and all

The follow-up the last entry recorded, closed. `Yields` and `StagesOf` now recognize
the seamed turn `(trivia & item: Entry)*` that SpaceLists weaves - guarded by the
rule's own trivia symbol, so a sequence turn that merely starts with some call is not
mistaken for one - and the stage carries the seam rule. The driver skips it at the
top of every turn, before the continuation probe and the element alike, with the
same grow-the-window loop every other read uses; retention is measured past the
seam, since the skip moves the window as it goes and only the element is ever held.
The seam rule is registered like any staged rule and gets a recognizer under its own
name - `Recognize_trivia` - whether or not it is scanner-shaped.

The interim GRAM5001 text ("does not yet skip the trivia") lasted one commit, which
is the good kind of diagnostic debt. A driver test pins the three claims that
matter: the string and reader overloads agree on a spaced list, a broken element is
stepped over across a seam, and four thousand seamed records read through a window
that holds none of them for long.

## Tried and declined: fusing RuleCapture into Completed

The last mechanical squeeze on the completion ceremony looked free: a Completed
entry already sits with its caller's index in CallIndex, its State field seemed dead
once Return had read the continuation out of it, so the capturing slot could live
there and the RuleCapture entry - one of the three per completion - would never be
written. Built in full: claim as an in-place rewrite, member walks reading the
completion itself, valued Calls linking at birth so the incremental pass cannot walk
past a not-yet-completed call (a real find - RuleCapture never had the problem only
because it was always born ahead of LinkedUpTo).

Two walls, both instructive. First, the state renumbering pass: Layout rewrites the
second argument of every resumable ParserEntry literal as a state id, and a slot
sitting in that position was silently renumbered into garbage - the exact "silent
corruption" its own comment warns about, met from the other side. Removable, and
removed. Second, the real one: a completed call can be resumed INTO - a standing
exit in its tail - and complete again, and the second Return reads the continuation
out of State. The continuation is not dead after Return; it is dead only when
nothing can resume into the span, which is a dynamic fact this session already
declined to track once. The claim and the continuation both need the field, there
is no other free field in ParserEntry, and encoding both means renumber-aware
arithmetic in every reader.

So the RuleCapture entry is not a duplicate answer after all: it is the claim as a
separate, poppable record - unwinding it is what un-claims - and the Completed's
State is the continuation for however many times the call completes. The same
lesson eager construction taught about records and backtracking, met at the next
record over. Reverted clean; the linked-at-birth insight goes with it, since only
the fusion needed it.

## Built: the checkpoint class - a way back in three locals

The last valueless shapes renting a parser - C, E and F of the catalog - were choices
that genuinely need coming back to: a shorter literal written first, or a continuation
that can spend the character the longer alternative would have taken. What the arena
held for such a choice is three facts, and for a choice no repetition stands over,
three locals hold them: `way` is the position, `alt` the next alternative to try,
`over` the site that was pending before this one opened. `pending` names the innermost
open site, and the flat method's `Fail:` becomes the engine's unwinding without the
engine: record the failure against the furthest seen - ties added, the same
max-comparison RenderEngine makes - then resume the innermost site's next alternative,
or close it and hand the failure to the site it opened over, until none is open and
the method returns. Re-entry from an outer resume runs the site's entry again, which
re-arms all three locals; the runtime nesting is a stack, flattened into per-site
locals because one site activates at most once.

Admission is `Silent`'s choice case grown a third clause, and the compile arm asks the
same helper, so the two cannot disagree. The refusals are the flag `_checkpointsAllowed`
going down inside every construct that routes failure around `Fail:` - a silent
repetition's door, an atomic chain, a lookahead's rewind - and `Deterministic` already
refuses the choice, so no silent repetition ever contains a site. The valued rendering
refuses too, for now: a retry would have to unset the capture locals the abandoned
attempt set, and no valued shape in the corpus asks for it yet.

Two things came out better than planned. E compiles to the review's target form
verbatim: try `"http"`, one shared `'s'` state, and the failing tail resumes `"https"`
through the dispatcher. And GRAM's one documented diagnostic gap - a prefix-conflicted
run under-reporting what it covers - closed itself: `"p" | "q" | "pr"` against `x` now
says all three, because every alternative's failure is recorded the way the engine
records it. The test that pinned the gap asked to be changed on purpose; it was.

One thing is honestly worse, and recorded: the engine's form of `"http" | "https"`
pushed its way back past the four matched characters and compared the fifth alone,
where the checkpoint retry rewinds and compares `"https"` whole - a retry-path cost
against a straight-line path that no longer rents a parser. Teaching `CompileLiterals`
to chain into a retry label instead of an arena entry would restore it; follow-up.

And the deferred "concatenation" - distributing E's tail into its alternatives,
`"https" | "httpss"` - closes as subsumed: the checkpoint class covers the shapes it
was invented for without touching expected-sets or failure positions.

The catalog: 6,055 lines to 5,359, and the only publications still renting a parser
are Sum (climbing), Sheet (recovery) and AnyItem (a find). All 1,076 tests green,
the reference differential and the fuzzer included.

## Built: the scanner's front test hands its character on

The one redundancy the scanner review left open: after a choice's front test proved
the position in bounds and read `c`, the first alternative asked both questions over
again - its own bounds check, its own read of the same character. The emission now
carries the invariant as a flag: true right after a front test, passed into every
alternative (each begins where the test read), through what consumes nothing -
`Behind`, a lookahead, `none` - and dropped at the first thing that moves. An element
head and a one-character literal head become the bare comparison.

Corpus, Release, two runs: 1.74/2.38/1.82/2.58 and 1.76/2.33/1.83/2.49 against
1.72/2.30/1.87/2.50 before - noise, and recorded as such. The seam's answer was
already one test; what this removes is two instructions per alternative behind it,
which the corpus cannot see. Kept for the same reason the CFG cleanups were: the
generated text stops re-deriving what the line above it just proved.

## Measured: the pool's threshold was cutting off the documents that need it

The review's third P1 point, held until measured; measured, and it bit. A pooled
parser whose arena grew past 4,096 entries was let go, and the corpus never noticed -
Csv 512, Feed 1,024, Minimal 2,048, Url exactly 4,096 - but an ordinary 12 KB grammar
document sits just over the line, so every parse of it rebuilt the whole machinery
from nothing: 1.13 ms and 3.8 MB per parse, of which the tree itself is 315 KB.

Three policies, one binary each, alternated five rounds. Dropping (the status quo):
1.13 ms, 3.8 MB. Trimming the tables back to the threshold and keeping the parser:
1.33 ms, 3.75 MB - worse than dropping, because the trim is itself a large-object
allocation per parse and throws away nearly everything anyway. Keeping the parser
whole: 0.85 ms and 315 KB - a quarter of the time gone and twelve times less
allocation, the remainder being the tree the parse exists to build.

So the policy stays what it was - keep unless outsized - and the bound moves to where
"outsized" actually is: 65,536 entries, a few retained megabytes for a thread whose
documents are that size, with the drop kept for genuine pathology. The trim variant
is recorded here so nobody builds it again hoping.

## Measured: two more forms the machine does not need to change

**The range comparison is already the subtracted form.** `c >= 'a' && c <= 'z'` as
the emitter writes it compiles to `sub eax, 97; cmp eax, 25; jbe` - the exact code
`(uint)(c - 'a') <= 'z' - 'a'` would ask for, because folding a double comparison
into an unsigned subtract is a peephole RyuJIT has. A second range in the same test
comes out branch-free (`setle`/`cmovl`). The generated file keeps the form a person
reads; the disassembly is the same either way. Third entry in the "the JIT already
does it" series, same method: `DOTNET_JitDisasm`, not an opinion.

**ParserEntry's size is not on the critical path.** The packing question - nine int
fields, forty bytes, would fewer be faster - answered by the cheap experiment first:
two dummy fields *added*, +20% per entry, corpus measured three runs against three
baseline runs. The ranges overlap completely (Csv 1.76-1.82 padded vs 1.74-1.77;
Url 2.48-2.57 vs 2.40-2.58). If a fifth more traffic per entry is invisible, a
third less cannot be visible either: at these arena sizes the entries live in cache
and the stores forward. Packing is declined without being built - the padding proxy
is the measurement - and the fields stay whole ints a debugger can read.

## Built: a lookahead's demand is a first set, and it settled the tower

The trace of Url.gram put 23% of all steps into fail/resume pairs, 128 of them - one
per operand - resuming the exit of `Prefixed`'s optional `("?=" | "?!")?`. The
optional's body begins with '?', nothing after it can, and it still compiled as the
general machinery. The chain led to one line: `CsExpr = ?="@(" & text: @CSharp` has
first set "anything", because `Following` skipped the lookahead as consuming nothing
and then met the external, whose honest answer is All - and that one answer poisoned
every first set the C# expression is reachable from, which is every operand of the
notation. The `?="@("` guard had been added to spare the runtime the speculative
scanner call; the analyses never heard about it.

Now they do. `First.And` is intersection - the one asymmetry to `Or` is that
"anything" is its identity, not its absorber - and `Following` holds a positive
lookahead met before anything could consume as a constraint on what the rest begins
with, dropped as soon as a part may have moved the position. `?="@(" & @CSharp`
begins with '@', whatever the external would say for itself. No emission changed for
any snapshot grammar; the notation's own optionals began to settle: 7,995 trace steps
became 7,011.

## Built: one character before the machinery, on every repetition that may take nothing

What remained after the tower settled was honest ambiguity paying dishonest rent:
`recovery: Recovery?` and `rebound: With?` stay unsettled because `recover` and
`with` collide with an identifier at a rule boundary - the diary's own "`recover`
against a rule named `recover`" - but the machinery ran at every operand, where even
the first character did not match: a Repeat entry, a pushed way out, a failed probe,
its dispatch, and the leave, to learn that '&' is not 'r'.

The fix is the test a choice link has made since it was measured worth making: a
repetition with `min == 0` whose body's first set is known is entered through one
character. Outside the set, the body cannot begin, so the repetition takes nothing
and the machinery is never built; inside it, everything is exactly as before - every
way back the general form keeps is kept, because entering commits to nothing. The
settled optional's char-test form stays what it was; this is its unsettled sibling.

Url.gram's trace: 7,995 steps at the day's start, 7,011 after the first sets, 4,523
now - fail/resume pairs 921 to 201 - and what remains is 62% completion ceremony,
the measured floor. Corpus, Release, three runs: Csv 1.33, Feed 1.71, Minimal 1.38,
Url 1.75, from 1.74 / 2.35 / 1.83 / 2.53 - the largest single step of the series,
and the lexical design's own targets (Csv at or under 1.5, Url under 2) reached
without a token in sight.

## Built: the guard learned to read the body instead of the ranges

The one repetition the entry guard could not cover was the one the trace said most
operands walk into: `(name: Identifier & ':')?`, whose body begins with `\p{L}` -
a few hundred ranges, which `Decidable` rightly refuses to spell out as comparisons.
But the same category as an *element* is one classification call, so the guard now
has a second source: `EntryTest` walks the body to the first thing that must
consume and lets each leaf write the test it already knows how to write - an
element's own test, a literal's first character, a choice's disjunction, a nullable
head alongside what follows it. Sound in one direction only: it may admit more than
the body would, never less. The compact `RangesTest` form is still preferred where
the first set is small; the walk is the fallback for the sets no rendering should
spell out.

Url.gram's trace: 4,523 to 4,203; the day as a whole, 7,995 to 4,203. Corpus,
Release, three runs: Csv 1.33, Feed 1.70, Minimal 1.34, Url 1.70. Two thirds of
what remains is the completion ceremony; the enumerable residue is now exactly one
shape - the `name:` probe reading a word that the reference after it reads again,
which is the two-token fusion the lexical design names as v2.

## Closed: the performance program, at the numbers it reached

Opened by one sentence - the generated parser must do the same work a hand-written
one does, or less - and closed here with the backlog empty, every item either built
and measured in, or measured and declined with the number written down.

Where it ended, corpus medians, Release, hand-written front end against the
generated notation grammar: Csv 1.33, Feed 1.70, Minimal 1.34, Url 1.70 - from a
flat ~3.1 at the series' start and 1.7-2.5 at this morning's. The document shape:
19.3 us and 46 KB from 287.5 us and 3.14 MB. Url.gram's step trace: 7,995 this
morning, 4,203 tonight, two thirds of it the completion ceremony whose floor two
recorded experiments drew. The lexical design's own finish line - Feed and Csv at
or under ~1.5, Url under 2 - crossed without building the token layer it thought
that would take.

The last day's ledger, for the record. Built: the checkpoint class (C/E/F flat,
ways back in locals, the under-reporting gap closed as a side effect); the scanner's
front test handing its character on; the pool keeping what it was built to keep
(65,536-entry bound, a quarter of the parse and megabytes per call returned);
lookahead-constrained first sets (one line of analysis un-poisoning every operand of
the notation); the entry guard on may-take-nothing repetitions, with `EntryTest`
reading the body where ranges are unwritable. Declined with measurement: trimming
the pool instead of keeping it; ParserEntry packing (the padding proxy); the
subtracted range form (the JIT's already); per-binding-power climbing entries (the
guard it removes is one predictable compare of the class both proxies just measured
invisible; Nitra needed it because its dispatch was dynamic - ours is an inline
constant against a register). Closed by count: multi-slot sited members, with zero
admitting shapes in the repository; partial FIRST dispatch, its target mass - 921
fail/resume pairs - reduced to 137 by the two analysis fixes, below anything a
dispatch could repay.

One enumerable shape remains and is named in docs/status.md: the optional `name:`
probe reads a word the reference after it reads again - the two-token fusion the
lexical design calls v2, also reachable as the same hand-factoring `RefOrCall`
already demonstrates. It is a direction with a design, not a debt: the program
closes with the floor measured, the residue enumerated, and every claim in this
file carrying the number that earned it.

## Fixed: a replacement reached through a binding did not observe its siblings

`parse Start with (A = B, Sep = Semi)` handed out a `B` still reading the unbound
`Sep`. Two holes of one shape: the affected-set's forward reachability walked the
graph as written, so `B` - reachable only through the binding - never entered the
set and was never cloned; and a bound call landed on the plain replacement even
where a clone of it existed. The walk now follows the binding edge, and a bound
call lands on the replacement's clone. §5.1's own words decide the semantics -
bindings resolve simultaneously over the whole call graph reached, and the
replacement is part of that graph the moment the binding reaches it. Found by
reading the machinery toward the parameterized-rebinding work, pinned by a
semantic test, and every existing pin held - a bug, not a decision.

## Built: a parameterized rule on either side of a rebinding

The status table's one red row, closed. `with (A = B)` where `A` takes parameters
used to be refused at Bind with "not supported yet", and the reason lived two passes
later: by the time a binding is realized, §4.2 has already instantiated every call -
`A('a')` is a call to a parameterless specialization, and a substitution keyed on
`A` would find no call to touch.

The meaning was never in question - a rebinding substitutes the rule and keeps every
call's arguments - so the machinery now says exactly that. The binder admits a pair
of the same signature (same parameter count, each parameter the same kind) and
refuses a mismatch with a message that explains itself; every instantiation records
what it is an instance of and of what arguments; and the specialization pass, on
meeting a call to an instance of a bound rule, builds the replacement's
instantiation for those same arguments and clones it under the site - so the
replacement's body and the spliced arguments alike observe the same header's other
bindings. The clone registers before its body is rewritten, which is what lets a
recursive specialization resolve to itself instead of cloning for ever. §14's type
check reads a parameterized rule's declared type where it is concrete C#, and skips
`: item` deliberately: both sides receive the same argument, so they produce the
same rule's value by construction.

On the way in, a §5.1 bug that predates the feature: a replacement reached only
through a binding was never in the forward-reachable set, so it was never cloned and
`with (A = B, Sep = Semi)` handed out a `B` still reading the unbound `Sep` - fixed
first, pinned separately.

Ten new tests: signatures accepted and refused at Bind, the header reaching a
parameterized call in a shared rule, a value argument carried, an argument observing
its sibling bindings, `: item` handing its value through, and §14 firing on declared
types. All 1,087 green; the old GRAM3009 pins survive as what they now are -
signature mismatches.

## Built: indirect left recursion, where the rules between only forward

Next red row, and it splits in two. §4.3 refuses a rule that reaches itself through
another because indirect recursion has arbitrarily many shapes - but one of those
shapes is not arbitrary at all, and it is the one every expression grammar is
written in:

    Primary : @Expr = p: Call => @(p) | n: Number => @(n)
    Call    : @Expr = target: Primary & '(' & args: Args & ')' => @Invoke(...)

`Primary` only forwards, which is the identity `CollapseTransparent` already proves
for the same shape: an alternative that is a captured call handed back unchanged
means nothing the call does not mean. So the leading `Primary` is the choice of what
it forwards, the alternative distributes over that choice - sound because calls are
transparent to backtracking, so `(X | Y) & rest` and `X & rest | Y & rest` try the
same readings in the same order - and what is left is `Call` calling itself leftmost.
§4.3's own rewrite folds it from there. Nothing new happens at run time; this
rewrites the grammar into a shape the language already had, and `7()()` folds left
as it reads.

The pass runs inside `RewriteLeftRecursion`, per rule, immediately before its
alternatives are classified, and only where the forwarder's sources actually include
the rule - so a layered grammar with no recursion in it is left node for node as it
was. Types are not computed yet, so the declared ones stand in and must match
exactly: a forwarder that widens is doing something after all. And a valueless alias
under a capture is left alone - its own value is the text it matched (§4.1 case 4),
and a source with a value of its own would put that value there instead.

What stays refused, now with its reason written down rather than discovered: an
intermediary that does anything of its own. Its operands and its `=>` would join the
tail of the fold, so a step would have to apply two constructions in order against an
accumulator that is itself the result of one - a staged fold, which neither the value
machinery nor the arena has a shape for. Both halves are pinned: the postfix chain
runs, the mutual `A = B - N | N` / `B = A + N | N` is refused, and the old
`Other = Start | 'y'` pin survives unchanged as what it now is - an intermediary that
is not only a name.

## Built: a value parameter is the literal the call passed, wherever it is written

Third red row, and it opened on a live bug. §4.2 says a value parameter is allowed
anywhere a value is expected - a quantifier count, the arguments of `@Method`, inside
`@(...)` - and only the count ever worked. The name written in C# was emitted as
itself, so `Digits(n: int) : @int = ['0'..'9']{n} => @(n * 100)` produced a factory
reading an `n` that does not exist: a compile error in somebody else's build, about a
file they never wrote, with no diagnostic of ours anywhere near it.

A specialization has one concrete argument, so what a value parameter stands for is
known where the specialization is made and it is a piece of C# text. `Substituted`
puts it where the name was written, identifier-aware rather than a string replace: a
parameter called `n` does not rewrite the `n` of `name`, of `x.n`, of a comment, of a
string, or of a character literal - and an interpolated string is read as what it is,
text with code in its holes, so `$"{t}-{n} n"` becomes `$"{t}-{7} n"`. Verbatim
strings, doubled quotes and doubled braces are all text.

With that in place the feature is the same mechanism: a literal argument is a value,
`Text(Expr)` already renders one as the C# it stands for, and the value is part of what
a specialization is - `Mark(Word, '!')` and `Mark(Word, '?')` are two rules. What the
literal is is C#'s to say, so the resolver is asked; a permissive one leaves the answer
to the consumer's own compiler, which is where §7.4 puts every other question about the
C# a grammar wrote.

Two things the first attempt got wrong, both caught by the suite rather than by
reading. A literal's kind comes from the parameter's declaration, not from the literal:
the same `' '` is a piece of grammar where the parameter is a recognizer (JSON's
`List(Value, ',')`, `Padded(Word, ' ')`) and a `char` where it is a value. And a number
is a value whatever the declaration says, because `Digits(n) = ['0'..'9']{n}` is how
§4.2's own example writes a count and `FixedWidthExample` is built on it.

§4.2's table promised "a literal or a previously captured value". The second spelling
cannot work and now says so: a specialization is made before anything runs, and a
captured value exists only while it does. Refused with that reason, and the spec
corrected rather than left promising it.

## Built: a rule may recover in more than one place

Fourth red row, and the machinery was already there. `_recoveryPlans` has been a list
since there was one plan, every plan carries an id, and the arena has dispatched a
recovery by that id all along - what was single was the *lookup*: `RecoveryIn` returned
the first marked repetition in a rule and the emitter asked once. So the refusal said
"the machine keeps one and would ignore a second", which was true of the lookup and not
of the machine.

`RecoveriesIn` returns them all, in the order the rule reads; the machine makes a plan
per marked repetition; and the one thing that had to differ - the name of the factory a
`recover`'s `=>` becomes - is settled by `RecoveryMethod(rule, index)` for both halves
of the emitter at once. The first keeps the name it always had, so every grammar that
had one recovery generates exactly the text it did.

The stream is the exception, and it stays one - for a reason, not for want of
machinery. The driver steps over a bad element as it hands the good ones back, reading
one repetition at a time, so a second `recover` in a streamed rule would be one that
quietly does not happen. That is not a refusal of the grammar, though, and it moved to
where it belongs: `StreamedParse` now names it as a reason, so the publication is told
it gets no reader overload (GRAM5001, Info) and parses whole exactly as it reads. The
§8.2 check in the normalizer is gone; a §6.3 constraint belongs in §6.3's own analysis.

Pinned by a parse that recovers twice in one rule and proves each rejection came
through its own `=>` - `a,!b1b|c,?d2d` - and by the streamed publication being told
why it has no reader.

## Built: a match says which kind of answer it is, and §7.5 says what is built

Fifth red row, and reading it found the row was arguing with two other sections.
§7.5 sketched a `RecognitionResult<T>` that is `internal`, carries a `SourceSpan` and
allocates a `Diagnostic`. Three of those four cannot be: an `internal` type cannot
appear in the `public` signature a publication hands it back through (§6.1, CS0051,
which is why `Match<T>` is public); a `SourceSpan` is int-based while an offset into
the input is a `long` (§6.3), and it is emitted only for grammars that ask for one;
and an allocated diagnostic per failure is what `Error`-built-on-demand exists not to
be. The row was a sketch the implementation had outgrown in three places and not
reached in one.

The one it had not reached is real and is the whole point of the section: a failure
that ran out of input is not a failure that met input which did not fit. A caller
reading from a stream wants a longer read; one reading a document wants a message.
`Match<T>` now carries `Outcome` — `Success | NoMatch | Starved` — with `IsSuccess`
kept and derived from it, so the two can never disagree. §7.5 is rewritten to
describe what exists, with the reasons, rather than what was drawn.

Getting it exact and free took three attempts, and the two that were dropped are
worth the lines. The first marked a local at every room check and adopted it in
`Fail:` beside `Expected` — correct, and it cost Minimal 1.34 to 1.45 (measured
against a stashed baseline, four runs each): two statements in the automaton's
unwinding path, which every backtrack passes. The second made the local a position to
drop the per-failure clear, and cost the same — so it was never the clear.

The third asks the question the boundary actually asks. What the boundary wants is
whether the *furthest* failure ran out, so nothing has to be threaded: a room check
writes its own position straight into the failure record, and the boundary compares
that with the furthest position. A room check somewhere the parse later got past does
not match and says nothing. Nothing is adopted, nothing is cleared, `Fail:` is
untouched — and only a test wanting more than one character writes at all, because a
test wanting one can fail for want of room exactly at the end of the input, which the
boundary reads off the position it already has. Minimal 1.28-1.32 against the
baseline's 1.34-1.36; the other three inside their spread.

`Error` — §7.5's fourth outcome, a failure past a commit point — is not there, and
the row says so. It would need "the furthest failure stands past a commit that still
holds", which the arena does not keep; a flag set where a commit happens would call
an abandoned alternative's commit an error. An outcome that is sometimes wrong is
worse than one a caller has to ask about another way.

And a fourth thing the benchmarks caught after the tests were green: a grammar whose
every test wants one character never writes the field at all, and a field nothing
assigns is CS0649 — a warning, which in a build that treats warnings as errors is a
broken compilation of a file the consumer did not write. `DotGram.Benchmarks` has four
such grammars and said so. Suppressed at the field, with the reason on it, rather than
gated: the gate would have to be decided before the machines render and the wrapper
that reads it is written before them.

## Read: the last two rows were decisions wearing a gap's clothes

The status table's remaining crosses were `document repair` and `incremental parsing`,
and going to build them found there was nothing to build.

**Repair is out of scope and was already said to be, three times.** `syntax.md` §11
lists it under "deliberately out of scope"; `implementation.md` §0 says it was tried
and abandoned in favour of §8.2; and that plan's own preamble names "recovery as a
search for the cheapest edit over a whole document" among the sections it *removed*
rather than corrected. The table meanwhile pointed at "§6 of the engine plan" - a
section that no longer exists, since the plan runs 0, 1, 3, 5, 7, 9, 10, 13. A row of
crosses against a decision reads as a debt, and pointing it at a deleted section reads
as a plan nobody kept.

**Incremental parsing is unstarted, and the plan says that too** - its memo-table
sketch is in the same removed list, taken out once it was clear it had never been
reached. What it does have is one concrete prerequisite, from reading Nitra earlier
this month: the arena would have to record each entry's size rather than its position,
so a tail nothing touched survives an edit that shifts everything after it.

So both moved out of the pipeline table and into a paragraph under it that says which
they are: a decision with its reason, and a direction with its first step. The table
is for constructs, and a construct that is not a construct does not belong in it. That
is the whole change - and it is the honest end of the walk through that table, because
every other row it carried is now either ✓ across the pipeline or a refusal with a
message that explains itself.

## Built: a directive names an expression, because a reference position takes one

The ruling that opened this: an inline rule on the right of a rebinding would be
another patch, and the question to ask instead is whether the language means "an
expression may stand wherever a rule is referred to". Walking the parser answered how
big that is. Of the eight places the notation requires a name, seven are *declarations*
— a namespace, a rule, a parameter, an `as` — where the name is the point. The
reference positions, where a rule is used rather than named, are exactly two: the right
side of a rebinding, and the target of a publication. Everything else that could want an
expression already takes one: call arguments, `recover`'s synchronization expression, a
rule's body.

So the rule is finite: **wherever the notation refers to a rule, one operand may
stand.** One operand, the bound §8.2 already gives `recover`'s sync — so a choice needs
brackets, and the `with` that may follow a directive is the directive's own rather than
the operand's, which is what keeps §5.1's two extents apart (an expression's `with` is
applied before a namespace's, a publication's composes on top of one, and only the
parentheses say which was meant).

The mechanism is a lift, in the parser: anything but a bare name becomes an ordinary
rule declared right there, and the directive publishes it by name. Nothing after the
front end changes — the binder, §5.1's specialization, the normalizer and the emitter
all read a publication of a rule exactly as before, and the scope question the diary
asked to settle first settles itself: the rule is declared where the directive is
written, so it reads that namespace's trivia, imports and bindings, because that is
what a rule written there would do. The name is the `as`, which a compound target now
has to carry: an expression has no name to derive one from, and `parse ('a' | 'b')`
alone is refused (GRAM2007) rather than given a name nobody wrote.

Two things the parser had to learn. `parse` and `find` stay contextual keywords, so
`AtPublication` had to admit every token an operand may open with rather than only an
identifier. And `StartsRule` decided that an identifier followed by `(` is a rule with
parameters — which `parse ('a' | 'b')` is not: the two are the same until the
parenthesis closes, and what tells them apart is the `=` or the `: Type` a declaration
has after its parameters. Scanned to the matching parenthesis, which is bounded and is
the only look this decision needs.

**And a limit worth stating rather than discovering.** The lifted rule is a rule, so
§4.1 case 4 says what its value is — the extent it matched. `parse Padded(Word, '#')`
answers with the text, not with what `Padded` builds, because a rule written that way
would answer with the text too. That is the language being uniform rather than the
feature being unfinished: an expression is published for what it recognizes. Reaching a
*value* from a directive would need the lifted rule to declare a type, which is the next
question and a separate one (`=>` without a declared type is GRAM4008, deliberately).

## Built: the other half — a type on the directive, and an expression on the right of `with`

The limit the first half ran into was that a lifted rule can only be what §4.1 makes of
a rule with no declared type: its extent. So the directive got the third part a rule has,
in the place that reads as the method's own — `parse … as Marked : @string`. With a type
declared, a `=>` inside the expression is legal where it always is, and everything §4.1
offers is reachable from a directive; without one it is refused where it always is
(GRAM4008). A directive that names a rule has nowhere to put a type and says so
(GRAM2008): that rule declared its own where it was written.

And the second reference position, which is where this began: `with (Comma = (',' | ';'))`.
The same lift, the same scope answer — a rule declared where the `with` is written, named
after what it replaces (`Comma_With1`), so the substitution reads the trivia, the imports
and the bindings that surround it. The left side stays a name: it identifies what is
replaced, and identifying is what a name is for.

Both reference positions now take an operand, which is the whole of what "an expression
may stand wherever a rule is referred to" comes to in this notation — and the two
diagnostics it needed are about naming, not about the rule. Nothing downstream of the
parser knows any of it happened.

## Built: the notation's own grammar caught up with the notation

Today's syntax changes were made in the hand-written front end, and the grammar of the
notation — written in itself, in `GramExample.cs` — still described the language as it
was that morning: a directive's target was `Name`, a rebinding's replacement was
`Identifier`. The self-hosting differential was green the whole time, which is the part
worth noticing: it was green because the corpus contained no grammar using the new
forms, so it was holding two implementations to a language both had already outgrown.
That is the same shape of gap the lexical inventory found once before, when the two
implementations turned out to accept different languages at `parse Xas y`.

So the grammar learned the two forms, and gained the production the hand parser has
implicitly: `QuantifiedCore` — an operand up to but not including a trailing `with` —
which is exactly what a directive's target and a rebinding's replacement take, and why
a `with` after either belongs to the thing around the operand. `Quantified` is now that
plus the `with`, which is what it always was underneath.

And a fifth file joined the corpus: `Notation.gram`, a catalog of what an author may
*write*, as `Minimal.gram` is a catalog of what the generator *emits*. It uses the
forms rather than describing them — a directive naming a call, one declaring the type
of what it lifts, a rebinding replacing a rule with a choice, a recursion through a
forwarder, a value parameter given a literal — so the differential reads them on every
run. Checked by reverting half of the grammar's own `Publication` rule and watching the
differential say `Notation.gram: the hand-written parser says yes, the generated one
says no`. That is the test being a test.

The fuzzer found something on the way in, and it was mine: `Text(Expr)` renders a
character literal as `CharRange.Quote(text[0])`, and the value-parameter work started
calling it on a call's arguments — where a mutated file can hold `''`. Normalization
does not stop at the first diagnostic, so a grammar the lexer has already refused still
reaches here, and an empty character literal threw. Answered rather than thrown on.

## Found by writing an example: three defects, two fixed, one named

Sitting down to write `SelectorExample` — postfix chains, `orders[2].lines.total(net)`,
the shape indirect left recursion through a forwarder exists for — turned up three
things in a row, which is what examples are for and why the repository keeps them.

**The question collector never asked about a parameter's type.** `Bracketed(item,
open: char, close: char)` failed the build with "the question collector did not foresee
the type question for 'char'" — an internal defect reaching a consumer as CS8785. The
collector walks a rule's declared type and its body and had never walked its
parameters, because until today a value parameter could only be given a number and
nothing asked the host about `int`. Every test that passes a value uses a resolver that
answers everything, so nothing asked until a real build did. One line, and a reminder
that the permissive resolver hides exactly this class.

**Distributing an alternative shared its tail's nodes.** Unfolding builds one
alternative per source and I kept the tail by reference, deliberately, so that
identity-keyed facts — a `recover`, a binding power — would survive. That is the wrong
half of §19's rule: a node standing in two alternatives has two owners, and whichever
capture layout is computed second clobbers the first. The tail is copied now, node for
node, through the same `CloneAndRewrite` a namespace clone uses, which carries those
facts onto the copies rather than leaving them behind. No test caught it because every
existing case has a tail that captures nothing.

**And the one that is a design question, not a defect.** After unfolding, a capture
written in the tail stands both inside the fold's loop and outside it — the alternative
leading with the rule itself becomes the step, the others stay bases, and they are
copies of one another. A capture under a fold loop is collected, because a step's `=>`
needs that iteration's value rather than the last one's; a rule has one member per
name; so the two spellings of `name` disagree about what it holds and GRAM4007 fires.

That means the feature works for `Call = target: Primary & "()"` and not for `Member =
target: Primary & '.' & name: Name` — which is the postfix chain it was built for. §4.3
says so now, the status table has the row, and a test pins it so that fixing it changes
a test on purpose. The example is parked rather than committed: an example whose
grammar cannot be written is not an example.

What a fix would have to decide: whether a name may be one member with two storages
(scalar outside the loop, collected inside), or whether the unfolding should rename the
step's captures and rewrite the `=>` that reads them — which is the author's own C#,
and this project does not rewrite that.

## Built: the selector example, and the five things it found on the way

`examples/SelectorExample.cs` reads `orders[2].lines.total(net)` as the chain of steps
it is — the postfix shape indirect left recursion through a forwarder exists for, and
the one levels-as-rules cannot write. Writing it found five things, which is what
examples are for and why this repository keeps whole parsers rather than snippets.

**A parameter's type was never asked of the host.** `open: char` failed the build with
"the question collector did not foresee the type question for 'char'" — an internal
defect reaching a consumer as CS8785. The collector walks a rule's declared type and its
body and had never walked its parameters, because until this week a value parameter
could only be given a number. Every test that passes a value uses a resolver that
answers everything, so nothing asked the host until a real build did.

**Distributing an alternative shared its tail's nodes.** Kept by reference on purpose,
to preserve identity-keyed facts — which is the wrong half of §19: a node in two
alternatives has two owners, and the second capture layout clobbers the first. Copied
now, through the same clone a namespace uses, which carries those facts onto the copies.

**A capture in the tail is not a sequence to the author.** Unfolding puts a tail capture
both inside the fold's loop and outside it, and a slot under a loop collects — so the
two spellings disagreed and GRAM4007 refused the whole shape. But the machinery had
already drawn this distinction and named it: a fold step's factory takes `int r`, not
`int[] r`, and both the emitter and the materializer write `member.IsSequence &&
factory.Accumulator is null`. What was missing was a word for it. `CaptureSlot.InFold`
is that word and `Collects` is what the author sees; the merge compares that instead of
the storage. `a.b.c` folds to `a[b][c]`, left-associatively.

**A folded rule's body is not a choice.** `BuildByConstructor` and `PassThrough` read a
rule's alternatives as "the choice, or the body" and guard on "one of them already
constructs" — but §4.3 rewrote a folded body into a *sequence* of the bases and the
loop, so the guard saw no construction where every alternative had one and wrapped the
whole body in another. The fold machinery then met a Construct where it laid out a
Sequence. Only reachable with a resolver that can find constructors, which is why no
test and no probe had ever seen it: both passes skip folded rules now.

**Two machines emitted one scanner twice.** A grammar with two publications has two
machines in one class, and a scanner was named `Scan_trivia` in both — one method
defined twice, in every spaced grammar publishing more than one thing. Tagged like every
other name a machine emits.

And one the example itself got wrong, which was worth the diagnostic it now has: a value
parameter standing where an operand goes lowered to an element set with nothing in it,
so the parse refused everything while naming a set the author never wrote. §4.2 says
where a value may stand; an operand is not one of those places, and it says so now.

The other limit the example met is real and is written into §4.3: several postfix rules
each beginning with the forwarder stay recursive through *each other*, which no rewrite
removes. One rule whose tail is a choice of steps is the same language and folds — which
is how the example is written, and the paragraph explaining why is the part worth
copying.

## Built: DotGram.Parsers, and an expression language that speaks only ET

A new project in `src/`, meant to be packaged: parsers for real formats, written in
`.gram`. The line between it and `examples/` is what each answers. An example shows one
feature and is written to be copied; a parser here answers whether the notation is
enough for a whole specification. And because it is an ordinary project the generator
runs over, it is the second place — after the examples — where a real Roslyn resolver is
exercised at all, which is where three of last week's five defects lived.

The first is a small language that compiles to a .NET expression tree, with parameters,
a block, local variables and `return`. The ruling that shaped it: **every `=>` builds
`System.Linq.Expressions` and nothing else** — no model of this project's own between
the grammar and the API — so that what is proved is that a third-party API can be wired
to a parser as it stands, rather than through a layer written to suit the parser.

Two facts about the language decide the shape, and the first was measured rather than
assumed: `=>` runs after the whole match, children before parents and, among siblings,
**from the end of the text backwards** — the materializer walks the arena by descending
index, and its own comment says why. So a use of `x` is constructed before the parameter
that declares it, and no `=>` can resolve a name. `when`, on the other hand, runs
*during* the match, in reading order. So the declarations are made by guards while
reading and the uses are built afterwards against a table that is by then complete;
twenty lines hold `ParameterExpression`s by name, which is the one thing the API has
nowhere to keep.

Guards run on readings the parse abandons, and that is not a footnote — the first run
died twice on it. `int x` is also `in` and `t` while a repetition gives characters back,
and a failed parse retries the parameter list. So a guard **answers** rather than
throws: a word that is not a type is not a declaration, which sends the parse to the
reading that is one. What it costs is written where the guard is: an abandoned reading
leaves its name behind, and no block shadows.

Two places where the grammar is shaped to the API rather than the other way round, both
deliberate and both documented in the file: a local says its type (`int sum = …`, not
`var`), because `Expression.Variable` wants one where the declaration is read; and a
name means one thing for the whole lambda, because shadowing would need a scope entered
and left around a *construction*, and construction is not where the reading is.

Twenty tests, every one of which compiles the tree and calls it — a lambda that builds
and answers wrongly is the failure a snapshot cannot see. The corpus is unmoved:
Csv 1.53, Feed 1.75, Minimal 1.39, Notation 1.31, Url 1.71.

## Fixed, on a reading: the helpers were the layer, spelled differently

The intermediate model was taken out of the expression language, and the next reading
found it still there — as static helpers. `Arithmetic(op, left, right)` and
`Compare(op, …)` chose a factory by switching on the operator's *text*, and
`TypeNamed(string)` chose a type the same way. Both put a decision one step away from
where C# could see it: a factory that does not exist, or one handed the wrong type, is a
C# error on the grammar's own line (§7.6) — unless a switch over strings turns it into a
run-time exception in a library, which is the compiler kept out of work it can do.

So the choices moved into the grammar, where they are alternatives:

    Additive : @Expression
        = left: Additive & '+' & right: Multiplicative => @(Expression.Add(left, right))
        | left: Additive & '-' & right: Multiplicative => @(Expression.Subtract(left, right))

    Type : @Type = "int" => @(typeof(int)) | "double" => @(typeof(double)) | …

Which pays twice over. Every construction is now a named call the compiler checks. A
word that is no type stops being a declaration by *not parsing*, so the guard no longer
has to answer no about it — and the `int` that a backtracking repetition could read as
`in` and `t` cannot arise at all, because `"int"` is a word literal with §4.6's boundary
woven round it. And the numeric widening went with them: nothing widens on its own, so
`x + 1.5` over an `int` and a `double` is refused by `Expression.Add` itself, in its own
words. A language that speaks only this API has no place to put a conversion the API did
not ask for.

What is left in the host class is a list worth reading as exactly what it is — the
things `System.Linq.Expressions` has nowhere to keep. A `ParameterExpression` is an
identity, and nothing in the API maps a name to one. A `return` is a jump to a label
that belongs to the block rather than to any statement in it. Four methods, and the
grammar says the rest.

1,144 tests green in both configurations.

## Built: a constant says its type, and where the suffix had to go

`1L`, `1m`, `1d`, `1.5m` — the C# suffixes, for the types this language has. One
alternative each, each handing `Expression.Constant` a value of the type it already is,
which is the same rule the rest of the grammar follows.

Two things decided where they are written, and both are worth the lines they cost.

**They are lexical.** Written in the spaced part of the grammar, `Digits & ['L' | 'l']`
would put a seam between the number and the letter and read `1 L` as a constant — §4.5
puts trivia between operands, and a suffix is not an operand of anything. So they live
in the namespace whose trivia is none, with the digits captured and handed back alone:
`decimal.Parse` reads a number, not a number and a letter.

**And they are sets, not literals.** `"L"` after `1` would be refused by §4.6's own
boundary — the weave asks that a word literal not continue a word, a digit is a word
character, and so the guard that keeps `int` out of `internal` would keep `L` out of
`1L`. A set is not a literal and carries no boundary. Which also leaves `m` and `L`
perfectly good names, and there is a test for exactly that.

One diagnostic on the way, and it was right: `text: Long` beside `text: Digits` is
GRAM4007 — a capture of a rule that builds a value and a capture of plain text are two
kinds of member, and §7.3 gives a rule one member per name. Two names, and the report
said so in those words.

## Built: the expression language reads C#'s expressions, and not a dialect of them

The point of `DotGram.Parsers` is a parser someone would ship, and the measure of one
here is that a reader who knows C# never has to ask how this language writes something.
So the whole of C#'s precedence ladder is in the grammar now, in the spec's own order,
with nothing skipped between a name and `?:`: the bitwise three between `&&` and `==`,
the shifts between the comparisons and `+`, `??` and `?:` above `||`, and `+`, `~` and a
cast beside the unary `-` and `!` that were already there. One rule per level, each
calling the next, so the file reads top to bottom as the table reads.

Two operators needed a lookahead rather than backtracking. `|` and `&` each begin a
two-character operator one level out, and `>` begins a shift one level in, so `a || b`
would be read as `a | (| b)` and `a >> b` as `a > (> b)` and only unwound afterwards.
`?!'|'`, `?!'&'`, `?!'<'` and `?!'>'` say why instead of leaving it to §11 to discover.

The literals went the same way — the forms C# has, not the ones that were convenient.
Digit separators, an exponent, a real with nothing before the point, `u`/`U`, `l`/`L`
and both orders of `ul`, `f`/`d`/`m`, the `0x` and `0b` prefixes with separators after
them, `\a \b \f \v \x \u \U` beside the escapes already written, verbatim strings, and
`null`. Two of them are worth the note:

**The integer suffixes are one rule specialized three times.** `Unsigned(N)`,
`SignedLong(N)` and `UnsignedLong(N)` take the digits they suffix as a parameter (§4.2),
so `1UL`, `0xFFUL` and `0b1UL` are three call sites and not nine rules. This is the
first parameterized rule in a parser meant to ship rather than in a test, and it is
exactly the case §4.2 was written for.

**An unsuffixed integer is an `int` where it fits and a `long` where it does not**,
which is C#'s own rule, and the grammar says it as two readings of the same digits:
`int.TryParse` asked in a `when` (§8.1) is what turns the first one down. No helper in
the host class, and no widening decided anywhere but in the BCL.

Three things are deliberately absent, and the file says so in its own header: `null` is
an `object` because target typing is a second pass this file exists to do without;
member access, calls and indexers need a name looked up in another assembly's metadata,
which is a seam this has not been given yet and is the obvious next one to give it; and
an assignment is a statement, never an expression.

1,199 tests green in both configurations.

## Found: two defects in how a repeated text capture is recorded

Writing `\uXXXX` above wanted `t: Hex{4}`, and that reads as the empty string. The
reduction has no rule call in it at all:

```dotgram
Digit = ['0'..'9']
Start : @string = t: (Digit+){2} => @(t)        // on "1234", answers ""
```

**A capture's start is a local, and a local does not unwind.** The open compiles to
`capture0 = p` and the close to an arena entry spanning `capture0..p`. Inside a counted
repetition the next turn runs the open again *before* the previous turn's close is
final: turn 2 sets `capture0 = 4`, its body fails, the give-back door re-enters the
machine at turn 1's close — and the close writes `4..3`. The arena unwinds; the local
does not. `Machine.Materialization.cs` already carries a comment about this family
("a reopened capture's start once survived backtracking in a local") and a `#if DEBUG`
guard against an inverted span, and the guard stays quiet here: the materializer takes
the first start and the last end, which come out `4..4`, an empty slice rather than an
inverted one.

The mechanism for it exists — `_nestedCaptures` keeps the start in the arena instead of
a variable — and is gated on `graph.Recursive` alone, which is one of the two ways a
capture can be reopened before it closes. It looks incomplete for its own case too: the
close marks the open finished by rewriting it in place, and unwinding does not undo a
rewrite, so a door inside the capture would re-enter a close whose open no longer says
it is open. An open entry and a close entry counted like brackets would answer both.

**And the value of a repeated text capture is the extent, where §10 says it is the
join.** `docs/syntax.md:1337` — "repeated text is the text joined":

```dotgram
Start : @string = (t: Digit+ & '-'){2} => @(t)  // on "12-34-", answers "12-34"
```

The extent is right exactly when consecutive turns are contiguous, which is the
condition `HoistTextCaptures` checks before lifting a capture out of its repetition —
and the materializer takes it unconditionally, for captures the hoist left where they
were. Both defects live in the same place and a single change answers them: an open
entry and a close entry per turn, and a value concatenated from the turns rather than
sliced between the first and the last.

Neither is in the way of anything shipped: the expression language writes `HexDigit{4}`
for the four digits, which has no door inside it and so no way to reopen. Recorded here
rather than fixed on the spot, because it changes the capture protocol in the arena.

One smaller thing seen in the same dumps: for an exact count, the loop head emits its
give-back door under the same condition as the jump that already left — `if (repeating
.Value >= 2) goto S8;` and then `if (repeating.Value >= 2) { … }`. Dead, not wrong.

## Fixed: a capture's start goes in the arena, and its turns are joined

Both defects the expression language turned up, and they were one defect and one
unbuilt feature.

**The start.** A capture compiles to `capture{n} = p` at the opening and an arena entry
spanning `capture{n}..p` at the close, and a variable is right for exactly as long as
nothing opens the same capture in between. Recursion was known to do that and already
kept its start in the arena. A repetition does it too, and that was not: turn two runs
the opening before turn one is final, and a door inside turn one — a run to shorten, an
alternative to resume — is a way back into turn one's close, which then reads a start the
parse has given back. `t: (Digit+){2}` over `"1234"` answered `""`, with the entry
recording 4..3.

The protocol the recursive case used would not have carried it either. It marked an
opening closed by rewriting it in place, and an in-place rewrite survives backtracking
that the close which wrote it does not — the same trap `ParserEntry.TurnDone` exists to
avoid. So the opening is now an entry of its own, `ParserEntry.CaptureOpen`, and a close
finds its own by counting openings against closes the way brackets are counted. Both
kinds unwind with everything above the door, so the count is always over a prefix that
makes sense.

**What it costs is nothing, once the question is asked exactly.** Answering "is this
capture inside a repetition whose body can be re-entered" carelessly costs real time, and
both careless answers were measured before they were replaced:

- treating every call as leaving a door put `port: Digit+` over `Digit = ['0'..'9']` in
  the arena, and a full URL a **fifth** slower. A call leaves whatever its callee leaves
  and nothing of its own — the arena's `Call` entry is a frame to restore while unwinding,
  never a state to resume at — so the question is settled per rule, from no rule leaving a
  door, which terminates because a cycle has to pass a repetition or a choice to come back
  round and either answers yes alone.
- counting an optional as a repetition put `(':' & port: Digit+)?` there as well, for a
  second turn that cannot happen: `X?` is how the model spells an optional. With both
  narrowed, the URL round-robin is the baseline again — 104.7 / 278.9 / 107.3 / 175.9 /
  55.7 against 103–107 / 284–288 / 110–115 / 173–181 / 52–54 over three runs a side.

**The join was the other half, and it was a `GRAM4006` all along.** §10 gives a repeated
text capture the text joined, and the materializer took the span from the first turn's
start to the last turn's end — the same thing only where the turns are adjacent. That
shape *was* refused, as "recognized and not built": a capture inside a repetition without
being the whole of what repeats. Except the check looked only at the innermost repetition,
so `(t: A+ & '-'){2}` walked past it and answered `"12-34"`, and whether it was refused at
all turned on whether `HoistTextCaptures` had lifted the capture first.

So it is built instead of refused. The pieces are measured while they are collected, and
the span is taken only where the measurements say the pieces tile it — one slice and one
string, which is what a contiguous capture cost before. Where they do not tile, the pieces
are copied out in reading order. `GRAM4006` now means one thing, a capture inside a
lookahead, and `(t: 'x' & 'y')+` is an ordinary grammar.

1,206 tests green in both configurations, and the corpus snapshots carry the new protocol
where a capture can be reopened — `Minimal` for a recursive one, and nothing in `Url`,
which is the narrowing working.

## Built: a block is an expression, and a name means what it means where it is written

Two changes that turn out to be one, because the second is what the first needs.

**`Expression.Block` is worth its last expression, so this language's block is too.**
`{ int sum = x + y; sum * sum }` — no `return`, no label — and being an expression it
stands wherever one does: `int doubled = { int half = x; half * 2 };`, or `1 + { int t =
x; t * 2 }`. The rule reads the way the API does, which is the whole point of writing a
language against a concrete API rather than around one.

`return` stays, and does what C# does: leave the whole lambda, from however deep in. That
means the label belongs to the lambda and not to a block — which is also the only place
that *can* hold it, since a block is built before the blocks around it and so cannot know
whether it is the outermost. `Returning` is the two lines that put it there.

**And then a name has to mean what it means where it is written.** One block was one
scope and a table by name was enough; nested blocks are not. C# forbids a nested local
shadowing an enclosing one (CS0136), so "one meaning per lambda" was already C#'s rule
for nesting — but two blocks *beside* each other may each declare a `t`, and those are
two variables.

So scopes are pairs of positions. A guard at the end of `Block` records the extent it
read, a declaration records where its name was written, and a use asks which declaration
of that name is visible where *it* is written: the innermost block holding the
declaration must hold the use too, and the innermost such declaration wins.

Recorded by position rather than pushed and popped, and the difference is backtracking. A
guard runs on readings the parse abandons, so a stack would be left holding a scope
nothing is inside — the same hazard that made `Declare` answer rather than throw. A
position is the same fact however many times a reading writes it down, and an abandoned
reading records an extent that no surviving name is written inside.

Shadowing an enclosing block's name is accepted here where C# refuses it, and the nearer
name wins. More permissive than C#, so no valid C# is read as something else, and the
check C# makes is one this has no reason to make.

**The notation needed one thing for this, and the spec already promised it.** §8.1 has
always said the supplied names are in scope inside a `when`, and only `parserText` was
ever handed over. A guard runs before its rule is finished, so `parserSpan` there is the
rule from where it began to where the parse stands — the same extent `parserText` cuts,
unread. That is the only way to record *where* something was read: a `=>` runs children
before parents, so every name inside a block is built before the block is, and a scope
recorded there arrives after the last thing that needed it.

1,216 tests green in both configurations.

## Built: the statement layer, and what measuring it found

`if`/`else`, `while`, `do`, `for`, `switch`, `break`, `continue`, assignment, every
compound assignment C# writes, and `++`/`--` in both positions. The expression language
now names **54 of the 120** factories on `System.Linq.Expressions.Expression`, up from 33.

**What the API allows was measured rather than assumed**, because the question — can an
`if` or a loop be worth something? — has a real answer either way:

| | |
| --- | --- |
| `Condition(int, int)` | `Int32`, and the branches must agree *exactly* |
| `Condition(a, b, typeof(void))` | `Void`, and takes branches that agree on nothing |
| `Loop(body, voidBreak)` | `Void` |
| `Loop(body, intBreak)` | `Int32` — and the built loop ran and answered |
| `Switch` with no default | refused: "Default body must be supplied if case bodies are not Void" |
| `Throw(ex, typeof(int))` | `Int32` — a throw may stand where a value is wanted |

So an `if` with an `else` is worth what its branches are worth, and is written that way:
`int n = if (c) 1 else 2;`. A loop is not, and cannot be in a language shaped like C#: the
type comes from the break label, so only a loop with no ordinary way out could have one —
the ordinary way out would have to carry a value too, and C#'s `break` carries nothing.

**Where the line between the grammar and the host is.** `Expression.Condition` is one
factory with two answers, and which one a given `if` meant is not in the syntax — `if (c)
a else b` reads the same whether its value is wanted or thrown away. That is a question
about this API and not about the language, so the host answers it and the grammar stays
the shape every language writes. The rule the file now states: the grammar carries what is
general, the host carries what is specific to the thing it is pointed at. A grammar that
carried `System.Linq.Expressions`' own distinctions would be a grammar for one API, and
whether the notation can be pointed at somebody else's is the whole question this parser
exists to answer.

Same division for `break`: which loop it leaves is where it is written, which is the
question a *name* asks, answered the same way — the guard records the loop's extent while
the text is read, the jump looks its label up when it is built. A `switch` records an
extent of its own, because a `break` in a case leaves the switch and not the loop, and
there is a test that would answer 1 instead of 8 if it did not.

### Found: two routes to one construct is what makes a parse exponential

A `Block` reachable both as a statement and as a `Primary` is read once as each, at every
level of a nest of them. So is an `if`. Measured, before anything was done about it:

| | |
| --- | --- |
| `if … else if … else if … else` (three deep) | **1,646 ms** |
| five nested braces | **428 ms** |
| two braces inside an `if`'s branches | **never finished** |

Giving each construct one route — `Control` and `Block` reachable only from `Statement`,
and a `Value` rule naming them where a value position may hold one — takes all three to
**under a millisecond**, and a repeat run shows the growth is linear in nesting: 191, 336,
387, 471 µs for one to four `else if`s.

The cost is that `1 + { … }` is no longer written: a block and an `if` stand where a value
is *expected* — an initializer, a `return`, a branch, the last thing in a block — and not
as an operand in the middle of one. That is a measurement rather than a taste.

A second multiplier went the same way. Eleven assignment alternatives each began `target:
Unary`, so each read a whole operand — possibly a whole block — before finding out it was
not the one. The left side of an assignment in this language can only be a name, since
there is no member access and no indexer, and eleven words is nothing.

### And three defects of this parser's own

**`TryParseLambda` does not forget the parse before it.** Every list the host keeps is
keyed by position, so a second parse of a different text finds the first one's blocks at
overlapping offsets and resolves names against them. `Parse` cleared what there was; the
generated `TryParseLambda` is the parser and nothing else, and knows nothing about a parse
beginning. `TryParse` beside it is the one to call, and `Begin` clears all six lists —
three of which this change added and none of which the old `Parse` knew about.

**A block's variables were read out of its assignments**, which was right while a
declaration was the only thing that could assign, and stopped being right the moment `a =
1;` was a statement: the same variable was collected twice and `Expression.Block` said so.
They are the declarations the block holds now — the ones whose innermost block is this one
— which is the scope machinery already there, asked a second question.

**And a `return` beside a block's value put the value out.** `Returning` wrapped the body
as `Block(body, Label(target, default))`, so a lambda that ends in an expression computed
it and answered `default` instead. The label takes the body instead: control reaching the
end of the body arrives at the label, and what it is worth there is what fell into it.

1,247 tests green in both configurations.

## Built: a type resolver in the host, and everything a name in metadata unlocks

`Exception`, `Math.Max(x, 7)`, `s.Length`, `s.IndexOf("c")`, `new Exception(s).Message`,
`new int[] { 10, 20 }[1]`, `o is string`, `o as string`, `try`/`catch`/`finally`, `throw`.
The expression language now names **71 of the 120** factories on
`System.Linq.Expressions.Expression`, up from 54.

**The resolver is the host's and needed no seam.** The one reason it might have needed
one — a position to report against — was already there: `parserSpan` reaches a `when`, and
has since the scopes work. Type names come from the *parsed text*, not from the grammar,
so `ISymbolResolver` (which the generator has for a grammar's own `@Name`) has nothing to
say about them. What the host holds is a list of namespaces to look in, which is what a
`using` is, and no grammar can carry one for an API it has not been pointed at yet.

The keywords stay written as `typeof(int)`, where the C# compiler reads them. A name goes
through a guard, which also settles the ambiguity C# needs a section of its own for:
`(Foo)x` is a cast where `Foo` names a type and a parenthesized expression where it does
not, and the guard answering no is what sends the parse to the other reading. A dotted
name is greedy and gives a part back at a time, so `System.Math.Max` resolves whole and
`s.Length` resolves not at all and is read as a name and a member of it.

**Most of the rest is the API's own resolution.** `Expression.Call` takes a method by name
and chooses the overload; `Expression.PropertyOrField` answers the same question for the
other two. What is left in the host is what the API has no by-name form of: a constructor
(`Expression.New` wants a `ConstructorInfo`), a static property or field (there is no
static `PropertyOrField`), and the two places where the *operand's type* picks the factory
— an array's `Length` is `ArrayLength` and its element is `ArrayIndex`, where every other
type's are properties, and `string` calls its own indexer `Chars` rather than `Item`.

That last one could not have been a guard, and finding out why is worth writing down: a
guard runs while the text is read, and the operand of a left-recursive fold is not built
until long after. So the only place that can ask an operand what it is, is a `=>`.

### Found: two more generator defects, both by writing the grammar

**A capture whose rule leads back into its own fold is emitted uncompilable.** Two lines:

```dotgram
Postfix : @Expression
    = target: Postfix & '[' & index: Expression & ']' => @(Expression.ArrayIndex(target, index))
    | p: Primary => @(p)
```

`Construct_Postfix_1(Expression[] index)` — the fold's own operand dropped from the
signature, and the capture typed as a sequence. The `=>` then names `target`, which is not
a parameter, and the *consumer's* build fails with CS0103 in a file they did not write.
`index: Primary`, `index: Name`, `index: Dec` and `index: Arguments` are all fine in the
same position; only the rule that reaches `Postfix` back is not. Worked around by writing
the index through a list rule, the way `Arguments` already was — which reads better anyway,
since an index is a list and a two-dimensional array now needs no second rule for it.

**And the capture-start condition from earlier today was too narrow.** `Math.Max(1, 2)`
threw `ArgumentOutOfRangeException` out of the parser. The condition said a capture keeps
its start in a variable unless a *repetition* could reopen it; the general case has no
repetition in it at all:

> the close runs, the parse goes on, the same rule is read again somewhere else and writes
> the variable, and then a failure unwinds to a door inside the first reading and runs its
> close again — with a start belonging to the other reading.

Here the capture was `name: TypeName` in the rule that resolves a dotted name, the door
was the `*` inside `TypeName`, and the second reading was the same rule tried again inside
`Math.Max`'s argument list. So the question is only whether the capture's *body* leaves a
door — where it leaves none there is no way back into the close, which is still most
captures, `port: Digit+` over a set of digits included.

**What it costs, measured.** Six of Url's capture starts move into the arena. The URL
round-robin, three runs a side: 110–113 / 288–291 / 114–117 / 175–179 / **62.2–62.6**
against 103–107 / 284–288 / 110–115 / 173–181 / **52–54**. Two of the five are inside the
noise, two are up about 3%, and the last — the input that fails and so backtracks hardest —
is up 17%. That is the price, and it is the right way round: the defect it buys off threw
an exception out of a shipped parser.

1,263 tests green in both configurations.

## Fixed: a fold stops naming its own loop when a pass rebuilds it

The defect the last entry only worked around, and the reduction is three rules:

```dotgram
C : @string = t: ['a'..'z']+ => @(t)
E : @string = e: P => @(e)
P : @string = t: P & '[' & m: E & ']' => @(t + m) | p: C => @(p)
parse E
```

`Construct_P_1(string[] m)` — the fold's own operand missing from the signature, and the
capture typed as a sequence. The `=>` then names `t`, which is not a parameter, and the
**consumer's** build fails with CS0103 in a file they never wrote.

**`E` forwards to `P`, and that is the whole of it.** `CollapseTransparent` replaces the
call to a forwarder, rebuilding the sequence that held it — and with it the repetition
around that sequence, which is the loop `P`'s fold named by reference. `CaptureLayout`
recognizes a fold's loop with `ReferenceEquals`, so from that moment it recognized nothing:
every capture in the tails came out `IsSequence` with `InFold` false, and the accumulator
went with it.

The pass's own comment had the principle exactly right — "everything before this pass has
already keyed facts by node reference … a clone of an untouched subtree would orphan them"
— and rebuilt nodes without handing those facts on. `Carry`, which the recursion pass
already had for binding powers and recoveries, now carries the fold's loop and the tail of
each of its accumulators as well, and `Inline` calls it on everything it rebuilds. The
climb's levels go the same way, for the same reason.

Two tests: the factory keeps its operand, and `a[b][c]` reads to `abc`.

`HoistTextCaptures` and `SpaceLists` were never bitten by this because both skip a rule
that owns a fold outright. That is a guard against the same hazard, arrived at from the
other side — and now the hazard itself is answered rather than avoided.

### Not done: narrowing the capture-start condition back

The other half of what was planned, and the measurement says leave it. What the widened
condition costs is inherent to putting a start in the arena: one more entry per capture,
and more to pop when a parse unwinds — which is why the input that costs most is the one
that fails. The narrowings that are *sound* are small:

- a capture at the head of its rule needs neither a variable nor an entry, because its
  start is the rule's own and the call entry already holds it and already unwinds. Two of
  Url's six moved captures are that shape — and a rule inlined at a call site (§7.3's
  sited calls) compiles its body under the *caller's* call entry, where that reasoning is
  wrong, so the narrowing needs the site interaction thought through before it is safe.
- "this rule is entered at most once per parse" would cover the rest, and is exactly the
  kind of whole-grammar reasoning that produced the defect this fixed. Entries are never
  popped on success, so every door of an earlier activation stays live for the whole
  parse; a second entry anywhere clobbers.

Three of the five URL inputs are inside the noise or up ~3%. Chasing the fourth with an
analysis of that kind, days after two capture defects that both came from reasoning of
that kind, is the wrong trade. Written down here so the next person does not have to
re-derive why.

1,265 tests green in both configurations.

## Built: generic types, invocation, initializers, and writing to what was read

`Func<int, int> f` and `f(x)`, `int[] a` and `a.Length`, `new Exception() { Source = s }`,
`new List<int>() { 10, 20, 30 }`, `a[1] = 7`, `e.Source = "set"`. **77 of the 120**
factories, up from 71 — and two of the six close a gap rather than a counter: nothing
could be written to but a name.

**A generic type is named by arity in metadata** — `Func`2` — which is one more thing
about the runtime than about the language, and so is the host's. The grammar reads a name,
a `<`, some types and a `>`, which is what every language calls one. It asks nothing while
reading, and could not: what a guard may look at is what the text said, and the arguments
are types the `=>` has not built yet. It needs no guard either, since nothing else here is
a name followed by `<`, a type and `>`.

**Reading an element and writing one are two nodes**, and the API is right to keep them
apart where C# does not: `ArrayIndex` answers with a value and cannot be assigned to,
`ArrayAccess` answers with the element. Which `a[0]` means is decided by which side of the
`=` it stands on — something the grammar knows and the API cannot, so the grammar says it
by calling two rules.

**And the parameters of a lambda are `Expression.Parameter` now**, where a block's locals
stay `Expression.Variable`. The API makes the same node either way and keeps two names for
it because a language does; this one reads them apart in two rules, so it can say so.

### Found: eleven alternatives reading an index eleven times

The same shape as the `else if` chain a few entries up, and it wanted the same discipline.
Every compound assignment is an alternative of its own — that is the doctrine, one
alternative per operator, each naming its factory — and each of the eleven began by
reading the target. With an element as a target, the target contains an *expression*:

| | |
| --- | --- |
| `a[0] = 1` | 1.6 ms |
| `a[a[0]] = 1` | 5.0 ms |
| `a[a[a[0]]] = 1` | 55 ms |
| `a[a[a[a[0]]]] = 1` | **826 ms** |

Reading the index in one alternative rather than eleven takes the last to **3.6 ms**. What
it costs is that a compound assignment writes to a name or a member of one and not to an
element: `a[1] += 7` is not written here, and is refused rather than misread.

That is the second time this session that a cost turned out to be a construct readable
more than one way, and both times the fix was to leave it one way. Worth stating as a rule
of thumb for grammars in this notation: **ordered choice is cheap when the alternatives
disagree early and expensive when they agree for a while**, and an alternative that reads a
whole operand before testing one character is the expensive kind.

1,271 tests green in both configurations.

## Found, on being asked "826 ms?": the whole ladder was exponential

The number was real — re-measured as a single parse, 904 ms, with the run's wall clock
agreeing. But checking it found something much worse than the shape it was about.

**A plain nested expression was exponential too.** `(((0 + 1) + 1) + 1)` and so on, no
assignment, no index, nothing but parentheses and `+`:

| depth | before | after |
| --- | --- | --- |
| 6 (50 chars) | 446 ms | 0.23 ms |
| 7 (56 chars) | 1,822 ms | 0.29 ms |
| 8 (62 chars) | 7,395 ms | 0.39 ms |
| 9 (68 chars) | **29,977 ms** | **0.43 ms** |

Factor 4.06 per level, settled. A sixty-eight-character expression took half a minute, and
an eighty-character one would have taken a working day.

**It was two rules.** `Conditional` and `Coalesce` are the only right-associative levels of
the ladder, and both were written as two alternatives:

```dotgram
Conditional = test: Coalesce & '?' & then: Conditional & ':' & otherwise: Conditional
            | c: Coalesce
```

The first reads its whole operand to look for a `?` that is almost never there; the second
reads it again to hand it on. Each such rule doubles per level of nesting, and two of them
multiply the cost of a parenthesis by four. Every level below them is left-recursive, and
§4.3 folds those — a fold reads its operand once by construction, which is why the twelve
levels that *are* folds never showed this and the two that are not showed all of it.

Written with the tail optional instead — `test: Coalesce & ('?' & … )?` — the operand is
read once and the same thirty seconds is 0.43 ms. What the host gains is a null test:
`Chosen` answers the condition where no tail was written.

Three more shapes were measured after it, and two of them had it too:

| | before | after |
| --- | --- | --- |
| nested `new` (256 chars, nine deep) | 1,001 ms | 0.53 ms |
| nested `if`/`else` branches | already linear | 0.68 ms |
| nested indexed assignment | 10,373 ms | 4.09 ms |

`new` was three alternatives reading `Type & Arguments` before diverging on what stood in
the braces after them — the same mistake, factor 3. Now one alternative with the
initializer optional, and the host chooses among `New`, `MemberInit` and `ListInit` by what
was written.

Nested indexed assignment is the one that is still not linear: 1.9 per level, because the
alternative that writes to an element reads a whole index before testing the `=` after it.
At nine deep that is 4 ms, so it is left as it is and written down here.

### What this is really about

Three times this session a cost turned out to be **an alternative that reads a whole
operand before testing one character**, and each time the fix was to stop it: one route per
construct, one reading of the target, one reading of the arguments. That is a rule of thumb
for writing grammars in this notation, and it is already in the file above.

It is also a missing generator feature, and the honest name for it is **left factoring**.
`A = X & p | X & q` can read `X` once and choose after it; nothing in the notation stops
the author writing the natural form, and nothing in the generator saves them from it. Every
workaround above trades a factory named in the grammar for a null test in the host —
`Chosen`, `Coalesced`, `Made` — which is precisely the trade a generator that factored the
common head would not ask for. It belongs on the list beside the lexical layer.

1,271 tests green in both configurations.

## Built: GRAM4016, and the two shipped examples it caught in its first run

The diagnostic the last entry asked for. Two alternatives that begin with the same
operand, where that operand leads back to the rule holding them — the shape whose cost
compounds per level of nesting rather than merely doubling once.

**It found the trap in the repository's own examples before it found anything else.**
`DecimalCalculatorExample` and `ExpressionTreeExample` both wrote

```dotgram
Power = left: Primary & '^' & right: Unary | value: Primary
```

and `Primary` leads back to `Power` through its parentheses. Measured on the shipped
calculator, one parse each:

| parentheses deep | before | after |
| --- | --- | --- |
| 11 (45 chars) | 1.9 ms | 0.029 ms |
| 13 (53 chars) | 5.4 ms | 0.035 ms |
| 16 (65 chars) | **29.6 ms** | **0.052 ms** |

Doubling per level before, flat after — and these are examples written to be copied.

**`docs/syntax.md` §4.3 taught the same shape**, in the paragraph explaining that
associativity is which side the recursion is on. It still explains that, and now also
explains why the left-recursive one costs nothing and the right-recursive one has to be
written with the tail optional: a left-recursive rule is rewritten into a loop over its
tails, so its head operand is read once however many alternatives there are; a
right-recursive one has no such rewrite.

### Why it reports rather than rewrites

Because the two forms are not the same grammar, and the difference is sometimes the
point. Two alternatives prefer every reading of the first over any reading of the second,
so a shared operand that *can give back* will give back to let the rest of the first
alternative fit. Measured, on `a/b/c`:

| | |
| --- | --- |
| `d: Segments & '/' & f: Name \| d: Segments` | `dir=a/b file=c` |
| `d: Segments & ('/' & f: Name)?` | `dir=a/b/c file=-` |

That is how the last segment of a path is split off, and no optional tail says it — the
notation has no lazy quantifier to ask for a shorter reading. So where the operand can
give back, which form to write is a decision about meaning and only the author has it.

Where the operand **cannot** give back it has one reading, the two orders hold the same
one thing, and the rewrite is exact — which is also when the diagnostic says nothing,
because there is nothing to weigh. It is why the emitter has always factored runs of
literal alternatives and why doing the same to a rule call in general would be wrong.

`Doors` moved out of `Machine` into `Grammar/Model` for this: whether something leaves a
way back into the middle of itself is now asked by two very different questions — whether
a capture may keep its start in a variable, and whether two alternatives mean the same as
one with an optional tail — and one of them is asked while the grammar is still being
checked.

### Not built: factoring where it is safe

Also planned, and dropped on the measurement rather than on the effort. Factoring is
invisible exactly where the shared operand leaves no door — and something that leaves no
door cannot be recursive, because going round a cycle has to pass a repetition or a
choice, and either of those *is* a door. So the cases where factoring is safe are exactly
the cases where the cost cannot compound: a constant factor, on a shape the emitter
already factors when it is written as literals. The exponential cases are precisely the
unsafe ones. Writing that down is worth more than the code would have been.

1,275 tests green in both configurations.

## Found: the choice of literals does not sharpen, and the position is a selector

`Sharpen` moves `p` to the character of a literal that did not fit, on the branch where
the comparison has already failed. Asked what it is for, the answer is not the caret. It
is that `Fail:` keeps the **furthest** failure and reports that one's expectation, and
sharpening is what lets a partial literal match count as having got somewhere.

```dotgram
A = "abcdef" & Tail
B = "abq" & Tail
Tail = ['0'..'9']+
Start = A | B
```

| input | reported |
| --- | --- |
| `abcdez` | `at=5  Expected "abcdef"` — five characters in beats two |
| `abqz` | `at=3  Expected ['0'..'9']` — that alternative got past its literal |
| `zbcdef` | `at=0  Expected "abcdef" or "abq"` — neither got anywhere, so both are named |

Without it the first line would be `at=0  Expected "abcdef" or "abq"`: everything that
could have stood there, instead of the one the author almost wrote.

**And the same grammar written as one rule does not get it.** A choice of literal
alternatives is compiled by `LiteralGroup`/`CompileLiterals`, the shared-prefix form, and
that path does not sharpen:

```dotgram
Start = "abcdef" & 'x' | "abq" & 'y'      // at=0  Expected "abcdefx" or "abqy"
```

So the quality of the message turns on which shape the author happened to write, in
exactly the place — a choice between literals — where choosing between expectations is
what the mechanism is for. The trie knows how far it got; it just does not say.

Fixed in the entry below.

## Fixed: a run of literals now fails where the same choice of rules does

`SharpenAll` — the walk of a trie over the alternatives of a literal run, on the branch
where all of them have failed. It moves to the deepest character any of them agreed with
and names the ones still agreeing there, which is what the same choice written one literal
per rule already reported. The two shapes now answer identically:

| input | both shapes |
| --- | --- |
| `abcdezz` | `at=5  Expected "abcdefx"` |
| `abqzzz` | `at=3  Expected "abqy"` |
| `abczzzz` | `at=3  Expected "abcdefx"` |
| `zbcdefx` | `at=0  Expected "abcdefx" or "abqy"` |

The run used to report `at=0` and name both, for all four. That also closes the gap the
code's own comment recorded — "nothing here narrows a subset the way a real trie would".

**Two things came out of doing it, and both are the sort a measurement finds.**

The position may only move where the failure goes to `Fail:`. A prefix conflict — `"p" |
"q" | "pr"` — splits one grammar-level choice into several runs chained through `fail`, and
the run jumped to reads from where this one started. Moving `p` there broke the `Filter`
example outright, which is how it was found; the guard is `fail == Fail`, and the
shared-prefix sharpen above it, which had the same hazard latent, now carries it too.

And the walk goes out of line. Written into the recognizer it sat between hot states and
cost the URL corpus five per cent on inputs that never reach it — a method of ten thousand
lines has nowhere to put anything without moving something else. Six alternating runs
against a worktree of the parent commit put the remaining cost at a few per cent on one
input, the one that fails, which is the path the walk exists for. The first reading of
"five per cent" was an ordering artefact: base always measured first, and the numbers drift
upward through a session. Reversing the order took most of it away.

1,279 tests green in both configurations.

## Built: `context`, the state a parse works out and has nowhere to keep

`ExpressionLanguage` had seven `[ThreadStatic]` fields and a `Begin()` to clear them. That
is the shape a grammar falls into when a `when` needs to remember something: a static, and
then a way to reset the static, and then the quiet knowledge that two parses on one thread
must not overlap. It works and it is wrong, and it was written by the parser this notation
is meant to make unnecessary.

So a grammar may now declare what it needs to remember:

```dotgram
context : @Names
```

**The caller makes one and hands it over.** Every publication of a grammar that uses a
context takes it as a second parameter, and it reaches whatever names it — a `when`, a
`=>` — by exactly the rule the supplied names of §8.2 already follow: **a hook that does
not name `context` is not passed it.** A grammar that declares one and never uses it emits
the same code as a grammar that declares none, publications included. That was worth
having: it means adding the declaration costs nothing until something reads it, and it
means the test for "is this threaded correctly" is a `DoesNotContain`.

Where it goes is fixed. After the `using` block, outside every namespace, once
(`GRAM3013`, `GRAM3014`). A context inside a namespace would be a context for part of a
parse and there is no such thing — the object the caller passes is passed to all of it.

**The word was free because it had been given up.** `context` was this notation's keyword
for what is now `namespace`, renamed earlier this session; the parser tells the two apart
by what follows, so `context = 'x'` is still an ordinary rule called `context` and there is
a test that says so.

**What it deliberately is not** is somewhere to put state that has to be *undone*. Nothing
here unwinds: a `when` runs on readings the parse goes on to abandon, and what it wrote
stays written. What belongs here is what can be written twice without harm. The other half
— a value that holds *while* something is being matched and is restored when the parse
backs out of it — is the scoped state agreed next, and it is a different mechanism: a
declared name whose value fits in an `int`, carried in the arena entry itself so that
`Fail:` restores it the way it already restores `TurnDone`. `checked`/`unchecked` is what
wants it.

Six tests: the two refusals, the rule that is still a rule, the threading through
publication, guard and factory, and the two that say a grammar without a context is
untouched.

## Built: `with state`, the other half of what a parse has to remember

`context` (above) is state a parse accumulates and keeps. This is the other kind: state
that holds **while** something is being read and is gone after it — `checked(...)`, "inside
a loop", "inside a query". It was designed in conversation and the design changed twice
under questioning, both times toward less machinery.

**First sketch, mine:** declared names, one slot per name, a save-and-restore record in the
arena per setting, restored by `Fail:` the way `TurnDone` is. **First correction, in
conversation:** that is a variable-declaration mechanism inside the parser — scoping, name
resolution, defaults, per-name diagnostics, and an answer owed for how it interacts with
`namespace (...)` cloning. One array of marks that the hook itself inspects is a fraction
of the work and puts the interpreting where the meaning already lives.

That correction turned out to answer the question the declarations were introduced for. I
had asked "how does a hook tell mark A from an unrelated mark B lying over it", and answered
it with names. The hook answers it by itself: it reads back from the end for the nearest
value of its own concern and walks past everything else. **Second correction:** and the type
need not be `object` either — the grammar declares one type, every mark is of it, and what
tells two concerns apart is their values. No boxing, no per-name table, one line of
declaration.

**What was kept from the first sketch is the one thing that made it cheap.** A mark is only
ever read by a `=>`, and a `=>` runs at `Accept:`, over an arena that by then holds only
what was accepted. So nothing has to be restored when a reading is abandoned — being gone
*is* the restoration — and nothing is spent while the text is read. `Fail:` gained two
entry kinds to step over and no work.

```dotgram
state : @Overflow

Checked : @Expression = "checked" & '(' & e: Expression with state @(Overflow.Checked) & ')' => @(e)
Additive : @Expression = … => @(Arithmetic.Add(left, right, parserState))
```

`with state`, chosen over `under`/`marked`/`in` after the alternatives were laid side by
side: it joins the `with` family, where all three extents already mean "on this extent,
something is different", and the qualifier answers the objection to reusing the bare word —
a reader can tell at the site which mechanism is running. `ParseWith` became a loop, so the
two compose in either order, and the notation's own grammar says so with `With* & Marking*`.

**How it is built.** `Node.Marked(Body, Text)` — a node, not a fact keyed by node identity,
because a fact carried by identity is a fact a rebuilding pass can drop, and this
implementation has already had that defect once (`CollapseTransparent.Inline`, earlier this
session). The node is transparent to every analysis and the ~25 sites that had to say so
were found by hand: the switches over `Node` are not exhaustive — they have `_ =>` defaults
— so the compiler said nothing, which is worth knowing about this codebase. What stands in
for the compiler is a differential test: the same grammar with marks and without, run over
inputs that succeed, that fail, and that fail after backtracking, agreeing on the answer,
the value, the message and the position.

Compiling a mark is two arena entries and no dispatch. Reading them is one pass over the
accepted arena filling one `int[]`: at a `StateSet` the slot holds the mark enclosing it,
and everywhere else the innermost mark standing over that slot. The two readings never meet
— nothing else sits at a `StateSet`'s own index — so one array answers both "which mark is
in force here" and "what encloses this mark", and a factory follows the chain instead of
scanning. The span it is handed is a view of one buffer the walk reuses.

**Three defects came out of it, one of them mine from the commit above.**

`parserState` was not in `SuppliedNames`, so a factory naming it made the *rule* unbindable:
the reference lowered to an unresolved element, and the whole call site compiled to a
failure. The symptom was a truncated machine with no obvious cause, and a long bisect
through the wrong suspects — the fast paths, the site optimization, the analyses — before
printing the lowered graph, which said `v: [Value]` and gave it away in one line. Print the
graph first.

`Tree.Children` — "the one place traversal is defined" — had no case for the new node, so
the binder never descended into a marked operand. Same symptom, same cause, found the same
way. A new `Expr` case has exactly two obligations and this is one of them.

And `Machine.Sites`'s `ComputeSitedValued` did not refuse a callee whose factory names
`context`. A sited call's arguments are built from the spans the site recorded and nothing
else, so such a callee emitted a call missing an argument — an error in the consumer's
build, not in ours. That one shipped in the commit above and is fixed here alongside
`parserState`, which would have fallen into the same hole.

The notation's own grammar was two features behind — it never learned `context` either —
and now reads both declarations, the mark, and a rule that happens to be called `context`
or `state`. Seven cases assert that the hand-written parser and the generated one agree on
all of it.

1,344 tests green in both configurations, and the `checked(1 + unchecked(2 + 3) + 4)` case
is one of them: one `Sum` rule, read one way, building two different trees.

## Built: `checked` and `unchecked`, which is what `with state` was for

The mark had tests and no user. Now it has one, and the first thing to say is that the
grammar barely changed: two alternatives in `Primary` that place a mark, and eight `=>`
that ask what they stand under. **85 of the 120** factories, up from 77 — the eight nodes
`System.Linq.Expressions` has two of.

```dotgram
| "checked"   & '(' & inner: Expression with state @(Reading.Checked)   & ')' => @(inner)
| "unchecked" & '(' & inner: Expression with state @(Reading.Unchecked) & ')' => @(inner)
```

`Additive` is still one rule with one alternative for `+`. What it builds now goes through
the host — `ExpressionLanguage.Add(left, right, parserState)` — and the host picks the
overload, which is exactly the division this class already ran on: the grammar says what a
`+` is, in the word every language uses for it, and the host says what a `+` turns into
here. A conditional written eight times into the notation would have put a C# question
where a reader is looking for the shape of an expression.

**The enum is `Reading`, not `Overflow`.** §7.8 says one type for the whole grammar and
values for the concerns, so naming it after the only concern it has today would have
invited the wrong second use. `Checked` reads back to the nearest value of *its* concern
and walks past anything else, which is the idiom §7.8 documents and the shape a second
concern will need.

**What the design cost here was nothing measurable, and the reason is structural rather
than lucky.** A mark makes `Silent` false on its operand, which pushes the surrounding
publication off the flat rendering; the worry was that this grammar would pay for it. It
does not — `ExpressionLanguage` emits one `Recognize_DotGram` and no flat method at all,
and always did. There was nothing on that path to lose. Two `StateSet` sites in 24,872
lines, and eight factories that read them.

### Two defects, one of them a hole the previous commit dug

**The fold assembled its own arguments.** `MaterializeFold` knew about the matched text and
the span and about none of the three supplied names added since — `parserInput`, `context`,
`parserState` — so a left-recursive rule whose factory named any of them emitted a call
missing an argument. `Additive` is left-recursive, which is how it surfaced immediately.
Both paths now call one `Supplied`, because a factory's parameters are written once in
`CSharpEmitter` and what fills them has to be written once too.

**A keyword was a keyword only by the order of alternatives.** `Postfix` reads
`Name & Arguments` before `Primary` is reached, so `checked(x + 1)` was an invocation of
something called `checked` and the failure surfaced in the host as "nothing named
'checked'". Ordering had been enough until now because every other keyword's own reading is
tried first where it can occur; a keyword followed by a parenthesized expression is
indistinguishable from a call until something says the word is not a name. So `Name` now
refuses a `Keyword`, which is what C# means by the word and what the grammar should have
said from the start.

That found a third thing worth knowing about the notation: **§4.6's woven word boundary
does not reach inside a lookahead.** `?!Keyword` refused `checkedTotal` for beginning with
`checked`. The boundary is written out in the rule instead — right either way, since what
`Keyword` means is "one of these words, whole" — and whether the weaving should reach into
a lookahead is left as a question for the notation rather than answered as a side effect of
this.

### And a message that read like a bug

Chasing the above turned up `Expected '<' or '<'.` — not a mangled expectation but a
duplicated one. A failure keeps everything recorded against the furthest position, and a
language this size has several sites wanting the same character; the `<` that opens a type
argument list is written in more than one rule. They were joined as they came. The message
builder now names each thing once, which every generated parser gets. The test is a
property — no term appears twice — rather than an assertion about the text, because *which*
things are named at that position is §7.5's question and a separate one.

1,356 tests green in both configurations.

## Built: nested initializers, and what the remaining 32 factories turn out to be

`MemberBind`, `ListBind` and `ElementInit` — the three the API keeps for an initializer
that goes a level deeper than an assignment.

```csharp
new Holder() { Name = "a", Inner = { Count = 1 }, Items = { 5 } }
new Dictionary<int, string>() { { 1, "one" }, { 2, "two" } }
```

The difference from an assignment is the whole point of the two nested forms: no `new`
stands after the `=`, so the object the member already holds is the one initialized. And
`ElementInit` exists because `Add` is not obliged to take one thing — a dictionary's takes
two, which a list of values cannot describe.

**One route, three answers, decided where the type is.** `Binding` reads `Word '='` once and
then whichever of the three followed, for the reason `Primary`'s `new` already gives: three
alternatives would read the name and the `=` three times over and the third reading holds a
whole expression. Which one was written is which of the three fields is not null, and that
question is answered in `Bound`, one step further in — a nested initializer is read against
the *member's* type, and the member is not known where its braces are. The same deferral
the outer initializer already made, made once more.

**88 of the 120.**

### And that is the ceiling, which is worth saying rather than counting toward

The remaining 32 are not a backlog. Named, because "coverage" is only a claim if the gap is:

* **Six are not nodes.** `GetActionType`, `GetDelegateType`, `GetFuncType`,
  `TryGetActionType`, `TryGetFuncType`, `SymbolDocument` return a `Type` or a document, not
  a tree.
* **Eight are the by-kind entry points** — `MakeBinary`, `MakeUnary`, `MakeIndex`,
  `MakeMemberAccess`, `MakeCatchBlock`, `MakeTry`, `MakeGoto`, `MakeDynamic`. They exist for
  code that decides the node kind at run time; a grammar knows which node it means and names
  it. Reaching them would mean the grammar had stopped saying what it read.
* **Ten have no C# syntax to read.** `Power` and `PowerAssign` (C# has no `**`), `IsTrue`
  and `IsFalse` (the operators a type defines, not something written), `Increment` and
  `Decrement` (C#'s `++` is an assignment, which is `PreIncrementAssign`), `TypeEqual` (`is`
  is `TypeIs`), `Unbox` (what a cast compiles to, not what anyone writes), `TryFault` (C#
  has no fault handler), and `IfThenElse` — the void form, where a block here has a value,
  so an `if`/`else` means `Condition` and `Chosen` says so.
* **Three are debugging** — `DebugInfo`, `ClearDebugInfo`, `RuntimeVariables`.
* **Two are the same node under another name.** `Expression.Equal` on two reference types
  already compares references, so `ReferenceEqual` and `ReferenceNotEqual` would be a second
  spelling rather than a second meaning.
* **Three would need more language than this has.** `Dynamic` is a binder story of its own,
  `Quote` wants a nested lambda standing where a value goes, and `Goto` wants labels —
  which want a scope, the way a block's locals do, and that is a piece of design rather
  than a factory. It is the one genuinely open item and it is parked on purpose.

So the sweep is finished rather than paused. What the exercise was for is answered: a
notation that reads as C# does, over an API it was not designed around, reaches everything
that API has a syntax for — and the 657 lines of grammar it took say where the notation was
thin, which is what `docs/next.md` above this line is mostly made of.

1,362 tests green in both configurations.

## And the thing the count could not see: an extension node

Raised in conversation against the paragraph above, which was overstated. `System.Linq
.Expressions` has a whole mechanism that is not one of the 120 and never could be, because
it is reached by **deriving** rather than by calling: an extension node — a class of your
own over `Expression` whose `NodeType` is `ExpressionType.Extension`, that says through
`CanReduce`/`Reduce` how to become ordinary nodes when something finally asks. There is no
`Expression.Extension(...)` to count. The count stands at 88 of 120 and the claim built on
it did not.

**Nothing has to be added for a grammar to reach it**, which is the interesting half. A
`=>` is C#, a rule's `: @T` is a C# type, and a class over `Expression` is an `Expression`
— so `=> @(new ClampExpression(value, low, high))` was always legal and nothing in the
notation or the generator has an opinion about which side of the API a construction came
from. That was worth proving rather than asserting, so
`examples/DotGram.Examples/ExtensionNodeExample.cs` now does: a grammar reading
`clamp(x, 0, 10)`, a node the API does not have, and four assertions in `ExampleTests` —
that the tree holds the node rather than its expansion, that `Compile` is what expands it,
that an `ExpressionVisitor` knowing nothing about clamping still rewrites inside one
(`VisitChildren`, whose omission is the usual silent first bug), and that it composes with
the nodes that do have factories.

Which puts the real ceiling somewhere else than the count did: **the API's own factories
are a floor, not a limit.** What a `=>` may build is any value of the type the rule
declares, and an API that can be extended can be extended from a grammar without the
grammar knowing that is what it is doing.

One thing the example ran into is worth recording as a note about the examples project
rather than about the notation: `DotGram.Examples` already declares an `Expression` — the
record tree `ExpressionTreeExample` builds — and a name declared in a namespace beats one
imported into it, so the API's has to be written out in full inside that namespace.
`ExampleTests` takes the same collision the other way and aliases it.

1,369 tests green in both configurations.

## Considered: parser inheritance, and what a grammar library could be

Raised in conversation and worked through rather than built. Nothing here is committed to;
one question is left open on purpose and is marked as such.

**The idea.** A host class inherits from a host that is itself a parser. The base has a
grammar; the derived grammar includes it — spelled `using Base;`, the same word §5.1
already has. Grammars could hang on interfaces too, and then a library is an ordinary
assembly reference.

### Why the `:` is load-bearing rather than decorative

The first objection raised against it here was that a grammar is not self-contained: it
names C#. The base's `=> @(Made(type, args))` and `@JsonObject` resolved in the *base's*
assembly, and merged into a deriving compilation they must resolve there.

Inheritance answers that, which is the whole point of the spelling. The generated code for
the derived parser lands in a class that **inherits the base's members and nested types**,
so `Made` resolves if it is `protected`, and `JsonObject` resolves if it is nested in the
base class. The `:` is what carries the base's C# scope to where its own grammar's hooks
are re-emitted.

And the generator does not stand in the way: it writes `partial class {name}` with no
`static` and no accessibility of its own (`CSharpEmitter`), so a host meant to be inherited
is simply written non-static. **No generator change is needed for the inheritance itself to
be legal C#** — only for the grammar to be merged.

### Which gives two levels of library, and the notation says which is which

An interface carries text but its members are not in the implementing class's scope; a base
class carries both. So:

* **a base class is a grammar with `=>`** — a parser somebody extends;
* **an interface is a grammar without `=>`** — a recognizer somebody brings their own
  constructions to.

Arrived at twice from opposite ends: once from "the composable part of a grammar is the
part that builds nothing", once from what each carrier can and cannot bring with it. Worth
keeping, because it is a rule the mechanism enforces rather than one a document asks for.

### Implementation: concatenation, and nothing new in the pipeline

Wrap the imported grammar in a `namespace` named after its class, put the texts together,
and run the existing pipeline. `Decl.Namespace`, `using`, rebinding and
`SpecializeNamespaces` do inclusion and overriding **unchanged** — there is no new model
concept in any of it.

Three things already work for free, checked rather than assumed:

* **`@using` of the base arrives.** `GrammarNormalizer.Imports` walks nested namespaces and
  collects C# imports recursively, so a `@using System.Text;` inside the wrapper reaches the
  generated file's head. Nothing has to be hoisted by hand.
* **`trivia` stays the base's.** §4.5 takes it from where a rule is *declared*, so wrapping
  is enough: a library written under `trivia = none` does not infect the grammar that
  imports it.
* **Overriding already has a form, and the diagnostic teaches it.** Redeclaring a name an
  import provides is `GRAM3012`, whose message says in so many words: *if this means to
  replace it rather than declare a new rule under the same name, say so with a rebinding
  instead — `namespace (Name = ...)`*. So "override a base rule" is a rebinding, not a
  redeclaration, and an author who reaches for the wrong one is told which is right.

And two where it stops being textual:

* **The base's publications would come along.** `parse X as Parse` inside the wrapper is
  legal and would generate methods on the derived class that also hide the inherited ones
  (CS0108). They have to be dropped — which is after parsing, so the merge is a small
  transform on `GrammarFile` rather than on a string. No loss: the text is parsed anyway.
* **Positions.** §7.6 maps a diagnostic's offset back into the `[Gram(...)]` literal as it
  was spelled. Concatenation shifts every offset, so the merge has to carry a map from
  merged offset to origin. This is the one genuinely non-trivial piece, and it is exactly
  what the word "concatenation" hides.

### Reading the base's grammar is the easy half

`Host.From` already holds the `INamedTypeSymbol` — `ForAttributeWithMetadataName` hands it
over because that provider is semantic anyway — so `type.BaseType.GetAttributes()` yields
the base's grammar as a plain string **in the cheap stage**. No `CompilationProvider`, no
incrementality lost: a string is equatable, the cache keys on it, and editing the base's
grammar invalidates its derivatives exactly as it should.

"The project is not compiled yet" is not an obstacle. Source symbols are complete long
before emit, and `[Gram]`'s argument is a constant that Roslyn hands back the same way from
source and from metadata.

### Where it actually breaks: diagnostics, at one line

`Host.From` keeps the literal token beside the decoded text, because a diagnostic carries an
offset into the value and putting it where the author can see it means finding that place in
the spelling:

```csharp
var written = attribute.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax syntax && …
```

**`ApplicationSyntaxReference` is non-null only for an attribute written in source.** Read
from metadata it is null. So:

* **base in the same assembly** — the literal is there, offsets map, an error in the base's
  rules is underlined where it was written, and the mechanism works untouched;
* **base in a referenced assembly** — there is no token, nothing to map onto, and the
  `#line` in an emitted `Construct_*` points into a file that may not be on the machine.

Three consequences to decide before any of this is built:

1. **Duplicates.** An error in the base's rules would be reported twice — once generating
   the base, once generating each derivative, at the same place. A rule is needed: a
   derivative reports what the merge caused and stays silent about what the base already
   says for itself.
2. **The live-editing cascade.** While `class Derived : Bas` is half typed, the base does
   not resolve, the merged grammar is the derived one without its imports, and every rule
   the base provided is suddenly undefined. Today a broken grammar breaks one class; here it
   would break every derivative, continuously. The answer is probably to generate nothing
   and say one thing when a base list is present and unresolved, rather than compile a
   grammar known to be missing half of itself.
3. **Cycles cannot happen.** C# forbids `A : B, B : A`, so inheritance gets termination for
   free — which a named `using grammar Foo;` would not have.

**So: same-assembly first.** It is the whole feature — composition, overriding, libraries
within a solution — and it costs nothing in diagnostics, because everything still points at
real source and no metadata is read. Cross-assembly is a second step whose price is not
access to the data but that errors lose their place, and that is a decision rather than a
detail.

### Open: what `context` and `state` mean under a merge

Left open deliberately.

Both are root-only by construction — `GRAM3013` and `GRAM3015` refuse them inside a
namespace, because a context for part of a parse is not a thing. So a base grammar that
declares either **cannot be wrapped**, and the first library above the recognizer level
walks straight into it. Two shapes were discussed:

**Per-namespace.** Tempting, and not a new concept: §4.5 already scopes `trivia` by where a
rule is declared. Argued against here on one ground — a publication's signature would then
depend on the transitive set of reachable rules, so adding a rule inside a library would
change every consumer's call site. And `state` per namespace would need one mark array per
namespace and a slot index in the arena entry, which is the machinery §7.8 was designed to
avoid.

**Lifted and reconciled**, the merge pulling them out of the wrapper before binding — the
same place the publications are dropped. Then the two behave differently, and the
difference is forced rather than chosen:

* `context` could reconcile by assignment: `DerivedState : BaseState`, one parameter of the
  most derived type, the base's `@(context.…)` still compiling because it is a `BaseState`.
  No variance problem, no signature churn.
* `state` cannot. `ReadOnlySpan<T>` is invariant, so a span of a derived mark type will not
  pass to a parameter of the base's. The rule would have to be exactly one state type
  declared once along the chain — and that turns §7.8's "one type for the whole grammar"
  from a convenience into a constraint on composition: a grammar meant to be a base would
  have to declare its `state` as a reference type, since an enum cannot be extended by the
  consumer who needs a concern of their own.

Both directions are written down; neither is chosen.

## Noted: `?.` is not the extension node to start with

Offered against the example above and worth keeping so it is not re-derived. Null-conditional
looks like the obvious first custom node in `ExpressionLanguage` and is not: `Postfix` is
left recursive, so `a?.b.c` builds as `Member(Member(NullConditional(a), b), c)` — the node
lands at the *bottom* of the chain while what it must short-circuit is the whole of it. One
node does not do it; the rule's shape has to change so that the node wraps the chain rather
than the operand. `foreach`, `using` and `lock` are the better first ones, because the
extent of the node is exactly what the rule read.

## Built: one `context` and one `state` for the whole assembly

Taken now, on the strength of the entry above, and for one reason: it is the constraint that
keeps a grammar includable in another later, and taking it before anything leans on it makes
it a rule rather than a later change of one. `GRAM3017` and `GRAM3018`.

**It is stricter than what will eventually be wanted**, which is one per inheritance chain —
and a chain cannot be checked today because there is nothing to follow. So two parsers in one
assembly that never meet are refused for a reason neither of them can see. That is the price,
it was taken knowingly, and the message says *why* rather than only what: a context belongs
to a whole parse, and a merged grammar could not say which of two it meant.

**Checked apart from generation rather than folded into it.** A `Collect` in the pipeline that
produces parsers would make every parser's output depend on every grammar in the project, and
a keystroke in one would recompile all of them. This is a second `RegisterSourceOutput` that
generates nothing and only says things, fed by the few grammars that declare anything — so it
holds still while the rest of the project is edited. The grammar is already parsed in the
cheap stage for its C# questions, so recording *where* the two declarations stand costs two
offsets and a length each, and those are what the collected value is keyed on.

**Said at every site rather than at all but one.** Which grammar the generator saw first is
not something an author can know or act on, and whichever file they are looking at is where
they need to be told. Placed inside the grammar text through the same path a binder
diagnostic takes — `Report.Of(GramDiagnostic, …)` with the host's literal and offset — so it
underlines the declaration and not the class.

Five tests, and the one worth naming is `And_the_two_are_counted_apart`: a `context` in one
grammar and a `state` in another is two of nothing, because they are two rules and not one.

1,374 tests green in both configurations.

## Built: the position map, which is the half inheritance rests on

The first piece of the entry above, on its own and provable on its own: joining grammars
into the one text they are compiled as, and taking a position in that text back apart.

**The including grammar goes first and is not moved.** Asked in conversation as
prepend-or-append, and the answer is diagnostic rather than aesthetic. A diagnostic in a
grammar written inside an attribute is placed by *searching* the literal's spelling for the
text — a C# string knows how to turn its spelling into a value and not the other way round.
So the text an author is actually editing keeps offsets `0..N`, its diagnostics land exactly
where they landed before any of this existed, and only what follows needs translating.
Putting the included grammars first would shift the one text somebody reads in order to
serve the order of a file nobody reads.

**What is included is wrapped in a namespace and not indented.** The wrapper is what hides
its rules until a `using` asks for them (§5.1) and what keeps its `trivia` its own (§4.5).
Indenting would read better and is refused: it would shift every position on every line, and
the translation is only exact while a segment's bytes are the segment's bytes. The braces
stand on lines of their own instead.

**The wrapper's own characters belong to no grammar** and are left out of the map rather
than attributed to one, so a position landing in them is answered "nowhere" — which is what
`ILineMap` asks for over a guess.

`ILineMap.TryMap(position, out file, out line, out column)` turned out to be exactly the
right seam already: it answers *which file*, so a composite of it is all a segment map has
to be. `SplicedLineMap` holds `(Start, Length, Map)` and delegates after translating;
`GrammarSplice.Join` produces the text and the map **in one call**, because they are one
fact said two ways and working them out separately is how they come to disagree.

The two tests that matter are the last two, and they are end to end rather than about the
map: a grammar including another **compiles** — its `using Base;` reaching declarations that
come after it in the file, which works because the binder declares everything before it
resolves anything — and an error inside what was included comes back as line 1, column 8 of
`Base.gram` rather than as somewhere in the joined text. That last one is what would catch a
boundary off by anything at all.

Nothing reads any of this yet. What it unblocks, in order: the merge on `GrammarFile`
(dropping the base's publications), reading the base's `[Gram]` in `Host.From`, and the two
diagnostics rules — duplicates, and an unresolved base list.

1,384 tests green in both configurations.

## Built: `IncludedAs`, and a report that was being dropped

The second piece: a host may say what name another grammar includes it under, where that
should not be its class's own — `JsonGrammarBase` wanting to be `using Json;`.

**A property on `[Gram]` rather than a second attribute.** The marker attributes are emitted
into *every* consumer's assembly, so a new type is a type everybody carries, including
everybody who never touches inheritance. A property costs nothing at the type level and
works for both forms `[Gram]` already has — the empty constructor for a `.gram` file and the
one taking text.

**Not called `Namespace`.** That word was carefully separated once already: `GramCompilerOptions
.Namespace` and the emitter's `@namespace` are the *generated code's* C# namespace, and the
`context` → `namespace` rename settled on "`GrammarNamespace`, never bare `Namespace`" to
keep the senses apart. `IncludedAs` also names the thing exactly: it means something only
when another grammar includes this one, and it is what they write.

The name has to be one identifier — a grammar is included by being wrapped in a namespace,
and a dotted name would mean nesting, which is written in the grammar itself. `GRAM0005`,
said at the host, before the splice can turn it into a parse error in a text nobody wrote.

### Two defects it turned up, and the second was silent

**A named argument would have taken every inline diagnostic's placement with it.** A
diagnostic in a grammar written inside an attribute is placed by searching the literal's
spelling, and the search matched `Arguments is [{ Expression: LiteralExpressionSyntax }]` —
*exactly one* argument. Writing `IncludedAs = "…"` beside the grammar makes two, the pattern
stops matching, `Literal` becomes null, and every squiggle silently falls back to the class.
Now it takes the first positional argument instead. The test asserts the placement rather
than the reading, because the reading would have gone on working.

**And the first stage's reports were being dropped.** `Compile` starts a fresh builder and
never carried `grammar.Reports` forward. It had never shown, because every report `Asked`
made until now came with an early return — no text, and the branch above hands those on. The
first report that could stand beside a grammar which reads perfectly well went missing, and
was found only because it was looked for. `Compile` now seeds its builder with what came
before it.

Nothing reads `IncludedName` yet either.

1,390 tests green in both configurations.

## Built: a host inherits its base's grammar

The pieces joined up. `class Reader : Lexemes` and `using Lexemes;` in the derived grammar,
and what comes out is one parser built from both — same assembly, which is the case that
costs nothing in diagnostics.

**Reading the chain is in the cheap stage and adds no dependency.**
`ForAttributeWithMetadataName` hands over the target's symbol because that provider is
semantic anyway, so walking `BaseType` and reading a constant off each costs nothing extra
and loses no caching. Matched **by display name** rather than by symbol: `[Gram]` is emitted
into every assembly separately and on purpose, so a base compiled elsewhere carries *its*
assembly's `DotGram.GramAttribute` and the two are not the same type. What they share is
what they are called. A base with no grammar is walked past rather than stopping the walk.

**The joining is in the cheap stage too**, where the additional files are; the placing is in
the third, where the diagnostics are. `Piece` is what travels between them, and both the
`#line` map and the squiggle are built from the same one — working out which grammar a
position came from twice, from two sets of numbers, is how the two come to disagree.

`TryResolveGrammar` stopped taking a `Host` and started taking a source, a name and a place
to report at. A base's grammar is then found the way any grammar is, by the same code, and
its "no such file" is reported against the class that declares it rather than the one that
inherited it. A base whose grammar cannot be found is left out and the rest still compiles;
what it was going to provide comes back as ordinary undefined names.

### The regression the splice tests should have caught and did not

`Join` appended the newline that keeps a grammar from running into the wrapper **whether or
not there was a wrapper**. One character makes the end of the text a different place, and a
rule that failed at the end of the input is reported there — so every host that inherits
nothing was quietly told its last token ran one character longer. Caught by an existing
generator test rather than by the eight tests written for the splice, because the example
they used happened to end with a newline already. The theory now has a case that does not.

**Joining one grammar has to be that grammar, to the character.**

### And a pre-existing imprecision, seen because inheritance put text after a grammar

`GramParser.From(start)` measures to the beginning of the *next* token rather than to the
end of this one, so a construct with anything after it takes the trailing whitespace into
its span. True of every location in every grammar and nothing to do with joining — it had
simply never shown, because a grammar's last token has nothing after it and the tests that
assert on exact spans use last tokens. An error in an inherited grammar has a wrapper's
brace after it, and there it shows. Left alone: the fix is one line and its blast radius is
every location the parser reports, which is a change to make deliberately rather than as a
side effect of this.

1,395 tests green in both configurations.

## Fixed: `context` reached the engine and nothing else

Found by an external review of `d744de9`, reproduced exactly as described, and worse than
described in one respect. A grammar as ordinary as

```dotgram
context : @C
Value : @int = d: ['0'..'9']+ => @(context.Count + d.Length)
parse Value
```

emitted this:

```csharp
public static int ParseValue(string input, C context)                        // takes it
static int Construct_Value(C context, string d) => …                         // wants it
static int Recognize_Value_Whole(…, ref Failure failure, out int value)      // has neither
    …
    value = Construct_Value(captured0_0);                                    // CS7036
```

**A valid grammar produced C# that does not compile** — the worst shape a generator defect
takes, because the error lands in a file nobody wrote. Two breaks, not one: the publication
handed a context to a recognizer whose signature did not have it, *and* that recognizer
called a factory without the one it did.

The streamed `find` was the same story a second time. `streamedHands` left the context out
where it carefully passed `null!` for the input, and the reasoning behind that turns out to
be exactly why the context should have stayed: a window has no whole input, and that is a
fact about the input. A context is the caller's object and says nothing about how much text
is held.

Fixed by threading rather than by refusing, which is the right answer and was the reviewer's:
`context` needs no arena. `RenderFlat` and `RenderFlatValued` take it, `EmitFlatFactoryCall`
passes it, and the streamed `find` and its sequence-of-lines overload hand it on.

### Why the tests did not have it

The corpus tested `context` against the engine, because that is where it was built. The
tests now go the other way round: one grammar per *rendering* — flat-valued, flat and
valueless, the engine, a call compiled in place, a stream — each pinned to a shape only that
rendering emits, and each compiled. Pinning the shape is the part that matters: five
grammars that all compile prove nothing if four of them quietly took the same path, and the
first draft of this test had exactly that problem — a capture makes a valueless rule
non-flat, so the case written to cover flat-and-valueless was the engine again.

**This is the fourth time the same defect class has been found this session** — `parserState`
missing from `SuppliedNames`, the fold materializer assembling its own arguments, sited
callees not refusing a `context`, and now this. Every one is a supplied name that reached
one rendering and not another. The review is right that the answer is not another `if`: what
is wanted is one place that says which requirements a rendering can meet, with the
renderings consuming it rather than each carrying its own list of what it forbids. Recorded
as the next architectural piece rather than done here.

1,400 tests green in both configurations.

## Fixed: the documentation, and what checking it found

Prompted by the same review. Each claim was checked against the compiler before anything
was written, which was worth doing: two of them were right, one was right about a bigger
problem than it named, and one thing nobody had noticed was worse than any of them.

**A diagnostic recommending syntax the compiler refuses.** `GRAM3012` told an author to
write `namespace (B = ...)` — and `ParseNamespace` has a diagnostic of its very own,
`NamespaceNeedsWith`, for exactly that mistake. The message also put the *rule's* name
where the *namespace's* goes. Now: `namespace Name with (B = ...) { ... }`.

**And that turned out to be systematic.** The `context` → `namespace` rename made the name
mandatory; the prose did not follow. Eleven places wrote the unnamed form — eight in
`syntax.md`, one in `README.md`, one in `examples/README.md`, and the header comment of
`LocaleNumberExample`, whose own grammar twenty lines below writes it correctly. All fixed.

**§5 contradicted itself, and its own example is refused.** Line 891 said "a rule of the
same name shadows the outer one" and the example said `B = 'd' // shadowing: a new,
unrelated rule named B`. Compiled, that example is `GRAM3012 Error` — and §5's own account
five hundred lines further down describes the refusal correctly. The early passage and the
example now say what the compiler does and why: a declaration always means a *new* rule,
which is almost never what was wanted and cannot be seen locally.

**README on indirect left recursion.** It said flatly not built; `GrammarNormalizer
.Recursion` rewrites the one shape of it that is not arbitrary. `status.md` had both — the
table said it works, a paragraph three hundred lines later said "Refused: indirect left
recursion". Both now say the same thing, which is the precise thing: through rules that only
forward, yes; through a chain that recurses through itself, or a rule that does something of
its own, refused. The second `recover` claim in README was simply stale and is gone.

**`CSharpEmitter`'s own header** said "Publications share one state-machine method" thirty-six
lines above the code saying "One machine per published rule". It now also says that a
publication needing none of the three things the arena is for reaches no machine at all.

**And the one nobody named: `status.md` did not know about this session at all.** No row for
`context`, none for `state`, none for inheritance. That is not historical drift, it is drift
made this week, by me, feature by feature. Nine rows added, including the one that says a
base in a referenced assembly does *not* work — which is the kind of row a status table
exists for.

`next.md` itself is renamed in spirit rather than in path: its header now says it is a
diary, newest last, authoritative about nothing present, with a table pointing at
`syntax.md` for the language and `status.md` for the compiler. The old handoff sections are
kept and labelled as what they are — an engine ninety commits ago — because the reasoning in
them is still the reasoning. The baseline of 887 tests is left standing and dated rather
than corrected: an entry edited to match the code stops being a record of a decision.

Splitting it into `architecture.md` / `design-notes/` / `backlog.md` is the reviewer's
suggestion and is not done here — 5,900 lines of prose to sort is a decision, not a tidy-up.

1,400 tests green in both configurations.

## Built: one place that says what each rendering can hand over

The review's central architectural point, taken. Four times in one week the same defect
appeared — a name the language supplies to a `=>` threaded through the rendering it was
built in and silently absent from the rest — and each time it was fixed where it was found.
`Renderings.cs` is the place that stops the fifth being found the same way.

**It is a table, not a capability system.** For every supplied name and every rendering —
the engine, the flat method, the call compiled in place — one entry: either the rendering
hands it over, or a sentence saying why it refuses. `ComputeFlatValued` and
`ComputeSitedValued` read it instead of each listing forbidden names, which is where the
lists kept going out of date.

**A missing pair throws rather than defaulting to "no".** That is the whole design. A
default would make an oversight look like a decision, which is precisely how this went
wrong: nobody ever decided the flat rendering could not supply a `context` — it was simply
never asked, and the generated call went one argument short.

**And the test asks for the decision rather than for the behaviour.** There already *was* a
test that `context` works; it passed while three renderings could not hand it over, because
it only ever ran the engine. This one enumerates every name against every rendering — thirty
pairs — and asserts that each is answered. Verified by removing one entry: the pair that
disappears is `(Flat, context)`, which is the P0 itself, and the failure names it.

The refusals are written as sentences an author of this compiler can act on, and asserted to
be sentences: "a flat rendering keeps no record of where the rule began", not `false`.

## Fixed: a room check that could overflow

`p + count > text.Length` is the obvious spelling and is wrong at the edge. A span may hold
`int.MaxValue` characters, so a position near the end plus a literal's length wraps
negative, the check passes, and what should have been an ordinary refusal to match becomes
an exception out of a slice. Four gigabytes of input to reach, and still a wrong answer
rather than a slow one.

Asked the other way round — `text.Length - p` against the count — it cannot overflow: both
sides are non-negative and the difference is between them.

**Signed, deliberately, where the single-character form beside it is unsigned.** That form
is unsigned because it is then the same comparison the indexer's own bounds check makes,
which is the measured reason it was written that way. Here unsigned would be wrong: were `p`
ever past the end, `text.Length - p` is negative and casting it to `uint` makes it enormous
— the check would report room where there is none, which is the one direction a room check
may not fail in.

The review suggested the change partly for range-check elimination. That half is not
claimed: this project's own gate says measure in a parser and expect noise, and no
measurement was made. What is claimed is that the overflow is gone.

Snapshots moved and the diff is nothing but the room checks.

1,433 tests green in both configurations.

## Fixed: a `with` cycle answered by document order

The oldest of the review's findings, and the comment warning about it had been sitting in
`GrammarNormalizer.With.cs` for some time:

> A cycle between two with-bearing rules … has no order that satisfies both.

What settled it was `visited` — a rule met twice simply stopped waiting for the other's
splice. Which of the two gave up depended on which the loop reached first, and that is the
order the rules are written in. **Moving a rule in the file changed what the parser did**,
silently, and nothing would ever have caught it: both orders compile, both produce a
parser, and the two parsers differ.

Refused now, `GRAM4017`, before the ordering runs — because a cycle is precisely what has no
order to run in.

**Refused rather than settled**, which is the reviewer's recommendation and the right one:
settling it means choosing what the notation means, and nobody has. The shape that would
answer it is a specialization keyed by (rule, bindings) with memoization and a placeholder
symbol standing in before the body exists — which turns sequencing side effects over
`_bodies` into ordinary graph construction, and is a piece of design rather than a fix.

**A rule reaching only itself is not this** and is not refused: there is one thing to do and
one order to do it in. What has no order is two.

The detection closes the reach relation over itself rather than walking strongly-connected
components. The graph has as many nodes as the grammar has rules containing a `with` — a
handful — so the clearer of the two costs nothing measurable.

### The test the defect deserved

Not "is it refused". The pass that settles a cycle by document order passes that. The one
that matters is **that the same thing is said either way round**: the same two rules, written
`A` first and then `B` first, and both refused. A test asserting only the first order is a
test the old behaviour also passes half the time.

And one that the refusal did not take the feature with it: a chain of `with` sites that does
not come back still finds its order, which is what the ordering was written for.

1,436 tests green in both configurations.

## Fixed: the one dangerous half of layout reading its own output

The review wants `Machine.Layout` to stop recovering a control-flow graph by regex over the
C# it just wrote. That is right and it is not what was done here — 85 `goto` sites and 43
arena pushes is a mechanical change at exactly the scale where a mechanical change in a code
generator goes quietly wrong, and it should be one deliberate piece of work rather than a
follow-on.

**What was done is the part that is actually dangerous**, which is not `goto S(\d+)` — that
pattern is unambiguous and cannot mean anything else. It is the other regex:

```
new ParserEntry\(ParserEntry\.(\w+), (\d+),
```

whose second capture means **different things for different kinds**. Layout rewrites it as a
state number, and reading a capture slot or a factory as one has already been a silent
corruption once: a slot that happened to equal a collapsed state's number came back as that
state's, the value it named was never built, and a construction was handed a null.

It was guarded by a list of the eight kinds that *are* states, which **said nothing about
the other ten**. A kind added later was undecided by default, in whichever direction the next
reader assumed — and two kinds were added this week.

Now every kind has an entry saying what its second field is: a state, a slot, a choice of
factory or recovery or `with state` site, or nothing numbered at all. `MeansAState` throws
for a kind nobody has decided about. The same shape as `Renderings.cs` two entries above,
for the same reason and against the same defect class.

**The test reads the kinds out of the emitted code**, from a checked-in snapshot rather than
from a list written beside it — so a kind added to the engine appears there on the next run
and is then asked about. Taking an entry out was tried: it breaks generation itself, loudly,
but as a downstream compile error in the consumer rather than as a sentence naming the kind.
So both are asserted — the throw says which kind, and the theory says every kind reaches it.

The larger change stays on the list, and its shape is the review's: `StateBlock` carrying
text plus typed edges, recorded where a jump is written rather than recovered from how it
was spelled.

1,455 tests green in both configurations.

## Fixed: a fourth table keyed by a node could be forgotten

The third instance this session of one shape of defect, and the third fix of the same
shape. Passes here record what they work out against the node they worked it out on, by
reference; a pass that rebuilds a node has to hand those on, and `Carry` did it as a run of
`if`s. The fold was once left out of that run, and `Recursion.cs`'s own remarks say what it
cost — C# the *consumer* could not compile, in a file they never wrote.

Nothing about a run of `if`s says a fourth table has been added and a fifth has not. `Carry`
now walks a list; adding a table is one line in one place; and a test asks, by reflection,
whether every field of the shape `Dictionary<Node, …>` is in that list.

The two tables keyed by *rule* whose values name nodes — `_folds`, `_climbing` — are in the
list too, and are why it holds a move rather than a dictionary. The reflection cannot demand
them, and the list can carry them.

### The first version of this test was worthless, which is worth writing down

It built the normalizer with `FormatterServices.GetUninitializedObject`, so no field
initializer ran, so **every field was null and so was every registered table**. "Is this null
among those nulls" is true of anything. It passed, and it would have passed with nothing
registered at all.

Then the check that it caught a missing table was itself wrong twice over: the probe field
it was supposed to add never got added, because the pattern it matched on omitted a
`readonly`, and the run that "passed" was a run against unmodified source. Two green results
in a row, both meaningless.

What settled it was printing what the reflection actually found — three names, then four
once the field really existed — and only then asserting. **A test that has not been seen to
fail has not been seen to do anything**, and the way to see it is to make the thing it
guards against, not to reason that it would.

1,456 tests green in both configurations.

## Decided: `context` is a contract, and inheritance refines it

Settled in conversation, and it overturns what was written above — under "Considered: parser
inheritance" the open question was recorded as a choice between scoping `context` per
namespace or lifting it and reconciling by assignment, with `state` unable to follow because
`ReadOnlySpan<T>` is invariant. The answer is neither: **`context` and `state` do not compose
the same way at all**, and treating them as one thing was the mistake.

### The rule

> A grammar declares the contract its own semantic code requires. A derived grammar may
> strengthen the effective type, provided that type satisfies every inherited contract and is
> visible where the derived grammar compiles. **Inherited semantic code stays statically bound
> to the contract it was written under.**

So a declaration in a derived grammar does not create a second context. It refines the
inherited one, and the parts of a composed grammar legitimately see one object through
different static contracts:

```text
                     the object: DerivedContext
                              |
              +---------------+---------------+
       base rules see                  derived rules see
        BaseContext                     DerivedContext
```

Which is ordinary subtyping — virtual dispatch works, new members are invisible to the base,
and nothing about it is special to this notation.

### Why the last clause is the load-bearing one

If base rules were recompiled against the derived type, **inheriting a grammar would change
what already-written C# means**. The example that raised it would not actually have flipped —
`context.Resolve(name)` with `name` a `string` picks `Resolve(string)` over an added
`Resolve(object)` either way, an exact match being the better one. What does flip:

* `new`-hiding — a derived `new bool Resolve(string)` takes a statically-derived call outright;
* an overload better by conversion, `Resolve(ReadOnlySpan<char>)` beside `Resolve(string)`;
* an extension method applicable to the derived type and not the base;
* optional parameters, `params`, generic inference.

So the principle holds more strongly than the example showed, and it is what settles the
design.

### And it overturns `GRAM3017`

A base and a derived host in one assembly both declare a `context`. Today that is an error —
the one taken deliberately as forward-compatible, on the reasoning that a constraint taken
early is a rule rather than a later change of one. This design is what it was being kept
compatible *with*, and it wants the opposite: not one per assembly, and not even one per
chain, but **an effective type satisfying every inherited contract**. A condition rather than
a prohibition. `GRAM3017` and `GRAM3018` are rewritten when the check exists.

The early constraint was not wasted: it means no existing grammar has two, so nothing breaks
when the rule changes shape.

### `state` does not follow, and the reason is not the variance

`ReadOnlySpan<T>` being invariant is the mechanical reason a derived mark type cannot be
handed where a base one is expected. The real reason is underneath it: **a context is one
object flowing down, and a mark is a heterogeneous stack of values from several authors.**
Different shapes compose differently.

So `state` stays invariant across composition — one type, declared once along the chain — and
§7.8's existing note gains weight: a grammar meant to be inherited declares its `state` as a
reference type, because a consumer cannot extend an enum to add a concern of their own. Named
channels or a heterogeneous mark model would be a separate design and are not this one.

### Four things this needs that were not in the proposal

1. **The contract type must be visible where the derived grammar compiles.** An emitted
   `BaseGuard(…, BaseContext context)` does not compile if `BaseContext` is `internal` to
   another assembly. Within one assembly it is nothing; for a library it is a rule, and the
   same rule as the one that makes `@JsonObject` resolve — what a library grammar names in
   its C# has to be reachable from where it is re-emitted.
2. **"Interface" means two different things here.** A contract that is an interface —
   `context : @INameContext` — is exactly right, and is what makes several inherited
   contracts satisfiable by one type. An interface *carrying a grammar* is a different thing
   and cannot have `=>` at all, because its members are not in the implementing class's
   scope. They will be conflated if not separated in writing.
3. **The scoping becomes asymmetric.** `context` is per-rule; `state` stays per-graph. Two
   scoping rules in one model, accepted rather than stumbled into.
4. **The check has a seam already.** `ISymbolResolver.IsAssignable(from, to)` exists for §4.1
   — "assignability is inheritance, interfaces and conversions, none of which a grammar can
   see". Nothing new is needed and `Grammar/` stays free of Roslyn.

### Order

1. `ContextOf(rule)` in the model — the contract by where a rule came from. The splice
   already knows which segment each rule was read from.
2. Signatures by contract: a guard and a factory take their origin's type, a publication
   takes the effective one, and the calls upcast on their own.
3. The refinement check through `IsAssignable`, and `GRAM3017` rewritten from a prohibition
   into a condition.
4. `state` invariant, with a diagnostic saying so.

**One thing about (1) and (2):** the contract has to survive `with` specialization, cloning,
inlining, the flat rendering and sites — which is exactly the class of "do not forget this on
a rebuild" that got a registry two commits ago and a per-rendering table one commit before
that. The place to put it now exists, and so does the kind of test that will not let it be
forgotten.

## Assessed: the second architecture review, and what it found that is a defect

A review of `d744de9` against the whole backend. Eight commits have landed since, so five of
its findings were already answered — recorded here because a review's value is partly in
which of its points survive contact:

* **the `with` cycle settled by mutation order** — refused now, `GRAM4017`, and the exact
  comment the review quotes as its red flag is gone;
* **`Carry` transferring node annotations by hand** — a registry, with a test that every
  `Dictionary<Node, …>` field is in it;
* **the hardcoded `Resumable` kinds inside layout** — one decision per kind, throwing for an
  undecided one, with the kinds read out of the emitted code;
* **`context`/`state` left deliberately open** — the design is decided and two of its four
  steps are built;
* **the scanner's signed room checks** — asked so they cannot overflow.

The core of its first finding stands untouched: layout still recovers a control-flow graph by
regex over the C# it just wrote. Only the dangerous half — an entry's second field meaning
different things for different kinds — was closed.

### The strongest finding is not debt

**Embedded C# is analysed by string search, and it is wrong in both directions.** Verified
rather than accepted:

```dotgram
Start : @string = t: ['a'..'z']+ => @(Log("parserInput") + t)
```

emits a factory taking `string parserInput` and sets `UsesInput` — and `CanLowerValued`
opens with `if (UsesInput || …) return false`, so **a string literal decides which rendering
a grammar gets**. The other direction is exact by reading: `Names` treats anything that is
not a letter, digit or underscore as a boundary, so `Other.context` — a member access —
counts as naming the `context`.

This is not an approximation inside an optimization. It is spelling deciding a signature and
a compilation strategy. And Roslyn is already in hand: `ICSharpScanner` is called on every
`@(...)` to find where it ends, so the free names can come from the same pass that is
already made.

### Where the review's order is changed

It puts the typed CFG first. Free names go first here instead: the CFG is a large refactor
bought for reliability, and this is a defect with a reproduction.

### Where it is argued with

**`recover` as a structural node.** There is a measurement from this week: `Node.Marked` took
about twenty-five sites and the compiler helped at none of them, because the switches over
`Node` carry `_ =>` defaults. It paid for `Marked` because a mark is transparent to
everything; `Recovery` is not, so every site is a decision rather than a line. The registry
and its test are cheaper and already stand.

**A `StreamStage` IR.** The complaint is right — `Yields()` recognizes shapes rather than
meaning — but normalizing into one canonical shape before the question is asked is cheaper
than an IR with a single consumer.

**And one where the review is right about something it could not have known:** the `with`
cycle check written two days ago closes the reach relation over itself by hand, which is the
fourth ad-hoc walk over the call graph. That is exactly the duplication its "SCC as a central
primitive" predicts.

### The line worth keeping

The most useful thing in the review is not a finding, it is a boundary:

| decides | may be a heuristic |
| --- | --- |
| what the language means | no |
| whether backtracking can be removed | only by proof |
| whether something can stream | proof, or a stated conservative limit |
| which rendering is *legal* | proof |
| which rendering is *cheaper* | yes |
| inline or call | yes |
| unroll 8, 16 or 24 | yes |

`Unrollable = 24` and `Emitted = 8` are fine **because** they choose between implementations
already proved equivalent. What is not fine is a heuristic on the other side of that line —
which is precisely where the string search sits.

`Weight()` should be called what it is, an estimate of emitted size, and kept away from the
analyses that have to be exact.

### Order taken

1. Free names through Roslyn, replacing `.Contains` and the boundary scanner.
2. `FIRST` as a least fixed point — recursion answers `Top` today, which cuts the proof power
   of everything below it, and `Nullable` and `FOLLOW` already have the shape.
3. One `CallGraph` with SCC, and the four hand-written walks folded into it.
4. The typed CFG, for the engine and the scanner at once, or it will be written twice.
5. The unified analysis layer, after 2 and 3, which are half of it.

## Fixed: the last two answers a grammar was giving twice

Steps 1 through 3 of the order above are built. What follows are the two smaller findings of
the same kind — a question about a grammar answered in two places, and worse in one of them —
taken now because 4 and 5 are large and these are not.

**Whether an element admits a line terminator.** `Retention` decides whether a rule can be
read from a window, and needed to know whether a character class can match `\n` or `\r`. It
worked it out itself, and treated a Unicode category as admitting one whatever category it
was. Safe in the direction that mattered when it was written — a rule wrongly said to cross a
line loses an overload, wrongly said not to it loses data — and wrong: a letter is not a line
terminator, so `[\p{L}]+` was said to cross lines and could not be read from a window.
`FirstSets` already expands a category into the characters it holds, so the question goes
there and the answer is the overlap with the two terminators. The conservative direction
survives without being restated: an element holding a C# predicate is "anything" over there,
and anything overlaps a terminator.

**Whether a node can match without consuming anything.** Written twice, and the two copies
had drifted apart in both directions at once. `FirstSets` stopped at `min == 0` for a
repetition, so `B{1,1}` over a nullable `B` was called consuming — a shape that is not refused
the way a nullable body under `*` or `+` is, since it cannot spin, and so reachable from
source. The normalizer had no case for `Behind` and fell through to "consumes", which would
make a sequence beginning with a lookbehind opaque to the left-recursion walk; nothing reaches
that today, since a lookbehind only comes from lowering a word lexeme and a consuming literal
follows it. Each was right where the other was wrong.

The shape of a node is answered once now. The only difference between the callers is who can
answer for a rule — the normalizer reads the estimate its own fixed point is still refining,
everything else reads the settled map — so that is a parameter.

### And what is not started

Step 4, the typed CFG, is not begun, and saying so is worth more than a partial attempt. The
reason is specific: a large fraction of the `goto` sites the layout pass has to see are not
plain gotos. They are conditions and jumps written into one line — `if (…) goto …`, a `case`
arm that jumps, a range test that falls through — so recording an edge only where a `goto` is
written on its own would give a control-flow graph that is *incomplete*, which is worse than
the regex it replaces: a regex that misses an edge is visibly a regex, and a graph that misses
one is trusted. Doing it properly means the emitter's writing layer records edges as it
writes, at every site, which is a change to how code is emitted rather than to how it is read
afterwards.

## Built: the context contract checked, and the state claim with it

Steps 3 and 4 of "Decided: `context` is a contract". They are one change rather than two:
both checks stop being about an assembly and start being about a composition, and doing only
one of them would leave `state` guarded by a rule the other half had just abandoned.

**The context.** Every namespace's declaration is a contract its own rules were compiled
against. The effective type is the root's where there is one — the grammar being compiled is
the one whose caller supplies the object — and otherwise the first inherited contract that
satisfies all the others. That type has to be assignable to every contract underneath it,
asked through `ISymbolResolver.IsAssignable`, which is the seam §4.1 already had.

**The state.** The same walk, the opposite condition: every declaration must name the same
type. What an included grammar declares is a claim, not a contract of its own.

### What this replaces, and how the numbers were handled

The design entry above says `GRAM3017` and `GRAM3018` are *rewritten*. They are retired
instead, and the new checks are `GRAM3019` and `GRAM3020`. The reason is written in this
repository already, next to `GRAM3013`: an id that changes meaning is worse than a gap,
because a suppression written against the old one silently applies to the new. That applies
here more strongly than it did there — the old rule and the new one are about the same
declaration, so a suppression aimed at the old would land squarely on the new.

`GRAM3015` goes with them, and it was the one actually blocking something. It refused a
`state` inside a namespace, which is exactly where an included grammar's declaration lands —
so a grammar that used `with state` could not be inherited at all. Refusing the place was the
wrong shape for a rule about the type.

### What it found

A defect neither the design nor the review had in view. The generator collects, from the
grammar text alone, every question it will ask the host, answers them in one Roslyn stage,
and then the pure half may only ask what has already been answered — that is what keeps the
expensive part of generation out of the per-keystroke path. `Questions.Of` had never looked
at a `context` declaration, so the first real refinement check threw `GRAM0001` with the
collector's own message: it "did not foresee the type question". The declarations are
collected now, and every ordered pairing of contracts is asked for — both ways round, since
which of them is the effective one is what the answers decide.

Two smaller things the tests found: `using` comes before `context` in a grammar file, and a
grammar that declares a context but never names it in a hook generates code that mentions no
context at all — so the test that proves the two contracts coexist had to make both halves
use theirs. It does, and one generated file now carries `IWords context` and
`IReading context` in different signatures, which is the design in one line.

## Built: a shared leading operand is read once

The author who writes `A & X | A & Y` writes the same language as the author who writes
`A & (X | Y)` and gets a different parser: every alternative after the first reads `A` again,
and the doubling compounds through nesting. Making the author write for the machine is what a
generator exists to avoid, so the compiler folds it.

**Only where the fold cannot be seen.** The two spellings are the same grammar exactly when
`A` has one reading where it stands, and the difference where it does not is not a subtlety
about speed. Measured on the parser rather than argued:

```dotgram
Chunk = 'x'+
Start : @string = a: Chunk & "xy" => @("first:" + a) | a: Chunk & "y" => @("second:" + a)
```

on `xxy` gives `first:x` spelled out and `factored:xx/y` folded — the same text consumed, a
different alternative matched, a different `=>` run. `'x'+` can give back, so the alternatives
prefer a shorter reading of it that lets a tail fit and the folded form prefers its own. With
`Word = "ab"` in place of `Chunk` both spellings answer identically, because there was one
reading to prefer.

So the whole condition is `Determinism`: the shared operand must have at most one match where
it stands. Where the proof does not reach, nothing is folded and `GRAM4016` is left to tell
the author, whose choice it then is — the diagnostic is unchanged and still fires where it
fired.

### Where it had to go, and what that cost

Not in the emitter, though that is where the continuation the proof needs is already threaded.
Capture slots are numbered per node in source order, and the numbering is load-bearing —
"everything written since this point" has to be a contiguous suffix. A fold drops a duplicate
capture and renumbers, so it has to happen before the results are computed, which is the
normalizer.

And after the checks, not before. A `=>` is refused anywhere but on an alternative of the
rule, and the folded shape puts one behind a shared head. The author may not write that and
the compiler may — so the grammar is checked as written, then folded, then the results are
computed again.

Three things had to learn to follow an alternative there, and each is one place:

* `Fold.Of`, which says what a rule's alternatives are, and so which constructions get
  factories. It looks through a sequence ending in a choice of constructions — a shape no
  author can have written, since that is what the check just refused, so no marker is needed
  to tell the compiler's fold from a hand-written one.
* `CaptureLayout`, which gives each alternative the range of slots its `=>` may name. A folded
  alternative begins at the head it shares, not at the tail that tells it from its siblings.
* The optionality of a member, which asked whether *this alternative* writes it. The head
  standing in front is as much a part of the alternative as the tail, and without that a
  capture written on every path came out nullable.

### What it does not do yet

Two things are left out rather than reasoned about carelessly: a rule rewritten for left
recursion, whose loop is held by node identity that a rewrite would break, and a grammar with
a recovery, which is a decision about a shape this moves.

And the case that motivated the whole thing is not covered. `Call | Reference`, where `Call`'s
body *begins with* a call to `Reference`, is not two alternatives with a shared leading
operand — it is one alternative whose prefix is the other alternative, one call down. Seeing
it means looking through a call, and folding it means putting a rule's body behind a head
while the rule itself stays for its other callers. That is the next step and it is the one
that pays: the corpus is byte-identical today, and the fold fires only where a grammar spells
the sharing out.

What is built is the proof, the place, and the three things that had to follow an alternative
into it.

## Built: the lowered grammar, printed as a tree

An attempt at left-factoring through a call was lost for an afternoon inside an ambiguity
that had nothing to do with the change. A node prints itself as the notation it came from,
and `c: Call => (c)` is what a construction around a capture prints *and* what a capture
around a construction prints — while which of the two it is decides where a factory belongs.
The whole question being investigated was where a factory belongs.

So `RecognitionGraph.Dump()` prints the tree as a tree: the kind of every node, indented,
with the one detail that tells one of that kind from another.

```text
Start:
  Construct => (w)
    Sequence
      Capture 'w'
        Call Word
      Repeat 0..1
        Literal '!'
```

Over `Node.Children`, which is where traversal is defined, so a node kind added later shows
up without anyone remembering to add it. A test asserts the whole of a small grammar's dump,
which pins the format and the lowering together.

**And the report carries it.** `A construction in 'X' has no factory` is a compiler defect,
raised where a construction reached compilation that the rule's own alternatives did not
offer — which is to say, where the shape the compiler made is not the shape something else
expected. The shape is the whole of what a reader needs next, so the message now has the
rule lowered underneath it. It used to say only that a construction somewhere had no factory,
and the shape was reachable only by building something that would not build.

### And a wrong conclusion, corrected

This entry first recorded a second finding: that the suite cannot run while a grammar in
`DotGram.Parsers` fails to generate, and that separating the tests which need a consumer's
generated code from the ones which need only the compiler would fix it.

There is nothing to fix. A project that does not build does not build for what depends on it,
and the test project depends on the parsers because it tests them. That is a build working,
not a repository with a flaw in it, and the proposal to split the tests over it was reaching
for a change in the layout to make up for a mistake in method.

The mistake was mine and is worth the line: told that the compiler had broken on a real
grammar, I kept trying to diagnose it through a suite that could not run, rather than
reproducing the break on a grammar that stands on its own. The dump above earns its place for
the same reason it was written — it puts the shape where it is read — and not as a way around
a build.

## Built: a prefix one call down, and the shape that hid it

The case left-factoring was for is not two alternatives sharing an operand. It is one
alternative whose prefix *is* the other alternative, a call down:

```dotgram
Primary   = … | c: Call | r: Reference | …
Call      = target: Reference & '(' & … & ')' => …
Reference = …
```

Every bare reference is read twice — once inside the `Call` that then fails for want of a
bracket, once as itself — and references are most of what a grammar is made of. Nothing where
the two alternatives are written says so; what says it is `Call`'s own first operand.

So an alternative that does nothing with a call but hand its value on is replaced by the body
it would have called. That is an equality: the alternative built the rule's value out of the
callee's value and nothing else, and the callee's body builds the callee's value. The prefix
is then a prefix where the fold can see it, and the fold decides on its own terms whether
sharing it is invisible. An inline the fold does not then take is put back — on its own it
duplicates a body and saves nothing.

Both alternatives have to hand on a call, the two rules and the one holding them have to
declare the same type, and nothing the body captures may already be captured in the rule it
moves into.

### The shape that hid it, and what it cost

`CollapseTransparent` writes a forwarding rule's choice into its caller, which leaves one
`=>` outside a choice rather than one on each alternative — `(p: Call | p: Reference) => @(p)`.
Nothing matches an alternative there, so the construction is given to each alternative first,
which says the same thing in the shape the rest of the pass reads.

**And only on a rule's own body.** An alternative that is itself a choice — which is what
collapsing a forwarding rule into one alternative among several leaves — would become a choice
of constructions nested inside the choice above it, and the alternatives of a rule are the
ones at the top. Nothing would ever give those constructions a factory, and nothing did: the
expression language failed to generate on `Statement`, whose fourth alternative is a collapsed
`Control`.

That defect took an afternoon the first time and one run the second, which is the whole
argument for the dump above it. The first attempt was abandoned and reverted; what found it
was a throwaway tool outside the repository that runs the compiler on a grammar and prints the
lowered tree — no consumer project, so nothing about a broken parser stops it.

### What it is worth

The generated expression language changes: 645 bytes smaller, and `Primary` most of all. A
driver test counts what is actually saved, with a `when` rather than a `=>` — construction is
deferred to acceptance, so a factory runs once however many readings were tried and thrown
away, while a guard runs while the text is read. Two readings become one, and the longer
alternative still wins where it fits.

## Built: four widenings of what the fold can reach, and the measurement that did not close

Set out to answer one question with a number — write the notation's own grammar the way §11
does not oblige anyone to avoid, `Call | Reference` instead of the hand-written `RefOrCall`,
and see whether the compiler now gives back what the author gave up an afternoon to. It does
not, yet. Four things were in the way; three of them are gone.

**A rebuild must carry what a node carried.** The pass refused to run on a left-recursive or
a climbing rule at all, because their shape is named elsewhere by node identity. That is two
different problems wearing one coat. Rebuilding a node is bookkeeping and `Carry` — the
registry written for exactly this — answers it, so every rebuild here goes through it now.
Folding *alternatives* of such a rule is not bookkeeping: an alternative of a climbing rule
carries a binding power and a step of a fold carries its accumulator's name, facts about that
alternative which folding a run of them into one would destroy rather than move. So those
alternatives are refused one at a time, and the rest of the rule is walked.

**A name that is only handed back can be renamed.** One operand survives a fold and the rest
are dropped, so the survivor's name is what everything in the run will see. An alternative
that uses another name has to be rewritten, and the case that can be rewritten with certainty
is the one whose whole `=>` is that name. Anything else names its capture inside C# the author
wrote, and renaming it would mean editing that text — declined, not attempted.

**A rule reached under a different continuation is a question, not a refusal.** It used to be
answered no on the grounds that the walk had no answer yet, and that refusal is exactly what a
real grammar runs into: a reference whose type arguments are optional reaches itself through
them under a continuation of their own. It is asked now. The walk still terminates for the
reason the same-question case does — a pair goes on the path before it is walked and comes off
after, and there are finitely many.

**An atomic group has one reading because that is what atomic means.** `Determinism` looked
inside the braces, which asks a harder question than the braces already answer, and answered
it badly wherever the body was a choice or a star. That is every `trivia` written the way §4.5
recommends — `trivia = { (Space | LineComment | BlockComment)* }` — and so nearly every
grammar. `Name = Identifier & ('.' & Identifier)*` went from unprovable to determinate on this
one line.

### Where it still stops

`Reference` in the notation's grammar is still not proved to have one reading, through
`Reference → TypeArgs → Type → Reference`. Two of the links in that chain were the two
findings above; at least one more is in there. The corpus barely moves — 782,412 bytes of
expression language against 782,408 before these four and 783,053 with the pass off — which is
the honest shape of it: these widen what can be proved rather than what happens to be there.

The number the exercise was for is still owed. What it took to get this far is written down
so the next attempt starts from the chain rather than from the beginning.

## Built: the comparison is made past the trivia both sides read

The chain the previous entry left open — `Reference → TypeArgs → Type → Reference` — had one
more link, and bisection named it exactly. A cut-down grammar folds; add `trivia` and it stops;
take the inner repetition out of `TypeArgs` and it folds again:

```dotgram
TypeArgs = '<' & Type & (',' & Type)* & '>'   // not proved
TypeArgs = '<' & Type & '>'                   // proved
```

§4.5 weaves `trivia` between every pair of operands, so the loop is lowered with one at the
head of each turn and another standing after the loop:

```text
Sequence '<', trivia, Type, trivia,
  Repeat 0..*  Sequence  trivia, ',', trivia, Type
  trivia, '>'
```

`trivia` is nullable, so its characters join the first set of everything it leads, and the
ordinary test sees a turn beginning with whitespace and a continuation beginning with
whitespace and concludes the loop might give a turn back. On a grammar that follows §4.5's own
recommendation that is nearly every loop there is.

They do not begin alike. `trivia` is an atomic group — it commits its first reading and never
gives it back — so the same run of it is consumed whether the loop takes another turn or
stops, and what decides between them is what stands after it: a `','` against a `'>'`, which
share nothing. So the comparison is made there, and only where both sides really do open with
a call to the same atomic rule. Only the comparison moves: whether a turn can match nothing,
and whether a turn is determinate in itself, are asked of the whole turn as before.

### And the link after that one

The notation's `Reference` is still not proved, and the shape is now named: a repetition that
*ends* its rule.

```text
Name = Identifier & trivia & (trivia & '.' & trivia & Identifier)*
```

There is no node after the loop to read the shared trivia from — what follows is the caller's,
and the caller weaves a `trivia` there that this rule cannot see. The argument is the same and
the structure is not available: it needs follow sets computed *past* the trivia, which is a
second flavour of `FOLLOW` and a design rather than a line.

The corpus is unchanged by this — 782,412 bytes of expression language, the same as before it,
against 783,053 with the fold off — which is what a sharper proof looks like when the thing it
newly proves is not on the path anything took.

## Built: the continuation carried whole, and the boundary that is not an analysis gap

The previous entry called the next step a design — follow sets computed past the trivia. It was
written already. `FollowSets.Continuation` is a pair, and its second half is exactly that, with
a doc comment describing the case bisection had just found:

> What the continuation can begin with once a leading application of the namespace's trivia has
> consumed what it consumes. §4.5 puts that application at the head of every spaced seam, so a
> repetition whose turns lead with the trivia and the continuation behind it both start by
> reading the same run of it — and the question that decides whether a turn could instead have
> been the continuation is asked of what each reads *next*.

`NeverGivesBack` in the emitter has used it all along. `Determinism` could not, because it
carried a bare first set, which is why the previous commit needed a structural special case:
in a set the shared trivia can no longer be told from anything else.

So the walk carries the whole continuation now, threads it with `FollowSets.Precedes`, and
asks the seam-aware half where a turn leads with the seam. The special case is gone — the
general mechanism subsumes it, and it reaches the shape the special case could not: a
repetition that *ends* its rule, where what follows is the caller's and only `FOLLOW` knows it.
`TypeArgs` in the notation's grammar is proved determinate now, where before it was not.

The emitter is handed `Continuation(following, following)` for the present. Over-approximating
the seam-aware half by the plain one is sound — what can follow past the seam is a subset of
what can follow — so nothing it used to prove is lost. Threading the real pair through `Silent`
is a separate step and a larger one.

### And the boundary, which is not an analysis gap

`Primary` in the notation still does not fold, and the reason has changed kind. It is no longer
a set poisoned by trivia; it is that `Name` is genuinely not determinate under its own
continuation:

```dotgram
Name = Identifier & ('.' & Identifier)*
```

`FOLLOW(Name)` contains letters, because `trivia` is nullable and nothing in the grammar says a
name is read to its end. The hand-written parser reads one greedily and never gives it back;
the grammar does not say so, so nothing can prove it. `wordboundary` exists and applies to word
*literals* (§4.6) — a rule that spells a lexeme out gets no such protection.

That is the lexical layer, named in the memory of this project long ago and not yet built: a
rule that is a token, read once and to its end. The fold is waiting on the notation, not on the
analysis, and that is a better place for it to be waiting.

## Found: the notation already says it, and two grammars here did not

The last entry concluded that left-factoring was waiting on a lexical layer. It was not. The
notation has said the thing all along: an atomic group commits its first reading, and
`Determinism` answers `true` for one outright. So a rule that spells a lexeme can say it is
read once, and where it does, everything above it follows.

Written unfolded — `Call | Reference` rather than the hand-written `RefOrCall` — and with
`Name` and `Reference` wearing braces, the notation's own `Primary` comes out as:

```text
Sequence
  Capture 'target' → Call Reference
  Choice
    Construct => (GramGrammar.Call(target, first, rest))   ← '(' … ')'
    Construct => (target)                                  ← nothing
```

Which is `RefOrCall`, written by the compiler. Without the braces it does not fold; with them
it does. That is the number the exercise was for, and it took two pairs of braces rather than
a lexical layer.

### The diagnostic that was built, measured, and thrown away

A warning was written to say so: where a fold is declined and the operand is a rule that
recognizes text and builds nothing, name it. It fired on exactly three rules in the whole
repository — `FilterExample.Name`, `ExpressionLanguage.Dec`, `ExpressionLanguage.TypeName` —
which is precise rather than noisy, and the first two took the braces and were better for it.

The third broke two tests. `TypeName = Word & ('.' & Word)*` is *supposed* to give characters
back: a dotted name is a type only as far as it resolves, and the rest is member access. So
the advice was wrong on one real grammar in three, and being a warning in a repository that
treats warnings as errors, it failed the build of the grammar whose author was right.

It is not a diagnostic, then. The two rules are structurally identical to the one that must
not change; what separates them is what the author meant, which the compiler cannot see. The
guidance went into §4.5 beside the one about `trivia`, with the counter-example beside it.

### What the braces bought

`DecRun` alone: the generated expression language went from 782,412 bytes to 768,650 — 1.8%,
for saying what the grammar already meant. `FilterExample.Name` likewise.

`TypeName` is left alone, and now has a comment saying why.

## Measured: the notation written the natural way, against the notation written by hand

The question the whole factoring program was for, put to the instrument that answers it —
`SelfHostingTests.And_this_is_what_each_costs`, the hand-written front end against the
generated one, medians of sixty parses per file, three runs each way.

`GramExample`'s grammar was rewritten to the spelling §11 does not oblige anyone to avoid:
`Invocation | Reference` in place of the hand-factored `RefOrCall`, with braces on `Name` and
`Reference` saying they are read once. The compiler folds it — `Primary` comes out with one
read of `Reference` and a choice of what may follow.

| file | hand-factored, generated | spelled out, generated |
| --- | --- | --- |
| Csv.gram | 0.035 ms | 0.033 ms |
| Feed.gram | 0.060 ms | 0.060 ms |
| Minimal.gram | 0.161 ms | 0.162 ms |
| Notation.gram | 0.062 ms | 0.062 ms |
| Url.gram | 0.170 ms | 0.170 ms |

The same, within the noise of the runs themselves. Which is the answer: **the author no longer
has to know the trick.** Written the way it reads, the grammar gets the parser the hand-factored
one got.

**And the change was reverted anyway.** Generated code went from 283,084 bytes to 293,145 —
3.6% more, the atomic groups carrying commit machinery this shape does not otherwise need. Same
time, more code, so there was nothing to take. The example keeps the hand-written form and its
comment now says what the alternative costs, which is more use to a reader than either spelling
alone.

The differential against the hand-written parser passed throughout, on both spellings, which is
what makes the comparison worth anything.

## Built: a rebinding may change a type, which is what a `with` on a publication is for

`parse Sum with (Value = IntNumber) as EvaluateInt` beside `parse Sum with (Value =
DecimalNumber) as EvaluateDecimal` — one grammar, two calculators, one working in `int` and
one in `decimal`. The syntax was already there (`Notation.gram` publishes `parse List with
(Sep = …) as Loose`). What was not there was the type following.

**Three things stood in the way, and the first was a defect of the worst kind here.**

A rule declared `Sum : Value` — §4.1 case 3, "my value is `Value`'s" — resolved that name
through its own namespace, so a specialization made for `Value = DecimalNumber` kept the
original `Value`'s `int` while its body built a `decimal`. No diagnostic, and the consumer's
build failed with `CS0266: Cannot implicitly convert type 'decimal' to 'int'` about code they
did not write. The clone resolves it against what the specialization actually put in that
rule's place now; every clone is allocated before any body is cloned, so the replacement's own
clone is already in the map.

**The question collector had never paired two declared types.** It crosses declared types with
sequence element types (§4.1 case 2) and nothing else, so the assignability question the
rebinding check asks — is what replaces this compatible with what it replaces — reached the
pure half unanswered and threw. Every declared type is now crossed with every other, which is
the same superset that file already takes everywhere else.

**And the check itself refused the feature.** `'DecimalNumber' cannot replace 'Value': expected
a result compatible with 'int', found 'decimal'` — correct about a replacement that has to fit
somewhere fixed, and wrong here, because nothing was expecting the old type. A capture is where
a rule's value lands, and where every landing belongs to a rule declared `: Value`, they are
all following `Value` and follow it to the replacement too. Where one of them captures into a
declared C# type, or into a sequence, or hands it to a constructor, something *is* expecting
that type and the check stands.

`examples/DotGram.Examples/TwoCalculatorsExample.cs` is the whole of it: four arithmetic rules
whose `=>` bodies name no type, one rule that says what a number is, and two publications. The
tests hold it to `7/2` being `3` in one and `3.5` in the other, and to the `int` calculator
refusing `1.5` outright.

## Measured: the parser's own method is too big for the compiler below it to optimize

The question was narrow — does RyuJIT already remove the bounds checks and character reads
that a block repeats after its predecessors have done them? `Rfc3986` writes 1706 bounds
checks and 1801 reads of `text[p]` against 1364 advances of `p`, so at least 342 checks and
437 reads are made at a position that has not moved.

The answer is that it removes nothing, for a reason that is worth more than the question.

    Recognize_DotGram_Uri ... [Instrumented Tier0,       IL size=63423]
    Recognize_DotGram_Uri ... [Instrumented Tier0,       IL size=63423]
    Recognize_DotGram_Uri ... [Tier-0 switched MinOpts,  IL size=63423]

The method is compiled three times and, on the attempt to promote it, the JIT switches it to
MinOpts: no common-subexpression elimination, no bounds-check elimination, no assertion
propagation. The threshold is 60000 bytes of IL, and the recognizer is 5.7% past it.
`ExpressionLanguage`'s is 95267 bytes, 59% past. In the same run 59 methods reach Tier1 and
exactly one is switched to MinOpts — the one where all of the work happens.

What that costs, measured on the same engine and the same emission style under the threshold
(the `Links` recognizer of `examples/UrlExample.cs`, 9869 bytes of IL): 566-589 ns/parse as
compiled, 3311-3341 ns/parse with the JIT forced to MinOpts. The whole workload is in that
comparison, not the recognizer alone, so the factor for the recognizer by itself is smaller —
but it is a factor, not a margin.

This makes the size of an emitted method a first-order constraint rather than a matter of
taste, and it puts a step in the middle of it: a recognizer under the threshold is optimized
and one over it is not. `Rfc3986` needs 5.7% removed. It is also a candidate explanation for
the residue against a hand-written parser that `Url.gram` still shows — a hand-written parser
is small enough to be optimized, and this one is not.

## Built: the state graph is recorded where it is written, and held against the one read back

`Machine.Layout` needed to know where each state can go and recovered it by reading the
finished text back with two regular expressions — `goto S(\d+);` and the second field of a
`ParserEntry`. That made every jump's spelling load-bearing, and the two halves fail
differently when one drifts.

A missed `goto` leaves its target judged unreachable and so unwritten, and the jump then names
a label that is not there: the C# compiler says so. A missed resume leaves the state out of
the dispatch instead. The block is written, the code compiles, and a parse that should have
resumed there falls to the default and refuses input it ought to accept. Written out as a
plain readability edit — naming the second argument, `ParserEntry.Choice, state: 41` — the
whole solution still built and the parsers were silently wrong.

So the edge is now recorded by the same call that writes the text. `Label(at, state)` returns
the label and says that `at` can jump there; `Resuming(at, state)` returns the state and says
that `at` can put it in the arena. There is no second spelling to keep in step, because the
recording and the text come out of one call.

Both graphs exist for now, and `Verify` holds them against each other at the end of
`PlanLayout` — which every rendering that uses the state table goes through, the general
engine and both flat ones. It says which way they differ, because the two directions are
different defects: a state recorded and not recovered is the one that would have shipped, and
a state recovered and not recorded means the record is no longer the whole graph. Both were
watched to fail before this was written down.

Then layout was moved onto it. What is written at all, what order it is written in, and which
states the dispatch has a case for are now decided from the recorded edges rather than from
reading the text; the corpus is byte-identical through both steps, which is what says the two
graphs were the same graph.

`Tail`, `JumpOnly` and `Named` still read the text, and deliberately. Each needs where in a
body something stands, which a list of edges does not carry — and each fails in a direction
worth having: miss one and a jump is not dropped or a signpost is not collapsed, which is
larger output and not wrong output, while the one under-reporting failure there is names a
label that is not written, which the C# compiler refuses. That is a different class from the
one this closed.

`Redirect` still rewrites the text, and `Verify` now covers it too: the recorded side is
resolved and the recovered side is read after redirection, so a rewrite that failed to happen
shows up as the two disagreeing.

What the graph is for is what comes next — the things a local emitter cannot do: merging
blocks that are the same body to the same successor (77 of 1442 in `Rfc3986`), removing a
check or a read that every path in has already made, and getting under the threshold the entry
above measures.

## Built: two states that do the same thing are one state

Compilation writes a rule's shape wherever the rule is used, so the table it leaves holds the
same few lines over and over with only the states around them differing. Once redirection has
been over the bodies those differences are gone too, and what is left is one block written
many times. No site can see that: each is written by whoever needed it. It takes the whole
table at once, which is what the recorded graph is for, and it is the first thing in layout
that is an optimization rather than a tidying.

The criterion is two conditions and the second is the one that is easy to miss. The bodies
have to be the same text, which after redirection means they do the same thing. And the body
has to end by jumping somewhere — a body that can fall out of itself does not say where it
goes, two states that read the same can be laid out before different things, and merging them
would send one of them somewhere it never went. That guard does not fire on anything here:
bodies are compiled continuation-passing and end with a jump, and layout only drops that jump
later. It stays because the reasoning is not obvious and the next emitter to write a body that
falls through would not think of it.

    Url snapshot        427 states -> 254        9875 lines -> 6534
    Rfc3986             1442 states -> 1205      63423 bytes of IL -> 47463

Every behavioural test was green through the change; only the three snapshots moved, which is
what says the parsers do the same thing and only the text of them is smaller.

**It converges in one round.** Collapsing one state into another can leave two more identical,
so it runs to a fixed point — but capped at one round the output is byte-for-byte what it is
uncapped, on every grammar here. The loop stays for the case that is not here yet; it costs
one more pass that finds nothing.

**What it cost, and what it bought.** Redirection is two passes of a regular expression over a
body, and doing that to every body each round was most of the cost of merging. The recorded
graph says what a body names, so only the bodies naming a state that has moved are written
again. After that the generator does more work and the consumer's build is faster anyway:
3669 ms to 3076 ms for `DotGram.Parsers`, because the C# compiler is handed a quarter less
code than it was.

**What it did not buy, yet.** `Rfc3986` is now well under the 60000-byte threshold the entry
above measures, and the JIT still switches it to MinOpts — so a second one of that mechanism's
limits is binding, and the parse time is unchanged. Which limit, and what it would take to get
under it, is the next question rather than an answered one.

## Measured: what the JIT gives up on is branches, not size

The entry above left the wrong number to aim at. `Rfc3986` was brought well under the
60000-byte threshold and RyuJIT went on refusing to optimize it, so a second limit was
binding and it was worth finding out which rather than guessing.

A harness generates grammars of a given size, compiles each, loads it and runs it hot, and
reads the counts back out of the IL — size, instructions, references to locals, and basic
blocks — beside what the JIT decided to do with it. Two shapes: a flat one and a recursive
one that compiles through the arena, which is the engine the real parsers use.

    flat    G44  IL 24923  instrs 11607  lvRefs 4102  blocks 1981   optimized
            G45  IL 25511  instrs 11884  lvRefs 4199  blocks 2028   MinOpts
    arena  R120  IL 39171  instrs 16344  lvRefs 7268  blocks 1986   optimized
           R122  IL 39894  instrs 16656  lvRefs 7394  blocks 2022   MinOpts

The two shapes differ by 57% in IL and 41% in instructions at the point they cross, and
agree on basic blocks to within 2%. Nothing else is even close to its documented limit —
instructions 11884 of 20000, references 4199 of 8000, locals 9 of 2000, size 25511 of 60000.
The count that binds is basic blocks. RyuJIT's own limit is 5000 of its blocks against the
~2000 IL leaders counted here, so it makes about two and a half of its own per leader; that
ratio is inferred, and the crossing itself is measured.

It is not about instrumentation. With tiering off, 9649 methods in the same run compile at
FullOpts and exactly one is switched to MinOpts — the one where all the work happens.

Where the real parsers stand, on the same counter:

    Rfc3986.Recognize_DotGram_Uri            blocks 3918   x2.0 over
    ExpressionLanguage.Recognize_DotGram     blocks 4486   x2.2 over
    Rfc3986.Recognize_DotGram_UriReference   blocks 7755   x3.9 over

So the thing to cut is branches, and the two ways to cut them are fewer branches per state
and fewer states per method. Only the second is certain to be enough.

## Built: a character class wider than two ranges is read from a table

A class was written out as comparisons — `(c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')
|| …` — and every `||` and `&&` in it is a branch. `Recognize_DotGram_Uri` held 1232 of
them. Read from a table the class is one test whatever it contains:

    if (!(c <= 127 && Recognize_DotGram_Uri_Class3[c] != 0))

Only for a class that is entirely ASCII, which is what makes the table 128 bytes and the
guard one comparison; a Unicode category or an inverted class keeps the comparisons. A plain
`byte[]` rather than a span over a literal, because the span form leans on RVA lowering and
netstandard2.0 is the floor this emits for. One table per distinct class — `Rfc3986` declares
twelve.

**Three ranges is where it starts to pay, and that was measured rather than reasoned.** Two
would already win on branches and loses anyway — a narrow class is comparisons on values
already in registers against a load that may not be in cache:

    threshold 2   blocks 3344
    threshold 3   blocks 3291
    threshold 4   blocks 3835

    branch operators in one method   1232 -> 501
    Rfc3986.Recognize_DotGram_Uri    3918 -> 3291 blocks   (-16%)
    Rfc3986.…_UriReference           7755 -> 6518
    ExpressionLanguage               4486 -> 4275
    parse                            1226 -> 951 ns        (-22%)

The parse got faster without crossing the threshold at all — it is still MinOpts, and fewer
comparisons per character is worth that much on its own. The generated file grew by the
tables (`Url`: 6534 lines to 6804) while its IL shrank, which is the trade being made.

What it does not do is close the threshold: 3291 against ~2000. It changes what closing it
takes — two parts rather than three for `Uri`, and with margin instead of on the line.

## Built: a recognizer too big for the compiler below it is written in several methods

The limit is per method, so the answer is more methods. They are local functions, which the
C# compiler gives a method of their own — and so a budget of their own — while writing the
frame that carries what crosses between them. Nothing about the states changed: they go on
naming the same variables, which are the enclosing method's now rather than their own.

    Rfc3986.Recognize_DotGram_Uri     one method   3291 blocks   MinOpts
                                      driver        344 blocks   FullOpts
                                      part 0       1114 blocks   FullOpts
                                      part 1       1244 blocks   FullOpts
                                      part 2        936 blocks   FullOpts

    parse   951 ns -> 344 ns          nothing in the run switched to MinOpts

Two and a half times, and three and a half against where this began. The whole of it is the
compiler below being willing to look at the method at all.

**Two things cannot be captured, and both were found by the compiler rather than by
thinking.** A `ReadOnlySpan<char>` is a ref struct and is not a field of any frame, so `text`
is handed to each part instead. A `ref` parameter cannot be captured either, so `failure`
goes the same way. Everything else — the position, the arena indices, the turn counters, the
capture slots — the frame carries.

**Where the cut falls matters more than where the budget wants it.** Control reaches the
dispatch about four times in a whole parse — measured, on a URL — so a crossing that goes
through it costs nothing worth counting. A `goto` that crosses is another matter: those run
per character. So the budget says roughly where to divide and the cut then moves to where the
fewest jumps cross it. They are never zero: the layout threads states into chains and the
chains are woven, so the cleanest cut in `Rfc3986` still has 59 jumps across it. It is fast
anyway, which says those jumps are not the ones being taken.

**Two defects on the way, and the first is the one worth writing down.** The dispatch answers
to what an arena entry says, and what it says is not always where control ends up: a state
that does nothing but jump somewhere is collapsed, and its old number still has to be
answered. Keying the new dispatch by the resolved state lost the case for every collapsed one
— 78 tests, all of them a parse refusing input it should accept. The second was quieter and
louder at once: a chain the layout threaded across a cut has its jump dropped for being the
next line, and the next line is in another method now, so the state it named lost the only
thing that named it — and the C# compiler said `No such label`.

**What it costs.** A build without optimization gets slower, and by about what it gains with
one: the `Url` benchmark in the test suite went from 2.87 times a hand-written parser to
5.51. In a Debug build the extra call and the second dispatch are paid and nothing is
optimized in return.

That is the price and it is paid. Dividing only where the consumer's compilation is optimized
was raised and refused: a build configuration may change diagnostics and it may not change
algorithms or behaviour, so the parser stepped through in a debugger is the parser that ships.
Written down in `.claude/rules/emitted-code.md`, where the appeal to a rule that had permitted
this was wrong — that rule is about the language version, and names Debug and Release only as
an analogy for two spellings of one thing.

**And one method is still over.** `ExpressionLanguage`'s materializer is 2004 blocks and is
not a recognizer, so nothing divides it. That is the next one of these.

## Measured: what a parser spends its time on is not the same question in two grammars

`Rfc3986` reads a URL in 20 traced steps. `ExpressionLanguage` reads
`(int x, int y) => (x + y) * 3 - x / 5 + y * (7 - x)` in 1516 — thirty steps a character
against a third of one. The two are not the same engine doing more of the same work.

    Rfc3986        20 events, 0 failures
    ET           1516 events: fail 228, push choice 177, resume 176   (38% backtracking)
                             return 125, rule capture 124, open capture 121

A first pass at this read the static count of emission sites instead and concluded the
opposite — that ET's arena is mostly captures. At run time it is mostly ways back. A count of
what is written is a fact about size; only running it says what it does.

**The abandoned attempts are shallow.** Of ET's failures, 33% consumed nothing before failing
and 49% consumed one character: 82% are settled inside two characters. That is not ambiguity,
it is prefix overlap — the kind a decision procedure removes.

**And they were not being decided at all.** `Assignment` has twelve alternatives, eleven of
which begin with a `Name`, and `Name` begins with `Word`, and `Word` begins with `[\p{L} |
'_']`. `Determinism.Distinguishable` said no, and its own comment said why:

> Knowable is not the same as worth writing down: a Unicode category is a few hundred ranges,
> exact and useful to the analyses, and a dispatch spelled out over them would be a page of
> comparisons where the alternative's own test is one call. The set stays precise; only the
> rendering declines.

The proof was never the problem. `Rfc3986` has no categories at all — every set narrow, every
choice decided, no backtracking whatever. ET is built on identifiers, and an identifier begins
with a category, so prediction switched itself off exactly where it was needed.

## Built: a set too wide to write out is held as its bounds and searched

Three renderings now, and the widest set has one too: comparisons while there are few enough
to read, the ASCII table from the entry above while the set fits in it, and a searched array
of bounds for everything else. A hundred ranges is seven steps of binary search, on a path
taken once per alternative rather than once per character.

With that, the width limit inside `Distinguishable` had nothing left to protect and is gone —
the proof was always over the exact sets. `ExpressionLanguage` now declares five arrays of
bounds and calls the search from thirty places that used to try an alternative and take it
back.

    ET, the same input     events   1516 -> 1160   (-23%)
                           ways back 581 ->  397   (-32%)
                           failures  228 ->  149   (-35%)

    Rfc3986                unchanged, and expected to be: no categories, nothing was declining

A third of the backtracking, from removing a rendering concession rather than proving anything
new. The remaining failures are the ones the ceiling above says are worth chasing — one and
two characters deep — and they are now the ones where the sets really do overlap.

Not measured: what this is worth in wall-clock for ET. `Parse` returns a LINQ expression tree
and building it dominates the call, so the parse cannot be timed through the public surface as
`Rfc3986`'s can. The event count is what changed and what is reported.

## Built: the materializer is divided the way the recognizer is, and the estimator learned
## the shape it was blind to

The entry on dividing recognizers ended with the one method still over the line:
`ExpressionLanguage`'s materializer, 2004 basic blocks, not a recognizer and so not divided
by anything. It is now — the same way, local functions the C# compiler writes the frame for,
each part a switch over its share of the value rules, the driver's switch calling the part a
rule's case lives in. The case bodies move verbatim: every `continue` in them belongs to an
inner loop of its own, which was checked before anything was moved, not assumed.

The estimator was the actual work. `Branches` counted `if`, `goto`, `case` and the short
circuits — the shapes a recognizer is made of — and read the materializer at half its real
size (1073 against 2004 measured), because a materializer is made of the shapes it did not
count: 181 `for` loops, three blocks each; 79 conditional expressions, two arms each; 352
`break`s. Counting those it reads 2126 against 2004, and the split fired.

    Materialize_DotGram      2004 blocks, MinOpts   ->   driver + 938 + 1008, all FullOpts

With that, nothing in a run of either real parser compiles at MinOpts any more — the count
of methods the JIT gives up on, across `Rfc3986` and `ExpressionLanguage` both, is zero.

The recognizers' own cuts did not move under the widened estimator — the corpus is
byte-identical and the suite green without a snapshot touched. That is luck as much as
stability: the estimates all grew, and the cuts happened to land in the same gaps.

## Tried and taken back out: a probe at a settled loop's head

The remaining backtracking in `ExpressionLanguage` divides by the pair of characters an
abandoned attempt died on, and the census pointed at loop turns: an attempt reads the trivia,
meets `+` where its own operators live, fails, and pays a standing exit, an unwinding and a
resume to learn what one character past the trivia already said. So a probe was built at the
head of a settled star — scan past a nullable leading operand that has a scanner, test the
character against the remainder's first set, and leave without entering when it says no,
writing the Repeat entry's last-end on the way out so the previous standing exit stays
visibly stale.

Measured, it refused itself. The turns it was aimed at are the loops of rewritten left
recursion, which do not meet its conditions — `ExpressionLanguage`'s event count moved from
1160 to 1158. And where it did fire, list grammars, it costs an extra scan and test on every
successful turn to save one refused entry per loop: the exact wrong side of frequency times
complexity, which is the criterion these have to answer to. Reverted.

What survives is the diagnosis. The one-character failures that remain live in the fold
loops, and a probe that pays would have to (a) reach those loops and (b) cost nothing on a
successful turn — enter the body past the trivia the probe already read, instead of reading
it twice. Both are design work on the fold machinery, not a guard bolted on beside it.

## Built: a method left past the limit is told about, as GRAM5003

The dividing machinery keeps every method of the real parsers under the line the JIT stops
optimizing at, but the grammar is the consumer's, and one rule big enough on its own can put
a method past it with nothing said — only a parse that quietly runs several times slower.
Now it is said: a warning, because the parser is correct, with the numbers the generator
acted under — the estimate, the ~2000-block line, the 1500 budget it divides under — and the
remedy, because the remedy is chosen against those numbers.

Emission gained a diagnostics channel to say it through: `CSharpEmitter.Emit` takes an
optional collection and `GramCompiler` passes its own, which is the first diagnostic to come
out of the emit stage at all.

**The first detector measured nothing, and the test caught it before it shipped.** It read
the part costs off the layout's own bookkeeping — and the flat rendering writes straight-line
code past the state table, so for exactly the method most likely to be left whole the costs
summed to zero. The detector that stayed sweeps the finished text instead, method by method,
local functions as methods of their own — which is how the JIT meets them — with the same
estimator the dividing uses. One detector for every rendering there is and every rendering
to come, because it measures what was written rather than what a writer remembered doing.

The materializer keeps one detector of its own beside the sweep: a single switch case over
the limit can name the rule it builds, and the remedy — build the value in a method of your
own called from the `=>` — is worth saying with the name in it.

Watched to fire on a 1200-literal rule and stay quiet on the whole corpus: the real parsers
are all under the line now, which is what the last three entries were for.

## Built: the second account of the graph is gone, and so are the patterns that read it

The entry above minted the marks. This deletes what they replace: `Redirect`, both regular
expressions, `MeansAState` with the table of what each entry kind's second field means, and
`Verify` with the comparison it existed to make. 258 lines out, 55 in.

`Verify` went because it had nothing left to compare. It held the recorded graph against one
read back out of the text, and the two now come out of one call with one argument — `Label`
records the edge and mints the mark, `Resuming` the same — so they cannot disagree. Keeping
it through the transition earned its place twice on the way here, catching a body left
unsettled that the balance check would not have seen.

`MeansAState` went because the hazard it guarded is gone by construction. A capture slot
could be rewritten as a state when that table said the wrong thing about a kind — a silent
corruption the project has already had once. Nothing rewrites a plain number now; only a
mark, and only `Resuming` makes one, so a slot cannot be mistaken for a state whatever
anyone forgets.

**What is left to guard is the forgetting itself, and the two tests that went are replaced by
one that guards it.** A site that writes a state's number instead of asking for a mark leaves
a number nothing will move, and what that costs is exact and quiet: the arena resumes at a
state that was not written, the dispatch has no case, and the parse refuses input it ought to
accept. So the emitter's own source is read: an arena entry whose kind carries a state must
write it as `{Resuming(...)}`. The three fixed labels are exempt and say why — they are never
collapsed, and a mark there would leak, since settling runs over the state bodies and the
root call is written into the file. Watched to fail by taking one site back to a bare number.

`Gotos` stays, for the two readers that work on the finished text rather than on the graph —
the departure rewriting a divided method needs, and which labels are still named once the
chained jumps are dropped. Both are positional questions about the text, and both fail into
larger output or a label the C# compiler says is missing, which is the other class.

## Profiled: what `ExpressionLanguage` spends a parse on, and the two guesses that were wrong

`Rfc3986` parses a URL in 344 ns. `ExpressionLanguage` takes 13 to 110 microseconds — two
orders apart — so it is the one to look at. A harness that parses expressions in a loop, a
dotTrace snapshot, and then, because the snapshot is not readable outside the GUI,
`dotnet-trace` into speedscope and a reader for it.

**The sampler's leaf attribution is not to be trusted here**, and two hypotheses taken from
it were refuted by experiment rather than by argument. It puts 89% of self time in a GC poll
worker and 63% under array copying inside recognition; removing the only list the recognizer
adds to — the expected-set accumulation on failure — changed nothing at all, and the theory
that a large earlier parse leaves buffers whose clearing costs a later small one was refuted
the other way round: after a large parse the small one is *faster*.

What direct measurement says instead:

    (int x) => x            builds        22 563 ns
    (int x) => ###          builds not     3 883 ns    - never reaches the body
    ###                     builds not       278 ns

    Expression.Lambda by hand                326 ns    - 1.6% of the parse
    new State()                                6 ns

So the tree it builds is not the cost, and steady-state allocation is 1.9 KB for the first of
those — the 76 KB an early measurement showed was the arena growing on a thread's first
parses, which is not what a parse costs and was my mistake to report. **Reading a body of one
identifier costs 18 microseconds.** The trace says why: on `(int x) => x * x - 1`, three
identifiers in the text, `Name` is called seventeen times and `Target` eleven. That is
`Assignment` — ten compound-assignment alternatives each reading `Target` and failing on the
operator, and the eleventh reading it again.

`GRAM4016` exists to say exactly this and was silent, because it asked whether the shared
operand leads back to the rule — whether the cost compounds with nesting. Here it does not
compound; it is merely paid eleven times, and eleven times was most of the parse.

## Fixed: an atomic group leaves no way back into the middle of it

`Doors` answers "whether matching something can leave a way back into the middle of it", and
answered it for an atomic group by walking inside the braces — where it finds the repetition
and says yes. An atomic group commits its first reading: a failure reaching past it has the
group to give back and nowhere inside it to resume. The same question `Determinism` used to
ask of braces and stopped asking.

Found while widening `GRAM4016` to the flat case: with the wider check, nine of fourteen
sites were the trivia §4.5 weaves between operands — which an author cannot factor out and
which the old `Reaches` condition had excluded by accident — and of the rest,
`FilterExample`'s `Expr` was reported only because a braced `Name` was said to leave a door.

**What it changes is not what the commit that made it said.** It said the compiler then folds
that operand and there is nothing left to report. It does not: the fold's own condition is
`Determinism`, not this, and the fold still declines. What the corrected predicate does is let
a capture inside the braces live in a variable — `FilterExample`'s parser loses an arena
entry, a trace call and a backward scan of the arena at every close of `Name`, and goes from
3433 lines to 3408. The five snapshot grammars are byte-identical.

So with the wider check landed, that site would go quiet while the operand is still read three
times. The check asks `Doors` and the fold asks `Determinism`, and the two are not the same
question — which is the thing to settle before the wider check lands, rather than after.

Not measured: what the capture moving out of the arena is worth in time. A harness over
`Filter` swung between 3.5 and 21 microseconds on the same build across processes, six times
over, so it says nothing in either direction and no number from it is reported here.

## Built: the JSON example reads its digits once

`Number = Digits & Fraction | Digits` reads the digits once for each alternative. Written with
the fraction as an optional tail it reads them once, and it is the same language here because
a fraction begins with `.`, which digits cannot contain — no shorter reading of the digits
lets a fraction fit that a longer one refuses.

## Built: the fold says what it could not do, and three rules in `ExpressionLanguage` stop
## reading their operand twice

`GRAM4016` was raised by a check of its own, which had to guess what the folding pass would
do with the same alternatives — it asked `Doors`, "can this leave a way back into the middle
of it", where the fold asks `Determinism`, "does this have one reading where it stands".
Those are different questions, and widening the check to the flat case showed how different:
of sixteen sites it named across this repository, **nine were the trivia §4.5 weaves between
operands**, which no author wrote and none can factor out, and **four more were operands the
fold went on to share anyway**. Three were real.

So the diagnostic moved into `Share`, which is the one place that knows: a run of alternatives
sharing an operand was found, `Determinism` was asked, and the answer was no. Nothing to
guess, and the sentence it can now write is the useful one — the remedy is to make the
operand's reading provable, not to rearrange anything. The check and its `Leading`,
`SameShape` and `Reaches` go with it.

And the scope is what a profile asked for. `Reaches` used to decide whether to say anything
at all, on the grounds that a cost compounding with nesting is the one worth interrupting an
author for. It now decides nothing: `Assignment` reads a non-recursive operand eleven times,
and reading a body of one identifier cost 18 microseconds.

### The three that were real

    Target      2 readings of `Name`       -> `n: Name & ('.' & member: Word)?`
    Primary     2 readings of `NamedType`  -> `& args: Arguments?`
    NamedType   2 readings of `TypeName`   -> `& args: ('<' & ... & '>')?`

`Target` is the one the profile was pointing at: eleven alternatives of `Assignment` begin
with it, so its own two readings were twenty-two, and folding it folded them — `Assignment`
went quiet without being touched.

`NamedType` needed care that the shape did not show. Its guard — `when Resolves(name)` — was
on the plain alternative and not the generic one, and that is load-bearing: `List<int>`
resolves where `List` alone does not. Moved in front of the arguments it would have refused
every generic type whose bare name means nothing, which is most of them. It asks where there
is nothing else to say what the name is: `when args != null || Resolves(name)`.

`TypeName` itself is left alone, and its own comment says why: a dotted name is a type only
as far as it resolves and the rest is member access, so it has to be able to hand a trailing
word back. Braces there would be wrong, which is why the remedy was the tail and not the
lexeme.

    (int x) => x * x - 1                       23 189 ns -> 18 073   -22%
    (int x, int y) => (x + y) * 3 - x / 5 …    51 500 ns -> 39 650   -23%
    (int x) => ((((x + 1) * 2) - 3) / 4) + …  109 976 ns -> 83 707   -24%

The shortest input, `(int x) => x`, is not reported: it measured 13 193, 18 515, 20 006 and
22 421 ns at different points of this session on builds that did not differ in anything it
touches, so nothing it says now would mean anything.

### Two mistakes on the way, both caught by the corpus

The capture in `Target` was first written `member: ('.' & Word)?`, which captures the group
and so puts the dot in the member's name — `'.Source' is not a member of type
'System.Exception'`, said by LINQ rather than by anything here. It belongs on the word:
`('.' & member: Word)?`.

And the first `NamedType` had the guard in front, which is the generic-type mistake above. It
compiled, and the corpus caught it.

## Fixed: refusal was exponential in the number of integer literals

The first run of the new `ExpressionBenchmarks` paired each graded nest with itself minus
its final operand, and the pairs said: accepting is linear, refusing is not. 74, 327,
1299 us at two, four and six parentheses — a little under four times per two levels, and
the gap against accepting the same text widened from 2.9x to 17.3x.

### Finding it

A `DOTGRAM_TRACE` build counted rule calls per input: every lexical leaf multiplied by
four per two levels, and `call Unary at <end>` — the whole failing suffix retried — went
4, 16, 64. Not the repetitions: the trace holds **no `give a turn back` at all**. The
walk from one arrival at the end to the next showed the mechanism whole:

    fail state=1 at 25          the parse fails with " + " left over
    resume state=1593 at 23     ...and lands on a live choice inside the nest,
    construct in Primary        which succeeds AGAIN over the same span,
    ...                         closes the outer ')' again, folds '+' again,
    call Multiplicative at 27   and rereads the suffix to the end

State 1593 named the culprit. `Primary` read a bare integer twice:

    | token: Dec & when @(int.TryParse(...)) => @(int constant)
    | token: Dec                             => @(long constant)

The fold shared `Dec`, but the choice between the tails stayed live — and both tails read
nothing, so the second reading consumes exactly the digits the first did and can never
change what fits after it. It is not a choice about the text at all. Every unsuffixed
integer literal left one such way back, a refusal walked every combination:
2^(literals) rereadings of everything after them.

The exponent counts literals, not depth, and that was the test of the diagnosis before
any fix: the same six-deep nest with names for operands traced 1,813 lines against
39,838, and with `L`-suffixed literals — one reading each — 2,713.

### The fix

The decision moved into the factory, where it never was a choice:

    | token: Dec => @(int.TryParse(token, ..., out var small)
                     ? Expression.Constant(small)
                     : Expression.Constant(long.Parse(token, ...)))

Same language, same values, one reading, nothing left alive. The trace goes 985, 1540,
2095 lines — +555 per two levels, flat — and the benchmark:

    refusal, 2 parens     73.8 us ->  30.8      5.9 KB -> 2.7
    refusal, 4 parens    327.1 us ->  47.0     19.4 KB -> 3.6
    refusal, 6 parens   1299.3 us ->  67.1     70.6 KB -> 4.5

Linear, and now cheaper than accepting at every depth, which is the right way round: it
reads one character less. Accepting itself moved inside noise (25.9/47.5/75.2 ->
25.4/45.5/71.4). 1505/1505 green.

### What this generalizes to, not built

The engine keeps every untried alternative of a succeeded choice live, which is ordered
choice working as specified — the cost shows only where a later alternative can *succeed
over the same span*, because that retry rereads the whole rest of the input for nothing.
The `Dec` pair was the one such place on this benchmark's path, but the shape exists
elsewhere: `If` with and without `else` is two alternatives whose shorter one always fits
inside the longer, so a refusal past a nest of ifs would pay the same way. Two possible
answers, neither taken today: the fold could commit the residual choice where every
remaining tail is provably empty (this case exactly), or general memoization, which is a
different engine. Worth measuring on an `if` nest before deciding anything.

## Built: the fold commits its residue, and the grammar takes its pair back

The int/long fix of the previous entry was a workaround, and it was called one: "ты
сейчас замазал проблему пользовательским кодом". Merging the pair into one factory works
for two alternatives and a ternary; three guards in a row have no such spelling, and the
engine was the thing being wrong — past the shared operand the choice was about which C#
runs, not about the text, and the engine kept a way back into a choice that could not
change anything.

So the fold now commits that residue. `Sharing` wraps the choice of tails in an atomic
group when three things hold:

- **The tails read nothing** — guards, factories, `none`. Then resuming a later tail
  lands on the same position with the same text ahead, and can only walk to the same
  failure.
- **No value window.** Which factory ran is invisible during recognition with one
  exception: a `when` elsewhere can materialize a value built over this rule and read
  the difference. So a rule reachable through calls from any capture a guard names —
  `FreeNames` says which; no scanner means every capture of a guarded rule — keeps its
  residue uncommitted, and so does a residue folded inside a capture the rule's own
  guards name.
- **Nothing else.** The first cut also demanded that nothing following the choice could
  begin with a trivia character, because the guarded tail led with a woven seam and the
  bare tail did not, so the two ended at different positions. That condition drowned:
  `FOLLOW(Primary).AfterSeam` held whitespace fed in by a nullable operand between two
  seams somewhere across the grammar, and one polluter anywhere refuses every site.

### The seam that should not have been there

The asymmetry itself was the bug. §4.5 weaves trivia between operands, and a guard is
not one — it reads nothing, so there is no token on its far side for a seam to separate.
Weaving one anyway cost a trivia scan per guard evaluation, made `parserSpan` include
trailing whitespace, and gave an alternative ending in a guard a longer extent than its
guard-free twin over the same tokens. The weave now skips guards; §4.5 says so; with it
gone, the guarded pair's tails are both pure epsilon and the commit needs no global
condition at all.

### What broke on the way

Two shape-matchers knew the fold's residue as `Sequence([..., Choice(Constructs)])` and
did not see through the atomic: `Fold.Shared` (which offers the tails' factories) and
`CaptureLayout`'s shared-head detection. "A construction in 'Primary' has no factory"
named the first; both unwrap the atomic now.

And one non-failure worth writing down: the first run after the change showed the old
exponential byte for byte, because the compiler server was still holding the old
generator. `dotnet build-server shutdown` is part of measuring a generator change.

### Measured, with the original pair restored

    trace lines, refusals at 2/4/6 parens:   2,346 / 9,872 / 39,838  ->  991 / 1,552 / 2,113
    benchmark, same refusals:                  74  /  327 / 1,299 us ->  35  /  55  /  76 us
    allocation:                               5.9  / 19.4 / 70.6 KB  -> 2.8  / 3.8  /  4.8 KB

Linear in both directions, with the guarded pair written the natural way. 1511/1511
green, six of them new: the committed shape, both refusal conditions, the answers
unchanged, and the seam count around a guard.

Against the ternary workaround the committed pair costs about ten percent on this
parser's refusal path — an atomic entry per literal and the commit walk over the entries
above it, where the merged factory left nothing in the arena at all. A dedicated
rendering for an all-epsilon committed choice — evaluate the guards, pick the factory,
write no entry — would close that; noted, not built.

## Built: a committed choice of weightless tails compiles as a decision

The follow-up the last entry noted. The committed residue was correct but paid rent: an
atomic entry per literal, a choice entry inside it, and the commit walk that put the
choice out — machinery for remembering a way back, spent on a choice proven to have
none.

Two changes, one of them one line. A guard's refusal now goes to wherever failure is
routed rather than to a hardwired `Fail:` — which today is `Fail:` everywhere else, so
every existing snapshot is byte-identical; a guard was never compiled under a redirected
failure before, because `Silent` refuses guards and every `_fail`-setting path demands
silence. And the emitter's `Atomic` case takes a committed choice of weightless tails —
guards, factories, `none`, the same shape the fold's `Committed` builds — and compiles
it as the decision it is: evaluate each guard where it stands, fall to the tail behind
on refusal, take the first that passes. No choice entry, no atomic boundary, no commit
walk.

The generated line is the sentence that started this: `if (!Guard10(token)) goto S1583;`
— worked the `when`, or call the other piece of C#.

### Measured

The trace is the flat answer: 985 / 1,540 / 2,095 lines on the graded refusals —
character for character the stream the hand-merged ternary produced. The engine now
compiles the natural two-alternative spelling into what the workaround was by hand.

Wall clock did not move against the previous commit (35/55/77 us), and the previous
commit's "ten percent against the ternary" deserves a correction: the floor input —
`(int x) => x`, which none of this touches — drifted 7.2 to 8.0 us across the same three
runs, so most of that gap was the machine warming through the evening, not the atomic
entries. What is real and remains is allocation: 2.82 KB against the ternary's 2.73 at
depth two, which is the guard itself — `when` runs while the text is read (§8.1), so
`token` is built as a string once per literal to ask it. That is the semantics of
writing a guard, not a cost of the choice, and the deferred ternary factory is the
spelling for whoever refuses to pay it.

1511/1511 green, snapshots untouched.

## Measured: what a valued rule boundary costs, and the hypothesis it killed

The self-hosting gap is the project's own criterion and the one number it is behind on, so
it was the thing to attack next. Attacking it began with a wrong guess, and the value of
writing this down is which measurement killed it.

### What the trace said, and what I read into it

`GramGrammar` reading `Url.gram` — 3,053 characters, the worst of the corpus:

    call            1020        fail             118
    return          1017        resume            61
    rule capture    1014

One arena write per input character, and almost no backtracking: 118 failures in 4,453
events. And the calls are concentrated — `QuantifiedCore`, `Primary`, `Prefixed`,
`Captured` and `Quantified` are called 128 to 130 times each, which is one operand walking
a chain of five rules that each read an optional decoration and, when it is absent, hand
the body on unchanged. 650 of the 1,020 calls.

So: frame traffic, and the fix is to collapse the chain. That was the hypothesis.

### Why neither existing mechanism collapses it

Worth recording, because it is not what I first thought. `ExecutionPlan.CompiledInPlace`
refuses a rule that declares a type, has results, or holds a `Construct` — all five do, so
recursion never even comes into it. `SitedValued` refuses on two counts: every member must
be a span of the input (`member.Rule is not null` returns false, and each link's value is
built from the next link's value), and the callee must be outside every cycle (`Primary`
leads back through `'(' & Body & ')'`). The chain is exactly the shape both passes exclude.

### The measurement

`CallCost` has priced a rule boundary since the first proofs, and what it prices is a
*valueless* one: its `Letter` writes a `Call` entry that its own return takes straight back
off the arena and leaves no `RuleCapture`. Every rule in a grammar that builds anything is
the other kind. So a valued row was added, and the two read against each other:

    compiled in place            648 ns    104 B
    called                       840 ns    104 B     ->  4.8 ns a boundary
    called and valued          1,823 ns  1,408 B     ->   29 ns a boundary

Six times, and 29 ns is the one a grammar is made of.

Against the gap: `GramGrammar` writes 1,014 rule captures on that file, so its valued
boundaries are about **30 of the 113 microseconds** it is behind. A quarter — not the
answer. Collapsing the five-rule chain to one would take four fifths of the chain's share,
about 15 us, or **13% of the gap**. A large refactor of two normalization passes for 13%
is not the thing to do next, and without this number it would have been done.

Where the other three quarters are is not established. What is ruled out: materialization
(the profile puts 97% of the time inside `Recognize`), and allocation — the generated
parser allocates **26 KB against the hand-written parser's 64 KB** on the same file, two
and a half times better while being nearly seven times slower.

### And the standing instrument understates the gap

`SelfHostingTests` reports 3.48x on this file. Warmed properly and run for three seconds a
side, it is **132 us against 19.4 us — 6.8x**. The test does 60 parses with no warm-up, and
cold start costs the hand-written side proportionally more, which compresses the ratio.
The test is a differential first and a timer second, so this is not a bug in it; but 3.5x
is not the number, and the memory of "2-3x" was formed from readings like it.

### One methodology note, because it nearly shipped

The valued row was first written `Letter : @string = ['a'..'z'] | '!' & Letter`, which is
not a valued rule: a rule that captures nothing is worth the text it matched (§4.1 case 4),
the declaration is dropped, and it normalizes to the valueless row character for character.
It benchmarked at 803 ns against 856 — *faster* than the row it was supposed to be slower
than, and with identical allocation, which is what said to go and compare the normalized
shapes. A capture is what makes a rule valued.

## Measured: the self-hosting gap is deferred construction, and the entry above got it wrong

### The correction first

The entry above says "materialization, which the profile puts at 3% against `Recognize`'s
97%". That is a misreading of my own profile and it is wrong. `Materialize_DotGram` is
called from *inside* `Recognize_DotGram`, at `Accept:` — so `Recognize`'s 97% contains
materialization rather than excluding it, and the 29% the same profile gave `Materialize`
was the number to read. Subtracting one from the other was arithmetic on nested
quantities.

### What the decomposition says

A profile could not settle it, so an experiment did. Three parsers over the same
`Url.gram`, in one process, each warmed and run for four seconds:

    hand         17 us     63,680 B      GramParser + GramLexer
    bare         54 us      2,024 B      the same grammar, every value stripped out
    generated   137 us     25,976 B      GramGrammar as it stands

`bare` is `GramExample`'s grammar with no declared types, no captures and no factories —
the same language recognized, nothing built. It is a decomposition tool and not a
competitor: the hand-written parser builds a tree, so only the third row is a fair
comparison to it.

    values cost   83 us    69% of the gap
    engine costs  37 us    31% of the gap
    whole gap    120 us    ~8x

Stable across three runs (67–70% / 30–33%). **Deferred construction is 61% of what the
generated parser spends** — 83 of 137 microseconds — and the hand-written parser pays none
of it, because it builds its tree as it recognizes rather than recording an arena and
replaying it.

That is the architecture's central claim (implementation.md §3, "Nothing is built while
matching") priced on a real grammar for the first time. It buys resumability: a step tried
and given back never ran its factory. On this grammar, which backtracks 118 times in 4,453
events, almost nothing is given back — so almost all of it is paid for nothing.

### The miniature already predicted it

`CallCost`'s new valued row, from the entry above: 840 ns valueless against 1,823 ns
valued over the same forty letters, so values are 54% of the valued parse. The self-hosted
grammar says 61%. A four-rule benchmark and a two-hundred-line grammar agree to within
seven points, which is the best evidence either of them is measuring what it says.

### Where this points, and what already exists for it

Not the forwarding chain — that was the previous entry's hypothesis and it is worth 13%.
The target is the value machinery, and the engine already has two answers aimed at exactly
it, both currently too narrow:

  * `Machine.Sites` compiles a *valued* call in place where the callee's value is built
    from spans of the input alone. Every member must be a span (`member.Rule is not null`
    refuses), and the callee must be outside every cycle.
  * the flat rendering (`_valuesInLocals`) keeps captures in locals and runs one factory
    at `Accept`, with no arena at all.

`GramExample`'s rules fail both on the same point: each rule's value is built from the
*next rule's value*, not from text. Widening a site to admit a member that is another
rule's value — the callee's own site, nested — is the shape to investigate, and 69% is
what is behind it.

Not started here. The measurement is the deliverable, and it moved the target twice: from
the forwarding chain to the value machinery, and from a profile reading to an experiment.

## Investigated and ruled out: sites cannot reach the self-hosting gap, nested or not

The previous entry proposed widening `Machine.Sites` to admit a member that is another
rule's value, and put 69% behind it. Checked against the grammar before writing any code,
that direction is closed, and the reason is worth recording so it is not proposed a third
time.

A site refuses a callee on two counts (`ComputeSitedValued`): every member of its value
must be a span of the input, and the callee must be outside every cycle. `GramExample`'s
rules fail **both**, and the same rules fail both:

    rule             recursive  members
    Body             True       [first:Alternative, rest:Alternative[]]
    Alternative      True       [body:Sequence, value:Value]
    Sequence         True       [first:Guard, rest:Guard[]]
    Quantified       True       [body:QuantifiedCore, rebound:text, mark:Marking[]]
    QuantifiedCore   True       [body:Prefixed, quantifier:Quantifier, recovery:Recovery]
    Prefixed         True       [prefix:text, body:Captured]
    Captured         True       [name:text, body:Primary]
    Primary          True       [text:text, e:ElementSet, cs:CsExpr, body:Body]

Nesting sites answers the first count only. The cycle stands: `Primary` leads back to
`Body` through `'(' & Body & ')'`, so the chain is one strongly connected component and a
nested expansion through it does not terminate statically. Twenty-one of the grammar's
fifty-five rules are in it; twenty-three are already compiled in place.

Relaxing the cycle restriction alone is the other half, and it is worth measuring at
7%, not 69%: `Reference`, `Type` and `Value` are recursive but every member of each is
text, so they are what a relaxed cycle rule would admit — and `Reference` is 68 of the
1,020 calls a parse makes.

So the 69% is not reachable by widening this mechanism. What is left for it is one of two
larger things, neither started: making the replay itself cheaper — the materializer walks
a linked list of capture entries per completed call, 1,014 of them — or revisiting eager
construction with a soundness argument the reverted attempt did not have. `=>` may have
side effects, which is why §7.2 defers it and why "build it anyway, it is only wasted"
is not available.

### Found while trying to split the cost: refusal is three times acceptance

The split was attempted with an input that fails at `Accept:` — `whole` is checked before
the materialize block, so recognition runs and nothing is built. It did not work, because
refusing is not free:

    accepted                     generated  135 us   bare   53 us
    refused on '%'                          388.5           177.6
    refused on ')'                          388.4           174.6
    refused on '='                          395.7           180.5
    refused on '&'                          395.0           174.2
    refused on '|'                          396.0           176.6

One trailing character that cannot begin a declaration costs **2.9x a whole successful
parse**, and 3.3x in the value-free grammar — so it is the engine's backtracking and not
construction. Stable across five characters, which says it is the unwinding after `Accept`
fails rather than anything about what was typed.

That is the shape this session already found and fixed once in `ExpressionLanguage`, on a
different grammar and a different cause. Worth its own look: an editor parsing an
incomplete file pays it on every keystroke.

## Measured and dropped: refusal is a constant factor of three, not a growth

The previous entry found that one trailing character costs the self-hosted parser 2.9x a
whole successful parse, and proposed it as the next thing to fix — "an editor parsing an
incomplete file pays it on every keystroke". Measured properly, it is not worth engine
work, and the measurement that says so is the one the proposal should have carried.

### Where it comes from

A trace of the refused parse against the accepted one:

    accepted    1020 call   1017 return   1014 rule capture
    refused     1243 call   4707 return   4441 rule capture

More returns than calls, because after `Accept:` fails on the leftover character the
unwinder lands on a resume point inside the already-read file and walks the return path
again. Where it lands is concentrated: 66 of the resumes are one state, the exit of the
optional in

    Captured = (name: Identifier & ':')? & body: Primary

— one live "or there is no name here" per operand in the file. `Determinism.NeverGivesBack`
is asked about it and answers no, correctly: FIRST of the body is letters and so is
FIRST(`Primary`), so no single character tells the two readings apart. What would tell
them apart is that the body ends in `':'` and nothing that could follow `Captured`
consumes one — a fact about what a construct *ends* with, which needs a LAST set this
compiler does not compute.

### What it is worth, and why that closes it

Committing that one optional by hand — `{ (name: Identifier & ':')? }`, which the notation
already offers — takes the refusal from 205 us to 152, **26%**. So the cost is real and
spread over several optionals rather than concentrated in one.

But the shape settles it. The same file repeated, accepted and refused:

     3,053 chars     131 us     393 us     3.0x     129 ns a character
     6,107           264        781        3.0x     128
     9,161           392      1,229        3.1x     134
    12,215           554      1,666        3.0x     136

**Linear**, flat per character across a fourfold range, and the ratio does not move. The
66 resume points do not compound: each is visited once and the work stays proportional to
the input. This is a bounded constant of three, not the doubling-per-level that
`ExpressionLanguage` had and that was worth a night.

So it is not the next thing. An author who minds it has the braces today, at a measured
26% for one of them, and a LAST-set analysis to shave a linear 3x sits well behind the 69%
that deferred construction costs on the same grammar.

The proposal in the entry above was made on a ratio without a curve beside it. The ratio
was right and the conclusion drawn from it was not.

## Profiled properly: it is the recognizer writing the arena, not the materializer reading
## it — and how three profiling modes disagreed

The value machinery is 69% of the self-hosting gap (two entries above, measured by
stripping every value out of the grammar and timing what was left). This entry finds where
inside it, and the finding is the opposite of what the first profile said.

### Three modes, and only one of them to be believed

A **sampling** profiler cannot see inside this engine at all: a generated parser is a few
very large methods with `goto` between their states, so there are no frames to attribute
to and the answer comes back as "Recognize 97%". That is what sent the last two entries
looking for other instruments.

**LineByLine** sees inside, and lies about what it sees. It instruments every line, which
turns off inlining, so the run took 14.3 ms a parse against 0.135 unprofiled — **106x** —
and the overhead lands on whatever executes the most lines. It reported
`Materialize_DotGram` at 42% and `ParserEntry.get_Kind` at 7% over 7.9 million calls; the
second is a field read that does not exist in a release build, and the first is the
biggest method in the file paying a probe per line executed.

**Sampling with `Reporter.exe`** is the one to believe, and the check is that it barely
distorts: 250,000 parses in 35,642 ms is 142 us each, against 135–137 unprofiled. What it
cannot do is look inside a method; what it can do is say which method, without lying about
the proportions.

    Reporter.exe report samp.dtp --pattern=all.xml --save-to=report.xml

with `<Patterns><Pattern>.*</Pattern></Patterns>`. `Reporter.exe` sits in the dotTrace
installation directory, not in the `dottrace` CLI, and an earlier attempt here concluded
the snapshot was unreadable without the GUI after finding only storage-level types in
`JetBrains.Profiler.Snapshot.dll`.

### What it says

    Recognize_DotGram_Part0    11,500   32.3%   own
    Recognize_DotGram_Part1     8,446   23.7%
    Materialize_DotGram         3,080    8.6%   (13.1% with its subtree)
    ParserArena.Add             1,652    4.6%
    Scan_trivia                 1,141    3.2%
    Recognize_DotGram own       1,128    3.2%
    ParserEntry..ctor             943    2.6%
    StelemRef_Helper+StelemRef  1,284    3.6%
    ClearWithReferences+Reset     765    2.1%

**Materialization is 13%, not 42%.** Recognition is about 70%, and the two parts of the
state machine are 56% of the parse between them.

That is consistent with the stripped-grammar experiment rather than against it. Values
cost 83 of the 137 microseconds; materialization is only about 18 of those, so the other
65 are the arena entries the recognizer writes *because* there are values — the
`Completed` rewrite and the `RuleCapture` per valued call. **The expensive half of
deferred construction is recording the derivation, not replaying it.**

### Four hypotheses measured and dropped on the way

Each was plausible and each is now closed by a number rather than by an argument.

  * **The struct copy.** `ParserEntry` is nine ints and the arena's indexer hands it back
    by value, so every read copies 36 bytes to look at one or two fields — the engine's own
    comment in `ArenaCost` calls this "the interesting one". `ArenaCost` now runs at two
    scales, the URL's 172 reads and the self-hosted grammar's 9,972 over a hundred-kilobyte
    array, and reading in place is within 2% at both. A copy inside the cache is not a cost.
  * **The pointer chase.** The materializer follows `capturedAt = linkNexts[capturedAt]`,
    a dependent load per hop that nothing can prefetch. Two new rows read the same entries
    swept and chained: the chain is not the slower one at either scale.
  * **The tables cleared between parses.** `Reset` does six `Array.Clear`s and a scalar
    loop writing −1 into two link tables. Timed at the real arena size it is 3 us of 125,
    and the profile agrees at 2.1%.
  * **The factories.** Every `Construct_*` and record constructor together is under 2%.
    What a `=>` builds is not what deferred construction costs.

### And one that is new

`StelemRef` and its helper are **3.6%** — the covariant store check the CLR runs on every
write into a reference array. The materializer writes `values[...] = parser` into an
`object?[]` used as a "already built" marker, and each of the per-type tables takes stores
of a type the JIT cannot prove exact. Small, but it is pure ceremony and nobody had
counted it.

### Where this points

At the recognizer, which is where the time is and not where the last three entries were
looking. Two counts from the line-by-line run are still exact whatever its timings were
worth: **3,152 arena appends** and **1,367 entries into the two parts** per parse of a
3,053-character file. The parts are this session's answer to the JIT's block limit, and
every crossing between them is a return to the driver and a switch over 321 states — a
cost that grammar pays for a rendering decision made about a different grammar.

## Measured: the method split is calibrated twice too tight, and one grammar pays 47% for it

The sampled profile put 56% of a self-hosted parse inside the two halves of the split
recognizer, and 1,367 crossings between them per parse. That is a cost this session
introduced, so the first question is whether the split is paying for itself. On
`GramGrammar` it is not.

### The two sides, timed identically

`line.exe`, 40,000 parses of `Url.gram` after 2,000 warm-up, in-process timer, three runs:

    split, two parts       125.9   126.5   128.4 us
    one method             85.0    86.1    86.6 us

**The split costs 47%**, and the estimator refuses the fast one: `GRAM5003` says the
undivided recognizer is 2,113 basic blocks, past the 2,000 where "the JIT compiles a
method without optimization and this one will run several times slower".

### What the JIT actually did

`DOTNET_JitDisasmSummary=1` with `DOTNET_JitStdOutFile` answers it directly, and both
sides are fully optimized:

    one method   Tier1-OSR with Synthesized PGO, IL size=33326, code size=90196
    two parts    Tier1 with Synthesized PGO, IL 30358 + 24463, code 46147 + 36335

No MinOpts anywhere. The premise the split was made under does not hold for this grammar,
and the total code is the same either way — 90,196 bytes against 86,846. The split buys
nothing and adds 1,367 returns to the driver and switches over 321 states per parse.
`Tier1-OSR` is the reason: a method whose body is one long dispatch loop is exactly what
on-stack replacement is for, and it gets promoted mid-run however large it is.

### And the other side, where the split earns everything

`ExpressionLanguage` at a budget of 4,000 — two parts instead of seven:

    (int x) => x                          11.5 us  ->   30.4
    (int x) => ((((x + 1) * 2) - 3) / 4)   67.9    ->  343.4
    the same six deep                     120.9    ->  522.6
    the same six deep, refused             90.5    ->  311.9

Three to five times slower, and the JIT says why:

    Part0  Tier-0 switched MinOpts, IL size=71017
    Part1  Tier-0 switched MinOpts, IL size=70186

**So the gate is IL size against `JITMinOptsCodeSize`, which is 60,000 bytes** — 71,017
is over it and 33,326 is not. Not a basic-block count, and not 2,000 of anything.

### The calibration

Two points, from the diagnostic and the JIT:

    GramGrammar     2,113 estimated blocks   ->   33,326 IL   ~15.8 bytes a block
    ExpressionLanguage's largest machine
                    6,708 estimated blocks   ->  ~141,000 IL  ~21 bytes a block

At the worse ratio, 60,000 IL is about **2,850 estimated blocks**. `Budget` is 1,500 and
divides to nine tenths of it, so a part comes out around 1,350 blocks — some 28,000 IL,
**less than half the gate**. The margin was set as "a quarter under" a `Limit` of 2,000
that was itself fitted to two grammars; measured against what the runtime actually
switches on, the whole scale is about twice too tight.

Five parsers in this repository are divided today: `Rfc3986` into nine, `ExpressionLanguage`
into seven, `Settlements` into four, `UrlGrammar` and `GramGrammar` into two. The two-part
ones are the ones to suspect — a machine only just over the budget is divided into halves
that were never in danger.

### Not changed here

Raising `Budget` changes the generated code of every consumer, and the number wants
choosing against the IL gate rather than nudging: an estimator that predicts IL bytes and
aims under 60,000 with a margin is a different heuristic from one that counts branches and
aims under a fitted 2,000. Recorded for that decision, with the tree restored to `Budget =
1500` and 1511/1511 green.

## Measured: finer is better all the way down, and the budget is leaving 2-3x on the table

The entry above found the split costing `GramGrammar` 47% and proposed separating two
decisions — whether to divide at all, and how large a part should be — with the second
left where it stood, because two data points bracketed an optimum near it. **That model
was wrong.** A sweep says the curve does not turn: it falls until the parts are tiny and
then flattens.

Both grammars, all timings in microseconds, `Budget` swept while everything else stands:

    Budget    ET parts   nest 4   nest 6   refused    GramGrammar
      1500        7        72.1    116.4     88.1        120.0
       700       14        40.4     68.6     37.5         51.7
       350       25        36.4     58.1     28.5         51.9
       200       44        37.0     69.7     27.2         48.9
       100       86        23.6     41.1     18.5         49.1
        50      167        24.7     39.9     19.6         49.1

`ExpressionLanguage` is **three times faster** at a budget of 100 than at the shipping
1500, and its refusal path three and a half. `GramGrammar` is **2.4 times faster**, and it
flattens by 700. Nothing turns back up as far down as 50, where the largest grammar is in
167 methods.

`GramGrammar` is the one that kills the previous entry's model outright: undivided it is
86 us, in two parts 126, and in four parts 51.7. Not a U with an optimum in the middle —
two parts is simply a bad place to be, and more is better than either.

### Why the old model was wrong

The reasoning was that a part's code quality falls off with size and crossings cost, so
there is an optimum in between. The first half is right and the second is much weaker
than assumed: at a budget of 50 the parsers cross constantly — `ExpressionLanguage`
already crosses 520 times per parse of a fifty-character input at a budget of 1500, ten
crossings per input character — and going finer still does not hurt. Whatever a crossing
costs, it is dominated by what the JIT does with a method small enough to hold in
registers.

### The concept this came from, and the one it points to

The engine is one large method because rules became states rather than methods, and that
was the answer to recursion: a method cannot be suspended and resumed, and resuming across
a rule boundary is what backtracking is. That reason is about the **arena**, which holds
the frames — not about how the emitted code is laid out. The one-method shape was never
required by it; it was what fell out of it.

So the shape can change, and the sweep says it should: **extract into methods everything
that extracts naturally**, which is to say per rule, rather than cutting the state list at
whatever index a budget lands on. Rules are the grammar's own boundaries, diagnostics are
already phrased in terms of them, a cut there is stable under an edit that adds a rule,
and each method comes out small — which is the thing the sweep says matters.

### What a fine split breaks today, and it is not semantics

At a budget of 100 the suite fails four: three snapshots, which is the generated code
changing as it should, and `ReferenceDifferentialTests` — not on a disagreement about what
the automaton accepts, but on `CS0164: This label has not been referenced`. A state whose
label is emitted in one part and only ever jumped to from the driver leaves an unreferenced
label behind, and the differential test holds the emitted source to compiling cleanly. The
same family as the `CS0159` this session already fixed at a part boundary. It has to be
solved for any finer division, and it is a codegen tidiness problem rather than a
correctness one.

Nothing changed here: the tree is back at `Budget = 1500`, 1511/1511 green. What is
recorded is that the constant is not the thing to tune — the shape is.

## Corrected: finer is better only for a grammar large enough to need dividing at all

The entry above swept the part budget over `ExpressionLanguage` and `GramGrammar`, found
the curve falling all the way down, and concluded that the constant is not the thing to
tune. The sweep had no small grammar in it, and with one the conclusion does not hold.

The URL grammar of `benchmarks/Urls.cs` is not divided at the shipping budget — one method,
and the flagship numbers of this project are made of it. Divided, it gets worse at every
input:

    input                    1500, one method   350, two parts   150, five parts
    http://example.com             154.6 ns        187.2  +21%      202.1  +31%
    every part named               315.2           366.8  +16%      420.9  +34%
    an IP host                     168.0           196.5  +17%      237.8  +42%
    eight path segments            251.6           368.7  +47%      333.7  +33%
    the refusal                    102.3           138.2  +35%      160.3  +57%

So a budget low enough to make `ExpressionLanguage` three times faster would cost the URL
grammar between a sixth and half of everything it has, on the comparison against
`RegexOptions.Compiled` that the README leads with.

### What the three grammars say together

    URL             ~600 estimated blocks    one method is best; every division costs
    GramGrammar     2,113                    86 us undivided, 126 in two, 51.7 in four
    ExpressionLanguage 6,708                 141 KB of IL undivided, MinOpts, and hopeless

Dividing is not free and not always worth it. It buys better code inside each method and
costs a crossing; which wins depends on how large the whole would have been. The
**two decisions** the earlier entry proposed and then withdrew are back, and now with a
third point to place them:

  * **Whether to divide at all** is a real threshold, somewhere between the URL grammar's
    six hundred blocks and `GramGrammar`'s two thousand. Below it a single method wins by
    16 to 57%; above it dividing wins by up to a factor of three.
  * **How large a part should be, once dividing**, is the flat basin the last sweep
    found — anywhere from about 350 blocks down to 50 measures the same, and the shipping
    1500 is far above it.

### And why today's shape is the worst of both

`parts = ceil(whole / (Budget * 9/10))` makes those one decision. A grammar just over the
budget is divided into **two** parts — which every measurement here says is the worst
place to be: `GramGrammar` at two parts is slower than at one *and* slower than at four.
Being just over the line does not cost a little, it costs the most.

Divide finely once the decision to divide is made, and that disappears: a grammar just
over the threshold gets many small parts, which is the good configuration, and the
threshold becomes a crossing between two costs rather than a cliff.

### Not built

Placing the first threshold wants a fourth and fifth point between six hundred and two
thousand blocks, which no grammar in this repository provides — it wants a synthetic
grammar swept by size. Recorded with the tree at `Budget = 1500` and 1511/1511 green.

## Built: whether to divide and how large a part is are two numbers now, and it is worth
## two to four times

The synthetic sweep the last entry asked for. A grammar of a fixed hot core — an
expression ladder — and a ballast of cold rules grown from nothing to four hundred, timing
an input that touches only the core, so what moves is the size of the machine around a hot
path that does not change:

    ballast      one method   parts of 150   parts
        0           379 ns        519 ns        2
       20           545           593           9
       50         2,199           586          18
      100         2,474           591          32
      200         2,708           555          61
      400         3,423           575         118

**Divided into small parts the hot path is flat — 519 to 593 ns whatever the grammar
is.** Undivided it holds while the machine is small and then falls off a cliff between
twenty and fifty ballast rules, ending four times worse.

Two things follow. The crossing is between about 1,150 and 2,350 estimated blocks, so the
`Budget` of 1,500 was **standing in the right place all along**. And the asymmetry is
sharp: dividing a machine that did not need it costs about a quarter, leaving one
undivided that needed it costs four times over.

So what was wrong was never the threshold. It was that `parts = ceil(whole / Budget)` made
one number answer two questions, and a machine only just over the line came out in **two
parts** — the worst arrangement there is, slower than one and slower than many.

### The change

One line, and a constant beside it. `Budget` still says whether to divide; a new `Part`,
150, says how large a piece should be. Everything else stands.

    URL grammar (~600 blocks)      undivided before and after, same code
    Url.gram snapshot              2 parts -> 13
    GramGrammar                    120.0 us -> 48.1      2.5x
    ExpressionLanguage             7 parts -> 42
      (int x) => x                  11.5 us ->  3.9      2.9x
      x * x - 1                     15.1    ->  4.6      3.3x
      four parentheses deep         72.1    -> 26.5      2.7x
      six deep                     116.4    -> 49.0      2.4x
      six deep, refused             88.1    -> 23.3      3.8x
    Rfc3986                        9 parts -> 81

The URL grammar is the check that matters as much as the speedups: it is under the
threshold, so it is still one method and its generated code is unchanged, which is what
keeps the comparison against `RegexOptions.Compiled` where it was. A budget low enough to
divide it costs it 16 to 57%, and that is why the threshold is kept rather than lowered.

One snapshot moved, `Url.gram`, from two parts to thirteen — the shape this fixes, caught
by the file that exists to catch it.

### What is still owed

The number should be a setting rather than a constant, and a wish rather than a
requirement: a consumer tuning for their own grammar should be able to ask for anything,
including nothing and a million, and get a working parser either way — the generator
dividing as near to what was asked as it can and never failing because it could not.
That is the next piece.

## Built: `PartSize`, an attribute option that is a wish rather than a setting

The measurements say the part size sits in a wide flat basin — sixty to two hundred and
fifty estimated blocks all measure alike, on `ExpressionLanguage`, on the self-hosted
grammar and on a synthetic one:

    Part   ET nest 6   ET refused   GramGrammar
      60      48.3 us      20.4 us      48.0 us
     100      44.9        19.8         47.2
     150      44.5        19.2         48.6
     250      46.4        19.8         50.9

Flat, and the same shape for grammars that differ in kind — so there is nothing to compute
from a grammar and the default is a constant. Wide is not universal, though, and the three
grammars measured are not a consumer's, so the number is theirs to change.

**On the attribute, not in the build.** It was written as an MSBuild property first, and
that was the wrong channel: a build property is per project and this is per grammar, and
it would have added a packaged `build/` folder to a package that has none. `[Gram("…",
PartSize = 80)]` puts it where the grammar is.

**A wish, and that is the load-bearing part.** Nothing an author writes there may fail a
build, because a knob that can break a compilation is one nobody can safely turn. Below
one asks for the finest division there is, anything past the size of the recognizer asks
for one part, zero is what the attribute holds when nobody set it and means the default,
and everything between is taken at its word. Measured on a grammar of three hundred
ballast alternatives:

    PartSize        parts   hot path
    (unset)            61     554 ns
    40                220     570
    0                  61     561
    1,000,000           4  20,791
    -5              1,452   1,996

The last two are what "as near as it can" looks like: both are answered, both parse, and
both are slow in the direction asked for.

Seven tests hold it — every value in that table generating a parser that parses, compiled
by the C# compiler rather than merely emitted, because an unreferenced label or a jump
with no target is exactly the failure a fine division produces and only a compiler finds
it. The test grammar had to be rebuilt twice on the way: a choice of four hundred literals
is settled enough to be lowered to one flat method, which has no parts at all and would
have measured nothing whatever the size said. Recursion with values is what asks for the
engine.

§6.4 documents it, and says the other half out loud: whether to divide at all is not
tunable and is a different question, decided from the estimate, because dividing a grammar
that did not need it costs a quarter and failing to divide one that did costs four times.

## Measured: the default is right for both parsers, and the first sweep that said otherwise
## was measuring the machine

The option exists so a grammar that wants something else can say so. Asked of the two
parsers this repository has numbers for, neither does.

### The URL grammar has nothing to tune

It sits under the divide-or-not threshold, so it is one method and `PartSize` is inert —
setting it to forty still emits nought parts. The question worth re-asking was whether
that is right, because the measurement behind it was taken when `Budget` was both the
threshold and the part size and so could not tell the two apart. Forced to divide, with
the two now separate:

    parts        short    full     IP host   long path   refused
    one (as is)  130.2    302.4    149.3     216.3        86.8 ns
    two          164.3    355.4    185.5     280.1       116.6
    five         179.1    401.6    201.0     290.7       131.6
    eight        296.3    455.8    388.8     426.7       164.6
    sixteen      227.4    490.6    312.9     873.8       166.2

Worse at every size and worse the finer it is cut. Its optimum is the arrangement it
already has, and the threshold that gives it that is doing its job.

### `ExpressionLanguage` has nothing to tune either

Swept through its own attribute, sixty to three hundred, the numbers are flat. A first
reading put a shallow optimum at ninety — about 3% better than the default over the five
inputs — and interleaving 90, 150, 90, 150 dissolved it: the first pair goes to ninety by
0.4% and the second to a hundred and fifty by 3%. There is no difference to find.

### What made the first sweep lie

Worth writing down, because it nearly produced a tuned constant out of noise. The harness
timed one window and reported its mean, and on this machine the spread between runs of
**one** build was larger than the spread between builds — the same default measured 44.5,
49.0 and 51.4 microseconds on the same input across the evening. A mean over that cannot
separate two configurations.

The fix is two lines: take the **best of several short windows** rather than the mean of
one long one — no run is ever faster than the work takes, so the minimum is the one least
interfered with — and **interleave** the configurations rather than running all of one and
then all of the other. Repeatability went from ±15% to ±0.8% on the shortest input and
±2.4% on the longest, which is what made the answer legible: there is nothing there.

So the option stays for a grammar that turns out to want it, and the two grammars measured
here are not it.

## Measured: the full suite after the division change, and what it can and cannot say

### `ExpressionBenchmarks`, which is where the change lands

    input                            before      after     times   allocation
    six deep                       80,280 ns   40,630 ns    2.0x   unchanged
    six deep, refused              76,982      16,700       4.6x   unchanged
    four deep, refused             54,515      11,520       4.7x   unchanged
    four deep                      52,231      23,101       2.3x   unchanged
    two deep, refused              35,383       6,956       5.1x   unchanged
    two parameters, parenthesized  28,522       9,264       3.1x   unchanged
    two deep                       28,197      10,348       2.7x   unchanged
    a block, through Assignment    24,976       8,478       2.9x   unchanged
    Math.Max, through NamedType    24,175       9,270       2.6x   unchanged
    the operator ladder            13,941       4,244       3.3x   unchanged
    a member read                  10,895       3,119       3.5x   unchanged
    the floor                       7,970       2,297       3.5x   unchanged

**Two to five times, and the allocation column is the control.** Byte for byte the same
at every input — 1.16, 1.59, 1.8, 2.59, 2.82 KB and the rest — so the two builds do the
same work and record the same derivation. What changed is only how the emitted C# was
compiled, which is what a division is.

Refusal is now cheaper than acceptance at every depth (6.96 against 10.35, 11.52 against
23.10, 16.70 against 40.63), which is the right way round: a refused input is one
character shorter and reads one operand less.

### What the full run cannot say

Everything in it moved down between eight and thirty per cent, including `Regex`,
`RegexOptions.Compiled` and the BCL's own `File.ReadLines` — none of which this repository
touched. That is the machine, and the third time this session it has moved by more than
the effect being looked for.

So the URL numbers are read as ratios, and they hold: `.Gram` against
`RegexOptions.Compiled` is 2.66 / 1.78 / 1.22 / 2.72 / 2.49 against the previous run's
2.49 / 1.81 / 1.26 / 2.92 / 2.32, scattered both ways within a few per cent. Which is
what should happen — the URL grammar is under the divide threshold, so its generated code
is unchanged, and an unchanged parser measuring the same is the check that the threshold
did its job.

And one honest gap: `Settlements`, the only benchmarked grammar that did change (four
parts to twenty-two), shows its `WideFeedBenchmarks` rows down 8.5 to 18.3% at the small
sizes — inside the same band the untouched `Regex` rows moved by. **That improvement
cannot be claimed from this run.** Separating it wants the two builds interleaved on one
machine state, the way `ExpressionBenchmarks` was measured, and that was not done.

## A/B, interleaved: the division is a large win on one grammar and nothing on another,
## and the synthetic experiment could not have told me

The previous entry declined to claim `Settlements`' 8.5 to 18.3% because the untouched
`Regex` rows moved by as much. Measured properly it was right to decline.

### How

The `PartSize` option makes the A/B possible in one process: `PartSize = 1500` reproduces
exactly what the generator did when one number answered both questions — `parts =
ceil(whole / (Budget * 9/10))` — so two copies of the benchmarks' own grammar, generated
from the one source and differing in that number alone, are the two builds. Four parts
against twenty-two, alternating which goes first each round, best of several rounds.

    20,000 records     1.05x   0.99x   0.99x   1.00x
    100,000 records    0.94x   0.98x   0.96x   0.96x

**Nothing at the small size and about 4% *worse* at the large one**, reproducibly, at a
spread of ±1%. Against `ExpressionBenchmarks`' two to five times on the same change.

### Why

A line-by-line profile counts the crossings, and they are the whole story:

    four parts    2 of them ever entered      3,015 entries
    twenty-two   10 of them ever entered     13,525 entries

Four and a half times the crossings, for no gain in code quality — the coarse parts were
already small enough to be compiled well. `Settlements` is one enormous `Row` rule, a
straight line of fifty fields that every record walks end to end, so its hot path *is* the
machine: dividing it finely can only add boundaries to something that has to cross them
all anyway. `ExpressionLanguage` is the other shape — a small operator ladder inside a
large machine most of which a given parse never touches.

### What that says about the experiment that set the number

The synthetic grammar built to place the threshold was **a fixed hot core with cold
ballast grown around it**, which is exactly the shape where dividing helps most. It could
not have shown this, and I generalized from it. The honest statement of the change is
narrower than the last entry's: two to five times where a grammar's hot path is a small
part of a large machine, inert where the machine is not divided at all, and a few per cent
worse where the hot path is the whole machine.

`Settlements` is therefore the first grammar here that would set `PartSize` for itself —
which is what the option is for, and the first evidence that it was worth having rather
than a knob added because it could be.

## Probed: a SQL-sized grammar, and the emitter is quadratic

Before writing any SQL. `TSql170.g` is 35,800 lines of ANTLR grammar against this
repository's largest at 683, so the first question is whether the generator survives that
at all. A synthetic grammar shaped like a dialect — one shared expression ladder and a
growing number of statement forms, each with keywords, an option list and captures that
build — answers it.

    forms   grammar   generated    parts   generate   compile C#
       10        99      19,039       72       0.4s        3.2s
       40       309      65,367      268       1.9s        4.3s
      100       729     157,935      650       4.2s       12.6s
      200     1,429     313,667    1,298      11.5s       17.0s
      400     2,829     625,171    2,594      56.7s       52.3s

Every one of them generates, compiles and parses. What they do not do is scale: the
generator's exponent climbs 0.9, 1.6, **2.3** as the grammar grows.

### Where, exactly

Stage by stage, and it is not where I would have guessed:

    forms   rules    lex   parse   bind   normalize      emit
       40      97     22       4      7         367     1,099 ms
      100     217      6       1      1         239     2,448
      200     417      2       4      2         228    10,357
      400     817      1       0      1         327    47,208

**Normalization is flat** — 230 to 370 ms whatever the size, so its fixed points are not
the problem. All of it is the emitter: 3.8 times the rules for 19.3 times the time.

A sampling profile names the methods:

    Machine.Dispatching        16.6%
    Machine.Named              11.8%
    String.SplitInternal       11.2%
    regex MatchCollection       5.3%

`RenderStates` is called once per part, and it calls `Named()` — which runs a regex over
every body — and `Dispatching()` — which walks every state and every jump. Neither
depends on which part is being written; both are pure functions of the finished bodies.
With parts growing in proportion to states, computing them per part is O(states²), and
that is the whole of it. **Cacheable once per machine**, which is the fix.

### What it says about SQL

Two blockers, and only one is ours.

Ours is the quadratic above, and it looks cheap to remove.

The other is not: 625,171 lines of C# took Roslyn 52 seconds, growing at about n^1.6, and
this synthetic grammar expands at **221 lines of C# per line of grammar**. Full T-SQL at
that ratio is millions of lines and minutes of the consumer's build, every build.

But the ratio is a property of how a grammar is written, not a constant:
`ExpressionLanguage` expands at 37, `Rfc3986` at 262, and this probe — many small rules of
literal alternatives — at 221. A SQL grammar written as fewer, richer rules would land
somewhere else entirely, and **which** is the next thing worth measuring, on a real
`SELECT` subset rather than on a synthetic.

So: not a no, and not a yes yet. The order is the emitter's quadratic first, because it is
ours and it is bounded; then a `SELECT` subset to find the real expansion ratio; then the
comparison against ScriptDom.

### And one correction to the framing

ScriptDom is built on **ANTLR 2**, not ANTLR 4 — `antlr.Tool`, a vendored
`antlr/antlr/*.cs` runtime, `options { k = 2; }`, LL(2) with hand-written syntactic
predicates, and a separate lexer. Beating it would be worth saying, because it is the
production T-SQL parser with 27.8 million downloads; "faster than ANTLR" would not be,
because that is not the ANTLR anyone means today.

## Built: the emitter's answers are worked out once, and it stops being quadratic

The probe above put the whole of the generator's superlinearity in `CSharpEmitter`, and a
sampling profile named `Machine.Dispatching` at 16.6% and `Machine.Named` at 11.8%, with
`String.SplitInternal` and a regex behind them. The reason is one line of structure:
`RenderStates` is called once per part, and each call worked those out again.

Neither depends on which part is being written. `Dispatching` is a map for the whole
machine; `Dispatched` is a list of the states the dispatch can land on; and the label set
`RenderStates` builds is four sets united, none of which is a fact about a part — the
jumps the finished bodies hold, the states the dispatch lands on, the ones named from
outside, and the ones a part is entered at. Each is now worked out once and held, and
cleared in `PlanLayout`, which is the one moment the bodies or the parts change.

    forms   rules   emit before   emit after
       40      97      1,099 ms       342 ms
      100     217      2,448          609
      200     417     10,357          963
      400     817     47,208        2,164

**Twenty-two times at the largest point**, and the shape changed rather than the constant:
3.8 times the rules now costs 3.55 times the time, an exponent of 0.96 against the 2.2 it
was. End to end the generator goes from 56.7 seconds to 2.5 on the largest grammar
measured.

The check that matters is that the emitted code did not move: caching a pure function
cannot change its answer, so every snapshot must be byte identical, and they are —
including `Url.gram`, which is seven thousand lines in thirteen parts. 1518/1518 green.

### What is now in the way of a SQL-sized grammar

Not us. At 625,171 lines of generated C# the generator takes 2.5 seconds and **Roslyn
takes 41.6**. The wall moved to the C# compiler, and the only lever on it is emitting
less: this synthetic grammar expands at 221 lines of C# per line of grammar, where
`ExpressionLanguage` expands at 37. Which of those a real SQL grammar is like decides
everything, and it is measured by writing one rather than by arguing about it.

## Built: the expression layer of standard SQL, recognizing

The bottom level first, which is the right order and not only because the standard is
layered that way: a `SELECT` is mostly places where an expression stands, so writing the
query level first means writing expressions anyway, badly scoped.

**The rule names are the standard's, production for production** — `SearchCondition`,
`BooleanTerm`, `BooleanFactor`, `BooleanTest`, `BooleanPrimary`, `Predicate`,
`RowValueConstructor`, `ValueExpression`, `Term`, `Factor`, `ValueExpressionPrimary`. Not
an implementation's object model: what I first proposed used `BooleanExpression`, which is
Microsoft's DOM name and not a name in ISO/IEC 9075 at all. The standard calls it
`<search condition>`, and in SQL-92 there is no `<boolean value expression>` to confuse it
with.

Two things the reading corrected beyond the naming. `<value expression>` in SQL-92 is the
four value categories and nothing else — `<search condition>` stands beside it as its own
nonterminal rather than being a branch of it, which is SQL:2003's arrangement and what I
had described. And a predicate compares `<row value constructor>`, not a value expression,
which is why `a = 1` and `(a, b) = (1, 2)` are one production rather than two.

### The one divergence, written where it happens

§6.11 gives four towers — numeric, string, datetime, interval — that share a bottom, so
`a + b` belongs to two at once and only the types of `a` and `b` say which. That is not a
defect in the standard: it describes syntax modulo type resolution, and a parser has no
types. One untyped ladder here, as in every implementation, and the grammar says so at the
place it does it.

### What it recognizes

Twenty-eight of thirty-one sample conditions, and the three refused are the three written
to be refused. Comparisons, `AND`/`OR`/`NOT`, `IS TRUE`, `BETWEEN`, `IN` both ways, `LIKE
… ESCAPE`, `IS NULL`, `EXISTS`, quantified comparison, row constructors, `CASE` both
forms, `CAST`, `EXTRACT`, `SUBSTRING`, `COALESCE`, set functions with `DISTINCT`, typed
literals, intervals, concatenation, delimited identifiers, parameters.

Nothing is built. The tree comes when its shape is decided; getting the language right
first is what keeps that decision about the tree rather than about the parse.

### Two defects it found on the way

**Mine, and the notation caught it:** `Identifier = RegularIdentifier & ?!Reserved` asks
whether a reserved word *follows* an identifier, where §5.2 says an identifier is a word
that *is not* one. In front, it reads; behind, it refuses `x IN (1, 2)` and `LIKE 'A%'
ESCAPE ''` — two failures from one transposition.

**The generator's, and nothing caught it:** a capture whose name is already capitalised
makes the emitted class use one name for both the property and the constructor parameter,
so the constructor comes out as `EXISTS = EXISTS;` — CS1717, uncompilable, with no
diagnostic from us. The same family as the CS0164 fixed earlier this session: emitted code
that does not compile and is not refused. Recorded here; not fixed yet.

### And the measurement it was written for

    grammar   generated    ratio
      334      119,722      358x    SqlExpressions
      683       25,349       37x    ExpressionLanguage
      126       33,024      262x    Rfc3986

**358 to one**, worse than the synthetic probe's 221 and ten times `ExpressionLanguage`.
So the answer to the question the probe left open is the unwelcome one: a real SQL grammar
expands like `Rfc3986` and not like `ExpressionLanguage`, and the C# compiler is the wall
that matters.

Where it goes is not yet measured, but the shape of the grammar says where to look: sixty
reserved words tried in order behind every identifier, and case-insensitive keyword
literals that expand per character. Both are size and both are speed.

## Built: two published rules that reach each other share one machine

`SqlStandard92` publishes both of its roots — a caller has either a condition or an
expression in hand — and that wrote the grammar **twice**: 119,722 lines against 60,317,
the second entry point costing a complete second copy.

The emitter grouped publications by rule identity. `parse R` and `find R` shared a machine
and always had; two *different* rules got one each, "even where both call a third rule,
which is then compiled into both", as its own comment said. `<search condition>` reaches
`<value expression>` through its predicates and `<value expression>` reaches back through
`CASE`, so each machine compiled everything.

They share one now, on the condition that each can reach the other. Mutual reachability is
the right test and one-way is not: a machine is compiled over what its root reaches, and
only where the reaching goes both ways are the two sets equal — so the two roots are two
entry states of one machine, which is a shape `Register` already had. `CallGraph.Together`
is the predicate, and it was already there, meaning the same components Tarjan finds for
recursion.

    two publications, two machines   119,722 lines   308 parts
    two publications, one machine     60,317         154

Nothing in the repository changed — no grammar here had two mutually reachable
publications, which is why the snapshots did not move and why two tests were added rather
than relying on them.

### And the headline of the entry above was wrong twice

It said a real SQL grammar expands at 358 lines of C# per line of grammar against
`ExpressionLanguage`'s 37. Half of that was this duplication. The other half was the
measure: **grammar lines say more about how an author wrapped them than about how much
grammar there is** — `SqlStandard92` writes 60 reserved words as one rule over ten lines,
where `ExpressionLanguage` spreads comparable content over many more. Counted in nodes of
the lowered tree, which is what the emitter is actually given:

    grammar              rules   nodes   C# lines   per node
    SqlStandard92           70   1,558     60,317       38.7
    ExpressionLanguage      86   1,713     25,349       14.8
    Rfc3986                 36     322     33,024      102.6

The two grammars are nearly the same size — 1,558 nodes against 1,713 — and SQL emits 2.4
times as much, not ten times. `Rfc3986` is worse than either, which says the shape that
costs is character-level recognition rather than SQL.

Of the remainder, one part is measured: making the 192 case-insensitive keyword literals
case-sensitive takes 60,317 lines to 51,976 — **case insensitivity is 14%**, which is a
real cost and not the difference.

## Built: a rule is written where it is called only while it is small

`ExecutionPlan.CompiledInPlace` had no size in it. Any rule that keeps no value, holds no
capture and sits outside every cycle was written at each of its call sites, and its own
comment said why that was safe — "what the duplication costs is generated text" — as
though text were free. For the helpers it was aimed at, four to six nodes each, it is.

Standard SQL's reserved-word list is 285 nodes, and the expansion is compositional:
`Reserved` is written into `Identifier`, `Identifier` into `QualifiedName` and three other
places, `QualifiedName` into four more, and `QualifiedName` holds `Identifier` twice. About
a dozen copies of a sixty-way choice.

    60 keywords          60,317 lines
     4 keywords          24,503
     no reserved check   22,228

**Fifty-nine per cent of the file.** So a boundary is kept where the body is large, at
sixty-four nodes — measured rather than chosen, because there is no continuum to cut in
the middle of. Across the three parsers here the rules this admits have a median of four
to six nodes, and what stands above the line stands well above it: 92 for `Rfc3986`'s IPv6
address, 228 and 285 for SQL's data types and reserved words. `ExpressionLanguage`'s
keyword list is 42 and stays inlined.

    SqlStandard92        60,317 -> 26,473    -56%    154 parts -> 62
    Rfc3986              33,024 -> 25,571    -23%
    ExpressionLanguage   25,349 -> 25,349      0%

One snapshot moved, `Url.gram`, by five lines and a full renumbering of states — which is
what a rule leaving the inline set does to everything written after it. Behaviour is the
other 1,519 tests, and the SQL grammar still accepts the twenty-eight of thirty-one it did.

### What a keyword actually costs, since the last entry said it badly

"640 lines per keyword" divided a total by a count and hid two different multipliers. What
is emitted for `"SET"` is twenty lines: one comparison per character, and beside each a
five-line block recording *which* character did not match.

    if (text.Length - p < 3 || ToUpperInvariant(text[p]) != 'S')
    {
        if (text.Length - p < 3) failure.OutOfInput = p + 1;
        expected = Recognize_DotGram_Expected144;
        { state = 2; goto Leave; }
    }
    …

So it is about six and a half lines per character of keyword, per copy, and there were a
dozen copies. Three of the twenty lines recognize; the rest report. Counted over the whole
file, **31% of it is failure reporting** — which is not a defect but the price
`implementation.md` §0 names out loud, and this is the first time it has been counted.

### And the answer to whether predicted dispatch helps here

Not as it stands. `Predictive` requires `Determinism.Distinguishable` — every pair of
alternatives with disjoint first sets — and a keyword list never has that: `AND`, `ALL`,
`ANY` and `AS` all begin with `A`. What is emitted instead is a chain of first-character
tests, one per alternative, each falling to the next. A jump table on the first character,
or a trie over the whole set, is the thing that shape wants, and neither exists here.
Recorded rather than built: the size problem above was worth more and cost less.

## Built: a case-insensitive literal is one comparison too

The entry above counted 31% of the generated file as failure reporting and left it there
as the price §0 names. Igor's reading of it was sharper: most of the time nobody needs to
know *which character* did not match, and it was being worked out after every comparison —
where the whole of it belongs on the branch that has already failed.

Which is exactly what a case-*sensitive* literal already did. One `SequenceEqual`, and
`Sharpen` inside the failing branch to say where. The case-insensitive one did not, on a
stated ground:

> Case-insensitive stays as it was. What it compares is each character folded, which is
> not the comparison any span method makes.

That is wrong. `MemoryExtensions.Equals` with `OrdinalIgnoreCase` is exactly it, and it is
on the netstandard2.0 floor through System.Memory, which the emitted code already needs
for the span.

### Measured before believed, because the fear was reasonable

`SequenceEqual` against a constant is folded by the JIT into word-sized compares; a call
into a runtime routine might not be, and a keyword list is mostly *misses* — fifty-nine of
sixty words failing at their first character, where the chain stops after one comparison
and a call cannot. Nanoseconds a call on a seven-character word:

                             chain   folded   exact
      a hit                   4.02     2.10    1.90
      a miss, first char      2.04     2.10    1.90
      a miss, last char       4.03     2.10    1.91

Twice as fast on a hit and on a late miss, within 3% on an early miss, and within 0.2 ns
of the exact compare. A win or a wash everywhere.

### How it gets there, since it is not the JIT

`Ordinal.EqualsIgnoreCase` is hand-written and the trick is that ASCII letters differ by
one bit: `c | 0x20` lowercases both sides with no table. One OR is not enough — `'@' |
0x20` is a backtick — so every difference is checked against `(v - 'a') <= ('z' - 'a')`
before being forgiven. Under `Vector128<ushort>.Count`, which is eight characters and so
every keyword worth the name, it takes the scalar path: 64 bits at a time, four characters
a chunk. "DEFAULT" is two chunks where the chain was seven `ToUpperInvariant` calls.

And the runtime falls back to full Unicode comparison the moment the data is not ASCII —
which is the same line the emitter now draws. Beyond ASCII the two foldings genuinely
part company (a surrogate pair has no per-`char` answer at all), so a literal in somebody
else's alphabet keeps the chain rather than quietly changing what it accepts.

    SqlStandard92        26,473 -> 23,837    -10%
    ExpressionLanguage   25,349 -> 25,358      0%
    Rfc3986              25,571 -> 25,571      0%

Only SQL moves, because only SQL is made of case-insensitive keywords — 192 of them. Two
snapshots moved with it and both diffs are the intended shape.

## Fixed: the failing position is worked out only where something reads it

Igor, reading the emitted `BETWEEN`: "in this code we work out which character we broke
on — how is that information used afterwards? Not in general, in this particular place.
Here it looks like a vestigial pattern, done because everything is done this way, and
nobody needs it."

Right, and the emitter agreed with him in one of the two places it does this. A literal
*run* guards the work with `if (fail == Fail)`; a literal on its own did not. `p` at a
terminal failure is what the caller is told; a failure routed anywhere else goes to a door
that opens with `p = turn{n};` — a lookahead rewinding, an atomic group trying the next
alternative — so the ladder is overwritten by the next line to run.

No correctness was at stake: `FailsWhereItBegan`, which is what admits a failure target
with no give-back, is true for a literal only at length one, and a literal of one never
sharpens. The two sites are the same question and now answer it the same way.

    SqlStandard92   23,837 -> 23,703

**134 lines, and the smallness is the finding.** The case it was aimed at is not fixed by
it: `?!Reserved` is where a keyword list is usually reached and where every one of sixty
ladders leads to a position nobody reads — but `Reserved` is over the inlining threshold
now, so it is a rule compiled once, in its own context, where `_fail` *is* `Fail`. The
emitter cannot see from inside a shared body that every caller will discard the answer.
That is a fact about the whole graph — "is this rule reached only from negative
lookaheads" — and a different pass from this one.

## Built: a choice of keywords is entered through a switch

Igor: «давай таблицу переходов по первому символу».

`Predictive` asks whether one character says *which* alternative this is, and a keyword
list never lets it: `AND`, `ALL`, `ANY` and `AS` all begin with `A`. But one character
does say which *group*, and that is the useful half of the same fact. So `Dispatchable`
gathers the alternatives by first set, admits the gathering only where the sets partition
— any two equal or disjoint — and `CompileDispatchedChoice` writes a `switch (c)` that
lands in the right group. The groups themselves are the same chain as before; the chain
moved into `CompileChainedChoice` so both callers write it.

Order is kept where order can matter. Between groups it cannot: one character decides,
and no alternative outside the chosen group could have matched whatever it was written
after. Inside a group the written order and every way back are exactly what they were.

**The second half is worth more than the switch.** A dispatched group is entered only
through the switch, and the switch proves both halves of what each alternative was about
to ask — that there is a character, and that it is this one's. So neither the test nor the
bounds check around it is written inside the group at all, which is what the `proven`
argument of `CompileChainedChoice` is for. The elision is sound *because* dispatch is the
only way in: the ways back a group writes name states in that same group and carry the
position the switch tested, and the one path that used to arrive untested — the end of the
input — now fails at the switch before any of them exists. The general chain cannot do
this, and says so where it declines (`Machine.cs`, the way-back comment).

    SqlStandard92   24,418 -> 23,500 lines

Interleaved A/B, best of three windows each, nanoseconds:

                                                       chain   switch
      a = 1                                              454      458
      salary BETWEEN 1000 AND 2000                       983      935
      x IN (1, 2, 3) AND y IS NOT NULL                 1,098    1,003
      (quantity + weight) * rate - … < offset          5,306    5,033
      warehouse.zip_code = 'X' AND vendor_key IS …     2,170    2,005

**Five per cent, and the smallness is again the finding.** Two reasons, both worth
writing down.

The first is that the chain was never sixty tests. `Skipped` makes a failed character
test jump past everything that begins the same way, so sixty keywords cost twenty-six
tests, not sixty — and `Grouped = 4` is set where four tests stop being obviously cheaper
than a jump table. The identifiers in the first run of this benchmark were `a`, `b`, `c`,
`x`, `y`, which met the chain at its first tests; spread across the alphabet the gain is
consistent and still small.

The second is that most of a SQL parse is not the keyword list. `?!Reserved` runs once per
identifier, and twenty-six tests saved there is five per cent of the whole.

**Two things it does not reach, both deliberate.**

`ExpressionLanguage` is flat — 0.97x to 1.03x, which is this machine's noise. Its keywords
are case-sensitive, so `CompileLiterals` had them already: a run of plain literals shares
its prefix and decides where the texts differ, without entries and without a first-set
test per alternative. A switch in front of that is a jump table in front of something that
was already deciding on the first character. It is not a loss, and it is not a win.

A bare list of case-insensitive literals never reaches the dispatcher at all: the
checkpoint class takes it first, and that class is the better machine — a way back three
locals hold rather than an arena entry. SQL's reserved words reach dispatch only because
`wordboundary` puts a look-behind in front of each one, which is not checkpoint-silent.
Left as it is on purpose: dispatch is the fallback for a choice that was going to write
entries anyway, not a replacement for a choice that writes none. Dispatching *into*
checkpoint groups is the shape that would have both, and it is a bigger change than this
one.

And one thing left on the floor: `CompileLiterals` does not take `proven`, so a
case-sensitive group re-reads the character the switch tested — `if ((uint)p >=
(uint)text.Length || text[p] != 'a')` right after `case 'a':`. One predictable branch, and
the measurement above is what it is worth.

## Measured: the syntactic machine over token kinds, and what it costs today not to have one

Igor: «мы занимаемся фигнёй… главное развязать логику выделения токенов и основную логику
парсера. мы смешали эти вещи и теперь никак не можем выйти ни на нормальный объём кода, ни
на нормальную производительность».

He is right, and the last three entries in this file are the evidence: each attacked the
keyword list from inside the character machine, and each ends by saying how small the win
was. So the question was put to a probe instead of to another optimization.

**What was built.** `.work/kinds`, scratch and not in git. `SqlStandard92`'s syntactic half
— the forty rules from `SearchCondition` through `Subquery` — transcribed *mechanically*
onto an alphabet of one character per token kind, compiled by the **unmodified** generator,
and fed by a hand-written SQL-92 tokenizer. Mechanically because a grammar retyped by hand
is a grammar that might be subtly easier than the one that ships.

Nothing in the compiler had to change, and that is the first finding. `CharRange` is
`(char From, char To)`, so `FirstSets`, `FollowSets`, `Determinism`, `Doors`, `RangesTest`,
`Predictive`, `Skipped` and the `Dispatchable` switch built two entries ago are **already a
machine over a 16-bit alphabet**. Handing them kinds instead of characters is a matter of
what the input string holds.

**A gate ran first and blocked.** Forty-two inputs — nine search conditions, their refusals
derived by cutting the last token, and adversarial ones — had to get the same verdict from
both parsers before a nanosecond was reported. A probe measuring a weaker grammar measures
nothing.

**What the arena holds:**

                                  chars    kinds
      generated lines            23,500    6,580   3.6x
      Choice entries written        143       19   7.5x
      Call entries written          299       64   4.7x
      Run / Lookahead / Atomic       20        0
      reads of text[p]              692      320

**Time**, nanoseconds, min of seven windows, three runs in agreement — `lex` the tokenizer,
`kinds` the parse alone, `total` both, `!` a refusal:

           chars        lex      kinds      total
            464n        85n       209n       294n   1.58x  a = 1
            919n       132n       282n       414n   2.22x  salary BETWEEN 1000 AND 2000
          4,533n       727n     1,007n     1,734n   2.61x  (a + b) * c - d / e > f AND …
          5,287n       843n       994n     1,837n   2.88x  (quantity + weight) * rate …
            777n        92n       137n       229n   3.39x  ! a =
         11,518n       637n     1,836n     2,473n   4.66x  ! (a + b) * c - d / e > f AND …
         19,248n       767n     1,800n     2,567n   7.50x  ! (quantity + weight) * rate …

Accepted 1.49x to 2.88x, median 2.22x. Refused 2.08x to 7.50x, median 4.27x. The bar set
before the measurement was 2x end to end **or** 3x smaller with no slowdown; both were met.

**Refusals gain most, and that is the arena numbers showing through.** Refusal is where a
backtracking engine walks every reading still alive, and there are seven times fewer ways
back to walk. It is also the case the last three entries were chipping at from the wrong
side.

**Two biases, opposite, both stated.** The tokenizer is hand-written, so a generated one is
unlikely to beat it — optimistic. It also builds three buffers and a string per call,
because the generated parser takes a `string` where the design wants a virtual stream —
conservative. An attempt to measure the second directly gave numbers that contradicted
themselves across two runs, so it was dropped rather than explained: `bare` came out
*slower* than the allocating path in a third of the rows, which is not physically possible,
and no explanation was found that survived a second run.

**What it does not show.** There is no generated lexer, so "the lexer needs no arena" is
asserted by construction. And 143-to-19 compares a whole parser, lexical rules included,
against a syntactic half that has none — the gap is wide enough that the conclusion holds,
but it is not like for like.

## Found: a repetition led by a word literal takes one turn, and the boundary is why

Turned up by the transcription above, unrelated to it, and larger than SQL. The smallest
form has no SQL in it at all:

```dotgram
wordboundary = ['a'..'z' | '0'..'9' | '_']
trivia = { ' '* }

Item  = "when" & ['a'..'z']
Start = "case" & Item+ & "end"
```

`case when a end` reads. `case when a when b end` does not, nor does three of them. Strip
the trivia and the word boundary, write the same shape over single characters, and it reads
any number.

So `SqlStandard92` refuses `CASE WHEN a > 1 THEN 'big' WHEN a > 0 THEN 'small' ELSE 'none'
END` — ordinary SQL — and refuses `CASE a WHEN 1 THEN 2 WHEN 3 THEN 4 END` the same way.
Both `SimpleWhen+` and `SearchedWhen+` are hit, and both begin with a word literal. The
transcription accepts them, having no word literals and therefore no weaving, which is how
the gate caught it: the one input of forty-two where the two parsers disagreed, and the
probe was the one that was right.

Not fixed here — this entry is the probe's, and `src/` was deliberately untouched by it —
but it goes first in what comes next. It is a correctness bug in shipping code, and where
it lives is worth noticing: in the seam machinery, which exists only because lexing and
parsing are the same machine.

## Fixed: a turn that holds a seam is spaced from the next one

The defect the probe turned up, and the first thing done about it.

Two passes space the turns of a repetition and neither saw this shape. `Repeated`, during
lowering, spaces a repetition whose body is a **sequence** — an author who wrote a seam
inside a turn has already allowed one at the join. `SpaceLists`, after the types exist,
spaces a repetition whose turn is a **valued rule** — a collection's elements are spaced.
A call to a *valueless* rule whose body is a sequence is neither, and fell between them:

```dotgram
trivia = ' '*
Item  = "when" & ['a'..'z']
Start = "case" & Item+ & "end"
```

`Repeated` saw a `Call`, not a `Sequence`, and looked no further. So the loop body began
with `"when"` where `p` still stood on the space, the literal failed, and `Item+` ended
after one turn. With `wordboundary` on, as `SqlStandard92` has it, it failed one step
earlier and more confusingly: §4.6's `?<!wordboundary` read `text[p - 1]`, found the last
letter of the previous turn, and refused the second turn before its literal was compared
at all.

The fix reaches one step further, in `SpaceLists` where the bodies already exist: a turn
that calls a rule **whose own body carries the seam** is spaced, for exactly the reason
`Repeated` gives. Valuedness stays as the other half of the same predicate — a valued rule
is a list's element and is spaced whatever its body looks like.

**What decides it is the callee's seam, not the caller's.** A rule declared where `trivia`
is empty has no seam to find, so `Word*` over a lexical `Word` is untouched and `ab cd`
stays two words. That is the whole of what keeps a lexeme a lexeme, and it is why the
existing `Assert.False(Matches("trivia = ' '*\nStart = Word*\nWord = ['a'..'z']+", "ab cd"))`
still holds — along with a new test that puts a *sequence* body in a `trivia = none`
namespace, since a sequence is precisely what would otherwise have tempted the spacing.

    SqlStandard92   23,500 -> 23,524 lines

Twenty-four lines, and two sites: `SimpleWhen+` and `SearchedWhen+`. `ExpressionLanguage`
and `Rfc3986` are byte for byte what they were, and so are both snapshots. So
`SqlStandard92` now reads `CASE WHEN a > 1 THEN 'big' WHEN a > 0 THEN 'small' ELSE 'none'
END` and `CASE a WHEN 1 THEN 2 WHEN 3 THEN 4 END`, which it refused before.

Nine tests, and they were checked the only way a regression test can be: by turning the
fix off and watching three of them fail. Timings after the fix sit about five per cent
above the ones measured earlier the same day, uniformly and including inputs with no
`CASE` in them at all — inputs the change cannot reach — so that is the machine drifting,
not the seam costing.

## Built: the terminal inventory, and it disagreed with me twice

The first pass of the lexical split (`docs/lexical-adt-design.md`, phase 1). A pure
function of the graph in `Grammar/TerminalInventory.cs`: it emits nothing and rewrites
nothing, and answers three questions — what a lexical machine would have to recognize, what
numbers those results carry, and what in this grammar stands in the way.

**The boundary is a reference, not a file.** A rule is syntactic where it carries trivia
(§4.5) and lexical where it does not, and a terminal is a call that crosses from the first
to the second. Nothing has to be declared, which is the whole point: an author who wrote
`namespace Lexical { trivia = none }` has drawn the line already, and one whose grammar has
no trivia at all has said the thing is lexical from end to end. `Rfc3986` answers
"scannerless: no rule carries trivia, so there is no boundary" and pays nothing.

**A word is told from a mark by the shape §4.6 already left.** `Bounded` weaves
`?<!boundary & literal & ?!boundary` round a literal whose every character continues a
word, and leaves `'('` alone — so the lowered graph carries the answer and the boundary
rule need not be asked again.

    SqlStandard92     106 Word    17 Mark    12 Class    135 terminals
    ExpressionLanguage 38 Word    47 Mark    22 Class    107 terminals
    Rfc3986            scannerless

**The 106 and the 17 are the finding.** Yesterday's probe derived the same two numbers by
hand from the grammar text — 56 reserved words plus 50 that SQL-92 does not reserve, and
seventeen symbols — and this pass reached them from the graph without being told. Two
readings of the same grammar agreeing is worth more than either.

**It disagreed with me twice on the way, and both times it was right.**

The first run put `"OR"`, `"AND"`, `"IS"` and thirty more in *both* the word group and the
mark group, and had `'\t'`, `"--"` and `"/*"` among SQL's terminals. Two mistakes, and both
were mine. Normalization flattens §4.6's woven triple into whatever sequence surrounded it,
so matching it as a whole rule body found only the keywords that stood alone — the shape
has to be looked for at every position. And `trivia` and `wordboundary` are ordinary rules
in the same spaced namespace as the syntax, so they were walked like syntax; excluding the
two roots is not enough, because `trivia = { (Whitespace | LineComment | BlockComment)* }`
is three more rules with trivia entries of their own. It is the closure that has to go.

**And it named something the design had not.** `ExpressionLanguage`'s `Keyword` lists
thirty-eight words inside a lexical namespace, reached only through `Name = ?!Keyword &
Word` — and every one of those words also stands in the syntax as a literal of its own.
Give `Keyword` a kind and the lexer has to decide whether `if` is the word or the class,
and either answer breaks the other reading. The design's answer is that it is neither: it
is the range those words already occupy, and `?!Keyword & Word` becomes one set difference
over integers. So it is reported rather than numbered.

`SqlStandard92` does not have the problem, and why not is worth reading: its `Reserved`
sits where trivia is *not* empty, so it is syntax, and its words are walked into the word
group like any others. The same list, one namespace apart, is two different problems.

**What is still blocked, and it is honest that it is.** SQL has two: `[^ '(' | ')']` in
`Subquery` and `Balanced`, sixty-five thousand characters each. Over kinds that means "any
token but a bracket", which is a different statement, and this is the rule the grammar's own
comment already calls "the one place this grammar is knowingly wrong". `ExpressionLanguage`
has nothing blocked but the `Keyword` overlap.

Eight tests, on the model rather than on generated code, because nothing is generated yet.

## Built: a rule that is a choice of literals is a set, and a set is a range

The inventory's own report said what to do next, so this does it. `TruthValue`, `CompOp`,
`Quantifier`, `Reserved`, `Keyword` — a rule written as a choice of literals recognizes
nothing its literals do not recognize already. Over characters that is a choice and costs a
choice. Over integers it is a *set*, and a set that occupies one run of kinds is a subtract
and a compare.

So the numbering is arranged for it. Greedy and laminar: take the largest set that divides
what is left, put its members before the rest, and order each half the same way inside.
Every set nested in another comes out whole, and so does every set disjoint from the rest.

    SqlStandard92                                ExpressionLanguage
      Reserved           56   1 range   1..56      Keyword  38   1 range   1..38
      ExtractField        8   1 range  57..64
      SetFunctionType     5   1 range  65..69
      TruthValue          3   1 range   1..3
      Quantifier          3   1 range   4..6
      TrimSpecification   3   1 range   7..9
      SetQuantifier       2   2 ranges  4..4, 10..10

**`Reserved` in one run is the whole point.** `Identifier = ?!Reserved & RegularIdentifier`
runs a fifty-six-way negative lookahead at every identifier position today; over kinds it
is `(uint)(kind - 1) > 55`. Yesterday's probe numbered exactly this way by hand, from the
grammar text; this reaches it from the graph.

And the three that sit *inside* `Reserved` — `TruthValue` at 1..3, `Quantifier` at 4..6,
`TrimSpecification` at 7..9 — are whole because they are nested, which is the laminar
ordering earning its keep rather than a coincidence.

**`SetQuantifier` is two ranges, and that is the honest answer rather than a complaint.**
It is `DISTINCT | ALL`, `Quantifier` is `ALL | SOME | ANY`, they share a word and neither
contains the other, so no ordering makes both a single run. The one that loses carries two
ranges: two comparisons, and still not a fifty-way choice. The design's lowering ladder has
a rung for it.

**`ExpressionLanguage` now has nothing blocked at all.** `Keyword` was the only entry, and
it was the entry precisely because it was being made a class while its strings were already
terminals — `if` with two kinds. As a range it is neither a kind nor a lookahead, which is
what the previous entry said the answer would be.

A set is recorded only where every one of its literals is already a terminal. A rule listing
a word that no syntax ever writes has a string in it that nothing else numbers, and
promoting it here would invent a terminal out of a lookahead; such a rule stays a class and
is reported as one.

Nine tests. `SqlStandard92`'s two blocked entries are unchanged and still the `Subquery`
rule its own comment calls knowingly wrong.

## Built: the syntactic machine over kinds, and three things it taught while being built

`LexicalSplit` rewrites a graph so its terminals are token kinds instead of characters.
The machine underneath does not change at all, which is the point: `CharRange` is
`(char From, char To)`, so a graph over kinds is a graph, and `FirstSets`, `FollowSets`,
`Determinism`, `Doors`, `Predictive` and `Dispatchable` all run over it without being told.

    "SELECT"                 -> one character standing for that kind
    ?<!b & "SELECT" & ?!b    -> the same; the boundary was the lexer's all along
    ['+' | '-']              -> the kinds those characters carry
    RegularIdentifier        -> the kind of the class
    Reserved                 -> the range those fifty-six words occupy
    trivia                   -> nothing

Emitted with the unmodified emitter and run against `SqlStandard92` on the probe's
forty-six inputs, the tokenizer being the hand-written one from `.work/kinds` now driven by
the *generated* numbering:

    SqlStandard92     23,500 lines -> 6,182       44 of 46 inputs agree

**Three things the design did not know, each found by running it rather than reasoning.**

**A class stands for itself and for the words it would have matched.** `zone` is a word of
SQL-92 — `WITH TIME ZONE` — and is not reserved, so `Identifier = ?!Reserved &
RegularIdentifier` takes it over characters. Over kinds it arrives as that keyword's kind
and never reaches `RegularIdentifier`. The gate said so precisely: one input failed, and
`zone` was the only word in it that could have been the reason. So a crossing rewrites to a
set — the class plus every word the class accepts — with `?!Reserved` in front taking the
reserved ones back out. That union is the set difference this file promised two entries
ago, reached from the other side, and it is the general answer to contextual keywords.
Deciding it wanted a matcher over the lexical rules; the strings are keywords and the rules
are small, so it answers exactly rather than approximating, which matters because
approximating one way refuses valid programs and the other way accepts invalid ones.

**Nothing may rewrite to nothing where nothing means something else.** `?!wordboundary` has
an operand that is entirely the lexer's, and rewriting it away leaves a negative lookahead
over what matches the empty string — which refuses everywhere. The first run cut half of SQL
out of the machine that way and *looked like a triumph*: 3,313 lines, `Choice` entries down
to 15. Then the gate refused all forty-six inputs. The number was measuring a machine that
accepted nothing.

**Two classes that accept the same string cannot both be numbered.** A token carries one
kind. `Digits` and `UnsignedNumericLiteral` both accept `0`, longest match gives the
second, and `Length = '(' & Digits & ')'` — the precision of a `NUMERIC` or a `VARCHAR` —
stops reading. That is the two inputs of forty-six that still disagree, and it is not the
compiler's to fix: one kind for both widens the language, choosing one narrows it. So it is
refused, with the string as the witness.

The check found three:

    SqlStandard92        Digits / UnsignedNumericLiteral         both accept "0"
                         CharacterStringLiteral / QuotedString   both accept "''"
    ExpressionLanguage   TypeName / Word                         both accept "A"

**And the third one moved an item up the plan.** `TypeName = Word & ('.' & Word)*` is not a
token; it is syntax living in a lexical namespace only so that `System . Text` will not read
with the spaces in it. Which says that **`trivia = none` marks where trivia is off, and that
is not the same line as where tokens end** — inference from it picks up rules that are not
terminals. Notation for an explicit lexical root was last on the list of what to do; it is
now third, because no analysis can tell `Word` from `TypeName` without being told.

Ten tests over the two passes. Nothing under `src/` outside the two new model files
changed, and the three shipping parsers are byte for byte what they were: this is a
function of a graph that nothing calls yet.

## Fixed: a kind is a set of patterns, not a pattern

Igor, on the claim that `INTERVAL '1' DAY` would stop reading: «почему? … это - keyword,
строка, keyword». He was right, and the reason is worth the entry.

The previous two entries numbered one kind per *pattern* — per keyword, per mark, per
class. But `SELECT` is matched by the keyword **and** by `RegularIdentifier`; `0` by
`Digits` **and** by `UnsignedNumericLiteral`; `'x'` by `QuotedString` **and** by
`CharacterStringLiteral`. A lexer that has to answer with one of them makes every syntactic
position that wanted the other stop reading — so the check written last time refused three
grammars, and all three were fine.

A kind is the **set** that matched:

    10          {Digits, UnsignedNumericLiteral}
    1.5         {UnsignedNumericLiteral}
    SELECT      {"SELECT", RegularIdentifier}
    zone        {"ZONE", RegularIdentifier}

`Length = '(' & Digits & ')'` takes the first. A value position takes the first and the
second. An identifier position takes the third and the fourth, and `?!Reserved` takes the
third back out again. The test for a pattern is "the kind's set holds it" — a set of kinds,
worked out here and lowered to a range test, so it costs what one comparison cost before.

**Contextual keywords, overlapping literal classes and the reserved-word lookahead turn out
to be one mechanism seen from three sides.** The union written by hand last time — a class
standing for the words it would have matched — is what falls out of this rather than
something added to it.

    SqlStandard92     135 patterns -> 137 kinds     46 of 46 inputs agree
    ExpressionLanguage 106 patterns -> 107 kinds     nothing blocked

Forty-six of forty-six, where the pattern model managed forty-four and refused to run at
all once its own overlap check was added. And the two extra kinds are exactly the two
overlaps: `{Digits, UnsignedNumericLiteral}` and `{QuotedString, CharacterStringLiteral}`.

**Time**, min of seven windows, the hand tokenizer now driven by the generated numbering
and the syntactic machine emitted from the rewritten graph by the unmodified emitter:

           chars        lex      kinds      total
            457n        98n       157n       255n   1.80x  a = 1
          4,728n       802n       819n     1,621n   2.92x  (quantity + weight) * rate …
          3,043n       517n       525n     1,043n   2.92x  amount * 1.05 + tax >= total …
          9,798n       674n     2,211n     2,885n   3.40x  ! (a + b) * c - d / e > f AND …
         16,880n       726n     2,211n     2,937n   5.75x  ! (quantity + weight) * rate …
          5,223n       507n       570n     1,077n   4.85x  ! amount * 1.05 + tax >= total …

    SqlStandard92     23,500 lines -> 6,217

**How the sets are found, and what is still approximate.** A literal's set is exact: its
text is known, so which classes accept it is a question with an answer, and `Language`
answers it by running the rule against the string. A class's sets are the cliques of the
classes it overlaps, and overlap is witnessed by the shortest string either accepts —
sound where it fires, and it fires on all three of the real cases. A clique that turns out
never to occur is a number nothing emits, which costs nothing; a set with *no* number would
be a string the lexer recognized and could not report, and that is what must not happen.

The exact enumeration is the automaton's, which is the next thing to build: one machine
over all the patterns, its accepting states carrying the sets. Which is what Igor asked for
two entries ago — «получить полный список всех правил, соптимизировать их» — and it turns
out to be the same construction as the numbering rather than a step after it.

Also settled by the same reasoning, and no longer needing anything from the language: a
negated class names what it *excludes*, so `[^ '(' | ')']` is "any token but a bracket";
and `List<List<int>>` needs no lexer state, because `>` is a declared pattern and a cursor
that can be asked for a particular kind splits `>>` by rescanning. Notation for a lexical
root, which the last entry moved up to third, moves back off the list entirely.

## Built: one automaton over all the patterns, and the kinds stop being a guess

The last entry worked out the kinds from witnesses — a clique for every pair of classes one
of which accepts the other's shortest string. Sound where it fired, and coarse: a clique
that no string can produce still got a number.

Now they come from the machine. Thompson over all the patterns at once, then a subset
construction, and the accepting sets *are* the kinds. Over an alphabet of atoms rather than
characters: cut at every boundary any pattern's element set has, and what lies between two
neighbouring cuts is inside all the same sets, so `\p{L}` costs one symbol per interval it
already had rather than one per letter.

    SqlStandard92      135 patterns -> 135 kinds     (the witnesses guessed 137)
    ExpressionLanguage 106 patterns -> 106 kinds

**The two that went are the finding.** `{Digits}` and `{QuotedString}` were numbered by the
approximation and are impossible: every string `Digits` accepts is also an
`UnsignedNumericLiteral`, and every `QuotedString` is also a `CharacterStringLiteral`, so
neither can ever accept alone. That is a fact about two languages and no witness can reach
it — only reading them together can. It cost nothing to be wrong about (a number nothing
emits), but being right about it is the difference between a construction and a heuristic.

Forty-six of forty-six inputs still agree against the shipping parser, and the whole of it —
parse, bind, normalize, build the automaton, emit six thousand lines — takes 1.45 seconds.

**One thing had to be put back.** The kinds come out in the order the subset construction
meets them, which is not the order the patterns are in — and the laminar ordering that makes
a named set one run of kinds only survives if the kinds follow the patterns. So they are
renumbered by the lowest pattern each set holds, which puts the word kinds in word order and
the class kinds after them. Two tests caught it: `Keyword` came back as `2..4` where it had
been `1..3`.

**What it refuses, and why refusing is right.** A pattern is a regular language or it is not
a pattern: a lookahead, an external recognizer or a rule that reaches itself is none of the
shapes a Thompson construction has. Rather than approximate them the split declines and the
grammar keeps the character machine, which is correct and right there. `BlockComment`'s
`(?!"*/" & any)*` is the shape that would come up, and it never does — it is trivia, which
is skipped rather than tokenized.

The lexer itself is next, and most of it now exists: the states are built, and what is
missing is writing them out.

## Built: the lexer, generated — and it is faster than the one it replaces

The automaton was already there; this writes it out. One method, one array of accepting
kinds, and a static field per wide character set.

    SqlStandard92    528 states, 2,222 lines, no arena write

**Direct code and not a table, and it was measured before it was chosen.** Over an alphabet
of 897 atoms a dense table is 473,616 cells and out of the question. Merging atoms that
neighbour each other leaves 186,342 tests, because the atoms alternate — a keyword's letters
cut the alphabet everywhere a category is dense. Grouping the ways out of a state by *where
they lead* leaves **1,034**, forty-three at the widest state, and each of those is the
character set the grammar wrote, lowered the way every other character set is: comparisons
while there are few, a searched array of bounds beyond that.

**No arena write survives in it**, which is what the design asked for as the correctness
signal. Not an optimization: a lexical machine that needed a way back would mean a pattern
had been admitted that is not a regular language, and the split would be wrong upstream. It
is checked by a test, not by reading.

**And it is faster than the hand-written tokenizer it replaces**, which every earlier
measurement had treated as an optimistic bound:

           chars        lex      kinds      total
            460n        69n       163n       232n   1.98x  a = 1
          3,891n       412n       838n     1,250n   3.11x  (a + b) * c - d / e > f AND NOT g < h
          4,767n       872n       828n     1,700n   2.80x  (quantity + weight) * rate …
          9,656n       394n     2,236n     2,630n   3.67x  ! (a + b) * c - d / e > f AND NOT g <
         16,676n       794n     2,244n     3,038n   5.49x  ! (quantity + weight) * rate …

The hand tokenizer took 710 nanoseconds on the fifth line where this takes 412. Forty-six
of forty-six inputs still agree with the shipping parser, and the only hand-written thing
left in `.work/kinds` is the trivia skip.

**Two things nearly sank it, and both were caught by running it.**

The wide sets were written inline — `new char[] { … }` inside the scanning loop, an
allocation per character per test. The first generated lexer came out **seventeen times
slower** than the hand one, 8,997 nanoseconds against 510, slow enough to make the whole
split a loss. Hoisted into static fields it is faster than hand-written. A generated lexer
that allocates in its inner loop is not a lexer, and nothing but running it would have said
so.

And there were two numberings. The automaton met its accepting sets in whatever order the
alphabet took it; the inventory sorted them so a named set stays one run of kinds. That was
fine while the tokenizer was hand-written against the inventory, and became nineteen
disagreements the moment the scanner printed its own numbers. The sort belongs in the
automaton, which is the only place that can promise there is one numbering.

## Found: what a grammar that builds values still needs, by splitting one

The next step is the cursor, and its real content is not laziness but *provenance*: a
syntactic machine over kinds has no text, and every value a grammar builds is cut from
text. Rather than guess at what that costs, the smallest grammar with a capture and a
factory was split and emitted:

```dotgram
Pair  : @string   = k: Lexical.Name & '=' & v: Lexical.Digits => @(k + ":" + v)
Start : @string[] = (p: Pair)* & eof => @(p)
```

**Two things had to be fixed before it would split at all, and both were about built-ins.**

`eof` came back as a *class* — a terminal of its own. A built-in carries no declaration and
therefore no trivia entry, so every call to one looked like a crossing into the lexical
half. And since `eof` is `?!any`, the automaton then refused the whole grammar for holding
a lookahead inside a pattern. A built-in is a shape and not a terminal; it is walked through
now, in both passes.

`any` is `[^ ]` — a negation of nothing. The negation handling added two entries ago asks
what a negated class *excludes*, and an empty exclusion came back as "cannot be listed".
Over kinds `any` means one of whatever the alphabet holds, which names no terminal, so
there is nothing to number and nothing to refuse.

Neither would have been found by SQL: it never writes `eof` or `any` in syntactic position.
It took the first grammar that builds a value to write them, which is a reason to keep
reaching for a *different* grammar rather than a bigger one.

**And then the list, which is the point of the exercise.** The grammar splits, the machine
builds, the lexer is six states — and what is wrong is exactly and only provenance, at four
kinds of site:

    var captured0 = text.Slice(captured0From, captured0To - captured0From).ToString();

`text` is the kinds, so `k` comes back as the character standing for `Name` rather than as
`a`. The same slice appears in the constructed value, in the streamed form, and in the
failure message that quotes what was found; and `Match<T>.Position` is a token index rather
than a character one.

So the cursor's work is: carry `kind + start + length`, thread the original text into the
machine beside the kinds, and route every position-to-text and position-to-report through
it. Four kinds of site, enumerated by running a grammar that has them rather than by
reading the emitter. That is the next thing, and it is now a list rather than a worry.

## Built: a value cut from the text the tokens came from

The list the last entry made, done. A machine over kinds has positions that are tokens, so
nothing may cut a value out of what it is reading — the text and the extents travel beside
the kinds, and every cut goes through one place.

**It is threaded the way the whole input already was.** §8.2's `parserInput` has carried a
string from the publication down to the materializer since it existed; `parserSource`,
`parserStarts` and `parserLengths` ride the same five sites. What changes inside is one
method, `Cut(from, length)`, and the six places that used to write `text.Slice(...)` now ask
it — over characters it answers the slice it always did, over kinds it answers a call to a
helper that finds the first token's start and the last one's end.

The last one's *end* and not the next one's start, so that trivia standing after a run is
left out: a capture is what was written, not what was written plus the space after it.

    Pair : @string = k: Lexical.Name & '=' & v: Lexical.Digits => @(k + ":" + v)

    a=1              -> a:1
    a=1 bb=22        -> a:1|bb:22
      x=9  y=10      -> x:9|y:10

**And the position was wrong in a way only the character parser could show.** A refusal came
back at character zero however far in the trouble was, while the same grammar over
characters said six and fourteen. The mapping measured `starts.Length` — the array, sized
for the worst case and filled at the front — instead of the token count, and past the count
the array holds zeros. The count is the length of the kinds, there being one character a
token. That is what the test compares now: not a value typed into it, but what the character
parser makes of the same input, value and refusal position alike.

**Two things had to be turned off rather than made to work.** A split grammar cannot stream:
a window is a stretch of characters, and a machine over tokens is handed what a lexer
already found — there is nothing to grow. It was found the way these things are found, as
three call sites in the streamed forms that handed the recognizer characters where it now
wants the text. And with streaming off, `Failure.Starved` is never assigned, which the test
harness rightly calls a warning and a warning is a failure: the field is not emitted either.

Every character grammar is byte for byte what it was — `SqlStandard92` 23,524 lines,
`ExpressionLanguage` 25,804, `Rfc3986` 25,771 — because all of this is behind a flag that
only a split graph sets.

**What is left before a grammar can ask for this in its own text.** Trivia: it is skipped
rather than reported, so it is no pattern and has no kind, and until the lexer eats it the
entry point takes four arguments — the source, the kinds, and where each one was — rather
than one string. That is the last hand-written thing in the probe and the last thing between
here and an opt-in.

## Fixed: trivia was woven into the rules that are trivia

Found while asking why a lexical machine cannot recognize whitespace. §4.5 weaves the seam
between the operands of every sequence in a spaced namespace, and the rules `trivia` is
*made of* are sequences in that namespace like any others:

    LineComment  = "--" & [^ '\n']*        ->  "--" & trivia & [^ '\n']*
    BlockComment = "/*" & (?!"*/" & any)* & "*/"

    BlockComment:
      Sequence
        Literal "/*"
        Call trivia          <- and `trivia` is a choice that holds BlockComment
        Repeat 0..*
          Sequence
            Call trivia
            Lookahead "*/"
            Call trivia
            Call any
        Call trivia
        Literal "*/"

A rule woven with itself. It says nothing — a seam is `trivia`, `trivia` matches the empty
string, and `A & trivia & B` accepts exactly what `A & B` accepts — and it costs a call at
every seam inside every comment and every run of whitespace, compiled into a scanner that
then calls itself.

    SqlStandard92   23,524 -> 21,773 lines

**Seven per cent of the file, and nothing else moved.** `ExpressionLanguage` and `Rfc3986`
are byte for byte what they were: their trivia is a repetition of one operand, which has no
seam to weave. Both snapshots are unchanged and all 1,579 tests pass, which is what one
expects of removing something that accepts the same language.

Only where the seam is nullable, which is where taking it out changes nothing. A grammar
whose `trivia` must match something has said that operands are separated, and that would be
a statement about its own rules too — however odd it would be to mean it.

**How it was noticed is the part worth keeping.** Nothing about the size. A lexical machine
reads its patterns together as one automaton, and a rule that reaches itself is not a shape
a Thompson construction has — so `trivia` could not be a pattern, and a split grammar had to
be handed its tokens with the whitespace already skipped by hand. Chasing that found a rule
that had been calling itself for no reason since §4.5 existed.

What is left in the way of trivia becoming a pattern is now only the idiom
`(?!"*/" & any)*` — "characters up to a delimiter" — which is a regular language and not one
a Thompson construction reaches without being told. That is the next thing.

## Built: "everything up to a delimiter" is a pattern, though a lookahead is not

    Comment = "/*" & (?!"*/" & any)* & "*/"
    Text    = '"'  & (?!'"'  & any)* & '"'

Half the grammars there are write a string this way and all of them write a comment this
way. The language is regular — the strings in which the delimiter does not occur — but it
is not one a Thompson construction reaches by following the shape, because the shape is a
lookahead and a lookahead is none of its three cases. So the idiom is recognized and built
as Knuth, Morris and Pratt's automaton with its accepting state taken out, which is exactly
"the delimiter does not occur". Anything else wearing a lookahead is still refused, and the
test that used to prove that had to be rewritten around recursion, which no automaton can
count.

Exact where the repetition is followed by the delimiter, which is the only way anyone writes
it. Standing alone it admits a little more, because the operand's lookahead sees past what
the repetition consumed and an automaton cannot; what it admits is a longer run, and the
delimiter that follows cuts it back.

**The failure links are the whole of it and I got them wrong first.** Written in one pass,
`*/` came out with a link from one matched character back to one matched character, and a
link to itself is a loop the builder never leaves — the test hung for ten minutes rather
than failing. The prefix function is computed the standard way now, and `/*a**/` is the case
that proves it: after `*` and then another `*`, one `*` is still matched.

## Not built: trivia as a pattern

Igor: «можно же было просто вызвать существующий метод тривии».

Right, and the attempt said so too. Adding trivia to the patterns made the subset
construction run for ten minutes without finishing — a comment's `any` crosses every atom
of every other pattern, which is a product nobody needs. And there was no reason to build
it: §4.5's trivia is already compiled to a scanner (`Machine.Scan.cs`), atomic-braced and
with nothing written down, and a tokenizer that wants whitespace skipped can call it.

So the lexer recognizes terminals and the trivia scanner skips between them, which is what
the two were already for. What is left before a grammar can ask for the split in its own
text is emitting that scanner beside the lexer — the machine that renders it exists and is
reached through a rule rather than through the graph, so it is a matter of asking it, not of
building anything.

## Built: a split grammar reads one string

Igor's point about calling the existing trivia scanner is what made this short. The lexical
half of a generated file is now three things and none of them new: the machine
`LexerEmitter` writes, §4.5's `trivia` asked for as the scanner it already compiles to, and
the loop between them.

    public static Match<string[]> TryParseStart(string input)
    {
        var source = input;

        input = Tokenize_DotGram(source, out var starts, out var lengths, out var stopped);

        if (stopped >= 0)
            return Match<string[]>.Failed(Outcome.NoMatch, "…", stopped, null, null);
        …

The seam is asked for by rule rather than found by compiling — a lexical machine has no
calls to compile and still needs it — and the machine that renders it is built over the
original graph restricted to the trivia rules, so what it costs is the scanner and not the
parser it could have been. Tagged `_Seam`, because everything a machine emits is named after
its tag and this one lands in a file another machine has already filled: without it the two
sets of character tables collide name for name.

**A grammar with values, from one string:**

    Pair : @string = k: Lexical.Name & '=' & v: Lexical.Digits => @(k + ":" + v)

    TryParseStart("a=1 bb=22")  ->  a:1 | bb:22
    TryParseStart("a=1 b=")     ->  refused at character 6

Forty-six of forty-six on SQL, against the shipping parser, with no hand-written tokenizer
anywhere.

**And the advantage narrowed, which is the honest part.**

           chars      kinds
            573n       718n   0.80x  x IN (1, 2, 3) AND y IS NOT NULL
          1,118n     1,234n   0.91x  warehouse.zip_code = 'X' AND …
          2,517n     1,317n   1.91x  (a + b) * c - d / e > f AND NOT g < h
          6,832n     2,714n   2.52x  ! (a + b) * c - d / e > f AND NOT g <
         10,726n     3,133n   3.42x  ! (quantity + weight) * rate - zone / …

Accepted inputs run 0.80x to 1.91x where the hand-tokenizer measurement said 1.47x to 3.11x,
and two of nine are now *slower*. Two reasons and both are worth saying. The character
parser got faster: unweaving trivia took seven per cent off it, so the thing being beaten
improved. And this measures one call rather than two, which means it includes the three
allocations a tokenized parse makes — a `char[]` and two `int[]` sized for the input — that
the earlier figure split out. On a short condition that is most of the difference.

So the lazy cursor stops being an optimization looking for a reason: three allocations per
parse is the reason.

**One more number, from a diagnostic the build raised on its own.** The lexer's `Scan` is
estimated at 3,669 basic blocks, past the ~2,000 where the JIT stops optimizing (GRAM5003).
Direct code was chosen over a table on the strength of 1,034 tests against 473,616 table
cells, and 1,034 tests is right — but 528 `case` labels is a lot of *blocks*, which is a
different measure and the one the JIT reads. The design said the lexer wants a table; that
was overridden on size, and the size argument was about the wrong size.

## Built: the token buffers are kept, and the answer is that they were not the problem

Three allocations a parse — a `char[]` and two `int[]` sized to the input — were named last
entry as the reason a split grammar could lose to a character one on a short condition. They
are kept now, in one `[ThreadStatic]` slot taken out while in use, exactly as the parser
itself is: a parse reached from inside another gets its own, and a set grown past what an
ordinary input needs is let go rather than pinned to the thread for ever.

           before      after
             233n       220n   a = 1
           1,317n     1,258n   (a + b) * c - d / e > f AND NOT g < h
             718n       684n   x IN (1, 2, 3) AND y IS NOT NULL

**Five per cent, and the two rows that were slower are still slower.** So the allocations
were not what was costing the short inputs, and saying they were — which the last entry
did — was a guess that measuring has now corrected.

           chars      kinds
            533n       684n   0.78x  x IN (1, 2, 3) AND y IS NOT NULL
          1,077n     1,177n   0.92x  warehouse.zip_code = 'X' AND …
          2,485n     1,258n   1.98x  (a + b) * c - d / e > f AND NOT g < h
          6,299n     2,627n   2.40x  ! (a + b) * c - d / e > f AND NOT g <
         10,264n     3,017n   3.40x  ! (quantity + weight) * rate - zone / …

What is left is the tokenizer's own work. On `x IN (1, 2, 3) AND y IS NOT NULL` the earlier
split measurement had the lexer at about three hundred nanoseconds and the parse at about
the same — so tokenizing is half of it, and the character parser does the whole thing in
less. A short input is where reading it twice cannot pay: the second reading is cheap, but
the first is not free, and the character machine reads once.

That is a real limit and it is worth stating rather than optimizing around. Where the split
wins it wins for a reason that grows with the input — a refusal walks a fifth as many ways
back, a long expression reads a fifth as many items — and where it loses it loses by a fixed
cost that does not.

## Fixed: the lexer was never optimized, and dividing it is what a table would have been for

The last entry left a diagnostic unexplained — the scanner estimated at 3,669 basic blocks,
past where the JIT stops optimizing. Asked directly, with `DOTNET_JitDisasmSummary`:

    Kinds.SqlKinds:Scan(…)              [Tier-0 switched MinOpts, IL size=26637]
    Kinds.SqlKinds:Recognize_DotGram(…) [Tier1 with Synthesized PGO, IL size=3980]

So the lexer never reached Tier1 at all, and when it was compiled again it was compiled
*worse*, while the syntactic machine beside it — divided into parts by machinery that has
been there for months — reached Tier1 with PGO. Direct code was chosen over a table on the
strength of 1,034 tests against 473,616 table cells; the 1,034 was right and the conclusion
was wrong, because a test is not a block and 528 `case` labels are a great many blocks.

The answer was not a table. It was the division the syntactic machine already had: ninety-six
states to a method, and the loop picks the part by state range.

           before      after    against the character parser
             684n       513n    0.78x -> 1.05x   x IN (1, 2, 3) AND y IS NOT NULL
           1,177n       891n    0.92x -> 1.23x   warehouse.zip_code = 'X' AND …
           1,258n     1,033n    1.98x -> 2.45x   (a + b) * c - d / e > f AND NOT g < h
           1,268n       973n    2.89x -> 3.85x   ! amount * 1.05 + tax >= total AND …
           3,017n     2,752n    3.40x -> 3.74x   ! (quantity + weight) * rate - zone / …

**No `MinOpts` anywhere in the run, and no input is slower than the character parser any
more.** Accepted 1.05x to 2.45x, refused 1.35x to 3.85x. The two rows that lost last entry
were not losing to allocation and were not losing to the idea; they were losing to a method
the JIT had given up on.

Which is the third time this session a number has been believed and then measured. The size
of a method has three meanings — lines, IL bytes, basic blocks — and only the last one is
the one that decides.

## Done: a split grammar end to end

`ProvenanceTests` no longer carries a tokenizer. It emits one file, compiles it, and calls
`TryParseStart(input)` — the same signature the character parser has — and compares the
answer with the character parser's, value and refusal position alike. What is left of the
plan is the lazy cursor, and after this it is an optimization again rather than a repair.

## Built: `Lexical = true`, and SqlStandard92 asks for it

The split is a word in the grammar's own attribute now, plumbed the way `PartSize` is:

    [Gram("""
        …
        parse SearchCondition as ParseSearchCondition
        """, Lexical = true)]

A request and not a setting. Four things stop it and each says so in its own words
(`GRAM5004`, information rather than a warning, because the parser that comes out is the
one that would have come out anyway):

- no rule carries trivia — a URL is characters, and there is nothing to tell a token from
  one;
- a terminal that is not a regular language, so the patterns cannot be read together;
- a `find`, which hunts through characters for a place to begin, and a stream of tokens has
  no such places;
- a `trivia` not written in braces, the seam between tokens being skipped by the scanner
  braces ask for.

**`SqlStandard92` asks for it, and what that is worth:**

    21,773 -> 8,673 lines                      2.5x smaller

           chars      kinds
          2,532n     1,065n   2.38x  (a + b) * c - d / e > f AND NOT g < h
          2,175n       978n   2.22x  amount * 1.05 + tax >= total AND …
         10,307n     2,752n   3.74x  ! (quantity + weight) * rate - zone / …
          3,748n       973n   3.85x  ! amount * 1.05 + tax >= total AND …

`ExpressionLanguage` and `Rfc3986` are untouched and stay on the character path — the first
because its `TypeName` is syntax in a lexical namespace and wants moving first, the second
because a URL is characters. Which leaves both machines exercised by a real grammar, the
larger of the two still on the older path.

**And the thing that had to be built before the switch could be believed.** The suite did
not read a single string with `SqlStandard92`. It compiled it and nothing more, so "1,586
green" said only that the generator had not crashed — and on that evidence switching a
shipping parser to a newer code path is not a decision, it is a hope. Thirty-two tests now:
the corpus the split was measured against, the non-reserved words that are names (`zone`,
`year`), the reserved ones that are not (`having`, `select`) and the one that is reserved
and reads anyway (`value`, which §6.3 makes a niladic function — the test expected a refusal
and the parser was right), a `CASE` with two `WHEN`s, and refusals whose position has to
move with the input.

They pass with `Lexical = true` and they pass without it, which is the point: they are about
the grammar and not about which machine read it.

## What the second grammar found

`ExpressionLanguage` was asked to split, and did not — but not before turning up five
defects that `SqlStandard92` had no way to show, four of them in the emitter and one that
would have been wrong in silence.

**`OverKinds` was an `init` property, and a `Machine` does its work in its constructor.**
So the object initializer ran after every state had already been written. The materializer's
*declaration* is emitted late enough to have seen it and the *calls* to it early enough not
to: a guard called a seven-parameter method with four arguments. It is a constructor
argument now, which is what it always was.

**Four things a rewritten graph dropped.** `State` and `Context` are `init` properties of
`RecognitionGraph` that the split simply did not carry, and dropping `State` changes the
publication's signature — the author's own calls stop compiling. Worse were `Climbing`,
`Powers`, `Recoveries` and each rule's `Fold`, which are keyed by *node*: a `Node` is a
record, so they key by structure, and changing structure is the whole of what the split
does. Carried across they key nothing, and a left-recursive rule is then emitted as a plain
repetition — the tail's capture becomes an array and the accumulator is never bound, so
`=> @(Expression.OrElse(left, right))` names a `left` that does not exist. The rewrite now
records what each node became and says the four dictionaries again in those terms.

**And `SourceSpan` was carrying token indices.** `Cut` had been given seven sites and the
spans beside them none, so a grammar over kinds that asked `parserSpan` where something
stood would have been told which token, in an interface documented as characters. There is
a `Span` beside `Cut` now, and a `Span_DotGram` emitted only where one is asked for, plus
`At` for a position and `Source` for the input a line and a column are counted in — which
is what a recovery hands its handler, and recovery was reporting all four in kinds.

**What actually stopped it, and why it stays stopped.** `Hex : @string = "0x"i & t: HexRun
=> @(t.Replace("_", ""))` is three statements at once: what a hexadecimal literal looks
like, which part of it is the number, and that the separators come out. The lexer answers
the first; the token it hands over is `0x_1F` whole, and the other two are gone with the
parts they named. Six of `ExpressionLanguage`'s terminals are written that way.

Refused, and named — `GRAM5004` says which rule. Handing back the token's own text instead
would be a different parser that compiles, which is the worst of the three answers. The fix
is a rule read twice, once by the lexer for its extent and once by its own character machine
for its value, which is item 6 of `docs/lexical-adt-design.md` and not yet written.

`SqlStandard92` could not have found any of this. It builds no values, declares no state,
climbs no precedence and recovers from nothing — it answers yes or no. The second grammar
was worth more than the first measurement.

## A terminal read twice

`Hex : @string = "0x"i & '_'* & t: HexRun => @(t.Replace("_", ""))` is three statements at
once, and the previous entry recorded that the lexer can answer only the first. That was
the honest place to stop and it is not where this stops.

The rule is read twice now. Once by the lexer, which says where it ends; once by **its own
character machine, over exactly the text the token covers**, which says what it is worth.
The second read runs the same states an unsplit parser would have run, so whatever the
author wrote in `=> @(...)` runs against the captures it was written for.

It is the shape the seam already had. A `Machine` over the original graph, restricted to the
terminals that build and what they reach, tagged `_Value` so its tables and states do not
collide with the syntax's, and one way in per rule:

    static string Value_Lexical_Hex_DotGram(string token)

The syntactic materializer calls it in place of the factories it no longer has — the same
line the external-recognizer case has always written, for the same reason: no captures to
walk, and a value recovered by asking again from what the arena recorded.

**Three things had to change for it to work at all.**

The rule has to stay a *call*. Every other terminal is replaced by its kind test where it is
called from — the rule was only ever a name for a set of characters — and doing that to one
of these leaves nothing to read again: no entry, no extent, no case in the materializer. It
took an hour to find, because the parser compiled and ran and simply built empty strings.

It has to keep its declared type and lose all its members. The type is what makes the call
a valued one and gives the caller something to read; the members are what would make the
syntactic machine try to build it out of parts that are inside a token.

And the value machine has to join the file's value tables before anything is rendered. A
machine names a type by where it sits in one list they all agree on, and a second read
writing into `values11` while its caller reads `values9` is a defect that no test shape
catches except running it.

**Where `ExpressionLanguage` now stands.** 127 of its tests failed the first time it was
asked to split; 11 fail now, and all eleven are one thing:

    () => Math.PI            refused
    (string s) => s.Length   refused
    () => new int[] { 1, 2 } refused

`TypeName = Word & ('.' & Word)*` is lexical, and deliberately: without it `System . Text`
would be captured with the spaces in it. Over kinds the lexer therefore takes `Math.PI`
whole and `Postfix` never gets to read the dot as member access. That is not a defect of the
second read — it is a grammar that says a dotted name is a lexeme, which is true of a type
and false of a member access, and only a symbol resolver knows which it was looking at.

So `ExpressionLanguage` stays on characters, and the second read is covered by a grammar of
its own instead: `RereadTests` runs both parsers over the same inputs and requires the same
answer, and then requires that the answer is what the rules say — the separators come out of
`0x_1f`, the base is read, the quotes are gone. Values, and not merely agreement, because
two parsers agreeing on nothing would pass the first half.

## The scanner reads a row where it was asking forty-four questions

`Scan_Part0`'s first state was every mark SQL-92 writes and both cases of every letter a
keyword begins with, asked one at a time:

    case 0:
        if (c == '"') return 1;
        if (c == '\'') return 2;
        …
        if (c == 'A' || c == 'a') return 17;
        …                                       // forty-four of them

It is now one subtraction, one unsigned compare and one load. **The licence for that is
determinism**: this is a machine over an alphabet of atoms, each character belongs to
exactly one atom and each atom leads to one state, so no character satisfies two of a
state's tests. The chain's order carries no meaning, and any subset of it can be lifted into
a row with the rest left where it was.

What is lifted is what fits a small window — an edge every range of which lies under Latin
Extended-A. That is what keeps this from becoming the dense table the design rejected: a
Unicode category, or the "anything but a quote" of a string body, stays a chain, because a
row for one of those is most of a plane and the binary search it already uses is short.

**The threshold was measured rather than reasoned about, and the reasoning was wrong.** The
first guess was six ways out; then three, then two, then one — each beat the last, and rowing
every state that has a near edge beat rowing only the wide ones. Two comparisons are not
cheaper than a subtraction and a load, and 528 chains that each predict well still occupy the
predictor. What is left is a threshold on *comparisons* and not on ways out: a single
character is one comparison and a range is two, and a row replaces two or more. That never
turns one compare into three operations, and it is a wash against rowing everything.

    chain    row
      201    205   0.98x  a = 1
      398    383   1.04x  salary BETWEEN 1000 AND 2000
      464    387   1.20x  x IN (1, 2, 3) AND y IS NOT NULL
      985    970   1.02x  (a + b) * c - d / e > f AND NOT g < h
      913    870   1.05x  amount * 1.05 + tax >= total AND …
      839    787   1.07x  warehouse.zip_code = 'X' AND …
      881    711   1.24x  CAST(x AS INTEGER) = 5 OR SUBSTRING(…)
     1447   1346   1.08x  (quantity + weight) * rate - zone / 2 > …
       23     19   1.21x  ! amount * 1.05 + tax >= total AND …

Interleaved binaries, five rounds each, min of all — because sequential runs of the same
binary drifted by five to eight percent, which is more than the effect. The one input that
is slower is the shortest, and it is slower in every variant tried.

367 rows and 12,000 cells, `short` where the states fit in one, which is 24 kilobytes. The
generated file goes from 8,647 lines to 10,726. No part of the scanner fell out of
optimization: every `Scan_Part` still reaches `Tier1 with Dynamic PGO`, and nothing anywhere
says `MinOpts`.

## And then the loop stopped asking which state it was in

The rows of the previous entry were per state, so reading a transition meant finding the
state's row first: a chain of range tests to pick which of six methods held it, a call, and
a `switch` over ninety-six labels — a jump table, and an indirect jump the predictor cannot
help with, once per character.

All of it is gone. There is one array of cells for the whole machine and one number per
state saying where that state's row begins:

    var c    = text[p];
    var row  = Scan_States[state];
    var at   = c - (int)(row >> 32);

    if ((uint)at < 128u)
        next = Scan_Cells[(int)row + at];
    else
        next = <the chain, for what a row cannot hold>;

One load, one subtract, one compare against a constant, one load. Every row is the same
width, so only where it *starts* has to be looked up — which is the half of the descriptor
worth loading, and the half that makes this work for an alphabet that is not ASCII.

**Three things were wrong before they were right.**

The first table was anchored at zero and 128 wide, which is a table for English. A grammar
whose words are Cyrillic got rows that were entirely -1 and took the chain for every
character of every word. The row is placed now, not assumed.

Placing it by the state's *first* character was the second wrong answer. A state admitting
`'='` and а..я has an alphabet a thousand apart, and a window at the `'='` answers for one
character and sends the whole language to the chain. So the window is chosen to cover the
most of what the grammar named — the most **ranges**, and the widest where two windows tie.
Ranges and not characters, because a Unicode category is a great many characters in a great
many scattered pieces and counting characters drags every window into the middle of one.

And then it is slid as far down as it can go without dropping any range it was chosen for.
That was the third: `SqlStandard92`'s first state begins at `'"'`, and an input beginning
with `!` fell past the table to be refused by a chain that the table already knew the answer
to. The room below is free — those cells refuse, which is what the chain would have said —
and it is where the characters that end a token live. That one change took the immediate
refusal from 0.85x of the old code to 1.77x.

    chain    rows   table   vs chain
      201     204     194      1.04x  a = 1
      391     373     357      1.10x  salary BETWEEN 1000 AND 2000
      449     389     394      1.14x  x IN (1, 2, 3) AND y IS NOT NULL
      975     946     926      1.05x  (a + b) * c - d / e > f AND NOT g < h
      899     860     768      1.17x  amount * 1.05 + tax >= total AND …
      829     759     691      1.20x  warehouse.zip_code = 'X' AND …
      858     709     745      1.15x  CAST(x AS INTEGER) = 5 OR SUBSTRING(…)
     1425    1319    1244      1.15x  (quantity + weight) * rate - zone / 2 > …
       23      19      13      1.77x  ! amount * 1.05 + tax >= total AND …

Interleaved, five rounds each, min of all. It is also **smaller than what it replaced**:
53 distinct rows and 6,784 cells — thirteen kilobytes — where the per-state rows were 367
fields and 24, and the generated file goes from 10,726 lines back to 9,078, against 8,647
for the plain chain. A keyword trie has a great many states that admit exactly the letters
continuing a word, and at a fixed width those states share one row.

Every `Scan_Part` still reaches `Tier1 with Dynamic PGO`; nothing says `MinOpts`.

## SqlStandard92 gets a benchmark, and the benchmark says something

Every number the lexical split has been justified by — the split itself, the inventory, the
generated lexer, its division into methods, the transition table that replaced its `switch` —
came from a throwaway program that no longer exists. A parser whose numbers live nowhere is
one whose next change is measured against a memory. So `SqlBenchmarks` now holds the corpus,
graded nests and their refusals, and the input that stops being this language at its first
character.

It earned its keep on the first run.

    input                                accepted    refused
    a = 1                                    167n          —
    salary BETWEEN 1000 AND 2000             321n          —
    CAST(x AS INTEGER) = 5 OR SUBSTRING(…)   740n          —
    (a + b) * c > d                          540n      1,274n
    ((((a + 1) * 2) - 3) / 4) + b > 0      1,904n      8,457n
    ((((((a + 1) * 2) …) * 6) + b > 0      3,892n     20,736n
    ! a = 1                                    —           7n

A short condition allocates nothing, which is the token buffers being rented and handed back
working as intended. A refusal at depth allocates 2,520 bytes against the accepted parse's
328, and costs five times as much.

**And the shape underneath it.** Not an exponential — the ratios between successive depths
fall away (1.88, 1.69, 1.59, 1.49, 1.42, 1.36, 1.36), which is a polynomial and not the
thing this repository has twice found and fixed. What it is, is quadratic in *nesting*:

     n   parentheses        AND chain          a + 0 + 1 + …
     2   17ch     609n      27ch     569n      13ch    254n
    32  220ch  56,168n     401ch   6,434n     155ch  1,456n

The `AND` chain is fifteen times longer and eleven times slower — linear. The sum is twelve
times longer and six times slower. The parenthesis nest is thirteen times longer and
**ninety-two** times slower. Length is not the variable; depth is.

**It is the engine and not the split.** `ExpressionLanguage`, which reads characters and has
never been near any of this, has the same exponent — 40.8x for eight times the depth against
the split parser's 39.3x. So this is how the recognizer has always behaved and nothing
measured it, which is the same sentence as the first paragraph and the reason the file
exists.

## What the quadratic was, and it was one alternative

The recognizer is not quadratic in nesting. One rule is, and finding out which took four
ablations of the same grammar rather than any reasoning about the engine.

**Everything synthetic was linear.** A bare parenthesis recursion, the same with an operator
written as a repetition, the same written left-recursively, a ladder of six precedence
levels, that ladder with a woven seam, and that ladder with a lexical namespace — all of
them double when the depth doubles. So the engine's arena, its unwinding and its left-
recursion folding are not it, which is what the first hour was spent suspecting.

**Then the layers, and only one of them.** `SqlStandard92` publishes two rules, and they
disagree: over the same nest `ValueExpression` is linear (2,970 / 5,679 / 11,256 at depth 32,
64, 128) and `SearchCondition` is quadratic (71,379 / 273,343 / 1,072,011 — a clean factor
of four for a doubling). So the cost is in the predicate layer above the expression ladder.

**Then the shape of the input.** `nest > 0`, `0 > nest` and `nest IS NULL` cost the same, so
it is not the tail; and `(a, nest) = (1, 2)` is linear, which is the pair that names it — a
list with a comma in it is recognized straight away, and a bare parenthesis is not.

**Then the rule, by removing one alternative at a time:**

     n      whole   no subquery      no list   element only
    32    58,556n      63,008n       1,608n         3,286n
   128   867,543n     891,265n       5,799n        12,247n

    RowValueConstructor = '(' & RowValueConstructorElement
                              & (',' & RowValueConstructorElement)+ & ')'
                        | TableSubquery
                        | RowValueConstructorElement

The first alternative is the whole of it. `TableSubquery` — the balanced-bracket scan that
was the obvious suspect, being the one place this grammar is knowingly wider than SQL — costs
nothing at all.

**Why it costs what it costs.** On `(((a+a)+a)+a)` the alternative takes the `'('`, reads an
element, and asks for a comma that is not there. So it fails — and then the engine tries the
element *shorter*: a `+` chain of length n offers n places to stop, and after each of them
the comma is asked for again. n attempts over n characters is the square, and the nesting
supplies a fresh chain at every level.

That is a real property of the machine and not of this grammar: a sequence whose second half
fails is retried against every shorter reading of its first half. A PEG would have committed
after the first reading; this engine explores. Which of the two is wanted is a decision about
the notation — §4.4's atomic braces already say "do not come back here", and writing them
around the element would end this instance today. What they would not do is find the next
one, and nothing in the compiler currently can: no rule warns that an alternative may consume
an unbounded prefix before deciding.

## A faster set test, and why it is not being written

`Scan_Between` answers whether a character is in a set by binary search over the set's
boundaries, which alternate: a character is inside exactly where the count of bounds at or
below it is odd. `SqlStandard92`'s widest set is 382 ranges, so that is about ten dependent
loads with ten branches nothing can predict.

A page table is the obvious better shape, and it is what the runtime itself uses for
`char.IsLetter`: the top byte of the character picks a page, the page is 256 bits, and pages
that are equal are shared. Measured over the real set, on random characters:

    input             searched       paged    IsLetter
    ASCII letters        8.68n       2.56n       2.24n
    ASCII marks          4.81n       1.96n       2.43n
    Cyrillic             5.41n       1.37n       2.78n
    CJK                  5.19n       1.36n       2.78n

Three and a half to four times faster, and faster than `char.IsLetter` on everything that is
not ASCII — two levels against the runtime's three plus a category to decode. It costs 1,856
bytes where the bounds cost 1,528, because fifty distinct pages is all a set like this needs.

**And it is worth nothing at all.** Both machines already tabulate ASCII — the lexer in its
transition rows and the parser in `Recognize_DotGram_Class`, 128 bytes apiece — so a search
runs only for a character outside them. Timed end to end on the case that should show it
most, the same expression with Latin and with Cyrillic identifiers:

    shape              latin       other
    short name        8,654n      8,433n   0.97x
    long name        10,744n     12,208n   1.14x
    many names       16,512n     14,664n   0.89x

Noise, in both directions. A few nanoseconds against a parse of eight to sixteen
microseconds is a tenth of a percent, and the direction closes here rather than in the
emitter.

**One thing the measurement did settle**, which was the reason for looking. The set and
`char.IsLetter` disagree on 56 of the 65,536 characters. Forty-eight are the letters the
keyword trie took for itself, in both cases. The other eight are `U+1C89`, `U+1C8A`,
`U+A7CB`–`U+A7CD` and `U+A7DA`–`U+A7DC` — Cyrillic and Latin Extended-D letters added in
Unicode 16.0. The generator's tables know them; the consumer's `net8.0` runtime does not.

So calling `char.IsLetter` from emitted code would make the language a parser accepts a
function of the framework it is running on: the same assembly would take `U+A7CB` in an
identifier on one machine and refuse it on another. Expanding `\p{L}` into ranges at
generation time is what freezes it, and those eight characters are what that is worth.

## Two thirds of the generated SQL parser was one set, written out sixty-seven times

Chasing a faster membership test turned up something much larger than the test. The 67
`Scan_Set` declarations were **65% of the file** — 532,823 characters of 816,461.

They are that many because a keyword trie has a state per prefix, and each state's "any
letter that is not one I branch on" is a set of its own: four hundred ranges, written out
again for every state. And they are nearly the same set. A trie branches on the letters that
begin the words of a language, and those are ASCII in every language that has keywords —
so above `U+0080` all sixty-seven are the same Unicode letters. Cut there, **three** distinct
upper halves remain among the sixty-seven, and one of them covers sixty-four.

So each set is emitted as two fields and searched as two: `c < 128 ? below : above`, the
same parity rule on whichever half. The half that is enormous is written three times, and
the half that differs is a handful of characters.

    816,461 -> 312,933 characters, 62% smaller

The line count barely moves — 9,078 to 9,088 — because a set was always one very long line;
what shrank is what is on them. And reading got *faster* rather than merely no slower, 1.01x
to 1.13x over the corpus, which is half a megabyte of static data no longer competing for
cache with everything else.

The same shape is in the character parser's `Recognize_DotGram_Set`, where `ExpressionLanguage`
has five of them rather than sixty-seven — a few percent rather than two thirds, and not yet
done.

## The set test is a bit, and the bits are printed

The binary search is gone. A set is two halves, and each is read as one bit:

    (c < 128
        ? (Scan_Low1[c >> 6] & (1UL << (c & 63))) != 0
        : (Scan_High0[c >> 3] & (1 << (c & 7))) != 0)

Below ASCII a set is 128 bits, which is two numbers — `Scan_Low1` is
`{ 0x03FF000000000000UL, 0x07B7AFFE87B7AFFEUL }`, the digits and the letters this state does
not branch on. Above it a set is eight kilobytes, which is every script of the plane, and it
is printed as it stands rather than searched or rebuilt: a `ReadOnlySpan<byte>` over a byte
literal is data in the assembly, so nothing is allocated and nothing runs at type load.

**It was worth measuring twice, because the first measurement asked the wrong parser.** A
membership test looked like a tenth of a percent when timed through `ExpressionLanguage`,
where a parse is eight to sixteen microseconds and mostly builds expression trees. Through
the SQL lexer, where a parse is two to six hundred nanoseconds, a bare non-Latin identifier
cost half as much again:

    shape           latin   cyrillic     before    after
    one name         207n       261n      1.26x    1.02x
    three names      655n       874n      1.33x    1.05x
    long name        361n       566n      1.57x    1.00x

The long name — fifty-two Cyrillic letters — went from 566 nanoseconds to 279. The penalty
for not writing in Latin is now nothing at all, and the ASCII corpus is 0.97x to 1.12x,
which is to say unchanged to better.

**One thing had to be written out rather than called.** Behind a method taking a
`ReadOnlySpan<byte>`, every ASCII character paid for materializing the span of the half it
was never going to read — a fifth of the time on an input that is all keywords, where the
answer is always on the first line. As an expression at the call site it costs nothing,
because the branch that touches the span is the branch not taken.

The file grows from 310,106 characters to 401,442 — three eight-kilobyte halves printed
instead of three sets of bounds, and the test written out at sixty-seven call sites instead
of called. Against the 816,461 it was this morning that is still half.

## The table was working for eighty-eight states out of five hundred

A question about whether the bitmap's bytes were really all letters turned into finding that
the transition table was mostly not being used. Its rows are placed by a heuristic, and the
heuristic was wrong three times over.

**Counted in ranges.** A row is 128 characters wide and the window is chosen to hold the most
of what a state admits — counted, at first, in *ranges*. That put 440 of `SqlStandard92`'s
528 rows at `U+0B25`, entirely above ASCII: a window there holds more separate pieces of
`\p{L}` than the whole of ASCII holds, and every ASCII character in those states missed the
table and took the chain. A range is an artefact of how a set is written; a way out is a
decision the machine makes, and that is what is worth counting.

**Slid by characters.** Counting ways out fixed 440 rows and left the windows anchored at
`'+'`, because a window from 43 holds forty-three more Latin-1 letters than a window from 0.
It also puts the space outside, and the space is what ends every token — so each token paid a
chain call to find out it had finished, and `a = 1` got three times slower. Sliding must
preserve ways out, not characters.

**Scored by how many ways rather than which.** That left 95 rows at `U+00F8`. They are the
states whose only way out is "any identifier character": ASCII holds sixty-three of those and
a window in Latin-1 holds a hundred and twenty-eight, so the count of characters chose
Latin-1, where nobody types. Two windows admitting the *same ways* are the same answer and the
lower is the better of two same answers — what the higher holds extra belongs to a way the row
already answers for, and the chain answers for it exactly as well. Where the ways genuinely
differ, the characters still decide, which is what keeps a Cyrillic grammar's row on its
letters rather than on the `'='` beside them.

All 528 rows sit at `U+0000` now, and what that was worth against the chain the day began
with:

    chain     now
      203     193   1.05x  a = 1
      400     275   1.45x  salary BETWEEN 1000 AND 2000
      453     306   1.48x  x IN (1, 2, 3) AND y IS NOT NULL
      907     601   1.51x  amount * 1.05 + tax >= total AND …
      837     493   1.70x  warehouse.zip_code = 'X' AND …
      876     583   1.50x  CAST(x AS INTEGER) = 5 OR SUBSTRING(…)
     1457     976   1.49x  (quantity + weight) * rate - zone / 2 > …
       22      14   1.57x  ! amount * 1.05 + tax >= total AND …

Non-Latin identifiers cost 1.03x, 1.04x and 0.92x of their Latin twins, which is to say
nothing. The earlier "1.02x to 1.20x" was a table answering for eighty-eight states.

**And what the question was actually about.** `Scan_High0` is 5,924 bytes of `0xFF` out of
8,192, and they are letters: `U+4E00..U+A48C` is 22,157 unbroken ideographs, `U+AC00..U+D7A3`
is 11,172 Hangul syllables, `U+3400..U+4DBF` is 6,592 more ideographs. Of the 48,921
characters the set holds, Unicode 15.1 declines to call eight of them letters, and those
eight are the ones added in 16.0.

## Where ASCII cannot arrive there is nothing to choose between

Every row now begins at `U+0000` and is 128 wide, so the table answers for the whole of
ASCII and a character reaching the chain is above it by construction. Which makes the lower
half of every set test dead: the state cannot be asked about a character the table already
answered for. So the test is one line and no branch —

    if ((Scan_High1[c >> 3] & (1 << (c & 7))) != 0) return 126;

— and the sixty-seven `Scan_Low` fields are gone with it. It is emitted per state and not
by rule: a state whose row sits above ASCII keeps both halves, because there ASCII really
can arrive.

**And the storage was wrong, which only measuring found.** The halves were
`static ReadOnlySpan<byte> X => new byte[] { … }`, which the compiler puts in the assembly's
data — no allocation, nothing at type load, and the right answer on paper. At 457 call sites
it is the wrong one: materializing the span per use made a short non-Latin identifier 1.66x
its Latin twin, where the branchy version had been 1.03x. A plain `static readonly byte[]`
costs three eight-kilobyte allocations once and puts it back to 1.04x.

Against the chain the day began with, and against the branchy version:

    chain  branch    none
      200     196     196   1.02x  a = 1
      393     275     279   1.41x  salary BETWEEN 1000 AND 2000
      840     489     492   1.71x  warehouse.zip_code = 'X' AND …
     1454     986     980   1.48x  (quantity + weight) * rate - zone / 2 > …
       23      14      14   1.64x  ! amount * 1.05 + tax >= total AND …

Removing the branch bought nothing — it was perfectly predicted, being always false. What
it bought is the file: 580,503 characters to 547,119, and sixty-seven fields nothing read.

## A bitmap only where eight kilobytes are paid for

A bitmap costs eight kilobytes whatever it holds, and one of `SqlStandard92`'s three held
360 characters and was read from a single place. A grammar naming many small classes would
put one in the assembly for each of them, which is a data segment growing with the grammar
rather than with the alphabet.

So it is spent by weight: above sixty-four ranges a set gets a bitmap, below it the parity
search over bounds. There is no continuum to cut in the middle of — a Unicode category is
four hundred ranges and a class somebody wrote out is a dozen — so the number only has to
fall between them. `SqlStandard92` keeps two bitmaps and one 144-byte bounds array where it
had three bitmaps, and the file goes from 547,029 characters to 531,106.

**What the three were**, since it is the clearest picture of what a lexer's wide sets
actually are:

    Scan_High0   48,921 characters   \p{L}              2 sites    identifier start
    Scan_High1   49,281 characters   \p{L} | \p{Nd}   457 sites    identifier continuation
    Scan_High2      360 characters   \p{Nd}             1 site     inside a number

The 360 are every decimal digit outside ASCII — Arabic-Indic, Devanagari, Thai and the rest.
That is the one that is bounds now.

**And against the runtime's own predicates**, which was the other question:

    input             bounds     bitmap   IsLetter    IsDigit     either
    ASCII letters      9.19n      2.13n      2.28n      2.02n      2.23n
    ASCII marks        4.97n      1.51n      1.69n      1.31n      1.32n
    Cyrillic           5.26n      1.12n      2.81n      2.82n      2.81n
    CJK                5.05n      1.11n      2.80n      2.82n      2.85n

Level on ASCII, where `char.IsLetter` is one arithmetic test and the bitmap is one load, and
two and a half times faster above it, where the runtime walks three levels of page table and
decodes a category and the bitmap still does one load. Which is what a table answering one
question beats a table answering every question by.

Not that speed is why the emitted code cannot call them. `\p{L}` is expanded from the
generator's Unicode tables at generation time, and `char.IsLetter` reads the consumer's — the
same assembly would take `U+A7CB` in an identifier on one runtime and refuse it on another.

## Forty-four questions, and the answer to forty-three was already known

The chain for `SqlStandard92`'s first state was every mark the language writes and both cases
of every letter a keyword begins with. None of it could run. The state's row covers
`[0,127]`, so a character reaching the chain is above ASCII by construction, and forty-three
of those forty-four tests ask about characters that were answered before the call.

A state's tests are clipped to the outside of its own window now, and what clips to nothing
is not written. The case that was forty-five lines is two:

    case 0:
        if ((Scan_High0[c >> 3] & (1 << (c & 7))) != 0) return 27;
        return -1;

And then what is left is shared. There is one question left in most states — "is this more
of the identifier I am reading" — and hundreds of trie states ask it and go to the same
place, so **480 case labels stand over 11 bodies**. `Scan_Part0` goes from 221 lines to 91.

**Which also ended the division into methods.** Six of them existed because one method was
26,637 bytes of IL and the runtime said `Tier-0 switched MinOpts`. There is nothing left to
divide: every state fits in one method, the six-way `state < 96 ? … : …` chain is gone from
the hot loop, and `Scan` drops from 214 bytes of IL to 121 — still `Tier1 with Dynamic PGO`,
still no `MinOpts` anywhere.

    531,106 -> 468,130 characters

Speed is unchanged, 1.00x to 1.05x, which is what removing unreachable code should do.

**On making the chain a `switch (c)` instead**, which is what prompted this: there is nothing
left to switch on. The dense run of `c == '('`, `c == ')'`, `c == '*'` was exactly the part
the row had already answered, and what survives clipping is one test against a Unicode
bitmap — a switch of no cases and a `default`. The table in front is the jump table, and it
is a direct load rather than an indirect branch.

## What a switch would cost, and how far the table can grow

Two questions about the table, both answerable by measuring.

**A `switch (c)` instead of the row.** The compiler turns a dense one into a jump table, and
a jump table is an *indirect branch*. The target changes with every character, so nothing
predicts it, and every miss is a pipeline. The row is a data dependency instead — a load
from a hot line, which the machine carries on around. Over `SqlStandard92`'s first state,
forty-four ways out:

    input               table     switch      chain
    letters            2.02n      8.05n      7.43n
    marks              2.07n      6.62n      1.87n
    digits             1.12n      1.29n      1.49n
    as SQL runs        1.12n      6.22n      3.25n
    refused            1.51n      1.90n      8.05n

Five and a half times, on the mixture that looks like SQL. And it is worst exactly where a
switch was supposed to help: the more keywords a grammar has, the more different letters the
first state admits, the more places the jump goes and the less any of it predicts. The table
does not care how many ways there are.

**And what the loop actually waits on**, which is the same answer from the other side. There
are four branches per character and all of them predict: the input is not finished, the
character is inside the row, the transition is not a refusal, the state does not accept.
What costs is the chain of loads — the state's row, then the cell, then the *next* state's
row — each address known only once the last has arrived. Pointer chasing, which no
prefetcher helps with, because a prefetcher can guess a stride and not a value.

Which is why compacting the table is not free. Characters that lead to the same place from
every state are one column, and `SqlStandard92` has 47 of them where it has 128 characters —
so a class map turns 50,560 cells into 18,565, 101 kilobytes into 37. It also makes the
chain three loads instead of two, and that measured five percent:

     direct  classed
        191      200   0.95x  a = 1
        303      370   0.82x  x IN (1, 2, 3) AND y IS NOT NULL
        980     1030   0.95x  (quantity + weight) * rate - zone / 2 > …

**So the size question decides it.** A lexical machine has about five and a half states per
keyword and the table is states by row width, so it grows linearly with the language:

    words  states   rows  classes    direct  classed  state x atom
       25     163    135       32       34K      34K         279K
      100     618    515       32      129K     129K        1056K
      200    1229   1026       32      256K      64K        2100K
      800    4506   3706       32      926K     232K        7701K

The classes do not grow. They are bounded by what the machine can tell apart, which is
thirty-odd for anything written in Latin letters and never more than 128 — so compacting is
worth a quarter of the table however large the grammar. Under 256 kilobytes the direct table
is kept and the five percent with it; over that, a grammar of some six hundred words, it is
compacted. The last column is the dense state-by-atom table this design rejected in its first
week: 7.7 megabytes where the row table is 232 kilobytes.

## The scannerless parsers are not one lexeme, and the measurement says why

A lexical machine is a DFA and a table falls out of it. The proposal was that a scannerless
grammar is just one big lexeme and should have the same table, which for `Rfc3986` — a URL
parser with ten call sites in 25,771 lines — looked very likely.

**Statically it is.** Of its 978 states, 770 do nothing a transition table could not say:
they read a character and jump. Only 79 write the arena.

    parser                states   only read and jump   write the arena
    Rfc3986                  978           770  (79%)         79  (8%)
    SqlStandard92            276           179  (65%)         83 (30%)
    ExpressionLanguage       696           268  (39%)        421 (60%)

**Dynamically it is not.** Counting steps rather than states, a corpus of eight URLs spends
37% of them in those states — and, decisively, they do not come in runs:

    parser              steps   pure   runs   mean  median  longest   in runs of 4+
    Rfc3986             1,478    37%    394    1.4       1        9              8%
    SqlStandard92         642    50%    121    2.7       1       13             60%
    ExpressionLanguage  1,734    28%    209    2.3       2       13             44%

A table pays for itself by being stayed in. The lexer's inner loop reads a token — a dozen
characters, a dozen table steps, no exit. A URL parser's pure states last **1.4 steps** on
average and a single step at the median: the loop would spend more on entering and leaving
than the chain of comparisons costs. Eight percent of its pure steps are in runs of four or
more.

Which is the answer to why a scannerless grammar is not one lexeme. A lexeme is regular
*and produces nothing*: the lexer writes no arena, so its states are pure by construction and
its runs are as long as a token. A parser records where every capture began, and in a URL
grammar a capture begins every few characters — so the pure states are real but scattered one
at a time between the writes, and there is no run to put a loop around.

**Where the time actually is.** Sixty per cent of `ExpressionLanguage`'s steps and half of
`Rfc3986`'s go to arena traffic, not to deciding which character was read. That is the target
the measurement points at, and it is a different piece of work from this one.

## And the same question asked properly: cut out the regular parts and scan them

The entry above measured the wrong thing. It asked how long the runs of pure states are *in
the machine as it stands*, found them 1.4 states long, and concluded there was nothing to put
a loop around. But what breaks those runs is not the captures — it is the machine's own
per-turn bookkeeping. A repetition writes down where each turn began, so a `Pchar*` scanning
a path alternates: test a character, record the turn, test a character, record the turn. 129
of `Rfc3986`'s states (13%) do nothing else.

Compiling the fragment as a lexeme removes that record entirely, which is exactly what the
question was. The right experiment is to try it, and DotGram already has the mechanism: a rule
written in braces is atomic, and an atomic rule that keeps no records compiles to a scanner —
one run of a machine with nothing written down. `Rfc3986` has no braces anywhere, which is
why it had no scanners.

A copy of it with braces round the twenty-two regular rules:

    url                                            plain   braced
    http://example.com/                             442n     188n   2.35x
    https://a.example/very/long/path/…              361n     248n   1.46x
    https://user:pass@www.example.co.uk:8443/…      331n     265n   1.25x
    http://[2001:db8::7]/c=GB?objectClass?one       979n     819n   1.20x
    //relative/reference?only                       239n     203n   1.18x
    mailto:someone@example.com                      146n     138n   1.06x

and the two agree on every input, accepted and refused. Thirty scanners, 25,771 lines down to
20,907, and 1,358 reads of `text[p]` down to 1,139.

**What stands between this and doing it automatically** is that braces are *possessive*.
`{ A }` does not give input back, and a rule that would have needed to is a rule the automatic
version would silently change the meaning of. It happened to be safe for all twenty-two here,
which is a fact about RFC 3986 and not a licence.

The compiler can prove it in the cases that matter: making a fragment possessive changes
nothing when what follows it cannot begin with what it consumes, which is `FollowSets` and
`Determinism.Distinguishable` — the same pair that already warns about an ambiguous repetition
(`GRAM5002`). So the rule would be: a fragment becomes a scanner when it is `Scannable`, keeps
no records, **and** giving input back could never have helped. The first two are written; the
third is the work.

## The proof is not the follow set, and the differential test said so in seconds

The plan was: a rule becomes a scanner when it is `Scannable`, keeps no records, and giving
input back could never have helped — the last being the follow set handed to `Scannable`
where braces hand it nothing.

It admits a great deal. `Rfc3986` goes from no scanners to **44**, and from 25,771 lines to
19,037; `SqlStandard92` to fifteen, `ExpressionLanguage` to eight. More than the twenty-two
that were braced by hand.

And it is wrong. `ReferenceDifferentialTests` — random grammars, random inputs, the engine
against the reference semantics — found a disagreement on the first seed it tried:

    trivia = [' ']*
    Start = (?! ['b'..'c' | 'x'] & (R1 | R1) & R2) & ({ ['b'..'c' | 'x'] } | { ['a'..'b'] })
    R1 = "c"i
    R2 = 'b'
    parse Start

    input " cba": the semantics say it matches, the engine says it does not.

Excluding published rules — a scanner answers where it stopped and a publication has to say
what it expected and whether the whole input was read — was the obvious first guess and did
not fix it. At that point the next move would have been a third guess, so the branch was
reverted instead.

**What the failure is worth knowing.** The follow set says what characters may come after a
rule. `Scannable`'s `after` says what still has to match *inside the group being committed*.
They are not the same question, and handing one to the other is a category error that happens
to be right most of the time — which is the worst kind, and exactly what a differential test
is for. The right condition has to be about the *call site*: a rule may commit where every
path that reaches it can live with the longest match, and that is a property of the graph
around it rather than of the characters after it.

What is not in doubt is the payoff. Braced by hand, the same twenty-two rules made `Rfc3986`
1.06x to 2.35x faster and agreed on every input. The mechanism works; the licence to apply it
without being asked is what is missing.

## Which of the two was wrong, and it was the engine

The entry above stopped at "the differential test refuses it" and offered a diagnostic
instead. That was ducking the question: an engine that disagrees with the semantics is wrong
somewhere, and finding out where is the work. So the counterexample was run rather than
abandoned.

    trivia = [' ']*
    Start = (?! ['b'..'c' | 'x'] & (R1 | R1) & R2) & ({ ['b'..'c' | 'x'] } | { ['a'..'b'] })
    R1 = "c"i
    R2 = 'b'
    parse Start

`Start`'s body, as the normalizer leaves it, is

    ?!['b'..'c' | 'x'] & trivia & R1 & { (none | none) } & trivia & R2 & trivia & ( … )

and the rules turned into scanners are `trivia`, `R1`, `R2`.

**The semantics is right and the engine was wrong.** On `" cba"` the reading that works has
the *leading* seam match nothing at all: `Start` begins at zero, `?!['b'..'c'|'x']` passes on
the space, and then the woven `trivia` inside eats it, `R1` takes `c`, `R2` takes `b`, the
group takes `a`, and the input ends. A possessive `trivia` cannot do that — it eats the space
at the leading seam and cannot give it back, so `Start` is forced to begin at `c` and the
lookahead refuses.

**And the reason is exact.** `FollowSets.Of` computes what may follow a rule from its call
sites *in the graph*. A publication weaves the seam around what it publishes, and that is not
a call site the graph records — so the follow set of `trivia` has never heard of the one place
where giving the space back is the whole parse. Excluding the seam makes that grammar agree,
and the whole differential suite with it.

**A second boundary, found the same way.** Over token kinds a scanner breaks the provenance:
a split grammar cuts its values out of the extents of the tokens it ran over, and a scanner
swallows tokens without recording any. `RereadTests` crashed in the materializer, which is the
right way to find out.

**And one that is not a boundary but a debt.** A scanner writes nothing down, so when it
refuses, the caller can only report the failure where the rule *began* — position 0 where the
machine used to say 3. That is a documented promise of this parser broken quietly, and it is
not fixed by excluding anything: it needs the scanner to carry how far it got. Returning `p`
at its `Refuse:` label is not that, because the scan restores `p` on its own backtracking; it
needs a furthest-reached local, which costs the hot path something and wants measuring.

So the state of it: the condition is sound where it is allowed to run — the differential suite
agrees on every seed with the seam and the split excluded — and it turns `Rfc3986` from no
scanners into forty-four, 25,771 lines into 19,037, for the 1.06x to 2.35x that hand-written
braces measured. What it still costs is the refusal position, and that is the piece to build
before any of this is turned on.

## The furthest a scanner came, and why it is three pieces and not one

Building it turned the debt into a shape, and the shape is worth writing down because the
next attempt should start from here rather than from the beginning.

**The scanner can carry it, and the successful path pays nothing.** A `furthest` local
starting at `pos`, raised wherever the scan gives input back to itself, and a refusal that
comes back as `-1 - furthest` — one return saying both that it refused and where it reached,
with `< 0` still meaning the first. The caller sets `p` from it before recording the failure.
Everything on the path that succeeds is untouched.

**Which was not enough, and the counterexample says why.** `"abqy"` and `"abcdefx"` both begin
`ab`; on `abqzzz` the message must name the one that got to the fourth character. But a
literal does not give input back — it refuses outright — so `furthest` never moves, and the
failure is reported where the literal began. The offset is right there at the emit site and
threading it in works, for the branch that walks a literal character by character. The branch
that compares the whole run with one `SequenceEqual` has no offset to thread, and that is the
branch a seven-character literal takes. Making it walk instead would be trading the scan's
speed for the message's precision, which is a decision and wants measuring.

**And a third piece, found by the split tests.** Over token kinds a refusal position is a
token index mapped back through the extents, so a scanner's `furthest` would have to be mapped
too. Excluding kinds — which this needed anyway, for provenance — leaves that alone.

**Two other things the build taught, both real defects rather than obstacles.**

A scanner's caller skips the failure check when the rule is nullable, on the reasoning that
something matching the empty string cannot fail. That is the wrong question: `?= X` matches
the empty string when it succeeds and refuses when it does not, and reading its refusal as a
position made a parse succeed on input the grammar refuses. The predicate wanted is
*infallible* — a repetition of none, a sequence of those, a choice with one — and nullability
is not it. `ReferenceDifferentialTests` found it on the first seed.

And a local emitted at a backtrack site must be declared wherever it is written, not only
where the label that reads it exists. Obvious once the build says so, in eight grammars at
once.

So the state: the condition is sound, the scanner can report where it reached, and what is
left is the literal run's bulk compare and the split path's mapping. `Rfc3986` is 44 scanners
and 19,037 lines against 25,771 whenever those two are done.

## Nullable is not the same question as infallible

Two things were called defects in the entry above and only one of them was. The local
declared where its label is rather than where it is written was my own code, half-built and
not compiling; the emitter has always declared its scanner locals by whether the body writes
them. That claim is withdrawn.

The other is real, and reachable without any of the scanner work. A rule in braces compiles
to a scanner, and the caller skips asking whether the scanner refused when the rule is
*nullable* — reasoning that something matching the empty string cannot fail:

    Ahead = { ?= 'a' }
    Start = Ahead & ['a'..'z']

`?= 'a'` matches the empty string when the lookahead passes and refuses when it does not. So
the caller writes `p = Scan_Ahead(text, p);` with no check, `p` becomes -1 on `"b"`, and what
happens next is whatever the rest of the rule does with a negative position. Here it is the
right answer by accident: the character test after it fails on a position out of bounds. In
the grammar that turned this up it was the wrong one — a parse accepting input the grammar
refuses.

The question wanted is whether the rule can refuse, not whether it can be empty: a repetition
of none, a sequence of those, a choice with one among them. `Infallible` asks that, and it is
conservative in the safe direction — a false answer costs a comparison the parse did not need,
and a true one has to be true.

Nothing in the repository's own grammars changes, snapshots included, because their scanners
really are infallible — `trivia` is a repetition of none, which is the case this was written
for and the only one it had ever been asked about.

## The scanner is asked for rather than written, and Rfc3986 never had to know

The three pieces the last entry listed are built, and two more turned up in the building.

**A rule becomes a scanner when the compiler can prove what braces assert.** Braces say
"commit the first reading"; the proof is that committing loses nothing, which is
`FollowSets` handed to `Scannable` where braces hand it nothing. The set is the union over
every call site, so a rule reached from two places is judged against both.

**Where it does not look, and why:**

- *The seam.* A publication weaves trivia around what it publishes, and that is not a call
  site the graph records — so the follow set has never heard of the one place where giving
  the spaces back is the whole parse. This was the counterexample two entries ago.
- *Kinds.* What a scanner is worth is swallowing a run of input in one call; over kinds a
  step is a whole token and there are no runs, so all that is left is the call. Measured:
  `SqlStandard92` took **twice as long**. It is excluded, and is unchanged now — 0.97x to
  1.03x, which is noise.
- *A body that spells itself.* A scanner is one call, so its refusal can only name the rule.
  `Expected B.` where the same grammar compiled in place says `Expected "abqy".` is a loss
  for a literal — and a gain for everything else, since `Expected RegName.` beats a hundred
  character ranges. So literals and choices of them keep the inline path.

**The refusal position, which was the debt.** A `furthest` local from `pos`, raised wherever
the scan gives input back, and a refusal returning `-1 - furthest` — one value saying both
that it refused and where it reached. Three things it needed:

- A literal compared in one `SequenceEqual` has no offset to report, so the run is walked —
  on the path that was going to refuse anyway, which costs the path that matches nothing.
- Only where the refusal is the scan's own answer. A literal failing into a loop's exit has
  refused nothing, and the seam of a spaced grammar ends that way at every operand; computing
  a reach there would have been pure cost on the hottest thing in the compiler.
- **A lookahead's advance is not distance covered.** It looked and put the position back.
  Counting it had `eof` — which is `?!any` — report a refusal one character past where the
  input failed to end, and that made the split parser and the character parser disagree about
  the same grammar. `ProvenanceTests` is what says they must not.

**What it is worth**, on a grammar with no braces anywhere and no changes to it:

    before   after
       206     185   1.11x  http://example.com/
       243     205   1.19x  urn:isbn:0451450523
       307     238   1.29x  https://example.com/%D0%BF%D1%83%D1%82%D1%8C/…
       937     765   1.22x  http://[2001:db8::7]/c=GB?objectClass?one
       338     248   1.36x  https://a.example/very/long/path/with/many/…
       335     284   1.18x  http://example.com/ has a space

Forty-four scanners where there were none, and 25,771 lines down to 19,277. The snapshots
move with it: `Url` from 7,121 lines to 4,656, `Feed` from 3,147 to 3,045.

## TypeName was a lexeme, and that was the mistake

`ExpressionLanguage` had one rule stopping it from being read as tokens:

    namespace Lexical { TypeName = Word & ('.' & Word)* }

A dotted name lexed whole, deliberately without braces so that it could hand its own tail
back: `Math.PI` is read as `Math.PI`, the guard on `NamedType` asks whether that names a type,
the answer is no, and the lexeme gives up `.PI` for member access to find. It works, and it
works **only over characters** — a tokenizer decides where a token ends once, and `Math.PI`
arriving whole is a member access that can never be read. That is why 11 of the parser's tests
refused when it was asked to split.

It is read here now, one word at a time, with the dots between them:

    NamePart : @string = w: Word => @(w)

    NamedType : @Type
        = head: Word & ('.' & part: NamePart)* & args: (…)?
          & when @(args != null || Resolves(Dotted(head, part)))

Same language and the same means — the repetition hands a turn back where the lexeme handed a
suffix back — and the comment that argued for the lexeme is answered rather than ignored:
`System . Text` was going to be captured with its spaces in it, so the *parts* are captured
and joined, and what the author put between them is nowhere in the name. `NamePart` exists
for exactly that: a bare `part: Word` under a repetition captures the run from the first to
the last, dots and spaces and all, where a typed part is an array of words.

**And with it the parser splits.** 25,843 lines become 22,856, and 127 refusals become **nine**
— all of them one thing, which the first of them names precisely:

    'V' is not a member of type 'System.Int32[]'

There is no `V` in `(int[] a) => a.Length`. It is kind number 86 read as a character: three
materialization sites still cut their text out of the machine's own input rather than out of
the extents of the tokens it ran over — the piece a sequence member copies, and the two the
fold of a left-recursive rule uses. `SqlStandard92` never found them because it builds no
values, and `Postfix` is the rule that does: left recursive, folded, and its member captured
as text.

Routing them through `Cut` is the next piece and is not this one — the first attempt at it
broke the character path, which is what `UrlTests` is for.

## And ExpressionLanguage reads tokens

Four sites still cut their text out of the machine's own input rather than out of the extents
of the tokens it ran over: the piece a sequence member copies into its buffer, the two a
left-recursive fold uses, and the one the flat path uses where there is no arena at all. Over
characters a position is where the text is; over kinds it indexes a token, and the text is
somewhere else. `SqlStandard92` never found them because it builds no values.

The piece is the one that needed care rather than a substitution. Over characters it is a
span copied straight out of what is being read, and that must stay — so it is the copy that
changes alphabet, not the expression around it. The first attempt made both paths go through
a string and broke `UrlTests`, which is what `UrlTests` is for.

**Then one more thing in the grammar, and it is the same mistake as `TypeName`:**

    | "new" & type: Type & '[' & ']' & '{' … '}'

`Type` above names `"[]"` as a literal, so a lexer takes the longest match and two marks
written apart here are one token by the time this rule sees them. The same thing said two
ways, and the second way cannot be read as tokens. One spelling now.

With those, `ExpressionLanguage` reads token kinds: **25,843 lines become 22,840**, and the
suite is green — the largest grammar in this repository, the one with the state, the guards,
the precedence climbing, the recovery and the values, on the same path SQL has been on since
this morning. What it took was two corrections to the grammar and four to provenance, and no
change at all to what the language accepts.

    input                                   before     after
    (int x) => x                            2.359us   1.912us   1.23x
    (int x) => x * x - 1                    4.062us   3.632us   1.12x
    (string s) => s.Length                  3.120us   2.532us   1.23x
    (int x) => { x += 1; x *= 2; return x }  8.356us   7.177us   1.16x
    (int x) => Math.Max(x, 1)               9.348us   8.053us   1.16x
    two levels of parenthesis               7.378us   4.461us   1.65x
    four levels                            12.239us   7.347us   1.67x
    six levels                             17.113us  10.393us   1.65x
    two levels, refused                    10.565us   9.045us   1.17x
    six levels, refused                    40.705us  36.777us   1.11x

Between 1.11x and 1.67x, and the nests gain most, which is where the character machine was
doing the most re-reading. Allocation moves both ways and is worth its own look: four inputs
allocate a fifth less and four a quarter more, and `x * x - 1` most of all — the token buffers
are pooled, so what grew is elsewhere.

## The states are numbered in the order they are written

Compilation reserves a state whenever it needs somewhere to come back to and numbers them as
it goes, so the numbers are dealt out before anything is known about which of them survive.
Layout then follows the signposts, merges what says the same thing twice and drops whatever
nothing can reach — three states in five — and what is left is what a sieve leaves.
`Rfc3986` wrote 532 states numbered up to 1304; `ExpressionLanguage` 648 up to 1441;
`SqlStandard92` 276 up to 857. Thirty to forty per cent dense, and the holes are not in one
place: they are wherever a rule was compiled into a caller and its own copy went unread.

The dispatch pays for that. It is a `switch` over the numbers something can resume at, and
over a set with those holes in it the C# compiler cannot lay one jump table: it bisects
instead, and where the table is written in parts it bisects a second time inside each of
them. Numbered in written order the same set is contiguous, each part is a run of it, and
both switches become what a run of consecutive labels compiles to.

It is a renaming and nothing else. Every state number in the file is written by `Settle`
from a mark holding the state it was compiled as — that was already true of every jump and
every arena entry, which is what `Machine.Graph.cs` exists to guarantee — so a map from
state to written number, applied where the mark is settled, moves all of them at once.
`Renumber` builds it from `_order` once the layout is decided, and `Rewrite` says every body
again.

**Two things were writing a state number without a mark**, and one of them was a defect
waiting for exactly this. Recovery finds the extent of a broken element by walking the arena
back for two entries it wrote itself, and it named them by their compiled numbers:

    if (candidate.Kind == ParserEntry.PendingRecovery && candidate.State == 19)
    if (!recoveryBoundary && candidate.Kind == ParserEntry.Choice   && candidate.State == 17)

while the entries themselves were written through `Resuming`, which resolves. The two agreed
only because nothing had ever moved a written state's number, and a recovered element came
back with an empty extent the moment something did. Twelve tests said so at once. The other
was the trace, which named the state a call was about to enter by its unresolved number — a
trace that cannot be lined up against a label is a trace that says nothing about the machine
it is tracing.

Both are marks now, and the trace one paid for itself twice over. Two bodies that differ
only by a number naming the same state are the same body, and `Merge` could not see it while
the number was the unresolved one. **`Rfc3986` fell from 19,277 lines to 11,635** — five
copies of the same `IPv6` block collapsed into one — and with them it dropped under the
budget and is written in one method again rather than twelve.

    grammar               before    after
    Rfc3986              19,277    11,635    532 states up to 1304 -> 283 up to 411
    SqlStandard92         9,930     9,856    276 states up to  857 -> 270 up to 440
    ExpressionLanguage   22,840    22,840    648 states up to 1441 -> 648 up to 974

What it is worth, measured interleaved, best of nine, one variant per process:

    ExpressionLanguage                          before      after
    (int x) => x                             429,973/s   432,426/s    +0.6%
    (int x) => x * x - 1                     238,506/s   223,466/s    -6.3%
    (string s) => s.Length                   258,186/s   269,546/s    +4.4%
    (int x) => Math.Max(x, 1)                 66,133/s    65,120/s    -1.5%
    two levels of parenthesis                 77,386/s    85,493/s   +10.5%
    four levels                               37,248/s    41,653/s   +11.8%
    six levels                                21,493/s    25,120/s   +16.9%
    six levels, refused                       82,826/s    84,586/s    +2.1%

`SqlStandard92` is flat within two per cent on every input, and `Rfc3986` half a per cent to
two. The nests are where it pays, which is where the dispatch is reached most; `x * x - 1`
is slower by six per cent on three separate runs and is the one number here that is not
explained.

### And the range chain, which was the point of the exercise, is a loss

A part is now a run, so "which part" can be asked as two comparisons rather than as a jump
table over every state there is, and bisected it is four comparisons for a machine in
twenty-three parts. That is the shape the generated lexer's transition table was chosen over
a `switch` for, where the direct load beat the indirect jump by 5.5x. Written out and
measured, it costs **five to eight per cent on every `SqlStandard92` input but the two
shortest**, and on `ExpressionLanguage` it is a wash — up four per cent in the middle of a
spread from minus seven to plus twelve.

The lexer's answer does not carry, and the reason is worth keeping. There the next state
varies with the character, so the indirect branch is unpredictable and the load is the
cheaper of two bad options. Here the parse returns to a handful of the same states over and
over — a rule returns, a choice resumes — which is the case a branch predictor has no
trouble with at all, and four comparisons that each mispredict cannot beat one indirect
branch that does not. Reverted; the numbering stays, and it is the numbering that let the
compiler lay the jump table in the first place.

## The step profile, taken again on the token path

`docs/next.md`'s "a scannerless grammar is just one big lexeme" entry counted steps on the
character machine and ended with a sentence pointing somewhere else: sixty per cent of
`ExpressionLanguage`'s steps and half of `Rfc3986`'s go to arena traffic, not to deciding
which character was read. Both parsers that could move have moved since — `SqlStandard92`
and `ExpressionLanguage` read token kinds now — so the question was worth asking again
before deciding whether the syntactic machine can be a transition table too.

Counted the way it was counted before: a marker in every state body, a corpus run once, the
sequence of states written out and read back. Eleven search conditions and eleven lambdas,
each set graded by depth and ending in refusals. A state is **pure** where its body is tests
and jumps and nothing else, and **arena** where it touches `entries`.

    parser                     states   read and jump   write the arena
    SqlStandard92 (kinds)         438       261 (60%)        147 (34%)
    ExpressionLanguage (kinds)    972       237 (24%)        731 (75%)

    parser               steps   pure   arena   runs   mean  median  longest  in runs of 4+
    SqlStandard92        2,315    34%     61%    489    1.6       1        7           13%
    ExpressionLanguage   6,545    19%     80%  1,142    1.1       1        3            0%

**The token path did not reduce arena traffic. It removed everything around it.**
`ExpressionLanguage` went from 60% of its steps in the arena on characters to 80% on kinds,
and the absolute count fell — which is the split working exactly as it was meant to, and
also the reason the remaining share is what it is. What a transition table would replace is
the other fifth, and it does not come in runs: the longest run of pure states anywhere in
the whole corpus is **three**, and not one pure step of `ExpressionLanguage`'s is in a run of
four or more. A table pays for itself by being stayed in, and there is nothing here to stay
in — the same answer the character machine gave, arrived at from further away and more
sharply.

Both figures above are of states classified by what their bodies can do, which is how the
earlier entry counted and so what it can be held against. Counted as writes actually made,
`SqlStandard92` writes the arena 0.30 times per step and `ExpressionLanguage` 0.51 — the
same shape, and the honest denominator for what follows.

**And then the arena, counted wrong the first time.** The first breakdown here counted
*steps in states whose body names an entry kind*, and read `Choice` as a third of every
parse in both machines. It is not: most of those names sit in a branch the visit does not
take — `if (repeating.Value >= 1) entries.Add(Choice…)` is entered on every turn and writes
on almost none. Counting the writes themselves, by putting a counter in front of each
`entries.Add`, says something else entirely:

    SqlStandard92 — 702 writes over 2,315 steps    ExpressionLanguage — 3,315 over 6,545
      Call              478   68.1%                  Call             1,115   33.6%
      LoopExit          130   18.5%                  RuleCapture        941   28.4%
      Repeat             54    7.7%                  Construct          746   22.5%
      Choice             38    5.4%                  Choice             233    7.0%
      TurnDone            2    0.3%                  Capture            151    4.6%
                                                     Repeat              41    1.2%
                                                     LoopExit            34    1.0%
                                                     CaptureOpen         52    1.6%

**`Choice` is five to seven per cent, and it is not waste either.** Of the 38 SQL writes 27
are resumed and of `ExpressionLanguage`'s 233, 191 — 71% and 82%. The way back that ordered
choice writes is a way back the parse actually takes; there is no speculation to remove and
no cheaper place to put a record that is going to be read.

So `CompileCheckpointChoice` is not the lever. Extending it to the engine means solving
three things at once — the interleaving of a stack in locals with a stack in the arena, the
four indices a resume restores beyond the position, and the truncation that unwinds captures
made since the site opened — for at most seven per cent of the writes and five per cent of
`SqlStandard92`'s. Not now. The mechanism keeps its place on the flat path, where there is
no arena to interleave with and it removes the *only* records there are.

**What the honest count points at is calls, and in `ExpressionLanguage` the value.**
Two thirds of `SqlStandard92`'s arena traffic is `Call`, and it builds nothing at all: a
`Call` entry there is pushed with `RuleIndex` −1 and popped again on return, and what it
carried in between was a return state and five indices for an unwinding that mostly never
happens. In `ExpressionLanguage` calls are a third and `RuleCapture` and `Construct` another
half between them — that is the value being recorded, which is what the arena is for and not
something to remove.

Which makes the two questions worth asking next, and neither is about `Choice`:

  * how many calls does `CanInline` decline that it could take — the threshold is a size
    budget and the file has just lost 40% of `Rfc3986` and nothing of the others;
  * whether a call that no open choice sits under needs an entry at all, or only a return
    state, which is a much narrower record than a `ParserEntry`.

## And a state most of them are reached from one place

Asked while the profile was being read: if a state is jumped to from exactly one place, why
is it a state at all rather than the next lines of the one that jumps? Counted over the
labels in the two token parsers, and the answer is that most of them are:

    parser              labelled   in-degree 1   2    3    4   5+
    SqlStandard92            270      114 (42%) 134    8    9    5
    ExpressionLanguage       579      387 (67%) 153   29    5    5

What that does *not* buy is a jump. The layout already threads the states into chains and
drops the trailing jump wherever the state it names is the one written next — which is why
168 of `SqlStandard92`'s 438 states and 324 of `ExpressionLanguage`'s 972 carry no label at
all. A state that still has a label with one thing naming it is one the chain could not
reach that way: its namer's jump is inside an `if` rather than at its end. Splicing the body
into that branch turns a taken jump into a fall-through, which is a thing the JIT's own
block layout is already trying to do.

What it does buy is size, and size is the constraint that forced `Budget`, `Part` and
`PartSize` in the first place. Each splice removes a label, a brace pair, a blank line and a
`goto` — call it four lines, times 387, on a file of 22,840. And fewer states means fewer
parts, and every part boundary that disappears takes a set of departures with it, each of
which is a real call. That is the part of it worth measuring.

Against that: `Merge` collapses states whose bodies are the same text, and it has just been
shown to be worth 40% of `Rfc3986`. Splicing makes bodies longer and more distinct, so it
works against exactly that. Which way the file moves is not something to reason out.

## What actually declines a call, and it is not the size threshold

`ExecutionPlan.CompiledInPlace` decides once per rule whether its body is written where it
is called, and the entry that added `Copied = 64` is about size: the reserved-word list of
standard SQL is 285 nodes and came to 59% of the generated file. So the first guess at the
call traffic was that the threshold is too tight. Asked of the three parsers, rule by rule,
it is not the threshold at all — and the answer is different for each:

    SqlStandard92        70 rules, 45 in place    23 recursive, 2 large
    ExpressionLanguage   86 rules, 14 in place    72 declared a type
    Rfc3986              36 rules, 29 in place     6 declared a type, 1 large

**`SqlStandard92` is blocked by recursion, and the rules blocked are tiny** — `Factor` 5
nodes, `Term` 8, `BooleanPrimary` 8, `ValueExpression` 10, `SearchCondition` 11. Every rung
of the expression ladder is a handful of nodes and every one of them is on the cycle that
closes at `'(' ValueExpression ')'`, so `graph.Recursive` refuses them all. Raising `Copied`
would admit nothing.

The obvious repair is to ask the question per call site instead of per rule — inline unless
the callee is already on the expansion path, so the ladder unrolls until the cycle really
closes. Estimated over the graph before writing any of it, that is **2,150x**: 6,044 nodes
compiled today become 13 million, because the ladder is mutually recursive at a dozen points
and the expansion multiplies rather than adds. `SearchCondition` alone goes from 43 nodes to
2.3 million. Closed.

**`ExpressionLanguage` is not blocked by recursion or size at all.** Seventy-two of its
eighty-six rules declare a type, and a rule that builds a value is never compiled in place —
which is right, because the value needs a boundary to be built at. Path-sensitive inlining
there is 1.0x: it changes nothing, because nothing it would admit was refused for a reason
inlining can address.

**But the ladder is paying three entries a rung to hand a value through unchanged.** Every
rung is written the way precedence ladders are:

    BitOr : @Expression = left: BitOr & '|' & ?!'|' & right: BitXor => @(Expression.Or(left, right))
                        | x: BitXor                                => @(x)

and the second alternative is an identity: capture the operand, build the value from it, and
that value *is* the operand's. Compiled, it is a `Call` frame, a `RuleCapture` of the
callee's result, a `Construct` naming a factory whose body is `(x)`, and a return. Three
arena writes and a frame to pass a reference upward.

The counters say how much of the parse that is. One identity factory is shared by
**twenty-five sites** — `Additive`, `And`, `Assignment`, `BitAnd`, `BitOr`, `BitXor`,
`Core`, `Equality`, `Multiplicative`, `Or`, `Postfix`, `Primary`, `Relational`, `Shift`,
`Type`, `Unary` and the rest — and it runs **451 times of 746 constructs**, 60%. With the
`RuleCapture` beside each, that is a little over nine hundred of the 3,315 arena writes:
**27% of everything the arena is asked to record, to say that a value is itself.**

So the piece worth doing is not inlining. It is recognizing the identity: an alternative
whose construction is exactly its one captured call, of the same type, builds nothing — the
callee's value is the rule's value, and neither the capture nor the construction needs a
record. Whether the `Call` frame can go with them is a second question and a harder one: the
alternative is the whole of the rule on that path, so it is a tail call, and the frame is
also where the failure unwinding stops.

Nothing here helps `SqlStandard92`, which builds no values and whose calls are the ladder
itself. That one still wants either a narrower record than a `ParserEntry` for a call
nothing can fail back past, or `<<` on the ladder — which is a change to the transcription
and not to the engine.

## Built: a publication read by methods, and the tape that keeps it exact

The hand-written recursive descent measured at the end of the last entry was the ceiling,
and the question was what stood between the automaton and it. Answered by building the
other thing: `Machine.Direct.cs` renders a publication as one C# method per rule, with a
call for a call, a local for a mark, and the arena nowhere in it. `SqlStandard92` is the
first parser through it, because it builds no values and that is the whole of what this
first rendering leaves out.

**What replaces the arena is a tape of the ways back still open.** A choice the proofs
cannot settle records one entry — the alternative in force and the last there is — and an
unsettled repetition records one per turn past its minimum, the option of having stopped
before it. Every construct is a segment: on failure it asks the tape for the latest way
opened since it began, takes it, drops what was decided after it, and runs itself again
from its own mark, replaying the tape up to the way it changed. Nothing is resumed in the
middle and nothing outside the construct moves; what is re-executed is only the construct
that failed. The order is the automaton's — innermost way first, latest turn first — and
`ReferenceDifferentialTests` agrees with the semantics on every seed.

**Four things the differential suite found before any grammar did**, each a rule of the
tape worth keeping:

- a retry may only look at what stands *before the cursor*. During a replay the tape past
  the cursor is the future, decisions of what comes after waiting to be read again, and a
  construct that fails on the way there exactly as it did the first time must leave it
  alone. Scanning to the end made `(a > 1)` loop for ever with a tape eight entries long;
- moving a choice on to its next alternative drops what the spent one decided, so the
  next one starts from nothing and a later replay of it reads its own decisions;
- an atomic group over a body that cannot fail still seals what was decided inside it: a
  loop that took its turns may not give one back, and `{ (' ' | …)* }` was giving back
  a space to a rule that could take it;
- `c` is only what stands at `p` until something reads another character — a look behind,
  a lookahead's body, a failed alternative — and every place a construct runs again or
  moves on reads it again.

**What it costs, and where the rest is.** Over the same inputs, interleaved with the
automaton and the hand-written parser:

    input                                automaton   methods   by hand
    a = 1                                    186 ns     78 ns     27 ns
    (a + b) * c > d                          725        223        88
    ((((a + 1) * 2) - 3) / 4) + b > 0      2,734      1,057       164
    x = 1 AND y IS NOT NULL                  378        168        91
    a0 = 1 AND … (64 terms)               12,075      5,489     2,543
    a0 + a1 + … (64 terms)                 4,139      1,616       749
    (a + b) * c >   refused                1,755        467       133

Two to three times the automaton, three times short of the hand. What remains is counted:
eleven ways opened for `a = 1`, one per level of the ladder whose alternatives genuinely
overlap — `ValueExpression | NULL | DEFAULT` where `NULL` is also a value — and a refusal
recorded at every door the hand-written parser walks past in silence. Three things bought
the rest of the way from the first measurement, which was slower than the automaton:
§5's filter before each alternative, so an operand no longer walks eight alternatives and
sixteen more inside one; the stack check on the back edges of the call graph rather than
in every recursive rule, one per level of nesting instead of a dozen per operand; and the
door of a settled loop kept quiet, as the engine's is.

**What it hands back to the engine**: values, guards, marks, recovery, streaming, climbing
and `find`. `CanDirect` refuses rather than guesses, `Direct = false` on the attribute or
the options keeps the automaton for every publication, and the engine's own tests say so
to compile against it. An input nested past the thread's stack is read again on a thread
with a deep one, the input copied once for the crossing.

## Built: values through the methods, on a log beside the tape

The first rendering left values to the engine, and that left every parser that builds
anything on the automaton. This one records them, as §7.2 requires — nothing is built
while matching, and a failure takes back what it recorded — but into a log of records
rather than an arena of entries (`Machine.Direct.Values.cs`).

**A record is what one rule matched, written when it ends.** Five words — length, rule,
factory, start, end — and then the members: a text capture is its two positions, a
captured rule is the index of that rule's record, a repeated capture is a count and the
indexes, gathered from a side stack that the turns pushed on. Post-order, because a rule
ends after everything it captured. The log and the side stack are two more counts on the
tape, put back on every path that gives a reading up: a segment that runs again from its
mark first drops what its first run recorded, exactly as it drops the ways decided after
it. A capture local made inside the reading given up goes back to nothing for the same
reason.

**Only what the root reaches is built.** A valued rule that matched without being captured
— an alternative tried and passed over, a rule read for its extent — is in the log too,
and its factory must not run. So the materializer marks first, from the last record
backwards: the root is live, and a live record's members are live. Then one pass forward
builds the live ones, each into a typed table of its own kind, so a member is read as
`values3[record]` and nothing is boxed. A terminal that builds — a lexical rule the lexer
measured — is not walked at all: its record is a span, and the character machine the
lexer already has builds the value from the text.

**Measured** on the JSON example, interleaved with the automaton, allocation to the byte
the same:

    input                       automaton    methods
    {"a": [1, true, null]}         413 ns     307 ns
    99 characters, nested        1,582         996
    200 objects, 14 KB         201,759     115,896

Less than the recognizers gained, and the reasons are counted below. Three things were
found on the way, each worth its line:

- **a run is one way back, not one per character.** `[' ']*` before a separator is not
  settled — the seam after the list can begin with a space too — and the first rendering
  opened a way per turn and asked at every door, which for JSON meant a `List<string[]>`
  of tied expectations allocated on every successful parse. The engine has always written
  one `Run` entry for a repetition of one character test; now the methods scan the run,
  open one way whose value is how many characters were handed back, and keep the door
  quiet, as the engine's is. The dead-mark pass then had to learn that a mark a run
  measures its length against is read, `p = m;` or not; and `p - m + 1` is not
  `p - (m + 1)`, which the differential suite said within a seed;
- **a lambda that captures a parameter costs at the entry of the method.** The deep-stack
  fallback captured `pos`, so the closure's display class was allocated on every call, for
  a `catch` that runs once in a lifetime. Sixty-four bytes on every parse, found by
  counting what `"1"` allocated. The lambda now captures locals of the `catch` block only;
- the differential suite reports the grammar when a generated parser throws, not just
  when it answers wrongly.

**What it hands back to the engine still**: folds, guards, marks, a context, externals
with values, captured lookaheads, and `find` — which between them keep ExpressionLanguage
on the automaton. Those are next, folds and guards first.

## Built: folds, guards, marks and a context on the direct path — ExpressionLanguage read by methods

What kept ExpressionLanguage on the automaton was four things the first two renderings
handed back, and each turned out to have a natural place in the log.

**A fold is its loop, and its records lead with the value so far.** §4.3 already rewrote
a left-recursive rule into `base & (step)*`, so the readers had been reading folds all
along; what they did not do was record them. Now the base writes an ordinary record and
keeps its index in a local, and each step's record puts that index first and holds the
step's own captures — those its factory was written against, each as the one thing the
step captured, which is how the step's factory takes them. The base's record keeps the
rule's own shapes, sequences included, because that is how the base's factory takes
them. Two mistakes on the way, both found by a test and not by thought: a step's record
held the base's members as `-1`, which the materializer read as records; and then it held
every step's captures, not this step's — `x is Type` handing `Type` to the step for `<`.

**A guard runs where it stands, with what the rule holds so far.** A text capture is cut
from the locals that hold it. A captured rule's value is built now, from the records
already in the log, by the same materializer told a root and a place to start — the
rule's own log mark, since nothing before it reaches them — and it stays built: a flag
per record says so, the final walk skips what a guard built, and a watermark on the tape
says how far the flags still stand for the records under them. Every place the log is put
back lowers the watermark, in the renderings that build anything and nowhere else. A
rule with a guard or a mark is never written in place, because both need the rule's own
start.

**A mark is a record of its own.** `with state` writes one where it opens and one where
it closes, so it goes wherever the log goes: an abandoned reading takes its marks with it
by being put back, which is the whole of what §7.8 asks. The walk keeps a stack of the
marks standing open, and a factory that names `parserState` is handed it as a span.
The stack's depth is a local of the walk. It was a field beside the array, and Windows
Defender's AMSI heuristics called the compiled parser `Trojan:MSIL/AgentTesla.MVR!MTB`
for the pair — one test failing under every antivirus signature update, bisected down
to those two fields. A local it is.

**A context is a parameter**, of the entry and, where a guard names it or builds a value,
of every reader. **A rule too large for one method is split by alternative**: `Primary`
of an expression language, thirty-five alternatives each building its own value, was
2,300 blocks with everything called, so each alternative becomes a method of its own
and the choice keeps the dispatch. Sound where every reading of an alternative ends by
writing a record, which the shared-head shape the factoring pass leaves also does.

**Measured**, ExpressionLanguage on the token path, the automaton against the methods:

    input                              automaton     methods
    (int x) => x + x                    1,949 ns     1,143 ns
    8 terms                             6,801        3,422
    128 terms                         222,138       39,187
    Math.Max(x, 1) once                 8,348        4,420
    Math.Max(x, 1), 32 terms        1,257,733      155,418

Linear where the automaton was not: 300 ns a term at every size. What remains on the
read side is a few hundred bytes a parse the automaton does not allocate — a guard cuts
`head` and `part` to ask whether a dotted name resolves, and the walk at the end cuts
them again where the automaton keeps what a guard built for text too.

**What still hands back to the engine**: a captured lookahead, an external with a value,
`find`, and a guard that names a capture repeated inside a loop or asks for the input.

## Where SQL lost its factor of five: the ways back that a token could have settled

The hand-written SQL parser was measured seven times faster than the methods on
`((((a + 1) * 2) - 3) / 4) + b > 0` and under three times on everything flat. Counted
rather than guessed — one counter per reader entered, one per way opened — the nested
input entered `Factor` seventy-four times for its six operands. Each level of parentheses
opened a way back, and taking one re-executes everything after it, so the levels
multiplied. Three things were behind it, two of them the grammar's and one the proofs'.

**A subquery that matched any balanced parentheses.** The stub stood for a query and
accepted anything in brackets, so `'(' & ValueExpression & ')'` and `ScalarSubquery`
matched the same text and every parenthesized operand kept a way to the other. Now the
stub opens with one of the three words a query begins with, and the proof of an
exclusive choice looks past a literal the alternatives share: `'(' & Expression` against
`'(' & "SELECT"` part ways at the second token, where each must read something and the
two cannot read the same thing. 1,142 ns to 652.

**A row read to its end before a single value was tried.** `RowValueConstructor` listed
the row of several first, so `(a + 1) * 2` was read as the first element of a row,
refused at the missing comma, and read again as a value — once per level. A row of
several needs the comma the single form cannot read, so the two never match the same
text and either order accepts the same language; the single value now stands first.
652 ns to 227, and the flat inputs a third faster with it.

**A negative lookahead the first sets ignored.** Over kinds a word is one kind whether
it is reserved or not — `case` is both an identifier and `CASE` to the lexer, and the
syntax tells them apart by `?!Reserved`. First sets read past a lookahead, so an
identifier began with every keyword and a primary's eight alternatives overlapped
pairwise; the way back that bought was opened at every operand. A negative lookahead
whose every reading is exactly one character — a keyword over kinds, a literal, a
class — now subtracts what it refuses from what follows it. Sound over characters too:
`?!"CASE"` over characters is four of them and subtracts nothing. With the functions
reserved as the standard reserves them, the choice of a primary is decided by its first
token, and the ways opened on the 64-term input went from 640 to 256.

**And a table that stopped at ASCII.** The narrowed sets were full of holes — every
reserved kind punched out of the identifiers — and a set of that many ranges was
rendered as a binary search in a call, which cost more than the way it replaced. The
class tables now reach 255, which is where the kinds of a token path live.

**Measured**, alternating with the hand-written parser on the same run:

    input                          before     after     by hand
    a = 1                            76 ns     68 ns      26 ns
    (a + b) * c > d                 134       115         74
    ((((a + 1) * 2) - 3) / 4) …   1,142       201        129
    x = 1 AND y IS NOT NULL         143       130         67
    a0 = 1 AND … (64 terms)       4,693     4,075      2,164
    a0 + a1 + … (64 terms)        1,372     1,190        641
    (a + b) * c >   refused         360       252         96

What is left on `a = 1` is fixed cost: thirteen readers entered for two operands, a
refusal at each door the hand-written parser walks past, and the lexer's thirteen
nanoseconds. The next factor is in the calls, not in the ways.

## A ladder written in place, under a budget the JIT counts

`a = 1` entered thirteen readers for its two operands: the boolean ladder down to the
predicate, the row, the element, the value ladder down to the primary. Each is a frame,
a prologue and a return around a body that is often one loop, and none of them keeps a
value. The plan writes a rule in place only where the engine could — small, valueless,
and not on a cycle, because the engine would need a frame for the cycle — and the
ladders are cycles by definition.

A method has no such need. A rule already being written in place above the point
being written is called instead, which breaks every cycle at its first re-entry, and
that is the whole of what soundness asks. What it asks in return is a budget: the first
attempt counted grammar nodes and gave the entry reader 2,700 basic blocks, past the
2,000 the JIT stops optimizing at, and `a = 1` took three times as long. The budget is
now in the JIT's units — a body is written into a buffer, measured with `Branches`,
kept if the method has room and thrown away if not — with a floor per rule, measured
once with everything under it called, so a body that cannot fit is not written at every
site only to be discarded. What a discarded rendering learned about the method is
forgotten with it, or `c` is declared for a use that was thrown away, which is the
warning the differential suite found.

**And only small rules.** Written without a size limit it made ExpressionLanguage a
quarter slower: `Keyword`, forty alternatives and valueless, was copied into every
reader that asks whether a word is one, and a large body gains nothing by losing its
call and costs the method it lands in its registers. A level of a ladder is under a
hundred branches; the limit sits there.

Measured against the build before, alternating on the same run:

    input                          before     after
    a = 1                            78 ns     61 ns
    (a + b) * c > d                 134       112
    ((((a + 1) * 2) - 3) / 4) …     233       193
    x = 1 AND y IS NOT NULL         148       122
    a0 = 1 AND … (64 terms)       4,700     3,800
    (a + b) * c >   refused         360       241

ExpressionLanguage and JSON are where they were. The lexer emitter's tests have a race
under the parallel runner — `A_kind_names_every_pattern_that_matched` failed once in a
full run and passes alone, as `ProvenanceTests` did earlier with "collection was
modified" — which is the emitter's static state and not this rendering's.

## Built: `~`, and the question a lexer must not be asked

The lexer was worth seventeen times on SQL and the last entry recommended keeping it. The
question that came back was the right one: how does a lexer, knowing no grammar, tell the
`>>` that closes two type argument lists from the `>>` that shifts?

It does not, and it must not be asked. What decides is where the thing stands: in
`List<List<int>>` a type argument list has to close, in `a >> b` a binary operator has to
go. The parser knows which and the lexer never will. Every compiler that reads both
resolves this in the parser — javac and Roslyn lex `>>` and split it back, and C++ moved
the rule into the grammar in C++11, which is why `list<list<int> >` needed its space
before then and not after.

**The ambiguity was ours, and the lexer made it.** Measured on one grammar in both modes,
before anything was built:

    input                       over characters   over kinds
    a >> b                            OK             OK
    list<int>                         OK             OK
    list<list<int>>                   OK             FAIL
    list<list<int> >                  OK             OK

`ExpressionLanguage` failed on `o is List<List<int>>` and passed with a space, which is to
say it was a pre-C++11 C#. The cause is maximal munch: with `">>"` in the terminal
inventory the scanner takes both characters, and the inner argument list — which wants one
`>` — is handed a shift. A token cannot be half spent, so no order of alternatives recovers
from it. The decision was made before the parser was asked.

**So `>>` stops being a token.** `'>' ~ '>'` is the shift, `~` says the two stand with
nothing between them, and the lexer is not consulted about anything. Nothing was needed
from it: a token already records where it began and how long it is, because a capture has
to be cut from the original text, so the gap between two tokens is one subtraction that
was already there. Emitting trivia as tokens would have been the expensive answer — it
roughly doubles the stream and makes every rule step past whitespace itself, which is what
§4.5 exists to spare the author.

**And it is not a lexer patch.** Over characters §4.5 weaves trivia between operands, so
`'>' & '>'` accepts `> >` there too, which the same measurement showed. The gap was in the
notation, not in the scanner: there was no way to say "here, nothing may intervene". So
`~` is one operator with one meaning in both halves of a split grammar — the seam withheld
where positions are characters, the same statement asked of the token positions where they
are kinds — and it binds tighter than `&`, so `a & b ~ c` is `a & (b ~ c)`.

One node carries it, zero-width like the look-behind §4.6 weaves, and the emitter renders
it as nothing over characters. Two mistakes on the way, both caught by the same
measurement run again:

- the first patch put the new case beside `Node.Behind` in `LexicalSplit`, which is a
  *rewrite* and not a walk, so crossing into the kinds half replaced the glue with
  nothing — erasing it in the one place it does any work;
- and beside `Node.Behind` in the lexical automaton, which *refuses* what it does not
  understand, so `~` inside a pattern was reported as a look-behind. Inside a pattern
  there is no woven seam to withhold, so it is accepted and worth nothing.

`ExpressionLanguage` reads `o is List<List<int>>` now, shifts by `>>`, and refuses
`a > > b` exactly as C# refuses it. The glued shift costs nothing measurable: a shift is
two tokens instead of one and the gap is two additions, and every figure of the last entry
stands. The notation's self-description learned the operator too — `GramGrammar` parses
`~` at the same precedence, and `SelfHostingTests` holds the two implementations to it,
because a notation whose own grammar cannot read it is a notation with two meanings.

**What this does not solve**, and the reason to keep it in view. `~` fixes maximal munch
and only that. A regular expression against a division in JavaScript, a heredoc, JSX: there
the content of the token differs, and the scanner has to be told what is expected. The seam
for it exists in shape — an external recognizer is already a rule that reads the input
itself — but on the token path it is handed the kinds, so re-reading a span as characters
would need a new external form. And `a<b>c` in C++, where the answer is whether `a` is a
template, is not a lexical question at all: that is what `when` and `context` are, and
`ExpressionLanguage` already resolves a dotted name against real reflection while it reads.

## Built: a way back opened at the alternative taken, and only where one could be taken

Two things settled before this one was touched, both in `benchmarks/` and both said at
length in its README. The yardstick is in the repository now — `HandSqlTokens.cs`, a
lexer into kinds and precedence climbing over them, held to the generated parser's
language over forty-two shapes before anything is timed — and it is the best hand-written
version this session could produce, which is the only kind worth dividing by: it beats
the first day's parser on six inputs of seven and loses only where an input has no keyword
at all, so a pass that sorts words into kinds is work with nothing to show for it. That
one input is why the first day's parser keeps a row. Against the yardstick the generated
SQL recognizer is 1.2 to 4 times behind, and the lexer is not where: subtracting the
hand-written lexer from both leaves two to six times on the reader alone.

**The way back was opened at the top of every unsettled choice**, at alternative zero,
and then walked past every alternative the gates refused, one `Next` each — a hundred and
six of them written into the SQL parser, and every one an array write and two field
stores on the path that takes the first alternative it was ever going to take. And it was
opened whether or not anything could ever take it: a choice whose entered alternative no
later alternative overlaps by first token has nothing to come back for once that
alternative has matched, because nothing else could match where it did.

**Now the way is opened on entering the alternative taken, in force at it**, reaching to
the last alternative that overlaps it, and not at all where none does. The gates run as a
chain — the first that passes is entered — and the alternative entered opens the way, or
reads it back on a replay, only if it is one a later alternative overlaps. An alternative
no later one overlaps records nothing: when it fails the next is tried in place, and a
replay runs it again to the same failure, reading back the spent ways it left on the tape,
which is what the exclusive fast path already relied on and what lets the two paths
become one.

One hole, found by the suite rather than by thought. A way moved past a failed
alternative kept the reach of the alternative that opened it; that reach was an argument
about what could match *where the opener matched*, and the opener had not matched. A
`0XFF` was refused at the `X`, and `SqlReadOnly` ran out of memory replaying a refusal
whose tape it could no longer read, because the switch that sends a replay to the
alternative in force knew only the values the reach allowed. So a way moved on takes the
reach of the alternative it moves to — spent, where nothing overlaps that one — and the
switch admits every later alternative. Both are the tape's own invariant said in full:
the value is the alternative in force, the reach is how far a mend could go from it.

The SQL parser writes fifty `Next` now against a hundred and six, and the measurement
moved three to six percent, not more — `a = 1` from 68 to 66 ns, the nested input from
196 to 188, the sixty-four predicates from 3,988 to 3,896. Not more because most ways on
the hot path are genuine: `(` opens a predicate and a parenthesized condition alike, an
identifier begins a column reference and a value function alike, and the grammar, as
written, decides those by trying the first and coming back. The hand-written parser does
not come back; it reads a value expression after `(` and then looks at what follows.
That is the rest of the distance, and it is not in the tape.

## Built: the alternatives of a choice share a frame

A reader's locals were numbered once through, in the order the constructs were written:
every segment its `s`, `lm` and `rr`, every call its `q`, every mark, turn and way its
own. A choice of eight alternatives, each a sequence of segments with calls inside, laid
all eight out side by side in one frame — `Read_ValueFunction` declared 377 of them — and
the JIT zeroes the frame on entry and keeps every one of them addressable, which is what
a method with 400 locals costs before it reads a character.

No two alternatives of one choice run in the same parse. So each now numbers from where
the choice stood, and what follows the choice numbers from the widest alternative rather
than from the sum: the frame holds one alternative's locals and the rest are the same
slots under other names. Labels are the exception, deliberately — a label is unique to
the method, and two alternatives with the same `again` would be a jump into the wrong
one. The parts a rule over budget is cut into are untouched: each is a method with a
writer of its own.

`Read_ValueFunction` declares 177 locals now against 377, and the widest reader in the SQL
parser is 181. Measured, the sixty-four predicates went from 3,896 to 3,565 ns and the sixty-four
operands from 1,361 to 1,259 — eight percent on inputs that enter the wide readers many
times — and the short inputs stayed within the noise. Two snapshots renumbered, nothing
else in them moved.

## Built: no arena where nothing runs on the engine

Every generated file carried the engine's runtime — `Parser`, `ParserArena`,
`ParserEntry`, the pooling hooks over them — whether or not a machine in it ran on the
engine, on the argument that a host might have implemented the hooks and what compiled
yesterday must compile today. Twenty of the thirty-one parsers the solution generates
have no engine in them, and each was two hundred and some lines of a class nothing
reaches, compiled on every build of the consumer for nothing.

The runtime is written now only where a machine runs on the engine — the valuing
machine over the characters included, which is the one part of `ExpressionLanguage`
still there, so that file keeps its arena and `SqlStandard92` and `Rfc3986` lose theirs.
The hooks go with the class, and that is the decision rather than an accident: a host
that had filled them in over a file that has since gone direct rented a parser nothing
rents. `SqlStandard92.cs` was such a host, with a thread-static one-slot pool written on
the day the engine was what ran; the pool is gone and the class is empty. Four benchmark
grammars had the same pool and are the same now, and `CallCost` loses its two pooling
rows — pooled against a fresh `Parser` per call — because there is no `Parser` in that
file to pool. The README keeps the table they made and says what it was.

Two tests said "falls back to the shared engine" of grammars that do not lower, and
asserted the arena to prove it. A grammar that does not lower goes to the methods now,
so what they assert is that the flat path was not taken — the arena or the tape of ways
back, either of which holds what a flat method's locals cannot — and they are named for
that. The emitter test that used `Parser` as its raw-literal block at depth two uses
`Ways` instead, which is a raw literal too.

Nothing measured moves: the class was never entered. The SQL file is 35,197 lines against
37,364 before the last two entries — the shared frame took the declarations, this took
the class — and every test the suite has stands.

## Built: over kinds a rule's answer stands

The way back was never the cost. Three entries above took the tape apart — opened at
the alternative taken, shared across a choice, the dead arena gone — and won fifteen
percent, and `{ }` around every choice the hand-written parser commits at made the SQL
recognizer *slower*: minus the ways, plus a segment and a `Seal` per group, seven of
them per predicate. Then the whole emitter was switched to commit by default for an
afternoon, every choice and every turn, and the SQL numbers did not move either, with
`Open` at zero. What the tape costs is not the entries. It is the scaffolding a reader
keeps so that it *could* be sent back: a mark, a segment, a log watermark and a side
stack watermark per sequence, per alternative, per group, and a retry on every failure
path. As long as the language says a caller may resume a choice inside a callee, every
reader carries that, whether or not anything in the parse ever comes back.

**So the language says something else now, over kinds.** In the syntactic half of a
split grammar a rule's answer stands: a choice is decided by the token in front of it
and never revisited, and nothing that fails after a rule has matched sends the parse
back into it. That is what a parser written by hand does at every choice — sees the
token, enters, never looks back — and it is the default because it is what nearly every
rule over tokens wants. A rule that needs to give back says so on its name, `Name? = …`,
and gives back inside itself: the greedy dotted name in `ExpressionLanguage` hands a
part back at a time until reflection says the type resolves, and once it has answered,
the answer stands at its boundary. Over characters nothing changed — every rule gives
back, `?` is accepted and means what it already is — because that is where backtracking
is understood and wanted: a regular expression, a feed, an RFC transcribed in the order
its ABNF lists the alternatives.

The experiment said where the old semantics was actually used, over kinds, in this
repository: five tests of `ExpressionLanguage`, two shapes. A loop giving back its
last turn to the caller's continuation — `Name ('.' Name)*` before `'.' Member`, and
`Type` reading `int[]` before a `"[]"` written after it — and a branch read as the
statement `0;` when the declaration around the `if` needed the `;`. The first is what
`?` is for and marks one rule. The other two are the caller's to read, and were
rewritten the way one writes them by hand: `new int[] { … }` reads the array type and
asks the guard whether it is one, and `if` in value position has branches that are
values, so the `;` is the declaration's. `SqlStandard92` needed nothing: its choices
were already ordered the way a committed parser reads them, which is why the commented
"read it again per level of parentheses" in that grammar was ever written.

**What the emitter does with it.** A reader over kinds whose rule is not marked writes
no tape: no way back, no segment, no retry on a failure path, no seal, and an atomic
group is its body. A failure is the position put back and the log put back, which is
what values still need. A `?` reader is the reader of before, and seals its ways when
it answers rather than dropping them, so that a replay of the rule reads the same
decisions in the same places; a rule that commits stays committed where it is written
in place inside a `?` reader, and a `?` rule is never written in place inside a reader
that commits. The core of a publication commits with the half it reads.

The SQL recognizer: 41 ns on `a = 1` against 63 the entry before, 2,568 against 3,561
on the sixty-four predicates, 938 against 1,266 on the sixty-four operands — and the
last is the first input on which the generated parser is *ahead* of the hand-written
one, at 0.79 of its time. Against the yardstick the recognizer stands at 1.2 to 2.2
times, from 2 to 4. What is left on the short inputs is the ladder — ten readers for
`a = 1` where the hand-written parser climbs three — and that is the grammar's shape.

**A gap, named.** The statement is about the language and the readers honour it; a
syntactic half that cannot be read by methods — `find`, `stream`, `recover`, a climb —
still runs on the engine, which backtracks as it always did. Neither split grammar in
the repository has such a rule, and the day one does is the day the engine learns to
commit or the compiler learns to say no.

## Measured: the ladder is not the cost, and collapsing it is a loss

The plan after the tape was the ladder. Reading `a = 1`, the SQL recognizer walks
thirteen rules from a search condition down to a column reference and walks them twice,
once for each operand, where the hand-written parser climbs three levels of binding
power. Two to one on the totals, and five to one on the reader alone once the lexer is
subtracted from both. The obvious next move was to collapse a chain of `X = X op Y | Y`
rules into one climbing loop, the way the hand-written parser is written.

**Climbing had to reach the methods first**, since the direct rendering refused a rule
of binding powers outright and refusing it put the whole grammar on the engine. A
climbing reader now takes the strength it is read at, an alternative below that strength
jumps to the choice's next without recording a refusal, and a call carries what `<<` or
`>>` recorded against it. Nothing else changed shape: the fold that left recursion
becomes is what the direct path already read. Three examples left the engine with it.

**Then the experiment, before building the detection that would make it automatic.**
`SqlStandard92`'s two ladders were rewritten by hand with binding powers — four boolean
levels into one rule of `OR`, `AND`, prefix `NOT` and the `IS` tail, three value levels
into one of `+ - ||`, `* /` and the sign. Same language, checked; the generated file
fell from 41,143 lines to 15,619. And it is slower:

| input | levels as rules | one climbing rule |
| --- | --: | --: |
| `a = 1` | 41 ns | 96 |
| `(a + b) * c > d` | 75 | 122 |
| 64 predicates joined by `AND` | 2,597 | 2,678 |
| 64 operands joined by `+` | 950 | 1,285 |

**Because the ladder was never thirteen calls.** The budget writes a rule in place
wherever a method has room, so twelve of the thirteen levels are already inside one
method, and what each contributes is its loop's test against a token already in a
register. A climbing rule is the opposite: it cannot be written in place, because its
alternatives are gated by a strength that is a parameter, and it cannot be split into
parts for the same reason — so every operand becomes a real call with a prologue, and
`a = 1` pays two of them where it paid none. Precedence climbing is what a person writes
because a person is not going to write thirteen methods; it is not what is fastest once
the thirteen are one method anyway.

So the ladder collapse is not worth building, and the entry stands as the reason not to
try it again.

**What the experiment did find is a bug, and only over kinds.** The lexical split
rewrites every node of the grammar into the kinds it will read, and it kept a map from
what each node was to what it became. That map was keyed by structure, deliberately, on
the argument that two nodes a grammar wrote the same way could not be told apart by the
dictionaries being remapped. They can: the normalizer keys `Powers`, `Recoveries` and a
fold's accumulators by the node object, so two calls to the same rule are two keys with
two values. Collapsed to one, every call to a climbing rule took the strength of
whichever call site was rewritten last — and `(a + b) * c` was read with the
parenthesized operand at the strength of the `*` around it, which refuses the `+`. It
had never shown because no split grammar had used binding powers until this experiment
did. Keyed by identity now, both there and in the remapped dictionaries, with a theory
that asks the same three shapes of both halves.

**And a hypothesis for next time, from the same numbers.** Fitting the two ends: the
difference on the sixty-four predicates is about 1.1 ns per token, and on `a = 1` it is
22 ns over three tokens. Nineteen of those twenty-two are fixed — paid once per parse
regardless of length — which is the whole of what the hand-written parser spends on that
input. That is where to look: the tape rented and returned, the values table rented
beside it, the try, the catch for a deep stack and the finally, and the failure struct.
Not the reader.

## Built: the gap over kinds says so out loud

The entry that made a rule's answer stand over kinds named a gap and left it: the
statement is about the language, the readers honour it, and a syntactic half the readers
cannot write falls back to the engine, which backtracks as it always did. Neither split
grammar in the repository has such a rule, so nothing was wrong — but a grammar that
reads one way and runs the other is the kind of thing that is discovered years later by
somebody debugging a parse.

`GRAM5005` is that grammar being told. Over kinds, a machine written by neither the flat
path nor the methods is a warning naming the rule and what about it was refused: a
recovery, a stream, a `find`, a captured lookahead, a guard handed what a reader cannot
hand it, or a rule called with arguments. The refusal was a bare `false` in seven places
and is a reason now, which is worth having on its own — "cannot be read by methods" is
not a thing an author can act on, and "`Row` recovers from a bad element" is.

A warning and not an error, because the parse is correct: it is ordered choice over
characters, which is what the engine implements and what the notation meant everywhere
until this week. What is not correct is the promise, and the promise is what the message
is about.
