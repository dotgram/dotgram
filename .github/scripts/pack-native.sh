#!/usr/bin/env bash
# The native executable of DotGram.Cli for one runtime identifier, published already, put into the
# archive a release attaches: dotgram-<version>-<rid>.zip on Windows, .tar.gz elsewhere (which keeps
# the executable bit), with the licences beside it. The archive goes into artifacts/.
#
#   .github/scripts/pack-native.sh <rid>
set -euo pipefail

rid="$1"
root="$(cd "$(dirname "$0")/../.." && pwd)"
published="$root/.build/native/publish/DotGram.Cli/release_$rid"
version=$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$root/Directory.Build.props" | head -1)

case "$rid" in
  win-*) executable=dotgram.exe ;;
  *)     executable=dotgram ;;
esac

test -f "$published/$executable" || { echo "no $published/$executable: publish it first"; exit 1; }

staging="$(mktemp -d)"
trap 'rm -rf "$staging"' EXIT

mkdir -p "$staging/licenses" "$root/artifacts"
cp "$published/$executable" "$staging/"
cp "$root/LICENSE" "$staging/"
cp "$root/licenses/LICENSE-GENERATED-CODE" "$staging/licenses/"

name="dotgram-$version-$rid"

case "$rid" in
  win-*)
    archive="$root/artifacts/$name.zip"
    rm -f "$archive"
    # Git's bash on the Windows runner: PowerShell reads Windows paths.
    pwsh -NoProfile -Command "Compress-Archive -Path '$(cygpath -w "$staging")\\*' -DestinationPath '$(cygpath -w "$archive")'"
    ;;
  *)
    archive="$root/artifacts/$name.tar.gz"
    tar -czf "$archive" -C "$staging" "$executable" LICENSE licenses
    ;;
esac

echo "$archive"
