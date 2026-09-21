# Paired stand, 2026-09-21 01:09

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.8 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 248057.8 | 216481.2 | 0.87x | 215459.4 | 0.87x | -0.5% | 403288 | 403288 | 17 % |  |
| tsql/columns1000 | generated | scriptdom | 2208525.0 | 247456.2 | 0.11x | 214168.8 | 0.10x | -13.5% | 200489 | 200489 | 48 % |  |
| web/json.array10000 | generated | hand | 168464.8 | 193719.5 | 1.15x | 186776.6 | 1.11x | -3.6% | 720048 | 720048 | 43 % |  |
| sql/select20.at | generated | hand | 6842.2 | 17720.5 | 2.59x | 17625.3 | 2.58x | -0.5% | 21416 | 21416 | 6 % |  |
| sql/select20.window | generated | hand | 6830.4 | 17933.9 | 2.63x | 17911.8 | 2.62x | -0.1% | 21416 | 21416 | 7 % |  |
| sql/select20.scan | generated | control | 377.2 | 370.1 | 0.98x | 371.3 | 0.98x | +0.3% | 0 | 0 | 16 % |  |
| tsql/select20.scan | generated | control | 371.2 | 375.1 | 1.01x | 379.8 | 1.02x | +1.2% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 136.8 | 135.7 | 0.99x | 135.7 | 0.99x | 0.0% | 0 | 0 | 14 % |  |
| el/ladder.bool | generated | hand | 946.0 | 1692.0 | 1.79x | 1671.4 | 1.77x | -1.2% | 1776 | 1720 | 6 % |  |
| sql/refused-late.bool | generated | hand | 2553.3 | 11860.5 | 4.65x | 5678.5 | 2.22x | -52.1% | 13552 | 6616 | 20 % |  |
| sql/select20.bool | generated | hand | 6738.8 | 17962.4 | 2.67x | 17847.2 | 2.65x | -0.6% | 21448 | 21392 | 2 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4284300.0 | 4412800.0 | 1.03x | 4325025.0 | 1.01x | -2.0% | 6276641 | 6276598 | 28 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2530993.8 | 2549306.2 | 1.01x | 2553228.1 | 1.01x | +0.2% | 2994142 | 2994166 | 9 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6301912.5 | 6327087.5 | 1.00x | 6406487.5 | 1.02x | +1.3% | 8778556 | 8778600 | 13 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6360825.0 | 6358475.0 | 1.00x | 6405925.0 | 1.01x | +0.7% | 8775448 | 8775424 | 17 % |  |
| fix/Orders128.yield-string | generated | hand | 80478.7 | 253642.7 | 3.15x | 261130.8 | 3.24x | +3.0% | 117936 | 117936 | 5 % |  |
| web/media-type.quoted | generated | control | 241.0 | 296.6 | 1.23x | 304.2 | 1.26x | +2.6% | 816 | 816 | 2 % |  |
| el/ladder | generated | hand | 949.9 | 1676.0 | 1.76x | 1701.1 | 1.79x | +1.5% | 1776 | 1776 | 6 % |  |
| el/ladder | immediate | hand | 949.9 | 1141.8 | 1.20x | 1131.7 | 1.19x | -0.9% | 1784 | 1784 | 6 % |  |
| el/terms1000 | generated | hand | 108300.7 | 148367.6 | 1.37x | 150824.4 | 1.39x | +1.7% | 169104 | 169104 | 26 % |  |
| el/terms1000 | immediate | hand | 108300.7 | 122268.1 | 1.13x | 127968.8 | 1.18x | +4.7% | 177123 | 177120 | 26 % |  |
| sql/select20 | generated | hand | 6689.5 | 17899.5 | 2.68x | 18085.1 | 2.70x | +1.0% | 21448 | 21448 | 10 % |  |
| sql/refused-late | generated | hand | 2535.3 | 11926.2 | 4.70x | 11821.0 | 4.66x | -0.9% | 13552 | 13552 | 5 % |  |
