The refusal ladders (RefusalLadders.cs, `DotGram.Benchmarks linearity-refused`, D57), 2026-09-20.

audit-unfixed-web.txt: the first audit, 91 series. Its binary was built from 9d12b35e (07:32), which PREDATES every D57 fix of the day
(b3bdc01f, 2ce5c9f2, 93d2261d, 2a0c627c are not ancestors of it; read from the DLLs' ProductVersion and `git merge-base --is-ancestor`), so its two
known exponentials (UriTemplate, a literal run then an unclosed {: 6.27; EmailAddress.TryParseList, a long atom then a bare @: 5.96) are the
calibration of the detector, and its expression-language interpolation exponentials (4.9-5.4) are true of that revision only. It is NOT a statement
about main and its numbers are not to be quoted for main. Cores 0-15, high priority; another session's runaway harness (cores 16-31) was alive for
most of it (09:34-09:41, disclosed by its owner): the exponents are what is read.
The first attempt of the day (not kept) hung: its first series carried a constant 24-character literal prefix, the first call at the first size was not
under the watchdog, and it ran for minutes. Every first call at a size is now a probe on a thread of its own with a 2 s watchdog.

audit-main-5d439127.txt: the rerun on a build of 5d439127 (main 35d2d315 plus the ladders as one shared file; the binary names its commit in its
first line), 101 series (the expression language's raw, verbatim and backslash forms and expr's three interpolation forms added; the SQL nested
parentheses renamed after sql-39: they cost the same closed). 10:19, cores 0-15, high priority, nothing else of mine running. Result: no series is
explosive, none is refused-then-accepted, none threw; 78 linear, 7 superlinear, 16 quadratic. The two known exponentials are gone (UriTemplate literal
run 0.70, EmailAddress atom 0.86), the interpolated strings of the expression language are flat in both readings (0.04-0.13), and EmailAddress
"words, then an unclosed <", exponential before b3bdc01f's second fix, is quadratic (1.98) after it. The columns: the exponent is the slope of log
time on log size over the largest sixteenth of the ladder; "projected at 64 KiB" carries the time at the largest size along that exponent to 65,536
characters. The last section is the baseline the guard (tests/DotGram.Tests.Slow/RefusalBaseline.txt) was written from.
