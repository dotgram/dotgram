"""sql-39's instrument (scratchpad/recprobe/instrument.py), adapted to the Web package.

One numbering across every generated parser, and none of the SQL-specific hooks. Beyond the
per-rule entry counts it answers D63's correctness question: a turn that fails has its log
rolled back, and the next attempt writes into the same place - are the entries the same?
"""
import re, os, glob

S = 'T:/TEMP/claude/P--dotgram-WorkTrees-performance/895062dd-b50a-45f4-8dcb-9f0659793f17/scratchpad/sqlcount'
G = 'P:/dotgram.WorkTrees/performance/.work/wt-main/src/DotGram.Sql/obj/GeneratedFiles/DotGram/DotGram.Generation.GramGenerator'

NL = chr(10)
TAB = chr(9)

names = []
# The third axis (D63): a memo of refusals is sound only where a rule's verdict is a
# function of (rule, position). A reader that asks the tape whether to replay a recorded
# way takes its alternative FROM the tape, so the same rule at the same position can go a
# different way and refuse in one visit and pass in another. Two flags, because the strict
# one is a lower bound on what is legal and the loose one is where the unsoundness is:
#   touches - mentions the tape at all (writing included)
#   reads   - reads it where a decision turns on it
touches, asks = [], []
READS = ('ways.Cursor', 'ways.Count', 'ways.Retry(', 'ways.Items')
skipped = [0]
sig = re.compile(r'^\s*public int (Read_\w+)\(int (\w+)')
enter = re.compile(r'global::RecProbe\.C\.Enter\((\d+), (\w+)\);')

# The restore a failing turn performs. `ways.Log` is evaluated at the call site, which is
# inside the parser class, so the probe only ever sees an int[] and two offsets.
rollback = re.compile(r'^(\s*)ways\.LogCount\s*=\s*(lm\w*);')

os.makedirs(S + '/gen', exist_ok=True)
for old in glob.glob(S + '/gen/*.cs'):
    os.remove(old)

# The tape's writes all go through Begin and Put, and LogCount advances nowhere else, so the
# furthest each call is about to reach is the furthest the counter ever holds: an exact peak
# rather than a sample. The hook sits before the resize, where LogCount is still the old value.
HOOKS = [
    ('internal void Begin(int arm)' + NL, 'global::RecProbe.C.Begins++; global::RecProbe.C.Wrote(2, LogCount + 2);'),
    ('internal void Begin(int arm, int start, int end)', 'global::RecProbe.C.Begins++; global::RecProbe.C.Wrote(4, LogCount + 4);'),
    ('internal void Put(int value)', 'global::RecProbe.C.Wrote(1, LogCount + 1);'),
    ('internal void Put(int a, int b)', 'global::RecProbe.C.Wrote(2, LogCount + 2);'),
    ('internal int Open(int at, int last)', 'global::RecProbe.C.Opens++;'),
    ('internal bool Retry(int segment)', 'global::RecProbe.C.Retries++;'),
]

for path in sorted(glob.glob(G + '/*.cs')):
    if 'SqlStandardParser' not in os.path.basename(path) and 'Attributes' not in os.path.basename(path):
        continue
    base = os.path.basename(path)
    if base.endswith('DotGramReport.g.cs'):
        continue

    src = open(path, encoding='utf-8-sig').read()
    out, pending, reader = [], None, False

    for ln in src.split(NL):
        if pending is not None and ln.strip() == '{':
            out.append(ln)
            out.append(TAB * 4 + 'global::RecProbe.C.Enter(%d, %s);' % pending)
            pending = None
            continue
        if re.match(r'^' + TAB * 3 + r'(public|internal|private|static) ', ln):
            reader = False
        if ln.strip() == 'var p = pos;':
            reader = True

        if names and 'ways.' in ln:
            touches[-1] = True
            if any(one in ln for one in READS):
                asks[-1] = True

        m = sig.match(ln)
        if m:
            names.append(base.replace('DotGram.Sql.', '').replace('.g.cs', '') + '  ' + m.group(1))
            touches.append(False)
            asks.append(False)
            pending = (len(names) - 1, m.group(2))

        # Before the log is rolled back, hand the probe what is about to be discarded - but
        # only inside a reader method, where `p` is the moving position and so distinguishes
        # one turn from the next. The way-back wrappers roll back too and have only `pos`,
        # their own entry, which would conflate every turn of a call; those are counted as
        # skipped rather than keyed wrongly.
        r = rollback.match(ln)
        if r:
            # Inside a reader the moving local distinguishes one turn from the next; inside a
            # way-back wrapper, which is where a give-back actually happens, there is only the
            # entry position - and that is the right key there, because the wrapper is entered
            # again at the same place each time the way is retried.
            out.append('%sglobal::RecProbe.C.Rolled(ways.Log, %s, ways.LogCount, %s);'
                       % (r.group(1), r.group(2), 'p' if reader else 'pos'))
            if not reader:
                skipped[0] += 1
        out.append(ln)

    # A `return -1` inside a reader method is a refusal, counted against that method.
    done, cur = [], None
    for ln in out:
        m = enter.search(ln)
        if m:
            cur = (m.group(1), m.group(2))
        if cur and 'return -1;' in ln:
            ln = ln.replace('return -1;', 'return global::RecProbe.C.Fail(%s, %s);' % cur)
        if re.match(r'^' + TAB * 3 + r'(public|internal|private|static|///)', ln) \
                and not ln.lstrip().startswith('public int Read_'):
            cur = None
        done.append(ln)

    whole = NL.join(done)
    for marker, statement in HOOKS:
        at = whole.find(marker)
        if at >= 0:
            brace = whole.index('{', at)
            whole = whole[:brace + 1] + NL + TAB * 4 + statement + whole[brace + 1:]

    open(S + '/gen/' + base, 'w', encoding='utf-8').write(whole)

open(S + '/names.txt', 'w', encoding='utf-8').write(NL.join(names))

# The flags reach the probe as a generated table rather than a file it parses: the
# classification is a property of the text this run instrumented, so it is written by the
# run that instrumented it.
open(S + '/gen/Legality.g.cs', 'w', encoding='utf-8').write(
    'namespace RecProbe' + NL + '{' + NL +
    TAB + 'static class Legality' + NL + TAB + '{' + NL +
    TAB * 2 + 'internal static readonly bool[] Touches = { ' + ', '.join('true' if one else 'false' for one in touches) + ' };' + NL +
    TAB * 2 + 'internal static readonly bool[] Asks = { ' + ', '.join('true' if one else 'false' for one in asks) + ' };' + NL +
    TAB + '}' + NL + '}' + NL)
print('readers that touch the tape:', sum(touches), 'of', len(touches))
print('readers whose decision reads it:', sum(asks))
print('reader methods instrumented:', len(names))
print('roll-backs in a way-back wrapper, keyed by its entry:', skipped[0])
