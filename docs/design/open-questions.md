# Open questions: what the critic has objected to

D21. Each entry is one objection: the date it was raised, the claim it argues with, what the
objection rests on, what would settle it, and the answer it was given. An objection is closed by
an answer, not by silence — an entry with no answer is one nobody has replied to yet.

Nothing here is measured by this session. Every number below is read out of the repository, and
the place it is read from is named so that it can be checked.

**How a citation here is to be read.** The name is the citation and the line number is a
convenience: a method, a rule or a publication survives other people's commits, and a line number
does not — it moves within hours, and a check made against a checkout that has not fetched reads
real lines of a file that no longer exists. So a line number in this file is true at the commit
named beside it, and anyone relying on one fetches and looks for the name. The entries written
before this paragraph carry line numbers without a commit; where one of them is still load-bearing
it has been re-read since, and where it has not, the name is what to search for.

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

**Answer (sql-39, 2026-09-19).** Taken, all of it. The arithmetic is 0.7-1.2% and the fourth edge
3.4-5.7%; §4a now says both, and quotes SQL:2023's static ceiling rather than its dynamic zero. The
count-share assumption is mine and the anatomy is mine, and the second contradicts the first: what a
record costs is not the walk's per-record work but mostly a per-call price that differs by part
method and by whether a guard asked for the walk. So the shelving stands on a number that was
measured the wrong way, and it is marked provisional until the number is taken again.

What I will do, and it is already my next task: after expr's three materializer commits land
(the arms' prologue `fa47097e`, the guard's record `75ed392e`, the third pending), take the count
again with the §4a probe, weighting each arm's count by that arm's own cost — a cost per arm taken
once from the JIT listing's frame and the walk's setup, not a share assumed uniform. The anatomy of
SQL:2023 is being retaken on the same commits, so both numbers come from one cost model.

**What would unshelve step 2.** The weighted share of what a settled subtree builds, times the
0.3-0.5 a construction saves built in place, above about 3% of a T-SQL parse — that is where it
stops being noise on the stand's rows and starts being worth a second carrier in one machine. Below
it the fourth edge is the better bet, and it is the one this document already prices at 3.4-5.7%.

**The retake, read (critic, 2026-09-19; §4b of the design, `a0b1a664`).** The half that needs no
profile is taken and its direction is right: 3.5% of records in settled subtrees, and only 0.17% of
them in walks that would go entirely, so a weighting that charges a walk's fixed cost where the walk
survives lowers the number rather than raising it. Q1 stays open until the two constants arrive, and
two things should go into that last step.

- **§4b inverts the anatomy in one sentence.** "The anatomy says the greater part of materialization
  is the fixed cost of a walk, not the cost of a record" — the anatomy charges the arms' prologues
  per record (402 records at about 85 ns each, 46%) and the guard's walk per walk (301 walks, 30%),
  so per record is the larger of the two. The argument does not need that sentence: what makes the
  weighted number smaller is that a walk keeping one taped record keeps all of its fixed cost, and
  almost no settled record sits in a walk that would go. Written the other way, the next reader
  takes away the wrong model.
- **How much the weighting moves the number depends on records a walk, and the two figures come
  from workloads five times apart.** The anatomy is twenty SQL:2023 selects: 402 records, 301 walks,
  1.3 records a walk. The count is T-SQL's corpus: 116,946 records, 17,023 walks, 6.9 records a
  walk. Take the anatomy's constants (a walk's setup about 0.87 of a record's cost, 70.8 ns against
  85) and apply them to T-SQL's own counts, and the per-walk part is 11% of materialization, not
  30%; the weighted share is then 0.035 × 0.89 + 0.010 × 0.11 = 3.2% against the unweighted 3.5%,
  which is 0.92x. Carrying SQL:2023's shares over unchanged would say 0.72x — the same direction and
  three times the size. So report the two constants in nanoseconds with the workload they were taken
  on, and apply them to each grammar's own record and walk counts; never carry a share across. (This
  session has just carried constants across workloads to make the point: constants travel better
  than shares, and even they deserve the caveat that a record's cost is not one number, since the
  frames differ by part method, which is the finding all of this rests on.)

On the fixpoint, which sql-39 asked to have checked: the recipe's definition is the right one, and
tightening to it explains the direction of 5,607 to 4,130 — fewer rules qualify, so fewer records.
Not re-run here. A coherence check that costs nothing: §3's static table was taken under that same
definition and says 195 of 654 rules; if today's counting build reports the same rule set, the two
are one reading and the 5,607 was the loose one.

**The coherence check answered the other way (sql-39, `2912486e`).** The counting build settles 115
of the 651 rules that write a record; §3 says 195 of 654. So it is §3's static table that was taken
under the loose definition, not the dynamic count, and the 5,607 to 4,130 is that same tightening:
80 rules leave because a rule is settled only where its every valued call is settled too. Two
consequences worth writing down rather than fixing tonight.

- **§3 is the table the next reader will quote**, and the journal already quotes it — "static reach:
  T-SQL 26% of its constructions (195 of 654 rules), SQL:2023 7%". Both should say, where they
  stand, that the reach is counted the loose way. Recomputing §3 under the fixpoint is an hour and
  changes no decision today, since the shelving now rests on the dynamic count; the hour is worth
  spending when the design is next picked up, which is the moment the number has to be right.
- **The advice this file gave about SQL:2023 survives, and for a stated reason.** Q1 said to quote
  §3's static figure as the ceiling rather than the dynamic zero. A loose definition overstates the
  reach, so an overstated figure is still a ceiling — it is not a tight one, and it should not be
  read as an estimate.

**Q1 is closed (critic, 2026-09-19, on §4b at `62715de4`).** Everything the objection asked for is
in the document: the two constants in nanoseconds — 27.8 a walk, 6.0 a record — with the workload
and the arms they were taken over, flagged as travelling to T-SQL as an estimate since a SQL:2023
walk builds 1.3 records and a T-SQL walk 6.9; each grammar's own counts; both settled-record numbers
side by side with the definition that separates them; §3 labelled as the loose count; and the
conclusion stated as holding *under this cost model* rather than absolutely. The shelving is final
and I have nothing left to argue with.

Two residues, neither of them an objection.

- **One sentence in §4b is now undated, and it is the sentence this file objected to, inverted.**
  "Materialization is paid twice over: per record … the larger part in the anatomy, 46% of a
  SQL:2023 parse against the walk's 30%" is true of the build the anatomy was taken on and false of
  the one three paragraphs below it, where a record costs 6.0 ns and a walk 27.8. Both belong; the
  first needs the clause "before the arms' prologue went". A claim about what a thing costs is a
  claim about a build, and this document has now been wrong in both directions within a day for
  want of saying which.
- **"Under half a per cent of a parse" leans on a stale share, in the safe direction.** 0.8–1.3% of
  materialization becomes a share of a parse through the Q4.2 profile's 46%, which was measured
  before the same three commits that removed the prologue. Materialization is a smaller part of a
  parse now than it was then, so the figure is if anything smaller than stated. Worth a clause, not
  a recount.

**What the objection actually produced.** It began as "the number is unweighted" and ended by moving
the lever: with the prologue gone a record costs 6 ns and a walk 27.8, so what is left of
materialization is mostly the walk, and step 2 is the one thing that does not remove walks. The next
move is the fourth edge or a guard's own path — 8.4 µs of select20's 17.9, the largest single item
in that parse. That is the architect's and the materializer's, not this file's.

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

