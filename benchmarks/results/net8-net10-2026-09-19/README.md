# The same commit on net10.0 and on net8.0, 2026-09-19

One row of each family, five runs of the stand each (three for the last leg), on IGOR-DESKTOP, cores 0-15, high priority. Taken as Igor's one-off, with the third leg added at the architect's request. Raw data beside this file: one directory per leg (`stand.md`, `stand.json`, `run-1`..), and the console log of each.

**What each leg is.** `net10`: the stand built for net10.0, the libraries (`DotGram.Finance`, `.Web`, `.Sql`, `.ExpressionLanguage`, `.Examples`) built for net10.0, run on .NET 10.0.12. `net8`: the same source built for net8.0, run on .NET 8.0.31; **the libraries are then their netstandard2.0 builds**, and `DotGram.Handwritten`, `DotGram.Examples` and the benchmarks project are compiled for net8.0. `net8-on-net10`: that net8.0 build run on the .NET 10.0.12 runtime (`DOTNET_ROLL_FORWARD=LatestMajor`). `net10-again`: the net10 build once more, three runs, as a drift check. Both builds are from one commit that is **not today's main**: `sql/select20` is at 72 us, before the lazy tokens. The commit hash printed in each `stand.md` header is the stand tree's HEAD when the leg ran, not the commit the build was made from. The build trees were throwaway worktrees with the project files retargeted; nothing of them was committed.

**The drift check.** `net10` against `net10-again`, per reading: within +-5%, the largest being the stream row of `fix/Order.stream` (generated -5.1%), and ScriptDom -4.1%, the hand `fix/Order.text` -4.1%, `stock-count` hand -3.8%. Any difference between legs below that is not a finding.

## Each reading, per leg (ns; then the ratio to the row's first reading, the hand parser or ScriptDom; then bytes a call)

| row | reading | net10 | net8 | net8 on net10 | net10 again |
| --- | --- | ---: | ---: | ---: | ---: |
| fix/Order.text | hand | 667 (1.00x, 920 B) | 648 (1.00x, 1,184 B) | 569 (1.00x, 920 B) | 640 (1.00x, 920 B) |
| fix/Order.text | generated | 492 (0.74x, 800 B) | 506 (0.78x, 832 B) | 423 (0.74x, 800 B) | 493 (0.77x, 800 B) |
| fix/Order.bytes | hand | 673 (1.00x, 920 B) | 661 (1.00x, 1,184 B) | 562 (1.00x, 920 B) | 673 (1.00x, 920 B) |
| fix/Order.bytes | generated | 604 (0.90x, 856 B) | 634 (0.96x, 888 B) | 526 (0.94x, 856 B) | 599 (0.89x, 856 B) |
| fix/Order.stream | hand | 807 (1.00x, 5,088 B) | 706 (1.00x, 5,088 B) | 665 (1.00x, 5,088 B) | 803 (1.00x, 5,088 B) |
| fix/Order.stream | generated | 2,362 (2.93x, 992 B) | 2,278 (3.22x, 992 B) | 2,080 (3.13x, 992 B) | 2,242 (2.79x, 992 B) |
| web/url.full | hand | 170 (1.00x, 328 B) | 144 (1.00x, 328 B) | 141 (1.00x, 328 B) | 175 (1.00x, 328 B) |
| web/url.full | generated | 240 (1.41x, 472 B) | 219 (1.52x, 472 B) | 208 (1.47x, 472 B) | 242 (1.38x, 472 B) |
| web/json.object | hand | 519 (1.00x, 1,976 B) | 463 (1.00x, 1,976 B) | 420 (1.00x, 1,976 B) | 513 (1.00x, 1,976 B) |
| web/json.object | generated | 809 (1.56x, 2,520 B) | 786 (1.70x, 2,520 B) | 672 (1.60x, 2,520 B) | 810 (1.58x, 2,520 B) |
| web/json.object | system-text-json | 476 (0.92x, 72 B) | 495 (1.07x, 72 B) | 410 (0.98x, 72 B) | 481 (0.94x, 72 B) |
| web/date-time.utc | hand | 27 (1.00x, 112 B) | 25 (1.00x, 112 B) | 22 (1.00x, 112 B) | 27 (1.00x, 112 B) |
| web/date-time.utc | generated | 56 (2.09x, 112 B) | 44 (1.80x, 112 B) | 49 (2.19x, 112 B) | 55 (2.05x, 112 B) |
| tsql/select-join | scriptdom | 32,274 (1.00x, 62,448 B) | 58,730 (1.00x, 62,800 B) | 29,872 (1.00x, 62,448 B) | 30,940 (1.00x, 61,512 B) |
| tsql/select-join | generated | 8,355 (0.26x, 3,280 B) | 8,352 (0.14x, 3,280 B) | 8,179 (0.27x, 3,280 B) | 8,452 (0.27x, 3,280 B) |
| tsql/select-join | located | 12,167 (0.38x, 3,280 B) | 12,345 (0.21x, 3,280 B) | 12,055 (0.40x, 3,280 B) | 12,386 (0.40x, 3,280 B) |
| feeds/stock-count.good.text | hand | 39,429 (1.00x, 197,664 B) | 34,498 (1.00x, 197,664 B) | 31,219 (1.00x, 197,664 B) | 37,948 (1.00x, 197,664 B) |
| feeds/stock-count.good.text | generated | 28,637 (0.73x, 111,168 B) | 29,366 (0.85x, 111,168 B) | 22,276 (0.71x, 111,168 B) | 28,296 (0.75x, 111,168 B) |
| el/ladder | hand | 994 (1.00x, 1,712 B) | 1,091 (1.00x, 1,712 B) | 899 (1.00x, 1,712 B) | 973 (1.00x, 1,712 B) |
| el/ladder | tape | 1,798 (1.81x, 1,616 B) | 1,906 (1.75x, 1,616 B) | 1,681 (1.87x, 1,616 B) | 1,792 (1.84x, 1,616 B) |
| el/ladder | immediate | 1,073 (1.08x, 1,624 B) | 1,205 (1.10x, 1,624 B) | 980 (1.09x, 1,624 B) | 1,053 (1.08x, 1,624 B) |
| sql/select20 | hand | 7,070 (1.00x, 23,976 B) | 5,892 (1.00x, 24,680 B) | 6,069 (1.00x, 23,976 B) | 7,219 (1.00x, 23,976 B) |
| sql/select20 | generated | 72,584 (10.27x, 21,328 B) | 70,428 (11.95x, 21,328 B) | 70,883 (11.68x, 21,328 B) | 74,015 (10.25x, 21,328 B) |

