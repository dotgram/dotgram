# Paired stand, 2026-09-20 03:46

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 5280.4 | 10540.7 | 2.00x | 10650.5 | 2.02x | +1.0% | 8384 | 8384 | 4 % |  |
| web/url.plain | generated | hand | 101.7 | 230.6 | 2.27x | 216.3 | 2.13x | -6.2% | 360 | 360 | 59 % |  |
| web/url.full | generated | hand | 168.5 | 297.7 | 1.77x | 283.7 | 1.68x | -4.7% | 536 | 536 | 15 % |  |
| web/url.ipv4 | generated | hand | 120.2 | 228.7 | 1.90x | 224.0 | 1.86x | -2.0% | 384 | 384 | 5 % |  |
| web/url.long-path | generated | hand | 181.4 | 422.2 | 2.33x | 418.1 | 2.30x | -1.0% | 512 | 512 | 11 % |  |
| web/url.refused | generated | hand | 68.8 | 248.7 | 3.62x | 249.1 | 3.62x | +0.2% | 64 | 64 | 56 % |  |
| web/url.relative | generated | hand | 76.0 | 184.8 | 2.43x | 184.2 | 2.42x | -0.3% | 312 | 312 | 7 % |  |
| web/url.relative-dot-colon | generated | hand | 63.9 | 159.8 | 2.50x | 155.0 | 2.42x | -3.0% | 280 | 280 | 15 % |  |
| web/url.relative-letters | generated | hand | 106.1 | 209.2 | 1.97x | 217.0 | 2.04x | +3.7% | 312 | 312 | 13 % |  |
