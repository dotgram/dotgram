# Paired stand, 2026-09-20 01:05

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.7 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1017693.8 | 122525.0 | 0.12x | 125565.6 | 0.12x | +2.5% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 657832.8 | 121482.0 | 0.18x | 124850.8 | 0.19x | +2.8% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 672860.9 | 125625.8 | 0.19x | 125878.9 | 0.19x | +0.2% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2703346.9 | 490403.1 | 0.18x | 502356.2 | 0.19x | +2.4% | 454400 | 422403 |
| tsql/script400.boolboth | generated | scriptdom | 2780328.1 | 518856.2 | 0.19x | 513406.2 | 0.18x | -1.1% | 422403 | 422400 |
| tsql/script400 | generated | scriptdom | 2758184.4 | 498540.6 | 0.18x | 511021.9 | 0.19x | +2.5% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1781414.1 | 198856.2 | 0.11x | 198779.7 | 0.11x | 0.0% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 948885.9 | 345725.8 | 0.36x | 350842.2 | 0.37x | +1.5% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 636900.8 | 257602.3 | 0.40x | 258562.5 | 0.41x | +0.4% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7880.0 | 20006.8 | 2.54x | 18248.6 | 2.32x | -8.8% | 21416 | 21416 |
| sql/select20.window | generated | hand | 8122.3 | 20731.4 | 2.55x | 18827.1 | 2.32x | -9.2% | 21416 | 21416 |
| tsql/insert-values.at | generated | scriptdom | 8540.9 | 1226.9 | 0.14x | 1233.1 | 0.14x | +0.5% | 1328 | 1328 |
| sql/select20.scan | generated | control | 392.8 | 388.6 | 0.99x | 403.9 | 1.03x | +3.9% | 0 | 0 |
| sql/conditions100.scan | generated | control | 3590.5 | 3620.8 | 1.01x | 3707.7 | 1.03x | +2.4% | 0 | 0 |
| tsql/select20.scan | generated | control | 392.9 | 397.8 | 1.01x | 391.8 | 1.00x | -1.5% | 0 | 0 |
| el/ladder.scan | generated | control | 141.9 | 143.4 | 1.01x | 148.6 | 1.05x | +3.6% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 2979.4 | 13660.2 | 4.58x | 6128.3 | 2.06x | -55.1% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7877.3 | 20564.5 | 2.61x | 18599.8 | 2.36x | -9.6% | 21448 | 21392 |
| sql/literal | generated | hand | 53.5 | 133.1 | 2.49x | 138.6 | 2.59x | +4.2% | 160 | 160 |
| sql/comment | generated | hand | 2258.1 | 5015.6 | 2.22x | 4701.5 | 2.08x | -6.3% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 53960.0 | 126987.8 | 2.35x | 123862.4 | 2.30x | -2.5% | 161736 | 161760 |
| sql/conditions1000 | generated | hand | 537094.1 | 1280682.4 | 2.38x | 1243053.9 | 2.31x | -2.9% | 1616160 | 1616160 |
| tsql/comment | generated | scriptdom | 19400.6 | 1535.5 | 0.08x | 1554.1 | 0.08x | +1.2% | 1192 | 1192 |
| sql/column | generated | hand | 135.6 | 353.0 | 2.60x | 354.5 | 2.61x | +0.4% | 392 | 392 |
| sql/arithmetic | generated | hand | 1640.4 | 4203.7 | 2.56x | 4131.8 | 2.52x | -1.7% | 3472 | 3472 |
| sql/nest8 | generated | hand | 2875.3 | 11455.2 | 3.98x | 12203.0 | 4.24x | +6.5% | 6504 | 6504 |
| sql/condition | generated | hand | 1869.6 | 4312.7 | 2.31x | 4229.7 | 2.26x | -1.9% | 4928 | 4928 |
| sql/select1 | generated | hand | 692.1 | 1563.5 | 2.26x | 1470.2 | 2.12x | -6.0% | 1688 | 1688 |
| sql/select20 | generated | hand | 7583.8 | 19871.6 | 2.62x | 18174.1 | 2.40x | -8.5% | 21448 | 21448 |
| sql/values | generated | hand | 497.5 | 1766.9 | 3.55x | 1741.4 | 3.50x | -1.4% | 1904 | 1904 |
| sql/create | generated | hand | 779.5 | 2620.4 | 3.36x | 2632.9 | 3.38x | +0.5% | 1592 | 1592 |
| sql/refused-late | generated | hand | 2819.1 | 12998.8 | 4.61x | 12172.8 | 4.32x | -6.4% | 13552 | 13552 |
