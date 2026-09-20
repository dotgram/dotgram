# A carrier per construction, not per machine: a design (for review)

Today `Auto` picks one carrier for a whole machine. The immediate carrier builds every
construction at the end of its rule, from registers, and writes nothing to the log. The tape
records every construction and builds it after the parse. So one construction without a proved
point (`Commit`) is enough to keep all of a machine's constructions on the tape. So is one rule
the reader can ask again. That is why T-SQL, with a point at 75% of its sites, is carried on the
tape as a whole. This asks whether one machine can build part of its constructions in place and
leave the rest to the tape. No code is proposed until the answer below and its number are
agreed.

## 1. Can one machine carry both

Yes. Most of what is needed exists already.

- **The reader does not depend on the carrier.** Ways, the tape's cursor, `Retry` and `Seal` are
  the same under both carriers. What differs is only where a value waits: in a register and a
  gathered stack (immediate), or as a record in the log and a slot in `DirectValues` (tape).
- **The tape already builds during the parse.** A guard that names a built value calls the
  materializer on one record, from the rule's mark, and marks it built (`Built`, the watermark
  on `ways.Built`). The walk after the parse then skips what is built. So "build this subtree
  now, and let the walk find it built" is a path the tape already takes. Since 584a7c1f and
  312d5b98 it is linear in what it builds.

A construction is then *immediate* or *taped*, decided per construction by the proof. There are
four edges between a parent and a child:

| Parent | Child | What passes between them |
| --- | --- | --- |
| immediate | immediate | a register, as today |
| taped | taped | a record, as today |
| taped | immediate | the child's value, put in a *held* table; the parent's record holds its index under a leaf kind, which the walk reads instead of building |
| immediate | taped | at the parent's point, the child's record is materialized from the parent's mark (the guard's path), then read from its table |

What may be immediate is what the proof allows (`Commit`, fix-reader §7). This design starts
with the narrowest safe set: a **settled subtree**. That is a rule whose every construction has
a point at the rule's end (`Kind.Rule`), and whose every building call reaches a rule of the same
kind. Such a rule never needs the fourth edge, and nothing under it is taped. Wider sets, with
`Turn`, `Atomic` and `Caller` points or taped children, come later and pay for the fourth edge.

## 2. What they share

- **Stores.** The reader holds both the immediate registers and stacks and the tape's log and
  `DirectValues`. The only new store is the held table: one array per value type, appended at
  the point, with a count that the record marks carry beside `LogCount`.
- **The log.** An immediate subtree writes no records. Its root writes a single leaf record into
  a taped parent's record, or nothing at all under an immediate parent. The log then grows only
  with the taped constructions.
- **Ways and unwinding.** A point is where a reading can no longer be taken back, short of the
  parse failing. By `Commit`'s definition, a point exists only where the chain above it holds.
  A held value is therefore never unwound by a way, only by a failed parse, which drops
  everything anyway. The held count is still marked and unwound with the records, since that is
  what the mark already covers. That costs nothing and keeps the store honest if a later, wider
  set weakens the invariant.
- **Refusals by the immediate carrier.** An extent collected across turns, and a repetition
  marked `recover` whose element is not settled at its rule's end: a construction that is
  refused falls back to the tape, not the machine.

## 3. What it would give

These are static counts. The file's point column counts calls too, so the constructions were
counted separately by a local probe over `Commit` and `Demand`, which was not committed.

**The reach here is counted the loose way.** "In settled subtrees" was taken before the fixpoint of
§4a was written down, so it does not require that every valued call a rule makes be settled as well.
Counted with it, T-SQL has 115 such rules of the 651 that write a record, not 195 of 654 (§4b). Every
figure in this table is therefore a ceiling and not an estimate, which is how it should be quoted —
and it is still the right figure to quote for SQL:2023, whose dynamic zero was taken over a corpus
of another grammar. Recounting the table under the fixpoint changes no decision while the shelving
rests on the dynamic count; it is worth an hour when this design is picked up again.

| Grammar | Constructions | With a point | At its rule's end | In settled subtrees | Rules in settled subtrees |
| --- | ---: | ---: | ---: | ---: | ---: |
| T-SQL | 1,509 | 1,146 (76%) | 867 (57%) | 399 (26%) | 195 of 654 |
| SQL:2023 | 1,164 | 571 (49%) | 526 (45%) | 83 (7%) | 35 of 555 |
| SQL-92 | 118 | 5 | 5 | 0 | 0 of 47 |
| Expression language | 356 | 6 | 6 | 0 | 0 of 106 |
| the web's grammars on the tape | all | all | all | all | all |

