# bb603091 against dba87a9e (expr's per-arm value tables), the examples' entry points

`stand --rough-examples`, unpinned on cores 16-31, both builds in the same rounds; ratios of the minima quoted, +-5%. Three runs:

| entry point | before us (min, median) | after us (min, median) | after / before, min | after / before, median |
| --- | ---: | ---: | ---: | ---: |
| MetricsLine.Read | 0.857, 1.027 | 0.926, 0.985 | 1.079 | 0.959 |
| MetricsLine.Read, quoted | 0.438, 0.604 | 0.437, 0.552 | 0.997 | 0.915 |
| FilterFile.ParseFilter | 0.121, 0.151 | 0.121, 0.150 | 1.003 | 0.991 |
| Filters.ParseFilter | 0.328, 0.503 | 0.291, 0.498 | 0.889 | 0.989 |
| Lexemes.ParseQuoted | 0.037, 0.038 | 0.037, 0.038 | 1.005 | 1.020 |
| Lexemes.ParseQuoted, long | 0.275, 0.283 | 0.274, 0.286 | 0.995 | 1.008 |
| SettingsFile.ParseSettings | 0.146, 0.195 | 0.126, 0.194 | 0.863 | 0.993 |
| SettingsFile.ParseSettings, 60 lines | 1.981, 2.614 | 1.848, 2.298 | 0.933 | 0.879 |
| Calculator.EvaluateInt | 0.920, 0.996 | 0.938, 1.008 | 1.020 | 1.011 |
| Calculator.BuildTree | 1.175, 1.212 | 1.153, 1.218 | 0.981 | 1.005 |
| ArithmeticTree.Read | 0.536, 0.655 | 0.545, 0.638 | 1.017 | 0.973 |
| ClampedExample.Read | 0.359, 0.375 | 0.355, 0.370 | 0.988 | 0.987 |
| JsonParser.Read | 1.520, 1.558 | 1.544, 1.581 | 1.015 | 1.015 |
| GramGrammar.ParseFile | 2.554, 3.000 | 2.714, 3.301 | 1.063 | 1.100 |

| entry point | before us (min, median) | after us (min, median) | after / before, min | after / before, median |
| --- | ---: | ---: | ---: | ---: |
| MetricsLine.Read | 0.612, 0.856 | 0.646, 0.993 | 1.055 | 1.159 |
| MetricsLine.Read, quoted | 0.446, 0.565 | 0.440, 0.548 | 0.986 | 0.970 |
| FilterFile.ParseFilter | 0.121, 0.130 | 0.122, 0.140 | 1.010 | 1.078 |
| Filters.ParseFilter | 0.333, 0.564 | 0.377, 0.512 | 1.131 | 0.908 |
| Lexemes.ParseQuoted | 0.038, 0.062 | 0.038, 0.065 | 1.006 | 1.047 |
| Lexemes.ParseQuoted, long | 0.285, 0.475 | 0.359, 0.460 | 1.260 | 0.968 |
| SettingsFile.ParseSettings | 0.121, 0.169 | 0.122, 0.166 | 1.016 | 0.986 |
| SettingsFile.ParseSettings, 60 lines | 1.996, 2.810 | 1.868, 2.415 | 0.936 | 0.859 |
| Calculator.EvaluateInt | 0.982, 1.107 | 0.980, 1.129 | 0.998 | 1.020 |
| Calculator.BuildTree | 1.177, 1.284 | 1.230, 1.284 | 1.045 | 1.000 |
| ArithmeticTree.Read | 0.549, 0.572 | 0.559, 0.574 | 1.018 | 1.003 |
| ClampedExample.Read | 0.332, 0.397 | 0.352, 0.427 | 1.060 | 1.077 |
| JsonParser.Read | 1.492, 1.619 | 1.520, 1.830 | 1.019 | 1.130 |
| GramGrammar.ParseFile | 2.356, 2.492 | 2.313, 2.396 | 0.982 | 0.961 |

| entry point | before us (min, median) | after us (min, median) | after / before, min | after / before, median |
| --- | ---: | ---: | ---: | ---: |
| MetricsLine.Read | 0.609, 0.618 | 0.606, 0.636 | 0.996 | 1.029 |
| MetricsLine.Read, quoted | 0.443, 0.460 | 0.440, 0.457 | 0.994 | 0.992 |
| FilterFile.ParseFilter | 0.121, 0.126 | 0.120, 0.128 | 0.994 | 1.015 |
| Filters.ParseFilter | 0.301, 0.318 | 0.317, 0.339 | 1.054 | 1.066 |
| Lexemes.ParseQuoted | 0.037, 0.040 | 0.037, 0.039 | 1.007 | 0.983 |
| Lexemes.ParseQuoted, long | 0.278, 0.298 | 0.278, 0.287 | 0.999 | 0.965 |
| SettingsFile.ParseSettings | 0.120, 0.125 | 0.121, 0.123 | 1.014 | 0.988 |
| SettingsFile.ParseSettings, 60 lines | 1.860, 1.966 | 1.834, 1.930 | 0.986 | 0.981 |
| Calculator.EvaluateInt | 0.951, 0.971 | 0.937, 0.976 | 0.986 | 1.005 |
| Calculator.BuildTree | 1.167, 1.207 | 1.156, 1.235 | 0.991 | 1.023 |
| ArithmeticTree.Read | 0.730, 0.938 | 0.604, 0.946 | 0.828 | 1.009 |
| ClampedExample.Read | 0.355, 0.438 | 0.351, 0.368 | 0.989 | 0.841 |
| JsonParser.Read | 1.553, 2.030 | 1.511, 1.975 | 0.973 | 0.973 |
| GramGrammar.ParseFile | 2.340, 3.049 | 2.383, 2.540 | 1.018 | 0.833 |
