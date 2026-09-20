# Paired stand, 2026-09-20 03:07

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

Sides: before (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486); after (commit d2e10448, framework net10.0, no properties, emitted 243ac9900d128486). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/script100.bool | generated | scriptdom | 1539900.0 | 160509.4 | 0.10x | 152793.8 | 0.10x | -4.8% | 113600 | 105600 | 291 % |  |
| tsql/script100.boolboth | generated | scriptdom | 663297.7 | 125885.2 | 0.19x | 121401.6 | 0.18x | -3.6% | 105600 | 105600 | 5 % |  |
| tsql/script100 | generated | scriptdom | 660351.6 | 131008.6 | 0.20x | 124138.3 | 0.19x | -5.2% | 113600 | 113600 | 2 % |  |
| tsql/script400.bool | generated | scriptdom | 2650534.4 | 507109.4 | 0.19x | 480478.1 | 0.18x | -5.3% | 454400 | 422400 | 7 % |  |
| tsql/script400.boolboth | generated | scriptdom | 2691337.5 | 495500.0 | 0.18x | 480259.4 | 0.18x | -3.1% | 422403 | 422400 | 6 % |  |
| tsql/script400 | generated | scriptdom | 2688815.6 | 506700.0 | 0.19x | 496475.0 | 0.18x | -2.0% | 454400 | 454400 | 16 % |  |
| tsql/columns1000 | generated | scriptdom | 1755442.2 | 191596.9 | 0.11x | 190210.9 | 0.11x | -0.7% | 200464 | 200464 | 21 % |  |
| tsql/conditions1000 | generated | scriptdom | 882516.4 | 334547.7 | 0.38x | 340676.6 | 0.39x | +1.8% | 424424 | 424424 | 17 % |  |
| tsql/rows1000 | generated | scriptdom | 595472.7 | 245729.7 | 0.41x | 247042.2 | 0.41x | +0.5% | 296472 | 296472 | 51 % |  |
| sql/select20.at | generated | hand | 7222.9 | 18488.7 | 2.56x | 17688.6 | 2.45x | -4.3% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 7119.6 | 18924.8 | 2.66x | 18019.8 | 2.53x | -4.8% | 21416 | 21416 | 11 % |  |
| tsql/insert-values.at | generated | scriptdom | 8465.4 | 1206.3 | 0.14x | 1179.7 | 0.14x | -2.2% | 1328 | 1328 | 15 % |  |
| sql/select20.scan | generated | control | 380.2 | 425.3 | 1.12x | 381.7 | 1.00x | -10.3% | 0 | 0 | 7 % |  |
| sql/conditions100.scan | generated | control | 3511.6 | 3764.7 | 1.07x | 3541.7 | 1.01x | -5.9% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 375.4 | 384.0 | 1.02x | 386.8 | 1.03x | +0.7% | 0 | 0 | 7 % |  |
| el/ladder.scan | generated | control | 137.9 | 141.7 | 1.03x | 137.9 | 1.00x | -2.7% | 0 | 0 | 12 % |  |
| sql/refused-late.bool | generated | hand | 2645.3 | 12542.3 | 4.74x | 5865.8 | 2.22x | -53.2% | 13552 | 6616 | 12 % |  |
| sql/select20.bool | generated | hand | 9594.1 | 21294.3 | 2.22x | 19394.0 | 2.02x | -8.9% | 21448 | 21392 | 51 % |  |
| sql/literal | generated | hand | 52.2 | 137.4 | 2.63x | 131.2 | 2.52x | -4.5% | 160 | 160 | 5 % |  |
| sql/comment | generated | hand | 2172.6 | 4756.2 | 2.19x | 4631.1 | 2.13x | -2.6% | 5136 | 5136 | 13 % |  |
| sql/conditions100 | generated | hand | 51550.5 | 123706.4 | 2.40x | 128345.3 | 2.49x | +3.7% | 161736 | 161736 | 4 % |  |
| sql/conditions1000 | generated | hand | 515449.6 | 1245356.6 | 2.42x | 1265452.7 | 2.46x | +1.6% | 1616160 | 1616160 | 13 % |  |
| tsql/comment | generated | scriptdom | 20279.3 | 1640.8 | 0.08x | 1590.1 | 0.08x | -3.1% | 1192 | 1192 | 6 % |  |
| sql/column | generated | hand | 140.0 | 354.3 | 2.53x | 358.7 | 2.56x | +1.2% | 392 | 392 | 22 % |  |
| sql/arithmetic | generated | hand | 1637.9 | 4299.6 | 2.63x | 4273.3 | 2.61x | -0.6% | 3472 | 3472 | 9 % |  |
| sql/nest8 | generated | hand | 2809.7 | 12341.4 | 4.39x | 12214.1 | 4.35x | -1.0% | 6504 | 6504 | 11 % |  |
| sql/condition | generated | hand | 1811.1 | 4330.3 | 2.39x | 4355.1 | 2.40x | +0.6% | 4928 | 4928 | 4 % |  |
| sql/select1 | generated | hand | 695.2 | 1544.6 | 2.22x | 1534.7 | 2.21x | -0.6% | 1688 | 1688 | 10 % |  |
| sql/select20 | generated | hand | 7280.0 | 19120.6 | 2.63x | 18370.7 | 2.52x | -3.9% | 21448 | 21448 | 9 % |  |
| sql/values | generated | hand | 454.7 | 1729.7 | 3.80x | 1779.6 | 3.91x | +2.9% | 1904 | 1904 | 2 % |  |
| sql/create | generated | hand | 780.0 | 2661.4 | 3.41x | 2602.5 | 3.34x | -2.2% | 1568 | 1568 | 2 % |  |
| sql/refused-late | generated | hand | 2675.1 | 12626.2 | 4.72x | 12364.7 | 4.62x | -2.1% | 13552 | 13552 | 6 % |  |
