# Paired stand, 2026-09-21 14:14

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.3 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit 871f0779, framework net10.0, no properties, emitted a06e1cf7e2550eb3). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 1541018.8 | 307237.5 | 0.20x | 314687.5 | 0.20x | +2.4% | 424449 | 424449 | 901 % |  |
| sql/select20.at | generated | hand | 6842.3 | 17861.9 | 2.61x | 17727.1 | 2.59x | -0.8% | 21416 | 21416 | 10 % |  |
| sql/select20.window | generated | hand | 6834.5 | 18227.7 | 2.67x | 17941.5 | 2.63x | -1.6% | 21416 | 21416 | 4 % |  |
| sql/select20.scan | generated | control | 378.0 | 380.7 | 1.01x | 385.0 | 1.02x | +1.1% | 0 | 0 | 14 % |  |
| tsql/select20.scan | generated | control | 380.6 | 380.5 | 1.00x | 378.3 | 0.99x | -0.6% | 0 | 0 | 3 % |  |
| sql/select20.bool | generated | hand | 6788.5 | 18188.1 | 2.68x | 18091.7 | 2.67x | -0.5% | 21448 | 21392 | 2 % |  |
| sql/literal | generated | hand | 55.1 | 137.8 | 2.50x | 190.1 | 3.45x | +38.0% | 160 | 160 | 5 % |  |
| sql/conditions1000 | generated | hand | 487064.5 | 1200497.7 | 2.46x | 1182386.3 | 2.43x | -1.5% | 1616203 | 1616203 | 3 % |  |
| sql/select1 | generated | hand | 654.2 | 1522.0 | 2.33x | 1518.4 | 2.32x | -0.2% | 1688 | 1688 | 3 % |  |
| sql/select20 | generated | hand | 6803.5 | 18163.6 | 2.67x | 18008.4 | 2.65x | -0.9% | 21448 | 21448 | 7 % |  |
