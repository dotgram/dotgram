performance-ff's 6fa231b5 ("A refusal no longer rents the store it never touches") against its parent bae7ea7e.

Window 77, 2026-09-20 04:49 - 07:26, stand-bin55 (before f0095f69), sides b1-0 = bae7ea7e and b1-1 = 6fa231b5
(the commit is the ProductVersion of the sides' DLLs: 0.1.0+bae7ea7e... and 0.1.0+6fa231b5...; the sides were built
before Build-Side.ps1 wrote build.txt, so there is no emitted hash).
Five runs of the pair and five A/A runs of the parent (b1-0 against itself) on the rows named by --aa-only,
pooled in paired.md / paired.json. run-N and aa-run-N hold only paired.md: the per-run paired.json files are
1.3 MB each and are not kept.
Rows without an A/A column had none taken (D50): their change is a pair of medians and its range over the runs, not an effect.

CORRECTION (2026-09-20, after window 80): the rows named .bool in this pair compare the Match form of the before side with the bool form of the after side (StandBool.cs), two APIs and not two commits. Their halving (el/refused-early.bool, el/refused-late.bool, sql/refused-late.bool, and the bytes of the accepted .bool rows and the T-SQL script .bool rows) is the price of the Match form and is NOT an effect of 6fa231b5: window 80, a pair of unrelated commits, shows the same on the same rows, and its A/A of one build against itself has sql/refused-late.bool at -52.2%. The message that reported them as an effect was withdrawn. The +2..+3.5% on el/refused-early, el/refused-late and sql/refused-late (Match against Match, no A/A) is what an unrelated pair shows too (second-slot bias of short rows) and is not a price.
