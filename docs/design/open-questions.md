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
and no more than two or three at a time.

## Q7 (2026-09-19). D25's audit: two verdicts, one test, and a hazard under the first candidate

Asked by the architect: walk the sources against D25 — the generator decides, the parser is a dumb
machine — and judge two candidates on their merits rather than on his reading of them.

**The test the audit was run with**, offered because it is the part that outlives this sweep. Hold
the input and the grammar fixed and take the mechanism away. Three outcomes, and only the third is
what D25 forbids:

- **Housekeeping.** The steps are the same and only allocation differs: the spare parsers and the
  `_deeper` stacks, `DeeperSpares = 3`, the retention cap, `ValueTable`'s flat prefix and pages,
  the 1 MB ceiling on what is worth recycling. None of these can change an answer or a step. The
  file `Support.Adaptive.cs` is named for capacity and not for strategy; it grows a table, it does
  not choose one.
- **Memoization.** The answer is identical and some steps are elided, by a rule fixed in the emitted
  code. Admissible, but only while the rule is fixed at generation *and* its cost is bounded.
- **Strategy.** Which steps are taken depends on something that is neither the input nor the
  grammar. Out.

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

**Candidate 2, the pools and the weak reference, is housekeeping — with the part I cannot audit
named.** Every pool in the emitted support passes the test above: take the spares away and the same
steps run over freshly allocated arrays. The only weak reference on main today is the one in
candidate 1, so if performance-ff has retention work in flight it is not yet in the tree and I have
not audited it. The test to put to it when it lands: does removing it change any step, or only the
allocation? If the retention limit is ever derived from what the parser has seen — a pool that grows
because inputs have been large — that is the third category and it is out; a limit that is a
constant, or one the consumer declares (D9), is the first.

**And one place where the rule and the code disagree today.** `FixGrammar.cs:62` and `:68` ask a
consumer's `FixFieldOptions` while reading, whether a tag carries a length-and-data pair. D25 names
exactly this and rules it out; the remedy decided with it — the consumer declares its tags where the
grammar is, and the generator builds the arms — has not landed. Until it does, the rule is written
and the code does otherwise. Worth knowing in that order rather than the other one.

**Answer:** —
