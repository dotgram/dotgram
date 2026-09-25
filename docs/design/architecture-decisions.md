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

**Corrected where it stands: the third bucket named the wrong API.** Recognising a keyword is not
`SearchValues<string>`. That searches a HAYSTACK for any of many needles; recognising a keyword is
a LOOKUP — is this span one of four hundred and ten words, and which one — and the API for it is
`Dictionary<string,V>.GetAlternateLookup<ReadOnlySpan<char>>()`, which needs the comparer to
implement `IAlternateEqualityComparer<ReadOnlySpan<char>, string>`. Both are net9, so the bucket
does not move and nothing else in this decision changes. It is corrected anyway, because somebody
building from it as written reaches for the wrong tool, finds it does not fit, and concludes the
bucket was overrated — a decision that names the wrong instrument discredits the finding it
carries. `FrozenDictionary` offers the same lookup, which matters here: this decision already puts
frozen tables in the net8 bucket for a grammar over tokens.

**And the rule about sources paid twice on one API.** A current article says `SearchValues<string>`
arrived in .NET 10; the reference's moniker range for `Create(ReadOnlySpan<string>, StringComparison)`
is net-9.0 through net-11.0. Read off the reference, it stays settled. The rest, each version read
rather than recalled: `SearchValues<byte>`/`<char>`, `MemoryExtensions.Count`, `IndexOfAnyExcept`,
`System.Text.Ascii` and `decimal.TryParse(ReadOnlySpan<byte>)` are net8; `allows ref struct` is
net9. `[InlineArray]` is explicitly NOT priced: believed net8 and C# 12, not read, therefore not
counted.

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

## D44. A configuration that is shippable and broken, found by the first snapshot, 2026-09-20

D42's first three rows are written (`16bd6c38`, `ff1ac947`): the harness now takes a snapshot of
every file a compilation yields, under the name the generator gave it, so a split file and a second
rendering land by themselves; a grammar declares what it is compiled with through a table in the
test rather than a new format in the file; the five old snapshots are byte for byte unchanged. Three
grammars, one per row, no matrix — the buffered and byte halves with a retention bound small enough
to read in the file, the same grammar a second time under a suffix and the immediate carrier (the
two files differ by 2,911 lines), and the reading over kinds.

**And the first row found what the exercise was ordered for.** A grammar with named captures
compiled over bytes emits code that does not compile: a capture with no declared type is a span of
bytes over that input, while the record it is put into is declared with string fields, so every
field is a conversion error. The only diagnostic emitted is an informational note about the
carrier — no refusal, no warning. That configuration is shippable and broken, and nobody had seen
it because no snapshot emitted a byte machine.

**The ruling: this is a refusal, not a silent default.** What a capture without a declared type
means over bytes is a question with three answers — refuse, decode to text by some encoding, or
hand back the bytes — and two of them are decisions about the consumer's data that the author has
not made. Decoding silently chooses an encoding and allocates; handing back bytes changes the
record's shape under a grammar that reads the same over characters. So the generator says so, by a
diagnostic naming the capture, and a default conversion, if one is ever wanted, is a change to what
a grammar means and therefore Igor's. To performance-ff, with the diagnostics document gaining the
number in the same commit, and expr's `Buffered` grammar noting in its header why it has no
captures.

**Two sentences from D44 worth keeping past it.** The snapshots were argued for on the ground that
an implementation no diff shows is unheld whatever its tests say — that it would catch a *change*
nobody could see. The first file a new snapshot produced showed a configuration that ships and does
not compile: it caught a *state* nobody could see, which is the stronger case and not the one that
was made for it. And on the neighbour check: if a declared type over bytes has the same hole it
closes with the same refusal rather than a second one, because two diagnostics for one confusion
teach a consumer that we do not understand our own boundary.

Beside them, the reason refusing beat being helpful, in the words of the session that had to resist
it: decoding silently would look like the accommodating choice and is the worst of the three — it
picks an encoding nobody named, allocates where the grammar promised not to, and does both
invisibly, so a consumer meets our decision about their data as a performance figure or as mojibake
months later. Handing back bytes is quieter and makes one grammar mean two things depending on how
it was compiled. Refusing is the only answer that decides nothing on the author's behalf, and the
diagnostic naming the capture is what turns it from an obstruction into an instruction.

**D43 has its number: about 5% of the built assembly, not half** (stand,
`benchmarks/results/portable-size-2026-09-20/`, `392329b8`). Built release once each from one
commit, the option written off on every grammar host: the SQL assembly falls by 924,160 bytes of
19,811,840, 4.7%, and by 5.8% compressed, which is the figure a package carries; the expression
language by 87,552 of 1,651,200, 5.3%, and 5.8% compressed. The two frameworks agree to a kilobyte.

Two qualifications the measurement was asked for and gives. For the expression language the option
removes the *second* copy only: the grammar is also the argument of the attribute that declares it,
which stays whatever the option says, so 5.3% is all it can ever save there. And for SQL the
difference is 226 KB **larger** than the three grammar texts together, which is about the size of
the standard's text — so something travels once more than counted, a second class of one grammar
or a nested one. That was not chased, the order having been for an order of magnitude.

**The recommendation to Igor: keep the default.** Single digits of an assembly do not pay for
surprising a consumer who builds a dialect on our grammar and finds the text absent, which is the
case the option exists for. The 226 KB is a separate question and a better one: a grammar's text
emitted more than once is a defect rather than a design cost, and it is ordered.

## D45. A criterion that named rows the mechanism never reaches, 2026-09-20

D36's pair said T-SQL did not move, which is the condition under which expr undertook to withdraw
the change. Before withdrawing it counted how often those rows ask a guard at all: `columns1000`
three walks a parse, `conditions1000` three, `rows1000` two, a script statement three, none of them
merged. `sql/select20` makes 194 walks of which 107 merge. **So the rows the criterion named make
two or three guard asks in a whole parse, and no change touching guard asks could move them.**

**The criterion is void rather than met, and that is a different thing from being re-read after the
numbers.** What makes it void is a fact independent of the outcome: those rows do not exercise the
mechanism, which was true before the pair was taken and would be true had the change helped. expr
established it and then refused to re-read its own criterion, handing the decision over instead,
which is the only way a pre-registered criterion survives being wrong — the author may show it
measured nothing, and someone else decides what follows.

**Decided: the change stays, unvalidated, and a new criterion is registered now.** It must show on
rows that reach the mechanism — `sql/select20` and the SQL:2023 rows, where the first measurement
already fell by 3 to 8% — and must not cost T-SQL or the expression language beyond noise. And
before that measurement, the premature flush is fixed: the reader merges only asks emitted back to
back and flushes before a text capture, so a guard naming a text capture between two built ones
falls into two walks although a text capture builds nothing. That is why the corpus merges 3.1%
where the static count promised far more — the gap is in the implementation, not in sql-39's count.

**And the rule the episode leaves, which is the stand's as much as anyone's: before pairing a
change, ask how many times the row reaches the place being changed.** A row with nothing to measure
returns zero, and that zero is indistinguishable from "the change does not work". The question
costs a counter and it is asked before the window, not after it.

