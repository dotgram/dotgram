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
