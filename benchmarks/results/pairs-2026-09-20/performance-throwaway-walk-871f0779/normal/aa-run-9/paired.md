# Paired stand, 2026-09-21 14:19

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.2 ns.

This binary, and the libraries it holds as the control, was built from 21c80961. JIT: DOTNET_TieredCompilation unset, DOTNET_TieredPGO unset, DOTNET_TC_QuickJitForLoops unset, DOTNET_ReadyToRun unset (unset is the runtime's default: tiered compilation on, dynamic PGO on; a row is warmed to a stable reading before it is timed).

Sides: before (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2); after (commit e29c9144, framework net10.0, no properties, emitted 9a576c87d31f69d2). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)

**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| tsql/conditions1000 | generated | scriptdom | 847887.5 | 296437.5 | 0.35x | 294825.0 | 0.35x | -0.5% | 424449 | 424449 | 55 % |  |
| sql/select20.at | generated | hand | 6767.5 | 17735.4 | 2.62x | 17364.8 | 2.57x | -2.1% | 21416 | 21416 | 61 % |  |
| sql/select20.window | generated | hand | 6685.4 | 17735.5 | 2.65x | 17673.6 | 2.64x | -0.3% | 21416 | 21416 | 1 % |  |
| sql/select20.scan | generated | control | 373.7 | 383.4 | 1.03x | 378.3 | 1.01x | -1.3% | 0 | 0 | 1 % |  |
| tsql/select20.scan | generated | control | 373.5 | 381.9 | 1.02x | 378.2 | 1.01x | -1.0% | 0 | 0 | 1 % |  |
| sql/select20.bool | generated | hand | 6673.8 | 17762.4 | 2.66x | 17788.3 | 2.67x | +0.1% | 21448 | 21392 | 1 % |  |
| sql/literal | generated | hand | 54.6 | 145.4 | 2.67x | 136.2 | 2.50x | -6.4% | 160 | 160 | 6 % |  |
| sql/conditions1000 | generated | hand | 479277.7 | 1189333.2 | 2.48x | 1172664.5 | 2.45x | -1.4% | 1616203 | 1616203 | 1 % |  |
| sql/select1 | generated | hand | 654.4 | 1493.8 | 2.28x | 1498.2 | 2.29x | +0.3% | 1688 | 1688 | 3 % |  |
| sql/select20 | generated | hand | 6749.1 | 17877.0 | 2.65x | 17666.7 | 2.62x | -1.2% | 21448 | 21472 | 2 % |  |
