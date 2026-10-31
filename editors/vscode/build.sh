#!/usr/bin/env bash
# Builds the .Gram extension: the language server published into server/, the client bundled, and the
# whole packaged as a .vsix in artifacts/ at the repository root. Never publishes.
#
#   editors/vscode/build.sh [--self-contained <rid>]
#
# --self-contained <rid> publishes the server for one platform with its own runtime, and packages a
# platform-specific .vsix (vsce package --target); without it the server is framework-dependent and
# the .vsix runs anywhere .NET 10 does (docs/vscode.md).
set -euo pipefail

here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
root="$(cd "$here/../.." && pwd)"
rid=""

while [[ $# -gt 0 ]]; do
	case "$1" in
		--self-contained) rid="$2"; shift 2 ;;
		*) echo "Unknown argument: $1" >&2; exit 2 ;;
	esac
done

rm -rf "$here/server"

publish=(dotnet publish "$root/src/DotGram.LanguageServer/DotGram.LanguageServer.csproj" -c Release -o "$here/server" -nodeReuse:false -warnaserror)
target=()

if [[ -n "$rid" ]]; then
	publish+=(--runtime "$rid" --self-contained true)

	case "$rid" in
		win-x64) target=(--target win32-x64) ;;
		win-arm64) target=(--target win32-arm64) ;;
		linux-x64) target=(--target linux-x64) ;;
		linux-arm64) target=(--target linux-arm64) ;;
		osx-x64) target=(--target darwin-x64) ;;
		osx-arm64) target=(--target darwin-arm64) ;;
		*) echo "No VS Code target for runtime $rid" >&2; exit 2 ;;
	esac
else
	publish+=(--no-self-contained -p:UseAppHost=false)
fi

"${publish[@]}"

cd "$here"
npm ci
npm run build
mkdir -p "$root/artifacts"
npx vsce package "${target[@]}" --out "$root/artifacts/"
