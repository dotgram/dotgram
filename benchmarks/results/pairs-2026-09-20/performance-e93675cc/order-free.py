"""The two orders of one pair read together: the change of the new side over the old one in each run, whichever slot it was in.

In the normal order the new side (r4-1, e93675cc) is `after`, so its change is after/before - 1; in the swapped order it is `before`, so it is before/after - 1.
The second slot's own lean is in the first number of one order and the opposite of it in the other, so the median of all ten runs is the change with the slot taken out
(as far as five runs an order allow), and the medians of each order beside it say how much the slot moved it. Usage: python order-free.py <this directory>
"""
import json
import statistics
import sys

root = sys.argv[1] if len(sys.argv) > 1 else '.'
rows = ['sql/select20', 'sql/select20.at', 'sql/select20.window', 'sql/select20.bool', 'sql/refused-late', 'sql/refused-cliff-paren-2715', 'sql/refused-cliff-and-2715',
        'sql/refused-cliff-case-1391', 'sql/refused-cliff-joins-891', 'web/json.array10000', 'fix/Orders128.yield-string', 'fix/orders400.text', 'el/ladder', 'el/terms1000',
        'web/media-type.quoted', 'tsql/columns1000']


def load(order):
    found = {}

    for run in range(1, 6):
        for row in json.load(open(f'{root}/{order}/run-{run}/paired.json'))['Rows']:
            found.setdefault(row['Id'], []).append({one['Reading']: one['Nanoseconds'] for one in row['Readings']})

    return found


normal, swapped = load('normal'), load('swapped')

print('row | normal per run | swapped per run | median of ten | positive of ten | median normal | median swapped')

for row in rows:
    a = [(one['after'] / one['before'] - 1) * 100 for one in normal[row] if 'before' in one and 'after' in one]
    b = [(one['before'] / one['after'] - 1) * 100 for one in swapped[row] if 'before' in one and 'after' in one]

    if not a or not b:
        continue

    ten = a + b

    print(f'{row} | {[round(v, 1) for v in a]} | {[round(v, 1) for v in b]} | {statistics.median(ten):+.1f}% | {sum(v > 0 for v in ten)} of 10 | {statistics.median(a):+.1f}% | {statistics.median(b):+.1f}%')
