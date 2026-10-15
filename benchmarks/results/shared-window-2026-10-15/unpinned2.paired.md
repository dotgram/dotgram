Median of 5 of 5 runs, each in a process of its own; control 44.3 ns (the runs' controls: 44.3, 42.5, 43.0, 44.9, 44.6).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-10-15 03:04

AIAgentHome, processors 0-7,16-23 (taskset), default priority, control 44.3 ns.

This binary, and the libraries it holds as the control, was built from 31970005. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 31970005, framework net10.0, no properties, emitted 4c3fef5e3fbf2a1c); after (commit 31970005, framework net10.0, no properties, emitted 4c3fef5e3fbf2a1c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one, and `tape` is this process's own build of the same grammar on the tape (SQL-92, whose shipped parser is carried immediately and has no hand-written one).

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1536450.0 | 210325.0 | 0.14x | 218525.0 | 0.14x | +3.9% | 344428 | 344428 | 222 % | [-15.0%..+4.5%], 1 of 5 positive |
| tsql/insert-values.at | generated | scriptdom | 14097.0 | 733.8 | 0.05x | 704.7 | 0.05x | -4.0% | 1120 | 1120 | 26 % | [-6.6%..+3.4%], 1 of 5 positive |
| sql/conditions100.scan | generated | control | 4782.8 | 4981.6 | 1.04x | 4868.9 | 1.02x | -2.3% | 0 | 0 | 18 % | [-2.3%..+1.9%], 4 of 5 positive |
| sql/select20.bool | match-vs-bool | hand | 11037.3 | 5375.7 | 0.49x | 5606.1 | 0.51x | +4.3% | 7024 | 6968 | 38 % | [-1.4%..+10.8%], 3 of 5 positive |
| sql/literal | generated | hand | 94.8 | 116.2 | 1.23x | 118.8 | 1.25x | +2.2% | 160 | 160 | 26 % | [-2.9%..+3.3%], 4 of 5 positive |
| sql/comment | generated | hand | 3910.5 | 2085.7 | 0.53x | 2063.0 | 0.53x | -1.1% | 1864 | 1864 | 24 % | [-2.5%..+3.4%], 3 of 5 positive |
| sql/conditions100 | generated | hand | 89852.0 | 37445.2 | 0.42x | 37243.1 | 0.41x | -0.5% | 44136 | 44136 | 23 % | [-3.7%..+5.9%], 3 of 5 positive |
| sql/conditions1000 | generated | hand | 941896.9 | 388026.6 | 0.41x | 368826.6 | 0.39x | -4.9% | 440136 | 440136 | 27 % | [-9.1%..+3.5%], 1 of 5 positive |
| tsql/comment | generated | scriptdom | 40359.4 | 1312.2 | 0.03x | 1330.5 | 0.03x | +1.4% | 1024 | 1024 | 24 % | [-2.0%..+8.1%], 3 of 5 positive |
| tsql/insert-values | generated | scriptdom | 20646.0 | 1716.7 | 0.08x | 1712.6 | 0.08x | -0.2% | 1984 | 1984 | 31 % | [-0.2%..+7.2%], 4 of 5 positive |
| tsql/select-long | generated | scriptdom | 166221.7 | 11423.8 | 0.07x | 11356.8 | 0.07x | -0.6% | 11632 | 11632 | 26 % | [-29.2%..+2.4%], 1 of 5 positive |
| sql/arithmetic | generated | hand | 2536.3 | 1516.0 | 0.60x | 1555.8 | 0.61x | +2.6% | 1488 | 1488 | 25 % | [-2.9%..+8.8%], 3 of 5 positive |
| sql/nest8 | generated | hand | 2595.2 | 1528.5 | 0.59x | 1484.1 | 0.57x | -2.9% | 1104 | 1104 | 32 % | [-2.9%..+1.3%], 3 of 5 positive |
| sql/condition1 | generated | hand | 717.0 | 530.4 | 0.74x | 551.4 | 0.77x | +4.0% | 464 | 464 | 35 % | [-8.7%..+6.1%], 3 of 5 positive |
| sql/select1 | generated | hand | 1202.8 | 1003.4 | 0.83x | 972.1 | 0.81x | -3.1% | 968 | 968 | 22 % | [-3.1%..+4.5%], 4 of 5 positive |
