#!/usr/bin/env bash
# Aside.sh <command> [arguments]: runs a build, a test run or anything else heavy beside the stand (D147).
#
#     benchmarks/Aside.sh dotnet build DotGram.slnx -c Linux
#
# It waits while a timing window is open, and holds timing-builds.lock SHARED while the command runs, so that a
# window that wants to open waits for the command to end. The locks are the ones benchmarks/WindowLib.ps1 takes
# (flock on files in /ramdisk/locks, or DOTGRAM_WINDOW_DIR). Inside a window's own process tree
# (DOTGRAM_WINDOW_HOLDER names the holder) it runs the command at once. Nothing is pinned: while no window is
# open a build may use the whole machine, and while one is open it does not run.
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

if [ -n "${DOTGRAM_WINDOW_HOLDER:-}" ] && [ "$(head -n 1 "$text" 2>/dev/null)" = "pid $DOTGRAM_WINDOW_HOLDER" ]; then
	exec "$@"
fi

if ! flock -n -s "$gate" true; then
	echo "Aside: a timing window is open, waiting for it to close: $(tr '\n' ';' < "$text" 2>/dev/null)" >&2
fi

# The gate is passed and let go; the builds lock is what the command holds. A window that took both in between
# makes this wait for it. -o: the lock is not handed to the command, so a compiler server or build node the
# command leaves running does not go on holding it.
# DOTGRAM_ASIDE tells a timing started from inside the command that it would be waiting for itself.
flock -s "$gate" true
export DOTGRAM_ASIDE=$$
exec flock -o -s "$builds" "$@"
