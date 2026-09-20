# Architecture decisions: generator performance

A log of decisions agreed for bringing generated parsers to the work a handwritten parser
does. Each entry says what was decided, why, and what it binds. Proposals, measurements
and rejected experiments live in their own reports under `docs/design/`; this file only
records the outcome and points at them.

## How a decision is made

Since 2026-09-17 a session named `architect` reviews, and does not write, changes to the
generator's architecture: the renderings (flat, reader, engine), the carriers, the arena
and materialization, what the normalizer proves, the shape of emitted code, and any deep
refactoring. A session proposing such a change sends it before implementing it, with the
problem and the measurement that shows it, the design, the alternatives considered, what
the change touches (other grammars, streaming, recovery, diagnostics, code size), how it
is measured against the hand parser, and how it is undone.

The architect's refusal stands until Igor overrides it. An experiment in scratch needs no
approval; landing it does.

**Everyone cleans up after themselves, Igor 2026-09-19.** The scratch disk T: is 48 GB. A
session deletes its build trees, copies, traces and dumps as soon as the work they served is
reported and its raw results are in the repository; the stand keeps only the trees of its current
queue and one base.

**The architect does not measure or write, Igor 2026-09-18.** Its work is plan, delegate, review,
reconcile and decide. Everything about the measuring stand — `Stand.cs`, baseline runs, the
before/after runs a session asks for — belongs to the `stand` session.

**Grammars and the language, Igor 2026-09-17.** The grammar of a concrete parser (SQL, EL,
FIX, Web and the rest) may be improved by the session that owns it without asking: that is
work on a parser, not on the generator. A change to the language itself (`syntax.md`: its
notation, what a construct means, what hooks can see) is discussed with Igor first, before a
design is written, let alone code.
**A language change only when nothing else will do, Igor 2026-09-19.** And only after it is
discussed with him. Any edit to `syntax.md`, and anything that changes what a grammar may say,
goes to Igor with the alternatives before it is written; the architect does not decide that an
edit is "only a clarification". The one case this was not followed in: GRAM4030 and its
sentence in §7.8 (`bdef4949`), put to Igor after the fact.
**Igor's answer, 2026-09-19:** the diagnostic stays, the sentence added to §7.8 is reverted — the
text of the specification is not touched.
Done by expr (`13e38513`): §7.8 is byte for byte what it was before `bdef4949`.

**Main is linear, 2026-09-18.** A session lands by rebasing onto `origin/main` and pushing
`HEAD:main` (CLAUDE.md: no branches). Five merge commits reached main on 2026-09-18
(`97ade22a`, `87433353`, `91383eec`, `06ddaca4`, `4effecbc`); they stay, but no more: a merge
on main makes the stand's bisection of a generator-time regression ambiguous, since a step
may land on a commit that never built on its own.

## D1. A handwritten parser reads exactly the grammar it is measured against

Decided 2026-09-17 by Igor.

Every parser in `examples/DotGram.Handwritten` reads the language of the generated parser
it is compared with, as that grammar is implemented now: the same publications, the same
accepted and refused input, the same values (trees, fields) and locations. Error wording
may differ; the position of an error may not.

**Why.** A ratio against a parser that reads less, or reads something else, measures the
difference in language and not the generator. It also lets a generator change that costs
time on an unmeasured construct pass unnoticed.

**What it binds.**

- Conformance is checked by tests that run with the ordinary suite, not only by a
  benchmark's agreement step. A hand parser whose agreement is checked only before timing
  drifts as soon as the grammar grows.
- When a grammar gains a construct, its hand parser gains it in the same change, or the
  change says which comparison is suspended until it does.
- A timing is quoted only for inputs both parsers were shown to agree on in that build.

**State at the time of the decision** (`main` at `4a41ecc9`):

| Hand parser | Generated counterpart | Conforms | Checked by |
| --- | --- | --- | --- |
| `Fix.HandFixParser` | `FixParser` | yes, field model and locations | `HandFixTests` |
| `HandSqlStandard` | `SqlStandardParser`, all 42 publications | yes | `Both` in `DotGram.Sql.Tests`, fuzz corpora |
| `HandExpression` | `ExpressionParser` | yes, since `85372f80` (expr-2d) | `Both` over 242 call sites of `ExpressionParserTests`, `ExpressionHandTests` over 239 shapes and 14,332 mutations |
| `HandSqlTokens` | `Sql92Parser` | no: `SearchCondition` only of four publications; datetime literals not read | `--hand` agreement only, 42 shapes |
| `HandSqlOriginal` | none | no, by its own description | none |

**Decided 2026-09-17 by Igor: `HandSqlTokens` and `HandSqlOriginal` are removed.** SQL is measured
against `HandSqlStandard` alone. Carried out by sql-ff, which owns the SQL benchmarks: the two
files go, and so does every benchmark column and command that compares with them (`--hand`,
`--lexers`, the hand columns of `--bytes`, `--big` and `--spin`, `SqlComparisonBenchmarks`,
`SqlCounters`, `SqlSlope`). Each piece is either deleted or pointed at `HandSqlStandard` and
`SqlStandardParser`; none keeps a comparison with a parser that no longer exists. `docs/next.md`
keeps the ratios measured against them as history.

## D2. The value store clears what was written, not what every machine might write

Decided 2026-09-17 by the architect, on the diagnosis in `docs/next.md` ("Why the shared
machine is not dense", `4a41ecc9`).

SQL:2023's big machine is not dense (`_directBuilds`: forty-five tower guards name a built
value), its `Literal` and `TableName` machines are, and the machines share one
`DirectValues` store. So every materializer call raises 301 high-water marks and `Return`
clears 301 arrays: 20 to 27 per cent of a short select, building nothing.

**Approved:** the store records which types were written where a value is stored, and
`Return` clears only those. The towers stay; the big machine is not forced dense.
**Not approved as the fix:** one shared mark for all types. It makes `Room` O(1) and keeps
the 301 clears.

**Conditions.** Emitted output for grammars that are all dense or all non-dense does not
change, or the difference is shown to cost nothing. The write-site test is timed on a grammar
that stores many values per parse, not only on SQL. Agreement with `HandSqlStandard` holds.

**Landed 2026-09-17 as `7ed58baa` (sql-ff).** The mark goes up beside the write in record-indexed
machines; `Room` loses its loop; `Return` is unchanged. Only `SqlStandardParser` has
record-indexed machines, so only it gains marks (+76 KB of 13.9 MB); three dense-only grammars lose
a loop that was dead for them; every other grammar is byte-identical. Pinned, paired, both
orders: twenty select items 327 to 220 ms (-32.7%, second run -31.8%), `SELECT a FROM t` -37%, a
literal (dense, the control) within 1%. Twenty items against the hand parser: 26.7x to 17.8x.
Condition 2 is met by construction: the write-site comparison runs only in SQL:2023's machines,
where it was measured.

**Owed:** the change added no test. A missed mark leaves a table uncleared, which is references
kept across parses. A generator test over a small grammar with both dense and record-indexed
machines checks that nothing a finished parse built stays reachable from the store (a weak
reference to a built value, collected after the parse and a collection).

**What it is not.** A fixed cost of about a fifth of a short statement. The per-operand rate,
7.27 us against 0.33 an item, is a different mechanism, and Q1 is where it is decided.

## D3. FIX stays on the engine; the next direction is one-pass construction

Decided 2026-09-17 by the architect, on performance-3f's profiles.

The reader refuses FIX because it recovers (`Machine.Direct.cs:122`). Moving it there
would mean the reader learning recovery and streaming, and nothing measured says that pays:
FIX spends 56 per cent (Order) and 73 per cent (BinaryMany) in recognize plus materialize,
and SQL, on the reader already, spends 53 to 62 per cent in materializing. The two-pass shape
costs on both renderings, so it is not fixed by choosing a rendering.

**Direction approved for design, not for code:** building a value where the recognizer has
committed to it, in regions where that is proved not to change what runs (`syntax.md` §7.3:
a construction runs once per node of the accepted derivation). FIX first.

**Conditions on the design.**

- The proof is one analysis, beside `Replay`, answering per rule or per site; every rendering
  that builds reads its answer. No rendering gets a proof of its own. The generator already
  writes a way in three times; a fourth kind of special case per rendering is refused.
- It says how a construction relates to `recover`: whether the unit that recovery throws away
  can hold an eager construction, and what happens to one already run.
- It says what is left of the tape in a mixed machine, and whether the existing carriers
  (Tape, Immediate, Mixed, Auto) remain four or become a per-rule choice of two.
- It keeps the factories' order and count, the typed caches and rollback invalidation, and
  string, TextReader and Stream input.
- It builds only what is demanded. A value the grammar reads and discards is never built:
  the expression language's `Held` reads a lambda body only to find its end and keeps a span,
  and the body is built later, once its parameters have types. The tape honours this because
  its walk builds on demand; the Immediate carrier does not, and builds the body early,
  where a construction throws (found by expr-2d, 2026-09-17:
  `using System.Linq; (int[] a) => a.Select(n => n * 2)` reads on the tape and throws
  immediately). That is a defect of the Immediate carrier today, and the rule D3 must keep.
  **Fixed 2026-09-18 (performance-3f, up to `610906e3`)**: `Grammar/Model/Demand.cs` answers, per
  call site, whether the tape would build its value — never, with its parent, or during the
  match for a guard — and Immediate builds by that answer. `Demand.Of` is the analysis D3 reads.
  Auto hands no grammar in the repository to Immediate with an unbuilt site; only the expression
  language's explicit Immediate reading changed, and its skipped test is skipped no more.

**Parked:** narrowing FIX's follow sets for `yield` (six Run records, correctness of the end of
a yield step not established) and removing Run/CaptureOpen records that depends on it. The
whole-field helper stays rejected.

## D4. Timings on the Ryzen machine are pinned

Recorded 2026-09-17 from performance-3f. The machine has two CCDs with the V-cache on one.
Unpinned paired runs varied up to twice batch to batch; pinned to logical processors 0-15 at
high priority, FIX Order's spread fell from 28 to 4 per cent. A timing that was not pinned is
not quoted as a comparison.

**A timing window starts after everyone has stopped, not when it is announced.** 2026-09-17, the
first window of expr-2d's `--el` ran over two other sessions' builds that were already running
when the notice arrived (one until 23:53, one until 23:54). The session that measures announces the
window, waits until every other session has answered that nothing of its own is running, and
only then starts; a notice received mid-build is answered with when the build will end.

**Sessions do not wait for each other — Igor, 2026-09-18.** The machine has the resources for
everyone to work at once, and the window rule had grown into every session standing still for
every build. Amended:

- Only a *timing* run needs the machine to itself, and only from other heavy loads. Builds,
  tests, profiling for diagnosis and scratch work need no window and wait for nothing; two
  sessions building at once is fine.
- Timing runs are batched: the stand collects orders and takes them in one window, kept
  short (measurements only, builds done beforehand). A build-time measurement is a timing run
  and goes in a window too, but it is rare.
- The two CCDs are used: the stand times on logical processors 0-15; during a window the other
  sessions may build and test pinned to 16-31 (`Process.ProcessorAffinity`, or
  `start /affinity FFFF0000`) instead of stopping, once the stand has shown with its control row
  that a build on the other CCD moves a timing by less than the run's own spread. Until then,
  a window still means "nothing heavy elsewhere", but windows are short and rare.

**Measured the same evening (stand, 56 loaded runs against 8 quiet):** with unpinned builds
running on all cores, the *medians* of every row stayed within -3..+4% of quiet (the quiet
run-to-run range itself is 4-16%), while *single* runs went up to +85%, and the control row
saw only some of those. So: a before/after is quoted as the median of at least five runs
(`--repeat N`), never a single run, even in a quiet window; runs whose control is more than
5% off the median are dropped; and the other sessions build during a window pinned to 16-31
(`[IntPtr]0xFFFF0000L` in PowerShell — `0xFFFF0000` alone is a negative Int32 and applies
nothing — or `start /affinity FFFF0000`). The pinned case itself is rerun once to confirm.

**A before/after comparison is paired, in one process.** `--stand-paired` (stand, `f39b139f`,
2026-09-18) loads both builds into two `AssemblyLoadContext`s and alternates hand, before and
after in every round, so that what moved the machine moved both sides. Two separate processes
had shown +10-13% where the paired run showed +4% steady, and once the cause was fixed, noise;
the paired form is what a before/after is quoted from. **A ratio needs ranges that do not
overlap (2026-09-19).** Held memory of FIX after the carrier change read as falls and one rise
until the stand printed a smallest and largest beside each median: the figure takes three values
and nothing between, so a median of five is a vote between two of them, and the ratios moved to
other rows entirely on a rerun. Where two sides' ranges overlap the ratio is not quoted at all —
it is not a small effect, it is no effect — and only what does not depend on it is said; here, that
neither side's memory grows with the input at any cap, which is what D5 asks. Every median in a
table of small whole numbers carries its range. Over SQL it compares acceptance and not
trees, since types from two contexts are never equal.

**The pinned-load experiment (stand, 2026-09-19, rerun with a sustained load):** repeated rebuilds
pinned to cores 16-31, which the sampler showed at 72% against 46-52% before and after, moved no
timing on 0-15 beyond its spread — generated readings -0.7..+0.4%, the hand parsers +2..+5% inside
their 5-14%, the stand's control +1.4%. The rule stands as amended: builds and tests pinned to
16-31 while the stand times; an unpinned build is not, it took an earlier window to pieces.

**Asked by Igor 2026-09-19, when there is time:** the same rows on net8.0 against net10.0 — the
generated and the hand parsers apart, since the ratio between them may move with the runtime's
JIT; every family, the first call and its compiled methods beside the steady state, default PGO
with a twin if there is room. Narrowed the same evening: measurements stay on net10, since net8 leaves support in a month, so
the comparison is a one-off curiosity — one run, one row a family, nothing added to the kit. **Taken
(`benchmarks/results/net8-net10-2026-09-19/`), four legs, drift within 5%.** The generated readings
barely move between the runtimes (a FIX message +2.9% on the string form, a T-SQL statement -0.0%,
twenty selects -3.0%); what moves is everything else — ScriptDom is 82% slower on the older
runtime, so T-SQL reads at 0.14x of it there against 0.26x here with the generated parser
unchanged. The third leg, the same older build on the newer runtime, is 9-22% faster than the
newer build on every family, hand and generated alike, so the ratios hold; why is untested and
left untested. The rule taken from it: a ratio to a hand parser is comparable only between runs of
one build.

**The full baseline of 2026-09-18 evening (stand, medians of five, every family; the doc is
`docs/design/stand-2026-09-18b.md` with the raw results).** What it settled:
- Every generated-to-hand ratio is quoted **with default tiered PGO**, and the PGO=0 run is kept
  as its twin, never mixed in. Without PGO the hand parsers slow by +38% at the median (up to
  +146%), generated readings by +23%, ScriptDom by +64..85%, so the ratios shrink (FIX one field
  2.54x to 2.27x, a plain URL 1.72x to 1.03x, an SQL literal 8.0x to 5.9x); 79 of 127 readings
  move by a tenth or more. The hand parsers profit from dynamic PGO more than the generated code
  does — small monomorphic methods against large ones — which is a question for the shape of
  the code C2 emits, open.
- T-SQL against ScriptDom, default PGO: the generated parser at 0.28-0.55 of ScriptDom's time
  (located 0.42-0.67), 1.8-3.6x faster, allocating 3.3-13 KB against 48-138 KB. These replace the
  single-row BenchmarkDotNet figures in `benchmarks/README.md` as the quoted comparison for
  the stand's rows. Over the whole corpus (BenchmarkDotNet, quiet machine, default PGO, main
  `68c60c58`, 7,716 statements an operation): ScriptDom's tree 17.9 µs a statement (its lexer
  alone 8.6), the generated parser 4.1 µs located and 3.4 without positions — 4.4x and 5.3x
  faster — and 1.2 KB a statement against 42.5, 34x less garbage. The README's section carries
  these with the date and the PGO setting (`22691b3e`); the raw report is under
  `benchmarks/results/tsql-scriptdom-2026-09-18`. The week's changes did not cost the T-SQL
  parser its lead.
- Regex, for the record Igor asked for: a regex that only splits FIX into tag and value costs
  1.3-2.1x the hand parser (compiled 1.0-1.7x), about what the generated parser costs; a URL by
  regex 6-12x the hand parser (compiled 2.4-4.1x) where the generated one is 1.4-1.9x; a
  date-time by regex 11-17x.
- The generator-time gate against the morning base: T-SQL +47% (4.1 to 6.0 s), located +43%,
  the generator's own grammar +187% (67 to 194 ms). A bisection by pinned builds names the
  commit. First datum of the bisection: the morning base commit rebuilt in the evening reads
  +22..+39% against its own morning report, so at least half of the gate's figure is machine
  state. Rule from it: the gate holds a head to a base built in the same run, alternately (base,
  head, base, head) on one pinned set with nothing beside, and quotes the ratio, not the
  milliseconds; the bisection is read as a curve with the base as its control, a step naming a
  commit, a slope naming the grammar's growth and the GRAM5009 check. **Closed the same night: a
  curve, no culprit.** One worktree per commit, pinned, nothing beside: T-SQL 4.6-5.7 s across
  the day's commits with one machine outlier at 7.0 s (its located twin rose with it) and the
  known 1875588b spike (114 s); the head +18% over the same-run control, the located parser
  +5%, the generator's own grammar noise at its size; not monotone in rules, no step. The
  spread of one build is ±10-20% (the base itself 4,996 then 4,556 ms), as large as the effect.
  So the gate's +47% was half machine state and the rest single-build noise. Consequences: the
  gate script (`benchmarks/Gate-Generation.ps1`, base and head alternating, ratio of medians,
  20% and 100 ms) runs three rounds at least, since one build a side is inside the noise; the
  absolute figures in the stand's record are history. No analysis change lands without its own
  gate run.
- Rows whose spread forbids a verdict below it: an SQL literal 44%, EL interpolation 52%, an
  early EL refusal 45%, FIX slope-4 22%.

