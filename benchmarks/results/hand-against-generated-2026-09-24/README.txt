THE HAND-WRITTEN SQL:2023 PARSER FALLS. It falls EARLIER than the generated one, at every stack
size tried, and the generated one does not fall at any depth tried at all. "It doesn't fall" was
never measured; nobody had fed it deep nesting.

That sentence is first because the task was set on the opposite premise.

                                    hand              generated
  256 KiB thread   survives 65, DIES at 70       refused at 2,000, 164 KiB touched
    1 MiB thread   survives 314, DIES at 319     refused at 2,000, 932 KiB touched
    4 MiB thread   survives 1,308, DIES at 1,313 refused at 2,000, 4,004 KiB touched

Input is the one StackDepthTests uses: `'(' x depth + "a = 1"`, refused because it is never
closed, through TryParseSearchCondition on both sides. Release, one process per point, the
high-water mark read by walking the thread's own committed pages after the parse returns. A death
is read as the absence of the program's own printed verdict, never as an exit code: a stack
overflow leaves no chance to set one.

WHY THE GENERATED ONE SURVIVES AND THE HAND ONE DOES NOT. The generated reader probes the stack
and, when the margin has gone, carries the reading onto a stack of its own (Deepen). The hand
parser has no depth guard at all -- no TryEnsureSufficientExecutionStack, no hand-off -- so it
recurses until the process dies. The touched figures above show it: the generated side's high
water stops growing (164 KiB on a 256 KiB thread, whatever the depth) because it has moved
elsewhere, while the hand side's grows until there is nothing left.

THE DYING DEPTHS ARE INTERNALLY CONSISTENT, which is the check that they are measuring the frame
and not the harness. At 3.09 KiB a level with about 50 KiB of fixed cost, a stack of S KiB should
die at (S - 50) / 3.09 levels: 67, 315 and 1,309 against 65-70, 314-319 and 1,308-1,313 measured.
Three sizes, three fits within two levels.

AND THE STACK PER LEVEL IS WITHIN TEN PER CENT, which is the answer to "the generated parser is
doing something wrong".

                    hand      generated   generated/hand
  Release           3.09 KiB    3.36 KiB       1.09x
  Debug             3.65 KiB    4.15 KiB       1.14x

Measured at bafdcf00, on a 64 MiB thread so that neither side diverts and the slope is the frame
cost alone, between depths 400 and 800 so the fixed part cancels.

THE REVISION MATTERS HERE MORE THAN USUAL. Earlier the same day the generated reader measured
4.50 KiB a level in Release, and that figure is quoted in D141's derivation and in several
messages. It is not wrong: it is the figure BEFORE bafdcf00 made the towers' four value types
classes. That commit is the whole of the difference between 4.50 and 3.36 -- a quarter -- and it
is why the gap against the hand parser is now a tenth rather than a half. Any figure from this
family taken before bafdcf00 has to be read with its revision attached.

CALLS PER CHARACTER, BOTH SIDES, SAME INPUTS, SAME REVISION (counts-hand.txt,
counts-generated.txt). One entry a rule method on each side, so the two numbers mean the same
thing: 422 probed methods on the hand side, 2,159 on the generated one.

  input             chars     hand /char   generated /char   generated/hand
  conditions100     1,275     4.32         4.71              1.09x
  conditions1000   14,775     3.72         4.06              1.09x
  nested200           205     410.04       616.22            1.50x
  nested400           405     810.02       1216.11           1.50x

THE CALL GAP AND THE STACK GAP ARE THE SAME NUMBER on ordinary input: 1.09x of entries a
character against 1.09x of stack a level, measured by two instruments that share nothing. So they
are one cause and not two -- the generated reader does about a tenth more work a token, and that
tenth shows up in both quantities.

AND THE SPEED GAP IS BIGGER THAN EITHER. The stand reads immediate against hand at about 1.3x on
SQL. Entries explain 1.09 of that, so roughly two thirds of the speed gap is how MANY calls and a
third is what each call costs. Neither figure is the 2.7 to 5.8x the stand once read for the tape;
that gap was closed by other work and is not what these numbers are about.

BOTH PARSERS ARE QUADRATIC ON NESTED PARENTHESES, and this is the finding least likely to have
been guessed. `'(' x n + "a = 1"` gives, on the HAND side, 20,100 entries to QueryExpressionBody at
n=200 and 80,200 at n=400 -- the triangular numbers 200x201/2 and 400x401/2 exactly -- and 3.90x
the total entries for twice the input. The generated side does the same shape at twice the
constant: 40,200 and 160,400, and 3.90x again.

So the quadratic is a property of the grammar's shape, not of the generator: a hand-written
recursive descent over the same rules re-scans the same way. What the generator adds is the
constant, and on this input it is 1.5x rather than the 1.09x of ordinary input.

WHAT THIS DOES TO THE QUESTION. Two of the premises the task was set on do not survive:

  - "the hand parser does not fall" -- it falls, at a fifth of the depth the generated one is
    still answering at, on a 256 KiB thread.
  - "the generated parser is doing something wrong with the stack" -- it costs 9 to 14 per cent
    more a level than a hand-written recursive descent over the same grammar, and it survives
    inputs that kill the hand-written one outright.

What is left of the original question is the SPEED gap, which this file does not touch, and the
per-frame anatomy: ten frames a level against the hand parser's, and what fills them. Those are
the next two measurements and they are not taken here.

HOW TO RUN IT. .work/stackdepth (ignored by git) is a console app that parses on a thread of a
stated size and reports what the thread was GIVEN, the high-water mark, and the verdict:
`stackdepth <hand|sql2023|tsql|el> <depth> <stackKiB> <outputFile>`. On Windows the mark comes
from walking the region with VirtualQuery; on Linux from the Rss of the mapping in
/proc/self/smaps, found by the mapping's size because a secondary thread's stack is anonymous.
Three traps are built into it rather than remembered: read the size the thread was GIVEN and not
the one asked for; walk on the thread that parsed, after it returns; and treat silence as death.
