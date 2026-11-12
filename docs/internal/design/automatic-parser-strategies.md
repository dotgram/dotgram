# Automatic parser strategies by grammar region

Status: analysis and implementation proposal, 2026-09-14. The measurements below describe the initial baseline. The first implementation is recorded under Implementation progress; the regional planner is not implemented.

## Objective

Choose an execution strategy at generation time for each useful region of a grammar. Small parsers must be able to use no reusable infrastructure. Large parsers must be able to combine direct recognition, local values, deferred values, and explicit state without forcing every region to pay for the most demanding one.

There is no input-independent fastest implementation. The initial objective is to remove structurally unnecessary work, then choose among semantically legal alternatives using a calibrated cost model. Do not introduce runtime benchmarking, adaptive counters, or strategy objects into generated parsers.

## What already exists

- `CSharpEmitter.cs:170`: publications are initially grouped by published rule, with reachable rules restricted per machine. Flat lowering and direct readers are tested independently for these groups. An unrelated publication does not automatically force another onto the arena.
- `CSharpEmitter.cs:2708`: `Joined` can share an existing larger machine for a reachable publication. It checks eligibility, but does not compare per-entry infrastructure cost. Sharing code and specializing an entry are separate decisions.
- `Machine.Flat.cs:197`: flat valued parsing already keeps captures locally and constructs at acceptance. Repeated captures, recursive rules, and several contextual features can prevent this route.
- `Machine.Reader.cs:53`: trivial rules can be inlined; choices, repeats, and calls already receive local control-flow optimizations.
- `Machine.Carrier.cs:65`: `Auto` chooses one carrier for a machine. A disqualifying valued rule or open way retains the tape for that machine. It does not choose `Mixed` automatically.
- `Replay.cs:98`: replay analysis is recorded per rule across the recognition graph. `Spread` propagates unsafe contexts to descendants. A safe call site cannot recover precision lost through another unsafe call to the same rule.
- `Machine.Sizes.cs`: code-size estimates are already explicitly separated from semantic proofs. Extend that separation instead of introducing a competing framework.

## Measured small-parser counterexample

Both fixtures use the current generator, default options, .NET 10 Release, and this leaf:

```dotgram
Letter = ['a'..'z']
```

The first captures each turn; the second captures the complete run:

```dotgram
Start : @string = t: Letter+   => @(t)
Start : @string = t: (Letter+) => @(t)
```

For this particular grammar there is no intervening trivia and the captured text is identical. This equivalence must not be generalized to valued rules, skipped trivia, optional captures, or effects without proof.

| Input | Per-turn capture, ns | Whole-run capture, ns | Per-turn bytes/call | Whole-run bytes/call |
| --- | ---: | ---: | ---: | ---: |
| One letter | 40.2 | 10.1 | 56 | 24 |
| 40 letters | 157.8 | 27.6 | 448 | 104 |
| 256 letters | 819.6 | 144.2 | 2608 | 536 |
| Invalid first character | 33.8 | 5.8 | 88 | 0 |
| 40 letters, then invalid character | 158.9 | 21.4 | 448 | 0 |

With warm JIT and the thread-local storage explicitly discarded before the measured call, the 40-letter case allocates 2472 versus 104 bytes. This measures fresh storage, not process startup or cold JIT. The warm 40-letter difference alone is 344 MB of allocation per million calls (decimal MB).

Method: seven rounds of 200,000 calls per case, medians, tiered compilation disabled for both fixtures. The two delegates perform the same result extraction. The allocations include result strings and diagnostics. These are local comparative measurements, not BenchmarkDotNet confidence intervals or a measurement isolating only pool overhead.

Agreement was checked for every UTF-16 single character and 2000 deterministic random strings, comparing outcome, position, length, and successful value. Diagnostic text, factory effects, and every possible input were not exhaustively compared.

### Why this happens

`GrammarNormalizer.Hoist.cs:129` recognizes pure text structurally but does not follow `Node.Call`. Thus the named `Letter` blocks a proof the direct character class permits.

The per-turn form reaches an immediate reader which pushes one packed span per character, copies those spans into a new array in `TakeSpans`, joins the pieces, and returns pooled storage. It also carries general `Ways` and unused value-stack capabilities. The whole-run form reaches existing flat lowering and only materializes the final string.

