Median of 9 of 9 runs, each in a process of its own; control 31.5 ns (the runs' controls: 31.3, 31.5, 31.8, 31.5, 31.5, 31.4, 31.7, 31.7, 31.6).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 02:49

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.plain | generated | hand | 86.1 | 206.8 | 2.40x | 207.1 | 2.41x | +0.1% | 360 | 360 | 1 % | [-2.9%..+8.1%], 7 of 9 positive |
| web/url.refused | generated | hand | 57.8 | 249.7 | 4.32x | 194.4 | 3.36x | -22.2% | 64 | 64 | 9 % | [-28.0%..-19.4%], 0 of 9 positive |
| web/url.relative | generated | hand | 90.3 | 180.0 | 1.99x | 184.2 | 2.04x | +2.3% | 312 | 312 | 7 % | [+0.3%..+5.7%], 9 of 9 positive |
| web/url.relative-dot-colon | generated | hand | 75.8 | 155.2 | 2.05x | 161.3 | 2.13x | +3.9% | 280 | 280 | 5 % | [+1.7%..+7.8%], 9 of 9 positive |
| web/url.relative-letters | generated | hand | 120.3 | 215.1 | 1.79x | 227.6 | 1.89x | +5.8% | 312 | 312 | 62 % | [+3.5%..+9.7%], 9 of 9 positive |
