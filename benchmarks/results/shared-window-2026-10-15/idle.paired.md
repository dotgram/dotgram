Median of 5 of 5 runs, each in a process of its own; control 41.7 ns (the runs' controls: 41.7, 41.7, 41.6, 42.1, 41.6).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-10-15 02:33

AIAgentHome, processors 0-7,16-23 (taskset), default priority, control 41.7 ns.

This binary, and the libraries it holds as the control, was built from 31970005. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 31970005, framework net10.0, no properties, emitted 4c3fef5e3fbf2a1c); after (commit 31970005, framework net10.0, no properties, emitted 4c3fef5e3fbf2a1c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one, and `tape` is this process's own build of the same grammar on the tape (SQL-92, whose shipped parser is carried immediately and has no hand-written one).

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1545650.0 | 195637.5 | 0.13x | 199525.0 | 0.13x | +2.0% | 344384 | 344428 | 26 % | [-3.6%..+5.0%], 4 of 5 positive |
| tsql/insert-values.at | generated | scriptdom | 12275.2 | 636.1 | 0.05x | 634.7 | 0.05x | -0.2% | 1120 | 1120 | 3 % | [-2.6%..+4.7%], 3 of 5 positive |
| sql/conditions100.scan | generated | control | 4504.7 | 4575.2 | 1.02x | 4572.3 | 1.02x | -0.1% | 0 | 0 | 1 % | [-0.2%..+0.4%], 3 of 5 positive |
| sql/select20.bool | match-vs-bool | hand | 9427.1 | 5134.3 | 0.54x | 5025.4 | 0.53x | -2.1% | 7024 | 6968 | 4 % | [-10.9%..+5.9%], 2 of 5 positive |
| sql/literal | generated | hand | 82.3 | 99.2 | 1.21x | 99.5 | 1.21x | +0.3% | 160 | 160 | 7 % | [-1.0%..+4.1%], 4 of 5 positive |
| sql/comment | generated | hand | 2952.7 | 1645.6 | 0.56x | 1678.1 | 0.57x | +2.0% | 1864 | 1864 | 3 % | [-5.4%..+2.6%], 4 of 5 positive |
| sql/conditions100 | generated | hand | 66909.1 | 29265.5 | 0.44x | 29016.3 | 0.43x | -0.9% | 44136 | 44136 | 2 % | [-11.6%..+1.1%], 2 of 5 positive |
| sql/conditions1000 | generated | hand | 683156.2 | 298957.8 | 0.44x | 288170.3 | 0.42x | -3.6% | 440179 | 440136 | 2 % | [-10.8%..+4.3%], 1 of 5 positive |
| tsql/comment | generated | scriptdom | 31253.7 | 1081.6 | 0.03x | 1091.1 | 0.03x | +0.9% | 1024 | 1024 | 4 % | [-1.3%..+1.6%], 2 of 5 positive |
| tsql/insert-values | generated | scriptdom | 18383.9 | 1546.6 | 0.08x | 1564.4 | 0.09x | +1.2% | 1984 | 1984 | 3 % | [-3.5%..+3.2%], 2 of 5 positive |
| tsql/select-long | generated | scriptdom | 157254.1 | 10959.6 | 0.07x | 11082.8 | 0.07x | +1.1% | 11632 | 11632 | 1 % | [-1.0%..+1.1%], 4 of 5 positive |
| sql/arithmetic | generated | hand | 2159.1 | 1309.2 | 0.61x | 1299.7 | 0.60x | -0.7% | 1488 | 1488 | 5 % | [-2.4%..+0.5%], 2 of 5 positive |
| sql/nest8 | generated | hand | 2202.4 | 1355.8 | 0.62x | 1364.4 | 0.62x | +0.6% | 1104 | 1104 | 2 % | [-1.2%..+6.1%], 3 of 5 positive |
| sql/condition1 | generated | hand | 660.0 | 494.2 | 0.75x | 495.0 | 0.75x | +0.2% | 464 | 464 | 5 % | [-2.0%..+2.1%], 3 of 5 positive |
| sql/select1 | generated | hand | 974.3 | 801.3 | 0.82x | 799.1 | 0.82x | -0.3% | 968 | 968 | 3 % | [-1.2%..+0.6%], 2 of 5 positive |
