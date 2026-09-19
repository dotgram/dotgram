# Open questions: what the critic has objected to

D21. Each entry is one objection: the date it was raised, the claim it argues with, what the
objection rests on, what would settle it, and the answer it was given. An objection is closed by
an answer, not by silence — an entry with no answer is one nobody has replied to yet.

Nothing here is measured by this session. Every number below is read out of the repository, and
the place it is read from is named so that it can be checked.

## Q1 (2026-09-19). Shelving the carrier per construction rests on a cost model the next measurement contradicted

**The claim.** "The time share is the count share … that puts step 2 at about 0.46 × 0.05 ×
0.3–0.5, one to two per cent of a T-SQL parse. Neither is worth a second carrier in a machine now."
([`carrier-per-construction-2026-09-19.md`](carrier-per-construction-2026-09-19.md) §4a; the
journal, "Step 1 measured, and it shelves step 2".)

**What it rests on.** A count of 116,946 records written over the T-SQL corpus, of which 5,607
(4.8%) are in settled subtrees and 29,125 (24.9%) in rules with every construction at the rule's
end. The count is unweighted. The step was set as "a count of records per rule over the corpora
**weighted by their materialization cost**"; the weighting was replaced by an argument — that the
factories are 1.2 of 12.0 ms of materialization, so what a record costs is "the walk's per-record
work" and is therefore the same for every record.

**The objection.** The anatomy taken by the same session a few hours later says the opposite (the
journal, "The anatomy (sql-39, next.md `96ba42ef`)"). Of 71 µs for twenty SQL:2023 selects, 54 are
the shape of the materializer's code: the arms' prologues 46% — 402 records at about 85 ns each,
the part methods zeroing 10–13 KB of frame on every call — and the walk each guard runs 30%, 301
walks for 402 records, each paying a fixed setup to build usually one record. So about three
quarters of materialization is a per-call fixed cost that varies with which part method an arm sits
in and with whether a record is built inside a guard's walk. That is exactly the cost the
count-share assumption declares uniform, and the factory argument does not touch it: the factories
are the one part of a record's cost already agreed to be small.

The settled subtrees are, in the document's own words, "the rarely written corners of the grammar".
Corners are where the per-call share is highest and the per-record share lowest, so the direction of
the error is not neutral. Three smaller things in the same paragraph:

- 0.46 × 0.048 × 0.3–0.5 is 0.7–1.2%, and is reported as "one to two per cent".
- The fourth edge is dismissed in the same breath, at "about 5%" — 0.46 × 0.249 × 0.3–0.5 is
  3.4–5.7% by the same arithmetic, which is not the same kind of number as 1%.
- SQL:2023's zero is taken over 334 T-SQL statements that happen to read. Its static ceiling (83
  constructions, 35 of 555 rules, §3) is the stronger evidence for "SQL:2023 gains nothing" and
  should be what is quoted; the dynamic zero quotes a corpus that is not that grammar's.

**What would settle it, cheaply.** The probe of §4a already counts per arm. Weight each arm's count
by its part method's frame, or by a per-arm cost taken once, and derive the share again. And the
prologue fix now decided — parts split by frame, or an arm a method — changes the very cost model
this shelving rests on: after it lands the count has to be taken again, whichever way the number
then moves. The document says "shelved with its number"; it should also say which measurement
would unshelve it.

**Answer:** —

## Q2 (2026-09-19). Of T-SQL's three heaviest causes, one has a witness and two have prose

**The claim.** "So the three heaviest causes in T-SQL's value tower are ambiguities of unbounded
depth; the grammar question is closed." (The journal, "Step 1 measured, and it shelves step 2".)

**Cause 1, `TSqlPrimaryCore`'s bracketed value against a subquery, stands.** `((SELECT 1) + 1)`
against `((SELECT 1) UNION (SELECT 2))`, and a query expression may be bracketed to any depth, so
the deciding token really is at an unbounded distance. No objection.

**Cause 2, `JoinedRight`'s turn (29 rules), is given no witness.** `TransactSql.gram:1069`–1085.
Reading the rule, every step of `tails: JoinedTail*` is decided by the next token. A tail begins
with a join type, a hint, `JOIN`, `CROSS` or `OUTER`; the star's only competitor at that point is an
`ON` belonging to an enclosing level, which is a different token. Each qualified tail takes its own
`ON` immediately after its nested `JoinedRight`, whose star also stops at `ON`, so the `ON`s bind
innermost-first the way brackets do, and greedy reading produces that binding without giving
anything back: `t1 JOIN t2 JOIN t3 JOIN t4 ON a ON b ON c` and `t1 JOIN t2 CROSS JOIN t3 ON a` both
read left to right with nothing taken back. What is left is that a tail which has consumed its
`JOIN` may fail later — on input that fails altogether — and the analysis cannot rule that out
without a commit on the tail's first token. That is a property of the analysis, and it is the same
possessiveness "against the union of the tails' first sets" already named as condition (ii) of the
fold. It is not the language.

