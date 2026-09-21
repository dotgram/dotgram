# Paired stand, 2026-09-21 01:17

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.8 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 244573.4 | 223142.2 | 0.91x | 219196.9 | 0.90x | -1.8% | 403288 | 403288 | 17 % |  |
| tsql/columns1000 | generated | scriptdom | 1671200.0 | 168981.2 | 0.10x | 171168.8 | 0.10x | +1.3% | 200489 | 200489 | 7 % |  |
| web/json.array10000 | generated | hand | 168664.8 | 187001.6 | 1.11x | 185321.1 | 1.10x | -0.9% | 720048 | 720048 | 29 % |  |
| sql/select20.at | generated | hand | 6773.8 | 17240.8 | 2.55x | 17480.0 | 2.58x | +1.4% | 21416 | 21416 | 17 % |  |
| sql/select20.window | generated | hand | 6742.0 | 17636.3 | 2.62x | 17724.3 | 2.63x | +0.5% | 21416 | 21416 | 9 % |  |
| sql/select20.scan | generated | control | 370.3 | 375.9 | 1.02x | 371.2 | 1.00x | -1.2% | 0 | 0 | 8 % |  |
| tsql/select20.scan | generated | control | 379.1 | 371.8 | 0.98x | 378.0 | 1.00x | +1.7% | 0 | 0 | 16 % |  |
| el/ladder.scan | generated | control | 140.4 | 135.3 | 0.96x | 135.0 | 0.96x | -0.2% | 0 | 0 | 10 % |  |
| el/ladder.bool | generated | hand | 966.1 | 1685.0 | 1.74x | 1748.5 | 1.81x | +3.8% | 1776 | 1720 | 3 % |  |
| sql/refused-late.bool | generated | hand | 2578.9 | 11932.7 | 4.63x | 5758.7 | 2.23x | -51.7% | 13552 | 6616 | 5 % |  |
| sql/select20.bool | generated | hand | 6791.5 | 17733.2 | 2.61x | 17995.2 | 2.65x | +1.5% | 21448 | 21392 | 11 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4246900.0 | 4381550.0 | 1.03x | 4485700.0 | 1.06x | +2.4% | 6276660 | 6276684 | 30 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2675190.6 | 2663018.8 | 1.00x | 2618340.6 | 0.98x | -1.7% | 2994166 | 2994142 | 10 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6563750.0 | 6668768.8 | 1.02x | 6801575.0 | 1.04x | +2.0% | 8778600 | 8778600 | 7 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6593381.2 | 6637568.8 | 1.01x | 6973312.5 | 1.06x | +5.1% | 8775448 | 8775424 | 13 % |  |
| fix/Orders128.yield-string | generated | hand | 81635.3 | 276123.9 | 3.38x | 269560.2 | 3.30x | -2.4% | 117936 | 117936 | 14 % |  |
| web/media-type.quoted | generated | control | 246.8 | 300.6 | 1.22x | 301.0 | 1.22x | +0.2% | 816 | 816 | 27 % |  |
| el/ladder | generated | hand | 1016.2 | 1720.1 | 1.69x | 1772.3 | 1.74x | +3.0% | 1776 | 1776 | 30 % |  |
| el/ladder | immediate | hand | 1016.2 | 1215.7 | 1.20x | 1192.9 | 1.17x | -1.9% | 1784 | 1784 | 30 % |  |
| el/terms1000 | generated | hand | 111873.2 | 149305.3 | 1.33x | 151292.6 | 1.35x | +1.3% | 169104 | 169150 | 5 % |  |
| el/terms1000 | immediate | hand | 111873.2 | 125181.2 | 1.12x | 129917.9 | 1.16x | +3.8% | 177144 | 177120 | 5 % |  |
| sql/select20 | generated | hand | 6843.3 | 17863.5 | 2.61x | 17783.5 | 2.60x | -0.4% | 21448 | 21448 | 4 % |  |
| sql/refused-late | generated | hand | 2565.0 | 11973.0 | 4.67x | 12003.1 | 4.68x | +0.3% | 13552 | 13552 | 8 % |  |
