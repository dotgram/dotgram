# Spike: rule frames a no-collapse reference shows on the why stacks and the trace build does not.
import sys, collections

def load(path):
    rows = {}
    with open(path, encoding='utf-8') as f:
        next(f)
        for line in f:
            g, i, pos, exp, cand, kept, atm, mis, stacks = line.rstrip('\n').split('\t')
            rows[(g, int(i))] = dict(pos=pos, exp=exp, cand=int(cand), kept=int(kept), atm=int(atm), mis=int(mis),
                                    stacks=[s.split('>') for s in stacks.split('|')] if stacks else [])
    return rows

traced, reference = load(sys.argv[1]), load(sys.argv[2])
per = collections.defaultdict(lambda: collections.Counter())
missing_names = collections.defaultdict(collections.Counter)
for key, r in reference.items():
    g = key[0]
    c = per[g]
    c['inputs'] += 1
    t = traced.get(key)
    if t is None:
        c['absent'] += 1
        continue
    if (t['pos'], t['exp']) != (r['pos'], r['exp']):
        c['answer differs'] += 1
    if t['kept'] > 0: c['explained (T)'] += 1
    if r['kept'] > 0: c['explained (R)'] += 1
    if t['kept'] > 0 and t['atm'] > 0: c['kept at Match.Position (T)'] += 1
    if t['mis'] or r['mis']: c['unbalanced exits'] += 1
    names = {n.rstrip('~') for s in t['stacks'] for n in s}
    for s in r['stacks']:
        for n in s:
            if n.endswith('~'):
                c['trivial frames (excluded)'] += 1
                continue
            c['frames'] += 1
            if n not in names:
                c['missing'] += 1
                missing_names[g][n] += 1
for g, c in per.items():
    share = 100.0 * c['missing'] / c['frames'] if c['frames'] else 0.0
    print(f"{g}: " + ", ".join(f"{k}={v}" for k, v in c.items()) + f", missing share={share:.1f}%")
    if missing_names[g]:
        print("   most missing: " + ", ".join(f"{n}({k})" for n, k in missing_names[g].most_common(8)))
