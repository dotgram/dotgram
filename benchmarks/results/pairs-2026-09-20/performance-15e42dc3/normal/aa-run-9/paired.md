# Paired stand, 2026-09-21 07:03

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 168119.5 | 192001.6 | 1.14x | 198460.2 | 1.18x | +3.4% | 720048 | 720048 | 343 % |  |
| sql/select20.at | generated | hand | 6682.8 | 17071.3 | 2.55x | 17212.5 | 2.58x | +0.8% | 21416 | 21416 | 5 % |  |
| sql/select20.window | generated | hand | 6682.2 | 17397.2 | 2.60x | 17515.6 | 2.62x | +0.7% | 21416 | 21441 | 3 % |  |
| sql/select20.scan | generated | control | 370.4 | 368.9 | 1.00x | 368.4 | 0.99x | -0.2% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 368.3 | 367.8 | 1.00x | 368.2 | 1.00x | +0.1% | 0 | 0 | 3 % |  |
| el/ladder.scan | generated | control | 134.5 | 133.1 | 0.99x | 133.1 | 0.99x | -0.1% | 0 | 0 | 3 % |  |
| el/ladder.bool | generated | hand | 949.8 | 1685.3 | 1.77x | 1644.0 | 1.73x | -2.4% | 1776 | 1720 | 11 % |  |
| sql/refused-late.bool | generated | hand | 2499.3 | 11682.6 | 4.67x | 5688.9 | 2.28x | -51.3% | 13552 | 6616 | 3 % |  |
| sql/select20.bool | generated | hand | 6624.3 | 17313.9 | 2.61x | 17609.8 | 2.66x | +1.7% | 21448 | 21392 | 2 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4767725.0 | 4331875.0 | 0.91x | 4784150.0 | 1.00x | +10.4% | 6276600 | 6276598 | 25 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6378831.2 | 6317906.2 | 0.99x | 6580868.8 | 1.03x | +4.2% | 8778556 | 8778600 | 10 % |  |
| el/ladder | generated | hand | 949.0 | 1682.9 | 1.77x | 1666.9 | 1.76x | -1.0% | 1776 | 1776 | 1 % |  |
| el/ladder | immediate | hand | 949.0 | 1157.8 | 1.22x | 1143.5 | 1.20x | -1.2% | 1784 | 1784 | 1 % |  |
| el/terms1000 | generated | hand | 108267.6 | 150110.8 | 1.39x | 148629.8 | 1.37x | -1.0% | 169104 | 169104 | 3 % |  |
| el/terms1000 | immediate | hand | 108267.6 | 123039.7 | 1.14x | 122204.1 | 1.13x | -0.7% | 177123 | 177120 | 3 % |  |
| sql/select20 | generated | hand | 6648.8 | 17355.4 | 2.61x | 17664.7 | 2.66x | +1.8% | 21448 | 21448 | 35 % |  |
| sql/refused-late | generated | hand | 2510.0 | 11628.0 | 4.63x | 11778.3 | 4.69x | +1.3% | 13552 | 13552 | 5 % |  |
