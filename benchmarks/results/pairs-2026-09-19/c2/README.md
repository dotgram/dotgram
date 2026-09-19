# C2 pair, 73b9e1c1 against 694bbe40 (performance-ff), taken 2026-09-19 00:28

Median of 5 runs, hand parser of this tree as the control.

**The `feeds/stock-count.good.*` and `.broken.*` rows in these files are not what they say.** Their
"good" lines were named `item12`, and the grammar's Name is letters only, so every line was a rejected
one in both builds; they measure the rejection path (which is quadratic, see `stock-slope`). Only
the `small.*` rows (letters only) and the FIX, EL and SQL rows are valid. The stand refuses such an
input now (`NotWhatItSays`).