Three things follow from the table:

- **The expression language gains nothing.** Its constructions have no points: 20 causes of
  their own under `Replay` hold them, and those are the grammar's and the analysis's to answer,
  not the carrier's.
- **The web's grammars on the tape gain nothing either.** Every construction there is settled.
  What keeps them on the tape is the reader's gate, a rule read again, and not a missing point.
- **The SQL grammars are what this is for.** T-SQL has a quarter of its constructions in settled
  subtrees today, and 57% at their rule's end if the fourth edge is paid for.

The number that decides is dynamic, not static: which constructions a real parse writes. Leaves
are where to look, since identifiers, literals and column references are written far more often
than statements, and they are the likeliest to be settled. What building costs is known.
Materialization was 12.0 of 26.0 ms own time per round of T-SQL over the corpus (the Q4.2
profile, 46%). Built in place instead of taped, a construction cost 48% less in FIX (95.6 → 49.5
ns a field) and 30% less in the expression language (1,000 terms, 275 → 192 µs). If half of
T-SQL's written records were in settled subtrees, the gain would be on the order of 0.46 × 0.5 ×
0.3–0.5, that is 7–11% of a T-SQL parse. That is an estimate to replace with a measurement
before any code (§5, step 1).

## 4. What it costs

- **Code size.** A settled rule gets the immediate carrier's building code: a register write at
  its end, and a leaf write where a taped parent reads it. Its arm leaves the tape's materializer
  unless a guard still asks for it there. The materializer shrinks by the settled rules' arms and
  the reader grows by their builders. For T-SQL, 195 of 654 rules: likely a wash; to be measured
  on the generated file.
- **Two carriers in one file.** `ImmediateValues` and `DirectValues` are both emitted, and the
  reader's state carries both. Each already exists alone. What is new is the held table, the leaf
  arm in the walk, and the choice itself, which is made per rule rather than per machine.
- **The first call.** Two stores to JIT, and larger reader methods for the settled rules. The
  first-call rows of the stand say how much; they have caught every change of this size so far.
- **Analysis.** The settled-subtree set is a fixpoint over `Commit`'s sites. That is one walk,
  and the report already computes it for the file's point column.
- **Proof and tests.** Correctness rests on `Commit`, as C4 does. The gates are C4's: carrier
  agreement against the tape on shapes that mix both edges, factory call counts against the tape
  (CarrierDemandTests), and the corpora.

## 4a. Step 1's answer: the dynamic share

**How to take the count again.** Nothing of this is committed, and the numbers below are only
worth what the recipe is, so here it is whole. Four things, an hour at most.

1. **Which rules are in a settled subtree.** In the generator, from
   `Commit.Of(graph, Replay.Of(graph))`: a rule qualifies when every construction of it has a
   point of `Commit.Kind.Rule`, and every call it makes that builds something names a rule that
   qualifies. That is a fixpoint over the call graph, seeded with the rules whose constructions
   all have `Kind.Rule` points and that build through no call. Write the set out of the generator
   as a list of rule names beside the generated file — the report file (`*.DotGramReport.g.cs`)
   is where such a list belongs.
2. **A counter an arm.** `MaterializeDirectArm` (`Machine.Direct.Values.cs`) writes one `case` a
   rule of the walk. Emit a line at the top of each — `Probe.Built[<n>]++;`, `<n>` the arm's
   number — into a static array of a class added to `DotGram.Sql` for the run. Both callers of
   the walk are then counted: the walk after the parse and the walk a guard asks for, since both
   reach the same arms. Build `DotGram.Sql` alone, with `-nodeReuse:false -p:UseSharedCompilation=false`.
3. **The corpus, cut as the stand cuts it.** As `ScriptDomBenchmarks.Setup` does it: every `*.sql`
   of `tests/Corpus`, parsed by ScriptDom with the version its file names, each statement taken
   out by `statement.StartOffset` and `statement.FragmentLength` and trimmed at the end, and kept
   only where `TransactSqlParser.TryParseStatement` reads it. That is 7,716 statements. For
   SQL:2023 the same statements are offered to its query, insert, update, delete and merge
   entries, and 334 of them read: a thin corpus, and its counts include what a guard built inside
   an entry that then refused.