**The 226 KB, accounted for to a seventh, and the rest the emitter cannot produce** (critic's Q19).
The attribute is written once per compilation — innermost class part only, never for a suffixed
reading, with the reason beside it: a suffix puts a second reading in a nested class, and what an
including grammar names is the class. What is carried is the *spliced* text, not the file: the host
grammar joined with every included one, because across a project reference the included grammar may
not be reachable, so what travels must be the whole of what was compiled. By host that is 223,698
for the standard, 32,871 for 1992, and 474,086 for T-SQL, which is compiled from itself joined with
1992 — 730,655 in all. So 32,871 of the excess is the design's price and should be named in the
figure rather than surface as a surprise, and it is **not** a defect. The remaining ~193,500 the
emitter cannot produce: one attribute per compilation, three compilations carrying a text.

**So the measurement is now the suspect, and its own shape says why.** The three figures are
multiples of 512, a PE file's alignment: the number is a difference of two builds' file sizes, and
such a difference attributes everything that moved to the thing that was changed. What settles it
is a count of the attribute blobs in the built assembly — rows and lengths, read from metadata
rather than built. Three rows of the expected lengths mean the emitter is right and the residue is
in the measurement; a fourth row, or one at twice its length, means a defect living where these
templates do not show. Ordered from stand, which holds the builds.

**And the denominator needs saying whenever the percentage is.** The T-SQL parser also declares a
located reading under a suffix — a second whole compilation of a 441 KB grammar, its own
recognizers and materializers, in its own class, carrying no grammar text and weighing a large part
of the assembly. Any "the text is N% of the assembly" is a fraction whose denominator holds T-SQL
twice. That is the design's own choice and priced in the comment above it, but it belongs in the
sentence with the percentage.

**Counted rather than subtracted: the text is 3.7% of the SQL assembly, not 4.7%** (stand,
`36aad585`). Three attributes in the built assembly, each once, no fourth and none twice:
474,107 bytes for T-SQL glued to the 1992 standard, 32,871 for 1992, 223,695 for the standard,
730,673 in all — critic's prediction to within 18 bytes. The expression language carries one
attribute of 67,567 bytes and the same text again as the argument that declares the grammar, which
stays whatever the option says, so counting gives 4.1% there against 5.3% by subtraction.

**And the difference of the two builds was 924,160, which is 193,487 bytes more than the
attributes hold.** Alignment cannot be it: a PE section rounds to 512 bytes and this is three
hundred times that. So the option changes something else in what is emitted, and nobody has looked
at what. That is the question the accounting leaves, and it is a good one: a small, exactly posed
one where before there was a vague excess.

**The recommendation to Igor stands and its number improves**: the grammar text on the classes is
under four per cent of either assembly, which does not pay for surprising a consumer who builds a
dialect on our grammar and finds the text absent. The denominator holds T-SQL twice, and the
result's own page now says so.

## D46. The assembly carries the generated source, compressed, and every size we quote includes it

critic's Q21 closes D43's residue and opens something larger. `Portable` pulls nothing else along:
the grammar text reaches exactly one emitted line. The missing 193,487 bytes are elsewhere — every
package sets embedded symbols, with the reason written above it (a stack trace from a parser here
has line numbers without a symbol server), and the repository sets sources-untracked-by-source-control
to be embedded beside its SourceLink block. Generated code is exactly what a debugger cannot find
on disk, so it travels inside the symbols. **The grammar therefore rides twice**: once raw as the
attribute blob and once as part of the generated source, deflated — 730,673 × 26.5% is 193,600
against the 193,487 that was missing.

**So the two measurements were never in conflict, and the one to quote Igor is the subtraction.**
Counting answers how many bytes of grammar text sit in the metadata, 3.7% and 4.1%; subtracting
answers what the file loses if the option is off, 4.7% and 5.3%, and that is the question being
asked.

**And the consequence is larger than the option.** If the symbols carry the generated source they
carry all of it: 22.0 MB for T-SQL, 13.8 for the standard, 0.8 for 1992 — 36.6 MB of generated C#
compressed inside a 19.8 MB assembly, which at the same ratio is seven to nine megabytes, a third
to a half of the file, and explains its size better than its IL does. Every assembly size this
repository has taken includes it, the code-size harness among them.

**Nothing here is wrong and the decision is not reopened by me**: symbols in the assembly have a
reason written beside them, and a stack trace through generated code without a symbol server is
worth paying for. What is wrong is that no figure we quote says it is being paid. And the trade may
be finer than it looks: embedded symbols alone give the line numbers the comment asks for, while
embedding the sources is what buys stepping into generated code — so the saving, if it is wanted,
may cost less than the reason it would seem to contradict. Igor's, with the number first: a read of
the debug directory and the symbol file's document table, both through metadata, two numbers and no
build.

**D46 counted, and my estimate of it was wrong** (stand, `488a7216`, read from metadata, no build).
The embedded symbol file of the SQL assembly is 3,544,414 bytes in the file, 17.9% of it, and
7,208,884 decompressed. Of its 29 documents, 13 carry their source, all generated or written by the
build: 39,044,259 bytes of text stored as 2,839,013 — **14.3% of the assembly**, compressed about
fourteen to one. I had estimated seven to nine megabytes by applying the assembly's own ratio to the
source; the ratio is the deflate ratio, not the assembly's, and the whole symbol file is 3.5 MB.
The residue closes too: each grammar text deflated on its own is 191,805, and 730,673 + 191,805 +
1,682 is exactly the 924,160 the two builds differed by, the 1,682 being what escaping a text into
a C# literal costs. The expression language agrees to 83 bytes.

**So the trade Igor is offered is smaller than it looked**: embedded sources cost 14.3% of the
assembly, not a third of it, against stepping through generated code without a symbol server. The
number belongs in the sentence; the recommendation is to keep them.

**What the code-size harness must say from now on.** A change that shrinks emitted code shows in an
assembly twice: as its IL, and again as about a fifteenth of its source through the symbols. Every
size taken so far includes both, which makes none of them wrong and all of them a different
quantity from "how much IL did this add".

**And one thing found on the way that is not about size.** The documents carry the absolute path of
the tree that built them. A shipped package should not name a build machine's directories, and a
build meant to be reproducible must map them; whether the packing workflow does is a question with
a yes-or-no answer, and it goes to finance-24 with the release mechanics.

**Whose estimate it was, corrected: critic's, and mine to have passed on.** The seven to nine
megabytes came from Q21, where the deflate ratio of *grammar text* — about 3.8 to 1, just measured
— was applied to generated C#, which compresses 13.75 to 1. That is this file's oldest family in a
new dress: a number that resembles a measurement because it was derived from one. The right ratio
was one read away, in the same symbols that answered the question. My part is that I took it into
the journal and to Igor as a figure rather than as an estimate from a borrowed ratio, which is the
step that turns someone's arithmetic into everybody's fact.

And the two measurements that disagreed by 193,487 now agree to the byte: 730,673 + 191,805 +
1,682 = 924,160. Each had answered its own question correctly all along — one counting what is in
the metadata, the other what the file loses — and the disagreement was between the questions, not
between the numbers. That is the sentence to keep from the whole episode.

**D36's flush is fixed (`fc0bb08f`) and was not the cause.** Merged walks over the corpus went from
45 to 57, 3.1% to 4.0%. sql-39 counted 45% of asks as second or later in one guard's run; between
that and four per cent lies something other than the flush, and expr declines to call either figure
"the share the change takes" until the two methods are one. The candidates are named: the counters
may count different things — asks on a build without the change against merges on a build with it —
or the samples differ, two hundred files split on the batch separator against the whole corpus of
7,716 statements.

**That reconciliation is now the interesting question, not a tidiness.** If 45% of asks really are
non-first in a run while only 4% merge, then nine tenths of the clusters are not being merged for
some reason nobody has named, and that reason is worth ten times the change already written. If it
is the sample, the static and dynamic counts agree after all and the change takes what it takes.
Either answer is worth having, and it costs one run: the same counter, on the current build, over
the same named set of files. Ordered from sql-39, who has the counter.

The pair does not wait on it: the new criterion names `sql/select20`, which merges 107 of 194
walks, and the SQL:2023 rows.

## D47. The generator writes absolute paths into what ships, 2026-09-20

finance-24 checked the built assemblies rather than the settings, with a reader of its own for the
embedded symbol file, and the answer is half yes and half no.

**Yes, for the document table.** The continuous-integration flag is set on all five packable
projects under the CI environment, and with the SourceLink properties beside it the SDK maps every
document path: built with the flag, Web has 46 documents and 46 neutral, Sql 29 of 29, Finance 26
of 26, and without it all of them are the machine's, as a local build should be. The `.gram` files
appear there too, so the file named by a line directive is mapped as well.

**No, for the embedded text.** Embedding untracked sources puts the *text* of generated files into
the symbols, and text is bytes from disk — in which our generator has written line directives
carrying absolute paths. Sql has 4 of 13 embedded sources naming a machine, Web 13 of 31, Finance 1
of 7. The compiler mapped the path when it recorded the document and did not touch the text it
merely embedded. So the shipped package does name build machines' directories, not in the table
where it was looked for but inside the 2.8 MB of compressed source that D46 found by weight.

**No property fixes this; we write the path, so we fix it.** To performance-ff, design to the
architect first, because there is a choice — apply the compilation's own path map so that the
generator agrees with the compiler, or write a path relative to the project — and the emitted
output changes either way, so the snapshots move with it.

**One edit closes three things.** The packages stop naming directories that are not the consumer's.
The standing rule that generated size may be compared only within one worktree, which exists
*because* line directives carry an absolute path, is retired rather than worked around. And
reproducibility stops being half-closed: while the text carries a path, two builds of one commit on
two machines differ in bytes, so "packed again from the same commit" cannot be checked — which is
the other half of the hole the publishing workflow's smoke closed. Whether mapping the paths makes
two builds byte-identical in every other respect is a separate check, by comparing two builds and
not by reasoning; finance-24 says so itself rather than claiming it.

**D47's choice, narrowed by reading the code.** The path reaches the generator as a piece's own and
goes straight into the line map; nothing in the generator reads a path map, a project directory or
the CI flag today. The compilation does carry the compiler's own map, and applying it is a few
lines on the Roslyn side of the seam, where that knowledge should stop anyway. **But it closes one
of the three things, not three.** A map exists only where something sets it, so agreeing with the
compiler neutralises exactly the builds whose document table was already neutral, and leaves every
other build absolute — which means two builds of one commit still differ off CI, and the rule about
comparing sizes only inside one worktree still cannot be lifted, because the person comparing sizes
is running a local build.

So the choice is: agree with the compiler and close one, or write a path of our own and close all
three. **Approved in principle: the path of our own**, on the condition performance-ff set itself —
that navigation from an emitted file to its grammar is tried in an editor before it is claimed, not
reasoned about. The specific thing to try, because it decides whether the shape works at all: a
generated file sits under the intermediate directory, and a relative directive is resolved against
a base the reader chooses, so a path relative to the project may resolve from the wrong place
exactly where a person clicks it.

**And the third shape is rejected for the right reason** — map where a map exists and go relative
otherwise makes the emitted bytes depend on the build's flags, which is the reproducibility hole
being closed, wearing a hat. Emitted output that differs between a local build and a CI build is
the defect, not the workaround.

**Two things the D47 exchange leaves, both about instructions rather than paths.** "Try it in an
editor" is the right instinct and a vague instruction: a week later the file would have been opened
from wherever was convenient, the click would have worked, and the form would have been declared
sound. **An instruction to try something names the case that decides it, or it will be tried where
it works.** Here that case is opening the generated file from the intermediate directory, because
the question is not what the path is relative to in our heads but where the reader is standing when
it resolves one.

And the consequence of a failure was mis-framed as a trade-off by the session that would have paid
it: if navigation breaks, the base was simply wrong, and the right base is the generated file
itself, that being the only thing the reader certainly has in hand. Project-relative was an
assumption smuggled in as the obvious meaning of "relative". A form that fails its own test may be
a wrong parameter rather than a cost to weigh — the two look alike and are not.

The header line goes in with the change rather than after it, and it answers the question that will
be asked: not why the path is relative, which the commit explains, but relative to what, which
nothing does.

**D26 corrected again, and the method is the finding.** Reading the file established what the
validate flag is checked *against* — the order of three header fields — and the words "and nothing
else" were wrong: the flag also calls the message's own validation, which checks the body length
and the checksum. What found it was not a re-reading but a run: a hand-built message was refused
with an expected body length against a received one, and the stack named the method called from
the parse. **Reading tells what a flag is checked against; running tells what it then calls.** Both
are needed, and yesterday this session was told — by me — that the first was enough. The substance
of its earlier correction stands: validation against a dictionary is still a separate call, and
the three readings are still three.

Two facts from the same hour. The message-definition package carries the dictionary file inside
it, so the second pairing needs no copy of anyone's file in this repository and the licence
question does not arise for it; the dictionary form was built and accepts a properly framed order.
And a trap caught by the trap: the message package's version is 1.14.0, 1.13.0 does not exist, and
asking for 1.13.0 resolves silently upward with a warning. The rule that follows is general —
**check the version that was restored, not the one that was requested** — and it applies to every
reference this repository pins.

**The agreement check decided D26's rows before any of them were timed.** finance-24 ran both
readings over every FIX input the stand already carries and reports, without taking a number:

**The message-level pair holds.** A properly framed order — body length and checksum computed, not
written — is accepted by our strict mode and by their dictionary form followed by the explicit
validation, both saying the same thing. It needs a new input, none of the present ones serving.

**The field-level pair cannot exist, and not through a defect.** Their message holds fields in a
dictionary sorted by tag, so wire order is lost — we hand back the tags in the order they arrived,
they hand them back sorted within each region — and **repeated tags collapse**: on an input of
sixteen fields carrying one repeated tag we return sixteen and they return one, the second
occurrence overwriting the first. Without a dictionary that is true of the members of a repeating
group as well, which is the ordinary case in this protocol. So on such an input their reading does
a fraction of the work — a sixteenth here — and a row would show them faster by exactly that
factor. **That is not "they are faster", it is "they read less", and in a number the two are
indistinguishable.**

**Decided: no field-level row at all**, rather than a row with a caveat. A caveat does not travel
with a figure, and the difference in work reaches sixteenfold and depends on the input. Only the
message-level pair is taken. If a field-level figure is ever wanted it can only be without an
agreement check and on an input with no repeated tags, no data fields and no damage — which is an
input with almost nothing to compare.

Of the five present inputs, one passes the field-level agreement check. The rest differ by order,
by our recovering where they throw, by a data field they have no notion of, and by the collapse.
Those differences belong in D26's account of the libraries, which is what that account was asked
for: their model keyed by tag against ours keeping the wire. And one thing is stated as unchecked
rather than assumed — whether order and repeats survive when their message is read *with* a
dictionary and a factory, the store being the same but groups then assembled apart.

## D48. Two counts withdrawn, and what they were withdrawn by, 2026-09-20

sql-39 reduced both counts to one method — one run, the current build, the corpus named exactly
(every script under the SQL corpus, cut as the reference benchmark cuts it, 7,716 statements, with
the soundness check that the final walks number exactly the statements) — and the run found two
errors of its own.

**First, the marker for "built for an ask" was the rule's mark, and an outer rule's mark is zero**,
so every guard of an outer rule was counted as a final walk. With the marker set at the entry
point: on T-SQL 70,269 records are built for asks, **60.1%**, against 46,679 at the end; on
SQL:2023's `select20`, 91% against 9%. The earlier figures were 14.6% and 85.4%. **So D38's
conclusion is withdrawn**: the price of asking accounts for the whole of the discarded fifth on
T-SQL as well, and "no less than a third is built by the final walk and replaced above" rested on
a spoiled marker. Both grammars turn out to be the same case, not opposite ones.

**Second, the 45% that D36 was credited with.** Grouping asks by run attributed each final walk to
the last guard, so all 7,716 final walks fell into "not first in a run". Corrected: 222 of 8,634
asks are not first in a run, 2.6% now, about 9.6% before merging — not 45%. **So there is no
tenfold lever hiding in the gap**, and the gap itself was never real: expr's 4.0% counts merges
against all walks, the corrected 9.6% counts runs against guard asks, and the denominators differ
while the phenomenon is one.

**What this leaves D36.** Its benefit is where sql-39's numbers now put the clusters: on T-SQL a
guard usually asks for one value — 726 merges against 8,634 asks — while SQL:2023 merges 107 of 193
runs because its guards ask for several. So the expectation written before the first pair, that
T-SQL would move most, was wrong for a structural reason, and the criterion registered after it,
naming `select20` and the SQL:2023 rows, is the one that matches where the mechanism lives. The
change is not weakened by the correction; the reason to expect it anywhere but SQL:2023 is.

**And the shape of both errors is one shape.** A marker standing in for the thing itself — a rule's
mark for "a guard asked", the last guard of a run for "whose run this is" — which is the family
this journal has been naming for three days, now in its own measurements. What caught them was
reducing two counts to one method, which was ordered to settle a disagreement and settled something
else instead. Both wrong numbers had travelled further than the session that made them, which is
why they are recorded here and not only fixed.

## D49. The first sweep that actually went outside, 2026-09-20

critic's Q23, opening with what the two earlier sweeps were not: both were titled "the outward
sweep" and read nothing outside this repository. That is said in the entry rather than quietly
fixed, and it is the right way round — a survey of what the outside does is worth having only if
someone went there.

**The deferred version question is settled by reading the reference, as D40 required.** The
byte and char forms of the platform's value-set search are available from net8.0; the *string*
form, with its comparison argument, is net9.0. So D40's third bucket — recognising a keyword from a
span without making a string — is net9.0 and not net10.0, and the first bucket needs no revision.
Two details beyond the version: the string form takes a comparison and accepts ordinal and
ordinal-ignoring-case only, which is exactly what a keyword list wants and exactly what the
emitter already chose for a single literal.

**One item lands where D20 is strictest.** A published account of beating a fast Rust lexer
generator attributes its 20 to 35% to three things, two of which are already ours: a keyword
compared as a machine word, and an ASCII fast path, which is D39. The third is not — replacing the
skip loop's table lookup with a word-at-a-time comparison, trading a jump table's data dependency
for a control dependency. It lands on the 58 seam places and specifically on the **floor** branch,
the one D20 forbids to regress and which keeps its per-character loop where the capable branch
takes the platform's search. It is the only candidate anyone has named for making that loop faster
with no API at all. The caveats are in the entry because the source does not carry them for us: a
different architecture, a different language, a gain that shrinks on realistic input by its own
numbers, and a clean case of a star over one character where our trivia is a braced set with two
comment forms. **What transfers is the idea and not the measurement**, which is the condition on
taking it further.

**And one refused on our numbers rather than its merits.** A formulation that memoizes every
position is further from our shape, not nearer: the anatomy has 1,381 refusals of 2,023 calls, each
on the first token, so what we would be memoizing is answers we reach immediately.

The computed recovery sets stay where D33 put them — Igor's, because they change what a grammar
says — with the mechanism now named: follow sets of the dominators, and dominators are one pass
over a graph we already build.

**Q20 closed (`4179707c`), and holding what a refusal *says* found two differences at once.** One
was an omission and is fixed: over buffered input the message did not name the rule, where the
string form names it — and the `Buffered` snapshot, one day old, showed the fix as a diff, which is
what it was added for. The other is named and pinned rather than smoothed: where a repetition
inside a called rule could take another turn, the string reading names what that turn would accept
and the buffered one does not. What is known: it is not under-filling, the two agreeing at every
split from one character to the whole input, and it is not the end of input, a refusal with text
still after it giving the same. What is not known is why the turn's expectation is not recorded
over a buffer. A test holds both exact strings, so it cannot drift away quietly.

**That one is the reader's machine rather than the differential, and it goes to expr with notice to
performance-ff**, whose area it is and whose queue is three deep. It matters beyond itself: D40's
condition is that the branches of a capability answer alike, the only branch we have lives in the
buffered reader, and a message that differs by input form is exactly the class of difference that
condition must be able to see.

**And a rule for the first time D40's condition disagrees.** That check compares one build against
itself compiled differently, over the refusal record. If a message already differs by input form
for a reason that is not branching, the first run reports a disagreement, and **the obvious wrong
move at that moment is to accept it as pre-existing and filter it out** — which quietly widens what
the check is allowed not to see, for as long as the filter lives. So the order is: the message
difference is explained before condition 1's second pass is built, or, if it is not, the readings
it affects are named exactly and the check excludes those by name rather than by symptom. A filter
written by symptom hides the next defect that wears the same symptom.

**The buffered refusal's missing expectation, explained.** A repetition whose turn begins with a
known character gets a door: if the character is wrong the turn is not read at all. But a turn that
was not read says nothing about what would have refused it, so the emitter puts the door under a
condition — a quiet reading leaves by the door, a recording one reads the turn and records what it
wanted — and that condition is emitted *only where the machine has a quiet reading at all*. A
machine whose every reading records, which is what buffered and streaming are because the input
cannot be read twice, has no condition and always takes the door. Hence the turn is never tried and
its expectation never recorded. Affected exactly: readings of machines with no quiet reading, only
in a repetition whose turn has a door, and only in the message — outcome, position and value agree,
the differential holding all three. That is the list by name D40's check would exclude by, if it
ever has to.

**Two fixes, one already disproved by building it.** Recording what the *door* wants is cheap and
wrong, and expr found that out by assembling it rather than by reasoning: the messages diverged
further, because the door promises something the turn would not have said — the turn may go deeper
and record its own, or be displaced by a further position. Not placing the door where every reading
records is correct by construction and costs buffered and streaming readings their fast exit at
every repetition end, which is what the door is for.

**Decided by measurement rather than by taste.** The choice is a message that omits an alternative
against a fast exit on every repetition end of every buffered and streaming parse, and neither side
of that is obviously worth the other. So build the second fix and pair it on the buffered and
streaming rows: inside noise, take it, because an incomplete expectation is not merely terser — it
names fewer alternatives than are true, and a reader fixing their input is misled by the omission.
Materially costly, keep the door and write the difference down where the forms are described, with
the cause rather than the symptom. Either way the test holding both exact strings stays.

**The second fix was built, run and refused: it does not produce agreement, it reverses the
disagreement.** With the door removed where every reading records, the pinned difference vanished
and a new one appeared the other way — over one grammar the buffered form now names an alternative
the string form does not. The cause, read off both renderings of one grammar in their built form:
the door's condition depends on whether the machine has a quiet reading *and* on whether a refusal
in that rule is recorded at all, and the second flag is legitimately different between the two
machines of one grammar. **The renderings' messages depend on analyses that differ by construction,
so word-for-word equality is not reachable by a change in one place.**

**Ruling, and it dissolves most of the problem rather than paying for it.** D40's condition is
untouched: that check compares one build against *itself compiled differently* — the same grammar,
the same rendering, the same analyses, one capability flipped — so verbatim equality is exactly the
right check there, and a difference between *input forms* is not a branch difference and never
enters it. Nothing has to be excluded by name, and no filter is needed.

What the specification promises about a refusal is therefore **soundness and not completeness**: an
alternative a message names is one that would have been accepted at that position, and the set may
be smaller in one input form than another because the two read the input differently. That is
written where the forms are described, with the cause. The differential keeps holding outcome,
position and value exactly — those agree and must — and stops requiring the message to be equal,
while expr's test keeps both exact strings so that a change to either is seen.

**Unifying the analyses is refused for now**: it is days, it touches what counts as a scan and how
far the seam reaches in each rendering, and it would be paid on the hot path of both forms to make
a refusal message longer. If an editor ever needs the complete set over a stream, that is the
reason to reopen it, and it will be a reason about a consumer rather than about symmetry.

**A guarantee that stays in correspondence becomes an assumption.** The one D40's first condition
rests on — that two compilations of one grammar differing only in a method body must name the same
refusals — was stated in a letter and is now written where the sessions read it, under the paragraph
it follows from, as a consequence rather than a new rule: a choice confined to a body is made after
the analyses have run and cannot move an analysis flag; the analyses settle what is read as a scan,
what records, and therefore which of a repetition's doors are unconditional; those decide what a
refusal names. So a disagreement between the two compilations is a defect in the branch and not a
property of it, which is what the check must be able to say. Beside it, the boundary, because
without it the sentence invites the wrong reading: refusals across two *input forms* are a different
question, the analyses there differ legitimately, and neither form names a superset of the other.

**Two conditions on it, neither about its content.** It lands in its own commit, not folded into
whatever unrelated work reaches it first: a rule every session follows must be visible as a change
to the rules, and today's whole theme is that what nobody reads is not held. And the architect
reads it after it lands rather than before it is written, which is the right order for a consequence
of an existing paragraph and the wrong one for a new rule — the distinction being exactly what the
author made when deciding to write rather than to propose.

## D50. A criterion named through rows needs two questions, not one, 2026-09-20

D36's second criterion is void as well, and expr said so before anyone asked. An A/A run — the same
build against itself, nine rounds in the quietest window of the day — moves `select20` by −2.9%,
`nest8` by −5.4% and `refused-late` by −1.5%, with the sign holding across most rounds. So the
SQL:2023 rows fall on their own, and "they moved" is satisfied by the floor rather than by the
change. What is left above that floor is two to five points on small nested and refused rows, in a
window whose base wandered by 24 to 108%, which the stand reads as drift and not as an effect.

**The first criterion named rows that do not reach the mechanism; the second named rows that fall
without it.** Both were registered before the numbers and both were void, for different reasons,
and what they share is that rows were named without asking what those rows do *without* the change.
So the rule gains its second half: a criterion named through rows is checked by "does this row
reach the mechanism", which cost the first failure, **and** by "what does this row show with
nothing changed", which cost the second. The A/A answers the second, and it is ordered *with* the
pair rather than after it.

**And the repair expr chose is the right one twice over.** The form that does not depend on drift —
two builds, differing by one constant, rounds taken in turn — is the same form that settled the
clearing question. And the expectation is now named in *numbers* rather than in rows: zero to three
per cent on `select20`, and zero within the spread of the rounds withdraws the change. A criterion
in numbers cannot be satisfied by the floor, which is what both of its predecessors were.

**D50 in the stand, and what it costs.** The paired run now takes, after each run of the pair, a
run of the parent against itself over the same rows in the same slot, and the pooled result goes
into the report as a column beside the pair's — "+3% against an A/A of +5% [−4..+9]" on the row
itself, rather than a paragraph under the table. That placement is the point: a caveat that does
not sit beside the number does not travel with it.

It doubles the runs, not the builds. On few rows that is minutes; on a whole-stand pair of 150 rows
it is the stand twice, paid for rows nobody will quote. **So the rule is by use rather than by
row: a row that will appear in a conclusion carries an A/A, a row printed as context does not.**
An order that names rows gets the A/A on those; an order that is a baseline run gets none, and the
report says it has none, so that a row from it cannot later be quoted as an effect. Turning it off
deliberately stays possible and visible in the report.

**The FIX message layer allocates three times what reading the fields does, and nobody has looked.**
finance-24 split one window's figure with a single run on one input of seventeen fields: reading
the fields alone is 1,216 bytes, 71.5 a field; the strict message is 4,192; **the lenient message is
4,192 as well.** So the layer adds 2,976 bytes, 71% of the total and 2.4 times the cost of reading
the wire — and the third row, taken for nothing, clears the first suspect: the strict mode's checks
cost no bytes at all, so what is there is the building of a message and not its validation.

A day was spent bringing the fields to 48.9 ns and below the hand-written parser, while the layer
above them was never in a stand row or a profile. **Approved: an allocation profile and an account
of what the 2,976 bytes are made of** — nodes, regions, group entries, dictionaries — before any
change is proposed. It needs no window, it is diagnosis rather than measurement, and the session is
blocked on other people's work meanwhile. Either answer is worth having: a cliff or a pass nobody
needed is cheap to remove, and "this is the message graph and it cannot be less" is a result that
stops the question being asked again.

## D51. Igor decides: the kept tokenization stays hidden, and the ISO grammar stays, 2026-09-20

**The kept tokenization keeps its present shape.** No reading object in a generated parser's public
API: `TransactSqlParser.Over(text)` is declined. The tokenization goes on being kept where it is
kept now — two slots per thread, the text held weakly and the tokens strongly, a slot taken by the
next text — and the defect that lived there is found, fixed and covered by a test. What stood
against the machinery was that it is invisible; what stood against replacing it is that the
replacement's cost is not the type but the places the type is *not*, a public shape following an
internal analysis, so that adding a `find` to a grammar would delete a class from a consumer's API.
The scaling test stays where it is, on the static positional calls, which is the shape a stranger
writes first. Reopened only if a consumer needs a reading it controls.

**The ISO grammar stays in the repository.** The file is kept as fetched, with its provenance and
the sentence ISO publishes it under; Microsoft's syntax stays under its own licence with the
attribution and the changes named, which is what that licence asks. The question was raised because
a public repository redistributes where that sentence speaks of use; it is weighed and answered.
Nothing follows for the packages, which never carried either file.

## D52. What the FIX message layer's bytes are, and the row that must come first, 2026-09-20

finance-24's account (`docs/design/fix-message-bytes-2026-09-20.md`, `a2aba2c6`). The nodes are 63%
of everything and are allocated in three places that are two mechanisms: one flat array of every
node, and a slice cut *per region* — header, body, trailer, and one for every entry of every group
— out of a list the reader grows from nothing on each message. So every node exists at least twice
and the list it is copied from is a third place. A node is two references and five integers, forty
bytes, so seventeen with a header is 704, which is exactly the measured figure for the flat array.

**The doubling was measured, not inferred from the type's name.** A ladder by field count puts the
cost per field at its lowest at 16, 32 and 64 — where a doubling list has just filled exactly — and
at its worst at 17, the step from sixteen fields to seventeen costing 1,456 bytes, which is a
thirty-two slot array with its header.

**The frame holds and the account applies it.** What is necessary is the per-region arrays: handing
out header, body and trailer as separate sets is what a message *is*, not scaffolding for building
one. What is not necessary is the intermediate flat array and the list. And the constraint any
change must answer before it starts: a region's nodes stop being a contiguous run of the flat array
as soon as there is a group, because entries are cut out and hung on their count — which is what
the list exists for. A message without groups could be sliced; FIX messages have groups.

**Ruled, and the third question is the one that governs.** No change lands on this document alone:
**the message layer has no row on the stand at all**, so we would be repairing something whose cost
we cannot state — the same mistake refused this morning in the field-level comparison. The row is
ordered first. Then the intermediate flat array may be designed away, writing straight into the
list the regions are cut from. And the reader's list living per thread rather than per message is
**not a separate question**: it is the retention boundary again, it holds references to fields, and
it joins the mechanism performance-ff is writing — a third user of it, under the same rule and
cleared at the same idle transition — rather than growing a policy of its own.

**D52's third user changes where the retention rule lives.** Asked whether a consumer in another
package could plug into it, performance-ff answers in two halves that go opposite ways. **Across
assemblies it is impossible, and not because of the mechanism**: nothing of ours ships at run time,
every support type is emitted into the consumer's own compilation and nested inside the generated
host class, so a stranger cannot reference them at all. If the third user were in another package
the answer would be no, and the remedy would not be a change to the mechanism but a change to what
ships, which is a far larger decision. **Within one assembly it is possible and the mechanism as
built would still refuse it**: the slot, the counter and the weak cell are written *inside* each
pool, by hand into four of them, so a sibling class could not reach any of it and would have to
grow its own copy of the policy — exactly what was forbidden.

**So the rule is emitted once, as one internal shape the four pools use and a sibling of the host
can use too**, parameterised by what it holds, with D34's quench belonging to the shape rather than
to each pool. Four hand-written copies cannot take a fifth user; one shape can. The approved rule
does not change — what moves is where it lives — and the design comes to the architect before it is
written, being larger than what was approved.

**One condition, because the shape becomes reachable by hand-written code in the consumer's own
assembly whether we mean it or not.** It is not documented and not offered: it carries no promise,
and our own message layer using it as a sibling is a risk taken inside this repository and pinned
by our own tests, not a surface a consumer is invited to write against. The day we want to offer
it, that is its own decision with its own reasoning.

**D52 said the message layer has no row on the stand at all; it has rows and no slope.** There are
paired rows for parsing and building a message, and a plain row added today under the external
reference. What exists nowhere is an input whose field count varies: the fields have a slope, the
message layer has none, and that is the axis the question lives on — a cost per field and the steps
of a doubling list cannot be seen at one size. So what was ordered is a slope, in the plain stand,
with allocation, those rows' times having arrived with a base wandering by 37% while the bytes are
exact. My sentence was wrong in a way that would have sent someone to build what is already there.

**And the sizes were chosen so that the result can refute the person who ordered it**: 4, 16, 17,
32, 33, 64, 65, 128, the pairs straddling each doubling. If the stand sees the same steps the probe
saw, they are a property of the code; if it does not, the probe is wrong and that must be known
before anyone repairs anything. A proof nobody else can re-check is a word rather than a proof,
which is the reason the sizes are in the order rather than in the account.

**One more thing worth keeping, about tone rather than content.** The constraint that explains why
the list exists goes first in the design not to save the next reader a day but so that they
understand the list is an answer to a problem and not a piece of carelessness — because that
decides whether they look for a third way or simply delete it.

## D53. Igor: FIX validation is a layer of its own, over a message already built, 2026-09-20

All validation is separated out. It runs over a message that has already been built — or is reached
as a method on the message itself, `Validate` — and it is not part of reading the wire and not part
of assembling the message. That settles the shape the dictionary study left open, and it settles it
the way the study's own line pointed: what is *read* and what is *checked* may be run-time tables,
while what is *constructed* — a class per tag, a factory arm, a message model — is build time,
because a type is not data.

**What follows, and what it makes easy.** A dictionary a counterparty hands a consumer becomes data
loaded at run time and consulted by the validating layer: required fields, code sets, types,
membership of a message, a group's count against its entries, conditional rules. None of it touches
the parse, which asks the schema exactly one thing — whether a tag begins a length/data pair — and
none of it touches building, whose cost D52 is measuring. We ship nobody's dictionary; a consumer
points at the file their counterparty gave them.

**And it lines the packages up with the comparison.** The external engine already has this shape:
its parse is one call and its validation against a dictionary is a separate one. So the pairing D26
takes at the message level compares like with like by construction rather than by argument.

**The questions the decision opens, to finance-24 as a design before code.** What becomes of the
strict and lenient modes, which today live in building and decide whether an unknown tag is
admitted — do they move out into the layer, leaving building unconditional, and if they stay, what
is left for them to do. What a consumer calls, and whether the entry is a method on the message or
a validator object holding the dictionary. What a failure is: a first refusal with a position, or
every finding, since a validating layer that stops at the first is of little use to somebody
reconciling a session. And whether validation may be asked for without a dictionary at all, against
the schema we already carry, which is what today's strict mode approximates.

**One principle, arrived at twice in an hour from opposite sides (performance-ff).** For the
measurement switch the question was whether a thing is reachable or merely promised, and there it
could be made *unreachable*: declaring the property where a consumer's build cannot set it turns a
promise into a fact held by the position of a line. For the emitted retention shape it cannot —
anything internal in their assembly is theirs to call, and no placement changes that — so the other
lever applies: withhold the promise. **Make it unreachable where you can, unpromised where you
cannot**, and the choice between the two is simply which is available. Neither is a weaker form of
the other; the first is enforced by the build, the second by nothing but our own restraint, which
is why it is written down.

And the shape gains a requirement from its third tenant rather than discovering one later: a list
growing with the number of fields and holding references to fields is bounded by a length rather
than by a sum of capacities, so the bound belongs to the tenant and not to the shape; and the
quench matters far more for it, a quiet thread otherwise holding a whole message's nodes instead of
some empty arrays. D34's seam living in the shape is what makes that automatic for a tenant that
has not thought about it.

## D54. D36 withdrawn by its own number, and a bias found in the form that withdrew it

Measured in the form that does not depend on the machine being quiet — six rounds, three arms taken
in turn, on the row where 107 of 194 walks merge: with merging 17,928 ns, without it 18,040, and
the same build as a third arm 18,040. **Merging is worth 0.6%, and so is the floor.** Zero within
the spread is what expr undertook to withdraw on, and the change is out (`6ebdbfa7`): the emitter's
edit, the tests that held it, and the merged call in the snapshot. What stays is the guard naming
two values in that snapshot, useful whether one walk or several serve it, and the design with the
numbers written into it so that the next attempt starts from them. The mechanism was real — 107 of
194 did merge — and the walks it removes are simply not where this reading spends its time.

**And the method finding is worth more than the result.** The first arm of a round is
systematically faster than the two that follow, and the second and third agree to the nanosecond.
So the form has a positional bias, and a floor taken as "the same build as a third arm" measures
the position rather than the floor. It was seen only because an untouched arm and a changed one
landed on one number. **The rule: the order of the arms is rotated between rounds.** This is a
property of the form, not of one measurement, so it reaches back: every number taken in this form
today is to be re-read with it, the clearing of the value store among them, where there was no
third arm at all and the figure attributed to the flags should be read again.

**D42 is closed (`9317a022`), and the second of its new grammars found the second shippable-broken
configuration.** The last six rows are covered by three grammars: locations as an option together
with a carrier the author names and a real symbol resolver, since a grammar naming the host's types
is only tested under one; several files out of one grammar with the sizes that divide them, both
parts compiled as one compilation, as they land for a consumer; and the reading over kinds with
transition tables turned off, which is the only way to show what they replace. The harness grew one
thing: a snapshot may bring declarations of the host, compiled beside the emitted file and resolved
over, without which the locations grammar would have named nothing.

**The defect.** With a location type set, a value the engine builds inline from ranges recorded by
their place receives no location at all — the arguments are the captured texts and nothing else —
while the factory is declared with a span as its first parameter, so the file does not compile. It
needs a location type and a rule whose value is built straight from characters rather than through
another rule. It never surfaced on a shipped grammar because the located T-SQL builds such values
as strings, and a string does not implement the location interface.

**Not fixed by its finder, rightly: a snapshot that does not compile is worse than none.** The
grammar avoids the branch, builds through a rule, and says in its own text why, so that the next
person does not "fix" it back. **It goes to performance-ff beside D44**, both being one question —
a configuration that emits code which does not compile — and one pass over that question is worth
more than two. The pass should say whether the two share a cause: both appear where a capture's
value has to be handed something the ordinary path supplies and the special path does not.

**The hypothesis the one pass will test, stated before it starts.** Both defects sit where **the
emitted call's argument list is built by one path and the factory's signature by another**, the two
agreeing only along the ordinary path: in one the signature says string fields while the argument
is a span of bytes, in the other the signature declares a location first and the argument list does
not have it. Different symptoms, one shape — two descriptions of a single call, produced
independently, checked against each other by nothing but the C# compiler in a consumer's build. If
that is the cause the fix is one place deriving the signature and the arguments together rather
than two guards, and the work after it is looking for the third such path, a fourth otherwise being
found by a consumer instead of by us. If they turn out not to share a cause that is said plainly,
because "one pass, one fix" is the tidy answer and tidiness is what should not be trusted here.

**And the second confirmation in a day of something worth keeping:** the case for a check is made
from the failures one can imagine, and those are not the ones it finds. Both of these were states
that had already shipped rather than changes that slipped in. The first argument for the snapshots
was that a change no diff shows is unheld; what they have actually caught twice is a configuration
that was already broken and had never been looked at.

## D55. The floor of the paired method is the runtime's profile, not the machine, 2026-09-20

stand fixed a positional asymmetry in the paired form — a round read forward and the next
backward, which put one side of a pair in the middle of every round and the other at the ends —
and the readings now rotate a place each round with the order reversing every n rounds, over a
multiple of 2n rounds, so each reading stands in each place equally often. Rows measured before
today were read the old way.

**It was not the main cause.** An A/A of one build after the fix still gives +1.5% to +3.4% on
several URL rows, with the sign holding in seven or eight runs of nine. With tiered profiling
turned off the same A/A falls to roughly zero on those rows. So the bias mostly goes with the
dynamic profile: the two sides of a pair are two loaded copies of one assembly, each compiled and
profiled by itself, and a copy's code can settle a few per cent away from its twin's. One
experiment stands behind that, and the result says so. It also matches what expr saw in three arms,
the second and third equal to the nanosecond and the first apart, if the first is the arm that
reads while the profile is still forming.

**What follows, and it is a bound on what this method can ever say.** An effect is read as its
excess over the A/A of the same rows in the same slot at the same profile setting — which is D50,
now earning itself rather than being a precaution. **A change worth a per cent or two on a row
whose A/A is three is not measurable by this method at all**, and no number of rounds fixes that,
because the two sides differ in what the runtime made of them. So a small change is argued from
something exact instead: bytes allocated, emitted size, a count of calls or of records. That is
what the last three days have in fact been doing, and it is now the rule rather than the habit.

The pair keeps running with profiling as a consumer has it; a run with it off is a cross-check to
take when an effect sits inside the A/A band, not the way pairs are taken. Making the two copies
compile alike is a stand change nobody has designed — loading each side twice and alternating the
copies is the one candidate named, and it doubles what is loaded.

## D56. A page's example is compiled as the page shows it, 2026-09-20

A hole in D37's mechanism, found by sql-39 reading rather than testing and confirmed by expr on its
own package: the tests compile an example inside a project with implicit usings, an environment
**more permissive than a consumer's**. A page showing a using for our own namespace and then a type
from the framework's compiles in the test and would not compile for someone who pasted it — and
our packages ship to the floor, where implicit usings do not exist at all.

**Decided: the pages are copied literally, so the test compiles a block as the page shows it** —
its own compilation, no implicit usings, with those usings and only those the block itself writes.
That is the only form that catches this class, and it catches it for every framework we ship to
rather than for the newest. Values go on being checked by tests written by hand; this checks that
what is written can be compiled at all.

**The blocks that are not compilation units are fragments, and D37 already says what a fragment
is.** A page showing one says what it omits, and the smallest complete form of it is held by a
test. So the helper compiles every block, and the exceptions are named **in the test, with a reason
each, not marked up in the page**: a shipped document carries no scaffolding for our tests. A block
that is neither compilable nor a declared fragment is a defect of the page, which is where this
started.

To expr, who found it and offered: the helper, and its own package's pages brought to it. Then the
same helper is pointed at the other three, whose owners fix what it finds. And the one-sentence
answer to the question that prompted it — whether a page may assume a modern SDK's defaults — is
no, and it is now written down rather than left for the next person to decide again.

**The second lever is measured and does not exist.** Counting a repeat as the same ask asked again
— the record's root together with the rule's mark — over the same named corpus: T-SQL has 201
repeats of 16,350 asks, 1.2%, with fourteen asks in the whole corpus asked more than twice;
SQL:2023's `select20` has none at all. So remembering an answer would save about one ask in eighty
on one grammar and nothing on the other. The question closes not on "cheaper or dearer" but on
"the phenomenon is not there". The bound is stated with it: an ask that returns under a different
root is not recognised by that key, so 1.2% is a floor — and with zero repeats on one grammar and
fourteen on the whole corpus of the other, there is little room for anything to hide in.

**That closes the whole line.** D29 refused; D36 explained structurally, built, and then withdrawn
by its own number; the discarded fifth accounted for by the price of asking; the second lever
measured and absent. Four questions, four answers, no code left behind except the snapshot grammar
that keeps a guard naming two values.

**D52's slope came back, and the arithmetic was registered before it.** The steps reproduced on the
stand's apparatus at the sizes and heights finance-24 had computed in advance: +656 against 664 at
the first doubling, then +1,304, +2,584 and +5,144, three of four exact to the byte. The fourth
disagreed and the person disagreed — a dropped array header in one line of arithmetic, which the
measurement remembered. So each step is one new array of doubled capacity and nothing else, the
line between steps is flat, and the doubling belongs to the code rather than to the probe, now
checked by somebody else's hands.

**And it settles which number answers this question at all.** In time the same ladder gives about
60 ns a field with a step of 85 ns — the price of one field — against a base wandering by 2 to 13%.
Bytes answer and time does not, which is D55's rule arriving from the other direction on the same
day. **The fix carries its own falsifiable criterion**: if holding the list works, the four steps
vanish and the line goes flat at 168 bytes a field. A change that removes a ladder of doublings has
to look like a vanished ladder.

**Ruled on the two designs.** The list's retention goes first, as a third tenant of the emitted
shape, with no policy of its own; the flat array is *not* attacked until that has landed and been
measured, because the flat array is an input and the list an output, counting first would have to
parse groups twice, and counting once and remembering is the list under another name. After the
first change the second may not be worth its complexity, which is a reason to wait rather than to
hurry.

**And on the validation layer.** A validator object rather than a method with no argument, since a
dictionary is loaded once and read many times and the only alternative place for it is a static —
which is what D25 keeps out of this package; a shared instance over the schema we compile in for
the common case. Every finding rather than the first, the reason being principled: parsing stops
because after an error the input's meaning is unknown, while a built message is fully known and
every rule can be asked independently. Of the three open questions: no validating overload on the
parse, because that is how a layer quietly becomes a mode again; a finding names the entry's index
in a repeating group as well as the tag, one being useless without the other; and conditional rules
stay out of scope while the finding's shape must carry a rule's identity and a path, so that adding
them later is not a change to everything that reads a finding.

**D56 is built (`0c0a219a`), with one refinement the rule needed and did not have: a later block
stands on what earlier blocks of the same page imported.** A page is read in order, and a reader
who copied the first block into a file does not copy its using again for the second; without that,
seven of eight blocks "failed" on a name the page had already imported, and the test would have
been lying rather than the page. Fragments are listed in the test with a reason each and not marked
in the page, as decided.

What it found in its author's own pages was real in every case: a call to a framework type with no
using for it, twice; a generic collection named without its namespace; and an example written as
two bare expressions, which C# does not accept as statements at all. Four different compilation
errors for anyone who copied the page. The rule has now found, in two packages, defects that the
previous form of the same rule had passed — because the previous form compiled examples in an
environment more permissive than a consumer's.

**The generator's own pages are closed (`3c541b86`), both halves.** Six grammar blocks are run
through the generator with no error and no warning and what each compiles into is built beside the
class the page declares around it; all six passed as they stood. The blocks that *call* what the
generator made cannot be compiled by the ordinary check, naming members no compiler has seen until
the grammar above them has been run, so they are compiled together with the page's class, the
generated file and the marker attributes — and there four of five did not copy: three showed a
result as a bare expression, which C# does not accept as a statement, and one parsed a name the
page never declared. Each now binds what it shows and states the answer in a comment, which is what
the page meant.

**The helper learned two things on the way, and both are general**: a block stands on the usings of
earlier blocks of its page, and it does not receive them again where it writes its own, since a
duplicate-using warning reads as a defect of the page when it is a defect of the check.

**And it now has three sessions editing it.** One extended it for two packages while another was
working in it; the second merged rather than overwrote, and kept the other's list as it was. That
is the right handling, and it makes the helper a shared contract: what it accepts and refuses is a
rule about pages, so a change to that is a change to the rule, not a private convenience. It is
linked by source into each test project rather than given a project of its own, which keeps one
copy and no drift.

## D57. Two shipped parsers took seconds and minutes to refuse, and both were found by luck

finance-24 has fixed a refusal in the URI-template reader — a repetition whose body is a repetition
over overlapping first sets, `(A+)*`, doubling the cuts with every character: eight characters
0.03 ms, twenty-four characters 3.2 seconds — and, looking for more, found a second in the email
address list: twenty-eight characters, **one hundred and seven seconds**. Both are atomic now
(`b3bdc01f`). Two shipped parsers, both exponential on a refusal, neither found by a test or a
review: the first surfaced because a stand row happened to carry an unclosed template, the second
because somebody went looking after it.

**The asymmetry is the argument.** We spend measurement windows on twenty-nanosecond constants
while two shipped parsers took seconds and minutes to say no.

**Three things follow, in this order.** First, before the release, a throwaway probe over every
grammar here listing the sites of that shape — a repetition inside a repetition, first sets
overlapping, nothing committing between them. The analyses exist: one already asks whether a
repetition can be made to give a turn back, another computes exactly the overlap that makes the
shape ambiguous, and the fix in both cases was the atomic group a third already reasons about. An
empty list closes the release question; a non-empty one is handed to each grammar's owner.

Second, the guard that would have caught them: the linearity runs cover accepted input, and a
refusal is where this shape explodes. Refusing inputs belong in that check across the packages.

Third, the diagnostic — **the generator can see this shape and does not say so** — which is the
same class as the two configurations that ship and do not compile, except that this one compiles
and then stops responding. It sits above the speed items, it comes as a design before any code,
and it waits for an undivided head, because a warning that fires on a correct grammar is worse
than none and that judgement is the whole of the work.

**D57's second step, designed: a refused ladder for every family that has an accepted one** — the
same head and a tail the reader cannot finish, plus the shapes the two defects actually had, a long
run of atoms before the bad character and a long quoted or bracketed construct left open. Coverage
is every web format, FIX, the feed, both SQL dialects and the expression language, and a series
whose input turns out to be *accepted* is reported as a fault of the series rather than counted as
a pass — a series that quietly stops refusing stops checking what it was written for.

**And the budget is the part worth keeping: a guard that needs twenty minutes to say "exponential"
reports nothing.** Sizes grow by a quarter rather than doubling, so a shape that doubles per
character rises sixteenfold a step instead of sixty-five thousandfold; a call is run once before it
is timed; a ladder stops at the first call over twenty milliseconds and prints the size, the time
and the word. The worst call any series makes is under two seconds, against the hundred and seven
that started this. A check against explosions must find the explosion faster than the explosion
eats the check.

**One addition: that is an audit, and what is wanted is a guard.** It answers whether there is a
defect today; the two that shipped survived precisely because nobody ran anything. So after the
first pass a trimmed version — the same series, fewer points, the same budget — belongs where the
build runs it. If it costs too much, cut the points and not the families: a coarse curve over every
entry point beats an exact one over three.

And the list of entry points that could *not* be given a refused ladder is half the result, not an
appendix: it is exactly what the static probe has to cover, and the two are read together.

**D57's probe: the list is not empty, and the probe was wrong twice before it was right.** Its first
version found *neither* known case and named two harmless ones: it looked at the outer repetition's
syntactic body, while the shape that actually shipped is two calls away and behind a choice, and
meanwhile it named two runs pinned by a digit that cut one way. **What caught it was asking whether
it finds what we already know is there** — with the fix absent from that tree, both known cases had
to appear and neither did. Had the first list been reported, "two places, both in one package"
would have closed the release question falsely. The corrected predicate follows calls and choice
alternatives to every way a turn can begin and requires that what follows the inner repetition can
read nothing; it finds both known cases, which is the only reason the rest of the list is worth
anything.

**The rule that comes out of it: a detector is calibrated against known positives before its output
is believed**, and a clean result from an uncalibrated detector is the most expensive kind of
answer, because it ends the search.

**Eighteen places over 113 grammars, as places and not as defects.** Two areas are the known cases,
already atomic on main. Of the rest the largest cluster is **ten in the expression language** — the
string, verbatim and interpolation bodies — which is a shipped package reachable from any consumer
parsing an expression someone else wrote; and three singles in examples, which are copied by
readers and so are not exempt.

**Triage by driving rather than by analysis**, which is cheap and answers without judgement: each
owner takes the shape's own worst input against their own parser, and a refusal that takes a second
answers the question, while one that does not is also an answer. The probe is kept rather than
thrown away: it has earned it by finding the known cases, it will be run again after each fix to
watch the list shrink, and step three's analysis is the same walk with the judgement added.

**A third instance of the shape, and the triage method earned itself in a minute.** Of the four
places the probe named in the address grammar, two had been fixed and two had not. The unfixed one
repeats over folding whitespace — a run of folded space inside angle brackets — and doubles about
every one and a half characters: twelve characters 3.2 ms, sixteen 48.8, twenty 188.9. Both folding
rules are atomic now, the obsolete and the strict, for the same reason atoms are: a space cut in
half is the same space. 189 ms became nothing.

**And the part that could not have come from reading.** Four inputs were tried into that rule —
spaces, commas, a mixture, and repeated routes — and exactly *one* was slow; the other three are
flat at every length. Choosing an input by eye, the commas look like the more obvious way in, and
the conclusion would have been that the shape is not reachable. **Driving inputs is cheaper than
reading not only in time: it also takes away the chance to pick the convenient one.**

**Reverse calibration is owed and is being taken from two directions.** The probe should stop
seeing the four now that they are fixed — its owner runs it, not the person who fixed them — and
beside it a guard of a different nature: the package's own slow-suite check now holds all three
shapes and was verified to fail on the unfixed grammars in 4.6 seconds, naming the shape and the
remedy. Two checks of different kinds against one class beat one.

**And the sentence worth keeping from the session that found the second and third:** it went
looking not out of diligence but because the first was found by someone else, which meant it did
not know how many there were. **When a defect is found by somebody else, the honest conclusion is
that one's own review did not see it, and the answer is to measure rather than to reason.**

**The reverse calibration did not come back clean, which is the useful outcome.** Eighteen places
became fifteen: three went, not five. Gone as expected is the template rule, and one of the two
entries each for the phrase and the route — the list held each twice because each rule reaches the
shape by two different paths, which the first report called "four places" without noticing.

**What remains are the other paths, and they are not what was fixed.** The fix sealed the run of
*folding whitespace*, which is the input measured at 189 ms for twenty spaces. The walk still sees
a path through an unbounded run of **comments** inside the same outer repetitions, with only a
nullable piece after it: structurally the same ambiguity over a different repeated thing. So
neither guess was right — the predicate is not seeing a shape the seal fails to remove, and the fix
is not wrong; **the fix is partial**, sealing one of two ways into one shape, and the second way
was never driven because the input that drives it is a run of comments rather than of spaces.

**That is the triage finding arriving from the other side**: there, one input of four was slow;
here, one path of two is sealed, and what tells them apart is an input rather than a reading. The
input to try is named — a refused address whose phrase or route carries a long run of comments, at
twelve, sixteen and twenty, the same ladder that showed the doubling for spaces. If it doubles, the
comment rule wants the same seal; if it does not, this becomes the first entry in the list judged
safe rather than assumed safe.

**And the probe now has calibration in both directions** — it finds a known presence and it saw a
known absence — and earned its keep a third time by showing that a fix removed three entries and
not five, which nobody would have checked by hand.

**The second path is closed, and it repeated the lesson literally rather than by analogy.** The
named input — a route carrying a run of comments — gives 6.6 ms at twelve, 96.5 at sixteen, 310.6
at twenty, doubling about every one and a half comments; sealed, it is 0.01 at every length. And
again **one input of four**: the same run was tried into the phrase, the route, the local part and
the domain, and only the route was slow, the phrase staying flat at 0.13 ms for twenty. Choosing by
head, the phrase is the obvious one — a display name stuffed with comments — and comments in a
route look exotic. Twice now the slow one has not been the likely-looking one, so the chance of
picking the right input by reasoning is not "fair": in two cases out of two it would have been zero.

**A rule we did not have, proposed by the session that needed it, and taken.** When a change is
confirmed by *the same input that found the defect*, what is confirmed is that this input is no
longer slow — not that the shape is gone. Those are different claims. To check the second, either
an instrument of a different nature is needed, which is what the static probe was here, or an input
found by a different route than the first. That is exactly why yesterday's work ended in honest
confidence: an input was measured, found, fixed, and re-measured flat, and every step confirmed
success while none of them asked how many paths lead to the shape.

**Calibration on absence passes, and the list is five.** Both rules of the address grammar and the
template rule are gone from the walk entirely, both paths of each; the probe now has all three
calibrations — it found the known cases while they were there, and it stopped naming each as it was
sealed. Meanwhile expr sealed eight of the expression language's ten in parallel, so what remains is
one rule in an allocation example, two in the expression language (`Text` and `Verbatim`), and one
each in the markdown and xml examples.

**And the two remaining expression-language entries are the question, not the remainder.** Eight
went and two did not: that is either judgement — they were driven, found flat and left deliberately
— or it is eight done and two not yet, and **a list cannot tell the difference**. The day's own
lesson is exactly that gap: a seal closed one path of two and the second went unnoticed because
nobody asked whether the fix was complete. So it is a question to their author rather than an
assumption either way, and if they were judged, they become the first entries recorded as *judged
safe* rather than merely listed.

**The probe's job description, revised by the rule it was given.** Its obvious use is finding
places; its more valuable one is confirming absences, because that is the claim nobody else in the
process can make — a ladder says "this input is fast now" and a walk says "the shape is not there
any more", and neither sentence implies the other.

**And step three's shape is settled by the same evidence:** twice out of twice the slow input was
not the likely-looking one, so the diagnostic says "here is the form, drive it" and does not
pronounce on whether a given site can be driven.

**All ten of the expression language's places are settled by driving, and four of them were real.**
The ones that exploded are the string starts the automaton only *begins* while a rule finishes
them: twenty-four characters took 20.6 seconds on one, 10.2, 10.8 and 6.8 on the others, each
character doubling. Sealed at the run of text inside a piece — the run stops at the first character
another alternative or the closing quote can read, so the longest reading is the only correct one
and there is nothing to give back — and the same inputs are now fractions of a millisecond, as is a
hundred thousand characters.

**The six that did not explode have one cause between them, and it is the criterion step three has
been missing.** `Text` and `Verbatim` are *whole lexemes*, read by the lexer's automaton, which has
no way back. That is the same shape as the four that exploded and is exactly what separates them —
so the question a diagnostic must ask is not only "is this shape here" but "who reads it": an
automaton that cannot give anything back, or a rule that can. The rest are repetitions of a single
character, where there is no text to cut, or alternatives that cannot read the same text.

**Judged safe rather than listed**, with the reason written into the grammar itself so the next
reader does not re-derive it. These are the first entries of the list to reach that state.

**And the permanent test's threshold is chosen the right way**: five seconds, a hundred times what
the fix leaves and a ten-millionth of what the defect cost, so a loaded machine cannot fail it and
a returning defect cannot pass it. A guard's threshold should be argued from both ends like that,
not picked.

The rule about confirming with the same input is taken into the follow-up work: the four sealed
places will be driven with several shapes of input rather than the one that found them.

**And the criterion reshapes the diagnostic from "truthful but tiresome" to "plausibly silent".** Of
the ten places in one package, six fall to things the generator already knows: four to the lexical
split — a whole lexeme is read by an automaton that has no way back — and two to analyses already
carried, a repetition of a single character having nothing to cut and alternatives that cannot read
the same text being first-set disjointness. False positives had been treated as the risk that might
sink the item, with a warning honest enough to live with noise as the plan. If most safe places are
filterable by what is already computed, **a warning that is silent where it should be is a
different proposition from one that is right but tiresome.**

So the design asks four questions in order — is the form here, who reads it, is there anything to
cut, can the alternatives read the same text — and only what survives all four is named; and what
is named is still "here is the form, drive it", because twice out of twice a verdict from reading
would have been wrong.

**On suppression, a position worth recording as a standard rather than as a preference:** the
analysis should see the automaton itself rather than give the author something to say back. An
author annotating a safe place is paying for our imprecision, and the reason for one such place is
already written in a grammar — if the diagnostic fired there anyway we would have turned a comment
into an obligation. An author-facing escape is the fallback that means we failed, not the plan.

**The rule paid for itself within the hour, and what it found was worse than what it confirmed.**
Driving the four sealed places with twelve shapes of input instead of the one that found them, all
were flat but one: a repeated escaped brace, twenty-four groups taking **62 seconds against the
20.6 of the defect just fixed**. Reading would have closed it.

**And its cause is a different class.** The atomic run has nothing to do with it: the text has two
readings — two escaped braces around a run, or a brace opening a hole — so two readings a group and
two to the n over n groups. The exponent came from ambiguity *between alternatives*, not inside a
run, and it is fixed by telling the hole it cannot begin with a brace, which takes nothing away
because no C# expression begins with one. **So the probe is not complete**: a walk looking for a
repetition inside a repetition would never have named this, and step three's design must account
for a second source rather than assume the first is the shape.

**The three example sites were all drivable, and the warning about the unlikely input held three
more times**: a document that cannot end, an unclosed element, a run of digits followed by a letter
— twelve, thirty-five and seventeen seconds — while the neighbouring shapes that look at least as
suspicious stayed flat.

**Two things left open by it.** An atomic group written around a capture yields text where the
capture wanted a list, and the consumer learns this from a C# conversion error rather than from us
— which is the class of the two uncompilable configurations again: our diagnosis delivered by
somebody else's compiler. And the allocation benchmark's fix has no permanent guard, benchmarks
having no test project; rather than copy the grammar into a test, the answer is to make the static
probe itself the guard — a test asserting that its list holds nothing but the entries judged safe,
each with its reason — which covers the benchmarks, the examples and every future grammar at once.

**The two sources turn out to be one question asked twice.** "Alternatives that cannot read the
same text" was one of the safety filters that made the diagnostic plausibly quiet; its *negation*,
inside a repeated group, is the second source of the exponent. So disjointness is asked once and
read in both directions — where it holds the place is safe, where it fails inside a repetition the
place is a candidate of the second kind — and the second source needs no analysis we do not have.
The scope claim that six of ten fall to existing filters is withdrawn by its author as a statement
about the item: it is true of the first source and silent about the second.

**And the guard takes its final shape: the file records a place, a verdict and a reason, and
*unjudged* is a failing state rather than a line.** A tool that records places becomes one that
refuses unjudged ones, which is what the process actually needs; and a walk over all the grammars
guards the originals where a copy in a test would have guarded the copy. Today's five defects were
found because somebody looked; the guard is what looks when nobody does.

The capture-inside-an-atomic-group question is deferred into the pass over the two uncompilable
configurations, with a prior and its evidence stated rather than a verdict: the specification's own
streaming proof accepts a collection inside atomic groups, which reads as the language not treating
an atomic wrapper as turning a collection into text. If it is intended it is a third instance of
that family — our diagnosis delivered by somebody else's compiler — and a third instance is itself
evidence about the cause.

## D58. A generated reading can kill the process, and the contract does not survive it

Driving the expression language with sixteen ordinary shapes found no more exponents and two
defects of another class.

**The severe one: depth kills the process.** A lambda nested three hundred deep — two thousand
characters — overflows the stack, and a stack overflow in .NET is not catchable: not a refusal, not
an exception, the process dies. So on that input the promise that a try-parse refuses rather than
throws does not hold at all. The recursion is visible in the trace, twelve frames a level.

**And the numbers do not behave as recursion should.** In a debug build two hundred levels pass and
three hundred die; in a release build fifty-one thousand pass, and two hundred thousand pass in 86
seconds without dying, on a megabyte stack and on a quarter of one alike. If depth grew with
nesting the release build would die in the low thousands. It does not die at all — so under
optimisation the stack does not grow with the input and in a debug build it does, from one emitted
reading. **That difference is the first thing to settle, because it decides the severity**: if a
release build is genuinely bounded, the defect is that a consumer's debug build dies on two
thousand characters, which is serious; if it is bounded by accident, this is a way to kill a
shipped process with untrusted input, and it goes to the head of everything.

**The lesser one: a quadratic, not an exponent.** A chain of members at four hundred, eight hundred
and sixteen hundred links takes 124, 367 and 1,527 ms — four times for twice — and depth behaves the
same way. Polynomial, and still a slow denial of service on untrusted input. It is a separate
question from D57 and gets its own decision if it wants one.

**What the answer will have to be, when the investigation names the cause:** a stated bound on depth
that *refuses* rather than dies. That is a promise in the generated API and therefore Igor's, and it
is not worth putting to him before the debug-release difference is understood. Everything else the
sixteen shapes touched was flat under a millisecond.

**D58 answered, and it is neither possibility: the release build is bounded deliberately, by a
mechanism of ours, whose margin nobody measured.** Twenty thousand levels pass on a 128 KB stack,
which no growing stack could do; the listing shows an ordinary call and no tail call, so the JIT is
not what flattens it. What flattens it is the emitted reader's own stack guard: it asks the runtime
whether enough stack remains and, when the answer is no, moves the reading onto a fresh thread with
a large stack and carries on, bounded by the option that says how many such hops are allowed.

**The defect is the spacing, not the mechanism.** The probe is throttled to once every sixty-four
levels, which at twelve frames a level is seven hundred and sixty-eight frames between checks.
Between two probes the reading must fit inside whatever margin the runtime's check guarantees — a
fixed margin against frames that are not fixed. Optimised frames fit; a debug build's frames, with
every local in memory, do not, and the stack is gone before the next probe asks. **We hold a
promise by an unmeasured margin, and we would not know if it stopped holding** — the same frames
could overflow an optimised build on another target without anything of ours changing.

**Ruled: remove the constant rather than re-derive it.** Measure what probing every level costs; if
it is small, probe every level and the calibration question disappears instead of being answered
with a better number. A number that is right until a frame grows is the shape of this defect, and a
second such number would be the same defect with a longer fuse. Only if probing every level is
genuinely expensive does the spacing get derived — from the measured margin and the measured frame,
not chosen.

**And one fact has to be known before Igor is asked for a declared depth limit:** where the input is
a window, the hop is not available at all and the reading throws instead, so for the streaming form
the bound is the caller's stack and nothing else. **A declared limit that cannot be honoured in one
mode is worse than no limit**, so the streaming answer comes with the proposal rather than after it.

**Probing every level costs nothing measurable, so the constant is deleted rather than re-derived.**
Three runs each, generator rebuilt between: at five thousand levels 176.5 ms against 178.3, at
twenty thousand 776.4 against 775.8, the two arrangements inside each other's spread at both sizes
and the every-level median faster at the larger one. Sixty-four probes become one and the reading
does not notice. The author says plainly what the experiment is not — one grammar, one shape, one
machine, not a pair — and that the decision it supports is not a number but "is this affordable",
which a difference invisible in three runs at two sizes does not answer with a magic constant.

**Landed without a window, and verified by the next baseline rather than by a pair of its own.** The
rule that emitted-code changes land with a pair exists to catch a regression the author did not
expect; here an in-process comparison at two sizes shows none, the change is a correctness fix, and
the retention pair is worth the window more. The expression-language rows ride the next baseline,
and if they moved we come back. **What the change removes is the class, not the number**: there is
no spacing left to be wrong when a frame grows.

**And the practice is worth naming: the constant was flipped to measure, put back, and the file
checked byte-for-byte rather than trusted.** An edit undone by hand is an edit until something says
otherwise.

The streaming half stays open and goes to Igor *with* the proposal: the probe on every level for
the in-memory readings, and an honest statement of what a streamed reading can promise, so that a
declared limit is not offered in the one mode where it cannot be kept.

## D59. Four corrections, one of them to a permission I had already given, 2026-09-20

**1. D58's number is withdrawn and my permission to land on it goes with it.** Two runs produced
output that looked like an answer: one died partway and printed only the baseline; the other
printed both sides while the "every level" side was *the baseline binary measured twice*, its build
having failed on a file lock left by the first run. **The tell is worth keeping: all eighteen rows
came out one to three per cent faster, uniformly — the signature of a warm second pass of one
binary rather than of a change.** A uniform small gain across every row is evidence of a harness
fault before it is evidence of an improvement. The class is fixed rather than the instance: a
failed build now aborts the run, and the flip must be visible in the *generated parser* before any
row is measured — a check which immediately caught a third error, the probe being looked for in the
wrong project's output. So the claim that probing every level is free rests on one shape only, and
nothing lands until the corrected run. I approved landing without a window on those numbers; that
approval is withdrawn with them.

**2. The chain of members is not a quadratic, and my filing of it was wrong.** Measured: 2.6 s at
two thousand, and at eight thousand it burned nineteen minutes of processor without finishing — a
fourfold input for at least four hundred and thirty-eight times the work, which is nowhere near
quadratic. It is also a repeated chain on a parse that *refuses*, which is the shape of D57's
second source. **It is run as a calibration case against the committed probe before step three**: if
the probe does not name a case we have in hand, the diagnostic would ship blind to it.

**3. A real retention defect, found while writing the design rather than by it.** In the emitted
return, the branch for a store past the bound ends before the clearing loop, so an outsized value
store is never cleared: it sits in a strong slot for eight parses and in a weak reference after,
holding every value the last parse built. Severity by pool, stated precisely rather than uniformly:
real for the two whose tables hold constructed values; **not** for the one whose arrays are
integers, where pinning numbers is the memory we meant to keep; unknown for the parser's arena
until it is read. **This makes D34's emptying a fix rather than a tidying**, and it names what the
operation actually is: drop the contents, keep the capacity — which is neither of the two
operations that exist, since the return's clearing and the demotion each keep both together.

**4. And a revision of my own ruling: unify the rule, not the text.** I had decided the retention
rule should be emitted once as one shape all four pools and a sibling could use. Three of the four
already go through the emitter's shared helpers; the other two are hand-written copies of the same
forty-line shape inside a literal block, differing in a prefix, a bound, an expression and a reset.
Unifying that text means turning two readable literal blocks into append calls, which is exactly
what the conventions' handwritten-and-emitted distinction exists to prevent. **The drift we fear is
behavioural, and a test catches behavioural drift**: the scaling tests are extended to cover all
four pools, so a copy that loses the ladder fails rather than passes a review. The condition that
came with D34 survives in a better form — the fifth tenant does not get a policy of its own, it
gets the same test.

## D60. The refusal audit: ninety-one series, and about twenty quadratics in shipped code

The first audit ran in one process in 67 seconds and is calibrated as D57 requires: its build
*predates* the two known exponential fixes, and it finds both — a literal run then an unclosed
brace at exponent 6.27, an atom run then a bare at-sign at 5.96. A rerun on main will show them
clear, and **until that rerun no result touching those two grammars is a statement about main.**

**The finding is larger than the exponentials.** About twenty refusals in shipped web and SQL code
are quadratic — media types, structured fields, web links, language tags, content dispositions, all
at exponents between 1.9 and 2.1, where three to nine kilobytes already cost twenty to thirty
milliseconds and sixty-four kilobytes projects to seconds. The contrast is measured rather than
argued: an *accepted* structured list of ten thousand items reads in 1.2 ms, while a *refused* one
of eleven hundred takes 23. Same head; what differs is the refusal after it. It appears only where
the head is a repeated unit, and a single unclosed string is linear in every format. Most of the
ninety-one are linear.

**A conflict to settle before either number is believed.** The audit reports the expression
language's interpolated string still exponential, at 4.94, and calls it unfixed on main; its author
measured the same shape at fractions of a millisecond after sealing it. Same claim, two
measurements — so the rerun on main decides, and neither figure is quoted until it does.

**The guard is approved as proposed, all three parts.** The series table and the ladder move into
one file with no dependency on the stand, linked into both the stand and the slow suite, so the
audit and the guard cannot drift. The guard compares each series against a checked-in baseline of
classes and fails when a series is *worse* than its baseline or stops being refused, reporting when
it is better, so the baseline only ever tightens. A new series enters at its measured class, and
**a class above quadratic may not enter the baseline at all**: an explosion cannot be grandfathered.
At fifteen to twenty seconds trimmed it goes in whole.

The half that is not covered is printed with the table rather than omitted — fixed-length formats,
a lenient one whose every tail is accepted, the reader and span forms, several entry points, the
examples and the hand parsers — and that list is what the static probe has to answer for.

**The conflict narrows to a build's revision, not to an input.** The two inputs are word for word
the same, and on current code all three shapes — including a third with a backslash — stand at ten
and a half milliseconds from eight characters to two thousand, a figure that does not move with
length at all because it is the first call in a fresh process rather than the reading. The audit's
ladder stopped at eighteen characters on thirty-six milliseconds. So there is no uncovered shape,
and what remains is the build.

**And the reason it is worth a rule: three fixes landed inside twenty-four minutes, so a binary
built at 08:40 carries the first and not the third, and "the tree that morning" does not
distinguish them.** Our rule that a verification names its revision was written for reading source;
it applies to a measured binary in exactly the same way, and more sharply, because a binary carries
no visible date. A measurement names the commit its binary was built from.

The author's position on the outcome is the right one and is recorded as such: if the audit's build
turns out to be the newer one and the ladder still climbs, their correctness is refuted rather than
debatable, and they have asked for the input byte for byte including what precedes it. Meanwhile
the audit's linear results for the ordinary string, the verbatim one, the character literal and the
interpolation with holes reach by a different road and a different harness exactly the places their
own driving had judged safe — which is better confirmation than repeating one's own measurement.

**The conflict is closed, and both measurements were right for their own revisions.** Read from the
assemblies' product version and an ancestry check rather than from memory: the audit ran on a
commit of 07:32 of which none of the day's four fixes is an ancestor. So every number in it — not
only the web ones — precedes every D57 seal, the flat interpolation ladder and the exponent of 4.9
are each true of their own build, and neither party was wrong about anything but the other's
revision. **That is what naming a binary's commit buys: a disagreement that looked like a
contradiction turns out to be two facts.**

What the rerun will change is therefore the two web grammars and the expression language's string
bodies; the quadratic findings live in files no seal touched and are expected to survive — an
expectation stated as one, with the rerun to speak for itself. The baseline the guard starts from
is generated from *that* run and not from this one, which is the right way round: a baseline built
on a stale binary would grandfather what has already been fixed.

## D61. Nested parentheses cost a cube, on accepted input too, and it may be one cause for twenty

sql-39 took the stand's row about unclosed parentheses apart and found it is not a property of
refusal at all. On T-SQL's search condition: refused, 64 levels 1.1 ms, 128 9.1, 300 125; **accepted
— closed parentheses — 64 levels 2.7 ms, 128 19.2, 300 65.** Both curves about n^2.8.

**The cause is counted rather than timed**, which is why it is worth trusting: at 32 levels three
rules are each called 1,056 times, and 1,056 is 32 × 33 — exactly the square of the depth. At every
opening parenthesis the reader asks whether this is a subquery or a parenthesised value, and the
attempt re-reads the whole remaining nesting. The ambiguity is the language's; **the square is
ours** — an ordered choice with no memory of failure re-reads what it has already read.

**So the consequence is the D57 family's, not a new one:** nothing at ordinary depths, seconds at
two thousand levels, and on untrusted input a slow denial of service. The difference is that it
costs on *correct* text as well, which the exponentials did not.

**Not started as a project, and deliberately.** The stand's audit has just found about twenty
quadratic refusals across the web and SQL packages, and the obvious question is whether they share
this cause — an ordered choice re-reading a stretch it has already read — in which case one
reader-level answer addresses all of them and a grammar-level answer addresses none. That is a
hypothesis with a cheap test: count calls on two or three of the twenty and see whether the same
square appears. **It is asked once, for the class, after the rerun on main** — not twenty times.

The level is the reader's, as sql-39 says: fixing the grammar would cost fidelity, which D22
forbids. And the remedy, if the class is one, is narrower than the packrat that was refused
earlier: remembering that a rule *failed* at a position is not memoizing everything, and the
objection recorded against full memoization — that most refusals are reached on the first token —
does not apply to a failure remembered after a long re-read.

**The retention defect is fixed by reordering rather than by a seam (`c786d3e8`, verification
pending).** Both value stores empty before the bound is tested, so a parked store is already empty
and the demotion has nothing to do — which means D34's emptying method would have had one caller
and no second meaning. **The fifth tenant inherits the order, not a method**, and the order is held
by a test: a value of a finished parse of twenty thousand addresses survived three forced
collections before the change and does not after, while below the bound it never did. That test
asks whether an object is reachable rather than how long anything took, so it is indifferent to
what else is on the machine — the only thing measured today that is.

**And the sentence the code was violating was written directly above it:** every retained reference
is cleared before the store is made available to another parse. The early return was an exception
nobody had written down, and the comment had been true when it was written. The two pools whose
arrays hold integers are deliberately untouched, read rather than assumed.

**On whether the probe misses the interpolated strings: not chargeable, and said before the idea
spread.** The audit's build is an ancestor of the guard's commit, so its expression language
predates the sealing the guard records; the question is open until the rerun rather than answered
either way, and ancestry was checked rather than inferred from clocks.

**The larger point, which belongs before the diagnostic's design and not after it: the probe finds
exponentials and says nothing about quadratics**, and the audit's twenty project from 1.2 seconds
to over a day at sixty-four kilobytes. A diagnostic that names exponential shapes and is silent
about a refusal that takes a day would be read as a clean bill of health it was never asked to
give.

**Ruled: D61's cheap test comes before any widening.** Count calls on two or three of the twenty and
see whether the same square appears. If it does, the class is a property of the reader — an ordered
choice re-reading what it has read — and the answer is one change to the reader, after which there
is nothing for a diagnostic to warn about. If they turn out to be heterogeneous, the question of a
polynomial diagnostic reopens with evidence. **Either way the diagnostic's own text says what it
does not cover**, so that its silence is never read as a verdict; that costs a sentence and is not
contingent on anything.

**The class test is set up to be able to fail.** The three cases counted are chosen to be
structurally *unlike* nested parentheses rather than three spellings of it: a character-level
refusal with no nesting at all, a flat list of items, and a parameter list. If all three land on
the same arithmetic — a rule called depth times depth plus one — then three unlike shapes have one
cause and the class belongs to the reader; if one does not, that one is the case to drive, and it
will be driven rather than reasoned about. The instrument is asked for from the session that
produced the 1,056 rather than rebuilt, so that a disagreement cannot be about the method.

**And a rule that widens beyond the diagnostic: what a measurement cannot see belongs beside what
it says.** The carriers report names a grammar's worst machine and not all of them; a refusal
ladder sees one carrier and one entry point; the audit's own page lists the starts it does not
reach. Writing what a diagnostic does not cover is the same discipline in a third place. It costs a
sentence, it depends on nothing, and its absence is what turns a narrow instrument into a
misplaced clean bill of health.

**A correction made while it was cheap:** the twenty quadratics were described as "in web and SQL"
rounded together, when the audit keeps them in separate sections and the parenthesis case comes
from the SQL one. The three being counted are the web's, and whether SQL's behave alike is part of
what the count answers rather than something already assumed.

## D62. The rerun on main: no explosions left, sixteen quadratics, and a class to settle

The audit reran on a binary built from a named commit, on the timing cores with nothing beside it,
101 series in 81 seconds. **Nothing explosive, nothing accepted by mistake, nothing thrown: 78
linear, 7 superlinear, 16 quadratic.** Calibration holds in both directions — the two known
exponentials are gone, and the expression language's interpolated string is flat in both readings
across all the forms its author asked for, so three independent sources now agree with the zero
they measured. One refusal that was exponential before the second seal is now quadratic, which is a
seal working rather than a seal missing.

**The expectation named before the numbers is confirmed**: the quadratics live in files no seal
touched. Projected at sixty-four kilobytes, most cost seconds — five to nine for several web
formats, thirty-eight for an unclosed quoted string in an address list, and the worst is T-SQL's
nested search condition.

**And the nesting rows are renamed, because they are not about refusal at all**: the closed
parentheses cost the same, so that is the price of nesting and its accepted twin now sits in the
accepted ladders. A row that measures something other than its name is worse than a missing row.

**The guard is written and is not pushed until it has compiled and run green** — a slow-suite
project that has never compiled must not reach the branch — and its baseline is written from this
run rather than the stale one. Every report now names the commit its binary was built from.

**The twenty-three flagged series go to their owners as information, not as work.** D61's question
governs: whether one cause — an ordered choice re-reading what it has read — accounts for shapes as
unlike as a character-level refusal, a flat list and a parameter list. Counting three web cases and
one SQL case answers it, and until it does, fixing them one at a time risks twenty separate
remedies for one defect.

**Two cheap facts that narrow the class count before it finishes** (finance-24, five minutes,
because the grammars are theirs and they can say what is *not* in them).

**The worst projection has no grammatical ambiguity at all.** An unclosed quoted string is a run of
one character, and in that rule the optional folding whitespace never matches while the content is
exactly one character — so there is one reading and nothing to cut. Whatever the square is there,
it cannot be a property of the grammar.

**And the quiet path is quadratic too.** The same input read without recording anything: 5.70,
22.56 and 91.55 ms at one, two and four thousand characters — exactly four times for twice, a clean
square — against 30.67, 113.40 and 239.65 recording. **So building the expectation is not the
cause**; it multiplies the refusal by about four and sits on top of a square that lives in the
reading.

**The hypothesis, marked as one:** a greedy repetition that cannot finish gives back what it ate
one element at a time and retries the continuation, which is n attempts — linear if giving one back
costs a constant, quadratic if it costs what has already been eaten. That is testable by the count
already under way.

**And the pair to count first, which is the sharpest thing in the report:** the address list's
unclosed quote is a run of *one character*, while the media type's is a repeated *unit*, and the
audit gives them the same exponent. If one arithmetic covers both, that is strong evidence for the
reader; if they differ, the list holds two diseases under one number and they must be cured apart.
Measuring that pair first decides more than any other pair would.

## D63. One illness, in the reader: the quadratics are a re-read prefix, 2026-09-20

Three shapes, chosen to be as unlike one another as the list allowed — a run of one character, a
repeating unit, a flat list — counted on one instrument at two sizes each. Every one lands exactly
on the triangular number with a small integer multiplier: three times it, twice it plus n+1, and it
plus one. **The multipliers differ because the grammars put different numbers of reader methods on
the re-read path; the function is the same, which is what the question asked.** The qualification
is given with the result: total call ratios drift because totals carry linear terms too, so the
exact figures are the per-rule ones, and two sizes were taken because one size cannot tell a
triangular number from any other quadratic.

**And the hypothesis is confirmed by exactly the sign its author named.** At sixty-four, the
repetition is entered 3n+3 times while the four rules beneath it are entered three times the
triangular number: linear give-backs, each re-reading a prefix that grows. A greedy repetition that
cannot finish gives back one element at a time and retries the continuation, and giving back costs
what has already been eaten rather than a constant. The one-character run now has a mechanism as
well as an absence: nothing is choosing between readings there, it is re-reading what it read.

**Decided.** The twenty-odd quadratics are one illness rather than several under one exponent, and
**the cure belongs to the reader**. The polynomial diagnostic is not designed and not needed: a
diagnostic warns about a shape an author could avoid, and this is not one. What is commissioned is
a design for the reader — after the two uncompilable configurations and the paths, and above the
speed items, because polynomial on untrusted input across twenty shipped entry points outranks
constants.

**Conditions on that design, set before it is written.** This count is its before-picture and
re-running it is the after-picture, in counts rather than times, so it needs no window. Remembering
a failed prefix must not cost an accepted parse, which is the common case and the one the whole
year's work is about. Whatever is remembered is bounded and released, joining the retention rule
rather than growing a policy of its own. And the earlier refusal of wholesale memoization stands
untouched: remembering that a rule failed at a position, after a long re-read, is not memoizing
every position, and the objection that most refusals arrive on the first token does not reach it.

**Two things are not claimed**, and the session counting them said so: three of the web's cases are
counted and none of SQL's, that being the other session's one case — whose 1,056 is already twice
the triangular number of 32 and fits; and a count says where the work goes, not what the fix is.

**A counter-case, and reading it carefully narrows D63 rather than breaking it.** sql-39 counted
SQL:2023's "case arms with no end" and found every count linear over an eightfold range — calls,
entries into the materialiser's walk, the walk's listing iterations, and the cells zeroed — with
records never given back, so nothing is re-read there at all. Divided by the product, the calls
halve at every doubling, which is a plain no to the test.

**But that row is one of the audit's *superlinear* seven at 1.49, not one of its sixteen
quadratics.** So it does not contradict the class: it shows that a superlinear row need not be
structural at all. Whatever bends its time is per-record work no count sees — allocation and
collection, cache, an arena growing — or the measurement itself, and the session said plainly that
it cannot separate those three by counting and will not publish a time from an instrumented build.

**So the decisive SQL case is still to be taken, and it must be a quadratic one** — the query or
the search condition at 1.9 to 2.0 — because that is what decides whether D63's arithmetic reaches
beyond the web package. A second superlinear case would answer a different question. The case-arms
row goes back to the stand to be retaken with allocation counted, since the question there has
turned from a structural one into a question about time.

**And the three things counted beside the calls are why the no is trustworthy**: the walk's count,
its length, and the zeroing were each a candidate for making the work grow while the call count
stayed linear, and each was measured flat rather than argued away.

**D63's scope is corrected by its own counter-case, and the line was already in the audit's last
column.** Put the four counted cases beside their exponents and the split is plain: every case near
2.0 lands on the triangular number exactly, and the one at 1.23 is flat in every count and gives
back no records at any size. The division is not between packages and not between instruments — it
is between the exponent-two rows and the exponent-one-point-two-to-one-point-five rows, which is
the distinction the audit's table already drew. **So the class commissioned for a cure is the
quadratic rows, eleven or twelve rather than twenty**, and the superlinear five are not this
illness; the reader change must not be expected to move them.

**And the limitation of the sample is named by the person who chose it:** three cases unlike in
*shape* and alike in *exponent* is strong evidence about that exponent and silent about the others,
which the counter-case revealed by being the first taken from the other group. The superlinear
group is not chased now — three places a quadrature could hide in it have been ruled out by count,
and what remains is per-record work outside re-reading, a different investigation with a different
instrument. It becomes its own item rather than being folded in, because folding in is how twenty
became the number.

**One method note carried over for the remaining cases:** walks can stay linear while each scans
further, which would read as a product in time with a linear call count. The three counted show the
product in the *calls*, so re-reading is established for them; if any of the remaining eight comes
out linear in calls, that is where to look, and to look before designing rather than after.

**The refusal guard is on main and green**, 85 tests in 69 seconds, and it was shown to fail in
three ways: a missing baseline line, a series worse than its baseline, and a baseline holding an
explosive class, which is refused whole. **What has not been seen fire is the assertion on an
explosive series itself** — read rather than observed — so it is calibrated against a tree from
before the seals, ten minutes and one build. A checker that has never failed is not checked, and
that rule does not stop applying because the rest of the checker has been proved.

## D64. The class is re-reading, and its degree is the nesting of ambiguous choices

The quadratic SQL row was taken and answers more than it was asked. First a fact about the list:
**all four quadratic SQL rows are one case in two grammars at two entry points** — nested
parentheses — so there was no other to pick, and a count of rows overstates the number of distinct
defects. The row taken is the steepest of the whole audit, at 2.68.

**The product is there exactly, and it is not the top term.** Per rule, at four sizes, the ratios
are 1.0000 to three formulas: one pair of rules at four thirds of n(n+1)(n+2), another at four
times n(n+1), a third at four times (n+1) squared. A square and a cube in one reading. **Two nested
ambiguous choices make a square; three make a cube**, and T-SQL's query expression sits one level
deeper than SQL:2023's, which is why the exponent is 2.68 and not 2.

**So the class is not "the quadratics" — it is re-reading, and the exponent is its symptom.**
Grouping the audit's rows by exponent was a proxy for the thing itself, which is the same habit this
journal has spent three days naming: a likeness counted in place of the construct. The correction
does not move the superlinear rows, which showed no re-reading at all in any count; it moves the
ceiling, because a cure aimed at squares would have been measured against a row that is a cube.

**And two facts strengthen the remedy rather than complicate it.** Nothing is materialised in this
case at all — the walk's listing loop runs zero times at every size — so the whole cost is
re-reading and the records given back are linear beside it. And a memory of a rule's refusal at a
position removes the cube as well as the square, and removes more of it: the deeper the nesting,
the more it saves. That is an argument for the remedy already named, not a new one.

## D65. Every D58 timing was taken on tier-0 code, and "it is free" comes out of the record

Neither harness ran a method long enough to reach the optimising tier, so all eighteen readings —
twelve from one session and six from the other — describe code no consumer runs. Tier-0 is about
three times slower on the shape in question; at an intermediate number of readings the transition
lands mid-series and a point goes bimodal, one build against a byte-identical copy of itself
swinging 0.825 and then 1.433 in consecutive runs. **That is not a measurement problem, it is the
runtime changing the thing being measured while it is measured.**

**So the claim that probing every level is free is withdrawn**, and with it the mechanism offered
this morning for a both-signed pattern: the pattern was tier-0 behaviour and not code layout, and a
mechanism proposed for a pattern measured on the wrong code is worse than no mechanism. The
arithmetic on those numbers was right; the numbers were about a tier nobody ships against.

**What may replace it is not a result yet.** One run with tiering off put the ratio at one to four
per cent above unity on all six shapes with a two per cent control; a second contradicted it with a
ten per cent control and therefore cannot be read. Two runs disagree and neither is quotable; five
consecutive runs with selection by control are asked for.

**The decision does not change and its wording does.** The probe on every level repairs a defect
that ties a consumer's usable depth to their build configuration, which is worth a few per cent if
that is what it costs. But "free" and "two to four per cent" are not the same promise, and the
difference is exactly what somebody finds after it lands and is right to be annoyed about. So the
change lands with whatever number the clean runs give, and if that number is material, the question
of a cheaper probe reopens — which it should not, given the alternative is a constant that was
wrong for the same reason twice.

**Three instrument failures in one day, and the rule they leave.** A harness that measured process
startup, a script believed dead that was alive, and two harnesses measuring tier-0 — each reported
confidently before the instrument was checked, and in each case checking cost less than the run.
**A timing report names the tiering mode it ran under, exactly as it names the commit its binary
was built from**, and a harness whose control is not stable produces no rows at all.

**The counts are untouched**: call counts do not depend on a JIT tier, so D63's before-picture, the
triangular numbers and the exponent boundary all stand exactly as reported.

**And the tier-0 warning immediately cost a correction elsewhere, which is how a rule earns its
keep.** The diary's entry on the parentheses carried three figures from a scratch harness that
timed one shot per size in a fresh process — tier-0 at the small end and promoted part way by the
large one. The entry now says so and says that nothing in it rests on them. **The reason it had to
be corrected rather than left: a number that was never sound reads as sound once it sits in a table
beside numbers that are.** That is the same shape as the tense corrected an hour earlier — the
context lends a claim a standing it never had on its own.

**The guard's explosive path is calibrated on the real tree, not on tampered numbers.** Built at
the parent of the day's first fix, with the guard's files copied in and the baseline from main: 85
tests, eleven failed, seventy-four passed. The eleven are the two known exponentials, the address
refusal that a later seal turned quadratic, and **all eight interpolated forms of the expression
language — including the raw and verbatim ones that nobody had listed as a risk**, which the
author's seals had covered anyway. Each failure carries the message naming the shape and the
remedy, and the other seventy-four sit at their baseline on the old tree, as they must, no fix
having touched them.

**So the guard would have failed the build that shipped all three defects, and would have named two
that no list held.** That is the strongest thing a guard can demonstrate: not that it repeats what
we already knew, but that it finds what nobody had written down.

And the run before it was wrong for a reason worth keeping: the script was pointed at the wrong
binary by an edit that silently did not take, and it was caught because **the failing identifiers
were the ones that had been tampered with rather than the explosive ones** — the shape of the
failure did not match the experiment. A wrong answer that fails in the wrong *place* is a cheaper
warning than one that fails plausibly.

## D66. The retention defect was corruption, not a leak, and the commit message understates it

Run at the parent of the retention work and at its head, because fifteen failures had appeared in
classes unrelated to clearing: **the parent fails 453 times over 190 distinct tests; the head fails
13 over 2, and nothing fails at the head that passed at the parent.** So the two commits introduced
nothing and removed a hundred and eighty-eight distinct tests' worth of breakage.

**And the defect is worse than it was reported.** The early return skips more than the clearing: it
also skips resetting the cursor, so an oversized store went to its slot with its tables full *and*
its position still at the end of the last parse, and the next rental received it in that state.
That is not a store that holds memory too long; **it is a store the next parse reads as though it
already held a document**. A hundred and ninety tests across unrelated suites is what stale reused
state looks like.

**Why it was reported as a leak: the probe answered the question it was asked.** A reachability
check said the values were no longer reachable, which was true, and said nothing about a cursor
nobody had thought to ask about. The full suite found the rest an hour later. A narrow instrument
confirms a narrow claim, and the confirmation reads as if it covered the area.

**The record is corrected in the diary rather than by amending the commit**, which is right: the
sha is cited in three sessions' messages, and rewriting it to fix a sentence trades a wrong
description for a wrong identifier. The diary is where what we believed at the time belongs, with
both shas and the dates.

**And one general note worth keeping, from the test still failing at the head.** It asserts the
*old* policy — that an oversized store is not retained — which the deliberate change inverted, and
it fails at the parent too. **A test that contradicts a decided design change looks exactly like a
test that caught a regression, and the only thing telling them apart is knowing which way the
decision went.** So it is rewritten to the policy chosen rather than deleted, and the lesson is
that the commit which inverts a test's premise should carry the test with it.

## D67. A cliff wearing an exponent: the superlinear rows need the same recount as the quadratic ones

Every point of the case-arms row was printed. Below about 1,391 arms it is flat in both senses —
constant bytes a call per arm and constant time per arm, matching the four structural counts
exactly. Between 1,391 and 1,738 arms the bytes jump elevenfold for a quarter more input. The
arithmetic points at the parser pool's retention bound, the same mechanism found on another row
earlier and already recorded in the performance notes as an oversized store read as a quadratic
when it is a cliff. **The fitted exponent reads the last sixteenth of the ladder, so two points
above the bound produced the 1.49.**

**So the superlinear seven need the recount the quadratic sixteen got, and it is cheaper.** A cliff
and a slope are told apart by a number every series already has — bytes a call per unit of input,
flat and then jumping against rising steadily — with no structural counting at all. If the joins
row and the two predicate rows are cliffs, the seven is not seven defects.

**Twice now a row count has been an over-count, from a different cause each time**: one case
wearing four rows, and a machine bound wearing an exponent. A row is not a defect, and the number
of rows is the least reliable summary of a table we have.

**What it does not break:** the guard still behaves correctly, because a new cliff appears as a
worse class and fails, and a cured one appears as better and tightens the baseline. What it costs
is naming — a cliff baselined as a slope is a true classification of the wrong thing — so the
report prints the discriminator beside the exponent rather than adding a class to the guard.

**And a self-correction worth the same weight as the finding.** The earlier report said every count
in that case was flat per arm; that was true of the range measured, eight to sixty-four arms, and
was stated of the row. The cliff is twenty times further out than anything the probe reached. The
conclusion stands — under the bound the row is structurally linear — but **the range belonged in
the claim**, and a claim without its range is how a measured fact becomes a wrong one.

**The discriminator is in and it separates the seven: four are cliffs, three are not.** Each series
now prints, beside its exponent, the shape of the bytes a call per unit and of the time per unit —
a step of two and a half or more between sizes a quarter apart is a cliff, otherwise rising or
flat — with the class and therefore the guard unchanged.

**The four cliffs are all one mechanism.** They are SQL:2023's rows, flat and then stepping, and
each step sits at the same product against the pool's bound of 65,536 cells. So those four are not
four defects but one, and it is the one the retention fix is aimed at — which also makes them the
natural rows of that fix's pair, each step bracketed by two sizes. The three that are not cliffs
are slopes of allocation with a mild slope of time, and one row sits on the class's edge at 1.49
then 1.50 across two runs, so it is read as unstable rather than as either.

**And the sixteen quadratics gain a caution that matters more than the four.** On ten of them the
bytes *also* jump in one place — an allocation cliff sitting underneath a real quadratic in time —
while three show bytes flat with time rising, which is exactly what the call counts said. **The
quadratic does not go away with the cliff.** So when the retention fix lands and those ten improve,
that improvement is not the re-reading being cured, and nobody may read it as such: the reader
change's acceptance remains the counts, which do not move for an allocation bound at all.

**The caveat is stated by its author rather than left to be found:** the shape column is a rule of
thumb over ladders of twenty to thirty points, a cliff smaller than the threshold reads as flat or
rising, and a tier change or a collection mid-ladder could in principle fake one. The four are each
bracketed by two sizes and repeat at the same places across two runs; a fifth from a single run
would be checked against the every-point dump before being named.

**And a correction sent before anyone could quote it: only one of the four cliffs is confirmed
twice.** The case-arms step appears at the same place in the every-point dump and in the audit; the
other three come from one run each, and the earlier audit agrees only that those ladders stopped
near the same sizes, which is a different statement. So one is confirmed and three are named
pending their own dumps. **That distinction is exactly the kind that evaporates in a summary** —
"repeated across two runs" was true of the row that prompted it and became a sentence about four —
and the correction arrived before the sentence had been used, which is the only time a correction
is cheap.

**Squash it: main may not carry a commit that corrupts reused stores.** The earlier instruction not
to amend rested on a reason that no longer holds — the chain is thirty commits behind, so a rebase
rewrites every identifier in it whatever we choose, and the citations die either way. What is left
to decide is only what main ends up containing, and there the answer is plain: **every commit on
main is a bisect target**, so a state that fails four hundred and fifty-three tests is not a record
of how we got here, it is a trap for whoever bisects through it for something unrelated.

The record lives in the diary and in this journal, which describe the episode without leaning on
identifiers; the diary entry is rewritten accordingly. And the squashed commit's message carries
what the intermediate state had done wrong, so that the knowledge does not leave with the commits
that held it. Against this stands a real position — that history and record agree if both hold the
defect and its fix — and it loses to the cost of a poisoned bisect, not to a rule.

The rest as proposed: rebase, retest, hand the stand the sha for the pair, push after it passes,
and never let the pooling change ride under a newer commit ungated. And the suite was re-run after
the commit because the message claimed it was green when only one class had been run — an inference
in a commit message being exactly what the day has taught.

## D68. Igor: FIX is the priority now, 2026-09-20

The order is to move faster on FIX, to have somebody survey the libraries of the same kind and
compare against them, and to find out what a validation dictionary is — **all of it to be built,
not only studied**.

**Two of the three already exist as studies and neither has become work.** There is an account of
the libraries, taken from the source of one of them rather than from its description, and a study
of validation dictionaries — both formats, what our schema already holds, what it does not, and
where the line between build time and run time falls. What is missing is implementation and
numbers, and that is what the order converts them into.

**The work, in this order.** The validating layer first, because D53 settled its shape and it needs
no new machinery: a validator object holding a dictionary, every finding rather than the first, a
finding naming the entry's index in a repeating group as well as the tag, and validation against
our own compiled schema when no dictionary is supplied. Then the reader of a counterparty's
dictionary — run time for what is checked, build time for what is *constructed*, since a type is
not data. Then the comparison's numbers, which the layer makes like-for-like by construction: the
other engine parses in one call and validates in another, and after this so do we.

**The survey widens and goes to the critic**, whose business is reading and whose licence work on
the first library was exactly this. Not only the one engine on this platform: what else exists
here and elsewhere, what each one's model is, how each expresses a dictionary, and what licence
each carries — read from the file, as before, because a shipped package may carry nobody's data
but its own.

**Four questions the dictionary study left open are answered so that nobody waits on them.** The
build-time reader is in scope for this repository, a generator over a foreign input being what this
repository is. It produces, in order, the length/data pairs and the schema tables first and the
typed field classes second, the third being the one that answers a real venue and also the largest.
Requiredness stays binary until something needs more; widening it later goes to the five the trade
body's own format uses and not to a vocabulary of ours. And whether the empty specification
directory was a place prepared for this is a question for the package's owner rather than a
decision.

**The empty specification directory is a leftover, and its history matters more than the answer.**
It held a thirty-three thousand line dictionary of somebody else's and a generation step over it
built on a template engine, and both were removed on 16 September under the heading of maintaining
the definitions by hand. **So reading a dictionary at build time has been in this package before,
and was taken out** — which is a sentence someone will reach for the moment this work is proposed.

**What was taken out is not what is proposed.** What went was a generator living in an editor
rather than ours, and a vendored copy of a third party's file inside our source tree, which is
exactly the licence problem the dictionary study describes. What is proposed is our own generator
over the *consumer's* file, with nothing of anyone else's in the repository. The shape is the same
and the substance is the opposite, and saying so now is cheaper than answering "we tried that"
later.

**The first step is split in two, which is right.** The layer arrives beside the existing
behaviour — findings, a validator, the standard instance over our compiled schema — changing
nothing; then building becomes unconditional and the modes leave. The second changes the answer
for messages the strict mode rejects today, and mixing that with the appearance of new code would
lose which of the two moved what. Without the first there is also nothing to check the second
with.

## D69. Igor: validation is asked of the message, 2026-09-20

The shape is settled from above: a separate method by which each message can be asked whether it is
valid. That overrides the design's preference — which I had approved — for a validator object as
the only entry, and the objection behind that preference survives inside the new shape rather than
against it.

**The objection was that a dictionary is loaded once and read many times, so it cannot be a
parameter of every call and must not be a mutable static, which D25 keeps out of this package.**
Both hold if the method on the message *takes* the validator: one form for a counterparty's
dictionary, and a bare one for the schema we compile in. The second needs no global configuration —
a shared instance built from our own compiled tables is data we ship, not a registry a consumer
mutates — so the thing D25 forbids never appears. Asking the message is the consumer's verb and the
dictionary still lives in one object.

**And the second half of the order stands on its own: look at how the other libraries expose it.**
That goes with the survey already commissioned, narrowed to the API rather than the model — whether
validation is a method on the message, a method on a dictionary, a separate service, what it
returns, whether it stops at the first fault, and what a fault carries. We have decided three of
those from first principles; seeing what a consumer of another engine already has in their hands is
worth more than another argument, because a consumer arriving from one of them brings its habits.

What does not change: every finding rather than the first, a finding naming the entry's index in a
repeating group as well as the tag, no validating overload on the parse, and validation without a
dictionary against our own schema.

**A correction from Igor about the critic's role, and it is about my behaviour rather than the
critic's.** The FIX survey is withdrawn from it: that session's job is to argue with the
architect's decisions and to find better ways of doing the work, and answering the architect's
research questions is neither. Looking back at the evening, four of its entries were "the
architect asked, the critic went and found out" — useful findings, and the queue was mine.
**A critic working from my list cannot object to my priorities, which is most of what it is for.**

So the exchange keeps its shape — answers to objections, disagreements, corrections, in both
directions — and I send no tasks, and relay none as Igor's. Where FIX is genuinely the subject, what
comes back should be an argument about how it is being approached or a better road, not a survey to
my specification. Nothing already recorded changes; the entries and the answers against them stand.

The survey itself is still wanted, so it goes to a session that takes work: it is reading and
writing, its own area's line is closed, and the priority is FIX.

**The layer's first half is written in D69's shape, an hour before the older shape would have been
fixed by tests.** One public road, read as the consumer's verb: ask the message, with or without a
dictionary, the bare form being exactly the standard one and a test holding that equality so it
cannot drift apart in silence. Seventeen tests for the layer, the package green, and behaviour
untouched — the strict mode still refuses what it refused.

**Two facts the move brought out, neither new and neither previously visible.**

A finding for a missing required field was naming a *component* — the identifier of a group of
fields — which a consumer cannot look up anywhere. It now names the first tag that component would
have held, which is the thing the reader actually forgot. **A message that names an internal
identifier is a message that cannot be acted on**, and the move surfaced two of them.

And a check with one reachable branch: a group whose count disagrees with its entries never
reaches the layer at all, because the entries are cut *by* that count, so the wire is refused
before a message exists — in the lenient mode too. What survives is a required group declaring no
entries, which is correct as wire and wrong as a message. **The check had stood in the strict mode
from the beginning and was almost always dead**, and only moving it made that visible. The test
holds the one branch and says in a comment why the other two do not exist.

And the session sends what it already knows about the other engines' API to whoever writes the
survey, rather than keeping it: an engine's validation there is a service that throws on the
*first* fault, which is the opposite of our choice on two of four points. Sending it first means
the survey can contradict it; keeping it would have meant the survey confirming the person who
commissioned it.

**D58 closes with a number rather than an ellipsis: the probe on every entry costs about one and a
half per cent on optimised code**, four clean runs, sixteen measurements, fifteen of one sign, the
control at unity. The morning's "free" was measured on the unoptimised tier and was wrong by a
factor of three. The explanation offered for the both-signed pattern is withdrawn with it, there
being nothing left to explain, and the experiment that was to test it is cancelled as answering a
question that no longer exists.

**The decision stands at the new number.** One and a half per cent buys the removal of a defect
that ties a consumer's usable depth to their build configuration, and of the class of constant that
was wrong twice for the same reason. What changes is only what we write down — and the difference
between "free" and "one and a half per cent" is exactly the kind of thing a reader finds later and
is right to mind.

**And the survey is set up so that it can contradict the person who commissioned it.** What one
session already knew about another engine's validation was sent ahead and handed to the reading as
*claims to be checked* rather than as data: a survey that repeats what it was told has checked
nothing. The readings go across platforms rather than around one, with one questionnaire each —
the model, the API's verb and what a fault carries, and the licence read from the file — and one
engine whose validation the existing account does not cover at all is named as the gap it is.

**The three cliffs named as pending are confirmed on their own dumps**, at the same places as the
first run: each of the four SQL rows now has flat per-unit bytes and time below its step and a
jump of four to eleven times across it, the case-arms row seen three times and the others twice.
**And the joins row is not one cliff but two** — a step in bytes between one pair of sizes and a
step in time between a later pair — which a single exponent could never have shown and which the
shape column separates only because it prints both quantities. Nothing changes class, and the guard
is untouched.

**A retraction about what the bool rows measure, and it lands better than it sounds.** Those rows
compare the *match* form of one side with the *bool* form of the other by construction: they are
two APIs, not two commits. So a halving of time and allocation on them supports nothing about any
commit, and an unrelated pair shows the same halving on the same rows, with an A/A of one build
against itself at minus fifty-two per cent. A hypothesis about a store rented and dropped past a
bound drew on those rows and must let go of them.

**What the journal already says is the correct use of the same number and stands.** D4's entry
records the halving as the price of the *match* form against the bool form, which is exactly what
the rows measure, and it is now supported by two independent runs rather than one. The error was
never in the number; it was in attributing an API's cost to a change.

Of the rest of that report: the constant fall of eleven to twenty nanoseconds on the web refusal
rows stands, the accepted rows are not separable from the A/A, and a two to three per cent rise
offered without an A/A turns out to be what an unrelated pair shows too — the second-slot bias of
short rows — so it is unsupported rather than a price.

**And the fix is procedural rather than an apology:** every pair carrying such a row will say in its
header what the row compares. A row whose name reads as a variant of its neighbour and whose
meaning is a different API is a trap laid for every future reader, and the person who fell into it
is the one who built it.

## D70. The publish workflow's one irreversible step is guarded only on a tag, 2026-09-20

Read while reducing Igor's two release decisions to one action each. The workflow runs on a tag and
on a manual dispatch. The check that the tag and the version in the build properties are the same
thing — described in the file itself as the one mistake nobody can take back — is conditioned on
the reference being a tag. **The step that pushes to the gallery is not.** It is conditioned only on
the API key being present.

So once the key is a secret, a manual dispatch publishes whatever version the build properties
happen to hold, with the version check skipped, from whatever commit the dispatch runs on. Nothing
about it is wrong today, because the key is not there yet and the step is skipped; it becomes wrong
on the day the key is added, which is also the day nobody re-reads the workflow.

**The fix is one condition**: the push runs only for a tag, or the version check runs for a
dispatch too. The first is smaller and says what we mean — a release is a tag, which is the
workflow's own first sentence. A dispatch then remains what the file says it is: the same run with
the last step skipped, which is how it can be read before it can publish.

**And the decisions themselves reduce to actions rather than choices.** The secret's name is
already fixed in the file, so what is wanted is the secret, not a decision about naming. The
version is a line in the build properties and a tag that matches it, and the workflow refuses the
pair if they disagree.

## D71. The FIX engines divide by when the dictionary is read, and our road is the majority one

The survey came back (critic, at Igor's asking, on the `critic` branch and unpushed while the
network is down). **The families divide by *when* the dictionary is read, not by speed.** One reads
it at run time; one reads it at build time and generates code, which is what the throughput engines
do — a codec generator run before compilation, a schema compiled statically, a typed-class
generator shipped beside a run-time dictionary; and one lets the schema decide the wire itself. **We
are in the second family**, which is worth recording because a critic saying the road is right is
rarer than one saying it is wrong.

**Run-time loading is not barred by D25**, and the objection I would have made from memory is
wrong. D25 allows a fork the *consumer* owns, a dictionary they supply being their data, and it
even prescribes the shape: not a dictionary consulted per field but a table merged when the options
are built, one cell read per field afterwards — which is what `FixFieldOptions` was already
changed into.

**The plan's hinge, and it is a measurement rather than an argument: the two roads need not be two
implementations.** If build-time generation and run-time loading fill *the same table shape*, there
is one reader and two ways to fill it. What decides it is whether those tables can be filled at run
time without losing what a compile-time constant buys on the hot path — one measurement, taken
before anything is written, and the answer is the difference between one implementation and two.

**Q25, the mutual check, is decided now rather than by the order of the work.** The idea is
Igor's: a loaded dictionary can verify the built-in tables, and the target is real, the schema
being tables already so the comparison is a walk rather than a parse. The objection is that the
economical build has *one* reader serving both roads, and then the two tables agree by
construction and the check verifies that a function equals itself. **So: one reader, and the check
is named a staleness guard rather than a verification of our reading.** That is honest about what
it proves and keeps the use that makes it worth having — a consumer built against one dictionary,
a counterparty issuing a newer one, and the disagreement firing at start-up instead of becoming
wrong parses hours later. What to do on a disagreement — refuse to start, believe the file, believe
the tables — is Igor's.

**The schema deciding the wire is a grammar, not an engine.** For tag-and-value it does not arise,
the sender fixing the order rather than the schema; for the binary wire the language already has
counted repetition and a byte of `any`, so a fixed-width field is expressible today. What that
format buys beyond decoding — reaching a field by computed offset without touching the bytes
between — is layout and not parsing.

**Two things to carry into the pages, neither covered before.** The licence obligation moves to the
*consumer* the moment a dictionary is an input to their build: the file sits in their repository
and their compiler reads it, so the page saying we ship none must say that in the same breath —
including that a diagnostic quoting a dictionary's text puts somebody else's words in their build
output. And the claim that nothing on this platform generates the codec *inside* the consumer's
compilation is marked searched-and-not-found rather than read-and-confirmed; if it holds it is a
better front page than any throughput number, and it may not be written anywhere until it is
confirmed.

And Q25 does not close Q20: it holds the *tables* equal, while what a parse says when it refuses
is still held for one input form and for none of the others.

## D72. Igor: a plain array of validation delegates, replaceable at any moment, 2026-09-20

The build-time road must produce **code**, not arrays: if compiling a dictionary only builds the
same tables earlier, loading covers it entirely and there is no second road. So a compiled
dictionary generates validation as straight-line code per message type, and the difference between
the two roads becomes real rather than a matter of when a table is filled.

**Overriding is a delegate, not inheritance.** Our field classes and message models are sealed and
an exhaustive switch over them is something a consumer relies on; opening the hierarchy takes that
from everyone, and it would need a second seam so that the builder constructs a consumer's
subclass. A replaceable delegate keeps the types sealed, keeps the pattern match exhaustive, and
needs no seam.

**The shape, decided: a plain array on the validator, written at any moment.** No immutability, no
copy on first write, no meaning attached to null, no restore. The validator holds its own array
from construction, a slot is a reference and its replacement is atomic, and loading an external
dictionary is simply a method that writes many slots. **Last write wins, and restoring anything is
the consumer's business, because the consumer controls all of it.**

The constraint I had proposed — immutable after construction — was inherited from the parse
options' shape rather than derived from this one's need, and it is withdrawn. Reading a slot
concurrently with replacing it is safe: a reader sees one delegate or the other. The one hazard is
order, and it needs a sentence rather than a rule: a dictionary loaded after an override overwrites
it.

**One thing follows if restoring is the consumer's business: they must be able to.** The generated
defaults are a table we ship, so it is readable and a consumer can put an entry back. Otherwise
"you control all of it" is not true.

**Custom messages use the same mechanism in the shape already chosen next door**: a dense array by
number for what we compiled in, a rare map by type code for what we did not — the slow branch
reached only by what was never described. And "custom" is a state rather than a property: undescribed,
described at run time by a loaded dictionary, or described at build time and no longer custom at
all. One delegate shape throughout, taking the base message, since a table can hold only one.

**D72 leaves one thing open, and it is the right question: what `Standard` becomes.** The rule that
a consumer controls all of it holds for a validator the consumer made. The bare form of the call
goes to a shared instance, and if *that* array were writable, one library inside a process could
load its counterparty's dictionary and change the answer for every caller that never asked — which
is the shape D25 keeps out, entering from the other side.

**Decided: the shared standard is not written to.** It is the generated defaults, a validator of
one's own takes its own array from them, and "the defaults are readable" means a consumer can take
a cell out of it and put it back into theirs, which is the restoring Igor left to them. The
alternative — a deliberately writable shared instance, "this process speaks 4.4 plus my venue's
rules" — is cheaper in code and dearer in explanation, and it would be used by accident. Igor's
rule survives either way; this only says which object it applies to.

**And the dictionary is in the repository now, by Igor's instruction**, which changes a premise the
session was reasoning from: a real one is kept beside the SQL corpus, byte for byte, with its
licence and its provenance, exempt from line-ending normalization, reaching no package. The
obligation that moves to a consumer is about the *feature* — their file in their build — and not
about material we read here, which is the same treatment the ISO grammar and somebody else's SQL
corpus already have. Hand-written fixtures are still wanted beside it: a small dictionary written
here drives the cases a test needs, and the published one is the thing the reader is held to
because we did not shape it.

## D73. The reader lands, and the published dictionary disagrees with our tables in ten places

**Ten tags where a published dictionary and our compiled schema say different things.** Every tag
our tables define exists in the file and every other type agrees; these ten do not. Four we call a
number and it calls a character or a string, three we call a group count and it calls an integer,
and the rest go the other way. **Neither side is an authority**: the file is one reading of the
protocol and ours is another, and what decides is the published specification, which nobody has
read for these ten. Two of them *look* like the other side's defect — a type that cannot hold
values the same file lists for it, and a counter that counts answers rather than entries — but
"looks like" is not a reading, so they are pinned with the rest. The test fixes the list rather
than a verdict: a new disagreement fails it, and closing an old one fails it too, so the list is
corrected in the commit that corrects the tables. **It goes to Igor as a question about authority,
not as a defect report.**

**And a correction to figures I published myself.** I counted the dictionary by grepping its
elements and quoted 3,783 fields and 451 components. Those are *references* from inside messages
and components; the declarations are 916 fields and 24 components, with 93 message types. The
corpus page is corrected. The figure nobody counted is the one that shaped the reader: **59 group
names used in 226 places** — this format declares a group where it is used, so one name carries a
different membership in different messages, and a reader keying a group by name would merge two
shapes into one. The identifier is by place.

**This format has no conditional rules at all.** Its whole vocabulary is eleven element names and
none of them is a rule or a condition, which agrees with what the earlier study found. So the
instruction to design the reader so that conditional rules can be handed out as text has no input
today: there is nothing to hand out. **Igor's idea about using our expression language is not
withdrawn — it is aimed at the other format**, where rules are written in a language of that body's
own, and there a rule compiled into a delegate *is* D72's cell.

What was done for it now, without inventing an input: the parse builds a tree of names and resolves
it in a separate pass rather than filling tables as it goes. A reader of the other format fills the
same tree and hangs a rule on a member; tables and delegates are built from the tree either way.
That is the only shape that costs nothing today and saves a rewrite then.

**The cross-check found what it was built for, and in three kinds rather than one** (`9f775b2e`).
Ten tags disagree on a declared type, as reported before. **Seventy-seven disagree on their value
sets**, in three sorts of which only the last is cosmetic: more often ours is the richer — the file
gives a boolean no set at all where we have both values, and omits the "other" entry that ends
several sets — less often theirs is, and twice it is spelling, where a value written differently is
a value that does not pass. **And twenty-six disagree on what a message is made of**, all in one
direction: the file reports what we do not. One group entry holds twenty tags the file does not put
there, and four messages require something we do not.

**That third kind could only come from the walk**, because the walk had to be right before either
side could say anything — and getting it right turned up three defects in the new wiring that a
test over compiled tables could never have reached: membership resolved against the compiled schema
so a loaded dictionary failed on its first message; type names taken from our vocabulary rather
than the file's, so every value of every tag read as wrong; and "described with no members" read as
"no such type", which made a type the file does describe come back unknown.

**So Q25's shape is corrected by its own first run, and this is the rule worth keeping: a check of
two implementations against each other runs along the path the consumer uses, not over the data
they are built from.** The table comparison found data. The same comparison driven through the walk
found data, composition, and the code that walks it.

**All three lists are pinned rather than judged**, and a row appearing or disappearing fails the
test, which makes them a guard rather than a note. Which reading is the protocol is answered by the
published specification, which nobody here has read for these rows. One thing is evidence without
being proof: the twenty-six run entirely in one direction, and a table maintained by hand is likelier
to omit than to invent.

**Before the size is measured, a count reshapes the question.** The published dictionary writes
3,225 members across its 93 messages; expanded through components, 12,532. The multiplier of just
under four is not an implementation choice but what expansion *means*: one component named in
twenty messages becomes twenty copies. **So expanding and sharing the components are opposites
rather than independent knobs** — sharing gives almost all of the size back, and with it much of
the benefit, because the indirection through a component is a good part of what the walk pays for.

Two forms are therefore measured from one emitter: fully expanded, and shared methods per
component. A single number would have answered the wrong question, since which of the two it
described depends on the decision it was taken to inform.

**One thing to measure beside the size, or the choice is made on an unconfirmed benefit.** The
shared form is not the walk: it replaces a table read, a loop and a branch per member with a call
and constants. If it comes within noise of the expanded form in speed, it wins outright and the
size question does not arise. So the pair of sizes is taken with a cheap comparison of the two on
one representative message — otherwise we would be paying a doubled assembly for something nobody
showed was faster.

The expectation is named first, with the direction of its likely error, and what the measurement
excludes is named with it: the per-tag checks of type and value set are shared by both forms, so
leaving them out lowers both absolute figures and leaves the ratio alone. And the third variant —
generating only the types a consumer names — becomes arithmetic once the cost of one type is
known.

## D74. Generated validation shares its components: +17% and nothing lost, 2026-09-20

Both forms emitted from the same schema, compiled and weighed against the package's 811,008 bytes.

| form | methods | positions | added | share |
| --- | ---: | ---: | ---: | ---: |
| expanded per message type | 823 | 12,527 | 297,984 B | +36.7% |
| a method per component | 196 | 2,904 | 138,240 B | +17.0% |

And on the largest fixture — an execution report of 391 fields with nested groups, best of fifteen
batches, three interleaved rounds in one process — **the expanded form buys nothing**: the shared
one is within one per cent and lower in every round, so the difference has not even a sign.

**The answer is the shared form, and the answer does not need a window.** A difference invisible
across three interleaved rounds cannot pay a hundred and fifty-eight kilobytes, whatever a careful
measurement would eventually say about the last fraction of a per cent. That is the distinction
this journal has been drawing all day between a number and a decision: the number would need a
window, the decision does not.

**The walk's column is a floor and not a comparison**, and the report says so: the walk also checks
every value and code set, which neither generated form does, so what it gives is the level below
which generated validation starts and above which it must still pay for the per-tag checks.

**And the measurement found what it was not looking for: membership cannot be shared.** A
component's code can carry its own requiredness, reading the caller's mask, and its own groups; the
*membership* cannot, because the calling region must list every tag of every component it names.
That is a property of the task rather than of the emitter, and it relocates where the saving lives:
in requiredness, group bodies and the number of methods, not in the switch labels, which are as
wide in both.

**It was caught by the rule that the two forms must answer alike before either is timed** — the
expanded form reported nothing on a valid message and the shared one reported 1,667. Comparing the
speed of two things that disagree measures nothing, and here the disagreement was a wrong
construction rather than a slip in the harness.

**The prediction failed in both size and direction, and is reported as such.** The expanded form
was put at 380 to 750 kilobytes with the stated likelihood of erring low; it came out at 298, below
the floor, so the named direction was wrong too. The mechanism is worth keeping: thousands of
switch labels are *cheaper* per label than a few dozen, dense integer ranges compiling to jump
tables and binary search, and the estimate had extrapolated a per-label cost measured in the small.
**A per-unit cost measured in the small does not extrapolate** — the third instance today of a
ratio taken from one material and applied to another.

**Igor: start-up time does not matter, validation speed does.** That settles three things that were
open or half-open.

**Loading compiles eagerly.** The worry that ninety-three validators built from a dictionary's text
would cost too much at load is withdrawn: nothing lazy is built, no cell holds a thunk, and a
delegate is a delegate from the moment the dictionary is read. The same goes for rules compiled
from the other format's expression language when that reader exists.

**It does not reverse D74.** The expanded form was refused because it bought *nothing* in speed for
twice the size, not because it cost something elsewhere. The instruction reweights the next such
choice rather than this one: a form that is genuinely faster may spend freely at load.

**And it names where the next number belongs.** Both generated forms were measured without the
per-tag checks of type and value set, which is why the walk's ten microseconds was reported as a
floor rather than a comparison. Validation speed being what matters, the comparison that decides
anything is the *whole* job — generated code with those checks against the walk with them — and
that number does not exist yet. It is the one to take next, and it needs no window if it comes out
the way the last one did.

## D75. Where the dictionary's generator lives, how it is asked for, and what it may say

**It is a package of its own**, installed beside the library rather than inside it. Most consumers
ship no dictionary — the standard schema is already compiled in — and an analyzer inside the library
package is a tax every one of them pays on every build. It also keeps the repository's own shape,
where an analyzer package is an analyzer package. The price is a name, a page and a skill, and one
condition: the two version in lockstep, and the generator's page says so, because a generator that
fills tables the library reads cannot be a version behind it.

**It is asked for exactly as a grammar is.** The dictionary is an additional file of the
compilation and an attribute on a partial class names it, so the path is fixed at build time and
nothing is asked of a consumer's object while parsing — D25 satisfied by construction rather than
by care. The generated class carries a ready validator as a property for the ordinary case and the
table beside it for someone mixing two dictionaries, which is D72's "the defaults must be
reachable" one level up. The name should read as the grammar attribute does. One member is added to
the public surface, a validator taking a prepared table, and it is wanted with or without the
generator: today two dictionaries cannot be mixed at all.

**And the rule about diagnostics is corrected rather than adopted.** The proposal was that a
diagnostic may name an element, a line and a tag but never quote the file, on the grounds that it
would put somebody else's words in the consumer's build output. **That reasoning is wrong, and it
is mine to correct because the sentence it came from is in D71.** The file in that build is the
*consumer's*: they hold it, their compiler reads it, and quoting a line of it back to them
redistributes nothing. The constraint that does hold is about *us* — nothing we ship or commit may
carry a dictionary's text, which is why the corpus copy is a corpus copy and why generated output
over it must not be checked in.

So the practice stands for a better reason: name the element, the line and the tag because that is
the more useful diagnostic, not because quoting is forbidden. Where quoting the text genuinely
helps a consumer fix their own file, it is allowed.

## D76. Generated validation is a quarter faster, and the rest of it is in one shared place

The whole job, both sides answering alike on all 186 fixtures before anything was timed, one
process, the largest fixture, three interleaved rounds:

| | ns |
| --- | ---: |
| the walk | 9,971 / 10,046 / 10,083 |
| generated | 8,184 / 8,023 / 8,105 |
| generated, without the value check | 5,585 / 5,261 / 5,289 |

**So the generated form is 1.24× the walk — a quarter, not a multiple** — and the decomposition
matters more than the ratio. Of ten thousand nanoseconds: some 1,930 is what the build road has
already taken, two table lookups a field becoming constants and the question of a length-and-data
pair disappearing; 2,720 is the value and code-set check, untouched because it is still a shared
call with a string switch over type names and a linear scan of a set that reaches ninety-four
entries; and 5,380 is walking the fields and the machinery of findings, which no arrangement
removes.

**The answer to what the build road buys: types, and a quarter.** The types were always the
product; the quarter is a bonus, and neither is a multiple. That is enough to write the generator
and not enough to expect it to change the shape of anything.

**And the largest movable block is in a place that serves both roads, which changes what to do
next.** Emitting those checks into generated code would mean reproducing the shared
implementation's semantics exactly — a quantity that may also be a code, a value that takes codes a
character at a time — and a divergence there breaks the equality everything rests on. **Fixing the
shared implementation instead has no such risk and makes the walk faster too**: a string switch
over type names where the schema already knows a tag's type, and a linear scan where a set could
be a switch. **Prefer repairing the one implementation to emitting a second copy of its
semantics** — one of them cannot disagree with itself.

So the ceiling of 1.87× is recorded as a ceiling and not a plan, exactly as its author framed it,
and the work it points at is taken on the shared code where it pays twice.

**And the prediction held, with its direction.** 1.3 to 1.8 was named with the likelihood of erring
high; 1.24 came out just below, and the reasoning under it was right rather than lucky — the second
prediction of the day and the first whose named direction survived.

**The shared repair paid eight per cent, a third of what the decomposition pointed at, and the
correction is to the decomposition rather than to the decision.** Two edits paid: a tag's type
travels as the byte the schema already holds instead of being written out as a name for the check
to parse back, and a tag's value set is remembered after its first ask instead of a search over
four hundred branches per field. On the largest fixture the walk went from about 9,850 ns to about
9,120 — some 750 ns.

**A third idea was mine and was worse.** I wrote that a linearly scanned set is either a switch or
a sorted search; the cheapest of that family — a sieve on length and first character before
comparing — came out *slower* than the plain scan, because the sets a message actually meets are
short, the comparison is already cheap, and the branches that saved comparisons cost more than they
saved. Reverted, with the reason written at the loop so that nobody tries it twice.

**And the morning's 2,720 ns was misread, by its own author, in a way worth naming.** Three
quarters of it is the character reading of 391 values — numeric, date, time, letters — which is the
work itself and does not disappear because it is emitted rather than called. **So the ceiling of
1.87× does not exist**: the generated form's advantage stays the same absolute 1,900 ns, now
against a walk of 9,100.

**The rule this leaves: a decomposition of time says where the time is, and says nothing about how
much of it is movable.** Splitting ten thousand nanoseconds into three true lines is one
measurement; attributing movability to one of them is another, and the second is taken with a probe
rather than read off the first. My own preference — "the shared repair gives more and is cheaper" —
was right about the order and wrong about the size, for exactly that reason.

The correction was added to the document as its own section rather than rewritten over the first
reading, which is the practice this journal has been keeping all day: a number that was wrong is
marked, not deleted, so the next person sees the road as well as the destination.

**Two shapes settled inside D75, and one public member that follows from D76.**

**The generator compiles the reader's source rather than loading the library beside itself.** The
obvious move is to put the library's assembly next to the analyzer; the better one is to compile
the same file into both. A second assembly loaded in another context turns "the two packages
version in lockstep" from a requirement on packages into a requirement on a loader, and **one
source compiled twice cannot drift at all**. That is the stronger form of the same guarantee and it
costs a file being linked rather than a dependency being carried.

**And the public member is granted, because refusing it forces the thing D76 forbids.** Generated
code lives in the consumer's assembly and cannot call an internal check; the only alternative is
emitting a copy of that check's semantics, which is exactly what the measurement said not to do —
three quarters of its cost is reading characters, so a copy would be both risky and pointless. So
the check gets a public entry: an enum of value types rather than the byte the schema carries
internally, a validity call, and a tag's type by number.

It earns its place without the generator too. A consumer writing one rule of their own has no way
today to ask whether a value fits its FIX type except by writing it again, and the only public
way to ask a tag's type returns the *name* — the string that was taken off the hot path this
morning — so the fast form being internal and the slow one public is backwards.

One condition on the shape: if both survive, they are named apart — the type by one name and its
name by another — rather than distinguished by return type. Breaking either is free until the
release is cut, which is one more reason to get the naming right now rather than to add a second
way later.

## D77. The generator works end to end, and the test that makes it a test of the generator

A real dictionary, a real analyzer, a real build: a class of three lines becomes a hundred and
thirty-one thousand lines of rules, and nothing generated is committed. **Both roads are given the
same file**, which is what turns the comparison from another reading of two dictionaries into a
test of two implementations: a disagreement is then a defect in one of them and never a difference
in the data. On all 186 fixtures they now agree exactly.

**And getting there found a defect that only that shape could find.** The walk distinguishes "the
schema has no such tag" from "it has it, but not in this region"; the generated code said one
sentence for both. Two roads over one dictionary answering in different words is a difference a
reader would see and could not explain, so **the wording turned out to be part of what has to
agree** — which a comparison by rule and tag would never have reported.

**A duplication was prevented rather than found.** The mapping from the dictionary's spelling to a
type nearly existed twice, once for each road, and the two would have drifted *silently* — a tag
left unchecked rather than a break. It is one file compiled by both, which is D76 applied before
the fact instead of after.

**And one rule reads from the dictionary rather than from us**, correctly: the encoding rule takes
its set of encoded fields from the file, because for a venue that set is theirs. For the standard
version the two coincide, and the cross-check says so.

**A sixth package means three lists must grow, and nothing says so when they do not.** The publish
workflow packs five projects by name, the shape check names the libraries one by one, and the
library smoke loops over four of them. A package missing from those lists is not packed, not
checked and not smoked, and the failure is silence. The smoke for the new package therefore comes
before the remaining work of the line rather than after it: a package that has never been installed
by somebody else is the one thing we know breaks at publishing time.

Then the time is taken again, the walk having got eight per cent faster this morning and the
generated form sharing the repair, so the ratio measured before it is no longer the ratio. The
second argument of the attribute — generating only the types a consumer names — stays last: the
arithmetic already says it is not needed to make the shape affordable.

**D77 said three lists and there are five, which is my miscount.** Each of the two workflows keeps
its *own* pack list and its *own* smoke step; only the shape script is shared. I read one workflow
and counted its list, its loop and the script, then wrote "three" as though the other workflow did
not exist. All five have grown, both workflows parse, and the analyzer smoke is an analyzer's
rather than a library's: install the package, put a dictionary in as an additional file, write
three lines and see the class come back with rules.

**Its dictionary is its own**, small and written here — borrowing the corpus copy to test a package
that ships nobody's dictionary would be the package doing the thing it refuses.

**And running it, rather than writing it, found two things.** The shape script refused the new
package for having no release notes, on the packed file, before any of it reached CI — the check
being worth exactly what it is worth before the event. And the first run failed with a wall of
missing types out of the generated file, which is not a defect at all: a package of the same
identifier and version was already in the global cache from an earlier life, so the old library
was served beside the new generator and the rules referred to types it did not have. **A stale
artifact of the right name does not look stale; it looks like the thing you just wrote is broken**,
and half a minute went into believing that. It is written into the development page with the shape
of the failure named, next to the half that was already there.

## D78. Igor: the published specification is the authority, and it is read

The three lists of disagreement between a published dictionary and our compiled tables are settled
by reading the specification, not by preferring either reading. Neither the file nor our tables is
evidence about the protocol; both are readings of it.

**Order by consequence rather than by list.** Two of the seventy-seven are *spellings* — a value
written one way here and another there — and a value spelled differently is a value that does not
pass, so they are false rejections of correct messages and go first; they are also minutes of work.
Then the twenty-six on composition, because a message required or not required is an accept or a
reject of real traffic. Then the ten types, which decide how a value is checked. Then the rest of
the code sets, where ours is mostly the richer and the failure is under-strictness rather than a
wrong answer.

**Each row closed carries its citation.** The section of the specification that decided it goes
beside the row, in the same commit that corrects the tables and the list — otherwise the next
person who doubts a row reads the specification again, and the one after that does it a third
time. That is the rule this repository already keeps for the SQL grammars: a rule written from the
published syntax says where it came from, and a transcription from somebody else's parser proves
nothing.

**And a row may close in either direction, including neither.** Ours right, theirs right, or the
specification saying something neither says — the last is the interesting case and the one a list
of two readings cannot reach on its own.

**The shipped-page test is flaky by construction, and the merge only changed the order that exposed
it.** Its reference set is "every assembly this process has loaded" — read once, when the property
is first touched. Nothing guarantees that the assembly a page names has been loaded by then: it is
loaded lazily, on first use, and whether some earlier test in the same process happened to touch it
decides the answer. Twenty-three blocks of the web and expression-language pages failed here with
"the type or namespace does not exist", and the same blocks pass elsewhere, on the same code.

**So the check answers a question about load order rather than about the page**, and it passes
today by luck. The fix is to name what it needs instead of collecting what happens to be there — a
type from each shipped assembly, touched before the set is read, or the references gathered from
the test's own dependencies rather than from the domain. This is the day's own rule arriving in a
new place: a measurement that takes what it finds is measuring the finder.

## D79 — A check that measures time reports; it does not gate

The refusal guard was run twice against one build of one tree, with nothing changed between the
runs. The first run failed three series, the second eleven, and the two failing sets had two rows
in common. The verdict was not a property of the tree.

That is not a threshold being slightly wrong. A series whose baseline is linear came back
superlinear in one run and passed in the next, and the set of series it happened to was almost
disjoint between the runs — so the quantity being compared to the margin varies by about as much
as the margin itself. The guard was calibrated on explosions, where the exponent is five to seven
and noise cannot reach the answer, and on a single quiet reading of the baseline; its spread on an
unchanged tree was never measured. Calibrating a discriminator only where the classes are far
apart tells you nothing about where they are close, and the whole value of this guard is the
series that are close.

**So the rule.** A check that gates a build must be reproducible on an unchanged tree. A check
whose reading is a measurement of time is not, and the two cannot be reconciled by choosing a
better threshold: the threshold can only be chosen once the spread is known, and if the spread is
comparable to the distance between the classes, no threshold exists. Such a check reports — it
prints what it saw, the build stays green — and what gates is only what does not depend on how
busy the machine was: the reader threw, the input was accepted that should have been refused, the
series is missing from the baseline, the call did not finish inside the watchdog, or the exponent
is high enough that no amount of noise could have produced it.

**Why this and not simply loosening the margin.** A check that fires at random teaches the people
who see it to disregard it, and a disregarded check is removed — taking with it the reason it was
built. This guard found eleven real explosions in a week, two of which nobody had listed. Its
value is the finding, and a finding survives being printed; it does not survive being distrusted.

**What a report must still do.** Repeat before it speaks. A series read as worse is read again, up
to three times, and is called worse only if it was worse every time; a series worse on some runs
and not on others is named as unstable, which is itself a fact about that series worth having.
Nothing in this is a reason to stop measuring — it is a reason to stop pretending a measurement is
a verdict.

**Corrected the same evening, and the correction is ours.** The stand then measured what this
entry said had never been measured: five runs of the ladders alone, on one unchanged build. One
series of a hundred and one changed class, the median range of the exponent over five runs was
0.02, the widest 0.30 — and against the guard's own margins that set would have failed nothing.
So the ladders are not what is unstable, and the sentence above that reasons from "a measurement
of time" to "cannot be reproducible" proves too much: on a quiet ladder the measurement
reproduces, and had we measured before generalizing we would have known that.

**What the two readings together say.** Something about running the guard *inside the whole slow
suite* — in one process, after the streaming and memory tests, against a baseline taken on a tree
before the retention work — produced eleven failures where the ladders alone produce none. That
is a fact about the setting, not about timing as such, and which part of the setting is doing it
is now the measurement to take rather than the conclusion to state.

**The rule survives, on narrower ground.** What gates a build must be reproducible in the
condition it gates in, and reproducibility there is shown, not assumed. The reporting form and
the repetition stand for that reason alone; if the cause turns out to be a stale baseline, the
right repair is a fresh baseline, and the guard may gate again.

**And the cause was found the same evening, which closes it.** On a loaded machine a fresh-process
reading of one series gave 1.19, 1.22, 1.30 — and then 5.2, 45.4, 11.4, 37.2, 3.4. An exponent of
forty-five is not a series drifting; it is ONE stalled point at the largest size, and a
least-squares line through a short tail is moved by one point as far as you like. So the eleven
failures were single outliers, not noise in the measurement, and the earlier reading of them as
"the measurement cannot be reproducible" was wrong twice over: the quantity is stable, and the
estimator was what let one bad reading through.

**What that changes.** The repair is the estimator, not the margin: the exponent is now the median
of the pairwise slopes over the tail, which is unmoved by a third of the points being wrong, and
each series is walked twice with the second walk read, so a series stops carrying whatever the
process did before it. A least-squares line over a handful of timed points is the wrong instrument
for a quantity whose worst error is a stall, and that is worth knowing away from this guard: where
a measurement's failure mode is a rare huge outlier, an average of any kind is the wrong summary.

**One consequence to carry out, not to forget.** Changing the estimator changes what the baseline
means, so the baseline is retaken — and it is retaken from the guard's own process, since the same
series reads differently in a fresh process and in a warm one. The file says which process and
which estimator produced it, because a baseline that does not is a number nobody can check later.

## D80 — The FIX package's framing is a value, not a second set of names

`FixParser` publishes `Parse` five times and `ParseLog` five times: half the methods are the other
half with a different field separator. One layer down, `FixFieldOptions` is a value passed to one
method — `Parse(string, FixFieldOptions?)` — and every reading goes through it. So the package
spells one question two ways: the lower layer holds the choice in a value, the upper one holds it
in the name of the method.

**The framing moves into the value, and `ParseLog` goes.** Which separator a stream uses is a
property of the input being read, not of the call the consumer wants to make; a property of the
input belongs in the options that describe the input. Ten methods become five, and the idiom the
package already uses one floor down is the one it uses throughout.

**What must survive the move is discoverability, and it can.** `ParseLog(text)` reads well and is
found by anyone scrolling a completion list; an options flag is not. So the options type carries a
named value for it — `Parse(text, FixFieldOptions.Log)` — which is as short, as findable, and one
thing rather than ten.

**The overloads that only copy stay.** Six methods take a `ReadOnlySpan<char>` and call
`ToString()`. They are documented as copying, and the cost is visible where it is paid; removing
them would move that cost to the consumer's own `ToString()` without removing it.

**And the lesson the recount taught, which outlives the finding.** The review that produced these
points was written against the package as it stood before its parse modes were removed. One of its
arguments no longer existed — and its number, ten methods of twenty-three, came out the same
anyway. Had the number been carried over rather than counted again, the conclusion would have been
right by accident, and a week later nobody could tell the accident from the reasoning. A review of
an area that has changed under it is recounted, number by number; checking that its conclusion
still sounds right is not the same act.

**Which settles the other options type, and not by waiting for the dictionary.** Fold the framing
in and `FixParseOptions` holds one enum and nothing else, and the question was put whether to keep
it until the dictionary work says where schema-level state will live. That is deciding by
prediction. Decide it on what each type answers instead: a field separator is what delimits a
field, which is the field layer's own fact, not a message-layer one. It belongs beside the per-tag
table on the merits, today, and the type left behind is empty because its content was never its
own. So the package has one options type, and it keeps the name it has: the owner checked rather
than guessed, and `FixFieldOptions` governs how the fields are read, of which what separates one
field from the next is a part. The wrapper goes; the message layer takes the same type the field
layer does. A consumer with both a dictionary of their own and log framing writes the constructor
— the common case is a named value, the particular one is still expressible. If the dictionary work then brings schema-level state,
it lands where the tables already are, which is the same type — not a re-split.

## D81 — A generated table cannot show what was never read

The carriers page is written from the reports a build leaves. Ask for it after building one
project and it comes out complete-looking: thirty-five grammars, every column filled, nothing on
the page saying that the solution has more. The reader is not misled by a wrong cell — there is no
wrong cell — but by the *shape* of the table, which says "this is the set" because that is what a
table says.

**Emptiness and absence are different, and only one of them a table can carry.** A blank cell is
an answer: a grammar read with the immediate carrier has nothing to say about a gate, and the
column is rightly empty. A grammar that was never compiled has no row, and no arrangement of rows
can show that. So absence has to be said in words, beside the table: this page was read from these
projects, and a short table here means a short build rather than a grammar with nothing to say.

**The same holds wherever we generate a document from what a run happened to find** — the
carriers page, the coverage page, any future one. Each says what it was read from and how much of
it there was, so that a reader can tell a small answer from a small question. The alternative, and
it is the one that bites, is that a page which is merely incomplete reads exactly like a page
which is complete — and unlike a wrong number, nothing in it looks wrong.

**It is the day's own rule in a third place.** A test that takes what the process has loaded
measures the finder; a document generated from what a build happened to leave describes the build.
In both, the defect is invisible to whoever produced it, because for them the thing was there.

## D82 — Where two implementations share names and types, the compiler guards nothing

Collapsing the FIX message layer meant rewriting every call: `ParseLog(x)` became
`Parse(x, FixFieldOptions.Log)`. The rewrite ran over `HandFixParser` too — the hand-written
parser in `examples/`, whose API did not change and still has both methods. And
`HandFixParser.Parse(x, FixFieldOptions.Log)` compiles perfectly, reading the wire where the log
was meant. Silently: the new argument is valid for the old method, so nothing was wrong to say.

**That is the dangerous shape of an API change, and it has a name worth keeping.** Not "the
signature changed" — that the compiler catches, at every call, for free. The one to fear is a
change after which the *old* call is still legal and means something else. There the compiler is
not weakly helpful; it is silent by construction, and every mechanical rewrite across that
boundary is unchecked.

**What caught it was the differential test**, comparing the generated parser against the hand
written one: a field came back `Invalid` where the other implementation read it. Nothing was
looking for this defect; the comparison found it because a comparison does not need to know what
to look for.

**So the second implementation is a correctness instrument, not only a measuring one.** The
hand-written parsers were kept for benchmarks and for differential tests, and their standing rule
is that no ordinary test reads them — they are run by hand after being touched. Today one of them
caught a defect in shipped behaviour that the whole suite would have carried. That does not
overturn the rule, but it prices it: what the rule buys is build time, and what it costs is the
window in which a change like this one lives undetected. Where an edit crosses that boundary — a
rewrite of calls, a change of what an argument means — the differential run is part of the edit,
not a thing to do later.

## D83 — The path in `#line` is one fact, and it is answered by the toolchain's path map

Three sessions met the same thing through three instruments. The size measurement cannot normalize
the repository root out of `FileBytes`, because the text inside the embedded symbols is the
unnormalized one. A shipped package's symbols name the disk of the machine that built them. And a
relative `#line` path cannot be made to work at all: it resolves against the *generated* file's
directory, which the compiler invents even when nothing is written to disk, and which carries the
configuration and the target framework — so the number of `..` would differ between a Debug build,
a Release build and a multi-targeted one, which the generator cannot know because it runs before
the compiler decides where, or whether, to write the file. That was measured, in four builds, not
reasoned about.

**So the choice is not relative-against-absolute. It is navigation against anonymity, and the path
map gives both.** A build that sets deterministic source paths has its paths mapped; one that does
not keeps them absolute. That is exactly the right split, and it is the split the toolchain
already draws: the person building locally has those directories and wants to click an error into
their grammar; the artifact that ships has no business naming anybody's disk. We honour a
mechanism the consumer already controls instead of inventing one.

**What changes in the earlier arithmetic, and it is the argument that moved me.** This shape was
priced before as "closes one of three goals" and set aside. It closes the goal that SHIPS. A
package a consumer downloads is built in CI; local reproducibility and the size rule are about
what we do on our own machines, where the leak harms nobody. "Closes one of three" and "closes the
one that ships" are the same fact read two ways, and the second reading is the one that matters.

**And this decides the other two, so it goes first.** What a shipped package says of the build
machine is answered by it; what a size measurement can compare is narrowed by it — inside CI the
bytes become comparable, on a developer's machine the rule "compare only within one worktree"
stands, with its mechanism now named rather than obeyed. Neither of those is settled ahead of
this one.

## D84 — The staleness guard answers the consumer's question, not the maintainer's

A dictionary loaded at run time can disagree with the tables compiled into the package. D71 asked
for a guard. Reading the existing reader first — as the order of work required — changed what the
guard should be.

**What the reader does today with a disagreement is nothing, deliberately.** It sets a rule for
every type the dictionary describes and never consults the compiled tables; two schemas coexist
and the last write wins. That is D72's shape — the consumer's table, and the package does not
argue with their entry. So the guard changes no behaviour; it is new, and the only question is
where it lives.

**And the consumer can reproduce none of our three disagreement lists**, because the schema is
internal: the schema type, its members, the composition array, and the vocabulary that maps a
spelling to a value are all internal, and the one line reachable from both sides does not agree in
shape. Ten disagreements on types, seventy-seven on code sets, twenty-six on composition — a
consumer can ask about none of them.

**What they can do is what found the most.** Read a corpus, ask both validators, compare the
findings: the strongest check we have is over messages, not tables, and it is public today —
parsing, validating, and every field of a finding. A guard written that way needs no new member.

**And it answers a different question, which is the one actually asked.** "Does this new file
change what my validation says" is not "do these two descriptions differ": they may differ in a
tag the consumer never sends. A table comparison returns seventy-seven code sets and leaves the
reader to work out which matter; a message comparison returns only what showed up in their own
traffic, in the words they already read.

**The price is named beside it.** Such a guard sees only what the corpus touches: a dictionary
that changed a message type they have never received stays silent until the day they receive one.
That is the opposite trade from tables — complete and undirected against partial and to the point
— and the recipe must say which of the two it is.

**So: the recipe first, and the table comparison only if somebody asks for it** — and then as a
decision about how much of the schema the package publishes, taken as that decision, not arrived
at while building a guard. Publishing a tag's code set, a tag's name, a message's composition and
the spelling vocabulary is four or five members and a public shape for a schema reference, which
is a larger promise than a method.

**Half of it already exists, from the maintainer's side.** Our three lists are a guard, fixed in
the suite, and they fire when our tables and the published dictionary disagree. What is missing is
the consumer's half, which is the half this decides.

## D85 — The emitted code's standard is what the machine must do, not what a reader would tidy

Counted over the whole shipped generated output — forty-four megabytes, 4,797 methods — there are
299 never-read locals, 250 copy chains, 23 empty blocks, and zero unreachable statements and zero
unused labels. The zeroes are not care: unreachable code is CS0162 and an unused label CS0164,
both warnings, and this repository builds warnings as errors. A never-read local survives because
CS0219 fires only on a constant initializer, and an array element is not a constant.

**So everything the compiler polices is clean and everything it does not police is not, and
nobody chose that.** Left alone, the tidiness of what we emit will drift with Roslyn's warning
list. That is the thing to decide, and the answer is neither "the compiler's warnings are the
standard" nor a campaign for tidiness.

**The standard is: the emitted code contains nothing the machine is obliged to execute for
nothing.** It is a criterion about cost, it does not move when a warning is added or removed, and
it sorts the shapes found here by itself.

**Which it does, into two items rather than one.** A copy chain is removed outright by the JIT: it
costs bytes and no time, so it is a size item, and where 240 of its 250 sit in three grammars it
is probably one emitter site rather than a habit. A never-read local reading an array element is
not removable — the bounds check can throw, so the JIT must keep it — and twelve array accesses
are performed for nothing; worse, were the index ever out of range the parser would throw from a
line that does nothing. That one is a defect in what we emit, not untidiness, and eighty of them
in one parser's hundred and sixty-seven methods is a pattern, not a slip.

**The withdrawn half belongs in the record too.** The argument that these locals also cost time
through zeroing the frame was carried from a figure measured on materializer arms — small methods
entered constantly — to reader methods entered once per rule per position. A ratio moved to
another material; withdrawn by the session that made it when the other caught it. The size effect
is the result, and it is not a proxy for a speed one.

**Two corrections to the pair above, and the first is not a quibble.** "Obliged to execute" is a
property of the JIT, not of the emitter: whether a load survives depends on what is provably
non-null in that method after inlining, which varies with the runtime and with what else the
method touches. The generator cannot ask that while generating, so as a principle it is right and
as a test it cannot be run — and a standard that cannot be run is applied by eye, which is how the
floor came to be an accident in the first place. **So the enforceable form is one step weaker and
the emitter can decide it alone: no emitted expression whose value is never read and whose
evaluation can fault.** Writing an array index or a load through a reference and discarding the
value is something the emitter knows it is doing. The principle says why; this is what is checked.

**And the pile the criterion excludes still needs a home.** By this standard a copy chain is not a
defect, and that is right — nothing is executed for it. But then it belongs to nobody, and a
criterion that sorts findings into defect and not-defect quietly retires the second pile. It is a
SIZE item, judged on the instrument that exists for size and not by eye: it becomes work when one
emitter site accounts for the bulk of it, so that the repair is a change rather than a campaign,
and the figure before and after is read off `DotGram.CodeSize`. When it is scattered instead, it
is recorded with its location and left — recorded, because the cost of not writing it down is
that the next person discovers it again as news.

**One condition on the path map, so that what symbols are for is not traded for what they cost.**
Mapping the paths makes a shipped package stop naming a build machine; it does not by itself keep
a consumer's stack trace landing somewhere they can open. Embedded symbols were bought for exactly
that. So the mapping goes in together with source link, and the grammars it points at are in the
repository already: mapped without it, the symbols are anonymous and useless; mapped with it, they
are anonymous and still navigable.

## D86 — A property travels by the likeness of names, not of mechanisms

Three times in one day, in three sessions, the same mistake with three faces.

A figure measured on the materializer's arms — small methods entered constantly — was carried to
the reader's methods, which are entered once per rule per position, and made an argument about
what a local costs. The spread of an exponent measured on quiet ladders was carried to the whole
slow suite and made an argument about what the measurement can do. And "a refusing parse
materializes nothing", true of the listing loop in a cubic case, was carried to "a refusing parse
records nothing" — a different stage of the same parse, and false: two refusals of three write
records, quadratically.

**Each carried a property across a boundary the name does not show.** Arms and readers are both
"methods"; a ladder and a suite are both "the guard running"; materializing and recording are both
"building". The likeness is in what we call them. The mechanisms are not alike at all, and nothing
in the sentence warns you, because the sentence was true where it was made.

**The cure is the one all three used once caught: measure on the material you are speaking
about.** Not "does this argument still sound right" — it will, the words are the same — but the
count, taken again, on the thing now under discussion. Each of the three took minutes.

**What this asks of a decision that rests on a figure.** The figure carries where it was taken, in
the sentence that uses it, not in a footnote: on which family, at which stage, in which process.
A number without that is a number that will be moved, because nothing in it resists the move. And
a claim whose evidence was gathered elsewhere is a hypothesis, however well it reasons — which is
the day's other rule seen from the front: what a measurement cannot see, it also cannot forbid.

**Two additions the evening's work earned, one on each.** A generated page states facts and not
verdicts: the carriers run now records when each project's report was written and prints the span,
because a date beside a project's name is something the run found, while "stale" would be a
comparison — against which build, on a machine where two target frameworks write the report twice?
— and a wrong verdict in a written page is worse than none. The reader who sees one date two days
older among a column of today's needs no help from us.

**And the size instrument settles its own comparability rather than inheriting a rule.** What the
path map changes is not where a figure may be compared but under which setting it was taken: two
builds that both map their paths are comparable anywhere, and a build that does not is comparable
only against another on the same machine in the same worktree. So `DotGram.CodeSize` asks for
mapped paths itself, and then its numbers are publishable by construction — instead of every
reader having to remember the range a figure was sound in. That is the cheaper half of today's
lesson about numbers read further than they were taken: an instrument that cannot produce an
unpublishable number needs no rule about publishing.

## D87 — A hypothesis killed by a count, and the count's own three near-misses

Keeping the log instead of rolling it back — reusing a doomed attempt's records the way a replay
reuses its decisions — is **unsound, and the number says so without argument**: of the segments
discarded and written again, not one comes back identical. It cannot: a give-back reads one turn
fewer, so the tail must differ. That closes the shape as stated, and closes it in an hour rather
than in a week of it half-built.

**What the same count opened is narrower and better founded.** The difference is confined to the
tail: between 52% and 91% of the discarded entries are written back unchanged, and the share
CLIMBS with the input — 57 to 73, 52 to 71, 84 to 92 per cent as the input goes from sixteen units
to sixty-four. One shape writes 111,312 entries at sixty-four units, discards them, and rewrites
91.5% of them identically. The climbing share is the signature: the repeated prefix lengthens
while the differing tail stays about constant. That is the quadratic, seen from the log instead of
from the calls — the same illness this whole line of work is about, showing up in a second
instrument, which is how one knows it is the illness and not an artefact.

**So the shape to consider is not "keep the records" but "roll back only the tail".** It is not
approved here and wants its own design; the question to answer before designing it is whether the
marks the rule already takes per turn locate that boundary, in which case the change is small and
local rather than a new meaning for the tape.

**And the three near-misses of the instrument belong in the record, because each would have been
believed.** Keyed by the log mark alone, every difference reported "first at 0" — which is where
every top-level attempt starts, so unrelated turns were being compared. Keyed by mark and
position, a gate on the method's shape silently dropped 249 of 280 roll-backs, precisely the ones
that matter, leaving a run with one per cent coverage reporting "none differ". Compared whole
segments, everything differed and the idea would have died on a technicality.

**A measurement with one per cent coverage reads exactly like a clean result**, and a conflated
key reads exactly like a finding. Neither announces itself. What caught all three was the same
question asked three times: what would this number look like if my key were not measuring what I
think it is? That question is cheap, it is asked before the result is believed rather than after
it is doubted, and today it saved three confident wrong answers in one hour.

**Withdrawn the same hour, and the withdrawal is ours.** The paragraph above told the size
instrument to ask for mapped paths so that its numbers would be publishable by construction. It
was measured instead of taken on trust, and it is wrong twice. `DotGram.CodeSize` weighs the
assembly, and the assembly came out byte-identical — 1,259,008 in all four builds — from two
worktrees whose roots differ by a hundred and one characters: the path lives in the portable
PDB's document names, which is a separate file. And the flag could not have worked anyway, for
D83's own reason: a `#line` path is literal text the generator writes before the compiler decides
anything, so mapping on both sides left the two PDBs 3,636 bytes apart. Only the emitter honouring
the map removes it.

**What the rule is actually about is the generated source figure**, and there the whole difference
is the path string: 2,727 bytes between the two worktrees, which is 27 directives carrying the
absolute path times the 101 characters of difference, exactly and with nothing else varying. So
the rule narrows to the "bytes UTF-8 C#" the report prints, and an assembly built with a separate
PDB was never the problem — that sentence belongs in the rule, because obeying a rule whose reason
is wrong is how a rule outlives its cause.

**The exception is the one that ships.** The packages emit embedded symbols, so in a packed
assembly the paths do ride inside the DLL and its size is path-dependent. That is D83's own
target, and it means the emitter honouring the map normalizes what ships as well as what we
compare — which is the same fact as before, now with the boundary drawn where the measurement put
it rather than where I guessed.

## D88 — Compiling a page catches a dead name, never a wrong claim

Three pieces of shipped documentation were found stale in one day, each found on the way to
something else: a validator's member explaining itself through a type deleted that morning,
release notes missing the day's third break, and a README section titled "Validation policies"
holding a table of two modes that no longer exist. The largest change of the day was described
nowhere and contradicted on the package's own front page.

**All three passed every check we have**, because the checks are the wrong shape for the fault.
The suite was green; the page's code blocks compiled. Compilation is an excellent detector of a
dead *name* and is blind to a wrong *claim*: a table of two modes compiles perfectly, since it is
prose, and prose is what carries the assertion. The one thing that would have caught it — reading
the page — is the thing an edit does not do.

**So the rule sits on the edit, where the knowledge is.** An edit that removes or renames a public
type or member re-reads the pages for the CONCEPT and not only for the identifier: the mode, the
policy, the default, the "either … or" that the removal collapsed. The compiler finds every
mention of the name for free; nothing finds a mention of the idea, and the person who removed it
is the only one who knows what the idea was called.

**Why this belongs beside the rule that official pages carry no history.** A page that keeps no
record of what used to be true has no way to say "this section is about the old design" — it can
only be right or wrong. That is the price of the form, and it is worth paying, but it means the
page's correctness rests entirely on the discipline of whoever changed the thing underneath it.

**A hash a file writes about itself cannot be right when it is needed.** Twice today a results
file named the commit of the branch it was written on, and both times the rebase that landed the
file changed that commit — so the header pointed at a hash nobody can resolve, which is worse than
naming nothing, because it looks checkable and costs whoever checks it their time. The cure is not
more care: a header names what a rebase cannot move — the published `origin/main` commit the
measurement was taken AGAINST, and a fingerprint of the thing measured, the binary or the library
file. Where the file itself ended up is said by the commit that lands it, and that is written by
somebody other than the author.

## D89 — A chosen cost belongs on the page where the choice is offered

One rule's body is emitted once per publication: 138 groups of byte-identical methods across the
shipped grammars, 252 redundant copies, 73.3 KB of source. The copies sit in different types —
one reader struct per publication, each method reading that struct's own fields — which is what
buys the direct access and the absence of dispatch. By D85 this is not a defect: every publication
executes its own copy and executes nothing in vain.

**But it is a price, and the price has never stood beside the benefit.** The multiplier is how
many ways a grammar publishes, and the sharpest pair says it without argument: T-SQL contributes
19.3 KB of duplicate source and SQL:2023 none. They are comparable grammars; the difference is not
size or complexity but the number of publications. A fifth way of publishing costs bytes that
nothing today quotes, and it is paid twice — as IL and inside the embedded symbols.

**So this is documentation, not a task, and the rule that sorts them is the one to keep.** An
ACCIDENTAL cost — a copy chain the emitter did not mean to write — is an item, and it is fixed at
its one site. A CHOSEN cost is a consequence of a design working as intended, and the repair for
it is a sentence on the page where the choice is offered, so that whoever adds a publication reads
what it costs at the moment of adding it. Deduplicating here would trade away exactly what the
per-publication struct exists for, which is why nobody is proposing it and why the finding has no
owner otherwise.

**One part of it is not chosen, and that part is an item.** A method whose body touches none of
the reader struct's fields — a stack check appearing five times in one grammar — is duplicated for
no reason the design needs, and the emitter can tell which those are: it knows whether a body
reads a field. Sharing only that subset takes nothing away from the direct access, so it belongs
in the other pile.

**Counted, before the decision rested on it: the exception is an eighth.** 31 of the 252 copies,
7.1 KB of the 73.3 — so the sentence on the page names about 66 KB, and the eighth is a small size
item rather than a revision of the price. Two instruments agree without having been written to
agree: 221 + 31 is 252 and 66.3 + 7.1 is 73.4, against the first count's independently taken 252
and 73.3. For the static methods the test is exact — a static method cannot read an instance
field, so duplicating it buys nothing the struct exists for. The few that rest on "the body names
no field" are weaker, since a field declared below the method or inherited would be missed, and
they are re-checked before anything is merged.

**And the distinction generalizes past the generator, which is the part worth carrying.** A
finding about a chosen cost keeps landing in a size report, where it is true, unowned and
unactionable; a year later someone finds it again and writes it again. It changes nothing not
because it is wrong but because it is in the wrong place. Beside the choice — in the page that
offers the option, in the words someone reads while deciding — the same sentence does work every
time the choice is made. That is the difference between a review that accumulates and one that
repeats.

## D90 — The FIX messages are nested in `FixMessage`, as the fields already are (Igor)

Igor's instruction: every class deriving from `FixMessage` becomes a nested class of it, the way
an algebraic data type is written. Ninety-three of them, today declared at namespace level in
`FixMessageTypes.cs`.

**The package already writes it that way one layer down.** `FixField.SettlDate`,
`FixField.Custom` — the fields are a closed hierarchy nested in their base, and the messages are
the same kind of thing: a fixed set of cases, each a case of the base, exhaustively matched by
whoever reads them. Two shapes for one idea in one package is what this removes, and it is the
same argument that collapsed the framing into the options: not narrow against wide, but a layer
not following its neighbour's idiom.

**It reads at the use site as a closed set rather than as ninety-three loose names.**
`FixMessage.NewOrderSingle` says what it is where it is written; `NewOrderSingle` alone says only
that somebody named a type. Nesting also puts the base's name in front of every arm of every
`switch`, which is what makes a hierarchy look closed at the place it is matched.

**The price is one public break, and it is free before release.** Every name a consumer writes
changes, which is the kind of break a version number is for — and this package has not been
published, so it costs nothing but our own call sites. After publication it would cost everyone
who had written against it, which is why it is done now rather than considered later.

**Two things go with it, and neither is optional.** The pages name these types throughout, and by
D88 the edit re-reads them for the CONCEPT — "a message class", "the message types" — and not only
for the identifiers the compiler will find. And the release notes gain their fifth break, in the
same commit as the change, since a break recorded later is a break somebody met first.

**Two refinements the work found, and both are general.** Only CASES of the type nest inside it.
`StandardHeader` and `StandardTrailer` derive from `FixFieldSet` and are regions WITHIN a message,
not messages; the first pass drew them into the nest with the rest and they came back out. A
non-case inside the set weakens exactly the claim the nesting is made to carry — that everything
in here is one of these. And `CustomFixMessage` belongs in it for the opposite reason: it is the
case for whatever the schema does not describe, and it is what lets the set be CLOSED without
being complete.

**And a nested case does not repeat its outer name.** The idiom cited for this decision is
`FixField.Custom`, never `FixField.CustomFixField`: nesting exists so that the outer name carries
the prefix at the use site, and a case that repeats it gives back what the nesting bought and
stammers while doing it. So `FixMessage.Custom`. The rename rides in the break already declared,
which is the whole reason it is free: a second break announced a week later is not.

## D91 — "Free before release" was a claim about the world, and nobody looked

Five packages are on nuget.org at 0.1.0: `DotGram`, `DotGram.Sql`, `DotGram.Web`,
`DotGram.Finance` and `DotGram.ExpressionLanguage`. `DotGram.Finance.Generator` is not. One
request to the flat container says so, and the day's decisions were taken without it.

**So the price line of D80 and D90 is wrong, and it was wrong in the same sentence both times:**
that collapsing the options type, removing `ParseLog`, nesting the ninety-three message classes
and renaming `CustomFixMessage` cost nothing "before release". They are breaking changes to a
published package. The release notes were written on the same premise, by the session I told it
to.

**The decisions themselves stand, and that distinction matters.** Each was argued from what the
package already does one layer down — the neighbour's idiom, the closed set at the place it is
matched, a nested case not repeating its outer name. None rested on the cost being zero. What
changes is what we now owe: a version, and words.

**What follows, concretely.** The version in `Directory.Build.props` is still 0.1.0, so the next
tag is `v0.2.0` and the bump comes first — the tag-is-version check would have caught a `v0.2.0`
tag against it, which is the one thing in that workflow that cannot be taken back. Under 0.x a
minor may break, so 0.2.0 is the right number. And every break in the notes gains the line that
makes it a migration rather than an announcement: not "`ParseLog` is gone" but what to write
instead. A consumer reading a break wants the replacement in the same sentence.

**And the policy's scope follows the same fact.** Because the generator package has never been
published, the first trusted-publishing run needs "push new packages and versions"; it narrows to
"new versions only" afterwards, which then makes publishing a new package ID from CI impossible —
the release mistake nobody can take back.

**The lesson is the day's own, arriving where it was least expected.** We spent the day refusing
to carry a figure from one material to another without measuring, and the one premise nobody
measured was whether the thing being changed was already in somebody else's hands. It was
cheaper to check than any number we took: one request, no window, no instrument. A claim about
the world is not made true by everyone repeating it, and "this is not published yet" is a claim
about the world.

## D92 — No new projects; the validators are compiled by the expression language at run time (Igor)

Igor's instruction, and it reverses D74 and D75. **New projects are not created, public ones least
of all.** And what he asked for in the first place was this: `DotGram.Finance` references the
expression language and the code that builds validators uses it as a library — validator text
composed from the dictionary and compiled into lambdas, in process, when a dictionary is loaded.

**What I did with that instead is the part worth naming.** He said, in so many words, that we have
the expression language and can compose validation as text and compile it to lambdas. I turned it
into a build-time analyzer in a package of its own — which answers a question he did not ask, and
answers it with the one thing he now says is forbidden. The reasoning inside D74 and D75 is sound
about the thing it was reasoning about; it was reasoning about the wrong thing.

**Why the run-time road is not a lesser version of the other.** He also said that time at startup
does not matter and the speed of validation does. Compiling once when a dictionary is loaded pays
at startup and gives the same executable code the analyzer would have emitted — so the property
that made the compiled road worth having is kept, and the package that carried it is not. It also
removes what the analyzer made awkward: one reader shared as source across two compilations,
version rules that turn into loader rules, a sixth package in five publication lists.

**What goes.** `src/DotGram.Finance.Generator`, `tests/DotGram.Finance.Generator.PackageSmoke` and
`tests/DotGram.Finance.Generated`, with their entries in the solution, in both workflows' pack and
smoke lists, in the layout in CLAUDE.md and in the pages that name them. None was ever published,
so nothing breaks outside this repository.

**What arrives.** A package reference from `DotGram.Finance` to `DotGram.ExpressionLanguage`, and
with it a cost to state plainly rather than discover: every consumer of the FIX package now
restores the expression language too. That is a consequence of the instruction, not an objection
to it, and it belongs in the release notes of 0.2.0 beside the breaks.

**And the rule reaches today's other new project.** `tests/DotGram.ExpressionLanguage.LoadOrder`
was created this evening with my approval, to hold a witness that needs a process of its own. The
witness is right and the project is not: the repository already has console programs run by hand
and by tests, and a mode in one of them costs nothing. The rule is not about packages only.

## D93 — Generated validator text is readable, keepable, and named in the failure (Igor)

Igor's instruction for the road D92 opens. A flag keeps the generated text — in a temporary
directory or a log — so it can be read. A compilation that fails throws, and the exception invites
the reader to look at the code that would not compile. And the generated code carries comments, so
that looking at it is worth doing.

**The comments are the part that decides whether the rest works.** Text compiled from somebody
else's dictionary fails for a reason that lives in *their* dictionary, not in our composition, so
every arm says where it came from: the tag, the message or component, and the spelling the
dictionary used. A reader who is shown an error at line 40 of something they never wrote needs
line 40 to name the entry they did write. It is the `#line` problem again — a generated artifact
that cannot point back at its source makes the reader translate, and they translate wrongly.

**The exception carries the text, and the flag decides whether the disk does.** Two separate
questions: what a failure tells you, and what a successful run leaves behind for inspection. A
failure is useless without the code, so the code — or the saved path when one exists — is in the
exception, always. The flag is for the other case, reading text that compiled, and it is the only
reason a library writes a file at all; a library that writes files unasked is a library that
surprises somebody's container.

**Loading is all or nothing.** The delegate array is the consumer's to change at any time, so a
dictionary whose middle entry fails to compile must not leave half the cells replaced and half
original. Everything compiles, then everything is installed — and the throw happens before the
first cell moves.

**And the dictionary is untrusted input, which composition must respect.** Values out of the file
become literals, never fragments of code, and names become comments with their line breaks and
comment terminators neutered. This is not a hypothetical: a dictionary is a file a consumer
downloads from a counterparty, and "compose code from it" is the shape that has to be got right
once rather than patched after.

**The expression language has no comments, and the way round it does not touch the language.**
Its trivia is whitespace; `//` and `/* */` are syntax errors, so the condition above cannot be met
by writing comments into the text that is compiled. Stripping them before compiling would be
worse than not writing them: the diagnostic would then point into a text without comments while
the reader is shown the text with them, and the line numbers would disagree — breaking exactly
what the condition exists for. So one text is composed, with its comments, and the copy handed to
the compiler has every comment **overwritten by spaces of the same length, newlines kept**. The
geometry agrees character for character: line 40, column 5 of what the compiler read is line 40,
column 5 of what the reader opens. The language is not touched for our convenience, and a test
holds the property rather than the intention — a deliberately broken arm, and the line the
diagnostic names must be the line naming the dictionary entry.

**The blanker is a scanner, not a search.** `//` inside a string literal is not a comment, and a
blanker that does not know the difference will quietly eat part of a value the dictionary
supplied. The trap is the same shape as the rest of this decision: text composed from somebody
else's file, processed by something that assumes our conventions.

**Provenance goes in twice, and for two readers.** The comment is for the person opening the saved
file; a string literal carrying the same entry — the message, the field, the requirement —
survives compilation and can reach a finding. Both are composed from the dictionary, so both are
escaped: the literal as a literal, the comment stripped of newlines and of both comment markers,
and bounded in length.

**One correction to what this decision asked for.** The layout in `CLAUDE.md` never named the
sixth package at all, so there was nothing to remove there. That is worth knowing rather than
passing over: the layout is where somebody looks to learn what exists, and a package absent from
it was invisible to that reader for the whole of its life.

## D94 — A merge is not the favour a cherry-pick was asked for

expr asked for their page-helper fix to be taken by cherry-pick and said why: their tree stood on
a stale base, so pushing the branch would drag that base under the commit. Five minutes before
someone else did exactly that, carefully, **I merged the whole branch** into main. It brought in
the stale base and a half-written review the author was holding back, which is now published under
a commit message about something else. The code is fine — main carries the finished helper — and
the author is replacing the document with its completed version. Nothing here is a code defect;
it is a process one, and it was mine.

**The distinction is worth stating because "I took their work" felt like the same act.** A
cherry-pick puts one commit on the current head. A merge puts that commit and everything beneath
it — the base its author already knows is stale, and whatever they had not finished. When a
session asks for the first, and gives the reason, doing the second is not the same favour done
another way. It is a different act with a different reach, performed on somebody else's unfinished
work.

**Third of the same shape in one evening**, and that is what makes it a rule rather than a
mishap: an ungated commit riding under a documentation fix in the morning, this merge, and a
result file naming its own branch's hash. Each time the thing sent was right and something
travelled with it. The question to ask before sending anything anywhere is not "is this correct"
but **"what comes with it"** — which parents, which files, which claims.

**What it costs to be careless here is other people's judgement, not code.** An author holds work
back because they have decided it is not ready; carrying it out for them overrides a decision that
was theirs, silently, and they find out afterwards from a third party. That is the part to
remember when the mechanics fade.

## D95 — What the before-count bounds, and when the fastest reading is the lying one

The two validation roads were counted before anything was built: the compiled-in tables at
1,183 ns a message, the dictionary's tables at 1,278 — **1.08×**. The number matters less than
what it does and does not bound. It says the two TABLE SHAPES cost nearly the same, now that both
roads are one implementation over two tables. It does not bound what compilation can win, because
both roads interpret: each reads a table at every step, and compiled code would read none. So the
8% is not a ceiling; it is the evidence that there is no gap between the roads left to close, and
that whatever the expression language buys must come from removing the interpretation itself.

**The acceptance was named before the work, which is the point of naming it.** If the dictionary
road does not go clearly below 1,278 once compiled, that is said out loud and written into the
notes, and whether the construction is worth its price is Igor's to decide with the number in
front of him. And the corollary is worth seeing now rather than being surprised by: if compiled
lambdas beat 1,183 as well, the same technique applies to the schema we compile in — a far larger
change, and his call, not one to drift into.

**And a refinement of tonight's other rule, from the same run.** I told the stand that with a
one-sided error — interference can only slow a reading — the minimum is the least corrupted
observation, and that stands. But a reading that is 40% FASTER than every other, on one side only,
cannot be explained by less interference: nothing was removed that could account for it. It is
evidence that the round did not do the work, or did different work. So the minimum is trustworthy
only where the fast tail is; a fast outlier appearing on one arm and not on its pair is a defect
to explain, not a clean number to keep. Fifteen rounds and a median, with the fastest and the
slowest printed beside it, says both things at once — which is why the report carries all three.

## D96 — A shipped page does not assert what somebody else's library does

The FIX package's page came to say that "the libraries this one is compared with stop at the first
problem". It is there for a good reason: someone porting code from another engine will otherwise
assume one finding a message and silently drop the rest. The warning is worth keeping. The claim
is not.

**Three things are wrong with asserting it, and only the first is about effort.** The evidence
available for the engines that matter most here — the commercial ones, bought by the people who
reconcile with a counterparty for money — is their documentation, which describes intent rather
than what is there; that is the rule we derived today from reading an assembly instead of a
vendor's page. Second, the ten implementations that can be read are all open and free, which is
not a sample of anything. Third and decisively: a page of ours cannot stay right about a product
we do not control. Our pages carry no history, so they can only be right or wrong — and this
sentence becomes wrong the day a vendor changes their API, with nothing in our build able to
notice.

**The warning survives without the claim, by being addressed to the reader instead of to the
competition.** "This package returns every finding; if you are porting from a library that stops
at the first problem, code written for one error a message will drop the rest." That is
unconditionally true, needs no survey, and does the same work for the person it was written for.

**What was read still gets written down — in the design note, with its date and its tags**, where
a dated internal document is exactly the right form and where its being a snapshot is understood.
A survey is knowledge; a comparative claim on a shipped page is a liability that ages without
telling anyone.

## D97 — A magnitude inside the scatter and a sign that never changes are two readings

The retention pair answers D67's question with a no: on all ten paired cliff rows the bytes a
call are identical to the last digits on both sides, the twelvefold step stands on both, and the
times sit inside the A/A. So whatever that commit keeps, the SQL:2023 steps are not made of it —
and the caution I attached to the ten cliff rows is retired rather than confirmed. Nothing moved,
so the rows keep the meaning they had, and nothing in them can be read as the re-reading class
cured.

**A hypothesis answered "no" by a measurement is a result, and the cheapest kind we get.** D67
said that if the boundary moved, the rows had to straighten. They did not straighten, so the
boundary is not what they are made of. That closes a line of explanation for good, at the cost of
one window, and it closes it in a way no amount of reading the code would have.

**And the part of the report worth reading twice is the small one.** A lean of two to three per
cent on the largest documents is *inside the scatter of the A/A* — and none of its five runs is
negative. Those are two different statements. Magnitude inside scatter says the effect is small
next to the noise; a sign that holds five times out of five says there is an effect, since under
no effect that costs one chance in thirty-two. Reporting only the first dismisses what the second
found. So a paired row carries both, and a consistent sign at a magnitude the noise could hide is
the case for one more measurement rather than for a shrug.

**What that next measurement has to do is swap the slots.** The one row below the bound moves 1.9%
with five signs out of five, and the session cannot separate it from the bias of the second slot —
which is exactly what swapping decides, and nothing else does.

## D98 — The rows were chosen for steepness, and they test a different bound

The negative pair has a cause read out of the code rather than guessed. `DirectValuesClass` has two
branches, and the parking that landed is in one of them; the **dense** branch still throws an
oversized table away at `TableKept = 65536` — exactly the boundary all four SQL:2023 cliffs step
at. SQL:2023 is dense by the gate that decides it. So the commit reaches the non-dense store, the
ways and the arena, and does not reach the store those rows use.

**The claim was not false; it was narrower than the rows chosen to test it.** Its evidence was
ladders on Web's accept and address lists, which are non-dense, and there the cliff is gone. The
SQL:2023 rows were picked for showing the steepest steps — which is how one picks a row to be
impressed by, not how one picks a row to test a mechanism. Steepness says where an illness is
worst; it says nothing about which of several bounds produces it, and two bounds with the same
disease look alike from outside.

**So the rule: a row is chosen by the mechanism it exercises, and the choice is written down with
it.** "The steepest ten" is a selection made by the answer rather than by the question, and a pair
run on such rows can come back negative while the change works perfectly — which is exactly what
happened, and it cost a window to learn.

**And the record is corrected before anything is built on it.** The commit pays two to three per
cent on the largest documents and buys nothing measurable on the rows everybody will look up. It
lands for a different reason — the intermediate state corrupted reused stores and failed 453 tests,
and the emptying is what fixes that — and the pooling half is justified by the non-dense ladders.
It must not be remembered as the cliff fix. A commit remembered for what it did not do is how a
later session concludes the cliffs are cured and stops looking.

## D99 — A bound written for the rare large document punishes the steady large workload

The dense store drops one oversized table and keeps the rest; the dropped table then regrows from
sixteen, and that regrowth is the cliff. Which exposes what the bound is actually for. `TableKept`
protects a process from holding a table grown by an unusual document — a sensible thing to want.
But a workload that is *always* past the bound is not an unusual document; it is the normal case
for that consumer, and the bound turns their every parse into a rebuild of the same cascade. A
limit designed for the exception, applied to the rule, is a tax on the rule.

**So the dense branch parks the store, as the other branch already does.** Keep the room, empty
the contents, release after the idle rentals. That gives the steady large workload its capacity
back and still lets the occasional large document go — which is the whole point of parking rather
than keeping. It is a change of policy and not an extension of one: today a store gives up one
table and keeps three hundred, afterwards it gives up nothing and holds everything for eight
rentals.

**Per table was rightly withdrawn by its author.** A slot, an idle counter and a weak reference
for each of three hundred tables is not the landed shape applied elsewhere; it is a different and
heavier mechanism wearing the same name.

**And the pair must include the case the change is worst for.** One runaway table among many small
ones now stays resident where it used to be released, and that is the shape to measure — not
because it is expected to fail, but because a pair that exercises only the case we hope for
measures our hope. That is D98 read forwards: rows are chosen by mechanism, and the mechanism that
loses is one of them. Retained bytes after a parse, not only bytes a call, since what is being
traded is memory for allocation.

## D100 — The compiled-in road's remaining advantage is the start (Igor)

Igor, reading the before-count: this is what he expected of the expression language, and it is what
compiling to IL was for. The emphasis moves. The road with the schema compiled into the package
keeps one advantage — it starts immediately — and the dictionary road offers a slightly slower
start with full flexibility in validation. Improving the compiled-in road stays possible; it stops
being the thing the design is arranged around.

**One caution belongs beside that, and it is ours to state rather than his to remember.** The
expression-language road has not been measured. The two numbers in hand — 1,183 ns and 1,278 —
are both table-walking roads, and they bound the difference between two table shapes, not what
removing the interpretation wins. `Expression.Compile` producing a delegate is a good reason to
expect it; it is not a reading. The acceptance named before the work stands: if the compiled
lambdas do not go clearly below 1,278 it is said out loud, and the emphasis above is then a
decision taken on an expectation that did not hold.

**And the corollary keeps its shape.** If the lambdas beat 1,183 as well, the same technique is
available to the schema compiled into the package — a far larger change, wanted or not, and his to
order rather than one to drift into on the strength of a validation benchmark.

## D101 — Which of two explanations is true, and the cheap answer that is not the general one

When a parse of an expression fails, two accounts compete: a syntactic error at the place reading
stopped, and a semantic refusal remembered when a name would not resolve. Today the refusal wins
unconditionally, and on input using a construct the language does not support — a collection
initializer, `new[]`, an object initializer — the parser backtracks into another reading of the
statement and reports a name from THAT reading, innocent and at a nonsensical position. Where no
alternative exists the message is already exact, which is what points at the cause.

**"Further along means truer" was tried and refuted by the suite**, and that is the useful part of
the report. Both repros were fixed by it and six tests broke, among them `(int x) => x + y`, where
the refusal is earlier than the syntactic error and is nonetheless the right answer. So position
cannot separate the two cases, and the criterion has to be about something else.

**What separates them is whether the refusal is what stopped a reading that would otherwise have
worked.** Define `y` and `(int x) => x + y` parses; resolve the refused name in the `new[]` case
and the construct is still unsupported. That is computable rather than guessable: parse once more
with resolution suppressed — every name assumed to resolve — and if that pass consumes the whole
input, the refusal is the story; if it fails too, the syntactic error is, at its own position. It
costs one extra parse on the failure path only, which is where we can afford anything.

**But that is a change to every error message this package produces, so it is designed and evidenced
on its own, not slipped in beside a repair.** The six tests that broke are the beginning of the
record of what today's behaviour is, and that record is what such a change is held against.

**The repair to make now is the narrow one, and it is not a language change.** Recognize the
unsupported constructs in the grammar for the purpose of refusing them by name — "collection
initializers are not supported" — and nothing becomes valid that was not, nothing invalid that was
not. That is a diagnostic, which the standing rule allows to proceed; only a change to what a
grammar may say goes to Igor. The list is driven by cases that actually mislead, and grows when
one appears; completing it against the whole of C# is not the work.

**And one finding to keep beside the code.** `state.Refused() ?? match.Error!` has a comment
describing a position-aware intent the code never had. The comment was not stale — it described
something that was never built. Where the two disagree, neither is evidence for the other.

**Answered with data, and the answer fits the estimator exactly.** Over 101 series on a quiet
machine, ninety never differ between the two passes by more than 1.7 at any size; eleven differ by
a multiple of 2.9 to 8.1, and every one of those eleven has its widest gap at 33 to 63 units — the
smallest sizes measured. From 121 units upward the widest gap of any series is 1.69. That is the
shape of a warm-up and not of a reading that is fast for a wrong reason: the first pass meets the
small sizes in code the tiered JIT has not promoted, which is why the second pass exists at all.
And the exponent is read at the largest sixteenth of the ladder, where no gap above 1.7 appears —
so taking the faster of two passes is safe in the region the guard uses, by a reading rather than
by an argument.

**What remains an inference is named as one.** Which pass was the fast one at each size was not
recorded, so "the fast side is always the second" follows from where the gaps sit rather than from
a column. It is worth one more field, because it is also the evidence that the warm-up pass earns
its cost — if the first pass were ever the faster, walking twice would be buying nothing there.
Nothing waiting on it: the claim the guard rests on was read, not inferred, and the distinction
between the two was drawn by the session that could have blurred it.

**Withdrawn within the hour, and the withdrawal is mine.** Collection and object initializers are
SUPPORTED — `new List<int>() { 448 }` and `new StringBuilder() { Capacity = 8 }` both run. What is
not supported is omitting the constructor's parentheses: the rule requires `Arguments`, and the
initializer is an optional tail after them. So the repair D101 authorized — refusing with
"collection initializers are not supported" — would have shipped a false statement about our own
package, and a worse failure than the one it replaced: today's message confuses, that one would
have misled confidently, and a reader believing it would stop looking for the working form they
already have.

**How it got into a decision is the part to keep.** A diagnosis travelled from the session that met
it, through the session that owns the area, to me, and each pass added standing without adding
evidence; I then wrote it into the repository's record, which is where a relayed claim becomes a
fact nobody re-checks. Checking cost four minutes and one run, and the session that finally did it
was the one furthest from the original observation. **A claim arriving through a third party is
evidence about the journey, not about the code** — and an architect writing it down is the last
place that can still tell the difference.

**What is actually true of the three constructs.** The implicitly typed array `new[] { … }` is
genuinely unsupported — `new int[] { … }` is the form — and where no alternative reading exists the
message is already exact, pointing at the `[`. The other two are a parenthesis away from working.

**So the choice narrows, and the larger half goes to Igor.** A diagnostic that recognizes
`new T { … }` and says the parentheses are needed is true, is allowed by the standing rule, and
teaches the working form. Making `Arguments` optional when an initializer follows is a change to
what the grammar accepts, is the same form C# accepts, and is his. The second is asked first,
because if it is granted the first is code written to be deleted.

## D102 — The compiled lambdas are not faster, and the acceptance said so in advance

Ninety-three rules compose, compile and agree — 186 fixtures, 271 findings, not one message where
the two roads differ, compared position by position through the consumer's own call. And the
speed, median, on the machine's pinned cores:

| hot message types | against the walk |
| --- | --- |
| one | 0.95× |
| five | 0.95× |
| ten | 1.79× |
| all ninety-three | 2.60× |

Five per cent when the traffic is narrow; two to three times SLOWER as it widens. The shape says
code size: each rule is its own dynamic method, and a loop over ninety-three large methods does
not run in warm code.

**So the acceptance named before the work is the one that applies, and it says this out loud.**
Below 1,183 — the compiled-in tables — there is no question today. Below 1,278 only with a narrow
hot set and only by five per cent. And that answers the corollary I was keeping in view: the same
technique does NOT obviously apply to the schema compiled into the package. A negative result that
removes a change an order of magnitude larger is worth more than the five per cent would have been.

**Two handicaps belong to this composition and not to the road**, and the session named them
before anyone could read the ratio as a verdict on compilation: the generated scope walks its
fields twice, because a switch on one tag cannot see the neighbour a length-and-data pair needs,
where the walk does it in one pass; and each scope allocates a mask where the walk takes it from
the stack, which the expression language has no way to express. Both are being repaired and
re-measured. What no repair reaches is the mechanism above: a compiled expression tree is a
dynamic method, and the code it is made of is the size it is.

**And the emphasis set in D100 has to move back with the number.** It was taken on an expectation —
reasonable, mine to caveat, and stated as a caveat — that compilation would be faster. It is not.
The dictionary road's advantage is flexibility; the compiled-in road's advantage is a fast start
AND the speed on wide traffic. A result announced as desired before it was measured is exactly
where a report needs to be flattest, and this one was.

## D103 — A counter reset by the very event it exists to count

The idle demotion never fires. `_largeIdle` is reset by every rental that takes the parked store,
and the parked store is all the pool has once a large parse has returned it — so each following
parse, however small, takes it and resets the counter. The counter can only advance when a rental
is served from elsewhere WHILE the large store is also set, which needs two stores in flight, which
needs nested parses. In the ordinary workload — one document at a time — the demotion is not slow,
it is unreachable.

**The comment describes an intention the mechanism cannot express**: "if eight parses in a row have
not wanted the large store". The pool has no notion of wanting. It hands out what it has, and a
rental is therefore evidence of nothing. That is the second time tonight a comment has described
something that was never built, and the two have the same tell — the sentence says what SHOULD
decide, and no line under it reads that quantity.

**The signal is the return, not the rental** — a parse tells you the size it needed only when it
gives the store back. **But the returned store's capacity is the wrong quantity**, and using it
would rebuild the same dead end: once the large store is handed out, every return has large
capacity whatever the parse did. What advances the counter is the high-water USE of the parse just
finished, which the store already tracks; a use past the bound resets it, a use below advances it,
and eight small parses in a row demote a store they have been borrowing all along. That is the
policy as written, for the first time.

**And the ordering, which was the question asked.** The dense change is not shipped on its
allocation figure while the release is broken. Its allocation win is real — about tenfold — and its
retention is 0.4 to 1.3 GB held for the life of the thread, which is not the policy anyone
approved; the approved policy was *keep the room, then let it go*. A change whose acceptability
depends on a repair that has not been made is not ready, however separable the two are on paper.
Repair first, then the pair, then the commit — and the parent is repaired on the same grounds,
since it is on main and carries the same dead counter.

**Two corrections to the entry above, and both are mine.** The parent is NOT on main: `5ed9682b`
is unpushed, the whole chain sits on a worktree over `origin/main` at `e48ab0aa`, and the dead
counter has never left it. I wrote "since it is on main" from a message rather than from git,
which is one command. The decision does not change — a commit whose acceptability rests on an
unmade repair is not ready — but the urgency I attached to it was invented.

**And the hypothesis I offered for the anomalies predicts the wrong sign.** I suggested the
emptying was releasing references the old side held. The emptying is in the parent, which is the
BEFORE side of that pair, so both sides empty; and where the dense branch differs it differs the
other way — the old side replaced an oversized table with a fresh small one and released the whole
array, while the new side clears it and keeps it. Every shape I can construct says the new side
should retain MORE. Four rows retain less at identical allocation, and no story either of us has
produces that sign.

**Which makes the next measurement the right one to insist on**: one anomalous row alone in a
fresh process, both sides, on a row that provably cannot have changed. If it is identical in
isolation the anomalies are contamination between rows; if it still differs, the change does
something neither of us understands, and that is worth knowing before a repair is written on top
of the understanding. **A result that contradicts your model of your own change outranks the
result you went looking for.**

**And the shape of my two errors is one shape.** Both are claims about code and about a repository
made from a letter instead of from the thing — on the evening whose whole subject was that a
relayed claim is evidence about the journey. The architect is not outside that rule; being the
place where claims are written down makes it stricter, not looser.

## D104 — A shared process hides a true result as readily as it invents a false one

Run one row per process, both sides come out identical to tens of kilobytes: `el/terms1000`
700.7 → 687.3 KB where the shared-process table had read 19.6 → 0.0; `paren-2715` 13,451 → 13,475
where it had read 10,264 → 27. The pools are thread-static and shared by every row in a process,
so each row's retention was a function of the rows before it. Neither the emptying story nor the
contamination story needed to be right about the sign, because the sign was not real.

**And the same contamination concealed a true result.** `predicates-3393` read 0.0 → 0.0 on both
sides — which is why it was flagged as inexplicable — and alone it reads 3,637 → 24,115 KB with
allocation falling 94.86 to 10.97 MB a call: the predicted trade, invisible in the shared run.
**That is the worse of the two failures.** A false anomaly gets argued about, and the argument is
how it is found. A concealed result reads as a clean negative, and a clean negative ends the
conversation — nothing about it invites a second look.

**Which gives the rule for choosing a process boundary.** An instrument measuring a RATE — time a
call, allocation a call — may share a process between cases, and must warm up. An instrument
measuring an ACCUMULATION — retained heap, pool state, anything a case can leave behind for the
next — may not: there the previous case is part of the apparatus. The question to ask of any stand
is not "is it fast enough this way" but "can a case reach the next one", and where it can, the
boundary is the process.

**And the discipline goes in the script, not the note.** The instrument now runs one row per
process by construction, so it cannot be used the wrong way again. A rule written beside a tool is
followed by whoever read the note; a rule written into the tool is followed by everybody.

**The magnitude is marked provisional and the decision does not rest on it.** The 0.4 to 1.3 GB
was read in the contaminated arrangement, and the worst rows ran last. The dead demotion is a fact
read in the emitted `Rent`, not a measurement, so "fix the demotion first, the dense change waits"
stands whatever the clean table says.

**The table above is withdrawn, and the withdrawal belongs here rather than only in D105.** Those
three ratios came from comparing "one hot type" against "all ninety-three", which compares
different SETS of messages and cannot separate the cost of widening the hot set from the difference
in what the messages contain. Their author found it and remeasured; the valid figures are in D105.
The conclusion survives — widening costs the compiled road three times what it costs the walk —
but the numbers here do not, and an entry that carries retracted numbers with its correction filed
two entries later is an entry that will be read alone and believed. A withdrawal goes where the
claim is, not where the correction was convenient to write.

## D105 — Compiled control flow is warm for one and cold for ninety-three

The comparison that produced D102's table was invalid, and its author found it: "one hot type"
against "all ninety-three" compares different SETS of messages, so it cannot separate the cost of
widening the hot set from the difference in what the messages contain. Measured properly — each
type alone, weighted by fixture count, against the measured corpus — the cost of widening is:

| | narrow | wide | widening |
| --- | --- | --- | --- |
| the walk | 3,349 | 3,707 | 1.11× |
| full rules, 8.8 M chars | 3,254 | 10,128 | 3.11× |
| without value checks, 4.5 M | 1,686 | 5,399 | 3.20× |
| without the membership switch, 1.4 M | 1,632 | 2,631 | 1.61× |

**Halving the text changed nothing — 3.11 to 3.20 — and removing one layer changed everything.**
So it is not code size in the sense of bytes. Each rule carries its own switch over one to three
hundred tags; ninety-three rules are ninety-three cold jump tables, while the walk has ONE
membership lookup shared by every type. And the switch is not cheap even when warm: 3,254 against
1,632 on a fifteen-field message.

**The principle, which is not about FIX.** Compiled control flow is fast because it is warm, and
warmth is per method. Compile the control flow of one thing and it stays warm; compile the control
flow of ninety-three and the rotation between them is the cost. A shared walk over DATA does not
have that problem: one small routine, always warm, reading a table that differs per type. So where
a schema has many alternatives exercised in rotation, compile the data the rule needs and not the
branching — and the form where that already exists is the one in the package.

**This is a negative result that transfers, which is what makes it worth more than the five per
cent we were chasing.** "Compile the schema into the package" is now closed by mechanism rather
than by a number, and the mechanism is not specific to this schema.

**And it asks a question of our own generated parsers, which nobody has asked.** A generated parser
is compiled control flow. For one grammar read over and over it is exactly the warm case, which is
why it wins. But the FIX 4.4 parser is fifty-seven megabytes of C# and T-SQL is 1,142 rules — and
whether a workload that rotates through many of their alternatives meets the same wall is a
measurement nobody has taken. It may not: a parser's branches are threaded by one input rather
than selected ninety-three ways. Asking it is cheap; assuming either answer is not.

**Sharpened by its author, and the sharpening is what makes it portable.** "Not code size in bytes"
was my phrasing and it is too loose. Halving the text changed nothing because what was removed was
the value checks; removing the switch, with the same number of arms still present in other layers,
halved the cost. So the quantity to count is **alternatives exercised in rotation**, not the volume
of what was generated. "A lot of code" is a poor indicator; "many cold forks, alternating in time"
is a good one — and only the second transfers to a schema nobody has seen.

**And the argument against transferring it one-to-one to our own parsers is worth keeping beside
the question.** A parser's branches are threaded by one input moving forward, not selected
ninety-three ways among independent procedures called in arbitrary order; the locality is a
different shape. That is a consideration and not a measurement, and it was labelled as one by the
session that offered it.

**One more thing the same session caught about itself, and it is a rule.** The figure it had given
another session — 3.6 MB/s for reading the generated text — was a single cold pass with default
tiering; measured warm it is 8 to 10. The discipline that had protected its own validation
measurement, warm-up by the clock, did not attach to a number it was *relaying*. **Care follows
the act of measuring and does not follow a number into someone else's hands** — which is the
relaying rule seen from the other end: the sender is the last person who can still apply it, and
the receiver has nothing to apply it to.

## D106 — The defect propagated by duplication faster than the fix did

The dead counter was not in two pools but in **five**. Two are written by the generator through one
emitter; the arena, the tape and the lexer's buffer carry hand-written copies of the same logic,
and every copy carried the same defect — including the one written two hours earlier that evening
by copying the pattern. A repair aimed at the place the defect was found reached two of five.

**That is the argument for holding the five to one policy with a test rather than to one text.** The
same morning's decision said so on general grounds; tonight it has a number. A test asserting the
policy — eight returns whose usage is under the bound demote a parked store, a return over the
bound resets — would have failed on three pools at once and named them. Commissioned now, with the
work, and not left as a thing to do later: a rule that only exists as a sentence is a rule that the
next copy will not inherit.

**And the four defects met on the way are a set rather than a list.** Two pools missed, found by
grepping for the counter; a repair script that matched the first occurrence in the file rather than
the intended one, found by reading the EMITTED output, since a broken emitted program is not a
broken emitter and the generator built happily; a malformed sum for a grammar with marks and no
stacks, found by the suite; a static method called as an instance one, found by the build. **No one
of the four instruments would have found the other three.** That is the case for keeping all of
them rather than the fastest.

**The comments described the version that did not work**, in four places, and sending the reader to
`Rent` is exactly how the next person rebuilds the broken model — and the broken model here is "the
counter counts rentals", which kept it dead through two commits. Third time tonight a comment has
named a quantity no line reads.

**The dense change is decided on the clean table, and the acceptance is named before it arrives.**
With the release working the arithmetic is different — hundreds of megabytes for eight small parses
rather than for the life of a thread — so the earlier verdict does not carry. Three conditions:
retention must be SEEN to fall after eight small parses, not asserted from the code; peak retention
must be of the order of what the parse itself needed rather than a multiple of it; and the
non-dense rows must not regress. Named now so that the numbers are read against a standard rather
than a standard fitted to the numbers.

## D107 — Correct in place where you can, and do not write what you cannot correct

A withdrawal goes where the claim is. That rule has a second half, found by the session it was
given to: **where the record cannot be edited, the rule becomes "do not put it there yet".** A
commit message is immutable, so a figure still liable to move does not belong in one — the
correcting commit does not stand where the claim stands and will never be read with it. The same
shape held here: D102 carried three retracted ratios with the correction two entries later, and an
entry is read alone.

So the axis is not care but **editability**, and it sorts our records:

- Editable — the journal, the memory, the pages, a diary: a wrong claim is corrected AT the claim,
  and the correction says it is one. Filing it elsewhere is the same as not filing it.
- Immutable — a commit message, a pushed history, a tag, a published package: nothing is written
  there that has not survived a check. There is no correcting in place; there is only a second
  record that nobody reads beside the first.

**And it reaches the things we ship.** A release cannot be recalled either, which is the same rule
one size up and why this morning's "free before release" cost what it did: 0.1.0 is immutable in
exactly the sense a commit message is. Where a number is still moving, its place is a letter or a
scratch directory — and a commit message when it has held.

**One observation the day earned, from three sessions independently.** Every one of tonight's
uncorrected claims was made while PASSING SOMETHING ON, and none while doing the work. Restating
feels like repeating something already checked; it is a fresh assertion made without the
instrument. The check belongs at the moment of handing over, because the sender is the last person
who can still apply it and the receiver has nothing left to apply it to.

## D108 — Two states that look alike from the far side, and a binary that was not the one built

The five-pool policy test asserts the count **at every step** rather than the state at the end, and
the reason is the defect it was written for: a counter that never advances and a counter that
advances and resets look identical from the outside — the store is held either way. This one has
now been written wrong in both of those ways, and neither an end-state assertion nor the eye caught
either. **Where a working mechanism and a broken one share their visible end state, the test
observes the transitions.**

**A limitation was left visible instead of smoothed over**, which is the right call. The pools
cannot all be exercised from one grammar: the lexer's buffer exists only for a grammar read as
tokens, the engine's arena only where the engine is emitted, and the cheapest thing that asks for
an arena is `find`, which is refused over tokens. So each case compiles the grammar that produces
its pool. What is held to one policy is the pools; the grammar is not, and the test says so.

**And the trap that `docs/development.md` already names caught somebody who knew it was there.** A
build failed, the assembly from the previous build stayed on disk, and the five failures read off
it came from code that no longer existed. The repair is not to read the document again: it is that
the tests now run through one command that refuses to run them when the build did not succeed.
That is D104's second half in a second place — **a rule that lives in a document is followed by
whoever remembers it, and a rule that lives in the tool is followed by everyone** — and it is worth
extending to every way this repository runs tests, since the document did not prevent it even for a
reader who had read it.

**One of mine belongs here too.** I read "started 2m ago" in the session list and reported that the
working sessions had restarted and lost their context. They had not; the list describes the
connection, not the conversation. Fourth time tonight: a description read as the state, and the
instrument this time was the one I had reached for precisely to avoid reporting from memory.

## D109 — Audit a duplicated list against its original, and then ask why it is duplicated

The editor's audit was done the right way round. The editor parses with the compiler's own front
end, so new syntax is read correctly the day it lands and cannot rot; **what can rot is only what
the editor writes by hand.** That decomposition makes the audit finite, and it turns "read the
specification and judge" into "compare each hand-written list against the compiler's own,
mechanically": the keyword set against every word literal the parser takes, the punctuation
tooltips against the spelling table, the built-ins against the binder's list. One built-in of seven
was missing, and no amount of reading the specification would have been as certain.

**And the finding raises the question behind it.** A list in the editor that mirrors a list in the
compiler is a duplication of exactly the kind that produced five dead counters in five pools. The
repair is to fill in the missing entry; the cure is to ask whether the list can be DERIVED from the
compiler's rather than kept beside it. A derived list cannot go stale, and these are small enough
that the answer is probably yes for at least the built-ins and the keywords. Worth pricing before
anyone writes the next one by hand.

**One of the three divergences is a wrong claim shown to a user**, which ranks above the other two:
the hover inside `[^ … ]` calls `^` a "recovery marker", and the language has no such thing — the
character is the complement of an element set, and recovery is `recover`. It is the only hover a
reader gets there, so the editor is the thing teaching them the wrong word.

**The third is predicted rather than demonstrated, and waits for its demonstration.** A constructing
group owns its captures and the editor hoists them to the rule, so two groups capturing the same
name become one symbol for Rename and Find All References. That follows from reading both sides;
it has not been run. It is repaired after it is shown, not before — the evening's rule, applied to
a prediction that happens to look certain.

**And the manual checks are behind in the place this week's work landed.** The Playground grammars
contain no `recover`, no `find`, no lookahead either way, and no `with state` at all — which is
exactly where §7.8 and GRAM4029/4030 arrived. A manual check that cannot see a regression in the
newest area is the part of the suite that looks like coverage and is not.

**A sharpening of D88's criterion, from applying it.** I gave "'existing' — compared to what?" as
the test for prose that narrates change. It finds candidates; it does not separate them. Of five
hits, four were history and one was **a real distinction wearing history's clothes**: "this layer
restores separate length nodes" was a wrong word for a true statement — a length/data pair is ONE
field to the field parser and TWO nodes in the message model, which the pair check depends on.
Deleting the word would have deleted the fact. It now reads without yesterday: one field there,
two nodes here, and this is the layer that separates them.

**And a word can compare against the subject matter rather than against the past.** "BodyLength is
unchanged" compares with the WIRE, which is legitimate — and a reader cannot tell that from a
comparison with a previous version without guessing. The repair is not to strike the word but to
name what the comparison is against: "BodyLength counts the wire's octets and the pipe rendering
does not change it."

So: **a search term produces candidates, and only reading what is asserted separates them.** That
is the day's own rule arriving in prose — checking by names rather than by mechanism is what we got
wrong three times tonight, and a grep over "existing" is checking by names.

**And the dated internal document was treated correctly by not being rewritten.** A half-updated
snapshot is worse than either state. It gained a header saying what it is, on what day, which names
have moved since, and where the current description lives; the reasoning and the measurements stay,
dated by what they measured.

## D110 — A count that the mechanism explains is not a finding

A sweep for tests repeated inside one method found sixteen `!text.Ensure(p, 1)` in a FIX method and
thirty-two bounds checks in an expression-language one. **The numbers mean nothing.** A
recursive-descent reader tests the same thing at every position it advances to; that is what it is.
A count is a finding only when the mechanism does not already account for it, and here it does,
completely.

**Pointed instead at the case the mechanism cannot explain — two identical guards on adjacent
lines — the same instrument finds zero**, in all three files. So the emitter does not write an
adjacent duplicate guard, and anything beyond that needs dataflow rather than text. That is a
result: a line of enquiry closed, cheaply, by an instrument that was sharpened until it could
return an honest nothing.

**Four of one session's instruments were withdrawn in a day, and that is the healthy number.** Each
was withdrawn by its own author, and each withdrawal came from asking what the number would look
like if the instrument were measuring something other than what it was pointed at. An instrument
that has never been withdrawn has usually never been asked.

**And a caveat that was nearly filed as a free win was not.** The UTF-8 parse overloads look like a
straight replacement for hand-written byte parsing in FIX — until the comment on the existing
overload says why it is hand-written: nine digits per operation, no text decoding, no decimal
rounding. Replacing deliberate code with a framework call is a PAIR on the rows that exercise it,
not a cleanup, and calling it the latter is how a measured decision gets undone by a tidy one.

## D111 — The dense change lands, and the slot that never had a release is next

The clean table, one row per process, three sides, meets all three conditions named before it was
taken. The fall after eight is SEEN: 583,978 KB to 4,234; 2,018,095 to 381; 2,327,730 to nought;
138,624 to 123 — and on the side with the dead counter none of them fell. The peak is of the order
of what the parse needed, 1.00 on every row with a largest deviation of 1.04. The non-dense rows
are byte-identical on all three sides. Against that, the rows fall 37% to 72% in time with
allocation down tenfold.

**So it lands.** Not because the numbers are good but because the standard they were read against
was fixed before they existed — which is the only arrangement in which a good number means
anything.

**And the table found a gap that none of the three conditions names**, which is the argument for
reading a table rather than checking a list. Above their step the cliff rows keep 19.8 to 24.9 MB
and **never fall** — not after eight small parses, not after sixteen. The cause is in the emitted
code rather than in the numbers: **the release exists only for the parked slot, and the ordinary
spare has none.** A store whose capacity is UNDER the bound is kept for the life of the thread
whatever it weighs. The dense change did not introduce it — the side without parking keeps 3.5 MB
on the same rows — but it enlarges it sixfold and permanently.

**Which does not block the commit, and here is the test that says so.** D103's rule is that a
change whose acceptability depends on an unmade repair is not ready. This one's acceptability does
not: even on the gap rows the retention is of the order of what the parse itself needed, and the
alternative on offer is rebuilding the machinery on every parse, which is what the time column
measures. The gap is a pre-existing defect made larger, not a condition of this change working.

**Two things go with it, and neither is a backlog entry.** A bound and a release for the ordinary
spare is the NEXT item. And it is measured in BYTES: today's bound counts array elements,
`> 1048576`, while the cost is bytes — a million `Held<T>` is some 16.8 MB and a million `int` is
4 MB, and the same test calls both small. That arithmetic was offered as arithmetic, with the
element sizes named as one read away and not read; **the byte bound is designed on the read, not on
the estimate**, which is the whole of this week in one sentence.

**And the number is written where the choice is.** Until the spare has a bound, a thread that
parses such documents keeps twenty-odd megabytes it will never be asked to give back. That belongs
beside the decision and in the diary, not discovered later in a size report.

**Withdrawn within the hour, by the author of the change, against the author's own interest.** The
arithmetic called "one read away" was read, and both halves of the diagnosis were wrong. `Held<T>`
holds one field, so eight bytes and not sixteen; and elements-against-bytes, while true, is not the
mechanism. **The mechanism is that the dense bound does not count the value tables at all.** As
emitted, it sums three record tables — `Live`, `Starts`, `Built` — and the three hundred value
tables, where the values actually are, do not appear in it. At the bound the record tables come to
about 2.1 MB and each written value table adds about 2.8 MB, uncounted; the observed 19.8–24.9 MB
is two megabytes of records and seven or eight value tables. That is why those stores are "small"
by the bound and go to the spare.

**So it is not an old defect enlarged. It is a new bound that cannot see what it guards**, written
in this change, with the not-summing chosen deliberately and the reason in the emitter's own
comment. Before it, each oversized value table was dropped individually — crude, and it did bound
the value tables.

**The landing decision above is therefore withdrawn, and the test that withdraws it is D103's.** A
change is not ready when its acceptability depends on an unmade repair. An hour ago that test
passed because the gap was older than the change; it fails now, because the gap IS the change. A
bound blind to nine tenths of what it bounds does not implement "keep the room, then let it go" —
it is the same class as a counter reset by the event it counts, and we have just spent a night on
that class.

**What lands instead is the bound counting what it protects.** The author's own proposal removes
their stated reason for not counting: a running total of the value tables' capacity, maintained
where a table grows rather than summed on every return — free at the point it changes, O(1) at the
point it is read. Then the ten cliff rows become a real test of it, and the three conditions are
read again against a bound that means something.

**And the shape of this correction is worth more than the decision it reverses.** The author
checked an arithmetic they had labelled as arithmetic, found it wrong in the direction that
implicated their own change, and brought it before the decision rather than after. Nothing in the
process would have caught it: the conditions were met, the table was clean, and I had already said
yes.

## D112 — A suppression that suppresses nothing disarms the check it names

The emitted `Failure` struct declares fields some grammars never set, and a field nothing assigns
is CS0649 — which, under a consumer's `TreatWarningsAsErrors`, fails their build on a file they did
not write. So each field's template carries a one-field-wide `#pragma warning disable 0649` with
its reason on the line above. That is careful work and it is why the sweep had to be aimed at what
the mechanism does NOT explain.

**What it does not explain: the pragma travels with the field unconditionally, while its reason is
per grammar.** In nineteen of the twenty shipped generated files the suppression sits around a
field the same file assigns — thirty (file, field) pairs, counted: `Quiet` assigned 255 times in
`SqlStandardParser` and `OutOfInput` 386, `Quiet` 186 in each T-SQL reading, 73 and 62 in the
expression language, 48 in `Rfc5322` through an object initializer, which counts for CS0649.

**The cost is not the two lines. It is that CS0649 is the one check that would catch a change which
stopped writing `Quiet` in some grammar, and the pragma switches it off where it was never needed.**
D85 found the emitted code's tidiness floor to be wherever Roslyn's warning list happens to stop;
this is the generator cutting a hole in that floor wider than its own reason. A suppression is not
neutral — it removes a guard, and the place it removes it is the consumer's build.

**The fix is one place, and its condition must be stated more precisely than "the same knowledge
that conditions the field".** The field is emitted when the feature is present; the pragma is
needed when the field is emitted AND no assignment site was. The emitter knows both, because it
writes the assignments. Getting that wrong re-breaks a consumer's build under warnings-as-errors,
which is the worst direction to be wrong in, so the acceptance is a grammar that emits the field
without assigning it, built with warnings as errors, still clean — and the nineteen files losing
the pragma.

**And a false start belongs in the record because it nearly went out as the finding.** Fifty-nine
of the seventy-five suppressions were first read as carrying no reason; every one carries it, on
the line above where the search was looking. The care about to be criticised was there all along.
That is D110 one step earlier: not a count the mechanism explains, but **a reading the source
explains if you look one line further**.

**The acceptance case exists already and does not need constructing.** The two conditions diverge
in the current set, in one place: **`Looking` in FIX is emitted, read forty-three times, and
assigned nowhere** — those reads are of the default, which is the value's meaning, so its pragma is
genuinely needed. `Quiet` is assigned in all nineteen files that emit it, so for that field the two
conditions have never diverged. So the acceptance is a diff, not a fixture: after the change FIX
still carries the pragma on `Looking`, and the nineteen lose theirs on `Quiet` and `OutOfInput`.

**And the worst direction of error already has a standing guard.** A change that gets it backwards
shows in the snapshot set and in `DotGram.Compatibility`, which builds three frameworks with
warnings as errors and whose whole assertion is that building succeeds — the one project shaped
like a consumer's build. That is what makes this safe to do: the failure mode we most feared is
the one thing already watched.

**What the narrow condition buys, said exactly.** Wherever a field is meant to be assigned, CS0649
is armed again, so a grammar that stops writing one is reported rather than silently defaulted.
`Looking` is the single place where "never assigned" is the design, and keeping its pragma says so
in the code — which is the difference between a suppression that documents an intention and one
that hides the absence of a check.

## D113 — A break is counted from the last published version, not from this morning

I called the API merge "the sixth break" and it is not one. The release notes are written from
**0.1.0**, and in 0.1.0 `FixMessages` was never public — there was one door, `Fix44`. For the
reader those notes address, nothing breaks; what changes is where the migration arrows already
there now point: `Fix44.Parse(wire, Strict)` becomes `FixParser.ParseMessage(wire)` rather than
`FixMessages.Parse(wire)`.

**Counting from an unreleased intermediate is the same error as calling a break free.** Both take
a state that exists only in our working tree and treat it as the one a consumer holds. The count a
consumer needs is against what they can install; the count of what changed since this morning is
ours, belongs in the journal, and means nothing on a page they read.

**And a rename anchors on a token boundary, not on a substring.** `"FixParser.Parse("` is a
substring of `"HandFixParser.Parse("` and `"IdealFixParser.Parse("`, so the first mechanical pass
renamed the hand-written parsers in fourteen files. The compiler caught it because these are
identifiers; the same mistake in a string literal would have been silent, and the session that made
it had made the same one in the same area a month ago. The rule is cheap and the exception is not.

**The rest of that pass is worth copying.** Rename everything to the buffer form, then take the
streaming form wherever the type checker objects: fifty-six errors are fifty-six places that need
it, and none is decided by eye. Where a rename has a semantic half, the type checker is the oracle
for which half each site is.

**And one revert inside it is the day's rule in miniature.** Two summaries describing what the
binder looks for **on any side** were renamed with the rest and then put back: prose about all
forms must not be narrowed to ours. A mechanical pass reaches text that is not about the thing
being renamed, and only reading stops it.

**And the method behind that acceptance is worth more than the acceptance.** The instinct when a
change has two conditions that might diverge is to BUILD the case where they do. Looking for it in
the tree first cost one search and found it already shipped — `Looking` in FIX — which is better
than a fixture in three ways: it exercises the real emitter rather than a miniature, it cannot be
written to pass, and if it ever stops diverging that is itself news. **Before constructing a case,
look for the one already there**; a repository of twenty generated files and a hundred grammars
usually has it.

**A note on the shape of the pool bound now written.** The value tables' total is carried at the
two sites where a table grows and read once where the bound is tested — free where it changes,
O(1) where it is used. That is the general answer whenever "summing it would cost more than the
bound saves" is the reason for not measuring something: the sum is not taken at the moment of
asking, it is maintained at the moments of changing, and those are few and already written.

## D114 — A collapsed premise returns the decision to whoever made it (Igor)

Igor's correction of the architect, and it is about how this file gets written. We decide
something; the decision turns out to rest on a misreading of the material; and I then change the
decision **myself** and report the new one as settled, without his knowing the ground had moved.

**He decided given what he was told.** When the premise is corrected the decision is his again.
Re-taking it alone — however plainly the new facts point — converts a choice he made into a report
he receives, and the second is worth much less than the first: a report can only be accepted or
argued with after the fact, while a choice can be made differently.

**Three from one evening, named rather than summarised.** From his rule that the package has one
door and the rest is data, I derived that `FixValues` becomes internal and dispatched the work —
the rule was his, that application of it was mine to bring. I approved a pooling change and
withdrew the approval an hour later when its own author corrected the diagnosis; he never knew
either had happened. And I authorised a user-visible refusal message on a premise relayed through
two sessions, then withdrew that too. Each new call was defensible. None of them was mine to take
alone.

**So: when a premise under his decision collapses, four things go back before anything is
dispatched** — what he decided, what the premise was, what it actually is, and the choice as it now
stands. Saying "the ground moved" is the whole of it, and it costs one message.

**The boundary, so the rule does not swallow the work.** Corrections to my own decisions inside the
delegated area stay mine, and executing something he has already approved needs no second
confirmation — that is a standing instruction and it is not what this is about. This is about a
decision of HIS, resting on something that turned out not to be the case.

**And it explains a pattern in this file.** A dozen entries here are corrections written after the
fact, in the voice of a conclusion. Several of them should have been questions asked before the
correction was acted on, and the file would be shorter and the work slower — which is the right
trade, because a fast wrong decision is the only kind this arrangement produces quickly.

## D115 — One static field a message, and a store of named rules (Igor)

Igor's design for validation, replacing D69's validator object and most of what grew around it.

**The relation between a message type and a class is one to one**, so the rule lives where the type
already does: **one static field on each message class**, holding the rule in force for it.
`message.Validate()` reads its own class's field. There is no validator instance, no table keyed by
anything, and nothing to look up — the class IS the key.

**`LoadDictionary` is a method that does the work and ends.** It reads the file, rewrites the
fields, returns nothing, and **throws** where the dictionary is wrong: a consumer tests their
dictionary before deploying it, and a silent partial load would be worse than a refusal.
`FixDictionary` stops being public with it — the door takes a stream and the parsed file is ours.

**`FixValidator` stays, meaning something else: a store of the rules this package compiles in**,
as named public static methods — `FixValidator.ValidateReject` and its ninety-two siblings, each
with the shape the field takes. It is not a second entry point: nothing is called *into* the
library through it. It is where a consumer finds the default by name, which is what makes replacing
one recoverable — they keep the name, not a copy.

**What this gives up, named rather than discovered.** One static field a message is one
configuration a process: two counterparties with different dictionaries in one process become
inexpressible, where an instance validator expressed them. Igor's call, and made knowing it — he
sees no scenario for it, and a scheme that is one field and one assignment is worth more than a
case nobody has.

**And one consequence of the hierarchy being closed, which is open.** A counterparty's dictionary
may describe a message type we have no class for. With a field per class, such a type has nowhere
of its own to go: it reaches `FixMessage.Custom`, whose single field then stands for every unknown
type at once. That follows from the classes being a closed set and the types not being one; it is
not a defect of this design, but it is the one place where "one to one" stops holding, and what
should happen there is still to be said.

**The reason for the withdrawal was wrong; the withdrawal stands, on a better one.** The bound now
counts the value tables, and on all eighteen rows the retention is **byte-for-byte what it was
without it** — the cliff rows still hold 19.8 to 24.9 MB and still never let go. So the blindness
of the bound was not what put twenty megabytes on those rows.

**And the arithmetic that says so was three lines away the whole time.** The bound counts ELEMENTS,
at most 1,048,576 of them; in SQL:2023 all but two of the three hundred and two value tables are
reference types, so an element is eight bytes and a store under the bound can hold about ten
megabytes at the outside. The rows hold twenty to twenty-five. The memory is therefore not in a
pooled store under the bound at all — and the second half of that sentence, "probably not in
`DirectValues`", is the part nobody has established.

**What that does to the three commits.** The release fix is measured and works. The dense trade is
measured and large. The bound fix is correct on its own terms — the old bound really was blind to
as much as eight megabytes — and **inert on every row we have**. And the twenty-odd megabytes are
unexplained.

**So the change waits, and the reason is better than the one it replaces.** Before, it waited
because the flaw was in it; now it waits because the flaw is somewhere nobody has named, and the
change sextuples it — from three and a half megabytes to twenty-five — for a cause we cannot state.
Landing an enlargement whose mechanism is unknown is worse than landing one we understand and have
priced. The readout of every pool's slot on the parsing thread turns "which pool holds it" from a
prediction into a reading, and it costs no window.

**Twice in one night the same session reasoned from a mechanism it had not priced** — sixteen bytes
for a one-field struct, and a bound blind to where the memory was — and both times the number was
three lines away. Naming that is worth more than either correction: **the cheapest measurement is
the one that would have stopped you writing the commit, and it is almost always an arithmetic on
sizes you already know.**

## D116 — The bound governs parking, not keeping; and a unit price without its count

A pool keeps up to **four** stores, not one: `_spare` and three `_deeper` slots, **neither bounded
nor ever released**. Four stores under the element bound is some forty megabytes with nothing in
the code that would give any of it back, and the observed nineteen-point-eight to twenty-four-point
-nine is two or three of them.

**Which explains the thing nobody could explain: why counting the value tables changed nothing.**
The bound decides whether a store is **parked**. It does not decide whether a store is **kept**.
Anything under it goes to a spare or a deeper slot unconditionally and stays there; counting more
in the bound can only move a store *over* it, into the one slot that does have a release — and none
crossed. An account that explains the observation you could not explain is worth more than one that
fits the observation you were looking at, and this one also fits the byte-for-byte identity that
made no sense an hour ago.

**So the next item is four times what it was priced at.** Not "a bound and a release for the
ordinary spare" but for the spare **and the three deeper slots**, which are the same shape and were
simply not in view.

**And the error that hid it has a name worth keeping separate: a unit price used without its
count.** Ten megabytes a store was right; one store was the assumption, and nothing had been read
that said so. It is the third of the same family in a night — sixteen bytes for a one-field struct,
a bound blind in the wrong place, and now a per-unit size without its multiplicity — and every one
was arithmetic over numbers already on the screen. That is distinct from not knowing a mechanism:
the mechanism was known each time and the multiplier was never asked for.

**The falsifier is stated before the reading, which is what makes it a hypothesis rather than an
account.** If the readout shows two or three occupied deeper slots of a few megabytes each and the
parked slot empty, this holds. If the slots are near-empty, the memory is somewhere still unnamed —
and the same session will have been wrong twice about the same twenty megabytes, which is worth
saying out loud in advance rather than discovering a reason afterwards.

**Three consequences of the form, met while building it.** A static field is process state, so the
FIX tests stopped running in parallel: a test that loads a foreign dictionary changes what every
other test sees while it runs. Collections are few and xunit runs them together, so the suite now
runs its collections one at a time — four and a half seconds against two. That is the price of the
shape, paid rather than avoided: a suite that passes because two tests happened not to meet is
worth less than a suite two seconds slower.

**A rule cannot be compared with `ReferenceEquals`, and the reason is worth stating correctly.**
`FixMessage.NewOrderSingle.Rule == FixValidator.ValidateNewOrderSingle` is true, and
`ReferenceEquals` of the two is false. A consumer asking "is our rule still in place" compares
with `==`. That belongs on the page, not only in a test.

**Both explanations offered for WHY were wrong, and the second was mine.** "Every method-group
conversion allocates" is false: since C# 11 a static one is cached. "It is cached per conversion
site, so two sites give two instances" is also false, and a test says so — two conversions at two
sites in one method returned **the same object**. Where the cache's boundary actually is has not
been established and is not asserted here. What the page carries is what a test holds: compare with
`==`, and whether it is also the same object is the compiler's business and may differ between call
sites, assemblies and versions. The correct statement survived; the reason under it did not, and a
reason nobody ran had no business being written down.

**And undoing one rule is an assignment while undoing a whole dictionary is ninety-three.** The
name makes a single replacement reversible, which is what it was for; a load has no such handle,
and an operation a consumer can perform but not reverse is a trap. Restoring the compiled set
exists internally for the tests, which must return the process to where they found it. Whether it
becomes public is Igor's, and the case for it is that `LoadDictionary` is public and changes the
whole process.

## D117 — Three plausible causes in one day, and nobody ran any of them

Today a stated cause survived until its first execution three times, and each time it was wrong:
`FixValues` is public "because generated code in the consumer's assembly reaches it" — no generated
file references it; "collection initializers are not supported" — they are, and only the
constructor's parentheses were missing; and "a method-group conversion gives a new object each
time / is cached per site" — neither, by a test that took minutes to write.

**In all three, both people in the conversation were wrong, and both were experienced with the
material.** Agreement is not evidence: two plausible accounts of the same mechanism agree with each
other for the same reason they are both plausible. What none of the three had was an execution —
not a deeper reading, not a second opinion, an execution.

**And the architect is where the cost is paid.** A cause repeated between sessions is talk; a cause
written into this file is repository fact, and the next reader takes it without re-running anything.
So the rule falls hardest here: **a mechanism goes into a decision only when something has run it**,
and where nothing has, the entry says what is held and stops — as the page for this one now does.

**The cheap form of "run it" is worth naming, because it is not a project.** A console of thirty
lines, a unit test, a `Debug.Assert`, a grep over the generated output. Each of today's three cost
under ten minutes, and each was available before the sentence was written rather than after it was
believed.

## D118 — An instrument that cannot see a release, and a correction that over-corrected

The readout settles it, and it inverts the retraction. Case-1738's 19.8 MB is **one**
`DirectValues` in the spare holding 1.43 million elements across 305 arrays — far over the bound,
sitting in the spare because the old bound counted only the record tables and could not see it.
With the new bound that store is parked, and with a collection after each small parse it falls from
20,018 KB to 3,012; without the new bound it stays at 20.0 MB. **The first mechanism was right.**

**Why it read as inert is the finding worth keeping.** A store that is let go is held only
**weakly**, and the next rental takes it back if no collection has run in between. Eight small
parses in a tight loop allocate nothing that collects — so "released" and "released and retaken"
are the same column. The instrument could not see the difference it was built to measure, and
nothing in its output said so. **Where a measurement asks whether something was released, and the
release is a weak reference, the measurement is meaningless until a collection is forced.**

**And the arithmetic agrees where it should**, which persuades more than the direction of the
result: below the step, where a store genuinely is under the bound, the readout gives 8.5 MB
against a prediction of about 8.4 of values plus 2 of records.

**The retraction was a new failure, not one of the three, and its shape is the dangerous one.**
Having just learned a real error class — a per-unit price used without its multiplicity — the
session applied it to a case where the multiplicity was one, prompted by a table that showed a
correct fix looking inert. The question not asked was "what could make a correct fix LOOK inert".
**That is over-correction wearing the clothes of rigour**, and it is the specific risk in learning
an error class at speed: the newest lesson is the one nearest to hand, and a surprising result is
exactly when reaching for it feels like care.

**So, by the criteria named before the reading:** the bound fix stays; the next item is the bound
and release for the four ordinary slots, the residue now being named rather than unexplained —
`Ways._spare` at 2.2 MB and the lexer's 0.15, both unbounded spares older than this chain; and the
dense decision comes after that. The timed pair confirms the trade in both orders.

**And a build made to be thrown away is the right instrument for a residue.** One to three per cent
on the small SQL rows, sign not flipping with the order, and a pair that cannot get under the
noise: replacing the one candidate with a constant answers whether it is the cause, in a build that
cannot land and is not meant to. A question asked by removing the suspect is cheaper than one asked
by measuring around it.

## D119 — Stop isolating where the remaining cause is one the apparatus cannot separate

The one-to-three per cent lean on the small SQL rows stays **measured and unattributed**, and the
isolation stops. The reason I would put first is not the cost.

**No remaining candidate is arithmetically sufficient.** The `used` capture is measured and
innocent — a throwaway build replaced it with a constant and moved nothing, −0.2 to +0.2% over
twenty runs in both orders. `Tables +=` cannot be it: it arrived after the lean. The best remaining
one — a dense `Return` now evaluating three field loads and a compare where a dense parser
evaluated no bound at all — is a nanosecond or two against rows measured in microseconds. What is
left is an accumulation of such additions, or the code layout and inlining that come free with any
change to a method the JIT was already deciding about. **The second cannot be separated by anything
we have**, and spending windows on a question the apparatus cannot answer is the failure we wrote
down twice tonight, in advance this time.

**The trade is stated rather than implied.** One to three per cent on `select20`, which is what
consumers actually run, against twenty-eight to sixty-five per cent on the cliff rows and a tenfold
fall in allocation. That is a trade worth taking, and the report says so in those words — including
that the common path pays, because a reader who finds the cost later and no sentence about it will
reasonably conclude nobody looked.

**What goes in the report is the honest middle sentence.** Not "explained", which we cannot write;
not silence, which leaves it to be rediscovered as a surprise; but a named row with its number, its
rows, and the note that no single-line candidate accounts for it. The next person gets a starting
point rather than an ambush, and the condition for reopening is written with it: if the lean grows,
or if another change lands on the same path, it is measured again.

**And the discriminator that did most of tonight's work deserves recording on its own.** *A change
belonging to the commit flips sign when the sides are exchanged; a bias of the second slot keeps
its sign.* Three small-row suspicions carried since the refusal-rent pair — a JSON array, a media
type, an orders row — dissolved under it. This lean is the one that did not, which is precisely why
it was worth asking about rather than dropping.

**Corrected within the hour, and the correction strengthens the decision while weakening one of its
premises.** I wrote that the `used` capture is "measured and innocent". It is not shown innocent.
The stand qualified the figure: the lean is +1.1 to +2.8% over two orders, and **the two orders
disagree by as much as eight points on the same row**. On an instrument whose own disagreement is
eight points, a genuine one-per-cent line would have measured exactly as flat as the capture did.
The throwaway did not exonerate the candidate; it showed that the apparatus cannot tell.

**Which makes "keep isolating" not a slower path to an answer but no path at all.** Every
sub-component of a one-to-three per cent effect has a share smaller still, and every share is far
under the resolution that would have to distinguish it. That is a stronger reason to stop than the
one I gave, and it is about the instrument rather than about the candidates.

**So the sentence in the report carries the resolution beside the number**: measured, unattributed,
one to three per cent on small SQL parses, sign stable under exchange of order, on a pair whose
orders disagree by up to eight points. A reader who does not know the resolution cannot tell a real
one-per-cent cost from an artefact — which is exactly what nobody here could tell.

**And the thing worth wanting is not more subtractions but a better instrument.** If a tax on every
small SQL parse matters, the answer is enough runs to bring the disagreement under a point — a
different order of machine time, and a decision about the stand rather than about this chain. Not
started, and worth **pricing** on its own: a stand that cannot resolve one per cent cannot answer
any future question at that scale either, and there will be more of them.

**And the numbers behind that, now that the stand has given them.** The throwaway's pair disagrees
by up to **1.8 points** when there is nothing to find, with an A/A of ±1.9. So the experiment
establishes "the capture costs less than about one to two per cent" — and the lean is one to three.
A cost of 1.8 is **not excluded**; on some rows it could be most of it. The honest sentence is that
the capture **is not shown to be the cause and is not shown not to be**, and the zero goes into the
README with its resolution beside it so nobody reads it as tighter than it is.

**A null result from an instrument whose resolution is the size of the effect is not evidence of
absence** — and it is hardest to see when the null points away from you, which is the direction a
careful person has been trained to trust.

**The alternative is priced, which is what closes the question.** ±0.5 points needs forty runs an
order: about five hours for both orders without the A/A, ten with, in windows nobody else can use.
And that buys the NUMBER, not the cause — each one-line subtraction would then need its own forty
runs to read a half-per-cent share. Ten hours for a figure, and multiples of it for an attribution,
against a one-to-three per cent lean on the common path. Not now, and the price is in the record so
that "not now" can be revisited with the cost already known.

**So the sentence is: a one-to-three per cent lean on small SQL parses, sign stable under exchange
of order, cause unattributable at the resolution of the pair that measured it, which was ±1.8
points at ten runs an order.**

**Corrected: the resolution belongs to the pair, not to the stand.** The same rows in the same
apparatus disagreed by 1.4 points in the morning and 3.4 in the evening. So "the stand's
resolution" is not a quantity — it is a property of the hour, measured with each pair and never
carried forward. Quoting the morning's figure against the evening's data would have claimed a
ceiling twice as tight as the instrument could support, which is the throwaway's error one level
up: a number read as tighter than the thing that produced it. The resolution belongs in the sentence; without it a later reader repeats tonight
exactly, including the part where a flat throwaway looks like an acquittal.

**And the reason this one needed two sessions is worth keeping.** Three of tonight's corrections
were arithmetic their author could have done alone. This one was not: **the resolution of an
instrument is knowledge its owner holds**, and the person reading the number usually cannot derive
it. So a figure handed over carries its resolution the way it carries its units — and where the
receiver cannot ask, the owner volunteers it.

## D120 — The dense trade lands; and three rules the landing turned up

**The ruling, asked for before the four-slot work because that work's shape depends on it: the
dense change lands, together with the bound that made it legible.** Every condition named before
the clean table is met; the twenty-odd megabytes are no longer unexplained but **explained and
bounded** — one store over the bound, parked by the new bound, falling from 20,018 KB to 3,012 once
a collection runs — and a bounded cost is one that can be accepted, which is the sentence I gave
when I deferred. The timed half is −28 to −65% on the cliff rows, confirmed in both orders. The
residue goes into the report as agreed, with its resolution inside the sentence. The four slots
follow, and are sized for dense stores because dense stores now exist.

**A correction written above a claim does not retract the claim.** An entry was found still
carrying "it also costs about two to three per cent on the largest documents" three paragraphs
below the paragraph that had withdrawn exactly that reading. Second time this week. So D107 gains
its missing half: in an editable record, **the withdrawn sentence is deleted, not merely
contradicted**. What may stay is narration — that it was believed, and why — in the voice of
history. What must not stay is the claim in the assertive voice, because that is the sentence a
reader will quote.

**A sweep asserts a floor, and the floor is what makes a filter defect legible.** The shape probe
tested an absolute path for `\.work\`, so in a worktree under `…/performance/.work/wt-main` it
filtered out every grammar in the repository — and said so, loudly: *"Only 0 grammars were read;
the walk over the repository is broken."* A sweep that had merely returned an empty list would have
passed, and the defect would have been a clean answer that stopped the search. We have collected
this class all night from the negative side; this is the positive instance, and the rule it gives
is cheap: **every sweep asserts that it found something, and says what it expected to find.**

**And fixing rather than reporting, across an area boundary, was right here.** Four things held
together: it blocked the fixer's own gate, the intent was unambiguous and written down in the
commit that introduced it, the repair was one line, and the owner was told. Where any of those
fails — an inferred intent, more than a mechanical change, no blockage — the finding is reported
and the work stops. The telling is not the optional part: it is what leaves the owner able to
disagree.

## D121 — The observed figure is a workload; the ceiling is the promise

The ordinary slots were described here as unbounded. They are not: the admission test is the
corrected dense one, on capacity including the value tables, and the idle counter inside is on
usage. **What they lack is not a bound but a release.**

**And the arithmetic now carries its multiplicity.** A store admitted to an ordinary slot holds up
to 1,048,576 capacity units; for a dense store the dominant term is the value tables at eight bytes
apiece, so about eight megabytes a store, and there are four slots — **some thirty-five megabytes
per pool per thread at the ceiling**, against the 2.35 MB the instrument reads today. Both belong
in the record and they answer different questions: **the reading is a workload, and the ceiling is
the promise.** A consumer's workload is not ours to assume; the ceiling is what we are committing
to, and it is the number a reader should meet first.

**The order is decided by a count, not by an argument, because the count is free.** How often is
the deeper count above zero at the end of a parse, across the shipped grammars? The deeper slots
exist for a parse re-entered from inside another — an interpolated hole, a guard, a value that
parses — so a thread that never re-enters never fills them and a thread that re-enters once holds
them for ever after. If the count is near zero outside the re-entrant families, then releasing the
three deeper slots is nearly the whole residue and the fourth is buying a last tenth; if it is not,
that is known before anything is written. No window, no timing.

**Then the three deeper slots, then the spare, as two commits.** The first changes nothing on the
common path, which makes it free in the sense that matters, and it makes the second's measurement
honest: with the deeper slots released, whatever the spare still holds is the spare's and not three
neighbours'.

**Lowering the admission bound is refused.** It would make the parked path the ordinary path for
mid-sized documents — changing behaviour for every consumer to avoid writing a release. A constant
is cheap to change and expensive to have changed.

**And the flagged concern is the right one to flag.** Demoting the spare to a weak reference puts a
weak reference near the common path. If the demotion happens only on expiry, the ordinary rent and
return never touch one — but that is a reading of the emitted code, to be confirmed by reading it,
not settled by this sentence. Tonight has cost us three mechanisms asserted without execution; this
one is named as unconfirmed on purpose.

**The count inverted the proposal, which is why it went first.** Across eighteen rows and three
sides the deeper slots **never appear**. What holds the residue is the spare, whole: 8,542 KB of
the value store's, 2,183 of the ways', 123 of the lexer's. So releasing the deeper three is free
and buys nothing measurable, and the spare's release is not the last tenth — **it is the entire
work**. The author had proposed the opposite order on a suspicion, and the file that settled it was
already on disk.

**And the absence was shown to be evidence rather than silence**, which is the step that usually
goes missing. The readout reflects over every thread-static field of the parser's types and their
nested types, drops a field only when it holds no array, and carries a case written for exactly the
array-of-stores shape the deeper slots have. They were looked for by name and by intent and held
nothing; the array is made lazily and was never allocated. **An absence counts only where the
instrument has been shown able to see the thing that is absent** — the positive form of the failure
we met four times tonight.

**What the count cannot say, said before anyone quotes it further.** The deeper slots fill only
when a parse begins while the spare is taken — an interpolated hole, a guard, a value that parses.
If no row drives a re-entrant parse, the reading cannot tell "they stay empty" from "these rows
never re-enter", and the honest sentence is the second. **Their ceiling is untested, not zero** —
three slots of some eight megabytes each, twenty-four of the thirty-five we would be promising.

**So: the spare's release now, and the second count after it** — which shipped grammars can
re-enter at all, and a readout after a suite run rather than a stand row. Not to order the work,
which the first count settled, but because a slot that never fills in any shipped grammar is not a
slot to release; it is a slot to question.

**The spare's release, read in the emitted text before landing.** The new weak slot is the **last**
branch of `Rent`, after the spare, the deeper array, the parked slot and its weak retake, so an
ordinary rental never evaluates it — which is the flagged concern answered by reading rather than
by either of our accounts of it.

**Both constants are arguments, and both are grounded in the data rather than in taste.** The
quantity compared is the **record tables against this parse's record count**, not the store against
its use: a dense store's three hundred value tables each grow to the largest record index written
in them, so their capacities sum to several times the record count **even when every one fits** —
the readout's own 305 arrays and 1.43 M elements are that arithmetic — and a test on the total
would demote a perfectly fitted store. `Live` doubles to hold the records, so room and use are read
in one unit.

**And the factor is four because growth doubles.** A table grows to the larger of the count and
twice its length, so a store serving a steady workload sits between once and twice what that
workload needs: **one doubling is ordinary slack**, and a threshold at twice would demote the spare
of every steady parse in the world every eight parses — a behaviour change on the common path
dressed as a bound. Two doublings cannot be reached by slack, only by a workload that actually
shrank, which is the thing the release exists to notice.

**A second construct this night that would have broken a consumer's build, caught by the same
guard.** A pool with marks and no stacks has no usage to read, and the first form gave it the
constant `false`, which emits `if (!(false))` — CS0162, unreachable code — plus two fields nothing
assigns. It failed in the Visual Studio tests at net472: **in the consumer-shaped build and not in
ours.** After the CS0649 pragma this is the second, and both were caught by the one project whose
whole assertion is that a build succeeds. That project earns its place twice over, and the argument
for running it early rather than last is now made of two instances instead of none.

**Landing on a transitions test, pairing after — and the pair must force a collection.** The
counter is read at every step, because a counter that never advances and one that advances and
resets are the same from outside. And the retained reading that follows has to force a collection
before it looks: a released store is weakly held, and the instrument that could not tell released
from released-and-retaken was this same reading without that step.

## D122 — "X is the exception" is a question about what else is

The pool chain removed a cliff and put a quieter one back **in the slot it had just given a release
to**. The admission test had been corrected to compare commensurable quantities; the idle test one
slot over compared the parse's use against the **bound**, while the room that put the store past
that bound is three hundred value tables. A store of 1,428,064 elements reads a use of about
524,288 — under the bound, on the parse that had just filled it — so every steady parse read as
idle, the eighth demoted and the ninth rebuilt seventeen megabytes. Twenty identical parses swung
from three megabytes to twenty. Now flat.

**The rule its author proposes, and I take it.** Having established that a dense store's room is
not commensurable with its use, they wrote it into a commit as *the* exception and did not look at
the slot next door, where the same incommensurability was already deciding when to throw the store
away; the lexer's guess against a count was a third instance. **A sentence of the form "X is the
exception" is a question about what else is, and is not finished until that question is asked.**
It is the positive twin of the rule that a count the mechanism explains is not a finding: there the
mechanism dissolves a false finding, here it explains one case and closes the search.

**And an instrument lesson that cost a day.** Every reading in this work took the **end state of a
run**, and a store demoted and rebuilt *inside* a run is invisible to all of them. What saw it was
a per-step column — twenty parses, a collection after each, the heap read after each, the low and
the high printed — built to answer a different question that turned out to have a clean answer.
**An end-state reading cannot see a cycle within the run**, and where a mechanism can undo its own
work between steps, the low and the high are the measurement and the final figure is not.

**The floor build is promoted from "last" to "part of an emitter change".** It has now caught three
defects in one chain, every one a shape no suite could see because every one is about what a
CONSUMER's compiler does: unreachable code from a constant condition, a value nothing reads, a
field nothing assigns. An emitter change is not finished until it has run.

**And the rule rests on the class, not on the tally** — the correction is its author's. Our suites
cannot see these, not because they are weak but because they ask a different question: they run the
parser, and this one only asks whether it compiles. Three cases did not earn the rule by being
three; they earned it by being **categorically invisible to everything else we have**, which a
fourth would not strengthen. Counting instances is how one argues for prudence; naming the class is
how one argues for a rule.

**How the second condition is obtained, ruled before it is built: observe it, do not re-derive it.**
The struct is written before any assignment site runs, so the two ways to know whether a field is
assigned are to restate four scattered emission conditions as predicates at a place that cannot see
them, or to have each site that emits an assignment **record that it did**, with the struct
carrying a placeholder settled once the file is assembled. The first is the conflation this
decision exists to stop, and getting one predicate wrong breaks a consumer's build under
warnings-as-errors. The second cannot conflate them because it does not derive one from the other:
for it to answer wrongly, the assignment site would itself have to be wrong, which is the parser
being wrong.

**Resolving part of the emitted text at the end of assembly is approved, and the emitter already
works this way** — a mark stands for a state whose name is not known when it is written, and
settling puts the name in. **Two conditions carry over from that mechanism.** An unsettled
placeholder must fail generation loudly, as an unsettled state mark already does, rather than
quietly emitting nothing: a placeholder that silently resolves to "no pragma" is the forbidden
direction wearing a different hat. And the output stays byte-identical for the same grammar
whatever the internal order, which the snapshot set already checks.

**And the resemblance found on the way is not to be acted on.** For one field the emission
condition and the assignment conditions look close to identical, which would mean the pragma is
never needed and the field should lose it outright. **Proving that identity requires exactly the
derivation we are refusing** — and it is the third time this week that one condition has turned out
to be two. The observing mechanism makes the question moot: where the answer is always "assigned",
the conditional costs nothing and a grammar nobody has written yet cannot break it. **A regularity
observed across the grammars we happen to ship is not a property of the emitter.**

## D123 — Measuring ourselves against somebody else's library (Igor)

Igor wants BenchmarkDotNet figures for SQL and, above all, for FIX, compared with the existing
libraries in two modes: the schema compiled in, and a dictionary loaded from outside.

**What is already there, read rather than recalled.** `ScriptDomBenchmarks` is a BenchmarkDotNet
class and already measures us against ScriptDom — tokens, tree, located, grammar. QuickFIX/n is
referenced too, but **only from the stand and only as a correctness oracle**: `FromString` with the
dictionary, then `DataDictionary.Validate`, asked whether it accepts. There is no timing comparison
with it anywhere, and `DotGram.Finance.Benchmarks` does not reference it at all. So the SQL side is
an extension and the FIX side is new work.

**The comparison is like for like, and where it cannot be, the asymmetry is the row's headline
rather than its footnote.** Three conditions decide whether these numbers are worth taking:

**The same work on both sides.** Their call parses and validates in one; ours parses and validates
in two. So a row is *parse* against *parse*, and *parse and validate* against *parse and validate*
— never one of ours against both of theirs.

**The fields are read.** Ours is source-backed and lazy; theirs builds a field map eagerly. A row
that parses and never looks at a field measures our laziness and calls it speed. **Every row reads
a stated number of fields on both sides**, and says how many.

**Their fast path, chosen from their documentation.** A competitor configured badly is not a
measurement, it is a claim we would not let anyone make about us. What was chosen, and on what
authority, goes in the file beside the number — as does their version, read from the package.

**The two modes are ours; the asymmetry is the finding.** We have a schema compiled in and a
dictionary loaded at run time; QuickFIX/n has only the second. That is not a flaw in the comparison
— it is the thing the comparison is for, and it is stated where a reader meets it. Loading is its
own row: their dictionary is read at construction, ours at `LoadDictionary`, and a startup cost
belongs beside a steady-state one rather than inside it.

**Allocation is measured with time**, since the road we did not take was chosen on allocation as
much as on nanoseconds.

**And nothing comparative reaches a shipped page without Igor.** Numbers about a named third
party's software are exactly the claim a page of ours cannot keep true, which is already decided;
internal, dated results files are where they live until he says otherwise.

**The comparison lives outside the repository (Igor).** QuickFIX/n is not referenced from anything
of ours: a throwaway project under `.work/`, which git ignores, with its own package reference and
a project reference back to `DotGram.Finance` — the shape `.work/genprof` already uses for the
profiling harness. It is not in the solution, it is not published, and nothing about their licence
has to be decided, because nothing we distribute touches their package. **The numbers are ours and
go in the dated results; their code stays on this machine.**

**That supersedes what I told the owner an hour before** — that adding a package reference to an
existing benchmark project would be fine because it is not a new project. It would have been fine
by that rule and wrong by a rule I had not considered: a third party's licence becomes a question
the moment their package is referenced by anything we build as ours. Igor's answer removes the
question rather than answering it, which is the cheaper of the two.

**ScriptDom is untouched by this.** It is Microsoft's, it has been referenced from
`DotGram.Benchmarks` for some time, and the SQL comparison stays where it is.

**Two cautions for the throwaway.** Central package management is off in it, as in the other
`.work` harnesses, so it does not fight ours. And nothing with a `.gram` extension is left there:
a sweep over the repository read `.work` once already this week and reported a scratch grammar as
a finding.

**And the first thing the gap list found is that the existing headline cannot be read.** The
T-SQL-against-ScriptDom class times **one operation over the whole corpus** — a short `SELECT` and
a three-hundred-line procedure averaged into a single figure — and the divisor is a property
BenchmarkDotNet never prints, so the per-statement number cannot be recovered from the report by
anyone, including us. **A number whose divisor is not in the report is not a slow way to the
answer; it is not an answer**, and that outranks adding coverage: the first deliverable is a figure
that can be read, not more figures. So the order is the readable ones first — buckets by statement
length as a parameter, ScriptDom's one configuration flag written down where it is chosen, its
version read from the loaded assembly rather than from the props file — and the missing parser and
the walking row after.

**One row is honest only if it says what it lacks.** Their lexer is timed alone and ours cannot be,
because ours is internal to the generated parser. Rather than expose a tokenizer to make a pair,
the row says it has no partner — otherwise a reader puts their lexing beside our whole parse and
the comparison lies without anybody writing a false sentence.

**And nothing is timed until the stand has given the configuration.** The benchmark project has no
BenchmarkDotNet job and no affinity at all, so a run today inherits whatever the machine offers —
and the only affinity in that directory pins the half opposite to the one the timing rules name.
Which of those is the defect is the stand's to say; that it must be said before a number is taken
is not.

**The affinity is not a defect, and the rule is two-sided.** Timing runs on cores 0–15 at high
priority; everybody else's builds and tests run pinned to 16–31 so that the timing half is quiet.
Both halves are written down — in the benchmarks' README and in the development notes — and the
code carries them: the stand's timing process takes 0–15, the generator's gate takes 0–15 because
it times, the side-building script takes 16–31 because it builds, and the retained-bytes script
takes 16–31 below normal because it counts. The C# alone shows one half; the scripts carry the
other. **16–31 is not the stand's half — it is the half the stand asks everybody else to use.**

**What is true and unwritten is the part that bit us.** A build on 16–31 still shares the
last-level cache and the memory bandwidth with a window on 0–15; the README says so and a window
was spoilt by it once already. The stand holds its own builds until a window ends — **a practice,
never written as a rule for anyone else**, which is why my own build walked into somebody's window
tonight. It is a rule now: while a window is announced, no one builds, on either half.

**And the two instruments answer different questions, which decides how a comparison is written.**
BenchmarkDotNet runs one process per case: right for an ABSOLUTE number and for allocations, since
each case pays for its own garbage — and wrong for a RATIO between two cases on this machine,
where three default-job runs were once thrown away because two methods doing identical work came
out 21% and 28% apart. The stand is round-robin in one process precisely so that a change in the
machine hits every method alike. Igor asked for comparisons, which are ratios, on an instrument
that is weak at ratios. **So every BenchmarkDotNet class that compares two things carries an A/A
row — the same method under two names — and is read against its spread.** Without it the number is
a ratio quoted at a resolution nobody measured, which is the mistake this week has already paid
for twice.

**Pinning belongs to the launcher, and the claim is made true by a check rather than by belief.**
The script pins itself, announces the window, and then reads a running child's affinity and stops
the run if it is not the timing half — because whether BenchmarkDotNet's children inherit it is not
known, and an unverified inheritance is exactly the shape of an instrument that cannot see what it
was built to measure.

**Start-up rows are not warm-up rows.** A dictionary read at construction against one read at
`LoadDictionary` needs a fresh process per sample, not a warmed loop; they belong in the cold form
or in the first-call harness, and putting them in a warmed class would measure the second call and
call it the first.

**And an allocation-only run needs no window**: it reads no time column, so it runs on the building
half whenever the machine is free. That is worth having as a rule, because it is most of what a
consumer's question about memory actually needs. **"Whenever the machine is free" was doing work I
did not notice it doing** — see D125, where it had to be said outright: needing no window of its
own is not permission to run inside somebody else's.

## D124 — Expression-bodied methods: the rule existed, the enforcement did not (Igor)

Igor: never write a method with an expression body — it reads as a property. **The rule was already
in the coding conventions, in those words**, and handwritten code violates it about two thousand
two thousand eight hundred and sixty times, in 353 files: 1,436 in `src`, 1,055 in `tests`,
174 in `benchmarks`, 195 in `examples`. One of them was written tonight, hours after his own style
pass over the neighbouring files.

**That count replaces a smaller one, and the gap is the point.** My first figure was 2,652 from a
text search, and it agreed with the analyzer everywhere but `src`, where it fell 217 short: a
declaration whose parameter list wraps puts the `=>` on the next line, where a line-wise search
cannot see it. The analyzer's number is the one the pass is checked against, because it is the one
the build will be checking afterwards.

**So the finding is not the rule; it is that a rule living only in a document is not enforced.** It
went into `docs/coding-conventions.md` and was read by whoever happened to read it. The fix is the
analyzer: the style rules for method, constructor and local-function bodies set to error in
`.editorconfig`, where the next occurrence fails a build instead of waiting to be noticed. That is
this week's other lesson arriving in a third place — a rule in a document is followed by whoever
remembers it, a rule in the tool by everyone.

**The emitter is exempt, and by his instruction; it also needs no work.** Generated files open with
`// <auto-generated/>`, which is the marker Roslyn's own style analysis skips. So the generator
neither suppresses anything nor changes what it emits.

**That was read off the first line of a generated file and off a documented default, and it has
since been measured instead**: the three rules were turned on and the whole solution built —
twenty-six projects — and the `.g.cs` files carry **zero** marks. The inference was right and it
was not evidence; what makes it a fact is the build.

**The repair is a tool's, not a regex's.** `dotnet format` applies exactly these diagnostics and
nothing else; the check afterwards is that the token streams agree apart from the body form, then
the suites, then the consumer-shaped build. Two and a half thousand sites is precisely the size at
which a hand-written pattern quietly eats something — a rename earlier this week ate fourteen files
because one identifier was a substring of another.

**And it lands in one commit on a quiet tree.** Every session is working in `src`, `tests`,
`benchmarks` and `examples` right now; a pass of this size against live work is a conflict for
each of them. It is announced, everyone lands or parks, the pass runs, and everyone rebases —
which is cheaper than five partial passes leaving the tree in two styles.

## D125 — A quiet window is everyone's rule, and a queue is ordered by readiness

Two sessions asked for the machine in the same hour, and the answer to both was about something
other than preference.

**The queue is ordered by readiness, not by importance.** SQL goes first because its comparing
classes are written and want reading; FIX goes second because its comparison class does not exist
yet, and a window spent on a case that is not built measures nothing. Writing the class needs no
window at all. This is worth stating as the rule rather than the ruling: **a window is given to
work that is finished enough to be read**, and everything upstream of that — writing, building,
fixing — happens off the window, because it can.

**And no number is taken before the script that pins the run exists.** `benchmarks/Run-Bdn.ps1`
pins the measuring process to cores 0–15 at high priority, announces the window, and — the part
that is not decoration — **reads the affinity back from the live child process ten seconds in.**
BenchmarkDotNet starts a separate process per case, so pinning the parent says nothing about where
the arithmetic happened. A verification that reads its own intention instead of the instrument is
the failure this repository has paid for twice, and the cheapest place to stop it is in the script
that starts the run.

**The affinity rule is two-sided, and the second side had no owner.** Timing runs on 0–15;
everything else — builds, tests, packs — is pinned to 16–31 so that the half being measured is
quiet. Both halves are written down. What was *not* written down is that **while a window is
announced the machine is taken whole**: no builds, no allocation runs, no dry runs, on either
half. The two halves share the last-level cache and the memory bus, which `benchmarks/README.md`
says plainly, and one window this week was spoiled exactly that way.

**The first draft of this rule said "nobody builds", and that was a hole I left.** An
allocation-only run needs no window — D123 says so and it still does — and nothing in that
sentence stopped one from starting inside another session's window, because I had written the
exemption as a property of the run rather than as a property of the machine. It took the stand
catching a `MemoryDiagnoser` run overlapping its own test windows to show the gap. **An exemption
written in terms of what a run reads exempts it from the wrong thing**: what makes a window quiet
is not what the other process reads, it is that the other process is not there. The session that runs the stand had been holding its own builds until windows
closed — a personal practice, correct and invisible, which is the same thing as absent for
everybody else. **A discipline only one session observes is not a rule; it is that session's
habit, and it protects nothing outside its own process.** It is now stated to every session.

**Every comparing class carries an A/A row** — the same method under two names. BenchmarkDotNet is
strong at absolute figures and allocations and weak at ratios; a ratio without its measured
resolution was retracted from this journal once already this week, and an A/A row is the cheapest
possible statement of what the instrument can and cannot separate.

## D126 — A figure that changes every week does not live in a comment

Two counts went stale in the SQL benchmark's remarks at once, and neither was wrong when it was
typed. The text said "6,861 of the 8,397 ScriptDom finds"; the run says **7,716** statements both
parsers read, because the grammar has read more of that corpus every week since somebody typed the
old pair. And the ScriptDom version: the project asks for `180.102.0`, the loaded assembly answers
`18.0.102.0+9d1d1c1d…` — **not even the same shape**, which is the whole argument for reading a
version off what ran rather than off what was requested.

**So the rule is about which figures may be written down at all.** A number that is a property of
the repository at a moment — how much of a corpus is read, which assembly loaded, how many rules a
grammar has — is printed by the run that uses it, not stated in prose beside it. Prose is for what
does not move: why the bucket boundaries are 100 and 300, why a flag is set. The test is simple
and worth applying before typing any figure into a comment: **would this still be true next
month, and would anybody notice if it were not?**

**A second finding, offered against itself, is better than a second finding.** The same work
recorded the `initialQuotedIdentifiers` flag with two reasons and named the second one weaker: the
server answers `0` to `SESSIONPROPERTY('QUOTED_IDENTIFIER')` under `sqlcmd`, which is `sqlcmd`'s
default and not the server's. Evidence that looks like it settles a question and does not is the
most expensive kind we handle, and the cheapest treatment is to write it down *with* what is wrong
with it rather than to leave it out — because left out, it gets rediscovered and believed.

**And the sharpest sentence of the week is a correction of one of my own relays.** I took the
blame for a mis-stated affinity rule as a relay error; the session that made it declined the
excuse and put it better: *"the file I read was true and what I concluded from it was not — a fact
I had checked sat next to one I had not."* That is not an argument for reading the README. It is
the observation that **adjacency confers nothing**: a verified statement lends no standing to the
statement beside it, and a list mixing the two reads as uniformly checked to everyone downstream,
including its author an hour later.

## D127 — Three checks that were cheaper than the thing they checked

A benchmark comparison against somebody else's library produced three findings in an evening, none
of them about speed, and all three are about the shape of a check rather than about FIX.

**The negative arm was cheaper than the positive one, and that is why it goes unwritten.** A guard
asserts at the start of each case that the rules in the process match the case's mode — compiled
or a dictionary's — because `LoadDictionary` is irreversible inside a process and the way back is
internal. The session verified that the compiled case *passes*, and called that verification. It
is not: **a passing case is consistent with the guard working and with the guard being absent**,
and only the arm that must refuse distinguishes them. Proving the refusal cost one dictionary load
and one call — less than proving the pass. The session's own sentence is the finding: *"the
negative arm here was cheaper than the positive one, and the usual assumption is the reverse,
which is why it does not get written."* The cost of a refusal check is estimated without being
measured, and estimated upward.

**Two lists of one set diverge in silence.** The shapes were enumerated once in `[Params]` and
again by hand in the agreement check. Nothing fails when they drift — a newly added shape is
simply never checked, and the discrepancy it would have caught is the one the check no longer
looks for. Iterating the enum removed not the duplication but **the possibility of divergence**,
which is the more expensive of the two. It was fixed while adding a fourth shape, which is the
only three-minute moment the fix ever had: the defect existed precisely for that act.

**A comparison of one size cannot see a mechanism.** The table began with three message shapes
chosen by what the reader does — flat, a repeating group read entry by entry, a length-data pair
read by its length — each at one size. That is a table about three messages, and it reads as a
table about the protocol. A `Parties` / `PartiesLarge` pair at three entries and a thousand was
added **before the first number**, because a per-unit price taken off a small case changes
mechanism at scale, and a row added after a table has circulated costs an explanation of why the
earlier table claimed more than it knew.

**And the agreement was reported as arithmetic rather than as equality.** Both sides read 10,030
fields: a thousand entries of three, plus the order's eight. Two equal numbers say the sides agree;
two equal numbers *broken into their terms* say both sides walked the whole group rather than
agreeing at its start. The second count came by another road, and it came unasked.

## D128 — The comparison rested on a claim about the other library that nobody had asked it

The FIX comparison paired our `Validate()` against QuickFIX/n's `FromString(..., validate: true,
...)`, on the reading that a flag called `validate` validates. Asked directly — a correctly framed
`NewOrderSingle` with `ClOrdID` missing — the library answers otherwise:

```
ours                                     1 finding
validate: false                          accepted
validate: true                           accepted     <- here
validate: true + DataDictionary.Validate refused: RequiredTagMissing
```

Required fields are checked by a separate static call the session makes after parsing. **So the
"validating" side of the pair was doing less work than ours, and the table would have been wrong
in our favour** — the direction nobody goes back and re-checks.

**The check that found it cost one message.** Everything else about the comparison had been
measured: four message shapes, three mechanisms, two schema modes, agreement counted field by
field and broken into its terms. All of it was measured on *valid* messages, where both sides
answer identically — and no number of shapes or sizes on valid input could have exposed a flag
that only differs when something is wrong. **The assertion is tested by the arm where it must be
false**, which is D127's lesson arriving in a second place within the hour, about a claim rather
than about a guard.

**And my own instruction would have hidden it.** I had ruled invalid messages out of scope, and
that was right about a *row* — stop-at-first-error against collect-all-findings is not one work,
and a row comparing them must be explained rather than measured. It was wrong about a *probe*. An
invalid message used to ask the other side what its flag means is not a measurement at all, and
the session ran it anyway. **A scope written in terms of the input excludes the probe along with
the measurement**, which is the same mistake in shape as writing a window's exemption in terms of
what a run reads: the boundary was drawn around the material instead of around the act.

**The correction is not finished when the pair is levelled once.** Adding a call to their side to
catch ours up raises the symmetric question immediately: does their pair now do something ours does
not — a checksum, a body length, a field order — in which case the table leans the other way and we
hear about it from a reader. The answer is owed as two lists and their difference, not as an
argument; and whatever stays different is written beside the table as a known lean with its
direction named.

## D129 — Two conditions that sound like one, in four places in one evening

D112 needed to know which `CS0649` suppressions a generated file still needs. I approved recording
at the sites: the emitter marks that it wrote an assignment, and the suppression is kept where it
did. Verification refused it, and in the direction I had forbidden — **`CS0649` in a file its
author did not write**, caught by thirteen tests that compile emitted code.

**The cause is one sentence.** Compiling happens inside the machine's constructor and writes into
scratch writers, and a discarded branch's text never reaches the file. So recording at the sites
answers *"did the emitter run a line of code"*, and the question is *"is there an assignment in
the file"*. Two conditions that sound like one, and they part exactly where a branch is thrown
away.

**Reading the finished text is not the weaker observation; it is the compiler's own question.**
`CS0649` fires where no assignment to the field occurs in the compilation, so what decides the
suppression is literally what is in the file — comment lines excluded, because the generator
writes the field's name beside it. The acceptance was then taken off the shipped output rather
than the snapshots, and in both directions: eighteen fields keep a suppression and none is ever
assigned, forty-one lost one and every one of those is assigned, zero violations either way.

**And that shape appeared four times in one evening, each in different clothes.** A guard whose
passing arm was checked and whose refusing arm was not. A flag named `validate` that does not
check required fields. A reading the instrument did not take, reported as a reading of absence. A
rule stated without the address of the file that holds its state — mine — where *not checking* and
*checking and finding nothing* are indistinguishable from the inside. In every one of them, two
conditions were treated as one because no input had been built on which they differ.

**The general form, in the words of the session that put it best:** a rule whose state lives
somewhere — a file, a lock, a flag — has to carry that address in the sentence that states the
rule, or it splits into one rule for the people who already know where to look and no rule at all
for everybody else.

**The last instance closes itself.** The window file does not exist while no window is open, so
"no file" and "cannot check" look identical to whoever is about to build — which is the same
defect again, in the fix for the defect. It is now written once, empty, and kept: its absence
means the stand is broken, which is a thing that can be seen.

## D130 — FIX validation: the rule lives on the type, and the type system guards it (Igor)

Igor read `FixValidator` and said what was true: ninety-four names, one body —
`FixRules.Check(CompiledTables.Instance, message, findings)` — and no knowledge of any type in any
of them. A run proved the consequence: **the names are interchangeable.** Assigning
`ExecutionReport`'s rule to `NewOrderSingle.Rule` changes nothing, because every rule reads the
type off the message. Put back by any name, replaced by any name, and nothing says so.

**Which defeats the reason the class existed.** Its own remarks say the point is the NAME, so that
a replacement is undoable by assigning it back. Undoability rests on the names meaning different
things. They did not.

**The form Igor settled on:**

```csharp
public sealed class NewOrderSingle : FixMessage
{
    public static Func<NewOrderSingle, FixFinding[]> Validator = ValidateDefault;

