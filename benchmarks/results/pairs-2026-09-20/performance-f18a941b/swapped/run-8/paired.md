# Paired stand, 2026-09-21 05:48

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 165414.8 | 181264.8 | 1.10x | 182480.5 | 1.10x | +0.7% | 720048 | 720048 | 20 % |  |
| sql/select20.at | generated | hand | 6609.5 | 16986.2 | 2.57x | 17073.7 | 2.58x | +0.5% | 21416 | 21416 | 3 % |  |
| sql/select20.window | generated | hand | 6607.6 | 17294.0 | 2.62x | 17533.4 | 2.65x | +1.4% | 21416 | 21416 | 22 % |  |
| sql/select20.scan | generated | control | 369.7 | 368.8 | 1.00x | 367.0 | 0.99x | -0.5% | 0 | 0 | 6 % |  |
| tsql/select20.scan | generated | control | 368.4 | 368.3 | 1.00x | 371.7 | 1.01x | +0.9% | 0 | 0 | 9 % |  |
| el/ladder.scan | generated | control | 133.8 | 133.9 | 1.00x | 134.6 | 1.01x | +0.5% | 0 | 0 | 10 % |  |
| el/ladder.bool | generated | hand | 935.1 | 1646.4 | 1.76x | 1641.6 | 1.76x | -0.3% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2488.9 | 11725.2 | 4.71x | 5697.1 | 2.29x | -51.4% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6705.7 | 17670.7 | 2.64x | 17706.0 | 2.64x | +0.2% | 21448 | 21392 | 8 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4367918.8 | 4396706.2 | 1.01x | 4331593.8 | 0.99x | -1.5% | 6276684 | 6276684 | 7 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6393118.8 | 6364500.0 | 1.00x | 6262793.8 | 0.98x | -1.6% | 8778600 | 8778600 | 5 % |  |
| el/ladder | generated | hand | 930.0 | 1652.7 | 1.78x | 1651.7 | 1.78x | -0.1% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 930.0 | 1124.4 | 1.21x | 1103.3 | 1.19x | -1.9% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 108732.6 | 148934.6 | 1.37x | 150736.2 | 1.39x | +1.2% | 169104 | 169104 | 3 % |  |
| el/terms1000 | immediate | hand | 108732.6 | 123362.0 | 1.13x | 121462.7 | 1.12x | -1.5% | 177120 | 177123 | 3 % |  |
| sql/select20 | generated | hand | 6587.4 | 17497.1 | 2.66x | 17388.4 | 2.64x | -0.6% | 21448 | 21448 | 6 % |  |
| sql/refused-late | generated | hand | 2483.6 | 11721.5 | 4.72x | 11752.9 | 4.73x | +0.3% | 13552 | 13552 | 2 % |  |
