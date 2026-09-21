# Paired stand, 2026-09-21 05:12

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 166504.7 | 182605.5 | 1.10x | 183189.8 | 1.10x | +0.3% | 720048 | 720048 | 7 % |  |
| sql/select20.at | generated | hand | 6667.0 | 17000.3 | 2.55x | 17057.3 | 2.56x | +0.3% | 21416 | 21416 | 38 % |  |
| sql/select20.window | generated | hand | 6628.5 | 17463.4 | 2.63x | 17440.2 | 2.63x | -0.1% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 367.3 | 380.7 | 1.04x | 370.1 | 1.01x | -2.8% | 0 | 0 | 3 % |  |
| tsql/select20.scan | generated | control | 375.0 | 367.0 | 0.98x | 366.0 | 0.98x | -0.3% | 0 | 0 | 11 % |  |
| el/ladder.scan | generated | control | 133.5 | 134.1 | 1.00x | 134.4 | 1.01x | +0.2% | 0 | 0 | 9 % |  |
| el/ladder.bool | generated | hand | 936.4 | 1647.3 | 1.76x | 1726.7 | 1.84x | +4.8% | 1776 | 1720 | 24 % |  |
| sql/refused-late.bool | generated | hand | 2496.1 | 11812.0 | 4.73x | 5616.2 | 2.25x | -52.5% | 13552 | 6616 | 2 % |  |
| sql/select20.bool | generated | hand | 6729.4 | 17433.6 | 2.59x | 17486.3 | 2.60x | +0.3% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4477050.0 | 4449481.2 | 0.99x | 4470375.0 | 1.00x | +0.5% | 6276598 | 6276641 | 4 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6314468.8 | 6400762.5 | 1.01x | 6251237.5 | 0.99x | -2.3% | 8778556 | 8778600 | 10 % |  |
| el/ladder | generated | hand | 929.2 | 1647.6 | 1.77x | 1742.0 | 1.87x | +5.7% | 1776 | 1776 | 10 % |  |
| el/ladder | immediate | hand | 929.2 | 1144.8 | 1.23x | 1121.8 | 1.21x | -2.0% | 1784 | 1784 | 10 % |  |
| el/terms1000 | generated | hand | 107478.3 | 148880.8 | 1.39x | 147881.2 | 1.38x | -0.7% | 169104 | 169150 | 11 % |  |
| el/terms1000 | immediate | hand | 107478.3 | 122596.7 | 1.14x | 121150.5 | 1.13x | -1.2% | 177144 | 177120 | 11 % |  |
| sql/select20 | generated | hand | 6690.0 | 17372.1 | 2.60x | 17483.0 | 2.61x | +0.6% | 21448 | 21448 | 11 % |  |
| sql/refused-late | generated | hand | 2503.7 | 11728.0 | 4.68x | 11766.4 | 4.70x | +0.3% | 13552 | 13552 | 13 % |  |
