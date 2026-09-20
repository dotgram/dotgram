# Paired stand, 2026-09-20 00:54

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/columns1000 | generated | scriptdom | 1733425.0 | 174100.0 | 0.10x | 173100.0 | 0.10x | -0.6% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 879630.5 | 301247.7 | 0.34x | 300464.1 | 0.34x | -0.3% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 593158.6 | 222901.6 | 0.38x | 226107.8 | 0.38x | +1.4% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7032.0 | 18478.4 | 2.63x | 18617.3 | 2.65x | +0.8% | 21441 | 21441 |
| sql/select20.window | generated | hand | 6905.5 | 18325.6 | 2.65x | 18645.5 | 2.70x | +1.7% | 21440 | 21440 |
| sql/select20.scan | generated | control | 380.4 | 386.8 | 1.02x | 381.7 | 1.00x | -1.3% | 0 | 0 |
| tsql/select20.scan | generated | control | 375.4 | 379.1 | 1.01x | 383.5 | 1.02x | +1.2% | 0 | 0 |
| sql/select20.bool | generated | hand | 6776.1 | 18185.7 | 2.68x | 18430.6 | 2.72x | +1.3% | 21448 | 21392 |
| tsql/comment | generated | scriptdom | 19771.1 | 1638.3 | 0.08x | 1626.2 | 0.08x | -0.7% | 1192 | 1192 |
| sql/select20 | generated | hand | 6825.2 | 18235.8 | 2.67x | 18828.4 | 2.76x | +3.2% | 21448 | 21448 |
