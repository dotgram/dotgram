# SPIKE (not for merging). Writes Rows.Fix42.cs, Rows.Fix44.cs and Rows.Fix50.cs beside it: the rows of each version from one template.
import sys
import os
out=os.path.dirname(os.path.abspath(__file__)) + '/'

def loop(ind, var, expr):
    return (f"#if FIX_SHAPE_C || FIX_SHAPE_D\n{ind}foreach (var {var} in {expr})\n#else\n{ind}foreach (var {var} in {expr} ?? [])\n#endif\n")

def count(expr):
    return ("#if FIX_SHAPE_D\n\t\t\t" + f"{expr}.Length" + "\n#else\n\t\t\t" + f"{expr}.Count" + "\n#endif\n")

def rows(v):
    ns = f"Fix{v}"
    full = v in ("44","50")    # the group of an Order is Parties
    all_rows = v == "44"
    s = f"""// SPIKE (not for merging): the rows of {ns}, written as a consumer of each column writes them. Written by
// a script from one template; the three versions differ in their types and in the group of an Order.

using System.Linq;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

using M{v} = DotGram.Finance.Fix.{ns}.FixMessage;
using P{v} = DotGram.Finance.Fix.{ns}.FixParser;
using C{v} = DotGram.Finance.Fix.{ns}.{ns}Context;

namespace DotGram.Finance.Benchmarks.ShapeSpike;

public partial class FixCollectionShapeBenchmarks
{{
	byte[] order{v}Bytes = [];
	byte[] snap{v}Bytes = [];
	M{v}.NewOrderSingle[] orders{v} = [];
	M{v}.NewOrderSingle[] bare{v} = [];
	M{v}.MarketDataSnapshotFullRefresh snap{v} = null!;

	void Setup{v}()
	{{
		order{v}Bytes = Latin1(Wire("{v}", "D", OrderBody("{v}", true)));
		snap{v}Bytes  = Latin1(Wire("{v}", "W", SnapshotBody()));
		orders{v}     = new M{v}.NewOrderSingle[Batch];
		bare{v}       = new M{v}.NewOrderSingle[Batch];

		var bareBytes = Latin1(Wire("{v}", "D", OrderBody("{v}", false)));

		for (var i = 0; i < Batch; i++)
		{{
			orders{v}[i] = (M{v}.NewOrderSingle)P{v}.ParseMessage(order{v}Bytes);
			bare{v}[i]   = (M{v}.NewOrderSingle)P{v}.ParseMessage(bareBytes);
		}}

		snap{v} = (M{v}.MarketDataSnapshotFullRefresh)P{v}.ParseMessage(snap{v}Bytes);
	}}

	static long Digest{v}(M{v} message)
	{{
		long digest = message.IsValid ? 0 : 1;
		long fields;

		foreach (var field in message.Fields)
			digest += field.Tag;

"""
    s += "#if FIX_SHAPE_D\n\t\tfields = message.Fields.Length;\n#else\n\t\tfields = message.Fields.Count;\n#endif\n"
    s += "\t\tdigest += fields * 1000003;\n\n"
    s += "\t\tif (message is M%s.NewOrderSingle order)\n\t\t{\n" % v
    grp = "NoPartyIDsGroups" if full else "NoAllocsGroups"
    s += loop("\t\t\t","e",f"order.{grp}").replace("\t\t\tforeach","\t\t\tforeach") + "\t\t\t\tdigest += 7;\n"
    s += "\t\t}\n\n\t\tif (message is M%s.MarketDataSnapshotFullRefresh snapshot)\n\t\t{\n" % v
    s += loop("\t\t\t","e","snapshot.NoMDEntriesGroups") + "\t\t\t\tdigest += e.MDEntryPx!.Position;\n\t\t}\n\n\t\treturn digest;\n\t}\n\n"

    # helpers (the I rows' bodies)
    s += f"\t// I1: foreach over the entries of a snapshot.\n\tstatic long Entries{v}(M{v}.MarketDataSnapshotFullRefresh message)\n\t{{\n\t\tlong sum = 0;\n\n"
    s += loop("\t\t","e","message.NoMDEntriesGroups") + "\t\t\tsum += e.MDEntryPx!.Position;\n\n\t\treturn sum;\n\t}\n\n"
    s += f"\t// I4: foreach over the Fields of a message.\n\tstatic long Fields{v}(M{v} message)\n\t{{\n\t\tlong sum = 0;\n\n\t\tforeach (var field in message.Fields)\n\t\t\tsum += field.Tag;\n\n\t\treturn sum;\n\t}}\n\n"
    # nested
    if full:
        s += f"\t// I3: parties, then the sub-IDs of each.\n\tstatic long Nested{v}(M{v}.NewOrderSingle message)\n\t{{\n\t\tlong sum = 0;\n\n"
        s += loop("\t\t","party","message.NoPartyIDsGroups") + "\t\t{\n\t\t\tsum += party.PartyID.Position;\n\n"
        s += loop("\t\t\t","sub","party.NoPartySubIDsGroups") + "\t\t\t\tsum += sub.PartySubID.Position;\n\t\t}\n\n\t\treturn sum;\n\t}\n\n"
    else:
        s += f"\t// The nested walk of 4.2 has no second level: the entries of the allocation group.\n\tstatic long Nested{v}(M{v}.NewOrderSingle message)\n\t{{\n\t\tlong sum = 0;\n\n"
        s += loop("\t\t","alloc","message.NoAllocsGroups") + "\t\t\tsum += alloc.AllocAccount.Position + alloc.AllocShares!.Position;\n\n\t\treturn sum;\n\t}\n\n"
    s += f"\t// I6: the values of ExecInst.\n\tstatic long Multiple{v}(M{v}.NewOrderSingle message)\n\t{{\n\t\tlong sum = 0;\n\n\t\tforeach (var value in message.ExecInst!.Value)\n\t\t\tsum += value.Length;\n\n\t\treturn sum;\n\t}}\n\n"

    def bench(cats, name, ret, body):
        c = ", ".join(f'"{x}"' for x in cats)
        return f"\t[Benchmark, BenchmarkCategory({c})]\n\tpublic {ret} {ns}_{name}()\n\t{{\n{body}\t}}\n\n"

    if all_rows:
        s += bench(["W1"], "P1_ParseOrder", "object", f"\t\treturn P{v}.ParseMessage(order{v}Bytes);\n")
        s += bench(["W1"], "P2_ParseSnapshot", "object", f"\t\treturn P{v}.ParseMessage(snap{v}Bytes);\n")
        s += bench(["W1"], "V1_ParseValidateOrder", "object", f"\t\tvar message = P{v}.ParseMessage(order{v}Bytes);\n\n\t\tmessage.Validate(C{v}.Default);\n\n\t\treturn message;\n")
        s += bench(["W1"], "V2_ParseValidateSnapshot", "object", f"\t\tvar message = P{v}.ParseMessage(snap{v}Bytes);\n\n\t\tmessage.Validate(C{v}.Default);\n\n\t\treturn message;\n")
        s += bench(["W1","W3","W4"], "I1_GroupsForeach", "long", f"\t\treturn Entries{v}(snap{v});\n")
        # I2
        b = f"\t\tlong sum = 0;\n\n#if FIX_SHAPE_D\n\t\tvar list = snap{v}.NoMDEntriesGroups;\n\n\t\tfor (var i = 0; i < list.Length; i++)\n#elif FIX_SHAPE_C\n\t\tvar list = snap{v}.NoMDEntriesGroups;\n\n\t\tfor (var i = 0; i < list.Count; i++)\n#else\n\t\tvar list = snap{v}.NoMDEntriesGroups ?? [];\n\n\t\tfor (var i = 0; i < list.Count; i++)\n#endif\n\t\t\tsum += list[i].MDEntryPx!.Position;\n\n\t\treturn sum;\n"
        s += bench(["W1","W3","W4"], "I2_GroupsFor", "long", b)
        s += bench(["W1","W3","W4"], "I3_NestedForeach", "long", f"\t\tlong sum = 0;\n\n\t\tforeach (var order in orders{v})\n\t\t\tsum += Nested{v}(order);\n\n\t\treturn sum;\n")
        s += bench(["W1","W3","W4"], "I4_FieldsForeach", "long", f"\t\treturn Fields{v}(snap{v});\n")
        b = f"#if FIX_SHAPE_C || FIX_SHAPE_D\n\t\treturn snap{v}.NoMDEntriesGroups.Count(static e => e.MDEntryType?.Value == '0');\n#else\n\t\treturn (snap{v}.NoMDEntriesGroups ?? []).Count(static e => e.MDEntryType?.Value == '0');\n#endif\n"
        s += bench(["W1","W3","W4"], "I5_LinqCount", "long", b)
        s += bench(["W1","W3"], "I6_MultipleForeach", "long", f"\t\tlong sum = 0;\n\n\t\tforeach (var order in orders{v})\n\t\t\tsum += Multiple{v}(order);\n\n\t\treturn sum;\n")
        b = f"\t\tlong sum = 0;\n\n\t\tforeach (var order in bare{v})\n\t\t{{\n" + loop("\t\t\t","party","order.NoPartyIDsGroups") + "\t\t\t\tsum += party.PartyID.Position;\n\n\t\t\tsum++;\n\t\t}\n\n\t\treturn sum;\n"
        s += bench(["W1","W3","W4"], "I7_AbsentForeach", "long", b)
        s += bench(["W1","W3"], "E1_ParseIterateSnapshot", "long", f"\t\tvar message = (M{v}.MarketDataSnapshotFullRefresh)P{v}.ParseMessage(snap{v}Bytes);\n\n\t\treturn Entries{v}(message) + Fields{v}(message);\n")
        s += bench(["W1","W3"], "E2_ParseIterateOrder", "long", f"\t\tvar message = (M{v}.NewOrderSingle)P{v}.ParseMessage(order{v}Bytes);\n\n\t\treturn Nested{v}(message) + Multiple{v}(message);\n")
    if not all_rows:
        s += bench(["W2"], "P2_ParseSnapshot", "object", f"\t\treturn P{v}.ParseMessage(snap{v}Bytes);\n")
        s += bench(["W2"], "E1_ParseIterateSnapshot", "long", f"\t\tvar message = (M{v}.MarketDataSnapshotFullRefresh)P{v}.ParseMessage(snap{v}Bytes);\n\n\t\treturn Entries{v}(message) + Fields{v}(message);\n")
        s += bench(["W2"], "E2_ParseIterateOrder", "long", f"\t\tvar message = (M{v}.NewOrderSingle)P{v}.ParseMessage(order{v}Bytes);\n\n\t\treturn Nested{v}(message) + Multiple{v}(message);\n")
    if all_rows:
        s_list = ["P1_ParseOrder","P2_ParseSnapshot","V1_ParseValidateOrder","V2_ParseValidateSnapshot","I1_GroupsForeach","I2_GroupsFor","I3_NestedForeach","I4_FieldsForeach","I5_LinqCount","I6_MultipleForeach","I7_AbsentForeach","E1_ParseIterateSnapshot","E2_ParseIterateOrder"]
    else:
        s_list = ["P2_ParseSnapshot","E1_ParseIterateSnapshot","E2_ParseIterateOrder"]
    s += f"\t// What each row of {ns} answers once, to be held to the constants in the source.\n\tvoid Check{v}(Action<string, long> check)\n\t{{\n"
    for n in s_list:
        if n[0] in "PV":
            s += f'\t\tcheck("{ns}_{n}", Digest{v}((M{v}){ns}_{n}()));\n'
        else:
            s += f'\t\tcheck("{ns}_{n}", {ns}_{n}());\n'
    s += "\t}\n}\n"
    return s

for v in ("42","44","50"):
    s = rows(v)
    s = s.replace("(M%s)Fix" % v, "(M%s)Fix" % v)
    open(out+f"Rows.Fix{v}.cs",'w',encoding='utf-8',newline='').write(s)
