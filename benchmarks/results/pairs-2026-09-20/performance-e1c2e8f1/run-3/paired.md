# Paired stand, 2026-09-21 00:59

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.9 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit e1c2e8f1, framework net10.0, no properties, emitted edc6933b61ef4954). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 244393.8 | 217575.0 | 0.89x | 215618.8 | 0.88x | -0.9% | 403288 | 403288 | 13 % |  |
| tsql/columns1000 | generated | scriptdom | 1933050.0 | 173075.0 | 0.09x | 179393.8 | 0.09x | +3.7% | 200489 | 200489 | 48 % |  |
| web/json.array10000 | generated | hand | 189887.5 | 190004.7 | 1.00x | 186281.2 | 0.98x | -2.0% | 720048 | 720048 | 42 % |  |
| sql/select20.at | generated | hand | 6943.5 | 19290.5 | 2.78x | 19350.0 | 2.79x | +0.3% | 21416 | 21416 | 18 % |  |
| sql/select20.window | generated | hand | 7384.0 | 19112.2 | 2.59x | 20147.6 | 2.73x | +5.4% | 21416 | 21416 | 36 % |  |
| sql/select20.scan | generated | control | 401.3 | 389.3 | 0.97x | 395.2 | 0.98x | +1.5% | 0 | 0 | 31 % |  |
| tsql/select20.scan | generated | control | 431.1 | 404.5 | 0.94x | 390.7 | 0.91x | -3.4% | 0 | 0 | 51 % |  |
| el/ladder.scan | generated | control | 143.1 | 147.9 | 1.03x | 135.9 | 0.95x | -8.1% | 0 | 0 | 26 % |  |
| el/ladder.bool | generated | hand | 991.1 | 1740.9 | 1.76x | 1706.3 | 1.72x | -2.0% | 1776 | 1720 | 8 % |  |
| sql/refused-late.bool | generated | hand | 2635.5 | 12110.3 | 4.60x | 5991.1 | 2.27x | -50.5% | 13552 | 6616 | 24 % |  |
| sql/select20.bool | generated | hand | 6976.0 | 18291.8 | 2.62x | 19216.0 | 2.75x | +5.1% | 21448 | 21392 | 22 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4932631.2 | 4608206.2 | 0.93x | 4790712.5 | 0.97x | +4.0% | 6276488 | 6276488 | 42 % |  |
| sql/refused-cliff-case-1738 | generated | control | 10483700.0 | 10330875.0 | 0.99x | 6447675.0 | 0.62x | -37.6% | 74362718 | 7842176 | 439 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2786984.4 | 2746853.1 | 0.99x | 2999715.6 | 1.08x | +9.2% | 2994166 | 2994056 | 41 % |  |
| sql/refused-cliff-joins-1113 | generated | control | 3743062.5 | 3823612.5 | 1.02x | 3215700.0 | 0.86x | -15.9% | 12948506 | 3740043 | 194 % |  |
| sql/refused-cliff-joins-1738 | generated | control | 8199462.5 | 8430968.8 | 1.03x | 5112531.2 | 0.62x | -39.4% | 21143688 | 5840129 | 42 % |  |
| sql/refused-cliff-joins-2172 | generated | control | 10428125.0 | 11173950.0 | 1.07x | 7033325.0 | 0.67x | -37.1% | 94725016 | 7298283 | 554 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 7268150.0 | 7416837.5 | 1.02x | 8019006.2 | 1.10x | +8.1% | 8778600 | 8778600 | 15 % |  |
| sql/refused-cliff-and-2715 | generated | control | 8007850.0 | 7816425.0 | 0.98x | 7244275.0 | 0.90x | -7.3% | 8775448 | 8775467 | 13 % |  |
| sql/refused-cliff-paren-3393 | generated | control | 14594075.0 | 14169025.0 | 0.97x | 8366200.0 | 0.57x | -41.0% | 94857877 | 10969680 | 140 % |  |
| sql/refused-cliff-and-3393 | generated | control | 31688237.5 | 25609925.0 | 0.81x | 8482925.0 | 0.27x | -66.9% | 94854786 | 10966550 | 99 % |  |
| el/parse-200k-tokens | generated | control | 16630075.0 | 15020037.5 | 0.90x | 16070662.5 | 0.97x | +7.0% | 16801614 | 16801507 | 30 % |  |
| el/parse-500k-tokens | generated | control | 52099150.0 | 41600950.0 | 0.80x | 38506350.0 | 0.74x | -7.4% | 49503195 | 42002327 | 33 % |  |
| sql/worst-columns-100k | generated | control | 418961500.0 | 423810800.0 | 1.01x | 283231350.0 | 0.68x | -33.2% | 999698491 | 95920958 | 24 % |  |
| fix/Orders128.yield-string | generated | hand | 79598.6 | 259507.6 | 3.26x | 292401.6 | 3.67x | +12.7% | 117936 | 117936 | 41 % |  |
| web/media-type.quoted | generated | control | 240.9 | 293.2 | 1.22x | 280.3 | 1.16x | -4.4% | 816 | 816 | 4 % |  |
| el/ladder | generated | hand | 968.8 | 1697.4 | 1.75x | 1708.5 | 1.76x | +0.7% | 1776 | 1776 | 6 % |  |
| el/ladder | immediate | hand | 968.8 | 1145.8 | 1.18x | 1141.0 | 1.18x | -0.4% | 1784 | 1784 | 6 % |  |
| el/terms1000 | generated | hand | 105971.2 | 144777.4 | 1.37x | 144873.3 | 1.37x | +0.1% | 169104 | 169104 | 11 % |  |
| el/terms1000 | immediate | hand | 105971.2 | 120467.3 | 1.14x | 119597.0 | 1.13x | -0.7% | 177123 | 177120 | 11 % |  |
| sql/select20 | generated | hand | 6891.3 | 17805.1 | 2.58x | 18385.9 | 2.67x | +3.3% | 21448 | 21448 | 2 % |  |
| sql/refused-late | generated | hand | 2600.1 | 11889.6 | 4.57x | 12145.1 | 4.67x | +2.1% | 13552 | 13552 | 10 % |  |
