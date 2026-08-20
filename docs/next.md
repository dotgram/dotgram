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

The checkout was clean before this handoff was written. The latest implementation commit
is `d074b2d` (`Capture negative lookahead success in the arena`).

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
813 tests; 812 pass. The one known failure is
`SemanticTests.A_stray_character_is_not_reported_seven_times`: it expects three
diagnostics but currently receives four (`GRAM1005`, `GRAM2001`, `GRAM2003`, and
`GRAM2002`). This failure predates the latest unified-machine steps and remains to be
diagnosed separately.

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

This is already the behavior of the unified generator, but the legacy `Machine` still
has implicit rule boundaries. Consequently the repository temporarily has two semantic
models. Do not update the public atomicity wording piecemeal while that split exists.
Known stale passages are in `docs/syntax.md` and `docs/status.md` and still describe rule
boundaries as atomic.

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

The default path creates a parser normally. A user may implement the hooks with a cache
or pool. The internal storage is not parameterized by grammar value types.

### One shared recognition arena

Recognition uses one `List<ParserEntry>` for the whole parse. `ParserEntry` is a readonly
internal all-integer record. It currently represents choices, calls, atomic boundaries,
repetitions, lookahead, captures, pending constructions, completed invocations, and rule
captures.

Successful value-producing calls are rewritten in place into `Completed` entries. A
completed invocation retains its rule id, matched extent, parent/frame links, and return
continuation. `RuleIndex` is stored separately: an earlier attempt to reuse the
continuation field broke EOF backtracking.

### Shared automaton and recursion

A supported parse publication is emitted as one automaton with shared label blocks.
Static transitions use direct `goto`; dynamic returns and backtracking use dispatchers.
Frequently called rules therefore remain shared instead of being expanded at every call
site, while simple grammar paths can still stay in one generated method. Recursive calls
are explicit arena frames and do not consume the C# stack.

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

Declared `@T[]` results and `Construction.Sequence` now use the unified path for eligible
non-streaming publications. Their generated factory counts scalar, optional, and sequence
members, allocates one exact `T[]`, and fills it in grammar order; it no longer builds a
typed `List<T>`. Materialization first marks the completed invocations reachable from the
accepted root, so construction code belonging only to an abandoned derivation is never
called.

Ordinary left-recursive folds now use the unified path. Recognition records each base or
step `Construct` in arena order. After acceptance, materialization builds the base once,
then walks the step markers from left to right and applies each factory to one accumulator.
Captures are read only from the arena segment belonging to that marker, so no per-fold
typed list is needed and a chain of 20,000 terms materializes iteratively. Binding-power
climbing remains separate because its recognition calls additionally carry a power.

Whole unified publications also compile the root rule's external leading and trailing
`Trivia`; EOF is still checked at `Accept`. This is distinct from the trivia already
inserted between sequence operands and fixes published folds beginning with whitespace.

Positive lookahead records the furthest seen position before restoring the input cursor.
Negative lookahead stores its capture slot in the frame and creates an empty successful
capture only on the inner-failure path. A capture around lookahead works; captures nested
inside lookahead are deliberately still excluded from unified generation.

### Diagnostics and tracing

Generated development checks use `Debug.Assert`. Detailed tracing is emitted through a
method marked `[Conditional("DOTGRAM_TRACE")]` and writes to `Debug.WriteLine`, so release
calls and argument evaluation disappear when the symbol is absent.

## What the unified machine supports now

The eligibility logic is in `UnifiedMachine.Supports`. The unified path handles:

- empty nodes, literals, and element predicates;
- sequences and choices;
- transparent rule calls and explicit atomic groups;
- repetitions and lookahead;
- external recognizers;
- capture-free guards;
- text/rule captures and supported construction;
- direct and mutual recursion through explicit frames.

It currently excludes:

- recovery;
- binding-power climbing;
- captures nested inside lookahead;
- guards in capture-bearing rules;
- unknown node forms;
- parse publications needing the streaming driver.

`find` is not unified. If a grammar has a `find` publication, `CSharpEmitter` also emits
legacy methods. Feed and URL snapshots therefore contain a transitional duplication:
unified parse code plus legacy find/stream code, which explains their size.

Do not broaden `Supports` casually. Previous broad attempts admitted explicit array types
and folds, then broke `TextReader`/chunk overloads, special sequence factories, or emitted
fold calls without their required scalar arguments.

## The unresolved guard problem

Capture-aware `when` needs an explicit language decision. Text captures can be available
as extents during recognition, but a typed captured rule result does not exist until its
deferred `=>` has run. The coherent options are:

1. restrict recognition-time guards to text/extents and other already available data;
2. materialize typed values early, weakening the deferred-construction guarantee and
   complicating rollback and side effects;
3. evaluate guards in a post-recognition candidate/derivation model, which is a larger
   architectural change.

Do not silently implement option 2. The semantics should be decided first and then stated
in the language reference.

## Recommended continuation order

1. Decide and implement capture-aware `when` semantics.
2. Port recovery without reintroducing implicit rule atomicity.
3. Move `find` and streaming onto shared automaton blocks and remove transitional duplicate
   generated engines.
4. Port binding-power climbing after ordinary folds, retaining power-aware calls without
   reintroducing C# recursion.
5. Remove the legacy semantic path, update public atomicity/compatibility documentation
   atomically, and add migration notes where observable behavior changed.
6. Only then benchmark generated size and hot paths and decide whether additional inlining
   or block-sharing heuristics are justified.

## Implementation map

- `src/DotGram/Grammar/Emit/UnifiedMachine.cs`: unified eligibility, arena model,
  automaton emission, backtracking, and materialization.
- `src/DotGram/Grammar/Emit/Support.cs`: generated support types including the nested
  parser infrastructure.
- `src/DotGram/Grammar/Emit/CSharpEmitter.cs`: publication routing and coexistence with
  legacy generation.
- `src/DotGram/Grammar/Emit/Machine.cs`: legacy generator and old implicit rule-boundary
  behavior.
- `src/DotGram/Grammar/Model/CaptureLayout.cs`: capture layout; recovery/fold structures remain
  here for historical reasons.
- `src/DotGram/Grammar/Model/RecognitionGraph.cs` and
  `src/DotGram/Grammar/Model/Retention.cs`: graph and retention/streaming analyses that
  constrain eligibility.
- `tests/DotGram.Tests/CSharpEmitterTests.cs`: generated-code shape and snapshot coverage.
- `tests/DotGram.Tests/SemanticTests.cs`: language semantics and the one known failing
  diagnostic-count test.
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

The restructuring is complete when every publication kind uses the same transparent-rule
semantics and explicit atomic groups, all user construction runs only for the accepted
derivation, recursive parsing/materialization remains iterative, legacy generated methods
are removable, public documentation describes one implementation rather than a semantic
split, and the full suite remains comfortably below the 30-second ceiling.
