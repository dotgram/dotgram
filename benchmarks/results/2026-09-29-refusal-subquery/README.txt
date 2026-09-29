T-SQL, "subqueries opened and not closed": the reported rise of the refusal's exponent from 0.91 (2026-09-20)
to 1.72, counted rather than timed. 2026-09-29. Verdict: the work did not change; 1.72 does not reproduce here, on CI, or in the guard.

THE SERIES. RefusalLadders' `T-SQL statement | subqueries opened and not closed`: "SELECT * FROM " + n times
"(SELECT * FROM " + "t", through TransactSqlParser.TryParseStatement(t).IsSuccess, n from 4 to 188.

WHERE 1.72 CAME FROM IS NOT RECORDED. The figure reached the queue in a handoff note with no build, machine, date
or output attached, and no file of the old machine's scratch holds it. So it was not reproduced from its source.
It was tested against three instruments instead.

1. COUNTS (instrument.py, probe.cs, rec.csproj, run.sh; each filed with a .txt suffix so nothing globs it).
   The generated parsers of DotGram.Sql, and every handwritten .cs file of the project, are rewritten with a
   counter at the head of every method and in the condition of every for and while loop (so an unbraced body
   is counted too); a foreach is counted in its braced body. 15,253 sites in the generated files and 15,917 in
   the handwritten ones at 3f091b49, with 3 and 45 loops left uncounted (no braces, or not one line). The
   parse is refused at every depth, on a 256 MiB thread so that no hand-off is in it, after one warm-up call.

   counts-by-commit.txt holds nine revisions: main on 2026-09-20 at 20:38 (06d34791), seven generator commits
   between, and 3f091b49. At 50, 100, 200 and 400 levels:

                                        06d34791         3f091b49
     reader method entries           1,330 -> 10,430  1,330 -> 10,430    identical at every depth and revision
     loop turns                      2,563 -> 20,103  2,563 -> 20,103    identical at every depth and revision
     other method entries            2,895 -> 22,190  2,989 -> 22,940    +1.9 a level: the stack probe (below)
     handwritten method entries        204 ->  1,604    208 ->  1,608    +4, constant

   Every one of these is linear: 26.0 reader entries a level, 50.1 loop turns a level. Over 30,000 counted
   sites, one grows faster than linear between 200 and 400 levels, and it is `Clear`'s loop at 16 -> 40
   turns: a pool that has doubled its capacity, not a quantity of the input.

   The one change the counts show is 4b6cc0b0 (every entry probes the stack, +4 a level) and 95a195a5 (every
   fourth, +1.9 a level). Both are linear.

   Allocation: 33,456 bytes at 188 levels at both ends, which is the 09-20 audit's 32.7 KB to the byte.

2. THE GUARD'S OWN LADDER. RefusalLadders.Run() from 3f091b49's file, cut to this series and a linear
   control, over each revision's DotGram.Sql.dll (hashes checked to differ: 4b8b71d49018 at 06d34791,
   a7f056c89e39 at 3f091b49). This is a clock, used only to see whether the effect exists in the instrument
   that reported it. No window was open, the machine was loaded (load 8 to 15 on 32 cores), and these exponents
   are not results:
     default stack:  06d34791  0.94 0.94 / 0.95 0.96     3f091b49  0.94 0.96 / 0.97 0.96
     1 MiB thread:   06d34791  0.95 0.96 / 0.96 0.95     3f091b49  0.96 0.95 / 0.95 0.96
   Linear and alike. The first run in a fresh process reads 0.06 to 0.82: that is the cold tiering the
   ladder's two passes exist for, and it does not reach the guard, which runs after others in one process.

3. THE GUARD ITSELF. DotGram.Tests.Slow -class DotGram.Tests.RefusalGuardTests at 3f091b49: 86 total, 0
   failed, 1 skipped. That one skip is a control planted on purpose (EmailAddress "words, then an unclosed <"
   written as Linear in the output directory's baseline file) to show that a REPORT ONLY row is counted as
   skipped in the summary. It was, with its exponents (1.96, 1.97, 1.97). The T-SQL series was not reported.
   CI run 36516197852 on 3f091b49 says "skipped: 0" for the build, checked and windows jobs, so the series was
   within its Linear line on Linux twice and on Windows once.

WHAT MOVED THAT THE COUNTS CANNOT SEE: THE STACK A LEVEL. On a 1 MiB thread the refusal carries itself onto a
thread of its own (the generated Deepen) from between 175 and 200 levels at 06d34791 and from between 150 and 175
at 3f091b49, which is about 13% more stack a level. The same holds whether the reader has warmed up or is held at
tier-0. The ladder's top size is 188, so on a runner whose test thread has 1 MiB, the ladder's top point would
now include two thread creations a call. It was measured in the ladder at 1 MiB and did not show: 84.7 us against
79.0 at 188, and the exponent was unchanged. It is recorded because it is the one real difference between the two
ends. Which commit moved it was not bisected.

WHAT THIS DOES NOT PROVE. The counts cover the generated code and DotGram.Sql's own. They do not cover the
runtime (JIT, GC, thread creation) or the machine. This says nothing about what was measured on the old machine.
The one plausible way a clock alone could rise while the work does not: the thread hop at about 175 levels now
falls inside the ladder's range, whose top rung is 188. A rung that pays for thread creations can bend a fit
over the top of a ladder, more so on Windows, where creating a thread costs more. It did not bend it here.

SINCE THEN. The guard prints every series' fitted exponent with where it was measured (commit, OS, runtime,
machine), so that the next figure like this one has a source.
