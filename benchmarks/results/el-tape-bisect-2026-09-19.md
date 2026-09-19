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
