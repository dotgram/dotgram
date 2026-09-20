Held while the stream form is walked, median of 5 processes each (kilobytes above the live heap before the walk):

The figure of a small stream moves in steps of a few kilobytes, so the median of five can be a draw between two levels: each cell is the median, then the smallest and the largest of the runs, and the ratio is left blank where the two ranges overlap.

| input | fields | hand | before | after | after / before |
| --- | ---: | ---: | ---: | ---: | ---: |
| made-2000000 | 2,000,000 | 4 (4-4) | 6 (1-9) | 9 (6-9) |  |
| slope-1000000 | 1,000,000 | 4 (4-4) | 6 (6-6) | 6 (6-6) |  |
| slope-100000 | 100,000 | 4 (4-4) | 6 (6-6) | 6 (6-6) |  |
| slope-16 | 16 | 4 (4-4) | 6 (6-6) | 6 (6-6) |  |
| recover-1000000 | 1,000,000 | 4 (4-7) | 6 (6-9) | 6 (6-9) |  |
| recover-100000 | 100,000 | 4 (4-4) | 6 (6-9) | 9 (6-9) |  |
| made-2000000-reader | 2,000,000 | 12 (12-12) | 13 (13-16) | 13 (13-16) |  |
| slope-100000-reader | 100,000 | 12 (12-12) | 4 (4-7) | 7 (7-7) |  |
| recover-100000-reader | 100,000 | 12 (12-15) | 8 (8-8) | 8 (5-8) |  |
| made-2000000@1024 | 2,000,000 | 4 (4-4) | 6 (6-6) | 3 (3-6) |  |
| made-2000000-reader@1024 | 2,000,000 | 12 (12-12) | 7 (7-10) | 10 (10-10) |  |
| recover-100000@1024 | 100,000 | 4 (4-4) | 3 (3-3) | 3 (3-3) |  |
| recover-100000-reader@1024 | 100,000 | 12 (12-12) | 8 (8-8) | 8 (8-8) |  |
| made-2000000@4096 | 2,000,000 | 4 (4-4) | 9 (6-9) | 6 (6-9) |  |
| made-2000000-reader@4096 | 2,000,000 | 12 (12-12) | 13 (13-16) | 16 (13-16) |  |
| recover-100000@4096 | 100,000 | 4 (4-4) | 6 (6-6) | 6 (6-6) |  |
| recover-100000-reader@4096 | 100,000 | 12 (12-12) | 8 (8-8) | 8 (5-8) |  |
