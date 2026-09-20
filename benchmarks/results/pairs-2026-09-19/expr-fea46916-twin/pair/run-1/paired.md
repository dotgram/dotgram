# Paired stand, 2026-09-19 23:07

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | hand | 6595.5 | 17291.3 | 2.62x | 16704.5 | 2.53x | -3.4% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7077.7 | 18845.5 | 2.66x | 18716.5 | 2.64x | -0.7% | 21416 | 21416 |
| sql/select20.scan | generated | control | 384.7 | 391.6 | 1.02x | 382.5 | 0.99x | -2.3% | 0 | 0 |
| tsql/select20.scan | generated | control | 487.7 | 383.5 | 0.79x | 381.6 | 0.78x | -0.5% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 2626.9 | 12299.3 | 4.68x | 5927.4 | 2.26x | -51.8% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 6857.5 | 18172.2 | 2.65x | 18276.9 | 2.67x | +0.6% | 21448 | 21392 |
| sql/select20 | generated | hand | 7010.8 | 18539.5 | 2.64x | 18480.7 | 2.64x | -0.3% | 21448 | 21448 |
| sql/refused-late | generated | hand | 2622.3 | 12368.7 | 4.72x | 12543.3 | 4.78x | +1.4% | 13552 | 13552 |
