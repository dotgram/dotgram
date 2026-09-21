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

WHAT THE THIRD COLUMN SET SAYS, AND IT CLOSES D63'S CURE. what-a-memo-would-cover.txt asks
the two questions that decide a memo of refusals: how much of the quadratic is a rule
re-entered at a position it has already been entered at, and how much of THAT is a rule
refusing where it has already refused.

The first is 94 to 99 per cent of all calls. The second is 1, 2, 2,080 and zero.

So the repeated work is repeated SUCCESS: a rule re-entered at the same position parses the
same prefix again, successfully, and only the tail fails. A memo that remembers refusals
shortens none of it. `structured field` says it without a word - 91,281 repeats and not one
repeated refusal, on the steepest quadratic in the set.

That turns D63's own sentence around. It read: "remembering that a rule failed at a
position, after a long re-read, is not memoizing every position". True - but the long
re-read is made of successes, so the thing it excused remembering is the thing that would
have to be remembered.

TWO EXITS, CLOSED BY DIFFERENT EVIDENCE AND WITH NOTHING BETWEEN THEM. A memo of refusals is
LEGAL on 21 to 74 per cent of the repeats (the `sound` columns, from reading the emitter) and
USELESS (it fires almost never, from counting). A memo of results would fire constantly and
is UNSOUND: a reader asks the tape whether to replay a recorded way and takes its
alternative FROM the tape, so the same rule at the same position can go a different way and
refuse on one visit and pass on another.

WHAT IS CLOSED IS THE CURE, NOT THE DISEASE. The quadratic is real, measured on two
instruments, and reproduced on today's main sixty commits after the first taking. Whoever
comes back to this must not rediscover it as news.

THE BAR FOR ANY FUTURE ATTEMPT, read off these same numbers. It must shorten repeated
SUCCESSES rather than refusals; it must stay correct while a reader's alternative comes off
the tape; and it must not become remembering everything. Each of the three is a row in this
directory rather than an opinion.

AND ONE FACT THAT OUTLIVES D63: even a sound memo would cover a minority. Between 21 and 74
per cent of repeats sit on rules where a key of (rule, position) could be correct at all -
which is a fact about how these grammars are built, not about this cure.

"NOTHING" WAS ONE OF TWO OUTCOMES FROM THE FIRST LINE, held so before any number existed and
not adopted afterwards. A report where the refusal appears only at the end reads as an
excuse; this one had it as an outcome from the start.