Asked: one input where the count of `ON`s forces a turn to be given back. If there is none, the
cause is reclassified from intended to the analysis's, and its 29 rules are back in play.

**Cause 3, `InlineReturn`'s query (26 rules), is the shape of our own rule.**
`TransactSql.gram:4652`. The two alternatives are `'(' with? q by? window? ')'` and
`with? q by? window?`. The give-back is real — on `RETURN (SELECT 1) UNION ALL SELECT 2` the first
alternative reads `(SELECT 1)` whole and the failure appears only at the caller — but the two
alternatives differ solely in what the brackets may hold: the second one's `QueryExpression`
already reads `( q )`, so the first earns its keep only for a `WITH`, an `ORDER BY` or an `OFFSET`
*inside* the brackets. The cause is therefore removable, at a price that should be said out loud
rather than folded into "intended": taking the extras into the bracketed query would accept a
`WITH` or an order inside brackets wherever a parenthesized query stands, which is over-acceptance
— "the half of correctness a refusal count cannot see", in the grammar's own words fifteen rules
above the join. That is a trade-off for the grammar's owner and for Igor to take, not a fact about
T-SQL.

**Answer:** —

## Q3 (2026-09-19). "FIX is faster than the hand parser" is one input form of three, and names the weaker of the two yardsticks we have

**The claim.** "FIX text: the generated parser went from 1.72x of the hand parser to 0.76x … it is
faster than the hand parser on strings now." (`e56f4650`;
[`stand-2026-09-19b.md`](stand-2026-09-19b.md).)

**Is the same work done by both sides?** On shape, yes: `HandFixBenchmarks.Setup` compares the two
parsers field by field — every field serialized, positions and raw text for an invalid one — and
throws before timing if they differ. One gap: the stand's own FIX rows check only the number of
fields and the number of invalid ones (`Stand.cs`, `FixNotWhatItSays`). The deep comparison lives in
the benchmark project and runs over its workloads, not over the slope rows the headline is taken
from. Worth closing, since that is the gate which would catch a reading that builds less per field.

**Did we win because the hand parser is written worse than it could be?** In part, and the
repository has already measured how much. The journal's anatomy of the hand parser's 51 ns a field
is: a `Peek` per character through its `Input<T>` class 14, the iterator 12, the factory 19,
`ToArray` 5. Only the factory is work the read owes; 31 of 51 ns is the parser's shape. That shape
is not an accident — one generic `Input<T>` covers four input forms, which is what D1 asks of it,
and specializing per form is exactly what a generator gets for free — but it means "0.76x the hand
parser" is in part a statement about our yardstick.

Which is why the repository built a floor, `IdealFixParser`, and it was timed in the same run on the
same rows (`benchmarks/results/stand-2026-09-19b.md`):

| fields | hand, ns | generated, ns | ideal, ns | generated / hand | generated / ideal |
| ---: | ---: | ---: | ---: | ---: | ---: |
| 0 | 25.4 | 15.5 | 9.6 | 0.61x | 1.61x |
| 1 | 100.5 | 73.0 | 64.9 | 0.73x | 1.12x |
| 2 | 167.3 | 130.4 | 115.8 | 0.78x | 1.13x |
| 4 | 290.5 | 219.3 | 198.3 | 0.75x | 1.11x |
| 8 | 548.6 | 431.2 | 366.4 | 0.79x | 1.18x |
| 16 | 1,042.1 | 798.5 | 758.5 | 0.77x | 1.05x |

Over 0 to 16 fields that is 63.5 ns a field by hand, 48.9 generated, 46.8 ideal. **Per field the
generated parser is 1.04x the ideal reader** — on the 18th it was 3.3x — and that is a far better
sentence than the one that went to Igor, because it cannot be answered with "your hand parser is
slow". The objection is only that the family table drops the `ideal` column the same run produced,
and the summary names the yardstick that flatters us.

**And where the claim does not hold.** Bytes 1.71x and a stream 2.19x of the hand parser, which pays
the same `Input<T>` and iterator tax on those forms too. There is no ideal reader for bytes or for a
stream, so two thirds of the FIX rows have no floor at all. "FIX is faster than the hand parser" is
a statement about the string form.

**A remark on D20, from the same place.** When the emitted code starts taking what a consumer's
framework offers, the generated side of every ratio moves and the hand parsers stay where they are.
Part of D20's gain will therefore arrive as a widening of the yardstick's handicap rather than as a
parser getting faster. The decision already requires that a pair compare our two branches on one
platform; a ratio against a hand parser has to be read the same way, or the hand parsers should be
given the same APIs when the branch lands.

**Answer:** —
