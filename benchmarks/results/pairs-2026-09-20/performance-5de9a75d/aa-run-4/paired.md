# Paired stand, 2026-09-20 23:54

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

This binary, and the libraries it holds as the control, was built from b746f489. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 235140.6 | 210106.2 | 0.89x | 217965.6 | 0.93x | +3.7% | 403288 | 403288 | 4 % |  |
| tsql/columns1000 | generated | scriptdom | 1722618.8 | 168643.8 | 0.10x | 170256.2 | 0.10x | +1.0% | 200489 | 200489 | 6 % |  |
| web/json.array10000 | generated | hand | 172307.0 | 190400.8 | 1.11x | 190132.8 | 1.10x | -0.1% | 720048 | 720048 | 39 % |  |
| sql/select20.at | generated | hand | 6900.1 | 17838.8 | 2.59x | 17952.6 | 2.60x | +0.6% | 21416 | 21416 | 18 % |  |
| sql/select20.window | generated | hand | 6911.0 | 18911.5 | 2.74x | 18582.8 | 2.69x | -1.7% | 21416 | 21416 | 5 % |  |
| sql/select20.scan | generated | control | 387.4 | 385.4 | 0.99x | 381.1 | 0.98x | -1.1% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 385.4 | 392.7 | 1.02x | 385.3 | 1.00x | -1.9% | 0 | 0 | 12 % |  |
| el/ladder.scan | generated | control | 142.4 | 141.4 | 0.99x | 140.5 | 0.99x | -0.6% | 0 | 0 | 14 % |  |
| el/ladder.bool | generated | hand | 990.9 | 1729.7 | 1.75x | 1791.9 | 1.81x | +3.6% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2615.7 | 12551.2 | 4.80x | 6046.4 | 2.31x | -51.8% | 13552 | 6616 | 10 % |  |
| sql/select20.bool | generated | hand | 6941.7 | 18418.6 | 2.65x | 18535.6 | 2.67x | +0.6% | 21448 | 21392 | 5 % |  |
| fix/Orders128.yield-string | generated | hand | 81551.1 | 262823.8 | 3.22x | 261154.2 | 3.20x | -0.6% | 117936 | 117936 | 16 % |  |
| web/media-type.quoted | generated | control | 254.5 | 316.3 | 1.24x | 292.8 | 1.15x | -7.4% | 816 | 816 | 12 % |  |
| el/ladder | generated | hand | 996.2 | 1746.9 | 1.75x | 1766.9 | 1.77x | +1.1% | 1776 | 1776 | 7 % |  |
| el/ladder | immediate | hand | 996.2 | 1196.6 | 1.20x | 1192.2 | 1.20x | -0.4% | 1784 | 1784 | 7 % |  |
| el/terms1000 | generated | hand | 111270.6 | 152245.3 | 1.37x | 156753.3 | 1.41x | +3.0% | 169107 | 169104 | 6 % |  |
| el/terms1000 | immediate | hand | 111270.6 | 125753.9 | 1.13x | 131534.6 | 1.18x | +4.6% | 177120 | 177120 | 6 % |  |
| sql/select20 | generated | hand | 6906.3 | 18596.4 | 2.69x | 18547.9 | 2.69x | -0.3% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2619.7 | 12553.1 | 4.79x | 12564.6 | 4.80x | +0.1% | 13552 | 13552 | 13 % |  |