4. **The share.** Sum the counters of the arms whose rules are in the set of (1), over the sum of
   all of them.


Step 1 below has been taken, with a local build of DotGram.Sql and nothing committed. Each arm of
the materializer counted the records it built, whether in the walk after the parse or for a
guard during it. The corpus was the 7,716 ScriptDom statements the stand times T-SQL over, cut
exactly as `ScriptDomBenchmarks` cuts them. SQL:2023 was put to the same statements through
its query, insert, update, delete and merge entries. It read 334 of them, which is a thin
corpus, and its counts include the guards' builds in the entries that were then refused.

| Grammar | Records built | In settled subtrees | In rules with every construction at its end |
| --- | ---: | ---: | ---: |
| T-SQL | 116,946 | 5,607 (4.8%) | 29,125 (24.9%) |
| SQL:2023 | 97,338 | 0 | 0 |

The static count said a quarter of T-SQL's constructions are in settled subtrees. The parse says
one record in twenty. The records a parse writes are the value tower's: `TSqlValueExpression`
(11,631), `TSqlValuePrimary` (11,154), `TSqlPrimaryCore` (5,990) and `ColumnReference` (3,805)
in T-SQL, and the towers' operands and predicates in SQL:2023. None of those is settled. What
holds them is Replay's causes, the first being `TSqlPrimaryCore`'s choice of a bracketed value
against a subquery. The settled subtrees are the rarely written corners of the grammar.

**The count above is unweighted, and the weighting it replaced was not a detail** (the critic's
Q1, `docs/design/open-questions.md`). It was argued that the time share is the count share, because
the factories are 1.2 of 12.0 ms of materialization and what is left is the walk's per-record work.
The anatomy taken hours later says the greater part of that is not per-record at all: the arms'
prologues are 46% of one SQL:2023 parse and a guard's walk 30%, both fixed per call, and both differ
by which part method an arm sits in and by whether a guard asked for the walk. Settled subtrees are
the rarely written corners, where a per-call price weighs most, so the error does not cancel.

So the numbers below are provisional, and the arithmetic is written out rather than rounded:
0.46 × 0.048 × 0.3–0.5 is **0.7–1.2% of a T-SQL parse** for step 2, and 0.46 × 0.249 × 0.3–0.5 is
**3.4–5.7%** for the fourth edge — which is not the same kind of number, and is the better bet of
the two. For SQL:2023 the evidence is the static ceiling of §3 — 83 constructions, 35 of 555 rules —
and not the dynamic zero, which was taken over the 334 T-SQL statements that happen to read as
standard SQL.

**When to take it again, and how.** After the materializer's prologue is split (expr's `fa47097e`
and what follows it), since that changes the cost model this rests on. Weighted: each arm's count
times that arm's own cost, taken once from the frame its method zeroes and the setup its walk pays,
rather than a share assumed uniform. **What would unshelve step 2:** a weighted share of what
settled subtrees build, times the 0.3–0.5 a construction saves built in place, above about 3% of a
T-SQL parse.

Until then the lever remains the one AtomicBody pulled: take away the Replay causes that hold the
value tower. Of the three heaviest, `TSqlPrimaryCore`'s choice (40 rules) is the language's,
`JoinedRight`'s (29) is the analysis's — a call needs no way back where every shortening of it
resumes on a token the continuation cannot take — and `InlineReturn`'s (26) would cost
over-acceptance, which Igor has declined: accuracy to the published syntax and to the server comes
before speed, so those rules stay on the tape. Each cause that goes makes the hot rules settled, whatever
carrier reads them.

## 4b. The weighted retake (2026-09-19, after the materializer's three commits)

Taken on main with `fa47097e`, `75ed392e` and `b4ad9f4c` in it, by the recipe of §4a: a counter in
each arm's body, a counter at each walk, the corpus cut as `ScriptDomBenchmarks` cuts it. One round
of the corpus, 7,716 statements:

| | |
| --- | ---: |
| records built | 116,946 |
| walks of the log | 17,023 (2.2 a statement) |
| records in settled subtrees | 4,130 (3.5%) |
| walks whose every record is in a settled subtree | 174 (1.0%), holding 197 records (0.17%) |

