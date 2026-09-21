# Paired stand, 2026-09-21 01:51

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.6 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| fix/orders400.text | generated | hand | 246354.7 | 217603.1 | 0.88x | 210668.8 | 0.86x | -3.2% | 403288 | 403288 | 19 % |  |
| tsql/columns1000 | generated | scriptdom | 1709112.5 | 168181.2 | 0.10x | 174243.8 | 0.10x | +3.6% | 200489 | 200489 | 4 % |  |
| web/json.array10000 | generated | hand | 166175.8 | 197178.1 | 1.19x | 189702.3 | 1.14x | -3.8% | 720048 | 720048 | 17 % |  |
| sql/select20.at | generated | hand | 6783.1 | 17440.8 | 2.57x | 16983.1 | 2.50x | -2.6% | 21416 | 21416 | 3 % |  |
| sql/select20.window | generated | hand | 6777.7 | 17905.2 | 2.64x | 17631.4 | 2.60x | -1.5% | 21416 | 21416 | 4 % |  |
| sql/select20.scan | generated | control | 404.2 | 371.4 | 0.92x | 371.9 | 0.92x | +0.1% | 0 | 0 | 17 % |  |
| tsql/select20.scan | generated | control | 377.2 | 372.3 | 0.99x | 372.5 | 0.99x | 0.0% | 0 | 0 | 5 % |  |
| el/ladder.scan | generated | control | 137.8 | 135.7 | 0.98x | 135.0 | 0.98x | -0.5% | 0 | 0 | 15 % |  |
| el/ladder.bool | generated | hand | 953.7 | 1710.9 | 1.79x | 1748.4 | 1.83x | +2.2% | 1776 | 1720 | 3 % |  |
| sql/refused-late.bool | generated | hand | 2552.8 | 11953.7 | 4.68x | 5628.5 | 2.20x | -52.9% | 13552 | 6616 | 4 % |  |
| sql/select20.bool | generated | hand | 6711.8 | 18015.5 | 2.68x | 17456.8 | 2.60x | -3.1% | 21448 | 21392 | 16 % |  |
| sql/refused-cliff-case-1391 | generated | control | 4262475.0 | 4348475.0 | 1.02x | 4331150.0 | 1.02x | -0.4% | 6276641 | 6276598 | 21 % |  |
| sql/refused-cliff-joins-891 | generated | control | 2498337.5 | 2593743.8 | 1.04x | 2523396.9 | 1.01x | -2.7% | 2994142 | 2994166 | 10 % |  |
| sql/refused-cliff-paren-2715 | generated | control | 6400912.5 | 6304931.2 | 0.99x | 6240243.8 | 0.97x | -1.0% | 8778600 | 8778600 | 14 % |  |
| sql/refused-cliff-and-2715 | generated | control | 6385887.5 | 6305362.5 | 0.99x | 6334506.2 | 0.99x | +0.5% | 8775467 | 8775448 | 12 % |  |
| fix/Orders128.yield-string | generated | hand | 76999.0 | 265957.9 | 3.45x | 258873.8 | 3.36x | -2.7% | 117936 | 117936 | 4 % |  |
| web/media-type.quoted | generated | control | 238.1 | 281.7 | 1.18x | 283.6 | 1.19x | +0.7% | 816 | 816 | 12 % |  |
| el/ladder | generated | hand | 945.2 | 1694.0 | 1.79x | 1745.9 | 1.85x | +3.1% | 1776 | 1776 | 4 % |  |
| el/ladder | immediate | hand | 945.2 | 1137.8 | 1.20x | 1119.0 | 1.18x | -1.7% | 1784 | 1784 | 4 % |  |
| el/terms1000 | generated | hand | 109609.8 | 148659.2 | 1.36x | 149719.7 | 1.37x | +0.7% | 169104 | 169107 | 2 % |  |
| el/terms1000 | immediate | hand | 109609.8 | 126217.5 | 1.15x | 123136.2 | 1.12x | -2.4% | 177120 | 177120 | 2 % |  |
| sql/select20 | generated | hand | 6638.1 | 17799.2 | 2.68x | 17595.9 | 2.65x | -1.1% | 21448 | 21448 | 9 % |  |
| sql/refused-late | generated | hand | 2507.3 | 11845.4 | 4.72x | 11471.9 | 4.58x | -3.2% | 13552 | 13552 | 5 % |  |
