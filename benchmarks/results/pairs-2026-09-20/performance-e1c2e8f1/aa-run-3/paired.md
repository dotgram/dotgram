# Paired stand, 2026-09-21 01:01

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.8 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 248193.8 | 224096.9 | 0.90x | 220232.8 | 0.89x | -1.7% | 403288 | 403288 | 734 % |  |
| tsql/columns1000 | generated | scriptdom | 2129812.5 | 173181.2 | 0.08x | 212443.8 | 0.10x | +22.7% | 200508 | 200489 | 54 % |  |
| web/json.array10000 | generated | hand | 170104.7 | 188593.8 | 1.11x | 185260.2 | 1.09x | -1.8% | 720048 | 720048 | 61 % |  |
| sql/select20.at | generated | hand | 7055.0 | 18525.2 | 2.63x | 18519.8 | 2.63x | 0.0% | 21416 | 21416 | 9 % |  |
| sql/select20.window | generated | hand | 6834.0 | 18232.1 | 2.67x | 18766.5 | 2.75x | +2.9% | 21416 | 21416 | 11 % |  |
| sql/select20.scan | generated | control | 372.2 | 375.1 | 1.01x | 369.3 | 0.99x | -1.5% | 0 | 0 | 7 % |  |
| tsql/select20.scan | generated | control | 376.0 | 373.1 | 0.99x | 369.3 | 0.98x | -1.0% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 135.5 | 135.9 | 1.00x | 134.8 | 0.99x | -0.8% | 0 | 0 | 6 % |  |
| el/ladder.bool | generated | hand | 954.1 | 1655.1 | 1.73x | 1646.3 | 1.73x | -0.5% | 1776 | 1720 | 3 % |  |
| sql/refused-late.bool | generated | hand | 2579.8 | 11910.7 | 4.62x | 5909.4 | 2.29x | -50.4% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 6787.7 | 18167.1 | 2.68x | 18472.3 | 2.72x | +1.7% | 21448 | 21392 | 6 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4276950.0 | 4242025.0 | 0.99x | 4859300.0 | 1.14x | +14.6% | 6276684 | 6276684 | 39 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2496253.1 | 2527981.2 | 1.01x | 2575678.1 | 1.03x | +1.9% | 2994123 | 2994166 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6324550.0 | 6787787.5 | 1.07x | 6457731.2 | 1.02x | -4.9% | 8778619 | 8778600 | 6 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6308306.2 | 6761125.0 | 1.07x | 6347781.2 | 1.01x | -6.1% | 8775448 | 8775448 | 4 % |  |
| fix/Orders128.yield-string | generated | hand | 79379.4 | 268906.2 | 3.39x | 251509.8 | 3.17x | -6.5% | 117936 | 117936 | 2 % |  |
| web/media-type.quoted | generated | control | 248.9 | 286.7 | 1.15x | 304.1 | 1.22x | +6.1% | 816 | 816 | 20 % |  |
| el/ladder | generated | hand | 962.4 | 1665.4 | 1.73x | 1713.2 | 1.78x | +2.9% | 1776 | 1776 | 17 % |  |
| el/ladder | immediate | hand | 962.4 | 1166.5 | 1.21x | 1175.2 | 1.22x | +0.7% | 1784 | 1784 | 17 % |  |
| el/terms1000 | generated | hand | 112433.7 | 151382.2 | 1.35x | 148599.4 | 1.32x | -1.8% | 169104 | 169128 | 19 % |  |
| el/terms1000 | immediate | hand | 112433.7 | 126376.5 | 1.12x | 120961.0 | 1.08x | -4.3% | 177123 | 177120 | 19 % |  |
| sql/select20 | generated | hand | 6709.3 | 18128.9 | 2.70x | 18309.4 | 2.73x | +1.0% | 21448 | 21472 | 16 % |  |
| sql/refused-late | generated | hand | 2564.4 | 11857.0 | 4.62x | 12025.0 | 4.69x | +1.4% | 13552 | 13552 | 2 % |  |
