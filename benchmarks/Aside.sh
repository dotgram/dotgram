#!/usr/bin/env bash
# Aside.sh <command> [arguments]: runs a build, a test run or anything else heavy beside the stand (D147).
#
#     benchmarks/Aside.sh dotnet build DotGram.slnx -c Linux
#
# It holds timing-builds.lock SHARED while the command runs, so that an exclusive window that wants to open waits
# for the command to end. The locks are the ones benchmarks/WindowLib.ps1 takes (flock on files in /ramdisk/locks,
# or DOTGRAM_WINDOW_DIR). What it does depends on the window (`kind` in timing-window.txt):
#   no window open         the command runs at once on the whole machine;
#   a shared window        the command runs at once, pinned to the processors the stand does not use (the other
#                          CCD: the complement of DOTGRAM_STAND_CPUS, or DOTGRAM_ASIDE_CPUS);
#   an exclusive window    it waits for the window to close (a window that names no kind is exclusive).
# Inside a window's own process tree (DOTGRAM_WINDOW_HOLDER names the holder) it runs the command at once.
set -euo pipefail

if [ $# -eq 0 ]; then echo "usage: benchmarks/Aside.sh <command> [arguments]" >&2; exit 2; fi

if [ -n "${DOTGRAM_WINDOW_DIR:-}" ]; then dir=$DOTGRAM_WINDOW_DIR
elif [ -d /ramdisk ]; then dir=/ramdisk/locks
else dir=${TMPDIR:-/tmp}
fi

mkdir -p "$dir"
gate=$dir/timing-window.lock
builds=$dir/timing-builds.lock
text=$dir/timing-window.txt

# Inside an Aside already: the outer one holds the builds lock. Waiting at the gate here would stall a window that
# has taken the gate and is waiting for the outer one to end.
if [ -n "${DOTGRAM_ASIDE:-}" ]; then
	exec "$@"
fi

if [ -n "${DOTGRAM_WINDOW_HOLDER:-}" ] && [ "$(head -n 1 "$text" 2>/dev/null)" = "pid $DOTGRAM_WINDOW_HOLDER" ]; then
	exec "$@"
fi

# The processors the stand does not use: DOTGRAM_ASIDE_CPUS, or every online processor outside DOTGRAM_STAND_CPUS
# (0-7,16-23 by default, the first CCD with its SMT siblings).
aside_cpus() {
	if [ -n "${DOTGRAM_ASIDE_CPUS:-}" ]; then echo "$DOTGRAM_ASIDE_CPUS"; return; fi
	local part a b stand=" " list=
	for part in $(echo "${DOTGRAM_STAND_CPUS:-0-7,16-23}" | tr ',' ' '); do
		if [[ $part == *-* ]]; then a=${part%-*}; b=${part#*-}; stand+="$(seq -s ' ' "$a" "$b") "; else stand+="$part "; fi
	done
	for part in $(tr ',' ' ' < /sys/devices/system/cpu/online); do
		if [[ $part == *-* ]]; then a=${part%-*}; b=${part#*-}; else a=$part; b=$part; fi
		for ((cpu = a; cpu <= b; cpu++)); do [[ $stand == *" $cpu "* ]] || list+="$cpu "; done
	done
	# Written as ranges ("8-15,24-31"), as taskset and a reader take it.
	echo "$list" | awk '{ first = last = $1; for (i = 2; i <= NF; i++) { if ($i != last + 1) { out = out range(first, last) ","; first = $i } last = $i } print out range(first, last) }
		function range(a, b) { return a == b ? a : a "-" b }'
}

# The gate is probed, not held: an exclusive window that takes the gate and then the builds lock in between makes
# the builds lock below wait for it. A window that has the gate but has not yet written its kind reads as
# exclusive for a moment, and the next probe sees it.
pin=
said=
until flock -n -s "$gate" true; do
	if [ "$(sed -n 's/^kind //p' "$text" 2>/dev/null | head -n 1)" = shared ]; then
		pin=$(aside_cpus)
		break
	fi

	if [ -z "$said" ]; then
		echo "Aside: an exclusive timing window is open, waiting for it to close: $(tr '\n' ';' < "$text" 2>/dev/null)" >&2
		said=1
	fi

	sleep 2
done

# -o: the lock is not handed to the command, so a compiler server or build node the command leaves running does
# not go on holding it.
# DOTGRAM_ASIDE tells what the command starts that it is inside an Aside: a nested Aside runs at once, and a timing
# refuses, since it would be waiting for itself.
export DOTGRAM_ASIDE=1

if [ -n "$pin" ]; then
	echo "Aside: a shared timing window is open ($(sed -n 's/^what //p' "$text" 2>/dev/null)): running on processors $pin" >&2
	exec flock -o -s "$builds" taskset -c "$pin" "$@"
fi

exec flock -o -s "$builds" "$@"
