# Generated validation against the walk, taken again (2026-09-20, later)

The earlier number, 1.24×, is no longer a ratio of anything: the walk has since had the shared
value check fixed and is about eight per cent faster, and the generated code has since been made to
report in the walk's order and is about three per cent slower. **Both sides of the fraction moved,
and in opposite directions.** A reader setting the two figures beside each other would conclude
that generated validation had got worse, and that is not what happened.

## The pair that answers the same

The earlier comparison was the generated code against the walk over the tables **this package
compiles in** — two readings of FIX 4.4 that disagree about ten types, seventy-seven code sets and
twenty-six compositions. It was the right pair then, because there was nothing else. There is now:

* the walk over a **loaded** dictionary, and
* the code **generated from that same dictionary**.

One schema, two roads, no difference in the data at all. Every difference between their answers is
a defect in one of them, so the equality is exact rather than pinned — and it is asserted, position
by position, over all 186 fixtures, before anything is timed.

## The numbers

One process, the largest fixture — an `ExecutionReport` of 391 fields with nested groups — 300
warm-up calls, best of fifteen batches of two thousand, three interleaved rounds.

| round | the compiled walk | the loaded walk | generated |
| --- | ---: | ---: | ---: |
| 0 | 9,219 ns | 10,044 ns | 8,569 ns |
| 1 | 9,192 ns | 9,816 ns | 8,660 ns |
| 2 | 9,143 ns | 9,889 ns | 8,635 ns |

**Generated validation is 1.13–1.17× the walk over the same dictionary, and 1.06–1.08× the walk
this package ships.** On this message that is about 570 ns of 9,180.

The loaded walk is the slowest of the three because it asks a dictionary for each tag's code set
where the compiled one reads a table it has already remembered. That is a fact about the run-time
road, not about the walk as such, and it is the honest denominator for the generated road: both
were given the same file.

## Why the number fell, and it is two reasons rather than one

**The denominator.** Fixing the shared value check — the type carried as a code rather than a name,
the code set remembered rather than searched for — took about 750 ns off the walk. The generated
code gained nothing from it: it never asked those tables, having the answers as constants. So the
advantage shrank by roughly what the walk was given.

**And the numerator, which is the part worth saying.** The generated code was made to report in the
schema's order, interleaved exactly as the walk does it, and that cost it about 200 ns. The first
version emitted every required check in a run, then the components, then the groups — the same
findings, in a different sequence. Correctness first: two roads over one dictionary handing a
reader the same findings in a different order is a difference the reader would see and could not
explain, and it is worth 200 ns without argument.

## How the order difference was found, which matters more than the 200 ns

Not by the test written to catch it. `The_generated_rules_answer_as_the_loaded_dictionary_does` had
been narrowed, earlier the same day, to report its failures as a set difference so that the message
would be readable — and a set difference does not see order. It passed.

What found it was printing both answers for one message while setting up this measurement: five
findings each, the same five, in a different order.

**Narrowing what a failure says can narrow what the test asks.** The test now compares position by
position and says which position differs, which is both a better question and a better message. The
lesson is not "compare sequences"; it is that a change made to a test's *output* is a change to the
test.

## Expectation against outcome

Written before measuring: 1.10–1.20× against the compiled walk and 1.2–1.35× against the loaded
one, with the error named as more likely an overestimate. The outcome was 1.06–1.08× and
1.13–1.17× — **both below the floor, and the named direction right for the third time running**.

The reason for the miss is the 200 ns the order fix cost, which was not in the expectation because
the defect it fixed was not yet known. That is not a flaw in the prediction so much as a reminder
of what a prediction covers: it was about the code as it stood, and the code changed for a reason
that had nothing to do with speed.

## What this does not say

The message is one message, the largest the package keeps. A short message pays proportionally more
for the per-message work — the entry switch, the header and trailer — and less for the per-field
work, which is where the generated road's advantage lives, so its advantage there will be smaller
still.

And the walk's remaining time is mostly not dispatch. Three quarters of the value check is reading
the characters of the values, measured earlier the same day, and neither road removes that.
