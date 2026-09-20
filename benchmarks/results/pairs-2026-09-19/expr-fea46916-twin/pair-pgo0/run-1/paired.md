# Paired stand, 2026-09-19 23:11

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.6 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| sql/select20.at | generated | hand | 8577.1 | 22566.2 | 2.63x | 22121.5 | 2.58x | -2.0% | 21416 | 21416 |
| sql/select20.window | generated | hand | 8610.2 | 23231.7 | 2.70x | 23004.6 | 2.67x | -1.0% | 21416 | 21416 |
| sql/select20.scan | generated | control | 385.8 | 393.8 | 1.02x | 392.1 | 1.02x | -0.4% | 0 | 0 |
| tsql/select20.scan | generated | control | 392.5 | 389.8 | 0.99x | 395.4 | 1.01x | +1.4% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 3354.6 | 15072.5 | 4.49x | 7369.0 | 2.20x | -51.1% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 8403.3 | 22630.9 | 2.69x | 22833.2 | 2.72x | +0.9% | 21448 | 21392 |
| sql/select20 | generated | hand | 8447.8 | 23388.2 | 2.77x | 23177.6 | 2.74x | -0.9% | 21448 | 21448 |
| sql/refused-late | generated | hand | 3358.5 | 15142.2 | 4.51x | 15331.2 | 4.56x | +1.2% | 13552 | 13552 |
