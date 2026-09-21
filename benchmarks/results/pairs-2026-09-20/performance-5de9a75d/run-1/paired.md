# Paired stand, 2026-09-20 23:29

IGOR-DESKTOP, pinned to 0-15, high priority, control 32.0 ns.

This binary, and the libraries it holds as the control, was built from b746f489. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5de9a75d, framework net10.0, no properties, emitted 194b78b7193dfafe). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 254312.5 | 214812.5 | 0.84x | 226518.8 | 0.89x | +5.4% | 403288 | 403288 | 61 % |  |
| tsql/columns1000 | generated | scriptdom | 1768281.2 | 175006.2 | 0.10x | 180025.0 | 0.10x | +2.9% | 200489 | 200489 | 10 % |  |
| web/json.array10000 | generated | hand | 173215.6 | 192368.8 | 1.11x | 195207.8 | 1.13x | +1.5% | 720048 | 720048 | 25 % |  |
| sql/select20.at | generated | hand | 7500.9 | 19245.4 | 2.57x | 19214.5 | 2.56x | -0.2% | 21416 | 21416 | 53 % |  |
| sql/select20.window | generated | hand | 7187.4 | 18910.9 | 2.63x | 19205.5 | 2.67x | +1.6% | 21416 | 21416 | 9 % |  |
| sql/select20.scan | generated | control | 387.9 | 387.7 | 1.00x | 399.7 | 1.03x | +3.1% | 0 | 0 | 21 % |  |
| tsql/select20.scan | generated | control | 399.5 | 400.2 | 1.00x | 395.0 | 0.99x | -1.3% | 0 | 0 | 53 % |  |
| el/ladder.scan | generated | control | 149.2 | 145.3 | 0.97x | 145.9 | 0.98x | +0.4% | 0 | 0 | 16 % |  |
| el/ladder.bool | generated | hand | 1017.5 | 1803.6 | 1.77x | 1942.0 | 1.91x | +7.7% | 1776 | 1720 | 19 % |  |
| sql/refused-late.bool | generated | hand | 2671.5 | 12376.8 | 4.63x | 6118.6 | 2.29x | -50.6% | 13552 | 6616 | 13 % |  |
| sql/select20.bool | generated | hand | 7050.9 | 18704.8 | 2.65x | 18862.3 | 2.68x | +0.8% | 21448 | 21392 | 14 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4810931.2 | 4888112.5 | 1.02x | 4786962.5 | 1.00x | -2.1% | 6276684 | 6276488 | 13 % |  |
| sql/refused-cliff-case-1738 | generated | control | 18878437.5 | 16438987.5 | 0.87x | 5711237.5 | 0.30x | -65.3% | 74362359 | 7842262 | 136 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2720906.2 | 2771165.6 | 1.02x | 2702675.0 | 0.99x | -2.5% | 2994142 | 2994056 | 23 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 4595362.5 | 5502912.5 | 1.20x | 3549431.2 | 0.77x | -35.5% | 12948549 | 3740108 | 112 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 7014925.0 | 7668987.5 | 1.09x | 5448587.5 | 0.78x | -29.0% | 21143694 | 5840045 | 145 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 22205262.5 | 23286462.5 | 1.05x | 6690200.0 | 0.30x | -71.3% | 94724845 | 7298302 | 126 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 7617037.5 | 7821131.2 | 1.03x | 8528543.8 | 1.12x | +9.0% | 8778600 | 8778600 | 27 % |  |
| sql/refused-cliff-and-2715 | generated | control | 8284606.2 | 7996093.8 | 0.97x | 7544412.5 | 0.91x | -5.6% | 8775318 | 8775467 | 14 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 30070462.5 | 30567037.5 | 1.02x | 8569025.0 | 0.28x | -72.0% | 94857750 | 10969699 | 113 % |  |
| sql/refused-cliff-and-3393 | generated | control | 41863500.0 | 39344775.0 | 0.94x | 8483262.5 | 0.20x | -78.4% | 94854601 | 10966827 | 111 % |  |
| el/parse-200k-tokens | generated | control | 15774575.0 | 15711600.0 | 1.00x | 15444650.0 | 0.98x | -1.7% | 16801485 | 16801614 | 45 % |  |
| el/parse-500k-tokens | generated | control | 41536750.0 | 40944850.0 | 0.99x | 40002200.0 | 0.96x | -2.3% | 49503178 | 42002327 | 42 % |  |
| sql/worst-columns-100k | generated | control | 462036600.0 | 462575100.0 | 1.00x | 289412650.0 | 0.63x | -37.4% | 999698515 | 95920877 | 15 % |  |
| fix/Orders128.yield-string | generated | hand | 83483.0 | 269694.3 | 3.23x | 261301.6 | 3.13x | -3.1% | 117936 | 117936 | 14 % |  |
| web/media-type.quoted | generated | control | 254.2 | 326.5 | 1.28x | 310.7 | 1.22x | -4.8% | 816 | 816 | 4 % |  |
| el/ladder | generated | hand | 987.4 | 1763.3 | 1.79x | 1837.9 | 1.86x | +4.2% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 987.4 | 1192.0 | 1.21x | 1186.0 | 1.20x | -0.5% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 108184.0 | 147412.8 | 1.36x | 146321.9 | 1.35x | -0.7% | 169104 | 169104 | 7 % |  |
| el/terms1000 | immediate | hand | 108184.0 | 124625.2 | 1.15x | 120712.0 | 1.12x | -3.1% | 177120 | 177120 | 7 % |  |
| sql/select20 | generated | hand | 6907.0 | 18147.9 | 2.63x | 18522.9 | 2.68x | +2.1% | 21448 | 21448 | 8 % |  |
| sql/refused-late | generated | hand | 2719.3 | 12317.5 | 4.53x | 12561.7 | 4.62x | +2.0% | 13552 | 13552 | 15 % |  |