This is the first implementation target: prove contiguous text through suitable calls so the existing cheaper backend becomes available automatically. It is not necessary to implement a regional planner to recover this example.

## ExpressionLanguage is a different workload

The existing comparison verified equal trees over 176 shapes. Representative results with tiered compilation disabled:

| Input | Tape, ns | Immediate, ns | Hand-written, ns |
| --- | ---: | ---: | ---: |
| `(int x) => x` | 1524.5 | 1200.6 | 1079.7 |
| `(int x) => x * x - 1` | 2023.7 | 1639.5 | 1418.4 |
| `using System; (int x) => Math.Max(x, 1)` | 4945.2 | 4342.0 | 3913.2 |
| `(int x) => x * x -` | 1369.1 | 1577.2 | 1071.3 |

The repeated bare-lambda row was 1557.2 / 1211.9 / 1080.7 ns, much closer than in the initial tiered run. That initial run measured the identical lambda at 3173.5 and 998.5 ns at different points, so its absolute times are not a reliable baseline.

Allocations show a separate tradeoff: bare lambda 1160 / 1176 bytes for tape / immediate; incomplete expression 912 / 1440 bytes. Immediate is neither universally faster nor universally cheaper in allocations.

`ExpressionParser.cs` contains context mutations in guards, scope tracking, deferred readings, member resolution, and reflection-related work. Those costs remain after changing the value carrier. Region boundaries must preserve guard order, cached values requested by guards, state restoration, and the calling-assembly visibility contract. Existing constructor overloads accepting an assembly should be considered when separating host setup from generated recognition; do not silently change which assembly is visible.

The full SQL92 results from the earlier audit remain in `.work/parser-audit`. Its 64-condition comparison was approximately 11.13 microseconds for tape and 4.40 for immediate; its large value tables and retained buffers make memory layout more prominent than in the smallest examples. These workloads must not share a single performance acceptance metric.

## Semantic boundaries before removing flags

Two existing distinctions cannot be erased as implementation details:

1. `docs/syntax.md:437` explicitly allows `Auto` to execute constructions on a parse that eventually fails, where successful derivations cannot replace them. Explicit `Tape` promises stronger deferral. Removing the switch requires preserving that promise as a semantic contract, or deliberately migrating to one stricter contract. The conservative proposal is strict acceptance-time construction by default, with early values only where equivalence is proved or an explicit semantic contract permits it. This is a proposed policy, not the current implementation.
2. Character parsing and parsing over token kinds have different documented choice/return semantics. A cost estimate alone cannot switch between them. Select eager versus demand-driven token production only while preserving the same lexical and syntactic language.

Pure-looking C# is not proof of purity. Arbitrary actions can throw, mutate context, call another parser, or observe execution order. A region which builds late and one which builds early must preserve the required ordering between them, not merely avoid running a discarded action.

## Proposed planning model

Treat these as independent dimensions, with dependencies, rather than one carrier enum:

| Dimension | Candidate implementations |
| --- | --- |
| Recognition control | Inline checks, direct methods, local checkpoints, explicit frames |
| Captured data | Nothing, one extent, fixed locals, bounded collection, growing collection |
| Construction timing | At accepted entry, at a proved commitment point, on guard demand |
| Deferred representation | Fixed typed payload, typed regional storage, general record log |
| Input access | Character span or the grammar's token stream; eager/lazy buffering within that contract |
| Diagnostics | Local failure state; additional accumulation only where required |

### Facts needed per site

- Whether the site consumes, can fail after consuming, can be retried, or can escape to another alternative.
- Whether its value is used, is one contiguous source extent, or requires a collection.
- Which actions, guards, marks, external calls, and context dependencies it observes.
- Where its result is committed relative to enclosing alternatives and whole-input acceptance.
- Bounds on local storage and whether the call participates in recursion.
- Which runtime capabilities remain after simplification: replay cursor, lookahead depth, value stack, log, token buffer, recovery frames.

Compute reusable summaries per rule, refine at call sites, and solve recursive dependencies over strongly connected call groups. Keep correctness facts in grammar analysis. Do not derive legality by searching emitted C# strings; `Opens` currently uses such a search and is a seam to replace with explicit emission requirements.

### Region formation and boundaries

