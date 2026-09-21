D63's before-picture, re-taken on 2026-09-21 at 80ba6733, with two columns the first one
did not have. Counts, not times: the instrumented build is slower and its durations mean
nothing. No window is needed and none was open - the window file read IDLE.

WHY IT WAS RE-TAKEN AT ALL. The filed picture was taken at 5ed9682b, about sixty commits
back, including the whole pooling chain that changed the emitted Return in every parser.
Checking an instrument against a known answer costs less before it measures something new
than after; discovering at the after-picture that the before-picture does not reproduce
would have cost the whole of the work.

IT REPRODUCES. 995 reader methods instrumented and 249 roll-backs in a way-back wrapper,
the same two figures as at 5ed9682b. The rewriting shares come back identical - media type
57.4 / 72.7, language tag 52.2 / 71.0, structured field 83.9 / 91.5 - and the re-reading
column, which the first run collected and did not print, reads 1,723 at n=16 and 25,195 at
n=64 for the unclosed quoted string: before-counts.txt to the digit. So the arithmetic is a
property of the reader and not of one tree, now on two instruments and sixty commits.

                                        calls x  writes x  deepest x
                                         16->64    16->64     16->64
  addresses, unclosed quote (refuses)      14.6       -          -
  addresses, valid list (ACCEPTS)           4.1       4.0        4.0
  media type (refuses)                     13.5      11.8        3.4
  language tag (refuses)                   12.8      12.2        3.5
  structured field (refuses)               17.6      17.5        4.0

Input x4. Linear is 4.0; the triangular number is 15.3. The raw table, with the absolute
figures every ratio is taken from, is four-columns.txt.

WHAT THE TWO NEW COLUMNS SAY, AND IT DECIDES WHAT D63 MAY PROMISE. Writes are quadratic
and the deepest Log is LINEAR - 3.4, 3.5, 4.0 against an input four times as long. Retries
multiply turnover and cannot raise the peak. So the cure buys TIME, NOT MEMORY: a
retained-bytes reading of it will show nothing, and will be right to. This is written down
before the cure exists, because promising memory would have made the instrument that told
the truth look broken.

WHY "DEEPEST" IS THE PEAK AND NOT A SAMPLE, and what would silently make that false.
LogCount advances in exactly two places - Ways.Begin and Ways.Put. End rewrites one slot in
place (Log[Opened] = LogCount - Opened) and a roll-back only lowers the counter. The hook
sits at the head of each of those two, before the resize, where LogCount still holds the old
value, so the maximum over what each call is about to reach is the maximum the counter ever
holds. That is a claim about the code, not about the measurement: ADD A THIRD PLACE THAT
ADVANCES LogCount AND THIS COLUMN BECOMES A LOWER BOUND WITHOUT ANYTHING SAYING SO.

THE ROW THAT DOES MOST. `addresses, an unclosed quoted string` writes NOTHING to the log -
zero writes, zero depth - while its calls go 1,723 to 25,195, a clean quadratic on the
triangular number. The disease is at full strength and the rewriting that a keep-the-prefix
cure would remove is not there at all. So on that shape such a cure buys exactly nothing,
measured rather than argued.

THE ACCEPTING ROW IS THE CONTROL AND BEHAVES LIKE ONE: linear in all three columns, and its
writes equal its depth to the digit (608/608 at n=16, 2,432/2,432 at n=64), so nothing is
ever written twice and there is no re-reading on an accepted parse at all. D63's condition -
that remembering a failed prefix must not cost an accepted parse - is observed in the same
table as the disease rather than argued in a separate exercise.

WHAT IS STILL UNMEASURED, and it is the only thing left. `begins` counts Ways.Begin - arms
begun on the tape - and NOT a repetition's turn; this file's earlier reader should not take
it for the multiplier of a remember-the-turn cure, which is what its name invites. The
degrees are known and equal for the disease and for any such cure; what decides is the ratio
of constants - what one re-read costs against what one stored position costs - and that
needs a definition of "turn" read out of the emitted repetition rather than off a tape
counter.

HOW TO RUN IT. instrument.py rewrites the generated Web parsers into a scratch directory
(set S at its head) and probe.cs is the counter and the driver; recprobe.csproj.txt is the
project that compiles the two together with the Web package's handwritten sources, filed
with a .txt suffix so that nothing in the repository globs it. Build the Web package first,
so the generated files are the ones being asked about. The project is filed this time
because the first taking left it in scratch, and scratch was cleaned: an artifact a queue
item depends on is filed before the cleaning, not after.
