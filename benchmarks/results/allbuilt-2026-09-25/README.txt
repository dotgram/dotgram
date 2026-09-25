THE MATERIALIZER'S SCAN WAS QUADRATIC ON A NESTED INPUT, AND A WATERMARK REMOVES IT. What this
file holds is not the speed-up — that is one table and it is at the bottom — but HOW THE CHANGE
WAS VERIFIED, because no test in this repository can fail on the defect the change guards
against, and that had to be established rather than assumed.

WHAT THE CHANGE IS. A guard that asks for the record it has just captured builds it where it
stands instead of walking the log, provided every record below the root is already built. Until
now "already built" was answered by scanning the flags from the walk's start to the root, and on
a tower of n guards that scan is walked n times over a range that grows with n: the triangle.
`Ways.AllBuilt` records how far up the log every record is known built, so the scan starts at
that watermark instead of at the walk's start, and is skipped altogether where the watermark is
already past the root.

   `Built`    says the flags below it are not stale. A record below it may still be false.
   `AllBuilt` says every record below it is TRUE. That is the stronger claim, and it is the
              one that lets the scan be skipped rather than merely shortened.

THE RAISE IS CONDITIONED, AND THAT IS THE WHOLE OF THE RISK. Where the fast path builds the
root, it may raise `AllBuilt` past it only if the proof covers everything below. The scan proves
`[from_, root)` and nothing below `first`, so a walk that began ABOVE a dead slot must not claim
it. Hence `if (known || ways.AllBuilt >= first)` rather than a bare raise.

NO TEST IN THIS REPOSITORY FAILS ON THE UNCONDITIONED FORM. That is a measured statement, not an
impression. The unconditional raise was built deliberately and run against DotGram.Tests (9,591),
DotGram.Sql.Tests (14,795), DotGram.Finance.Tests (4,458), DotGram.Finance.Fix44.Tests (613) and
DotGram.Tests.Slow (271): all green, every exit code 0. Two grammars written on purpose to reach
the case — a dead slot below a guarded tower, and a dead sibling under one — passed with the
defect in. One red appeared and was NOT the defect: RefusalCorpusTests grew from 2,931 to 3,261
lines because of the shape added to reach the case, and removing the shape made the wrong version
pass everything again.

SO THE VERIFICATION IS BY INSTRUMENT, RUN THROUGH EVERY SUITE. The emitter was made to write two
checks into EVERY generated parser, which is what puts the SQL corpus, the carrier shapes, the
refusal corpus, FIX, the expression language and the Web through them rather than two grammars of
my own devising. The checks were then removed; nothing here ships.

  1. SOUNDNESS, on every fast path. Does the shortened scan answer what the full scan would?
     Emitted as a comparison of the two at the condition, throwing on disagreement.

     ZERO DISAGREEMENTS, all five suites, 29,728 tests.

  2. EXPOSURE, at the raise. How often would the UNCONDITIONAL raise set `AllBuilt` over a slot
     that is actually unbuilt?

     NOT ZERO, AND NOT EXOTIC. `a + b * c` reaches it.

        slot=0  allBuilt=0  first=1  root=1    TransactSqlTests, an ALTER TABLE
        slot=7  allBuilt=7  first=9  root=15   ShippedExampleTests, the skill's standard example
        slot=7  allBuilt=7  first=9  root=13   SqlWalkerTests
        slot=7  allBuilt=7  first=9  root=15   SqlStandardTreeTests  "a + b * c"
        slot=0  allBuilt=0  first=5  root=9    SqlStandardTreeTests  "T::m(1)"
        slot=0  allBuilt=0  first=5  root=9    SqlStandardTreeTests  "f(a => 1, b)"
        slot=15 allBuilt=15 first=17 root=23   SqlStandardTreeTests  "NULLIF(a, b)"

     So the condition is doing real work against a case our grammars produce constantly, and
     "the case never arises" — the comfortable answer — is false.

AND THEN THE QUESTION THAT DECIDES IT: IS THE LIE EVER READ BACK? Exposure alone is not a bug; a
false `AllBuilt` matters only where a later fast path consults it. The soundness check answers
exactly that, so it was run AGAINST THE UNCONDITIONAL RAISE:

     THIRTY DISAGREEMENTS, all in DotGram.Sql.Tests, all of this shape:

        ALLBUILT-UNSOUND root=4 first=0 allBuilt=4 known=True from=4 ours=True full=False

     and every one of them on a REFUSED reading — the JSON-path inputs of
     SqlStandardParserTests.A_JSON_value, `a[?($ < 1)]`, `a[last to]`, `a[?(exists($))]` and
     their kin, each carrying `reads: False`.