    public static FixFinding[] ValidateDefault(NewOrderSingle message) { … }

    public override FixFinding[] Validate() { return Validator(this); }
}
```

Straight-line typed checks, written beside the type's own fields; no tables for the ninety-three
described types. The table walk survives exactly where it cannot be avoided — `Custom`, whose type
is unknown at build time, and a loaded dictionary. `Custom` carries the virtual method alone, with
no field: a consumer subclasses it. `FixValidator` disappears; the name lives on the type.

**The interchangeability is closed by the compiler, not by a check.**
`NewOrderSingle.Validator = ExecutionReport.ValidateDefault` does not build. That is strictly
better than the runtime comparison I first proposed — which, being a comparison of a literal with
the literal the constructor wrote, could never have failed at all.

**And the rule returns its findings rather than filling a list.** Igor's question — why not a
return — landed on waste: `Validate()` allocates a `List<FixFinding>` on **every** message,
including the valid ones, to discover it is empty. Returning makes the valid path `Array.Empty`
and builds a list only when there is something to put in it. It also retires a contract that lived
in a comment: "add and do not clear", which a rule could break silently.

**Two things I got wrong on the way, both the same wrong.** I proposed a typed field plus
ninety-three `Validate` overrides whose only work was to reach that field — machinery in service of
a mechanism that exists for the *external dictionary* mode, which Igor had just postponed. He
named it: fixing one defect with another. Then I proposed removing the field entirely without
saying that this removes a capability **he** had chosen. **A seam he decided on is not mine to
withdraw as a consequence of an implementation I prefer** — the session working the package caught
that second one before he did.

**What is not decided:** whether the 4,823 checks are written by hand or produced from the
specification, and whether generated code derived from FIX Protocol Ltd's tables carries an
attribution line.

## D131 — Ask the instrument what you already know, before you measure anything with it

Five sessions spent a day on different work and each one, independently, found the same thing:
**an instrument that has never been seen refusing has not been seen at all.** The instances are
worth listing together, because the cure is one sentence and the disguises are all different.

- A guard asserting the schema mode in every benchmark case was seen to **pass**, and that was
  called verification. It is not: a passing case is consistent with the guard working and with the
  guard being absent. Loading a dictionary and watching it refuse cost one call — **less than
  proving the pass**, though the reverse is always assumed.
- A benchmark's case names printed as `arith(...)ters) [26]`: BenchmarkDotNet keeps twenty
  characters of a parameter and elides the middle. Nothing fails; the names simply become
  indistinguishable, and a table read from a real window would have been quoted. The fix — a limit
  asserted in the setup — was then checked by lengthening a name until it fired and putting it back.
- `--affinity 0xFF` is a bad format to BenchmarkDotNet, which prints its help and exits **zero**: a
  run that looks finished and measured nothing. The window script now turns that into "no benchmark
  worker was seen".
- A flag named `validate` on somebody else's library, asked with a message missing a required
  field, **accepts it**. The comparison had rested on the flag's name for a day.
- `dotnet format` has three entry points: `--diagnostics` edits whitespace beyond what it was
  given, `analyzers` does not see these rules and answers "no changes", and only `style` sees them.
  Two plausible false answers in a row, and one of them — "nothing to fix" — would have ended the
  work.

**And the day's own scorekeeper failed the same way.** I stated the window rule to four sessions
without naming the file that holds its state; I passed on a script as usable from another session's
"done", never having run it; I repeated a session's retracted figure because it had been true of a
road nobody took; and I built inside a window twenty seconds after announcing it, because **I knew**
no window was open — I was the one about to open it. Knowing instead of reading, from the author of
the sentence that they are different.

**The rule, and its economics.** Ask the instrument a question whose answer you already know, and
do it **before** it has measured anything: it is free then, and it gets dearer with every figure
taken after. Three of this day's five were caught the other way round — a number first, the
instrument checked afterwards — and each one cost a rerun or a letter of correction.

**The corollary for reading a null result:** an instrument that cannot refuse reports agreement.
So "no difference" from an unexercised check and "no difference" from a working one are the same
sentence, and only the refusing arm tells them apart.

## D132 — A cross-check earns the right to ask, not the right to fix

The FIX package was read against the specification's own machine-readable form, now in the tree.
Seven divergences came out of it. **Four were defects. Two were the package being right and the
specification contradicting itself. One was the checking test.**

**The specification disagrees with itself in exactly two places.** `MiscFeeType` (139) is declared
`char` and its own code set publishes `10`, `11`, `12`; `MassCancelRejectReason` (532) is declared
`char` and publishes `99`. Under the declared type both would refuse values the same document
prints. The package held them wider — and that was a repair, not an error. The session "fixed"
both, then read the code sets and **reverted before committing**. A repair withdrawn before it
lands is worth more than one made correctly first time: the second proves nothing about the
process, the first proves it catches.

**The rule, in that session's words:** *a reconciliation does not give the right to fix; it gives
the right to ask which of the two is wrong — and sometimes the answer is both, and we knew it three
months ago in a comment.* A test had pinned ten divergences against the implementor's dictionary,
with a remark saying that the published specification, which decides them, **had not been read**,
and a guess about `MiscFeeType`. It was read: nine of the ten are the dictionary's, one is ours,
and the guess was right. A debt written down as a debt got paid.

**And the count is a boundary, not an impression:** fields in FIX 4.4 whose declared type does not
hold their own published code set — exactly two, and exactly those two.

**Igor settled the shape as one rule rather than two ad-hoc choices:** where the declared type
demonstrably does not hold the published values, the field is a **string**. Nothing is guessed in
its place; the values are checked by the code set, not by the shape of the type. That moved
`MassCancelRejectReason` from `Int` to `String` — the one change of the five that alters a **type
on the public surface** rather than what validation reports.

**The test's own defect twice, and it was repaired as a test both times.** First it decided "group
or block" from the repository's `ComponentType`, which is unreliable — `Hop` is declared a block
and is structurally a group, and there the package is right; so the shape is decided by structure
and `ComponentType` was demoted to a cross-check. Then it counted the standard header and trailer
as required components of the body, which they are not. **An oracle wrong in one place is neither
discarded nor obeyed: it becomes a second opinion.**

**A mixture is worse than following the wrong source.** The cross-order tables took tag 41 from the
repository and 586 from the dictionary — **no single reading end to end**, so nothing could be
checked against anything. Both now come from the repository.

## D133 — D63 buys time, not memory, and one row closed the cheaper cure

The reader's re-reading was measured on four columns, two shapes and two sizes, at `80ba6733`,
after the instrument had been made to reproduce its own two-month-old answer **to the digit**
across some sixty commits — 995 reader methods, 249 roll-backs, and the re-reading counts 1,723 at
n=16 and 25,195 at n=64, exactly as filed.

```
shape                                 calls x  writes x  deepest x
addresses, unclosed quote (refuses)      14.6       -          -
addresses, valid list (ACCEPTS)           4.1      4.0        4.0
media type (refuses)                     13.5     11.8        3.4
language tag (refuses)                   12.8     12.2        3.5
structured field (refuses)               17.6     17.5        4.0
```

Input ×4: linear is 4.0, triangular 15.3.

**Writes are quadratic and the deepest `Log` is linear**, so retries multiply turnover and cannot
raise the peak. **Therefore the cure buys time, not memory** — and that sentence belongs in the
record *before* anyone measures a cure, because a retained-bytes reading of it would show nothing
and would be right. Had memory been promised, the instrument telling the truth would have looked
broken.

**One row closed the cheaper of the two cures, by measurement rather than by argument.**
`addresses, an unclosed quoted string` writes **no records at all** and has **no depth**, while its
calls climb 1,723 → 25,195 on the triangular number: the disease at full strength with nothing
downstream to remove. Keeping the log's prefix buys exactly nothing there. So the choice is
"remember where the turn ended, or nothing" — and **nothing remains a result**.

**The accepting shape is the control and behaves like one**: linear in all three columns, its
writes equal to its depth to the digit (608/608, 2,432/2,432), so nothing is written twice and an
accepted parse re-reads nothing. D63's condition — that an accepted parse must not pay — is
observed in the same table as the disease rather than argued beside it.

**What the table cannot decide is a ratio of constants, not a degree.** Both the disease and the
proposed cure are quadratic on the same count, so the whole question is what one re-read costs
against one stored position. A cure whose count grows at the rate of the disease is not thereby
refuted — that argument was made and withdrawn here — it is refuted or paid for by the multiplier.

**And the column that looked like an answer was a name.** `begins` counts arms begun on the tape,
not repetitions' turns; it was reported as "the multiplier is already printed", and it was not. The
filed record now warns the next reader against the reading its own author made that morning, and
states in capitals what would silently make the peak column a lower bound: a third place that
advances `LogCount`.

## D134 — Four rules for handing a measurement on, each bought by an error today

All four came out of work done in one afternoon, and three of them were formulated by the sessions
that made the mistake rather than by the one keeping the record. They are about reporting, not
about FIX or SQL, and they are the part of the day most likely to be needed again.

**A ratio without both numbers is not a result.** "The package is three times larger" travelled one
hop and became true of the wrong artifact. There were three ratios, not one: the net10.0 assembly
×3.96, the netstandard2.0 assembly ×2.47, and **the `.nupkg` anyone downloads ×1.65** — the
generated code compresses well, being repetitive by construction. Which of the three a consumer
feels depends on what they are short of: traffic once, or the bytes mapped and read at every
process start. I took the ratio without asking what it was of, and carried it.

**A property named without its axis is a property of the measurement wearing the clothes of a
property of the thing.** "The walk is flat" was true across message *types* and false across the
*number* of types: 6.8 ms for one and 12.2 ms for all ninety-three, because ninety-three messages
reach 345 of its methods instead of 64. What is unbounded in the type count is the generated path,
not the walk being free. Said by the session that had written "flat" an hour earlier.

**Read the zeroes first and the percentages second**, in the words of the session that did not:
*"A table that contains its own refutation does not protect you from it. A zero reads as 'nothing
was measured here' when it is the measurement, and the eye goes instead to whatever number beside
it looks informative — most often a percentage, which describes the remainder. When a column can
hold the answer 'never', a percentage of what did happen cannot tell you that the thing itself
never did."* The `same` column was zero in every row of their own table, printed an hour before
they argued the opposite from the prefix percentage next to it.

**A test has two subjects — the one in its name and the one in its control — and the decision goes
by the name.** When a road is removed, a test that merely *used* the removed helper may still be
about surviving behaviour, and a test whose *name* states the vanished question is finished even
though its control assertions still pass. Here `A_tag_the_mask_does_not_cover_is_looked_for` named
a question that only a loaded dictionary could raise; its two control lines about the mask are
alive and covered 9,320 times elsewhere. My own formulation — "a test survives a behaviour's move
but not a question's disappearance" — was an observation; theirs is a procedure: **look at what
stands in the name.**

**And the deletion is guarded at its reason, not at its consequence.** One assertion replaced the
removed test: the largest tag the repository declares is inside the mask. It is trivially true
today, and its remark says what it is holding up — the day it stops being trivial is the day a
presence question would be asked of a bit that does not exist.

## D135 — A half-applied correction is worse than the defect it corrects

Tag 674 was fixed against the specification in the schema that validation reads and **not** in the
`FixField` class that parsing builds. For some hours the package therefore **accepted a non-numeric
value as permitted and returned a field whose `IsValid` was false** — two answers about one value.

**Before the fix it was wrong and consistent**; after it, right in half and inconsistent. The defect
was visible to anyone holding the package against the specification. The disagreement was visible
to **nobody**, because each half agrees with itself and no test brought them together. So: **a
change that touches two halves which each agree with themselves must be atomic, or there must
exist a place where the halves meet.** Here that place was the fixture grammar, which names a
converter per tag and so forces the compiler to reconcile them.

**And the solution did not build for hours while every suite was green.** `DotGram.Finance.Fix44`
is a separate project *on purpose* — D12 keeps it out so the ordinary tests build in seconds — so
"the folder is green" and "the solution builds" were different statements that looked the same.
**The arrangement that makes the run fast is what hid the break.** The repair was not to undo D12
but to give the fast suite **a cheap way to ask the same question**: a test that reads the fixture
grammar as *text* in milliseconds and asks, for all 880 rules by reflection, whether what
`FixConvert` returns fits what the `FixField` class takes. It was pointed at the defect and made to
name it before it was believed.

**A test written before a rewrite pins what the old code promised; one written after pins what the
new code does.** Almost always the second is written and called the first. Three refusal texts and
the whole behaviour of an unknown message type were pinned *before* the assembler work was
authorised, for that reason.

**A witness that refuses to fail is worth more than one that fails as expected.** The first attempt
to make a field "not permitted" — a repeated header tag at the end of the body — simply parsed:
the body claims every tag nobody else lists, so a field can be surplus **only after the trailer has
begun**. That rule is written in no comment, test or page; it follows from which scopes are
declared `body`, and it is **ours**, not the specification's. It is now written beside the witness
that found it.

**And a change was cancelled by arithmetic that could have gone either way.** Splitting the field
factory on 100 instead of 64 was argued from readability and from whether C# would still compact
the jump tables. The densities — 0.75–1.00 at 64, 0.80–1.00 at 100, tail 0.57, against a threshold
near a half — say a jump table is emitted either way, so that argument supported neither side; and
the case labels inside each part are the real tag numbers, so the readability it promised was
already there. **1,824 cases were not regrouped for nothing, because the arithmetic was done
before the regrouping and not after.**

## D136 — FIX: the schema is a passed context, not a static field (Igor)

D130 put a message type's validator in a static field on the class, and named the two consequences
in the release notes: one configuration a process, and two counterparties with two schemas at once
is not expressible. Igor is replacing that with a **context**, passed to the calls that need it.

**The context is the grown `FixFieldOptions`, not a second object.** It is already immutable,
already handed to every entry point, and already carries two of the three things a schema is: the
custom field factory and the length/data pairs. It gains the message factory and the validators.
A `readonly record` class, changed with `with`, which retires the hand-written `With(FixFraming)`.
The name goes with it: "Options" stops being true of an object that IS the counterparty's schema.

**Virtual methods where there is one, a table where there are ninety-three.** The three hooks —
a tag outside the 912, a field the message does not place, a `MsgType` outside the 93 — are virtual
methods on the context, overridden by deriving. The validators are a table, because what a loaded
dictionary replaces is the check of *one message type*, and a virtual method would make the
consumer write the ninety-three-armed switch we already have. One default table instance, held by
reference, so `with` swaps a pointer and no 744 bytes of delegates are copied. Igor: without
`Load` a single virtual method would have done — the table exists for the dictionary and for
nothing else.

**`message.Validate(context)`, returning a flag.** No parameterless overload. Findings are
appended to the message, not returned, because the constructor already writes findings there and a
second list would disagree with the first. `IsValid` is public; `_isValidated` is private and
exists only so a second call does not fill the findings twice. **One run, one context:** Igor —
a different context is a different pass over the input, and there is no scenario that validates one
message by two schemas. So a second `Validate` doing nothing is a rule, not a defect.

**The streaming step yields messages; it does not copy findings.** `Parse … Validate …` over a
sequence is one lazy operator that hands on the messages that have findings. Copying the findings
into a shared list would strip the owner — a `FixFinding` is about one message and holds no
identity of it — and the message is what the consumer would have to re-attach.

**A finding is five fields.** `FixRule Rule`, `int Tag`, `int Position`, `FixField? Field`,
`int EntryIndex`.
What was dropped and why, all four bought by Igor reading the type and asking what each field was
for:

- `Position` — kept, and I twice got it wrong on the way. I dropped it because `FixField` has
  carried `Position`, `Length` and `ValuePosition` since it was written, so a finding holding the
  field holds them. Igor: a nullable `Field` then makes the position cost a null check. I offered
  a computed member and argued a stored one would have nothing to put in it where `Field` is null.
  **That was wrong.** It reasons from "the field is not there" to "the place is not there", and
  the place is known: a required field absent from its scope is reported at the field it was
  expected after or before, and the validator walking the message has that field in its hand.
  So `Position` is stored and always answerable, like `Tag`.
- `GroupTag` — derivable. We established by exhaustive check that **no tag belongs to two scopes
  within any of the ninety-three messages**, so the tag names the group. `EntryIndex` does not
  follow from anything and stays.
- `Reason` — composed in `ToString()` from the rule, the tag and the entry. As a stored string it
  was an allocation per finding, paid whether anybody read it or not.
- `Scope` — Header/Body/Trailer was a property of the three-scope node model. The new `FixMessage`
  is one flat `Fields`.

`Rule` survives because a session that turns a message away sends `Reject` with
`SessionRejectReason` (373) and `RefTagID` (371): the kind of the fault and the tag it is about,
machine-readable. Neither can be recovered from prose. `Tag` survives beside `Field` for the same
reason, and because of the one case where there is no field at all: **`Field` is null exactly when
the finding is about something that is not there** — `RequiredFieldMissing`,
`RequiredComponentMissing`, `MessageEncodingMissing`, three of the eleven rules.

**A tag read but not recognised becomes `FixField.Invalid` carrying its tag.** Igor's distinction:
`Invalid` without a tag is input that did not parse, `Invalid` with one is input that parsed into
something we cannot name. The octets survive in `RawText`/`RawBytes`, so a tag a counterparty
explains six months later is still recoverable from a parsed log. Its diagnostic string has to be
a literal, or a venue with its own tags pays a string per field.

**What QuickFIX/n does, measured rather than remembered** (1.14.1, twelve inputs through their own
`FIX44.xml`; the probe was thrown away):

| input | `FromString(validate: true)` | then `DataDictionary.Validate` |
| --- | --- | --- |
| bad CheckSum, bad BodyLength, no 35 | refused | — |
| tag not in this message type | accepted | refused |
| undefined tag | accepted | refused |
| required tag missing | accepted | refused |
| value outside its code set | accepted | refused |
| group count disagrees with the entries | accepted | refused |
| a header field after the body | accepted | refused |

Their `validate` flag is **framing only**; every schema check lives in the separate static
`Validate(message, transport, app, beginString, msgType)`, which returns `void` and throws on the
first fault. Two readings for us: the split we have is the split they have, so **no parse-time
validation flag is needed** — their flag covers what we refuse unconditionally anyway — and the
list of findings against their one exception is where we are better, which is worth its cost.

**A finding cannot locate its message in a log, and nothing here changes that.** Positions are
within a message, because streaming parses each from its own frame, and no message carries the
offset it was read from. That is a question for the message, not for the finding, and it is not
answered here.

**Four refusals became readings, and the pinned contract says so.** `FixBuilderContractTests` was
written before the rewrite to record what the reader promised, word for word, because those
sentences reach a consumer in a `FormatException`. Four changed: a NumInGroup that is not a number,
a NumInGroup larger than the fields that follow it, a group entry that does not open with its
delimiter, and a field no scope would take. All four were refusals of the reading and are now read
without complaint. The cause is single: a message was a scope of nodes held against the schema
while the input was still being read, and is now its typed fields built by a switch over the tags,
which has nothing to disagree with. Each is a rule of `FixRule` and returns with validation — which
is what makes finishing validation a correctness matter and not only a feature.

**What is not decided:** how the factories are arranged, and therefore whether `FixField.Custom`
survives at all. As Igor describes them — the consumer's factory accepts and builds, or the base
builds `Invalid` — nothing is left to build a `Custom`, but that is a consequence of the factory
design and is not settled ahead of it.

## D137 — An exception the host throws is not caught, not even by a `Try` method (Igor)

Raised by expr, 2026-09-24, while EL's refusals were being examined: the generated `TryParseX`
lets an exception from host code escape — a `=>` factory, a `when` guard, an external recognizer.
§7.5 said an outcome is a value and exceptions appear only in the methods without `Try`; §7.4 says
C# stays C#. Read strictly, the first made the generated `Try` methods wrong.

**Igor: the host's exception is not caught.** A `Try` method that caught it would turn a defect in
the consumer's code — a `NullReferenceException` in somebody's factory — into "the input did not
match", and the defect would be lost where it is cheapest to find. §7.5 now says so: what host code
throws propagates from every publication; a refusal of the grammar is still a value.

**The condition that comes with it.** The parser must never let escape an exception from a reading
it has abandoned: the host is asked about what the parse reads, and a reading given up cannot fail
it. Whether that holds today is not established. performance-9f counted 117 speculative throws from
EL's semantic factories on the immediate carrier before expr's 5bfa7b7b, caught by the internal
`TryParseLambda`'s harness; they stopped with that commit for a reason not yet known. expr is
looking for a witness — a valid text whose abandoned reading throws through `Parse` — and it would
be a correctness defect with its own priority.

**Not changed:** EL's public `TryParse` already refuses a text that does not compile, with the
exception's own message. That is EL's catching of its own semantic errors, not the generator's of
a host's, and it stays.

## D138 — Forcing the immediate carrier over an unsafe grammar is a warning (Igor)

D137's condition — no exception escapes from a reading the parser abandons — is kept by the
generator only where it chooses the carrier: `Auto` keeps a grammar on the tape when a building
rule may be read where the reading may not stand (`replay`). performance-9f, 2026-09-24: an
author's `[GramOptions(Carrier = GramCarrier.Immediate)]` is not checked at all, the one
`Commit`-based refusal applying only to grammars that use `recover`. EL's
`ExpressionParser.Immediate` is such an override over a grammar with 18 of 844 building sites
settled; it is safe because its grammar puts a `when` in front of every construction that could
throw (expr: 10,722 texts, both carriers, no divergence, no witness), not because anything checks.

**Igor: a warning.** Where the author forces the immediate carrier and the generator would have
kept the grammar on the tape, the generator warns and says why; the author suppresses it where the
grammar is guarded, with the reason. §7.5 says the condition holds for the generator's choice and
passes to the author with the override. Not an error: `ExpressionParser.Immediate` keeps building.
performance-9f owns the diagnostic; EL suppresses it with a reference to its guards and to expr's
standing two-carrier comparison.

Done at b2a30937 as GRAM5015 (docs/diagnostics.md). A grammar in a string literal is suppressed
with `#pragma` beside it; a grammar in a `.gram` file is reported in that file, which no pragma
reaches, so there the suppression is `NoWarn` in the host's project, as for GRAM5003. Reporting
it at the host instead would change `GramDiagnostic`'s contract and was not done.