Start with the largest compatible region, split where semantics or capability needs differ, and compare the cost of the split including adapters. A rule is not necessarily one region. Conversely, do not introduce a region for every node.

A direct region returns position and typed values or a typed deferred payload. Entering a general region acquires storage only there. Leaving it must preserve deferred lifetimes and backtracking ownership. Avoid object boxing, a heap object per rule, delegates per capture, and repeated copying of the same values between formats.

Pure recognition under lookahead should use a value-free entry where guards do not require values. A normal call to the same rule may use a valued entry. Keep the number of specializations bounded by semantic context categories, with a code-size budget.

A tiny publication may share recognition methods with a large publication without renting that publication's entire infrastructure. Recursive regions can retain explicit continuation state while leaf readers stay direct.

### Cost model

Account for entry setup, per-character/per-token work, per-value work, exit cleanup, boundary conversions, first-use allocations, retained capacity, stack usage, and generated code size. Input length and failure frequency are not statically known, so use several representative workload classes instead of one guessed average.

Remove dominated alternatives first. For example, unused storage has no benefit; removing it needs no speculative timing model. Calibrate thresholds using the benchmark matrix. Keep forced backend selection internally for differential testing; a public user should not need to select a backend to avoid obvious overhead.

## Implementation sequence and gates

### 1. Recover cheap existing paths

Extend contiguous-text analysis through safe calls; retain exclusions for valued captures, optional null semantics, trivia gaps, effects, recovery, and recursion not covered by the proof. Memoize summaries without speculative expansion.

Gate: the small fixture selects flat/local storage automatically and loses its per-character array allocation. Compare outcomes, captures, diagnostic positions, and factory invocation traces. Include call chains, alternatives, backtracking suffixes, Unicode classes, trivia, zero-length bodies, and nullable captures.

### 2. Emit only required runtime capabilities

Replace unconditional reader-entry rental with explicit requirements. Separate failure/lookahead state from `Ways` replay and log storage. Emit only value stacks actually pushed or inspected. Preserve reentrancy and exception cleanup whenever storage remains necessary.

Gate: a pool-free reader is structurally pool-free, not merely zero-allocation after warmup. Compare first-use allocation and millions of warm calls as separate measures.

### 3. Refine replay facts by call site

Build on `Replay` and existing first/follow information. Separate recognition-only calls from calls requesting values. Preserve action ordering and guard caching. Make publication sharing consider entry requirements, not just direct-backend eligibility.

Gate: side-effect traces agree on successful, failed, abandoned, and recovered paths; unrelated publications do not add hot-entry infrastructure.

### 4. Introduce bounded regional composition

First compose direct recognition/local extents with one deferred region. Add context-dependent and recursive cases after the boundary protocol is validated. Limit specialization and generated method size.

Gate: compare against the established engine with generated grammars and the existing corpus; audit values surviving unwind and exceptions. Do not infer a working composition from merely having a carrier abstraction.

### 5. Optimize large regional stores

Use dense per-type tables, capacity budgets, and a measured policy for token buffering where applicable. These are regional capabilities, not mandatory setup for every parser.

Gate: SQL and ExpressionLanguage improve without introducing setup or allocation into tiny parsers. Measure retained memory after large-then-small input on multiple long-lived threads.

## Measurement matrix

- Tiny: literal, numeric token, named text loop, fixed captures, short URL; short/long success, first-character and final-character refusal; millions of calls.
- ExpressionLanguage: bare lambda, operator chains, nested parentheses, scopes, guards, member resolution, incomplete input. Separate host-state construction from recognition where the public API permits it.
- SQL: short statement, large tree, long single token/comment, early and late errors, deep nesting, large-then-small reuse.
- Streaming: short chunks, long records, recovery, end-of-window behavior, cancellation/termination behavior where supported.

Report cold process/JIT, fresh infrastructure with warm code, steady-state throughput, allocations, retained memory, and stack/thread use separately. Fresh-storage numbers in this investigation are not a substitute for cold-process measurements, which remain to be added.

## Reproduction and scope

Scratch experiments are ignored by git:

- `.work/strategy-audit/Program.cs`, `audit.csproj`, `small-results.txt`: two small grammars and equivalence/timing harness.
- `.work/parser-audit/expression-timings.txt`: initial tiered exploratory run; unsuitable for absolute-time claims.
- `.work/parser-audit/expression-no-tiering.txt`: comparative run with tiered compilation disabled, `--el 5 10000`.
- `.work/parser-audit/expression-allocations.txt`: `--elbytes 1000`.

