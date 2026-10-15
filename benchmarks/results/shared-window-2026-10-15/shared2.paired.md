Median of 5 of 5 runs, each in a process of its own; control 44.7 ns (the runs' controls: 46.5, 45.5, 44.2, 44.5, 44.7).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-10-15 02:57

AIAgentHome, processors 0-7,16-23 (taskset), default priority, control 44.7 ns.

This binary, and the libraries it holds as the control, was built from 31970005. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 31970005, framework net10.0, no properties, emitted 4c3fef5e3fbf2a1c); after (commit 31970005, framework net10.0, no properties, emitted 4c3fef5e3fbf2a1c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one, and `tape` is this process's own build of the same grammar on the tape (SQL-92, whose shipped parser is carried immediately and has no hand-written one).

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1386387.5 | 224587.5 | 0.16x | 219087.5 | 0.16x | -2.4% | 344384 | 344428 | 62 % | [-4.6%..+4.4%], 2 of 5 positive |
| tsql/insert-values.at | generated | scriptdom | 13510.7 | 672.5 | 0.05x | 695.8 | 0.05x | +3.5% | 1120 | 1120 | 8 % | [-1.5%..+4.1%], 2 of 5 positive |
| sql/conditions100.scan | generated | control | 4946.4 | 4957.0 | 1.00x | 5093.4 | 1.03x | +2.8% | 0 | 0 | 13 % | [+0.1%..+8.1%], 5 of 5 positive |
| sql/select20.bool | match-vs-bool | hand | 10395.8 | 5527.3 | 0.53x | 5332.2 | 0.51x | -3.5% | 7024 | 6968 | 5 % | [-9.7%..+6.5%], 1 of 5 positive |
| sql/literal | generated | hand | 87.2 | 104.9 | 1.20x | 106.2 | 1.22x | +1.2% | 160 | 160 | 10 % | [-2.9%..+4.7%], 3 of 5 positive |
| sql/comment | generated | hand | 3092.3 | 1788.1 | 0.58x | 1772.0 | 0.57x | -0.9% | 1864 | 1864 | 2 % | [-2.5%..-0.1%], 0 of 5 positive |
| sql/conditions100 | generated | hand | 69831.1 | 30958.4 | 0.44x | 30873.6 | 0.44x | -0.3% | 44136 | 44136 | 2 % | [-1.9%..-0.3%], 0 of 5 positive |
| sql/conditions1000 | generated | hand | 721781.2 | 314500.8 | 0.44x | 317932.8 | 0.44x | +1.1% | 440136 | 440136 | 7 % | [-2.2%..+1.1%], 4 of 5 positive |
| tsql/comment | generated | scriptdom | 32937.6 | 1089.0 | 0.03x | 1062.8 | 0.03x | -2.4% | 1024 | 1024 | 4 % | [-2.4%..+2.7%], 2 of 5 positive |
| tsql/insert-values | generated | scriptdom | 19421.2 | 1649.2 | 0.08x | 1630.9 | 0.08x | -1.1% | 1984 | 1984 | 3 % | [-3.4%..+5.6%], 3 of 5 positive |
| tsql/select-long | generated | scriptdom | 167091.2 | 11622.1 | 0.07x | 11545.1 | 0.07x | -0.7% | 11632 | 11632 | 6 % | [-0.7%..+1.0%], 2 of 5 positive |
| sql/arithmetic | generated | hand | 2329.2 | 1391.3 | 0.60x | 1390.0 | 0.60x | -0.1% | 1488 | 1488 | 4 % | [-2.4%..+2.2%], 3 of 5 positive |
| sql/nest8 | generated | hand | 2339.9 | 1447.0 | 0.62x | 1495.3 | 0.64x | +3.3% | 1104 | 1104 | 5 % | [-1.1%..+4.0%], 3 of 5 positive |
| sql/condition1 | generated | hand | 700.5 | 519.2 | 0.74x | 522.4 | 0.75x | +0.6% | 464 | 464 | 5 % | [-1.8%..+0.6%], 2 of 5 positive |
| sql/select1 | generated | hand | 1043.3 | 862.7 | 0.83x | 873.3 | 0.84x | +1.2% | 968 | 968 | 5 % | [-0.8%..+1.5%], 4 of 5 positive |
