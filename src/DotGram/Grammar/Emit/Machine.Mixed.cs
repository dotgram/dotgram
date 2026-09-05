using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

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
	/// <b>What it carries so far</b>: a rule that builds one way, out of runs of text and of
	/// other rules' values, over characters. Everything else is refused and the tape carries
	/// it — the list is <see cref="MixedCarrier.Refuses"/>, and it shortens a shape at a
	/// time, each with <c>CarrierTests</c> holding it to the same language as the tape. That
	/// is the order the immediate carrier was built in and the reason it was never wrong on
	/// the way.
	/// </para>
	/// </remarks>
	sealed class MixedCarrier(Machine machine) : ValueCarrier
	{
		// The record under construction: what Begin was told, and the members put since —
		// the same bookkeeping the immediate carrier does, ending in a shape rather than in
		// a call to the author.
		RuleSymbol? _rule;
		int _factory;
		readonly List<(DirectMember Member, string Value)> _puts = [];

		/// <summary>
		/// Which shapes are not written. Everything named here is left to the tape, and this
		/// is the list that shortens as they are written.
		/// </summary>
		/// <remarks>A register per rule, so a value handed up is not where the caller reads.</remarks>
		public override bool ForwardsInPlace => false;

		public override string? Refuses()
		{
			if (machine.OverKinds)
				return "it reads tokens";

			if (machine._graph.Recoveries.Count > 0)
				return "it recovers";

			if (machine._graph.State is not null)
				return "it lays marks over what it reads";

			foreach (var rule in Shaped)
			{
				if (machine.IsExtent(rule))
					return $"'{rule.Name}' is an extent";

				if (machine._graph.Folds.ContainsKey(rule))
					return $"'{rule.Name}' folds";

				if (machine._graph.Climbing.ContainsKey(rule))
					return $"'{rule.Name}' is read at a strength";

				if (NodeWalk.Descendants(machine._graph.Bodies[rule]).Any(one => one is Node.Guard))
					return $"'{rule.Name}' has a guard";

				foreach (var member in Union(rule))
					if (member.Shape is MemberShape.Pieces or MemberShape.Records)
						return $"'{rule.Name}' gathers '{member.Member.Name}' across turns";
			}

			return null;
		}

		/// <summary>The rules that have a shape: the ones the reader reads and that build.</summary>
		IEnumerable<RuleSymbol> Shaped =>
			(machine._directRules ?? machine._rules).Where(machine.Valued);

		/// <summary>What a rule's shape is called, and the reader's field holding the last one.</summary>
		static string Named(RuleSymbol rule) => "Shape_" + CSharpEmitter.IdentifierOf(rule);

		static string Register(RuleSymbol rule) => "shape_" + CSharpEmitter.IdentifierOf(rule);

		/// <summary>
		/// Whether a rule's shape is a reference: whether the rule can be reached from
		/// itself, which is the whole of the question.
		/// </summary>
		/// <remarks>
		/// A value cannot contain itself, so something on every cycle has to be one. Nothing
		/// off a cycle needs to be, and those are held inside whatever captured them — which
		/// is where the leaves are, and why they cost no allocation. Left recursion is not a
		/// cycle for this: §4.3 turned it into a loop before the reader saw it.
		/// </remarks>
		bool Reference(RuleSymbol rule) => machine._graph.Calls.Recurses(rule);

		/// <summary>A shape as a member or a local holds it: nullable where it is a reference.</summary>
		string Held(RuleSymbol rule) => Named(rule) + (Reference(rule) ? "?" : "");

		/// <summary>Nothing there, said the way the shape says it.</summary>
		string Nothing(RuleSymbol rule, string local) =>
			Reference(rule) ? $"{local} == null" : $"{local}.IsNothing";

		/// <remarks>
		/// A field per rule rather than one per value type: two rules may build the same type
		/// and their shapes are still two types. Fields of the reader for the reason
		/// <see cref="ValueCarrier.ReaderRegisters"/> gives.
		/// </remarks>
		public override IEnumerable<(string Type, string Name)> ReaderRegisters
		{
			get
			{
				foreach (var rule in Shaped)
					yield return (Named(rule), Register(rule));
			}
		}

		public override IEnumerable<(string Type, string Name)> ReaderState => [];

		public override string GatherHanding(RuleSymbol owner, bool declared, bool inBody) => "";

		public override IEnumerable<string> MarkRecords(string name) => [];

		public override IEnumerable<string> MarkGathered(RuleSymbol? owner, string name) => [];

		public override IEnumerable<string> UnwindRecords(string name) => [];

		public override IEnumerable<string> UnwindGathered(RuleSymbol? owner, string name) => [];

		public override string DeclareRecordLocal(int slot, RuleSymbol rule) => $"{Held(rule)} r{slot} = default;";

		public override string RecordLocalType(RuleSymbol rule) => Held(rule) + " ";

		public override string ResetRecordLocal(int slot) => $"r{slot} = default;";

		public override string Absent(string local) => $"{local}.IsNothing";

		public override string FirstRecord(IReadOnlyList<int> slots, RuleSymbol rule)
		{
			if (slots.Count == 1)
				return $"r{slots[0]}";

			var chain = "default";

			for (var i = slots.Count - 1; i >= 0; i--)
				chain = $"!({Nothing(rule, "r" + slots[i])}) ? r{slots[i]} : {chain}";

			return chain;
		}

		public override string Begin(RuleSymbol rule, int factory, string? start, string? end)
		{
			_rule    = rule;
			_factory = factory;
			_puts.Clear();

			return "";
		}

		/// <remarks>Where it stands and not what it says: the text is cut when it is built.</remarks>
		public override string PutText(DirectMember member, string from, string to)
		{
			_puts.Add((member, $"{from}, {to}"));

			return "";
		}

		public override string PutRecord(DirectMember member, string record)
		{
			_puts.Add((member, record));

			return "";
		}

		/// <summary>The shape, made now; what it is worth is asked for after the parse.</summary>
		public override string End(string gatheredFrom)
		{
			var rule = _rule ?? throw new InvalidOperationException("A record ended that never began.");
			var made = machine.DirectMembers(rule, _factory).Select(Value);

			return $"{Register(rule)} = {Named(rule)}.{Maker(Which(rule, _factory))}({string.Join(", ", made)});";

			string Value(DirectMember member)
			{
				foreach (var (put, value) in _puts)
					if (ReferenceEquals(put, member) || put.Index == member.Index)
						return value;

				throw new InvalidOperationException($"Member '{member.Member.Name}' of '{rule.Name}' was never put.");
			}
		}

		public override string Last(RuleSymbol rule) => Register(rule);

		public override IEnumerable<string> Rent() => [];

		public override IEnumerable<string> Return() => [];

		public override IEnumerable<string> BuildRoot(RuleSymbol rule, string type, bool extent)
		{
			yield return $"value = reader.{Register(rule)}.Build(text);";
		}

		/// <summary>How many things a rule's shape may be.</summary>
		/// <remarks>
		/// A rule with no <c>=&gt;</c> builds its own type out of its members, which is one
		/// way of being like any other.
		/// </remarks>
		int Ways(RuleSymbol rule) => Math.Max(1, machine._factories[rule].Count);

		/// <summary>Which of them a factory is, and which factory that way is.</summary>
		int Which(RuleSymbol rule, int factory) => machine._factories[rule].Count == 0 ? 0 : factory;

		int Factory(RuleSymbol rule, int which) => machine._factories[rule].Count == 0 ? -1 : which;

		static string Maker(int which) => "Of" + which.ToString(System.Globalization.CultureInfo.InvariantCulture);

		/// <summary>Every member of every construction, each named once and in one order.</summary>
		IReadOnlyList<DirectMember> Union(RuleSymbol rule)
		{
			var union = new List<DirectMember>();

			for (var which = 0; which < Ways(rule); which++)
				foreach (var member in machine.DirectMembers(rule, Factory(rule, which)))
					if (!union.Exists(one => one.Index == member.Index))
						union.Add(member);

			union.Sort((a, b) => a.Index.CompareTo(b.Index));

			return union;
		}

		/// <summary>
		/// One shape per rule: what it read, which way it read it, and what it is worth once
		/// the parse is accepted.
		/// </summary>
		/// <remarks>
		/// One type for the rule and a byte saying which construction it holds, rather than a
		/// type per construction and a virtual call to build it. The fields are the union of
		/// the constructions', so a rule of thirty alternatives has a shape wide enough for
		/// all of them; whether that is worth what it saves is what the yardsticks are for
		/// (docs/next.md).
		/// </remarks>
		public override string RenderBuilder(IReadOnlyList<RuleSymbol> rules)
		{
			var file = new Writer(0);

			foreach (var rule in Shaped)
			{
				var fields = Fields(Union(rule)).ToList();
				var ways   = Ways(rule);

				file.Line($"/// <summary>What <c>{rule.Name}</c> read, and what it is worth (Machine.Mixed.cs).</summary>");

				using (file.Block(
					Reference(rule)
						? $"private sealed class {Named(rule)}"
						: $"private readonly struct {Named(rule)}"))
				{
					foreach (var (type, name) in fields)
						file.Line($"private readonly {type} {name};");

					if (ways > 1)
						file.Line("private readonly byte which;");

					// A shape read is a shape made, and one never made is a member that was
					// not there. A class says that by being null; a struct has no null to
					// say it with, so it says it here.
					if (!Reference(rule))
						file.Line("private readonly bool read;");

					file.Line();

					var taken = fields.ConvertAll(one => one.Type + " " + one.Name.Substring(1));

					if (ways > 1)
						taken.Insert(0, "byte which");

					file.Line($"private {Named(rule)}({string.Join(", ", taken)})");

					using (file.Block(""))
					{
						if (ways > 1)
							file.Line("this.which = which;");

						foreach (var (_, name) in fields)
							file.Line($"this.{name} = {name.Substring(1)};");

						if (!Reference(rule))
							file.Line("this.read = true;");
					}

					for (var which = 0; which < ways; which++)
					{
						var mine   = Fields(machine.DirectMembers(rule, Factory(rule, which))).ToList();
						var passed = new List<string>();

						if (ways > 1)
							passed.Add(which.ToString(System.Globalization.CultureInfo.InvariantCulture));

						// What a construction does not have, as what stands for absent: a
						// run of text that was never read begins nowhere.
						foreach (var (_, name) in fields)
							passed.Add(
								mine.Exists(one => one.Name == name) ? name.Substring(1) :
								name[1] == 'm'                       ? "default" :
								                                       "-1");

						file.Line();
						file.Line(
							$"internal static {Named(rule)} {Maker(which)}(" +
							$"{string.Join(", ", mine.ConvertAll(one => one.Type + " " + one.Name.Substring(1)))})");

						using (file.Block(""))
							file.Line($"return new {Named(rule)}({string.Join(", ", passed)});");
					}

					file.Line();

					if (!Reference(rule))
					{
						file.Line("/// <summary>Whether the reading that would have made this one ever happened.</summary>");
						file.Line("internal bool IsNothing { get { return !this.read; } }");
						file.Line();
					}
					file.Line($"internal {machine._results.ValueOf(rule)} Build(global::System.ReadOnlySpan<char> text)");

					using (file.Block(""))
					{
						if (ways == 1)
						{
							file.Line($"return {Built(rule, Factory(rule, 0))};");
						}
						else
						{
							using (file.Block("switch (this.which)"))
							{
								for (var which = 0; which < ways - 1; which++)
								{
									file.Line($"case {which}:");

									using (file.Indent())
										file.Line($"return {Built(rule, which)};");
								}

								file.Line("default:");

								using (file.Indent())
									file.Line($"return {Built(rule, ways - 1)};");
							}
						}
					}
				}

				file.Line();
			}

			return file.ToString();
		}

		/// <summary>
		/// A shape's fields, in the order its constructor takes them: two integers where a
		/// member is a run of text, and the captured rule's own shape where it is a record.
		/// </summary>
		IEnumerable<(string Type, string Name)> Fields(IReadOnlyList<DirectMember> members)
		{
			foreach (var member in members)
				if (member.Shape == MemberShape.Text)
				{
					yield return ("int", $"_a{member.Index}");
					yield return ("int", $"_b{member.Index}");
				}
				else
				{
					yield return (Held(member.Member.Rule!), $"_m{member.Index}");
				}
		}

		/// <summary>One construction, over the fields the shape kept for it.</summary>
		string Built(RuleSymbol rule, int factory)
		{
			var members = machine.DirectMembers(rule, factory);

			if (factory < 0)
				return $"new {machine._results.QualifiedOf(rule)!}({string.Join(", ", members.Select(Value))})";

			var made      = machine._factories[rule][factory];
			var arguments = machine.DirectArguments(
				rule, made, members,
				() => "text.ToString()", () => "default",
				() => "default!", Value);

			return $"{made.Method}({string.Join(", ", arguments)})";

			string Value(DirectMember member) =>
				member.Shape == MemberShape.Text
					? $"(this._a{member.Index} < 0 ? {(member.Member.IsOptional ? "null" : "string.Empty")} : " +
						$"text.Slice(this._a{member.Index}, this._b{member.Index} - this._a{member.Index}).ToString())"
					: member.Member.IsOptional
						? $"({Nothing(member.Member.Rule!, "this._m" + member.Index)} ? default : this._m{member.Index}.Build(text))"
						: $"this._m{member.Index}.Build(text)";
		}

		// What the shapes written so far do not need, and Refuses keeps the machine from
		// asking for.

		public override string DeclareAccumulator(RuleSymbol rule) => throw Unwritten();

		public override IEnumerable<string> DeclareGathered(int slot, string elementType) => throw Unwritten();

		public override string PutAccumulator() => throw Unwritten();

		public override string Collect(DirectMember member, string from, bool pairs) => throw Unwritten();

		public override string PushText(int slot, string from, string to) => throw Unwritten();

		public override string PushRecord(int slot, RuleSymbol rule) => throw Unwritten();

		public override string Mark(int kind, int site) => throw Unwritten();

		public override string Materialize(string record, string sinceMark) => throw Unwritten();

		public override string ValueOf(RuleSymbol rule, string record) => throw Unwritten();

		public override void Gathered(
			Writer code, string from, IReadOnlyList<int> slots, string handed, string type, string build, bool text) =>
			throw Unwritten();

		static InvalidOperationException Unwritten() =>
			new($"The mixed carrier was asked to carry something it refuses ({nameof(Machine)}.Mixed.cs).");
	}
}