The scratch project builds with the already-built generator DLL as its analyzer. Set `DOTNET_TieredCompilation=0` for the reported small-parser comparison. The initial investigation did not modify production code. The implementation and regression checks that followed are recorded below.

## Implementation progress

### Contiguous text through rule calls

Implemented the first bounded change in `GrammarNormalizer.Hoist.cs`: repeated text captures can now be lifted across calls to untyped, capture-free text rules, including call chains. Proof results are memoized for the normalization pass; recursive calls conservatively fail the proof. Valued rules, constructions, guards, state marks, recovery, and unsupported nodes retain the general path. Intervening separators are not absorbed into a captured value, and the optional null-versus-empty distinction remains intact.

The unchanged `t: Letter+` scratch fixture now selects flat lowering without `Ways`, `ImmediateValues`, or `DirectValues`. At 40 characters, warm allocation fell from 448 to 104 bytes; fresh-storage allocation fell from 2472 to 104 bytes. The final string remains allocated. The local timing changed from approximately 158 to 26 ns; these are separate before/after runs, not a controlled same-process speedup claim. In the post-change run, the explicit whole-extent reference was approximately 27 ns with the same allocation.

Added 11 carrier test cases covering pool-free selection, call chains, alternatives, backtracking suffixes, empty repetition, optional captures, trivia/separator gaps, recursion, typed collections, lookahead, and guards. Corrected the existing carrier-test value helper: `EmittedCode.Match` returns a tuple, whose value must be read directly; reflection for a `Value` property was returning null and weakening prior comparisons.

The URL snapshot now records the port's complete digit run instead of one capture per digit. The snapshot diff was reviewed and its baseline updated. The main suite ran 2219 tests with only that expected snapshot mismatch; all five snapshot checks passed after the update. All 14465 SQL tests passed. Compatibility builds passed for net8.0, netstandard2.0, and net472. The full-domain single-character and random-string scratch comparison also passed after regeneration.

Runtime capability elimination and regional strategy composition remain subsequent work. This change enables an existing cheap backend; it does not introduce a new global mode.

### Required stacks in immediate readers

Implemented explicit requirements for immediate value stacks. Generating a push, collect, guard peek, or checkpoint records the stack it requires. `ImmediateValues` emits only those arrays, counters, helper methods, and return-time clearing checks. Requirements are unioned across immediate machines sharing the generated class, using symbolic type-table names until final table numbering is resolved.

This removes infrastructure for value types that only travel through reader registers. It also removes the text/string stack and packed-span stack when neither is used. Existing mark-state storage, rental, and replay infrastructure remain; complete removal of the store from entries that need no storage is not part of this change.

Three new tests cover typed collection storage, text-piece storage, and multiple publications sharing different stack types. Repeated calls alternate input sizes or publications to exercise reuse. All 60 carrier tests and all 2222 tests in the main suite passed. Compatibility builds passed for net8.0, netstandard2.0, and net472; the benchmark project containing SQL and ExpressionLanguage also built successfully.

In the existing immediate SQL92 benchmark, generated stack arrays fell from 13 to 5: only Clause, string, Expression, Clause.When, and TableReference collections remain. Thus first use of that store allocates eight fewer arrays, and each return tests eight fewer high-water counters. This is a property of the emitted code, not a measured throughput claim. Warm per-parse allocation figures stayed unchanged on the SQL and ExpressionLanguage benchmark inputs, as expected for already-pooled storage; their equivalence checks passed on 42 and 176 shapes respectively.

The generated before/after stack declarations and allocation reports are retained in `.work/strategy-audit/sql-stacks-before.txt`, `sql-stacks-after.txt`, `sql-bytes-stacks.txt`, and `expression-bytes-stacks.txt`.

### Immediate readers without value storage

Immediate machines now omit the value-store rental, return, constructor argument, and reader field when they require neither gathered stacks nor mark-state storage. Each machine retains its own requirements; the class-wide union is used only to emit the shared store. Consequently, a publication that collects values does not force a separate scalar reader in the same generated class to rent its stacks.

