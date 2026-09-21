Median of 10 of 10 runs, each in a process of its own; control 31.2 ns (the runs' controls: 31.3, 31.3, 31.1, 31.3, 31.3, 31.2, 31.3, 31.2, 31.2, 31.0).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-09-21 14:22

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs | A/A of the parent |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| tsql/conditions1000 | generated | scriptdom | 1508296.9 | 307653.1 | 0.20x | 308800.0 | 0.20x | +0.4% | 424449 | 424449 | 45 % | [-3.5%..+22.6%], 6 of 10 positive | -0.6% [-8.6%..+5.8%], 4 of 10 positive |
| sql/select20.at | generated | hand | 6831.7 | 17632.0 | 2.58x | 17669.8 | 2.59x | +0.2% | 21416 | 21416 | 3 % | [-2.0%..+10.1%], 5 of 10 positive | -1.1% [-6.1%..+8.1%], 4 of 10 positive |
| sql/select20.window | generated | hand | 6790.9 | 18121.5 | 2.67x | 18082.2 | 2.66x | -0.2% | 21416 | 21416 | 2 % | [-3.1%..+5.9%], 5 of 10 positive | -0.3% [-6.2%..+1.4%], 4 of 10 positive |
| sql/select20.scan | generated | control | 377.8 | 379.5 | 1.00x | 379.4 | 1.00x | 0.0% | 0 | 0 | 4 % | [-1.6%..+6.9%], 5 of 10 positive | 0.0% [-1.3%..+1.0%], 5 of 10 positive |
| tsql/select20.scan | generated | control | 375.9 | 379.0 | 1.01x | 378.3 | 1.01x | -0.2% | 0 | 0 | 2 % | [-1.6%..+1.3%], 4 of 10 positive | -0.6% [-5.0%..+0.3%], 3 of 10 positive |
| sql/select20.bool | generated | hand | 6767.5 | 17930.8 | 2.65x | 18180.4 | 2.69x | +1.4% | 21448 | 21392 | 4 % | [-1.5%..+2.3%], 8 of 10 positive | -0.1% [-6.4%..+1.8%], 5 of 10 positive |
| sql/literal | generated | hand | 54.9 | 136.3 | 2.48x | 186.6 | 3.40x | +37.0% | 160 | 160 | 10 % | [+27.9%..+49.4%], 10 of 10 positive | -0.6% [-6.4%..+6.8%], 4 of 10 positive |
| sql/conditions1000 | generated | hand | 487205.9 | 1185146.1 | 2.43x | 1187141.6 | 2.44x | +0.2% | 1616203 | 1616203 | 3 % | [-2.2%..+6.8%], 6 of 10 positive | -1.1% [-3.8%..+0.8%], 4 of 10 positive |
| sql/select1 | generated | hand | 651.6 | 1505.0 | 2.31x | 1532.3 | 2.35x | +1.8% | 1688 | 1688 | 4 % | [-0.3%..+8.4%], 8 of 10 positive | -0.3% [-6.4%..+5.6%], 4 of 10 positive |
| sql/select20 | generated | hand | 6777.7 | 18004.4 | 2.66x | 18104.8 | 2.67x | +0.6% | 21448 | 21448 | 3 % | [-2.8%..+2.2%], 6 of 10 positive | -0.3% [-5.6%..+3.4%], 4 of 10 positive |
