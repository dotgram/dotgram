Median of 5 of 5 runs, each in a process of its own; control 44.9 ns (the runs' controls: 45.9, 44.9, 44.2, 44.1, 45.1).
No run was dropped.
The spread column is the spread of the base reading between runs, not between rounds.

# Paired stand, 2026-10-15 02:38

AIAgentHome, processors 0-7,16-23 (taskset), default priority, control 44.9 ns.

This binary, and the libraries it holds as the control, was built from 31970005. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit 31970005, framework net10.0, no properties, emitted 4c3fef5e3fbf2a1c); after (commit 31970005, framework net10.0, no properties, emitted 4c3fef5e3fbf2a1c). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one, and `tape` is this process's own build of the same grammar on the tape (SQL-92, whose shipped parser is carried immediately and has no hand-written one).

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1390837.5 | 215150.0 | 0.15x | 207650.0 | 0.15x | -3.5% | 344384 | 344428 | 27 % | [-6.4%..+1.4%], 4 of 5 positive |
| tsql/insert-values.at | generated | scriptdom | 13541.0 | 695.3 | 0.05x | 686.4 | 0.05x | -1.3% | 1120 | 1120 | 4 % | [-1.8%..+2.0%], 3 of 5 positive |
| sql/conditions100.scan | generated | control | 5009.1 | 4952.4 | 0.99x | 4938.0 | 0.99x | -0.3% | 0 | 0 | 7 % | [-0.3%..+1.1%], 2 of 5 positive |
| sql/select20.bool | match-vs-bool | hand | 10112.4 | 5686.9 | 0.56x | 5340.1 | 0.53x | -6.1% | 7024 | 6968 | 7 % | [-6.3%..+3.6%], 3 of 5 positive |
| sql/literal | generated | hand | 85.5 | 104.0 | 1.22x | 109.7 | 1.28x | +5.5% | 160 | 160 | 4 % | [-0.9%..+8.0%], 4 of 5 positive |
| sql/comment | generated | hand | 3221.5 | 1785.6 | 0.55x | 1753.0 | 0.54x | -1.8% | 1864 | 1864 | 8 % | [-2.2%..+4.6%], 1 of 5 positive |
| sql/conditions100 | generated | hand | 70070.6 | 30980.1 | 0.44x | 30533.5 | 0.44x | -1.4% | 44136 | 44136 | 7 % | [-2.2%..-0.8%], 0 of 5 positive |
| sql/conditions1000 | generated | hand | 720626.6 | 318556.2 | 0.44x | 312669.5 | 0.43x | -1.8% | 440179 | 440179 | 7 % | [-2.1%..+0.5%], 2 of 5 positive |
| tsql/comment | generated | scriptdom | 33714.6 | 1127.7 | 0.03x | 1168.4 | 0.03x | +3.6% | 1024 | 1024 | 8 % | [-5.7%..+3.6%], 4 of 5 positive |
| tsql/insert-values | generated | scriptdom | 19832.9 | 1627.0 | 0.08x | 1687.6 | 0.09x | +3.7% | 1984 | 1984 | 9 % | [-2.0%..+4.4%], 3 of 5 positive |
| tsql/select-long | generated | scriptdom | 168925.0 | 11920.7 | 0.07x | 11709.2 | 0.07x | -1.8% | 11632 | 11632 | 6 % | [-3.7%..+1.8%], 1 of 5 positive |
| sql/arithmetic | generated | hand | 2271.0 | 1400.6 | 0.62x | 1388.7 | 0.61x | -0.8% | 1488 | 1488 | 7 % | [-0.8%..+1.9%], 3 of 5 positive |
| sql/nest8 | generated | hand | 2351.7 | 1461.8 | 0.62x | 1478.3 | 0.63x | +1.1% | 1104 | 1104 | 10 % | [-1.7%..+7.9%], 4 of 5 positive |
| sql/condition1 | generated | hand | 699.5 | 529.0 | 0.76x | 537.7 | 0.77x | +1.7% | 464 | 464 | 10 % | [-5.6%..+5.6%], 4 of 5 positive |
| sql/select1 | generated | hand | 1059.8 | 867.0 | 0.82x | 859.3 | 0.81x | -0.9% | 968 | 968 | 8 % | [-0.9%..+3.9%], 2 of 5 positive |
