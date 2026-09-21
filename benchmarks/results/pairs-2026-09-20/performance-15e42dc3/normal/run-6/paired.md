# Paired stand, 2026-09-21 06:54

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 169353.9 | 184266.4 | 1.09x | 183573.4 | 1.08x | -0.4% | 720048 | 720048 | 350 % |  |
| sql/select20.at | generated | hand | 6574.0 | 17474.3 | 2.66x | 17055.6 | 2.59x | -2.4% | 21416 | 21416 | 3 % |  |
| sql/select20.window | generated | hand | 6591.2 | 17442.7 | 2.65x | 17535.3 | 2.66x | +0.5% | 21416 | 21416 | 11 % |  |
| sql/select20.scan | generated | control | 370.9 | 372.7 | 1.00x | 376.4 | 1.01x | +1.0% | 0 | 0 | 8 % |  |
| tsql/select20.scan | generated | control | 367.9 | 384.6 | 1.05x | 376.1 | 1.02x | -2.2% | 0 | 0 | 1 % |  |
| el/ladder.scan | generated | control | 137.4 | 140.1 | 1.02x | 136.7 | 0.99x | -2.4% | 0 | 0 | 13 % |  |
| el/ladder.bool | generated | hand | 930.3 | 1636.5 | 1.76x | 1646.4 | 1.77x | +0.6% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2487.1 | 11784.7 | 4.74x | 5634.1 | 2.27x | -52.2% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 6581.3 | 17490.2 | 2.66x | 17429.2 | 2.65x | -0.3% | 21448 | 21392 | 6 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4329050.0 | 4567900.0 | 1.06x | 4369275.0 | 1.01x | -4.3% | 6276684 | 6276684 | 6 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6281943.8 | 6369462.5 | 1.01x | 6258131.2 | 1.00x | -1.7% | 8778600 | 8778600 | 7 % |  |
| el/ladder | generated | hand | 937.5 | 1650.4 | 1.76x | 1665.1 | 1.78x | +0.9% | 1776 | 1776 | 8 % |  |
| el/ladder | immediate | hand | 937.5 | 1117.3 | 1.19x | 1140.3 | 1.22x | +2.1% | 1784 | 1784 | 8 % |  |
| el/terms1000 | generated | hand | 107399.2 | 145157.3 | 1.35x | 144473.0 | 1.35x | -0.5% | 169104 | 169104 | 3 % |  |
| el/terms1000 | immediate | hand | 107399.2 | 120397.9 | 1.12x | 122766.7 | 1.14x | +2.0% | 177120 | 177120 | 3 % |  |
| sql/select20 | generated | hand | 6628.8 | 17459.1 | 2.63x | 17584.7 | 2.65x | +0.7% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2482.2 | 11666.5 | 4.70x | 11764.6 | 4.74x | +0.8% | 13552 | 13552 | 8 % |  |