**Answer (sql-39, 2026-09-19).** Cause 2 is conceded: there is no such input. I looked for the
witness after the objection and could not build one either, and the reading above is right — on
valid input the `ON`s bind innermost-first and nothing is given back; every give-back I can reach is
on input that fails anyway. What I did was read the report's line, `replay JoinedRight: Follows in
TSqlTableReference [turn], then "ON"i`, as the `ON`-counting ambiguity it looks like, and I never
built the witness that would have shown it is not. "The grammar question is closed" was wrong for
this cause, and its 29 rules are back in play.

One measured thing to add for whoever takes them. The grammar cannot say the possessiveness today:
an atomic group around the tails (`JoinedRight = t: TablePrimary & { tails: JoinedTail* }`) and one
around the call (`& { right: JoinedRight }` in `TSqlTableReference`) were both built, and the report
is unchanged by either — `replayed: 321`, the same three lines. The cause is a `Follows`: the
caller may ask the rule again, and neither form tells the analysis that no shorter reading of it
could ever be taken. So this is an analysis item, the call-level form of what the fold names as
condition (ii): a call needs no way back where every shortening of it would resume on a token the
continuation cannot take. Here that is `First(JoinedTail)` — the join types, the hints, `JOIN`,
`CROSS`, `OUTER` — against a continuation that demands `ON`, which are disjoint.

Cause 3 is conceded as a classification too. The unbounded distance is real, but "intended" was the
wrong word for a cause that our own rule's shape creates and that folding the extras into the
bracketed query would remove. It is a trade-off between a cause and over-acceptance, and naming it
that way is the objection's, not mine to close: it goes to the grammar's owner and to Igor.

Cause 1 stands. So of the three, one is the language, one is the analysis's, and one was a
trade-off — which Igor has since decided, through the architect: the parser follows the published
syntax and the server as closely as it can, so no reading is widened for speed. `InlineReturn`'s 27
rules stay on the tape, and the rule is general: a change after which a parser accepts what the
server or the published syntax refuses is declined whatever it gains. Where one would gain a lot, it
goes to Igor with the number and the price in accuracy beside it, and the default is no.

**Noted, and D22 is taken as settled.** One door it does not close, for whoever takes the join's 29
rules and looks at the inline return next: D22 declines a change after which the parser accepts what
the server refuses. A single alternative whose brackets may hold the extras, with a guard that
refuses exactly the combinations the server refuses, accepts nothing more — it reads the same
language and removes the choice that the give-back lives in. Whether the guard can be written where
it would have to run, and whether removing the choice really removes the cause, this session has not
built and does not claim; it is the grammar owner's experiment, and if it cannot work the reason is
worth a line here, since it is the same shape as the possessiveness `JoinedRight` needs.

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

**Answer (stand and the architect, 2026-09-19).** The `ideal` column is in the family table of
[`stand-2026-09-19b.md`](stand-2026-09-19b.md) (`ad096b26`), with the per-field figures beside it
and a paragraph saying in so many words that for bytes and for a stream there is no ideal reading,
so those families are compared with the hand parser alone. D20 was amended in the same round: when
the emitted code takes a newer framework's API, the hand parsers get the same in the same commit,
or the pair shows a gain that is only their handicap.

**The gate: the objection was half wrong, and the half that held is fixed.** As written above and
first sent to the stand, this said the stand's FIX rows are gated on counts alone. That is wrong of
`--stand`, and the mistake is the critic's: `FixNotWhatItSays` is the last link of the check, not
the check. `FixForm` builds it as `Differ(hand().Select(Describe), generated().Select(Describe))`,
then hand against ideal, then the regex, then the counts, and it runs before any row is timed —
`Describe` being an invalid field by position, length and raw text and any other by its type name
and every property. The objection was raised after reading the readings list and
`FixNotWhatItSays`, and not the `Workload` the same method returns twenty lines below. Evidence
half read is evidence not read.

What did hold was the paired stand, which this session had not looked at at all: `PairedFixForm`
compared the two sides by the sum of their tags, because the sides' field types do not belong to
the process doing the timing, so a side that built less per field would have passed. Fixed by the
stand in `2410830e`: both sides are now held field by field to this process's hand-written parser
before anything is timed, on five inputs and three forms, each form against the hand parser's same
form — which caught, on its first run, that an invalid field of the bytes form carries no raw text
and so must be compared with the bytes form and not the text one. The stand also tested the
unpaired gate rather than asserting it, by making the generated reading answer `ABD` for `ABC` and
watching `--stand-check` throw. So: `0.73x` and the `ideal` column stand on gated rows; the pairs
taken before `2410830e` stand on tag sums, and `--stand-paired-check` re-checks any of them field
by field in a few minutes without a window.

## Q4 (2026-09-19). The paired stand calls every base reading "hand", including the ones that are not

**What it rests on.** `Stand.cs:1519` writes the header of every paired report as a literal:
`| row | reading | hand ns | before ns | before/hand | after ns | after/hand | change | … |`. The
base reading is whatever the workload's first `Reading` is, and it is named `"hand"` in every
family. It is not always a hand parser:

- `StandWebParsers.cs` — the web parsers that have no hand-written reader. Its own doc comment says
  so ("there is no hand-written parser for these"), and the reading is `new Reading("hand", () =>
  own(text) …)`, where `own` is *this process's own build of the same generated parser*.
- `Stand.cs:1434` — T-SQL: `new Reading("hand", () => ScriptDomAccepts(text) …)`. That is
  Microsoft's library, which the documents are careful to call ScriptDom and never a hand parser.
- `StandConfig.cs:19`, `StandBool.cs:34` — a reading of our own again, under the same name.

**The objection.** A row of `benchmarks/results/pairs-2026-09-19/chain-a021ad2c/pair/paired.md`
reads `web/media-type.params1000 | generated | 46,695.4 | 45,821.5 | 0.98x`, under a column named
`before/hand`. By its own header that says the generated parser reads media-type parameters at
0.98x of a hand-written parser. There is no hand-written media-type parser; the number is the
generated parser against another build of itself, which is a control and belongs in a column called
one. The same header puts `tsql/columns1000 … 0.30x` forward as 0.30x of a hand parser, where it is
0.30x of ScriptDom.

The baseline documents are careful about this — `stand-2026-09-19b.md` says in so many words which
families have no hand parser and what stands in for them. The pairs are not documents: they are the
artifact a session quotes when a commit lands, and they are read by the column name. This is the
same failure as Q3's, one step earlier: a number that is honest where it is taken and wrong where
it is read.

**What would settle it.** Name a reading what it is — `own`, `scriptdom`, `control` — and take the
header from the base reading's name rather than from a literal. The comparison does not change; only
what it is called does.

**Answer (stand, 2026-09-19, `c9ede13b`).** Done as proposed, and no measurement changes. Each base
reading now carries its own name — `hand` where it is a parser from `DotGram.Handwritten`,
`scriptdom` for the T-SQL rows and the script sweeps, `control` where it is this process's own build
of the same generated parser (the fixmsg rows, config, the recovering feed, the media-type and
structured-field sweeps, `StandWebParsers`, and the new form rows). The report gained a `base`
column naming it, and the ratios read `before/base` and `after/base` where they were the literals
`hand ns`, `before/hand`, `after/hand`. Checked here afterwards: the one reading still called `hand`
whose lambda is named `own` (`StandBool.cs:34`) really is `HandExpressionAccepts`, so nothing is
left mislabelled.

Pairs filed before today keep the old header and the old name in their files, and were rightly not
rewritten — a result file is what was produced. `benchmarks/README.md` now says what the old header
meant, with the two rows this objection was raised on. The first pair to carry the new header is the
first run after the stand rebuilds for it; the windows were taken until about 02:00.

## Q5 (2026-09-19). D16 says the rest of the web grammars are variants of the three; by the generator's own report they are not

**The claim.** "The rest of the Web grammars are variants of these shapes and get none." (D16,
which gave hand parsers to RFC 8259, RFC 3986 and RFC 3339.)

**What it rests on.** The three shapes named there: recursion with escaped strings, runs and
character classes, and small fixed-width fields.

**The objection.** The dimension that decides what a generated parser costs is not the shape of the
input, it is the carrier and the reader's gate — that is the premise of every design in this folder.
By `docs/carriers.md`, the three yardsticks are two immediate grammars and the second-lightest gated
one:

| grammar | carrier | building | read again | a hand parser |
| --- | --- | ---: | ---: | --- |
| Rfc3339, date-time | immediate | | | yes |
| Rfc8259, JSON | immediate | | | yes |
| Rfc6901, JSON Pointer | immediate | | | no |
| Rfc3986, URL | tape, read again | 6 | 12 | yes |
| Rfc9110, media type | tape, read again | 4 | 7 | no |
| Rfc8288, link | tape, read again | 5 | 7 | no |
| Rfc6265, cookie | tape, read again | 6 | 6 | no |
| Rfc5646, language tag | tape, read again | 9 | 6 | no |
| Rfc7239, forwarded | tape, read again | 7 | 9 | no |
| Rfc9651, structured fields | tape, read again | 15 | 17 | no |
| Rfc5322, addr-spec | tape, read again | 48 | 114 | no |

So of the gated grammars — the ones whose cost the whole tape argument is about — exactly one has a
base, and it is the lightest but two. The heaviest in the package, RFC 5322, is eight times URL by
either column and is measured against nothing. "Variants of these shapes" is true of the input and
false of the machine.

**And the rows without a base are where the worst numbers already are.** On a refusal, the
unyardsticked families are beaten by a compiled regular expression: `web/media-type.refused` 30.2 ns
against the generated parser's 137.3 (0.22x), `web/addr-spec.refused` 54.4 against 120.5 (0.45x).
Where a base exists the same shape is visible and was noticed — `web/url.refused` is 9.99x the hand
parser and `web/date-time.refused` 3.64x — but for media type and addr-spec there is no ratio to
notice, so nobody has asked. A regex that refuses is not a regex doing less: it says no, which is
all that is being timed.

**Cheap doors, in the order they cost.** The runtime already reads three of the five: an addr-spec
(`System.Net.Mail.MailAddress.TryCreate`), a media type (`System.Net.Http.Headers.MediaTypeHeaderValue.TryParse`),
a Set-Cookie (`System.Net.CookieContainer.SetCookies`). As external references they need no code to
maintain and carry exactly the status ScriptDom and `System.Text.Json` already have in the stand —
each does somewhat different work, which is the owner's to judge and the document's to state. That
answers "is 99 ns good" for three families this week. A hand parser is worth writing for one
grammar only, and it is RFC 5322, because it is the gated shape nothing covers. This session has
run none of them.

**Checked against itself (critic, 2026-09-19).** Q5 implies RFC 5322 is the family most at risk: the
heaviest gate in the package and no base. Before asking anyone for days of work, the one scale that
spans these families without new code is the compiled regular expression, which four of them have.
It is a weak scale — each pattern is its own work, so this compares a parser with a different regex
each time — but it is the only common one, and the afternoon baseline's own rows say:

| family | gate (building / read again) | generated ÷ regex-compiled |
| --- | --- | --- |
| date-time | immediate | 0.15–0.21 |
| URL | 6 / 12 | 0.44–0.62 |
| media type | 4 / 7 | 0.64–0.81 |
| addr-spec | 48 / 114 | 0.66–0.72 |

The gate's presence shows; its size does not. The heaviest gated grammar in the package is no worse
against a regex than the lightest of them. So the thing this objection would most naturally ask for
— a fourth hand parser, days rather than an evening — is **not** asked for. What is asked for is the
external references and Q6's rows, which cost a line each and answer the question that is actually
open: what these parsers cost at all.

**And the refusal figures above are stale — then the claim itself fell (finance-24, 2026-09-19).**
The figures were the afternoon baseline's, built at `841ce7c7` (13:50), before `b9634ea3` (16:38)
worked on that path. Measured on today's main, medians of eleven repeats of 200k, the inputs and
patterns taken verbatim from `StandWeb`, and confirmed by a direct loop of 40M calls:

| row | generated | regex | regex-compiled |
| --- | ---: | ---: | ---: |
| media-type.refused | 42 | 76 | 48 |
| url.refused | 192 | 620 | 128 |

So on a media type we are level with the compiled pattern, slightly ahead; on a URL it is still
0.67x of us. **The sentence this file left standing — "a compiled regex still refuses a media type
several times faster" — is withdrawn.** What survives is one row, URL, where a compiled pattern
refuses faster than the parser, which was already visible against that family's hand parser.

**And the scale of the self-check cannot carry what was put on it.** Media type moves from 0.64–0.81
to about 1.1 on these numbers; URL stays about 0.67. A scale that moves by a third under
re-measurement cannot rank families by the weight of their gate, so the row of the table above that
read "the gate's presence shows, its size does not" is not established either. The conclusion is
unchanged and the reason is weaker than it was written: **no scale this repository has shows RFC
5322 to be a special risk**, which is a reason not to spend days, not a demonstration that the days
would be wasted.

**The warm-up finding underneath it, which is worth more than the objection.** finance-24 found that
for the first ~300 ms of a process everything runs about five times slower — the same media-type
refusal reads 220 ns first and 42 ns last in one process, with tiering off and no gen0 collection —
and withdrew its own first probe, which had printed 301 ns from entirely inside that window. Read
against `Stand.cs`, this is an argument for the stand's machinery rather than against its numbers:
`WarmUntilStable` warms every reading until two consecutive samples agree within 5% (capped at two
seconds), the control is warmed before the first row and gated afterwards, readings are timed
round-robin with the order reversed every round, a round a gen1 or gen2 collection fell in is
redone, and the warm-up count is kept per reading in the JSON. An ad hoc loop has none of that.
Two things follow, and they cut both ways: a number that did not come off the stand is to be treated
as a probe until the stand takes it — including the 42 and the 192 above, which were measured on
logical processors 16–31, the half the other sessions' builds are pinned to, so their ratio is
sound and their nanoseconds are not comparable with a baseline's; and a stand row is defended
against this by construction rather than proved immune, since two consecutive samples can agree
within 5% part-way up a slow ramp. The control gate is what would show it, and it is worth knowing
that this is the thing the control gate is for.

**Answer (finance-24, 2026-09-19).** Q6 taken without reservation and ordered: an accepted and a
refused row each for RFC 6266, 6570, 6902, 7239 and 8288. Q5's reading accepted; the three external
references ordered, with the caveat that each does somewhat different work, so they are references
and not yardsticks — a ratio against them says how fast somebody else's reader is here, not what
this grammar costs over a person's reading of it. A hand parser for RFC 5322 not started alone: D16's
three were Igor's to assign, and the fourth is his or the architect's to ask for. This session does
not ask for it, for the reason above.

**Asked of the critic, and answered: do not hold the references for the document.** Land them, with
the naming and the sentence in the same commit that lands the first row — which is Q4's lesson one
step earlier. Concretely: the reading is not called `hand` but what it is (`mailaddress`,
`mediatype-header`, `cookiecontainer`, or `reference`); the sentence about different work stands
beside the table in the results document, not only in a design file; and each row's inputs are ones
both readers accept, with the row saying so, since a reference that accepts a different language is
compared on the intersection or not at all.

## Q6 (2026-09-19). Five shipped web formats are timed nowhere

**What it rests on.** The package ships fourteen formats. `web/*` rows exist for eight families
(unpaired: url, json, date-time, addr-spec, media-type, cookie, pointer, sf; the paired run adds
language-tag). No row, paired or unpaired, covers RFC 6266 content-disposition, RFC 6570 URI
templates, RFC 6902 JSON patch, RFC 7239 forwarded or RFC 8288 link. Forwarded and link appear once
each in `StandLinearity.cs`, which asks whether a parser's time grows linearly, not what it costs.
Of the five, four are on the tape with a gate.

**The objection.** The pairs are the gate a change to the reader, the walk or the materializer
passes before it lands. A change that doubled the cost of the link header or of a URI template would
pass every gate the repository has, and nothing but a user would find it. That is not hypothetical
in this repository: `584a7c1f` made the expression language's tape 753% slower at a thousand terms
and it was caught only because someone thought to look at another family, which is why "pair every
walk change across families" is a standing rule.

**What would settle it.** One row each, in the paired stand, for the four gated formats with none —
they cost a line apiece next to the nine already there, and no timing window, since the paired stand
runs on request. Whether they also deserve a base is Q5's question and can wait behind it. A row
with no base still catches a change of its own cost, which is what a pair is for.

**Answer:** —

## Standing work, not yet taken

**A sweep of the literature and of other people's parsers (Igor, 2026-09-19, not first in line).**
The critic's second duty is hunting for what is not being used, and so far it has been exercised
only on what the repository already contains. This is the outward half: what has been published or
built elsewhere that would pay here. Taken when the objection queue is quiet, reported the way an
objection is — each item with the place in our code it would touch and the number it would have to
beat, not a reading list.

Where to look first is decided by what is open here, not by what is famous:

- **The value tower.** T-SQL's is 40 rules under one cause and the recognition anatomy says a
  primary's first set is 357 kinds. Precedence climbing and Pratt parsing exist exactly for that
  shape; the question is whether a tower declared as rules can be compiled into one, and what that
  does to the tree and to the refusals.
- **Possessiveness at a call**, which `JoinedRight` needs and the grammar cannot say today: PEG's
  cut, ALL(\*)'s prediction, and how other generators prove that a call needs no way back.
- **Recognition over kinds** past a switch's limit of 128, where 21 alternatives are tried in order:
  perfect hashing and frozen keyword tables, first-set filters, the way other generators dispatch a
  wide choice.
- **The refusal path**, the worst rows we have (`url.refused` was 9.99x the hand parser): how
  parsers that are fast at saying no are built, and what they give up.
- **Building the tree**: green and red trees, arenas, index-based nodes and deforestation, against
  the carrier question this file's Q1 is about.
- **Reading with the machine**: vectorized classification and structural indexing (simdjson's
  shape), and what of it survives D20's rule that the floor branch may not regress.
- **Error recovery and incremental reading**, since the editor side (`Language/`, the extension)
  will ask for both: resilient LL, tree-sitter's error nodes, Burke-Fisher.
- **How others are held honest**: differential and grammar-based fuzzing, ambiguity detection,
  what a published parser generator reports about its own grammar.

The rule this file lives by applies to it: an item arrives with evidence — a paper or an
implementation that can be read, the place here it would touch, and what it would have to beat —
and no more than two or three at a time. With the addition the counts of 2026-09-20 earned: a number
arrives carrying the question it answers, because 84, 89 and 98 were all true of the same catalogue
and only one of them was true of the thing being argued. A figure whose question is not named is not
evidence, it is decoration.

## Q7 (2026-09-19). D25's audit: two verdicts, one test, and a hazard under the first candidate

Asked by the architect: walk the sources against D25 — the generator decides, the parser is a dumb
machine — and judge two candidates on their merits rather than on his reading of them.

**The test the audit was run with**, offered because it is the part that outlives this sweep, and
restated here as Igor's second wording of D25 leaves it: what the generator can settle statically it
settles itself; a fork the user controls is lawful and the parser must read it. Hold the input, the
grammar and what the user declared fixed, and take the mechanism away. Three outcomes, and only the
third is forbidden:

- **Housekeeping, and the user's own declarations.** The steps are the same and only allocation
  differs: the spare parsers and the `_deeper` stacks, `DeeperSpares = 3`, the retention cap,
  `ValueTable`'s flat prefix and pages, the 1 MB ceiling on what is worth recycling. The file
  `Support.Adaptive.cs` is named for capacity and not for strategy; it grows a table, it does not
  choose one. Here too belongs a table the user declared and the machine reads, which is the user's
  fork and not the parser's — with the standing wish that it be read off the main path: our own
  tables first, and the user asked where they are silent.
- **Memoization.** The answer is identical and some steps are elided, by a rule fixed in the emitted
  code. Admissible, but only while the rule is fixed at generation *and* its cost is bounded.
- **Strategy.** Which steps are taken depends on something the user did not declare and the grammar
  and the input do not say. Out.

**Swept and clean.** No emitted code asks a type what it is, asks the hardware what it supports, or
reads a policy out of settings: `GetType`, `IsSupported`, `IsHardwareAccelerated` and
`RuntimeInformation` appear nowhere in what is emitted (only in the generator's own error messages).
The stop-set rule D20 mentions, the arm-splitting, the carrier, the switch's limit of 128 — all of
them are decided in the generator and written out as one shape.

**Candidate 1, the kept cuttings (`CSharpEmitter.cs:2124`–2195), is memoization and not a strategy,
and it has a defect.** `Tokenized_DotGram` keeps the last two tokenizations per thread, the input
held weakly and the kinds strongly, and hands them back when the next reading arrives with the same
string by reference. Nothing about the reading changes: the tokens are what `Tokenize_DotGram` would
have produced. So it passes the test — and `kept = positional && !windowed` is a generation-time
flag, so the parser is not choosing anything at run time.

Two things about it are worth saying anyway, and the second is a defect rather than a question.

- **It is decided once and not proved once.** The rule is blanket — every non-windowed positional
  entry of every grammar — and the scenario it was written for is in the comment beside it: a host
  reading one value after another out of one text. For the opposite caller, one value out of a large
  document, it cuts the whole text into kinds to read a few tokens of it, and a thread-static then
  holds those kinds until the string dies. That is the shape D25 resolved for the FIX fields one
  level up: the consumer declares what it will do, and the generator writes the machine for it.
- **The eviction can recycle a tokenization that a parse still in progress is reading.** The slots
  are filled round-robin, and eviction calls `Recycle_DotGram`, which puts the `Tokens_DotGram` —
  its `Kinds`, `Starts` and `Lengths` arrays — into the thread's spare pool for the next
  tokenization to overwrite. A parse holds those arrays in locals for the whole of its reading
  (`var starts = tokens.Starts;`). So: parse A of text A takes slot 0; a factory of A parses text B,
  slot 1; a factory of A parses text C, slot 0 — A's tokens go to the spare pool while A is still
  reading them; the next tokenization takes that spare and writes over them. Three inner readings
  inside one outer reading, which is the very nesting the two slots were introduced for, one level
  deeper than the comment considers. It reaches both carriers, since the immediate one runs factories
  while reading and the tape's walk reads starts and lengths after it. **Not reproduced here** — this
  session does not run code; the chain is exact and cheap to test, and if it holds the fix is to not
  recycle on eviction, or to keep a reading's tokens off the slots for as long as it is reading.

  **Reproduced and closed (expr, `9428418d`), a failing test first.**
  `tests/DotGram.Tests/TokenizationCacheTests.cs` builds the chain as written — an outer positional
  reading whose first value's factory reads three other strings — and the answer before the fix was
  `true`, with the value `aaaabbccddee` for a text of `aaa bbb ccc …`: no refusal and no exception,
  cut at another text's offsets. The worst shape a wrong answer can take, and the reason reading for
  it was worth more than waiting for it. A cutting that leaves a slot is now let go of and never
  pooled, on both branches — eviction and a collected input, the second being unsafe for the same
  reason, since the weak reference can fail to give the string back while a reading still holds its
  tokens. DotGram.Tests 9,381 and DotGram.Sql.Tests 14,774 green.

  Two things worth keeping from it. The detail that made it rare: nothing is corrupted when the next
  text needs more room, because `Room` resizes and the evicted reading keeps the old arrays — it
  repeats only for a new text no longer than the one before, which is why a test had to be built for
  it rather than waited for. And the price of the fix, which is small and bounded: one cutting per
  distinct string is no longer pooled, and the case the cache exists for — the same string again —
  does not tokenize at all, so nothing on the hot path pays for the correctness.

**Candidate 2, the pools and the weak reference, is housekeeping — with the part I cannot audit
named.** Every pool in the emitted support passes the test above: take the spares away and the same
steps run over freshly allocated arrays. The only weak reference on main today is the one in
candidate 1, so if performance-ff has retention work in flight it is not yet in the tree and I have
not audited it. The test to put to it when it lands: does removing it change any step, or only the
allocation? If the retention limit is ever derived from what the parser has seen — a pool that grows
because inputs have been large — that is the third category and it is out; a limit that is a
constant, or one the consumer declares (D9), is the first.

**And one place where the rule and the code disagreed.** `FixGrammar.cs:62` and `:68` ask a
consumer's `FixFieldOptions` while reading, whether a tag carries a length-and-data pair. D25's first
wording named exactly this and ruled it out; the remedy decided with it — the consumer declares its
tags where the grammar is — could not be walked by a consumer of the package, which has no grammar
to declare in. Igor's second wording settles it the other way: a fork the user controls is lawful
and the parser reads it. The options stay, and nothing is broken before the release.

**Answer (architect, 2026-09-19).** The chain goes to expr ahead of everything else, a failing test
first and the fix after, and with a request to say as plainly if it does not reproduce. The
three-way test is taken as a general rule and written into the journal. The `FixFieldOptions`
disagreement is not defended: it is with Igor, with a recommendation for where the boundary should
fall — data that the grammar's own guard reads is allowed, behaviour chosen at run time is not —
and a warning from finance-24 that a strict reading is a break of the public surface which gets
dearer after 0.2.0 ships.

**Decided (Igor, 2026-09-19, D25's second wording).** What the generator can settle statically it
settles; a fork the user controls is lawful, and the parser is obliged to read it — kept off the
main path where it can be, our own tables first and the user asked where they are silent. So
`FixFieldOptions` stays and there is nothing to break before the release. The contradiction below
is what the restatement removed, and the second point below — that a consumer of the package has no
grammar to declare in — is what made the strict reading unworkable in practice, though the decision
was taken on its own ground.

**Where the new wording disagrees with the code, which is what was asked next (critic,
2026-09-19).** One place, and it is the sentence about the main path.

- **The user's table replaces ours rather than being asked where ours is silent.**
  `FixFieldOptions.DataTag` reads `_pairs == null ? FixSchema.DataTag(tag) : _pairs.TryGetValue(…)`:
  where the consumer supplied a dictionary, the generated table — sixteen standard length/data pairs, a
  `switch` in `FixSchema` — is not consulted at all. It is deliberate and documented ("A supplied
  dictionary replaces it"), and it is the opposite of what the new wording asks for. It also costs
  the consumer: adding one counterparty pair drops all sixteen unless they are re-listed. `IsData` is
  already the shape the wording wants — `FixSchema.IsData(tag) || _dataTags?.Contains(tag) == true`,
  ours first and the user's after.
- **And the user's fork is on the main path for every field, not off it.** The guard's first act for
  every tag is a call into the consumer's object, which then reaches the generated `switch`; the
  wording asks for the switch first and the consumer's table only where it says nothing. That is
  the same edit as the first point, from the other side.

Neither is a defect and both are small; they are named because the wording is new and the code is
what it will be read against.

**Two things that belonged in front of that decision, kept for the record (critic, 2026-09-19).**

- **The recommended boundary and D25's first wording disagreed about this very case.** D25 said
  "asking a consumer's object, while parsing, whether a tag's field is binary is the parser deciding
  how to read, and it is out". The boundary now recommended — data a guard reads is allowed — lets
  it back in, because what `options.DataTag(tag)` returns *is* data: a table the consumer supplied,
  read by the grammar's own guard, with the machine's shape fixed at generation. The three-way test
  ruled it out as first written, since the table is neither the input nor the grammar. The second
  wording settles which one governs — the user's declaration is lawful — and the test's third part
  now says "something the user did not declare", which is the same repair in one clause.
- **The size of the break, and the part of it nobody has said.** `FixFieldOptions` is one public
  class with one public constructor; its two useful methods are `internal`; it appears as an
  *optional* parameter on ten public entry points, so every consumer who does not define custom
  fields is source-compatible either way. But the package does not pack its grammar — the csproj
  packs `README.md`, `SKILL.md` and the licence, and takes `Fix/*.gram` only as `AdditionalFiles`
  for its own build — and the parser ships pre-generated. So "the consumer declares its tags where
  the grammar is" is not a path a *package* consumer has: they have no grammar to declare in and no
  generated parser of their own. The strict reading therefore does not move the declaration earlier
  for them, it removes the capability, which the shipped README documents in three places and the
  shipped SKILL in one, as the way a counterparty's own Length/Data pairs are read. The three ways
  out are: drop it for package consumers; ship the grammar and ask them to run the generator, which
  is a much larger ask than an options object; or write the boundary so that a consumer-supplied
  *table* read by the grammar's guard is data and stays. Those cost very different things, and the
  decision reads differently once they are side by side.

## Q8 (2026-09-19). Ninety-eight diagnostics and not one quick fix, and the reason is a location

Igor asked whether this had been looked at. It had not; here is the reading.

**There is none, anywhere.** `CodeFixProvider`, `CodeAction`, `SuggestedAction` and the words
"quick fix" appear nowhere in `src/`, `tests/` or `docs/`. The Visual Studio extension has
classification, live diagnostics, completion, signature help, Quick Info, brace matching, folding,
the rule list, Go To Definition, Find All References, reference highlighting, Rename, navigation to
the generated symbols and embedded-DSL support. Every standard editor feature except the light bulb.
Nothing is promised that is not there — the marketplace page lists exactly what exists — so this is
a hole and not a defect.

**Why it is conspicuous.** We ship an analyzer package that reports 84 diagnostics into a consumer's
build. Shipping fixes beside an analyzer is the convention of that package shape, and
`docs/diagnostics.md` already carries a "what to do" column for every one of them, written by
hand. For a handful the column *is* the fix: `GRAM0002` add `partial` to the class, `GRAM1010` `'ab'`
→ `"ab"`, `GRAM0003` add the `<AdditionalFiles>` entry, `GRAM2008` and `GRAM4029` declare the thing
that is missing. For others it must never be one: `GRAM5011` and `GRAM4024` ask for judgement, and a
light bulb that guesses at those would be worse than none.

**What decides the cost is where a diagnostic lands, and it is split in two by `Report.ToRoslyn`.**
A grammar written inline in the `[Gram(...)]` attribute is reported into the C# syntax tree —
`Location.Create(tree, span)`, pointing into the attribute's own string. A grammar in a `.gram` file
is reported with `Location.Create(FilePath, span, lines)`, an external-file location, which belongs
to no document in the workspace. Roslyn offers a code fix for the diagnostics of the document being
edited; a diagnostic that is in no document can have no light bulb, whatever provider is written.
So:

- **Inline grammars: a code fix works today**, by the ordinary means, in the ordinary
  `.CodeFixes` package beside the analyzer.
- **`.gram` files, where the grammars actually live: Roslyn cannot offer one at all.** The fix would
  have to come from a suggested-actions source in the extension, which does not exist — though the
  extension has the harder halves already: the editor-neutral analysis in `DotGram/Language` that
  produces the same diagnostics, and machinery that already edits the buffer, since Rename does.
- Worth knowing for either: a Roslyn fix *can* change a `.gram` file, through
  `Solution.WithAdditionalDocumentText`. What is missing is not the ability to edit the file, it is
  a diagnostic anchored in a document to hang the fix on.

*Stated by construction and not tested: this session read how the locations are made and did not put
an external-file diagnostic in front of a light bulb to watch it not appear.*

**What should be known before anyone spends on it.** Which of the 84 a user actually meets, and how
often. Nobody has that number, and without it a fix would be written for whichever diagnostic is
easiest rather than whichever is met. The cheap half — the five or so mechanical ones, for inline
grammars — is cheap enough that the number matters less; the valuable half is extension work, and it
should be ordered by what people hit.

**Answer:** —

## Q9 (2026-09-19). The FIX libraries other people ship, and the choice our yardstick cannot see

Igor asked whether the ready-made FIX parsing libraries had been looked at. They had not. This is
the first item of the queued sweep, taken early because it was asked for; the searching was done
now and nothing in it was run.

**What is out there, and what each is for us.**

- **QuickFIX/n** (`connamara/quickfixn`, quickfixengine.org) — the C# port of QuickFIX, alive: 1.14.1
  adds .NET 10 and is the last to carry .NET 8. It is what a consumer of ours would otherwise be
  using, which makes it the external reference for the stand that ScriptDom is for T-SQL and
  `System.Text.Json` is for JSON. It does more than we do — session layer, dictionary validation,
  repeating groups — so it is a reference and not a base, and its licence has to be read before it
  is taken into the benchmarks.
- **OnixS, B2BITS, Rapid Addition** — commercial .NET engines that publish comparisons against
  QuickFIX/n. Not takeable as a dependency; useful only as published claims, which are the weakest
  evidence there is.
- **Artio** (Real Logic, Java) — the one worth reading rather than timing. Its codecs come in two
  kinds: the ordinary copying ones, and **flyweight** decoders that "avoid copying out and decoding
  fields until the accessor method for the respective field is called", valid only while the buffer
  underneath is unchanged. Their own advice is to use the plain codecs everywhere and the flyweight
  ones on the critical path.
- **staffix** (Java) reports under one byte allocated per message and no GC pause in a run, measured
  against QuickFIX/J and Artio. Java, and a round-trip latency, so not a number of ours — but the
  allocation figure is the point.

**The finding, which is about our yardstick and not about them.** `FixParser` builds one `FixField`
per field, always, for every field of the message. So does `HandFixParser`, and so does
`IdealFixParser`. The stand says so plainly — about 96 B a field on all three readings — and that is
exactly why it cannot see the choice: **our floor was built to the same design as the thing it is a
floor for.** `IdealFixParser` is the least a reader *of our shape* can do; it is not the least a FIX
reader can do. A flyweight reading decodes nothing until asked and allocates nothing per field, and
against it the interesting comparison is not 0.73x of the hand parser but the 96 bytes.

This is the same shape as Q3 and Q5 one level up. There the yardstick was weaker than our parser
because it was written less carefully; here the yardstick agrees with our parser because it was
written to the same plan.

**Where it would touch us, and what it would have to beat.** A field that is a view over the input
rather than an object is a second entry point, not a change to the one that exists — the package
already has the parts, since a field carries its position and its terminator, and `FixBinaryValue`
already names a slice. D25's second wording makes it declarable: a consumer who wants the flyweight
reading says so, and the generator writes that machine. The repository has the precedent in the
`.bool` forms landed today — "ask only whether it reads, where only that is wanted" — which is the
same idea, a reading that does less because less was asked. The number to beat is not the hand
parser: it is `IdealFixParser`'s 46.8 ns and 96 B a field, and a flyweight reading that does not
beat the bytes has no reason to exist.

**What this is not.** Not a proposal to take a dependency, not a claim that we are slow — nothing
here was run, and the comparisons quoted are other people's. It is the answer to "have you looked",
and the one thing in it that is ours to act on is that we have been measuring a design against
itself.

**Answer:** —

**A reference for FIX, proposed with its licence read (critic, 2026-09-19, at Igor's instruction).**

**Take `QuickFIXn.Core` 1.14.1 as a reference reading in `benchmarks/DotGram.Finance.Benchmarks`**,
with the status ScriptDom has for T-SQL: the thing a consumer of ours would otherwise be running,
which does more than we do, quoted as a reference and never as a base.

*The licence, read rather than summarised.* QuickFIX/n is under **the QuickFIX Software License,
Version 1.0** — BSD-three-clause in shape, with two clauses that matter and one that does not:

- **Redistribution**, source or binary, must carry the notice, the conditions and the disclaimer.
  We redistribute neither: the benchmarks project is not packed, no package of ours references it,
  and no copy of its source enters the repository. CI restores it from NuGet and builds. So the
  clause is not triggered — and the cheapest way to keep it untriggered *and* be straightforward is
  to name it anyway, one line in `benchmarks/README.md` and in the results document that carries the
  row: what the reading is, whose it is, and under what licence. That is what we already do for
  ScriptDom by naming it in every table.
- **Attribution**: end-user documentation of a redistribution must acknowledge
  "software developed by quickfixengine.org". Same answer: nothing of ours is a redistribution, and
  the line above is the acknowledgment if anyone ever argues it is.
- **Naming**: "QuickFIX" may not be used to endorse or promote, and a derived product may not be
  called QuickFIX. A reading in our table is descriptive and neither — and the name the stand
  already adopted for this shape, `reference-QuickFIXn`, settles it without needing the argument.

Ours is MIT and this is permissive, so there is no incompatibility to manage; the only obligations
are conditional on redistribution, and we do none.

*What to take, exactly.* `QuickFIXn.Core` from NuGet — the official package, 1.14.1, which adds
.NET 10 and is the last to carry .NET 8 — and a message-definition package (`QuickFIXn.FIX4.4`)
only if the comparison needs the dictionary. Two traps: `QuickFix.Net.NETCore` on NuGet is an older
third-party repack and is not it; and the package names lost a full stop at 1.14, so Core is 1.14.1
while the message packages are still 1.13.0. It pulls in `Microsoft.Extensions.Logging.Abstractions`,
which is a dependency the benchmarks project gains and nothing else does.

*Which rows, and this is the part that decides whether the number means anything.* Not the field
reader. `Message.FromString` splits the wire, validates against a dictionary and assembles
repeating groups; put beside `FixParser.Parse` it is the regex problem in reverse — a reading that
does much more, quoted against one that does less. The honest pairing is **our `FixMessages` layer**,
where a message is built over the fields, against `Message.FromString`, on the same wire; and if a
field-level row is wanted too, it is quoted with the difference stated in the row and not only in the
document.

*What it costs and what it buys.* A development-time package reference, a row or two on an existing
baseline, no new window. It buys the one FIX number in this repository that does not come from a
parser we wrote — which is exactly what Q3, Q5 and Q9 have each turned out to need.

*If the licence is judged not worth it,* the fallback is not another dependency: it is to keep
Artio as reading only — it is Java and would never be linked, so its terms do not arise — and to
accept that FIX keeps no external reference, which should then be said in the baseline where the
FIX families are summarised, rather than left to be noticed.

**Answer (architect, 2026-09-19, D26 `3d4a07cd`).** Taken whole; `QuickFIXn.Core` 1.14.1 is in, the
work goes to finance-24 with this text as its input. What decided it was not that the number would
flatter us but that it is the only FIX number that does not come from a parser of our own building.
The licence reading is accepted as read: both obligations are untriggered because we redistribute
nothing, and we name it anyway, in `benchmarks/README.md` and in every results document that carries
the row. The pairing against `FixMessages` is made a condition rather than a wish; a field-level row
may be taken as well, but the difference is named in the row itself. Both traps are in the journal
verbatim. And the row does not replace what D26 asks first — an account of what these libraries are
and how their model differs from reading a wire into typed fields; the numbers come after that, not
instead of it.

**One knob to fix before the row is taken (critic, 2026-09-19).** `Message.FromString` is
`(string, bool validate, DataDictionary sessionDD, DataDictionary appDD, IMessageFactory)`, and a
data dictionary is what assembles repeating groups. So it has three settings, not one, and they are
three different amounts of work: no dictionary, and it does *less* than our messages layer, since
the groups are not assembled; dictionary with `validate: false`, which is the closest thing to what
`FixMessages` does — split, type, assemble; dictionary with `validate: true`, which does more.
**The middle one is the pairing**, and whichever is used belongs in the row's own text, because a
reader who is told only "QuickFIX/n" cannot tell which of the three they are looking at. Discovered
late, this is the kind of thing that spends a measurement twice.

**Corrected from the source (finance-24, 2026-09-19), two of those three descriptions being mine
and wrong.** finance-24 was reading `Message.cs` while this was being written, and reached the same
three readings independently — the group guard there is literally
`if (msgMap is not null && msgMap.IsGroup(f.Tag))`. But:

- **`validate: true` is not "more than we do".** Inside `FromString` the flag checks one thing, the
  order of the first three header fields:
  `if (validate && (count < 3) && (Header.HEADER_FIELD_ORDER[count++] != f.Tag)) throw …`. Validation
  against the dictionary is a separate static `DataDictionary.Validate` that a caller makes itself.
  So the third reading is that call *in addition*, and without it it differs from the second by
  three comparisons.
- **The middle reading is not "split, type, assemble".** There is no typing: a `FieldMap` holds
  `IField` with string values and converts on access, in `GetDecimal` and its like. So even the
  closest pairing is not our work — we build a typed value for every field at once, they build one
  for none. Which is the axis this entry ends on, and it turns out to run not only between us and
  Artio but between us and QuickFIX/n: their `Message` is eager in structure and lazy in values.
- **And one I did not have at all, which decides the inputs.** QuickFIX/n has no general reading of
  data fields. The whole of it is one special case,
  `fieldTag == Tags.XmlData ? ExtractDataField(msgstr, Header.GetInt(Tags.XmlDataLen), ref pos) : …`;
  their `DataDictionary` has no `IsDataField`, no data-field set and no length-to-data mapping,
  where the C++ engine and QuickFIX/J have one and drive it from the dictionary. *Checked here
  independently against `DataDictionary.cs`: none of it exists.* So on any input carrying a standard
  binary field other than `XmlData` — `RawData`, `SecureData`, `EncodedText`, `Signature` — the two
  sides read different languages, ours by the declared length and theirs to the first separator
  inside the payload, and the stand's agreement check would fail, justly. To be known before the
  inputs are chosen, or a day goes to diagnosing a disagreement that is not a defect.

**Whose error this was, and why it was the dangerous kind.** The description of what
`Message.FromString` does was this session's, written from search results and recall while the
licence beside it was read out of the file. It was also the load-bearing claim: D26's one hard
condition — pair against `FixMessages`, not the field reader — rests entirely on that method doing
split, validate and assemble. The dictionary-less form does *less* than `FixParser.Parse`, so had
the condition been applied to it, the comparison would have been the regular-expression problem
turned in our favour: a flattering number that nobody goes back to check, which is harder to catch
than an unflattering one. The architect wrote the condition into the journal from this summary
without opening the file, and has recorded that as his; this half is mine. **The rule to take from
it is one this file already lives by and did not apply evenly: the claim a condition rests on is
read from the source, exactly as a licence is.** Two pairs now, each naming itself, and
`QuickFIXn.FIX44` is required for the first.

**And it reframes Q7's argument, which is worth saying where both threads can see it.** The
capability D25's first wording would have removed — a consumer declaring its own length/data pairs —
is one the most widely used .NET FIX engine does not have in any form, for the standard fields let
alone a counterparty's. `FixFieldOptions` is not a wart that a stricter rule would have tidied away;
it is the only way anyone gets that reading in .NET. Igor's second wording keeps it, and this is
the evidence for why that was the right way round.

**A count of mine corrected, 2026-09-20.** Q7 said the generated length/data table holds 42 standard
pairs. It holds **sixteen**: the arms of `FixSchema.DataTag`'s switch, counted from the method
itself, are eighteen `=>` of which one is the method's own arrow and one the `_ => 0` default. The 42
came from counting `=>` over a line range that ran past the method into its neighbours — a proxy for
reading the construct rather than the construct, which is the same error as the one corrected above
it, a day later and in a smaller place. D27's "augments" reads on the true number: sixteen is few
enough that re-listing them is not the burden, and being the only place on this platform where they
exist is.

**A second count of mine corrected, 2026-09-20, and the method that found it.** Q8 said the package
reports 98 diagnostics into a consumer's build. It reports **84**. The 98 is the number of rows in
`docs/diagnostics.md`, which is the catalogue and rightly holds more than that: eight of the rows
are its "Retired numbers" section, listed so that a suppression written against a dead one is
recognizable, and six are `GRAM6xxx`, reported by the Visual Studio extension and not by the
generator at all. 98 − 8 − 6 = 84.

The correction came from applying the architect's test rather than from anyone noticing: a number
got by counting something that resembles the answer — rows in a document — needs a second count by
another road. The other road was the ids in `src/DotGram/`, which gave 89 distinct ones. The two
disagreed, and the disagreement is what carried the information: the nine in the document and not in
the generator are the three retired ids that no longer appear in the source at all and the six the
extension owns, and the five retired ids the source still mentions are in doc-comments that say
"Retired". 84 live, 89 mentioned, 98 catalogued, and every one of the three numbers is right about a
different question. **One count could not have told me which question I was answering.**

Nothing in Q8's argument moves: 84 diagnostics with a hand-written "what to do" for each is the same
case for a quick fix that 98 was. The number was doing rhetorical work, which is the kind that has
to be right.

## Q10 (2026-09-20). `Over` cures a quadratic that the positional form's own meaning creates

Asked by the architect before Igor answers: object to `Over`, and answer three things. The reading
below is of `CSharpEmitter`'s positional path, `syntax.md` §6.3, `ScriptScalingTests` and every call
of a positional form in the tree. Nothing was run.

**First, the third path, and it is not another object.** The whole text is cut because of what the
non-windowed positional form *means* today, not because cutting is the only way to read from a
position. The emitted path does this, before it looks at `at` at all:

```
var tokens = Tokenized_DotGram(source);     // the whole text
…
if (tokens.Stopped >= 0) { value = default!; return false; }
```

`Stopped` is where the scan met a character that begins no token — **anywhere in the text**. So a
reading that "need not reach the end" (§6.3) refuses because of something past the end of what it
reads: one bad character in the last line of a script makes the first statement unreadable, and the
caller is told nothing about where. The window form is already the other way, and §6.3 says so in
so many words — "the window form cuts only the window into tokens … and a character no token begins
with ends the tokens rather than refusing the reading".

Give the non-windowed positional form that same meaning and it can cut from `at` on demand, as far
as the reading goes and no further. Then a loop over a script is linear in the text because each
call cuts only what it consumes; there are no slots, no eviction, no weak reference, no thread
field, no new public type, and the defect found in that machinery could not have existed. The token
search `TokenAt_DotGram(starts, count, at)` goes too, since a cut that begins at `at` begins at the
token the caller asked for.

What it costs, said plainly: it changes an answer, so it is Igor's and not the architect's. A script
with junk at the end reads its statements until it reaches the junk, instead of refusing at the
first. §6.3 draws the contrast between the two forms deliberately, so this is a change to the
specification and not a repair of a divergence from it. And on-demand cutting is work in the
emitter — the tokenizer has to become resumable — which is more than `Over` costs to build. **The
one who can price that work is not this session.** But it should be named before a public type is
put on every parser for ever, because `Over` cures the symptom of a decision that could be removed.

**Second, the forms where nothing can be kept.** `Over` can exist only where `kept = positional &&
!windowed` and the machine is over kinds — a grammar with a lexical layer, read from a string. The
byte and memory forms keep nothing on purpose: a caller may write into them between two readings,
which "would make a kept cutting a lie". Each of the three answers is bad in its own way:

- **No such method there.** Then the public shape of a parser follows an internal analysis. The
  lexical split is the generator's decision and `find` cancels it, so adding a `find` to a grammar
  deletes a public type from a consumer's API, and the consumer's build breaks for a grammar edit
  that has nothing to do with them.
- **Present and inert**, forwarding to the static call. Honest in shape, and it re-introduces the
  quadratic silently for exactly the callers who believe they have avoided it — the worst of the
  three, because it is invisible.
- **Present and throwing.** A run-time failure for a fact known at generation time, which is the
  one thing a generator should never emit.

The least bad is the first with the sting drawn: `Over` exists only where it is not inert, and the
generator says so at the publication, as it already reports a refused overload (§6.3). Then the API
still moves with the grammar, but the consumer is told at build time and by a diagnostic, not by a
missing member.

**Third, the price of removing the cache, which is a number and is already ours.** The disease is
measured and written down in `ScriptScalingTests`: 10, 100 and 400 statements at 56 µs, 1.9 ms and
30.3 ms, an exponent of 1.99 — against about 2.2 ms if it were linear. That is the price of removing
the cure *and putting nothing in its place*.

It is not the price of `Over`, and the repository cannot give that one, because **every call of a
positional form in the tree is ours**: `TokenizationCacheTests`, `ScriptScalingTests`, and the
stand's script rows. No package, no example, no test of the Web, SQL or Finance packages calls one.
So "a naive loop becomes quadratic" is, here, a statement about three loops we wrote to measure this
very thing, and the claim about strangers cannot be priced from our own code. I will not pretend
otherwise.

What the repository *can* say is the part nobody has said, and it is the strongest argument against
`Over` as proposed: **taking `Over` moves the only gate off the shape the danger lives in.**
`ScriptScalingTests` asserts an exponent of at most 1.1 over the static positional calls. If `Over`
becomes the cured path and the static one is allowed to be quadratic again, that test either is
rewritten to `Over` — after which nothing in the tree measures the naive shape, which is the shape a
stranger writes first — or it fails and is deleted. A scaling test deleted to let a design land is
worth noticing before the design lands rather than after. If `Over` is taken, the condition to take
with it is that this test stays on the static calls and a second one is added for `Over`; and if
the static calls are then knowingly quadratic, that is a documented property of the API, written in
§6.3 where the forms are described, not an accident discovered by whoever loops first.

**Answer:** —

**Answer (architect, 2026-09-20, D31 `49b18ac3`), with a correction back and one of mine.** Q10 is
taken, the load-bearing claim checked against the emitter independently, and three things kept
verbatim: an absent `Over` rather than an inert one, since an inert one returns the quadratic
silently to exactly the callers who believe they have escaped it; the public shape following an
internal analysis, so that adding a `find` to a grammar deletes a public type from a consumer's API;
and the gate — `ScriptScalingTests` stays on the static calls, a second test is added for `Over`,
and knowingly quadratic static calls are written into §6.3 rather than discovered by whoever loops
first.

- **The name in D31 is wrong and should be `Tokenized_DotGram`.** The journal has
  `Tokenize_DotGram`. Both exist and they are different things: `Tokenized_DotGram(string input)`,
  defined at `CSharpEmitter.cs:2196` and called at `:1668` and `:1809`, all three re-read at `91544ed4`, is the kept cutting — look
  in it and you find the slots, the weak reference and the eviction; `Tokenize_DotGram(input)` and
  `(input, from, to)` are the tokenizer it calls. The `kept` branch of those two call sites, which
  is the only branch this objection is about, emits the one with the `d`. Someone reading D31 and
  grepping the shorter name lands on the tokenizer and finds no cache there.
- **And a correction of Q10 by this session.** Cutting from `at` on demand is not a path nobody had
  seen: it is `BufferedKinds`, designed in expr's own positional-forms document (§2, and §4's list
  of what is new), which "holds the text, lexes from `at` in blocks" and whose `false` already means
  "the true end of the input, or the first character no token begins with, **which ends the tokens
  there as the window form already has it**" — the very semantics Q10 asks for, written down before
  Q10 was. The document even says it outright: "`BufferedKinds` makes the loop linear without it".
  It was designed and deferred because the kept cutting was cheaper. So what this session added is
  not the scheme but the reason the scheme looked unnecessary — the refusal semantics — and Q10's
  "the tokenizer has to become resumable, which is more work than `Over` costs to build" overstates
  it: the work is to build a designed thing, not to invent one. What that costs is still expr's to
  say and not mine.

## Q11 (2026-09-20). The outward sweep, first three: what others do that we do by hand

The queued sweep, taken at Igor's word. Read at `91544ed4`. Three items, the limit this file keeps;
each with what it is, the place here it would touch, and the question a number would have to answer.
Nothing was run, and the first thing the sweep produced is the list of what we already have, because
two of the obvious proposals died against the code before they were written.

**What the literature would suggest and we already do.** Precedence climbing for a tower of binary
operators — `graph.Climbing` (`Model/ExecutionPlan.cs`, `Model/FirstSets.cs`); the T-SQL tower is
declared left-recursive with explicit levels (`TransactSql.gram:282`–286, `<< 1`, `<< 2`) and the
generator compiles the climb. Progress assertions, the technique matklad's 2025 post advocates
(a parser that asserts it advanced, with fuel as the fallback): we prove it at generation instead —
a rule that can read nothing is a diagnostic, not a run-time crash, which is D25's side of the same
question and the better one for a generator. Neither is worth proposing; both were nearly proposed.

**1. Recovery sets computed from the grammar, instead of written on one repetition.** Today `recover`
is the author's: it marks a repetition and names the synchronization expression itself
(`recover eol`, §8.2), it exists only on repetitions, and nothing else in a grammar tolerates an
error — a failed parse yields no structure at all. Outside, `lelwel` is a resilient LL(1) generator
whose recovery sets are *computed*: the follow sets of the dominators of the graph the grammar
induces. We already compute both halves of the input to that — `Model/FirstSets.cs` and
`Model/FollowSets.cs` — and have no dominators. Matklad's resilient LL parsing tutorial is the
design that generator implements, and Pika parsing is the other road to the same place, packrat
reformulated so that error recovery falls out of the dynamic-programming table.
*Where it touches:* `Machine.Recovery.cs`, `Model/FollowSets.cs`, and §8.2, since it changes what a
grammar has to say. *The question a number answers:* of a file whose first error is at line N, how
much structure does the reader still return — today, zero past the failure unless the author marked
a repetition; the number to beat is the fraction of rules that keep their extent.

**2. Incremental re-parsing, which nothing here does.** `GramLanguageService.Analyze(grammarSource)`
takes a document and analyses it whole; the extension calls it from the classifier, the embedded-site
analysis and the diagnostics, and there is no word for "incremental", "reparse" or a reused tree
anywhere in `DotGram/Language` or `DotGram.VisualStudio`. The marketplace page answers the cost with
"responsive background analysis for large grammar files" — which is backgrounding, not incrementality.
`TransactSql.gram` is 7,264 lines. Outside, tree-sitter is the canonical answer, a sentential-form
incremental LR after Wagner and Graham, reusing subtrees across an edit.
*Where it touches:* `DotGram/Language`, and only the editor — a generated parser is not asked to be
incremental. *The question a number answers:* what one keystroke in the middle of a 7,000-line
grammar costs now, and what it would cost re-using the unedited subtrees. **Nobody has the first
number**, and it should be taken before any of this is designed, because backgrounding may already
have made it a non-question.

**3. A tree built where it is asked for, not everywhere.** Every construction a parse records is
materialized: the walk after the parse builds all of it, a guard's walk builds some of it early, and
`Locations are opt-in` is the only thing a consumer can decline. sql-39's anatomy puts the walk the
guards run at 8.4 µs of select20's 17.9 — the largest single item in that parse — and Q1 closed with
the lever being the walk rather than the record. Outside, two well-tried shapes: Roslyn's green and
red trees, where the persistent green nodes are the parse and a red node is created when someone
touches it, and Artio's flyweight FIX codecs, which decode a field only when its accessor is called
(Q9). Both are the same idea against our largest number, and the second is already in this file for
FIX.
*Where it touches:* `Machine.Direct.Values.cs` and the materializer, `docs/design/sql-ast.md`, and
the shape of what a `parse` hands back. *The question a number answers:* what fraction of the nodes
a real consumer touches. For `--roundtrip` it is all of them and this buys nothing; for a consumer
who asks a script for its statement kinds it is a few per statement. **That fraction is not known
here**, and it decides the whole item, which is why it is the number to take first and not the
design.

**Answer:** —

**Answer (architect, 2026-09-20, D33 `2457961a`), and a correction of item 3 by this session.**
The third goes forward and its number is ordered from the stand; the first is parked with its number
named; the second is parked behind the number this file said to take first. And a cost item 3 did
not name: deferring construction past the end of the parse means holding what it will be built
from — the log, and for a value cut from the text, the text.

Read at `6b847f2d`, the cost is real and it is narrower than it sounds, in three parts.

- **It is extent-valued captures that need the text, not the tree.** An extent on the tape is a pair,
  `Start` and `Length`, and `On(text)` slices the caller's text to read it (`Support.cs`, the extent
  support). A capture already turned into a value by a factory during the parse needs nothing. So
  the cost is "the nodes whose values are extents hold the text", and what share of a real tree that
  is nobody here knows.
- **D5 is about streams, and for a stream the objection is decisive.** Its binding is that no path
  reaches a contiguous form and that a streamed `yield` holds only the record being read; the
  buffered support copies (`Text(from, length)` is `new string(...)`) precisely because the window
  moves. Deferring past the parse there would hold buffers that D5 forbids growing — so for the
  streamed forms a tree handed back unbuilt is out, not to be weighed.
  For a `string` input the text is the caller's own object and holding it is a lifetime, not growth;
  Artio names the same hazard for its flyweight codecs, valid only while the buffer is unchanged.
  That is an API promise to write down, not a rule to break.
- **And item 3 conflated two mechanisms, which is the part worth correcting.** A lazy *value* needs
  the text. A lazy *node* — Roslyn's red over green — needs none, because the green side is already
  built; it saves the allocation of wrappers nobody touches. But our number is not allocation: the
  anatomy puts the factories at about a tenth of materialization and the walk's own machinery at the
  rest, so deferring the wrapper saves little, since the walk still has to run to know what is there.
  **What would cut the 8.4 µs is a lazy walk** — not walking a subtree until someone asks for it —
  and that needs an index from a record to the extent of its subtree in the log, which neither
  Roslyn nor Artio hands us. So the architect's limited form is not a smaller version of item 3; it
  is the only version of it that touches our number, and the outside gives it less than item 3
  implied.

The number the stand is taking — what fraction of the nodes a consumer touches — decides it either
way, and it is the same number for both forms.

**On the architect's summary — that a subtree's extent, expr's count from a mark and sql-39's share
of the fast path are one thing, "knowing about a stretch of the log without leafing through it"
(critic, 2026-09-20, read at `a4bbc682`).** Tested rather than agreed with, because a tidy
generalisation that suits three plans at once is the kind that goes unexamined.

It nearly went the other way. Every record carries a length: `End` writes
`Log[Opened] = LogCount - Opened` into the record's first slot, and the walk advances by exactly
that — `for (var at = from; at < ways.LogCount; at += log[at])`. Read those two lines alone and the
index this file said nobody hands us appears to have been in the log all along, and the right report
would have been "your consequence is a field you already have". **What says otherwise is that
`Opened` is one field and not a stack.** A record cannot be open while another is written, so a
construction is emitted as one straight-line block — `Begin`, a `Put` for each member, `End` — at
the point it completes, and a member that is itself built is *referred to* by a record number
(`PutRecord`), not contained. The log is flat and post-order. A record's length is its own, the walk
steps over siblings, and **a subtree's extent is nowhere written**.

So the summary holds in spirit and bundles three stretches that are not alike:

- expr's count from a mark is a **live** stretch: the mark is on the stack while the parse is in it.
- sql-39's fast path is a **predicate** over a stretch — are this record's children built — not a
  way to find its bounds.
- A subtree's extent is **derived and unrecorded**: in a post-order log it is "from the least record
  number below this one to this one", and nothing keeps that.

The objection, and it is small but worth making before anyone starts: bundled, the three invite the
reading I nearly published — that the log already knows a subtree's bounds, because every record
does carry a length and it is not the one wanted. If the summary is written, it should say that of
the three only the third needs something recorded that is not, and that what it needs is a bound,
not a length.

**Answer (architect, 2026-09-20, `e69cbcb1`).** The summary is corrected before anyone started, and
both lines were re-read independently in a fresh tree at `87fbb326`. It now says that of the three
only the lazy walk needs something recorded that is not, and that what it needs is a bound and not a
length — "from the least record number below this one to this one". The three are named as neighbours
in what they avoid rather than in what they need: expr's is a live stretch, sql-39's a predicate over
one, and only the third has to write something down.

What a proposal for the lazy walk will owe, when the stand's number allows one: what records the
bound, what it costs on writing and on unwinding, and what becomes of the completeness of the walk at
the end. One pointer for whoever takes it, so that it is not searched for twice: unwinding is
where the answer is likeliest to hurt, and the ground is already trodden — `carrier-per-construction`
§2 settles the same question for the held table, "the held count is still marked and unwound with the
records, since that is what the mark already covers". A bound is the same kind of thing and may
inherit the same answer, or may not, since a bound written at `End` is written after the stretch it
describes rather than before it.

*Not taken further here: the architect asked for the number first, and this file does not start work
it was told to wait on.*

## Q12 (2026-09-20). 0.2.0's readiness, checked by someone who did not build it

Asked by the architect: do the five packages promise what they do. Read at `87fbb326`; nothing was
built or run, and where the answer is "in order" it is said, because a readiness check that lists
only defects is not one.

**One class of defect, three places, all in Finance's shipped pages and all made by D27.** The
decision that a supplied length/data dictionary *adds to* the standard's sixteen pairs rather than
replacing them reached the code, the constructor's own documentation and the release notes. It did
not reach the two pages the package ships.

- **The first example of the feature does not run.** `README.md:109` and `SKILL.md:145` both open
  with `new FixFieldOptions(new Dictionary<int, int> { [95] = 96, [5000] = 5001 })`. `95 => 96` is
  one of the standard sixteen (`FixSchema.DataTag`), and the constructor now throws
  `ArgumentException` where "either tag is one the standard already defines" — its own XML comment
  says so. So both shipped pages open with a call that throws on the package they ship with. The
  release notes even predict this exact mistake in a reader: "a dictionary that repeated a standard
  pair to keep it is now refused with the tag named".
- **The SKILL states the reversed rule in so many words.** The line beside `[95] = 96` reads
  `// a supplied dictionary replaces the standard one`. That is the sentence D27 overturned,
  shipped in the file written for an agent to follow.
- **The README contradicts itself within forty lines.** `:78` — "`FixFieldOptions` configures only
  replacement Length/Data pairs" — against `:120` — "A supplied length/data dictionary **adds to**
  the standard's sixteen pairs … neither tag of a supplied pair may be one the standard defines".

Both examples are fixed by dropping `[95] = 96`, which is also the honest fix: the pair exists to
show a counterparty's own tags, and the standard pair in it was never the point.

**What was checked and is in order.**

- **Release notes.** All five packages carry `PackageReleaseNotes`. This session first concluded
  there were none — there is no changelog file — and found them by looking for the property instead,
  which is the second road this file now owes every count. The two breaks the architect named are
  both there with what to do: `FixField.Unknown` → `Custom` and the supplied dictionary, in
  Finance's; the trivia after a rule and the positional reading no longer refused by an unread
  character, in DotGram's. No third break was found unrecorded: Finance's withdrawal of the typed
  entry classes is stated in its notes, and the expression language's narrowing to `Parse`,
  `TryParse` and `Compile` is stated in its own.
- **What a package carries besides code.** `Directory.Build.props` gives every packable project the
  MIT licence expression, the repository and project URLs, authors, copyright and the icon; the
  icon is packed by a shared item. Nothing is missing package by package.
- **XML documentation.** No page of any package promises it. The expression language, Finance and
  Web generate it; `DotGram` and `DotGram.Sql` do not, and the SQL package says so in its own pages.
  Nothing promises what it does not carry.
- **Nobody else's text in a package.** The five pack `README.md`, `SKILL.md` and the icon, and
  Finance also packs our own `LICENSE`. The grammars are `AdditionalFiles`, which are compiler
  inputs and not packed, so `TransactSql/Specification/` and `Standard/Specification/` — Microsoft's
  published syntax and the ISO BNF — reach no package. No package references anything taken for the
  benchmarks: the only `PackageReference`s are `System.Memory` and, with `PrivateAssets="all"`,
  `Meziantou.Polyfill` and the Roslyn packages.
- **The specification followed the positional change.** §6.3 now says that a character no token
  begins with ends the tokens rather than refusing the reading "in all of these forms … a reading
  must not be refused by what it never read", with the script example. The page and the behaviour
  agree.

**One line outside the scope I was given, and a stop.** The ISO BNF and Microsoft's published syntax
sit in the repository itself, at `src/DotGram.Sql/*/Specification/`. They reach no package, which is
the question I was asked; whether they may sit in a public repository is a different one, and not
mine to answer.

**Answer:** —

**The process question, answered (critic, 2026-09-20, read at `c58eaeae`).** Asked: is there a place
where a package's promise can drift from its behaviour *between* the writing of the release notes
and their collection at packing, unnoticed. The answer is that there is no such place, because there
is nothing in between — and that is the finding, not a reassurance.

**The moments, as they actually are.** `PackageReleaseNotes` is a literal string in each `.csproj`,
written by whoever edits it. `README.md` and `SKILL.md` are packed verbatim by a `None … Pack=true`.
Between the edit and the pack there is no step that reads any of the three: no test opens them, the
package smoke does not look at them, and `dotnet pack` copies them. So nothing can drift between the
two moments, because nothing is checked at either. The pages that were wrong this morning were
wrong at the keystroke and would have been packed wrong; there was no window in which anyone could
have caught it, which is why the defect survived a day of people working on the same package.

**Two things would close it, and they close different halves.**

- **The half that bit us is behaviour, not signature.** D27 changed what a supplied dictionary
  *means* without changing a single signature. A public-API baseline — the Roslyn
  `PublicApiAnalyzers` with a tracked `PublicAPI.Shipped.txt`, which this repository does not have —
  would have caught `FixField.Unknown` → `Custom` at the keystroke, and would **not** have caught
  this one. Worth saying plainly, because a baseline is the obvious answer and adopting it would
  leave someone believing the class is closed.
- **What would have caught this one is running the pages.** Both defects are in fenced C# in a
  shipped file, and the first is a call that throws. Compiling and running the fenced examples of
  `README.md` and `SKILL.md` as an ordinary test is the forcing function that matches the defect:
  it fails at the keystroke, it needs no new discipline from anyone, and it is the only check that
  reads what the consumer reads.

**And one gap in the publishing path itself, which is the same question one step later.**
`build.yml` packs and then runs the package smoke — the packed package asked what it promises, under
the oldest Roslyn it supports, and the library smokes on both frameworks. `publish.yml` packs and
pushes to nuget.org **with no smoke step at all**. The artifacts that ship are not the artifacts that
were smoked; they are packed again in the workflow that publishes them. Whether that is a hole
depends on something this session cannot see — whether the repository requires `build.yml` green on
the commit a tag is placed on. If it does not, a tag on an unbuilt commit publishes packages nothing
has ever opened. *What would settle it: a branch rule requiring the build, or the smoke steps copied
into `publish.yml` between Pack and Push.*

**Two near-misses, recorded because they are the method working.** `publish.yml` builds
`--configuration Linux` and packs `--configuration Release --no-build`, which reads as packing what
was never built; `build.yml` explains it three lines up — the Linux *solution* configuration builds
every project as Release and writes to `bin/Release` — so it is right, and it is commented exactly
where it is surprising. And this session first reported that the packages have no release notes,
having looked for a changelog file rather than the property.

**Answer (finance-24, 2026-09-20, `91dc766c`), and the same question put to the other packages.** All
three are fixed, `[95] = 96` is gone from both examples with the true rule beside them, and — beyond
what was asked — the examples are now tests: the same four calls compiled and run, the refusal with
its tag named, and the assertion a reader is likeliest to doubt, that declaring a pair of one's own
does not cost the standard's. The diagnosis is finance-24's own and it is the right one: the
examples were prose, so nothing held them to the package, and D27 could reach the code, the
constructor's comment and the release notes without reaching them, because there was nothing to
reach.

Asked back: does the same gap sit in the other packages' pages. **Structurally, yes, and by the same
construction.** `DotGram.Web`'s two pages make 29 distinct calls in fenced C# and `DotGram.Sql`'s
make 7; the expression language's make 3. Nothing compiles any of them.

Whether any is wrong today, this session could not answer reliably, and the way it failed is the
argument. Two attempts at a text-based audit — pairing each `Type.Member` in the pages against the
public surface — returned uniform false negatives: the first said sixteen of sixteen were missing,
the second twenty-two of twenty-nine, including `AddrSpec.TryParseStrict`, which is four lines from
the top of `Rfc5322.cs` and is called by the stand. Both times the harness was broken, not the
pages. Hand-sampled instead: `AddrSpec.TryParseStrict`, `MediaRange.Quality`,
`ForwardedElement.ParseField`, `CookiePair.ParseField`, `TransactSqlParser.TryParseSelect` (the
grammar publishes `ParseSelect`) and the standard's two publications all exist. **The sample is
clean and the population is unchecked.**

So the proposal, with the evidence it now has: **an example in a shipped page exists as a test.** One
such example survived in two files until the first check made before a release, in a package whose
owner had edited those files the same week; and a reader's audit of the other 39 could not be made
reliable in two tries, while a compiler does it correctly every time and at the keystroke. The rule
costs one test per package, and finance-24 has written the first one, so the shape is known.

**Answer (architect, 2026-09-20, D37 `f6ce6662`).** The rule is taken — an example on a page a
package ships exists as a test that compiles and runs it — one per package, handed to the owners.
The publishing gap is closed by copying the smoke between packing and pushing rather than by a
branch rule, and for a better reason than this file gave: a branch rule proves that *some* packing
passed, not that this one did, since publishing packs again. The caveat about an API baseline is
recorded separately, so that adopting one does not close the class in anyone's mind.

**What must travel with those steps, read at `f6ce6662` so that the copy is not tried twice.**
`build.yml` has four things between Pack and the artifact upload, not one: the `check_library` block
that reads each packed `.nuspec` and fails where the newest target is not dependency-free; the
generator's smoke; an `actions/setup-dotnet@v4` pinned to `8.0.x`, **which is required**, since the
library smokes run `--framework net8.0` and `publish.yml` installs only one SDK; and the library
smoke with `NUGET_PACKAGES` pointed at a scratch directory. The feed is safe without anything else:
each smoke project carries its own `nuget.config` that clears the sources and maps `DotGram` and
`DotGram.*` to `../../artifacts`, so a floating `Version="*-*"` cannot quietly take a published
package from nuget.org — and the comment there says exactly that, which is the third time in this
audit that the surprising thing was commented where it surprises.

## Q13 (2026-09-20). The sweep's second three, and where the frontier actually is

Read at `f6ce6662`. The first thing to report is that most of the standard repertoire is already
here, which narrows what is worth proposing and is itself the answer to "what else could we use".

**Already ours, checked before proposing and not proposed.** A switch over kinds where the groups
are narrow enough (`Dispatchable`); a **group table** where they are not — kind to group, then a
switch on the group, within a span of 4,096 (`SpansAKindTable`, `KindTable`, `KindTableSize`), which
is the dense class-dispatch technique in full; a chain of first-character tests, narrowest first,
where one character decides; **column compression in the lexer**, 128 cells folded into as many as
the machine can tell apart — 47 for SQL-92 — which is the classic DFA table compression; and
precedence climbing for an operator tower (`graph.Climbing`). Four of the five things a reading of
the literature would suggest for recognition are in the tree already.

**1. A character class is a 256-byte array of noughts and ones, and half of it is never read.**
Measured on the checked-in snapshot `tests/Snapshots/Url.gram.g.cs`: nine tables named
`Recognize_DotGram_Class0`…`Class8`, each 256 bytes, **2,304 bytes for one small grammar** — and
every one of the nine is entirely zero above index 127, so 1,152 of those bytes are there to be
skipped. No two of the nine are identical, so deduplication is not the answer; the representation
is. The same information is two `ulong`s per class — 16 bytes, no array, no bounds check, no
indirection — with membership a shift and a mask, and the all-ASCII case is decidable at generation,
where a class naming a Unicode category keeps the table it needs. 9 × 256 becomes 9 × 16 for this
grammar. *Where it touches:* `LexerEmitter` and the recognizer's class emission. *The question a
number answers:* what those tables total across the real grammars and what share of a generated
assembly they are — `DotGram.CodeSize` weighs assemblies already, and `sql-code-size-2026-09-17`
counts the lexer's `Scan_Class` but not these. **This is a size claim and not a speed one**: a byte
load from an L1-hot table is one instruction, and whether the shift is faster is not obvious. The
saving to claim is bytes and cache, and anyone who takes it should say so in those words.

**2. The algorithm under D20's `IndexOfAnyExcept`, which is not the same as the API.** The reader's
gate found 58 places where a grammar's own seam rule — `Ows`, `Fws?`, `Blank` — is a greedy star
over a class, at the head of a turn and of a continuation. A star over a class is a state of the
automaton that loops on a wide set and leaves on its complement, which is exactly what a regular
expression engine's *prefilter* is for: where the machine would spin in one state, do not step it,
search for the first character that leaves. D20 names the API half; the half that is ours is
recognising the shape in the automaton and emitting the search instead of the loop, which is a
property of the machine and can be decided at generation, once, for all 58. *Where it touches:*
the lexer's emission of a star over a class, and D20's floor branch, which must keep its loop.
*The question a number answers:* what share of a parse is spent in star-over-class states. Not known
here, and the stand's families differ enough that it should be taken per family.

**3. Looked at and rejected, with the reason, because a rejected item saves the next reader the
walk.** Memoizing a rule's refusal at a position — packrat's table, in the small — against the
recognition anatomy's 1,381 refusals of 2,023 rule calls: the refusals are on the *first token*, so
each costs a call and a test, and a memo table costs a hash and a write. It would buy nothing here,
and it is the first thing the number tempts one to propose. Deduplicating the class tables above:
checked, no two of nine are equal. A public-API baseline for the packages: caught the rename that
already had its own answer, and would not have caught what actually went wrong (Q12).

**Answer:** —

**Item 1 corrected before it was handed over (critic, 2026-09-20, read at `f6ce6662`).** Igor asked
for it to go to the architect; reading the emitter first shrank it, and two things it said are
wrong.

- **Bit tables are not absent: they are the path for a class that reaches past 256.** `Machine.cs`
  emits a byte-per-character table (`TableSize = 256`) for a class that lies entirely below 256, and
  `Bits(ranges)` — a real bit table, windowed from `from & ~7` and capped at `KindTableSize` — for
  one that does not. Both representations exist and are written today.
- **Deduplication is already done, in both paths.** `_classesByRanges` and `_bitTables` key on the
  emitted text, so an identical table is never emitted twice. Q13 said "no two of nine are
  identical, so deduplication is not the answer"; the conclusion was right and the reason was wrong —
  they cannot be identical, because duplicates are collapsed at emission.

**What is left, and it is the real item.** The choice between the two representations is made by
**reach and never by cost**: a class entirely below 256 always gets the eight-times-larger form, and
`Bits` is reached only when a class is too wide for it, not when it would be smaller. For the ASCII
case that is nine tables of 256 bytes in `Url.gram.g.cs` — 2,304 bytes — where the same nine as bit
tables are 288, and as a pair of `ulong` constants each, with no array and no bounds check, 144. The
byte table is presumably the deliberate fast path, one load against a shift and a mask; nothing in
the tree says so, and nothing has measured it.

So the item to hand over is one question, not a design: **is the threshold in the right place?** It
is a size-against-speed trade between two paths that both already exist, the size half has a harness
(`DotGram.CodeSize`) and no measurement, and the speed half is a byte load against a shift — which
is why this file will not guess it. The number to take first is what these tables total in the
packages' generated code, which needs a build and so is not this session's.

## Q14 (2026-09-20). Optimisations by target framework: how many buckets, and what holds them equal

Igor asked for the options sketched. Read at `f6ce6662`. Our own libraries target
`netstandard2.0;net10.0`; the generator is netstandard2.0; and the emitted code is held to
netstandard2.0, net472 and net8.0 by `DotGram.Compatibility`, at the C# 8 floor. What follows is
about the *emitted* code, which is compiled in the consumer's project and so sees the consumer's
framework, not ours.

**The buckets worth having, and the one that is not.**

- **The floor — netstandard2.0, net472, C# 8.** Unchanged byte for byte, which D20 already requires.
  It has `Span` through the `System.Memory` package we already reference, and `IndexOf` of one to
  three characters. Everything below is measured against it.
- **net8.0, where nearly all of the value is.** `SearchValues<char>` and `SearchValues<byte>`, which
  retire our own rule that a stop set of more than five characters is read one character at a time;
  `IndexOfAnyExcept`, which is the mechanism under Q13's second item — the star over a class that
  the automaton spins in, at 58 seam places; `FrozenDictionary`/`FrozenSet` for a grammar over
  tokens, where SQL has 410 words; and the `Ascii` helpers for comparisons that ignore case.
- **A later bucket — net9.0 or net10.0 — for recognising a keyword from a span without making a
  string**, through an alternate lookup keyed on `ReadOnlySpan<char>`, and for searching many
  strings at once. **The exact boundary between 9 and 10 is what this session would not swear to**:
  the sources disagree about which of them carried `SearchValues<string>`, and it decides which
  bucket the keyword work lands in. To be read off the API reference before anything is built, not
  off a blog and not off this file.
- **Not a bucket: netstandard2.1.** net472 does not reach it, and a consumer on 2.1 alone is rare
  enough that the branch would double the matrix for almost nobody.

**The shape this should take, which is the part worth arguing about.** Emit **capability switches,
not framework switches** — `#if DOTGRAM_HAS_SEARCHVALUES` rather than `#if NET8_0_OR_GREATER` — with
one place mapping a framework to its capabilities. Three reasons, in the order they bite: a framework
test scattered across five emission sites multiplies by the number of buckets, while a capability
name stays one word wherever it is written; the consumer's framework set is not ours to predict, and
a package that multi-targets gets every branch compiled whether we thought about it or not; and D10
already establishes the shape for this — unsafe code and skipping locals' initialisation are the
consumer's option rather than our default, so a consumer who wants the floor on a new framework can
have it, which a raw `NET8_0_OR_GREATER` cannot express.

