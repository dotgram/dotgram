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

# The libraries are one shape: an assembly per target, the README, and no generator. No runtime
# assembly ships (docs/syntax.md §6.1), so what a library may depend on is named here, per target,
# and anything else means something got in — the generator itself, or a polyfill that should have
# been private. Every library takes System.Memory on netstandard2.0, for the `ReadOnlySpan<char>` the
# generated methods take, and nothing on net10.0. DotGram.Finance takes two more, each on purpose:
# DotGram.ExpressionLanguage, which compiles the checks a loaded dictionary describes, on both
# targets; and Portable.System.DateTimeOnly, the DateOnly and TimeOnly its date and time fields
# hold, on netstandard2.0, where the runtime has neither.

# The ids a target's group of the nuspec depends on, one a line, sorted; nothing for an empty group.
deps_of() {
  awk -v tfm="$2" '
    /<group targetFramework=/ { inside = index($0, "\"" tfm "\"") > 0 && $0 !~ /\/>[[:space:]]*$/; next }
    /<\/group>/              { inside = 0; next }
    inside && /<dependency / { if (match($0, /id="[^"]*"/)) print substr($0, RSTART + 4, RLENGTH - 5) }
  ' "$1" | sort -u
}

check_library() {
  local dir="$1" id="$2" standard="$3" newest="$4"
  unzip -o -q "$id".[0-9]*.nupkg -d "$dir"
  for tfm in netstandard2.0 net10.0; do
    test -f "$dir/lib/$tfm/$id.dll" || { echo "$id: $tfm assembly missing"; exit 1; }
  done
  test -f "$dir/README.md" || { echo "$id: readme missing"; exit 1; }
  if [ -d "$dir/analyzers" ]; then echo "$id carries the generator:"; find "$dir/analyzers" -type f; exit 1; fi
  check_face "$dir"

  local want got
  want=$(printf '%s\n' $standard | sed '/^$/d' | sort -u)
  got=$(deps_of "$dir/$id.nuspec" ".NETStandard2.0")
  test "$got" = "$want" || { echo "$id: netstandard2.0 depends on"; echo "$got"; echo "and should depend on"; echo "$want"; exit 1; }

  want=$(printf '%s\n' $newest | sed '/^$/d' | sort -u)
  got=$(deps_of "$dir/$id.nuspec" "net10.0")
  test "$got" = "$want" || { echo "$id: net10.0 depends on"; echo "$got"; echo "and should depend on"; echo "$want"; exit 1; }
}

check_library sql         DotGram.Sql                "System.Memory" ""
check_library web         DotGram.Web                "System.Memory" ""
check_library finance     DotGram.Finance            "System.Memory Portable.System.DateTimeOnly DotGram.ExpressionLanguage" "DotGram.ExpressionLanguage"
check_library expressions DotGram.ExpressionLanguage "System.Memory" ""
