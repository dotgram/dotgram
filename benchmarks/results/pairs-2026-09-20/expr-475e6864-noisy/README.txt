Window 78: 475e6864 against fea46916, the split grammars rows. Taken beside builds and a node count on cores 16-31; the SQL:2023 rows disagreed between runs
(the base of sql/select20 9.4 to 13.6 us, 7.0 us when quiet), one run was dropped for its control, so read only the rows whose between-run spread is small
(tsql/script*, tsql/*1000). The SQL:2023 rows are taken again in a quiet window.
