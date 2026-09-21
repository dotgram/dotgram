"""The two orders of one pair read together, generic: the change of the SECOND build (`after` in the normal order, `before` in the swapped one) over the FIRST, in each run, whichever slot it was in, with the A/A of the first build in each order beside it.

Usage: python order-free-generic.py <directory holding normal/ and swapped/ each holding pair/run-N/paired.json and pair/aa-run-N/paired.json> [runs]
'New' means the build that is `after` in the normal order. The rows named .bool compare two APIs and are left out.
"""
import json
import statistics
import sys

root = sys.argv[1]
runs = int(sys.argv[2]) if len(sys.argv) > 2 else 10


def load(order, prefix):
    found = {}

    for run in range(1, runs + 1):
        try:
            document = json.load(open(f'{root}/{order}/{prefix}-{run}/paired.json'))
        except FileNotFoundError:
            continue

        for row in document['Rows']:
            found.setdefault(row['Id'], []).append({one['Reading']: one['Nanoseconds'] for one in row['Readings']})

    return found


normal, swapped = load('normal', 'run'), load('swapped', 'run')
aa_normal, aa_swapped = load('normal', 'aa-run'), load('swapped', 'aa-run')


def change(readings, key_new, key_old):
    return [(one[key_new] / one[key_old] - 1) * 100 for one in readings if key_new in one and key_old in one]


print('row | new over old, median normal | median swapped | median of all | positive | A/A of the old build: normal | swapped | run range normal | swapped')

for row in sorted(normal):
    if row.endswith('.bool') or row not in swapped:
        continue

    a = change(normal[row], 'after', 'before')
    b = change(swapped[row], 'before', 'after')

    if not a or not b:
        continue

    both = a + b
    aa_a = change(aa_normal.get(row, []), 'after', 'before')
    aa_b = change(aa_swapped.get(row, []), 'after', 'before')

    def median(values):
        return f'{statistics.median(values):+.1f}%' if values else 'n/a'

    def span(values):
        return f'[{min(values):+.1f}..{max(values):+.1f}]' if values else 'n/a'

    print(f'{row} | {median(a)} | {median(b)} | {median(both)} | {sum(v > 0 for v in both)} of {len(both)} | {median(aa_a)} | {median(aa_b)} | {span(a)} | {span(b)}')
