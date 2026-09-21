# Paired stand, 2026-09-21 05:55

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit f18a941b, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 165539.1 | 182670.3 | 1.10x | 182863.3 | 1.10x | +0.1% | 720048 | 720048 | 10 % |  |
| sql/select20.at | generated | hand | 6622.2 | 16959.9 | 2.56x | 17054.0 | 2.58x | +0.6% | 21416 | 21416 | 9 % |  |
| sql/select20.window | generated | hand | 6610.4 | 17474.5 | 2.64x | 17550.1 | 2.65x | +0.4% | 21416 | 21416 | 2 % |  |
| sql/select20.scan | generated | control | 383.9 | 373.8 | 0.97x | 371.8 | 0.97x | -0.5% | 0 | 0 | 11 % |  |
| tsql/select20.scan | generated | control | 367.2 | 374.7 | 1.02x | 374.2 | 1.02x | -0.1% | 0 | 0 | 16 % |  |
| el/ladder.scan | generated | control | 136.6 | 135.1 | 0.99x | 133.6 | 0.98x | -1.1% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 926.8 | 1664.9 | 1.80x | 1623.0 | 1.75x | -2.5% | 1776 | 1720 | 23 % |  |
| sql/refused-late.bool | generated | hand | 2507.4 | 11678.8 | 4.66x | 5616.2 | 2.24x | -51.9% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 6712.8 | 17493.4 | 2.61x | 17602.8 | 2.62x | +0.6% | 21448 | 21392 | 11 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4252025.0 | 4266962.5 | 1.00x | 4382950.0 | 1.03x | +2.7% | 6276684 | 6276641 | 15 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6414743.8 | 6235318.8 | 0.97x | 6268731.2 | 0.98x | +0.5% | 8778600 | 8778619 | 13 % |  |
| el/ladder | generated | hand | 940.4 | 1680.6 | 1.79x | 1655.9 | 1.76x | -1.5% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 940.4 | 1124.8 | 1.20x | 1193.0 | 1.27x | +6.1% | 1784 | 1784 | 4 % |  |
| el/terms1000 | generated | hand | 107926.3 | 145207.4 | 1.35x | 145206.1 | 1.35x | 0.0% | 169104 | 169128 | 2 % |  |
| el/terms1000 | immediate | hand | 107926.3 | 119264.6 | 1.11x | 123244.5 | 1.14x | +3.3% | 177120 | 177120 | 2 % |  |
| sql/select20 | generated | hand | 6712.1 | 17305.1 | 2.58x | 17476.9 | 2.60x | +1.0% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2507.6 | 11648.4 | 4.65x | 11757.1 | 4.69x | +0.9% | 13552 | 13552 | 5 % |  |