## D139 — FIX: a field is a class of the type of its value, and its tag says which (Igor)

Until 2026-09-24 the FIX package had a class per tag, 912 of them (`FixField.OrderQty`), a
`FixCustomField<T>` for tags outside FIX 4.4, and a `FixFieldFactory` delegate on the context —
which `Load` also compiled from a dictionary through the expression language. Igor: a field is a
base class and the classes of its value's type, nothing else; no factory; compatible with FIX 5.

- **The classes.** `FixField` (tag, extent, `IsValid`), `Typed<T>`, and eleven sealed classes of
  the value types — `Text`, `Character`, `Boolean`, `Integer`, `Decimal`, `Timestamp`, `Time`,
  `Date`, `MonthYear`, `Multiple`, `Data` — plus `Invalid`. No finer FIX type is stored on a field.
  Sealed because the parser builds only these and, with no factory, nothing it reads can be a
  derived class (opened at 2858c557 and sealed again at eebd2ab2 for that reason).
- **The tag is its number, an `int`,** and `FixTag` holds the standard's tags as constants named
  for them, as QuickFIX/n's `Tags` does. Matched as `FixField.Decimal { Tag: FixTag.OrderQty }`; a
  tag outside the standard is `25005`, with no cast. (It was an enum from 582a63c5 until Igor,
  2026-09-24: the number is what logs, the wire and a consumer's own tags are written in, and an
  enum asked for a cast at every one of them. The constants keep every pattern and `case`.)
