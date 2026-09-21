# Paired stand, 2026-09-21 07:28

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 170303.9 | 183334.4 | 1.08x | 190984.4 | 1.12x | +4.2% | 720048 | 720048 | 341 % |  |
| sql/select20.at | generated | hand | 6669.0 | 17228.8 | 2.58x | 17077.9 | 2.56x | -0.9% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6655.7 | 17583.2 | 2.64x | 17421.9 | 2.62x | -0.9% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 369.3 | 376.8 | 1.02x | 371.9 | 1.01x | -1.3% | 0 | 0 | 20 % |  |
| tsql/select20.scan | generated | control | 374.9 | 369.8 | 0.99x | 373.1 | 1.00x | +0.9% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 136.4 | 134.4 | 0.99x | 135.0 | 0.99x | +0.4% | 0 | 0 | 6 % |  |
| el/ladder.bool | generated | hand | 940.1 | 1696.2 | 1.80x | 1675.4 | 1.78x | -1.2% | 1776 | 1720 | 3 % |  |
| sql/refused-late.bool | generated | hand | 2495.4 | 11835.0 | 4.74x | 5560.3 | 2.23x | -53.0% | 13552 | 6616 | 7 % |  |
| sql/select20.bool | generated | hand | 6712.8 | 17810.0 | 2.65x | 17579.1 | 2.62x | -1.3% | 21448 | 21392 | 3 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4513931.2 | 4322318.8 | 0.96x | 4328212.5 | 0.96x | +0.1% | 6276684 | 6276684 | 18 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6506137.5 | 6356975.0 | 0.98x | 6217350.0 | 0.96x | -2.2% | 8778619 | 8778600 | 4 % |  |
| el/ladder | generated | hand | 929.4 | 1704.7 | 1.83x | 1689.2 | 1.82x | -0.9% | 1776 | 1776 | 17 % |  |
| el/ladder | immediate | hand | 929.4 | 1133.7 | 1.22x | 1151.7 | 1.24x | +1.6% | 1784 | 1784 | 17 % |  |
| el/terms1000 | generated | hand | 107249.5 | 149110.5 | 1.39x | 145885.4 | 1.36x | -2.2% | 169104 | 169104 | 12 % |  |
| el/terms1000 | immediate | hand | 107249.5 | 122460.4 | 1.14x | 119206.9 | 1.11x | -2.7% | 177120 | 177120 | 12 % |  |
| sql/select20 | generated | hand | 6666.5 | 17621.2 | 2.64x | 17281.7 | 2.59x | -1.9% | 21448 | 21448 | 14 % |  |
| sql/refused-late | generated | hand | 2512.3 | 11834.0 | 4.71x | 11607.7 | 4.62x | -1.9% | 13552 | 13552 | 2 % |  |
