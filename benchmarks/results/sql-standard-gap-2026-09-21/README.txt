"FLAT" ANSWERS "DOES IT GROW", NOT "IS IT THE SAME AMOUNT AS THE HAND PARSER". The level is
about four rule entries per character, and what the hand side does per character is NOT
measured here. Established is exactly one thing: whatever the ratio of units is, it does not
change with size.

That sentence is first because it is the one most likely to be read wider than it was said.

WHY ANY OF THIS. The stand timed SqlStandardParser against HandSqlStandard on 2026-09-21 in
the SQL window and found the generated side 2.67 to 5.80 times slower, at A/A 0.94-1.04, while
allocating LESS (0.80-0.99). Tables in bdn-2026-09-21/sql-window/standard/. Other families sit
near 1.3x, so this is the outlier, and the question was whether the generated parser does more
work or the same work more slowly.

THE COMPARISON IS FAIR, CHECKED BEFORE ANYTHING WAS EXPLAINED. tests/DotGram.Sql.Tests/Both.cs
holds the two parsers to the same answers on the same inputs, and SqlStandardBenchmarks checks
in its own GlobalSetup that both build one tree before it times either. The generated side also
allocates less, so it is not winning by building less.

WHAT THE TIMED TABLE ALREADY SAID, BEFORE ANY INSTRUMENT. Every input but the shortest lands
between 2.66 and 2.99; only `literal` is 5.80. A model of 2.7x on the work plus about 43 ns
fixed per parse fits both ends: literal 13.8 x 2.7 + 43 = 80.3 against 80.1 measured, and
conditions1000 431,368 x 2.7 + 43 = 1,164,737 against 1,149,497. So the gap is a uniform
multiplier and a small constant, not a constant that amortises - and `literal`'s 5.80 is the
constant becoming visible, not the problem being worst there. Its whole gap is 66 ns, one
ten-thousandth of the largest row's.

THE COUNT (counts.txt, taken at 80ba6733, instrumented build, counts only, no window):

  input             chars        calls   calls/char      fails  rollbacks   log writes  deepest
  literal               1            2         2.00          0          0            8        8
  arithmetic           19          197        10.37         93         53          513      377
  select1              15           80         5.33         35         23          230      188
  select20            115        1,111         9.66        514        323        2,937    2,215
  conditions100     1,275        6,002         4.71      2,500      1,500       16,416   13,016
  conditions1000   14,775       60,002         4.06     25,000     15,000      164,016  130,016

ENTRIES PER CHARACTER ARE FLAT: 4.71 to 4.06 across an eleven-fold growth of input, falling if
anything. Log writes and depth track it - 16,416 to 164,016 and 13,016 to 130,016, ten times
for eleven times the input. Nothing is superlinear, so the 2.7x is PER-UNIT COST and not extra
units done as the input grows.

AND `literal` DOES TWO RULE ENTRIES AND EIGHT LOG WRITES. Two. On that input the generated
parser barely parses and still costs 80 ns against 13.8, so the 66 ns is not parsing: it is
what happens around the parse. That is where the per-table walk lives.

THE PER-TABLE WALK, AND WHY IT IS THE SUSPECT. Every generated parser does work per value
table on every parse, whatever its store: a dense one tests each table for content, a plain one
clears each and reads its length for the bound, and an adaptive one calls Clear on each.

    302 dense     SqlStandardParser
     48 adaptive  TransactSqlParser   (and 48 again in its Located reading)
     14 plain     ExpressionParser
     10 ... 7 ... 5 ... 3 ... 1    Web, Sql92, the calculators

TABLE COUNT IS NOT GRAMMAR SIZE - it is the number of distinct value types the grammar builds.
T-SQL is the larger grammar and has six times fewer tables, because its tree reuses types the
standard's declares separately. So the cost is paid per distinct value type by every parser,
and one grammar's tree happens to have six times more types than the next.

Three hundred and two branches at about a cycle each is roughly 70 ns at this machine's clock,
against the 43 ns the timed table implies. SAME ORDER IS NOT THE SAME NUMBER: this says the
hypothesis is not arithmetically absurd and nothing else. What settles it is a throwaway build
with the walk removed, which needs a window and is the next step.

WHAT IS STILL OPEN, AND THEY WANT DIFFERENT WORK. The fixed part - two calls and eighty
nanoseconds - is one throwaway and one window. The per-unit part - four entries per character
against an unmeasured hand-side figure - is either an instrumented HandSqlStandard, or first the
cheaper probe: the generator writes a way three times (flat, reader, engine), so the three can
be counted against each other on these same inputs without touching any hand-written code. If
the flat road is markedly cheaper, "four calls where one would do" is located inside our own
output and the hand side need not be measured at all.

HOW TO RUN IT. instrument.py rewrites the generated SQL parser into a scratch directory (set S
at its head); probe.cs drives the six inputs the stand times, taken from SqlStandardBenchmarks
by the same text and the same entry point rather than invented again. sqlcount.csproj.txt is
the project, filed with a .txt suffix so that nothing in the repository globs it, and shim.cs
supplies the embedded attribute that the real build gets from its own polyfill.
