Median of 9 of 9 runs, each in a process of its own; control 31.5 ns (the runs' controls: 32.2, 31.5, 31.4, 31.4, 31.5, 31.5, 31.5, 31.4, 31.5).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-20 03:49

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 5280.4 | 10582.2 | 2.00x | 10579.7 | 2.00x | 0.0% | 8384 | 8384 | 4 % | [-1.9%..+1.0%], 4 of 9 positive |
| web/url.plain | generated | hand | 95.6 | 217.8 | 2.28x | 216.3 | 2.26x | -0.7% | 360 | 360 | 75 % | [-6.2%..+16.0%], 4 of 9 positive |
| web/url.full | generated | hand | 169.3 | 295.4 | 1.74x | 286.3 | 1.69x | -3.1% | 536 | 536 | 74 % | [-6.1%..+5.5%], 1 of 9 positive |
| web/url.ipv4 | generated | hand | 113.4 | 222.8 | 1.96x | 230.2 | 2.03x | +3.3% | 384 | 384 | 101 % | [-2.0%..+8.7%], 8 of 9 positive |
| web/url.long-path | generated | hand | 177.5 | 419.0 | 2.36x | 415.3 | 2.34x | -0.9% | 512 | 512 | 3 % | [-6.1%..+7.4%], 4 of 9 positive |
| web/url.refused | generated | hand | 68.5 | 254.9 | 3.72x | 255.0 | 3.72x | +0.1% | 64 | 64 | 4 % | [-5.1%..+1.9%], 4 of 9 positive |
| web/url.relative | generated | hand | 76.1 | 181.5 | 2.38x | 184.2 | 2.42x | +1.5% | 312 | 312 | 8 % | [-0.3%..+4.2%], 8 of 9 positive |
| web/url.relative-dot-colon | generated | hand | 63.4 | 155.8 | 2.46x | 160.9 | 2.54x | +3.3% | 280 | 280 | 2 % | [-3.0%..+10.1%], 8 of 9 positive |
| web/url.relative-letters | generated | hand | 106.8 | 209.8 | 1.96x | 217.0 | 2.03x | +3.4% | 312 | 312 | 5 % | [-1.1%..+7.2%], 7 of 9 positive |
