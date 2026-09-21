# Paired stand, 2026-09-21 14:17

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.1 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 909656.2 | 301750.0 | 0.33x | 300956.2 | 0.33x | -0.3% | 424449 | 424449 | 26 % |  |
| sql/select20.at | generated | hand | 6708.3 | 17275.2 | 2.58x | 17304.9 | 2.58x | +0.2% | 21416 | 21416 | 45 % |  |
| sql/select20.window | generated | hand | 6637.0 | 17636.4 | 2.66x | 17836.5 | 2.69x | +1.1% | 21416 | 21416 | 3 % |  |
| sql/select20.scan | generated | control | 370.8 | 372.6 | 1.00x | 376.5 | 1.02x | +1.0% | 0 | 0 | 7 % |  |
| tsql/select20.scan | generated | control | 378.0 | 391.6 | 1.04x | 372.0 | 0.98x | -5.0% | 0 | 0 | 3 % |  |
| sql/select20.bool | generated | hand | 6733.0 | 17624.1 | 2.62x | 17942.0 | 2.66x | +1.8% | 21448 | 21392 | 19 % |  |
| sql/literal | generated | hand | 54.8 | 145.2 | 2.65x | 141.7 | 2.59x | -2.4% | 160 | 160 | 19 % |  |
| sql/conditions1000 | generated | hand | 488270.3 | 1172292.6 | 2.40x | 1174036.3 | 2.40x | +0.1% | 1616203 | 1616203 | 2 % |  |
| sql/select1 | generated | hand | 638.0 | 1478.8 | 2.32x | 1464.3 | 2.30x | -1.0% | 1688 | 1688 | 9 % |  |
| sql/select20 | generated | hand | 6736.0 | 17644.0 | 2.62x | 17758.9 | 2.64x | +0.7% | 21448 | 21448 | 2 % |  |