**The last row is the weighting.** Materialization is paid twice over: per record, in the arm that
builds it, and per walk, in the setup each one pays. Which of the two weighs more depends on the
build: in the anatomy of the morning of 2026-09-19, before the arms' prologue was taken out, the
record carried the larger part — 46% of a SQL:2023 parse against the walk's 30% — and in the anatomy
of that evening, with the prologue gone, a record costs 6.0 ns and a walk 27.8 (below). Both
readings are of this document's subject, and neither is true of the other's build. Step 2 takes the record's cost off 3.5% of the records. It takes a
walk's setup off almost nothing: a walk that keeps one taped record keeps all of its setup, and only
0.17% of the records sit in walks that would go entirely. So the weighted share is a shade below the
unweighted 3.5%, not above it, and the unweighted 0.7–1.2% of a parse is an upper bound rather than
an estimate.

**The two costs, measured.** From the stand's profile of `sql-loop` on main after the materializer's
three commits (benchmarks/results/sql-anatomy-2026-09-19b), scaled to one parse of select20 by that
loop's wall of 17,862 ns: **27.8 ns a walk** and **6.0 ns a record**. Both are SQL:2023's, taken over
that grammar's arms on that statement, where a walk builds 1.3 records; T-SQL's walks carry 6.9, so
these travel as an estimate and not as a measurement of T-SQL.

Carried to T-SQL's own counts, materialization over a corpus round is 116,946 × 6.0 + 17,023 × 27.8
= about 1.18 ms, of which the records are 60% and the walks 40%. What step 2 could take off it is
4,130 × 6.0 + 174 × 27.8 = 29.6 µs, **2.5% of materialization**; at the 0.3–0.5 a construction saves
built in place, **0.8–1.3% of what materializing costs**. As a share of a whole T-SQL parse that is
under half a per cent, reached through the Q4.2 profile's 46% — which was measured before the three
commits that took the prologue out, so materialization is a smaller part of a parse now and the
figure is if anything smaller than this.

**The shelving is final under this cost model, and the lever has moved.** Step 2 was worth one to
two per cent when a record carried the arms' prologue; it is worth less now that the prologue is
gone, because what is left is mostly the walk, and the walk is what step 2 does not remove. What
does remove walks is a guard that needs no walk to see a built value — the fourth edge of §1, or the
guard's own path through the materializer. On select20 that is 8.4 µs of 17.9, the largest single
item in the parse.

**The rule sets differ too, and by the same tightening.** The counting build marks 115 of the 651
rules that write a record as settled. §3's static table, taken before the fixpoint was written down,
says 195 of 654. Nothing else about the two counts differs enough to explain 5,607 against 4,130, so
the definition is where it lies: a rule is settled today only where every valued call it makes is
settled too, and that takes 80 rules out.

**Two numbers, and why they differ.** §4a's count of records in settled subtrees was 5,607 (4.8%);
today's is 4,130 (3.5%). The counting build behind the first was never committed, so the two cannot
be compared line by line. The cause is the definition, as the rule sets above show: today a rule is settled
only where every valued call it makes is settled too, which is the fixpoint §4a's recipe states. Both numbers
stand here rather than one replacing the other, because a number that moved when its assumption was
written down is exactly what the earlier reading of this document got wrong.

For the shape of a parse: 1,160 walks build twenty records or more and hold 38,721 of the records,
a third of them. Those are the walks at the end of a reading; the rest are small, one to five
records, and they are what the guards cost.

## 5. Proposed steps

1. **Measure before building.** Using the tape, over the T-SQL and SQL:2023 corpora, count the
   records written per rule and split them into settled subtrees and the rest. This needs a
   counting build only, not a generator change. The share decides whether step 2 is worth it.
2. **Settled subtrees immediate in a taped machine:** the held table, the leaf record, and a
   choice per rule. A pair on sql/\* and tsql/\*, the gate, and a code-size comparison.
3. **Only if step 2 pays:** points at the rule's end with taped children (the fourth edge, through
   the guard's materializer), then `Turn` and `Caller` points.

What is proved stays where it is: nothing is built ahead of `Commit`'s point. What is chosen here
is which of two correct renderings each construction gets, and that is measured (fix-reader §7).
