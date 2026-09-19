# The expression language's tape at 1,000 terms: which commit made it slow

`stand --el-terms dir...`: the tape of `(int x) => x + x + ... + x` at 1,000 terms, in microseconds a call, unpinned on
logical processors 16-31 (a rough figure, good for an eightfold effect), three seconds of warm-up, the median of seven
rounds of a hundred calls. The good side (2d9bd3b6) beside each step.

| step | commit | tape, 1,000 terms | verdict |
| --- | --- | ---: | --- |
| good | 2d9bd3b6 | 155-173 us | |
| bad | 8b0a3b89 | 1,331-1,352 us | |
| 11 of 21 | 2ed92091 | 165, 153 us (a first reading of 277 was noise) | good |
| 16 | 45155db6 | 253, 231 us (the good side read 250 in the same run: machine busy) | good |
| 19 | 7e13fe68 | 1,436, 1,351 us | bad |
| 18 | 164967f8 | 1,355, 1,371 us | bad |
| 17 | 584a7c1f | 1,337, 1,365 us | bad |

First bad commit: **584a7c1f** "Build a list a guard is handed in one walk, not one walk an element" (04:05 on
2026-09-19, session performance-ff). Its parent 45155db6 is good, and the commits between them (b4790d77, 5438ccd7)
touch no source. The small rows' lean (2d9bd3b6 against 8b0a3b89, window 26: tape +5..+15% on ladder, loop, block,
floor, refused-*, immediate flat) is not attributed to a commit by this: the bisection read one row.

## The remainder, after a3b4e410's change is applied

`stand --el-rows dir...`: the tape at terms100 and at the ladder row, every directory's build read in the same rounds
(fifteen of about ten milliseconds each, turn and turn about), unpinned on 16-31, three seconds of warm-up for each build
and row. The minimum of the fifteen rounds is what is quoted: the median follows the machine, the minimum does not. The
figure of a step is the ratio to 2d9bd3b6 in the same run. Where a step has the quadratic (584a7c1f and after),
a3b4e410's change to `Machine.Direct.Values.cs` is applied to it first (it applies from 584a7c1f on, not before, since
it names `_recoveryReads`).

| build | terms100 (min, us) | ratio | ladder (min, us) | ratio |
| --- | ---: | ---: | ---: | ---: |
| 2d9bd3b6 (good) | 16.17 | 1.00 | 1.92 | 1.00 |
| 2ed92091 (plain) | 15.96 | 0.99 | 2.02 | 1.04 |
| 45155db6 (plain) | 16.96 | 1.05 | 2.00 | 1.04 |
| 584a7c1f + a3b4e410 | 20.57 | 1.27 | 2.13 | 1.11 |
| 7cb9afba (expr's pair side) | 20.77 | 1.28 | 2.15 | 1.12 |

The remainder, +27% at a hundred terms and +11% on the ladder, is in the same commit, 584a7c1f: its parent is at 1.05 and
1.04, and the builds after it add nothing (1.28, 1.12). The paired stand read the same at the ends (window 25 against
window 26: terms100 21.2 against 16.9 us, ladder 2,142 against 1,959 ns).
