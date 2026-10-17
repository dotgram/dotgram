using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using DotGram.Finance.Fix;

namespace DotGram.Finance.FixShape;

// FixShape --probe: what the query operators of the column's list type allocate in a call, measured on the process's own
// allocation counter after warm-up, and which methods the compiler bound each call to (read from the IL).
public partial class FixCollectionShapeBenchmarks
{
	long ProbeCount()               { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.Count(); }
	long ProbeCountPredicate()      { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.Count(x => x.MDEntryType.Value == '0'); }
	long ProbeAny()                 { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.Any() ? 1 : 0; }
	long ProbeAnyPredicate()        { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.Any(x => x.MDEntryType.Value == '1') ? 1 : 0; }
	long ProbeFirst()               { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.First().MDEntryType.Value; }
	long ProbeFirstPredicate()      { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.First(x => x.MDEntryType.Value == '1').MDEntryType.Value; }
	long ProbeFirstOrDefault()      { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.FirstOrDefault()?.MDEntryType.Value ?? 0; }
	long ProbeFirstOrDefaultPredicate() { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.FirstOrDefault(x => x.MDEntryType.Value == '1')?.MDEntryType.Value ?? 0; }
	long ProbeWhere()               { var e = _snapshots[0].NoMDEntriesGroups ?? []; var n = 0L; foreach (var x in e.Where(x => x.MDEntryType.Value == '0')) n++; return n; }
	long ProbeToArray()             { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.ToArray().Length; }
	long ProbeWhereToArray()        { var e = _snapshots[0].NoMDEntriesGroups ?? []; return e.Where(x => x.MDEntryType.Value == '0').ToArray().Length; }

	public static int Probe()
	{
		var bench = new FixCollectionShapeBenchmarks();

		bench.Setup();

		var names = typeof(FixCollectionShapeBenchmarks).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
			.Where(m => m.Name.StartsWith("Probe", StringComparison.Ordinal) && m.ReturnType == typeof(long) && m.GetParameters().Length == 0)
			.Select(m => m.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray();


		foreach (var name in names)
		{
			var method = typeof(FixCollectionShapeBenchmarks).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!;
			var call   = (Func<long>)Delegate.CreateDelegate(typeof(Func<long>), bench, method);
			var result = 0L;

			for (var i = 0; i < 20000; i++)
				result = call();

			System.Threading.Thread.Sleep(300);

			for (var i = 0; i < 20000; i++)
				result = call();

			var before = GC.GetAllocatedBytesForCurrentThread();

			for (var i = 0; i < 1000; i++)
				result = call();

			var bytes = (GC.GetAllocatedBytesForCurrentThread() - before) / 1000.0;

			Console.WriteLine($"{name,-32} result {result,5}  alloc {bytes,8:F1} B/call  IL: {Calls(method)}");
		}

		return 0;
	}

	// The calls, boxes and casts of a method body, from its IL.
	static string Calls(MethodInfo method)
	{
		var il     = method.GetMethodBody()!.GetILAsByteArray()!;
		var module = method.Module;
		var all    = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => (OpCode)f.GetValue(null)!).ToDictionary(o => (ushort)o.Value);
		var text   = new StringBuilder();

		for (var i = 0; i < il.Length;)
		{
			var code = (ushort)il[i++];

			if (code == 0xFE)
				code = (ushort)(0xFE00 | il[i++]);

			var op = all[code];
			var size = op.OperandType switch
			{
				OperandType.InlineNone => 0,
				OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
				OperandType.InlineVar => 2,
				OperandType.InlineI8 or OperandType.InlineR => 8,
				OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, i),
				_ => 4,
			};

			if (op == OpCodes.Call || op == OpCodes.Callvirt)
			{
				var target = module.ResolveMethod(BitConverter.ToInt32(il, i), method.DeclaringType!.GetGenericArguments(), null)!;
				var name   = target.DeclaringType!.Name + "." + target.Name;

				if (name.StartsWith("Fix", StringComparison.Ordinal) || name.StartsWith("Enumerable", StringComparison.Ordinal) || name.StartsWith("List", StringComparison.Ordinal))
					text.Append(name).Append(' ');
			}
			else if (op == OpCodes.Box || op == OpCodes.Castclass || op == OpCodes.Isinst)
				text.Append(op.Name).Append(' ');

			i += size;
		}

		return text.ToString().Trim();
	}
}
