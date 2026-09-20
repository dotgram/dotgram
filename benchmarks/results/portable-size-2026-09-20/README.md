# What share of a built assembly is the grammar text that travels on the class, 2026-09-20

The architect's question: `Portable` follows the visibility of a host, so a public host writes the whole text of its grammar as the argument of `[GramSource]` on the class; how much of the shipped assembly is that. The measure is the **size of the built file on disk** (Release, one build of each, from origin/main at 3d0d3434 in a scratch worktree), as it is and with `Portable = false` written on every grammar host of the project (`Sql92Parser`, `SqlStandardParser`, `TransactSqlParser`, and `ExpressionParser`), and the same file compressed with gzip, since a package is a zip and text compresses where compiled code does not. `portable.ps1.txt` is the script, `sizes.txt` the table. Nothing was timed.

| assembly | framework | as it is | Portable = false | difference | of the file | compressed, as it is | compressed, false | difference |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| DotGram.Sql | net10.0 | 19,811,840 | 18,887,680 | 924,160 | 4.7% | 6,564,893 | 6,182,052 | 382,841 (5.8%) |
| DotGram.Sql | netstandard2.0 | 19,836,416 | 18,913,280 | 923,136 | 4.7% | 6,587,811 | 6,204,823 | 382,988 (5.8%) |
| DotGram.ExpressionLanguage | net10.0 | 1,651,200 | 1,563,648 | 87,552 | 5.3% | 661,102 | 622,494 | 38,608 (5.8%) |
| DotGram.ExpressionLanguage | netstandard2.0 | 1,690,112 | 1,602,560 | 87,552 | 5.2% | 691,832 | 653,455 | 38,377 (5.5%) |

**Read the percentage with this: the denominator holds T-SQL twice.** The T-SQL parser declares a second rendering with locations under a suffix (`Located`), a whole second compilation of the grammar (about 441 KB of source) with its own recognizers and materializers, and it carries no grammar text; the choice is the device's, but "the text is N% of the assembly" without it reads wrongly.

So it is **a few percent of the assembly, about 5%, on disk and in the package**, not half.

Two things to read with it:
- **Sql: the difference is 924 KB, and the three texts sum to 697,784 bytes** (the architect's figures: 441,215 + 223,698 + 32,871). The difference is 226 KB more than the texts. I have not found where the extra 226 KB goes (a text carried by a class I did not count, or once more than I assumed); it is about the size of the SQL:2023 grammar, 223,698, and that is a guess.
- **Expression language: what disappears is the second copy.** The grammar is also the argument of `[Gram("""...""")]` on the class, and that stays whatever `Portable` says; `Portable = false` removes only the carried copy, 87,552 bytes, which is the size of one copy of the grammar. The assembly with the option off still holds the text once.

## The attributes counted, not subtracted (the architect's second order)

`attribute-count.txt`: the `GramSource` attributes read from the metadata of `DotGram.Sql.dll` and `DotGram.ExpressionLanguage.dll` (Mono.Cecil, the build of 39d7d356 in `side/n1-1`; its grammar texts are those of the build weighed above), with the length of each argument. **There are exactly three in DotGram.Sql, of the lengths the emitter's reading predicts**: `TransactSqlParser` 474,107 bytes (its own grammar glued to the included 1992 one), `Sql92Parser` 32,871, `SqlStandardParser` 223,695; **730,673 bytes in all**, no fourth and none twice. So the emitter is right, and the difference of the two builds, 924,160, holds **193,487 bytes that are not in the attributes**: they are in the measurement, and it cannot say what they are (a file's difference is everything that moved: alignment of sections, the order of the metadata, whatever else the option changes in what is emitted). The expression language has one `GramSource` (67,567 bytes) and the same text once more as the argument of `[Gram]`, two copies as stated; the difference of the two builds was 87,552, again 19,985 more than one copy. The sizes are multiples of 512 because a PE file is aligned so.

What is answered: the text on the classes is 730,673 bytes of a 19,811,840-byte assembly, **3.7%**, and 67,567 of 1,651,200, **4.1%**, by counting; the difference of builds says 4.7% and 5.3%. What is not: where the rest of the difference goes.
