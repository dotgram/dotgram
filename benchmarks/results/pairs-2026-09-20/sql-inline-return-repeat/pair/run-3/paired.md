# Paired stand, 2026-09-20 00:56

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/columns1000 | generated | scriptdom | 2167031.2 | 167968.8 | 0.08x | 169281.2 | 0.08x | +0.8% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 869260.9 | 298587.5 | 0.34x | 301878.1 | 0.35x | +1.1% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 599561.7 | 226053.1 | 0.38x | 224353.1 | 0.37x | -0.8% | 296472 | 296472 |
| sql/select20.at | generated | hand | 6816.4 | 18169.4 | 2.67x | 18247.6 | 2.68x | +0.4% | 21416 | 21416 |
| sql/select20.window | generated | hand | 6916.4 | 18729.1 | 2.71x | 19090.5 | 2.76x | +1.9% | 21441 | 21441 |
| sql/select20.scan | generated | control | 379.2 | 381.2 | 1.01x | 387.2 | 1.02x | +1.6% | 0 | 0 |
| tsql/select20.scan | generated | control | 375.0 | 389.7 | 1.04x | 378.1 | 1.01x | -3.0% | 0 | 0 |
| sql/select20.bool | generated | hand | 6842.3 | 18145.9 | 2.65x | 18381.9 | 2.69x | +1.3% | 21448 | 21392 |
| tsql/comment | generated | scriptdom | 20105.1 | 1592.5 | 0.08x | 1587.3 | 0.08x | -0.3% | 1192 | 1192 |
| sql/select20 | generated | hand | 6778.0 | 18177.0 | 2.68x | 18163.1 | 2.68x | -0.1% | 21448 | 21448 |
