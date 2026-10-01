#!/usr/bin/env bash
# The CI job's own pragma inventory: every `#pragma warning disable` the generator writes
# into emitted code, checked in with the reason beside it
# (tools/generated-analysis/pragma-inventory.txt) -- a NEW one, this script's whole job, fails
# the job rather than passing unnoticed. Not about #line-mapped regions (the grammar author's
# own pragmas, if any, are theirs to account for); only the emitter's own.
#
# Usage:
#   pragma-inventory.sh scan   <emit-scratch-dir>              # print the current inventory
#   pragma-inventory.sh check  <emit-scratch-dir> <baseline>    # fail on anything new
set -euo pipefail

HERE=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)

scan() {
	local scratch=$1
	# One line per (package, file, pragma code, the comment on the same line, if any),
	# normalized so a run on a different machine or checkout still matches the baseline.
	for pkg_dir in "$scratch"/*/gen/DotGram/DotGram.Generation.GramGenerator; do
		[ -d "$pkg_dir" ] || continue
		local pkg
		pkg=$(basename "$(dirname "$(dirname "$(dirname "$pkg_dir")")")")
		for f in "$pkg_dir"/*.g.cs; do
			[ -f "$f" ] || continue
			local name
			name=$(basename "$f")
			# "|| true": grep exits 1 on a file with no pragma at all, which is the ordinary
			# case for most generated files, and under "set -e -o pipefail" that nonzero
			# status would abort this whole for-loop (and the scan) at the FIRST such file --
			# silently, since the caller only sees a short write and a nonzero exit, not an
			# error message. Confirmed empirically: an unguarded run stopped after exactly one
			# file, DotGram.Attributes.g.cs, alphabetically first and pragma-free.
			grep -n '#pragma warning disable' "$f" 2>/dev/null | while IFS=: read -r line rest; do
				printf '%s\t%s\t%s\n' "$pkg" "$name" "$(echo "$rest" | sed -E 's/^[[:space:]]*#pragma warning disable[[:space:]]*//')"
			done || true
		done
	done | sort -u
}

case "${1:-}" in
	scan)
		scan "$2"
		;;
	check)
		current=$(mktemp)
		diffout=$(mktemp)
		trap 'rm -f "$current" "$diffout"' EXIT
		scan "$2" > "$current"
		if ! diff -u "$3" "$current" > "$diffout"; then
			echo "pragma-inventory: emitted code carries a pragma not in $3 (or the baseline carries one no longer emitted):" >&2
			cat "$diffout" >&2
			exit 1
		fi
		echo "pragma-inventory: matches $3 ($(wc -l < "$current") entries)"
		;;
	*)
		echo "usage: pragma-inventory.sh scan <emit-scratch-dir> | check <emit-scratch-dir> <baseline>" >&2
		exit 2
		;;
esac
