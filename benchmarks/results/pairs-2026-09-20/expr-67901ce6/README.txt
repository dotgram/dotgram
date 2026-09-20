Window 82, 67901ce6 against 73f4b49a, with the A/A of the parent beside each row.
sql/select20 leaned +3.2% (5 of 5 runs) above an A/A of -0.1%. expr read the generated file afterwards: select20 calls TryParseQueryExpression(string), which tokenizes with
Tokenize_DotGram and never reaches Tokenized_DotGram, the only body the commit changed (two calls to Recycle_DotGram removed). So the lean is not the price of the change: there is
nothing on that row for the change to cost, and it is layout or the disturbance of the window (the base of these rows varied 69-86% between runs). Reachability (D45) was not asked before the pair.
