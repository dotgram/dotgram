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
  14-24 seconds on the machines used so far; stop it at 30 seconds and investigate.
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
828 tests and all 828 pass. The stray-character recovery regression is fixed: a broken
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
`Trivia`; EOF is still checked at `Accept`. This is distinct from the trivia already
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

## Open: a malformed repetition count takes the generator down

`{.4}` where a repetition wants a count — one character of the URL grammar changed — ends
in an `IndexOutOfRangeException` out of the compiler. In a consumer's build that is
`GRAM0001` and nothing they can act on.

`GramParser.ParseCount` handles it properly as far as it goes: `.` is neither an integer
nor a parameter name, it reports `InvalidCount`, and it returns neither a number nor a
name. Something downstream then indexes with what it was handed. Two guesses at where were
both wrong — refusing to build the quantifier at all, before and then after the closing
brace is consumed — so the place is still unknown and the guesses are not in the tree.

Found by `FuzzTests` on its first run, from seed 3 at round 83. That seed is commented out
of the theory with this written beside it; putting it back is what shows the fix is one.

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
