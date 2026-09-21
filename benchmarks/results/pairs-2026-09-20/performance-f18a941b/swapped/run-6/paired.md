# Paired stand, 2026-09-21 05:43

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 166005.5 | 184798.4 | 1.11x | 186347.7 | 1.12x | +0.8% | 720048 | 720048 | 19 % |  |
| sql/select20.at | generated | hand | 6601.4 | 17244.9 | 2.61x | 17139.5 | 2.60x | -0.6% | 21416 | 21416 | 13 % |  |
| sql/select20.window | generated | hand | 6672.6 | 17687.7 | 2.65x | 17644.2 | 2.64x | -0.2% | 21416 | 21416 | 59 % |  |
| sql/select20.scan | generated | control | 372.7 | 371.1 | 1.00x | 372.4 | 1.00x | +0.4% | 0 | 0 | 11 % |  |
| tsql/select20.scan | generated | control | 374.0 | 369.7 | 0.99x | 369.8 | 0.99x | +0.1% | 0 | 0 | 4 % |  |
| el/ladder.scan | generated | control | 135.9 | 133.4 | 0.98x | 133.0 | 0.98x | -0.3% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 928.3 | 1666.0 | 1.79x | 1624.9 | 1.75x | -2.5% | 1776 | 1720 | 10 % |  |
| sql/refused-late.bool | generated | hand | 2495.7 | 11944.7 | 4.79x | 5609.5 | 2.25x | -53.0% | 13552 | 6616 | 7 % |  |
| sql/select20.bool | generated | hand | 6709.5 | 17684.4 | 2.64x | 17481.9 | 2.61x | -1.1% | 21448 | 21392 | 4 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4421587.5 | 4372268.8 | 0.99x | 4434612.5 | 1.00x | +1.4% | 6276598 | 6276555 | 8 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6420656.2 | 6328462.5 | 0.99x | 6364806.2 | 0.99x | +0.6% | 8778600 | 8778600 | 29 % |  |
| el/ladder | generated | hand | 948.9 | 1668.4 | 1.76x | 1674.0 | 1.76x | +0.3% | 1776 | 1776 | 11 % |  |
| el/ladder | immediate | hand | 948.9 | 1146.4 | 1.21x | 1143.4 | 1.21x | -0.3% | 1784 | 1784 | 11 % |  |
| el/terms1000 | generated | hand | 109489.5 | 148597.0 | 1.36x | 148874.5 | 1.36x | +0.2% | 169104 | 169104 | 2 % |  |
| el/terms1000 | immediate | hand | 109489.5 | 123792.3 | 1.13x | 121239.1 | 1.11x | -2.1% | 177166 | 177144 | 2 % |  |
| sql/select20 | generated | hand | 6708.5 | 17728.8 | 2.64x | 18017.8 | 2.69x | +1.6% | 21448 | 21448 | 10 % |  |
| sql/refused-late | generated | hand | 2495.9 | 11854.1 | 4.75x | 11730.2 | 4.70x | -1.0% | 13552 | 13552 | 6 % |  |
