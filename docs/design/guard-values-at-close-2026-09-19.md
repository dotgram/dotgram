# A guard's value at the capture's close: a design, refused by its own numbers

**Refused on 2026-09-20, in both of the shapes it ended with, and kept as the record of why.**
sql-39 counted, on an instrumented parser and without a machine, why a guard's ask does not take
the walk's fast path:

| | SQL:2023, `select20`, 301 asks | T-SQL, 7,716 statements, 17,023 asks |
| --- | --- | --- |
| the fast path | 170 (56%) | 1,322 (7.8%) |
| the root is not the last record | 107 (36%) | 1,110 (6.5%) |
| something below it is unbuilt | 24 (8%) | **13,300 (78%)** |
| a list of roots | 0 | 1,291 (7.6%) |
| asks that list the log | 131 | 15,701 (92%) |

What that says. **The count (§3.1)** answers "is everything since the mark built?" in constant
time — and on T-SQL the answer is *no* in 78% of asks. A cheap "no" is still a "no": the walk
happens anyway, and the listing, the clearing and the reaching pass all remain. What is saved is
the `IndexOf`, which is 6% of a walk. **And the close (§2) does not help either**, which I had not
seen until the causes were counted: "something below is unbuilt" is a record *no guard asked for*,
and by §3.7 a close builds only what a guard names, so that record stays unbuilt and the fast path
still fails. Only building everything where the reading is proved — the wider
`carrier-per-construction` design — reaches that cause.

On SQL:2023 the fast path already works, and what blocks the rest is the other cause, which
neither shape touches: the root is not the last record.

So this is refused where it would have paid least and cannot pay where the cost is.

**And the listing, counted next, says where to aim instead.** A slow walk lists 12.3 records on
`select20` and 14.9 on the T-SQL corpus, and builds 1.8 and 7.4 of them — so it lists 6.9 records
per record built on SQL:2023 and 2.0 on T-SQL. There are no tens of records listed for one built,
and what redundancy there is lives on one family only, which is the case sql-39 warned about: a
change that removes one grammar's cause does nothing on the other.

The part worth keeping from that count is sql-39's caveat rather than the count. The listing loop
is the cheapest thing in a walk — one store per element — and on the same tally of elements run the
clearing of `Live` from the mark and the reaching pass backwards over the same list. So 234,089 is
not the price of listing; it is the scale of what three things touch. **Aim at the walk not
starting, not at it listing faster** — which is either building earlier, where the reading is
proved (the wider design), or asking less often.

Asking less often has one shape worth counting before anything else: a guard with several captured
values materializes each of them with a call of its own from the same place — three in a row is
visible in the generated SQL:2023. If those are many, the asks are fewer than they look and the
walks are not, and merging the asks of one guard into one walk takes work away without promising
anything about §3.7. **A pair on anything here takes both families** — the two grammars fail the
fast path for opposite reasons, so a measurement on one is a measurement about one.

What follows is the design as it stood, unchanged, because the reasoning in it is what the numbers
are answering.

---

A `when` that names a captured value asks the tape for it while the text is still being read.
The tape answers by walking: `Materialize(record, sinceMark)` from the rule's mark, which lists
the records the root reaches, builds them, and marks them built. The walk is linear in what it
builds (584a7c1f, 312d5b98), and a tower of guards takes the fast path that builds the record
where it stands. What is left is the case the fast path refuses — a record that is not the last
one, marks over the walk, a stray that was never captured — and there the guard pays for a pass
over the log from the mark on every ask.

This asks whether the value can be handed to the guard at the moment the capture closes, so that
the guard reads a slot and no walk runs at all. No code until the answer and its numbers are
agreed.

## 1. What the specification allows, and what it does not

§3.7 is the whole constraint: a construction "is deferred until recognition has selected the
accepted derivation, unless a later `when` on that same path explicitly asks for the computed
value", and "an alternative later abandoned by backtracking does not invoke an unrequested
construction". The tape is what a host asks for when a construction counts, logs or throws.

So "build it at the close" is not free to do everywhere. At the close of a capture the reading
that made it may still be given up — the alternative may be abandoned, a repetition may turn, a
caller may retry — and the factory would then have run for a derivation that was never accepted.
That is exactly the promise the tape is chosen for.

It is free to do where the reading cannot be given up: where the record stands past a proved
point (`Commit`, fix-reader §7), which is the same proof the immediate carrier is chosen by. The
guard itself narrows this further and helpfully: a guard that reads the value is a reason the
construction runs *anyway* — §3.7 says so in as many words. What must be proved is only that this
reading of it is the accepted one.