- **One table a context,** a byte a tag: the value type and the tag's half of a length/data pair.
  The version's is the default (`Fix/Fix44/FixStandard.cs`, written by `generate.py`);
  `LengthDataPairs` sets the pair bits, `Load` sets the types of the tags a dictionary adds. A
  dictionary never retypes a standard tag — a message property's cast would fail. A pair's tag
  nothing types is read as its half: an `Integer` length, a `Data` data. A tag nothing defines is
  an `Invalid`. The builder is a switch of twelve arms over the type.
- **Messages, components and checks** are typed by the value class; names are unchanged. Field
  check slots stay one per tag, typed by the value class (`Func<…, FixField.Decimal, bool>`).
- **What went:** a consumer's own class for a field, and a check of a non-standard field's value
  in a slot; the second is written in `FixCustomMessage.OnValidate`.

- **What every version shares, and what is FIX 4.4's own** (Igor, same day). Tag numbers are
  shared by every version — 6,066 across 4.0 to 5.0 SP2, none reassigned, 41 renamed, 119 read
  into a different class in some version — so the field model, `FixTag`, `FixConvert`, the
  findings, the field grammar, the dictionary reader and the context's table are one layer,
  `DotGram.Finance.Fix` in `Fix/`; the parser, the messages and the checks are the version's,
  `DotGram.Finance.Fix.Fix44` in `Fix/Fix44/`. The messages are not shared: each version keeps
  its own property names.
  - `FixContext` is an abstract record: framing, buffer and retention bounds, pairs and the table.
    A version hands it its pairs and types (`FixVersion`) and its checks. `Fix44Context :
    FixContext` adds `Default`, `WithLogFraming`, `FixMessageFactory` and the public `Load` and
    `LoadFile`, which answer a `Fix44Context`. One object for the consumer, as before.
  - The load is shared (`FixValidatorBase`, which `FixValidators` derives from): it writes a check
    from the dictionary's names, and the version supplies only the namespaces the text opens with
    and its context's name. A tag in that text is its number, since a version's dictionary may
    spell a field otherwise than `FixTag`. A finding is said to `IFixFindings`, which `FixMessage`
    implements, so the shared helpers name no version's message.
  - `FixTag` today holds FIX 4.4's 912; the union of every version, named as 5.0 SP2 names them
    (`IOIID`, `NoLinesOfText`), is the next step.

`582a63c5`; the split `cd989ade`, the tag as a number `9fbba0aa`, `FixValidatorBase` `778c75f2`.
Consequence for EL: the tuple, the target-typed switch and the untyped lambda built
that week for the factory lost their only consumer; Igor keeps them as C# parity (232 KB, +13.5%
over 5bfa7b7b~1, measured in one worktree after 3a8d5bcd).
