# A run followed by the padding its delimiter begins with (proposal, 2026-09-29)

Status: a proposal for review. Nothing here is built.

## The shape

    Separator = ' '* & '|' & ' '*
    Text      = (?!Separator & any)+
    Item      = Key & '=' & Text & ' '* & ('|' & ' '* | eof)

A run up to a padded delimiter, then padding of its own, then a terminator or the end of the
input. "A statement, trailing spaces, then `;` or the end" is the same shape. The run's scan
(`Machine.Delimiter.cs`, and in the reader `EmitScan`) is not used for it: what follows the run
begins with a space, which the run's turns also read, so nothing proves the run need never give
back, and it is read as it was written.

## Measured

Counted under `DOTGRAM_COUNTS` (buffer asks), the grammar above with `Items = Item*`, both
carriers asked for, every buffered reading of a `parse` and a `yield`, at 32, 64, 128 and 256
spaces. The reader and the engine agree on every answer.

| Input | Reader (direct) | Engine |
| --- | --- | --- |
| `k=a` + n + `b \| k=v` (accepted) | 1,380 / 4,740 / 17,604 / 67,908 | 107 / 171 / 299 / 555 |
| `k=a` + n + `b` + n (accepted, trailing padding) | 2,590 / 9,246 / 34,846 / 135,198 | 147 / 275 / 531 / 1,043 |
| `k=a` + n + `b` + n + `\|=` (refused, `parse`) | 30,371 / 206,083 / 1,518,019 / 11,653,955 | 1,440 / 4,896 / 17,952 / 68,640 |
| the same, `yield` | 1,480 / 4,968 / 18,088 / 68,904 | 216 / 408 / 792 / 1,560 |
| `k=a` + n + `\|` + n + `=x` (refused) | 290 / 546 / 1,058 / 2,082 | 217 / 409 / 793 / 1,561 |

The reader is quadratic where it accepts and cubic where a whole-result parse refuses: every turn
asks `Separator`, which reads the rest of a run of spaces, and every give-back replays the run
from its start. **The engine is linear where it accepts but quadratic where it refuses**: its
run keeps one entry and gives back one character at a time, and each give-back into a run of
spaces has `' '*` read the rest of that run again.

## Rendering it through the engine does not cover it

Why the reader is chosen: `CanDirect` asks whether the reader can read every construct a
publication reaches, not what it costs, and it reads every construct here. Sending the shape to
the engine instead would:

- move **whole publications**, not rules: `CanDirect` decides per group of publications, so
  every rule those publications reach would change rendering;
- change the **carrier** where it is the immediate one: the engine carries only on the tape
  (GRAM5007), so where constructions run would change;
- leave the refused `parse` **quadratic** (the table's engine column).

No rule of any grammar in the solution takes this path today: an instrumented rebuild of the
whole solution, logging every repetition the reader recognizes as a padded scan but does not
settle, logged none.

## The tape design, and why it is not enough

One way for the whole run instead of one per turn, with the run's end kept in it, so that a
replay sets the end and does not read the turns again. That makes a give-back cost nothing to
replay, but the continuation still reads the padding again after each one: it is exactly what
the engine's run entry already is, and the engine is quadratic on refusal. It would also need a
rule that steps over a whole run of padding in one give-back, which is the proof below applied
at run time.

## Proposed: prove the give-back futile, and settle the run

Let the run be `(?!D & any){m,}` with `D = P* & S & Q*`, the padded delimiter the scan already
recognizes (`P` and `S` single-character classes, disjoint). Let the scan end at `e`: the first
position where `D` matches, or the end of the input. Every position before `e` is one `D` does
not match at.

Let what follows the run be: zero or more repetitions with a minimum of 0 of single-character
classes `C1 ... Ck`, each contained in `P`, then a part `E` every way into which begins with a
character of `S`, or is `D` itself, or is the end of the input.

**Claim.** A shorter reading of the run never gives an answer the longest does not.

- *Ending at x < e, not in padding that runs to the end of the input.* If what follows matched
  at `x`, the `Ci` read only characters of `P` from `x` to some `y`, and `E` read a character of
  `S` at `y` (the end of the input at `y` is the next case). `S` and `P` are disjoint, so the run
  of `P` from `x` ends at `y`, and `D` matches at `x`. It does not. So the give-back fails, and
  fails before `e`: what it records is behind what the longest reading reaches, which is the
  argument the existing proof for `Text & (Separator | eof)` rests on (`FollowSets.Lead`).
- *Ending in padding that runs to the end of the input.* Then `e` is the end, the longest reading
  ends there, and what follows matches there too (every `Ci` may read nothing, then `E` is the
  end). Both readings stand at the end of the input with the same continuation; only the split
  of the padding between the run and the `Ci` differs. That is an answer only where something
  after it can succeed or fail by the split: a `when` handed a capture or a value that holds the
  run. So this case requires that no guard the publication reaches is handed a capture or value
  (the observer question `SeamSplits` asks for the seam; here asked for guards only). Where one
  is, the proof does not apply and the rendering is as today.

Where the claim holds, the run is **settled**:

- The reader already scans a settled padded run (`EmitScan`) on either carrier and every input
  form; this only makes it settled.
- The engine's padded scan (`CompilePaddedScan`, `FinishScan`) pushes its run entry
  unconditionally; it would not push it where the run is settled, so it is not given back.

Where this lives: `FollowSets.Lead`, the lattice of what a continuation begins to consume with,
answers "unknown" for a repetition that may read nothing. It would gain one more kind, "padding
of class C, then lead L", joined like the others, and `Lead.Refuses` would gain the delimiter
form above. `Determinism.NeverGivesBack` asks it in the branch that already handles a turn led by
`?!X`. Over kinds nothing changes: the padded scan is characters only.

### Prototype evidence

Forcing every `?!`-led run settled (unsound in general; for measurement only), the same table:

| Input | Reader (direct) | Engine |
| --- | --- | --- |
| accepted, padding inside | 135 / 199 / 327 / 583 | 107 / 171 / 299 / 555 |
| accepted, trailing padding | 155 / 283 / 539 / 1,051 | 147 / 275 / 531 / 1,043 |
| refused, `parse` | 296 / 552 / 1,064 / 2,088 | 219 / 411 / 795 / 1,563 |
| refused, `yield` | 294 / 550 / 1,062 / 2,086 | 216 / 408 / 792 / 1,560 |

Linear on both, with the same answers.

### What it does not cover

- A continuation whose padding class is wider than the delimiter's (`[' ' | '\t']*` after a
  delimiter padded by `' '*`): a shorter reading can then genuinely match, and the give-back is
  real. That is ordinary backtracking over two overlapping runs, not this defect.
- The trailing-padding case where a guard can see the split: stays as today.
- On the immediate carrier, a construction that would have run inside a give-back the proof
  removes no longer runs. The first case removes only give-backs that fail before any
  construction of what follows; the second removes constructions of readings the parse then gave
  up, which syntax.md 7.5 already keeps from failing the parse.

## Questions for review

1. Is the second case's observer condition (no guard handed a capture or value, anywhere the
   publication reaches) strict enough, and should it be narrower (only guards after the run)?
2. The engine: settle its padded scan only where the new proof holds, or wherever
   `NeverGivesBack` holds (which also drops the run entry from padded scans the existing proof
   already settles, a wider change to generated code)?
3. A refusal message: the give-backs removed fail at positions before `e`. If every way past
   `e` then fails without recording anything, the furthest refusal could have been one of
   theirs. The existing lead proof accepts the same; the parent-against-branch differential is
   the check.
