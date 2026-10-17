
namespace DotGram.Finance.FixShape;

// The same in a file that imports no namespace of the package: System.Linq (implicit) only.
public partial class FixCollectionShapeBenchmarks
{
	long ProbeSlCount() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; return e.Count(); }
	long ProbeSlCountP() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; return e.Count(x => x.MDEntryType.Value == '0'); }
	long ProbeSlAny() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; return e.Any() ? 1 : 0; }
	long ProbeSlAnyP() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; return e.Any(x => x.MDEntryType.Value == '1') ? 1 : 0; }
	long ProbeSlFirst() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; return e.First().MDEntryType.Value; }
	long ProbeSlFirstP() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; return e.First(x => x.MDEntryType.Value == '1').MDEntryType.Value; }
	long ProbeSlFirstOrDefault() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; return e.FirstOrDefault()?.MDEntryType.Value ?? 0; }
	long ProbeSlFirstOrDefaultP() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; return e.FirstOrDefault(x => x.MDEntryType.Value == '1')?.MDEntryType.Value ?? 0; }
	long ProbeSlToArray() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; return e.ToArray().Length; }
	long ProbeSlSum() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; return e.Sum(x => (long)x.MDEntryType.Value); }
	long ProbeSlToList() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; return e.ToList().Count; }
	long ProbeSlMax() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; return e.Max(x => x.MDEntryType.Value); }
	long ProbeSlWhereForeach() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; var n = 0L; foreach (var x in e.Where(x => x.MDEntryType.Value == '0')) n++; return n; }
	long ProbeSlSelectForeach() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; var n = 0L; foreach (var x in e.Select(x => (long)x.MDEntryType.Value)) n += x; return n; }
	long ProbeSlOrderByForeach() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; var n = 0L; foreach (var x in e.OrderBy(x => x.MDEntryType.Value)) n++; return n; }
	long ProbeSlSkipTakeForeach() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; var n = 0L; foreach (var x in e.Skip(10).Take(5)) n++; return n; }
	long ProbeSlElementAt() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; return e.ElementAt(500).MDEntryType.Value; }
	long ProbeSlLast() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; return e.Last().MDEntryType.Value; }
	long ProbeSlContains() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; return e.Contains(e[999]) ? 1 : 0; }
	long ProbeSlNewList() { var e = _snapshots[0].NoMDEntriesGroups; if (e is null) return 0; return new List<DotGram.Finance.Fix.Fix44.FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup>(e).Count; }
}
