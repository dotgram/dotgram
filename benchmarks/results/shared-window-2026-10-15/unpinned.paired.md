Median of 4 of 5 runs, each in a process of its own; control 43.6 ns (the runs' controls: 44.3, 43.4, 43.7, 46.7, 43.6).
Dropped for a control more than 5% off the median: run 4.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-10-15 02:45

AIAgentHome, processors 0-7,16-23 (taskset), default priority, control 43.6 ns.

This binary, and the libraries it holds as the control, was built from 31970005. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 31970005, framework net10.0, no properties, emitted 4c3fef5e3fbf2a1c); after (commit 31970005, framework net10.0, no properties, emitted 4c3fef5e3fbf2a1c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one, and `tape` is this process's own build of the same grammar on the tape (SQL-92, whose shipped parser is carried immediately and has no hand-written one).

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1391106.2 | 221675.0 | 0.16x | 213712.5 | 0.15x | -3.6% | 344384 | 344406 | 22 % | [-12.5%..+0.2%], 1 of 4 positive |
| tsql/insert-values.at | generated | scriptdom | 13888.0 | 681.8 | 0.05x | 669.4 | 0.05x | -1.8% | 1120 | 1120 | 17 % | [-4.0%..+4.6%], 1 of 4 positive |
| sql/conditions100.scan | generated | control | 4902.1 | 4764.1 | 0.97x | 4765.8 | 0.97x | 0.0% | 0 | 0 | 18 % | [-0.7%..+0.2%], 1 of 4 positive |
| sql/select20.bool | match-vs-bool | hand | 10819.4 | 5554.1 | 0.51x | 5375.1 | 0.50x | -3.2% | 7024 | 6968 | 17 % | [-4.2%..-0.5%], 0 of 4 positive |
| sql/literal | generated | hand | 92.8 | 123.1 | 1.33x | 116.0 | 1.25x | -5.8% | 160 | 160 | 23 % | [-10.7%..+0.7%], 1 of 4 positive |
| sql/comment | generated | hand | 3615.9 | 1964.7 | 0.54x | 1999.4 | 0.55x | +1.8% | 1864 | 1864 | 33 % | [+1.2%..+5.6%], 4 of 4 positive |
| sql/conditions100 | generated | hand | 84356.6 | 34616.5 | 0.41x | 35461.4 | 0.42x | +2.4% | 44136 | 44136 | 31 % | [-2.8%..+8.6%], 2 of 4 positive |
| sql/conditions1000 | generated | hand | 846750.0 | 347784.0 | 0.41x | 358568.8 | 0.42x | +3.1% | 440158 | 440136 | 34 % | [-5.3%..+9.2%], 3 of 4 positive |
| tsql/comment | generated | scriptdom | 37403.8 | 1231.1 | 0.03x | 1319.4 | 0.04x | +7.2% | 1024 | 1024 | 28 % | [+0.9%..+8.0%], 4 of 4 positive |
| tsql/insert-values | generated | scriptdom | 20661.8 | 1799.5 | 0.09x | 1749.2 | 0.08x | -2.8% | 1984 | 1984 | 33 % | [-7.0%..+2.6%], 1 of 4 positive |
| tsql/select-long | generated | scriptdom | 179393.6 | 12096.6 | 0.07x | 12263.5 | 0.07x | +1.4% | 11632 | 11632 | 25 % | [-1.4%..+17.6%], 2 of 4 positive |
| sql/arithmetic | generated | hand | 2317.4 | 1400.9 | 0.60x | 1403.5 | 0.61x | +0.2% | 1488 | 1488 | 31 % | [-3.7%..+0.6%], 1 of 4 positive |
| sql/nest8 | generated | hand | 2568.3 | 1497.5 | 0.58x | 1440.2 | 0.56x | -3.8% | 1104 | 1104 | 28 % | [-7.4%..+8.7%], 2 of 4 positive |
| sql/condition1 | generated | hand | 787.8 | 555.0 | 0.70x | 534.5 | 0.68x | -3.7% | 464 | 464 | 27 % | [-7.3%..+0.6%], 2 of 4 positive |
| sql/select1 | generated | hand | 1183.3 | 960.4 | 0.81x | 959.0 | 0.81x | -0.1% | 968 | 968 | 33 % | [-2.0%..+3.2%], 3 of 4 positive |
