"""The two orders of the 15e42dc3 pair read together: the change of 15e42dc3 (r8-1) over c9904b56 (r7-0) in each run, whichever slot it was in.

Normal order: 15e42dc3 is `after`, so its change is after/before - 1. Swapped: it is `before`, so its change is before/after - 1. The rows named `.bool` compare two APIs
(the Match form of `before` and the bool form of `after`), so their swapped reading is the price of the API and not of the commit; they are left out. Usage: python order-free.py <this directory>
"""
import json
import statistics
import sys

root = sys.argv[1] if len(sys.argv) > 1 else '.'


def load(order):
    found = {}

    for run in range(1, 11):
        for row in json.load(open(f'{root}/{order}/run-{run}/paired.json'))['Rows']:
            found.setdefault(row['Id'], []).append({one['Reading']: one['Nanoseconds'] for one in row['Readings']})

    return found


normal, swapped = load('normal'), load('swapped')

print('row | median normal | median swapped | median of twenty | positive of twenty')

for row in sorted(normal):
    if row.endswith('.bool'):
        continue

    a = [(one['after'] / one['before'] - 1) * 100 for one in normal[row] if 'before' in one and 'after' in one]
    b = [(one['before'] / one['after'] - 1) * 100 for one in swapped[row] if 'before' in one and 'after' in one]

    if a and b:
        t = a + b
        print(f'{row} | {statistics.median(a):+.1f}% | {statistics.median(b):+.1f}% | {statistics.median(t):+.1f}% | {sum(v > 0 for v in t)} of {len(t)}')