**Two things must exist before the first branch lands, and neither does.**

- **Nothing asserts the branches answer alike.** `DotGram.Compatibility` builds the emitted code for
  netstandard2.0, net472 and net8.0, and building *is* its assertion — it proves the code compiles on
  three frameworks and says nothing about what it reads. The moment a branch changes how a stop set
  is searched, the thing to hold is that every bucket returns the same values, the same refusals and
  the same positions. The material is all here — the refusal record, the corpora, the snapshot
  baseline — and what is missing is running it per bucket rather than per commit.
- **The stand cannot pair two branches.** D20 asks for a pair comparing the two branches on one
  platform rather than two commits; the stand pairs two *builds of two commits*. Building one grammar
  twice with different capabilities, in one run, is a stand feature that does not exist, and the
  measurement D20 requires cannot be taken until it does.

**And the order I would put them in**, since each bucket costs a branch to write, to test and to
pair: `SearchValues` for stop sets first, because it retires a rule of our own and the places are
already counted; the star-over-class search second, because Q13 names where it lands; frozen tables
third, because they pay only for a grammar over tokens and SQL is the one that has them; and the
ASCII helpers last, being the smallest. The keyword-from-a-span work waits on the version question
above.

**Answer:** —

**Answer (architect, 2026-09-20, D39 `fa731475`), and it is the better question.** Not a threshold
between the two existing forms but a third that may beat both: a class lying wholly below 128 is two
`ulong` constants — a comparison, a shift and a mask, with no memory read and no array at all — so
there may be no size-against-speed trade to argue about. Size first, from the packages' generated
code rather than the snapshots, with `DotGram.CodeSize` pointed at a question it has never been
pointed at; then speed, as a pair, on the Url and Web rows. With performance-ff.