THAT IS WHY NO TEST CAN FAIL, AND IT IS A BETTER ANSWER THAN "IT NEVER HAPPENS". The invariant IS
violated, and the violation IS read back. It changes no answer because the only readings that
reach it are refused ones, and a refused reading's values are never asked for. Removing the check
and running the suites against the unconditional raise alone confirms it: DotGram.Sql.Tests exit
0, 14,795 green.

  A test that asserts the PARSER'S ANSWER therefore cannot fail on this defect by construction,
  since the only inputs reaching it have "refused" for an answer either way. The gate has to be
  the invariant, and the invariant is not reachable from a test project — `ways` is internal to
  emitted code. Hence this file, and hence the snapshot below.

  It also says where this stops being harmless: anything that reads values out of a refused
  reading. A recovery that keeps a partial tree would turn all thirty into live defects.

THE SNAPSHOT GAP, CLOSED. Of the eight files under tests/Snapshots, NOT ONE emitted the fast
path, so the whole of this code could have been rewritten without appearing in a diff anybody
reads. `Tower.gram` is a guarded tower eighteen levels deep — eighteen because the walk's arms
must be methods of their own before the fast path is emitted at all, and that asks for sixteen —
with the guard between the inner value and the closing bracket, and a tail read after the bracket
closes. Its carrier is named rather than left to `Auto`, which reads the language as one that
need not replay and hands it the immediate carrier, and an immediate reader has no walk to skip.

A DEFECT FOUND ON THE WAY, worth recording because it was in the change and not in the code the
change was about: the `AllBuilt` give-back was emitted without braces under `if
(machine._directBuilds)`, so grammars that build nothing got it anyway — Buffered, Feed and
Minimal carried 27, 13 and 4 sites against zero of `Built`. The lowerings now pair one for one.

================================================================================================
WHAT IT IS WORTH

