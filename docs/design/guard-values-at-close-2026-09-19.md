# A guard's value at the capture's close: a design (for review)

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
rule, and whose children are all built. That is the shape the fast path already recognizes at
walk time (`root == ways.Records - 1` and nothing unbuilt since `first`), read back one step
earlier — at the close, where it is known without looking at the log at all.

## 2. What changes

At the close of such a capture the reader calls the record's arm directly, writes the value into
its slot and sets `built[record]`. The guard then reads `ValueOf(rule, record)` — a table lookup
— and `Materialize` is not called. The walk at the end of the parse finds the record built and
skips it, as it does today for what a guard built.

Three consequences, all of which have to be paid for or refused:

- **The arms must be callable from the reader.** Today an arm is a case of the walk's switch, or
  a local function of it (`CallDirectArm`). Calling one at a capture's close means they become
  static methods taking what they read. That is the architect's condition, and its price is file
  size: a static method carries its own signature and prologue where a switch case carried
  neither. **To report before anything else: the emitted size of T-SQL, SQL:2023 and the
  expression language, before and against.** If the growth is not small, this design stops here
  and the walk stays.
- **A record built at the close and then given up** must be undone, or must never happen. The
  proof above is what makes it never happen; where the proof does not hold, the close does not
  build and the guard walks as today. There is no third answer: clearing a built flag on a rewind
  is a pass over the log, which is the cost this removes.
- **What the walk clears is unchanged.** `built` is cleared from `ways.Built` on, and a record
  built at a close moves that watermark exactly as a record built for a guard does.

## 3. What has to be measured, and what I expect

Stated before the pair, so that the pair can say no.

- **Factory calls.** A count of factory calls over the SQL corpus and the expression language's,
  on accepted input and on refused input, against the tape as it is today. My expectation:
  **identical on accepted input**, and identical on refused input too — because the set in §1 is
  exactly the set where the reading cannot be given up. A difference on refused input is a defect
  in the proof, not a cost to weigh. This is the check that this design does not sell §3.7.
- **The corpora and the snapshots**, unchanged but for the arms' new shape.
- **The stand**, on `sql/*`, `el/*` and the script rows. My expectation: the SQL rows move a
  little or not at all, because T-SQL's guards are mostly the tower the fast path already serves;
  the rows that move are the ones sql-39's anatomy found paying for the listing pass. I do not
  expect the script rows to move at all.

If the SQL rows do not move, this design is refused on its own evidence, whatever the file size
turns out to be.

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