**One detail for whoever writes it, read at `fa731475`.** The guard is already there and the third
form does not add one. `TableTest` emits `c <= 255 && {name}[c] != 0`, and the snapshot shows it as
`if (c <= 255 && Recognize_DotGram_Class0[c] != 0) goto S185;`. An all-ASCII class becomes
`c <= 127 && …`, which is the same single comparison and a *tighter* one, and what follows it is a
shift and a mask instead of an indexed load whose bounds check the JIT must eliminate from the
comparison and the array's length. So the third form is one comparison, as today, minus a memory
access — which is why it may win on both counts rather than trading one for the other.

**Answer (architect, 2026-09-20, D40 `6350e1b5` and `d2e10448`).** Taken as written — three buckets,
capability names rather than framework tests, the `SearchValues<string>` version read off the API
reference rather than guessed, and both conditions kept as conditions. The second of them is also
answered rather than merely accepted: `--stand-paired` already loads two directories of assemblies
into two isolated contexts of one process and holds their answers equal before timing anything, so
it does not care what makes the two directories differ. The pair a branch must carry is two builds
of one commit differing by a property of the generator — about an hour on the stand's side — and the
cheap look, netstandard against net10 of one commit, is declined because those two builds differ by
more than the branch.

## Q15 (2026-09-20). The first branch has already landed, twice, and neither condition was asked of it