**The baseline of 2026-09-19 (stand, main `8b0a3b89`; `docs/design/stand-2026-09-19.md`).**
Generated to hand, geometric means with default PGO (PGO=0 twin): FIX string 1.72x (1.68x), bytes
1.81x (2.23x), stream 2.63x (2.83x); Web URL 2.61x, date-time 6.89x, JSON 3.02x; EL tape 2.18x,
immediate 1.39x; SQL:2023 7.88x; StockCount 1.20x; T-SQL at 0.33x of ScriptDom's time (located
0.45x). Since the evening before: FIX's string form -29..-34%, SQL:2023 -22%. Flagged: the EL tape
superlinear with linear allocation (an exponent of 1.46 from 100 to 1,000 terms, immediate 0.98)
and drifting +8..+17% across three baselines — expr, diagnosis first; Web date-time at 6.9x —
finance-24, an anatomy; SQL:2023's conditions linear now.
**The afternoon baseline (stand, main `841ce7c7`; `docs/design/stand-2026-09-19b.md`).** Generated
to hand, default PGO, geometric means: FIX string 0.76x (morning 1.72x, the 18th 2.43x), its slope
0.73x, bytes 1.71x (C4c's first step landed after this base), stream 2.19x; URL 2.60x; date-time
2.32x (6.89x); JSON 1.38x (3.02x); EL tape 1.98x, immediate 1.36x; SQL:2023 7.31x (7.88x; 10.54x on
the 18th); StockCount 1.07x; T-SQL 0.32x of ScriptDom's time. Regexes against the hand parser,
interpreted and compiled: FIX's split of tag from value 2.01x and 1.64x, allocating five to eight
times the generated parser; URL 10.7x and 3.3x; date-time 6.0x and 5.0x. No linearity flag of an
algorithm is left (EL's tape 1.46 to 0.48-0.78); what remains are the collector's. The PGO=0 twin
is two runs of three, below the rule; five more are queued.
**The EL tape, diagnosed and fixed locally (expr, `7cb9afba`):** the materializer. The walk a
guard runs first rebuilt the `with state` marks by reading the log from its start, at every name,
so every term; 94% of the time in one materializer. Marks are now rebuilt only where a record the
walk may build hands its factory the state or the marks. 1,000 terms 1,353 to 183 µs, 3,000 terms
11,010 to 546, linear; a scaling test in the slow project. It may also be the tape's drift on
small inputs; the pair says. With it, locally: the renderings now agree on every line of the
refusal record — the wording disagreements, 100 two days ago, are none.
**The EL tape drift is a regression (stand, an EL-only pair, every run's range positive):** 1,000
terms 158 to 1,349 µs (1.36x the hand parser to 11.6x), 100 terms +100%, the small rows +6..+15%,
the immediate carrier flat — introduced by one of the 21 commits since the evening baseline. expr's
fix is measured on all EL rows; the introducing commit is bisected whatever that shows, to learn
which pair missed it and why.
**Bisected (stand):** `584a7c1f`, the first part of the gathered-list fix — one walk for a guard's
list, which made EL's per-name guard walk quadratic in the terms. No pair missed it: none was run.
The architect had told it to land alone at once, on sql-39's rough SQL figures, with the pair left
to the store's second part; and a pair would not have seen it either, since the EL rows were
lambdas of at most twenty terms. Rules from it: no commit that changes emitted code lands without
its own pair, whoever says "now"; the stand's pairs carry size-sweep rows for every family with a
linearity series (EL terms, FIX orders, JSON arrays, feeds), so a change of exponent shows in the
pair of the commit that makes it; and the linearity family runs at the end of every pair window,
not once a day. expr's `7cb9afba` removes it by his figures; the stand's EL pair says what is left.
Landed as `a3b4e410`: the quadratic is gone (1,000 to 3,000 terms 3.0x), but a constant part is
left, +5..+15% on the small rows and +26..+29% at 100 and 1,000 terms against the base before
`584a7c1f`: every guard now runs the general walk the gathered list needed. Decided, once a
bisection confirms the one cause: that walk only for a guard handed a gathered list, decided by
the guard's arguments when generating; every other guard back to the cheaper build; paired on EL
and on SQL's conditions, so that neither loses what it gained. Also landed from expr: the
renderings agree on every line of the refusal record.
The second bisection confirmed one cause. Fixed more simply than decided (expr, `7937b564`, held
for its pair): EL's guard is handed a gathered list too, empty for a plain name; the walk now runs
only when the list holds something, so lists that do are built as before and SQL's towers are
untouched. Rough: 100 terms 15.9 µs and 1,000 terms 137, below the base before the regression
(16.9 and 158).
Paired: the EL tape back to the base before the regression at 100 and 1,000 terms (16,841 and
159,387 ns against 16,852 and 158,176), SQL flat or -3..-7%. The regression is closed.
**Web date-time's anatomy (finance-24):** the reader read nothing twice; the values did — each
field became a string twice, once for the guard and once for the construction, and was parsed
twice, about twenty strings and fourteen parses for 23 characters. The grammar's part is fixed
(span captures and span helpers): allocation equals the hand parser's, time 1.4-1.6x. The rest is
the generator's: a refusal read twice, quietly and then recording, for a `Try` that hands out no
message (68 ns and 88 B where the hand parser takes 25 ns and nothing; the URL refusal's 9.8x) —
decided that an entry returning only a bool does not read again, and one returning a match reads
again lazily when its error is asked for, over input still alive (expr, design first); and a
capture read by a guard and a construction materialized for each — C4's design gives the
construction the guard's value (performance-ff).
**The refusal read twice, put to Igor (2026-09-19).** §6 has no form that only recognizes: `parse`
publishes a throwing `ParseR` and a `TryParseR` returning a match, whose error needs the second,
recording reading. Two ways, both changing what the generator emits, so Igor's: (1) every `parse`
over a string also emits `bool TryParseR(string, out R)` at the publication's accessibility,
reading once and building no message, and Web's wrappers call it — an addition, nothing existing
changes, only callers who choose it gain; (2) the match becomes lazy, keeping the refused input
and reading again when its position, outcome or error is first asked — every caller gains, at two
more references in every match returned, and span and stream entries stay eager. Recommended:
(1) now, (2) only if a caller of the match is shown to pay for a message it never reads.
**Decided by Igor, 2026-09-19: option 1.** expr emits the bool form (a line in §6); finance-24
moves Web's wrappers onto it.
It breaks a principle §6.1 states in so many words, "no `out` parameters anywhere", whose reason
is that a result with room for a failure needs no second shape and that whatever a parse will
later want to say becomes a field, not a parameter. The new form is the one that says nothing,
so that reason does not reach it. Put to Igor with the alternatives (a nullable result without
`out`, or the lazy match); **Igor, 2026-09-19: the `out` form, and §6.1 says so** — its lead
becomes "one `out` parameter, and only where nothing is said", with one sentence on why, and
§6's sentence on what `TryParse` returns names the exception.
Landed `9acc28ac` with §6 in Igor's words: on a refusal the bool form takes half the time and
half the allocation (EL 1,284 to 661 ns, a late SQL refusal 23.1 to 11.3 µs), an accepted input
flat. finance-24 moves Web's wrappers onto it.

## D5. A stream is read without holding it

Decided 2026-09-17 by Igor: streaming exists to process volumes larger than memory, so a
`Stream` or `TextReader` is read through the parser's own buffering and never loaded whole.

**What it binds.**

- No path reaches a contiguous form by reading a stream to its end (`ReadToEnd`, copying into a
  `MemoryStream` or an array). A contiguous overload (Q2) is for data the caller already holds,
  never a shortcut for a stream.
- What a streamed `yield` holds is bounded by the record being read and what the grammar
  cannot yet let go of, not by the input read so far. Arena, value tables, capture buffers and
  input buffers all count. A limit that throws (`IOException` on capacity) is a bound; growth
  proportional to the input is a defect.
- The claim is tested, not argued: a streamed input several times larger than the process is
  allowed to hold, produced on the fly and never resident, read through `yield` with peak live
  memory asserted. The benchmark README says this scenario has not been measured; until it is,
  bounded streaming is a design intent, not a property.
- Any lexical mechanism chosen under Q1 reads tokens lazily over buffered input.

**Measured 2026-09-17 by performance-3f**, at `2f22d5fd`, FIX through `yield`, input made on the
fly under a 32 MB heap limit, live heap after forced collections every 250,000 fields: Stream
9 KB above a 263 KB floor at 16 MB, 256 MB and 1 GB of input (111 million fields); TextReader
29 KB at each size. Flat, and a 1 GB input completed at thirty-two times the heap limit. The
24 MB at 8,000 fields is total allocation of `Parse(string)` with a whole result, not live
memory, and not a D5 case. Scratch harness and results: performance-3f's `.work/streammem`.

**Approved as the standing test:** in `tests/DotGram.Tests`, over a small streaming grammar,
peak live memory at two generated sizes, the larger held to the smaller plus a constant, for
Stream, TextReader and `IEnumerable<string>`, with and without `yield`, and with a bad record
recovered every so often so that recovery is inside the bound. A FIX twin in
`tests/DotGram.Finance.Tests`, agreed with finance-03. The 1 GB heap-limited run stays out of the
suite until it has a place in `benchmarks/`.

**Ruling 2026-09-17, on a whole-result parse of a stream.** performance-3f's test found that a
buffered `parse` without `yield` keeps its whole input to the end: 4,092 bytes a record over
TextReader and 2,016 over Stream, for records of about 1,001 characters and a result of one
`int` each. `syntax.md` §6.3 describes this as the current, conservative release analysis.

Under D5 it is a defect. What a parse may hold grows with its **result**, which the caller asked
for, and not with its **input**: bounded working set plus the result. A root whose value is the
whole matched text holds the input because the input is its result, and that is allowed; a
root of one `int` a record is not. `maxRetained` turning the growth into an `IOException` is a
limit, not a release. The sentence in §6.3 goes when the defect is fixed, not before.

The cause is deferred construction: a capture is built after acceptance from the input it points
into, so the input cannot be let go first. D3's one-pass construction removes exactly that
dependency, so the whole-result memory test is one of D3's acceptance checks. The D3 proposal
compares it with the other way out, copying committed captures into owned storage at release
points: that keeps §7.3's deferral but holds captured text, which here is the whole record, so
it would not pass this test.

The lazy half of the test lands now. The whole-result half lands with its skip reason naming
this ruling, and the skip is removed by the change that fixes it.

**Buffers kept by the shared pool: confirmed.** Test `98d54101` landed as ruled. After one large
whole-result parse, a fresh process keeps about 47 MB (Stream) and 112 MB (TextReader) live when
idle, after a forced compacting collection, while the last input was about 2 MB. Attribution
is by elimination and size (the generated spare parser is not kept past 65,536 entries), not by
a heap dump. The pool trims on its own schedule, so collections do not release it.

**Approved as a change of its own, not folded into D3:** a buffer past a threshold is dropped
rather than returned to `ArrayPool.Shared`, the shape `KeptEntries` already has for the arena.
It stays needed after D3, since D5 allows one record to be large and its buffer would then be
kept the same way. Conditions: the threshold is justified by measurement; the idle figure above
falls to the floor, which also confirms the attribution; the time of small-record parses (FIX
One and Order, pinned) does not move; the retention test gains the idle check after a large
parse.

**Threshold, 2026-09-17.** Not returning buffers costs time at every size, because the buffer
grows by doubling from 4,096 and every step past 85,000 bytes is a large-object allocation: one
FIX record through `Parse(Stream)`, pinned, kept against dropped, 4 KB +24%, 64 KB 6.9x, 1 MB
3.2x, 4 MB +62%. FIX One and Order never leave the first 4,096 buffer, so any threshold of 8K
or more leaves them alone. **Approved:** 1,048,576 elements, the bound the generated token and
value stores already use, justified by this table where it is written. **Approved in principle
as a follow-up proposal:** the next parse on a thread rents at the capacity the last one needed,
remembered as one integer, which removes the doubling ladder that is most of both the time and
the idle retention.

The idle figure is explained: measured against a reading taken before the first parse, a parse
with no pool retention holds 4 KB after it, at 1 MB and 4 MB records alike; the rest was the
harness's own input. Returned to the pool, 4 MB records leave 16,385 KB, the doubling ladder.
The idle check runs live, not skipped (it is not blocked by the whole-result defect), with a
slack of the pool's legitimate keep, about two thresholds of elements, and it runs where no
other test shares the process's heap at the same time.

**For D7:** the test also showed the two mechanisms offering different forms. `IEnumerable<string>`
exists only on the legacy window mechanism, a `yield` publication gets no reader overload
there, and the buffered machines offer TextReader and Stream.
- A speed change to the buffered machines is measured with peak live memory beside time.

## D6. One SQL tree for every SQL parser, handwritten ones included

Decided 2026-09-17 by Igor. It extends what `design/sql-parsers.md` and
`design/sql-tsql-tree.md` decided on 2026-09-15 (T-SQL and `Sql92Parser` move onto the
SQL:2023 tree in `DotGram.Sql.Ast`) to the handwritten parsers.

Today there are two trees: `DotGram.Sql` (`SqlSyntax.cs`), built by `Sql92Parser`,
`TransactSqlParser` and `HandSqlTokens`; and `DotGram.Sql.Ast` (`Sql2023Ast.cs`), built by
`SqlStandardParser` and `HandSqlStandard`.

**Why it matters for performance.** With one tree, the generated and handwritten parsers
make the same allocations for the same text, so the ratio between them is recognition and
construction machinery and nothing else. It is also what D1 needs to hold across dialects.

**What it binds.**

- The public tree holds only the final nodes. A grammar's intermediate carriers (`Nodes.*`,
  `Towers.Typed`) are construction details of that grammar. A hand parser may share what is a
  property of the language, such as the towers' role bits, but not a generated parser's
  intermediate records. A hand parser that allocates the generator's intermediates would
  flatter the generator.
- When `Sql92Parser` and `TransactSqlParser` switch, a handwritten parser measured against
  them builds the new tree in the same change, or is retired in it (see D1's open question on
  `HandSqlTokens`).
- `Both` excludes `Span` from its comparison (`Both.cs:514`). D1 includes locations, so under a
  location-bearing reading both parsers are compared on spans too.
- The switch is measured for allocation per parse and for the number of value types a machine
  stores (D2's 301 grows with the tree), beside `--engine` and `--roundtrip`.

## D7. The input form is a dimension the generator compiles for

Decided 2026-09-17 by Igor: a string, an array of strings, a stream of strings and a stream
of bytes are each a form the generator uses as such, not an adapter onto another form.

This confirms the decision in `design/reader-input-forms.md` (a compile-time input form:
symbol domain, access form, result delivery as independent dimensions, with an emitter-side
abstraction and no runtime interface per symbol) and makes it binding.

**What D7 is about, Igor 2026-09-17.** The forms are already offered, and what matters is that
they are the API the user is given: the caller decides which one to read by, by the type of
what it passes (`syntax.md` §6.3), and some forms are declared by the grammar (`stream`,
`stream bytes` on a publication). How a form is served inside is the generator's business. So
the adapters below are performance debts, paid where a measurement shows the form costs more
than the one it is adapted onto (FIX bytes against FIX text), not a mandate to rewrite every
form natively; and the conditions below bind the generator's inside, not the API.

**Adapters in place today:**

- `IEnumerable<string>` puts the line terminators back and forwards to the `TextReader` path
  (`status.md`, "And from a sequence of lines").
- `FixParser.Parse(byte[])` wraps a `MemoryStream` and runs the buffered byte iterator.
- `FixParser.Parse(ReadOnlySpan<char>)` copies to a string (Q2).

**What it binds.**

- One input abstraction in the emitter: each form supplies the operations recognition needs
  (the `Peek`/`Get`/`Position`/`Restore`/`ReleaseBefore`/`Copy` set of
  `reader-input-forms.md`, and plain indexing for contiguous memory), and every rendering is
  written against it. Recognition and construction are not written again per form.
- A form does not choose the rendering. Today the reader refuses any streamed publication,
  so a stream sends a grammar to the engine; that is where D7 is not yet met, and it is the
  measure of progress on it.
- Two streaming mechanisms exist: `StreamingEmitter`'s window-and-retry wrappers around span
  recognizers, and the buffered machines. D7 converges them into one; a proposal names which
  survives and how the other's cases (retention analysis, stages, `find` over a reader) are
  carried over.
- D5 applies to every streamed form.
- A parse answers the same whichever form the input came in by; a test holds each form to
  the string form.

**Decided 2026-09-17 by Igor: an array of strings is lines.** The elements are one text with a
line break between them, which is what a feed is, as `IEnumerable<string>` reads today. The
break is `\n`, the one `eol` matches (`status.md`, "And from a sequence of lines"). As a form of
its own it reads the break where an element ends, without copying the elements into a
`TextReader` stream; an element boundary is also a point a feed can release input at.

**Decided 2026-09-17 by Igor: `ReadOnlySpan<char>` is a variant of the text form**, not a form
of its own and nothing to do with bytes: a caller who holds text in a span reads it without
making a string first. A convenience users will likely want rather than a performance
requirement, so it follows the forms above in priority; when it comes it reads the span, not
a copy of it.

**Decided 2026-09-17 by Igor: `byte[]` is a variant of the byte form**, as the span is of the text
form: bytes the caller already holds are read where they lie, not wrapped in a `MemoryStream`.
That retires the `FixParser.Parse(byte[])` adapter above once the variant exists.

**How, decided 2026-09-18 by Igor.** The generator offers `byte[]` (and `ReadOnlyMemory<byte>`) for
every publication with the byte form, not only for FIX. It is served by the existing buffered byte
machine over a source already in memory, which hands over the whole array at once and then
reports its end: no machine of its own, no copy, no `MemoryStream`, and next to no emitted code.
What remains of the gap between bytes and text afterwards is measured; a contiguous byte machine
is weighed against its code size only if that remainder is large. `ReadOnlySequence<byte>` and
`PipeReader`, the networking forms, are candidates for later on the same machine.
**Landed 2026-09-18 as `737376e5` (performance-3f)**: a `BufferedBytes` constructor over memory, one
fill that is the whole input, no copy, no buffer parameters; only FixGrammar and Fix44 differ.
`96d86795` makes the lazy buffered overloads plain methods that validate at the call and call a
private iterator over `int`s, so D9's `int?` costs no allocation (the stand had found +16 bytes a
call). Found by the compatibility build with warnings as errors: an emitted CS0649, which a
consumer building strictly would have failed on; the verification now stops at the first failed
build, since a stale assembly once let tests "pass".

**The remainder, and no separate byte machine (finance-03's report, 2026-09-18).** With bytes
read in place, the generated FIX parser still reads bytes 12-20% slower than text, the hand
parser reads both alike. The remainder is in the buffered rendering, not the input: the scalar
guard is off for buffered input (`Machine.cs`, `!BufferedInput &&`, decided on the stream form
before bytes were in place) and costs 40-45% of the gap, measured with the line removed; text
conversion allocated twice (fixed, `e7d17ceb`); what is left, some 3-6%, is a bounds check through
the buffer object per character and the missing `Scan_*` helpers on the buffered side. A
contiguous byte machine would be a third copy of the recognizer, about +39% source in Finance, for
those last percent. **Decided: no separate byte machine.** The scalar guard for buffered input is
performance-3f's next small proposal, with the stream rows measured before it lands; the rest
comes with Q7.5, where "in memory, never refills" becomes a constant the JIT folds.

**Several forms per parser, and feeds read by line — Igor, 2026-09-17.** A parser offers whichever
forms its grammar asks for, several at once: FIX needs the byte form, and the same parser must
also read a string in memory and a text stream. For a feed, streaming may read the stream a
line at a time, up to `eol`.

What the architect adds to reading by line:

- A line is read by the parser's own buffering and keeps its terminator. `TextReader.ReadLine`
  is not used: it drops `\r\n` and `\n` alike and cannot say whether the last line had one, so
  positions in diagnostics and locations would drift by a character a line on a CRLF file, and
  a grammar that tells the two apart would read something else than was sent (§0: diagnostics
  before speed).
- A line boundary is a release point: what a feed holds is the record being read, which is
  the D5 bound.
- A record that spans lines still reads, since the lines are one text; a single long line is
  held whole, which D5 allows.
- Reading by line is a strategy of the text-stream form for grammars whose records end at `eol`,
  chosen by the generator; the form's answers do not change with it.

## D8. The expression language's hand parser, from expr-2d's inventory

Decided 2026-09-17 by the architect, on expr-2d's inventory of `HandExpression` against
`ExpressionParser` (thirteen grammar commits since the hand parser last changed).

- **A grammar defect is fixed in the grammar first, not copied into the hand parser.** The
  grammar accepts key words where it writes `Word` without `?!Keyword` (`(int if) => 1`,
  `{ var if = 1; x }`, `nameof(if)`, member names after a dot, a foreach variable), which C#
  refuses. expr-2d owns the grammar; the fix (an identifier rule refusing key words at the
  declaration and member sites, keeping `@if` if the grammar reads verbatim identifiers) lands
  before the hand parser is brought up to it. D1 then holds against the corrected grammar.
- **What the hand parser gets wrong is fixed in the hand parser**: escapes, character literals,
  number forms, building on paths the grammar abandons, the order of calls into `State`.
- **Error positions follow the generated parser as it stands**, including two consequences of
  its implementation rather than of the language: a lexer error anywhere in the input wins,
  because the whole input is tokenized first, and a failure counts the positions lookaheads
  reached. Both are recorded as such. If Q1 or D7 makes tokenizing lazy, which D5 requires for
  streams, the first changes, and which position is right is then decided in `syntax.md`, not
  by whichever parser is written second.
- **One corpus.** `ExpressionAgainst`'s corpus moves to `examples/DotGram.Handwritten` as plain
  data, read by the benchmark and by the conformance test alike.
- **The conformance test** compares outcome, position on failure, `State` refusals, exception
  type and the tree by its debug view, over `ExpressionParserTests`, the shared corpus and a
  mutated corpus. It holds the hand parser to the tape, which is what ships.
- The Immediate carrier's defect (D3) goes to the generator's owner; until it is fixed no
  Immediate figure is quoted for untyped lambdas. `ExpressionCarrierTests` gains untyped
  lambdas, interpolated and raw strings, the Immediate half skipped with a reason naming it.

**Done 2026-09-18 (expr-2d).** The hand parser conforms (`85372f80`) and allocates less than the
Immediate reading on every input but one (`b36599bc`). Pinned, over the whole language
(`f88e3216`): common rows tape/hand 1.36-2.68x, immediate/hand 0.92-1.69x; interpolated and raw
strings tape/hand 2.91-3.62x, immediate/hand 2.16-2.67x, the cost being a publication entered
again for every hole (a spare per nesting depth, performance-3f); untyped lambdas about 1.04x,
both parsers spending 50-66 us in overload resolution and inference, which is the host's.

## D9. A parser's retention limit: set by its grammar, overridable by whoever uses it

Decided 2026-09-18 by Igor. The limit on what a streamed parse may retain (`maxRetained`), and
likewise the initial `bufferSize`, is an option of the generated parser, not a constant written
beside it in a package.

- **The grammar sets the default**, as an option on its attribute (`[Gram(…, MaxRetained = …)]`,
  and `BufferSize`). FIX sets 16 MiB in `FixGrammar`, the same as `FixMessages`' message limit.
- **The user of a shipped parser changes it per call**, without rebuilding the package: the
  parameter below. The default is exposed read-only (FIX: `FixParser.DefaultMaxRetained`). No
  settable static: it would be one value for the whole process, which two libraries using the
  same package, or parallel tests, would overwrite for each other (finance-03's objection,
  Igor's choice the same day). An application-wide setting (`AppContext`) is added only if a
  case appears where the call site is out of the user's reach.
- **Per call it stays a parameter**, now `int? maxRetained = null`: `null` means the parser's
  current default, a number overrides it for that call. A caller passing a number compiles as
  before.
- What a grammar gets when its attribute says nothing is decided with D7 (a finite default or
  `int.MaxValue`).

Generator: performance-3f. FIX: finance-03, instead of a constant of its own.

## D10. Faster code on newer targets, and unsafe code, by the user's choice

Decided 2026-09-18 by Igor. A feature that makes generated code faster is not given up because
the default floor cannot have it. Generating on `ref struct` type arguments (`allows ref struct`,
C# 13 on .NET 9 and later), or with `unsafe` code over `char*` and `byte*`, is offered as an
option the user turns on; it is never the default, which stays C# 8 on netstandard2.0 and net472.

**What it binds.**

- The default rendering is the reference: an opt-in rendering answers exactly as it does, and the
  tests compile and run the grammars both ways.
- An option is added only with a measured gain on a shipping grammar, and names what it
  requires of the consumer (target framework, `AllowUnsafeBlocks`); a build that cannot have it
  says so as a diagnostic rather than failing in the compiler.
- It fits Q3's rule, not against it: Q3 removed options that chose between renderings nobody
  needed; these choose what the consumer's build allows.

## D11. A diagnostic that says the grammar reads otherwise than written is a warning

Decided 2026-09-18 by the architect, on sql-ff's finding.

`GRAM5009` says that, over kinds, an optional or a repetition takes what the part after it
needed, so the split reads something other than the grammar as written (`FirstSets.Committed`).
It was Info, which a build does not print, and it had fired eight times on parsers already
shipped over kinds — seven in T-SQL, one in SQL-92 — without anyone knowing, and six more on
SQL:2023 in flight. A diagnostic about the language a parser accepts cannot be one nobody sees.

- `GRAM5009` becomes a Warning. The other Info diagnostics in `Grammar/` are reviewed by the
  same rule: what tells the author about a change in what is read is a Warning; what reports a
  choice the generator made (carrier, rendering) stays Info.
- Order: the fourteen places are fixed in the grammars first (SQL:2023 by sql-ff before its
  split lands; T-SQL and SQL-92 by sql-ff as well, their owner, each fix with a test of the
  input the grammar meant), then performance-3f raises the severity, so that
  `TreatWarningsAsErrors` never meets it red.
- `docs/development.md` says how to see Info diagnostics (`-v:detailed`).

**`Committed` refined (`610906e3`)**: the cures the diagnostic asks for are recognized when an author
writes them — `?=` on what follows at the end of an optional or repetition, also through a call
or a choice; a `?!` of more than one token at the start of what follows, by a second-token
analysis limited to that form; and atomic braces, as before. Repetitions the analysis does not
see (`*`, sql-ff's `UNIQUE (a, p WITHOUT OVERLAPS)`) are the next refinement. **The repetition
check landed as `1875588b` and cost generation time — DotGram.Sql 17 to 76 s, T-SQL 4 to 86 s, the
split SQL:2023 never finished — because it walked every caller in full on every question; found by
sql-39 and the stand, fixed in `c8d44074` (asked only where the first token overlaps, call sites
from an index built once, a budget of steps). Rule from it: a change to `FirstSets` or `Committed`
is measured for generation time on DotGram.Sql before it lands, and the stand's generator-report
column is read at every base run.**

**The list after the repetition check (sql-39, 2026-09-18): 49 places** — T-SQL 44, SQL-92 2, SQL:2023 3
— against 7 + 1 + 6 before it; many of the new ones look like over-reporting (an alias taking a hint
word that could only follow with `WITH`). Order: sql-39 fixes the twelve that are his (the eight
shipped ones plus the four the check found in his own code), each with a test, and *classifies*
the other thirty-five in one pass — real, intended, false, one line each — without fixing the
false ones; performance-ff narrows the analysis by the classes of false ones after the FIX steps;
the severity goes up after both. Not a week of grammar work on what the analysis over-reports.

**Classified (sql-39, `3dee45c5`, `docs/next.md`).** The one real trap in the shipped parsers is the
**label**: a T-SQL statement may begin with `name:`, the separator is optional, and every word an
optional at a statement's end takes is a legal label — `SELECT a FROM t` then `x: PRINT 1` on the
next line is read by the server and was refused here, the table alias having taken `x`; `--engine`
cuts at statements and could not see it, the tests kept labels only after `;`. Real: aliases,
RETURN, THROW, WITH MARK, EXEC's `out:`, column tails, ENFORCED, FOR XML options, option units,
login password words. Intended (greed is the language): JSON_ARRAY's NULL ON NULL, XML(CONTENT),
TRIM(LEADING), statement lists, ARRAY/MULTISET. False, five classes for the analysis: a lookahead
after an element inside a turn; a turn that cannot end and gives back; a closing `?=` behind a
parametrized call; a word inside a bracketed option list or a reserved word no statement begins
with; a literal, variable or digits no statement begins with. **Decided:** every real label place
is cured with `?!(word & ':')` and a test of a label right after the statement; intended greed is
said with atomic braces, which the analysis already recognizes; the false classes narrow the
analysis after the FIX steps; then the severity goes up.

**The 49, classified (sql-39, `8682f167`, next.md "GRAM5009's 49 places, by what the analysis
would have to see"):** 29 are a refusal behind a call (an inline `?!Label` whose body is a
call, `?!Label` at the start of the called rule, `Trailing(word)`); 2 a positive lookahead in
front of the call; 17 have nothing that can follow with the word, the analysis's own classes;
1 unexplained (`AuditAction`'s `','?`). Three more real label traps found on the way (`AS ROW
START` then `hidden:`, `WITH SORTED_DATA` or `WITH FILLFACTOR = 80` then `sorted_data:`),
checked with sqlcmd, fixed in the grammar, the round trip 7,716 of 7,716. When `Leading` looks
through calls (performance-ff, after C2), the 29 should go and this list is the check; the
severity is raised when the list is empty.

## D12. Tests are reviewed for what each one proves

Decided 2026-09-18 by Igor: the test suites are reviewed and what is redundant or no longer
needed goes. The stand measures first (build and run time per project, per class and per test);
each owner then reviews its area by these rules, each removal named in the commit with why:

- A test goes when it asserts what another test already asserts on the same input, when it
  tests a feature that was removed (Mixed, the options of Q3), or when it asserts the text of
  emitted code rather than what the code does.
- A test stays when it is the only one holding a claim: the snapshots, the agreement tests
  (`Both`, `RefusalTests`, the hand-parser conformance), the retention and streaming tests,
  the diagnostics corpus.
- A slow test is not removed for being slow; it is moved where its cost is paid only when it
  is asked for. The first candidate: `Finance.Tests` compiles Fix44's 59 MB on every build
  (about 150 s) because Fix44 is its oracle; the oracle tests move to a project of their own,
  so that the ordinary Finance tests build in seconds and the oracle runs when the grammar or
  the parser changes.
  **Done 2026-09-18 (finance-24, `8698f294`)**: `tests/DotGram.Finance.Fix44.Tests` holds the
  oracle comparisons and links the shared field-reader tests, which Finance.Tests runs on the
  generated and hand parsers; the README says when the oracle is run locally, and CI always
  runs it. Finance.Tests 6,376 tests, Fix44.Tests 614; build times to be confirmed by the stand.
  **The mechanism for the rest (decided 2026-09-18 on expr's plan):** the oracle's is the
  convention — a project, not a trait. `tests/DotGram.Tests.Slow` holds what is paid on demand
  and in CI: the full refusal corpus (`RefusalTests`, 20 s, the ordinary run keeping a
  deterministic one-in-five sample of every shape and rendering against the same recorded
  file, the full corpus always before a change to failure recording lands), the streaming
  retention sampling (49 s, D5's holder), and performance-ff's oversize and 49-second buffered
  split tests. `dotnet test` on the solution runs it, `dotnet test` on a project does not; no
  filter to remember, one line in the layout and in development.md. Kept as they are: the
  precondition asserts that say which rendering a test exercises; replaced by behaviour: the
  context test that asserts emitted text (a counting guard, once without the pair, twice with).
  **Landed `195bb324` (expr):** the project is in the solution with the whole refusal record and
  the retention tests; the sample is one reading in five (what costs is compiling a parser per
  reading, not the inputs) held to the same recorded lines, RefusalTests with CarrierTests 9 s
  against 20; the context test asserts behaviour. Duplicate hunting in the EL classes stays open,
  low priority at 9 s together.

## D13. FIX first: the gap is to be explained and closed by hand-like code

Decided 2026-09-18 by Igor. FIX is the priority; SQL and the rest go on beside it. The
generated FIX parser is about 2.6x the hand-written one on a string and the reason is to be
found, not assumed: first the parser's own initialization on small inputs, which can dominate
a one-field parse; then the generator is to write code like the hand parser's. If that needs
the architecture revisited, it is discussed with Igor.

**What is known.** One field: 196 ns generated against 72 by hand — 124 ns over a parse that
reads six characters, so most of a small parse is not reading. Order: recognizer 32%,
materializer 27%, `Parser.Reset` 6%, factories 24% (shared with the hand parser). FIX is on the
engine because it recovers; the hand parser is a loop that reads a tag into an int, finds the
separator, switches on the tag's kind and builds.

**First, the anatomy (performance-3f, with stand):**

1. A slope over 0, 1, 2, 4, 8 fields, string form, generated against hand: the intercept is
   what a call costs before it reads, the slope is a field. Each side's intercept is broken
   down from the emitted code: renting the spare parser, `Reset`, the arena and value tables,
   the context, the result array.
2. A field's cost broken down: recognition (tag digits, `=`, the value to the separator),
   the records written for it, materialization, the factory.
3. Beside it, the same for `HandFixParser`, so that each line of the generated cost has the
   hand parser's line next to it or a blank where the hand parser does nothing.

**The slope, measured (stand, `4cdd4c59`, `docs/design/fix-slope-2026-09-18.md`).** Six sizes,
string form, paired, least squares with R² 0.999: hand 27 ns + 51 ns a field, generated 72 ns +
131 ns a field; 113 B a field on both sides, the generated fixed part smaller (90 B against 203).
So the 2.6x is not initialization against fields but both alike: about 80 ns of CPU per field
over the hand parser, and 45 ns per call. Memory is not where it is. The per-field 80 ns is what
the anatomy has to account for item by item.

**The anatomy, with numbers (performance-ff, sampling over the stand's times).** A generated field,
131 ns: recognition with the arena writes 47, the second pass over the arena (four passes and
the link walks) 38, the factory 23 (the same code the hand parser calls), `Reset` and clearing
8, covariant stores into `object?[]` tables 7, the rest table checks. A call, 72 ns: recognition's
fixed part 23, `Reset` 8, rent and return 8, construction's fixed part 8, entry and `Match` 5.
The hand parser's field, 51 ns: a `Peek` per character through its `Input` class 14, the
iterator 12, the factory 19, `ToArray` 5. An ideal reader (`IdealFixParser`, `fe3b6ff5`: a loop
over the span, `IndexOf`, the same factory) is estimated at 33 ns a field, the factory being
most of it. **So 85 of the 131 ns are the arena and the pass over it** — what a reader with
`recover` building as it reads removes — and the items that need no architecture (`IndexOf` for
the value, the tag and `=` read twice) are a few nanoseconds. The design comes next.

**The design (performance-ff, `.work/d13/DESIGN.md`, to be committed as
`docs/design/fix-reader-2026-09-18.md`), accepted by the architect and put to Igor.** The
generated `Fields` becomes a loop whose turn is a method: a turn that returns -1 is the broken
element, the sync is found by `IndexOf` from the turn's start and the recovery value built
there; `Field` scans the digits, builds `tag` at once (Demand: a guard names it), switches on
`context.Kind(tag)`, finds the value by `IndexOf`; the turn's constructions run where the turn
commits, once `end` is read. Against the ideal only `Construct_Tag` and Finance's second `=`
remain. What the generator needs: (1) the reader accepts `recover` — a recovering repetition is
this loop, §8.2 as it stands; (2) a new answer from Replay/Demand per site, *the innermost point
past which the reading stands* — for FIX the turn, not the rule — and the reader keeps captures
in locals until that point and builds there: the tape's semantics without an author's promise;
(3) a guard's value as a local argument, no table; (4) the reader over buffered input (D7/Q7.5)
for the stream and `yield` forms, `yield` being the turn method called once a step. The engine
stays for `find`, captured lookahead, arguments and what climbing cannot do; for FixGrammar it is
then not emitted at all: 588 KB today, 120-150 KB estimated.

**Steps, each with a number and the gates** (HandFixParser, the Fix44 oracle, RefusalTests,
snapshots, Compatibility, the stand with hand and ideal): 1) the target code written by hand as
the generator would emit it, timed — no generator change; 2) reader with `recover`, string and
`byte[]` leave the engine (most of the 47 ns); 3) construction at the commit point (most of the
38 ns), gated by factory counts against the tape; 4) value runs by search; 5) the reader over
buffered input. **Conditions the architect adds:** the reader's recovering loop keeps all of
§8.2, including "try the complete continuation first at every boundary" for a repetition that
is not the whole parse (a `Feed` with a `Trailer` is the test), not only FIX's end-of-input
case; the commit-point answer is the one analysis of D3, published beside `Demand` and `Replay`
and read by every rendering that builds; a guard's side effect in a broken element runs as it
does today, and the design says so; recovering grammars keep one recording reading (Q7.2).
Step 1 starts now. **Igor, 2026-09-18: steps 2-5 agreed, after step 1's number.**

**Step 5 designed (expr, `0cde15a5`, `docs/design/fix-reader-buffered-2026-09-18.md`), accepted.**
The reader is written once against the machine's helpers (ReadAt, Room, Search, Slice, Cut) and
emitted twice, over a span and over `BufferedText`/`BufferedBytes`, which gain an `IndexOf` that
refills; a runtime type parameter was rejected for the reader, since its body is small and the
engine goes. Q7.5 stays open where a copy is large (T-SQL's located reading, Q4.2). `yield`
releases per element as now; a whole-result parse releases before each turn once constructions
run at the turn's commit, which un-skips the D5 whole-result test — that test is the step's gate.
Order: the reader's access sites onto the helpers, byte-identical; then the buffered branches;
then release per turn after step 3. expr carries it beside performance-ff's steps 2-3; finance-24
takes the Finance-side items of the anatomy (the tag parsed twice, `Create` searching `=` again,
two constructions a field). D14 waits.

**Step 5, first slices (expr):** the reader's access through the machine's helpers (`964e1f26`,
byte-identical); a buffered character `parse` read by the same reader as its string form, where
that form is a reader (`d994009a`) — bytes, `yield`/`find`, deep recursion, release-as-you-read and
externals stay on the engine for now. Found on the way: the reader refuses inside a multi-character
literal at the literal's start, the engine and flat where it broke off; by the rule that a refusal
is where the parse refused, the reader is wrong, and `RefusalTests` will hold the three renderings
to one answer (performance-ff, after C1).

**Step 5, the byte slice (expr, `c6e0348b`):** the same reader reads `BufferedBytes` — the
character is an `int`, multi-character literals go through the input's `Matches`; a text capture
joined across turns takes the input's element type (both carriers wrote a char array before;
characters unchanged). GRAM5014 (Info) says why a publication stays on the engine over a buffer:
a rule it reaches can reach itself. Gates: the generated files byte-identical against the parent
(944 of 944), both suites green, no warnings. Left of step 5 and waiting on C2: `yield` and
`find`, recover, the buffered search branches, and release per turn after C4 (the D5 test is
un-skipped there).

**Window 2 (stand, 2026-09-18 20:34; medians of five, paired).** The rule for holding a change
from here on: a row's paired spread is the hand parser's own run-to-run spread (2-13%, the EL
interpolation row 28%), and a change counts only outside it.
- C1 (performance-ff, `137600f4`: the reader's helpers and the runs by `IndexOf`): FIX flat, the
  slope rows within 3%; EL and SQL flat. C1 is groundwork, and the gain the design attributes to
  the scan of the separator is now expected of C2, where the reader with `recover` reads the
  field. C1 lands as it is; the EL interpolation row's +17% is inside its spread and is re-read
  at C2's pair, not rerun alone.
- The FIX anatomy items (finance-24), measured one on top of the other, in finance-24's order
  (the stand's numbering was its commit order; corrected by finance-24 the same evening):
  `a8d8952a`, `Create` not searching `=` again and not reading the data tag twice, +4..+13% on
  10 of 12 rows — it replaced the search by a `value:` capture, and a capture costs arena
  records on every field, more than the search did; `80d1704b`, one construction a field,
  -5..-13% on all 12, on main; `7141660c`, the digits read in the guard, faster everywhere but
  the many-binary row (+5..+10%): on a length/data pair the binary arm's guard still needs the
  size and the data tag as numbers, so the value tables stay and the tag is read a third time.
  Done as ruled: the capture is withdrawn (`98d230dc`, the data-tag part kept, paired against
  main); the digits form reworked to slice the value right after the digits and `=` with no
  search and no capture (`0b2b8e08`, paired against `98d230dc`; dropped if the many-binary row
  is still outside its spread). Allocation unchanged by all three. The lesson is the generator's:
  on the engine a capture is not free, it is arena records per occurrence, and a grammar author
  trading a scan for a capture loses. `JsonValue`'s depth defect in `ToString`/`Equals`/
  `GetHashCode` is fixed on main (`817ef9bd`, a 100,000-deep test).
  Closed: the value from the wire again, no capture, landed (`4effecbc`), 0..-12% and no row
  slower; the digits form dropped — the many-binary row +5..+10% on all three forms against
  3-5% spreads, though bytes gained up to -15%. The lesson: reading the digits in the guard
  wins where the guard's value tables are the cost, and loses where a second `when` on the
  same field still needs the numbers, since it reads the tag again. The C2 control example,
  a feed with a trailer (`StockCount`), is on main at `91383eec`.

**The literal refusal (performance-ff, on top of C1).** The reader now refuses as the engine does,
for a lone literal and for a choice of them: at the deepest character any of them agreed with,
naming only those still agreeing, through the engine's own trie walk shared out of line (the
inline form was +22% on the SQL:2023 file). `RefusalTests` gains the shape "literals that part
late", 42 inputs, the three renderings agreeing on every one. Found beside it: the other shapes
already hold 100 inputs where the reader and the engine disagree — the expected set named
(`[' ' | '+']` against `'+'`), and "Expected X" against "Input does not match 'Start'" after a
trailing character. Rule: the engine is the reference reading unless shown wrong; a disagreement
in the position of a refusal is a correctness defect and goes before C2, one in the wording
only goes after it.
Answered: none of the hundred differ in position or outcome; all are wording (58 the same
one — the reader names the trivia class where the engine names the literal, or splits a class,
or says "Expected X" for "Input does not match 'Start'"), and they go after C2. The helper is
written only inside the refusal branch, and the run's walk only where the whole run failed;
a count from the generated FixGrammar confirms it before C1 goes in. Found on the rebase: with
comments now read through the search, C1 wrote U+0085/U+2028/U+2029 raw into a C# literal,
where C# reads them as line breaks — spelled through the emitter's character escaping now, a
test in `DelimiterScanTests`.
Landed: C1 `f2b218da`, the literal fix `aef52068` (the two hand-parser lines with finance-24's
go, the shape in the refusal corpus), each verified alone on the rebased base, the whole slow
record included; FixGrammar byte for byte the same after both. Next for performance-ff: the
speller commit, the D12 move of the oversize and buffered-split tests into the slow project,
then C2 over StockCount.

**C2's note (performance-ff, 2026-09-18) and the order of `recover`.** The trigger is by shape:
a rule holding a repetition marked `recover` whose turn the reader can read and nothing
outside re-enters, followed in the same sequence by a continuation ending in `eof` (empty for
FIX, `total: Total & eof` for StockCount); the loop is one small method, the turn a method, the
broken-element search and the continuation cold and out of line; no delegate or virtual call
of the reader's own on the hot path; the tape stays until C3/C4. Accepted, with one thing
decided against the note: performance-ff proposed the engine's order — read turns to the end,
then give them back from the end until the continuation holds, the latest boundary — instead
of `syntax.md` §8.2, which says the continuation is tried first at every boundary and the
repetition ends at the first boundary where it holds. **The specification stands.** It is the
language (Igor decides it, not a rendering), and it is the only order a stream can have: a
`TextReader` parse hands good elements back as it goes and cannot give them back from the
end, and §8.2 says the driver steps over a bad element one repetition at a time. It is also
the cheaper code: no stack of boundary marks and no truncation from the end, one mark per
boundary attempt, and for FIX the continuation is `eof`, one comparison. If the engine really
answers by the latest boundary, that is a defect of the engine against §8.2, found by the
two-boundary case the note proposes; it is fixed in the engine, the examples and corpora
re-read, and reported before it lands since it can change an accepted tree. If Igor prefers
the greedy order, §8.2 changes first and every rendering follows.
Settled the same evening: the engine answers by the first boundary, as §8.2 says; the note
had described it wrongly, there is no defect. The probe — a repetition whose continuation
holds at every boundary — is the two-boundary case in C2's tests, and the reader is held to
its three answers. The speller commit is on main (`f1baf4f3`).

**C2 written and reviewed (performance-ff, 2026-09-18, late).** Emitted code moves in three
grammars: FixGrammar's string and byte forms of the field lists, StockCount, RecoveringFeed;
FIX's windowed stream forms stay on the engine for now. The loop, per §8.2: at each boundary the
continuation as a part, then the turn as a part committed by resetting the ways it opened, a
missing continuation at the end a refusal, the broken element out of line and cold, writing
its record for the walk to call the recovery factory on. A rule asked again after failure no
longer re-enters the repetition (a hang found and fixed on the way: a retried `eol` way replayed
into the committed region). `RecoveringReaderTests`, five shapes incl. two `recover` in one rule
and the every-boundary continuation, compare positions and values in full. Approved with two
conditions: a literal synchronization is found by the C1 search, not by an attempt per
character; and the reader with `recover` over the buffered forms (FIX over a stream, D5/D7) is
the next slice, expr's search-branch patch in hand, owner to be agreed. The pair: slope,
One/Order, stock-count in three forms, first calls with the methods/IL column; push after it.
**C2 measured (stand, window 9, medians of five, paired; raw under
`benchmarks/results/pairs-2026-09-19`).** FIX's string form faster on every row: one field -18%
(198 to 162 ns), an order -14%, the slope rows -9..-19%, so about 102 ns a field against the hand
parser's 51 and step 1's target of 36; allocation identical; the first call nine methods fewer,
an empty message's 4.3 to 2.6 ms. StockCount's four-line case -26..-43%. Controls flat. Accepted,
to land; the byte forms, which C2 also changed and the stand did not measure, are paired next
and the byte part reverted if they lose. What remains of the gap is C3 and C4 — the arena and
the second pass the anatomy put at 85 of 131 ns.
C2 landed `779018c8`; the byte forms' pair is ordered against its parent.
**C2 as landed (stand, window 10, medians of five):** FIX's string forms -16..-22%, the stream
form flat, the byte forms +2.5..+6.5% (one field +6.5%, the slope rows +4..+6% against spreads of
3-4%). Checked before any revert: C2 changed no byte path at all — the in-place array goes
through the buffered-bytes input to the engine before and after, identical to the line, since a
buffered machine goes to the reader only where the engine cannot prove its release; C2's note
had said otherwise. So nothing is reverted; the byte rows are paired again with a PGO=0 twin and
profiled if the sign holds (a neighbour's JIT or layout, not an algorithm). Paired again
(window 11, identical byte code on both sides): faster on all eight byte rows, -2..-6%, flat under
PGO=0 — the opposite sign, the difference being which other rows shared the process. A rule for
the stand from it: the rows of one paired run share one dynamic-PGO profile, so a change that
touches some forms is read both in a run of those forms alone and in the run with the rest. The consequence that
matters: FIX's byte form, the one FIX needs, has not had C2. A whole array is not a stream and
has nothing to release; decided that it is read by the reader over a span of bytes as the string
is over characters, performance-ff, after the two quadratic defects and the scanner's search and
before C3, its design to the architect first.
**The design (performance-ff) and the decision.** The in-place array enters the same buffered
machine as the stream, and a buffered machine goes to the reader only where the engine cannot
prove it may release the window — right for a stream, wrong for memory, which holds everything.
A: the in-place entry gets a machine of its own, read by the reader over the same buffered input,
gated by readability alone; small, in one method, at the cost of a window check on each read.
B: a span-of-bytes input kind, read exactly as a string with a byte for a character; faster, but
a new input kind with many touch points. The language is not touched either way. Decided: A,
measured — the byte rows alone and then with the string rows, the first call with its methods
and IL, FixGrammar's size; if A keeps less than two thirds of the string's gain, B follows with
A's profile as its reason; otherwise B waits with D10.
Corrected by performance-ff's measurement while writing A: FIX's window release was not the
gate; an external recognizer given a view of the input (`@ReadData`) keeps a buffered machine
on the engine, since over a stream it could make the buffer fetch under the reader. Over a
whole array the view is the engine's own and nothing is fetched, so the in-place machine is gated
by readability alone. The same external is what the stream form's reader will have to answer.
FixGrammar grows 11% (four readers where there were two).
**A measured (stand, window 18):** FIX's byte rows -28..-30% alone and -23..-29% with the string
rows (one field -29.5%, an order -28.3%, sixteen fields -30.2%); string and stream flat;
allocation identical; the first byte parse nine methods and 1.8 KB of IL fewer. Far past the
two-thirds bar: A stands, B waits with D10.
**The stream form next (performance-ff's design, agreed).** The recovering reader over FIX's
stream form, under one invariant that is now a contract of the emitted code: across anything
that can fill the buffer — a read, a sub-rule, an external such as `@ReadData` — the reader holds
positions only, never a span, since a fill may move the buffer and return the old array to the
pool. It holds today (captures on the tape are pairs of positions, sliced by the walk after the
parse); it is written beside every place a span is made and beside the fill, and tested by a
stream read a byte at a time with an external that grows the buffer right after a capture.
**Found by the design, and open:** the whole-parse stream form of FIX holds the entire input
today, up to `maxRetained`, on the engine as on this reader (the `yield` forms, which the stand
measured, go a window at a time) — its values are built by the walk
after the parse, from slices of the input, so nothing can be released before. D5 is therefore
not met for it, and it is met only by C4, which builds at each turn's commit and can release the
window behind it: a stream of 10,000 fields under a retained window of a few kilobytes is C4's
acceptance test, not only its speed.
Written (`8d874d1c`, held for its pair): the contract stands in the doc of the emitter's cut, at
the search, the literal comparison and the match, and beside the emitted fill; a buffered
machine goes to the reader wherever nothing is released, the external included; C1's and C2's
searches run through the buffer's own search. FIX has no buffered engine left: six readers, the
file 626 to 521 KB. The in-place array now goes through the stream's machine, so the pair must
show A's byte gain kept, or the separate memory machine comes back.

**C3's design (performance-ff), approved 2026-09-19.** A new analysis beside `Replay` and
`Demand`, pure over the graph: for each reading site, the innermost enclosing point past which
what it read stands, or none (built after the parse, as now). A point is the end of a rule that
stands, the turn of a repetition that nothing after it can reopen (a `recover` repetition in
the scenario the reader takes, whose loop commits a turn by construction; or one `Determinism`
proves never gives back, in a rule that stands), or the end of an atomic group in a rule that
stands; the innermost wins, and a point counts only if every enclosing point stands, or a
factory would run for a node that was not accepted (§7.3). Inside a rule that stands, a point
only makes construction earlier; the one case where it turns "may not stand" into "stands" is
the recovering turn — FIX's field. Not points: an alternative, a lookahead, anything under a
rule that does not stand, an atomic group something after which can still fail. Held to a table
of shapes and to `Replay` (a rule point exactly where the rule stands); its internals are arrays
by node, not dictionaries of records; the causes report counts the sites with a point per
grammar, which answers what C4 gives SQL before its causes reach zero. C4 reads it.
Written (performance-ff), four refinements accepted: the analysis is public like `Replay` and
`Demand`, for the tests; a fourth kind of point, the caller's — a rule not kept itself whose
every call has a point is settled where the call is (FIX's tag, called twice), a greatest
fixpoint; a recursive rule whose inner call can be given back keeps its point too, since a point
says only when to build and C4 builds from the derivation that reached the point, on which a
reading given back does not lie (the architect had asked otherwise and was answered); "kept" is `Replay`'s
stands-or-losing, the grade `Auto` already builds early by, so a factory may run in a parse that
fails in the end, as now; and a rule read anywhere its reading is thrown away (a lookahead, an
argument, a seam's body, a recovery's synchronization) is not kept. Generated code identical;
generation unchanged. Sites with a point: FIX and Fix44 all, Web and feeds all, SQL:2023 49%,
T-SQL 1.7%, SQL-92 5%, EL 2% — so C4 is for FIX first, and why T-SQL has so few is counted later.
Found with it: `Replay` takes a nullable call for one that cannot refuse, but `eof` and a word
boundary are nullable and refuse, so a replaced reading can be marked as losing and built early —
a correctness defect, sql-39's before the reader's gate.

**C4's design for FIX (performance-ff), approved 2026-09-19.** C3 gives every building site in
FIX a rule point, but the field call, which has the recovering turn's; and a rule point is where
the immediate carrier already builds. So C4 is not a new carrier: the immediate carrier learns
the recovery scenario, with C3 as its gate — the one thing that keeps FIX on the tape today is
that the immediate carrier refuses any grammar that recovers. Three commits:
- C4a: a value built for a guard is the one the construction gets, where nothing between them
  writes the capture again (RFC 3339's date: three strings where six were cut), held against the
  known defect of a guard over a repeated capture.
- C4b: the immediate carrier reads a recovering repetition — the element built at the end of its
  own rule, pushed at the turn's commit, a failed turn putting the stack back, a broken element
  built on the spot; the read-again gate learns that a committed turn is never read again. After
  sql-39's fix of `Replay`'s nullable refusals, on which "kept" rests. With it go the value store,
  the walk, the log and its writes and watermarks for FIX; what stays is the ways, as long as the
  field opens any, and `Failure` and `Match`, which are the API. The expected figure per field is
  named before the pair: 55-65 ns a field on the string form, against 102 today, the hand
  parser's 51 and the target's 36 — the value store, the walk, the record writes and above all the
  mid-parse walk the switch guard ran for the tag on every field go; the two ways the field opens
  stay (the end's goes with sql-39's exclusive choice, to about 45-50; the switch's needs the
  reader to prove nothing in the field is retried). The first call about 73-75 methods. Outside
  55-65, an anatomy against the target code before any conclusion.
  Simpler than designed: the carrier's choice keeps a machine on the tape wherever any rule opens a
  way, and with sql-39's exclusive choice extended to a called `eof` FIX's field opens none, so FIX
  passes the existing gate unchanged; C4b is only the recovery scenario admitted, the broken
  element built on the spot and the gathered stack. The expectation, restated before the pair:
  38-46 ns a field on the string form — below the hand parser's 51, near the target's 36 — and the
  first call about 72-75 methods; the pair's base is main with sql-39's commits and without C4b.
  FIX's log form keeps two ways and the tape, a later item.
  **Its design, approved 2026-09-19.** Two ways hold it, and both must go. The first is general:
  the follow given to a lookahead's body is everything the caller's continuation can be, for a
  reason that is about the caller and not the body — a look is decided at its first match and
  rewinds, so nothing after it can ask the body for another reading, exactly as with an atomic
  group; the body gets nothing, and the union over a rule's other sites keeps the rest honest.
  The second is the shape `(?!N & any)+` followed by `N` — read until a delimiter, the commonest
  scanner there is: the turn begins only where `N` failed and the continuation is that same `N`,
  which the analysis cannot see today because a lookahead contributes nothing to a first set. It
  is decided by the leading node of what follows, per site, joined over sites, under two
  conditions that are the proof: the turn must consume, and a nullable continuation is unknown
  rather than followed, which keeps a streaming publication out of it. Each its own commit and
  pair, with the generation-time gate, the corpora and a byte-for-byte dump.
  **Measured, and the first way was not the cause (performance-ff, 2026-09-19).** With the
  lookahead's body given nothing, the log separator's way is untouched, no way anywhere in the
  solution goes (357 before and after), FIX's file does not change by a byte, and the only shipped
  effect is expected tables renumbering among themselves. The cause is elsewhere and is exact: a
  publication's root is given "anything may follow" unless it is a `parse`, and FIX publishes its
  field list four times, twice as a parse and twice as a yield — so the yield readings poison the
  parse readings of the same rule, and that travels down to the separator. A yield's continuation
  is the next step the driver asks for, not anything the current element could give back to (the
  sentence already approved for the recovery scenario), so it contributes nothing; not the end
  either, since a caller may stop enumerating. With that the separator's way disappears and the
  only way left in FIX is way 1's. `find` stays at "anything", with its being unanalysed written
  down rather than guessed. The lookahead change waits until FIX's one rule counted as read again
  more than before is explained; a checked-in test loses its example to it, and the factory counts
  say §7.3 still holds, so the claim is rewritten with a look whose body cannot be read silently.
  Explained: the movement is the report's accounting and not behaviour — the emitted file is
  byte-identical, the ways and rents of the whole solution unchanged, the carrier and the gate the
  same; what changed is which rules the reader counts as its own, since a look's body compiled
  silently leaves that account. So the two land together under one pair and one gate, the pair
  measuring the change that moves FIX, with each effect named separately in the message so that no
  one looks for a gain where there is none.
  **And it narrows what the carrier's gate means, which the old test was the witness for.** A
  lookahead over a rule that builds no longer keeps a grammar on the tape by itself; six probed
  shapes say so, and the three that still keep it keep it for other gates. What keeps the tape is
  not a reading that is thrown away but something already built for it, and where a look's body
  builds nothing there is nothing to hold back — a guard reading a built value does not change
  that, and a host-decided switch in the body goes immediate although it is not silent, so
  silence is not the gate either. The old test is retired rather than dressed up, its grammar
  becoming the witness for the opposite with the factory counts behind it; the tape-keeping claim
  keeps its own witness in the test beside it. The narrowed meaning goes into the code beside the
  gate, into the commit's message, and into `status.md` and `implementation.md` in the same commit.
  **Committed, and the gate ran the other way:** the generator got faster, not slower — T-SQL's
  generation 0.90x, SQL:2023 0.94x, no host outside tolerance. A narrower follow set is less to
  carry round the fixed point and less for every later question to chew on, so precision here buys
  generation time where D11 B's new analysis cost it; worth remembering when the next analysis
  change is weighed. The separator's way is gone (every way removed in the solution is that one),
  no grammar changed carrier, every suite and corpus green, and the carriers report's whole diff
  is four rows. **Way 1 written, and FIX leaves the tape altogether:** the report goes from tape,
  held by a rule read again, to the immediate carrier with nothing holding it, and the solution's
  grammars on the tape fall from 43 to 42. The turn led by a lookahead was the last thing left once
  the separator's way had gone — so this is D13's goal for FIX reached, not only the log form's
  second way. The lead is a third field on the continuation rather than an analysis of its own,
  since the emitter threads continuations exactly as the follow does and one field cannot drift
  from the other; an unknown lead is the default, so the sites nobody reasoned about claim nothing;
  a node that may read nothing answers unknown; and the containment the fixed point turns on had to
  be a real ordering rather than equality, or a rule would have been re-enqueued for ever. Five
  conditions before its pair, a carrier change on a shipped grammar being behaviour and not speed:
  a differential against the hand parser over messages including broken ones, the factory counts
  per field against the tape on accepted and refused input, D5's retention under a small window,
  the locations unmoved, and the first call with its methods and IL. **Measured (window 65): the log
  form halves and now costs what the plain form costs** — an order 1,297 to 661 ns against the
  plain form's 663, a hundred and twenty-eight orders 77,161 against 77,440, the byte and stream
  forms -40..-54%, every row in one direction, the same direction without the profile's help, and
  the log rows' first-call method counts falling to the plain rows'. **Two of the three things
  named in advance were wrong, and the why is the useful part.** The plain rows did not move at
  all: the carriers report answers whether *any* machine of a grammar is on the tape, and it was
  read as if it described them all — the plain readings had neither a walk nor a way before the
  change, every one of the ten in the file belonging to the log machines. And allocation did not
  fall: the tape's records live in the pooled arrays the reader rents, not in per-call bytes, which
  is the same code the pool-retention work is about. The rule taken: the report says whether, the
  emitted file says which, and a prediction needs the second. Not landed until the missing
  condition is answered — there is no malformed wire for the log form, so "does the immediate
  carrier build for a reading later given up" has been measured only for the form that did not
  change carrier. Named for later: the byte and stream log forms remain 13-15% above their plain
  counterparts where the text forms have converged, which is the multi-character separator and a
  candidate rather than a defect. **Landed `098ee374` once that condition closed:** on a malformed
  log wire the bytes a call are identical on both sides in all fifteen runs, so nothing is built
  for a reading given up — with the caveat, named by performance-ff rather than glossed, that a
  byte count sees a per-call field and never the tape's pooled records, which is why the
  differential and the factory counts carry the rest of that claim. FIX now emits no ways at all
  and its file lost 52 KB of 404. **D13's goal is reached: the generated FIX parser does what the
  hand-written one does, by the same route** — and faster than it on every form but the lazy one,
  where they are level. What is left of a walk in FIX is the two yield steps, one per element,
  which the in-memory lazy form takes next. **And a second rendering found
  behind it, approved 2026-09-19:** a `yield` publication is read by methods only over a buffer,
  because that is what the driver had been taught first, so `ReadFields` from a reader is carried
  immediately while `ReadFields` from a string runs the engine, with an arena and a materialization
  per element — one publication meaning two things, and the slower one is the side a caller with
  the message already in memory uses. Nothing about a step needs a buffer: it is a repetition of
  one turn whose continuation is the next request of the driver, and the in-memory driver already
  exists, since it is what drives the engine today. The condition is lifted after the two pairs
  land, with a stand row for the in-memory lazy form taken first, the differential given that form,
  **Corrected by performance-ff an hour later, before anything was planned on it:** nothing calls
  the in-memory lazy form — FIX's lazy entries take a reader or a stream, its in-memory entries take
  the eager array form, and the overload the emitter writes beside the buffered one is entered by
  nobody here. So for FIX as shipped the gain is code size and not time: that overload is plausibly
  the last thing keeping an automaton in FIX's file at all. The change is made anyway, after the two
  pairs, on the consistency of what a publication means and measured by size rather than by a time
  nobody here would see; if the automaton does leave, FIX becomes the first of our grammars with
  none. The speed of it belongs to a consumer calling the in-memory lazy form of their own grammar. **And a gap in the stand found
  by it:** — and, landed as `d6fad9e1`, it did what was promised before it was seen: the log rows'
  slope -8.0% on the string form, -5.1% on bytes, -4.4% over a stream, against a named -3 to -8%,
  and whole messages better still at -6.5..-10.6%; about nine nanoseconds a field. The plain FIX
  rows did not move, and allocation is identical to the byte on all forty rows, which answers
  whether anything began building early: it did not. A control that read +5% was settled without a
  run, since the Web parsers' emitted code is byte-for-byte the same on both sides and identical
  code cannot be slower. **And a gap in the stand found by it:** there were no rows for the log form at all,
  so nothing measured the form this work is about until the stand added eighteen. The rule that
  follows: before a form is optimized, it has a row.
  **The whole coverage counted (stand, `docs/design/stand-coverage-2026-09-19.md`):** no span form
  anywhere has a row; of FIX's message layer one parse method of twenty-one is timed and its
  stream, reader and lazy forms none; the Web has rows for twelve publications of thirty-four; SQL
  has six of forty-two rules in the standard, one of thirty-one in T-SQL, nothing for SQL-92, no
  row for the window form anywhere and none for the token scanner the split grammars publish (the
  stand first called it the search publication and corrected itself); of thirty-six examples three are measured. Added in this order as hands are free,
  pairs first: the positional and window forms of the SQL parsers (the path expr is changing now),
  a row that loops the token scanner, which today hides inside every SQL and EL row,
  the message layer's stream and lazy forms, the Web's list publications
  into the linearity family, one span row per library, and the examples' lazy feed reader.
  Coded and pushed the same evening: fourteen paired rows — the positional and window forms of the
  SQL parsers, the token scanner looped over a text in four grammars, the message layer's stream,
  reader and lazy forms, a span row for each library that has one, the examples' lazy feed reader —
  and five Web list series in the linearity family. Their baselines are not taken yet, so no figure
  for these forms is quotable until they are; the "hand" side of a paired row here is the same
  form built from the other tree, a control and not a hand-written parser.
  **Their baselines (window 62):** the token scanner 378 ns over twenty selects and 137 over an
  expression ladder, allocating nothing; the positional form 17.7 µs and the window 18.3; the
  message layer about 2 µs a message over a stream or a reader. Two findings came out of rows that
  had never been read. The lazy form over a string costs 2.8-3.2x the same form over bytes held in
  memory — the string side runs the engine and the byte side the methods, which is the change
  already approved on consistency and size, now with a number — and the number moved the diagnosis:
  the split is not a stream against memory but characters against bytes, both in memory. The byte
  overload borrows the caller's array through the buffered input and so got the methods for free;
  a string cannot be borrowed as an array, so the character overload was left with the engine —
  nobody decided it. Which rules out the tempting small fix, a memory constructor for buffered
  text, since that would copy the whole input to reach a path whose purpose is releasing what it
  has passed. The remedy is the approved one: the ordinary span reader steps the lazy form over
  memory, with no buffer and no copy. Whether the byte overload should then drop its buffer too is
  measured, not assumed, and is a commit of its own. And the header of acceptable media
  types allocates quadratically, 63.8 MB at ten thousand ranges with an exponent of 2.07: a
  shipped API. **Diagnosed (finance-24), and it is not a curve but a cliff, and not the grammar's
  but the generator's:** the allocation is exactly linear to three thousand ranges and jumps
  twentyfold between three and four, then goes on linearly at a worse price. The parser pool keeps
  a parser only while its arena stays under 65,536 entries; a range costs about eighteen, so the
  limit runs out exactly there, and every later parse builds its machinery afresh. Not guessed: the
  constant was raised in a throwaway build and ten thousand ranges went from 63,843 to 5,391 KB and
  from 18,803 to 3,461 µs, the exponent from 2.07 to 1.00, and the build was thrown away. Raising
  it is not the fix — the note beside it records that 4,096 was raised to 65,536 for an ordinary
  document and that trimming the tables instead was measured and was slower — so the question is
  how to keep both properties: a repeated large parse reusing its parser, a single one not holding
  it for ever. performance-ff designs it (his suggestion: hold an oversized parser weakly), after
  the refusal's rent and before the lazy form; the scaling test is his, at the generator's level,
  so that it fails on his own code. **The design, decided:** an outsized parser is kept by a weak
  reference per thread rather than dropped, and looked for after the strong slots. A repeated large
  parse finds it and the cliff goes; a single one leaves it to the next collection, so no thread
  retains anything, which is what the bound was for. Its limit is named rather than hidden: the
  parser survives only to the next gen2, so a large parse once a minute still rebuilds — and that
  is not the workload that hurts. Raising the constant is refused (it moves the cliff and buys the
  move with retention), trimming on return is refused (the comment beside the constant already
  measured that as slower), and a second bounded slot is written down as where this extends if a
  parse ever grows so large that even a weak reference to it matters. The test is an allocation
  ratio rather than a time: the same document parsed twice on one thread, the second parse
  allocating a fraction of the first, with the twin below the bound to catch a regression the other
  way. **And the stand corrected its own word for it:** three points ten times apart measure a
  slope, not a shape, and a cliff between them reads as a square; a flagged series is now retaken
  with points in between before its exponent is believed. **A separate finding beside it:** the address list is linear in
  memory and superlinear in time from a thousand addresses on, before any collection happens, so it
  is an algorithm and not the collector; suspected in the arena's removals. **The profile did not
  support that and finance-24 withdrew it:** there is no arena in that reading at all, and the
  growth per address fades as the input doubles — 1.35, 1.45, 1.18, 1.08 — which is a working set
  outgrowing a cache and not a square, which would double and never fade. Said as a reading of
  numbers rather than a measurement: hardware counters were not used. **But a second cliff of the
  same kind turned up beyond where the stand measures:** the reader's own pools keep nothing past a
  million array elements in total, so at twenty thousand addresses the memory jumps fivefold and
  goes on linearly at the new level; raised in a throwaway build, the cliff disappears and the
  line is straight to eighty thousand. So there are two bounds of one kind, the tape's and the
  reader's, and performance-ff designs for both at once — one rule for keeping, not two numbers
  with separate fates. **The rule, approved:** a pool never drops what it has grown; past its bound
  it holds it weakly instead of strongly — three places of one shape, the parser's recycling and
  the two reader pools' returns. The reader pool needs no rule of its own: the check on return is
  the one it already makes, and only the branch that does nothing today stores into the weak slot;
  the weak slot is read only when the strong spares are empty, which a loop of ordinary parses
  never reaches. Being shared by every form argues for one rule rather than against it. Refused and
  **Revised before either was written, on finance-24's objection:** a weak reference is cleared by
  any collection, and between two parses of a real program there is other work that allocates — so
  the reuse it promises holds only inside a quiet steady state, and any foreign collection knocks
  it out, turning a reliable twentyfold jump into an intermittent one, which is worse to diagnose.
  It is a strict improvement and not a guarantee, and performance-ff said so rather than defending
  the design he had proposed. Taken instead: the outsized store is held in a slot of its own and
  released when some small number of consecutive parses have not needed it — retention measured in
  the thread's own work rather than in the collector's mood — and, when that count expires,
  downgraded to a weak reference rather than dropped, so that a thread which parses one huge
  document and falls idle does not hold an arena for ever, which is what the bound was written for.
  The number is to be justified as a statement — eight consecutive parses that did not touch the
  large arena mean the work has changed — and the test measures that statement. The test also gains
  work between the two parses, since one that parses twice back to back cannot tell a mended
  behaviour from a mended test, and it states what the design does not promise as well as what it
  does. Refused and recorded rather than rediscovered: handing the outsized arrays to the runtime's shared array pool
  would be writing "drop it" in more words, its own maximum pooled array being the same million
  elements these bounds are written in. Each bound's comment is rewritten to say what now happens
  past it, keeping the history of why the number is what it is. The buffered reader's pair passed (the
  byte lean was the profile's, gone alone and under PGO=0); it lands with C3.
  Landed: the buffered reader `6a500f21` and C3 `f5658882`, with the commit-point column in
  `carriers.md`. C4a waits for its pair; C4b is being written.
  C4a paired (a cookie -40..-48 B, EL's immediate loop -320 B, the rest identical). C4b written
  (`ba6bc238`, held for its pair): StockCount and the recovering feed move to the immediate carrier;
  FIX follows when sql-39's exclusive `eof` lands, since its field still opens one way on main.
  Two corrections made while writing it: the gate asks that every construction of the machine has
  a rule point — a call only hands up a value built at its rule's end; and lines and columns are a
  cursor the reader keeps, not a refusal. Against the tape it builds the same, and one tag more
  on a broken field (the losing grade, allowed). Two recovering repetitions in one rule stay on
  the tape, by the shared-stack fix (`4f5457dc`).
  C4a landed (`f77f5c10`). sql-39's exclusive `eof`, on which FIX's move rests, removes only work
  from FixGrammar — both way-back loops, the switch's recorded selection, the end's way — yet reads
  +5..+11% on FIX's string rows, most likely the inlining a separate body used to get. If the lean
  survives a control alone and under PGO=0, it lands only with C4b, paired as a stack against main
  on C4b's promised 38-46 ns; the shape of a method with no way back goes to performance-ff.
  **A counting defect, and what it hid:** since the shared-stack fix, the causes report counted only
  machines the immediate carrier could carry, so SQL's machines, now refused for gathering two
  members onto one stack, dropped out of its columns silently; generated code unchanged. The report
  is fixed to show such a refusal per rule. What it shows matters more: every SQL:2023 and T-SQL
  machine is refused by that conservative rule, so nothing Q7.1 and C3 opened for SQL can reach the
  immediate carrier while it stands. Marks per gathered member, left "for when a measurement asks",
  are asked for: performance-ff, after C4b and FIX's log form, with the count of SQL machines held
  by that refusal alone first.
  **Corrected by the fixed report (`ea818170`):** the refusal holds three SQL machines, not all —
  two in SQL:2023, one in T-SQL — and each would be kept on the tape by `Replay` anyway; none is held
  by the refusal alone. The architect's conclusion was drawn from the broken count. Marks per
  member stay in the queue without priority; the report says when a machine is refused and nothing
  else. What holds SQL is `Replay`, per machine: C4b's gate asks every construction of a machine for
  a rule point, all or nothing, so SQL's 75% of sites with a point reach the immediate carrier only
  when a construction's carrier can be chosen alone — a design question for after FIX.
  **C4b landed (`6df44160`, window 33):** StockCount's string form -14..-26%, the recovering feed
  -23..-27%, allocation identical (the saving is the walk), first calls five and eight methods
  fewer and 17-25% faster; the buffered forms wait for C4c. sql-39's `eof` lands only stacked with
  C4b's FIX move (its lean at default PGO is the string entry's; the code is faster under PGO=0).
  Found on the way: FIX's tag is an atomic group whose inside opens no way, yet the reader wrapped
  it in a segment, a retry loop and a seal — the last `ways.` in FIX's immediate reader, for which
  every call rented the ways. An atomic group that opens no way is written as the call alone; its
  own commit and pair, stacked with `eof` for FIX's row.
  The JIT's view of the `eof` lean (stand): with the body merged into one method, the string path's
  hot code grows 55% while the byte path's shrinks 24% — the shape of that method is
  performance-ff's to settle in the stack.
  **The stack measured (stand, window 36), FIX's string form per field:** main 95.6 ns; with `eof`
  62.7; with no ways where nothing opens one 63.5; with the recovering element kept a method of its
  own 49.5 — against the hand parser's 60.8 in the same run. **The generated FIX parser reads a
  string 19% faster than the hand-written one**, -48% a field, one field 151 to 81 ns. The promise
  was 38-46 and is missed by a few nanoseconds: under PGO=0 the last two read the same, 78.3, so
  the last step is the tiered JIT's and not the emitted code's. Lands as one stack, `eof` carried
  with its author. The byte rows stay at 1.65x the hand parser: the in-place array goes the
  buffered path, on the tape until C4c, which is next. The rest to the target (the field's and the
  tag's constructions, the stack's push) after it, with an anatomy.
  Landed `9d5b0846..9c2237da`.
  **C4c's design, agreed:** (1) the buffered machines were made without the file's carrier and
  without `Replay`, so every one stayed on the tape — the reason the in-place bytes and the streams
  did not move; they are handed both, under the span contract (expected: FIX's bytes 55-65 ns a
  field against 96, the hand parser 58). (2) Release before each turn of an admitted recovering
  repetition in the whole-stream form, where nothing holds a position from before the turn to the
  rule's end; lines counted as released; the entry's starved check over a released position fixed.
  A factory cannot keep a reference into the buffer through the generated API (captures are spans,
  the input view a ref struct, `parserInput` refuses a buffered form). Gates: D5's skipped test
  un-skipped, a stream larger than `maxRetained` answered, and the held buffer the same at 10,000
  and 100,000 fields — the result grows, the input does not. (3) FIX's log form after it.
  Step 2 written (performance-ff). Its gate is FIX's own: the whole-stream parse over a reader and
  a stream, 10,000 and 100,000 fields with a broken one every hundred, `maxRetained` of 1,024 —
  answered with release, an error without it; a hard bound rather than a slope. The D5 test that
  stood skipped already passed on main (its grammar is read by the streamed parse, which lets each
  row go); the skip was stale and goes.
  **D5 in numbers (stand, held memory on the whole-stream parse itself, a collection inside every
  second read):** over a stream, 10,000 and 100,000 fields, 772 and 9,125 KB held before step 2,
  728 and 7,065 KB with it; over a reader 1,024 and 11,162 against 732 and 7,091. With step 2 the
  held memory grows 9.7x for 10x the fields — the result array, about 70 B a field, and nothing
  else; without it, about the input on top. Each step lands after its own time pair.
  **Step 1 landed (`d194c138`, window 38):** FIX's in-place bytes 96.3 to 53.6 ns a field against
  the hand parser's 58.3 — faster than the hand parser on bytes too; the whole-stream forms 103 to
  57 (stream) and 104 to 59 (reader); StockCount's readers -18..-20%. Pushed after a rebase over new
  source without re-verifying the combination — a slip, reported, main verified after. Step 2 read
  +2..+12% on FIX with no lines to count: a turn should only record where it may release, the work
  done in the fill when the buffer needs room; profiled before its pair.
  Reworked so and paired (window 45): the whole-stream rows inside the A/A floor, the held memory
  kept (7,065 against 9,128 KB at 100,000 fields), StockCount's readers flat or faster; one row
  leans, a 64-character buffer over good lines (+5..+9%), since each compaction now counts the lines
  it lets go and so small a buffer compacts on every fill. Lands with the row named; compacting
  only once half the buffer is released is a later step of its own.
  **Why FIX over a stream is still 2.2-2.6x (2026-09-19):** FIX's public stream API returns an
  enumeration through the `yield` form, which still reads a field at a time the old way; all of
  C2-C4c went to the whole-stream form, 57 ns a field, which the public API does not use. **Igor:
  the `yield` form on the reader next for FIX**, after C4c's second step and before the log form —
  performance-ff, with expr's driver and step-5 patch, a design first; expected 55-60 ns a field.
  **Measured (stand):** the stream rows -37..-72%, 217 to 65 ns a field, allocation per call
  identical — past the expectation. The ideal reading has a column in the family table now, and
  the table says in words that bytes and streams have none, so their figures stand against the
  hand parser alone. Landed `f457de98`: 217.3 to 64.5 ns a field over a stream and 215.4 to
  65.0 over a reader, allocation the field and nothing else (96 B), the first call 7.05 to 4.88 ms
  with 27 methods fewer, and -30..-45% still there at PGO=0. The expectation named beforehand was
  55-60 and the result is 65 — a tenth short of its own range; the hand parser's lazy form is about
  68 ns a field, so the generated one has drawn level rather than passed it.
  **Design agreed:** `yield` is lowered to a step rule, one recovering turn, which the driver
  calls once per element and the engine reads today. The step is admitted as the recovery scenario
  with its turn as its point; the reader writes it without a loop or a continuation, since the next
  step is the continuation; a yield publication is read by methods wherever its machine can be; the
  existing carrier gates then carry FIX's step immediately. The driver stays, making a reader frame
  per step; nothing is allocated per element. Expected: FIX's stream rows 213 to 55-60 ns a field.
  Found writing it: the graph's reachability skipped a recovery's synchronization, so a rule called
  only from there was missing from buffered machines — and the emitter builds every machine's rules
  and asks the release checks from it, so a lookbehind there would go unseen and a form release
  when it must not. Fixed in the graph, in the `yield` commit. The call graph `Replay` reads skips
  synchronizations too, so a rule read only there is answered "stands" by default and `Auto` could
  build a reading that is thrown away; fixed in its own commit right after, with a count of the
  grammars where a synchronization calls a building rule. Checked first: none in the solution
  does, and the danger is not reachable — a synchronization exists only where there is recovery,
  every recovering machine passes C4b's gate, and that gate asks `Commit`, which counts what a
  synchronization reads as thrown away. A regression test holds it now; `Replay`'s call graph
  walks synchronizations later, after FIX's log form, as a correction of the analysis.
  **A correctness defect found on main while writing C4b:** the immediate carrier merges two
  members of one rule gathered onto one stack — `a: X* & ';' & b: X* & eof` over "ab;cd" gives
  a=[a,b,c,d] and b=[] where the tape gives [a,b] and [c,d]; `Auto` picks immediate there. Fixed
  first and conservatively: such a rule keeps the tape. The list of grammars that change carrier,
  and for each shipped one whether it ever returned a wrong tree, comes with the commit.
- C4c: release before each turn in the whole-stream form, where nothing holds a position across
  the turn and no factory keeps a reference into the buffer (a memory or an array, which a span
  cannot be); D5's retention test un-skipped, and a stream larger than `maxRetained` answered
  where today it is an error.
StockCount with real names: good lines -53% on the string form, **0.97x the hand parser**, -45%
on the stream form; broken lines -8..-24%, still 3.3-4.0x the hand parser until `LineAt`.
Controls flat.
**Found by the same window: StockCount is quadratic.** Nanoseconds a line, generated string
form, no broken lines: 196 at 4 lines, 433 at 100, 1,479 at 1,000, 22,008 at 10,000 — n^2.2
(the stream form n^2.3); the hand parser linear. The same with broken lines, so not recovery;
the same before C2; the continuation fails on its first character, so not that. A parser that
reads a feed in quadratic time breaks D5's premise outright: it goes to performance-ff ahead of
C3, with a test in the slow project that holds the ratio of 10,000 lines to 1,000 below 15, and
FIX checked on long messages for the same. The stand gains a linearity family: every parser
with a long input at 1x, 10x and 100x, its exponent in a column, flagged above 1.2.
**Its cause (finance-24, the same night):** the generated `LineAt` counts newlines from position
0 on every call, and a recovery `=>` reading `parserLine` calls it for every rejected element; good
lines are linear at about 77 ns, twice the hand parser (the stand's "good" input may itself be
read as broken, being checked). Worse, the buffered `LineAt` reads from 0 through the input's
`Get`, behind what the window has released — a correctness fault of D5, not only a cost; `ColumnAt`
has the same shape. Decided: the parse state keeps a (position, line) pair, and the last
newline's position for columns; a call at or past it counts on from it and moves it, a call
behind it after a backtrack counts from the nearest kept point at or before it, never from 0;
on the buffered path the lines are counted as the window is released, and `LineAt` reads only
what is held. performance-ff, before C3; tests in the slow project: linearity with broken lines
on both forms (finance-24), and a stream with a small retained window whose broken line
10,000 reports line 10,000.
**Landed `a8fa877a` (performance-ff):** a line cursor in the engine's parser and local in the
reader's walk, counting back only the distance; the buffer counts lines as it releases and reads
nothing behind what it holds. Found and fixed with it: the `yield` and `find` drivers held the
whole input to be able to answer a line, which D5 forbids; they release with line tracking now.
Tests: every rendering's location against a count from the start, and in the slow project
10,000 lines through a 64-character window, the bad element at 10000:4; finance-24's scaling
test lands on it.
**Corrected by the stand the same night:** its "good" input named items with digits, which the
grammar reads as broken lines, so every StockCount row it had timed, C2's good and broken rows
included, was the rejection path; agreement passed because both parsers refused alike. With
letter names: good lines linear at about 81 ns a line, twice the hand parser, both forms; every
tenth line broken: the generated parser n^1.9 (43x the hand at 10,000 lines on the stream form),
the hand linear — so the quadratic is exactly the rejection path, as finance-24 found. The rule
taken from it: a stand row states what it reads and refuses to be timed when its input is not
that ("good" has no unreadable line, "broken" exactly a tenth); agreement between two parsers
is not evidence that the input is the case it claims.
**The linearity family (stand, `benchmarks/results/linearity-2026-09-19.md`; rough, 31 series,
three sizes ten apart, flagged above an exponent of 1.2; every row of the stand now asserts
what it reads, and all 77 passed unchanged).** Linear: FIX in every form, generated and hand; the
URL path, media-type parameters, structured lists; T-SQL's columns, conditions and INSERT rows;
the expression language on both carriers. Flagged, each to be diagnosed with a reproduction
before a cause is named: StockCount's rejection path (known, performance-ff); the feed example
(1.55 in the last step, no hand parser; finance-24); SQL:2023's search condition (1.38 and 1.21,
112x the hand parser at 1,000 predicates, where T-SQL's is linear; sql-39); a JSON object
(generated 1.36, and the hand parser 1.65 — worse than the generated, while System.Text.Json is
linear; finance-24). The family runs with every baseline from now on.
**SQL:2023's search condition, diagnosed (sql-39, a reproduction and dotTrace):** the generator,
not the grammar. Even `a0 + a1 + …` alone is quadratic (exponent 1.98). The towers' guard over a
gathered list — `first: Operand & rest: Operated* & when @(Towers.Common(first, rest))`, six such
places — has the reader materialize `rest` element by element, each from the mark of the whole
rule, walking the rule's whole log, and each clearing the live table over every record: n
elements times O(n). T-SQL has no guard over a gathered list there and is linear; no grammar
form avoids it, since a fold with a guard per step materializes from the rule's mark too.
Decided: the materialization for a guard over a gathered list costs O(the list) in total —
one walk for all its roots, or each from its own record, performance-ff's choice — and the
live table is cleared over the range the walk uses only. performance-ff, after `LineAt` and
before C3; the reproduction becomes a linearity test in the slow project; the stand pairs the
SQL rows and a 1,000-predicate search condition against the hand parser's 553 µs.
**The other two flags (finance-24), neither the generator:** the feed example's own growing
`List`, whose last array went to the large object heap on every read at 10,000 records — an
array of the rows' length instead, 3,075 to 1,603 µs, no gen2; and the hand JSON parser's lists
that doubled and were copied — now two stacks per thread reused across texts and an exact array
per closed value, an object member 270 to 97 ns at 10,000 members, below the generated parser's
261, and an array element 53 to 15 ns, linear. What is left on a large object in both parsers is
the result's own members array on the large object heap, a cost of the model. The linearity
family gains a gen2-per-call column so that it tells the collector from an algorithm.
With it (stand, rerun): every other flag reads as the collector — linear allocation, gen2 at
the largest size; the two algorithms left are StockCount's rejection path (time 1.9, allocation
linear: CPU only) and SQL:2023's search condition, whose allocation grows with an exponent of
3.1 — 164 MB to read 1,000 predicates, gen2 on every call — so the gathered-list fix is accepted
only when both time and allocation are linear.
**The fix in two parts (performance-ff).** The walk: a guard over a list builds all its
elements in one walk that takes the list as its roots, and the live table is cleared from the
walk's first record only — the reproduction's 1,000 terms 37 to 23 ms, 2,000 terms 90 to 34 ms,
time and allocation linear from 1,000 to 2,000; landing alone. What is left is the value store's
shape: SQL:2023 has 302 value tables, sparse by record number because a guard's machine builds
while it reads; every `Room` grows all 302 to the record count, and the store is dropped when it
passes a million cells in total, so past about 3,500 records every parse allocates all of it
again — 164 MB at 1,000 predicates, 160 KB a predicate. Decided: a dense store for machines with
guards too — a map from record to slot and values in arrays as long as what was written,
rewound by dropping the slots above the mark — and the drop threshold per table, not in total;
the store costs what is written, not records times tables. sql-39 (the store is his from D2);
accepted when time and allocation both have an exponent of 1.1 or less.
**Measured (stand, window 14), not accepted as it is:** 100 and 1,000 predicates -76% and -80%,
21 MB to 162 KB and 168 MB to 1.6 MB a call, generation unchanged — but every EL row on the tape
+10..+23% and the short SQL rows +9..+42% (a column +42%, one select +20%), the long ones flat: a
fixed cost per parse on small inputs, which are most of what people parse. The criterion is
both at once: small inputs within their spread and large ones linear. First the row mix is ruled
out (EL and short SQL alone, a PGO=0 twin); if it holds, the fixed cost is found by profile and
removed, or the store becomes adaptive (sparse up to a record count, dense past it, or dense only
for machines with a guard over a gathered list).
**Reworked (sql-39, `e4ad4638`, replacing the dense store, held for its pair):** the row mix was
ruled out, and the profile put the cost in the dense form itself — a call to add each written
value and an indirection to read each one, in materializers too large to inline. So the store
keeps indexing by record and grows a table only where a value is written into it, instead of
growing all 302 in `Room`; it is dropped per table, and no longer sums every table's length on
every parse. Small grammars' stores and every snapshot unchanged. Locally: a column -12%, one
select -12%, EL unchanged; 1,000 predicates 35 ms and 164 MB to 4.4 ms and 1.6 MB, 2,000
linear, exponents at most 1.08; a scaling test in the slow project.
**Measured (stand, window 19):** every SQL:2023 row faster — a column -14%, one select -17%,
twenty -25%, values -26%, 100 and 1,000 predicates -85% and -86% with 21 MB to 162 KB and 168 MB to
1.6 MB a call; EL flat; the same under PGO=0; generation within 1.05x. Accepted; the quadratic of
the search condition is closed.

**Step 1's number (stand, 2026-09-18 18:17).** The target code, written by hand as the design
says the reader would emit it, per field: generated 185 ns, hand 53, ideal 33, **target 36** —
0.67 of the hand parser and 1.1-1.2 of the ideal, allocating exactly what the generated parser
allocates. The design is worth what the anatomy said; steps 2-3 go. One figure to settle before
any ratio is quoted: performance-ff's harness reads generated/hand at 3.5x where the stand reads
2.4x on the same main.

**Then the design**, from that table: what the emitted code for `Fields` would have to be to
match the hand parser line for line — a loop with no arena for a grammar whose only way back
is `recover`, values built as they are read (D3, `Demand`), the separator found by a scan —
and which of the generator's parts stand in the way (the engine for `recover`, the two-pass
shape, per-call setup). That design comes to the architect and to Igor as a question about
the architecture before anything is written.

## D14. The normalizer is two things: building the graph, and optimizing it

Decided 2026-09-18 by Igor, on the architect's account of the generator's steps. Normalization
(step 3) builds the plan of the parse as a graph that can be optimized; the lexical split (step
4) then changes that graph — rules are replaced, some disappear — so what was optimized may need
optimizing again. Today the split re-runs one pass (`FactorCommittedPrefixes`) and the analyses
run again implicitly over the kinds graph; the other passes do not run again, which is why a
grammar can carry a `GRAM4016` over characters that would not hold over kinds.

**Decided:** the normalizer is divided into building the graph, run once, and optimizing it,
a set of passes that are idempotent over a graph and can run any number of times — after the
split, and after any later rewrite. Step 5 (choosing the rendering and the mechanisms each
machine gets) is where performance is decided and is kept apart from both.

**First, the inventory** (expr-2d, after Q7.2 step 3; performance-3f stays on FIX): every pass
of `GrammarNormalizer` classified as building (settles meaning: specialization, conditions,
left recursion, nullability, types, implicit captures, trivia, results) or optimizing
(transparent rules, factoring, pruning, whatever else changes shape and not meaning), with
what each reads and writes, whether it is idempotent today, and what order the optimizers
need. Then a design: `Optimize(graph) → graph` called after `Build` and after the split, with
the emitted code of every grammar byte-identical when nothing new is found, and the SQL:2023
kinds graph as the case where something is.

**Inventory and design landed (expr, `44b03532`, `3857460f`):** thirty-one passes classified,
five of them optimizers; stages Build → Optimize → Analyze → Check, the same three again over
kinds after the split; optimizers keep the language, members, types and node facts, return
bodies by reference when nothing changed, and report nothing — `GRAM4016` is said by Check
from Factor's declines, after the last Optimize. Two present defects found by the inventory go
first: `GRAM4007` reported up to three times, `LowerYieldPublications` not idempotent.
**The architect's answers to the design's two questions** (Igor may override): (1) step 6 changes
the split grammars' emitted code, and that is D14's purpose — one step, measured on the stand
and gated by the agreement tests like any emitter change, no flag (a flag would be an option of
Q3's kind); (2) the four rewrites inside `LowerAll` move into Optimize, as a step of their own
after 6, byte-identical, since Optimize is meant to be complete.

**Step 3 landed `6fde0ca0` (expr):** the two shape-changing passes that also spoke no longer
do — the forwarding of an `on fail` message is a builder run before the collapse, and
`Factor` records its declines for a check to say as GRAM4016 after it — so an optimizer can
run again over its own result without saying anything twice. 944 files identical, a guard test
for a diagnostic said once over a fold. Next in the same step: Q7.1's C, counted on sql-39's
counter after A and B.

## D15. A heuristic is for a scenario, never for a grammar

Decided 2026-09-18 by Igor, on the FIX reader design. Two layers are kept apart in the
generator's choices (step 5 of its work: which rendering and which mechanisms a machine gets):

- **What is proved** decides correctness and is never overridden: where a reading stands
  (§7.3, a construction once per node of the accepted derivation), what a repetition may not
  give back, what a lookahead settles. An analysis that cannot prove answers "no", and the
  general machinery stays.
- **What is chosen** among correct renderings is a heuristic, and a heuristic applies to a
  *typical scenario* recognized in the grammar's shape — a repetition whose only way back is
  `recover`, a run to a stop set of a few characters, a choice of many literals, a rule that is
  a token — never to a named grammar. FIX is the first instance of the recovering-loop
  scenario, not its owner: any feed grammar of that shape gets the same code.

**What it binds.** Every heuristic in the generator names its scenario and its trigger (the
structural test that recognizes it), in the code and in `implementation.md` §4's list; a
change that would key on a grammar, a host or a rule name is refused. Such scenarios are to
be sought deliberately: where a proof is out of reach, a recognizable shape with a cheaper
correct rendering is the next best thing, and the catalogue of scenarios is a deliverable of
its own (performance-ff, after the FIX steps: the scenarios the generator recognizes today,
each with trigger, rendering and the grammars in the repository it fires on).

## D16. Three hand parsers for the Web grammars

Decided 2026-09-18 by Igor. The reader over characters — the rendering most users' grammars
get — has had no hand-written yardstick: FIX measures the recovering loop on the engine, EL and
SQL the reader over kinds, and a regular expression does less work than a parser and is slow on
its own. Three hand parsers are written, one per shape the other families do not cover:

- **RFC 8259, JSON** — recursion, strings with escapes, numbers; with `System.Text.Json` as an
  external reference reading beside it, as ScriptDom is for T-SQL.
- **RFC 3986, URL** — runs and character classes; the regex transcription and its five inputs
  already exist.
- **RFC 3339, date-time** — small, fixed-width fields, the shape of a typical user grammar.

Under D1 each reads exactly its grammar (values, refusals, positions), is held to it by a test
in the ordinary suite, and allocates no more than the generated parser; the stand's `web/*`
rows take them as their base. The rest of the Web grammars are variants of these shapes and get
none. Owner: finance-24, after the Finance-side items of the FIX anatomy.

**Done 2026-09-18 (finance-24): `HandUrl` `88d52428`, `HandDateTime` `3c616435`, `HandJson` `9dbb410a`**, each
held through `Web/Both` on the package's own tests, a mutation corpus and, for JSON, the whole
JSONTestSuite and a value nested 100,000 deep. The mutations found two places where the generated
parser refuses at the start of what it expected rather than at the wrong character (a lone `:` in
an IPv6 literal, a truncated literal name); the hand parsers follow. Found beside it: `JsonValue.ToString`
and `Equals` recurse and overflow where the parser reads, to be fixed in the package. The JSON model
stays as it is (Igor): the number is text, `null`/`true`/`false` are literals, strings are unescaped.

## Open questions

### Q1. SQL:2023 through a lexical layer

Raised 2026-09-17. Owner: sql-ff, with performance-3f for the generator.

`SqlStandardParser` is `[Gram("SqlStandard.gram")]`, compiled over characters; `Sql92Parser`
and `TransactSqlParser` are `Lexical = true`. `HandSqlStandard` reads through a token cursor
and classifies a word as an integer once (`SqlCursor`, `SqlWords`). Every per-token cost the
SQL timings name — the reserved-word bucket walk behind a first-letter switch, a word costing
1.0 us more than a literal even in an empty bucket, trivia at ten times the hand skip loop per
character, 3.6 us a token overall — is the cost of reading tokens as characters.

The reason recorded for not splitting (`docs/design/sql-parsers.md`): tokens overlap, a date
string is a character string whose meaning depends on the key word before it, and a choice
over characters can go back where one over tokens cannot. The same note says it would be
asked again when the grammar is whole and speed matters. Both are now true.

Before any further per-token optimization of the character reading lands, the answer to Q1
is required:

1. Which constructs of the standard cannot be read over kinds, listed from the grammar, with
   what the hand parser does for each. The hand parser already reads the whole language
   through tokens, which is an existence proof for most of them.
2. Whether those can be expressed with what the split already has (a kind as a set of
   patterns, a valued terminal read twice, a terminal the host measures), or need a new
   mechanism.
3. If a new mechanism: whether it is a lazy token cursor (tokens made as the reader asks, as
   `SqlCursor` does) rather than a lexer over the whole input first. That choice would also
   bear on streamed input, where the engine now reads characters (FIX).

**How large it is.** performance-3f's profile (SELECT a FROM t and twenty items): reading
reserved words is about 8 per cent, and the letters family puts bucket crowding at 7 to 20 per
cent of a line depending on the initial. So the character reading of words is a real cost
but the smaller part; Q1 is required before per-token work so that nothing is built twice
over characters and again over kinds, not because it is where most of the time goes.

**A constraint from FIX.** A lexer over the whole input first would break `stream bytes` and
lazy `yield`, which exist so that a long input is never held whole. Whatever mechanism Q1
chooses, a lazy cursor over buffered input is a supported form.

**Answered 2026-09-18 by sql-ff** (`docs/design/sql-over-kinds.md`, `030e882a`): nothing in the
standard is unreadable over kinds, since `HandSqlStandard` reads all of it through a token cursor.
Three things the token layer must allow: a literal of several quoted runs with separators between
them, and a token whose body is narrower than itself (`UESCAPE`), both covered by a terminal the
lexer begins and a rule finishes; and a key word glued on its left (`198OCTETS`), which needs
`wordboundary` read as a guard on both sides, a change to the language. Mechanism: a lazy cursor,
with a lexer over the whole input as an additional form for contiguous input. sql-ff's estimate of
the gain: about a tenth of what is left.

**Landed 2026-09-18 as `7d3ba2d5` (sql-39): SQL:2023 reads over kinds.** 14,718 tests, 143,291
corpus lines against `HandSqlStandard` with no difference; generation 4.0 s and 7.6 MB against
13.9; four visible changes to users, two towards the BNF (an introduced literal's parts) and two
away from it (comments nest six deep; `/*` inside a comment always opens one, since the trivia
scanner decides by the first character and does not backtrack — both go when the scanner
counts nesting). **The stand's rows (medians of five, paired): every SQL row faster, -4..-40%
(column -40%, a late refusal -34%, arithmetic -27%, twenty items -25%), allocation -0..-7%; still
4-21x the hand parser. The first call got worse on every row, +14..+56 ms (a literal 26 to 82 ms)
— diagnosed by sql-39 the same evening, phase by phase in a fresh process: the whole +63 ms is the
JIT of the parser's static constructor (20 to 84 ms), which initializes the `static readonly
string[] …_ExpectedN` arrays the refusal message reads; over kinds the expected set at a name
position is all ~410 keywords, in hundreds of positions (elements 1,891 to 21,319; IL 34.7 to
240.7 KB; 976 distinct arrays, so merging does not help). The lexer's tables are not involved;
the first parse itself got cheaper. Decision (D17): the arrays are built lazily on the refusal
path, where alone they are read (`??=` behind a property, an explicit field for the C# 8 floor);
the type initializer then compiles none of them. sql-39 implements it now in the Expected
emission only, tells performance-ff which file, and the stand measures first call before and
after on the SQL, FIX and EL rows — every parser pays this initializer, and D13 asked about
FIX's first call on small inputs. The deeper form, a set of kinds per position and one table of
names, so that the arrays leave the assembly's code altogether, is performance-ff's after C4.**

Landed 46eba522 (sql-39), one method changed (`Machine.DeclareExpected`): a property over a field
filled by `Interlocked.CompareExchange`, so that one instance per set survives — the refusal
tells sets apart by reference. Snapshots differ only in those declarations; the compatibility
project builds at C# 8; generation of DotGram.Sql no worse (four hosts 14-16 s either way),
files +5%. sql-39's own figure, loaded cores: the initializer 84 to 10 ms, a first literal parse
about 26 ms against 35 before the split. **The stand's record (paired, medians of five, five
fresh processes):** first call SQL literal 79 to 16 ms (-80%), one select 127 to 65 (-49%), twenty
131 to 68 (-49%); EL -4..-16%; FIX unchanged at 7.5 ms (its initializer was small; the hand
parser's first call 5.0 ms). Steady state within 2.6% everywhere, allocation identical. The split
is now a net gain on the first call too; FIX's first-call gap to the hand parser is 2.5 ms and
lies elsewhere than the type initializer.

**FIX's first call by phase (sql-39, 2026-09-18, fresh process, three runs; the answer to D13's
question about initialization).** Load 1 ms and the initializers under 1 ms on both sides; the
first parse is JIT: generated 86 methods, 25.7 KB IL, 12.6-13.7 ms; hand 40 methods, 18.2 KB,
5.6 ms; the second parse 0.4 ms on both. The largest single method is shared and is not the
generator's: `FixSchema.Type`, a 903-arm switch from tag to type name, 13.8 KB IL — over half
of the generated side's IL and three quarters of the hand side's — and `FixFieldOptions` calls
it for every tag of the range to build its data-tag table. Only in the generated one: the
recognizer (5.4 KB), the materializer (1.5 KB) and about forty small methods of the machine's
support (arena, pool, reset, construct, read, scan, guard), about 1 KB together but a JIT each,
3-4 ms in all. Decisions: `FixSchema` goes to tables — a type code per tag as RVA data and one
table of names by code, `IsData` read from the codes — finance-24, paired on first call and
steady state, both parsers; the method count of the support set is C2's first-call baseline
(86 to 40 is the gap); the phase tool goes from sql-39 to the stand's kit as the first-call
anatomy, run for C2 before and after. **Landed `59648810` (finance-24):** `Type` is two reads, a byte per tag in RVA data and 23
names; `IsData` compares a code and `FixFieldOptions` keeps no table of its own; a one-off
comparison against the switch found no difference for tags -2..1100. `Component` and `Codes`
are switches still, off the parser's startup path (the message layer's), the same technique if
their first call ever matters. The pair is ordered. Follow-up `41d23b96`: the 23 names had been a static array, and touching it ran
`FixSchema`'s initializer, which builds the message layer's 448 arrays — the string-literal
switch never had; the names are a 23-case switch now and `Type` reads only the RVA codes; the
parser path was never affected. The message layer's own first `Build` pays that initializer,
not the `Codes` switch; the fix there would be arrays built per id on demand, a reshaping of a
maintained file. Decided: measured first, by the stand's first-call anatomy on a message build,
and decided from the number. The number: the initializer about 35 ms, almost all JIT of one 82.5 KB method of 448 array
initializers. Decided: lazy slots by a scripted conversion of the file — the reference triples
in one RVA table with an offset per slot, decoded into a kept array on first use; code sets as
one string per set split on first use; the int-to-int switches stay; no static arrays left, so
no initializer. Verified by dumps before and after for every id, the Finance suites, and the
stand's first Build and steady-state message rows. finance-24 also found and fixes a
double-encoding of every non-ASCII character in nine files it wrote through a helper, and the
two FIX design documents of 2026-09-17 in the same commit. **Landed `da7e5a81`:** each array keeps its source form as a property filling a null slot,
rather than a table of triples, so the file stays readable as the schema it is; the slot is
published by `Interlocked.CompareExchange`, since `FixSemantics` keys a `ConditionalWeakTable`
by the array; the type initializer is gone; every answer of the schema dumped identical before
and after. The encoding repair is `b74ff7fd` (eleven files, mojibake of cp1251 through UTF-8);
`HandUrl` builds its record once (`7068ae0e`) and is now below the generated parser on every
URL shape, 176-336 B against 320-480. **Measured (stand, window 5, five fresh processes a side):** the initializer 24.2 ms and
85.6 KB of IL to 0.29 ms; the first message parse 41.0 to 18.8 ms (-54%), the first build 40.8
to 19.0 (-53%); steady state and allocation unchanged. The type tables alone: the field parsers'
first call -7% (hand -17%), 13.8 KB less IL, steady flat.

**The architect's review.** The estimate counts the word layer's own costs (bucket crowding,
trivia), not what reading over kinds does to the machine: over kinds a rule's answer stands, so
there is no tape of ways and no replay, and each choice is a switch on one token. On SQL-92 the
split measured 1.05-2.45x on accepted conditions and 1.35-3.85x on refused ones, far above a
tenth. And D5 does not stand in the way now: SQL:2023 publishes no stream, so the existing split,
a lexer over the whole input, serves its string input today; the lazy cursor is needed when a
split grammar streams. It also changes Q7.1: over kinds, `Replay`'s causes and the shared
beginnings are other ones. So, before Q7.1's design: SQL:2023 compiled with `Lexical = true` in
scratch, the three cases handled minimally or kept out of the measured corpora, timed against the
character reading and the hand parser. Its result decides the order.

**Measured 2026-09-18 (sql-ff, stand; `036d247e`).** SQL:2023 compiled over kinds against the same
grammar over characters, pinned, tiering off, accepted and refused apart: queries -23% / -36%,
DDL -36% / -46%, twenty select items -25%, value expressions -72%; generated code 6.77 MB against
13.86; generation 13% faster. The one loss is a refusal at the first token: 905 ns over kinds
against 227, the floor a lexer over the whole input pays on very short refused input. Of eight
obstacles to the split, seven were guards the character automaton needed and one is the language
(nesting comments), served by a terminal the lexer begins. **Decided by the architect: SQL:2023
ships over kinds.** sql-ff carries it out, with Q7.1 designed over kinds afterwards, under:

- No divergence from `HandSqlStandard` (D1): every corpus and `Both` agree before the switch lands.
- A reserved word is a token kind, not a guard: `?!ReservedWord` placed where the split already
  reads a lookahead as a range test over kinds (the syntactic rule), never a `when` after an atomic
  identifier, which fixes the first reading and cannot be taken back (271 DDL lines broke that way).
- Nesting comments and the interval string's body are read by a terminal the lexer begins and a rule
  finishes; nothing is left unchecked.
- The glued key word (`198OCTETS`, `2K`) is a question about the language for Igor: `wordboundary`
  guarding both sides, or the split reading a number glued to a word as the standard does not.
  Until answered the grammar refuses what the standard refuses by whatever the notation has, and
  says how.
- The stand's first-call row for SQL is quoted before and after: half the code should show there.

Q1 and D3 are not rivals. performance-3f attributes 53 to 62 per cent of SQL's time to
materializing, of which D2's store bookkeeping is a large part; the rest of a parse is the
character reading. Each report gives time exclusive of the factories both parsers call, so
that the two causes are sized against each other and not against the work the hand parser
also does.

### Q2. Contiguous input: characters and bytes

Raised 2026-09-17 by performance-3f. Language and public API: Igor's decision first, then
the architect's review.

`FixParser.Parse(byte[])` wraps a `MemoryStream` and runs the buffered iterator; the Bytes
form has the worst ratio to the hand parser in every FIX workload. Proposed: `stream bytes`
also offers a `ReadOnlyMemory<byte>` overload, fed to the existing buffered-byte machine.
The review will ask how much of the Bytes gap is the adapter and copy, which this removes,
and how much is the buffered machine's own checks, which it keeps; the Text form, read from a
contiguous span, is the measure of the second.

**Characters too, and the language already promises them.** Raised 2026-09-17 by finance-03:
`FixParser.Parse(ReadOnlySpan<char>)` copies the input to a string and calls the string form,
and `HandFixParser` does the same. `syntax.md` §6.3 lists `ReadOnlySpan<char>` beside
`string` as an input the generated overloads take; no emitted parser offers it. So the
character half of Q2 is completing the specification, not new API, and is designed with the
byte half as one question: a span for a whole-result `parse`, a memory for anything that
outlives the call (`yield`, a lazy context). Owner: performance-3f.

Until then finance-03 documents the copy on both span overloads, as `FixMessages.Parse`
already does (approved: comments only, no signature or emitted code changes). Removing the
overloads was refused: it would be a breaking change out and another one back.

### Q3. Options and modes left behind by experiments

**Decided 2026-09-17 by Igor: everything listed under "Remove" and "Beside the options" goes.**
Who and when:

- performance-3f, after the pool threshold lands: `GramCarrier.Mixed` with `Machine.Mixed.cs` and
  `MixedSql`; `PrefixTables = false`; `Direct` out of the user's attribute; the experiment
  benchmark projects and classes and their dated reports.
- sql-ff, with or right after D2, since it changes the same store: `ValueStorage` out of the
  user's attribute, `Paged` and its code, `ValueStorageBenchmarks`.
- finance-03: `Direct = false` out of `FixGrammar` and the Fix44 example.
- performance-3f, with D7's convergence of the two streaming mechanisms: `BufferedInput` and
  `BufferedBytes` as host options.
- `PartSize` and the internal `SourceFileSize`, `SharedTypes`, `Own`, `Inherits` are measured
  first and brought back to the architect.

**Done 2026-09-18:** Mixed and `MixedSql` (`03445198`), Shapes (`fd054645`), the experiment
benchmarks (`ceda9b7a`), `ValueStorage`/`Paged` (`2af82772`, sql-ff), `Direct = false` out of the
FIX grammars (finance-03), `PrefixTables` (`5c6d1bc6`) and `Direct` (`0f3ef41a`) out of the
attribute with an internal lever kept for tests. Emitted parsers byte-identical in every case.
Left: `BufferedInput`/`BufferedBytes` with D7, and the report on `PartSize` and the internals.

**The spare stack by nesting depth landed as `fdea50f1`**: a lazy stack of at most four spares a
pool, bounded by the existing thresholds, depth three by measurement. Paired: EL interpolation
-36% time, -70% allocation, tape/hand 4.25x to 2.71x; untyped lambdas -14%; SQL and FIX
byte-identical.

Raised 2026-09-17 by Igor: the experiments have left options and modes of the generator, some
of which may no longer earn their place; those go. The architect's inventory, by what an
option is for. Usage counts are `[Gram]`/`[GramOptions]` sites in `src`, `examples`, `tests`,
`benchmarks`.

**Keep: they are API a user chooses by, and each changes what the user gets.**
`Suffix` (a second reading), `LocationType`, `Portable`, `Stacks`, `SpanCaptures` (changes the
type a factory receives), `Lexical` (a request, and over kinds a rule's answer stands, which
changes what the grammar means), `Carrier = Tape | Immediate | Auto` (Immediate is the
author's statement that the factories are pure).

**Remove, recommended.**

1. `GramCarrier.Mixed` and `Machine.Mixed.cs` (about 1,170 lines). Measured slower than the
   tape on SQL and recorded as not to be expected to beat a log on a grammar whose rules are
   mostly on cycles. Used by one benchmark (`MixedSql`) and nothing else. D3's design replaces
   the question it asked.
2. `PrefixTables = false` ("use the previous strategy"). Used by tests only. The fallback to
   the ordinary alternative chain inside the prefix tables stays; the switch back to the old
   emission goes.
3. `ValueStorage` as an option: `Flat`, `Adaptive`, `Paged` as author choices. Used by tests and
   `ValueStorageBenchmarks` only; `Auto` already chooses (dense, adaptive). `Paged`, which `Auto`
   never chooses, goes with its code; the rest becomes the generator's choice, not API. After
   D2 lands, since D2 changes the same store.
4. `Direct = false` in `FixGrammar` and the Fix44 example: moot, since the reader refuses a
   recovering grammar anyway (performance-3f). The option stays only if a test of the engine
   needs it, and then it is not offered in the user's attribute.
5. `BufferedInput` / `BufferedBytes` as host options beside `stream` / `stream bytes` on a
   publication: two ways to ask for one thing. Under D7 some forms are declared by the grammar,
   so the publication syntax stays and the host options go. With D7's convergence of the two
   streaming mechanisms, not before.

**Open, measured before deciding.** `PartSize` (a wish; measured flat from 60 to 250, set only
by the Fix44 example at 1,000); the compiler's internal `SourceFileSize` (file split),
`SharedTypes`, `Own`, `Inherits`.

**Beside the options.** Benchmark projects and classes that exist only for a rejected or
finished experiment (`CompilationSplitExperiment`, `DotGram.HandDeferred`,
`DotGram.PrefixBenchmarks`, `MixedSql`, `ValueStorageBenchmarks`, `DeferredShape`), and the dated
reports in `docs/design` whose subject was removed, go with it; `docs/next.md` keeps the
history.

Each removal is one change, by the owner of the area (performance-3f for the generator, the
option's grammar owner for its use), and shows that emitted code for the shipping grammars
(SQL, EL, Web, FIX) is unchanged or explains each difference.

### Q4. What else in the generator can be simpler

Raised 2026-09-17 by Igor. Three candidates go to measurement first, confirmed by Igor the
same day; each comes back to the architect as a report, with no code changed.

1. **The machine that builds a token's value.** Over kinds a terminal with a value is read a
   second time by a `_Value` machine; in the expression language's generated code (snapshot of
   2026-09-14) that machine is on the engine, seventeen parts. It cannot fail, since the lexer
   has accepted the extent. Measure its share of parse time and of code size in EL, T-SQL and
   SQL-92; find why it is on the engine; say whether it can be a reader or be built in the
   lexer. performance-3f.
2. **A second reading only for locations.** `TransactSqlParser.Located` is a whole second parser
   (about 33 MB beside 32 MB, same snapshot) so that a caller who wants no locations does not
   pay about 14 per cent. Prototype one reading generic over a struct parameter that says
   whether locations are offered, which the JIT specializes; measure time without locations,
   time with, JIT and first-call cost when both are used, and the build for netstandard2.0.
   sql-ff.
   **Sized 2026-09-18 (sql-39):** the located SQL-92 differs from the plain one by 3,375 of
   18.2k lines, and only 118 of them are `Locate` calls; the rest is the recording — a start
   in every rule's `ways.Begin`, a second layer of value materialization, extra method parts.
   So one generic reading means the reader and the walk written once with the recording under
   a type flag, and the record format holding positions in both variants or computing offsets
   differently — all in the reader performance-ff is rewriting for C2. Decided: first a measure
   without code — the stand profiles the located against the plain T-SQL reading over the corpus
   (dotTrace, sql-39's question and harness), to say what share is the recording itself against
   the `Locate` calls, which decides whether a type flag is enough or the record format has to
   be one; the prototype after C2, with performance-ff, in his reader. No hand prototype now,
   since C2 changes the reader it would copy.
   **Profiled (stand, window 8; results under `benchmarks/results/tsql-located-profile-2026-09-18`):**
   located pays +15% wall over the corpus at the same allocation and GC; of the extra CPU, 76%
   is the materializers' own time (+33%), reader and recognizer 12.5%, the `Locate` calls
   nothing (inlined, at most 1 ms of 35 forced out of line). So recording positions is nearly
   free and consuming them in materialization is the cost. Direction for the prototype after
   C2: one reader that always records, one record format, and the walk consuming positions
   under a struct type parameter the JIT specializes away — the plain variant should then read
   within a couple of per cent of today's, the located within today's located; sql-39 reads the
   profile and says whether the 12.5% in the reader (the `start` carried through the parts)
   changes that.
   sql-39's reading agrees: plain pays about 2% for the start carried through the reader even
   with no flag; a flag around `Locate` alone saves nothing; the walk must stay specialized,
   since plain through the located walk pays the whole +13%. Decided as Q4.2's design; the
   prototype after C2 with performance-ff. No further variant is needed.

   **The reader's gate, classified (sql-39, `b19575b2`, `0fcebca6`; next.md "The reader's gate, by
   the shape that opens each way", the table in `docs/carriers.md`).** It holds 43 of 84 grammars
   and 353 rules; 209 open a way themselves, at 271 places. By what removes them:
   - a graph rewrite, 90: a seam at the head of every alternative of a turn or a choice (30, the
     expression grammars); a grammar's own seam rule — `Ows`, `Fws?`, `Blank` — at the head of
     the turn and of the continuation (58, the RFC grammars and INI), which is the seam hoist
     generalized from trivia to any rule that is a greedy star over a class; a seam the
     continuation can begin inside of (2, a comment in trivia);
   - the reader, 59: alternatives that begin apart, so the way is dead (33 in 12 grammars); an
     alternative that may read nothing, `(eol | ?=eof)` (15); literals (10); a lookahead at a
     turn's head (1);
   - intended, the language's, 122: maximal munch (40), alternatives that begin alike (38; one,
     `DecOctet`, a factoring candidate), a tail that decides (33), IPv6's counted groups (11).
   And a defect of the count itself: 29 places are in rules nothing calls openly — every call
   inside an atomic group or a lookahead, or an entry — whose ways nobody can re-enter, yet the
   gate counts them.
   Decided: expr generalizes the seam hoist to any greedy-star seam rule and to the heads of
   every alternative, as one rewrite, measured by the diff of `carriers.md`; sql-39 takes the
   reader class (dead ways first, then the empty alternative, then literals) and the count's
   defect, each a separate commit after C2 lands, performance-ff told before each, the same
   conditions as `014136df`; the intended stay, `DecOctet` goes to C's factoring if it folds.
   **Done locally (sql-39), pushed after the stand's gate and pairs:** the count — 8 places truly
   sealed, not 29, since 21 were entries, which are asked again to reach the end (a benchmark's
   config to immediate); the chain of first-character tests (RFC 8259 and RFC 6901 to immediate,
   "begin apart" 33 to 19); the empty alternative (15 to 14). The fold's shared seam is a separate
   step. And `Replay`'s nullable refusals fixed (`99f1c011`): only `eof` and a rule reading nothing
   through a lookahead or a guard refuse — a word boundary declared is not a refusal; no grammar
   changed carrier. It lands first, since C4b rests on it.
   Landed `9bf12288`, the gate flat. The fourth item done locally: literals under a construction
   and ignore-case literals settle in the reader's way decision, both classes gone. The quoted
   string's `""` turn needs a follow two characters ahead, which is `Determinism`'s never-gives-back
   question; it goes to expr, and is probably `SettingsFile`'s last cause. sql-39 next: why T-SQL
   has a commit point at 1.7% of its sites.
   **The proof reached (expr, `94c10b3f`, held for its gate):** the quoted string's `""` turn is
   settled by asking past the one-character stop — a turn that begins with it must go on with
   something that cannot begin what follows the stop. `SettingsFile` and `FilterFile` then compile
   immediate by `Auto` on their own, both gates at zero, their answers unchanged: the mechanism,
   the seam, the reader's ways and the analysis together, proved on the example chosen for it.
   Every package's code byte-identical; five examples change, timed roughly before and after.
   Landed `48b1eb77`, with the empty-list fix `e384d37e`. Next for expr: the expression language's
   own parser, on the tape with 131 of 137 building rules held by `Replay` and 20 direct causes,
   2.18x the hand parser against 1.39x for the immediate host an author's option forces — the
   causes classified and removed until `Auto` chooses immediate by itself and the second host goes.
   **Classified (expr):** ten sites, each twice (a rule and its `_With1` twin). Twelve are
   alternatives that begin with the same built operand; six a turn or an optional given back after a
   token refuses; two a building rule read inside a lookahead. Decided: the lookahead is a grammar
   edit now (look through a rule that builds nothing). The shared built head is a scenario, not a
   grammar (D15): first whether C's fold can take a head that is a building capture — read once,
   its value handed to every tail's construction, identical bindings only — which SQL would gain
   from too; the grammar is factored by hand only where the pass provably cannot. The turns given
   back are what `Replay`'s refinement B should already settle; why it does not goes to sql-39.
   `NamedType`'s `<` — C#'s own ambiguity with a comparison — is intended and stays, so the parser
   reaches the immediate carrier by `Auto` only when a carrier can be chosen per construction.
   **The fold on a building head, decided (expr's design):** a run of alternatives folds on a head
   that is a capture when every alternative binds the same name to the same call with the same
   arguments, the called rule has one reading that can lead anywhere before any tail (possessive
   against the union of the tails' first sets — what a committed token has for free), and the
   tails' constructions read the head by that name; then the head is read and built once. A proof,
   not a heuristic. In EL it takes the `try` and the three `new` sites; assignment, a qualified core
   and a member inside a fold's loop it cannot. Held to the refusal record, the diff of
   `carriers.md` across the solution, the gate and an EL and SQL pair. The turns given back in EL
   turned out to be the two-characters-ahead question (`?.`, `?[`, `??` and a following block),
   the family of the quoted string's turn, not refinement B: expr looks whether the same answer
   generalizes. The lookahead edit is done; the cause it held moved, honestly counted, to a member
   in `Postfix`.
   **Written (expr, `7428b037`, held for window 37):** the pass already took a capture head; what
   stopped it was the fold giving up on a choice as soon as one alternative was not a construction.
   Condition (ii) is the one the pass already checks — over kinds a call to a rule that does not
   give back commits its answer. No refusal position or outcome moves; nineteen EL lines reword (a
   type's own message where a list of keywords stood). One loss came with it and is fixed in the
   same commit: a guard leading a dispatched group refused naming nothing, so two expected tokens
   vanished; it now names the other groups' first tokens. EL's direct causes 20 to 18; SQL's code
   changes where its heads fold, its causes do not; the gate is noise. The two-characters-ahead
   question does not settle EL's `?` and `{` turns: an `if … else` can end a statement, so a
   continuation is any statement, and an initializer and a following block stay alike for an
   expression of any length — the language's, intended.
   **The fold's shared seam (sql-39, `9c98a48e`, held for its gate):** the reader peeks past the
   seam once, decides by the character after it, puts the position back and reads the chosen
   alternative whole, where the seam builds nothing and opens no way; `NeverGivesBack` sees the seam
   through the fold step's construction. Five examples to immediate (48 to 43 on the tape); what
   is left are folds whose right operand is the fold itself, where past the seam the continuation
   really does begin like a turn. Next for sql-39: a design, no code, for a carrier chosen per
   construction rather than per machine — the one thing between SQL's and EL's commit points and
   the immediate carrier — to the architect, then to Igor.
   **The design, reviewed (sql-39, `docs/design/carrier-per-construction-2026-09-19.md`):** one
   machine can carry both — the reader and the ways do not depend on the carrier, and the tape
   already builds mid-parse for a guard; four edges between parent and child, of which the
   narrowest safe set, a settled subtree (every construction with a point at its rule's end, every
   building call into the same), needs only a held table and a leaf record. Static reach, counted under the loose
   definition and so a ceiling rather than an estimate — the counting build settles 115 of the 651
   rules that write a record, not 195 — T-SQL 26% of its constructions, SQL:2023 7%, EL and the Web's tape grammars nothing —
   EL is held by `Replay`, Web by the reader's gate. Estimate for T-SQL 7-11% of a parse. Decided:
   step 1, a count of records per rule over the corpora weighted by their materialization cost,
   with no generator change; step 2, two carriers in one machine, is architecture and goes to
   Igor with that number.
   **Step 1 measured, and it shelves step 2 (sql-39):** of 116,946 records T-SQL writes over the
   corpus, 4.8% are in settled subtrees (24.9% in rules with every point at the rule's end);
   SQL:2023 none. The records are the value tower, and none of it is settled — `Replay`'s causes
   hold it. The factories are a tenth of materialization, so a record's cost is the walk and the
   time share is the count share: step 2 would give 1-2% of a parse. Shelved with its number; the
   lever is `Replay`'s causes in the value tower (a bracketed value against a subquery, 40 rules
   under it; a join's turn, 29; an inline return, 26), after which the design is measured again.
   The first is the language's: after `(` both a value and a query can begin with a bracketed
   query, `((SELECT 1) + 1)` against `((SELECT 1) UNION (SELECT 2))`, decided at an unbounded
   distance; no fold or grammar edit removes it without changing the tree. Intended, like
   `DecOctet`. The join's turn and the inline return are the language's too: nested joins' tails
   take an outer `JOIN … ON` greedily and only the count of `ON`s decides, and an inline return's
   bracket can be a query's own. So the three heaviest causes in T-SQL's value tower are ambiguities
   of unbounded depth; the grammar question is closed. What remains for SQL is measured next: an
   anatomy of SQL:2023's 7.9x against the hand parser, phase by phase, as FIX's step 1 was.
   **The anatomy (sql-39, next.md `96ba42ef`; twenty selects, 71 µs against the hand parser's
   7.3):** 54 of the 71 are the shape of the materializer's code, not C4 and not the analysis. The
   arms' prologues, 46%: the part methods zero 10-13 KB of frame on every call, since every arm's
   locals have slots of their own and the JIT must clear references whichever arm runs — 402
   records a parse at about 85 ns each. The walk each guard runs, 30%: the towers put a guard on
   almost every level, 301 walks for 402 records, each paying a fixed setup to build, usually, one
   record whose children are built. Recognition 13% (9.2 µs against the hand parser's 4.9, which
   builds as it goes); the lexer is faster than the hand parser's. Decided: parts split by frame
   rather than by code, or an arm a method, chosen by the numbers with the first call measured;
   then a direct path for a guard whose record's children are built — both expr's, the
   materializer's owner, expecting about 20 µs, 3x the hand parser; recognition analysed next by
   sql-39.
   **Taken again with both constants (sql-39, `62715de4`):** twenty selects 17,862 ns against the
   hand parser's 6,766 — 2.64x, where the morning's anatomy found 10.6x. A walk costs 27.8 ns and a
   record 6.0, and the phases now read: the walk a guard runs 8,379 ns and 47% of the parse, the
   arms' built records 2,394, recognition 4,167 against the hand parser's 4,883, which builds as it
   reads. So **the lever has moved**: what costs is not what is built but that we come back to
   build it — 301 walks for 402 records — and that is the materializer's, expr's, with a design
   asked before code. **The design, decided:** the guard is handed its value where the value is
   made. All of SQL:2023's tower guards have one shape — a capture, then a guard naming it, with
   nothing between that can consume or fail — and for that shape the walk is provably unnecessary,
   since the guard will ask for that record, at that position, with those children; building it
   when the capture closes is the same construction moved earlier inside one reading, so §7.3 is
   untouched and everything no guard names is still deferred. Where the shape does not hold, the
   walk stays. Refused and recorded: building every record as it closes, which would run factories
   for readings no guard asks about. First, though, a smaller thing the profile found beside it and
   unrelated to guards — the value store clears all three hundred tables at the end of every parse,
   used or not, which is 6% — as its own commit and its own pair. Cutting the walk's constant
   instead is held back: if the walks go, there is no constant worth cutting. With the constants carried onto T-SQL's counts, the shelved carrier per
   construction is closed for good: less than half a per cent of a parse, and it looked larger only
   while a record still carried an arm's prologue.
   **Recognition, analysed (sql-39, next.md `02035cc7`):** no way is opened, retried or replayed;
   almost all of the difference is the choice over kinds hitting the switch's limit of 128 named
   kinds. A primary's first set is 357 kinds, since an identifier begins with any unreserved word,
   so no switch is built and 21 alternatives are tried in order, each refusing on its first token —
   1,381 of 2,023 rule calls refuse on the first token, about 37 calls a column; the hand parser
   switches on the token's kind and word. Decided: the switch unbound from named labels — a byte
   table from kind to group, the group's members tried in order — and a bit table for membership
   and long comparison chains, separately. sql-39, who lifted the same limit over characters,
   performance-ff told first. Expected: recognition about 4 µs, level with the hand parser.
   **The kind table landed (`51d40d9f`), paired:** SQL:2023 -4..-6% (twenty selects -3.7 µs, as
   forecast), T-SQL -9..-12%, first calls -7..-16% though T-SQL's file grew 4.4%; carriers and the
   refusal record unchanged; the table is RVA data on netstandard2.0 and net472 (no array allocated
   in the getter). The bit tables that followed (`74b9737c`) read neutral — the gain is only where
   a class is wider than 256 characters, which no row has — and are kept for the simpler code, no
   search left in T-SQL. The optional tails left in recognition are 1.7% and are not taken; the
   anatomy is redone once expr's three land. The gate caught the generator's own grammar at +34 ms (group splitting for
   Unicode sets the table never takes), fixed with byte-identical output (`972414e8`). The bit tables
   wait for their window. sql-39's check of the prologue on T-SQL agrees with expr's figure.
   **The prologue, written (expr, `057cd797`, held for its pair):** each arm a local function of its
   own and a part a switch that calls it; the first part's prologue went from zeroing 13.3 KB to
   none, and only the arms that run are compiled on the first call. Rough: twenty selects 90 to 44
   µs, the T-SQL corpus 47.7 to 37.0 ms a round. EL's single materializer zeroes 4.8 KB on every
   guard walk; the same form goes to it with the guard walk's direct path.
   Then (expr, `bb603091`, held): what was left of the walk's own time was the preload of about a
   hundred value tables into the closure the arms share — a struct of references zeroed and filled
   on every walk, a guard's one-record walk included; an arm now takes the tables it names from
   their fields and the walk takes none, and EL's single materializer gets arm methods. Rough:
   twenty selects 44.5 to 30.8 µs — 90 to 31 from where the anatomy started, against an expected
   20; EL -12..-21%. The bool `TryParse` is written with §6 as approved (`dba87a9e`, held).
   The guard's direct path (`dafc5698`, held): where the guard's root is the last record opened and
   everything since the rule's mark is built, the walk builds that one record and returns. Rough:
   twenty selects 39 to 24 µs — the anatomy's 90 on this machine to about 24, the rest being
   recognition (sql-39's kind table). A first version duplicated the walk's switch and pushed
   T-SQL's and SQL:2023's walks past the JIT's budget; it never ran.
   **The prologue landed (`fa47097e`), paired:** twenty selects 76 to 41 µs, T-SQL -48..-63%, first
   calls faster (a third less JIT), EL flat. The other three wait for their windows.
   **Answered (sql-39):** 643 of T-SQL's 658 building rules are not kept, 84 by a cause of their
   own; of the rest, 343 hang on one place — `?!SqlPiece` after an atomic block, a negative
   lookahead that reads a whole statement, so every statement and all under it counts as read
   where the reading is thrown away. The rest are ordinary choices, as in SQL:2023. Decided: the
   grammar checks the end of the batch there instead, once sql-39 shows from the published syntax
   and the server that the language is the same (the position of a refusal unchanged, only its
   expectation); and the scenario goes to the generator after C4 — a lookahead over a building
   rule could call a variant that only recognizes, so that its reading poisons nothing else.
   **Done (sql-39, `a021ad2c`, after the stand):** checked against the server — after an atomic
   block only the end of the batch can follow; one input the grammar used to read and the server
   refuses (a module nested in a block) is now refused, and a stray `END` is refused where the
   server refuses it; the engine comparison over the corpus identical, the round trip whole.
   T-SQL's commit points 54 to 2,362 of 3,146 sites (1.7% to 75%), rules held by `Replay` 643 to
   321 — one line of grammar. The reader's gate items (1) and (2) landed (`109770ac`, `d032591f`):
   config -44..-56%, JSON -49..-58% (1.27-1.66x the hand parser), none of the three grammars moved
   to immediate has two gathered members on one stack.
   **The seam hoist, written (expr):** `File = trivia & (Setting & trivia & eol & trivia)* & eof`,
   but `SettingsFile` stays on the tape for a reason outside the rule: a publication is entered
   as `trivia File trivia`, and where the rule begins with its seam the entry's seam is followed
   by the same characters, so the run is never settled and every trivia grammar opens a way
   there. Two corrections to the proof: the language's star does give back, so the rewrite holds
   only where neither the turn nor the continuation can begin inside the seam — the question
   `NeverGivesBack` asks, from which the comment-in-trivia case falls out; and adjacent seams the
   hoist creates collapse into one under the same condition. Decided: the entry's leading seam
   is dropped where the published rule begins with that seam, in `FollowSets`' entries and in the
   emitted entry method in one commit (analysis and code must not disagree), by expr, after C2
   lands, performance-ff and sql-39 told first.
   **Superseded before it was written (expr's probes):** the entry is one case of a general one.
   A seam followed by something that may read nothing and then a seam again — `t N t`, in every
   grammar with implicit trivia, e.g. `Sum = l: Num & (op & r: Num)*` — leaves the first seam's
   run unsettled, since its follow holds the seam's own characters; and `eof`, `?!any`, passed
   what follows it through as if something could follow the end. Decided instead of the entry
   change: a seam's run is settled against the half of its follow past the next seam
   (`AfterSeam`) — two adjacent readings of an only-reading seam end at the same places, however
   the characters are split — provided the seam is not captured and no capture, mark or
   location boundary falls between the two seams, since a different split would change a value
   though not a position; and `?!any` contributes the end only. expr, two commits after C2: the
   analysis alone, whose emitted-code change across every trivia grammar is seen on its own
   (snapshots, corpora, the refusal record, the gate, the diff of `carriers.md`, a stand pair on
   the EL rows), then the hoist on top.
   The value condition, sharpened with expr: the settled run reads exactly the split the engine
   tries first, so captures, marks and locations — read from the first success — are the same
   either way; only something that succeeds or fails on the split itself can tell the two
   apart. So a seam's run keeps its way, per grammar, where a node whose success depends on the
   position without consuming can stand between it and the next seam through what may read
   nothing: a `when`, a lookahead either way, an external or host-measured terminal that may
   match empty. The hoist takes the same condition.
   **Both written, uncommitted until C2 (expr):** with them `carriers.md` moves one grammar to
   immediate (`SqlDialect`) and "again trivia: opens a way" from ten grammars to four, each of the
   four explained (a comment in trivia; JSON's seam before a body that may begin with a space;
   `Spacing?`, an optional of a star, recognized next). `SettingsFile` is no longer held by the
   seam but by a quoted string's `""` turn and the reader's way into a three-way choice, both
   sql-39's reader class; fold rules' operator alternatives behind one seam go to his choice
   emission too, since their constructions are keyed by the fold. **Found, and put first:** a
   refusal defect already on main — where a turn's first element fails at the turn's start and
   then `eof` fails, the reader records nothing and answers "does not match" at 0 where the
   engine answers "Expected …" at the right place. It is a position disagreement, so
   correctness; the hoist would make it common. expr fixes it (the re-read path is his, Q7.2),
   after C2, with the shape added to the refusal corpus, and asks why the hundred compared
   inputs missed it. Order: C2, the analysis, the refusal defect, the hoist.
   **Reordered by expr's verification of the analysis alone:** it moves four refusal lines onto the
   reader defect (a newly settled run meets it), and it turns settled rules into scanners whose
   repetition has no C1 search, so Fix44 would lose almost every `IndexOf` (1,770 to 8). Order now:
   the reader's refusal fix with the corpus shapes, the search in the scanner's repetition
   (performance-ff, after `LineAt`), the analysis, the hoist. performance-ff's queue before C3:
   `LineAt`, the scanner's search, the gathered-list guard.
   **The reader's refusal fix landed `9f292dd6` (expr):** the recording reading tries a turn behind
   a closed door as the engine does, except in scanners and the seam; a rule that is only a look
   is named on refusal as the engine names it. An inline look still refuses silently: recording
   it moved EL refusals past the hand parser's, so it was left out. Over the whole refusal record
   and the EL record no position or outcome moves; the wording moves toward the engine, the
   renderings' disagreements 100 to 77; three new corpus shapes. The hot path reads one more
   field at each loop exit; paired (window 12): every accepted path flat and allocation
   identical, one FIX row +5% repeated alone; a refusal allocates 88 B more and costs more (a URL
   refusal +15%, an early EL refusal +4..5%) — accepted for the refusal's correctness, the 88 B to
   be explained and removed if it can be.
   Repeated alone (window 13): FIX's string rows flat, the +5% did not repeat. The 88 B are the
   list of a second tied expected set, now named as the engine names it; held inline and listed
   only from a third, after the hoist (expr). The scanner's search paired: T-SQL comments -4.6%,
   SQL comments -1.0%, controls flat.
   **The seam analysis landed `7e13fe68` (expr), on the scanner's search `164967f8`:** the refusal
   and EL records unchanged line for line; seven generated files change; Fix44's 1,770 inline
   searches are one searching scanner called from 1,764 places. The hoist follows, measured by
   the diff of `carriers.md`; one stand run pairs FIX, EL and Web for both.
   It landed without its generation gate, which is now run together with the hoist's. The hoist
   (`ed567e40`, held): `SettingsFile` loses its trivia way, and what holds it now is sql-39's
   reader classes; the Unicode-category seams keep theirs, since their extent is not known and
   nothing is assumed. Held for one more reason: two engine refusals lose an expected item
   ("Expected eof or 'b'" becomes "Expected eof" where another item could begin) — an equivalent
   rewrite must not narrow what a refusal names, so the engine's door is fixed first.
   Fixed (`e35bc176`, held for its pair): where the engine's machine probes a repetition that may
   take nothing, the recording reading now goes in as the reader's does; no position or outcome
   moves, 58 engine lines gain the expectations they had lost (the "[x" kind already on main, and
   "Input does not match" becoming "Expected '+'" in a fold), and the renderings' wording
   disagreements fall from 88 to 30. With it the hoist (`7c475a28`) changes no line of the refusal
   record. Both wait for the stand: the engine rows' pair and the generation gates.
   The analysis `7e13fe68` paired: FIX's string rows -5.5..-7.4%, bytes flat, EL and Web flat,
   allocation identical.
   The hoist's gate named the generator's own grammar +32 ms (the pass computed the whole
   grammar's follow); reworked to read only the rest of its own sequence (`7c1920ac`), every
   generated file identical, gated again. The 88 B a refusal stays: it is the list of every set
   tied at the furthest position when there are three or more, on the refusal path.
   Both land: the reworked hoist leaves generation flat everywhere; the engine probe costs a real
   ~37 ns on the smallest FIX stream parse (+7%, repeated), accepted as the price of a correct
   expected set and moot once FIX's stream form is read by the reader.
   Landed: the engine probe `ad76ad64`, the hoist `101e8896` (its message carries the diff of
   `carriers.md`), and D12's last item for EL, three duplicate tests dropped, each named with
   the test that holds its claim (`eb79e52a`). Next for expr: the thirty wording disagreements
   left between renderings.
   The scanner's search landed `45155db6` (performance-ff): it already fires in the SQL grammars'
   line comments and two examples; expr's analysis is unblocked, and FIX's `IndexOf` stays with it.
   **The dead ways, located (sql-39):** not the sets — a negated class is an exact complement
   and the analysis keeps it apart — but the reader's choice: it decides by the first character
   only through a switch, and refuses the switch past 128 named characters, a limit on code
   size that also shut off the path without a way. Decided: where the switch is refused but the
   alternatives are pairwise disjoint and none can read nothing, the reader writes a chain of
   first-character tests from the narrowest set to the widest, the widest last as the else, and
   opens no way — for `(Plain | Escape)*` one comparison with `\`, as a hand parser writes it.
3. **Flat against reader.** Flat writes a publication as one method of states; reader writes a
   method per rule; flat recompiles the machine to do it, which is a known source of defects.
   Measure both on the Web grammars and the examples. No difference: flat goes. Flat faster: the
   reader takes over what makes it so, and flat goes. performance-3f.

The other candidates are conditions on work already decided: the engine's special paths
(scalar scanners and guards, sites, bounded materializer scans) are reviewed against D3's
analysis once it exists and removed where it covers them; `Auto`'s second rendering goes when
D3's analysis decides the carrier; the value store's remaining modes are revisited after D3;
the legacy `Parse(TextReader)` overload and the diagnostics about removed choices go with D7
and Q3.

### Q5. State that holds while something is read, and that guards read

Raised 2026-09-17 from expr-2d's audit of the expression language's `State`. A change to the
language, so discussed with Igor before any design. It has a twin in the other direction:
SQL:2023's tower guards need a cheap value that rises from a rule while it is read (the roles),
and can only have it by building the node beside it; recognition-time values that guards read,
inherited (this) and synthesized (that), are one subject for that discussion.

The expression language keeps, in its §7.7 `context`, pairs that open before a body and close
after it: lambdas (`Entering`/`Leaves`), loops and breakables (`Opening`/`Breaking`), and
variables whose type is not yet settled (`Awaits`/`Settles`). A reading abandoned between the
open and the close leaves an open extent, and every later position is attributed to it: a
`return`, `break` or `continue` after it can go to the wrong target, and a `var` after it can
read as unsettled (`object`) instead of inferred. The tape is exposed wherever the grammar
abandons a lambda, loop or switch after its opening guard. No minimal reproduction yet.

`syntax.md` says why: §7.7's context is for what can be written more than once without harm,
and nothing in it unwinds; "this holds while that is being read" is §7.8's mark, which is
abandoned with an abandoned reading. But a §7.8 mark is read by constructions after the parse,
and these pairs are read by guards during it: which alternative reads a `var` depends on them.
The language has no state that both holds while something is read and is visible to a `when`.

Options:

1. **Marks readable by guards.** A `when` may name `parserState`, the marks standing over the
   place it runs. The generator already keeps marks in order and drops an abandoned reading's
   (`StateSet`/`StateEnd` on the engine); the reader keeps them on a stack its calls unwind.
   Nothing is undone by the host. The architect's recommendation.
2. **The host undoes.** The generated parser calls the context's mark and rollback at every
   way back. Couples every choice point to a host interface and costs on every backtrack.
3. **Keep the context and make each entry answer for itself**, by position. Not sound: after a
   reading is abandoned the parse can move past the stale opening without opening again.

**Most of it needs no change to the language — Igor agreed 2026-09-17.** `break`, `continue` and
`return` find their target in `=>`, after the parse, where §7.8's marks are already visible and
only the accepted reading's marks stand. So loops, breakables and lambdas move from the
context's open/close pairs to `with state` marks, as an improvement of the expression
language's grammar (expr-2d). What is left for the language discussion is only what a guard
must see while reading: whether a `var` is read as unsettled. The notation offered for that
discussion: marks visible to `when` (downward). A value carried up while reading, for SQL's
towers, was withdrawn the same day: Igor recalled that whatever a `when` asks for may be built
while reading, which is the author's choice; the open problem is reusing what was built (Q6).

**Correction the same day (expr-2d).** A mark's value is a constant of its site: the arena keeps the
site number and the value is written as `site == 0 ? … : …`, with no span, capture or context. So a
mark says "inside a loop" but not which loop; two loops side by side are one `[Loop]` to a `break`
in each. The jumps cannot move onto today's marks after all; the pairs stay until the language
decides. Options, for the discussion with Igor:

- (a) A mark's value may name `parserSpan` and is computed where the mark is placed; the arena
  keeps the value.
- (b) Values stay constants, and a construction is handed where each mark standing over it was
  placed: `parserMarks`, positions parallel to `parserState`. Narrower, and the key a jump needs.
  Recommended by expr-2d and by the architect.

Together with marks visible to `when` (above), (b) covers the whole of Q5.

**Decided 2026-09-18 by Igor: (b).** A construction is handed, beside `parserState`, where each mark
standing over it was placed (`parserMarks`); mark values stay constants of their site. The
expression language's jumps then move from the context's open/close pairs onto marks. Whether
guards see marks while reading (for the unsettled `var`) was not part of the decision and stays
open; until it is decided, `Awaits`/`Settles` stay in the context, where the defect is latent.

**The defect is latent (expr-2d, 2026-09-17).** After every successful tape parse over the corpus
and all its mutations, no open/close pair was left open; every abandoned reading that could be
constructed makes the whole parse fail, and overload resolution catches nothing while building a
candidate's lambda. It shows only if the grammar gains an alternative that succeeds after an
abandoned opening. Q5 is therefore not urgent, and stays a question about the language.

**Landed 2026-09-18 (expr-2d, `c4a79af9`..`f81ad6d2`).** `parserMarks` in the generator with tests over
the three carriers and both symbol domains; §7.8 says what it is; the expression language's jumps
go by marks and its context loses the open/close pairs (the unsettled `var` stays); the hand parser
in the same change. Every grammar but EL emits byte for byte what it did; EL emits 2-3% less.
`GRAM4029` refuses `with state` or a hook naming `parserState`/`parserMarks` where no `state` is
declared, which used to surface as CS0103 in the consumer.

Until decided, the expression language's hand parser uses a checkpoint of its own `State` for
its second, recognize-only reading (a mark at the start of the publication, or of a hole or
body window), which is local to it and changes nothing in the generator.

### Q6. Reusing what a guard built

Raised 2026-09-17 by Igor. Building what a `when` asks for while reading is settled: it is the
author's choice. The problem is that what is built is not reused.

Today, on the reader: a value a guard built is flagged on its record (`values.Built`) and the
walk at the end reuses it, so within the accepted derivation nothing is built twice. But a way
back lowers the watermark (`ways.Built`), because a record's index is its name and the next
derivation writes other records at the same indices; everything built in the abandoned reading
is dropped, and when the grammar reads the same rule at the same place again it builds it again.
That is SQL's case: a `(` read as one thing and then as another, the towers' guards building the
operand's subtree on each reading.

What reuse would be: a value built for a rule read from one position to another, kept by where it
was read rather than by record index, and taken again when the same rule, specialised the same
way and read at the same strength, reads again from that position to the same end. Recognition
is still repeated; construction is not. It holds only where the reading is the same reading,
which a guard or a factory that reads the context or the marks can break.

**First, measured:** in SQL:2023, how many guard builds happen per parse against how many values
the accepted tree holds, on the corpora sql-ff times. If the ratio is near one, there is nothing
to reuse and Q6 closes; if not, the ratio bounds what reuse can give. sql-ff.

**Closed 2026-09-18: nothing to reuse (sql-ff).** No guard or factory in SQL:2023 reads the context
or the marks, so reuse would have been safe; but on accepted parses nothing dropped by a way back
is built again. Per parse, 2,000 parses a corpus: twenty select items 322 builds, 2 dropped, 0
rebuilt; nested brackets at depth 1, 2, 4 and 8 build 54, 67, 93 and 145, drop 2 and rebuild 0 at
every depth; accepted fuzz queries 1.65 rebuilt of 67.5 (2.4%, an upper bound, since the probe's
key could only be the rule and its factory); accepted DDL 0.3%. A refused parse drops everything
it built, which is what refusing means. Counted, not timed; the count needs no quiet window.

### Q7. Further improvements, decided 2026-09-18 by Igor

1. **Factoring shared beginnings across rule boundaries** — go. Most rules stay on the tape
   because of a few causes (SQL-92: 5 direct, 38 under them; T-SQL: 114 and 523); each cause
   removed moves a whole subtree to one-pass construction. Part of D3's track. sql-ff, after Q1,
   design first.
   **Design over kinds 2026-09-18 (sql-39, `docs/design/shared-beginnings-over-kinds-2026-09-18.md`),
   reviewed and decided the same evening.** What the design established: over kinds 342 of
   573 rules of SQL:2023 are read where the reading may not stand, 66 by a cause of their own;
   184 of them are one cycle through `ValueExpression`, held by any of about twenty causes in it;
   and the carrier is chosen per machine (`Machine.Choose`), so nothing in a parse moves until the
   count is zero. Two refinements of `Replay`, exact and not heuristic, take the causes 66 to 43
   and the cycle's closure down by a third: A, a sibling alternative replaces a reading only where
   it can begin with the token the reading began with; B, a failed turn of a repetition or an
   optional is replaced only where what follows can begin with it. The rest is shared beginnings
   in the grammar: C, folding a prefix shared by a run of adjacent alternatives and not only by
   all (`FactorCommittedPrefixes`); D, across a rule boundary, mostly gone after A; E and §5, five
   grammar edits that decide at a token instead of reading and looking.
   Decisions:
   - A and B go, in `Replay.cs`, by sql-39 (performance-ff is on C2; told before the edit, the
     functions named). Their statement is made exact before they are written: an alternative's
     or a turn's failure gives the enclosing context's answer (`elsewhere` from above), not
     `Losing` outright; "can begin with" is over the first sets with nullability — a nullable
     later alternative can begin with anything the choice's follow can, and a nullable follow
     reaches through to the rule's own follow. A control grammar each, the generation-time gate
     on DotGram.Sql, and any snapshot that moves is explained (`CSharpEmitter` reads `Replay`
     only under `Auto`).
   - The first milestone is SQL-92, not SQL:2023: nine causes after A. They are taken to zero
     first, so that the whole chain — the count at zero, `Auto` choosing immediate on its own,
     the stand reading the half `ImmediateSql` promised — is proved on a shipped parser before
     the 43 of SQL:2023 are ground down. Where `ImmediateSql` then still differs from what `Auto`
     chose, that difference is the finding.
   - C is a normalizer pass and re-runs over kinds: expr's, as part of D14 step 3 (the
     optimizing passes separated and repeatable), gauged by sql-39's count before and after.
   - E and §5 are sql-39's grammar edits, each with the `Both` test and the hand parser (D1),
     independent of the rest and started at once.
   - D waits for the count after A and C, and is counted by hand before anything is built.
   - The all-or-nothing carrier is the real limit, and it is what D13's C4 removes for FIX:
     construction at the innermost point past which the reading stands is a carrier chosen per
     position, not per machine. When C4 lands, Q7.1's count says how much SQL gains from it
     without reaching zero; the two tracks meet there and the plan is read again.
   - The counting tool ships as a report, not as a scratch file: what `GRAM5012` counts, listed
     per rule with its cause, the way `--coverage` writes `coverage.md`. Its form is sql-39's to
     propose when A lands.
   **Landed 2026-09-18 (sql-39):** A and B, `0e68711c`, and with them an older defect of `Replay`
   (a part inside a given-up turn stayed `Losing` where the parse succeeds without it; the
   reason from inside is now added to the one from outside); `ReplayTests` holds each case
   beside its opposite; no grammar changes carrier or text; generation no slower. The grammar
   edits: JSON `ON EMPTY`/`ON ERROR` read once (`ed7ed7dd`), the `WHEN` operand (`27b9d04b`), the
   `NOT` before `BETWEEN`/`IN`/`LIKE` in SQL-92 (`a82193ae`), each with tests, corpora, and the
   server for T-SQL. SQL:2023: 325 of 556 building rules on the tape; eight causes left in the
   expression cycle, five of them C/D (shared beginnings), three §5 (sql-39's).
   **The SQL-92 milestone, revised.** After `NOT`, SQL-92's seven causes are all structural — the
   bracket that opens an expression, a row and a subquery in three rules, the select list's
   `name.*` against an expression, the joins' turn, `CORRESPONDING`, two `TRIM` alternatives —
   and T-SQL overrides every rule involved, so taking SQL-92 to zero would rebuild its brackets
   and joins for a parser few use and move T-SQL not at all. GRAM5012 across all 101 hosts
   (sql-39): every SQL and EL grammar carries tens to hundreds of causes; every Web grammar and
   most examples are held not by the graph but by the reader's own way back (`Because.Turn`,
   "can be read again after answering"), which Q7.1 does not touch; near zero only three
   examples. Decided: the mechanism — zero, `Auto` choosing immediate by itself, the answers
   unchanged — is proved on `CaseRegionExample` (one cause, one edit) and nothing more is
   claimed of it; the gain for SQL comes from C4 (a carrier per position, after which
   all-or-nothing is not needed) or, failing that, from the bracket read once designed for
   T-SQL, which waits for C4's answer. The reader's way back is the second gate on `Auto`, to be
   counted per grammar once C2's reader with `recover` exists. Found on the way and blocking any
   immediate carrier over a grammar with a guard beside a group: the open CS0103 `lm` defect (a
   `when` next to a part method under Immediate) — `ImmediateSql` hit it, the `TRIM` merge was
   reverted for it; expr takes it right after C.
   `CaseRegionExample` does not give the proof either: with its one cause removed, the second
   gate holds it (the identifier is read first and given back). `SettingsFile` is held by a gap
   in B — over characters the turn and the continuation begin with the same seam (trivia), so
   their first tokens always overlap; `FollowSets` already compares after the seam
   (`AfterSeam`), `Replay` did not. B takes the seam into account (sql-39, a control grammar
   with trivia); if `SettingsFile` then reaches zero and the second gate still holds it, what
   the reader gives back there is the input for C2/C4. A second CS0103 of the same family:
   `with state @(Region(w))` naming a capture of its own alternative breaks the consumer's
   build instead of a diagnostic — expr, with `lm`; whether a mark may name a capture is read
   from §7.8, and asked of Igor if the text is silent.
   Read: §7.8 already says a mark's value is "what stands over a construction and never
   which", the same every time the site is reached, which a value built from a capture cannot
   be. So the answer is a GRAM error at the site, a line in `diagnostics.md`, and one sentence
   in §7.8 making the rule explicit — a clarification of the text, not a change to the language.
   `lm` is reproduced on all three renderings and fixed (a part receives only the marks
   declared for it; a theory of six cases); both land after C.
   **AfterSeam landed (sql-39):** only `SettingsFile` moved, exactly as expected — `Replay` no
   longer holds it, the second gate does; every emitted file identical; a control grammar with
   trivia. What the reader gives back there, read from the graph: over a seam `File = (Setting &
   eol)* & eof` becomes a turn `trivia & Setting & trivia & eol` and a continuation `trivia &
   eof`; at the end the reader begins a turn, reads its leading trivia, fails at `Setting`, gives
   the turn back with the trivia, and the continuation reads the same trivia again. Decided: the
   seam leaves the head of the turn in the graph — `(t S t e)* t eof` is rewritten `t (S t e t)*
   eof`, which holds because the seam is a greedy star (`t t` reads what `t` reads) — a
   normalizer rewrite in the optimizing passes (expr, D14), not a special case in `Determinism`
   and the reader; every analysis then sees the turn begin past the seam. `SettingsFile` is the
   proof case: both gates at zero, `Auto` choosing immediate on its own, answers unchanged.
   SKILL.md for Sql is done (`87741771`).
   Held until verified: the reading of what the reader gives back is sql-39's from the shape
   of the normalized rule, not from the emitted code; sql-39 checks which repetition gets a way
   and why `NeverGivesBack` says no before expr writes the rewrite.
   Verified (sql-39, by variants through the compiler and the emitted code): two causes,
   independent. (1) The seam at the turn's head, confirmed — the rewritten form compiles
   immediate, the original opens a way per turn and one inside the trivia rule; expr's rewrite
   goes. (2) `eol` itself: the choice `"
" | '
' | ''` keeps a way into itself after
   answering `"
"`, since `''` alone is a later alternative, although nothing after a turn
   can begin with `'
'`. Decided: `Determinism` asks the question it asks of a seam of a choice
   too — an accepted alternative is settled where no later alternative reads a proper prefix
   of it whose remainder the follow can begin with — sql-39 (an exact analysis, the same family
   as A/B), with performance-ff told, since the reader stops opening a way where it says so and
   every line-based grammar changes emitted code; corpora, the refusal record, and a stand pair
   on the feed rows, where fewer ways per line may show. With both, `SettingsFile` is the proof.
   Corrected by sql-39 before writing: the analysis already exists (`Machine.Analysis`,
   `PrefixSettled`) and the engine reads it; the reader over characters opens a way into every
   non-predictive choice regardless and uses the analysis only for the refusal message. So the
   change is in the reader's choice emission, not `Determinism`: sql-39, a separate commit before
   C2, the same conditions, performance-ff rebasing C2 on it.
   **Landed `014136df`:** a control pair; snapshots and the refusal record unchanged; no package
   grammar's code moves (Web, Finance, SQL, EL byte-identical), seven grammars of examples and
   benchmarks do; the gate flat (13.3/12.9/13.2 to 13.3/13.2/13.0 s). `FixedWidthExample`, whose
   only cause was `eol`, now compiles immediate by `Auto` on its own with its tests unchanged —
   the mechanism proof, arrived at before `SettingsFile`. `SettingsFile` keeps the seam only.
   **§5's three, decided 2026-09-18:** each is the language's own ambiguity, not the grammar's.
   `AFTER` is not reserved, so `AFTER (x) IN y` is both a flag with a pattern and a function; the
   two `VALUES` are told apart only by what follows the rows; `(a, b) AS query` against a list of
   elements only by the `AS` after the bracket. The first two cannot reach zero without changing
   answers; the third has one exact form (the bracket read once as elements, a guard turning
   them into names where `AS` follows), which is a rewrite with a guard beside a group. All
   three are recorded as intentional, like the six atomic ones, and not touched; the third is
   taken up only when C has brought the cycle to one or two causes and it is the one left.
   **The causes report (sql-39's form, approved 2026-09-18):** what GRAM5012 decides goes into
   the existing `*.DotGramReport.g.cs`, under the same MSBuild property, as rows — the carrier
   `Auto` chose and the gate, each building rule the tape keeps with its `Replay.Because` and,
   for a direct cause, its site (the owner rule; which later alternative overlaps on which
   token, or what fails after a turn, or the lookahead), for an `Under` the direct cause it
   hangs under, and for the second gate the rules the reader can read again with the node that
   opened the way. Sites are kept only under the property, so the ordinary generation carries
   nothing. `--carriers` in the benchmarks project collects the reports and writes
   `docs/carriers.md` ("written by --carriers, never edited"; a line in the layout and in the
   docs index): a table per grammar — building, kept, held by `Replay`, direct causes, second
   gate, carrier — then each grammar's causes by owner. C and the seam rewrite are measured
   by its diff.
   A quiet machine reads T-SQL's generation at 4.4 s, which the morning's 4.1 and the evening's
   4.6-5.7 bracket.
   **The report landed `597af083` (sql-39):** sites and the opening node are kept only under the
   report option; generated code unchanged; the path without a report does strictly less (the
   gate, three rounds, flat). `docs/carriers.md` is in the docs index; the layout line in
   CLAUDE.md is left to Igor by the session's own rule. First run: 84 grammars, 53 on the tape;
   SQL:2023 327 of 556 before C, 29 with a cause of their own.
   **C landed `5ae11836` (expr), `lm` `58aee5f7`, GRAM4030 `bdef4949`:** SQL:2023 325 to 308 of
   556 building rules on the tape; T-SQL and SQL-92 unmoved (625 of 640, 44 of 48); only the
   four SQL parser files differ, the two fixes change no generated file; the gate inside
   tolerance (T-SQL 1.04x, located 1.03x, SQL:2023 1.00x, SQL-92 1.06x). The seam hoist is next.
2. **Diagnostics off the hot path** — go. Where recording the furthest failure stands in the way of
   a faster reading, it leaves the fast path: the fast reading records nothing, and a refused
   input is read again with recording on, which gives the same message. Where a second reading is
   impossible (a stream past its retained window), the case comes back to the architect before
   anything is weakened. expr-2d, after `parserMarks` (the expression language is where recording
   costs 15-25 per cent).
   **Design 2026-09-18 (expr-2d, `docs/design/diagnostics-off-the-hot-path-2026-09-18.md`).**
   Recording is a fifth to a quarter of an EL parse (880 sites) and nothing on SQL. Two places
   record for nobody and go first: `find` over a string, and the lexer's value and measure
   re-reads. Then the in-memory `Parse`/`TryParse` forms: a held `bool` gating every
   recording site (one body, C# 8 floor), a refused input read again with recording on; a
   generic-flag reader only if the stand shows the bool costing more than noise. Streams,
   recovery and `yield` keep recording as today. Refused input then reads twice, up to 2x where
   recording was nearly free (FIX, web headers): the stand gets refused-input rows per family.
   Under Immediate a refused input runs constructions up to twice; the carrier's contract says
   so. Implementer: expr-2d, in the generator, coordinating files with performance-3f.

   **For Igor: the context on the second reading.** The generator cannot rewind a type it did
   not write, so a grammar declaring a `context` (the expression language) gets a second reading
   only if the language says how the context is restored: (2) by duck typing, the resolver
   finding `Mark()`/`Rollback(mark)` on the context type as §7.3 finds constructors, with an
   Info diagnostic saying what was found; or (3) by an explicit clause on the `context`
   declaration. Without either, a grammar with a context keeps recording on the hot path.
   The architect recommends (2) with the diagnostic. **Decided 2026-09-18 by Igor: (2), the
   generator does it itself** — it finds `Mark()` and `Rollback(mark)` on the context type through
   the resolver, marks before the quiet reading and rolls back before the recording one, and says
   so in an Info diagnostic; a context type without them keeps one recording reading, and the
   diagnostic says that too. **Landed 2026-09-18 (expr, `1630f936`)**: the resolver answers
   `Rewinds`, `GRAM5013` says it either way, the emitter marks and rolls back around the quiet
   reading; gated by `ExpressionRefusalTests` (1,933 recorded refusals with the state after each).
   Paired: EL accepted -2..-11% with less allocated on every row, refused +61..88%, FIX and SQL
   identical. Q7.2 is complete.

   **Landed 2026-09-18 (expr-2d).** Step 1, `cf16f1cd`: `find` over text and the split lexer's
   re-reads record nothing. Step 2, `d4f9a45c`: in grammars with no context and no `recover`,
   an in-memory `Parse`/`TryParse` reads quietly and reads again with recording only if it
   refused; equivalent field by field on 2,466 recorded refusals (`RefusalTests.txt`,
   `385f935d`) over engine, tape and immediate. Paired: SQL:2023 accepted -1..-6% time and
   less allocated on every row (the quiet reading makes no tie list); a late refusal +98%
   time; EL and FIX byte-identical, since EL has a context and waits for the decision above.

   **Found beside it (performance-3f):** `Ways.Lookahead` is read by `Refuse_DotGram` and never
   incremented since `487362c5`, so a refusal inside a lookahead is recorded on the reader and
   not on the engine, and the two renderings can report different positions. A defect with a
   test to write: reader and engine agree on the position of a refusal inside a lookahead. **Fixed
2026-09-18 (`c4587e05`, performance-3f)**: the depth is counted on `Failure`, which every reading
carries. The ruling it needed: a refusal's position is where the parse refused, not where a look
got to, as the language says; the expression language's positions moved by one or two on 32
texts, and `HandExpression` followed in the same commit. A lookahead keeps a grammar off the flat
path, so the test covers the reader and the engine.

3. **Large literal sets as tables** — go, generator part after finance-03's Fix44 report.
   **First step landed 2026-09-18 (finance-03, `9a80c42c`): a fast refusal on a prefix-table
   miss.** Where a choice of literals has a prefix table (only Fix44 in this repository, 16
   tables), a miss inside the table no longer replays the whole alternative chain: it records
   the same expected literals at the same position and goes to `Fail`, proven equivalent by a
   test compiling the grammar with and without tables over every short input. Fix44 BinaryMany
   -93% (string) and -85% (stream); Order unchanged once the cold miss block was hoisted out of
   the hot method (in it, +4% on stream). Fix44 itself is not an example but an oracle and the
   stress fixture for literal sets, and moved to `tests/DotGram.Finance.Fix44` as `Fix44Parser`.
4. **One measuring stand** — go. One command for the ratios to the hand parsers of FIX, EL and
   SQL:2023, allocation, peak memory, first call, a control row and the agreement check. Scratch
   and results on `T:\TEMP` (a fast RAM disk; losing it costs nothing), not in `.work`.
5. **Specialization by a struct type parameter instead of copies of code** — possible, with one
   constraint: the generated code's floor is C# 8 on netstandard2.0 and net472
   (`DotGram.Compatibility`), where a `ref struct` cannot be a type argument. That
   is a runtime limit (byref-like generic arguments arrive with .NET 9 and C# 13's
   `allows ref struct`, and the compiler refuses them on older targets), not a compiler one. It
   costs less than it sounds: the reader may itself be a `ref struct` holding the span and be
   generic over a plain struct that says how its source behaves; only the type argument may not
   hold a span. The runtime specializes generic code
   over value types on every target, .NET Framework included. Prototype first: JIT time and first
   call when several forms are used. It is how D7's one input abstraction is implemented, and Q4.2's
   locations prototype uses the same technique.
6. **Generation time and first call as metrics** — lower priority; columns of the stand (4).

**4 and 6 done 2026-09-18 by the architect**: `--stand` and `--stand-compare` in `DotGram.Benchmarks`
(`Stand.cs`, `benchmarks/README.md`). First run, `cd707572`, pinned, control 31.5 ns; generated
time over hand-written:

| family | rows | generated / hand |
| --- | --- | --- |
| FIX, string | One, Order, BinaryMany, 128 orders | 2.5-3.7x |
| FIX, bytes in memory | the same | 4.4-5.3x |
| FIX, stream | the same | 2.1-4.3x |
| EL, tape | seven expressions | 1.5-3.4x (interpolation the worst) |
| EL, immediate | the same | 1.1-2.5x |
| EL, untyped lambda | one | 1.0x (both in the host) |
| SQL:2023 | nine productions | 8.3-21.8x |

First call in a fresh process: FIX 10.7 ms against 7.8, EL 28.1 (tape) and 20.0 (immediate) against
15.2, SQL:2023 26.8 against 4.4 for a literal and 106 against 21 for twenty select items. A lazily
streamed FIX parse of two million fields holds nothing measurable above the floor on either side.

**The input forms, generalized (Igor's question).** The forms are two independent axes, the
symbol (`char` or `byte`) and the source (in memory, or pulled a block at a time), plus what a
source does at element boundaries (lines insert `\n`). String and `ReadOnlySpan<char>` are text
in memory; `IEnumerable<string>`, `string[]` and `TextReader` are text pulled; `byte[]` and
`ReadOnlyMemory<byte>` bytes in memory; `Stream` bytes pulled. So the generalization is one
machine per symbol type, generic over its source (5), and a source per form, rather than one
machine per form.

## D18. A third directive: `read R`

Decided 2026-09-19 by Igor. Besides `parse R` (the whole input is an `R`) and `find R` (`R`s inside
something else), `read R` reads one `R` exactly at a given position and answers its value and where
it ended, leaving the rest of the input untouched — for tokenizers and framing driven by the host,
a generated reading inside a hand-written parser, and parsing at a position without a copy. Its
methods are `ReadR` (the name is the BCL's for reading one item and advancing: `BinaryReader`,
`Utf8JsonReader`), over strings, spans and bytes held whole; no stream form, since a buffer reads
ahead and cannot leave a stream just past the rule — a stream is read element by element with
`parse … yield`. Where the rule ends is its first successful derivation, in the order of its
alternatives, as everywhere in the language, not its longest match; a grammar that needs a boundary
says so with `?!`. Leading trivia is skipped, trailing is not consumed. It was measured against the
alternatives `match` (read as a search, as `Regex.Match` is), `next`, `take` and a `prefix` modifier on
`parse`.

**Revised the same evening, Igor: no new directive — the positional forms are finished instead.**
Reading one rule from a position is already in the language: §6.3's `TryParseX(input, at)` begins
there and need not reach the end, and `TryParseX(input, at, length)` also bounds what it sees; over
tokens the second may begin anywhere and ends at a character no token begins with. The architect
proposed `read` without checking that, and expr's design found it. What the positional forms lack
is what gets built: a form that moves the caller's position and answers yes or no
(`bool TryReadR(input, ref int at, out R value)`), a form over bytes held whole, and the choice of
leaving the trivia after the rule for the caller. Lazy tokens belong to the first step, not a
later one: a loop of readings over one long text must not cost the square of it. The text of §6.3
says what the finished set is; expr writes the design, and §6.3's wording goes to Igor before code.
**Igor, 2026-09-19: a break in shipped behaviour is not a concern at this stage** — the releases
are trials and there are no users. So the positional forms all stop where the rule stops, one
behaviour rather than two; the expression language's one caller, which measures a lambda's body by
the match's length, is fixed in the same commit, and the release notes carry the line.
Checked against the SQL grammars (sql-39): over tokens the positional form already ends at the end
of the last token, so the trivia after a rule is no difference at all; what it cannot do is begin
where no token begins — a comment, for instance — and the host is then made to implement the
language's trivia itself, line comments and nested blocks included. So the form skips leading
trivia and begins at the first token at or after the position, its match saying where it really
began; §6.3 says the opposite today, and that is the sentence going to Igor. The window form does
not answer it, since it wants the length, which is what the reading is for. Lazy tokens are the
acceptance criterion, not a later step: a script of thousands of statements read one at a time is
the case. §6.3 also gains a sentence on what `eof` means from a position (the end of the input; in
the window form, the end of the window); the follow is seeded with End, and no second seed.
**The lazy tokens, decided 2026-09-19 after expr reconsidered his own design.** The loop costs the
square today — a T-SQL script read a statement at a time is 56 µs, 1.9 ms, 30.3 ms at 10, 100 and
400 statements — because every positional call tokenizes the whole input. Tokens made as they are
asked for would be correct by construction but give a split grammar a second rendering for its
positional forms, and T-SQL's file is 14.5 MB already; a doubling window stays refused, since a
negative lookahead peeking past the edge answers the other way. So the tokenization is kept
between calls, which is the same eager tokenization and therefore the same answers, and makes the
loop linear: two slots per thread, for strings only — an array the caller may rewrite is not
cached — the input held weakly so that a large document is not pinned, the tokens dropped when
another input arrives. Two slots because a host may alternate between documents and because a
parse inside a factory would otherwise evict the outer reading's tokens and bring the square back.
A scaling test holds the loop linear. What it does not answer, and the design says so, is one short
reading out of a huge text; tokens on demand stay written down as the answer if that case ever
arrives.
**Written (expr, four commits):** a reading from a position stops where the rule ends (paired
flat); the forms that move the position and answer yes or no; the tokenization kept between
readings, which takes the loop's exponent from 1.99 to 1.00 — 400 statements 42 µs against 1.7 ms;
and a reading over tokens beginning at the first token at or after the position, with nothing but
trivia left answered as a starved input rather than a refusal to begin. It carries a defect found
by its own test and unrelated to position: the extent handed back was cut by the index the reading
reached rather than by a count from where it began, so an extent-only rule read from anywhere but
the start cut too much and threw below the first token; nothing in the solution reads one that way.
Left over and decided: over characters the position reported is still the one handed in, so a value
with trivia before it carries it. That is done next, in its own commit before the release chores,
because §6.3 now promises the position is where the value begins and a promise kept for tokens
only is two behaviours again. **Done (`6faa6e83`):** one field on the record a failure already
carries, written by the reader's entry between the trivia and the rule; the engine reads the
trivia as a reading of its own and so skips it at a position, which it never did, and a test holds
the two renderings to the same answers. The caveat is out of §6.3: the position is where the value
begins and the length its own extent, in both halves.

## D19. What 0.2.0 owes before it is cut

Asked by Igor 2026-09-19 and audited against the tree (654 commits since the tag of 0.1.0).
Blocking, in the order they matter:
- **The package smokes stopped proving anything.** At 0.1.0 the build smoked Sql, Web, the
  expression language and Finance, on two frameworks; three of those projects are gone, the step
  installing the older runtime with them, and the two that remain pin the 0.1.0 package and keep
  the public feed among their sources, so they resolve the released package instead of the one
  just built.
- **The version and its notes.** The version is still 0.1.0, four packages of five still say
  "First release", `GRAM0009` is still unshipped in the analyzer's release file, and the version
  is written by hand in six more places.
- **Finance breaks its consumers.** Its public surface is replaced: the field type is a class
  where it was a struct, the old model and the whole generated tree are gone from the package, the
  grammar now lives in a test fixture, and the dual licence with its third-party notices was
  dropped. Legitimate, but the notes must say it, and the csproj still carries the old licence
  arrangement's leftovers.
- **There is no way to publish.** No release workflow and no tag-triggered job; the build only
  uploads artifacts.
Smaller: no badge for Finance, XML documentation only in Finance, no source link or symbol
packages, the extension's project nominally packable, its notes still "Initial preview".
Sound: no TODO or unimplemented marker anywhere in the sources, a clean tree, the analyzer's
packaging asserted by the build, compatibility compiled at the C# 8 floor, and the gaps in
behaviour named in `status.md` rather than hidden.

**Assigned 2026-09-19 on Igor's word, and a break is no concern at this stage.** finance-24 has
the mechanics: the three smoke projects and the older runtime restored, both smokes taking the
packages just built rather than the feed's (and failing when a version is wrong), a publish
workflow by tag, source link and symbol packages and documentation for all five, the missing
badge, the extension's project marked unpackable and its notes given a version heading, and the
notes for Finance and Web — Finance's saying plainly that its public surface was replaced. expr
moves the unshipped diagnostics into a 0.2.0 block and writes the notes for the generator and the
expression language; sql-39 writes the SQL package's. The version itself is raised last, by
finance-24, when the stand's windows are done, together with the six places that spell it by hand.
Symbols stay embedded, as the packages say deliberately, with source link beside them and no
symbol package — the two are alternatives. Documentation is turned on where writing it is an
evening's work, the web's 82 places and the expression language's 8 — which turned out to be more,
and turned up a defect: the generator wrote doc comments on a local function, which C# has no place
for, so a consumer who generates documentation got a warning out of our emitted file. The class is
closed by compiling the generated code with documentation on and warnings as errors in the project
that exists for compiling it. A grammar's own diagnostics are not declared descriptors, being made
as the message arrives, so the analyzer's release files hold only the fixed ones and
`diagnostics.md` stays their catalogue and says so; declaring them all statically would mean a
second table kept in step by hand for almost no gain to a consumer, and is not done. Documentation
is left off for the SQL
tree's 6,500 and the generator's 908: a file-wide suppression over a whole package is an empty
flag. Recorded as those packages' debt, not as anything the release waits for; the SQL notes say
the package carries no XML documentation yet.
**Done 2026-09-19 (finance-24, `19a60765..6bed33a3`, nine commits):** the three smoke projects and
the older runtime are back and all four libraries are smoked on both frameworks; the smokes take
the packages just built and fail at restore without them; a publish workflow runs on a tag, checks
that the tag matches the version, packs the five and holds the push until the repository has a key;
source link everywhere; the web package documents its whole surface and fails its build on a
missing comment; the extension is unpackable and its notes have a heading per version; the notes
for Finance and Web are written, Finance's saying plainly that the public surface is replaced.
What is left for Igor: the name of the key the workflow will use, and when the version is raised —
with it the six files that spell it by hand.

## D20. The emitted code may take what the consumer's framework offers

Igor's, 2026-09-19: what the generator emits is compiled in the consumer's project, so a `#if` in
it selects the consumer's framework, not ours. Today everything is written to the floor — C# 8 and
what netstandard2.0 has — so a consumer on a current framework gets code written for net472.
Worth taking, in the order of what they promise: a search over a set of characters or bytes
(.NET 8), which also removes our own rule that a stop set of more than five characters is read one
character at a time; the search for the first character outside a class, which is every run a
scanner reads; recognizing a keyword from a span without making a string (.NET 9) and frozen
tables, where a grammar over tokens spends its time — SQL has 410 words; and the ASCII helpers
for comparisons that ignore case. Skipping locals' initialization and pointers are not part of it:
D10 makes those an option, not a default. Conditions: the floor branch stays byte for byte what it
is today, so no reading on an old framework can regress; both branches are compiled and held by
tests; the cost in emitted size is stated; a pair compares the two branches on one platform rather
than two commits; one item at a time, each with its pair. performance-ff designs it after FIX's log
form, with the stand's measure of what compiling for the newer framework already buys our own
libraries as the ceiling to expect.

## D21. A session that argues: the critic

Decided 2026-09-19 by Igor, on the architect's account of its own day: the architect makes the
decisions and no one reviews them, and it showed — a directive proposed without noticing the
language already had the form, a conclusion drawn from a report that was miscounting, a commit
allowed to land without its pair which cost a factor of eight in the expression language. Each was
caught by the session doing the work, not by the architect. And the one idea of the day that
looked outward — that emitted code may take what the consumer's framework offers (D20) — came
from Igor: five sessions were executing and none was looking around.

So a session named `critic`, reporting to Igor directly and not through the architect.

- **It writes no product code and takes no measurements.** It reads the repository, this journal,
  the designs and what the sessions report. It needs no timing window, so it does not lengthen the
  stand's queue.
- **Three duties.** To argue with the architect's decisions before they reach Igor or the work; to
  hunt for what is not being used — a framework's newer API, an algorithm, the shape of the public
  API, something done by hand that could be generated; and to check the claims plans rest on, of
  the kind "this is the language's own ambiguity" or "4.8% of the records are in settled
  subtrees", by its own reading rather than by trust.
- **It writes one file**, `docs/design/open-questions.md`: each objection with its date, what it
  rests on, and the answer it was given, so that an objection cannot be quietly dropped.
- **An objection comes with evidence** — a place in the code, a number, or a reproduction — and no
  more than two or three at a time.

**Its first three, and what they changed (2026-09-19, `docs/design/open-questions.md`).** The
carrier per construction was shelved on an unweighted count of records with "the time share is the
count share" standing in for the weighting the step had been set with — which the anatomy of the
same evening contradicts, since the prologues and the guard's walk are fixed costs per call; the
arithmetic also gives 0.7-1.2% rather than 1-2%, and the fourth edge, dismissed with it, 3.4-5.7%.
The number is taken again, weighted, after the prologue lands. Of the three ambiguities only the
bracketed value against a subquery has a witness: the join's turn has none, and if none is found it
is a question for the analysis and 29 rules come back; the inline return is our own rule, removable
at a named price, which makes it Igor's trade-off and not a fact about T-SQL. **The door tried and
shut (sql-39):** one alternative with optional brackets and a guard pairing them does remove the
cause from the report, and frees nothing — the same rules are held by the subquery's cause, and
the reason is the one the critic named: a guard refuses a reading, it does not make anything try
another, so what is wanted is the same possessive form the join's turn wants. The analysis now has
two clients rather than one. **And a gap in fidelity found while checking it:** the server reads
`CREATE FUNCTION … RETURNS TABLE AS RETURN (SELECT 1 AS a) UNION ALL SELECT 2 AS a` and we refuse
at the token after the brackets, though the query rule alone reads it. Under D22 that is a defect
and goes before the anatomy. **Answered by sql-39
the same evening (`0ab26872`):** there is no witness for the join's turn, so the 29 rules are back
in the work, and the cure cannot be said in the grammar today — an atomic group round the tails or
round the call changes nothing, because the cause means "the caller may ask the rule again" and a
group does not take that away. It is the analysis's, in the words he gave it: a call needs no way
back where any shortening of its reading would resume on a token the continuation cannot accept.
sql-39 takes it after the anatomy. The shelved carrier is marked provisional, its arithmetic
corrected, and it is re-derived weighted from one cost model together with the anatomy. **Half of that
re-derivation is in (sql-39, on today's main):** the recipe reproduces the record count exactly,
and of 116,946 records 4,130 are in settled subtrees — 3.5%, against the 4.8% written before,
the difference being that a rule now counts as settled only where every call it makes is settled
too; both numbers stay in the document with that reason. The weighting itself is the line that
matters: of 17,023 walks only 174, one per cent, build nothing but settled records, so the fixed
cost of a walk — which the anatomy says is most of materialization — is removed almost nowhere.
Weighted, the estimate falls rather than rises. The two constants come from the profile, and the
conclusion then says plainly whether the shelf is final. On FIX the comparison
is honest — the stand holds both parsers to the same answers before timing — but the stronger
sentence was left out of the table: against the ideal reader we built as a floor, the generated
parser is 1.04x (48.9 ns a field against 46.8), where it was 3.1x the day before by the stand's own
arithmetic, and that cannot
be answered with "the hand parser is slow"; the byte and stream forms have no ideal reader, so
their 1.71x and 2.19x stand against the hand parser alone. And when the emitted code takes a newer
framework's API (D20), the hand parsers get the same in the same commit, or the pair will show a
gain that is only their handicap.
**The pattern behind the architect's mistakes of the day, as the critic put it:** each was a number
or a form accepted on the word of the session that produced it, at the moment it agreed with the
plan. **Rule taken from it:** where a decision turns on one number, the entry names the assumption
that number rests on, so the next measurement can be held against it rather than filed beside it.
- **It may speak to any session and to Igor directly**, including where it holds the architect
  wrong, which is the point of it.

## D22. A parser answers as the specification and the server do, before it answers fast

Decided 2026-09-19 by Igor, asked about one trade-off and answered as a rule. T-SQL's inline
return could be written so that a cause of `Replay` disappears and 26 rules under it leave the tape
(27 with the rule itself), at the
price of accepting a `WITH` and an order inside brackets that the server rejects. The price is not
paid. Any change that would have a parser accept what the published syntax or the server refuses is
declined however much it buys; where one buys a great deal it comes to Igor with its number and
the exactness it costs, and the default answer is no. This is D13's order of criteria applied to
acceptance rather than to correctness of construction: first the parser reads what the language
reads, then it reads it quickly.

## D23. The generator documents what it makes public

Igor, 2026-09-19: every public member the generator emits carries XML documentation. It follows
the defect of the same evening — a doc comment written where C# has no place for one broke a
consumer's build — but it is the larger half of the same subject: a consumer who turns
documentation on today gets a warning for every entry we write, because we write none. The design
is expr's, before code: which members count as public in emitted code (each publication's entries,
the types nested in the host, the values a grammar publishes), one template per form rather than
a sentence per method, what the templates say for the positional forms' position and length and
for the lazy form's window, and what is not ours to say — the meaning of a particular rule belongs
to the author, and whether an author may supply that text is a change to the language and Igor's
to decide separately. Held by compiling the generated code with documentation on and the
missing-comment warning as an error, so an undocumented member fails here rather than for a
consumer; the cost in emitted size and the snapshot churn are stated with it.
**The design, approved as written (expr).** The scope was asked of the compiler rather than
remembered: what a consumer sees is the host's own methods and the types nested in it, and what is
undocumented is everything emitted outside the publication's own entry — the reader and stream
overloads, the byte ones, the lazy entry and its iterator, the buffered `Try`, and `find` over a
reader. One template a form, built once in the emitter: what the throwing form throws, that the
match answers instead, that the bool form says only whether, what §6.3 now says of a position and
a length, that a reader's input is the caller's to dispose, that an enumeration is lazy and
stopping early leaves read-ahead in the buffer. What a rule means stays the author's: the
templates say a rule is parsed, never what it is, and expr says plainly that this will not read as
written prose. Whether an author may supply that text in the grammar is a change to the language
and goes to Igor on its own; the templates reserve nothing for it. **Written, and the cost is smaller than the design guessed**
(`a830c3aa`): three files in the solution grow at all — a feed example by 0.3%, FIX by 0.8%, the
FIX 4.4 fixture by six thousandths of a per cent — and T-SQL and the JSON example not by a byte,
since they publish no buffered or stream form and their string entries were documented already.
expr's own estimate of a few hundred lines for T-SQL came from counting its entries rather than
the undocumented ones, and the measurement corrected it. Coverage is the missing-comment warning
staying silent with it on, over three frameworks, warnings as errors. **Igor, 2026-09-19: not now —
there is more important work — but kept as a question for later.** So the emitted documentation
stays the generator's own words until someone asks for the author's.

## D24. What a refusal costs, measured

The critic's objection that a compiled regular expression refuses faster than the generated parser
sent finance-24 to measure it on today's main, twice by different means, with a profile. The
premise half held: on a media type the compiled expression is level with us (48 ns against 42),
on a URL it is faster (128 against 192), and the interpreted one is slower than us everywhere.
Where the time goes is not what either of us assumed: recording what was expected costs nothing,
since a quiet reading writes nothing and both refusal rows allocate nothing at all. It goes on two
things. The entry rents the value store before it reads and returns it in a finally, though a
refusal never touches it — 20 ns of a 42 ns refusal, and the return sums eight lengths and clears
eight empty arrays even when nothing was used (performance-ff, after his queue: rent it only where
a reading succeeds, or leave early on return, chosen by the numbers, with the accepted rows as the
control). And a URL's refusal reads the input twice, since a reference is a URI or a relative one
and both start from the beginning: a fifth of the run, and reading the common beginning once would
take the refusal to about 145 ns, level with the compiled expression. The fold cannot take that
one and expr said so plainly: the two alternatives begin with different captures of different
rules, and what they share is that they read the same characters under different names, which is
a fact about the language and not a prefix. The cure is the grammar's own — a lookahead for a
scheme and its colon in front of the first alternative, so that a refusal re-reads the scheme
rather than the whole input and an accepted relative reference never tries the other alternative
at all — and it is finance-24's to write and measure. **Done (`12a8804f`), and better than the
architect proposed:** the look is a negative one on the second alternative rather than a positive
one in front of the first, so an accepted URI pays nothing at all and only an accepted relative
reference walks the scheme text twice; the refusal is 21% faster and the accepted reading
unmoved. It is a theorem and not a guess, and the reason stands beside the rule: a relative path's
first segment may not contain a colon, and every later segment is behind a slash where the scheme
text would have stopped anyway. Six refusals were printed before and after: the same texts and the
same positions, the look never reaching a message.
**A measurement rule came with it:** the first 300 ms of a process runs five times slower, and it
is neither tiering nor the collector — the same row measured last in the same process is at its
final speed from the first repeat. A short probe on a cold process overstates its first row about
fivefold; finance-24's own first numbers were retracted for exactly that.

## D25. What the generator can settle, it settles; a fork the user owns stays a fork

Igor, 2026-09-19, and restated by him the same evening after the first wording was read too
widely. The rule is about **static choices**: wherever the generator can decide, while building
the parser, which way of working is better, it decides — the emitted parser does not carry the
choice to run time and pick. That is what makes it a machine that does as it is told.

A fork the *user* controls is another matter and is legitimate: the parser must read what the
user's code or the user's data says, because the grammar asked for it. What is asked of it is
the preference, not a prohibition: **keep such a fork off the main path** — decide from the
generator's own tables first and ask the user only where they say nothing, as with a tag outside
the standard set. How far that can go depends on the grammar: our short FIX grammar is written
without knowing which tag it is reading, so its guard is asked for every field by construction,
and no rule of ours changes that — only a different grammar would.

So the earlier consequence drawn from the first wording is withdrawn: the FIX options a consumer
supplies, read by the grammar's own guard, are a fork the user owns and they stay. What the rule
does forbid is the parser choosing *behaviour* for itself — which factory, which reading, which
strategy — on anything that is neither the input, nor the grammar, nor something the user
declared. The critic's three-way test keeps its shape with that third clause added: only
allocation is housekeeping; the same answers in fewer steps by a rule fixed when generating is
memoization; different steps for a reason the user did not declare is strategy, and is out.

**The compromise, Igor the same evening: user code may settle its own fork early.** A fork the
user owns need not be asked on every field — it can be answered once, where the user's code is
cheap to ask, and read from a table afterwards. For FIX: when the options are built, the
package's table and the consumer's pairs are merged into one byte a tag, indexed by the tag, with
a sparse dictionary only beyond the array; then the grammar's guard reads one cell a field, with
no dictionary, no set and no branch on whether the consumer supplied anything. The fork stays the
user's, the grammar and the generator are untouched, and the main path keeps an array read. The
price is stated in the package's own documentation: to change the pairs, build new options. The
move generalizes — wherever a user's fork is asked during a parse, the question to put first is
whether it can be a table built when the options are.

**The audit under the first wording, and a defect it turned up (critic, expr).** Nothing emitted
asks a type what it is, asks the hardware what it supports or reads a policy from settings; the
kept tokenization is memoization and the pools are housekeeping. But under the first of those sat
a real defect, reasoned out by the critic and said to be unreproduced: an eviction handed a live
reading's token arrays to the thread's spares, and the next tokenization wrote over them — three
nested readings inside one outer reading reach it, a level deeper than the two slots were written
for. Confirmed and fixed (`9428418d`), test first, and the test failed silently: a reading answered
`true` with a value cut from another text's offsets. Tokens leaving a slot are no longer pooled,
at an eviction or when the text is collected; the price is garbage at an eviction, which two slots
make rare.

Found while checking the code against that wish (critic): the two halves of the FIX options are
written the two opposite ways. Whether a tag carries data asks the package's table first and the
consumer's set after; finding a tag's pair does not consult the package's table at all once a
consumer has supplied a dictionary, because a supplied dictionary replaces the standard
forty-two — which is deliberate and documented, and a trap of its own: adding one counterparty's
pair drops all the standard ones unless they are listed again. Merging into one table settles it
by construction, but changes what the package promises from replacing to extending, so it is
Igor's: extend, with a consumer's tag that contradicts the standard refused by name, or keep
replacing and give extending a way of its own. Either way the two halves are written alike.

## D26. FIX: two things Igor asked for, 2026-09-19

**Compared with the libraries that already exist**, QuickFIX and whatever else the platform has,
free or not. What is wanted first is an account of them — what each does, what its model is, where
it differs from reading a wire into typed fields — and only then numbers. The rule that governs
the numbers is the one the stand already follows for the SQL and JSON references: like is compared
with like, and where a library does more on the way (validating against a dictionary, keeping a
session) that is said in words rather than hidden inside a ratio. A licence is checked before
anything is referenced, and nothing of the kind enters a shipped package: this repository has just
finished removing somebody else's notices from one.

**Validation dictionaries.** What FIX calls a data dictionary — fields and their types, value sets,
components, groups, which fields a message requires — and what the trade body now publishes as
Orchestra. The account wanted: what such a dictionary holds, what of it the package already has in
its schema and its strict and lenient modes, what it does not, and what a dictionary would buy
that we cannot do at all — a venue's own extensions above all. The design question is set by D25
before it is asked: a dictionary that changes how the wire is read is compiled when the parser is
generated, while a dictionary that only checks a message already built may be data at run time,
because checking is not reading. Where that line falls exactly, with examples, is what the design
has to say. finance-24, both to the architect before code.

## D27. FIX: a consumer's tags are a switch in the consumer's own code, 2026-09-19

Igor, asked where a consumer's fields are built, drew the shape: a big switch over tags; the tag
that falls through reaches the default arm, and there a question — is this tag's field binary —
opens two ways of building it. And all of that, he said, is in the consumer's code.

So the package keeps exactly what it has. Its own switch over 912 tags is ours, compiled, sealed
and untouched; a known tag pays nothing it does not pay today. The one thing that changes is what
its default arm does: instead of always building the fallback field, it hands the tag and its
value to the consumer's object where one was supplied — one null check and one virtual call, on a
path that already allocates. That is shape **E** of `fix-custom-fields-2026-09-19.md`, read the way
Igor reads it: not a table we look things up in, but a seam the consumer's own switch hangs on.

The consumer's side is then their program, not our configuration. Their override is their switch
over their tags, and their default arm — the tag neither of us knows — asks whether the tag is
binary and builds one of two things. Which means `IsData(tag)` is a question the *consumer's code*
puts to the package, a public read of the table we already keep, and not a question the parser
puts to the consumer. D25 is satisfied by that direction of the arrow: while reading, nobody asks
a consumer's object how to read; afterwards, a consumer's code asks us what we know.

**Framing is the exception, and it is settled at build time.** Whether a tag begins a length/data
pair has to be known *before* its value is read, so it cannot be a question asked of anybody in
the middle of a field. A consumer's pairs are declared when the options are built, and the fork is
answered once, there: the standard sixteen and the consumer's own are folded into one table
indexed by the tag, and the parse does a bounds check and a load. finance-24's design has this
written: `sbyte[]` to 65,536 with a rare dictionary above it, a shared static table when nothing
was supplied, and `DataTag` off the per-field path into `BeginData`, which is called once a pair.

**The dictionary extends the standard sixteen; it does not replace them.** Architect's ruling,
Igor to overturn. Replacing is a trap nobody chooses on purpose: adding one counterparty's pair
silently drops 95/96 and the other fifteen, and the damage surfaces far from its cause — the first
half reads as an ordinary int, the second half finds no arm, the field is refused and recovery
resynchronises on a separator that may sit inside binary payload. We are saved from silent
corruption only by the two halves being written differently, which is luck. Add-only removes it:
a supplied pair that contradicts the standard is refused in the constructor, by tag, with a
message — the same constructor already refuses a data tag whose standard type is not `data`, so
the precedent and the table are both there, and it needs one new question of the schema, "is this
tag defined by the standard", one byte wide. A consumer who genuinely means to discard the
standard pairs can be given a named way to say so later; nobody should arrive at it by omission.

## D28. One watch list, named, for the three places that need it, 2026-09-19

Three designs arrived within a day needing the same question answered — what may not be moved:
sql-39's owning continuation, the hoist of a seam out of a rule, and expr's value handed to a
guard at the capture's close. Three copies of a list like that drift, and the drift is silent,
so it is written once as **the watch list** (`docs/design/no-way-back-2026-09-19.md` §6,
`d1c23e5d`) and the other two cite it. Five rows, one sentence of cause each: `when`, a lookahead
either way, a look-behind, an external or host-measured terminal that may match empty, and
`with state`. What unites them is one property — the node's answer depends on more than the
position it stands at.

**It is deliberately stricter than one of its three users needs.** sql-39 checked and reported the
excess rather than quietly trimming it: for two readings that end at the *same* position, a
lookahead answers the same both ways, so the two lookahead rows are caution there and not
necessity; only what depends on the reading rather than the position — `when`, `with state`, a
host that may read state — can tell such readings apart. For the hoist and for the close, which
move a reading to a different position, every row earns its place. The ruling is to keep one list:
a shared list that refuses a little too often costs a few causes, and two lists that drift cost
correctness. If a row ever has to differ, the narrower list gets a name of its own and a sentence
saying why, rather than a second copy of the table.

## D29. A guard's value at the capture's close, approved, and the arms stay where they are

expr's design (`docs/design/guard-values-at-close-2026-09-19.md`): a `when` that names a captured
value asks the tape for it while the text is still being read, and the tape answers by walking the
log from the rule's mark. The proposal hands the value over at the moment the capture closes, so
the guard reads a slot and no walk runs. Approved to build, on four conditions, with the size of
the emitted code as the first number and a right to stop there.

**The set is the one the immediate carrier is chosen by.** Not "wherever it is convenient" but
"where the reading can no longer be given up", which is the same `Commit` proof. §3.7 is the
constraint: a construction is deferred until the accepted derivation is selected, and an
abandoned alternative must not invoke an unrequested construction. A guard that reads the value
is itself a reason the construction runs anyway, so what has to be proved is only that this
reading is the accepted one.

**The conditions.** (1) The watch list is taken from sql-39 by reference, not copied (D28), and
expr needs all five rows, not the narrower reading: at a close the question is a *moment* and not
a position, and undoing what was built costs the very pass this removes. (2) A lookahead's body is
excluded by construction and not by proof: `Commit` inside one holds and tells the truth, while
the derivation under it never becomes the accepted one. (3) A repetition's turn is answered by a
named test written before the change, three turns with different values, a guard that rejects the
wrong one, and a turn read, given back and read again; the last-turn guard defect closed itself
once, which is worse than having been fixed. (4) No commit lands without its pair, and the pair
carries size-growing rows on every guarded family, not only SQL, with linearity at the end of the
window. The order of work: size first, then the count of factory calls on accepted and refused
input, and only then a window on the machine.

**And the answer to the condition that worried me.** I required the arms to become static methods
so that the reader could call one at a close, and said the price was file size, a signature and a
prologue per rule. expr's counter is better: the close enters the *same* walk by a second door,
which does what the fast path does without what that path pays to prove it — no clearing from the
watermark, no scan over the flags from the rule's mark, which is the pass being removed. The arms
stay local functions, the file grows by a parameter and a branch rather than by a signature per
rule, and the price is one switch dispatch instead of a direct call to a known arm. Size stays the
first number, now as a *check*: the expectation is near zero, and a growth proportional to the
number of rules would mean the arms multiplied after all. The condition that replaces mine: the
second door must not restate the fast path's test in a second place, and the branch must not be
paid for by a grammar that has no guard at all — the generator emits the door where a guard needs
it, which is D25 applied to this.

**A third form, which would make the project unnecessary, and the rule it illustrates.** Before
writing the second form expr looked again at what is actually expensive on the ask, and it is not
the building. The fast path already builds a record where it stands; what it pays for is proving
itself entitled to — an `IndexOf` over the `built` flags from the rule's mark to the root. That
question, "is everything from the mark built", has a constant-time answer: a watermark of
contiguity in `Ways`, raised by one as each record is built and lowered on a rewind in the same
place `ways.Built` is already lowered. The test becomes a comparison.

Why it is better than what I approved: the factories are called exactly when they are called
today, only when a guard asks. §3.7 is not touched at any point, so the watch list is not needed,
lookahead bodies need no exclusion, the repetition test has nothing to test and the proof of a
point is not required. The whole risk of the project disappears and bookkeeping is left. The edit
is also smaller than the second form: a field, one line at a build, one at a rewind, a comparison
instead of a scan.

**The rule this is recorded for.** Where two designs answer the same measurement, the one that
does not touch what the specification promises wins even at equal numbers — the other is paid for
in proofs that have to keep being true as everything around them changes. A risk removed is worth
more than a risk carefully held.

What it does not cover, and why the project is not withdrawn: two other linear costs sit on the
ask beside the scan, `values.Room` clearing `Live` from `from`, and the blanket
`Array.Clear(built, …)`. Constant time for one of three does not help if the cost is spread over
all three. expr has asked sql-39 for its anatomy split over those parts and writes no code until
the answer: if the cost is the scan, the third form wins and the second is not needed; if it is in
the clearings, the second stays and the third lands inside it as its cheap part. The condition
that comes with the watermark, whenever it lands: it is a cache of a property that is otherwise
computed honestly, so a test must hold it against the honest scan over the corpora, because a
watermark that is wrong by one answers "all built" and the building is skipped in silence.

**The watermark's check stays on our side of the fence.** expr chose the form at once: where the
generator emits the comparison, a second emission also performs the honest scan and fails on a
disagreement, so the check rides every corpus already run, rewinds included, and no separate run
can be forgotten. The condition that goes with it, held as a condition and not as an intention:
none of it may reach a consumer's build. Under a consumer's `DEBUG` their debug build would pay
the very linear cost being removed, unasked and unannounced, and our assertion would surface as a
crash in code they did not write. The switch is ours, beside the one that already turns on the
generation report, and off by default. If the second emission turns out not to be cheap, the
answer is to narrow what is checked — rewinds, where the watermark will be wrong if it is wrong —
and not to move it into a run on the side.

**D26, the external reference, taken: `QuickFIXn.Core` 1.14.1** (critic's Q9,
`docs/design/open-questions.md`, `b6b58c0a`). It holds the status ScriptDom holds for T-SQL: what
a consumer of ours would otherwise be running, doing more than we do, quoted as a reference and
never as a base for a ratio. It is the one FIX number this repository can have that does not come
from a parser we wrote, and Q3, Q5 and Q9 each turned out to need exactly that.

The licence was read from the file rather than assumed: the QuickFIX Software License 1.0,
BSD-three-clause in shape, whose two obligations — carrying the notice and acknowledging
quickfixengine.org in an end-user document — are untriggered, because nothing is redistributed.
The benchmarks project is not packed, no package of ours references it, no copy of its source
enters the repository and CI restores from NuGet. We name it anyway, in `benchmarks/README.md`
and in every results document carrying the row, as we already name ScriptDom in every table: it
costs a line and makes the question unarguable rather than merely answerable. The row is
`reference-QuickFIXn`, by the naming convention stand adopted the same day.

**The condition that decides whether the number means anything.** It is paired against
`FixMessages`, not against the field reader: `Message.FromString` splits the wire, validates
against a dictionary and assembles repeating groups, so setting it beside `FixParser.Parse` would
be the regex comparison in reverse, a reading that does much more quoted against one that does
less. A field-level row may be taken as well, and then the difference is said in the row, not only
in the prose around it. Two traps recorded so nobody falls in them twice: `QuickFix.Net.NETCore`
is an older third-party repack and is not this package, and the package names lost a full stop at
1.14, so Core is 1.14.1 while the message packages are still 1.13.0.

The work is finance-24's, as the owner of the FIX benchmarks, with critic's write-up as its input;
the account of the libraries that D26 asks for is still owed and is not replaced by the row.

**Correction to the third form, in one word, and it matters: a count, not a watermark.** Written
above as "a watermark of contiguity", which is what expr proposed and what I approved; expr found
it wrong on sitting down to write the code, and says so rather than quietly writing something
else. There is no contiguous run of built records to mark. Records below the guarded rule's mark
belong to rules above it and are lawfully unbuilt, so a watermark counted from zero would answer
"no" to every ask. The question is relative to the mark. A count answers it: `Ways` holds how many
records are written and not yet built; the mark saves that number beside what it already saves,
and an ask compares — everything from the mark is built exactly when the count is what it was at
the mark. Writing a record raises it, building one lowers it, a rewind restores what the mark
saved. The rewind is what makes the count better than a watermark rather than merely equal to it:
a watermark would have to find its new place by a scan, and the count does not. The check of the
previous paragraph is unchanged and now holds the count against the honest scan, which is what
makes the cheaper mechanism safe to have. A grammar with no building guard emits none of it, by
the same condition under which it emits no `ways.Built` today.

**The condition above was written from a claim nobody had read, and finance-24 read it.** It says
`Message.FromString` "splits the wire, validates against a dictionary and assembles repeating
groups". That describes one of two ways of calling it, and the difference decides what may be set
beside what. Read from `Message.cs` of the source itself:

- Groups are assembled only with a dictionary: the guard is `if (msgMap is not null &&
  msgMap.IsGroup(f.Tag))`, and `msgMap` comes from the application dictionary. Constructed without
  one, a message keeps the members of a group as flat repeated tags.
- There is no validation inside `FromString` at all. The `validate` flag checks one thing there,
  that the first three header fields are in order. Validation against a dictionary is a separate
  static call the caller makes.
- The .NET port has no general reading of data fields. One tag is special-cased, `XmlData`, by its
  length; there is no set of data fields in its dictionary. The C++ and Java engines do this by
  dictionary, and this one does not.

**So the pairing is two pairings, and each says what it is.** Against `FixMessages`: with both
dictionaries and `validate: true`, followed by the explicit `DataDictionary.Validate` — which
makes `QuickFIXn.FIX44` a requirement rather than a maybe. Against `FixParser.Parse`: the
dictionary-less form, with the row saying that it assembles no groups and builds no typed values.
Setting the dictionary-less form against `FixMessages` would be the regex comparison inverted in
our own favour, which is worse than the ordinary kind: it flatters us.

**And the third point chooses the inputs, before the stand does.** Any input carrying a standard
binary field other than `XmlData` — `RawData`, `SecureData`, `EncodedText`, `Signature` — is read
differently by the two sides, because ours reads a data field by its length and theirs stops at
the first separator inside the payload. The stand's check that both sides answer alike fails there,
and fails justly. Either the inputs carry no such field, or the row says plainly that there is no
agreement on it and why. This has to be settled before inputs are chosen, or a day goes into
diagnosing a "divergence" that is a documented difference.

The correction is finance-24's, made by reading the source rather than taking the condition it was
given. The error travelled through me: I wrote it into D26 from Q9's summary without opening the
file, on a question where the whole condition rests on what that file does.

**A consequence of that reading which is not about numbers at all** (critic, checked
independently before building on it): QuickFIX/n's dictionary has no data-field set, no
`IsDataField` and no mapping from a length tag to its data tag. So the capability D25's first
wording would have taken from us — a consumer declaring its own length/data pairs — is one the
most used .NET FIX engine does not have in any form, for the standard fields let alone a
counterparty's. `FixFieldOptions` is therefore not an untidiness that a stricter rule would have
swept away; it is the only way anyone gets that reading on this platform. Which is the evidence
for Igor's second wording being the right way round, and for D27's "extends": the thing being
extended is the only one of its kind, and losing the standard sixteen by omission would have been
losing them from the only place they exist.

## D30. The FIX seam: one call where there were thirty-three, 2026-09-20

finance-24's design (`docs/design/fix-custom-seam-2026-09-20.md`, `1850398c`), approved as
written, with the three open points settled below.

**What opening the factory showed.** The default arm is not one place. `FixFieldFactory` dispatches
in two levels — `Value(tag, span)` switches on `tag / 64` into fifteen `PartN` methods, each
switching on the tag inside its block of 64 — and a default exists at *both* levels, because a tag
the standard does not define inside a block falls into its own block's default and never reaches
the outer one. Counting both span forms and the binary one, thirty-three places construct the
fallback field today. The seam turns them into one call, which is less repetition than the package
has now, not more.

**The object is threaded through the two levels as a parameter**, thirty-two signatures changed
mechanically. Approved: at run time a reference travels in a register and a known tag never reads
it. The alternative finance-24 rejected — `PartN` returns null and the outer level sorts it out —
would put a test on every field including the 912 known ones to save one argument, which gives
away exactly what the change is for. Two conditions: no per-field null test on the known path
either, so where no object is supplied the package's own instance stands in and the arm has one
shape; and the package's standard instance is a sealed internal derivation of the class, since its
members are abstract.

**Three abstract members, not one**, one per form: a span of characters, a span of bytes, a binary
pair. A consumer who answered only one would have forms that disagree, which is the defect we
spent a day removing elsewhere, and abstract rather than virtual puts that question to the
compiler instead of to operations. They return a field rather than null, so the arm is one virtual
call and not a call plus a test, and a consumer who does not recognise a tag calls a `protected`
helper that builds what we would have built. `ReadOnlySpan` holds D5 by the type in two of three.

**The name is settled, and now is when it is free.** Igor chose `Custom` over `Unknown`;
finance-24's observation removes the last objection to it, that `Custom` would carry two meanings
— a tag nobody declared and a tag the consumer declared. The two never meet: if the consumer
declared the tag, they returned their own field and ours was never built. The rename is a break,
and `FixField.Unknown` is named in six places outside the package in this repository alone. 0.2.0
is not out, so it costs nothing today and costs a deprecated synonym and a major version after.

**A consumer's tag does not become known to the message layer, and that is said out loud.** The
seam lets a consumer build their own field objects; the strict mode still rejects such a tag,
because the schema is silent about its type. That is the next question, it rests on a dictionary
read when the parser is generated, and it stays out of this change.

## D31. The kept tokenization: a cure, and the cause it cures, 2026-09-20

Two proposals now stand against the same symptom, and they are not rivals: one cures it and the
other removes what causes it. Both are Igor's, because both change what a consumer sees.

**The symptom.** Over a grammar cut into tokens, the positional form tokenizes the whole input on
every call, so a loop of readings over one text costs its square. Measured and ours:
`ScriptScalingTests` has 10, 100 and 400 statements at 56 µs, 1.9 ms and 30.3 ms, an exponent of
1.99 against about 2.2 ms if it were linear.

**The cure that landed** is a kept tokenization: the whole text is cut once and kept for the next
reading that arrives with the same string. Two slots per thread, the text held weakly and the
tokens strongly, a slot taken by the next text. It works, and it is where the eviction defect
lived — a live reading's tokens went back to the pool and the next tokenization wrote over them.

**expr's proposal** gives the kept tokenization a home the host holds: `TransactSqlParser.Over(text)`,
and the tokens belong to that object. Slots, eviction, weak references and the thread field all go,
and so does the class of defect. It costs a public type on every parser, for ever.

**critic's, written as an objection to it, and it names the cause.** The whole text is cut because
of what the non-windowed positional form *means*. I read the emitter rather than take it: at
`CSharpEmitter.cs:1594` and `:1690` the emitted path cuts the input and then refuses on
`tokens.Stopped >= 0`, where `Stopped` is wherever the scan met a character that begins no token,
anywhere in the text. So a form that "need not reach the end" is refused by what it does not read:
one bad character in the last line of a script makes the first statement unreadable. The window
form is deliberately the other way, and the emitter says so in a comment beside it — inside a
window, a character no token begins with is where the tokens end. Give the non-windowed form that
same meaning and it can cut from `at` on demand, as far as the reading goes: a loop is then linear
because each call cuts what it consumes, and there is nothing to keep, nothing to evict and no new
public type. The token search goes too, since a cut beginning at `at` begins at the token asked for.

**What the two proposals share, which neither letter said.** The on-demand cutting is not new
machinery to invent: it is `BufferedKinds`, the third scheme in expr's own positional-forms design,
which that document already calls "the answer if this ever has to be exact" and parks because the
kept tokenization was cheaper. critic's contribution is not the scheme, it is the reason the scheme
was not needed: the refusal semantics. Removing the refusal makes the parked scheme the whole
answer. So the question to Igor is not "which of two mechanisms" but "do we change what the
positional form refuses, and then build the resumable tokenizer, or do we keep the meaning and pay
for it with a public type".

**Conditions, whichever way it goes.** `Over` can exist only where the machine is over kinds and
the input is a string, so it is absent from byte and memory forms — and absent with the sting
drawn, since a public shape following an internal analysis means adding a `find` to a grammar
deletes a public type from a consumer's API. Present and inert is worse: it re-introduces the
square silently for the callers who believe they have avoided it. And the gate stays where the
danger is: `ScriptScalingTests` asserts an exponent of at most 1.1 over the *static* positional
calls, and if `Over` becomes the cured path that test must not be rewritten to `Over` and must not
be deleted. It stays on the static calls, a second one is added for `Over`, and if the static calls
are then knowingly quadratic that is written into §6.3 where the forms are described, rather than
found by whoever loops first.

**What is not known, and is said rather than dressed up.** The price of `Over` in practice cannot
be given: every call of a positional form in this tree is ours, in the tests and the stand's script
rows. No package, no example and no Web, SQL or Finance test calls one. So "a naive loop goes
quadratic" is a claim about how a stranger writes code, not a measurement, and critic declined to
present it as one.

**Two corrections to D31, and the second one is mine and is about method.** First, the name:
`Tokenized_DotGram` is right and my "correction" of it was wrong. Both exist and they are
different things. `Tokenized_DotGram(string input)` is the kept cutting — the slots, the weak
reference and the eviction are inside it — and `Tokenize_DotGram(input)` and `(input, from, to)`
are the tokenizer it calls when the slots miss. The emitted call is a ternary on `kept`, and the
branch this whole question is about is the one with the `d`. A reader of D31 grepping the shorter
name would have found the tokenizer, seen no cache in it and concluded the objection described
something not in the tree.

**How I got it wrong: I verified the claim against a checkout a day old.** `P:\dotgram` stood at
`b1daccd9` while the work had moved on, so the lines I quoted were real lines of a file that is no
longer the file. The substance survives — re-read in the current tree, the non-windowed positional
form still refuses on `tokens.Stopped >= 0` while the window form does not, and the token search
after it is still there — but that is luck, not method. The rule this session has been handing to
others all day has a clause I did not apply to myself: reading the source is not enough if the
source is not the current one. A verification says which revision it was taken at.

**Second, critic's correction of its own claim, which changes what Igor is choosing between.**
Q10 presented cutting from `at` on demand as a path nobody had seen. It is `BufferedKinds`, in
expr's positional-forms document at §2 and §4, and its `false` already means "the true end of the
input, or the first character no token begins with", which is exactly the semantics Q10 asks for,
written before Q10 was; the document says outright that it makes the loop linear. So Q10's "the
tokenizer has to become resumable, which is more work than `Over` costs" overstates it in the same
direction: the work is to build a designed thing, not to invent one. What critic added is not the
scheme but the reason it looked unnecessary. The choice in front of Igor is therefore not "a new
public type versus inventing on-demand lexing" but "a new public type versus finishing something
already designed, whose deferral rested on a semantic nobody had questioned".

**The convention that comes out of it, for every document in `docs/design/`.** A citation is the
name — a method, a rule, a section — and the line number is a convenience that is true at the
commit named beside it. A name survives other people's commits and a line does not, and with five
sessions rebasing through one repository a line moves within hours. critic has put this at the top
of `docs/design/open-questions.md` and re-read Q10's three citations at `91544ed4` so that they
carry the commit they hold at. The part worth repeating is not "check it twice" but which checkout
is likeliest to be wrong: a session working in a branch fetches constantly and is usually current,
while the tree that feels canonical is the one nobody rebases. The main worktree is the stale one.

**D29's decomposition, measured (sql-39), and it moves the answer.** One guard's ask on `select20`,
301 asks over 402 records, 17,862 ns of wall: the walk's own body 4,892 ns, 16.3 per ask, with
`Room`, the listing loop and the outer loop inlined into it; the clears 937, 416 and 78;
`IndexOf` 495; the arms 2,394, that is 6.0 ns a record; plus 649 for marking what is reachable and
911 for dispatch.

So the scan is 6% of the walk's cost and the listing loop is most of it, and the count answers only
the scan. The third form alone is therefore not enough, which is what the decomposition was asked
for. But expr read the code against the numbers and found the sharper question: today's fast path,
when it fires, does not list at all — it leaves early. So the 8.4 µs is paid by the asks that did
*not* take the fast path, and what decides the design is not the price of the parts but the share:
how many of the 301 take it, and which of the three reasons keeps the rest out. sql-39 has taken
that count, it needs no machine, and it is now what D29 waits on.

**And a measurement that corrects two models, including one of mine by implication.** The store's
blanket clear in `DirectValues.Return` costs 1,148 ns a parse, 6.3% of the reading, taken by a
direct pair of builds. expr's two models of the same thing said 126 and 211, and sql-39's estimates
erred the same way. A clear is the kind of cost a model underestimates by five to nine times,
because what it costs is not instructions. The rule both have taken, and it belongs here: an
anatomy carries no modelled number without a measured one beside it.

**D31's third path, priced (expr, `docs/design/lazy-kinds-cost-2026-09-20.md`, `dcef8892`), and the
price decides it.** Cutting on demand is not a helper added to the reader: over a split grammar the
kinds are a `char[]` and the reader reads them as a span, which is why there is one rendering and
it costs nothing. Making the end of the input a *question* rather than a length is a property of
the machine, chosen once per machine, so a grammar with positional forms over lazy kinds is
rendered twice. Two ways round it were tried and both fail: a span refreshed after lexing more
does not reach the recursion's copies, and an array sized to the input and filled lazily cannot let
the reader tell "not cut yet" from "did not match", which is a wrong answer rather than a slow one.

Measured in that tree: the reader and the walk are 33% and 12% of T-SQL's emitted code (14.17 MB),
31% and 28% of SQL:2023's (8.56), 26% and 33% of the expression language's (1.87). A second
rendering adds about as much again: **+44% and some 6.3 MB for T-SQL**, +59% for the other two.
The ratio is not an estimate — `DotGram.Examples.Feeds.StockCountReader` already carries both
renderings of one grammar. And the generator is the slowest thing that builds the solution, so a
second rendering of the reader and the walk is close to a second pass of what takes the time in it,
paid by every consumer on every build, for a symptom only a host reading from a position feels.

**So the third path is refused as the general answer**, and the choice is between keeping the
hidden cache and taking `Over`. My recommendation to Igor is to keep the cache. Its defect is
found, fixed and covered by a test; a loop is linear today; and `Over`'s cost is not the type but
*where the type is not*: it can exist only over kinds and only over a string, so adding a `find` to
a grammar would delete a public type from a consumer's API. A public shape that follows an internal
analysis is a worse hazard than a hidden mechanism with a test on it. `Over` is revisited when a
consumer needs a reading it controls, rather than to cure a square that only our own tests have
ever met.

**What lands regardless: the refusal.** A form that need not reach the end is refused by what it
does not read, which is a defect in any outcome. The positional form ends its tokens at `Stopped`,
as the window form already does. expr takes it next, `ScriptScalingTests` stays on the static
positional calls and is not rewritten. The single short reading out of a huge text stays as it is,
known and written down: it is the one thing only lazy cutting would fix, and it does not buy 6.3 MB.

## D32. The owning continuation: a threshold named before the answer, 2026-09-20

sql-39's prototype on T-SQL removes 23 of 83 own causes and 18 of 321 replaceable rules, and it
arrives with three qualifications, two of which are the report's value. The cause the analysis was
built for, `JoinedRight`, survives; the recursion is handled by returning an empty set on a cycle
and caching what was computed with that stub, which is an *under*estimate and therefore an error
in the direction of "there is no way back", which is the direction of incorrectness; and the
generator takes 201 s against about 60 on `DotGram.Sql`, three times over.

**The order stands as proposed, with one interleave.** Print the set beside the cause first and
find out why `then ')'` and `JoinedRight` survive, since that decides whether the analysis is worth
anything at all; then the real fixpoint; then the generation time. The interleave: the fast-path
share that D29 waits on is taken after that first step, not after all three, because it costs no
machine and another session's decision hangs on it, while steps two and three are days.

**The threshold, named now rather than after the numbers are known.** Generation time is a gate and
not a trade: D11 B put T-SQL from 4 s to 86 s and had to be taken out, so an analysis that triples
`DotGram.Sql` does not land whatever it buys, and the version that goes to a pair costs single
percents. And the benefit is not counted in causes. A count of removed causes is a likeness of the
thing wanted, the same error this repository has spent two days naming: what has to move is a
carrier, a measured time or a measured size. Twenty-three causes that change no carrier are worth
nothing, and one cause that moves T-SQL onto the immediate carrier is worth the work on its own.
sql-39 offered to come and propose closing it rather than push on; that offer is accepted in
advance, and closing it after the fixpoint is a result, not a failure.

**D30 landed (`9ce9058f`), and it found a second place while being written.** The parser returns
only the data half of a pair; the length half is built by the message layer, in its own default
arm. Left alone, a consumer's own pair would have come back half theirs and half ours, which is
exactly the divergence of forms the seam has three members to prevent. finance-24 threaded the seam
there too and judged it the finishing of its own change rather than a widening of scope, since the
divergence would have been created by that change. The judgement is right, and the boundary D30
draws is unaffected: building the length field is not the schema knowing the tag, and the strict
mode still rejects a consumer's tag inside a message.

The rename to `FixField.Custom` went through the package, the oracle grammar, the tests, the README
and the skill, and it turned up a README that had been wrong since the morning — it still said a
supplied dictionary replaces the standard pairs, which D27 reversed. Both documents now carry the
sentence saying what the seam does not do. The rename is a break and belongs in 0.2.0's notes as
one, which is what makes doing it before the release the cheap moment rather than merely the tidy
one.

## D33. The outward sweep: three items, triaged, 2026-09-20

critic's Q11, the hunt for what the outside does and we do not. Three items rather than a list, each
with the place here it touches and the number that would decide it. What the sweep found we already
have is worth as much: precedence climbing for a tower of binary operators is `graph.Climbing`, and
the progress assertions the outside advocates as a run-time fuel counter we prove at generation and
report as a diagnostic instead, which is D25's side of the same question.

**Third item first: a tree built where it is asked for.** It aims at our largest number, the guards'
walk at 8.4 of `select20`'s 17.9 µs, and the deciding fact is not known here — what fraction of the
nodes a real consumer touches, all of them for `--roundtrip`, a few per statement for a host asking
a script what kinds of statements it holds. That count is ordered from stand and needs no machine.
**But the item carries an objection the sweep did not price: D5.** Deferring a construction past the
end of the parse means keeping what it will be built from — the log, and for a value cut from the
text, the text. Nothing may retain the input, which is the bound this repository has spent its
memory work on, and §3.7 defers a construction only to the accepted derivation, not past the parse.
So the shape that survives is a *bounded* one: built where it is asked for, within the parse, which
is what `carrier-per-construction` and D29 are already circling. A tree handed back unbuilt is a
different promise and would have to be priced against D5 before it is designed.

**First item, computed recovery sets, is parked with its number.** Today `recover` is the author's:
it marks a repetition, names its own synchronization expression, exists nowhere else, and a failed
parse returns no structure at all. Computing recovery sets from the grammar — follow sets of the
dominators, as `lelwel` does — uses two halves we already have in `FirstSets` and `FollowSets` and
one we do not. It changes what a grammar says, so it is Igor's, and it belongs with the tooling
project rather than ahead of D13. The number to take when it is opened: of a file whose first error
is at line N, how much structure comes back.

**Second, incremental re-parsing, is parked outright, with the same instruction.** It is the editor
only; a generated parser is not asked to be incremental. Nothing here reuses a tree, and the
extension answers the cost by backgrounding. The number comes before any design: what one keystroke
in the middle of a 7,264-line grammar costs today. Backgrounding may already have made it a
non-question, and that is cheaper to find out than to design against.

**D33's third item, corrected by its author, and the correction is the useful part.** Two things
were one: a lazy *value* and a lazy *node*. A lazy value needs the text, because an extent on the
tape is a start and a length and reading it slices the caller's text; a capture a factory already
turned into a value needs nothing. A lazy node — Roslyn's red over green — needs no text at all,
since the green side is built, and what it saves is the allocation of wrappers nobody touches. But
our number is not allocation: the factories are about a tenth of materialization and the walk's own
machinery is the rest, so deferring a wrapper saves little, because the walk still runs to know
what is there. **What would cut the 8.4 µs is a lazy walk** — not walking a subtree until someone
asks for it — and that needs an index from a record to the extent of its subtree in the log, which
is ours to invent and which neither of the outside answers hands over.

**And my D5 objection is narrower than I put it.** It binds the streamed forms, where no path may
reach a contiguous form and the buffered support copies precisely because the window moves: there
an unbuilt tree holds buffers D5 forbids growing, and it is out rather than to be weighed. Over a
`string` the text is the caller's own object, so holding it is a lifetime and not growth — the
hazard Artio names for its flyweight codecs, valid only while the buffer is unchanged. That is a
promise to write into the API, not a rule being broken, and D5 should not be quoted against the
string forms later on the strength of this item. The bounded form is therefore not a smaller
version of the item: it is the only version that touches our number, and the count ordered from
stand decides both.

**The summary that nearly misled, corrected before anyone acted on it (critic, checked here at
`87fbb326`).** Bundling the three as "knowing about a stretch of the log without listing it"
invites the reading critic nearly published itself: every record does carry a length — `End` writes
`Log[Opened] = LogCount - Opened` into the record's first slot (`Emit/Support.cs`), and the walk
advances by exactly that (`for (var at = from; at < ways.LogCount; at += log[at])`,
`Emit/Machine.Direct.Values.cs`) — so a reader of those two lines concludes the index is already
there and stops. It is not the index wanted. `Opened` is one field and not a stack: a record cannot
be open while another is written, so a construction is emitted as one straight-line block at the
point it completes, a built member is referred to by record number rather than contained, and the
log is flat and post-order. A record's length is its own, the walk steps over siblings, and a
subtree's extent is nowhere written.

So of the three, only the lazy walk needs something recorded that is not, and what it needs is a
**bound** and not a length: in a post-order log, "from the least record number below this one to
this one". expr's count from a mark is a live stretch, the mark being on the stack while the parse
is inside it; sql-39's fast path is a predicate over a stretch, whether a record's children are
built, and not a way to find its bounds. The three are neighbours in what they want to avoid, not
in what they need.

**One pointer left for whoever takes the lazy walk.** Unwinding is where it is likeliest to hurt,
and the same question is already settled next door for the held table in
`carrier-per-construction` §2: the held count is marked and unwound with the records, because that
is what the mark already covers. A bound may inherit that answer or may not, and the reason it
might not is worth having before the work starts — a bound is written at `End`, *after* the stretch
it describes, where the held count is marked before it.

## D34. The value store's clearing: the number first, then a shape that costs nothing, 2026-09-20

expr's anatomy (`docs/design/value-store-clearing-2026-09-20.md`, `7e8f8795`): clearing the value
store on `Return` costs 1,148 ns of an 18,106 ns reading, 6.3%. On `select20` that is some four
hundred values across twenty-two tables, five kilobytes in twenty-odd calls — fifty nanoseconds a
call and 2.5 a slot, an order more than writing five kilobytes costs. So the price is the calls and
the cold tables, not the bytes, and a cheaper way of clearing the same stretches saves nothing.

**The small levers are small, and one of them is refused for a reason worth keeping.** Skipping
tables whose type holds no references is sound in itself, but the emitter knows type *names* and
not what they are made of, and it has to stay free of Roslyn; the framework's own question about it
sits above our floor and would need a second writing beneath it. And it covers few tables, a tree
being mostly references.

**The real lever is to stop clearing what the next parse overwrites**, which is true: `Add` hands
out indices from zero up and the caller writes a slot before reading it, so every slot handed out
is overwritten before it is read. Only the tail needs clearing, where this parse took fewer slots
than the last, and with the previous high-water mark remembered that is a comparison which is
usually false. In a loop of like parses the clearing disappears.

**Its price is retention, and that is the question, not the code.** Between parses the store holds
the last parse's values, so a thread that parsed a document and went quiet holds a whole tree
through its pool.

**The order: the number before the decision.** The 1,148 ns covers the tables *and* the `Built`
flags, because the build measured against had neither, and the flags stay whatever is decided. One
pair, six minutes, gives the tables alone — and a retention question must not be answered on a
number that includes something we keep anyway.

**And the shape I would approve, if the number holds up, costs nothing on the hot path.** Not
"clear the tail on every `Return`" but "clear nothing on `Return`, and clear when the thread goes
quiet". The pools already have that moment: the retention rule decided for them holds a buffer
strongly while it is used, releases it after N consecutive unused parses and then keeps only a weak
reference. Clearing the tables at that same transition makes the hot path free, bounds the
retention by the thread's own work rather than by the collector's mood, and gives the mechanism its
second user rather than inventing one. What has to be designed is only the join: the store is
cleared *before* it is handed to the idle state, so a quiet thread holds no tree at all.

**D29 is closed by its own numbers, and expr closed it.** On T-SQL's corpus, 17,023 asks: the fast
path fires on 7.8% of them, and 78% miss it for one reason — there is an unbuilt record below. A
count answers that question in constant time and answers "no", after which the walk happens anyway:
the listing, the clearing and the marking all remain, and only the scan is saved, which is 6% of
the walk. So the third form buys nothing on T-SQL. Nor does the second, which expr saw only after
reading the reasons: an unbuilt record below is a record *nobody asked for*, and §3.7 lets a close
build only what a guard named, so it stays unbuilt, the fast path still misses, and nothing moves.
The cause is cured only by building everything whose reading is proved, which is the wider
`carrier-per-construction` and not this. SQL:2023 has a different shape — 56% pass, and what stops
the rest is mostly "the root is not the last record", 36% — but neither the count nor the close
removes that either.

The design is rewritten as a refusal carrying these numbers and kept: a record of why something was
not done is worth more than a deleted file, and this one was refused by the criterion its own §3
named in advance. The next number on this line is sql-39's: how many records the slow walk lists
per ask. If it lists tens for one, that is where to aim, and it is a different project.

**D34's shape is a requirement on work not yet written.** The moment "the thread went quiet" does
not exist in the code: the spares in `Emit/Support.cs` are still a strong slot and deeper spares,
with no counter of unused parses and no weak reference. The retention rule was decided and not
built. So the join has nothing to join to yet, and D34's shape becomes a condition on whoever
writes the retention — performance-ff: **at the transition into idle the value store is cleared
whole**, before the buffers are handed to the idle state, so that a quiet thread holds no tree. The
fork does not reopen in the meantime, because nothing is proposed before that mechanism exists.

## D35. 0.2.0's readiness, checked by someone who did not build it, 2026-09-20

critic's Q12. The guess that the risk lives where the public surface moved this week was right, and
what it found is a class rather than a case.

**The defect: a decision that reached the code and not the pages the package ships.** D27 — a
supplied dictionary adds to the standard's sixteen rather than replacing them — reached the code,
the constructor's own XML comment and the release notes, and did not reach `README.md` and
`SKILL.md`. Both open their example with `[95] = 96`, which is one of the standard sixteen, and the
constructor now throws when either tag of a supplied pair is one the standard defines. So both
shipped pages open with a call that throws on the package they ship with, and the release notes
predict that exact reader error. The skill carries the overturned sentence beside it, in the file an
agent follows. The README says "replacement" in one place and "adds to" forty lines later. Fixed by
dropping the standard pair from both examples; with finance-24.

**This is why the rule written this morning is a rule.** A decision that changes what a README or a
SKILL promises edits them in the same commit that changes the behaviour. Three places, one decision,
one day — and the two that were missed are the two a consumer reads first.

**What is in order, said because a check that lists only defects is not one.** All five packages
carry release notes as a property rather than a changelog file — critic first concluded there were
none and found them by looking for the property, which is the second road it now owes every count.
Both breaks are there with what to do, and there is no unrecorded third: Finance's withdrawal of the
typed entry classes and the expression language's narrowing to `Parse`, `TryParse` and `Compile`
are both in their own notes. Licence, repository, authors, copyright and icon come from
`Directory.Build.props` for every packable project. No page promises XML documentation that is not
generated, and Sql says it carries none. Nothing of anyone else's is packed: the grammars are
additional files, the `Specification` directories reach no package, and the only package references
are `System.Memory` and, privately, a polyfill and Roslyn.

**One line outside the check, and it goes to Igor.** The ISO BNF and Microsoft's published syntax
sit in the repository, which is public (`github.com/dotgram/dotgram`). Both carry their provenance:
Microsoft's is CC-BY 4.0 with the attribution and the changes named, which is exactly what that
licence asks. The ISO file is kept as fetched, with ISO's own sentence quoted — "this grammar may be
used by implementors of SQL-implementations when generating parsers" — which speaks to *use* and
not plainly to redistribution, and a public repository redistributes. That is Igor's to weigh, not
the architect's and not critic's.

**There are four pools, not three, and the fourth was invisible to the report that found the other
three** (performance-ff). `ImmediateValues.Return` carries the same constant and the same cliff as
the parser's `Recycle`, `Ways.Return` and the emitted `DirectValues.Return`. finance-24's ladders
could not have shown it: their shapes are carried on the tape, and that store belongs to the other
carrier — so a grammar carried immediately, with a large document, would have walked into the same
twentyfold jump with nothing in the report connecting it to the three. That is the same blind spot
as the carriers report's, one rung down: a measurement taken through one carrier cannot see what
the other carrier's machinery does. The rule now lives in all four, and two of them share it through
one emitter helper rather than by being edited alike.

**D34's seam is built into the retention commit rather than sewn on after it**, which is right: the
moment it needs is the moment that change creates. The route is a store's own `Empty()`, called by
`Return` and called again by the demotion before the store passes to the weak reference; emptying an
already-empty store costs nothing, every cleared span being bounded by a count that `Return` zeroes.

**And a stop worth recording as a decision rather than a delay.** performance-ff stopped short of
that surgery for the evening after three attempts went into fighting shell quoting rather than into
the change — the point, as it put it, at which it is likelier to break something quietly than to
finish. Nothing is half-applied: the retention change builds, its test passes, and what is missing
is described rather than started. A session that stops there and says so is doing the thing this
journal exists to encourage.

**One measurement note that cost a wrong number today.** The +23% on the located T-SQL was the
stand's window and a generation gate sharing the boards; on a quiet machine it is 1.07x. A gate is a
timing run and takes the same discipline as any other: pinned, and not inside somebody's window.

**The general form, which performance-ff drew and which is worth more than either instance.** A
summary that is true about a grammar and false about its parts, and a ladder that is true about one
carrier and silent about the other, are the same error in different clothes. The carriers report
named a grammar's worst machine and was read as naming its machines, which is why the plain FIX
rows were predicted to move most when they had never been on the tape at all; the pool ladders were
taken through the tape and could not see the immediate carrier's store. Both are one grep from
being avoided. Beside it, in the same session's own words: three named predictions on that commit,
one right, and the number belongs in the record rather than a tidy account of a good result.

## D36. One walk for a guard that names several values, 2026-09-20

expr's replacement for the closed D29 (`docs/design/one-walk-per-guard-2026-09-20.md`,
`5f39c600`), and its virtue is that it asks nothing of §3.7. A guard naming several captured values
materializes each by its own walk — three calls in a row from one point in generated SQL:2023 — and
every one pays the whole prologue, and when the fast path misses, the listing over the records from
the mark and the marking pass back. sql-39's numbers put the slow walk at 12 to 15 records listed,
with the clearing and the marking over the same elements, so three walks from one guard list three
times. Taken statically over this tree's generated code, the sites standing in a run with their
neighbours are 40% for SQL:2023, 46% for T-SQL with the longest run at seven, and 51% for the
expression language. Merging a run of n removes n−1 walks.

It is cheap because the walk already takes several roots: with a side stack it marks every built
record whose slot is in the mask and then lists, marks and builds once, which is how a guard handed
a sequence builds it in one pass. The one case not covered is ours, for a small reason — a single
capture's record is a local of the reader rather than an entry of the side stack, so the mask
cannot name it.

**Approved to design further, on four conditions.** Which factories run and when does not change,
and that claim has to be *held* rather than asserted: the merged walk builds the union, so say in
the document what the order becomes, and whether a value built in one of the old walks could be
observed by the next — if it could, merging is limited to a run with nothing between the calls that
can see the intermediate state, and the document says so rather than leaving it to the reader.
Second, the dynamic count from sql-39 comes first, since the static figure counts places and not
frequency, and a run that is common in the source and rare in a corpus buys nothing. Third, the
pair carries emitted size beside time: a walk that takes several roots where it took one is a wider
signature on a hot path. Fourth, the expectation is written before the pair, as it is: T-SQL moves,
SQL:2023 less, the expression language least, and if T-SQL does not move the design is refused by
its own criterion, as its predecessor was.

## D37. An example in a shipped page exists as a test, 2026-09-20

**The process question first, answered by critic: there is no drift, because there is nothing in
between.** The release notes are a literal in each project file, the README and the skill are
copied into the package by a pack item, and no test, no smoke and no pack step reads any of the
three. Nothing can drift where nothing is checked at either end. The pages that were wrong this
morning were wrong at the keystroke, would have been packed wrong, and there was no moment at which
anyone could have caught them — which is why the defect survived a day of work on that very
package.

**The rule.** An example in a page a package ships exists as a test that compiles and runs it. The
evidence is now a number rather than an instinct: one example that throws survived in two shipped
files until the first check made before a release, in a package whose owner had edited both files
that week, and the other packages carry 39 more fenced calls — Web 29, Sql 7, the expression
language 3 — that nothing compiles. critic tried to audit those by reading, twice, and both passes
returned uniform false negatives, one declaring a method missing that sits four lines into its file
and is called by the stand. A compiler would have answered correctly both times and at the
keystroke. finance-24 has written the first such test, so the shape is known and the rule costs one
per package.

**And what the rule does not cover, said before someone believes it does.** A public-API baseline
would have caught the rename at the keystroke and would *not* have caught this morning's defect,
because D27 changed a meaning without changing a signature. The two halves are closed by different
things, and this repository has neither.

**A gap in the publishing path, one step later.** The build workflow packs and then runs the
package smoke; the publish workflow packs and pushes to nuget.org with no smoke at all. What ships
is not what was smoked — it is packed again by the workflow that publishes it. That is closed
either by a branch rule requiring the build green on the commit a tag is placed on, or by copying
the smoke between pack and push. The second is the answer to prefer: a branch rule proves that some
packing passed, not that this packing did.

**D37 is done, and it came back better than it was given.** I said copy the smoke between pack and
push; finance-24 extracted it instead — fifty lines of package-shape checking became one script
both workflows call. The reason is the day itself: it had already fixed a README that disagreed
with a decision and a comment that disagreed with the code beside it, and a checker answering one
question in two places is that same defect with a longer fuse, at the most expensive point there
is, where the gate and the release disagree. Short steps are copied as they are, their drift being
visible; fifty lines of shell are not. Approved as done.

**The checker was proved by a negative control**, which is what makes it a checker: a README was
cut out of one package and the script failed with that package named and a non-zero code. A checker
that has never failed is not checked. And critic's note about the runtime was material — the smoke
runs on the older framework too, while the publishing workflow installs one SDK, so without an
explicit install those legs either fail on a clean runner or pass on one where that framework
happened to be present, which is green that means nothing.

**And the rule grew a clause in the doing.** The Web pages became sixteen tests over 29 calls, and
they hold the *values* the comments claim, not merely that the calls compile. A comment saying
`0.7` beside a call that returns `0.3` lies to a reader exactly as a call that throws does. All
sixteen passed first time, which answers the population question critic left open: the sample was
clean and the population is now checked and clean too.

## D38. What share of what a parse builds a consumer touches, counted, 2026-09-20

stand's count (`benchmarks/results/node-touch-2026-09-20/`, `17abefe1`), taken by rewriting a built
`DotGram.Sql.dll` so that every node property getter reports what it hands back and every node
constructor counts a construction. Three numbers a parse: built, reachable from the returned tree,
and fetched by the consumer, with the counter on only around the consumer. A getter handing back an
array counts every element, so fetched is an upper bound. The control is the writer, which fetches
98.4% of what is reachable and never more than a statement holds.

**On ScriptDom's corpus, 1,086 files and 7,694 statements.** A host that asks a script for the kind
of each statement and does not go inside touches one node a statement: 15.95% of the reachable
nodes, inflated by a corpus of small statements — for the 275 statements of more than 20 nodes it
is 3.00%, and on the stand's own statements 1.1% for SQL:2023's `select20`, 0.1% for 100 conditions,
0.05% for 1,000 columns. So a lazy tree leaves 97 to 99% of a real statement unbuilt for a consumer
that does not go inside, and nothing at all for one that goes everywhere. The middle — a consumer
reading some part of every statement — is not measured and is only bounded by these two.

**And one number that is not about laziness at all: 61,171 built against 48,248 reachable.** A
fifth of what the T-SQL parser builds is thrown away before the tree is handed back, 12,923 nodes
over the corpus; SQL:2023's hundred conditions build 908 and reach 708, the same fifth. That is
worth its own question. §3.7 promises that an alternative abandoned by backtracking does not invoke
an unrequested construction, so either these are requested — a guard naming a value on a path later
given up — or they are built and then replaced as the tree is shaped. Which of the two it is
decides whether it is a cost to remove or the price of asking.

**The scale of the walk, for the record:** `select20` in SQL:2023 reaches 91 nodes and builds 93,
so the guards' 8.4 µs is about 90 ns for each node built.

**D36's benefit, now counted dynamically — and a misreading of mine, corrected.** sql-39's counter
sits where a guard's code is emitted and rises on each *execution* of it, so an ask is compared
with the last execution: the first ask after one counts as a new guard and every ask before the
next execution is "the same". The number is therefore the share of asks that are the second and
later *value of one execution of one guard* — exactly what D36 merges. 108 of 301 on `select20`,
7,736 of 17,023 on the T-SQL corpus: a third and nearly a half of all asks are walks that merging
removes. That is the strongest evidence D36 has, and it is dynamic rather than static.

I read the column's name, "asks of the same guard", as repeated asks by one guard at different
times and wrote to expr that a second lever might be hiding there — memoizing a guard's answer. It
was not there; the column was D36's own number under an ambiguous name. Recorded because the
misreading travelled: I told another session to look for something on the strength of a column
heading, which is the same error as counting a likeness, one level up. A count is read by what it
counts, not by what its column is called.

A repeat of the same guard at a *different* execution was not counted at all — such asks fall under
"a new guard" — so whether that second lever exists is still unknown rather than answered. It is a
cheap count, by the pair of rule and mark rather than by an execution counter, and it sits behind
the fixpoint and the discard bound.

**The middle consumer, written and counted (stand, `118b33f9`, `7881b084`), closes the lazy tree.**
A dependency scanner over the 2,250 statements of the corpus it has a case for, held to an oracle
that reads everything. Shallow — only where a table reference can stand by the tree's shape —
fetches 32.5% of those statements' nodes and misses 92 of 1,654 names in 78 statements: tables
inside a subquery standing in a condition, in `OFFSET`/`FETCH`, in `USING (SELECT …)`. Deep, which
misses nothing, fetches 97.6%. Getting deep right took five rounds, each a place a subquery turns
out to stand that the tree's shape does not announce.

So the middle does not exist: a consumer reading part of every statement is either wrong or reads
nearly everything. **A lazy tree gains for the kinds-only consumer, nothing for a correct scanner,
and for the cheap scanner it gains by being wrong.** And the kinds-only consumer does not need a
lazy tree either — a host that wants the kind of each statement wants a publication that reads
that, which `find` and `yield` already shape. The item is refused as a general lever, and what
survives of it is not laziness but an index: a partial walk exists only if the parser records where
the subqueries and table references of a statement are, which moves the work into the parser and is
a feature to be asked for rather than an optimization.

**And a finding that is not about performance at all.** Nothing in the tree says where a subquery
may stand, so every consumer who writes a dependency scanner writes the shallow one first and is
wrong in 3.5% of the statements it handles — silently, and never by naming a table that is not
there, which is the hardest kind to notice. That belongs in `docs/ast.md` as a sentence: a partial
walk of a statement is unsound, and the walker is what covers it.

## D39. Character classes below 128: two constants instead of a table, 2026-09-20

critic's Q13, corrected by reading the emitter before handing it over: bit tables are not missing —
`Machine.cs` emits a byte-per-character table for a class lying entirely below 256 and a windowed
bit table for one reaching past it — and deduplication is already done in both paths, keyed on the
emitted text, which is why none of Url's nine tables matched. What is left is one question, and it
is sharper than a threshold.

**The representation is chosen by reach and never by cost.** A class entirely below 256 always gets
the eight-times-larger form, and the bit table is reached only when a class is too wide for a byte
table, never when it would merely be smaller. On the checked-in snapshot `Url.gram.g.cs` carries
nine such tables at 256 bytes each, 2,304 bytes for one small grammar, and every one of them is
zero above index 127. The same nine as bit tables are 288 bytes; as a pair of `ulong` constants
each, with no array and no bounds check, 144.

**And that last form is likely not a trade at all.** A class entirely below 128 is 128 bits, which
is two constants: the test is one compare for the range, a shift and a mask, with no memory access
and no array bounds check. Against a byte table's load that may be faster as well as sixteen times
smaller, so the question is not "where should the threshold sit between two existing paths" but
"is there a third form that beats both for the commonest case". That is what to measure, and it
does not make the existing threshold anyone's mistake: the byte table is presumably a deliberate
fast path, nothing in the tree says so, and nothing has measured it.

**Order.** The size number first, over the packages' generated code rather than the snapshots —
four of the five snapshots emit none of these tables, so Url is the shape and the Web set is where
the real total is; the harness for it exists in `DotGram.CodeSize` and has never been pointed at
this. Then the speed half, on the stand's Url and Web rows, as a pair. To performance-ff, behind
the retention work.

**D32 is closed by the threshold named before its numbers.** sql-39 finished the analysis honestly
— the recursion is a real least fixpoint over a work list rather than a stub, so the underestimate
that erred toward incorrectness is gone — and made it cheap: 62 s of generation against about 60,
where the first version was 201. It removes 70 of 222 causes over the five libraries and 45 of
1,167 replaceable rules, 25 causes on T-SQL. And it moves nothing: 41 grammars on the tape before
and after, with one grammar's gate changing from replay to read-again and still held; the commit
points on T-SQL identical at 2,362 of 3,146; and the emitted code *larger*, SQL:2023 by 6.3%,
SQL-92 by 4%, T-SQL by 0.6%. Fewer causes, more code, no carrier moved. Refused, without
bargaining, by the rule D32 set in advance: a count of causes is a likeness, and what has to move
is a carrier, a measured time or a measured size.

What would make it worth having is written beside the refusal: something downstream that turns a
removed cause into a point or a carrier — a fourth edge of the carrier-per-construction, or a gate
reading what stands per machine rather than per grammar. No such consumer exists today, so the
number has nothing to be spent on. And `JoinedRight`, the cause the work began for, survives the
honest version too: what the paths inside the rule read again meets the continuation on one token
kind. Where to start if it is ever reopened is therefore written down rather than rediscovered.

D39's expectation, named before the pair in performance-ff's own words: smaller for certain, and
quite possibly not slower — a direction promised no further than it has been earned. And the
correctness line belongs in the code beside the emitted test rather than in a commit message: a
character above 127 takes its "no" from the range comparison and never from the mask. It is one
comparison replacing a bounds check, so it costs nothing, and it is the kind of thing that is
obvious while writing and invisible afterwards.

## D40. Per-framework emission: capabilities, and two things that must exist first, 2026-09-20

critic's Q14, at Igor's asking. Three buckets, not four: the floor — netstandard2.0, net472, C# 8 —
byte for byte unchanged as D20 requires; net8.0, where nearly all the value is (`SearchValues` for
char and byte, which retires our own rule that a stop set of more than five characters is read one
character at a time; `IndexOfAnyExcept`, the mechanism under the seam work, landing on 58 places;
frozen tables for a grammar over tokens, where SQL has 410 words; the `Ascii` helpers); and a later
bucket for recognising a keyword from a span without making a string. netstandard2.1 is not a
bucket, net472 not reaching it. The version in which `SearchValues<string>` arrived decides which
bucket the keyword work lands in and the sources disagree, so it is read off the API reference
before anything is built rather than guessed.

**Capability switches, not framework tests** — `#if DOTGRAM_HAS_SEARCHVALUES`, with one place
mapping a framework to its capabilities. A framework test scattered over five emission sites
multiplies by the buckets while a capability name stays one word, a consumer's framework set is not
ours to predict, and D10 already has this shape, unsafe code and skipped initialisation being the
consumer's option rather than our default. Approved.

**And two conditions before the first branch lands, because neither exists.** First, nothing
asserts that the branches *answer alike*: `DotGram.Compatibility` builds three frameworks and
building is its whole assertion, so it proves compilation and says nothing about what is read. The
material is already here — the refusal record and the corpora — run per bucket rather than per
commit. Second, the stand cannot pair two branches: D20 asks for a pair comparing them on one
platform, and the stand pairs two builds of two commits, so the measurement D20 itself requires
cannot be taken. A branch that is faster and reads differently is worse than no branch, and a
branch whose speed nobody can measure is a guess, so both come before the first `#if`.

**A detail for whoever writes D39**, checked by critic after the reframing: the comparison the third
form needs is already emitted. `TableTest` writes `c <= 255 && table[c] != 0`, so an all-ASCII class
becomes `c <= 127 && …` — the same single comparison, and a tighter one, with a shift and a mask
where an indexed load stood, whose bounds check the JIT must otherwise eliminate from the comparison
and the array's length. One comparison as today, minus a memory access.

**D40's second condition, answered: hours, and mostly not the stand's.** `--stand-paired` already
loads two folders of assemblies into two isolated contexts of one process, on one runtime and one
profile, reads every row round-robin and holds the answers equal before timing anything. It does
not care what makes the folders differ — two commits is only how they have been filled. So a pair
of two branches is any two folders built from one commit whose emitted code differs by the branch.

Two ways to fill them, and the choice matters. **By target framework** — the netstandard build
against the net10 build of one commit, run on one runtime — costs nothing and is *not* all else
equal: those two builds already differ by polyfills and by the conditionals they carry, so the
difference is the branch plus whatever else. A first look, never the pair a branch must carry.
**By a property of the generator**, a named switch the emitter reads and a build can set from its
command line, gives the same grammar built twice, same framework, same commit, differing by the
branch alone. That is the requirement: about an hour on the stand's side for a parameter on the
side builder, and the switch itself on the generator's side.

**And the switch has to be decided rather than acquired.** A property a build can set is a knob a
consumer can set too, so the design says which it is: a measurement switch, documented as one, or a
consumer's option in the shape D10 already has for unsafe code and skipped initialisation. A knob
that exists by accident becomes API by accident.

The first look by framework is declined for now: no branch exists to look at, and a number that
conflates the branch with everything else those builds differ by is exactly the kind this
repository has spent two days learning not to quote. It is taken later if the branch's design
stalls, and the header of any such report names both sides' build properties.

**The switch is decided, and decided the right way (performance-ff).** It is a measurement switch,
documented as one, and explicitly not a performance option. D10's form is for choices the consumer
genuinely owns, where the trade-off is theirs — unsafe code against portability, skipped
initialisation against a guarantee. This one is not theirs: on a platform that has the capability
the fast branch should always be right, and where it is not, that is our defect and not their
setting, so offering it as an option would be offering a workaround for a bug we have not found
yet, dressed as a choice. If an escape hatch is ever wanted for a capability misbehaving in the
field, it is taken deliberately in D10's form with its own reasoning, not inherited from having
needed two folders to measure. Concretely: named as a measurement switch, documented where
measurement is documented rather than on a package's front page, and its documentation says that
flipping it is supported for taking a pair and unsupported as a way to run.

**The discarded fifth: both causes, in opposite proportions (sql-39).** The walk marks itself by the
mark it came from — zero is the final walk, anything else a guard's ask — and the counter sits in
the arm, with the totals agreeing with the old record count. On T-SQL's corpus 17,079 records are
built for an ask against 99,869 in the final walk, 14.6% against 85.4%. On SQL:2023's `select20` it
is the other way: 349 for asks against 53 at the end, 86.8% against 13.2%.

The units differ — the discarded fifth was counted in tree nodes and this in tape records, and a
record need not build a node while a factory may build several — so the reading is a bound, not an
equation. On T-SQL the price of asking can account for at most 14.6 points of the 21, so **no less
than a third of what is discarded is built by the final walk and replaced above**, and since
building for an ask is not necessarily discarded, that share is probably larger. On SQL:2023 the
price of asking accounts for everything.

**So the question is answered without the joint hour, and the hour is declined.** An exact number
in place of "no less than a third" would not change what is decided, which is only whether there is
a project here. There is, on T-SQL, and the next question is not how much but *what*: which
constructions are built and then replaced. That names whether it is the shape of the grammar — a
rule building what the rule above rebuilds — or a cost of the engine, and the two have nothing in
common but the number. Behind the line for `ast.md` and the repeats count.

**D36 is written (`7f59a2d2`), and the size went the other way from the usual.** All four conditions
are closed. The order question was answered in two parts rather than asserted: captures standing
side by side occupy disjoint ordered stretches of the log, so the merged order is exactly "the
first one's records, then the second's", which is what two walks did. Order moves only where a
guard names a value *inside* another — today the inner subtree is built whole and the outer is
finished after it, so a record standing before the inner subtree is built after it, while the
merged walk builds in log order. The set is the same and a child still precedes its parent; only a
factory with a side effect could tell, and §7.2 promises order for guards and "after the match" for
constructions, not this. Noted rather than waved past: if that promise is ever widened, this is
where it breaks.

Condition 3 was the one where time usually wins and the file grows unwatched. It shrank: T-SQL by
8,208 bytes over 48 merged calls, SQL:2023 by 5,172 over 40, taken before and after in one tree.
One call is shorter than the run it replaces, and a machine whose guards never name two built
values emits neither the parameters nor the marking — three snapshots byte for byte unchanged.

**And two defects of its own, both out of bounds, both from editing a branch without reading whose
`else` it was.** A guard with no first value calls with −1 in its place and the walk indexed by it;
the guard added for that then captured the `else` belonging to the gathering branch, sending an
ordinary ask down the side stack with −1. The tests caught both, and expr's own comment is the
right one: caught by tests is not an excuse when the edit was five lines and reading the next line
was cheaper than two runs.

**D40 said "before the first branch", and there are already two.** critic's Q15, confirmed here at
`5eb63fa3`: `BufferedEmitter.MoveLine` emits a `NET8_0_OR_GREATER` branch, added yesterday, and
`Machine.Reader`'s stack probe emits a `NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER` one
whose own emitted remark names what the floor pays, an exception on one probe in sixty-four. Those
are the only two. So the capability rule arrives with two sites to convert rather than none, and
D20's promise that the floor may not regress already has two cases nobody has measured. Both are
converted when the capability mapping lands; neither is grandfathered, because the reason for the
form — a framework test at five sites multiplies by the buckets — applies to them exactly as to a
third.

**And D40's first condition is much cheaper than its wording.** `EmittedCode.Compile` parses
emitted source with *no* preprocessor symbols defined — nothing in the repository calls
`WithPreprocessorSymbols` — against references taken from the test process's own net10 assemblies.
So the whole in-memory level, the refusal sample, the slow suite's whole record, the buffered input
tests and the carrier shapes, has been compiling the **floor** branch and running it on net10 all
along, while the packages and the stand run the capable one. Both branches are already exercised by
different levels of one suite, and nothing anywhere says so. What is missing is one argument and a
second pass, not a test run per bucket. That is the shape to build, and the fact that it was true
by accident for a year is itself worth the sentence.

**The measurement switch can be made a fact rather than a promise.** Declare the compiler-visible
property in `Directory.Build.props` rather than in the package's own build asset, which the
repository reads through its own targets: the pair works and a consumer's build cannot set it at
all. A rule enforced by where a line sits beats a rule enforced by a sentence in a document.

**D37 for the expression language (`4b9e7194`), and a clause the doing added.** Eight fenced calls
across the two pages now compile, run and hold the values their comments claim. All eight passed at
once — the pages were right — and what the rule bought is not a defect count but the tie: until
today nothing bound those pages to anything.

One of the eight was not an example but a *fragment*, and that case will recur in every package.
The skill shows two lines, a static field and a call, and leaves both the type and the text to the
reader; written out literally it does not work, because the language resolves only what the text
and the given assembly say, and the text never declares the type. So the first thing a reader
repeating the page meets is a refusal. **The clause: a page that shows code carries the smallest
complete form of it as a test, and where a fragment cannot stand alone the page says what it
omits.** The defect this catches is not "the call throws" but "the page shows half, and the half
does not work" — which no compiler catches, because the half is not what anybody compiles.

The fragment on the page is fixed by naming the namespace in the text, one line, which is the
smallest complete form the test already holds.

**Why the accidental check was worth nothing until it was written down (performance-ff).** The
in-memory level has been reading the floor branch on net10 for a year while the packages read the
capable one, so the comparison D40 asks for has been running all along and could never have been
quoted, because nobody knew it was happening. A check nobody knows about is not a weaker check, it
is not a check at all: it constrains nothing, because the first person to change what it happens to
cover will change it without knowing they had to keep it true. That is the sentence worth keeping,
not the curiosity that it existed.

And the same session names the other half of the day's lesson: it proposed to make the switch a
measurement switch by naming and documenting it, which is a promise, and the placement in
`Directory.Build.props` makes it a fact held by the position of a line. After a day in which a
promise about the floor turned out to have two unmeasured cases under it, the fact is worth more.
The branch of the wrong form in `BufferedEmitter` is that session's own, written yesterday without
a thought, and it goes over with the other rather than staying as legacy: the multiplying argument
is about the second and third bucket and does not care which branch was written first.

**Two clauses D37 gained from being carried out.** First, from expr: a test that holds a page must
*copy the page's text*, not write its own of the same meaning. Its first test would have passed had
it written the missing declaration in without comparing against the fence — it would have checked
an example that is not on the page. So the half is invisible to a test as well as to a compiler,
unless the test is a copy; and where the copy does not work, the page is fixed and not the copy.

Second, the generator's own pages are the one set that shows grammars rather than calls, so
"smallest complete form" means something else there and has to be settled before anyone starts: a
grammar on a page exists as a test that the generator *compiles it without a diagnostic*, and where
the page says what the grammar reads, that reading is run too. It is in the rule, with that reading
of it. The SQL package's seven calls carry the same trap the expression language's did, its parsers
demanding a completeness a page may not show.

## D41. Where emitted code still reads one character at a time, 2026-09-20

critic's Q16, answering the question put to it — is there a win that needs no branching at all.
Character-at-a-time scanning in emitted code is four places and no more; everything else already
calls the span APIs. Three of the four are ranked, and the first needs nothing the floor lacks.

**The streaming window counts lines by hand, and it is the one place where the input is longest.**
The buffered window keeps its place and moves it with `LastIndexOf`, and its own comment says why:
a feed asking for lines had a branch on every character it read, counted again. The whole-text
located class keeps its place and counts the distance by hand. The *streaming* window's `LineAt`
and `ColumnAt` do neither — a per-character loop from the window's start on every call, which is
exactly the algorithm the located class's remark was written against, in the form where there is
most input to walk. Streaming maps its line and column to it and recovery reports through it, so a
feed asking per record pays the window per record: a shape, not a constant. `IndexOf` and
`LastIndexOf` are on the floor through the memory package that emitted code already requires for
the span itself, which is the same argument `Machine.cs` makes for ordinal comparison. So: no
capability, no bucket, no `#if`. **Approved to design, and the number is available today** by
comparing a streaming parse that asks per record against the same feed buffered.

**And a size defect beside it.** The window class is emitted whenever a grammar streams, ungated,
two lines below the buffered classes, which are gated on whether the grammar locates anything. So
every streaming parser carries the naive pair whether or not a thing calls it. Gated in the same
commit.

**Parked with reasons.** Both incremental implementations vectorize forwards and still walk back a
character at a time. The link chains reset by a loop where the floor has a span fill, weakest of
the three and already cautioned by the clearing anatomy: what `Return` pays is calls and cold
tables, not bytes. Refused for now: the two refusal-path prefix walks, the floor having no
common-prefix API and the net8 one belonging to D40's bucket, and the case-folded one having no API
anywhere.

**One thing it says about the record.** No checked-in snapshot emits the line-moving code at all,
while four of five carry the naive pair — so the implementation carrying a framework branch appears
in no diff anybody reads. A snapshot covering it is part of the work, on the principle recorded an
hour earlier: what nobody reads is not checked.

**Two shapes worth hunting by, drawn from D41 rather than from the fix.** The first: a lesson
learned twice and not carried to the third place. Two classes here already keep their place and
move it, one of them with a note explaining why the naive way was abandoned, and the third does
neither in the form that sees the most input. That is not a constant nobody tuned, and it is
something to search for rather than wait to be shown. The second is the mirror of the evening's
other rule: a check nobody knows about constrains nothing, and **an implementation nobody sees is
the same thing from the other end**. The line-moving code appears in no snapshot, so the branch
written yesterday in the form D40 now forbids has existed in no diff any human or session reads.
And beside them a smaller habit: a class emitted without a gate two lines from classes that have
one reads as deliberate until somebody asks.

**D41's count is five, not four, and the fifth was invisible to the method that found the others.**
critic took a second count off the emitter's templates rather than the snapshots and found
`BufferedBytes.Matches`, a per-byte loop, called from six emission sites — a literal, a literal in
a run, a shared prefix and the residue after one — every one of them exactly where the character
path calls `SequenceEqual` or ordinal comparison. It is not a scan the platform cannot do: it is
the one comparison in emitted code where the decision already taken for characters was never taken
for bytes, and `Machine.cs`'s own argument for the span — folded into word-sized compares, bounds
checked once instead of per element — says nothing about `char` rather than `byte`. Soundness was
checked before it was raised, since read the other way it would have been a correctness defect: a
byte machine is refused outright for a case-insensitive literal or any character above 255, so
every literal reaching it is a byte string, and the assumption in the comment is enforced rather
than assumed.

Priced honestly by its finder and not put above the window: the byte path is FIX's, its literals
are two to five bytes, and much of the dispatch goes through a shared prefix, so what reaches the
comparison is often a one or two byte residue. That is a constant-factor claim, paired as one.

**What the fifth place says about the snapshots is larger than the fifth place.** No checked-in
snapshot emits a byte machine at all, so an entire rendering appears in no diff anybody reads —
the same gap D41 is already fixing for the line-moving code, one size up. Two instances in one
evening make it a question rather than two fixes: which emission paths does the snapshot set never
exercise? That audit is ordered, because a sample whose coverage nobody knows is not a sample of
anything.

**D41's own first item, corrected by its author, and the correction inverts its priority.** "A
shape, not a constant" was wrong: the streaming window's `Extend` counts the newlines it drops, so
asking for a line walks the live window and not the input, and the quadratic that wording described
was found and fixed before — a scaling test holds it, linear from a string and from a reader both,
with a remark naming the old defect. What is wrong is one line above: the *counting*, a
per-character loop over everything the window drops, emitted with no gate although the two fields
it fills are read only by the line and column questions. The buffered window decides both halves
the other way and says so in its own comment — the span search on release, and no counting at all
where nothing locates. So the same two classes disagree twice about the same two decisions.

**So what was filed as a size defect is a time defect first**: one branch for every character of
every streaming parse that never asks a question. That is the part to do first, and the number is a
streaming parse that asks nothing, with the counting and without — every streaming row of the stand
shows it. The line-moving work stands behind it, and the snapshot that covers it stands with
whichever lands first.

**The sharpest form of the evening's heuristic, from performance-ff: when one neighbour has
recorded its reasoning and the other is silent, the silence is the finding.** Not merely that a
lesson reached two places of three — the buffered window decided *both* halves the other way and
wrote down why, so the streaming one differs twice over the same two questions with nothing said
about either. That is a stronger signal than an unexplained asymmetry, because the explanation
already exists a few lines away and simply was not applied, and it costs nothing to check: the
comment is there telling you what the question was.

Beside it, the same session's count of its own day: three things it had reasoned its way to were
corrected by something written down near the code — the carriers report naming a grammar's worst
machine, a scaling test documenting the cliff it works around, and now this comment. Each was
cheaper to read than to derive, and each time it derived first. That is the habit to change, and it
is not only that session's: the architect spent the day handing out a rule about reading the source
and then read a checkout a day old.

**D40's pairing mechanism exists (`5c60d6ee`, `bc1c1492`).** A side builder takes a name, a commit
and any number of properties, builds the five libraries in a worktree of its own — from scratch
when a property is given, so the generator runs again — pinned away from the timing cores, and
writes the commit, the framework, the properties and a hash of the emitted code beside the
assemblies.

**It was tested by a negative control, which is what makes it a mechanism rather than a script.**
One commit was built twice, plain and with a property nobody reads, and both sides hashed to the
same emitted code from two different worktrees — so the hash does not depend on the path, and the
header of a paired report says in bold that two sides with different properties emitted the same
code and that this is therefore not a pair of branches. The header also says the check applies only
to two sides of one commit, so its silence on a pair of commits cannot be read as a positive.

**And that control closes the gap its author reported.** The builder cannot refuse a property the
generator does not consume, and the list of what the generator reads is the generator's to keep —
but a property that does nothing is exactly what the hash comparison catches, loudly, in the report
that would otherwise carry the number. No requirement on the generator follows from this; a control
that fires on the failure it guards against is the guard.

## D42. What no snapshot emits, and why that is the harness rather than the grammars, 2026-09-20

critic's Q17, nine rows, each with the marker it was counted by. The finding is structural: the
snapshot harness compiles each grammar with four options — class name, namespace, the Roslyn
scanner, a line map — every other option at its default and no per-grammar options at all, and it
asserts a single source. So a dozen options cannot appear in the set however many grammars are
added, and a grammar that splits into several files cannot be a snapshot as the harness is written.
**The set is therefore not a sample of what the generator emits; it is a sample of one
configuration of it.**

Ranked by the rule the inventory was ordered with. First, the buffered and byte half entire —
zero in all five snapshots, and it is a reader whose whole job is holding and releasing input under
a retention bound, which is where both of tonight's misses lived; character streaming through a
window *is* covered, so it is the buffered and byte forms specifically. Second, a second rendering
of one grammar: the immediate and state classes are zero, and those are what the expression
language ships and the stand times on every paired run. Third, the lexical half, one of the three
ways the generator writes a way in and the one with the largest measured effect on SQL. Then prefix
tables, locations as an option, a carrier the author asked for, the symbol resolver under a real
implementation, several files out of one grammar, and the sizes rather than the mechanism of part
division.

**The qualification matters as much as the list, and its author wrote it in rather than leaving it
implied:** none of this is untested. The buffered reader has tests, the lexical half has a package,
locations have a measurement. The claim is only the one asked for — no diff shows them, so a change
to what they emit is reviewed by nobody, and a count taken off this set reads zero where the truth
is "not emitted here", which is exactly how tonight's two misses stayed invisible.

**So the harness changes before the grammars do**: snapshots may carry their own options, and a
grammar that emits several files may be one. Then the first three rows get their files. Not a
matrix — a few files chosen to cover the rows, because a set that takes minutes to run is a set
people stop reading. To expr, which is waiting on pairs.

## D43. The grammar rides in the assembly, by a default nobody chose, 2026-09-20

critic's Q18, and it is D42's first concrete instance: where does the configuration the snapshots
hold differ from the one a consumer gets? Here, by an attribute carrying the whole grammar.
`Portable` follows the host's visibility — the attribute's value, else an inherited one, else
whether the type is visible — and where it holds, the emitter writes the grammar's entire text onto
the class as an attribute argument. The snapshots leave it at the compiler options' default, which
is false; a public parser's default is true.

**So the SQL package ships 697,784 bytes of grammar text as attribute blobs** — 441,215 for T-SQL,
223,698 for the standard, 32,871 for SQL-92 — because its three parsers are public. The expression
language carries its inline grammar twice, as the grammar's own argument and again as the carried
text. The Web package pays nothing, its grammar classes being internal, which is the shape of its
design rather than a decision. And the one package that sets the option off is Finance, whose
grammar is the smallest and already in metadata as its own argument: the decision exists and was
taken where it saved a duplicate of a small grammar, not where it adds seven hundred kilobytes of a
large one.

**What settles it is not known and costs one run.** The source figures exist and do not answer it —
441 KB against a 22 MB generated file is two per cent of *source*, but code compiles down by a
large factor and a string does not compile down at all, so the share of the shipped assembly is a
different question. The instrument exists, has never been pointed at this, and needs no pair and no
window. Ordered from stand.

**And a check that costs nothing comes first, which critic already ran:** nothing in this
repository includes another project's grammar through the class. The one inclusion is inside a
single project, where the file is read and the carried text is only the fallback. So the default is
paying, in every public host, for a capability offered to consumers and used by nobody here. That
may be exactly right for a library — a consumer building a dialect on our T-SQL grammar is the
case it exists for — which is why it is stated with the number beside it and decided by Igor, the
default being part of what a grammar can say. A snapshot with the option on belongs in D42's list
either way.
