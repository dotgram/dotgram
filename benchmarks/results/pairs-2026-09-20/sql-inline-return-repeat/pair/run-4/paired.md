# Paired stand, 2026-09-20 00:56

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/columns1000 | generated | scriptdom | 1665143.8 | 168331.2 | 0.10x | 168206.2 | 0.10x | -0.1% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 882541.4 | 303811.7 | 0.34x | 306191.4 | 0.35x | +0.8% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 598664.1 | 221426.6 | 0.37x | 227776.6 | 0.38x | +2.9% | 296472 | 296472 |
| sql/select20.at | generated | hand | 6931.3 | 19338.9 | 2.79x | 18392.1 | 2.65x | -4.9% | 21441 | 21441 |
| sql/select20.window | generated | hand | 6904.6 | 19629.7 | 2.84x | 18483.7 | 2.68x | -5.8% | 21440 | 21440 |
| sql/select20.scan | generated | control | 378.5 | 386.3 | 1.02x | 390.3 | 1.03x | +1.0% | 0 | 0 |
| tsql/select20.scan | generated | control | 382.0 | 396.0 | 1.04x | 383.2 | 1.00x | -3.2% | 0 | 0 |
| sql/select20.bool | generated | hand | 6776.1 | 19360.8 | 2.86x | 18255.1 | 2.69x | -5.7% | 21448 | 21392 |
| tsql/comment | generated | scriptdom | 19674.1 | 1614.4 | 0.08x | 1693.5 | 0.09x | +4.9% | 1192 | 1192 |
| sql/select20 | generated | hand | 6804.8 | 19383.2 | 2.85x | 18379.2 | 2.70x | -5.2% | 21448 | 21448 |