Read at `d2e10448`.

**The claim.** "And two conditions before the first branch lands, because neither exists … A branch
that is faster and reads differently is worse than no branch, and a branch whose speed nobody can
measure is a guess, so both come before the first `#if`." (D40.)

**What it rests on.** That per-framework emission has not started.

**The objection: it started on 2026-09-09, and the emitter writes two framework tests today.**

- `BufferedEmitter`, in `MoveLine`, writes `#if NET8_0_OR_GREATER` → `MemoryExtensions.Count(span,
  '\n')`, `#else` a loop over `IndexOf`. Counting the newlines a buffered feed has gone past is where
  a reported position's line number comes from. Added yesterday, `64235e43`.
- `Machine.Reader`, in `EnoughStack_DotGram…`, writes `#if NETCOREAPP2_0_OR_GREATER ||
  NETSTANDARD2_1_OR_GREATER` → `TryEnsureSufficientExecutionStack()`, `#else` the older pair inside a
  `try`/`catch`. The remark the emitter writes beside it states the difference in so many words: the
  floor pays "an exception on the one probe in sixty-four that finds the margin gone". Added
  `8ec21fe3`, 2026-09-09.

Those are the only two — `OR_GREATER` across `src/DotGram` — and they are exactly the shape D40
rules out, a raw framework test at the emission site. So the capability rule does not arrive at zero
sites. It arrives with two to convert, and a decision that binds the next branch should say whether
these are converted or grandfathered, because the reason given for capability names — a framework
test multiplies by the buckets while a capability name stays one word — applies to them first.

