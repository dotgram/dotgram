# What share of a built assembly is the grammar text that travels on the class, 2026-09-20

The architect's question: `Portable` follows the visibility of a host, so a public host writes the whole text of its grammar as the argument of `[GramSource]` on the class; how much of the shipped assembly is that. The measure is the **size of the built file on disk** (Release, one build of each, from origin/main at 3d0d3434 in a scratch worktree), as it is and with `Portable = false` written on every grammar host of the project (`Sql92Parser`, `SqlStandardParser`, `TransactSqlParser`, and `ExpressionParser`), and the same file compressed with gzip, since a package is a zip and text compresses where compiled code does not. `portable.ps1.txt` is the script, `sizes.txt` the table. Nothing was timed.

| assembly | framework | as it is | Portable = false | difference | of the file | compressed, as it is | compressed, false | difference |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| DotGram.Sql | net10.0 | 19,811,840 | 18,887,680 | 924,160 | 4.7% | 6,564,893 | 6,182,052 | 382,841 (5.8%) |
| DotGram.Sql | netstandard2.0 | 19,836,416 | 18,913,280 | 923,136 | 4.7% | 6,587,811 | 6,204,823 | 382,988 (5.8%) |
| DotGram.ExpressionLanguage | net10.0 | 1,651,200 | 1,563,648 | 87,552 | 5.3% | 661,102 | 622,494 | 38,608 (5.8%) |
| DotGram.ExpressionLanguage | netstandard2.0 | 1,690,112 | 1,602,560 | 87,552 | 5.2% | 691,832 | 653,455 | 38,377 (5.5%) |

So it is **a few percent of the assembly, about 5%, on disk and in the package**, not half.

Two things to read with it:
- **Sql: the difference is 924 KB, and the three texts sum to 697,784 bytes** (the architect's figures: 441,215 + 223,698 + 32,871). The difference is 226 KB more than the texts. I have not found where the extra 226 KB goes (a text carried by a class I did not count, or once more than I assumed); it is about the size of the SQL:2023 grammar, 223,698, and that is a guess.
- **Expression language: what disappears is the second copy.** The grammar is also the argument of `[Gram("""...""")]` on the class, and that stays whatever `Portable` says; `Portable = false` removes only the carried copy, 87,552 bytes, which is the size of one copy of the grammar. The assembly with the option off still holds the text once.