## What the table says

1. **The generated code is not slower on net8.0 by a margin the drift does not cover, but the ratio to the hand parser moves on some rows.** Generated over hand, net10 -> net8: `fix/Order.text` 0.74x -> 0.78x, `fix/Order.bytes` 0.90x -> 0.96x, `fix/Order.stream` 2.93x -> 3.22x, `web/url.full` 1.41x -> 1.52x, `web/json.object` 1.56x -> 1.70x, `web/date-time.utc` 2.09x -> 1.80x, stock count 0.73x -> 0.85x, `el/ladder` tape 1.81x -> 1.75x and immediate 1.08x -> 1.10x, `sql/select20` 10.27x -> 11.95x. The rows moved in both directions and by up to 0.12x, and the two runtimes differ in the hand parser as much as in the generated one (see 3), so a change of ratio is not a change of the generated code.
2. **The generated readings barely move between the two runtimes; the hand and the ScriptDom readings do.** net8 against net10, generated ns: `fix/Order.text` +2.9%, `.bytes` +4.9%, `.stream` -3.6%, `web/json.object` -2.8%, `tsql/select-join` -0.0%, `sql/select20` -3.0%, stock count +2.5%; hand ns: -2.9%, -1.7%, -12.5%, -10.7%, `sql/select20` -16.7%, stock count -12.5%. The extreme is **ScriptDom, +82.0% on net8** (58,730 ns against 32,274 for the same statement), so `tsql/select-join` is 0.14x of ScriptDom on net8 and 0.26x on net10 with the generated parser at 8,352 and 8,355 ns: the ratio is a fact about ScriptDom and the runtime. On the net10 runtime the same net8 build reads ScriptDom at 29,872 ns (-7.4% against net10), so the slowness is the net8 runtime's, not the build's.
3. **The `net8-on-net10` leg is faster than `net10` itself on almost every hand and generated reading, and I do not know why.** Against `net10`: `fix/Order.text` hand -14.7%, generated -14.0%; `.bytes` -16.5% and -13.0%; `.stream` -17.6% and -12.0%; `web/url.full` -16.9% and -13.3%; `web/json.object` -19.1% and -16.9%; `web/date-time.utc` -15.6% and -11.7%; stock count -20.8% and -22.2%; `el/ladder` hand -9.6%, tape -6.5%, immediate -8.7%. The rows that barely move are the T-SQL and SQL:2023 ones (`tsql/select-join` generated -2.1%, `sql/select20` generated -2.3%). Both sides of every row improve alike, so the ratios of the fix and web rows are the same as on net10 (0.74x for `fix/Order.text` on both), and the control (30.7 ns) is the same in both legs. That is beyond the +-5% of the drift check. What differs between the two legs is what the process loads: the libraries (netstandard2.0 build instead of the net10.0 build) and the hand parsers, examples and benchmark code compiled for net8.0 instead of net10.0. I have tested none of the explanations that offer themselves, so read it only as: **the platform a stand binary is built for changes what the hand parser costs by 10-20%, and any comparison of a ratio to the hand parser across builds has to hold the build fixed.**
4. **Allocation** is identical between the legs except on net8.0, where the hand `fix/Order.*` readings allocate 1,184 B against 920 B (`.text`, `.bytes`) and `sql/select20` hand 24,680 B against 23,976 B, and generated `fix/Order.text` 832 B against 800 B, `.bytes` 888 B against 856 B (net8 against net10, and `net8-on-net10` matches net10's figure): an allocation of the net8.0 build itself, not of the runtime.
