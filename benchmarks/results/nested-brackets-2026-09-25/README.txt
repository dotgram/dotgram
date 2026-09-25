THERE IS NO FACTOR OF TWO BETWEEN THE GENERATED PARSER AND THE HAND-WRITTEN ONE ON NESTED
BRACKETS. Asked the same question, the two make exactly the same number of rule entries: 20,100 at
n=200 and 80,200 at n=400, the triangular numbers to the digit, on the accepted input and on the
refused one alike. The 40,200 and 160,400 filed in hand-against-generated-2026-09-24 for the
generated side are the `Match<T>` form of the publication, which reads a refused input a SECOND TIME
to record what was expected where; the hand parser has no such form, so the comparison was between
two different questions.

That sentence is first because a task was opened on the other reading of it -- fold an alternative in
and halve the count -- and there is nothing to halve.

The proof is one line of the table below and it reproduces the filed numbers exactly:

  refused, n=400, TryParse(out T)    246,263 entries    QueryExpressionBody  80,200
  refused, n=400, Match<T>           492,526 entries    QueryExpressionBody 160,400

492,526 and 160,400 are what was filed. The package's own release notes say this plainly -- "a
refusal costs it about half, since nothing is read a second time to record the message" -- so it is
by design and it is the reason the third form exists. WHAT IT MEANS FOR A CALLER: on a refused deep
input, `Match<T>` costs twice `TryParse(out T)`, and the cheaper form is the one to hold hostile
input to.

WHAT IS REAL, AND IT IS THE WHOLE OF IT: THE EXPONENT. Both parsers are quadratic in the nesting
depth, and both on input that is ACCEPTED, not only on refusals.

  side, shape                                bytes/prev (n=100,200,400,800)   ns/prev
  generated, '(' x n + "a = 1"   refused      1.00, 1.00, 1.00                 1.19, 3.18, 4.22
  generated, closed                           1.98, 1.99, 1.99                 3.60, 3.89, 3.92
  hand,      refused                          1.00, 0.97, 0.99                 1.57, 3.95, 4.26
  hand,      closed                           1.97, 1.99, 1.99                 3.64, 2.94, 3.57

And by entries rather than by the clock, which is the count that settles it: 65,962 at n=200 against
251,862 at n=400 on the accepted input, 3.82x for twice the input.

T-SQL IS NOT THE SAME SHAPE. Same screen, TryParseStatement, SELECT and brackets: closed is LINEAR
(bytes 1.96, 1.98, 1.99; ns 1.97, 1.38, 1.14) and only the refusal is superlinear. So the standard's
grammar has this in the hot path and T-SQL does not.

THE MECHANISM, named by the count. `PrimaryReading` (SqlStandard.gram) has two alternatives that begin
with a bracket:

    = q: ScalarSubquery & s: PrimarySteps         <- Subquery = '(' & QueryExpression & ')'
    | b: Bracketed & s: PrimarySteps & when ...   <- Bracketed = '(' & Disjunction & BracketTail
    | p: PrimaryBase & s: PrimarySteps & when ...

`Read_Subquery` is entered exactly n times -- 200 and 400 -- once per bracket, and each of those
attempts descends the whole remaining nest through QueryExpressionBody -> QueryTerm -> QueryPrimary,
whose own second alternative is again '(' & QueryExpressionBody, before failing. Sum over the levels
and it is n(n+1)/2, which is what the three of them count. Every other rule is linear (402, 804).

The grammar records the same fix applied once already, in the comment above `Bracketed`: three
readings of a bracket -- a parenthesized value expression, an explicit row value constructor and a
generalized invocation -- were left-factored into one because "tried one after another, each read the
bracket's contents again, and a line refused deep inside nested brackets cost a power of their depth".
`ScalarSubquery` is the fourth reading, left outside that factoring. But it cannot simply join it: the
choice is query-or-value and cannot be made at the bracket without looking past the whole run of
brackets, which is why the hand-writer, working from the same BNF, wrote the same exponent.

TWO WAYS TO MEASURE THIS WRONG, both of which cost a wrong answer here first.

BYTES ARE BLIND TO IT. Allocation is CONSTANT IN DEPTH on the refused input -- 3,264 bytes at n=100
and 3,265 at n=800 -- and exactly linear on the accepted one, while the time is quadratic on both.
The extra work is attempts that build nothing, so a parser can be quadratic and allocate flat, and a
screen by allocated bytes reads clean.

AND A DEEP PARSE MUST BE TIMED ON A THREAD BIG ENOUGH THAT NOTHING IS HANDED OFF. On the default
stack the closed series read 3.44 / 3.95 / 9.79 per doubling and the last figure is not an exponent:
it is `Deepen` creating a 16 MB thread inside the reading being timed. On a 64 MiB thread the same
series is 3.60 / 3.89 / 3.92. The hand parser needs the big thread for the other reason -- it has no
depth guard at all and dies at 314 levels on 1 MiB.

HOW TO RUN IT. instrument.py rewrites the generated SqlStandardParser into gen/ beside it with one
increment at the head of each of the 2,159 reader methods, and writes the name table the probe
prints; it is a SNAPSHOT, so a grammar change means running it again or counting yesterday's parser.
probe.cs drives the four inputs and the two publication forms. nestcount.csproj.txt is the project,
filed with a .txt suffix so nothing in the repository globs it, and it compiles DotGram.Sql's own
sources beside the instrumented .g.cs -- shim.cs of the September instrument supplies the embedded
attribute, and is not copied again here. screen.cs and screen.csproj.txt are the exponent screen,
which needs no instrumented build and reads both sides and T-SQL.

Taken at bafdcf00. The entry counts agree to the digit with performance-9f's second instrument,
which shares nothing with this one but the parser it counts.
