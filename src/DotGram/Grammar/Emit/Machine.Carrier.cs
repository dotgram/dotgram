using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	/// <summary>How this machine's readers carry what they read.</summary>
	/// <remarks>
	/// One carrier today, and the seam is what the second one will be written behind. It is
	/// a property of the machine rather than of a reader because every method of every rule
	/// in a file has to agree on it: a part hands its marks to the body and the entry builds
	/// what the rules recorded.
	/// </remarks>
	internal Carrier Carrier => field ??= new TapeCarrier(this);

	/// <summary>
	/// The tape: records in a log and gathered references on a stack, both on <c>Ways</c>,
	/// built into values by a walk over the log once the derivation is accepted.
	/// </summary>
	/// <remarks>
	/// Every string here is what the reader wrote itself before the seam was cut, character
	/// for character, and the snapshots are what say so. The tape is the carrier that
	/// streams, finds and recovers — the others cannot yet — and it stays behind the seam
	/// for as long as that is true (<c>docs/next.md</c>).
	/// </remarks>
	sealed class TapeCarrier(Machine machine) : Carrier
	{
		public override string MarkRecords(string name) => $"var {name} = ways.LogCount;";

		public override string MarkGathered(string name) => $"var {name} = ways.RefsCount;";

		/// <remarks>
		/// With the watermark of what a guard built, where anything builds: a record above
		/// the watermark is one written since, and a value a guard built in a derivation that
		/// was then abandoned is not the value of the record the next derivation writes at
		/// the same place.
		/// </remarks>
		public override IEnumerable<string> UnwindRecords(string name)
		{
			yield return $"ways.LogCount  = {name};";

			if (machine._directBuilds)
				yield return $"if (ways.Built > {name}) ways.Built = {name};";
		}

		public override string UnwindGathered(string name) => $"ways.RefsCount = {name};";

		public override string Begin(int arm, string? start, string? end) =>
			start is null ? $"ways.Begin({arm});" : $"ways.Begin({arm}, {start}, {end});";

		public override string PutAccumulator() => "ways.Put(fold);";

		public override string PutText(string from, string to) => $"ways.Put({from}, {to});";

		public override string PutRecord(string record) => $"ways.Put({record});";

		public override string Collect(string from, long slots, bool pairs) =>
			$"ways.Collect({from}, {slots}L, {(pairs ? "true" : "false")});";

		public override string End(string gatheredFrom) => $"ways.End({gatheredFrom});";

		public override string Last => "ways.Last";

		public override string PushText(int slot, string from, string to) => $"ways.Push({slot}, {from}, {to});";

		public override string PushRecord(int slot) => $"ways.Push({slot}, ways.Last, -1);";

		public override string Mark(int kind, int site) => $"ways.Mark({kind}, {site}, p);";

		public override string Materialize(string record, string sinceMark) =>
			$"{machine.DirectMaterializer}(ways, text, values, {record}, {sinceMark}" +
			$"{machine.TokensArgument}{machine.ContextArgument});";

		/// <summary>From the tables, or for an extent the record itself.</summary>
		public override string ValueOf(string type, string record) =>
			type == "SourceSpan"
				? machine.RecordValue(type, record).Replace("log[", "ways.Log[")
				: $"values.V{machine.TableFor(type)}[{record}].Value";

		/// <remarks>
		/// Gathered turn by turn on the tape, and collected here the way the rule's end would
		/// collect them: counted first so the array is the right size, then visited.
		/// </remarks>
		public override void Gathered(Writer code, string from, long slots, string handed, string type, string build)
		{
			var bracket = type.IndexOf('[');

			code.Line($"var {handed}Count = 0;");
			code.Line($"for (var at = {from}; at < ways.RefsCount; at += 3)");
			code.Then($"if (({slots}L & (1L << ways.Refs[at])) != 0) {handed}Count++;");
			code.Line(
				$"var {handed} = new {(bracket < 0 ? type : type.Substring(0, bracket))}[{handed}Count]" +
				$"{(bracket < 0 ? "" : type.Substring(bracket))};");
			code.Line($"{handed}Count = 0;");

			using (code.Block($"for (var at = {from}; at < ways.RefsCount; at += 3)"))
			{
				code.Line($"if (({slots}L & (1L << ways.Refs[at])) == 0) continue;");

				if (build.Length > 0)
					code.Line(string.Format(build, "ways.Refs[at + 1]"));

				code.Line($"{handed}[{handed}Count++] = {ValueOf(type, "ways.Refs[at + 1]")};");
			}
		}

		public override IEnumerable<string> Rent()
		{
			yield return "var values = DirectValues.Rent();";
		}

		public override IEnumerable<string> Return()
		{
			yield return "DirectValues.Return(values);";
		}

		/// <remarks>
		/// An extent's value is the span its record stands on; every other value is in the
		/// tables the walk filled.
		/// </remarks>
		public override IEnumerable<string> BuildRoot(string type, bool extent)
		{
			yield return
				$"{machine.DirectMaterializer}(ways, text, values, ways.Last, 0" +
				$"{machine.InputArgument}{machine.TokensArgument}{machine.ContextArgument});";

			yield return
				$"value = {(extent ? machine.RecordValue(type, "ways.Last").Replace("log[", "ways.Log[") : machine.DirectFrom(type, "ways.Last").Replace("values", "values.V"))};";
		}

		public override string RenderBuilder(IReadOnlyList<RuleSymbol> rules) => machine.RenderDirectMaterializer(rules);
	}
}
