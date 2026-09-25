"""Per-rule entry counts for the generated SQL:2023 reader, and nothing else.

Descended from sql-39's instrument (benchmarks/results/sql-standard-gap-2026-09-21/instrument.py)
with the tape, roll-back and legality machinery taken out: the question here is only how many times
each rule is entered, on nested brackets, closed and unclosed.

A SNAPSHOT: it rewrites the .g.cs as it is now. Change the grammar and this must be run again, or
the count is yesterday's parser and the change will look like it did nothing.
"""
import re, os, glob, sys

HERE = os.path.dirname(os.path.abspath(__file__))
G = 'P:/dotgram.WorkTrees/sql/src/DotGram.Sql/obj/GeneratedFiles/DotGram/DotGram.Generation.GramGenerator'

NL  = chr(10)
TAB = chr(9)

sig = re.compile(r'^\s*public int (Read_\w+)\(int (\w+)')

names = []
os.makedirs(HERE + '/gen', exist_ok=True)
for old in glob.glob(HERE + '/gen/*.cs'):
    os.remove(old)

for path in sorted(glob.glob(G + '/*.cs')):
    base = os.path.basename(path)
    if 'SqlStandardParser' not in base and 'Attributes' not in base:
        continue
    if base.endswith('DotGramReport.g.cs'):
        continue

    out, pending = [], None

    for ln in open(path, encoding='utf-8-sig').read().split(NL):
        if pending is not None and ln.strip() == '{':
            out.append(ln)
            out.append(TAB * 4 + 'global::RecProbe.C.ByRule[%d]++;' % pending)
            pending = None
            continue

        m = sig.match(ln)
        if m:
            names.append(m.group(1))
            pending = len(names) - 1

        out.append(ln)

    open(HERE + '/gen/' + base, 'w', encoding='utf-8').write(NL.join(out))

# The names reach the probe as a generated table rather than a file it reads: the numbering is a
# property of the text this run instrumented, so the run that instrumented it writes it out.
open(HERE + '/gen/Names.g.cs', 'w', encoding='utf-8').write(
    'namespace RecProbe' + NL + '{' + NL +
    TAB + 'static class Names' + NL + TAB + '{' + NL +
    TAB * 2 + 'internal static readonly string[] All =' + NL + TAB * 2 + '{' + NL +
    ''.join(TAB * 3 + '"' + one + '",' + NL for one in names) +
    TAB * 2 + '};' + NL + TAB + '}' + NL + '}' + NL)

print('reader methods instrumented:', len(names))
