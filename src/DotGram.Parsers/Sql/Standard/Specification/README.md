# ISO/IEC 9075-2:2023, SQL/Foundation — the BNF

`ISO_IEC_9075-2(E)_Foundation.bnf.txt` is the grammar of Part 2 of the SQL standard,
edition 6 (2023), as ISO publishes it for implementors:

<https://standards.iso.org/iso-iec/9075/-2/ed-6/en/ISO_IEC_9075-2(E)_Foundation.bnf.txt>

Fetched on 2026-09-10 and kept as it came — 1,758 productions, the file's own spacing
included — so that a rule written in a grammar here can be held against the text it was
written from. Its header says what it is for:

> This grammar may be used by implementors of SQL-implementations when generating parsers
> for the SQL language.

The grammar of the standard in this directory is written from it and cites it by rule
name; the tree the SQL parsers build is named and shaped after it
(`docs/design/sql-parsers.md`). The syntax rules alone are not the standard: what each
production means, and which are optional features, is in the standard's text, which ISO
sells and this repository does not have.
