# Paired stand, 2026-09-19 23:14

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | hand | 8154.5 | 21753.7 | 2.67x | 21327.9 | 2.62x | -2.0% | 21416 | 21416 |
| sql/select20.window | generated | hand | 8536.0 | 23662.1 | 2.77x | 23519.5 | 2.76x | -0.6% | 21416 | 21416 |
| sql/select20.scan | generated | control | 389.3 | 394.5 | 1.01x | 396.6 | 1.02x | +0.5% | 0 | 0 |
| tsql/select20.scan | generated | control | 384.9 | 395.1 | 1.03x | 395.7 | 1.03x | +0.2% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 3347.4 | 15586.6 | 4.66x | 7614.8 | 2.27x | -51.1% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 8461.9 | 23345.2 | 2.76x | 23355.2 | 2.76x | 0.0% | 21448 | 21392 |
| sql/select20 | generated | hand | 8574.9 | 23034.4 | 2.69x | 23152.1 | 2.70x | +0.5% | 21448 | 21448 |
| sql/refused-late | generated | hand | 3286.8 | 15569.7 | 4.74x | 15612.4 | 4.75x | +0.3% | 13552 | 13552 |