**What holds them today, which is the opposite of what one would assume.** `EmittedCode.Compile`,
the in-memory level of the test suite, parses emitted source with
`CSharpParseOptions.Default.WithLanguageVersion(CSharp8)` and **defines no preprocessor symbols** —
nothing in the repository calls `WithPreprocessorSymbols` — while its `References` are the test
process's own loaded assemblies, which are net10.0's. So every test at that level compiles the
**floor** branch and runs it on net10: the refusal sample, `Tests.Slow`'s whole refusal record,
`BufferedInputTests`, the carrier shapes, and the twenty-odd other files that call it. The
analyzer-attached level — this project's own `Url` parser, the shipped packages, the benchmarks and
the stand — compiles the capable branch. Both branches are therefore already run, by different
levels of the same suite, and no test, comment or document says so; a reader of `EmittedCode` has no
way to know which side of the `#if` their test is about.

Compilation is covered, and by `DotGram.Compatibility` rather than by luck: `Consumer.cs` has
grammars with `BufferedInput = true` and grammars that recurse, so both sides of both branches are
compiled on their frameworks. The snapshots are not: `tests/Snapshots/Minimal.gram.g.cs` carries the
stack branch, and no snapshot emits `MoveLine` at all, so a change to the newline branch shows up in
no diff.

**What follows for D40's first condition, and it is cheaper than the wording suggests.** "Run the
corpora per bucket rather than per commit" reads as multi-targeting the test projects and running
them three times. It need not be: the material is already compiled in one process against one
reference set, and what is missing is the argument that selects the branch. One
`.WithPreprocessorSymbols(…)` on the parse options, the corpus taken twice, and the two answers held
equal — in the suite that exists, on the runtime that is already there. What that does *not* cover
is a branch that needs an API the floor lacks; there the capable side will not compile against a
netstandard reference set, but the check wanted here is the other direction, and the net10 reference
set compiles both.

**And the measurement switch can be made unreachable rather than merely undocumented.** D40's
amendment (`97cefaa4`) settles that it is a measurement switch and not a consumer's option, for a
reason this session has nothing to add to: on a platform that has the capability the fast branch
should always be right, and where it is not, that is our defect and not their setting. There is a
mechanism that makes that true instead of stating it, and one hazard before it.

The hazard first. `Build-Side.ps1` (`5c60d6ee`) passes the switch as an MSBuild property —
`-Property DotGramSearchValues=off`, its own example. A property reaches a source generator only as
`build_property.…` in the analyzer config, and only where something declares
`<CompilerVisibleProperty Include="…" />`. Two are declared today, `DotGramReportGeneration` and
`DesignTimeBuild`, and until a third joins them the example is a no-op: both sides build the same
emitted code, the pair reads as "the branch costs nothing", and nothing in the run says why. A side
builder that can silently build the wrong side deserves a guard — the switch's own name in
`build.txt` is not enough, since that is what it was *asked* for, not what the generator read. The
generation report (`DotGramReportGeneration`, which this repository turns on for every project) is
where the generator can be made to say which branch it wrote.

Then the mechanism. Both declarations live in `src/DotGram/build/DotGram.targets`, which is the
package's own build asset, and the repository's projects read that same file because
`Directory.Build.targets` imports it wherever a project references the generator as an analyzer. So
a third declaration written where the two are ships. Written in `Directory.Build.props` instead, the
pair works exactly as before and a consumer's build cannot set the switch at all — the property is
not visible to their compiler, so setting it does nothing. That turns "supported for taking a pair
and unsupported as a way to run" from a sentence in a document into a fact of the build, and it
costs one line in a different file.

**What would settle it.** Name the two existing branches in D40 — converted or grandfathered — say
which side of the `#if` the in-memory level compiles, in `EmittedCode` where the answer lives, and
declare the measurement switch where the package cannot carry it. None of the three costs a
measurement.

**Answer:** —

## Q16 (2026-09-20). One question is counted three ways, and the platform at the floor already answers two of them

The architect's, after Q15: read the two existing framework branches as content rather than form —
are there places where the emitted code writes a loop and the platform **on the floor** also does
better, a gain that needs no branch at all? Read at `3638853f`.

**The whole of the character-at-a-time scanning in emitted code.** Taken from the five checked-in
snapshots and from the templates they come out of, and it is a short list:

- `Window.LineAt` and `Window.ColumnAt` (`Support.cs`, `WindowClass`) — `_buffer[at]`, twice;
- `Located_DotGram.Move` (`Support.cs`, `LocateHelper`) — `text[_at]` going forward, `text[at]` going
  back, `text[_start - 1]` walking to the line's beginning;
- `Reach_DotGram` (`DirectSupport`) — `text[pos + at]`, how far a literal run agreed;
- `…_Agreeing` (the recognizer) — `text[p + i]`, the same question with case folding.

Nothing else reads input a character at a time. Everything else already calls the platform:
`SequenceEqual` and `MemoryExtensions.Equals` with `OrdinalIgnoreCase` for a literal, `IndexOf` and
`IndexOfAny` for a stop set, `Array.Copy`, `Array.Resize`, `Array.Clear`. The repertoire is mostly
taken, as Q13 found for recognition.

**And the first two are the same question, counted three ways in one tree.**

- **The fixed one.** `LocatedMembers` (`BufferedEmitter`), the buffered window: it keeps `_lineAt`,
  `_lines` and `_lineStart`, and moves them with `LastIndexOf` and a count. Its own comment says why
  — "Searched for, not read a character at a time: a feed asking for lines had a branch on every
  character it read counted again." This is the code Q15 is about, the `#if NET8_0_OR_GREATER` one.
- **The incremental one.** `Located_DotGram`, the whole-text path: it keeps the last place asked
  about, so each question costs what lies between it and the one before — its remark argues the case
  in full, "a parse that asks about every bad line of a feed pays for the feed once, where counting
  from the start paid for it once a line" — and then it counts that distance a character at a time.
- **The one that got neither.** `Window.LineAt` and `Window.ColumnAt`, which streaming uses: a
  per-character loop **from the window's start on every call**. Not incremental, not searched for.
  It is exactly the algorithm `Located_DotGram`'s remark was written against, in the one form where
  the input is longest.

**Where it is reached from, and what that bounds.** `StreamingEmitter` maps the host variables
`parserLine` and `parserColumn` to `window.LineAt(from)` and `window.ColumnAt(from)`;
`Machine.Recovery` reports a recovered item's line and column the same way. So the cost is paid by a
grammar that asks — and a feed that asks per record pays the window once per record, which is a
shape and not a constant. A grammar that never asks pays nothing at run time, and pays anyway in
code: `CSharpEmitter` writes `WindowClass` whenever a grammar streams, with no gate, two lines below
the buffered classes, which are gated on `Locating(graph)`. The asymmetry is visible in the three
lines that emit them.

**What the floor has.** `MemoryExtensions.IndexOf` and `LastIndexOf` are on netstandard2.0 through
`System.Memory`, which emitted code already requires for the span itself — the same argument
`Machine.cs` makes where it decided `MemoryExtensions.Equals` with `OrdinalIgnoreCase` was the right
call there. No capability, no bucket, no `#if`.

**So, ranked.**

1. **Give the streaming window what the buffered one already has.** Both halves are written twice
   over in the tree — the incremental state in `Located_DotGram`, the searched-for move in
   `LocatedMembers` — and the streaming window has neither. *Where it touches:* `WindowClass` in
   `Support.cs`, and the `Locating` gate two lines above it in `CSharpEmitter` if the members are to
   stop being emitted where nothing calls them. *The question a number answers:* what a streaming
   parse that asks for a line per record costs now against the same parse over a buffered feed,
   which has the fixed one — a comparison available today, without writing anything.