If no immediate machine in the class needs storage, `ImmediateValues` emits no thread-local pool or rental/return methods. Its static `IsDefault` helper remains available for optional values. A grammar declaring state retains mark storage. `Ways` and recursion support are unchanged. This removes a first-use empty-store allocation and the repeated pool operations for the affected entries; throughput has not been measured for this step.

Three regression cases cover recursive scalar parsing in Auto and Immediate, plus alternating scalar and collection publications, with both valid and invalid input.

Validation: all 63 carrier tests and all 2225 main-suite tests passed. Compatibility builds passed for net8.0, netstandard2.0, and net472 with no warnings. `git diff --check` passed.

### Immediate readers without a ways rental

After rendering immediate rule bodies and their extracted parts, the emitter checks for actual uses of ways members and open replay paths. If neither exists, it omits the rental, return, instance field, constructor parameter, and deep-recursion handoff field. Diagnostic calls use a null constant instead; the refusal recorder accepts it and continues to update the same independent failure state. When no value store needs returning either, the entry also omits its try/finally block.

The decision is per machine. Tape carriers retain their existing infrastructure. Entry trivia is rendered under its own continuation, so publications with trivia conservatively retain Ways until those requirements are included in this analysis. Atomic/lookahead seals or any other emitted member access also retain it, even when the body opens no replay path. This step does not remove the shared Ways helper definition or implement arbitrary regional composition.

The recursive scalar depth fixture now allocates zero bytes for a successful ((a)) call after clearing thread-static caches with already-warm code, and zero bytes across one million subsequent calls. The same fixture successfully parses 20000 nested parentheses and returns 20001. These are allocation and correctness results, not timing or cold-process measurements. Scratch reproduction is .work/no-ways-audit/audit.csproj; output is results.txt beside it.

Validation: all 70 carrier tests and all 2232 main-suite tests passed, including snapshots. Seven added cases compare complete match tuples against Tape for collections, recursion, replay, lookahead, atomic groups, and trivia, and verify a class with both replaying and non-replaying publications. Three support snapshots changed only the nullable diagnostic parameter and its null check. Compatibility builds passed for net8.0, netstandard2.0, and net472. The benchmark build passed; SQL and ExpressionLanguage equivalence checks passed on 42 and 176 shapes, and their warm allocation reports were identical to the preceding step. No throughput claim is made.

### Include entry trivia in infrastructure requirements

Entry reading bodies are now emitted before entry wrappers. The Ways decision examines the complete reader members, including entry trivia under whole-input and positional continuations, rather than conservatively retaining storage for every publication with trivia. Each entry body is still generated once. This also makes any value-stack requirements discovered at an entry available before rental and constructor emission.

The immediate recursive scalar reader with ordinary space trivia no longer rents Ways. A trivia repetition that overlaps the rule's first character still rents it, since the entry may need to return a character swallowed by trivia. Regression cases compare the complete match result with Tape, plus positions, lengths and diagnostics from positional and bounded-window overloads.

The scalar fixture with spaces surrounding the expression and inside parentheses allocates zero bytes after clearing thread-static caches with warm code, and zero across one million subsequent calls. Parsing 20000 nested parentheses also succeeds. Reproduction is .work/trivia-audit/audit.csproj, with results.txt beside it. These are allocation and correctness checks; throughput and cold-process startup have not been measured for this step.

Validation: all 72 carrier tests and all 2234 main-suite tests passed, including unchanged snapshots. Compatibility builds passed for net8.0, netstandard2.0, and net472 without warnings. SQL and ExpressionLanguage benchmark builds and equivalence checks passed on 42 and 176 shapes respectively; warm allocation reports were identical to the preceding step. Formatting checks passed.

### Captured prefixes after lexical splitting

Added a bounded factoring pass after a successful lexical split, before building the syntax graph. It shares the identical prefix of a choice of constructions, including captures with identical names and calls whose token-level answers are committed. Capture slots are recomputed for the affected rule while retaining the existing result members. Adjacent sequence wrappers are flattened to preserve the established shared-head/factory layout.

The pass leaves character parsing unchanged, including lexical fallback. Explicit replay rules and calls, folds, climbing rules, binding-power metadata, and recovery are excluded. Guards and state marks are not moved as prefix nodes. It does not rename captures or create nested groups of factories.

