# Paired stand, 2026-09-21 07:00

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.3 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 168582.0 | 183839.1 | 1.09x | 184421.9 | 1.09x | +0.3% | 720048 | 720048 | 161 % |  |
| sql/select20.at | generated | hand | 6698.6 | 17168.2 | 2.56x | 17093.7 | 2.55x | -0.4% | 21416 | 21416 | 11 % |  |
| sql/select20.window | generated | hand | 6677.0 | 17737.4 | 2.66x | 17345.8 | 2.60x | -2.2% | 21416 | 21416 | 8 % |  |
| sql/select20.scan | generated | control | 364.0 | 370.7 | 1.02x | 368.4 | 1.01x | -0.6% | 0 | 0 | 2 % |  |
| tsql/select20.scan | generated | control | 396.6 | 369.0 | 0.93x | 364.5 | 0.92x | -1.2% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 132.7 | 134.0 | 1.01x | 133.6 | 1.01x | -0.3% | 0 | 0 | 2 % |  |
| el/ladder.bool | generated | hand | 946.1 | 1688.3 | 1.78x | 1638.9 | 1.73x | -2.9% | 1776 | 1720 | 66 % |  |
| sql/refused-late.bool | generated | hand | 2503.6 | 11920.1 | 4.76x | 5672.0 | 2.27x | -52.4% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 6726.0 | 17538.1 | 2.61x | 17720.9 | 2.63x | +1.0% | 21448 | 21392 | 5 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4996362.5 | 4322625.0 | 0.87x | 4622775.0 | 0.93x | +6.9% | 6276660 | 6276660 | 11 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6496587.5 | 6624593.8 | 1.02x | 6371700.0 | 0.98x | -3.8% | 8778600 | 8778600 | 12 % |  |
| el/ladder | generated | hand | 927.9 | 1684.4 | 1.82x | 1654.7 | 1.78x | -1.8% | 1776 | 1776 | 1 % |  |
| el/ladder | immediate | hand | 927.9 | 1119.9 | 1.21x | 1131.0 | 1.22x | +1.0% | 1784 | 1784 | 1 % |  |
| el/terms1000 | generated | hand | 108732.8 | 148931.8 | 1.37x | 145696.6 | 1.34x | -2.2% | 169104 | 169107 | 27 % |  |
| el/terms1000 | immediate | hand | 108732.8 | 122798.1 | 1.13x | 124924.6 | 1.15x | +1.7% | 177120 | 177120 | 27 % |  |
| sql/select20 | generated | hand | 6729.2 | 17486.9 | 2.60x | 17336.7 | 2.58x | -0.9% | 21448 | 21448 | 6 % |  |
| sql/refused-late | generated | hand | 2518.8 | 11958.6 | 4.75x | 11896.1 | 4.72x | -0.5% | 13552 | 13552 | 30 % |  |
