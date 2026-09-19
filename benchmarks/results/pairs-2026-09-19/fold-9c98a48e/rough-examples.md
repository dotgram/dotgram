# 9c98a48e against 9c2237da (sql-39's fold seam), the examples' entry points

`stand --rough-examples`, unpinned on cores 16-31, both builds in the same rounds (fifteen of about ten milliseconds, turn and turn about, two seconds of warm-up each); the minimum and the median of the rounds, in microseconds a call. Rough: +-5%, and more when the machine is busy (the medians of these runs are); read the ratio of the minima. The four runs of the session gave ArithmeticTree.Read 0.44, 0.45, 0.46, 0.46x, ClampedExample.Read 0.63, 0.67, 0.64x (three runs), Levels.Levelled 0.43, 0.39, 0.41, 0.39x, Levels.Levelled on 20 nested brackets 0.40, 0.39, 0.38, 0.45x; the examples the commit does not touch 0.94-1.11x on the minima. One more run:

| entry point | before us (min, median) | after us (min, median) | after / before, min | after / before, median |
| --- | ---: | ---: | ---: | ---: |
| MetricsLine.Read | 0.687, 0.907 | 0.687, 0.961 | 1.000 | 1.060 |
| MetricsLine.Read, quoted | 0.464, 0.659 | 0.500, 0.733 | 1.078 | 1.112 |
| FilterFile.ParseFilter | 0.121, 0.173 | 0.123, 0.160 | 1.022 | 0.924 |
| Filters.ParseFilter | 0.322, 0.479 | 0.316, 0.500 | 0.983 | 1.043 |
| Lexemes.ParseQuoted | 0.038, 0.045 | 0.037, 0.047 | 0.990 | 1.046 |
| Lexemes.ParseQuoted, long | 0.284, 0.561 | 0.297, 0.560 | 1.045 | 0.999 |
| SettingsFile.ParseSettings | 0.131, 0.163 | 0.123, 0.151 | 0.942 | 0.932 |
| SettingsFile.ParseSettings, 60 lines | 1.933, 2.830 | 1.979, 2.317 | 1.023 | 0.819 |
| Calculator.EvaluateInt | 0.973, 1.153 | 0.969, 1.104 | 0.996 | 0.958 |
| Calculator.BuildTree | 1.210, 1.641 | 1.182, 1.640 | 0.977 | 0.999 |
| ArithmeticTree.Read | 1.447, 1.917 | 0.565, 1.002 | 0.390 | 0.523 |
| ClampedExample.Read | 0.542, 0.594 | 0.353, 0.370 | 0.651 | 0.624 |
| Levels.Levelled | 0.468, 0.639 | 0.199, 0.263 | 0.426 | 0.413 |
| Levels.Levelled, deep | 2.774, 3.596 | 1.119, 1.432 | 0.403 | 0.398 |
