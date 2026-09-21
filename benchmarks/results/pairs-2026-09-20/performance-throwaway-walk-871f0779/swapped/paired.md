Median of 10 of 10 runs, each in a process of its own; control 31.2 ns (the runs' controls: 31.2, 31.2, 31.2, 31.0, 31.2, 31.0, 31.3, 31.2, 31.5, 31.2).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-21 14:46

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| tsql/conditions1000 | generated | scriptdom | 1276465.6 | 302040.6 | 0.24x | 304378.1 | 0.24x | +0.8% | 424449 | 424449 | 56 % | [-31.9%..+16.3%], 5 of 10 positive | -0.3% [-9.7%..+34.8%], 5 of 10 positive |
| sql/select20.at | generated | hand | 6776.9 | 17584.0 | 2.59x | 17587.6 | 2.60x | 0.0% | 21416 | 21416 | 3 % | [-2.5%..+1.3%], 6 of 10 positive | -0.5% [-1.2%..+1.7%], 5 of 10 positive |
| sql/select20.window | generated | hand | 6778.2 | 17989.9 | 2.65x | 18035.2 | 2.66x | +0.3% | 21416 | 21416 | 2 % | [-3.6%..+2.7%], 4 of 10 positive | -0.1% [-4.6%..+2.2%], 5 of 10 positive |
| sql/select20.scan | generated | control | 376.0 | 379.2 | 1.01x | 378.5 | 1.01x | -0.2% | 0 | 0 | 2 % | [-2.5%..+2.2%], 5 of 10 positive | +0.3% [-5.0%..+4.0%], 4 of 10 positive |
| tsql/select20.scan | generated | control | 379.1 | 378.6 | 1.00x | 377.8 | 1.00x | -0.2% | 0 | 0 | 3 % | [-3.1%..+2.2%], 4 of 10 positive | +0.1% [-4.3%..+1.2%], 3 of 10 positive |
| sql/select20.bool | generated | hand | 6767.5 | 18015.2 | 2.66x | 18036.7 | 2.67x | +0.1% | 21448 | 21392 | 5 % | [-2.6%..+1.7%], 5 of 10 positive | +0.5% [-1.1%..+5.2%], 9 of 10 positive |
| sql/literal | generated | hand | 55.0 | 186.6 | 3.39x | 140.2 | 2.55x | -24.9% | 160 | 160 | 12 % | [-31.3%..-14.3%], 0 of 10 positive | +0.1% [-11.3%..+18.1%], 6 of 10 positive |
| sql/conditions1000 | generated | hand | 487211.1 | 1198060.9 | 2.46x | 1201114.6 | 2.47x | +0.3% | 1616203 | 1616203 | 3 % | [-4.7%..+5.2%], 6 of 10 positive | +1.0% [-1.1%..+4.5%], 7 of 10 positive |
| sql/select1 | generated | hand | 651.6 | 1540.0 | 2.36x | 1506.4 | 2.31x | -2.2% | 1688 | 1688 | 7 % | [-8.2%..+3.1%], 1 of 10 positive | -0.9% [-3.5%..+3.6%], 4 of 10 positive |
| sql/select20 | generated | hand | 6780.4 | 18003.3 | 2.66x | 18017.1 | 2.66x | +0.1% | 21448 | 21448 | 3 % | [-2.4%..+5.4%], 6 of 10 positive | 0.0% [-1.1%..+2.8%], 8 of 10 positive |