In the emitted ExpressionLanguage If reader, the condition and closing parenthesis now precede the choice. The condition is held in a local and passed to the first tail; on its failure, the second tail reads Statement at the position after the parenthesis and uses the same condition. No runtime cache or pooled storage was introduced.

Seven regression cases cover condition construction exactly once with and without else, longer conditions, incomplete else, and comparison with a differently named capture baseline for character and explicit replay cases. The lexical fixtures also reject a silent fallback to character parsing.

Validation: all seven new tests and all 2241 main-suite tests passed. Compatibility builds passed for net8.0, netstandard2.0, and net472. SQL and ExpressionLanguage benchmark builds and equivalence checks passed on 42 and 176 shapes; their existing warm allocation reports were unchanged. The generated ExpressionLanguage If reader was inspected to confirm that the condition and closing parenthesis precede both tails. No throughput measurement is claimed.

### Bound retained direct-reader storage

Token buffers, Ways, DirectValues, and ImmediateValues now return to their thread-local pools only when their arrays contain at most 1048576 elements in total. The budget includes capacity retained from earlier calls, not just the current used count, and includes mark-state arrays where present. Sums use 64-bit arithmetic. This is a capacity budget, not a byte limit: arbitrary user value types have different element sizes.

Oversized stores are discarded as a whole before clearing their arrays; no retained pool reference needs those arrays cleared. Ordinary stores retain their existing reset and clearing behavior. An oversized outer return does not overwrite a smaller spare returned by a nested parse. Readers with no stores acquire no new checks. The separate automaton Parser pool and user-provided pooling hooks are outside this bounded change.

Four reflection-assisted regression cases verify the inclusive capacity boundary, summed array capacities, rejection above the limit, preservation of a nested spare, and subsequent parsing. Reflection makes the capacity boundary test independent of grammar record counts and growth heuristics. Repeated-large allocations are measured separately: discarding an oversized store necessarily trades reuse for lower retained memory.

The repeated SQL92 resource run confirmed the tradeoff. With 50000 conditions, cached array payload fell from 54587190 bytes (52.06 MiB) to zero immediately after returning the oversized stores. A subsequent one-condition parse retained 2250 bytes; its warm allocation stayed at 256 bytes, while the first small call after eviction allocated 3336 bytes. These cache figures exclude object headers and other process memory.

Repeated 50000-condition parses allocated 80149628 bytes per call instead of 15120052. At 10000 conditions, cached payload fell from 11303018 to 2843112 bytes, while warm allocation increased from 2960052 to 11422288 bytes. The 1000-condition case retained the same 1179466 bytes and allocated the same 296048 bytes per warm call. This policy bounds retention at a substantial allocation cost above the threshold; dense per-type value tables remain the next improvement for frequent large parses. Before/after outputs are .work/strategy-audit/pool-before.txt and pool-after.txt. Their exploratory timings used tiered compilation and are not reported as a controlled throughput comparison.

Validation: all four pool tests and all 2245 main-suite tests passed. Three support snapshots were reviewed and updated. Compatibility builds passed for net8.0, netstandard2.0, and net472. SQL and ExpressionLanguage benchmark equivalence checks passed on 42 and 176 shapes, and ordinary-input warm allocation reports were unchanged. Formatting checks passed.

### Baseline after synchronization with main (2026-09-15)

The worktree was advanced to main at 7bbc3ec, retaining the parser changes above.
All 3846 main-suite tests passed after this synchronization. SQL and ExpressionLanguage
comparison checks passed on 42 and 176 shapes, and their ordinary-input allocation
reports were unchanged. The benchmark project built without warnings or errors.

Added ParserResourceBenchmarks and TinyParserBenchmarks to the maintained benchmark
project. Together with the existing ExpressionBenchmarks and URL Grammar method,
25 BenchmarkDotNet cases completed successfully on .NET 10.0.12 x64, SDK 10.0.400.
The run used InProcessEmitToolchain, DOTNET_TieredCompilation=0, three warmups and
five 100 ms measurement iterations. Tests and builds had finished before timing.
These are short, noisy warm-call baselines, not evidence of a speedup; several timing
confidence intervals are wide. Allocation figures from the separate resource harness
exclude BenchmarkDotNet measurement overhead and are the preferred large-input counts.

