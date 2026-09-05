using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	/// <summary>
	/// Deferral without a tape: what a rule read, kept in a typed shape of its own, and the
	/// author's constructions called by a walk over those shapes once the parse is accepted.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The tape keeps §7.3 by writing what it read into a log and building afterwards, and
	/// pays for it in the walk and in the log. The immediate carrier pays neither and gives
	/// up §7.3. This is the third answer, and the one the second stage exists to measure: a
	/// shape per rule, filled where the tape writes a record, and a <c>Build</c> per shape
	/// that calls the construction — so a factory runs once per node of the accepted
	/// derivation, as §7.3 requires, and what it runs over is fields of a known type rather
	/// than integers read back out of a log.
	/// </para>
	/// <para>
	/// A rule off every cycle is a value held inside whatever captured it, so a leaf costs
	/// no allocation at all; a rule that can reach itself is a reference, because a value
	/// cannot contain itself (<c>Shapes</c>, and <c>benchmarks/DotGram.HandDeferred</c>,
	/// where the shape was written by hand first). A rule with one construction needs no
	/// field saying which it is, and most rules have one: forty-four of the expression
	/// language's seventy-three, and every one of RFC 3986's.
	/// </para>
	/// <para>
	/// <b>What it carries so far: nothing.</b> The shapes are written one at a time, each
	/// with <c>CarrierTests</c> holding it to the same language as the tape, and until a
	/// shape is written this refuses it and the tape carries it instead. That is the order
	/// the immediate carrier was built in and the reason it was never wrong on the way.
	/// </para>
	/// </remarks>
	sealed class MixedCarrier(Machine machine) : ValueCarrier
	{
		public override string? Refuses() => "the shapes are not written yet";

		// Everything below is asked only of a carrier the machine chose, and the machine
		// chooses this one for nothing yet. Each answer arrives with the shape it belongs
		// to, and until then saying so is better than answering wrongly.

		public override IEnumerable<(string Type, string Name)> ReaderState => throw Unwritten();

		public override string GatherHanding(RuleSymbol owner, bool declared, bool inBody) => throw Unwritten();

		public override IEnumerable<string> MarkRecords(string name) => throw Unwritten();

		public override IEnumerable<string> MarkGathered(RuleSymbol? owner, string name) => throw Unwritten();

		public override IEnumerable<string> UnwindRecords(string name) => throw Unwritten();

		public override IEnumerable<string> UnwindGathered(RuleSymbol? owner, string name) => throw Unwritten();

		public override string DeclareRecordLocal(int slot, RuleSymbol rule) => throw Unwritten();

		public override string DeclareAccumulator(RuleSymbol rule) => throw Unwritten();

		public override IEnumerable<string> DeclareGathered(int slot, string elementType) => throw Unwritten();

		public override string RecordLocalType(RuleSymbol rule) => throw Unwritten();

		public override string ResetRecordLocal(int slot) => throw Unwritten();

		public override string Absent(string local) => throw Unwritten();

		public override string FirstRecord(IReadOnlyList<int> slots, RuleSymbol rule) => throw Unwritten();

		public override string Begin(RuleSymbol rule, int factory, string? start, string? end) => throw Unwritten();

		public override string PutAccumulator() => throw Unwritten();

		public override string PutText(DirectMember member, string from, string to) => throw Unwritten();

		public override string PutRecord(DirectMember member, string record) => throw Unwritten();

		public override string Collect(DirectMember member, string from, bool pairs) => throw Unwritten();

		public override string End(string gatheredFrom) => throw Unwritten();

		public override string Last(RuleSymbol rule) => throw Unwritten();

		public override string PushText(int slot, string from, string to) => throw Unwritten();

		public override string PushRecord(int slot, RuleSymbol rule) => throw Unwritten();

		public override string Mark(int kind, int site) => throw Unwritten();

		public override string Materialize(string record, string sinceMark) => throw Unwritten();

		public override string ValueOf(RuleSymbol rule, string record) => throw Unwritten();

		public override void Gathered(
			Writer code, string from, IReadOnlyList<int> slots, string handed, string type, string build, bool text) =>
			throw Unwritten();

		public override IEnumerable<string> Rent() => throw Unwritten();

		public override IEnumerable<string> Return() => throw Unwritten();

		public override IEnumerable<string> BuildRoot(string type, bool extent) => throw Unwritten();

		public override string RenderBuilder(IReadOnlyList<RuleSymbol> rules) => throw Unwritten();

		static InvalidOperationException Unwritten() =>
			new($"The mixed carrier was asked to carry something it refuses ({nameof(Machine)}.Mixed.cs).");
	}
}
