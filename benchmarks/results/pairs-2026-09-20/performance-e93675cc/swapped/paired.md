Median of 5 of 5 runs, each in a process of its own; control 30.7 ns (the runs' controls: 30.5, 30.7, 30.7, 30.7, 30.6).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-21 02:36

IGOR-DESKTOP, pinned to 0-15, high priority, control 30.7 ns.

This binary, and the libraries it holds as the control, was built from 278ee907. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e93675cc, framework net10.0, no properties, emitted 29a22662dd651417); after (commit 5ed9682b, framework net10.0, no properties, emitted 3afa74326eaf0055). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| fix/orders400.text | generated | hand | 245142.2 | 215098.4 | 0.88x | 218926.6 | 0.89x | +1.8% | 403288 | 403288 | 3 % | [-2.0%..+4.9%], 4 of 5 positive | +1.0% [-1.9%..+2.4%], 2 of 5 positive |
| tsql/columns1000 | generated | scriptdom | 1796562.5 | 173475.0 | 0.10x | 171943.8 | 0.10x | -0.9% | 200489 | 200489 | 13 % | [-3.7%..+20.0%], 2 of 5 positive | +0.9% [-13.9%..+21.5%], 3 of 5 positive |
| web/json.array10000 | generated | hand | 167485.9 | 186680.5 | 1.11x | 186570.3 | 1.11x | -0.1% | 720048 | 720048 | 14 % | [-2.3%..+2.1%], 3 of 5 positive | -1.3% [-1.5%..-0.4%], 0 of 5 positive |
| sql/select20.at | generated | hand | 6682.4 | 17615.1 | 2.64x | 17296.2 | 2.59x | -1.8% | 21416 | 21416 | 7 % | [-4.4%..+5.1%], 1 of 5 positive | +1.2% [-2.4%..+3.2%], 3 of 5 positive |
| sql/select20.window | generated | hand | 6715.4 | 17953.0 | 2.67x | 17644.3 | 2.63x | -1.7% | 21416 | 21416 | 6 % | [-3.9%..+4.1%], 1 of 5 positive | +1.0% [-1.1%..+3.0%], 2 of 5 positive |
| sql/select20.scan | generated | control | 374.4 | 371.2 | 0.99x | 372.1 | 0.99x | +0.3% | 0 | 0 | 2 % | [-3.1%..+0.9%], 3 of 5 positive | +0.3% [-0.8%..+1.1%], 3 of 5 positive |
| tsql/select20.scan | generated | control | 370.9 | 373.4 | 1.01x | 372.7 | 1.00x | -0.2% | 0 | 0 | 3 % | [-4.6%..+1.4%], 3 of 5 positive | +0.5% [-12.4%..+1.4%], 4 of 5 positive |
| el/ladder.scan | generated | control | 135.5 | 135.6 | 1.00x | 134.9 | 0.99x | -0.6% | 0 | 0 | 3 % | [-1.2%..+0.8%], 2 of 5 positive | -0.4% [-2.1%..+0.3%], 2 of 5 positive |
| el/ladder.bool | generated | hand | 964.6 | 1695.0 | 1.76x | 1695.5 | 1.76x | 0.0% | 1776 | 1720 | 7 % | [-6.9%..+1.7%], 2 of 5 positive | -3.1% [-4.7%..+1.1%], 1 of 5 positive |
| sql/refused-late.bool | generated | hand | 2545.1 | 11898.4 | 4.68x | 5712.9 | 2.24x | -52.0% | 13552 | 6616 | 3 % | [-53.5%..-50.8%], 0 of 5 positive | -51.3% [-53.5%..-51.0%], 0 of 5 positive |
| sql/select20.bool | generated | hand | 6696.2 | 17792.6 | 2.66x | 17809.0 | 2.66x | +0.1% | 21448 | 21392 | 6 % | [-3.8%..+6.2%], 3 of 5 positive | +1.4% [-2.4%..+2.6%], 3 of 5 positive |
| sql/refused-cliff-case-1391 | generated | control | 4432775.0 | 4535175.0 | 1.02x | 4484856.2 | 1.01x | -1.1% | 6276488 | 6276488 | 12 % | [-3.3%..+1.9%], 4 of 5 positive | +0.2% [-2.4%..+8.1%], 3 of 5 positive |
| sql/refused-cliff-case-1738 | generated | control | 11199837.5 | 5659587.5 | 0.51x | 9967100.0 | 0.89x | +76.1% | 7842195 | 74362470 | 111 % | [+58.6%..+151.0%], 5 of 5 positive |  |
| sql/refused-cliff-joins-891 | generated | control | 2454003.1 | 2520337.5 | 1.03x | 2490462.5 | 1.01x | -1.2% | 2994056 | 2994142 | 3 % | [-4.1%..+9.5%], 3 of 5 positive | +0.2% [-0.8%..+0.7%], 3 of 5 positive |
| sql/refused-cliff-joins-1113 | generated | control | 4223156.2 | 3278668.8 | 0.78x | 4249650.0 | 1.01x | +29.6% | 3740043 | 12948549 | 22 % | [+11.9%..+77.2%], 5 of 5 positive |  |
| sql/refused-cliff-joins-1738 | generated | control | 6617312.5 | 5182825.0 | 0.78x | 7030387.5 | 1.06x | +35.6% | 5840043 | 21143715 | 42 % | [+23.7%..+72.2%], 5 of 5 positive |  |
| sql/refused-cliff-joins-2172 | generated | control | 17536412.5 | 6294250.0 | 0.36x | 16492737.5 | 0.94x | +162.0% | 7298259 | 94724988 | 38 % | [+78.2%..+311.0%], 5 of 5 positive |  |
| sql/refused-cliff-paren-2715 | generated | control | 6412637.5 | 6692787.5 | 1.04x | 6466200.0 | 1.01x | -3.4% | 8778600 | 8778600 | 7 % | [-5.3%..+0.4%], 1 of 5 positive | +0.6% [-2.8%..+2.5%], 3 of 5 positive |
| sql/refused-cliff-and-2715 | generated | control | 6423650.0 | 6664193.8 | 1.04x | 6586575.0 | 1.03x | -1.2% | 8775448 | 8775448 | 22 % | [-10.4%..+2.3%], 1 of 5 positive | 0.0% [-1.4%..+2.4%], 3 of 5 positive |
| sql/refused-cliff-paren-3393 | generated | control | 14922725.0 | 8422887.5 | 0.56x | 14616775.0 | 0.98x | +73.5% | 10969785 | 94857969 | 134 % | [+60.7%..+303.7%], 5 of 5 positive |  |
| sql/refused-cliff-and-3393 | generated | control | 13430075.0 | 8121075.0 | 0.60x | 15825050.0 | 1.18x | +94.9% | 10966572 | 94854749 | 36 % | [+60.6%..+96.8%], 5 of 5 positive |  |
| el/parse-200k-tokens | generated | control | 15323250.0 | 15270075.0 | 1.00x | 14829325.0 | 0.97x | -2.9% | 16801593 | 16801507 | 3 % | [-16.6%..+0.3%], 1 of 5 positive |  |
| el/parse-500k-tokens | generated | control | 40412375.0 | 44297725.0 | 1.10x | 44502950.0 | 1.10x | +0.5% | 42002327 | 49503176 | 3 % | [-12.1%..+2.0%], 3 of 5 positive |  |
| sql/worst-columns-100k | generated | control | 423419300.0 | 274670350.0 | 0.65x | 432753750.0 | 1.02x | +57.6% | 95920882 | 999698470 | 6 % | [+56.0%..+70.6%], 5 of 5 positive |  |
| fix/Orders128.yield-string | generated | hand | 78792.6 | 249835.9 | 3.17x | 267495.7 | 3.39x | +7.1% | 117936 | 117936 | 4 % | [-0.4%..+10.8%], 4 of 5 positive | -4.1% [-7.9%..+5.6%], 1 of 5 positive |
| web/media-type.quoted | generated | control | 237.7 | 290.9 | 1.22x | 296.3 | 1.25x | +1.8% | 816 | 816 | 2 % | [-6.0%..+9.7%], 2 of 5 positive | -0.6% [-4.3%..+1.6%], 1 of 5 positive |
| el/ladder | generated | hand | 967.2 | 1693.4 | 1.75x | 1694.8 | 1.75x | +0.1% | 1776 | 1776 | 6 % | [-5.6%..+2.4%], 2 of 5 positive | -1.8% [-3.6%..+1.6%], 2 of 5 positive |
| el/ladder | immediate | hand | 967.2 | 1136.5 | 1.18x | 1137.6 | 1.18x | +0.1% | 1784 | 1784 | 6 % | [-0.8%..+0.2%], 2 of 5 positive | -0.7% [-6.5%..+2.2%], 3 of 5 positive |
| el/terms1000 | generated | hand | 103044.4 | 142970.1 | 1.39x | 140464.4 | 1.36x | -1.8% | 169104 | 169104 | 27 % | [-17.0%..+0.4%], 1 of 5 positive | -1.3% [-3.7%..-0.6%], 0 of 5 positive |
| el/terms1000 | immediate | hand | 103044.4 | 117263.2 | 1.14x | 115386.4 | 1.12x | -1.6% | 177120 | 177120 | 27 % | [-5.9%..+0.6%], 1 of 5 positive | -2.3% [-4.3%..+0.0%], 0 of 5 positive |
| sql/select20 | generated | hand | 6706.3 | 18809.9 | 2.80x | 17811.3 | 2.66x | -5.3% | 21448 | 21448 | 7 % | [-8.0%..+5.8%], 1 of 5 positive | 0.0% [-2.2%..+1.2%], 3 of 5 positive |
| sql/refused-late | generated | hand | 2528.4 | 12645.0 | 5.00x | 11645.6 | 4.61x | -7.9% | 13552 | 13552 | 3 % | [-8.0%..+1.4%], 1 of 5 positive | +0.3% [-3.3%..+0.9%], 3 of 5 positive |
