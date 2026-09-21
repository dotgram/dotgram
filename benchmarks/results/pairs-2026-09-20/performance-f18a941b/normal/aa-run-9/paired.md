# Paired stand, 2026-09-21 05:27

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 601e6608. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7); after (commit 0783f47b, framework net10.0, no properties, emitted a743c79456502eb7). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| web/json.array10000 | generated | hand | 163935.2 | 181634.4 | 1.11x | 182142.2 | 1.11x | +0.3% | 720048 | 720048 | 353 % |  |
| sql/select20.at | generated | hand | 6623.5 | 18008.3 | 2.72x | 17199.6 | 2.60x | -4.5% | 21416 | 21416 | 31 % |  |
| sql/select20.window | generated | hand | 6585.7 | 17944.4 | 2.72x | 17633.5 | 2.68x | -1.7% | 21416 | 21416 | 19 % |  |
| sql/select20.scan | generated | control | 371.9 | 375.9 | 1.01x | 367.5 | 0.99x | -2.2% | 0 | 0 | 16 % |  |
| tsql/select20.scan | generated | control | 368.9 | 368.9 | 1.00x | 371.2 | 1.01x | +0.6% | 0 | 0 | 6 % |  |
| el/ladder.scan | generated | control | 134.7 | 134.7 | 1.00x | 134.4 | 1.00x | -0.2% | 0 | 0 | 8 % |  |
| el/ladder.bool | generated | hand | 932.1 | 1675.8 | 1.80x | 1682.4 | 1.80x | +0.4% | 1776 | 1720 | 5 % |  |
| sql/refused-late.bool | generated | hand | 2528.0 | 12087.8 | 4.78x | 5631.4 | 2.23x | -53.4% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 6639.7 | 17791.6 | 2.68x | 17652.6 | 2.66x | -0.8% | 21448 | 21392 | 10 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4355381.2 | 4461768.8 | 1.02x | 4405393.8 | 1.01x | -1.3% | 6276598 | 6276555 | 5 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6374906.2 | 6449525.0 | 1.01x | 6339812.5 | 0.99x | -1.7% | 8778600 | 8778600 | 12 % |  |
| el/ladder | generated | hand | 941.5 | 1682.1 | 1.79x | 1724.5 | 1.83x | +2.5% | 1776 | 1776 | 2 % |  |
| el/ladder | immediate | hand | 941.5 | 1140.0 | 1.21x | 1140.2 | 1.21x | 0.0% | 1784 | 1784 | 2 % |  |
| el/terms1000 | generated | hand | 106924.8 | 150870.9 | 1.41x | 148847.0 | 1.39x | -1.3% | 169104 | 169104 | 13 % |  |
| el/terms1000 | immediate | hand | 106924.8 | 120509.5 | 1.13x | 121068.0 | 1.13x | +0.5% | 177120 | 177123 | 13 % |  |
| sql/select20 | generated | hand | 6690.2 | 17962.1 | 2.68x | 17665.6 | 2.64x | -1.7% | 21448 | 21448 | 10 % |  |
| sql/refused-late | generated | hand | 2512.1 | 12100.7 | 4.82x | 11820.2 | 4.71x | -2.3% | 13552 | 13552 | 15 % |  |
