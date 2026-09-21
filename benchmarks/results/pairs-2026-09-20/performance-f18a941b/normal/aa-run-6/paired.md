# Paired stand, 2026-09-21 05:19

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 163354.7 | 182227.3 | 1.12x | 184169.5 | 1.13x | +1.1% | 720048 | 720048 | 355 % |  |
| sql/select20.at | generated | hand | 6621.9 | 16877.8 | 2.55x | 17106.2 | 2.58x | +1.4% | 21416 | 21416 | 7 % |  |
| sql/select20.window | generated | hand | 6690.9 | 18026.8 | 2.69x | 17257.8 | 2.58x | -4.3% | 21416 | 21441 | 5 % |  |
| sql/select20.scan | generated | control | 378.2 | 379.5 | 1.00x | 372.9 | 0.99x | -1.8% | 0 | 0 | 28 % |  |
| tsql/select20.scan | generated | control | 368.9 | 378.6 | 1.03x | 372.7 | 1.01x | -1.6% | 0 | 0 | 2 % |  |
| el/ladder.scan | generated | control | 133.9 | 136.8 | 1.02x | 136.2 | 1.02x | -0.4% | 0 | 0 | 3 % |  |
| el/ladder.bool | generated | hand | 920.1 | 1667.2 | 1.81x | 1709.1 | 1.86x | +2.5% | 1776 | 1720 | 4 % |  |
| sql/refused-late.bool | generated | hand | 2535.2 | 11749.1 | 4.63x | 5644.0 | 2.23x | -52.0% | 13552 | 6616 | 17 % |  |
| sql/select20.bool | generated | hand | 6649.4 | 17346.5 | 2.61x | 17339.6 | 2.61x | 0.0% | 21448 | 21392 | 1 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4375393.8 | 4501231.2 | 1.03x | 4329331.2 | 0.99x | -3.8% | 6276684 | 6276660 | 15 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6399412.5 | 6286206.2 | 0.98x | 6387337.5 | 1.00x | +1.6% | 8778619 | 8778600 | 10 % |  |
| el/ladder | generated | hand | 929.9 | 1656.8 | 1.78x | 1791.6 | 1.93x | +8.1% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 929.9 | 1140.6 | 1.23x | 1130.0 | 1.22x | -0.9% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 106830.0 | 149304.5 | 1.40x | 149808.8 | 1.40x | +0.3% | 169104 | 169104 | 5 % |  |
| el/terms1000 | immediate | hand | 106830.0 | 123769.5 | 1.16x | 123026.0 | 1.15x | -0.6% | 177123 | 177120 | 5 % |  |
| sql/select20 | generated | hand | 6630.3 | 17372.9 | 2.62x | 17306.6 | 2.61x | -0.4% | 21448 | 21448 | 3 % |  |
| sql/refused-late | generated | hand | 2494.3 | 11661.7 | 4.68x | 11659.2 | 4.67x | 0.0% | 13552 | 13552 | 7 % |  |
