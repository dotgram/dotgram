# Allocations against QuickFIX/n, 2026-09-21

What each side allocates to read one message and to hold it to the schema. **KB here is 1024 bytes**, as BenchmarkDotNet prints it. **Times are not in this
file**: the run that produced it was pinned to cores 16-31 without a window, which is right for
allocation counters and wrong for a clock. Allocation counters are deterministic — they do not
move with the load on the machine — so these numbers stand on their own.

Library under test: this repository at `32070558`. Compared against **QuickFIXn.Core 1.14.1** and
**QuickFIXn.FIX44 1.14.1**, both sides reading the FIX 4.4 dictionary that ships with the latter.
The basis of the comparison — which call faces which, what each row reads, and what is deliberately
not measured — is in `basis.md` beside this file.

## Two instruments, agreeing

Every figure below was taken twice: by BenchmarkDotNet's `MemoryDiagnoser`, and by reading
`GC.GetAllocatedBytesForCurrentThread` around single calls (`--alloc-check`, the median of nine
after four warm calls). On our parse rows they agree to the second decimal — 4.30 and 4.30, 5.95
and 5.95, 488.92 and 488.89, 2.84 and 2.84 — and elsewhere within a few tenths of a kilobyte, which
is the cost of the delegate and the array the second instrument allocates around the call it
measures. The third size below was taken with the second instrument alone, in seconds rather than
in a run.

## The resolution of the run

Every pair has an A/A row: the same work under a second name. They agree to the last digit printed,
0.01 KB at the largest shape:

| shape | ours | ours again | QuickFIX/n | QuickFIX/n again |
| --- | ---: | ---: | ---: | ---: |
| Order | 4.30 KB | 4.30 KB | 4.54 KB | 4.54 KB |
| Parties | 5.95 KB | 5.95 KB | 10.63 KB | 10.63 KB |
| PartiesLarge | 488.92 KB | 488.91 KB | 1888.38 KB | 1888.38 KB |
| Binary | 2.84 KB | 2.84 KB | 3.55 KB | 3.55 KB |

So a difference above about 0.01 KB is a difference, not the hour.

## Reading a message, with eight fields read on both sides

| shape | ours | QuickFIX/n | theirs / ours |
| --- | ---: | ---: | ---: |
| Order | 4.30 KB | 4.54 KB | 1.06× |
| Parties (3 entries) | 5.95 KB | 10.63 KB | 1.79× |
| PartiesMedium (100 entries) | 52.95 KB | 193.98 KB | 3.66× |
| PartiesLarge (1000 entries) | 488.92 KB | 1888.38 KB | 3.86× |
| Binary (length/data pair) | 2.84 KB | 3.55 KB | 1.25× |

## Reading it and holding it to the schema

| shape | ours | QuickFIX/n | theirs / ours |
| --- | ---: | ---: | ---: |
| Order | 4.34 KB | 6.55 KB | 1.51× |
| Parties | 6.01 KB | 14.21 KB | 2.36× |
| PartiesLarge | 489.03 KB | 2374.90 KB | 4.86× |
| Binary | 2.88 KB | 5.45 KB | 1.89× |

## What the growing sizes say, which no single size could

The same message shape at three entries, a hundred, and a thousand. The ratio is not the same at
all three — **1.79× at three, 3.66× at a hundred, 3.86× at a thousand** — so what differs is not a
constant per message but a cost per entry, with a fixed part that dominates the smallest case.

Two sizes would only have been *consistent* with a straight line; three say it plainly. The cost
per entry, taken between neighbouring sizes:

| | 3 → 100 | 100 → 1000 |
| --- | ---: | ---: |
| ours | 0.4845 KB | 0.4844 KB |
| QuickFIX/n | 1.8888 KB | 1.8828 KB |

Both are linear in entries over this range, to within a tenth of a percent, and the ratio per entry
is **3.89×**. It is still three points and not a proof: a term that only bites past a thousand
entries would not show here.

### An open question about OUR number, not theirs

**496 bytes an entry — 0.4845 KB at 1 KB = 1024 bytes, which is the base every figure in this file uses — for an entry of three short fields.** Theirs is four times worse, and that is
exactly why this is easy not to notice: a number that looks good beside a worse one is still a
number. Nobody has asked yet where it goes — the entry's own `FixFieldSet`, its node array, the
boxing of the view, the group list — and nothing here is a defect until somebody does. Written down
so that it is a question with a date rather than something rediscovered in a year.

A table of one size each would have reported 1.79× and said nothing about which of these numbers a
reader with a big message should expect.

## Our two schema modes allocate identically

Every row above is the same to the last digit whether our schema is the one compiled into the
package or one loaded from their dictionary at run time. That is expected — parsing does not
consult the schema, and validating a VALID message finds nothing to report either way — and it is
worth stating because it locates the cost of the dictionary mode entirely in the one-time load,
which is measured separately and cold.

## Holding the schema itself, cold

`FixDictionaryFirstCall`: one invocation a sample, a fresh process a launch, ten launches, the file
read once first so that neither side pays the disk for both.

| | allocated | wall, median |
| --- | ---: | ---: |
| ours, `FixParser.LoadDictionary` | **1.08 MB** | 16.6 ms |
| QuickFIX/n, `new DataDictionary` | **4.85 MB** | 21.1 ms |

The allocation is the number this file stands behind: it is deterministic and holds whatever the
machine was doing. **The times are NOT quotable** — taken outside an announced window, and their
side's spread was large (a mean of 28 ms against a median of 21). They are here only to say the
order of magnitude, because that matters for one correction below.

### A figure of mine that was about something else

Earlier I reported "about 4.5 s to load a dictionary", and it travelled into the brief for this
comparison. That number was the EXPRESSION-LANGUAGE road — composing 8.8 M characters of rule text
and compiling it — and that road was not taken. What the package does today is read the file into
tables, and that is **tens of milliseconds, not seconds**. The old figure was true of a thing that
no longer exists, which is the most durable kind of wrong number.
