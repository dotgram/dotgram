using DotGram.Finance.Fix;

namespace DotGram.Finance.FixShape;

// Query operators in a file that imports System.Linq (implicitly) and the namespace of FixList<T>.
public partial class FixCollectionShapeBenchmarks
{
	long ProbeOpCount() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.Count(); }
	long ProbeOpCountP() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.Count(x => x.MDEntryType.Value == '0'); }
	long ProbeOpAny() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.Any() ? 1 : 0; }
	long ProbeOpAnyP() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.Any(x => x.MDEntryType.Value == '1') ? 1 : 0; }
	long ProbeOpFirst() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.First().MDEntryType.Value; }
	long ProbeOpFirstP() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.First(x => x.MDEntryType.Value == '1').MDEntryType.Value; }
	long ProbeOpFirstOrDefault() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.FirstOrDefault()?.MDEntryType.Value ?? 0; }
	long ProbeOpFirstOrDefaultP() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.FirstOrDefault(x => x.MDEntryType.Value == '1')?.MDEntryType.Value ?? 0; }
	long ProbeOpToArray() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.ToArray().Length; }
	long ProbeOpSum() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.Sum(x => (long)x.MDEntryType.Value); }
	long ProbeOpToList() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.ToList().Count; }
	long ProbeOpMax() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.Max(x => x.MDEntryType.Value); }
	long ProbeOpWhereForeach() { var e = _snapshots[0].NoMDEntriesGroups ?? []; var n = 0L; foreach (var x in e.Where(x => x.MDEntryType.Value == '0')) n++; return n; }
	long ProbeOpSelectForeach() { var e = _snapshots[0].NoMDEntriesGroups ?? []; var n = 0L; foreach (var x in e.Select(x => (long)x.MDEntryType.Value)) n += x; return n; }
	long ProbeOpOrderByForeach() { var e = _snapshots[0].NoMDEntriesGroups ?? []; var n = 0L; foreach (var x in e.OrderBy(x => x.MDEntryType.Value)) n++; return n; }
	long ProbeOpSkipTakeForeach() { var e = _snapshots[0].NoMDEntriesGroups ?? []; var n = 0L; foreach (var x in e.Skip(10).Take(5)) n++; return n; }
	long ProbeOpElementAt() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.ElementAt(500).MDEntryType.Value; }
	long ProbeOpLast() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.Last().MDEntryType.Value; }
	long ProbeOpContains() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.Contains(e[999]) ? 1 : 0; }
	long ProbeOpNewList() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return new List<DotGram.Finance.Fix.Fix44.FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup>(e).Count; }
}
