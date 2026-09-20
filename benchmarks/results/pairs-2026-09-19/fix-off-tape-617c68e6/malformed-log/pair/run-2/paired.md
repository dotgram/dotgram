# Paired stand, 2026-09-19 23:04

IGOR-DESKTOP, pinned to 0-15, high priority, control 31.4 ns.

The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.

| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| fix/OrderMalformed.text | generated | hand | 666.3 | 609.7 | 0.92x | 622.0 | 0.93x | +2.0% | 1248 | 1248 |
| fix/OrderMalformed.bytes | generated | hand | 697.4 | 778.1 | 1.12x | 760.9 | 1.09x | -2.2% | 1304 | 1304 |
| fix/OrderMalformed.stream | generated | hand | 830.5 | 1094.2 | 1.32x | 1119.3 | 1.35x | +2.3% | 1864 | 1864 |
| fix/OrderMalformed.log-text | generated | hand | 810.0 | 1271.3 | 1.57x | 673.7 | 0.83x | -47.0% | 1256 | 1256 |
| fix/OrderMalformed.log-bytes | generated | hand | 795.5 | 1461.6 | 1.84x | 864.6 | 1.09x | -40.8% | 1312 | 1312 |
| fix/OrderMalformed.log-stream | generated | hand | 798.8 | 2642.6 | 3.31x | 1237.0 | 1.55x | -53.2% | 1872 | 1872 |
