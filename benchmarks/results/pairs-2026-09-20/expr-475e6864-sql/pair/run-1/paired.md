# Paired stand, 2026-09-20 01:01

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.8 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| tsql/script100.bool | generated | scriptdom | 1034981.2 | 126850.0 | 0.12x | 126903.1 | 0.12x | 0.0% | 113600 | 105600 |
| tsql/script100.boolboth | generated | scriptdom | 680360.9 | 125964.8 | 0.19x | 132321.9 | 0.19x | +5.0% | 105600 | 105600 |
| tsql/script100 | generated | scriptdom | 681843.8 | 127887.5 | 0.19x | 132089.8 | 0.19x | +3.3% | 113600 | 113600 |
| tsql/script400.bool | generated | scriptdom | 2730431.2 | 504337.5 | 0.18x | 501578.1 | 0.18x | -0.5% | 454400 | 422400 |
| tsql/script400.boolboth | generated | scriptdom | 2750978.1 | 499006.2 | 0.18x | 504437.5 | 0.18x | +1.1% | 422403 | 422403 |
| tsql/script400 | generated | scriptdom | 2784215.6 | 507196.9 | 0.18x | 521300.0 | 0.19x | +2.8% | 454400 | 454400 |
| tsql/columns1000 | generated | scriptdom | 1749504.7 | 192950.0 | 0.11x | 192525.0 | 0.11x | -0.2% | 200464 | 200464 |
| tsql/conditions1000 | generated | scriptdom | 981102.3 | 349747.7 | 0.36x | 351918.0 | 0.36x | +0.6% | 424424 | 424424 |
| tsql/rows1000 | generated | scriptdom | 611393.8 | 252294.5 | 0.41x | 252466.4 | 0.41x | +0.1% | 296472 | 296472 |
| sql/select20.at | generated | hand | 7595.5 | 18191.5 | 2.40x | 18352.1 | 2.42x | +0.9% | 21416 | 21416 |
| sql/select20.window | generated | hand | 7556.5 | 19285.9 | 2.55x | 18772.5 | 2.48x | -2.7% | 21441 | 21441 |
| tsql/insert-values.at | generated | scriptdom | 8202.9 | 1178.2 | 0.14x | 1258.7 | 0.15x | +6.8% | 1328 | 1328 |
| sql/select20.scan | generated | control | 431.6 | 385.8 | 0.89x | 383.9 | 0.89x | -0.5% | 0 | 0 |
| sql/conditions100.scan | generated | control | 4005.9 | 3661.5 | 0.91x | 3581.0 | 0.89x | -2.2% | 0 | 0 |
| tsql/select20.scan | generated | control | 387.6 | 385.0 | 0.99x | 386.4 | 1.00x | +0.4% | 0 | 0 |
| el/ladder.scan | generated | control | 143.9 | 142.3 | 0.99x | 141.2 | 0.98x | -0.8% | 0 | 0 |
| sql/refused-late.bool | generated | hand | 2790.6 | 12542.8 | 4.49x | 5990.7 | 2.15x | -52.2% | 13552 | 6616 |
| sql/select20.bool | generated | hand | 7533.8 | 18508.0 | 2.46x | 18525.6 | 2.46x | +0.1% | 21448 | 21392 |
| sql/literal | generated | hand | 52.9 | 140.4 | 2.65x | 136.5 | 2.58x | -2.7% | 160 | 160 |
| sql/comment | generated | hand | 2159.6 | 4639.0 | 2.15x | 4688.2 | 2.17x | +1.1% | 5136 | 5136 |
| sql/conditions100 | generated | hand | 49914.2 | 124144.9 | 2.49x | 123567.4 | 2.48x | -0.5% | 161760 | 161760 |
| sql/conditions1000 | generated | hand | 492649.2 | 1227974.2 | 2.49x | 1228970.3 | 2.49x | +0.1% | 1616160 | 1616160 |
| tsql/comment | generated | scriptdom | 20277.0 | 1586.0 | 0.08x | 1563.1 | 0.08x | -1.4% | 1192 | 1192 |
| sql/column | generated | hand | 136.6 | 357.0 | 2.61x | 345.4 | 2.53x | -3.2% | 392 | 392 |
| sql/arithmetic | generated | hand | 1584.2 | 4250.1 | 2.68x | 4335.7 | 2.74x | +2.0% | 3472 | 3472 |
| sql/nest8 | generated | hand | 2744.7 | 11303.9 | 4.12x | 12191.3 | 4.44x | +7.8% | 6504 | 6504 |
| sql/condition | generated | hand | 1779.8 | 4332.0 | 2.43x | 4373.8 | 2.46x | +1.0% | 4928 | 4928 |
| sql/select1 | generated | hand | 679.7 | 1482.3 | 2.18x | 1512.4 | 2.22x | +2.0% | 1688 | 1688 |
| sql/select20 | generated | hand | 7157.6 | 18152.1 | 2.54x | 18032.0 | 2.52x | -0.7% | 21448 | 21448 |
| sql/values | generated | hand | 449.9 | 1661.6 | 3.69x | 1751.3 | 3.89x | +5.4% | 1904 | 1904 |
| sql/create | generated | hand | 774.7 | 2617.2 | 3.38x | 2582.8 | 3.33x | -1.3% | 1568 | 1568 |
| sql/refused-late | generated | hand | 2669.2 | 12289.3 | 4.60x | 12352.5 | 4.63x | +0.5% | 13552 | 13552 |
