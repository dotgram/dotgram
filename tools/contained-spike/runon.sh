#!/usr/bin/env bash
# runon.sh <name> [maxx]
set -e
S=/ramdisk/tmp/contained-spike; N=$1
cd /ramdisk/agents/dotgram/contained-spike
source ~/.profile; export MSBUILDDISABLENODEREUSE=1 TMPDIR=$S
B="dotnet build src/DotGram.Sql/DotGram.Sql.csproj -c Release -f net10.0 -nodeReuse:false -p:UseSharedCompilation=false"
rm -rf $S/dump-$N; rm -f src/DotGram.Sql/obj/Release/net10.0/DotGram.Sql.dll
[ -n "$2" ] && export DOTGRAM_SPIKE_MAXX=$2
DOTGRAM_SPIKE_CONTAINED=1 DOTGRAM_SPIKE_DUMP=$S/dump-$N benchmarks/Aside.sh $B > /ramdisk/logs/contained-spike-$N.log 2>&1
mkdir -p $S/$N; rm -rf $S/$N/gen; cp -r src/DotGram.Sql/obj/GeneratedFiles/DotGram/DotGram.Generation.GramGenerator $S/$N/gen; cp src/DotGram.Sql/obj/Release/net10.0/DotGram.Sql.dll $S/$N/
