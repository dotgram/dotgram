# Paired stand, 2026-09-21 01:57

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.4 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 244350.0 | 209675.0 | 0.86x | 258975.0 | 1.06x | +23.5% | 403288 | 403288 | 27 % |  |
| tsql/columns1000 | generated | scriptdom | 1659043.8 | 171993.8 | 0.10x | 169693.8 | 0.10x | -1.3% | 200489 | 200489 | 5 % |  |
| web/json.array10000 | generated | hand | 188276.6 | 183760.2 | 0.98x | 184669.5 | 0.98x | +0.5% | 720048 | 720048 | 17 % |  |
| sql/select20.at | generated | hand | 6606.3 | 17287.1 | 2.62x | 17245.1 | 2.61x | -0.2% | 21416 | 21416 | 11 % |  |
| sql/select20.window | generated | hand | 6605.6 | 17545.2 | 2.66x | 17656.7 | 2.67x | +0.6% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 366.9 | 366.7 | 1.00x | 371.5 | 1.01x | +1.3% | 0 | 0 | 13 % |  |
| tsql/select20.scan | generated | control | 368.8 | 369.9 | 1.00x | 372.5 | 1.01x | +0.7% | 0 | 0 | 6 % |  |
| el/ladder.scan | generated | control | 136.7 | 134.0 | 0.98x | 135.1 | 0.99x | +0.8% | 0 | 0 | 11 % |  |
| el/ladder.bool | generated | hand | 946.9 | 1717.9 | 1.81x | 1728.2 | 1.83x | +0.6% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2517.9 | 11666.1 | 4.63x | 5715.3 | 2.27x | -51.0% | 13552 | 6616 | 10 % |  |
| sql/select20.bool | generated | hand | 6711.8 | 17610.4 | 2.62x | 17727.6 | 2.64x | +0.7% | 21448 | 21392 | 12 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4449087.5 | 4469787.5 | 1.00x | 4433300.0 | 1.00x | -0.8% | 6276488 | 6276488 | 11 % |  |
| sql/refused-cliff-case-1738 | generated | control | 16499900.0 | 11244625.0 | 0.68x | 5455100.0 | 0.33x | -51.5% | 74362418 | 7842305 | 133 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2511793.8 | 2594131.2 | 1.03x | 2523246.9 | 1.00x | -2.7% | 2994166 | 2994166 | 4 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 5199665.6 | 5676531.2 | 1.09x | 3181968.8 | 0.61x | -43.9% | 12948506 | 3740064 | 96 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 8882756.2 | 9281368.8 | 1.04x | 5092625.0 | 0.57x | -45.1% | 21143760 | 5840151 | 38 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 10145525.0 | 9504075.0 | 0.94x | 7043750.0 | 0.69x | -25.9% | 94724859 | 7298283 | 247 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 7899650.0 | 7727318.8 | 0.98x | 7264287.5 | 0.92x | -6.0% | 8778600 | 8778600 | 22 % |  |
| sql/refused-cliff-and-2715 | generated | control | 7127525.0 | 7481362.5 | 1.05x | 7913325.0 | 1.11x | +5.8% | 8775448 | 8775232 | 21 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 10804925.0 | 11311700.0 | 1.05x | 7931300.0 | 0.73x | -29.9% | 94857748 | 10969680 | 335 % |  |
| sql/refused-cliff-and-3393 | generated | control | 11272100.0 | 10886500.0 | 0.97x | 8039500.0 | 0.71x | -26.2% | 94854700 | 10966571 | 46 % |  |
| el/parse-200k-tokens | generated | control | 15211137.5 | 16158737.5 | 1.06x | 16070687.5 | 1.06x | -0.5% | 16801636 | 16801593 | 22 % |  |
| el/parse-500k-tokens | generated | control | 40313450.0 | 39615850.0 | 0.98x | 38999600.0 | 0.97x | -1.6% | 49503175 | 42002310 | 40 % |  |
| sql/worst-columns-100k | generated | control | 430364150.0 | 419050500.0 | 0.97x | 278259600.0 | 0.65x | -33.6% | 999698498 | 95920876 | 19 % |  |
| fix/Orders128.yield-string | generated | hand | 76863.7 | 246534.4 | 3.21x | 254701.8 | 3.31x | +3.3% | 117936 | 117936 | 7 % |  |
| web/media-type.quoted | generated | control | 239.0 | 290.8 | 1.22x | 279.4 | 1.17x | -3.9% | 816 | 816 | 4 % |  |
| el/ladder | generated | hand | 949.3 | 1738.5 | 1.83x | 1751.7 | 1.85x | +0.8% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 949.3 | 1143.3 | 1.20x | 1166.7 | 1.23x | +2.0% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 102328.6 | 142867.6 | 1.40x | 140439.0 | 1.37x | -1.7% | 169104 | 169104 | 17 % |  |
| el/terms1000 | immediate | hand | 102328.6 | 114730.0 | 1.12x | 114856.2 | 1.12x | +0.1% | 177120 | 177123 | 17 % |  |
| sql/select20 | generated | hand | 6653.1 | 17628.2 | 2.65x | 17709.0 | 2.66x | +0.5% | 21448 | 21448 | 6 % |  |
| sql/refused-late | generated | hand | 2576.4 | 12052.7 | 4.68x | 11901.7 | 4.62x | -1.3% | 13552 | 13552 | 14 % |  |