THE WITNESS, counted rather than timed (.work/matcount, an instrumented copy of
SqlStandardParser that counts what the fast path's condition scans). The triangle is gone:

  nested n        scan total before        scan total after      short-circuits
  200                  2,122,963                      1,050                 604
  400                  8,405,763                      2,050               1,204
  800                 33,451,363                      4,050               2,404
                    (3.96x, 3.98x)                (1.95x, 1.98x)

Before, twice the input cost four times the scanning; after, twice the input costs twice. The
scans that remain average about one element, because the range starts at max(first, AllBuilt)
rather than at first. On the same brackets laid FLAT the ratio was 2.00x before and after: the
triangle was a property of nesting and nothing else.

The walk's own triangle is untouched at 20,902,113 — that is the other 60% of the cost, a
separate question, and this change does not claim it.

THE PAIRED STAND SHOWS NOTHING, AND THE FIRST VERSION OF THIS FILE CLAIMED THAT IT DID.

What was written here, and sent to the architect twice, was `sql/refused-late.bool` 1.97x faster
and `el/refused-early.bool` 3.55x -> 1.82x and `el/refused-late.bool` 1.91x -> 1.00x, with a
tidy story about the fast path paying on refused readings where the tree is never asked for.
EVERY ONE OF THOSE IS AN ARTIFACT OF THE ROW, not of this change, and the stand says so itself,
in bold, in the header of the very file the numbers were read from:

  "The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the
   before side and `after` the bool form of the after side (StandBool.cs), so their difference
   is the price of the Match form against the bool form. It is there in an A/A of one build and
   in a pair of commits that have nothing to do with it, and it is not an effect of the commit
   under test."

The Match form reads a REFUSED input a second time to record what was expected; the bool form
does not. So a `.bool` row on refused input is about two-to-one before anything is committed at
all. This is the same fault, on the same day, as the one in
hand-against-generated-2026-09-24 — a round factor on one input class means the two readings
asked different questions — and the lesson had already been written down when these numbers
were quoted. Reading a table is not reading its header.

WHAT THE TIMING ACTUALLY SAYS, with the `.bool` rows struck out: NOTHING ABOVE THE SPREAD. Every
row of the expression language moves between -0.3% and +4.5%, against base spreads the stand
prints beside them of 3% to 38%. `el/ladder` immediate -0.3%, `el/nest7` generated +4.5% against
a 3% spread, `el/parse-500k-tokens` +17.1% against a 38% spread. The immediate carrier, which
this change cannot touch, moves as much as the tape does.

That is not a disappointment, it is the expected reading, and the counts say why. The scan is
quadratic in NESTING DEPTH, and no row on the stand nests deeply enough to pay for it: the
triangle at 800 levels is 33 million elements, and at the depths these rows reach it is a few
thousand. The row that would show this change does not exist yet. That is the 584a7c1f lesson
the other way round — that commit went quadratic and no pair saw it because no row was longer
than twenty terms.

So the case for the change rests on the COUNTS, which cannot be confounded by an API form, and
on the two facts that the counts establish: the triangle is gone, and nothing else grew.

THE SIZE OF IT. DotGram.Sql.dll 19,820,544 -> 20,028,928 bytes (+208 KB, +1.1%);
DotGram.ExpressionLanguage.dll +15 KB (+0.8%). The growth is 1,291 give-back sites in SQL and 8
in the expression language, one line each.

DotGram.Finance.dll is THE SAME SIZE AND NOT THE SAME BYTES — 13,984,768 both, md5
98d6a8a00a369cdc88cc332910b7de05 against 43346ae6b22dfd69b58d8da1b82a68f3. This file said "byte
for byte identical" until the hashes were taken, on the strength of the sizes agreeing; a PE
file is padded to its alignment, so a change of a few dozen bytes leaves the size exactly where
it was. What actually reaches FIX is two lines in FixGrammar.g.cs — the field's declaration and
its reset — and neither a give-back nor a fast path, FixGrammar having no guard that builds. So
the claim that wanted making still holds, and it is the generated code that makes it and not the
file size: nothing FIX executes changed.

================================================================================================
HOW TO RUN THE CHECK AGAIN

The check is not in the emitter. Keeping it there was priced and declined: it would want a new
flag on `CSharpEmitter.Emit`, which is public, threaded to six `Machine` construction sites and
a nineteen-parameter constructor, and then a test that either compiles the whole SQL grammar in
memory or reproduces the read-back in a small grammar — and two small grammars written on
purpose to reach the case have already failed to. So it is a recipe instead, and the recipe is
ten minutes.

In `Machine.Direct.Values.cs`, ahead of the fast path's `if`, emit:

    if (roots < 0 && root >= first && root == ways.Records - 1)
    {
        var full_ = IndexOf(new ReadOnlySpan<bool>(built, first, root - first), false) < 0;
        var ours_ = known || IndexOf(new ReadOnlySpan<bool>(built, from_, root - from_), false) < 0;
        if (ours_ != full_) throw new InvalidOperationException("ALLBUILT-UNSOUND root=" + root + ...);
    }

and, at the raise, for the exposure count:

    if (!(known || ways.AllBuilt >= first))
        for (var chk_ = ways.AllBuilt; chk_ < first; chk_++)
            if (!built[chk_]) throw new InvalidOperationException("ALLBUILT-EXPOSED slot=" + chk_ + ...);

Then `dotnet build DotGram.slnx -c Release` and run the five suites, reading the EXIT CODE and
not the summary line. The two throws are what makes it a check and not a report: a suite that
swallowed one would still be red.

  THE ONE THING THAT MAKES THIS WORTH TEN MINUTES rather than reasoning: emitting the check into
  EVERY parser is what puts the SQL corpus, the carrier shapes, the refusal corpus, FIX, the
  expression language and the Web through it. A console harness over one parser reaches none of
  those, and the two grammars I wrote by hand to reach the case both passed with the defect in.

NOT OURS, AND SINCE EXPLAINED. `fixmsg/Order.parse-stream` fails its control check in a paired
run — "the control reading refuses it, and the row says it accepts" — before anything is timed.
It fails the same way with the BEFORE build against itself (99effd0f, .work/paired/before twice),
so it is not this change — that run is the evidence, and it is direct. (An earlier draft of this
file offered "and DotGram.Finance.dll is identical across the two builds" as a second reason. It
is not identical, only the same size; see above. The before-against-before run stands on its
own.)

The cause, from finance-41 through the architect: the three `parse-*` rows answer
`InvalidFindings is null ? 0 : 1`, so a VALID message answers 0 — and `Unasserted` reads 0 as a
refusal. The library accepts the wire through both doors, probed. It is the rows that are wrong,
not the parser and not this change. finance-41 is fixing them.

================================================================================================
THE DEEP ROWS, FIRST READING (--stand-paired --only nested, 8 rows, every reading agreeing)

They do the job they were added for: the exponent in DEPTH is readable inside one run, and it
tells the two grammars apart.

  row                             100        400     400/100
  sql/nested.match             587 us   7,886 us       13.4x   quadratic
  sql/nested-refused.match      53 us     906 us       17.0x   quadratic
  el/nested.match             14.5 us    54.6 us        3.8x   LINEAR
  el/nested-refused.match     16.3 us    63.4 us        3.9x   LINEAR

Four times the depth costs SQL:2023 thirteen to seventeen times and the expression language
under four. EL is linear because 3a8d5bcd made a parenthesis and a tuple one way in, read once;
SQL:2023 still reads a bracket more than once, which is sql-47's ScalarSubquery finding and not
this change.

Before against after is flat on every one of the eight, 0.94x to 1.06x, which is what the counts
already said: the scan's triangle is removed, and at these depths the WALK's triangle is what the
clock sees. A row that can see this change at all would have to nest into the hundreds with the
walk's own cost taken out, and no such row exists. That is the honest end of the timing story.
