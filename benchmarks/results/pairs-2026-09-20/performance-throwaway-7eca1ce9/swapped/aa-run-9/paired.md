# Paired stand, 2026-09-21 03:58

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b); after (commit 7eca1ce9, framework net10.0, no properties, emitted d482945ac6d8ed8b). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| sql/select20.at | generated | hand | 6713.8 | 17352.5 | 2.58x | 17383.8 | 2.59x | +0.2% | 21416 | 21460 | 3 % |  |
| sql/select20.window | generated | hand | 6709.4 | 17425.5 | 2.60x | 17731.1 | 2.64x | +1.8% | 21416 | 21440 | 5 % |  |
| sql/select20.scan | generated | control | 365.5 | 374.9 | 1.03x | 372.1 | 1.02x | -0.8% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 371.1 | 375.4 | 1.01x | 374.8 | 1.01x | -0.2% | 0 | 0 | 10 % |  |
| el/ladder.scan | generated | control | 134.9 | 137.9 | 1.02x | 136.8 | 1.01x | -0.8% | 0 | 0 | 7 % |  |
| el/ladder.bool | generated | hand | 951.5 | 1669.8 | 1.75x | 1642.6 | 1.73x | -1.6% | 1776 | 1720 | 62 % |  |
| sql/refused-late.bool | generated | hand | 2563.5 | 11737.1 | 4.58x | 5723.8 | 2.23x | -51.2% | 13552 | 6616 | 7 % |  |
| sql/select20.bool | generated | hand | 6798.2 | 17367.0 | 2.55x | 17714.0 | 2.61x | +2.0% | 21448 | 21392 | 6 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6198450.0 | 6323350.0 | 1.02x | 6422250.0 | 1.04x | +1.6% | 8778600 | 8778600 | 45 % |  |
| el/ladder | generated | hand | 950.5 | 1672.1 | 1.76x | 1646.7 | 1.73x | -1.5% | 1776 | 1776 | 1 % |  |
| el/ladder | immediate | hand | 950.5 | 1127.5 | 1.19x | 1110.0 | 1.17x | -1.5% | 1784 | 1784 | 1 % |  |
| sql/select20 | generated | hand | 6746.8 | 17433.0 | 2.58x | 17579.4 | 2.61x | +0.8% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2559.1 | 11699.7 | 4.57x | 11832.8 | 4.62x | +1.1% | 13552 | 13552 | 20 % |  |
