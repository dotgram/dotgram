# 94c10b3f against 7937b564, the five examples it changes

`stand --rough-examples`, unpinned on cores 16-31, both builds read in the same rounds (fifteen of about ten milliseconds, turn and turn about, two seconds of warm-up each); the minimum and the median of the rounds, in microseconds a call. Rough: +-5%. A second run of the same:

| entry point | before us (min, median) | after us (min, median) | after / before, min | after / before, median |
| --- | ---: | ---: | ---: | ---: |
| MetricsLine.Read | 0.603, 0.658 | 0.592, 0.660 | 0.980 | 1.004 |
| MetricsLine.Read, quoted | 0.425, 0.458 | 0.419, 0.487 | 0.985 | 1.065 |
| FilterFile.ParseFilter | 0.212, 0.219 | 0.121, 0.128 | 0.573 | 0.582 |
| Filters.ParseFilter | 0.331, 0.340 | 0.300, 0.304 | 0.909 | 0.896 |
| Lexemes.ParseQuoted | 0.052, 0.055 | 0.036, 0.037 | 0.692 | 0.666 |
| Lexemes.ParseQuoted, long | 0.512, 0.516 | 0.266, 0.277 | 0.520 | 0.537 |
| SettingsFile.ParseSettings | 0.169, 0.173 | 0.124, 0.125 | 0.732 | 0.727 |
| SettingsFile.ParseSettings, 60 lines | 2.580, 4.131 | 1.832, 3.062 | 0.710 | 0.741 |