**The set this design starts with**: a capture whose record is closed at or past a point of its
rule, whose children are all built, and which stands under no node of **the watch list**
(`no-way-back-2026-09-19.md` §6, sql-39's, cited and not copied). That is the shape the fast path
already recognizes at walk time (`root == ways.Records - 1` and nothing unbuilt since `first`),
read back one step earlier — at the close, where it is known without looking at the log at all.

### 1.1 A reading that is proved and thrown away anyway

The point proves that this reading will not be given up and replaced. It says nothing about a
reading that is *read in order to be discarded*, which is what a lookahead is: `?!` succeeds by
throwing away what it read, and `?=` throws it away too and keeps only the answer. A `Commit`
inside a lookahead's body holds within that body and is true — and the derivation under it is
never the accepted one. So the proof does not answer this, and the exclusion cannot come from it:
**lookahead bodies are out by construction**, both ways and backwards, which is exactly what the
watch list's second and third rows say.

Every row of that list is needed here, and for the reason sql-39 gives: two of its rows stand out
of caution for their question, because there a reading ends where another reading ended and the
text and the position are the same. Here the close of a capture is a *moment* rather than a
position — the value is built at it and cannot be unbuilt — so a node whose answer depends on
anything but the position is a node this must not build under. The narrower list sql-39 offers is
not the one to take, and this design does not name a list of its own.

What a guard still gets under a lookahead is what it gets today: it asks, and `Materialize` walks.
Nothing there is made worse.

## 2. What changes

At the close of such a capture the reader calls the record's arm directly, writes the value into
its slot and sets `built[record]`. The guard then reads `ValueOf(rule, record)` — a table lookup
— and `Materialize` is not called. The walk at the end of the parse finds the record built and
skips it, as it does today for what a guard built.

Three consequences, all of which have to be paid for or refused:

- **The arms must be reachable from the close.** Today an arm is a case of the walk's switch, or
  a local function of it (`CallDirectArm`), and a local function of the walk cannot be called from
  the reader. **Two shapes, and I propose the second.**

  *The arms become methods of their own.* The reader then calls exactly the arm it just closed,
  with no switch at all, because at the close the rule and the factory are known where the code is
  written. The price is file size — a signature and a prologue an arm did not have, on every arm
  of the grammar — and, because `text` may be a span, a frame they are methods of rather than free
  statics with six parameters each. This is the shape the condition named.

  *The close enters the walk, which already has the arms.* A second entry — `record` is the last,
  complete, all its children built, and proved — that does the little the walk's fast path does
  without what that path spends proving it: no clearing from the watermark, and no `IndexOf` over
  the flags from the rule's mark, which is the pass this design exists to remove. What is left is
  `values.Room`, one flag, the switch and the return. The arms stay where they are, so **the file
  grows by one parameter and one branch and by nothing per rule**, and the whole size question the
  first shape raises does not arise. What is given up against the first shape is one switch on the
  kind per close, which is a jump table.

  I propose the second and would measure the first only if the second's numbers disappoint, since
  the second is the one that can be refused cheaply. **The size is still the first number
  reported** — T-SQL, SQL:2023 and the expression language, before and against — but under the
  second shape I expect it to be near zero, and that expectation is itself the check: a growth per
  rule would mean the arms were duplicated after all.
- **A record built at the close and then given up** must be undone, or must never happen. The
  proof above is what makes it never happen; where the proof does not hold, the close does not
  build and the guard walks as today. There is no third answer: clearing a built flag on a rewind
  is a pass over the log, which is the cost this removes.
- **What the walk clears is unchanged.** `built` is cleared from `ways.Built` on, and a record
  built at a close moves that watermark exactly as a record built for a guard does.

### 2.1 A capture inside a repetition, turn by turn

A capture in a repetition closes once a turn, and the question is what the guard reads on the
second turn. What this owes is not a sentence but a test, because a defect of exactly this shape —
a guard reading the last turn's value rather than its own — was here and closed itself, which is
worse than having been fixed.

What is intended: each turn writes its own record, and the value built at a close goes to the slot
of the record just closed (`ways.Last`), which is the one the guard on that turn names. A turn that
is given up puts its record back with the log, and the `built` flags above `ways.Built` are cleared
by the watermark the walk already keeps — so a re-read turn builds again rather than reading the
value of the turn that was given up. A repetition that turns after a proved point is what makes
this safe to do at all; a repetition without one keeps today's walk.

The test: at least three turns with values that differ, a guard on each turn that refuses unless it
is handed *its own* turn's value, and a turn that is read, given up and read again with another
value. It must fail before the change and pass after, and a guard that can see only the last turn
must not be able to pass it.

## 3. What has to be measured, and what I expect

Stated before the pair, so that the pair can say no, and in the order the architect set: the size
first and without a window, the factory counts second and without a window, and the stand only
when both have passed.

- **The emitted size, first.** T-SQL, SQL:2023 and the expression language, before and against,
  counted in one worktree and saying whether the BOM is in the figure (a `#line` carries the
  grammar's absolute path, so sizes compare only within a tree). If the growth is not small, this
  stops here and is reported, and no window is asked for.
- **Factory calls.** A count of factory calls over the SQL corpus and the expression language's,
  on accepted input and on refused input, against the tape as it is today. My expectation:
  **identical on accepted input**, and identical on refused input too — because the set in §1 is
  exactly the set where the reading cannot be given up. A difference on refused input is a defect
  in the proof, not a cost to weigh. This is the check that this design does not sell §3.7.
- **The corpora and the snapshots**, unchanged but for the arms' new shape.
- **The stand**, on `sql/*`, `el/*` and the script rows, and — the rule since 584a7c1f, because
  this is a change to the walk — **a size-growing row on every guarded family and not on SQL
  alone**. The expression language's eightfold loss on the tape came from a change to the walk and
  showed itself only on input that grew. Linearity is read at the end of the window, over the same
  rows. My expectation: the SQL rows move a little or not at all, because T-SQL's guards are mostly
  the tower the fast path already serves; the rows that move are the ones sql-39's anatomy found
  paying for the listing pass; the growing rows do not bend.

If the SQL rows do not move, this design is refused on its own evidence, whatever the file size
turns out to be. If a growing row bends, it is refused whatever the flat rows say.

## 3.1 A third shape, which may make all of this unnecessary

Found after the second shape was accepted, and put before any code was written.

The walk's fast path already builds the record where it stands. What is expensive in it is not the
building but what it spends proving its own right to build: `IndexOf` over the `built` flags from
the rule's mark to the root — the very pass this design exists to remove. That question — "is every
record since the mark built?" — can be answered in constant time.

**Not by a watermark, which was my first word for it and is wrong.** There is no contiguous run of
built records to keep the end of: the records below the guard's rule's mark belong to the rules
above it, they are legitimately unbuilt, and a watermark counted from zero would answer "no" for
every ask. What is wanted is relative to the mark.

**By a count.** `Ways` keeps how many records are written and not yet built. The mark saves it, as
it already saves the log's count, the record count and the side stack (`UnwindRecords`), and the
ask compares: *every record since the mark is built exactly when the count is what it was at the
mark*. Writing a record raises it by one, building one lowers it by one, and a rewind puts back
the number the checkpoint saved — which is what makes it free of a scan on the path where the log
is put back, the path a watermark would have had to scan on. A grammar whose guards build nothing
emits none of it, as it emits none of `ways.Built` today.

What that buys, and it is the whole argument: **the factories run exactly when they run today** —
only where a guard asked. §3.7 is not touched at any point. No watch list, no lookahead bodies to
exclude, no turn of a repetition to hold a test against, no point to prove. The change is a field,
a line where a record is built, a line where the log is put back, and a comparison in place of a
scan. The architect's rule on it, recorded as D29: where two designs answer the same measurement,
the one that does not touch what the specification promises wins even on equal numbers, because
the other's proof is rent and not a price.

**What it does not cover.** Beside the `IndexOf` a guard's ask pays two more linear things:
`values.Room(…)`, which clears `Live` from `from`, and `Array.Clear(built, ways.Built, …)`. If the
cost is spread over all three, making one of them constant does not save it, and building at the
close — the second shape — is still the answer, with this as a cheap part of it. So the breakdown
of sql-39's anatomy into those four terms decides which design survives, and it is asked for
before any code.

**The condition, whichever shape carries it (the architect's, and hard).** The count is a cache of
a property that is computed honestly today. An off-by-one answers "all built" where not all are,
and the building is then skipped in silence — no refusal and no exception, which is the shape of
the eviction defect. So it owes a test that holds the count against the honest scan over the
corpora, not over three cases, and over input that is given up and re-read, since the rewind is
where it will be wrong if it is wrong.

**How it is checked**: a second emission, chosen by the generator beside what already turns the
reports on, off by default, which emits the honest scan next to the comparison at every ask and
fails where the two disagree. It rides on the corpora that are run anyway, so there is no separate
run for anyone to forget.

**And a boundary on that, which is a condition of its own.** The check never reaches a consumer's
build, under any circumstance — in particular it is never put under the consumer's `DEBUG`. Their
debug build would then pay the linear scan they came here to be rid of, without asking for it or
being told, and a disagreement would surface as their program failing in code they did not write.
The switch is ours and lives on the generator's side. Where the second emission turns out to be
expensive, what narrows is *what* is compared — the rewinds alone, where a mistake would be — and
not where the checking runs.

## 4. Where it meets the other design

`carrier-per-construction-2026-09-19.md` proposes deciding per construction, by the same proof,
whether a value is built in place or taped. This is that decision, taken for one construction —
the one a guard names — and taken at the close rather than at the rule's end. If the wider design
lands first, this is a case of it and not a design of its own; if this lands first, it is the
narrow set the wider one widens. They must not be built twice, and whichever is written second
reads the first.

## 5. What is not proposed

Building at the close for a construction **no** guard names. That is the wider design's business,
it pays nothing here, and it is where §3.7's promise is easiest to lose sight of.