2. **Search backwards as well as forwards.** Both incremental implementations vectorized the forward
   direction and left the backward one a character at a time: `MoveLine`'s `for (var at = _lineAt -
   1; at >= position; at--)` and its walk to `_lineStart`, and `Located_DotGram.Move`'s two backward
   loops. `LastIndexOf` is the same floor API. *The question a number answers:* how often a parse
   asks about a place behind the one before — unknown here, and cheap to count.
3. **The pooled link reset, and it is the weakest of the three.** `DirectValues.Return` clears its
   value tables with `Array.Clear`, but resets the link chains with a loop —
   `_linkHeads[i] = -1; _linkNexts[i] = -1` over `_valuesUsed`. `Span<int>.Fill(-1)` is on the floor
   where `Array.Fill` is not. The caution belongs with it:
   [`value-store-clearing-2026-09-20.md`](value-store-clearing-2026-09-20.md) measured that what
   `Return` pays is calls and cold tables rather than bytes, so a vectorized fill of the same region
   may buy very little. Unmeasured, and named only because it is the one clearing there that is
   still a loop.

**Rejected, with the reason, because a rejected item saves the next reader the walk.**
`Reach_DotGram` and `…_Agreeing` are the two remaining per-character scans and neither belongs here:
they answer "how far did this literal agree", the floor has no common-prefix API at all, and
`CommonPrefixLength` is net8 — so they are an item for Q14's net8 bucket, and the case-folded one
has no API on any framework. A star over a class needs `IndexOfAnyExcept`, which is also not on the
floor; that is D20's, and Q13 named where it lands.

**And one thing this makes visible about the record.** No checked-in snapshot emits `MoveLine` at
all, while four of the five carry the naive pair. The fixed implementation — the one with the
framework branch — appears in no diff anybody reads, and the unfixed one appears in four.

**Answer:** —

**Answer (architect, 2026-09-20, D41 `5b883f1f`).** Item 1 approved to design, with the size defect
beside it and a snapshot covering the line-moving code, all in one commit; items 2 and 3 parked in
the order given; the refusal-path walks refused with the reasons as stated. To performance-ff.

**The count was wrong, and it is mine to correct: five places, not four (critic, read at
`5b883f1f`).** A second count, taken off the emitter's templates rather than off the snapshots,
found one the snapshots cannot show — no checked-in snapshot emits a byte machine at all.

`BufferedBytes.Matches(int position, string literal)`, written by `BufferedEmitter` into the byte
window, is a per-byte loop: `for (var i = 0; i < literal.Length; i++) if (_buffer[position - _start
+ i] != literal[i]) return false;`. Six emission sites call it — a literal, a literal in a run, a
shared prefix and the residue after one, in `Machine.cs` and `Machine.Reader.cs` — and every one of
them is the place where the character path calls `SequenceEqual` or `MemoryExtensions.Equals`
instead. So it is not a scan the platform cannot do; it is the one comparison in emitted code where
the decision `Machine.cs` took for characters was not taken for bytes.

**And the argument for taking it is `Machine.cs`'s own**, written where it chose the span for
characters: `SequenceEqual` against a constant is folded by the JIT into word-sized compares —
"abcd" becomes a single 64-bit `cmp` — and it is bounds-checked once, where the chain it replaces
was checked once per character. Nothing in that reasoning is about `char` rather than `byte`; four
bytes are one 32-bit compare on the same ground.

**What it would take, and the two things that make it sound.** The literal is known at generation,
so it is emitted as `static ReadOnlySpan<byte> … => new byte[] { … };` — an RVA constant since C#
7.3, no allocation, under the C# 8 floor — and `Matches` takes a span and calls
`MemoryExtensions.SequenceEqual` after its own `Ensure`, which is where the buffer can still move.
`ByteRefusal` already guarantees the conversion: a byte machine refuses a case-insensitive literal
and any character above 255, in so many words — "byte literals must be case-sensitive values in
0..255" — so every literal reaching `Matches` is a byte string, and the comment at
`Machine.Reader.cs` that states this as an assumption is in fact enforced. I checked that before
writing this, because the same code read the other way would have been a correctness defect: a
case-insensitive literal over bytes comparing exactly.

**Where it would pay, said honestly.** The byte path is FIX's, and FIX's literals are short — the
4.4 field grammar is hundreds of tags spelled `"956="`, two to five bytes, and much of the dispatch
goes through a shared prefix so what reaches `Matches` is the residue, often one or two bytes. At
that length a loop and a call are close, and this is not the streaming window's shape argument; it
is a constant-factor claim that has to be paired like one. What makes it worth raising anyway is
that it is the only comparison left where two renderings of one grammar disagree about a decision
already taken. *Where it touches:* `BufferedEmitter`'s `Matches`, the six call sites, and a byte
literal's emission. *The question a number answers:* the Fix44 and byte rows of the stand, paired —
and a snapshot with a byte machine in it, which does not exist either.

**And the shape claim in item 1 was wrong, which makes the item better (critic, read at
`ca5cb587`). Mine to correct, and it moves the cost from the question to the drop.** I wrote that a
feed asking per record "pays the window per record — a shape, not a constant", and D41 took that
wording. It is not a shape. `Window.Extend` drops what lies behind the parse's position before
reading more, and on the way out it counts the newlines it is dropping into `_lines`, so
`LineAt(position)` walks the *live window* and not the input. A window is bounded by the largest
element that cannot be dropped, so a feed of ordinary records pays a bounded walk per question — a
constant factor, and a small one. The quadratic that the wording describes existed and was fixed:
`StockCountScalingTests` holds a count with a rejection every tenth line linear in its length, from
a string and from a reader both, and its remark names the defect — "That number used to be counted
from the start of the input for each of them".

**What is actually wrong there is one line above it, and it is worth more than what I claimed.**
The counting on the way out is a per-character loop — `for (var at = 0; at < from; at++) if
(_buffer[at] == '\n')` — over everything the window drops. Each character of the input is dropped
once, so a streaming parse reads every character a second time, with a branch, to count line
terminators. **And it is not gated on anything.** `_lines` and `_break` are read only by `LineAt`
and `ColumnAt`; a grammar that never asks for a line pays the whole scan anyway, because
`CSharpEmitter` writes `WindowClass` for any streaming grammar with no `Locating` gate.

The buffered window decides both halves of this the other way, and says so in its own words. Its
release counts through `MoveLine` — `LastIndexOf`, and a count rather than a walk — and `Located`
strips the counting entirely where nothing locates: "Written only where a recovery asks for a line
or a column: every other buffer releases without counting anything." So the two classes disagree
twice about the same two decisions, which is the ranking rule this file used on Q16: not a technique
to borrow, an answer already given and not carried across.

**So item 1, restated, and it is two things.** For a grammar that does not locate, the drop's
counting should not be emitted at all — the gate D41 already takes as a size defect is a *time*
defect first, one branch for every character of every streaming parse that never asks a question.
For a grammar that does locate, the drop's count is the floor's `IndexOf`, as the buffered window
already does it. `LineAt` and `ColumnAt` themselves are worth fixing after those two and for a
different reason — an element too large to drop makes the window large — and not for the one I gave.

*The question a number answers, corrected with it:* not a feed asking per record against the same
feed buffered, but a streaming parse **that asks nothing at all** against the same parse with the
drop's counting removed. That is the one every streaming row of the stand would show, and the stock
rows are where it is already read.

## Q17 (2026-09-20). What the snapshots do not emit, and therefore do not hold

The architect's, after two misses in one evening — the line-moving code and then a whole byte
machine, both invisible to a count taken off the snapshots. A sample whose coverage nobody knows is
not a sample of anything. Read at `ca5cb587`, counted off the emitter's templates and then tested
against the five checked-in snapshots by a marker each, so that every row below can be re-counted.

**First, why the set is shaped as it is, because it is not an oversight in any one grammar.**
`SnapshotTests` compiles each `.gram` with `GramCompilerOptions` carrying four things — the class
name, the namespace, the Roslyn C# scanner and a line map — and every other option at its default.
So no snapshot can exercise an option-driven path at all: not `Lexical`, not `LocationType`, not
`PartSize`, not `SourceFileSize`, not `Portable`, and nothing from `[GramOptions]`, which the
snapshot path does not use — `BufferedInput`, `BufferedBytes`, `SpanCaptures`, `Carrier`,
`MaxRetained`, `BufferSize`, `Stacks`, `Suffix`. And `Assert.Single(result.Sources)` makes it
structural rather than incidental: a grammar that compiles into more than one file cannot be a
snapshot as the harness is written.

**Ranked as Q16 was: at the top, where two renderings of one grammar disagree, or where a decision
already taken was not carried across.**

1. **The buffered and byte input half, entire.** `class BufferedText`, `class BufferedBytes`,
   `ReadBuffered_…`, `IParserInputSource`, `DefaultMaxRetained` and `DefaultBufferSize`,
   `ArrayPool`, `System.IO.Stream` and `ReadOnlyMemory<byte>` overloads: zero in all five. *What no
   diff checks:* a reader whose whole job is holding and releasing input under a retention bound —
   D7's read-in-place, the pooling, the compaction — and the two places this file has already found
   in it, `MoveLine`'s framework branch and `Matches`'s per-byte loop. Character streaming through
   a window *is* covered (`TextReader` overloads are in four of five); it is the buffered and byte
   forms that are not.
2. **A second reading of one grammar.** `[GramOptions(Suffix = …)]` — `class Immediate`, `class
   State`: zero. *What no diff checks:* how a second compilation of one grammar is scoped, named
   and reached, which is what the expression language ships and what the stand times on every
   paired run.
3. **The lexical half.** `Lexical` is never set, so `Tokens_DotGram`, the scanner the seam asks
   for and the whole machine over kinds: zero. *What no diff checks:* one of the three ways the
   generator writes a way in, and the one with the largest measured effect on SQL.
4. **Prefix tables.** `PrefixTables` defaults to on, but no snapshot grammar triggers one:
   `Prefix_DotGram…`, zero. *What no diff checks:* a table built at generation to choose between
   literal alternatives — built, named and indexed, with no file showing it.
5. **Locations as an option.** `LocationType` is never set. `Located_DotGram` appears once, in
   `Minimal`, because a recovery there asks for `parserLine`. *What no diff checks:* the per-rule
   range offering a grammar opts into, which is a shipped feature with a measured price.
6. **A carrier the author asked for.** No snapshot names one, so every carrier in the set is what
   `Auto` chose. *What no diff checks:* the refusal path — a carrier asked for and not given, and
   the parser the tape writes instead, which is where GRAM5007 and GRAM5012 are said.
7. **The symbol resolver.** The snapshots wire the Roslyn *scanner* and leave the resolver
   permissive. *What no diff checks:* what `@Name` compiles into when a real resolver answers,
   which is the seam the Web package exercises and the reason it exists.
8. **Several files out of one grammar.** `SourceFileSize` is never set and the harness asserts one
   source. *What no diff checks:* how a divided file names and reaches what is in its other halves.
9. **`Portable`, `Stacks`, `PartSize` at anything but their defaults**, each changing emission and
   each with no file to read. Part *division* itself is covered — `…_Part…` is in three of five —
   so this is the sizes, not the mechanism.

**What this does not say.** None of these is untested: the buffered reader has its own tests, the
lexical half has a package, locations have a measurement. The claim is narrower and is the one the
architect asked for — no *diff* shows them, so a change to what they emit is reviewed by nobody,
and a count taken off the snapshots reads as zero where the truth is "not emitted here". That is
the shape of both of this evening's misses, and it is why the answer to "how many places does X" is
wrong by construction when X is taken off this set.

**Answer:** —

**Answer (architect, 2026-09-20, D42 `53712a93`).** Taken with the order as given, and the harness
changed before any grammar: a snapshot may carry its own options, and a grammar that compiles into
several files may be a snapshot. Then files for the first three rows — a few of them, not a matrix,
since a set that takes minutes stops being read. The qualification is kept as written: none of the
nine is untested, and the claim is only that no diff shows them.

## Q18 (2026-09-20). A public parser carries its whole grammar into the assembly, and nothing has weighed it

Proposed rather than assigned, and it comes out of Q17: if the snapshot set is one configuration,
the first thing worth asking is where that configuration differs from the one a *consumer* gets.
Here is one, and it is the whole grammar. Read at `53712a93`.

**The claim.** "`Portable` | follows the class's visibility | whether the grammar's text travels on
the class, for an include across a project reference (§6.7)." (`docs/syntax.md`, the `[Gram]`
table.) `GramGenerator` settles it as the attribute's own value, else an inherited one, else
`Visible(type)` — public all the way out. `CSharpEmitter` then writes
`[global::DotGram.GramSourceAttribute("…")]` onto the last part of the class, the argument being the
grammar's entire text.

**What it rests on.** That a project downstream will include this grammar and needs the text to do
it. Within one project it is never needed: the `.gram` file is an additional file and the generator
reads it, taking a carried text only as the `else if`.

**The objection is the size, and who is paying it.**

- **`DotGram.Sql` publishes three public parsers over file-referenced grammars.**
  `TransactSql.gram` is 441,215 bytes, `SqlStandard.gram` 223,698, `SqlStandard92.gram` 32,871 —
  **697,784 bytes of grammar text**, carried into the shipped assembly as custom-attribute blobs,
  because `SqlStandardParser`, `TransactSqlParser` and `Sql92Parser` are public.
- **`DotGram.ExpressionLanguage` carries its grammar twice.** The host is public and the grammar is
  inline, so the text is in metadata once as the `[Gram]` argument the source wrote and once more as
  `[GramSource]` — a block of about 67 KB in the source either way.
- **`DotGram.Web` pays nothing, and by the shape of its design rather than by a decision.** Its
  grammar classes are internal — `static partial class Rfc3339` — so `Visible` is false and no text
  travels.
- **`DotGram.Finance` is the one package that says `Portable = false`**, and it is the one whose
  grammar is smallest and already in metadata as its own `[Gram]` argument. So the decision exists,
  and it was taken where it saved a duplicate of a small grammar and not taken where it adds seven
  hundred kilobytes of a large one. That is this file's ranking rule again: a decision taken once
  and not carried across.

**What is not known, and it is the whole of what would settle this.** What share of the shipped
assembly that is. The *source* figures exist —
[`sql-code-size-2026-09-17.json`](../../benchmarks/results/sql-code-size-2026-09-17.json) has
`TransactSqlParser.g.cs` at 22,026,182 bytes, against which 441 KB of grammar is two per cent — but
source bytes and assembly bytes are different questions: the code compiles down by a large factor
and the string does not compile down at all, so the share in the DLL is necessarily larger and
may be much larger. `DotGram.CodeSize` weighs assemblies and has never been pointed at this.

**The question a number answers.** The byte share of `GramSourceAttribute` in `DotGram.Sql.dll` and
in `DotGram.ExpressionLanguage.dll`. One run, no pair, no window — it is a size question.

**And a check that costs nothing and should come first.** Does anything include another project's
grammar through the class? In this repository, no: `TransactSqlParser` includes `Sql92Parser`, and
they are in one project, where the file is read and the carried text is not consulted. So the
default is paying, in every public host, for a capability offered to consumers and used by nobody
here — which may be exactly right for a library, and is worth saying out loud with the number
beside it rather than left as a guess about a downstream project.

**Why no snapshot shows it.** `SnapshotTests` leaves `Portable` at the `GramCompilerOptions`
default, which is `false`, while a consumer's default is their host's visibility. So the emission
every consumer gets differs from the emission every snapshot holds by an attribute carrying the
whole grammar — Q17's point in one concrete instance, and the reason this was worth looking for.

**Answer:** —

## Q19 (2026-09-20). Where the grammar's text is written, counted off the templates, and what it does not account for

The architect's, narrow: how many times and onto what classes does the emitter write the grammar's
text when `Portable` holds, and why does `DotGram.Sql` measure 226,376 bytes more than the three
grammar files add up to? Read at `441eb8ca`. Nothing here is measured; it is counted off the
emitter and off the three hosts.

**The emitter writes it once per compilation, and the code says why.** `CSharpEmitter`, walking the
host's class path: `if (i == classParts.Length - 1 && grammarSource is not null && suffix is not {
Length: > 0 })` — the innermost class part only, and never for a suffixed reading, with the reason
written beside it: "Only on the compilation that owns the class: a `Suffix` puts a second reading in
a nested class, and what an including grammar names is the class, not the reading." There is one
other emission of a whole grammar's text — `LanguageDescriptorAttribute`, two lines below — and it
is written only where `LanguageId` is set, which is a `.gram` DSL host and not a parser.
`SourceFileSize` is never set by the generator, so no second file repeats the class header.

**What is carried is the spliced text, not the file.** `GramGenerator` joins the host's own grammar
with every `[GramInclude]`'d one through `GrammarSplice.Join`, each wrapped in `namespace <name> {
… }`, and compiles *that*; `GramCompiler` then passes `options.Portable ? grammarText : null` — the
joined text — to the emitter. That is by design and the design is stated: across a project
reference the included grammar may not be reachable, so the text that travels has to be the whole
of what was compiled.

**So the accounted arithmetic for `DotGram.Sql`, by host.**

| host | carries | bytes |
| --- | --- | --- |
| `SqlStandardParser` | `SqlStandard.gram` | 223,698 |
| `Sql92Parser` | `SqlStandard92.gram` | 32,871 |
| `TransactSqlParser` | `TransactSql.gram` + `Sql92Parser`'s spliced in | 441,215 + 32,871 + ~20 |
| `TransactSqlParser.Located` | nothing — suffixed | 0 |
| | | **730,655** |

**Which answers the architect's distinction, and only half of the number.** The excess over "one
text per grammar file" is **32,871 and it is not a defect**: the standard's 1992 text rides inside
T-SQL's because T-SQL is compiled from the two joined. That is the price of the design and it
should be named in the figure rather than left to surprise. But it accounts for 32,871 of 226,376.
**The remaining ~193,500 bytes are not explained by the emitter**, and the templates say plainly
that they cannot be: there is one attribute per compilation, three compilations carry a text, and
the fourth carries none.

**So the number needs a second count by another road, and the first one's road is the problem.**
924,160, 1,651,200 and 19,811,840 are all multiples of 512, which is a PE file's alignment: the
figure is a difference of two builds' file sizes, and such a difference attributes to the thing
that was changed everything else that moved with it. What would settle it is a count of the
`GramSourceAttribute` blobs in the built `DotGram.Sql.dll` — how many rows, and the length of each
— which is a metadata read and not a build: `ildasm`, `System.Reflection.Metadata`, or
`GetCustomAttributesData` on the three types. Three rows of 223,698, 32,871 and 474,086 say the
emitter is right and the residue is in the measurement. A fourth row, or one of them twice the
expected length, says the defect is real and is somewhere these templates do not show — and that
would be the third time this week that what the snapshots cannot emit is where the answer was.

**One thing worth saying beside it, since a share of the assembly is about to be argued about.**
`TransactSqlParser` carries `[GramOptions(LocationType = typeof(ISqlSpan), Suffix = "Located")]` —
a second whole compilation of a 441 KB grammar, in a class of its own, with its own recognizers and
materializers. It carries no grammar text, and it is a large part of what `DotGram.Sql.dll`
weighs. Any statement of the form "the grammar's text is N% of the assembly" is measured against a
denominator that holds T-SQL twice, which is the design's choice — the comment above it prices it,
"locations cost fourteen per cent of the parse and nothing in memory" — but it belongs in the
sentence with the percentage.

**Answer:** —

**Answer (architect, 2026-09-20, D43 `a37f4a3d`).** Taken as counted: one attribute per
compilation, the spliced text by design and its 32,871 named in the figure rather than left to
surprise, the remaining ~193,500 not something the emitter can produce, and therefore the
measurement is the suspect — a count of the attribute rows ordered from the stand. The
denominator's caveat goes with the percentage wherever it is quoted.

## Q20 (2026-09-20). Two records hold a refusal, and between them nobody holds what a buffered parse says

Proposed. Read at `a37f4a3d`.

**The claim.** "A publication whose string form is read by methods is read by methods over a buffer
too, **and answers as the string form does** however the input arrives."
(`BufferedInputTests.A_buffered_form_is_read_by_the_reader_the_string_form_is_read_by`, and the
byte form beside it.)

**What it rests on, and it is a strong check.** For every input and **every split point of it** —
`ShortReader` hands the reader one character at a time, then two, and so on — the buffered answer
is held to the string answer on `IsSuccess`, on `Position`, and on `Value` where it succeeded. Read
at `a37f4a3d`, `Read` in `BufferedInputTests.Input.cs` fetches exactly those three off the `Match`.

**The objection is the fourth property.** `Match` also carries `Error`, the message, and nothing
compares it. The other record — `RefusalCorpus` — is explicitly about that message: "the outcome,
the position and **the whole message, which carries every set expected there and every set tied
with it**". It compiles each shape in every rendering, under each carrier, split and unsplit, and
calls `TryParseStart(string)`. So the two records divide the ground like this, and leave a strip
between them:

- the **message** is held for string input, across renderings, carriers and the lexical split;
- the **outcome, position and value** are held for buffered and byte input, across every split;
- **what a buffered or byte parse says when it refuses is held by neither.**

**Why that strip is where a difference would live rather than anywhere else.** The buffered path
has failure paths written for it — `failure.Starved`, `failure.OutOfInput = p + 1`, `RefusedRun` —
and over bytes a literal is compared by `Matches`, which answers whether it matched and not how far
it agreed, where the character path has `Reach_DotGram` and the recognizer's `…_Agreeing` for
exactly that. "How far it got" is therefore computed by different code on the two sides, and the
message is where how far it got is spoken. Position is held, so a divergence there would be caught;
the wording of the expectation is not.

**What it would take.** `Read` already reads three properties off the `Match` and `RefusalCorpus`
reads this one. A fourth field and one assertion, in a test that already runs every split of every
input. **And it is worth doing whichever way it comes out**: equal, and the strip is closed for one
line; not equal, and we have learned that a refusal reads differently depending on how its input
arrived — which is a thing to decide deliberately rather than to find out from somebody's bug
report.

**And it is owed sooner than it looks.** D40's first condition is that the branches of a
per-framework emission answer alike, and the material named for it is the refusal record. The one
framework branch that exists today, `MoveLine`, is inside the buffered reader — which the refusal
record does not reach.

**Answer:** —

## Q21 (2026-09-20). The option's second ride, and what every shipped assembly carries besides its code

The architect's, after the count confirmed the emission to eighteen bytes: what else does `Portable`
pull along, since turning it off shrank the file by 193,487 bytes more than the attributes hold?
Read at `a37f4a3d`.

**In the emitter, nothing.** `grammarSource` reaches exactly one line — the `[GramSourceAttribute]`
of Q19 — and `Portable` is read nowhere else in `Grammar/Emit`. Its other uses are all at
generation and none of them changes what is written: `GramGenerator` decides the flag from the
attribute, an inherited one, or `Visible(type)`, and reads a *referenced* assembly's carried text
when an included grammar's file cannot be found. So the answer to the question as put is that the
option pulls nothing else along **in the emitted code**. The 193,487 bytes are somewhere else, and
they are.

**The explanation is beside the code, as it was three times yesterday.** `DotGram.Sql.csproj`:
"Symbols inside the assembly, as the generator carries them: a stack trace from a parser here has
line numbers without a symbol server." — `<DebugType>embedded</DebugType>`, which every one of the
five packages sets, and `Directory.Build.props` sets `<EmbedUntrackedSources>true</EmbedUntrackedSources>`
beside its SourceLink block. So each shipped assembly carries its own symbols, and the symbols
carry source that a debugger cannot find on disk — which is what generated code is.

**Which means the grammar rides twice.** Once as the attribute blob, raw UTF-8, 730,673 bytes by
the count. And once inside the embedded symbols, as the *escaped C# literal* that
`CSharpEmitter.Quoted` wrote — `"` to `\"`, `\` to `\`, `\r` and `\n` to two characters each — as
part of the generated source. Embedded source is deflated, and grammar text deflates the way
repetitive BNF does: **730,673 × 26.5% is 193,600, against the 193,487 that was missing.** The two
numbers were never in conflict; they answer different questions. The count answers "how many bytes
of grammar text are in the metadata", and the subtraction answers "how many bytes does the file
lose if the option is turned off" — which is the question Igor is actually being asked, so **the
figure to quote him is the subtraction one**, 4.7% and 5.3%, not 3.7% and 4.1%.

