Median of 6 of 9 runs, each in a process of its own; control 31.6 ns (the runs' controls: 31.9, 31.7, 31.5, 31.6, 45.0, 46.2, 46.2, 31.6, 31.3).
Dropped for a control more than 5% off the median: run 5, 6, 7.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 02:47

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 86.1 | 207.8 | 2.41x | 207.9 | 2.42x | +0.1% | 360 | 360 | 1 % | [-1.3%..+1.9%], 3 of 6 positive |
| web/url.refused | generated | hand | 59.9 | 249.7 | 4.17x | 251.9 | 4.20x | +0.8% | 64 | 64 | 4 % | [-2.2%..+3.1%], 4 of 6 positive |
| web/url.relative | generated | hand | 91.0 | 183.1 | 2.01x | 190.2 | 2.09x | +3.9% | 312 | 312 | 8 % | [-2.2%..+4.4%], 4 of 6 positive |
| web/url.relative-dot-colon | generated | hand | 77.0 | 160.5 | 2.08x | 163.5 | 2.12x | +1.9% | 280 | 280 | 7 % | [-1.6%..+4.7%], 5 of 6 positive |
| web/url.relative-letters | generated | hand | 117.4 | 216.2 | 1.84x | 226.4 | 1.93x | +4.7% | 312 | 312 | 34 % | [-3.8%..+12.1%], 3 of 6 positive |