| Input | Indicative BDN mean | Allocation per call |
| --- | ---: | ---: |
| Scalar `a` | 9 ns | 0 B |
| Scalar `  ( ( a ) )  ` | 16 ns | 0 B |
| Scalar first-character refusal `?` | 33 ns | 88 B |
| Scalar final-character refusal `((a)` | 17 ns | 0 B |
| Short URL `http://example.com` | 207 ns | 264 B |
| ExpressionLanguage `(int x) => x` | 1.96 us | 936 B |
| SQL, 1 condition | 283 ns | 256 B |
| SQL, 1000 conditions | 0.249 ms | 296048 B |
| SQL, 10000 conditions | 9.46 ms | about 11.42 MB |
| SQL, 50000 conditions | 22.67 ms | about 80.15 MB |

The ExpressionLanguage BDN method calls the shipping parser directly; its allocation
figure must not be equated with the comparison harness, which has different host setup.
Tiny-parser figures exclude process startup and initial JIT. SQL/URL methods and their
inputs likewise differ from one another, so these rows are not parser rankings.

The separate SQL resource run again retained zero cached array payload after 50000
conditions and 2250 bytes after the subsequent one-condition parse. That first small
call allocated 3336 bytes, falling to 256 bytes on warm calls. At 10000 conditions,
2.71 MiB remained cached while DirectValues was discarded. An early syntax error with
a lexically valid tail still scanned the entire input: the 500000-character case took
about 1.29 ms in the exploratory harness. This remains a separate lexer opportunity.

Reproduction commands are in benchmarks/README.md. Raw BDN reports are under
.work/strategy-audit/bdn-baseline/results; the resource log is sync-resource.txt,
and validation logs are sync-full.txt, sync-sql-bytes.txt and sync-expression-bytes.txt
in the same audit directory. baseline-bin preserves the benchmark executable and
its dependencies for subsequent comparisons. The next implementation is dense
per-type value storage, followed by rerunning these resource and correctness checks.


### Dense value tables for a final materialization walk

Direct tape machines with at least eight value types now use per-type dense indices
when no guard builds captured values during recognition. The decision is automatic
and per machine. Guard-driven materialization retains record-indexed tables and its
existing rollback watermark. This first step deliberately leaves dense indexing across
incremental materialization and rollback for separate work.

The materializer reserves an index only in the table receiving that value, growing
that table as needed. After reachability has been computed, the existing Starts array
is reused as the record-to-value index map. There is no additional mapping array.
Reads use the table field rather than a cached array reference because later values
can grow it. Reservation happens before evaluating the assignment target, so resizing
cannot leave an assignment holding the old array. SourceSpan records still read their
extent directly from the tape.

A shared DirectValues store emits dense counters and helpers only if a machine in its
class uses them. Dense and guarded publications can share it. Per-type counters also bound clearing:
dense writes increment the counter, a sparse walk raises it to the record high-water
count, and return clears the used prefix and resets the counter. Unused type tables
need no clearing. The sparse path retains a record high-water count for Built clearing.
The dense walk neither grows nor clears Built; a later guarded walk expands Built
independently of Live, preserving any already-built flags. There are no new parser flags. The eight-type threshold is a
conservative initial choice to avoid adding indirection to small tables, not a claim
of a globally optimal threshold.

Three regression cases cover character and lexical parsing with nine value types,
array growth, unused factories, alternating dense and guarded publications, refusal,
exception cleanup and reentrant parsing. They verify result values and that type tables
grow with their own values and release references on return. All 3849 tests passed with the final dense storage, per-type cleanup and lazy Built
growth. Compatibility builds passed for net8.0, netstandard2.0 and net472.

The SQL92 resource harness measured these changes against the saved pre-dense binary:

| Conditions | Allocation before | Allocation after | Cached array payload before | Cached array payload after |
| --- | ---: | ---: | ---: | ---: |
| 1000 | 296048 B | 296048 B | 1179466 B | 510923 B |
| 10000 | about 11.42 MB | about 2.96 MB | 2843112 B | 4342979 B |
| 50000 | about 80.15 MB | about 48.49 MB | 0 B | 0 B |

At 10000 conditions the dense store now fits under the retention budget. Retaining it
increases cached payload compared with evicting the former oversized sparse store,
but avoids allocating it on every subsequent parse. At 50000 conditions the stores
still exceed the budget and are discarded. After a subsequent small parse the cache
again holds 2250 bytes and warm per-call allocation remains 256 bytes; the first small
call after eviction allocates 3376 bytes, 40 more than before because of the counters.

