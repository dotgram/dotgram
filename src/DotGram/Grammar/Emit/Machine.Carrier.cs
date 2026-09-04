using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	/// <summary>Which carrier the host asked for; what it gets is <see cref="Carrier"/>.</summary>
	readonly CarrierKind _carrierKind;

	/// <summary>Why the carrier asked for was not the one used, or null.</summary>
	public string? CarrierRefusal { get; private set; }

	/// <summary>How this machine's readers carry what they read.</summary>
	/// <remarks>
	/// A property of the machine rather than of a reader because every method of every rule in
	/// a file has to agree on it: a part hands its marks to the body and the entry builds what
	/// the rules recorded. Chosen once, the first time it is asked for, which is after the
	/// machine knows its rules — and the tape where the one asked for cannot carry them, with
	/// the reason kept for whoever asks.
	/// </remarks>
	ValueCarrier Carrier
	{
		get
		{
			if (field is not null)
				return field;

			if (_carrierKind == CarrierKind.Eager)
			{
				var eager = new EagerCarrier(this);

				if (eager.Refuses() is { } why)
					CarrierRefusal = why;
				else
					return field = eager;
			}

			return field = new TapeCarrier(this);
		}
	}

	/// <summary>Whether values are built as they are read rather than after (<see cref="CarrierKind.Eager"/>).</summary>
	internal bool CarriesEagerly => Carrier is EagerCarrier;

	/// <summary>
	/// How a reader carries what it read until the derivation is accepted and the author's
	/// constructions can run.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The reader recognizes; something else holds the pieces of the value it is not yet
	/// allowed to build. Today that something is the tape — a log of records and a stack of
	/// gathered references, both on <c>Ways</c> — and until this seam was cut the reader wrote
	/// the tape's own calls at some fifty sites. Behind this type it writes the same fifty
	/// sites, and what they turn into is the carrier's business.
	/// </para>
	/// <para>
	/// Every method returns the C# to emit, or nothing where a carrier has nothing to do at
	/// that site: a carrier that keeps values in locals has no store to mark and nothing to
	/// put back when an alternative fails. The names are for what a site <em>means</em> — a
	/// record begun, a member put in it, the store put back to a mark — and not for how the
	/// tape does it, so that another carrier can answer the same questions differently
	/// (<c>docs/next.md</c>, the redesign; <see cref="CarrierKind"/> for the ones offered).
	/// </para>
	/// <para>
	/// Marks and unwindings come in two because the tape has two stores and the reader marks
	/// them at different sites under different conditions; where a rule gathers, the second
	/// store is the one a failed turn has to be taken back out of, whatever the carrier keeps
	/// it in. A carrier with one store, or none, answers the other with nothing.
	/// </para>
	/// </remarks>
	abstract class ValueCarrier
	{
		// ---- what a reader is handed ---------------------------------------------------------

		/// <summary>The parameter every reader takes for the carrier's own store, or nothing.</summary>
		public abstract string ReaderParameter { get; }

		/// <summary>And the argument that fills it.</summary>
		public abstract string ReaderArgument { get; }

		/// <summary>
		/// What a part of a rule that gathers is handed so that it can gather into the same
		/// place, each item with its leading comma: declarations where <paramref name="declared"/>,
		/// arguments otherwise, named as the body names them where <paramref name="inBody"/>.
		/// </summary>
		public abstract string GatherHanding(RuleSymbol owner, bool declared, bool inBody);

		// ---- marks and unwinding -------------------------------------------------------------

		/// <summary>Locals remembering where the records stood, to put them back to.</summary>
		public abstract IEnumerable<string> MarkRecords(string name);

		/// <summary>Locals remembering where the gathered members of the rule stood.</summary>
		public abstract IEnumerable<string> MarkGathered(RuleSymbol? owner, string name);

		/// <summary>The records put back to a mark — and with them whatever a guard built above it.</summary>
		public abstract IEnumerable<string> UnwindRecords(string name);

		/// <summary>The gathered members put back to a mark.</summary>
		public abstract IEnumerable<string> UnwindGathered(RuleSymbol? owner, string name);

		// ---- the locals a value is kept in -----------------------------------------------------

		/// <summary>
		/// The local a captured record is kept in until the rule's own record is written — on
		/// the tape an index, elsewhere the value itself.
		/// </summary>
		public abstract string DeclareRecordLocal(int slot, string valueType);

		/// <summary>The local a fold's value so far is kept in (§4.3).</summary>
		public abstract string DeclareAccumulator(string valueType);

		/// <summary>What the body of a rule that gathers into a slot declares for it, beside the position it keeps.</summary>
		public abstract IEnumerable<string> DeclareGathered(int slot, string elementType);

		/// <summary>The type of a record local, with a trailing space, for a parameter that hands it on.</summary>
		public abstract string RecordLocalType(string valueType);

		/// <summary>A record local put back to nothing, when the part that wrote it failed.</summary>
		public abstract string ResetRecordLocal(int slot);

		/// <summary>Whether a record local was never written.</summary>
		public abstract string Absent(string local);

		/// <summary>
		/// The one of a member's record slots that was written: the same name in two
		/// alternatives is one member with a slot per alternative, and the record takes
		/// whichever is set.
		/// </summary>
		public abstract string FirstRecord(IReadOnlyList<int> slots, string valueType);

		// ---- a record -----------------------------------------------------------------------

		/// <summary>
		/// A record of one alternative of a rule begun, with the span it stands on where those
		/// are kept. What follows, up to <see cref="End"/>, is its members in the order the rule
		/// lists them.
		/// </summary>
		public abstract string Begin(RuleSymbol rule, int factory, string? start, string? end);

		/// <summary>The value so far, as a fold step's first member (§4.3).</summary>
		public abstract string PutAccumulator();

		/// <summary>A member that is a span of text.</summary>
		public abstract string PutText(DirectMember member, string from, string to);

		/// <summary>A member that is another record.</summary>
		public abstract string PutRecord(DirectMember member, string record);

		/// <summary>A member gathered across a repetition: everything pushed since the rule began, in its slots.</summary>
		public abstract string Collect(DirectMember member, string from, bool pairs);

		/// <summary>The record closed.</summary>
		public abstract string End(string gatheredFrom);

		/// <summary>An expression for the value of the record most recently closed, of the type.</summary>
		public abstract string Last(string valueType);

		// ---- gathering ----------------------------------------------------------------------

		/// <summary>One piece of text pushed for a member gathered across turns.</summary>
		public abstract string PushText(int slot, string from, string to);

		/// <summary>One record pushed for a member gathered across turns.</summary>
		public abstract string PushRecord(int slot, string valueType);

		/// <summary>A §7.8 mark, opened or closed, at the position.</summary>
		public abstract string Mark(int kind, int site);

		// ---- building -----------------------------------------------------------------------

		/// <summary>A record built into a value where the reader is, for a guard that asks (§3.6); nothing where it already is one.</summary>
		public abstract string Materialize(string record, string sinceMark);

		/// <summary>The value a record holds, as a guard sees it.</summary>
		public abstract string ValueOf(string type, string record);

		/// <summary>The gathered members of the given slots as one array, for a guard that names a sequence member.</summary>
		public abstract void Gathered(Writer code, string from, IReadOnlyList<int> slots, string handed, string type, string build);

		/// <summary>What an entry rents before reading, beside the ways.</summary>
		public abstract IEnumerable<string> Rent();

		/// <summary>And returns after.</summary>
		public abstract IEnumerable<string> Return();

		/// <summary>The whole derivation built into the entry's value.</summary>
		public abstract IEnumerable<string> BuildRoot(string type, bool extent);

		/// <summary>The code that builds records into values, once per file; nothing where values are built as they are read.</summary>
		public abstract string RenderBuilder(IReadOnlyList<RuleSymbol> rules);

		/// <summary>Why this carrier cannot carry the machine's rules, or null where it can.</summary>
		public abstract string? Refuses();

		/// <summary>The slots of a member, as a mask the tape collects by.</summary>
		protected static long MaskOf(IReadOnlyList<int> slots)
		{
			var mask = 0L;

			foreach (var slot in slots)
				mask |= 1L << slot;

			return mask;
		}
	}

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
	sealed class TapeCarrier(Machine machine) : ValueCarrier
	{
		public override string ReaderParameter => machine._directBuilds ? ", DirectValues values" : "";

		public override string ReaderArgument => machine._directBuilds ? ", values" : "";

		/// <remarks>
		/// Where the rule gathers across turns, what a record collects is everything pushed
		/// since the rule began — not since the part did — so the rule's mark is handed on.
		/// </remarks>
		public override string GatherHanding(RuleSymbol owner, bool declared, bool inBody) =>
			declared ? ", int refs" : inBody ? ", rb" : ", refs";

		public override IEnumerable<string> MarkRecords(string name)
		{
			yield return $"var {name} = ways.LogCount;";
		}

		public override IEnumerable<string> MarkGathered(RuleSymbol? owner, string name)
		{
			yield return $"var {name} = ways.RefsCount;";
		}

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

		public override IEnumerable<string> UnwindGathered(RuleSymbol? owner, string name)
		{
			yield return $"ways.RefsCount = {name};";
		}

		public override string DeclareRecordLocal(int slot, string valueType) => $"var r{slot} = -1;";

		public override string DeclareAccumulator(string valueType) => "var fold = -1;";

		public override IEnumerable<string> DeclareGathered(int slot, string elementType) => [];

		public override string RecordLocalType(string valueType) => "int ";

		public override string ResetRecordLocal(int slot) => $"r{slot} = -1;";

		public override string Absent(string local) => $"{local} < 0";

		public override string FirstRecord(IReadOnlyList<int> slots, string valueType)
		{
			if (slots.Count == 1)
				return $"r{slots[0]}";

			var chain = "-1";

			for (var i = slots.Count - 1; i >= 0; i--)
				chain = $"r{slots[i]} >= 0 ? r{slots[i]} : {chain}";

			return $"({chain})";
		}

		public override string Begin(RuleSymbol rule, int factory, string? start, string? end) =>
			start is null
				? $"ways.Begin({machine.DirectArm(rule, factory)});"
				: $"ways.Begin({machine.DirectArm(rule, factory)}, {start}, {end});";

		public override string PutAccumulator() => "ways.Put(fold);";

		public override string PutText(DirectMember member, string from, string to) => $"ways.Put({from}, {to});";

		public override string PutRecord(DirectMember member, string record) => $"ways.Put({record});";

		public override string Collect(DirectMember member, string from, bool pairs) =>
			$"ways.Collect({from}, {member.Mask}L, {(pairs ? "true" : "false")});";

		public override string End(string gatheredFrom) => $"ways.End({gatheredFrom});";

		public override string Last(string valueType) => "ways.Last";

		public override string PushText(int slot, string from, string to) => $"ways.Push({slot}, {from}, {to});";

		public override string PushRecord(int slot, string valueType) => $"ways.Push({slot}, ways.Last, -1);";

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
		public override void Gathered(Writer code, string from, IReadOnlyList<int> slots, string handed, string type, string build)
		{
			var bits    = MaskOf(slots);
			var bracket = type.IndexOf('[');

			code.Line($"var {handed}Count = 0;");
			code.Line($"for (var at = {from}; at < ways.RefsCount; at += 3)");
			code.Then($"if (({bits}L & (1L << ways.Refs[at])) != 0) {handed}Count++;");
			code.Line(
				$"var {handed} = new {(bracket < 0 ? type : type.Substring(0, bracket))}[{handed}Count]" +
				$"{(bracket < 0 ? "" : type.Substring(bracket))};");
			code.Line($"{handed}Count = 0;");

			using (code.Block($"for (var at = {from}; at < ways.RefsCount; at += 3)"))
			{
				code.Line($"if (({bits}L & (1L << ways.Refs[at])) == 0) continue;");

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

		public override string? Refuses() => null;
	}
}
