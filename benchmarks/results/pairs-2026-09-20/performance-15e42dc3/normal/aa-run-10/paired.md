# Paired stand, 2026-09-21 07:06

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 166914.1 | 189793.8 | 1.14x | 189840.6 | 1.14x | 0.0% | 720048 | 720048 | 30 % |  |
| sql/select20.at | generated | hand | 6688.0 | 17204.7 | 2.57x | 17685.8 | 2.64x | +2.8% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6642.7 | 17520.0 | 2.64x | 17747.1 | 2.67x | +1.3% | 21416 | 21416 | 7 % |  |
| sql/select20.scan | generated | control | 367.8 | 370.9 | 1.01x | 372.1 | 1.01x | +0.3% | 0 | 0 | 5 % |  |
| tsql/select20.scan | generated | control | 367.7 | 370.2 | 1.01x | 370.3 | 1.01x | 0.0% | 0 | 0 | 8 % |  |
| el/ladder.scan | generated | control | 133.6 | 135.5 | 1.01x | 134.2 | 1.00x | -0.9% | 0 | 0 | 5 % |  |
| el/ladder.bool | generated | hand | 942.9 | 1651.4 | 1.75x | 1670.6 | 1.77x | +1.2% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2498.1 | 11824.5 | 4.73x | 5672.1 | 2.27x | -52.0% | 13552 | 6616 | 1 % |  |
| sql/select20.bool | generated | hand | 6638.4 | 17600.9 | 2.65x | 17997.0 | 2.71x | +2.3% | 21448 | 21392 | 2 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4332381.2 | 4391756.2 | 1.01x | 4370006.2 | 1.01x | -0.5% | 6276555 | 6276555 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6325175.0 | 6496475.0 | 1.03x | 6832612.5 | 1.08x | +5.2% | 8778556 | 8778600 | 5 % |  |
| el/ladder | generated | hand | 942.7 | 1652.4 | 1.75x | 1653.4 | 1.75x | +0.1% | 1776 | 1776 | 3 % |  |
| el/ladder | immediate | hand | 942.7 | 1133.8 | 1.20x | 1136.5 | 1.21x | +0.2% | 1784 | 1784 | 3 % |  |
| el/terms1000 | generated | hand | 108195.2 | 147387.3 | 1.36x | 148313.8 | 1.37x | +0.6% | 169104 | 169104 | 2 % |  |
| el/terms1000 | immediate | hand | 108195.2 | 122327.9 | 1.13x | 126562.4 | 1.17x | +3.5% | 177123 | 177120 | 2 % |  |
| sql/select20 | generated | hand | 6644.9 | 17557.4 | 2.64x | 17897.8 | 2.69x | +1.9% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2502.4 | 11822.9 | 4.72x | 11776.1 | 4.71x | -0.4% | 13552 | 13552 | 4 % |  |
