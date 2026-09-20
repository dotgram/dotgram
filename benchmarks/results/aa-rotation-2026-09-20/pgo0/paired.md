Median of 9 of 9 runs, each in a process of its own; control 31.1 ns (the runs' controls: 31.1, 31.2, 31.6, 31.3, 31.0, 31.0, 31.1, 31.1, 31.1).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 03:56

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 15732.5 | 12830.0 | 0.82x | 12881.1 | 0.82x | +0.4% | 8384 | 8384 | 2 % | [-0.8%..+0.8%], 4 of 9 positive |
| web/url.plain | generated | hand | 181.9 | 261.8 | 1.44x | 266.5 | 1.47x | +1.8% | 360 | 360 | 2 % | [-4.3%..+17.5%], 5 of 9 positive |
| web/url.full | generated | hand | 289.7 | 361.8 | 1.25x | 368.8 | 1.27x | +1.9% | 536 | 536 | 3 % | [-4.5%..+13.2%], 7 of 9 positive |
| web/url.ipv4 | generated | hand | 196.6 | 268.3 | 1.36x | 267.2 | 1.36x | -0.4% | 384 | 384 | 1 % | [-5.3%..+13.8%], 3 of 9 positive |
| web/url.long-path | generated | hand | 434.1 | 483.8 | 1.11x | 482.8 | 1.11x | -0.2% | 512 | 512 | 34 % | [-1.8%..+7.8%], 4 of 9 positive |
| web/url.refused | generated | hand | 112.6 | 289.5 | 2.57x | 296.4 | 2.63x | +2.4% | 64 | 64 | 2 % | [+0.2%..+14.9%], 9 of 9 positive |
| web/url.relative | generated | hand | 142.7 | 230.9 | 1.62x | 231.0 | 1.62x | 0.0% | 312 | 312 | 3 % | [-2.5%..+2.4%], 5 of 9 positive |
| web/url.relative-dot-colon | generated | hand | 110.5 | 201.7 | 1.83x | 202.2 | 1.83x | +0.3% | 280 | 280 | 2 % | [-3.2%..+1.6%], 5 of 9 positive |
| web/url.relative-letters | generated | hand | 241.7 | 253.1 | 1.05x | 257.0 | 1.06x | +1.5% | 312 | 312 | 2 % | [-1.0%..+3.8%], 6 of 9 positive |
