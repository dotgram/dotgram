Median of 5 of 5 runs, each in a process of its own; control 42.1 ns (the runs' controls: 41.4, 42.0, 42.2, 42.2, 42.1).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-10-15 02:52

AIAgentHome, processors 0-7,16-23 (taskset), default priority, control 42.1 ns.

This binary, and the libraries it holds as the control, was built from 31970005. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 31970005, framework net10.0, no properties, emitted 4c3fef5e3fbf2a1c); after (commit 31970005, framework net10.0, no properties, emitted 4c3fef5e3fbf2a1c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one, and `tape` is this process's own build of the same grammar on the tape (SQL-92, whose shipped parser is carried immediately and has no hand-written one).

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1578262.5 | 204750.0 | 0.13x | 211000.0 | 0.13x | +3.1% | 344384 | 344428 | 64 % | [-0.6%..+5.1%], 4 of 5 positive |
| tsql/insert-values.at | generated | scriptdom | 12238.8 | 645.0 | 0.05x | 650.1 | 0.05x | +0.8% | 1120 | 1120 | 6 % | [-6.3%..+5.2%], 3 of 5 positive |
| sql/conditions100.scan | generated | control | 4536.9 | 4601.3 | 1.01x | 4647.2 | 1.02x | +1.0% | 0 | 0 | 77 % | [-2.8%..+14.7%], 3 of 5 positive |
| sql/select20.bool | match-vs-bool | hand | 10318.8 | 5400.4 | 0.52x | 5232.3 | 0.51x | -3.1% | 7024 | 6968 | 11 % | [-9.9%..-0.2%], 0 of 5 positive |
| sql/literal | generated | hand | 82.5 | 103.3 | 1.25x | 103.2 | 1.25x | -0.1% | 160 | 160 | 2 % | [-1.3%..+2.5%], 1 of 5 positive |
| sql/comment | generated | hand | 3001.1 | 1671.2 | 0.56x | 1647.2 | 0.55x | -1.4% | 1864 | 1864 | 4 % | [-2.5%..+2.2%], 1 of 5 positive |
| sql/conditions100 | generated | hand | 66091.3 | 29111.5 | 0.44x | 28430.0 | 0.43x | -2.3% | 44136 | 44136 | 2 % | [-5.0%..+4.6%], 1 of 5 positive |
| sql/conditions1000 | generated | hand | 685895.3 | 294665.6 | 0.43x | 283489.8 | 0.41x | -3.8% | 440136 | 440136 | 2 % | [-6.5%..+3.5%], 2 of 5 positive |
| tsql/comment | generated | scriptdom | 31053.0 | 1092.3 | 0.04x | 1114.7 | 0.04x | +2.1% | 1024 | 1024 | 5 % | [-1.3%..+3.5%], 3 of 5 positive |
| tsql/insert-values | generated | scriptdom | 18351.5 | 1569.1 | 0.09x | 1577.2 | 0.09x | +0.5% | 1984 | 1984 | 4 % | [-2.5%..+4.4%], 2 of 5 positive |
| tsql/select-long | generated | scriptdom | 157201.4 | 11296.9 | 0.07x | 11419.7 | 0.07x | +1.1% | 11632 | 11632 | 3 % | [-2.4%..+1.6%], 3 of 5 positive |
| sql/arithmetic | generated | hand | 2152.0 | 1300.4 | 0.60x | 1291.0 | 0.60x | -0.7% | 1488 | 1488 | 3 % | [-2.6%..+1.6%], 2 of 5 positive |
| sql/nest8 | generated | hand | 2216.5 | 1380.4 | 0.62x | 1384.5 | 0.62x | +0.3% | 1104 | 1104 | 2 % | [-4.2%..+1.6%], 3 of 5 positive |
| sql/condition1 | generated | hand | 666.4 | 501.2 | 0.75x | 496.2 | 0.74x | -1.0% | 464 | 464 | 13 % | [-5.7%..+3.4%], 1 of 5 positive |
| sql/select1 | generated | hand | 986.0 | 815.7 | 0.83x | 824.7 | 0.84x | +1.1% | 968 | 968 | 4 % | [-1.0%..+1.7%], 3 of 5 positive |
