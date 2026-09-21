# Paired stand, 2026-09-21 07:24

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.2 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 167560.9 | 182960.2 | 1.09x | 184374.2 | 1.10x | +0.8% | 720048 | 720048 | 161 % |  |
| sql/select20.at | generated | hand | 6730.1 | 17105.4 | 2.54x | 17045.3 | 2.53x | -0.4% | 21416 | 21416 | 37 % |  |
| sql/select20.window | generated | hand | 6622.7 | 17469.8 | 2.64x | 18448.0 | 2.79x | +5.6% | 21416 | 21416 | 9 % |  |
| sql/select20.scan | generated | control | 364.7 | 367.3 | 1.01x | 389.0 | 1.07x | +5.9% | 0 | 0 | 15 % |  |
| tsql/select20.scan | generated | control | 368.1 | 367.4 | 1.00x | 375.0 | 1.02x | +2.1% | 0 | 0 | 1 % |  |
| el/ladder.scan | generated | control | 134.0 | 133.7 | 1.00x | 133.6 | 1.00x | 0.0% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 948.5 | 1692.6 | 1.78x | 1627.8 | 1.72x | -3.8% | 1776 | 1720 | 8 % |  |
| sql/refused-late.bool | generated | hand | 2538.3 | 11936.1 | 4.70x | 5712.0 | 2.25x | -52.1% | 13552 | 6616 | 13 % |  |
| sql/select20.bool | generated | hand | 6654.0 | 17440.1 | 2.62x | 17444.1 | 2.62x | 0.0% | 21448 | 21392 | 1 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4641781.2 | 4426581.2 | 0.95x | 4612975.0 | 0.99x | +4.2% | 6276684 | 6276684 | 3 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6338750.0 | 6339562.5 | 1.00x | 6334318.8 | 1.00x | -0.1% | 8778600 | 8778600 | 10 % |  |
| el/ladder | generated | hand | 940.3 | 1679.7 | 1.79x | 1671.2 | 1.78x | -0.5% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 940.3 | 1138.2 | 1.21x | 1123.5 | 1.19x | -1.3% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 108573.3 | 145497.8 | 1.34x | 148908.8 | 1.37x | +2.3% | 169104 | 169107 | 2 % |  |
| el/terms1000 | immediate | hand | 108573.3 | 118394.5 | 1.09x | 122435.5 | 1.13x | +3.4% | 177120 | 177120 | 2 % |  |
| sql/select20 | generated | hand | 6645.9 | 17476.0 | 2.63x | 17489.2 | 2.63x | +0.1% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2496.3 | 11804.3 | 4.73x | 11678.1 | 4.68x | -1.1% | 13552 | 13552 | 6 % |  |
