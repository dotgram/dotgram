# Paired stand, 2026-09-21 06:44

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.5 ns.

This binary, and the libraries it holds as the control, was built from 94d65c20. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit c9904b56, framework net10.0, no properties, emitted c4a11adbcaea2002); after (commit 15e42dc3, framework net10.0, no properties, emitted 903cfef625dcf62c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 162882.8 | 183406.2 | 1.13x | 190390.6 | 1.17x | +3.8% | 720048 | 720048 | 6 % |  |
| sql/select20.at | generated | hand | 6571.0 | 18380.1 | 2.80x | 16839.2 | 2.56x | -8.4% | 21416 | 21416 | 7 % |  |
| sql/select20.window | generated | hand | 6606.0 | 18774.1 | 2.84x | 17220.8 | 2.61x | -8.3% | 21416 | 21416 | 12 % |  |
| sql/select20.scan | generated | control | 368.0 | 372.4 | 1.01x | 364.8 | 0.99x | -2.0% | 0 | 0 | 19 % |  |
| tsql/select20.scan | generated | control | 370.3 | 366.4 | 0.99x | 364.7 | 0.98x | -0.4% | 0 | 0 | 6 % |  |
| el/ladder.scan | generated | control | 134.3 | 133.1 | 0.99x | 132.8 | 0.99x | -0.2% | 0 | 0 | 4 % |  |
| el/ladder.bool | generated | hand | 944.9 | 1665.3 | 1.76x | 1717.1 | 1.82x | +3.1% | 1776 | 1720 | 45 % |  |
| sql/refused-late.bool | generated | hand | 2476.4 | 12311.7 | 4.97x | 5576.4 | 2.25x | -54.7% | 13552 | 6616 | 10 % |  |
| sql/select20.bool | generated | hand | 6569.3 | 18699.0 | 2.85x | 17265.7 | 2.63x | -7.7% | 21448 | 21392 | 7 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4351437.5 | 4663800.0 | 1.07x | 4429287.5 | 1.02x | -5.0% | 6276684 | 6276684 | 6 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6301750.0 | 6460412.5 | 1.03x | 6298050.0 | 1.00x | -2.5% | 8778600 | 8778600 | 11 % |  |
| el/ladder | generated | hand | 950.7 | 1671.5 | 1.76x | 1745.0 | 1.84x | +4.4% | 1776 | 1776 | 8 % |  |
| el/ladder | immediate | hand | 950.7 | 1127.8 | 1.19x | 1155.0 | 1.21x | +2.4% | 1784 | 1784 | 8 % |  |
| el/terms1000 | generated | hand | 109731.2 | 148396.0 | 1.35x | 146217.6 | 1.33x | -1.5% | 169104 | 169104 | 1 % |  |
| el/terms1000 | immediate | hand | 109731.2 | 122336.1 | 1.11x | 120240.0 | 1.10x | -1.7% | 177123 | 177120 | 1 % |  |
| sql/select20 | generated | hand | 6517.3 | 18662.6 | 2.86x | 17232.2 | 2.64x | -7.7% | 21448 | 21448 | 2 % |  |
| sql/refused-late | generated | hand | 2484.7 | 12284.2 | 4.94x | 11626.1 | 4.68x | -5.4% | 13552 | 13552 | 7 % |  |
