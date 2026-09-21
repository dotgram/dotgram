# Paired stand, 2026-09-21 00:02

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.5 ns.

This binary, and the libraries it holds as the control, was built from b746f489. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 240459.4 | 215768.8 | 0.90x | 209406.2 | 0.87x | -2.9% | 403288 | 403288 | 5 % |  |
| tsql/columns1000 | generated | scriptdom | 1710768.8 | 173050.0 | 0.10x | 174856.2 | 0.10x | +1.0% | 200489 | 200489 | 13 % |  |
| web/json.array10000 | generated | hand | 167540.6 | 189859.4 | 1.13x | 186746.9 | 1.11x | -1.6% | 720048 | 720048 | 41 % |  |
| sql/select20.at | generated | hand | 6845.3 | 18263.1 | 2.67x | 17863.8 | 2.61x | -2.2% | 21416 | 21416 | 18 % |  |
| sql/select20.window | generated | hand | 6869.6 | 18443.1 | 2.68x | 18334.8 | 2.67x | -0.6% | 21416 | 21416 | 10 % |  |
| sql/select20.scan | generated | control | 562.6 | 576.3 | 1.02x | 566.4 | 1.01x | -1.7% | 0 | 0 | 56 % |  |
| tsql/select20.scan | generated | control | 415.6 | 401.2 | 0.97x | 497.4 | 1.20x | +24.0% | 0 | 0 | 121 % |  |
| el/ladder.scan | generated | control | 137.6 | 140.5 | 1.02x | 138.3 | 1.01x | -1.5% | 0 | 0 | 5 % |  |
| el/ladder.bool | generated | hand | 980.3 | 1682.6 | 1.72x | 1693.3 | 1.73x | +0.6% | 1776 | 1720 | 13 % |  |
| sql/refused-late.bool | generated | hand | 2939.3 | 13396.0 | 4.56x | 6657.7 | 2.27x | -50.3% | 13552 | 6616 | 30 % |  |
| sql/select20.bool | generated | hand | 8362.9 | 21427.1 | 2.56x | 22284.9 | 2.66x | +4.0% | 21448 | 21392 | 40 % |  |
| fix/Orders128.yield-string | generated | hand | 81500.6 | 253773.8 | 3.11x | 257255.6 | 3.16x | +1.4% | 117936 | 117936 | 3 % |  |
| web/media-type.quoted | generated | control | 247.1 | 286.7 | 1.16x | 332.4 | 1.35x | +15.9% | 816 | 816 | 7 % |  |
| el/ladder | generated | hand | 984.2 | 1702.8 | 1.73x | 1690.8 | 1.72x | -0.7% | 1776 | 1776 | 20 % |  |
| el/ladder | immediate | hand | 984.2 | 1279.7 | 1.30x | 1176.9 | 1.20x | -8.0% | 1784 | 1784 | 20 % |  |
| el/terms1000 | generated | hand | 113343.2 | 152526.3 | 1.35x | 153365.9 | 1.35x | +0.6% | 169104 | 169107 | 10 % |  |
| el/terms1000 | immediate | hand | 113343.2 | 145302.8 | 1.28x | 125349.5 | 1.11x | -13.7% | 177120 | 177120 | 10 % |  |
| sql/select20 | generated | hand | 6790.5 | 18038.2 | 2.66x | 18125.1 | 2.67x | +0.5% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2582.2 | 12136.6 | 4.70x | 12257.6 | 4.75x | +1.0% | 13552 | 13552 | 5 % |  |
