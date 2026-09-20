# Paired stand, 2026-09-20 03:43

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.2 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 5290.4 | 10527.0 | 1.99x | 10579.7 | 2.00x | +0.5% | 8384 | 8384 | 5 % |  |
| web/url.plain | generated | hand | 167.0 | 329.2 | 1.97x | 381.8 | 2.29x | +16.0% | 360 | 360 | 49 % |  |
| web/url.full | generated | hand | 293.2 | 481.4 | 1.64x | 507.8 | 1.73x | +5.5% | 536 | 536 | 60 % |  |
| web/url.ipv4 | generated | hand | 226.9 | 421.1 | 1.86x | 424.6 | 1.87x | +0.8% | 384 | 384 | 36 % |  |
| web/url.long-path | generated | hand | 177.8 | 421.9 | 2.37x | 415.9 | 2.34x | -1.4% | 512 | 512 | 75 % |  |
| web/url.refused | generated | hand | 68.2 | 249.5 | 3.66x | 246.4 | 3.61x | -1.2% | 64 | 64 | 6 % |  |
| web/url.relative | generated | hand | 77.2 | 177.2 | 2.30x | 184.7 | 2.39x | +4.2% | 312 | 312 | 4 % |  |
| web/url.relative-dot-colon | generated | hand | 63.7 | 152.8 | 2.40x | 161.4 | 2.53x | +5.7% | 280 | 280 | 4 % |  |
| web/url.relative-letters | generated | hand | 106.9 | 209.8 | 1.96x | 219.9 | 2.06x | +4.8% | 312 | 312 | 8 % |  |