Relative to the preceding implementation, SQL92 generated source grew by 10352
normalized UTF-8 bytes (1.349%) and its method IL by 4428 bytes (4.93%). MSSQL, its
located variant, SQL Standard and both ExpressionLanguage variants have unchanged
source and IL sizes: their current machines do not select dense final materialization.
The ordinary SQL and ExpressionLanguage allocation reports stayed unchanged, and the
42/176-shape equivalence checks passed. Performance logs and final validation are under
.work/strategy-audit/dense-*. The preserved baseline-bin is the version before this step.


Two final paired runs loaded the saved pre-dense assembly and the candidate into
separate load contexts in one process. They used identical input strings, disabled
tiered compilation, warmed both versions, and rotated their execution order across
15 rounds. No test or build ran alongside these timing samples. Ratios below are
medians of paired rounds, so they need not equal the ratio of separate time medians.

| Conditions | Before/after time ratio, run 1 | Before/after time ratio, run 2 |
| --- | ---: | ---: |
| 1 | 1.034 | 1.110 |
| 64 | 0.986 | 0.976 |
| 1000 | 1.007 | 0.987 |
| 10000 | 3.274 | 3.460 |
| 50000 | 1.311 | 1.331 |

Thus the large inputs improved substantially, while the 64-condition input was about
1-2.5% slower. The 1000-condition result moved around parity. The one-condition input
no longer has the regression observed before per-type cleanup and omission of Built
work were added. These are warm comparisons, not cold-process startup measurements.
The 10000-condition gain depends on the store now fitting within the existing capacity
budget and being reused; it should not be extrapolated to every input size.

Final logs: dense-built-tests.txt, dense-built-compatibility.txt,
dense-built-sql-bytes.txt, dense-built-expression-bytes.txt, dense-built-size.jsonl,
dense-built-resource.txt, dense-built-speed.txt and dense-built-speed-repeat.txt under
.work/strategy-audit. The paired SQL92 harness is .work/dense-speed.


## Guarded dense storage: measured and rejected

Extending dense tables to guard-driven materialization requires separate ownership
maps: Starts is scratch space on each reachability pass, and guards can build surviving
records after later records that rollback subsequently removes. A swap-remove prototype
handled that ordering and reclaimed failed attempts, but introduced costs on every
stored value and more arrays on first use.

Paired measurements show a useful large-input tradeoff, not a general replacement.
MSSQL with 10000 conditions allocates about 11.18 MB instead of 109.56 MB and runs
1.35-1.57x faster, while 64-1000 conditions regress by about 5-13%. Short expressions
mostly regress by 5-10%. The mixed MSSQL corpus has no established speed change.
The emitter changes were therefore removed, preserving the completed final-only dense
optimization. The full evidence, code-size increases and reproduction artifacts are
recorded in [the comparison report](parser-comparison-2026-09-15.md#guarded-dense-storage-experiment-not-adopted).

Next: evaluate a storage decision made once per parse before any typed guard runs.
A grammar-wide type count alone is insufficient. The sparse path for short inputs
must retain cheap reads and writes; the large-input path must keep rollback bounded.
Any duplicated materializer must also justify its generated source and IL growth.


## Adaptive tables follow-up

The next experiment keeps a direct flat prefix and adds lazy pages beyond it. The
selected implementation improves measured MSSQL short and large parses and leaves
ExpressionLanguage and URL on their previous strategies. A true interface-backed
adapter replacement was also implemented and measured, including a tiered JIT/PGO run.
It preserved correctness but was slower than the selected direct path. Results,
retention and first-use costs, generated-source/IL growth, the refreshed main baseline,
and reproduction artifacts are in [Adaptive value tables](adaptive-value-tables-2026-09-15.md).

## Guard materialization follow-up

Profiling found repeated subtree traversal rather than repeated requests for an already
built root. Large partitioned materializers without marks now skip built subtrees using
their existing Live/Starts arrays. Small and marked materializers retain their previous
walk. See [guard-materialization-2026-09-15.md](guard-materialization-2026-09-15.md) for
profiling counts, paired SQL results, eligibility, and rollback validation.
