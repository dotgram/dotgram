#!/usr/bin/env bash
#
# What the packages must look like, checked on the packed files themselves.
#
# It lives here rather than inside a workflow because BOTH workflows need it and neither may be
# the one that has it: `build.yml` proves that a pack passes, and `publish.yml` packs again and
# sends what it packed, so the check has to run over the files that are about to leave. Two
# copies of fifty lines of shell would answer the same question until the day they did not.
#
# Run from the repository root, with the packages already in ./artifacts.

set -euo pipefail

cd artifacts

# An analyzer package is the opposite shape of a library: the assembly is in analyzers/ and there
# is no lib/ at all. A lib/ here would have the compiler load the generator as a reference, which
# is a different failure from not loading it and a quieter one.
check_analyzer() {
  unzip -o -q "$2".[0-9]*.nupkg -d "$1"
  test -f "$1/analyzers/dotnet/cs/$2.dll" || { echo "$2: analyzer assembly missing"; exit 1; }
  test -f "$1/README.md"                  || { echo "$2: readme missing";            exit 1; }
  test -f "$1/SKILL.md"                   || { echo "$2: skill missing";             exit 1; }
  if [ -d "$1/lib" ]; then echo "$2 has a lib/ folder:"; find "$1/lib" -type f; exit 1; fi
}

check_analyzer generator DotGram

# What nuget.org shows before anyone reads a word, and what no later version can
# change for this one: the icon, where the project lives, and what to search for.
check_face() {
  test -f "$1/icon.png"                             || { echo "$1: icon missing";       exit 1; }
  grep -q '<icon>icon.png</icon>'       "$1"/*.nuspec || { echo "$1: no <icon>";         exit 1; }
  grep -q '<projectUrl>'                "$1"/*.nuspec || { echo "$1: no <projectUrl>";   exit 1; }
  grep -q '<tags>'                      "$1"/*.nuspec || { echo "$1: no <tags>";         exit 1; }
  grep -q '<releaseNotes>'              "$1"/*.nuspec || { echo "$1: no <releaseNotes>"; exit 1; }
}
check_face generator

# The libraries are one shape: an assembly per target, the README, and no
# generator. No runtime assembly ships (docs/syntax.md §6.1), so the only
# dependency one may acquire is what its framework is missing: System.Memory,
# for the `ReadOnlySpan<char>` the generated methods take, on netstandard2.0
# alone. A dependency on the newest target, or any other name on either, means
# something else got in — the generator itself, or a polyfill that should have
# been private.
check_library() {
  unzip -o -q "$2".[0-9]*.nupkg -d "$1"
  for tfm in netstandard2.0 net10.0; do
    test -f "$1/lib/$tfm/$2.dll" || { echo "$2: $tfm assembly missing"; exit 1; }
  done
  test -f "$1/README.md" || { echo "$2: readme missing"; exit 1; }
  if [ -d "$1/analyzers" ]; then echo "$2 carries the generator:"; find "$1/analyzers" -type f; exit 1; fi
  check_face "$1"
  deps=$(grep -o 'id="[^"]*"' "$1/$2.nuspec" | sed 's/id="//;s/"//' | sort -u)
  test "$deps" = "System.Memory" || { echo "$2: unexpected dependencies:"; echo "$deps"; exit 1; }
  grep -q '<group targetFramework="net10.0" />' "$1/$2.nuspec" || {
    echo "$2: the newest target is not dependency-free:"; grep -A3 'net10.0' "$1/$2.nuspec"; exit 1
  }
}
check_library sql         DotGram.Sql
check_library web         DotGram.Web
check_library finance     DotGram.Finance
check_library expressions DotGram.ExpressionLanguage

# The second analyzer. Its own page and skill, and no lib/ either: it fills tables the library
# reads and carries nothing a consumer references.
check_analyzer finance-generator DotGram.Finance.Generator
check_face finance-generator
