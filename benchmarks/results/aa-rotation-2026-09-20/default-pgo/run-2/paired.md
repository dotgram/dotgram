# Paired stand, 2026-09-20 03:44

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/url.path1000 | generated | hand | 5166.4 | 10543.7 | 2.04x | 10497.3 | 2.03x | -0.4% | 8384 | 8384 | 5 % |  |
| web/url.plain | generated | hand | 95.5 | 215.1 | 2.25x | 217.9 | 2.28x | +1.3% | 360 | 360 | 11 % |  |
| web/url.full | generated | hand | 167.3 | 292.4 | 1.75x | 291.5 | 1.74x | -0.3% | 536 | 536 | 11 % |  |
| web/url.ipv4 | generated | hand | 113.6 | 222.8 | 1.96x | 236.8 | 2.08x | +6.3% | 384 | 384 | 5 % |  |
| web/url.long-path | generated | hand | 178.6 | 410.4 | 2.30x | 415.3 | 2.33x | +1.2% | 512 | 512 | 16 % |  |
| web/url.refused | generated | hand | 70.3 | 262.4 | 3.73x | 258.0 | 3.67x | -1.7% | 64 | 64 | 65 % |  |
| web/url.relative | generated | hand | 76.0 | 176.3 | 2.32x | 183.3 | 2.41x | +4.0% | 312 | 312 | 67 % |  |
| web/url.relative-dot-colon | generated | hand | 63.1 | 153.9 | 2.44x | 158.7 | 2.51x | +3.1% | 280 | 280 | 6 % |  |
| web/url.relative-letters | generated | hand | 105.5 | 205.6 | 1.95x | 214.5 | 2.03x | +4.3% | 312 | 312 | 19 % |  |
