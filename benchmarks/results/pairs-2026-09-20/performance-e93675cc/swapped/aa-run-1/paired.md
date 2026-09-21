# Paired stand, 2026-09-21 02:06

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 246259.4 | 221896.9 | 0.90x | 219510.9 | 0.89x | -1.1% | 403288 | 403288 | 746 % |  |
| tsql/columns1000 | generated | scriptdom | 1706156.2 | 172231.2 | 0.10x | 176093.8 | 0.10x | +2.2% | 200489 | 200489 | 7 % |  |
| web/json.array10000 | generated | hand | 164151.6 | 182593.8 | 1.11x | 181896.9 | 1.11x | -0.4% | 720048 | 720048 | 8 % |  |
| sql/select20.at | generated | hand | 6637.0 | 17346.0 | 2.61x | 17530.9 | 2.64x | +1.1% | 21416 | 21416 | 3 % |  |
| sql/select20.window | generated | hand | 6646.6 | 17538.6 | 2.64x | 17836.9 | 2.68x | +1.7% | 21416 | 21416 | 9 % |  |
| sql/select20.scan | generated | control | 372.5 | 374.0 | 1.00x | 371.1 | 1.00x | -0.8% | 0 | 0 | 19 % |  |
| tsql/select20.scan | generated | control | 373.2 | 371.4 | 1.00x | 376.6 | 1.01x | +1.4% | 0 | 0 | 16 % |  |
| el/ladder.scan | generated | control | 137.1 | 136.2 | 0.99x | 136.6 | 1.00x | +0.3% | 0 | 0 | 18 % |  |
| el/ladder.bool | generated | hand | 955.6 | 1655.0 | 1.73x | 1651.2 | 1.73x | -0.2% | 1776 | 1720 | 2 % |  |
| sql/refused-late.bool | generated | hand | 2509.1 | 11789.9 | 4.70x | 5778.2 | 2.30x | -51.0% | 13552 | 6616 | 6 % |  |
| sql/select20.bool | generated | hand | 6643.3 | 17614.1 | 2.65x | 18079.6 | 2.72x | +2.6% | 21448 | 21392 | 16 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4381462.5 | 4372356.2 | 1.00x | 4383275.0 | 1.00x | +0.2% | 6276555 | 6276617 | 11 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2589156.2 | 2543512.5 | 0.98x | 2543831.2 | 0.98x | 0.0% | 2994166 | 2994166 | 3 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6429256.2 | 6416212.5 | 1.00x | 6238643.8 | 0.97x | -2.8% | 8778600 | 8778600 | 7 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6335868.8 | 6372937.5 | 1.01x | 6280662.5 | 0.99x | -1.4% | 8775448 | 8775448 | 8 % |  |
| fix/Orders128.yield-string | generated | hand | 78770.9 | 249265.4 | 3.16x | 245971.9 | 3.12x | -1.3% | 117936 | 117936 | 2 % |  |
| web/media-type.quoted | generated | control | 241.1 | 280.2 | 1.16x | 284.8 | 1.18x | +1.6% | 816 | 816 | 13 % |  |
| el/ladder | generated | hand | 960.0 | 1663.0 | 1.73x | 1668.8 | 1.74x | +0.4% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 960.0 | 1127.8 | 1.17x | 1132.3 | 1.18x | +0.4% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 108373.4 | 148281.5 | 1.37x | 145659.2 | 1.34x | -1.8% | 169107 | 169104 | 6 % |  |
| el/terms1000 | immediate | hand | 108373.4 | 121267.5 | 1.12x | 119606.9 | 1.10x | -1.4% | 177120 | 177120 | 6 % |  |
| sql/select20 | generated | hand | 6661.2 | 17648.0 | 2.65x | 17854.9 | 2.68x | +1.2% | 21448 | 21448 | 2 % |  |
| sql/refused-late | generated | hand | 2508.3 | 11730.7 | 4.68x | 11766.0 | 4.69x | +0.3% | 13552 | 13552 | 4 % |  |