**And the consequence is larger than the option.** If the symbols carry the generated source, they
carry *all* of it: `sql-code-size-2026-09-17.json` puts `TransactSqlParser.g.cs` at 22,026,182
bytes and `SqlStandardParser.g.cs` at 13,791,317, with `Sql92Parser.g.cs` at 780,755 — **36.6
megabytes of generated C#**, compressed, inside a 19.8-megabyte assembly. At the same deflate ratio
that is seven to nine megabytes, which would be a third to a half of `DotGram.Sql.dll` and would
explain its size better than its IL does. Every size this repository has taken of a shipped
assembly includes it, `DotGram.CodeSize` among them.

**The check that settles both, and it is a read rather than a build.** The debug directory of
`DotGram.Sql.dll` says how large the embedded PDB is, and the PDB's document table says which
sources it holds and how long each one is: `System.Reflection.Metadata` opens both. Two numbers
come out — the embedded symbols' share of the assembly, and the grammar text's share of the
symbols — and they close this question and correct every assembly figure taken so far. Building
once with `DebugType` set to nothing would answer the first alone, and is the cruder of the two.

**Said plainly, because it is the part worth acting on.** Nothing here is wrong: symbols in the
assembly are a decision with a reason written beside it, and a stack trace through generated code
without a symbol server is worth paying for. What is wrong is that no figure this repository quotes
says it is being paid. A percentage whose denominator holds a compressed copy of the whole
generated source is not the percentage anybody thinks they are reading.

**Answer:** —

## Q22 (2026-09-20). What the size instrument weighs, now that we know what is in the file

Following the trail from Q21, and narrower than the worry it came from. Read at `be243fbd`.

**The instrument is better than the fear.** `DotGram.CodeSize` reports two things per assembly and
not one: `FileBytes`, the whole file, and `ILBytes`, the sum of every method body it walks the
metadata for. So the symbols are in the first and cannot be in the second, and any statement made
from `ILBytes` is untouched by Q21. It also measures the generated *source* twice — `Bytes` and
`NormalizedBytes`, the second with the repository root replaced by `/_` — so the source figure was
deliberately made independent of where the checkout sits.

**What needs saying is which of the two a sentence is made from.** A change that removes S bytes of
generated source removes from the file its own IL **and about a quarter of S again**, through the
embedded source. The source instrument and the assembly instrument therefore overlap by design:
they are not two independent readings of one change, and a report that quotes both as if they were
is counting part of the same bytes twice.

**And the path rule now has its mechanism.** "Compare sizes only within one worktree" has been the
rule because `#line` directives carry the grammar's absolute path. The tool normalizes that away
for the source figure and cannot for `FileBytes`, because the text inside the symbols is the
unnormalized one. The scale: the two large SQL grammars hold 1,463 and 1,196 actions, so a few
thousand `#line` directives each carrying a path of sixty to eighty characters — hundreds of
kilobytes in the source, much less in the file since a repeated path deflates almost to nothing,
and zero in `ILBytes`. Nothing in the tool's output says which of its numbers moved for this
reason.

**The architect's distinction, checked against the properties, and it holds.** Two separate
switches are in play. `DebugType=embedded` puts the symbols in the assembly, and that alone is what
buys the reason written beside it — "a stack trace from a parser here has line numbers without a
symbol server". Embedding the *source text* is `EmbedUntrackedSources`, set once in
`Directory.Build.props`, and what it buys is stepping through code the debugger cannot find on
disk. Handwritten files are tracked and reached through SourceLink, so they were never embedded;
the untracked files are the generated ones. **So the lever is exact**: turning that one property
off removes the generated source from every shipped assembly and keeps every line number in every
stack trace.

**And it is two orders of magnitude larger than the thing being decided.** If the count of the
symbols confirms Q21's estimate, it is seven to nine megabytes of a 19.8-megabyte assembly, against
the 4.7% that `Portable` costs. Whoever is choosing about the grammar's text on a figure of 4.7%
has not been shown the larger number on the same page, and the two are answers to the same
question — what a consumer downloads.

**What this is not.** Not a proposal to turn it off: the numbers come first, the decision is
Igor's, and stepping through a generated parser is worth something to whoever is debugging one. It
is a request that the lever and its price be named beside the percentage, and that every size
sentence from here on say whether it was made from the file or from the IL.

**Answer:** —

**Counted, and it corrects a number of mine (critic, read at `3334ca1e`).** The symbols were read
rather than estimated, and the estimate in Q21 — "seven to nine megabytes" — was wrong by three and
a half times. The count: `DotGram.Sql.dll`'s embedded symbol file is 3,544,414 bytes in the file
(17.9% of it) and 7,208,884 unpacked; of its 29 documents 13 carry source, all of them generated,
holding 39,044,259 bytes of text stored as **2,839,013 — 14.3% of the assembly**. And the residue
closes exactly: the grammar texts alone deflate to 191,805, and 730,673 + 191,805 + 1,682 = 924,160,
the difference of the two builds to the byte, the 1,682 being what escaping the text into a C#
literal costs. The expression language agrees to 83 bytes.

**The mistake is worth naming because this file has spent two days on its family.** I took the
deflate ratio measured on *grammar text* — 730,673 raw against 193,487 compressed, about 3.8 to 1 —
and applied it to *generated C#*, which is a different material and compresses 13.75 to 1. That is
the same move as counting off something that resembles the answer: a ratio is a measurement of one
thing, and carrying it to another is a guess wearing a measurement's clothes. The right ratio was
one read away, in the same file that answered the question.

**What changes in Q22 and what does not.** The conclusion about the instrument stands: `ILBytes` is
untouched, statements made from it hold, and the overlap between the source figure and the file
figure still has to be named. The lever stands and is exactly as described —
`EmbedUntrackedSources`, one property, whose removal takes the generated source out of every
shipped assembly and leaves every line number in every stack trace. What was wrong was the scale.
It is not two orders of magnitude above the decision being taken; it is **14.3% against 4.7%** —
three times, not a hundred. Still worth standing on the same page as the `Portable` figure, because
both answer the one question a consumer asks, and no longer worth the word "dwarfs".

**And one thing the count found that is not about size at all.** The symbol documents carry the
absolute path of the machine that built them. That is a question about what a shipped package says
of the disk it was made on, not about bytes, and it belongs with whoever owns reproducible packing.

**Answer (architect, 2026-09-20, D46 `be243fbd`, counted at `3334ca1e`, corrected at `cff09063`).**
Q21 and Q22 taken as they stand: `ILBytes` untouched and statements from it in force, the overlap
between the source figure and the file figure named wherever both are quoted, and the lever —
`EmbedUntrackedSources`, one property, line numbers kept — on the same page as the `Portable`
percentage, both answering what a consumer downloads. The scale is threefold and not a
hundredfold. Both figures go to Igor together, with both levers and the price of each.

**And the sentence the three days were for, which is the architect's and is recorded there.** The
two measurements never disagreed. **The questions disagreed.** The count answered how many bytes of
grammar text are in the metadata; the subtraction answered how much the file loses with the option
off. Each was right about its own question for as long as it had been quoted, and the 193,487 was
the distance between two questions rather than an error in either.

**Q9 corrected twice more (2026-09-20), and the second one is mine.** finance-24 assembled both
readings and *ran* them, and two things this file carries turned out wrong.

- **The version trap is wrong, and wrong in the way it was written to prevent.** Q9 says "the
  package names lost a full stop at 1.14, so Core is 1.14.1 while the message packages are still
  1.13.0". `QuickFIXn.FIX44` has no 1.13.0: *checked here independently against the package page* —
  it exists at 1.14.0 and 1.14.1 only, and 1.13.0 belongs to the **old** id, `QuickFIXn.FIX4.4`,
  which is exactly what the rename separated. finance-24 saw a request for 1.13.0 resolve silently
  to 1.14.0 with `NU1603` while restoring. So a trap written against a silent version substitution
  induced one. The mechanism is the one this file has now recorded four times: the figure was taken
  off a listing that showed the old-named package, which resembles the answer and is not it.
  **Both message packages are 1.14.1 beside Core's 1.14.1**, and the trap is now only the rename.
- **`validate: true` does more than the order of three header fields.** The correction this file
  took on 2026-09-19 said it checks that and nothing else; it also calls the message's own
  `Validate()`, which checks `BodyLength` and `CheckSum`. Validation *against the dictionary* is
  still the separate static `DataDictionary.Validate`, so the three readings remain three and the
  pairing stands — but the first framing in Q9, that the third reading "does more than we do", was
  closer to true than the correction that replaced it: it does more on the **frame** and not on the
  **schema**.

**And the method, which is a refinement of this file's own rule.** Reading the source says what a
flag is *tested against*; running says what it then *calls*. `if (validate) { Validate(); }` is
plain once you are looking at it, and neither of us was, because we had each read the line the flag
appears on. Reading is necessary and it is not sufficient — finance-24 found it from a refusal
naming `Expected BodyLength=119, Received BodyLength=118` with `Message.Validate()` in the stack,
and only then went back to the file. This file will keep insisting on the source over a summary, and
will stop treating a read as the end of the question where running is available to whoever owns the
work.

**What this does not change.** The licence reading, the pairing against `FixMessages`, the three
readings and D26 all stand. And one thing improved: `QuickFIXn.FIX44` carries
`DataDictionary/FIX44.xml` itself, so the dictionary arrives with the reference and no file of
theirs enters the repository — the question this file raised about their text is answered by the
package.
